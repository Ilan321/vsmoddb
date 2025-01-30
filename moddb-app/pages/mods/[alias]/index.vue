<script setup lang="ts">
import useModDetailsStore from '@/store/mod-details';
import dayjs from 'dayjs';
import { TransitionGroup } from 'vue';

const store = useModDetailsStore();

const timeCreatedReadable = computed(() => {
  return dayjs(store.mod.timeCreatedUtc);
});

const timeUpdatedReadable = computed(() => dayjs(store.mod.timeUpdatedUtc));
</script>

<template>
  <div class="mod-description py-2">
    <div
      id="mod-description__hero"
      class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8"
    >
      <div id="mod-description__slideshow">
        <img :src="store.imageUrl" class="w-full" />
      </div>
      <div id="mod-description__stats">
        <div id="mod-description__stats__title">
          <span class="me-2 text-gray-600">Category:</span>
          <span
            v-for="tag of store.mod.tags"
            :key="tag.value"
            :style="{
              backgroundColor: tag.color
            }"
            class="rounded px-1"
          >
            #{{ tag.value }}
          </span>
        </div>
        <div>
          <span class="text-gray-600"> Author: </span>
          <span>{{ store.mod.author }} </span>
        </div>
        <div>
          <span class="text-gray-600"> Side: </span>
          <span>{{ store.mod.side }}</span>
        </div>
        <div>
          <span class="text-gray-600"> First uploaded: </span>
          <span>{{ timeCreatedReadable }}</span>
        </div>
        <div>
          <span class="text-gray-600"> Last updated: </span>
          <span>{{ timeUpdatedReadable }}</span>
        </div>
        <div>
          <span class="text-gray-600">Downloads: </span>
          <span>{{ store.mod.downloads }}</span>
        </div>
      </div>
    </div>
    <div id="mod-description__description">
      <v-comment-renderer :value="store.mod.description!" />
    </div>
  </div>
</template>
