import { MatchState } from '../enums/match-state';

export const matchStateMap: Record<MatchState, string> = {
  [MatchState.inProgress]: 'In Progress',
  [MatchState.waiting]: 'Waiting to Start',
  [MatchState.intermission]: 'Match Intermission',
  [MatchState.playerIntro]: 'Player Intro'
};
