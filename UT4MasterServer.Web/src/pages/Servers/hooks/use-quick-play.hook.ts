import { computed } from 'vue';
import { gameModeMap } from '../data/game-mode-map';

import { GameMode } from '../enums/game-mode';
import { ServerAttribute } from '../enums/server-attribute';
import { ServerStore } from '../stores/server.store';
import { IMatchmakingResponse } from '../types/matchmaking-response';
import { IQuickPlayServer } from '../types/quick-play-server';
import { useMatch } from './use-match.hook';

export function useQuickPlay() {
	const { mapMatch } = useMatch();

	function quickPlayFilter(r: IMatchmakingResponse) {
		return (r.attributes[ServerAttribute.ranked] as number) > 0;
	}

	function mapQuickPlayServer(
		response: IMatchmakingResponse
	): IQuickPlayServer {
		const gameMode = response.attributes[ServerAttribute.gameMode] as GameMode;
		return {
			...mapMatch(response, {}),
			id: response.attributes[ServerAttribute.dcid] as string,
			gameMode,
			gameModeDisplay: gameModeMap[gameMode] ?? gameMode,
			region: response.attributes[ServerAttribute.region] as string,
			targetElo:
				(response.attributes[ServerAttribute.targetElo] as number) ?? undefined
		};
	}

	if (!ServerStore.allServers.length) {
		ServerStore.fetchAllServers();
	}

	const quickPlayServers = computed(() =>
		ServerStore.allServers.filter(quickPlayFilter).map(mapQuickPlayServer)
	);

	return {
		quickPlayServers
	};
}
