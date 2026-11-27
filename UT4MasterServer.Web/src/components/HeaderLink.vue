<template>
	<RouterLink
		:class="{
			'nav-link': !dropdown,
			'dropdown-item': dropdown,
			active: active
		}"
		:to="path"
		>{{ text }}</RouterLink
	>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const props = defineProps({
	text: {
		type: String,
		required: true
	},
	path: {
		type: String,
		required: true
	},
	dropdown: {
		type: Boolean,
		default: false
	}
});

const route = useRoute();

const active = computed(() => route.path === props.path);
</script>
