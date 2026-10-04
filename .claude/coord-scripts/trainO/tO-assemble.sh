#!/usr/bin/env bash
# TRAIN O assembly (COORD): SIGNED merges of every seat AT ITS SEATED SHA, in draft order, onto TRAIN N's landed master
# (BASE: a REQUIRED variable, never a literal here: N lands tomorrow as its union 59ee0d21bf + fixup-2 + the bank step +
# the MS13 refresh, so the sha does not exist at the derive), on the new local branch claude/coord-trainO-union in
# /h/go2cs-tmp-coord/tO. The pre-map MUST read every seat CLEAN on that base, in this order (the map is RE-RUN on every
# change of the list), and there is NO ruled resolution: ANY conflict ABORTS the run (M's and N's doctrine; a narrow
# ruled pre-resolution path stayed an open question at N, trainN/tN-README.md section 7 Q14, and N took re-cuts).
# THE O PRE-MAP (trainO/tO-premap.md, 2026-10-03 19:50, read on N's union 59ee0d21bf as a stand-in base) READS FIVE
# CONFLICTING ROWS IN THE PROPOSED ORDER, each an owed re-cut (tO-README.md MS21): r-typeswitch-collapsed-func-case
# (BOARD + go2cs-src.projitems), c2-nuget-followups (docs/README.md, two hunks), g-method-value-names and
# g-method-value-fm-record (src/core/runtime/managed_impl.cs vs N; encoding/json/package_info.cs vs p2), and
# p1-hashset-module (packageStateOperations.go, visitAssignStmt.go). Plus g-float-untyped-const-compare's BOARD tip
# (route: a new ref at its BOARD-free parent b967f9fa7f). Until those land, the map will not read the list clean and this
# script refuses it. Plus one CLEAN merge that does not compile, which no map can see: c1-darwin-xsys-libc adds two
# unqualified HashSet[ uses that p1-hashset-module deletes the type of (MS19's scan; the hashset re-cut converts them).
# Launch from a per-run copy (floor 4). Resumable onto its own clean union branch. The seat count is READ from the seat
# list, never written here: a seat added or dropped changes the list and nothing else (re-run tO-conflict-map.sh on the
# new list first).
# DERIVED 2026-10-03 from trainN/tN-assemble.sh (itself from trainM/tM-assemble.sh): the names, the base (now REQUIRED,
# with two new table checks below) and the literal-site list change (trainO/tO-CHANGES.md O1, O2); the table check, the
# map refusal and the shape asserts are M's and N's, unchanged. N's REVIEW ROUND 1 (trainN/tN-CHANGES.md section 4): an
# 'after <ref>' order note is checked against the row order, and the patch-id loop skips a row the table already refused
# as landed.
# CHANGED FROM TRAIN L (at TRAIN M): a seat-table check before anything is created (duplicate ref or sha, a 'stack-on' note that
# names no earlier row or no ancestor, a seat that is an ancestor of an earlier row); 'already contained' is accepted
# only on a RESUME (the seat's own merge is on the first-parent line) and is otherwise refused, because a seat skipped
# that way leaves the union one first-parent merge short and the fixup and the battery then refuse the shape; the
# first-parent count is asserted against the seat count at the end.
# VERIFY ROUND 1 (trainM/tM-CHANGES.md section 9): the seat list is the RUN FOLDER's copy, read ONCE into a snapshot (the hnd
# file is COORD's live notes: a row re-pointed mid-run was merged without having passed the table check); the launch
# refusal and the signing preflight of tO-fixup.sh; table problems added: a branch tip that moved past its seated sha
# (unless TIP_MOVED_OK names the ref), a row that descends from an earlier row with no stack-on note covering it, one
# patch under two shas; a failed merge is reported as what it is (refused, clean but uncommitted, or a conflict); the
# shape assert counts a ruled follow-up merge on a resume; the assembled tree is compared with the pre-map union's.
# VERIFY ROUND 2 (trainM/tM-CHANGES.md section 10): the map union is no longer a sha written here: it is found through the
# local ref tO-conflict-map.sh keeps for the rows it read (refs/coord/tO-map/rows-<rows-sha256>; MAP_UNION=<sha> still
# overrides); each seat's tip is read with `git ls-remote` at assembly time (a withdrawn branch keeps its local
# tracking ref: no prune); table problems added: a row already on the base (a landed seat), a seat patch master
# already carries under another sha; TABLE_ONLY=1 stops after the table check, before anything is created (the
# check's controls: tO-README.md MS9c); on a resume the seats' merges must sit on the first-parent line in ROW order;
# the MAP TREE line is also written to coord-scratch/tO/assemble-maptree.txt, which tO-fixup.sh stamps in its PRE.
# VERIFY ROUND 3 (trainM/tM-CHANGES.md section 11; RULED): the assembly REFUSES to create or merge anything unless a map has
# read THIS list: the rows ref refs/coord/tO-map/rows-<rows-sha256> exists, or MAP_UNION=<sha> is given explicitly and
# resolves. (Round 2 printed a NOTE and merged a list no map had read; the tree then read 'NOT COMPARED'.) An assembled
# tree that is not the map union's tree is exit 4 at the end, after the shape lines are printed.
set -u
WT=/h/go2cs-tmp-coord/tO
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tO-seats-draft.txt}
X=/h/go2cs-tmp-coord/coord-scratch
MAP_UNION=${MAP_UNION:-}                # normally EMPTY: derived below from the rows-sha256 of the list this run merges
case "$SD" in
  /h/go2cs-tmp-coord/tO|/h/go2cs-tmp-coord/tO/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS): copy trainO/tO-seats-draft.txt into the run folder"; exit 2; }
