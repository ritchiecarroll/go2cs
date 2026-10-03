# log/slog TestSetDefault first launch at TC0 with loose PDBs: master against the TRAIN M union (2026-10-02)

**Point-in-time record.** Measured 2026-10-02, 13:14:35 to 14:26:08 local time (71.6 minutes, inside the run's 75-minute cap), by a measurement sub-agent for COORD on the i7 (windows/amd64), at master `aa0a07d5fd` (control) and the TRAIN M union `e2008427b1`. Amend with dated blocks; never rewrite, never execute from.

Scope: measurement only. The run committed, pushed and configured nothing. Three independent verifiers then re-derived the numbers from the raw host output. None refuted the report. Every correction they listed is applied in this text, and section 10 names each one.

<!-- Provenance. Source: the run's scratch report and its raw logs (88 direct launch sets, 12 pipeline result
     files, sampler series, stamped phase logs), kept in COORD's scratch on the i7 and not in the repository.
     Verification: one workflow of three verifiers (lenses: raw-log fidelity, confounds and stale binaries,
     inference), each reading the raw files with its own scripts. Rulings: ledger 2026-10-02 14:53.
     The condensed tables below replace the report's verbatim log blocks, whose lines carry scratch paths. -->

## 1. The question

At the TRAIN M union, `log/slog` is the one roster row that still opts out of the configuration of record (Release, tiered JIT off) with `execution: release-tiered`. The roster's class note gives the reason: with tiering off, the one-second deadline in `TestSetDefault` is exposed to a timing dependence in how the runtime resolves caller frames. An earlier first-launch run on the i7, the one this apparatus was built from, located it at master: symbol-file reads at the first source-line resolution on a busy disk, because `runtime.Callers` symbolized at capture. Under the Windows Search indexer, the first launch of a fresh host copy read a fail at 1.0202 s, then 0.0846, 0.1311, 0.1322 and 0.1191 s (1 of 5 over 0.30 s).

The TRAIN M union carries the seat `g-lazy-callers`: `runtime.Callers` captures without file info and resolves a frame on first read. The question: at the union, does the first launch of a freshly published `log/slog` test host, at Release with `DOTNET_TieredCompilation=0` (TC0) and loose PDBs, still inflate `TestSetDefault` toward its deadline? Two decisions wait on it:

- **(a)** Can the row drop `execution: release-tiered`?
- **(b)** Is an embedded-PDB seat needed to protect the deadline?

## 2. The prediction, as worded before the run

**Conditions.** `P` is the first launch of a fresh copy of the published host: the exe is copied by a read/write loop (so it is cached), and the loose `.pdb` files by `CopyFileW`, a block clone on this ReFS volume (so they are cold). `mw` is the saved master re-launched right after (warm). `wu` is a warm-up launch, never counted. A cycle is one `P` and one `mw` per tree, the tree order alternating by cycle.

The clauses, as the scorer printed them ("stated before the run, scored as worded"):

1. The union (25 seats), P, quiet disk: `TestSetDefault` never fails and stays under 0.20 s in every cycle.
2. The union (25 seats), P, disk load: `TestSetDefault` never fails and stays under 0.20 s in every cycle.
3. Control, P, disk load: at least one cycle over 0.30 s, or a fail on the 1 s deadline.

**Verdict words.** HELD AS WORDED; PREDICTION FAILED when a union clause does not hold, whatever the control did; NOT DISCRIMINATING when both union clauses hold and the control does not inflate.

**Scorable rows.** A P row is scored only if it has a verdict, reports tiered false, shows a cold probe (first pass at least 3x the second) and met the block's disk condition at that launch: at most 20 % busy before the copy in the quiet block, at least 80 % busy before the copy and during the launch in a loaded block. A control fail counts only on the deadline (its output holds "context deadline exceeded", or its elapsed time is at least 1.0 s).

**The load, and the pilot rule.** The synthetic load is 8 threads on a 4 GiB scratch file (1 sequential read and rewrite, 7 random 64 KiB unbuffered readers). The rule for choosing it: a control-only pilot of 3 P launches under load, never pooled into a clause; keep 8 threads if at least one pilot launch exceeds 0.30 s or fails on the deadline, otherwise go to 16. No union row is read before that choice.

**What cannot be confirmed about the pre-registration.** The file holding the pilot rule was last written at 13:38:51, after the pilot, and the scorer at 13:33:04, after the quiet block was scored, so timestamps do not prove the rule preceded the data. It is not load-bearing: the knobs line stamped at 13:14:36 already shows 8 load threads, the launch notes written at 13:15 name the pilot as never pooled and the loaded block as 12 cycles, no other loaded block exists, and an independent extraction reproduces every scored reading against the worded 0.20 and 0.30 thresholds.

## 3. Result

**The prediction held as worded, and the experiment was discriminating.** All 30 scored P rows qualified; none was excluded.

| Clause as worded | Rows | Reading | Score |
|---|---|---|---|
| (1) Union, P, quiet disk: never fails, under 0.20 s in every cycle | 6 of 6 scorable | 0 fails, 0.0623 to 0.0652 s | **held** |
| (2) Union, P, synthetic disk load: same | 12 of 12 scorable | 0 fails, 0.0618 to 0.0690 s | **held** |
| (3) Control, P, synthetic disk load: at least one cycle over 0.30 s, or a fail | 12 of 12 scorable | 9 passes over 0.30 s, 2 fails on the 1 s deadline, 1 pass at 0.2145 s | **inflated** |

