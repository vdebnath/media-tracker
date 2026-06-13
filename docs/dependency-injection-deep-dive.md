# Dependency Injection Deep Dive for Media Tracker
### The one pattern that shows up on *both* ends of your stack

> **Same ground rule.** Examples use a **Contacts** address-book app, never your Media Tracker — read, understand, translate to your own code. C# examples are backend (`MediaTracker.Api/Services/Data`); TypeScript examples are frontend (Angular). macOS commands marked 🖥️.

> **Why this gets its own guide.** DI confuses almost everyone the first time, then becomes invisible once it clicks. It's also the single concept that appears *identically in spirit* in your C# backend and your Angular frontend — so learning it well pays off twice. And it's the thing that makes your Sprint 4 unit tests possible. If you only deeply understand one design pattern this whole project, make it this one.

---

## Table of Contents

1. [The Problem: Code That Builds Its Own Dependencies](#1-the-problem)
2. [The Idea: Inversion of Control](#2-the-idea-inversion-of-control)
3. [Constructor Injection (The Main Event)](#3-constructor-injection)
4. [Why Interfaces Make DI Powerful](#4-why-interfaces-make-di-powerful)
5. [The DI Container](#5-the-di-container)
6. [Registering Services in .NET](#6-registering-services-in-net)
7. [Service Lifetimes: Scoped, Transient, Singleton](#7-service-lifetimes)
8. [How the Container Resolves a Dependency Graph](#8-resolving-the-graph)
9. [The Testing Payoff (Why You Actually Care)](#9-the-testing-payoff)
10. [DI in Angular (The Frontend Mirror)](#10-di-in-angular)
11. [Common Mistakes & Gotchas](#11-common-mistakes--gotchas)
12. [A Full Worked Example](#12-a-full-worked-example)
13. [Quick Reference](#13-quick-reference)
14. [Resources](#14-resources)

---

## 1. The Problem

Start with code that does *not* use DI, so you feel the pain it removes. Here's a service that creates its own repository:

```csharp
public class ContactService
{
    private readonly ContactRepository _repository;

    public ContactService()
    {
        _repository = new ContactRepository();   // ❌ the service builds its own dependency
    }

    public Task<List<Contact>> GetAllAsync() => _repository.GetAllAsync();
}
```

This *works*. So what's wrong with it? Four things, and they get worse as the app grows:

**1. It's welded to one exact class.** `ContactService` can only ever use `ContactRepository`. Want a different implementation — a fake one for tests, a Postgres one later, a logging wrapper? You can't, without editing this file.

**2. It's untestable.** To test `ContactService` you'd need a *real* `ContactRepository`, which needs a *real* database. Your "unit" test just became an integration test that's slow and fragile. (This is the big one for your Sprint 4.)

**3. It hides its needs.** Looking at `new ContactService()`, you'd never know it secretly depends on a database. Dependencies should be *visible*, not buried inside constructors.

**4. It doesn't scale.** Now imagine `ContactRepository` itself needs a `DbContext`, which needs a connection string, which needs configuration. With manual `new`, *you* have to construct that entire chain by hand, in the right order, everywhere you use it. Multiply across dozens of classes and it's a nightmare.

> **The smell to remember:** whenever a class uses `new` to create something it *depends on* (a repository, a service, a database context, an HTTP client), that's a candidate for injection instead. `new`-ing up plain data objects (`new Contact()`) is totally fine — it's `new`-ing up *dependencies* that's the issue.

---

## 2. The Idea: Inversion of Control

**Dependency Injection** flips the relationship. Instead of a class *creating* its dependencies, it *receives* them from the outside:

```csharp
public class ContactService
{
    private readonly IContactRepository _repository;

    public ContactService(IContactRepository repository)   // ✅ handed in from outside
    {
        _repository = repository;
    }

    public Task<List<Contact>> GetAllAsync() => _repository.GetAllAsync();
}
```

The service no longer knows or cares *how* its repository was made. It just declares "I need something that can act as an `IContactRepository`," and trusts that someone will provide one. This is called **Inversion of Control (IoC)** — control over creating dependencies has been *inverted*, moved out of the class and up to whoever is constructing it.

DI is the specific technique; IoC is the principle behind it. You'll hear both terms; they point at the same idea.

> **The restaurant analogy, extended.** Without DI, the chef has to go grow the vegetables, raise the cattle, and mill the flour before cooking — every single time. With DI, ingredients are *delivered* to the kitchen; the chef just cooks. The chef declares what they need ("I need flour"); a supplier provides it. The chef doesn't care which farm it came from, which is exactly what lets you swap suppliers freely.

---

## 3. Constructor Injection

There are a few ways to inject dependencies, but **constructor injection** is the standard — it's what you'll use ~99% of the time in both .NET and Angular.

The dependency is declared as a constructor parameter and stored in a `readonly` field:

```csharp
public class ContactsController : ControllerBase
{
    private readonly ContactService _service;

    public ContactsController(ContactService service)   // injected here
    {
        _service = service;                              // stored here
    }
}
```

Why the constructor specifically?

- **It makes dependencies explicit.** Anyone reading the constructor sees exactly what this class needs to function. No hidden surprises.
- **It guarantees the dependency exists.** The object can't even be created without its dependencies, so you never hit a "this was null" surprise at runtime.
- **`readonly` locks it.** Assigned once in the constructor, never reassigned — one less thing that can go wrong.

> **Other injection types exist** (property injection, method injection) but they're niche. If you see them later, fine — but reach for constructor injection by default. It's the clearest and the most testable.

---

## 4. Why Interfaces Make DI Powerful

Notice the service depends on `IContactRepository` (an interface), not `ContactRepository` (a concrete class). This pairing is what gives DI its superpower. Without the interface, DI is just "passing arguments." *With* it, DI becomes "swap any implementation you like."

```csharp
// The contract
public interface IContactRepository
{
    Task<List<Contact>> GetAllAsync();
}

// Real implementation — talks to the database
public class ContactRepository : IContactRepository
{
    public Task<List<Contact>> GetAllAsync() { /* EF Core query */ }
}

// Fake implementation — returns canned data, no database
public class FakeContactRepository : IContactRepository
{
    public Task<List<Contact>> GetAllAsync()
        => Task.FromResult(new List<Contact> { new Contact { Id = 1 } });
}
```

Because `ContactService` depends on the *interface*, you can hand it **either** implementation and it has no idea which one it got:

```csharp
var realService = new ContactService(new ContactRepository());      // production
var testService = new ContactService(new FakeContactRepository());  // testing
```

Same service, two completely different behaviours, zero changes to the service code. *This* is the moment DI earns its keep. The interface is the seam; DI is what lets you cut along it.

> **The principle:** "Depend on abstractions, not concretions." Your high-level code (services) should depend on *what* a thing does (the interface), never on *how* it does it (the concrete class). DI + interfaces are how you put that principle into practice.

---

## 5. The DI Container

So if classes don't build their own dependencies, who *does*? In a real app you don't want to manually `new` up the whole chain either. That's the job of the **DI container** (also called the IoC container or service container).

The container is a registry. You tell it, once, "when something needs an `IContactRepository`, give them a `ContactRepository`." From then on, whenever the framework needs to build a class, it consults the container, sees what that class's constructor requires, builds those dependencies (and *their* dependencies, recursively), and assembles the finished object for you.

.NET has a container built in. Angular has one built in. You rarely interact with the container directly — you just *register* things and the framework does the resolving when it creates your controllers/components.

```
You: "Register IContactRepository → ContactRepository"
You: "Register ContactService"
You: "Register ContactsController" (the framework does this for controllers automatically)

Framework, on each request:
  "Need a ContactsController."
  "Its constructor wants a ContactService."
  "ContactService's constructor wants an IContactRepository."
  "I have that registered → build a ContactRepository."
  "Now build ContactService with it."
  "Now build ContactsController with that."
  → hands you a fully assembled controller
```

You declared the *what*; the container handled the *how* and the *order*. That recursive assembly is the thing that would be miserable to do by hand.

---

## 6. Registering Services in .NET

Registration happens in `Program.cs`, on the `builder.Services` collection, *before* the app is built:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register your dependencies
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ContactService>();

// EF Core registers its DbContext through DI too
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=contacts.db"));

builder.Services.AddControllers();   // registers your controllers

var app = builder.Build();
// ...
```

Reading the registrations:

- `AddScoped<IContactRepository, ContactRepository>()` — "when someone needs the interface, give them the concrete class." Two type arguments: the contract, then the implementation.
- `AddScoped<ContactService>()` — a concrete class with no interface; just register it by itself so the container knows how to build it.
- `AddDbContext<AppDbContext>(...)` — EF Core's helper that registers your database context with the container.

The `AddScoped` part is the *lifetime* — covered next, because it matters more than it looks.

---

## 7. Service Lifetimes

When the container creates a dependency, how long does that instance live and get reused? That's the **lifetime**, and .NET gives you three choices:

| Method | Lifetime | One instance per… |
|--------|----------|-------------------|
| `AddScoped<T>()` | **Scoped** | …HTTP request |
| `AddTransient<T>()` | **Transient** | …time it's requested (always new) |
| `AddSingleton<T>()` | **Singleton** | …entire application lifetime |

### Scoped (your default for web apps)

One instance is created per HTTP request and shared across everything in that request, then thrown away. If your controller, service, and repository all need the `DbContext` during one request, they all share the *same* one — which is exactly what you want for database work (they're all part of one logical unit of work).

```csharp
builder.Services.AddScoped<IContactRepository, ContactRepository>();
```

**Use Scoped for:** repositories, services, anything touching the database. This is your go-to.

### Transient

A brand-new instance every single time it's requested, even within the same request. Good for lightweight, stateless helpers where sharing doesn't matter.

```csharp
builder.Services.AddTransient<ISomeStatelessHelper, SomeStatelessHelper>();
```

### Singleton

One instance for the entire lifetime of the application, shared by every request and every user. Great for things that are expensive to create and safe to share (configuration, caches, loggers). **Dangerous** for anything holding per-request state.

```csharp
builder.Services.AddSingleton<IAppConfig, AppConfig>();
```

### The lifetime trap (a real bug you can avoid)

Never inject a *shorter*-lived dependency into a *longer*-lived one. Injecting a Scoped `DbContext` into a Singleton service is a classic disaster: the Singleton captures one request's `DbContext` and keeps using it forever, across all users, long after that request ended. This is called a **captive dependency**, and .NET will often throw an error to stop you — but understanding *why* matters. Rule of thumb: a service's dependencies should live *at least as long* as the service itself.

> **For Media Tracker:** you'll use `AddScoped` for your repository and service, and EF Core's `AddDbContext` (Scoped by default). You won't need Singleton or Transient for the core CRUD. But knowing the three — and the captive-dependency trap — is exactly the kind of thing that comes up in interviews and code review.

---

## 8. Resolving the Graph

Let's trace exactly what happens when an HTTP request hits your API, so the "magic" becomes mechanical:

```
GET /api/contacts arrives
        │
        ▼
1. Framework needs to create ContactsController to handle it.
2. ContactsController's constructor requires: ContactService
3. Container: "Do I have ContactService? Yes. What does IT need?"
4. ContactService's constructor requires: IContactRepository
5. Container: "Do I have IContactRepository? Yes → mapped to ContactRepository. What does IT need?"
6. ContactRepository's constructor requires: AppDbContext
7. Container: "Do I have AppDbContext? Yes (via AddDbContext). Build it."
        │
        ▼  (now assemble bottom-up)
8. Build AppDbContext
9. Build ContactRepository(appDbContext)
10. Build ContactService(contactRepository)
11. Build ContactsController(contactService)
        │
        ▼
12. Hand the fully-built controller to the framework → it calls your action method
```

You wrote three lines of registration. The container walked the entire dependency tree, built every piece in the correct order, and handed you a ready-to-use controller. Every arrow in your N-Tier architecture got wired automatically. *That's* the labour DI saves — and it's why adding a new dependency later is just "add a constructor parameter + one registration line" instead of hunting down every place you manually built the chain.

---

## 9. The Testing Payoff

Here's the reason DI matters *for you specifically* in Sprint 4. Because your service receives an `IContactRepository` instead of building a `ContactRepository`, you can hand it a fake during tests — no database required.

```csharp
[Fact]
public async Task GetAllContacts_ReturnsContactsFromRepository()
{
    // ARRANGE — build a fake repository with Moq
    var mockRepo = new Mock<IContactRepository>();
    mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Contact> { new Contact { Id = 1 } });

    // Inject the fake — this is DI, done manually in the test
    var service = new ContactService(mockRepo.Object);

    // ACT
    var result = await service.GetAllContactsAsync();

    // ASSERT
    Assert.Single(result);
}
```

Walk through why this is only possible because of DI:

- `new ContactService(mockRepo.Object)` — you're *injecting* a dependency by hand. The constructor accepts it because the service was built to receive an `IContactRepository`, not create one.
- `Mock<IContactRepository>` — Moq can only fake an *interface* (or virtual members). The interface from Chapter 4 is what makes the fake possible.
- No database, no network, no web server — the test runs in milliseconds and never flakes.

Three design decisions — **interfaces** (Ch 4), **DI** (this guide), and **layered architecture** (the field guide) — all converge right here to make testing trivial. When people say "good architecture," *this convergence* is what they mean. You didn't write testable code by adding tests; you wrote it by injecting dependencies through interfaces from the start.

---

## 10. DI in Angular

Now the beautiful part: Angular does the *exact same thing* on the frontend. Same pattern, same reasoning, slightly different syntax. Once you see this, the two halves of your stack stop feeling like separate worlds.

### Making something injectable

A service is marked with `@Injectable` and registered with Angular's container via `providedIn: 'root'`:

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })   // register as an app-wide singleton
export class ContactService {
  constructor(private http: HttpClient) {}   // HttpClient is injected here

  getAll() {
    return this.http.get<Contact[]>('http://localhost:5000/api/contacts');
  }
}
```

`providedIn: 'root'` is Angular's equivalent of a .NET `AddSingleton` — one shared instance for the whole app. (Angular's lifetimes work differently from .NET's, but `'root'` = "one shared instance" is the mental model you need here.)

### Injecting it into a component

```typescript
@Component({ /* ... */ })
export class ContactListComponent {
  constructor(private contactService: ContactService) {}   // injected automatically
  //          └─ same constructor-injection pattern as your C# controller
}
```

Compare side by side — they're the same idea twice:

| Concept | .NET (backend) | Angular (frontend) |
|---------|----------------|--------------------|
| Mark as injectable | register in `Program.cs` | `@Injectable({ providedIn: 'root' })` |
| Inject a dependency | constructor parameter | constructor parameter |
| The container | built-in .NET service provider | built-in Angular injector |
| App-wide single instance | `AddSingleton` | `providedIn: 'root'` |
| Per-scope instance | `AddScoped` | provided at component level |

When you wrote `constructor(private http: HttpClient)` in Angular and `public ContactsController(ContactService service)` in C#, you were doing the *identical thing* in two languages. That's not a coincidence — DI is a universal pattern, and you now recognise it on sight.

---

## 11. Common Mistakes & Gotchas

**`new`-ing up a dependency anyway.** The whole point is gone the moment you write `new ContactRepository()` inside your service. If you're typing `new` followed by the name of a *dependency*, stop and inject it instead.

**Forgetting to register.** In .NET, if you inject `IContactRepository` but never add it in `Program.cs`, you get a runtime error: *"Unable to resolve service for type IContactRepository."* The fix is always "add the missing `AddScoped`/`AddTransient`/`AddSingleton` line." This error is so common it's basically a rite of passage — now you know what it means on sight.

**Injecting the concrete class instead of the interface.** It compiles and runs, but you've thrown away the swappability and testability. Depend on `IContactRepository`, register the mapping to `ContactRepository`.

**The captive dependency.** Injecting Scoped into Singleton (Chapter 7). A service's dependencies must live at least as long as the service.

**Constructor doing real work.** The constructor should *only* store injected dependencies. Don't load data or call the database in it — that belongs in a method (or, in Angular, in `ngOnInit`). A constructor that does heavy work is hard to test and surprising to read.

---

## 12. A Full Worked Example

The complete chain, end to end, with DI at every seam. Study how no class ever builds its own dependency:

```csharp
// ---------- DATA LAYER ----------
public interface IContactRepository
{
    Task<List<Contact>> GetAllAsync();
}

public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;
    public ContactRepository(AppDbContext context)   // DbContext injected
    {
        _context = context;
    }
    public async Task<List<Contact>> GetAllAsync()
        => await _context.Contacts.ToListAsync();
}

// ---------- SERVICE LAYER ----------
public class ContactService
{
    private readonly IContactRepository _repository;
    public ContactService(IContactRepository repository)   // repository injected
    {
        _repository = repository;
    }
    public Task<List<Contact>> GetAllContactsAsync()
        => _repository.GetAllAsync();
}

// ---------- API LAYER ----------
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly ContactService _service;
    public ContactsController(ContactService service)   // service injected
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllContactsAsync());
}

// ---------- REGISTRATION (Program.cs) ----------
// builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=contacts.db"));
// builder.Services.AddScoped<IContactRepository, ContactRepository>();
// builder.Services.AddScoped<ContactService>();
// builder.Services.AddControllers();
```

Trace it: the controller gets a service, the service gets a repository, the repository gets a DbContext — each one *received*, never *created*. Three registration lines wire the whole thing. And because every seam is an injected dependency (and the repository hides behind an interface), every layer is independently testable.

Now go build *your* version with `MediaItem`. You have the pattern; the implementation is yours.

---

## 13. Quick Reference

```csharp
// .NET registration (Program.cs)
builder.Services.AddScoped<IThing, Thing>();      // one per HTTP request (default for web/db)
builder.Services.AddTransient<IThing, Thing>();   // new every time
builder.Services.AddSingleton<IThing, Thing>();   // one for the whole app

// .NET injection (anywhere)
public MyClass(IThing thing) { _thing = thing; }
```

```typescript
// Angular registration
@Injectable({ providedIn: 'root' })   // app-wide single instance
export class MyService {}

// Angular injection
constructor(private myService: MyService) {}
```

**The mental checklist when you add a dependency:**
1. Does it have an interface? (Should it, for swappability/testing?)
2. Inject it via the constructor — never `new` it.
3. Register it (`.NET`: a line in `Program.cs`. `Angular`: `@Injectable`).
4. Pick the right lifetime (`.NET`: usually `Scoped`).

---

## 14. Resources

**.NET**
- [Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/dependency-injection)
- [DI guidelines & service lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)

**Angular**
- [Dependency injection overview](https://angular.dev/guide/di)
- [Understanding DI](https://angular.dev/guide/di/dependency-injection)
- [Injectable services](https://angular.dev/guide/di/creating-injectable-service)

---

*Keep this with the other guides in `/docs`. DI is the thread that ties your whole stack together — when you can explain why `new ContactRepository()` inside a service is a mistake, and why the interface is what makes the fix work, you've understood the most important pattern in the project. Peas and carrots. 🫛🥕*
