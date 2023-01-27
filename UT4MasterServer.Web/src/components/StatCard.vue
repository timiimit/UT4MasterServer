<template>
    <div class="col-lg-3 col-md-6 col-sm-12">
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
                        <span class="key">{{ StatisticDisplay[stat] }}: </span>
                        <span class="value">{{ getStatValueDisplay(stat) }}</span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.card {
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
import { IStatisticCard } from '@/types/statistic-config';
import { IStatisticData } from '@/types/statistic-data';
import { PropType } from 'vue';
import { Statistic } from '@/enums/statistic';
import { StatisticDisplay } from '@/enums/statistic-display';

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

const imgIconUrl = new URL(`@/assets/weapons/${props.card.headingIcon}`, import.meta.url).href;

function getAccuracy(): string {
    const shots = props.card.stats.find((s) => s.includes('Shots'));
    const hits = props.card.stats.find((s) => s.includes('Hits'));
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
    const matchingStat = props.data.find((d) => d.name === stat);
    let value = matchingStat?.value ?? 0
    if (stat.includes('Dist') || stat.includes('Hits')) {
        value = value / 100;
    }

    return value;
}

function getStatValueDisplay(stat: Statistic): string {
    if (stat === Statistic.Accuracy) {
        return getAccuracy();
    }
    const value = getStatValue(stat);
    if (stat === Statistic.TimePlayed) {
        return toHoursMinutesSeconds(value);
    }
    if (stat.includes('Dist')) {
        return `${value}m`;
    }

    return `${value}`;
}

function toHoursMinutesSeconds(rawtime: number) {
    var hours = Math.floor(rawtime / 3600);
    var minutes = Math.floor((rawtime - (hours * 3600)) / 60);
    var seconds = rawtime - hours * 3600 - minutes * 60;
    var hoursString = `${hours}`;
    var minutesString = `${minutes}`;
    var secondsString = `${seconds}`;
    if (hours < 10) {
        hoursString = "0" + hours;
    }
    if (minutes < 10) {
        minutesString = "0" + minutes;
    }
    if (seconds < 10) {
        secondsString = "0" + seconds;
    }
    return `${hoursString}:${minutesString}:${secondsString}`;
}

</script>