- **Control under load:** `TestSetDefault` P read 0.21 to 1.28 s (median 0.71 s) against 0.063 to 0.072 s warm. Two launches failed with "wanted canceled, got context deadline exceeded". `TestPanics` failed in the same two launches: its captured output starts with `TestSetDefault`'s late log line (`logger_test.go:358: INFO A~`).
- **Union under load:** `TestSetDefault` P read the same as its warm re-launch in every cycle (P minus warm: -0.008 to +0.004 s).
- **Strength of the control:** at or over the union's own 0.20 s bar in 12 of 12 loaded cycles. The pilot before it read 0.1120, 0.6111 and a fail at 1.0257 s, so the scored block ran at 8 threads.
- **Paired, per cycle** (I = P minus the same cycle's warm re-launch): control inflation exceeded union inflation in 12 of 12 loaded cycles (exact one-sided sign test p = 0.0002) and 6 of 6 quiet cycles (p = 0.0156). The smallest control inflation under load (+0.145 s) is 34 times the largest union inflation (+0.0043 s).
- **Quiet disk:** the control also inflated once (0.5414 s, the block's first P launch); its other five quiet readings were 0.066 to 0.074 s.
- **The pipeline's own first launch under load** (the condition the roster row runs): union 0.0621 to 0.0632 s in 5 of 5; control 0.3618 to 0.9250 s in 5 of 5 (all over 0.30 s), no fail. All ten loaded pipeline runs and both prep runs ended rc 0 with "Validated 199 tests against go test" and 17 disclosed-divergent, the roster's 199 and 17.
- **Symbols were cold in every direct P launch of both trees:** the probe's first pass was 20.4x to 499.1x its second in all 33 (30 scored, 3 pilot). The pipeline first launches carry no probe (section 9).

| Block | I control (min / median / max) | I union (min / median / max) | Control above union |
|---|---|---|---|
| quiet, 6 cycles | +0.0029 / +0.0074 / +0.4792 s | -0.0039 / +0.0001 / +0.0020 s | 6 of 6, p = 0.0156 |
| loaded, 12 cycles | +0.1450 / +0.6431 / +1.2163 s | -0.0084 / +0.0004 / +0.0043 s | 12 of 12, p = 0.0002 |

**What the data licenses.**

- **(a)** On this box, at Release with `DOTNET_TieredCompilation=0` and loose PDBs, the union's `TestSetDefault` first launch equals its warm cost in 18 direct P launches and 6 pipeline first launches, and the row validates 199 and 17 at TC0 in 6 of 6 union pipeline runs. Nothing measured here stands in the way of dropping the annotation. This is a windows-only reading on one hard-disk box. The row also carries `linux: 199 + 17`, and dropping the annotation moves that leg to TC0 too: that leg and the row's own gate run remain owed.
- **(b)** No embedded-PDB seat is needed to protect `TestSetDefault`'s deadline, and no `log/slog` verdict depends on symbol-read latency: the only timeout in Go 1.24.13's `log/slog` test sources is `TestSetDefault`'s (`logger_test.go:353`, a 1 s `context.WithTimeout`; read in `<GOROOT>/src/log/slog`). The first-launch cost itself is not gone. "Removed" is true of `TestSetDefault` only: the union's package still pays about +1.06 s median on a loaded first launch (3.64 against 2.55 s), concentrated in three tests (section 8). That these are the tests that read source lines is the report's reading, consistent with the union's lazy reader; no log shows which files a launch read. Whether embedded PDBs would remove that residual was not measured: no embedded-PDB arm was run.
- **The roster's own reason sentence does not match.** The `log/slog` row says it carries the annotation because `TestCallDepth`'s pc=0 "recovers fully with tiering on"; the class note cites `TestSetDefault`'s deadline. `TestCallDepth` passed at TC0 in all 100 launches of both trees here (0.0084 to 0.0130 s), so neither documented reason reproduces at the union. Whoever drops the annotation rewrites that row sentence too.
- **Attribution.** The arms are master and the **whole union (25 seats, 96 commits apart)**. Attribution to the seat `g-lazy-callers` is by source read, not by this A/B.
- **TC0** is evidenced by delivery of the knob (the launch environment, the host's echo, the result records), not by a reading of the JIT's state. That qualifier travels with any verdict quoted from this record.

## 4. Arms and proofs per tree

Toolchain on both arms: go1.24.13 windows/amd64, .NET SDK 10.0.400, runtime .NET 10.0.11. Condensed from prep's stamped log lines.

| Proof | Control | Union |
|---|---|---|
| Commit | master `aa0a07d5fd` | TRAIN M union `e2008427b1`; the control is its ancestor (`merge-base --is-ancestor` rc 0), 96 commits |
| Tree | TRAIN L's union worktree, HEAD verified, no tracked modification | a detached worktree created for the run, 0 untracked files |
| Capture site, `src/core/runtime/managed_impl.cs` | line 2749: `new StackTrace(skipFrames: 0, fNeedFileInfo: true)` | line 2895: `fNeedFileInfo: false`, plus the lazy reader |
| Converter | built in the run from that tree after a purge; sha `44aff4127ee3dfa4`, equal to the earlier run's build at the same commit | built in the run from that tree after a purge; sha `f73cff85b2df93aa` |
| Pipeline at prep | rc 0, 288,669 ms, 199 validated, 17 disclosed-divergent | rc 0, 275,762 ms, 199 validated, 17 disclosed-divergent |
| Published host | 146,846,170 bytes, sha `225e2c55f1db4a34`, equal to the earlier run's host | 146,858,458 bytes, sha `d9868dbdb353fd7d` |
| Loose PDBs | 214 published files, 186 loose `.pdb`; 187 obj assemblies, 0 with an embedded PDB, 187 with a sibling `.pdb` | the same counts |
| `goILPosition` in the built `runtime.dll` / the host | 0 / 0 | 1 / 1 |
| Tiered off, host result record | Release, tiered false; written in the run (13:20:27) | Release, tiered false; written in the run (13:25:42) |
| Tiered off, pipeline comparison record | tiered false, but **carried over, not rewritten**: last written 05:08:49 by the earlier run in the same tree | tiered false; written in the run (13:25:42) |
| Runtime config | no tiering knob bundled; the switch is the launch variable | the same |

- **The carried-over record is benign.** The writer rewrites the comparison record only when its content changes, and the host's own result record, written in the run, independently says tiered false.
- **Converter provenance is by path, purge and build time only.** Neither `go2cs.exe` carries a VCS stamp. The supporting facts are the tree HEAD check, the purge, the in-run build time, the control sha reproducing the earlier build, and the `-tests` staleness guard present in both trees (neither pipeline ran with the stale-converter override).
- **The union reads symbols from the same place the control does.** Its lazy reader opens the `.pdb` beside the host, which in a P launch is the fresh copy's directory.
- **Every launch row** shows the control's exe size on control rows and the union's on union rows; P rows ran from fresh copy directories, mw rows from the saved masters. The union's event stream is framed (a 0x16 marker per line) and the control's is not, an independent fingerprint of which build wrote each log.
- **The pipe phases were not re-proved.** The ten republished hosts have no sha, no `goILPosition` check and no converter re-hash; the evidence is the constant exe size per tree and the chain of publish times.
- **The hashes cannot be recomputed now.** The trees are purged and the masters and copies deleted, and the per-block "master unchanged since prep" check leaves no line on success. Those claims rest on the stamped log lines and the constant per-tree exe size in every row.
- **Fail set at TC0.** The union's failing top-level set was exactly the five disclosed alloc tests (`TestAlloc`, `TestAnyLevelAlloc`, `TestAttrNoAlloc`, `TestTextHandlerAlloc`, `TestValueNoAlloc`) in all 36 P and mw launches, its 4 warm-ups and its 6 pipeline runs. The control's differed in 3 launches (pilot c03, loaded c06, loaded c08), each adding `TestSetDefault` and `TestPanics`.

## 5. Box state during the run

No other conversion, build or battery ran on the box: a foreign-process guard ran before every pipeline leg and at every block and pipe start. **The box was not otherwise idle**, so "alone on the box" holds only in that sense.

- **Windows Search was running, not disabled** (service Running, start type Automatic, one indexer process for the whole run). The owner was working on the search index during the run; the service was not touched. Its share of one core by phase, from the stamped CPU readings: about 45 % during prep (696 to 997 CPU s over 675 s), 48 % in the quiet block (1009 to 1187 over 374 s), 45 % in the pilot block (1198 to 1347 over 328 s), 12 % in the loaded block (1352 to 1569 over 1751 s), 6 % and 5 % in the two pipe phases, and 22 % over the whole run (696 to 1622 over 4165 s).
- **The indexer was not equal per arm in the loaded block.** Summed over P launches its CPU was 20.6 s (control) against 7.5 s (union), and its writes about 158 against 38 MB. It does not explain the result: the control's three largest readings (0.88, 0.96 and the 1.28 s fail) came with the indexer idle, and the union stayed at 0.066 s with the indexer at 1.5 to 2.3 CPU-seconds.
- **Defender ran at about one core for about 24 minutes of the scored loaded block** (13:41 to about 14:05): 1,488 CPU-seconds over the block's 1,596 s, and 539 MB read with 12.58 s CPU in the 10 s load-on reading at 13:41:07, before the first scored launch (13:41:57). It ran at 2 to 6 % in every other phase. It was balanced across arms: the same per wall-second in both for cycles 1 to 11, and absent for both in cycle 12 (control 0.9604 s, union 0.0618 s). The pilot and the pipe phases, without it, show the same result.
- **Idle readings of the work volume (H:) that are logged**, each a 10 s window with nothing of this apparatus running: 0 % (13:26:38), 2 % (13:27:36), 7 % (13:32:34), 0 % (13:33:17), 3 % (13:38:28), 0 % (13:39:13) and **89 %** (14:08:06, ten seconds after the load generator stopped and its scratch file was deleted; queue 0 / 1 / 1, 2,131 reads). The quiet clause rests on the per-launch gate, not on these: H: read 0 to 1 % busy in the 2 s before every quiet P copy. During the quiet P launches themselves it was 17 to 40 % busy, from the apparatus's own flush of the copied exe.
- **Synthetic load:** H: busy 100 % in every 5 s window of the pilot and the loaded block and before and during every P launch in them; queue median 9 and 10, peaks 26 and 44. In the ten pipeline cycles: 100 % busy, queue median 9, peaks 31 to 94. The generator's log shows 8 workers alive and 0 errors throughout. In total the load held H: at 100 % for about 47 minutes while the owner's index work was running.
- **Unlogged observations of the report's author** (no log line stands behind them): at a 13:01 preflight, no converter, MSBuild or test-host process was present, and H: read 0 to 2 % busy then and at 13:14. The earliest logged free-space reading is 13:15:03 (3,576 GB on H:, 109 GB on C:).

## 6. What ran, in order

| Phase | Time | rc |
|---|---|---|
| prep (both trees: converter build, publish, proofs, masters) | 13:14:35 to 13:25:53 | 0 |
| quiet block, 6 cycles, cycle 1 starts with the control | 13:26:17 to 13:32:40 | 0 |
| pilot block, control only, 3 P launches under load (never pooled) | 13:32:55 to 13:38:33 | 0 |
| loaded block, 12 cycles under load, cycle 1 starts with the union | 13:38:52 to 14:08:17 | 0 |
| pipe1, 3 pipeline cycles per tree under load | 14:08:40 to 14:17:16 | 0 |
| pipe2, 2 pipeline cycles per tree under load | 14:17:23 to 14:23:43 | 0 |
| cleanup | 14:24:01 to 14:26:08 | 0 |

One take: only these blocks exist. After cleanup both trees were back at their commits with no tracked change, build output purged, and no process of the apparatus left; the search service was unchanged. The appendix holds every direct launch.

## 7. Statistics

**Per (block, tree, condition).** Seconds are the host's own elapsed, min / median / max. `wall` is the driver's external wall time per launch. Under load the read-MB column is dominated by the generator and is not evidence of symbol reads; in the quiet block it is: 1.06 MB (control P), 0.32 MB (union P) and 0.05 MB (warm), constant over the 6 cycles.

| Block | Tree | Cond | n | TestSetDefault fails | TestSetDefault s | TestPanics s | TestCallDepth s | TestJSONAndTextHandlers s | Package s | Wall s | H: read MB, median |
|---|---|---|---|---|---|---|---|---|---|---|---|
| quiet | control | P | 6 | 0 | 0.0660 / 0.0707 / 0.5414 | 0.0270 / 0.0283 / 0.0312 | 0.0087 / 0.0092 / 0.0096 | 0.4321 / 0.4377 / 1.6311 | 2.597 / 2.907 / 3.895 | 5.28 / 6.23 / 6.66 | 1.1 |
| quiet | control | mw | 6 | 0 | 0.0623 / 0.0633 / 0.0654 | 0.0267 / 0.0270 / 0.0280 | 0.0089 / 0.0094 / 0.0094 | 0.3511 / 0.3599 / 0.3653 | 2.439 / 2.467 / 2.692 | 4.39 / 4.56 / 5.03 | 0.1 |
| quiet | union | P | 6 | 0 | 0.0623 / 0.0638 / 0.0652 | 0.0245 / 0.0264 / 0.0297 | 0.0086 / 0.0089 / 0.0091 | 0.3796 / 0.3942 / 0.8610 | 2.535 / 2.600 / 3.135 | 5.00 / 5.47 / 6.04 | 0.3 |
| quiet | union | mw | 6 | 0 | 0.0622 / 0.0635 / 0.0686 | 0.0261 / 0.0272 / 0.0299 | 0.0085 / 0.0088 / 0.0102 | 0.3524 / 0.3567 / 0.3615 | 2.442 / 2.487 / 2.637 | 4.42 / 4.73 / 5.37 | 0.1 |
| pilot | control | P | 3 | 1 | 0.1120 / 0.6111 / 1.0257 | 0.1190 / 0.1320 / 0.2226 | 0.0100 / 0.0104 / 0.0130 | 1.2431 / 1.7606 / 2.2488 | 5.623 / 7.241 / 7.909 | 9.36 / 10.83 / 11.96 | 175.0 |
| pilot | control | mw | 3 | 0 | 0.0622 / 0.0641 / 0.0642 | 0.0271 / 0.0296 / 0.0304 | 0.0089 / 0.0093 / 0.0095 | 0.3542 / 0.3585 / 0.3590 | 2.434 / 2.479 / 2.605 | 5.44 / 6.15 / 6.31 | 98.0 |
| loaded | control | P | 12 | 2 | 0.2145 / 0.7100 / 1.2817 | 0.1091 / 0.2383 / 0.6348 | 0.0087 / 0.0094 / 0.0108 | 1.0311 / 1.7507 / 3.0875 | 5.181 / 7.698 / 9.096 | 8.46 / 10.78 / 12.66 | 159.1 |
| loaded | control | mw | 12 | 0 | 0.0630 / 0.0646 / 0.0716 | 0.0267 / 0.0283 / 0.0350 | 0.0091 / 0.0095 / 0.0117 | 0.3482 / 0.3658 / 0.4009 | 2.424 / 2.577 / 2.821 | 5.31 / 6.42 / 7.50 | 107.3 |
| loaded | union | P | 12 | 0 | 0.0618 / 0.0649 / 0.0690 | 0.0245 / 0.0274 / 1.0347 | 0.0086 / 0.0091 / 0.0108 | 0.5550 / 0.6882 / 1.7548 | 3.007 / 3.642 / 5.015 | 5.56 / 7.42 / 9.69 | 101.1 |
| loaded | union | mw | 12 | 0 | 0.0623 / 0.0642 / 0.0714 | 0.0262 / 0.0280 / 0.0334 | 0.0085 / 0.0092 / 0.0108 | 0.3521 / 0.3703 / 0.3836 | 2.400 / 2.545 / 2.818 | 5.66 / 6.73 / 7.30 | 110.6 |

`TestPanics` failed where `TestSetDefault` did (once in the pilot, twice in the loaded block); no other row has a fail in these four tests.

**Cold evidence.** After each P launch, every 4th `.pdb` of that copy (47 files, 2.7 MB) is read twice through an unbuffered handle; cold means the first pass is at least 3x the second.

| Block | Tree | n | First pass s (min / median / max) | Second pass s (min / median / max) | Ratio min | Ratio median | Ratio max |
|---|---|---|---|---|---|---|---|
| quiet | control | 6 | 0.834 / 1.072 / 1.606 | 0.026 / 0.027 / 0.034 | 32.3 | 36.6 | 57.1 |
| quiet | union | 6 | 0.530 / 0.835 / 1.049 | 0.026 / 0.026 / 0.034 | 20.4 | 27.7 | 39.5 |
| pilot | control | 3 | 9.316 / 9.667 / 10.687 | 0.026 / 0.032 / 0.034 | 282.3 | 287.1 | 414.0 |
| loaded | control | 12 | 9.615 / 11.719 / 13.792 | 0.026 / 0.028 / 0.038 | 316.0 | 416.6 | 499.1 |
| loaded | union | 12 | 9.013 / 10.532 / 13.076 | 0.026 / 0.028 / 0.035 | 286.6 | 389.4 | 459.4 |

**P minus warm**, median of the same-cycle difference:

| Block | Tree | Cycles | TestSetDefault s | TestPanics s | TestCallDepth s | TestJSONAndTextHandlers s | Package s | Pages in | H: read MB |
|---|---|---|---|---|---|---|---|---|---|
| quiet | control | 6 | +0.0074 | +0.0013 | -0.0000 | +0.0788 | +0.432 | +1670 | +1.0 |
| quiet | union | 6 | +0.0001 | -0.0013 | +0.0001 | +0.0379 | +0.119 | +2372 | +0.3 |
| pilot | control | 3 | +0.5469 | +0.1049 | +0.0014 | +1.4021 | +4.636 | +22887 | +75.5 |
| loaded | control | 12 | +0.6431 | +0.2075 | -0.0004 | +1.3767 | +5.080 | +6308 | +54.0 |
| loaded | union | 12 | +0.0004 | -0.0002 | -0.0001 | +0.3199 | +1.064 | +1313 | -11.9 |

**Disk, per block** (the sampler over the block's launch window):

| Block | Window | Overall busy | Lowest / median 5 s window | Queue median / max | Read MB | Write MB | Samples | Busy % before each P copy (min / median / max) | Indexer CPU s over the launches |
|---|---|---|---|---|---|---|---|---|---|
| quiet | 13:27:37 to 13:32:23 | 23 % | 0 / 18 % | 0 / 18 | 47 | 1850 | 1123 | 0 / 0 / 1 | 70.3 |
| pilot | 13:34:52 to 13:38:04 | 100 % | 100 / 100 % | 9 / 26 | 3400 | 2670 | 752 | 100 / 100 / 100 | 22.7 |
| loaded | 13:41:08 to 14:07:42 | 100 % | 100 / 100 % | 10 / 44 | 25899 | 20316 | 6267 | 100 / 100 / 100 | 46.6 |

The quiet block's 23 % includes the apparatus's own I/O (each P copy writes the 147 MB exe); its quiet gate is the before-copy column.

**Paired inflation of TestSetDefault, per cycle** (seconds; I = P minus the same cycle's mw; each pair is I control / I union):

- Quiet: c01 +0.4792 / -0.0017; c02 +0.0084 / +0.0003; c03 +0.0029 / -0.0002; c04 +0.0087 / +0.0020; c05 +0.0064 / -0.0039; c06 +0.0038 / +0.0009.
- Loaded: c01 +0.5943 / -0.0000; c02 +0.1450 / -0.0084; c03 +0.3260 / +0.0015; c04 +0.7067 / +0.0043; c05 +0.8162 / -0.0003; c06 +1.0806 / -0.0014; c07 +0.6920 / +0.0024; c08 +1.2163 / -0.0024; c09 +0.4491 / +0.0025; c10 +0.2662 / +0.0015; c11 +0.5929 / +0.0008; c12 +0.8974 / -0.0005.

**Pipeline first launches** (the `-tests` pipeline's own run of a host it has just published). Every row: all three named tests pass, tiered false, fail set the disclosed five, pipeline rc 0.

| Run | Tree | Condition | TestSetDefault s | TestPanics s | TestCallDepth s | TestJSONAndTextHandlers s | Package s | H: busy % before / during | Pipeline wall ms |
|---|---|---|---|---|---|---|---|---|---|
| prep | control | quiet, n=1, control first | 0.1129 | 0.0297 | 0.0095 | 0.6088 | 3.061 | NA / 44 | 288669 |
| prep | union | quiet, n=1 | 0.0626 | 0.0263 | 0.0093 | 0.3865 | 2.516 | NA / 34 | 275762 |
| pipe1 c01 | union | synthetic load | 0.0627 | 0.0261 | 0.0088 | 1.2803 | 4.350 | 100 / 100 | 58383 |
| pipe1 c02 | control | synthetic load | 0.9250 | 0.1544 | 0.0090 | 1.5513 | 7.081 | 100 / 100 | 61991 |
| pipe1 c03 | union | synthetic load | 0.0621 | 0.0260 | 0.0088 | 0.4119 | 3.167 | 100 / 100 | 60239 |
| pipe1 c04 | control | synthetic load | 0.3751 | 0.0281 | 0.0090 | 2.2250 | 6.522 | 100 / 100 | 66891 |
| pipe1 c05 | union | synthetic load | 0.0621 | 0.0266 | 0.0088 | 0.8212 | 3.353 | 100 / 100 | 60528 |
| pipe1 c06 | control | synthetic load | 0.8195 | 0.1396 | 0.0091 | 1.5504 | 6.446 | 100 / 100 | 64519 |
| pipe2 c01 | union | synthetic load | 0.0624 | 0.0263 | 0.0084 | 1.1527 | 3.585 | 100 / 100 | 62637 |
| pipe2 c02 | control | synthetic load | 0.3618 | 0.1607 | 0.0092 | 1.8520 | 6.907 | 100 / 100 | 63775 |
| pipe2 c03 | union | synthetic load | 0.0632 | 0.0273 | 0.0087 | 0.7038 | 3.263 | 100 / 100 | 63991 |
| pipe2 c04 | control | synthetic load | 0.9028 | 0.2545 | 0.0095 | 1.2972 | 8.609 | 100 / 100 | 60999 |

## 8. Per-test observations

**TestSetDefault and TestCallDepth.** Warm, `TestSetDefault` costs the same in both trees (median 0.0633 s control, 0.0635 s union in the quiet block), so the union's flat P reading is not a lower warm cost. `TestCallDepth` read 0.0084 to 0.0130 s and passed in all 100 launches of both trees, cold or warm, loaded or quiet: no inflation.

**TestPanics.**

- Control under load: P 0.109 / 0.238 / 0.635 s against 0.028 s warm. It failed twice, both times in the launch where `TestSetDefault` failed.
- Union under load: median 0.027 s, the same as warm, but one launch (loaded c05) read **1.0347 s** and passed. That the union paid a cold symbol read there is an inference from this one outlier in 12 launches: no log shows which files that launch read.
- Quiet disk: 0.025 to 0.031 s in both trees.

**Where the union's remaining cold cost lands under load** (all 56 top-level tests, median P minus median warm):

- `TestLogValue` +0.40 s, `TestJSONAndTextHandlers` +0.32 s, `TestSlogtest` +0.12 s. Those are the only 3 of 56 over +0.02 s.
- `TestLogValue` ran at least 0.5 s over its warm maximum in 5 of 12 loaded P launches (0.598, 0.680, 0.955, 1.543 and 1.859 s; median 0.437 against 0.038 s warm).
- Four tests reached 1.0 s or more in a single union launch under load, and passed: `TestLogValue` 1.8586 s (c10; also 1.5433 s in c12), `TestJSONAndTextHandlers` 1.7548 s (c11), `TestDefaultHandle` 1.5399 s (c01), `TestPanics` 1.0347 s (c05). `TestSlogtest` did not: its maximum was 0.6778 s (c05).
- Control, same view: 7 of 56 tests over +0.02 s, led by `TestJSONAndTextHandlers` +1.38 s, `TestConnections` +1.37 s and `TestSetDefault` +0.65 s.
- Sum of top-level test seconds, median: union P 3.55 s against 2.45 s warm; control P 7.60 s against 2.48 s warm.
- In the pipeline's own first launch under load the package read 3.17 to 4.35 s (union) against 6.45 to 8.61 s (control).

## 9. What was NOT measured

- **Attribution to the seat.** The arms are master and the whole union. No third arm at the seat's parent (`d4aae0aca0` against `209f055a89`) was run.
- **The JIT's own tiering state.** "Tiered false" is the launch variable echoed back by the host, identical for both trees; neither host bundles a tiering knob.
- **The linux leg, and any other platform or disk.** One windows box, one hard-disk volume. Linux, Darwin and an SSD were not run.
- **The roster row's gate.** No sweep or roster gate was run; these are direct launches and single-package pipeline runs.
- **An embedded-PDB arm.** Whether embedding would remove the union's residual first-launch cost is unmeasured. Tiered JIT on and ReadyToRun were not run either, nor any package other than `log/slog`.
- **The indexer's own load.** The inflation was produced by a synthetic generator, not by the indexer behind the original failure. The synthetic load is harsher: the earlier control P exceeded 0.30 s in 1 of 5 launches under the indexer, here in 11 of 12.
- **A truly quiet box.** The indexer used about half a core during prep and the quiet block, and Defender about one core for most of the loaded block (section 5). The quiet block is a quiet-disk reading at each P copy, not an idle-box reading.
- **Coldness in the pipeline first launches.** The 10 loaded pipeline launches have no cold probe. Coldness there is inferred: the control inflated 5 of 5, and the union's `TestJSONAndTextHandlers` read 0.41 to 1.28 s against 0.36 s warm. One union cycle (pipe1 c03, 0.41 s) is marginal.
- **A full cold probe.** Coldness was shown on every 4th `.pdb` of each copy (47 of 186 files), read after the launch. The files the host itself read during the launch are not separated out.
- **Position balance inside the quiet block.** The control's one quiet inflation (0.5414 s) was the block's first P launch. The union's first quiet P launch came third and read 0.062 s; the loaded block started with the union (0.066 s).
- **Other packages.** Nothing here speaks for a package whose deadline tests read source lines.

## 10. Verification, and the corrections applied to the report

Three verifiers, one lens each, none refuted the report. Each wrote its own readers instead of using the run's parser.

- **Raw-log fidelity.** Over the 88 direct launch sets and the 12 pipeline result files: the result record, the event stream and the JUnit file agree per launch (one terminal event per test, 0 missing, 0 duplicate), every row of both tables matches, and every statistic in section 7 reproduces.
- **Confounds and stale binaries.** Binaries, tiering, coldness, alternation and load were re-derived from the block records, the sampler series, the generator's log and read-only git. The top-level test order is identical in both hosts (56 tests, `TestSetDefault` at index 30). The result does not rest on the host's own clock: the driver's external wall time reads 10.8 s (control P) against 7.4 s (union P), median under load.
- **Inference.** Each clause re-scored as worded from an independent extraction; the sign-test values are 0.5^12 and 0.5^6; the Go test sources were read for deadlines.

Corrections applied, against the scratch report's wording:

1. The cold-probe ratio range is 20.4x to 499.1x, not "20x to 400x".
2. `TestSlogtest` did not reach 1.0 s (maximum 0.6778 s); four tests did, named in section 8.
3. The "H: was quiet at 13:01, 13:14, 13:26 and 13:27" line is replaced by the logged idle readings, the 89 % reading included.
4. The indexer's share is stated per phase: about half a core during prep and the quiet block, not "a fifth of a core throughout".
5. The 13:01 preflight statement is marked as an unlogged observation.
6. The control's pipeline comparison record is stated as carried over, not rewritten.
7. Defender's load is stated (about one core for about 24 minutes of the loaded block, balanced across arms), and "alone on the box" is qualified.
8. The indexer's per-arm imbalance in the loaded block is stated, with why it is not causal.
9. Converter provenance is stated in words: no VCS stamp, so by path, purge and build time.
10. "Cold in every P launch" is limited to the direct P launches; the pipeline first launches carry no probe.
11. The pipe-phase binaries are stated as not re-proved.
12. Read MB under load is no longer offered as evidence of symbol reads; the quiet-block figures are.
13. The hashes are stated as not recomputable now.
14. The roster row's reason sentence (`TestCallDepth`'s pc=0) is recorded as not reproducing, to be rewritten with the annotation.
15. The deadline caveat is closed: the only timeout in the Go 1.24.13 `log/slog` tests is `TestSetDefault`'s.
16. "Removes the first-launch inflation" is limited to `TestSetDefault`; the residual package cost and the unmeasured embedded-PDB arm are stated.
17. The claim is stated as windows-only on one hard-disk box, with the linux leg and the roster gate owed.
18. "Its index lives on H:" is dropped: no log supports it, and the indexer's writes did not land on H: in the two windows that could show it.
19. The quiet clause is stated as resting on the per-launch before-copy reading; H: was 17 to 40 % busy during quiet P launches.
20. The TC0 qualifier (delivery of the knob, not a reading of the JIT) is attached to the verdict.
21. The pilot rule's pre-registration is stated as not confirmable by timestamp, with why it is not load-bearing.
22. "TestPanics reads source lines, so the union pays the cold symbol read there" is restated as an inference from one outlier.

## 11. Rulings

COORD, ledger 2026-10-02 14:53, from this measurement:

- **No embedded-PDB seat.**
- **`log/slog` drops `execution: release-tiered` as its own roster seat in TRAIN N**, cut by lane G with a linux TC0 reading, not inside TRAIN M.

What that seat still owes, from sections 3 and 9: the linux leg at TC0, the row's own gate run, and the rewrite of the row's reason sentence and of the class note's sentence about the row.

## Appendix: every direct launch

Seconds are the host's own elapsed. `cond`: wu = warm-up (not counted), P = first launch of a fresh copy, mw = the master re-launched. `busy before/during` is H: busy % in the 2 s before the P copy and over the launch. `probe` is the seconds to read every 4th `.pdb` of that copy, first and second pass, right after the launch. `fail set`: "five" is exactly the five disclosed alloc tests.

| block | cycle.pos | tree | cond | TestSetDefault | TestPanics | TestCallDepth s | TestJSONAndTextHandlers s | package s | busy before/during | probe first/second s | fail set | at |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| quiet | 01.1 | control | wu | pass 0.0623 | pass 0.0313 | 0.0090 | 0.357 | 2.513 | NA/28 | NA/NA | five | 13:26:45 |
| quiet | 01.2 | union | wu | pass 0.0639 | pass 0.0262 | 0.0085 | 0.352 | 2.399 | NA/10 | NA/NA | five | 13:26:54 |
| quiet | 02.1 | union | wu | pass 0.0626 | pass 0.0262 | 0.0090 | 0.354 | 2.424 | NA/2 | NA/NA | five | 13:27:02 |
| quiet | 02.2 | control | wu | pass 0.0642 | pass 0.0269 | 0.0088 | 0.349 | 2.429 | NA/9 | NA/NA | five | 13:27:11 |
| quiet | 01.1 | control | P | pass 0.5414 | pass 0.0312 | 0.0089 | 0.432 | 3.122 | 0/40 | 1.05/0.026 | five | 13:27:47 |
| quiet | 01.2 | control | mw | pass 0.0623 | pass 0.0268 | 0.0089 | 0.351 | 2.439 | NA/8 | NA/NA | five | 13:27:56 |
| quiet | 01.3 | union | P | pass 0.0623 | pass 0.0269 | 0.0090 | 0.380 | 3.039 | 1/38 | 1.05/0.027 | five | 13:28:12 |
| quiet | 01.4 | union | mw | pass 0.0640 | pass 0.0294 | 0.0086 | 0.352 | 2.550 | NA/14 | NA/NA | five | 13:28:21 |
| quiet | 02.1 | union | P | pass 0.0625 | pass 0.0262 | 0.0089 | 0.396 | 2.535 | 0/18 | 0.91/0.026 | five | 13:28:36 |
| quiet | 02.2 | union | mw | pass 0.0622 | pass 0.0264 | 0.0087 | 0.355 | 2.442 | NA/19 | NA/NA | five | 13:28:45 |
| quiet | 02.3 | control | P | pass 0.0719 | pass 0.0281 | 0.0087 | 1.429 | 3.679 | 0/36 | 1.61/0.028 | five | 13:29:02 |
| quiet | 02.4 | control | mw | pass 0.0634 | pass 0.0270 | 0.0094 | 0.353 | 2.445 | NA/17 | NA/NA | five | 13:29:12 |
| quiet | 03.1 | control | P | pass 0.0660 | pass 0.0270 | 0.0096 | 0.434 | 2.597 | 1/20 | 0.83/0.026 | five | 13:29:26 |
| quiet | 03.2 | control | mw | pass 0.0631 | pass 0.0267 | 0.0094 | 0.358 | 2.444 | NA/5 | NA/NA | five | 13:29:34 |
| quiet | 03.3 | union | P | pass 0.0629 | pass 0.0262 | 0.0086 | 0.393 | 2.626 | 0/19 | 0.60/0.026 | five | 13:29:49 |
| quiet | 03.4 | union | mw | pass 0.0631 | pass 0.0299 | 0.0085 | 0.358 | 2.480 | NA/25 | NA/NA | five | 13:29:58 |
| quiet | 04.1 | union | P | pass 0.0649 | pass 0.0266 | 0.0090 | 0.391 | 2.575 | 0/17 | 0.53/0.026 | five | 13:30:12 |
| quiet | 04.2 | union | mw | pass 0.0629 | pass 0.0261 | 0.0093 | 0.362 | 2.637 | NA/9 | NA/NA | five | 13:30:20 |
| quiet | 04.3 | control | P | pass 0.0741 | pass 0.0312 | 0.0095 | 0.435 | 2.692 | 0/36 | 0.89/0.027 | five | 13:30:35 |
| quiet | 04.4 | control | mw | pass 0.0654 | pass 0.0280 | 0.0091 | 0.362 | 2.512 | NA/8 | NA/NA | five | 13:30:44 |
| quiet | 05.1 | control | P | pass 0.0695 | pass 0.0286 | 0.0089 | 0.440 | 2.682 | 0/40 | 1.12/0.027 | five | 13:30:59 |
| quiet | 05.2 | control | mw | pass 0.0631 | pass 0.0270 | 0.0094 | 0.364 | 2.692 | NA/11 | NA/NA | five | 13:31:08 |
| quiet | 05.3 | union | P | pass 0.0647 | pass 0.0245 | 0.0087 | 0.400 | 2.567 | 0/36 | 0.76/0.026 | five | 13:31:23 |
| quiet | 05.4 | union | mw | pass 0.0686 | pass 0.0269 | 0.0102 | 0.359 | 2.491 | NA/5 | NA/NA | five | 13:31:31 |
| quiet | 06.1 | union | P | pass 0.0652 | pass 0.0297 | 0.0091 | 0.861 | 3.135 | 0/29 | 0.92/0.034 | five | 13:31:45 |
| quiet | 06.2 | union | mw | pass 0.0644 | pass 0.0274 | 0.0089 | 0.356 | 2.482 | NA/7 | NA/NA | five | 13:31:54 |
| quiet | 06.3 | control | P | pass 0.0673 | pass 0.0275 | 0.0094 | 1.631 | 3.895 | 0/35 | 1.09/0.034 | five | 13:32:10 |
| quiet | 06.4 | control | mw | pass 0.0635 | pass 0.0269 | 0.0094 | 0.365 | 2.488 | NA/10 | NA/NA | five | 13:32:19 |
| pilot | 01.1 | control | wu | pass 0.0630 | pass 0.0284 | 0.0090 | 0.354 | 2.449 | NA/5 | NA/NA | five | 13:33:23 |
| pilot | 02.1 | control | wu | pass 0.0636 | pass 0.0292 | 0.0090 | 0.363 | 2.473 | NA/2 | NA/NA | five | 13:33:31 |
| pilot | 01.1 | control | P | pass 0.1120 | pass 0.1320 | 0.0104 | 1.761 | 5.623 | 100/100 | 10.69/0.026 | five | 13:35:40 |
| pilot | 01.2 | control | mw | pass 0.0641 | pass 0.0271 | 0.0089 | 0.359 | 2.479 | NA/100 | NA/NA | five | 13:35:51 |
| pilot | 02.1 | control | P | pass 0.6111 | pass 0.2226 | 0.0100 | 1.243 | 7.241 | 100/100 | 9.67/0.034 | five | 13:36:45 |
| pilot | 02.2 | control | mw | pass 0.0642 | pass 0.0304 | 0.0095 | 0.359 | 2.605 | NA/100 | NA/NA | five | 13:36:55 |
| pilot | 03.1 | control | P | fail 1.0257 | fail 0.1190 | 0.0130 | 2.249 | 7.909 | 100/100 | 9.32/0.032 | +TestPanics,TestSetDefault | 13:37:50 |
| pilot | 03.2 | control | mw | pass 0.0622 | pass 0.0296 | 0.0093 | 0.354 | 2.434 | NA/100 | NA/NA | five | 13:38:00 |
| loaded | 01.1 | control | wu | pass 0.0633 | pass 0.0289 | 0.0097 | 0.386 | 2.576 | NA/25 | NA/NA | five | 13:39:21 |
| loaded | 01.2 | union | wu | pass 0.0633 | pass 0.0268 | 0.0092 | 0.370 | 2.494 | NA/6 | NA/NA | five | 13:39:31 |
| loaded | 02.1 | union | wu | pass 0.0639 | pass 0.0269 | 0.0084 | 0.360 | 2.444 | NA/1 | NA/NA | five | 13:39:39 |
| loaded | 02.2 | control | wu | pass 0.0785 | pass 0.0290 | 0.0097 | 0.361 | 2.579 | NA/89 | NA/NA | five | 13:39:49 |
| loaded | 01.1 | union | P | pass 0.0659 | pass 0.0303 | 0.0097 | 0.869 | 4.950 | 100/100 | 10.44/0.033 | five | 13:41:57 |
| loaded | 01.2 | union | mw | pass 0.0659 | pass 0.0274 | 0.0092 | 0.372 | 2.644 | NA/100 | NA/NA | five | 13:42:07 |
| loaded | 01.3 | control | P | pass 0.6580 | pass 0.1091 | 0.0093 | 3.088 | 7.766 | 100/100 | 11.49/0.033 | five | 13:43:00 |
| loaded | 01.4 | control | mw | pass 0.0637 | pass 0.0271 | 0.0101 | 0.371 | 2.630 | NA/100 | NA/NA | five | 13:43:11 |
| loaded | 02.1 | control | P | pass 0.2145 | pass 0.2364 | 0.0095 | 2.100 | 6.634 | 100/100 | 13.79/0.031 | five | 13:44:06 |
| loaded | 02.2 | control | mw | pass 0.0695 | pass 0.0350 | 0.0095 | 0.397 | 2.821 | NA/100 | NA/NA | five | 13:44:16 |
| loaded | 02.3 | union | P | pass 0.0630 | pass 0.0270 | 0.0093 | 0.617 | 3.346 | 100/100 | 9.89/0.034 | five | 13:45:09 |
| loaded | 02.4 | union | mw | pass 0.0714 | pass 0.0334 | 0.0098 | 0.382 | 2.659 | NA/100 | NA/NA | five | 13:45:20 |
| loaded | 03.1 | union | P | pass 0.0665 | pass 0.0272 | 0.0086 | 0.555 | 3.635 | 100/100 | 12.91/0.029 | five | 13:46:17 |
| loaded | 03.2 | union | mw | pass 0.0650 | pass 0.0294 | 0.0092 | 0.383 | 2.818 | NA/100 | NA/NA | five | 13:46:28 |
| loaded | 03.3 | control | P | pass 0.3910 | pass 0.1601 | 0.0108 | 1.707 | 5.658 | 100/100 | 11.51/0.028 | five | 13:47:26 |
| loaded | 03.4 | control | mw | pass 0.0650 | pass 0.0296 | 0.0112 | 0.374 | 2.597 | NA/100 | NA/NA | five | 13:47:36 |
| loaded | 04.1 | control | P | pass 0.7782 | pass 0.1175 | 0.0095 | 2.216 | 9.096 | 100/100 | 11.92/0.038 | five | 13:48:35 |
| loaded | 04.2 | control | mw | pass 0.0716 | pass 0.0289 | 0.0100 | 0.401 | 2.661 | NA/100 | NA/NA | five | 13:48:46 |
| loaded | 04.3 | union | P | pass 0.0690 | pass 0.0277 | 0.0108 | 0.746 | 3.007 | 100/100 | 11.38/0.027 | five | 13:49:37 |
| loaded | 04.4 | union | mw | pass 0.0647 | pass 0.0285 | 0.0108 | 0.384 | 2.628 | NA/100 | NA/NA | five | 13:49:48 |
| loaded | 05.1 | union | P | pass 0.0635 | pass 1.0347 | 0.0092 | 0.728 | 5.015 | 100/100 | 10.62/0.026 | five | 13:50:44 |
| loaded | 05.2 | union | mw | pass 0.0637 | pass 0.0274 | 0.0087 | 0.369 | 2.529 | NA/100 | NA/NA | five | 13:50:55 |
| loaded | 05.3 | control | P | pass 0.8800 | pass 0.4077 | 0.0091 | 1.252 | 6.959 | 100/100 | 13.49/0.027 | five | 13:51:56 |
| loaded | 05.4 | control | mw | pass 0.0638 | pass 0.0269 | 0.0099 | 0.353 | 2.490 | NA/100 | NA/NA | five | 13:52:06 |
| loaded | 06.1 | control | P | fail 1.1499 | fail 0.3735 | 0.0096 | 1.795 | 7.438 | 100/100 | 12.81/0.028 | +TestPanics,TestSetDefault | 13:53:06 |
| loaded | 06.2 | control | mw | pass 0.0693 | pass 0.0293 | 0.0117 | 0.374 | 2.793 | NA/100 | NA/NA | five | 13:53:16 |
| loaded | 06.3 | union | P | pass 0.0661 | pass 0.0273 | 0.0091 | 0.569 | 3.118 | 100/100 | 12.60/0.028 | five | 13:54:12 |
| loaded | 06.4 | union | mw | pass 0.0675 | pass 0.0290 | 0.0093 | 0.379 | 2.646 | NA/100 | NA/NA | five | 13:54:22 |
| loaded | 07.1 | union | P | pass 0.0655 | pass 0.0313 | 0.0089 | 0.581 | 3.108 | 100/100 | 13.08/0.028 | five | 13:55:22 |
| loaded | 07.2 | union | mw | pass 0.0632 | pass 0.0282 | 0.0096 | 0.374 | 2.560 | NA/100 | NA/NA | five | 13:55:33 |
| loaded | 07.3 | control | P | pass 0.7562 | pass 0.5842 | 0.0106 | 1.196 | 7.897 | 100/100 | 12.76/0.027 | five | 13:56:32 |
| loaded | 07.4 | control | mw | pass 0.0643 | pass 0.0286 | 0.0095 | 0.364 | 2.557 | NA/100 | NA/NA | five | 13:56:44 |
| loaded | 08.1 | control | P | fail 1.2817 | fail 0.6348 | 0.0089 | 1.573 | 7.629 | 100/100 | 12.29/0.029 | +TestPanics,TestSetDefault | 13:57:47 |
| loaded | 08.2 | control | mw | pass 0.0655 | pass 0.0279 | 0.0092 | 0.360 | 2.605 | NA/100 | NA/NA | five | 13:57:58 |
| loaded | 08.3 | union | P | pass 0.0631 | pass 0.0276 | 0.0089 | 0.829 | 3.419 | 100/100 | 9.96/0.035 | five | 13:58:55 |
| loaded | 08.4 | union | mw | pass 0.0655 | pass 0.0273 | 0.0090 | 0.359 | 2.470 | NA/100 | NA/NA | five | 13:59:05 |
| loaded | 09.1 | union | P | pass 0.0658 | pass 0.0284 | 0.0094 | 0.748 | 3.648 | 100/100 | 10.33/0.033 | five | 14:00:03 |
| loaded | 09.2 | union | mw | pass 0.0633 | pass 0.0277 | 0.0085 | 0.367 | 2.470 | NA/100 | NA/NA | five | 14:00:14 |
| loaded | 09.3 | control | P | pass 0.5129 | pass 0.3271 | 0.0102 | 1.383 | 8.296 | 100/100 | 11.44/0.027 | five | 14:01:09 |
| loaded | 09.4 | control | mw | pass 0.0638 | pass 0.0276 | 0.0096 | 0.358 | 2.487 | NA/100 | NA/NA | five | 14:01:20 |
| loaded | 10.1 | control | P | pass 0.3294 | pass 0.1696 | 0.0091 | 1.031 | 5.181 | 100/100 | 9.61/0.026 | five | 14:02:11 |
| loaded | 10.2 | control | mw | pass 0.0632 | pass 0.0277 | 0.0095 | 0.361 | 2.477 | NA/100 | NA/NA | five | 14:02:21 |
| loaded | 10.3 | union | P | pass 0.0642 | pass 0.0271 | 0.0089 | 0.648 | 4.843 | 100/100 | 10.96/0.027 | five | 14:03:17 |
| loaded | 10.4 | union | mw | pass 0.0627 | pass 0.0277 | 0.0090 | 0.368 | 2.515 | NA/100 | NA/NA | five | 14:03:28 |
| loaded | 11.1 | union | P | pass 0.0631 | pass 0.0245 | 0.0086 | 1.755 | 4.466 | 100/100 | 10.30/0.028 | five | 14:04:18 |
| loaded | 11.2 | union | mw | pass 0.0623 | pass 0.0316 | 0.0092 | 0.359 | 2.479 | NA/100 | NA/NA | five | 14:04:28 |
| loaded | 11.3 | control | P | pass 0.6638 | pass 0.2401 | 0.0087 | 2.926 | 8.748 | 100/100 | 10.37/0.029 | five | 14:05:22 |
| loaded | 11.4 | control | mw | pass 0.0709 | pass 0.0267 | 0.0092 | 0.368 | 2.491 | NA/100 | NA/NA | five | 14:05:33 |
| loaded | 12.1 | control | P | pass 0.9604 | pass 0.2210 | 0.0087 | 2.578 | 8.652 | 100/100 | 10.42/0.027 | five | 14:06:31 |
| loaded | 12.2 | control | mw | pass 0.0630 | pass 0.0294 | 0.0091 | 0.348 | 2.424 | NA/100 | NA/NA | five | 14:06:40 |
| loaded | 12.3 | union | P | pass 0.0618 | pass 0.0263 | 0.0094 | 0.604 | 4.370 | 100/100 | 9.01/0.027 | five | 14:07:28 |
| loaded | 12.4 | union | mw | pass 0.0624 | pass 0.0262 | 0.0094 | 0.352 | 2.400 | NA/100 | NA/NA | five | 14:07:38 |
