import { GameServerTrust } from "@/enums/game-server-trust";

export interface ITrustedGameServer {
    ID: string;
    OwnerID: string;
    TrustLevel: GameServerTrust;
}
