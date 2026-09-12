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
     that stood before it. -->

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
  must be > 0 (`log.Fatal` otherwise).
- `-go2cspath <dir>` — runtime/stdlib root and default output root (default `~/go2cs`; env
  `GO2CSPATH`); also the root each imported package's `package_info.cs` is read from to mint the
  emitted `<ImportedTypeAliases>` block. A single-package or `-tests` conversion whose root is not a
  go2cs root (no `core\golib\golib.csproj`) SELF-LOCATES up its OUTPUT path's ancestors; when none is
  found, a loud once-per-run stderr warning names the resolved path and the consequence (NOT fatal —
  standalone conversion is legitimate). `-recurse` warns but never self-locates; `-recurse=nuget` and
  `-stdlib` do neither. An explicit *working* root always wins, and every harness passes an EXPLICIT
  `-go2cspath <repo>\src` computed from its own location.
- `-platforms os/arch` — the ONE target emitted for (default: the host), or a comma-separated LIST
  (`-platforms windows/amd64,linux/amd64,darwin/amd64`)
  which **with `-stdlib`** performs the multi-platform EMISSION (`platformEmit.go`): convert per target
  into a seeded staging root (`-platform-stage <dir>`), then MERGE into the `-go2cspath` corpus as
  layout L3 — shared files flat, platform-varying per-GOOS, hand-owns routed to their principal's
  platform set. **A list without `-stdlib` is REJECTED**, never narrowed to the first target.
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
- `-provenance` — deterministic `// Converted from Go source: "<path>"`, GOROOT- or module-relative,
  default OFF, no timestamp. Leading copyright/license notices are emitted even WITHOUT `-comments`.
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

## Reading a `-tests` result

Four checks BEFORE any shape analysis, in order.

1. **The TAIL of `go2cs_test_results.json`** — a deadline kill states itself:
   `{"test":"","action":"timeout","output":"package timeout after <hh:mm:ss>"}`. Match the ESCAPED JSON
   form or parse the field; a substring count of `"action":"timeout"` returns 0 on a record whose tail
   states the kill.
2. **Which artifact.** The results file answers WHETHER the run was killed (its event carries
   `"test":""` and never reaches the per-test maps); the FLAT `<pkg>/go2cs_test_comparison.json`
   (`writeComparisonRecord`, `testConversion.go`) answers WHICH verdicts diverged. **There is no
   `go2cs_test_comparison/` DIRECTORY** — a tail read scripted against a subdirectory path reports "the
   run did not get that far" on a run that completed perfectly. Look for BOTH shapes, say which you found.
3. **Freshness** — results-file timestamp against the comparison's. A stale results.json beside a fresh
   comparison is NOT a deadline kill; a gated or filtered census gates on the CAPTURED STREAM.
4. **Which TREE produced it** — a mass-empty from an invalid tree cannot be attributed either way, and a
   careless cleanup afterwards (killing the converter alone orphans a child holding `runtime.dll`)
   manufactures a SECOND mass-empty with an identical shape.

### The shape of an empty set

The host reports in sorted order, in TWO phases: serial tests, then the parallel batch.

- **Contiguous alphabetical TAIL** = died partway; with NO results file it is a DEAD PROCESS and the
  **FIRST MISSING NAME is the first test reaching the defect** — a root placed without a bisect.
- **ALL empty** = the orphaned-`dotnet run` file lock.
- **SCATTERED** = genuine divergence, with ONE exception: an empty set EXACTLY EQUAL to the package's
  `t.Parallel()` set is ONE serial-phase death parking the whole parallel batch, scattered-looking only
  because parallel names interleave alphabetically. One grep, and the difference between one root and
  dozens of phantom findings. **The predicate computing that set must FOLLOW HELPER DELEGATION** or a
  `t.Parallel()` one frame down becomes a manufactured divergence.
- **A CRASHED row and a FAILED row are different evidence about ONE defect** — a process that wrote its
  results file never reached the faulting call, so its failure is UPSTREAM. Read the split before
  pricing a bisect.
- **ONE hang as the FIRST test after a gate is N phantom empties** — exclude it and re-run before
  reading any other row as a divergence.
- **A deadline RAISED with a byte-identical result is a BLOCK, not slowness** — equal terminal and
  orphan SETS across two deadlines mean one hang worth N verdicts, not N divergences.
- **A death attributed in the PARALLEL phase is a budget EXPIRY with an arbitrary name** (every row
  carries its run event when the batch opens, no pause/continue events, slots are SCHEDULING). **The
  batch's true hang is found only by running each member SOLO under its own deadline.**
- **A stub can HIDE the frame before it** — measure the EARLIER frame on a probe before predicting a
  stub's body clears the row.
- **The tail separates a HANG from a FAILURE**, and only one is disclosable: the same count comes out
  whether the assertion fails or a finalizer blocks until the deadline kills it.
- **A blocker that moves one deeper with an UNCHANGED count is a RESULT** — compare the SYMBOL, not the
  count.

### Host-death signatures

- Goroutine panic: `died on an unrecovered panic in a goroutine`.
- Package deadline: `"action":"timeout"` in the RESULTS file, **not the log** — a kill check that greps
  the log reads a deadline as an unexplained short count.
- **Markerless**: a .NET exception from an unimplemented linkname stub inside `Goroutine.Run` stops the
  stream mid-test with NO EVENT AT ALL; one stub costs every test after it.
- **Module-init deaths DO state themselves in the tail** — a `NotImplementedException` from the host's
  static constructor verbatim, or a `TypeInitializationException` on a per-GOOS package
  (`syscall/windows` loading `kernel32` on Linux); hundreds of `Go=pass / C#=""` rows around one is ONE
  death, not hundreds of divergences.
