import { MatchState } from '../enums/match-state';

export interface IMatch {
  id: string;
  name: string;
  gameType: string;
  map: string;
  duration: number;
  publicPlayers: string[];
  playersOnline: number;
  maxPlayers: number;
  matchState: MatchState;
  matchStateDisplay: string;
}
