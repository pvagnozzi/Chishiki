#!/usr/bin/env bash
# ==============================================================================
# start — Start the full development stack via Aspire AppHost
# ==============================================================================
#
# SYNOPSIS
#   ./start.sh [--detach] [--help]
#
# DESCRIPTION
#   Launches the .NET Aspire AppHost which orchestrates all infrastructure
#   services and containers required for local development (PostgreSQL, Redis,
#   Keycloak, Ollama, Qdrant, Grafana, Prometheus, the Orleans host, and
#   the API). Checks if the stack is already running before launching
#   (idempotent). Dashboard: http://localhost:15888
#
# OPTIONS
#   --detach     Run the AppHost in the background and return immediately
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./start.sh
#   ./start.sh --detach
#
# NOTES
#   Requires: bash 4+, .NET 10 SDK, Docker Engine (running)
#   Idempotent: safe to re-run if the stack is already up.
# ==============================================================================

set -euo pipefail
IFS=$'\n\t'
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

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

APP_HOST_PROJ="$REPO_ROOT/src/infrastructure/aspire/Chishiki.Infrastructure.Aspire.AppHost/Chishiki.Infrastructure.Aspire.AppHost.csproj"
DASHBOARD_URL="http://localhost:15888"
DASHBOARD_PORT=15888
LOG_FILE="/tmp/chishiki-apphost.log"

OPT_DETACH=0
for arg in "$@"; do
  case "$arg" in
    --detach) OPT_DETACH=1 ;;
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🚀 Start Dev Stack'
info "Repo:      $REPO_ROOT"
info "Dashboard: ${CYAN}${DASHBOARD_URL}${R}"

step 'Checking prerequisites...'
if ! command -v dotnet &>/dev/null; then
  fail '.NET SDK not found. Run: scripts/linux/setup/install-prereqs.sh --install'
  exit 1
fi
ok ".NET SDK $(dotnet --version)"

if ! docker info &>/dev/null 2>&1; then
  fail 'Docker daemon is not running — please start Docker first'
  exit 1
fi
ok 'Docker daemon reachable'

[[ -f "$APP_HOST_PROJ" ]] || { fail "AppHost project not found: $APP_HOST_PROJ"; exit 1; }

step 'Checking if stack is already running...'
if nc -z localhost "$DASHBOARD_PORT" 2>/dev/null; then
  warn "Stack already running — dashboard at ${CYAN}${DASHBOARD_URL}${R}"
  echo ""
  exit 0
fi

step 'Launching Aspire AppHost...'
echo ""

if [[ "$OPT_DETACH" == "1" ]]; then
  cd "$REPO_ROOT"
  nohup dotnet run --project "$APP_HOST_PROJ" > "$LOG_FILE" 2>&1 &
  APPHOST_PID=$!
  ok  "AppHost started (PID $APPHOST_PID)"
  info "Logs:     $LOG_FILE"
  info "Dashboard available at ${CYAN}${DASHBOARD_URL}${R} in ~30 s"
  info "Stop with: scripts/linux/dev/stop.sh"
else
  warn 'Running in foreground — press Ctrl+C to stop the stack'
  echo ""
  cd "$REPO_ROOT"
  exec dotnet run --project "$APP_HOST_PROJ"
fi
