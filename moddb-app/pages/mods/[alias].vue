<script setup lang="ts">
import ModComment from '~/components/mods/ModComment.vue';
import Breadcrumbs from '~/components/Breadcrumbs.vue';
import useModDetailsStore from '~/store/mod-details';
import { ModSortDirection as SortDirection } from '~/store/mods';

const router = useRouter();
const route = useRoute();
const store = useModDetailsStore();

const pageName = computed(() => {
  if (store.loading.value) {
    return 'loading..';
  }

  return store.mod.name!;
});

useTitle(
  computed(() => {
    if (store.loading.value) {
      return store.alias;
    }

    return store.mod.name!;
  })
);

function scrollToTop() {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  });
}

const commentSortItems = [
  {
    text: 'newest first',
    value: SortDirection.DESC
  },
  {
    text: 'oldest first',
    value: SortDirection.ASC
  }
];

async function initAsync() {
  const result = await store.initAsync(route.params.alias as string);

  if (!result.success) {
    // Handle error

    if (result.statuscode === 404) {
      console.log(`got 404 for ${route.fullPath}`);

      return router.push(`/404?path=${route.fullPath}`);
    }
  }
}

initAsync();
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
      <tab
        v-if="store.mod.homepageUrl"
        :to="store.mod.homepageUrl"
        target="_blank"
      >
        Home page
        <font-awesome icon="up-right-from-square" class="ms-1 text-xs" />
      </tab>
      <tab
        v-if="store.mod.sourceCodeUrl"
        :to="store.mod.sourceCodeUrl"
        target="_blank"
      >
        Source code
        <font-awesome icon="up-right-from-square" class="ms-1 text-xs" />
      </tab>
      <tab
        v-if="store.mod.issuerTrackerUrl"
        :to="store.mod.issuerTrackerUrl"
        target="_blank"
      >
        Issuer tracker
        <font-awesome icon="up-right-from-square" class="ms-1 text-xs" />
      </tab>
      <tab v-if="store.mod.wikiUrl" :to="store.mod.wikiUrl" target="_blank">
        Wiki
        <font-awesome icon="up-right-from-square" class="ms-1 text-xs" />
      </tab>
    </div>
    <NuxtPage v-if="!store.loading.value" />
    <div v-if="!store.loading.value" class="mod-page__comments" id="comments">
      <div class="mod-page__comments-header">
        <h3 class="text-lg mb-2 flex items-baseline gap-1">
          {{ store.comments.value.length }} comments
          <span
            class="text-sm"
            v-if="
              store.comments.value.length < store.comments.total &&
              store.comments.total > 0
            "
            >(out of {{ store.comments.total }})
          </span>
          <v-select
            v-if="store.comments.total > 0"
            text-only
            :items="commentSortItems"
            v-model="store.comments.sort"
            @update:model-value="
              store.loadComments({ reset: true, clear: false })
            "
          >
            <template #trigger-content="{ text }">
              <span class="grow text-sm text-gray-500">{{ text }}</span>
            </template>
          </v-select>
          <spinner v-if="store.comments.loading.value" class="text-sm pt-0.5" />
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
          <div
            class="w-full flex flex-col md:flex-row justify-center items-center gap-2"
          >
            <v-button
              v-if="
                store.comments.value.length > 0 &&
                store.comments.total > store.comments.value.length
              "
              @click="store.loadComments"
              :loading="store.comments.loading.value"
              :disabled="store.comments.loading.value"
            >
              Load more ({{
                store.comments.total - store.comments.value.length
              }}
              comments left)
            </v-button>
            <span
              class="text-sm text-gray-500 cursor-pointer underline"
              v-if="store.comments.value.length > 0"
              @click="scrollToTop"
            >
              back to top
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
