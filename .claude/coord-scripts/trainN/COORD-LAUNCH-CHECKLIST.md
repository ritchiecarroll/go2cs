# TRAIN N: COORD's launch checklist, from the seat table to master (DERIVED 2026-10-03 from TRAIN M's)

Git Bash on the i7. Every command matches `trainN/` as derived on 2026-10-03 (`tN-CHANGES.md`). State at the derive:
origin master = TRAIN M's landed head **8f46a9adae**; the draft seat list holds **27 rows** (cutoff 10:00 Central);
no tN worktree, no union branch, no `.battery.lock`; the follow list is EMPTY. One heavy job at a time on this box
(floors 1, 11). A reading that differs from an EXPECT line is a STOP. **Review round 1 (tN-CHANGES.md section 4):** the
draft does not assemble as cut (section 0, MS21), and the fixup stops at step 5 on the template-class csproj (MS20):
both are COORD's rulings before the commands they gate.

```
HND=/h/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainN
X=/h/go2cs-tmp-coord/coord-scratch/tN
R=$X/run1                                   # a FRESH run folder: the map, the assembly, the fixup AND the battery run from copies here
hp(){ python -B "$(cygpath -w "$R/tN-helpers.py")" "$@"; }
```

## 0. The seat table (seconds; nothing is created)

