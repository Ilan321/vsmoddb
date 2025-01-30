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
    class="mod-card h-72 grow shadow hover:shadow-xl transition-shadow shrink bg-white/60"
    :title="props.mod.name"
  >
    <NuxtLink :to="url">
      <div class="mod-card__banner">
        <img :src="bannerUrl" class="w-full h-48 inline-block object-cover" />
      </div>
    </NuxtLink>
    <div class="mod-card__details px-2 py-1 grid grid-cols-[1fr_auto] gap-2">
      <div class="mod-card__name truncate">
        <NuxtLink :to="url">
          <div class="font-bold whitespace-normal line-clamp-2">
            {{ props.mod.name }}
          </div>
          <div class="text-sm h-full whitespace-normal line-clamp-2">
            {{ props.mod.summary }}
          </div>
        </NuxtLink>
      </div>
      <div class="mod-card__stats">
        <div
          class="mod-card__downloads px-1 rounded transition-colors hover:bg-black/20"
        >
          <NuxtLink :to="`${url}/downloads`">
            <font-awesome icon="download" />
            {{ props.mod.downloads }}
          </NuxtLink>
        </div>
        <div
          class="mod-card__comments px-1 rounded transition-colors hover:bg-black/20"
        >
          <NuxtLink :to="`${url}#comments`">
            <font-awesome icon="comments" />
            {{ props.mod.comments }}
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>
