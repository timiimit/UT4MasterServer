import { GameMode } from '../enums/game-mode';
import { IMatch } from './match';

export interface IQuickPlayServer extends Partial<IMatch> {
	id: string;
	region: string;
	gameMode: GameMode;
	gameModeDisplay: string;
	targetElo?: number;
}
