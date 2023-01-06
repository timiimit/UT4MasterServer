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
          <template v-if="!UserStore.isAuthenticated">
            <HeaderLink text="Register" path="/Register" />
            <HeaderLink text="Log In" path="/Login" />
          </template>

          <li class="nav-item dropdown pull-right" v-if="UserStore.isAuthenticated">
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
  <div class="navbar navbar-primary bg-light user-info" v-if="UserStore.isAuthenticated">
    <div class="container">
      <div>
        <label>Logged in as: </label>{{ UserStore.username }}
      </div>
      <div>
        <label>Authorization Code: </label>{{ UserStore.authCode }}
        <button type="button" class="btn btn-secondary" @click="copyCode">Copy</button>
      </div>
    </div>
  </div>
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
import { UserStore } from '../stores/user-store';
import { onMounted, onUnmounted, shallowRef } from 'vue';
import HeaderLink from './HeaderLink.vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const showProfileDropdown = shallowRef(false);

function logOut() {
  // TODO: call service to kill session
  UserStore.authCode = null;
  router.push('/Login');
}

function closeNav() {
  showProfileDropdown.value = false;
}

function copyCode() {
  if (UserStore.authCode) {
    navigator.clipboard.writeText(UserStore.authCode);
  }
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