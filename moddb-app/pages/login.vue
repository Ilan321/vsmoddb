<script setup lang="ts">
const user = ref('');
const pass = ref('');
const loggingIn = ref(false);

async function loginAsync() {
  if (loggingIn.value) {
    return;
  }

  try {
    loggingIn.value = true;

    const response = await $fetch('/api/v1/account/login', {
      method: 'POST',
      body: {
        username: user.value,
        password: pass.value
      }
    });
  } catch (error: any) {
    console.error({ error });
  } finally {
    loggingIn.value = false;
  }
}
</script>

<template>
  <div class="login">
    <h1 class="text-xl">Login</h1>
    <div class="flex flex-row justify-start">
      <form class="mt-4 min-w-96" @submit.prevent="loginAsync">
        <v-input v-model="user" label="Username" autocomplete="username" />
        <v-input
          v-model="pass"
          class="mt-2"
          label="Password"
          type="password"
          autocomplete="current-password"
        />
        <div class="mt-4 flex justify-start items-center">
          <v-button type="submit">Login</v-button>
          <NuxtLink to="/register" class="ms-4 text-sm text-gray-700 underline">
            Link your account
          </NuxtLink>
        </div>
      </form>
      <div
        class="flex flex-col justify-center items-center border-l border-l-primary ms-4 ps-4"
      >
        <v-button disabled>
          <font-awesome class="me-1" icon="fa-brands google"></font-awesome>
          Login with Google
        </v-button>
      </div>
    </div>
  </div>
</template>
