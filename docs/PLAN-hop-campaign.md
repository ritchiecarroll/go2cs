# PLAN — the hop campaign: the .NET 10 and Go 1.23.12 hops across this fleet

> **STATUS: RATIFIED (coordinator, 2026-08-22) -- all six SS7 OQs ruled AS RECOMMENDED (H5 noted as an ANCHOR OBLIGATION: the consolidation sweep retains per-row wall times); formerly PROPOSED** — drafted 2026-08-22. This is the **instance plan** for the two hops the ladder
> schedules after the anchor release. It supplies *this* fleet, *these* two releases, the concrete
> shard map, the sequencing, the adversarial pass and the open questions. It supplies **no
> procedure** — procedure lives in two runbooks and is referenced, never restated:
>
> - [`DotNetMigration.md`](DotNetMigration.md) — how to move the project to a new .NET release
> - [`GoCorpusMigration.md`](GoCorpusMigration.md) — how to move the corpus to a new Go release
>
> The **strategy** is fixed and lives in [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md): §0 is the
> ruling frame, §2 the H-series inventory, §4 the parity-gate definitions, §8 the nineteen ruled open
> questions. **Every ⟨OQ-n⟩ there is settled**; this plan cites those rulings and never reopens them.
>
> **Measured-bill-first.** Every number below carries its date, machine and source, or is named as a
> placeholder the hop supplies. Unlike the runbooks — which are deliberately figure-free because they
> outlive any corpus — this document is an instance and therefore quotes real readings.

---

## 0. The two hops, and what is already true

The frame orders the work: 75 % is Go 1.23.1's terminal marker → the **.NET 10 hop** → the **Go
1.23.12 hop** → 1.24.13 → an evidence-based decision on 1.25/1.26. This plan covers the first two.

**The terminal marker is crossed.** `docs/ValidatedTestPackages.md`'s header reads **162 / 215
testable packages validated — 75.3 %**, **18,569 matching test verdicts · 85 disclosed** (updated
2026-08-22), with **Linux: 4 of 162 rows validated at their Linux counts** (578 verdicts · 1
disclosed). The frame's precondition — "Complete 1.23.1 (130 → 162 rows)", `PLAN-corpus-upgrade.md`
§6 — is satisfied, and both hops are unblocked in principle.

### 0.1 A naming collision, named rather than inherited

`PLAN-corpus-upgrade.md` §6 letters the hops **A = 1.23.12**, **B = 1.24.13**, **C = 1.25.x**,
**D = 1.26.x**. The .NET 10 hop was added to the frame on 2026-08-16 (§0's *".NET 10 hop scope
additions"*) and **precedes** hop A, but received no letter. Two documents lettering one sequence
differently is a coordination hazard, so this plan fixes the spelling:

| This plan | `PLAN-corpus-upgrade.md` | Runbook | What moves |
|:--|:--|:--|:--|
| **Hop N** | *(the ".NET 10 hop", unlettered)* | [`DotNetMigration.md`](DotNetMigration.md) | the .NET runtime and TFM. **No Go release moves** |
| **Hop A** | Hop A | [`GoCorpusMigration.md`](GoCorpusMigration.md) | Go 1.23.1 → 1.23.12. **No .NET release moves** |

### 0.2 Two instance facts that remove work from hop A

Both are source-verified on this checkout, and both narrow the migration runbook's general case:

1. **Zero release-tag delta.** `releaseTagsForVersion` (`src/go2cs/directiveOperations.go:264`) trims
   any patch suffix (`minor[:end]`, lines 271–278), so `go1.23.1` and `go1.23.12` both expand to
   `go1.1 … go1.23`. **No `//go:build go1.NN` guard flips.** `GoCorpusMigration.md` §1.1's channel 1
   is therefore inert for this hop, and its H9 golden diff should be small or empty (§4.3).
2. **Zero language delta and zero package delta.** `PLAN-corpus-upgrade.md` §1.3: *"The 1.23 →
   1.23.12 rehearsal hop has **zero** language delta. That is the point of it."* Same minor, same
   package set. H4 should be ~empty and H3's census should come back **∅** — a non-empty census here
   is a finding.

> **Both facts EVIDENCED against the actual upstream range (recon, 2026-08-23,
> [`phase4/RECON-go12312-diff.md`](phase4/RECON-go12312-diff.md)):** 83 commits, 161 files, ~59
> stdlib-visible across ~20 packages; every "added" file is testdata or a `_test.go` in an
> existing package, so H3 = ∅ is now measured, not predicted. The recon also pre-loads the hop's
> attention list: **9 of 162 validated rows intersect the range, 4 HIGH** (`syscall` — CVE fix +
> two direct hand-own file overlaps for H6's differential; `os/exec` — LookPath security fix +
> 27 host-limit disclosures to re-derive; `database/sql` — a real Rows/Scan race fix; `time` —
> test-only upstream, but the new Stop/Reset tests probe the hand-owned managed timers). The
> other 153 rows re-run at banked counts; the 7 Linux-annotated rows intersect nowhere. Seven
> CVE-class fixes ride the hop (a security-posture argument for shipping it), `crypto/tls`'s
> banked suite stands on certificates upstream fixed for wall-clock expiry (a future sweep
> false-red with a ready explanation — the hop levels it), and `reflect`'s Seq/Seq2 fixes are
> the most converter-relevant behavior change: the corpus currently reproduces the 1.23.1 bug
> faithfully.

What hop A buys is the **machinery**: the pin bump exercised for real, the hand-own `.auto`
differential's first run, the badge churn, the release ritual, and H10 executed as a parallel fleet
campaign rather than a single serial run.

---

## 1. This fleet

**Re-measure before sizing a shard map** — every row is a reading, not a constant.

| Machine | Role | What it is | Measured |
|:--|:--|:--|:--|
| **Desktop (coordinator)** | assigns lanes, merges, signs, lands; owns the mailbox, every full-roster dispatch and every ruling | i7-5820K, 6C/12T, 32 GB, Haswell-E (2014); replaced the i9-13900K that died 2026-08-09 | CNR **1,059 / 1,132 s solo, 1,440 / 1,711 s loaded** at ~625 packages (2026-08-17/19); behavioral suite **~6,552 s at 603 packages** (2026-08-21); `go2cs.slnx` Debug `--no-incremental` **~3,546 s at 722 projects** (2026-08-21); `go2cs-stdlib.slnx` **516 s** (2026-08-14) |
| **The i9 — "the sweeper"** | JOB-served worker; runs named instruments at stated budgets, reports raw output. **Makes no rulings, never commits to master.** Joined 2026-08-21 17:16 | **i9-13900K, 16C/24T, 64 GB**, `C:` 446 GB free; SDK 9.0.317, Go 1.23.1; clone at `C:\go2cs-build\repo`, **every job in its own `git worktree` at the SHA the job names** | Full roster (159 rows, JOB-001): **159/159 PASS in 7,059 s (~117.6 min)** — *above the ~50–60 min baseline the budget table still carries*. JOB-R3 (39 rows): **39/39, 14,148 verdicts, 2,431 s** summed row time. JOB-G2: **162/162, exactly 18,569 verdicts**, clean first pass. JOB-G1, same roster: **160/2** first pass, both reds retried green in isolation |
| **Laptop R** | lane machine; holds the WSL2 Ubuntu-22.04 distro running the Linux legs | Ryzen 7 PRO 6850U, 32 GB; distro 16 threads, 15 GB | CNR **1,060 s**; the 161-row Linux roster re-run, per-row, detached |
| **Laptop G** | lane machine; ran the Linux measurement campaign v1–v4 and **the .NET 10 perf scout** | Ryzen 7 PRO 6850U, hostname `G-LAPTOP` | CNR **720 s**; behavioral suite **1,792 s** (2026-08-07, i9-era corpus) |
| **The perf-canon box** | dedicated, solo, sleep-proofed; owns the canonical AOT performance column. **A separate machine from laptop G** — the scout's method line says so | AMD Ryzen 5 PRO 6650U, 6C/12T, 30.8 GB; Win 11 10.0.26200, SDK 9.0.316, MSVC 14.44 | the bflat exploration's numbers (§3.4) and the canonical three-column table |

⚠ **Two roster facts to verify before a dispatch, not to assert.** (1) The **R / G labels appear to
have been reassigned** between 2026-08-11 (board: laptop G "Ryzen 5 PRO 6650U, 6C/12T") and
2026-08-21 (board: laptop G "Ryzen 7 PRO 6850U … **NOT** the perf-canon 6650U host") — both R and G
now read as 6850U. The later, explicitly-disambiguating entry should win, but a shard map that
mis-identifies a worker mis-sizes its bin. (2) **No canonical fleet-roster document exists.**
[`phase4/LANES.md`](phase4/LANES.md)'s assignment table is frozen at 2026-08-11 ("Tonight's fleet:
three machines") and **does not mention the i9 at all**; `MAILBOX.md`'s protocol header still reads
"the laptop lanes (R, G)" and predates the i9's arrival by hours. Both are *right about protocol* and
*stale about membership*.

**The fleet is expandable.** §4.2's shard map is parameterized on worker count `W`, sized for
**W = 3…6**.

### 1.1 The bus, and the two mechanics the campaign cannot run without

Protocol is [`phase4/MAILBOX.md`](phase4/MAILBOX.md), branch `claude/mailbox`: append-only, pull
before appending, push immediately after, union on conflict, **poll at session start and before final
gates**. Transport, not record.

⚠ **The file in a master checkout is the protocol header and little else; the live operational record
is on the branch** — `git show origin/claude/mailbox:docs/phase4/MAILBOX.md`.

- **A JOB entry must name a tip the worker can fetch.** A set was *not* dispatched to the i9 on
  2026-08-22 for exactly this reason. **A shard dispatch naming an unpushed SHA is a dead dispatch.**
- **A session with capacity remaining ARMS A WATCHER as its turn's last action** (protocol addition,
  2026-08-22): *"the mailbox cannot wake an idle session … measured: a **40-minute gap** between an
  assignment posted and a lane with declared capacity."* A background loop over
  `git ls-remote origin refs/heads/claude/mailbox`, ~150 s sleep, **exit when the tip moves**; written
  **positively**; **armed after your last push** so it does not fire on your own signal. Across `W`
  workers and several shards each, this is the campaign's dominant idle time if skipped.
