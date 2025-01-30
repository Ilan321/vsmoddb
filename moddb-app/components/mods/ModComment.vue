<script setup lang="ts">
import dayjs from 'dayjs';
import { ModCommentContentType } from '~/models/enums/ModCommentContentType';
import type { ModCommentModel } from '~/models/mods/ModCommentModel';
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';

const props = withDefaults(
  defineProps<{
    comment: ModCommentModel;
    mod?: ModDisplayModel;
    author?: boolean;
  }>(),
  {
    mod: undefined,
    author: false
  }
);

const timeCreatedRelative = computed(() =>
  dayjs(props.comment.timeCreatedUtc).fromNow()
);

const isHtml = computed(
  () => props.comment.contentType === ModCommentContentType.Html
);
const isMarkdown = computed(
  () => props.comment.contentType === ModCommentContentType.Markdown
);
</script>

<template>
  <div class="mod-comment w-full border border-gray-400 rounded bg-primary">
    <div class="mod-comment__title">
      💬
      {{ props.comment.author }}
      <span
        v-if="props.author"
        class="text-xs rounded px-1 text-white bg-blue-900 me-1"
      >
        Author
      </span>
      <span v-if="!props.mod" class="text-xs">{{ timeCreatedRelative }} </span>
      <span v-else class="text-xs">{{ timeCreatedRelative }}, </span>
      <span v-if="props.mod" class="text-xs text-gray-600">
        in
        <NuxtLink
          :to="`/mods/${props.mod.urlAlias || props.mod.id}`"
          class="underline hover:text-gray-500"
          >{{ props.mod.name }}</NuxtLink
        >
      </span>
    </div>
    <div
      class="mod-comment__body bg-secondary px-2 py-1 max-h-32 overflow-y-auto"
    >
      <v-comment-renderer :value="props.comment.comment" />
    </div>
  </div>
</template>
