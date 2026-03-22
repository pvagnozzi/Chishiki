#!/usr/bin/env zsh
# ==============================================================================
# start — Start the full development stack via Aspire AppHost
# ==============================================================================
#
# SYNOPSIS
#   ./start.zsh [--detach] [--help]
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
#   ./start.zsh
#   ./start.zsh --detach
#
# NOTES
#   Requires: zsh 5.8+, .NET 10 SDK, Docker Desktop (running)
#   Idempotent: safe to re-run if the stack is already up.
# ==============================================================================

emulate -L zsh
setopt errexit nounset pipefail
SCRIPT_DIR="${0:A:h}"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

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
  fail '.NET SDK not found. Run: scripts/macos/setup/install-prereqs.zsh --install'
  exit 1
fi
ok ".NET SDK $(dotnet --version)"

if ! docker info &>/dev/null 2>&1; then
  fail 'Docker Desktop is not running — please start it first'
  exit 1
fi
ok 'Docker daemon reachable'

[[ -f "$APP_HOST_PROJ" ]] || { fail "AppHost project not found: $APP_HOST_PROJ"; exit 1 }

step 'Checking if stack is already running...'
if nc -z localhost "$DASHBOARD_PORT" 2>/dev/null; then
  warn "Stack already running — dashboard at ${CYAN}${DASHBOARD_URL}${R}"
  print ""
  exit 0
fi

step 'Launching Aspire AppHost...'
print ""

if [[ "$OPT_DETACH" == "1" ]]; then
  cd "$REPO_ROOT"
  dotnet run --project "$APP_HOST_PROJ" > "$LOG_FILE" 2>&1 &
  APPHOST_PID=$!
  ok  "AppHost started (PID $APPHOST_PID)"
  info "Logs:     $LOG_FILE"
  info "Dashboard available at ${CYAN}${DASHBOARD_URL}${R} in ~30 s"
  info "Stop with: scripts/macos/dev/stop.zsh"
else
  warn 'Running in foreground — press Ctrl+C to stop the stack'
  print ""
  cd "$REPO_ROOT"
  exec dotnet run --project "$APP_HOST_PROJ"
fi
