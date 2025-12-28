<template>
	<LoadingPanel :status="status" :error="errorMessage">
		<form
			:class="{ 'was-validated': submitAttempted }"
			novalidate
			@submit.prevent="handleSubmit"
		>
			<fieldset>
				<legend>Update Cloud File</legend>
				<p>
					Note: The filename will be
					<strong>{{ file?.filename }}</strong> regardless of the name of the
					uploaded file.
				</p>
				<div class="form-group row">
					<label for="name" class="col-sm-12 col-form-label">Name</label>
					<div class="col-sm-6">
						<input
							id="file"
							type="file"
							class="form-control"
							name="file"
							required
							@change="handleFileChange($event.target)"
						/>
						<div class="invalid-feedback">File is required</div>
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
					<button type="submit" class="btn btn-primary">Update File</button>
				</div>
			</fieldset>
		</form>
	</LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, PropType } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin.service';
import { ICloudFile } from '../types/cloud-file';

const props = defineProps({
	file: {
		type: Object as PropType<ICloudFile>,
		required: true
	}
});

const emit = defineEmits(['updated', 'cancel']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const updatedFile = shallowRef<File | undefined>(undefined);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef('Error updating file. Please try again.');

function handleFileChange(eventTarget: EventTarget | null) {
	const target = eventTarget as HTMLInputElement;
	if (!target?.files) {
		return;
	}
	updatedFile.value = target.files[0];
}

async function handleSubmit() {
	submitAttempted.value = true;
	if (!updatedFile.value) {
		return;
	}
	try {
		status.value = AsyncStatus.BUSY;
		const formData = new FormData();
		formData.append('file', updatedFile.value, props.file.filename);
		await adminService.upsertCloudFile(formData);
		status.value = AsyncStatus.OK;
		emit('updated');
	} catch (err: unknown) {
		status.value = AsyncStatus.ERROR;
		errorMessage.value = (err as Error)?.message;
	}
}
</script>
