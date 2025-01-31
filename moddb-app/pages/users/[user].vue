<script setup lang="ts">
import useUserProfileStore from '~/store/user-profile';

const route = useRoute();

const userName = route.params.user.toString();

const store = useUserProfileStore();

store.initAsync(userName);

useTitle(`${userName}'s mods`);
</script>

<template>
  <div class="user-profile">
    <div class="flex flex-row items-center mb-2">
      <h2 class="text-xl font-semibold">{{ userName }}'s mods</h2>
      <spinner v-if="store.loading.value" />
    </div>
    <mod-grid v-if="!store.loading.value" :mods="store.mods" />
  </div>
</template>
