<script setup lang="ts">
import { ModCommentContentType } from '~/models/enums/ModCommentContentType';

const props = withDefaults(
  defineProps<{
    type?: ModCommentContentType;
    value: string;
  }>(),
  {
    type: ModCommentContentType.Html
  }
);

const content = ref(null! as HTMLDivElement);

const isHtml = computed(() => props.type === ModCommentContentType.Html);
const isMarkdown = computed(
  () => props.type === ModCommentContentType.Markdown
);

function handleClick(element: HTMLElement) {
  console.log({ element });

  if (element.classList.contains('spoiler-toggle')) {
    element.parentElement?.classList.toggle('expanded');
  }
}
</script>

<template>
  <div class="v-comment-renderer" ref="content">
    <div
      v-if="isHtml"
      v-html="props.value"
      @click="handleClick($event.target as HTMLElement)"
    ></div>
    <MDC v-else-if="isMarkdown" :value="props.value" />
  </div>
</template>

<style>
@reference "@/main.css";

.v-comment-renderer {
  p {
    @apply not-last:mb-2;
  }

  a {
    @apply text-blue-700 hover:text-blue-500;
    @apply underline;
  }

  .spoiler {
    @apply px-2 py-1;
    @apply border border-gray-500;
    @apply bg-gray-100;
    @apply mb-2;

    .spoiler-toggle {
      @apply font-bold;
      @apply cursor-pointer;
      @apply select-none;

      span {
        @apply pointer-events-none;
      }
    }

    .spoiler-toggle:before {
      @apply font-mono;
      @apply me-1;
      content: '►';
    }

    .spoiler-text {
      @apply hidden;
    }
  }

  .spoiler.expanded {
    .spoiler-toggle:before {
      content: '▼';
    }

    .spoiler-text {
      @apply block;
    }
  }

  h1 {
    font-size: var(--text-2xl);
  }
  h2 {
    font-size: var(--text-xl);
  }
  h3 {
    font-size: var(--text-lg);
  }
  h4 {
    font-size: var(--text-base);
  }
  h5 {
    font-size: var(--text-sm);
  }
  h6 {
    font-size: var(--text-xs);
  }

  li {
    @apply list-disc;
    @apply ms-8;
  }

  table {
    @apply w-full;
    @apply table-auto;
    @apply border-collapse;
    @apply border border-gray-300;

    thead {
      @apply bg-gray-200;

      tr {
        th {
          @apply text-start;
          @apply px-2 py-1;
        }
      }
    }

    tbody {
      tr {
        td {
          @apply border border-gray-300;
          @apply px-2 py-1;
        }
      }
    }
  }

  .mention.username {
    background-color: rgba(159, 207, 52, 0.5);
    @apply rounded;
    @apply px-1 py-0;
    @apply me-1;
  }

  .mention.username:before {
    content: '@';
  }
}
</style>
