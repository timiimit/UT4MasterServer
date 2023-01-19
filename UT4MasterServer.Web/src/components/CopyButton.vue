<template>
    <button class="copy-button btn btn-primary btn-sm" :class="classes" @click="copy">
        {{ copied ? 'Copied' : 'Copy' }}
    </button>
</template>

<style lang="scss" scoped>
    button.copy-button {
        min-width: 50px;
        transition: background-color color 0.5s ease-in-out;
    }
</style>

<script setup lang="ts">
import { computed, shallowRef } from 'vue';


const props = defineProps({
    subject: {
        type: String,
        required: true
    },
    customClass: {
        type: String,
        default: ''
    }
});

const copied = shallowRef(false);

const classes = computed(() => ({
    [props.customClass]: true,
    'btn-success': copied.value
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