- **Mailbox-branch commits may be UNSIGNED** (user ruling, 2026-08-22) — scope exactly
  `claude/mailbox`, and the reason is this campaign's: *"so a rebooted machine (the sweeper daily, any
  box after a crash) can post ACKs and results without a human passphrase entry."* Everything else
  stays signed. ⚠ **The mechanism is not yet exercised on the i9**: the worktree-scoped unsign
  (`git config extensions.worktreeConfig true` + `git config --worktree commit.gpgsign false`) is
  refused by the harness's auto-mode classifier as a security-setting change and is **user-hands-only**.
  **Resolve it before H10 dispatch** — see §6 lens 3.

### 1.2 The worker contract, as this fleet's sweeper declared it

`GoCorpusMigration.md` §3.6 carries the generalized contract. The i9's own words, on arrival
(mailbox, 2026-08-21), are what it was generalized from:

> *"I run named instruments at stated budgets and report raw output … sweep dirt classified ONLY
> against CLAUDE.md's documented classes; anything else is posted raw as **UNCLASSIFIED** for you to
> rule on. I make no rulings and never commit to master. A run that exceeds its stated budget is
> killed and reported **TIMEOUT** with the log tail — **I do not extend a budget on my own**."*

**Capacity: 3 concurrent jobs in separate worktrees, 4 if none is a full suite.** That is this fleet's
concurrency unit — and per `GoCorpusMigration.md` §3.1 it is *worktrees*, because
`run-validated-sweep.ps1` is serial by design (`src/run-validated-sweep.ps1:199`).

**Reboots are declared, not exceptional:** *"⚠ This box reboots randomly (~daily, pending RMA). That
is expected. On session start after a restart I re-poll and re-run anything I ACK'd but never posted a
result for, so a job lost to a reboot costs a re-run, not a gap."*

**One rule this fleet has already paid for twice, worth naming here rather than in a runbook:** the
worker's outer wrapper must clear the instrument's internal budget. The i9's `timeout -k 10 8m`
wrapper cut a healthy `crypto/tls` row (exit 124) that then passed **400 verdicts in 669 s** under a
15-minute wrapper.

### 1.3 What this fleet's Linux workers need

The **F15 distro recipe** ([`PLAN-linux-operation.md`](PLAN-linux-operation.md) §F15), re-measured at
**~4 min including the clone** on laptop R: Go 1.23.1 from the official tarball into `$HOME/golang`,
the .NET SDK via `dotnet-install.sh --channel 9.0 --install-dir $HOME/.dotnet`, PowerShell as a global
tool **pinned to 7.5.x**. Two gotchas the recipe names: **GOROOT is `$HOME/golang`, not `$HOME/go`**
(the go command refuses `GOPATH == GOROOT`); and a bare `dotnet tool install -g powershell` resolves
7.4.x, which targets `Microsoft.NETCore.App 8.0.0` and **fails to launch against a 9.0-only runtime
set**.

**The C-toolchain line, added 2026-08-22** — *"the one non-user-space line"*: `apt-get install -y
build-essential`, after which `go env CGO_ENABLED` **must read 1**. A bare `Ubuntu-22.04` image does
not carry gcc, and without it every cgo-dependent **Go-side** baseline misreports. Measured effect:
`debug/buildinfo` 197 → 204 Go verdicts, `go/internal/gcimporter` 581 → 582, `go/internal/srcimporter`
7/7 Go-pass — **C# side unchanged in every one**, so a Go-baseline effect, not a corpus one. ⚠ It also
**re-exposes `plugin`'s W3 converter crash** (`conversionDriver.go:228`; the same conversion under
`CGO_ENABLED=0` exits 0 — so an earlier `PASS plugin 1` was the CGO-off artifact).

**Also inherited from the Linux campaign** (board, 2026-08-21, *"Campaign infrastructure — what later
Linux campaigns inherit"*): `_paths.ps1`'s platform pins; the ICU-safe duration table (both micro
signs folded to one key under pwsh/ICU on Linux and killed every sweep that reached the parser); the
sweep's `-Exact` filter; per-package log retention with an idempotent resume ledger; the two-monitor
watch pattern. The `GO2CSPATH` case-insensitive child-environment race is **fixed at the converter**
as of 2026-08-22, and **the Linux harness pin in `_paths.ps1` stays** until a Linux lane re-measures
without it.

⚠ **The ledger and the per-row driver are lane-local scratch, not committed instruments.** `src/`
holds no such script; the only artifact named by path is G's `/root/campaign-logs/verdicts.txt`, whose
**format is nowhere specified**, on a distro since force-reset. Campaign v4 adopted retention because
**v3 lost its 8 non-CS failure shapes to a `/tmp` overwrite**. Four lanes have each rebuilt this;
H10 needs up to six workers running the same one. ⟨OQ-H1⟩.

---

## 2. What each hop does and does not trip

| Guard | Hop N | Hop A |
|:--|:--|:--|
| `checkCorpusToolchainPin` — exact equality on the FULL Go release | **inert** (no Go release moves) | **FIRES.** `1.23.1 → 1.23.12` is a patch move and the comparison includes the patch. The guard's own message prescribes H2-before-H5 |
| `checkNuGetStdLibCompatibility` — `version.Lang` only | **inert** | **inert** — `1.23.1` and `1.23.12` share a `Lang` of `1.23` |
| False-green route #4 (stale `go2cs.exe` after a **Go** toolchain hop) | **inert** — hop N installs no Go toolchain | **FIRES.** OQ-6's stamp remedy lands **before** hop A, not inside it |
| Release-tag emission | inert | **inert** — §0.2, source-verified |
| **The embedded-asset stale binary** ([`DotNetMigration.md`](DotNetMigration.md) §5.2) | **COVERED** — hop N's TFM stage still edits both csproj templates and eight publish profiles, but the predicates now see them | covered; fires nowhere |

