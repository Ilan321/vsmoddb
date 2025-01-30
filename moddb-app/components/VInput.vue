<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    modelValue?: string;
    label?: string;
    error?: string;
    message?: string;
    placeholder?: string;
    type?: string;
    name?: string;
    id?: string;
  }>(),
  {
    type: 'text'
  }
);

const defaultId = useId();

const elementId = computed(() => props.id || defaultId);
</script>

<template>
  <div class="v-input">
    <label
      v-if="label"
      :for="elementId"
      class="block text-sm/6 font-medium text-gray-900"
    >
      {{ label }}
    </label>
    <div class="mt-2">
      <input
        :type="type"
        :name="name"
        :id="elementId"
        class="block w-full rounded-md bg-secondary/90 px-3 py-1.5 text-base text-gray-900 sm:text-sm/6 outline outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline focus:outline-2 focus:-outline-offset-2 focus:outline-primary"
        :placeholder="placeholder"
        :aria-describedby="`${elementId}-description`"
      />
    </div>
    <p
      v-if="message"
      class="mt-2 text-sm text-gray-500"
      :id="`${elementId}-description`"
    >
      {{ message }}
    </p>
  </div>
</template>
