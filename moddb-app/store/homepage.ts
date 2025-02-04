import type { LatestModCommentModel } from '~/models/mods/LatestModCommentModel';
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';
import useAuthStore from './auth';

function getState() {
  return {
    mods: {
      loading: {
        value: false,
        id: '',
        error: false
      },
      value: [] as ModDisplayModel[]
    },
    myMods: {
      loading: {
        value: false,
        id: '',
        error: false
      },
      value: [] as ModDisplayModel[]
    },
    comments: {
      loading: {
        value: false,
        id: '',
        error: false
      },
      value: [] as LatestModCommentModel[]
    }
  };
}

const useHomepageStore = defineStore('homepage', {
  state: getState,
  actions: {
    async refreshAsync() {
      const loadTasks = [
        this.refreshLatestModsAsync(),
        this.refreshLatestCommentsAsync()
      ];

      const authStore = useAuthStore();

      if (authStore.isLoggedIn) {
        loadTasks.push(this.refreshMyModsAsync());
      }

      return Promise.all(loadTasks);
    },
    async refreshLatestModsAsync() {
      return this._refreshInternal('/api/v1/mods/latest', this.mods);
    },
    async refreshLatestCommentsAsync() {
      return this._refreshInternal(
        '/api/v1/mods/latest/comments',
        this.comments
      );
    },
    async refreshMyModsAsync() {
      return this._refreshInternal('/api/v1/mods/mine', this.myMods);
    },
    async _refreshInternal<TResponse>(
      url: string,
      state: {
        loading: { value: boolean; id: string; error: boolean };
        value: TResponse[];
      }
    ) {
      state.loading.value = true;

      const loadId = getLoadToken();
      state.loading.id = loadId;

      try {
        const response = await useFetch<TResponse[]>(url);

        if (!checkLoadToken(state.loading.id, loadId)) {
          return;
        }

        state.value = response.data.value!;
      } catch (error) {
        state.loading.error = true;

        console.error(error);
      } finally {
        if (checkLoadToken(state.loading.id, loadId)) {
          state.loading.value = false;
        }
      }
    }
  }
});

export default useHomepageStore;
