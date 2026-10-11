#!/usr/bin/env bash
# TRAIN T3 (2026-10-10; derived from TRAIN FL's run2 kit, every change in tT3-CHANGES.md). T3 is NOT a corpus
# re-conversion. The rows of tT3-seats-draft.txt (24 at the derive) merge signed --no-ff, in row order, onto the base
# (origin/master's tip: 4e6322d770 at the derive) on the LOCAL branch train-t3-union in /h/go2cs-tmp-coord/tT3, after
# tT3-conflict-map.sh mapped THIS list from the base (FL's MAP_FROM resume is gone: no T3 row is a hand merge).
# NOTHING HERE PUSHES. T3 adds:
#   T3-2  two notes tokens. 'contains <sha>': in the TABLE check, the row's seated sha carries that commit (row 15 carries
#         cut 8 pack-exclude; rows 19 and 20 carry 64804ed9b5). 'in-head <sha>': in the TABLE check an EARLIER row
#         carries it, and at MERGE time the union's HEAD holds it before the row merges (row 20: 07411b26f7, row 19's tip).
#   T3-3  the i9 tree arm (i9tree, between markers; tT3-controls.sh arm I9 runs it on planted heads): the tree at the
#         last seat merge must EQUAL the tree of the i9's rehearsal union (claude/i9-union-t3-ref, read from the same
#         ls-remote as the seats; I9_TREE=<the posted tree, a prefix is enough> pins what that ref must hold). EQUAL is
#         the only pass; DIFFERENT prints git diff --stat (i9 -> this union) and stops at exit 4 after every other line;
#         a missing ref is a NOTE, and a STOP when I9_TREE is given.
# TRAIN Q assembly (COORD): SIGNED merges of every seat AT ITS SEATED SHA, in table order, onto TRAIN P's landed master
# line as it stands when Q is assembled (BASE: a REQUIRED variable, never a literal here: the checklist carries the sha
# and reads it from the remote), on the new local branch claude/coord-trainQ-union in /h/go2cs-tmp-coord/tT3. Q is an
# ORDINARY train: nothing in this set assumes a release (whether one follows is decided at the landing). The train
# that follows Q re-converts the corpus, so Q lands a clean, fully gated tree.
# Q HAS RULED RESOLUTIONS (trainQ/tQ-CHANGES.md Q20). The i9 REHEARSED the union on Windows (trainQ/tQ-map.md): every
# conflict it met is an insert against an insert at one point -- the BOARD in five rows (seven since review round 1:
# two docs rows accepted behind the rehearsal append there too) and, for ONE row
# (g-named-pointer-equality), go2cs.slnx and the four BehavioralTests lists -- and COORD ruled 'keep both in merge
# order'. Those rows and files are the lines of tT3-ruled.txt; tT3-resolve.py makes each resolution from the index's
# three stages in the diff3 form and asserts the result. ANY other conflict ABORTS with the row and every hunk named
# (exit 3), as at L to P. HOW P DID IT: P had NO resolution (its two pre-map conflicts were closed by stacked re-cuts
# before the table froze; its registration lists merged clean and were read by precheck's COUNT / REG / BOTH arms,
# which still read every merge here). The resolver's mechanism is TRAIN K's (trainK/tK-resolve.py + tK-assemble.sh),
# the last train that resolved by rule: a resolver keyed by the seat, an expected conflict set, a refusal on anything
# else, 'Resolved by rule' in the merge's message.
# ALSO NEW AT Q (Q20): a stacked row's base row must ALREADY BE MERGED on the union's first-parent line when the row
# merges (the table check proves ancestry; this proves the merge), an 'after' row likewise; a SLOT row (a ref whose
# sha field is empty or not a sha) is refused by name, and a list that still holds '#   SLOT' COMMENTS is refused
# unless SLOTS_OK=<their count> acknowledges them (review round 1); a CAND row is refused until its notes carry
# 'READING IN (<when>, <where>)' (that form: review round 1); BASE must be origin/master's tip (Q1b;
# BASE_NOT_TIP_OK=1 by COORD's explicit call).
# Launch from a per-run copy (floor 4). Resumable onto its own clean union branch. The seat count is READ from the seat
# list, never written here: a seat added or dropped changes the list and nothing else (re-run tT3-conflict-map.sh on the
# new list first: the map makes the SAME resolutions through the SAME resolver and the assembled tree must equal its).
# DERIVED 2026-10-06 from trainP/tP-assemble.sh (P: 24 signed merges, MAP TREE EQUAL, 2026-10-04 18:23): names, the
# texts that name the base, the ruled-resolution arm, the table checks above, the literal-site list and the NEXT line
# (trainQ/tQ-CHANGES.md R0, Q1, Q20); every other check is P's. P's was DERIVED 2026-10-04 from trainO/tO-assemble.sh.
# O's was DERIVED 2026-10-03 from trainN/tN-assemble.sh (itself from trainM/tM-assemble.sh): the names, the base (now REQUIRED,
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
# refusal and the signing preflight of tT3-fixup.sh; table problems added: a branch tip that moved past its seated sha
# (unless TIP_MOVED_OK names the ref), a row that descends from an earlier row with no stack-on note covering it, one
# patch under two shas; a failed merge is reported as what it is (refused, clean but uncommitted, or a conflict); the
# shape assert counts a ruled follow-up merge on a resume; the assembled tree is compared with the pre-map union's.
# VERIFY ROUND 2 (trainM/tM-CHANGES.md section 10): the map union is no longer a sha written here: it is found through the
# local ref tT3-conflict-map.sh keeps for the rows it read (refs/coord/tT3-map/rows-<rows-sha256>; MAP_UNION=<sha> still
# overrides); each seat's tip is read with `git ls-remote` at assembly time (a withdrawn branch keeps its local
# tracking ref: no prune); table problems added: a row already on the base (a landed seat), a seat patch master
# already carries under another sha; TABLE_ONLY=1 stops after the table check, before anything is created (the
# check's controls: tT3-README.md MS9c); on a resume the seats' merges must sit on the first-parent line in ROW order;
# the MAP TREE line is also written to coord-scratch/tT3/assemble-maptree.txt, which tT3-fixup.sh stamps in its PRE.
# VERIFY ROUND 3 (trainM/tM-CHANGES.md section 11; RULED): the assembly REFUSES to create or merge anything unless a map has
# read THIS list: the rows ref refs/coord/tT3-map/rows-<rows-sha256> exists, or MAP_UNION=<sha> is given explicitly and
# resolves. (Round 2 printed a NOTE and merged a list no map had read; the tree then read 'NOT COMPARED'.) An assembled
# tree that is not the map union's tree is exit 4 at the end, after the shape lines are printed.
set -u
WT=/h/go2cs-tmp-coord/tT3
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tT3-seats-draft.txt}
X=/h/go2cs-tmp-coord/coord-scratch
MAP_UNION=${MAP_UNION:-}                # normally EMPTY: derived below from the rows-sha256 of the list this run merges
case "$SD" in
  /h/go2cs-tmp-coord/tT3|/h/go2cs-tmp-coord/tT3/*|/h/go2cs-tmp-coord/hnd|/h/go2cs-tmp-coord/hnd/*|*/wf/draft*|/h/Projects/go2cs*)
    echo "ABORT: launched from $SD -- launch from a per-run copy (floor 4)"; exit 2 ;;