mkdir -p "$X/tO"
# O (tO-CHANGES.md O1): BASE is TRAIN N's LANDED master, a REQUIRED variable (N has not landed at the derive; no script
# of this set carries a base sha). It must resolve, and after the fetch it must be ON origin/master (landed), never a
# union head or a lane ref.
BASE_IN=${BASE:?set BASE to the landed master of TRAIN N, the full sha origin/master holds after N lands: tO-README.md step 0}
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
BASE=$(git rev-parse --verify -q "$BASE_IN^{commit}") || { echo "ABORT: BASE=$BASE_IN does not resolve to a commit (fetch, or a typo)"; exit 2; }
git merge-base --is-ancestor "$BASE" origin/master || { echo "ABORT: BASE ${BASE:0:10} is not on origin/master: TRAIN O's base is TRAIN N's LANDED master, never a union head or a lane ref"; exit 2; }
# Verify round 2 (merge-hazards: "an announced branch is verified by ls-remote at the moment it is needed"): the fetch
# above does not prune, so a seat branch DELETED on the remote keeps its tracking ref and would read 'tip == seated
# sha'. The tips are read from the remote itself, once, here. Pruning the shared repository is not the remedy (another
# train's job may still read those refs).
git ls-remote origin 'refs/heads/claude/*' > "$X/tO/assemble-lsremote.txt" || { echo "ABORT: git ls-remote origin failed (the seat tips are verified on the REMOTE, never on a local tracking ref)"; exit 1; }
[ "$(git rev-parse origin/master)" = "$BASE" ] || echo "NOTE: origin/master is $(git rev-parse --short=10 origin/master), not the base ${BASE:0:10} (the landing merges it and re-runs the precheck: tO-README.md MS11)"
# Review round 1 (tO-CHANGES.md section 5, F2-1): the ancestor check above passes a STALE base too: N's union 59ee0d21bf,
# or any point of N's landing line below its fixup-2 / bank step / MS13 refresh, is an ancestor of origin/master, and
# O2 (a) then reads 0 for every row (the 59ee-cut rows carry nothing over 59ee). The map, the assembly, the fixup's and
# the battery's first-parent checks all agree with such a base, and the ~12 h battery would measure a tree without N's
# landing commits. Refused when origin/master's FIRST-PARENT line ABOVE the base holds a train-assembly commit (O2's
# subjects); BASE_BEHIND_OK=1 runs past it by COORD's explicit call, stamped.
BEHIND=$(git log --first-parent --format='%h %s' "$BASE..origin/master" | grep -E '^[0-9a-f]+ (Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )')
if [ -n "$BEHIND" ]; then
  nbh=$(printf '%s\n' "$BEHIND" | grep -c .)
  if [ "${BASE_BEHIND_OK:-}" = 1 ]; then
    echo "NOTE: BASE_BEHIND_OK=1 (COORD's explicit call): origin/master's first-parent line above the base ${BASE:0:10} holds $nbh train-assembly commit(s): $(printf '%s\n' "$BEHIND" | head -n 3 | cut -c1-90 | tr '\n' ';')"
  else
    echo "ABORT: BASE ${BASE:0:10} is BEHIND a landed train: origin/master's first-parent line above it holds $nbh train-assembly commit(s) ($(printf '%s\n' "$BEHIND" | head -n 3 | cut -c1-90 | tr '\n' ';')). TRAIN O's base is TRAIN N's LANDED master, its MS13 refresh included: read BASE from ls-remote (tO-README.md step 0); BASE_BEHIND_OK=1 only by COORD's explicit call"
    exit 2
  fi
