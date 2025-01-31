import type { ModDetailsModel } from '~/models/mods/ModDetailsModel';
import type { TagModel } from '~/models/TagModel';
import { ModSideFilter } from './mods';
import type { ModCommentModel } from '~/models/mods/ModCommentModel';

function getState() {
  return {
    alias: undefined as string | undefined,
    loading: {
      value: false,
      id: undefined as string | undefined
    },
    mod: {
      id: undefined as number | undefined,
      name: undefined as string | undefined,
      urlAlias: undefined as string | undefined,
      description: undefined as string | undefined,
      tags: [] as TagModel[],
      author: undefined as string | undefined,
      side: undefined as string | undefined,
      timeCreatedUtc: undefined as string | undefined,
      timeUpdatedUtc: undefined as string | undefined,
      downloads: 0
    },
    comments: {
      loading: {
        value: false,
        id: undefined as string | undefined
      },
      value: [] as ModCommentModel[]
    }
  };
}

const useModDetailsStore = defineStore('mod-details', {
  state: getState,
  actions: {
    async initAsync(modAlias: string) {
      this.$reset();

      this.alias = modAlias;

      return this.refreshAsync();
    },
    async refreshAsync() {
      this.loading.value = true;

      const loadId = getLoadToken();
      this.loading.id = loadId;

      try {
        // Fetch mod details

        this.refreshComments();

        const response = await useFetch<ModDetailsModel>(
          '/api/v1/mods/' + this.alias
        );

        if (!checkLoadToken(this.loading.id, loadId)) {
          return;
        }

        const mod = response.data.value!;

        this.mod.id = mod.id;
        this.mod.name = mod.name;
        this.mod.urlAlias = mod.urlAlias;
        this.mod.description = mod.description;
        this.mod.tags = mod.tags;
        this.mod.author = mod.author;
        this.mod.side = mod.side;
        this.mod.timeCreatedUtc = mod.timeCreatedUtc;
        this.mod.timeUpdatedUtc = mod.timeUpdatedUtc;
        this.mod.downloads = mod.downloads;
      } finally {
        if (checkLoadToken(this.loading.id, loadId)) this.loading.value = false;
      }
    },
    async refreshComments() {
      this.comments.loading.value = true;

      const loadId = getLoadToken();
      this.comments.loading.id = loadId;

      try {
        const response = await useFetch<ModCommentModel[]>(
          `/api/v1/mods/${this.alias}/comments`
        );

        if (!checkLoadToken(this.comments.loading.id, loadId)) {
          return;
        }

        this.comments.value = response.data.value!;
      } finally {
        if (checkLoadToken(this.comments.loading.id, loadId))
          this.comments.loading.value = false;
      }
    }
  },
  getters: {
    imageUrl: (state) => `/api/v1/mods/${state?.alias}/banner`
  }
});

export default useModDetailsStore;
