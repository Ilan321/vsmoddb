function getState() {
  return {
    initialized: false,
    initializing: false,
    uid: undefined as string | undefined
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
      } finally {
        this.initialized = true;
        this.initializing = false;
      }
    }
  },
  getters: {
    isLoggedIn: (state) => !!state.uid
  }
});

export default useAuthStore;
