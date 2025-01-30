<script setup lang="ts">
defineProps<{
  modelValue?: string;
  label?: string;
  error?: string;
  message?: string;
}>();

const id = useId();
</script>

<template>
  <div class="v-input">
    <label
      v-if="label"
      :for="id"
      class="block text-sm/6 font-medium text-gray-900"
    >
      {{ label }}
    </label>
    <div class="mt-2">
      <input
        type="email"
        name="email"
        :id="id"
        class="block w-full rounded-md bg-secondary/90 px-3 py-1.5 text-base text-gray-900 sm:text-sm/6 outline outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400 focus:outline focus:outline-2 focus:-outline-offset-2 focus:outline-primary"
        placeholder="you@example.com"
        aria-describedby="email-description"
      />
    </div>
    <p
      v-if="message"
      class="mt-2 text-sm text-gray-500"
      :id="`${id}-description`"
    >
      {{ message }}
    </p>
  </div>
</template>
