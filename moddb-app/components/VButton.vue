<script setup lang="ts">
const props = defineProps<{
  text?: string;
  loading?: boolean;
  disabled?: boolean;
  icon?: string;
}>();

const emit = defineEmits<{
  (e: 'click'): void;
}>();

const showSlot = computed(() => {
  if (props.loading) {
    return false;
  }

  if (props.icon) {
    return false;
  }

  return true;
});
</script>

<template>
  <button
    type="button"
    :disabled="disabled"
    class="rounded-md bg-indigo-600 px-2.5 py-1.5 text-sm font-semibold text-white
      shadow-sm hover:bg-indigo-500 focus-visible:outline-2
      focus-visible:outline-offset-2 focus-visible:outline-indigo-600 cursor-pointer
      disabled:cursor-auto disabled:bg-indigo-500 disabled:focus-visible:outline-0
      disabled:text-white/80 relative"
    @click="emit('click')"
  >
    <span
      :class="{
        invisible: !showSlot
      }"
    >
      <slot></slot>
    </span>
    <span
      class="absolute left-1/2 transform -translate-x-1/2"
      :class="{
        invisible: !props.loading
      }"
    >
      <font-awesome icon="spinner" spin />
    </span>
    <span
      class="absolute left-1/2 transform -translate-x-1/2"
      :class="{
        invisible: !props.icon
      }"
    >
      <font-awesome v-if="props.icon" :icon="props.icon" />
    </span>
  </button>
</template>
