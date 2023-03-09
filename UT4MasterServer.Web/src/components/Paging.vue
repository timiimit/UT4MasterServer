<template>
  <div v-if="pages > 1" class="paging d-flex justify-content-between">
    <div class="showing-items">
      Showing items {{ start + 1 }} - {{ end < itemCount ? end : itemCount }} of
      {{ itemCount }}
    </div>
    <div class="page-buttons">
      <button
        class="btn btn-icon"
        :disabled="!hasPrevious"
        @click="previousPage"
      >
        <FontAwesomeIcon icon="fa-solid fa-chevron-left" />
      </button>
      {{ currentPage + 1 }}
      <button class="btn btn-icon" :disabled="!hasNext" @click="nextPage">
        <FontAwesomeIcon icon="fa-solid fa-chevron-right" />
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { shallowRef, computed, watch, onMounted } from 'vue';

const props = defineProps({
  itemCount: {
    type: Number,
    default: 0
  },
  pageSize: {
    type: Number,
    default: 10
  },
  page: {
    type: Number,
    default: 0
  }
});

const emit = defineEmits(['update']);

const currentPage = shallowRef(props.page);
const pages = computed(() => Math.ceil(props.itemCount / props.pageSize));
const start = computed(() => currentPage.value * props.pageSize);
const end = computed(() => currentPage.value * props.pageSize + props.pageSize);
const hasNext = computed(() => currentPage.value < pages.value - 1);
const hasPrevious = computed(() => currentPage.value > 0);

function emitUpdate() {
  emit('update', start.value, end.value, currentPage.value);
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
  if (currentPage.value > pages.value - 1) {
    currentPage.value = pages.value - 1;
  }
  emitUpdate();
}

watch(
  () => props.page,
  () => {
    currentPage.value = props.page;
    emitUpdate();
  }
);

onMounted(emitUpdate);
</script>
