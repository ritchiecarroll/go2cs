# CIMatrix — the on-demand OS matrix

> `.github/workflows/os-matrix.yml` runs go2cs's own instruments on GitHub-hosted runners, on
> demand only. It reaches platforms the project's own test machines do not cover — **both macOS
> legs above all** — and gives Linux a second, independent host. It is **never a merge gate**: the
> project's own machines and their measured budgets stay authoritative, and nothing this workflow
> produces lands in the repository. Its timeouts are scaled from the budgets measured on the project's
> own machines ([Budgets](#budgets)).
<!-- Pre-2026-10-26 wording: "It reaches platforms the fleet does not own ... and gives the Linux lane a second,
     independent host ... the fleet's machines and their measured budgets stay authoritative". The census-first
     pattern is the F-series of [`PLAN-linux-operation.md`](PLAN-linux-operation.md) (an internal record; its
     visible link was removed under the 2026-10-26 owner ruling OQ-1 on fleet language in visitor pages). -->
<!-- 2026-11-09 (audit F147, F148): the visible links to `.claude/rules/corpus.md` (corpus mechanics, GoTargetOS
     flavors) and `.claude/rules/harness-gates.md` (the budget table the timeouts are scaled from) were removed under
     OQ-1, with the sentence on the census-first pattern. -->

## The three fits

**1. The darwin census — the one thing nothing else can do.** Layout L3 emits a `darwin/` folder
for every platform-varying package, and `-p:GoTargetOS=darwin` selects it, so the flavor *exists*
in the corpus. Nothing else builds it **on Apple hardware** — and there are two of those, arm64 and
x64, which are different compilations of the same flavor. A darwin dispatch fans out to both, and
both compile clean. **Compile is the milestone, operational is later**, so the census is the
deliverable whether or not it is green.
<!-- Pre-2026-10-26 wording: "This is the Phase-3 doctrine applied to a platform". -->

**2. The native-Linux control.** A Linux reading taken under WSL shares its machine with a Windows
host. A hosted `ubuntu-latest` is a second Linux with a different kernel, a different
filesystem and no Windows host underneath it. A finding that reproduces on both is a property of
the platform; a finding that reproduces on only one is a property of the topology. That
distinction is worth a dispatch before Linux work is planned around it.

**3. Sweep-shard overflow.** The validated sweep is the project's longest-running check, and moving
the corpus to a new Go release multiplies it. When every machine is committed, a shard can run here
instead of waiting — filtered, one dispatch per shard, results read from the artifacts.
<!-- Pre-2026-10-26 wording: "**3. Hop-shard overflow.** The validated sweep is the fleet's long pole, and a corpus
     hop multiplies it." -->

## The one schedule — the darwin regression guard

**`goos=darwin stage=census` runs daily at 04:41 UTC, and nothing else does.** It is still not a
merge gate: no push trigger, no pull_request trigger, and no other flavor or stage on a timer. It is
a regression guard whose conclusion is read each morning.
<!-- Pre-2026-10-26 wording: "whose conclusion is read on the morning board". -->

**Why darwin and only darwin.** windows and linux are each compiled by standing gates on the project's
own machines, so a mechanical break in either surfaces the day it lands. **Nothing else compiles the
darwin flavor.** Without a schedule, a darwin-only break goes unnoticed until the next dispatch, however
long that is. A darwin census dispatch took about 10 to 17 minutes on the hosted `macos-15` and
`macos-15-intel` runners in September 2026.
<!-- The runner minutes: docs/phase4/DESIGN-darwin-run-layer.md section 6.1 (2026-09-02), "~10-17 minutes per round
     trip"; the runners: .github/workflows/os-matrix.yml, the darwin targets. -->
<!-- The dated instance (pre-2026-10-26 visible text): "On 2026-09-02 a one-line CS0266 — `var` inferring
     `StandardBox<T>` where the base `ж<T>` was needed, left in `os/darwin/dir_darwin_impl.cs` by the box-kind
     split's 754-site emitter flip (`36b7e9d96`) — was found **seven days** after it landed, by the first darwin
     dispatch since, on both mac legs byte-identically." -->

**A scheduled event carries no inputs**, so the `plan` job resolves the effective
goos/stage/filter/dotnet once — `schedule` takes the fixed darwin/census pair, a dispatch takes its
own inputs unchanged — and every consumer downstream reads `needs.plan.outputs.*` rather than
`inputs.*`. The resolution is positive-controlled in all four arms, negative control included (with
the schedule branch neutered, a schedule event falls to `linux`, not `darwin` — so the `darwin` in
the live arm is the branch firing and not a default agreeing by luck).

**Reading a red scheduled census:** it is a regression attributable to that day's merges, not a
newly discovered wall. darwin compiles clean on both architectures. Root it against the day's merges.
<!-- darwin has compiled clean since 2026-08-23 (census run 32649840220, zero errors on both architectures). -->
<!-- Pre-2026-10-26 wording: "attributable to that day's trains ... exactly as the CS0266 above was rooted". -->

## How to trigger

Actions → **OS matrix (on demand)** → *Run workflow*. Or from the CLI:

```
gh workflow run os-matrix.yml -f goos=darwin -f stage=census
gh workflow run os-matrix.yml -f goos=linux  -f stage=sweep-shard -f filter=compress/flate
```

| Input | Values | Meaning |
|:--|:--|:--|
| `goos` | `windows` · `linux` · `darwin` | The corpus flavor to bind. Each maps to a runner + RID pair; **`darwin` fans out to BOTH mac runners** (arm64 and x64) in one dispatch. |
| `stage` | `census` · `behavioral-smoke` · `behavioral-full` · `behavioral-stderr` · `sweep-shard` · `release-smoke` · `aot-smoke` · `nugetgo-behavior` · `golib-tests` · `stack-probe` · `converter-cgo` | What to run. One stage per dispatch. `behavioral-full` runs the whole behavioral enumeration in slices; `behavioral-stderr` runs named projects directly and keeps their whole stderr; `golib-tests` builds and runs `src/tests/GolibTests` for the GOOS on each of its runners, with `filter` as an MSTest `--filter` expression (blank runs the whole suite), and annotates each test's outcome by name. `stack-probe` is an opt-in diagnostic that never banks: it sweeps ONE row with `run-validated-sweep.ps1 -TestFilter` while `.github/stack-probe.ps1` takes `dotnet-stack` reports of that row's converted test host at fixed offsets (`filter` options below). It first proves the instrument on a deliberately hung program, and fails as blind if the planted frame is missing. `converter-cgo` is opt-in: it runs the converter's `go test` in `src/go2cs` on each runner, where a C compiler lets `go list` run cmd/cgo, with `filter` as a `go test -run` regular expression (blank selects the cgo tests), and opens its summary with the compiler `go env CC` names. It is the only hosted stage that gives the converter's compiler-gated cgo tests a compiler, including cgo Phase 1's exit gate. `filter` may end `;cc=zig` or `;cc=llvm-mingw` (windows only), with an optional `;sha=HEX`: the leg then downloads that pinned toolchain, refuses it unless its sha256 is the pin's (or `sha=`'s, which exists to show the refusal), and only then hands it to go as `CC`. |
| `filter` | free text, optional | The shard. Comma-separated **exact** roster package names for `sweep-shard` (each swept with `-Exact`, and a name not on the roster refuses the run), a project-name substring for `behavioral-smoke`, comma-separated project names for `behavioral-stderr`. `behavioral-full` ignores it. Blank takes the stage's documented default. |
| `dotnet` | `10.0.x` · `10.0.402` | The SDK channel for `setup-dotnet`. Blank takes `10.0.x`. |
| `run_timeout` | 1-1800, optional | The behavioral stages' per-program run budget in seconds (`GO2CS_RUN_TIMEOUT`). Blank takes 120. |
| `published_version` | a four-part go.* version, optional | `release-smoke` only. Blank packs this tree and consumes that pack (the pre-publish gate). Set, the pack is skipped and every leg restores that version from nuget.org (the post-publish smoke). |
| `nugetgo_module` | `MODULE@VERSION` | `nugetgo-behavior` only, and required there. Its sample program and mappings row must exist at `src/tests/ModuleBehavior/MODULE@VERSION/` on the dispatched ref. |
| `nugetgo_package` | a NuGet package version | `nugetgo-behavior` only, and required there: the rehearsal package every row runs (e.g. `1.1.0-int.1`). |
| `nugetgo_feed` | a NuGet v3 feed URL, or `git:REF:FOLDER`, optional | `nugetgo-behavior` only. Blank takes int.nugettest.org, where module releases are rehearsed. `git:REF:FOLDER` is a folder of nupkgs in this repository at a branch or commit, for a package no gallery serves: the plan resolves REF to one commit, and each leg extracts the folder and passes it to the row as a local folder feed. The go.* closure still restores from nuget.org. |
| `nugetgo_feed_sha256` | 64 hex digits | `nugetgo-behavior` with a `git:` feed only, and required there: the SHA-256 of the package's nupkg in that folder. Each leg refuses a nupkg that hashes otherwise. Refused with a gallery feed. |
| `aot_bound_minutes` | 1-340, optional | `aot-smoke` only: the AOT step's own limit. Blank takes 330 (5 h 30 m). An expired bound reads NOT MEASURED (timeout) on osx-x64 and fails the leg on linux-x64 and win-x64. A small value makes the bound fire on purpose. |
| `arch` | `x64` · `arm64` · `both` | `goos` `linux` only. `x64`, the default, is the `ubuntu-latest` leg (`linux-x64`) every linux dispatch runs; a dispatch that omits the input, and the schedule, derive the same matrix as before the input existed. `arm64` runs the `ubuntu-24.04-arm` leg (`linux-arm64`) instead, and `both` runs the two. Until the linux/arm64 flavour exists (no `linux-arm64/` folder under `src/core`), the arm leg is a **calibration**: it builds the linux/amd64 flavour, so it runs `census` and `golib-tests` only, its environment report says `calibration: YES`, and it banks nothing. Every other stage refuses it by name, as do `release-smoke`, `aot-smoke` and `nugetgo-behavior`, which run the shipped RIDs. Its budgets take the mac legs' 1.5x until its own runs replace them. |
| `filter` options | `;key=value` pairs after the stage's own filter, no `;` inside a value | The dispatch form's input count is guarded (`Test-NugetgoBehaviorProof.ps1`), so the two diagnostic stages take their options here, parsed and checked by the plan job. `golib-tests`: `;jit=PATTERN`, a `DOTNET_JitDisasm` pattern; with tiering off, the JIT's listing of each matching method is printed to the job log (e.g. `FullyQualifiedName~P256FieldDifferential;jit=p256Mul p384Mul`). `stack-probe`: `ROW;test=REGEX[;at=S,S]` -- `test` is required and passed as `-TestFilter`, which filters both sides of the compare; `at` is when to dump the test host, in seconds from when it starts (default `120,870`; the per-test deadline is 900 s). `sweep-shard`, on goos `darwin` only: `ROWS;dns=IP[,IP]` sets the job's DNS servers on every enabled network service before the sweep (an opt-in for `net`'s `TestLookupNoSuchHost` NXDOMAIN keys, which the hosted mac resolver answers late), flushes the caches, logs the effective resolver, and times NXDOMAIN for `invalid.invalid` through Go's resolvers before and after as `DNSPROBE` lines. An unknown or repeated key, an empty value, or an option on any other stage fails the plan by name. |

There is no `all` value: one dispatch per flavor keeps each leg's artifacts, budget and verdict
separate, which is what makes a wall readable. Dispatching all three is three cheap clicks.

<!-- Pre-2026-10-26 wording said "lane" for "host" throughout this paragraph (a restricted-egress cloud session). -->
<!-- 2026-11-09 (audit F155): the paragraph the comment above refers to, on dispatching from a restricted-egress host,
     was removed. -->

The Go toolchain is **derived, never written here** — the workflow reads `<GoStdLibVersion>` out of
`src/version.props` and hands that exact release to `setup-go`. Both the converter and the sweep
refuse a toolchain whose release differs from that pin (such a run is NOT MEASURED, never a
verdict), so a hardcoded version in CI could only duplicate the pin or silently contradict it.

## The stages

### `census` — compile the corpus flavor and bucket the wall

Builds `src/go2cs-stdlib.slnx` at `-c Debug -m --no-incremental -p:GoTargetOS=<goos>
-p:UseSharedCompilation=false -clp:ErrorsOnly`. `--no-incremental` is not optional for a
target-changing pass: what differs between flavors is the `<Compile>` **item set**, not any source
timestamp, so an incremental build validates the other target's assemblies.

It then reports the census the way the repository measures one — **packages-compiling first**:
projects under `src/core`, assemblies actually produced (each project's own
`bin/Debug/<TargetFramework>/<AssemblyName>.dll` — the probe **derives** that folder rather than
spelling it, so a target-framework change cannot make it report zero on a green build), diagnostics bucketed
by code (raw and distinct), the same buckets by project, and the first distinct errors inline.
Everything lands in the job summary and in the artifact. Raw error count is deliberately *not* the
headline: fixing a file-inclusion defect raises it by surfacing what was hiding behind the failure.

### `behavioral-smoke` — prove the harness runs on the runner

Builds the converter explicitly (`go build` in `src/go2cs`) and then runs a filtered
`run-behavioral.ps1` pass over a small pure-compute family — the `Defer` projects by default:
defer/panic/recover ordering printed through `fmt`, with no clock, filesystem or socket in them, so
a failure indicts the harness or the corpus flavor rather than platform semantics.

The explicit converter build is there on purpose. A hosted runner has no `go2cs` binary at all, and
every harness rebuild predicate keys on a converter `*.go` being newer than the binary — which a
toolchain change never touches. Building it as its own step also separates a Go-toolchain failure
from a harness failure in the log.

### `sweep-shard` — a filtered validated sweep against `go test`

Runs `run-validated-sweep.ps1 -Filter <name> -Exact` under `pwsh` once for each roster package
named in the filter, with `setup-go` supplying the
`go test -json` baseline toolchain the comparison needs. `container/heap` is the default filter, a small,
quick row, and the right answer to "does a sweep run here at all".
Per-row evidence (each swept package's comparison record and results files) is collected into the
artifact under `rows/`, alongside the full log and a post-sweep `git status` of `src/core`.

Keep shards small. The sweep's own per-package deadline floors reach into the hours, and the job
budget is a ceiling, not an allowance.

### `release-smoke` — consume a rehearsal feed the way a user does

The release's own pack, rehearsed, then consumed on the chosen flavor's legs. A `pack` job on
`windows-latest` runs `push-nuget.ps1 -VersionSuffix ci.<run id>` under Windows PowerShell — pack-only by
the script's own contract (no bump, no tag, nothing under `docs/validation`, no push) — and uploads the
merged packages as the `release-smoke-feed` artifact. It is the only Release-configuration build of the
darwin flavor anywhere. The stage fans out to ALL FOUR shipped RIDs (win-x64, linux-x64, osx-arm64, osx-x64) from that
one pack, whatever `goos` says. Each leg then runs `src/tests/PackageTests/release-smoke.ps1` against the feed,
every arm restoring into a fresh cache with `go.*` mapped to the feed alone:

- **A** the RID-selected compile asset gate (`RidCompileAsset`): rid-less, `-r`, publish, and its control;
- **B** a small generated stdlib program converted with `-recurse=nuget`, its stdout compared with `go run`
  (it prints `runtime.GOOS`, so a wrong-flavor load is a visible mismatch, and it calls every `sort` form, so a
  self-recursive converted body fails the release as a named stack overflow; its `BuildTag =`
  line fails it when the converter's default tags and the baseline's `go run -tags safe` disagree);
- **C** `Behavioral/StatLayoutTruth`, converted and compared the same way;
- **D** the README walkthrough (`fatih/color`), compared the same way — it **gates on every leg**, darwin
  included, since `x/sys/unix` reaches libc on a Mac.
  <!-- Pre-2026-10-26 wording: "darwin joined windows and linux once darwin-xsys-libc (TRAIN O) made `x/sys/unix`
       reach libc on a Mac." -->
- **E** `PublishSymbols`: the dependency symbol files survive a second single-file publish (the guard) and the target
  leaves a Native AOT publish untouched (the AOT control), against the same feed. **Measured, never gating** until it
  has read green on two separate merges to master; every `go.lib` before the symbols target fails the guard.
  <!-- Pre-2026-10-26 wording: "until it has read green on two trains" (a train is one batch of branches landed on
       master together). -->
- **F** `PackageSymbols`: the packages' OWN symbol files reach a consumer -- a std frame prints its Go file:line on
  `dotnet run`, on a first and a second unchanged single-file publish and on a framework-dependent publish, the off
  switch prints `none`. **Measured, never gating** on E's terms; every release before the one that ships symbols fails
  RUN, PUBLISH and FDD. Its Native AOT run is the `aot-smoke` stage below, not an arm here.
- **G** a consumer of `runtime/coverage` and `runtime/debug`, converted and compared the same way, then the assembly
  NuGet hands this RID for `go.internal.coverage.decodemeta` and `go.runtime.debug` (`runtimes/<rid>/` when the
  package carries one, else `lib/`) read for the OS's syscall buffer type. It must not name the other OS's type
  (`WSABuf` off Windows, `Iovec` on it), and a `runtimes/<rid>/` twin must name this OS's type; an assembly that
  names neither is platform-neutral and passes. The consumer loads decodemeta at init, so a build for the wrong OS
  fails the run at startup (`TypeLoadException`), and the assembly check names the cause. It gates.

A leg is green when arms A-D and G pass. Dispatch it at the tree that ships, before every release:
it is what proves the packages carry what that tree's converter emits on every OS (a converter that emits
a runtime API the packages lack fails here). The pack job's budget is its own
(240 min; measured 38-53 min).
<!-- Pre-2026-10-26 wording: "(C2's 2026-10-03 finding: N's converter emits `unsafe.ArrayPointer`, which the
     published 1.24.13.3 lacks)". C2 and N were fleet lane names. -->

**After the publish**, the same stage with `published_version` set to the version just pushed
(`-f published_version=<version>`) skips the pack and runs the same arms on the same four RIDs against
nuget.org itself, `go.*` and everything else restored from there: the release as a user receives it. It is
dispatchable once every id in the walkthrough's restore closure is listed (a partial index fails restore loudly,
NU1102). The input is refused on any other stage and must be four numeric parts.

### `aot-smoke` — a Native AOT publish of a package consumer

The same `pack` job, then ONE Native AOT publish and run of `PackageSymbols` (a hand-written consumer that sets no trim
mode of its own) on all FOUR shipped RIDs (win-x64, linux-x64, osx-arm64, osx-x64): `test-package-symbols.ps1 -AotOnly`. The executable must exit 0 and print
its `PACKAGE-SYMBOLS` line (the value printed, not judged); the publish wall time is a `READ` line, and so are two ILC
readings, measured and not judged: `READ (ILC cycle-cutoff methods)` (distinct "will always throw" lines ILC logs when it cuts a
self-nesting generic instantiation) and `READ (IlcCompile ms)` (from MSBuild's performance summary). The cut-off count is
deterministic; IlcCompile is not (measured 2026-10-14: one tree read 7,001 s and 5,444 s on hosted linux, a 22% spread,
and 6,340 s and 9,897 s on hosted windows, a 56% spread; runs 38555847362 and 38576996407), so a single-run IlcCompile
comparison is read against its RID's band. A consumer takes
go.lib's `TrimMode=partial` default, under which the publish compiles every referenced go.* assembly whole. For this
fixture's 31 assemblies that takes about 3 min on hosted linux and windows, 6 on osx-arm64 and 11 on osx-x64, with 0
cycle cut-offs on every RID, since golib's generic-cycle fix (2026-10-14) broke ILC's generic cycle in golib (runs
38575258530 and 38587876788).
<!-- Pre-2026-10-26 wording named the fix by its branch: "since e2move broke ILC's generic cycle in golib on
     2026-10-14". -->
It is read before every release like the census, and a red holds the release. It reads this tree's pack only:
`published_version` is refused for it.
<!-- Pre-2026-10-26 wording: "**Non-gating on its first train (Q)**; from the release after, read before every
     release like the census, and a red holds the release." -->
**osx-arm64 is read, and gates like linux-x64 and win-x64.** On the hosted `macos-15` (3-core arm64) runner it
publishes in about 6 min (run 38587876788).
<!-- Pre-2026-10-26 wording: "**osx-arm64 is read, and gates like linux-x64 and win-x64** (2026-10-14). From
     2026-10-07 it was NOT MEASURED ... (array/ж/slice, 35,336 cut-off methods on linux at master). With the cycle
     broken (e2move, merge b1ba1380b4) run 38587876788 published osx-arm64 in 354 s (0 cut-offs, exit 0), and the
     plan's filter and every leg's NOT MEASURED line were retired." -->
**osx-x64 is BOUNDED, not open-ended.** The AOT step stops at `aot_bound_minutes` (330 by default), inside the 6
hours a hosted job may run. On
osx-x64 the step is continue-on-error and the next step turns an expired bound into "NOT MEASURED (timeout)" with exit 0,
while a publish that ENDED with a non-zero exit stays red. linux-x64 and win-x64 keep their gate: an expired bound fails them.
<!-- Pre-2026-10-26 wording: "**osx-x64 is BOUNDED, not open-ended** (2026-10-14): T4's osx-x64 publish ran past 6 h
     ..." (T4 was a fleet run label). -->

### `nugetgo-behavior` — one sample row per shipped RID

A third-party module with no Go tests cannot be validated by `go2cs -recurse -tests`, so its nugetgo package ships a
behavioral proof: one row per shipped RID, each written ON a host of that RID. A module WITH Go tests may carry the
same four rows beside its test proof, never in its place: its page gains a "Sample behavior" section and its README a
Sample behavior badge, and the rows replace a local-feed sample run in the module publishing procedure. No row is
inferred from another, so the stage runs the same four legs as `release-smoke`, whatever `goos` says, with no pack.
<!-- Both rules were COORD rulings of 2026-10-12. Pre-2026-10-26 wording: "the rows replace the owner's local-feed
     sample run in the runbook's section 3" (the runbook is docs/NugetgoPublish.md, an internal record). -->
Each leg builds the converter and runs `src/tools/nugetgo/Invoke-NugetgoBehaviorRow.ps1`: the sample program under
`src/tests/ModuleBehavior/MODULE@VERSION/` is run with `go run`, converted against `nugetgo_package` on
`nugetgo_feed`, built with `-r <rid>` and run with dotnet. The row records both outputs' hashes, the module assembly
that loaded, the RID the host's dotnet reports, the GOOS go reports, and the host as the runner label, `ImageOS` and
`ImageVersion`. The leg exits non-zero when the outputs differ; the row is still written, as the evidence.

The row is annotated in its OWN step as one base64 notice titled `nugetgo-row <rid>`, and kept in the artifact.
`src/tools/nugetgo/Get-NugetgoBehaviorRows.ps1 -RunId <id> -OutDir <dir>` reads the four rows back through the
annotations API, and `nugetgo-pack.ps1 -BehaviorProofDir <dir>` binds each to the bytes the release packs for its RID.
The plan refuses the three `nugetgo_*` inputs on any other stage, a malformed value, and a module with no sample at the
dispatched ref.

```
gh workflow run os-matrix.yml -f stage=nugetgo-behavior \
  -f nugetgo_module=github.com/inconshreveable/mousetrap@v1.1.0 -f nugetgo_package=1.1.0-int.1
```

## Results flow

Nothing is automatic and nothing is written back. Every leg uploads an `artifacts/` bundle
(**7-day retention**, uploaded on failure too) and writes its headline table into the run's job
summary, so a verdict is readable without downloading anything.

Every leg ALSO echoes its summary as **annotations** (`.github/annotate-summary.ps1`). That is
not decoration: a job summary and an uploaded artifact are both served from GitHub's Azure blob
storage, reached by a redirect from the GitHub API, so a host
whose egress policy allows the API and denies that storage domain — which is the position a
restricted-egress cloud host is in — can dispatch a run, read every job and step *conclusion*, and
not one line of what the run measured. Annotations come back from
`GET /repos/{owner}/{repo}/check-runs/{id}/annotations` as JSON from the API itself, so the
headline survives that block, and it lands on the run page where a reader sees it without opening
the summary tab. The summary and the artifact are still written in full; this is a second,
cheaper-to-read copy. GitHub caps annotations at ten per level per step, so the helper packs the
text into a few chunks, stays clear of the length cap, and — when a summary is longer than the
budget — **says how much it dropped** rather than truncating silently.

A `sweep-shard` summary carries, above its tail, each FAILING row's comparison record as lines: its
status, how many verdicts matched, how many are disclosed, the undisclosed divergence count, and the
first 40 undisclosed divergences with both verdicts, each followed by the C# side's first 8 failing lines from the
row's `go2cs_test_results.json` (`Get-ComparisonDivergenceLines`, `src/_roster.ps1`,
guarded by `src/tests/sweep-row-divergence-selftest.ps1`). The C# FAIL listing in the log shows one
shape only; a Go pass / C# skip divergence, a Go skip / C# pass one or a one-sided row is readable from
these lines alone on a host that cannot fetch the artifact.

**Report a result in the repository's terms.** If you trigger a run, read the summary, download the
artifact if the detail matters, and report the finding the way the repository measures it — a census
as a census (assemblies produced, buckets by code), a sweep row as its arithmetic.
<!-- Pre-2026-10-26 wording: "**The triggerer owns the relay.** ... and carry the finding to the status board or the
     fleet mailbox in the same words the fleet uses". -->

Read a sweep row from its **comparison record**, never from its pass line: the disclosed count is
not on the pass line, so a row summarized from it under-reports by exactly the disclosures — and a
disclosure count is half of what makes a row honest.

## Reading a run

- **The checkmark answers the instrument, not the exercise.** Each stage exits with its own tool's
  verdict, so a darwin census that walls shows red — and that red *is* the deliverable. Read the
  summary.
- **A non-Windows sweep exits non-zero even when it validates.** Rows with no per-OS annotation in
  the roster report as **comparison-validated-at-count**: the comparison matched verdict for
  verdict, but nothing is recorded for that OS to compare against. That is a reading, not a
  regression, and it retires row by row as annotations land.
- **A timeout is NOT MEASURED, never a failure of the corpus.** If a leg dies on its budget, raise
  the named budget and re-run; do not read it as a wall.
- **The disk preflight is bypassed here.** Hosted images sit near or under the harness's free-space
  floor, so the harness stages pass `-IgnoreDiskPreflight`. The free-space number is recorded in
  each leg's environment report *before* anything runs, so a genuine mid-run exhaustion — which
  surfaces as corpus failures, not as a disk message — can still be recognized rather than guessed.
- **The environment report is calibration data.** Cores, RAM-class, toolchain versions and free
  space per runner: the dispatches are what anchor these runners the way measured budgets anchor
  the project's own machines. The census, `pack` and `aot-smoke` budgets are measured; the others
  are estimates.

## What this is NOT

- **Not a merge gate, ever.** No `push` trigger, no `pull_request` trigger, and only the one
  daily darwin census on a schedule ([above](#the-one-schedule--the-darwin-regression-guard)). A
  merge that adds a validated row still takes its proof from the project's instruments on the
  project's own machines.
- **Not a substitute for the validated sweep or the behavioral suite and its regression check.** It runs the same scripts, but a
  hosted runner is an unanchored machine; a number measured here is provisional until one of the
  project's own machines agrees or the runner is itself calibrated.
- **Not a place that writes to the repository.** It has read-only permissions, records nothing,
  updates no roster row and pushes no branch. A validated count seen here is a *candidate* for the
  roster through the normal validation policy, on a pinned machine.
- **Not a budget baseline.** The timeouts below are scaled estimates, except for the census, the
  `pack` job and `aot-smoke`, which are measured on hosted runners; those readings are in the stage
  sections above and in the `aot-smoke` row.
<!-- Pre-2026-10-26 wording: "A banking merge's proof still comes from the fleet's instruments on the fleet's
     machines" / "provisional until a fleet machine agrees" / "banks nothing ... a *candidate* for banking through
     the normal policy". -->

## Budgets

| Stage | Base | macOS legs | Scaled from |
|:--|--:|--:|:--|
| `census` | 150 min | 225 min | The full stdlib solution build `--no-incremental` on the project's slowest anchored machine, plus a **cold** NuGet restore of every project. |
| `behavioral-smoke` | 90 min | 135 min | Converter `go build` plus a filtered four-phase run whose shared core closure is built from nothing. |
| `behavioral-full` | 150 min | 225 min | The whole behavioral enumeration in slices, with a purge between. Provisional. |
| `behavioral-stderr` | 60 min | 90 min | Converter `go build`, a transpile and compile of a handful of named projects, and one direct run of each. Provisional. |
| `sweep-shard` | 210 min | 315 min | One convert/build/run/compare cycle per matched row; the sweep's own per-package floors are the reason this is the largest. |
| `golib-tests` | 90 min | 135 min | One Release build of `GolibTests` and its core closure for the GOOS, from cold, then `dotnet test` (the whole suite, or a `--filter`). Provisional. |
| `stack-probe` | 90 min | 135 min | One row's `-tests` convert and build, the filtered compare, and up to the 900 s per-test deadline while the probe waits. Provisional. |
| `converter-cgo` | 15 min | 23 min | The converter's `go test` filtered to its cgo tests: one cold module download and build, plus one converter `go build` for the driver-level arms. Provisional. |
| `release-smoke` | 90 min | 135 min | The leg only: converter `go build` plus seven small restore/build/run arms. The `pack` job (windows, 240 min) is separate. Provisional. |
| `nugetgo-behavior` | 90 min | 135 min | The leg: converter `go build`, one `go run`, one conversion against the feed, and one cold restore, build and run of the sample. `release-smoke`'s leg is the closest shape. Provisional. |
| `aot-smoke` | 240 min | 360 min | The leg only, four RIDs: one Native AOT publish of a 31-package consumer under `TrimMode=partial` -- linux 56, windows 71, osx-x64 293 min before golib's generic-cycle fix (37523439849, where osx-arm64 did not finish in 360); after it, linux 212 s, windows 167 s, osx-arm64 354 s (38587876788), osx-x64 654 s (38575258530). |

<!-- Pre-2026-10-26 wording: the census row said "on the fleet's slowest anchored box", and the aot-smoke row named the
     generic-cycle fix by its branch, "before e2move". -->

The macOS multiplier is 1.5x because `macos-15` (arm64) is a 3-core runner and every phase here is
parallel-MSBuild bound. GitHub's hard ceiling for a hosted job is 360 minutes and every value stays
under it. A timeout is a safety net against a hung child, never a performance assumption — these
sit well above the top of each scaled range on purpose.

`BehavioralRunner` carries **its own** budgets that no caller-side timeout can influence, so the
smoke stage sets them explicitly through their documented environment variables
(`GO2CS_BUILD_TIMEOUT` and friends), sized for a cold four-core runner where the Transpile phase
rewrites every `.cs` immediately before Compile and warm state can never make the run a no-op.

## Notes on the darwin legs

What a darwin leg establishes, and what it cannot, so a failure is recognized rather than re-diagnosed:

- **The darwin corpus compiles on macOS.** It builds with zero errors on `osx-arm64` and `osx-x64`
  (census run 32649840220). The two compile the same sources, so a divergence between them is a
  finding about the corpus, not about macOS.
  <!-- Settled 2026-08-23. -->
  <!-- Settled 2026-08-23. -->
- **Converted programs run on both legs.** A darwin `behavioral-full` runs on both legs (run
  33898025427), and the [roster](ValidatedTestPackages.md) shows a macOS count for each package that
  validates on both chips.
  <!-- Settled 2026-08-25; the behavioral-full run 33898025427 was by 2026-09-04. Evidence and remedy shapes:
       [`phase4/FINDING-darwin-run-layer.md`](phase4/FINDING-darwin-run-layer.md) (an internal record). -->
- **The `darwin` binding reaches MSBuild.** `GoTargetOS` in this workflow's `env:` block reaches
  MSBuild, and a macOS leg builds and runs the darwin flavor. The same env-only mechanism selects the
  flavor on Linux, and `fmt`'s closure carries platform-varying packages including `os`, `runtime`
  and `syscall`, so a windows flavor would fault on `kernel32.dll` in every program. Do not
  re-diagnose a darwin failure as a flavor-binding failure.
  <!-- Settled 2026-08-25. -->
- **The behavioral transpile targets the host.** The converter defaults `-platforms` to the
  *host*, so a mac leg re-transpiles the behavioral corpus for `darwin/arm64` (or `darwin/amd64`)
  and compares the result against goldens captured on Windows. A golden mismatch on a mac leg can
  therefore come from the darwin conversion itself.
- **Case sensitivity.** Default APFS is case-**insensitive** like Windows, so the mac legs prove
  nothing about path casing. Linux remains the only filesystem that does.
- **The `macos-15-intel` image works.** Its `osx-x64` legs run the census, `behavioral-full` and
  `aot-smoke` (run 38575258530).
- **`pwsh` version drift.** The project's Linux readings come from one specific pwsh; hosted images
  ship whatever they ship. A PowerShell-level failure on a runner is a portability finding about the
  instrument, not about the corpus.
  <!-- Pre-2026-10-26 wording: "The Linux campaign's readings". -->