fi
# Signing preflight BEFORE anything is created (tO-fixup.sh's block): every merge below is -S, and a locked agent fails a
# CLEAN merge at its commit.
SIGNFMT=$(git config --get gpg.format || true); SIGNKEY=$(git config --get user.signingkey || true)
[ -n "$SIGNKEY" ] || { echo "ABORT: no user.signingkey: merge -S cannot sign"; exit 2; }
case "${SIGNFMT:-openpgp}" in
  openpgp)
    gp=$(git config --get gpg.program || echo gpg); case "$gp" in ?:*) gp=$(cygpath -u "$gp") ;; esac
    "$gp" --batch --list-secret-keys "$SIGNKEY" > /dev/null 2>&1; rc=$?
    [ "$rc" = 0 ] || { echo "ABORT: the signing key is not in the secret keyring (gpg rc=$rc)"; exit 2; }
    if [ "${SIGN_PROBE:-1}" = 1 ]; then  # a real signature through the agent; SIGN_PROBE=0 skips it
      echo "tO-assemble signing probe" | "$gp" --batch --yes --local-user "$SIGNKEY" --clearsign > "$X/tO/assemble-sign-probe.asc" 2> "$X/tO/assemble-sign-probe.err"; rc=$?
      [ "$rc" = 0 ] || { echo "ABORT: the signing probe failed (gpg rc=$rc, $X/tO/assemble-sign-probe.err): unlock the agent, then rerun"; exit 2; }
    fi ;;
  *) echo "PRE signing format '$SIGNFMT': key configured, no agent probe for this format" ;;
