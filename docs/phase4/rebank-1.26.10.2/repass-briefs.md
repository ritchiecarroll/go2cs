# 1.26.10.2 re-pass briefs (dispatch-ready, one per lane)

These briefs carry out [the re-pass at the new freeze tip](../../PLAN-release-1-26-10-2.md), ruled on 2026-11-03.
COORD fills `<TIP>` and sends each block as it stands. Every line traces to that section, to the lane briefs and
rulings in the same plan, or to [`PLAN-rebank-go126.md`](../../PLAN-rebank-go126.md) §12 and §15.1. A line marked
`[FLAG n]` needs a rule that does not exist yet; it is listed under *Flags for COORD* below and waits for COORD's
ruling. Each block stays under 2 KB, so a Remote Control send arrives whole.

`<TREE>` is a clone or worktree detached at `<TIP>`. `<GOROOT>` is the go1.26.10 root, spelled exactly as
`go env GOROOT` prints it. The rows come from
[`repass-legs.tsv`](../data/rebank-1.26.10.2/repass-legs.tsv) (mode B), in `seq` order.

## i9

```
LANE i9 -- 1.26.10.2 RE-PASS, WINDOWS i9-S. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip"; act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 84, docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv shard i9-S, seq order (net/http/internal/ascii ..
 runtime). Write the list from the file; never retype it.
net/http is OFF this box (net/http/internal/ascii is its own package). One heavy task at a time; -TestBuildMaxCpu 4.
KIT: the TIP's own src/run-validated-sweep.ps1; the plan pins no windows kit hash. [FLAG 3]
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch: >= 30 GB to start (sweep floor 25 GB, re-checked every invocation);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - §15.1's survival canary and ENV; pass no -TestConfig.
PER ROW: §15.1's invocation, rc on the next line.
 - GUARD 1: PASS with rc != 0 is PASS?rc, a finding;
 - GUARD 2, before the restore: git status --porcelain | grep '^ D' EMPTY, else stop and name the row;
 - restore BY STATUS; porcelain then empty.
FLOOR ABORT: stop, post the row and free disk; COORD rules. [FLAG 2]
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/i9-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/i9-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post, <=40 lines): tip, HEAD^{tree}, ref@SHA, launch free disk, tally (run/84, words), wall_s per row,
 every non-PASS and PASS?rc by name, both ' D' censuses, residue before/after, the triple.
```

## H

```
LANE H -- 1.26.10.2 RE-PASS, WINDOWS H-S. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip"; act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 59, docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv shard H-S, seq order (unicode/utf8 .. runtime/pprof).
 Write the list from the file; never retype it.
One battery per box: nothing else overlaps this shard.
KIT: the TIP's own src/run-validated-sweep.ps1; the plan pins no windows kit hash. [FLAG 3]
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch: >= 30 GB to start (sweep floor 25 GB, re-checked every invocation);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - §15.1's survival canary and ENV; pass no -TestConfig.
PER ROW: §15.1's invocation, rc on the next line.
 - GUARD 1: PASS with rc != 0 is PASS?rc, a finding;
 - GUARD 2, before the restore: git status --porcelain | grep '^ D' EMPTY, else stop and name the row;
 - restore BY STATUS; porcelain then empty.
FLOOR ABORT: stop, post the row and free disk; COORD rules. [FLAG 2]
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/H-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/h-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post, <=40 lines): tip, HEAD^{tree}, ref@SHA, launch free disk, tally (run/59, words), wall_s per row,
 every non-PASS and PASS?rc by name, both ' D' censuses, residue before/after, the triple.
```

## G

```
LANE G -- 1.26.10.2 RE-PASS, WINDOWS G-S. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip"; act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 94, docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv shard G-S, seq order (container/ring .. testing,
 testing LAST): net and seven off-i9 rows (crypto/x509, net/http, net/http/httptest, net/http/cgi,
 net/http/httptrace, net/smtp, os/exec). Write the list from the file; never retype it.
One battery per box. KIT: the TIP's own src/run-validated-sweep.ps1; no windows kit hash. [FLAG 3]
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch: >= 30 GB to start (sweep floor 25 GB, re-checked every invocation);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - nslookup invalid.invalid answers NXDOMAIN, in the ledger before net;
 - §15.1's survival canary and ENV; pass no -TestConfig.
PER ROW: §15.1's invocation, rc on the next line.
 - GUARD 1: PASS with rc != 0 is PASS?rc, a finding;
 - GUARD 2, before the restore: git status --porcelain | grep '^ D' EMPTY, else stop and name the row;
 - restore BY STATUS; porcelain then empty.
FLOOR ABORT: stop, post the row and free disk; COORD rules. [FLAG 2]
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/G-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/g-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post, <=40 lines): tip, HEAD^{tree}, ref@SHA, launch free disk, tally (run/94, words), wall_s per row,
 every non-PASS and PASS?rc by name, both ' D' censuses, residue before/after, the triple.
```

## R-LAPTOP (windows)

