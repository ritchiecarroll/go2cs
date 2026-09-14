#!/usr/bin/env bash
# Assert the train-31 merge-result invariants. Read-only.
#   usage: coord-merge-invariants.sh [ref]        (default: HEAD)
#          coord-merge-invariants.sh --self-test  (each check must FIRE)
# Every check names what it asserts and why a file-level merge resolution breaks it.
set -uo pipefail
REPO="${REPO:-/c/Projects/go2cs}"
cd "$REPO" || exit 2
FAIL=0
ZS=src/core/syscall/darwin/zsyscall_darwin_amd64.cs
BOARD=docs/phase4/BOARD-next-validation-candidates.md
MASTER_REF=origin/master

blob() { git show "$1:$2" 2>/dev/null; }

check() { # name expected actual why
  if [ "$2" = "$3" ]; then printf '  OK   %-34s %s\n' "$1" "$3"
  else printf '  FAIL %-34s expected=%s actual=%s\n       %s\n' "$1" "$2" "$3" "$4"; FAIL=1; fi
}

run_checks() {
  local ref="$1"
  echo "== merge invariants at $ref =="

  # C1 -- darwin zsyscall pins. A file-level take of EITHER side compiles clean and
  # loses the other's work: taking C2's branch drops master's 105 pins, taking
  # master's drops C2's pipe displacement. The COUNT is the check, not the diff.
  # DERIVED, not a literal. A displaced body takes ITS OWN pin with it, so the
  # expected count is master's total minus the pins inside functions this ref
  # turned into placeholders. Measured 2026-09-06: the recorded rule said "assert
  # 105" and a CORRECT resolution reads 104, because one of master's 105 sits
  # inside the `pipe` body that c2-darwin-inc10 displaces. A literal here would
  # false-red the one file where a careless resolution silently destroys work.
  m_pins=$(blob "$MASTER_REF" "$ZS" | grep -c 'System.GC.KeepAlive')
  r_pins=$(blob "$ref" "$ZS" | grep -c 'System.GC.KeepAlive')
  ph=$(blob "$ref" "$ZS" | grep -c 'go2cs generated this placeholder')
  lost=$((m_pins - r_pins))
  ok=$([ "$lost" -ge 0 ] && [ "$lost" -le "$ph" ] && echo yes || echo no)
  check "darwin pins lost <= placeholders" "yes" "$ok" \
        "master=$m_pins here=$r_pins lost=$lost placeholders=$ph -- a file-level take loses ALL of them"

  # C2 -- BOARD Liquid guard. A docs seat once deleted the closing guard and appended
  # below it; the new section published INSIDE a comment, invisible, and no gate saw it.
  local raw endraw last total
  raw=$(blob "$ref" "$BOARD" | grep -cE '\{%-? *raw')
  endraw=$(blob "$ref" "$BOARD" | grep -cE '\{%-? *endraw')
  last=$(blob "$ref" "$BOARD" | grep -nE '\{%-? *endraw' | tail -1 | cut -d: -f1)
  total=$(blob "$ref" "$BOARD" | wc -l | tr -d ' ')
  check "BOARD raw guards"    "1" "$raw"    "exactly one raw opener"
  check "BOARD endraw guards" "1" "$endraw" "exactly one endraw closer"
  check "BOARD endraw is FINAL line" "$total" "$last" \
        "content appended BELOW endraw publishes outside the guard; ABOVE a deleted one publishes inside a comment"

  # (A comment-balance check was written here and DELETED: it fired on the board's own
  #  PROSE describing this very trap -- backticked `<!-- ` examples at lines 22394/22408 --
  #  so it failed on a known-good master. A check that cannot pass its baseline is not a
  #  check. The raw/endraw/endraw-final trio above already covers the real invariant.)

  # C4 -- conflict markers in any tracked blob. A resolver that fails must stop the
  # commit; one chained with ';' rather than '&&' has committed markers before.
  local marks
  marks=$(git grep -c -E '^<<<<<<< |^>>>>>>> ' "$ref" -- . 2>/dev/null | wc -l | tr -d ' ')
  check "files with conflict markers" "0" "$marks" "grep every merge commit's blobs before pushing"
}

if [ "${1:-}" = "--self-test" ]; then
  echo "== SELF-TEST: every check must FIRE (a green that cannot go red is not a measurement) =="
  T=$(mktemp -d); trap 'rm -rf "$T"' EXIT
  git worktree add -q --detach "$T" origin/master 2>/dev/null || { echo "cannot make control worktree"; exit 2; }
  ( cd "$T"
    sed -i '0,/System.GC.KeepAlive/{s/System.GC.KeepAlive/System.GC.KeepAliveX/}' "$ZS"
    printf '\ncontent below the guard\n' >> "$BOARD"
    git commit -qam "control: violate two invariants" )
  CTRL=$(cd "$T" && git rev-parse HEAD)
  echo "control ref $CTRL"
  REPO="$T" run_checks HEAD 2>/dev/null || true
  ( cd "$T" && REPO="$T" bash "$REPO/../coord-scripts/coord-merge-invariants.sh" HEAD ) 2>/dev/null
  git worktree remove -f "$T" 2>/dev/null
  echo "(inspect above: pin count and endraw-final MUST read FAIL, or the instrument is dead)"
  exit 0
fi

# REFUSE an ambiguous ref. $REPO is fixed, so "HEAD" resolves in the MAIN checkout
# no matter which worktree the caller is standing in -- measured 2026-09-06, when a
# run from the assembly worktree read the main checkout's two-trains-stale HEAD and
# reported a FALSE RED on the pin count. The direction was safe that time; the same
# defect yields a false GREEN whenever the main checkout happens to pass.
REF="${1:-}"
case "$REF" in
  ""|HEAD|@|HEAD*|@*)
    echo "REFUSING an ambiguous ref (\"$REF\")." >&2
    echo "  \$REPO is fixed at $REPO, so HEAD resolves THERE, not in your worktree." >&2
    echo "  Pass an explicit ref: a sha, origin/master, or a branch name." >&2
    exit 2 ;;
esac
run_checks "$REF"
[ "$FAIL" = "0" ] && echo "ALL INVARIANTS HOLD" || echo "INVARIANTS VIOLATED -- do not push this merge"
exit "$FAIL"
