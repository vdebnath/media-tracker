# HTML, CSS, SCSS & Angular Styling Deep Dive
### Structure, appearance, and how it all wires into your Angular components

> **Same ground rule.** Examples use a **Contacts** address-book app, never your Media Tracker — read, understand, translate. macOS terminal commands marked 🖥️. This guide is your reference for Sprint 3 when you're building the Angular frontend.

> **Where this fits.** This covers everything between "here's a component class" and "here's a polished page." HTML is the bones, CSS is the skin, SCSS is CSS with a power drill, and Angular's binding system is what makes all of it *alive*. They're inseparable in practice so they're together in one guide.

---

## Table of Contents

**Part 1 — HTML**
1. [What HTML Actually Is](#1-what-html-actually-is)
2. [Elements, Tags & Attributes](#2-elements-tags--attributes)
3. [Document Structure](#3-document-structure)
4. [Semantic HTML](#4-semantic-html)
5. [Forms & Inputs](#5-forms--inputs)
6. [HTML in Angular Templates](#6-html-in-angular-templates)

**Part 2 — CSS Fundamentals**
7. [How CSS Works: The Cascade](#7-how-css-works-the-cascade)
8. [Selectors](#8-selectors)
9. [The Box Model](#9-the-box-model)
10. [Layout: Flexbox](#10-layout-flexbox)
11. [Layout: CSS Grid](#11-layout-css-grid)
12. [Typography](#12-typography)
13. [Colors & Backgrounds](#13-colors--backgrounds)
14. [Pseudo-classes & Pseudo-elements](#14-pseudo-classes--pseudo-elements)
15. [Responsive Design & Media Queries](#15-responsive-design--media-queries)
16. [CSS Custom Properties (Variables)](#16-css-custom-properties)
17. [Transitions & Animations](#17-transitions--animations)

**Part 3 — SCSS**
18. [What SCSS Adds](#18-what-scss-adds)
19. [SCSS Variables](#19-scss-variables)
20. [Nesting](#20-nesting)
21. [Partials & @use](#21-partials--use)
22. [Mixins](#22-mixins)
23. [Functions & @each](#23-functions--each)
24. [SCSS vs CSS Custom Properties: When to Use Which](#24-scss-vs-css-custom-properties)

**Part 4 — Angular + Styling**
25. [Component Styles & View Encapsulation](#25-component-styles--view-encapsulation)
26. [Global vs Component-Scoped Styles](#26-global-vs-component-scoped-styles)
27. [Class & Style Binding](#27-class--style-binding)
28. [Light/Dark Mode Toggle (The MSU Theme)](#28-lightdark-mode-toggle)
29. [Putting It Together: A Styled Contact List](#29-putting-it-together)
30. [Resources](#30-resources)

---

# Part 1 — HTML

## 1. What HTML Actually Is

**HTML (HyperText Markup Language)** is the structure of a web page. It describes *what things are* — not how they look, not what they do. A heading is a heading, a list is a list, a button is a button. Appearance is CSS's job; behaviour is JavaScript/TypeScript's job. Keep these three concerns separate in your head even when they work together.

A browser reads your HTML top to bottom, builds a tree of objects from it (the **DOM — Document Object Model**), and paints the result to screen. CSS then styles those objects; JavaScript/Angular can read and modify them. Angular specifically works by *manipulating the DOM* on your behalf based on your templates — when you use `@for` or data binding, Angular is updating the DOM tree for you.

---

## 2. Elements, Tags & Attributes

HTML is made of **elements**. Most have an opening tag, content, and a closing tag:

```html
<p>This is a paragraph.</p>
<!-- opening ^     ^ closing (note the slash) -->
```

Some elements are **self-closing** (void elements) — they have no content or closing tag:

```html
<input type="text" placeholder="Search...">
<img src="logo.png" alt="Site logo">
<br>
<hr>
```

**Attributes** go inside the opening tag and provide extra information. The pattern is always `name="value"`:

```html
<a href="https://example.com" target="_blank">Open link</a>
<!-- ^ attribute name  ^ attribute value -->
```

Common universal attributes (work on any element):

| Attribute | Purpose |
|-----------|---------|
| `id` | Unique identifier — only one per page |
| `class` | One or more CSS classes (space-separated) |
| `style` | Inline CSS (avoid in favour of classes) |
| `data-*` | Custom data you can read in JavaScript |
| `hidden` | Hides the element |

---

## 3. Document Structure

A real HTML document has a required skeleton. In Angular this is `index.html` — you usually only touch it once:

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <!-- NOT visible on screen — metadata and links -->
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Media Tracker</title>
  <link rel="stylesheet" href="styles.css">  <!-- external stylesheet -->
</head>
<body>
  <!-- Everything visible goes here -->
  <app-root></app-root>  <!-- Angular's root component selector -->
</body>
</html>
```

In an Angular SPA, `<app-root>` is where the whole application lives. Angular swaps content inside it without ever reloading the page.

---

## 4. Semantic HTML

Semantic elements describe *meaning* rather than just layout. A `<div>` means "a box" (no meaning). A `<nav>` means "navigation." Semantics matter for accessibility, SEO, and readability.

```html
<!-- Non-semantic — just boxes, no meaning -->
<div class="header">
  <div class="nav">...</div>
</div>
<div class="main">
  <div class="article">...</div>
</div>

<!-- Semantic — the same thing, but meaningful -->
<header>
  <nav>...</nav>
</header>
<main>
  <article>...</article>
</main>
```

The semantic elements you'll use in Media Tracker:

| Element | Use for |
|---------|---------|
| `<header>` | Page or section header |
| `<nav>` | Navigation links (your sidebar) |
| `<main>` | The primary content area |
| `<section>` | A thematic grouping of content |
| `<aside>` | Sidebar or supplementary content |
| `<footer>` | Footer information |
| `<article>` | Self-contained content (a card, a row) |
| `<h1>–<h6>` | Headings, in hierarchical order |
| `<ul>` / `<ol>` | Unordered / ordered lists |
| `<li>` | A list item |
| `<button>` | An interactive button |
| `<form>` | A form container |
| `<label>` | Labels a form control |
| `<input>` | A form field |
| `<select>` / `<option>` | A dropdown |
| `<textarea>` | Multi-line text input |

> **Rule of thumb:** if you're reaching for a `<div>` or `<span>`, ask "is there a semantic element that describes what this *is*?" Only use `<div>` (block) and `<span>` (inline) when genuinely no semantic element applies — they're containers of last resort.

---

## 5. Forms & Inputs

Forms are where users submit data. In Angular you'll use Angular's reactive forms *on top of* HTML form elements, but you still need to know the HTML side.

```html
<form>
  <!-- Text input -->
  <label for="fullName">Full Name</label>
  <input id="fullName" type="text" placeholder="Ada Lovelace">

  <!-- Select dropdown -->
  <label for="type">Type</label>
  <select id="type">
    <option value="book">Book</option>
    <option value="movie">Movie</option>
    <option value="show">Show</option>
  </select>

  <!-- Textarea -->
  <label for="notes">Notes</label>
  <textarea id="notes" rows="4" placeholder="Optional notes..."></textarea>

  <!-- Buttons -->
  <button type="submit">Save</button>
  <button type="button">Cancel</button>
</form>
```

Key notes:
- **`for` on `<label>` matches `id` on `<input>`** — this links them so clicking the label focuses the input. Required for accessibility.
- **`type="submit"`** fires the form's submit event. **`type="button"`** does nothing by default — you attach your own click handler.
- In Angular's reactive forms, you won't typically use `action` on the form or `name` on inputs — Angular takes over. You wire the form to your component instead.

---

## 6. HTML in Angular Templates

Angular templates are HTML files with extra capabilities baked in. The key rule: **it's still valid HTML first**, with Angular directives and bindings layered on top. The browser never sees the Angular syntax — Angular processes it and produces regular DOM.

Things Angular adds to HTML templates:
- Interpolation: `{{ expression }}`
- Property binding: `[attribute]="expression"`
- Event binding: `(event)="handler()"`
- Structural directives: `@if`, `@for`, `@switch`
- Component selectors: `<app-contact-list>` becomes a full component

```html
<!-- A plain HTML button: -->
<button disabled>Save</button>

<!-- The same button, with Angular property binding: -->
<button [disabled]="form.invalid">Save</button>
<!-- Angular evaluates form.invalid and sets the disabled attribute dynamically -->
```

Angular templates stay close to HTML intentionally — the difference is that properties and events become *programmable*, driven by your component class.

---

# Part 2 — CSS Fundamentals

## 7. How CSS Works: The Cascade

**CSS (Cascading Style Sheets)** applies visual rules to HTML elements. The "cascading" means when multiple rules target the same element, a defined priority order determines which wins. Understanding this is the key to not fighting your own styles.

### Specificity

The more specific the selector, the higher the priority:

```
Inline style      > ID selector    > Class selector  > Element selector
style="..."          #id               .class              p, div
(1,0,0,0)           (0,1,0,0)         (0,0,1,0)          (0,0,0,1)
```

```css
p { color: gray; }            /* specificity: 0,0,0,1 */
.text { color: blue; }        /* specificity: 0,0,1,0 — wins over p */
#intro { color: green; }      /* specificity: 0,1,0,0 — wins over .text */
```

When specificity is equal, the **later rule** wins (cascade order). This is why global base styles come first, component-level overrides come later.

### Inheritance

Some CSS properties automatically pass from parent to child (`font-family`, `color`, `line-height`). Others don't inherit by default (`margin`, `padding`, `border`, `background`). You can force inheritance with `inherit`:

```css
.child { color: inherit; }   /* use whatever the parent has */
```

---

## 8. Selectors

Selectors target which elements a rule applies to. Master these — they're used in both plain CSS and SCSS.

```css
/* Element — all <p> tags */
p { }

/* Class — everything with class="card" */
.card { }

/* ID — the one element with id="sidebar" */
#sidebar { }

/* Descendant — any .title inside a .card, at any depth */
.card .title { }

/* Direct child — only .title that is a DIRECT child of .card */
.card > .title { }

/* Adjacent sibling — .badge immediately after a .title */
.title + .badge { }

/* Attribute — inputs with type="text" */
input[type="text"] { }

/* Multiple — apply same rules to both */
h1, h2 { }

/* Universal — everything (use sparingly) */
* { }
```

---

## 9. The Box Model

Every HTML element is a rectangular box. The CSS box model describes how its size is calculated:

```
┌─────────────────────────────────────┐
│              MARGIN                 │  ← space outside the element
│  ┌───────────────────────────────┐  │
│  │           BORDER              │  │  ← the visible border
│  │  ┌─────────────────────────┐  │  │
│  │  │         PADDING         │  │  │  ← space inside, between content & border
│  │  │  ┌───────────────────┐  │  │  │
│  │  │  │      CONTENT      │  │  │  │  ← text, images, child elements
│  │  │  └───────────────────┘  │  │  │
│  │  └─────────────────────────┘  │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

```css
.card {
  width: 300px;
  padding: 16px;           /* inside spacing — all four sides */
  border: 1px solid #ccc;
  margin: 24px;            /* outside spacing */
}
```

Shorthand for padding/margin (clockwise from top):
```css
padding: 16px;                    /* all four sides */
padding: 8px 16px;                /* top/bottom  left/right */
padding: 8px 16px 12px 16px;     /* top  right  bottom  left */
```

### The most important reset

By default, `width` is calculated *without* padding and border, which makes sizing counterintuitive. Fix it globally:

```css
*, *::before, *::after {
  box-sizing: border-box;
}
```

With `border-box`, a `width: 300px` element is *always* 300px wide including padding and border. This is in your SCRUM-11 base styles for good reason — always set this globally.

---

## 10. Layout: Flexbox

Flexbox is your primary layout tool for rows and columns. Apply it to a **container** and it controls how that container's **children** are arranged.

```css
.container {
  display: flex;                  /* activate flexbox */
  flex-direction: row;            /* row (default) or column */
  justify-content: space-between; /* alignment along the MAIN axis */
  align-items: center;            /* alignment along the CROSS axis */
  gap: 16px;                      /* space BETWEEN children */
  flex-wrap: wrap;                /* wrap to next line if children overflow */
}
```

### Main axis vs cross axis

In `row` direction: main axis is **horizontal** (left→right), cross axis is **vertical**.
In `column` direction: main axis is **vertical** (top→bottom), cross axis is **horizontal**.

`justify-content` controls the main axis; `align-items` controls the cross axis. Get that straight and Flexbox becomes predictable.

### `justify-content` values

```css
justify-content: flex-start;      /* pack left (or top) */
justify-content: flex-end;        /* pack right (or bottom) */
justify-content: center;          /* centre */
justify-content: space-between;   /* first at start, last at end, rest evenly spaced */
justify-content: space-around;    /* equal space around each item */
justify-content: space-evenly;    /* equal space between AND at edges */
```

### `align-items` values

```css
align-items: stretch;     /* default: fill the cross axis */
align-items: flex-start;  /* align to start of cross axis */
align-items: flex-end;    /* align to end of cross axis */
align-items: center;      /* centre on cross axis */
```

### Child properties

```css
.child {
  flex: 1;            /* grow to fill available space (shorthand for flex-grow: 1) */
  flex: 0 0 200px;    /* don't grow, don't shrink, stay 200px (flex-grow, flex-shrink, flex-basis) */
  align-self: flex-start;  /* override align-items for this child only */
}
```

### Your sidebar + main layout

```css
.app-layout {
  display: flex;
  height: 100vh;          /* full viewport height */
}
.sidebar {
  width: 240px;
  flex-shrink: 0;         /* don't let the sidebar shrink */
}
.main-content {
  flex: 1;                /* take all remaining space */
  overflow-y: auto;       /* scroll the main area, not the whole page */
}
```

---

## 11. Layout: CSS Grid

Grid is for **two-dimensional** layouts — rows *and* columns simultaneously. For Media Tracker you'll mostly use Flexbox, but Grid is worth knowing for card layouts.

```css
.card-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);   /* 3 equal columns */
  gap: 16px;
}

/* Explicit rows and columns */
.dashboard {
  display: grid;
  grid-template-columns: 240px 1fr;       /* sidebar 240px, main fills rest */
  grid-template-rows: 60px 1fr;           /* header 60px, content fills rest */
}
```

`1fr` means "one fraction of the available space." `repeat(3, 1fr)` creates three equal columns. Grid and Flexbox are complementary — Grid for the page macro-layout, Flexbox for items *within* each section.

---

## 12. Typography

```css
body {
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  font-size: 16px;       /* base size — rem units are relative to this */
  line-height: 1.5;      /* 1.5 × font-size = comfortable reading */
  color: #1a1a1a;
}

h1 { font-size: 2rem; font-weight: 700; }    /* 32px at base 16 */
h2 { font-size: 1.5rem; font-weight: 600; }
h3 { font-size: 1.25rem; font-weight: 600; }
p  { font-size: 1rem; }

/* Use rem (root-relative) not px for scalable type */
```

### Units you'll use

| Unit | Relative to | Use for |
|------|-------------|---------|
| `px` | screen pixels | borders, shadows, fine details |
| `rem` | root `<html>` font size | type, consistent spacing |
| `em` | parent element's font size | spacing that scales with local type |
| `%` | parent element's size | fluid widths |
| `vh` / `vw` | viewport height/width | full-screen sections |

---

## 13. Colors & Backgrounds

```css
.element {
  /* Color formats — all equivalent-ish: */
  color: #18453B;                     /* hex */
  color: rgb(24, 69, 59);             /* rgb */
  color: rgba(24, 69, 59, 0.5);       /* rgb + alpha (transparency) */
  color: hsl(163, 48%, 18%);          /* hue, saturation, lightness */

  background-color: #ffffff;
  background-image: url('pattern.svg');
  background-size: cover;             /* fill the element */
  background-position: center;
}
```

### Opacity vs rgba

```css
/* rgba changes color transparency only */
background: rgba(0, 0, 0, 0.5);   /* semi-transparent black */

/* opacity changes the WHOLE element and its children */
.overlay { opacity: 0.5; }   /* text inside also becomes 50% visible */
```

---

## 14. Pseudo-classes & Pseudo-elements

**Pseudo-classes** style elements in a particular *state*:

```css
.button:hover  { background: #0f3d33; }   /* mouse over */
.button:active { transform: scale(0.98); } /* being clicked */
.input:focus   { outline: 2px solid #18453B; } /* keyboard-focused */
.input:disabled { opacity: 0.5; cursor: not-allowed; }

/* structural */
li:first-child  { border-top: none; }    /* first item in a list */
li:last-child   { border-bottom: none; } /* last item */
li:nth-child(2) { background: #f5f5f5; } /* second item */
li:nth-child(even) { background: #fafafa; } /* every even row */
```

**Pseudo-elements** style a *virtual part* of an element:

```css
p::first-line  { font-weight: bold; }        /* just the first line */
.item::before  { content: '→ '; color: green; } /* inject content before */
.item::after   { content: ''; display: block; }  /* inject content after (common for clearfix tricks) */
input::placeholder { color: #999; font-style: italic; }
```

You'll use `:hover`, `:focus`, `:disabled`, and `::placeholder` constantly for your form and list styling.

---

## 15. Responsive Design & Media Queries

Media queries apply styles only when certain conditions are true — usually screen width. This lets one stylesheet handle all device sizes.

```css
/* Base styles (mobile-first — smallest screen) */
.container { padding: 16px; }

/* Tablet and up */
@media (min-width: 768px) {
  .container { padding: 24px; }
}

/* Desktop and up */
@media (min-width: 1024px) {
  .container { padding: 32px; max-width: 1200px; margin: 0 auto; }
}
```

**Mobile-first** (min-width) is the recommended approach: write the simplest layout for small screens, then *add* complexity as the screen gets larger. It's the natural way to progressively enhance.

For Media Tracker you'll likely want your sidebar to collapse or hide on smaller screens — this is where media queries earn their keep.

---

## 16. CSS Custom Properties

**CSS custom properties** (often called CSS variables) are the foundation of your MSU theme. Unlike SCSS variables (compile-time), these exist *in the browser at runtime* — which means they can change dynamically, making them perfect for light/dark mode.

```css
/* Define on :root — available everywhere */
:root {
  --color-primary: #18453B;          /* MSU Green */
  --color-background: #ffffff;
  --color-text: #1a1a1a;
  --color-sidebar-bg: #18453B;
  --color-sidebar-text: #ffffff;
  --color-border: #e5e7eb;
  --color-badge-backlog: #6b7280;
  --color-badge-progress: #18453B;
  --color-badge-done: #ffffff;
  --font-sans: 'Inter', system-ui, sans-serif;
  --radius: 8px;
  --shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

/* Use them with var() */
.button {
  background: var(--color-primary);
  border-radius: var(--radius);
  font-family: var(--font-sans);
}

/* Override for dark mode by changing values on :root */
.dark-mode {
  --color-background: #121212;
  --color-text: #ffffff;
  --color-sidebar-bg: #1a1a1a;
  --color-border: #374151;
}
```

Every component that uses `var(--color-background)` automatically switches when you toggle the dark mode class — no component-level changes needed. One switch, the whole app repaints.

> **Critical distinction:** SCSS `$variables` are replaced with their values at compile time — the browser never sees `$msu-green`, only `#18453B`. CSS `--custom-properties` exist at runtime and can be changed by JavaScript or by toggling classes. For anything that needs to switch dynamically (light/dark mode, themes), always use CSS custom properties. SCSS variables are for static values like breakpoints and computed values you reuse in the SCSS itself.

---

## 17. Transitions & Animations

Smooth state changes make UI feel polished.

```css
/* Transition: smoothly animate a property change */
.button {
  background: var(--color-primary);
  transition: background 200ms ease, transform 150ms ease;
}
.button:hover {
  background: #0f3d33;
  transform: translateY(-1px);   /* subtle lift */
}

/* Multiple properties */
.card {
  transition: box-shadow 200ms ease, transform 200ms ease;
}
.card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
}
```

`transition` syntax: `property duration timing-function delay`

Common timing functions:
- `ease` — starts fast, slows down (natural)
- `linear` — constant speed
- `ease-in-out` — slow start and end
- `cubic-bezier(...)` — custom curve

For your status badges hover states and the add/delete button interactions, a simple `200ms ease` transition on `background` and `transform` is all you need to feel noticeably polished.

---

# Part 3 — SCSS

## 18. What SCSS Adds

SCSS (Sassy CSS) is a **preprocessor** — you write `.scss` files, and a compiler converts them to regular `.css` that browsers understand. Angular does this compilation automatically; you just write SCSS and the build handles the rest.

SCSS is a strict superset of CSS: every valid CSS file is valid SCSS. You can adopt it gradually — start with plain CSS syntax and add SCSS features where they help.

The additions you'll actually use:
- **Variables** — reusable values (for static, build-time values)
- **Nesting** — mirror your HTML structure in CSS
- **Partials & @use** — split into multiple files and import them
- **Mixins** — reusable blocks of CSS with optional arguments

---

## 19. SCSS Variables

```scss
// _variables.scss
$msu-green: #18453B;
$msu-black: #121212;
$msu-dark-green: #0f3d33;
$font-sans: 'Inter', system-ui, sans-serif;
$radius-sm: 4px;
$radius-md: 8px;
$radius-lg: 12px;
$transition-speed: 200ms;
```

Use them anywhere:

```scss
.button {
  background: $msu-green;
  border-radius: $radius-md;
  transition: background $transition-speed ease;
}
```

SCSS variables are great for values you use *within your SCSS code itself* — spacing scales, breakpoints, values used in calculations. For anything that changes at runtime, use CSS custom properties instead (see Chapter 16 and 24).

---

## 20. Nesting

Nesting lets you write selectors inside other selectors, mirroring your HTML structure and reducing repetition.

```scss
// Without nesting — repetitive
.contact-card { background: white; }
.contact-card .name { font-weight: 600; }
.contact-card .name:hover { color: $msu-green; }
.contact-card .badge { border-radius: 4px; }

// With nesting — clean and structured
.contact-card {
  background: white;

  .name {
    font-weight: 600;

    &:hover {                     // & = the parent selector (.name:hover)
      color: $msu-green;
    }
  }

  .badge {
    border-radius: 4px;
  }
}
```

The **`&` (ampersand)** refers to the current selector in the nesting chain. It lets you:

```scss
.button {
  background: $msu-green;

  &:hover  { background: $msu-dark-green; }  // .button:hover
  &:active { transform: scale(0.98); }         // .button:active
  &.large  { padding: 16px 32px; }            // .button.large
  &--primary { font-weight: 600; }            // .button--primary (BEM)
}
```

> **Don't over-nest.** Nesting more than 3 levels deep produces overly-specific CSS that's hard to override. If you find yourself 4 levels deep, flatten it. A good rule: nest to reflect the DOM structure, not to replicate the entire tree.

---

## 21. Partials & @use

A **partial** is an SCSS file prefixed with `_` (underscore) that's meant to be imported, not compiled on its own. Splitting your styles into partials keeps things organised:

```
src/
├── styles.scss              ← global entry point
└── styles/
    ├── _variables.scss      ← your MSU colours & constants
    ├── _reset.scss          ← base reset / box-sizing
    ├── _typography.scss     ← font stack, sizes
    └── _utilities.scss      ← helper classes
```

Import them with `@use`:

```scss
// styles.scss
@use 'styles/variables' as v;
@use 'styles/reset';
@use 'styles/typography';
@use 'styles/utilities';

body {
  background: v.$msu-green;   // access variables via the namespace
}
```

> **`@use` vs `@import`:** Old SCSS used `@import`, which is now deprecated. Use `@use` instead — it properly scopes what it imports and prevents duplicate compilation.

---

## 22. Mixins

A **mixin** is a reusable block of CSS you can include anywhere, optionally with arguments:

```scss
// Define a mixin
@mixin flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
}

@mixin badge($bg-color, $text-color) {
  background: $bg-color;
  color: $text-color;
  border-radius: 99px;         // pill shape
  padding: 2px 10px;
  font-size: 0.75rem;
  font-weight: 500;
}

// Use them with @include
.loading-spinner { @include flex-center; }

.badge-backlog   { @include badge(#e5e7eb, #374151); }
.badge-progress  { @include badge(#18453B, #ffffff); }
.badge-completed { @include badge(#ffffff, #18453B); }
```

Mixins are the SCSS answer to "I keep copy-pasting these five lines." Your status badges in Media Tracker are a perfect candidate — same shape, different colours.

---

## 23. Functions & @each

### SCSS built-in functions

```scss
@use 'sass:color';

.button-hover {
  // darken/lighten a colour by a percentage
  background: color.adjust($msu-green, $lightness: -10%);
}
```

### @each — loop over a map

```scss
$badge-styles: (
  'backlog':   (#e5e7eb, #374151),
  'progress':  (#18453B, #ffffff),
  'completed': (#d1fae5, #065f46),
);

@each $status, $colors in $badge-styles {
  .badge-#{$status} {               // #{} = string interpolation in SCSS
    @include badge(
      nth($colors, 1),             // first value: background
      nth($colors, 2)              // second value: text
    );
  }
}
```

This generates `.badge-backlog`, `.badge-progress`, and `.badge-completed` from a single loop — far better than three copy-pasted blocks.

---

## 24. SCSS vs CSS Custom Properties

This is the most practically important question for your SCRUM-11 work. Here's the definitive guide:

| | SCSS `$variables` | CSS `--custom-properties` |
|--|---|---|
| **When resolved** | Compile time | Runtime (in browser) |
| **Can JS change them?** | No (baked in) | Yes |
| **Can they toggle with a class?** | No | Yes |
| **Good for** | Breakpoints, values used in calculations, repeated static values in SCSS | Theming, light/dark mode, anything that changes at runtime |
| **Browser sees them?** | No — replaced with values | Yes |

```scss
// ✅ SCSS variable — correct for a static breakpoint used in SCSS
$breakpoint-md: 768px;
@media (min-width: $breakpoint-md) { ... }

// ✅ CSS custom property — correct for a theme colour that flips with dark mode
:root { --color-background: #ffffff; }
.dark-mode { --color-background: #121212; }
body { background: var(--color-background); }  // changes when class toggles

// ❌ SCSS variable for a theme colour — won't work for dark mode
$bg: #ffffff;
body { background: $bg; }   // baked in at build time, can't change later
```

**Rule of thumb:** does it need to change while the page is running? CSS custom property. Is it a value you calculate or reuse inside SCSS itself (breakpoints, spacing scale)? SCSS variable. For your MSU theme, colour values go in CSS custom properties; your breakpoints and static SCSS utilities use `$variables`.

---

# Part 4 — Angular + Styling

## 25. Component Styles & View Encapsulation

Angular components are styled by their own `.scss` file. By default, styles in `contact-list.component.scss` are **scoped** to that component only — they won't leak out and affect other components, even if they use the same class names.

```scss
// contact-list.component.scss
.card { border: 1px solid var(--color-border); }
// This .card rule ONLY applies inside ContactListComponent's template.
// A .card in a different component is unaffected.
```

This is **View Encapsulation**. Angular achieves it by adding a unique attribute to every element in the component and making the CSS selector more specific:

```css
/* What you wrote: */
.card { border: 1px solid #e5e7eb; }

/* What Angular compiles to (roughly): */
.card[_ngcontent-xyz-c123] { border: 1px solid #e5e7eb; }
/* The unique attribute ensures it only matches elements in THIS component */
```

You usually don't need to think about this — just know that component SCSS files are private to their component by design. If you're styling something and it won't take effect, check whether you're in the right component's SCSS file.

### Encapsulation modes

```typescript
@Component({
  encapsulation: ViewEncapsulation.Emulated,  // default — scoped
  encapsulation: ViewEncapsulation.None,       // no scoping — global CSS
  encapsulation: ViewEncapsulation.ShadowDom,  // real Shadow DOM
})
```

You'll almost always want the default. `ViewEncapsulation.None` is sometimes useful if you're styling third-party components from inside your own — but use it sparingly, it makes styles global and can cause unexpected overrides.

---

## 26. Global vs Component-Scoped Styles

You have two places to put styles in an Angular app:

### `styles.scss` — global, applies everywhere

```scss
// src/styles.scss — good for:
// 1. CSS custom properties (theme variables)
// 2. Reset / base styles
// 3. Typography
// 4. Utility classes used across components

:root {
  --color-primary: #18453B;
  --color-background: #ffffff;
  --color-text: #1a1a1a;
}

*, *::before, *::after { box-sizing: border-box; }

body {
  margin: 0;
  font-family: var(--font-sans);
  background: var(--color-background);
  color: var(--color-text);
}
```

### `component.scss` — scoped, applies to one component

```scss
// contact-list.component.scss — good for:
// 1. Layout of this specific component
// 2. Styles for elements in this component's template
// 3. State-specific styles (.selected, .loading, etc.)

.contact-list {
  padding: 24px;

  .list-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }

  .empty-state {
    text-align: center;
    color: #6b7280;
    padding: 48px 0;
  }
}
```

Think of it as: **global = the design system** (tokens, resets, base), **component = the specific layout and states of that one piece of UI**. If you find yourself writing the same SCSS in two component files, it probably belongs in a global partial.

---

## 27. Class & Style Binding

Angular lets you dynamically add/remove CSS classes and inline styles based on component data — this is how your status badges get their colour.

### `[class]` binding — replace all classes

```html
<div [class]="currentClasses">Row</div>
```

### `[class.name]` binding — toggle a single class

```html
<!-- Adds 'selected' class when isSelected is true, removes it when false -->
<div class="row" [class.selected]="isSelected">Row</div>

<!-- You can stack these -->
<div class="badge"
     [class.badge-backlog]="status === 'Backlog'"
     [class.badge-progress]="status === 'InProgress'"
     [class.badge-completed]="status === 'Completed'">
  {{ status }}
</div>
```

### `[ngClass]` — multiple classes at once

```html
<div [ngClass]="{
  'badge': true,
  'badge-backlog': status === 'Backlog',
  'badge-progress': status === 'InProgress',
  'badge-completed': status === 'Completed'
}">{{ status }}</div>
```

### `[style.property]` binding — a single style

```html
<div [style.color]="isActive ? '#18453B' : '#6b7280'">Text</div>
<div [style.width.px]="progressPercent">Bar</div>  <!-- .px unit suffix -->
```

### `[ngStyle]` — multiple styles at once

```html
<div [ngStyle]="{ 'background-color': badgeColor, 'color': badgeTextColor }"></div>
```

> **Prefer class binding over style binding.** Keep visual rules in CSS (or SCSS); let the component just toggle classes. `[class.badge-completed]="..."` is cleaner than `[ngStyle]="{ background: '#d1fae5' }"` because the color lives in CSS where it belongs. Hard-coding colors in component TypeScript is the equivalent of mixing your layers.

---

## 28. Light/Dark Mode Toggle

Here's the complete MSU theme toggle implementation pattern. The idea: `--custom-properties` are defined on `:root` for light mode; a `.dark-mode` class on `<body>` overrides them for dark mode. One class toggle on the body, everything repaints.

### The SCSS setup (`styles.scss`)

```scss
:root {
  // Light mode (default)
  --color-primary: #18453B;             // MSU Green
  --color-background: #ffffff;
  --color-surface: #f9fafb;             // slightly off-white for cards
  --color-text-primary: #1a1a1a;
  --color-text-secondary: #6b7280;
  --color-border: #e5e7eb;
  --color-sidebar-bg: #18453B;          // green sidebar
  --color-sidebar-text: #ffffff;
  --color-sidebar-hover: #0f3d33;
}

body.dark-mode {
  --color-background: #121212;
  --color-surface: #1e1e1e;
  --color-text-primary: #ffffff;
  --color-text-secondary: #9ca3af;
  --color-border: #374151;
  --color-sidebar-bg: #1a1a1a;          // near-black sidebar
  --color-sidebar-text: #ffffff;
  --color-sidebar-hover: #18453B;       // green hover on dark sidebar
}
```

### The Angular component toggle

```typescript
// theme.service.ts
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private isDark = false;

  toggleTheme(): void {
    this.isDark = !this.isDark;
    document.body.classList.toggle('dark-mode', this.isDark);
  }

  get currentTheme(): string {
    return this.isDark ? 'dark' : 'light';
  }
}
```

```html
<!-- In your header/nav component template -->
<button (click)="themeService.toggleTheme()" class="theme-toggle">
  {{ themeService.currentTheme === 'dark' ? '☀️' : '🌙' }}
</button>
```

Every component that uses `var(--color-background)` in its SCSS automatically switches. No component knows or cares about the current theme — they just use the variables. This is the correct separation of concerns: the theme is global state, the variables are the API.

---

## 29. Putting It Together: A Styled Contact List

A full example in the Contacts domain — the shape of what you'll build in Sprint 3. Study the HTML structure, the SCSS nesting, and the Angular bindings. Then close this file and go build your Media Tracker version from scratch.

### The component class

```typescript
@Component({
  selector: 'app-contact-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contact-list.component.html',
  styleUrl: './contact-list.component.scss'
})
export class ContactListComponent implements OnInit {
  contacts: Contact[] = [];
  isLoading = true;
  activeFilter: string = 'all';

  constructor(private contactService: ContactService) {}

  ngOnInit(): void {
    this.contactService.getAll().subscribe({
      next: (data) => {
        this.contacts = data;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  setFilter(filter: string): void {
    this.activeFilter = filter;
  }

  deleteContact(id: number): void {
    if (confirm('Are you sure?')) {
      this.contactService.delete(id).subscribe(() => {
        this.contacts = this.contacts.filter(c => c.id !== id);
      });
    }
  }
}
```

### The template

```html
<div class="contact-list">

  <!-- Header row -->
  <div class="list-header">
    <h2>Contacts</h2>
    <button class="btn btn-primary">+ Add Contact</button>
  </div>

  <!-- Filter tabs -->
  <div class="filter-tabs">
    <button class="tab" [class.active]="activeFilter === 'all'"
            (click)="setFilter('all')">All</button>
    <button class="tab" [class.active]="activeFilter === 'active'"
            (click)="setFilter('active')">Active</button>
    <button class="tab" [class.active]="activeFilter === 'archived'"
            (click)="setFilter('archived')">Archived</button>
  </div>

  <!-- Loading state -->
  @if (isLoading) {
    <div class="loading">Loading contacts...</div>
  }

  <!-- List -->
  @if (!isLoading) {
    @for (contact of contacts; track contact.id) {
      <div class="contact-row">
        <div class="contact-info">
          <span class="contact-name">{{ contact.fullName }}</span>
          <span class="contact-email">{{ contact.email }}</span>
        </div>

        <span class="badge"
              [class.badge-active]="contact.status === 'Active'"
              [class.badge-archived]="contact.status === 'Archived'">
          {{ contact.status }}
        </span>

        <div class="row-actions">
          <button class="btn btn-ghost" (click)="editContact(contact)">Edit</button>
          <button class="btn btn-danger" (click)="deleteContact(contact.id)">Delete</button>
        </div>
      </div>
    } @empty {
      <div class="empty-state">
        <p>No contacts yet. Add your first one.</p>
      </div>
    }
  }

</div>
```

### The SCSS

```scss
// contact-list.component.scss

.contact-list {
  padding: 24px;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;

  h2 {
    font-size: 1.5rem;
    font-weight: 600;
    color: var(--color-text-primary);
    margin: 0;
  }
}

.filter-tabs {
  display: flex;
  gap: 4px;
  border-bottom: 1px solid var(--color-border);
  margin-bottom: 16px;

  .tab {
    padding: 8px 16px;
    border: none;
    background: none;
    cursor: pointer;
    color: var(--color-text-secondary);
    border-bottom: 2px solid transparent;
    transition: color 200ms ease, border-color 200ms ease;

    &:hover { color: var(--color-primary); }

    &.active {
      color: var(--color-primary);
      border-bottom-color: var(--color-primary);
      font-weight: 500;
    }
  }
}

.contact-row {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px 0;
  border-bottom: 1px solid var(--color-border);
  transition: background 150ms ease;

  &:hover { background: var(--color-surface); }
  &:last-child { border-bottom: none; }
}

.contact-info {
  display: flex;
  flex-direction: column;
  flex: 1;

  .contact-name { font-weight: 500; color: var(--color-text-primary); }
  .contact-email { font-size: 0.875rem; color: var(--color-text-secondary); }
}

// Status badge
.badge {
  padding: 2px 10px;
  border-radius: 99px;
  font-size: 0.75rem;
  font-weight: 500;

  &.badge-active   { background: #d1fae5; color: #065f46; }
  &.badge-archived { background: #e5e7eb; color: #374151; }
}

.row-actions {
  display: flex;
  gap: 8px;
  opacity: 0;
  transition: opacity 150ms ease;

  .contact-row:hover & { opacity: 1; }   // show on row hover
}

.empty-state {
  text-align: center;
  padding: 48px 0;
  color: var(--color-text-secondary);
}

// Buttons (put shared ones in global styles.scss instead)
.btn {
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: background 200ms ease;

  &.btn-primary {
    background: var(--color-primary);
    color: #fff;
    &:hover { background: #0f3d33; }
  }

  &.btn-ghost {
    background: transparent;
    color: var(--color-text-secondary);
    &:hover { background: var(--color-surface); }
  }

  &.btn-danger {
    background: transparent;
    color: #ef4444;
    &:hover { background: #fee2e2; }
  }
}

.loading {
  text-align: center;
  padding: 48px 0;
  color: var(--color-text-secondary);
}
```

Study the pattern:
- All colours come from CSS custom properties — they switch automatically with dark mode.
- SCSS nesting mirrors the template structure so you always know which HTML block a SCSS rule targets.
- Angular class binding (`[class.active]`, `[class.badge-active]`) drives visual state from the component.
- Row action buttons use a parent hover (`contact-row:hover &`) trick to hide until hovered.

Now close this guide and build your `MediaItem` version.

---

## 30. Resources

**HTML**
- [MDN HTML — Learn Web Development](https://developer.mozilla.org/en-US/docs/Learn/HTML)
- [MDN HTML elements reference](https://developer.mozilla.org/en-US/docs/Web/HTML/Element)
- [MDN forms and inputs](https://developer.mozilla.org/en-US/docs/Learn/Forms)

**CSS**
- [MDN CSS — Learn Web Development](https://developer.mozilla.org/en-US/docs/Learn/CSS)
- [MDN Flexbox guide](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_flexible_box_layout)
- [MDN CSS Grid guide](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_grid_layout)
- [MDN CSS custom properties](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
- [CSS Tricks — A Complete Guide to Flexbox](https://css-tricks.com/snippets/css/a-guide-to-flexbox/)
- [CSS Tricks — A Complete Guide to Grid](https://css-tricks.com/snippets/css/complete-guide-grid/)

**SCSS**
- [SCSS official guide](https://sass-lang.com/guide/)
- [SCSS documentation](https://sass-lang.com/documentation/)

**Angular Styling**
- [Angular component styling](https://angular.dev/guide/components/styling)
- [Angular class and style binding](https://angular.dev/guide/templates/class-binding)
- [View encapsulation](https://angular.dev/guide/components/styling#style-scoping)

---

*Keep this in `/docs` with the rest. Part 2 (CSS) is the one to reread when you're laying out the sidebar and main area. Part 3 (SCSS) becomes relevant when you reach SCRUM-11 for the global variables. Part 4 (Angular + Styling) is your Sprint 3 companion — open it next to your component files as you build.*
