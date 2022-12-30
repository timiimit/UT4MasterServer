<template>
  <Header />
  <div class="container">
    <router-view />
  </div>
</template>

<style lang="scss" scoped>
.container {
  padding: 2rem;
}
</style>

<script setup lang="ts">
import Header from './components/Header.vue';
</script>
