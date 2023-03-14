import { GameMode } from '../enums/game-mode';
import { MatchState } from '../enums/match-state';

export interface IMatch {
  id: string;
  name: string;
  gameType: string;
  map: string;
  duration: number;
  elapsedTime: number;
  publicPlayers: string[];
  playersOnline: number;
  maxPlayers: number;
  matchState: MatchState;
  matchStateDisplay: string;
  gameMode: GameMode;
  gameModeDisplay: string;
  mutators: string[];
  uuInstalled: boolean;
  gameOptions: Record<string, string>;
  forcedMutators: string[];
  passwordProtected: boolean;
  teamSizes: string;
  teamScores: string;
  soloScores: string;
}
