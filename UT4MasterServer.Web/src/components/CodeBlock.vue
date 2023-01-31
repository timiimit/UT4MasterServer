<template>
  <div class="code codeblock">
    <div ref="code">
      <slot />
    </div>
    <CopyButton
      v-if="code"
      :subject="code?.innerText"
      custom-class="btn-smaller"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import CopyButton from './CopyButton.vue';

const code = ref<HTMLElement | undefined>(undefined);
</script>

<style lang="scss" scoped>
.copy-button {
  position: absolute;
  top: 10px;
  right: 10px;
}
</style>
