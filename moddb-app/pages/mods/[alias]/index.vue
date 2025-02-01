<script setup lang="ts">
import useModDetailsStore from '@/store/mod-details';
import dayjs from 'dayjs';
import { TransitionGroup } from 'vue';

const store = useModDetailsStore();

const timeCreatedReadable = computed(() => {
  return dayjs(store.mod.timeCreatedUtc).format('LLLL');
});

const timeUpdatedReadable = computed(() =>
  dayjs(store.mod.timeUpdatedUtc).format('LLLL')
);
</script>

<template>
  <div class="mod-description py-2">
    <div
      id="mod-description__hero"
      class="flex flex-col gap-4 pb-4 mb-4 border-b border-b-primary"
    >
      <div id="mod-description__slideshow">
        <img :src="store.imageUrl" class="h-full max-h-96" />
      </div>
      <div id="mod-description__stats">
        <div class="flex flex-row items-center flex-wrap">
          <span class="me-2 text-gray-600">Category:</span>
          <span
            v-for="tag of store.mod.tags"
            :key="tag.value"
            :style="{
              backgroundColor: tag.color
            }"
            class="rounded px-1 not-last:me-2 text-sm"
          >
            #{{ tag.value }}
          </span>
        </div>
        <div>
          <span class="text-gray-600"> Author: </span>
          <NuxtLink :to="`/users/${store.mod.author}`" class="link"
            >{{ store.mod.author }}
          </NuxtLink>
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
        <div>
          <span class="text-gray-600">
            Latest file (for
            {{
              getReadableGameVersions(store.latestFile!.gameVersions)
                .map((f) => f.version)
                .join(', ')
            }}):
          </span>
          <span>
            <a
              :href="`/api/v1/mods/${store.alias}/releases/${store.latestFile!.modVersion}`"
              class="link-blue"
            >
              {{ store.latestFile!.fileName }}
            </a>
          </span>
          <span
            class="text-sm ms-2"
            title="Works only on Windows and v1.18.0-rc.1 or newer"
          >
            <a
              :href="`vintagestorymodinstall://${store.alias}@${store.latestFile!.modVersion}`"
              class="link-dotted text-blue-700"
              >(1-click install)</a
            >
          </span>
        </div>
      </div>
    </div>
    <div id="mod-description__description">
      <v-comment-renderer :value="store.mod.description!" />
    </div>
  </div>
</template>
