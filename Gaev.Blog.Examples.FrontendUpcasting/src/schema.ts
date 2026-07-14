// Plain-TypeScript upcasting: evolve a frontend-persisted schema on *read*.
//
// The running example is a **saved data-grid view** — which columns a user
// picked, how they sorted, and any filters they saved on a table. It's
// persisted through the API as an opaque JSON string; only this grid component
// ever reads it back. That single owner is exactly the condition upcasting-on-
// read wants (see README for why this beats "user data" as an example).

// ── V1: the first release ────────────────────────────────────────────────
// Just the visible columns, in order.
export type GridViewV1 = {
  schemaVer: 1;
  columns: string[];
};

// ── V2: renamed the field, added a saved sort ────────────────────────────
// `columns` became `visibleColumns`, and a `sort` string joined it.
export type GridViewV2 = {
  schemaVer: 2;
  visibleColumns: string[];
  sort: string;
};

// ── V3: display grouped under `layout`, filters added ────────────────────
// `visibleColumns` + `sort` moved under `layout`, and a `filters` string
// arrived with no equivalent in older views.
export type GridViewV3 = {
  schemaVer: 3;
  layout: {
    visibleColumns: string[];
    sort: string;
  };
  filters: string;
};

// The store holds a mix of all three at once — a healthy state, not a bug.
export type AnyGridView = GridViewV1 | GridViewV2 | GridViewV3;

// ── One small step per version ───────────────────────────────────────────
// Each takes exactly one version and returns the next. Small, pure, boring.

export function upcastToV2(data: GridViewV1): GridViewV2 {
  return {
    schemaVer: 2,
    visibleColumns: data.columns,
    sort: '',
  };
}

export function upcastToV3(data: GridViewV2): GridViewV3 {
  return {
    schemaVer: 3,
    layout: {
      visibleColumns: data.visibleColumns,
      sort: data.sort,
    },
    // New capability — nothing in older views maps to it, so it starts empty.
    filters: '',
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
      // add a GridViewV4 without an upcastToV4 hop and `data` no longer narrows
      // to `never` here, so this line stops compiling — a TS error, not a
      // production surprise
      const _exhaustive: never = data;
      throw new Error(
        `Unknown grid view schema version: ${(_exhaustive as AnyGridView & { schemaVer: number }).schemaVer}`,
      );
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
