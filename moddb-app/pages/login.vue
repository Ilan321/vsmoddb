<script setup lang="ts">
import useAuthStore from '~/store/auth';

const router = useRouter();
const authStore = useAuthStore();

const user = ref('');
const pass = ref('');
const loggingIn = ref(false);
const loginError = ref(false);

async function loginAsync() {
  if (loggingIn.value) {
    return;
  }

  try {
    loggingIn.value = true;

    await $fetch('/api/v1/account/login', {
      method: 'POST',
      body: {
        username: user.value,
        password: pass.value
      }
    });

    await authStore.initAsync({ force: true });

    await router.push('/');
  } catch (error: any) {
    loginError.value = true;
  } finally {
    loggingIn.value = false;
  }
}
</script>

<template>
  <div class="login">
    <h1 class="text-xl">Login</h1>
    <div class="flex flex-col justify-start max-w-md">
      <form
        class="mt-4 pb-4 mb-4 border-b border-primary"
        @submit.prevent="loginAsync"
      >
        <v-input v-model="user" label="Username" autocomplete="username" />
        <v-input
          v-model="pass"
          class="mt-2"
          label="Password"
          type="password"
          autocomplete="current-password"
        />
        <div v-if="loginError" class="mt-2 text-error text-sm">
          Invalid username or password
        </div>
        <div class="mt-4 flex justify-between items-center">
          <v-button :loading="loggingIn" type="submit">Login</v-button>
          <NuxtLink to="/register" class="ms-4 text-sm text-gray-700 underline">
            Link your account
          </NuxtLink>
        </div>
      </form>
      <v-button disabled>
        <font-awesome icon="fa-brands google"></font-awesome>
        Login with Google
      </v-button>
    </div>
  </div>
</template>