**The embedded-asset route is new and this plan is where it was found**, so it is stated once, here,
and in full in the runbook: `src/go2cs/embeddedTemplates.go` `//go:embed`s the csproj templates, the
`package_info.cs` skeleton, the icons and `profiles/*` (and `stdlibMetadata.go` embeds
`stdlib-metadata.txt`); all three rebuild predicates filtered on top-level `*.go` only
(`BehavioralRunner/Program.cs:332`, `BehavioralTests/BehavioralTestBase.cs:145`,
`PerformanceRunner/Program.cs:275`) — 204 files seen against 224 real inputs. **The remedy landed
2026-08-22, ahead of and independent of OQ-6's toolchain stamp**: `src/tests/ConverterBuildInputs.cs`,
one build-input set linked into all three predicates, with the embedded half derived from the
`//go:embed` directives and guarded under the converter's `go test ./...`
(`src/go2cs/embeddedAssets_test.go`). CNR turned out **never to have been exposed** — it has no
rebuild predicate because it runs `go build` unconditionally, and `go build`'s cache is
content-addressed over embedded assets (A/B-verified on the linked binary's hash). Route #5 in
`CLAUDE.md` carries the standing record.

---

## 3. HOP N — the .NET 10 hop

> **EXECUTED — this section is now largely a RECORD (2026-08-24).** N0 (fleet provisioning,
> [`phase4/STAGE0-provisioning.md`](phase4/STAGE0-provisioning.md)), N1 (the SDK alone) and **N3 (the
> TFM)** are merged to master — Stage 2 at `925e48067`, the anchors table at `f630a6fde` — and **N5 is
> RESOLVED** at `1e51e9ad5` (§3.3's prediction answered: the bflat anomaly was preview/packaging, not
> .NET 10 codegen). The first execution was the runbook's own shakedown, so several of this section's
> generalizable lessons have since moved INTO [`DotNetMigration.md`](DotNetMigration.md) — that
> document leads on procedure, and the figures below stay here because they are this instance's.
> **N6 / N7 / N8 have not landed**; their rows below are still forward-looking, and the hop's ordering
> rule (§5's "hop N must complete, entirely, before hop A's H1") is unchanged by what has.

**Procedure: [`DotNetMigration.md`](DotNetMigration.md).** This section supplies only what is specific
to .NET 10 and to this fleet.

### 3.1 The measured bill

From the **.NET 10 performance scout** (board 2026-08-22, lane G, `claude/dotnet10-perf-scout`).
Method: SDK **10.0.400** side-by-side to a user-local dir, the machine's 9.0 default untouched; the 10
leg selected by `DOTNET_ROOT` + `DOTNET_ROLL_FORWARD=LatestMajor`, verified by a
`FrameworkDescription` probe (`.NET 9.0.18` → `.NET 10.0.11` → restored). **Both legs execute
identical IL.** Same day, same silicon (Ryzen 7 PRO 6850U, `G-LAPTOP` — *not* the perf-canon
6650U host; ratios internal to that box), quiet machine, `run-performance.ps1 --no-aot`, median-of-5.
Go columns reproduce across legs within noise (Fib 118.3 vs 119.0) — the same-day control the protocol
demands.

The scout's reading, quoted: **"a solid single-digit-to-20 % improvement across most of the corpus
with a >2× win on string-heavy code, financed by two narrow regressions to re-measure at hop time."**
Wins land where the transpiled corpus hurts most — String **1,278.2 → 615.4 ms (−52 %, 11.66× →
5.67× Go)**, StringView to Go parity (1.13× → 0.99×), MatMul −20 %, Map −23 % (0.88× → 0.68×).

**The three named regressions** — the hop's re-measurement obligation, not its blockers:

| Regression | 9-JIT → 10-JIT | Scout's note |
|:--|:--|:--|
| **Startup** | 243.1 → 285.0 ms, **+17 %** | JIT-path process start — *"AOT is the startup story anyway"* |
| **Channel** | 83.7 → 105.8 ms, **+26 %** | synchronization-heavy; *"worth a targeted look at the hop"* |
| **Iface / RefLower** | 523.4 → 567.2 and 605.2 → 654.2 ms, **+8 %** each | interface dispatch shapes |

**Rollout precedent:** the scout hit **no JIT-path friction** — official `dotnet-install` with
`-NoPath`, side-by-side clean, `net9.0` IL on 10.0.11 under `LatestMajor` first try, zero
NETSDK/analyzer noise, the netstandard2.0 `go2cs-gen` analyzer loading unmodified.

**The four traps, instantiated** ([`DotNetMigration.md`](DotNetMigration.md) §3 states them generally):
(1) SDK 10.0.400 publishing `net9.0` resolves `Microsoft.DotNet.ILCompiler/9.0.19` — 9-AOT Fib
**178.2 ms** (964 s publish) vs "10-AOT" Fib **177.1 ms** (1,138 s publish), *identical and
structurally so*; (2) the runner reused a stale publish across an SDK-env change — **"a 51 s '10-AOT
leg' re-measured the 9-ILC binary"**, and on this corpus a real publish is 964–1,138 s on a laptop and
~25 min on the i7-5820K, so **a 51-second publish is the tell**; (3) Roslyn 10 warns CS7022 on
`PerformanceRunner`'s top-level-statements + `Runner.Main` shape (benign); (4) `net9.0` under the 10
SDK still runs on 9 without explicit selection.

### 3.2 The stage sequence for this hop

| Stage | Changes | Runbook | Owner |
|:--|:--|:--|:--|
| **N0** | fleet provisioning; nothing in the repo | §2 | every machine; probes recorded on the mailbox |
| **N1** | the SDK only | §4 | coordinator |
| **N2** | nothing — the .NET 9 baseline capture | §6 | perf-canon host, SOLO |
| **N3** | the TFM (one line) | §5, **incl. §5.2** | coordinator |
| **N4** | nothing — the .NET 10 CPU measurement | §6 | perf-canon host, SOLO |
| **N5** | nothing — the AOT / ILC verification | §7 | perf-canon host, SOLO, **hours** |
| **N6** | golib trim-safety annotations | §8 | own gate cycle |
| **N7** | the test-host publish shape | §9 | own gate cycle; **the roster arithmetic moves** |
| **N8** | the deployment-shape ruling | §10 | coordinator |

**Instance budgets** (CLAUDE.md's table, i7-5820K unless noted): `go2cs-stdlib.slnx` ~516 s per
flavor; `go2cs.slnx` ~3,546 s `--no-incremental`; behavioral suite ~6,552 s — ⚠ **the runner's own
`--build-timeout` default of 2,400 s hit its budget at 604 projects** on this class and needed
**9,000 s** to build clean in one shot; converter `go test ./...` 192–309 s across the 2026-08-22
lanes.

**Not owed at N1 or N3**: `check-no-regression.ps1` and the converter's `go test ./...` — CNR measures
converter emission and the converter is a Go binary. They become owed the moment N3 edits an embedded
asset (§2), and N7 owes the converter test run because it changes
`unsupportedRuntimeCapabilities`. State the accounting; do not skip silently.

### 3.3 N5 — the bflat breadcrumb, and the prediction that closes it

The scout's attribution, verbatim, because it is what makes this a *to-verify* rather than a claim:

> bflat's Fib **70.9 ms** is NOT generic .NET-10 codegen: the 10-JIT Fib is **161.5 ms** (−11 %,
> nothing like halving). […] the bflat halving requires the net10 ILC+framework **pair** bflat ships;
> nothing reachable from the net9.0 corpus reproduces it. **It becomes measurable exactly AT the hop,
> and stands priced as a to-verify upside there.**

Corroboration: bflat v10.0.0-rc.1 ships the .NET 10.0.0-rc.1 ILC and framework, ran Fib in **70.9 ms**
against the SDK's **175.6**, reproduced in an independent 11-run pass (71.4 vs 175.7, Go 120.5), ~14 %
faster on String ([`PLAN-bflat-perf-exploration.md`](PLAN-bflat-perf-exploration.md) Finding 4).

**The falsifiable prediction, stated before the run** (as `DotNetMigration.md` §7 requires): once the
TFM is `net10.0`, the SDK's publish resolves an ILC 10 runtime pack and the AOT Fib row should
approach ~71 ms rather than sitting at ~176 ms. **If it does, the halving was .NET 10's ILC and bflat
closes forever as a data point.** **If it does not, bflat's advantage is something else** and the
finding reopens with a sharper question.

**RESOLVED — it did not.** 174.7 vs 175.3 ms (−0.3 %) with the control row at 0.0 %: the advantage is
attributed to the **preview/bflat packaging**, and the anomaly is closed as not-a-hop-question. The
measurement, the prediction as stated before the run, and the two riders it established — the
10-ILC's ~10.6× near-serial compile cost and its ~15 GB working-set floor — are banked in
[`phase4/DATA-hopN-perf.md`](phase4/DATA-hopN-perf.md) §2, which is where this hop's numbers live now
that [`DotNetMigration.md`](DotNetMigration.md) holds only the protocol.

### 3.4 N6 — the trim-safety numbers this hop inherits

From the concluded exploration (laptop G, 2026-08-16, Ryzen 5 PRO 6650U — the perf-canon box):

| Quantity | stock `A0` (`TrimMode=partial`) | `X2` (`A1` + `TrimMode=full`) | bflat `B1` | Go |
|:--|--:|--:|--:|--:|
| binary on disk (Startup) | **288.26 MB** | **8.77 MB** | 8.73 MB | 2.12 MB |
| startup wall (15-run pass) | 79.7 ms (**3.25×** Go) | 33.2 ms (**1.36×** Go) | 34.5 ms | 24.5 ms |
| peak working set | 74.4 MB | **6.8 MB** | 6.7 MB | 2.5 MB |
| one benchmark's publish | 977–1,049 s | **28–29 s** | 28–38 s | — |

*"Of the ~279 MB between the stock profile and bflat, ~99 % is `TrimMode=partial` and ~1 % is the
feature switches."* The feature-switch profiles move **neither** startup nor working set materially
and are *slower* to build (~1,600 s vs ~1,000 s).

**94 trim-analysis diagnostics** at the time of measurement, **22 of them IL3050** applying to the
*existing* AOT column, concentrated in `golib/TypeExtensions.ExtensionMethodRegistry.cs` (16),
`golib/GoReflect.FieldAccess.cs` (14), `golib/AdapterBinder.cs` (14), `golib/GoReflect*.cs` (18 across
3 files), `core/reflect/value_impl.cs` (8), `golib/TypeExtensions.GoMethodSets.cs` (8), and others
(16). **Re-measured at the hop** — a new ILC adds and drops diagnostics.

`SuppressTrimAnalysisWarnings=true` is at `src/tests/Performance/Directory.Build.props:12`. The
frame's disposition (2026-08-16): *"it stays in the perf tree's defaults … the suppression must never
be the reason the diagnostics go unread."* **The audit build overrides it locally and never edits it.**

### 3.5 N7 — this fleet's `host-limit` constituency, and what each entry needs

The frame makes the retirement a hop deliverable (`PLAN-corpus-upgrade.md` §0, ruled 2026-08-19):
*"`host-limit` retires BY the .NET 10 hop (single-file host publish is hop scope)."*
[`DotNetMigration.md`](DotNetMigration.md) §9 is the pattern; this is the instance, **measured from the
committed manifests, 2026-08-22**:

| Package | entries | Lever it needs |
|:--|--:|:--|
| **`os/exec`** | **25** pinned leaves | **Relocatability.** `TestCommand` / `TestLookPathWindows` copy the test executable and run the copy. The 2 parents ride disclosed-parent aggregation, so the row banks **74 matched + 27 disclosed**. A self-contained single-file publish retires all 27 |
| **`crypto/tls`** | **1** (`TestBogoSuite`) | **Startup speed, NOT relocatability.** The BoringSSL runner spawns the shim once per case — **5,481 spawns** — inside a child `go test`'s default 10-minute deadline the test cannot express. The converted host starts in **3.30 s** against Go's **0.038 s** — **~87×**. Measured 2026-08-18: the runner reaches **~267 of 5,481** cases at its wall even with 16 parallel shim workers, *"a ~20× shortfall no deadline the test can express would close."* The manifest names its own lever: *"publish the host with fast startup (ReadyToRun/AOT) and this row starts passing"* |
| **`runtime/debug`** | **1** (`TestStack`) | **NOTHING. Structural and permanent.** Ruled 2026-08-21: *"unlike the os/exec relocatability entries, it does NOT retire at the .NET 10 single-file host, and the entry must say so"* — and the manifest's `reason` already does |

**Two levers, not one**, and this is the fact most easily missed: a self-contained single-file publish
retires os/exec's 25 and does **not** by itself retire crypto/tls's row. A hop that ships single-file
without ReadyToRun retires 25 and leaves 1 standing — honest, but only if predicted.

**A fourth site, and it is a GATE, not a disclosure.** `src/go2cs/testConversion.go:2890` lists
`os_test.TestRemoveAllWithExecutedProcess` in `unsupportedRuntimeCapabilities` with the reason
*"relocatable single-file test executable"*, and prices the remedy: *"publishing every converted test
host self-contained single-file — **~70 MB and a publish rather than a build, per package**."* The map
is guarded by `TestUnsupportedRuntimeCapabilityGate`. **N7 removes that entry too** — and the same
comment records that `os` is not yet on the roster, *"its disposition is decided when it banks."*

**Consequences for the arithmetic** (the ordered pattern is `DotNetMigration.md` §9.3): `os/exec`'s 27
disclosed verdicts become matched; its manifest empties and **the file is removed** (the
`chan-direction` precedent); the roster header's `18,569 · 85` moves in **both** components;
`crypto/tls` re-derives **only** if the startup lever ships, and its manifest's `hostConditional` note
makes one reading insufficient by declaration; `runtime/debug`'s entry stays, and a *pass* there is an
investigation. `docs/Roadmap.md`'s retirement-path section becomes a record rather than a proposal.

**The cost lands on exactly the step hop A parallelizes** (H10), at ~70 MB and a publish per package
across a roster of 162 rows. ⟨OQ-H4⟩.

### 3.6 N8 — this hop's evidence checklist

`DotNetMigration.md` §10 is the generic checklist. This hop's instantiation adds three rows the
general case cannot know:

| # | Evidence | Bar |
|:--|:--|:--|
| **E-a** | The bflat Fib question (§3.3) | answered either way, on the perf-canon host |
| **E-b** | `crypto/tls`'s `TestBogoSuite` under the new shape | **three runs, two machines** — its `hostConditional` note makes one insufficient |
| **E-c** | The three named regressions (§3.1) re-measured | reported whether or not they reproduce; a regression that vanished on other silicon is a finding about `G-LAPTOP` |

---

## 4. HOP A — Go 1.23.12 as a fleet campaign

**Procedure: [`GoCorpusMigration.md`](GoCorpusMigration.md).** This section supplies the machine
assignment, this hop's opening regen bundle, and the shard map.

### 4.1 The H-series mapped to machines

| Step | Owner / machine | Parallel? | Instance notes |
|:--|:--|:--:|:--|
| **H0** baseline capture | coordinator, desktop | no | Fresh `.cs.auto` baselines from a seeded 1.23.1 regen (ruled). CleanupBacklog item 18 levels separately |
| **H1** toolchain | every machine; coordinator verifies | **yes** | Go 1.23.12 side-by-side, `GOROOT/VERSION` confirmed. **OQ-6's stamp lands BEFORE this hop.** The embedded-asset predicate widening (§2) already landed 2026-08-22 and is no longer coupled to it |
| **H2** pin bump | coordinator, desktop | no | `<GoStdLibVersion>` → `1.23.12`; build number **resets** → the hop publishes `1.23.12.1`. **One reviewable pair with H1** |
| **H3** census | one lane | no | Expect **∅** (§0.2). Deliverable `docs/phase4/CENSUS-go1.23.12-packages.md` |
| **H4** converter work | — | — | **Expected empty.** H4a is this hop's converter work instead |
| **H4a** opening regen bundle | one lane, coordinator gates | no | §4.2 |
| **H5** seeded reconvert | coordinator, desktop | no | Marker census **re-measured**: last readings 53 (2026-08-14) and **66–67** across the 2026-08-22 lanes — it moves weekly |
| **H6** hand-own re-audit | one lane, coordinator gates | partly | Deliverable `docs/phase4/AUDIT-handowns-go1.23.12.md` |
| **H7** compile parity | coordinator, desktop | no | Both flavors. Linux last measured **307/307, 0 errors, 149 warnings, 475 s** (2026-08-14) |
| **H8** L3 re-emission | coordinator, desktop | no | Three targets, ~545–560 s at r50a/r51b. The 37-package L3 figure is a measurement |
| **H9** golden rebank | coordinator, desktop | no | **Predicted small-to-empty** (§0.2, §4.4) |
| **H10** roster re-derivation | **the fleet** | **YES** | §4.3 |
| **H11** NuGet + guards | coordinator, desktop | no | Monotonicity to verify: `1.23.1.7 → 1.23.12.1` |
| **H12** docs / badges | one lane | no | The **19 GOROOT-vendored `golang.org/x/*`** packages re-pin — on a patch hop, the badge family most likely to actually move |

> **A PRE-H10 OBLIGATION, named here because it is this hop's one open blocker (2026-08-24).**
> The `time` row was pre-staged by measurement, not by reasoning
> ([`phase4/hopA-time-prestage.md`](phase4/hopA-time-prestage.md)), and the answer was a fourth shape
> nobody had offered: the banked 159 verdicts are **safe**, the fixed Stop/Reset semantics **already
> hold** on the shipping modes — and `GODEBUG=asynctimerchan=2` **crashes the host** with an
> `AccessViolationException` through `NewTimer`'s `unsafe.Pointer`-wrapped channel box
> (`src/core/time/time_impl.cs:367` is the locus). **A disclosure cannot absorb a crash**: an AV kills
> the process and takes ~100 later verdicts with it as the documented alphabetical-tail shape, so a
> mode-2-scoped disclosure is unreachable until the host survives the test. The closure is **one
> bounded piece of runtime work before H10** — fix the mode-2 path, or make the managed emulation
> treat `asynctimerchan=2` as an unsupported debug mode that degrades without crashing and disclose
> *that* choice. Left undone, H10 sees a `time` row failing with a mass-empty tail that reads like
> total conversion failure and is one AV on an undocumented debug mode.

### 4.2 H4a — this hop's opening regen bundle: one regen, three families

The board queues it explicitly (2026-08-21): *"**RIDES THE QUEUED LEVELING REBANK** (the time-class
born-stale leveling + map-coverage completion, due after the crossing) so **one deliberate regen levels
all three families at once**. Not taken inline by any current lane; the train does not stop."*
The crossing has happened, so the bundle is eligible.

| Family | Contents | Home |
|:--|:--|:--|
| **1 · born-stale leveling** | `time`'s banked test sources (surfaced in three lanes); `encoding/base64`'s directional-channel `.RecvOnly` initializer (the `cargo-recv` emission postdates that bank); `database/sql/driver`'s `package_test_info.cs` `global using Value = object;`; the seven `runtime` `unsafe.Pointer` box-compare sites; and the **`initᴛᴛtests()` hook + `static partial` declaration** in `encoding/xml`, `go/types`, `html/template` plus `crypto/tls`'s empty `<GoSourcePositionMaps>` block — all four banked before their emissions existed | BOARD's standing *born-stale, restore rather than level* rule; `MILESTONE-75pct-prep.md` §6 |
| **2 · map-coverage completion** | behind the dims-cargo arc; `encoding/gob` reaches 105 of 106, residual `reflect.ArrayOf`/`StructOf` — *"an arc with a price, explicitly not a disclosure"* | BOARD, *MAP KEY/ELEM DIMS ARE DESCRIPTOR CARGO* (2026-08-20) |
| **3 · the `go.`-prefix regen** | `[assembly: go.GoPositionMap(...)]` drops its redundant `go.` prefix — the file's `using` covers it and every sibling record is emitted unprefixed. **One emission-string fix** plus the corpus-wide info-file diff | BOARD, NIT banked (user, 2026-08-21) |

Owed contents per `GoCorpusMigration.md` §H4a — each converter fix with its CNR, the seeded reconvert,
**`go generate .` in `src/go2cs`**, and the born-stale rows re-swept at their banked counts. **Two
`.slnx` registrations ride along**: `core/math/big` and `core/runtime/debug` are in `go2cs.slnx`'s
build closure via `GolibTests` but unregistered — one line each, deferred twice for lane
conflict-avoidance. Re-run `check-solution-integrity.ps1`.

### 4.3 H10 — the shard map for this fleet

**Procedure: [`GoCorpusMigration.md`](GoCorpusMigration.md) §3** (why it shards, the ordering
resolution, the ledger, the signals, the incremental merge rule). This is the instantiation.

**The reserved set — pinned to the i9, never sharded blind.** Rows carrying `$longTimeouts` floors or
known to dominate:

> ⚠ **SUPERSEDED BY GENERATOR (2026-08-24).** The table below is a **copied** list, and it had already
> drifted **twice** by the time it was written: measured against the live `$longTimeouts`
> (`src/run-validated-sweep.ps1:495`) it misses `go/parser` (90 m), `crypto/internal/mlkem768` (30 m)
> and `crypto/tls`'s own 30 m floor, and it misquotes two more (`crypto/dsa` 60 m → **120 m**,
> `archive/zip` 30 m → **60 m**). A missed floor is not cosmetic: it deals exactly the rows that need a
> raised budget to a slow worker, which is the false red the floor table exists to prevent. **The
> generator now DERIVES the reserved set from `$longTimeouts` at generation time**
> ([`phase4/hopA-inputs/shardmap.py`](phase4/hopA-inputs/shardmap.py), `549b4e556`) — the same hoist-vs-derive
> ruling `_paths.ps1` got the same week. Read the table as the drifted-twice copy that ruling retired,
> kept for the reasoning in its *Why* column; the authoritative set is whatever the generator emits at
> dispatch.

