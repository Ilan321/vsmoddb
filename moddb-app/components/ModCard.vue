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
  <NuxtLink :to="url">
    <div
      class="mod-card h-72 w-[300px] max-w-sm shadow grow bg-white/60 overflow-hidden"
    >
      <div class="mod-card__banner">
        <img :src="bannerUrl" class="w-full h-48 inline-block object-cover" />
      </div>
      <div class="mod-card__details px-2 py-1 grid grid-cols-[1fr_auto] gap-2">
        <div class="mod-card__name">
          <div class="font-bold">
            {{ props.mod.name }}
          </div>
          <div class="text-sm">
            {{ props.mod.summary }}
          </div>
        </div>
        <div class="mod-card__stats">
          <div class="mod-card__downloads">
            <font-awesome icon="download" />
            {{ props.mod.downloads }}
          </div>
          <div class="mod-card__comments">
            <font-awesome icon="comments" />
            {{ props.mod.comments }}
          </div>
        </div>
      </div>
    </div>
  </NuxtLink>
</template>
