<script setup lang="ts">
const props = defineProps<{
  to: string;
}>();
</script>

<template>
  <NuxtLink
    :to="props.to"
    class="tab bg-black/20 hover:bg-black/15 px-2 h-8 flex justify-center items-center first:rounded-tl last:rounded-tr not-first:border-l-0 border border-stone-400"
    exact-active-class="active"
  >
    <div>
      <slot></slot>
    </div>
  </NuxtLink>
</template>

<style>
.tab.active {
  background-color: rgba(156, 145, 58, 0.2);
  border-bottom: 2px solid rgba(156, 145, 58, 0.2);
}
</style>
