import { GameServerTrust } from "@/enums/game-server-trust";
import { IClient } from "../../Clients/types/client";

export interface ITrustedGameServer {
    id: string;
    ownerID: string;
    trustLevel: GameServerTrust;
}

export interface IGridTrustedServer extends ITrustedGameServer {
    editing?: boolean;
    client?: IClient;
}