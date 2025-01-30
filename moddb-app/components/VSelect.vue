<script setup lang="ts">
defineProps<{
  label?: string;
  items: string[];
  modelValue?: string;
}>();
</script>

<template>
  <div class="block text-sm/6 font-medium text-gray-900">
    <label v-if="label" for="location" class="v-select__label">{{
      label
    }}</label>
    <div class="mt-2 grid grid-cols-1">
      <select
        id="location"
        name="location"
        class="col-start-1 row-start-1 w-full appearance-none rounded-md bg-white py-1.5 pl-3 pr-8 text-base text-gray-900 outline outline-1 -outline-offset-1 outline-gray-300 focus:outline focus:outline-2 focus:-outline-offset-2 focus:outline-primary sm:text-sm/6"
      >
        <option
          v-for="item of items"
          :key="item"
          :selected="modelValue === item"
        >
          {{ item }}
        </option>
      </select>
      <font-awesome
        icon="chevron-down"
        class="pointer-events-none col-start-1 row-start-1 mr-2 size-5 sm:size-4 self-center justify-self-end text-gray-500"
      />
    </div>
  </div>
</template>
