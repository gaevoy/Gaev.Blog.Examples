# Frontend schema evolution with upcasting

Companion code for the blog post **"Evolving a Frontend Schema with Upcasting"**.

The idea, borrowed from backend event sourcing: don't migrate the stored data.
Leave every blob exactly as it was written and teach the app to transform any
old shape into the current one *as it reads* — **upcasting**. The store becomes
a museum of old versions; the rest of the app only ever sees the latest schema.

This repo demonstrates the pattern two ways over the **same three schema
versions**:

- [`src/schema.ts`](src/schema.ts) — **plain TypeScript.** A discriminated union
  tags every version, one pure upcaster per hop, and a `upcast()` that chains
  any version up to the latest with an exhaustiveness check.
- [`src/schema-zod.ts`](src/schema-zod.ts) — **zod.** The same upcast logic,
  but zod *parses* the untyped blob at the boundary and rejects anything
  malformed. Plain TS trusts the input type at compile time; zod verifies it at
  runtime. "Parse, don't guess."
- [`src/schema-zustand.ts`](src/schema-zustand.ts) — **zustand.** The same
  `upcast()` again, handed to the `persist` middleware's `migrate`. The library
  owns the plumbing — the version check, the trigger, and the write-back that
  makes the upgrade stick — so old blobs migrate themselves one read at a time.
  Uses `zustand/vanilla`, so it's still pure TypeScript.

## Why a saved data-grid view, not "user data"?

The post's original example evolves a **user profile** (`name`/`surname` →
`fullName` → `bio.name`). It's easy to follow, but it's a slightly awkward fit
for upcasting-on-read, and the post says so itself: the technique is cleanest
when **exactly one client owns the blob**, and it breaks down "the moment more
than one independent consumer touches the raw stored data." User profiles are
the *worst* case for that — the backend, reporting, and other services read
them all the time.

So this demo uses a **saved data-grid view** instead — the columns a user
picked on a table, how they sorted it, and any filters they saved:

- **Genuinely frontend-owned.** The backend stores it as an opaque string;
  nobody else parses it. That's the single-owner condition the pattern wants.
- **Evolves for real.** Every SaaS table grows features, so its saved-view
  schema keeps changing — the exact pressure the post is about.
- **Exercises all three kinds of change** in one story: retype a field
  (`sort` string → structured), restructure/nest (`columns` → `layout.columns`
  with widths), and add a brand-new field with a default (`filters`).

## The three versions

| Version | Shape | What changed |
|---|---|---|
| **V1** | `{ columns: string[]; sort: string }` | first release; sort crammed into `"createdAt desc"` |
| **V2** | `sort: { field, direction }` | sort became structured (a **type change**) |
| **V3** | `{ layout: { columns: {id,width}[], sort }, filters: [] }` | widths + filters added, display nested under `layout` (**restructure + new field**) |

```
V1 --upcastToV2--> V2 --upcastToV3--> V3 --> app only ever sees V3
```

## Run it

```bash
npm install
npm test         # vitest — both the plain-TS and zod suites
npm run typecheck # tsc --noEmit, proves the exhaustiveness check compiles
npm run demo     # prints the boundary upcast over a mixed-version store
```

## Tests

- [`test/upcast.test.ts`](test/upcast.test.ts) — each hop in isolation (with a
  fixture table for the fiddly `sort`-string parsing), the full chain from every
  version, idempotency on V3, and the boundary upcast over a mixed store.
- [`test/schema-zod.test.ts`](test/schema-zod.test.ts) — parse+upcast for every
  version, plus the cases plain TS *can't* catch: unknown versions, bad enum
  values, and structurally broken blobs all rejected at the boundary.
- [`test/schema-zustand.test.ts`](test/schema-zustand.test.ts) — persist runs
  `upcast()` as its `migrate`, and the "saving makes the upgrade stick"
  behaviour: a migrated V1 gets written back to storage as a V3 envelope, an
  already-current V3 is left untouched, and ordinary edits persist as V3.