The derive's dry run (`coord-scratch/tN/derive/tableonly/table-only.log`, 02:36) read **ONE table problem** on the
27-row draft, and nothing else (every remote tip at its seated sha, no duplicate, no landed row, no patch under two
shas, c2-nugetgo-id-pattern's stack on r-module-license verified):

```
TABLE row 23 r-jwt-promoted-iface-repros: 'stack-on i9-crosspkg-promoted-forwarders' but a9cc98c1e3 is not an ancestor of ee52639557
ABORT: 1 seat-table problem(s) ...   rc=2
```

ee52639557 is cut on e2008427b1 (TRAIN M's first fixup), not on the i9's seat: the notes ask for ROW ORDER ("i9
forwarders, THEN claude/r-jwt-promoted-iface-repros", tL-seats-draft.txt:133), which the row order already gives. With
the note reading `after i9-crosspkg-promoted-forwarders` instead (a scratch variant, `seats-variant-after.txt`) the
table reads `TABLE OK: 27 rows ... rows-sha256=d62f0b3453e0` (the hash covers ref|sha only, so it is the real list's).
**COORD's ruling (tN-README.md MS1, section 7 question 1)**: edit row 23's note (file line N:25) in
`$HND/tN-seats-draft.txt`, or have R re-cut the repros on the i9's seat. **Review round 1: Q1 and H4 (MS2) are ONE
decision**: the `after` edit alone passes the table and leaves the fixup to die on H4; the reviewer recommends R's
re-cut on a9cc98c1e3 with committed emission and registrations (the stack-on note then TRUE), else row 23 waits for
TRAIN O.

**Review round 1: the draft does NOT assemble as cut (tN-README.md MS21, section 7 Q13 / Q14): route all four BEFORE the
map**, or the map prints `CONFLICT` for rows 9, 21 and 27 and the assembly refuses the list:
1. row 9 g-trim-annotations x row 4 c2-native-array-view: both append to the END of the BOARD (G re-cut without
   c2e7cef853 recommended);
2. row 21 p1-hashset-module x row 19 c2-multi-func-define: adjacent hunks in `src/go2cs/visitAssignStmt.go`;
3. row 27 p2-n-cleanup x row 17 r-module-driver-gomod-less: an insert beside a delete in `go2cs-src.projitems` (P2 re-cut
   on 537abad2a2, `stack-on r-module-driver-gomod-less`, recommended);
4. row 15 g-using-static-alias-guards x row 21: a CLEAN merge that does not compile (two unqualified `HashSet[string]{}`
   in `usingStaticNamespaceAlias_test.go`; one P1 re-cut on b9e2d5ffa1 + 1c182b5ca2 for items 2 and 4 recommended).
   MS19's scan (README) must then print nothing.
Also COORD's own table edits (Q17): the accepted `c2-toml-test-host|15838bfff5|...` row before p2-n-cleanup (MS18), and
row 22's stale `(owes go2cs.slnx)` note. Every edit moves `<n>` and `<h>`; read them from the lines below. Then:

```
[ ! -e $R ] && mkdir -p $R && cp -r $HND/. $R/
( cd $R && TABLE_ONLY=1 SIGN_PROBE=0 bash ./tN-assemble.sh ) | tail -n 3; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: <n> rows (tN-seats-draft.txt) rows-sha256=<h> ...   then   TABLE_ONLY=1: stopping ...   rc=0
grep -E '^[A-Za-z0-9._-]+\|' $R/tN-seats-draft.txt | tr -d '\r' | cut -d'|' -f1,2 | sha256sum | cut -c1-12   # = <h>
```
A row added, dropped, moved or re-pointed before the cutoff moves `<h>`: every later step reads the new hash.
**Hazard H4 (tN-README.md MS2) is COORD's before the map**: r-jwt-promoted-iface-repros adds three behavioral directories
with Go sources and no C#; precheck's H4 arm (the fixup's PRERES) REFUSES the union until the route is ruled.

## 1. The draft into hnd (the lanes read it from origin), the map, the assembly

```
ls -a $HND | grep -c __pycache__                                         # EXPECT 0 (a py_compile writes one; trainM tracks 0 .pyc: delete it, it is inside trainN)
git -C /h/go2cs-tmp-coord/hnd add -f -n .claude/coord-scripts/trainN  # DRY RUN (review round 1): read every path it would add; plain `status --porcelain` lists NOTHING under .claude/ (it is excluded)
#  EXPECT  only trainN/*.sh *.py *.md *.txt *.ps1 and controls/*; no __pycache__, no *.pyc, no tN-logs or run output
git -C /h/go2cs-tmp-coord/hnd add -f .claude/coord-scripts/trainN     # -f: .claude/ is in the clone's info/exclude (trainM's 27 files were force-added the same way)
git -C /h/go2cs-tmp-coord/hnd status --porcelain --ignored .claude/coord-scripts/trainN   # EXPECT the trainN files as 'A ' and no '!!' line
git -C /h/go2cs-tmp-coord/hnd commit -S -m "coord-scripts: TRAIN N script set (derived from TRAIN M's)"
git -C /h/go2cs-tmp-coord/hnd push origin claude/coord-handover          # announce, then push (floor 9)
( cd $R && SEATS=$R/tN-seats-draft.txt bash ./tN-conflict-map.sh ) | tail -n 4
#  EXPECT  MAP DONE head=<map10> rows=<n> rows-sha256=<h> (every row clean: kept as refs/coord/tN-map/rows-<h> ...)
#          -- only AFTER MS21's re-cuts (review round 1): on the draft as cut it prints SEAT 9 / 21 / 27 CONFLICT and
#          NOT-CLEAN rows=3, writes no rows ref, and the assembly below refuses the list. That reading is a STOP.
#  READ every 'shared paths' list of a clean row that is not a registration file (tN-README.md MS1)
( cd $R && bash ./tN-assemble.sh ) | tail -n 8
#  EXPECT  ASSEMBLED head=<asm10> seats=<n>; MAP TREE: EQUAL to <map10>; ANCESTRY; SHAPE: <n> first-parent merges; ORDER; TREE CLEAN; NEXT: ...
git -C /h/go2cs-tmp-coord/tN rev-parse --short=10 HEAD                   # <ASSEMBLED>
```
`grep -vE '^[[:space:]]*(#|$)' $R/tN-follow.txt | tr -d '\r' | sha256sum | cut -c1-12` reads `e3b0c44298fc` (EMPTY)
unless MS2's ruling is a follow-up merge (then: merge it signed --no-ff on the union, add its entry, re-read the hash).

## 2. Controls, once, before the fixup (floor 13; read-only except 2e, which aborts before creating anything)
```
hp teattr    "$(cygpath -w $R/controls/te-pos-goframe.patch)"      # EXPECT TE files=3 hunks=5 g-frame=4 (... g-frame-only files=2) map-only=1 other=0
hp hunkclass "$(cygpath -w $R/controls/te-pos-goframe.patch)"; echo rc=$?    # EXPECT HUNKCLASS-VERDICT files=3 hunks=5 other-files=0 ... rc=0
hp teattr    "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"   # EXPECT TE files=1 hunks=25 g-frame=0 (...) map-only=0 other=25
hp hunkclass "$(cygpath -w $R/controls/te-neg-i9-testing.patch)"; echo rc=$? # EXPECT ... other-files=1 ... rc=1
hp cnrexpect 'H:\Projects\go2cs' 8f46a9adae --base aa0a07d5fd | tail -n 2     # EXPECT CNRDELTA ... added=7 ...; CNREXPECT ... enumerated=791 skipped=6 n=785
# 2b. precheck on the assembled union (the fixup's PRERES, predicted)
hp precheck 'H:\go2cs-tmp-coord\tN' 8f46a9adae "$(cygpath -w $R/tN-seats-draft.txt)" head | grep -E '^(FAIL|NOTE|PRECHECK|ok +(H4|ROSTER execution))'; echo rc=${PIPESTATUS[0]}
#  EXPECT, while MS2 is unruled: rc=1 and ONE FAIL line, H4, 3 directories (PromotedIfaceFromLibEmbed,
#          PromotedIfaceFromLibEmbedLib, PromotedIfaceTestEmbed); 'ok ROSTER execution annotations: none (the ruled set ...)';
#          no NOTE (the roster's 'No row opts back out' reads 0, matching 0 annotated rows); and (review round 1) 'ok   REG
#          src/go2cs/go2cs-src.projitems: ... base-keys-lost=[] ... removed-key-back=[] seat-removed=[...HashSet.go ...]'
#          (M's arm FAILED here on p1-hashset-module's and p2-n-cleanup's intended removals).
#  EXPECT, once MS2's route is in the tree: rc=0, PRECHECK hard-failures=0 notes=0 mode=head.
# 2c. the same reader made to FAIL, on the base: a checkout whose HEAD is 8f46a9adae (a fresh child worktree; remove it after)
#  EXPECT  one 'FAIL BOTH <seat>: no first-parent merge' per row, FAIL COUNT / REG on the registration files the seats
#          add to, FAIL PROOF (log.slog.md is master's blob, not G's), FAIL ROSTER execution annotations: log/slog[release-tiered] (ruled: none)
# 2d. the LIVE gate
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock
hp live 'H:\go2cs-tmp-coord\coord-scratch\tN\.battery.lock' 1; echo rc=$?     # EXPECT one 'LIVE lock ...tZ...' line, rc=1
rmdir /h/go2cs-tmp-coord/coord-scratch/tZ/.battery.lock /h/go2cs-tmp-coord/coord-scratch/tZ
hp live 'H:\go2cs-tmp-coord\coord-scratch\tN\.battery.lock' 1; echo rc=$?     # EXPECT 'LIVE-VERDICT none ...', rc=0 (an idle box)
```
2e. the assembly's map refusal (safe only now, with every row merged and no fixup): a copy of the list without its LAST
row (p2-n-cleanup: no row stacks on it):
```
grep -v '^p2-n-cleanup|' $R/tN-seats-draft.txt > $R/c4-seats.txt
( cd $R && SIGN_PROBE=0 SEATS=$R/c4-seats.txt bash ./tN-assemble.sh ) | tail -n 2; echo rc=${PIPESTATUS[0]}
#  EXPECT  TABLE OK: <n-1> rows (c4-seats.txt) ...  then  ABORT: no map run is recorded for THIS list (...). ...   rc=2
git -C /h/go2cs-tmp-coord/tN rev-parse --short=10 HEAD                        # EXPECT <ASSEMBLED> still
```

## 3. The fixup (INFERRED about 1 h 30 m: the emission check is most of it; takes the lock)
Values:
- `EXPECT_HEAD=<ASSEMBLED>` (or the head after MS2's follow-up merge).
- No `PROSE_PATCH` (none is owed at N: MS12).
- `EXP_DESC`, `EXP_TH`: **TO BE SET BY COORD**, or unset (reported). Read-only derivation:
  `python "$(cygpath -w /h/go2cs-tmp-coord/tN/docs/phase4/recipes/quickjit-optin/optin.py)" check 'H:\go2cs-tmp-coord\tN' | tail -n 2`.
- Defaults: `REGEN=apply GOLDENS=regen SIBLINGS=report GOLDEN_CLASS=gframe CSPROJ_TEMPLATE=stop FIXUP_N=1`;
  `REGEN_ALLOW` composed (to NOTHING at the draft list); `EXEC_ROWS_EXPECT` from the helper (**0**). Optional:
  `SEATS_EXPECTED=<n>`. `CSPROJ_TEMPLATE=accept` only by COORD's ruling (MS20, Q15).
- MS19 (COORD's call): hashset v1.0.0 is ABSENT from the i7's module cache (exact paths read 2026-10-03, review round 1),
  so the fixup's first converter build fetches it from the proxy; to keep the legs off the network, warm it first:
  `( cd /h/go2cs-tmp-coord/tN/src/go2cs && go mod download ); git -C /h/go2cs-tmp-coord/tN status --porcelain | grep -vc '^??'` (EXPECT rc 0, then 0).
```
cd $R && EXPECT_HEAD=<ASSEMBLED> SEATS_EXPECTED=<n> bash ./tN-fixup.sh > fixup.console.log 2>&1; echo "fixup rc=$?" >> fixup.console.log
```
EXPECT, in order (read `$R/tN-fixup-logs/SUMMARY.txt`):
- `PRE head=<ASSEMBLED> seats=<n> ... EXEC_ROWS_EXPECT=0 PROSE_PATCH=none lists=rows:<h>/follow:<f>`
- `PRE derived (R1): REGEN_ALLOW=/^$/ :: composed from the seat list: NOTHING (no row measures a footprint and the G seat is not a row: every regenerated path is refused)`
- `PRERES precheck PRECHECK hard-failures=0 notes=0 mode=worktree`; the roster guard x2 with `0 with an execution config`
- `0b prose: none (no PROSE_PATCH given; none is owed at N: tN-README.md MS12)`; `1 S1: COUNT=0 (TRAIN M's master 0; N adds ...)`
- `4 regen`: if the emission check lists ANY union-attributable path the fixup STOPS (MS4): read the list; relaunch with
  `REGEN_ALLOW='<pattern>'` by COORD's explicit call (the evidence in `/h/go2cs-tmp-coord/tNemitfix` is re-used), or route
  the path back to its seat.
- `5 goldens PREDICTION: none ...`; a moved golden STOPS at its class (`GOLDEN_CLASS=gframe`): READ `gold-cs.patch`,
  then relaunch with `GOLDEN_CLASS=any` by COORD's explicit call (MS4b).
- (review round 1, MS20) `5 csproj CNR moved: 13 :: CSPROJTEMPLATE-VERDICT files=13 template-only=13 other=0 ...` at the
  draft list (14 with the toml row; NativeFieldPortAlias is linux-only), then a STOP unless the launch carried
  `CSPROJ_TEMPLATE=accept` (COORD's ruling, Q15), or none when a follow-up seat regenerated them first. `other` above 0
  dies whatever the switch: READ `gold-csproj-class.log`.
- last: `FIXUP DONE head=<10 hex> subject='fixup: TRAIN N' lists=rows:<h>/follow:<f>`, rc 0.

## 4. After the fixup: the signature by KEY, push, then GO (before the battery, so the lanes run beside it)
```
git -C /h/go2cs-tmp-coord/tN log -1 --format='%H %G? %GK %s' | cut -c1-140
#  EXPECT  <40 hex> <G or U> 941694536F21BAFF fixup: TRAIN N -- ...
#  (item N8: on this box %G? reads U for every COORD commit -- a good signature, local ownertrust unset, read on all 26
#   M union commits and on master, tL-seats-draft.txt:91 -- so the IDENTITY is the key: %GK must be 941694536F21BAFF,
#   and %G? may read G or U, never B, E, N, R, X or Y)
git -C /h/go2cs-tmp-coord/tN log --format='%G? %GK' 8f46a9adae..HEAD | sort | uniq -c   # EXPECT every line '<G|U> 941694536F21BAFF'
git -C /h/go2cs-tmp-coord/tN status --porcelain | grep -vc '^??'              # EXPECT 0
UNION=$(git -C /h/go2cs-tmp-coord/tN rev-parse HEAD)                          # the GO's UNION and FIXUP (one sha)
git -C /h/go2cs-tmp-coord/tN push origin claude/coord-trainN-union            # announce, then push; never replaced afterwards
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainN/tN-i9-shard.txt | sha256sum | cut -d' ' -f1   # EXPECT 057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d (M's and L's bytes)
git -C /h/go2cs-tmp-coord/hnd show HEAD:.claude/coord-scripts/trainN/tN-i9-shard.txt | grep -c .                   # EXPECT 132
git -C /h/go2cs-tmp-coord/hnd rev-parse --short=10 HEAD                       # the briefs' <HND> value
```
GO to the i9 (`tN-lane-brief-i9.md`): `UNION`, `FIXUP`, `EXPECT_LIST_SHA`, `EXPECT_ROWS=132`, the handover tip; FOUR files
in its run folder. GO to P1 and P2 (`tN-lane-brief-linux.md`): `UNION`, `FIXUP`, the handover tip; three files.

## 5. The battery (INFERRED 10 to 11 h, as tN-README.md step 6: M's run3 took 9 h 48 m, N adds PUB about 5 to 8 min, CC seconds, 12 projects in leg 5 about 30 min)
Preflight: `ls -d $X/.battery.lock` (absent); `ls $R/tN-logs $R/mod-logs` (absent); free disk at least 30 G on H: and
8 G on C:; nothing else converting or building on the box (the LIVE gate reads it; `LIVE_ACK=1` only by COORD's call).
Values: `EXPECT_HEAD` = the first 10 chars of `$UNION`. `CNR_EXPECT_N`: leave UNSET (PRE-D derives it: item N2); a
value given is COORD's explicit call and is stamped beside the derived one. `REALMOD_TEST_TIMEOUT`: leave at `2m`.
`I9_PATCH` when the i9's patch has arrived. Optional: `SEATS_EXPECTED=<n>`.
```
cd $R && EXPECT_HEAD=${UNION:0:10} SEATS_EXPECTED=<n> bash ./tN-battery.sh > battery.console.log 2>&1; echo "battery rc=$?" >> battery.console.log
```
EXPECT within the first minutes:
- `PRE union shape OK: first-parent=<n + follow-ups + 1> (= <n> seat merges + <k> follow-up merge(s) ... + 1 fixup commit(s) ...)`
- `PRE head=<10 hex> ... master=8f46a9adae seats=<n> (tN-seats-draft.txt) lists=rows:<h>/follow:<f> ...`
- `PRE-D CNR: EXPECT N=801 (derived (cnrexpect at HEAD)); platform-exclusive skips at HEAD [the six + NativeFieldPortAlias] (added AND skipped: [NativeFieldPortAlias]); the reader's control at 8f46a9adae: N=785 vs TRAIN M's run3 785; reconciliation: 785 + 16 measurable added - 0 removed = 801 ...`
  (the draft list: 17 package directories added, the H4 three among them, NativeFieldPortAlias linux-only: a different
  list moves these numbers, never by hand)
- `PRE-D behavioral: projects the union adds=[...] (12; slnx agrees: yes, the platform-exclusive excepted: [NativeFieldPortAlias])`
- `PRE-D execution configs (roster at 8f46a9adae vs HEAD): rows that keep an annotation (EXPECT tiered=True)=[] rows that drop it (EXPECT tiered=False)=[log/slog]; EXEC_ROWS_EXPECT=0`
- `PRE-1 precheck rc=0 :: PRECHECK hard-failures=0 notes=0 mode=head`
- later: `CC (EXPECT rc 0, 'ok', no FAIL): rc=0 ...`; `PUB environment: dotnet --version ... = 10.0.*`; `PUB (...): rc=0 :: published-output gate: 5 of 5 published programs match go, none hung`;
  `NGF-j0 (EXPECT nugetgo.github.com.google.uuid in all four ...)`; `G1: ... 0 with an execution config ...`
STOP conditions (the exit status): **2** an ABORT in PRE; **3** a leg's disk or build abort; **5** a wall cap fired
(PUB's 30m among them) OR a process is still running from `$R/out` after MOD: the lock is KEPT, kill the listed PIDs
by PID, `rmdir $X/.battery.lock`, relaunch from a FRESH folder; **9** the deadline; **6** at END = a leg outside FXc1 /
FXc2 / SIc / NVR is non-zero or a FINDING exists; **0** clean. A red needing a tree change after the push is a `fixup-2`
ON TOP (`FIXUP_N=2`), never a replaced sha.

## 6. Landing (tN-README.md MS11, then MS13), after the battery and the lane readings
1. Merge master into the union if it moved; `git diff --stat 8f46a9adae <master>`, read whole (MS11).
2. The landing precheck (`hp precheck <tN> <master> <run>/tN-logs/seats-effective.txt head`) and the roster guard x2 with
   `EXEC_ROWS_EXPECT=0`: BEFORE the bank step.
3. The bank step (as at M).
4. **MS13, the refresh of committed -tests sources** (a bank step, never a leg): the tree back at HEAD (the T legs'
   material is saved in `T-rewrites.patch`), then
   `hp testsrc-refresh 'H:\go2cs-tmp-coord\tN' --out <run>\tN-logs\testsrc-refresh.patch <run>\tN-logs\S-rewrites.patch <run>\tN-logs\T-rewrites.patch <the i9's tracked-changes.patch>`
   (EXPECT `REFRESH-VERDICT ... refused=0 conflicts=0`), `git apply --check` then `git apply` (add `--unidiff-zero` only
   if the verdict says so), `hp testsrc-refresh 'H:\go2cs-tmp-coord\tN' --check-worktree` (EXPECT rc 0, `ONLY committed
   test sources changed`), then ONE signed commit `refresh: TRAIN N -- committed -tests sources from the windows
   re-emission (MS13)`, its signature read by KEY as in section 4.
5. Fast-forward master to the union; the ledger line; the FLEET post.