esac
[ -f "$SEATS" ] || { echo "ABORT: no seat list ($SEATS): copy trainT3/tT3-seats-draft.txt into the run folder"; exit 2; }
mkdir -p "$X/tT3"
# O (trainO/tO-CHANGES.md O1), carried at P and Q: BASE is the PREVIOUS train's LANDED master, a REQUIRED variable (at Q:
# TRAIN P's landed line: its landing 40a1f839c5, then the docs batch, the 1.24.13.4 release record and the known-issues
# entry; no script of this set carries a base sha). It must resolve, and after the fetch it must be ON origin/master
# (landed), never a union head or a lane ref.
BASE_IN=${BASE:?set BASE to the landed master line of TRAIN Q (and the docs commits COORD landed after it), the full sha origin/master holds when FL is assembled, read from the remote: COORD-LAUNCH-CHECKLIST.md step 0}
cd /h/Projects/go2cs || exit 1
git fetch -q origin || exit 1
BASE=$(git rev-parse --verify -q "$BASE_IN^{commit}") || { echo "ABORT: BASE=$BASE_IN does not resolve to a commit (fetch, or a typo)"; exit 2; }
git merge-base --is-ancestor "$BASE" origin/master || { echo "ABORT: BASE ${BASE:0:10} is not on origin/master: TRAIN T3's base is TRAIN Q's LANDED master line, never a union head or a lane ref"; exit 2; }
# Verify round 2 (merge-hazards: "an announced branch is verified by ls-remote at the moment it is needed"): the fetch
# above does not prune, so a seat branch DELETED on the remote keeps its tracking ref and would read 'tip == seated
# sha'. The tips are read from the remote itself, once, here. Pruning the shared repository is not the remedy (another
# train's job may still read those refs).
git ls-remote origin 'refs/heads/claude/*' > "$X/tT3/assemble-lsremote.txt" || { echo "ABORT: git ls-remote origin failed (the seat tips are verified on the REMOTE, never on a local tracking ref)"; exit 1; }
# Q (Q1b): BASE is origin/master's TIP at the assembly. At P a difference was a NOTE. At Q master is EXPECTED to move
# before the freeze (COORD's notes queue a KnownIssues entry and the census tool), every move re-reads the table at the
# new base, and a base below the tip that is still above P's landing passes every other check (the commits above it
# carry no train subject). Refused; BASE_NOT_TIP_OK=1 runs past it by COORD's explicit call (the landing then merges
# master into the union: tT3-README.md MS11).
if [ "$(git rev-parse origin/master)" != "$BASE" ]; then
  if [ "${BASE_NOT_TIP_OK:-}" = 1 ]; then echo "NOTE: BASE_NOT_TIP_OK=1 (COORD's explicit call): origin/master is $(git rev-parse --short=10 origin/master), not the base ${BASE:0:10} (the landing merges it and re-runs the precheck: tT3-README.md MS11)"
  else echo "ABORT: BASE ${BASE:0:10} is not origin/master's tip ($(git rev-parse --short=10 origin/master), $(git rev-list --count "$BASE..origin/master") commit(s) above it): read BASE from ls-remote at launch (COORD-LAUNCH-CHECKLIST.md step 0), re-read the table at it; BASE_NOT_TIP_OK=1 only by COORD's explicit call"; exit 2; fi
fi
# Review round 1 (trainO/tO-CHANGES.md section 5, F2-1), restated for Q: the ancestor check above passes a STALE base too:
# P's union 446d2c8ba0 or d843ff263d (most Q seats were cut on one of them) is an ancestor of origin/master, and O2 (a)
# then reads 0 for every row cut there. Refused when origin/master's FIRST-PARENT line ABOVE the base holds a
# train-assembly commit (O2's subjects: P's seat merges, its fixups, its refresh); BASE_BEHIND_OK=1 runs past it by
# COORD's explicit call, stamped. (A base above P's landing and below the tip is the tip check's, above.)
BEHIND=$(git log --first-parent --format='%h %s' "$BASE..origin/master" | grep -E '^[0-9a-f]+ (Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )')
if [ -n "$BEHIND" ]; then
  nbh=$(printf '%s\n' "$BEHIND" | grep -c .)
  if [ "${BASE_BEHIND_OK:-}" = 1 ]; then
    echo "NOTE: BASE_BEHIND_OK=1 (COORD's explicit call): origin/master's first-parent line above the base ${BASE:0:10} holds $nbh train-assembly commit(s): $(printf '%s\n' "$BEHIND" | head -n 3 | cut -c1-90 | tr '\n' ';')"
  else
    echo "ABORT: BASE ${BASE:0:10} is BEHIND a landed train: origin/master's first-parent line above it holds $nbh train-assembly commit(s) ($(printf '%s\n' "$BEHIND" | head -n 3 | cut -c1-90 | tr '\n' ';')). TRAIN T3's base is TRAIN Q's LANDED master line, its refresh and the release record included: read BASE from ls-remote (COORD-LAUNCH-CHECKLIST.md step 0); BASE_BEHIND_OK=1 only by COORD's explicit call"
    exit 2
  fi
