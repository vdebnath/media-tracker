# Entity Framework Core Deep Dive

> A practical EF Core reference covering both **SQLite** and **SQL Server**.
>
> Code samples use a generic **Product catalog** domain (`Product`, `Category`) —
> deliberately *not* our Media Tracker project. Map each concept to `MediaItem` /
> `MediaTrackerDbContext` yourself as you read; that mapping is the point.

---

## 1. What EF Core actually is

Entity Framework Core is an **ORM** — an *Object-Relational Mapper*. Its job is to sit
between your C# objects and your relational database so you (mostly) don't write raw SQL.

The "mapping" goes both directions:

| C# world | Database world |
|---|---|
| an entity class | a table |
| a property on that class | a column |
| an instance of the class | a row |
| `DbSet<T>` | the table as a whole |
| LINQ query | translated into SQL |

The key mental shift: **you work with objects and collections, and EF Core figures out
the SQL.** You write `context.Products.Where(p => p.Category == Category.Electronics)`,
EF Core emits `SELECT ... FROM Products WHERE Category = 0`.

---

## 2. The three building blocks

### 2.1 The entity (model) class

A plain C# class that maps to a table. No logic, just data.

```csharp
namespace Catalog.Data.Models
{
    public enum Category { Electronics, Apparel, Grocery }

    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public Category Category { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
```

**Why `Id` matters specifically:** EF Core uses *convention over configuration*. A property
named `Id` (or `<ClassName>Id`, e.g. `ProductId`) is automatically treated as the
**primary key** and, for integer keys, set to auto-increment. You don't have to tell it.

**Enums under the hood:** `Category` is stored as an integer in the database
(`Electronics`=0, `Apparel`=1, `Grocery`=2). In C# you write `Category.Electronics`; in the
database it's just `0`. Type-safety in code, compact storage in the DB.

**Nullability maps to NULL:** `required string Name` → `NOT NULL` column. `string?
Description` → nullable column. This works *because EF reads your model's nullable
annotations as part of its conventions* — but only for things EF infers. Some constraints
(max length, defaults) you have to declare explicitly (see §8).

### 2.2 The DbContext

The `DbContext` is the **live object your code uses at runtime** to talk to the database.
Think of it as a session: it tracks the objects you've loaded, notices changes you make, and
knows how to push those changes back as SQL when you call `SaveChanges()`.

```csharp
using Microsoft.EntityFrameworkCore;
using Catalog.Data.Models;

namespace Catalog.Data
{
    public class CatalogDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options)
        {
        }
    }
}
```

Two parts:

- **`DbSet<Product> Products`** — represents the table. This is what you query against
  (`context.Products...`) and add to (`context.Products.Add(product)`).
- **The constructor taking `DbContextOptions<CatalogDbContext>`** — this is *how* the context
  gets configured (which provider, which connection string). Critically, the context **does
  not configure itself**. It receives its configuration from outside, passed in via
  dependency injection. In an N-Tier layout, this is what keeps your data-access layer
  ignorant of *which* database it's pointed at — that decision lives wherever the app
  actually starts up.

> **Anti-pattern to avoid:** the `OnConfiguring` override with an inline
> `UseSqlite("Data Source=...")` or `UseSqlServer("...")`. Lots of beginner tutorials do this
> because it's quick to demo, but it hardcodes the provider and connection string directly
> into the class that defines the entity model, collapsing the separation between "defines
> the context" and "configures the context." The constructor-injection pattern above is the
> one you want for any real application.

### 2.3 The DbContextOptions

`DbContextOptions` is a bundle of configuration: the database provider, the connection
string, logging, retry behavior, etc. It's built once at startup and handed to the context.
Because it's *injected* rather than hardcoded, the same `DbContext` class can point at a real
database in production or an in-memory/test database in unit tests — without changing a line
of the context itself.

---

## 3. Choosing a provider: SQLite vs SQL Server

EF Core is provider-agnostic at the `DbContext` level — the difference between database
engines shows up in three places: the NuGet package, the extension method, and the
connection string format.

