import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';
import type { GetModsResponse } from '~/models/responses/mods/GetModsResponse';
import type { GetTagsResponse } from '~/models/responses/mods/GetTagsResponse';
import type { TagModel } from '~/models/TagModel';

function getState() {
  return {
    sort: {
      type: ModSortType.CREATED,
      direction: ModSortDirection.DESC
    },
    filter: {
      text: '',
      side: ModSideFilter.Any,
      author: '',
      gameVersions: [] as string[],
      tags: [] as string[]
    },
    loading: {
      value: false,
      id: undefined as string | undefined
    },
    mods: [] as ModDisplayModel[],
    tags: {
      loading: {
        value: false,
        id: ''
      },
      tags: [] as TagModel[],
      gameVersions: [] as TagModel[]
    },
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

      await Promise.all([
        this.refreshTagsAsync(),
        this.fetchModsAsync({
          initial: true
        })
      ]);
    },
    async refreshTagsAsync() {
      this.tags.loading.value = true;

      const loadId = getLoadToken();
      this.tags.loading.id = loadId;

      try {
        const response = await useFetch<GetTagsResponse>('/api/v1/mods/tags');

        if (response.error.value) {
          throw response.error.value;
        }

        if (!checkLoadToken(this.tags.loading.id, loadId)) {
          return;
        }

        this.tags.tags = response.data.value!.tags;
        this.tags.gameVersions = response.data.value!.gameVersions;
      } finally {
        if (checkLoadToken(this.tags.loading.id, loadId)) {
          this.tags.loading.value = false;
        }
      }
    },
    async fetchModsAsync(options?: { reset?: boolean; initial?: boolean }) {
      this.loading.value = true;

      const loadId = getLoadToken();
      this.loading.id = loadId;

      try {
        const request = {
          text: this.filter.text,
          sort: this.sort.type,
          direction: this.sort.direction,
          side: this.filter.side,
          author: this.filter.author,
          gameVersions: this.getApplicableGameVersions(
            this.filter.gameVersions
          ).map((f) => f.value),
          tags: this.filter.tags,
          skip: options?.reset || options?.initial ? 0 : this.mods.length,
          take: this.pageSize
        };

        let response: GetModsResponse | null | undefined;

        if (options?.initial) {
          const modsFetchResponse = await useFetch<GetModsResponse>(
            '/api/v1/mods/search',
            {
              method: 'POST',
              body: request
            }
          );

          if (modsFetchResponse.error.value) {
            throw modsFetchResponse.error;
          }

          response = modsFetchResponse.data.value;
        } else {
          response = await $fetch<GetModsResponse>('/api/v1/mods/search', {
            method: 'POST',
            body: request
          });
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
  getters: {
    filterTags: (state) =>
      state.tags.tags.map((f) => ({
        text: f.value,
        value: f.value
      })),
    filterGameVersions: (state) =>
      getCommonGameVersions(state.tags.gameVersions.map((f) => f.value)).map(
        (f) => ({
          text: f,
          value: f
        })
      ),
    /**
     * Returns the selected game versions by parsing the selected gameVersions
     */
    getApplicableGameVersions: (state) => (gameVersions: string[]) =>
      state.tags.gameVersions.filter((f) =>
        gameVersions.some(
          (gv) =>
            f.value.split('.').slice(0, 2).join('.') ===
            gv.split('.').slice(0, 2).join('.')
        )
      )
  }
});

export default useModsStore;

export enum ModSortType {
  CREATED = 'Created',
  DOWNLOADS = 'Downloads',
  COMMENTS = 'Comments',
  TRENDING = 'Trending',
  NAME = 'Name',
  UPDATED = 'Updated'
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
