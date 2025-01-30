import type { StateTree } from 'pinia';

export async function waitForStoreAsync<TState extends StateTree>(
  store: TState,
  selector: (store: TState) => any
) {
  return new Promise((resolve) => {
    if (selector(store)) {
      return resolve(selector(store));
    }

    const unwatch = store.$subscribe((_: any, state: TState) => {
      if (selector(state)) {
        unwatch();

        resolve(selector(state));
      }
    });
  });
}

export function getLoadToken() {
  return Math.random().toString();
}

export function checkLoadToken(a?: string, b?: string) {
  return a === b;
}
