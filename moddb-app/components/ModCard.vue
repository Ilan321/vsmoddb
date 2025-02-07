<script setup lang="ts">
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';

const props = defineProps<{
  mod: ModDisplayModel;
}>();

const url = computed(() => {
  if (props.mod.urlAlias) {
    return `/mods/${props.mod.urlAlias}`;
  }

  return `/mods/${props.mod.id}`;
});

const bannerUrl = computed(() => `/api/v1/mods/${props.mod.id}/banner`);
</script>

<template>
  <div
    class="mod-card flex flex-col grow shadow hover:shadow-xl hover:outline outline-primary
      transition-all shrink bg-white/60"
    :title="props.mod.name"
  >
    <NuxtLink :to="url">
      <div class="mod-card__banner">
        <img :src="bannerUrl" class="w-full h-48 inline-block object-cover" />
      </div>
    </NuxtLink>
    <NuxtLink :to="url">
      <div class="mod-card__details h-24 px-2 py-1">
        <div class="mod-card__name truncate">
          <div class="font-bold whitespace-normal line-clamp-2">
            {{ props.mod.name }}
          </div>
          <div class="text-sm h-full whitespace-normal line-clamp-2">
            {{ props.mod.summary }}
          </div>
        </div>
      </div>
    </NuxtLink>
    <div
      class="mod-card__stats flex flex-row justify-between items-center px-2 py-1 pb-2"
    >
      <NuxtLink :to="`/users/${props.mod.author}`">
        <span
          class="h-full text-sm py-1 px-1 rounded text-gray-700 transition-colors
            hover:bg-black/10"
        >
          {{ props.mod.author }}
        </span>
      </NuxtLink>
      <div class="flex flex-row items-center gap-1">
        <NuxtLink :to="`${url}/files`">
          <div
            class="mod-card__downloads h-full px-1 rounded transition-colors hover:bg-black/10"
          >
            <font-awesome icon="download" />
            {{ props.mod.downloads }}
          </div>
        </NuxtLink>

        <NuxtLink :to="`${url}#comments`">
          <div
            class="mod-card__comments px-1 rounded transition-colors hover:bg-black/10"
          >
            <font-awesome icon="comments" />
            {{ props.mod.comments }}
          </div>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>
