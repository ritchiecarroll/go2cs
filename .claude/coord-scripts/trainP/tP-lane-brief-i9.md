# TRAIN P: the i9's complement shard

From COORD. DERIVED 2026-10-04 from TRAIN O's brief (`trainO/tO-lane-brief-i9.md`; you read O's union with it: 132/132,
0 movers, WHEA 0, 07:17 to 08:15, and one SOFT line, UF 1). **TRAIN P is the release train for go.\* 1.24.13.4**: the
release waits for it because it carries the fix for a converter defect in the published `go.sort`, and the `sort` row is
on your shard. The i9 sweeps its share of the banked roster at the TRAIN P union, one row at a time, in its own clone.
**Readings only**: nothing to cut, commit or push. Start when COORD's GO message names the SHAs.

| | |
|---|---|
| Union ref | `claude/coord-trainP-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` (the TRAIN P fixup commit, or a `fixup-N: TRAIN P` on top of it) |
| Base | `<BASE: 40 hex, filled in by COORD>` = TRAIN O's LANDED master (O's union eb88ab9492 + a possible fixup-2 + its MS13 refresh). The union is that base + one signed merge per row of the seat list (22 rows drafted 2026-10-04; the driver reads the rows, never a count) + any ruled follow-up merge + the fixup. **The driver REQUIRES `BASE=` and refuses a base that is not on the union's first-parent line.** |
| Shard list | `.claude/coord-scripts/trainP/tP-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to TRAIN O's (and N's, M's, L's and K's), sha256 `057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d` (LF bytes). **The GO is authoritative**: COORD states the sha256 of the COMMITTED blob and its row count there, and the driver refuses to start without both. |
| Driver | `.claude/coord-scripts/trainP/tP-i9-shard.sh` (same branch; handover tip `<HND: filled in by COORD>`) |
| Seat list | `.claude/coord-scripts/trainP/tP-seats-draft.txt` (same commit). The driver asserts every row is an ancestor of the union, and reads whether `p2-test-warning-entries` is a row (UF, below). |
| Helper | `.claude/coord-scripts/trainP/tP-helpers.py` (same commit). The driver calls its `execrows` reader (python 3.8 or newer, read-only). It refuses to start without the file. |

## What the shard is

The complement of the i7's list over the **225** banked rows (`docs/ValidatedTestPackages.md` at the union). No P row
touches the roster, so the split is O's: 93 rows on the i7 (with `runtime` and `runtime/pprof` as direct `-tests` legs
there), 132 here. **Two of your rows are also read on the i7 in this train, as X rows: `encoding/json` (as at O) and
`sort` (new: the release fix's package is read on both boxes).** Nothing changes for you: sweep both as ordinary rows.

**Your standing exclusion holds.** No crypto/tls row and no TLS-using net row is in the list: `crypto/tls`, `net`,
`net/http`, `net/http/httptest`, `net/http/httputil`, `net/http/cgi`, `net/http/fcgi`, `net/rpc`, `net/smtp` and
`crypto/x509` all stay on the i7. Never add one. The driver refuses, by row name, any list row matching `crypto/tls`,
`crypto/x509`, `net` or `net/*`. (The owner's 2026-10-02 crypto/tls limits experiment is separate: no leg depends on it.)

## 1. Setup (O's, with the P names)

1. A **fresh worktree** of your clone at the union, detached, on the volume you used for O's shard:
   `git fetch origin claude/coord-trainP-union` then `git worktree add --detach <path> <UNION SHA>`.
   Assert `git -C <path> rev-parse HEAD` equals the SHA in the GO, and `git -C <path> cat-file -e <BASE>^{commit}`.
2. A **run folder outside the worktree** (floor 4). The driver, the list, the seat list and the helper, as blob bytes,
   all four from ONE fetch:
   ```
   git fetch origin claude/coord-handover
   git show FETCH_HEAD:.claude/coord-scripts/trainP/tP-i9-shard.sh    > <run>/tP-i9-shard.sh
   git show FETCH_HEAD:.claude/coord-scripts/trainP/tP-i9-shard.txt   > <run>/tP-i9-shard.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainP/tP-seats-draft.txt > <run>/tP-seats-draft.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainP/tP-helpers.py     > <run>/tP-helpers.py
   ```
3. go1.24.13 with `GOROOT` spelled **exactly** as `go env GOROOT` prints it (floor 6); a .NET 10 SDK that resolves
   under `global.json`. The converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0` (in the base since O). If your
   box builds with the proxy off and the cache is cold, warm it in the worktree at the union (go.sum is complete, so
   nothing tracked changes): `(cd <path>/src/go2cs && go mod download)`; then `git -C <path> status --porcelain` must
   still print nothing.
4. Concurrency as at O: MSBuild `-m:4`, go `-p 4`. Heavy work is allowed. WHEA corrected errors are counted per row and
   are not a stop. Two reboots inside 24 h means light-only work and a line to COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> BASE=<BASE from the GO> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=<from the GO> \
     [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tP-i9-shard.sh
   ```
   While it runs the worktree is FROZEN (floor 4).

## 2. What the driver does (O's driver; the P changes)

- **Asserts.** It requires `BASE` on the union's first-parent line and EVERY row of `tP-seats-draft.txt` as an
  ancestor, a `fixup: TRAIN P` commit, and 0 csproj still at `<LangVersion>latest</LangVersion>`. **Since review round
  1 it also refuses a BASE that is BELOW the master the union was assembled on**: every first-parent commit between
  `BASE` and the union must be a TRAIN P seat merge or a TRAIN P fixup. `ABORT: BASE ... is BELOW the master the union
  was assembled on ... (refresh: TRAIN O ...)` means the `BASE` given is O's union eb88ab9492 (the stand-in the P seats
  were cut on) or another point of O's landing line: take `BASE` from the GO.
- **Execution configs.** The PRE line `execution-config rows` shows, read from the roster's annotation field: **NONE at
  the base and NONE at the union** (no row is annotated since N; no P row touches the roster). The reader's control is
  O's: it reads the roster at the parent of the NEWEST commit that changes the count of `execution: release-tiered` in
  the base's roster AND whose parent annotates `log/slog` (`g-slog-roster-tc0`'s roster commit 2d46eba8f0 unless a
  landing changed the phrase's count; a walk past the first commit is stamped). If the union's list shows ANY row the
  driver STOPS itself: post the PRE line and the ABORT line; do not sweep by hand.
- **Named readings** from each row's FRESH comparison record: `log/slog` (`TestSetDefault`, `TestPanics`,
  `TestCallDepth`), `internal/godebug` (`TestCmdBisect`), `internal/synctest` (`TestDeadlockRoot`, `TestDeadlockChild`),
  `os` (`TestRemoveAllWithExecutedProcess`) and, **new at P, `sort`** (`TestSortIntSlice`, `TestSortFloat64Slice`,
  `TestSortStringSlice`). Read the `sort` reading for what it is: sort's own suite sorts through `sort.Sort(...)` and
  never calls the three `.Sort()` methods the fix repairs, so pass/pass says the PACKAGE still validates at 0 moved
  verdicts, not that the fix works (that is `SortMethodSelfCapture` on the i7 and C1's release-smoke arm B).
- **UF, the known class (P).** At O your shard ended `UF 1` and exit 4 on `src/core/weak/.editorconfig`: the per-file
  warning entries a `-tests` run writes for a package whose only fact sits in a `_test.cs`, where the committed corpus
  held no entry file (ruled a known class, 08:21). The fix is P2's row `p2-test-warning-entries`, which commits that
  file (and `math/bits`', an i7 row). The driver now reads the seat list: **with that row, EXPECT `UF 0`**; without
  it, `src/core/weak/.editorconfig` is stamped KNOWN and is not a SOFT line, and any OTHER untracked path still is.
- **The purge retry (P).** The end-of-shard purge retries up to 6 times, 5 s apart, before it aborts (O's battery
  aborted on an empty `obj` folder a build node let go of seconds later). A `retries=` above 0 is a reading: post it.
- **The refresh patch** (as at O). At the end, beside the TE patch (`tP-i9-logs/tracked-changes-U0.patch`), the driver
  writes the SAME rewrites with context, binary-safe: **`tP-i9-logs/tracked-changes.patch`**, and stamps its line count
  and sha256 on the HS line. It is the i9's input to COORD's MS13 refresh of committed `-tests` sources at P (a bank
  step after the battery). **Post its path and its sha256**, and keep the file until COORD says the refresh is committed.
- **Unchanged from O:** the row-loop count assert and exit 4 on a failed row; the record deletion before each row; the
  infra rerun rule; `GO2CS_MODULE_ROOT` unset; the per-row deadlock-line count; HS; every `SOFT:` line.

**Do not pass these switches:** `-Hop`, `-TestConfig` / `-TestTiered`, `-SkipBuild`, `-IgnoreDiskPreflight`,
`-PublishBinlog`. The sweep takes each row's execution config from the roster.

## 3. What to expect

**132 of 132 at their banked counts, no mover, UF 0, exit 0.** You read 132/132 at TRAIN O's union.

What P adds on top (by the seat lines; NOT MEASURED on your rows by anyone unless said):
- **`sort` (THE RELEASE FIX, `g-sort-self-capture`)**: `sort/sort.cs`, three lines (`Sort((Interface)(x));` where
  `Sort(x);` called itself). The row should read its banked count.
- **your own row, `i9-incremental-cs-writes-r2`**: a `-tests` conversion now leaves an unchanged converted source
  unwritten. The driver's freshness readings are its own markers and the three gitignored record files it deletes
  before each row, which the seat does not touch; a row whose named reading says `NO FRESH COMPARISON RECORD` on a
  passing row would be a finding about that interaction: post it.
- **your second row, `i9-tests-host-crash-verdict`** (stacked on the first): a host that crashes, or holds a test to
  its package deadline, now gives the test(s) it was running a C# fail verdict, and the timeout event names them
  (`running tests: <names>`). On a passing row nothing moves (your seat: six rows, 0 differing leaves). A row that
  used to read 'no verdict' on a crashed host now reads NAMED failing tests: report it by those names.
- **the position map** (`g-method-value-fm-record-r3`, then `c2-elseif-position-record-r3` cut on it): `package_info.cs`
  GoPositionMap lines in `bytes`, `crypto/internal/hpke` and `encoding/json` among your rows (123 files corpus-wide), and
  hand-owned `runtime/managed_impl.cs` (a value-receiver method value is named `<entry>-fm`, its frame hidden from
  `Callers`). A row that asserts on a line number or a function name through a method value can move.
- **`c2-sibling-package-name-r2`**: it COMMITS the `-tests` sources of `crypto/internal/fips140test` (a row of yours):
  the namespace of the package it tests keeps its directory. Its rewrites should hold no hunk in those files; a hunk
  there is the first thing to read for that row.
- **golib** (`g-debugger-views`): debugger attributes on golib's slice, array, map, string, pointer and channel types
  and `DebuggerNonUserCode` on generated plumbing. No verdict should move; a row that prints a type through reflection
  of attributes would be the place.
- **-tests conversion itself**: `c2-embed-promoted-refs`, `c2-lifted-iface-test-cast`, the sibling seat's
  `testConversion.go` edit. A row that fails to CONVERT its tests is theirs to read first.
- **the converter rows** (`c2-named-basic-conv`, `c2-anon-struct-named-conv`, `c2-alias-table-same-name`,
  `g-go-namespace-shadow-r2`, `c2-literal-float-fold`, the two P2 rows) re-convert every row's test sources. N's and O's
  MS13 refreshes are in the base, so the TE patch should be small: a G-FRAME hunk is a source the refreshes missed
  (post it), OTHER is what P moves. `c2-anon-struct-named-conv` predicted ONE line in the `unique` test variant (a
  GoImplicitConv record): `unique` is your row; name it if you see it.

## 4. A row that fails

- Read its saved row output and classify it: count drift, divergence (test names plus the first line of the C# text),
  timeout, or build/infra. Compare it with that row's reading in your TRAIN O shard.
- **A `timeout` on a row that used to pass is read as a possible SPIN first** (G, 11:35: under the `-tests` host's
  Release + TieredCompilation=0 default a self-recursive tail call is an infinite loop, not a stack overflow; the host
  runs to its package timeout with no crash and no stack). Name the tests in flight.
- For a verdict failure, run a **CONTROL** after the shard, never beside it: the same sweep command on the same row at
  the base **`<BASE>`**, in a second fresh worktree. Name a mover by row and test, with `control -> union` verdicts.
- Do not fix it. Report it.

## 5. What is NOT yours in this train

- Linux and darwin items: the linux lanes read the golib rows; darwin is a mac CI dispatch, and the release gate
  (release-smoke on four RIDs, the darwin behavioral FULL) is C1's dispatch at the union.
- GenTests, ChannelTests, go2cs.slnx, the warning-entries gate, the behavioral suite: the i7's battery runs them.
- The TLS limits experiment is not a shard row.
- The MS13 refresh itself: COORD applies it on the i7; your part is the patch.
- The release (the version bump, the pack, the PIN): after P lands, the owner's and COORD's.

## 6. What to post back

One message to COORD, at most 40 lines. Subject: `TRAIN P i9 shard at <union sha10>: <pass>/<rows>`.
```
WHAT: the i9's TRAIN P complement shard at <sha10>: <pass>/132 at banked counts, <n> movers.
EVIDENCE:
- tree <sha10> tree <tree10>; base <sha10> on the first-parent line; list sha <16 hex> rows <n> (= the GO's); seats <n> all ancestors; go1.24.13 GOROOT <as printed>; sdk <dotnet --version>; -m:4 / -p 4 via <mechanism>
- PRE execution-config rows: base [<expect none>] union [<expect none>]; control ref <sha10>^ [<expect log/slog>]
- wall <s>; WHEA total <n> (rows with nonzero: <pkg=n ...>); reboots <n>
- infra reruns: <pkg: attempt lines | none>
- FAIL rows: <pkg: sweep line; the tests; control verdict at <BASE>; O-shard verdict> (one line each)
- sort: <N + D>, TestSortIntSlice/TestSortFloat64Slice/TestSortStringSlice <v>; encoding/json, bytes, crypto/internal/hpke, crypto/internal/fips140test, unique: <N + D each | at banked>
- log/slog: <N + D>, TestSetDefault/TestPanics/TestCallDepth <v>; internal/godebug: <N + D>, TestCmdBisect <v>
- deadlock lines: <0 | pkg=n ...>; NAMED internal/synctest TestDeadlockRoot/Child <v>; os TestRemoveAllWithExecutedProcess <v>
- tracked files rewritten: <n> across <n> rows; csproj among them: <0 | names>; HS <module-path hosts n, GoDefaultGodebug files n>
- UF: the driver's UF line verbatim (<0 with the row seated | KNOWN n, other n>); purge retries <n>
- driver exit <0 | 4>; SOFT lines: <none | each line>
- TE patch: <run>/tP-i9-logs/tracked-changes-U0.patch (<lines> lines; G-FRAME hunks <n>); your TRAIN O patch, if you still hold it: <path | gone>
- REFRESH patch (MS13): <run>/tP-i9-logs/tracked-changes.patch (<lines> lines, sha256 <16 hex>)
- not measured: <what and why>
NEXT: <idle, worktree kept dirty for COORD's refresh | the control you are running>
```
Keep `<run>/tP-i9-logs/` and the worktree until COORD says the shard is read and the refresh is committed. If you still
hold your TRAIN O run folder, keep its `tO-i9-logs/tracked-changes-U0.patch` too: it is the TE-i9 baseline (COORD also
holds a copy, `coord-scratch/tO/i9-patches/tO-tracked-changes-U0.patch`).
