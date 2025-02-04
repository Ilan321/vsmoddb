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
    disabled?: boolean;
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
    <slot name="label" :input-id="elementId">
      <label
        v-if="label"
        :for="elementId"
        class="block text-sm/6 font-medium text-gray-900"
      >
        {{ label }}
      </label>
    </slot>
    <div class="mt-2">
      <input
        :type="type"
        :name="name"
        :id="elementId"
        :disabled="disabled"
        class="block w-full rounded-md bg-secondary/90 px-3 py-1.5 text-base text-gray-900
          sm:text-sm/6 outline outline-1 -outline-offset-1 outline-gray-300
          placeholder:text-gray-400 focus:outline focus:outline-2 focus:-outline-offset-2
          focus:outline-primary"
        :placeholder="placeholder"
        :value="modelValue"
        :aria-describedby="`${elementId}-description`"
        @input="
          $emit('update:modelValue', ($event.target as HTMLInputElement).value)
        "
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
