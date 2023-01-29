<template>
    <button class="copy-button btn btn-icon" :class="classes" @click.stop="copy" :title="copied ? 'Copied to clipboard' : title">
        <FontAwesomeIcon v-if="!copied" icon="fa-regular fa-copy" />
        <FontAwesomeIcon v-else icon="fa-solid fa-check" />
    </button>
</template>

<style lang="scss" scoped>
button.copy-button {
    transition: background-color color 0.5s ease-in-out;
}
</style>

<script setup lang="ts">
import { computed, shallowRef } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const props = defineProps({
    subject: {
        type: String,
        required: true
    },
    customClass: {
        type: String,
        default: ''
    },
    title: {
        type: String,
        default: 'Copy'
    }
});

const copied = shallowRef(false);

const classes = computed(() => ({
    [props.customClass]: true,
    'text-success': copied.value
}));

function copy() {
    if (props.subject) {
        const content = props.subject;
        navigator.clipboard.writeText(content);
        copied.value = true;
        setTimeout(() => copied.value = false, 2000);
    }
}
</script>