| | SQLite | SQL Server |
|---|---|---|
| **Package** | `Microsoft.EntityFrameworkCore.Sqlite` | `Microsoft.EntityFrameworkCore.SqlServer` |
| **Extension method** | `UseSqlite(...)` | `UseSqlServer(...)` |
| **Connection string** | `Data Source=catalog.db` | `Server=localhost;Database=Catalog;Trusted_Connection=True;TrustServerCertificate=True` |
| **What it's pointing at** | a single file on disk | a running SQL Server instance (local or remote) |

SQLite's connection string is just a file path — there's no server, no host/port, no
credentials, because the "database" *is* the file. SQL Server's connection string identifies
a server, a database name on that server, and how to authenticate to it (`Trusted_Connection`
for Windows auth, or `User Id=...;Password=...` for SQL auth).

Everything else in this guide — model conventions, migrations, LINQ, `SaveChanges` — works
identically regardless of which provider you choose. Only the registration step differs.

---

## 4. Wiring it up at startup

The runnable application (the one with `Program.cs`, the one you actually launch) owns
startup configuration: the connection string and the DI registration.

**`appsettings.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=catalog.db"
  }
}
```

```json
// SQL Server equivalent
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Catalog;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**`Program.cs` — SQLite:**

```csharp
using Catalog.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();
app.Run();
```

**`Program.cs` — SQL Server (only the registration line changes):**

```csharp
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Things to notice:

- **`GetConnectionString("DefaultConnection")`** looks inside the `ConnectionStrings` section
  for the key you name. The key isn't just a label — it's the literal lookup string your code
  uses, so the name in `appsettings.json` and the name passed to `GetConnectionString` must
  match exactly.
- **`AddDbContext<CatalogDbContext>(...)`** registers the context with the DI container. The
  lambda receives a `DbContextOptionsBuilder`, on which you call the provider-specific
  extension method (`UseSqlite` or `UseSqlServer`).
- **All of this must happen *before* `builder.Build()`.** You're configuring the service
  container; once `Build()` is called, the container is locked. Registration has to come
  first.
- **Project reference required:** whatever project contains `Program.cs` needs a project
  reference to whatever project defines `CatalogDbContext`, or none of this compiles.

---

## 5. Migrations

A **migration** is a generated, version-controlled C# file describing how to transform the
database schema from its current state to match your model. EF Core diffs your model against
the last known schema and writes the difference. This is the mechanism that *creates your
database from your model* — you don't build tables by hand.

### Required packages

| Package | Goes in | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore.Sqlite` **or** `...SqlServer` | wherever `DbContext` lives | the provider |
| `Microsoft.EntityFrameworkCore.Design` | the startup project (the one with `Program.cs`) | enables the CLI tooling |
| `dotnet-ef` (global tool) | machine-wide | the `dotnet ef` commands |

Install the global tool once:

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version   # confirm it's there
```

### Creating and applying the first migration

The commands are **identical for SQLite and SQL Server** — the provider only affects what
SQL gets generated under the hood, not the CLI workflow:

```bash
# Generate the migration (creates a Migrations/ folder with C# files)
dotnet ef migrations add InitialCreate --project <DataProjectPath> --startup-project <ApiProjectPath>

# Apply it — this actually creates/updates the database
dotnet ef database update --project <DataProjectPath> --startup-project <ApiProjectPath>
```

- **`migrations add InitialCreate`** — `InitialCreate` is just a name; pick something
  descriptive. The first one is conventionally called `InitialCreate`.
- **`--project`** — where the `DbContext` and model live.
- **`--startup-project`** — the runnable project that has the configuration and DI wiring. EF
  needs this to know *how* the context is configured (which connection string, which
  provider).

With SQLite, `database update` produces a `.db` file you can open in DB Browser for SQLite.
With SQL Server, it connects to the server named in your connection string and creates the
database/tables there — you'd inspect it with SQL Server Management Studio or Azure Data
Studio instead.

### The migration workflow over time

Whenever you change your model (add a property, change a type), repeat:

