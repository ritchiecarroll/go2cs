# PLAN — the post-hop performance re-baseline (Go 1.26 packages)

> **STATUS: DRAFT (G, 2026-10-23). RULED by COORD on 2026-10-23: accepted as a draft, and it stays DRAFT until
> S0 lands. The rulings on its open questions are in section 9.** Document type: an [instance plan](doctrine/ProcessGlossary.md#plan). The
> owner ruled the run itself: after the 1.26 NuGet publish, re-run the [performance suite](Performance.md) (Go against
> converted C#, JIT and Native AOT), with every timed run on G-LAPTOP for comparability with the original baselines.
> The i9 only builds the Native AOT apps and ships them to G-LAPTOP. The procedure for a re-measurement is
> [`DotNetMigration.md`](DotNetMigration.md) section 6 (paired same-session A/B, a named control, Verify before
> timing, bank through the instrument). This plan supplies the instance: arms, box state, the handoff, the RID per
> line, the PERF-REVIEW list and what a regression triggers. It runs in parallel with the nugetgo wave. When it
> executes, it becomes a record.

## 1. What is measured, and against what

**The subject is the published 1.26 packages, as a consumer gets them.** Each benchmark program is converted the way
a user converts a program: with that release's own converter and `-recurse=nuget` (see
[`CommandLine.md`](CommandLine.md)), so the project references `go.lib`, `go.gen` and the `go.*` packages by
`PackageReference` at the release version, read from nuget.org. Nothing in the timed binaries comes from a project
reference into `src/core`. The suite's own tree (`src/tests/Performance`, project references, re-transpiled at
batch 26) stays the development instrument. Its numbers are not the published ones.

**The comparison is 1.24.13.7, measured again in the same session.** The runbook rules out the alternative:
*"a perf comparison against an earlier-session baseline is not evidence — only a paired same-session A/B is"*
(`DotNetMigration.md` section 6). The table published today is not a 1.24.13.7 table anyway: it is the Go 1.23.1 /
.NET 10 era (2026-08-25), and the Roadmap still lists the 1.24.13.7 re-baseline as next. So this run measures both
releases, each through its own converter and its own published packages, in one session on one box. Any
1.24.13.7 figures recorded elsewhere serve as a cross-check, never as the comparand.

**The programs are all 17 `Perf*` mains in the suite's tree**, the 14 rows of the published table plus FieldView,
IfaceShellChurn and TlsHandshake. The Roadmap asks for TlsHandshake to enter the published table at this
re-baseline.

## 2. Arms

Eight binaries per program, in two release legs of four arms each:

