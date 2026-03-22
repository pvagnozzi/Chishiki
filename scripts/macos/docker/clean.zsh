#!/usr/bin/env zsh
# ==============================================================================
# clean — Remove project Docker images and prune dangling layers
# ==============================================================================
#
# SYNOPSIS
#   ./clean.zsh [--force] [--dangling-only] [--help]
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
#   ./clean.zsh
#   ./clean.zsh --force
#   ./clean.zsh --dangling-only
#
# NOTES
#   Requires: zsh 5.8+, Docker Desktop
#   Idempotent: safe to run when no images are present.
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

trap 'fail "Unexpected error on line $LINENO — exit code: $?"; exit 1' ERR TERM INT

OPT_FORCE=0
OPT_DANGLING_ONLY=0
for arg in "$@"; do
  case "$arg" in
    --force)         OPT_FORCE=1 ;;
    --dangling-only) OPT_DANGLING_ONLY=1 ;;
    --help|-h)       grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

header '🧹 Clean Docker Images'

if ! command -v docker &>/dev/null; then
  fail 'docker not found — please install Docker Desktop'; exit 1
fi
if ! docker info &>/dev/null 2>&1; then
  fail 'Docker Desktop is not running — please start it first'; exit 1
fi

# ── Remove chishiki/* images ──────────────────────────────────────────────────
if [[ "$OPT_DANGLING_ONLY" != "1" ]]; then
  step 'Finding chishiki/* images...'
  typeset -a IMAGES
  IMAGES=(${(f)"$(docker images --format '{{.Repository}}:{{.Tag}}' 2>/dev/null | grep '^chishiki/' || true)"})

  if (( ${#IMAGES[@]} > 0 )); then
    if [[ "$OPT_FORCE" != "1" ]]; then
      print ""
      for img in "${IMAGES[@]}"; do info "$img"; done
      print ""
      read -r "ans?  Remove these ${#IMAGES[@]} image(s)? (yes/no): "
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
if (( DANGLING > 0 )); then
  docker image prune -f 2>/dev/null | tail -1
  ok 'Dangling layers pruned'
else
  ok 'No dangling layers found'
fi

print "\n  ${GREEN}${B}✔ Docker clean complete.${R}\n"
