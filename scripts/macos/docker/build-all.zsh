#!/usr/bin/env zsh
# ==============================================================================
# build-all — Build all Docker images
# ==============================================================================
#
# SYNOPSIS
#   ./build-all.zsh [--no-cache] [--filter <name>] [--help]
#
# DESCRIPTION
#   Discovers and builds every Dockerfile in the repository:
#   infrastructure containers (containers/*) and service images
#   (src/backend/*/Dockerfile). Each image is tagged as
#   chishiki/<name>:latest using the parent directory as the name.
#   Safe to re-run — Docker layer caching makes subsequent builds fast.
#
# OPTIONS
#   --no-cache       Build without Docker layer cache (full rebuild)
#   --filter <name>  Only build images whose name contains <name>
#   --help, -h       Show this help and exit
#
# EXAMPLES
#   ./build-all.zsh
#   ./build-all.zsh --no-cache
#   ./build-all.zsh --filter ollama
#
# NOTES
#   Requires: zsh 5.8+, Docker Desktop (running)
#   Idempotent: re-running updates images in place.
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

OPT_NO_CACHE=0
OPT_FILTER=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --no-cache)   OPT_NO_CACHE=1; shift ;;
    --filter)     OPT_FILTER="$2"; shift 2 ;;
    --help|-h)    grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $1"; exit 1 ;;
  esac
done

header '🐳 Build All Docker Images'
info "Repo: $REPO_ROOT"

if ! command -v docker &>/dev/null; then
  fail 'docker not found — please install Docker Desktop'; exit 1
fi
if ! docker info &>/dev/null 2>&1; then
  fail 'Docker Desktop is not running — please start it first'; exit 1
fi
ok 'Docker daemon reachable'

# ── Discover Dockerfiles ──────────────────────────────────────────────────────
step 'Discovering Dockerfiles...'

typeset -a DOCKERFILES=()
for search_dir in "$REPO_ROOT/containers" "$REPO_ROOT/src/backend"; do
  [[ -d "$search_dir" ]] || continue
  while IFS= read -r df; do
    dir_name="$(basename "$(dirname "$df")")"
    name="${dir_name:l}"
    name="${name//[^a-z0-9-]/-}"
    if [[ -n "$OPT_FILTER" && "$name" != *"$OPT_FILTER"* ]]; then continue; fi
    DOCKERFILES+=("${df}|${name}")
  done < <(find "$search_dir" -maxdepth 3 -name 'Dockerfile' 2>/dev/null | sort)
done

if (( ${#DOCKERFILES[@]} == 0 )); then
  warn 'No Dockerfiles found matching the current filter'
  exit 0
fi

ok "Found ${#DOCKERFILES[@]} Dockerfile(s)"
for entry in "${DOCKERFILES[@]}"; do
  info "  chishiki/${entry##*|}:latest"
done

# ── Build each image ──────────────────────────────────────────────────────────
step 'Building images...'
typeset -a FAILED=()
BUILT=0

for entry in "${DOCKERFILES[@]}"; do
  df="${entry%%|*}"
  name="${entry##*|}"
  tag="chishiki/${name}:latest"
  printf "\n  ${B}${WHITE}▶ Building %s${R}\n" "$tag"
  info "  Dockerfile: $df"

  typeset -a BUILD_ARGS
  BUILD_ARGS=(build -f "$df" -t "$tag")
  [[ "$OPT_NO_CACHE" == "1" ]] && BUILD_ARGS+=(--no-cache)
  BUILD_ARGS+=("$REPO_ROOT")

  if docker "${BUILD_ARGS[@]}"; then
    ok "$tag — built successfully"
    (( BUILT++ )) || true
  else
    fail "$tag — FAILED"
    FAILED+=("$tag")
  fi
done

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
ok "$BUILT image(s) built successfully"
if (( ${#FAILED[@]} > 0 )); then
  fail "${#FAILED[@]} image(s) failed:"
  for t in "${FAILED[@]}"; do info "  $t"; done
  exit 1
fi
print "\n  ${GREEN}${B}✔ All images built.${R}\n"
