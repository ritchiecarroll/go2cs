#!/usr/bin/env bash
# TRAIN K POST-ASSERTS (PRERES-K8 + PRERES-K9) on the ASSEMBLED tree -- the checks a clean-looking resolution can fail
# silently. READ-ONLY: greps files and reads git objects; builds, converts and writes nothing. Called by tK-fixup.sh
# as a precondition and by the TRAIN K battery's PRE.
#   bash tK-postasserts.sh [repo]          # default /h/go2cs-tmp-coord/tK; reads the WORKTREE files
#   REV=<commit> bash tK-postasserts.sh    # reads the files as committed at <commit> instead
# Exit 0 = POSTASSERTS PASS, 4 = at least one FAIL (every check runs and prints; nothing short-circuits).
set -u
R=${1:-/h/go2cs-tmp-coord/tK}
REV=${REV:-}
BASE=f819887fa3
C1=ed7859d20e   # c1-print-fidelity (PRERES-K9: print.go + runtime.go map lines, t_chunk)
A3=b9c8948630   # i9-a3-runtime-lock-profile (PRERES-K9: chan.go + lock_spinbit.go + lockrank_off.go, t_runtimeLockProfilePending)
fails=0
rd(){ if [ -n "$REV" ]; then git -C "$R" show "$REV:$1" 2>/dev/null; else cat "$R/$1" 2>/dev/null; fi; }
cnt(){ rd "$1" | tr -d '\r' | grep -cF -- "$2"; }      # fixed-string line count (grep -c counts LINES)
chk(){ # label, got, want
  if [ "$2" = "$3" ]; then echo "  OK   $1 = $2"; else echo "  FAIL $1 = $2 (want $3)"; fails=$((fails + 1)); fi
}
git -C "$R" rev-parse --verify -q "${REV:-HEAD}^{commit}" > /dev/null || { echo "POSTASSERTS ABORT: $R is not a repo or ${REV:-HEAD} is unknown"; exit 4; }
if [ -n "$REV" ]; then echo "POSTASSERTS repo=$R reading=REV $REV = $(git -C "$R" rev-parse --short=10 "$REV^{commit}")"
else echo "POSTASSERTS repo=$R reading=worktree (HEAD $(git -C "$R" rev-parse --short=10 HEAD))"; fi

G=src/core/golib/runtime/Goroutine.cs
T=src/tests/GolibTests/ThreadStateCensusTests.cs
[ -n "$(rd "$G")" ] || { echo "  FAIL $G unreadable"; fails=$((fails + 1)); }
[ -n "$(rd "$T")" ] || { echo "  FAIL $T unreadable"; fails=$((fails + 1)); }

echo "K8 (Goroutine.cs: G's gated Register + P2's creatorILOffset; G's DECISIVE assert first)"
chk "count 'm_profileLabels = ' (the initializer seed inside Register only; P2's tip reads 2)" "$(cnt "$G" 'm_profileLabels = ')" 1
chk "count 's_live[goroutine.Id] = goroutine;' (the live-set insertion --theirs would lose)" "$(cnt "$G" 's_live[goroutine.Id] = goroutine;')" 1
chk "count 'goroutine.m_profileLabels = s_profileLabels.Value' (post-Register seed = the torn snapshot)" "$(cnt "$G" 'goroutine.m_profileLabels = s_profileLabels.Value')" 0
chk "count 'int creatorILOffset = System.Diagnostics.StackFrame.OFFSET_UNKNOWN)' (Register's trailing parameter kept DEFAULTED, G review)" "$(cnt "$G" 'int creatorILOffset = System.Diagnostics.StackFrame.OFFSET_UNKNOWN)')" 1
chk "count the Start-path call 'Register(isMain: false, creator, parentId, entry, s_profileLabels.Value, creatorILOffset);'" "$(cnt "$G" 'Register(isMain: false, creator, parentId, entry, s_profileLabels.Value, creatorILOffset);')" 1
chk "count the constructor order '(..., isMain, creator, creatorILOffset, parentId, entry)' inside Register" "$(cnt "$G" 'isMain, creator, creatorILOffset, parentId, entry)')" 1

echo "K9 (ThreadStateCensusTests.cs rows = base + 2; runtime package_info.cs map lines from the seat that changed them)"
rowrx='^\s*\("[^"]+\|[^"]+", Disposition\.'
base_rows=$(git -C "$R" show "$BASE:$T" | tr -d '\r' | grep -cE "$rowrx")
chk "census rows (base $base_rows + 2)" "$(rd "$T" | tr -d '\r' | grep -cE "$rowrx")" $((base_rows + 2))
chk "row t_chunk KeptThreadResource" "$(cnt "$T" '|t_chunk", Disposition.KeptThreadResource')" 1
chk "row t_runtimeLockProfilePending Registered" "$(cnt "$T" '|t_runtimeLockProfilePending", Disposition.Registered')" 1
K9_PAIRS_WANT=15   # 5 sources x 3 OS: every pair must actually be compared, else a format change could skip them all
k9n=0
for os in windows linux darwin; do
  P=src/core/runtime/$os/package_info.cs
  chk "$os map-line count vs base" "$(rd "$P" | tr -d '\r' | grep -c '^\[assembly: go\.GoPositionMap(')" "$(git -C "$R" show "$BASE:$P" | tr -d '\r' | grep -c '^\[assembly: go\.GoPositionMap(')"
  for pair in print.go:$C1 runtime.go:$C1 chan.go:$A3 lock_spinbit.go:$A3 lockrank_off.go:$A3; do
    src=${pair%%:*}; sha=${pair#*:}
    rx="^\[assembly: go\.GoPositionMap\(\"runtime/${src//./\\.}\""
    want=$(git -C "$R" show "$sha:$P" | tr -d '\r' | grep -E "$rx")
    [ "$(printf '%s\n' "$want" | grep -c .)" = 1 ] || continue   # the ruling: compare only where the seat has exactly one
    k9n=$((k9n + 1))
    got=$(rd "$P" | tr -d '\r' | grep -E "$rx")
    if [ "$got" = "$want" ]; then echo "  OK   $os $src map line = ${sha}'s"; else echo "  FAIL $os $src map line differs from ${sha}'s"; fails=$((fails + 1)); fi
  done
done
chk "K9 map-line pairs actually compared (5 sources x 3 OS)" "$k9n" "$K9_PAIRS_WANT"

echo "MARKERS (no conflict marker survives in any file a TRAIN K resolution touched)"
for f in "$G" "$T" src/core/runtime/windows/package_info.cs src/core/runtime/linux/package_info.cs src/core/runtime/darwin/package_info.cs docs/ConversionStrategies-Reference/manual-conversions.md; do
  chk "markers in $f" "$(rd "$f" | tr -d '\r' | grep -cE '^(<<<<<<< |>>>>>>> |=======$)')" 0
done

if [ "$fails" = 0 ]; then echo "POSTASSERTS PASS"; exit 0; fi
echo "POSTASSERTS FAIL: $fails check(s)"; exit 4
