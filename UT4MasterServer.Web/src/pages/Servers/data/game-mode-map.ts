import { GameMode } from '../enums/game-mode';

export const gameModeMap: Record<GameMode, string> = {
  [GameMode.duel]: 'Duel',
  [GameMode.deathmatch]: 'Deathmatch',
  [GameMode.ctf]: 'Capture the Flag',
  [GameMode.blitz]: 'Blitz',
  [GameMode.empty]: 'Empty'
};
