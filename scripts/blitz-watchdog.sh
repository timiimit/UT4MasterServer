#!/usr/bin/env bash
# Watchdog that keeps a ranked Blitz (FlagRun) server alive for QuickPlay.
#
# AUTGameSessionRanked transitions the world to UTEmptyServerGameMode after a
# match finishes or the server idles. The empty-mode server still accepts
# QuickPlay reservations (because of the AUTPartyBeaconHost IsValid bypass
# baked into ut4-install — see ut4-install/pkgs/ut4-server-base.nix), but the
# joining client lands on the empty UT-Entry void map instead of FR-MeltDown.
#
# This script tails the server's Saved log and restarts the process whenever
# it detects the empty-mode transition, so subsequent QuickPlay clicks always
# land in a fresh FlagRun match.
#
# Configurable via env vars (all optional):
#   INSTALL_DIR  default /home/lucas/Games/UT4-server/LinuxServer
#   PORT         default 7779
#   QUERY_PORT   default 7789
#   STEAMRUN     default steam-run from PATH
#   MAP          default FR-MeltDown
#   GAMEMODE     default /Script/UnrealTournament.UTFlagRunGame
#   MAX_PLAYERS  default 10
#   BOT_FILL     default 10
#   MIN_PLAYERS  default 1
#   RUN_LOG      default /tmp/ut4-blitz.log

set -uo pipefail

INSTALL_DIR=${INSTALL_DIR:-/home/lucas/Games/UT4-server/LinuxServer}
PORT=${PORT:-7779}
QUERY_PORT=${QUERY_PORT:-7789}
STEAMRUN=${STEAMRUN:-steam-run}
MAP=${MAP:-FR-MeltDown}
GAMEMODE=${GAMEMODE:-/Script/UnrealTournament.UTFlagRunGame}
MAX_PLAYERS=${MAX_PLAYERS:-10}
BOT_FILL=${BOT_FILL:-10}
MIN_PLAYERS=${MIN_PLAYERS:-1}
RUN_LOG=${RUN_LOG:-/tmp/ut4-blitz.log}

SAVED_LOG="$INSTALL_DIR/UnrealTournament/Saved/Logs/UnrealTournament.log"
URL="${MAP}?Game=${GAMEMODE}?MaxPlayers=${MAX_PLAYERS}?MatchmakingSession=1?BotFill=${BOT_FILL}?MinPlayers=${MIN_PLAYERS}?MaxPlayerWait=1?QuickMatch=1"

start_blitz() {
  echo "[$(date '+%H:%M:%S')] watchdog: starting fresh Blitz on port $PORT" >&2
  pkill -f "UE4Server-Linux-Shipping.*port=$PORT" 2>/dev/null || true
  sleep 2
  ( cd "$INSTALL_DIR" \
    && LD_LIBRARY_PATH="$INSTALL_DIR/Engine/Binaries/Linux" \
       "$STEAMRUN" ./Engine/Binaries/Linux/UE4Server-Linux-Shipping \
       UnrealTournament "$URL" \
       -epicapp=UnrealTournamentDev -epicenv=Prod -Unattended \
       -log -port=$PORT -queryport=$QUERY_PORT \
       >"$RUN_LOG" 2>&1 \
  ) &
  echo "[$(date '+%H:%M:%S')] watchdog: Blitz PID=$!" >&2
  sleep 5
}

start_blitz

while true; do
  # --pid=$$ exits the tail when the watchdog dies.
  tail -F --pid=$$ "$SAVED_LOG" 2>/dev/null | while read -r line; do
    case "$line" in
      *"Server switch level: ut-entry?game=empty"*)
        echo "[$(date '+%H:%M:%S')] watchdog: empty-mode transition detected, restarting" >&2
        sleep 2
        start_blitz
        break
        ;;
    esac
  done
  echo "[$(date '+%H:%M:%S')] watchdog: tail exited, restarting tail loop" >&2
  sleep 1
done
