<script setup lang="ts">
import type { LatestModCommentModel } from '~/models/mods/LatestModCommentModel';
import type { ModDisplayModel } from '~/models/mods/ModDisplayModel';
import ModComment from '~/components/mods/ModComment.vue';
import useAuthStore from '~/store/auth';

const auth = useAuthStore();

const latestMods = useFetch<ModDisplayModel[]>('/api/v1/mods/latest');
const latestModComments = useFetch<LatestModCommentModel[]>(
  '/api/v1/mods/latest/comments'
);
</script>

<template>
  <div class="home flex flex-col justify-start align-start">
    <div v-if="!auth.isLoggedIn">
      <h1 class="text-xl mb-2">
        Welcome to the unofficial mod repository for Vintage Story!
      </h1>
      <p>
        The goal of this site is to provide a complete, modern rewrite of the
        <NuxtLink
          to="https://mods.vintagestory.at"
          class="text-blue-600 hover:text-blue-700 transition-colors"
          >Vintage Story ModDB</NuxtLink
        >
        using .NET and Vue.js.
      </p>
    </div>
    <div v-else>Your mods</div>
    <div class="flex justify-start gap-2 align-center mt-4 mb-2">
      <h2 class="text-xl">Latest 10 mods</h2>
      <spinner v-if="latestMods.status.value === 'pending'" />
      <error-icon
        v-if="latestModComments.status.value === 'error'"
        tooltip="An error occurred while loading the latest mods"
      />
    </div>
    <div
      v-if="latestMods.data.value"
      class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 2xl:grid-cols-5 gap-2"
    >
      <mod-card v-for="mod of latestMods.data.value" :key="mod.id" :mod="mod" />
    </div>
    <div class="flex justify-start gap-2 align-center mt-4 mb-2">
      <h2 class="text-xl">Latest 20 comments</h2>
      <spinner v-if="latestModComments.status.value === 'pending'" />
      <error-icon
        v-if="latestModComments.status.value === 'error'"
        tooltip="An error occurred while loading the latest comments"
      />
    </div>
    <div v-if="latestModComments.data.value" class="flex flex-col gap-2">
      <mod-comment
        v-for="comment of latestModComments.data.value"
        :key="comment.comment.id"
        :comment="comment.comment"
        :mod="comment.mod"
      />
    </div>
  </div>
</template>
