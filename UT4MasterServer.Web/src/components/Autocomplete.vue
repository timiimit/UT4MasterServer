<template>
  <div class="autocomplete-container">
    <input
      v-model="searchText"
      type="text"
      class="form-control"
      @focus="handleFocus"
      @blur="handleBlur"
      @keydown="handleKeydown"
    />
    <div v-show="menuOpen" class="autocomplete-menu dropdown-menu show">
      <a
        v-for="(item, index) in filteredItems"
        :key="`${item[itemKey]}-${index}`"
        href="#"
        class="autocomplete-menu-item dropdown-item"
        :class="{ active: index === activeIndex }"
        @click.stop.prevent="handleSelect(item)"
      >
        <div v-show="!$slots.item" class="autocomplete-menu-item-display">
          {{ searchKey ? item[searchKey] : item }}
        </div>
      </a>
      <div
        v-show="!filteredItems.length"
        class="autocomplete-menu-item dropdown-item no-items"
      >
        No items
      </div>
    </div>
    <button
      v-if="searchText.length"
      type="button"
      class="btn btn-primary btn-sm btn-smaller clear-button"
      @click="handleClear"
    >
      Clear
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, PropType, computed, watch, onMounted } from 'vue';

const props = defineProps({
  items: {
    type: Array as PropType<Record<string, string>[]>,
    default: () => [],
  },
  itemKey: {
    type: String,
    required: true,
  },
  searchKey: {
    type: String,
    required: true,
  },
  value: {
    type: String,
    default: undefined,
  },
});

const emit = defineEmits(['select']);

const searchText = ref('');
const menuOpen = ref(false);
const activeIndex = ref(-1);

const filteredItems = computed(() =>
  props.items.filter((i: Record<string, unknown>) =>
    ((props.searchKey ? i[props.searchKey] : i) as string)
      ?.toLocaleLowerCase()
      .includes(searchText.value.toLocaleLowerCase())
  )
);

function handleFocus() {
  menuOpen.value = true;
}

function handleBlur(event: FocusEvent) {
  if (!event.relatedTarget) {
    menuOpen.value = false;
  }
}

function handleSelect(item?: Record<string, unknown>) {
  emit('select', item);
  menuOpen.value = false;
}

function handleClear() {
  searchText.value = '';
  handleSelect();
}

function setActiveIndex(i: number) {
  if (i < 0) {
    activeIndex.value = filteredItems.value.length - 1;
  } else if (i > filteredItems.value.length - 1) {
    activeIndex.value = 0;
  } else {
    activeIndex.value = i;
  }
}

function handleKeydown(event: KeyboardEvent) {
  menuOpen.value = true;
  if (event.key === 'ArrowDown') {
    event.preventDefault();
    setActiveIndex(activeIndex.value + 1);
  } else if (event.key === 'ArrowUp') {
    event.preventDefault();
    setActiveIndex(activeIndex.value - 1);
  } else if (event.key === 'Enter') {
    event.preventDefault();
    if (filteredItems.value.length === 1) {
      activeIndex.value = 0;
    }
    const item = filteredItems.value[activeIndex.value];
    handleSelect(item);
  }
}

function valueChanged() {
  if (props.value) {
    const match = props.items.find((i) => i[props.itemKey] === props.value);
    if (match) {
      searchText.value = match[props.searchKey];
    }
  }
}

watch(() => props.value, valueChanged);
onMounted(valueChanged);
</script>

<style lang="scss" scoped>
.autocomplete-container {
  position: relative;

  .autocomplete-menu {
    width: 100%;
    max-height: 50vh;
    overflow: auto;

    .autocomplete-menu-item {
      cursor: pointer;
    }
  }

  button.clear-button {
    position: absolute;
    right: 10px;
    top: 10px;
  }
}
</style>
