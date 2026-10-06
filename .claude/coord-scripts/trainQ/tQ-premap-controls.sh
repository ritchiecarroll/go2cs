#!/usr/bin/env bash
# R0 ONLY, NOT ADAPTED TO TRAIN Q (review round 1): this is trainP's pre-map reading tool under Q's name. It reads no ruled resolution (on Q's table every ruled row reads as a conflict), it needs git 2.44 or later through GITX, its comments below describe TRAIN P's pre-map, and it is on NO step of COORD-LAUNCH-CHECKLIST.md: the map of record is tQ-conflict-map.sh.
# tQ-premap-controls.sh -- the pre-map instruments made to PASS on a known union and to FAIL on planted ones
# (DRAFT 2026-10-04, UNCOMMITTED; floor 13: a gate that has never been made to fail proves nothing).
#   OUT=<dir> [CTL_FROM=54f7f4439d CTL_TO=eb88ab9492 P_BASE=eb88ab9492] bash tQ-premap-controls.sh
# CTL_FROM..CTL_TO is a LANDED train's union range (default: TRAIN O's, 38 seat merges + its fixup); P_BASE is the
# commit the planted controls stand on. These are controls' inputs, not P's base: P's base is never a literal.
# No worktree, no build; objects go to OUT/objects (scratch), nothing to the shared store.
#   C1 REPLAY     every first-parent merge of the range re-merged from its two real parents: tree IDENTICAL to git's.
#   C2 CHAIN      the range's seats replayed as a chain from CTL_FROM: the head tree equals the real assembled head's
#                 (the last merge of the range), with a criss-cross row in it when the train had one.
#   C3 REGCHECK   ok x6 at that head (a multi-base row and an identical insert credited); FAIL x6 at the bare base.
#   C4 CONFLICTS  refs known to conflict read CONFLICT on the named path (rows-controls-*.txt beside this run's logs).
#   C5 UNIONCHECK a planted twin Go file (duplicate declarations + no projitems entry), a duplicate declared below a
#                 backtick held in an interpreted string (review round 1: P13's class, which the parity filter skipped),
#                 a Go-only behavioral directory (H4), a marked tree (markers), an unqualified HashSet use; and head =
#                 base reads all zero.
#   C6 GOFMT      ort's marked tree does not parse; a REFINED union of the same file does not parse; the unrefined does.
set -u
HERE=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
: "${OUT:?OUT is required (a fresh folder)}"
CTL_FROM=${CTL_FROM:-54f7f4439d}; CTL_TO=${CTL_TO:-eb88ab9492}; P_BASE=${P_BASE:-eb88ab9492}
export MSYS_NO_PATHCONV=1
mkdir -p "$OUT/objects" || exit 2
OUT=$(cd "$OUT" && pwd); OUTW=$(cygpath -m "$OUT")
export GD=${GD:-H:/Projects/go2cs/.git}
export OBJDIR=$OUT/objects
export GIT_OBJECT_DIRECTORY=$(cygpath -m "$OBJDIR")
export GIT_ALTERNATE_OBJECT_DIRECTORIES="$GD/objects"
GITX=${GITX:-/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/CommonExtensions/Microsoft/TeamFoundation/Team Explorer/Git/cmd/git.exe}
GOFMT=${GOFMT:-/h/sdk/go1.24.13/bin/gofmt.exe}
g() { git --git-dir="$GD" -c core.quotepath=false "$@"; }
py() { python -B "$(cygpath -m "$HERE/$1")" "${@:2}"; }
pm() { ( cd "$HERE" && "$@" bash ./tQ-premap.sh ); }
fail=0; say() { echo "$1"; case "$1" in *'CONTROL FAILED'*) fail=$((fail+1));; esac; }
echo "PREMAP-CONTROLS range=$CTL_FROM..$CTL_TO planted-on=$P_BASE out=$OUT $(date '+%Y-%m-%d %H:%M %Z')"

# C1
pm env REPLAY_FROM=$CTL_FROM REPLAY_TO=$CTL_TO OUT=$OUT/tmp MODE=replay > "$OUT/c1-replay.log" 2>&1
l=$(tail -n 1 "$OUT/c1-replay.log"); nm=$(echo "$l" | sed -E 's/.*merges=([0-9]+).*/\1/'); ni=$(echo "$l" | sed -E 's/.*identical=([0-9]+).*/\1/')
cc=$(grep -c 'CRISS-CROSS' "$OUT/c1-replay.log")
[ -n "$nm" ] && [ "$nm" -ge 1 ] && [ "$nm" = "$ni" ] && say "C1 REPLAY ok: $ni of $nm merges identical to git's own trees ($cc criss-cross)" || say "C1 REPLAY CONTROL FAILED: $l"