```bash
dotnet ef migrations add DescriptiveNameForTheChange --project ... --startup-project ...
dotnet ef database update --project ... --startup-project ...
```

Each migration is additive and committed to source control, so the schema history travels
with the codebase. Other useful commands:

```bash
dotnet ef migrations list          # show all migrations and which are applied
dotnet ef migrations remove        # undo the last migration (if not yet applied to DB)
dotnet ef database update <Name>   # migrate to a specific migration (forward or back)
```

> **Tip:** never hand-edit the schema once migrations exist. If your model and your database
> drift apart, EF gets confused about what's already applied. Always change the model, then
> add a migration — let EF generate the diff.

---

## 6. Querying with LINQ

Once the context is wired up, you query the `DbSet` with LINQ. EF Core translates it to SQL
— the same LINQ code works against SQLite or SQL Server without modification.

```csharp
// Get everything (executes immediately because of ToList)
var all = context.Products.ToList();

// Filter
var electronics = context.Products
    .Where(p => p.Category == Category.Electronics)
    .ToList();

// Single item by primary key — returns null if not found
var product = context.Products.Find(id);          // by PK, checks tracker first
var product2 = context.Products
    .FirstOrDefault(p => p.Id == id);              // by any predicate

// Sorting and projection
var recentNames = context.Products
    .OrderByDescending(p => p.DateAdded)
    .Select(p => p.Name)
    .ToList();
```

### Deferred vs immediate execution

This trips people up. A LINQ query is **not executed when you write it** — it's executed when
you *enumerate* it. Methods like `ToList()`, `FirstOrDefault()`, `Count()`, `Find()`, or a
`foreach` trigger the actual SQL. Until then you're just building up an expression.

```csharp
var query = context.Products.Where(p => p.Category == Category.Apparel); // no SQL yet
var apparel = query.ToList();                                            // SQL runs HERE
```

This matters because you can compose queries before they hit the database:

```csharp
var query = context.Products.AsQueryable();
if (filterInStock)
    query = query.Where(p => p.Price > 0);
var results = query.ToList(); // one SQL statement with whatever filters were applied
```

### Async variants

In a web API you'll typically use the async versions so the request thread isn't blocked
while the database responds:

```csharp
var all = await context.Products.ToListAsync();
var one = await context.Products.FindAsync(id);
```

---

## 7. Saving changes (Add / Update / Delete)

EF Core's change tracker watches the entities the context knows about. You make changes to
objects, then call `SaveChanges()` (or `SaveChangesAsync()`) once to flush them all to the
database in a single transaction.

```csharp
// CREATE
var product = new Product
{
    Name = "Wireless Mouse",
    Category = Category.Electronics,
    Price = 24.99m,
    DateAdded = DateTime.UtcNow
};
context.Products.Add(product);
await context.SaveChangesAsync();   // INSERT runs here; product.Id is now populated

// UPDATE
var existing = await context.Products.FindAsync(id);
if (existing is not null)
{
    existing.Price = 19.99m;
    await context.SaveChangesAsync(); // EF detects the change, emits UPDATE
}

// DELETE
var toDelete = await context.Products.FindAsync(id);
if (toDelete is not null)
{
    context.Products.Remove(toDelete);
    await context.SaveChangesAsync(); // DELETE runs here
}
```

Key points:

- **`Add` then `SaveChanges`** — `Add` stages the insert; nothing hits the database until you
  save. After saving, EF populates the auto-generated `Id` back onto your object.
- **Updates are tracked automatically** — because you loaded `existing` *through the
  context*, EF is watching it. Changing a property is enough; `SaveChanges` figures out the
  `UPDATE`. (A detached object — say one deserialized from an HTTP request body — isn't
  automatically tracked; you typically load the tracked entity first and copy values onto it,
  or explicitly attach it.)
- **`SaveChanges` is the commit point.** Multiple `Add`/`Remove`/property changes between
  saves are batched into one transaction.

---

## 8. Going beyond conventions: Fluent API

