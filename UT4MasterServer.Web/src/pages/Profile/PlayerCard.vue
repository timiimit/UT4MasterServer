<!-- 
    Rough replacement for the playerCard UI, e.g.
  https://www.epicgames.com/unrealtournament/en-US/playerCard?playerId=0b0f09b400854b9b98932dd9e5abe7c5
  -->
<template>
  <h1>Player Card</h1>
  <div v-if="AccountStore.account" class="row">
    <div v-if="!emailVerified">
      <div
        v-if="
          verificationLinkSent ||
          (verificationLinkGUID && !verificationLinkExpired)
        "
        class="alert alert-success"
      >
        <div>
          Verification link sent. Check your email and then refresh the page.
        </div>
      </div>
      <div v-else class="alert alert-danger">
        <LoadingPanel
          :status="status"
          error="Error occurred while sending verification link!"
        >
          <div>
            Email is not verified. Click
            <button
              type="button"
              class="btn btn-link p-0"
              @click="sendVerificationLink"
            >
              here
            </button>
            to send the verification link.
          </div>
        </LoadingPanel>
      </div>
    </div>
    <div class="col-sm-6">
      <table class="table table-hover">
        <tbody>
          <tr class="table-primary">
            <th scope="row">
              <img
                class="avatar"
                :src="`/assets/avatars/${AccountStore.account.avatar}.png`"
              />
              {{ AccountStore.account.username }}
            </th>
            <td>
              <img
                class="flag"
                :src="`/assets/flags/${AccountStore.account.countryFlag}.png`"
              />
              {{ AccountStore.account.countryFlag }}
            </td>
          </tr>
          <tr class="table-primary">
            <th scope="row">Level (Experience)</th>
            <td>
              {{ AccountStore.account.level }} ({{ AccountStore.account.xp }})
            </td>
          </tr>
          <tr class="table-primary">
            <th scope="row">Challenge Stars</th>
            <td>
              {{ AccountStore.account.blueStars
              }}<span class="blue star">★</span
              >{{ AccountStore.account.goldStars
              }}<span class="gold star">★</span>
            </td>
          </tr>
          <tr class="table-primary">
            <th scope="row">ID</th>
            <td>{{ AccountStore.account.id }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.table td {
  vertical-align: middle;
}

img.avatar {
  width: 40px;
}

.star {
  font-size: 1.25rem;
  line-height: 1rem;
  padding-right: 1rem;

  &.blue {
    color: blue;
  }

  &.gold {
    color: gold;
  }
}
</style>

<script lang="ts" setup>
import { shallowRef, computed } from 'vue';
import { AccountStore } from '@/stores/account-store';
import AccountService from '@/services/account.service';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { Role } from '@/enums/role';

const status = shallowRef(AsyncStatus.OK);
const emailVerified = computed(() =>
  AccountStore.account?.roles?.some((r) => r === Role.EmailVerified)
);
const verificationLinkGUID = computed(
  () => AccountStore.account?.verificationLinkGUID
);
const verificationLinkExpired = computed(
  () =>
    AccountStore.account?.verificationLinkExpiration &&
    new Date(AccountStore.account?.verificationLinkExpiration).getTime() <
      Date.now()
);
const verificationLinkSent = shallowRef(false);

const accountService = new AccountService();

async function sendVerificationLink() {
  try {
    status.value = AsyncStatus.BUSY;

    const formData = {
      email: AccountStore.account?.email!
    };

    await accountService.resendVerificationLink(formData);

    status.value = AsyncStatus.OK;
    verificationLinkSent.value = true;
  } catch (err: unknown) {
    console.error('Error occurred while sending verification link:', err);
    status.value = AsyncStatus.ERROR;
  }
}
</script>
