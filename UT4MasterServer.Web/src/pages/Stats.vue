<template>
  <h1>Stats</h1>
  <LoadingPanel :status="accountsStatus">
    <div class="form-group row">
      <div class="col-sm-6">
        <label for="accountId" class="col-sm-6 col-form-label">Account</label>
        <Autocomplete :value="accountId" :items="accounts" item-key="id" search-key="displayName"
          @select="handleSelectAccount" />
      </div>
      <div class="col-sm-6">
        <label for="statWindow" class="col-sm-6 col-form-label">Timeframe</label>
        <select class="form-select" v-model="statWindow" @change="handleParameterChange">
          <option :value="window.value" v-for="window in statWindowOptions">
            {{ window.text }}
          </option>
        </select>
      </div>
    </div>
  </LoadingPanel>
  <LoadingPanel :status="statsStatus">
    <template v-if="accountId">
      <h5>Viewing stats for: {{ viewingAccount?.displayName }}</h5>
      <StatSection :data="stats" :section="section" v-for="section in statSections" />
    </template>
    <h5 v-else class="text-center">Select a player to view stats</h5>
  </LoadingPanel>
</template>

<script setup lang="ts">
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { shallowRef, onMounted, computed } from 'vue';
import { SessionStore } from '@/stores/session-store';
import StatsService from '@/services/stats.service';
import { StatisticWindow } from '@/enums/statistic-window';
import { IAccount } from '@/types/account';
import { Statistic } from '@/enums/statistic';
import { IStatisticSection } from '@/types/statistic-config';
import StatSection from '@/components/StatSection.vue';
import { IStatisticData } from '@/types/statistic-data';
import Autocomplete from '@/components/Autocomplete.vue';
import { AccountStore } from '@/stores/account-store';

const statsStatus = shallowRef(AsyncStatus.OK);
const statWindow = shallowRef(StatisticWindow.AllTime);
const stats = shallowRef<IStatisticData[]>([]);

const accountsStatus = shallowRef(AsyncStatus.OK);
const accountId = shallowRef<string | undefined>(undefined);
const accounts = computed(() => AccountStore.accounts ?? []);
const viewingAccount = computed(() => accounts.value.find((a) => a.id === accountId.value));

const statsService = new StatsService();

const statWindowOptions = [
  { text: 'All Time', value: StatisticWindow.AllTime },
  { text: 'Daily', value: StatisticWindow.Daily },
  { text: 'Weekly', value: StatisticWindow.Weekly },
  { text: 'Monthly', value: StatisticWindow.Monthly }
];