esac
# SEAT TABLE CHECK (train-assembly skill: "the row order is the merge order, so a stacked seat carries a higher row
# number than its base"). Rows are ref|sha|notes. Refused before anything is created: a sha that does not resolve, a ref
# or a sha listed twice, a 'stack-on <ref>' note naming a ref that is not an EARLIER row or not an ancestor of the
# stacked seat, and a seat that is an ancestor of an EARLIER row's seat (merged after it, its own merge would be a no-op).
# Verify round 1 adds: a branch tip that is not the seated sha ("the slot takes the REMOTE TIP ... a stale SHA seats a
# PARENT and drops a row", while a seated branch takes no commits: either way a moved tip needs a ruling, so it is a
# table problem unless COORD names the ref in TIP_MOVED_OK); a row that DESCENDS from an earlier row with no stack-on
# note covering that row ("a legitimate stack is a DECLARATION, printed and counted"); and one patch carried under two
# different commit shas across the rows (a cherry-pick or a re-cut: git merges it clean, twice). TRAIN M's 15 drafted
# rows read 0 of each (2026-10-02: every tip equals its seated sha; 7 descents, all covered; 47 commits, 47 patch-ids).
# Verify round 2 adds: the tip is the REMOTE's (ls-remote above; a branch that is gone reads 'none' and is refused,
# and TIP_MOVED_OK acknowledges only a remote tip that DESCENDS from the seated sha); a row whose sha is already an
# ancestor of the base (a landed seat: before, it passed the table and was refused only after the earlier rows were
# merged); and a seat patch that MASTER already carries under another sha (at TRAIN M: 0 of the 47 seat patch-ids among
# master's 39 non-merge commits c2591d5b95..aa0a07d5fd, read 2026-10-02 on the 15-row list). N: several rows are cut on
# e2008427b1 (TRAIN M's first fixup, an ancestor of the base), so master's fixup-2 joins the patch-id list for them.
# O adds two (tO-CHANGES.md O2). (a) A row that carries ANOTHER TRAIN'S ASSEMBLY COMMITS the base lacks ('Merge
# claude/... into TRAIN X', 'fixup...: TRAIN X', 'refresh: TRAIN X'): O's rows are cut on THREE bases (8f46a9adae, M's
# master; 59ee0d21bf, N's union; e2008427b1, M's union before its fixup-2), and on the right BASE (N's landed master) the
# 59ee rows carry none of N's 31 first-parent commits over it. On a wrong BASE (8f46a9adae, or N's union before its
# fixup-2) they would merge N's whole train in through a seat, git would report it clean, and the shape asserts would
# read the seat merges and not the train they smuggled. Refused by name. (b) A row whose best common ancestor with the
# base and the EARLIER rows is not unique (a re-cut on a local merge of two of them: N's Q20, trainN/tN-DERIVE-REPORT.md
# section 4) is stated as a NOTE: tO-helpers.py precheck then synthesizes that row's own change from every merge base
# (MULTI-BASE), where N's took one of them and over-counted the other's registration lines.
ROWS=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
[ -n "$ROWS" ] || { echo "ABORT: no seat rows in $SEATS"; exit 2; }
NSEATS=$(printf '%s\n' "$ROWS" | wc -l)
RSHA=$(printf '%s\n' "$ROWS" | cut -d'|' -f1,2 | sha256sum | cut -c1-12)   # the rows this run merges; tO-conflict-map.sh prints the same hash for the list it read
dup=$(printf '%s\n' "$ROWS" | cut -d'|' -f1 | LC_ALL=C sort | uniq -d | tr '\n' ' ')
[ -z "$dup" ] || { echo "ABORT: ref listed twice in $SEATS: $dup"; exit 2; }
declare -A ROWOF FULLOF
i=0; tbad=0
while IFS='|' read -r ref sha desc; do
  i=$((i + 1))
  full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "TABLE row $i $ref: seated sha $sha does not resolve"; tbad=$((tbad + 1)); continue; }
  if git merge-base --is-ancestor "$full" "$BASE"; then
    echo "TABLE row $i $ref: ${full:0:10} is already an ancestor of the base ${BASE:0:10} (a LANDED seat: drop the row)"; tbad=$((tbad + 1)); continue
  fi
  # O2 (a): another train's assembly commits that the base lacks, carried in by this row.
  tx=$(git log --format=%s "$BASE..$full" | grep -cE '^(Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )')
  [ "$tx" = 0 ] || { echo "TABLE row $i $ref: carries $tx train-assembly commit(s) the base ${BASE:0:10} lacks (cut on a train union the base does not contain: is BASE TRAIN N's LANDED master?)"; tbad=$((tbad + 1)); }
  # O2 (b): a NOTE, never a refusal (precheck reads such a row as MULTI-BASE).
  nmb=$(git merge-base --all "$full" "$BASE" "${FULLOF[@]}" 2>/dev/null | grep -c .)
  [ "${nmb:-0}" -le 1 ] || echo "TABLE row $i $ref: NOTE $nmb best common ancestors with the base and the earlier rows ($(git merge-base --all "$full" "$BASE" "${FULLOF[@]}" | cut -c1-10 | tr '\n' ' ')): a cut on a local merge of two of them; precheck synthesizes its own change (MULTI-BASE)"
  tip=$(awk -v r="refs/heads/claude/$ref" '$2 == r { print $1 }' "$X/tO/assemble-lsremote.txt"); tip=${tip:-none}
  if [ "$tip" != "$full" ]; then
    echo "TABLE row $i $ref: origin tip ${tip:0:10} != seated ${full:0:10}$([ "$tip" = none ] && echo ' (no such branch on the remote: a withdrawn seat, or a mistyped ref)')"
    tdesc=0; [ "$tip" != none ] && git cat-file -e "$tip^{commit}" 2>/dev/null && git merge-base --is-ancestor "$full" "$tip" && tdesc=1
    case " ${TIP_MOVED_OK:-} :$tdesc" in
      *" $ref "*:1) echo "  acknowledged (TIP_MOVED_OK; the remote tip descends from the seated sha): the commits past the seat ride as a ruled follow-up on a branch of their own (tO-follow.txt), or wait for the next train" ;;
      *" $ref "*:0) echo "  TIP_MOVED_OK names $ref, but its remote tip is gone, not fetched, or NOT a descendant of the seated sha: refused"; tbad=$((tbad + 1)) ;;
      *) tbad=$((tbad + 1)) ;;
    esac
  fi
  for r in "${!FULLOF[@]}"; do
    if [ "${FULLOF[$r]}" = "$full" ]; then
      echo "TABLE row $i $ref: the same sha as row ${ROWOF[$r]} $r"; tbad=$((tbad + 1))
    elif git merge-base --is-ancestor "$full" "${FULLOF[$r]}"; then
      echo "TABLE row $i $ref: ${full:0:10} is an ANCESTOR of the earlier row ${ROWOF[$r]} $r (a base seat must come before the seat stacked on it)"; tbad=$((tbad + 1))
    elif git merge-base --is-ancestor "${FULLOF[$r]}" "$full"; then
      cov=0   # covered = a declared stack-on names this earlier row, or a row that itself descends from it
      for so in $(printf '%s' "$desc" | grep -oE 'stack-on [A-Za-z0-9._-]+' | cut -d' ' -f2); do
        [ -n "${FULLOF[$so]:-}" ] && git merge-base --is-ancestor "${FULLOF[$r]}" "${FULLOF[$so]}" && cov=1
      done
      [ "$cov" = 1 ] || { echo "TABLE row $i $ref: descends from row ${ROWOF[$r]} $r and no 'stack-on' note covers it (an UNDECLARED stack)"; tbad=$((tbad + 1)); }
    fi
  done
  for so in $(printf '%s' "$desc" | grep -oE 'stack-on [A-Za-z0-9._-]+' | cut -d' ' -f2); do
    if [ -z "${FULLOF[$so]:-}" ]; then
      echo "TABLE row $i $ref: 'stack-on $so' names no EARLIER row"; tbad=$((tbad + 1))
    elif ! git merge-base --is-ancestor "${FULLOF[$so]}" "$full"; then
      echo "TABLE row $i $ref: 'stack-on $so' but ${FULLOF[$so]:0:10} is not an ancestor of ${full:0:10}"; tbad=$((tbad + 1))
    else
      echo "TABLE row $i $ref: stacked on row ${ROWOF[$so]} $so (declared; ancestor verified)"
    fi
  done
  # Review round 1: an ORDER note ('after <ref>': the row must merge after that row, with no git stack) was read by no
  # check. A token that is a ref of this table must name an EARLIER row (no ancestry is asked); a later row is a problem.
  # A hyphenated token that is no row of the table is stated (a dropped or mistyped row), never refused: notes are prose.
  for ao in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])after [A-Za-z0-9._-]+' | sed 's/.*after //'); do
    if [ -n "${FULLOF[$ao]:-}" ]; then
      echo "TABLE row $i $ref: ordered after row ${ROWOF[$ao]} $ao (declared; order verified)"
    elif printf '%s\n' "$ROWS" | cut -d'|' -f1 | grep -qxF -- "$ao"; then
      echo "TABLE row $i $ref: 'after $ao' names a LATER row (the row order is the merge order)"; tbad=$((tbad + 1))
    else
      case "$ao" in *-*) echo "TABLE row $i $ref: NOTE 'after $ao' names no row of this table (a dropped or mistyped row?)" ;; esac
    fi
  done
  ROWOF[$ref]=$i; FULLOF[$ref]=$full
