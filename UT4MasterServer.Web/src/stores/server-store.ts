import { ref } from "vue";
import { IGameHub, IGameServer } from "@/types/game-server";
import ServerService from "@/services/server.service";
import { AsyncStatus } from "@/types/async-status";

const _allServers = ref<IGameServer[]>([]);
const _hubs = ref<IGameHub[]>([]);
const _serverService = new ServerService();
const _status = ref(AsyncStatus.OK);

function hubFilter(s: IGameServer) {
    return s.attributes.UT_HUBGUID_s === s.attributes.UT_SERVERINSTANCEGUID_s;
}

function matchFilter(h: IGameServer, s: IGameServer) {
    return h.attributes.UT_SERVERINSTANCEGUID_s === s.attributes.UT_HUBGUID_s && !hubFilter(s)
}

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
            _hubs.value = _allServers.value
                .filter(hubFilter)
                .map((h) => {
                    const matches = _allServers.value.filter((s) => matchFilter(h, s));
                    const hub: IGameHub = {
                        ...h,
                        matches,
                        // totalPlayers and the hub UT_PLAYERONLINE_i both showed 0 in my testing, but they are reported
                        // correctly from the match servers, so I am summing them to display the totalPlayers
                        totalPlayers: matches.reduce((accumulator, m) => accumulator + m.attributes.UT_PLAYERONLINE_i, 0)
                    };
                    return hub;
                });
            _status.value = AsyncStatus.OK;
        }
        catch (err: unknown) {
            console.error('Error fetching servers:', err);
            _status.value = AsyncStatus.ERROR;
        }
    }
};