- **On a flavour's FIRST CONTACT the INIT door is billed FIRST** — a build-tagged `_test.go` whose
  `init()` reaches a throwing stub kills the test package's STATIC CONSTRUCTOR and shadows every row
  (record: conversion-blocked, N Go entries, 0 C#). Probe PAST it with an ITEMISED unbanked patch, then
  count **BY DOOR** (build / crash+unreached / stub / oracle / divergence), unreached shadows on their
  own line.
- **`exit status 0xc0000142` (STATUS_DLL_INIT_FAILED) is a TORN `bin`/publish tree, never a finding** —
  delete the publish dir and re-run, however much its shape (every verdict `C#=""`, stream 0/0/0) reads
  like a corpus-wide regression from your own cut.
- **The host-fatal class is crash OR deadline-consuming HANG**, so a hang skips like a crash — but a
  widening of a RULED class is asked, not assumed, states a fact about TODAY's host, and carries its own
  retirement trigger.
- **A host must write its results file on EVERY exit path** — ALL-PASS stream + exit 1 + no results file
  is `os.Exit(1)` from a `TestMain` leak check skipping the write. **Read the NON-JSON lines of a
  record's error text before naming an exit code**: a crashed host's code lives there
  (`child error exit status 0xc0000005`, child stderr quoted), not in the sweep log's FAIL block — and
  that FAIL block can sit beside a record whose rows all agree and whose acceptance was MET.
- **A converted `runtime.Stack` must render the innermost caller and the runner's own frames the way
  Go's filters expect**, since suites string-match their own stacks; read a frame list against Go's OWN
  call chain before naming its shape (`Stack` never renders its own frame in Go either). The leak-check
  ROOT was a host IDENTITY artifact — `TestMain` on a pool thread with no goroutine identity is a
  FRAMELESS block Go's filter cannot drop — fixed by ADOPTING the main identity for the scope, never by
  re-plumbing the deadline. **A guard filtering survivors by SUBSTRING cannot see a frameless block and
  reads green against the defect** (route #8); a mechanism read on ONE platform is not the other's.
- **A check that NEVER RAN is not a clean check** — the leak check is guarded by
  `if v == 0 && goroutineLeaked()`, so "no Too-many-goroutines text" is the absence of a failure's text,
  not a pass. Confirm the check's own evidence that it EXECUTED.
- **A stub's NAME is not the whole reading when the row's text went to fd 2** — Go's `throw` PRINTS
  before `fatalthrow` and the stream captures the TEST's output, not the process's stderr, so the fix
  may be record-side (a body that continues Go's death path buys fidelity NEGATIVE in verdicts). That
  text can also be PRESENT but UNADDRESSABLE (whole stream in one half-megabyte `errors[]` string) or
  lost outright (a deadline kill's dropped output; a forgiven non-zero exit nil'ing its error); derive a
  bounded tail from the stream the pipeline already holds, never a second pipe.
- **A repair that unlocks hundreds of tests by SUPPRESSING a faithful behaviour is a FALSE GREEN and
  will look like the campaign's biggest win** — `runtime`'s host panics *correctly*, reproducing Go's
  own guard, when an unwaited `testenv.CommandContext` goroutine calls `t.Logf` after its test returned.
  Removing it masks the real defect and makes every verdict behind it unbankable. **Price a repair
  BEFORE writing it**; none of three priced in one week was predictable from the bug's shape.
- **A package priced "mostly stub" from its NAMES can read mostly MATCHED once run** (sized off an
  artifact, not a run), and **many rows can be ONE root** when the package's own ordering leaks a flag
  (`StartCPUProfile` sets `cpu.profiling` before the throw) — proven by skipping ONE test.

### Comparison-record hygiene

- **A FAILED `-tests` build leaves the PREVIOUS comparison record in place** — the family's nastiest
  member: the record is rewritten only when a run COMPLETES, so a fix whose build DIED re-reads the OLD
  record and reads exactly like "the fix does not work", or worse, like a stable count. **Verify the
  build succeeded and the record is NEWER than the edit.**
- **A gated (`-test-filter`) run REWRITES the record and it is not bank-eligible until an UNGATED run
  overwrites it** — but it is SELF-MARKING: `testFilter` carries the filter expression
  (`commandLineOptions.go:61`), guarded both ways by `filterZeroMatch_test.go` (`:149`/`:176` require
  the key on a filtered run, `:163` its ABSENCE on an unfiltered one). Detection is one read.
- **A paired before/after needs two FILES, not two runs** — the record is git-ignored, so the baseline
  overwrites it in place and the diff compares a file with itself, reading "zero moved".
- **`git checkout HEAD -- src/core` + `git clean -fd` clears NONE of the pipeline's ignored state**
  (`bin/`, `obj/`, manifest, records): a "restored" tree is WARM and a filtered run's record travels
  into the next one. Delete records after every sweep; state cold-vs-warm when comparing runs.
- **Preserve a failed row's record to a distinct path BEFORE any restore or cleanup** — deletion is for
  hygiene, never for evidence. Restore a diagnostic patch on a BANK host and delete its records first.
- **The `-tests` pipeline is SILENT on success** (no stdout, no stderr, exit 0): a run's evidence is
  its ARTIFACTS — `*_test.cs`, host, csproj — never its exit code.
- **A record's "differing" count still COUNTS disclosed rows** — print **differing / disclosed /
  UNDISCLOSED** as three figures, and read a regression's row set from the preserved record's own
  `disclosed` and `errors` ARRAYS before assigning a cause; standing disclosures read as regressions.
- **A `C#=""` beside a SUBTEST NAME Go never produced** (`[][]uint8#01`, `t.Run`'s dedup) is two sides
  running DIFFERENT-NAMED subtests, not an empty verdict.

### Instruments around the pipeline

- **`run-validated-sweep.ps1` walks the ROSTER** — `-Filter <pkg> -Exact` on an UNBANKED row throws "No
  banked packages matched" while the wrapping leg exits 0 over the hole. Run such a row through the
  `-tests` pipeline DIRECTLY: it is the STRONGER instrument, since it READS the disclosure manifest and
  hard-errors on an entry naming a test that records a MATCHING verdict. Record that the wrapper was
  ruled and the pipeline substituted, and carry every leg's failure in the wrapper's exit code.
- **A golib-emitted STDERR line is INVISIBLE through the sweep** (log, stderr, results file all silent),
  so a plan driven through it completes GREEN having measured NOTHING — probes drive the published host
  DIRECTLY.
- **A gate whose CLEANUP destroys the artifact it measures reads as a clean sweep** — exclude the
  artifact's path from the cleanup AND assert it present before each leg.
- **A `-clp:ErrorsOnly` build log cannot corroborate an assembly count** (the flag suppresses the lines
  the grep counts): `exit=0 CS=0 MSB=0 asmLines=0` is the INSTRUMENT's zero. **And a DISK count by mtime
  counts FILES, not assemblies** — every behavioral project's `bin` holds a private copy of the shared
  core closure, so a `go2cs.slnx` build reading 46,802 produced 878. Count the log's OWN per-project
  output lines (drop `-clp:ErrorsOnly` to get them), cross-check the wall RATIO at the SAME count, and
  read a disk count (`find … -newermt <pre-build stamp>`, BEFORE the next purge) as proof the build WROTE
  something, never as the population. **A baseline whose population its author cannot name has no business
  being the control**; CS and MSB/NETSDK stay two numbers, and the exit code stays primary.
- **A census taken WHILE THE BUILD RUNS counts the build, not the package** — wait for the producing
  step to EXIT; a disagreeing count is a scheduling question first. A PowerShell `-match` per line
  captures only the FIRST hit; re-derive with `finditer`.
- **A preservation leg keys on the row's EXIT CODE, never the sweep's printed word** — the exit is
  non-zero for every non-pass shape AND every way a row goes NOT MEASURED (unbanked filter, toolchain
  refusal, preflight abort), and the record you most need is often the row that never RAN. Four
  mechanics beside it: ONE dot-sourced definition of the preserved-record NAMING (a second copy drifts;
  a leg that cannot find its helper aborts loudly); a train LABEL from the running script's own
  basename, never written out; a RED control neutered by removing the SOURCE, not by a production-path
  switch; ORDERING proven by a LIVE arm that plants records, runs the real leg, and reads the preserved
  copy PRESENT with the worktree copies GONE. **A REFUSING leg owes three more**: a refused run must not
  TRUNCATE the record it refused (publish-on-exit); its refusal set is PARTITIONED ONCE with the OK flag
  set in ONE place, so a third refusal still aborts; and a named acceptance path emits its OWN anchor,
  never the wired leg's "= 0" sentence for something the run never measured.
- **The preserved-record NAMESPACE is load-bearing** — a moved-set instrument taking the NEWEST
  preserved record as "the previous run" reads a control's synthetic one-row record as the baseline and
  prints a whole-suite FIXED/BROKEN set reading exactly like a catastrophic regression. A control
  writing there REMOVES its own artifact and ASSERTS the removal.
- **An unfiltered enumeration answers "is it there"; a head-limited one answers a different question** —
  `find … | head` filled ten lines with unrelated paths and read as the ABSENCE of a record sitting there.
- **"Absent from source" and "absent from THIS PATH" are different claims, and the weaker phrasing is
  falsifiable in one command — taking the conclusion down with it.** State the scope you measured, not
  the one that sounds stronger; **a verification SCOPED BY THE CLAIM IT CHECKS can only CONFIRM, never
  refute**. Run the UNFILTERED search over the whole tree first: **`| head` is a silent WHERE clause**,
  and knowing that trap by name does not immunise you — only the unfiltered command does.
- **A test sizing its own timeout from `t.Deadline()` converts a DEADLINE into a DURATION** —
  `internal/poll`'s `TestSplicePipePool` consumes 0.9 × T + 6 s at ANY deadline, so it CANNOT time out
  and a longer floor only costs more wall: budget-vs-wall reasoning runs BACKWARDS there.
- **A load-sensitive row is measured SOLO and carries the host's load beside its verdict** — `time`'s
  `TestLongAdjustTimers` takes a hard 60 s wall after 5,000 goroutines and Go's own source says it
  fails on slow hosts. **Two arms failing together under shared load is not an A/B**; no two `time`
  suites share a box, and a control REFUSED by the disk preflight is UNMEASURED, never argued around.

<!-- Derivations for the whole "Reading a `-tests` result" section, verbatim from Phase 1:

     ⚠ **Before ANY shape analysis, read the results-file TAIL — a deadline kill states itself
     outright** (added 2026-08-29 after the third instance in one week): the C# host's
     `go2cs_test_results.json` ends with an explicit
     `{"test":"","action":"timeout","output":"package timeout after <hh:mm:ss>"}` event when the
     package deadline killed it, so the mass-empty diagnosis is a one-line read, not an inference.
     The three lanes that paid it — `bytes` at the 2m default, `sync/atomic` at a lane's own 30m,
     `net/http` at 25m (213 empties published as "divergences across 87 parents" before the tail
     was read) — each had the explicit event sitting in the file the whole time. The shape
     heuristics above remain for the cases the tail cannot settle (a crash leaves no timeout
     event), but the tail is checked FIRST and quoted in any census that reports empty verdicts.

     ⚠ **That tail is a SECOND artifact, and this file named the wrong one until 2026-09-02**
     (corrected against the converter source, not against habit): the package deadline is reported by
     `TestHost` into the host's own `--result` file — `go2cs_test_results.json` — and that event
     carries `"test":""`, so it never reaches the comparison record's per-test maps at all. The
     comparison record is the FLAT `<pkg>/go2cs_test_comparison.json` (`writeComparisonRecord`,
     `testConversion.go`); there is no `go2cs_test_comparison/` DIRECTORY anywhere in the pipeline, and
     `src/core/.gitignore` lists the three files under their real names. Two artifacts, two questions:
     the record answers WHICH verdicts diverged, the results file answers WHETHER the run was killed.

     ⚠ And the tail read has its own false-empty: the event can be carried as an ESCAPED JSON
     string, so a substring count of `"action":"timeout"` returned **0** on a record whose tail states
     the kill (2026-09-02) — match the escaped form too, or parse the field.

     ⚠ **The tail rule is right and its PATH is not universal** (2026-09-06): for a row whose record is
     the FLAT root comparison file holding the full stream, a tail read scripted against a subdirectory
     path reports that the run did not get that far — on a run that completed perfectly, which is the
     worst false reading that rule can produce — so a tail read looks for BOTH shapes and SAYS WHICH IT
     FOUND.

     ⚠ **The tail rule presumes the results file is the RUN'S OWN — verify freshness before
     reading it** (measured 2026-08-29, the gated-census lane): a host invoked **DIRECTLY** with
     `--run` did not rewrite `go2cs_test_results.json`/`.xml` (four-way A/B: order- and
     exit-code-independent; `WriteResults` has no filter guard and the arg parse advances, so the
     mechanism is UNROOTED — do not assert one), while the comparison beside them was written
     fresh. **Scope NARROWED same day by the same lane's own re-test: the `-test-action compare`
     PIPELINE path with a filter DOES write results.json fresh** — the suppression reproduces
     only on direct host invocation, and which half of that difference is load-bearing is
     unmeasured (the routed chip re-measures both paths before anyone asserts a mechanism). The
     durable half of the rule stands regardless: a stale results.json next to a fresh comparison
     is NOT a deadline kill; a gated/filtered census gates on the CAPTURED STREAM; and the cheap
     check is the results file's timestamp against the comparison's.
     ⚠ And the gated/direct freshness split narrows again (2026-09-03): a gated PIPELINE run ALSO
     reproduced the stale `results.json` beside a fresh comparison, so the mechanism stays unrooted and
     **the freshness check — the results file's timestamp against the comparison's — is the rule**, not
     the invocation path.

     ⚠ **And before the tail: check WHICH TREE produced it.** A canary leg returning 232 rows all `C#=""`
     was withdrawn in full without diagnosis, because the tree it came from was not the merge result.
     **Diagnosing a mass-empty from an invalid tree spends real hours on a finding that cannot be
     attributed either way** — and worse, a careless cleanup afterwards (killing the converter alone
     orphans a child holding `runtime.dll`) manufactures a SECOND mass-empty with a different cause and an
     identical shape, whose natural reading is that the first one was real.

     **The tell is the SHAPE of the empty set, and it generalizes to any mass-empty comparison:** a
     contiguous **alphabetical tail** is a run that died partway (deadline or crash) because the host
     reports in sorted order; **scattered** empties are genuine divergence; **ALL** empty is the
     documented file-lock case.

     ⚠ **Refinement measured 2026-08-29 (`net`), and it is the one exception to "scattered =
     divergence": SCATTERED empties that EXACTLY EQUAL the package's `t.Parallel()` test set are ONE
     serial-phase death, not divergence.** The host reports in TWO phases — the serial tests first,
     then the parallel batch — so a single deadlock in the serial phase leaves a contiguous tail there
     AND parks the entire parallel batch unreported, and the union of the two reads as scattered
     because the parallel names interleave alphabetically with the serial ones. `net`'s 43-name
     "deadline family" was exactly this: one deadlock seen from two phases, and the arithmetic closed
     to the verdict once the TransmitFile seam landed. **Compare the empty set against the parallel
     set before reading scattered as genuine divergence** — a set equality is one grep, and it is the
     difference between one root and 43 phantom findings.

     ⚠ **A PREDICATE OVER TEST BODIES MUST FOLLOW HELPER DELEGATION, OR IT MANUFACTURES DIVERGENCES OUT
     OF PARKED TESTS.** A row reported 799 absent verdicts whose set was NOT a contiguous prefix but
     carried 20 holes inside its span; **all 20 were `t.Parallel()` tests and 0 of the package's 67
     parallel tests reported at all** — one serial-phase death parking the whole parallel batch, the
     documented two-phase shape (2026-09-07). ⚠ **The first classification said 18 of 20**: two
     "exceptions" delegate to a helper that calls `t.Parallel()` one frame down, which the regex did not
     follow. Set equality against the parallel set is the discriminator, and the predicate that computes
     that set follows helpers.

     ⚠ **A deadline RAISED with a byte-identical result is a BLOCK, not slowness** (measured 2026-09-02,
     `net` at 40m vs 60m: 501 terminal verdicts / 27 orphans on BOTH runs, both ending at the same test)
     — more budget cannot move a hang, and the stream's last `run` event is what places it. Compare the
     terminal and orphan SETS across the two deadlines before budgeting a longer one: equal sets mean the
     unreported names are one hang plus its serial tail and the parked parallel batch — the re-pricing
     shape (one test worth N verdicts), not N divergences.

     ⚠ **A death attributed in the PARALLEL phase is a budget EXPIRY with an arbitrary name**
     (2026-09-04): a skip-list census names "the last STARTED unfinished row" soundly only in the
     SERIAL phase, because in the parallel phase every row carries its run event when the batch opens,
     the host emits no pause/continue events and several execute at once — the in-flight set at the
     deadline fluctuated 35 → 12 → 33 → 29 → 22 → 20 → 29 → 25 across iterations with rows leaving AND
     joining, so which rows reach a slot inside the budget is SCHEDULING. **The batch's true hang, if
     any, is found only by running each member SOLO under its own deadline.** Companion: **a stub can
     HIDE the frame before it** — rows dying at a hash stub were reinterpreting a slice header one
     frame earlier (a native box over pinned bytes for an unaliasable pair), so measure the EARLIER
     frame on a probe before predicting that a stub's body clears the row.

     ⚠ **One HANG as the FIRST test executed after a gate is N phantom empties** (2026-09-04): every
     later name reads `C#=""` for one blocked test, so EXCLUDE it and re-run to recover the table
     before reading any other row as a divergence.

     ⚠ **THE CRASH SHAPE PLACES A ROOT WITHOUT A BISECT, and the two shapes say OPPOSITE things**
     (2026-09-05, a union whose blast radius on banked rows was read entirely from the sweep leg). A
     contiguous alphabetical tail of empty verdicts with **NO results file** is a DEAD PROCESS, and the
     FIRST MISSING NAME is the first test that reaches the defect — three rows died at their first
     HTTP-server test, their first real connection and their first dial while 18 other rows passed on the
     same host the same hour. Its complement: **a CRASHED row and a FAILED row are different evidence
     about ONE defect** — a process that produced its verdicts and WROTE its results file never reached
     the faulting call, so its failure is UPSTREAM of it (one row completed 341 verdicts and lost only a
     test whose Go source dials in a loop, which moved the suspect off the cert-chain candidates
     entirely). Read the crash/no-crash split before pricing a bisect.

     ⚠ **Three tail-reading rules, 2026-09-06.** **A row's NUMBER can be produced by two mutually
     exclusive mechanisms, and the tail separates them in one command**: 19 of 20 is the same count
     whether the assertion FAILS (the bytes are retained and Go's own `t.Fatal` fires) or the row HANGS
     (the bytes are collected, the finalizer blocks, the collection call never returns and the package
     deadline kills it) — so read the tail BEFORE specifying an instrument, because a hang and a failure
     are not the same question and only one of them is disclosable. ... And **a blocker that moves one
     deeper with an UNCHANGED count is a RESULT**: a host dying on a different symbol in each arm, both
     fatal, reads as an identical verdict count while the wall moved — and what it moved to was the one
     destination neither registry could serve, which turned a declined widening into a measured item on a
     row's critical path. Compare the SYMBOL, not only the count.

     ⚠ **THREE host-death signatures, and the third is MARKERLESS** (measured 2026-09-03): (1) a
     goroutine panic writes `died on an unrecovered panic in a goroutine`; (2) a package deadline
     writes `"action":"timeout"` into the RESULTS file — **not the log**, so a per-slice kill check
     that greps the log reads a deadline as an unexplained short count; (3) a **.NET exception from an
     unimplemented linkname stub thrown inside `Goroutine.Run` stops the results stream mid-test with
     NO event at all** — the mass-empty family's silent member, one stub costing every test after it.
     (The `runtime.Stack(all)` host-killer is a FAMILY of six reached through helpers referenced as
     TABLE VALUES, invisible to a call-graph grep; a derivation with a positive control — one member
     watched killing a slice — is what made the six trustworthy after two confident wrong sets.)

     ⚠ Not every crash is tail-silent: a **module-init** death states itself there too — a
     `NotImplementedException` thrown from the host's static constructor is written into the tail
     verbatim (2026-09-01) — so a mass-empty on a flavor with no run layer is the same one-line
     read, not an inference. ⚠ A second signature for the same door (2026-09-05): 389 rows reading
     `Go=pass / C#=""` whose INNERMOST exception is a `TypeInitializationException` on a per-GOOS
     package — `syscall/windows` loading `kernel32` on a Linux host — is ONE module-init death, i.e.
     the re-pricing shape, not 389 divergences.

     ⚠ **On a flavour's FIRST CONTACT the INIT door is billed separately and FIRST** (2026-09-04): a
     build-tagged `_test.go` whose `init()` reaches a throwing stub takes the host down in the test
     package's STATIC CONSTRUCTOR and shadows EVERY row (436 of 436), the record reading
     conversion-blocked with N Go entries and 0 C# while the classifier's host-crash-at-init
     short-circuit fires. The census probes PAST that door with an ITEMISED, unbanked patch on the
     emitted files before counting anything, then counts BY DOOR — build / crash+unreached / stub /
     oracle / divergence — with unreached shadows on their own line, never as divergences. (A
     prediction can miss in the SHARPER direction — "the first alphabetical half" missing BEFORE the
     first test — and is scored as such.)

     ⚠ A new tail-stated member (2026-09-02, the `chanDir` arm): 388 divergences, every verdict `C#=""`,
     stream 0/0/0 — reading like a corpus-wide regression from the lane's own cut — with the tail saying
     `exit status 0xc0000142` (STATUS_DLL_INIT_FAILED), a TORN `bin`/publish tree from an interrupted
     run. Delete the publish dir and re-run; `0xc0000142` in the tail is a cleanup, never a finding.

     ⚠ **The host-fatal class is crash OR deadline-consuming HANG** (2026-09-05): both lose every test
     after the member in its phase, so a hang belongs in the same skip list as a crash — but **a
     widening of a RULED class is asked, not assumed**, and such an entry states a fact about TODAY's
     host and carries its own retirement trigger. Two companions from the same read: a package priced
     "mostly stub" from its NAMES read mostly MATCHED once run (120 against 35) — the
     phantom-divergence shape, sized off an artifact instead of a run; and **thirteen rows can be ONE
     root** when the package's own ordering leaks a flag (`StartCPUProfile` sets `cpu.profiling`
     before the throw), proven by skipping ONE test and watching exactly one row move.

     ⚠ **A host must write its results file on EVERY exit path, and two 2026-09-03 readings say why.**
     A sweep's printed FAIL block — the stream's last three events plus `oracle-only check: no
     converted-host results file` — read exactly like a dead host while the comparison RECORD held
     1,345 rows both sides, 1,323 passing and three real divergences: the row had reached the END of
     its stream and the acceptance it existed to measure was MET. And an **ALL-PASS stream with exit
     status 1 and no results file** is a third mass-empty member whose cause sat in the record's own
     stderr: `net/http`'s `TestMain` runs Go's goroutine-leak check after the suite, the converted
     `runtime.Stack(all)` rendered the checking goroutine one frame too shallow and through the
     hand-owned host's frame names, none of Go's filter strings matched, the main goroutine counted as
     a leak, and `os.Exit(1)` skipped the results write. **Read the NON-JSON lines of a record's error
     text before naming an exit code**, and note the second rule that falls out: a converted
     `runtime.Stack` must render the innermost caller and the runner's own frames the way Go's filters
     expect, because test suites string-match their own stacks.

     ⚠ **And what that preserved record answers, which the log cannot** (2026-09-02): a crashed host's
     EXIT CODE is nowhere in the sweep log's printed FAIL block (the stream's last three JSON events) —
     it is inside the comparison record's oracle-side error text (`child error exit status 0xc0000005`,
     with the child's stderr quoted). Two neighbours: the `-tests` pipeline is **SILENT on success**
     (nothing on stdout or stderr, exit 0), so a run's evidence is its ARTIFACTS — the `*_test.cs`, host
     and csproj files — never its exit code; and a diagnostic patch applied to a BANK host's tree is
     restored, and its records deleted, before anything banks from that host.

     ⚠ **That leak check's ROOT, measured 2026-09-04: a host IDENTITY artifact, not a leak and not a
     missing frame.** Go runs `M.Run` on the MAIN goroutine with the deadline as a timer; the converted
     host parked the registered main goroutine in its wait and ran `TestMain` on a pool thread with no
     identity — so the host's own main goroutine is a FRAMELESS foreign block Go's leak filter cannot
     drop, and a row whose verdicts all agree exits 1. The fix is the running thread ADOPTING the main
     identity for its scope (one static reference, no per-object state), never a re-plumbed deadline.
     Three companions. A guard arm that filters survivors by a SUBSTRING cannot see a frameless block
     and reads green against the defect (route #8, named by its author). A mechanism read on ONE
     platform is not the other platform's until measured there (the Windows records carry BOTH shapes).
     And **a record's frame list is read against Go's OWN call chain before its shape is named** —
     `Stack` never renders its own frame in Go either, so a block beginning at the caller's caller is
     ONE frame short, and of the three mechanisms that produce it a `NoInlining`-at-the-callee remedy
     fixes exactly one: the guard that DISCRIMINATES them runs before the 35-minute arms.

     ⚠ **And a check that NEVER RAN is not a clean check** (2026-09-04, the same `TestMain`): the leak
     check is guarded by `if v == 0 && goroutineLeaked()`, so on a run where two leaves fail it is
     SKIPPED — and a summariser reading "no Too-many-goroutines text" reported the leak check CLEAN, an
     absence of the failure's text summarised as the check's pass. Confirm the check's own evidence
     that it EXECUTED (here, the per-call dumps, which showed the survivor in every one) before banking
     a clean: the false-empty family's summariser member.

     ⚠ **A stub's NAME in the comparison record is not the whole reading when the row's text went to
     fd 2** (2026-09-05): Go's `throw` PRINTS before it calls `fatalthrow`, the host printed
     `fatal error: …` too, and the record lacked it because the results stream captures the TEST's
     output and not the process's stderr. **Read the host's stderr for the row before sizing a body to
     "make the text print"** — the fix may be record-side (carry the tail) — and note that a body
     which continues Go's death path buys fidelity that is NEGATIVE in verdicts.

     ⚠ **That fd-2 reading can be PRESENT but UNADDRESSABLE, which is a different defect from absent**
     (2026-09-05): the whole combined stream sat inside ONE half-megabyte `errors[]` string with a .NET
     stack trace at its tail, so "unfindable" is the honest statement — while two OTHER paths lose it
     outright (a deadline kill's returned-then-dropped output; a forgiven non-zero exit whose error is
     nil'd). The remedy DERIVES a bounded tail from the stream the pipeline already holds — the lines
     that do not decode as test events — rather than splitting descriptors, because a second pipe
     changes the capture and interleaving of the very stream the verdicts are parsed from; the attach
     predicate snapshots the RAW error BEFORE any forgiveness arm; and because a validated row's record
     stays byte-identical, **the banked row is the no-regression control and the guard is the positive
     control**.

     ⚠ **A REPAIR THAT UNLOCKS HUNDREDS OF TESTS BY SUPPRESSING A FAITHFUL BEHAVIOUR IS A FALSE GREEN,
     and it will look like the biggest win of the campaign.** `runtime`'s host dies because an unwaited
     `testenv.CommandContext` goroutine calls `t.Logf` after its test returned and the converted testing
     host panics — **correctly, reproducing Go's own guard** (2026-09-07). Stopping that panic unlocks
     the parked parallel batch and ~780 unmeasured tests; it also removes real behaviour, masks the
     actual defect (the unwaited command), and makes every verdict harvested behind a weakened host
     unbankable. The legitimate fix is at the tracer root that made the command fail. **Three repairs
     were priced BEFORE being written that week and the answers were all different — pprof's leak WORSE,
     the crash fix BETTER, this one catastrophically better-LOOKING and worse. None was predictable from
     the bug's shape.**

     ⚠ **A FAILED `-tests` BUILD leaves the PREVIOUS comparison record in place** — the family's
     nastiest member, paid three times by one lane (2026-08/09). The pipeline rewrites
     `go2cs_test_comparison.json` only when a run completes, so a fix whose build DIED
     (e.g. a hand-own registered under a bare name where Go declares a method — key `"Type.method"`
     — displacing nothing and duplicating into CS0111) re-reads the OLD record and reports the OLD
     failures: it reads exactly like "the fix does not work", or worse, like a stable count. Before
     believing any post-fix count, verify the build the record claims actually succeeded and the
     record is newer than the edit; for the registry case, checking the placeholder was actually
     emitted is the cheap tell. The record is only the verdict when the run that wrote it completed.

     ⚠ **Three record-file rules, all measured 2026-09-02.** (1) A **gated** (`-test-filter`) run
     REWRITES the package's comparison record — a harvest read `runtime/debug` as bankable off a
     filtered control's record, the only tell being 9 go entries where the full run had 10 — so after
     any gated diagnostic that record is poisoned for banking until an UNGATED run overwrites it.
     ⚠ **This clause said "with nothing marking it gated" until 2026-09-06, and that half is now FALSE:
     a gated record is SELF-MARKING.** The record carries `testFilter` with the filter expression as its
     value (`commandLineOptions.go:61`), guarded BOTH ways by `filterZeroMatch_test.go` — `:149`/`:176`
     require the key on a filtered run, `:163` requires its ABSENCE on an unfiltered one, and `:195`
     records why the key is `testFilter` rather than `gated`. **The ritual above stands unchanged on its
     own merits — a gated record is still not bank-eligible — but "you cannot tell" has become "you can
     tell, and here is the field", so the detection is one read rather than an inference from entry
     counts.** Found by a lane reading a record it had just written, confirmed independently from the
     converter source, and verified at master before this edit: the standing example of why a doc
     describes what was true when it was written and only somebody with a record open ever re-checks it.
     (2) A paired before/after measurement needs two FILES, not two runs: the
     record is git-ignored, so a branch restore cannot bring the "after" back and the baseline overwrites
     it in place — the diff then compares a file with itself and reads "zero moved"; copy each side's
     `results.json` to a distinct path first. (3) `git checkout HEAD -- src/core` + `git clean -fd` clears
     NONE of the pipeline's git-ignored state (`bin/`, `obj/`, the manifest, the comparison and results
     files), so a "restored" tree is WARM and a filtered run's record travels into the next one: delete
     the record files after every sweep, and state cold-vs-warm when comparing two runs.

     ⚠ **Two more, 2026-09-02.** (4) A gate PRESERVES a failed row's comparison record to a distinct
     path BEFORE any restore or cleanup — a union battery deleted the records after a `net/http` sweep
     FAILED, discarding the only evidence of which rows diverged; deletion is for hygiene, never for
     evidence. (5) `run-validated-sweep.ps1` walks the ROSTER, so `-Filter <pkg> -Exact` on an UNBANKED
     row throws "No banked packages matched" while the battery leg wrapping it exits 0 over the hole —
     route #6 in a coordinator instrument: run an unbanked row through the pipeline DIRECTLY, and carry
     every leg's failure in the wrapper's exit code.

     ⚠ **Three record-reading rules from the same week.** A comparison record's **"differing" count
     still COUNTS disclosed rows** — a disclosure is a row that differs by design — so a tail quoted as
     76 was 16 UNDISCLOSED (4.75x inflation on every sizing sentence it appeared in); every tail census
     prints **differing / disclosed / UNDISCLOSED** as three figures. A regression's row set is read
     from the preserved record's own `disclosed` and `errors` ARRAYS before a cause is assigned: two of
     four "regressed" rows were standing disclosures measured four days earlier, and a fix justified by
     them would have rested on a premise the record falsifies. And a **mass-empty member one row wide**:
     a `C#=""` beside a SUBTEST NAME Go never produced (`[][]uint8#01` — `t.Run`'s dedup of two types
     that render to one string) is two sides running DIFFERENT-NAMED subtests, not an empty verdict.

     ⚠ **A GOLIB-EMITTED STDERR LINE IS INVISIBLE THROUGH `run-validated-sweep.ps1`** (2026-09-08): a
     probe's control and summary lines appeared in NEITHER the sweep log, NOR its stderr, NOR the
     results file, so a multi-row plan driven through the sweep would have completed GREEN having
     measured nothing — caught only because the runbook's first rule was "read the CONTROL line before
     any number". **Probes drive the published host DIRECTLY**, and the sweep owes a bounded
     host-stderr capture as its own instrument item. Beside it, the reading that made the probe
     trustworthy: two counters DISAGREEING MAXIMALLY — one pegged at zero on every row while the other
     reached millions — was the prediction's own stated clause, not a fault.

     ⚠ **Two more, 2026-09-06.** (6) **A gate whose CLEANUP destroys the artifact it measures reads as a
     clean sweep**: a checkout over the corpus reverted the very disclosure manifest the leg existed to
     exercise, and the log line for it was an innocuous swept-dirt-restored count of one. The repair is
     BOTH halves — EXCLUDE the artifact's path from the cleanup AND assert the artifact is present before
     each leg — so a future silent revert fails loudly. (7) Rule (5)'s substitute has a standing now:
     **the converter pipeline is not merely what a roster-walking sweep is replaced BY on an unrostered
     row, it is the STRONGER instrument** — it is what READS the manifest, and it hard-errors on any entry
     naming a test that records a MATCHING verdict, so a clean pipeline run with the entries present is
     itself the discriminating check. Record that the sweep wrapper was ruled and the pipeline
     substituted, so the next reader sees a substitution rather than an unexplained difference.

     ⚠ **A `-clp:ErrorsOnly` BUILD LOG CANNOT CORROBORATE AN ASSEMBLY COUNT — the flag suppresses the
     very lines the grep counts.** A stdlib leg stamped `exit=0 CS=0 MSB=0 asmLines=0` and the zero was
     the INSTRUMENT, not the build (2026-09-07). **Count artifacts on DISK** (`find … -newermt
     <pre-build stamp>`) **and count them BEFORE the next iteration's purge** — the same chain's
     `clean-bin` destroyed the windows assemblies ~28 s after the leg ended, so a later census reads 0
     for the purge and not for the build. Second instance in one session of a gate whose CLEANUP
     destroys the artifact it measures; the exit code remained the sound primary verdict in both.

     ⚠ **A CENSUS TAKEN WHILE THE BUILD IS RUNNING IS A COUNT OF THE BUILD, NOT OF THE PACKAGE.** A
     mid-build stub census read 138 where the settled count was 142 (2026-09-07) — four generator outputs
     had not been written yet. The reading was not wrong about what it SAW; it was wrong about what it was
     MEASURING. **Any census over generated artifacts waits for the producing step to EXIT, and a count
     that disagrees with a later one is a scheduling question before it is a finding.** Beside it: a
     PowerShell `-match` per line captures only the FIRST hit, so a line bearing two stubs counts one — a
     Python `finditer` re-derivation is what settled that number.

     ⚠ **How a preservation leg is WRITTEN — five mechanics (2026-09-04).** It keys on the row's **EXIT
     CODE, never on the sweep's printed word**: the exit is non-zero for every non-pass shape the sweep
     knows AND for every way a row can go NOT MEASURED (an unbanked filter, a toolchain refusal, a
     preflight abort), and the row whose record you most need is often the one that never RAN. Beside
     it: ONE definition of the preserved-record NAMING that every leg dot-sources (a second copy of the
     string is the thing that drifts, and a leg that cannot find its helper aborts loudly); a train
     LABEL derived from the running script's own basename, never written out, because a train script is
     a copy of the previous one and a hand-written label survives the copy; a RED control neutered by
     removing the SOURCE rather than by a switch in the production path (a gate with a lie-lever in it
     is one more thing that can be left on); and the property that decides the class is ORDERING —
     proven by a LIVE arm that plants records, runs the real leg, and reads the preserved copy PRESENT
     and the worktree copies GONE after it, which is the exact property a battery lost when its hygiene
     delete ran first. ⚠ The preserved-record NAMESPACE is load-bearing in turn: a moved-set instrument
     that takes the NEWEST preserved record as "the previous run" reads a control's synthetic one-row
     record as the next train's baseline and prints a whole-suite FIXED/BROKEN set that reads exactly
     like a catastrophic regression — so a control writing into that namespace REMOVES its own artifact
     and ASSERTS the removal, and the newest record is verified by hand to be the real prior run before
     the next battery.

     ⚠ **And the filtered-status trap has a SEARCH costume** (2026-09-02): `find … | head` filled ten
     lines with unrelated paths and the truncated view was read as the ABSENCE of a preserved record
     that was sitting there. An unfiltered enumeration answers "is it there"; a head-limited one
     answers a different question — the same split as filtered vs unfiltered `git status --porcelain`.

     ⚠ **"ABSENT FROM SOURCE" AND "ABSENT FROM THIS PATH" ARE DIFFERENT CLAIMS, AND THE WEAKER PHRASING
     IS FALSIFIABLE IN ONE COMMAND — TAKING THE CONCLUSION DOWN WITH IT.** A lane reported a helper as
     *"in EIGHT DOCS and ZERO source files"*; it is in **176 source files**, including the very package
     under discussion. What they had actually measured, and what carried their argument, was that it is
     absent from one specific STORAGE PATH — a sharper and true claim (2026-09-07). **State the scope you
     measured, not the scope that sounds stronger**; a correct conclusion resting on an overstated premise
     is discarded by the first person who greps. ⚠ **And a VERIFICATION SCOPED BY THE CLAIM IT IS
     CHECKING IS NOT INDEPENDENT — it can only CONFIRM, never refute, because the claim chose where to
     look.** The coordinator searched the two files that framing implied, found nothing, and endorsed the
     conclusion as "the sharper claim" — while the disputed symbol existed at a third path and the
     original record had been correct all along. **Correcting a claim's premise and adopting its
     conclusion in the same breath reads like scrutiny and is not**: run the UNFILTERED search over the
     whole tree first. ⚠ The originating trap, for completeness: `git grep -ln <x> | head -8` returned
     eight DOC paths and was reported as "zero source files" — **`| head` is a silent WHERE clause** — and
     the lane had quoted that exact rule to the fleet twice the same evening before walking into it.
     **Knowing a trap by name does not immunise you against it; only the unfiltered command does.**

     ⚠ **A test that sizes its own timeout from `t.Deadline()` converts a DEADLINE into a DURATION**
     (measured 2026-09-03 at 60m and 15m): `internal/poll`'s `TestSplicePipePool` consumes 0.9 × T + 6 s
     at ANY package deadline, so the row CANNOT time out and a `$longTimeouts` floor would only make it
     cost 54 minutes instead of 9 — the budget-vs-wall reasoning of the timeout table runs BACKWARDS
     for that class. Its neighbour: **a load-sensitive row is measured SOLO and carries the host's load
     beside its verdict.** `time`'s `TestLongAdjustTimers` gives itself a hard 60-second wall budget
     after 5,000 goroutines (Go's own source says it fails on slow hosts) and failed on TWO trees at
     once while two `time` suites ran CONCURRENTLY on the i7 — two arms failing together under a shared
     load is not an A/B. No two `time` suites share a box; a control REFUSED by the disk preflight is
     reported UNMEASURED, never argued around.

     BATCH19, anchor 487 — the refusing-leg mechanics added to the preservation-leg rule above:

     ⚠ **FOUR MECHANICS FOR A REFUSING LEG, MEASURED WHILE ONE WAS BEING WRITTEN** (2026-09-08): a REFUSED
     full run must not TRUNCATE the record it refused — the publish-on-exit trap, proven by planting a
     sentinel, making the run refuse, and reading it back byte-identical; a land script's refusal set is
     PARTITIONED ONCE into named classes with the OK flag set in ONE place, so a record carrying any third
     refusal still aborts; a named acceptance path emits a DIFFERENT anchor from the wired leg's sentence,
     because emitting the wired leg's "= 0" text would stamp something the run never measured; and a
     helper's non-`local` loop variable clobbered its caller's `$a` and pointed the script at a file named
     `cut`, reported as "zero anchors read" — the bash cousin of the PowerShell case-insensitive variable
     collision. **`local` every loop variable in a helper.** (That fourth mechanic is carried visibly under
     "Census and launch traps"; the first three amend the preservation-leg bullet.)

     BATCH19, anchor 985 — the reading that SUPERSEDES "count on DISK" in the assembly-count rule above:

     ⚠ **AN ASSEMBLY COUNT TAKEN FROM DISK BY MTIME COUNTS FILES, NOT ASSEMBLIES** (2026-09-08): every
     behavioral project's `bin` holds a private copy of the shared core closure, so a same-box baseline
     first read **46,802** "assemblies" for a `go2cs.slnx` build that produces **878** — caught by
     IMPLAUSIBILITY against the tree's own recorded figure, re-derived from the build log's per-project
     output lines (878, exact), and cross-checked by the wall RATIO at the SAME count (235 s on the i9
     against 923 s on the i7 class; a faster wall with a smaller count would have meant skipped work).
     **A baseline whose population its author cannot name has no business being the control**, and the
     correction is posted, not quietly replaced. CS and MSB/NETSDK stay two numbers. **This SUPERSEDES the
     2026-09-07 `-clp:ErrorsOnly` remedy's "count on DISK" clause** (which stands as the answer to "did the
     build write anything", not to "how many assemblies"); both narratives are kept, newer wins on fact. -->

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
     Under the two-pin pairing `GOTOOLCHAIN` stays UNSET (auto) on the 1.23.12 pin so the converter build
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
behind. **`GOTOOLCHAIN` stays UNSET (auto) on the 1.23.12 pin** so that switch can happen: at
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

## Census and launch traps

**A zero is the INSTRUMENT's until proven the tree's.** Known producers:

- bare `rg` over `src/core` (obeys its `.gitignore`) — census with `git grep` or a raw walk;
- a converted-C# census keyed on a type's spelled NAME (misses every minted alias — `_type`, `Δio`,
  `abiꓸFuncType`, the `ꓸ` family) — resolve the denotation or enumerate the aliases first;
- a behavioral-tree census keyed on the DIRECTORY name (the emission keys on the Go package CLAUSE — a
  `sortlocal/` directory declaring `package sort` emits `sort_package`) — key it on the package name;
- a LEADING-SLASH pattern handed to a native binary from the POSIX shell (the shell rewrites the
  argument first) — use MSYS `grep`, the Grep tool, or no leading slash;
- `git -C <drive-letter path>` from Git Bash under `MSYS_NO_PATHCONV` (fails silently, wrapper says
  "0 dirty") — verify tree state from INSIDE the tree (`cd`, then `git`);
- a build summary's own "0 assemblies written" — check it against artifact mtimes.

**A path is spelled for ONE namespace**, and the wrong spelling fails as something else entirely.

- `MSYS_NO_PATHCONV=1` cuts BOTH ways — it keeps a `cmd /c` argument intact but breaks Windows git:
  `git commit -F /c/path/msg.txt` fails `could not read log file … No such file or directory` on a
  file that exists. `cygpath -w`, or scope the export to the one command that needs it.
- A bash `-f` test and a native tool's path ARGUMENT are different namespaces with NO variable set —
  read a present-then-absent pair (bash `present`, python `FileNotFoundError`) as a namespace split,
  never a timing bug.
- Git Bash rewrites `cmd /c` into `cmd C:\`, so `cmd` opens interactively, reads EOF and **exits 0** —
  drive it from `powershell -File`, and grep a gate's log for its VERDICT LINE before believing exit 0.
- `robocopy` from Git Bash with forward-slash paths copies NOTHING and exits 1, its SUCCESS code.
- Bare `tar` resolves to TWO programs by PATH order (MSYS tar reads `C:\…` as a REMOTE HOST,
  `Cannot connect to C: resolve failed`, so an A/B's old arm never extracts) — name the tar by path,
  and verify the built binary exists at the exact path you will invoke.
- `Start-Process -ArgumentList` in ARRAY form does not quote a path with a space: `C:\Program Files\Go`
  dies as `Failed to access input file path "C:\Program"`, reading like a missing GOROOT.
- The behavioral runner shells out to a BARE `dotnet` — `DOTNET_ROOT` alone does not prevent
  NETSDK1045.
- A PINNED PATH hides a global tool's runtime: with the .NET 10 root prepended `pwsh` exits **150** on
  GOOD and BROKEN files alike, so unsetting `DOTNET_ROOT` is not enough — run
  `env -u DOTNET_ROOT PATH="$ORIG_PATH" pwsh`, derive `ORIG_PATH` by REMOVING the pin directories
  rather than trusting the launch shell, and state the launch shape in the script's header. **A control
  running INSIDE the instrument under its own environment beats one run beside it.**

**An exit code, a match and a label all lie in characteristic ways.**

- **Capture `rc=$?` as the FIRST statement after a command, and gate every step on the previous exit.**
  `git push | tail` makes `$?` tail's (a state-advancing tool read a REJECTED push as success and
  advanced its anchor over unread posts); `cmd | head -N` masks an abort; `cmd || true` reports
  `true`'s; a `$(...)` INSIDE the line reading `$?` resets it. A failing state-advancing tool resets
  itself and exits non-zero with its rejection path POSITIVE-CONTROLLED; claim an instrument change
  only after its control printed the expected line.
- **`local` every loop variable in a helper** — a non-`local` `$a` clobbered its caller's and pointed the
  script at a file named `cut`, reported as "zero anchors read" (bash's PowerShell-collision cousin).
- `MSB4166 "child node exited prematurely"` is BUILD-INFRASTRUCTURE, not a package root — set
  `MSBUILDDISABLENODEREUSE=1` for back-to-back `-tests` queues before believing a diagnostic-free
  build failure.
- A WSL reconfiguration can silently change which USER automation runs as, and the wrapper EXITS 0
  over the permission error in its log — route #6's shape, a runner that cannot reach its own work
  reporting success. The LOG catches it, not the exit code (`wsl -u root`).
- **A CI workflow step is validated TWICE, one layer apart** — a YAML parse passes a `run:` block whose
  PowerShell dies at PARSE (`$p:` in an interpolated string is a scoped-variable reference), and the
  PowerShell parser still cannot see argument SPLITTING (a bare `transpile,compile` is an ARRAY). Parse
  with the real interpreter, then RUN it once locally end to end with the step's own env block.
- Never locate a comparison binary by a recursive glob's first hit —
  `Get-ChildItem -Recurse -Filter <name>.exe | Select -First 1` returns `bin\Release\Go\…` before
  `bin\Release\net10.0\…`, so "C# matches Go exactly" can be ONE binary printed twice. Name the TFM path
  (`bin/Release/Go/<p>.exe` IS THE GO BINARY, `BehavioralRunner/Program.cs:1049`; a stack naming
  `main.cs` is the tell), and positive-control by making the C# side differ once.
- **The LF-anchor trap is not converter-only** — harness C# is CRLF under `eol=crlf`, so an
  LF-anchored patch to `BehavioralRunner/Program.cs` matches zero times and the build reports exit 0 /
  0 errors *because the file never changed*. (`strings -el` for .NET UTF-16 literals, checked against a
  literal known present.)
- Python `print` to a REDIRECTED file on Windows emits CRLF, so a bash `while IFS=… read` loop's LAST
  FIELD carries a trailing CR and every path built from it fails — one assembler's `git merge` failed
  and **the script reported CONFLICT**. Tells: the same merge BY HAND succeeds, and the conflict lists
  NO unmerged paths. Strip fields (`${var%$'\r'}`) or emit LF, and `cat -A` the intermediate file.
- **A failure LABEL is part of the instrument; a wrong one costs more than no label** — print what was
  OBSERVED (`MERGE FAILED — unmerged paths follow; NONE means it was not a conflict`), and withdraw
  EXPLICITLY any estimate derived from a broken instrument.
- **The Bash tool TRUNCATES a command past roughly 6 KB MID-HEREDOC**: the shell reports
  `unexpected EOF` and NOT ONE statement executes — route long text through Write/Edit. Companions: a
  heredoc-fed python anchor holding a backslash must be built from `chr(92)`; a grep pattern ENDING in
  a backslash is a malformed ERE and must be bracketed; PowerShell `-replace` on a lone backslash is an
  invalid regex that errors every run WITHOUT stopping the script; and a python patch whose anchor is
  never satisfied raises `StopIteration` while the chain continues past it when the `&&` after the
  heredoc is missing.

<!-- Derivations, verbatim from Phase 1. The launch traps were numbered informally there ("a sixth" ...
     "a thirteenth"); nothing cites them by number, so they are renamed by subject above.

     **Three more census/launch traps, each paid repeatedly (2026-08-17):** a DEFAULT ripgrep honors
     `src/core/.gitignore` and under-counts the marker census by one — census with `git grep` or a raw
     filesystem walk, never bare `rg` over `src/core`. A census over CONVERTED C# never keys on a
     type's spelled NAME: the converter deliberately mints aliases (`_type`, `Δio`, `abiꓸFuncType`,
     the whole `ꓸ` family), so a spelling-matched scan silently under-reports by every alias in scope
     — measured 2026-08-31 at ~1.9x, when 59 of 117 `Reinterpret` descriptor sites were spelled
     `_type` (runtime's `global using` alias for `abi.Type`) and were invisible to a name-keyed census
     however many times it was re-run. Resolve what the name denotes, or enumerate the aliases first
     and search for all of them. `Start-Process -ArgumentList` in ARRAY form does
     not quote a path containing a space (`C:\Program Files\Go` dies as `Failed to access input file
     path "C:\Program"`, reading exactly like a missing GOROOT — three lanes paid this); pass ONE
     pre-quoted argument string. And `MSB4166 "child node exited prematurely"` is a BUILD-INFRASTRUCTURE
     crash, not a package root — a `-tests` batch measured a package as a hard build failure (eleven
     MSB4166s, zero CS diagnostics) that reached its real 9-of-10 verdict in 45 s once
     `MSBUILDDISABLENODEREUSE=1` was set; set it for any back-to-back `-tests` queue before believing a
     diagnostic-free build failure.

     **Five more launch/instrument traps, each paid 2026-09-01/02.** (1) **Git Bash rewrites `cmd /c`
     into `cmd C:\`** — MSYS path conversion eats the `/c` — so the command never runs: `cmd` opens
     interactively, reads EOF, exits **0**. A "runner gate" passed that way with a log holding only the
     cmd banner; the EMPTY grep for its verdict line, not the exit code, is what caught it. Drive
     `cmd /c` from PowerShell (a `.ps1` launched by `powershell -File`) or set `MSYS_NO_PATHCONV=1`,
     and **grep a gate's log for its verdict line before believing "exit 0"** — route #6's shape,
     hand-typed — but read the NINTH trap below before reaching for that variable.
     (2) **`robocopy` from Git Bash with forward-slash paths copies NOTHING and exits 1**,
     which is robocopy's SUCCESS code — a silent no-op that reads as a completed stage. (3) **The
     behavioral runner shells out to a BARE `dotnet`**, so the SDK must be on PATH: `DOTNET_ROOT`
     alone does not prevent NETSDK1045. (4) **Never locate a comparison binary by a recursive glob's
     first hit** — `Get-ChildItem -Recurse -Filter <name>.exe | Select -First 1` returned
     `bin\Release\Go\…` ahead of `bin\Release\net10.0\…` (G sorts before n), so a byte-identical
     289/289 "C# matches Go exactly" reading was ONE binary printed twice, and the runner reporting a
     real gap was right; name the TFM path, and positive-control an output comparison by making the C#
     side differ once. (5) **The LF-anchor trap is not converter-only** — harness C# is CRLF under the
     same `eol=crlf` pin, so an LF-anchored patch to `BehavioralRunner/Program.cs` matches zero times
     and the build that follows reports exit 0 with 0 errors *because the file was never changed*.
     (`strings` also cannot see a .NET UTF-16 literal: use `strings -el`, and check the checker against
     a literal known to be present.)

     **A sixth, 2026-09-02:** a WSL reconfiguration can silently change which USER a lane's automation
     runs as — after a resolver change the default user flipped, the lane's scripts became unreadable, and
     the wrapper EXITED 0 over a permission error in its log: route #6's shape again, a runner that cannot
     reach its own work reporting success. The LOG caught it, not the exit code; `wsl -u root` is the fix.

     **A seventh, 2026-09-02 — the exit code a PIPE throws away, in three costumes in one day.**
     `git push | tail` makes `$?` **tail's** status, so a mailbox tool reported a REJECTED push as success
     and advanced its read anchor to a local-only commit, marking unread posts read; `cmd | head -N`
     masked a toolchain wrapper's abort, which is why its first negative control read 0; and
     `cmd || true; echo "exit: $?"` reports `true`'s status. Capture the real exit BEFORE any pipe (to a
     file when the command must also be read), and make a failing state-advancing tool reset itself and
     exit non-zero — with its rejection path POSITIVE-CONTROLLED, since no normal run exercises it.
     ⚠ **A FOURTH costume, and it needs no pipe at all** (2026-09-05): a `$(...)` substitution INSIDE the
     line that READS `$?` resets it — `echo "end $(date) exit=$?"` reported `exit=0` for a script that had
     exited 2 having deleted nothing. **Capture `rc=$?` as the FIRST statement after the command, then
     format.** Three companions from one bad call the same day, all the same shape: a python patch whose
     anchor predicate was NEVER satisfied raised `StopIteration` and the chain CONTINUED past it because
     the `&&` after the heredoc was missing; a control then invoked a switch that did not exist; and
     `cmd | tail -1; rc=$?` captured `tail`'s zero — after which a post asserted the fix as done. So:
     **gate every step on the previous one's exit**, and **a post claims an instrument change only AFTER
     its control has printed the expected line.**

     **An eighth, 2026-09-03/04 — a CI workflow step is validated TWICE, one layer apart.** A PyYAML
     parse passed a `run:` block whose PowerShell died at PARSE one second into the step on both mac
     legs (`$p:` inside an interpolated string is a scoped-variable reference), so the gate was one layer
     short: **a step is parsed by the INTERPRETER that will run it** — extract every `shell: pwsh` run
     block from the parsed workflow and hand it to the PowerShell parser, positive-controlled on the
     broken step (3 errors before the fix, 0 after). The amendment came the same hour, because that
     parser cannot see argument SPLITTING: a bare `transpile,compile` in PowerShell argument position is
     an ARRAY, so the runner received two words and died one layer past the parse gate. **So: parse it
     with the real interpreter, then RUN it once locally end to end, in whatever flavour the host has,
     with the step's own env block, before the push** — the third dispatch was the first whose step had
     executed anywhere. Same family as the `cmd /c` and `$ErrorActionPreference` traps. ⚠ A related
     reading rule: **a build summary's own predicate is checked against the artifacts' mtimes before
     "0 assemblies written" is believed** — 13,779 in-window assemblies stood behind a wrapper line that
     read zero.

     **A ninth, 2026-09-04 — `MSYS_NO_PATHCONV=1` cuts BOTH ways.** The switch that keeps a `cmd /c` or
     PowerShell argument intact ALSO disables the conversion Windows **git** relies on, so
     `git commit -F /c/path/msg.txt` fails with `could not read log file … No such file or directory`
     while the file verifiably exists — a message that reads as a missing file and is a path-FORM
     failure, diagnosed only when a second attempt failed identically with the file's existence proven
     first. In a shell that exports the variable, spell every path handed to a native Windows tool in
     Windows form (`cygpath -w`), or scope the export to the ONE command that needs it: a fixup that must
     commit and then run a PowerShell instrument is exactly where the two requirements collide.
     ⚠ It cuts a third way, at `git` itself: **`git -C <drive-letter path>` from Git Bash under
     `MSYS_NO_PATHCONV` can FAIL SILENTLY**, after which the wrapper around it reports "0 dirty" — the
     INSTRUMENT's zero, not the tree's (2026-09-04). **Verify tree state from INSIDE the tree** (`cd`,
     then `git`).
     ⚠ And the same split bites with **no variable set at all: a bash `-f` test and a native tool's
     path ARGUMENT are different namespaces** (2026-09-06). A probe printed `record: present` from bash
     and then died `FileNotFoundError` inside python on the SAME path string, because `/c/...` resolves
     for the shell and not for a native interpreter. The contradiction reads as a race or a vanished
     artifact and is one path spelled for two namespaces: **`cygpath -w` before handing a path to a
     native tool, and read a present-then-absent pair as a namespace split rather than a timing bug.**

     **A tenth, 2026-09-04 — bare `tar` resolves to TWO PROGRAMS on a Windows box depending on PATH
     order**, and only the system one (`System32\tar.exe`) reads a Windows path: MSYS tar reads `C:\…` as
     a REMOTE HOST (`Cannot connect to C: resolve failed`), so a two-seeded A/B's OLD arm never extracted
     and its binary never built. "It worked last time" proves which binary answered last time and nothing
     else. The abort guard — verify the built binary exists at the exact path you will invoke — fired, so
     nothing was measured against an absent arm; but the invocation's `> log 2>&1; echo EXIT=$?; grep`
     reported the GREP's exit over that abort, and the tell that remained was a three-minute wall against
     an expected twenty-five. **Name the tar by path in an instrument, and capture the native exit BEFORE
     anything touches `$?`.**

     **An eleventh, 2026-09-05 — the Bash tool TRUNCATES a long command, and nothing runs.** A command
     past roughly 6 KB is cut MID-HEREDOC, so the shell reports `unexpected EOF` *inside the heredoc* and
     not one statement executes — the silence-not-error family through the tool's own door. **Route long
     text through Write/Edit and keep the Bash call short.** Its backslash companion: a heredoc-fed python
     anchor containing a backslash must BUILD it from `chr(92)`, because the doubled form collapses on the
     way through — the same reason a census grep pattern that ENDS in a backslash is a malformed ERE and
     must be bracketed. (`.Replace` for a literal in PowerShell, too: a `-replace` whose pattern is a lone
     backslash is an invalid regex that errors on every run WITHOUT stopping the script.)

     **A twelfth, 2026-09-06 — a NATIVE binary invoked from the POSIX shell never sees a pattern with a
     LEADING SLASH.** The shell's path conversion rewrites the ARGUMENT before the binary starts, so a
     ripgrep count of a home-directory prefix spelled with its leading slash reads a clean, well-formed
     ZERO against a file that plainly contains it, while the same pattern without the leading slash reads
     1 and the MSYS-side `grep -cF` on the slash-leading form reads 1 (measured both directions, plus the
     control that disabling the conversion then breaks the FILE path too — which is what proves the
     mechanism rather than merely fitting it). **Every security census keyed on a home-directory prefix
     uses the MSYS `grep`, the Grep tool, or a pattern with no leading slash** — the same path-conversion
     family as the shell eating a command interpreter's switch, one argument over, and exactly the
     false-clean a pre-post census exists to prevent.

     ⚠ **TRAP (5)'s FAMILY IN A THIRD COSTUME, AND THE LABEL IT WORE (2026-09-06).** Python's
     `print` to a REDIRECTED file on Windows emits CRLF, so a bash `while IFS=… read` loop's **LAST
     FIELD carries a trailing carriage return** and any path built from it silently fails: a drop-train
     assembler read its seat list that way, every `-F <message-file>` path ended in a CR, `git merge`
     failed on the bad path — and the script reported **CONFLICT**. Two tells, both cheap: the same
     merge run BY HAND succeeds, and the reported conflict lists **NO unmerged paths**. Strip every
     field (`${var%$'\r'}`), or have the writer emit LF explicitly, and `cat -A` the intermediate file
     before believing any loop that reads it — the same family as the LF-anchored patch and the UTF-16
     log. And **a failure LABEL is part of the instrument; a wrong one costs more than no label**: that
     `CONFLICT at <seat>` named a CAUSE that would have been acted on (a conflict resolution nobody
     needed) and inflated a published cost estimate, so a failure branch prints what it OBSERVED
     (`MERGE FAILED — unmerged paths follow; NONE means it was not a conflict`), never what it assumes,
     and **an estimate derived from a broken instrument is withdrawn EXPLICITLY** — said to be
     unverified until a clean run replaces it — rather than quietly re-derived.

     **A thirteenth, 2026-09-07 — a PINNED PATH HIDES A GLOBAL TOOL'S RUNTIME.** With the .NET 10 root
     prepended, `pwsh` — a dotnet global tool needing the .NET 8 runtime — exits 150 on a GOOD and a
     BROKEN file alike, so a workflow-parse arm that unsets only `DOTNET_ROOT` is still dead under the
     pin. A train assembly's own parse control caught it (good=150 / broken=150 against a want of 0 / 1)
     and REFUSED before merging anything; the arm runs with the PRE-PIN PATH captured before the pin
     (`env -u DOTNET_ROOT PATH="$ORIG_PATH" pwsh`), proven good=0 / broken=1 / without-restore=150 in the
     pinned shell. **A control that runs INSIDE the instrument under the instrument's own environment is
     worth more than the same control run beside it.** ⚠ **And the capture itself has a LAUNCH
     dependency**: the next train's assembly was launched from a shell that had ALREADY exported the
     go/dotnet pin, so `ORIG_PATH` captured the PINNED path and the same arm read 150 on both files again
     — the control caught it (NOTHING assembled, ABORT) and the relaunch cost two minutes (2026-09-08
     00:00). **A derived script states its launch shape in its header, and a robust one derives
     `ORIG_PATH` by REMOVING the pin directories rather than by trusting the launch.**

     BATCH19 (2026-09-08), three further instances of traps already carried above.

     ⚠ **`bin/Release/Go/<p>.exe` IS THE GO BINARY, not the C# one** (named at
     `BehavioralRunner/Program.cs:1049`): running it as the C# side produced `8 1` exit 0 twenty times
     — one step from a false "passes in one build flavour, fails in another" finding. The C# program is
     `bin/Release/net10.0/<p>.exe`, and a stack naming `main.cs` is the tell. Disclosed by its author
     before it was posted; the same family as trap (4) above.

     ⚠ The `local`-in-a-helper bullet above is the fourth mechanic of the refusing-leg reading (anchor 487,
     2026-09-08): a helper's non-`local` loop variable clobbered its caller's `$a` and pointed the script
     at a file named `cut`, reported as "zero anchors read" — the bash cousin of the PowerShell
     case-insensitive variable collision. Full narrative in the "Reading a `-tests` result" comment.

     ⚠ The DIRECTORY-vs-PACKAGE census producer above (anchor 852, 2026-09-08, G): the emission keys on
     the package CLAUSE — a `sortlocal/` directory declaring `package sort` emits `sort_package` — so a
     directory-keyed census of root-package collisions read NONE while the package-keyed one read exactly
     one file, the lane's own guard. The Go-side twin of the minted-alias rule: a directory census answers
     a different question and reports a clean zero for the wrong reason, and the guard being its own
     counter-example is what exposed it before publication. -->
