import { GameMode } from '../enums/game-mode';

export const gameModeMap: Record<GameMode, string> = {
	[GameMode.duel]: 'Duel',
	[GameMode.duel2]: 'Duel',
	[GameMode.deathmatch]: 'Deathmatch',
	[GameMode.ctf]: 'Capture the Flag',
	[GameMode.blitz]: 'Blitz',
	[GameMode.empty]: 'Empty',
	[GameMode.hub]: 'Hub',
	[GameMode.siege]: 'Siege',
	[GameMode.teamDeathmatch]: 'Team Deathmatch',
	[GameMode.showdown]: 'Showdown',
	[GameMode.elimination]: 'Elimination',
	[GameMode.bunnytrack]: 'Bunny Track'
};
