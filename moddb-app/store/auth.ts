import type { GetUserProfileResponse } from '~/models/responses/account/GetUserProfileResponse';

function getState() {
  return {
    initialized: false,
    initializing: false,
    username: undefined as string | undefined,
    email: undefined as string | undefined
  };
}

const useAuthStore = defineStore('auth', {
  state: getState,
  actions: {
    async initAsync() {
      if (this.initialized || this.initializing) {
        return;
      }

      this.initializing = true;

      try {
        const response = await useFetch<GetUserProfileResponse>(
          '/api/v1/account/profile'
        );

        this.username = response.data.value!.username;
        this.email = response.data.value!.email;
      } catch (error: any) {
        if (error?.statusCode === 401) {
          // Not logged in

          return;
        }
      } finally {
        this.initialized = true;
        this.initializing = false;
      }
    }
  },
  getters: {
    isLoggedIn: (state) => !!state.username
  }
});

export default useAuthStore;
