#!/usr/bin/env zsh
# ==============================================================================
# stop — Stop the development stack
# ==============================================================================
#
# SYNOPSIS
#   ./stop.zsh [--help]
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
#   ./stop.zsh
#
# NOTES
#   Requires: zsh 5.8+
#   Idempotent: safe to run if the stack is already stopped.
# ==============================================================================

emulate -L zsh
setopt errexit nounset pipefail

R=$'\e[0m'; B=$'\e[1m'
RED=$'\e[31m'; GREEN=$'\e[32m'; YELLOW=$'\e[33m'
BLUE=$'\e[34m'; CYAN=$'\e[36m'; WHITE=$'\e[37m'

header() { print "\n${B}${BLUE}╔══════════════════════════════════════════════╗${R}"
           printf  "${B}${BLUE}║  ${WHITE}%-44s${BLUE}║${R}\n" "$1"
           print   "${B}${BLUE}╚══════════════════════════════════════════════╝${R}"; }
step()   { print "\n${B}${CYAN}  ──${R} $1"; }
ok()     { print "    ${GREEN}✔${R}  $1"; }
warn()   { print "    ${YELLOW}⚠${R}  $1"; }
fail()   { print "    ${RED}✖${R}  $1"; }
info()   { print "    ${CYAN}·${R}  $1"; }

for arg in "$@"; do
  case "$arg" in
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🛑 Stop Dev Stack'

# ── Stop AppHost process ──────────────────────────────────────────────────────
step 'Looking for Aspire AppHost process...'
typeset -a PIDS
PIDS=(${(f)"$(pgrep -f "Chishiki.Infrastructure.Aspire.AppHost" 2>/dev/null)"} )

if (( ${#PIDS[@]} > 0 )); then
  for pid in "${PIDS[@]}"; do
    info "Sending SIGTERM to PID $pid"
    kill -TERM "$pid" 2>/dev/null || true
  done
  sleep 2
  for pid in "${PIDS[@]}"; do
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
  typeset -a PATTERNS FOUND
  PATTERNS=(postgresql redis keycloak prometheus grafana qdrant ollama chishiki)
  FOUND=()

  while IFS= read -r cname; do
    for p in "${PATTERNS[@]}"; do
      if [[ "$cname" == *"$p"* ]]; then
        FOUND+=("$cname"); break
      fi
    done
  done < <(docker ps -a --format '{{.Names}}' 2>/dev/null || true)

  if (( ${#FOUND[@]} > 0 )); then
    for cname in "${FOUND[@]}"; do
      info "Removing container: $cname"
      docker rm -f "$cname" 2>/dev/null || true
    done
    ok "${#FOUND[@]} container(s) removed"
  else
    ok 'No lingering project containers found'
  fi
fi

print "\n  ${GREEN}${B}✔ Stack stopped.${R}\n"
