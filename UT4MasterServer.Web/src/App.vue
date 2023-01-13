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

<style lang="scss" src="./styles.scss" />

<script setup lang="ts">
import Header from './components/Header.vue';
</script>
