# 1.26.10.2 re-pass briefs (dispatch-ready, one per lane)

These briefs carry out [the re-pass at the new freeze tip](../../PLAN-release-1-26-10-2.md), ruled on 2026-11-03.
COORD fills `<TIP>` and sends each block as it stands. Every line traces to one of three sources:
- that section, or the lane briefs and rulings in the same plan;
- [`PLAN-rebank-go126.md`](../../PLAN-rebank-go126.md) §12 and §15.1;
- COORD's rulings on this file's five flags, recorded under *Rulings* below.

Each block stays under 2 KB, so a Remote Control send arrives whole.

`<TREE>` is a clone or worktree detached at `<TIP>`. `<GOROOT>` is the go1.26.10 root, spelled exactly as
`go env GOROOT` prints it. The rows come from
[`repass-legs.tsv`](../data/rebank-1.26.10.2/repass-legs.tsv) (mode D), in `seq` order.

**Mode D** (COORD, 2026-11-03) replaces mode B. R-LAPTOP is intermittent for about two days, so it carries nothing:
- `crypto/tls` moves to G windows, which also keeps `net`;
- R-WSL's linux legs move to G-LAPTOP's WSL (G-WSL), which runs them after G's windows leg;
- every other windows row is dealt onto the i9 and H.

The rules for that move are recorded under *Mode D* below.

## i9

```
LANE i9 -- 1.26.10.2 RE-PASS, WINDOWS i9-S. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md re-pass section; act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 141, docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv shard i9-S, seq order (container/ring .. runtime).
 Write the list from the file; never retype it.
net/http and crypto/tls are OFF this box. One heavy task at a time; -TestBuildMaxCpu 4.
KIT: the TIP's own src/run-validated-sweep.ps1 is the pin; read its sha256 at launch.
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch: >= 30 GB to start (sweep floor 25 GB, re-checked every invocation);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - §15.1's survival canary and ENV; pass no -TestConfig.
PER ROW: §15.1's invocation, rc on the next line. GUARD 1: PASS with rc != 0 is PASS?rc, a finding. GUARD 2,
 before the restore: git status --porcelain | grep '^ D' EMPTY, else stop and name the row. Restore BY STATUS;
 porcelain then empty.
FLOOR ABORT: git clean -fdXq -- src/tests/Behavioral src/core; no ' D'; re-read free disk. >= 25 GB: re-enter at
 the first unrecorded row, same ref. Under: stop; post the row, free disk and last wall; COORD reassigns the rest.
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/i9-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/i9-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post): tip, HEAD^{tree}, ref@SHA, the sweep script's sha256, launch free disk, tally (run/141,
 words), wall_s per row, every non-PASS and PASS?rc by name, both ' D' censuses, residue, the triple.
```

## H

```
LANE H -- 1.26.10.2 RE-PASS, WINDOWS H-S. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md re-pass section; act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 95, docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv shard H-S, seq order (net/http/internal/ascii ..
 testing, testing LAST), with seven off-i9 rows (crypto/x509, net/http, net/http/httptest, net/http/cgi,
 net/http/httptrace, net/smtp, os/exec). Write the list from the file; never retype it. One battery per box.
KIT: the TIP's own src/run-validated-sweep.ps1 is the pin; read its sha256 at launch.
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch: >= 30 GB to start (sweep floor 25 GB, re-checked every invocation);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - §15.1's survival canary and ENV; pass no -TestConfig.
PER ROW: §15.1's invocation, rc on the next line. GUARD 1: PASS with rc != 0 is PASS?rc, a finding. GUARD 2,
 before the restore: git status --porcelain | grep '^ D' EMPTY, else stop and name the row. Restore BY STATUS;
 porcelain then empty.
FLOOR ABORT: git clean -fdXq -- src/tests/Behavioral src/core; no ' D'; re-read free disk. >= 25 GB: re-enter at
 the first unrecorded row, same ref. Under: stop; post the row, free disk and last wall; COORD reassigns the rest
 to another windows box after its own leg (net/http never to the i9).
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/H-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/h-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post): tip, HEAD^{tree}, ref@SHA, the sweep script's sha256, launch free disk, tally (run/95,
 words), wall_s per row, every non-PASS and PASS?rc by name, both ' D' censuses, residue, the triple.
```

## G

