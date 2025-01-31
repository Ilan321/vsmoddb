<script setup lang="ts">
import ModComment from '~/components/mods/ModComment.vue';
import Breadcrumbs from '~/components/Breadcrumbs.vue';
import useModDetailsStore from '~/store/mod-details';

const route = useRoute();
const store = useModDetailsStore();

const pageName = computed(() => {
  if (store.loading.value) {
    return 'loading..';
  }

  return store.mod.name!;
});

store.initAsync(route.params.alias as string);

useTitle(
  computed(() => {
    if (store.loading.value) {
      return store.alias;
    }

    return store.mod.name!;
  })
);
</script>

<template>
  <div class="mod-page">
    <breadcrumbs
      class="mb-2"
      :items="[
        {
          name: 'Mods',
          url: '/mods',
          hideOnMobile: true
        },
        {
          name: pageName
        }
      ]"
    />
    <div
      v-if="!store.loading.value"
      class="tabs flex flex-row border-b-2 border-black/20"
    >
      <tab :to="`/mods/${store.alias}`"> Description </tab>
      <tab :to="`/mods/${store.alias}/files`"> Files </tab>
    </div>
    <NuxtPage v-if="!store.loading.value" />
    <div class="mod-page__comments" id="comments">
      <div class="mod-page__comments-header">
        <h3 class="text-lg mb-2">
          {{ store.comments.value.length }} comments
          <span class="text-sm">(out of {{ store.comments.total }}) </span>
        </h3>
        <div
          class="mod-page__comments-container flex flex-col justify-start items-start gap-2"
        >
          <mod-comment
            v-for="comment of store.comments.value"
            :key="comment.id"
            :comment="comment"
            :author="comment.author === store.mod.author"
          />
          <div class="w-full flex justify-center">
            <v-button
              v-if="store.comments.total > store.comments.value.length"
              @click="store.loadComments"
              :loading="store.comments.loading.value"
              :disabled="store.comments.loading.value"
              class="mt-2"
            >
              Load more
            </v-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
