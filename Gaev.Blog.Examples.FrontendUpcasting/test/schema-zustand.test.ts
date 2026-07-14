import { describe, it, expect } from 'vitest';
import { createApiStorage, createGridViewStore } from '../src/schema-zustand';

// Older stored blobs carry only the envelope `version` — no inline schemaVer.
// That's exactly why `migrate` stitches the version back on before upcasting.
const V1_ENVELOPE = JSON.stringify({
  state: { columns: ['id', 'total'], sort: 'total desc' },
  version: 1,
});
const V2_ENVELOPE = JSON.stringify({
  state: { columns: ['id'], sort: { field: 'id', direction: 'asc' } },
  version: 2,
});
const V3_ENVELOPE = JSON.stringify({
  state: {
    schemaVer: 3,
    layout: { columns: [{ id: 'id', width: 80 }], sort: { field: 'id', direction: 'asc' } },
    filters: [],
  },
  version: 3,
});

describe('zustand persist runs upcast() as its migrate', () => {
  it('migrates a stored V1 blob up to V3 on read', async () => {
    const storage = createApiStorage({ 'orders:default': V1_ENVELOPE });
    const store = createGridViewStore('orders:default', storage);
    await store.persist.rehydrate();

    const view = store.getState();
    expect(view.schemaVer).toBe(3);
    expect(view.layout.sort).toEqual({ field: 'total', direction: 'desc' });
    expect(view.layout.columns).toEqual([
      { id: 'id', width: 120 },
      { id: 'total', width: 120 },
    ]);
  });

  it('migrates a stored V2 blob up to V3 on read', async () => {
    const storage = createApiStorage({ 'orders:finance': V2_ENVELOPE });
    const store = createGridViewStore('orders:finance', storage);
    await store.persist.rehydrate();
    expect(store.getState().schemaVer).toBe(3);
  });

  it('leaves an already-current V3 blob alone', async () => {
    const storage = createApiStorage({ 'orders:ops': V3_ENVELOPE });
    const store = createGridViewStore('orders:ops', storage);
    await store.persist.rehydrate();
    expect(store.getState().layout.columns).toEqual([{ id: 'id', width: 80 }]);
  });
});

// "Saving makes the upgrade stick": persist writes the migrated value straight
// back to storage, so the store migrates itself lazily, one read at a time.
describe('saving makes the upgrade stick', () => {
  it('writes the upgraded V3 shape back to storage after migrating a V1', async () => {
    const storage = createApiStorage({ 'orders:default': V1_ENVELOPE });
    const store = createGridViewStore('orders:default', storage);
    await store.persist.rehydrate();

    const written = JSON.parse(storage.dump()['orders:default']);
    expect(written.version).toBe(3);
    expect(written.state.schemaVer).toBe(3);
    expect(written.state.filters).toEqual([]);
    // the loose V1 sort string is gone — it's structured now
    expect(written.state.layout.sort).toEqual({ field: 'total', direction: 'desc' });
  });

  it('does not rewrite a blob that was already current', async () => {
    const storage = createApiStorage({ 'orders:ops': V3_ENVELOPE });
    const store = createGridViewStore('orders:ops', storage);
    await store.persist.rehydrate();
    expect(JSON.parse(storage.dump()['orders:ops'])).toEqual(JSON.parse(V3_ENVELOPE));
  });

  it('persists an ordinary edit as the latest shape', async () => {
    const storage = createApiStorage();
    const store = createGridViewStore('orders:new', storage);
    await store.persist.rehydrate();

    store.setState({ filters: [{ field: 'status', op: 'eq', value: 'open' }] });

    const written = JSON.parse(storage.dump()['orders:new']);
    expect(written.version).toBe(3);
    expect(written.state.filters).toEqual([{ field: 'status', op: 'eq', value: 'open' }]);
  });
});
