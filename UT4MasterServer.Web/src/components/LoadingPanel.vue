<template>
	<div class="loading-panel-container">
		<div v-show="asyncStatus === AsyncStatus.BUSY" class="loading-panel">
			<div class="spinner" />
		</div>
		<div class="content">
			<slot />
		</div>
		<div
			v-show="asyncStatus === AsyncStatus.ERROR"
			class="alert alert-dismissible alert-danger"
		>
			<button type="button" class="btn-close" @click="dismissError"></button>
			<div>{{ error }}</div>
		</div>
	</div>
</template>

<style lang="scss" scoped>
.loading-panel-container {
	position: relative;

	.loading-panel {
		position: absolute;
		width: 100%;
		height: 100%;
		z-index: 1;
		background-color: white;
		opacity: 0.5;
		display: flex;
		align-items: center;
		justify-content: center;

		.spinner {
			background-image: url(../assets/loading.gif);
			background-size: contain;
			height: 50px;
			width: 50px;
			z-index: 10;
		}
	}
}
</style>

<script setup lang="ts">
import { PropType, ref, watch, onUnmounted, onMounted } from 'vue';
import { AsyncStatus } from '@/types/async-status';

const props = defineProps({
	status: {
		type: Number as PropType<AsyncStatus>,
		default: AsyncStatus.OK
	},
	error: {
		type: String,
		default: 'An error occurred. Please try again.'
	},
	onLoad: {
		type: Function,
		default: undefined
	},
	autoLoad: {
		type: Boolean,
		default: false
	}
});

const emit = defineEmits(['load']);

const asyncStatus = ref(props.status);

const destroyWatch = watch(
	() => props.status,
	() => {
		asyncStatus.value = props.status;
	}
);

function dismissError() {
	asyncStatus.value = AsyncStatus.OK;
}

onMounted(() => {
	if (props.autoLoad) {
		emit('load');
	}
});

onUnmounted(destroyWatch);
</script>
