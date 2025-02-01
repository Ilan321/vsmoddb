<script setup lang="ts">
import debounce from 'lodash.debounce';
import useModsStore, {
  ModSideFilter,
  ModSortDirection,
  ModSortType
} from '~/store/mods';

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

const sideFilterItems = [
  {
    text: 'Any',
    value: ModSideFilter.Any
  },
  {
    text: 'Both',
    value: ModSideFilter.Both
  },
  {
    text: 'Client',
    value: ModSideFilter.Client
  },
  {
    text: 'Server',
    value: ModSideFilter.Server
  }
];

const onFilterChange = debounce(() => {
  store.fetchModsAsync({ reset: true });
}, 300);

store.initAsync();

useTitle('Mods');
</script>

<template>
  <div class="mods">
    <div class="flex flex-row gap-2">
      <v-input
        class="w-full max-w-64 lg:max-w-80"
        v-model="store.filter.text"
        label="Search"
        placeholder="Search by name, author, description"
        @update:model-value="onFilterChange"
      >
        <template #label="{ inputId }">
          <label
            :for="inputId"
            class="block text-sm/6 font-medium text-gray-900 flex justify-start items-center"
          >
            Search
            <font-awesome
              class="ms-1"
              icon="circle-info"
              title="Surround your search with quotes to search for an exact phrase."
            />
          </label>
        </template>
      </v-input>
      <div class="flex flex-col justify-end max-w-32 w-full hidden">
        <v-select
          v-model="store.filter.side"
          :items="sideFilterItems"
          label="Side"
          @update:model-value="store.fetchModsAsync({ reset: true })"
        />
      </div>
    </div>
    <h3 class="mt-4 mb-2 pb-2 flex flex-col md:flex-row items-baseline gap-1">
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
