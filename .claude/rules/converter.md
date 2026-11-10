---
paths:
  - "src/go2cs/**"
---

# Converter internals and the stale-binary routes

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 181-189, 225-771, 772-831, 832-997, 998-1081.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

<!-- PHASE 2 DISTILLATION, 2026-09-12. The visible half is now imperative rules led by their TELL;
     every date, SHA, file:line, measured number, package name and named incident from the Phase-1
     text sits in a comment beside the rule it justifies. Nothing was deleted, only relocated.
     Cross-file citations kept intact: false-green routes #1-#5 are DEFINED here and cited by number
     from .claude/skills/gate-forensics and elsewhere; routes #6-#8 are defined there and only cited
     here. The launch traps the Phase-1 text numbered informally ("a sixth" ... "a thirteenth") are
     renamed by subject under "Census and launch traps" — nothing cites them by number.
     BATCH19 MERGE, 2026-09-12: the items routed to this file from claude/coord-doctrine-batch19
     (commits 24bfc8304 / c5e17217b / e9e56b657; CLAUDE.md@44f858717 anchors 303, 487, 789, 816, 852,
     914, 985) were merged — most as further evidence inside the comment of the rule they re-instantiate,
     the rest as amendments to a visible rule, and ONE (the mtime assembly count) SUPERSEDING the remedy
     that stood before it.
     CONTEXT DIET RELOCATION, 2026-09-12. Two sections LEFT this file for
     .claude/skills/gate-forensics/SKILL.md: "Reading a `-tests` result" (with its subsections "The shape
     of an empty set", "Host-death signatures", "Comparison-record hygiene" and "Instruments around the
     pipeline" -- 180 effective lines) and "Census and launch traps" (84 effective lines), measured with the
     same strip-comments predicate TestContextBudget uses; this file went 588 -> 325. Reason: this
     file is PATH-SCOPED (`paths: src/go2cs/**`), so it loads IN FULL whenever anyone -- a subagent
     included -- reads any file under src/go2cs/, and it was the largest such load in the repo; a skill's
     body loads only when the skill is invoked, and both sections are about READING A GATE RESULT rather
     than about converter internals, which is exactly gate-forensics' advertised scope. Every date, SHA,
     file:line, measured number, error code and named incident from both sections travelled with them, in
     the comment beside the rule it justifies; where an incoming rule duplicated one already standing there
     it was folded into that rule's comment and the sharper visible wording kept. What STAYS here: the flag
     reference, the GOROOT-spelling rule, "Invoking the converter without producing a false reading", the
     test-harness mechanics and the false-green routes -- routes #1-#5 are DEFINED here and cited by number
     from gate-forensics, which defines #6-#8, and that split is deliberate and unchanged. A one-line
     pointer sits where the first section was. -->

Entry `src/go2cs/main.go`; stdlib driver `stdLibConverter.go` (dependency graph + topological
`sortedQueue`). `visit*.go` walk AST nodes → C# declarations/statements (`visitFuncDecl.go`,
`visitRangeStmt.go`, `visitDeferStmt.go`, `visitSelectStmt.go`); `conv*.go` convert expressions/types
(`convCallExpr.go`, `convSliceExpr.go`, `convStarExpr.go`). Analysis passes:
`escapeAnalysisOperations.go`, `variableAnalysisOperations.go` (shadowing),
`nameCollisionAnalysisOperations.go`, `constraintOperations.go` (generics), `importOperations.go`.
Full taxonomy: [`docs/Architecture.md`](docs/Architecture.md).

## Flag reference

`go2cs [options] <input_dir> [output_dir]`; `main.go` is authoritative.

- `-stdlib` — convert the Go stdlib; `-stdlib fmt strings io` converts only those packages.
- `-recurse` — convert a module + its third-party deps against the pre-converted stdlib via local
  `$(go2csPath)` project refs. A second positional output root isolates the generated `src\` app and
  `pkg\` dep trees from the runtime root (converted packages then reference one another relatively);
  without it, output defaults to `-go2cspath`. `-recurse=module` converts only the input module's own
  packages, still referencing third-party ones into `pkg\<import-path>` (issue #32). `-recurse=nuget`
  emits NuGet PackageReferences instead (`go.<pkg>`/`go.lib`/`go.gen`, versioned `$(GoStdLibVersion)`,
  floating default in a generated output-root `Directory.Build.props`) so an app restores from
  nuget.org with no `deploy-core` staging; the app's own packages stay relative refs. Values compose:
  `-recurse=module,nuget`. A local-refs recurse PINS `$(go2csPath)` in that generated
  `Directory.Build.props` (condition-guarded; relative `$(MSBuildThisFileDirectory)` when the roots
  coincide, absolute otherwise) — without the pin an isolated output root falls back to the template's
  `$(USERPROFILE)/go2cs/` and no stdlib reference resolves (issue #36).
- `-tests` — also convert the eligible `_test.go` suite and emit a runnable test-host project. Default
  off; **mutually exclusive with `-recurse`** (`log.Fatal`). Forces `-comments` on, resolves the output
  path absolute, self-locates `$(go2csPath)` by walking the output dir up to the first root holding
  `core/golib` — so `go2cs -tests -test-action all <goroot-pkg-dir> <converted-pkg-dir>` needs no flags
  or env from a clone.
- `-test-action convert|build|run|compare|all` (default `convert`) — `convert`/`all` convert-and-hook
  (production sources then tests); `build`/`run`/`compare` act on EXISTING digest-validated artifacts
  WITHOUT reconverting; `compare`/`all` diff the host's terminal results against `go test -json -count=1`.
- `-test-timeout <dur>` — package deadline for build/run/compare; Go duration syntax, default `2m`,
  must be > 0; on run/compare handed to BOTH sides (`go test -timeout` and the host's own `-timeout`)
  so they agree, the child killed a minute later as a safety net.
- `-convert-timeout <dur>` — the `-stdlib` driver's cap on ONE package's conversion; default `10m`,
  must be > 0 (`log.Fatal` otherwise). A package that hits it STOPS the run, exit 1 (since 2026-11-05).
- `-go2cspath <dir>` — runtime/stdlib root and default output root (default `~/go2cs`; env
  `GO2CSPATH`); also the root each imported package's `package_info.cs` is read from to mint the
  emitted `<ImportedTypeAliases>` block. A single-package or `-tests` conversion whose root is not a
  go2cs root (no `core\golib\golib.csproj`) SELF-LOCATES up its OUTPUT path's ancestors; when none is
  found, a loud once-per-run stderr warning names the resolved path and the consequence (NOT fatal —
  standalone conversion is legitimate). `-recurse` warns but never self-locates; `-recurse=nuget` and
  `-stdlib` do neither. An explicit *working* root always wins, and every harness passes an EXPLICIT
  `-go2cspath <repo>\src` computed from its own location.
- `-platforms os/arch` — the ONE target emitted for (default: the host), or a comma-separated LIST
  (`-platforms windows/amd64,linux/amd64,darwin/amd64,linux/arm64`)
  which **with `-stdlib`** performs the multi-platform EMISSION (`platformEmit.go`): convert per target
  into a seeded staging root (`-platform-stage <dir>`), then MERGE into the `-go2cspath` corpus as
  layout L3 — shared files flat, platform-varying in a key folder (`<goos>/`, `<arch>/`, `<goos>-<arch>/`),
  hand-owns routed to their principal's platform set. The list must equal `platformUniverse`
  (`platformLattice.go`). **A list without `-stdlib` is REJECTED**, never narrowed to the first target.
- `-platform-census <dir>` — READ-ONLY instrument over the same staging (`-stdlib` + ≥2 targets):
  converts per target into `<dir>\<goos>-<goarch>\src` (each root SEEDED from `-go2cspath`, wiped and
  re-seeded per run so "never convert twice into one root" is mechanical), classifies every artifact
  (shared / variant / partial / exclusive) into `<dir>\platform-manifest.json`, and writes **nothing**
  into the corpus. Emitted-vs-seeded is a sentinel MTIME, not content, since the control target is
  *supposed* to reproduce the seed byte for byte. Its per-target marker gate (hand-owns the seed held,
  plus any emitted as plain `.cs` — must be zero) keeps a failed seeding from reading as a finding.
- `-license <SPDX expression>` — overrides the emitted NuGet `PackageLicenseExpression` for every
  project; never relicenses source. Default packing: a library's local `LICENSE`, a stdlib package's
  `src/core/LICENSE` by relative path, a `-recurse` dependency's own module license copied verbatim to
  the converted module root once per module (`licensing.go`).
- `-provenance` — a deterministic two-line notice, `// Converted from Go source: "<path>" in module
  <path>@<version>.` (GOROOT- or own-module-relative) then `// Modified from the original by conversion to C#
  with go2cs <release>.`, after the upstream notices and before the package doc comment; default OFF, no
  timestamp, not in GoInputDigest. nugetgo-pack turns it on for an Apache-2.0 `-LicenseSpdx`. Leading copyright/license notices are emitted even WITHOUT `-comments`.
  The converter is AGPL-3.0-only with the output exception in `src/go2cs/LICENSE-EXCEPTION`; every
  converter `.go` header carries the section 7 notice (`TestLicensingConverterHeaders`). See
  `LICENSING.md`.
- `-goroot` / `-gopath`, `-indent 4`, `-var` (on), `-uco` (channel operators, on), `-comments`, `-cgo`,
  `-tree`, `-csproj <tmpl>`, `-debug`, `-allow-stale-converter` (route #1). Single project/file:
  `go2cs package_dir` or `go2cs example.go [out.cs]`.
- **Always pass `-comments` on stdlib runs.** It defaults off and the converted C# is a derivative
  work: the per-file `// Copyright … The Go Authors … BSD-style license` header MUST survive and the
  doc-comments are what make the output readable. **Do not flip the default** — behavioral goldens were
  captured without comments.

