#!/usr/bin/env bash
# TRAIN Q map: merge each seat AT ITS SEATED SHA (column 2), in table order, onto TRAIN P's LANDED master line ($BASE, a
# REQUIRED variable) in a scratch worktree. A conflict goes to tFL-resolve.py with tFL-ruled.txt (trainQ/tQ-CHANGES.md
# Q20): a RULED one ('keep both in merge order', in the rows and files that file names) is resolved exactly as the
# assembly will resolve it and the row is merged; ANY OTHER conflict is recorded with the row and every hunk named, that
# seat's merge is ABORTED (later seats merge without it) and the map reads NOT-CLEAN. Also reports any seat whose branch
# tip has MOVED past its seated SHA, a SLOT row and a CAND row (neither is merged). Output = the full map in one pass.
# Q's map replaces a pre-map: the i9 REHEARSED the union on Windows (trainQ/tQ-map.md). The rehearsal merged six seats
# in other positions than the table's, so its trees are not this map's trees: THIS run is the reading of the ORDER.
# DERIVED 2026-10-06 from trainP/tP-conflict-map.sh (P's map: 24/24 clean, head b58466a460, rows-sha256 49c3ab180a98:
# names, the texts that name the base, the ruled-resolution arm, the slot and candidate refusals: R0, Q1, Q20), itself
# DERIVED 2026-10-04 from trainO/tO-conflict-map.sh (O's map: 38/38 clean at 03:15, head 127bb71d72, rows-sha256
# 716a21129c57), itself DERIVED 2026-10-03
# from trainN/tN-conflict-map.sh (N's map: 29/29 clean at 06:20, head 1e40cd7a19, rows-sha256
# 584ae8af08b9), itself from trainM/tM-conflict-map.sh (TRAIN M's run 1 there: 15 seats clean, map union 8d7305053f; its
# frozen list mapped 24 of 24 clean at 11c188daf3). O changes the names, makes the base REQUIRED and refuses a row that
# carries another train's assembly commits the base lacks (trainO/tO-CHANGES.md O1, O2). The worktree-free pre-map that
# reads seat-vs-seat pairs and a cumulative chain without merging anything is trainQ/tQ-premap.sh (P's is git's own
# merge-ort through a git >= 2.38, one command: trainQ/tQ-premap-all.sh). RE-RUN
# it after any change to the seat list (a late seat, a moved sha, a re-ordered row): a map is a reading of ONE list.
# VERIFY ROUND 2 (trainM/tM-CHANGES.md section 10): the list is overridable (SEATS=...; the default is still COORD's live hnd
# file); a map that read EVERY row clean stamps the rows it read (rows-sha256 = sha256 over the 'ref|sha' columns of the
# seat rows, 12 hex: comment lines do not move it) and keeps its union under a second LOCAL ref named by that hash,
# refs/coord/tFL-map/rows-<hash>. tFL-assemble.sh computes the same hash over the list IT merges and finds its map union
# through that ref, so no script carries a map union sha (M's list grew from 15 to 20 to 24 rows in four hours).
# VERIFY ROUND 3 (trainM/tM-CHANGES.md section 11): the list is read ONCE, into a snapshot under coord-scratch/tFL; the merges
# and the hash both read the snapshot. (TRAIN M's frozen list: MAP DONE head=11c188daf3, coord-scratch/tM/asm1/map-frozen.log.)
# FL (FL8): MAP_FROM. TRAIN FL's first ten rows were merged BY HAND on claude/coord-trainFL-union (signed, pushed,
# 14c543bb06), three of them with hand resolutions (rows 3 and 4: the BOARD, kept both with ONE separator blank line
# dropped; row 8: go2cs-src.projitems, kept both in alphabetical order). No ruled kind reproduces the BOARD pair byte
# for byte (tFL-controls.sh arm RSFL reads that from the objects), so a map from BASE cannot equal the hand union's
# tree. MAP_FROM=<the union head, full or short sha> starts the map AT that head instead: its first-parent merges must
# be exactly the list's FIRST k rows, in order (second parents = the seated shas), with nothing single-parent among them;
# those rows print 'merged at MAP_FROM' and are not merged again; the rows after them merge (and resolve by rule) on
# top. tFL-assemble.sh resumes on the same union branch, merges the same rows through the same resolver, and its tree
# is compared with this map's head as before. Unset (the default) = the map from BASE, Q's behaviour.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
WT=/h/go2cs-tmp-coord/tFL-map
RULEDF=${RULED:-$SD/tFL-ruled.txt}; RESOLVER=$SD/tFL-resolve.py
[ -f "$RULEDF" ] && [ -f "$RESOLVER" ] || { echo "ABORT: $RULEDF or $RESOLVER is missing beside this script (tFL-ruled.txt, tFL-resolve.py)"; exit 2; }
RULSHA=$(grep -vE '^[[:space:]]*(#|$)' "$RULEDF" | tr -d '\r' | sha256sum | cut -c1-12)
SEATS=${SEATS:-/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainFL/tFL-seats-draft.txt}
BASE_IN=${BASE:?set BASE to the landed master line of TRAIN Q (and the docs commits COORD landed after it), read from the remote (COORD-LAUNCH-CHECKLIST.md step 0)}
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS)"; exit 2; }
# Verify round 3 (RULED): the list is read ONCE. The default list is COORD's LIVE hnd file, and the map read it twice:
# the rows it merged, and again at the end for the hash it stamped and named the rows ref by. A row re-pointed between
# the two reads (it happened at 07:43 on 2026-10-02, at M) wrote refs/coord/tM-map/rows-<hash of the NEW list> onto the union
# of the OLD rows. The rows are snapshotted to the scratch folder here; the merges read the snapshot and the hash is the
# snapshot's (the same bytes tFL-assemble.sh hashes: the 'ref|sha' columns of the row lines, CR-stripped).
X=/h/go2cs-tmp-coord/coord-scratch/tFL
mkdir -p "$X" || exit 1
ROWS=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
[ -n "$ROWS" ] || { echo "ABORT: no seat rows in $SEATS"; exit 2; }
RSHA=$(printf '%s\n' "$ROWS" | cut -d'|' -f1,2 | sha256sum | cut -c1-12)
SNAP="$X/map-rows-$RSHA.txt"
printf '%s\n' "$ROWS" > "$SNAP" || { echo "ABORT: cannot write the row snapshot $SNAP"; exit 2; }
[ "$(cut -d'|' -f1,2 "$SNAP" | sha256sum | cut -c1-12)" = "$RSHA" ] || { echo "ABORT: the snapshot $SNAP does not hash to $RSHA"; exit 2; }
echo "MAP list read ONCE: $(grep -c . "$SNAP") rows of $SEATS, rows-sha256=$RSHA, snapshot $SNAP (the merges below read the snapshot)"
# REVIEW ROUND 1 (tFL-assemble.sh's table check, the same two lines): an open slot written as a '#   SLOT' comment was
# counted and the map went on, so a rows ref could be written for a list COORD still meant to add a row to. Refused
# before anything is created unless SLOTS_OK=<their count> acknowledges exactly that many (COORD's explicit call).
NSLOT=$(grep -cE '^#[[:space:]]*SLOT' "$SEATS"); NSLOT=${NSLOT:-0}
slotsok(){ [ "$1" = 0 ] || [ "${2:-}" = "$1" ]; }
if slotsok "$NSLOT" "${SLOTS_OK:-}"; then
  [ "$NSLOT" = 0 ] || echo "MAP NOTE SLOTS_OK=$SLOTS_OK (COORD's explicit call): the list holds $NSLOT '#   SLOT' comment(s), no rows, nothing is merged for them"
