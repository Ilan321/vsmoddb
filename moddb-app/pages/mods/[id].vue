<script setup lang="ts">
const route = useRoute();

const pageId = computed(() => route.params.id);
</script>

<template>
  <div class="mod-page">
    {{ pageId }}
  </div>
</template>
