# TypeScript & Angular Deep Dive for Media Tracker
### The frontend, from language fundamentals to a wired-up UI

> **Same ground rule.** Examples use a **Contacts** address-book app, never your Media Tracker — translate everything to your own code. macOS terminal commands are marked 🖥️ and assume you're inside the `frontend/media-tracker-ui` folder unless noted.

> **Where this fits.** This is the `media-tracker-ui` Angular project — everything that runs in the browser. It lands mostly in Sprint 3. If you've only just finished JavaScript fundamentals, Part 1 (TypeScript) is the bridge from JS to the typed world; Part 2 (Angular) is the framework that organises it all.

---

## Table of Contents

**Part 1 — TypeScript**
1. [The Angular CLI on macOS](#1-the-angular-cli-on-macos)
2. [Why TypeScript, and How It Runs](#2-why-typescript)
3. [Types & Type Inference](#3-types--type-inference)
4. [Interfaces & Types](#4-interfaces--types)
5. [Functions & Arrow Functions](#5-functions--arrow-functions)
6. [Classes & Access Modifiers](#6-classes--access-modifiers)
7. [Generics & Union Types](#7-generics--union-types)

**Part 2 — Angular**
8. [Project Structure](#8-project-structure)
9. [Components](#9-components)
10. [Templates & Data Binding](#10-templates--data-binding)
11. [Control Flow: Loops & Conditionals](#11-control-flow)
12. [Services & Dependency Injection](#12-services--dependency-injection)
13. [HttpClient & Observables](#13-httpclient--observables)
14. [Reactive Forms](#14-reactive-forms)
15. [Lifecycle Hooks](#15-lifecycle-hooks)
16. [The Flow of a Click](#16-the-flow-of-a-click)
17. [Resources](#17-resources)

---

# Part 1 — TypeScript

## 1. The Angular CLI on macOS

The `ng` command is your control panel for everything Angular.

### Creating & running

```bash
🖥️ ng new media-tracker-ui      # create a new app (asks: stylesheet? → SCSS; SSR? → No)
🖥️ ng serve                      # run dev server at http://localhost:4200, auto-reloads
🖥️ ng serve --open               # same, but opens your browser automatically
🖥️ ng build                      # production build into /dist
```

`ng serve` watches your files and live-reloads the browser on every save — leave it running in one terminal tab while you work. `Ctrl+C` stops it.

### Generating code (scaffolding — fair game to use)

```bash
🖥️ ng generate component contact-list    # or short:  ng g c contact-list
🖥️ ng generate service contact            # or short:  ng g s contact
🖥️ ng generate interface contact          # or short:  ng g i contact
```

`ng g c contact-list` creates a folder with four files — the component class, template, styles, and a test file — and registers it. Generating the *skeleton* this way is scaffolding (allowed); writing the actual template logic and component behaviour inside is your job.

### npm (Node's package manager) on macOS

```bash
🖥️ npm install                  # install everything in package.json (run after cloning)
🖥️ npm install <package>         # add a dependency
🖥️ npm install -g @angular/cli   # update the global Angular CLI
```

> **macOS note:** if `ng` ever returns "command not found" after a Node upgrade, reinstall the CLI globally with the last command. Global npm binaries on macOS usually live under a path npm prints during install.

---

## 2. Why TypeScript

JavaScript runs in the browser but has no type checking — `"3" * 2` is `6`, `"3" + 2` is `"32"`, and nothing warns you. **TypeScript** is a superset of JavaScript that adds a *compile-time* type system. You write TypeScript; a compiler checks your types and then strips them away, producing plain JavaScript the browser runs. The types exist only to catch your mistakes before runtime — they vanish in the output.

For you, coming from C#, TypeScript will feel like home: it has types, interfaces, generics, and access modifiers. The big mental shift from C# is that it's still *JavaScript underneath* — async works differently, and the type system is "structural" (shapes matter more than names), which we'll see.

---

## 3. Types & Type Inference

```typescript
let count: number = 0;
let title: string = "Dune";
let isDone: boolean = false;
let tags: string[] = ["sci-fi", "classic"];   // array of strings
let anything: any = "avoid this";              // turns OFF type checking — use sparingly
```

### Inference: you don't always annotate

TypeScript infers types from values, so you can often skip the annotation:

```typescript
let title = "Dune";     // inferred as string
// title = 5;           // error — it's locked to string
```

Annotate explicitly when it aids clarity or when there's no value to infer from (like function parameters). Avoid `any` — it silences the very safety you're using TypeScript for.

### `null` and `undefined`

```typescript
let phone: string | null = null;       // explicitly allow null
let nickname: string | undefined;      // might be unset
let optional?: string;                 // the ? shorthand for "| undefined"
```

The `?` for optional is the TypeScript echo of C#'s `string?` — same idea, slightly different mechanics.

---

## 4. Interfaces & Types

### Interfaces describe object shapes

This is your most-used TypeScript feature — it's how you mirror your C# models on the frontend.

```typescript
export interface Contact {
  id: number;
  fullName: string;
  email: string;
  phone?: string;        // optional
  dateCreated: string;
}
```

Any object that has those properties *is* a `Contact` — TypeScript is **structurally typed**, meaning it checks shape, not name. If it has the right properties, it fits.

### The casing rule (the #1 frontend↔backend gotcha)

Your C# model uses `PascalCase` (`FullName`). When ASP.NET Core serialises it to JSON, it converts to `camelCase` (`fullName`) by default. So your TypeScript interface must use **camelCase** to match what actually arrives over the wire:

```typescript
// C#:  public string FullName { get; set; }
// JSON on the wire:  { "fullName": "Ada" }
// TypeScript interface:
export interface Contact {
  fullName: string;     // camelCase — matches the JSON
}
```

When a value is mysteriously `undefined` in your component, check the casing first. It's almost always this.

### `type` aliases

`type` does a similar job and is great for unions (next chapter):

```typescript
type Status = 'Backlog' | 'InProgress' | 'Completed';
type Id = number | string;
```

Rule of thumb: `interface` for object shapes, `type` for unions and aliases. They overlap a lot; don't agonise over it.

---

## 5. Functions & Arrow Functions

```typescript
// Typed function — annotate params and return
function add(a: number, b: number): number {
  return a + b;
}

// Arrow function — concise, you'll see these everywhere
const add2 = (a: number, b: number): number => a + b;

// void return — does something, returns nothing
function log(message: string): void {
  console.log(message);
}
```

Arrow functions (`=>`) are the C# lambda's twin and dominate Angular code — every `.subscribe(data => ...)` and event handler uses them. They also handle `this` more predictably than old-style functions, which matters inside classes.

---

## 6. Classes & Access Modifiers

Angular components and services *are* classes, so this matters.

```typescript
export class ContactService {
  private apiUrl = 'http://localhost:5000/api/contacts';  // only this class sees it
  public readonly version = '1.0';                        // public, can't be reassigned

  constructor(private http: HttpClient) {}   // see the shortcut below

  getAll() {
    return this.http.get<Contact[]>(this.apiUrl);
  }
}
```

### The constructor parameter shortcut (very Angular)

Putting an access modifier (`private`, `public`) on a constructor parameter automatically creates and assigns a field of that name. These two are equivalent:

```typescript
// Long form
private http: HttpClient;
constructor(http: HttpClient) {
  this.http = http;
}

// Shorthand — what Angular code actually uses
constructor(private http: HttpClient) {}
```

This is exactly how dependencies get injected into components and services (Chapter 12). When you see `constructor(private http: HttpClient)`, read it as "inject an HttpClient and store it as `this.http`."

---

## 7. Generics & Union Types

### Generics

Same concept as C#: type-parameterised code.

```typescript
let contacts: Array<Contact> = [];     // same as Contact[]
this.http.get<Contact[]>(url);          // "this GET returns Contact[]" — typed response
```

That `<Contact[]>` on the HTTP call tells TypeScript the response shape, so the data you get back is fully typed.

### Union types

A value that can be one of several types — incredibly handy for fixed sets of options:

```typescript
type Status = 'Backlog' | 'InProgress' | 'Completed';

let s: Status = 'Backlog';   // ✓
// let s: Status = 'Done';   // ✗ compile error — not in the union
```

This is perfect for your status and type fields — the compiler stops you from typo-ing a status that doesn't exist.

---

# Part 2 — Angular

## 8. Project Structure

After `ng new`, the parts that matter:

```
media-tracker-ui/
├── src/
│   ├── app/
│   │   ├── app.component.ts        # root component (class)
│   │   ├── app.component.html      # root template
│   │   ├── app.component.scss      # root styles
│   │   └── app.config.ts           # app-wide configuration & providers
│   ├── styles.scss                 # GLOBAL styles (your MSU theme lives here — SCRUM-11)
│   ├── main.ts                     # entry point that bootstraps the app
│   └── index.html                  # the single HTML page everything loads into
├── angular.json                    # build/serve configuration
└── package.json                    # dependencies & scripts
```

Angular is a **single-page application**: `index.html` loads once, and Angular swaps content in and out via components without full page reloads. Your global theme goes in `styles.scss`; each component gets its own scoped `.scss`.

---

## 9. Components

A component is a reusable chunk of UI. Modern Angular uses **standalone components** (no NgModules needed). The class is decorated with `@Component`:

```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-contact-list',           // use as <app-contact-list></app-contact-list>
  standalone: true,
  templateUrl: './contact-list.component.html',
  styleUrl: './contact-list.component.scss'
})
export class ContactListComponent {
  contacts: Contact[] = [];                // data the template can show
  title = 'My Contacts';

  deleteContact(id: number): void {        // behaviour the template can call
    // your logic here
  }
}
```

- **`selector`** — the custom HTML tag you place the component with.
- **`templateUrl` / `styleUrl`** — point to the HTML and SCSS files.
- The **class body** holds the data (properties) and behaviour (methods) the template can use.

The template and class are two halves of one unit: the class provides `contacts` and `deleteContact`, the template displays and triggers them. Keeping that pairing in mind makes data binding (next) obvious.

---

## 10. Templates & Data Binding

Data binding keeps the template and class in sync. Four forms — memorise the symbols:

```html
<!-- {{ }} Interpolation: show a class value as text -->
<h1>{{ title }}</h1>

<!-- [ ] Property binding: class value → element property (data flows IN) -->
<button [disabled]="isSaving">Save</button>
<div [class.active]="isSelected">Row</div>

<!-- ( ) Event binding: element event → class method (data flows OUT) -->
<button (click)="deleteContact(contact.id)">Delete</button>
<input (input)="onSearch($event)">

<!-- [( )] Two-way binding: both directions (forms) -->
<input [(ngModel)]="searchText">
```

The shorthand: `{{ }}` **shows**, `[ ]` goes **in**, `( )` comes **out**, `[( )]` does **both** (the "banana in a box"). Two-way binding is really just property binding + event binding fused together.

---

## 11. Control Flow

Modern Angular (v17+) uses block-style control flow right in the template:

```html
<!-- Loop -->
@for (contact of contacts; track contact.id) {
  <div class="contact-row">
    {{ contact.fullName }}
    <button (click)="deleteContact(contact.id)">Delete</button>
  </div>
} @empty {
  <p>No contacts yet.</p>
}

<!-- Conditional -->
@if (isLoading) {
  <p>Loading…</p>
} @else {
  <div class="list"><!-- ... --></div>
}

<!-- Switch -->
@switch (contact.status) {
  @case ('Completed') { <span class="badge green">Done</span> }
  @default { <span class="badge gray">{{ contact.status }}</span> }
}
```

- **`track`** in `@for` is required — it tells Angular how to identify each item (use a unique id) so it can update the list efficiently instead of redrawing everything.
- **`@empty`** renders when the collection is empty — perfect for "no items" states.

> You'll see the older `*ngFor` / `*ngIf` directive syntax in many tutorials and older codebases. It does the same thing. Write the new block syntax; recognise the old.

---

## 12. Services & Dependency Injection

### Why services

Components should focus on the *view*. Anything else — fetching data, shared logic, state — belongs in a **service**. This is the exact same separation-of-concerns idea as your backend's service layer.

```typescript
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })   // makes it injectable app-wide as a singleton
export class ContactService {
  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<Contact[]>('http://localhost:5000/api/contacts');
  }
}
```

`@Injectable({ providedIn: 'root' })` registers the service with Angular's DI system as a single shared instance for the whole app.

### Injecting into a component

```typescript
export class ContactListComponent {
  constructor(private contactService: ContactService) {}   // injected automatically
}
```

Recognise this? It's the same DI pattern as your C# controllers and services. You declare what you need in the constructor; Angular's DI container builds and supplies it. Once you notice DI is *identical in spirit* on both ends of the stack, the whole app stops feeling like two unrelated technologies and starts feeling like one idea expressed twice.

---

## 13. HttpClient & Observables

### Setting up HttpClient

`HttpClient` needs to be provided once. In modern Angular that's in `app.config.ts`:

```typescript
import { provideHttpClient } from '@angular/common/http';

export const appConfig = {
  providers: [provideHttpClient()]
};
```

### Observables: the part that's genuinely new

`HttpClient` methods don't return data directly — they return an **Observable**. An Observable is a *stream of values that arrive over time*. The network call hasn't happened yet when you get the Observable back; it fires when you **subscribe**:

```typescript
export class ContactListComponent implements OnInit {
  contacts: Contact[] = [];

  constructor(private contactService: ContactService) {}

  ngOnInit(): void {
    this.contactService.getAll().subscribe({
      next: (data) => this.contacts = data,        // runs when the response arrives
      error: (err) => console.error(err)           // runs if the call fails
    });
  }
}
```

The `subscribe` callback runs *later*, when the HTTP response comes back. Until then your `contacts` array is empty — which is why you often show a "loading" state.

> **First-pass mental model:** treat an Observable as "a Promise you have to subscribe to." That's not the whole truth (Observables can emit many values, can be cancelled, and come with a toolbox called RxJS), but it'll carry you cleanly through this project. The deeper RxJS material can wait until you have a reason to need it.

### Why not just return data?

Because the network takes time and you don't want to freeze the UI waiting. Observables (like async/await on the backend) are how the frontend stays responsive while a request is in flight. Same problem, different language's solution.

---

## 14. Reactive Forms

Your add/edit form uses **reactive forms**, where the form's structure is defined in the *component class* (not scattered through the template). This gives you clean validation and value handling.

### Setup

```typescript
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],     // required to use reactive forms in a standalone component
  // ...
})
export class ContactFormComponent {
  contactForm = this.fb.group({
    fullName: ['', Validators.required],          // initial value + validation rules
    email: ['', [Validators.required, Validators.email]],
    phone: ['']                                    // optional, no validators
  });

  constructor(private fb: FormBuilder) {}

  onSubmit(): void {
    if (this.contactForm.valid) {
      const value = this.contactForm.value;        // the typed form data
      // send `value` to your service here
    }
  }
}
```

### The template side

```html
<form [formGroup]="contactForm" (ngSubmit)="onSubmit()">
  <input formControlName="fullName" placeholder="Full name">
  @if (contactForm.get('fullName')?.invalid && contactForm.get('fullName')?.touched) {
    <span class="error">Name is required</span>
  }

  <input formControlName="email" placeholder="Email">

  <button type="submit" [disabled]="contactForm.invalid">Save</button>
</form>
```

Key pieces: `[formGroup]` binds the template form to your class's `FormGroup`; `formControlName` links each input to a control; `(ngSubmit)` fires your handler; and `Validators` give you `.invalid` / `.valid` / `.touched` flags to drive error messages and disable the button. This is one of the meatier Angular topics — give it real time when you reach it, and lean on the official guide.

---

## 15. Lifecycle Hooks

Angular calls specific methods at specific moments in a component's life. The one you'll use most:

```typescript
export class ContactListComponent implements OnInit {
  ngOnInit(): void {
    // runs ONCE after the component is created — the right place to load data
    this.loadContacts();
  }
}
```

**Why not load data in the constructor?** The constructor is for setting up the class (receiving injected dependencies). `ngOnInit` runs after Angular has finished wiring the component up, so it's the correct, conventional place for startup work like fetching from your API. Putting data-loading here, not in the constructor, is a small habit that marks code as idiomatic Angular.

Others exist (`ngOnDestroy` for cleanup, etc.), but `ngOnInit` covers the vast majority of your needs in this project.

---

## 16. The Flow of a Click

The frontend mirror of the C# guide's "flow of a request." A user clicks "Delete" on a contact:

```
1. (click)="deleteContact(c.id)" in the template fires the component method
        │
        ▼
2. Component calls  this.contactService.delete(id).subscribe(...)
        │   (service injected via DI)
        ▼
3. Service calls  this.http.delete(`${apiUrl}/${id}`)  → returns an Observable
        │
        ▼
4. HTTP DELETE request crosses the network to your C# API
        │   ... (the entire C#-guide flow happens on the backend) ...
        ▲
5. API responds (e.g. 204 No Content)
        ▲
6. The Observable emits; your subscribe callback runs
        ▲
7. Component updates its `contacts` array → Angular re-renders the list automatically
```

Step 7 is the payoff of data binding: you change the class property, and the template updates itself. You never touch the DOM by hand. When you can narrate this click *and* hand off cleanly to the backend request flow, you understand the full stack — which is the entire point of this project.

---

## 17. Resources

**TypeScript**
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [Everyday Types](https://www.typescriptlang.org/docs/handbook/2/everyday-types.html)

**Angular**
- [Learn Angular (official tutorial)](https://angular.dev/tutorials/learn-angular)
- [Components](https://angular.dev/guide/components)
- [Templates & binding](https://angular.dev/guide/templates)
- [Control flow](https://angular.dev/guide/templates/control-flow)
- [Dependency injection](https://angular.dev/guide/di)
- [HttpClient](https://angular.dev/guide/http)
- [Reactive forms](https://angular.dev/guide/forms/reactive-forms)
- [Lifecycle hooks](https://angular.dev/guide/components/lifecycle)
- [Component styling](https://angular.dev/guide/components/styling)

**CLI**
- [Angular CLI reference](https://angular.dev/cli)

---

*Keep this beside the field guide and the C# deep dive in `/docs`. Part 1 will make sense immediately; Part 2 clicks once you're building components and watching the browser live-reload. Come back and reread sections as you hit them in Sprint 3.*
