<script setup lang="ts">
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';

defineProps<{
  mods: ModDisplayModel[];
}>();
</script>

<template>
  <div class="mod-grid">
    <mod-card v-for="mod of mods" :key="mod.id" :mod="mod" />
    <slot name="append"></slot>
  </div>
</template>

<style>
@reference "@/main.css";

.mod-grid {
  @apply grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 2xl:grid-cols-5;
  @apply gap-2;
}
</style>
