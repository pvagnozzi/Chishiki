#!/usr/bin/env bash
# ==============================================================================
# stop — Stop the development stack
# ==============================================================================
#
# SYNOPSIS
#   ./stop.sh [--help]
#
# DESCRIPTION
#   Finds and terminates the Aspire AppHost process, then stops any
#   lingering project Docker containers. Safe to run when the stack
#   is already stopped (idempotent).
#
# OPTIONS
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./stop.sh
#
# NOTES
#   Requires: bash 4+
#   Idempotent: safe to run if the stack is already stopped.
# ==============================================================================

set -euo pipefail
IFS=$'\n\t'

R='\033[0m'; B='\033[1m'
RED='\033[31m'; GREEN='\033[32m'; YELLOW='\033[33m'
BLUE='\033[34m'; CYAN='\033[36m'; WHITE='\033[37m'

header() { printf "\n${B}${BLUE}╔══════════════════════════════════════════════╗${R}\n"
           printf   "${B}${BLUE}║  ${WHITE}%-44s${BLUE}║${R}\n" "$1"
           printf   "${B}${BLUE}╚══════════════════════════════════════════════╝${R}\n"; }
step()   { printf "\n${B}${CYAN}  ──${R} %s\n" "$1"; }
ok()     { printf "    ${GREEN}✔${R}  %s\n" "$1"; }
warn()   { printf "    ${YELLOW}⚠${R}  %s\n" "$1"; }
fail()   { printf "    ${RED}✖${R}  %s\n" "$1"; }
info()   { printf "    ${CYAN}·${R}  %s\n" "$1"; }

for arg in "$@"; do
  case "$arg" in
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🛑 Stop Dev Stack'

# ── Stop AppHost process ──────────────────────────────────────────────────────
step 'Looking for Aspire AppHost process...'
PIDS=$(pgrep -f "Chishiki.Infrastructure.Aspire.AppHost" 2>/dev/null || true)

if [[ -n "$PIDS" ]]; then
  for pid in $PIDS; do
    info "Sending SIGTERM to PID $pid"
    kill -TERM "$pid" 2>/dev/null || true
  done
  sleep 2
  # Force-kill any survivors
  for pid in $PIDS; do
    kill -0 "$pid" 2>/dev/null && kill -KILL "$pid" 2>/dev/null || true
  done
  ok 'AppHost process stopped'
else
  warn 'No AppHost process found — may already be stopped'
fi

# ── Stop lingering Docker containers ─────────────────────────────────────────
step 'Checking for lingering project containers...'
if ! command -v docker &>/dev/null; then
  warn 'docker not found — skipping container cleanup'
else
  PATTERNS="postgresql redis keycloak prometheus grafana qdrant ollama chishiki"
  FOUND=()
  while IFS= read -r cname; do
    for p in $PATTERNS; do
      if [[ "$cname" == *"$p"* ]]; then
        FOUND+=("$cname"); break
      fi
    done
  done < <(docker ps -a --format '{{.Names}}' 2>/dev/null || true)

  if [[ ${#FOUND[@]} -gt 0 ]]; then
    for cname in "${FOUND[@]}"; do
      info "Removing container: $cname"
      docker rm -f "$cname" 2>/dev/null || true
    done
    ok "${#FOUND[@]} container(s) removed"
  else
    ok 'No lingering project containers found'
  fi
fi

printf "\n  ${GREEN}${B}✔ Stack stopped.${R}\n\n"
