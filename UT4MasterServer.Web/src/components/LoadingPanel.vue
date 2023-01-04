<template>
  <div class="loading-panel-container">
    <div class="loading-panel" v-show="asyncStatus === AsyncStatus.BUSY">
      <div class="spinner" />
    </div>
    <div class="content">
      &nbsp;
      <slot />
    </div>
    <div class="alert alert-dismissible alert-danger" v-show="asyncStatus === AsyncStatus.ERROR">
      <button type="button" class="btn-close" @click="dismissError"></button>
      <slot name="error" />
      <div v-if="!$slots.error">An error occurred. Please try again.</div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.loading-panel-container {
  position: relative;

  .loading-panel {
    position: absolute;
    width: 100%;
    height: 100%;
    z-index: 1;
    background-color: white;
    opacity: 0.50;
    display: flex;
    align-items: center;
    justify-content: center;

    .spinner {
      background-image: url(../assets/loading.gif);
      background-size: contain;
      height: 50px;
      width: 50px;
      z-index: 10;
    }
  }
}
</style>
  
<script setup lang="ts">
import { PropType, ref, watch, onUnmounted } from 'vue';
import { AsyncStatus } from '../types/async-status';

const props = defineProps({
  status: Number as PropType<AsyncStatus>
})

const asyncStatus = ref(props.status);

const destroyWatch = watch(() => props.status, () => {
  asyncStatus.value = props.status;
})

function dismissError() {
  asyncStatus.value = AsyncStatus.OK;
}

onUnmounted(destroyWatch);
</script>