```
LANE G -- 1.26.10.2 RE-PASS, WINDOWS G-S. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md re-pass section (mode D); act: PLAN-rebank-go126.md §15.1 at TIP.
ROWS: 2, docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv shard G-S, seq order (net, crypto/tls). Write the list
 from the file; never retype it. crypto/tls qualified here at 25e3b6ede6.
ORDER: this shard, POST, then GWSL-1, then GWSL-2; one battery on the box at a time.
KIT: the TIP's own src/run-validated-sweep.ps1 is the pin; read its sha256 at launch.
BEFORE ROW 1:
 - a throwaway worktree DETACHED at TIP; git rev-parse HEAD is the ledger's first line;
 - read-back in <TREE>: dotnet --version 10.0.4xx, go version go1.26.10 (the toolchain triple);
 - free disk read and POSTED at launch: >= 30 GB to start (sweep floor 25 GB, re-checked every invocation);
 - clean-bin.ps1 -Root <TREE> -Force, then src\gen\go2cs-gen\bin and \obj separately; residue census;
 - nslookup invalid.invalid: NXDOMAIN, in the ledger before net;
 - §15.1's survival canary and ENV; pass no -TestConfig.
PER ROW: §15.1's invocation, rc on the next line. GUARD 1: PASS with rc != 0 is PASS?rc, a finding. GUARD 2,
 before the restore: git status --porcelain | grep '^ D' EMPTY, else stop and name the row. Restore BY STATUS.
FLOOR ABORT: git clean -fdXq -- src/tests/Behavioral src/core; no ' D'; re-read free disk. >= 25 GB: re-enter at
 the first unrecorded row, same ref. Under: stop; post the row, free disk and last wall; COORD reassigns the rest
 to another windows box after its own leg (crypto/tls never to the i9).
SHARD END: GUARD 3: purge as before row 1, then the ' D' and residue censuses.
EVIDENCE: docs/phase4/rebank-1.26.10.2/sweep/G-S.tsv + .ledger.tsv, §15.1's columns, on FRESH ref
 claude/g-sweep-1262c cut from TIP; ls-remote after the push.
DONE (one post): tip, HEAD^{tree}, ref@SHA, the sweep script's sha256, launch free disk, tally (run/2, words),
 crypto/tls's count beside 1,393, wall_s per row, non-PASS/PASS?rc by name, both ' D' censuses, residue, the triple.
```

## R-LAPTOP

Mode D gives R-LAPTOP nothing on the critical path. Its mode-B blocks (R-S, then RWSL-1 and RWSL-2) are retired:
`crypto/tls` moves to G-S, and the linux legs move to G-WSL below.

## G-WSL

```
LANE G -- 1.26.10.2 RE-PASS, LINUX GWSL-1 then GWSL-2 on G-LAPTOP's WSL. GO NOW, ONLY after G-S has posted.
 TIP <TIP>. Plan: PLAN-release-1-26-10-2.md re-pass (mode D) + "R-WSL" brief; act: PLAN-rebank-go126.md §12 R block.
ROWS: docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv, seq order; write each list from the file:
 - GWSL-1: 15 (internal/runtime/gc/scan .. runtime): the 14 reserved rows + gc/scan (no AVX-512 here);
 - GWSL-2: 129 (container/list .. slices).
KIT (R's frozen kit, sha256), ledgered before row 1; a mismatch stops the lane:
 rebank-linux.sh 99cdef2e8d0ba16a81e23eff0ebc6508a5d8f9af0edd4285fdb9c919c008a1c1
 classify_row.py 81e543608d9cabce35e38ce0f2e6013cfcc3189fdc0642771c1c51cfc03d7fad
 derive-argv.ps1 0ea6f0b90f915fc717853de71c7b9edbbeb895d11cfac044c1d5a18319f0a6d9
LAYOUT, as root: pwsh (dotnet tool) on PATH; /root/dotnet10 links the .NET root; GOROOT passed; KIT from master.
HOST: TMPDIR <= 10 bytes; <GOROOT> has test/; dotnet 10.0.4xx; go1.26.10; uid; NXDOMAIN before net.
DISK: host C: free POSTED beside the kit's df -BG at every launch and shard end. Kit floor 25 GB; HOST floor 35 GB
 on C: for every row. Never compact, re-configure or sparse the VHD.
ACT: §12's R block, a clone detached at TIP; argv derived by AST at TIP; rc on the next line; restore BY STATUS;
 purge build output; ' D' census every row.
FLOOR ABORT (either floor) / STALL: stop at the first unrecorded row; post it, both free figures and the last wall.
 COORD moves the rest to P2 after its own leg; P1 never (constraint 1).
EVIDENCE: linux/GWSL-1.tsv, GWSL-2.tsv (+ .ledger.tsv) under docs/phase4/rebank-1.26.10.2/, the R-WSL columns, on
 FRESH ref claude/g-linux-1262c cut from TIP, pushed at each shard's end; ls-remote.
DONE (per shard): tip, HEAD^{tree}, ref@SHA, host C: and df -BG, tally, wall_s per row, matched (+disclosed) beside
 linux:, non-PASS with cause, TIMEOUT tails, host checks, residue.
```