fi
# Signing preflight BEFORE anything is created (tT3-fixup.sh's block): every merge below is -S, and a locked agent fails a
# CLEAN merge at its commit.
SIGNFMT=$(git config --get gpg.format || true); SIGNKEY=$(git config --get user.signingkey || true)
[ -n "$SIGNKEY" ] || { echo "ABORT: no user.signingkey: merge -S cannot sign"; exit 2; }
case "${SIGNFMT:-openpgp}" in
  openpgp)
    gp=$(git config --get gpg.program || echo gpg); case "$gp" in ?:*) gp=$(cygpath -u "$gp") ;; esac
    "$gp" --batch --list-secret-keys "$SIGNKEY" > /dev/null 2>&1; rc=$?
    [ "$rc" = 0 ] || { echo "ABORT: the signing key is not in the secret keyring (gpg rc=$rc)"; exit 2; }
    if [ "${SIGN_PROBE:-1}" = 1 ]; then  # a real signature through the agent; SIGN_PROBE=0 skips it
      echo "tT3-assemble signing probe" | "$gp" --batch --yes --local-user "$SIGNKEY" --clearsign > "$X/tT3/assemble-sign-probe.asc" 2> "$X/tT3/assemble-sign-probe.err"; rc=$?
      [ "$rc" = 0 ] || { echo "ABORT: the signing probe failed (gpg rc=$rc, $X/tT3/assemble-sign-probe.err): unlock the agent, then rerun"; exit 2; }
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
# O adds two (trainO/tO-CHANGES.md O2). (a) A row that carries ANOTHER TRAIN'S ASSEMBLY COMMITS the base lacks ('Merge
# claude/... into TRAIN X', 'fixup...: TRAIN X', 'refresh: TRAIN X'): O's rows are cut on THREE bases (8f46a9adae, M's
# master; 59ee0d21bf, N's union; e2008427b1, M's union before its fixup-2), and on the right BASE (N's landed master) the
# 59ee rows carry none of N's 31 first-parent commits over it. On a wrong BASE (8f46a9adae, or N's union before its
# fixup-2) they would merge N's whole train in through a seat, git would report it clean, and the shape asserts would
# read the seat merges and not the train they smuggled. Refused by name. (b) A row whose best common ancestor with the
# base and the EARLIER rows is not unique (a re-cut on a local merge of two of them: N's Q20, trainN/tN-DERIVE-REPORT.md
# section 4) is stated as a NOTE: tT3-helpers.py precheck then synthesizes that row's own change from every merge base
# (MULTI-BASE), where N's took one of them and over-counted the other's registration lines.
# AT Q (read with git 2026-10-06, trainQ/tQ-seats-draft.txt line 2 and trainQ/tQ-map.md C): the 64 rows are cut on
# eight bases, every one on BASE's first-parent line (446d2c8ba0 and d843ff263d: P's union at its two fixups;
# 40a1f839c5: P's landing; e1ad9dbc11, 76a9508dfb: the docs batch and the release record; 7a1b2e3631, eb88ab9492: O's;
# and P's landed line itself). None carries a train-assembly commit the base lacks. TWO rows have two best common
# ancestors (c2-module-disclosures, c2-noinline-partial-on-nameof: each merged an earlier row in): NOTEs, never
# refusals. The stated blind spot at Q: a base BELOW master's tip that is still above P's landing (the release record's
# own commits carry no train subject) passes the BEHIND check; the tip check above is its guard.
ROWS=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | tr -d '\r')
[ -n "$ROWS" ] || { echo "ABORT: no seat rows in $SEATS"; exit 2; }
NSEATS=$(printf '%s\n' "$ROWS" | wc -l)
RSHA=$(printf '%s\n' "$ROWS" | cut -d'|' -f1,2 | sha256sum | cut -c1-12)   # the rows this run merges; tT3-conflict-map.sh prints the same hash for the list it read
dup=$(printf '%s\n' "$ROWS" | cut -d'|' -f1 | LC_ALL=C sort | uniq -d | tr '\n' ' ')
[ -z "$dup" ] || { echo "ABORT: ref listed twice in $SEATS: $dup"; exit 2; }
declare -A ROWOF FULLOF
# Q (Q20): a CAND row. The word CAND in a row's notes marks a seat that cannot merge yet (its acceptance waits on a
# reading that is still out, or a re-cut is owed); 'READING IN (' in the same notes marks it as read (COORD writes it,
# with the line that reads it: 'READING IN (<when>, <where>)').
# REVIEW ROUND 1: the release is that FORM and nothing looser. The bare two words released a planted row whose notes
# said 'the hosted READING IN the mailbox is still out' (measured by the review on the extracted predicate).
# >>> candpred (tT3-controls.sh arm CS extracts the lines between the markers and runs them on planted rows)
candrow(){ printf '%s' "$1" | grep -qE '(^|[^A-Za-z0-9_-])CAND([^A-Za-z0-9_-]|$)' && ! printf '%s' "$1" | grep -qF 'READING IN ('; }
# <<< candpred
i=0; tbad=0
while IFS='|' read -r ref sha desc; do
  i=$((i + 1))
  # Q (Q20): a SLOT row. A slot written as a ROW ('ref||notes', 'ref|SLOT|notes') is refused until the table carries a
  # sha for it; a slot written as a '#   SLOT' comment is no row at all (counted below, stated).
  printf '%s' "$sha" | grep -qE '^[0-9a-f]{7,40}$' || { echo "TABLE row $i $ref: a SLOT row (its sha field is '${sha:-empty}', not a sha): refused until the table carries a sha for it"; tbad=$((tbad + 1)); continue; }
  # Q (Q20): a CAND row (candrow, defined above the loop). No override.
  if candrow "$desc"; then
    echo "TABLE row $i $ref: a CAND row whose reading is not marked as in (its notes say CAND and hold no 'READING IN (<when>, <where>)'): strike the row (and every row that rides only with it), or write that form in its notes once the reading or the re-cut it waits on is in"; tbad=$((tbad + 1))
  fi
  full=$(git rev-parse --verify -q "$sha^{commit}") || { echo "TABLE row $i $ref: seated sha $sha does not resolve"; tbad=$((tbad + 1)); continue; }
  if git merge-base --is-ancestor "$full" "$BASE"; then
    echo "TABLE row $i $ref: ${full:0:10} is already an ancestor of the base ${BASE:0:10} (a LANDED seat: drop the row)"; tbad=$((tbad + 1)); continue
  fi
  # O2 (a): another train's assembly commits that the base lacks, carried in by this row.
  # FL (FL9): a row CUT ON THIS TRAIN'S OWN UNION carries the union's seat merges ('Merge claude/<ref> (<sha>) into TRAIN
  # FL ...'): row 10, C2's ONE regeneration, is cut on the hand union 34ccf6981f (rows 1-9), as ruled (ledger 04:11 on
  # 2026-10-08). Those merges are admitted when each one's second parent is an EARLIER row's seated sha; any other
  # train's assembly commit, and any 'fixup' or 'refresh' (this train's included), is still refused.
  tx=$(git log --format=%s "$BASE..$full" | grep -E '^(Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN [A-Z]+ |fixup(-[0-9]+)?: TRAIN [A-Z]+ |refresh: TRAIN [A-Z]+ )' | grep -vcE '^Merge claude/[^ ]+ \([0-9a-f]+\) into TRAIN T3 ')
  for um in $(git log --merges --format=%H "$BASE..$full" --grep='^Merge claude/[^ ]* ([0-9a-f]*) into TRAIN T3 '); do
    up2=$(git rev-parse "$um^2"); uok=0
    for r in "${!FULLOF[@]}"; do [ "${FULLOF[$r]}" = "$up2" ] && uok=1; done
    [ "$uok" = 1 ] || { echo "TABLE row $i $ref: carries the TRAIN T3 union merge ${um:0:10} whose second parent ${up2:0:10} is no EARLIER row of this table"; tbad=$((tbad + 1)); }
  done
  [ "$tx" = 0 ] || { echo "TABLE row $i $ref: carries $tx train-assembly commit(s) the base ${BASE:0:10} lacks (cut on a train union the base does not contain: is BASE TRAIN Q's LANDED master line?)"; tbad=$((tbad + 1)); }
  # O2 (b): a NOTE, never a refusal (precheck reads such a row as MULTI-BASE).
  nmb=$(git merge-base --all "$full" "$BASE" "${FULLOF[@]}" 2>/dev/null | grep -c .)
  [ "${nmb:-0}" -le 1 ] || echo "TABLE row $i $ref: NOTE $nmb best common ancestors with the base and the earlier rows ($(git merge-base --all "$full" "$BASE" "${FULLOF[@]}" | cut -c1-10 | tr '\n' ' ')): a cut on a local merge of two of them; precheck synthesizes its own change (MULTI-BASE)"
  tip=$(awk -v r="refs/heads/claude/$ref" '$2 == r { print $1 }' "$X/tT3/assemble-lsremote.txt"); tip=${tip:-none}
  if [ "$tip" != "$full" ]; then
    echo "TABLE row $i $ref: origin tip ${tip:0:10} != seated ${full:0:10}$([ "$tip" = none ] && echo ' (no such branch on the remote: a withdrawn seat, or a mistyped ref)')"
    tdesc=0; [ "$tip" != none ] && git cat-file -e "$tip^{commit}" 2>/dev/null && git merge-base --is-ancestor "$full" "$tip" && tdesc=1
    case " ${TIP_MOVED_OK:-} :$tdesc" in
      *" $ref "*:1) echo "  acknowledged (TIP_MOVED_OK; the remote tip descends from the seated sha): the commits past the seat ride as a ruled follow-up on a branch of their own (tT3-follow.txt), or wait for the next train" ;;
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
  # T3 (T3-2): 'contains <sha>' -- the row CARRIES that commit; 'in-head <sha>' -- an EARLIER row carries it (the merge
  # loop asserts it is in HEAD before this row merges).
  for cs in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])contains [0-9a-f]{7,40}' | sed 's/.*contains //'); do
    csf=$(git rev-parse --verify -q "$cs^{commit}") || { echo "TABLE row $i $ref: 'contains $cs' does not resolve"; tbad=$((tbad + 1)); continue; }
    if git merge-base --is-ancestor "$csf" "$full"; then echo "TABLE row $i $ref: contains ${csf:0:10} (declared; ancestor verified)"
    else echo "TABLE row $i $ref: 'contains ${csf:0:10}' but it is NOT an ancestor of ${full:0:10}"; tbad=$((tbad + 1)); fi
  done
  for ih in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])in-head [0-9a-f]{7,40}' | sed 's/.*in-head //'); do
    ihf=$(git rev-parse --verify -q "$ih^{commit}") || { echo "TABLE row $i $ref: 'in-head $ih' does not resolve"; tbad=$((tbad + 1)); continue; }
    ihr=''; for r in "${!FULLOF[@]}"; do git merge-base --is-ancestor "$ihf" "${FULLOF[$r]}" && ihr="$ihr$r "; done
    if [ -n "$ihr" ]; then echo "TABLE row $i $ref: in-head ${ihf:0:10} (declared; carried by the earlier row(s) ${ihr% }; asserted in HEAD at the merge)"
    else echo "TABLE row $i $ref: 'in-head ${ihf:0:10}' but NO earlier row carries it (the merge could never find it in HEAD)"; tbad=$((tbad + 1)); fi
  done
  ROWOF[$ref]=$i; FULLOF[$ref]=$full