done < <(printf '%s\n' "$ROWS")
: > "$X/tO/assemble-patchids.txt"; MBPS=''
while IFS='|' read -r ref sha desc; do
  # Review round 1: a row the table already refused as LANDED (an ancestor of the base) has itself as its merge-base, so
  # this loop read patch-ids for ALL of master since then (MS9c control 3: about 10 minutes on this box under load).
  git merge-base --is-ancestor "$sha" "$BASE" 2>/dev/null && continue
  mbp=$(git merge-base "$BASE" "$sha" 2>/dev/null) || continue
  git log -p --no-merges "$mbp..$sha" | git patch-id --stable | sed "s|\$| $ref|" >> "$X/tO/assemble-patchids.txt"
  case " $MBPS " in *" $mbp "*) ;; *) MBPS="$MBPS $mbp" ;; esac
done < <(printf '%s\n' "$ROWS")
# Verify round 2: the same question asked against MASTER. For every distinct merge-base of a row with the base, master's
# own commits since that merge-base join the list under the name MASTER, so the awk below also refuses a seat patch
# that master already carries under another sha (a cherry-pick that landed with an earlier train: git merges it clean, twice).
for mbp in $MBPS; do
  [ "$mbp" = "$BASE" ] || git log -p --no-merges "$mbp..$BASE" | git patch-id --stable | sed 's|$| MASTER|' >> "$X/tO/assemble-patchids.txt"