| Row | Why | Measured |
|:--|:--|:--|
| `index/suffixarray` | 120 m floor | — |
| `hash/maphash` | 60 m floor | 2,406 s (40.1 min); 2,026 s under concurrent gates |
| `crypto/dsa` | 60 m floor | 2,444 s under concurrent gates |
| `archive/zip` | 30 m floor | ~775 s (4 GiB streamed twice) |
| `crypto/tls` | 400 verdicts, network- and cert-dependent, expired-fixture ceiling | 669–724 s |
| `go/doc/comment` | 10,059 verdicts; spawns `go build` throughout `TestStd` | — |
| `go/types` | 557 verdicts | 364 s |

**The map's construction**, deterministic so two coordinators build the same one:

*[GENERALIZED 2026-08-24 into [`GoCorpusMigration.md`](GoCorpusMigration.md) §3.2, which is now the
maintained copy of the construction, its symbol table, the derive-the-reserved-set rule and the
calibration protocol. What stays below is this hop's INSTANCE: the symbols' sources, the numbers and
the fleet.]*

```
1.  rows  := roster rows at the hop branch tip          (162 today; re-read, never carried)
2.  R     := reserved set ∩ rows                        (pinned to the i9)
3.  P     := rows \ R, sorted ASC by t_r                → phase recon: deal round-robin across W
4.  B     := rows \ R, sorted DESC by t_r               → phase bulk: LPT-greedy — assign the
             largest unassigned row to the bin with the smallest (load / s_w)
5.  split any bin whose load/s_w exceeds C into ceil(load / (s_w·C)) sequential shards
6.  emit  docs/phase4/SHARDMAP-go1.23.12.md — one table, W columns, every row named
          exactly once, with a checksum line: |rows| == |R| + |B|
```

