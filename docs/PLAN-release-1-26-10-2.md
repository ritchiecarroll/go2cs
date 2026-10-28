# PLAN — go.* 1.26.10.2: the first fix release on the Go 1.26 line

> **STATUS: DRAFT (COORD, 2026-10-26).** The owner asked for the post-publish wave and this plan on 2026-10-26, right
> after 1.26.10.1 published. Document type: an [instance plan](doctrine/ProcessGlossary.md#plan). It names the scope of the next
> release on master's Go 1.26.10 line, the seats and their lanes, how they land, and what gates the release.
> Procedure is not restated: seats land by the train discipline in the `train-assembly` skill, a re-bank follows
> [`GoCorpusMigration.md`](GoCorpusMigration.md), and the release follows its *Release day* section. If Go publishes
> 1.26.11 before this release is cut, the release becomes the patch hop's 1.26.11.1 instead
> ([`PLAN-version-lines.md`](PLAN-version-lines.md)), and the seats below ride it unchanged.
<!-- Sources: the post-publish queue COORD kept through the hop (rulings dated 2026-10-15 .. 2026-10-26, each in the
     ledger on claude/mailbox), the BOARD rows those rulings cite, and the 1.26.10.1 record (master 5100d1b16c). -->

---

## 1. Scope

**The headline: a pointer to another package's exported variable reaches the variable.** Today `&otherpkg.V`,
`otherpkg.V.M` taken as a method value, and `flag.BoolVar(&otherpkg.Flag, ...)` bind a copy, because the owning
package cannot see that an importer takes the address. 1.26.10.1 fixed the same case inside one package and documents
the cross-package case as a limitation. The fix, as ruled: **box every exported package-level value variable**. That
moves emission across the corpus, so this release carries a **full re-bank**.

**Also in scope**, each a seat with its own BOARD row:

| # | Item | Kind | Lane |
|:--|:--|:--|:--|
| 1 | Cross-package exported-variable box (the headline): design note, then seat | converter + golib | G (design), then a seat lane by ruling |
| 2 | Package-level box marker (`StaticBox`): a package-level `sync.WaitGroup` is Unbubbled, as in Go | converter + golib, 311 files | G (gated seat `a07906ae84`, re-base and re-A/B) |
| 3 | Goroutine stack reserve, option A: a small default reserve plus converter-marked large reserves | golib + converter | R |
| 4 | A deferred or `go` method call on an array element reads the element at the statement | converter | P2 |
| 5 | Converting a single `.go` file names its output correctly; a relative `-go2cspath` resolves | converter CLI | P1 |
| 6 | A rate-aware, resumable NuGet push (honours Retry-After; skips what is already published) | release tooling | P1 |
| 7 | golib's quiet build, stage 4 of [`PLAN-golib-full-trim.md`](PLAN-golib-full-trim.md): the 14 IL warnings the 1.26.10.1 pack printed | golib | H (G reviews) |
| 8 | Darwin harness items: the amd64-only test skip on the arm64 RID, cfile's host identity, `testing`'s working directory (fix B) | harness + hand-owned `testing` | C1 |
| 9 | Spawned test processes start faster: the crypto/tls test host is published ReadyToRun (a per-package opt-in), and its BoGo fan-out banks only from a measured full run of 480 s or less on the bank host | harness | P1 |
| 10 | Alloc-free element-address compare (`&a[i] == &b[j]`) | converter + golib | after #1 lands; lane by ruling |
| 11 | The `go2cs` dotnet tool: install with `dotnet tool install -g go2cs` and convert with no Go installed (Go fetched on first use, verified), per [`PLAN-dotnet-tool-installer.md`](PLAN-dotnet-tool-installer.md) | converter + packaging + release ritual | P2 |
| 12 | Signed release binaries: the release machine downloads the CI-built archives, Authenticode-signs the Windows executables in the release's PIN session, re-packs and re-uploads them, and GPG-signs `SHA256SUMS`; the attestation covers the bytes users download | release ritual + workflow | P1 |
| 13 | Proof pages show visitor-facing disclosure reasons: each disclosure gains a rendered `reason` in the present tense and a non-rendered history field, and the proof pages are regenerated (the owner's ruling that pages reachable from go2cs.net carry no fleet language) | test tooling + docs | R |
| 14 | A ReadyToRun publish of a converted program does not run out of memory on golib's nested generic types: the project template passes crossgen2's generic-cycle detection, and the profiles' ReadyToRun workaround comes off, gated on the single-file publish completing inside an 11 GiB memory limit | template + docs | P1 |
| 15 | A type switch's `case int:` no longer also takes int32/rune, nor `case uint:` uint32 | converter | H (landed) |
| 16 | A pointer to a native element equals the same element through any other native view | golib | P2 (landed) |
| 17 | golib names a Go int32 `int32` and a byte `uint8` in its panic texts | golib | H (landed) |
| 18 | Native AOT: golib's native struct marshalling stops building generic methods at run time, so a windows AOT program using `net` or TLS runs | golib | G |
| 19-32 | The visual-parity train: all 17 items of the 2026-10-16 review, landed in one deferred-corpus window and regenerated once, per [`PLAN-parity-train.md`](PLAN-parity-train.md) (#19, E1, is the first) | converter + golib + gen | per that plan |

**#1's NEWS text (breaking for compiled C#).** Release day lifts this verbatim into the 1.26.10.2 NEWS entry and the
release notes:

> **Another package's variables, by address.** Taking the address of a variable another package exports now
> reaches the variable, as in Go: `&otherpkg.V`, `flag.BoolVar(&otherpkg.Debug, ...)`, a pointer-receiver method
> value such as `f := otherpkg.V.Reset`, and an address inside it, `&otherpkg.S.Field` or `&otherpkg.Arr[i]`. Only a
> pointer- or interface-typed variable is still handed over as a copy ([Limitations](Limitations.md)). **This is a
> breaking change for compiled C# code**: each exported variable of a value type is now a `ref` property over its
> storage, not a field, so an assembly compiled against 1.26.10.1 must be rebuilt against 1.26.10.2. C# source that
> reads or assigns `pkg.V` compiles unchanged. go.* packages now depend on each other at the exact release version,
> so a project that mixes 1.26.10.1 and 1.26.10.2 packages fails at restore and names them. A read of such a variable
> costs about 0.5 ns more than a field read on the JIT, and about 0.7 to 0.9 ns more under Native AOT (measured on
> `win-x64`, one development machine).

<!-- From docs/phase4/DESIGN-crosspkg-exported-box.md section 11 (the seat as built), where the same text sits under
     COORD's OQ-2 ruling; COORD ruled 2026-10-27 that it ships on release day, lifted from here. The per-read costs are
     that section's bench (probe docs/phase4/probes/g-crosspkg-read-ab, median of 9, each runtime run twice): JIT nint
     0.25 for a field against 0.80 adopted; AOT nint 0.32-0.47 against 1.18. -->

**Not in this release, already scheduled:** the performance re-baseline against the published 1.26.10.1 packages
([`PLAN-perf-rebaseline.md`](PLAN-perf-rebaseline.md), G with the i9's AOT builds), the nugetgo rebuild wave against
1.26.10.1 (C2), and R's docs follow-up from the 1.26.10.1 veracity review. They need no converter change.

**Later, sized but not scheduled:** breaking the golib generic nest crossgen2 still expands, which is the root cause behind #14's flag (one lane, golib only), stack-allocation parity
([`DESIGN-stack-allocation.md`](DESIGN-stack-allocation.md)), the deferred-closure frame marker, `runtime.Goexit` on the
main goroutine, real `debug.ReadBuildInfo` data, the goroutine-leak profile, the crash block's `created by` line, the
remaining darwin list items that need a design, and stage 3d of
[`PLAN-golib-full-trim.md`](PLAN-golib-full-trim.md) (the dynamic-code class under the AOT analyzer, which reads 26
IL3050 sites at the plan's base), the named-result constant shift that masks its count, and long `TMPDIR` paths that skip the unix-socket tests
(both stated in Limitations and Known issues until fixed).

## 2. Order and landing

1. **Independent seats land first, in small batches on master**: #4, #5, #6, #7 and #8. Each is red-first, carries
   its own `src/core` footprint where it changes emission, and passes the batch gates (go test, GenTests, GolibTests,
   the solution builds, CNR). None of them moves a banked count.
2. **#2 and #3 land next.** Both change emission corpus-wide; each lands alone with its two-seeded footprint.
3. **#1 lands last of the emission seats**, after its design note is ruled, because it moves the most emission and
   triggers the re-bank.
4. **The re-bank**, sharded across the windows boxes with linux and darwin annotation legs, as for 1.26.10.1.
5. **Release day**, by the runbook, including the push from #6.

## 3. What gates the release

- Every seat landed, each with its BOARD row closed or amended.
- The full re-bank: every banked row at or above its count, every disclosure re-signed.
- Release-smoke on the four shipped RIDs, the darwin census, and the ritual rehearsal on the release machine.
- The docs state what the release does, in the present tense.

## 4. Open questions for the owner

- **OQ-1.** Four pages on the published docs site use fleet language: CIMatrix, Glossary,
  DESIGN-mn-scheduler-sizing and NugetgoPublish. Rewrite them for visitors, or exclude them from the site?

## The re-bank legs

The windows leg of the 1.26.10.2 re-bank is the §6 full sweep, sharded across i9, H, G-LAPTOP and R-LAPTOP as
[`PLAN-rebank-go126.md`](PLAN-rebank-go126.md) §15 describes. The linux and darwin legs re-read every annotated row
at the same tip, **`f75e0c53ec`** (`f75e0c53ec523161f1990db7a407c9249c9ed326`), so that each `linux:` and `darwin:`
segment is a reading at the corpus that ships (platform parity is a standing owner goal). Both legs start at the
tip. They do not wait for the windows bank: every row is already banked at go1.26.10, the linux act is the
per-package pipeline (never the sweep), and the darwin sweep scores a row against its own `darwin:` expectation. The
procedure is the runbook's H10 linux leg and its darwin leg (`GoCorpusMigration.md` H10, the 2026-10-24
amendment, item 5). This section supplies the deal, the projections and the briefs.

The deal is [`data/rebank-1.26.10.2/legs.tsv`](phase4/data/rebank-1.26.10.2/legs.tsv) (`<LEGS>` below). It has one
line per (os, lane, shard, seq, row), with `projected_s` and the source of that wall. Each list runs
smallest-first, and `testing` runs last in its list.

### The eligible sets

These counts come from `ValidatedTestPackages.md` at `f75e0c53ec`, row by row, and from
`data/rebank-go1.26.10/eligible-<os>.txt`:

| OS | Eligible | Read from | Not eligible |
|:--|--:|:--|:--|
| linux | **236** | every row whose segment is a number (`linux: N[ + D]`) | `internal/syscall/windows` and `internal/syscall/windows/registry` (`linux: n/a`) |
| darwin | **236** | every banked row in `eligible-darwin.txt` | the same two |

The roster has 238 rows, and all 238 carry a `linux:` segment. 228 carry a numeric `darwin:` segment. The other 8
darwin-eligible rows carry none, and the leg reads them as well, because each could bank now:
- `crypto/sha1`, `internal/coverage/cfile` and `testing` are the subject of this release's darwin harness seat (#8);
- `internal/synctest`, `internal/trace`, `runtime`, `runtime/pprof` and `syscall` did not bank at 1.26.10.1.

The four population rows outside the roster (the E-class exclusions) are unbanked. The sweep cannot reach them, and
neither leg dispatches them.

### The shards

| OS | Lane | Shard | Rows | Projected | Holds |
|:--|:--|:--|--:|--:|:--|
| linux | R-WSL | RWSL-1 | 14 | 2.92 h | the reserved set: every `$longTimeouts` key at the tip, `internal/poll` included; `runtime` 1.0 h, `internal/poll` ~0.5 h |
| linux | R-WSL | RWSL-2 | 129 | 1.11 h | the rows the WSL box reads far below the cloud's per-row floor |
| linux | P1 | P1-1, P1-2 | 23, 23 | 2.43 h, 2.51 h | `testing` last in P1-2 |
| linux | P2 | P2-1, P2-2 | 24, 23 | 2.58 h, 2.45 h | `crypto/x509` in P2-1 |
| darwin | C1 | C1-1 … C1-12 | 233 | 0.37–1.72 h each | the alphabetical waves, 19 or 20 rows each, `testing` last in C1-12 |
| darwin | C1 | C1-13, C1-14, C1-15 | 1 each | 1.70, 0.39, 0.72 h | `runtime`, `sync/atomic` and `net`, each in a dispatch of its own |

**Lane totals:**
- R-WSL: 143 rows, 4.02 h, after R's windows shard;
- P1: 46 rows, 4.94 h;
- P2: 47 rows, 5.03 h;
- C1: 236 rows in 15 dispatches. The slower RID's walls sum to 12.8 h, and both RIDs together are about 20 h of
  mac-job time.

**Where the walls come from.** Every wall is a go1.26.10 measurement from the 1.26.10.1 re-bank, and its source is
named on the row in `<LEGS>`. Linux walls are of two box classes, because the cost of a row depends on the box that
reads it:
- **WSL walls** come from R's WSL evidence: RWSL-1, RWSL-L2 and RWSL-R, plus P1-1 to P1-3, which R's WSL read at
  1.26.10.1;
- **cloud walls** come from the container evidence: P2-1 to P2-3, P2-L1, P2-L2, P2-R, P1-L2, P1-L2-poll and P1-R.

The latest wave wins. The 35 rows read on both classes put a cloud row at its WSL wall plus about **275 s**, the
per-row floor of a container read. A row measured on one class only is estimated on the other by that offset, and the
estimate is marked `est:`. 116 of the 236 linux lines carry an estimate, most of them small rows estimated on WSL.

Darwin walls are the slower RID's per-row wall from the latest 1.26.10.1 os-matrix `sweep-shard` run that read the
row (the `[Ns]` on each row line of the job log), with 300 s per dispatch for the build. `log/slog` has no darwin wall
in a readable log, so it takes its linux cloud wall.

**The split.** The rows whose cloud cost is least inflated go to P1 and P2, dealt longest-first. The search minimizes
the latest finish, with R-WSL starting after R's windows shard. Keeping R-WSL to the reserved set alone (1.26.10.1's
shape) projects P1 and P2 to about 10.3 h each.

**Disk.** The containers have about 30 GB of disk, so every row that is heavy on disk or memory is in RWSL-1. That
includes `archive/zip`'s two 4 GiB streams, `runtime`, the TLS and HTTP rows and `index/suffixarray`. The largest
cloud row projects to 12 minutes (`runtime/pprof`).

### The critical path

- **Linux, about 5.0 h from dispatch:** P2 at 5.03 h, with R tied. R is its windows shard, measured at 0.79 h in
  1.26.10.1's sweep and taken as 1.0 h with the canary, then R-WSL at 4.02 h.
- **Darwin, about 4–4.5 h:** the account runs 5 mac jobs at once (1.26.10.1's D-W1 start times), so about 20 h of
  mac-job time needs at least 4 h. Dispatch C1-13 (`runtime`) first. Its osx-x64 wall ranged from 1,808 to 5,811 s
  across 1.26.10.1's reads. Dispatch C1-15, C1-3 and C1-11 next.
- **The projections exclude each shard's clone and converter build**, which were not measured here. Add about
  15 minutes per linux shard.

### The closing checks

- **Linux:**
  - the `linux:`-annotated set equals the linux-eligible set by name: 236, and `n/a` stays on its 2 rows;
  - every numeric segment equals that row's 1.26.10.2 reading (PASS, `matched[ + disclosed-divergent]`, with the
    host-fatal entries in scope subtracted, as `annotate_linux.py` computes it), so the residual set is EMPTY.
- **Darwin:**
  - a row banks a `darwin:` segment only when osx-arm64 and osx-x64 agree verdict for verdict: both read
    `validated`, with equal matched and equal disclosed-divergent counts (`divergent - new`);
  - a row whose two RIDs disagree only on an oracle-side environment difference banks only under a named agreement
    ruling (`$DarwinAgreementRulings`; `net` carries one);
  - the `darwin:`-annotated set equals the darwin-eligible set by name: 236. Every eligible row that does not bank
    is named with both RIDs' readings, and its segment is REMOVED, never carried from 1.26.10.1;
  - every numeric segment equals its 1.26.10.2 reading on both RIDs.
- **Per OS:** read + not dispatched = 236, each row exactly once. Every darwin run's `headSha` and every linux
  TSV's `tree` column read `f75e0c53ec`.

### The lane briefs (one block per lane, ready to paste)

`<GOROOT>` is the go1.26.10 root, spelled exactly as `go env GOROOT` prints it on that box. `<TREE>` is a clone
detached at the tip. `<LEGS>` is `docs/phase4/data/rebank-1.26.10.2/legs.tsv`.

#### R-WSL (after R's windows sweep post; one battery per box)

```
LANE R -- 1.26.10.2 RE-BANK, LINUX READING. TIP f75e0c53ec523161f1990db7a407c9249c9ed326.
  Plan: docs/PLAN-release-1-26-10-2.md "The re-bank legs"; the act is docs/PLAN-rebank-go126.md §12's R block.
WHEN: ONLY after R's windows sweep shard has posted (one battery per box).
SHARDS RWSL-1 (14, the reserved set) then RWSL-2 (129): <LEGS>, rows with lane R-WSL, in seq order. Write each
  list from the file. Never retype it.
HOST CHECKS, before row 1, recorded in the ledger:
  - TMPDIR is 10 bytes or less (/tmp);
  - <GOROOT> carries test/;
  - the toolchain triple reads go1.26.10, with the .NET version and the uid;
  - nslookup invalid.invalid answers NXDOMAIN (before net);
  - free disk >= 25 GB, re-checked at every row boundary (a WSL VHD at ENOSPC remounts the distro read-only).
LINUX ENV and LINUX ACT: exactly §12's R block, in a clone detached at the TIP whose src/core is the seed, checked
  clean (tracked AND untracked under tracked paths) before row 1:
  - the argv is DERIVED by AST from the TIP's run-h10-recon.ps1 functions and run-validated-sweep.ps1 tables, never
    typed: runtime 120m/60m, internal/poll 30m/30m, sync/atomic 150m/150m, crypto/dsa 120m/120m, go/parser 90m/90m;
  - rc on the next line; restore the row's dirt BY STATUS; purge build output.
EVIDENCE: docs/phase4/rebank-1.26.10.2/linux/RWSL-1.tsv and RWSL-2.tsv, each with its .ledger.tsv:
  - columns: row word verdicts sweep_s first_in_list rc diverged platform tree wall_s post_s matched disclosed;
  - on lane ref claude/r-linux-1262, pushed as each shard ends, then ls-remote; raw JSON kept outside the clone.
POST (one per shard, <=40 lines):
  - WHAT: lane, shard, TIP, ref@SHA;
  - EVIDENCE: the triple; rows run against listed; the word tally; each row's matched (+ disclosed) BESIDE its
    linux: segment, every row that moved named; every non-PASS row with its cause; each TIMEOUT with its
    results-tail line; the host checks; the residue before and after;
  - NEXT.
```

#### P1 and P2 (linux, cloud containers)

```
LANE P1 (P2 the same, with its own shards) -- 1.26.10.2 RE-BANK, LINUX READING.
  TIP f75e0c53ec523161f1990db7a407c9249c9ed326.
  Plan: docs/PLAN-release-1-26-10-2.md "The re-bank legs"; docs/PLAN-rebank-go126.md §12's R and P1/P2 blocks.
SHARDS P1-1, P1-2 (23 each; testing LAST in P1-2) | P2-1 (24), P2-2 (23):
  - <LEGS>, rows with your lane, in seq order;
  - write each list from the file; never retype it. No reserved row is yours.
HOST CHECKS before row 1: TMPDIR <= 10 bytes; <GOROOT> carries test/; the toolchain triple (go1.26.10), .NET, uid.
LINUX ENV and ACT: §12's R block, in a clone detached at the TIP, checked clean; the argv derived by AST at the TIP.
  P2: crypto/x509 runs with SSL_CERT_FILE and NIX_SSL_CERT_FILE UNSET on both sides, and the ledger line says so.
CONTAINER RULES (about 30 GB of disk; tmpfs wiped on restart; OOM history):
  - free disk >= 15 GB at every row boundary (linux-legs.sh's floor); purge build output after every row;
  - the resume ledger on PERSISTENT disk, one line per row, keyed on the corpus commit, the converter commit and
    the binary's mtime;
  - after a restart, re-enter at the first unrecorded row, and re-run a killed row at a RAISED budget;
  - commit and push each shard's TSV and ledger the moment the shard ends.
EVIDENCE: docs/phase4/rebank-1.26.10.2/linux/<shard>.tsv (+ .ledger.tsv):
  - columns as R-WSL's;
  - on lane ref claude/p1-linux-1262 (P2: claude/p2-linux-1262); ls-remote after each push.
POST: one per shard, as R-WSL's, plus the container's restarts during the shard.
```

#### C1 (darwin, hosted runners)

```
LANE C1 -- 1.26.10.2 RE-BANK, DARWIN READING. TIP f75e0c53ec523161f1990db7a407c9249c9ed326.
  Plan: docs/PLAN-release-1-26-10-2.md "The re-bank legs"; GoCorpusMigration.md H10, Amendment 2026-10-24 item 5.
FIFTEEN DISPATCHES: os-matrix.yml, stage sweep-shard, goos darwin (both mac runners, osx-arm64 and osx-x64), filter =
  the dispatch's rows from <LEGS> (lane C1), comma-separated, EXACT:
  - C1-1 .. C1-12: the alphabetical waves of 19-20 rows;
  - C1-13 runtime, C1-14 sync/atomic, C1-15 net: one row each.
  Dispatch order: C1-13 first, then C1-15, C1-3, C1-11, then the rest (5 mac jobs run at once).
  Every row is banked, so sweep-shard reaches all 236 at the TIP. A count red against the roster is the reading's
  signal, not a stop.
THE TIP: dispatch from a ref whose head IS f75e0c53ec, and commit nothing to that ref while runs are queued. A run
  whose headSha is not f75e0c53ec is not evidence.
A ROW BANKS a darwin: segment only when osx-arm64 and osx-x64 agree verdict for verdict:
  - both `validated`, equal matched, equal disclosed-divergent (divergent - new);
  - the segment is that count, read from the row's own artifacts on both RIDs;
  - a row that does not bank carries NO darwin: segment;
  - the 8 rows with no segment today are read like every other row.
EVIDENCE: one TSV per dispatch, docs/phase4/rebank-1.26.10.2/darwin/<dispatch>.tsv:
  - columns: shard row RID verdicts matched disclosed divergent new status agree win_expected win_disclosed
    darwin_seg win_differs;
  - one line per row per RID;
  - on lane ref claude/c1-darwin-1262; ls-remote after each push.
POST: one per dispatch:
  - WHAT: the dispatch, the run id, the headSha;
  - EVIDENCE: rows agreeing on both RIDs, with their counts beside the current darwin: segments; rows disagreeing,
    by name with both counts; rows the sweep could not reach; each RID's job wall;
  - NEXT.
```

### Rulings (COORD, 2026-10-27)

1. **Evidence location:** the legs write under `docs/phase4/rebank-1.26.10.2/linux/` and `.../darwin/`, which
   collides with nothing from 1.26.10.1. Before the annotation seat, `annotate_linux.py`, `annotate_darwin.py` and
   the roster guard's darwin check gain an evidence-directory parameter (a tooling seat, run in parallel with the
   legs), so this release's readings are the ones read.
2. **The split stands.** R-WSL keeps 143 rows because its small-row cost is a fraction of a container's. If R-LAPTOP
   stalls, its unrecorded RWSL-2 rows move to P1 and P2 from the resume ledger; R posts at each shard's end.
3. **P1 runs its canary row first.** If its container cannot hold the shard, P1's rows move to R-WSL after RWSL-2.
4. **The 8 unannotated darwin rows are read.** Each banks a `darwin:` segment only when both RIDs agree.
5. **`internal/poll` on R-WSL** is accepted as derived.
6. **C1 writes the darwin TSVs** from its own dispatches' artifacts.
7. **`runtime`'s darwin projection keeps the larger wall.**
8. **The estimated linux walls are the weakest numbers here,** and the critical path is read with that margin.
9. **A container needs room before its first row:** a lane whose free disk is under 20 GB clears its own caches
   first (the Go build cache and the NuGet locals). If it still cannot reach 20 GB, its shards move to the other
   container, after that container's own.