done
for id in $(awk '{ if (($1 in c) && c[$1] != $2) d[$1] = 1; else c[$1] = $2 } END { for (k in d) print k }' "$X/tO/assemble-patchids.txt"); do
  echo "TABLE: one patch under two SHAs: $(grep "^$id " "$X/tO/assemble-patchids.txt" | awk '{ print substr($2, 1, 10) " (" $3 ")" }' | LC_ALL=C sort -u | tr '\n' ' ')-- drop the stale row or split the commit"
  tbad=$((tbad + 1))
done
[ "$tbad" = 0 ] || { echo "ABORT: $tbad seat-table problem(s) in $SEATS"; exit 2; }
echo "TABLE OK: $NSEATS rows ($(basename "$SEATS")) rows-sha256=$RSHA, no duplicate ref or sha, no row already on the base, every declared stack-on verified, no undeclared stack, every REMOTE tip at its seated sha (or acknowledged), no patch under two shas (the seats' and master's)"
# Verify round 2 (floor 13): the table check is all that stands between a mis-ordered or duplicated list and one signed
# merge per row, and it had no control, because a planted list that happened to pass went straight on to the merges.
# TABLE_ONLY=1 stops here, before the worktree and before any merge (tO-README.md MS9c names the three planted lists).
[ "${TABLE_ONLY:-0}" = 1 ] && { echo "TABLE_ONLY=1: stopping after the table check (nothing created, nothing merged)"; exit 0; }
ROWFULL=$(printf '%s\n' "$ROWS" | while IFS='|' read -r ref sha desc; do git rev-parse "$sha^{commit}"; done)   # the seats' full shas, in ROW order
# The map union of THIS list: the local ref tO-conflict-map.sh wrote for the same rows-sha256 (ruling R1: no map sha is
# written in a script). MAP_UNION=<sha> at launch overrides it (COORD's explicit call: e.g. a map run by the older hnd
# copy of the map script, which wrote no rows ref).
if [ -n "$MAP_UNION" ]; then MAPSRC="MAP_UNION given at launch"
else MAP_UNION=$(git rev-parse -q --verify "refs/coord/tO-map/rows-$RSHA^{commit}" || true); MAPSRC="refs/coord/tO-map/rows-$RSHA"; fi
# Verify round 3 (RULED): the only tie between the list this run merges and a list a map has read. It passed by absence:
# with no rows ref the line above was a NOTE, the worktree was created and one signed merge was made per row of a list
# NO map had read (a stale or short run-folder copy of the list passes the table check just as well). Refused here,
# before anything is created, with the remedy named.
[ -n "$MAP_UNION" ] || { echo "ABORT: no map run is recorded for THIS list (rows-sha256=$RSHA, $NSEATS rows: no refs/coord/tO-map/rows-$RSHA, and no MAP_UNION given). Remedy: SEATS=$SEATS bash tO-conflict-map.sh from a per-run copy (tO-README.md step 0; it writes that ref when every row is clean), then relaunch; or give MAP_UNION=<the head a map of THIS list printed> by COORD's explicit call. Nothing was created or merged"; exit 2; }
git cat-file -e "$MAP_UNION^{commit}" 2>/dev/null || { echo "ABORT: the map union $MAP_UNION ($MAPSRC) is not a commit in the object store: the assembled tree could not be compared with it. Nothing was created or merged"; exit 2; }
echo "MAP: the union of THIS list is $(git rev-parse --short=10 "$MAP_UNION") ($MAPSRC)"
if [ -d "$WT" ]; then
  [ "$(git -C "$WT" rev-parse --abbrev-ref HEAD)" = claude/coord-trainO-union ] || { echo "ABORT: $WT is not on the union branch"; exit 2; }
  [ -z "$(git -C "$WT" status --porcelain | grep -v '^??')" ] || { echo "ABORT: $WT is dirty"; exit 2; }
  echo "RESUME at $(git -C "$WT" rev-parse --short=10 HEAD)"
else
  git worktree add -q -b claude/coord-trainO-union "$WT" "$BASE" || exit 1
