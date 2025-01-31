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
    class="mod-card flex flex-col grow shadow hover:shadow-xl hover:outline transition-all
      shrink bg-white/60"
    :title="props.mod.name"
  >
    <div class="mod-card__banner">
      <NuxtLink :to="url">
        <img :src="bannerUrl" class="w-full h-48 inline-block object-cover" />
      </NuxtLink>
    </div>
    <div class="mod-card__details h-24 px-2 py-1">
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
    </div>
    <div class="mod-card__stats flex flex-row px-2 py-1 pb-2 gap-1">
      <NuxtLink :to="`${url}/files`">
        <div
          class="mod-card__downloads px-1 rounded transition-colors hover:bg-black/10"
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
</template>