else
  echo "ABORT: the list still holds $NSLOT '#   SLOT' comment(s) (an open slot with no ref) and SLOTS_OK is '${SLOTS_OK:-unset}': seat each slot as a row or delete its comment, or acknowledge them all with SLOTS_OK=$NSLOT (COORD's explicit call). Nothing was created or merged"; exit 2
fi
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
BASE=$(git rev-parse --verify -q "$BASE_IN^{commit}") || { echo "ABORT: BASE=$BASE_IN does not resolve to a commit"; exit 2; }
git merge-base --is-ancestor "$BASE" origin/master || { echo "ABORT: BASE ${BASE:0:10} is not on origin/master (TRAIN FL's base is TRAIN Q's LANDED master line)"; exit 2; }
# Review round 1 (F2-1, tFL-assemble.sh's check, the same predicate): a STALE base on origin/master (at Q: P's union
# 446d2c8ba0 / d843ff263d, or any point below P's refresh) is refused; BASE_BEHIND_OK=1 by COORD's explicit call. A base
# above P's landing and below the tip is the ASSEMBLY's tip check (Q1b): the map reads any base COORD gives it.
BEHIND=$(git log --first-parent --format='%h %s' "$BASE..origin/master" | grep -E '^[0-9a-f]+ (Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )')
if [ -n "$BEHIND" ]; then
  if [ "${BASE_BEHIND_OK:-}" = 1 ]; then
    echo "NOTE: BASE_BEHIND_OK=1 (COORD's explicit call): $(printf '%s\n' "$BEHIND" | grep -c .) train-assembly commit(s) above the base on origin/master's first-parent line"
  else
    echo "ABORT: BASE ${BASE:0:10} is BEHIND a landed train: origin/master's first-parent line above it holds $(printf '%s\n' "$BEHIND" | grep -c .) train-assembly commit(s) ($(printf '%s\n' "$BEHIND" | head -n 3 | cut -c1-90 | tr '\n' ';')): read BASE from ls-remote (COORD-LAUNCH-CHECKLIST.md step 0)"
    exit 2
  fi
