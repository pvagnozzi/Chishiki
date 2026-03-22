#!/usr/bin/env bash
# ==============================================================================
# reset-data — Reset all persistent development data
# ==============================================================================
#
# SYNOPSIS
#   ./reset-data.sh [--force] [--help]
#
# DESCRIPTION
#   Stops the running stack, removes all project Docker containers,
#   and prunes unused Docker volumes (PostgreSQL data, Redis AOF/RDB,
#   Keycloak realm data, etc.). Use this to get a completely clean
#   local environment. Prompts for confirmation unless --force is given.
#
# OPTIONS
#   --force      Skip the confirmation prompt
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./reset-data.sh
#   ./reset-data.sh --force
#
# NOTES
#   Requires: bash 4+, Docker Engine
#   ⚠ DESTRUCTIVE: all local development data will be lost.
#   Idempotent: safe to run on an already-clean environment.
# ==============================================================================

set -euo pipefail
IFS=$'\n\t'
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

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

trap 'fail "Unexpected error on line $LINENO — exit code: $?"; exit 1' ERR

OPT_FORCE=0
for arg in "$@"; do
  case "$arg" in
    --force) OPT_FORCE=1 ;;
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🗑️  Reset Development Data'
printf "  ${RED}${B}⚠  This will permanently delete all local development data.${R}\n"
printf "     Databases, caches, and auth state will be wiped.\n"

if [[ "$OPT_FORCE" != "1" ]]; then
  echo ""
  read -rp "  Type 'yes' to confirm: " ans
  if [[ "$ans" != "yes" ]]; then
    warn 'Aborted — no data was removed'
    exit 0
  fi
fi

# ── Step 1: Stop the stack ────────────────────────────────────────────────────
step '1/3  Stopping the dev stack...'
STOP_SCRIPT="$SCRIPT_DIR/stop.sh"
if [[ -x "$STOP_SCRIPT" ]]; then
  bash "$STOP_SCRIPT"
else
  PIDS=$(pgrep -f "Chishiki.Infrastructure.Aspire.AppHost" 2>/dev/null || true)
  if [[ -n "$PIDS" ]]; then
    echo "$PIDS" | xargs kill -TERM 2>/dev/null || true
    sleep 2
    echo "$PIDS" | xargs kill -KILL 2>/dev/null || true
  fi
  ok 'Stack stopped'
fi

# ── Step 2: Remove project containers ────────────────────────────────────────
step '2/3  Removing project containers...'
if ! command -v docker &>/dev/null; then
  warn 'docker not found — skipping'
else
  PATTERNS="postgresql redis keycloak prometheus grafana qdrant ollama chishiki"
  FOUND=()
  while IFS= read -r cname; do
    for p in $PATTERNS; do
      if [[ "$cname" == *"$p"* ]]; then FOUND+=("$cname"); break; fi
    done
  done < <(docker ps -a --format '{{.Names}}' 2>/dev/null || true)

  if [[ ${#FOUND[@]} -gt 0 ]]; then
    for cname in "${FOUND[@]}"; do
      info "Removing: $cname"
      docker rm -f "$cname" 2>/dev/null || true
    done
    ok "${#FOUND[@]} container(s) removed"
  else
    ok 'No project containers found'
  fi
fi

# ── Step 3: Prune unused volumes ──────────────────────────────────────────────
step '3/3  Pruning unused Docker volumes...'
if ! command -v docker &>/dev/null; then
  warn 'docker not found — skipping'
else
  BEFORE=$(docker volume ls -q 2>/dev/null | wc -l | tr -d ' ')
  docker volume prune -f 2>/dev/null | tail -1
  AFTER=$(docker volume ls -q 2>/dev/null | wc -l | tr -d ' ')
  ok "Volume prune complete — $(( BEFORE - AFTER )) volume(s) removed"
fi

printf "\n  ${GREEN}${B}✔ Data reset complete. Run start.sh to rebuild from scratch.${R}\n\n"
