<template>
  <h1>Player Card</h1>
  <div class="row">
    <div class="col-sm-6">
      <table class="table table-hover">
        <tbody>
          <tr class="table-primary">
            <th scope="row">
              <img class="avatar" :src="`/assets/avatars/${playerCard?.Avatar ?? 'UT.Avatar.1'}.png`" />
              {{ playerCard?.Username }}
            </th>
            <td>
              <img class="flag" :src="`/assets/flags/${playerCard?.CountryFlag ?? 'Unreal'}.png`" />
              {{ playerCard?.CountryFlag }}
            </td>
          </tr>
          <tr class="table-primary">
            <th scope="row">
              Level (Experience)
            </th>
            <td>
              {{ playerCard?.Level }} ({{ playerCard?.XP }})
            </td>
          </tr>
          <tr class="table-primary">
            <th scope="row">Challenge Stars</th>
            <td>{{ playerCard?.BlueStars }}<span class="blue star">★</span>{{ playerCard?.GoldStars }}<span
                class="gold star">★</span></td>
          </tr>
          <tr class="table-primary">
            <th scope="row">ID</th>
            <td>{{ playerCard?.ID }}</td>
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
import { SessionStore } from '../../stores/session-store';
import { onMounted, shallowRef } from 'vue';
import CustomService from '../../services/custom.service';
import { IPlayerCard } from '../../types/player-card';

const customService = new CustomService();

const playerCard = shallowRef<IPlayerCard | undefined>(undefined);

async function fetchProfile() {
  if (SessionStore.session) {
    playerCard.value = await customService.getPlayerCard(SessionStore.session.account_id);
  }
}

onMounted(fetchProfile);

</script>