| Symbol | Meaning | Source |
|:--|:--|:--|
| `W` | worker count | the fleet as engaged, **3…6** |
| `s_w` | worker speed factor, i9 = 1.00 | **measured**, from LANES.md's calibration pair (`run-behavioral.ps1 --filter Atomic` + `run-validated-sweep.ps1 -Filter 'container/heap'`), reported with the worker's first shard |
| `t_r` | row `r`'s expected wall time | the **anchor consolidation sweep's per-row log**, × `k` |
| `k` | the convert-and-build multiplier for `-test-action all` vs `-SkipBuild` | **placeholder — the hop supplies it.** Measure on the recon phase's first ten rows |
| `R` | the reserved set | the table above |
| `C` | target shard wall time | one session's worth; propose **~90 min** so a shard fits a turn with margin |

### 4.3.1 The computed map at the JOB-007 reading — DRAFT, pending recon calibration

> **DRAFT (2026-08-23) — pending hop-recon factor calibration.** The construction above,
> executed against the anchor's per-row wall times
> ([`phase4/DATA-sweep-row-walltimes.md`](phase4/DATA-sweep-row-walltimes.md), windows · corpus
> `18770d083` · i9-13900K, JOB-007: 162 rows, 18,569 verdicts, **7,701 i9-s total**). ⚠ **Every
> `s_w` below is a provisional placeholder** — LANES.md's roster marks historical cross-machine
> ratios SUSPECT, so the factors here are class-level readings (i9 = 1.00 by definition,
> R 6850U = 0.45, i7-5820K = 0.35, G 6650U = 0.35, any fifth worker = 0.35 assumed), to be
> replaced by the calibration pair's fresh numbers at recon and the map re-emitted. `k` is
> assumed 1 pending its recon measurement. The full per-row deal (every row named exactly once,
> checksum 162 = 7 reserved + 155 bulk) is emitted to `SHARDMAP-go1.23.12.md` at dispatch.
>
> ⚠ **SUPERSEDED BY GENERATOR (2026-08-24), and no longer stranded.** The computed draft and its
> generator banked out of the coordinator's scratchpad —
> [`phase4/hopA-inputs/shard-map-draft.md`](phase4/hopA-inputs/shard-map-draft.md) and
> [`phase4/hopA-inputs/shardmap.py`](phase4/hopA-inputs/shardmap.py) (`e0d8930e1`) — and the generator then stopped
> carrying a copied reserved set at all, deriving it from `$longTimeouts` at generation time
> (`549b4e556`). So the numbers below are the **generator's output at placeholder factors**, not a map:
> re-run the generator with the recon's measured `s_w` and `k` and it re-emits. The **7 reserved / 155
> bulk** split above is itself a reading of the drifted copy — the derived set is larger, so expect the
> checksum's two halves to move even before calibration does.

**The reserved set is the makespan from W = 4 up.** R's seven rows sum to **3,956 s (65.9 min)**
serial on the i9 — and at W ≥ 4 the remaining 155 rows (3,745 i9-s) fit on the other workers
~18 % under that floor, so LPT hands the i9 zero bulk rows and the campaign finishes when the
reserved set does:

| `W` | Fleet | Makespan (local wall) | Binding bin | Bulk bins finish |
|:--:|:--|--:|:--|--:|
| 3 | i9 + R + coordinator | **~4,289 s (71.5 min)** | all three balanced | — |
| 4 | + G | **~3,956 s (65.9 min)** | i9's reserved set alone | ~54 min |
| 5 | + one engaged machine | **~3,956 s (65.9 min)** | i9's reserved set alone | ~42 min |

