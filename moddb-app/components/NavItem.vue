<script setup lang="ts">
const props = defineProps<{
  to?: string;
}>();
</script>

<template>
  <NuxtLink
    :to="props.to"
    active-class="nav-item--active text-black"
    class="nav-item py-1 md:py-0 px-4 first:rounded-tl first:rounded-tr
      md:first:rounded-tr-none"
  >
    <slot></slot>
  </NuxtLink>
</template>

<style>
.nav-item {
  background-color: rgba(255, 255, 255, 0.2);
}

.nav-item:hover {
  background-color: rgba(255, 255, 255, 0.4);
}

.nav-item--active {
  background-color: rgba(255, 255, 255, 0.9);
}
</style>
