import type { ModDetailsModel } from '~/models/mods/ModDetailsModel';
import type { TagModel } from '~/models/TagModel';
import { ModSideFilter, ModSortDirection } from './mods';
import type { ModCommentModel } from '~/models/mods/ModCommentModel';
import type { ModRelease } from '~/models/mods/ModRelease';
import type { GetModCommentsResponse } from '~/models/responses/mods/GetModCommentsResponse';
import type { PageLoadResult } from '~/models/pages/PageLoadResult';

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
      downloads: 0,
      releases: [] as ModRelease[]
    },
    comments: {
      loading: {
        value: false,
        id: undefined as string | undefined
      },
      sort: ModSortDirection.DESC,
      value: [] as ModCommentModel[],
      total: 0
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
    async refreshAsync(): Promise<PageLoadResult> {
      this.loading.value = true;

      const loadId = getLoadToken();
      this.loading.id = loadId;

      try {
        // Fetch mod details

        this.loadComments({
          reset: true
        });

        const response = await useFetch<ModDetailsModel>(
          '/api/v1/mods/' + this.alias
        );

        if (!checkLoadToken(this.loading.id, loadId)) {
          return {
            success: true
          };
        }

        if (response.error.value) {
          throw response.error.value;
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
        this.mod.releases = mod.releases;

        this.loading.value = false;

        return {
          success: true
        };
      } catch (error: any) {
        return {
          success: false,
          statuscode: error?.statusCode
        };
      }
    },
    async loadComments(options?: { reset?: boolean; clear?: boolean }) {
      this.comments.loading.value = true;

      if (options?.reset && options.clear !== false) {
        this.comments.value = [];
      }

      const loadId = getLoadToken();
      this.comments.loading.id = loadId;

      try {
        const response = await useFetch<GetModCommentsResponse>(
          `/api/v1/mods/${this.alias}/comments`,
          {
            query: {
              skip: options?.reset ? 0 : this.comments.value.length,
              sort: this.comments.sort
            }
          }
        );

        if (!checkLoadToken(this.comments.loading.id, loadId)) {
          return;
        }

        this.comments.total = response.data.value!.totalComments;
        this.comments.value = options?.reset
          ? response.data.value!.comments
          : [...this.comments.value, ...response.data.value!.comments];
      } catch (error) {
        // ignore comment load errors
      } finally {
        if (checkLoadToken(this.comments.loading.id, loadId))
          this.comments.loading.value = false;
      }
    }
  },
  getters: {
    imageUrl: (state) => `/api/v1/mods/${state?.alias}/banner`,
    latestFile: (state) =>
      state.mod.releases.length > 0 ? state.mod.releases[0] : undefined
  }
});

export default useModDetailsStore;