Findings the numbers force, stated before dispatch:

1. **C = 90 min holds at every W at k = 1 — no bin splits.** The split thresholds: a bin first
   exceeds C at k ≥ 1.26 (W=3) / k ≥ 1.37 (W≥4). If recon's k lands above that, step 5 splits
   mechanically; the map re-emits, it does not redesign.
2. **At W ≥ 4 the makespan is only mildly sensitive to the suspect factors** (±0.10 on every
   non-i9 factor moves it 0–6 %) because the bulk bins are not the critical path;
   mis-calibration costs projection accuracy and single-digit finish slip, never the 90-min
   target. **At W = 3 it is capacity-bound and every factor
   error passes through** — a W=3 dispatch waits for real calibration; W≥4 tolerates
   placeholders. The one live sensitivity is the i9 itself: 20 % degradation → +25 % makespan.
3. **W = 5 buys margin, not makespan** (bulk bins 54 → 42 min). Engage a fifth machine for
   re-deal resilience against the i9's ~daily reboots, not for speed.
4. **Six rows carry 53 % of the wall**, five of them already reserved; `crypto/dsa` alone on a
   0.35 box would be 62.7 min. The pin is arithmetic, not caution. Two reserves are pinned for
   reasons this table cannot show: `go/doc/comment` (18 s here, but 10,059 verdicts and spawns
   `go build` throughout `TestStd` — the row most exposed to k) and `go/types` (137 s here vs
   364 s at its prior reading — it moves between readings).
5. **Reserved-set fallback (OQ-H6): R**, the largest non-i9 factor — at raised budgets only
   (~2.1 h local for the top four rows; never re-dealt at the i9's budgets, per §6 lens 3).
6. If ~66 min must shrink: the i9's 3-worktree capacity packs the reserved set to ~1,369 s
   (~23 min) in 3 parallel worktrees — at the cost of running exactly the `$longTimeouts` rows
   under concurrent load (crypto/dsa measured 2,444 s under concurrent gates). A coordinator
   trade, not a default.

⚠ **The map's measured cost input is the anchor's consolidation sweep** ⟨OQ-H5⟩ — the Windows
leg's per-row walls are **banked** (`phase4/DATA-sweep-row-walltimes.md`, JOB-007, the reading
above); the Linux leg's ledger **posted 2026-08-23** (`861475db0`) into the same file — 162 rows,
149 PASS / 10 FAIL / 3 CVAC, aggregate 19,113 s, directly instrumented per row rather than
mtime-derived ([`phase4/DATA-sweep-row-walltimes.md`](phase4/DATA-sweep-row-walltimes.md), the
`linux · corpus 18770d083` section). ⚠ **It is a second SECTION, not a second column**, and the
ratio is not uniform — `crypto/dsa` is 4,366 s there against Windows' 1,317 s — so `t_r` as written
above is not OS-aware. **Which leg a row is costed from is not a fresh question**: it falls to the
standing rule §4.3's `s_w` row and the shard-map draft's own banner already carry — *cost inputs and
speed factors come from FRESH same-workload calibration at campaign recon, never from pre-anchor
history* ([`phase4/LANES.md`](phase4/LANES.md)) — so the recon that measures `k` and every `s_w`
measures the leg's row costs with them, on the leg the shard will actually run.
Future sweeps carry per-row
`[NNNs]` natively (`run-validated-sweep.ps1` since `4e91a03e2`), so this input is no longer
unrecoverable.

⚠ **A worker cannot sweep before H2 reaches its clone**, and will say so loudly:
`run-validated-sweep.ps1:85-133` throws when `version.props` disagrees with GOROOT's `VERSION`. Confirm
Go 1.23.12 and the hop-branch fetch **in the shard's ACK**, not at row 1.

**Per-shard signal** — the mailbox's entry format with `JOB` as the body's first word, per
`MILESTONE-75pct-prep.md` §2, plus the shard lines:

```markdown
## <YYYY-MM-DD HH:MM> · FROM <worker>/h10-shard-<n> · TO coordinator

JOB COMPLETE — H10 shard <n>, <M> rows.

Branch:    claude/h10-shard-<n>  @ <SHA>   (PUSHED — a tip you can fetch)
Shard map: docs/phase4/SHARDMAP-go1.23.12.md, column <w>, rows <first>..<last>
Ledger:    <path in the branch>   (M rows, all terminal, checksum M == assigned)
Result:    <P> PASS · <F> re-derived-with-movement · <C> COUNT · <N> NOT MEASURED
Movement:  every row whose count moved, named, with its upstream attribution
           (the triage class) or the word UNATTRIBUTED
Machine:   <box>, CGO_ENABLED=<0|1>, elapsed <hh:mm>
```

**The merge canary set is derived at gate time, never carried.** As of 2026-08-19 the five largest
banked reflect consumers by verdict count are `go/internal/gcimporter` 583, `go/types` 557,
`encoding/json` 491, `crypto/tls` 402, `encoding/xml` 386 — **recompute from the roster at the gate**;
the known escape happened precisely because a canary set predated the newest bank.

### 4.4 Two hop-A predictions, stated so they can be falsified

1. **The behavioral golden diff should be small or empty.** Channel 2 is source-verified inert (§0.2);
   channel 1 moves only if a *referenced* package's `package_info.cs` moved — the behavioral closure is
   `golib` + the analyzer + `fmt`/`time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`/`reflect`; channel
   3 moves only if a patch changed observable behavior in a program the suite runs. **A large diff here
   is a finding, not a rebank.**
2. **Roster counts should move very little.** A patch release adds tests rarely and removes them almost
   never; the gate is on **absolute row count** (OQ-10) with upstream-deletion exceptions, and **no
   package is deleted on this hop**. Both numbers reported regardless.

---

## 5. Sequencing and dependencies — one page

`■` blocks · `◆` gated by · `○` free to start

| Work | Before the anchor ships? | Gated by | Blocks |
|:--|:--|:--|:--|
| **Route-#4 toolchain stamp** (OQ-6) | **○ yes** — converter change with its own CNR; the stamp must land **before hop A** by ruling | nothing | **■ hop A's H1** |
| ~~**The embedded-asset predicate widening** (§2)~~ | **LANDED 2026-08-22** — separated from the stamp and shipped first; nothing waits on it | — | — |
| **`.cs.auto` staleness** (CleanupBacklog 18) | ○ yes, on its own merits (OQ-7) | nothing | nothing — H0 generates fresh baselines regardless |
| **`.slnx` registrations** (`core/math/big`, `core/runtime/debug`) | ○ yes, one line each | nothing | nothing (an unregistered member breaks only the VS build) |
| **SDK provisioning (N0)** | **○ yes** — side-by-side disturbs nothing | nothing | ■ N1 |
| **Shard-map input: per-row wall times** | **○ yes, and it MUST** | ◆ the anchor sweep retaining per-row logs ⟨OQ-H5⟩ | ■ H10's map |
| **The ledger/driver instrument** ⟨OQ-H1⟩ | ○ yes | nothing | ■ H10's recon phase |
| **The i9's unsigned-mailbox path** (§1.1) | **○ yes — user-hands-only** | ◆ a human setting it once on that box | ■ the sweeper reporting after a reboot |
| **Anchor release `1.23.1.7`** | — | ◆ (a) readiness poller landed and measured · (b) remaining Linux seams closed or classified-final · (c) per-OS verdict-arithmetic ruling — **DONE, ruled 2026-08-22, harness landed `249b47b74`** · (d) one full dual-OS consolidation sweep green | **■ hop N and hop A both** |
| **Hop N · N1** | no | ◆ anchor · ◆ N0 | ■ N2/N3 |
| **Hop N · N2 → N4** | no | ◆ N1 for N2; ◆ N3 for N4 | ■ E-c |
| **Hop N · N3** | no | ◆ N1 green · ◆ N2 captured | ■ N5, N7, every later .NET measurement |
| **Hop N · N5** | no — **structurally impossible before N3** (the ILC runtime-pack binding) | ◆ N3 | ■ E-a |
| **Hop N · N6** | annotations ○ can be drafted early; the **audit** needs the new SDK | ◆ N1 | ■ the trim evidence rows |
| **Hop N · N7** | no | ◆ N3 · ◆ N6 where ReadyToRun/AOT is the lever | ■ E-b · ■ the roster/manifest re-derivation |
| **Hop N · N8** | no | ◆ the full evidence checklist | ■ hop A's H10 cost model ⟨OQ-H4⟩ |
| **Hop A · H4a's bundle** | **○ the converter fixes and their CNRs can be prepared** | ◆ anchor (*"due after the crossing"*) | ■ H5's diff readability |
| **Hop A · H1→H2 (one pair)** | no | ◆ the stamp · ◆ anchor | ■ H5 |
| **Hop A · H5 → H7 → H8 → H9** | no | ◆ H2 | ■ H10 |
| **Hop A · H6** | **partly ○** — run A's `.auto` baseline is H0's | ◆ H0's baseline · ◆ H5's staging root | ■ the hand-own parity gate |
| **Hop A · H10** | no | ◆ H5–H9 green · ◆ the shard map · ◆ the ledger | ■ the roster parity gate |
| **Hop A · H11/H12** | H12's badge-diff *expectation statement* ○ can be written early | ◆ H10 | ■ the ritual gate |
| **Master cutover** | no | ◆ all five parity gates | — |

