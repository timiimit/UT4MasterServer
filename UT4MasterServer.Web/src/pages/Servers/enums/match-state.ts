export enum MatchState {
  playerIntro = 'PlayerIntro',
  countdown = 'CountdownToBegin',
  enteringOvertime = 'MatchEnteringOvertime',
  overtime = 'MatchIsInOvertime',
  mapVote = 'MapVoteHappening',
  intermission = 'MatchIntermission',
  exitingIntermission = 'MatchExitingIntermission',
  rankedAbandon = 'MatchRankedAbandon',
  waitingTravel = 'WaitingTravel',
  enteringMap = 'EnteringMap',
  waiting = 'WaitingToStart',
  inProgress = 'InProgress',
  waitingPostMatch = 'WaitingPostMatch',
  leavingMap = 'LeavingMap',
  aborted = 'Aborted'
}
