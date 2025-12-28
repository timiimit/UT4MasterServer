import { SimpleType } from '@/types/simple-type';
import { ServerAttribute } from '../enums/server-attribute';

export interface IMatchmakingResponse {
	id: string;
	ownerId: string;
	ownerName: string;
	serverName: string;
	serverAddress: string;
	serverPort: number;
	maxPublicPlayers: number;
	openPublicPlayers: number;
	maxPrivatePlayers: number;
	openPrivatePlayers: number;
	attributes: Record<ServerAttribute | string, SimpleType>;
	publicPlayers: string[];
	privatePlayers: string[];
	totalPlayers: number;
	allowJoinInProgress: boolean;
	shouldAdvertise: boolean;
	isDedicated: boolean;
	usesStats: boolean;
	allowInvites: boolean;
	usesPresence: boolean;
	allowJoinViaPresence: boolean;
	allowJoinViaPresenceFriendsOnly: boolean;
	buildUniqueId: string;
	lastUpdated: string;
	started: boolean;
}
