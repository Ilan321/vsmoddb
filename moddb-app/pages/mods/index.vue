<script setup lang="ts">
import useModsStore, { ModSortDirection, ModSortType } from '~/store/mods';

const store = useModsStore();

const sortItems = [
  {
    text: 'date created',
    value: ModSortType.CREATED
  },
  {
    text: 'date updated',
    value: ModSortType.UPDATED
  },
  {
    text: 'number of downloads',
    value: ModSortType.DOWNLOADS
  },
  {
    text: 'number of comments',
    value: ModSortType.COMMENTS
  },
  {
    text: 'trending',
    value: ModSortType.TRENDING
  },
  {
    text: 'name',
    value: ModSortType.NAME
  }
];

const sortDirectionItems = [
  {
    text: 'descending',
    value: ModSortDirection.DESC
  },
  {
    text: 'ascending',
    value: ModSortDirection.ASC
  }
];

store.initAsync();
</script>

<template>
  <div class="mods">
    <h3 class="mods__title hidden">Filter</h3>
    <div class="flex flex-row gap-2 hidden">
      <v-input
        class="max-w-64"
        v-model="store.filter.text"
        label="Search"
        placeholder="Search by name, author, description"
      />
      <v-select
        class="max-w-32"
        :model-value="store.filter.side"
        :items="[]"
        label="Side"
        @update:model-value="console.log"
      />
    </div>
    <h3
      class="mb-2 pb-2 flex flex-row items-baseline gap-1 border-b border-primary"
    >
      Showing {{ store.mods.length }} out of {{ store.totalMods }}, sorted by
      <div class="flex flex-row gap-2">
        <v-select
          v-model="store.sort.type"
          :items="sortItems"
          text-only
          @update:model-value="store.fetchModsAsync({ reset: true })"
        />
        <v-select
          v-model="store.sort.direction"
          :items="sortDirectionItems"
          text-only
          @update:model-value="store.fetchModsAsync({ reset: true })"
        />
      </div>
      <spinner v-if="store.loading.value" />
    </h3>
    <mod-grid :mods="store.mods" />
    <div class="mt-4">
      <v-button
        @click="store.fetchModsAsync"
        :disabled="store.loading.value"
        :loading="store.loading.value"
      >
        Load more
      </v-button>
    </div>
  </div>
</template>
