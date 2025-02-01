<script setup lang="ts">
import dayjs from 'dayjs';
import useModDetailsStore from '~/store/mod-details';

const store = useModDetailsStore();

function getReadableTimeCreated(time: string) {
  return dayjs(time).fromNow();
}
</script>

<template>
  <div class="py-2">
    <table class="hidden md:block table-auto border-collapse">
      <thead>
        <tr class="border-b border-primary">
          <th class="px-2 py-1 border text-start">Version</th>
          <th class="px-2 py-1 border text-start">For game version(s)</th>
          <th class="px-2 py-1 border text-start">Downloads</th>
          <th class="px-2 py-1 border text-start">Release time</th>
          <th class="px-2 py-1 border text-start">Changelog</th>
          <th class="px-2 py-1 border text-start">Download</th>
          <th class="px-2 py-1 border text-start">1-click mod install</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="file of store.mod.releases"
          :key="file.modVersion"
          class="border-b border-primary not-even:bg-primary"
        >
          <td class="px-2 py-1 border">{{ file.modVersion }}</td>
          <td class="px-2 py-1 border">
            <span
              v-for="version of getReadableGameVersions(file.gameVersions)"
              class="bg-gray-300 px-1 py-0.5 rounded not-last:me-1 border border-gray-400"
              :title="version.tooltip"
            >
              {{ version.version }}
            </span>
          </td>
          <td class="px-2 py-1 border">{{ file.downloads }}</td>
          <td class="px-2 py-1 border">
            {{ getReadableTimeCreated(file.timeCreatedUtc) }}
          </td>
          <td class="px-2 py-1 border">
            <code>// TODO</code>
          </td>
          <td class="px-2 py-1 border">
            <a
              :href="`/api/v1/mods/${store.alias}/releases/${file.modVersion}`"
              class="link-blue"
            >
              {{ file.fileName }}
            </a>
          </td>
          <td class="px-2 py-1 border">
            <!-- vintagestorymodinstall://primitivesurvival@3.7.5 -->
            <a
              :href="`vintagestorymodinstall://${store.alias}@${file.modVersion}`"
              class="link-blue"
            >
              Install now
            </a>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="md:hidden w-full">
      <div
        v-for="file of store.mod.releases"
        :key="file.modVersion"
        class="rounded px-2 py-1 border border-primary not-last:mb-2 shadow"
      >
        <div class="flex justify-between">
          <div class="text-lg font-semibold">
            {{ file.modVersion }}
          </div>
          <div class="text-sm flex items-center">
            <span
              v-for="version of getReadableGameVersions(file.gameVersions)"
              class="bg-gray-300 px-1 py-0.5 rounded not-last:me-1 border border-gray-400"
              :title="version.tooltip"
            >
              {{ version.version }}
            </span>
          </div>
        </div>
        <div class="mt-1">
          <font-awesome icon="download" />
          <span class="ms-2">{{ file.downloads }}</span>
        </div>
        <div>
          <font-awesome icon="clock" />
          <span class="ms-2">
            {{ getReadableTimeCreated(file.timeCreatedUtc) }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>
