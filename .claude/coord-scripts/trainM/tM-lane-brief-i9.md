# TRAIN M: the i9's complement shard

From COORD. DRAFT 2026-10-02, adapted from TRAIN L's brief. The i9 sweeps its share of the banked roster at the
TRAIN M union, one row at a time, in its own clone. **Readings only**: nothing to cut, commit or push. Start when
COORD's GO message names the SHA.

| | |
|---|---|
| Union ref | `claude/coord-trainM-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` (the TRAIN M fixup commit). The union is TRAIN L's landed master `aa0a07d5fd` + one signed merge per row of the seat list (**24 rows, FROZEN** at 08:20 on 2026-10-02; the driver reads the rows, never a count) + the fixup (`fixup: TRAIN M`, and any `fixup-N: TRAIN M` commit on top of it). No follow-up merge rides: the follow list is empty |
| Shard list | `.claude/coord-scripts/trainM/tM-i9-shard.txt` on `claude/coord-handover`: **132 rows**, byte-identical to TRAIN L's and TRAIN K's shard, sha256 `057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d` (LF bytes). **The GO is authoritative**: COORD states the sha256 of the COMMITTED blob and its row count there, and the driver refuses to start without both. |
| Driver | `.claude/coord-scripts/trainM/tM-i9-shard.sh` (same branch; handover tip `<HND: filled in by COORD>`) |
| Seat list | `.claude/coord-scripts/trainM/tM-seats-draft.txt` (same commit). **New in M:** the driver reads the seats from this file and asserts every row is an ancestor of the union. It no longer carries a list of shas of its own. |
| Helper | `.claude/coord-scripts/trainM/tM-helpers.py` (same commit). **New in verify round 3:** the driver calls its `execrows` reader (python, read-only: two `git show` reads of the roster) to learn which rows carry an execution config. It refuses to start without the file. |

## What the shard is

It is the complement of the i7's list over the **225** banked rows (`docs/ValidatedTestPackages.md` at the union).
M adds no roster row and removes none, so the split is TRAIN L's: 93 rows on the i7 (with `runtime` and
`runtime/pprof` as direct `-tests` legs there), 132 here.

Two of your rows are also read on the i7 in this train, on purpose: `internal/godebug` and `log/slog`. They are the
two rows whose execution config G's seat decides, and COORD wants each read on two boxes. They stay in your shard.

**Your standing exclusion holds.** No crypto/tls row and no TLS-using net row is in the list: `crypto/tls`, `net`,
`net/http`, `net/http/httptest`, `net/http/httputil`, `net/http/cgi`, `net/http/fcgi`, `net/rpc`, `net/smtp` and
`crypto/x509` all stay on the i7. Never add one. The driver refuses, by row name, any list row matching `crypto/tls`,
`crypto/x509`, `net` or `net/*` (not only through the list's sha256).

## 1. Setup (L's, with the M names)

1. A **fresh worktree** of your clone at the union, detached, on the volume you used for L's shard:
   `git fetch origin claude/coord-trainM-union` then `git worktree add --detach <path> <UNION SHA>`.
   Assert `git -C <path> rev-parse HEAD` equals the SHA in the GO.
2. A **run folder outside the worktree** (floor 4). Write the driver, the list, the seat list and the helper into it
   as blob bytes, all four from ONE fetch:
   ```
   git fetch origin claude/coord-handover
   git show FETCH_HEAD:.claude/coord-scripts/trainM/tM-i9-shard.sh    > <run>/tM-i9-shard.sh
   git show FETCH_HEAD:.claude/coord-scripts/trainM/tM-i9-shard.txt   > <run>/tM-i9-shard.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainM/tM-seats-draft.txt > <run>/tM-seats-draft.txt
   git show FETCH_HEAD:.claude/coord-scripts/trainM/tM-helpers.py     > <run>/tM-helpers.py
   ```
   The helper needs the `python` your L shard's driver already called (3.8 or newer). `python --version` first; if
   it is older, tell COORD before launching.
3. The toolchain is go1.24.13, with `GOROOT` spelled **exactly** as `go env GOROOT` prints it (floor 6). A
   forward-slash spelling misroutes the emission and breaks the stdlib exclusion of the DefaultGODEBUG stamp. Use a
   .NET 10 SDK that resolves under `global.json`.
4. Concurrency is the same as your L shard: MSBuild `-m:4`, go `-p 4`. Heavy work is allowed (owner, 2026-09-28).
   WHEA corrected errors are counted per row and are not a stop. Two reboots inside 24 h means you fall back to
   light-only work and tell COORD.
5. Launch OS-detached:
   ```
   W=<worktree, POSIX path> UNION=<SHA> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<from the GO> EXPECT_ROWS=<from the GO> \
     [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir>] bash <run>/tM-i9-shard.sh
   ```
   While it runs the worktree is FROZEN (floor 4).

## 2. What the driver does

It is the driver that read 132/132 at TRAIN L. The M changes:
- **Asserts.** It requires `aa0a07d5fd` and EVERY row of `tM-seats-draft.txt` as ancestors, a `fixup: TRAIN M` commit,
  and 0 csproj still at `<LangVersion>latest</LangVersion>`.
- **Named readings** come from each row's FRESH comparison record, and each now prints the execution config the record
  itself states (`ENV configuration=... tiered=...`):
  - `internal/godebug`: `TestCmdBisect`. **This row runs at TC0 now** (G's roster commit drops its
    `execution: release-tiered`): expect `tiered=False`.
  - `log/slog`: `TestSetDefault`, `TestPanics`, `TestCallDepth`. **This row keeps release-tiered**: expect `tiered=True`.
  - `internal/synctest`: `TestDeadlockRoot` and `TestDeadlockChild`, pass/pass, as at L.
  - `os`: `TestRemoveAllWithExecutedProcess`, pass/pass, as at L.
- **Completeness and exit code, new (verify round 1).** The driver asserts that it swept as many rows as the list
  holds (its row loop reads the list on stdin, and the helpers it calls inside the loop now read `/dev/null`), and it
  exits **4** when any row failed (it used to exit 0). The two config rows' `ENV` line ends `ENV-OK` or
  `ENV-MISMATCH (expected tiered=...)`: post a mismatch.
- **Fresh records, and soft expectations in the exit code (verify round 2).** Before each row (and before an infra
  rerun) the driver deletes that row's three gitignored record files, so the record it reads afterwards is that row's
  own (the converter rewrites the comparison record only when its bytes change; an unchanged one kept an old mtime).
  Which rows carry an execution config is read from the roster at the base and at the union, not typed: the PRE line
  `execution-config rows` shows them. **Expect `internal/godebug`, `log/slog` and `net/http` at the base, and
  `log/slog` alone at the union.** The reader is `tM-helpers.py execrows` (verify round 3): it reads the row's
  annotation field, never the phrase in a row's prose (the two rows that dropped the annotation still quote it in
  their notes, and the earlier reader took that for the annotation). If the union's list shows anything but
  `log/slog`, stop and post the PRE line: do not sweep.
  A config row whose named reading prints neither `ENV-OK` nor `ENV-MISMATCH` (its record did not parse) is stamped
  `NO-RECORD` and counts as a soft miss.
  The exit status is **4** when a row failed OR a soft expectation missed, each stamped `SOFT:`: an `ENV-MISMATCH`, a
  config row with no fresh record, module-path hosts not 0, the GoDefaultGodebug file count off the base's, UF not 0,
  a csproj among the rewrites. Post every `SOFT:` line.
