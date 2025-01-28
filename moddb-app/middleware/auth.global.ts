import useAuthStore from '~/store/auth';
import storeUtils from '~/utils/store.utils';

export default defineNuxtRouteMiddleware(async (to, from) => {
  if (!to.meta.requireAuth) {
    return;
  }

  const authStore = useAuthStore();

  authStore.initAsync();

  await storeUtils.waitForStoreAsync(authStore, (f) => f.initialized);

  if (!authStore.isLoggedIn) {
    return navigateTo('/');
  }
});
