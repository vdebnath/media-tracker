# The Media Tracker Field Guide
### A study companion for building a full-stack app with .NET, SQL, and Angular

> **How to use this guide.** This is not a tutorial you copy from. Every code example here uses a *different* example app — a simple **Contacts** address book — so that you always have to translate the idea into your own **Media Tracker** code. If you ever find yourself able to paste an example straight into your project, something has gone wrong. The goal is that you read a chapter, understand the *why*, then go write your own version from scratch.

> **The layers at a glance.** Your app is split into tiers that each have one job and only talk to their neighbour:
>
> `Angular (browser)` → HTTP → `API / Controllers` → `Services` → `Data / Repository` → `SQLite`
>
> Keep this picture in your head for the whole project. Almost every concept below is really just "how do I move data cleanly across one of those arrows."

---

## Table of Contents

1. [The Big Picture: What You're Building and Why](#1-the-big-picture)
2. [N-Tier Architecture](#2-n-tier-architecture)
3. [The Data Layer, Part 1 — SQL & SQLite](#3-the-data-layer-part-1--sql--sqlite)
4. [The Data Layer, Part 2 — EF Core, Models & Migrations](#4-the-data-layer-part-2--ef-core)
5. [The Data Layer, Part 3 — The Repository Pattern](#5-the-data-layer-part-3--the-repository-pattern)
6. [The Service Layer & Dependency Injection](#6-the-service-layer--dependency-injection)
7. [The API Layer — REST, Controllers, HTTP](#7-the-api-layer)
8. [The Frontend, Part 1 — TypeScript Foundations](#8-the-frontend-part-1--typescript)
9. [The Frontend, Part 2 — Angular Components & Services](#9-the-frontend-part-2--angular)
10. [Styling — HTML & SCSS](#10-styling--html--scss)
11. [Testing — xUnit & Moq](#11-testing--xunit--moq)
12. [Git & Workflow](#12-git--workflow)
13. [Glossary](#13-glossary)
14. [The Resource Library](#14-the-resource-library)

---

## 1. The Big Picture

You're building a **CRUD** app. CRUD stands for **Create, Read, Update, Delete** — the four things you can do to a piece of stored data. Almost every business application you'll ever touch is, underneath all the features, a CRUD app with opinions. Learn it well here and you've learned the spine of the whole industry.

Your specific app tracks **media items** (books, movies, shows). A single item has a title, a type, a status, optional notes, and a date it was added. The user can list them, filter them, add new ones, edit them, and delete them. That's it. The simplicity is the point — it lets you focus on *how the pieces connect* instead of drowning in features.

**Why full-stack matters.** Each layer speaks a different language and has a different job. The magic — and the part that's genuinely hard the first time — is the *handoffs* between them. A title typed into an Angular form has to survive a trip through TypeScript, across an HTTP request, into a C# controller, through a service, into the database, and all the way back. Most beginner confusion lives at those boundaries, not inside any single layer. This guide spends most of its energy on the boundaries.

---

## 2. N-Tier Architecture

### What it is

N-Tier (also called multi-tier or layered architecture) means splitting your application into separate layers, each with a single responsibility, where each layer is only allowed to talk to the one directly beneath it. Your backend has three:

| Layer | Project | Job | Talks to |
|-------|---------|-----|----------|
| **Presentation / API** | `MediaTracker.Api` | Receive HTTP requests, return HTTP responses | Services |
| **Business / Service** | `MediaTracker.Services` | Apply rules and logic | Data |
| **Data Access** | `MediaTracker.Data` | Read & write the database | SQLite |

### Why bother? (The part that actually matters)

Imagine all your code lived in one giant controller method: it parsed the HTTP request, ran the business rules, *and* wrote raw SQL. It would work. So why don't we?

- **Change isolation.** Suppose you swap SQLite for PostgreSQL later. With layers, only the Data project changes — the API and Services don't even notice. Without layers, you're editing database code that's tangled into your HTTP handling.
- **Testability.** You can test your business rules without a real database or a real web server (you'll do exactly this in Chapter 11). That's only possible because the logic lives in its own layer with clean inputs and outputs.
- **Readability.** When every file has one job, a new developer (or you, in three months) knows exactly where to look. "Bug in how we save? That's the Data layer. Bug in a rule? Service layer."

### The golden rule: dependencies point downward

```
Api  ──references──▶  Services  ──references──▶  Data
```

Notice your project references (the ones you set up in SCRUM-8) enforce this. `Api` references `Services`; `Services` references `Data`. `Data` references *nothing* upward. This is deliberate. The database layer must never know that a web API exists — that knowledge would couple them together and defeat the whole point.

**The controller must never touch the database directly.** This is the single most common rule beginners break. If your controller `new`s up a database context and runs a query, you've collapsed three layers into one. The controller's job is *only* to translate between HTTP and method calls. It asks the service for things. The service asks the data layer. Each arrow is one hop.

> **Mental model:** Think of a restaurant. The **waiter** (API) takes your order and brings your food but never cooks. The **chef** (Service) decides how the dish is made but doesn't grow the vegetables. The **pantry/farm** (Data) just supplies raw ingredients. A waiter who runs to the farm mid-shift is a broken restaurant.

📚 *Deep dive:* [Common web app architectures — Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

---

## 3. The Data Layer, Part 1 — SQL & SQLite

### What a database actually is

A relational database stores data in **tables** — grids of rows and columns, like a spreadsheet with strict rules. Each **column** has a name and a type. Each **row** is one record. SQLite is a tiny, file-based relational database — the entire database is a single `.db` file on disk. No server to install, perfect for learning and for small apps.

### SQL: the language you talk to it with

SQL (Structured Query Language) is how you tell the database what you want. There are two halves worth naming:

- **DDL (Data Definition Language)** — defines *structure*. `CREATE TABLE`, `ALTER TABLE`, `DROP TABLE`.
- **DML (Data Manipulation Language)** — works with *data*. `INSERT`, `SELECT`, `UPDATE`, `DELETE`. Notice these line up exactly with CRUD.

### Defining a table (DDL)

Here's a `Contacts` table for our example address book. Study the *shape*, then go design your own `MediaItems` table for SCRUM-12 — do **not** copy this, the columns are different:

```sql
CREATE TABLE Contacts (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName    TEXT    NOT NULL,
    Email       TEXT    NOT NULL,
    Phone       TEXT,
    DateCreated TEXT    NOT NULL
);
```

Key ideas:

- **`PRIMARY KEY`** — the column that uniquely identifies each row. No two rows can share it. It's how you point at one specific record.
- **`AUTOINCREMENT`** — the database assigns the next number automatically, so you never set `Id` yourself.
- **`NOT NULL`** — this column is required. Try to insert a row without it and the database refuses. `Phone` here is *nullable* (optional) because it lacks `NOT NULL`.
- **Types in SQLite** — SQLite is relaxed about types. It really only has `TEXT`, `INTEGER`, `REAL`, `BLOB`, and `NULL`. Notice dates are stored as `TEXT` (an ISO string like `2026-06-09`). SQLite has no dedicated date type — a classic gotcha.

### The four CRUD statements (DML)

```sql
-- CREATE: add a row
INSERT INTO Contacts (FullName, Email, Phone, DateCreated)
VALUES ('Ada Lovelace', 'ada@example.com', '555-0100', '2026-06-09');

-- READ: get rows
SELECT * FROM Contacts;                          -- all columns, all rows
SELECT FullName, Email FROM Contacts;            -- specific columns
SELECT * FROM Contacts WHERE Email = 'ada@example.com';  -- filtered

-- UPDATE: change existing rows (WHERE is critical!)
UPDATE Contacts SET Phone = '555-0199' WHERE Id = 1;

-- DELETE: remove rows (WHERE is even more critical!)
DELETE FROM Contacts WHERE Id = 1;
```

> ⚠️ **The most expensive lesson in SQL:** `UPDATE` and `DELETE` without a `WHERE` clause apply to *every row in the table*. `DELETE FROM Contacts;` empties the whole table instantly with no undo. Professionals have taken down production systems this way. Always write your `WHERE` first, then the rest of the statement.

### Filtering and the building blocks of READ

The `WHERE` clause is where reading gets powerful. A few you'll use:

```sql
SELECT * FROM Contacts WHERE FullName = 'Ada Lovelace';   -- exact match
SELECT * FROM Contacts WHERE FullName LIKE 'A%';          -- starts with A
SELECT * FROM Contacts WHERE DateCreated > '2026-01-01';  -- comparison
SELECT * FROM Contacts ORDER BY FullName ASC;             -- sorted A→Z
```

When you build your status/type filtering in the frontend later, it ultimately becomes a `WHERE` clause (EF Core writes it for you, but it's still SQL underneath). Understanding this now makes the Angular filtering feel obvious later.

📚 *Deep dives:* [SQLite Tutorial](https://www.sqlitetutorial.net/) · [SQLBolt interactive](https://sqlbolt.com/)

---

## 4. The Data Layer, Part 2 — EF Core

### The problem ORMs solve

Your C# code thinks in **objects** (a `Contact` with a `.FullName` property). Your database thinks in **rows and columns**. Translating between the two by hand — writing SQL strings, reading raw results, mapping them back into objects — is tedious and error-prone. This mismatch even has a name: the *object-relational impedance mismatch*.

An **ORM (Object-Relational Mapper)** does that translation for you. **Entity Framework Core (EF Core)** is .NET's ORM. You write C# and LINQ; EF Core generates the SQL, runs it, and hands you back objects.

### The model class (the C# mirror of a table)

A model (or "entity") is a plain C# class whose properties mirror your table's columns. For our example:

```csharp
public class Contact
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }          // the ? means nullable/optional
    public string DateCreated { get; set; } = string.Empty;
}
```

Notes for when you write your own `MediaItem`:
- The property named `Id` is recognised by EF Core *by convention* as the primary key. You don't have to tell it.
- `string?` (with the question mark) marks a property as nullable — the C# equivalent of a column without `NOT NULL`.
- `= string.Empty` initialises non-nullable strings so the compiler stops warning you about nulls.

### The DbContext (your session with the database)

The `DbContext` is the object that represents a connection/session to your database. It exposes your tables as properties you can query:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts { get; set; }   // <- this becomes your table
}
```

A `DbSet<Contact>` is "the Contacts table, as far as your C# code is concerned." Querying `context.Contacts` is querying the table.

### Migrations (version control for your schema)

You changed your model — added a column, say. The database doesn't know yet. **Migrations** are EF Core's way of recording schema changes as code and applying them to the database in order. Two commands you'll live by:

```bash
dotnet ef migrations add InitialCreate   # generate a migration from your models
dotnet ef database update                # apply pending migrations to the .db file
```

The first command looks at your models, compares them to the last known state, and writes a migration file describing the difference. The second runs those migrations against the actual database. Think of migrations as `git commit` but for your database structure.

> **Code-first vs database-first.** You're doing *code-first*: you write C# classes and EF Core builds the database from them. (You'll have already hand-written the SQL in SCRUM-12 as a learning exercise — that's so you understand what EF Core is doing for you, not because the app needs it.)

📚 *Deep dive:* [EF Core docs](https://learn.microsoft.com/en-us/ef/core/)

---

## 5. The Data Layer, Part 3 — The Repository Pattern

### Why not just use the DbContext everywhere?

You *could* call `context.Contacts` directly from your service. But then your service is welded to EF Core. The **repository pattern** puts a thin layer between your business logic and EF Core, so the rest of your app asks for data in plain terms ("give me all contacts") without knowing or caring that EF Core is behind the curtain.

A repository is usually defined as an **interface** (a contract) plus a **class** that implements it:

```csharp
// The contract — WHAT you can do, not HOW
public interface IContactRepository
{
    Task<List<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(int id);
    Task AddAsync(Contact contact);
    Task UpdateAsync(Contact contact);
    Task DeleteAsync(int id);
}
```

The interface lists capabilities with no implementation. The class then fulfils that contract using the `DbContext`. (Writing that class is *your* job for the project — this guide deliberately doesn't hand it to you. You now know the shape; go fill it in.)

### Why interfaces matter (this unlocks testing later)

Because your service depends on `IContactRepository` (the contract) and not on a concrete class, you can swap in a *fake* repository during tests — one that returns canned data without touching a database. That single design choice is what makes Chapter 11 possible. Hold that thought.

### A word on `async`/`await`

Database calls are *slow* relative to CPU work — they involve disk or network. `async`/`await` lets your app do other work while waiting instead of freezing a thread. The rules of thumb: methods that hit the database return `Task<T>`, you `await` them, and the method is marked `async`. You'll see `Async` suffixes everywhere in .NET data code by convention. Don't fight it; embrace it from the start.

---

## 6. The Service Layer & Dependency Injection

### What lives here

The service layer holds **business logic** — the rules and decisions that make your app *yours*. "When a new item is added, set its DateAdded to today." "A completed item can't be moved back to backlog." Those rules don't belong in the controller (which only knows HTTP) or the repository (which only knows storage). They live in the service.

For Media Tracker the logic is light — that's fine. The *structure* is what you're practising. A service typically wraps the repository and adds rules on top:

```csharp
public class ContactService
{
    private readonly IContactRepository _repository;

    // The repository is handed to us — we don't create it. (See DI below.)
    public ContactService(IContactRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Contact>> GetAllContactsAsync()
        => _repository.GetAllAsync();

    // ...your own methods, with your own rules, go here
}
```

### Dependency Injection (DI) — the concept that confuses everyone at first

Look at that constructor. `ContactService` *needs* an `IContactRepository`, but it doesn't create one with `new`. Instead, something hands it one. That "handing it one" is **Dependency Injection**.

**Why?** If `ContactService` did `new ContactRepository()` itself, it would be permanently tied to that exact class — you could never give it a fake one for testing, and swapping implementations would mean editing the service. By *receiving* its dependencies instead of *creating* them, the service stays flexible and testable.

**Who does the handing?** .NET has a built-in **DI container**. In `Program.cs` you *register* which concrete class should satisfy each contract:

```csharp
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<ContactService>();
```

This says: "whenever something needs an `IContactRepository`, give it a `ContactRepository`." Now when the framework builds your controller, it sees the controller needs a service, the service needs a repository, and it wires the whole chain automatically. You declare the *what*; the container handles the *how*.

> **Mental model:** DI is like ordering at a restaurant versus going into the kitchen to cook your own meal. You declare what you need ("a service that needs a repository"); the kitchen (the DI container) assembles it and brings it to you fully built. You never reach for the `new` keyword to construct your dependencies.

> **`AddScoped` vs `AddSingleton` vs `AddTransient`** — these are *lifetimes* (how long an instance lives). `Scoped` = one per HTTP request, which is the right default for web apps and database work. You'll mostly use `AddScoped`. File this away; it matters more as apps grow.

📚 *Deep dive:* [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/dependency-injection)

---

## 7. The API Layer

### REST in one paragraph

**REST** is a convention for designing web APIs around *resources* (nouns) and *HTTP verbs* (actions). A resource is a thing — `contacts`. A verb is what you do to it. Instead of inventing method names like `/getAllContacts` and `/deleteContactById`, REST says: use the standard HTTP verbs against a clean resource URL. The verbs map — surprise — directly onto CRUD.

| CRUD | HTTP verb | Example URL | Meaning |
|------|-----------|-------------|---------|
| Create | `POST` | `/api/contacts` | Add a new contact |
| Read (all) | `GET` | `/api/contacts` | Get every contact |
| Read (one) | `GET` | `/api/contacts/5` | Get contact #5 |
| Update | `PUT` | `/api/contacts/5` | Replace contact #5 |
| Delete | `DELETE` | `/api/contacts/5` | Remove contact #5 |

### HTTP status codes (the API's body language)

Your API should answer with the right code so the frontend knows what happened:

- **200 OK** — success, here's your data.
- **201 Created** — success, a new thing was made (the right response to a `POST`).
- **204 No Content** — success, nothing to return (common after a `DELETE`).
- **400 Bad Request** — you sent me something invalid.
- **404 Not Found** — that resource doesn't exist.
- **500 Internal Server Error** — *I* broke, not you.

Returning the correct codes is part of your acceptance criteria, and it's a real professional habit. A frontend that gets a `404` can show "not found"; one that gets a `200` with empty data can't tell the difference between "no results" and "broken."

### The controller's job — and only its job

```csharp
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly ContactService _service;

    public ContactsController(ContactService service)   // DI again!
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var contacts = await _service.GetAllContactsAsync();
        return Ok(contacts);     // 200 + the data
    }

    // POST, GET-by-id, PUT, DELETE are yours to write
}
```

The controller does three things and nothing more: (1) receive the HTTP request, (2) call the service, (3) translate the result into an HTTP response with the right status code. No business rules. No database. If you catch yourself writing an `if` about *business logic* in a controller, it belongs in the service.

### CORS — the thing that will block you on day one of frontend work

Your Angular app runs on `localhost:4200`. Your API runs on a *different* port. Browsers, for security, block a page on one origin from calling an API on another origin unless the API explicitly says it's allowed. That permission is **CORS (Cross-Origin Resource Sharing)**. You configure it once in `Program.cs` to allow your Angular origin, and the mysterious "blocked by CORS policy" console error disappears. Knowing the name in advance will save you a frustrating evening.

📚 *Deep dive:* [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)

---

## 8. The Frontend, Part 1 — TypeScript

### Why TypeScript exists

JavaScript will happily let you do `"5" - true` and won't complain until it explodes at runtime. **TypeScript** is JavaScript plus a *type system*: you annotate what shape your data has, and the compiler catches mismatches *before* the code runs. It compiles down to plain JavaScript that the browser runs. For someone coming from C#, TypeScript will feel comfortingly familiar — it has types, interfaces, and generics.

### The pieces you'll use most

```typescript
// Type annotations: declare what a variable holds
let count: number = 0;
let title: string = "Dune";
let isDone: boolean = false;

// Interface: describe the shape of an object (your C# model's twin)
interface Contact {
  id: number;
  fullName: string;
  email: string;
  phone?: string;        // ? = optional, just like C#'s string?
  dateCreated: string;
}

// Union types: a value that can be one of a fixed set
type Status = 'Backlog' | 'InProgress' | 'Completed';
```

### The boundary that trips people up: C# is PascalCase, TypeScript is camelCase

Your C# model has `FullName`. Your TypeScript interface has `fullName`. When data crosses the HTTP boundary as JSON, ASP.NET Core *by default* converts your C# `PascalCase` properties to `camelCase` JSON. So your TypeScript interface should use `camelCase` to match what actually arrives. This mismatch — "my data is undefined and I don't know why" — is almost always a casing problem. Now you know.

📚 *Deep dive:* [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)

---

## 9. The Frontend, Part 2 — Angular

### The component model

Angular builds UIs out of **components**. A component is a reusable, self-contained piece of UI made of three (usually co-located) parts:

- **The template** (`.html`) — what it looks like.
- **The class** (`.ts`) — its data and behaviour.
- **The styles** (`.scss`) — how it's dressed, *scoped to this component only*.

You'll build (at minimum) a list component to show your items and a form component to add/edit them. Each is its own little world.

### Data binding — the four flavours

This is Angular's core magic: keeping the template and the class in sync.

```html
<!-- Interpolation: show a class value in the template -->
<h1>{{ title }}</h1>

<!-- Property binding [ ]: push class → element property -->
<button [disabled]="isSaving">Save</button>

<!-- Event binding ( ): element event → class method -->
<button (click)="save()">Save</button>

<!-- Two-way binding [( )]: both directions, for form inputs -->
<input [(ngModel)]="searchText" />
```

Remember it as: `{{ }}` shows, `[ ]` goes *in* to the element, `( )` comes *out* of the element, `[( )]` does both. The "banana in a box" `[()]` is just property + event binding combined.

### Rendering lists and conditionals

```html
<!-- Loop over an array -->
@for (contact of contacts; track contact.id) {
  <div class="row">{{ contact.fullName }}</div>
}

<!-- Conditional rendering -->
@if (contacts.length === 0) {
  <p>No contacts yet.</p>
}
```

> *Note:* Angular's newer `@for`/`@if` "control flow" syntax (shown above) is the modern form in recent versions. You may also see the older `*ngFor`/`*ngIf` directive style in tutorials — they do the same job. Use the modern one; recognise the old one.

### Services & HttpClient — how the frontend talks to your API

Just like the backend, Angular separates concerns: components handle the *view*, **services** handle data and logic. An Angular service uses `HttpClient` to call your API:

```typescript
@Injectable({ providedIn: 'root' })
export class ContactService {
  private apiUrl = 'http://localhost:5000/api/contacts';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Contact[]> {
    return this.http.get<Contact[]>(this.apiUrl);
  }
  // add, update, delete are yours to write
}
```

Notice DI shows up here too — the component receives the service, the service receives `HttpClient`. Same pattern as the backend. Once you see that DI is *everywhere*, both stacks start to feel like one idea.

### Observables (a gentle first contact)

`HttpClient` doesn't return data directly — it returns an **Observable**, a stream of values that arrive *later* (because the network takes time). You **subscribe** to it to get the result:

```typescript
this.contactService.getAll().subscribe(data => {
  this.contacts = data;   // runs when the response arrives
});
```

For now, treat Observables as "a Promise that you subscribe to." That mental shortcut is imperfect but it'll carry you through this project. The depth comes later.

### Reactive forms

Your add/edit form will use Angular's **reactive forms**, where the form's structure is defined in the component class (not the template). This gives you validation, value tracking, and clean submission handling. It's one of the meatier Angular topics — budget real time for it when you reach SCRUM in Sprint 3.

📚 *Deep dives:* [Angular tutorial](https://angular.dev/tutorials/learn-angular) · [Components](https://angular.dev/guide/components) · [HttpClient](https://angular.dev/guide/http) · [Reactive forms](https://angular.dev/guide/forms/reactive-forms)

---

## 10. Styling — HTML & SCSS

### HTML is structure, CSS is appearance

HTML defines *what things are* (a heading, a list, a button). CSS defines *how they look*. Keep them mentally separate: if you're describing meaning, that's HTML; if you're describing colour/spacing/layout, that's CSS.

### What SCSS adds over plain CSS

SCSS is CSS with superpowers. The three you'll actually use in this project:

```scss
// 1. Variables — define once, reuse everywhere
$msu-green: #18453B;
$radius: 8px;

// 2. Nesting — mirror your HTML structure
.card {
  background: white;
  border-radius: $radius;

  .title {           // = .card .title
    color: $msu-green;
    font-weight: 600;
  }
}

// 3. Partials & @use — split styles into files and import them
@use 'variables';
```

> **A subtlety for your theme work (SCRUM-11):** SCSS variables (`$msu-green`) are resolved at *compile time* — they're baked in before the browser sees them. For light/dark mode switching you want **CSS custom properties** (`--color-primary: #18453B;`) instead, because those live in the browser and can change at runtime when you toggle a class on `<body>`. Use CSS custom properties for anything that changes with the theme, and SCSS variables for fixed values. This distinction is exactly why your acceptance criteria specify custom properties.

### Layout: Flexbox is your workhorse

Most of your layout — a sidebar next to a main area, a row with content pushed to each end — is **Flexbox**:

```scss
.layout {
  display: flex;          // children sit in a row
}
.row {
  display: flex;
  justify-content: space-between;  // push children to far ends
  align-items: center;             // vertically centre them
}
```

📚 *Deep dives:* [MDN HTML](https://developer.mozilla.org/en-US/docs/Learn/HTML) · [Flexbox](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_flexible_box_layout) · [SCSS guide](https://sass-lang.com/guide/)

---

## 11. Testing — xUnit & Moq

### Why unit tests exist

A **unit test** is code that checks a small piece of your code does what you expect. Run them and you get instant confirmation nothing broke. The payoff isn't writing them — it's six weeks later when you change something and the tests catch the thing you forgot you'd break.

You're testing the **service layer**, because that's where logic lives. Remember Chapter 5's promise about interfaces? This is where it pays off.

### The AAA pattern

Every good test has three parts:

```csharp
[Fact]   // xUnit's marker for "this is a test"
public async Task GetAllContacts_ReturnsAllContacts()
{
    // ARRANGE — set up the scenario
    var fakeRepo = new Mock<IContactRepository>();
    fakeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Contact> { new Contact { Id = 1 } });
    var service = new ContactService(fakeRepo.Object);

    // ACT — do the thing you're testing
    var result = await service.GetAllContactsAsync();

    // ASSERT — check the result is what you expect
    Assert.Single(result);
}
```

### What Moq is doing

You don't want your tests hitting a real database — that's slow and fragile. **Moq** creates a *fake* (mock) object that satisfies the `IContactRepository` interface and returns whatever you tell it to. `Setup(...).ReturnsAsync(...)` means "when someone calls `GetAllAsync`, hand back this canned list." Now you can test your service's logic in complete isolation — no database, no network, milliseconds to run.

This *only works* because your service depends on the interface `IContactRepository`, not a concrete class. Loop back to Chapters 5 and 6 and notice how three separate design decisions — interfaces, DI, layered architecture — all converge to make testing trivial here. That convergence is what "good architecture" actually feels like.

> Your job for the project: write tests for *add*, *get*, and *update* on your own service, using your own MediaItem. The example above shows the *shape* — the assertions and setup for your cases are yours.

📚 *Deep dives:* [xUnit with .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) · [Moq quickstart](https://github.com/moq/moq4/wiki/Quickstart) · [Testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

## 12. Git & Workflow

### The mental model

Git tracks the history of your code as a series of **commits** — snapshots you can return to. The basic loop:

```bash
git status            # what's changed?
git add .             # stage changes for the next commit
git commit -m "..."   # snapshot them with a message
git push              # send commits up to GitHub
```

### Conventional commits

Prefix your commit messages so the history reads like a changelog:

- `feat:` — a new feature
- `fix:` — a bug fix
- `chore:` — setup, config, dependencies, non-code housekeeping
- `docs:` — documentation only
- `test:` — adding or fixing tests
- `refactor:` — restructuring code without changing behaviour

Example: `feat: add delete endpoint to contacts controller`. It's a small habit that makes you look (and work) like a pro.

### A note on PR review (a Sprint 3+ skill)

When you review a pull request, you're not just hunting bugs — you're asking: *Is this readable? Does it belong in the right layer? Will the next person understand it?* A good review comment explains the *why*, suggests a direction, and stays kind. You'll practise writing these about your own code at check-ins. Reviewing your own work as if it were a stranger's is one of the fastest ways to level up.

📚 *Deep dives:* [GitHub Git basics](https://docs.github.com/en/get-started/using-git/about-git) · [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)

---

## 13. Glossary

| Term | Plain-English meaning |
|------|----------------------|
| **CRUD** | Create, Read, Update, Delete — the four basic data operations |
| **N-Tier** | Splitting an app into layers that each do one job |
| **ORM** | Tool that maps database rows to code objects (EF Core) |
| **Entity / Model** | A C# class mirroring a database table |
| **DbContext** | Your code's session/connection to the database |
| **Migration** | A recorded, replayable change to the database structure |
| **Repository** | A layer wrapping database access behind a clean interface |
| **Interface** | A contract listing capabilities without implementation |
| **Dependency Injection (DI)** | Receiving your dependencies instead of creating them |
| **DI Container** | The framework piece that assembles objects and their dependencies |
| **Service** | The layer holding business logic / rules |
| **Controller** | The layer translating HTTP ↔ method calls |
| **REST** | A convention for APIs built on resources + HTTP verbs |
| **HTTP verb** | GET / POST / PUT / DELETE — the action in a request |
| **Status code** | The number an API returns to say what happened (200, 404…) |
| **CORS** | Browser permission for one origin to call another |
| **TypeScript** | JavaScript with a compile-time type system |
| **Component** | A self-contained piece of Angular UI (template + class + styles) |
| **Data binding** | Keeping an Angular template and its class in sync |
| **Service (Angular)** | A class holding shared data/logic, injected into components |
| **HttpClient** | Angular's tool for making HTTP calls |
| **Observable** | A stream of values that arrive over time; you subscribe to it |
| **Reactive form** | An Angular form whose structure is defined in the component class |
| **SCSS** | CSS with variables, nesting, and partials |
| **CSS custom property** | A runtime-changeable variable (`--name`), good for theming |
| **Flexbox** | A CSS layout system for rows/columns |
| **Unit test** | Code that verifies a small piece of your code behaves correctly |
| **Mock** | A fake object standing in for a real dependency in a test |
| **AAA** | Arrange, Act, Assert — the structure of a good test |
| **Commit** | A snapshot of your code in Git history |

---

## 14. The Resource Library

Everything in one place. Read each section *when you reach that topic in a sprint*, not all at once.

**C# & .NET**
- [C# Language Tour](https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/)
- [Inheritance & OOP](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [N-Tier / Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/dependency-injection)

**SQL**
- [SQLite Tutorial](https://www.sqlitetutorial.net/)
- [T-SQL SELECT (MSSQL, for work)](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-transact-sql)
- [SQLBolt interactive](https://sqlbolt.com/)

**TypeScript & Angular**
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [Angular tutorial](https://angular.dev/tutorials/learn-angular)
- [Angular components](https://angular.dev/guide/components)
- [Reactive forms](https://angular.dev/guide/forms/reactive-forms)
- [HttpClient](https://angular.dev/guide/http)

**HTML & SCSS**
- [MDN HTML](https://developer.mozilla.org/en-US/docs/Learn/HTML)
- [MDN Flexbox](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_flexible_box_layout)
- [SCSS guide](https://sass-lang.com/guide/)
- [Angular component styling](https://angular.dev/guide/components/styling)

**Testing**
- [xUnit with .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Moq quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [Testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

**Git & Agile**
- [GitHub Git basics](https://docs.github.com/en/get-started/using-git/about-git)
- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
- [Atlassian Agile & Scrum](https://www.atlassian.com/agile/scrum)

---

*End of guide. Keep it in `/docs` and update it as you learn — the best textbook is one you've scribbled in.*
