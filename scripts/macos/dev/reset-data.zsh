#!/usr/bin/env zsh
# ==============================================================================
# reset-data — Reset all persistent development data
# ==============================================================================
#
# SYNOPSIS
#   ./reset-data.zsh [--force] [--help]
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
#   ./reset-data.zsh
#   ./reset-data.zsh --force
#
# NOTES
#   Requires: zsh 5.8+, Docker Desktop
#   ⚠ DESTRUCTIVE: all local development data will be lost.
#   Idempotent: safe to run on an already-clean environment.
# ==============================================================================

emulate -L zsh
setopt errexit nounset pipefail
SCRIPT_DIR="${0:A:h}"

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

trap 'fail "Unexpected error on line $LINENO — exit code: $?"; exit 1' ERR TERM INT

OPT_FORCE=0
for arg in "$@"; do
  case "$arg" in
    --force) OPT_FORCE=1 ;;
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🗑️  Reset Development Data'
print "  ${RED}${B}⚠  This will permanently delete all local development data.${R}"
print "     Databases, caches, and auth state will be wiped."

if [[ "$OPT_FORCE" != "1" ]]; then
  print ""
  read -r "ans?  Type 'yes' to confirm: "
  if [[ "$ans" != "yes" ]]; then
    warn 'Aborted — no data was removed'
    exit 0
  fi
fi

# ── Step 1: Stop the stack ────────────────────────────────────────────────────
step '1/3  Stopping the dev stack...'
STOP_SCRIPT="$SCRIPT_DIR/stop.zsh"
if [[ -x "$STOP_SCRIPT" ]]; then
  zsh "$STOP_SCRIPT"
else
  typeset -a PIDS
  PIDS=(${(f)"$(pgrep -f "Chishiki.Infrastructure.Aspire.AppHost" 2>/dev/null)"})
  for pid in "${PIDS[@]:-}"; do
    kill -TERM "$pid" 2>/dev/null || true
  done
  sleep 2
  for pid in "${PIDS[@]:-}"; do
    kill -0 "$pid" 2>/dev/null && kill -KILL "$pid" 2>/dev/null || true
  done
  ok 'Stack stopped'
fi

# ── Step 2: Remove project containers ────────────────────────────────────────
step '2/3  Removing project containers...'
if ! command -v docker &>/dev/null; then
  warn 'docker not found — skipping'
else
  typeset -a PATTERNS FOUND
  PATTERNS=(postgresql redis keycloak prometheus grafana qdrant ollama chishiki)
  FOUND=()

  while IFS= read -r cname; do
    for p in "${PATTERNS[@]}"; do
      if [[ "$cname" == *"$p"* ]]; then FOUND+=("$cname"); break; fi
    done
  done < <(docker ps -a --format '{{.Names}}' 2>/dev/null || true)

  if (( ${#FOUND[@]} > 0 )); then
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

print "\n  ${GREEN}${B}✔ Data reset complete. Run start.zsh to rebuild from scratch.${R}\n"
