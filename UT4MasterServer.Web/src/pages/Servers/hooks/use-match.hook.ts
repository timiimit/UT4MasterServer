import { gameModeMap } from '../data/game-mode-map';
import { matchStateMap } from '../data/match-state-map';
import { GameMode } from '../enums/game-mode';
import { MatchState } from '../enums/match-state';
import { ServerAttribute } from '../enums/server-attribute';
import { IMatch } from '../types/match';
import { IMatchmakingResponse } from '../types/matchmaking-response';

export function useMatch() {
  function mapGameOptions(optionsString?: string): Record<string, string> {
    if (!optionsString) {
      return {};
    }
    const params = optionsString.split('?').filter((s) => s.length);
    const optionsMap: Record<string, string> = {};
    params.forEach((p) => {
      const kvPair = p.split('=');
      const key = kvPair[0];
      const value = kvPair[1];
      optionsMap[key] = value;
    });
    return optionsMap;
  }

  function mapMatch(
    response: IMatchmakingResponse,
    customMatchNames: Record<string, string>
  ): IMatch {
    const matchState = response.attributes[
      ServerAttribute.matchState
    ] as MatchState;

    const id = response.attributes[
      ServerAttribute.serverInstanceGuid
    ] as string;
    const gameMode = response.attributes[ServerAttribute.gameMode] as GameMode;
    const gameOptions = mapGameOptions(
      response.attributes[ServerAttribute.uuGameOptions] as string
    );
    const mutators = gameOptions['mutator']?.split(',') ?? [];
    const forcedMutatorsString = response.attributes[
      ServerAttribute.uuForcedMutators
    ] as string;
    const forcedMutators = forcedMutatorsString?.split(',') ?? [];
    return {
      id,
      name:
        customMatchNames[id] ??
        (response.attributes[ServerAttribute.serverName] as string),
      gameType: response.attributes[ServerAttribute.gameType] as string,
      map: response.attributes[ServerAttribute.mapName] as string,
      matchState,
      matchStateDisplay: matchStateMap[matchState] ?? matchState,
      maxPlayers: response.attributes[ServerAttribute.maxPlayers] as number,
      playersOnline: response.attributes[
        ServerAttribute.playersOnline
      ] as number,
      duration: response.attributes[ServerAttribute.matchDuration] as number,
      elapsedTime: response.attributes[ServerAttribute.elapsedTime] as number,
      publicPlayers: response.publicPlayers,
      mutators,
      gameMode,
      gameModeDisplay: gameModeMap[gameMode] ?? gameMode,
      uuInstalled: (response.attributes[ServerAttribute.uu] as number) === 1,
      gameOptions,
      forcedMutators,
      passwordProtected:
        (response.attributes[ServerAttribute.uuServerFlags] as number) === 1,
      soloScores: response.attributes[ServerAttribute.uuSoloScores] as string,
      teamScores: response.attributes[ServerAttribute.uuTeamScores] as string,
      teamSizes: response.attributes[ServerAttribute.uuTeamSizes] as string
    };
  }

  return {
    mapMatch
  };
}
