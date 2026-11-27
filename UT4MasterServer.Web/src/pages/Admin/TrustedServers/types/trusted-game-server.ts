import { GameServerTrust } from '@/enums/game-server-trust';
import { IAccountExtended } from '@/types/account';
import { IClient } from '../../Clients/types/client';

export interface ITrustedGameServer {
	id: string;
	ownerID: string;
	trustLevel: GameServerTrust;
	client: IClient;
	owner: IAccountExtended;
}

export interface IGridTrustedServer extends ITrustedGameServer {
	editing?: boolean;
}