fi
cd "$WT" || exit 1
n=0
while IFS='|' read -r ref sha desc; do
  n=$((n + 1))
  full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "ABORT seat $n $ref: $sha missing"; exit 2; }
  if git merge-base --is-ancestor "$full" HEAD; then
    # Accepted on a RESUME only: this seat's own merge is already on the union's first-parent line. A seat that is
    # contained WITHOUT such a merge rode in through another seat: refused, never skipped (the table check above refuses
    # the row orders that produce it; this is the same question asked of the tree).
    git log --first-parent --format=%P "$BASE..HEAD" | cut -s -d' ' -f2 | grep -qxF "$full" \
      || { echo "ABORT seat $n $ref: ${full:0:10} is contained in HEAD but is not the second parent of a first-parent merge (it rode in through another seat)"; exit 3; }
    echo "SEAT $n $ref: already merged (resume)"; continue
  fi
  printf 'Merge claude/%s (%s) into TRAIN O -- %s\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)" > "$X/tO/msg-seat$n.txt" || exit 2
  # Verify round 1 (merge-hazards: "an empty conflict list is equally the signature of a merge that never ran"): the
  # merge's output is kept, and a failure is named as what it is.
  git merge -q -S --no-ff -F "$X/tO/msg-seat$n.txt" "$full" > "$X/tO/merge-seat$n.log" 2>&1; mrc=$?
  if [ "$mrc" = 0 ]; then echo "SEAT $n $ref: merged $(git rev-parse --short=10 HEAD)"; continue; fi
  un=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  if ! git rev-parse -q --verify MERGE_HEAD > /dev/null; then
    echo "ABORT seat $n $ref: git refused the merge before it ran (rc=$mrc): $(tail -n 2 "$X/tO/merge-seat$n.log" | tr '\r\n' '  ')"
  elif [ -z "$un" ]; then
    echo "ABORT seat $n $ref: the merge is CLEAN and the COMMIT failed (rc=$mrc; signing?): $(tail -n 2 "$X/tO/merge-seat$n.log" | tr '\r\n' '  ')"
  else
    echo "ABORT seat $n $ref: UNRULED conflict in [$un]"
  fi
  git merge --abort 2>/dev/null; exit 3
done < <(printf '%s\n' "$ROWS")
echo "ASSEMBLED head=$(git rev-parse HEAD) seats=$n"
# Verify round 1: the pre-map's readings (every row clean, the shared paths a person read) transfer only if this tree
# IS the map's tree. Verify round 2: the by-name lists of the battery, the fixup and the lane drivers are DERIVED at
# run time from the tree they run on, so what a DIFFERENT line leaves as a premise is the short list of literals a
# derivation cannot produce (tO-README.md MS1 names each by file and variable). The line is also written to
# coord-scratch/tO/assemble-maptree.txt, which tO-fixup.sh stamps in its PRE (before, it was console text only).
LITS='tO-battery.sh GT_NEIGH GT_TESTS CHECKDEAD_GUARDS NEIGH E_BISECT_KNOWN (empty at O) X_ROWS T_ROWS HS_WANT PUB_MIN NPOST; tO-helpers.py EXEC_RULED (empty at O) H3_TOKENS (empty at O) S1_NONE; tO-fixup.sh REGEN_G_SEAT REGEN_G_PKGS (empty at O); tO-linux-legs.sh GT_NEIGH LB_FIXED LX_KNOWN and the row list of L4 / L5 / L6; tO-i9-shard.sh the named rows'
MTBAD=1   # verify round 3: only EQUAL is a pass; every other reading is exit 4 at the end (the two 'not compared' arms are refused before the merges now, and are kept as readings only)
# The tree compared is the one at the NEWEST SEAT MERGE on the first-parent line: HEAD on a first assembly; on a resume
# after a ruled follow-up was merged (tO-README.md step 3) the follow-up legitimately moved HEAD's tree past the map's.
SEATTOP=$(for c in $(git rev-list --first-parent "$BASE..HEAD"); do p2=$(git rev-parse -q --verify "$c^2") || continue; if printf '%s\n' "$ROWFULL" | grep -qxF "$p2"; then echo "$c"; break; fi; done)
SEATTOP=${SEATTOP:-HEAD}
if [ -z "$MAP_UNION" ]; then
  MT="MAP TREE: NOT COMPARED (no map union is recorded for rows-sha256=$RSHA)"
elif git cat-file -e "$MAP_UNION^{commit}" 2>/dev/null; then
  if [ "$(git rev-parse "$SEATTOP^{tree}")" = "$(git rev-parse "$MAP_UNION^{tree}")" ]; then
    MT="MAP TREE: EQUAL to $(git rev-parse --short=10 "$MAP_UNION") ($MAPSRC): the pre-map read THIS tree$([ "$(git rev-parse "$SEATTOP")" = "$(git rev-parse HEAD)" ] || echo " (compared at the last seat merge $(git rev-parse --short=10 "$SEATTOP"); HEAD is past it by follow-up merge(s))")"; MTBAD=0
  else
    MT="MAP TREE: DIFFERENT from $(git rev-parse --short=10 "$MAP_UNION") ($MAPSRC) -- the map read another tree: re-run tO-conflict-map.sh on this list, and walk the literal sites once at this head ($LITS)"
  fi
