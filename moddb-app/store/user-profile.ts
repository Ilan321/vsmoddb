import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';
import { ModSortDirection, ModSortType } from './mods';
import type { GetModsResponse } from '~/models/responses/mods/GetModsResponse';

function getState() {
  return {
    userName: '',
    loading: {
      value: false,
      id: ''
    },
    mods: [] as ModDisplayModel[]
  };
}

const useUserProfileStore = defineStore('user-profile', {
  state: getState,
  actions: {
    async initAsync(userName: string) {
      this.userName = userName;

      return this.refreshAsync();
    },
    async refreshAsync() {
      this.loading.value = true;

      const loadId = getLoadToken();
      this.loading.id = loadId;

      try {
        // Fetch mod details

        const mods = await useFetch<ModDisplayModel[]>(
          '/api/v1/mods/by-author',
          {
            query: {
              author: this.userName
            }
          }
        );

        if (!checkLoadToken(this.loading.id, loadId)) {
          return;
        }

        this.mods = mods.data.value!;
      } finally {
        if (checkLoadToken(this.loading.id, loadId)) this.loading.value = false;
      }
    }
  }
});

export default useUserProfileStore;
