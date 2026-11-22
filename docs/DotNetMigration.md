# Migrating go2cs to a new .NET version

> The standing runbook for moving the repository — the runtime library, the converted standard
> library, the generated projects and the Phase-4 test host — from one .NET release to the next.
> It is **version-agnostic by design**: it names instruments, gates and traps, never a particular
> release.
>
> **This runbook leads.** It is the living procedure for a .NET migration, amended in-stage as
> lessons are learned; its first instance (the .NET 10 hop) was executed as written, every deviation
> fixing this document in the stage that found it. **Instance plans** supply a given hop's fleet,
> readings and sequencing ([`PLAN-hop-campaign.md`](PLAN-hop-campaign.md) was the first) and become
> records when their hop completes; the **ruling frame**
> ([`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md)) schedules .NET hops relative to Go hops and
> holds the ruled decisions. **A runbook edit never reopens a ruling**; where this document and any
> plan disagree about *procedure*, the plan is stale — fix this document if it is wrong, and mark the
> plan superseded in the same change.
>
> Companion: [`GoCorpusMigration.md`](GoCorpusMigration.md), the same procedure for a new **Go**
> release. The two are deliberately separate documents because they are deliberately separate hops.

**No frozen figures.** Corpus-sized quantities — project counts, roster rows, verdict totals,
diagnostic counts, wall times — are **named by instrument and re-measured at the hop**. Where a
budget matters, this document names the row in CLAUDE.md's measured budget table rather than copying
a number that goes stale between hops. A migration that asserts last hop's number has already
started lying.

---

## 1. The invariant: one variable at a time

> **A migration's schedule is set by the OUTGOING release's support window, not by the incoming
> release's readiness.** *"Is the new one ready"* is almost never the live question — the target is
> typically GA, and often LTS, long before the hop is scheduled; the question is how long the
> **current** target may remain supported. Read both dates at the migration, and state in the record
> which one makes the hop mandatory rather than discretionary. Worked instance, with the dates that
> made the .NET 10 hop mandatory-by-November:
> [`phase4/RECON-dotnet10.md`](phase4/RECON-dotnet10.md) §1.

A .NET migration moves the runtime and the target framework moniker. **It moves nothing else.** In
particular it does not move the Go toolchain, the corpus's Go release, or the converter's language
level — those belong to a Go corpus migration, which is a separate hop with separate gates.

The invariant is not stylistic. Almost every trap in §3 produces a *plausible number* rather than a
failure, and a plausible number is only attributable when exactly one thing changed to produce it.
The stage ladder in §4 exists to keep that true: each stage changes one thing and is gated before the
next begins.

### 1.1 What a .NET migration does NOT trip

Three guards dominate a Go corpus migration and **none of them fires here**. Stating this removes
work rather than discovering later that it was never owed:

| Guard | Why it is inert |
|:--|:--|
| `checkCorpusToolchainPin` (`src/go2cs/toolchainResolution.go`) | Compares the **Go** release in `version.props` against GOROOT's `VERSION`, exactly. No Go release moves |
| `checkNuGetStdLibCompatibility` | Compares the Go release's `version.Lang`. Same reason |
| Release-tag emission (`releaseTagsForVersion`, `src/go2cs/directiveOperations.go`) | Derives the `go1.1 … go1.N` build-tag set from the **Go** version. No `//go:build` constraint flips |
| False-green route #4 (the stale-`go2cs.exe`-after-a-toolchain-hop trap) | Its mechanism is that no harness's rebuild predicate observes the **Go** toolchain. A .NET migration installs no Go toolchain |

**One route it DOES touch**: §5.2's embedded-asset stale binary — the TFM stage edits files compiled
into `go2cs.exe`. The predicates cover it since 2026-08-22; read §5.2 for what that means for the
stage's gate accounting.

---

## 2. Stage 0 — provisioning the fleet

1. **Install the target SDK side-by-side**, to a user-local directory, via the official
   `dotnet-install` script with `-NoPath`. **Do not disturb the machine's existing default** until
   the deployment-shape review (§9) says so: every pre-cutover measurement is an A/B that needs both
   runtimes present, and removing the old one destroys the control.

   > **Authorization (first-execution finding, 2026-08-24).** A software download and machine-level
   > install is a **user-class action**. A pre-written command block — in a plan, a sibling's
   > provisioning note, or a coordination channel — is a *convenience, not an authorization*; a lane
   > parks and requests. That boundary was crossed by two of three lanes on this runbook's first
   > execution, self-reported by all three unprompted, and answered by the owner with a **standing
   > grant**: installing the toolchain THIS RUNBOOK NAMES — side-by-side, user-local, machine default
   > untouched — is authorized for the duration of a hop, on any fleet machine, without re-asking per
   > box. The grant does **not** extend to changing a machine default (§9's review, still a user
   > decision), uninstalling anything, or software the runbook does not name. Those still park.

   > **The install root is USER-RELATIVE, and the first invocation is a state change.** Two
   > mechanics the "side-by-side, user-local, default untouched" formula does not cover on its own:
   >
   > - **Derive the install directory from the running account** (`$env:USERPROFILE` / `$HOME`),
   >   never from another box's row. Fleet accounts differ, and a literal path copied from a
   >   sibling's provisioning row either provisions the wrong account's directory or fails
   >   outright. The provisioning note records each box's **resolved** root for exactly this
   >   reason — it is a record, not a template.
   > - **The SDK's first-run experience writes user-level state on the first `dotnet` call**, not
   >   at install time: a telemetry sentinel, and an ASP.NET Core HTTPS development certificate
   >   that **replaces** the account's existing one. Neither touches the machine default and
   >   neither is destructive to a build, but a box whose dev certificate is trusted for other
   >   work has had that trust invalidated by a provisioning step that claimed to change nothing.
   >   Set `DOTNET_NOLOGO=1`, `DOTNET_CLI_TELEMETRY_OPTOUT=1` and
   >   `DOTNET_GENERATE_ASPNET_CERTIFICATE=false` for the first invocation where that matters, and
   >   say in the row which way it went.
   >
   > **The canonical shape, both OSes — written out here because a row cannot carry it.** A
   > provisioning row records what a box *resolved to*, not what to type, and rows that cite each
   > other (*"same commands as the row above"*) terminate in no command at all:
   >
   > ```
   > # 0. BEFORE — probe BOTH hives (step 2). The path test is the one that catches a
   > #    side-by-side install the default hive cannot report.
   > dotnet --version ; dotnet --list-sdks ; dotnet --list-runtimes
   > <path test on the runbook's own install target>
   >
   > # 1. INSTALL — official dotnet-install script, user-local, no PATH change.
   > dotnet-install.{ps1,sh}  <channel-band>  <install-dir = derived from the running account>  -NoPath
   >
   > # 2. AFTER — both hives again. The DEFAULT's list must be UNCHANGED from step 0.
   > dotnet --version ; dotnet --list-sdks
   > <install-root>/dotnet --list-sdks ; <install-root>/dotnet --list-runtimes
   > ```
   >
   > Three things the shape encodes, each of which a fleet has gotten wrong: **`<install-root>`
   > derives from the running account** (`$env:USERPROFILE` / `$HOME`), never copied literally from
   > a sibling's row; **install by channel BAND and record the patch it resolved to**, per box,
   > because patch levels drift across a fleet and the band is the only portable instruction; and
   > **`--list-runtimes` on the side-by-side root is not optional** — the SDK carries its own host,
   > and the runtime patch it brings is the number every later leg's `FrameworkDescription` probe
   > must match. The SDK number alone does not imply it.
   >
   > Each box's **resolved values** — install root, SDK and runtime patch, both hives before and
   > after — are recorded in
   > [`phase4/STAGE0-provisioning.md`](phase4/STAGE0-provisioning.md). That file is the record; the
   > shape above is the procedure.
2. **Record both inventories per machine** — `dotnet --list-sdks` and `dotnet --list-runtimes` — in
   the machine's provisioning note, which lives in
   [`phase4/STAGE0-provisioning.md`](phase4/STAGE0-provisioning.md) (one section per machine;
   append, never rewrite — the note is the stage's record). *This location was the first
   execution's shakedown finding: the step named a note without naming its home.* Patch levels differ across a fleet, and a cross-machine
   comparison that does not name both SDK and runtime patch is not a measurement.

   > **Probe BOTH hives before declaring a box's state (first-execution finding, 2026-08-24 —
   > and the exact INVERSE of §2(3)'s hazard).** `dotnet --list-sdks` reads the MACHINE store; a
   > side-by-side install lives in a user-local directory it never reports. One fleet box was
   > declared "clean, no new runtime present" on that reading and in fact already carried the
   > full new SDK and runtime from an earlier scouting run — the probe was true of the default
   > hive and false of the machine. So the two hazards bracket each other: §2(3)'s is a
   > machine-default runtime that MASQUERADES as the side-by-side leg, this one is a
   > side-by-side install the probe cannot SEE. **A `Test-Path` on the runbook's own install
   > target, beside the two inventory commands, is one line and closes it.** Record both hives
   > in the row: default (SDKs, runtimes, and that it is untouched) and user-local (SDK,
   > runtime, and whether it pre-existed this stage).
3. **Verify the leg; never assume it.** A project targeting the *old* TFM continues to run on the
   *old* runtime even under the new SDK. Selecting the new runtime for a measurement leg takes an
   explicit environment (`DOTNET_ROOT` plus a roll-forward policy), and the selection must be
   **proved by a `FrameworkDescription` probe** whose output is recorded — probe before, probe after,
   probe again on restore. **A leg without a probe is not a leg**; it is the old runtime wearing the
   new one's name.

   > **The recipe, concretely** (trap 5 refers to this block as the one that "already spells the
   > fix", so it is written out rather than implied). A leg is **three** environment variables and
   > they are set together or not at all:
   >
   > | variable | value | why the leg breaks without it |
   > |:--|:--|:--|
   > | `DOTNET_ROOT` | the side-by-side root | without it an apphost-launched instrument silently uses the **machine-registered** install — trap 4, wearing the new runtime's name |
   > | `DOTNET_ROLL_FORWARD` | `LatestMajor` | without it every OLD-TFM binary in the leg fails to launch — trap 5 |
   > | `PATH` | the side-by-side root prepended | so a bare `dotnet` in a harness resolves to the leg's muxer rather than the default one |
   >
   > The probe itself is a program, not a CLI flag: a one-file console app printing
   > `RuntimeInformation.FrameworkDescription` **and**
   > `RuntimeEnvironment.GetRuntimeDirectory()`. Print both — the description alone cannot
   > distinguish two identically-versioned runtimes in different hives, which is exactly the
   > confusion a box carrying the new runtime under its machine default invites. The directory
   > names the hive; the description names the version.
   >
   > **Probe through the same launch path the instrument uses.** A probe run through the muxer
   > proves nothing about an apphost-launched harness in the same shell, and vice versa — trap 5's
   > measured matrix is why.
4. **Pin the SDK with `global.json`** once the TFM moves (§5), not before. During the SDK-only and
   baseline stages both SDKs must remain selectable by environment, which is precisely what a pin
   fights. Choose the roll-forward policy deliberately — a pin to a *major* with tolerance inside it
   keeps a contributor on a different patch level working; a pin to an exact patch does not.

   > ⚠ **Put nothing in it but the SDK pin — and specifically no `test` key.** `global.json` is also
   > where `dotnet test`'s **runner selection** is configured, so a key added in passing opts the
   > repository into a different test platform and changes the test host's shape in the middle of a
   > migration. That is a second variable, introduced in the one stage whose entire purpose is to
   > have one, and every testhost caveat the repository carries is written against the current
   > runner. A runner change may well be the direction of travel; evaluate it as a **separate item
   > after** the migration, never inside it.

5. **Gate the stage on BOTH lanes, with built artifacts.** An inventory listing proves a directory
   exists, not that the box can use it, and the stage's whole purpose is a machine that can build the
   new TFM *without* having lost the old one. The bar is two builds and two probes, on throwaway
   projects outside the repository:

   | leg | built by | bar |
   |:--|:--|:--|
   | a console app at the **new** TFM | the side-by-side SDK | builds; runs; probe reports the new runtime **from the side-by-side hive** |
   | a console app at the **old** TFM | the **machine-default** SDK | builds; runs; probe reports the old runtime from the default hive |
   | one **real repository project** at the old TFM (`golib` is the cheap one) | the machine-default SDK | zero errors — the old lane is load-bearing for the release ritual and is not proven by a scratch project |

   Scratch projects go outside the tree. The repository is not touched by this stage, and
   `git status` at the end of it says so.

**Fleet note.** Provisioning is per machine and can proceed in parallel with anything, because it
changes nothing in the repository. It is the one stage of a migration with no dependencies.

---

## 3. The toolchain trap catalog

The traps below, each measured on a real run, each stated here in the form that survives a version
change. **Every one produces a number rather than an error** — which is what makes the catalog worth
carrying. The numbering is stable: entries are appended, never renumbered, because they are cited by
number from other documents.

### Trap 1 — the AOT compiler binds to the TFM, not the SDK

The Native-AOT compiler (ILC) arrives as a **runtime pack selected by the project's target
framework**, not by the SDK performing the publish. A new SDK publishing an old TFM therefore
resolves the **old** ILC, and every AOT number it produces is the previous generation's codegen.

**Consequence for planning:** *no AOT measurement of the new runtime is reachable before the TFM
moves.* A migration plan that schedules AOT measurement early has scheduled an impossibility. AOT
becomes measurable at §5 and not before.

**How it presents:** an AOT leg on the new SDK reproduces the old SDK's AOT timings to within noise,
across benchmarks, in a way that looks like "the new runtime changed nothing" — a plausible null
result rather than a blocked measurement.

**Detection:** read the resolved ILC package version out of the restore. Do not infer it from the
SDK version; that inference is the trap.

### Trap 2 — the runner's up-to-date check reuses a publish across an environment change

Switching runtimes by environment variable changes nothing on disk, so a publish-artifact freshness
check sees a current artifact and skips the publish. The next leg then measures the **previous** leg's
binary.

**The tell is the clock.** A real AOT publish of this corpus compiles the full converted-stdlib
closure and costs minutes to tens of minutes on every machine class the budget table records. **A
publish that completes in seconds has not happened.** Treat an implausibly fast publish as a
stale-artifact alarm, never as a win.

**Remedy:** purge the benchmark's intermediate and output directories before any cross-SDK A/B. This
is the same class as the repository's documented `GoTargetOS` hazard — an item set or an environment
changes while timestamps do not — and it has the same remedy.

### Trap 3 — a new compiler adds new diagnostics to unchanged code

A new Roslyn emits warnings the old one did not, on source nobody touched. These are usually benign
and always noise at exactly the wrong moment.

**Procedure:** the SDK-only stage (§4) produces a **classified warning delta**, not a count. Each new
diagnostic is named and dispositioned once — benign, suppressed with a reason, or fixed — so that a
later stage's warning output can be read as signal.

### Trap 4 — an old-TFM app runs on the old runtime under the new SDK

The default is compatibility: the app targets the old framework, the old runtime is installed, and
that is what loads. This is trap 1's sibling on the JIT path and it silently invalidates any
"new runtime" measurement taken without explicit runtime selection.

**Remedy is §2 step 3**, and it is worth repeating because the failure is silent: probe
`FrameworkDescription` inside the measured process, record the output, and restore the environment
afterward with a third probe.

---

### Trap 5 — a side-by-side root runs no old-TFM app without a roll-forward policy

**Confirmed cross-platform on the first execution (2026-08-24, both fleet legs, same mechanism and
same fix).** A side-by-side install carries only the NEW runtime. An old-TFM app — including
`dotnet test`'s **test host** — asks the muxer for the old `Microsoft.NETCore.App`, does not find
it under that root, and aborts (`app-launch-failed`, "you must install ... to run this
application"). It presents as a harness failure at exactly the moment a stage's ladder is being
run, which is the worst moment to misread it.

**Remedy:** the LEG'S ENVIRONMENT carries the policy — `DOTNET_ROLL_FORWARD=LatestMajor` beside
`DOTNET_ROOT`. §2(3)'s probe recipe already spells it, which is why the probe succeeds while a bare
harness invocation in the same shell dies: *the probe accidentally pre-documents the fix.* Any
instrument run on a new-runtime leg inherits the same requirement — probe and harness must share
one environment, or they are measuring two different runtimes.

**Which instruments are exposed — the discriminant is whether the launch RESOLVES THROUGH the
side-by-side root, and apphost launch is not by itself a defence** (measured on the i7-5820K leg,
2026-08-24, a ten-cell matrix over an old-TFM console app and a new-TFM one; it **corrects** an
earlier reading of this trap that called apphost launch immune). Two independent mechanisms route a
launch into that root, and an instrument is exposed if **either** applies:

| launch path | what selects the framework search root | exposed when |
|:--|:--|:--|
| **muxer** — the side-by-side `dotnet.exe` invoked on a `.dll` (a pure test library, `dotnet test`-only) | the muxer **is** its own root; it searches its own install tree and nothing else | **always** — `DOTNET_ROOT` neither helps nor hurts, because the muxer ignores it |
| **apphost** — the instrument's own compiled `.exe` | `DOTNET_ROOT` if set, otherwise the machine-registered global install | **whenever `DOTNET_ROOT` points at the side-by-side root** — which §2(3) *requires* of a leg |

Measured, old-TFM app, side-by-side root carrying only the new runtime: muxer launch fails with
`DOTNET_ROOT` unset **and** set (`.NET location: <sxs root>` in both); apphost launch **succeeds** with
`DOTNET_ROOT` unset even with the side-by-side root first on `PATH` (`.NET location:` the global
install, running the old runtime), and **fails identically to the muxer** with `DOTNET_ROOT` set.
Adding `DOTNET_ROLL_FORWARD=LatestMajor` recovers both.

So the apphost's independence is from **`PATH`**, not from the side-by-side root — and that is the whole
of it. A shell with `PATH` pointed at the new root but no `DOTNET_ROOT` is a **half-constituted leg**:
its apphost instruments quietly keep running the OLD runtime while its muxer-launched ones die, which
reads as "apphost instruments are immune" and is really "those instruments were never on the leg."
That shape is what an apphost-immunity reading is built on, and it is the more dangerous half of the
trap, because a green apphost instrument in a half-constituted leg is a **trap-4 false measurement**
wearing a passing result. **Constitute the leg completely (§2(3)) and every instrument is exposed
equally** — which is the simpler rule, and the fix is the same one line of environment for all of them.

**The exposure INVERTS at the TFM stage, and its symptom is identical** (same matrix, new-TFM cells).
Once §5 moves the property, the binaries are new-TFM and the machine default is the root that lacks a
matching runtime: a new-TFM apphost launched with `DOTNET_ROOT` **unset** fails —
`.NET location: <global install>`, newest framework found the old one — while the same binary under
`DOTNET_ROOT=<sxs root>` runs. The failure text is the *same* `app-launch-failed` /
"You must install or update .NET" wall trap 5 catalogs, so at the TFM stage it invites the trap-5
reflex, and **`DOTNET_ROLL_FORWARD` cannot fix it**: roll-forward rolls forward, and there is no newer
runtime in that hive to roll to. `DOTNET_ROOT` is the fix, and it is now owed by **every apphost
instrument on the box** — the behavioral and performance runners, and every converted `package main`
the Output phase executes. A machine whose default hive already carries the new runtime (a VS or
servicing install) hides this by succeeding on the *wrong* hive, which is §2(3)'s hazard once more:
the two hives are then distinguishable only by the probe's runtime-directory line.

**Corollary trap, same family, found in the same hour:** a test run at the DEFAULT `$(GoTargetOS)`
on a non-Windows box fabricates failures that read exactly like runtime deltas — the Windows
flavor's P/Invoke surface loads `kernel32.dll.so` and dies in type initializers. **A leg's
instruments must name the flavor**, not inherit it.

### Trap 6 — a new language version flips a `params` call between its normal and expanded forms

**The overload-set audit is not enough, and this is the shape it misses.** A migration that surveys
overload SETS looking for a changed *winner* can correctly find none — every emitted argument shape
matching an overload by identity, and no rule outranking identity — and still ship a behavioral
change. A `params` method has **two applicable forms**, and the language prefers the **normal** form
whenever the argument converts to the collection type. So a new implicit conversion does not need a
second candidate to change a call's meaning: it only needs to make the normal form applicable where
previously only the expanded form was.

**How it presents:** no compile error, no ambiguity diagnostic, no new warning. One argument that
used to arrive as a single element arrives spread instead, and every value shifts by one level of
nesting.

**Detection was the VALIDATED SWEEP, not the behavioral suite.** The recorded instance surfaced as
one ordinary failed assert inside one package's own upstream test table, in a stage whose full
behavioral suite was green — the behavioral corpus simply does not exercise the argument shapes
upstream test tables do. **A language-level change therefore owes a validated-roster reading**, and
scheduling one is the whole of the remedy's detection half.

Two survey conclusions to read as retracted wherever this shape is in scope, because both are the
natural ones to reach and both are wrong here: *"the new conversions are widening, and a widening
cannot break existing code"* is **false** for a `params` slot; and *"a single-candidate surface is
structurally immune"* is exactly backwards — the single-candidate surface is the one that broke.

**Remedy at the CONVERTER, not at the runtime library.** A slice or array of the variadic element
type, passed as the sole argument of a non-spread variadic slot, is cast to the element type — which
is what the language's own preference then binds, restoring the source semantics. Fixing it in the
runtime library instead would mean withdrawing a conversion the corpus legitimately uses elsewhere.


## 4. Stage 1 — the SDK alone

**Changes:** the SDK on `PATH`. **Not** the TFM.

> **⚠ Stage 1 changes more than it looks like it does (recon finding, 2026-08-23,
> [`phase4/RECON-dotnet10.md`](phase4/RECON-dotnet10.md)):** every converted csproj pins
> `LangVersion=latest`, so the SDK hop alone recompiles the whole corpus as **C# 14** while still
> targeting net9.0 — and C# 14's first-class span conversions (`T[]` → `ReadOnlySpan<T>` as a
> built-in conversion) land squarely on golib's `slice<T>`/`@string`/`array<T>`
> implicit-conversion and overload surface. The one-variable invariant holds — the SDK IS the one
> variable — but the stage's record must state that the language version rode with it, and the
> behavioral suite is the detection net for any overload-resolution shift. Also from the same
> recon: the .NET 10 CLI writes informational output to **stderr**, a fresh surface for the
> repository's documented PS 5.1 `$ErrorActionPreference='Stop'`/NativeCommandError trap — audit
> harness call sites before trusting a Stage-1 red.

**Gate — the full compile-and-run ladder, by instrument:**

| Instrument | Bar |
|:--|:--|
| `dotnet build src/go2cs-stdlib.slnx -c Debug -p:UseSharedCompilation=false` at the default `$(GoTargetOS)` | zero errors; skipped-dependents enumerated and zero |
| the same at **every other supported `$(GoTargetOS)`** whose flavor currently builds | zero errors. ⚠ purge `bin`/`obj`/`Generated` between target switches — the `<Compile>` item set changes while timestamps do not, so an incremental build after a switch validates the *other* target's assemblies |
| `dotnet build src/go2cs.slnx` | zero errors — the only gate that compiles the non-generated solution members (utilities, examples) |
| `src/tests/Behavioral/run-behavioral.ps1` (all four phases) | green, zero `NOT MEASURED` |
| `GolibTests` | green |

**Warning delta classified** (trap 3), not counted.

**Not owed, and say so rather than skipping silently.** `check-no-regression.ps1` measures **converter
emission**, and the converter is a Go binary whose output cannot move because a .NET SDK changed. The
same accounting covers the converter's own `go test ./...`. Both become owed the moment a converter
source or **embedded asset** moves — see §5.2. State the accounting in the stage's record; the
repository's habit is *"not run; accounting stated"*, and it is what lets a reader tell a skipped gate
from a forgotten one.

---

## 5. Stage 2 — the target framework

**Changes:** one MSBuild property. **Nothing else.**

### 5.1 The bump is one line, and the inert copies are the interesting part

> **Instrument: [`src/migrate-tfm.ps1`](../src/migrate-tfm.ps1)** (user directive, 2026-08-24) —
> census by default, `-Apply` to act, `-WhatIf` honored. It edits exactly the SOURCE set (the
> property of record, the two emission templates, the nine embedded publish profiles, the CI SDK
> channel, the present-tense doc lines), refuses Class C by list with reasons, and self-verifies to
> zero remaining sites. What it does NOT do — the regens, the §4 gate ladder, CNR's accounting, the
> stale-profile purge, and the commit-message staleness statement — it prints as the operator's
> checklist, in CENSUS-tfm-inventory.md §8 order.

> **⚠ AMENDMENT (first-execution census, 2026-08-24 —
> [`phase4/CENSUS-tfm-inventory.md`](phase4/CENSUS-tfm-inventory.md)). The one-line claim is
> exactly right about csproj and INCOMPLETE about the harness.** Verified: all 1,127 tracked
> `.csproj` spelling the old TFM carry the identical conditioned form — no unconditional spelling
> exists — so the props edit really is the single project-side change and the emitted copies
> really are inert. **But the OUTPUT PATH moves with the property**, and nine sites across six
> files hardcode `bin/<config>/<tfm>/`: both runners' `NetVersion` constants
> (`BehavioralRunner`, `PerformanceRunner`), three PowerShell instruments, and the CI census
> step. Each is a FALSE-RED GENERATOR of the catalogued shape — the build succeeds, the probe
> misses, and the instrument reports a corpus failure (the behavioral runner would report
> hundreds of failures; the CI census would report "0 assemblies produced" on a green build).
>
> **Sequencing ruling: Class D (the path-deriving sites) is fixed FIRST, on the OLD TFM, and
> verified green there** — before the property moves. A ladder run through false-red generators
> produces a verdict that means nothing. **And the durable fix is already in the tree**:
> `BehavioralTestBase.cs` DERIVES its TFM from its own bin tail, which is exactly why it is the
> one C# harness a hop does not touch. The runners adopt that derivation and the PowerShell
> sites **derive** the TFM from `Directory.Build.props` through `src/_paths.ps1`; nine hand-edits
> become **none**, and the NEXT hop is free. A Stage 2 that only search-and-replaces leaves the same
> landmines for hop N+1.
>
> > ⚠ **The HOIST was tried here and falsified — this clause originally prescribed it** (*"the
> > PowerShell sites read one hoisted value from `src/_paths.ps1`"*), and the correction is the
> > generalizable half: **a hoist still needs an editor; a derivation needs nobody.** Prefer a
> > derivation wherever a single source of truth is readable from the consumer — and make it throw
> > rather than yield an empty value, because an empty derived TFM is the same false red the
> > derivation exists to prevent, in its worst form. **Where a hoist is genuinely unavoidable, it is
> > not complete until the migration instrument knows about the site it just created**: the hoist and
> > the instrument's entry for it land in the SAME commit, or the next migration pays for it exactly
> > as this one did.
>
> Three corrections the census also settled: **`push-nuget.ps1` is already TFM-agnostic** (its
> merge globs `lib/*`, unions dependencies by the nuspec's own `targetFramework` attribute, and
> every TFM spelling in it is a comment — its one code literal is `netstandard2.0`, a
> must-not-change); **publish profiles never self-heal** (the writer skips any profile that
> already exists and they are gitignored, so the template edit is an invisible no-op on every
> existing tree — delete them by hand); and **a deployed GOPATH tree does not receive the hop**
> (`deploy-core.ps1` writes its own props pinning only `$(go2csPath)`, so a deployed corpus
> builds at the emitted fallback until a regen reaches it).


`src/Directory.Build.props` owns the repository-wide TFM and says so in its own comment: *"The
repository-wide TARGET FRAMEWORK, and the one line a .NET hop edits."* The hoist is deliberate —
before it, a framework hop meant rewriting both converter templates, every hand-written project, and
then regenerating the whole corpus to level the emitted copies.

Three mechanics ride on it, all guarded or documented in place:

1. **Emitted projects keep a CONDITIONED fallback**
   (`<TargetFramework Condition="'$(TargetFramework)'==''">…</TargetFramework>`), because an emitted
   project does not always have this file above it: `deploy-core.ps1` stages the corpus under a
   GOPATH root with its own props, a `-recurse` conversion writes under an arbitrary output root, and
   a single-package conversion can land anywhere. Where the root props IS in scope it wins and the
   project's own line is inert; where it is not, the project still names a framework and still builds.
2. **A guard enforces the shape.** `TestTemplatesLeaveTheTargetFrameworkOverridable`
   (`src/go2cs/csprojMetadata_test.go`) fails a template that sets `<TargetFramework>`
   unconditionally — *"an unconditional value cannot be hoisted"* — and fails one that sets none at
   all. It runs under the plain converter `go test ./...`.
3. **Nested props files must import the root explicitly.** MSBuild stops at the first
   `Directory.Build.props` walking up; the existing nested ones each carry an explicit
   `GetPathOfFileAbove` import. **Any new nested props file owes the same import or it silently
   shadows the hop** for everything beneath it.

**After the one-line edit, the tracked mentions of the old TFM are inert but wrong-looking** — there
are thousands of them, and they level on the next regeneration. Two consequences worth scheduling
around:

- **A .NET migration does not owe a corpus regen for the TFM.** Forcing one spends a gate cycle to
  change a string no build reads. **State the expected staleness in the commit message** so a later
  reader does not diagnose it as drift, and let a Go corpus migration's own reconvert level it.
- **The families that are NOT inert must be enumerated by hand**, because they are load-bearing
  rather than cosmetic:
  - **`push-nuget.ps1`'s package layout.** The TFM appears in `lib/<tfm>/` (the compile-time asset
    and RID-agnostic runtime fallback) and `runtimes/<rid>/lib/<tfm>/` (the RID-selected runtime
    asset). These are NuGet asset-selection facts.
  - **The publish profiles** under `src/go2cs/profiles/` — one per shipped RID, each carrying a
    `<TargetFramework>` and a TFM-bearing `<PublishDir>`.
  - **The two converter csproj templates'** conditioned fallbacks.
  - **`src/go2cs/glyphSystemNames.go`'s name list**, read from the reference assemblies of the TFM it names
    (Microsoft.NETCore.App.Ref 10.0.0): every public type of System.ComponentModel, System.Diagnostics and
    System.Runtime.CompilerServices, each attribute under both its names. A package declaring one keeps the
    `global::` spelling of its package_info.cs System attributes (packageInfoSystemUsings.go). A new TFM can add
    types to those namespaces, and a name missing from the list is a CS0104 at build, not a silent change: re-read
    the list from the new reference assemblies and rerun the converter tests.

### 5.2 The embedded-asset false-green route — closed

**The TFM stage edits files that are compiled INTO `go2cs.exe`, and the harnesses now know it.**

- `src/go2cs/embeddedTemplates.go` `//go:embed`s the csproj templates, the `package_info.cs`
  skeleton, the icons and the whole `profiles/` directory into the converter binary — *"Embedding
  them makes `go2cs.exe` a single self-contained executable."* `stdlibMetadata.go` embeds
  `stdlib-metadata.txt` the same way.
- Editing one changes what the converter emits **without touching any `.go` file**. Until 2026-08-22
  every rebuild predicate asked whether a **top-level** `*.go` was newer than the binary, so the whole
  embedded set — plus the converter's `internal/` packages and `go.mod`/`go.sum` — invalidated the
  binary nowhere: 204 files seen against 224 real inputs. A template edit reported "up to date", the
  old template stayed embedded, and every runner gate validated the previous emission and printed
  PASS. A `runtime.Version()` toolchain stamp (§ route #4) does not cover this — a stamp says nothing
  about a template's modification time.

**What covers it now.** `src/tests/ConverterBuildInputs.cs` is the single definition of the
converter's build-input set, linked into `BehavioralRunner`, `BehavioralTestBase` and
`PerformanceRunner`; the embedded half is **derived from the `//go:embed` directives themselves**, so
a directive this stage adds is covered without anyone widening a list. Two guards ride the converter's
plain `go test ./...` (`src/go2cs/embeddedAssets_test.go`): the directive forms stay inside the subset
the resolver understands, and the three predicates still delegate to the shared helper. Editing a
template or a profile now rebuilds `go2cs.exe` on the next runner invocation, with nothing to remember.

**`check-no-regression.ps1` was never exposed**, and the reason is worth carrying into the stage's
accounting: it has no rebuild predicate at all — it runs `go build` unconditionally before
transpiling, and `go build`'s cache is content-addressed over embedded assets. Verified by
measurement: editing `csproj-template.xml` changes the linked binary's SHA-256, and reverting
reproduces the baseline byte-for-byte. So CNR's verdict on a TFM-stage template edit is trustworthy
on its own, exactly as it is for the stale-output and stale-toolchain routes.

**One residual, and it is narrow.** cmd/go's test cache drops files resolving outside the module root,
and the three predicate sources live under `src/tests`. A change that touches **only** harness C# can
therefore be served a cached PASS by the second guard; run `go test -count=1 ./...` from `src/go2cs`
after such a change. The first guard reads only in-module files and has no such gap.

### 5.3 The TFM stage's gate

The §4 ladder again, in full, **plus** the `-tests` pipeline proven on a representative row set — the
generated test host carries its own TFM fallback and its own publish shape, and it is the surface
§8 is about to change.

---

## 6. The performance re-measurement protocol

A .NET migration's whole CPU story is a comparison, so the protocol is about **attribution**, not
about running a benchmark.

**The paired same-session A/B is the only admissible evidence.** The repository's own doctrine, paid
for by a laptop measurement that read as a 30 % regression and turned out to be machine drift:
*"on a laptop, a perf comparison against an earlier-session baseline is not evidence — only a paired
same-session A/B is."* Both legs run on the same silicon, the same day, in the same session, on a
quiet machine.

**Protocol:**

1. **Capture the OLD-runtime baseline first**, at the stage *before* the variable moves. It is a
   separate, gated stage precisely so it exists.
2. **Run both legs over identical IL** where the TFM permits it — selecting the runtime by
   environment rather than by rebuild makes the delta pure runtime/JIT codegen, with nothing else in
   it. Verify each leg with the `FrameworkDescription` probe (trap 4).
3. **Carry a same-day control.** The Go binaries' columns must reproduce across legs within noise. A
   control that drifts invalidates the pass, and it is the cheapest possible check.
4. **Verify before timing, always** — `PerformanceRunner`'s standing doctrine: identical
   timing-filtered stdout across every variant before anything earns a number.
5. **Name the host, and name it as a host.** A scouting measurement taken on a machine that is not
   the perf-canon host produces ratios *internal to that box*. They are real and they are not the
   canonical table. Say which, in the record.
6. **Regressions are re-measured, not blocked on.** A narrow named regression on one box is a
   candidate, not a verdict; the migration's own measurement is what settles it. Report each whether
   or not it reproduces — a regression that vanished on other silicon is a finding about the scouting
   box, and worth as much as one that held.
7. **Bank through the instrument**, not by hand: the results table lives between the `PERF-RESULTS`
   markers in the performance suite's README and is rewritten by the runner's own flag, with prior
   toolchain tables accumulating in its *History* section — which is exactly the cross-version
   comparison that section exists for.

**Cost discipline.** A full AOT pass is hours and **must run solo**; concurrent load has pushed a
healthy publish past its watchdog. The publish also costs substantial disk, including a debug-symbol
file far larger than the executable. Run the disk preflight by hand first — the validated sweep
refuses below a documented free-space floor precisely because full-drive failures surface as *corpus
failures* that name everything except the disk.

### 6.1 Amendment — the control row is NAMED and allocation claims are COUNTED (hop-era, per the user's perf directive)

Two additions promoted from ratified doctrine into this protocol's numbered steps, and one statement
of where a scouting leg's numbers belong:

**Step 3 is strengthened: the control row is named IN THE RECORD, and read FIRST.** "Within
noise" is unfalsifiable until the row that *cannot* have changed is reported beside the rows that
could. The span-tranche measurement that ratified this: the untouched control row moved **+5.0 %**
and the unchanged Go binaries up to **+17 %** between two back-to-back same-session legs on the
perf-canon laptop — a noise floor several times a typical effect. So every banked comparison names
its control row (a benchmark the change cannot reach), reports it first, and a pass whose control
moved more than its subject rows is **void**, not "within noise". If the change reaches every
benchmark's path (a runtime hop does), the Go columns are the control — that is what step 3's
reproduce-across-legs already demands; this amendment makes the *reporting* of it mandatory.

**Step 4 gains a twin: where the claim is about ALLOCATION, gate by COUNT, never by time.**
`AllocationCounter` counts are deterministic and host-independent; a timing gate on a laptop
mostly measures the laptop (the ratified counting-gate doctrine, minted when a tranche of real
allocation deletions produced timing deltas smaller than the control row's drift). A migration
stage claiming "no allocation regressed" runs the count-gated GolibTests rows, not a stopwatch.

**And the pre-hop scouting is not a protocol step — it is a MEASUREMENT, and it lives in a record.**
A scouting leg's lessons that generalize are already in this document: the ILC binds to the TFM
(trap 1), an implausibly fast publish has not happened (trap 2), a new compiler's diagnostics are
classified rather than counted (trap 3), and an old-TFM binary runs on the old runtime unless
explicitly selected (trap 4). Its **numbers** — which ILC version resolved, what the publish cost,
what the Fib row read — belong to the hop that took them. Latest:
[`phase4/DATA-hopN-perf.md`](phase4/DATA-hopN-perf.md).

---

## 7. AOT / ILC verification

**Only reachable after the TFM moves** (trap 1). The procedure is **verify, then bank**, in that
order:

1. **Purge** the benchmark's intermediates and outputs (trap 2 — the seconds-long publish is the
   tell).
2. **Confirm the resolved ILC version from the restore**, not from the SDK version.
3. **Verify** — identical timing-filtered stdout across Go, JIT and AOT.
4. **Measure**, solo, on the perf-canon host, at the suite's own run counts.
5. **Bank** through the runner's update flag.

⚠ **An ILC publish of this closure has a real MEMORY floor, and it is a provisioning fact rather
than a tuning knob.** A full-corpus publish's peak working set has been measured in the **tens of
gigabytes** on a current compiler, so a machine sized comfortably for the build can still swap
through the publish — and per-box concurrency is bounded by that peak, not by core count, because
ILC is near-serial. Provision from the latest measured peak in
[`phase4/DATA-hopN-perf.md`](phase4/DATA-hopN-perf.md) and **re-measure it at each migration**: the
compiler's cost is one of the things a runtime hop moves, and it has moved by an order of magnitude
across a single release.

**State the expectation as a falsifiable prediction before running.** A migration whose AOT stage has
no prediction cannot be surprised, and the surprise is the information. If an external toolchain's
advantage was previously attributed to *"the newer ILC and framework pair it ships"*, then the new
TFM's own AOT numbers either reproduce that advantage — closing the attribution — or they do not, and
the attribution reopens with a sharper question. Both outcomes are worth the run; only an
unpredicted one is worth nothing.

---

## 8. Trim-safety audit

The deployment floor — binary size, startup time, working set — is dominated by **trim rooting
policy**, not by the compiler. A partial trim mode roots every assembly of the converted closure
whole; a reachability-based mode does not. The measured gap between them is large in every dimension
and the recovery is available from the stock SDK.

**The lever is fixed for a correctness reason, and that reason is binding.** golib's formatting and
the sort interfaces bind members through reflection, and full trim can remove exactly those. **A
sample of small programs passing is not evidence**; the population is the behavioral suite and the
validated roster.

**Procedure:**

1. **Run the audit build with trim warnings VISIBLE.** The performance tree sets
   `SuppressTrimAnalysisWarnings` in its own defaults deliberately — perf publishes should not fail
   on trim noise — and the standing disposition is that **the suppression must never be the reason
   the diagnostics go unread**. Override it locally on the audit build (`-p:SuppressTrimAnalysisWarnings=false`);
   never edit the perf tree's default to do it.
2. **Re-measure the diagnostics.** They concentrate in golib's adapter and reflection layer and in
   the converted `reflect` implementation. The count and its per-file distribution are **re-measured
   at each migration** — a new ILC adds and drops diagnostics — and a subset are dynamic-code
   diagnostics that apply to the *existing* AOT publish and are not introduced by trimming. Separate
   the two before pricing the work.
3. **Annotate** the named surface (`DynamicallyAccessedMembers`, `DynamicDependency`).
4. **Accept against the population**: the behavioral suite's Output phase and a validated-roster
   sample, both published under the candidate trim mode, with zero output divergence. Any divergence
   **names the reflective site the trim removed**.
5. **A trim-safe golib is a golib API-surface change**, so the standing rule applies: build
   `src/go2cs.slnx` once before banking. No other gate covers the non-generated solution members.

---

## 9. Host-shape changes, and their roster / manifest consequences

A .NET migration is the natural moment to re-price *publish shapes*, because publish shapes are what
it changes. When a shape change makes a previously-impossible test possible, the consequence reaches
the validation roster — and the disclosure machinery is **built to force that revisit** rather than
allow drift.

### 9.1 The mechanism

A disclosure is a Go assertion the converted suite provably cannot satisfy, pinned by **exact failure
signature** in a committed, hand-owned manifest. Disclosed tests **keep running in every sweep**. So
the day a deployment-shape change makes one of them satisfiable, it begins passing, **the sweep's
disclosure arithmetic breaks loudly, and the entry must be removed**.

That is the intended behavior, not a fault. Design the migration so it *expects* the break.

### 9.2 The worked pattern — a host-capability disclosure class

The pattern generalizes; the host-limit class is its worked example.

1. **Enumerate the constituency from the committed manifests, not from prose.** Prose drifts; the
   manifests are the authority. Count entries per package and note that a package's *disclosed
   verdict count* may exceed its entry count, because parent rows ride disclosed-parent aggregation.
2. **Separate the LEVERS.** Entries in one class do not all retire on one change. A relocatable
   single-file publish and a fast-starting publish are **different levers**, and a class can hold
   entries needing each. A migration that ships one lever retires the entries that need it and leaves
   the others standing — an honest outcome, but only if it was **predicted** rather than discovered.
3. **Identify the entries that never retire.** Some rest on a property the design deliberately
   forecloses; their manifest text must say so explicitly, and a *pass* on such an entry is cause to
   investigate, not to celebrate.
4. **Find the sibling GATES, not just the disclosures.** The same host property may also appear in
   the converter's `unsupportedRuntimeCapabilities` map — a *gate* that removes tests from the run
   set rather than disclosing them. The map is guarded by its own converter test, so removing an
   entry is a converter change owing the converter's `go test ./...`. A migration that retires the
   disclosures and forgets the gate has retired half the class.
5. **Price the shape before adopting it.** A self-contained single-file test host is *a publish
   rather than a build*, per package, at a substantially larger artifact size. Across a full roster
   that is a different campaign from a build-per-row. **Measure it**; do not assume it absorbs.

### 9.3 The consequences, in order

1. **The affected rows re-derive.** Previously-disclosed verdicts become matched verdicts; the row's
   matched and disclosed columns both move.
2. **The manifest loses its retired entries**, and at zero remaining entries **the file itself is
   removed** — the established precedent, because the roster's arithmetic moves when it goes.
3. **The roster header re-derives** in both components. It is recomputed from its own table, never
   typed.
4. **A row whose GO side is host-conditional cannot be banked from one measurement.** Where a
   manifest's own `hostConditional` note says the Go baseline varies per host and per run, the
   evidence bar is repeated measurement on more than one machine.
5. **Every affected package's proof page regenerates**, and the README validation badge recomposes
   from it. The badge reads `src/version.props` and the current proof directory, so the seeding rules
   in the reconvert ritual apply to any regeneration that follows.
6. **The disclosure class's prose narrows but need not disappear** — a permanent entry keeps a class
   alive, and the class text should already carry the "not every entry retires this way" paragraph
   that says so.
7. **The roadmap's retirement-path section is rewritten from a proposal into a record**, present-tense
   for visitors; the history goes to NEWS and the board.

---

## 10. The deployment-shape review

The migration's closing stage, and a ruling rather than a gate. It decides the corpus's published trim
mode, the performance suite's AOT profile, and the test host's publish shape.

**It may not be taken without its evidence**, and the evidence is a checklist of measurements and
named absences, not an impression. The generic checklist:

| Evidence | Bar |
|:--|:--|
| Trim diagnostics under the new SDK, per file | **re-measured**; a risen count is information, not a blocker |
| Trim correctness against the **population** | behavioral Output phase + a roster sample, zero divergence; any divergence names the site |
| The floor, re-measured on the migration's own toolchain | size / startup / working set against a **same-session** control |
| The AOT attribution | §7's prediction answered either way |
| Each retired disclosure entry measured **passing** | the arithmetic must break **loudly** — a silent pass means the entries are not being run |
| Each gate-map entry removed, with its converter guard green | `go test ./...` |
| Each **permanent** entry still failing, with its manifest text still saying why | a pass here is an investigation |
| Host-conditional entries measured repeatedly, on more than one machine | one reading is insufficient by declaration |
| Publish-shape cost, per package and in aggregate | feeds the roster-campaign cost model |
| Publication consequence of the TFM move | §11 |
| Named regressions re-measured | reported whether or not they reproduce |

---

## 11. The publication consequence

**A .NET migration changes the published packages' target framework while changing no Go release** —
so the published version number, which encodes the Go release plus a build counter, carries **no
signal that the runtime moved**. A consumer on the previous runtime restoring the next published
build would receive a compile asset it cannot load.

Three shapes exist and the choice is a ruling, not a default:

- **Do not publish across the migration.** Cleanest when the preceding release is already framed as
  the reference point on the outgoing runtime, and the next Go corpus migration publishes on the new
  one. The migration still **rehearses** the release ritual as a gate.
- **Multi-target.** NuGet's asset selection then answers for both runtimes. Correct, and it multiplies
  the build cost of every converted package — price it, do not assume it.
- **Publish and document the break.** Cheapest and worst; a version scheme that cannot express the
  break is not made to express it by a release note.

Whatever is chosen, the ordering assertion still applies: published versions must remain **monotonic**
across the whole ladder, and monotonicity is **verified with a scripted comparison before the first
publish**, never believed. A non-monotonic sequence on a public feed is not correctable.

---

## 12. Gate accounting — what a .NET migration owes

Stated as an accounting so a skipped gate is distinguishable from a forgotten one — the repository's
habit of writing *"not run; accounting stated"*.

| Gate | Owed? |
|:--|:--|
| the Stage-0 two-lane provisioning gate (§2(5)) | **yes**, once **per fleet machine**, and re-owed on any box that later changes its default hive. It is the only gate that is not repository-wide, and the only one a machine can fail alone |
| `go2cs-stdlib.slnx`, every buildable `$(GoTargetOS)` flavor | **yes**, at the SDK stage and again at the TFM stage |
| `go2cs.slnx` | **yes** at both, and again after any golib API change |
| full behavioral suite (four phases) | **yes** at both |
| `GolibTests` | **yes** at both |
| `check-no-regression.ps1` | **no** for the SDK and TFM properties themselves — CNR measures converter emission and the converter is a Go binary. **Yes** the moment a converter source **or embedded asset** moves (§5.2) |
| converter `go test ./...` | same accounting, plus **yes** whenever the `unsupportedRuntimeCapabilities` map or a csproj template changes |
| full validated-roster sweep | **yes once**, at the migration's closing gate, coordinator-owned and backgrounded |
| full performance suite with AOT | **yes once**, solo, on the perf-canon host — this is the migration whose whole point includes it |
| release-ritual dry run | **yes**, as the closing rehearsal |

Budget every one of these from CLAUDE.md's measured budget table, **from the top of each range**, and
**re-measure and update the table** when a row is exceeded on healthy work — a stale baseline is what
makes a healthy run look hung.

---

## Sources

- `CLAUDE.md` — the measured budget table; the false-green route catalogue; the `GoTargetOS` /
  incremental-build hazard; the golib-API-change rule for `go2cs.slnx`
- [`Roadmap.md`](Roadmap.md) — *"Declared host limits and their retirement path"*, the
  standing answer §9 generalizes
- [`ValidatedTestPackages.md`](ValidatedTestPackages.md) — the disclosure classes, their
  signature-pinning rule, and the self-retiring text §9.1 relies on
- [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) — the ruling frame that schedules .NET migrations
  relative to Go corpus migrations, and the scope additions that place the trim-safety and
  deployment-shape work here
- [`PLAN-bflat-perf-exploration.md`](PLAN-bflat-perf-exploration.md) — the concluded floor
  exploration whose findings §8 generalizes, including the trim-diagnostic surface by file
- [`GoCorpusMigration.md`](GoCorpusMigration.md) — the companion runbook
- Source read directly: `src/Directory.Build.props`; `src/go2cs/embeddedTemplates.go`;
  `src/go2cs/csproj-template.xml`, `src/go2cs/test-csproj-template.xml`;
  `src/go2cs/csprojMetadata_test.go`; `src/go2cs/toolchainResolution.go`;
  `src/go2cs/directiveOperations.go`; `src/go2cs/testConversion.go`
  (`unsupportedRuntimeCapabilities`); `src/push-nuget.ps1`; `src/run-validated-sweep.ps1`;
  `src/tests/Behavioral/BehavioralRunner/Program.cs`,
  `src/tests/Behavioral/BehavioralTests/BehavioralTestBase.cs`,
  `src/tests/Performance/PerformanceRunner/Program.cs` (the `*.go` rebuild predicates)