done < <(printf '%s\n' "$ROWS")
# REVIEW ROUND 1: an OPEN SLOT written as a comment ('#   SLOT ...': the table's own form for a row that has no ref
# yet) was counted on one RULED line and the assembly went on: a train could be assembled with a row COORD still meant
# to seat. A list that holds such comments is a table problem unless COORD acknowledges exactly that many with
# SLOTS_OK=<n> (the slots then land behind Q, or never: COORD's call, stamped here and on the RULED line).
# >>> slotpred (tT3-controls.sh arm CS extracts the lines between the markers; tT3-conflict-map.sh carries the same two)
NSLOT=$(grep -cE '^#[[:space:]]*SLOT' "$SEATS"); NSLOT=${NSLOT:-0}
slotsok(){ [ "$1" = 0 ] || [ "${2:-}" = "$1" ]; }
# <<< slotpred
if slotsok "$NSLOT" "${SLOTS_OK:-}"; then
  [ "$NSLOT" = 0 ] || echo "TABLE: NOTE SLOTS_OK=$SLOTS_OK (COORD's explicit call): the list holds $NSLOT '#   SLOT' comment(s), no rows, nothing is merged for them"
else
  echo "TABLE: the list still holds $NSLOT '#   SLOT' comment(s) (an open slot with no ref) and SLOTS_OK is '${SLOTS_OK:-unset}': seat each slot as a row or delete its comment, or acknowledge them all with SLOTS_OK=$NSLOT (COORD's explicit call)"; tbad=$((tbad + 1))
