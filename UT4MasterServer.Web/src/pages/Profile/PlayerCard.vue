<!-- 
    Rough replacement for the playerCard UI, e.g.
  https://www.epicgames.com/unrealtournament/en-US/playerCard?playerId=0b0f09b400854b9b98932dd9e5abe7c5
  -->
<template>
  <h1>Player Card</h1>
  <div v-if="AccountStore.account" class="row">
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
import { AccountStore } from '@/stores/account-store';
</script>
