import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';
import type { GetModsResponse } from '~/models/responses/mods/GetModsResponse';

function getState() {
  return {
    sort: {
      type: ModSortType.CREATED,
      direction: ModSortDirection.DESC
    },
    filter: {
      text: '',
      side: ModSideFilter.Any
    },
    loading: {
      value: false,
      id: undefined as string | undefined
    },
    mods: [] as ModDisplayModel[],
    pageSize: 25,
    totalMods: 0
  };
}

const useModsStore = defineStore('mods', {
  state: getState,
  actions: {
    async initAsync() {
      if (this.mods.length > 0) {
        return;
      }

      await this.fetchModsAsync({
        initial: true
      });
    },
    async fetchModsAsync(options?: { reset?: boolean; initial?: boolean }) {
      this.loading.value = true;

      const loadId = getLoadToken();
      this.loading.id = loadId;

      try {
        const requestOptions = {
          query: {
            sort: this.sort.type,
            direction: this.sort.direction,
            skip: options?.reset ? 0 : this.mods.length,
            take: this.pageSize
          }
        };

        let response: GetModsResponse | null | undefined;

        if (options?.initial) {
          const fetchResponse = await useFetch<GetModsResponse>(
            '/api/v1/mods',
            requestOptions
          );

          if (fetchResponse.error.value) {
            throw fetchResponse.error;
          }

          response = fetchResponse.data.value;
        } else {
          response = await $fetch('/api/v1/mods', requestOptions);
        }

        if (!checkLoadToken(this.loading.id, loadId)) {
          return;
        }

        if (!response) {
          return;
        }

        this.totalMods = response.totalMods;
        if (options?.reset) {
          this.mods = response.mods;
        } else {
          this.mods = [...this.mods, ...response.mods];
        }
      } catch (error) {
        console.error(error);
      } finally {
        if (checkLoadToken(this.loading.id, loadId)) {
          this.loading.value = false;
        }
      }
    }
  },
  getters: {}
});

export default useModsStore;

export enum ModSortType {
  CREATED = 'Created',
  DOWNLOADS = 'Downloads',
  COMMENTS = 'Comments',
  TRENDING = 'Trending',
  NAME = 'Name'
}

export enum ModSortDirection {
  ASC = 'Ascending',
  DESC = 'Descending'
}

export enum ModSideFilter {
  Any = 'Any',
  Both = 'Both',
  Client = 'Client',
  Server = 'Server'
}
