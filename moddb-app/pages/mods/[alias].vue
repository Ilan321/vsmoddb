<script setup lang="ts">
import type { ModCommentModel } from '~/models/mods/ModCommentModel';
import type { ModDetailsModel } from '~/models/mods/ModDetailsModel';
import ModComment from '~/components/mods/ModComment.vue';

const route = useRoute();

const modAlias = computed(() => route.params.alias as string);

const breadcrumbName = computed(() => {
  if (modDetails.status.value === 'pending') {
    return 'loading...';
  }

  return modDetails.data.value!.name;
});

const apiCalls = computed(() => [modDetails, comments]);

// TODO: handle loading errors

const loading = computed(() =>
  apiCalls.value.some((s) => s.status.value === 'pending')
);

const loadingError = computed(() =>
  apiCalls.value.some((s) => s.status.value === 'error')
);

const modDetails = useFetch<ModDetailsModel>(`/api/v1/mods/${modAlias.value}`, {
  ignoreResponseError: true
});
const comments = useFetch<ModCommentModel[]>(
  `/api/v1/mods/${modAlias.value}/comments`,
  {
    ignoreResponseError: true
  }
);
</script>

<template>
  <div class="mod-page">
    <breadcrumbs
      class="mb-2"
      :items="[
        {
          name: 'Mods',
          url: '/mods'
        },
        {
          name: breadcrumbName
        }
      ]"
    />
    <div class="tabs flex flex-row border-b-2 border-black/20">
      <tab :to="`/mods/${modAlias}`"> Description </tab>
      <tab :to="`/mods/${modAlias}/files`"> Files </tab>
    </div>
    <NuxtPage />
    <div class="mod-page__comments">
      <div class="mod-page__comments-header">
        <h3 v-if="comments.status.value === 'pending'">Loading comments..</h3>
        <template v-else>
          <h3 class="text-lg mb-2">
            {{ comments.data.value!.length }} comments
          </h3>
          <div
            class="mod-page__comments-container flex flex-col justify-start items-start gap-2"
          >
            <mod-comment
              v-for="comment of comments.data.value!"
              :key="comment.id"
              :comment="comment"
            />
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