fi
echo "MAP base ${BASE:0:10} (BASE=$BASE_IN; on origin/master; no train-assembly commit above it)"
# Verify round 1: a map union is cited by sha in the scripts and the notes (at M, 8d7305053f: tM-assemble.sh compared
# its tree with it; at N, O and P the assembler compares with the rows ref below), and the detached map worktree is all that holds it. Before the worktree is removed its head is kept under
# a LOCAL ref (refs/coord/..., never pushed), so a cited map union stays in the object store across re-runs.
if [ -d "$WT" ]; then
  prev=$(git -C "$WT" rev-parse HEAD 2>/dev/null) && git update-ref "refs/coord/tFL-map/${prev:0:10}" "$prev" && echo "PREVIOUS map union ${prev:0:10} kept as refs/coord/tFL-map/${prev:0:10} (local)"
  git worktree remove --force "$WT" || exit 1
fi
MAPFROMP=''
if [ -n "${MAP_FROM:-}" ]; then
  MF=$(git rev-parse --verify -q "${MAP_FROM}^{commit}") || { echo "ABORT: MAP_FROM=$MAP_FROM does not resolve to a commit (fetch the union branch first)"; exit 2; }
  git merge-base --is-ancestor "$BASE" "$MF" || { echo "ABORT: MAP_FROM ${MF:0:10} does not descend from the base ${BASE:0:10}"; exit 2; }
  for c in $(git rev-list --first-parent "$BASE..$MF"); do
    [ "$(git rev-list --parents -n 1 "$c" | wc -w)" = 3 ] || { echo "ABORT: MAP_FROM's first-parent line holds a single-parent commit ${c:0:10} ($(git log -1 --format=%s "$c" | cut -c1-60)): the map starts BELOW any fixup"; exit 2; }
  done
  MAPFROMP=$(git rev-list --first-parent --reverse "$BASE..$MF" | while read -r c; do git rev-parse "$c^2"; done)
  kf=$(printf '%s\n' "$MAPFROMP" | grep -c .)
  WANTP=$(head -n "$kf" "$SNAP" | while IFS='|' read -r r s x; do git rev-parse --verify -q "$s^{commit}" || echo "UNRESOLVED:$r"; done)
  [ "$MAPFROMP" = "$WANTP" ] || { echo "ABORT: MAP_FROM ${MF:0:10}'s $kf first-parent merges are not the list's first $kf rows in order (second parents vs seated shas): the map cannot start there"; exit 2; }
  echo "MAP FROM ${MF:0:10} (MAP_FROM, COORD's call): its $kf first-parent merges are the list's rows 1-$kf in order; those rows are NOT merged again"
  git worktree add -q --detach "$WT" "$MF" || exit 1
else
  git worktree add -q --detach "$WT" "$BASE" || exit 1
