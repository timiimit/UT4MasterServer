<template>
    <div v-if="pages > 1" class="paging d-flex justify-content-between">
        <div class="showing-items">
            Showing items {{ start + 1 }} - {{ end < items.length ? end : items.length }} of {{ items.length }}
        </div>
        <div class="page-buttons">
            <button class="btn btn-icon" @click="previousPage" :disabled="!hasPrevious">
                <FontAwesomeIcon icon="fa-solid fa-chevron-left" />
            </button>
            {{ currentPage + 1 }}
            <button class="btn btn-icon" @click="nextPage" :disabled="!hasNext">
                <FontAwesomeIcon icon="fa-solid fa-chevron-right" />
            </button>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { PropType, shallowRef, computed, watch, onMounted } from 'vue';

const props = defineProps({
    items: {
        type: Array as PropType<unknown[]>,
        required: true
    },
    pageSize: {
        type: Number,
        default: 10
    }
});

const emit = defineEmits(['update']);

const pages = computed(() => Math.ceil(props.items.length / props.pageSize));
const start = computed(() => currentPage.value * props.pageSize);
const end = computed(() => (currentPage.value * props.pageSize) + props.pageSize);
const hasNext = computed(() => currentPage.value < (pages.value - 1));
const hasPrevious = computed(() => currentPage.value > 0);
const currentPage = shallowRef(0);

function emitUpdate() {
    emit('update', start.value, end.value);
}

function previousPage() {
    currentPage.value = currentPage.value - 1;
    if (currentPage.value < 0) {
        currentPage.value = 0;
    }
    emitUpdate();
}

function nextPage() {
    currentPage.value = currentPage.value + 1;
    if (currentPage.value > (pages.value - 1)) {
        currentPage.value = pages.value - 1;
    }
    emitUpdate();
}

watch(() => props.items.length, () => {
    emitUpdate();
});

onMounted(emitUpdate);

</script>