fi
: > "$X/tT3/assemble-patchids.txt"; MBPS=''
while IFS='|' read -r ref sha desc; do
  # Review round 1: a row the table already refused as LANDED (an ancestor of the base) has itself as its merge-base, so
  # this loop read patch-ids for ALL of master since then (MS9c control 3: about 10 minutes on this box under load).
  git merge-base --is-ancestor "$sha" "$BASE" 2>/dev/null && continue
  mbp=$(git merge-base "$BASE" "$sha" 2>/dev/null) || continue
  git log -p --no-merges "$mbp..$sha" | git patch-id --stable | sed "s|\$| $ref|" >> "$X/tT3/assemble-patchids.txt"
  case " $MBPS " in *" $mbp "*) ;; *) MBPS="$MBPS $mbp" ;; esac
done < <(printf '%s\n' "$ROWS")
# Verify round 2: the same question asked against MASTER. For every distinct merge-base of a row with the base, master's
# own commits since that merge-base join the list under the name MASTER, so the awk below also refuses a seat patch
# that master already carries under another sha (a cherry-pick that landed with an earlier train: git merges it clean, twice).
for mbp in $MBPS; do
  [ "$mbp" = "$BASE" ] || git log -p --no-merges "$mbp..$BASE" | git patch-id --stable | sed 's|$| MASTER|' >> "$X/tT3/assemble-patchids.txt"
done
for id in $(awk '{ if (($1 in c) && c[$1] != $2) d[$1] = 1; else c[$1] = $2 } END { for (k in d) print k }' "$X/tT3/assemble-patchids.txt"); do
  echo "TABLE: one patch under two SHAs: $(grep "^$id " "$X/tT3/assemble-patchids.txt" | awk '{ print substr($2, 1, 10) " (" $3 ")" }' | LC_ALL=C sort -u | tr '\n' ' ')-- drop the stale row or split the commit"
  tbad=$((tbad + 1))
done
[ "$tbad" = 0 ] || { echo "ABORT: $tbad seat-table problem(s) in $SEATS"; exit 2; }
echo "TABLE OK: $NSEATS rows ($(basename "$SEATS")) rows-sha256=$RSHA, no duplicate ref or sha, no row already on the base, every declared stack-on verified, no undeclared stack, every REMOTE tip at its seated sha (or acknowledged), no patch under two shas (the seats' and master's)"
# Verify round 2 (floor 13): the table check is all that stands between a mis-ordered or duplicated list and one signed
# merge per row, and it had no control, because a planted list that happened to pass went straight on to the merges.
# TABLE_ONLY=1 stops here, before the worktree and before any merge (tT3-README.md MS9c names the three planted lists).
[ "${TABLE_ONLY:-0}" = 1 ] && { echo "TABLE_ONLY=1: stopping after the table check (nothing created, nothing merged)"; exit 0; }
ROWFULL=$(printf '%s\n' "$ROWS" | while IFS='|' read -r ref sha desc; do git rev-parse "$sha^{commit}"; done)   # the seats' full shas, in ROW order
# The map union of THIS list: the local ref tT3-conflict-map.sh wrote for the same rows-sha256 (ruling R1: no map sha is
# written in a script). MAP_UNION=<sha> at launch overrides it (COORD's explicit call: e.g. a map run by the older hnd
# copy of the map script, which wrote no rows ref).
if [ -n "$MAP_UNION" ]; then MAPSRC="MAP_UNION given at launch"
else MAP_UNION=$(git rev-parse -q --verify "refs/coord/tT3-map/rows-$RSHA^{commit}" || true); MAPSRC="refs/coord/tT3-map/rows-$RSHA"; fi
# Verify round 3 (RULED): the only tie between the list this run merges and a list a map has read. It passed by absence:
# with no rows ref the line above was a NOTE, the worktree was created and one signed merge was made per row of a list
# NO map had read (a stale or short run-folder copy of the list passes the table check just as well). Refused here,
# before anything is created, with the remedy named.
[ -n "$MAP_UNION" ] || { echo "ABORT: no map run is recorded for THIS list (rows-sha256=$RSHA, $NSEATS rows: no refs/coord/tT3-map/rows-$RSHA, and no MAP_UNION given). Remedy: SEATS=$SEATS bash tT3-conflict-map.sh from a per-run copy (tT3-README.md step 3, checklist section 3; it writes that ref when every row is clean or resolved by rule), then relaunch; or give MAP_UNION=<the head a map of THIS list printed> by COORD's explicit call. Nothing was created or merged"; exit 2; }
git cat-file -e "$MAP_UNION^{commit}" 2>/dev/null || { echo "ABORT: the map union $MAP_UNION ($MAPSRC) is not a commit in the object store: the assembled tree could not be compared with it. Nothing was created or merged"; exit 2; }
echo "MAP: the union of THIS list is $(git rev-parse --short=10 "$MAP_UNION") ($MAPSRC)"
# Q (Q20): the rulings this run may make are the ones the map made. tT3-conflict-map.sh stamps the hash of the ruled
# file it read beside its rows snapshot (map-ruled-<rows-sha256>.txt); a different file here is refused before anything
# is created (with a MAP_UNION given at launch there is no stamp to compare: the tree comparison at the end decides).
RULEDF=${RULED:-$SD/tT3-ruled.txt}; RESOLVER=$SD/tT3-resolve.py; NRULED=0
[ -f "$RULEDF" ] && [ -f "$RESOLVER" ] || { echo "ABORT: $RULEDF or $RESOLVER is missing from the run copy (copy trainT3/tT3-ruled.txt and tT3-resolve.py)"; exit 2; }
RULSHA=$(grep -vE '^[[:space:]]*(#|$)' "$RULEDF" | tr -d '\r' | sha256sum | cut -c1-12)
MAPRUL=$(head -n 1 "$X/tT3/map-ruled-$RSHA.txt" 2>/dev/null | cut -d' ' -f1)
if [ "$MAPSRC" = "refs/coord/tT3-map/rows-$RSHA" ]; then
  [ "$MAPRUL" = "$RULSHA" ] || { echo "ABORT: the ruled file of this run ($RULEDF, ruled-sha256=$RULSHA) is not the one the map of this list read (${MAPRUL:-no map-ruled-$RSHA.txt stamp}): a ruling is written BEFORE the map; re-run tT3-conflict-map.sh with this file, then relaunch. Nothing was created or merged"; exit 2; }
