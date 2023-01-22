<template>
    <div class="autocomplete-container">
        <input type="text" class="form-control" v-model="searchText" @focus="handleFocus" @blur="handleBlur" />
        <div class="autocomplete-menu dropdown-menu show" v-show="menuOpen">
            <a href="#" class="autocomplete-menu-item dropdown-item" v-for="item in filteredItems"
                @click.stop.prevent="handleSelect(item)">
                <div class="autocomplete-menu-item-display" v-show="!$slots.item">
                    {{ searchKey? item[searchKey]: item }}
                </div>
            </a>
            <div class="autocomplete-menu-item dropdown-item no-items" v-show="!filteredItems.length">
                No items
            </div>
        </div>
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

function valueChanged(value?: string) {
    if (props.value) {
        const match = props.items.find((i) => props.itemKey ? i[props.itemKey] === props.value : i === value);
        if (match) {
            searchText.value = props.searchKey ? match[props.searchKey] : match;
        }
    }
}
// TODO: keyboard input handling (arrow keys, enter, etc)
watch(() => props.value, valueChanged);
onMounted(valueChanged);
</script>