# C2 + C3
g log --first-parent --reverse --merges --format='%s %P' "$CTL_FROM..$CTL_TO" | awk '{n=$2; sub(/^claude\//,"",n); print n, $NF}' > "$OUT/c2-rows.txt"
ASM=$(g log --first-parent --merges -1 --format=%H "$CTL_FROM..$CTL_TO")
pm env BASE=$CTL_FROM ROWS=$OUT/c2-rows.txt OUT=$OUT/tmp MODE=chain ONCONFLICT=skip > "$OUT/c2-chain.log" 2>&1; cp "$OUT/tmp/chain.tsv" "$OUT/c2-chain.tsv"
H=$(tail -n 1 "$OUT/c2-chain.tsv" | cut -f4); T=$(g rev-parse "$H^{tree}"); RT=$(g rev-parse "$ASM^{tree}")
[ "$T" = "$RT" ] && say "C2 CHAIN ok: $(grep -c . "$OUT/c2-rows.txt") rows replayed from $CTL_FROM, head tree ${T:0:12} = the real assembled head ${ASM:0:10}'s ($(grep -c 'CRISS-CROSS' "$OUT/c2-chain.log") criss-cross step(s), $(grep -c ': CONFLICT' "$OUT/c2-chain.log") conflicted)" || say "C2 CHAIN CONTROL FAILED: head tree ${T:0:12}, real ${RT:0:12}"
py tQ-regcheck.py "$CTL_FROM" "$H" "$OUTW/c2-chain.tsv" > "$OUT/c3-regcheck-pass.log" 2>&1; r=$?
[ $r = 0 ] && say "C3 REGCHECK pass arm ok: $(grep -c '^ok ' "$OUT/c3-regcheck-pass.log") of 6 files ok ($(grep -o 'MULTI-BASE([^)]*)' "$OUT/c3-regcheck-pass.log" | sort -u | grep -c .) multi-base class, $(grep -c 'identical-insert credit' "$OUT/c3-regcheck-pass.log") file(s) with an identical-insert credit)" || say "C3 REGCHECK pass arm CONTROL FAILED rc=$r"
py tQ-regcheck.py "$CTL_FROM" "$CTL_FROM" "$OUTW/c2-chain.tsv" > "$OUT/c3-regcheck-fail.log" 2>&1; r=$?
[ $r = 1 ] && [ "$(grep -c '^FAIL ' "$OUT/c3-regcheck-fail.log")" = 6 ] && say "C3 REGCHECK fail arm ok: FAIL x6 with head = the bare base" || say "C3 REGCHECK fail arm CONTROL FAILED rc=$r"

# C4 (the rows files are this folder's inputs: name sha expected-path)
c4() { # c4 <base> <ref> <path expected to conflict>
  printf 'ctl %s\n' "$2" > "$OUT/c4-row.txt"
  pm env BASE=$1 ROWS=$OUT/c4-row.txt OUT=$OUT/tmp MODE=alone > "$OUT/c4-one.log" 2>&1; cat "$OUT/c4-one.log" >> "$OUT/c4-conflicts.log"
  grep -q "CONFLICT .*$3" "$OUT/c4-one.log" && say "C4 CONFLICT ok: $2 onto $1 reads CONFLICT in $3" || say "C4 CONFLICT CONTROL FAILED: $2 onto $1 did not read CONFLICT in $3"
}
: > "$OUT/c4-conflicts.log"
c4 59ee0d21bf a7a7a197dd src/go2cs/visitAssignStmt.go
c4 59ee0d21bf 0c4397a069 docs/phase4/BOARD-next-validation-candidates.md
c4 59ee0d21bf d49be54e11 src/core/runtime/managed_impl.cs
c4 eb88ab9492 338ef4a2af docs/README.md
c4 eb88ab9492 d2e09a681c src/go2cs/conversionDriver.go
c4 6f54f28be2 c5cb51d3b6 src/go2cs/importAliasOperations.go
rm -f "$OUT/c4-row.txt" "$OUT/c4-one.log"

