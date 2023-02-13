import { MatchState } from '../enums/match-state';

export const matchStateMap: Record<MatchState, string> = {
  [MatchState.playerIntro]: 'Player Intro',
  [MatchState.countdown]: 'Countdown to Begin',
  [MatchState.enteringOvertime]: 'Entering Overtime',
  [MatchState.overtime]: 'Match in Overtime',
  [MatchState.mapVote]: 'Map Vote Happening',
  [MatchState.intermission]: 'Match Intermission',
  [MatchState.exitingIntermission]: 'Match Exiting Intermission',
  [MatchState.rankedAbandon]: 'Ranked Match Abandoned',
  [MatchState.waitingTravel]: 'Waiting Travel',
  [MatchState.enteringMap]: 'Entering Map',
  [MatchState.waiting]: 'Waiting to Start',
  [MatchState.inProgress]: 'In Progress',
  [MatchState.waitingPostMatch]: 'Waiting Post Match',
  [MatchState.leavingMap]: 'Leaving Map',
  [MatchState.aborted]: 'Aborted'
};
