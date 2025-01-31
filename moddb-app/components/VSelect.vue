<script setup lang="ts">
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/vue';

const props = defineProps<{
  items: { text: string; value: string }[];
  modelValue?: string;
  textOnly?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:model-value', value?: string): void;
}>();

const selectedItem = computed(() =>
  props.modelValue
    ? props.items.find((f) => f.value === props.modelValue)
    : undefined
);
</script>

<template>
  <Menu as="div" class="relative inline-block text-left">
    <div>
      <slot name="trigger">
        <MenuButton
          :class="[
            textOnly
              ? 'underline'
              : `inline-flex w-full justify-center gap-x-1.5 rounded-md bg-white px-3 py-2
                text-sm text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50`
          ]"
        >
          <span class="grow">
            {{ selectedItem?.text }}
          </span>
          <font-awesome
            icon="chevron-down"
            :class="[
              '-mr-1 size-5 pt-0.5 text-gray-400',
              textOnly ? 'text-sm' : ''
            ]"
            aria-hidden="true"
          />
        </MenuButton>
      </slot>
    </div>
    <transition
      enter-active-class="transition ease-out duration-100"
      enter-from-class="transform opacity-0 scale-95"
      enter-to-class="transform opacity-100 scale-100"
      leave-active-class="transition ease-in duration-75"
      leave-from-class="transform opacity-100 scale-100"
      leave-to-class="transform opacity-0 scale-95"
    >
      <slot name="dropdown">
        <MenuItems
          class="absolute left-0 md:left-auto md:right-0 z-10 mt-2 w-56 origin-top-right
            rounded-md bg-white shadow-lg ring-1 ring-black/5 focus:outline-none"
        >
          <div class="py-1">
            <MenuItem
              v-for="item of items"
              :key="item.value"
              v-slot="{ active }"
            >
              <a
                :class="[
                  active
                    ? 'bg-gray-100 text-gray-900 outline-none'
                    : 'text-gray-700',
                  'block px-4 py-2 text-sm'
                ]"
                @click="emit('update:model-value', item.value)"
              >
                {{ item.text }}
              </a>
            </MenuItem>
          </div>
        </MenuItems>
      </slot>
    </transition>
  </Menu>
</template>
