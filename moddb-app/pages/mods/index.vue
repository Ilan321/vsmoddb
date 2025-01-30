<script setup lang="ts">
import useModsStore, { ModSortDirection, ModSortType } from '~/store/mods';

const store = useModsStore();

const sortText = computed(() => {
  const dir =
    store.sort.direction === ModSortDirection.ASC ? 'ascending' : 'descending';
  let text = '';

  switch (store.sort.type) {
    case ModSortType.COMMENTS:
      text = 'number of comments';
      break;
    case ModSortType.DOWNLOADS:
      text = 'number of downloads';
      break;
    case ModSortType.NAME:
      text = 'name';
      break;
    case ModSortType.CREATED:
      text = 'upload date';
      break;
    case ModSortType.TRENDING:
      text = 'trending';
      break;
  }

  return `${text}, ${dir}`;
});

store.initAsync();
</script>

<template>
  <div class="mods">
    <h3 class="mods__title">Filter</h3>
    <div class="mods__filter">
      <v-input class="max-w-64" v-model="store.filter.text" label="Search" />
      <v-select
        class="max-w-32"
        v-model="store.filter.side"
        :items="['Any', 'Both', 'Client', 'Server']"
        label="Side"
      />
    </div>
    <h3 class="mods__title">
      {{ store.totalMods }} mods, sorted by {{ sortText }}
    </h3>
    <mod-grid :mods="store.mods" />
  </div>
</template>

<style>
@reference "@/main.css";

.mods {
  .mods__title {
    @apply text-lg mb-2;
  }

  .mods__filter {
    @apply mb-4;
    @apply flex flex-row gap-2;
  }
}
</style>
