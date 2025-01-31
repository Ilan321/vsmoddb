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
        <NuxtLink to="https://mods.vintagestory.at" class="link-blue"
          >Vintage Story ModDB</NuxtLink
        >
        using .NET and Vue.js.
      </p>
      <p>
        <span class="underline">This site's currently in "read-only" mode</span
        >, meaning you can't login, submit comments or mods, etc.
      </p>
      <p class="mt-2">
        This site's still very much in alpha, so expect bugs and missing
        features.
      </p>
      <p>
        In the meantime, check out the
        <NuxtLink to="/mods" class="link-blue">new mod list</NuxtLink>, and if
        you're tech-savvy, take a look at the
        <NuxtLink to="https://github.com/Ilan321/vsmoddb" class="link-blue"
          >source code</NuxtLink
        >!
      </p>
      <p class="text-sm mt-2">
        In case it needs to be said: this site is not affiliated with Vintage
        Story or Anego Studios.
      </p>
      <p class="text-sm">
        If you have any questions, suggestions, or concerns - feel free to
        <NuxtLink
          to="https://discordapp.com/channels/@me/238803571867385857"
          class="link-blue"
          >contact me</NuxtLink
        >
        on Discord.
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
    <mod-grid
      v-if="latestMods.status.value === 'success'"
      :mods="latestMods.data.value!"
    />
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
        short
      />
    </div>
  </div>
</template>