const statSections: IStatisticSection[] = [
  {
    heading: 'Overall Stats',
    cards: [
      {
        heading: 'Quick Look',
        stats: [
          Statistic.SkillRating,
          Statistic.TDMSkillRating,
          Statistic.CTFSkillRating,
          Statistic.DMSkillRating,
          Statistic.ShowdownSkillRating,
          Statistic.MatchesPlayed,
          Statistic.MatchesQuit,
          Statistic.TimePlayed,
          Statistic.Wins,
          Statistic.Losses,
          Statistic.Kills,
          Statistic.Deaths,
          Statistic.Suicides
        ]
      },
      {
        heading: 'Kill Achievements',
        stats: [
          Statistic.MultiKillLevel0,
          Statistic.MultiKillLevel1,
          Statistic.MultiKillLevel2,
          Statistic.MultiKillLevel3,
          Statistic.SpreeKillLevel0,
          Statistic.SpreeKillLevel1,
          Statistic.SpreeKillLevel2,
          Statistic.SpreeKillLevel3,
          Statistic.SpreeKillLevel4,
          Statistic.BestShockCombo,
          Statistic.AmazingCombos,
          Statistic.AirRox,
          Statistic.FlakShreds,
          Statistic.AirSnot
        ]
      },
      {
        heading: 'Power Up Achievements',
        stats: [
          Statistic.UDamageTime,
          Statistic.BerserkTime,
          Statistic.InvisibilityTime,
          Statistic.UDamageCount,
          Statistic.BerserkCount,
          Statistic.InvisibilityCount,
          Statistic.BootJumps,
          Statistic.ShieldBeltCount,
          Statistic.ArmorVestCount,
          Statistic.ArmorPadsCount,
          Statistic.HelmetCount,
          Statistic.KegCount
        ]
      }
    ]
  },
  {
    heading: 'Weapon Stats',
    cards: [
      {
        heading: 'Impact Hammer',
        headingIcon: 'ih.png',
        stats: [
          Statistic.ImpactHammerKills,
          Statistic.ImpactHammerDeaths
        ]
      },
      {
        heading: 'Enforcer',
        headingIcon: 'en.png',
        stats: [
          Statistic.EnforcerKills,
          Statistic.EnforcerDeaths,
          Statistic.EnforcerShots,
          Statistic.EnforcerHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Bio Rifle',
        headingIcon: 'br.png',
        stats: [
          Statistic.BioRifleKills,
          Statistic.BioRifleDeaths,
          Statistic.BioRifleShots,
          Statistic.BioRifleHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Grenade Launcher',
        headingIcon: 'br.png',
        stats: [
          Statistic.BioLauncherKills,
          Statistic.BioLauncherDeaths,
          Statistic.BioLauncherShots,
          Statistic.BioLauncherHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Shock Rifle',
        headingIcon: 'sr.png',
        stats: [
          Statistic.ShockBeamKills,
          Statistic.ShockBeamDeaths,
          Statistic.ShockCoreKills,
          Statistic.ShockCoreDeaths,
          Statistic.ShockComboKills,
          Statistic.ShockComboDeaths,
          Statistic.ShockRifleShots,
          Statistic.ShockRifleHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Link Gun',
        headingIcon: 'lr.png',
        stats: [
          Statistic.LinkKills,
          Statistic.LinkDeaths,
          Statistic.LinkBeamKills,
          Statistic.LinkBeamDeaths,
          Statistic.LinkShots,
          Statistic.LinkHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Minigun',
        headingIcon: 'mg.png',
        stats: [
          Statistic.MinigunKills,
          Statistic.MinigunDeaths,
          Statistic.MinigunShardKills,
          Statistic.MinigunShardDeaths,
          Statistic.MinigunShots,
          Statistic.MinigunHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Flak Cannon',
        headingIcon: 'fc.png',
        stats: [
          Statistic.FlakShardKills,
          Statistic.FlakShardDeaths,
          Statistic.FlakShellKills,
          Statistic.FlakShellDeaths,
          Statistic.FlakShots,
          Statistic.FlakHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Rocket Launcher',
        headingIcon: 'rl.png',
        stats: [
          Statistic.RocketKills,
          Statistic.RocketDeaths,
          Statistic.RocketShots,
          Statistic.RocketHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Sniper',
        headingIcon: 'snr.png',
        stats: [
          Statistic.SniperKills,
          Statistic.SniperDeaths,
          Statistic.SniperHeadshotKills,
          Statistic.SniperHeadshotDeaths,
          Statistic.SniperShots,
          Statistic.SniperHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Lightning Rifle',
        headingIcon: 'lightning.png',
        stats: [
          Statistic.LightningRiflePrimaryKills,
          Statistic.LightningRiflePrimaryDeaths,
          Statistic.LightningRifleSecondaryKills,
          Statistic.LightningRifleSecondaryDeaths,
          Statistic.LightningRifleShots,
          Statistic.LightningRifleHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Redeemer',
        headingIcon: 'rd.png',
        stats: [
          Statistic.RedeemerKills,
          Statistic.RedeemerDeaths,
          Statistic.RedeemerShots,
          Statistic.RedeemerHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Instagib',
        headingIcon: 'sr.png',
        stats: [
          Statistic.InstagibKills,
          Statistic.InstagibDeaths,
          Statistic.InstagibShots,
          Statistic.InstagibHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Translocator',
        headingIcon: 'xloc.png',
        stats: [
          Statistic.TelefragKills,
          Statistic.TelefragDeaths
        ]
      }
    ]
  },
  {
    heading: 'Miscellaneous',
    cards: [
      {
        heading: 'Movement',
        stats: [
          Statistic.RunDist,
          Statistic.SprintDist,
          Statistic.InAirDist,
          Statistic.SwimDist,
          Statistic.TranslocDist,
          Statistic.NumDodges,
          Statistic.NumWallDodges,
          Statistic.NumJumps,
          Statistic.NumLiftJumps,
          Statistic.NumFloorSlides,
          Statistic.NumWallRuns,
          Statistic.NumImpactJumps,
          Statistic.NumRocketJumps,
          Statistic.SlideDist,
          Statistic.WallRunDist
        ]
      },
      {
        heading: 'Capture the Flag',
        stats: [
          Statistic.FlagCaptures,
          Statistic.FlagReturns,
          Statistic.FlagAssists,
          Statistic.FlagHeldDeny,
          Statistic.FlagHeldDenyTime,
          Statistic.FlagHeldTime,
          Statistic.FlagReturnPoints,
          Statistic.CarryAssist,
          Statistic.CarryAssistPoints,
          Statistic.FlagCapPoints,
          Statistic.DefendAssist,
          Statistic.DefendAssistPoints,
          Statistic.ReturnAssist,
          Statistic.ReturnAssistPoints,
          Statistic.TeamCapPoints,
          Statistic.EnemyFCDamage,
          Statistic.FCKills,
          Statistic.FCKillPoints,
          Statistic.FlagSupportKills,
          Statistic.FlagSupportKillPoints,
          Statistic.RegularKillPoints,
          Statistic.FlagGrabs,
          Statistic.AttackerScore,
          Statistic.DefenderScore,
          Statistic.SupporterScore
        ]
      }
    ]
  },
];

async function loadStats() {
  if (!accountId.value) {
    return;
  }
  try {
    statsStatus.value = AsyncStatus.BUSY;
    stats.value = await statsService.getStats(accountId.value, statWindow.value);
    statsStatus.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    statsStatus.value = AsyncStatus.ERROR;
    console.error(err);
  }
}

async function loadAccounts() {
  try {
    if (AccountStore.accounts?.length) {
      return;
    }
    accountsStatus.value = AsyncStatus.BUSY;
    await AccountStore.fetchAllAccounts();
    accountId.value = SessionStore.session?.account_id?.toString();
    accountsStatus.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    accountsStatus.value = AsyncStatus.ERROR;
    console.error(err);
  }
}

function handleParameterChange() {
  if (!accountId.value || !statWindow.value) {
    return;
  }
  loadStats();
}

function handleSelectAccount(account?: IAccount) {
  accountId.value = account?.id;
  handleParameterChange();
}

onMounted(async () => {
  await loadAccounts();
  loadStats();
});

</script>
