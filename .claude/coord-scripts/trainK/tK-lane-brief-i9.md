# TRAIN K: the i9's complement shard

From COORD. The i9 sweeps its share of the banked roster at the TRAIN K union, one row at a time, in its own clone.
**Readings only**: nothing to cut, commit or push. Start when COORD's GO message names the SHA.

| | |
|---|---|
| Union ref | `claude/coord-trainK-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` |
| Shard list | `.claude/coord-scripts/trainK/tK-i9-shard.txt` on `claude/coord-handover`: **132 rows**, sha256 `057c78c1f9ad47f181fe2ef8fdb62519252cc486b48ea7b3541d8f550d9cdf3d` (LF bytes, as `git show` writes them). **The GO is authoritative**: COORD states the sha256 of the COMMITTED blob and its row count there, and the driver refuses to start without both. |
| Driver | `.claude/coord-scripts/trainK/tK-i9-shard.sh` (same branch; handover tip `<HND: filled in by COORD>`) |

## What the shard is

It is the complement of the i7's list over the 223 banked rows (`docs/ValidatedTestPackages.md` at f819887fa3). It is
TRAIN J's 130-row shard minus three reflect canaries that move to the i7 for TRAIN K's reflect-bridge seats:
`crypto/cipher`, `go/types` and `encoding/json`.

**Your standing exclusion holds.** No crypto/tls row and no TLS-using net row is in the list: `crypto/tls`, `net`,
`net/http`, `net/http/httptest`, `net/http/httputil`, `net/http/cgi`, `net/http/fcgi`, `net/rpc`, `net/smtp` and
`crypto/x509` are all the i7's. Never add one.

## 1. Setup

1. A **fresh worktree** of your clone at the union, detached, on the volume you used for TRAIN J's shard:
   `git fetch origin claude/coord-trainK-union` then `git worktree add --detach <path> <UNION SHA>`.
   Assert `git -C <path> rev-parse HEAD` equals the SHA in the GO.
2. A **run folder outside the worktree** (floor 4). Write the driver and the list into it as blob bytes:
   ```
   git fetch origin claude/coord-handover
   git show FETCH_HEAD:.claude/coord-scripts/trainK/tK-i9-shard.sh  > <run>/tK-i9-shard.sh
   git show FETCH_HEAD:.claude/coord-scripts/trainK/tK-i9-shard.txt > <run>/tK-i9-shard.txt
   ```
3. The toolchain:
   - go1.24.13, with `GOROOT` spelled **exactly** as `go env GOROOT` prints it (backslashes; floor 6). Keep it on the
     same non-ReFS volume as TRAIN J. On the i7, GOROOT on ReFS deadlocks one runtime child. The shard has no runtime
     row, but the pin stays the state of record.
   - A .NET 10 SDK that resolves under the union's `global.json` (the S1 pin, 10.0.100 with latestFeature). The driver
     runs `dotnet --version` in the worktree and aborts if it fails. **That is a finding: post it.**
4. Concurrency is the same as your TRAIN J shard: MSBuild `-m:4`, go `-p 4` (ledger 2026-09-29 14:57), set by the
   same mechanism you used then. The driver prints `GOFLAGS` in its PRE line; state your MSBuild setting in the post.
   Heavy work is allowed (owner, 2026-09-28).
   - WHEA corrected errors are COUNTED per row by the driver (a read-only System-log query). They are not a stop.
   - Two reboots inside 24 h means you fall back to light-only work and tell COORD.
5. Launch OS-detached, so it survives a session boundary:
   ```
   W=<worktree, POSIX path> UNION=<SHA> GOROOT_WIN='<go env GOROOT>' EXPECT_LIST_SHA=<list sha256 from the GO> EXPECT_ROWS=<rows from the GO> \
     [FIXUP=<fixup SHA from the GO>] [DOTNET_DIR=<SDK dir, if yours is not already first on PATH>] bash <run>/tK-i9-shard.sh
   ```
   `EXPECT_LIST_SHA` and `EXPECT_ROWS` are mandatory: a list that differs from the GO's aborts before the first row
   rather than sweeping a row twice or missing one.
   While it runs, the worktree is FROZEN (floor 4): the sweep rebuilds go2cs.exe from disk for every row.

## 2. What the driver does

- It asserts the list's sha256 and row count equal the GO's, with no repeated row.
- It checks `GOROOT_WIN` itself before exporting it (floor 6): no forward slash, a drive-rooted backslash path, and a
  `VERSION` file reading go1.24.13. Then `go env GOROOT` must echo it and `go version` must be go1.24.13.
- It asserts HEAD equals the union SHA, a clean tree, and that f819887fa3 plus the seats that bear on the shard are
  ancestors: S1 pin, R1-A commit 2, D4, D5, c2-gen-guard, the population generator, and the roster cleanup.
- It refuses a union that lacks the **TRAIN K fixup**: no `fixup: TRAIN K` commit after f819887fa3, or any
  `src/core`/Behavioral/Performance csproj still at `<LangVersion>latest</LangVersion>` (the S1 regeneration). Without
  it, every row would re-emit its committed test csproj. With `FIXUP=<sha>` it also asserts that commit is an ancestor.