Conventions cover a lot, but some constraints aren't inferred from C# types alone — e.g. a
`NOT NULL` is inferred from non-nullable reference types, but a **max length**, a **default
value**, or an **index** is not. You declare those explicitly by overriding
`OnModelCreating` in the context:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(p => p.Name)
              .IsRequired()
              .HasMaxLength(200);

        entity.Property(p => p.DateAdded)
              .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQL Server: GETUTCDATE()
    });
}
```

This is called the **Fluent API**. The alternative is **data annotations** — attributes like
`[Required]`, `[MaxLength(200)]` placed directly on the model properties. Both work; Fluent
API keeps the model class free of attributes and is more powerful for complex configuration.
Whichever you use, **remember to add a migration afterward** so the schema actually picks up
the change — `OnModelCreating` only affects what the *next* migration generates, not the
database that already exists.

> Note the SQL Server default-value function differs from SQLite's (`GETUTCDATE()` vs
> `CURRENT_TIMESTAMP`) — this is one of the rare places where switching providers means
> revisiting provider-specific SQL fragments, even though the C# around it stays the same.

---

## 9. Where this fits in a layered (N-Tier) architecture

If your application is split into layers — a data-access layer, a business-logic/service
layer, and an API/presentation layer — EF Core usage should be scoped tightly:

| Layer | EF Core responsibility |
|---|---|
| **Data-access layer** | Entity classes, `DbContext`, migrations, and a **repository** that wraps the context. The *only* layer that touches `DbContext` directly. |
| **Service/business layer** | Calls the repository through its interface. Holds business logic. **Never** touches `DbContext` directly. |
| **API/host layer** | Owns the connection string and the startup DI wiring (`AddDbContext`, `UseSqlite`/`UseSqlServer`). Calls the service. |

The golden rule: **only the data-access layer (specifically the repository) imports and uses
`DbContext`.** If you find yourself wanting `using Microsoft.EntityFrameworkCore` inside a
service or API project to write query logic directly, that's a sign the abstraction has
leaked — the repository exists precisely so the rest of the app depends on an *interface*,
not on EF Core itself.

---

## 10. Quick-reference cheat sheet

```bash
# Tooling
dotnet tool install --global dotnet-ef
dotnet ef --version

# Migrations (provider-agnostic; adjust --project paths to your layout)
dotnet ef migrations add <Name>  --project <DataProject> --startup-project <ApiProject>
dotnet ef database update        --project <DataProject> --startup-project <ApiProject>
dotnet ef migrations list        --project <DataProject> --startup-project <ApiProject>
dotnet ef migrations remove      --project <DataProject> --startup-project <ApiProject>
```

```csharp
// Read
await context.Products.ToListAsync();
await context.Products.FindAsync(id);
context.Products.Where(p => p.Category == Category.Electronics).ToList();

// Create
context.Products.Add(product);
await context.SaveChangesAsync();

// Update (entity loaded through the context)
existing.Price = 19.99m;
await context.SaveChangesAsync();

// Delete
context.Products.Remove(product);
await context.SaveChangesAsync();
```

| Provider | Package | Extension method | Connection string shape |
|---|---|---|---|
| SQLite | `Microsoft.EntityFrameworkCore.Sqlite` | `UseSqlite(...)` | `Data Source=<file>.db` |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` | `UseSqlServer(...)` | `Server=...;Database=...;...` |

| Concept | One-liner |
|---|---|
| `DbContext` | Runtime session; tracks changes, talks to the DB |
| `DbSet<T>` | Represents a table; what you query and add to |
| `DbContextOptions` | Injected configuration (provider + connection string) |
| Convention | `Id` → primary key, auto-increment, no config needed |
| Migration | Generated schema-change script; creates/updates the database |
| LINQ | C# query syntax translated to SQL by EF |
| Deferred execution | Query runs when enumerated (`ToList`, `foreach`), not when written |
| `SaveChanges()` | The commit point; flushes tracked changes in one transaction |
| Fluent API | Explicit config in `OnModelCreating` for what conventions miss |

---

*General-purpose EF Core reference. Map each `Product`/`Category`/`CatalogDbContext` example
to your own project's entities as you read.*
