<script setup lang="ts">
const props = defineProps<{
  items: { name: string; url?: string }[];
}>();
</script>

<template>
  <h2 class="breadcrumbs flex flex-row gap-2 text-lg font-semibold">
    <div
      v-for="(item, index) of props.items"
      :key="item.name"
      class="breadcrumbs__item"
    >
      <NuxtLink v-if="item.url" :to="item.url" class="me-2 underline">
        {{ item.name }}
      </NuxtLink>
      <span v-else>
        {{ item.name }}
      </span>
      <span v-if="index < props.items.length - 1"> / </span>
    </div>
  </h2>
</template>
