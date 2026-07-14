// The same upcasting, wired through zustand's `persist` middleware.
//
// Here you don't hand-roll the "when do I migrate / write back" plumbing —
// persist does it. The upcastToV2 / upcastToV3 / upcast() functions from
// schema.ts don't change at all: you hand `upcast()` to persist's `migrate` and
// it decides *when* to run it. persist works without React (zustand/vanilla),
// so this stays pure TypeScript.

import { createStore } from 'zustand/vanilla';
import {
  persist,
  createJSONStorage,
  type StateStorage,
} from 'zustand/middleware';
import { upcast, type AnyGridView, type GridViewV3 } from './schema';

// An in-memory stand-in for "a string storage backed by your API instead of
// localStorage". In real code getItem/setItem would GET/PUT the opaque string
// (and may be async — zustand handles the promises). `dump()` is a test hook so
// we can peek at what actually landed in storage.
export type ApiStorage = StateStorage & {
  dump(): Record<string, string>;
};

export function createApiStorage(seed: Record<string, string> = {}): ApiStorage {
  const backing = new Map<string, string>(Object.entries(seed));
  return {
    getItem: (name) => backing.get(name) ?? null,
    setItem: (name, value) => {
      backing.set(name, value);
    },
    removeItem: (name) => {
      backing.delete(name);
    },
    dump: () => Object.fromEntries(backing),
  };
}

const freshView = (): GridViewV3 => ({
  schemaVer: 3,
  layout: { visibleColumns: [], sort: '' },
  filters: '',
});

export function createGridViewStore(name: string, storage: StateStorage) {
  return createStore<GridViewV3>()(
    persist(freshView, {
      name,
      version: 3,
      storage: createJSONStorage(() => storage),
      migrate: (persisted, version) =>
        // zustand keeps the version on the envelope; our upcast() reads it
        // inline, so stitch it back on and reuse the very same function.
        upcast({ ...(persisted as object), schemaVer: version } as AnyGridView),
    }),
  );
}