# C5
B=$(g rev-parse "$P_BASE^{commit}")
IDX="$OUTW/plant.idx"; rm -f "$OUT/plant.idx"
GIT_INDEX_FILE="$IDX" g read-tree "$B"
blob=$(g rev-parse "$B:src/go2cs/readme.go")
GIT_INDEX_FILE="$IDX" g update-index --add --cacheinfo "100644,$blob,src/go2cs/plantedTwin.go"
printf 'package main\n\nfunc plantedSetUser() { _ = NewHashSet[string]() }\n' > "$OUT/plant.go"; hb=$(g hash-object -w --no-filters "$OUTW/plant.go")
GIT_INDEX_FILE="$IDX" g update-index --add --cacheinfo "100644,$hb,src/go2cs/plantedHashSet.go"
# REVIEW ROUND 1 (trainP/tP-CHANGES.md RR1-2): P13's class, planted. One backtick held in an INTERPRETED string, and
# below it a second declaration of plantedSetUser. O's backtick parity skipped every line below that backtick, so this
# duplicate was invisible to the census; the lexical scan must name it (the file is the third with no projitems entry).
printf 'package main\n\nvar plantedTick = "a ` in an interpreted string"\n\nfunc plantedSetUser() {}\n' > "$OUT/plant2.go"; tb=$(g hash-object -w --no-filters "$OUTW/plant2.go")
GIT_INDEX_FILE="$IDX" g update-index --add --cacheinfo "100644,$tb,src/go2cs/plantedTick.go"
gb=$(printf 'package main\n\nfunc main() {}\n' | g hash-object -w --stdin)
GIT_INDEX_FILE="$IDX" g update-index --add --cacheinfo "100644,$gb,src/tests/Behavioral/PlantedGoOnly/main.go"
printf '<<<<<<< ours\na\n=======\nb\n>>>>>>> theirs\n' > "$OUT/plant.md"; mb=$(g hash-object -w --no-filters "$OUTW/plant.md")
GIT_INDEX_FILE="$IDX" g update-index --add --cacheinfo "100644,$mb,docs/planted-markers.md"
PT=$(GIT_INDEX_FILE="$IDX" g write-tree); rm -f "$OUT/plant.idx" "$OUT/plant.go" "$OUT/plant2.go" "$OUT/plant.md"
PL=$(echo "premap control: planted twin, HashSet use, a duplicate below a backtick, Go-only directory, markers" | GIT_AUTHOR_NAME=premap GIT_AUTHOR_EMAIL=premap@invalid GIT_COMMITTER_NAME=premap GIT_COMMITTER_EMAIL=premap@invalid GIT_AUTHOR_DATE='1700000000 +0000' GIT_COMMITTER_DATE='1700000000 +0000' g commit-tree --no-gpg-sign "$PT" -p "$B")
py tQ-unioncheck.py "$B" "$PL" > "$OUT/c5-unioncheck-planted.log" 2>&1
py tQ-unioncheck.py "$B" "$B" > "$OUT/c5-unioncheck-negative.log" 2>&1
ck() { grep -qE "$2" "$OUT/$1" && say "C5 UNIONCHECK ok: $3" || say "C5 UNIONCHECK CONTROL FAILED: $3"; }
ck c5-unioncheck-planted.log "^A PROJITEMS head: .*files-without-exactly-one-entry=3 " "the three planted Go files have no projitems entry (3)"
ck c5-unioncheck-planted.log "^C GODECL .*NEW at head=[1-9]" "the planted twin's declarations read as NEW duplicates ($(grep -c '^  DUPLICATE' "$OUT/c5-unioncheck-planted.log"))"
ck c5-unioncheck-planted.log "^  DUPLICATE func plantedSetUser: .*plantedTick\.go" "the duplicate declared below a backtick held in an interpreted string is NAMED (P13's class: the parity filter skipped it)"
ck c5-unioncheck-planted.log "^C GODECL scan control: ok" "the declaration scan's own control reads exactly its two planted declarations"
ck c5-unioncheck-planted.log "^B H4 .*: 1 \['PlantedGoOnly'\]" "the planted Go-only behavioral directory reads H4"
ck c5-unioncheck-planted.log "^D HASHSET .*: 1 in 1 file" "the planted unqualified NewHashSet reads 1"
ck c5-unioncheck-planted.log "^E MARKERS .*with a conflict marker=1 " "the planted marker file reads 1"
ck c5-unioncheck-negative.log "^A PROJITEMS head: .*files-without-exactly-one-entry=0 .*entries-without-a-file=0" "negative arm: projitems 0 / 0 at the base"
ck c5-unioncheck-negative.log "^C GODECL .*NEW at head=0" "negative arm: no new duplicate declaration at the base"
ck c5-unioncheck-negative.log "^D HASHSET .*: 0 in 0 file" "negative arm: HashSet 0"
ck c5-unioncheck-negative.log "^E MARKERS .*with a conflict marker=0 " "negative arm: markers 0"