fi
echo "RULED: $(grep -cvE '^[[:space:]]*(#|$)' "$RULEDF") line(s) of $(basename "$RULEDF"), ruled-sha256=$RULSHA (the map's: ${MAPRUL:-not stamped}); SLOT comments in the list (no rows, nothing merged for them): $NSLOT$([ "$NSLOT" = 0 ] || echo " (acknowledged: SLOTS_OK=${SLOTS_OK:-})")"
if [ -d "$WT" ]; then
  [ "$(git -C "$WT" rev-parse --abbrev-ref HEAD)" = train-t3-union ] || { echo "ABORT: $WT is not on the union branch"; exit 2; }
  [ -z "$(git -C "$WT" status --porcelain | grep -v '^??')" ] || { echo "ABORT: $WT is dirty"; exit 2; }
  echo "RESUME at $(git -C "$WT" rev-parse --short=10 HEAD)"
else
  git worktree add -q -b train-t3-union "$WT" "$BASE" || exit 1
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
  # Q (Q20): a stacked row asserts its base row is ALREADY MERGED (that row's own merge sits on the union's first-parent
  # line), and so does an ORDER row ('after'). The table check proved ancestry and row order; this is the same question
  # asked of the union, at the moment of the merge.
  for so in $(printf '%s' "$desc" | grep -oE 'stack-on [A-Za-z0-9._-]+' | cut -d' ' -f2) $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])after [A-Za-z0-9._-]+' | sed 's/.*after //'); do
    [ -n "${FULLOF[$so]:-}" ] || continue   # a prose token that names no row: the table check stated it
    git log --first-parent --format=%P "$BASE..HEAD" | cut -s -d' ' -f2 | grep -qxF "${FULLOF[$so]}" \
      || { echo "ABORT seat $n $ref: its base row $so (${FULLOF[$so]:0:10}) is NOT merged on the union's first-parent line yet: a stacked or ordered row never merges before the row it names"; exit 3; }
  done
  # T3 (T3-2): 'in-head <sha>': the union's HEAD holds that commit BEFORE this row merges (row 20: 07411b26f7, row 19's).
  for ih in $(printf '%s' "$desc" | grep -oE '(^|[^A-Za-z0-9_-])in-head [0-9a-f]{7,40}' | sed 's/.*in-head //'); do
    git merge-base --is-ancestor "$ih" HEAD || { echo "ABORT seat $n $ref: in-head $ih is NOT in HEAD $(git rev-parse --short=10 HEAD): the row that carries it has not merged"; exit 3; }
    echo "SEAT $n $ref: in-head ${ih:0:10} verified in HEAD $(git rev-parse --short=10 HEAD)"
  done
  printf 'Merge claude/%s (%s) into TRAIN T3 -- %s\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)" > "$X/tT3/msg-seat$n.txt" || exit 2
  # Verify round 1 (merge-hazards: "an empty conflict list is equally the signature of a merge that never ran"): the
  # merge's output is kept, and a failure is named as what it is.
  git merge -q -S --no-ff -F "$X/tT3/msg-seat$n.txt" "$full" > "$X/tT3/merge-seat$n.log" 2>&1; mrc=$?
  if [ "$mrc" = 0 ]; then echo "SEAT $n $ref: merged $(git rev-parse --short=10 HEAD)"; continue; fi
  un=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  if ! git rev-parse -q --verify MERGE_HEAD > /dev/null; then
    echo "ABORT seat $n $ref: git refused the merge before it ran (rc=$mrc): $(tail -n 2 "$X/tT3/merge-seat$n.log" | tr '\r\n' '  ')"
  elif [ -z "$un" ]; then
    echo "ABORT seat $n $ref: the merge is CLEAN and the COMMIT failed (rc=$mrc; signing?): $(tail -n 2 "$X/tT3/merge-seat$n.log" | tr '\r\n' '  ')"
  else
    # Q (Q20): a conflict. The ONLY resolutions are tT3-ruled.txt's, made by tT3-resolve.py (two phases: nothing is
    # written unless EVERY unmerged path of this merge is ruled for this row at this sha and passes its asserts).
    python -B "$(cygpath -w "$RESOLVER")" "$(cygpath -w "$WT")" "$(cygpath -w "$RULEDF")" "$ref" "$full" > "$X/tT3/resolve-seat$n.log" 2>&1; rrc=$?
    tr -d '\r' < "$X/tT3/resolve-seat$n.log" | sed 's/^/  /'
    if [ "$rrc" = 0 ] && [ -z "$(git diff --name-only --diff-filter=U)" ]; then
      printf 'Merge claude/%s (%s) into TRAIN T3 -- %s\n\nResolved by rule (trainT3/tT3-ruled.txt through tT3-resolve.py: an insert against an insert, both sides kept in the ruled order): %s\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n' "$ref" "${full:0:10}" "$(printf '%s' "$desc" | cut -c1-220)" "${un% }" > "$X/tT3/msg-seat$n.txt" || exit 2
      git commit -q -S -F "$X/tT3/msg-seat$n.txt" >> "$X/tT3/merge-seat$n.log" 2>&1 || { echo "ABORT seat $n $ref: the ruled resolution was made and the COMMIT failed (signing?): $(tail -n 2 "$X/tT3/merge-seat$n.log" | tr '\r\n' '  ')"; git merge --abort 2>/dev/null; exit 3; }
      [ "$(git rev-parse -q --verify HEAD^2)" = "$full" ] || { echo "ABORT seat $n $ref: the commit after the ruled resolution is not a merge of ${full:0:10}"; exit 3; }
      NRULED=$((NRULED + 1))
      echo "SEAT $n $ref: merged WITH RULED RESOLUTION $(git rev-parse --short=10 HEAD) [${un% }]"; continue
    fi
    echo "ABORT seat $n $ref: UNRULED conflict in [$un] (tT3-resolve.py rc=$rrc: the row and each hunk are named above; $X/tT3/resolve-seat$n.log). Nothing was resolved: a ruling is a line of tT3-ruled.txt, written BEFORE the map"
  fi
  git merge --abort 2>/dev/null; exit 3