<!-- Phase-1 provenance for the flag block:
     * `-recurse` isolated-output-root pin and issue #36; `-recurse=module` / issue #32 as written.
     * `-platforms` multi-platform emission: ~560 s for three targets (measured r51b).
     * `-platform-census`: increment 1, landed 2026-08-08; the "never convert twice into one root"
       rule it mechanises is r41's.
     * `-go2cspath` as the `<ImportedTypeAliases>` source: a stale/missing root used to emit a silently
       EMPTY block — no warning, exit 0 — and the OUTPUT varied with the shell's ambient `GO2CSPATH`
       (found 2026-08-06). The harnesses now passing an explicit root are `check-no-regression.ps1`,
       `BehavioralRunner`, `BehavioralTestBase`, `PerformanceRunner`, `run-validated-sweep.ps1`, "so no
       gate's verdict can move with the ambient variable again".
     * `-license` / `-provenance` / `licensing.go`: landed 2026-09-11.
     * `-test-timeout` both-sides threading: "Before that threading each side fell back to its OWN
       10-minute default, so no value of the flag could let a slower-than-Go suite finish — hash/maphash
       self-terminated at exactly 600 s under `-test-timeout 40m` and reported its still-running
       TestSmhasherAvalanche as an empty verdict that reads like a real failure." A suite legitimately
       slower than 10 min needs its own value (maphash: `-test-timeout 30m`, ~15 min in C# vs 7.6 s in
       Go — a performance gap, not a correctness one).
     * `-convert-timeout` was hard-coded at 10m until 2026-09-02, when concurrent lane load on the i7
       class pushed one package past it mid two-seeded A/B, which would have banked a whole package as a
       spurious emission difference. The fired message names the package, the elapsed budget and the flag.
     * `-comments`: "(Behavioral-test goldens were captured *without* comments, so don't flip the
       default — pass the flag on stdlib `-stdlib` runs.)" -->

## Spell `GOROOT` exactly as `go env GOROOT` prints it — argument AND environment

**Tell: `std.<pkg>.csproj` / `std.<pkg>.tests.csproj` appear beside the committed `<pkg>.csproj`, or a
csproj carries `RootNamespace=go.std`.** A forward-slash spelling — which `go` itself accepts, and which
a Bash-side lane naturally types — fails `getProjectName`'s
`strings.HasPrefix(importPath, options.goRoot)` (`importOperations.go:48`), so the walk-up branch finds
`$GOROOT/src/go.mod`, which declares `module std`, and the whole emission lands in `namespace go.std.*`
while the run **EXITS REPORTING SUCCESS**. The damage surfaces in CONSUMER packages as
`CS0117: 'utf8_package' does not contain a definition for …` or
`CS0246: 'sparseFileWriterжWriter' could not be found` — reading exactly like a converter regression
that dropped public members, or a witness/generator regression in the wrong package. **It reproduces
identically on a baseline converter**, so "it fails at master too" is NOT evidence the tree is at fault
when the environment is the variable. `run-validated-sweep.ps1` and the `-tests` pipeline both read
`GOROOT` from the environment; single-quote it in Bash so the backslashes survive. **A path the
converter half-recognizes is worse than one it rejects** — native paths convert clean first time. The
LOADER side is fixed and guarded (`isPathUnder`, `checkGoRootSpelling`); do not re-diagnose it. The
project-IDENTITY residual is G's.

<!-- Derivation, verbatim from Phase 1:

     ⚠ **Pass GOROOT EXACTLY as `go env GOROOT` spells it — a forward-slash path silently misroutes the
     whole emission into `namespace go.std.*`** (found 2026-08-24). `getProjectName`
     (`importOperations.go:48`) decides the namespace with `strings.HasPrefix(importPath, options.goRoot)`;
     on Windows a `C:/Users/.../go1.23.1/src/unicode/utf8` argument fails that prefix test against the
     backslash form `go env` returns, so the walk-up branch runs instead and finds **`$GOROOT/src/go.mod`,
     which declares `module std`**. Every file is then emitted into `go.std.unicode` rather than
     `go.unicode`, the conversion **exits reporting success**, and the damage surfaces as
     `error CS0117: 'utf8_package' does not contain a definition for …` in the CONSUMER packages
     (`strings`, `syscall/windows`) — pointing away from the cause and reading exactly like a converter
     regression that dropped public members. Same family as the `-go2cspath` empty-`<ImportedTypeAliases>`
     trap below: **a path the converter half-recognizes is worse than one it rejects.** Native paths convert
     clean first time. (The durable fix LANDED 2026-08-28, `433e9e4e0`: `isPathUnder` +
     `checkGoRootSpelling` + 3 guard tests — the loader-side comparison is path-normalized now. ⚠ The
     PROJECT-IDENTITY side has a measured open residual: `std.<pkg>`-named csproj artifacts dated AFTER
     the fix, with all sources namespace-correct — 2 csproj carrying `RootNamespace=go.std` while 13
     `.cs` declare `namespace go;` — from a run that exited reporting success. Mechanism unestablished,
     G owns the root-cause; do not re-diagnose the loader side, it is fixed and guarded.)

     ⚠ **It bites through the ENVIRONMENT just as readily as through an argument, and the Bash tool is
     where that happens** (paid again 2026-08-26). `run-validated-sweep.ps1` and the `-tests` pipeline
     read `GOROOT` from the environment, so `export GOROOT="C:/Users/.../sdk/go1.23.12"` — the
     forward-slash spelling a Bash-side lane naturally types, and which `go` itself accepts — routes the
     whole emission into `namespace go.std.*` exactly as the argument form does. **The visible tell is the
     project NAME**: the run writes `std.<pkg>.csproj` / `std.<pkg>.tests.csproj` beside the committed
     `<pkg>.csproj`, and the failure surfaces as CS0246 on a generated adapter type in a CONSUMER file
     (`writer.cs: 'sparseFileWriterжWriter' could not be found`) — which reads like a witness/generator
     regression and invites a hunt in the wrong package. It also survives an A/B: running the SAME sweep on
     a baseline converter reproduces it identically, so "it fails at master too" is NOT evidence the tree is
     at fault when the environment is the variable. Check for `std.*` artifacts before believing any such
     diagnosis, and set `GOROOT` from `go env GOROOT` verbatim (single-quoted in Bash so the backslashes
     survive). -->

## Which GOROOT the LOADER reads, and the corpus pin that only guards a seeded root

**Operator side: assert `go version` AND `go env GOROOT` against the pin from a NO-MODULE directory, in
the RUN'S OWN environment, before the run; read the emission's own path lines after it.** Both halves are
load-bearing. The no-module directory closes the `GOTOOLCHAIN` re-exec: a `go.mod` requiring a newer
release makes the toolchain switch and **rewrite `GOROOT` in the process**, so a `GOROOT=X` you exported
and asserted against `X/VERSION` can be answered by an entirely different tree. The run's own environment
closes the ambient install: a pin asserted in another shell says nothing about this one. **An assertion in
the calling command is necessary and NOT sufficient** — the sufficient read is the converter's own
provenance line, below.

**Converter side.** Every conversion and the census print the root, its `VERSION` read **in-process**, and
the loader-directory's own `go env GOROOT` when the two disagree (`printToolchainProvenance`,
`toolchainResolution.go:391`). **`-goroot` is never read by the LOADER** — `go/packages` shells out to
`go list`, which inherits `os.Environ()`, while the flag lands on `build.Default.GOROOT`, which steers
`go/build` only. It reaches the loader *solely* because `main.go:443` now exports the resolved root when
the environment carries none; **a flag/environment disagreement REFUSES**, naming both
(`loaderGoRootDecision`, `main.go:142`), rather than silently switching which tree a working invocation
reads. A spelling-, case- or symlink-only difference is not a disagreement (`sameGoRoot`) — do not
re-diagnose that. The loader's env also carries `GOTOOLCHAIN=local` (`conversionDriver.go:121`).

**Seed `version.props` beside `core` into a hand-seeded temp root, or the pin is inert.** Floor rule 2
says seed the temp root from `src/core`, and `version.props` is a **sibling** of `core` (`src/version.props`)
— so a root seeded by that rule alone carries `core/golib/golib.csproj` and no pin, and
`checkCorpusToolchainPin` then passed unconditionally on an empty read. **Every hand-seeded `-stdlib`
reconvert has therefore run with the toolchain pin inert.** `seedCensusRoot` already copies it
(`platformCensus.go:481`); the hand workflow has to match. `corpusPinnedReleaseOrError` now refuses by
name — scoped to a root that *looks* like a corpus (`isGo2CSRoot`, i.e. `core/golib/golib.csproj` present,
`testConversion.go:531`), because a bare unseeded target is a corpus's FIRST conversion, not a fault.

