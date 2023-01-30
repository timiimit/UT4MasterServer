import { ITrustedGameServer } from "../../TrustedServers/types/trusted-game-server";
import { IClient } from "../types/client";

export function useClientOptions() {
    const reservedNames = ['Launcher (our website)', 'Game', 'ServerInstance'];

    function isReservedClient(client: IClient) {
        return reservedNames.includes(client.name);
    }

    function isValidName(name: string, allClients: IClient[]) {
        return name.length && !allClients.find((c) => c.name === name);
    }

    function getClientOptionsForTrustedServer(clients: IClient[], servers: ITrustedGameServer[]) {
        return clients.filter((c) => !isReservedClient(c) && !servers.find((s) => s.id === c.id));
    }

    return {
        isReservedClient,
        isValidName,
        getClientOptionsForTrustedServer
    };
}