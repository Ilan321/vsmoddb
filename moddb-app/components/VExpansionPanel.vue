<script setup lang="ts">
const open = ref(false);
</script>

<template>
  <div class="">
    <div class="">
      <slot
        name="activator"
        :props="{
          onclick: () => (open = !open)
        }"
      ></slot>
    </div>
    <div class="" :class="[open ? 'block' : 'hidden']">
      <slot></slot>
    </div>
  </div>
</template>
