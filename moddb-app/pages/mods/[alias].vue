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
        <h3 v-if="store.comments.loading.value">Loading comments..</h3>
        <template v-else>
          <h3 class="text-lg mb-2">
            {{ store.comments.value.length }} comments
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
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
