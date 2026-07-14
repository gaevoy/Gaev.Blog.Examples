import { describe, it, expect } from 'vitest';
import {
  upcast,
  upcastToV2,
  upcastToV3,
  upcastStore,
  type GridViewV1,
  type GridViewV2,
  type GridViewV3,
  type AnyGridView,
} from '../src/schema';

// Each upcaster is a pure function from one shape to the next.
describe('one hop per version', () => {
  it('V1 → V2 renames columns and defaults an empty sort', () => {
    const v1: GridViewV1 = { schemaVer: 1, columns: ['id', 'total'] };
    expect(upcastToV2(v1)).toEqual<GridViewV2>({
      schemaVer: 2,
      visibleColumns: ['id', 'total'],
      sort: '',
    });
  });

  it('V2 → V3 nests display under layout and starts filters empty', () => {
    const v2: GridViewV2 = { schemaVer: 2, visibleColumns: ['id', 'name'], sort: 'name asc' };
    expect(upcastToV3(v2)).toEqual<GridViewV3>({
      schemaVer: 3,
      layout: { visibleColumns: ['id', 'name'], sort: 'name asc' },
      filters: '',
    });
  });
});

describe('chain to the latest', () => {
  const latest: GridViewV3 = {
    schemaVer: 3,
    layout: { visibleColumns: ['x'], sort: 'x desc' },
    filters: 'x > 1',
  };

  it('walks a V1 blob through both hops', () => {
    const v1: GridViewV1 = { schemaVer: 1, columns: ['id'] };
    expect(upcast(v1)).toEqual<GridViewV3>({
      schemaVer: 3,
      layout: { visibleColumns: ['id'], sort: '' },
      filters: '',
    });
  });

  it('walks a V2 blob through one hop', () => {
    const v2: GridViewV2 = { schemaVer: 2, visibleColumns: ['id'], sort: 'id asc' };
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
      a: { schemaVer: 1, columns: ['id'] },
      b: { schemaVer: 2, visibleColumns: ['id'], sort: 'id asc' },
      c: { schemaVer: 3, layout: { visibleColumns: [], sort: '' }, filters: '' },
    };
    const upcasted = upcastStore(store);
    expect(Object.keys(upcasted)).toEqual(['a', 'b', 'c']);
    expect(Object.values(upcasted).every((v) => v.schemaVer === 3)).toBe(true);
  });
});