- **UF, new.** After the last row it counts untracked, not-ignored paths under `src/core`. At TRAIN L this shard left
  fixture copies there (archive/tar, bigmod and rsa testdata, go/build, go/doc, go/parser); c1-fixture-tracking
  committed them. Expect **0**.
- **Unchanged from L:** `GO2CS_MODULE_ROOT` unset; the per-row deadlock-line count (console + `full output:` file +
  the fresh record's stderr tails); the infra rerun (an MSB4166-only failure with no `error CS` reruns at most twice
  and is stated; a verdict failure is never rerun); the two artifacts for COORD, `tM-i9-logs/tracked-changes-U0.patch`
  and the **HS** stamp; the purge and the floors.

**Do not pass these switches:** `-Hop`, `-TestConfig` / `-TestTiered`, `-SkipBuild`, `-IgnoreDiskPreflight`,
`-PublishBinlog`. The sweep takes each row's execution config from the roster. Passing `-TestTiered` or `-TestConfig`
by hand is exactly how `internal/godebug` and `log/slog` would be read at the wrong config.

## 3. What to expect

**132 of 132 at their banked counts, no mover.** The precedents: your L shard read clean at L's union, and you read
all 132 with zero movers at L's union plus your own crash-class seat.

What M adds on top:
- **G's seat (g-godebug-pc-line).** Four things reach your rows.
  - The roster's execution configs: `internal/godebug` must pass **5 + 0 at TC0** with `TestCmdBisect` passing (on
    master it is red at TC0 and green tiered: the line the test asserts was one call late). `log/slog` stays
    **199 + 17, release-tiered**.
  - The line a frame resolves to changes for every caller-line assert. G's one-axis A/B named 17 rows; yours are
    `context`, `encoding/json`, `go/build`, `io`, `log`, `log/slog`, `sync`, `internal/godebug`. G read them on a base
    BEFORE TRAIN L. This is the first reading beside L. **Any pass-to-fail in these rows blocks the train.**
  - A goroutine is classed system or user by its START function now, not by its creator frame. TRAIN L's deadlock
    detector counts only user goroutines, so the two seats meet here for the first time. Watch `internal/synctest`
    (28), `sync` (46 + 6), `context` (57 + 1): a false `all goroutines are asleep` report, or a row that hangs where
    it used to report, is this.
  - `NoInlining` on every function that executes a `go`. No verdict should move. The sweeps WILL rewrite committed
    test sources with that attribute (G regenerated none of them), so the TE patch is expected to carry it.
- **Your own seat (i9-crashclass-byref-recv).** `src/core/testing/testing.cs` is in every host. You measured your 132
  with it at L's union; this reading is with every other M seat on top.
