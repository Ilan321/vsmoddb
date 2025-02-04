<script setup lang="ts">
import {
  createPopper,
  type Instance,
  type Placement,
  type PositioningStrategy
} from '@popperjs/core';
import type { OffsetsFunction } from '@popperjs/core/lib/modifiers/offset';

const props = withDefaults(
  defineProps<{
    placement?: Placement;
    strategy?: PositioningStrategy;
    offset?: number;
  }>(),
  {
    placement: 'bottom-end',
    strategy: 'absolute',
    offset: 0
  }
);

const activator = ref(null! as HTMLElement);
const tooltip = ref(null! as HTMLElement);

const _popper = ref(null! as Instance);
const popper = computed(() => {
  if (!_popper.value) {
    _popper.value = createPopper(activator.value, tooltip.value, {
      placement: props.placement,
      strategy: props.strategy,
      modifiers: [
        {
          name: 'offset',
          options: {
            offset: [0, props.offset]
          }
        }
      ]
    });
  }

  return _popper.value;
});

function show() {
  tooltip.value.setAttribute('data-show', '');

  // We need to tell Popper to update the tooltip position
  // after we show the tooltip, otherwise it will be incorrect
  popper.value.update();
}

function hide() {
  tooltip.value.removeAttribute('data-show');
}
</script>

<template>
  <div
    class="v-menu"
    @mouseenter="show"
    @mouseleave="hide"
    @focus="show"
    @blur="hide"
  >
    <div ref="activator" class="v-menu__activator flex">
      <slot name="activator"></slot>
    </div>
    <div ref="tooltip" class="v-menu__content">
      <slot></slot>
    </div>
  </div>
</template>

<style>
.v-menu {
  .v-menu__content {
    display: none;
  }

  .v-menu__content[data-show] {
    display: block;
  }
}
</style>
