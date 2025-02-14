<script setup lang="ts">
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/vue';

interface SelectItem {
  text: string;
  value: string;
}

const props = withDefaults(
  defineProps<{
    items: SelectItem[];
    modelValue?: string | string[];
    textOnly?: boolean;
    label?: string;
    class?: any;
    placeholder?: string;
    maxItemsVisible?: number;
  }>(),
  {
    maxItemsVisible: 2
  }
);

const emit = defineEmits<{
  (e: 'update:model-value', value?: string | string[]): void;
}>();

const valueText = computed(() => {
  if (!Array.isArray(props.modelValue)) {
    return props.modelValue
      ? props.items.find((f) => f.value === props.modelValue)?.text
      : undefined;
  }

  if (!props.modelValue) {
    return '';
  }

  const selectedItems = props.items.filter((f) =>
    props.modelValue!.includes(f.value)
  );

  const items = selectedItems.slice(0, props.maxItemsVisible);

  let text = items.map((f) => f.text).join(', ');

  if (selectedItems.length > props.maxItemsVisible) {
    text += ` (+ ${selectedItems.length - props.maxItemsVisible} more)`;
  }

  return text;
});

function handleClick(item: SelectItem, close: () => void) {
  if (Array.isArray(props.modelValue)) {
    const isInModelValue = props.modelValue.includes(item.value);

    const newValue = isInModelValue
      ? props.modelValue.filter((f) => f !== item.value)
      : [...props.modelValue, item.value];

    emit('update:model-value', newValue);

    return;
  }

  emit('update:model-value', item.value);

  close();
}
</script>

<template>
  <Menu as="div" class="relative inline-block text-left">
    <div :class="props.class">
      <slot name="label">
        <label
          v-if="label"
          class="block text-sm/6 font-medium text-gray-900 mb-2"
        >
          {{ label }}
        </label>
      </slot>
      <slot name="trigger">
        <MenuButton
          :class="[
            textOnly
              ? 'underline'
              : `inline-flex w-full h-9 justify-center gap-x-1.5 rounded-md bg-secondary/90 px-3
                py-2 text-start text-sm text-gray-900 outline outline-1 -outline-offset-1
                outline-gray-300 placeholder:text-gray-400 hover:bg-gray-50`
          ]"
        >
          <slot name="trigger-content" :text="valueText">
            <span v-if="valueText" class="grow">
              {{ valueText }}
            </span>
            <span v-else-if="placeholder" class="grow text-gray-400">
              {{ placeholder }}
            </span>
          </slot>
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
              v-slot="{ active, close }"
            >
              <a
                class="flex items-center max-w-full truncate"
                :class="[
                  active
                    ? 'bg-gray-100 text-gray-900 outline-none'
                    : 'text-gray-700',
                  'block px-4 py-2 text-sm'
                ]"
                @click.prevent="handleClick(item, close)"
              >
                <span class="grow">
                  {{ item.text }}
                </span>
                <font-awesome
                  v-if="
                    Array.isArray(props.modelValue) &&
                    props.modelValue.includes(item.value)
                  "
                  icon="check"
                />
              </a>
            </MenuItem>
          </div>
        </MenuItems>
      </slot>
    </transition>
  </Menu>
</template>
