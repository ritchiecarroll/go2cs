# PLAN — the post-hop performance re-baseline (Go 1.26 packages)

> **STATUS: DRAFT (G, 2026-10-23), for COORD's ruling.** Document type: an [instance plan](Glossary.md#plan). The
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
- **R2R** is new. It is the publish a consumer gets from the converter-written project with no extra properties:
  `dotnet publish -c Release -r win-x64`, which the template makes ReadyToRun, trimmed (partial) and therefore
  self-contained. It sits between the other two columns: precompiled code like AOT, but still tiering up under the
  JIT. It costs a small fraction of an AOT publish, so G-LAPTOP builds it. (Open question O1 confirms that this is the arm P1
  means.)
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
- **Disk:** at least 30 GB free before the AOT drop lands (34 images at about 300 MB each, plus build output).
- **The session carries its own control.** The 1.24.13 Go Fib and Sieve binaries run at the start, the middle and
  the end. If either control drifts by more than the session's A/A null (section 6), the window is void and is
  re-run, not averaged.

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

Three rows get extra treatment, because their number can move without their program changing:

- **RefLower and Iface: the layout lottery.** These rows can shift with code placement, which moves with unrelated
  changes elsewhere in the image. For the AOT arm, the i9 publishes each of these programs **twice per leg**. The
  pair's spread is that row's own A/A, and a leg delta smaller than it is reported as no change. For the JIT and
  R2R arms, these rows take 11 runs, and their spread is reported beside the median.
- **Sort: tiering.** Its workload is a few hundred milliseconds, so tier-up (call counting, OSR, dynamic PGO)
  lands inside it, and a shift in when tier-up fires moves the number. The published Sort number stays the default
  configuration, which is what a consumer runs. A diagnostic arm attributes any Sort leg delta to code rather than to
  tier-up timing: JIT and R2R under `DOTNET_TieredCompilation=0`, and again under `DOTNET_TieredPGO=0`. Diagnostic
  readings go to the record, never to the table.

(Open question O4: the findings behind these two items are not in the repository. If COORD's sources name other
rows or a different mechanism, this list is amended before S4.)

## 7. Steps

| step | what | where | gate |
|:--|:--|:--|:--|
| **S0** | Instrument seat: the runner gains a package-reference leg (convert with a named converter and `-recurse=nuget` at a named version), the R2R column, a hash-verified adopt-AOT path with a manifest, two-leg interleaved measurement, and the four-column table. Red-first: a planted hash mismatch refuses by name, a planted self-contained JIT binary is refused, and an unadopted foreign AOT binary is never timed. | G, a seat in a batch | lands before S2 |
| **S1** | The 1.26 publish lands. The i9 converts and publishes 34 AOT images, plus a second RefLower and a second Iface publish per leg (4 more), and ships them. | the i9 | manifests complete |
| **S2** | G-LAPTOP converts both legs, checks the source hashes against the i9's manifest, and builds Go x2, JIT and R2R. If ILC moved, it re-licenses on one row. | G-LAPTOP | hashes equal; licence holds |
| **S3** | Verify all 17 programs x 8 binaries. | G-LAPTOP | identical output |
| **S4** | Quiet window: A/A null, control, paired measurement, PERF-REVIEW extras, control again. | G-LAPTOP | control inside the null |
| **S5** | Bank: the runner's `--update-readme` writes the 1.26 table. The same session's 1.24.13.7 leg goes into History beside the retained 1.23.1 and .NET 9 tables. Raw medians, runs, hashes, box state and diagnostic arms go to `docs/phase4/DATA-hop126-perf.md`. | G, a docs seat | repoguard green |

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

## 9. Open questions for COORD

- **O1.** "P1's ReadyToRun" is read here as the converter template's default publish (section 2). If P1's arm is a
  different recipe (composite R2R, framework-dependent R2R, a pack-time R2R of the packages), name it and section 2
  changes.
- **O2.** The 1.24.13.7 comparand is re-measured in the same session (section 1). That doubles the i9's AOT work
  (34 publishes instead of 17, plus the 4 PERF-REVIEW repeats). The alternative is a 1.26-only AOT leg compared with no same-session 1.24.13.7 AOT
  figure, which the runbook does not admit as evidence.
- **O3.** The published table gains a fourth column (ReadyToRun), and the History section gains this session's
  1.24.13.7 table.
- **O4.** The PERF-REVIEW items' source findings (section 6).
