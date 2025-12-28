import { ref } from 'vue';
import ServerService from '../services/servers.service';
import { AsyncStatus } from '@/types/async-status';
import { IMatchmakingResponse } from '../types/matchmaking-response';

const _allServers = ref<IMatchmakingResponse[]>([]);
const _serverService = new ServerService();
const _status = ref(AsyncStatus.OK);

export const ServerStore = {
	get allServers() {
		return _allServers.value;
	},
	get status() {
		return _status.value;
	},
	async fetchAllServers() {
		try {
			if (_status.value === AsyncStatus.BUSY) {
				return;
			}
			_status.value = AsyncStatus.BUSY;
			_allServers.value = await _serverService.getAllServers();
			_status.value = AsyncStatus.OK;
		} catch (err: unknown) {
			console.error('Error fetching servers:', err);
			_status.value = AsyncStatus.ERROR;
		}
	}
};
