import { describe, it, expect } from 'vitest';
import {
  parseAndUpcast,
  safeParseAndUpcast,
  gridViewV3Schema,
} from '../src/schema-zod';

describe('parse + upcast at the boundary', () => {
  it('parses a V1 blob off the wire and upcasts it to V3', () => {
    const raw: unknown = JSON.parse('{"schemaVer":1,"columns":["id","name"]}');
    const view = parseAndUpcast(raw);
    expect(view.schemaVer).toBe(3);
    expect(view.layout.visibleColumns).toEqual(['id', 'name']);
    expect(view.layout.sort).toBe('');
    expect(view.filters).toBe('');
    // the result is itself a valid V3
    expect(gridViewV3Schema.safeParse(view).success).toBe(true);
  });

  it.each([
    [2, { schemaVer: 2, visibleColumns: ['id'], sort: 'id asc' }],
    [3, { schemaVer: 3, layout: { visibleColumns: [], sort: '' }, filters: '' }],
  ])('parses and upcasts a V%i blob to V3', (_ver, blob) => {
    expect(parseAndUpcast(blob).schemaVer).toBe(3);
  });
});

// This is the value zod adds over plain TS: a runtime gate. Plain TS would
// happily hand these malformed blobs to `upcast()` and blow up somewhere deep.
describe('the runtime gate plain TS does not give you', () => {
  it('rejects a blob with an unknown schema version', () => {
    expect(() => parseAndUpcast({ schemaVer: 99, whatever: true })).toThrow();
  });

  it('rejects a V2 blob whose columns are the wrong type', () => {
    expect(() => parseAndUpcast({ schemaVer: 2, visibleColumns: 'nope', sort: 'id asc' })).toThrow();
  });

  it('rejects a structurally broken blob', () => {
    expect(() => parseAndUpcast({ schemaVer: 1, columns: 'not-an-array' })).toThrow();
  });

  it('safeParseAndUpcast reports failure instead of throwing', () => {
    const result = safeParseAndUpcast('total garbage');
    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error.issues.length).toBeGreaterThan(0);
  });

  it('safeParseAndUpcast returns the upcasted view on success', () => {
    const result = safeParseAndUpcast({ schemaVer: 1, columns: ['id'] });
    expect(result.ok).toBe(true);
    if (result.ok) expect(result.view.schemaVer).toBe(3);
  });
});
