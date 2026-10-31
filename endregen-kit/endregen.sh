#!/usr/bin/env bash
# endregen.sh <tip> [work-dir] -- the END REGEN kit (docs/PLAN-parity-train.md §3.4 step 1-2, DESIGN-ladder-identity
# Step C/D): given a tip SHA, emit the standard library at that tip on three targets and measure it against the
# committed corpus. It never touches a repository's working tree and never applies anything.
#
#   1. preflight: 25 GB free, no go2cs.exe alive, the tip resolves, GOROOT spelled as `go env GOROOT` prints it;
#   2. seed: ONE frozen `git archive` of <tip> (src, docs/validation, docs/ValidatedTestPackages.md) extracted as
#      `committed/` (the baseline, never converted into) and as one root per target; every root's seeded .cs count
#      must equal the tip's tracked src/core .cs count (bin/obj/Generated are untracked, so the archive holds none);
#   3. converter: built from the same archive (`go build -trimpath -buildvcs=false`, the ladder recipe); its sha256
#      (first 16 hex) is recorded for Step C (it must equal the last rung's "after");
#   4. emit: `go2cs -stdlib -comments -platforms <goos>/amd64 -go2cspath <root>/src`, windows then linux then darwin,
#      each into its own root, one at a time; asserted: rc 0, the binary newer than its sources, a written-file count,
#      and no position sentinel (NUL) left in any written .cs (a raced or failed emission's tell);
#   5. measure (endregen_measure.py): per target, the files whose CR-normalised bytes differ from committed, by kind,
#      with per-file hunk counts; the path-precise marker gate; the unified diff for pcensus;
#   6. attribute: `pcensus classify` over each target's diff (every item), its refusals kept for the rung table.
#
# Never run two of these at once on one box (floor 1). Output: <work-dir>/run-<tip10>/{committed,<goos>,report/}.
set -u
TIP_ARG=${1:?usage: endregen.sh <tip> [work-dir]}
WORK=${2:-/c/go2cs-tmp/endregen}
REPO=${REPO:-$(git rev-parse --show-toplevel)}
KIT=$(cd "$(dirname "$0")" && pwd)
TARGETS=${TARGETS:-"windows linux darwin"}
FLOOR_GB=${FLOOR_GB:-25}

TIP=$(git -C "$REPO" rev-parse --verify "$TIP_ARG^{commit}") || { echo "REFUSED: cannot resolve $TIP_ARG"; exit 2; }
RUN=$WORK/run-${TIP:0:10}
REP=$RUN/report
mkdir -p "$REP" || exit 2
SUM=$REP/SUMMARY.txt
: > "$SUM"
say() { echo "$(date -u +%FT%TZ) $*" | tee -a "$SUM"; }
fail() { say "FAILED: $*"; exit 1; }

# --- 1. preflight -------------------------------------------------------------------------------------------------------
# stat -f, never df: a stale mapped drive hangs df on this box.
FREE=$(stat -f -c '%a %S' "$WORK" | awk '{print int($1*$2/1073741824)}')
[ "${FREE:-0}" -ge "$FLOOR_GB" ] || fail "free space ${FREE}G below ${FLOOR_GB}G"
if tasklist //FI "IMAGENAME eq go2cs.exe" 2>/dev/null | grep -qi 'go2cs.exe'; then fail "a go2cs.exe is alive (floor 1)"; fi
[ "$(go env GOROOT)" = "$GOROOT" ] || fail "go env GOROOT reads $(go env GOROOT), not GOROOT=$GOROOT (floor 6)"
GOV=$(go version)
echo "$GOV" | grep -q 'go1\.26\.10 ' || fail "go is $GOV, not go1.26.10"
[ "${GOTOOLCHAIN:-}" = local ] || fail "GOTOOLCHAIN is '${GOTOOLCHAIN:-}', not local"
say "endregen at $TIP; repo $REPO; free ${FREE}G; $GOV; GOROOT=$GOROOT; targets: $TARGETS"

