<script setup lang="ts">
const props = defineProps<{
  to: string;
  target?: '_blank';
}>();
</script>

<template>
  <NuxtLink
    :to="props.to"
    class="tab bg-black/20 hover:bg-black/15 px-2 h-8 flex justify-start md:justify-center
      items-center first:rounded-tl first:rounded-tr md:rounded-tr-none
      md:last:rounded-tr not-first:border-l-0 border border-stone-400"
    exact-active-class="active"
    :target="props.target"
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
