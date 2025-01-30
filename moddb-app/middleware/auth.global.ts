import useAuthStore from '~/store/auth';

export default defineNuxtRouteMiddleware(async (to, from) => {
  if (!to.meta.requireAuth) {
    return;
  }

  const authStore = useAuthStore();

  authStore.initAsync();

  await waitForStoreAsync(authStore, (f) => f.initialized);

  if (!authStore.isLoggedIn) {
    return navigateTo('/');
  }
});
