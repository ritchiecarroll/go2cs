# TRAIN P: conflict pre-map over TRAIN O's union (DRAFT 2026-10-04 12:15 CDT, UNCOMMITTED; COORD reviews and commits)

> **AMENDMENT 2026-10-04 (review round 1; dated: the text below is the 12:15 map and is left as written).**
> This file is the **v4 map**: 20 pushed rows + 3 slots numbered 1 to 23, '2 owe a re-cut', the go.go seat at
> c5cb51d3b6, the else-if seat at -r2 2753954c39, three stacks. **Read alone, it still says two rows conflict. They no
> longer do.** The table of record is the **22-row v5 list** (`tP-seats-draft.txt`, `rows-sha256=8491acbec194`, six
> stacks), and its readings are:
> - `premap-run2/v8-eb88` (14:01, step 3): `PREMAP-ALL DONE conflicted-rows=0 regcheck-of-record=ok
>   skip-head-parse-errors=0 union-head=f9a7b0b11b skip-head=f9a7b0b11b`, 22 steps, 104 both-changed pairs, 0
>   conflicting, one tree e31f01ef9b24 (`tP-DERIVE-REPORT.md` 3.2, `tP-README.md` section 8);
> - `premap-run3/v8-eb88-rr1` (review round 1, with the instruments as that round changed them): the same list, the
>   same head and tree; `premap-run3/controls` is the control run of the changed instruments (`tP-CHANGES.md` 8).
>
> What changed since v4: **row 11** `g-go-namespace-shadow-r2` 05b61c616a (re-cut on the sibling row: clean) and
> **row 22** `c2-elseif-position-record-r3` e00629855f (re-cut on fm-record-r3: clean) replace the two conflicting
> rows; **row 4** `g-xsync-bank` 51f5b68770 and **row 15** `i9-tests-host-crash-verdict` 3c9c2a74df were accepted and
> seated; **go-cmp L** was re-routed to LAST (as cut it conflicts with the else-if row in `reflect/package_info.cs`:
> ONE re-cut owed by C2, `claude/c2-reflect-newat-field-r2` stacked on e00629855f, ruled 14:05; it was pushed by 14:58
> as 92500eb260 and reads clean as row 23 in `premap-run3/trial-L-r2`, a trial list, not a seat table). This map's order and
> its three conflicts are all honoured by the table: go.go -r2 stacked on sibling -r2 with its `shadowing.md` section
> away from the sort seat's tail, the else-if -r3 stacked on fm-record-r3, and L x else-if routed by that ruling.
> Section 0's instrument notes hold with three changes made by the review: the git gate is 2.44 (was 2.38; D1 below),
> the duplicate-declaration census is a lexical scan (was a backtick parity that skipped 161 real declarations at
> eb88ab9492), and C5 plants that class (three unregistered files where the table below says two). D9's 'the
> production namespace of two packages' is narrower than the sibling row's rule: it also moves every third-party
> package named differently from its directory (`tP-README.md` Q6).

