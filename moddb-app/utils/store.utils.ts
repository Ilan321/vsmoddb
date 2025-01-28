import type { StateTree, Store } from 'pinia';

async function waitForStoreAsync<TState extends StateTree>(
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

export default {
  waitForStoreAsync
};
