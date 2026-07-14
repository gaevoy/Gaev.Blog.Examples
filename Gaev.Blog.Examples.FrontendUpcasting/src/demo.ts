// Runnable walk-through: `npm run demo`.
import { upcastStore, type AnyGridView } from './schema';
import { parseAndUpcast, safeParseAndUpcast } from './schema-zod';
import { createApiStorage, createGridViewStore } from './schema-zustand';

// A store that's been around a while holds a mix of every version — users who
// saved a view last year look nothing like users who saved one this morning.
const savedViews: Record<string, AnyGridView> = {
  'orders:default': {
    schemaVer: 1,
    columns: ['id', 'total', 'createdAt'],
    sort: 'createdAt desc',
  },
  'orders:finance': {
    schemaVer: 2,
    columns: ['id', 'total'],
    sort: { field: 'total', direction: 'desc' },
  },
  'orders:ops': {
    schemaVer: 3,
    layout: {
      columns: [
        { id: 'id', width: 80 },
        { id: 'status', width: 140 },
      ],
      sort: { field: 'id', direction: 'asc' },
    },
    filters: [{ field: 'status', op: 'eq', value: 'open' }],
  },
};

console.log('— Plain TS: upcast the whole store at the boundary —');
console.dir(upcastStore(savedViews), { depth: null });

console.log('\n— zod: parse an untyped blob off the wire, then upcast —');
const fromApi: unknown = JSON.parse(
  '{"schemaVer":1,"columns":["id","name"],"sort":"name asc"}',
);
console.dir(parseAndUpcast(fromApi), { depth: null });

console.log('\n— zod: a malformed blob is caught, not silently trusted —');
const broken = safeParseAndUpcast({ schemaVer: 2, columns: 'oops', sort: {} });
console.log(broken.ok ? broken.view : `rejected: ${broken.error.issues.length} issue(s)`);

console.log('\n— zustand: persist runs upcast() as migrate, then writes V3 back —');
const storage = createApiStorage({
  'orders:default': JSON.stringify({
    state: { columns: ['id', 'total'], sort: 'total desc' },
    version: 1,
  }),
});
const store = createGridViewStore('orders:default', storage);
await store.persist.rehydrate();
console.log('in memory :', JSON.stringify(store.getState()));
console.log('in storage:', storage.dump()['orders:default']); // now a version-3 envelope
