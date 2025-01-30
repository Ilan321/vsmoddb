<script setup lang="ts">
import dayjs from 'dayjs';
import type { ModCommentModel } from '~/models/mods/ModCommentModel';
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';

const props = withDefaults(
  defineProps<{
    comment: ModCommentModel;
    mod?: ModDisplayModel;
  }>(),
  {
    mod: undefined
  }
);

const timeCreatedRelative = computed(() =>
  dayjs(props.comment.timeCreatedUtc).fromNow()
);
</script>

<template>
  <div class="mod-comment w-full border border-gray-400 rounded bg-primary">
    <div class="mod-comment__title">
      💬
      {{ props.comment.author }}
      <span class="text-xs">{{ timeCreatedRelative }}, </span>
      <span v-if="props.mod" class="text-xs text-gray-600">
        in
        <NuxtLink
          :to="`/mods/${props.mod.urlAlias || props.mod.id}`"
          class="underline"
          >{{ props.mod.name }}</NuxtLink
        >
      </span>
    </div>
    <div
      class="mod-comment__body bg-secondary px-2 py-1 max-h-32 overflow-y-auto"
    >
      <div v-html="props.comment.comment"></div>
    </div>
  </div>
</template>

<style>
.mod-comment__body {
  span.username {
    background-color: rgba(159, 207, 52, 0.5);
    border-radius: 4px;
    padding: 2px 3px;
  }

  span.username:before {
    content: '@';
  }

  a {
    color: var(--color-blue-700);
    text-decoration: underline;
  }
}
</style>