<!-- Derivation, 2026-09-13. Wording ruled by COORD at mailbox `9bdca5025` (operator and converter sides),
     landing with C2's census/-goroot seat `7c1d8832f` (accepted at `c5580c113a` §2 with its narrowing).

     ⚠ THE CONVERTER-SIDE CLAUSE IS ONE WORD NARROWER THAN THE RULING'S LITERAL WORDING, DELIBERATELY.
     `9bdca5025` set it as "`-goroot` is not read by the loader in any mode". That was exactly true of the
     PRE-FIX tree and is the defect statement; it is no longer true unqualified of the tree the line ships
     with, because `main.go:443` exports the resolved root in the environment-unset case, so the flag DOES
     steer the loader there. Written as "never read by the LOADER … it reaches the loader solely because
     main.go exports it" — the durable fact (go/packages reads os.Environ(), build.Default.GOROOT steers
     go/build only) plus the one mechanism that bridges it. Flagged to COORD in the announce, not slipped.

     THE ROOT CAUSE, measured on a 24-line probe mirroring conversionDriver.go's config shape (C2, this
     date; the fleet's diagnosis before it was census-local, which was true-and-not-the-cause):
         flag (build.Default.GOROOT) : <toolchain>/go1.23.12
         env GOROOT                  : (unset)
         loader read                 : /usr/local/go1.24.7/src/errors/errors.go   <- the AMBIENT root
     So the flag was inert for source SELECTION in EVERY mode -- census, -stdlib and -tests alike -- and a
     control that once read "plain -stdlib honours it" can only have agreed because flag and environment
     matched on that box: the flag echoing itself. `platformCensus.go` copies `options` wholesale, so
     `goRoot` DID travel into the census; it just had no effect anywhere.

     THE GOTOOLCHAIN RE-EXEC, same probe, and it is NOT a second undiagnosed mechanism -- it is named at
     `projectFileWriter.go:39-42` and the remedy shipped there:
         the same 1.23.12 binary, asked its own version:  GOTOOLCHAIN=auto -> go1.24.7
                                                          GOTOOLCHAIN=local -> go1.23.12
         GOROOT=1.23.12 exported AND its bin first on PATH: `go: downloading go1.24.7`, and
             `env GOROOT` then read <toolchain>/go1.24.7 -- THE VALUE EXPORTED WAS REWRITTEN.
         GOROOT exported, bin NOT on PATH: `compile: version "go1.23.12" does not match go tool version
             "go1.24.7"` -- refuses LOUDLY, which is the good case: a GOROOT/binary mismatch names both.
     The probe's own switch was triggered by `go mod tidy` writing `go 1.24.0` + `toolchain go1.24.7` into
     the probe module; the converter's loads are of stdlib packages inside a GOROOT `src` dir, which carry
     no such requirement, so the switch may not fire there at all. Recorded as the mechanism, not as a
     claim about the converter's own loads.

     THE INERT PIN: `isGo2CSRoot` keys on `core/golib/golib.csproj`, which a floor-rule-2 seeding
     CREATES, while `version.props` sits beside `core` and is NOT copied by that rule -- so the seeded
     root is precisely the shape that looks like a corpus and carries no pin. Before the refusal,
     `corpusPinnedRelease` returned ("", nil) for it and the pin check was a silent no-op. The narrowing
     to isGo2CSRoot came from `toolchainResolution_test.go`'s own fixture, which documents a bare root as
     "the normal state of a first -stdlib conversion, not a fault" -- COORD accepted the narrowing rather
     than the unconditional refusal it had ruled. Ten tests at the seat, including a pre-fix control that
     reconstructs the silent no-op through the old function and shows it PASSING. -->
- **READ A BOX'S BARE GO WITH `GOTOOLCHAIN=local go version` FROM A DIRECTORY WITH NO MODULE — a directory
  named for one Go release can hold another, and a module-context reading will not say so.** <!-- ⚠
     2026-09-15, C1; the fleet was ordered to state it once. `GOTOOLCHAIN=auto` switches UP inside a module
     to satisfy its `go` directive, and `go version` then answers for the SWITCHED toolchain: C1's
     `/usr/local/go` was go1.24.7 while every reading taken inside the converter module said 1.24.13.
     Every instrument pins `GOROOT` explicitly rather than inheriting one. -->
- **A GUARD THAT READS GO'S OWN SOURCE IS MEASURED AT THE CORPUS PIN, NEVER AT THE CONVERTER'S BUILD PIN.**
  <!-- ⚠ 2026-09-15, C1. `TestLinknamePushRegistryMatchesGoSource` run under a go1.25.1 GOROOT reported
     `unique.runtime_registerUniqueMapCleanup` as undeclared — it exists at 1.24.13 and is absent at
     1.25.1. A GOROOT-axis FALSE REGRESSION, which is the two-pin window met from the other side: the
     guard was right, the tree was right, and the toolchain the guard happened to run under was the whole
     finding. -->
- **WHICH `GOEXPERIMENT` ARM IS ON LIVES IN `internal/buildcfg`'s BASELINE, NOT IN THE ARM FILES.** <!-- ⚠
     2026-09-13, C1 `0038b75b8` s1. `exp_<name>_{off,on}.go` say what each arm DEFINES, never which is on.
     At 1.24.13 `SwissMap: true` sits in `ParseGOEXPERIMENT`'s baseline beside `RegabiWrappers` (which is
     arch-gated; SwissMap, checked, is not), so `!goexperiment.swissmap` test files are NOT COMPILED and
     `TestMapBuckets` deletes itself at the hop. Read BOTH exact pins — the real 1.24.13, fetched, not a
     nearby local 1.24.x, because identical file SETS say nothing about CONTENT — and note that no harness
     path pins `GOEXPERIMENT` at all (grepped), so the baseline decides. -->

## Invoking the converter without producing a false reading

- **Rebuilt converter → run `convert` THEN `build`.** Otherwise `-test-action build` exits 1 with ZERO
  compile errors (tail: `test manifest is stale: input digest changed`) — a false red reading exactly
  like the route-#7 gate. **The EMPTY error-code histogram is the tell: no `error (CS|MSB|NETSDK)[0-9]+`
  line means it is not a build failure.** Inversely, N errors ALL I/O-coded
  (MSB3491/MSB3027/MSB3021/CS0016/CS0041/CS8104, "not enough space on the disk") are ONE environmental
  failure wearing N codes. **And an error CODE is not a remedy class** — one code spans two remedies while
  other codes are one root wearing another (a wrongly-bound package alias fails METHOD resolution, so
  CS1929/CS0117 are alias consequences). The histogram SIZES a wall; the error TEXT — and the DECLARING
  file it names, not the file it appears in — CLASSIFIES it.
- **`-test-action all`/`compare` RE-CONVERTS every non-marked corpus file before building**, wiping an
  instrumentation edit. Sequence `convert` → edit → `compare`; `grep -c MARKER <file>` after the run is
  the tell. A probe whose readings are CONSTANT across the population is its own false-empty.
- **Pass `-test-timeout 10m` explicitly on any hand-invoked row** — the `2m` default is 5x smaller than
  the sweep's, so a hand run fails where the sweep passes on nothing but which default applied.
- **`-convert-timeout` is a hang net, not a performance assumption** — a killed-but-healthy conversion
  is reported as a FAILED package (log, summary, `failed_packages.txt`) and reads exactly like a
  converter defect. Raise it on the command line, and pass the SAME value to both binaries of an A/B.
- **Pass the output dir as the SECOND POSITIONAL for any single-package conversion** — it emits BESIDE
  ITS INPUT and `-go2cspath` does not redirect it; this has written `.cs` into a GOROOT (loud) and
  untracked copies of a `src/core` package into the repo root (silent, for hours). A filtered
  `git status` answers "did my change land"; only an UNFILTERED `git status --porcelain`, read whole,
  answers "is the tree clean".
- **A byte-identity green needs its negative control** (blank line → red → byte-identical restore): a
  gate diffing a seeded copy against its own source cannot go red, and an "emission" whose mtimes
  PREDATE the seed copy is not an emission.
- **A marker-PRESERVATION measurement is the one shape that runs IN PLACE** — `writePackageInfoFile`
  preserves a hand-added line by reading the EXISTING file at the output path, so a fresh scratch output
  has nothing to preserve and the diff is VACUOUS. Run it in a worktree against git HEAD. Where two
  ten-second forms could disagree, run BOTH: the disagreement is the finding.
- A scratch OUTPUT root injects ABSOLUTE paths into `GoPositionMap`'s first argument exactly as a
  scratch input does — **not postable without redaction**; read the delta by counting line KINDS. A
  silent exit-0 zero-diff run is proven a measurement by emitted mtimes, the pipeline being silent on
  success.
- **Anchor a pathspec with `:/` or run from the repo root and say which** — the same
  `git diff --name-only <a>..<b> -- '*package_info.cs'` returns paths from the root and NOTHING from
  `src/go2cs`, and the empty read passes for "no metadata debt". **In a git pathspec `*` MATCHES `/`**,
  unlike a shell glob: `src/core/runtime/*.cs` swallows `windows/proc.cs`, so a per-flavour census
  returns FOUR IDENTICAL numbers — spell it `:(glob)`, and read arms that CANNOT differ as the census
  announcing that it does not discriminate.
