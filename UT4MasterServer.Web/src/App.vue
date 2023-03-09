<template>
  <Header />
  <div class="container main-content">
    <router-view :key="$route.fullPath" />
  </div>
  <Footer />
</template>

<style lang="scss" scoped>
.container {
  padding: 1rem;
}
</style>

<style lang="scss" src="./styles.scss" />

<script setup lang="ts">
import Header from './components/Header.vue';
import Footer from './components/Footer.vue';
</script>
