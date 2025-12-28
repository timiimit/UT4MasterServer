import { ITrustedGameServer } from '../../TrustedServers/types/trusted-game-server';
import { IClient } from '../types/client';

export function useClientOptions() {
	// These are hardcoded in ClientIdentification.cs, we could add an endpoint to retrieve them to avoid the duplcated hardcoding at some point, but I don't think these are likely to ever change.
	const reservedIds = [
		'34a02cf8f4414e29b15921876da36f9a',
		'1252412dc7704a9690f6ea4611bc81ee',
		'6ff43e743edc4d1dbac3594877b4bed9'
	];

	function isReservedClient(client: IClient) {
		return reservedIds.includes(client.id);
	}

	function isValidName(name: string, allClients: IClient[]) {
		return name.length && !allClients.find((c) => c.name === name);
	}

	function getClientOptionsForTrustedServer(
		clients: IClient[],
		servers: ITrustedGameServer[]
	) {
		return clients.filter(
			(c) => !isReservedClient(c) && !servers.find((s) => s.id === c.id)
		);
	}

	return {
		isReservedClient,
		isValidName,
		getClientOptionsForTrustedServer
	};
}