- Each row runs `src/run-validated-sweep.ps1 -Filter <pkg> -Exact`, one at a time, exactly as TRAIN J's i7 S leg
  does. Before each row it checks free disk: 30 GB on the worktree's drive and 8 GB on C:. It captures rc before any
  pipe and counts WHEA per row.
- **A build-infra failure is rerun, at most twice, and stated.** The driver classifies from the row's FULL output, the
  file the sweep names on its `full output: <path>` line (the console shows only a failed row's last three lines).
  Infra means that file names MSB4166 or "child node ... exited prematurely", AND it carries no `error CS####`, AND the
  console shows no `GENERATED TYPE MISSING`. A failure with no `full output:` line (a COUNT or DRIFT) is a verdict
  failure. Each attempt's verdict line is stamped, so a rerun that turns green still shows the attempt before it. The
  precedent is `debug/pe`: eleven MSB4166s and no CS diagnostic, which reached its real verdict on a rerun (BOARD,
  *MSB4166 "Child node exited prematurely" is NOT a build root*).
- **A verdict failure is NEVER rerun by the driver.** The sweep's own named reruns still apply inside a row:
  - the oracle-only re-run;
  - c2-gen-guard's `GENERATED TYPE MISSING` re-run once. `REPRODUCED` on the second attempt is a real build failure.
  - Either way, each FAIL row's full output is saved under
    `<worktree>/scratchpad/sweep-oracle-flake/<stamp>/<pkg>/run<N>/row-output.txt`.
- At the end it lists every tracked file the sweeps rewrote (`tracked-changes.txt`, plus a diff stat), purges
  bin/obj/Generated, and asserts none remain and no tracked file was deleted (floor 8).
- **Leave the rewritten test sources in the worktree, uncommitted.** COORD rules whether they ride a ref.

**Do not pass these switches:**
- `-PublishBinlog`. It is NOT required and NOT present: C2's publish-binlog seat is TRAIN L, so K's sweep would refuse
  it as an unknown parameter.
- `-Hop`.
- `-TestConfig` / `-TestTiered`. An explicit value makes every row publish under it, which is not bank-eligible.
- `-SkipBuild`.
- `-IgnoreDiskPreflight`.

## 3. What to expect

**127 of 127 at their banked counts, no mover.** The precedents:
- TRAIN J's shard read 130/130 with no mover.
- Your D4 census (212 rows at d616cc686c + D4) and your D5 census both found ZERO go2cs-only movers, and both seats are
  in this union.

What K adds on top of those readings:
- `i9-r1a-commit2`: the slice-bound panics with Go arities. Its corpus moved, and test sources re-emit at the sweep:
  expect rewritten test `.cs` files on many rows.
- `p1-s1-dotnet-pin` with its csproj regeneration (C# 14): **expect 0 rewritten csproj.** The fixup's sed must equal
  the template's own emission, so any csproj in `tracked-changes.txt` is a finding.
- `c2-gen-guard`: generator-load warnings CS8032/CS8034/CS8784/CS8785 are now build ERRORS. Report every
  `GENERATED TYPE MISSING` line.
- `r-s2-cs15-escape` (footprint 0).
- The reflect-bridge seats `r-field-ptr-equality`, `c1-reflect-hash-band` and `r-named-slice-reflect-dims`. Your
  shard keeps reflect consumers such as `encoding/gob` and `encoding/xml`.
- `i9-a3`'s NoInlining lock paths.
- `g-gpc-torn-snapshot`'s gate. Every goroutine registers under a read lock.

## 4. A row that fails

- Read its saved row output. Classify it: a count drift (the named rows), a divergence (the test names and the first
  line of the C# failure text), or build/infra.
- Compare it with that row's reading in your D4 census.
- For a verdict failure, run a **CONTROL**: the same sweep command on the same row at master f819887fa3, in a second
  fresh worktree, after the shard is done and never beside it. A mover is named by row and test, with
  `control -> union` verdicts.
- Do not fix it. Report it.

## 5. What to post back

Send one message to COORD, at most 40 lines. Subject: `TRAIN K i9 shard at <union sha10>: <pass>/<rows>`.
```
WHAT: the i9's TRAIN K complement shard at <sha10>: <pass>/<127> at banked counts, <n> movers.
EVIDENCE:
- tree <sha10> tree <tree10>; list sha <16 hex> rows <n> (= the GO's); go1.24.13 GOROOT <as printed>; sdk <dotnet --version>; -m:4 / -p 4 via <mechanism>
- wall <s>; WHEA total <n> (rows with nonzero: <pkg=n ...>); reboots <n>
- infra reruns: <pkg: attempt 1 <verdict line>; attempt k <verdict line> | none>
- FAIL rows: <pkg: sweep line; the tests; control verdict at f819887fa3; D4-census verdict> (one line each)
- GENERATED TYPE MISSING: <pkg: once | REPRODUCED | none>
- tracked files rewritten: <n> across <n> rows; csproj among them: <0 | names>; the stat is <run>/tK-i9-logs/tracked-changes.stat.txt
- not measured: <what and why>
NEXT: <idle, worktree kept dirty for COORD's ruling on the test sources | the control you are running>
```
Keep `<run>/tK-i9-logs/` and the worktree until COORD says the shard is read.