TRAIN P is the release train for go.* 1.24.13.4. Its BASE is TRAIN O's landed master, which does not exist yet
(O's battery bat2 is running). Every reading below merges onto O's union **eb88ab9492** (`claude/coord-trainO-union`,
read back by `ls-remote` seven times between 11:02 and 12:12 CDT) as an explicit stand-in. Scripts take BASE as a
variable and never as a literal.

**The answer in four lines.**

- **20 rows are pushed and in P** (section 1), G's sort seat and C1's arm on it among them. **18 merge clean in the
  proposed order; 2 owe a re-cut**, both already ruled by COORD while this map was measured: `g-go-namespace-shadow`
  (-r2 stacked on `c2-sibling-package-name-r2`; it conflicts with that row AND with G's own sort seat) and
  `c2-elseif-position-record` (-r3 stacked on `g-method-value-fm-record-r3`). The pushed
  `c2-elseif-position-record-r2` 2753954c39 (cut on eb88 alone) still conflicts with fm-record-r3 in 5 files and must
  not be seated.
- **Rows cut on 59ee0d21bf:** `c2-ide-spike`, `g-debugger-views` and (on a 59ee-based O row) `r-csharp-consumer-guide`
  **merge clean as they are**. The other three did not: `i9-incremental-cs-writes` and `c2-elseif-position-record`
  conflicted with O and are re-cut onto eb88 (the i9's -r2 7a5609dfc8 is clean in the chain; the else-if seat owes its
  -r3); `r-csharp-consumer-smoke` (stacked on the guide) conflicted with O and is replaced by COORD's -r2 86553e51b5,
  which is clean.
- **Every registration file auto-merges at every step**: 8 writers of go2cs.slnx and of each BehavioralTests list, 11 of
  go2cs-src.projitems; counts and keys agree at the chain head; 8 new behavioral projects, each registered once in all
  five files.
- **Nothing a build would see was measured.** The compile-level censuses read 0 (duplicate Go declarations, unqualified
  HashSet uses, Go parse errors) at the head of the list as it stands.

**What moved under the map (11:02 to 12:12).** Six refs were pushed and are measured here:
`claude/i9-incremental-cs-writes-r2` 7a5609dfc8, `claude/c2-elseif-position-record-r2` 2753954c39, G's sort seat
`claude/g-sort-self-capture` b719825826, `claude/c1-release-smoke-sort-arm` c49e2b7063 (one commit on the sort seat),
`claude/p2-test-warning-entries` 8d89695095 and COORD's `claude/r-csharp-consumer-smoke-r2` 86553e51b5. COORD ruled
(notes L:244-247; ledger d849c198ad and 1e3a978a6f): go.go re-cuts as -r2 on sibling-r2 after the sort push; else-if
re-cuts as -r3 on fm-record-r3; the i9's -r2 and the sort seat are accepted; the smoke seat first waited for the next
train (11:28) and then, on the owner's word (12:03), rides P as -r2. No acceptance line for p2-test-warning-entries
was read here. So there are two readings: **AS CUT** (step 1's 17 candidates, `premap-run1/ascut-eb88/`) and **v4**
(the 20 pushed rows as they stand at 12:05, `premap-run1/v4-eb88/`). v4 is the reading of record here.

**Hard rules kept.** No worktree, no checkout, no build, no conversion, no `go test`. Nothing was read from tO's or
tN's working tree. Nothing under trainO/, trainN/ or trainL/ was edited. Four refs were fetched, each with
`git -C /h/Projects/go2cs fetch origin <ref>` (the other two were already in the store). No push, no post, nothing
committed.

**One thing COORD should know about the shared object store.** The first runs (11:05 to 11:28) wrote merged trees,
blobs and dangling chain commits into `H:/Projects/go2cs/.git/objects`, as tO-premap.sh did and as this step's task
prescribed. That took the store past `gc.auto`'s default 6,700 loose objects: 7,095 at 11:42 and still at 12:12,
against about 5,633 before (at most 1,462 are this step's; the window also holds two fetched refs' objects).
Unreachable objects cannot be packed away, so **a fetch, commit or merge in any worktree of this repository now runs
`gc --auto` and prints "too many unreachable loose objects; run 'git prune'"** until they age out (gc.pruneExpire) or
COORD prunes. This step's own fetches at 11:26 and 12:05 printed it; COORD's 11:53 fetch went through it (the count
read 7,131 and was back at 7,095 inside the minute). git's gc is safe beside running git commands; whether a battery
leg triggers it was not checked. From 11:29 on tP-premap.sh writes to a SCRATCH object directory (section 0) and the
count has not moved. The remedy is COORD's call after O lands: leave it, set `gc.auto 0` for the landing, or
`git prune --expire=<date>` (which also removes tO's and step 1's dangling map objects).

Files (trainP/, all uncommitted):

- `tP-premap.sh` the instrument (alone, pairs, chain, replay). `tP-premap-all.sh` the whole reading in one command.
  `tP-premap-controls.sh` the controls.
- `tP-regcheck.py` counts and keys of the registration files and of every content-merged path. `tP-unioncheck.py`
  the censuses a clean merge cannot give. `tP-gofmt-parse.sh` a parse-only read of the merged Go files.
  `tP-footprints.sh` own footprints and writers. `tP-hunks.sh` the conflict regions of one merge.
- `premap-run1/`: `v4-eb88/` and `ascut-eb88/` (each: alone.log, chain-union / chain-skip .log and .tsv,
  regcheck.log, unioncheck.log, gofmt.log, pairs.log and .tsv, own-footprints.tsv, writers.tsv,
  shared-hunk-positions.log, objects/), `controls/`, `conflict-hunks.log`, `not-p-alone.log`, the row lists
  (`rows-order-v4.txt`, `rows-order-v1.txt`, `rows-as-cut.txt`, `rows-not-p.txt`), seven `lsremote-*.txt` readings,
  `elseif-r2-corpus-files.txt`, `gofmt-parse-refined-union-carry.log`.

## 0. Instrument and controls

The box's default git is **2.35.2** and has no `merge-tree --write-tree`. Visual Studio 2022 bundles **git
2.55.0.windows.5** (`.../Team Explorer/Git/cmd/git.exe`), which does. `tP-premap.sh` runs every merge through it
(`GITX`): `git merge-tree --write-tree --messages <ours> <theirs>`, which is git's own merge-ort. Against tO-premap.sh
(a temp-index `read-tree` plus `merge-file`) that changes three things:

- **Criss-cross bases are ort's**, a recursive virtual base, not the first base. This is COORD's 03:15 derive item:
  tO-premap.sh read a false README conflict on c2-nuget-followups-r2.
- **Renames are detected.** c2-sibling-package-name-r2 carries 6.
- **OUT takes either path style.**

Three modes, as at O, plus `replay`:

- `alone`: each row onto BASE.
- `pairs`: every pair of rows, **each first put on BASE** (its alone merge), then merged with the other. What remains is
  seat versus seat; a row's own conflict with O is factored out instead of repeating in every pair (step 1's
  tip-versus-tip pairs read 31 conflicts, 29 of them O-carried).
- `chain`: the cumulative merge in row order. Each step is committed as a dangling commit with both parents, at a
  FIXED date, so a re-run reproduces the same commit shas and not only the same trees.

`ONCONFLICT=union` carries a conflicted path forward as `git merge-file --union --diff3` of the three stages ort
reports, so later rows meet both sides. **The `--diff3` matters and is a finding for the go.go re-cut** (section 5,
R1): the default (refined) union moves a line both regions share out of the conflict and keeps it once. On
`importAliasOperations.go` that line is a closing brace: the refined union is 502 lines and does not parse (15 gofmt
errors); the unrefined one is 504 = 473 + 19 + 12 and parses. A carried tree is never a ruled resolution.
`ONCONFLICT=skip` leaves the conflicted row out.

Objects go to `OUT/objects` (scratch; the shared store is read through `GIT_ALTERNATE_OBJECT_DIRECTORIES`) and are
packed at the end. To read a chain commit or tree:

```
GIT_ALTERNATE_OBJECT_DIRECTORIES=H:/go2cs-tmp-coord/hnd/.claude/coord-scripts/trainP/premap-run1/v4-eb88/objects \
  git -C /h/Projects/go2cs show a8714c5b1d:src/go2cs/convCallExpr.go
```

The as-cut chain's commits (6127f41aab, 2b19e2ec07) are in the shared store from the first runs and read with plain git.

Stated limits: a merge that compiles nowhere is invisible; a union-carried path is not a resolution; the sequential
`git merge` rehearsal at the real BASE stays the reading of record.

Controls (`OUT=<dir> bash tP-premap-controls.sh`, run 12:04 with the final scripts, `premap-run1/controls/`, 22 control
lines, failed=0):

| Control | Reading |
|---|---|
| **C1 replay (identity)**: O's 38 first-parent merges 54f7f4439d..eb88ab9492, each re-merged from its two real parents | **38 of 38 trees identical** to git's own merge commits, the criss-cross row c2-nuget-followups-r2 (2 bases) among them |
| **C2 chain**: O's 38 seats replayed as a chain from 54f7f4439d | head tree 65b092d15c8c = the real assembled head 6f3fea87db's; 0 conflicted steps |
| **C3 regcheck** at that head, and at the bare base | ok x6 (one multi-base row diffed against its merged bases; O's D1 identical insert credited in 3 files); FAIL x6 at the base |
| **C4 conflicts** known from O's map and step 1 | p1-hashset-module a7a7a197dd, A2 0c4397a069 and g-method-value-names d49be54e11 onto 59ee; p2-cli 338ef4a2af and i9 d2e09a681c onto eb88; go.go onto sibling-r2: each reads CONFLICT on its named path |
| **C5 unioncheck** on a planted commit (a twin Go file, an unqualified NewHashSet, a Go-only behavioral directory, a marker file) and on head = base | 5 planted arms fire by name (2 unregistered files, 8 duplicate declarations, H4 1, HashSet 1, markers 1); 4 negative arms read 0 |
| **C6 gofmt** on sibling-r2 x go.go | refined union: 15 errors; unrefined union: 0; ort's marked tree: does not parse |
| **Cross-instrument** | the four rows not cut on eb88 give the SAME alone trees as step 1's read-tree instrument: 59f62b6aac65, 7d8df7210319, 077014dcda6c (also G's own reading), 077cff8f769a |

## 1. The rows: tips, cut, and each row alone onto O's union

Tips read by `ls-remote` at 12:05 and 12:12 CDT (`lsremote-1205.txt`, `lsremote-1212.txt`): every sha below is its
remote tip. `cut` is the merge base with eb88ab9492. `own` is the row's own footprint (files, +/-), against its stack
where it has one.

| ref | sha | cut | commits | own | alone onto eb88ab9492 |
|---|---|---|---|---|---|
| c2-ide-spike | a4bfed39f5 | **59ee** | 4 | 34, +2205/-0 | **clean as cut** (no both-changed path; tree 59f62b6aac65) |
| r-csharp-consumer-guide | 1c608d4c37 | 84fcc6373d (an O row's parent, on 59ee) | 1 | 2, +167/-0 | **clean as cut** (docs/README.md content-merged; tree 7d8df7210319) |
| **r-csharp-consumer-smoke-r2** | **86553e51b5** | on the guide (COORD's re-cut: d03d96a285 + 1 commit) | 3 | 9, +425/-2 | clean (3 content merges with O: docs/README.md, go.lib.targets, PackageTests/README.md; tree 6857df60e77f) |
| g-debugger-views | 5ec7b56a46 | **59ee** | 3 | 21, +407/-9 | **clean as cut** (StructTypeTemplate.cs content-merged with O's B6; tree 077014dcda6c) |
| **g-sort-self-capture** (THE RELEASE FIX) | **b719825826** | eb88 | 3 | 17, +543/-3 | clean (descends; tree e448a5a045c9). P ACCEPTED 12:03 |
| **c1-release-smoke-sort-arm** | **c49e2b7063** | on the sort seat | 1 | 3, +25/-3 | clean (descends; tree edb6e26001fb). Its acceptance waits on the reading of its green run 37218568547 (COORD 12:03) |
| p2-cli-version-diagnostics-r2 | cc43ba79e4 | eb88 | 5 | 9, +496/-14 | clean (descends; tree a218742dc1b2) |
| p2-untyped-region-nil-safety-r2 | 5d303cbd1a | eb88, on the row above | 2 | 4, +325/-0 | clean (descends; tree 0198217259a8) |
| c2-sibling-package-name-r2 | 6f54f28be2 | eb88 | 12 (1 internal merge, one base) | 77, +1459/-107, **6 renames** | clean (descends; tree b7c6404b6d5b) |
| g-go-namespace-shadow | c5cb51d3b6 | eb88 | 2 | 27, +942/-0 | clean alone (tree 5a6ca3e02736); **CONFLICT with sibling-r2 and with the sort seat** |
| c2-alias-table-same-name | bfdb6709ac | eb88 | 2 | 32, +1144/-2 | clean (descends; tree 56d4d2b18c72) |
| c2-embed-promoted-refs | 2fa2ccd83a | eb88 | 2 | 4, +187/-0 | clean (descends; tree 9ca7c89aac68) |
| **i9-incremental-cs-writes-r2** | **7a5609dfc8** | eb88 | 6, signed | 11, +569/-36 | clean (descends; tree fe4589b13df3). P ACCEPTED 11:30 |
| c2-lifted-iface-test-cast | a655ced9e1 | eb88 | 2 | 3, +80/-2 | clean (descends; tree 194cb62e0418) |
| c2-named-basic-conv | 22972a9349 | eb88 | 2 | 15, +515/-0 | clean (descends; tree 5b59f2d56309) |
| c2-anon-struct-named-conv | 87fe8f16a6 | eb88 | 3 | 23, +919/-3 | clean (descends; tree cf79cb41714d) |
| c2-literal-float-fold | 00f11c949b | cca633f1ba (an O row) | 3 | 19, +634/-4 | **clean as cut** (8 content merges: slnx, projitems, the four lists, convBinaryExpr.go, visitorState.go; tree 077cff8f769a) |
| **p2-test-warning-entries** | **8d89695095** | eb88 | 2 | 3, +45/-14 | clean (descends; tree 92bf45ebf9c1). Pushed by 12:05; no acceptance line read here |
| g-method-value-fm-record-r3 | 07b53bc99d | eb88 | 5 | 42, +810/-42 | clean (descends; tree a6c586d0ecd3) |
| **c2-elseif-position-record-r2** | **2753954c39** | eb88 (NOT on fm-record-r3) | 3 | 130, +557/-402 | clean alone (tree 539f10095a7e); **CONFLICT with fm-record-r3** in 5 files. Superseded by the ruled -r3 |
| (as cut) i9-incremental-cs-writes | d2e09a681c | **59ee** | 6 | 11, +568/-36 | **CONFLICT** conversionDriver.go, testConversion.go (1 hunk each, vs O's p2 warning entries); replaced by -r2 |
| (as cut) c2-elseif-position-record | 2265dcdb6e | **59ee** | 2 | 130, +557/-402 | **CONFLICT** encoding/json/package_info.cs (vs O's p2); replaced by -r2, then -r3 |
| (as cut) r-csharp-consumer-smoke | d03d96a285 | on the guide | 2 | 9, +425/-2 | **CONFLICT** src/tests/PackageTests/README.md (1 hunk, append-append vs O); replaced by -r2 |

No row has several best common ancestors with the base or with an earlier row (`own-footprints.tsv`, column 5: all 1).
The one internal merge (sibling-r2's 39b01cf813, the design note f4d823e5f9) leaves one base. Stacks, by ancestry,
three and no other: smoke-r2 on the guide; C1's arm on the sort seat; p2-untyped-region-nil-safety-r2 on
p2-cli-version-diagnostics-r2.

The refs that are not rows, for the record (`not-p-alone.log`): p2-cli-version-diagnostics 338ef4a2af and
p2-untyped-region-nil-safety b4d7ff1cf9 conflict in docs/README.md; c2-sibling-package-name d545684a89 in
testConversion.go; g-method-value-fm-record 1690d58503 in 10 paths; g-method-value-fm-record-r2 240512b217 (two
bases) and C1's control 1d68d5dd55 are clean.

## 2. Seat versus seat (`v4-eb88/pairs.log`, each row first put on BASE)

190 pairs; 3 are ancestry (the three stacks). **88 pairs have a both-changed path. 4 read CONFLICT, which is 3
conflicts** (C1's arm descends from the sort seat and repeats its reading). All three are real, and all are ruled; two
of them are one row's (go.go):

| pair | path | what collides |
|---|---|---|
| c2-sibling-package-name-r2 x g-go-namespace-shadow | src/go2cs/importAliasOperations.go, 1 hunk | Both insert an `if` block at ONE point of `computeImportAliasRenames`' closure loop, below base line 170 (sibling +7 there, go.go +12). The two blocks share their closing-brace line (section 5, R1) |
| g-sort-self-capture (and C1's arm on it) x g-go-namespace-shadow | docs/ConversionStrategies-Reference/shadowing.md, 1 hunk | G's two seats each insert a `##` section at the SAME point, below base line 729, above the file's closing `---` (+27 and +33) |
| g-method-value-fm-record-r3 x c2-elseif-position-record-r2 | src/core/{bytes, crypto/tls, encoding/json, internal/buildcfg, strings}/package_info.cs, 1 hunk each (crypto/tls: 2 marked regions) | Both rewrite the SAME `GoPositionMap` line (encoding/json/encode.go; strings/strings.go; crypto/tls handshake_client.go, handshake_client_tls13.go and handshake_server_tls13.go) or ADJACENT lines (bytes: G's bytes.go sits between C2's buffer.go and iter.go; internal/buildcfg: exp.go beside cfg.go). Emission, never hand-merged |

fm-record and else-if share four more files (database/sql and runtime darwin, linux, windows `package_info.cs`); those
content-merge clean. The regions are in `premap-run1/conflict-hunks.log`.

**The sort seat is clean against every other row** but G's own go.go: convCallExpr.go with named-basic (H2) and with
anon-struct (H1); shadowing.md with embed; the registration files with six rows.

**The three rows pushed last are clean against everything:** smoke-r2 with C1's arm in
src/tests/PackageTests/README.md (the -r2's section sits at old line 33, the arm's edit at 67) and with the two P2 rows
in docs/README.md (through the guide's line); p2-test-warning-entries shares no path with any row.

Every other pair that touches one file merges clean (`pairs.tsv`, by path; a stacked row repeats its base's readings):
go2cs-src.projitems 54 pairs; go2cs.slnx and each of the four lists 35 pairs; conversionDriver.go 5; convCallExpr.go 5;
docs/README.md 4; testConversion.go 3; visitImportSpec.go 2; shadowing.md 3 of its 5; importOperations.go,
packageInfoWriter.go, visitorState.go, PackageTests/README.md 1 each. Section 4 says what to read in them.

As cut (`ascut-eb88/pairs.log`, no sort seat): 71 pairs with a both-changed path, the first and third conflicts above.
The as-cut i9 row and the as-cut smoke row conflict with O alone and with no other row.

## 3. Proposed order and the cumulative chains

**The proposed order (v4).** The row number is the merge order. Slots have no ref and were not in the chain; their
place is the proposal. "chain" is the step in `v4-eb88/chain-union.log`.

| row | ref | sha | cut | stack-on | chain | route |
|---|---|---|---|---|---|---|
| 1 | c2-ide-spike | a4bfed39f5 | 59ee | | 1 clean | as is (docs) |
| 2 | r-csharp-consumer-guide | 1c608d4c37 | 84fcc6373d | | 2 clean | as is (docs) |
| 3 | r-csharp-consumer-smoke-r2 | 86553e51b5 | on row 2 | row 2 | 3 clean | as is (GOLIB: a PACKED go.lib file; S1_NONE owes ConsumerUsings.csproj) |
| 4 | (slot) COORD's P BOARD docs row | none | BASE | | | docs: G's 11:35 observability correction (and the x/sync bank block, if COORD folds it in). No pushed row touches the BOARD, the roster or a proof page |
| 5 | g-debugger-views | 5ec7b56a46 | 59ee | | 4 clean | as is (GOLIB + GEN; before every converter row) |
| 6 | **g-sort-self-capture** | b719825826 | eb88 | | 5 clean | as is: **the first converter row**, so the release fix is never the row that re-cuts |
| 7 | c1-release-smoke-sort-arm | c49e2b7063 | on row 6 | row 6 | 6 clean | as is |
| 8 | p2-cli-version-diagnostics-r2 | cc43ba79e4 | eb88 | | 7 clean | as is |
| 9 | p2-untyped-region-nil-safety-r2 | 5d303cbd1a | eb88 | row 8 | 8 clean | as is |
| 10 | c2-sibling-package-name-r2 | 6f54f28be2 | eb88 | | 9 clean | as is |
| 11 | g-go-namespace-shadow | **G's -r2 owed** (as cut c5cb51d3b6) | on row 10 | row 10 | 10 CONFLICT as cut (2 files) | R1 (ruled 11:30 and 11:35) |
| 12 | c2-alias-table-same-name | bfdb6709ac | eb88 | | 11 clean | as is |
| 13 | c2-embed-promoted-refs | 2fa2ccd83a | eb88 | | 12 clean | as is |
| 14 | i9-incremental-cs-writes-r2 | 7a5609dfc8 | eb88 | | 13 clean | as is (R3) |
| 15 | (slot) i9-tests-host-crash-verdict | none | on row 14 | row 14 | | GO 11:30, P only if gated by the freeze |
| 16 | c2-lifted-iface-test-cast | a655ced9e1 | eb88 | | 14 clean | as is |
| 17 | c2-named-basic-conv | 22972a9349 | eb88 | | 15 clean | as is |
| 18 | c2-anon-struct-named-conv | 87fe8f16a6 | eb88 | | 16 clean | as is (GEN) |
| 19 | (slot) C2's go-cmp L | none | eb88 | | | ruled GO 10:17 with its conditions |
| 20 | c2-literal-float-fold | 00f11c949b | cca633f1ba | | 17 clean | as is (corpus 1 file) |
| 21 | p2-test-warning-entries | 8d89695095 | eb88 | | 18 clean | as is once accepted (2 `.editorconfig` + the guard script) |
| 22 | g-method-value-fm-record-r3 | 07b53bc99d | eb88 | | 19 clean | as is (corpus 11 files + golib + a hand-own) |
| 23 | c2-elseif-position-record | **C2's -r3 owed** (pushed -r2 2753954c39) | on row 22 | row 22 | 20 CONFLICT as pushed | R2 (ruled 11:28) |

Why this order:

- **Docs and the consumer seat first**, then the golib and gen seat, so the union gates of the golib rows (2b, GT, GN)
  read before any converter row moves.
- **The sort fix is the first converter row**, its release-smoke arm right under it. It is clean against all other
  rows except G's own go.go seat, which re-cuts anyway. Seated first, any later surprise is another row's re-cut and
  never the release fix's.
- **Hot files are adjacent.** Rows 8 to 14 are the seven rows that write conversionDriver.go, visitImportSpec.go,
  importAliasOperations.go, importOperations.go, testConversion.go and packageInfoWriter.go. Rows 17 and 18 share
  convCallExpr.go with row 6. Rows 20 and 22 share visitorState.go.
- **Each re-cut sits directly below its one parent** (rows 10 and 11; rows 22 and 23): single-parent stacks, one
  `stack-on` token each, no multi-base row.
- **Corpus movers last** (rows 20 to 23), the position-map pair at the very end, so the fixup's regeneration reads
  them after every converter row. (The sort seat's corpus is 3 lines of sort/sort.cs and no position-map line.)

The chains, all on eb88ab9492:

| chain | rows | conflicted steps | head (commit, tree) |
|---|---|---|---|
| **v4, union carry** (`v4-eb88/chain-union.log`) | 20 as pushed | **2**: step 10 g-go-namespace-shadow (importAliasOperations.go AND shadowing.md); step 20 c2-elseif-position-record-r2 (5 package_info.cs) | a8714c5b1d76, 9237e4a723e6 |
| **v4, skip** (what the assembler would hold without the two) | 20 | the same 2, left out; **18 rows clean** | cdb79754a4b4, 4d31f266e3a1 |
| as cut, union carry (`ascut-eb88/`) | 17 (step 1's) | 4: smoke as cut (PackageTests/README.md); go.go (importAliasOperations.go); i9 as cut (conversionDriver.go, testConversion.go); else-if as cut (the 5 files) | 6127f41aabc9, 634ede17a762 |
| as cut, skip | 17 | the same 4; 13 rows clean | 2b19e2ec07d3, 822217ce390e |

In every chain each conflicted step is exactly one row's own conflict; no clean row turns conflicted later, and no
conflict appears that the pairs did not show. **Prediction:** the 20 rows with G's -r2 and C2's -r3 in place of rows
11 and 23 assemble with 0 conflicts. It is a prediction: both re-cuts are unpushed, the go.go -r2 moves its
shadowing.md section as well, and three slots have no ref. Re-run `tP-premap-all.sh` with each new sha (five minutes).

## 4. Every pair touching the same file: what git will not mark

**Registration files** (`v4-eb88/regcheck.log`, union-carry head a8714c5b1d; the skip head reads ok too):

| file | writers | base -> head | reading |
|---|---|---|---|
| src/go2cs.slnx | 8: sort (+1), sibling-r2 (+4/-1), go.go (+3), alias-table (+3), named-basic (+1), anon-struct (+2), float-fold (+1), fm-record-r3 (+1) | 1111 -> 1126 | ok: expected 1126, missing 0, dup 0 |
| src/go2cs/go2cs-src.projitems | 11: p2-cli (+3), p2-untyped (+2), float-fold (+2), i9-r2 (+2), and +1 each from sibling-r2, alias-table, embed, lifted-iface, named-basic, anon-struct, else-if. The sort seat adds no file | 492 -> 508 | ok |
| CompileTests.cs, OutputComparisonTests.cs, TargetComparisonTests.cs, TranspileTests.cs | 8 each, +3 lines per project | +24 each | ok x4 |

- No step conflicts in a registration file: no two rows insert at one point.
- sibling-r2's `-1` is the slnx key of `AliasNamespaceShadow/sortlocal`, which it renames to `sort/` (6 renames, read
  by ort). Precheck's REG arm must read it as seat-removed (section 6, D3).
- `tP-unioncheck.py`, arm A: all 426 top-level Go files of src/go2cs have exactly one projitems entry (410 at the
  base), and no entry lacks its file.
- Arm B: **8 behavioral projects are added** (AnonStructNamedConversion, GoHostModuleShadow, LiteralFloatConstFold,
  MethodValueFmRecord, NamedBasicConversion, SameNameImportAlias, SiblingPackageNames, SortMethodSelfCapture), each in
  slnx once and in each list once, each with its `.cs` and `.cs.target`; H4 (a Go-only directory) reads 0.
  **LiteralFloatConstFold.csproj is the one pre-N template** (no TrimMode line; the other seven hold it): the
  CSPROJ_TEMPLATE class is 1 file, confirmed accept at 11:28.

**Shared converter, test and docs files that merge clean** (`shared-hunk-positions.log`; counts are regcheck's, base ->
head, each equal to the base plus every writer's own net, with every added line present and none duplicated):

- **convCallExpr.go, 3 writers, 6978 -> 7144.** The sort seat (+10 in `convCallExpr` at old 2363, and its new
  `sameNameMethodCapture` at the file's tail, +73), named-basic H2 (+27 in `convCallExpr` at 1271), anon-struct H1
  (+59/-3 in `applyImplicitConversion` at 3819 to 3850 and a new function at 4014). Three separate regions; two rows
  edit the SAME function (`convCallExpr`), 1,092 lines apart. H1 was not cut on H2, and neither on the sort seat.
  **Read it whole at the union: it is the release fix's file.**
- **conversionDriver.go, 4 writers, 835 -> 878.** All four edit `processConversion`: p2-cli-r2 (the diagnostics at
  old 227 and 282, plus `refuseSelectedCgoSources` and `importsC`), p2-untyped-r2 (+2 at 232), sibling-r2 (+1 at 287,
  five lines under p2-cli's 8-line insert), i9-r2 (+3 at 107, +4 at 644, right under O's `updateWarningEntries`).
- **testConversion.go, 3 writers, 10004 -> 10097.** sibling-r2 (+1 in `processTestConversion`), embed (+79 in four
  closure functions at 5329 to 5641), i9-r2 (+13 in `convertTestVariants`). Different functions.
- **visitImportSpec.go, 2 writers.** p2-cli-r2 edits `visitImportSpec` (268); sibling-r2 rewrites
  `convertImportPathToNamespace` and `packageClassPath` and adds `packageKeepsDirectorySegment` (981 to 1027).
- **importOperations.go, 2 writers.** sibling-r2 (+3 at 576, 593, 648) and alias-table (+14/-1 in
  `applyExportedTypeAliases`, 1058).
- **packageInfoWriter.go, 2 writers.** Both edit `writePackageInfoFile`: sibling-r2 at 251 (and +37 at 1098), i9-r2
  at 900 to 912.
- **visitorState.go, 2 writers.** float-fold (+5 at 191) and fm-record-r3 (+7/-3 at 390): Visitor fields.
- **docs/ConversionStrategies-Reference/shadowing.md, 3 writers.** embed (+16 at 405) merges clean with both others.
  The sort seat (+27) and go.go (+33) both insert at 729: the conflict of section 2.
- **src/tests/PackageTests/README.md, 2 writers, 72 -> 97.** smoke-r2 (+20 at old 33, before RidCompileAsset) and
  C1's arm (+6/-1 at 67, inside O's release-smoke section).
- **docs/README.md, 2 writers.** guide (+1 at 41), p2-cli-r2 (+21 at 226 and 232).
- **Against O alone (one P writer):** go.lib.targets (smoke-r2 +16 at 73; O's c1-macos-flavors +2 at 17; 77 -> 93);
  StructTypeTemplate.cs (debugger-views, 2 lines at 108 and 1725; O's B6 at 25 to 156); convBinaryExpr.go (float-fold
  at 108 and 1381; O at 1736 to 2277).
- **One P writer, nothing merges:** projectFileWriter.go (i9-r2, `writeOutputFile`, 767 to 778; the -r2 is cut on
  eb88 and holds O's edit at 440); convExpr.go and convExprList.go (the sort seat); release-smoke.ps1 and CIMatrix.md
  (C1's arm); check-warning-entries.sh and the two `.editorconfig` (p2-test-warning-entries).

**Semantic pairs no gate has run together** (clean merges; the union's behavioral suite and re-reads are the gate):

- **sort x H2 x H1** in convCallExpr.go. The sort seat casts an argument at call sites; H2 and H1 rewrite how
  conversions and implicit-conversion arguments render. Gates: SortMethodSelfCapture, NamedBasicConversion,
  AnonStructNamedConversion; the sort and pflag / cobra re-reads; go-cmp; **C1's release-smoke arm B on four RIDs**.
- **sibling-r2 x go.go x alias-table**, the import and namespace machinery. go.go's new block calls
  `convertImportPathToNamespace`, whose result sibling-r2 changes (same signature; sibling appends a tail segment,
  go.go tests the head). Gates: SiblingPackageNames, GoHostModuleShadow, SameNameImportAlias, CrossPkgSameNameAlias,
  AliasNamespaceShadow; the go-cmp, testify, logrus, cobra and yaml.v3 re-reads.
- **p2-cli-r2 x p2-untyped-r2 x sibling-r2 x i9-r2** in processConversion: the converter suite and a `-tests` run.
- **g-debugger-views x anon-struct** in src/gen (10 files and ImplicitConvGenerator.cs, no shared file), over O's
  four gen rows: 2b, GN, GT, leg 5, H7.

**Corpus (emission) and goldens:**

| row | committed corpus | shared with another row |
|---|---|---|
| sort | sort/sort.cs, 3 lines (130, 160, 179) | none |
| fm-record-r3 | 11 `package_info.cs` + hand-own runtime/managed_impl.cs + golib GoPositionMapAttribute.cs | 9 with else-if (5 conflict) |
| else-if (-r2 as pushed) | 123 `package_info.cs`, +398/-398, the SAME 123 paths as the original (`elseif-r2-corpus-files.txt`); 4 behavioral `package_info.cs` | the 9 above; none with sibling-r2, float-fold or sort (sort/package_info.cs is not among the 123) |
| sibling-r2 | 30 src/core files: 4 production `.cs`, 2 production csproj, 24 committed -tests files (fips140deps, fips140test, runtime/internal/wasitest) | none |
| float-fold | internal/trace/traceviewer/mmu.cs | none |
| p2-test-warning-entries | math/bits/.editorconfig, weak/.editorconfig (new files) | none |

- Arm F: across all rows every changed line of a src/core `package_info.cs` is a `GoPositionMap` line, except
  sibling-r2's 4 `using` and 4 `namespace` lines. `stdlib-metadata.txt` moves in no row.
- None of the 8 new projects' Go sources holds an `else if`, so the else-if seat moves no golden born on another row.
  Whether fm-record's record or the sort seat's cast moves one (a value-receiver method value, or a same-name call, in
  a project born on another row) is the union CNR's to say.
- No two rows touch one existing behavioral project's files (writers.tsv holds no behavioral path but the lists).

**Compile-level censuses at the heads** (`unioncheck.log`, `gofmt.log`):

| census | v4 union head | v4 skip head | as cut |
|---|---|---|---|
| duplicate top-level Go declarations new at head (package main) | 0 | 0 | 0 |
| unqualified `HashSet[` / `NewHashSet` in added lines (MS19) | **0** | 0 | **2**, both in the i9's as-cut `incrementalWrites_test.go`; the -r2 writes `hashset.NewHashSet` |
| conflict markers in changed files | 0 of 414 | 0 of 274 | 0 |
| gofmt parse errors, Go files changed under src/go2cs | 0 of 44 | 0 of 42 | 0 |

## 5. Routes, one per conflict

- **R1 g-go-namespace-shadow (RULED 11:30 and 11:35: G's -r2 stacked on c2-sibling-package-name-r2 6f54f28be2, cut
  after the sort push; its shadowing.md section at a non-tail anchor).** One re-cut clears both of its conflicts. It
  is the cheapest side: 2 commits and 12 converter lines against C2's 12 commits, 77 files and 30 regenerated corpus
  files; and the sort seat, the release fix with C1's arm stacked on it, stays as pushed. What the re-cut must show:
  - **importAliasOperations.go: keep both blocks, each with its OWN closing brace.** The two regions share that line,
    and a refined union or a "take both" in an editor keeps it once. The arithmetic: 492 lines at 6f54f28be2,
    **504** after the re-cut (base 473 + 19 + 12). Symbol counts in the merged file: `packageKeepsDirectorySegment(tail`
    1, `RootNamespace+"."+RootNamespace` 2. `gofmt -e` 0 errors. A build.
  - **shadowing.md: the section away from base line 729** (the sort seat's anchor, the file's tail) **and away from
    line 405** (embed's). A single-parent -r2 on sibling-r2 does not hold the sort seat's section, so only the
    placement keeps the three writers apart.
  - Its notes declare `stack-on c2-sibling-package-name-r2`. Its gates read at the stacked tip, where sibling's
    namespace rule and the go.go block meet for the first time (GoHostModuleShadow and SiblingPackageNames together).
  - Then re-run the chain: this map's step 10 must read clean, regcheck 504, and shadowing.md 733 + 27 + 33 + 16 = 809.
- **R2 c2-elseif-position-record x g-method-value-fm-record-r3 (RULED 11:28: C2's -r3 stacked on 07b53bc99d).** The
  pushed -r2 2753954c39 was cut on eb88 alone and conflicts in bytes, crypto/tls, encoding/json, internal/buildcfg and
  strings `package_info.cs`, whatever the order. The -r3's two seeded arms stand on G's tip, so the shared lines come
  from a converter carrying both changes and the row descends from row 22: clean by construction. EXPECT the same
  123 paths. Its notes declare `stack-on g-method-value-fm-record-r3`. The table must not seat 2753954c39.
- **R3 i9-incremental-cs-writes (DONE: -r2 7a5609dfc8, P ACCEPTED 11:30).** As cut it conflicted with O in two files.
  The -r2 is single-parent on eb88, clean at step 13 and clean against every other row. It keeps both calls with O's
  `updateWarningEntries` first and `restoreUnchangedMarkedSources` after, at both sites. range-diff against the
  original: 6 of 6 commits carried (3 identical, 3 changed). HashSet scan 0.
- **R4 r-csharp-consumer-smoke (DONE: COORD's -r2 86553e51b5).** As cut it conflicted with O at the tail of
  src/tests/PackageTests/README.md. The -r2 places its section before RidCompileAsset and conditions
  CSharpConsumer.csproj's LangVersion: clean onto eb88, clean at step 3, clean against C1's arm in the same README.
  ConsumerUsings.csproj holds no LangVersion: S1_NONE owes its entry (section 6, D7).
- **Slots.** No pushed row writes the BOARD, the roster or a proof page, so the BOARD docs row is predicted clean. C2's
  L and the i9's crash-verdict seat are unknown here. Each slot is re-read by the chain when its ref exists.

## 6. tP derive items this map surfaced

- **D1, the instrument.** tP-premap.sh needs a git >= 2.38 (`GITX`, default the Visual Studio one); it aborts by name
  without it. It closes COORD's two tO items (a Windows-style OUT; the first-base-only criss-cross reading).
- **D2, H3_TOKENS.** g-debugger-views edits golib/runtime/Goroutine.cs: an entry is owed or precheck hard-fails (step 1).
- **D3, the REG arm.** sibling-r2 removes one slnx key (`AliasNamespaceShadow/sortlocal/AliasNamespaceShadow.sortlocal.csproj`)
  and adds four: expect `seat-removed=[c2-sibling-package-name-r2: ...]`, as hashset's HashSet.go at O.
- **D4, REGEN_ALLOW.** Its grammar admits `.cs` and `.editorconfig`. sibling-r2 commits 2 production csproj and 24
  -tests files outside it. The footprint notes to compose: sort 1 (sort/sort.cs), float-fold 1, sibling-r2 4,
  p2-test-warning-entries 2 (`.editorconfig`), fm-record-r3 11, the else-if -r3 123 (re-measured at its cut).
- **D5, CSPROJ_TEMPLATE.** 1 file today (LiteralFloatConstFold.csproj); each slot that adds a project is read when
  pushed.
- **D6, PRE-D.** 8 behavioral projects added by the pushed rows (C2's L fixture comes with its slot): the NPOST
  literal and the CNR reconciliation.
- **D7, S1_NONE.** `src/tests/PackageTests/ConsumerUsings/ConsumerUsings.csproj` (smoke-r2; COORD's 12:03 item).
  CSharpConsumer.csproj is conditioned in the -r2 and needs none.
- **D8, leg E and the fixup's step 4** read "written" sets; under i9-r2 an unchanged file is not written (the i9: the
  multi-target `-stdlib` stages use the census path, which bypasses). Read at the derive.
- **D9, the release gates.** Four rows reach the packed tree or its gate: sort (sort.cs), smoke-r2 (go.lib.targets, a
  packed go.lib file), sibling-r2 (the production namespace of two packages, release-visible where they are packed:
  not checked here), C1's arm (release-smoke.ps1). MS23's release-smoke and darwin FULL runs are owed at P's union.
- **D10, the shared object store** (top of this file): `gc --auto` fires on fetch, commit and merge until COORD rules
  a remedy.

## 7. Not measured, and how to re-read at BASE

- **O's landing commits.** BASE = eb88ab9492 + a possible fixup-2 + the bank step + the MS13 refresh.
  - The refresh rewrites committed -tests sources. One P row is in that class: sibling-r2's 24 files. fips140deps and
    runtime/internal/wasitest are in neither sweep list; fips140test is an i9 shard row, and the i9's O patch shares no
    file with a P row (step 1). So no overlap is INFERRED; the i7's S and T patches do not exist yet.
  - The bank step edits the roster. No P row touches the roster, the BOARD or a proof page (measured: 0 rows).
- **The two owed re-cuts** (go.go -r2, else-if -r3) and **the three slots**: no ref.
- **Anything a build would see.** The censuses of section 4 are the only compile-level readings.
- **Re-read at BASE** once O lands, and after each re-cut or new row (read-only, five minutes, nothing written to the
  shared store):

  ```
  BASE=$(git -C /h/Projects/go2cs ls-remote origin refs/heads/master | cut -f1)
  BASE=$BASE ROWS=<the P list, tP-seats-draft.txt format> OUT=<a fresh folder> bash tP-premap-all.sh
  ```

  EXPECT its last line `PREMAP-ALL DONE conflicted-rows=0 regcheck-of-record=ok skip-head-parse-errors=0 ...` and rc 0.
  With today's list it reads `conflicted-rows=2` and rc 1 (the two owed re-cuts). The controls:
  `OUT=<a fresh folder> bash tP-premap-controls.sh`, EXPECT `PREMAP-CONTROLS DONE failed=0`. The sequential
  `git merge` rehearsal at BASE remains the reading of record.

Low: COORD's two 11:28 posts carry "11:50" in their bodies (already noted in L:244). Step 1 counted 5 renames in
sibling-r2; `diff -M` reads 6 (the csproj, go2cs.ico, go2cs.png, package_info.cs, the `.cs` and the `.go`).
