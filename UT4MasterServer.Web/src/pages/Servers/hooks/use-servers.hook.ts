import { GameServerTrust } from '@/enums/game-server-trust';
import { computed } from 'vue';
import { gameModeMap } from '../data/game-mode-map';
import { matchStateMap } from '../data/match-state-map';
import { GameMode } from '../enums/game-mode';
import { MatchState } from '../enums/match-state';
import { ServerAttribute } from '../enums/server-attribute';
import { ServerStore } from '../stores/server.store';
import { IHub } from '../types/hub';
import { IMatch } from '../types/match';
import { IMatchmakingResponse } from '../types/matchmaking-response';
import { IQuickPlayServer } from '../types/quick-play-server';
import { IServer } from '../types/server';

export function useServers() {
  function hubFilter(r: IMatchmakingResponse) {
    return (
      r.attributes[ServerAttribute.gameInstance] !== 1 &&
      r.attributes[ServerAttribute.ranked] !== 1 &&
      r.attributes[ServerAttribute.gameMode] === GameMode.hub
    );
  }

  function serverFilter(r: IMatchmakingResponse) {
    return (
      r.attributes[ServerAttribute.gameInstance] !== 1 &&
      r.attributes[ServerAttribute.ranked] !== 1 &&
      r.attributes[ServerAttribute.gameMode] !== GameMode.hub
    );
  }

  function quickPlayFilter(r: IMatchmakingResponse) {
    //TODO: may need some additional filtering to exclude matches running on Epic trusted hub
    return (
      r.attributes[ServerAttribute.gameInstance] === 1 &&
      r.attributes[ServerAttribute.serverTrustLevel] === 0 &&
      r.attributes[ServerAttribute.gameMode] !== GameMode.empty
    );
  }

  function hubMatchFilter(r: IMatchmakingResponse, hubGuid: string) {
    return (
      r.attributes[ServerAttribute.gameInstance] === 1 &&
      r.attributes[ServerAttribute.serverVersion] === '3525360' &&
      r.attributes[ServerAttribute.hubGuid] === hubGuid
    );
  }

  function mapCustomMatchNames(matchNames: string) {
    const matchStrings = matchNames.split('\n');
    const customMatchNames: Record<string, string> = {};
    matchStrings.forEach((s) => {
      const parts = s.split(':');
      const guid = parts[0];
      const name = parts[1];
      customMatchNames[guid] = name;
    });
    return customMatchNames;
  }

  function mapHub(response: IMatchmakingResponse): IHub {
    const hubGuid = response.attributes[ServerAttribute.hubGuid] as string;
    const customMatchNamesString = response.attributes[
      ServerAttribute.customMatchNames
    ] as string;
    const customMatchNames = mapCustomMatchNames(customMatchNamesString);
    const matches = ServerStore.allServers
      .filter((r) => hubMatchFilter(r, hubGuid))
      .map((m) => mapMatch(m, customMatchNames));
    const playersInMatches = matches.reduce(
      (sum, m) => sum + m.playersOnline,
      0
    );
    return {
      id: hubGuid,
      serverName: response.attributes[ServerAttribute.serverName] as string,
      serverTrustLevel: response.attributes[
        ServerAttribute.serverTrustLevel
      ] as GameServerTrust,
      // Hub.totalPlayers is the number of players in the lobby, doesn't include players in matches
      totalPlayers: response.totalPlayers + playersInMatches,
      matches,
      customMatchNames: mapCustomMatchNames(customMatchNamesString)
    };
  }

  function mapMatch(
    response: IMatchmakingResponse,
    customMatchNames: Record<string, string>
  ): IMatch {
    const matchState = response.attributes[
      ServerAttribute.matchState
    ] as MatchState;

    const mutatorsList = response.attributes[
      ServerAttribute.mutators
    ] as string;
    const id = response.attributes[
      ServerAttribute.serverInstanceGuid
    ] as string;
    return {
      id,
      name:
        customMatchNames[id] ??
        (response.attributes[ServerAttribute.serverName] as string),
      gameType: response.attributes[ServerAttribute.gameType] as string,
      map: response.attributes[ServerAttribute.mapName] as string,
      matchState,
      matchStateDisplay: matchStateMap[matchState] ?? matchState,
      maxPlayers: response.attributes[ServerAttribute.maxPlayers] as number,
      playersOnline: response.attributes[
        ServerAttribute.playersOnline
      ] as number,
      duration: response.attributes[ServerAttribute.matchDuration] as number,
      publicPlayers: response.publicPlayers,
      mutators: mutatorsList?.split(',') ?? []
    };
  }

  function mapServer(response: IMatchmakingResponse): IServer {
    return {
      ...mapMatch(response, {}),
      serverTrustLevel: response.attributes[
        ServerAttribute.serverTrustLevel
      ] as GameServerTrust
    };
  }

  function mapQuickPlayServer(
    response: IMatchmakingResponse
  ): IQuickPlayServer {
    // Leaving this console debug in for now to see what real quick play matches return when they are live
    console.debug('Quick Play', response);
    const gameMode = response.attributes[ServerAttribute.gameMode] as GameMode;
    return {
      id: response.attributes[ServerAttribute.dcid] as string,
      gameMode,
      gameModeDisplay: gameModeMap[gameMode] ?? gameMode,
      region: response.attributes[ServerAttribute.region] as string,
      eloRangeStart:
        (response.attributes[ServerAttribute.eloRangeStart] as number) ??
        undefined,
      eloRangeEnd:
        (response.attributes[ServerAttribute.eloRangeEnd] as number) ??
        undefined
    };
  }

  if (!ServerStore.allServers.length) {
    ServerStore.fetchAllServers();
  }

  const hubs = computed(() =>
    ServerStore.allServers.filter(hubFilter).map(mapHub)
  );
  const servers = computed(() =>
    ServerStore.allServers.filter(serverFilter).map(mapServer)
  );
  const quickPlayServers = computed(() =>
    ServerStore.allServers.filter(quickPlayFilter).map(mapQuickPlayServer)
  );

  return {
    hubs,
    servers,
    quickPlayServers
  };
}
