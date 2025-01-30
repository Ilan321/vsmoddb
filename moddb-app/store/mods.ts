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

      await this.fetchModsAsync();
    },
    async fetchModsAsync() {
      this.loading.value = true;

      const loadId = getLoadToken();
      this.loading.id = loadId;

      try {
        const response = await useFetch<GetModsResponse>('/api/v1/mods', {
          query: {
            sort: this.sort.type,
            direction: this.sort.direction,
            skip: this.mods.length,
            take: this.pageSize
          }
        });

        if (!checkLoadToken(this.loading.id, loadId)) {
          return;
        }

        if (!response.data.value) {
          return;
        }

        this.totalMods = response.data.value.totalMods;
        this.mods = [...this.mods, ...response.data.value.mods];
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
