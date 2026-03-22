#!/usr/bin/env bash
# ==============================================================================
# clean — Remove project Docker images and prune dangling layers
# ==============================================================================
#
# SYNOPSIS
#   ./clean.sh [--force] [--dangling-only] [--help]
#
# DESCRIPTION
#   Removes all local Docker images tagged chishiki/* and then prunes
#   any dangling (untagged) image layers. Does not touch running containers
#   or volumes. Safe to re-run (idempotent).
#
# OPTIONS
#   --force           Skip the confirmation prompt
#   --dangling-only   Only prune dangling images, keep chishiki/* tags
#   --help, -h        Show this help and exit
#
# EXAMPLES
#   ./clean.sh
#   ./clean.sh --force
#   ./clean.sh --dangling-only
#
# NOTES
#   Requires: bash 4+, Docker Engine
#   Idempotent: safe to run when no images are present.
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

trap 'fail "Unexpected error on line $LINENO — exit code: $?"; exit 1' ERR

OPT_FORCE=0
OPT_DANGLING_ONLY=0
for arg in "$@"; do
  case "$arg" in
    --force)        OPT_FORCE=1 ;;
    --dangling-only) OPT_DANGLING_ONLY=1 ;;
    --help|-h)      grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🧹 Clean Docker Images'

if ! command -v docker &>/dev/null; then
  fail 'docker not found — please install Docker Engine'; exit 1
fi
if ! docker info &>/dev/null 2>&1; then
  fail 'Docker daemon is not running — please start Docker first'; exit 1
fi

# ── Remove chishiki/* images ──────────────────────────────────────────────────
if [[ "$OPT_DANGLING_ONLY" != "1" ]]; then
  step 'Finding chishiki/* images...'
  mapfile -t IMAGES < <(docker images --format '{{.Repository}}:{{.Tag}}' 2>/dev/null | grep '^chishiki/' || true)

  if [[ ${#IMAGES[@]} -gt 0 ]]; then
    if [[ "$OPT_FORCE" != "1" ]]; then
      echo ""
      for img in "${IMAGES[@]}"; do info "$img"; done
      echo ""
      read -rp "  Remove these ${#IMAGES[@]} image(s)? (yes/no): " ans
      if [[ "$ans" != "yes" ]]; then warn 'Aborted'; exit 0; fi
    fi
    for img in "${IMAGES[@]}"; do
      info "Removing: $img"
      docker rmi -f "$img" 2>/dev/null || true
    done
    ok "${#IMAGES[@]} image(s) removed"
  else
    ok 'No chishiki/* images found'
  fi
fi

# ── Prune dangling layers ─────────────────────────────────────────────────────
step 'Pruning dangling image layers...'
DANGLING=$(docker images -f 'dangling=true' -q 2>/dev/null | wc -l | tr -d ' ')
if [[ "$DANGLING" -gt 0 ]]; then
  docker image prune -f 2>/dev/null | tail -1
  ok 'Dangling layers pruned'
else
  ok 'No dangling layers found'
fi

printf "\n  ${GREEN}${B}✔ Docker clean complete.${R}\n\n"
