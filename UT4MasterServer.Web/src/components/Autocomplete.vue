<template>
    <div class="autocomplete-container">
        <input type="text" class="form-control" v-model="searchText" @focus="handleFocus" @blur="handleBlur"
            @keydown="handleKeydown" />
        <div class="autocomplete-menu dropdown-menu show" v-show="menuOpen">
            <a href="#" class="autocomplete-menu-item dropdown-item" :class="{ 'active': index === activeIndex }"
                v-for="(item, index) in filteredItems" @click.stop.prevent="handleSelect(item)">
                <div class="autocomplete-menu-item-display" v-show="!$slots.item">
                    {{ searchKey? item[searchKey]: item }}
                </div>
            </a>
            <div class="autocomplete-menu-item dropdown-item no-items" v-show="!filteredItems.length">
                No items
            </div>
        </div>
        <button v-if="searchText.length" type="button" class="btn btn-primary btn-sm btn-smaller clear-button"
            @click="handleClear">Clear</button>
    </div>
</template>

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

<script setup lang="ts">
import { ref, PropType, computed, watch, onMounted } from 'vue';

const props = defineProps({
    items: {
        type: Array as PropType<any[]>,
        default: () => []
    },
    itemKey: {
        type: String,
        default: undefined
    },
    searchKey: {
        type: String,
        default: undefined
    },
    value: {
        type: String,
        default: undefined
    }
});

const emit = defineEmits(['select']);

const searchText = ref('');
const menuOpen = ref(false);
const activeIndex = ref(-1);

const filteredItems = computed(() =>
    props.items.filter((i) =>
        ((props.searchKey ? i[props.searchKey] : i) as string).includes(searchText.value)
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

function handleSelect(item: any) {
    emit('select', item);
    menuOpen.value = false;
}

function handleClear() {
    searchText.value = '';
    handleSelect(undefined);
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

function valueChanged(value?: string) {
    if (props.value) {
        const match = props.items.find((i) => props.itemKey ? i[props.itemKey] === props.value : i === value);
        if (match) {
            searchText.value = props.searchKey ? match[props.searchKey] : match;
        }
    }
}

watch(() => props.value, valueChanged);
onMounted(valueChanged);
</script>
