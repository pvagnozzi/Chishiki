#!/usr/bin/env bash
# ==============================================================================
# build-all — Build all Docker images
# ==============================================================================
#
# SYNOPSIS
#   ./build-all.sh [--no-cache] [--filter <name>] [--help]
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
#   ./build-all.sh
#   ./build-all.sh --no-cache
#   ./build-all.sh --filter ollama
#
# NOTES
#   Requires: bash 4+, Docker Engine (running)
#   Idempotent: re-running updates images in place.
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
  fail 'docker not found — please install Docker Engine'; exit 1
fi
if ! docker info &>/dev/null 2>&1; then
  fail 'Docker daemon is not running — please start Docker first'; exit 1
fi
ok 'Docker daemon reachable'

# ── Discover Dockerfiles ──────────────────────────────────────────────────────
step 'Discovering Dockerfiles...'

declare -a DOCKERFILES=()
for search_dir in "$REPO_ROOT/containers" "$REPO_ROOT/src/backend"; do
  [[ -d "$search_dir" ]] || continue
  while IFS= read -r df; do
    dir_name="$(basename "$(dirname "$df")")"
    name="${dir_name,,}"  # lowercase
    name="${name//[^a-z0-9-]/-}"
    if [[ -n "$OPT_FILTER" && "$name" != *"$OPT_FILTER"* ]]; then continue; fi
    DOCKERFILES+=("$df|$name")
  done < <(find "$search_dir" -maxdepth 3 -name 'Dockerfile' 2>/dev/null | sort)
done

if [[ ${#DOCKERFILES[@]} -eq 0 ]]; then
  warn 'No Dockerfiles found matching the current filter'
  exit 0
fi

ok "Found ${#DOCKERFILES[@]} Dockerfile(s)"
for entry in "${DOCKERFILES[@]}"; do
  IFS='|' read -r _ name <<< "$entry"
  info "  chishiki/${name}:latest"
done

# ── Build each image ──────────────────────────────────────────────────────────
step 'Building images...'
FAILED=()
BUILT=0

for entry in "${DOCKERFILES[@]}"; do
  IFS='|' read -r df name <<< "$entry"
  tag="chishiki/${name}:latest"
  printf "\n  ${B}${WHITE}▶ Building %s${R}\n" "$tag"
  info "  Dockerfile: $df"

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
if [[ ${#FAILED[@]} -gt 0 ]]; then
  fail "${#FAILED[@]} image(s) failed:"
  for t in "${FAILED[@]}"; do info "  $t"; done
  exit 1
fi
printf "\n  ${GREEN}${B}✔ All images built.${R}\n\n"