⚠ **The one ordering with no defense: hop N must complete, entirely, before hop A's H1.** N7
re-derives roster rows against a new host shape; H10 then re-derives every row against 1.23.12's test
sources. **If they overlap, some shards measure the old host shape and some the new**, and the
disagreement presents as a *machine* difference — the hardest kind to chase across six boxes.

---

## 6. Adversarial pass — three lenses

Per the charter §7 (*"invest in adversarial review up front"*), applied to this plan's operations.

### Lens 1 — what breaks if a hop step runs out of order

| Inversion | What actually happens | Defense |
|:--|:--|:--|
| **H5 before H2** | `checkCorpusToolchainPin` refuses `-stdlib` and **names the remedy in its own error text**. Loud, cheap, self-correcting | none needed; the code is the gate |
| **H1 and H2 as separate merges** | The three-pin window opens: the binary claims 1.23.12 while `version.props` names 1.23.1, so `-recurse=nuget` **refuses legitimate old-pin modules and accepts new-pin ones for a corpus that does not exist**. Silent, and it only bites a user | §4.1 makes the pair a table row |
| **N3 before N1** | Two variables at once; a break is unattributable between "the SDK" and "the TFM" — and Roslyn 10's new CS7022 is exactly the noise that would then read as a TFM consequence | separate gates; N1's deliverable is a **classified warning delta** |
| **N5 attempted before N3** | **Nothing breaks, and that is the danger.** It produces an ILC-9 number that looks like an ILC-10 number, because the runtime pack is TFM-versioned. Measured: 178.2 vs 177.1 ms, *"identical, and structurally so"* | §3.1 states the impossibility first and makes the **51-second tell** a named alarm |
| **H10 before H9** | Rows re-derived against goldens the hop has not rebanked; every moved count carries two candidate causes | §4.1's ordering; the triage protocol consumes H9's classification |
| **H4a's bundle AFTER H5** | H5's overlay diff — the hop's primary signal — arrives mixed with three families of unrelated born-stale drift. Not a failure; a **loss of signal**, and the expensive kind | §4.2 schedules it as the opening move, and says why |
| **A shard merged without its post-merge sample re-sweep** | The `crypto/tls` shape recurs: green on the shard tip, red at the merge result, found later by an unrelated lane | `GoCorpusMigration.md` §3.5 rule 1, with the 2026-08-19 precedent |
| **`go generate .` skipped after the bundle** | `stdlib-metadata.txt` drifts and `TestStdLibMetadataInSync` goes red at master **for the next lane, not the one that caused it** (measured 2026-08-15) | §4.2's owed set |

### Lens 2 — where a false-green could hide in a re-derived roster

The repository catalogues four false-green routes; a **re-derived roster** is a new surface. Five
shapes deserve naming before they are met:

