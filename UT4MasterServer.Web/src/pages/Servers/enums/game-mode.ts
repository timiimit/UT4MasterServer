// This isn't a complete list, but it is only used in quick play for now and I think these are all the possible values for quick play
export enum GameMode {
  duel = '/Script/UnrealTournament.UTDuelGameMode',
  deathmatch = '/Script/UnrealTournament.UTDMGameMode',
  ctf = '/Script/UnrealTournament.UTCTFGameMode',
  blitz = '/Script/UnrealTournament.UTFlagRunGame',
  empty = 'EMPTY',
  hub = '/Script/UnrealTournament.UTLobbyGameMode'
}