else
  MT="MAP TREE: $MAP_UNION ($MAPSRC) is not in the object store (not compared)"
fi
echo "$MT"
echo "head=$(git rev-parse --short=10 HEAD) rows=$NSEATS rows-sha256=$RSHA :: $MT" > "$X/tO/assemble-maptree.txt"
while IFS='|' read -r ref sha desc; do git merge-base --is-ancestor "$sha" HEAD || { echo "ANCESTRY FAIL $ref $sha"; exit 4; }; done < <(printf '%s\n' "$ROWS")
echo "ANCESTRY: all seats contained"
# The shape: one first-parent merge per seat row. On a RESUME after a ruled follow-up was merged (tO-README.md step 3),
# the line also holds merges whose second parent is no listed seat: counted and stated here, and checked one by one
# against tO-follow.txt by tO-fixup.sh. A single-parent commit on the line (a fixup already present) is refused.
fp=0; nfu=0; nstray=0
for c in $(git rev-list --first-parent "$BASE..HEAD"); do
  fp=$((fp + 1))
  p2=$(git rev-parse -q --verify "$c^2") || { nstray=$((nstray + 1)); continue; }
  printf '%s\n' "${FULLOF[@]}" | grep -qxF "$p2" || nfu=$((nfu + 1))
done
[ "$nstray" = 0 ] && [ "$fp" = $((NSEATS + nfu)) ] && [ "$n" = "$NSEATS" ] || { echo "SHAPE FAIL: first-parent count $fp (single-parent commits $nstray, merges of no listed seat $nfu), rows merged-or-resumed $n, seat rows $NSEATS"; exit 4; }
echo "SHAPE: $fp first-parent merges = $NSEATS seat rows + $nfu other (follow-ups: tO-fixup.sh checks each against tO-follow.txt)"
# Verify round 2: the count above cannot see ORDER. On a resume, a row inserted mid-list after a first assembly is
# merged LAST and read SHAPE OK, while "the row order is the merge order" is the premise of the table check, of the
# pre-map and of precheck's per-seat bases. The seats' merges, read off the first-parent line oldest first, must be the
# rows in row order.
GOTFULL=$(for c in $(git rev-list --first-parent --reverse "$BASE..HEAD"); do git rev-parse -q --verify "$c^2"; done | grep -xF -f <(printf '%s\n' "$ROWFULL"))
[ "$GOTFULL" = "$ROWFULL" ] || { echo "SHAPE FAIL: the seats' merges on the first-parent line are NOT in row order (a row was inserted or moved after a first assembly): remove the worktree and the union branch and assemble again from the list, never re-order by hand"; exit 4; }
echo "ORDER: the $NSEATS seat merges sit on the first-parent line in row order"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] && echo "TREE CLEAN" || { echo "TREE DIRTY"; git status --porcelain | head; exit 5; }
# Verify round 3: a tree that is not the map union's is not the tree the map's readings describe. With the rows ref it
# cannot differ by construction (same base, same rows, same order); with a MAP_UNION given at launch it means the sha
# named another list's union. Either way the assembly is NOT accepted: exit 4, after every line above was printed.
[ "$MTBAD" = 0 ] || { echo "STOP: $MT -- the assembled union stands at $(git rev-parse --short=10 HEAD) and is NOT accepted (exit 4): run tO-conflict-map.sh on this list and compare, before any fixup"; exit 4; }
echo "NEXT: EXPECT_HEAD=$(git rev-parse --short=10 HEAD) bash tO-fixup.sh from a per-run copy (nothing here pushed). tO-follow.txt is EMPTY; hazard H4 FIRES at the $NSEATS-row list if r-atlas-batch-repros boards without BOTH g-float-untyped-const-compare and r-ptrptr-anon-struct-lift (each completes one of its two Go-only directories), or if r-jwt-promoted-iface-repros is added as cut (three Go-only directories: tO-README.md MS2); if COORD rules a follow-up, merge it signed --no-ff first, list it in tO-follow.txt (tO-README.md step 3), and EXPECT_HEAD is the head after it"