done < <(printf '%s\n' "$ROWS")
echo "ASSEMBLED head=$(git rev-parse HEAD) seats=$n"
# Verify round 1: the pre-map's readings (every row clean, the shared paths a person read) transfer only if this tree
# IS the map's tree. Verify round 2: the by-name lists of the battery, the fixup and the lane drivers are DERIVED at
# run time from the tree they run on, so what a DIFFERENT line leaves as a premise is the short list of literals a
# derivation cannot produce (tT3-README.md MS1 names each by file and variable). The line is also written to
# coord-scratch/tT3/assemble-maptree.txt, which tT3-fixup.sh stamps in its PRE (before, it was console text only).
LITS='tT3-ruled.txt (every line: a ruling names a row AND its sha); tT3-battery.sh GT_NEIGH GT_TESTS CHECKDEAD_GUARDS NEIGH X_ROWS T_ROWS T2_ROW HS_WANT PUB_MIN IDC_MIN NPOST CB_KNOWN_SKIPS (UF_KNOWN is empty at Q; E_BISECT_KNOWN is DERIVED from the list since P); tT3-helpers.py EXEC_RULED (empty) H3_TOKENS (empty at Q: no row edits golib Goroutine.cs) S1_NONE (three hand csproj of i9-pack-symbols added at Q) EXTERNAL_RX; tT3-fixup.sh REGEN_G_SEAT REGEN_G_PKGS (empty) TESTS_ARM_ROWS; tT3-modules-legs.sh MOD_TAGS_FALLBACK; tT3-linux-legs.sh GT_NEIGH LB_FIXED LX_KNOWN TWOPASS_ROW and the row list of L4 / L5 / L6; tT3-i9-shard.sh the named rows and TWOPASS_ROW'
# >>> i9tree (T3-3; tT3-controls.sh arm I9 extracts the lines between the markers and runs them on planted heads)
i9tree(){ # head i9-tip [posted i9 tree, a prefix] -> one verdict line; rc 0 EQUAL; 1 DIFFERENT, or the i9 tip does not hold the posted tree; 2 not comparable
  local h=$1 t=$2 e=${3:-} ht tt
  [ -n "$t" ] || { echo "I9 TREE: NOT COMPARED (no i9 union ref at origin)"; return 2; }
  git cat-file -e "$t^{commit}" 2>/dev/null || { echo "I9 TREE: NOT COMPARED (${t:0:10} is not in the object store: fetch it)"; return 2; }
  tt=$(git rev-parse "$t^{tree}"); ht=$(git rev-parse "$h^{tree}")
  if [ -n "$e" ] && [ "${tt#"$e"}" = "$tt" ]; then echo "I9 TREE: REFUSED -- the i9 union ${t:0:10} holds tree ${tt:0:10}, not the posted $e"; return 1; fi
  if [ "$ht" = "$tt" ]; then echo "I9 TREE: EQUAL -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} = the i9 union ${t:0:10} tree ${tt:0:10}"; return 0; fi
  echo "I9 TREE: DIFFERENT -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} != the i9 union ${t:0:10} tree ${tt:0:10}; the paths (git diff --stat, i9 -> this union):"
  git diff --stat=160 "$t" "$h" | sed 's/^/  /'; return 1
}
# <<< i9tree
MTBAD=1   # verify round 3: only EQUAL is a pass; every other reading is exit 4 at the end (the two 'not compared' arms are refused before the merges now, and are kept as readings only)
# The tree compared is the one at the NEWEST SEAT MERGE on the first-parent line: HEAD on a first assembly; on a resume
# after a ruled follow-up was merged (the procedure is trainP/tP-README.md step 3) the follow-up legitimately moved HEAD's tree past the map's.
SEATTOP=$(for c in $(git rev-list --first-parent "$BASE..HEAD"); do p2=$(git rev-parse -q --verify "$c^2") || continue; if printf '%s\n' "$ROWFULL" | grep -qxF "$p2"; then echo "$c"; break; fi; done)
SEATTOP=${SEATTOP:-HEAD}
if [ -z "$MAP_UNION" ]; then
  MT="MAP TREE: NOT COMPARED (no map union is recorded for rows-sha256=$RSHA)"
elif git cat-file -e "$MAP_UNION^{commit}" 2>/dev/null; then
  if [ "$(git rev-parse "$SEATTOP^{tree}")" = "$(git rev-parse "$MAP_UNION^{tree}")" ]; then
    MT="MAP TREE: EQUAL to $(git rev-parse --short=10 "$MAP_UNION") ($MAPSRC): the pre-map read THIS tree$([ "$(git rev-parse "$SEATTOP")" = "$(git rev-parse HEAD)" ] || echo " (compared at the last seat merge $(git rev-parse --short=10 "$SEATTOP"); HEAD is past it by follow-up merge(s))")"; MTBAD=0
  else
    MT="MAP TREE: DIFFERENT from $(git rev-parse --short=10 "$MAP_UNION") ($MAPSRC) -- the map read another tree: re-run tT3-conflict-map.sh on this list, and walk the literal sites once at this head ($LITS)"
  fi
else
  MT="MAP TREE: $MAP_UNION ($MAPSRC) is not in the object store (not compared)"
fi
echo "$MT"
echo "head=$(git rev-parse --short=10 HEAD) rows=$NSEATS rows-sha256=$RSHA :: $MT" > "$X/tT3/assemble-maptree.txt"
# T3 (T3-3): the i9's rehearsal union, read from the SAME ls-remote as the seat tips (never a local tracking ref).
I9_REF=${I9_REF:-claude/i9-union-t3-ref}
I9TIP=$(awk -v r="refs/heads/$I9_REF" '$2 == r { print $1 }' "$X/tT3/assemble-lsremote.txt")
I9V=$(i9tree "$SEATTOP" "$I9TIP" "${I9_TREE:-}"); i9rc=$?
printf '%s\n' "$I9V"
echo "head=$(git rev-parse --short=10 HEAD) :: $(printf '%s' "$I9V" | head -n 1) (I9_REF=$I9_REF I9_TREE=${I9_TREE:-unset})" >> "$X/tT3/assemble-maptree.txt"
I9BAD=0
case "$i9rc" in 0) ;; 1) I9BAD=1 ;; *) [ -z "${I9_TREE:-}" ] || I9BAD=1 ;; esac   # not comparable: a NOTE, a STOP when COORD posted a tree
while IFS='|' read -r ref sha desc; do git merge-base --is-ancestor "$sha" HEAD || { echo "ANCESTRY FAIL $ref $sha"; exit 4; }; done < <(printf '%s\n' "$ROWS")
echo "ANCESTRY: all seats contained"
# The shape: one first-parent merge per seat row. On a RESUME after a ruled follow-up was merged (trainP/tP-README.md step 3),
# the line also holds merges whose second parent is no listed seat: counted and stated here, and checked one by one
# against tT3-follow.txt by tT3-fixup.sh. A single-parent commit on the line (a fixup already present) is refused.
fp=0; nfu=0; nstray=0
for c in $(git rev-list --first-parent "$BASE..HEAD"); do
  fp=$((fp + 1))
  p2=$(git rev-parse -q --verify "$c^2") || { nstray=$((nstray + 1)); continue; }
  printf '%s\n' "${FULLOF[@]}" | grep -qxF "$p2" || nfu=$((nfu + 1))
