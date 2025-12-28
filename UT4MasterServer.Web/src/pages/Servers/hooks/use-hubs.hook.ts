import { GameServerTrust } from '@/enums/game-server-trust';
import { computed } from 'vue';
import { GameMode } from '../enums/game-mode';
import { ServerAttribute } from '../enums/server-attribute';
import { ServerStore } from '../stores/server.store';
import { IHub } from '../types/hub';
import { IMatchmakingResponse } from '../types/matchmaking-response';
import { useMatch } from './use-match.hook';
import { intersection } from 'lodash';

export function useHubs() {
	const { mapMatch } = useMatch();

	function hubFilter(r: IMatchmakingResponse) {
		const instanceIdReferencedAsHubGuid = ServerStore.allServers.some(
			(s) =>
				r.attributes[ServerAttribute.serverInstanceGuid] ===
					s.attributes[ServerAttribute.hubGuid] &&
				r.attributes[ServerAttribute.serverInstanceGuid] !==
					r.attributes[ServerAttribute.hubGuid]
		);
		return (
			(instanceIdReferencedAsHubGuid ||
				r.attributes[ServerAttribute.gameMode] === GameMode.hub) &&
			// not quickplay
			!r.attributes[ServerAttribute.ranked]
		);
	}

	function hubMatchFilter(r: IMatchmakingResponse, hubGuid: string) {
		return (
			r.attributes[ServerAttribute.gameInstance] === 1 &&
			r.attributes[ServerAttribute.hubGuid] === hubGuid
		);
	}

	function mapCustomMatchNames(matchNames?: string) {
		const matchStrings = matchNames?.split('\n');
		const customMatchNames: Record<string, string> = {};
		matchStrings?.forEach((s) => {
			const parts = s.split(':');
			const guid = parts[0];
			const name = parts[1];
			customMatchNames[guid] = name;
		});
		return customMatchNames;
	}

	function mapHub(response: IMatchmakingResponse): IHub {
		const serverId = response.attributes[
			ServerAttribute.serverInstanceGuid
		] as string;
		const customMatchNamesString = response.attributes[
			ServerAttribute.customMatchNames
		] as string;
		const customMatchNames = mapCustomMatchNames(customMatchNamesString);
		const matches = ServerStore.allServers
			.filter((r) => hubMatchFilter(r, serverId))
			.map((m) => mapMatch(m, customMatchNames));
		const playersInMatches = matches.reduce(
			(sum, m) => sum + m.playersOnline,
			0
		);
		const forcedMutators = intersection(
			...matches.map((m) => m.forcedMutators)
		);
		return {
			id: serverId,
			serverName: response.attributes[ServerAttribute.serverName] as string,
			serverTrustLevel: response.attributes[
				ServerAttribute.serverTrustLevel
			] as GameServerTrust,
			// Hub.totalPlayers is the number of players in the lobby, doesn't include players in matches
			totalPlayers: response.totalPlayers + playersInMatches,
			matches,
			customMatchNames: mapCustomMatchNames(customMatchNamesString),
			uuInstalled: (response.attributes[ServerAttribute.uu] as number) === 1,
			forcedMutators
		};
	}

	if (!ServerStore.allServers.length) {
		ServerStore.fetchAllServers();
	}

	const hubs = computed(() =>
		ServerStore.allServers.filter(hubFilter).map(mapHub)
	);

	return {
		hubs,
		hubFilter
	};
}