| arm | 1.24.13.7 leg | 1.26 leg | built on |
|:--|:--|:--|:--|
| **Go** | Go 1.24.13 | Go 1.26.10 | G-LAPTOP |
| **JIT** | `dotnet build -c Release`, framework-dependent | same | G-LAPTOP |
| **R2R** (third arm, P1's ReadyToRun) | the converter template's own publish | same | G-LAPTOP |
| **AOT** | Native AOT publish | same | the i9, shipped |

- **Go** is built by each leg's Go pin, so a Go-side change between 1.24.13 and 1.26.10 shows in the Go column
  and divides out of that leg's ratio. Both pins are installed on G-LAPTOP.
- **JIT** is the published table's JIT column: framework-dependent, dynamic code enabled. The runner's
  configuration guard refuses any other shape.
- **R2R** is new, and it is the TEMPLATE-DEFAULT publish (ruled O1): what a consumer gets from the
  converter-written project with no property of their own, `dotnet publish -c Release -r win-x64`. The release's
  `csproj-template.xml` sets, for an executable:
  - `PublishReadyToRun` = `true`
  - `PublishTrimmed` = `True`
  - `TrimMode` = `partial`, unless the project already sets one
  - `IncludeNativeLibrariesForSelfExtract` and `EnableCompressionInSingleFile` = `true`. Both act only on a
    single-file publish, which the template does not request.
  - `TieredCompilationQuickJitForLoops`: left commented out, so at the runtime default.

  It sets no `SelfContained` and no `PublishSingleFile`. The SDK makes a trimmed publish self-contained, and the arm
  records the published `runtimeconfig.json` to show it. The list is read from the template at hop tip
  `05b7c851c3`; S2 records the release's own and stops if they differ. The arm sits between the other two
  columns: precompiled code like AOT, but still tiering up under the JIT. It costs a small fraction of an AOT
  publish, so G-LAPTOP builds it.
- **A fifth, diagnostic arm: Fd1, framework-dependent ReadyToRun** (P1's consumer-side DATA, mailbox `b8b36f7083`,
  linux-x64, folded in at S0 per O1). It is recorded but not published. Whether the template changes is a separate
  ruling.
  - **P1's PerfStartup medians (ms):** JIT 469, fd0 488, fd1 329, template default 331, AOT 197, Go 2.
  - **What ReadyToRun saves:** JIT'd methods drop from 2,651 to 1,162. Of the 1,162 still jitted, 1,159 are
    cross-bubble generic instantiations: `ж<T>` 409, `StandardBox` 181, `System.Collections` 178.
  - **ReadyToRun is publish-only,** so a `dotnet run` or build consumer never gets it.
  - **How S0 builds it:** `dotnet publish -c Release -r win-x64 --self-contained false -p:PublishTrimmed=false
    -p:PublishReadyToRun=true` (`--fd1`). The runner refuses it unless the runtimeconfig is framework-dependent and
    the program assembly carries ReadyToRun code.
- **AOT** is `PublishAot`, self-contained, partial trim, `-r win-x64`, at the SDK's default instruction set: no
  `IlcInstructionSet`, nothing host-specific. The i9 builds both legs (34 publishes). Section 4 covers the handoff.

The .NET runtime is the same in both legs (one `net10.0` runtime on G-LAPTOP), so the leg delta is converter,
golib, generators, the converted packages and Go's own source. The runtime is recorded through the
`FrameworkDescription` probe in each leg (runbook trap 4).

## 3. The RID on every line

The published table names each column's RID and shape in its legend, one line per arm:

| column | RID / shape |
|:--|:--|
| Go | `windows/amd64`, default `GOAMD64` |
| C# (JIT) | framework-dependent, `win-x64` apphost, runtime version as probed |
| C# (ReadyToRun) | `win-x64`, self-contained, partial trim, ReadyToRun |
| C# (Native AOT) | `win-x64`, self-contained, partial trim, default instruction set, compiled on the i9, measured on G-LAPTOP |

The Environment line keeps its form (CPU, OS, Go version, SDK, date) and adds the package version, e.g.
`go.* 1.26.10.N`.

## 4. The AOT build and ship handoff

The same protocol as the 1.23.1-era farm run ([`evidence-aot-farm-ab-session.md`](phase4/evidence-aot-farm-ab-session.md)):

1. **Sources.** The i9 converts each program for each leg exactly as section 1 says, at the release tags. For each
   program and leg, the manifest records the SHA-256 of the sorted converted `.cs` set and of the `.csproj`.
   G-LAPTOP converts the same way for its JIT and R2R arms and must read the same two hashes. A mismatch means the
   two boxes measured different programs: stop and find out why.
2. **Publish.** The i9 runs `dotnet publish -c Release -r win-x64 -p:PublishAot=true` (partial trim, self-contained,
   no instruction-set property) for each program and leg. It records the ILC version, SDK version, publish wall,
   publish peak working set and image size. Concurrency is the i9's call; each publish needs its own 15-18 GB.
3. **Ship.** Each executable goes over the LAN share with a manifest line: program, leg, SHA-256, size, ILC, SDK,
   source hashes. No PDBs.
4. **Receive.** G-LAPTOP recomputes every SHA-256 before anything runs. A mismatch is re-shipped and never
   measured. Hashes certify transport only: AOT output is size-deterministic but not byte-deterministic, so a hash
   says nothing about measurement identity.
5. **Licence.** The cross-box adoption was licensed on 2026-08-25 at ILC 10.0.11: an i9-built Sieve measured 0.25 %
   from a canon-built one, against a 1.19 % same-box A/A null. If the i9's ILC version differs from 10.0.11, that
   licence does not carry. Re-license on one row first: G-LAPTOP publishes it once and the pair is measured on the
   farm-session protocol (alternating, medians of 5, Go control). Only then are the shipped AOT binaries timed.

The runner today can neither adopt a shipped binary (its AOT skip requires a publish stamp, and a foreign binary
without one is re-published) nor build from package references or publish R2R. Section 7 S0 is that instrument work.

## 5. Box state on G-LAPTOP

- **AC power**; sleep and hibernate off on AC; lid action "do nothing". Sleep has killed this kind of run three
  times.
- **Power plan: Balanced**, the only scheme installed on the box today. No earlier table recorded its plan, so this
  session records the active scheme's GUID at the start and the end of each window. It does not install or switch
  to another plan.
- **Nothing else running.** No battery, `-tests` row, conversion, build, NuGet wave step or second fleet session on
  this box during a timed window. The second G lane, when one is live on the box, is idle. A process
  census (by executable path, never by name) at the start and the end of each window shows no `dotnet`, `go`,
  `MSBuild` or `link` process except the runner's own. Builds run before the timed window, never inside it.
- **No relay writes to the box during a window** (COORD, 2026-10-27, standing). A copy over the share runs no local
  process, so the census cannot see it. COORD asks G before any relay into G-LAPTOP and starts it only on G's
  go-ahead. The window script also records the newest file time and the file count under `C:\go2cs-perf` at the
  start and the end of the window, and the window is void if either moved.
  <!-- 2026-10-27: window 1's first run (14:00-15:11) was voided by an aot-foldoff2 relay that wrote ~560 MB images
       into C:\go2cs-perf from 15:01, over the last ~10 minutes of the measure phase and the end control; both
       process censuses read 0. -->
- **A warm start.** The control and null run once, discarded, before the readings that count. The first window's
  start control was read on a box that had been idle, and the box then stepped about 2.3 % slower and held there.
- **Disk:** at least 30 GB free before the AOT drop lands (34 images at about 300 MB each, plus build output).
- **The session carries its own control.** The 1.24.13 Go Fib and Sieve binaries run at the start, the middle and
  the end. A window is void, and is re-run rather than averaged, when a control's start-to-end median drift is
  significant: its bootstrap 95 % interval (5,000 resamples of each side) excludes 0 by more than the session's A/A
  null (section 6). A control whose own run spread is more than 5 times the null is reported but cannot void a
  window alone. On G-LAPTOP that is Sieve (15-30 % spread), so Fib carries the void test. (COORD's ruling,
  2026-10-27.)
  <!-- Why the rule changed: as first written ("drifts by more than the A/A null") it compared an unpaired
       start-to-end difference of medians with a paired same-binary difference. Window 1's first run read Fib
       +2.29 % (bootstrap 95 % interval [-0.71 %, +5.10 %]) and Sieve +2.33 % ([-12.2 %, +8.0 %]) against a null of
       0.83 % / 0.42 % whose own interval is about +/-1-3 %; the runner's mid-window Go rows sat at the end values.
       Sieve could not meet the first bar on this box even with no drift. -->
- **The middle reading** is the runner's own 1.24.13.7 Go rows for Fib and Sieve, measured inside the window.

## 6. Measurement protocol

- **Verify first** (the runner's standing doctrine). All eight binaries of a program print the same
  timing-filtered output, and the two Go pins agree with each other. A program that fails Verify publishes no
  number, and its failure is a correctness finding (section 8), not a perf one.
- **Paired and interleaved.** For each program and arm, the two legs alternate run by run (1.24, 1.26, 1.24, ...)
  so thermal drift falls on both. One discarded warmup per binary. The published count stays a median of 5; the
  PERF-REVIEW rows below take 11.
- **The session's A/A null** comes first: two runs of one fixed JIT binary and one fixed AOT binary on the
  same protocol. That null is the floor every reported delta is read against.
- **Name the control, read it first** (runbook 6.1). Every changed column can move, so the control is the fixed
  1.24.13 Go binaries.
- **Allocation claims are counted, not timed.** This run makes no allocation claim. Any allocation finding goes to
  the count-gated GolibTests rows.

### The PERF-REVIEW list

These rows moved during the hop on the windows JIT, read 2026-10-21/22 by the i9's regression read and bisect and by
G's profile. Their durable record is the BOARD row of 2026-10-23, "the hop's windows JIT perf readings"
([`BOARD`](phase4/BOARD-next-validation-candidates.md)), with each row's mechanism and status. Each gets extra
treatment here:

- **Startup: confirm the fix.** Its rise was real: 794 more methods jitted at start-up, since taken back out by lazy
  registries and lazy static data. The re-baseline is where that shows or does not. The R2R column and the TC0
  column (below) separate start-up JIT work from start-up code.
- **Sort: tiering.** Its workload is a few hundred milliseconds, so tier-up (call counting, OSR, dynamic PGO)
  lands inside it, and its medians were identical under `TieredCompilation=0`. The published Sort number stays the
  default configuration, which is what a consumer runs. A diagnostic arm also runs it under `DOTNET_TieredPGO=0`.
- **RefLower and Iface: the layout lottery.** Both read bimodal across builds of unchanged source, so their number
  moves with code placement. For the AOT arm, the i9 publishes each of these programs **twice per leg**. The pair's
  spread is that row's own A/A, and a leg delta smaller than it is reported as no change. For the JIT and R2R arms,
  these rows take 11 runs, and their spread is reported beside the median.
- **IfaceCall: creep.** It rose across the hop with no single step. It takes 11 runs, and the leg delta is read
  against its spread. If it holds, section 8's cause brackets apply.
- **Channel: unconfirmed.** It read +11.7 % once and did not confirm in the bisect. It takes 11 runs, and either the
  re-baseline confirms it or the BOARD row closes it as noise.

**The TC0 column.** The runner gains a JIT column run under `DOTNET_TieredCompilation=0` (G's proposal of
2026-10-22) for every program. It is diagnostic: it goes to the DATA record, never to the published table. A leg
delta that appears at default tiering and not at TC0 is tier-up timing, not code.

## 7. Steps

| step | what | where | gate |
|:--|:--|:--|:--|
| **S0** | Instrument seat: the runner gains a package-reference leg (convert with a named converter and `-recurse=nuget` at a named version), the R2R column, the diagnostic TC0 and Fd1 arms, a hash-verified adopt-AOT path with a manifest, two-leg interleaved measurement, and the four-column table. Red-first: a planted hash mismatch refuses by name, a planted self-contained JIT binary is refused, and an unadopted foreign AOT binary is never timed. | G, a seat in a batch | lands before S2 |
| **S1** | The 1.26 publish lands. The i9 converts and publishes 34 AOT images, plus a second RefLower and a second Iface publish per leg (4 more), and ships them. | the i9 | manifests complete |
| **S2** | G-LAPTOP converts both legs, checks the source hashes against the i9's manifest, and builds Go x2, JIT and R2R. If ILC moved, it re-licenses on one row. | G-LAPTOP | hashes equal; licence holds |
| **S3** | Verify all 17 programs x 8 binaries. | G-LAPTOP | identical output |
| **S4** | Quiet window: A/A null, control, paired measurement, PERF-REVIEW extras, control again. Two windows (COORD, 2026-10-27): window 1 is JIT, R2R, Fd1 and TC0; window 2 is the AOT column. Each has its own null and control. | G-LAPTOP | no significant control drift (section 5); no write under `C:\go2cs-perf` |
| **S5** | Bank: the runner's `--update-readme` writes the 1.26 table. The same session's 1.24.13.7 leg goes into History beside the retained 1.23.1 and .NET 9 tables. Raw medians, runs, hashes, box state and diagnostic arms go to `docs/phase4/DATA-hop126-perf.md`. | G, a docs seat | repoguard green |

**Timing (COORD's ruling, 2026-10-27; the owner asked whether the run goes before or after the 1.26.10.2 re-bank and
release).**

1. **Now, and before the .2 re-bank starts: S2 to S4 for 1.24.13.7 against 1.26.10.1.** The AOT column uses the
   folding-off set the i9 publishes. Both windows close on G-LAPTOP before the re-bank starts there, because the
   re-bank wants the windows boxes and each box runs one battery at a time. COORD holds the re-bank's G-LAPTOP
   shard until G reports both windows closed.
2. **After go.\* 1.26.10.2 publishes, only the 1.26 column is measured again**, against 1.26.10.2 as published. The
   i9 publishes the 1.26 AOT leg again (the template default plus the folding-off cross-version set, about 1.5 h),
   and G-LAPTOP runs S2 to S4 for that leg. Before that publish, the runner's source hash stops covering the two
   host-specific lines it reads today (below), and the i9 recomputes its manifest's source column with it.
3. **S5 banks the Performance page at 1.26.10.2**, the version users install at launch. The 1.26.10.1 and
   1.24.13.7 legs go into History and into `docs/phase4/DATA-hop126-perf.md`. The target is the page live by about
   2026-11-07 to 11-09 (launch gate 4), ahead of the soft stage on 2026-11-10.

**The S2 source hash, as S0 built it, is host-specific.** `ConsumerProject.SourceHash` covers `package_info.cs`'s
`GoPositionMap` line, which names each Go file by its absolute path under the `--work` root, and the project's
`GoInputDigest`, which folds in a hash of the converter executable's own bytes. Two boxes therefore agree only when
they convert with the same converter executables in the same `--work` root. For the 1.26.10.1 and 1.24.13.7 legs,
G-LAPTOP converts with the i9's two executables, and the gate rewrites the path prefix to the i9's root
(G-LAPTOP has no D: drive); it matched all 27 manifest lines that had landed, and a hash taken without the rewrite
does not match. The runner fix masks both lines, with a test that fails first, after window 2 and before the
1.26.10.2 re-publish. Each leg's converter is built from its release's record commit, never from the tag: the tag
sits one commit earlier, where `corpus-release.txt` still names the previous release.

## 8. What a regression triggers

A **regression** is a program whose 1.26 C#/Go ratio is worse than its 1.24.13.7 ratio in the same column, by more
than the larger of the session's A/A null for that column and that row's own spread. Improvements are reported by
the same rule.

1. **Re-measure that row alone** in a fresh quiet window: paired, interleaved, 11 runs, control first. If it does not
   hold, it is recorded as noise, with both readings.
2. **If it holds, it is a finding, not a blocker.** Nothing published is held back. The numbers bank as measured, and
   "Reading the results" names the regression.
3. **G posts one PERF-REVIEW line to COORD:** the row, the column, the magnitude against the floor, and the control
   reading. It names the candidate cause by bracket: the Go column moved (Go's own source); the converted package
   source moved (the hop's stdlib port); golib or a generator moved; or the emission moved (compare the two legs'
   converted `.cs`). A probe sized to that bracket follows.
4. **A BOARD row** carries it until a seat retires it or a measurement shows it is Go's own.
5. **A Verify failure is not a regression.** Output that differs from Go's is a correctness defect: a BOARD finding
   and a converter or golib fix. That row publishes no number until it passes.
6. **An AOT build failure on the i9** (ILC or link) is a deployment defect with the same treatment as 5: that cell
   reads "n/a" with its reason. It is never filled from another build.

## 9. Rulings (COORD, 2026-10-23)

- **O1: the R2R arm is the TEMPLATE-DEFAULT publish**, with its exact properties named in section 2. If P1's
  consumer-side ReadyToRun DATA recommends a different recipe (composite or framework-dependent), that recipe
  becomes a fifth, diagnostic arm, and the template question becomes a separate ruling.
- **O2: YES.** The i9's AOT publishes double for the same-session 1.24.13.7 comparand: 34 publishes instead of 17,
  plus the 4 PERF-REVIEW repeats.
- **O3: YES, both.** The published table gains a fourth column (ReadyToRun), and History gains this session's
  1.24.13.7 leg.
- **O4: the source findings are recorded** in the BOARD row of 2026-10-23 (section 6 cites it). IfaceCall creep and
  Channel join the review rows.
