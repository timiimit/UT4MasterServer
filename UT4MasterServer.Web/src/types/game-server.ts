export interface IGameServerAttributes {
    UT_SERVERNAME_s: string;
    UT_REDTEAMSIZE_i: number;
    UT_BLUETEAMSIZE_i: number;
    UT_NUMMATCHES_i: number;
    UT_GAMEINSTANCE_i: number;
    UT_MAXSPECTATORS_i: number;
    BEACONPORT_i: number;
    UT_PLAYERONLINE_i: number;
    UT_SERVERVERSION_s: number;
    GAMEMODE_s: string;
    UT_HUBGUID_s: string;
    UT_MATCHSTATE_s: string;
    UT_SERVERTRUSTLEVEL_i: number;
    UT_SERVERINSTANCEGUID_s: string;
    UT_TRAININGGROUND_b: boolean;
    UT_MINELO_i: number;
    UT_MAXELO_i: number;
    UT_SPECTATORSONLINE_i: number;
    UT_MAXPLAYERS_i: number;
    UT_SERVERMOTD_s: string;
    MAPNAME_s: string;
    UT_MATCHDURATION_i: number;
    UT_SERVERFLAGS_i: number;
}

export interface IGameServer {
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
    attributes: IGameServerAttributes;
    publicPlayers: unknown[]; //EpicID, not sure it is useful in the front end
    privatePlayers: unknown[];
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

export interface IGameHub extends IGameServer {
    matches: IGameServer[];
}