import useAuthStore from '~/store/auth';

export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore();

  authStore.initAsync();

  await waitForStoreAsync(authStore, (f) => f.initialized);

  if (!to.meta.requireAuth) {
    return;
  }

  if (!authStore.isLoggedIn) {
    return navigateTo('/');
  }
});