- **In a multi-target comparison neither "differs" NOR "identical" means anything until you know which
  side was WRITTEN** — a single-target conversion re-emits only its own per-GOOS files, so per-PATH
  diffs report fresh-vs-seeded pairs, and a host-default reconvert reports ZERO diff on `linux/` files
  another lane measured as carrying missing forced-init hooks. Classify by write-evidence PER TARGET;
  measure an L3 package with the three-target emission.
- **A single-target number is not "the" population** — report union AND intersection; the GAP between
  them is the finding.

<!-- Derivations, verbatim from Phase 1:

     ⚠ **A REBUILT CONVERTER makes `-test-action build` exit 1 with ZERO compile errors** (measured
     2026-09-06) — a false red that reads exactly like the route-#7 gate it was being run as. The tail
     states it outright (`test manifest is stale: input digest changed (run -tests -test-action convert)`)
     and **the error-code histogram is EMPTY, which is the tell: a build failure carrying no
     `error (CS|MSB|NETSDK)[0-9]+` line is not a build failure.** Since `build`/`run`/`compare` act on
     existing digest-validated artifacts, any gate that rebuilds the converter first must run **convert
     THEN build**; both `reflect` and `internal/reflectlite` went green on the corrected invocation.
     The general form, from the same day: **a build reporting N errors whose codes are ALL I/O
     (MSB3491/MSB3027/MSB3021/CS0016/CS0041/CS8104, every message "not enough space on the disk") is ONE
     environmental failure wearing N codes, not N defects** — read the code histogram before believing a
     count.

     ⚠ **An instrumented `-tests` probe reads ZERO because `-test-action all`/`compare` RE-CONVERTS
     every non-marked corpus file before building** (measured 2026-09-03) — the marker was wiped
     between the edit and the binary, the same mechanism that silently reverted a hand-own prototype
     hours earlier. Instrument in the sequence `convert` → **edit** → `compare`, and `grep -c MARKER
     <file>` immediately AFTER the run is the one-command tell. (The probe's own readings VARYING
     across the population were what made its non-zero answer trustworthy — a probe whose output is
     constant is its own false-empty.)

     ⚠ **The 2m default is FIVE TIMES SMALLER than the sweep's, so a hand-invoked `-tests` run fails
     where `run-validated-sweep.ps1` passes — on nothing but which default applied** (measured
     2026-08-24: `bytes` reported `Go="pass" C#=""` on 38 tests with ZERO reported, which is the exact
     signature the orphaned-`dotnet run` file lock produces and reads as total conversion failure; the
     sweep's `-TestTimeout` default is `10m`, and at that value the same tree validated 82/82, exit 0).

     ⚠ **Single-package mode emits BESIDE ITS INPUT — `-go2cspath` does NOT redirect its output**
     (measured 2026-08-31: `go2cs -go2cspath <tmp>\src <GOROOT>\src\internal\abi` wrote seventeen
     artifacts into GOROOT and nothing into the temp root, and the byte-identity gate then diffed
     the seeded copy against its own source — IDENTICAL, vacuously, with oracle contamination on
     top). Pass the output dir as the SECOND POSITIONAL for any single-package emission you intend
     to diff. Two tells, both cheap: an "emission" whose mtimes predate the seed's copy is not an
     emission; and a byte-identity green is only believable after its negative control (inject one
     blank line → the gate must go red → the restore must be byte-identical) — a gate diffing a
     seeded copy against its own source is a gate that cannot go red.
     ⚠ **Paid twice more on 2026-09-02, and the REPO-ROOT form is the silent one.** A lane omitting the
     positional wrote 167 `.cs` into a GOROOT — loud, because GOROOT is not supposed to hold `.cs`; the
     same omission with the repo root as cwd left **41 untracked byte-identical copies of
     `src/core/strconv` in the REPOSITORY ROOT for eight hours**, invisible to every `| head`/`| grep`
     status check shaped around expected files. A FILTERED `git status` answers "did my change land";
     only an UNFILTERED `git status --porcelain`, read whole, answers "is the tree clean".

     ⚠ The same `git diff --name-only <a>..<b> -- '*package_info.cs'` returned four paths from the repo
     root and **NOTHING** from `src/go2cs`, and the empty read was briefly taken as "no metadata debt"
     (2026-09-06). **Anchor the pathspec with `:/`, or run from the root and say which.**

     ⚠ **The not-postable-emission rule has an OUTPUT door as well as an input one** (2026-09-04): a
     scratch OUTPUT root injects ABSOLUTE paths into `GoPositionMap`'s first argument exactly as a
     scratch input does, so a seeded-scratch run's delta is read by counting its line KINDS (twelve
     position-map first arguments, tables byte-identical, nothing else) rather than by "one file
     differs" — and none of it is postable without redaction (see the SECURITY convention). A silent
     exit-0 ZERO-diff run is proven a MEASUREMENT rather than a no-op by the emitted files' mtimes
     against the checkout time, since the pipeline's documented silence on success makes the artifacts
     the only evidence. ⚠ **A marker-PRESERVATION measurement is the one shape that runs IN PLACE**
     (input = output, the harnesses' own invocation): `writePackageInfoFile` preserves a hand-added
     line by reading the EXISTING file at the output path, so a fresh scratch output has no marker to
     preserve and the diff is VACUOUS — run it in a worktree and diff against git HEAD, while the
     output-positional rule above guards the other trap (a gate diffing a seeded copy against its own
     source). Where two ten-second forms could disagree, run BOTH: the disagreement would be the
     finding.

     ⚠ In any multi-target staging comparison, "differs" means NOTHING until you know which side was
     actually WRITTEN: a single-target conversion re-emits only its own target's per-GOOS files, so a
     per-PATH diff across staging roots reports fresh-vs-seeded pairs as differences (a confounded
     census nearly banked 60 false hits, 2026-09-01) — compare only paths BOTH conversions write, or
     classify by write-evidence first.
     ⚠ Its MIRROR, measured 2026-09-02: **IDENTICAL means nothing when the side was not WRITTEN
     either.** A windows-default single-target reconvert reported ZERO diff on an L3 package's
     `linux/` files — the very files another lane had measured, under a linux-target conversion, as
     carrying four missing forced-init hooks. Classify by write-evidence PER TARGET, and measure an L3
     package with the three-target `-platforms` emission rather than the host default.

     ⚠ **And a single-target number is not "the" population.** Re-derived from the generator's own output
     on all three targets, an unimplemented-stub population read **windows 232 / linux 256 / darwin 458 —
     union 510, intersection 214** (2026-09-06). A single-target run would have reported one of those
     three as the population and hidden a spread of nearly 300. **The gap between intersection and union
     is the finding**; a total conceals which members are platform-specific and which are universal.

     BATCH19, anchor 303 — the two readings behind "an error CODE is not a remedy class":

     ⚠ **AN ERROR CODE IS NOT A REMEDY CLASS — SORT FAILURES BY THE ERROR TEXT THAT NAMES THE SYMBOL**
     (2026-09-08, R echoed by G): on the H5 ladder CS0426 spanned TWO remedies (an alias shadow,
     re-QUALIFY; a `HashTrieMap` move, re-POINT), while CS1929 and CS0117 WERE alias consequences wearing
     other codes, because a wrongly-bound `sync_package` fails METHOD resolution rather than NAME
     resolution. R predicted the magnitude exactly (16 of 40) and named the wrong four projects by sorting
     on codes; G flagged the axis without predicting the members, and neither half alone was the answer.
     **The histogram sizes a wall; the error TEXT classifies it.**
     ⚠ **A PREDICTION THAT TREATED SIX ERRORS AS ONE CLASS MISSED BECAUSE THEY WERE TWO** (2026-09-08, R):
     a coordinator's 7 → 1 read 7 → 5 — the CS0111 pair cleared with the re-derive that named it, while
     four CS0759 in one hand-own are ORPHANED BY THE PACKAGE SPLIT (their defining declarations now live
     in the new `internal/sync`, 0–1 against 2–3 declarations per file), which NO re-derive of that file
     can reach. The lane declined a verbatim copy of the peer's file (3-wayed against a different base; it
     would drop the peer's bodies and its own seat) in favour of applying that seat's DELTA to the merged
     file, and held until the chain released the tree. **Score a routing by the ERROR TEXT's declaring
     file, not by the file the error appears in.**

     BATCH19, anchor 852 — the git-pathspec half of the pathspec rule above:

     ⚠ **IN A GIT PATHSPEC `*` MATCHES `/`, UNLIKE A SHELL GLOB** (2026-09-08, C1): `src/core/runtime/*.cs`
     swallowed `windows/proc.cs`, so a per-flavour census read 101/36 for windows, linux, darwin AND the
     flat-only control — four identical numbers from one query, and the only tell was that **a census
     whose arms CANNOT differ is announcing that it does not discriminate.** Fixed with the `:(glob)`
     magic pathspec and controlled 0 against 3. Two companions from the same record: a "displacing X
     leaves Y UNREACHED" sentence read as if the displacement achieved it for ALL of Y when it achieved
     it for four sites and 58 were dead by construction (two reasons in one clause — stated, not
     rewritten); and at go1.24.13 the `getcallerpc` family is ABSENT (0 sites), reborn as
     `internal/runtime/sys.GetCallerPC/SP/GetClosurePtr` at 208 sites in five packages, so a cut keyed on
     the old spelling is deleted by the hop. -->

Reading a `-tests` result, mass-empty signatures and the census/launch traps now live in the `gate-forensics` skill.
- **NEVER PASS `-tags` TO THE `-stdlib` RECONVERT, AND QUOTE THE PRINTED TAG SET IN THE RECORD** — the bare
  default resolves `purego,math_big_pure_go`, and it is applied ONLY when `-tags` is not passed. <!-- ⚠
     2026-09-13, i9 `7ae5355bb` / COORD `d2ad84bdb`. Five "deselections" in one step-2 run were all
     purego/generic variants whose build tags are IDENTICAL across the two releases — the tell of an
     emission run made without the default, whose asm-backed twins are throwing stubs that COMPILE. A gate
     is not read on a corpus whose selection differs from the committed one. -->
- **TWO COMPONENTS MUST NOT RESOLVE BUILD TAGS TWICE: THE CONVERTER IS THE AUTHORITY ON WHAT IT SELECTED**,
  so "deselected" is derived from the staging root, never re-derived by a second tag engine. <!-- ⚠
     2026-09-13, i9 `bb3a1a747` / COORD `9c07f494f`. The converter printed `Applying build tags:
     purego,math_big_pure_go (default…)` and selected the five files; the deletion pass then re-derived
     selection with tag logic of its own and DELETED them as DELETE-DESELECTED. The sound predicate:
     committed `.cs` present + principal present at the target + ABSENT from the emission, with those five
     as the standing control. COORD's tag-drift hypothesis was wrong on all three arms and was withdrawn —
     the corpus was right and the instrument was wrong. Arm-3 trap in the same cut: ABSENT from a
     directory that does not exist reads exactly like an absence, so print the directory's file count
     beside every absence verdict, and FIND the directory rather than composing its path. -->
- **CONSTRAINT TEXT IS NOT SELECTION: compare SELECTION at both GOROOTs under ONE tag set — the converter's
  PRINTED line for the run — never `//go:build` text and never a mirrored constant.** <!-- ⚠ 2026-09-13,
     C2 `5c47976ea`. Comparing constraint TEXT at the two GOROOTs read 4 of 6 rows WRONG, in the UNSAFE
     direction on three live `|| purego` files (the negated arch list grew) and refusing the one genuine
     deletion (`exp_aliastypeparams_off.go`, whose text is unchanged — the flip is in the experiment
     DEFAULTS). Selection is the constraint evaluated against an environment (tags, arch list, default
     GOEXPERIMENT set), and between two releases EVERY input moves. The selection-based gate predicted 7
     of 7. Blast radius recorded with it: 19 std files at 1.24.13 are selected ONLY under the default tag
     set, 7 of them in the corpus at their exact path. -->
- **FILE PRESENCE IS THE WRONG FRAME FOR A HOP THAT MOVES SELECTION** — a release can delete ZERO files and
  still stop COMPILING a whole family. <!-- ⚠ 2026-09-13, G `9e50ebe92`. The 1.23->1.24 hop deletes no
     negated-goexperiment file (19 present at both pins) yet four tags turn ON — `aliastypeparams`,
     `swissmap`, `spinbitmutex`, `synchashtriemap` — so the `!X` variants stop being selected: the whole
     `map_*_noswiss` family, `reflect/map_noswiss.go`, the `lock_*_tristate` pair and `sync/map.go` (the
     CLASSIC `sync.Map`) never reach the converter at 1.24.13, measured by `go list` at the default. The
     selected COUNT reads 14 before and 14 after with a different COMPOSITION — count-versus-set arriving
     on its own. -->
- **A SINGLE-PACKAGE CONVERSION DOES NOT EMIT THE SAME `.csproj` AS THE `-stdlib` RUN** — to prove a
  skipped file is converter output, force the SAME mode to write it. <!-- ⚠ 2026-09-15, the s30 probe, run
     7. The single-package mode conditions `PackageLicenseFile` on the LICENSE's existence and OMITS the
     versioned validation-proof block (`GoValidationProofFile`), so a probe that re-emits a package by
     another MODE and compares to the corpus measures the MODE, not the corpus. Delete the file from the
     cut seed before the `-stdlib` arm instead: it then classes ADDED and scores against the base-era
     blob. -->
- **AN INSTRUMENT THAT DELETES A FILE TO FORCE A WRITE MUST FIRST READ WHAT THE CONVERTER READS FROM THAT
  FILE** — preservation-bearing metadata is EXEMPT from such a deletion. <!-- ⚠ 2026-09-15, train 48 run
     8's LEG D missed three ways with ZERO converter defects. `package_info.cs` carries non-marker lines
     forward from the existing output (the `GoHandOwnTypeAccessibility` block has no preservation code at
     all — it survives by copy-through); a `.csproj`'s `GoHandOwnReferences` are read back from the
     existing file; `platformLayoutDir` picks the per-GOOS folder only if `<goos>/<file>` EXISTS; and a
     missing dependency `package_info.cs` silently degrades to derived aliases. EXEMPT-UNMOVED is a named
     class, not a gate. -->
- **A CONVERTER COMMENT THAT PREDICTS A SILENT FAILURE MODE IS A QUEUED REFUSAL, NOT DOCUMENTATION.**
  <!-- ⚠ 2026-09-15. `platformLayout.go:150-157` names the mode in its own words — "no error, no warning,
     just a quietly different closure" — and that mode ate `math/bits`'s runtime alias in train 48's run
     8. The author had already done the hard half (seeing it); only the refusal was missing. -->
- **`//go:linkname` PUSH WIRING IS A CURATED REGISTRY, AND A MISSING ROW IS A THROWING STUB — A RUN-TIME
  DEATH AT FIRST CALL, NOT A COMPILE ERROR.** <!-- ⚠ 2026-09-15, `fips140`'s three. A package is converted
     from its own syntax, and dependencies contribute TYPES, not directives, so a bodyless declaration
     under a handle is indistinguishable from an assembly stub without a registry row
     (`linknamePushTargets`, keyed by the consumer's fully-qualified declaration; a push ARRIVES as a
     one-line forwarder emitted into the consumer package across an existing project reference, and the
     pusher's definition widens to public via `linknamePushSources`). It is NOT derivable at convert time:
     single-package runs have no pusher in the set, unhonorable rows must be exceptions BEFORE any
     derivation, hand-owns collide, and cycles need a gate. -->
- **A GUARD'S PREMISE CHECK IS READ AGAINST ITS OWN COMMENT** — the predicate must test what the comment
  says, not the nearest thing that was easy to write. <!-- ⚠ 2026-09-15, ruled (i).
     `linknamePushRegistry_test.go` says "if it ever gains a DEFINITION the directive becomes a local
     alias" and was implemented as `findGoFuncDecl` — ANY declaration, bodyless or not — so a self-named
     directive above the consumer's own BODYLESS declaration (`fips140`'s `getIndicator`/`setIndicator`/
     `fatal`) was refused while the emitting matcher admits it. The one existing member (pprof) passed
     only because its local name differs from its declaration, which is why nothing had caught it.
     Narrowed to "the declaration found has a BODY", with the old member still passing, a planted
     defined-with-body fixture still failing, and the new rows passing. -->
- **THE `-tests` VARIANT BOUNDARY CLEARS THE DYNAMIC-TYPE LIFT REGISTRY, AND THE EXTERNAL VARIANT IS SEEDED
  ONLY FROM PRODUCTION** — so a test-only anonymous composite declared in an INTERNAL `_test.go` and used
  from the EXTERNAL one emits as raw Go. <!-- ⚠ 2026-09-15, C2 `b60257ee22`, ruled (b)+(d).
     `packageDynamicTypeNames` is cleared by the next variant's `resetPackageState`, and
     `seedProductionDynamicTypeLifts` seeds only from production's `package_info.cs`; `time.InternalTests`
     at 1.24 is the first member, while `runtime.IfaceHash` resolves ONLY because production happens to
     lift the same signature — which makes it the must-stay-covered control. The converter's "unresolved
     dynamic type" refusal fired by NAME before any build, which is the design working. Fix: carry the
     live map across the reset into a variant-scoped map consulted after the registry and before the
     deferred marker. -->
- **C# CHECKS A GENERIC CONSTRAINT NOMINALLY, SO NEITHER A `ж<T>` BOX NOR A SIBLING INTERFACE EVER
  SATISFIES A METHOD-SET CONSTRAINT — THE ANSWER IS PROJECTION THROUGH THE GENERATED ADAPTER, AND ELISION
  IS NOT A STRATEGY FOR A CONSTRAINT A BOX MUST SATISFY.** <!-- ⚠ 2026-09-15, the RED 3/4/5 family at the
     1.24 version tip. A Go generic `New[H fips140.Hash](h func() H, …)` called as `New(sha256.New, …)`
     infers `H = *Digest`, and the emission `New<ж<Digest>>` fails CS0311 because the box satisfies the
     interface only through generated `[GoRecv]` adapters, never nominally. The converter's answer is to
     substitute the CONSTRAINT as the type argument and project the pointer through
     `GoImplement<Pointee, Constraint>(Pointer = true)` — already done for slice elements (`go/ast`
     `walkList`) and generalized to func-result arguments. The self-referential proxy (nistec) answers a
     DIFFERENT question (F-bounded self-typed boundaries); "generic constraint" and "F-bounded" are not
     the same set. The interface-to-interface case is the same shape with no box at all: Go interface
     EMBEDDING becomes C# interface INHERITANCE, so `hash.Hash` and `fips140.Hash` — identical method
     sets, both embedding only `io.Writer` — are SIBLINGS in C# and satisfy each other only through
     `GoImplement<A, B>`. Never invent an inheritance edge the Go tree lacks. A converter unit-test
     fixture is never compiled as C#, so a negative control there can assert a FALSE PREMISE for a phase
     without anyone noticing: bank these shapes as behavioral goldens that compile. -->
- **A CONSTRAINT PROXY EMITTED `internal` UNCONDITIONALLY BREAKS THE MOMENT AN EXPORTED SIGNATURE USES IT,
  AND A CURE THAT MOVES AN ERROR FROM THE BOX TO THE PROXY HAS RELAXED ONE CLAUSE OF A TWO-CLAUSE
  PREDICATE.** <!-- ⚠ 2026-09-15, G `19f9de075a` and `cacfc57c90`, C2 `3d15626145`. `constraintProxyFor`'s
     G4 names the proxy, but the elided `where P : /* … */ new()` is emitted by `getGenericDefinition`'s
     inexpressible-union arm, which never consults the proxy gate — so relaxing G4 alone moves CS0310 from
     the box to the proxy. A three-arm in-process probe (base / G4 only / G4 + declaration arm, with a
     `fips140` fixture and an `elliptic` control) found it BEFORE the cut and was committed as the seat's
     unit test, because it is the only arm that discriminates the two clauses. Three more axes in the same
     class: the proxy class is emitted `internal` at `ImplementGenerator.cs:1410` while 1.24's exported
     fips140 curves put it in PUBLIC signatures (CS0050 x8); `constraintProxyFor` spells a cross-package
     INTERFACE by its Go import path; and a consumer of a foreign self-referential constraint minted its
     OWN proxy of the same bare name — two classes, one name, two assemblies (CS1503 x16). Population at
     the two pins: 1.23.12 had 10 self-referential constraints, all method sets, 0 elided; 1.24.13 has 30,
     15 elided, 29 sites. -->
- **A CONSUMER-SIDE QUALIFIER RENDERED AS AN ALIAS UNCONDITIONALLY RESOLVES ONLY WHERE THE CONSUMER HAPPENS
  TO IMPORT THE PACKAGE** — use the renderer whose contract is "resolves where it lands". <!-- ⚠
     2026-09-15, G `10daf47fc4`. Both corpus consumers DID import it, so the corpus could not falsify the
     spelling; `getScopeCheckedTypeName` (with Go-path conversion) is the right renderer, and the arm that
     discriminates it is a THIRD-package fixture — a consumer importing only a middle package. The
     pre-A/B single-package probe of the unprobed consumer turned a would-be miss into a stated line and a
     fixed latent. -->
- **THE CS0576 RENAME PRE-PASS TESTS ONLY DIRECT IMPORT NAMES AND EXPLICIT ALIASES, SO A PACKAGE REACHED
  ONLY THROUGH A TYPE GETS A BARE SYNTHESIZED `using` THAT CAN COLLIDE WITH A CHILD NAMESPACE.** <!-- ⚠
     2026-09-15, G `11be4ed385`, ruled (A): test every package NAME in the closure the pre-pass already
     walks. C# refuses a `using X = …;` whose name equals a CHILD namespace of the file's enclosing
     namespace — `hpke.cs` in `namespace go.crypto.@internal` with a `fips140` child — and the package
     arrived through RED 4's widened naming of `fips140.Hash` in a file that never imports it. Census
     lesson recorded with it: a PLANT proves a predicate CAN fire, not that it is the RULE (v2 =
     "existence anywhere" gave 504 false hits, where the converter's rule is VISIBILITY through the
     project-reference closure), and a KNOWN NEGATIVE from the real corpus (`bufio`'s `using io`) is the
     second control beside the known member. -->
- **THE CONVERTER CASTS A CONSTANT IDENTIFIER ARGUMENT OF BUILTIN `min`/`max` TO THE TYPED SIBLING
  OPERAND'S TYPE BUT EMITS A CONSTANT EXPRESSION ARGUMENT UNCAST** — the folded constant must take the same
  path as the identifier. <!-- ⚠ 2026-09-15, RED 6. `UntypedInt` leaves the generic `min<T>` unable to
     infer `T`, so resolution falls to the `params` overload (CS1503). Go types both as untyped constants
     taking the typed operand. Found at the FIRST C# compile of a 1.24 TEST file, which is the reminder
     underneath it: "the host compiles" is a claim about a file no instrument has read until a `-tests`
     emission is built. -->
- **AT A MAP `Set` WHOSE VALUE TYPE IS A POINTER, THE CONVERTER MUST STORE THE POINTER, NOT THE DEREF'D
  LOCAL.** <!-- ⚠ 2026-09-15, RED 10, `x509` `verify.cs:1313`: the emission spelled
     `ref var n = ref Ꮡn.DerefOrNull()` where Go stores `n` itself. Found as a first-compile finding
     BEHIND another red, attributed by a base arm that never reaches its file — which is why every first
     compile behind a cured red is treated as a finding of its own. -->
- **`golib`'s `at<TElem>(nint)` LIVES ON THE `ж<T>` BOX, so a pointer-receiver method on a NAMED ARRAY must
  spell the box, not the deref'd local.** <!-- ⚠ 2026-09-15, RED 5. `type p256Table [16]P256Point` emits
     `ref var table = ref Ꮡtable.DerefOrNull(); table.at<P256Point>(0)` and the deref'd local has no `at`;
     compiling sites spell `Ꮡlevels.at<levelInfo>`. New at 1.24 because 1.23's `p256Table` was a POINTER
     array — the shape did not exist to be got wrong. -->
- **READ THE WRITER'S DISPATCH BEFORE MODELLING A PER-PROJECT EMISSION: AN EXECUTABLE OUTPUT NEVER REACHES
  A LIBRARY-ONLY BRANCH, SO A CORPUS-WIDE WARNING COUNT IS A LIBRARY COUNT.** <!-- ⚠ 2026-09-13, COORD
     reading `projectFileWriter.go:497`. A model of "one line per measurable project" (731) missed by 679,
     because 678 of the 735 behavioral packages are `package main` and the writer routes only
     `outputType Library` through the licensing call. -->
- **A "ONCE PER X" PROMISE IS BOUNDED BY THE PROCESS THAT HOLDS THE MAP: an instrument that runs ONE
  PROCESS PER UNIT counts per unit.** <!-- ⚠ 2026-09-13, C2 `d0807ee31` s2. The license dedupe is a
     process-global `sync.Map` and CNR spawns one converter per project, so "reported once per module"
     bounds ONE invocation and says nothing about a corpus sweep's total. -->

## Test-harness mechanics (important when changing the converter)

- **`dotnet build` does NOT run the converter** — it only compiles committed C#, and a clean build
  leaves the tree clean. **Running the tests re-runs the converter**: `BehavioralTestBase` rebuilds
  `go2cs.exe` via `go build` whenever any converter `*.go` is newer than the binary, then
  re-transpiles, so the behavioral `.cs` may show as modified in git after a converter change.
- **Converted C# projects** build with plain `dotnet build` (target net10.0, C# latest); each
  references `golib`, the `go2cs-gen` analyzer and the stdlib packages it imports. The `$(go2csPath)`
  MSBuild property resolves to `$(SolutionDir)` in Debug builds and is **distinct** from the
  converter's `-go2cspath` flag. The `BehavioralTests` MSTest phases are `TranspileTests`,
  `CompileTests`, `OutputComparisonTests` (Go vs C# stdout) and `TargetComparisonTests` (byte-compare
  the transpiled `.cs` against its `.cs.target` golden).
- **A new converter `.go` file must be registered in `src/go2cs/go2cs-src.projitems`** — nothing
  *builds* from it (`go build` walks the directory), so a missing entry is invisible at the command
  line and only bites in Visual Studio. `projitemsIntegrity_test.go` gates it both ways under the plain
  `go test ./...` from `src/go2cs` and prints the exact `<None Include=… />` line to add;
  `tests/Behavioral/check-solution-integrity.ps1` applies the same invariant to `go2cs.slnx`. The file
  is UTF-8 **with BOM** with uniform line endings — edit it in place or via
  `[System.IO.File]::ReadAllText/WriteAllText`, **never** PS 5.1 `Get-Content`/`Out-File`.
- **`check-no-regression.ps1` re-transpiles UNCONDITIONALLY** (it has no `UpToDate` equivalent), which
  is why CNR is immune to the false-green routes and remains the authoritative drift instrument.
  **Preserve that asymmetry: never add an up-to-date skip to CNR.** **A CNR WRAPPER asserts the VERDICT
  LINE (`==> NO REGRESSION` / `==> CHANGED`) and the measurable count** — CNR builds the converter
  itself, so a THROWN CNR read as "CHANGED 0" is route #6's shape handing back the predicted number from
  a run that measured nothing. A zero from an ABSENT list is not a reading.
- **A behavioral guard reading a child's state through an interpreter the harness does not install
  fails in BOTH directions** — a false-RED generator where the tool is missing, and a **false-GREEN
  where it silently errors into the same string on both sides**, which is two sides agreeing BECAUSE
  NEITHER RAN. Exactly three behavioral projects call `exec.Command` and all three launch `os.Args[0]`
  or `exec.LookPath`; none references `python` and no workflow installs it.

<!-- Phase-1 provenance: the projitems list had drifted silently until 2026-08-06 (shproj
     `go2cs-src.shproj`, member of `go2cs.slnx`; `internal\*` included in the both-ways gate). The
     behavioral-guard rule was measured before asserting, 2026-09-06. Behavioral figures from Phase 1,
     deliberately not quoted in the visible half because they drift: "Since 2026-08-01 those references
     bind the converted packages, so the suite's 515 stdout comparisons against `go run` are also the
     broadest running validation the converted `fmt` gets — its closure is 57 projects (cold ~48 s,
     warm ~4 s)." Most behavioral tests also reference `core/fmt`; a few reference
     `time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`/`reflect`.

     BATCH19, anchor 914 — the CNR-wrapper clause above, and the `GOTOOLCHAIN` constraint it shares with
     route #4:

     ⚠ **A CNR WRAPPER THAT READS A THROWN CNR AS "CHANGED 0" PRODUCES THE PREDICTED NUMBER FROM A RUN THAT
     MEASURED NOTHING** (2026-09-08, i9): `GOTOOLCHAIN=local` on the pairing pin makes CNR's OWN `go build`
     of the converter fail (`go.mod requires go >= 1.24.13`), CNR throws, and the wrapper reported CHANGED 0
     — route #6's shape, caught only by the exit 1. **A CNR wrapper asserts the VERDICT LINE (`==> NO
     REGRESSION` or `==> CHANGED`) and the measurable count; a zero from an absent list is not a reading.**
     Under the two-pin pairing `GOTOOLCHAIN` stays UNSET (auto) while the corpus pin trails the converter's go.mod (1.23.12 during the 1.24.13 hop; closed at go1.24.13, where both read 1.24.13) so the converter build
     can switch UP through the module graph. -->

## The false-green routes

### Route #1 — a STALE `go2cs.exe`

**A hand-invoked `-stdlib` or `-tests` run REFUSES a stale binary** — route #1 closed inside the
converter, where those two paths have no caller to instrument. `go2cs` compares its executable's mtime
against every build input beside it (`converterStaleness.go`, over the `ConverterBuildInputs.cs` set)
and, when any is newer, **ENUMERATES the extent**: count, ten newest paths, which are
emission-affecting (all but a `_test.go`, which `go build` excludes). Those two drivers then exit
non-zero; `-allow-stale-converter` proceeds deliberately and is what an A/B against a PRESERVED binary
passes, so a stale run says so in its own command line. Every other shape — single file or package,
`-recurse` — warns and runs, because that is the scratch-probe loop.

**A staleness warning names a SYMPTOM, not the extent** — the load-bearing half: an advisory naming ONE
file where `find -newer` gave six, four affecting every package's emission, was one line from banking a
right number off an invalid measurement. Four holes: a plain `cp` without `-p` stamps a fresh mtime;
mtime is not content, so a branch switch can fire the refusal; no toolchain comparison (route #4's, in
the harness); silent beside a deployed binary with no converter source tree. **Three derivations of one
predicate are kept in lockstep or one under-reports** — the Go-side predicate is byte-for-byte the C#
`ConverterBuildInputs` one. **The same arithmetic governs TWO BRANCHES of one converter predicate and an
instrument's PRIVATE COPY of a rule the system already defines**: fix it as ONE helper both branches call
or ONE definition the instrument CONSULTS, never a second copy — a drifted instrument copy files its own
target under the SOUND bucket, the worst direction for an instrument whose finding is a zero.

<!-- Derivation, verbatim from Phase 1:

     ⚠ **What that refusal replaced, and why ENUMERATING the extent is the load-bearing half**
     (measured 2026-09-03, the run that motivated the cut): a converter built before a train landing
     emitted the PREVIOUS converter's output while reporting success, and the old ADVISORY named ONE
     file where `find -newer` gave **six** — four of them paths that affect every package's emission —
     so "that entry only touches syscall" was a confident wrong justification one line from banking a
     right number off an invalid measurement. **A staleness warning names a SYMPTOM, not the extent.**
     Four holes are recorded in the cut itself: a plain `cp` without `-p` stamps a fresh mtime; mtime is
     not content, so a branch switch can fire the refusal; there is no toolchain comparison (route #4
     stays the harness's); and it is silent for a relocated binary. One method note from the same cut —
     the Go-side predicate is byte-for-byte the C# `ConverterBuildInputs` one, and a THIRD derivation of
     the embed-directive rule had drifted (no whitespace guard): **three derivations of one predicate are
     kept in lockstep, or one of them under-reports.**

     BATCH19, anchor 789 — two 2026-09-08 instances that widened that rule past the staleness predicate:

     ⚠ **A DEFECT WAS A DRIFT BETWEEN TWO BRANCHES OF ONE PREDICATE** (2026-09-08): the converter's
     literal path has two above-MaxUint32 branches — the UNSIGNED parse (value > MaxInt64) carried the
     native-width rule WITH a comment stating it (`(nuint)` before the `UL`, because a bare `ulong` has
     no implicit conversion) and the SIGNED parse (value <= MaxInt64, where `0x0102030405060708` lives)
     never got it, so a `uintptr` variable initialised from such a constant emitted
     `0x0102030405060708UL` against golib's EXPLICIT-only `uint64` operator (CS0266). Fixed as ONE
     helper both branches call, never a second copy of the rule, with the red control neutering the
     helper. **And the ZERO corpus footprint is DERIVED, not censused**: the corpus compiles 307/307
     today, so any site taking the signed branch in a native-width unsigned context would ALREADY be
     CS0266 — a differing file would be a finding about the COMPILE GATE, not about the fix.
     ⚠ **AN INSTRUMENT CARRYING ITS OWN COPY OF A RULE THE SYSTEM ALREADY DEFINES CAN FILE ITS OWN TARGET
     UNDER "NOTHING TO DO HERE"** (2026-09-08, C2): a census's 2a/2b discriminator copied "the token this
     box reports" as two arms (`INilPointer`, `IChannel`, else 0) while `ManagedPointerTokens.CurrentToken`
     has a THIRD, so a registered object implementing neither projected to 0, compared unequal to its own
     token, and was filed 2b — the SOUND bucket — when it is 2a, the DEFECT bucket, which is the worst
     direction for an instrument whose finding is a zero. Found by READING the classifier for a peer's
     defect, not by looking for it, and fixed by making the rule ONE definition the instrument CONSULTS,
     guarded by name. Its companion perturbation was also a value already in hand. Neutrality at suite scale
     is identical failure counts across the env gate (48/48), predicted before running. -->

### Route #2 — stale OUTPUT

The exe IS current but the runners *skip transpiling* and validate the **previous** converter's `.cs`.
`BehavioralRunner.UpToDate`, `PerformanceRunner.UpToDate` and `BehavioralTestBase.TranspileProject` all
short-circuited on a `.cs`-newer-than-`.go` check alone — and converter work is exactly the case where
the `.go` files *don't* change, so every project stayed "up to date", transpile was skipped, and
Target/Output compared the old converter's output against goldens that same converter had generated:
everything matched and the suite printed **PASS**. **A guard test "validated" that way guards nothing.**
All three now also require the `.cs` to be newer than **`go2cs.exe`**.

**Route #2 has a door NO BINARY opens — a `.cs`-ONLY restore** (`git checkout --` of the behavioral
tree, a `Copy-Item`, an editor save): every `.cs` is stamped newer than `go2cs.exe`, so the up-to-date
predicate is satisfied by HEAD's OWN committed emission — transpile is skipped, `--update-targets`
copies that `.cs` over its golden as a no-op and reports `Updated`, and the run behind it validates the
OLD emission and passes. **A golden re-baseline that re-baselined nothing.** The tell is arithmetic (an
EMPTY numstat where CNR had just read `1 1`); the remedy is CNR's own, rebuild the converter first.
**A re-baseline is believed only after its diff is non-empty and its golden byte-compares against the
on-disk emission.** **The skip is the DEFAULT state after any restore** — the restore's own write stamps
the `.cs` newer than both its `.go` and `go2cs.exe` — so a per-project `--phase transpile` used as
EVIDENCE FORCES the emission (delete the `.cs`, touch the `.go`) and asserts a NUMSTAT, never `pass 1`.

<!-- Fixed 2026-07-20. Verified by neutering a real converter fix (`lhsReusedInLaterRhs`) and
     rebuilding: the old runner reported PASS, the fixed runner reports `FAIL [Target,Output]` with no
     manual touch. Door measured 2026-09-03; a re-baseline path that transpiles unconditionally is queued.
     ⚠ **Corrected 2026-09-04 on the MECHANISM, the door itself standing:** a WHOLE-TREE `git checkout`
     writes the `.cs` and the `.go` within the same instant, so under a strict mtime comparison it does
     NOT reliably leave the up-to-date relation over foreign content — what does is a `.cs`-ONLY restore,
     a `Copy-Item`, or an editor save. The comments in the code say the MEASURED shape, not the plausible
     one; the remedy (transpile unconditionally) is unchanged either way, since it does not depend on
     which restore stamped what.

     BATCH19, anchor 816 — the same door, walked again and resolved the same hour:

     ⚠ **AFTER ANY RESTORE, A FILTERED `--phase transpile` SKIPS BY DEFAULT AND REPORTS `pass 1`**
     (2026-09-08, i9, resolved the same hour): the restore's own write makes the `.cs` newer than both its
     `.go` and `go2cs.exe`, which is exactly the runner's up-to-date predicate, so "main.cs rewritten 41
     seconds ago" was the RESTORE's mtime and the runner never ran. CNR, unconditional, flagged the drifting
     golden; the per-project runner "reproduced the committed bytes" because it did nothing; a FORCED
     transpile with the emission deleted first reproduced CNR's one line at numstat 1/1. **A per-project
     transpile used as EVIDENCE forces the emission — delete the `.cs` or touch the `.go` — and asserts a
     NUMSTAT, never `pass 1`**; the skip is the DEFAULT state after a restore, not an edge case. Two
     instruments disagreeing located the blind spot in an hour, and the lane withdrew its own sentence by
     name. -->

### Route #3 — NESTED sub-library packages were never enumerated

All three transpile gates walked `tests\Behavioral\*` **top-level only**, so sub-library packages nested
inside a test folder (`IoLike\FsLike`, `VersionedImport\vlib`, `CrossPackageArrayZeroValue\bufpkg`,
`GoNamespaceShadow\nsshadowlib`, …) were transpiled by **no gate at all** and their committed `.cs`
froze. The dangerous half: **a sub-library's `package_info.cs` is an INPUT to its parent's transpile**
(the parent reads the sibling's `[assembly: GoImplement]` records to decide whether to mint a local `ᴠ`
value adapter), so a regression there could not make the parent's golden fail — the parent kept reading
stale-but-plausible records. That silently disarmed `ForeignValueImplementSuppression`,
`ValueAdapterDynamicType` and `SamePackageImplementNoWitness`.

All three gates now walk **recursively, DEEPEST-FIRST** (`GoPackageDirs` in
`BehavioralRunner`/`BehavioralTestBase`; recursive `Get-ChildItem` + depth sort in CNR) so a sub-library
is regenerated before its parent consumes it, and `UpToDate` considers nested packages. **Goldens remain
top-level-only** — nested packages have no `.cs.target` (`UpdateTestTargets` deliberately unchanged), so
nested drift is caught by CNR's `git status` and the *cross-package* effect by the parent's golden.

<!-- Fixed 2026-08-02. Phase-1 figures: "17 files across 13 packages had drifted by 2026-08-02, spanning
     three separate increments (the `// <TypeAccessibility>` block, string-literal hoisting, the
     compound-assign result cast)." "Enumeration is now **570 packages** (545 top-level + 25 nested; it
     was 543 = 521 + 22 when this note was written), not 521." Counts drift — measure, don't quote. -->

### Route #4 — a TOOLCHAIN hop did not invalidate `go2cs.exe`

Every rebuild predicate rebuilds when a converter **`*.go` file** is newer than the binary; installing a
new Go toolchain touches **none** of them, so after a hop every predicate still says "up to date" and
every gate runs a binary embedding the OLD release's `go/parser` + `go/types` front end
(`conversionDriver.go` uses `packages.LoadAllSyntax`, the converter's OWN compiled-in type-checker)
against the NEW release's sources. **It does not fail cleanly**: the old parser mis-parses or rejects
new constructs and the run degrades into the best-effort "did not fully type-check" path — which CNR
reports as **NOT MEASURED** (good) and the runners do not. Remedy: one compare, since every Go binary
embeds its toolchain release and `go version <exe>` reads it back.
`src/tests/ConverterBuildInputs.IsConverterStale` — the ONE shared helper all three predicates delegate
to since route #5 — fails stale-wards (unreadable stamp or unanswerable GOVERSION forces the rebuild),
guarded by `TestConverterStalenessConsultsTheToolchain`. **No explicit `go build` is owed after a
toolchain change any more.**

**The two-pin pairing is enforced by the MODULE GRAPH, not only by the shell** — with the run
environment re-exported to the corpus release and the toolchain rule on `auto`, Go switches ONLY the
converter's own build to the newer directive and leaves every corpus module at the corpus release. Gate
it with an AFTER-GUARD that re-reads the BINARY's release; NOT MEASURED beats a count nobody can stand
behind. **`GOTOOLCHAIN` stays UNSET (auto) while the corpus pin trails the converter's `go.mod`** (a hop's two-pin window; at go1.24.13 both read 1.24.13 and the window is closed) so that switch can happen: at
`GOTOOLCHAIN=local` CNR's OWN `go build` of the converter fails `go.mod requires go >= 1.24.13` and
throws, which a wrapper then reports as "CHANGED 0". **The behavioral runner is green by a DIFFERENT route than CNR**: its predicate reads the
toolchain version at the RUNNER's cwd, which has no module file above it and answers the CORPUS release
against the binary's NEWER stamp — so it reads PERMANENTLY STALE and rebuilds on every invocation
(seconds; the content-addressed cache re-links only), which **fails safe: never a stale binary, never a
Transpile skip.** The property is cwd-dependent; the switch resolves through the module cache, so a
cold box fetches the newer release there.

<!-- CLOSED 2026-08-24 (H1.4): "The remedy landed and came out smaller than planned: nothing needed
     stamping, because every Go binary ALREADY embeds its toolchain release and `go version <exe>` reads
     it back." Predicates: `BehavioralTestBase`, `BehavioralRunner`, `PerformanceRunner`.
     ⚠ **THE TWO-PIN PAIRING IS ENFORCED BY THE MODULE GRAPH, NOT ONLY BY THE SHELL** (2026-09-08): with
     the run environment re-exported to the corpus release and the toolchain rule left on `auto`, Go
     switches ONLY the converter's own build up to the newer directive and leaves every corpus module
     loading at the corpus release — CNR under that shell read every package byte-identical, 0 NOT
     MEASURED, with the converter still stamped at the newer release afterwards, gated by an AFTER-GUARD
     that re-reads the BINARY's release (NOT MEASURED being the alternative to a count nobody can stand
     behind). ⚠ BATCH19, anchor 914 (2026-09-08, i9): the `GOTOOLCHAIN` half of that pairing is stated
     visibly because `GOTOOLCHAIN=local` on the pin breaks CNR's own converter build
     (`go.mod requires go >= 1.24.13`) — full narrative in the Test-harness mechanics comment. -->