fi
cd "$WT" || exit 1
n=0; nbad=0; nruled=0
while IFS='|' read -r ref sha rest; do
    n=$((n + 1))
    # Q (Q20, tFL-assemble.sh's table check, the same predicates): a SLOT row and a CAND row are not merged.
    printf '%s' "$sha" | grep -qE '^[0-9a-f]{7,40}$' || { echo "SEAT $n $ref: a SLOT row (its sha field is '${sha:-empty}', not a sha): NOT merged; the assembly refuses the list until the table carries a sha for it"; nbad=$((nbad + 1)); continue; }
    # REVIEW ROUND 1: the release is the FORM 'READING IN (' (the assembler's candrow, the same two greps): the bare
    # words in unrelated prose released a row.
    if printf '%s' "$rest" | grep -qE '(^|[^A-Za-z0-9_-])CAND([^A-Za-z0-9_-]|$)' && ! printf '%s' "$rest" | grep -qF 'READING IN ('; then
      echo "SEAT $n $ref: a CAND row whose reading is not marked as in (CAND and no 'READING IN (<when>, <where>)' in its notes): NOT merged; the assembly refuses the list (strike the row and every row that rides only with it, or mark the reading)"; nbad=$((nbad + 1)); continue
    fi
    full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "SEAT $n $ref: seated sha $sha MISSING"; nbad=$((nbad + 1)); continue; }
    tip=$(git rev-parse --verify -q "origin/claude/$ref" || echo none)
    [ "$tip" != "$full" ] && echo "SEAT $n $ref: NOTE branch tip ${tip:0:10} != seated ${full:0:10}"
    if [ -n "$MAPFROMP" ] && printf '%s\n' "$MAPFROMP" | grep -qxF "$full"; then echo "SEAT $n $ref: merged at MAP_FROM (the hand union's own merge; not merged again)"; continue; fi
    if git merge-base --is-ancestor "$full" HEAD; then echo "SEAT $n $ref: already contained"; nbad=$((nbad + 1)); continue; fi
    # O2 (tFL-assemble.sh's table check, the same predicate): a row cut on a train union the base does not contain.
    # FL (FL9, tFL-assemble.sh's table check): a row cut on THIS train's union carries its seat merges; they are not
    # counted here (the assembler's table check reads each one's second parent against the earlier rows).
    tx=$(git log --format=%s "$BASE..$full" | grep -E '^(Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )' | grep -vcE '^Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN FL ')
    [ "$tx" = 0 ] || { echo "SEAT $n $ref: REFUSED: carries $tx train-assembly commit(s) the base lacks (is BASE TRAIN Q's landed master line?)"; nbad=$((nbad + 1)); continue; }
    # Verify round 1 (merge-hazards: "a merge probe's green is readable only if the two sides are independent AND
    # overlapping ... print the shared-path count beside every row"): the paths this seat and the union so far BOTH
    # changed since their merge-base. A clean row with shared paths is where a silent duplicate or a dropped line can
    # sit; precheck's BOTH arm reads each of them in the merge's own tree, and every shared path that is not a
    # registration file is READ by a person before the fixup (tFL-README.md MS1).
    mbx=$(git merge-base HEAD "$full")
    shared=$(LC_ALL=C comm -12 <(git diff --name-only "$mbx" "$full" | LC_ALL=C sort) <(git diff --name-only "$mbx" HEAD | LC_ALL=C sort))
    ns=$(printf '%s\n' "$shared" | grep -c .)
    if git -c user.name=map -c user.email=map@invalid merge -q --no-ff --no-edit -m "map: merge $ref" "$full" >/dev/null 2>&1; then
        echo "SEAT $n $ref: clean (shared paths with the union so far: $ns: $(echo $shared | cut -c1-300))"
    else
        files=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
        if ! git rev-parse -q --verify MERGE_HEAD > /dev/null; then
            echo "SEAT $n $ref: git REFUSED the merge before it ran (not a conflict; read the worktree's state)"; nbad=$((nbad + 1)); continue
        fi
        # Q (Q20): the ONLY resolutions are tFL-ruled.txt's, made by the resolver the assembly runs (two phases: nothing
        # is written unless every unmerged path of this merge is ruled for this row at this sha and passes its asserts).
        python -B "$(cygpath -w "$RESOLVER")" "$(cygpath -w "$WT")" "$(cygpath -w "$RULEDF")" "$ref" "$full" > "$X/map-resolve-seat$n.log" 2>&1; rrc=$?
        if [ "$rrc" = 0 ] && [ -z "$(git diff --name-only --diff-filter=U)" ] \
           && git -c user.name=map -c user.email=map@invalid -c commit.gpgsign=false commit -q --no-edit -m "map: merge $ref (resolved by rule: ${files% })" > /dev/null 2>&1; then
            nruled=$((nruled + 1))
            echo "SEAT $n $ref: RULED resolution in: ${files% } (shared paths with the union so far: $ns)"
            tr -d '\r' < "$X/map-resolve-seat$n.log" | grep '^RESOLVED' | sed 's/^/    /'
        else
            echo "SEAT $n $ref: CONFLICT (UNRULED) in: $files (tFL-resolve.py rc=$rrc)"; nbad=$((nbad + 1))
            tr -d '\r' < "$X/map-resolve-seat$n.log" | grep -E '^(UNRULED|RESOLVE-)' | sed 's/^/    /'
            git merge --abort
        fi
    fi