## P2

```
LANE P2 -- 1.26.10.2 RE-PASS, LINUX P2-1 then P2-2. GO NOW. TIP <TIP>.
 Plan: PLAN-release-1-26-10-2.md re-pass + "P1 and P2" brief; act: PLAN-rebank-go126.md §12 R block.
ROWS: docs/phase4/data/rebank-1.26.10.2/repass-legs.tsv, seq order; write each list from the file:
 P2-1: 46 (iter .. crypto/internal/fips140test); P2-2: 46 (os/user .. testing LAST).
KIT (R's frozen kit, sha256), ledgered before row 1; a mismatch stops the lane:
 rebank-linux.sh 99cdef2e8d0ba16a81e23eff0ebc6508a5d8f9af0edd4285fdb9c919c008a1c1
 classify_row.py 81e543608d9cabce35e38ce0f2e6013cfcc3189fdc0642771c1c51cfc03d7fad
 derive-argv.ps1 0ea6f0b90f915fc717853de71c7b9edbbeb895d11cfac044c1d5a18319f0a6d9
HOST: TMPDIR <= 10 bytes; <GOROOT> has test/; dotnet --version 10.0.4xx; go version go1.26.10; uid.
DISK: free disk POSTED at launch; under 20 GB, clear the Go build cache and NuGet locals.
BEFORE EACH ROW, only if free < 25 GB: 1. git clean -fdXq -- src/core; 2. no ' D' in git status --porcelain;
 3. re-check the 15 GB floor. Log each purge (row, free before/after).
FLOOR ABORT (under 20 GB at launch or 15 GB at a row): stop at the first unrecorded row; post it, free disk and
 the last wall. COORD moves the rest to G-WSL after its own leg; P1 never (constraint 1 supersedes ruling 9).
ACT: clone detached at TIP, checked clean; argv derived by AST at TIP; rc on the next line; restore BY STATUS;
 purge build output and ' D' census every row. crypto/x509: SSL_CERT_FILE, NIX_SSL_CERT_FILE UNSET, in ledger.
CONTAINER: resume ledger on persistent disk (corpus commit, converter commit, binary mtime); re-enter at the first
 unrecorded row; a killed row re-runs at a RAISED budget.
EVIDENCE: linux/P2-1.tsv, P2-2.tsv (+ .ledger.tsv) under docs/phase4/rebank-1.26.10.2/, the R-WSL columns, on
 FRESH ref claude/p2-linux-1262c cut from TIP, pushed at each shard's end; ls-remote.
DONE (per shard): tip, HEAD^{tree}, ref@SHA, launch free disk, tally, wall_s per row, purge lines, matched
 (+disclosed) beside linux:, non-PASS with cause, restarts, residue.
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
KIT: the TIP's own os-matrix.yml is the pin; read its sha256 at launch. READ-BACK: each job log's .NET SDK
 (10.0.4xx) and Go (go1.26.10). DISK: hosted runners, no floor; post each job's runner image name and version.
A ROW BANKS darwin: only when osx-arm64 and osx-x64 agree verdict for verdict: both validated, equal matched, equal
 disclosed-divergent (divergent - new), read from each RID's own artifacts. A row that does not bank carries NO
 darwin: segment.
EVIDENCE: docs/phase4/rebank-1.26.10.2/darwin/<dispatch>.tsv, the C1 brief's columns (no wall column), one line
 per row per RID, on FRESH ref claude/c1-darwin-1262c; ls-remote after each push.
DONE (one post per dispatch, <=40 lines): tip = headSha, run id, HEAD^{tree} of the evidence ref, ref@SHA,
 os-matrix.yml's sha256, runner images, tally (agreeing / disagreeing / unreached, counts beside darwin:), each
 RID's job wall.
```

