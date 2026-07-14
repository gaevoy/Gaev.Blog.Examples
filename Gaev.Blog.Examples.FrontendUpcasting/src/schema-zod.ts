// The same upcasting, with zod doing the parsing at the boundary.
//
// Plain TS (schema.ts) *trusts* that the blob already matches `AnyGridView` —
// but that trust is compile-time only and evaporates at runtime. A blob read
// off the wire is really `unknown`. zod closes that gap: it parses the untyped
// blob and rejects anything malformed, so what reaches `upcast()` is a real,
// known version. "Parse, don't guess."
//
// Note the upcast *logic* is unchanged — we reuse the very same `upcast()` from
// schema.ts. zod only adds the runtime gate in front of it.

import { z } from 'zod';
import { upcast, type GridViewV3 } from './schema';

const sortSpecSchema = z.object({
  field: z.string(),
  direction: z.enum(['asc', 'desc']),
});

export const gridViewV1Schema = z.object({
  schemaVer: z.literal(1),
  columns: z.array(z.string()),
  sort: z.string(),
});

export const gridViewV2Schema = z.object({
  schemaVer: z.literal(2),
  columns: z.array(z.string()),
  sort: sortSpecSchema,
});

export const gridViewV3Schema = z.object({
  schemaVer: z.literal(3),
  layout: z.object({
    columns: z.array(z.object({ id: z.string(), width: z.number() })),
    sort: sortSpecSchema,
  }),
  filters: z.array(
    z.object({
      field: z.string(),
      op: z.enum(['eq', 'contains', 'gt', 'lt']),
      value: z.string(),
    }),
  ),
});

// A discriminated union keeps the versions honest — zod picks the right member
// by the `schemaVer` tag and errors if the blob is none of them.
export const anyGridViewSchema = z.discriminatedUnion('schemaVer', [
  gridViewV1Schema,
  gridViewV2Schema,
  gridViewV3Schema,
]);

// Parse the untyped blob into a known version, then upcast to the latest.
// Throws a ZodError if the blob doesn't match any version.
export function parseAndUpcast(raw: unknown): GridViewV3 {
  return upcast(anyGridViewSchema.parse(raw));
}

// Non-throwing variant — for when a single bad blob shouldn't crash the read.
export type ParseResult =
  | {
      ok: true;
      view: GridViewV3;
    }
  | {
      ok: false;
      error: z.ZodError;
    };

export function safeParseAndUpcast(raw: unknown): ParseResult {
  const result = anyGridViewSchema.safeParse(raw);
  return result.success
    ? { ok: true, view: upcast(result.data) }
    : { ok: false, error: result.error };
}
