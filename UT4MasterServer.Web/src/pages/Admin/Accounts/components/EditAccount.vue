<template>
	<EditRoles :account="account" @updated="$emit('updated')" />
	<hr />
	<ChangePassword :account="account" @updated="$emit('updated')" />
</template>

<script lang="ts" setup>
import { IAccountWithRoles } from '@/types/account';
import { PropType } from 'vue';
import ChangePassword from './ChangePassword.vue';
import EditRoles from './EditRoles.vue';

defineProps({
	account: {
		type: Object as PropType<IAccountWithRoles>,
		required: true
	}
});

defineEmits(['updated']);
</script>
