# C# Deep Dive for Media Tracker
### The backend language, from fundamentals to the bits you'll actually use

> **Same ground rule as the field guide.** Every example uses a **Contacts** address-book app, never your Media Tracker. Read, understand, then go write your own `MediaItem` version. macOS terminal commands are marked 🖥️ and assume you're in your project folder unless stated.

> **Where this fits.** This guide is the `MediaTracker.Api`, `MediaTracker.Services`, and `MediaTracker.Data` side of the house — everything that runs on .NET. You're currently around inheritance in Head First C#; this picks up the fundamentals and carries them through to the project-grade features (generics, async, LINQ) you'll need by Sprint 2.

---

## Table of Contents

1. [The .NET CLI on macOS — Your Daily Commands](#1-the-net-cli-on-macos)
2. [Types, Variables & Nullability](#2-types-variables--nullability)
3. [Classes, Properties & Constructors](#3-classes-properties--constructors)
4. [Access Modifiers & Encapsulation](#4-access-modifiers--encapsulation)
5. [Inheritance](#5-inheritance)
6. [Interfaces (The Heart of Your Architecture)](#6-interfaces)
7. [Generics](#7-generics)
8. [Collections](#8-collections)
9. [LINQ](#9-linq)
10. [async / await](#10-async--await)
11. [Namespaces, using & Project Files](#11-namespaces-using--project-files)
12. [NuGet Packages on macOS](#12-nuget-packages-on-macos)
13. [Putting It Together: The Flow of a Request](#13-the-flow-of-a-request)
14. [Resources](#14-resources)

---

## 1. The .NET CLI on macOS

You'll live in the terminal for backend work. Here's the toolkit, organised by when you use it.

### Checking your install

```bash
🖥️ dotnet --version          # should print 10.x
🖥️ dotnet --info             # full SDK + runtime detail
```

### Creating projects (you did most of this in SCRUM-8)

```bash
🖥️ dotnet new sln -n MediaTracker          # a solution file (the container)
🖥️ dotnet new webapi -n MediaTracker.Api    # a Web API project
🖥️ dotnet new classlib -n MediaTracker.Data # a class library (no entry point)
🖥️ dotnet new console -n Scratch            # a console app (great for experiments)
```

A **solution** (`.sln`) is just a grouping of projects so they build together. A **project** (`.csproj`) is one compilable unit. A **class library** produces a reusable `.dll` with no `Main` method — that's why your Data and Services layers are class libraries; they're meant to be *used by* the API, not run on their own.

### Wiring projects together

```bash
🖥️ dotnet sln add MediaTracker.Api/MediaTracker.Api.csproj    # add project to solution
🖥️ dotnet add MediaTracker.Api/MediaTracker.Api.csproj \
      reference MediaTracker.Services/MediaTracker.Services.csproj   # one project uses another
```

### The everyday loop

```bash
🖥️ dotnet restore     # download dependencies (usually automatic)
🖥️ dotnet build       # compile; reports errors/warnings
🖥️ dotnet run         # build + run (use in a runnable project like the Api)
🖥️ dotnet run --project MediaTracker.Api   # run a specific project from the solution folder
```

> **Tip for your setup:** since only `MediaTracker.Api` is runnable, from the `/backend` folder you'll usually do `dotnet run --project MediaTracker.Api`. When it's running it prints a URL like `http://localhost:5xxx` — that's your API. Hit `Ctrl+C` to stop it.

### Testing (Sprint 4)

```bash
🖥️ dotnet test        # finds and runs all test projects in the solution
```

### EF Core migrations (Sprint 2) — needs a one-time tool install

```bash
🖥️ dotnet tool install --global dotnet-ef     # one time, installs the 'ef' command
🖥️ dotnet tool update --global dotnet-ef       # to update it later
🖥️ dotnet ef migrations add InitialCreate      # generate a migration from your models
🖥️ dotnet ef database update                   # apply migrations to the .db file
🖥️ dotnet ef migrations remove                  # undo the last (unapplied) migration
```

> **macOS gotcha:** after `dotnet tool install --global`, the tools live in `~/.dotnet/tools`. If `dotnet ef` returns "command not found," that folder isn't on your `PATH`. Add this line to your `~/.zshrc` and restart the terminal:
> ```bash
> export PATH="$PATH:$HOME/.dotnet/tools"
> ```

---

## 2. Types, Variables & Nullability

### C# is statically typed

Every variable has a type known at compile time. The compiler catches type errors before the program runs — this is one of C#'s biggest strengths over a dynamic language.

```csharp
int count = 5;
string title = "Dune";
bool isActive = true;
double rating = 4.5;
DateTime created = DateTime.Now;
```

### `var` — inferred, not dynamic

`var` lets the compiler figure out the type from the right-hand side. The variable is *still strongly typed* — `var` is not "anything."

```csharp
var name = "Ada";          // compiler knows this is string
var contacts = new List<Contact>();   // knows it's List<Contact>
// name = 5;               // compile error — it's locked to string
```

Use `var` when the type is obvious from the right side; spell out the type when it aids readability.

### Value types vs reference types

- **Value types** (`int`, `bool`, `double`, `DateTime`, `struct`) hold their data directly. Assigning copies the value.
- **Reference types** (`class`, `string`, arrays, `List<T>`) hold a *reference* to data on the heap. Assigning copies the reference — both variables point at the same object.

```csharp
var a = new Contact { FullName = "Ada" };
var b = a;                 // b points at the SAME object as a
b.FullName = "Grace";
// a.FullName is now "Grace" too — they're the same object
```

This trips up beginners constantly. If two variables "magically" change together, you've copied a reference, not the data.

### Nullable reference types (modern C#)

Modern .NET treats reference types as **non-nullable by default**. A `string` is assumed to always have a value; a `string?` is allowed to be null.

```csharp
string required = "always here";   // compiler warns if this could be null
string? optional = null;           // explicitly allowed to be null
```

This is why your model properties look like:

```csharp
public string FullName { get; set; } = string.Empty;  // non-null, initialised
public string? Phone { get; set; }                    // genuinely optional
```

The `= string.Empty` satisfies the compiler that a non-nullable string always has a value. The `?` on `Phone` says "this one really can be absent" — matching a database column without `NOT NULL`.

---

## 3. Classes, Properties & Constructors

### A class is a blueprint

```csharp
public class Contact
{
    // Properties — the data
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // Constructor — runs when you create an instance
    public Contact(string fullName)
    {
        FullName = fullName;
    }

    // Method — behaviour
    public string Describe() => $"{FullName} (#{Id})";
}
```

Create instances with `new`:

```csharp
var c = new Contact("Ada Lovelace");
Console.WriteLine(c.Describe());   // Ada Lovelace (#0)
```

### Properties: the C# way to expose data

A **property** looks like a field from outside but is really a pair of accessors — a `get` and a `set`. The shorthand `{ get; set; }` is an **auto-property**: the compiler generates the hidden backing field for you.

```csharp
public string FullName { get; set; }        // read + write
public int Id { get; private set; }          // read publicly, write only inside the class
public string Display => $"{FullName}";      // computed, read-only (expression-bodied)
public DateTime Created { get; init; }       // settable only during construction
```

`init` is worth knowing: it lets you set a property when creating the object but locks it afterward — handy for things that shouldn't change after creation.

### Object initializer syntax

You'll see this everywhere, including in your tests:

```csharp
var c = new Contact("Ada")
{
    Id = 1,
    Phone = "555-0100"
};
```

This sets properties immediately after construction. Clean and common.

---

## 4. Access Modifiers & Encapsulation

Access modifiers control *who can see what*. Encapsulation — hiding internals behind a clean surface — is why N-Tier works at the code level.

| Modifier | Who can access |
|----------|----------------|
| `public` | Anyone, any project |
| `private` | Only this class |
| `protected` | This class and subclasses |
| `internal` | Anything in the same project/assembly |

```csharp
public class ContactService
{
    private readonly IContactRepository _repository;  // hidden; only this class uses it
    public ContactService(IContactRepository repo) => _repository = repo;
    public Task<List<Contact>> GetAllAsync() => _repository.GetAllAsync();  // the public surface
}
```

The `_repository` field is `private` — nothing outside the service can reach it. The world only sees the public methods. That's encapsulation: the *how* is hidden, the *what* is exposed. `readonly` means it can only be assigned in the constructor and never reassigned afterward — a small guard that prevents a class of bugs.

> **Convention:** private fields are commonly prefixed with `_` (underscore). It's not enforced by the language but it's near-universal in .NET codebases, including yours at work.

---

## 5. Inheritance

This is where you are in Head First, so let's be thorough.

### The idea

Inheritance lets one class build on another. The **derived** (child) class gets everything the **base** (parent) class has, and can add to or change it.

```csharp
public class Media               // base class
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public virtual string Summary() => $"{Title}";   // virtual = overridable
}

public class Book : Media        // Book inherits from Media
{
    public string Author { get; set; } = string.Empty;
    public override string Summary() => $"{Title} by {Author}";  // override changes behaviour
}
```

- `: Media` means "Book is a Media." A `Book` automatically has `Id`, `Title`, and `Summary()`.
- `virtual` marks a method as overridable in the base class.
- `override` provides a new version in the child class.

### `base` — reaching the parent

```csharp
public class Book : Media
{
    public override string Summary() => $"{base.Summary()} (book)";  // call parent's version too
}
```

### `abstract` — a base that can't stand alone

An `abstract` class can't be instantiated directly; it's only a foundation for derived classes. An `abstract` method has no body — children *must* implement it.

```csharp
public abstract class Media
{
    public abstract string Summary();   // no body; every child must provide one
}
```

### When to actually use inheritance

Inheritance models "is-a" relationships (a Book *is a* Media). For your project, you genuinely don't need inheritance for `MediaItem` — a single class with a `Type` property is simpler and better here. **Knowing a tool and knowing when *not* to reach for it are both skills.** Don't force inheritance into the project just because you learned it. (The next chapter, interfaces, is the OOP tool you *will* lean on heavily.)

---

## 6. Interfaces

If you take one chapter to heart, make it this one — interfaces are the keystone of your whole architecture.

### An interface is a contract

It lists *what* a type can do, with no *how*. Any class that implements it promises to provide those members.

```csharp
public interface IContactRepository
{
    Task<List<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(int id);
    Task AddAsync(Contact contact);
    Task UpdateAsync(Contact contact);
    Task DeleteAsync(int id);
}
```

No bodies — just signatures. Now a class fulfils it:

```csharp
public class ContactRepository : IContactRepository
{
    // must implement EVERY member of the interface or it won't compile
    public Task<List<Contact>> GetAllAsync() { /* ...your EF Core code... */ }
    // ...the rest
}
```

> **Convention:** interface names start with `I` (`IContactRepository`). Again not enforced, but universal.

### Why this is the keystone

Your service depends on the *interface*, not the concrete class:

```csharp
public class ContactService
{
    private readonly IContactRepository _repository;   // the contract, not the class
    public ContactService(IContactRepository repo) => _repository = repo;
}
```

Because of this one decision:

1. **You can swap implementations** — a real EF Core repository in production, a fake one in tests — without changing the service.
2. **You can unit test** the service by passing a mock that satisfies `IContactRepository` (Sprint 4).
3. **Layers stay decoupled** — the service knows the *shape* of data access, not the *mechanism*.

This is "program to an interface, not an implementation," the single most repeated piece of OOP advice, and you're about to feel exactly why.

### Interface vs abstract class

Both define contracts, but: a class can implement **many** interfaces but inherit from only **one** base class. Interfaces describe *capabilities* ("can be saved," "can be compared"); abstract classes describe *a kind of thing* with shared code. For dependency boundaries (repositories, services), interfaces are the standard choice.

---

## 7. Generics

### The problem generics solve

Suppose you wanted a "list of contacts" and a "list of integers." Without generics you'd either write two classes or use a loosely-typed list and lose type safety. **Generics** let you write code parameterised by type.

```csharp
List<Contact> contacts = new();   // a list that holds ONLY Contacts
List<int> numbers = new();        // a list that holds ONLY ints
```

The `<T>` is a type placeholder filled in when you use it. `List<T>` is generic; `List<Contact>` is that generic filled with `Contact`. Try to add an `int` to a `List<Contact>` and it won't compile — type safety preserved.

### Generics you'll use constantly

- `List<T>` — a resizable list.
- `Task<T>` — an async operation that will eventually produce a `T` (Chapter 10).
- `DbSet<T>` — EF Core's representation of a table of `T`.
- `IEnumerable<T>` — "something you can loop over" of `T`.

```csharp
Task<List<Contact>> result = _repository.GetAllAsync();
//   └─ async op that yields ─┘ a list of contacts
```

Reading nested generics left-to-right ("a Task that produces a List of Contact") makes async data code far less intimidating.

---

## 8. Collections

The workhorses for holding groups of data.

```csharp
// List<T> — ordered, resizable, your default
var contacts = new List<Contact>();
contacts.Add(new Contact("Ada"));
contacts.Count;                 // how many
contacts[0];                    // by index
contacts.Remove(someContact);   // remove an item

// Dictionary<TKey, TValue> — key/value lookup
var byEmail = new Dictionary<string, Contact>();
byEmail["ada@example.com"] = new Contact("Ada");
byEmail.ContainsKey("ada@example.com");   // true

// Arrays — fixed size, less common in app code
string[] types = { "Book", "Movie", "Show" };
```

Iterate with `foreach`:

```csharp
foreach (var contact in contacts)
{
    Console.WriteLine(contact.FullName);
}
```

For 90% of your project, `List<T>` is the answer.

---

## 9. LINQ

LINQ (Language Integrated Query) lets you query collections — and, through EF Core, your database — with readable, chainable operations. This is how you'll filter, sort, and shape data without writing loops.

### The methods you'll reach for

```csharp
var contacts = new List<Contact>();

// Where — filter
var withPhone = contacts.Where(c => c.Phone != null);

// Select — transform / project
var names = contacts.Select(c => c.FullName);

// FirstOrDefault — first match or null
var ada = contacts.FirstOrDefault(c => c.FullName == "Ada Lovelace");

// Any — does anything match?
bool hasContacts = contacts.Any();

// OrderBy — sort
var sorted = contacts.OrderBy(c => c.FullName);

// Count — how many match
int total = contacts.Count(c => c.Phone != null);

// ToList — materialise the result into a List
var list = contacts.Where(c => c.Phone != null).ToList();
```

### The lambda `=>`

`c => c.FullName == "Ada"` is a **lambda** — a tiny inline function. Read it as "given a `c`, return whether its FullName equals Ada." The `c` is just a parameter name; call it whatever's clear.

### LINQ + EF Core = SQL, written in C#

The magic: when you write LINQ against a `DbSet`, EF Core **translates it into SQL** and runs it on the database.

```csharp
// This C#...
var books = await _context.Contacts
    .Where(c => c.Phone != null)
    .OrderBy(c => c.FullName)
    .ToListAsync();

// ...becomes roughly this SQL, executed by SQLite:
// SELECT * FROM Contacts WHERE Phone IS NOT NULL ORDER BY FullName;
```

You write C#; EF Core writes the SQL. Remembering this connection (back to the SQL chapter of the field guide) makes your repository code feel grounded rather than magical. Note `ToListAsync()` — the async version — when querying a database.

---

## 10. async / await

### Why async exists

Database queries and network calls are *slow* compared to CPU work. Blocking a thread while you wait wastes resources and, in a web app, limits how many requests you can serve. `async`/`await` lets the thread go do other useful work while waiting, then resume when the result is ready.

### The three-part rule

```csharp
public async Task<List<Contact>> GetAllContactsAsync()  // 1. async + Task<T> return
{
    var contacts = await _repository.GetAllAsync();     // 2. await the slow call
    return contacts;                                    // 3. return the actual value
}
```

1. Mark the method `async` and return `Task<T>` (or `Task` for "no value").
2. `await` any async call inside it — this is the "pause here until ready, without blocking" point.
3. You return the inner value (`List<Contact>`), and the framework wraps it back into the `Task`.

### Reading the types

- `Task` — an async operation with no return value (like a `void` that's async).
- `Task<List<Contact>>` — an async operation that will produce a `List<Contact>`.
- `await someTask` — unwrap the `Task<T>` into the `T`, pausing until it's done.

### The convention

Async methods get an `Async` suffix (`GetAllAsync`, `SaveChangesAsync`). It's not enforced but it signals "this returns a Task, you probably want to await it." EF Core gives you async versions of everything — prefer them in web code.

> **Golden rule:** async is "contagious" — if you `await` something, your method must be `async`. Don't try to "unwrap" a Task synchronously with `.Result` or `.Wait()`; in web apps that can deadlock. `await` all the way up.

---

## 11. Namespaces, using & Project Files

### Namespaces organise code

A namespace is a named container that prevents naming clashes and groups related types.

```csharp
namespace MediaTracker.Data.Models;   // file-scoped namespace (modern, one per file)

public class Contact { /* ... */ }
```

### `using` brings names into scope

```csharp
using MediaTracker.Data.Models;   // now you can write Contact instead of the full path
using Microsoft.EntityFrameworkCore;
```

Modern .NET has **implicit usings** — common namespaces (`System`, `System.Collections.Generic`, etc.) are included automatically, which is why you rarely write `using System;` anymore.

### The `.csproj` file

Each project has a `.csproj` (XML) describing how it builds and what it depends on. You'll mostly edit it to add packages and references, but it's worth opening once to demystify it:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>          <!-- nullable reference types on -->
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="..." />
  </ItemGroup>
</Project>
```

---

## 12. NuGet Packages on macOS

**NuGet** is .NET's package manager (the equivalent of npm for JavaScript). You'll add a few packages in Sprint 2 for EF Core.

```bash
# Add a package to a specific project (run from /backend)
🖥️ dotnet add MediaTracker.Data/MediaTracker.Data.csproj \
      package Microsoft.EntityFrameworkCore.Sqlite

🖥️ dotnet add MediaTracker.Data/MediaTracker.Data.csproj \
      package Microsoft.EntityFrameworkCore.Design

# Restore all packages for the solution (rarely needed manually)
🖥️ dotnet restore

# List packages in a project
🖥️ dotnet list MediaTracker.Data/MediaTracker.Data.csproj package
```

The two EF Core packages above are the typical pair for SQLite code-first: the `.Sqlite` provider (talks to SQLite) and `.Design` (powers the `dotnet ef` migration commands). You'll confirm exact package details when you reach that story — versions move, so check the EF Core docs then rather than trusting a number written here.

---

## 13. The Flow of a Request

Tie it all together. A user clicks "load contacts" in the browser. Here's the round trip through your C# code:

```
1. HTTP GET /api/contacts arrives at ContactsController.GetAll()
        │   (controller — knows HTTP, nothing else)
        ▼
2. Controller calls  await _service.GetAllContactsAsync()
        │   (service handed to controller via DI)
        ▼
3. Service applies any rules, calls  await _repository.GetAllAsync()
        │   (repository handed to service via DI)
        ▼
4. Repository runs LINQ on the DbContext:  _context.Contacts.ToListAsync()
        │   (EF Core translates LINQ → SQL)
        ▼
5. SQLite runs  SELECT * FROM Contacts;  returns rows
        ▲
6. EF Core maps rows → List<Contact> objects
        ▲
7. Service returns the list; controller wraps it:  return Ok(contacts)  → HTTP 200 + JSON
```

Every concept in this guide appears in that flow: interfaces (the repo and service contracts), DI (the handoffs), generics (`Task<List<Contact>>`), LINQ (the query), async (`await` at each hop), and the layer boundaries from N-Tier. When you can narrate this trip from memory, the backend has clicked.

---

## 14. Resources

- [C# Language Tour](https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/)
- [Inheritance & OOP](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/)
- [Interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces)
- [Generics](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [LINQ](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)
- [async/await](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [.NET CLI reference](https://learn.microsoft.com/en-us/dotnet/core/tools/)
- [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/dependency-injection)

---

*Keep this next to the field guide in `/docs`. When a concept here shows up in your real code, come back and reread that section — it'll read completely differently once you've felt the problem it solves.*
