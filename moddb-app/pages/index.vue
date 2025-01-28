<script setup lang="ts">
import useAuthStore from '~/store/auth';

const auth = useAuthStore();
</script>

<template>
  <div class="home flex flex-col justify-start align-start">
    <div v-if="!auth.isLoggedIn">
      <h1 class="text-xl mb-2">
        Welcome to the unofficial mod repository for Vintage Story!
      </h1>
      <p>
        The goal of this site is to provide a complete, modern rewrite of the
        <NuxtLink
          to="https://mods.vintagestory.at"
          class="text-blue-600 hover:text-blue-700 transition-colors"
          >Vintage Story ModDB</NuxtLink
        >
        using .NET and Vue.js.
      </p>
    </div>
  </div>
</template>
