# TRAIN N: the i9's complement shard

From COORD. DERIVED 2026-10-03 from TRAIN M's brief. The i9 sweeps its share of the banked roster at the TRAIN N
union, one row at a time, in its own clone. **Readings only**: nothing to cut, commit or push. Start when COORD's GO
message names the SHA.

| | |
|---|---|
| Union ref | `claude/coord-trainN-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` (the TRAIN N fixup commit). The union is TRAIN M's landed master `8f46a9adae` + one signed merge per row of the seat list (27 rows drafted, cutoff 10:00 Central 2026-10-03; the driver reads the rows, never a count) + any ruled follow-up merge + the fixup (`fixup: TRAIN N`, and any `fixup-N: TRAIN N` commit on top of it) |
| Shard list | `.claude/coord-scripts/trainN/tN-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to TRAIN M's (and L's and K's) shard, sha256 `057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d` (LF bytes). **The GO is authoritative**: COORD states the sha256 of the COMMITTED blob and its row count there, and the driver refuses to start without both. |
| Driver | `.claude/coord-scripts/trainN/tN-i9-shard.sh` (same branch; handover tip `<HND: filled in by COORD>`) |
| Seat list | `.claude/coord-scripts/trainN/tN-seats-draft.txt` (same commit). The driver asserts every row is an ancestor of the union. |
| Helper | `.claude/coord-scripts/trainN/tN-helpers.py` (same commit). The driver calls its `execrows` reader (python 3.8 or newer, read-only) to learn which rows carry an execution config. It refuses to start without the file. |

## What the shard is

The complement of the i7's list over the **225** banked rows (`docs/ValidatedTestPackages.md` at the union). N adds
no roster row and removes none, so the split is TRAIN M's: 93 rows on the i7 (with `runtime` and `runtime/pprof` as
direct `-tests` legs there), 132 here.

One of your rows is also read on the i7 in this train, on purpose: `log/slog`. G's seat `g-slog-roster-tc0` drops its
`execution: release-tiered`, the last annotated row, so it runs at the default (Release, tiering off) from N on, and
COORD wants that read on two boxes. It stays in your shard. (`internal/godebug` was the other two-box row at M; at N
it is an ordinary row of yours again.)

**Your standing exclusion holds.** No crypto/tls row and no TLS-using net row is in the list: `crypto/tls`, `net`,
`net/http`, `net/http/httptest`, `net/http/httputil`, `net/http/cgi`, `net/http/fcgi`, `net/rpc`, `net/smtp` and
`crypto/x509` all stay on the i7. Never add one. The driver refuses, by row name, any list row matching `crypto/tls`,
`crypto/x509`, `net` or `net/*`. (The owner's 2026-10-02 permission for a crypto/tls limits experiment on this box is a
separate, recorded experiment: it is not part of this shard and no leg depends on it.)

## 1. Setup (M's, with the N names)

1. A **fresh worktree** of your clone at the union, detached, on the volume you used for M's shard:
   `git fetch origin claude/coord-trainN-union` then `git worktree add --detach <path> <UNION SHA>`.
   Assert `git -C <path> rev-parse HEAD` equals the SHA in the GO.
2. A **run folder outside the worktree** (floor 4). Write the driver, the list, the seat list and the helper into it
   as blob bytes, all four from ONE fetch:
   ```
   git fetch origin claude/coord-handover
   git show FETCH_HEAD:.claude/coord-scripts/trainN/tN-i9-shard.sh    > <run>/tN-i9-shard.sh
   git show FETCH_HEAD:.claude/coord-scripts/trainN/tN-i9-shard.txt   > <run>/tN-i9-shard.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainN/tN-seats-draft.txt > <run>/tN-seats-draft.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainN/tN-helpers.py     > <run>/tN-helpers.py
   ```
3. go1.24.13 with `GOROOT` spelled **exactly** as `go env GOROOT` prints it (floor 6); a .NET 10 SDK that resolves
   under `global.json`. **New at N:** the union's converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0`
   (`p1-hashset-module`). If your box builds with the proxy off, warm the cache first, in the worktree at the union
   (go.sum is complete, so nothing tracked changes): `(cd <path>/src/go2cs && go mod download)`; then
   `git -C <path> status --porcelain` must still print nothing.
4. Concurrency as at M: MSBuild `-m:4`, go `-p 4`. Heavy work is allowed. WHEA corrected errors are counted per row and
   are not a stop. Two reboots inside 24 h means light-only work and a line to COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=<from the GO> \
     [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tN-i9-shard.sh
   ```
   While it runs the worktree is FROZEN (floor 4).

## 2. What the driver does (M's driver; the N changes)

- **Asserts.** It requires `8f46a9adae` and EVERY row of `tN-seats-draft.txt` as ancestors, a `fixup: TRAIN N` commit,
  and 0 csproj still at `<LangVersion>latest</LangVersion>`.
- **Execution configs.** The PRE line `execution-config rows` shows, read from the roster's annotation field:
  **`log/slog` alone at the base, NONE at the union.** A row annotated at either is read with an ENV verdict; `log/slog`
  left the union's list, so its record must say **`tiered=False`** (`ENV-OK`). If the union's list shows ANY row the
  driver now STOPS itself (review round 1: it compares the union's list with the ruled set, `tN-helpers.py execruled`,
  empty at N): post the PRE line and the ABORT line; do not sweep by hand.
- **Named readings** from each row's FRESH comparison record: `log/slog` (`TestSetDefault`, `TestPanics`,
  `TestCallDepth`; a `TestSetDefault` red is read as the first-launch symbolization class first, TRAIN M's note),
  `internal/godebug` (`TestCmdBisect`), `internal/synctest` (`TestDeadlockRoot`, `TestDeadlockChild`), `os`
  (`TestRemoveAllWithExecutedProcess`).
- **NEW at N: the refresh patch.** At the end, beside the TE patch (`tN-i9-logs/tracked-changes-U0.patch`), the driver
  writes the SAME rewrites with context, binary-safe: **`tN-i9-logs/tracked-changes.patch`**, and stamps its line count
  and sha256 on the HS line. It is the i9's input to COORD's MS13 refresh of committed `-tests` sources (a bank step
  after the battery: the committed test sources are refreshed from the WINDOWS re-emission, and your 132 rows are half
  of it). **Post its path and its sha256**, and keep the file until COORD says the refresh is committed.
- **Unchanged from M:** the row-loop count assert and exit 4 on a failed row; the record deletion before each row; the
  infra rerun rule; `GO2CS_MODULE_ROOT` unset; the per-row deadlock-line count; HS and UF; every `SOFT:` line.

**Do not pass these switches:** `-Hop`, `-TestConfig` / `-TestTiered`, `-SkipBuild`, `-IgnoreDiskPreflight`,
`-PublishBinlog`. The sweep takes each row's execution config from the roster.

## 3. What to expect

**132 of 132 at their banked counts, no mover.** You read 132/132 at TRAIN M's fixup-2 head.

What N adds on top (by the seat lines; NOT MEASURED on your rows by anyone):
- **`g-slog-roster-tc0`**: `log/slog` at TC0, **199 + 17**, `tiered=False`.
- **`g-cctor-init-frame`** (golib, `runtime/managed_impl.cs`): package initialization frames are named `init`,
  `init.funcN`, `init.N` as in Go. Any row that asserts on a traceback or a caller's function name through an init
  frame can move; `internal/godebug`'s `TestCmdBisect` reads call stacks.
- **Your own two seats** (`i9-crosspkg-promoted-forwarders`, `i9-gomethodvalue-nilfunc`: golib forwarders, reflect
  method values, `IsNil`), and `c2-native-array-view` (golib's native boxes). Watch `reflect`-heavy rows.
- **`r-module-driver-gomod-less`** edits the testing host (`src/core/testing/TestHost.cs`, `PackageAncestry.cs`) every
  row's host is built from: a row that fails to BUILD its host is theirs to read first.
- **`g-publish-keep`** changes the template every `-tests` host project is cut from (TrimMode partial, ReadyToRun only
  when not single-file); hosts never trim, so no verdict should move, but a `.tests.csproj` among the rewrites is a
  finding: post its diff.
- The converter seats re-convert every row's test sources; the TE patch is EXPECTED to carry G's NoInlining class (no
  committed test source is regenerated before the MS13 refresh) and fixup-2's using-alias lines.

## 4. A row that fails

- Read its saved row output and classify it: count drift, divergence (test names plus the first line of the C# text),
  timeout, or build/infra. Compare it with that row's reading in your TRAIN M shard.
- For a verdict failure, run a **CONTROL** after the shard, never beside it: the same sweep command on the same row at
  TRAIN M's master **8f46a9adae**, in a second fresh worktree. For `log/slog` the control at `8f46a9adae` is its OLD
  config: the sweep reads `release-tiered` there from the roster, so the two runs differ in config as well as tree; say
  so in the post. Name a mover by row and test, with `control -> union` verdicts.
- Do not fix it. Report it.

## 5. What is NOT yours in this train

- Linux and darwin items of your seats: the linux lanes read the golib seats (GolibTests, the behavioral projects your
  forwarders seat adds); darwin is a mac CI dispatch.
- GenTests, ChannelTests, go2cs.slnx: the i7's battery runs them.
- The TLS limits experiment is not a shard row.
- The MS13 refresh itself: COORD applies it on the i7; your part is the patch.

## 6. What to post back

One message to COORD, at most 40 lines. Subject: `TRAIN N i9 shard at <union sha10>: <pass>/<rows>`.
```
WHAT: the i9's TRAIN N complement shard at <sha10>: <pass>/132 at banked counts, <n> movers.
EVIDENCE:
- tree <sha10> tree <tree10>; list sha <16 hex> rows <n> (= the GO's); seats <n> all ancestors; go1.24.13 GOROOT <as printed>; sdk <dotnet --version>; -m:4 / -p 4 via <mechanism>
- PRE execution-config rows: base [<expect log/slog>] union [<expect none>]
- wall <s>; WHEA total <n> (rows with nonzero: <pkg=n ...>); reboots <n>
- infra reruns: <pkg: attempt lines | none>
- FAIL rows: <pkg: sweep line; the tests; control verdict at 8f46a9adae; M-shard verdict> (one line each)
- log/slog: <N + D>, ENV tiered=<False?> <ENV-OK?>, TestSetDefault/TestPanics/TestCallDepth <v>; internal/godebug: <N + D>, TestCmdBisect <v>
- deadlock lines: <0 | pkg=n ...>; NAMED internal/synctest TestDeadlockRoot/Child <v>; os TestRemoveAllWithExecutedProcess <v>
- tracked files rewritten: <n> across <n> rows; csproj among them: <0 | names>; HS <module-path hosts n, GoDefaultGodebug files n>; UF <n>
- driver exit <0 | 4>; SOFT lines: <none | each line>
- TE patch: <run>/tN-i9-logs/tracked-changes-U0.patch (<lines> lines); your TRAIN M patch, if you still hold it: <path | gone>
- REFRESH patch (MS13): <run>/tN-i9-logs/tracked-changes.patch (<lines> lines, sha256 <16 hex>)
- not measured: <what and why>
NEXT: <idle, worktree kept dirty for COORD's refresh | the control you are running>
```
Keep `<run>/tN-i9-logs/` and the worktree until COORD says the shard is read and the refresh is committed. If you still
hold your TRAIN M run folder, keep its `tM-i9-logs/tracked-changes-U0.patch` too: it is the TE-i9 baseline.