```
LANE R -- 1.26.10.2 RE-PASS, WINDOWS R-S, FIRST of R-LAPTOP's legs. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip"; act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 1, crypto/tls (repass-legs.tsv shard R-S). Mode B: it stays here because its count depends on the host's
 BoGo runner and throughput; R-LAPTOP banked the full 1,393.
ORDER: this shard, POST, then R-WSL's RWSL-1, then RWSL-2. One battery on the box at a time.
KIT: the TIP's own src/run-validated-sweep.ps1; the plan pins no windows kit hash. [FLAG 3]
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch (not recorded in discovery): >= 30 GB to start (sweep floor 25 GB);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - §15.1's survival canary and ENV; pass no -TestConfig.
THE ROW: §15.1's invocation, rc on the next line.
 - GUARD 1: PASS with rc != 0 is PASS?rc, a finding;
 - GUARD 2, before the restore: git status --porcelain | grep '^ D' EMPTY, else stop;
 - restore BY STATUS; porcelain then empty.
FLOOR ABORT: stop, post free disk; COORD rules. [FLAG 2]
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/R-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/r-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post, <=40 lines): tip, HEAD^{tree}, ref@SHA, launch free disk, word and count beside 1,393, wall_s,
 rc, both ' D' censuses, residue before/after, the triple. Then RWSL-1.
```

## R-WSL

```
LANE R -- 1.26.10.2 RE-PASS, LINUX RWSL-1 then RWSL-2. GO NOW, ONLY after R-S has posted. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip" + "R-WSL" brief; act: PLAN-rebank-go126.md
 §12's R block at TIP. One battery per box.
ROWS: docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv, seq order; write each list from the file:
 - RWSL-1: 15 (internal/runtime/gc/scan .. runtime): the 14 reserved rows + gc/scan (no AVX-512 here);
 - RWSL-2: 129 (internal/unsafeheader .. slices).
KIT PINS (sha256, R's frozen kit), in the ledger before row 1; a mismatch stops the lane:
 rebank-linux.sh 99cdef2e8d0ba16a81e23eff0ebc6508a5d8f9af0edd4285fdb9c919c008a1c1
 classify_row.py 81e543608d9cabce35e38ce0f2e6013cfcc3189fdc0642771c1c51cfc03d7fad
 derive-argv.ps1 0ea6f0b90f915fc717853de71c7b9edbbeb895d11cfac044c1d5a18319f0a6d9
HOST CHECKS before row 1, in the ledger: TMPDIR <= 10 bytes; <GOROOT> has test/; dotnet --version 10.0.4xx,
 go version go1.26.10, uid; nslookup invalid.invalid NXDOMAIN (before net);
 free disk read and POSTED at launch, >= 25 GB at EVERY row boundary.
ACT: §12's R block in a clone detached at TIP, seed checked clean; argv DERIVED by AST at TIP, never typed; rc on
 the next line; restore BY STATUS; purge build output; git status --porcelain | grep '^ D' EMPTY per row.
FLOOR ABORT / STALL: ruling 2 moves unrecorded RWSL-2 rows by the resume ledger, but its P1 half is barred by
 constraint 1; post, COORD rules the target. [FLAG 1]
EVIDENCE: docs/phase4/rebank-1.26.10.2/linux/RWSL-1.tsv, RWSL-2.tsv (+ .ledger.tsv), the R-WSL brief's columns,
 on FRESH ref claude/r-linux-1262c cut from TIP, pushed at each shard's end; ls-remote.
DONE (one post per shard, <=40 lines): tip, HEAD^{tree}, ref@SHA, launch free disk, tally (run/listed, words),
 wall_s per row, matched (+disclosed) beside linux:, every non-PASS with cause, TIMEOUT tails, host checks, residue.
```

## P2

```
LANE P2 -- 1.26.10.2 RE-PASS, LINUX P2-1 then P2-2. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip" + "P1 and P2" brief; act: §12's R block.
ROWS: docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv, seq order; write each list from the file:
 P2-1: 46 (iter .. crypto/internal/fips140test); P2-2: 46 (os/user .. testing, testing LAST).
KIT PINS (sha256, R's frozen kit), in the ledger before row 1; a mismatch stops the lane:
 rebank-linux.sh 99cdef2e8d0ba16a81e23eff0ebc6508a5d8f9af0edd4285fdb9c919c008a1c1
 classify_row.py 81e543608d9cabce35e38ce0f2e6013cfcc3189fdc0642771c1c51cfc03d7fad
 derive-argv.ps1 0ea6f0b90f915fc717853de71c7b9edbbeb895d11cfac044c1d5a18319f0a6d9
HOST: TMPDIR <= 10 bytes; <GOROOT> has test/; dotnet --version 10.0.4xx; go version go1.26.10; uid.
DISK: free disk read and POSTED at launch; >= 20 GB before row 1, else clear the Go build cache and NuGet locals
 (ruling 9). Still under 20 GB: post; COORD rules. [FLAG 1]
BEFORE EACH ROW, only if free < 25 GB (floor + 10): 1. git clean -fdXq -- src/core; 2. git status --porcelain
 shows no ' D'; 3. re-check the 15 GB floor, abort if under. Log each purge: row, free before, free after.
ACT: clone detached at TIP, checked clean; argv derived by AST at TIP; rc on the next line; restore BY STATUS;
 purge build output every row; ' D' census every row. crypto/x509 runs with SSL_CERT_FILE and NIX_SSL_CERT_FILE
 UNSET on both sides, said in the ledger.
CONTAINER: resume ledger on persistent disk, keyed on corpus commit, converter commit and binary mtime; after a
 restart re-enter at the first unrecorded row; a killed row re-runs at a RAISED budget.
EVIDENCE: docs/phase4/rebank-1.26.10.2/linux/P2-1.tsv, P2-2.tsv (+ .ledger.tsv), the R-WSL columns, on FRESH
 ref claude/p2-linux-1262c cut from TIP, pushed at each shard's end; ls-remote.
DONE (per shard, <=40 lines): tip, HEAD^{tree}, ref@SHA, launch free disk, tally, wall_s per row, purge lines,
 matched (+disclosed) beside linux:, non-PASS with cause, restarts, residue.
```

