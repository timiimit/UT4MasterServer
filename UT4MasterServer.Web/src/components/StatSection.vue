<template>
    <h3 class="text-center">
        {{ section.heading }}
    </h3>
    <div class="stat-section row">
        <StatCard :card="card" :data="data" v-for="card in section.cards" />
    </div>
</template>

<style lang="scss" scoped>
.stat-section {
    display: flex;
    justify-content: center;
    margin-bottom: 30px;
}
</style>

<script setup lang="ts">
import { IStatisticSection } from '../types/statistic-config';
import { IStatisticData } from '../types/statistic-data';
import { PropType } from 'vue';
import StatCard from './StatCard.vue';

defineProps({
    section: {
        type: Object as PropType<IStatisticSection>,
        required: true
    },
    data: {
        type: Array as PropType<IStatisticData[]>,
        default: () => []
    }
});

</script>