## Rulings (COORD, 2026-11-03, on this file's five flags)

1. **Linux floor abort or stall.** The lane stops at its first unrecorded row and posts that row, its free disk and
   the last wall. COORD reassigns the unrun rows to the OTHER band-400 linux box: P2's go to R-WSL, and R-WSL's go
   to P2. That box takes them only after its own leg, one battery per box, on its own fresh `-1262c` ref. P1 never
   takes them. For the re-pass, constraint 1 supersedes ruling 9's "the other container" and ruling 2's P1 routing.
2. **Windows floor abort.** The fix is tried in place first:
   - purge the tree's ignored build output (`git clean -fdXq -- src/tests/Behavioral src/core`);
   - assert no ` D` line;
   - re-read free disk. At or above 25 GB, re-enter at the first unrecorded row, on the same ref.

   If free disk is still under 25 GB, the lane stops and posts. COORD reassigns the rest as in ruling 1, to another
   windows box after that box's own leg. `net/http` and `crypto/tls` never move to the i9.
3. **No pinned windows or C1 kit (accepted).** The tip's own `run-validated-sweep.ps1` and `os-matrix.yml` are the
   pin. Each windows and C1 DONE post gives their sha256 as read at launch, so the bank records what ran.
4. **No disk floor on hosted runners (accepted).** In its place, C1 posts each job's runner image name and version.
5. **No per-row wall for darwin (accepted).** There is no new column. C1 posts each RID's job wall, and darwin is not
   a free choice for the cutter.

The two windows floors are accepted as written: 30 GB to start and 25 GB in-sweep.

## Mode D (COORD, 2026-11-03: R-LAPTOP intermittent for about two days)

- **R-LAPTOP carries nothing.** In rulings 1 and 2, G-WSL now takes R-WSL's role: P2's unrun rows go to G-WSL,
  and G-WSL's unrun rows go to P2.
- **`crypto/tls` runs on G windows**, after G read it at `25e3b6ede6` as a qualifying discovery reading. `net` stays
  on G (the NXDOMAIN placement). Every other windows row is dealt onto the i9 and H, and the seven other off-i9 rows
  go to H; `net/http` and `crypto/tls` never go to the i9.
- **G-WSL is G-LAPTOP's WSL** (Ubuntu 22.04, band 400 at 10.0.401, go1.26.10, the kit's pins). It has no AVX-512,
  so it carries `internal/runtime/gc/scan` on the same basis R-WSL did. It runs the kit as root, after three setup
  steps: the PowerShell dotnet tool, a `/root/dotnet10` link (P2's precedent), and GOROOT passed explicitly.
- **One battery per physical box.** G-WSL's legs run only AFTER G's windows leg, never beside it.
- **The host disk rule.** A dynamic VHD never hands space back to the host when the kit purges inside the distro,
  so the host's free disk only falls during the linux legs:
  - at every WSL launch and every shard end, the post gives host C: free beside the kit's `df -BG`;
  - a HOST floor of 35 GB on C: applies to every WSL row. Under it, the lane stops at its first unrecorded row and
    posts, the same remedy as ruling 1;
  - the VHD is never compacted, re-configured or switched to sparse mode. Those are the owner's box settings; if
    the floor bites, COORD asks him.

<!-- Provenance (G, 2026-11-03, COORD's order of the same day). Rows, counts and endpoints are read from
     repass-legs.tsv (mode B; the cutter re-run at this branch's base reproduces it byte for byte against the blob).
     Kit hashes are the plan's R-WSL block. Evidence refs are the -1262c names of constraint 6. The P2 purge steps
     are the plan's ruled conditional purge. The five flags were raised at f2399dcb1d and ruled by COORD the same
     day; the blocks were amended on those rulings in the commit that follows it.
     Mode D (G, 2026-11-03, COORD's re-cut order of the same day): the blocks were re-cut from repass-legs.tsv in
     mode D. The G-WSL facts are G's read-only census, accepted by COORD, and crypto/tls's qualification is G's
     reading at 25e3b6ede6. -->
