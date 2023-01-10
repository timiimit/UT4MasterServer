<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
      <a class="navbar-brand" href="#">UT4MasterServer</a>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarColor02"
        aria-controls="navbarColor02" aria-expanded="false" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarColor02">
        <ul class="navbar-nav me-auto">
          <template v-if="!SessionStore.isAuthenticated">
            <HeaderLink text="Register" path="/Register" />
            <HeaderLink text="Log In" path="/Login" />
          </template>

          <li class="nav-item dropdown pull-right" v-if="SessionStore.isAuthenticated">
            <a class="nav-link dropdown-toggle" href="#" role="button" aria-haspopup="true" aria-expanded="false"
              @click.stop="showProfileDropdown = !showProfileDropdown">Profile</a>
            <div class="dropdown-menu" :class="{ 'show': showProfileDropdown }">
              <HeaderLink text="Stats" path="/Profile/Stats" dropdown />
              <HeaderLink text="Change Username" path="/Profile/ChangeUsername" dropdown />
              <HeaderLink text="Change Password" path="/Profile/ChangePassword" dropdown />
              <div class="dropdown-divider"></div>
              <button class="dropdown-item" @click="logOut">Log Out</button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </nav>
  <UserInfo />
</template>

<style lang="scss" scoped>
.navbar-nav {
  width: 100%;
  justify-content: flex-end;

  .nav-link.router-link-active,
  .navbar-nav .show>.nav-link {
    color: var(--bs-navbar-active-color);
  }
}

.user-info {
  padding: 10px 0;
  text-transform: none;
  font-size: 1rem;

  label {
    text-transform: uppercase;
    font-size: 0.6rem;
    margin-right: 5px;
  }
}
</style>
  
<script setup lang="ts">
import { onMounted, onUnmounted, shallowRef } from 'vue';
import HeaderLink from './HeaderLink.vue';
import { useRouter } from 'vue-router';
import UserInfo from './UserInfo.vue';
import AuthenticationService from '../services/authentication.service';
import { SessionStore } from '../stores/session-store';

const router = useRouter();
const showProfileDropdown = shallowRef(false);
const authenticationService = new AuthenticationService()

function logOut() {
  authenticationService.logOut();
  router.push('/Login');
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