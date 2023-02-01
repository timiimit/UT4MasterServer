<template>
  <Multiselect
    v-model="filterRoles"
    placeholder="Filter by Roles..."
    :options="roleOptions"
    mode="tags"
  />
</template>

<script lang="ts" setup>
import { PropType, computed, shallowRef, onMounted, watch } from 'vue';
import Multiselect from '@vueform/multiselect';
import { Role } from '@/enums/role';
import AdminService from '@/services/admin-service';

const props = defineProps({
  modelValue: {
    type: Array as PropType<Role[]>,
    required: true
  }
});
const emit = defineEmits(['update:modelValue']);

const adminService = new AdminService();

const filterRoles = shallowRef<Role[]>(props.modelValue);

const allRoles = shallowRef<string[]>([]);
const roleOptions = computed(() =>
  allRoles.value.map((r) => ({ label: r, value: r }))
);

async function loadRoles() {
  try {
    allRoles.value = await adminService.getRoleOptions();
    allRoles.value = allRoles.value.filter((r) => r !== Role.None);
  } catch (err: unknown) {
    console.error('Error loading roles', err);
  }
}

onMounted(loadRoles);
watch(filterRoles, () => {
  console.debug('filter roles changed', filterRoles.value);
  emit('update:modelValue', filterRoles.value);
});
</script>
