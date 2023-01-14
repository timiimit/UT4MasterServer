<template>
    <div class="code codeblock">
        <div ref="code">
            <slot />
        </div>
        <button id="copy-button" class="btn btn-primary btn-sm btn-smaller" @click="copyCode">Copy</button>
    </div>
</template>

<style lang="scss" scoped>
#copy-button {
    position: absolute;
    top: 10px;
    right: 10px;
}
</style>

<script setup lang="ts">
import { ref } from 'vue';

const code = ref<HTMLElement | undefined>(undefined);
function copyCode() {
    if (code.value) {
        const content = code.value.innerText;
        navigator.clipboard.writeText(content);
    }
}
</script>