done < "$SNAP"
git update-ref "refs/coord/tFL-map/$(git rev-parse --short=10 HEAD)" HEAD   # local, never pushed: the map union stays reachable
# (verify round 3: RSHA is the SNAPSHOT's hash, computed before the first merge; the list file is not read again)
# Q (Q20): the rulings the map USED, read from its own merges; a ruled line no merge used is printed (the row merged
# clean in this order, or names no row). The ruled file's hash is stamped beside the rows snapshot: the assembly
# compares its own copy with it before it creates anything.
runused=''
while IFS='|' read -r rr rs rp rk rrest; do
  git log --format=%s "$BASE..HEAD" | grep -F "map: merge $rr (resolved by rule:" | grep -qF -- "$rp" || runused="$runused $rr:${rp##*/}"
done < <(grep -vE '^[[:space:]]*(#|$)' "$RULEDF" | tr -d '\r')
echo "RULED: $(grep -cvE '^[[:space:]]*(#|$)' "$RULEDF") line(s) in $(basename "$RULEDF") (ruled-sha256=$RULSHA), $nruled merge(s) resolved by rule; UNUSED:${runused:- none}; SLOT comments in the list (not rows): $NSLOT$([ "$NSLOT" = 0 ] || echo " (acknowledged: SLOTS_OK=${SLOTS_OK:-})")"
# The ruled files at the map head (trainQ/tQ-map.md A and B give the 64-row expectations: 26507 / 959 / 816 816 816 787).
BOARDP=docs/phase4/BOARD-next-validation-candidates.md
lr=''; for lf in TranspileTests CompileTests TargetComparisonTests OutputComparisonTests; do
  lr="$lr $lf=$(git show "HEAD:src/tests/Behavioral/BehavioralTests/$lf.cs" | tr -d '\r' | grep -c '^[[:space:]]*\[TestMethod\]$')/$(git show "HEAD:src/tests/Behavioral/BehavioralTests/$lf.cs" | tr -d '\r' | grep -cE '^[[:space:]]*public void Check[A-Za-z0-9_]*\(')"
done
echo "RULED-FILES at $(git rev-parse --short=10 HEAD): BOARD lines=$(git show "HEAD:$BOARDP" | wc -l) (base $(git show "$BASE:$BOARDP" | wc -l)) guard-final=$(git show "HEAD:$BOARDP" | tail -n 1 | grep -c '^<!-- {% endraw %}'); go2cs.slnx Project lines=$(git show HEAD:src/go2cs.slnx | grep -c '<Project Path=') (base $(git show "$BASE:src/go2cs.slnx" | grep -c '<Project Path=')); lists [TestMethod]/Check:$lr"
if [ "$nbad" = 0 ] && [ "$n" -ge 1 ]; then
  git update-ref "refs/coord/tFL-map/rows-$RSHA" HEAD   # local: the union of THIS list, found by tFL-assemble.sh through the list's own hash
  echo "$RULSHA rows-sha256=$RSHA head=$(git rev-parse --short=10 HEAD) ruled-merges=$nruled" > "$X/map-ruled-$RSHA.txt"
  echo "MAP DONE head=$(git rev-parse --short=10 HEAD) rows=$n rows-sha256=$RSHA ruled-sha256=$RULSHA (every row clean or resolved by rule ($nruled): kept as refs/coord/tFL-map/rows-$RSHA, the ref tFL-assemble.sh reads; the ruled hash is stamped in $X/map-ruled-$RSHA.txt)"
else
  echo "MAP DONE head=$(git rev-parse --short=10 HEAD) rows=$n rows-sha256=$RSHA NOT-CLEAN rows=$nbad (missing, already contained, a slot, an unread candidate or an UNRULED conflict: NO rows ref written; this head is not the union of the list)"
fi
