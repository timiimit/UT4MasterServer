import { ref } from "vue";
import { IGameHub, IGameServer } from "../types/game-server";
import ServerService from "../services/server.service";
import { AsyncStatus } from "../types/async-status";

const _allServers = ref<IGameServer[]>([]);
const _hubs = ref<IGameHub[]>([]);
const _serverService = new ServerService();
const _status = ref(AsyncStatus.OK);

export const ServerStore = {
    get allServers() {
        return _allServers.value;
    },
    get hubs() {
        return _hubs.value;
    },
    get status() {
        return _status.value;
    },
    async fetchGameServers() {
        try {
            _status.value = AsyncStatus.BUSY;
            _allServers.value = await _serverService.getGameServers();
            const hubMap: Record<string, IGameHub> = {};
            // TODO: I'm not sure about this logic for determining Hubs, there may be a better way to handle this once I see real data
            _allServers.value.forEach((server: IGameServer) => {
                if (!Object.keys(hubMap).includes(server.id)) {
                    const hub: IGameHub = {
                        ...server,
                        matches: _allServers.value.filter((s) => s.attributes.UT_HUBGUID_s === server.id)
                    };
                    hubMap[server.id] = hub;
                }
            });
            console.debug('Servers', _allServers.value);
            _hubs.value = Object.values(hubMap);
            _status.value = AsyncStatus.OK;
        }
        catch (err: unknown) {
            console.error('Error fetching servers:', err);
            _status.value = AsyncStatus.ERROR;
        }
    }
};
