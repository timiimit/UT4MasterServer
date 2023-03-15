import { GameServerTrust } from '@/enums/game-server-trust';
import { computed } from 'vue';
import { ServerAttribute } from '../enums/server-attribute';
import { ServerStore } from '../stores/server.store';
import { IMatchmakingResponse } from '../types/matchmaking-response';
import { IServer } from '../types/server';
import { useHubs } from './use-hubs.hook';
import { useMatch } from './use-match.hook';

export function useServers() {
  const { mapMatch } = useMatch();
  const { hubFilter } = useHubs();

  function serverFilter(r: IMatchmakingResponse) {
    return (
      r.attributes[ServerAttribute.gameInstance] !== 1 &&
      r.attributes[ServerAttribute.serverInstanceGuid] ===
        r.attributes[ServerAttribute.hubGuid] &&
      r.attributes[ServerAttribute.gameMode] !== 'EMPTY' &&
      !hubFilter(r)
    );
  }

  function mapServer(response: IMatchmakingResponse): IServer {
    return {
      ...mapMatch(response, {}),
      serverTrustLevel: response.attributes[
        ServerAttribute.serverTrustLevel
      ] as GameServerTrust
    };
  }

  if (!ServerStore.allServers.length) {
    ServerStore.fetchAllServers();
  }

  const servers = computed(() =>
    ServerStore.allServers.filter(serverFilter).map(mapServer)
  );

  return {
    servers
  };
}