*[GENERALIZED 2026-08-24 into [`GoCorpusMigration.md`](GoCorpusMigration.md) §3.4 — the five shapes
and the structural build-error-without-diagnostics one now live in the runbook, version-agnostic.
They stay here with this hop's specifics attached, which is what an instance plan is for.]*

1. **The vacuous shard.** Every row PASSes because the clone was never moved to the hop's corpus SHA.
   The ledger's corpus-commit field is the defense and the merge asserts it. Without it, "162/162" can
   be a measurement of the *old* corpus, run six ways.
2. **The rebased-disclosure launder.** A pin breaks when a test is reworded, and the fast fix — edit
   the signature to match — converts a real divergence into a rubber stamp. **Re-sign, never edit**;
   every re-signed entry names the upstream commit. The `runtime-capability` class's **anti-laundering
   clause** (*"an entry pins its rows AS FAILING"*; writing one byte to pass `TestWriteHeapDumpNonempty`
   is *forbidden by the class's own text*) is the precedent.
3. **The disclosure that closed silently.** On a patch hop, closures are the *more* likely direction
   (eleven releases of upstream fixes). A closure is a good outcome and must still be **retired with
   evidence** — the arithmetic must move, visibly, or nothing was proven.
4. **The truncated artifact protected by an up-to-date check.** Measured 2026-08-22 on the behavioral
   side: a transpile timeout left a `.cs` **zero bytes on disk**, the next run's `UpToDate` check
   **skipped it** (an empty file is still newer than `go2cs.exe`), and the build failed with `CS5001` —
   a false RED that time. *"The same mechanism could hide a real one."* On six workers with three
   worktrees each under budget pressure, transpile timeouts are **more** likely, not less. **A shard
   reporting zero timeouts on a slow box deserves a second look, not a congratulation.**
5. **A stale `go2cs.exe` reaching hop A.** The embedded-asset half of this is closed (§2, landed
   2026-08-22), but the shape survives for any input no predicate observes — route #4's Go toolchain
   being the live one. Its worst form *is* a shard campaign: a worker whose binary predates the change
   re-derives against the old emission and reports green, while a worker that happened to rebuild
   reports drift — and the disagreement reads as a *machine* difference. **The ledger's
   converter-commit field is not enough**; a commit does not say whether the binary was rebuilt after
   it. Record the binary's modification time beside it, or have each ACK confirm an explicit
   `go build`.

**And the structural one:** `run-validated-sweep.ps1` collapses build errors into bare `FAIL <pkg>`
rows *with zero diagnostics* — the harness-honesty item priced 2026-08-22, where `CS0117` hid behind
three batches of silence until someone ran the row by hand. Across `W` workers nobody reads `W × M`
logs. **The shard's report must distinguish "failed with named verdicts" from "failed with none"** —
the second is a build failure wearing a verdict's clothes.

### Lens 3 — what the fleet does when the i9 reboots mid-campaign

*[GENERALIZED 2026-08-24 into [`GoCorpusMigration.md`](GoCorpusMigration.md) §3.6 — the
classify-before-diagnosing ladder, the resume rule, the raised-budget re-dispatch, the named-fallback
answer to losing a control worker, and the parity-sweep recovery. This lens keeps the i9's declared
failure mode, the measured JOB-R2 reading and the unfinished GPG item, which are this fleet's.]*

**Not hypothetical, and mostly already answered.** The i9 declares the failure mode as routine: *"⚠
This box reboots randomly (~daily, pending RMA). That is expected. On session start after a restart I
re-poll and re-run anything I ACK'd but never posted a result for, so a job lost to a reboot costs a
re-run, not a gap."* Measured live during JOB-R2: the machine rebooted after **58 of 59 rows had gone
green**, and **the worktree, the built `go2cs.exe` and every log survived on disk intact, no
corruption** — only the processes and the GPG agent's cache were lost.

The resilience is structural: idempotent ledger keyed on corpus commit, per-row logs on disk, shard
branch pushed. **A reboot costs the rows in flight, not the shard.**

**Recovery, in order:**

1. **Classify before diagnosing.** A truncated log with no diagnostic has four known causes and only
   one is a defect: (a) a sibling's bare-name `Stop-Process` — machine-global, worktree isolation does
   not help; (b) harness background-task **tree** reaping at a turn boundary — walks parentage, not
   names, so the apphost-rename defense does nothing; (c) a sibling's `dotnet build-server shutdown`;
   (d) an actual reboot. **Check uptime first** — one command, and on the i9, (d) is the most likely
   answer.
2. **Resume, do not restart.** Rows with a terminal verdict at the current corpus commit are skipped;
   in-flight rows re-run. A reserved row re-runs whole — which is one reason the reserved set is pinned
   to the fastest worker.
3. **The GPG cache is the sharp edge, and it is unfinished.** A rebooted worker cannot sign; the
   2026-08-22 ruling exists so it can still post unsigned on `claude/mailbox`. But the worktree-scoped
   unsign is **classifier-blocked and user-hands-only** (§1.1). Until a human sets it once on that box,
   **a reboot mid-campaign costs the sweeper its voice, not just its rows** — it can compute and cannot
   report. **Resolve before H10 dispatch, not during.**
4. **Re-dispatch if the box is gone rather than rebooting.** The reserved set is the only shard that
   cannot be re-dealt cheaply — a 120-minute floor and a measured 2,406 s row land differently on a
   3–4× slower box. Raise `-TestTimeout` (which **raises** the floors) and re-deal. **Never re-deal at
   the same budget**: under-budgeting exactly those rows is the false red the floor table exists to
   prevent.
5. **Do not let the coordinator absorb the reserved set silently.** The i7-5820K runs these rows at
   3–4× the i9's, and the coordinator is the one machine whose stalling stalls everyone. Say so on the
   mailbox.
6. **The one thing that does not survive**: a full-roster parity sweep — coordinator-owned,
   backgrounded, ~2 hours on the fastest box; two already lost to turn-boundary reaping at 106/110 and
   98/110. **Recovery is `roster − logged`, re-run inline, with the arithmetic checked to close.**

**The deeper hazard.** The i9 is a single point of *measurement*, not merely of throughput: it runs the
Windows control legs that make every Linux finding attributable (*"every Linux FAIL is Linux-specific
**by measurement**, not inference"*). If it is gone for a campaign, hop A's Linux legs lose their
control and their findings degrade from measured to inferred. That is not a scheduling problem to solve
inside a shard map — it is a fact the coordinator states in the record. ⟨OQ-H6⟩.

---

## 7. Open questions — operational only

Six. The frame's nineteen strategic questions are ruled and are not reopened; each of these is a
genuinely **new** execution decision, and each carries a recommendation.

| # | Question | Why it is open | Recommendation |
|:--|:--|:--|:--|
| **OQ-H1** | Does the per-row campaign driver + resume ledger become a **committed instrument** (`src/run-campaign-shard.ps1`), or stay lane-local scratch? | Four lanes have each rebuilt it and it has never been in the repository — the only artifact named by path is `/root/campaign-logs/verdicts.txt`, whose **format is nowhere specified**, on a distro since force-reset. H10 needs up to six workers running the same one, and ledgers that differ per worker cannot be merged as evidence. Against: a new committed instrument is a new gate surface, and the repo's own precedent (the per-OS format guard, 2026-08-22) is to keep one **standalone** until a quiet point decides its wiring | **Commit it, standalone.** It reads the roster through `_roster.ps1`, invokes `run-validated-sweep.ps1 -Filter <pkg> -Exact -SkipBuild` unchanged (the instrument stays **serial by design**, untouched), and wires into **no** preflight. The format-guard lesson is that adding a Windows failure mode to an *existing* gate is what the lane correctly refused — not that new instruments are unwelcome. `GoCorpusMigration.md` §3.4 is the schema |
| **OQ-H2** | Does the repository adopt a **`global.json`** to pin the SDK, and when? | There is none today. Across five machines "which SDK built this" is a `PATH` accident, and hop N makes two SDKs present on every box. Against: a pin to a major would break a contributor on a different patch level, and roll-forward policy is its own decision | **Adopt one, at N3, with a roll-forward policy tolerant inside the pinned major.** Not at N0: during N0–N2 both SDKs must be selectable by environment, which a pin fights. Record it in the same commit as the TFM line so the two move together |
| **OQ-H3** | What happens to the **published NuGet packages** between the anchor (`1.23.1.7`, net9.0) and hop A's `1.23.12.1`? Hop N changes the TFM without changing `<GoStdLibVersion>`, so a `1.23.1.8` would be a `net10.0` package under a version scheme carrying no runtime signal, and a .NET 9 consumer would fail | OQ-14 ruled *"every hop publishes"*, but that ruling was framed over **Go-version** hops; a runtime hop was not in view. The anchor's own framing calls itself *"the last publication of the 1.23.1 corpus on .NET 9"* | **Hop N does not publish.** The anchor is the .NET 9 reference point by its own definition, and the next publication is `1.23.12.1` on the new runtime. Hop N still **rehearses** the ritual. If publishing between them is later wanted, the honest shape is multi-targeting so NuGet's asset selection answers for both — priced, not assumed, since it multiplies 306 packages' build |
| **OQ-H4** | If N7 makes every converted test host a **self-contained single-file publish** (~70 MB and *a publish rather than a build*, per package), what does that do to H10's per-row cost — and does the shard map absorb it, or does the publish shape become opt-in? | The cost is priced in the converter's own source comment but **never measured**. A publish-per-row across 162 rows is a different campaign from a build-per-row | **Measure it in hop N's evidence pass and let the number decide, defaulting to opt-in.** The disclosure machinery needs the single-file shape only where a `host-limit` row is measured — **3 packages, not 162**. Recommend a publish-shape switch on the sweep so those 3 pay for it and the other 159 do not; drop the switch if the measurement shows the cost is small |
| **OQ-H5** | Must the **anchor's dual-OS consolidation sweep retain per-row wall times and logs**, as a hard deliverable? | It is the shard map's only measured cost input (§4.3). Without it the map is built on verdict counts, which is a bad proxy — `archive/zip` (100 verdicts, ~775 s) and `go/doc/comment` (10,059 verdicts) would be mis-binned in opposite directions | **Yes — make it an anchor obligation.** One line in the `MILESTONE-75pct-prep.md` §2 Step 5 JOB block ("Report: … and the per-row wall times") plus per-row log retention on the sweeper. It costs the anchor nothing and is unrecoverable afterward |
| **OQ-H6** | Should a **second machine hold a standing control role**, so the fleet is not one box deep on measurement attribution? | The i9 runs every Windows control leg; that is what makes each Linux finding *measured* rather than inferred (§6 lens 3). A campaign that loses it does not merely slow down — its findings change class. Against: a standing second control costs a shard worker, and controls are only occasionally needed | **Designate a named FALLBACK rather than a standing second control.** The cost of a duplicate is a worker; the cost of a fallback is one line in the shard map. The fallback runs the control leg **at its own speed, with the budget raised**, and its results carry its machine name — which is all attribution ever required |

---

## Sources

- [`DotNetMigration.md`](DotNetMigration.md) and [`GoCorpusMigration.md`](GoCorpusMigration.md) — the
  procedure this plan instantiates
- [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) — the ruling frame, the H-series inventory, the
  parity gates, the risk register, and the nineteen ruled open questions
- [`PLAN-bflat-perf-exploration.md`](PLAN-bflat-perf-exploration.md) — the concluded floor exploration
  (Findings 1–5, the `X1`/`X2` caveat, the trim diagnostics by file)
- [`phase4/MILESTONE-75pct-prep.md`](phase4/MILESTONE-75pct-prep.md) — §2 (the crossing sequence, the
  mailbox JOB block), §3 (the anchor checklist and its four trigger conditions), §6 (what rides behind
  the milestone)
- [`phase4/BOARD-next-validation-candidates.md`](phase4/BOARD-next-validation-candidates.md) — the .NET
  10 performance scout (2026-08-22, lane G); the Linux measurement campaign and the four lanes after
  it; the per-OS verdict-arithmetic ruling and its landed harness (`249b47b74`); the position-map
  ruling fixing `runtime/debug`'s host-limit entry as permanent; the `go.`-prefix NIT and its
  one-regen-three-families queue
- [`phase4/MAILBOX.md`](phase4/MAILBOX.md) — the protocol header. **The live record is on branch
  `claude/mailbox`**: the i9's arrival and worker contract, every `JOB-*` dispatch/ACK/result, the
  Linux campaign's shard signals, the unsigned-commits ruling, and the watcher protocol addition
- [`phase4/LANES.md`](phase4/LANES.md) — the five standing lane rules and the calibration pair (its
  assignment table is stale); [`PLAN-linux-operation.md`](PLAN-linux-operation.md) — the F15 recipe
- [`ValidatedTestPackages.md`](ValidatedTestPackages.md) — the roster header arithmetic and the
  disclosure classes; [`Roadmap.md`](Roadmap.md) — the host-limit retirement path
- `CLAUDE.md` — the measured budget table, the false-green routes, the concurrent-session and
  detachment rules, the banked-row merge protection
- Source read directly: `src/Directory.Build.props`; `src/go2cs/embeddedTemplates.go`;
  `src/go2cs/csprojMetadata_test.go:211`; `src/go2cs/directiveOperations.go:264`;
  `src/go2cs/testConversion.go:2864-2914`; `src/run-validated-sweep.ps1` (parameters, serial-by-design
  comment at :199, toolchain pin at :85-133, disk preflight); `src/push-nuget.ps1`;
  `src/core/{os/exec,crypto/tls,runtime/debug}/go2cs_test_disclosures.json`;
  `src/tests/Behavioral/BehavioralRunner/Program.cs:332`,
  `src/tests/Behavioral/BehavioralTests/BehavioralTestBase.cs:145`,
  `src/tests/Performance/PerformanceRunner/Program.cs:275`
