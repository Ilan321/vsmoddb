<script setup lang="ts">
import dayjs from 'dayjs';
import type { ModCommentModel } from '~/models/mods/ModCommentModel';

const props = defineProps<{
  comment: ModCommentModel;
}>();

const timeCreatedRelative = computed(() =>
  dayjs(props.comment.timeCreatedUtc).fromNow()
);
</script>

<template>
  <div class="mod-comment w-full border border-gray-400 rounded bg-primary">
    <div class="mod-comment__title">
      💬
      {{ props.comment.author }},
      {{ timeCreatedRelative }}
    </div>
    <div class="mod-comment__body bg-secondary px-2 py-1">
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
}
</style>
