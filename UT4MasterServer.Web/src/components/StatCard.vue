<template>
    <div class="card bg-light">
        <div class="card-header">
            <div>
                <img v-if="card.headingIcon" :src="imgIconUrl" />
                {{ card.heading }}
            </div>
        </div>
        <div class="card-body">
            <div class="card-text">
                <div v-for="stat in card.stats">
                    <span class="key">{{ stat }}: </span>
                    <span class="value">{{ stat === Statistic.Accuracy ? getAccuracy() : getStatValue(stat) }}</span>
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.card {
    width: calc(25% - 20px);
    margin: 10px;

    .card-header {
        display: flex;
        justify-content: space-around;

        img {
            height: 1.5rem;
        }
    }

    .card-body .card-text {
        .key {
            font-weight: 700;
        }
    }
}
</style>

<script setup lang="ts">
import { IStatisticCard } from '../types/statistic-config';
import { IStatisticData } from '../types/statistic-data';
import { PropType } from 'vue';
import { Statistic } from '../enums/statistic';

const props = defineProps({
    card: {
        type: Object as PropType<IStatisticCard>,
        required: true
    },
    data: {
        type: Array as PropType<IStatisticData[]>,
        default: () => []
    }
});

const imgIconUrl = new URL(`../assets/weapons/${props.card.headingIcon}`, import.meta.url).href;

function getAccuracy(): string {
    const shots = props.card.stats.find((s) => s.includes('Shots'));
    const hits = props.card.stats.find((s) => s.includes('Shots'));
    if (shots && hits) {
        const shotValue = getStatValue(shots);
        const hitValue = getStatValue(hits);
        if (shotValue > 0) {
            return `${((hitValue / shotValue) * 100).toFixed(2)}%`;
        }
    }
    return '0.00%';
}

function getStatValue(stat: Statistic): number {
    const matchingStat = props.data.find((d) => d.Name === stat);
    return matchingStat?.Value ?? 0;
}

</script>
