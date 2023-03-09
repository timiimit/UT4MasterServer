import { GameMode } from '../enums/game-mode';

export interface IQuickPlayServer {
  id: string;
  region: string;
  gameMode: GameMode;
  gameModeDisplay: string;
  eloRangeStart?: number;
  eloRangeEnd?: number;
}
