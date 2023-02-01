import { ref } from 'vue';
import { IGameHub, IGameServer } from '@/types/game-server';
import ServerService from '@/services/server.service';
import { AsyncStatus } from '@/types/async-status';

const _allServers = ref<IGameServer[]>([]);
const _hubs = ref<IGameHub[]>([]);
const _serverService = new ServerService();
const _status = ref(AsyncStatus.OK);

function hubFilter(s: IGameServer) {
  return s.attributes.UT_HUBGUID_s === s.attributes.UT_SERVERINSTANCEGUID_s;
}

function matchFilter(h: IGameServer, s: IGameServer) {
  return (
    h.attributes.UT_SERVERINSTANCEGUID_s === s.attributes.UT_HUBGUID_s &&
    !hubFilter(s)
  );
}

export const ServerStore = {
  get allServers() {
    return _allServers.value;
  },
  get hubs() {
    return _hubs.value;
  },
  get status() {
    return _status.value;
  },
  async fetchGameServers() {
    try {
      _status.value = AsyncStatus.BUSY;
      _allServers.value = await _serverService.getGameServers();
      _hubs.value = _allServers.value.filter(hubFilter).map((h) => {
        const matches = _allServers.value.filter((s) => matchFilter(h, s));
        const hub: IGameHub = {
          ...h,
          matches,
          // totalPlayers and the hub UT_PLAYERONLINE_i both showed 0 in my testing, but they are reported
          // correctly from the match servers, so I am summing them to display the totalPlayers
          totalPlayers: matches.reduce(
            (accumulator, m) => accumulator + m.attributes.UT_PLAYERONLINE_i,
            0
          )
        };
        return hub;
      });
      _hubs.value = [
        {
          id: '9bb08ca41b4da5361c933f0a60e700a2',
          ownerId: '0163F5D907020801003101F128C7D791',
          ownerName: '[DS]gh-ur-178.gamerzhost.net-27025',
          serverName: '[DS]gh-ur-178.gamerzhost.net-27025',
          serverAddress: '194.26.183.170',
          serverPort: 7877,
          maxPublicPlayers: 10000,
          openPublicPlayers: 9999,
          maxPrivatePlayers: 0,
          openPrivatePlayers: 0,
          attributes: {
            BEACONPORT_i: 7787,
            GAMEMODE_s: '/Script/UnrealTournament.UTLobbyGameMode',
            MAPNAME_s: 'UT-Entry',
            UT_SERVERNAME_s: '[PHX] PHOENIX GERMANY ',
            UT_SERVERMOTD_s:
              '<UT.Font.NormalText.Small>The real PHOENIX Clan ---Fun Gaming since 1994---</>\nPHX on Discord, connect to discord.gg/d9akhwz\nRemember, the Pickups from Elimination are for the KILLER, unless he says it can be used by others! We do not accept insults, rage quits or midgame team changes!\nPlayers who not accept the rules will be kicked without warning\nPlay fair and have FUN! ',
            UT_MATCHDURATION_i: 0,
            UT_HUBGUID_s: '005FE17F0D0905110007020E6C59A069',
            UT_REDTEAMSIZE_i: 0,
            UT_BLUETEAMSIZE_i: 0,
            UT_TRAININGGROUND_b: false,
            UT_MINELO_i: 0,
            UT_MAXELO_i: 0,
            UT_PLAYERONLINE_i: 1,
            UT_SPECTATORSONLINE_i: 0,
            UT_SERVERVERSION_s: '3525360',
            UT_SERVERINSTANCEGUID_s: '005FE17F0D0905110007020E6C59A069',
            UT_GAMEINSTANCE_i: 0,
            UT_SERVERFLAGS_i: 0,
            UT_NUMMATCHES_i: 1,
            UT_MAXPLAYERS_i: 60,
            UT_MAXSPECTATORS_i: 2,
            UT_MATCHSTATE_s: 'InProgress',
            UT_SERVERTRUSTLEVEL_i: 2
          },
          publicPlayers: ['959a3d2e145539be5842ddb6aaad0faf'],
          privatePlayers: [],
          totalPlayers: 5,
          allowJoinInProgress: true,
          shouldAdvertise: true,
          isDedicated: true,
          usesStats: false,
          allowInvites: true,
          usesPresence: false,
          allowJoinViaPresence: true,
          allowJoinViaPresenceFriendsOnly: false,
          buildUniqueId: '256652735',
          lastUpdated: '2023-02-01T10:47:15.026Z',
          started: true,
          matches: [
            {
              id: 'a8ebc8f004649cd6d1f596932369306b',
              ownerId: '033AF6711A020A010007006340D6CD21',
              ownerName: '[DS]gh-ur-178.gamerzhost.net-28158',
              serverName: '[DS]gh-ur-178.gamerzhost.net-28158',
              serverAddress: '194.26.183.170',
              serverPort: 8001,
              maxPublicPlayers: 10000,
              openPublicPlayers: 9995,
              maxPrivatePlayers: 0,
              openPrivatePlayers: 0,
              attributes: {
                BEACONPORT_i: 7788,
                GAMEMODE_s: '/Game/ALTS/Elimination_113.Elimination_113_C',
                MAPNAME_s: 'DM-Deadfall',
                GAMENAME_s: 'Absolute Elimination v1.742',
                UT_MATCHELO_i: 0,
                UT_SERVERNAME_s: 'Absolute Elimination v1.742 on DM-Deadfall ',
                UT_SERVERMOTD_s:
                  '<UT.Font.NormalText.Small>The real PHOENIX Clan ---Fun Gaming since 1994---</>\\nPHX on Discord, connect to discord.gg/d9akhwz\\nRemember, the Pickups from Elimination are for the KILLER, unless he says it can be used by others! We do not accept insults, rage quits or midgame team changes!\\nPlayers who not accept the rules will be kicked without warning\\nPlay fair and have FUN! ',
                UT_MATCHDURATION_i: 0,
                UT_HUBGUID_s: '005FE17F0D0905110007020E6C59A069',
                UT_REDTEAMSIZE_i: 2,
                UT_BLUETEAMSIZE_i: 3,
                UT_TRAININGGROUND_b: false,
                UT_MINELO_i: 0,
                UT_MAXELO_i: 0,
                UT_PLAYERONLINE_i: 5,
                UT_SPECTATORSONLINE_i: 0,
                UT_SERVERVERSION_s: '3525360',
                UT_SERVERINSTANCEGUID_s: '2602A7881C020B01002D03669F7AC714',
                UT_GAMEINSTANCE_i: 1,
                UT_SERVERFLAGS_i: 0,
                UT_NUMMATCHES_i: 1,
                UT_MAXPLAYERS_i: 8,
                UT_MAXSPECTATORS_i: 2,
                UT_MATCHSTATE_s: 'WaitingToStart',
                UT_SERVERTRUSTLEVEL_i: 2
              },
              publicPlayers: [
                '5ade2dfa0502d51ec0e2ba90b2d7ce5f',
                'c46c448f092e4f2fb815fb7a0ec5ec34',
                'a949470f54348cfd1a459e24321b9b6b',
                '18ea0a6f5166272cc6ac7c50f77d9d53',
                'ed0867c516076e0082b66924d5869b86'
              ],
              privatePlayers: [],
              totalPlayers: 5,
              allowJoinInProgress: true,
              shouldAdvertise: true,
              isDedicated: true,
              usesStats: false,
              allowInvites: true,
              usesPresence: false,
              allowJoinViaPresence: true,
              allowJoinViaPresenceFriendsOnly: false,
              buildUniqueId: '256652735',
              lastUpdated: '2023-02-01T10:46:58.820Z',
              started: true
            }
          ]
        }
      ];
      _status.value = AsyncStatus.OK;
    } catch (err: unknown) {
      console.error('Error fetching servers:', err);
      _status.value = AsyncStatus.ERROR;
    }
  }
};
