<script setup lang="ts">
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';
import useAuthStore from '~/store/auth';

const auth = useAuthStore();

const latestMods = useFetch<ModDisplayModel[]>('/api/v1/mods/latest');
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
    <div v-else>Your mods</div>
    <div class="flex justify-start gap-2 align-center mt-4 mb-2">
      <h2 class="text-xl">Latest 10 mods</h2>
      <spinner v-if="latestMods.status.value === 'pending'" />
    </div>
    <div v-if="latestMods.data.value" class="flex flex-row flex-wrap gap-2">
      <mod-card v-for="mod of latestMods.data.value" :key="mod.id" :mod="mod" />
      <!-- <div v-for="mod of latestMods.data.value" :key="mod.name">
        {{ mod.name }}
      </div> -->
    </div>
  </div>
</template>
