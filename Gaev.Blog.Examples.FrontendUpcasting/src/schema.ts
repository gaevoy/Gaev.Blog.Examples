// Plain-TypeScript upcasting: evolve a frontend-persisted schema on *read*.
//
// The running example is a **saved data-grid view** — which columns a user
// picked, how they sorted, and any filters they saved on a table. It's
// persisted through the API as an opaque JSON string; only this grid component
// ever reads it back. That single owner is exactly the condition upcasting-on-
// read wants (see README for why this beats "user data" as an example).

export type SortDirection = 'asc' | 'desc';

export type SortSpec = {
  field: string;
  direction: SortDirection;
};

export type ColumnSpec = {
  id: string;
  width: number;
};

export type Filter = {
  field: string;
  op: 'eq' | 'contains' | 'gt' | 'lt';
  value: string;
};

// ── V1: the first release ────────────────────────────────────────────────
// Visible columns, in order, plus one sort column crammed into a loose string
// like "createdAt desc".
export type GridViewV1 = {
  schemaVer: 1;
  columns: string[];
  sort: string;
};

// ── V2: sorting became structured ────────────────────────────────────────
// Splitting "createdAt desc" by hand in every reader was fragile, so the field
// and direction moved into their own shape. (A type change on `sort`.)
export type GridViewV2 = {
  schemaVer: 2;
  columns: string[];
  sort: SortSpec;
};

// ── V3: widths + filters, display grouped under `layout` ─────────────────
// Columns grew from bare ids into {id, width}, saved filters arrived, and the
// display bits were tucked under `layout`. (A restructure *and* a brand-new
// field with no equivalent in older views.)
export type GridViewV3 = {
  schemaVer: 3;
  layout: {
    columns: ColumnSpec[];
    sort: SortSpec;
  };
  filters: Filter[];
};

// The store holds a mix of all three at once — a healthy state, not a bug.
export type AnyGridView = GridViewV1 | GridViewV2 | GridViewV3;

// ── One small step per version ───────────────────────────────────────────
// Each takes exactly one version and returns the next. Small, pure, boring.

export const DEFAULT_COLUMN_WIDTH = 120;

export function upcastToV2(data: GridViewV1): GridViewV2 {
  const [field = '', rawDirection] = data.sort.trim().split(/\s+/);
  return {
    schemaVer: 2,
    columns: data.columns,
    sort: { field, direction: rawDirection === 'desc' ? 'desc' : 'asc' },
  };
}

export function upcastToV3(data: GridViewV2): GridViewV3 {
  return {
    schemaVer: 3,
    layout: {
      columns: data.columns.map((id) => ({ id, width: DEFAULT_COLUMN_WIDTH })),
      sort: data.sort,
    },
    // New capability — nothing in older views maps to it, so it starts empty.
    filters: [],
  };
}

// ── Chain any version up to the latest ───────────────────────────────────
// A V1 blob runs through both hops; a V2 through one; a V3 is already home.

export function upcast(data: AnyGridView): GridViewV3 {
  switch (data.schemaVer) {
    case 1:
      return upcastToV3(upcastToV2(data));
    case 2:
      return upcastToV3(data);
    case 3:
      return data;
    default: {
      throw new Error(`Unknown grid view schema version: ${JSON.stringify(data)}`);
    }
  }
}

// ── Upcast at the boundary ───────────────────────────────────────────────
// Do it once, where API data enters the app. Everything downstream is V3 and
// nothing has to know the other versions ever existed.
export function upcastStore(
  store: Record<string, AnyGridView>,
): Record<string, GridViewV3> {
  return Object.fromEntries(
    Object.entries(store).map(([key, view]) => [key, upcast(view)]),
  );
}
