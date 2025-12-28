<template>
	<li class="nav-item dropdown pull-right">
		<a
			class="nav-link dropdown-toggle"
			href="#"
			role="button"
			aria-haspopup="true"
			aria-expanded="false"
			@click.stop="toggle"
			>{{ text }}</a
		>
		<div class="dropdown-menu" :class="{ show: show }">
			<slot />
		</div>
	</li>
</template>

<script setup lang="ts">
import { shallowRef, onMounted, onUnmounted } from 'vue';

defineProps({
	text: {
		type: String,
		required: true
	}
});

const show = shallowRef(false);

function close() {
	show.value = false;
}

function toggle() {
	show.value = !show.value;
}

onMounted(() => {
	document.addEventListener('keydown', close);
	document.addEventListener('click', close);
});

onUnmounted(() => {
	document.removeEventListener('keydown', close);
	document.removeEventListener('click', close);
});
</script>
