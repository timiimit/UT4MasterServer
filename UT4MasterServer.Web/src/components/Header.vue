<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
      <a class="navbar-brand" href="#">UT4UU</a>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarColor02"
        aria-controls="navbarColor02" aria-expanded="false" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarColor02">
        <ul class="navbar-nav me-auto">
          <!-- <li class="nav-item">
            <HeaderLink text="Profile" path="/Profile" />
          </li> -->
          <li class="nav-item dropdown pull-right">
            <a class="nav-link dropdown-toggle" href="#" role="button" aria-haspopup="true" aria-expanded="false" @click.stop="showProfileDropdown = !showProfileDropdown">Profile</a>
            <div class="dropdown-menu" :class="{'show': showProfileDropdown }">
              <HeaderLink text="Stats" path="/Profile/Stats" dropdown />
              <HeaderLink text="Change Username" path="/Profile/ChangeUsername" dropdown />
              <HeaderLink text="Change Password" path="/Profile/ChangePassword" dropdown />
              <div class="dropdown-divider"></div>
              <a class="dropdown-item" @click="logOut">Log Out</a>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </nav>
</template>

<style lang="scss" scoped>
.navbar-nav {
  width: 100%;
  justify-content: flex-end;
  .nav-link.router-link-active, .navbar-nav .show>.nav-link {
    color: var(--bs-navbar-active-color);
}
}
</style>
  
<script setup lang="ts">
import { onMounted, onUnmounted, shallowRef } from 'vue';
import HeaderLink from './HeaderLink.vue';

const showProfileDropdown = shallowRef(false);

function logOut() {
  console.debug('log out');
}

function closeNav() {
  showProfileDropdown.value = false;
}

onMounted(() => {
  document.addEventListener('keydown', closeNav);
  document.addEventListener('click', closeNav);
});

onUnmounted(() => {
  document.removeEventListener('keydown', closeNav);
  document.removeEventListener('click', closeNav);
});
</script>