# --- 2. seed ------------------------------------------------------------------------------------------------------------
rm -rf "$RUN/committed" "$RUN/seed.tar" && mkdir -p "$RUN/committed" || fail "cannot make $RUN/committed"
git -C "$REPO" archive -o "$RUN/seed.tar" "$TIP" src docs/validation docs/ValidatedTestPackages.md || fail "git archive"
tar -xf "$RUN/seed.tar" -C "$RUN/committed" || fail "extract committed"
# -z: ls-tree QUOTES a non-ASCII path ("src/core/golib/\320\266.cs"), whose closing quote hides the .cs suffix.
WANT=$(git -C "$REPO" ls-tree -r -z --name-only "$TIP" -- src/core | tr '\0' '\n' | grep -c '\.cs$')
HAVE=$(find "$RUN/committed/src/core" -name '*.cs' | wc -l)
[ "$HAVE" -eq "$WANT" ] || fail "committed .cs count $HAVE != tracked $WANT"
say "seed: one archive of $TIP; tracked src/core .cs = $WANT; committed/ holds $HAVE"
for T in $TARGETS; do
  R=$RUN/$T
  rm -rf "$R" && mkdir -p "$R" || fail "cannot make $R"
  tar -xf "$RUN/seed.tar" -C "$R" || fail "extract $T"
  N=$(find "$R/src/core" -name '*.cs' | wc -l)
  [ "$N" -eq "$WANT" ] || fail "$T seeded .cs count $N != $WANT"
  say "seed $T: $N .cs"
done

# --- 3. converter -------------------------------------------------------------------------------------------------------
EXE=$RUN/go2cs.exe
rm -f "$EXE"
(cd "$RUN/committed/src/go2cs" && go build -trimpath -buildvcs=false -o "$EXE" .) || fail "converter build"
[ -f "$EXE" ] && [ "$EXE" -nt "$RUN/committed/src/go2cs/main.go" ] || fail "converter binary not newer than its sources"
CONV_SHA=$(sha256sum "$EXE" | cut -c1-16)
say "converter: built from the tip archive, sha256 $CONV_SHA (Step C: must equal the last rung's 'after')"

# --- 4. emit, one target at a time --------------------------------------------------------------------------------------
for T in $TARGETS; do
  R=$RUN/$T
  if tasklist //FI "IMAGENAME eq go2cs.exe" 2>/dev/null | grep -qi 'go2cs.exe'; then fail "a go2cs.exe is alive before $T"; fi
  touch "$R/convert.stamp"; sleep 1
  s=$SECONDS
  "$EXE" -stdlib -comments -platforms "$T/amd64" -go2cspath "$(cygpath -w "$R/src")" > "$REP/convert-$T.out" 2>&1
  rc=$?
  WRITTEN=$(find "$R/src/core" -type f -newer "$R/convert.stamp" | wc -l)
  WRITTEN_CS=$(find "$R/src/core" -name '*.cs' -newer "$R/convert.stamp" | wc -l)
  say "emit $T: rc=$rc $((SECONDS - s))s; files written $WRITTEN (.cs $WRITTEN_CS); tail: $(tail -n 1 "$REP/convert-$T.out" | cut -c1-120)"
  [ "$rc" -eq 0 ] || fail "emit $T rc=$rc"
  [ "$WRITTEN_CS" -gt 500 ] || fail "emit $T wrote only $WRITTEN_CS .cs"
  python -I -X utf8 "$(cygpath -w "$KIT/endregen_measure.py")" nulcheck "$(cygpath -w "$R/src/core")" "$(cygpath -w "$R/convert.stamp")" >> "$SUM" 2>&1 \
    || fail "emit $T: a written .cs carries a position sentinel (NUL)"
done

# --- 5. measure ---------------------------------------------------------------------------------------------------------
python -I -X utf8 "$(cygpath -w "$KIT/endregen_measure.py")" "$(cygpath -w "$RUN")" $TARGETS >> "$SUM" 2>&1 || fail "measure"

# --- 6. attribute -------------------------------------------------------------------------------------------------------
# pcensus reads the patch git writes (the seats' rung readings were taken from git diffs); difflib's grouping pairs
# differently, and splits a mechanism's lines into unequal runs more often. --ignore-cr-at-eol is the CR normalisation.
for T in $TARGETS; do
  (cd "$RUN" && git -c core.autocrlf=false -c core.quotepath=false diff --no-index --ignore-cr-at-eol -U3 \
      committed/src/core "$T/src/core" > "$REP/gitdiff-$T.patch")
  (cd "$RUN/committed/src/go2cs" && go run ./internal/pcensus classify "$(cygpath -w "$REP/gitdiff-$T.patch")") > "$REP/pcensus-$T.txt" 2>&1
  rc=$?
  say "pcensus classify $T: rc=$rc (1 = refusals present; see pcensus-$T.txt)"
  python -I -X utf8 "$(cygpath -w "$KIT/endregen_refusals.py")" "$(cygpath -w "$REP/pcensus-$T.txt")" "$(cygpath -w "$REP/refusals-$T")" > "$REP/refusals-$T.txt" 2>&1
  say "refusals $T: $(grep '^== ' "$REP/refusals-$T.txt" | tr '\n' ' ' | cut -c1-300)"
done
say "converter sha256 $CONV_SHA; report in $REP"
say "ENDREGEN DONE"
