<template>
  <Header />
  <div class="container main-content">
    <router-view />
  </div>
  <Footer />
</template>

<script setup lang="ts">
import Header from './components/Header.vue';
import Footer from './components/Footer.vue';
</script>

<style lang="scss" scoped>
.container {
  padding: 1rem;
}
</style>

<style lang="scss" src="./styles.scss" />