- **c1-fixture-tracking.** The UF count above.
- **P2's F4 and F8** edit the test conversion every row runs through (the publish deadline; the test project's
  references). A row that fails to BUILD its test host, or a `.tests.csproj` among the rewrites, is theirs to read.
- **P2's two host seats, when they are rows of the list** (`p2-host-test-list`, `p2-host-event-line-start`: both are
  in the 24-row list). H1 makes the testing host honour `-test.list` (list and exit). H2 changes the host's `--json`
  event framing AND the comparer that reads it, so **every row reads through a new reader**: a row that loses or
  gains a verdict with no change in its tests is theirs to read first.
- **Three more golib seats** are rows of the 24-row list: C1's `c1-darwin-inode64` and `c1-darwin-variadic` (by
  their seat lines, a macOS x64 symbol rename and a darwin/arm64 argument slot) and P1's `p1-warnings-tranche1` (by
  its seat line, a warning cleanup of golib, the generator source and the generator's header; no converted `.cs`).
  Their effect on your rows is NOT MEASURED by anyone: a mover with no other owner is read against them.
- **csproj among the rewrites: 0** was TRAIN L's reading for your rows. F8 can add a reference to a regenerated test
  project. Any csproj in `tracked-changes.txt` is a finding: post its diff.

## 4. A row that fails

- Read its saved row output and classify it: count drift, divergence (test names plus the first line of the C# text),
  timeout, or build/infra.
- Compare it with that row's reading in your TRAIN L shard.
- For a verdict failure, run a **CONTROL** after the shard, never beside it: the same sweep command on the same row at
  TRAIN L's master **aa0a07d5fd**, in a second fresh worktree. Name a mover by row and test, with
  `control -> union` verdicts.
- Do not fix it. Report it.

## 5. What is NOT yours in this train

- **Your two seats' linux and darwin items.** The crash-class seat's record says "linux/darwin run + AOT tier NOT
  MEASURED". The linux build and run of that seat (its `internal/syscall/unix/linux/siginfo_linux.cs` compiles only
  for linux) is P1's leg in this train. Darwin is a mac CI dispatch that C1 owns. Neither is an i9 reading, and
  neither is a shard row. Do not attempt them from this box.
- **ChannelTests and GenTests.** Your seats owe them; the i7's battery runs both (legs CT and GN).
- **The cross-package promoted-forwarders seat.** It is TRAIN N. Its footprint base is the pushed M union once it
  exists. It is not seated here and nothing in this brief reads it.
- **TRAIN L's NGE section** is not in this brief: i9-nugetgo-pack banked at L on your full reading from the module
  cache. `c2-s2-source-metadata` IS a row of the frozen list; its i9 half (a uuid pack) is run only from a step COORD
  writes into the GO WITH THAT SEAT'S NUMBERS. Do not run L's NGE from its old text (its counts are L's).
- **Converting `net/http` test sources** (the open owner question of 2026-10-02): not part of this brief. Wait for
  COORD's ruling; the ban on RUNNING crypto/tls and the TLS net suites is unchanged either way.

## 6. What to post back

Send one message to COORD, at most 40 lines. Subject: `TRAIN M i9 shard at <union sha10>: <pass>/<rows>`.
```
WHAT: the i9's TRAIN M complement shard at <sha10>: <pass>/132 at banked counts, <n> movers.
EVIDENCE:
- tree <sha10> tree <tree10>; list sha <16 hex> rows <n> (= the GO's); seats <n> all ancestors; go1.24.13 GOROOT <as printed>; sdk <dotnet --version>; -m:4 / -p 4 via <mechanism>
- wall <s>; WHEA total <n> (rows with nonzero: <pkg=n ...>); reboots <n>
- infra reruns: <pkg: attempt lines | none>
- FAIL rows: <pkg: sweep line; the tests; control verdict at aa0a07d5fd; L-shard verdict> (one line each)
- internal/godebug: <N + D>, ENV tiered=<False?>, TestCmdBisect <v>; log/slog: <N + D>, ENV tiered=<True?>, TestSetDefault/TestPanics/TestCallDepth <v>
- deadlock lines: <0 | pkg=n ...>; NAMED internal/synctest TestDeadlockRoot/Child <v>; os TestRemoveAllWithExecutedProcess <v>
- GENERATED TYPE MISSING: <pkg: once | REPRODUCED | none>
- tracked files rewritten: <n> across <n> rows; csproj among them: <0 | names>; HS <module-path hosts n, GoDefaultGodebug files n>; UF <n>
- driver exit <0 | 4>; SOFT lines: <none | each line>
- TE patch: <run>/tM-i9-logs/tracked-changes-U0.patch (<lines> lines) -- COORD reads it; your TRAIN L patch, if you still hold it: <path | gone>
- not measured: <what and why>
NEXT: <idle, worktree kept dirty for COORD's ruling on the test sources | the control you are running>
```
Keep `<run>/tM-i9-logs/` and the worktree until COORD says the shard is read. If you still hold your TRAIN L run
folder, keep its `tL-i9-logs/tracked-changes-U0.patch` too: it is the baseline COORD's TE reading compares against.
