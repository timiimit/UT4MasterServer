<template>
	<LoadingPanel :status="status" :error="errorMessage">
		<form
			:class="{ 'was-validated': submitAttempted }"
			novalidate
			@submit.prevent="handleSubmit"
		>
			<fieldset>
				<legend>Add Cloud File</legend>
				<div class="form-group row">
					<label for="name" class="col-sm-12 col-form-label">Name</label>
					<div class="col-sm-6">
						<input
							id="name"
							v-valid="fileValid"
							type="file"
							class="form-control"
							name="name"
							required
							@change="handleFileChange($event.target)"
						/>
						<div class="invalid-feedback">A unique filename is required</div>
					</div>
				</div>
				<div class="d-flex justify-content-between mb-2">
					<button
						type="button"
						class="btn btn-secondary"
						@click="emit('cancel')"
					>
						Cancel
					</button>
					<button type="submit" class="btn btn-primary">Add File</button>
				</div>
			</fieldset>
		</form>
	</LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, PropType, computed } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin.service';
import { ICloudFile } from '../types/cloud-file';
import { valid as vValid } from '@/directives/valid';

const props = defineProps({
	allFiles: {
		type: Array as PropType<ICloudFile[]>,
		default: () => []
	}
});

const emit = defineEmits(['added', 'cancel']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const name = shallowRef('');
const file = shallowRef<File | undefined>(undefined);
const fileValid = computed(
	() => !props.allFiles.map((f) => f.filename).includes(name.value)
);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef('Error adding file. Please try again.');

function handleFileChange(eventTarget: EventTarget | null) {
	const target = eventTarget as HTMLInputElement;
	if (!target?.files) {
		return;
	}
	file.value = target.files[0];
	name.value = file.value.name;
}

async function handleSubmit() {
	submitAttempted.value = true;
	if (!file.value || !fileValid.value) {
		return;
	}
	try {
		status.value = AsyncStatus.BUSY;
		const formData = new FormData();
		formData.append('file', file.value, name.value);
		await adminService.upsertCloudFile(formData);
		status.value = AsyncStatus.OK;
		emit('added');
	} catch (err: unknown) {
		status.value = AsyncStatus.ERROR;
		errorMessage.value = (err as Error)?.message;
	}
}
</script>