### Route #5 — a converter build INPUT that is not a top-level `*.go`

The widest trigger of route #1's family. All three rebuild predicates asked whether any **top-level**
`*.go` in `src\go2cs` was newer than the binary; the converter is built from more, and each omission
changes what it **emits** while touching no top-level `.go`: (a) the `//go:embed` assets —
`embeddedTemplates.go` embeds both csproj templates, the `package_info.cs` skeleton, the icons and
`profiles/*`; `stdlibMetadata.go` embeds `stdlib-metadata.txt`; (b) the `internal\` packages the
converter imports (`internal\stdlibmeta` and siblings); (c) `go.mod`/`go.sum`. Edit one and every
predicate reports "up to date", the OLD binary keeps running, and every runner gate validates the
PREVIOUS emission and prints PASS. **The edit reads as a no-op, indistinguishable from "the change was
already correct"** — and a .NET migration's TFM stage edits exactly those templates and profiles
([`docs/DotNetMigration.md`](docs/DotNetMigration.md) §5.2), the step most likely to meet this route.
Route #4's stamp says nothing about a template's mtime.

**Remedy:** `src\tests\ConverterBuildInputs.cs` — one definition of the build-input set, LINKED into all
three projects (the runners take no assembly dependency), with the embedded half **DERIVED from the
`//go:embed` directives themselves** rather than listed, so tomorrow's directive is covered the day it
is written. Two guards under the plain `go test ./...` (`embeddedAssets_test.go`): directive FORMS stay
inside the subset the C# resolver understands, and the three predicates still delegate to the shared
helper. **CNR was never exposed** — no rebuild predicate, `go build` run unconditionally, its cache
content-addressed over embedded assets. Same asymmetry that made CNR immune to routes #2 and #4.

**cmd/go's test cache DROPS files resolving outside the module root** (`computeTestInputsID`) and the
predicate sources live under `src\tests`, outside `src\go2cs` — so the second guard reports
`ok (cached)` and only fails under **`-count=1`**: **a change touching ONLY harness C# owes
`go test -count=1 ./...`.** **The same cache serves a NESTED invocation, the hardest vacuous green to
see** — a Go guard shelling out to a script that itself runs `go test` gets `ok (cached)` and **tests
nothing** while the outer suite genuinely runs and the arm genuinely appears in its output. **A nested
`go test` is always `-count=1`.**

<!-- Found 2026-08-21 by the hop-campaign planning read; fixed 2026-08-22. Measured at the fix:
     **204 top-level `*.go` seen, 224 real inputs — 20 invisible.** CNR immunity A/B-verified: editing
     `csproj-template.xml` changes the linked binary's hash, reverting reproduces it byte-for-byte.
     `computeTestInputsID`'s comment: "Do not recheck files outside the module, GOPATH, or GOROOT root".
     The first guard has no such gap (every input it reads is inside the module). -->
