<template>
  <h1>{{ title }}</h1>

  <transition mode="out-in" name="cross-fade">
    <div v-if="adding && $slots.add">
      <slot name="add" :cancel="cancelAdd" />
    </div>
    <div v-else>
      <div class="header">
        <div class="filters d-flex justify-content-between align-items-center">
          <slot name="filters" />
        </div>
        <div class="action-buttons">
          <slot name="action-buttons" />
          <button
            v-if="$slots.add"
            class="btn btn-lg btn-icon"
            title="Add"
            @click="adding = true"
          >
            <FontAwesomeIcon icon="fa-solid fa-plus" />
          </button>
        </div>
      </div>
      <slot />
    </div>
  </transition>
</template>

<script lang="ts" setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { shallowRef } from 'vue';

defineProps({
  title: {
    type: String,
    required: true,
  },
});

const adding = shallowRef(false);

function cancelAdd() {
  adding.value = false;
}
</script>

<style lang="scss" scoped>
.header {
  width: 100%;
  padding: 1rem 0;
  display: flex;
  justify-content: space-between;
  align-items: center;

  .filters {
    flex-grow: 1;

    :deep(> div) {
      flex-basis: 30%;
    }
  }

  .action-buttons {
    justify-content: flex-end;

    button {
      margin-left: 0.5rem;
    }
  }
}
</style>
