import { describe, it, expect } from 'vitest';
import {
  upcast,
  upcastToV2,
  upcastToV3,
  upcastStore,
  DEFAULT_COLUMN_WIDTH,
  type GridViewV1,
  type GridViewV2,
  type GridViewV3,
  type AnyGridView,
} from '../src/schema';

// Each upcaster is a pure function from one shape to the next — ideal for a
// small table of fixtures.
describe('one hop per version', () => {
  it('V1 → V2 splits the loose sort string into a structured spec', () => {
    const v1: GridViewV1 = { schemaVer: 1, columns: ['id', 'total'], sort: 'total desc' };
    expect(upcastToV2(v1)).toEqual<GridViewV2>({
      schemaVer: 2,
      columns: ['id', 'total'],
      sort: { field: 'total', direction: 'desc' },
    });
  });

  it.each([
    ['name asc', { field: 'name', direction: 'asc' }],
    ['name desc', { field: 'name', direction: 'desc' }],
    ['name', { field: 'name', direction: 'asc' }], // missing direction → asc
    ['  createdAt   desc  ', { field: 'createdAt', direction: 'desc' }], // sloppy whitespace
    ['name sideways', { field: 'name', direction: 'asc' }], // unknown direction → asc
  ])('V1 → V2 parses sort %j', (sort, expected) => {
    const v1: GridViewV1 = { schemaVer: 1, columns: ['name'], sort };
    expect(upcastToV2(v1).sort).toEqual(expected);
  });

  it('V2 → V3 gives each column a default width and starts filters empty', () => {
    const v2: GridViewV2 = {
      schemaVer: 2,
      columns: ['id', 'name'],
      sort: { field: 'id', direction: 'asc' },
    };
    expect(upcastToV3(v2)).toEqual<GridViewV3>({
      schemaVer: 3,
      layout: {
        columns: [
          { id: 'id', width: DEFAULT_COLUMN_WIDTH },
          { id: 'name', width: DEFAULT_COLUMN_WIDTH },
        ],
        sort: { field: 'id', direction: 'asc' },
      },
      filters: [],
    });
  });
});

describe('chain to the latest', () => {
  const latest: GridViewV3 = {
    schemaVer: 3,
    layout: { columns: [{ id: 'x', width: 200 }], sort: { field: 'x', direction: 'asc' } },
    filters: [{ field: 'x', op: 'gt', value: '1' }],
  };

  it('walks a V1 blob through both hops', () => {
    const v1: GridViewV1 = { schemaVer: 1, columns: ['id'], sort: 'id asc' };
    const out = upcast(v1);
    expect(out.schemaVer).toBe(3);
    expect(out.layout.columns).toEqual([{ id: 'id', width: DEFAULT_COLUMN_WIDTH }]);
    expect(out.layout.sort).toEqual({ field: 'id', direction: 'asc' });
    expect(out.filters).toEqual([]);
  });

  it('walks a V2 blob through one hop', () => {
    const v2: GridViewV2 = { schemaVer: 2, columns: ['id'], sort: { field: 'id', direction: 'desc' } };
    expect(upcast(v2).schemaVer).toBe(3);
  });

  it('leaves a V3 blob untouched — already home', () => {
    expect(upcast(latest)).toBe(latest); // same reference; the chain falls through
    expect(upcast(upcast(latest))).toEqual(latest); // and it's idempotent
  });

  it('throws on an unknown version — the blob really comes from untyped storage', () => {
    const rogue = { schemaVer: 4, whatever: true } as unknown as AnyGridView;
    expect(() => upcast(rogue)).toThrow(/Unknown grid view schema version/);
  });
});

describe('upcast at the boundary', () => {
  it('upcasts a store holding a mix of versions to all-V3', () => {
    const store: Record<string, AnyGridView> = {
      a: { schemaVer: 1, columns: ['id'], sort: 'id asc' },
      b: { schemaVer: 2, columns: ['id'], sort: { field: 'id', direction: 'asc' } },
      c: {
        schemaVer: 3,
        layout: { columns: [], sort: { field: 'id', direction: 'asc' } },
        filters: [],
      },
    };
    const upcasted = upcastStore(store);
    expect(Object.keys(upcasted)).toEqual(['a', 'b', 'c']);
    expect(Object.values(upcasted).every((v) => v.schemaVer === 3)).toBe(true);
  });
});