# C6: sibling-package-name x go-namespace-shadow, importAliasOperations.go (the pair that taught the unrefined carry)
A=6f54f28be2; Bx=c5cb51d3b6; F=src/go2cs/importAliasOperations.go
if g cat-file -e "$A^{commit}" 2>/dev/null && g cat-file -e "$Bx^{commit}" 2>/dev/null; then
  mbase=$(g merge-base "$A" "$Bx"); S="$OUT/tmp-c6"; mkdir -p "$S"
  for r in $mbase $A $Bx; do g cat-file blob "$r:$F" > "$S/$r.go"; done
  SW=$(cygpath -m "$S")
  for mode in "--union" "--union --diff3"; do
    "$GITX" merge-file -p $mode --diff-algorithm histogram "$SW/$A.go" "$SW/$mbase.go" "$SW/$Bx.go" > "$S/importAliasOperations.go" 2>/dev/null
    e=$("$GOFMT" -e -l "$SW/importAliasOperations.go" 2>&1 >/dev/null | grep -c .); n=$(grep -c '' "$S/importAliasOperations.go")
    echo "C6 union mode [$mode]: lines=$n gofmt-errors=$e (base $(grep -c '' "$S/$mbase.go"), sides $(grep -c '' "$S/$A.go") and $(grep -c '' "$S/$Bx.go"))" >> "$OUT/c6-gofmt.log"
    if [ "$mode" = "--union" ]; then [ "$e" -gt 0 ] && say "C6 GOFMT ok: the REFINED union does not parse ($e errors, $n lines)" || say "C6 GOFMT CONTROL FAILED: the refined union parsed"
    else [ "$e" = 0 ] && say "C6 GOFMT ok: the unrefined union parses ($n lines)" || say "C6 GOFMT CONTROL FAILED: the unrefined union reads $e errors"; fi
  done
  rm -rf "$S"
  mt=$("$GITX" --git-dir="$GD" merge-tree --write-tree --name-only "$A" "$Bx" | head -1)
  GOFMT=$GOFMT bash "$HERE/tQ-gofmt-parse.sh" "$mbase" "$mt" "$OUT/tmp-gofmt" >> "$OUT/c6-gofmt.log" 2>&1
  grep -qE '^GOFMT-PARSE .*parse-errors=[1-9]' "$OUT/c6-gofmt.log" && say "C6 GOFMT ok: ort's MARKED tree ${mt:0:12} does not parse" || say "C6 GOFMT CONTROL FAILED: the marked tree parsed"
else
  say "C6 GOFMT not run: $A or $Bx is not in the store"
fi
rm -rf "$OUT/tmp" "$OUT/tmp-gofmt"
nloose=$(find "$OUT/objects" -mindepth 2 -type f -not -path '*/pack/*' | wc -l)
if [ "$nloose" -gt 0 ]; then
  find "$OUT/objects" -mindepth 2 -type f -not -path '*/pack/*' | sed -E 's|^.*/objects/(..)/([0-9a-f]{38})$|\1\2|' > "$OUT/objlist.txt"; mkdir -p "$OUT/objects/pack"
  pk=$(g pack-objects -q "$GIT_OBJECT_DIRECTORY/pack/pack" < "$OUT/objlist.txt") && g verify-pack "$GIT_OBJECT_DIRECTORY/pack/pack-$pk.idx" && for d in "$OUT"/objects/??; do rm -rf "$d"; done
  rm -f "$OUT/objlist.txt"
fi
echo "PREMAP-CONTROLS DONE failed=$fail $(date '+%H:%M:%S')"
[ "$fail" = 0 ]
