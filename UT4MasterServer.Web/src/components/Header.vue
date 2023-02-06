<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
      <a class="navbar-brand" href="/">UT4 Master Server</a>
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarColor02"
        aria-controls="navbarColor02"
        :aria-expanded="menuExpanded"
        aria-label="Toggle navigation"
      >
        <span
          class="navbar-toggler-icon"
          @click="menuExpanded = !menuExpanded"
        ></span>
      </button>
      <div class="collapse navbar-collapse" :class="{ show: menuExpanded }">
        <ul class="navbar-nav me-auto">
          <li class="nav-item dropdown pull-right">
            <a
              class="nav-link dropdown-toggle"
              href="#"
              role="button"
              aria-haspopup="true"
              aria-expanded="false"
              @click.stop="showInstructionsDropdown = !showInstructionsDropdown"
              >Instructions</a
            >
            <div
              class="dropdown-menu"
              :class="{ show: showInstructionsDropdown }"
            >
              <HeaderLink
                text="Stock UT4"
                path="/Instructions/StockUT4"
                dropdown
              />
              <HeaderLink text="UT4UU" path="/Instructions/UT4UU" dropdown />
              <HeaderLink
                text="Hub Owners"
                path="/Instructions/HubOwners"
                dropdown
              />
            </div>
          </li>
          <HeaderLink text="Servers" path="/Servers" />
          <HeaderLink text="Rankings" path="/Rankings" />
          <a href="https://discord.gg/2DaCWkK" class="nav-link" target="_blank"
            >Discord</a
          >
          <a
            href="https://github.com/timiimit/UT4MasterServer"
            class="nav-link"
            target="_blank"
            >GitHub</a
          >
          <template v-if="!SessionStore.isAuthenticated">
            <HeaderLink text="Register" path="/Register" />
            <HeaderLink text="Log In" path="/Login" />
          </template>
          <template v-else>
            <HeaderLink text="Stats" path="/Stats" />
          </template>
          <li v-if="AccountStore.isAdmin" class="nav-item dropdown pull-right">
            <a
              class="nav-link dropdown-toggle"
              href="#"
              role="button"
              aria-haspopup="true"
              aria-expanded="false"
              @click.stop="showAdminDropdown = !showAdminDropdown"
              >Admin</a
            >
            <div class="dropdown-menu" :class="{ show: showAdminDropdown }">
              <HeaderLink text="Accounts" path="/Admin/Accounts" dropdown />
              <HeaderLink text="Clients" path="/Admin/Clients" dropdown />
              <HeaderLink
                text="Trusted Servers"
                path="/Admin/TrustedServers"
                dropdown
              />
            </div>
          </li>
          <li
            v-if="SessionStore.isAuthenticated"
            class="nav-item dropdown pull-right"
          >
            <a
              class="nav-link dropdown-toggle"
              href="#"
              role="button"
              aria-haspopup="true"
              aria-expanded="false"
              @click.stop="showProfileDropdown = !showProfileDropdown"
              >Profile</a
            >
            <div class="dropdown-menu" :class="{ show: showProfileDropdown }">
              <HeaderLink
                text="Player Card"
                path="/Profile/PlayerCard"
                dropdown
              />
              <div class="dropdown-divider" />
              <HeaderLink
                text="Change Username"
                path="/Profile/ChangeUsername"
                dropdown
              />
              <HeaderLink
                text="Change Password"
                path="/Profile/ChangePassword"
                dropdown
              />
              <HeaderLink
                text="Change Email"
                path="/Profile/ChangeEmail"
                dropdown
              />
              <div class="dropdown-divider" />
              <button class="dropdown-item" @click="logOut">Log Out</button>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </nav>
  <UserInfo v-if="SessionStore.isAuthenticated" />
</template>

<style lang="scss" scoped>
.navbar-nav {
  width: 100%;
  justify-content: flex-end;

  .nav-link.router-link-active,
  .navbar-nav .show > .nav-link {
    color: var(--bs-navbar-active-color);
  }

  .nav-item.pull-right {
    margin-right: 0;
  }

  .router-link-active {
    font-weight: 700;
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
import AuthenticationService from '@/services/authentication.service';
import { SessionStore } from '@/stores/session-store';
import { AccountStore } from '@/stores/account-store';

const router = useRouter();
const showProfileDropdown = shallowRef(false);
const showInstructionsDropdown = shallowRef(false);
const showAdminDropdown = shallowRef(false);
const authenticationService = new AuthenticationService();
const menuExpanded = shallowRef(false);

async function logOut() {
  await authenticationService.logOut();
  router.push('/Login');
}

function closeNav() {
  showProfileDropdown.value = false;
  showInstructionsDropdown.value = false;
  showAdminDropdown.value = false;
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