done
[ "$nstray" = 0 ] && [ "$fp" = $((NSEATS + nfu)) ] && [ "$n" = "$NSEATS" ] || { echo "SHAPE FAIL: first-parent count $fp (single-parent commits $nstray, merges of no listed seat $nfu), rows merged-or-resumed $n, seat rows $NSEATS"; exit 4; }
echo "SHAPE: $fp first-parent merges = $NSEATS seat rows + $nfu other (follow-ups: tT3-fixup.sh checks each against tT3-follow.txt)"
# Verify round 2: the count above cannot see ORDER. On a resume, a row inserted mid-list after a first assembly is
# merged LAST and read SHAPE OK, while "the row order is the merge order" is the premise of the table check, of the
# pre-map and of precheck's per-seat bases. The seats' merges, read off the first-parent line oldest first, must be the
# rows in row order.
GOTFULL=$(for c in $(git rev-list --first-parent --reverse "$BASE..HEAD"); do git rev-parse -q --verify "$c^2"; done | grep -xF -f <(printf '%s\n' "$ROWFULL"))
[ "$GOTFULL" = "$ROWFULL" ] || { echo "SHAPE FAIL: the seats' merges on the first-parent line are NOT in row order (a row was inserted or moved after a first assembly): remove the worktree and the union branch and assemble again from the list, never re-order by hand"; exit 4; }
echo "ORDER: the $NSEATS seat merges sit on the first-parent line in row order"
# Q (Q20): the ruled files at the assembled head, read from the tree (compare with trainQ/tQ-map.md sections A and B:
# with all 64 rows the BOARD reads 26507 lines, go2cs.slnx 959 Project lines, the lists 816 / 816 / 816 / 787), and the
# invariants that no count carries: the BOARD's guard is its final line, and every list holds as many [TestMethod]
# lines as Check methods (a resolution made from the wrong conflict form loses exactly one attribute line a list).
BOARDP=docs/phase4/BOARD-next-validation-candidates.md
bl=$(git show "HEAD:$BOARDP" | wc -l); blb=$(git show "$BASE:$BOARDP" | wc -l); bg=$(git show "HEAD:$BOARDP" | tail -n 1 | grep -c '^<!-- {% endraw %}')
sp=$(git show HEAD:src/go2cs.slnx | grep -c '<Project Path='); spb=$(git show "$BASE:src/go2cs.slnx" | grep -c '<Project Path=')
lr=''; lbad=0
for lf in TranspileTests CompileTests TargetComparisonTests OutputComparisonTests; do
  tm=$(git show "HEAD:src/tests/Behavioral/BehavioralTests/$lf.cs" | tr -d '\r' | grep -c '^[[:space:]]*\[TestMethod\]$')
  ck=$(git show "HEAD:src/tests/Behavioral/BehavioralTests/$lf.cs" | tr -d '\r' | grep -cE '^[[:space:]]*public void Check[A-Za-z0-9_]*\(')
  lr="$lr $lf=$tm/$ck"; [ "$tm" = "$ck" ] || lbad=1
done
echo "RULED-FILES at $(git rev-parse --short=10 HEAD): BOARD lines=$bl (base $blb, +$((bl - blb))) guard-final=$bg; go2cs.slnx Project lines=$sp (base $spb, +$((sp - spb))); lists [TestMethod]/Check:$lr"
[ "$bg" = 1 ] && [ "$lbad" = 0 ] || { echo "SHAPE FAIL: a ruled file reads wrong at the assembled head (the BOARD's final line is not its endraw guard, or a list's [TestMethod] count is not its Check count)"; exit 4; }
# The rulings the union's merges USED, read from the merges themselves (so a resume reads the same answer): a line of
# tT3-ruled.txt whose row merged with no 'Resolved by rule' naming its path is UNUSED (the row merged clean: the order
# moved, or the ruling was never needed). A NOTE: the tree comparison below is the gate.
rused=0; runused=''
while IFS='|' read -r rr rs rp rk rrest; do
  [ -n "${FULLOF[$rr]:-}" ] || { runused="$runused $rr(no such row)"; continue; }
  rm_=$(for c in $(git rev-list --first-parent "$BASE..HEAD"); do [ "$(git rev-parse -q --verify "$c^2")" = "${FULLOF[$rr]}" ] && { echo "$c"; break; }; done)
  if [ -n "$rm_" ] && git log -1 --format=%B "$rm_" | grep -F 'Resolved by rule' | grep -qF -- "$rp"; then rused=$((rused + 1)); else runused="$runused $rr:${rp##*/}"; fi
done < <(grep -vE '^[[:space:]]*(#|$)' "$RULEDF" | tr -d '\r')
echo "RULED: $(grep -cvE '^[[:space:]]*(#|$)' "$RULEDF") line(s) in $(basename "$RULEDF") (ruled-sha256=$RULSHA), $rused used by the union's merges ($NRULED resolution merge(s) made in this run); UNUSED:${runused:- none}"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] && echo "TREE CLEAN" || { echo "TREE DIRTY"; git status --porcelain | head; exit 5; }
# Verify round 3: a tree that is not the map union's is not the tree the map's readings describe. With the rows ref it
# cannot differ by construction (same base, same rows, same order); with a MAP_UNION given at launch it means the sha
# named another list's union. Either way the assembly is NOT accepted: exit 4, after every line above was printed.
[ "$I9BAD" = 0 ] || { echo "STOP: $(printf '%s' "$I9V" | head -n 1) -- the assembled union stands at $(git rev-parse --short=10 HEAD) and is NOT accepted (exit 4): COORD reads the listed paths before anything else"; exit 4; }
[ "$MTBAD" = 0 ] || { echo "STOP: $MT -- the assembled union stands at $(git rev-parse --short=10 HEAD) and is NOT accepted (exit 4): run tT3-conflict-map.sh on this list and compare, before any fixup"; exit 4; }
echo "NEXT: EXPECT_HEAD=$(git rev-parse --short=10 HEAD) bash tT3-fixup.sh from a per-run copy (nothing here pushed). tT3-follow.txt is EMPTY; hazard H4 is read by precheck at this head (no Q row adds a Go-only behavioral directory, read from each row's diff: tT3-README.md MS2); if COORD rules a follow-up, merge it signed --no-ff first, list it in tT3-follow.txt (the procedure: trainP/tP-README.md step 3), and EXPECT_HEAD is the head after it"