## C1

```
LANE C1 -- 1.26.10.2 RE-PASS, DARWIN C1-1..C1-15. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip" + "C1" brief; GoCorpusMigration.md H10.
ROWS: docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv, os darwin, one dispatch per shard (unchanged: C1-1..12
 the alphabetical waves of 19-20, testing last in C1-12; C1-13 runtime, C1-14 sync/atomic, C1-15 net).
 Write each filter from the file, comma-separated, EXACT; never retype it.
DISPATCH: os-matrix.yml, stage sweep-shard, goos darwin (osx-arm64 and osx-x64). Order C1-13, C1-15, C1-3, C1-11,
 then the rest (5 mac jobs at once). A count red against the roster is the reading, not a stop.
THE TIP: dispatch from a ref whose head IS <TIP>; commit nothing to it while runs are queued. A run whose headSha
 is not <TIP> is not evidence.
KIT: the TIP's os-matrix.yml; no pinned kit. [FLAG 3] READ-BACK: each job log's .NET SDK (10.0.4xx) and Go
 (go1.26.10). DISK: hosted runners, no box floor to read. [FLAG 4]
A ROW BANKS darwin: only when osx-arm64 and osx-x64 agree verdict for verdict: both validated, equal matched, equal
 disclosed-divergent (divergent - new), read from each RID's own artifacts. A row that does not bank carries NO
 darwin: segment.
EVIDENCE: docs/phase4/rebank-1.26.10.2/darwin/<dispatch>.tsv, the C1 brief's columns, one line per row per RID,
 on FRESH ref claude/c1-darwin-1262c; ls-remote after each push.
DONE (one post per dispatch, <=40 lines): tip = headSha, run id, HEAD^{tree} of the evidence ref, ref@SHA, tally
 (agreeing / disagreeing / unreached, with counts beside darwin:), each RID's job wall. [FLAG 5]
```

## Flags for COORD

Each flag names a rule a brief needs that no plan section or ruling holds yet. The brief carries the parts that do
exist and marks the gap.

1. **The linux floor-abort and stall remedy has no target.** Ruling 9 sends a container that cannot reach 20 GB to
   "the other container", and ruling 2 moves a stalled R-WSL's RWSL-2 rows to P1 and P2. Constraint 1 bars P1 from
   carrying a linux leg, so P2's only other band-400 box is R-WSL, and R-WSL's is P2. Which one takes the rows?
2. **There is no windows floor-abort remedy.** §15.1 checks free disk before row 1 and at each row boundary. No
   ruling says whether an aborted shard purges and re-enters at its first unrecorded row, or moves its rows.
3. **The windows sweep and C1 have no pinned kit.** The plan pins R's linux kit only. The windows sweep runs the
   TIP's own `run-validated-sweep.ps1`, and C1 runs the TIP's own `os-matrix.yml`, so the TIP is their pin. The
   `run-h10-recon.ps1` and `run-h10-dispatch.ps1` hashes belong to the remedy route, not to these briefs.
4. **C1 has no free-disk preflight.** C1 runs on hosted runners, with no box disk to read against the 25 GB floor.
5. **C1's TSVs carry no per-row wall.** The C1 brief's columns have none, and its post gives each RID's job wall.
   A `wall_s` column would be a new column.

Also: the plan's disk table gives 30 GB for the windows boxes before row 1, and the sweep's own floor is 25 GB.
The windows briefs carry both, as §15.1 does.

<!-- Provenance (G, 2026-11-03, COORD's order of the same day). Rows, counts and endpoints are read from
     repass-legs.tsv (mode B; the cutter re-run at this branch's base reproduces it byte for byte against the blob).
     Kit hashes are the plan's R-WSL block. Evidence refs are the -1262c names of constraint 6. The P2 purge steps
     are the plan's ruled conditional purge, quoted. -->
