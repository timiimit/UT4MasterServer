<template>
	<LoadingPanel :status="ServerStore.status">
		<div class="d-flex justify-content-center">
			<div class="col-md-9 col-sm-12">
				<!-- Filters -->
				<div class="row mb-3">
					<div class="col-md-7 col-12">
						<input
							v-model="filterText"
							type="text"
							class="form-control"
							placeholder="Filter..."
						/>
					</div>
					<div class="col-md-4 col-9 d-flex align-items-center">
						<div class="form-check d-flex align-items-center">
							<input
								id="hideEmpty"
								v-model="hideEmpty"
								class="form-check-input m-2"
								type="checkbox"
								value=""
							/>
							<label class="form-check-label" for="hideEmpty">
								Hide Empty
							</label>
						</div>
					</div>
					<div
						class="col-md-1 col-3 d-flex align-items-center justify-content-end"
					>
						<button
							class="btn btn-lg btn-icon"
							title="Refresh Servers"
							@click="ServerStore.fetchAllServers"
						>
							<FontAwesomeIcon icon="fa-solid fa-arrows-rotate" />
						</button>
					</div>
				</div>
				<!-- Tabs -->
				<ul class="nav nav-tabs" role="tablist">
					<li class="nav-item" role="presentation">
						<a
							class="nav-link"
							:class="{ active: activeTab === 'hubs' }"
							role="tab"
							@click="activeTab = 'hubs'"
							>Hubs</a
						>
					</li>
					<li class="nav-item" role="presentation">
						<a
							class="nav-link"
							:class="{ active: activeTab === 'servers' }"
							role="tab"
							@click="activeTab = 'servers'"
							>Servers</a
						>
					</li>
					<li class="nav-item" role="presentation">
						<a
							class="nav-link"
							:class="{ active: activeTab === 'quickPlay' }"
							role="tab"
							@click="activeTab = 'quickPlay'"
							>Quick Play</a
						>
					</li>
				</ul>
				<div class="tab-content">
					<div
						class="tab-pane fade"
						:class="{ 'show active': activeTab === 'hubs' }"
						role="tabpanel"
					>
						<HubList :filter-text="filterText" :hide-empty="hideEmpty" />
					</div>
					<div
						class="tab-pane fade"
						:class="{ 'show active': activeTab === 'servers' }"
						role="tabpanel"
					>
						<ServerList :filter-text="filterText" :hide-empty="hideEmpty" />
					</div>
					<div
						class="tab-pane fade"
						:class="{ 'show active': activeTab === 'quickPlay' }"
						role="tabpanel"
					>
						<QuickPlayList :filter-text="filterText" :hide-empty="hideEmpty" />
					</div>
				</div>
			</div>
		</div>
	</LoadingPanel>
</template>

<style lang="scss" scoped>
#searchContainer {
	margin-bottom: 1rem;
}

.nav-tabs {
	.nav-item {
		a {
			cursor: pointer;
		}
	}
}

.tab-content {
	.tab-pane {
		transition: all 0.4s ease-in-out;
		&.fade {
			opacity: 0;
			display: none;
			&.show {
				display: block;
				opacity: 1;
			}
		}
	}
}
</style>

<script lang="ts" setup>
import { shallowRef } from 'vue';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { ServerStore } from './stores/server.store';
import HubList from './components/HubList.vue';
import ServerList from './components/ServerList.vue';
import QuickPlayList from './components/QuickPlayList.vue';

const filterText = shallowRef('');
const hideEmpty = shallowRef(true);
const activeTab = shallowRef('hubs');
</script>
