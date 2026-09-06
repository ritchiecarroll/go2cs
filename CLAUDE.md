# CLAUDE.md — go2cs orientation

> Canonical orientation for any Claude/AI task working in this repo. This file is **authoritative**;
> where it disagrees with `docs/README.md` or the `.bat`/`.cmd` build scripts, those are considered **stale** —
> trust this file and the source. See companion docs: [`docs/Architecture.md`](docs/Architecture.md),
> [`docs/Roadmap.md`](docs/Roadmap.md).

> **Document authority — one ladder.** This file is repo doctrine and gates. The two migration
> **runbooks** — [`docs/GoCorpusMigration.md`](docs/GoCorpusMigration.md) and
> [`docs/DotNetMigration.md`](docs/DotNetMigration.md) — are the **living procedure** for release
> hops: they **lead**, amended in-stage from lessons learned, and no plan or record overrides them on
> procedure. `docs/PLAN-*.md` hold ruled strategy and instance campaigns; their OQ rulings are
> settled, and only a new ruling reopens one — but a ruling's **SCOPE** is a claim about *who reaches
> the seam*, not a permanent property: the netpoll ruling's "zero runtime edits" covered
> `internal/poll`'s consumers and never runtime's own suite reaching `netpollGenericInit` through an
> `export_test` re-export, so a NEW consumer re-opens the scope question without re-opening the
> ruling (2026-09-01; the remedy was one honest no-op equivalence, both halves measured). `docs/phase4/` RECON-/REHEARSAL-/CENSUS-/DATA-/STAGE0-
> files are point-in-time **records** — amended with dated blocks, never rewritten, never executed
> from. The BOARD is the append-only findings ledger; the mailbox is transport, not record. A lesson
> lands the day it is learned: procedure → the runbook; harness/gate doctrine → this file; findings
> and measurements → the board. (Doc-type definitions: [`docs/Glossary.md`](docs/Glossary.md),
> *Document types*.)

## What this is

`go2cs` is a **transpiler that converts Go source code into C#** that is both *behaviorally* and
*visually* similar to the original Go — the goal is that a Go developer can read the generated C# and
follow it easily. Go's compiler-provided conveniences (slices, maps, channels, multiple returns,
defer/panic/recover, goroutines, struct embedding, interface duck-typing) are emulated either by a
hand-written runtime library or by Roslyn source generators, so the visible converted code stays close
to the Go original.

This is the **"go2cs iteration 2"** generation of the project: the converter is now **written in Go** using the
official `go/ast` + `go/types` toolchain. The earlier converter (C# + ANTLR4 grammar) is fully retired —
the last build scripts that referenced it (`convert-gosrc.*`) were removed 2026-07-11.

> **General working principles** (think before coding, simplicity first, surgical changes, goal-driven
> execution) live in the user-global `~/.claude/CLAUDE.md` so they apply across all projects. This file adds
> the go2cs-specific discipline: root-cause against the real emitted `.cs`/`.cs.target` (the golden is the
> authoritative record) and **read the emission BEFORE spending a gate battery — it is the cheapest layer
> and it keeps paying**, keep the A/B footprint minimal, change *only* the goldens a fix must, and prove no
> corpus drift with `check-no-regression` — **compiling is not correctness** (that is the Phase-3 → Phase-4
> distinction). And **prefer the durable path over the shortcut**: when a task could be solved
> quickly-but-throwaway or correctly-but-harder, take the harder, general fix — a converter change over a
> one-off hand-patch, a real root cause over a workaround, the reproducible-from-repo result over a deploy-only
> hack. go2cs is a long-horizon project; work that advances the long-term vision is worth the extra effort, and
> throwaway code that has to be redone later is a net loss even when it ships faster today (the
> *nothing-throwaway* principle). This does not license speculative machinery — it is still the *minimal*
> solution, just the one that generalizes and lasts rather than the one that merely unblocks today.

## Architecture map

| Component | Location | Language | Role |
|---|---|---|---|
| **Converter** | `src/go2cs/*.go` (~67 files) | Go | Parses Go with `go/ast`/`go/types`, emits C#. |
| **Runtime library (`golib`)** | `src/core/golib/` | C# | Hand-written Go semantics: `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `array<T>`, `builtin` (`append`/`len`/`make`/`panic`/`recover`…), `ж<T>` heap box, `nil`, type aliases. **Shared by everything; never auto-overwritten.** |
| **Source generators** | `src/gen/go2cs-gen/` | C# (Roslyn) | Compile-time Go semantics: `ImplementGenerator` (interface impl), `RecvGenerator` (pointer-receiver overloads), `ImplicitConvGenerator` (type-alias conversions), `TypeGenerator` (struct embedding/promotion). Referenced as an **analyzer** by every converted project. |
| **Standard library** | `src/core/<pkg>` | C# (converted) | The whole Go stdlib (**305** converted packages on disk as of 2026-08-13 — **306** projects under `src/core` counting the hand-written `golib`; Go 1.23.12) auto-converted by `go2cs -stdlib`. **Compiles clean** (Phase-3 milestone, 2026-07-10); 146 packages **validate operationally** against `go test` as of 2026-08-16 — this figure drifts fast during campaigns; the roster header in `docs/ValidatedTestPackages.md` (recomputed from its own table) is the authority, swept by `src/run-validated-sweep.ps1`. Two packages are hand-owned and never queued for conversion: `unsafe` and `testing` (the Phase-4 test host). Its own `src/go2cs-stdlib.slnx`, generated by the converter and adopted verbatim. |
| **Behavioral tests** | `src/tests/Behavioral/` (593 top-level test projects = 621 transpiled Go packages incl. 28 nested sub-libraries; 622 registered `.csproj` incl. the `BehavioralTests`/`BehavioralRunner` tooling — re-measured 2026-08-17; this row drifts within DAYS during guard-heavy campaigns, so **measure, don't decrement**: `Get-ChildItem -Recurse -Filter *.go` on unique directories is the transpiled-package count, `check-solution-integrity.ps1` prints the registered count) | Go + C# | Per-feature Go↔C# equivalence (arrays, channels, defer, generics, interfaces…). |
| **Performance tests** | `src/tests/Performance/` (14 `Perf*` benchmarks + `PerformanceRunner`) | Go + C# | Go vs transpiled C# (JIT **and** Native AOT) time/memory comparison — results table in its `README.md`. |
| **Examples** | `src/Examples/` | Go + C# | Hand-converted Tour-of-Go / go101 / misc samples. |

**Two solutions, one tree:** `src/go2cs.slnx` = converter-dev workspace (golib + `go2cs-gen` + all
tests/examples/utilities + the ~61 `core/` packages their closure reaches) — **builds green**.
`src/go2cs-stdlib.slnx` = every `core/` package (**307** projects since layout L3 — the three platform-exclusive
adopted verbatim; it is what `push-nuget.ps1` packs. They overlap deliberately — same tree, same paths,
different scope — so a project can be opened from either. (The old hand-maintained classic `.sln` files
are all retired -- `src/go2cs-examples.sln` included, removed before the 75% anchor; only the two
`.slnx` remain.)

⚠ **NOTHING routinely builds `go2cs.slnx` end to end, so a broken solution member rots invisibly.**
Every harness — `BehavioralRunner`, MSTest, `check-no-regression.ps1`, `run-validated-sweep.ps1` —
builds each `.csproj` **by path**, never through the solution (the same by-path habit
`check-solution-integrity.ps1` polices from the other direction). A solution member that no gate
compiles can therefore break while every gate stays green. That is what happened to the
`utilities/QuickTest` scratch project: the r41 GoFrame arc (`6adab2909`, 2026-08-05) retired golib's
`func`/`Defer`/`Recover` execution-context API and carried the corpus with it, but QuickTest was
hand-written scratch nobody builds, so it took `go2cs.slnx` down for two days unnoticed. It was
**retired** on 2026-08-07 rather than hand-fixed again: it was the solution's only hand-written,
un-gated member, so it would have rotted at the next golib change, and the experiments it held
(struct/interface promotion hand-simulated *before* `go2cs-gen` existed) are covered by real
behavioral tests now. Git keeps it at `d3223d252` if a shape is ever wanted back. **After changing a
golib/runtime API, build `src/go2cs.slnx` once before banking** — ~90 s, and no other gate covers it.
⚠ Two golib cost rules, both measured 2026-09-01: **a golib change adding INSTANCE state to `ж<T>`
(or any per-box base class) is a corpus-wide byte-cost change** — +8 B lands on EVERY pointer box,
proportional to boxes allocated per path (measured 14/1/0 boxes across three alloc rows), so the
commit states the cost even when correctness demands the field (the element-aliasing publish gate
did; its unfavorable direction shipped unmeasured and later burned an attribution run). And **an
alloc row's B/op is only comparable against a figure taken at the same suite scope** — filtered vs
unfiltered differed by +167.04 B/op on ONE tree (AllocsPerRun's single warmup doesn't cover
one-time costs a full run has already paid), so a filtered census never compares its bytes against
a full-run record: the alloc-instrument sibling of the gated-census stream rule.
⚠ **THE ALLOC INSTRUMENT'S OWN CONVERGENCE — six rules from one arc (2026-09-04), because a byte
endpoint quoted off an unconverged instrument is a false measurement.** **A prediction's BASELINE is
measured at the same scope in the SAME RUN, never quoted from a record** — a 1,457.8 B/run baseline
taken out of a design record read 1,510.8 under the acceptance's own filter (the comparability rule
above, met on the prediction side) — and a reduction LARGER than predicted does not make the
prediction less wrong: both corrections go in the commit message, not only in the post. **A reduction
claim quotes the deterministic FLOOR** (the minimum over reps) **or a high-runs figure, and names its
UNIT and its CONFIGURATION**: the measured os row is 1,320.00 B/run in every configuration except
Release+tiered (1,256, tier-1 escape analysis stack-allocating one non-escaping box), and a 100-run
`AllocsPerRun` sample carries a fixed 0–800 B/run per-window accounting term, so it cannot resolve a
change under ~150 B/run. golib's object COUNT is charged at the `new` while the BYTE cost diverges
under a tiered JIT — a box the JIT stack-allocates still counts 1.00 — so no JIT improvement banks a
count-conditioned row; only not constructing the boxes does. **A COUNT-based byte prediction is a
LOWER bound whenever the cut also UN-ESCAPES surviving boxes**: six deleted boxes at 64 B predicted
384 and the converged floors read 512 (1,320.00 → 808.00), the two surviving receiver boxes having
stopped escaping. **A minimum over 40 draws of a 100-run window is unconverged by a near-constant
offset (+42.5/+44.4 here) that CANCELS in a DIFFERENCE of two such minima** — so a difference of
unconverged minima may be right while both absolutes are wrong; record absolutes from the converged
instrument, and when the count is exact and the bytes do not close, SEGMENT (a per-frame byte probe
with literal tags, an exact segment sum and a one-row positive control) before naming a mechanism.
**An instrument that reads HIGHER on strictly FEWER objects is telling you it is NOT CONVERGED** — a
40-rep minimum read 789.8 after a cut removed two boxes from a tree reading 785.0, impossible for a
true floor — and that reading is REPORTED as non-convergence, never smoothed or explained. Finally,
**a static census of "boxes" is bounded by the INSTRUMENT's population**: golib's counter counts
golib allocation sites only, so a defer's delegate, a params array and an interface box are not among
the counted eight, and a capability can have TWO populations of different sizes (239 sites minting a
box AND a delegate, 207 a delegate alone) of which only the first is visible — census BOTH, segment
the row with the converged instrument BEFORE sizing an increment against it, and size a lowering by a
`go/ast` census with each exclusion counted separately, because the exclusions are the residue the
null owns. A count is the unit that carried information at every step of that arc; the bytes needed
the instrument.
⚠ **Four more from the same ladder's next increment (2026-09-04), each a way to quote a wrong
endpoint.** **A cut that removes boxes an EARLIER cut already UN-ESCAPED saves ZERO bytes** — deleting
two receiver boxes read 744.25 → 744.25 B/run with the count 10 → 8, because an earlier increment had
already priced those two at 0.00 B: 64 B is a property of a box that ESCAPES, the converse of the
count-based LOWER-bound rule above, so the unit is re-read from the CURRENT tree's segment table
before every prediction and never carried down the ladder (that prediction was exact on the count and
wrong by the whole 128 B). **A reading is comparable only under the SAME WINDOW PROTOCOL as the record
it is read against**: the converged 744.25 B / 8 obj is the FLOOR of windows 2 and 3 of three
1,000,000-run windows in one process, while a 100-run single window read 898 / 916 / 895 — the count
exact, the bytes ~150 B high and varying, because a short window cannot dilute the amortised slack —
so stop-and-reconcile before reading anything against the record (a GC-configuration hypothesis, gen0
0 against 99, was tested with ONE variable and dropped when it moved nothing but the noise). Three
ladder mechanics beside them: window 1 of a 1,000,000-run `AllocsPerRun` reading always sits ~1.3
B/run above the integer floor while windows 2–3 land on it, so **TWO windows is the minimum
protocol**; `-p:BaseOutputPath` and `-p:BaseIntermediateOutputPath` on the command line are GLOBAL
properties that propagate into every referenced corpus project and mint parallel `obj-*` trees under
`src/core` which then collide on the SDK's default compile glob (CS0579/CS1537), so an
outside-the-repo probe project sets `EnableDefaultCompileItems=false` with one explicit `Compile`
item; and a segment an instrument could not enter is reported COMBINED with its split marked DERIVED,
cross-checked against an identical construction measured directly in the same run, so nobody quotes a
derivation as a reading. ⚠ And **a LAYOUT read (`Unsafe.SizeOf<T>`) is decided by the field set at JIT
time and cannot move with load**, so the loaded-versus-solo rule does not apply to it — but a size row
is an ASSERTION only after a control grows the struct by one word and reads RED; before that it is a
baseline that passes either way.
⚠ **A REPRESENTATION choice is measured on the AXIS THAT SEPARATES ITS CANDIDATES** (2026-09-05):
three arms of a box-slot design read their predicted bytes to the byte on the alloc row — which
separates only the slot's +8 B/box — and were told apart ONLY by a synthetic 10M-call hot loop
(18.0 / 20.2 / 37.0 ns against a 33.8 baseline), because a syscall-dominated row cannot resolve a
lookup at all (the TLS handshake sat in a 4% spread in no consistent order). **Name the instrument
that CAN see the difference before the arms are built**, and state a corpus-wide cost as a PER-ROW
FORMULA (+8 B × consumer-type boxes per op), never as a single verdict.
⚠ **Three more from the same ladder's tail (2026-09-05).** **An alloc census table carries ns/op
BESIDE obj/op and B/op** — it is the only column that separates a real zero from an ELIDED call
(a 9 ns control against a 2,516 ns `DeepEqual(slice,slice)`) — and, per the instrument-population rule
above, **an allocation counter charging golib's own sites is structurally BLIND to CLR boxing**, so a
"floor of N boxes" stays a PROOF CLAIM until a byte-arithmetic instrument against a hand-boxed control
measures it; two instruments in two PROCESSES agreeing to the object (probe 3 obj against suite 3 obj)
is what makes a census the reading of record. **Two INDEPENDENT derivations agreeing to the byte AND
the object LOCATE a residue without a third instrument** — a per-frame byte probe taken at an earlier
base and a later cut's own measured total closing at 120+88+120+56+104 = 488 B and 2+2+2 = 6 objects —
so the re-probe is OFFERED, not owed. And **a count-based bank condition (objects = 0) is served ONLY
by the candidates that remove OBJECTS**: a bytes-only residue (pins) is named as its own increment and
never stands between the row and its condition.

Converter internals (full taxonomy in [`docs/Architecture.md`](docs/Architecture.md)):
- Entry: `src/go2cs/main.go`. Stdlib driver: `src/go2cs/stdLibConverter.go` (builds the package
  dependency graph + topological `sortedQueue`).
- `visit*.go` — walk AST nodes → C# declarations/statements (e.g. `visitFuncDecl.go`, `visitRangeStmt.go`,
  `visitDeferStmt.go`, `visitSelectStmt.go`).
- `conv*.go` — convert expressions/types (e.g. `convCallExpr.go`, `convSliceExpr.go`, `convStarExpr.go`).
- Analysis passes: `escapeAnalysisOperations.go`, `variableAnalysisOperations.go` (shadowing),
  `nameCollisionAnalysisOperations.go`, `constraintOperations.go` (generics), `importOperations.go`.

## One tree (read this before touching `src/core`)

`src/core` is **the** go2cs standard library. Everything — behavioral tests, examples, the tour, the
Phase-4 test pipeline, NuGet — binds `$(go2csPath)core\<pkg>`, which is the exact reference the converter
has always emitted. There is no second tree and no path rewriting anywhere.

What lives under it:

1. **Converted packages** (`src/core/<pkg>`) — `go2cs -stdlib` output, regenerable wholesale. Nothing is
   hand-edited here long-term; fixes belong in the converter, in `golib`, or in a declared hand-own.
2. **Hand-owned packages** — `src/core/unsafe` and `src/core/testing`. Both are skip-listed in the
   conversion queue (`isNonConvertedStdLibPackage`, `stdLibConverter.go`) and both are recovered into the
   generated solution from their dependents' references, so they publish to NuGet like any other package.
   `unsafe` is a compiler intrinsic; `testing` is the Phase-4 test host, and hand-owning it is what makes
   F15b's "ONE testing package, period" structural instead of a remap. `testing`'s **subpackages**
   (`fstest`, `iotest`, `quick`, `slogtest`, `internal/testdeps`) are ordinary converted packages.
3. **Hand-owned FILES inside converted packages** — the `[module: GoManualConversion]` whole-file
   replacements and the `*_impl.cs` companions. A reconvert leaves the
   marked `.cs` alone and drops a `<name>.cs.auto` review sibling beside it.
4. **`golib`** (`src/core/golib/`) — the hand-written runtime, shared by everything, never auto-generated.
   `src/core/go2cs` (the `Symbols.cs` shared project) sits beside it.

**History (why this section used to say something else).** The hand-finished baseline lived at
`src/gocore/` (2020–2025), was renamed to `src/core/` on 2025-03-08 (`ba6fef6c9`), then **overwritten in
place** by the first full-stdlib conversion on 2025-05-05 (`6ca1c45b7`, +508k lines) — which stalled the
loop, because "conversion succeeded" there meant the transpiler didn't crash, not that the C# compiled.
The 2026-06-25 repair relocated that conversion to `src/go-src-converted/` and restored the stub into
`src/core`, giving a green baseline immediately and a **two-tree** doctrine: never reference both, because
both emit `namespace go` with `<pkg>_package` partial classes and would collide. That doctrine held for
six weeks and cost a rewrite pass on every csproj, two exact-path exceptions in the overlay, an inverse
rewrite in `deploy-core`, and a `-tests` remap. Its premise expired at Phase 3 — the corpus compiles under
a standing gate and 69 packages validate — so on **2026-08-01** the conversion moved home to `src/core`,
the stub retired, and all of that machinery was deleted rather than re-pointed. There is still only ever
ONE stdlib in a build; there is now only one on disk.

## Build / test workflow

- **Converter (Go):** built with the Go toolchain from `src/go2cs/`. Usage:
  `go2cs [options] <input_dir> [output_dir]`. Key flags (from `main.go`, authoritative):
  - `-stdlib` — convert the Go stdlib. `-stdlib fmt strings io` — convert only those packages (+filter).
  - `-recurse` — recursively convert an end-user module + its third-party deps (references the pre-converted
    stdlib via local `$(go2csPath)` project refs). A second positional output root isolates the generated
    `src\` app + `pkg\` dependency trees from that runtime root; converted packages reference one another
    relatively. Without it, recurse output defaults to `-go2cspath` for backward compatibility.
    `-recurse=module` narrows the SCOPE to the input module's own packages: every third-party package is
    still referenced into `pkg\<import-path>` but none is converted, so a dependency closure go2cs cannot
    convert can't hold up the module's own code (issue #32). Values compose — `-recurse=module,nuget`.
    A local-refs recurse conversion pins `$(go2csPath)` to the resolved runtime root in the output root's
    generated `Directory.Build.props` (condition-guarded default; relative `$(MSBuildThisFileDirectory)`
    form when the roots coincide, absolute otherwise) — before that pin an isolated output root fell back
    to the csproj template's `$(USERPROFILE)/go2cs/` default and no stdlib reference resolved (issue #36).
    `-recurse=nuget` instead emits NuGet PackageReferences
    (`go.<pkg>`/`go.lib`/`go.gen`, versioned `$(GoStdLibVersion)`) for the go2cs stdlib/runtime/analyzer so a
    converted app restores from nuget.org with no `deploy-core` staging; the app's own converted packages
    stay relative project refs, and the converter emits an output-root `Directory.Build.props` with a
    floating `GoStdLibVersion` default.
  - `-tests` — also convert the package's eligible `_test.go` suite + emit a runnable test-host project
    (default off; mutually exclusive with `-recurse` — `log.Fatal` on both). Forces `-comments` on (test
    conversions are derivative works), resolves the output path absolute, and self-locates `$(go2csPath)` by
    walking the output dir up to the first root containing `core/golib` — so the canonical two-argument form
    `go2cs -tests -test-action all <goroot-pkg-dir> <converted-pkg-dir>` needs no flags or env from a clone.
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
    survive).
  - `-test-action convert|build|run|compare|all` (default `convert`) — `convert`/`all` convert-and-hook
    (production sources then tests); `build`/`run`/`compare` act on EXISTING digest-validated artifacts
    without reconverting; `compare` (and `all`) diffs the C# host's terminal results vs `go test -json -count=1`.
  - `-test-timeout <dur>` — the **package deadline** for a converted-test action (build/run/compare);
    Go duration syntax, default `2m`, must be > 0. For `run`/`compare` it is handed to **both** sides
    (`go test -timeout` and the converted host's own `-timeout`) so they agree, and the child process
    is killed one minute later purely as a safety net. Before that threading each side fell back to
    its OWN 10-minute default, so **no** value of the flag could let a slower-than-Go suite finish —
    `hash/maphash` self-terminated at exactly 600 s under `-test-timeout 40m` and reported its
    still-running `TestSmhasherAvalanche` as an empty verdict that reads like a real failure. A suite
    whose C# run legitimately exceeds 10 min needs an explicit value (maphash: `-test-timeout 30m`,
    ~15 min in C# vs 7.6 s in Go — a performance gap, not a correctness one).
    ⚠ **The 2m default is FIVE TIMES SMALLER than the sweep's, so a hand-invoked `-tests` run fails
    where `run-validated-sweep.ps1` passes — on nothing but which default applied** (measured
    2026-08-24: `bytes` reported `Go="pass" C#=""` on 38 tests with ZERO reported, which is the exact
    signature the orphaned-`dotnet run` file lock produces and reads as total conversion failure; the
    sweep's `-TestTimeout` default is `10m`, and at that value the same tree validated 82/82, exit 0).
    **The tell is the SHAPE of the empty set, and it generalizes to any mass-empty comparison:** a
    contiguous **alphabetical tail** is a run that died partway (deadline or crash) because the host
    reports in sorted order; **scattered** empties are genuine divergence; **ALL** empty is the
    documented file-lock case. Check the ordering before believing the diagnosis — and pass an
    explicit `-test-timeout 10m` on any hand-invoked row so the default is never the variable.
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
    ⚠ And the tail read has its own false-empty: the event can be carried as an ESCAPED JSON
    string, so a substring count of `"action":"timeout"` returned **0** on a record whose tail states
    the kill (2026-09-02) — match the escaped form too, or parse the field.
    ⚠ A new tail-stated member (2026-09-02, the `chanDir` arm): 388 divergences, every verdict `C#=""`,
    stream 0/0/0 — reading like a corpus-wide regression from the lane's own cut — with the tail saying
    `exit status 0xc0000142` (STATUS_DLL_INIT_FAILED), a TORN `bin`/publish tree from an interrupted
    run. Delete the publish dir and re-run; `0xc0000142` in the tail is a cleanup, never a finding.
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
    ⚠ **Three record-file rules, all measured 2026-09-02.** (1) A **gated** (`-test-filter`) run
    REWRITES the package's comparison record with nothing marking it gated — a harvest read
    `runtime/debug` as bankable off a filtered control's record, the only tell being 9 go entries where
    the full run had 10 — so after any gated diagnostic that record is poisoned for banking until an
    UNGATED run overwrites it. (2) A paired before/after measurement needs two FILES, not two runs: the
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
    ⚠ **And what that preserved record answers, which the log cannot** (2026-09-02): a crashed host's
    EXIT CODE is nowhere in the sweep log's printed FAIL block (the stream's last three JSON events) —
    it is inside the comparison record's oracle-side error text (`child error exit status 0xc0000005`,
    with the child's stderr quoted). Two neighbours: the `-tests` pipeline is **SILENT on success**
    (nothing on stdout or stderr, exit 0), so a run's evidence is its ARTIFACTS — the `*_test.cs`, host
    and csproj files — never its exit code; and a diagnostic patch applied to a BANK host's tree is
    restored, and its records deleted, before anything banks from that host.
    ⚠ **And the filtered-status trap has a SEARCH costume** (2026-09-02): `find … | head` filled ten
    lines with unrelated paths and the truncated view was read as the ABSENCE of a preserved record
    that was sitting there. An unfiltered enumeration answers "is it there"; a head-limited one
    answers a different question — the same split as filtered vs unfiltered `git status --porcelain`.
    ⚠ **A FAILED `-tests` BUILD leaves the PREVIOUS comparison record in place** — the family's
    nastiest member, paid three times by one lane (2026-08/09). The pipeline rewrites
    `go2cs_test_comparison.json` only when a run completes, so a fix whose build DIED
    (e.g. a hand-own registered under a bare name where Go declares a method — key `"Type.method"`
    — displacing nothing and duplicating into CS0111) re-reads the OLD record and reports the OLD
    failures: it reads exactly like "the fix does not work", or worse, like a stable count. Before
    believing any post-fix count, verify the build the record claims actually succeeded and the
    record is newer than the edit; for the registry case, checking the placeholder was actually
    emitted is the cheap tell. The record is only the verdict when the run that wrote it completed.
    ⚠ **THREE host-death signatures, and the third is MARKERLESS** (measured 2026-09-03): (1) a
    goroutine panic writes `died on an unrecovered panic in a goroutine`; (2) a package deadline
    writes `"action":"timeout"` into the RESULTS file — **not the log**, so a per-slice kill check
    that greps the log reads a deadline as an unexplained short count; (3) a **.NET exception from an
    unimplemented linkname stub thrown inside `Goroutine.Run` stops the results stream mid-test with
    NO event at all** — the mass-empty family's silent member, one stub costing every test after it.
    (The `runtime.Stack(all)` host-killer is a FAMILY of six reached through helpers referenced as
    TABLE VALUES, invisible to a call-graph grep; a derivation with a positive control — one member
    watched killing a slice — is what made the six trustworthy after two confident wrong sets.)
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
    ⚠ **Three record-reading rules from the same week.** A comparison record's **"differing" count
    still COUNTS disclosed rows** — a disclosure is a row that differs by design — so a tail quoted as
    76 was 16 UNDISCLOSED (4.75x inflation on every sizing sentence it appeared in); every tail census
    prints **differing / disclosed / UNDISCLOSED** as three figures. A regression's row set is read
    from the preserved record's own `disclosed` and `errors` ARRAYS before a cause is assigned: two of
    four "regressed" rows were standing disclosures measured four days earlier, and a fix justified by
    them would have rested on a premise the record falsifies. And a **mass-empty member one row wide**:
    a `C#=""` beside a SUBTEST NAME Go never produced (`[][]uint8#01` — `t.Run`'s dedup of two types
    that render to one string) is two sides running DIFFERENT-NAMED subtests, not an empty verdict.
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
    ⚠ **An instrumented `-tests` probe reads ZERO because `-test-action all`/`compare` RE-CONVERTS
    every non-marked corpus file before building** (measured 2026-09-03) — the marker was wiped
    between the edit and the binary, the same mechanism that silently reverted a hand-own prototype
    hours earlier. Instrument in the sequence `convert` → **edit** → `compare`, and `grep -c MARKER
    <file>` immediately AFTER the run is the one-command tell. (The probe's own readings VARYING
    across the population were what made its non-zero answer trustworthy — a probe whose output is
    constant is its own false-empty.) ⚠ And the gated/direct freshness split narrows again: a gated
    PIPELINE run ALSO reproduced the stale `results.json` beside a fresh comparison, so the mechanism
    stays unrooted and **the freshness check — the results file's timestamp against the
    comparison's — is the rule**, not the invocation path.
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
    ⚠ **Three tail-reading rules, 2026-09-06.** **A row's NUMBER can be produced by two mutually
    exclusive mechanisms, and the tail separates them in one command**: 19 of 20 is the same count
    whether the assertion FAILS (the bytes are retained and Go's own `t.Fatal` fires) or the row HANGS
    (the bytes are collected, the finalizer blocks, the collection call never returns and the package
    deadline kills it) — so read the tail BEFORE specifying an instrument, because a hang and a failure
    are not the same question and only one of them is disclosable. **The tail rule is right and its PATH
    is not universal**: for a row whose record is the FLAT root comparison file holding the full stream,
    a tail read scripted against a subdirectory path reports that the run did not get that far — on a run
    that completed perfectly, which is the worst false reading that rule can produce — so a tail read
    looks for BOTH shapes and SAYS WHICH IT FOUND. And **a blocker that moves one deeper with an
    UNCHANGED count is a RESULT**: a host dying on a different symbol in each arm, both fatal, reads as
    an identical verdict count while the wall moved — and what it moved to was the one destination
    neither registry could serve, which turned a declined widening into a measured item on a row's
    critical path. Compare the SYMBOL, not only the count.
  - `-convert-timeout <dur>` — the `-stdlib` driver's cap on ONE package's conversion; Go duration
    syntax, default **10m**, must be > 0 (`log.Fatal` otherwise). It is a **safety net against a hung
    conversion, never a performance assumption**: the value has to clear the slowest legitimate
    package on the slowest legitimate host, because a killed-but-healthy conversion is reported as a
    FAILED package — named in the log, counted in the summary, listed in `failed_packages.txt` — and
    reads exactly like a converter defect. It was hard-coded at 10m until 2026-09-02, when concurrent
    lane load on the i7 class pushed one package past it mid two-seeded A/B, which would have banked a
    whole package as a spurious emission difference. The fired message names the package, the elapsed
    budget and this flag, so raise it there (`-convert-timeout 90m`) rather than editing a constant —
    and pass the SAME value to both binaries of an A/B, since the cap is part of what a run measures.
  - `-go2cspath <dir>` — runtime/stdlib root and default output root for converted code (default `~/go2cs`;
    env `GO2CSPATH`). `go2cs -recurse <input> <output>` keeps generated code under the explicit output root
    while `$(go2csPath)` references continue to resolve against this runtime root. **It is also the root the
    converter reads each imported package's `package_info.cs` from** to mint the emitted
    `<ImportedTypeAliases>` block, so a stale/missing root used to emit a silently EMPTY block — no warning,
    exit 0 — and the OUTPUT varied with the shell's ambient `GO2CSPATH` (found 2026-08-06). Two protections
    since: **self-location** — any single-package or `-tests` conversion whose configured root is not a go2cs
    root (no `core\golib\golib.csproj`) walks its OUTPUT path's ancestors for one, so a bare
    `go2cs <pkg-dir>` inside a clone resolves against that clone with no flag or env; and a **loud
    once-per-run stderr warning** naming the resolved path and the consequence when none is found
    (deliberately NOT fatal — converting standalone code with no deployed root is legitimate). An explicitly
    configured *working* root always wins.
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
    `-recurse` warns but never self-locates (without a second
    positional its root doubles as the output root, so moving it would move the generated tree);
    `-recurse=nuget` does neither (published package refs need no local root); `-stdlib` does neither (its
    root IS the output root the run itself populates, so an absent `golib` is the normal first-conversion
    state). Every harness that invokes the converter — `check-no-regression.ps1`, `BehavioralRunner`,
    `BehavioralTestBase`, `PerformanceRunner`, `run-validated-sweep.ps1` — now passes an EXPLICIT
    `-go2cspath <repo>\src` computed from its own location, so no gate's verdict can move with the ambient
    variable again.
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
  - `-platforms os/arch` — the ONE target a conversion emits for (default: the host). It also accepts a
    comma-separated **list** (`-platforms windows/amd64,linux/amd64,darwin/amd64`). With `-stdlib` a list
    now performs the multi-platform **EMISSION** (`platformEmit.go`): it converts once per target into a
    seeded staging root (`-platform-stage <dir>`) and MERGES the emissions into the `-go2cspath` corpus as
    layout L3 — shared files flat, platform-varying ones in per-GOOS folders, hand-owns routed to their
    principal's platform set. ~560 s for three targets (measured r51b). `-platform-census` remains the
    READ-ONLY instrument over the same staging (a manifest, no corpus output). A list without `-stdlib`
    is rejected rather than silently converting the first target.
  - `-platform-census <dir>` — the **multi-platform emission census** (increment 1, landed 2026-08-08).
    With `-stdlib` and ≥2 `-platforms` targets it converts once per target into `<dir>\<goos>-<goarch>\src`
    — each staging root SEEDED from `-go2cspath` per the reconvert ritual below, wiped and re-seeded per
    run so the r41 "never convert twice into one root" rule is mechanical rather than remembered — then
    classifies every emitted artifact (shared / variant / partial / exclusive) and writes
    `<dir>\platform-manifest.json`. It writes **nothing** into the corpus: `-go2cspath` is read as the seed
    and never as an output. ⚠ In any multi-target staging comparison, "differs" means NOTHING until
    you know which side was actually WRITTEN: a single-target conversion re-emits only its own
    target's per-GOOS files, so a per-PATH diff across staging roots reports fresh-vs-seeded pairs
    as differences (a confounded census nearly banked 60 false hits, 2026-09-01) — compare only
    paths BOTH conversions write, or classify by write-evidence first.
    ⚠ Its MIRROR, measured 2026-09-02: **IDENTICAL means nothing when the side was not WRITTEN
    either.** A windows-default single-target reconvert reported ZERO diff on an L3 package's
    `linux/` files — the very files another lane had measured, under a linux-target conversion, as
    carrying four missing forced-init hooks. Classify by write-evidence PER TARGET, and measure an L3
    package with the three-target `-platforms` emission rather than the host default.
    Emitted-vs-seeded is decided by a sentinel modification time, not by content,
    because the control target's emission is *supposed* to reproduce the seed byte for byte. The manifest
    carries the marker gate per target (hand-owned files the seed held, and any the run emitted as a plain
    `.cs` — must be zero) so a failed seeding cannot be mistaken for a platform finding.
  - `-goroot` / `-gopath`, `-indent 4`, `-var` (default on),
    `-uco` (channel operators, default on), `-comments`, `-cgo`, `-tree`, `-csproj <tmpl>`, `-debug`.
  - Single project/file: `go2cs package_dir` or `go2cs example.go [out.cs]`.
  - **Always pass `-comments` when converting the Go stdlib.** It defaults **off**, but the converted C#
    is a derivative work: the per-file `// Copyright … The Go Authors … BSD-style license` header **must be
    preserved** (license requirement), and the Go doc-comments are what make the output readable. Without
    it the header and all comments are stripped. (Behavioral-test goldens were captured *without* comments,
    so don't flip the default — pass the flag on stdlib `-stdlib` runs.)
- **Converted C# projects:** standard `dotnet build` (target **net10.0**, C# latest). Each converted
  `.csproj` references `golib`, the `go2cs-gen` analyzer, and the stdlib packages it imports. The
  `$(go2csPath)` MSBuild property resolves to `$(SolutionDir)` in Debug builds (so refs point at
  `src/core/...`); it is **distinct** from the converter's `-go2cspath` output flag.
- **Behavioral tests** (`src/tests/Behavioral/`): each test references `golib` + the `go2cs-gen` analyzer;
  most also reference `core/fmt` (a few reference `time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`/
  `reflect`). Since 2026-08-01 those references bind the **converted** packages, so the suite's 515
  stdout comparisons against `go run` are also the broadest running validation the converted `fmt` gets —
  its closure is 57 projects (cold ~48 s, warm ~4 s).
  The `BehavioralTests` MSTest runner has these phases: `TranspileTests`, `CompileTests`,
  `OutputComparisonTests` (runs Go vs C#, compares stdout), `TargetComparisonTests` (byte-compares the
  transpiled `.cs` against a `.cs.target` golden).

### Test-harness mechanics (important when changing the converter)
- **`dotnet build` does NOT run the converter** — it only compiles committed C#. A clean build leaves the
  tree clean. **Running the tests re-runs the converter:** `BehavioralTestBase` rebuilds `go2cs.exe` via
  `go build` whenever any converter `*.go` is newer than the binary, then re-transpiles. So after a
  converter change, running the suite regenerates the behavioral `.cs` from current source (and may show
  them as modified in git — that's expected).
- **A hand-invoked `-stdlib` or `-tests` run REFUSES a stale binary — route #1 closed from inside the
  converter, where those two paths have no caller to instrument.** `go2cs` compares its own executable's
  mtime against every build input in the source tree beside it (`converterStaleness.go`, over the set
  `ConverterBuildInputs.cs` defines) and, when any is newer, ENUMERATES the extent: the count, the ten
  newest paths (all of them at ten or fewer), and which are emission-affecting — everything except a
  `_test.go`, which `go build` excludes from the binary. For those two drivers, whose output is banked
  or measured, it then exits non-zero; **`-allow-stale-converter`** proceeds deliberately and is what an
  A/B against a PRESERVED binary passes, so a stale run says so in its own command line. Every other
  shape — a single file or package, `-recurse` — keeps the warning and runs, because that is the
  scratch-probe loop and a pinned binary there is ordinary. Two limits to know: it is silent when no
  converter source tree sits beside the executable (a deployed binary), and it is blind to a TOOLCHAIN
  hop, which stays route #4's embedded-stamp comparison in the harness predicate.
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
- **FALSE-GREEN route #2 — stale OUTPUT (fixed 2026-07-20).** Distinct from the stale-`go2cs.exe` trap
  (route #1, where an un-rebuilt binary runs old logic): here the exe IS current but the runners *skip
  transpiling* and validate the **previous** converter's `.cs`. All three of `BehavioralRunner.UpToDate`,
  `PerformanceRunner.UpToDate`, and MSTest `BehavioralTestBase.TranspileProject` short-circuited on a
  `.cs`-newer-than-`.go` check alone. Converter work is exactly the case where the `.go` files *don't*
  change, so every project stayed "up to date", transpile was skipped for all of them, and Target/Output
  then compared the old converter's output against goldens that same converter had generated — everything
  matched and the suite printed **PASS**. A guard test "validated" that way guards nothing. All three now
  also require the `.cs` to be newer than **`go2cs.exe`**, so any converter rebuild invalidates the whole
  corpus. Verified by neutering a real converter fix (`lhsReusedInLaterRhs`) and rebuilding: the old
  runner reported PASS, the fixed runner reports `FAIL [Target,Output]` with no manual touch.
  ⚠ **Route #2 has a door NO BINARY opens — a `git checkout --` of the behavioral tree** (measured
  2026-09-03). The restore stamps every `.cs` with a fresh mtime NEWER than `go2cs.exe`, so the
  up-to-date predicate (`.cs` newer than `.go` *and* than the binary) is satisfied by HEAD's OWN
  committed emission: transpile is skipped, `--update-targets` copies that `.cs` over its golden as a
  no-op and reports `Updated`, and the four-phase run behind it validates the OLD emission and
  passes — a golden re-baseline that re-baselined nothing. The tell is arithmetic (an EMPTY numstat
  where CNR had just read `1 1`); the remedy is CNR's own — rebuild the converter first so every
  project is stale again (a re-baseline path that transpiles unconditionally is queued). **A
  re-baseline is believed only after its diff is non-empty and its golden byte-compares against the
  on-disk emission.**
  ⚠ **Corrected 2026-09-04 on the MECHANISM, the door itself standing:** a WHOLE-TREE `git checkout`
  writes the `.cs` and the `.go` within the same instant, so under a strict mtime comparison it does
  NOT reliably leave the up-to-date relation over foreign content — what does is a `.cs`-ONLY restore,
  a `Copy-Item`, or an editor save. The comments in the code say the MEASURED shape, not the plausible
  one; the remedy (transpile unconditionally) is unchanged either way, since it does not depend on
  which restore stamped what.
- **`check-no-regression.ps1` re-transpiles UNCONDITIONALLY** (it has no `UpToDate` equivalent), which is
  why CNR was immune to both false-green routes and remains the authoritative drift instrument for
  converter changes. Preserve that asymmetry: never add an up-to-date skip to CNR.
- **A new converter `.go` file must be registered in `src/go2cs/go2cs-src.projitems`** — the VS
  shared-project item list `go2cs-src.shproj` imports (and that shproj is a member of `go2cs.slnx`).
  Nothing *builds* from it (`go build` walks the directory), so a missing entry is invisible at the
  command line and only bites in Visual Studio, where the unlisted source is absent from Solution
  Explorer. It had drifted silently until 2026-08-06. **`projitemsIntegrity_test.go`** now gates it
  both ways — every `*.go` on disk (including `internal\*`) is registered, every registered path
  exists — under the plain `go test ./...` run from `src/go2cs`, so no new harness and nothing to
  remember; a failure prints the exact `<None Include=… />` line and the entry it goes after. (Same
  invariant `tests/Behavioral/check-solution-integrity.ps1` applies to `go2cs.slnx`.) The file is
  UTF-8 **with BOM** and its line endings are uniform — a third guard holds both, so edit it in place
  or via `[System.IO.File]::ReadAllText/WriteAllText`, never PS 5.1 `Get-Content`/`Out-File`.
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
- **FALSE-GREEN route #3 — NESTED sub-library packages were never enumerated (fixed 2026-08-02).** All
  three transpile gates walked `tests\Behavioral\*` **top-level only**, so the 22 sub-library packages
  nested inside a test folder (`IoLike\FsLike`, `VersionedImport\vlib`, `CrossPackageArrayZeroValue\bufpkg`,
  `GoNamespaceShadow\nsshadowlib`, …) were transpiled by **no gate at all**. Two consequences, the second
  the dangerous one: (1) their committed `.cs` froze at whatever converter last touched them by hand — 17
  files across 13 packages had drifted by 2026-08-02, spanning three separate increments (the
  `// <TypeAccessibility>` block, string-literal hoisting, the compound-assign result cast); (2) a
  sub-library's `package_info.cs` is an **INPUT** to its parent's transpile — the parent reads the
  sibling's `[assembly: GoImplement]` records to decide whether to mint a local `ᴠ` value adapter — so a
  regression in that area could not make the parent's golden fail, because the parent kept reading the
  stale-but-plausible records. That silently disarmed the `ForeignValueImplementSuppression`,
  `ValueAdapterDynamicType` and `SamePackageImplementNoWitness` guards. All three gates now walk
  **recursively, DEEPEST-FIRST** (`GoPackageDirs` in `BehavioralRunner`/`BehavioralTestBase`; a recursive
  `Get-ChildItem` + depth sort in CNR) so a sub-library is regenerated before its parent consumes it, and
  `UpToDate` in both runners considers the nested packages too. Enumeration is now **570 packages**
  (545 top-level + 25 nested; it was 543 = 521 + 22 when this note was written), not 521. Note goldens remain top-level-only: nested packages have no
  `.cs.target` (`UpdateTestTargets` is deliberately unchanged), so nested drift is caught by CNR's
  `git status`, while the *cross-package* effect is caught by the parent's golden.
- **FALSE-GREEN route #4 — a TOOLCHAIN hop did not invalidate `go2cs.exe` (CLOSED 2026-08-24, H1.4).** A
  fresh instance of route #1's stale-binary trap that route #1's mitigation does not cover. Every rebuild
  predicate — `BehavioralTestBase`, `BehavioralRunner`, `PerformanceRunner` — rebuilds the converter when
  a converter **`*.go` file** is newer than the binary. Installing a new Go toolchain touches **none** of
  them, so after a hop every predicate still says "up to date" and every gate keeps running a binary that
  embeds the OLD release's `go/parser` + `go/types` front end (`conversionDriver.go` uses
  `packages.LoadAllSyntax`, i.e. the converter's OWN compiled-in type-checker) against the NEW release's
  sources. It does not fail cleanly: the old parser mis-parses or rejects the new constructs and the run
  degrades into the converter's best-effort "did not fully type-check" path — which CNR reports as **NOT
  MEASURED** (good) but the runners do not. **The remedy landed (H1.4, 2026-08-24) and came out smaller
  than planned: nothing needed stamping, because every Go binary ALREADY embeds its toolchain release and
  `go version <exe>` reads it back.** So the whole fix is one compare, in the ONE shared helper all three
  predicates already delegate to since route #5 — `src/tests/ConverterBuildInputs.IsConverterStale` — which
  fails stale-wards (unreadable stamp or unanswerable GOVERSION forces the rebuild) and is guarded by
  `TestConverterStalenessConsultsTheToolchain`. **No explicit `go build` is owed after a toolchain change
  any more; the predicates rebuild on mismatch exactly as on an mtime change.**
- **FALSE-GREEN route #5 — a converter build INPUT that is not a top-level `*.go` file invalidated
  `go2cs.exe` NOWHERE (found 2026-08-21 by the hop-campaign planning read; fixed 2026-08-22).** The
  third instance of route #1's stale-binary trap, and the one with the widest trigger. All three
  rebuild predicates — `BehavioralRunner` (`Program.cs`), MSTest `BehavioralTestBase`,
  `PerformanceRunner` (`Program.cs`) — asked whether any **top-level** `*.go` in `src\go2cs` was newer
  than the binary. The converter is built from more than that, and each omission changes what it
  **emits** while touching no top-level `.go` file at all: (a) the `//go:embed` assets —
  `embeddedTemplates.go` embeds both csproj templates, the `package_info.cs` skeleton, the icons and
  `profiles/*`, and `stdlibMetadata.go` embeds `stdlib-metadata.txt`; (b) the `internal\` packages the
  converter imports (`internal\stdlibmeta` and siblings), which a top-level walk never saw either; (c)
  `go.mod`/`go.sum`. Measured at the fix: **204 top-level `*.go` seen, 224 real inputs — 20 invisible.**
  Edit one and every predicate reports "up to date", the OLD binary keeps running, and every runner gate
  validates the PREVIOUS emission and prints PASS. The edit reads as a no-op, which is
  indistinguishable from "the change was already correct" — and a **.NET migration's TFM stage edits
  exactly those templates and profiles** (`docs/DotNetMigration.md` §5.2), which makes it the step in
  the project most likely to meet this route. Route #4's `runtime.Version()` stamp does not cover it: a
  stamp says nothing about a template's modification time. **Remedy (landed):**
  `src\tests\ConverterBuildInputs.cs` — one definition of the converter's build-input set, LINKED into
  all three projects (the two runners take no assembly dependency, so a shared assembly is not
  available), with the embedded half **DERIVED from the `//go:embed` directives themselves** rather
  than listed, so a directive added tomorrow is covered the day it is written. Two guards under the
  plain converter `go test ./...` (`embeddedAssets_test.go`): the directive **forms** stay inside the
  subset the C# resolver understands, and the three predicates still delegate to the shared helper.
  **`check-no-regression.ps1` was never exposed** — it has no rebuild predicate at all, it runs
  `go build` unconditionally, and `go build`'s cache is content-addressed over embedded assets
  (A/B-verified: editing `csproj-template.xml` changes the linked binary's hash, reverting reproduces
  it byte-for-byte). That is the same asymmetry that made CNR immune to routes #2 and #4 — preserve it.
  ⚠ One caveat on the second guard: cmd/go's test cache **drops files that resolve outside the module
  root** (`computeTestInputsID`, "Do not recheck files outside the module, GOPATH, or GOROOT root"), and
  the three predicate sources live under `src\tests`, outside `src\go2cs`. A narrowed predicate therefore
  reports `ok (cached)` and only fails under **`-count=1`** — so a change touching ONLY harness C# owes
  `go test -count=1 ./...`. The first guard has no such gap (every input it reads is inside the module).
- **FALSE-GREEN route #6 — an instrument that cannot find its own runner reports SUCCESS (found
  2026-08-24 by two lanes from opposite directions; closed the same day).** A different SHAPE from
  #1–#5: those all run a gate and measure the WRONG thing — a stale binary, stale output, an
  unenumerated package, an old front end — so each yields a verdict that is merely untrue. This one
  measures **nothing** and prints a pass over the hole. `src\_paths.ps1` spelled the corpus TFM as a
  **literal** (`$NetVersion = 'net9.0'`), the TFM census's Class-D hoist that had gathered nine
  hardcoded sites out of six files into that one line. Hoisting fixes the SPREAD, not the KIND: a
  hoisted literal is still a literal, and on a `net10.0` tree every consumer composes
  `bin/Debug/net9.0/`, which does not exist. `run-behavioral.ps1` fails loudly; **`run-performance.ps1`
  died in 20 seconds having run nothing**, and the only tell was the implausible speed — a full perf
  run is HOURS on this machine class. **No existing gate can see it**, because each wrapper's only
  preflight is the `dotnet build` exit code and **the build is genuinely green** (it writes to the TFM
  the projects declare); the runner that would have counted anything is never reached, so there is no
  phase to fail, no project list to come up short, and nothing to compare against a golden. Both
  halves are closed. **(a) `$NetVersion` is DERIVED** from the property of record —
  `src\Directory.Build.props`'s `<TargetFramework>` element, read by one file-read-plus-regex with no
  MSBuild and no `dotnet` (this module is dot-sourced by every instrument on every invocation),
  comments stripped so the props file's own prose cannot be read as the property, and **no fallback to
  a literal**: an instrument that cannot know its TFM throws, naming the file. Replacing the literal
  with `net10.0` is the tempting fix and the wrong one — it re-breaks at the next hop, which is what
  `docs/DotNetMigration.md`'s *derivation, not replacement* means. It also makes `migrate-tfm.ps1`
  honest: that instrument carries no site for `_paths.ps1` because its census already believed the
  PowerShell probe derived. **(b) Every wrapper that launches a runner asserts the executable EXISTS**
  before invoking it, and exits **non-zero** naming the expected path when it does not
  (`run-behavioral.ps1`, `run-performance.ps1`, and `run-performance-floor.ps1`'s bflat arm — which
  runs at `'Continue'`, so a missing compiler would otherwise report `ok` off a stale `$LASTEXITCODE`).
  Derivation removes today's trigger; the guards close the class, since any future cause of a missing
  runner is now loud. ⚠ The guards use an explicit `exit 1` rather than `throw` because the exit CODE
  is the property that matters and a `throw` leaves it to the host: on Windows PowerShell 5.1 the
  missing-runner path already exits 1 (measured, both wrappers, `-File` and `-Command`), so the
  exit-**0** sighting is a host- or wrapper-dependent swallow — which is the argument for stating the
  code rather than inheriting it.
- **FALSE-GREEN route #7 — a `go2cs-gen` (analyzer) change is invisible to EVERY standing gate except
  a behavioral COMPILE (found 2026-08-30, the W3a promoted-forwarder regression; fixed `0df5a3f2b`).**
  CNR is transpile-only, so generator output never enters its verdict; the stdlib solution compiles
  one assembly at a time, so an accessibility demotion that breaks only CROSS-assembly consumers
  stays green there (`internal` binds fine same-assembly); and the corpus 307/0 + CNR-byte-identical
  ladder a converter arc normally runs therefore proves NOTHING about gen changes. The W3 merge
  demoted net's public `TCPConn.Read/Write` promoted forwarders to internal and shipped green on
  exactly that ladder; the escape was caught days later by a derived net/http canary sweep — the only
  union gate that compiles a cross-assembly consumer of metadata-promoted surface. **Rule: any change
  under `src/gen/` owes a full behavioral COMPILE phase (slnx-dev build or the runner's Compile) and
  at least one cross-assembly consumer gate before banking.** Corollary paid the same night: ONE red
  behavioral project collapses the full-suite verdict into 651-suspect attribution (the Transpile
  phase rewrites every `.cs` first, so no assembly is up-to-date and the batch-build failure
  attributes everywhere) — measured: exactly 1 Release assembly written corpus-wide vs a clean
  78-project filtered batch. "651 suspects" means "one project is red", not "the corpus is broken".
- **⚠ An UNBANKED package's `-tests` assembly is in NO standing gate — route #7's shape, one
  assembly over (found 2026-09-01 by a lane's own sweep, by no gate).** CNR is transpile-only and
  the stdlib solution compiles PRODUCTION assemblies, so nothing at master ever builds the test
  emission of a package that has not banked: `reflect`'s `-tests` assembly sat compile-broken at
  master after a widened lift dedup bound a PUBLIC lifted struct's member shape to an INTERNAL
  prior lift — the dedup crossing ACCESSIBILITY tiers, CS0050/51/52 — with every standing gate
  green. **Standing amendment: any converter change touching lift identity, dedup registries or
  anonymous-type naming owes a `-tests` CONVERT-then-BUILD of `reflect` at the MERGE RESULT, beside
  CNR.** ⚠ **CONVERT then BUILD — a bare `-test-action build` measures NOTHING on a box that has not
  converted the row** (measured 2026-09-04): `build` consumes an EXISTING digest-validated manifest,
  and `go2cs_test_manifest.json` is machine-specific and git-ignored, so on a clean box the run exits
  in **0 s** with `test manifest is missing` — which an error-pattern filter reads as a bare `exit=1`
  and a green-word grep reads as nothing at all. The gate is `-test-action convert` (or `all`)
  followed by the build, on every box that has not already emitted that row. The same hole has a
  second door: **the production-only two-seeded diff is blind to
  TEST-side emission** — a carrier stamp that dangled two banked rows lived in `x509_test.cs`,
  which `-stdlib` never writes, so the diff matched its prediction exactly and said nothing about
  the footprint that broke them. A converter change that emits CROSS-PACKAGE references therefore
  (a) lands its corpus footprint in the SAME train — the two-seeded diff applied verbatim,
  byte-identity asserted, exactly as a hand-own registration lands with its body — and (b) owes a
  `-tests` emission census of the banked rows it can reach, beside the `-stdlib` diff.
  ⚠ **The instrument that DOES walk every banked row's TEST emission is a roster-wide `-tests`
  reconversion, and it found what nothing else could (2026-09-02):** a BANKED row (`errors`, 61
  verdicts) whose test assembly no longer built at master — a production-registry dedup arm whose
  accessibility guard reasons within ONE assembly (`v.inFunction` short-circuits) bound the EXTERNAL
  test variant's function-local lift to the production assembly's INTERNAL lift (CS0122), bisected to
  `5442b402e`, whose own blast-radius census was a `-stdlib` two-seed diff and therefore structurally
  blind to test emission. Rules: a cross-assembly reuse is admissible only if REACHABLE — an internal
  variant, a PUBLIC candidate, **or an EXTERNAL variant whose test-project MODEL puts production's
  internals in sight** (whitebox-reference or recompile; only the plain reference model, chosen exactly
  when the package has NO internal test file and therefore gets no `InternalsVisibleTo` grant, cannot
  see them). ⚠ **The axis is the test ASSEMBLY, not the Go VARIANT — corrected 2026-09-04 by
  measurement, after this line's first form said "an internal variant, or a PUBLIC candidate" and the
  converter's predicate agreed with it.** `f38c2ae01` keyed reachability on `testExternalVariant`,
  which is right for `errors` (external-only suite, no grant) and wrong for every package that HAS an
  internal test file: there BOTH variants emit into the ONE `.tests` project, and the same fact that
  selects the whitebox model (`selectTestProjectModel`) is what makes the production csproj emit
  `InternalsVisibleTo $(AssemblyName).tests` (`insertFriendAssemblyAccess`) — so the grant is a
  CONSEQUENCE of the model and decidable without reading the csproj back. runtime paid it on EVERY
  target: `hash_test.go`'s `IfaceKey.i interface{ F() }` calls production `ifaceHash` through the
  `export_test.go` bridge, so the refusal minted a second `IfaceKey_i` and the call could not bind
  (`hash_test.cs(540,52) CS1503`), byte-identically on windows and linux. The two rules had been
  written by two arcs: `liftNameNeedsPublicType`, directly above the refusal, documents that same
  reuse as one hash_test.go "needs to compile at all". A documented invariant that a later seeding
  violates is a bug the doc cannot catch; **a lift/dedup change owes a `-tests` convert-then-build
  of a row with an EXTERNAL test variant — `errors`, the cheapest — beside `reflect`**; and a delta
  table carries build failures as their own REGRESSION column, distinct from movers. (A bisect
  converging on adjacent commits with BOTH controls valid is an attribution; the named suspects were
  exonerated by measurement, not by argument. And a bisect is not always OWED: where the green-to-red
  window holds exactly ONE commit touching the predicate, an instrumented reading names the SEAM as
  well as the commit for the price of one convert — 2026-09-04, the CS1503 above.)
  ⚠ **The `-tests` driver MIRRORS `processConversion`'s analysis sequence BY HAND** (measured
  2026-09-03), so a new converter pass wired into the `-stdlib` driver binds NOTHING under `-tests`
  until it is wired there too — self-caught by a darwin conversion emitting 0 records where 28 were
  predicted. Two hand-mirrored sequences drift: wire a new pass into ONE sequence both drivers call,
  or make its guard run under BOTH drivers.
  ⚠ **One assembly FURTHER over: a BANKED row whose TEST side carries a hand-owned `*_impl_test.cs`
  companion is compiled by NO standing gate either** (2026-09-05, `internal/reflectlite`). CNR is
  transpile-only, the stdlib solution compiles PRODUCTION assemblies, and the union sweep's roster is a
  fixed list — so an abi/golib API retirement broke that row at master for TWO trains with every gate
  green, found only by a lane's own banked-row sweep. **The class is named by the FILE SUFFIX** (one
  `git ls-tree`, re-derived per train — 2 packages at `9c44a6d6a`) **and the guard is a BUILD of each
  such row's test host at assembly**, never a grep over bridge internals, which rots with renames and
  cannot see a type-level break.
- **⚠ FALSE-GREEN route #7's BEHAVIORAL twin — a golib/reflect/gen change that alters RUNTIME
  behavior while emitting byte-identical `.cs` is invisible to CNR *and* to the `reflect` `-tests`
  build (found 2026-09-03; it shipped through TWO trains).** CNR is transpile-only and byte-identical
  by construction here; the `-tests` gate is compile-only; and a FILTERED behavioral runner never
  exercises the affected row — so nothing sees it. `ReflectArrayOf`'s identity assertion
  (`reflect.SliceOf(reflect.ArrayOf(…))` == the declared slice-of-array type) went red at the
  descriptor-cargo increment and was caught only when a darwin census flagged it as an outside-model
  movement and the coordinator reproduced it on windows. **Rule: the train battery runs the FULL
  behavioral suite — all phases, Output included — when a seat touches `golib`/`reflect`/`src/gen`;
  it is the only leg that sees a behavioral (non-compile, non-emission) regression.** Corollary: a
  full-suite PASS NUMBER quoted in a seat message is trustworthy only if the run ENUMERATED the
  affected row and was not stale-green (route #2), so a seat claiming NNN/0 Output over a tree with a
  known-red Output-compared row is reconciled before the number is believed.
  ⚠ **Its narrowest member: a banked SINGLE-VERDICT row whose mechanism lives in `golib` is guarded
  NOWHERE but that row** (2026-09-04), and where the load-bearing half is a WIRING line in a hand-own
  (a `runtime.GC()` call into a cache clear), its DELETION stays green on every standing gate. Such a
  mechanism takes a **GolibTests guard whose WIRING arm is the one that pays for the file** — the arm
  that fails when the line is removed, not the arm that restates what the library does.
  ⚠ **And a behavioral project's PASS is PER PHASE** (2026-09-04): a project without
  `[GoTestMatchingConsoleOutput]` passes Target ONLY — its emitted text byte-compared, its printed
  output never diffed against `go run` — so a golib RENDERER change guarded by a Target-only project
  has no guard at all: a battery reported `ChanElemDims PASS` with the value row genuinely RED
  (`chan [3]int` rendering as `chan []int`). **A post quoting a guard's PASS says which PHASE compared
  what**; the reader's own pre-read carried the tell (`Output: 0 compared`) and read it as vacuity
  rather than as an unmeasured red. And **a guard's header is a CLAIM that ages** — this one said an
  increment would close the row while that increment landed slices-only: correct the claim at the
  site, name the owning increment, and never turn an arm on for the strength of a comment. ⚠ One
  guard-author mechanic from the same family: in a file carrying `using static go.runtime_package` the
  bare name `GC` binds Go's `runtime.GC()` and SHADOWS `System.GC` (CS0119) — qualify it.
  ⚠ **And the tell has a CAUSE worth knowing: an Output line reading `0 compared, 0 failed, skip 1`
  is NOT a pass** (2026-09-05) — a FRESHLY transpiled `package_info.cs` lacks the hand-added
  `[GoTestMatchingConsoleOutput]` (the converter preserves it by reading the EXISTING file at the
  output path, so there is nothing to preserve where the file was regenerated from scratch), the
  Go-vs-C# comparison never runs, and Transpile/Compile still read true: route #6 in a costume. **Read
  the comparison COUNT, not the banner.**
  ⚠ **A design RULING is not reversed inside a FIX commit** (2026-09-05, the reinterpret remedy): a
  fix that made a reference-bearing box's reinterpret a GC-safe VIEW over the slot contradicted the
  seated design's loud-failure choice, and the design's own retention guards went RED on it (2 of
  651) — the guard working, the fix withdrawn. Its companion, when the row was reconsidered honestly:
  **a row whose runtime pass was a PUN surviving by luck** (a pointer-to-pointer reinterpret through a
  transient slot address) **is DEMOTED to the compile-shape guard its own comment already claimed**,
  with the demotion stated in the commit as a SCOPE change of that row and the golden re-baselined from
  the REBUILT binary.
- **⚠ Route #7's ATTRIBUTION mirror: a crash INSIDE a generated shell is usually the shell being
  faithful** (measured 2026-09-02, runtime's `textAddr`). The `RecvGenerator` shell's
  DerefOrNull → NullRef → NRE on the first field touch IS Go's nil-receiver semantics; the nil came
  from `funcInfo()`'s module search, which can never succeed because the package's sole moduledata is
  a permanent empty stub (`len(pclntable)==0` skips it every time). A structurally guaranteed nil is
  not a race and not goroutine-specific — which tests crash is decided only by which ones reach the
  call at all. **Trace to the ASSIGNMENT, not the frame**, before billing `src/gen/`.
  ⚠ **And the CHEAPEST instrument for reaching that assignment is the BUILT GUARD BINARY run by hand**
  under the right runtime root (`DOTNET_ROOT`), 2026-09-05: the runner captures only the FIRST stderr
  line (`Fatal error.`) while the binary itself prints the whole managed stack, and the frame names the
  assignment. Two runs, five seconds, in place of a four-arm bisect that had already been priced.
- **FALSE-GREEN route #8 — a guard DISARMED by a LEGITIMATE change (found 2026-09-01, the
  init-hook relocation).** Distinct from routes #1–#7: nothing is stale and nothing mis-runs — the
  guarded property genuinely moved house, and a guard asserting an assembly-level property by
  grepping ONE emitted file goes silently VACUOUS in its negative direction ("the bare form must
  not appear" is trivially satisfied by a file that no longer holds the construct at all). The
  positive direction fails loudly and gets fixed; the negative just stops testing, and the exit
  code says two failures when the real damage is four assertions. Glob-widening cannot fix the
  class when a DRIVER writes the artifact the exercised call never touches. Remedy: assert the
  DECISION (the recorded map/registry the pass writes — `packageImportInits` in the measured
  case), never the artifact's text, and re-check a guard's negative arm whenever the construct it
  greps for legitimately relocates.
  ⚠ **Route #8's sharper form KILLS the suite instead of going vacuous, and its verdict rides CLASS
  ORDER** (measured 2026-09-02, GolibTests): the guard's premise — "GolibTests does not reference
  converted `flag`" — was disarmed by a later `ProjectReference`, after which the converted `flag.Parse()`
  parsed MSTest's OWN command line through the process-global `flag.CommandLine` (`ExitOnError` →
  `os.Exit(2)`) unless a sibling class that replaced it with `ContinueOnError` happened to run first: one
  host's 460/460 was a lucky ordering, another's 82-then-abort the same defect. The fix SHAPE matters as
  much — the host parses its OWN args and **never mutates a process-global that converted tests read**
  (`os.Args` feeds `sync`'s `TestMutexMisuse`, `flag`'s `TestExitCode` and every self-re-exec): a
  divergence STATED against the ruling is how to diverge.
  ⚠ **Route #8's TEXT-GREPPING family, three members measured 2026-09-03/04, all cured by the same
  remedy — assert the DECISION, not the emission.** (1) A guard grepping emitted `.cs` went false-RED
  at master on a hand-own's HEADER COMMENT quoting the pre-fix emission: strip comments, or assert the
  recorded decision. (2) **A census of the EMITTED text cannot recover a decision the converter made**
  — `[GoRecv] … this ref T` matches **5,686** declarations because it is ALSO the emission for every
  Go VALUE receiver on a struct, so a grep for "pointer-receiver primaries" reads the wrong population
  by an order of magnitude; records that describe a converter decision derive from the front end
  (`go/types` receiver kind) plus the pass's OWN selection map, and a guard over them compares against
  that map. A claim nearly carried on such a grep was retired by the census before it reached a
  design. (3) **A guard whose PATTERN under-scopes cannot go red where it matters**: the keep-alive
  census's `syscall.`-qualified pattern scanned 83 of 351 protected sites and, over the `syscall`
  package alone, reported ZERO temps — tripping its own vacuity check — so an injected defect there
  was INVISIBLE. Two companions from the same arc: **a guard arm can be RED-BY-DESIGN at a branch tip
  and correct** (arm 2's 12 sites are exactly what a sibling seat fixes, green on the union), so merge
  ORDER decides a guard's verdict and the train's ASSEMBLED tree is what it is measured on; and a
  STATIC "this guard is red at master" claim was FALSIFIED by running it (clean 83/83) — run the
  guard before reporting its colour.
  ⚠ **Two more members, measured 2026-09-04, both inside guards written to notice a REWORDING.**
  (4) **A guard that greps a source file for a MARKER STRING reads the PROSE that explains the marker
  beside the code that matches on it** — every instrument that classifies a converter stderr line
  carries a comment naming that line — so a `strings.Contains` guard stayed GREEN with the live
  classifier DELETED, twice, until its own positive control said so. The guard extracts the LIVE regex
  literal from the code, and REJECTS `-notmatch` EXCLUSION lines, which carry the marker's words a
  second time and would read fine with the classifier gone. (5) **A substring "is the marker still
  consulted" check passes a reworded pattern that CONTAINS the original** (`GoArchExclusiveXX`) — the
  glyph-substring over-match inside a guard whose entire purpose is to notice rewording: extract each
  CONSUMER's live pattern, compile it, and require it both to MATCH a real marker line and to REJECT a
  prose decoy. Their companion at the CNR: a **check-only switch that empties the measurable set AFTER
  the skip block prints** is what lets a live classifier be controlled without a transpile.
  ⚠ **The constructive form of "assert the DECISION": a CONVERTER ATTRIBUTE that RECORDS a decision IS
  a census of its own population, and beats any predicate written later** (2026-09-05).
  `[GoValueClone]` marks every struct carrying a fixed-size array field (or another struct that does),
  so the READ-side defect (a `*[N]T` cannot be VIEWED over native memory) and the WRITE-side defect
  (such a struct cannot be PASSED to the kernel by address) share ONE population — 493 attributes
  across 95 package metadata files plus 47 in 27 other files, measured with both controls. **Two open
  items that share a population are ONE class**, and the point remedies are debt against the model fix,
  named as such in its design record.
  ⚠ **A guard that documents a defect it does not ASSERT is PARKED, not landed half-green** (measured
  2026-09-03, the nine-shape dims guard and the canonical-identity tripwire). A red-by-construction
  guard lands WITH its arc — complete, with its registration reverted — rather than shipping a
  commented-out row that the next reader reads as coverage.
- **⚠ MID-BATTERY SOURCE FREEZE — while any gate battery is running, converter/gen/golib source is
  untouchable, on ANY branch (ruled 2026-08-30).** The behavioral runners rebuild `go2cs.exe` from
  DISK source the moment a `.go` file is newer than the binary, and golib/gen compile into every
  project the battery builds — so an edit mid-run makes the remaining legs measure a MIX of committed
  and uncommitted state (route #1's stale-binary trap inverted: a too-FRESH binary). Lanes queue
  their cuts until the battery's summary prints; the coordinator announces battery start/close on the
  mailbox for exactly this reason.
  ⚠ Scope, stated 2026-09-02 after a lane held a cut it never needed to: the freeze binds **the
  worktree the battery runs in, on any branch checked out THERE** — the runners rebuild `go2cs.exe`
  from that tree's disk and golib/gen compile into the projects that battery builds, so a lane
  editing its own clone on its own machine cannot reach a battery leg elsewhere on the fleet.
  ⚠ **An UNPINNED SHELL violates the freeze without failing anything** (2026-09-04): `IsConverterStale`
  compares the exe's embedded release against the AMBIENT toolchain, so an unpinned shell makes every
  harness invocation REBUILD `go2cs.exe` — in a worktree carrying a battery that is the freeze broken
  by a shell rather than by an edit, and the only reason one such run was benign is `go.mod`'s own
  `go 1.23.12` line steering the toolchain switch, not the pin. Pin the shell (route #4's stamp is
  what it is compared against), and read a surprise converter rebuild as this before anything else.
  ⚠ **AND THE FREEZE BINDS THE HARNESS SCRIPTS** (2026-09-04, the coordinator against itself):
  **bash reads a script incrementally BY BYTE OFFSET**, so an insertion above the current position
  shifts the bytes and the next command is parsed from the middle of a line — a train assembly died
  thirty minutes after a seat slot was inserted into that same file for the NEXT train, on
  `syntax error near unexpected token '('` at a leg with nothing to do with the edit. `bash -n` passed
  before AND after; the running process never re-parses. **Launch every long run from a PER-RUN COPY
  of its script** (`coord-<train>-assemble-runN.sh`) and edit only the original; the relaunch then
  resumes on the intact head behind a skip flag, since the seats, the guards and the completed leg's
  own log holding its verdict are not in doubt.
  ⚠ **The door that opens by ITSELF: a DERIVE for the NEXT train must never write the RUNNING train's
  scripts** (2026-09-05). One derive's copied writer pairs carried the previous train's names as BOTH
  source and destination and rewrote the running assembly script IN PLACE while its chain was executing
  it — saved only by the chain being blocked inside a leg's command and by the derive being
  deterministic, so a re-derive restored the launched bytes. **A derive's writer REFUSES any path
  belonging to the running train**, and the freeze above binds coordinator scripts exactly as it binds
  converter source.
- **⚠ STANDALONE (no-solution-context) builds of tests/behavioral projects measure the DEPLOY ROOT,
  not the repo — and the errors look SEMANTIC (paid 2026-08-30, cost one full invalidated bisect).**
  Without `$(SolutionDir)`, `$(go2csPath)` falls back to the machine-global deploy root
  (`%USERPROFILE%/go2cs` / `%GOPATH%\src\go2cs`), which is STALE between deploys. A missing root is
  loud (CS0246 on `go`); a stale root is not — it produces plausible type-mismatch errors (CS1503,
  CS1929, CS0234 on a newer attribute) that read as real regressions and are COMMIT-INDEPENDENT,
  which is how a bisect probe built this way reported "no green endpoint" across three anchors whose
  in-solution builds were all green. **Any standalone build of a project under `src/tests` must pin
  `-p:go2csPath=<repo>/src/` (forward slashes), and a bisect probe must carry the pin.**
  collision site (root-caused 2026-08-21, fixed at the converter 2026-08-22).** A POSIX environment
  block is case-SENSITIVE, so `GO2CSPATH=/root/go2cs` and `go2csPath=/root/go2cs/src/` are two
  entries; MSBuild materializes environment variables as properties and resolves property NAMES
  case-INSENSITIVELY, so both fold into ONE `$(go2csPath)` and the winner is decided by enumeration
  order inside the .NET env-table plumbing — a per-process coin flip. The losing draw concatenated
  `$(go2csPath)gen/...` into `/root/go2csgen/...`, dangled the analyzer and every stdlib
  ProjectReference, and the build died in a CS0246 storm on every golib type: intermittent,
  package-shuffling Linux `-tests` failures that killed three measurement campaigns with every
  plausible suspect A/B-eliminated first. **Windows environment blocks are case-insensitive at the OS
  level — the two names are ONE slot — so five weeks of Windows sweeps could not see it.** The
  converter now (a) never exports its own derived `GO2CSPATH` (`resolveGo2CSPathDefault`, `main.go`)
  and (b) scrubs every case-variant from the inherited environment before appending the canonical
  entry (`childEnvWithGo2CSPath`, `testConversion.go`), so a child carries exactly one spelling
  whatever the invoking shell holds; guarded by `childEnvGo2CSPath_test.go`. The general rule outlives
  this variable: **anything a child reads through a case-insensitive resolver must be injected ONCE —
  scrub-then-append, never append-and-hope — and "Windows is fine" proves nothing about the class.**
  The Linux harness pin (`_paths.ps1`) STAYS until a Linux lane re-measures without it.
  ⚠ **A pin the CONVERTED side needs goes in the SHARED child-env base, never in one side's env**
  (measured 2026-09-02, the TZ pin): `runtime.envs` is filled by a `[ModuleInitializer]` before
  `Main`, so no host code precedes the snapshot and `TestHost.Run` cannot pin `TZ` from inside the
  process — and making the snapshot live would break Go's own set-at-process-start semantics. The fix
  is the process environment at LAUNCH, beside GOROOT/PATH, applied to BOTH sides of the comparison:
  a cross-SIDE divergence is worse than the cross-platform one it was meant to cure.
  ⚠ **A control harness reproduces the caller's ENVIRONMENT, not just its command** (measured
  2026-09-02): `BehavioralRunner` invoked DIRECTLY inherited neither the CI job's `GoTargetOS` nor
  `_paths.ps1`'s pin, so every L3 csproj took the windows default on a Linux host and the leg read
  "red by construction" — for a pin that already existed. Diff the CI step's environment against the
  repro's before believing a local red; a repro differing from its caller by one unstated variable is
  measuring its own shell.
- **⚠ TOOLCHAIN RESOLUTION: the pipeline's ORACLE side runs whatever bare `go` resolves on PATH —
  GOROOT alone does NOT pin it (measured 2026-08-29, the net/http bank lane).** `go2cs.exe` shells
  out to `go test -json` for the compare oracle, and that child inherits PATH; on a box whose system
  SDK differs from the pinned one (this machine class: ambient 1.23.1 vs pinned 1.23.12), a shell
  setting only GOROOT runs the WRONG release's oracle. The failure shape is a new member of the
  mass-empty family: **Go="" for every test** — the ORACLE side blank while the C# side reads
  plausible — the mirror of the file-lock signature (C# side blank), and it reads like total
  conversion failure. Prepend `$env:GOROOT\bin` to PATH in every pipeline shell and verify with bare
  `go version`, never just `go env GOROOT`. Same family, third member (lane R, 2026-08-29): a bare
  `go2cs -tests` on a **Linux** host bypasses the sweep's `GoTargetOS` pin and links the WINDOWS
  dependency set, minting phantom CS0426s that read as Linux defects — net-family Linux work routes
  through the SWEEP, always.
  ⚠ **Corrected 2026-09-04 on the MECHANISM, the habit standing: the load-bearing part is the
  `GoTargetOS` PIN, and the sweep is ONE way to supply it, not the only one.** A hand-invoked `-tests`
  run on a non-Windows host needs `GoTargetOS=linux` in its environment — MSBuild materializes
  environment variables as properties — which a lane can export directly; routing net-family Linux
  work through the sweep stays the reliable habit because the sweep carries the pin, the toolchain and
  the cgo state together, but "it must go through the sweep" was the wrong reason.
  ⚠ **Fourth member — the RIGHT SPELLING of the WRONG RELEASE, and every existing GOROOT check is
  blind to it** (measured 2026-09-02, a cloud lane): on a box carrying side-by-side SDKs bare `go`
  resolved 1.24.7 while the corpus pins 1.23.12 — `go env GOROOT` stays self-consistent, the conversion
  succeeds and exits 0, and the spelling/namespace guards pass because nothing about the PATH is wrong.
  **A conversion against the corpus prints `go version` AND GOROOT before it runs.** It is armed at
  BOOT too: a stale `/etc/profile.d` lane script exporting an older `/usr/local/go/bin` beat the newer
  fleet file (profile.d sources alphabetically — a `zz-` prefix fixes it), and `wsl.exe -- bash -lc` does
  not source profile.d like a real login: verify by bare `go version` in a real login shell.
  ⚠ **And its QUIET shape, with the seatbelt that is not one (2026-09-02, the container class):** where
  the loud form misroutes the namespace and exits 0, an oracle run under an ambient 1.24.7 against a
  1.23.12 corpus answers NORMALLY — no empties, no errors, a real comparison against a corpus the tree
  does not have. `GOROOT="$(go env GOROOT)"` is the trap wearing a seatbelt: pin explicitly, put its
  `bin` FIRST on PATH, ABORT unless bare `go version` reports the pinned release, and re-measure
  anything banked under an ambient one. The container class is NOT uniform (no bare `go` on one host,
  1.24.7 on another, 1.25.1 off PATH on a third) and a persistent USER-scope GOROOT can pin an old
  release on a laptop lane, so no lane assumes another's toolchain number — and pin `-go2cspath
  <worktree>/src` on every hand-invoked `-tests` run, whose generated csproj otherwise falls back to
  the machine-global deploy root (MSB4006 loud; a plausible verdict from uncompiled bits quiet).
  Because nothing recorded WHICH release ran the oracle, `oracleGoVersion` now goes into the comparison
  record, captured as OBSERVED — a `go version` through the same call, directory and environment the
  `go test -json` child inherits, `omitempty` so a late probe failure cannot invalidate a comparison.
  ⚠ **Third door, and the instrument that closes all three (2026-09-02).** The lane's OWN
  `export PATH=<toolchain>/bin:$PATH` defeats the fleet's `zz-` profile.d pin by construction — on a
  fleet host use the login shell and never prepend a toolchain path; a probe answering
  `command not found` is describing its own environment, not the host's. Cross the WSL boundary with a
  heredoc (`wsl -- bash -s <<'EOF'`): `wsl -- bash -lc '…'` expands `$(...)`/`$VAR` in the OUTER shell,
  so verification prints come back EMPTY (`GOROOT=`, `HEAD=`) and read as answers — three false
  "command not found" probes and one cut-presence line evaluated against the wrong tree. An empty
  verification print is a broken instrument, never a pass. And the wrapper that prints bare
  `go version` and ABORTS on a mismatch is itself NEGATIVE-CONTROLLED once against the box's other
  toolchain — the control must exit non-zero having run zero sweep stages — before any green it
  reports is believed.
  ⚠ **Two amendments to that third door, both measured 2026-09-02.** (1) **PRINTING a pin is not
  CHECKING it** — the "prints `go version` AND GOROOT before it runs" wording above is satisfied by a
  DECORATION: a control script printed `go1.24.7` on its first line, from a `go env GOROOT` taken in an
  unpinned shell, and carried on; three findings descended from that run and were withdrawn. An
  instrument that prints its pin and proceeds has no guard — it ABORTS on mismatch, and the print is
  only evidence of what the abort compared. (2) The WSL quoting rule WIDENS: **every substitution
  inside a single-quoted `wsl … -lc '…'` string is expanded by the OUTER shell** — verification prints,
  loop variables AND exit codes, which is how a false `(exit 0)` and three empty-path parse errors
  landed in one evening. The heredoc form (`wsl -- bash -s <<'EOF'`) is the only spelling for crossing
  that boundary.
- **`TargetComparisonTests` compares goldens with line endings NORMALIZED** (CRLF→LF; see
  `TargetComparisonTests.FileMatch` / `BehavioralRunner.FilesEqual`, both strip CRs). It was a raw
  byte-for-byte compare until 2026-07-07. Content diffs are still caught exactly; a pure line-ending
  difference is ignored (it can only come from autocrlf, never from the deterministic converter). To
  re-baseline goldens after an *intended* output change, run the **`UpdateTestTargets`** project with
  **`--createTargetFiles`**, or `run-behavioral.ps1 --update-targets` — don't hand-edit goldens.
  ⚠ **Both re-baseline paths RE-TRANSPILE each project they are about to re-baseline, unconditionally,
  immediately before the copy, and REFUSE (exit non-zero, by name) when that transpile fails, times
  out, or exits 0 having converted best-effort (2026-09-04).** Until then neither did: the utility ran
  the converter *not at all* and this file carried the prerequisite as a sentence ("re-transpile
  first … or the copy silently re-baselines stale output"), while the runner ran it only when its
  **mtime** predicate said the project was out of date. Both are the same hole and it is FALSE-GREEN
  ROUTE #2 turned on the goldens: `UpToDate` answers on mtimes, and a `.cs`-only restore, a
  `Copy-Item` or an editor save all leave a `.cs` newer than both its `.go` and `go2cs.exe` while its
  CONTENT came from some other converter — after which the copy makes `.cs`, `.cs.target` and
  `UpToDate` agree by construction and no later run can see it. There is deliberately **no**
  up-to-date predicate on either path now, for the reason CNR has never had one: a stale COMPARISON
  is recoverable, a stale RECORD is not. The utility's `--only <Name>[,<Name>…]` narrows one
  invocation's transpile-and-copy (the four `<TestMethods>` blocks stay a function of the whole
  project set) — it exists so the refusal branch is not a ~25-minute control nobody runs.
  ⚠ **Three rules that fell out of landing it (2026-09-04).** A whole-corpus re-baseline banks any
  `.cs`-vs-committed drift into the goldens SILENTLY, so **a byte-identical CNR verdict is its
  PRECONDITION and runs FIRST, never after**. The **refusal property asserted is the golden UNCHANGED
  ON DISK** — a refusal that has already copied is a report, not a refusal. And a corpus-wide control
  states its BASELINE: 718 golden pairs byte-identical at the head is what makes a whole-corpus copy a
  no-op and ONE moved golden a measurement. (The demonstration that such a path is broken is the
  runner printing `ok` having invoked the converter ZERO times and minting a poisoned `.cs` into the
  golden at exit 0.)
  ⚠ **A golden that byte-matches a DEGRADED emission is a phase actively VOUCHING for the hole**, not
  one merely proving nothing (2026-09-04): at master the Target phase PASSED over a best-effort
  `main.cs`. A harness that cannot MEASURE a transpile skips its Target compare exactly as it does for
  Fail and Timeout — a golden compare is a statement about a measured emission or it is nothing.
  ⚠ **A RED CONTROL LEAVES THE DEFECTIVE EMISSION ON DISK BESIDE THE FIXED GOLDEN** (2026-09-05):
  reverting the fix and re-transpiling the guard to reproduce the defect rewrites its `.cs` while the
  `.cs.target` still holds the FIXED form, so a commit taken there banks the wrong emission next to
  the right golden and only a later Target phase would catch it. **After any control that
  re-transpiles: restore the source, purge `bin`/`obj`, re-transpile, and require the `.cs`
  CR-strip-identical to its golden before staging** — and re-read the worktree's state after any
  interruption, since a resumed agent inherits whatever the control left behind.
- **autocrlf gotcha (`core.autocrlf=true`) — two SEPARATE concerns:** the converter emits CRLF for C# line
  endings but preserves the Go source's LF inside multi-line string literals, so those `.cs`/`.cs.target`
  contain mixed CRLF/LF, and autocrlf rewrites the in-string LFs to CRLF on checkout.
  (1) **Golden text comparison** — no longer an issue: the comparison is line-ending-insensitive (above),
  so a smudged golden still matches and **no `-text` mark is needed just for the byte compare**.
  (2) **Runtime correctness** — still needs `-text`: if a project's *compiled program* embeds and observes
  a multi-line string literal at runtime (e.g. `Solitaire`'s board, printed via `println`), autocrlf smudges
  that literal's newlines to CRLF in the on-disk `.cs`, and any build that compiles the committed `.cs`
  *without* re-transpiling (VS, CI `dotnet build`, or the runner's up-to-date-skip) bakes the wrong `\r`
  runes into the value → the program misbehaves (Solitaire's board geometry breaks and the solver hangs).
  So `Solitaire`/`SortArrayType`/`StdLibInternalAbi` keep their `.cs` `-text` marks. A NEW multi-line-string
  test only needs `-text` if its program's *behavior/output* depends on the literal's exact bytes; if the
  literal is inert (never printed/measured), no mark is needed and the golden compare stays green regardless.
  **⚠ The CRLF working-tree form is now PINNED, not inherited from `core.autocrlf` (2026-08-08, r46c).**
  `.gitattributes` carries a `text eol=crlf` block for every converter-emitted artifact type — `*.cs`,
  `*.cs.auto`, `*.cs.target`, `*.csproj`, `*.slnx`, `*.props`, `*.targets`, `src/core/**/README.md` —
  ordered ABOVE the `-text` blocks so those keep their verbatim-bytes exemption (last matching pattern
  wins). Rationale: the converter emits CRLF *unconditionally*, so the checkout was the only variable,
  and a clone with `autocrlf=false` (git's default on Linux/macOS) materialized LF and made
  `check-no-regression` report the entire corpus as drifted before any work started. **Nothing about
  the Windows lane changed** — `eol=crlf` reproduces exactly what `autocrlf=true` was already doing,
  verified by `git add --renormalize .` over all 9,380 tracked files staging **zero** corpus files
  (every non-LF blob in the index was already `-text`). Two consequences worth carrying: a whole-tree
  renormalization is **not** owed, and the mixed-CRLF/LF phantom described above is *unchanged* in
  shape — it is simply platform-independent now. Do not "fix" a `.cs` to LF to match a Linux habit;
  the pin will put it back.
- **testhost lock gotcha:** a stray `testhost`/`vstest.console` from a prior run can lock
  `BehavioralTests.dll` → next build fails with `MSB3027` ("file locked by testhost"). Kill it (and
  `dotnet build-server shutdown` frees bin/obj locks) before rebuilding — not a real compile error.
  **Root cause + mitigation (2026-06-30):** the MSTest `Exec()` used an unbounded `WaitForExit()`, so a
  hung child (a deadlocked transpiled program, or a build blocked on a lock) hung the suite forever and
  orphaned testhost. `Exec` now has a per-call timeout (180s build/transpile, 30s run) that kills the
  whole child **process tree**, and disables MSBuild node reuse (`MSBUILDDISABLENODEREUSE=1`) so in-test
  builds don't leave lock-holding worker nodes; `AssemblySetup.[AssemblyCleanup]` runs
  `dotnet build-server shutdown` **only for a bare `dotnet test`** — a `run-behavioral-tests.ps1` run
  sets an env-var contract that suppresses it, since the script's default path isolates its own children
  instead (chip `6fe128108`, 2026-08-08). Prefer **`src/tests/Behavioral/run-behavioral-tests.ps1`**
  (clears stale hosts *before* the build — the lock manifests at build time — and runs with
  `--blame-hang`) over a bare `dotnet test`.
  **⚠ An MSTest verdict WORD is not a verdict — an ABORTED run prints one anyway** (measured 2026-09-02,
  GolibTests on a Linux lane): the second-to-last line reads `Passed! - Failed: 0, Passed: 82` and the
  LAST reads `Test Run Aborted.`, against a declared count near 470 — the exit code is honestly 1, but a
  verdict-word grep reads green, and `$?` after a pipe is the LAST command's status (grep's), so a piped
  invocation captures the raw exit first. **A GolibTests gate greps for `Test Run Aborted` AND compares
  the run's Total against the DECLARED count (`grep -c '\[TestMethod\]'`)**; an abort is an UNMEASURED
  suite, never a pass — the tell was adding 7 tests and watching the total stay 82. Run it `--no-build`
  behind the solution leg, too: a `dotnet test` that BUILDS raced twice in one night on a spurious
  CS0234/CS0246 that was gone on `--no-build` against the build just completed.
  ⚠ **And that DECLARED count is derived from the COMPILE SET, not from a raw `[TestMethod]` grep**
  (measured 2026-09-02): `GolibTests.csproj` `Compile Remove`s the Linux-only test files when
  `$(GoTargetOS)` is not linux, so a run reporting 474 against 479 grep-counted methods is
  COUNT-MATCHED, not an abort. Subtract the methods in `Remove`d files whose condition holds before
  reading a shortfall as a truncated suite.
  ⚠ **And WHAT belongs in that `Remove` set is decided by the hand-own's flavour** (2026-09-05): a
  per-GOOS hand-own's `Go`-prefixed test helpers exist only under that flavour, so the GolibTests
  classes referencing them are linux-only FILES **by construction** and belong in the `Compile Remove`
  set for every other `$(GoTargetOS)`. The WINDOWS-flavour solution build at the union is the gate
  that sees it (22 CS0103 on five helper names); a lane that cannot build the other flavour's solution
  STATES that gap in its seat and adds the `Remove` in the same cut. **A chain is STOPPED at a red
  compile gate**, never run on to legs that cannot mean anything on a broken union.
- **⚠ CONCURRENT-SESSION KILLS — worktree isolation does NOT isolate `Get-Process <name> | Stop-Process`.**
  Those cleanup preambles (here, and the ad-hoc `Get-Process BehavioralRunner,testhost | Stop-Process` that
  is easy to type before a run) match by process NAME across the whole machine, so they kill a SIBLING
  worktree's in-flight suite. Signature: **exit `-1` with the log truncated mid-line and no diagnostic** —
  e.g. a full run died at 124s inside `PreBuildSharedDeps` and another at 163s inside `RunCompileGo`, and
  the same corpus then passed 521/521 untouched. Read that as "killed externally", NOT as a compile failure,
  and do not go hunting for a runner bug. Waiting for the other worktree's process to exit is not enough
  (it re-arms for its next run); the reliable defence is to be **unmatchable by name** — copy the apphost to
  a unique name in the same bin dir and run that (`Copy-Item BehavioralRunner.exe myRunner.exe`; it still
  launches the embedded `BehavioralRunner.dll` and `AppContext.BaseDirectory` is unchanged, so discovery is
  identical). Scope your own kills by path (`Where-Object { $_.Path.StartsWith($myWorktree) }`) so you are
  not the one doing this to somebody else.
  **⚠ The apphost rename does NOT cover the other reaper — harness background-task TREE reaping (measured
  2026-08-12, the A2 integration agent).** Being unmatchable by name defends against a sibling's
  `Get-Process <name> | Stop-Process`; it does nothing against the harness reaping a session's own process
  tree when a turn ends, because that walks parentage, not names. A long runner started as a background
  Bash/PowerShell child is IN that tree and dies with it — the same truncated-log, no-diagnostic signature,
  which is why it reads as the by-name kill and gets misdiagnosed as one. Surviving it required launching
  the runner **DETACHED** via `Start-Process` so it is not a child of the turn's process tree. Same shape
  as the sweep caveat in the budget table below (a LANE parking a detached sweep still loses it); the
  difference is that `Start-Process` detachment is what makes a long run survivable at all.
  **The detachment flags are load-bearing (measured 2026-08-14, the argv-stop and os-signal lanes):**
  `Start-Process -WindowStyle Hidden` with output redirected to a log file survives the reap;
  `Start-Process -NoNewWindow` followed by `Wait-Process` does NOT — the wait re-parents the session's
  fate onto the child and the turn boundary kills it exactly as if it had been spawned inline.
  ⚠ **The two detachment stories are measured and point OPPOSITE ways (2026-09-02).** A
  `Start-Process -WindowStyle Hidden` from INSIDE a PowerShell TOOL call died silently ~15 s in (the
  documented pattern covers a BASH-launched child surviving the turn boundary, not a tool call's own
  job scope), while a Bash `run_in_background` task is reaped with the SESSION's process tree — a
  2-hour solo sweep died ~13 min in and sat UNDETECTED for 76, with no completion notification.
  Anything longer than a turn runs DETACHED, env-pinned in the SAME command, logged unique-per-run
  and polled POSITIVELY by PID;
  clean-death evidence before a restore is modified files with ZERO untracked.
  ⚠ `Wait-Process` has ALSO reported a still-running target as exited, twice in a row (2026-09-01,
  the residual-pass lane): a background-wrapped `Wait-Process -Id` said done while
  `Get-CimInstance Win32_Process` showed the host alive with a live `go2cs.exe` child — one
  redundant CNR raced into the same behavioral tree before it was caught (the r41 overlap hazard,
  avoided only just). Mechanism unconfirmed; treat any `Wait-Process` "done" as unverified until a
  positive `Get-Process -Id` poll agrees (`while (Get-Process -Id $pid) { Start-Sleep 20 }` read
  correctly where the wait lied). Poll the
  log file (or the process by PID) instead of `Wait-Process` — and write the poll POSITIVELY
  (`while` + explicit `exit 0`/`exit 1`), never `until ! powershell -Command "exit (Get-Process …)"`:
  `exit $true` is exit code 1, so that loop ends instantly and reports "exited" while the process
  still runs (measured 2026-08-16 — the false reading launched a SECOND CNR into the same tree, two
  racing transpiles, caught only by PID inspection).
  ⚠ **The same false-"exited" reading has a SECOND, entirely different mechanism: Git Bash's
  `kill -0 <pid>` cannot see a WINDOWS pid** (measured 2026-08-26). The Bash tool's `kill` resolves
  pids in its own emulation namespace, so `while kill -0 $PID; do sleep 30; done` against a pid from
  `Start-Process -PassThru` (or any `Get-Process` id) exits on the FIRST iteration and reports the
  process gone while it is still running — no error, exit 0, indistinguishable from a real
  completion. It reproduced the 2026-08-16 damage exactly and then some: two CNR runs believed dead
  were alive, a third was launched, and THREE concurrent transpiles raced into one behavioral tree
  (the r41 "never let two conversions overlap" hazard), with a partial 2-package `git status` that
  read as a reassuring near-clean verdict. The tell was an mtime census — 288 of 641 packages never
  re-transpiled — and the proof was `Get-CimInstance Win32_Process` showing both "dead" hosts alive
  with live `go2cs.exe` children. **Rule: never wait on a Windows pid from Bash.** Wait from
  PowerShell (`Get-Process -Id`), or — better — make the long run the harness BACKGROUND TASK itself
  (`run_in_background`, the child of bash) so its real exit code is the task's, which is what
  PROTOCOL v3's mailbox monitor already relies on. And when auditing for strays, exclude your own
  querying process: a `Where-Object { $_.CommandLine -like '*check-no-regression*' }` sweep matches
  the very command line performing the sweep, so it reports a phantom survivor and, if you kill it,
  kills your own shell.
  **Three probe-hygiene rules from the same family (2026-09-01/02).** **Process AGE is read from
  `CreationDate` against `Get-Date`**, never against an assumed clock — a healthy three-minute-old
  run was killed in the belief it had been hanging for hours. **`pgrep -f <name>` matches its own
  wrapper's command line** — the bash edition of the self-match above — so a `while pgrep -f` wait
  loop spins forever on its own reflection after the child has exited; match on `/proc/*/exe`, i.e.
  the executable, never on a pattern that can match the process running the check. And **completion
  inferred from a SIDE EFFECT is not completion**: a file reverted because a running CNR had already
  transpiled it is a footprint, not an exit code — check the run.
  **⚠ Two more, 2026-09-02.** **Relaunching a chain while its predecessor's TAIL leg is still alive
  puts two runs in one worktree** — a third rebuild attempt met the second chain's in-flight `reflect`
  `-tests` convert as untracked `*_test.cs` and aborted on its dirt gate, the r41 overlap hazard
  caught only because that gate existed: census live processes (and wait for the task notification)
  before relaunching anything into a worktree. And **the harness's own
  `git status --untracked-files=all` over a worktree full of `bin`/`obj` can run for an HOUR** —
  slow, not hung, and not evidence of anything else.
  **⚠ A pid captured from `ps` seconds after `setsid` can be the WRAPPER** (met by two lanes on
  2026-09-04), and `ps | grep <script> | head -1` picks it too, because the wrapper's own eval line
  carries the script's text — such a "process" reports EXITED instantly, the false-"exited" family
  through a third door. **The chain writes its own PIDFILE and the waiter reads that.** Beside it, on
  the container class: **a restart notice is not evidence either way** — one restart killed the
  watcher and spared the detached chain, the next killed the chain mid-leg and left a 0-byte log — so
  check PID IDENTITY before relaunching, and never relaunch on assumption.
  ⚠ **And `setsid nohup … &` leaves `$!` naming the SETSID PARENT**, not the chain (2026-09-05) — the
  same wrapper-pid reading through the LAUNCHER's own door rather than through `ps`. **PID-record a
  detached run from INSIDE it** (`echo $$`, then `exec`), and kill by an exact `bash <path>` match
  that excludes `$$`.
  ⚠ **A ROW WHOSE DEADLINE FLOOR EXCEEDS A HOST'S UPTIME IS NOT MEASURABLE THERE, and is not
  retried** (2026-09-04): an hourly-restarting container against a 30-minute row cannot produce a
  verdict however many attempts it makes. State it, and move the row to a host that stays up — or drop
  it.
  **⚠ "An instrument that can match ITSELF is measuring its own presence" (named by the lane that
  paid it, 2026-09-03) — and it kills.** The coordinator's own process census
  `CommandLine -like '*<worktree-id>*'` matched the coordinator's OWN bash (whose command line carried
  the id as a variable) and `Stop-Process` killed the querying shell mid-command, exit 255; a kill
  loop keyed on a branch-name fragment matched its own shell the same way; and a `/proc` census keyed
  on a marker string counted the probe whose own `case` pattern contained it (2 monitors reported
  where 1 ran, a healthy one nearly killed). **Any census that KILLS excludes the shell/host process
  names (`bash`, `powershell`, `pwsh`) and its own PID, or matches on the EXECUTABLE
  (`/proc/*/exe`) — and better, keys on something the RUNNING loop ASSIGNS** (a variable the loop
  sets, which a probe merely describing the loop does not contain). Every instance was caught only by
  a second derivation disagreeing, never by the census itself. A rule met twice becomes a CHECKLIST
  line in the lane's own scripts, not a thing to remember.
  (`pkill -f <script path>` issued from a command line that CARRIES that path is the same self-match
  in bash, and it kills the caller's own shell: kill by PID from a bracketed grep.)
  ⚠ **And SPLITTING the pattern with concatenation does NOT help** (measured 2026-09-05): the
  substrings are still in the querying shell's own command line, so the wildcard still matches — a "no
  chain may be live" gate reported 4, then 3, every one of them the shell performing the check, age
  0m. **Filter by AGE** — a real long-running process is minutes old and the querying shell is seconds
  old, read from `CreationDate` as above — **or by ancestry.**
  **⚠ ABSENCE AT ONE INSTANT IS NOT DEATH, in three costumes (2026-09-02/03).** A sub-agent's
  **0-byte task-output file does not mean it died** — the transcript is written at COMPLETION, and a
  worktree with no new files for an hour can be a seeded reconvert writing outside `src`; the
  coordinator declared a live agent dead, dispatched a duplicate, and had to stop it when the original
  reported with full gates. A gate wrapper called "dead" had merely finished its leg and then spent
  **26 minutes** inside `git checkout` + `git clean` over a `bin`/`obj`-heavy worktree — the process
  probe landed BETWEEN spawns — while `| tail -25` buffered the task's whole output so its log read
  empty while it ran. **Liveness is a process census against the worktree's path PLUS the agent's own
  notification; a stopped-with-no-children notification is the only "done".**
  **⚠ Correction to the pipe rule, measured 2026-09-03: a pipe masks a command's exit status only
  WITHOUT `set -o pipefail`** (`set -uo pipefail; false | tail -1` → 1; without pipefail → 0), and a
  REDIRECT (`> log 2>&1`) preserves the command's own status. The durable half of the earlier note
  stands — capture the real exit before any pipe, and read the RECORD rather than an exit code,
  because the record carries per-test verdicts an exit code cannot — but "an exit code is worthless
  the moment a pipe is in the command" is FALSE and was retracted within the hour by the lane that
  wrongly confessed it. **A false CONFESSION corrupts the record as much as a false success.**
  **⚠ A harness `TaskStop` (or killing the bash) does NOT reap the converter child** (measured
  2026-09-03): `go2cs.exe` kept spawning after the stop, the PIDs `taskkill` reported differed from
  the ones listed seconds earlier, and a REUSED log path spliced two runs into one file — together
  putting TWO conversions into one seed root (the r41 DYNTYPE hazard), with nothing banked only
  because the diff never ran. **A seeded-conversion instrument carries a PREFLIGHT that refuses to
  start while any `go2cs.exe` is alive, and a run-tagged log name.** ⚠ And the r41 rule WIDENS
  (2026-09-03): **ONE converter process per lane box at a time** — a CNR died 43 s after a `-stdlib`
  conversion started on the same box with a DIFFERENT output root (mechanism unrooted), so footprint
  diffs wait for the CNR to print its verdict.
  **⚠ Two 2026-09-05 sharpenings, and the first inverts how a killed run is read.** **A KILLED WRAPPER
  TASK IS NOT A VERDICT ON THE CHILD IT LAUNCHED**: a `-stdlib` converter OUTLIVED its reaped wrapper
  and kept writing, so the staged trees were already full and a diff taken at the kill would have read
  a confident ZERO — the emitted-before-seeded retraction reached by a second route. Continuing after
  such a kill is legitimate ONLY because the run's sentinel files survive (the written-this-run
  assertion can still be made per arm and target): **wait on the CHILD's pid POSITIVELY from
  PowerShell** (never bash `kill -0`, never `Wait-Process`), then RE-ASSERT both arms' written counts
  before diffing. (Two lanes' independent arms reproducing the same per-target emission counts —
  1,656 / 1,724 / 1,727 — is a cross-check on the instrument worth naming, not a coincidence to pass
  over.) And **stopping a background TASK kills the task's SHELL, not the chain it launched**: an
  assembly script survived as an ORPHAN, its killed leg returned looking finished, and the relaunch
  put TWO chains in one worktree writing interleaved stamps into one log — every reading discarded.
  **A stop is a TREE kill by parentage from the chain's ROOT** (`taskkill /T` on the root pid — and in
  PowerShell `$pid` is a RESERVED automatic variable, so a hand-rolled walk that assigns it silently
  kills nothing), verified by a census that names ZERO survivors; and **no process sweep by executable
  NAME while a sub-agent shares the box** — the pattern that matched the chain's converter also matched
  a sub-agent's two-seeded-diff arm, twice in one morning.
  **⚠ WHY that slot is censused by the PARENT processes and NEVER by `go2cs.exe`** (measured
  2026-09-04). A CNR re-transpiles every behavioral dir by spawning one SHORT-LIVED converter per
  package, so between any two packages there is a real interval with ZERO `go2cs.exe` alive: a binary
  census reads FREE hundreds of times during a run that holds the slot, and a lane waiting on
  `while (Get-Process go2cs)` would have dropped a `-tests` pipeline into a running battery on its
  first sampled gap (900 s of luck before it was caught). The holder of a slot during a CNR has no
  converter of its own most of the time. **Census the HOSTS** — `powershell`/`pwsh`/`BehavioralRunner`
  whose command line names `check-no-regression`, `run-behavioral`, `run-validated-sweep` or
  `BehavioralRunner` — excluding the querying process and the lane's own tree, POSITIVE-CONTROLLED
  before it is trusted (with N holders live it must report N), with the launcher RE-CHECKING
  immediately before it starts and refusing on a live parent, which is what makes the rule cheap.
  ⚠ **BUT `CommandLine` IS NOT RELIABLY READABLE FOR EVERY PROCESS, so a filter over it answers ZERO
  and reads exactly like a dead run** (2026-09-05, three times in one night against a run that was
  demonstrably alive — its converter executing, its emission dirt growing). **Census by process NAME
  UNFILTERED, or corroborate with the run's OWN artifacts** (emitted files, log mtime), before
  believing a zero: the same shape as the bash-`kill -0`-on-a-Windows-pid and `pgrep`-self-match traps
  above, and the reason the positive control on the host census is not optional. Its other half, from
  the same week: **classify a background shell by its COMMAND LINE — the parent bash's, via
  `Get-CimInstance` — and NEVER by its RHYTHM.** A coordinator's task list shows SUB-AGENTS' background
  shells beside its own, and eight staggered 60 s poll loops were classified as "monitor iterations"
  from their cadence when one belonged to a sub-agent; a sub-agent's poller dies with the sub-agent.
  ⚠ **And a CLAIM is closed by its owner's release POST, never by an absence somebody else observed**:
  `Get-Process go2cs` = 0 and a MISSING `go2cs.exe` between two legs of a gate chain is exactly what a
  REBUILD looks like (`go build` deletes and rewrites the binary; CNR rebuilds it unconditionally), so
  a lane reading them as a release can start an hour-long run under a claim that is still live — the
  other sibling-misread being the inverted `exit $count` poll above. A claim past its stated size is
  re-read by ASKING its owner, not by inference.
  ⚠ **The serial order relaxes on MEASURED properties, never on impatience** (2026-09-04): it protects
  exactly ONE property — a refusal branch that trips on a per-project transpile TIMEOUT — so CNR (no
  per-package budget), separate worktrees (the r41 hazard is per-ROOT), a `-stdlib` A/B pinned at
  `-convert-timeout 90m` that cannot be pushed past its cap, and a corpus-scale control carrying a
  raised transpile budget plus a "timeout-shaped refusal re-runs SOLO before belief" rule may overlap
  them; what stays serial is anything WITHOUT those guards. What binds across worktrees is LOAD, which
  is why an A/B under concurrent lanes passes the same `-convert-timeout` to BOTH binaries and every
  loaded run records its WALL beside its verdict — a loaded wall can only produce a false red. And **a
  `-stdlib` census is never PARKED**: it can only be re-seeded from scratch, throwing away completed
  targets. ⚠ One census trap from the same family: **a process census keyed on a WORKTREE NAME
  over-matches its PREFIX** (`sub-q2` inside `sub-q23`) — the glyph-substring rule in a new costume.
  **⚠ A watcher keyed on a background task's OUTPUT FILE cannot see a chain whose stdout is
  REDIRECTED to a log** (2026-09-03): the task output is 0 bytes, the trigger never fires, and the
  watcher waits out its whole timeout looking healthy — route #6's shape, one layer out. Key a watcher
  on the artifact the watched process actually WRITES, and check the watcher's own log for its trigger
  line rather than assuming it armed.
  **⚠ A battery's VERDICTS must land in the log the watcher tails, or a red leg is INVISIBLE**
  (2026-09-04): one train's assembly log stamped only leg BOUNDARIES while the CNR's `exit=1` and its
  one CHANGED file went to a per-leg log nothing watched — the boundary stamp read as progress, the
  restore stamp erased the drift, and the red sat unseen for half an hour until the leg log was read
  by hand. **Every leg stamps its EXIT CODE and its one-line verdict** (the count, the changed set)
  into the assembly log, the monitor's pattern includes `exit=[1-9]`, and a leg that CONTINUES past a
  failure by design (so later legs' verdicts transfer when the remedy is a golden) says so in the same
  stamp.
  **⚠ Three 2026-09-05 sharpenings of that stamp, and the first is the FALSE-RED twin of the
  verdict-word false green.** **Gate on the CODE the instrument prints (`GUARD exit=[1-9]`), never on
  a WORD in its prose**: a launch gate spelled `grep -icE 'guard.*(FAIL|RED|MISMATCH)'` counted an
  honest `ROSTER GUARD exit=0 (0 fail lines)` as a failure and refused a clean launch. **A leg that
  keeps the runner's output in MEMORY and stamps only summary lines leaves a `Failed: 1` with no
  NAME** — write the WHOLE output to a per-leg detail file and stamp every `Failed <test>` line into
  the assembly log beside the exit code. And **a monitor built on `tail -F <log>` can die SILENTLY**
  (twice in one day: its tail process gone, 0 events, the task still listed) — a watch whose death is
  indistinguishable from quiet. Watch a chain with SINGLE-NOTIFICATION background waits
  (`until grep -q '<stamp>' log; do sleep 60; done`) re-armed per leg, census a monitor's tail by
  command line before trusting its silence, and kill stale tails by PID — they hold the log open
  across sessions.
  **⚠ A ROW-LIST DRIVER GUARDS `processed == listed`, PRINTED AND FLAGGED** (2026-09-04): PowerShell
  ate the loop's stdin, so a 3,643-verdict row was swallowed WHOLE and the next row arrived as
  `atabase/sql` — no error, no failure, a 21-row sweep that would have reported as 23 (the fix is
  `< /dev/null` on the child, with the row list on FD 3). Four companions from the same driver: a
  verdict grep ANCHORED AT COLUMN 0 misses the sweep's INDENTED verdict line, so every passing row
  read as a fault — and the fallback printing `NO VERDICT LINE -- instrument fault` rather than
  defaulting to a verdict is the right shape (a control runner whose arms yield no verdict line ABORTS
  the arm; four green arms in a row is the tell, not a pass); the driver RESTORES after EVERY row, not
  at the end (54 files of dirt after two rows); it REFUSES to start on a dirty tree or with a
  `go2cs.exe` alive (a relaunch started at dirty=14 because the killed run's children were still
  writing); and **a harness `TaskStop` kills the harness TASK, not the detached bash → PowerShell →
  converter chain beneath it** — kill by process ANCESTRY keyed on `go2cs.exe`, which a probe never
  spawns (two probes matched their own querying shells).
  **⚠ A measurement taken ACROSS a host SUSPENSION is not a measurement** (2026-09-03): a laptop going
  lid-closed inside `net/http` fabricates exactly the mid-stream death signature that row instruments.
  The honest choice is a clean tree-kill by VERIFIED PARENTAGE (22 processes; a bare `go2cs` kill
  orphans the host and locks `runtime.dll`) and **no record**, then a relaunch behind a readiness gate
  (pinned toolchain answering, network up, no converter alive, clean tree) on an ABSOLUTE-deadline
  wait that re-reads the clock, so it fires on wake rather than during standby.
  **⚠ DISK is a gate input, and a battery preflights its own floor (measured 2026-09-03).** A train
  battery's TAIL legs — runner, sweeps, pair, `reflect` — all aborted in ONE MINUTE on the sweep's
  disk preflight (18 GB free against a 25 GB floor) while the chain itself EXITED 0: seven sub-agent
  worktrees plus one leg's own 21 GB of behavioral build output crossed the floor mid-battery, the
  chain's exit code carried nothing, and the leg logs carried everything. Three rules. **A battery
  carries a disk-floor preflight of its OWN before its first leg, and a chain's exit code is the OR
  of its legs.** **The instrument is a per-worktree `bin`/`obj`/`Generated` SIZE census, not a
  directory count** — the box's largest build output (87 GB) sat in the MAIN checkout nobody had built
  from in two days while two batteries fought over the last 20 GB; a checkout is purged only after its
  newest build-output mtime AND a process census both say idle. And **`src/clean-bin.ps1` asks a
  `Read-Host` confirmation**, so a non-interactive invocation CANCELS silently (exit 0, nothing
  removed) — purge with a direct `Get-ChildItem -Include bin,obj,Generated -Recurse | Remove-Item`,
  ⚠ scoped so it does not match `src/go2cs/bin` (the standard `find … -name bin … -exec rm -rf`
  idiom DELETES `src/go2cs/bin/go2cs.exe`; it failed loudly with rc=127 that time), and re-verify the
  converter exists at the invoked path after any purge.
  ⚠ **A BATTERY LEG REFUSED BY ITS OWN PREFLIGHT STAMPS A PLAUSIBLE-LOOKING VERDICT LINE**
  (2026-09-05): the sweep's 25 GB disk floor refused at 20.2 GB free, the leg stamped "0 records
  preserved" and an empty `PAIR:`, and the chain rolled on into two more UNMEASURED legs — the tell
  was the CLOCK (22 rows in 24 seconds). **A chain reads every leg's log for its REFUSAL markers and
  STOPS on one** (now wired: a DISK PREFLIGHT line in the sweep's log stamps the leg UNMEASURED and
  exits 3), and **a leg's stamp carries its ROW COUNT** so an empty leg cannot read as a green one.
  Disk is a battery INPUT: census free space before a train launches AND after its full suite (the
  per-project bins are ~20 GB), and purge the behavioral bins the moment the Output phase ends.
  ⚠ **And a chain whose FULL-SUITE leg FAILS must not roll into the SOLO COST leg** (2026-09-05, a
  train chain that did): the SWEEPS after a red suite are still data — they read the same root row by
  row — but a cost pair measures COST on a tree that WILL change and must be re-run anyway, so the
  derive STOPS the chain before the pair on suite red. Which legs are still worth running after a red
  is a per-leg judgment the derive encodes, not a blanket continue.
  ⚠ **A TRAIN SCRIPT WITHOUT ITS OWN `cd` RUNS IN THE CALLER'S CWD** (2026-09-05): a rehearsal
  launched from the coordinator's scripts folder ran inside the MAIN checkout at a stale head, and
  the only thing that stopped it merging sixteen seats into the wrong tree was the dirt gate reading
  an untracked `.claude/` folder. **Every derived train script names its worktree in its first lines
  and refuses any other** (`[ "$(git rev-parse --show-toplevel)" = <expected> ] || exit`); a dirt
  gate is a backstop, not the address.
  **⚠ Two more instrument-name traps (2026-09-03).** **PowerShell helper functions named `LP` and `H`
  were NEVER CALLED** — the built-in aliases `lp` (Out-Printer) and `h` (Get-History) outrank
  functions, so a "long-path-safe" comparer returned `$null` for every path and null-vs-null read as a
  clean `True`; only the positive control surfaced it. Run `Get-Alias <name>` before defining an
  instrument helper. And **a fixed-size `tail -N` over an output whose length GROWS with the run is
  not an instrument**: the sweep prints drift AFTER the verdict, the drift list outgrew the tail, and
  a PASSING row read as never having run (exit 0 could not distinguish) — read verdicts from the proof
  page the sweep rewrites (`docs/validation/current/<row>.md`) against the roster, using the page's
  mtime to tell swept from stale.
  Two adjacent PS 5.1 traps the same lanes paid for:
  a repo script's `Write-Host` output goes to the INFORMATION stream, so capture with `*>&1`
  (a bare `2>&1` silently drops every `==>` status line and the log reads as hung); and the sweep's
  `-SkipBuild` expects the converter at `src\go2cs\bin\go2cs.exe` — an outside-the-repo binary path is
  not consulted, so a lane that built elsewhere re-pays the build or copies the exe there first.
  **⚠ The scratchpad directory is SHARED across concurrent lanes on one machine** (measured
  2026-08-15: two lanes both writing `cnr.log` — one clobbered the other's gate log mid-run, and the
  verdict had to be recovered from `git status`). It is session-scoped, not lane-scoped. Prefix every
  scratch filename with your lane/branch name (`<lane>-cnr.log`), and treat an unexpectedly truncated
  or rewritten scratch log as a collision first, a gate failure second. Make the name unique per RUN
  too, not just per lane — a REUSED log path on Windows can splice a fresh run's header onto a stale
  run's tail (file tunneling + partial overwrite) and fabricate readings like "CNR finished in 20 s"
  (measured 2026-08-15). And never census with `grep -P` on this box: it dies with "-P supports only
  unibyte and UTF-8 locales", so with stderr discarded it returns 0 matches and reads as "no sites"
  — a false-empty census that nearly got banked. Use ripgrep (`rg`)/the Grep tool.
  **⚠ Two more, 2026-09-04, and the first is the family's `go test` member.** A NON-VERBOSE
  `go test ./...` prints package-level `ok` lines and **no test names**, so a ladder leg counting
  named-guard results off it reads ZERO by construction — "named-guard-results=0" was the INSTRUMENT,
  not the guards, which ran 11/11 in a filtered verbose run whose own positive control is its eleven
  `=== RUN` lines: **a guard-count leg runs `-v -run <names>` and treats the RUN lines as its
  control.** And **a POSIX bracket expression eats `\[`**, so a GOROOT population of zero taken from
  such a grep is an artifact until the pattern has been made to FIRE on a probe known to contain the
  shape.
  ⚠ **A third member, 2026-09-05:** Git Bash's `grep -F -i -f <patterns>` ABORTS (rc 134, no stdout)
  when one pattern ends in a backslash, and the empty output reads as an EMPTY CENSUS rather than as
  a dead instrument. Use ripgrep, and positive-control the detector on planted lines before believing
  a zero.
  **⚠ The false-empty family has a deeper member: instrumentation that never compiled in (measured
  2026-08-28, the defer-multivalue-spread lane).** A type-aware census was built by patching an
  `fmt.Fprintf(os.Stderr, …)` marker into a converter helper via a heredoc python script, running
  `-stdlib` into a seeded temp root, and counting marker lines: ZERO hits across the whole stdlib,
  and the run looked entirely healthy — the stderr carried the normal spread of converter WARNINGs,
  proving the conversion had really traversed the corpus. The zero was an artifact. The converter's
  `.go` sources are CRLF in the working tree; the script's anchors were LF
  (`"\treturn tuple.Len()\n}"`), so they matched zero times and python's `assert` fired — but the
  script ran under `set -u` rather than `set -e`, so execution CONTINUED and built an
  UNINSTRUMENTED binary. Every downstream step then behaved normally, and the census counted a
  marker that was never compiled in. Two cheap tells were sitting there: the "instrumented" binary
  was BYTE-IDENTICAL IN SIZE to the uninstrumented one, and `grep -c SPREADCENSUS <binary>`
  returned 0 — the marker string was not in the executable at all. The durable rules: patch
  converter sources with the Edit tool (it matches the file's actual bytes), never an LF-anchored
  script — a script that must exist reads/writes with `newline=''` and anchors on CRLF, or
  normalizes first; `set -euo pipefail`, never bare `set -u`, in any instrument whose later steps
  assume an earlier edit succeeded; and ALWAYS positive-control a census before believing a zero —
  run the instrumented binary over a target KNOWN to contain the shape and confirm it fires with
  the expected count (the lane's control fired 12/12 on the behavioral guard's spread rows and
  stayed silent on its two controls, which is what made the real — also zero — production-corpus
  reading trustworthy). Same family as the `grep -P` and bare-`rg` notes: an instrument that
  cannot fail reports success over a hole.
  **⚠ The general form, named after five instances in ONE day (2026-09-01): an instrument built out
  of the thing under test cannot independently measure it — the corrective is a SECOND
  DERIVATION.** The sharpest instance is structural: **a `-stdlib` census answers "how much does the
  corpus change" and is BLIND to "does the fix reach the row" whenever the motivating site is in a
  `_test.go` file** — `-stdlib` never writes test emission, so those are two questions and they cost
  two runs. The others rhyme: a probe keyed on its own incomplete predicate reports the predicate;
  a probe blind to unwired slots reports the wiring; a type-name census was believable only once a
  `go/parser` derivation reproduced it, and a classifier's 66 only once an independent predicate
  reached the same number. Four corollaries, all measured 2026-09-01/02. **Name the LAYER a census
  is attached to**: a `claude/g-*` lookup over bare `g-*` refs returned a confident EMPTY, and a
  working-tree line-ending count under the `eol=crlf` pin was reported as a COMMITTED fact (the blob
  is LF, the checkout makes it CRLF, and `git checkout --` "cured" a state that was never in the
  index) — two retractions in one night, both from an unnamed layer. **An empty enumeration inside a
  redirected log is not evidence of absence** until a second instrument agrees. **An EMPTY diff
  after a "fix" is the fix saying it was not needed**, never the gate agreeing. And **count errors
  with the strict `error (CS|MSB|NETSDK)[0-9]+` pattern only** — a loose `grep -cE 'error '` scored
  1 error on a clean 831-assembly build by matching `internal.oserror ->`, and matched the word
  inside Go type names (`(…, error)`) where a case-sensitive `ERROR` read 0 against the loose
  grep's 140. ⚠ Finally, **a census attaches to the DEFECT's boundary — defined by the EXISTING
  marker set — not to the boundary the dispatch named**: a call-argument census missed two
  composite-literal sites the pointer twin's `anyBoxedPtrArgs` already marks, one of them a live
  defect. Grep the marker set first, and attach at every site it covers.
  ⚠ **And that strict pattern stays SPLIT into TWO numbers — `error CS[0-9]+` and
  `error (MSB|NETSDK)[0-9]+` — never folded into one "errors" count** (2026-09-04): a contention-born
  MSB3030 storm, from a filtered build started INTO a running leg, reads CS 0 / MSB 36 and clears on a
  solo re-run, while a real regression reads CS N / MSB 0 — folding them makes the two
  indistinguishable. Its companion, which is why that storm existed at all: **a lane does not start
  any build into a running gate leg**, however small the guard it wants sooner.
  ⚠ **Two 2026-09-02 refinements from one census that read ZERO against thirteen real sites.** Every nil
  construction of pointer-to-array type in Go 1.23.12 lives in a `_test.go` (reflect 10, runtime/arena 2,
  encoding/binary 1), so the production census of 64 nil-to-pointer conversions found none: **ask the
  `-tests` dimension whenever the motivating site is a test.** Where three derivations disagreed (grep 6,
  an instrument pointed at the grep-NOMINATED packages 11, an independent `go/packages` pass over all std
  packages 13) the disagreement was SCOPE, not predicate: scoping a census with the tool just shown to
  under-report reproduces its blind spot. Then **re-derive the population before any design is cut against
  it**: the 13 split into three tiers (6, 3, 4) and the "most interesting" members were the tier that
  needs nothing — a summary restating a lane's conclusion inherits its unvaried axis, so state what was
  MEASURED, not what was concluded.
  ⚠ **Three more, 2026-09-02, one shape: a property INFERRED from an artifact instead of measured.** A
  census can be exactly right about what EXISTS and exactly wrong about what it MEANS — thirteen
  typed-nil sites counted correctly by two derivations, then classified off the emissions and wrong
  twice (what the named spelling preserves is C# TYPE IDENTITY, not the dimension). **A converter hook
  that FIRES is not a hook that CHANGES the emission**: `getExprContext` returns the FIRST matching
  context, so cargo APPENDED as a second one is unreachable while the instrumentation reads healthy —
  instrument, then DISBELIEVE the instrument's agreement with the emission. And **a utility that exits
  0 with NO output is indistinguishable from one that never found its input**: its zero is a result
  only after a positive control (delete a known line, re-run unchanged, require it byte-identical).
  ⚠ **Three census rules from one shift, 2026-09-02.** **Attribution rides on a caller-supplied TAG,
  never on a stack walk** — a per-admit walk attributed 0 of 14 rows because the frames were inlined,
  while the tag read every row, whatever the walk costs. **A classifier applies the rule its CALLER
  asked**: 70,065 of 70,070 admits came through the marshalling (CONVERSION) callers, so the four pairs
  flagged WRONG under the ASSIGNMENT rule are legal Go conversions — attribute by caller MODE before
  classifying, verify flagged pairs against Go's own predicates, check whether a refusal at a fast path
  is RECOVERED downstream before predicting breakage, and prefer an explicit mode parameter over
  relying on that recovery. **And classify each site by the QUESTION it asks before counting it**,
  because a census can measure an option OUT OF EXISTENCE: 4 of 82 `GetType` uses were raw eface
  type-word comparisons and all four sat in ONE hand-owned file, leaving the converter-emission remedy
  with nothing to emit. A new census is also cross-checked against the HISTORICAL population (70,070
  against 70,071 admits) before its counts are believed.
  ⚠ **A SUBSTRING predicate over converter-minted GLYPH names over-matches BY CONSTRUCTION**
  (measured 2026-09-02): the `Δ`/`ж`/`ᴛ` families are prefixes of one another's identifiers, so
  `ΔHandle` matched inside `ΔHandler` and eight census hits were never real. Anchor on the WHOLE
  alias, or resolve what the name denotes — the alias-census rule above, one layer down. Its
  companion: **"carries the alias" is not "drifts on the other platform"** — only a transpile,
  mtime-verified, answers the class question, and the drift-measured number was one.
  ⚠ **And a census of an emitted SPELLING under-reports by every spelling it did not enumerate**
  (2026-09-05): a retention census counted `FromPinnedBox(` and missed `FromBox(` — the
  reference-bearing spelling, which is exactly where the token-route boxes that most need retention
  live — and missed pointer-typed VARIABLES entirely, while the converter's own predicate is
  `go/types`' and reached them all. The alias-census rule one spelling over, with a sharper
  corollary: **a guard keyed on the SAME spellings as the census is not a second derivation** — it is
  the census run twice, and it shares the blind spot.
  ⚠ **So an EMISSION grep is an UPPER BOUND that misses every spelling it did not think of, and a
  census keyed on ONE MINT OR HELPER is a LOWER BOUND on that shape** (both 2026-09-05). The first:
  173 `heap(new T(), out var)` sites against **292** once `ref var n = ref heap<T>(out var)` was
  counted — the population is a `go/types` census over the SOURCE, per flavour, positive-controlled on
  a probe carrying one site per kind plus the excluded shapes; and the DIFFERENCE between the two
  numbers (396 address-taken scalar locals against 232 boxed on one flavour) is itself a MEASUREMENT
  of an existing capability's reach — 164 already kept unboxed by parameter ref-lowering, read for the
  first time as a by-product of sizing the next one. The second: twenty sites resolved through a
  pinned-box mint missed the emission that took FOUR banked rows down, because a plain `(uintptr)`
  conversion of a heap box is a SECOND DOOR into the same class and never touches the helper. **A
  helper-keyed count answers "how many go through THIS door" and READS as "how many exist" unless the
  record says otherwise.**
  ⚠ **Five census rules from 2026-09-04; the first two are about how a ZERO and a LEG are believed.**
  **A NARROW census's zeros are believed only because the SAME detectors read NON-ZERO on the BROAD
  population in the SAME run** — three exclusions predicted at ~30 combined came back at exactly ZERO
  over one capability's 220 sites, and were trusted because those same detectors read 19, 45 and 74
  across the 859-site population beside them: the broad population IS the narrow census's positive
  control, since a dead detector reads zero on both. The prediction's own inversion is the other half
  — the row named as the biggest exposure came back 14 against a reasoned ~15, while the three rows
  merely ASSUMED were wrong by their whole size — so **hedge what you assumed, not what you reasoned
  about, and let a prediction table say which rows are which.** **A TWO-LEG census is scored from TWO
  legs or it is not scored**: one leg read a headline row unmoved and a falsification went out with an
  honest scope caveat naming the unread leg — which is where the answer was — and a scope caveat tells
  the READER a finding may be wrong without telling the AUTHOR. Read every leg before scoring, and
  post the correction the moment the second leg reads. **A ratio reading ABOVE ONE is the instrument
  saying its counters measure different POPULATIONS**: one counted slot-allocating standard-box
  constructions while the other counted pins taken through the `ж<T>` base and so fired for element-
  and field-reference boxes too (183%, 116%) — the remedy is per-kind attribution, positive-controlled
  with the ZEROS PRINTED (7 takes → standard 7, elem 0, field 0) before any row is read. **A count's
  POPULATION is named by TYPE and by STACK before a design's gate row or its cost canary is chosen**:
  a measured population of timers and sockaddr field boxes — consumers, never victims — sat in a
  different class from the one the falsifier named, which moved the gate row, moved the canary, and
  exposed a DEAD TAKE whose result nothing reads (**a take whose number nothing reads is DISPLACED,
  not registered**). And **an instrument built out of the thing under test is refuted by its own
  TOTALS**: a consumer-side test of a recovered box's `IsNative` to drop stale label pointers dropped
  ALL 91 labels, because `IsNative` is the NORMAL state of that number and not evidence of staleness —
  caught only because the guard PRINTED its warning count (182 drops where at most one was expected).
  There is no cheap consumer-side test separating a live address from a dead one; the honest form
  WITHHOLDS the half it cannot guarantee and writes the witness into the hand-own.
  ⚠ **Two more, 2026-09-05, both about WHICH population a census counted.** **A defect measured at
  ONE SPELLING can have a SECOND LAYER at another, and the population can live ENTIRELY behind the
  second**: an empty-composite-literal fix was correct for its spelling and moved ZERO sites, because
  all five std needy named arrays are built by the ZERO VALUE through the wrapper's lazy backing —
  so **census the CONSTRUCTION SITES at the Go source before choosing the layer to cut**, and say the
  expected zero out loud before an empty diff can read as a pass. (A census instrument reading 0 on
  the probe module KNOWN to contain the shape is a broken loader, repaired before any number is
  believed.) And **a class census reports EMITTED sites and REACHED sites as TWO numbers** (21 / 2):
  the REACHED subset is what a cut displaces now under the population-of-one rule, while the class
  remedy is RECORDED with the census as its sizing and BUILT when a second row reaches a third site.
  ⚠ **Two more, 2026-09-05, and the first is why a GOROOT census and an EMISSION census are different
  numbers.** **A census over GOROOT with `go/packages` counts types the EMITTED corpus never carries**
  — `-stdlib` defaults to `-tags purego`, so asm-path declarations (nistec's `p256AffineTable`) are
  absent from every GOOS folder and reference-element variants replace struct ones, which is how a
  five-site GOROOT census read TWO latent sites in the corpus. **Census the EMISSION, every per-GOOS
  folder**, cross-check against an independent count, and positive-control the instrument on a shape
  known present before believing its number. Its selection corollary: when a defect appears at N site
  KINDS, **measure which EMISSION each funnels through before choosing a carrier** — 13 of 15 divergent
  lines passing through one generated property decided a carrier over a converter patch covering four.
  And **a census arm that runs over a HAND-TYPED SUBSET is not a census arm**: it reports on the
  population while testing the sample, and the shortfall is invisible because every row it prints is
  TRUE (a by-address remediation arm asked 9 of 38 members, so 29 were never asked; the sibling arms,
  keyed on the converter's OWN recorded decision plus a field scan, stood).
  ⚠ **Four census rules from 2026-09-06.** **The strongest answer to "is this helper sufficient" is
  "the shape set is CLOSED", and a LANGUAGE FACT can prove it where no census can**: Go declares the
  socket-address interface with an UNEXPORTED method, so no package outside its own can implement it —
  three arms in the writer, three method definitions in the tagged sources, three implementation
  records in the converted corpus, all agreeing. Look for the closure proof before counting instances.
  **An EMPTY census is actionable only in its STRONG form — what the population DOES, not what it
  lacks**: a caller-side by-address census found every one of four hazardous caller behaviours PRESENT
  and repeated across 43 reference-bearing sites of 80, each answered by a named mechanism the
  companion was written against; "I found nothing" would have closed nothing. **Any claim about a SET —
  a map's members, a registry's population, a census's rows — is derived from the WHOLE construct by an
  enumeration that PRINTS A COUNT, and the count goes beside the members wherever the claim is made**:
  a fixed-context search window is a window with a NUMBER on it and a number looks like completeness,
  which is how a registry was enumerated from a 25-line context window showing six of its eight rows.
  Two of that evening's three same-shaped failures would have died at the count step, because six and
  eight are visibly different numbers, where a resolution to be careful would have caught none of them
  — the head-limited status check's family, one layer up. And **a comparison whose SELECTOR matched
  nothing is not a comparison, and it fails as a FALSE POSITIVE rather than as an error**: a row-lookup
  pattern matched nothing and the fallback silently hashed the WHOLE FILE, so two refs' hashes differed
  for reasons that had nothing to do with the row, caught only because they differed where they should
  not have. Redo such a comparison by locating the target on each side's own content.
  **Scope DELETES by lane prefix too, not just writes** (measured 2026-08-16: a lane's cleanup swept
  the whole shared scratchpad and unrecoverably deleted sibling lanes' artifacts). A cleanup command
  must name your own `<lane>-*` files; `Remove-Item <scratchpad>\*` is a cross-lane destructive act.
  **⚠ Killing `go2cs.exe` alone ORPHANS its `dotnet run` child and the test host under it**, which
  keeps `runtime.dll` locked — the NEXT pipeline run then fails MSB3027/MSB3021 and its comparison
  reports `Go="pass" C#=""` for every test, reading exactly like total conversion failure (measured
  2026-08-16, cost one invalid run). It is a file lock: kill the process TREE by verified parentage,
  then re-run before believing any mass-empty verdict.
  **⚠ `dotnet build-server shutdown` is ALSO machine-global** (found 2026-08-03: one lane's startup
  cleanup yanked the shared MSBuild servers out from under a sibling's in-flight compile — same
  truncated-log signature, no Stop-Process anywhere). While sibling sessions may be building, do NOT
  run it; isolate your own builds instead (`MSBUILDDISABLENODEREUSE=1`, `-p:UseSharedCompilation=false`)
  and reserve `build-server shutdown` for solo contexts or coordinator-owned quiet points. The repo's
  own instruments are safe by default since 2026-08-08 (`db427e6e9`): `run-behavioral-tests.ps1` runs
  its shutdowns only under an opt-in `-ShutdownBuildServers` switch, and `AssemblySetup`'s teardown
  honors the same env-var contract — the hazard that remains is the ad-hoc, hand-typed invocation.
- **Faster alternative to MSTest — the standalone runner `src/tests/Behavioral/BehavioralRunner`
  (2026-06-30).** A dependency-free console app that runs the same four phases over every behavioral
  project but is **not** hosted in testhost, so the
  self-lock failure mode above is structurally absent. It collapses the per-project `dotnet build`
  calls into one parallel MSBuild invocation (pre-building the ~31 shared `golib`/analyzer/`core/*` deps
  sequentially first to avoid the parallel-build MSB3026/27 race, then fanning out). **All green**, at
  parity with MSTest — the parallel MSBuild invocation keeps wall-time from
  scaling linearly with project count. Drive it via **`run-behavioral.ps1 [--filter X]
  [--phase transpile,compile,target,output] [--update-targets] [--list]`**. Only output-compared
  (`[GoTestMatchingConsoleOutput]`) projects are `go build`- and stdout-compared, matching MSTest
  (library-style projects like `Constraints` have no `package main`). For a pure converter no-regression
  check with no compile/run at all, use **`check-no-regression.ps1`** (re-transpiles every behavioral dir
  and `git status`es the converter-emitted `.cs` **and `.csproj`** — the transpile rewrites both, and the
  `.cs`-only pathspec it had until 2026-08-08 made a csproj-emission change invisible on every platform.
  Converter stderr is captured, not discarded: a package the run could not fully regenerate — best-effort
  "did not fully type-check", a recovered "visit file error", or a non-zero exit — fails the gate by name
  as **NOT MEASURED** even with a clean `git status`, so the byte-identical verdict is never vacuous;
  other WARNINGs are counted as advisory, never fatal. Coordinator ruling 2026-08-08, from lane r48b's
  Linux `FindFirstFileData` finding — see `docs/PLAN-linux-operation.md`. Until F8 platform-gates the
  enumeration, a Linux CNR run therefore reports `FindFirstFileData` as NOT MEASURED by design).
  ⚠ **A converter that EXITS 0 on a DEGRADED emission makes "the exit code" a false-green predicate in
  every harness that asks it** (2026-09-04), which is why the classification of a converter stderr line
  lives in ONE linked predicate the harnesses share rather than being re-derived per instrument — the
  same shape as `ConverterBuildInputs` for the staleness set (route #5). ⚠ And **two harness statuses
  whose REMEDIES are opposite are separate members**: a budget that expired wants MORE BUDGET, a
  best-effort conversion wants a HOST THAT CAN TYPE-CHECK. A report that cannot tell them apart — or a
  remediation hint naming one remedy for both — sends the reader to the wrong fix, so the hint names
  BOTH or neither.
  ⚠ **The class bites in BOTH directions now (2026-09-02): a behavioral guard written against ONE
  platform's syscall API cannot type-check on the other and turns THAT host's CNR red by name.** A
  lane's own-platform CNR green says nothing about the other host's gate — the union battery there is
  where it surfaces. F8 landed with train 11 (2026-09-02): a converter-preserved
  `[GoPlatformExclusive("<goos>")]` marker in `package_info.cs` naming the native platform(s), plus a
  LOUD skip-by-name BEFORE transpile in every enumerator (CNR, `BehavioralRunner`, MSTest as
  `Inconclusive`), its gating set DERIVED from the other platform's NOT MEASURED list (six
  windows-native, `ScmRightsSeam` linux) and positive-controlled both ways; commit markers before any
  CNR `-Revert`, which destroys uncommitted ones. Worse, a best-effort conversion on a
  NON-native host REWRITES the package's csproj and `package_info.cs` (the stdlib ProjectReferences and
  import aliases drop when the type-check that supplies them fails), so a Windows CNR POISONS a
  Linux-only behavioral package and every later leg of the chain measures the poisoned file — 5
  CS0246/CS0234 reading as a missing-reference regression. A chain therefore RESTORES behavioral dirt
  (`git checkout HEAD -- src/tests/Behavioral`) between CNR and any build leg, and F8's skip must
  precede the converter. Such a guard also carries a `runtime.GOOS` early-out as `main`'s first
  statement (raw `syscall.Socket` panics on Windows without the WSAStartup `net` performs), goldens
  stay WINDOWS-generated, and a Linux CNR-EQUIVALENT's DRIFT column is noisy by construction — the NOT
  MEASURED column is the honest one there.
  ⚠ **F8's consequences, measured at its landing (2026-09-02).** The marker has TWO halves — the
  harnesses' skip on a foreign host AND the `go2cs.slnx` UNREGISTRATION (the solution has one Windows
  flavour, so a non-windows-native package cannot compile there on any host) — and
  `check-solution-integrity.ps1` is the one gate that sees the second: run it. A platform-exclusive
  guard's golden (`.cs.target`) and its four MSTest entries are therefore verified ONLY on a
  native-host leg — on the other host F8 skips every phase, Target included, and CNR is transpile-only
  — which is how `ScmRightsSeam` landed with neither and nothing could see it. The same marker also
  covers a package that type-checks everywhere but FAULTS at run time (`LocalTimeZone`'s kernel32
  call): an Output-phase exclusive. The OTHER cross-platform class is ACCEPTED, not gated — a package
  that runs meaningfully on both platforms whose emission differs only by the `Δ`-alias flavour
  (`EnvironBlockWalk` and `SendtoSeam` — the class is EMPTY at master as of C2's marker seat, landing
  with train 14, and the COUNT is what retires there: the derivation below stands, because the next
  platform-varying guard brings the next member. Its two members were remediated by OPPOSITE
  mechanisms. `EnvironBlockWalk` is WINDOWS-native with a golden captured on Windows and read on
  Linux, so it takes the `[GoPlatformExclusive("windows")]` marker; `SendtoSeam` is LINUX-native with
  a golden captured on Windows, so it was REGENERATED on its own platform and marked linux
  (`e731145b7c`, train 12). **The remedy is decided by whether the package's NATIVE platform matches
  the host that captured its golden — never by how the drift looks in a diff.** The MECHANISM stays
  doctrine whatever the count: a generated ADAPTER TYPE NAME in production `.cs` follows the imported
  alias — `SockaddrInet4жΔSockaddr` on Windows, `жSockaddr` on Linux — measured against a master
  control with identical numstat on both trees; a follow-up census naming a third member was a glyph
  SUBSTRING over-match, `ΔHandle` inside `ΔHandler`, its transpile byte-identical. A class claim is
  re-derived at the TIP before it is quoted) is
  NAMED beside the package, with a standing Linux-CNR derivation (CHANGED files whose whole diff is
  the alias hunk or the adapter-name hunk) so its members surface by census rather than one at a time;
  a Linux CNR's honest verdict on this corpus WAS "clean modulo the windows-alias class" until C2's marker
  seat landed with train 14 — since then it is "clean" with no modifier (measured at `038c87786e`: 688
  byte-identical, 8 platform-exclusives skipped by name, 0 NOT MEASURED).
  ⚠ **The `.slnx` exemption criterion is platform-exclusive AND not-windows-native** (stated
  2026-09-02): the solution has ONE Windows flavour, so a `linux`/`darwin` marker unregisters the
  project and a `windows` marker changes registration not at all. A guard's own analogy check caught
  that in seconds — read the criterion's second half before predicting a registration change.
  ⚠ **F8's class ONE AXIS OVER — ARCH, and it is decided by the ORACLE (2026-09-04).** The arch a
  census transpiles for is the RUNNER's host arch: no harness passes `-platforms` and the converter
  defaults to `runtime.GOOS/GOARCH`, so two census legs on one corpus can differ at TRANSPILE while
  both compile the committed corpus identically. A behavioral project whose own **Go source does not
  BUILD on an arch is arch-exclusive by the oracle** — `go run` cannot build it there — so no layout
  dimension and no emission change can make it measurable without hardware to capture goldens;
  skip-by-name is the remedy a fleet can implement AND verify. **The acceptance number of a
  skip-by-name cut is N−1 measurable with the skip NAMED, never N** — a dispatch quoting the old N
  would read the fix as a failure.
  ⚠ **What F8 is NOT for: a flavour with no RUN layer** (2026-09-05). A guard for such a flavour is
  neither a `[GoPlatformExclusive]` row skipped fleet-wide (a guard that cannot go red ANYWHERE — a
  coverage claim the fleet cannot honour) nor a text grep over the companion (route #8). It is an ARM
  of the ACCEPTANCE PROBE that runs on that flavour's CI legs, asserting the PROPERTY (here:
  `Wait4(-1)` → ECHILD after a failed Foreground start with a non-tty `Ctty` forcing ENOTTY) with a
  neuter and a SHA-identical restore. And a 548-line hand-own companion's MISSING `using` is invisible
  to the registration ledger and to the emission diff alike — **only that flavour's own BUILD sees it,
  which is why per-flavour builds are battery legs.**
- **The emitted corpus's project-reference graph must be ACYCLIC, and that is now asserted on every
  CNR run (2026-08-30).** `check-solution-integrity.ps1` — CNR's preflight — DFSes the `src/core`
  `.csproj` graph once per `$(GoTargetOS)` (windows, linux, darwin: the per-GOOS `<ItemGroup>` blocks
  make each target a *different* graph) and requires 0 cycles, naming every cycle it finds. A C#
  project reference is a **compile-time** edge, so a cycle is MSB4006 and every project on the path
  stops building; Go's own imports are acyclic by construction, so the only thing that can create one
  is a reference the converter introduces that Go's graph does not contain — a `//go:linkname`
  forwarding property, which points wherever the directive names, in **either** direction. That is
  W1 (`docs/phase4/DESIGN-linkname-push-cycles.md`): a `-tests` conversion of `runtime` emitted
  `runtime → internal/syscall/windows`, and since Go's own imports contain
  `internal/syscall/windows → syscall → runtime`, **no conversion order can undo it** — the emitted
  edge itself has to go. The invariant this makes mechanical is narrower than "`-tests` must not
  rewrite the production emission" (which the four standing closure families contradict) and sharper
  than "the push must not add a reference": **a `-tests` conversion's production emission may differ
  from `-stdlib`'s only in ways that do not change the project GRAPH.** Positive control, kept as a
  parameter so it needs no tracked-file edit:
  `./check-solution-integrity.ps1 -TargetOS windows -InjectReference 'runtime=internal/syscall/windows'`
  must print exactly the six W1 cycles and exit 1.
- **Run the behavioral suite via the solution, not the project:** `dotnet test src/go2cs.slnx`. Running
  `dotnet test` on `BehavioralTests.csproj` directly breaks because `$(go2csPath)` (→ `$(SolutionDir)`)
  has no solution context, so the `core\golib` ref fails to resolve. The baseline solution is now an
  **`.slnx`** (`src/go2cs.slnx`); `src/go2cs-stdlib.slnx` is ALSO `.slnx` — auto-generated by the converter's `-stdlib` run (solutionGenerator.go) with solution folders mirroring the Go package namespaces. Since the trees unified its project paths match the repository's, so a fresh one is adopted by **copying it from the output root verbatim** (no rewriting; verified byte-identical). The old hand-maintained classic `.sln` is retired.
- **VS prompts to save `go2cs.slnx`/`go2cs-stdlib.slnx` on EVERY open — expected, harmless, and
  unfixable at the file level (bisected 2026-08-06, ten probe solutions).** Any `.slnx` containing a
  project that imports a `.projitems` shared-items file (`golib` and `go2cs-gen` both import
  `core/go2cs/go2cs.projitems`, the Symbols shared project) is marked dirty by VS's shared-project
  bookkeeping: classic `.sln` serialized it (`SharedMSBuildProjectFiles` section), `.slnx` has no
  element for it, so the model always differs from the parsed file while every save — including
  Save-As — writes **byte-identical** content (hash-verified; SolutionPersistence 1.0.52 round-trips
  both solutions exactly, and that is the version VS ships). Accept or dismiss the prompt, nothing
  changes on disk either way. Do NOT re-diagnose this as file drift, a generator formatting defect,
  or a reason to restructure the Symbols import. (Upstream: filed as
  [vs-solutionpersistence#156](https://github.com/microsoft/vs-solutionpersistence/issues/156) —
  the format has no shared-items element as of 1.0.52; if it gains one, or VS stops dirtying
  non-serializable state, this caveat retires.)
- **When iterating on regression work, use FILTERED + `--no-build` tests — don't run the full suite each
  time.** The full `dotnet test go2cs.slnx` rebuilds all **502** registered projects first and can take
  10+ min or hang under Visual Studio lock contention. Instead, from `src/tests/Behavioral/BehavioralTests`, run
  `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"` — that reuses the existing test
  assembly and runs just that project's 4 phases (Transpile/Compile/TargetComparison/OutputComparison) in
  seconds. `--no-build` is valid as long as the `*Tests.cs` files haven't changed (`git status` them).
  Reserve a single full-suite run for final confirmation. Faster still for a pure no-regression check:
  re-transpile every behavioral dir and `git status` the `.cs` + `.csproj` — byte-identical generated code
  ⟹ identical compile+output ⟹ identical results, with no compile/run at all.
  ⚠ **A converter EMISSION change is measured by CNR BEFORE it is seated — a filtered behavioral run
  cannot see a project it does not build** (measured 2026-09-02). A seated commit's PREMISE was false
  (golib has `Func`-shaped defer overloads at arities 1–16; only arity 0 lacks one), so its rung
  rewrote corpus emission for a reason that does not exist and carried no footprint; the drift surfaced
  only when CNR ran for the NEXT commit and reported `DeferTypelessReturns` drifting on the rung alone.
  A lane that falsifies its own seated commit posts the HOLD before finishing the measurement.
- **Budget each command against its MEASURED baseline — the old flat "~3 min" cap is no longer right for
  the full runs (re-measured 2026-08-04 by r40, corpus at 569 transpiled packages / 571 registered `.csproj`).** The
  corpus keeps growing (371 → 457 → 518 → 543 → 569 packages), and both full instruments
  legitimately exceed three minutes. Timeouts must clear the real number or a healthy run gets killed
  mid-flight (a 600s ceiling killed a *passing* full suite once). ⚠ **These are DESKTOP numbers —
  and that desktop (the i9-13900K) DIED of hardware failure on 2026-08-09.** The
  same repo is also worked from a laptop (Ryzen 7 PRO 6850U, a 15–28W mobile part), where the parallel
  MSBuild phases run materially slower — a full behavioral suite measured **1,792s** there on 2026-08-07
  with nothing else running. A run over the table on the laptop is the machine, not corpus growth: do
  **not** re-baseline these rows from a laptop run, and size timeouts from the top of the range.
  ⚠ **The replacement coordinator machine (2026-08-10) is an i7-5820K — 2014 Haswell-E, 6C/12T,
  32 GB — and runs the table's rows at roughly 3–4x the i9 numbers.** Measured there on day one:
  full behavioral suite **2,820–4,131s** solo (the 4,131s end was a cold-ish tree; **2,820s**
  re-measured 2026-08-10 — either end is well over the table's 1,575s ceiling), CNR **1,505s** solo / **~3,190s** with two
  sibling lanes, converter `go test ./...` **200s** solo / **332s** loaded — ⚠ and go test's own
  DEFAULT `-timeout` is 10m, which a loaded run on this class now reaches: a healthy suite was
  killed at exactly 600.4s with a goroutine dump that reads like a hang (2026-09-01; 236s solo,
  578s under one sub-agent's load, dead at the wall under two) — pass an explicit
  `go test -timeout 30m` on any box carrying concurrent work, and read a FAIL at ~600s as the
  wall, not the code — full `go2cs.slnx` Debug
  build **1,432s** cold, `archive/zip`'s Debug test suite **774s** (vs 391s on the i9). ⚠ Those
  day-one figures are themselves STALE as the corpus grows — re-measured 2026-08-21 on the same
  i7-5820K: full behavioral suite **~6,552s at 603 packages** (and the runner batch-build default needed **9,000s** at 604 projects -- the stock 2,400s false-redded a healthy run, 2026-08-22), full `go2cs.slnx` Debug
  `--no-incremental` **~3,546s at 722 projects** — so budget those two from the 2026-08-21/22
  numbers and re-measure again at the next corpus jump. ⚠ **The `go2cs.slnx` row re-measured
  2026-08-29 on the same i7-5820K, and the spread is LOAD, not corpus growth: 845s wall SOLO at
  802 assemblies** (`--no-incremental -m -p:UseSharedCompilation=false`, golib rebuilt, 385 corpus
  warnings emitted — positive evidence of a genuine full compile rather than a skipped-work green,
  which is the only reason the number is worth quoting). The tree GREW over that interval (722
  projects → 802 assemblies) while the wall FELL 3,546s → 845s, and no corpus change runs that
  direction — so read the **3,546s as the under-sibling-load end** (it was never recorded as solo)
  and **845s as the current solo baseline**. Budget the row from the loaded end as this table
  always does — ~3,600s, not 845s — and treat a SOLO run materially past ~900s as contention to
  go find rather than work to wait out. Keep the i9
  columns as the historical reference the ratios hang off; budget commands from the i7-5820K figures
  (or 3–4x a row's i9 ceiling when unmeasured), and treat HARD-CODED harness watchdogs as suspects on
  this class of machine — at the old sizes, `PerformanceRunner`'s 600s AOT-publish cap and
  `BehavioralRunner`'s 300s build-all cap BOTH fired on healthy runs here and faked failures (each
  was raised 2026-08-10 with the evidence in a source comment; a timeout is a safety net against a
  hung child, never a performance assumption). Native-AOT perf publishes are the extreme case: ~7s
  each on the i9 in the stub era, **~25 min each** on this machine now that ILC compiles the full
  converted-stdlib closure per benchmark (post-unification), so a full perf run is hours, not
  minutes — and it must run SOLO: concurrent lane load pushed a healthy publish past even an 1,800s
  cap once, and only the Measure phase's numbers are trustworthy on a quiet machine anyway:

  | Command | Measured (warm) | Set timeout | Notes |
  |---|---|---|---|
  | `run-behavioral.ps1` (full, 4 phases) | **~370–1575s (6–26 min; 642s measured 2026-08-07 at 549 projects with a sibling lane converting; 416–957s on 2026-08-05 SOLO at 545 across four r41 stage gates — the spread is warm-vs-cold C# build state, not load; 626s on 2026-08-04 at 544, 1575s on 2026-08-02 with THREE sibling worktrees running pipelines)** | 2100s | 549/549 Transpile+Compile+Target; 523 Output-compared, 26 skipped (no `package main`); the top of the range is concurrent-lane load — budget for it. ⚠ At that load the **Go toolchain itself** can crash building one project (`panic: … compress/flate.(*huff…` inside `go build`) and the runner reports it as a Go build failure; re-run that one project filtered before believing it. Data point 2026-09-01: **1,916s at 652 projects, laptop-class host, SOLO, runner invoked DIRECTLY** (not via the Stop-preference wrapper) with `--build-timeout 10800 --build-one-timeout 900` — the stock 2400s batch cap sized at ~604 projects would have reported the whole corpus NOT MEASURED at 652 |
  | `check-no-regression.ps1` (full) | **~1,050–1,750s (17–29 min; re-measured 2026-08-17/19 at ~625 packages on the i7-5820K: 1,059s and 1,132s solo, 1,440s and 1,711s under sibling-lane load; laptops ran 720s (G) and 1,060s (R). The prior row read 350–510s/700s at 574 packages on the dead i9 — a timeout kept at that figure kills every healthy run on this corpus)** | 2400s | transpile-only, no compile/run; re-transpiles unconditionally |
  | `run-behavioral.ps1 --filter <Name>` | **~10–20s** (8 projects) | default | the iteration loop — use this, not the full suite |
  | `go2cs -stdlib -comments` (full reconvert) | **~195–240s (240s measured r47a 2026-08-08 with two sibling lanes; 223s at r41, 2026-08-05)** | 600s | 307 projects; per-file work is sub-second, the cost is `go/packages`. A three-target `-platforms` merge is ~3x this (545s measured r50a) |
  | single `core` pkg build | **~6s** (log/slog) – **~60s** cold (go/types) | 180–400s | cold includes the dependency chain |
  | full `go2cs-stdlib.slnx` build | **~92–188s** warm (307 projects; 149s measured r50a at `-p:GoTargetOS=windows`, 188s at r41 and 158s at r40, all with `-p:UseSharedCompilation=false`, the isolation flag a lane uses instead of `build-server shutdown`). ⚠ **i7-5820K on a healthy disk: 516s** `--no-incremental` (2026-08-14) | 600s (900s on the i7 class) | cold restore adds a few minutes. `-p:GoTargetOS=linux` is a DIFFERENT build and **completes clean: 307/307, 0 errors, 475s** (2026-08-14, after the three-target regen wave — `docs/phase4/CENSUS-linux-compile-wall.md` §10). It must be run `--no-incremental`: what differs between targets is the `<Compile>` ITEM SET, not any source timestamp |
  | full `go2cs.slnx` build | **~87s** `--no-incremental` / **~39s** incremental (573 projects; measured 2026-08-07) | 900s | the ONLY gate that compiles the non-generated solution members (utilities, examples) — run it after any golib/runtime API change. ⚠ Under concurrent-lane load a `go2cs-gen` run can die with `AccessViolationException` inside `TypeGenerator`'s recursive `PromotedStructDeclarations`, reported as an `error` against the package (seen once on `core/runtime`, NOT reproducible in two immediate retries with identical flags): re-run before believing it, exactly as with the Go-toolchain crash above |
  | `run-validated-sweep.ps1` (full roster) | **~46–53 min solo (3,138s measured 2026-08-07 at 109 packages / 13,611 verdicts; the roster is 131 packages / 14,769 matching verdicts / 47 disclosed, re-measured 2026-08-14 — so budget well ABOVE the 3,138s figure, and re-measure; ~90+ min under two concurrent lane loads — both r47 attempts were killed externally before finishing, so no clean loaded figure exists)** | run it BACKGROUNDED from the COORDINATOR session only — ⚠ a LANE parking a detached sweep and ending its turn gets it KILLED (the lane's process tree is reaped; happened twice on 2026-08-08 at 106/110 and 98/110, log ends between packages with no summary — recovery: re-run `roster − logged` inline and check the verdict arithmetic closes) | ~29 s for a typical package; i9 full roster measured **7,059-7,705s** at 159-162 rows (2026-08-22) -- the 46-53 min row is the dead i9-13900K era and stands only as the ratio anchor; use `-Filter` for anything but a final gate. ⚠ **ELEVEN** packages carry per-package deadline FLOORS in the script's `$longTimeouts` (re-counted 2026-09-02; `sync/atomic` 60m, `net` 40m and `net/http` 60m joined since the eight — the last sized to a TRUNCATED Debug measurement on the i7 class, where the row's two arms bracket the train's 30m at 1,836 s and a deadline-killed 2,171 s) (`hash/maphash` 60m, `index/suffixarray` 120m, `crypto/dsa` 120m, `archive/zip` 60m, `go/parser` 90m, `crypto/internal/mlkem768` 30m, `crypto/tls` 30m, `time` 40m -- its 1.23.12 suite is 169 tests and ~19 min on laptop-class — the table grew three rows and two floors moved by 2026-08-25, which is WHY the shard map derives its reserved set from the script at generation time instead of copying this sentence; **the script is the authority, this prose is a pointer**), **slow-host-calibrated** since 2026-08-10 — the original i9-sized values false-red every bare sweep on this machine class (hash/maphash and crypto/dsa both reported `FAIL … package timeout after 00:30:00` here; maphash then validated **22/22 in 2,406 s / 40.1 min** given room). The table is also a **floor, not an override** since the same date: a LARGER `-TestTimeout` raises it for a still-slower box (a smaller one still loses, since under-budgeting these four is the false red the table exists to prevent) — before that fix the flag was silently ignored for exactly the four packages that need it |

  Materially *past* these means the test host has hung under lock contention, not real work — stop and
  clear it rather than waiting 10–20 min. **Re-measure and update this table when the corpus grows again**;
  a stale baseline is what makes a healthy run look hung (and vice versa). The spreads above are real
  run-to-run variance on the same corpus (machine load), so budget from the TOP of the range, not the
  midpoint. A converter rebuild invalidates every project's up-to-date check, so the *next* full run
  after one always pays full price.
  ⚠ **An EXTRAPOLATION written in a MEASUREMENT's voice is a false measurement** (2026-09-02): a
  budget comment presented "~236 s fixed, ~62 min" as measured when the fixed term is not constant at
  all (6 shared deps in a 3-project slice against ~31 corpus-wide) and the runs behind it had timed a
  different flavour. The fix is a LABEL, not a better guess — mark the figure PROVISIONAL, state the
  measured points separately, and let the first real run replace it. Every row in the table above is
  a measurement or it does not belong in it.

  ⚠ **`BehavioralRunner` has its OWN internal timeout budgets, and no timeout the CALLER sets can
  influence them** — a generous outer budget on the `run-behavioral.ps1` call does nothing if the
  runner kills its own child first. They were hardcoded constants until 2026-08-10; they are now
  overridable, in SECONDS, at **flag > environment variable > default**:
  `--build-timeout`/`GO2CS_BUILD_TIMEOUT` (batch build, **2400**), `--build-one-timeout`/
  `GO2CS_BUILD_ONE_TIMEOUT` (per-project build, shared-dep pre-build, `go build`, **300**),
  `--transpile-timeout`/`GO2CS_TRANSPILE_TIMEOUT` (**60**), `--run-timeout`/`GO2CS_RUN_TIMEOUT`
  (one program run in the Output phase, **30**). The build defaults are sized for the slowest
  legitimate host per the safety-net doctrine (the i7-5820K measurement below is what sized them);
  a fast lane that wants the old fail-fast behavior opts DOWN explicitly (`--build-timeout 300`).
  **The slow-machine row this table was missing (measured 2026-08-10, i7-5820K 6C/12T, ~3x slower than
  the desktop rows, at 555 packages):** the one-shot parallel build exceeded the stock 300 s **cold and
  warm alike** — warm state cannot save it, because the Transpile phase rewrites every `.cs` immediately
  before Compile, so the batch is never an incremental no-op. For scale, a full
  `dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false` of the same tree took **1,432 s
  cold** (573 projects, 0 errors), ~5x the old 300 s batch budget; a single cold filtered project
  measured 163 s. That measurement is what sized the current build defaults, so such a machine needs
  no configuration; the overrides exist to opt a fast lane back down or to survive a still-slower host.
  **A budget that expires is now reported as `NOT MEASURED`, never as a failure** — a fourth
  `Status.Timeout` alongside Pass/Fail/Skip, borrowing CNR's word for the same idea. This closes a
  **FALSE RED**, the mirror of the false-green routes catalogued above: on the cold slow machine the
  batch timed out, all 555 projects fell to the sequential per-project fallback, each *also* exceeded
  180 s (every one must first build the core dependency closure), and ~15 minutes produced zero
  assemblies and 555 `Status.Fail` entries that read exactly like a corpus regression. Timeouts still
  fail the run and still exit 1 — an unmeasured project must never read as a pass — but they are
  counted, listed and summarized separately. Two related traps the same change closed: an Output-phase
  run timeout used to surface as `exit code mismatch: C# -1 vs Go 0`, i.e. as a *behavioral* divergence
  naming a real test; and the per-project fallback now bails out after **3 consecutive** timeouts rather
  than spending the full budget on all 555 to re-learn one fact.
  ⚠ **A behavioral leg that must SHARD, and the two facts that decide how (2026-09-02).** Every
  behavioral project's build output copies the same ~55-dll core closure into its own `bin` (~29 MB
  each, ~20.5 GB at 695 projects), so an unfiltered Output leg cannot fit a hosted runner's disk in one
  batch: the ruled shape is shard-with-purge — alphabetical slices, `clean-bin` between them, verdicts
  unioned — never a narrowed enumeration (the durable follow-up is a shared-closure csproj template).
  And **`BehavioralRunner`'s `--filter` is a case-insensitive SUBSTRING** (filter `S` matched 455 of
  664), so no filter set can partition the enumeration: a sharding leg takes an INDEX SLICE over the
  deepest-first list and asserts the slice counts sum to the whole.
  ⚠ **Piping a long run through `Select-Object -Last N` buffers ALL output until it completes** — a
  backgrounded suite will look stuck at its first line for its entire duration. Check liveness with
  `Get-Process BehavioralRunner,dotnet`, not the output file. **`-First N` is WORSE: it terminates
  the pipeline once satisfied and KILLS the upstream native process mid-run** (measured 2026-08-16:
  a `-stdlib` reconvert died at ~100/304 with exit −1, reading exactly like a converter failure).
  Redirect long runs to a file and read the file — and redirect with **`Start-Process
  -RedirectStandardOutput`**, not `... *>&1 | Out-File`: the pipeline form BUFFERS, so a run that
  dies leaves a few-hundred-byte log ending mid-line, indistinguishable from an external kill
  (measured 2026-08-31, a 485-byte log from a dead full-suite run). In BASH, `*>&1` is not
  redirection syntax at all — the shell GLOBS it, silently no-op'ing the command (measured
  2026-08-31: one CNR and two runner attempts read as failures that never ran).
  **⚠ PowerShell-REDIRECTED output is UTF-16, and an ASCII grep over it returns a well-formed
  EMPTY** (measured twice 2026-08-31, independently): both `go2cs.exe … > log 2>&1` and a
  `Tee-Object` log land as UTF-16LE, so `grep <marker>` finds nothing and reads as "probes never
  fired" / "the run never happened" — a full retraction was built on six such empty greps, and a
  CNR verdict was nearly lost the same way. The tell costs one command:
  `head -c 200 <log> | tr -d -c '\000' | wc -c` — a nonzero NUL count means every grep against
  that log has been lying — then decode (`iconv -f UTF-16LE`) before grepping. Same
  silence-not-error family as the globbed `*>&1` and the buffered pipe above.
  **⚠ THE TRUNCATED-LOG READING INVERTS FOR POWERSHELL WRAPPERS (measured 2026-09-01, a
  self-inflicted two-runner race):** a wrapper running at `$ErrorActionPreference='Stop'`
  (`run-behavioral.ps1` line 49) dies on the FIRST native stderr line — killing the WRAPPER and
  leaving the runner alive, orphaned, and invisible. The truncated log reads exactly like the run
  being killed and invites the restart that puts two runners in one behavioral tree. Before
  believing a truncated wrapper log, census for the CHILD by executable path; a lane driving a
  long native child invokes it DIRECTLY (or at `'Continue'`), never through a Stop-preference
  wrapper. And never inject
  non-ASCII C# source (`Ꮡ`, `ж`, `Δ`) through a PowerShell command STRING — the argument pass
  mojibakes it even when file I/O is correct; write such content with the Edit/Write tools.
  **⚠ The same mojibake hits a `.ps1` SCRIPT FILE ITSELF when Windows PowerShell 5.1 parses it —
  and unlike the argument case, file I/O is NOT correct here, so the usual fix does not apply**
  (measured 2026-08-30, the syscall-pinning census-guard lane). A `.ps1` written UTF-8 without a
  BOM (the Write/Edit tools' default) is read back by 5.1's PARSER under the system codepage, not
  UTF-8 — so a literal non-ASCII glyph embedded in the script's own source (a regex pattern
  matching `ᴋ`, a string comparison against `Ꮡ`) silently decodes to mojibake at PARSE time, before
  the script ever runs. The instrument does not error: it runs, and reports whatever a
  never-matching pattern reports — in this case a false "0 sites found" that read as a correct RED
  result against a not-yet-fixed corpus, and stayed silently wrong against a freshly fixed one
  until the fresh run's *also* being zero broke the positive control. The fix is a UTF-8 BOM on the
  `.ps1` file itself (`[System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($true))`
  after writing it any other way) — confirmed to make 5.1 parse the literal correctly. Positive-control
  any regex-bearing PowerShell instrument that embeds a converter glyph literally: run it against a
  known-populated target and confirm it finds a nonzero count before trusting a zero anywhere else.
  **⚠ A SHARED PowerShell instrument owes a run on BOTH editions before it banks (measured
  2026-09-02).** `_roster.ps1`'s comparison reader took `Add-Type -AssemblyName
  System.Web.Extensions` — a genuine PS 5.1 case-folding fix, smoke-proven on Windows only, and
  .NET-Framework-ONLY. Under pwsh 7 the script died at its second block, so the sweep's three
  absorption arms (host-conditional, capability-absent, host-limit) silently **DECLINED on every
  Linux host**, and the catch's own message named the missing assembly rather than the missing
  capability — pointing at the wrong artifact, in the file that decides whether a row banks. The
  fix is an edition-conditional reader (Desktop keeps `JavaScriptSerializer`; Core uses
  `System.Text.Json.JsonDocument`, explicit, never `-AsHashtable` behaviour inherited from a newer
  host), and the guard exercises both. **Rule: 5.1 on a Windows lane AND 7 on a Linux lane — or the
  OS-matrix linux leg — before a shared `.ps1` change merges.**
  ⚠ **What that check IS, stated 2026-09-02: the PARSE of every shared script under pwsh 7 Core, plus
  one row actually run.** A cloud container may carry NO PowerShell at all (`dotnet tool install
  --global PowerShell` lands one on the user's tool path) and its writable allowance may sit under the
  sweep's own disk-preflight floor — such a host runs the edition and gate checks with
  `-IgnoreDiskPreflight` STATED, and never banks a Linux row.
  ⚠ **Verify what a host HAS before stating what it LACKS** (2026-09-04): the container that reported
  "no PowerShell at all" corrected itself the same hour — pwsh 7.6.5 was installed at the dotnet
  global-tool path and simply not on `PATH`.
  ⚠ **And its LOCAL form, for a lane with no Linux host reachable** (2026-09-04): the load-bearing
  half is EXERCISED on 5.1 Desktop AND pwsh 7 Core on the REAL path with a DECOY (a Framework-only API
  behind a variable is the `System.Web.Extensions` shape), with the Linux host NAMED as the honest
  closer. Measured is not parsed, and not-yet-closed is not untested — say which of the three a check
  was.
  **⚠ And a PowerShell FUNCTION named `Git` shadows `git.exe`** — command names resolve
  case-insensitively, so `& git` inside it recurses until "call depth overflow" (measured 2026-09-02,
  coordinator). The overflow line, captured through `2>&1`, then counted as ONE dirty entry in a
  `status --porcelain` check and aborted a rebuild twice with a message that read like real tree
  dirt. Name wrappers distinctly, invoke `git.exe` explicitly, and take `status --porcelain` with
  stderr dropped.
  **⚠ The same case-insensitivity binds PowerShell VARIABLE names** (measured 2026-09-02): a results
  array `$main = @()` silently overwrote the `$Main` worktree PARAMETER, so every main-tree round ran
  against an empty path and reported "(no verdict line) 0s" while the control rounds read fine. Name
  arrays distinctly from parameters, and let a function that RETURNS a number write its progress with
  `Write-Host` — a body that `Write-Output`s its progress returns those lines AS its value, and the
  caller's `Measure-Object` then chokes on strings. **And a `git` command run from a DELETED cwd prints
  plausible answers** — "0 commits not in master" for all seven branches, the only tell a
  `getcwd: cannot access parent directories` line at the END of the output: re-run from the repo root
  before believing any count taken after a worktree removal.
- **⚠ Banked-row protection at MERGE time — two rules, both paid for by the crypto/tls regression
  (found 2026-08-19; rooted and fixed by lane `claude/tls-regression`).** The flagship row banked
  green on its lane tip and was RED at master the moment its merge landed, because the guilty change
  (`d1ed1f7c1`, local-iface-cast) had merged to master AFTER the lane forked — each side green
  alone, the union never swept. A lane's sweep proof binds its OWN tree, never the merge result.
  (1) **A BANKING merge owes a post-merge filtered sweep of its own row at the merge RESULT** —
  `run-validated-sweep.ps1 -Filter <pkg>` on the merged master, not the lane tip; the lane-tip proof
  is necessary but not sufficient. (2) **Any reflect-bridge-touching change's canary set is the FIVE
  largest banked reflect consumers BY VERDICT COUNT — recomputed from
  `docs/ValidatedTestPackages.md` at gate time, never carried forward.** The consumer PREDICATE,
  explicit since 2026-08-29: **the package's OWN Go source — production OR `_test.go` — imports
  `reflect`** (test usage counts because the canary protects VERDICTS, and verdicts are produced by
  test code; a suite leaning on `reflect.DeepEqual` is exactly what a bridge regression breaks).
  Derivation is a grep of the GOROOT sources for `reflect` as an **IMPORT** — a name-LIST match
  over-matches (`go/doc/comment`'s `std.go` carries the name as data), so positive-control the
  predicate before using it (`encoding/json` in, `cmp` out) — plus the roster's counts, both at
  gate time. As of
  2026-08-29 it yields: `crypto/tls` 3,643 (bogo-capable hosts only; the collapsed-verdict path
  otherwise), `go/types` 557, `encoding/json` 491, `encoding/xml` 386, `crypto/x509` 341. ⚠ The
  PREVIOUS worked example here included `go/internal/gcimporter` (583) — a package with **zero
  reflect touchpoints in prod or test** — and that membership was then carried across several merge
  windows with only the counts re-read, until a lane's fresh grep caught it while holding an
  expensive sweep: the example had substituted for the derivation, which is precisely what this
  rule forbids, this time performed by the coordinator. Worked examples date and drift; the grep is
  the rule. ⚠ SECOND carried-membership catch (2026-09-01): `crypto/internal/nistec` (2,195) ALSO
  imports reflect nowhere — it had been travelling in the set beside gcimporter and was carried
  again the day before a lane's fresh derivation dropped both. "Reflect-bridge-touching" reads
  broadly: `src/core/reflect/*_impl.cs`, `src/core/internal/reflectlite`, golib's
  `GoReflect.*`/adapter/equality machinery, and the go2cs-gen adapter/shell templates all qualify.
  **And the canary RULE is now split (R's proposal, ratified 2026-09-01):** a change to the reflect
  BRIDGE takes the reflect-importer canaries above; a change to `abi.synthType`/golib **descriptor
  synthesis** takes a **COST canary as well**, because synthesis runs on every interface boxing
  corpus-wide — a blast radius the importer predicate cannot see at all (an unmemoized
  `GoPtrBytesOf` pushed nistec from 354s past its 600s deadline; the memoized re-measure is 384s).
  `crypto/internal/nistec` re-enters as exactly that cost canary: run it and compare its WALL TIME
  against the recorded baseline, not just its verdict.
  ⚠ **How a datum the MARKER loses is carried, and why it is never carried in the marker** (2026-09-05):
  where a marker's TEXT is what `go2cs-gen` dispatches on (`[GoType("chan …")]`, `[GoType("ж<…>")]`),
  a datum the marker drops — a defined channel type's DIRECTION, a named pointer-to-array's LENGTH —
  rides as a SIBLING attribute the converter stamps beside the UNTOUCHED marker and golib reads once
  per type (the `GoArrayDims`/`GoMapKeyDims` family extended from values to a defined type). Changing
  the marker's SPELLING instead teaches the generator a new prefix and buys a test-only population a
  route-#7 ladder. The descriptor synthesizer fills the stamped cargo AHEAD of the interning key so
  every route interns ONE descriptor; where a MEASURED cargo and the type's STAMP disagree the stamp
  decides and the disagreement is refused BY NAME; and descriptor synthesis takes the cost canary
  above.
  ⚠ **THE WORKED EXAMPLE IS RETIRED — the derivation IS the rule (2026-09-03, its THIRD drift).** A
  fresh gate-time derivation over the roster read `crypto/tls` 3,643, `net/http` 1,343, `go/types`
  557, `encoding/json` 491, `net` 472 — `net/http` and `net` outranking the two rows the example named
  — which is the third time a dated top-five carried past its date. Fresh top fives belong on the
  board as DATED data; this file carries only the derivation and its controls. ⚠ And the derivation
  itself sharpened the same day: derive from **PARSED import declarations**, not a line-anchored grep,
  which admitted `go/doc/comment` and `go/internal/gccgoimporter` while BOTH standing controls passed
  — both vary "imports it or not" and neither varies "mentions it as DATA", so add the control that
  pins the axis the counterexample names (`go/doc/comment` must be OUT).
  ⚠ **A canary can also be chosen by MECHANISM rather than by rank, and sometimes must be** (measured
  2026-09-03). `encoding/gob` keys its type caches DIRECTLY on `reflect.Type` identity and is banked
  at 106 green WITH a `[][6]uint8`/`[][8]uint8` identity collapse present: a banked consumer green
  with the defect **cannot detect it** (its suite never exercises the collapsing shapes — green is not
  evidence the model is sound) but **can be broken by the repair**. Such a row enters a change's gate
  list because it keys on the thing the change alters, not because a largest-importer rank would
  select it — the promoted-forwarder / `net/http` precedent generalised. **And the reflect-bridge gate
  list now carries the FULL behavioral suite** (route #7's behavioral twin, above): a bridge change
  that emits byte-identical `.cs` is invisible to CNR and to the `reflect` `-tests` build alike.
  ⚠ **Derive the canary set in a clone whose refs you have verified.** A derivation run with the
  mailbox clone as cwd read `origin/master` **15 rows behind** and produced the SAME top five by luck
  (every dropped row was smaller); only a row-count reconciliation — 178 against the guard's 193 —
  caught it (2026-09-02). The mailbox clone's non-mailbox refs are stale BY DESIGN and are never read
  for repo content; reconcile any derived count against an independent one before using it.
  ⚠ The MECHANISM, stated 2026-09-02 after a second instance: the mailbox clone is TRANSPORT, and its
  refspec carries `claude/mailbox` ALONE — so `git fetch origin master` there moves nothing and its
  `origin/master` stays pinned wherever the clone was made (one such read was 15 rows behind and nearly
  escalated as "fifteen banked Linux rows lost"). Repo content is read only from a work tree, and
  `ls-remote` arbitrates when two refs disagree. ⚠ Met again 2026-09-03 as a DEAD MONITOR ARM: a
  watcher's MASTER-CHANGED leg ran in the mailbox clone, where `git fetch origin master` moves
  nothing, so the arm was structurally dead the whole time it reported healthy. **Assert the clone's
  refspec before any fetch-based arm**, and put the assertion in the lane's own script rather than in
  its memory.
  ⚠ **Three ref-reading rules from one night, 2026-09-02.** **Name the REF you read**: a claim that
  says "at master" is read from `origin/master` AFTER a fetch, never from a branch's working tree — a
  branch's base is a snapshot of master at fork time and ages out from under every claim made through
  it (`git fetch origin master && git show origin/master:<path>`), the stale-base illusion applied to
  a single FILE rather than a diff. **After a fetch that PRINTED AN ERROR, verify the ref actually
  MOVED before reading anything off it**: a fetch dying on a clone's object corruption left
  `origin/master` unmoved, `git show origin/master:<path>` answered about the past while looking like
  the present, and "master is RED on the roster guard" was reported — falsely. "Benign for pushes"
  (they verified the remote moved) is not "benign for reads". **And an ANCESTRY question goes to a
  clone that HAS the ancestry**: a depth-200 shallow clone answered "NOT on the remote" for a ref the
  full-history repo showed contained, and it is the clone a lane reaches for by habit.
- **⚠ The three-run flake standard, and the A/B a re-converting SWEEP silently invalidates
  (2026-09-01/02).** A row that fails once is not a finding: the standard is **fail-WITH the change,
  pass CLEAN, pass again WITH the change restored** — three runs, in that order, before anything is
  attributed to a commit. The strong form is what costs lanes: **reverting the `.cs` is not an A/B
  when the instrument re-converts.** `run-validated-sweep.ps1` re-emits from the LIVE binary, so a
  hand-reverted corpus file is overwritten before the row runs and both arms measure the same
  converter — which is exactly what happened on the h2 deadline rows, identical signatures on both
  sides reading as "the change is innocent". Swap the **PRESERVED pre-change `go2cs.exe`** into the
  sweep path instead, and state which binary each arm ran.
  ⚠ **An A/B ARM that names a ref IS a ref derivation, and inherits the stale-ref rule** (2026-09-02):
  a Linux clone that had only ever fetched its own branch checked out `origin/master` at a commit
  predating the corpus hop, and the sweep's toolchain-pin guard refused it ("version.props pins
  1.23.1 … NOT MEASURED, never a verdict") — the guard caught it, the lane did not. Any arm naming a
  ref runs `git fetch origin <ref>` immediately before the checkout and PRINTS the resolved SHA; and a
  mid-run checkout makes a count belong to a tree nobody named, so check WHICH TREE a run measured
  before believing its number (a sweep measuring a 13-entry tree while the branch moved under it read
  exactly like a silently-rejected disclosure).
  ⚠ **An instrument that FAILS IDENTICALLY on BOTH ARMS confirms whatever was predicted**
  (2026-09-04): four legs called the converter DIRECTLY on a Linux host, bypassing the `GoTargetOS`
  pin and linking the WINDOWS dependency set (phantom CS0426, exit 1, no comparison record), so both
  arms read 0 matched / 0 diverged — and a prediction of "unchanged within noise" is CONFIRMED by two
  zeros. The only tell was a `record=` column in the lane's own verdict line reading EMPTY. **An arm
  proves it MEASURED something — a comparison record present, carrying the row's verdict count —
  before its number is compared to the other arm**, and the invalid run's logs are kept as wreckage
  rather than deleted.
  ⚠ **Stated generally 2026-09-05: A CONTROL INHERITS ITS INSTRUMENT'S DEFECTS.** Two arms both run
  without the `GoTargetOS` pin on Linux both linked the WINDOWS flavour and failed identically — which
  is valid evidence for "not caused by the cut" and WORTHLESS for "pre-existing at master", and a
  finding minted on the second reading was withdrawn. **Name the FLAVOUR an arm linked before believing
  a pre-existing.**
- **⚠ THE MEASUREMENT CONFIGURATION IS PART OF THE VERDICT — the `-tests` pipeline publishes DEBUG
  (measured 2026-09-02, the net/http h2 pair).** The generated `<pkg>.tests.csproj` pins no
  Configuration, so every roster verdict to date was taken at an optimization level no user ships: one
  published artifact flips `TestWriteDeadlineEnforcedPerStream/h2` fail→pass under Release (43.7 ms vs
  500–1000 ms per handshake), and default tiering flips it BOTH ways across consecutive runs of that
  same binary — a validation-integrity defect, the flake class arriving through the JIT. Ruled
  contract: **Release + `DOTNET_TieredCompilation=0`, both RECORDED** in two places that cannot
  silently drift — the comparison record (`testEnvironmentRecord{Configuration,Tiered}`, never
  `omitempty`: absence must not read as Debug) and the host's own `results.json` — plus the proof
  pages. The converter carries it as TWO flags since the tiering census — **`-test-config
  Debug|Release`** (Release publishes with an explicit `-p:go2csPath`, replacing the csproj template's
  Debug-conditional default, and disables the CLR's tiered JIT by default) and **`-test-tiered`** (the
  explicit opt back IN to tiered JIT, meaningless under Debug); the earlier `-test-release-tc0`
  spelling is RETIRED and survives only in one comment in `testConversion.go`. A bare `dotnet build
  -c Release` on the generated csproj is the trap they avoid: **grep the converter's flags before
  building an instrument**, and NAME both sides' configuration.
  ⚠ **Owner ruling (2026-09-02 11:44): the validation configuration of RECORD is Release with tiering
  off; Debug stays available by flag; the pipeline and sweep defaults flip after the Release census.**
  ⚠ **Falsify at the CHEAPEST layer, and separate a gate's PREMISE from its CONSEQUENCE** (2026-09-02,
  the Release census's own blocker): a `beforefieldinit` lazy-static-init hypothesis for a shim's
  Release-only flag rejection died to one grep of the pinned GOROOT — the flag does not exist in Go
  1.23.12 and the converted shim registers 45 = 45, i.e. an external runner/shim version skew — while
  the real event (an access violation at Release in a published single-file host, since unreproduced
  in two further runs and carried OPEN) stands unexplained. **A gate whose premise was wrong keeps its
  CONSEQUENCE when the consequence stands on its own**: a default flip that would UNMEASURE a
  3,643-verdict row is not taken on a corrected premise.
  ⚠ **THE FLIP LANDED 2026-09-02**, the census complete (`docs/phase4/CENSUS-release-tc0-delta.md`:
  195 of 201 rows unchanged, six disclosures retiring, nothing owed a root). `-test-config` defaults
  to **Release** and `run-validated-sweep.ps1`'s `-TestConfig` to **Release**; Debug is a flag away.
  **THREE rows opt back OUT via a new `execution: release-tiered` annotation** — `internal/godebug`
  (`TestCmdBisect`), `log/slog` (`TestCallDepth`) and `net/http` (`TestRegisterErr`) — all three
  PC/line-attribution assertions that tiering's presence supplies, each measured as a one-axis A/B,
  never inferred. `release-tc0` is retained though redundant. ⚠ **The sweep's override predicate had
  to change WITH the default and it is the trap in this flip:** it was `($TestConfig -ne 'Debug') -or
  $TestTiered`, which carried past the flip makes EVERY default run an override — and an override
  SUPERSEDES per-row annotations, so all three opt-outs would silently run at TC0 and fail while no
  run stayed bank-eligible. It now keys on whether the caller SPECIFIED the parameter
  (`$PSBoundParameters.ContainsKey`), so the default respects annotations and is bank-eligible while
  any EXPLICIT flag — the default's own value included — forces uniformity and is not. A default's
  value and a default's *explicitness* are different questions, and a predicate written when they
  coincided answers the wrong one afterwards. Proof pages and comparison records written before the
  flip still say Debug and are stale-until-reswept BY DESIGN; a rebank wave levels them.
  ⚠ **The stack-walk tiering class has a member in our OWN hand-own** (measured 2026-09-02, a one-axis
  A/B at `01a7fdefe`): `reflect`'s `valueMethodName` walks `StackTrace(2)` for a `_package` frame and
  LOSES the Recv frame under Release+TC0 inlining, so `TestValuePanic` passes at Debug and fails at
  Release on the SAME head. **A row that appears under the new default is attributed by the
  configuration A/B BEFORE any commit is suspected**; the remedy is the method name reaching `mustBe`
  explicitly with the walk retired, because a hand-own that infers identity from a STACK is
  configuration-fragile by construction.
  ⚠ **After the flip, every comparison NAMES its configuration beside the tree** (2026-09-02), and a
  set diff whose arms were taken at different times reads the configuration back from each RECORD
  rather than assuming it: a morning control at Debug against an evening pair at the new Release+TC0
  default made a row "appear" that had merely flipped on the configuration axis. Two runs agreeing
  prove DETERMINISM, not causation, when both sit on the same side of an unnoticed axis.
  ⚠ **A codegen-liveness disclosure whose measurement PREDATES the flip is re-measured at Release
  before it is quoted** (measured 2026-09-03): `unique` read 7/20 at Debug and **16/20** at the
  Release default with ZERO flake over eleven runs, so a board blocker resting on a Debug
  frame-residency A/B (RETAINED 4/4) dissolved — eight of its ten GC rows pass at Release, exactly as
  the class's own text predicted, because tier-0 frame liveness is a JIT artifact. ⚠ **Both figures
  are superseded by the 2026-09-05 re-read on the same head: `unique` is 19 of 20 at Release+TC0 and
  8 of 20 at Debug**, the ten `checkMapsFor` rows CONFIRMED as codegen-liveness by a one-axis tier
  A/B — and the twentieth is the one that matters, because `TestMakeClonesStrings` fails IDENTICALLY
  at BOTH tiers, so **frame residency is FALSIFIED as its mechanism** (five candidates eliminated:
  clone retention, `strings.Clone` delegation, CWT keying, the StringData pin, the tier). Per the
  four-candidates rule below, the next step there is an INSTRUMENT — a heap root-path read — never a
  sixth hypothesis.
  ⚠ **A pin-lifetime or GC-liveness PROBE runs at Release with tiering off, or it is not a
  measurement** (2026-09-03): the SAME probe read 6 million clean calls at Debug and went red in 9 s
  at Release+TC0, because a non-optimizing frame roots its temporaries for the method's life so every
  pin holds. This is the MIRROR of the `internal/poll` finding where the same hypothesis was measured
  FALSE — **both stand**: the configuration is part of the measurement, and which way it cuts is
  decided per case by running it, never by precedent.
  ⚠ **And the flip can make an ALLOCATION probe read zero** (measured 2026-09-04): under
  Release+TC0, .NET's escape analysis stack-allocates a small object from the first call, so a probe
  reads zero for a body that allocates at Debug — the probe's own SELF-CONTROLS go red, and that red
  is the instrument telling you every allocation guard resting on it would otherwise PASS VACUOUSLY.
  Keep the self-controls; the durable fix is a self-control body that ESCAPES (a static store, an
  interface return) so it allocates under full optimization too — never a skip-with-reason, which
  would leave the whole allocation-guard family unmeasured at exactly the configuration the roster's
  alloc verdicts are taken under. Its sibling in the same run: **literal-frame NAMING guards that read
  a stack lose their lambda frames to inlining** at the same flag — pin the named bodies
  `NoInlining` for the guard, and RECORD that the feature itself (`runtime.Stack` frame sets,
  recorded-literal-frame names) is inlining-dependent at the configuration of record, which any
  stack-counting host logic must account for. **The one-variable matrix — base vs cut × tiering ON vs
  OFF, same box, same build — separates a configuration class from a cut's regression in one read.**
  ⚠ **A guard for a TIERING class WARMS UP, or it is vacuous by construction** (2026-09-04):
  `net/http`'s leak check lost its first frame from the **174th** call of a hot method on, in ONE
  process — tier-1 promotion with PGO raising the inline budget at a hot call site — while a
  single-test arm (five calls) and a one-call guard both rendered the frame PRESENT at both tiers and
  "refuted" the mechanism. **A missing frame is read as a SEQUENCE** (dump every call, find the
  boundary), never as a sample; a guard for the class makes thirty-plus calls, waits out the tier-1
  delay, then asserts, and its control goes RED on the old code under tiered+PGO. "Refuted at one
  call" is a statement about one call.
  ⚠ **Frame-pinning mechanics from the same week** (2026-09-04): an attribute on a lambda EXPRESSION
  reaches its synthesized backing method, so pinning a frame is ONE attribute and not a shape change;
  an edit INSIDE a `#line` region moves the very position a guard measures, so attributes go inline on
  the mapped line, explanations OUTSIDE the region, and the mapped lines are re-read afterwards; and a
  `NoInlining` sink is GENERIC, never `object`-typed, because boxing a value type would hand a byte
  invariant bytes it did not earn. The byte-cost invariant's DIRECTION decides what a blind probe can
  hide: a stack-allocated object only makes `objects*24 > bytes` MORE likely, so an unescaped table
  produces phantom violations and can never hide a real over-charge — every shape is made to escape so
  no future one goes quietly stack-allocated.
  ⚠ One adjacent hazard the same census surfaced: the finalizer sentinel runs the Go finalizer
  INLINE on the .NET finalizer thread, so a finalizer doing an unbuffered channel send DEADLOCKS if
  the object ever becomes collectible during `runtime.GC()`'s `WaitForPendingFinalizers`.
  ⚠ **That hazard was ROOTED 2026-09-04, and it has NO JIT-tier axis** — which is what a hang
  identical at Debug and at Release+TC0 was saying (four candidates measured out, then an INSTRUMENT:
  five arms with the prediction committed first, the unfixed tree the guard's own red). A Go finalizer
  body run INLINE on the CLR finalizer thread deadlocks against a `runtime.GC()` that waits for
  pending finalizers whenever the body waits on its CALLER — a test that blocks inside its finalizer
  until the test ends does exactly that, and a deadlock has no tier to vary. Go's model is ONE
  goroutine running all finalizers sequentially, a parked finalizer parking only itself, and
  `runtime.GC()` waiting for no body; the fix is that shape (a dedicated runner, the sentinel handing
  off) with the converted GC's stronger-than-Go drain KEPT for well-behaved finalizers and BOUNDED as
  a safety net, the divergence stated.
  ⚠ **Two 2026-09-06 amendments — one WIDENS the hazard, one NARROWS its motivating row.** It is a
  CORPUS-WIDE latent defect rather than a row's problem: any Go finalizer that blocks — an unbuffered
  send, a mutex, a receive — parks the host's finalizer thread FOREVER, so the converted collection
  call never returns for its caller AND the thread is disabled for every LATER test in that host. The
  evidence it is real is a census rather than a hypothesis: **EVERY working finalizer-notification
  channel in the corpus is BUFFERED**, across two packages, and the only unbuffered one is the single
  test that does not pass — a property of Go's tests, not of our runtime. But **read the test's OWN
  SOURCE before framing its failure mode**: the deadlock framing for that row was refuted from its own
  lines — its `select` IS a concurrent receiver for a full second and the collection call is made
  BEFORE the wait, so a call that never returned would produce a package deadline rather than the
  measured one-second failure. The hazard survives with a NARROWER trigger, the finalizer running LATE
  after the receiver gives up, from which point the send has nobody; and the row that raised it cannot
  answer whether that happened, being the last row in its package and the only finalizer in it. The
  faithful shape is still a real finalizer goroutine, and it stays a DESIGN increment because other
  rows rely on finalizer bodies having RUN by the time the collection call returns — the drain
  semantics need a ruling before anything moves.
- **⚠ HOST QUALIFICATION for a network row: preflight `go test -count=1 net` BEFORE any net-family run
  (2026-09-02).** A host whose Go's OWN suite fails is disqualified as a bank host (a container
  answering `TestLookupCNAME` with the CDN CNAME and no IPv6; a WSL host failing that AND all 18
  `TestLookupNoSuchHost` leaves), and on an unqualified host the two arms of an A/B run different
  oracles — evidence, never a bank. A test asserting a live PUBLIC DNS record is UNIVERSAL drift once
  three independent resolvers agree (disclose it on the host-qualification ledger, not any one
  host's); and **a lane does not change a host's system configuration on its own initiative** — relay
  the commands to the owner, and RE-qualify afterwards (G-LAPTOP's WSL did, the same day: the 18
  leaves pass, wall 707 s → 35 s, and it is the fleet's Linux `net` bank host).
  ⚠ **The gate's criterion is the FAILING SET, and it is a LEDGER rather than a threshold** (stated
  2026-09-05): a NAMED, EVIDENCED universally-drifted leaf is tolerated with its evidence at the site;
  ANY other failing leaf ABORTS by name; the leaf names are printed either way. Go 1.23.12's
  `TestLookupCNAME` is such a leaf as of that date — `www.iana.org`'s CNAME now resolves through a CDN
  and three independent resolvers plus the host resolver agree — so it fails on every host until Go's
  own source changes and is NOT a re-qualification item. The criterion block is `.`-sourced and
  controlled in four arms against the LIVE block, never a retyped copy; and **a gate with a
  warn-and-continue switch is the lie-lever shape** — it does not get one.
  ⚠ **A prediction CARRIED from another lane's box predicts a HOST property as a property of the CODE**
  (2026-09-04): "the standing symlink-privilege trio" failed on one laptop and passed on the i7, so a
  baseline read 6 where the prediction said 9 — a miss in the FAVOURABLE direction, owned rather than
  absorbed. A count borrowed across boxes is re-measured on the box that will score it. Its neighbour:
  **a first-run failure that does not recur on a second identical run is a HOST ARTIFACT, named as
  such** (a COM-port semaphore timeout was one), never a disclosure. ⚠ And **a row whose counts differ
  between two hosts is read against the record's own HOST-CONDITIONAL ENTRY before either reading is
  called a regression** (2026-09-05): `os/exec` reading 86+2 on a single-file container host IS that
  entry — `TestExtraFiles` fires where fds 3..100 are held — against 87+1 on the fleet's bank host.
- **⚠ Before a divergence is NAMED, read the ORACLE at the ROW's own source and measure it under the
  SAME shape (2026-09-02).** A converted `crypto/tls` shim exiting 89 under bogo's flag set was
  compared against Go's 2 measured with the flag ALONE — and the answer was in Go's OWN source, not in
  a run: the row's TestMain exits 89 under bogo mode by its own line. The source to read is the
  row's own `TestMain`, not the file the flag was registered in: crypto/tls's prints `Usage of %s` over
  `os.Args` and exits 89 in bogo mode by its own line, and both were reported as divergences from what
  the flag package "normally does" — **"not what the package normally does" is not "not what Go does
  here."** Two neighbours from the same week. A shared CONVENTION name is not a shared MECHANISM:
  `GO_WANT_HELPER_PROCESS` spans suites whose re-exec paths differ (`exec.Command` →
  `posixSpawnForkExec` against `syscall.Exec` → `execve`), so a "row X after fix Y" dependency adopted
  from a lane's note was false and died on the fix's own measured null — read the CALL PATH before
  scheduling a row behind a fix. And a dramatic finding is re-derived from ITS OWN record before it is
  posted: a `"disclosed": []` grep read off `net/http`'s record was nearly published as `sync`
  falsifying its own `TestOnceXGC` disclosure — the record one file over is not this row's record.

### Performance comparison suite (`src/tests/Performance`, 2026-07-02)
- **Purpose:** answer "how fast is the transpiled C# vs the original Go?" — 14 small `Perf*` benchmark
  projects (Startup, Fib, Sieve, MatMul, String, StringView, StringMatch, Map, Sort, Channel, IfaceCall,
  Iface, IfaceShell, RefLower), each a behavioral-test-shaped folder,
  measured across **three variants**: Go binary, C# JIT (`Release`), C# **Native AOT** self-contained.
  Drive via **`run-performance.ps1 [--filter X] [--no-aot] [--runs N] [--update-readme]`** (standalone
  `PerformanceRunner`, no testhost; phases Transpile → Build → Verify → Measure; Verify requires identical
  timing-filtered stdout across all three binaries before anything is timed). The results table lives in
  `src/tests/Performance/README.md` between `PERF-RESULTS` markers (`--update-readme` rewrites it; prior
  toolchain tables accumulate in its *History* section for .NET 9 → 10 comparisons).
- **Mechanics gotchas:** benchmarks self-time via `time.Now().UnixNano()` (added to the baseline
  `core/time` stub for this) and print `elapsed_ns:` lines the runner strips before output comparison; the
  converter **regenerates each benchmark csproj on transpile**, so shared settings live in
  `Directory.Build.props`/`.targets` there (AOT is gated by custom `-p:PerfAot=true` — passing `PublishAot`
  globally breaks the netstandard2.0 `go2cs-gen` analyzer with NETSDK1207); AOT publish needs MSVC
  `link.exe` and the runner prepends the VS Installer dir to PATH for the SDK's `vswhere` probe; AOT trims
  with `TrimMode=partial` because golib `fmt` formatting and sort's `Interface<T>` bind members via
  reflection. ⚠ Cost changed at the 2026-08-01 tree unification: each AOT publish now ILC-compiles the
  full converted-stdlib closure (~7 s each in the stub era; **~25 min each on the i7-5820K**), so a full
  run is HOURS and must run SOLO — concurrent lane load once pushed a healthy publish past an 1,800s
  watchdog. `--no-aot` drops the whole column and stays fast. Keep each
  benchmark ≥50 ms and output deterministic (inline xorshift, no `math/rand`).
- **⚠ Two measured 2026-09-02, both from the TLS-handshake row.** Verify found a **SEMANTIC** divergence
  before anything was timed: the converted `crypto/tls` negotiates ChaCha20-Poly1305 where Go negotiates
  AES-128-GCM on the same host, because `internal/cpu`'s `doinit()` calls `cpuid` — x86 assembly, a
  throwing generated stub — and the throw is SWALLOWED, so x86 feature detection is all-false corpus-wide
  and every AES-NI/AVX fast path runs its software fallback. **A silently-ignored package init is a
  corpus-wide false green**; trace the swallow before pricing anything above it. And for a
  near-threshold SERIAL-latency row, **core count is the wrong lever — a NATIVE control on the same host
  is what exonerates the stack**: Go passed at 250 ms where the managed side failed at 250/500/1000 ms in
  the same run, leaving managed-vs-native handshake latency as the residual.
- **⚠ Both halves of that row are CORRECTED by later measurement (2026-09-02) — read them together.**
  There is no swallow: `schedinit` never runs, so `cpuinit`/`cpu.Initialize`/`doinit`/`cpuid` are
  UNREACHABLE and every `X86.Has*` is simply its zero value; the fix is a `[ModuleInitializer]`
  stand-in (the `goenvs`/`goargs` precedent) hand-owning `internal/cpu` over
  `System.Runtime.Intrinsics.X86`, 14 of Go's 20 flags mapped and 5 left false as the conservative
  direction. **A silently-UNREACHED package init is the same corpus-wide false green as a swallowed
  one** — trace the CALL CHAIN, not a `catch`. And the handshake residual was FALSIFIED as the h2
  pair's cause: a clean negative A/B moved 0 rows with AES-GCM negotiated, an isolated handshake is
  ~44 ms (which cannot blow a 250 ms rung), and the pair is a build-CONFIGURATION artifact — see the
  Debug-publish rule above; a cut's justification stays what it MEASURED.
- **⚠ The unreached-init class, ONE MEMBER WIDER (2026-09-05).** A converted package can be TWO
  CONTRADICTING HALF-IMPLEMENTATIONS — hand-owned no-ops sitting beside converted checkers that
  nil-deref or return at their first line — so neither half can be read as the package's behaviour.
  Its root is the same door as `internal/cpu`'s: `debug.cgocheck = 1` is assigned only on the
  `schedinit` → `parsedebugvars` path, which never runs, so the field's DECLARED value is not the
  value any check sees. **Read the DEFAULT's ASSIGNMENT PATH, not the field's declaration, before
  believing a check runs.**

### Adding a regression test when a converter defect is fixed
When a meaningful converter bug is fixed, lock it in with a behavioral test so later changes can't silently
reintroduce it. **Prefer extending an existing behavioral project** if one already covers a similar
construct; otherwise add a new one (example: `tests/Behavioral/GlobalStructFieldPointers`, which guards the
`&cpu.X86.HasADX` cross-file address-of-field fix). To add one:
1. **New folder** `src/tests/Behavioral/<Name>/` with a Go program that *exercises the specific construct*
   (multiple `.go` files are fine and run as one package — needed to reproduce cross-file bugs). Include a
   `go.mod` (`module go2cs/<Name>` — ⚠ but a test carrying a nested sub-library PACKAGE inside its own
   module takes a BARE `module <Name>` instead, or the sub-library's namespace and the consumer's
   emitted alias disagree and the parent fails CS0234; measured 2026-09-02, and the corpus agrees —
   24 of the 27 behavioral projects with a nested sub-package are bare, and the three that are not give
   the sub-library its own `go.mod`, i.e. a separate module path), and copy `go2cs.ico` + a
   `<Name>.csproj` from a sibling test (adjust
   `AssemblyName`; keep the `golib`/`fmt` refs the program needs). Verify it with `go run .` first. ⚠ A test that imports a SIBLING sub-library names its module `<Name>` with NO `go2cs/` prefix and imports `<Name>/<sub>` (the `NamedSliceChildPkg`/`netlike` pattern): the converter references the sub-library as `<sub>/<Name>.<sub>.csproj`, the name it also emits for the sub-library's own project. Under `module go2cs/<Name>` the parent's reference becomes `go2cs.<Name>.<sub>.csproj`, a file nothing emits, and the build dies CS0246 on the sub-library's namespace inside the GENERATED shells -- pointing away from the `go.mod` (paid 2026-09-05, the ReflectFieldMetadata guard).
2. **Make the Go↔C# output match** so `OutputComparisonTests` passes. Mind known runtime limitations — e.g.
   `Ꮡ(value)` (address of a non-boxed value) currently boxes a *copy*, so don't write through a
   `&global.field` pointer and then read the *original* global; read back through the same pointer.
3. **Register in the solution** — add a `<Project Path="tests/Behavioral/<Name>/<Name>.csproj" />` line under
   the `/tests/behavioral/target-projects/` folder in `src/go2cs.slnx` (alphabetical). **If the test pulls in
   a sibling library sub-project via `<ProjectReference>`** (e.g. `GoNamespaceShadow` → `nsshadowlib/go.nsshadow.csproj`),
   register **that** too, on the line right after its parent (the pattern used by `IoLike`→`IoLike/FsLike`,
   `NamedSliceChildPkg`→`.../netlike`). **Then verify it stuck** — run **`./check-solution-integrity.ps1`**
   (from `src/tests/Behavioral`): it asserts every behavioral `.csproj` on disk is registered in `go2cs.slnx`
   and flags any dangling entry, exit-1 on violation. (Also runs automatically as the preflight of
   `check-no-regression.ps1`.) This matters because the harness builds each `.csproj` **by path**, not via the
   solution, so a missing registration still passes the whole suite — it only breaks the `go2cs.slnx` build in
   Visual Studio (the unregistered project loses the Debug/`$(go2csPath)` context and its `core\*`/`gen\*` refs
   fail: CS0246/CS0234). That is exactly how `nsshadow` slipped through (added in `96eff53cd`, unregistered
   until `53dd2497e`). If Visual Studio has the `.slnx` open it can rewrite/reformat the file and silently drop
   an external edit — re-add and re-verify if so.
   **⚠ Windows CASE trap when you `git add` the new folder (found 2026-08-07).** `git add .` / `git add -A`
   — and any add run from a cwd *inside* the tree — records the path git gets from **readdir, i.e. the
   ON-DISK casing**, whereas an explicit lowercase pathspec (`git add src/tests/Behavioral/<Name>`) is
   canonicalized to the casing already in the index. Under `core.ignorecase=true` the difference is
   invisible locally, so a clone whose `src\tests` had drifted to a capital `src\Tests` on disk banked
   `DeferFrameScopes` at `src/Tests/Behavioral/…` while the other 4,240 files stayed `src/tests/…` — ONE
   directory on Windows, TWO on any case-sensitive filesystem (Linux clone, container CI, case-sensitive
   macOS volume), where the `.slnx`'s lowercase `tests/Behavioral/…` registration then fails to resolve.
   `check-solution-integrity.ps1` now asserts case-sensitively that every tracked path under the behavioral
   tree is exactly `src/tests/Behavioral/…`, so this cannot recur silently. If it fires: `git mv` will NOT
   do a case-only rename on Windows — rewrite the INDEX with plumbing (`git update-index --force-remove
   <wrong-cased-path>`, then `git update-index --add --cacheinfo 100644,<sha>,<lowercase-path>` reusing the
   SHAs from `git ls-tree -r HEAD`, which keeps the blobs byte-identical) — **and fix the on-disk directory
   casing too** (rename through a temp name, `Tests` → `__tmp__` → `tests`), or the next `git add -A`
   re-creates the wrong path. Both are working-tree-invisible: `git status` stays clean throughout.
4. **Transpile once** (`go2cs.exe src/tests/Behavioral/<Name>`, no `-comments` — behavioral goldens omit
   them) to generate the `.cs` + `package_info.cs`. For output comparison, add `[GoTestMatchingConsoleOutput]`
   to the generated `package_info.cs` class (a hand-added attribute the converter preserves).
5. **Generate tests + goldens:** run the **`UpdateTestTargets`** utility **with `--createTargetFiles`** (from
   its `bin/Debug/net10.0`). It scans every `tests/Behavioral/*` folder, rewrites the `// <TestMethods>`
   blocks in all four `*Tests.cs` classes (adding `Check<Name>()`), then **re-transpiles every project it
   is about to re-baseline** and copies each freshly transpiled `.cs` to a `.cs.target` golden. It only
   emits an `OutputComparison` test for projects whose `package_info.cs` has
   `[GoTestMatchingConsoleOutput]`. Afterward, `git status` should show only your new project + four
   `+3`-line test-class diffs (no other `.target` churn). The transpile is what makes step 4 above a
   convenience rather than a prerequisite — and because it walks the whole corpus, a whole-tree run is
   CNR-length; add **`--only <Name>`** to re-baseline one project (and to exercise the refusal branch,
   which exits non-zero naming any project whose transpile failed, timed out, or degraded).
   ⚠ **ONE WORKTREE PER CUT — `UpdateTestTargets` enumerates the DIRECTORY, not your change**
   (measured 2026-09-02): a stray untracked project left by ANOTHER cut was enumerated into this
   cut's four test classes, and the ASYMMETRY is the tell — one new project gives `3/3/3/3`, that run
   gave `6/3/6/6`. Two dirty converter files from the same neighbour would also have made any build
   there measure a MIX. Neither fails a gate, so the check is the diff's shape: count the added
   `Check<Name>()` lines per class before staging.
6. **Verify (filtered, fast):** preferred — from `src/tests/Behavioral`, run
   `./run-behavioral.ps1 --filter <Name>` → the 4 phases (Transpile, Compile, TargetComparison,
   OutputComparison) for that project via the standalone runner, in seconds, with no testhost/lock risk.
   Equivalent MSTest path (still valid): from `src/tests/Behavioral/BehavioralTests`, run
   `dotnet test --no-build -c Debug --filter "FullyQualifiedName~<Name>"`. Either way, avoid the full
   `dotnet test go2cs.slnx` while iterating — it rebuilds everything and can hang under VS lock contention
   (see the test-harness notes above). The golden comparison is line-ending-insensitive, so a multi-line
   string literal needs **no** `.gitattributes` handling for the byte compare — mark the `.cs` `-text` **only
   if** the compiled program's behavior/output depends on that literal's exact newlines (autocrlf gotcha above).
7. **Record the conversion decision (keep the strategy docs living).** The conversion strategy lives in
   **two** documents, and a notable decision updates the right one (often both):
   - [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) — the exhaustive
     **technical reference**. Nearly every conversion decision lands here: add or update the `###` subsection
     under the matching `##` topic with the emitted form, the edge case, the reasoning, and the guarding
     behavioral test. This is where the deep detail and history accumulate.
   - [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — the high-level **summary** (one section
     per topic, tight prose + a couple of real Go→C# examples, each linking into the reference). Update it
     only when the decision changes the *headline* mapping of a construct or warrants a better/clearer
     example — not for every edge-case fix. Keep it short and readable; push the detail to the reference.

   Do this **in the same change** so both docs keep matching reality. Verify every C# snippet against the
   actual `.cs.target` golden (it is the authoritative record of emitted forms — e.g. `u8` format strings,
   `throw panic(...)`, `ж<T>`/`Ꮡ`); the summary's examples should prefer real snippets pulled from the
   converted stdlib in `src/core` (Go source ↔ converted C#). Skip only for pure bug-fixes that restore an
   already-documented behavior. (This rule is not limited to the regression-test flow — it applies to *any*
   commit that lands a notable conversion decision.)

### Corpus mechanics — measuring/iterating the converted stdlib (`src/core`)
- **⚠ 37 packages are in LAYOUT L3 and the ritual below is UNCHANGED because of it, not despite it.**
  Since 2026-08-08 a package whose emitted C# varies by `GOOS` keeps the varying files in per-GOOS
  subfolders (`<pkg>/{windows,linux,darwin}/`) and its `.csproj` carries a `$(GoTargetOS)` block that
  compiles exactly one of them, defaulting to **`windows`** — so a plain `dotnet build` and a plain
  `-stdlib` reconvert both still mean "the Windows corpus", byte for byte. That default is what keeps
  the seeded-reconvert control honest: a SINGLE-target run **honors** an L3 tree (it writes
  `<name>.cs` back to the `<goos>/` folder the tree already holds it in) rather than laying a flat
  duplicate beside it, so a seeded reconvert of an L3 corpus is still 0 new / 0 absent / 0 content
  differences. Nothing about seeding, the marker gate, the overlay rule or the phantom classification
  changes. What DOES change: an **unseeded** root now breaks layout adoption as well as the marker
  gate (there is no `<goos>/` folder to route into, so every varying file lands flat and the next
  build compiles two copies) — one more reason the seeding is non-negotiable. Hand-owned files are
  routed too, by their principal's platform set; the invariant is guarded by `platformHandOwn_test.go`
  under the plain `go test ./...`. Design: `docs/phase4/DESIGN-multiplatform-corpus.md`.
  ⚠ **What that routing does and does NOT cover, read at the code 2026-09-05.** The three-target merge
  places a hand-own per LOGICAL name (the GOOS folder stripped) **only when its basename maps to an
  EMITTED principal** — `<stem>.cs` for a `<stem>_impl.cs`, or the `.cs.auto` of a marked file — and a
  placed hand-own's candidates are every flavour folder plus flat, whose copies must be BYTE-IDENTICAL
  or the merge REFUSES (`trace_impl.cs`). A companion with NO emitted principal is left in place
  UNCOMPARED, which is why two same-basename pairs that DIFFER across `linux/` and `darwin/` sit on
  master unrefused — an accident of naming, not a ruling. So: **a per-flavour companion that differs
  from its sibling carries a flavour-distinct basename; a byte-identical one keeps one basename and is
  routed as one hand-own.**
  **Two L3 gate lessons (measured 2026-08-15, the three-target leveling lane):** (1) a change whose
  files live in linux/darwin per-GOOS folders is NOT compiled by the default windows build — the
  windows `go2cs-stdlib.slnx` gate alone would have skipped 15 of that regen's 27 files, so L3 work
  owes a `-p:GoTargetOS=linux` build too. ⚠ **The darwin half of this note was STALE for ten days
  and is corrected here (2026-09-02): darwin COMPILES CLEAN** — census run 32649840220 at
  `c003d32af`, **zero errors on osx-x64 AND osx-arm64**, the wall history **19 → 10 → 9 → 0**
  closed by lane G within ~24 hours of the first darwin build ever attempted, re-confirmed green
  at master by the 2026-08-25 census (run 32852475367, both legs). The retired text said "darwin
  does not currently build — `os/dir.cs` cannot resolve `File.readdir`, 19 pre-existing errors";
  that is the state of 2026-08-22, and it survived here long enough to be copied into a lane
  prompt and to send a lane looking for a wall that no longer exists. **The darwin census is a
  REGRESSION GUARD now, cheap and dispatchable at any branch tip** (`.github/workflows/os-matrix.yml`,
  `goos=darwin stage=census`), not a wall to census. What darwin still lacks is a RUN layer, which
  is a separate and open question: `docs/phase4/FINDING-darwin-run-layer.md`).
  (2) **A `GoTargetOS` switch poisons `obj/`**: the `<Compile>` item set changes while timestamps
  don't, so an incremental build after a target switch silently validates the OTHER target's
  assemblies — purge `bin`/`obj`/`Generated` between target switches before trusting any build or
  suite that follows one.
- **⚠ The corpus's emission cgo state is `CGO_ENABLED=0`, and a conversion against the committed
  tree must MATCH it** (measured 2026-08-29, net's Linux first contact — a corpus-level fact, not a
  lane detail: `net/linux/cgo_stub.cs` is on disk and `cgo_stub.go` is selected ONLY when cgo is
  off). This is the coherent convention — the converter cannot process cgo C halves regardless (it
  skips toolchain intermediates loudly since the Syntax-pairing fix), and Go's own cgo-off file
  selections are fully functional pure-Go paths. The trap is a MIXED-state tree: converting under
  cgo-ON against a cgo-OFF corpus changes the build-tag file selection, so declarations MIGRATE
  between files while the stale other-selection file remains — measured as a CS0111 duplicate init
  forcer in `net` (the forcer moved from cgo_stub.cs to dial.cs with both on disk). Reads exactly
  like a converter defect; it is an environment mismatch. On any Linux host with gcc (where cgo-on
  is the default), set `CGO_ENABLED=0` before converting against or regenerating the corpus.
  ⚠ **It bites from the TEST side too, and there the state is PER-PACKAGE** (measured 2026-09-02 on
  a Linux lane as a one-variable A/B on `os/user`, whose Go file selection is cgo-conditional): a
  sweep converts under the session's `CGO_ENABLED`, so a cgo-ON run selects `_test.go` files the
  cgo-OFF corpus never carried, leaves untracked `cgo_*_test.cs` artifacts behind, and dies in the
  closure build in ~12 s with **zero verdicts** — a build failure that reads like a conversion
  defect. Both comparison sides must share ONE cgo state, and the converted side can only be the
  corpus's. `run-validated-sweep.ps1` pins it per package (`$cgoOffPackages`, beside
  `$longTimeouts`); a row whose file selection is cgo-conditional joins that table rather than
  depending on the session it happened to run in.
  ⚠ **And a row can AGREE by a COINCIDENCE OF ERRNO** (measured 2026-09-02, one variable proven both
  ways): Go's `AllThreadsSyscall` tests skip on `ENOTSUP` because the ORACLE is cgo-LINKED, and the
  converted side skips on the same word because an unimplemented stub answers `ENOTSUP` — pass/pass at
  cgo-OFF, skip/skip at cgo-ON. So the `$cgoOffPackages` predicate is not only "does cgo change which
  files convert": the ORACLE's own behaviour (its cgo-gated branches) is a second axis in the
  predicate, and every row number carries the cgo state it was taken under.
  ⚠ **RULED 2026-09-03: cgo OFF is the state of record on EVERY platform, the sweep pins it for the
  WHOLE run, and the roster preamble states it once.** The Linux annotations had been banked without
  naming the axis: a cgo-OFF re-sweep read three rows LOW by exactly the ORACLE's `testenv.HasCGO`-
  gated tests (buildinfo +7, gcimporter +1, pe +3 skip/skip) with **zero verdicts moving**, because
  that bank host's default was cgo ON while the corpus is emitted cgo OFF. The evidence that decided
  it came from the OTHER platform — the Windows bank host has no C compiler, and its counts for the
  same rows ARE the cgo-OFF readings — so ruling the other way would have left the two platform
  bubbles on different unstated axes. A re-sweep already in flight at the ruled state IS the record;
  no second sweep is owed. ⚠ **The DRIVER's CONTEXT is a third axis a row number carries, beside cgo
  and host** (2026-09-04): two `syscall` tests skip under a detached no-TTY driver on BOTH sides and
  DIVERGE where a controlling terminal exists, so every fleet sweep matched them skip/skip and no
  sweep can see the divergence — **a bank states the terminal context it measured in.**
- **The on-disk corpus can be stale** relative to converter changes made since the last regen; building
  the committed tree measures *that* output, not today's. To measure the current converter you reconvert.
- **⚠ For ADDRESS-OF/ALIASING (`Ꮡ`-machinery) converter changes, a seeded corpus reconvert-and-BUILD
  joins the gate list — CNR alone is not sufficient** (proven 2026-08-15, the element-field-address
  fix: of the three defects in that arc, CNR caught ONE; the other two — a pointer-receiver named-array
  blind spot and its over-broad first fix — appeared in NO behavioral test's shape and were found only
  because the whole corpus was reconverted and compiled. The same census surfaced a real shipped lost
  write, `encoding/xml`'s attribute-namespace translation writing into a copy). The behavioral corpus
  is a SAMPLE of Go's shapes; the stdlib is the population — aliasing changes get measured against the
  population before banking.
- **⚠ Blast-radius measurement for a converter change: TWO seeded reconverts diffed against each
  other, never the committed-tree diff** (2026-08-29, the position-table splitter fix). A naive
  reconvert-vs-committed diff reported 147+ files — almost all PRE-EXISTING unbanked drift from
  arcs that landed without their regens (the standing position-map staleness two census lanes
  rooted independently the same day). Seeding one root at the PRE-change converter and one at the
  CHANGED converter and diffing the two emissions isolates exactly the change's own footprint
  (26 metadata files, zero production code, in the measured case). The committed tree is a moving
  baseline; two emissions of the same sources differ only by the change.
  ⚠ Two mechanical tells the ritual owes on EVERY run (paid 2026-09-01 — a lane's "old" binary
  never existed and the diff silently compared committed-tree-vs-fixed, reporting 724 phantom
  files): (1) `go build -o <path> <dir>` with a bare directory as the last positional can land
  the binary at `<dir>/go2cs.exe` and leave the named `-o` path NONEXISTENT while exiting 0 —
  verify the built binary exists at the exact path you will invoke; (2) before trusting any
  two-root diff, assert BOTH sides' emitted files carry THIS RUN's mtimes — a diff between a real
  reconvert and an untouched seed returns a normal-looking result with nothing marking it invalid
  (the emitted-before-seeded family, build-step edition).
  ⚠ **Tell (1) is TOO WEAK when a STALE file already sits at the invoked path, and was strengthened
  2026-09-05**: `go build -o` fed an MSYS `/c/…` path wrote into a stray `C:\c\` tree while the
  existence-and-size check read the STALE binary sitting at the right path and passed. **Assert the
  binary's MTIME MOVED and that it is newer than its sources** — which is the converter's own
  staleness predicate, and note that checking out a branch whose converter sources are newer than the
  binary is exactly what that guard REFUSES, correctly.
  ⚠ **And it runs over the WHOLE corpus, never scoped to the package the cut names** (2026-09-04),
  because a scoped census reproduces its own scope: the whole-corpus run found both a publication
  defect and fourteen sites in `sync/map.cs` that the name-keyed census had not predicted.
  ⚠ **A THIRD failure mode neither documented tell covers (2026-09-04): a script that SEEDS EACH ARM
  from the LIVE worktree at the moment that arm starts, while the lane edits that worktree between
  arms, seeds `base` from one tree and `new` from another** — and the diff then reports files as
  "differing" that NEITHER converter wrote, reading exactly like a footprint. Both standing tells
  passed (the binary at the exact path; this run's mtimes on both sides). **Take EVERY seed before ANY
  arm converts, or seed from a frozen snapshot (`git archive`)** — and make the write-evidence check a
  CONTENT test, never a timestamp: hash a `[module: GoManualConversion]` file across every seed and
  require equality, since a file neither converter writes must be byte-identical everywhere. That
  marker file must sit IN A PACKAGE THE ARC TOUCHES (a hand-own in an untouched package discriminates
  nothing); where the affected packages hold none, fall back to whole-tree content identity outside
  the emitted set. Mtimes are a HINT only — `find -newermt` reported 179 `src/core` files inside one
  run's window that were a branch switch completing before the first seed, and trusting them would
  have discarded a sound A/B — **but a written count of ZERO is decisive where a nonzero one is not**
  (see the fourth mode below). Two mechanics from the same run: derive hunk anchors from the BASE
  EMISSION (difflib opcodes) so no converter glyph passes through a shell (a heredoc mangled one,
  caught at zero matches — the LF-anchor family through its glyph door); and **`git show HEAD:<file>`
  returns the LF blob against a CRLF checkout** (1,408 "differing" lines that CR-strip to 18), so name
  the LAYER before quoting a number.
  ⚠ **A FOURTH mode, 2026-09-05, and it is the emitted-vs-seeded trap's PUREST instance: both arms
  piped through `Select-Object -First N` were KILLED at exit −1 with ZERO `.cs` written, and the
  comparison of two untouched seeds read 0/0/0 — indistinguishable from a clean gate.** (The
  `-First N` kill is the documented pipeline trap; what is new is that it can take out an A/B's arms
  silently, since the trap's usual tell — a visibly dead run — is what the diff then reads as a
  green.) Three tells, one decisive: an impossible WALL (minutes where a three-target conversion
  floors at ~9), the exit code, and a per-arm per-target count of files WRITTEN this run. **The diff
  script ASSERTS the written count and ABORTS on an arm that emitted nothing**, and **a lane that
  finds its own vacuous reading RETRACTS it publicly with the mechanism before anything seats on it.**
  ⚠ Beside it: **a two-seeded diff NAMES the toolchain that built EACH arm** (`go version <binary>`) —
  a `go build` in a shell without the GOROOT pin stamped one arm go1.24.7 against the other's
  go1.23.12, caught only by byte-comparing a pinned rebuild against the running binary.
- **⚠ The bank unit for a converter change's corpus footprint is the two-seeded diff's HUNKS, never
  its FILE set (measured 2026-09-02).** Applying the A/B's ten whole files onto a corpus that is
  stale in OTHER families carries those families in with them: the whole-file application landed
  six relocation hooks into one `package_info.cs` while the file that declares them — byte-identical
  between the two binaries, so never flagged — still declared three, and the result was CS0111 ×3.
  Byte-identity to the new emission PASSED and an exact path-set assertion PASSED; neither can see
  a file the diff never named. The tell was arithmetic: **279 applied diff lines against 32
  measured**. Apply the change's OWN lines (the re-done application was 9 hunks / 24 lines, zero
  `GoPositionMap` and zero import-hook lines in the delta, with one untouched package as the direct
  control, and built clean everywhere); position maps and relocation hooks belong to the deliberate
  regen, not to a converter train. ⚠ **And COMMIT the corpus edit BEFORE any sweep** (paid twice in
  one day): a sweep wrapper's restore step (`git checkout HEAD -- src/core`) cannot distinguish your
  uncommitted work from the sweep's own dirt, so hand-applied hunks vanished between two rows and
  the second row failed **invalidly** — a phantom red. Ordering is the fix, not the script; re-run
  the application's own assertions after any restore.
  ⚠ Two corollaries measured 2026-09-02. **"Byte-identical to the emission" is a property of the FILE, not
  of the CHANGE** — copying the footprint files wholesale out of the NEW seeded root is byte-identical BY
  CONSTRUCTION and still wrong, carrying every arc not yet regen'd into the corpus (numstat read 3/9,
  13/31, 3/15, 5/6, 1/7 against a change that owns six lines); numstat is the cheaper instrument, and the
  strongest-looking provenance check cannot see the difference. And **a footprint hunk is the fresh
  emission's STATEMENT even when the committed statement carries another arc's unbanked drift**: carry the
  one inert, byte-verified foreign line and SAY in the commit which line belongs to which arc — a
  hand-written shape no converter emits is worse than one foreign line named. Read the COMMITTED bytes,
  never the seed, before cutting a footprint against a base.
  ⚠ **The hunk rule binds METADATA files too, and the tell is the diff's line KINDS** (measured
  2026-09-02): re-emitting a `package_info.cs` from a `-tests` run imports closure family #2 wholesale
  — 139 `[assembly: go.GoPositionMap` lines became `global::go.GoPositionMap` while the other two GOOS
  folders kept the `go.` form — where exactly ONE map line was owed; counting the diff's KINDS
  (139 −/139 + of a single attribute) caught at the seat what reading it did not. Three companions.
  After ANY pipeline run, every file in the commit is diffed against the PRE-run tree and its line
  kinds counted — an INTENTIONAL file gets the same check as an unintentional one (one lane paid this
  twice in a session, past its own written lesson, because a `-tests` measurement run hands back the
  whole-file form of the metadata file a surgical hunk had touched). A "differs" is CR-STRIPPED before
  it is named (eight runtime files reported as additional drift at one regen were the in-comment-LF
  phantom under a raw-byte `diff -rq`; every one CR-strips identical). And the BEFORE arm of a
  before/after is taken at the CUT'S OWN BASE, never at an earlier landing on the reasoning that "none
  of the intervening merges touch it". A file that is not gofmt-clean at master leaves foreign
  whitespace lines in the next cut's diff: verify they are pure whitespace and NAME them.
  ⚠ **THE HUNK RULE'S INSTRUMENT (adopted 2026-09-03).** `git merge-file -p <committed>
  <base-emission> <cut-emission>` — a 3-way merge with the BASE seeded emission as merge base —
  COMPUTES a converter change's own lines instead of leaving them to be picked out by hand. It is
  PROVED by a pair: **applied delta == emission delta (CR-normalised)**, and **the residual drift
  against the emission is the IDENTICAL SET before and after** (plus 0 `GoPositionMap` and 0
  import-hook lines in an additive delta). Both are needed — a hand span-replacement that obeyed the
  rule still dropped six comment lines the converter HOISTS out of displaced bodies and re-emits
  standalone: it compiled, the placeholders were present and the path set passed, and the tell was
  arithmetic (93 emission-delta lines against 65 applied). ⚠ `diff(base emission, committed)` coming
  back NON-EMPTY is the HEALTHY control reading, not a fault — the corpus carries standing families —
  and position-map hash changes stay with the deliberate regen.
  ⚠ **`patch` applies some hunks and REJECTS others while exiting non-zero** (2026-09-04), leaving a
  file PARTIALLY patched with a `.rej` beside it: an exit-code check calls the run "failed" while
  `git diff --numstat` shows the file half-changed (+9/−9). **Read the TREE, not the exit code** — and
  prove a hunk application IS exactly the change by the emission's added-line count equalling the
  applied added-line count on EVERY file, with no `.rej`/`.orig` surviving.
  ⚠ **Count a footprint with `git diff --numstat`, and take the third reading from a DIFFERENT
  instrument** (measured 2026-09-03): `grep -cE '^[-+][^-+]'` drops every removed BLANK line (a bare
  `-`), so an emission count and an applied count taken the same way agreed with each other (78 = 78)
  while numstat said 82. Two blind instruments agreeing is not a closed arithmetic.
  ⚠ **Line KINDS are read against what the change DOES, never against a remembered number** (three
  measurements, 2026-09-03). An ADDITIVE change's delta carries **zero** `GoPositionMap` lines, so any
  map line there is foreign drift. A change that **REMOVES** emitted code re-encodes the file's map
  and carries its OWN map lines — their ABSENCE would mean the emission had not shrunk — and it kills
  any lift whose only source was the removed body (`nanotime1_r`), so the class-B "zero map lines in
  the delta" bar is for additions only. Refinement: when a displacement removes a file's **LAST**
  mapped content the converter RETIRES the record rather than re-encoding it, so a removal can own a
  map line's ABSENCE; the prediction states WHICH, per file, and names "a map line removed rather than
  re-encoded" as its own falsifier. A prediction's COUNT can hold while its KIND does not — state
  both, and leave a wrong prediction VISIBLE above the measurement that refuted it (appended, never
  rewritten), because a prediction is only worth having if it cannot be edited after the result.
  ⚠ **A prediction is SCORED in the record whether or not it HELD, and its MECHANISM and its SPECIFICS
  are scored SEPARATELY** (2026-09-05): one prediction's mechanism was right and all THREE of its
  specifics wrong — not those sites, not those tests, and on Windows not an errno but a fault. **A
  record that cites its predictions only when they land is an advertisement.**
  ⚠ **A displacement's map line is invalidated by ANY later change to the same source file, and the
  check runs ONCE at the ASSEMBLED tip** (2026-09-03, the seam cut into four times in one day). A
  train's comment drain shifted positions after a keystone's map was derived, so the source file
  merged byte-identical to the fresh emission while the map carried a value NEITHER converter emits
  (three hashes: drain-only, displacement-only, both). Sharper: **the seat that invalidates a position
  map need not write a map line at all** — a co-seat following the hunk rule CORRECTLY (zero map
  lines) moves the positions the value describes and is structurally invisible to a per-seat union
  check; the more correctly it follows the rule, the more invisible. So the fresh-emission check
  covers `package_info.cs` as well as the source, and runs at the assembled tip by the coordinator,
  never per seat; a knowingly wrong map line is fixed there from the emission as one stated fixup
  line, never pasted, and pre-existing stale siblings stay with the regen.
  ⚠ **And a footprint hunk that would re-encode a position map against a fresh emission SHORTER than
  the committed file is a WRONG map on that file — never applied in a converter train** (2026-09-04):
  an unbanked relocation sits between the two, so the value would describe NEITHER tree. The
  deliberate regen levels the map and the relocation together, with the other arc's delta NAMED in the
  commit; and an L3 package is measured on EVERY target, each into its own seed.
  ⚠ **A converter change's corpus footprint is measured on ALL THREE TARGETS or it is not measured**
  (2026-09-04): an A/B run single-target on the host default posted "over the whole corpus" while its
  linux and darwin lines were never applied — the L3 blind spot this file names, walked into while
  quoting the rule. The tell is the emission COUNT PER TARGET stated in the post (1,656 windows
  against 1,724 and 1,727). ⚠ **And the hunk-application arithmetic is NOT sufficient by itself**:
  line counts, the foreign-line grep and `git apply`'s exit are all SET properties, and a hunk landed
  in the WRONG FUNCTION has the identical set — a blanket `-C1` put a `bool` flag declaration 55 lines
  early, inside a neighbouring function, while its set and its read stayed in the intended one, caught
  only by the three-target build (CS0103 ×2 on every target, MSB 0). The instrument is THREE checks:
  the counts (whole-file), the foreign-line grep (imported arcs), and a POSITIONAL per-function
  balance — every flag set or read declared in that function, every declared flag used, both function
  names printed on a mismatch. Two checker rules from that checker's own first failure: **print TOTALS
  unconditionally**, so "found nothing" is distinguishable from "the predicate never fired" (an `awk`
  version treating a function as starting at `internal|public|private` merged two `[GoRecv] public
  static` functions into one self-satisfying block and read 2 declarations where the file has 3); and
  **a control whose INPUT is wrong is thrown out and rebuilt, never read** (a two-hex-digit `printf`
  trap spelled the wrong glyph).
  ⚠ **A footprint PREDICTION is derived from the same KIND of run that will measure it — the
  two-seeded `-stdlib` emission — never from a single-package probe** (2026-09-03), whose numstat
  carries the import-init hook closure family the driver keeps: a probe predicted −86 where the
  emission read −77 plus the placeholder. Post the predicted number AS READ, never rounded, and score
  a miss against master line by line rather than explaining it away. Its companion: **a footprint's
  SHAPE can be predicted while its LOCATION cannot** — a defect measured only in the probes built to
  find it landed on one production field, and a footprint smaller than sized and on REAL code is
  better evidence than a large one on guards ("every instance so far is in my own probes" is a
  statement about the census, not about the corpus).
  ⚠ **A BASELINE NUMBER IS DERIVED FROM THE TREE BEING MEASURED, AT RUN TIME — never remembered from
  another tree** (two instances, 2026-09-04). A count prediction that SPANS A TRAIN carries the
  train's own additions: a census predicted `665 − 1 = 664` holding the PREVIOUS train's project count
  fixed while the train being measured seated four new guards, and both legs read `668 = 665 + 4 − 1`
  — so **predict the DIFFERENCE the change makes** (cross-leg: 668/15 against 669/14, exactly one, and
  that one the marked project) and derive totals from the tree at run time, never the reverse. Its
  one-scale-smaller sibling: a length remembered from a DIFFERENT worktree is not a baseline — an
  appended record read as "+372 lines" against a length measured in a pin tree, while the cut sat on a
  landed master the file had already grown under, where `git diff --numstat` against the cut's OWN
  base read 55/0. **A number that surprises you is checked against the instrument before it is
  reported.**
  ⚠ **A census's SCOPE must match the EMISSION it predicts** (2026-09-04): a `-stdlib` footprint is
  scored by the PRODUCTION-only census, because `-stdlib` never emits test packages, while the
  with-tests figure answers the DEMAND question — one increment's "~130 predicted sites" was the
  with-tests number against a measured production-only **12**, exactly the six packages the production
  census named. Two censuses, two questions: a prediction quoted from the wrong one is corrected in
  the commit, and the ruling made from the right one stands.
  ⚠ **A CENSUS PREDICATE AND AN EMISSION PREDICATE ANSWER DIFFERENT QUESTIONS, and a gate carried
  from one into the other unexamined refuses what the emission already makes correct** (the
  defer-to-`finally` lowering arc, 2026-09-04). "Direct child of the body" was a cheap conservative
  proxy for a POPULATION census and was carried into the cut unchanged, while the per-site reached
  flag the cut ITSELF emits makes a conditional defer correct by construction — so the gate refused a
  real site and the arc's acceptance row did not move under it, found by READING Go's source after the
  acceptance had been re-sized from a segment table. **Every gate carried from a census into an
  emission is re-derived from the emission's own mechanism**; a correct cut whose acceptance row does
  not move lands with that null STATED in its commit; and the widenings that WOULD move the row are a
  sized successor with their own census and ordering proof, never a re-cut of a measured increment.
  ⚠ Worse than conservative: **a gate that is CORRECT as a census heuristic can be UNSOUND as an
  emission rule, and the two read identically in code.** A prefix gate scanning preceding statements
  with `ast.Inspect` walks INTO an `if`, a `switch`, a loop and a func literal, so a dereference on a
  branch that may never run witnessed one that must have — over-counting a population by four in a
  census, and silently moving a nil-receiver panic past an already-run body in an emission (four of
  170 qualifying sites leaned on exactly that). Neither CNR nor a build can see it: both arms compile
  and emit fine. What found it was re-reading the predicate against the sentence that justifies it and
  noticing the code does not establish it. **Where a gate's justification is "this provably ran
  before", the gate COMPUTES that relation and never approximates it with syntax** — three gates in
  one arc were each a syntactic proxy narrower than the real thing (a witness required at body level;
  an `if`'s INIT and COND, which run unconditionally once the statement is reached, skipped with its
  body — a third of one population, qualify 182 → 226, hidden behind it; and an own-block-only scan
  blind to an outer statement preceding the enclosing construct), each refusing sites the widening
  existed to admit, the last caught by a NAMED falsifier rather than by a compile error. So: walk OUT
  through the ancestor chain collecting each level's earlier siblings plus every enclosing
  `if`/`switch` init and condition; scan an `IfStmt`'s Init and Cond, a `SwitchStmt`'s Init and Tag, a
  `TypeSwitchStmt`'s Init; and refuse bodies, `select`, loops and labels — an `ast` walk that must
  prove "always executed" skips every branching node and refuses to descend into a `FuncLit`.
  ⚠ **The census is the INSTRUMENT, not the CONTRACT**: when the census predicate and the converter
  predicate — two independent implementations of one rule — disagree, read the DIRECTION before
  relaxing anything. Two predicted paths that did not move rooted to the converter requiring a
  function's OTHER defers to be LOWERABLE where the census required only TOP-LEVEL: the stricter side
  cost population, not correctness, so the census is recorded as over-counting by exactly that class
  and the converter is left as it is. A falsifier phrased as "any path OUTSIDE the prediction" catches
  the dangerous direction; absences INSIDE it are sized, rooted and stated. And **a census that must
  agree with a converter gate ports the converter's predicate VERBATIM** (the `provablyBefore` pair
  copied into the census), so the two run the same code and the only difference left is SCOPE — a
  different and stated question: one arc's "166 against 129 reached" was predicate drift, not a class
  to live with.
  ⚠ Two prediction rules from the same arc. **Predict the quantity the INSTRUMENT measures**: an A/B
  whose PRE arm is the PREVIOUS increment's tip measures only the new increment's delta, so a file
  whose qualifying sites the previous increment already lowered shows NO change — a 54-file census
  population was predicted against an arm that read 21, all inside the set, the falsifier silent and
  the number wrong for a reason the data already held. And **a population prediction names the SHAPE
  it counts** — "deferred method calls" (everywhere, mostly on locals) is not "a method on the
  enclosing method's own receiver" (332), and a wide band around the wrong population still misses by
  a factor. **A source-level census cannot see EMISSION properties**: 10 of 65 lowered calls kept
  their box because the deferred method is a PROMOTED method on an EMBEDDED field, reached through a
  field-reference box the lowering moves into the `finally` rather than removes (correct, LIFO holds,
  the delegate still saved) — so the receiver-method bucket has two byte profiles it does not split,
  stated rather than chased.
  ⚠ And the lowering's own three-part obligation, kept as doctrine because it documents a HAZARD
  rather than a heuristic: lowering a Go `defer` into a C# `finally` moves THREE things from
  registration time to exit time, each a silent wrong answer — the RECEIVER's evaluation, the
  ARGUMENTS' evaluation, and the REGISTRATION itself. So a receiver, or any PREFIX of its path,
  reassigned afterwards disqualifies the site, as does an argument changed afterwards; and because the
  call is registered only if control REACHES the defer statement, the lowered call is guarded by a
  local flag set at the defer's own source position unless the defer is the body's first statement. **A
  gate that reads ZERO on today's corpus is kept anyway when it documents a hazard rather than a
  heuristic**, and a predicate built from a census is re-read AT THE EMISSION SITE before the cut —
  which is where this one was found.
  ⚠ **A ZERO corpus footprint is a FALSIFIABLE CLAIM, and is banked only when it is EXPLAINED**
  (2026-09-04). The claim is "these shapes cannot occur in a compiling corpus", settled by the
  two-seeded diff with any non-empty diff posted as HUNKS before anything is applied — and what turns
  a zero three-target diff (18,720 files a side, only the run's own timestamped reports differing)
  from luck into a statement is a POSITIVE-CONTROLLED census of the pinned GOROOT, production AND
  `_test.go`, finding zero occurrences of the trigger shapes: the fix is for end-user Go reached
  through `-recurse`. **A reviewer wanting a non-trivial footprint should be suspicious of the FIX,
  not reassured by the zero.**
  ⚠ **Amended 2026-09-04: a predicted-ZERO production footprint turns the two-seeded `-stdlib` diff
  into the NEGATIVE ARM, not into a gate to drop.** "Zero movement" is a prediction that can FAIL, so
  the diff runs with its positive control beside the `-tests` emission census that measures the change
  — "empty by construction" applies only where nothing PREDICTED it empty.
  ⚠ **And a std census answers "how many sites in std" — it is BLIND to the BEHAVIORAL corpus, a
  SECOND population with its own shapes** (2026-09-05): the chan-of-array `make` the std census found
  ZERO of in production existed once in the behavioral tree, and CNR is what censused it (CHANGED =
  one golden). **A footprint prediction names BOTH populations, and CNR's CHANGED set IS the
  behavioral census.**
  ⚠ **A DISPLACED BODY TAKES ITS CONVERSION SITES WITH IT, and no guard sees it** (measured
  2026-09-03): registering a function in `manualConversionFuncs` drops every `GoImplement` record the
  converted body minted (two endian-order records vanished from `package_info.cs`, in a seeded run
  too, since a package conversion does not merge existing records). The CLOSURE COMPILE is what
  caught it — the assignment compiles only through the operator the record generates — so the
  companion DECLARES the records it performs, spelled as the file spells its own, and the closure
  build is the witness. A hand-own displacing an `init` declares `[GoInit]` itself, since a displaced
  init emits only the placeholder.
  ⚠ **Two census-scope rules for a footprint over converted C#.** Anchor on the alias family for
  **package** qualifiers, not only type names — `Ꮡ((Δ)?pkg.Var)` — because the converter mints a
  Δ-prefixed alias for an imported PACKAGE as readily as for a type: a name-keyed census of one fix's
  sites counted 5 where the `(Δ)?`-anchored one counted 10 in 5 files (2026-09-03). And **a corpus
  compare includes `*.cs.auto`** whenever the change can reach a hand-owned file's declarations: a
  `*.cs`-only walk is blind to the OWNER-arm emission of a whole-file `[module: GoManualConversion]`
  hand-own, whose converter output lands in the `.cs.auto` review sibling while the committed `.cs`
  stays hand-written by design. The sibling's drift is the standing `.cs.auto` class — named, not
  overlaid — but the compare must still SEE it to confirm the owner arm reached no other hand-own.
  ⚠ **A converter fix that ADDS a branch is proven SURGICAL by measuring that the OTHER branch
  reproduces existing emission byte-for-byte AT THE BASE** (2026-09-03) — the surgical claim is a
  measurement, not an argument.
  ⚠ **Scope DELETES by TRACKEDNESS as well as by lane prefix — `git ls-files` decides** (paid three
  times, 2026-09-03). A cleanup glob `rm -f …/go2cs_test_*.json` deleted the TRACKED disclosures
  manifest `go2cs_test_disclosures.json` (pipeline artifacts and a committed manifest share the
  prefix), and `rm -f *_test.cs` deleted TRACKED test sources twice in one night because the reflex
  was carried from an UNBANKED row (test emission untracked, glob correct) to a BANKED one (test
  emission committed) — the two rows differ in exactly the property that matters. The durable form is
  a STEP, not care: **`git status --porcelain | grep '^ D'` after any cleanup, asserting
  `deleted-tracked: 0`, before the next command**; clear emission with `git clean -nd` then `-fd`.
  Companion: a bank-check's own `-tests` pass OVERWRITES a merged non-marked file — restore from the
  preserved artifact and re-run every footprint invariant after any pipeline pass.
- **Reconvert → overlay → build → bucket (the measurement loop):**
  1. **⚠ SEED FIRST — non-negotiable (learned 2026-07-25, cost a false operational-break alarm):**
     `cp -r src/core <tmp>/core` BEFORE reconverting. ⚠ The SEED ITSELF can fail halfway and
     carry on (fleet-confirmed twice in one day, 2026-09-01): `Copy-Item -Recurse` dies on
     go2cs-gen's long `obj\...\Generated\` paths, and under the ritual's own required
     `$ErrorActionPreference='Continue'` the copy continues past the death — a PARTIAL seed,
     which is the unseeded-root hazard through a door this rule never named. Exclude build output
     (`bin`/`obj`/`Generated`) from the seed copy and verify the seeded `.cs` COUNT before
     converting; afterward, an emitted-files control (untouched seeded files reproducing HEAD
     byte-for-byte) is what makes a suspect seed's readings trustworthy. The converter emits a hand-owned
     file as `<file>.cs.auto` ONLY when the `[module: GoManualConversion]`-marked file already
     exists at the output path; an EMPTY temp root gives the marker nothing to detect, so every
     hand-owned whole-file rewrite is emitted as plain `.cs` and the standard overlay rule
     ("copy `*.cs`, exclude `*.cs.auto`") protects NOTHING — 14 hand-owned files get clobbered
     with auto conversions that COMPILE but are operationally broken (godebug's auto `init()`
     throws in a module initializer and takes down every dependent). **Hard gate before
     overlaying — PATH-PRECISE, not a count:** for every `[module: GoManualConversion]`-marked
     committed file, the temp root must NOT contain a freshly-EMITTED plain `.cs` at that path
     (either a `.cs.auto` sits beside it, or nothing was emitted there). Counts intentionally
     differ — **41 marked files (re-measured r44a, 2026-08-07) but only 15 produce `.cs.auto`**; the
     other 26 are `*_impl.cs` companions and hand-owned packages the converter never re-emits at
     that path, so they need no protection. A same-count assertion is wrong in both directions.
     ⚠ The number is NOT stable: it was 40 at r40, fell to **39** when `math/unsafe.cs` shed its
     marker, returned to 40 when `internal/weak/pointer.cs` joined at r43e, and is **41** since
     `internal/cpu/cpu_x86_impl.cs` joined at r44a. This is exactly why the census is re-measured,
     never carried forward.
     **The marker scan must read WHOLE FILES** — a head-window scan (e.g. first 40 lines) reported
     35 marked files against the real 60 (measured 2026-08-17), which would have made the clobber
     gate vacuous for 25 hand-owns; some markers sit below long license/using blocks.
     **The marker scan must be LINE-ANCHORED (`^\s*\[module:\s*(go\.)?GoManualConversion\]`)** —
     `reflect/value.cs` and `internal/reflectlite/value.cs` *mention* the marker inside
     bodyless-partial placeholder comments; an unanchored `grep GoManualConversion` reports **63**
     against the real 40 and turns the gate into a false clobber alarm. (The census moves in BOTH
     directions — 32 at r14, 39 before `internal/concurrent`'s `hashtriemap.cs` joined in r39d, 40
     at r40, DOWN to 39 when the r41 train's regen retired `math/unsafe.cs`'s hand-own without
     saying so — the BitConverter bit casts went back to the auto
     `Ꮡf.Reinterpret<float32, uint32>()`, correct now that `Reinterpret` genuinely aliases managed
     storage, and `math`'s banked **76/76** re-proves it every sweep — and back to 40 when
     `internal/weak/pointer.cs` joined at r43e. Benign in that instance, but a hand-own disappeared
     under an overlay while the commit reported its marker gate "40/0": so re-measure the census,
     never assert last session's number, and treat a SHRINK as something to explain rather than to
     copy forward. ⚠ Since r50a the census counts **42**, and for a NEW reason: layout L3 routes a
     hand-owned file into its principal's per-GOOS folders, and `runtime/lock_sema_impl.cs`'s
     principal is selected on Windows *and* macOS — so one hand-own now exists as TWO files. The
     count of marked FILES is no longer the count of distinct hand-owns; both numbers are fine and
     the gate is still per-PATH. Since **r51b it is 44**: `runtime/lock_managed_impl.cs` (the flat,
     platform-neutral managed core of the mutex/note protocol) and `runtime/linux/lock_futex_impl.cs`
     (the futex flavor's 2-arg `notetsleep_internal`) both carry the marker. Multiple `[module:
     GoManualConversion]` attributes in ONE assembly are legal and already normal — `runtime` alone
     carries eight — so a new marked file never needs to displace an existing one. ⚠ At the r59
     regen bank (2026-08-11) the census is **49 marked files / 41 `*_impl.cs` companions / 59
     distinct hand-owns** — r52–r59 growth over r51b's 44; re-measure, never carry, as always.
     At the Linux regen wave (2026-08-14) it is **53 marked files / 42 `*_impl.cs` companions**,
     0 violations across 3 targets × 2 merge passes. At the post-merge rebank (2026-08-24) it is
     **73 marked files / 49 `*_impl.cs` companions / 24 whole-file rewrites**, 0 violations on the
     windows and the linux target alike.
     The regen ritual also gained a check the seed makes necessary: because seeding puts every
     repo file in the temp root, an overlay can never reveal a file the converter has STOPPED
     emitting — classify emitted-vs-seeded by the sentinel mtime and report would-be deletions,
     which is what surfaced the hand-owned-by-consequence class below.) ⚠ The `.cs.auto` siblings are **tracked in git but are NOT refreshed by the
     overlay**: the same exclusion that protects the hand-owned `.cs` beside them also freezes
     them, so they go stale on their own schedule and are RE-MEASURED at every rebank head rather
     than assumed (CleanupBacklog item 18). The measurement moves: 11 of 16 were stale at r40,
     and **0 of 23 at the 2026-08-24 post-merge rebank** — a seeded reconvert per target re-emits
     each sibling, and CR-stripped equality against the committed file is the test (a raw byte
     compare reports the whole set as differing, because a fresh emission carries the in-literal
     LF the working tree holds as CRLF).
  1a. **⚠ SEED `version.props` AND `docs/validation` TOO, and MIRROR THE `src/` LAYOUT** (added
     2026-08-02 with the README validation badges). Each package README's badge LINE carries two
     badges, and the **Tests** one is composed from two REPOSITORY files, not from the conversion:
     `src/version.props` (the published version that pins the proof URL) and
     `docs/validation/current/<dot-id>.md` (the matched/disclosed counts).
     The converter finds both by the same upward walk it uses for `$(go2csPath)` — version.props at the
     root holding `core/golib`, `docs/` as that root's SIBLING — and emits **no Tests badge at all**
     when either is missing, which is a silent, corpus-wide README diff on overlay. So seed
     `<tmp>/src/core`, `<tmp>/src/version.props` and `<tmp>/docs/validation`, and convert with
     `-go2cspath <tmp>/src` so the temp root mirrors the repository. (Seeding a versioned
     `docs/validation/<version>/` is NOT needed — the badge reads `current/`; the versioned directory
     is only the link target and the `Exists`-guarded pack input.)
     ⚠ The badge line holds FOUR badges and they split two-and-two on this exact question. **Docs**
     (2026-08-08) and **Source·Go** (2026-08-08) read the TOOLCHAIN, not the repository — `go env
     GOVERSION` and, for the 19 GOROOT-vendored `golang.org/x/*` packages, GOROOT's own
     `src/vendor/modules.txt` — so they need no seeding and survive an unseeded root. **Tests** and
     **Source·C#** read the repository's `version.props`, so both vanish without it. That is why an
     unseeded reconvert no longer produces a README with NO badge line: it produces one carrying the
     two toolchain badges alone, which is a subtler diff to spot. Seed anyway; the rule is unchanged,
     only the symptom is.
  1b. `go2cs.exe -stdlib -comments -go2cspath <tmp>/src` → output lands in **`<tmp>/src/core/<pkg>`**
     (the `core` subdir is hardcoded; `-go2cspath` is the *output* root, unrelated to the MSBuild
     `$(go2csPath)`). Full stdlib ≈ 3–4 min (per-file work is sub-second; the cost is `go/packages`
     loading the whole type graph, so **batch** — don't invoke per package).
  1c. **⚠ NEVER convert twice into the same temp root, and never let two conversions overlap in one
     (found r41, 2026-08-05).** A `-stdlib` run whose PowerShell wrapper aborted on the converter's
     stderr WARNINGs — `$ErrorActionPreference = 'Stop'` turns a native-stderr line into a terminating
     NativeCommandError, so wrap the converter call in `'Continue'` or do not pipe its stderr at all —
     left a `go2cs.exe` alive; a re-run into the same root raced it, and the result was ONE corrupted
     file: `runtime/arena.cs` with nine unresolved `«DYNTYPE:…:DYNTYPE»` anonymous-struct lift markers,
     which fails the corpus build with CS1056/CS1003 and reads exactly like a converter regression. It
     is not one — a clean-room reconvert (fresh root, seeded, single run) emits zero DYNTYPE markers
     anywhere in the corpus, and so does a single-package run. The rule is therefore mechanical rather
     than diagnostic: **delete the temp root and re-seed for every reconvert**, and confirm no
     `go2cs.exe` is alive before starting one.
  2. Overlay the fresh `.cs`, **`.csproj` and `README.md`** onto `src/core/<pkg>`. Since the trees
     unified (2026-08-01) the reconvert's paths ARE the repository's paths — a straight copy, no
     rewriting, no exceptions. A seeded reconvert of the whole stdlib is byte-identical to the
     committed tree (2518 `.cs`/`.csproj` verified on the consolidation commit; 300 `README.md` joined
     the byte-identical set on 2026-08-02), so any diff after an overlay is a real converter change.
     Two knowns that are NOT: the SIX root attribution files the converter re-copies (`src/core/README.md`
     and its five siblings — measured 2026-08-17; this note previously named only the one — all show
     modified with an EMPTY `git diff --numstat`, pure CRLF phantoms; restore them), and
     the hand-owned-by-consequence **class of FOUR** — `crypto/internal/boring/bcache`,
     `internal/concurrent`, `internal/godebug` and `internal/weak` (censused 2026-09-01 at
     `3e31de03a` over all 306 production packages; the note previously said three, and before that
     godebug alone — bcache was the member nobody had counted, evidenced by the hand-edited
     position-map hash in its `package_info.cs` at `f1df6cbd9`, which a re-emitting converter would
     never need a human to fix) — each a package whose every non-test Go file is hand-owned, so
     `unmarkedFileCount == 0` makes the driver `continue` before `writeProjectFile` and its
     `.csproj`, `package_info.cs` **and `README.md`** are hand-owned by consequence, never
     re-emitted. (`unsafe` is also fully hand-owned but by the OTHER mechanism — skip-listed.)
     Consequence counted the same day: the hand-own FENCE leaves **8 forced-init hooks missing**
     inside this frozen class (godebug 4, concurrent 3, weak 1) that only Stage B's frozen-README
     option (a) can fix — the relocation cannot, since these `package_info.cs` are never re-emitted.
  3. Build single packages with **`dotnet build <pkg>.csproj -c Debug`** — `src/core/Directory.Build.props`
     pins `$(go2csPath)` to the src root, so `core\golib` + the `go2cs-gen` analyzer resolve to live source
     with **no `-p:go2csPath` flag**; or build the whole `go2cs-stdlib.slnx` (~92–150 s warm, 305 assemblies — the 306th, `crypto/x509/internal/macos`, is darwin-exclusive and compiles nothing under the default `$(GoTargetOS)`).
     (If you ever do pass the flag explicitly, use forward slashes —
     `-p:go2csPath=H:/Projects/go2cs/src/` — a trailing `\` escapes the closing quote and mangles the path
     into phantom golib-not-found errors.)
  4. Bucket: `dotnet build … -clp:ErrorsOnly` then group by `error CS####`. Errors shown are *own-errors*
     of leaf-most failures — dependents of a failed project are skipped, not errored.
- **⚠ csproj I/O (2026-07-25):** any script that rewrites a csproj must read AND write with explicit
  UTF-8/no-BOM (`[System.IO.File]::ReadAllText/WriteAllText` + `UTF8Encoding($false)`) — PS 5.1
  `Get-Content` reads the converter's BOM-less UTF-8 as ANSI and `Out-File utf8` re-encodes the damage,
  double-encoding the `©` in `<Copyright>` on every pass (this is what created, then tripled, the
  258-file corpus mojibake; root-caused and leveled in the r11 bank). Python has the same trap in
  the OTHER direction: `utf-8-sig` STRIPS a BOM on read but always ADDS one on write, so a
  read-sig/write-sig round trip silently BOMs a BOM-less file (caught 2026-08-31 by the
  hand-application byte-identity bar during a probe restore). Three encodings, three silent
  corruptions — PS 5.1 ANSI, UTF-16 redirects, utf-8-sig — one rule: byte-compare any
  restore/round-trip against the original before trusting it.
  ⚠ **The BOM round trip breaks in BOTH directions, and the second was measured 2026-09-03:** reading
  with `utf-8-sig` and writing with plain `utf-8` **STRIPS** a BOM the file had — two `go2cs-gen`
  sources lost theirs, caught only by reading the FIRST line of the diff (`-<BOM>// …` against
  `+// …`). **After any scripted edit, diff the file's FIRST and LAST lines specifically**: encoding
  damage (BOM, trailing newline, CRLF) lands there, and review attention is weakest there.
- **Metric:** measure **packages-compiling**, not raw error count. Fixing file-inclusion bugs (e.g. the
  filename build-constraint fix) *raises* the error count because newly-included files surface their own
  latent defects — that's progress, not regression. The claim "my fix caused N new errors" is
  therefore never banked without the five-minute control (named 2026-09-01, after a
  substantially-correct fix was discarded on the misread): REVERT the fix, build PAST the original
  blocker, and see whether the "new" errors are still there. Unmasked errors appear precisely where
  compilation could not previously reach — i.e. in files OTHER than the ones you touched — so "the
  errors are in different files from my change" is evidence of unmasking, never evidence of
  causation.
  ⚠ **The rule read from the other direction: a CLEAN second half is believable only after the first
  half is FIXED** (2026-09-05, the field-walk dedupe): fixing the walk UNMASKED a CS1929 on the METHOD
  half of the same row that the unfixed generator had reported clean — and the second half needed a
  DIFFERENT fix (depth-aware promotion inside an embed), not the first fix applied twice. Errors behind
  a blocker are invisible until the blocker moves, so a two-half row is re-measured after each half.
- **A corpus regen that moves `package_info.cs` records owes `go generate .` in `src/go2cs`** —
  `stdlib-metadata.txt` is generated FROM the corpus and gated by `TestStdLibMetadataInSync` under the
  plain converter `go test`, so banking a regen without the regenerate leaves the converter gate red at
  master for whoever runs it next (happened 2026-08-15: the second leveling regen moved 6 records and
  the drift surfaced in an unrelated lane's gate run). Regenerate, verify the test, commit together.
- **Don't commit corpus regens casually.** `src/core/<pkg>` is regenerable; the unit of work is the
  **converter fix**. Keep the tree restorable (overlay into a branch or restore with `git checkout HEAD --`
  + remove untracked) so a converter-fix commit isn't buried under thousands of generated-file changes.

## Current state & known issues

- **One tree since 2026-08-01:** the stub baseline retired and the converted standard library moved into
  `src/core`. `src/go2cs.slnx` builds clean and the behavioral suite is GREEN against the CONVERTED
  packages — **547/547** transpile+compile+golden, **521/521** stdout comparisons vs `go run`
  (r43g, 2026-08-07; 26 skipped, no `package main`). All rewrite machinery is gone: one path scheme,
  `$(go2csPath)core\<pkg>`, everywhere.
- **Windows local time works (fixed 2026-08-01).** Binding the converted `time` exposed a pre-existing
  crash the stub had hidden: `time.Now().Weekday()` → `initLocal()` → `syscall.GetTimeZoneInformation`
  access-violated, because the wrapper hands the kernel the address of a managed `Timezoneinformation`
  whose `array<uint16>` name fields are managed references where Windows expects inline `WCHAR[32]`. That
  wrapper is now hand-owned against a blittable mirror (`core/syscall/windows/zsyscall_windows_impl.cs` — per-GOOS since r50a), guarded
  by the `LocalTimeZone` behavioral test — which compares real zone abbreviations and offsets against
  `go run`, not merely the absence of a fault. **The CLASS is still open, and it is now TWO classes**
  — wrappers passing a non-blittable struct by ADDRESS (the layout defect above), and wrappers taking
  a `**T` OUT-parameter, which arrive as NULL because `ж<T> → uintptr` answers 0 for a heap-boxed
  pointer that is still nil. The running census, the per-member remedy and why they are deliberately
  NOT fixed speculatively live on
  [`docs/phase4/BOARD-next-validation-candidates.md`](docs/phase4/BOARD-next-validation-candidates.md);
  re-measure there rather than carrying a count. The old note said "nothing exercises them today;
  `net` and `crypto/x509` will" — both now do: `net`'s DNS path forced `GetAddrInfoW`/`FreeAddrInfoW`
  (fixed 2026-08-16, guarded by `LookupServicePort`), and `crypto/x509`'s Windows system verifier is
  the measured consumer of the OUT-parameter class. Two walls stood behind them, both `net` /
  `crypto/x509` arcs rather than syscall ones — a **third** fork, where the kernel memory is a byte
  buffer the CALLER reinterprets, so no wrapper is at fault and no mirror-the-wrapper remedy applies.
  The first is CLOSED: `net.adapterAddresses` walked a native `IP_ADAPTER_ADDRESSES` chain out of a
  managed byte buffer and killed the process on the loop's own nil test; it is hand-owned since
  2026-08-17 (`core/net/windows/interface_windows_impl.cs` transcribes the whole chain — every
  record, its six nested lists and every sockaddr — into managed boxes), guarded by the
  `IpAdapterAddresses` behavioral test, and it is what unblocked Windows name resolution at all,
  since `dnsReadConfig` is `getSystemDNSConfig`'s only source of DNS servers. The second is still
  open: the CryptoAPI chain walk reads `CertContext` / `CertChainContext` back through raw addresses.
  ⚠ **The class has its ROOT, and it is not "one word where four bytes belong" (measured 2026-09-02):
  the CLR gives AUTO layout to any struct holding a reference-typed field and REORDERS it, so the
  KERNEL READS THE WRONG FIELD.** Converted `Msghdr`'s `Namelen` sits at managed offset 40 while the
  kernel reads `msg_namelen` at 8 — where it finds `Iov`, an object reference and therefore never zero:
  EISCONN on a connected stream, EINVAL on a datagram; `RawSockaddrInet4`'s `Addr` at managed offset 8
  is the heap pointer earlier instrumentation dumped. A correctly laid-out struct (`Iovec`) can still
  hand the kernel managed addresses, so the remedy is to ENCODE into a native buffer (a
  `writeNativeSockaddr`) or an explicit-layout blittable mirror — never a managed struct passed by
  address. A three-arm A/B (hand-own / generated body restored / hand-own back, `--no-incremental`) is
  what turns a "does not reproduce" into an attribution.
  ⚠ **That remedy has a BOUNDARY, and an identity seam sits on the far side of it** (2026-09-06):
  **marshalling answers a LAYOUT problem; a pointer the OS issued and REFERENCE-COUNTS is an IDENTITY
  problem, and no correct marshalling can answer one.** A fallback handing the kernel a marshalled
  native image of a managed view hands back an address the OS never issued — fatal where the site FREES
  memory the OS owns — so the remedy for a MISS on an identity seam is to **REFUSE BY NAME**, never to
  rebuild; the hit path already knows this, since it REMEMBERS an address rather than reconstructing
  one. Offering marshal-on-miss and refuse-by-name as two options on ONE axis is the sizing error, and
  they answer different problems: the certificate-context helper censused five producers — three
  native-backed, where the fallback is right, and two managed views that both remember — leaving the
  reaching set for the fallback EMPTY.
  ⚠ **The class's FIFTH member, and the reading rule it re-proves (2026-09-03):** `readMapping` handed
  `Module32FirstW` a managed `ModuleEntry32` — auto-layout ~64 B against the native 1080, with
  `module.Size` folded to 1080 — overwriting a kilobyte of heap, faulting in one test and resolving as
  an unrelated cctor crash in another. The routed description named two other functions entirely; the
  RECORD named neither. **A finding's prose is re-derived from its record before a cut is aimed.** The
  remedy is the class's: a `fixed`-buffer mirror plus a registry displacement, and its positive
  evidence is a downstream test getting PAST the API that validates the record size — an UNGATED row
  can read "unchanged" after a real fix when an earlier host-killer runs first, so only a gated run
  measures it.
  ⚠ **A TOKEN CUT'S ACCEPTANCE NAMES THE PLATFORM WHERE THE NATIVE SIDE DOES NOT PROBE** (2026-09-05,
  a union battery): "EFAULT rather than reordered memory" holds where libc merely READS the pointer;
  on Windows `ntdll` (`RtlGetVersion`) **WRITES** through it and the process faults 0xC0000005 — so the
  class such a cut makes LOUD (reference-bearing structs passed by address) is a PER-PLATFORM census
  question, and every REACHED member is a hand-own of the `GetTimeZoneInformation` shape. Two behavioral
  guards went red on `rtlGetVersion`, reached on every Windows TCP dial, where master had read silent
  zeros.
  ⚠ **A SECOND native-boundary class, measured 2026-09-03/04: LIFETIME, not layout.** golib's
  `uintptr` operator pins DURABLY but only for the BOX's lifetime, so a call site passing
  `(uintptr)Ꮡ(a, 0)` without HOLDING the box lets the backing array move during the native call (an
  unpinned array shown to MOVE first, then ARM 2 reading 0 and ARM 3 stable with the box held) —
  "durable" is not "unconditional". Sharper: the pin's holder is FINALIZABLE and sits on a box that is
  garbage the instant the take returns, so the pin is released on the finalizer's schedule and "the
  four takes are atomic" is true only while nothing runs between them. **The population is the
  PREDICATE (an address reaches a native call with no holder alive across it), never the idiom** — 43
  `(uintptr)Ꮡ(` sites are the idiom's count, not the hazard's. Its worst form compiles, emits
  byte-identical C# and passes every SERIAL gate: the `syscall` read/write wrappers mint their buffer
  pointer through the NON-RETAINING door (an implicit box-to-`uintptr` conversion), so only
  concurrency inside the kernel window sees it — 77 sites, `KeepAlive` **zero** corpus-wide, an
  absence turned into a measurement by a GOROOT-side derivation of the shape. **A pointer type with a
  retaining and a non-retaining constructor is a trap wherever an implicit conversion can select the
  non-retaining one**; mint through the retaining door, and note that a door taking its address inside
  `fixed` retains the BOX but not the PIN — retention and pinning are two properties a kernel-bound
  pointer needs both of.
  ⚠ **A B/op census cannot SEE what a pin COSTS** (2026-09-04): the `GCHandle`'s handle-table entry
  never lands on the GC heap at all, and a FINALIZABLE holder whose finalizer is suppressed only on a
  `Dispose` the contract never calls rides the finalization queue, is promoted at least a generation,
  and frees its handle on the finalizer thread — GC pressure proportional to pinned-box COUNT,
  reported as ZERO bytes. **Price a pin from the TYPE** (header, fields, padding, the handle, the
  finalizer) **and from the QUEUE it joins**, never from the bytes a run reports. And a remedy that
  changes WHERE storage is allocated (pinned-object-heap boxes) is its OWN increment with its own
  footprint and cost pair — never folded under the correctness cut whose evidence it would otherwise
  borrow.
  ⚠ **TRANSCRIBE the reference compiler's predicate rather than paraphrase the rule** (2026-09-04, the
  root of that gap): the converter's KeepAlive analysis carried the sentence "never through an
  intermediate variable", but `cmd/compile`'s `escape.rewriteArgument` tests the OPERAND TYPE
  (`IsUnsafePtr` into `IsUintptr`), so Go's guarantee covers the two-step form its own generated
  wrappers use 16 times per platform — **one wrong sentence in a comment was the whole gap**, and the
  fix is the compiler's predicate in the converter. Its census sibling: **a closed arc's set can be
  NARROWER than the Go contract it says it reproduces** — Go marks FOUR functions
  `//go:uintptrkeepalive` (`RawSyscall`, `RawSyscall6`, `Syscall`, `Syscall6`) and the converter's
  keep-alive funnel set covered only the `Syscall` family, leaving eleven generated Linux wrappers
  handing the kernel addresses with no keep-alive; re-derive such a set from Go's own annotations.
  ⚠ **Two more members of that class, measured 2026-09-05.** It has a MANAGED-CALLEE member: a
  `(uintptr)` bridge strips retention from a PINNABLE same-frame box exactly as it does from a syscall
  buffer, and a CONVERTED callee that resolves the number through validate-on-read refuses after any
  GC between the mint and the resolve — the landed `KeepAlive` fix's predicate was FUNNEL-shaped and
  never modelled a converted callee taking `unsafe.Pointer`, and a weak token does not retain either.
  **A retention fix is keyed on the ARGUMENT's shape (a bridged same-frame box), never on the callee's
  KIND.** And **a NAME-keyed predicate is per-platform BY CONSTRUCTION**: that fix's funnel set was
  the uppercase `Syscall*` family while darwin funnels through lowercase libc trampolines, so it
  covered ZERO darwin sites while its census guard read green on every train — **a guard reading ZERO
  on a target is either "no sites" or "blind", and only a PER-TARGET positive control (the guard RED
  on a tree known to carry unheld sites there) tells them apart.** Route #8's shape, one target over.
  ⚠ **A REMEDIED member of a class can still be EXPOSED when the class's RULE changes underneath it**
  (2026-09-05): a hand-own written BEFORE the managed-pointer token cut can be correct in SHAPE and
  broken in FACT if its own body takes the `uintptr` of a reference-bearing box — so **census the
  COMPANIONS' INTERNALS, not only the undisplaced wrappers.** What closed that question is the shape
  of the answer rather than its emptiness: **a NEGATIVE census is worth what its SHAPE is worth** —
  "no hand-owned companion is exposed" carried because 72 of 96 conversions were MEASURED to take a
  native pointer, positive evidence that the remedy's shape had been applied consistently, where an
  empty grep would have closed nothing. **State what the population DOES, not only what it lacks**,
  and name the limits (here an unflagged struct and the caller-side variant) that make the closure
  honest.
  ⚠ **A box cannot OBSERVE a write performed through a `ref` it has already handed out** (2026-09-05):
  the converted `*(*uintptr)(unsafe.Pointer(&slot)) = addr` writes through `ж<T>.Value`'s ref over the
  same managed storage, so a golib-side hook at the reinterpret seam runs when the VIEW is taken and
  not when the value is STORED — the remedy for a pointer-typed destination is a converter EMISSION
  change, and **an increment whose halves are converter + golib owes BOTH gate families and lands
  together.** Its record-first companion: `NativeBox<array<T>>` over Go's headerless `*[N]T`
  reinterprets element bytes as a managed `T[]` HEADER — it IS the prestub null read it was meant to
  fix — and a design refuting its own shorthand on paper is the argument for writing the record first.
  ⚠ **A RULE KEYED ON THE WRONG ATTRIBUTE IS A LOWER BOUND, NOT A RULE — and the ruled remedy is a
  THIRD ANSWER declared ABSTRACT on the base** (2026-09-06). The pointer operator asked a PINNABILITY
  property ("can this be held still?") the question "is there an address here at all?", and those
  differ for every field or element reference rooted in a reference-bearing CONTAINER whose pointee is
  reference-FREE — the class that regressed every Windows dial. The narrow remedy, testing the
  POINTEE's type, restates the same confusion one level down and leaves a human obligation with no
  compiler behind it; declaring the answer ABSTRACT makes every box kind STATE it or the assembly does
  not compile. Two things that bought, neither party having them in mind. **A lane's BASE can lack a
  type the UNION contains**: the repair implemented the new member for all SIX box kinds its base
  carried and the assembly — which also carries a SEVENTH kind from another lane's increment on the
  intervening train — failed at the first build with CS0534, where the pointee-typed predicate would
  have COMPILED at the union and given the seventh kind an answer to the wrong question. And **a
  comment recording WHY a member is abstract, at the site, turns the next author's surprise into an
  instruction** — the eighth kind's author meets the same build error, and the difference between a
  wall and a signpost is one paragraph naming the error code and what it prevented, paired with the
  argument from the other side (ordering forwards to the SOURCE's token, so the identity is the source
  variable's and never the materialized copy's location). **A REPAIR CHANGES ONE THING and says which
  thing it is NOT changing**: this one reopens the standing pin-unheld hole for the restored class,
  exactly as the pre-merge code did and no wider, and closing that hole is its own arc with its own
  population and guard — a scope statement of that shape, made BEFORE the cut, is what keeps both
  changes reviewable.
  ⚠ **Ordering managed pointers by a token derived from `RuntimeHelpers.GetHashCode` is unsound BY
  CONSTRUCTION** (2026-09-03): identity hashes collide (26 bits on CoreCLR), so two distinct
  allocations can share a span and every address-ordering predicate over them (`alias.AnyOverlap`,
  `slices.overlaps`) can answer true for disjoint buffers — a birthday event that shows only on a path
  minting millions of pairs. Two rules beside it: **a seat that moves the corpus onto the path Go
  takes is CORRECT even when that path has a latent defect the old path never reached** — the seat
  stays and the defect roots; and a crash a QUARTER of the way into a stream is a different signature
  from a two-row divergence at the end of a complete one, so a standing "NOT MEASURABLE on this box"
  ruling covers the latter only.
  ⚠ **A shared syscall dispatcher that mis-calls the WRITE primitive MUTES the entire platform's
  runtime error output** (measured 2026-09-03 on darwin): the keystone walked the pointee of an ABI0
  `&fd` — which names the first of three contiguous stack args — and passed fd + junk, breaking
  `read`/`write1`; `runtime`'s `throw`/`print` reach `writeErr` → `write1` on the same shape, so every
  death on that platform is exit-138/SIGBUS with a **blank stderr** and nothing can reach it. For a
  stderr instrument, a blank-stderr death on such a platform is the mis-call SIGNAL — a symptom to
  attribute, never an instrument gap to chase — so the mute baseline is recorded BEFORE the primitive
  is seamed. Its neighbour, and the reason the instrument came first: **a STACK before a design.** A
  full-stderr instrument read two deaths whose predicted sites — both with falsifiers named — were
  FALSIFIED on site in twenty minutes of runner time, retiring two designs; an instrument's COST is
  quoted with its result so the next reader can afford to measure before designing.
  ⚠ **READ THE DISPATCHER'S SOURCE before naming the address a native write went through**
  (2026-09-05): "through the interior address of an unpinned box" was the fitting story, and the code
  said the box was never handed to libc at all — 35 of darwin's 50 `libcCall` sites box the FIRST
  parameter ALONE where Go hands a contiguous `cgo_unsafe_args` block. The REMEDY did not move; the
  MECHANISM sentence did, and a "slot" instrument that would have cost a day was answered by
  construction.
  ⚠ **A stdout LINE COUNT places a mute death without a byte of stderr** (2026-09-04): a guard that
  prints six lines printed exactly TWO on both mac legs, so both died inside the third statement — and
  the leg that did produce a stack named the same frame, two independent derivations agreeing. **A
  capture stage's NULL is evidence only beside a POSITIVE CONTROL in the same capture** (the leg that
  printed through that same stage), and **a ranked hypothesis the run does not support is WITHDRAWN**
  rather than kept alive as "still possible": an alignment story explained one leg's death MODE and
  nothing about WHERE, which the line count settled.
  ⚠ **A CORRUPTION THAT DOES NOT CRASH ON ONE PLATFORM IS THE SAME CORRUPTION** (2026-09-05): x64 and
  arm64 perform the identical 16-byte native write through a STALE REGISTER — the libc dispatcher
  places the first parameter's one field and leaves the trampoline's second and third registers as
  the caller-saved state left them, while Go's `cgo_unsafe_args` block is unpacked by offset — and
  only the leg whose stack walker reads those bytes dies, so **the MUTE leg was the honest one.** Two
  reading rules with it: **a death inside a panic's own REPORT (a stack-trace capture) is scored at
  the door the report was ABOUT**, and **a mute death's exit code says nothing until the crash
  report's frames and registers place it** (pc/far/esr — a data read of a value that is nothing's
  address is a fingerprint to decode, here `sigaction`'s mask/flags word).
  ⚠ **A DUMP, NOT A STACK AND NOT A STORY, NAMES THE WALL** (2026-09-05): a blank-stderr **exit 139**
  from a managed host can be the CLR's own PRESTUB dispatching a generic method on a CORRUPTED
  reference — a raw native address landed in a managed slot by an unsafe store — and the SINGLE-FILE
  host writes no dump and no stderr at all. The framework-dependent host under
  `DOTNET_DbgEnableMiniDump` plus `dotnet-dump … clrstack -all` (and `gdb` for the faulting
  instruction) reads it; nothing cheaper does.
  ⚠ **A SIGNAL DISPOSITION IS A KERNEL FACT, and a bridge that models it in managed code satisfies
  neither of its consumers** (2026-09-05, met on linux and darwin in one week). Go's `signal.Ignore`
  is a kernel `SIG_IGN` — the KERNEL consults it (`tty_check_change` lets a background-pgrp process
  `tcsetpgrp` only if SIGTTOU is ignored or blocked) and it is INHERITED ACROSS EXEC — so a bridge
  modelling Ignore as swallow-in-handler leaves `SIG_DFL`, and a host in a background process group
  under a tty STOPS FOREVER on SIGTTOU: the mute class in a T-state costume, no handler, no deadline,
  no results file. Attributed by four arms varying ONE axis each, the last two differing only in the
  kernel disposition. The TERMINAL-FREE instrument is an exec'd child PRINTING each disposition (Go
  reads 1 on every line): it needs no pty, cannot stop, and separates the two models where a canary
  under `script` can only hang — **read at the code first, then measure, then cut.** Two companions.
  **A CORRECT change exposing a latent gap is a FINDING, never a regression** (the roster is untouched
  because no fleet sweep has a controlling terminal). And **the CLR-free signal class is PER FLAVOUR**:
  darwin's differs from linux's by SIGUSR1, coreclr's activation-injection signal where no `SIGRTMIN`
  exists (`pal` `signal.cpp`), so a kernel `SIG_IGN` there discards GC-suspension activations — read
  the RUNTIME's SOURCE for such a boundary, never memory, and state the class per flavour in the
  bridge's own header.
  ⚠ **A cross-package COPY of a hand-owned type with lazily-shared state is correct BY ACCIDENT after
  the owner's first use and fatal before it** (2026-09-03): the converter's box wrap over a
  package-qualified selector boxes a COPY of an exported package var, and a hand-owned `RWMutex`'s
  state is created on first use and shared — so a copy taken after the owner touches the var lands on
  the real lock while a copy taken before gets its own. That is why no linux/windows row ever saw the
  class and why one darwin arrangement fatals. **A guard for such a fix must hold the sub-library var
  UNTOUCHED before the cross-package pair, or it is green by the mask** — measuring the mechanism on a
  scratch module before cutting the guard is what caught it, the corollary to "a stack before a
  design". The SAME-PACKAGE control rooted the defect: the converter emits the right form (the
  exported box) for a local var and loses it on the qualified selector, so the defect is in the
  selector path, not the receiver machinery.
  ⚠ **Two companion-file rules from the same seam** (2026-09-06). **A companion may deliberately
  COMPENSATE for a caller belief that is factually FALSE, and it must say so IN ITS OWN WORDS or the
  compensation is invisible**: a DNS list-free is a NO-OP because the caller keeps Go's deferred free
  where Go put it while the native chain was already freed and the caller holds a managed
  transcription; and the accept path IGNORES the caller's buffer because under our representation that
  buffer is unusable both as data and as a table key. A caller-side reading cannot see either without
  the companion's own statement. And **a handoff keyed on the CURRENT GOROUTINE turns a cross-package
  sequencing property into a load-bearing invariant that no type, signature or gate expresses**: the
  accept staging is parked in a weak table keyed on the goroutine and consumed by an entry point whose
  signature carries no handle, no overlapped and no identity at all, with the premise — one goroutine,
  no interleaved accept — stated only in a comment. Both violation modes throw by name, so it fails
  loudly rather than silently; but the remedy that REMOVES the invariant keys on something structural
  (the descriptor, or the overlapped that already identifies the operation at submit and harvest),
  while a guard only documents it.
- **Phase 3 complete (2026-07-10 — commit `51ba5d9cf`, tag `stdlib-green-2026-07-10`):** all **302**
  packages of the full conversion (Go 1.23.1) compile clean — zero errors, zero
  exclusions (`runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`, … all included).
  **Compiling is the milestone, NOT operational** — operational validation is Phase 4 (running Go's own
  package tests). Campaign detail: [`docs/Roadmap.md`](docs/Roadmap.md) (Phase 3 iteration log) and the
  [`docs/README.md`](docs/README.md) NEWS section.
  - **Promotion happened, once, wholesale (2026-08-01) — superseding the 2026-07-01 defer.** That ruling
    said not to promote package-by-package on a clean compile, and it was right: the chicken-and-egg it
    guarded against was real while the corpus was unproven. Phase 3 plus 69 operationally-validated
    packages dissolved it, so the whole tree moved at once instead. The hand-owned
    `[module: GoManualConversion]` / `*_impl.cs` files now simply LIVE in the one tree — no canonical copy,
    no overlay-back step, no two-tree exceptions.
  - **⚠ Swapping a hand-own's file contents (backup restore, A/B neutering) can leave a STALE dll
    winning the build**: `Copy-Item` preserves `LastWriteTime`, so the restored source is OLDER than
    the assembly built from the neutered version and incremental MSBuild keeps the wrong dll — a
    defect then "reproduces" against clean, HEAD-matching source with a clean `git status` (measured
    2026-08-16, cost one invalid run). After any hand-own swap: touch the file or build
    `--no-incremental` before believing a repro.
    ⚠ `cp -p` is that trap's bash costume (2026-09-05) — it preserves the timestamp exactly as
    `Copy-Item` does, so the restored source is again OLDER than the assembly built from the neutered
    one.
  - **⚠ A hand-APPLIED edit to a generated file must be proven BYTE-IDENTICAL to the converter's
    own emission before it banks** (standing bar, ruled 2026-08-31). Regenerate into a seeded root
    and byte-compare the hand-applied file against the emission: the first measured
    hand-application was ONE BLANK LINE short of what the converter emits, and without the check
    the next regen reports that cosmetic delta as drift and bills a phantom investigation to
    whoever runs it. The comparison is only meaningful when the emission actually landed in the
    compared root (see the single-package output-positional trap above) and only after the gate's
    negative control has been made to fail once.
    ⚠ **Two control-FORM rules, both measured 2026-09-02.** An ABSOLUTE "byte-identical to the committed
    file" control is unsatisfiable under standing corpus drift — three banked rows' `-tests` emissions
    changed WITH a cut and WITHOUT it (closure drift plus relocation debt), so a no-op would have failed
    the gate; the DIFFERENTIAL form (emission with the change vs without) is the one that carries
    information, and the five-minute control (revert, re-emit) runs BEFORE any violated control is
    reported. And **a positive control's premise must hold at the CONVERTER the measurement used**: "the
    landed hunk must reproduce with zero diff" assumed a binary carrying the merge while the
    measurement's binary was built pre-merge — it failed for its premise, not for the instrument. Name
    the converter a control assumes, and say which form the control took.
  - **⚠ Phase-4 operational: two hand-owned patterns, and a WHOLE-FILE rewrite MUST carry the marker.** Making a
    package *run* (not just compile) often needs a native reimplementation where the literal conversion compiles
    but cannot work — e.g. `sync`'s Mutex/RWMutex/WaitGroup (2026-07-11), whose Go runtime sleeping semaphore
    cannot be emulated, are hand-rewritten on `SemaphoreSlim`/monitors. A `<name>_impl.cs` companion
    *supplements* some declarations (bodyless `partial` + a comment placeholder the converter emits); a
    **whole-file** hand rewrite *replaces* the converted `<name>.cs` and **must carry `[module:
    go.GoManualConversion]`** — else a `-stdlib` reconvert regenerates the Go version over it (`main.go`'s
    `containsManualConversionMarker` drops marked files from the convert set; place it after the `using`s,
    before the file-scoped namespace). Further hand-own detail:
    [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) (the two-tree history
    is archived at `src/archived/Baseline-vs-FullConversion.md`).
    ⚠ **Two displacement mechanisms, priced differently (2026-09-02) — and read the NEIGHBOURING
    rulings before sizing one.** A BODYLESS `public static partial` (a linkname-declared destination)
    is displaced simply by WRITING a body: `PartialStubGenerator`'s predicate is
    `IsPartialDefinition && PartialImplementationPart is null`, so the throwing stub steps aside BY
    CONSTRUCTION — no `manualConversionFuncs` entry, no converter change, no two-seeded diff (though a
    change that removes generated stubs still owes a behavioral COMPILE, route #7's neighbourhood). A
    BODIED converted function is displaced ONLY through that registry — a converter change, with a
    two-seeded emission diff and a hunk-only corpus footprint. The cheap-looking third option (mark the
    whole file `GoManualConversion` and edit in place) freezes every function in the file to optimise a
    few and creates a permanent hand-merge obligation: rejected by the minimal-footprint rule even
    where the file is stable. Accessibility follows the pattern already in the tree — a `Go`-prefixed
    PUBLIC helper per operation, native mirrors PRIVATE to the seam file, so no consumer assembly sees
    a native type — and a hand-own's own "deliberately not covered" scope header is re-read and
    corrected in the SAME commit that changes the scope (one still named two functions that had been
    hand-owned three days and one hour earlier; a scope header that lies reads as the census).
    ⚠ **A DEAD CALLER GETS NOTHING, and the residue is a census assertion whose whole value is the day
    it goes RED** (2026-09-06): two of three callers of a defective wrapper had no call site anywhere,
    test emission included, so displacing them would have cost a registration, a placeholder and a body
    each to change behaviour nothing observes. Record them as MEASURED DEAD, with the assertion that
    fires the moment a forwarding property makes one live.
    ⚠ **And an EMPTY body is not a no-op when the throwing stub was a BRAKE** (measured 2026-09-02):
    empty `runtime_BeforeExec`/`AfterExec` bodies were argued correctly from `execLock`'s readers and
    FORK-BOMBED the `syscall` row (96 children in 7 minutes), because `Exec` hands `execve` MANAGED
    argv/envp, the exec'd image comes up with garbage argv and an empty environ, loses its
    `-test.run`/helper-process markers and re-runs the whole suite — and a CHAIN of child processes is
    itself proof `execve` did not replace the image (it keeps the pid). "Semantically sound" and "safe"
    are two claims: a cut touching `execve` runs under a process ceiling, withdraws first and analyses
    second, and the marshalling fix precedes any body.
    ⚠ **Which FORMATTER runs decides what a converted test PRINTS, and a hand-own's private
    reimplementation of a stdlib contract diverges silently** (measured 2026-09-02): the hand-owned
    test host carries its own verb dispatch (`TestFormat.cs`) with a SMALLER contract than `fmt`'s —
    `#` parsed and dropped, `%T` of nil as `nil` — so every PRODUCTION-dimension control was green by
    construction, because production calls the converted `fmt` and the test dimension never does. Once
    the converted package banks, the hand-own DELEGATES to it rather than carrying a second
    implementation. The measurement that settled it was a probe printing ZERO lines where the failure
    reproduced: a function the path never enters is falsified by its own silence.
    ⚠ **A `-tests` run RE-CONVERTS every non-marked file, so a hand-own PROTOTYPE cannot be measured
    through the pipeline without its registry displacement** (measured 2026-09-03; only a
    marker-carrying `_impl.cs` survives). Packaging is therefore decided by MEASUREMENT, not by
    convenience. Two neighbours from the same arc: **displacing a `[GoRecv]` method whose receiver is
    `ref T`** (not a value) makes the converter emit the BOX-form call `Ꮡa.m(…)` at call sites inside
    `ref` bodies where no box exists (CS0103) — every prior displacement on that seam took a value
    receiver, so the shape was never exercised; the cut whose value had dropped below the fix's cost
    was PARKED and the defect routed as its own cut with the parked code as acceptance. And
    **`[module: GoManualConversion]` needs the `go.` qualifier when it precedes the namespace
    declaration** (CS0246 otherwise).
    ⚠ **A hand-own protected only by a skip-list in ONE driver is unprotected in the OTHER** (measured
    2026-09-03): the hand-owned test host `src/core/testing/*.cs` carried **zero** `[module:
    GoManualConversion]` markers and `-tests` was unguarded on `testing`, so a mistyped `go2cs -tests
    … testing` would regenerate over the Phase-4 host and mint the F15b two-testing-packages collision
    — and it is not hypothetical: the marker saves the FILE (SHA-identical beside a `.cs.auto`) but
    not the ASSEMBLY (56 errors, 25× CS0111, the collision measured). The `testing` row's EXTERNAL
    variant therefore cannot be measured by the canonical `-tests` command at all — the conversion's
    natural output path IS the hand-owned host's directory, so the run rewrites `testing.cs` and dies
    in the publish with only a manifest written: *unmeasured because the instrument clobbers its
    subject*. The `-tests` REFUSAL on `testing` is the real guard, with an explicit census escape that
    demands a scratch output root. Two rules beside it: a host defect that Go's OWN suite finds
    (`Setenv`/`Parallel` ordering raised as a .NET exception, text truncated at the semicolon, no
    reverse guard) is **FIXED, never disclosed** — disclosing launders a bug into a class; and a
    "race" test running with `race.Enabled == false` on BOTH sides collapses to a count-of-zero
    assertion and is not a genuine exclusion.
    ⚠ **The host must print NOTHING a RE-EXECUTED helper's reader can see that Go's binary would not
    print** (measured 2026-09-04): a results-file flush reporting through the PRINTING reporter wrote
    `PASS … exit status 0` onto the stdout of every re-executed helper — the one stream `os/exec`'s
    tests read back — and 22 helper-stdout readers went RED while the target row read clean. Go prints
    nothing on `os.Exit`, the `PASS` line is `M.Run`'s own, and the fail action a non-zero status
    implies is the PARENT's (`go test`'s) to append. **A host-side change to what the process emits on
    exit is measured on the RE-EXEC rows (`os/exec`, `syscall`, `flag`) before it banks**, not only on
    the row that motivated it — and a CONTROL row that fails after a fix is the control doing its job:
    the reading is the mechanism it quotes.
    ⚠ **A refusal at a class-B/C site is a PANIC, not a plain exception** (2026-09-03): the host
    classifies a non-panic exception as an infrastructure error, which is unbankable AND a lie (the
    host is fine). Two companions from that increment: synthetic PCs are minted from the canonical
    HIGH half of the address space so a dereference FAULTS rather than reading a stranger's memory,
    and each function owns a 4 KiB span because the corpus does arithmetic on PCs (`+ sys.PCQuantum`,
    `+ 1`) — a one-value map resolves neither expression. **Reading the tree first turned a symbolizer
    increment into a RECONCILIATION**: `runtime/managed_impl.cs` already carried the whole traceback
    surface, and a second symbolizer was one afternoon away. Three independently minted token spaces
    (caller frames, a 32-bit managed-pointer hash, synthetic PCs in the high half) are disjoint on
    64-bit and COLLIDE on 32-bit — a latent defect in a just-landed increment, found by its own
    follow-up census and remedied by throwing at mint time on 32-bit so it cannot rot. **A guard for a
    RESOLUTION change asserts RESOLUTION** (token → its own function; neighbour → not; a caller-space
    token still routed to the caller table), with the name as confirmation only. ⚠ And **a ruling
    that names a DATA SOURCE states HOW that source is reached from the input at hand, or it is a
    guess**: "the file comes from the map record" was falsified by one measurement — every route into
    the `GoPositionMap` records starts from a live frame's PDB file name and the records are keyed by
    C# FILE, so a synthetic PC, which has no frame, cannot read a file with what the tree has.
    ⚠ **A registration SPLIT from its corpus footprint is refused by the converter suite itself**
    (`TestManualConversionRegistrationsDisplaceSomething`, whose witness is the on-disk placeholder),
    so "registration and footprint are ONE commit" is enforced by a guard rather than by memory — the
    `syscall.Uname` silent subtraction caught at the converter suite instead of days later at a red
    corpus. **A branch that looks seatable and is RED at the converter suite is posted as HOLD by its
    author before anyone assembles it.**
    ⚠ **THE HAND-OWN SET IS DECIDED BY THE PROTOCOL SPAN, NOT BY THE BOX CENSUS** (2026-09-04). A
    census answers where an ALLOCATION happens; it never answers who else must AGREE about the word —
    so when a hand-own changes a synchronisation MECHANISM the unit is every function that touches it:
    `rwlock`/`rwunlock` hand-owned onto an inline gate while `increfAndClose` still released through
    the side table lost every close-time wakeup, caught by Go's OWN `internal/poll/fd_mutex_test.go`
    (`TestMutexCloseUnblock`, Go=pass C#=fail at its own 10 s deadline) and fixed by the THIRD
    displacement. ⚠ **A hand-own inside a BANKED package has its guard already written**: the row's
    own validated suite is the standing gate, so run that row BEFORE believing the cut, and **read the
    row's DESCRIPTION first** — the named test's blocked-reader wakeup was readable before anything
    ran, and a synchronisation word's protocol span is readable off the banked row's test SET, not
    only off the source. **A guard Go SHIPS for the seam** — oracle-compared, authored upstream —
    beats a hand-written probe on every axis, so look for one before writing one: red-before /
    green-after on a guard NEITHER lane wrote is the strongest form of the red-first bar. And a
    CONDITIONED prediction is resolved by READING the row before the run wherever the condition is
    checkable there, so the branch cannot be chosen to fit the measurement afterwards.
    ⚠ **Two registry rules from the same arc** (2026-09-04). **A ROUTE re-scores every box the body
    FORMS, not only the ones the design targeted** — a `ref` receiver cannot form ANY field-address
    box, so a ref-receiver hand-own collects the state word's atomics boxes along with the semaphore
    boxes it was cut for (a prediction corrected a THIRD time BEFORE the run, by the lane's own
    falsifier firing on its own change; retracted-and-restated is the only honest form) — and
    `[GoRecv]` on a ref receiver GENERATING the `ж` overload is what lets such a hand-own be ADDITIVE
    with zero call-site edits, the other face of the box-form call trap above. **And a registry can
    serve TWO ROLES of which only one is gated**: REGISTERING an unexported ref-primary is correct and
    useful (same-package callers consult it), while PUBLISHING it is a promise to nobody, since no
    foreign assembly can name the type — the hand-own registration path had never applied the exported
    bar the converter's own selections always had, which did not matter until an increment registered
    something unexported. **Gate the ROLE, not the entry.**
    ⚠ **And a THIRD, on the CALL SITES a registration re-emits** (2026-09-05). A displaced
    BOX-receiver method is re-declared as a `[GoRecv]` REF receiver, so the generators mint both the
    promoted forwarder and the `ж<T>` twin and every call form binds — but the converter's manual
    box-receiver arm spells `Ꮡ<X>.<method>()` for ANY registered method with a box in scope, a
    PROMOTED selection through an embedded field included, which no receiver form can bind and whose
    forwarder the name-collision skip refuses. **The arm is gated to DIRECT selections**
    (`len(sel.Index()) == 1`), a promoted selection falls through to the hop machinery, and the guard
    asserts the displaced and undisplaced emissions of a promoted call AGREE. Two rules with it: **a
    stated remedy is checked against the LANGUAGE at the emission before the cut** — a `ref` returned
    from a property cannot observe the last of three header stores, so a header WRITE is a
    displacement, never a materializing box — and **a registration that changes how a registered
    method's CALL SITES emit owes a `-tests` emission census of every BANKED row whose test reaches
    the method through an embed**: one such registration had master's converter emitting the box form
    for `internal/poll`'s banked test (compiling only because the generators minted a forwarder
    there), so the committed test source and master's emission disagreed for a day with every standing
    gate green, and the production two-seeded diff is structurally blind to it. The prediction scored
    against that census missed in BOTH directions (a premise error on the base arm; a real move):
    **the census, not the prediction, is the instrument.**
  - **⚠ The S1/CS0030 "architectural wall" was a FORK, not a wall (2026-07-01) — and the fork held to 302/302.**
    **Native-type** pointer/unsafe ops (identical memory semantics in both GC languages) get a faithful
    conversion in the converter/`golib`. **Managed-referent** cases (`guintptr`/`muintptr`/… hiding a managed
    pointer in a `uintptr`) hold the `ж<T>`/`object` **directly** (like `core/sync/atomic` `atomic.Pointer<T>`),
    never a `nuint` round-trip. Genuine **raw-metal on non-native types** (memory-layout math, type-descriptor
    walking, `*.asm`) is stubbed with `[module: GoManualConversion]` (a stub that compiles is an acceptable
    milestone solution).
  - **Next — Phase 4 (operational):** convert and run Go's own `_test.go` suites against the compiling
    packages; design in [`docs/TestingInfrastructureRequirements.md`](docs/TestingInfrastructureRequirements.md)
    and Phase 4 of [`docs/Roadmap.md`](docs/Roadmap.md). The `-tests` pipeline is live (`go2cs -tests
    -test-action all <goroot-pkg> <src/core-pkg>`): converts `_test.go` variants, builds a
    hand-owned `go.testing` host (`src/core/testing`), runs it isolated, and diffs terminal results
    against `go test -json`. `-tests` **always forces `-comments`** (test conversions are derivative
    works — the per-file Go copyright header must survive) and **self-locates `$(go2csPath)`** by walking
    the output dir up to the tree root (so the two-arg command works from a bare clone, no env). First
    validated package: `unicode/utf8` (2026-07-17, tag `utf8-tests-green-2026-07-17`).
  - **⚠ Validated-package commit policy (2026-07-17 user ruling):** when a package's Go test suite
    **validates** through the pipeline, COMMIT its converted C# test sources into
    `src/core/<pkg>` beside the production code — `*_test.cs`, `package_test_info.cs`,
    `go2cs_test_host.cs`, `<pkg>.tests.csproj` — so the passing suite is **visible and reviewable on
    GitHub**, and reproducible via the [README "Try it yourself"](docs/README.md#try-it-yourself--validate-a-converted-test-suite)
    instructions. The pipeline's regenerated inputs/outputs are **git-ignored** by
    `src/core/.gitignore` (the staged `*.go` source copies + `go2cs_test_manifest.json`
    [machine-specific exe-hash digest] + `go2cs_test_comparison.json` +
    `go2cs_test_results.json`/`.xml`). The production
    `<pkg>.csproj` also updates on this run (the IP-4 test-artifact `<Compile Remove>` exclusion) — that
    change is intended, not drift. Refresh the committed test sources at each milestone rebank alongside
    the production tree.
    ⚠ **The committed test sources, proof pages and README badges ARE the WINDOWS record; a Linux-axis
    bank lives in the ROSTER ANNOTATION only** (measured 2026-09-03: 22 `*_windows_test.cs`, zero
    `*_linux_test.cs` corpus-wide). A Linux sweep rewrites the README badge and that rewrite is
    RESTORED, not banked.
    ⚠ **What a roster ROW must SAY — four rules measured 2026-09-06.** **A banked row's DENOMINATOR is
    a coordinator question, answered by MEASUREMENT**: a hand-owned package banked 37 tests against an
    upstream suite whose files it carries six of eleven, and the discriminator turned out MECHANICAL —
    every carried file is the EXTERNAL test package and three of the five absent are the INTERNAL one,
    a principled boundary rather than an accident of effort (four of the five cannot convert or
    contribute no verdicts; the fifth is a real bounded gap). Write the denominator and the per-file
    dispositions INTO the roster beside the row: **a banked row that states what it COVERS is worth
    more than one that merely reads finished.** **A row that states a RATIO states its UNIVERSE, and a
    row carrying TWO denominators names what each ranges over IN THE SENTENCE** — verdicts within the
    files we carry is not files within the upstream suite, neither is derivable from the other, and the
    absent files' verdicts belong to NEITHER count because the oracle emits only over files present; a
    reader who conflates them gets a number wrong in the direction that FLATTERS us, which is the one
    direction an honesty claim cannot afford (position and context had been doing that work, and stop
    being safe the moment a row carries two). **Deepening a banked row is not banking a new one** — the
    objective is a ROW metric, so a bounded increment adding up to seven tests to an already-banked row
    queues BEHIND the rows still unbanked however cheap it is, and that is said explicitly when it is
    queued or it competes for attention on its size rather than on its place. And **a green STATES ITS
    LIMIT in the same post as the green**: the roster guard's 611 passing checks cover a row's
    structure and its arithmetic and CANNOT check that its prose is true — that rests on the per-file
    reads behind it, each naming what it read — because a green allowed to imply more than it measured
    is how a roster stops being trustworthy.
  - **⚠ Disclosure-manifest doctrine, seven rules measured 2026-09-03/04.** (1) **A per-package
    disclosure manifest is ONE file shared by every platform, so any REMOVAL is a cross-platform
    edit** whatever evidence motivated it, while additions are safe: the first Linux annotation
    refresh to remove an entry (a row passing on Linux under Release+TC0) turned the WINDOWS row red
    on the next union battery with exactly that one unabsorbed mismatch. Read the OTHER platform's
    preserved record for the row before removing an entry, and RESTORE an entry that still fires
    elsewhere — an entry that is present but does not fire is not counted and changes nothing on the
    platform that retired it. The durable form is platform-scoped entries (schema plus reader) so a
    per-platform retirement is expressible without touching the other platform's absorption. (2) The
    fix has its own trap: **an entry is a multi-line JSON object, not the two lines a string-occurrence
    grep counts, and a manifest that does not PARSE reads as NO disclosures** — worse than the failure
    being fixed, since it strips every other entry's absorption too. A manifest restore is whole-file
    (valid only when the seat's sole delta to the file is the removal, asserted by counting the commits
    that touch the path), numstat-checked, and PARSED before it is committed; an existence guard ("a
    disclosed count has a committed manifest") cannot see entry- or platform-level correctness and
    must not be read as if it did. (3) **A disclosure CLASS the comparison cannot ADMIT is a guard
    that cannot go green, one layer down**: `matchTerminalStatuses` unlocked a Go=pass / C#=skip pair
    only for `platform-skip`, so three committed `cgo-configuration` entries written for exactly that
    shape could never fire — invisible while the bank host ran cgo ON, where the oracle skipped those
    tests too and they matched skip/skip. Ruling cgo OFF made the class LIVE for the first time, which
    is how a dormant class surfaces: **a configuration ruling exercises manifest entries the bank never
    did.** The durable fix is to make the pipeline ADMIT the class (the entry's name carries WHY — the
    axis — and re-labelling to an admitted class would throw that away), with a positive control that
    a MISSPELLED class stays unabsorbed. (4) **Two seats that are only honest TOGETHER are one train**:
    a re-annotation whose entry carries a class the pipeline cannot yet admit lands its row failing by
    construction, so the assembly gates the first on the presence of the second rather than flipping
    the class in the interim (a flip-and-flip-back leaves a landed master with a mislabelled entry).
    (5) **A row that cannot say WHY it skipped is not disclosed, it is unmeasured** — a Go=pass /
    C#=skip on a row whose host wrote NO results file has an unreadable converted-side reason (the
    comparison record carries verdicts only), so the entry's class waits on a direct-host read.
    (6) **A SAME-FAMILY disclosure is written only from the row's OWN results-file signature, never
    from the family name** — "its 37 siblings are alloc-profile" would have hidden a corpus-visible
    NAME defect (`reflect`'s `Type().String()` dropping an array's LENGTH at an element position, a
    production wrong answer). **Fix, then disclose**; and a `%T`/`TypeOf().String()` name change is
    reflect-bridge-touching AND owes the behavioral OUTPUT phase.
    (7) **A capability-registry KEY is PINNED PER ENTRY with the package clause its GOROOT file
    declares** — internal tests bare, external tests suffixed (2026-09-04). A guard premised on "every
    gated test lives in an external package" REJECTED a correct internal key, and accepting BOTH
    spellings would have discarded its silent-mis-key protection: three negative controls, each
    mis-spelling rejected, are what make the widened guard a guard. Its admission bar: **a
    test-liveness finding in the codegen-liveness shape** (a finalizer a test blocks on forever)
    **takes that class's one-axis Debug A/B as its ADMISSION before an entry is minted**, since this
    family's most convincing story was measured FALSE once.
    ⚠ **A manifest is a STRUCTURED file: it is PARSED, never matched with a spacing-sensitive TEXT
    pattern** (2026-09-05, sharpening rule (2) above) — one manifest spelling a key with TWO SPACES
    silently dropped 54 of 176 disclosure entries from a census, and the only thing that caught it was
    a second derivation being on the table to disagree with. **A count that disagrees with another
    derivation is an INSTRUMENT BUG until proven otherwise.**
    ⚠ **THE ALLOCATION-DISCLOSURE LABEL IS DECIDED BY THE METER, NOT BY THE BOUND** (owner-delegated
    ruling, 2026-09-05). An **incomparable unit** — a byte-derived shim where Go counts OBJECTS — is
    alloc-count-semantics with nothing to retire; **the same meter with a named mechanism** is DEFERRED
    plus a plan; **the same meter with a stated proof** is STRUCTURAL; **none of the three** is a
    reading OWED and no label at all. A want of ZERO and a want of ONE are the same question. Four
    mechanics the ruling carries. The meter CLAIM inside an entry's text is VERIFIED before any entry
    rests on it — ⚠ and per-ENTRY, because **a measurement unit can be a per-entry RUNTIME property
    rather than a per-instrument static one**: the shim reports Go's own meter (a COUNT) when its
    counter saw the allocations and a byte-derived figure when it saw NONE, since reporting the zero
    would be a FALSE PASS, and it NOTES which, with both numbers, on every nonzero result — so "verify
    the unit once per path and let entries inherit it" is unsatisfiable by construction. **A FLOOR is a
    labelling hazard**: a nonzero-byte result reports AT LEAST 1 deliberately, so an assert wanting 1
    that reads 1 may be sitting ON the floor rather than agreeing — an entry whose measured value
    equals BOTH its want and the floor takes NO label until the raw numbers behind the note are read.
    **AN ABSENCE IS A MEASUREMENT TOO**: "no record exists for this family" was asserted inside an
    otherwise three-ways-derived census without opening the file the entries themselves cite BY NAME
    and section number (it existed, at 979 lines, with a staged plan), and for a DEFERRED disclosure
    the question is never "does a record exist" but **"does a stage REMOVE the allocation the assert
    counts"** — reducing the cost is an addendum's justification, not a retirement. And **"AMORTIZES"
    and "REMOVES" are the same thing to an allocation counter under ONE condition, which is a property
    of the TEST**: a per-referent cache drives the steady-state average to zero exactly when the
    asserted closure REUSES its referents across iterations, and cannot help when each iteration boxes
    fresh values — so a structural-vs-deferred call on an amortizing plan is MEASURABLE (read the
    assert's closure) and a coordinator hands back the AXIS rather than a verdict. ⚠ But **the
    amortizing plan's own ELIGIBILITY RULES decide before the test's shape does**: one family's
    closures reused their referents perfectly (which by itself said "deferred") while the record's
    MANDATORY rule — a shell over a VALUE type may not be cached, since its constructor copies the
    struct out of the box and a cached instance would freeze a snapshot — makes the cache refuse
    exactly the referents being reused, so the family SPLIT (value-type rows STRUCTURAL on a three-part
    proof inside the record; no-boxed-value rows DEFERRED against the one proposal that REMOVES an
    allocation rather than amortizing it).
    ⚠ **Five more, 2026-09-06, all about the ENTRY rather than the file.** (8) **A disclosure that
    cannot MATCH is not inert — it is a NEW RED**: a parent test whose fail event carries a null output
    emits nothing of its own, absorption is a substring match against the entry's signature, and an
    empty signature is refused at load for every class but one, so no signature can ever match it —
    measured in BOTH directions (WITH the entry, seven undisclosed rows and the parent reported as not
    matching its own disclosed signature; WITHOUT it, six and the parent ABSENT). **Never add an entry
    whose signature cannot be satisfied**, and measure both states before assuming an unmatched entry
    is harmless. (9) **The DISCLOSED-PARENT AGGREGATION is the mechanism behind that**: a parent rides
    its children when ALL of its subtests are disclosed and does NOT when even one leaf remains
    undisclosed, so **the remedy for a failing parent is to enter the missing LEAF, never a parent
    bookkeeping entry** — named in the converter's own source, where another package pins twenty-five
    leaves and lets two parents ride it (the finding shrank its own queued re-bank list from two
    entries to one). (10) **A stale SIGNATURE fails safe; a stale REASON does not.** A disclosure whose
    pin no longer matches stops absorbing and the row goes honestly RED, which is how two stale entries
    were found at all; a disclosure whose REASON has drifted off its own failure, with a signature that
    still FITS, keeps absorbing while explaining the wrong thing — a row reading green for a cause that
    is no longer its cause. **Re-derive the REASON against a fresh measurement before re-pinning the
    signature, even when the reason looks obviously still true** — the entry a lane called the SAFE
    half was carrying exactly the hazard it had described one post earlier, already authorised on that
    reading. (11) **The strongest disclosure is one whose own control proves the underlying defect
    ABSENT**: a `reflect` row exists to catch a zero-sized return aliasing the next result's storage,
    and the cleared-slot arm COLLECTED — which could not have happened had the alias existed, since the
    value under test still holds the first result across the collection — so the entry documents a
    frame-slot pin AND measures the defect the test was written to detect as absent. That is what makes
    it a disclosure rather than a hiding place. (12) **Check the SCHEMA before writing into it, and do
    not invent a field to carry a ruling's wording**: an execution annotation is a ROSTER ROW field
    parsed by the sweep's module, not a manifest field, so it lands at BANK time — and the class in
    question needed no allow-list entry at all, i.e. no converter change. Written the same evening an
    inexpressible entry was measured to CREATE a red.
  - **⚠ Ruling #1 and its boundary (owner, re-read 2026-09-03/04).** A Go=pass / C#=skip whose skip
    reason is OUR OWN missing feature is a **FEATURE GAP**, never a disclosure; and an annotation
    banked where the ORACLE skipped for a HOST reason is host-conditional and says so, since on a
    capable host the gap shows and the row reads red. ⚠ **A Go=pass / C#=HANG is the same ruling**
    (2026-09-05, `TestMutexWaitTimeMetric`) — it stays on the host-deadline gate until the feature
    lands — and the classification is earned by **reading the LIVE stacks (`dotnet-stack` on the
    running host) before classing a hang**: what read as the lock protocol's contended path was two
    threads, one spinning in Go's own status predicate and one parked holding nothing, with no runtime
    lock anywhere on the path — so the root is a PREDICATE (the managed g never leaves `_Grunning`
    because the converted `sync` parks without `gopark`'s accounting), not a lock. A hook in the seam
    every converted wait passes through is a golib API change whatever its line count.
    ⚠ **A coordinator ruling that cites a PRECEDENT
    re-reads the ruling that governs it before it is posted**: the `alloc-profile` want-zero
    disclosures in `bytes`/`bufio` PREDATE ruling #1 (a want-zero alloc assert is satisfiable in
    principle and is never a disclosure) and stand as LEGACY to be re-examined, not as precedent to
    extend — a "measured floor is disclosable" sentence built on them was retracted the same hour. **An
    owner ruling outranks a coordinator's inference from artifacts.** ⚠ And **a count gap is read from
    the row's OWN Go gate before it is attributed to the axis under discussion** (a `cgroup2`
    permission gate is not the cgo axis).
    ⚠ **A COORDINATOR RULING CANNOT MINT AN EXCLUSION CLASS** (2026-09-04). The roster's classes are
    the OWNER's — E1 no eligible tests, E2 broken oracle, E3 the subject IS the replaced
    representation — and the bar refuses "merely hard, unimplemented, or expensive", so "untestable by
    capability" was a phrase doing work the ledger does not license, held by the lane against the
    parser and the format guard before it was ever written. **A row whose tests an unbuilt
    implementation WOULD satisfy stays IN the denominator as unimplemented** — its recon is the
    disposition and the implementation is the queued hard thing; widening E3 for it would be the
    precedent every later frontier row cites.
    ⚠ **The bar read from BOTH sides, one evening's five rules (2026-09-06).** **"The blocker now has a
    NAME" is not "the row is impossible"**: a hand-owned function that refuses BY NAME because a
    capability was never built is the clearest possible evidence of UNIMPLEMENTED, which the bar names
    as a reason to REFUSE exclusion — a coordinator read a retired blocker as a class change and was
    refused by the schema's owner, correctly, and withdrew without escalation. **Representational
    impossibility QUALIFIES where unbuilt does not**, demonstrated the same evening from the opposite
    direction: a set of disclosures qualifies because the pointer model is a deliberate identity scheme,
    so the numeric comparison the assert performs has nothing to compare — the pair states the class
    boundary better than either case alone. **A precedent transfers only if its PREMISE does**: the race
    row is excluded because Go declares NO ELIGIBLE TESTS outside an instrumented build, so the
    comparison is vacuous by Go's own definition, and a row with two eligible tests that RUN and produce
    verdicts is the NEGATION of that premise, not an instance of it — reaching for "the same kind of
    reason" on a resemblance rather than reading the class is the same move as reading a registration as
    remediation. **A MECHANISM does not transfer by resemblance either**, and that cut both ways in one
    evening: it refused an exclusion the coordinator wanted AND a disclosure a lane would have benefited
    from (the second half of a billed increment stands as WORK until it has its own measurement). And
    **before ruling on a row's class, read the row's own RECON and any design record that exists BECAUSE
    of a prior ruling**: the same question had been ruled the other way one day earlier — "unimplemented,
    not untestable; expensive rather than impossible" — and the design record for the missing capability
    exists precisely BECAUSE the row stayed in the denominator. **A prior ruling stands until a NEW
    measurement reopens it, and a measurement CONFIRMING the prior ruling's premise is not a reopening.**
    ⚠ **Which side of that bar an entry sits on is a MEASURED discriminator, never a wording choice**
    (2026-09-06). **"No managed body exists" reads as unimplemented and may be representational** — the
    discriminator is whether the CONCEPT exists for anything: can the host produce a comparable code
    pointer for ANY function? If it can, the block is one missing body and the entry is WORK that no
    rewording converts into a disclosure; if it cannot for any function, the concept is absent from the
    model and the argument stands on the same footing as the pointer-identity set. Measure that BEFORE
    writing the new reason. **One probe answered three questions because its properties were reported
    SEPARATELY rather than averaged**: plain functions non-zero, stable, distinct and resolvable (so the
    concept EXISTS and a missing body is unimplemented work) while method values fail STABILITY alone (a
    fresh identity per read) — which disposed of two entries in OPPOSITE directions and named the
    smaller, more valuable fix. **Split a probe's population when the answer might differ across it.**
    And the floor under all of it: **a test that only asks for SELF-CONSISTENCY can never be excused
    representationally** — if the assertion compares two of OUR OWN values and requires them equal (as a
    method-value pointer test does, since Go's two method values share one trampoline whatever the
    receiver), then nothing about the foreign system is being asked of us and a host with the missing
    property passes it UNCHANGED. **Ask what the assertion REFERENCES before writing or repairing any
    disclosure**: the foreign system's values (a representational argument is available and must be
    argued) or only ours (no such argument exists, and the entry is a DEFECT wearing a disclosure's
    clothes).
  - **⚠ A BANKED ROW CAN BE A VACUOUS PASS, and a census over the roster is what says how many**
    (2026-09-03). `internal/abi`'s `TestFuncPC` compares `FuncPCABI0(fn)` against a value `_test.s`
    writes in Go — assembly never converts, so the C# side reads `0 == 0` while Go compares two real
    addresses, and the verdicts MATCHED. **A pass whose two arms are equal for OPPOSITE reasons is a
    false green**; under every honest answer the row stops passing, so it was RULED to spend the
    verdict (2 → 1 + 1 disclosed, runtime-capability, permanent by construction). The census over 202
    banked rows found that class has exactly ONE member, and a SECOND vacuous-as-recorded page beside
    it: `internal/cpu`'s four `if HasX && !HasY` implications could not fail while `doinit` never ran
    (all flags false) — remediated by the `[ModuleInitializer]` hand-own, but the BANKED page stayed
    vacuous until reswept. **A hand-own that changes a package's INIT STATE re-sweeps every banked row
    whose asserts read that state.** The class stays small because bodyless partials THROW and the
    corpus converts under purego (17 silent hand-own bodies exist, one read by a banked assert), and
    nine banked passes are tautological by Go's OWN construction (both arms call the `*Generic` twin) —
    honest, since that IS the production path. ⚠ Two neighbours: **vacuous-if-stubbed tests are NAMED
    before any stub lands** (no content assertion around `StartCPUProfile`); and **a banked row that
    dies on a corpus defect is UNREADABLE, so it cannot gate anything** — an increment whose canary it
    was declares final on the remaining canaries and STATES the row's unreadability, rather than
    waiting for a row nobody can run.
    ⚠ **A pass that NO HOST DEFECT COULD EVER MOVE is not a measurement either** (2026-09-04): a test
    asserting `count(<a Go output literal the host never writes>) == 0` passes vacuously on BOTH
    sides — the anti-laundering clause read from the other direction — so it is EXCLUDED with the
    reason stated, never counted. Three neighbours from one row. A test that fails deterministically
    for a divergence the project CHOSE (the host-identity class) is run and DISCLOSED, not excluded,
    because a chosen divergence belongs where it can be seen. A test whose failure path is a loop that
    never ends (a timeout-dump scrape that doubles and retries forever) is a HAZARD needing a
    capability entry, since admitting it turns the row into a deadline kill with a contiguous
    alphabetical tail. And **a row's SIZE is its VERDICT count (156), not its top-level NAME count
    (59)** — the two are reconciled before a row is sized. ⚠ Beside them: **a design PREMISE about an
    output path is checked against an existing BANKED instance before options are costed** —
    colocation is the pipeline's norm, which one dispatch's premise had backwards, falsified by
    reading a banked row's directory.
  - **⚠ A disclosure pins a failing NAMED row, so a HOST-KILLER cannot be disclosed at all**
    (2026-09-03): a goroutine panic escaping the process, or an access violation, produces no row to
    pin — the arc's gate is making the killers produce rows. Two measured limits on that. "Make the
    host-killer produce a row" was ALREADY implemented and buys nothing for the escaping-panic case:
    the host emits a fail row and a package fail, faithful to Go's own death, and surviving the panic
    would be a false green twice. And **a converted `recover()` does NOT catch a raw
    `NotImplementedException` from an unimplemented-external stub** — only a converted panic — so any
    unimplemented stub reached from an http handler TERMINATES the process where Go answers 500. A
    regex-shaped test assertion is sized against the WHOLE regex (another goroutine's header AND its
    frames), not against the state word.
  - **⚠ After an operational SWEEP, `git status` is dirty and it is (almost always) NOTHING — classify,
    don't chase, and never bank it.** Two *different* phenomena get conflated here; a sweep produces only
    the first. ⚠ **The dirt is NOT confined to `src/core`: the sweep REWRITES each swept package's
    proof page under `docs/validation/current/`** (that rewrite is by design — it is why the
    host-conditional check reads the COMMITTED page from HEAD), so a restore scoped to the corpus
    leaves the pages behind — 56 of them measured on one lane's shift (2026-08-29). Restore both
    roots, or the next diff reads as proof-page drift. ⚠ **A gate's own DIRT PREFLIGHT is what
    catches the half a corpus-scoped restore leaves** (2026-09-06): a solution leg aborted
    `ABORT dirty` after a canary run whose restore had been scoped to `src/core` while the pipeline
    had also rewritten `docs/validation/current/<row>.md` — the class this paragraph documents,
    met by the gate rather than by the reader. The preflight earned its keep by REFUSING to measure
    a tree it could not vouch for, **which is the shape every gate should have: refuse over an
    unclean tree rather than measure it and report a number.**
    1. **CRLF phantoms — most of a healthy sweep's dirt.** The converter preserves the Go source's
       **LF** inside multi-line string literals while emitting CRLF everywhere else, and `core.autocrlf`
       smudges those in-string LFs to CRLF on checkout. A `-tests` run re-emits them as LF, so every
       banked file containing a multi-line literal shows **modified with no diff hunks at all** — it
       does not even appear in `--numstat`. Do **not** memorize a file list; the count tracks how many
       banked packages hold multi-line literals and grows with every bank (15 at the 47-package roster,
       16 once strconv's `testdata/testfp.txt` joined, **5 + 10 at the 73-package roster** — see the
       split below). *Positive control:* `git diff --numstat HEAD~1` must be non-empty, or your check
       is broken, not clean.
       ⚠ **The "numstat must be empty" rule is FALSE for `-text` paths (found r40, 2026-08-04).**
       `src/core/compress/testdata/*` is marked `-text`, so git does **not** normalize it and a pure
       CRLF flip shows a **real, non-empty numstat** (`gettysburg.txt` 29/29) that reads exactly like
       content drift. Verbatim `testdata`/`*.s` copies are therefore a SECOND phantom shape: test
       CR-stripped equality against `HEAD` directly rather than trusting `--numstat`.
    2. **`-tests`-CLOSURE production files.** A handful of production `.cs` differ between the two
       emissions (`Δio` alias, `global::go.*` root escape, the using-block REORDER the alias
       causes, and — the FOURTH shape, named 2026-08-17 — the `initᴛᴛtests()` hook a `-tests` run
       adds to a package's `package_init.cs` as +7 REAL lines `-stdlib` omits, which survives a
       numstat check that filters phantoms; same class, restore it — **AMENDED 2026-08-26,
       ratified at the leveling-rebank floor: for a row whose test sources are REBANKED at or
       after the init-order arc, the `initᴛᴛtests()` hook is BANKED, not restored** — a
       re-derived suite does not compile without it, so those packages' `package_init.cs` rests
       on the `-tests` side and it is the `-stdlib` overlay ritual that must classify-and-KEEP
       it; the restore rule stands only for rows still carrying pre-arc sources) because the
       `-tests` closure imports more. **Staging corollary (paid for 2026-08-26): never
       `git add -A`/`git add .` on a tree that has had a sweep or `-tests` run against it — name
       the paths; the hook shape survives numstat filters and lands in the commit silently.**
       Two additions, 2026-08-29/30: the **FIFTH shape** — a `GoPositionMap` funcLit/range
       argument the `-tests` emission adds and `-stdlib` omits (rooted independently by two
       lanes the same day; survives numstat filters; evidenced by banked `cookiejar` carrying
       it) — same class, classify-and-restore per the side the tree rests on. And ONE-WAY
       emission changes are NOT closure shapes: the `-tests` init-forcing hook (+7 lines in
       `package_test_info.cs`, landed 2026-08-30) appears at a row's next test-source
       REGENERATION and stays — 193 reference-model banked test infos are stale-until-rebank by
       design, no standing restore, and the rebank wave that levels them owes the full-roster
       sweep (the throwing-production-init regression shape can only materialize there).
       Whether a sweep SHOWS them depends on which
       side the committed tree currently rests on, so do not treat either state as the invariant:
       when the tree rests on the `-tests` side they are invisible to a sweep and surface only under
       an `-stdlib` reconvert control; when it rests on the `-stdlib` side — **where r40 left it** —
       every sweep flips them and they must be **RESTORED**. Measured at r40: **13 files**, wider than
       the six recorded in [`docs/phase4/DESIGN-named-interface-wrappers.md`](docs/phase4/DESIGN-named-interface-wrappers.md)
       §7 — also `bufio/{bufio,scan}.cs`, `crypto/md5/{md5,md5block}.cs`, `regexp/{regexp,exec,backtrack}.cs`.
       Both emissions are correct for their own closure — only the pipeline pairs them — so this is a
       STANDING restore, not a one-off cleanup, until the two agree on one alias per import.
    3. **`.cs.auto` review siblings.** Tracked, and refreshed by a `-tests` run but NOT by an
       `-stdlib` overlay (which excludes them to protect the hand-owned `.cs` beside them). Restore
       them in a sweep; re-measure the whole set at each rebank head, one seeded reconvert per
       target, rather than banking a count (CleanupBacklog item 18).
    4. **Deduplicated same-shape anonymous structs — LEVELED, so a reappearance is news.** The
       converter binds a second anonymous `[GoType("dyn")]` struct of identical shape to the FIRST
       declaration's type instead of minting its own (`e61758549`, the reflectlite arc). In a diff
       that reads as the duplicate `[GoType("dyn")]` block vanishing while the slice and element
       types rename onto the original's `ᴛ1`, with a knock-on in `package_test_info.cs`, whose
       witness list sheds the declarations that no longer exist. A suite banked before that commit
       keeps the old shape until its own pipeline rerun; the 2026-08-24 post-merge rebank ran the
       last five (`math/cmplx`, `go/build/constraint`, `regexp`, `strings`, `time`). Unlike classes
       1–3 this one does NOT stand: it is banked, so meeting it again means a NEW unbanked converter
       change — find that commit rather than restoring the file.
    Anything that is none of these — a non-empty `numstat` on a production `.cs` that is not a closure
    re-flip, or any change to a production `.csproj` — is **real drift**: stop and root-cause it before
    landing. (A production-`.csproj` change specifically meant the validation-pack block had been
    stripped; fixed in `ce82093b0` and proved clean across the full r40 sweep.)
    ⚠ **After a `-tests` run a package directory holds THREE populations — tracked corpus files,
    tracked hand-owns, and untracked generated emission — so any glob- or directory-wide operation hits
    the wrong one** (paid twice, 2026-09-02). `rm -f src/core/reflect/*_test.cs` deleted the TRACKED
    `export_impl_test.cs` hand-own (the glob encoded "test files under a converted package are
    generated" — true for 13 of 14), and `git checkout -- src/core/reflect` reverted the lane's own
    guard edit in `value_impl.cs`. Restore by FILENAME, clear emission with `git clean -nd` then `-fd`
    — the primitive that reads the tree's state beats the pattern encoding a belief about it.
    ⚠ **`git checkout -- <path>` restores from the INDEX, so a control-arm restore DESTROYS an
    unstaged change in the same file** while "restore clean: 0" TRUTHFULLY reports a match with HEAD
    (2026-09-04 — the sweep-restore trap above, met from the control-arm direction). **Stage the cut
    FIRST, restore control arms from the index**, and only then does a diff-against-index check mean
    what it says.
- Open converter items: `src/go2cs/ToDo.md` (e.g. `visitMapType` completion, remaining dynamic-struct
  implicit-cast checks, optional recursive dependent-package conversion, comment conversion, cgo/asm targets).

### Deploying the core to the GOPATH root
`src/deploy-core.ps1` (cmd launcher `deploy-core.bat`) stages the runtime + standard library at
`%GOPATH%\src\go2cs` so converted projects — and, later, recursively converted end-user apps that target
that same root — resolve their `$(go2csPath)core\<pkg>` / `gen\go2cs-gen` references relatively. It has ONE
mode since the trees unified (the old `stub`/`stdlib` argument is gone): a straight copy of `src/core`,
because the repository layout and the deployed layout are now the same layout and no reference needs
rewriting. It also deploys the `go2cs-gen` analyzer, writes a root `Directory.Build.props` that pins
`$(go2csPath)` to the deploy root (so no `-p:go2csPath` is needed), generates `go2cs-core.slnx`, and builds
to verify. The other src PowerShell utilities `clean-bin.ps1` (remove bin/obj/Generated) and
`set-version.ps1` each also have a `.bat` launcher.
⚠ Purge with that instrument, or with an explicitly depth-UNLIMITED walk: an ad-hoc
`find … -maxdepth 3` purge missed 274 of 388 output directories and drove a lane's disk into the
harness's own free-space floor (2026-09-02). **Non-interactively — a background task, a harness
tool call, any `-NonInteractive` host — pass `-Force`**, and invoke it as `clean-bin.bat -Force` or
`powershell -NoProfile -ExecutionPolicy Bypass -File .\clean-bin.ps1 -Force`: the script is
unsigned, so on any host whose execution policy requires signing a bare `powershell -NoProfile
-File` dies "is not digitally signed" (observed 2026-09-03; it does NOT reproduce on a box whose
LocalMachine policy is already `Bypass`, which is exactly why the bypass belongs in the invocation
rather than in an assumption about the host — the `.bat` already carries it and forwards arguments). `-Confirm:$false` is equivalent in-process but
does NOT bind through `powershell -File` on 5.1, which literalizes the argument and rejects it
before the script runs — `-Force` is the contract. **Its exit code is load-bearing before any build
that follows a purge** (0 = everything found is gone; 1 = declined; 2 = the host could not prompt
and `-Force` was absent; 3 = `-WhatIf`; 4 = something survived): a found-but-not-deleted run never
exits 0, and a wrapper that captured a non-zero clean and carried on ran a target-switch build
without the purge it reported attempting (2026-09-03) — the same day the other half of that hole
was met, where `Read-Host` on EOF printed "Found 2866 folders to delete. Operation canceled." and
exited **0** having deleted nothing. Read the code through `-File`/the `.bat`, which propagate it
exactly; `-Command "& …"` collapses every non-zero code to 1. And the purge is only the BELT: after
a `$(GoTargetOS)` switch the braces are the per-target compile item-set read
(`dotnet msbuild -getItem:Compile` — e.g. 39 windows / 0 linux under one `GoTargetOS` and 0 / 75
under the other), and only that second reading answers the question the purge exists for.
⚠ **And its `-Root` default resolves to an EMPTY STRING under `powershell -File`** (2026-09-05):
`Resolve-Path` refuses the empty path, the script then enumerates the CWD anyway and reports "Found
2640 folders", and the run exits 2 having deleted NOTHING. **Pass `-Root` explicitly as well as
`-Force` for any non-interactive purge**, and read the EXIT CODE plus a dirs-left count — never the
log's "Found N", which is what the enumeration found and not what it removed.
⚠ **PURGE AFTER EVERY PROBE, and read an I/O error HISTOGRAM before believing an error count**
(2026-09-06, one disk exhaustion in two costumes). A coordinator's own two scratch worktrees held
**5,096** build-output directories and filled the disk to zero: the fleet's mailbox push failed on
`Out of diskspace`, and a solution leg that had built clean an hour earlier came back exit 127 with
**78 errors** whose histogram was MSB3491/MSB3027/MSB3021/CS0016/CS0041/CS8104 and every message
read *not enough space on the disk*. **N errors whose codes are all I/O are ONE environmental
failure wearing N codes, not N defects** — purging recovered 43 GB. Two readings ride on it: a
measurement taken on a tree that CHANGED under it — here a `bin`/`obj` purge while MSBuild was
writing there — is not a measurement at all, whichever way it reads; and what made the exhaustion
VISIBLE rather than silent was the posting tool REFUSING to claim delivery and saying the commit
did not land, so **a state-advancing tool that reports its own failure honestly is worth more than
one that succeeds quietly.**
**⚠ Its default target is MACHINE-GLOBAL** — `%GOPATH%\src\go2cs`, shared with every sibling worktree —
so never run it bare as a gate. It supports **`-WhatIf`** (a real dry run: the three non-cmdlet writes
are explicitly `ShouldProcess`-gated, and the solution enumeration reads the SOURCE so the projected
project count is truthful) and **`-Target <dir>`** for a scratch deploy. The copy is pure PowerShell
(`Copy-SourceTree`) rather than robocopy since 2026-08-08, and is byte-identical to what robocopy
produced (3,979 files A/B-verified); that also removed the repository's last external Windows tool
dependency. Harness path/platform primitives (`$IsWindowsHost`, `$ExeSuffix`, `$SepPattern`, the roots,
`Get-PathDepth`) live in **`src/_paths.ps1`**, dot-sourced by every instrument — never re-derive them,
especially `$IsWindowsHost` (`$IsWindows` does not exist on PowerShell 5.1, so a bare `-not $IsWindows`
reads backwards on the one platform 5.1 runs on).

### Known staleness (do not trust blindly)
- `docs/README.md` is a **maintained visitor surface** — its NEWS block is current and its references are
  corrected. What is historical is its milestone table's ANTLR-era rows: read those as history, not as
  instructions. (The older "carries a banner" note here was itself stale; there is no such banner.)
- The retired `net6.0` C# converter scripts (`src/convert-gosrc.cmd` / `convert-gosrc.bat`) were **removed**
  2026-07-11; the current converter is the Go build with the flags listed above.

## Conventions

- **⚠ SECURITY — no real machine names or other internal-infrastructure identifiers on ANY pushed
  surface (owner order, 2026-09-01).** Every committed file, mailbox entry, commit message and branch
  name refers to fleet machines ONLY by their nicknames — `R-LAPTOP`, `G-LAPTOP`, `i9`,
  `i7`/`coordinator`. Real hostnames, UNC paths carrying them, and any other detail that exposes the
  owner's internal network (share names, non-public usernames) stay off GitHub entirely. The
  2026-09-01 scrub replaced every occurrence at both public tips (master and the mailbox branch); git
  HISTORY retains the originals (owner-accepted) — so never reintroduce one by quoting a pre-scrub
  record verbatim: re-census with a case-insensitive grep before banking any doc that copies old text.
  ⚠ **Two scrub rules paid for on 2026-09-02.** (1) **A SCRATCH-directory transpile's emission is not
  postable**: it records an ABSOLUTE source path in `GoPositionMap` (the committed file carries the
  relative `main.go`) and drops the hand-added `[GoTestMatchingConsoleOutput]`, so such a
  `package_info.cs` is never copied into the corpus and never pasted onto a pushed surface — one was,
  carrying a profile path plus worktree and session layout, and had to be scrubbed off the mailbox.
  Post emissions from a repo-relative run, or redact before posting. (2) **The pre-post grep covers
  the PATTERNS you quote, not only your prose** — a post that described its own census as
  `<name>|<profile-root>|/home/` spelled the real account name onto the pushed surface — and a
  security census of the mailbox reads `origin/claude/mailbox` after a VERIFIED fetch (an
  already-scrubbed line was re-reported from a stale copy). Census case-insensitively over BOTH
  profile-root spellings and `/home/`.
  ⚠ **THE CENSUS THAT CLOSES THE CLASS — the order landed 2026-09-01 and was BREACHED in pushed docs
  by 2026-09-04, with no gate that could see it.** A security census takes **TWO PASSES**: a
  path-anchored pattern cannot see an identifier used OUTSIDE a path BY CONSTRUCTION (a directory
  listing's owner column, an "account X" parenthetical, a machine name in a roster row — the order's
  own headline clause), so a LITERAL pass over the identifiers the first pass surfaced is not
  redundant. Every hit outside the scrub scope is enumerated with a per-line REASON into an allowlist
  the instrument consults; the arithmetic closes in BOTH directions (the substituted class rising by
  exactly the substituted spans, the allowlisted class unmoved); the substitution is the identifier
  ALONE, in the tree's existing placeholder spelling; and the verification compares at the SAME LAYER
  (the working-tree form under the `eol=crlf` pin, never the LF blob) after being shown RED on one
  extra byte and after REFUSING an empty file list — a verification that passed vacuously over an
  empty list had already happened once. The durable form is a **standing census guard in the
  converter's own test suite**, allowlist and positive control inside it.
  ⚠ **Four mechanics that guard carries** (2026-09-04). A guard's OWN source is a tracked file the
  guard scans — and the file most likely to be edited by whoever adds the next entry — so it is NEVER
  exempted: planted fixtures are assembled through `Sprintf` so the source reads as a placeholder
  while the runtime string is real-looking, and the green arm going RED first because the guard found
  ITSELF is the control working. A denylist that would otherwise put the identifiers it forbids on the
  pushed surface is stored as HASHES, with each token's LENGTH in the SAME struct as its hash (a
  length kept in a second list can silently disagree and disarm the entry while every test passes).
  Clearances are keyed by **(path, segment)** with a liveness test, NEVER by line — a line-numbered
  clearance goes stale on the next edit above it, route #8. A structural pass skipped over
  fixture-heavy trees stays honest only if a second, token-keyed pass still runs there. And a guard
  that is RED at master BY CONSTRUCTION names its merge-order constraint (with or after the cure) and
  is its own full-scale positive control.
  ⚠ **What a POST may say, and when the census runs** (2026-09-04). "Spell GOROOT exactly as
  `go env GOROOT` prints it" is an instruction about the ARGUMENT, never about the POST — a dispatch
  that said it invited a lane to quote its GOROOT verbatim onto the mailbox, profile root and account
  name included, scrubbed by a follow-up commit within minutes. **A post quotes the PATTERN it
  checked** (profile root, home prefix, doubled-backslash network prefix) **and never a value that
  matches one**; a toolchain pin is proven on a post by the bare `go version` line ALONE, which
  carries no path; and a docs seat's security grep NAMES its patterns rather than spelling them.
  **The pre-post census runs before EVERY push — diff-scoped and EXIT-GATED, or it is decoration**: a
  census whose exit code does not gate the push is a guard built and not armed.
  ⚠ **ITS COMPLETENESS SIBLING — a post that ships with a TEMPLATE PLACEHOLDER still in it, and the
  guard that catches one (2026-09-05, three lanes in one evening).** A gate-readings placeholder went
  out unfilled and the seat could not be filled from it: **an announce is read for its GATE READINGS,
  not its prose** — a seat is wired at the SHA and FILLED only when the readings are on the record.
  So the APPENDER refuses a body still carrying a placeholder token and ABORTS before the commit,
  because **the security census is a CONTENT check and never a COMPLETENESS check** — two guards, two
  questions. Six mechanics the three independent derivations of that guard cost. (1) The predicate is
  a **whole-word, case-SENSITIVE TOKEN LIST**, not a bare-uppercase pattern, which refuses ordinary
  words a gate line legitimately contains; an unanchored uppercase pattern also over-matched a bare
  three-letter word inside a longer token, found only by running the guard against the author's own
  honest post — **a guard's DESCRIPTION is not the guard, so state the mechanism you IMPLEMENTED and
  run it on real input before quoting it.** (2) The gate's PLACE is part of the property: it runs
  BEFORE the fetch/checkout, with controls asserting the side effect did NOT happen. (3) A guard over
  marker TEXT cannot tell a QUOTATION from an unfilled marker, so **write about a marker form in prose
  and never by spelling it** — and by the same token a legitimate upper-case TYPE spelled inside angle
  brackets (the converter's `ж<…>` forms) trips it, which is cured by naming the type OUTSIDE the
  brackets rather than by relaxing the case sensitivity. (4) **A pre-check and the guard it fronts must
  be ONE predicate**: a case-sensitive `grep` cleared an entry that PowerShell's `Select-String` —
  case-INSENSITIVE by default — then refused, so pass `-CaseSensitive` and carry two controls (an
  uppercase token refused, a lowercase generic admitted). (5) An UNQUOTED heredoc executes every
  BACKTICK PAIR as a command substitution: a SHA in a heading became an empty string ("command not
  found" on stderr) and the post published with the SHA missing while the commit subject carried it —
  interpolate with a QUOTED heredoc plus a `sed`/python substitution, and **read the body BACK between
  fill and append**, since no placeholder guard can see an empty substitution. (6) **A positive
  control of an ADMIT arm on a state-advancing tool PUBLISHES** — one posted a junk entry — so such
  controls run behind a dry-run switch that stops after the guards (controlled: the remote tip
  unchanged before and after), and a published artifact is NAMED as such in a follow-up, never
  rewritten away.
- C# style: see [`docs/coding-style.md`](docs/coding-style.md) (Allman braces, 4 spaces, `m_`/`s_`/`t_`
  field prefixes, explicit types over `var`, language keywords over BCL types, `\uXXXX` for non-ASCII).
- Conversion strategy: [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — a high-level,
  example-driven **summary** of how each Go construct maps to C#; each section links into the exhaustive
  [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) for the full detail.
- Process/gate terminology as used in commit messages and reviews (CNR, A/B footprint, census,
  chip, guard, golden, overlay, banked…): [`docs/Glossary.md`](docs/Glossary.md).
- Generated C# intentionally targets Go-like *behavior first* (no implicit async), and Go-like *appearance*
  second (extra machinery hidden in partial classes / generated files).

### Integrating concurrent lanes (the hazards that do NOT show up as conflicts)

When several machines work the same tree, the dangerous merges are the ones git reports as clean.
Each rule below was paid for.

- **⚠ Two lanes solving the same problem produce a SILENT DUPLICATION, not a conflict.** Independently
  added blocks land at different offsets under different names, so git merges both as ordinary
  additions and marks nothing. Measured 2026-08-24: two independently written 17-element apply-set
  arrays in `migrate-tfm.ps1` auto-merged *cleanly*, and the result would have appended every site to
  `$applySites` **twice**. The mirror case bites from the other side — a symbol introduced OUTSIDE the
  markers (`$shadowed`) is left undefined at its use site if you take one side of a marked hunk. Neither
  is visible from the conflict markers. **Resolving the marked hunks is not resolving the merge: read
  the merged file whole, and run the thing.**
  ⚠ **The shape reaches CUTS, not only merges** (2026-09-02): the same two-line guard fix was written
  twice within one hour by two sessions, caught only because both announced before either merged — the
  author's branch was taken and the duplicate deleted. A coordinator-critical fix to a LIVE lane's own
  file is announced as an ASK to that lane first.
  ⚠ **Two 2026-09-04 refinements, one preventive and one diagnostic.** When two concurrent cuts need
  the SAME shared predicate, **write it at the SAME PATH on purpose** so the merge collides as a loud
  add/add conflict rather than auto-merging two differently-named definitions of one concept — the
  silent-duplication shape forced into the open — and the ruling then bases the LATER cut on the
  earlier one's file before the seat, so the assembly sees one definition and no conflict at all. And
  **a clean auto-merge of two cuts touching ONE method is read whole for two REPRESENTATIONS of one
  fact, not only for two definitions of one NAME**: a status value from one cut and a marker list from
  the other arrived in the same method from edits git had no reason to conflict, and the tree failed
  to compile ONLY because one call named the pre-merge API — a same-NAMED helper would have compiled
  with one representation silently authoritative. Resolve by DELETING the weaker representation (the
  one that reaches fewer consumers), re-run every arm on the MERGED tree, and state which TIP a CNR
  ran at rather than claiming a union CNR the train will measure.
  ⚠ **Its GUARD-PROJECT instance, 2026-09-05:** two seats that EXTEND the same behavioral guard
  project from the same base conflict on its EMISSION files (`main.cs`, the golden,
  `package_info.cs`) as well as on its Go source. **The resolution is never a hand-merge of
  goldens**: the LATER cut rebases onto the EARLIER, unions the Go source (deduping shared type
  declarations), REGENERATES the emission with a converter carrying BOTH fixes, adds a RED control
  against a converter carrying only the EARLIER fix (so the control names the LATER fix), and
  re-reads its gates at the new tip. A dispatch naming an existing guard project as the extension
  target tells the sub-agent which SIBLING seats also extend it.
  ⚠ **Its ATTRIBUTE instance, and it conflicted only by luck** (2026-09-05): two lanes widened the
  SAME attribute's `AttributeUsage` from two different rows (`+Struct` for named arrays; `+Class|Struct`
  for named pointer-to-array wrappers) and it surfaced as a TEXTUAL conflict ONLY because both edits
  landed on one line — had they not, git would have merged two widenings silently. Resolved by the
  SUPERSET, with the union's CNR plus `check-solution-integrity.ps1` as the check that the two stampers
  never double-stamp one declaration. **Read what a cross-lane attribute widening REACHES before
  merging it.**
  ⚠ **When a window EXPIRES and the lane arrives late, DIFF THE TWO ANSWERS rather than choosing by
  AUTHORSHIP** (2026-09-06): the coordinator's landed member and the lane's announced commit were
  byte-identical after whitespace — same override, same value, same placement — so the running battery
  already carried exactly the tree the lane's commit would have produced, and only the COMMENT differed.
  The better comment then lands as a FOLLOW-UP, because the mid-battery source freeze binds the worktree
  and documentation is never worth stopping a multi-hour gate run.
- **⚠ An INSERT adjacent to a line the other side edited folds into ONE hunk, and BOTH single-side
  resolutions silently lose a line** (measured 2026-08-29: master inserted the `go/build` roster row
  directly above `go/build/constraint`, which the branch had annotated — `--ours` dropped the new
  row entirely, `--theirs` dropped the annotation; nothing in the markers says a line vanishes, so
  it reads like an ordinary either/or). Resolve by keeping BOTH sides' content, then **assert the
  structural invariant** (row/line count before == after + known inserts) instead of eyeballing it —
  and validate any re-derived aggregate by **positive control against a known-good blob first**: the
  same derivation must reproduce the other side's banked value exactly before its new value is
  believed. A derivation that cannot reproduce a known-good value is not a derivation.
- **⚠ Its MIRROR is the SILENT SUBTRACTION, and it is worse: one lane REMOVES a definition because
  another branch supplies the replacement, and the merge drops the supplier.** Both diffs are pure
  additions/removals, git merges them without a conflict or a warning, and the result compiles
  nowhere. Paid for 2026-08-29 (`syscall.Uname`): the converter registration that DISPLACES the
  generated wrapper merged, the hand-own `*_impl.cs` BODY it displaces to did not, and the whole
  **linux corpus went RED at master with a clean `git status`** — `kernel_version_linux.cs`
  CS0117 `'syscall_package' does not contain a definition for 'Uname'`, discovered days later by a
  lane building that flavor, not by any merge. Note this is one step PAST the regenerate-never-merge
  seam rule the guilty file's own header documents: the generated side was correct, the destination
  was missing. **Mechanical preflight, cheap and now owed:** if
  `git diff --name-only <base>..<branch>` shows a `manualConversionFuncs` registration or a
  generated-body deletion, **assert the matching `*_impl.cs` body is present in the MERGE RESULT** —
  the same shape as the `package_info.cs` ⟹ `stdlib-metadata.txt` preflight above.
  ⚠ **Its golib-API form, and it is about SIGNATURES rather than bodies** (both measured 2026-09-05).
  **A WIDENING of a shared golib door keeps the PREVIOUS arity as a forwarding overload for at least
  one train**, with a remark naming the retirement condition (grep-proven zero callers), because a
  sibling seat cut against the old arity merges at the union with no fix-up any lane's gates measured —
  the silent subtraction caught one train EARLY only because a lane read another lane's post; the
  two-word form of `OverNativeMemory` means what `unsafe.Slice` means (cap = len). Its mirror: **an
  EXACT-MATCH constructor or overload added for ONE caller silently CAPTURES every existing call that
  bound through an IMPLICIT conversion** — `Pointer(INilPointer box)` took every `new Pointer(box)`
  that had gone through `ж<T>`→`uintptr`, so overload RESOLUTION changed and not just behaviour, and
  only a guard asking the question saw it. **A golib-touching seat runs GolibTests at BOTH
  configurations before it seats**, and a fix to a defect in a SEATED train's union lands on THAT
  seat's branch tip, never on a later branch that stacks on it.
- **⚠ A seam check that verifies a displacement HAPPENED but not that its destination EXISTS passes
  the exact failure it was written for, in mirror form.** The ten-names/zero-bodies property offered
  as the struct-passing merge instrument — every registered name has zero generated bodies and
  exactly one placeholder — was run twice over all ten names and reported as the check that would
  catch a lost registration. It is ONE-SIDED: a placeholder pointing at a hand-own body that does not
  exist passes it cleanly, which is exactly what master held, and the branch carrying the check
  carried the same gap (2026-08-29). **Every seam check carries both sides of the ledger** —
  registration ⇒ displaced wrapper ⇒ body, and the reverse (a dead hand-own nothing displaces) where
  the shape allows it cheaply. Put it in the tier every lane already pays for (the converter's own
  `go test ./...`, beside `projitemsIntegrity_test`) so the class turns into a red converter suite at
  the merge rather than a red corpus later.
  ⚠ **And check what the check's WITNESS is made of.** A displacement guard whose witness is
  ON-DISK placeholders is ENVIRONMENT-dependent for TEST-side hand-owns: it passes on a tree that
  has run that package's `-tests` and fails on every clean clone, because an unbanked row has no
  committed test emission (2026-09-02). Ruled remedy: a GOROOT `_test.go` witness arm, matched by
  CLASS rather than by name and counted separately as the weaker witness; the production arm is
  unchanged.
  ⚠ **A WITNESS CARRYING FEWER AXES THAN THE PROPERTY GOES GREEN ON EXACTLY THE CASE THE PROPERTY WAS
  WRITTEN FOR** (2026-09-05): the registration ledger's body witness collapsed every per-GOOS folder
  into ONE name set, so a scope widened to a flavour with NO body passed whenever another flavour held
  one — route #8's shape, disarmed by the legitimate arrival of a second flavour. **The witness takes
  the PROPERTY's axes** (name × flavour), asserted both ways per flavour. Three rules from correcting
  it. The census of what the CORRECTED guard fires on at master is posted as FINDINGS before any fix,
  and is positive-controlled IN THE DIRECTION OF THE FINDING before its list is believed (the known
  case fires nothing at its real scope and fires on the widened flavours when the scope is simulated
  wider); rows the raw predicate flags are CLASSIFIED before they are counted (a `partial` completion
  is the OTHER displacement mechanism — exempt, with its own control that stripping `partial` goes
  red; a bare-name regex over-collects a different member of the same name, stated at the arm, costing
  a false pass only); and **a corrected GENERAL guard retires a PER-FAMILY guard only if it reproduces
  every one of that guard's controls and is at least as sharp** — a receiver-discriminating witness
  stays beside a bare-tail one. ⚠ Its reading twin: **a NAME appearing in a hand-owned file cannot
  distinguish a WRAPPER DISPLACEMENT from a CONSUMER-SIDE remedy** — a consumer that transcribes a
  native structure MENTIONS the wrapper it is avoiding, textually identical and semantically opposite —
  so remediation status takes THREE signals (a `manualConversionFuncs` key, the placeholder in the
  generated file, a body) and NAMES the mechanism; anything less ranks the work backwards.
  ⚠ **The ledger's REVERSE arm earns its keep on hoisted literals** (2026-09-02): the converter hoists a
  body's string literals WITH the body, so a displaced body's `…ˢ` literals cease to exist and any
  hand-own referencing one dangles — the reverse side found it before a compile did; a hand-own spells
  its own panic text and depends on no hoist the displacement removes. ⚠ **And that clause names only
  the literals with NO OTHER USER: a SHARED hoisted literal RELOCATES into its next user's file**
  (2026-09-05, runtime's `cannot allocate memory` leaving `malloc.cs` for `mheap.cs`, +3), so the
  two-seeded diff shows the relocation as a THIRD file and a footprint applied without that hunk fails
  CS0103. **Read a displacement's diff for hoists that MOVE, not only for hoists that vanish.**
  ⚠ And a **linkname destination declared in a `_test.go` lands in the INTERNAL-test class, where a
  production-side push cannot reach
  it** (`reflect`'s `gcbits`, provided by runtime via linkname: emitted bodyless into the internal-test
  package and picked up by the throwing partial stub) — completion is the reflectlite pattern,
  registration plus a body in `export_impl_test.cs`, witnessed by the guard's test-side arm.
- **A branch behind master shows master's newer files as DELETIONS.** This is the stale-base illusion,
  not data loss, and it has been mis-read as a lane destroying work more than once. Diff from the
  **merge base** (`git merge-base A B`), never from a moving tip.
  ⚠ **And a BRANCH census takes the THREE-DOT form — `git diff master...branch` — never the two-dot
  tree diff against master's tip** (2026-09-04, the coordinator against itself): with a branch based
  on an older master the two-dot form shows master's OWN newer content REVERSED, which read as 33
  "identifier hits" that were pre-scrub doc lines master had since scrubbed, five "golib files" that
  were other trains' landed golib, and an is-ancestor NO against master's tip — all of it looking like
  a lane's defects. The stale-base illusion in a census's clothes: **the merge-base rule binds every
  branch instrument, the SECURITY census included.** The merge's own file count (63 = 24 + 39) was the
  honest number all along.
- **Check the diffstat against the claim BEFORE the push, never after.** A merge whose file list does
  not match what the commit says it does is stopped at that point, not explained afterwards.
- **⚠ A resolver that FAILS must stop the commit** (2026-09-02): a conflict-resolver script's
  assertion failed and the `git add; git commit` chained after it with `;` rather than `&&` committed
  a board carrying three conflict markers — caught only by the marker count printed beside the
  commit, and amended before the push. Chain `python … && git add … && git commit`, and grep every
  merge commit's blobs for `^<<<<<<<` before pushing.
  ⚠ **A docs seat can split a file's FINAL guard line, and no gate sees it** (2026-09-02): the board's
  closing `endraw` guard was deleted, a bare HTML-comment opener written, 284 lines appended and the
  tail half re-added LAST — so the new section published INSIDE a comment, invisible, while the commit
  read normally. A board-touching merge asserts the structural invariant before it lands: one `raw`,
  one `endraw`, the `endraw` FINAL, zero bare openers.
  ⚠ **Two seats told to append a dated block "at the END" of one record collide BY CONSTRUCTION**
  (2026-09-04, met twice in two files on one train): an add/add at the TAIL is the adjacent-insert
  hunk in its purest form. The resolution is mechanical exactly when both sides are PURE APPENDS over
  the merge base (**asserted, not assumed**) — the merged file is HEAD's bytes plus the other side's
  suffix over the base, a duplicate numbered heading renumbered and STATED in the merge message, and
  the line count asserted as base plus both inserts: both sides kept, neither lost, which is what the
  adjacent-insert rule requires. The instruction that AVOIDS the collision names a per-seat ANCHOR (a
  dated heading the resolver can key on) or generalises the board's append-append resolver to design
  records — **"append at the end" is not an anchor.** Its routing companion: **a correction to another
  lane's ROW is that lane's cut**, and a design-record correction lands as a dated block when the
  owner's branch next touches the section.
  ⚠ **A DOCTRINE LANDING IS MEASURED STRUCTURALLY BEFORE IT IS BELIEVED** (2026-09-04): the
  line-ending count unchanged in KIND (CR == LF), ZERO table lines in the diff when no table was meant
  to move, bullet indentation matched at every insertion point, enumerations intact (an amendment
  written INSIDE a numbered list orphaned an item onto a run-on line and was moved out), and the
  removed-line count ACCOUNTED FOR — re-emitted paragraph anchors plus exactly the wording changes the
  batch itself rules on. A batch's item RANGE is re-counted from the accumulator at landing, never
  taken from the dispatch, and the accumulator's own "BATCH n LANDED" line belongs to the train that
  lands it.
- **A separated stack must be verified from BOTH branches** — `git log --oneline master..<branch>` on
  each shows what a merge would really carry.
- **Re-fetch immediately before any merge in a live campaign.** Refs move under you; arithmetic against
  a SHA you read ten minutes ago is arithmetic against a tree nobody has. ⚠ A rebase REWRITES a SHA
  someone else has already been handed: **never force-push a tip whose SHA has been posted — post
  the fresh SHA first** (paid twice in one day, 2026-09-01, the second time crossing a coordinator
  merge that was reading the old one).
  ⚠ **And the rule binds a FAST-FORWARD too** (2026-09-02, three pushes announced AFTER the fact on the
  reasoning that the announced SHA was still reachable): the reader takes the REMOTE TIP, so an ADD
  moves the thing being read exactly as a rewrite does. The form is **announce, THEN push**, for any
  commit on a branch whose SHA has been posted, whatever the update's shape — and a sentence in an
  already-announced commit is retracted in the MERGE message, never by rewriting the SHA.
  ⚠ **Met again 2026-09-05 as a NON-DESCENDANT replacement, which is the worst shape**: a fix to a
  commit whose SHA has been announced, pushed and VERIFIED is a commit **ON TOP**, never a rewrite —
  one narrowed guard replaced its announced SHA with a non-descendant on the remote, leaving the record
  naming a SHA the branch no longer holds, and **announcing the new SHA first does not license the
  rewrite**. SHA-pinned seats survive it only because the coordinator re-points them, and the record
  states that a rewrite happened. Its automation half: **a land script's branch prune FETCHES the
  branch immediately before its ancestry check** — `origin/<branch>` as last fetched at assembly is the
  SEATED SHA by construction, so a lane that kept cutting on the seated branch would have its unmerged
  commits deleted from the remote while the check read "tip in master" (fixed fetch-then-check).
- **Three merge mechanics, measured 2026-09-01/02.** **Union CNR is never skipped on composition
  reasoning** — "both sides are transpile-clean, so the union is" is not a verdict, and the case it
  cannot see is exactly the one that bit: a merge carrying a NEW behavioral test cut from an older
  base went red at the union (the `CollidingPackageNames` red). **A conflict dry-run does not need
  `git merge-tree --write-tree`** — that subcommand is unavailable on this box's git; the form that
  works is a temporary-index `read-tree -m --aggressive -i <base> <ours> <theirs>` plus
  `git merge-file -p` over each unmerged path, a 3-way CONTENT check that never touches the
  worktree, so it is legal under the mid-battery source freeze. And **rebase equivalence is checked
  by TREE, not by commit list**: `git diff <merge-of-old-tip> <rebased-tip>` coming back EMPTY is
  what proves a running battery's verdicts transfer to a train rebuilt on new SHAs.
  ⚠ **The ONE verdict a rebase cannot transfer is a GOLDEN's** (2026-09-05): equivalence transfers the
  verdicts of gates that were RUN, and a golden's validity against the UNION's converter was never
  among them — so **a rebase onto a master whose CONVERTER moved owes a CNR of the branch's OWN
  goldens at the rebased tip even when the branch touches no converter file.** One guard's golden had
  been baselined by a pre-train converter and its nine `FromPinnedBox` lines were invisible to the
  union CNR because the rows carrying them arrived WITH the rebase. Remedy: re-baseline as ONE
  one-file commit — transpile with the REBUILT converter at the rebased tip, then `--update-targets` —
  and never classify a known-stale golden's CHANGED at the battery, nor land it.
- **⚠ SEATING MECHANICS, five rules measured 2026-09-03.** **A SEATED branch takes NO commits at
  all — not even a doc-only one**: the seat's arithmetic is a claim about a SHA, "harmless" is the
  seat owner's judgment, and a train assembling in the two-minute window would have merged something
  unverified; a lane's follow-on work goes on its OWN branch off the same base (the corollary of
  announce-then-push). **A seating instruction gets its OWN LINE at the top of a post** — one embedded
  in a post about something else was read past for forty minutes — and **the slot takes the REMOTE
  TIP** while the assembly log prints the merged PARENT SHA, which is what the lane checks: a stale
  SHA quoted in a train listing would have seated a PARENT and dropped the row that made the tree
  honest, undetectable by the format guard because the table would be consistent and merely untrue.
  **A measurement tree must CONTAIN every seat the cut depends on** — `git merge-base --is-ancestor
  <seat> HEAD` before a single gate runs — because "the fix is in master" says nothing about a branch
  that forked before it: a stacked tree lacking a train's seam fix reported the ORIGINAL bug as a
  fresh "twin" of the fix, retracted within ten minutes once the seat tree was measured. And the
  coordinator's half: **a dispatch that names a MECHANISM nobody has measured is a "measure why first"
  order, never a cut.**
  ⚠ **THREE MORE DISPATCH RULES, 2026-09-04.** **A dispatch's PREMISE is re-derived against the roster
  at DISPATCH time**, never carried from the census's read date: a remaining-rows record read on the
  2nd named a row as an unowned stub, the row BANKED on the 3rd, and the dispatch went out on the 4th
  with the stale clause in every line — the lane's first act was to MEASURE the premise and post the
  table, which is the right first act. **A dispatch gated "after the landing" idles a lane for as long
  as the battery runs, and the gate is usually unnecessary**: when the lane's own seat lands
  UNCHANGED, a branch cut on that seat's tip merges onto the landed master with no seam the seat does
  not already own — so **gate on the SEAM** (a file both sides touch, a registry both register into),
  never on the SHA; a lane silent past the watch's threshold with a landing-gated item is the
  coordinator's idle, not the lane's. And **a cloud lane cannot read the coordinator's scripts
  directory**, so a queue file it is dispatched from is pasted to the mailbox VERBATIM.
  ⚠ **TWO MORE, 2026-09-05, both from a queue file that did not carry its own state.** **A queue file
  carries its LANDED state or the item is dispatched TWICE** — an item landed on train 23 was
  re-dispatched from a file that never got its mark, and the tell that caught it was
  `git worktree add -b` REFUSING an existing branch name; every queue file opens with a `# STATUS`
  header, and the coordinator stamps it AT THE LANDING, never from memory. **And a dispatch preflight
  probe scoped to where the FIX would live is structurally BLIND to a seat that banked a NEGATIVE
  result** — it touched no converter file precisely because it correctly wrote no fix — while
  `ls-remote --heads` is EMPTY for a merged branch because the remote ref is pruned at merge. The
  durable pair is `git ls-tree -r origin/master -- <the deliverable's path>` (the guard project, the
  doc, the manifest) plus `git merge-base --is-ancestor <local branch> origin/master`, with the
  surviving LOCAL branch as the tell that fires when both of those read new.
  ⚠ **And the lane's half of the same idle: a lane whose ORDERED items have all landed STARTS the next
  item on its list without waiting for a coordinator prompt** (2026-09-05, three quiet hours a lane
  named honestly). **The mailbox is read BETWEEN steps, not INSTEAD of them.**
  ⚠ **TWO MORE, 2026-09-06, both about what a SEATED branch may carry.** The no-commits rule has a
  consequence worth spelling: **a correction to a seated record rides in the CUT's OWN commit as a
  DATED amendment, leaving the original sentence visible above it**, because a prediction is never
  edited after its result — including a prediction that turns out to be an inverted statement of fact
  — and **the coordinator corrects the SEAT MESSAGE too when that message repeated the error**, since
  the seat message is the train's own record of what it merged and is where a future reader looks
  first. And **a repair is BASED on the commit that INTRODUCED the defect**, not on a local assembly
  head that has no remote ref: based that way it merges cleanly into any assembly containing that seat
  and it reviews as a repair of a named thing — a train assembles on the coordinator's machine and
  lands by pushing master, so a base nobody can fetch is a base nobody can check.
  ⚠ **A GENERALISATION FROM ONE MEASURED ROW TO ITS SIBLING IS A HYPOTHESIS, NOT A ROUTING**
  (2026-09-06). A confident dispatch said two remaining rows sat behind one unimplemented stub; the
  sibling MEASURED an hour later RUNS — 122 of 160 matching, 38 differing on an entirely different
  axis — so the generalisation was wrong in the direction that costs a lane time, by naming a large
  row as cheap. **The measurement took four minutes: measure the sibling BEFORE naming it in a
  dispatch.**
- **⚠ REBASE AND RE-LANDING, two shapes with explicit acceptances (2026-09-03).** Two of three
  "conflicts" against a new master were ONE DUPLICATE COMMIT (the same patch as the landed seat,
  differing only in blob ids and one hunk offset) — **dropped by `rebase --onto`, not resolved by
  editing** — and a rebase is verified by ARITHMETIC (commit count minus the duplicate, the doc
  byte-equal, the applied delta identical to the original over its files, a temp-index 3-way reporting
  zero unmerged paths first) and lands as a **NEW branch** so the posted SHA stays untouched.
  Re-landing work that master MERGED and then content-REVERTED is the other shape: a direct merge of
  the original branch fights the revert everywhere it touched (58 conflicts, a modify/delete on a test
  the revert removed) and a rebase may silently drop the already-upstream body, so **the admissible
  form is revert-the-revert then cherry-pick the fix**. Its acceptance had to be rewritten too: "the
  re-landed tree equals the branch tip over `src/…`" CANNOT be empty once master has moved, so the
  acceptance is **PATCH-EQUIVALENCE** — the re-landing's delta against current master has the same
  file set and per-file numstat as the branch's own delta (`base^..tip`), the stable patch-id as the
  strong form, the displaced construct's occurrence count at the branch's number, and zero markers.
  **State an acceptance as a relation between two DELTAS, never between two trees, whenever the base
  has moved.**
- **⚠ A SEAT'S GATE LIST AND GOLDEN SCOPE ARE BOTH DERIVED AT THE UNION, not at the seat's base
  (2026-09-03).** A seat's golden re-baseline covers only the projects that EXISTED at its base, so
  two guards born on master AFTER the seat drifted by exactly the seat's intended line and surfaced
  at the union CNR — classify by the diff's CONTENT (the seat's own intended line, nothing else) and
  re-baseline at the train's tip with the runner's four phases as the check: a STATED fixup, never a
  silent one, and never read as a regression. And **a seat whose corpus footprint lands in BANKED
  packages owes those rows' sweeps as its OWN gate**: an array-range-copy seat regressed a banked row
  at the union sweep (224 → 222 + 2 infra) where its full behavioral suite and GolibTests could not
  see an alloc assert in a banked row at all. Its companion rule: **a semantics fix that changes the
  COST of a Go construct is measured against the alloc-assert rows before it is called cheap** — an
  alloc mirror that maps to `GC.GetTotalAllocatedBytes` cannot be exempted by a counter, so the copy
  must genuinely not allocate (a rented snapshot returned on dispose, measured 0 objects / 0 bytes
  against a 1,000-object control). Remedy shape: unseat, fix forward on the branch (an uncounted site
  with the reason, never a weakened instrument), reseat with the sweeps.
  ⚠ **Its inverse, 2026-09-05: an emission change whose ONLY corpus movement is TEST-side in BANKED
  rows lands those hunks WITH the cut** — attribute lines only, numstat taken against the PRE-RUN
  tree. The lane's filtered sweeps re-derive exactly that emission, so **the tree the sweeps
  validated is the tree that lands**, and a banked row's committed test source never disagrees with
  the converter that just swept it. Distinct from the stale-until-rebank class, which is for changes
  NO sweep in the train re-derives.
  ⚠ **A UNION-ONLY EMISSION CHANGE — a golden the union CNR moves that NEITHER seat's own CNR could**
  (2026-09-05). Two converter cuts each byte-identical under their OWN CNR together stamped a named
  NESTED array (`type nn [2][3]int` → `[GoArrayDims(2, 3)]`) that neither stamped alone. ⚠ **Corrected
  by the same coordinator the same day, because the first reading of WHY was wrong**: it is not "two
  rules composing" — it is ONE rule meeting NEW SOURCE. The named-array stamp had nothing to stamp at
  its own base (`nn` was declared and unused, so no wrapper was emitted, and that seat's CNR was
  honestly byte-identical — MEASURED by rebuilding its exact converter in scratch: 0 diff lines),
  while a SIBLING seat added +93 rows of `main.go` exercising `nn`, so at the union the struct exists
  and the rule fires. **A seat's CNR is scoped to the SOURCES AT ITS BASE; a sibling seat landing new
  source rows creates the shape another seat's rule fires on; only the union CNR sees it.** Handling:
  before re-baselining, MEASURE the union emission (the filtered runner at the union — Compile plus
  Output against `go run`) so the golden change is a proven-intended emission rather than a
  papered-over regression; the re-baseline is an ASSEMBLY commit NAMING the composition, since no lane
  owns a golden; and because the CNR leg's restore step destroys the evidence, reproduce the emission
  into SCRATCH with the union converter. (The coordinator's first inference — "possible false-green CNR
  at a sub-agent tree" — was retracted in the open: **when an inference about a lane's instrument can
  be measured in minutes, measure before naming it.**)
- **⚠ THE UNION IS THE GATE OF RECORD, and a landing HOLDS on a broken banked set (2026-09-03).** A
  lane's own CNR is evidence for a seat REQUEST; the union CNR at assembly is the gate of record — so
  a lane-side freeze slip that cannot reach a transpile verdict is REPORTED, which is the remedy, and
  does not re-owe the lane's run. ⚠ A finality table's CNR row read "(read the log)" because the union
  CNR had ended WITHOUT a verdict line and the finalize step's placeholder fill had a FALLBACK that
  printed a string where the verdict belonged — route #6's shape inside a lane's own finalize, an
  absence MASKED instead of stopping; the lane retracted FINAL the same minute, the coordinator
  honoured the retraction with a stated hold window and independently preflighted the branch **from
  its MERGE BASE** (a two-point diff against a moved master had read 92 files / −5,700 where the
  merge-base diff read 30 / −5). And **a pass→fail on a BANKED row at a train head is a BROKEN SET
  that holds the landing until an arm names the seat** — master plus one seat at a time, the
  three-run standard, the attributed seat unseated at the tip.
  ⚠ **A banked row reading RED at a union is attributed against the PREVIOUS unions' PRESERVED RECORDS
  before it is called a regression** (2026-09-04): one train's `net/http` FAIL was byte-for-byte the
  previous three trains' shape — every verdict matched, the leak check exiting 1 — a STANDING red that
  the crypto/tls merge rule does not reach, read in ONE command because the preservation rule had put
  each union's record at a distinct path. **A union red with no prior record to compare against is the
  case that costs a bisect: keep preserving.** And **a filtered `-tests` run on a branch trains behind
  master measures the OLD closure**, so a gated re-measure runs on the MERGE RESULT.
  ⚠ **FOUR ATTRIBUTION RULES FROM ONE UNION (2026-09-05).** **A tree that SHOWS a failure often cannot
  say whether the failure PREDATES it**: the cut's red control crashed BEFORE reaching the dial, so
  only an arm with the FIX APPLIED and the suspect seats ABSENT could separate "the union broke it"
  from "the fix made a dead path REACHABLE" — build that arm (landed master plus the same three fix
  files PASSED the guard completely; the assembly head with the same files failed every dial; one axis,
  same machine, four minutes apart). **Probe the PRIME SUSPECT first and a bisect can end in ONE arm** —
  the pointer-token seat was the FIRST merge on the branch, so testing it cost one probe and settled the
  attribution outright — and **copy only the CORPUS files of a fix into a bisect arm, never the
  converter registry**, since the behavioral runner transpiles only the TEST tree, so the corpus files
  are what the guard compiles against and leaving the converter alone keeps other seats' rows out of the
  arm. **An IDENTICAL verdict count across two runs is NOT evidence that nothing changed**: the same row
  stopped at the same test both times because it is the first one that DIALS, while the mechanism
  changed completely — before, an access violation and NO results file; after, a results file stating
  the process ended before the host completed, with the sweep log carrying the refused socket option
  verbatim one line up. **The difference lived in the TAIL, never in the count.** And **clearing a
  MASKING fault is progress even when the row stays red**: an access violation with the stdout
  comparison never reached became a stdout MISMATCH with both sides exiting 0 — **report a guard's
  movement by FAILURE KIND** before anyone reads "still failing" as "nothing changed".
  ⚠ **A seat that lands ONE SIDE of a two-route identity NAMES the assertion it breaks — in its seat
  message, derived from the DESIGN before any battery is asked** (2026-09-04): a
  value-versus-constructed identity assertion goes red BY CONSTRUCTION when only one route moves, and
  a row that had been passing because BOTH routes were equally wrong is not a regression — but it is
  still named, because the union will report it. The coordinator's half: **a union set-diff's BROKEN
  entry is attributed by MECHANISM — from the failure text and the seats touching that code — before
  it is called transient.**
  ⚠ **THE LEG THAT READS BY SET DIFFERENCE CATCHES WHAT PASS-OR-FAIL LEGS CANNOT** (2026-09-06).
  Thirteen legs were green — the converter suite, a byte-identical corpus across 721 packages,
  three-platform compiles, a 684-project behavioural suite with zero failures, 25 of 25 sweep rows —
  and the FOURTEENTH found that a row's caught nil-dereference had become an uncatchable ACCESS
  VIOLATION killing the host and leaving 221 rows unmeasurable. No banked row regressed and no roster
  number would have shown it. **Trading a MEASURABLE row for an UNMEASURABLE one is a REGRESSION even
  when no banked number moves**: master measured that package 388 of 388, the union kills its host
  partway and leaves 221 rows unanswered, in a row a lane is actively working — so the train HELD. The
  four one-axis readings that attributed it (previous master, the suspect seat's own merge, the bare
  assembly head, the repaired head) also proved the repairs neither caused nor cured it, which is what
  a set-difference finding owes before it names a seat.
  ⚠ **THREE RULES FROM ONE HELD TRAIN, 2026-09-06.** **A REGRESSION AGAINST A BANKED ROW IS NOT
  ELIGIBLE FOR ACCEPT-AND-NAME** — landing with a defect named and open is for an OPEN defect; a
  change that takes banked verdicts DOWN is either fixed or its seat comes out of the train. The
  coordinator offered accept-and-name for a `reflect` regression on the reasoning that its root was
  a model question too large to answer under a landing deadline, and withdrew it: **388 verdicts
  falling to 167 reported is the roster going backwards, and no elegance of root makes that
  landable.** **A SEAT THAT CHANGES A FAILURE'S MODE FROM CAUGHT TO UNCATCHABLE IS A REGRESSION even
  though it created no new defect** — pre-seat the same operation already failed while the package
  still reported every verdict; post-seat the host dies and 221 rows go EMPTY — and reading the two
  trees honestly is what SHRINKS the fix: the ask is not "make the operation work" (the model
  question) but "make it fail the way it failed before", refuse by name, catchable, the defect left
  open and LOUD. **Compare the failure MODES across the two trees before sizing a fix against the
  failure itself.** And the arm that names the seat is cheaper than it looks: **bisect the TRAIN
  before reasoning about its seats.** Building an alternative assembly to cost a seat-drop
  incidentally produced every intermediate SHA, so the ladder over the known merge order — master,
  ten seats, eleven, thirteen, fifteen, sixteen — was a checkout and a run per rung and converged
  exactly. The measurement most needed was the cheapest available and it was run LAST, after an hour
  spent building an alternative to a misdiagnosed problem; **the intermediates already exist.**
- **⚠ TRAIN-ASSEMBLY MECHANICS, four traps in one assembly (2026-09-03).** A seat cut off an OLDER
  base conflicted on SIX roster blocks, not the four a `head -12` grep showed — the filtered-status
  trap in a grep costume — and a resolver asserting `len == 4` bailed BEFORE writing while the
  `git add` chained after it with `;` staged marker-bearing files (the `;`-vs-`&&` rule again, caught
  before commit by the roster guard reading the HEAD side). The resolution rule that worked: **take
  the RULED side's prose for every conflicted block and re-derive only the numbers, then let the
  guard-as-calculator confirm.** A script trap beside it: `${X:-default}` treats an EMPTY env override
  as UNSET, so blanking a seat by env cannot skip it — a seat script carries an explicit
  already-seated list.
  ⚠ **A TRAIN'S MERGES ARE REHEARSED IN A THROWAWAY WORKTREE AT THE LANDED MASTER BEFORE THE ASSEMBLY
  RUNS** (2026-09-04). A pairwise three-way against master cannot see SEAT-VERSUS-SEAT collisions —
  two seats appending to one test file, two to one record — and the old-form `merge-tree` PREFIXES its
  markers, so a line-anchored marker count reads ZERO: the sequential rehearsal named FIVE conflicts
  where one was expected. **Each resolution is VERIFIED before it is saved** — a both-kept resolution
  of a CODE block is proven by `gofmt`, a build, and a COUNT of the symbols both sides own, never by
  the absence of markers: one both-kept resolution SPLIT a function, and a failed three-way apply left
  a clean file MISSING one side's function behind a green `gofmt`, caught only by the 2-of-3 symbol
  count (and the exit printed for `vet` was a pipe's). The saved resolutions are then applied
  mechanically at assembly and stamped PRE-RESOLVED, so the assembly meets no surprise and the hand
  work happened where a mistake cost nothing.
  ⚠ **A guard that pins entries BY EXACT KEY is a seam every later seat's entries cross**
  (2026-09-04): one train's amendment made a registry guard per-entry — an unpinned key fails outright
  — while a sibling seat's twelve entries had been cut before it under the older suffix rule, so each
  branch was green ALONE and the union RED on the first unpinned key by name: the crypto/tls merge
  shape in a converter test rather than in a corpus row. The rehearsal caught it; the fix is a UNION
  commit (the seats' entries pinned under the stricter rule, stamped as the train's own), and **a seat
  that adds keyed entries checks the UNION's rule for that map, not its base's.** Two instrument notes
  from the same hour: a resolution that replaces a WHOLE FILE with one branch's version silently drops
  every OTHER seat's change to it (master plus each seat's patch is the shape), and a bare `grep -c`
  of a symbol that must read N is the check that caught a clean `gofmt` hiding a missing function.
  ⚠ **A SEATED-BUT-UNLANDED BRANCH IS INVISIBLE TO EVERY CHECK A LANE CAN RUN, and that is the
  coordinator's debt, not the lane's** (2026-09-06). The assembly is local to the coordinator's machine
  BY DESIGN, so a lane censusing DELIVERABLE PRESENCE at master — the correct instrument, since a merged
  branch's ref is pruned and a surviving ref is the tell — correctly reads five of its own branches as
  ABSENT while they are ancestors of a live assembly head. The lane's three right moves were to census
  rather than guess, ask rather than re-offer, and NAME what it could not see; the coordinator's
  obligations are three. **Put the SEAT LEDGER's contents into status posts BY NAME** rather than
  leaving them in private notes. **"Specified and queued" means nothing if the QUEUE IS LOCAL** — an
  instrument was queued with predictions on record, in a file on the coordinator's machine that no lane
  can read, and the lane that could not find the specification derived its own, which cost it time and
  produced a BETTER instrument: send the artifact, or say it does not exist where they stand. And
  **when a lane asks for a REF instead of a description, PUBLISH one** — a read-only, transient
  reference branch with nothing based on it, nothing merged into it, deleted at landing — rather than
  answering with a caveat the lane cannot check; then answer "which commits touch my subject" by
  MEASURING it (exactly one commit of twenty-one touched the row's own trees, it was the one the
  dispatch was about, and the nearest reachable approximation would have been contaminated in precisely
  that place). The lane-side consequence: **a row's current NUMBER can depend on an UNLANDED seat, so a
  measurement at master answers about a DIFFERENT row** — assert the seat is an ANCESTOR of the
  measurement tree before running, build the converter from that tree, and check the binary's mtime
  MOVED rather than merely existing, three separate ways of not fooling yourself. Such a row's
  improvement ARRIVES WITH ITS TRAIN and belongs in the objective's arithmetic, not in a lane's memory.
  ⚠ **AN ALTERNATIVE THAT MUST BE COSTED IS BUILT, NOT DESCRIBED — and a coordinator's OWN assembly
  commits are ungated by construction** (2026-09-06, one held train). **A file-overlap census
  predicts where conflicts are POSSIBLE; only an assembly says where they ARE.** A seat-drop was
  sized by diffing each commit's file list against the dropped seat's two named collision points (a
  registry and a golden) and NEITHER conflicted, while a file the census had attributed to a
  different seat did — a conflict is a property of what the three-way BASE looks like once a seat is
  absent, not of which files a commit touches. The real assembly came in CHEAPER than the estimate
  (16 merges, ONE trivial resolution where the seat's side was a strict superset, zero markers) and
  it is the only thing that could have found the actual collision. The other half is the
  coordinator's own: a hand-own cut by a sub-agent and merged as an ASSEMBLY commit carried a
  CONVERTER registry change, so its blast radius was never the two corpus files it edited and it
  emptied 221 verdicts in a package two subsystems away — while four hours were spent blaming a
  lane's seat. A lane's seat arrives with its own gate lines; an assembly commit arrives with none
  and the battery that follows attributes to the whole train, so **an assembly commit carrying a
  converter change owes the same measured blast radius as a seat, taken BEFORE it is seated** — or
  it is not an assembly commit, it is an unmeasured seat wearing the coordinator's authority.
- **⚠ A CONCURRENCY TRANSIENT IS READ FROM THE FAIL LINE'S ERROR TEXT, and its arm is an ISOLATED
  re-run (2026-09-02/03).** The converter suite failed ONCE under five concurrent sub-agents with
  `go: go.mod file not found in current directory or any parent directory` from its `go` child and
  passed 3/3 in isolation at the same tip twelve minutes later; CNR printed `[transpile FAILED]
  <Package>` mid-run under seven concurrent processes while a hand transpile with the SAME binary
  minutes later emitted a `main.cs` byte-identical to the committed golden. The mechanism is UNROOTED
  (a shell-out whose cwd vanished under it is the shape; which concurrent purge did it is not
  measured) — **an environment-shaped message is the tell**. The gate honestly reports NOT MEASURED by
  name, and the COMPLETION is a per-package re-transpile IN PLACE after the run with a `git status` of
  that directory, stated in the landing post — never a whole-CNR re-run and never a "close enough".
- **⚠ A STATE-ADVANCING TOOL ASSERTS THE STATE MOVED (2026-09-03/04).** A delivery check comparing
  LOCAL to REMOTE PASSES when nothing was committed: a mailbox post instrument split its message on
  embedded double quotes (PS 5.1 native-argument quoting), the commit failed as a bad pathspec, and
  `DELIVERED=True` printed because the failed commit left local equal to remote. Such a tool asserts
  `HEAD != pre-append tip`, exits non-zero otherwise, passes free text through a FILE (`-F`) rather
  than an argument, and is positive-controlled with the INPUT SHAPE that broke it before its next real
  run — route #6's shape in the coordinator's own hand, surfaced by the read-anchor rule. ⚠ And **a
  ledger entry is stamped from `git log --date=format-local` of the post it records, never from an
  estimate**: lane commit stamps carry the LANE's clock (a cloud container's is UTC), and reading them
  as local ran a ledger ~35 minutes fast for an hour and mis-sized a running CNR leg as past its
  budget when it was on pace.
  ⚠ **IN A SHARED CLONE AN UNCOMMITTED EDIT BELONGS TO WHOEVER TOUCHES THE PATH NEXT** (2026-09-04): a
  sibling's mailbox post swept a scrub lane's three uncommitted substitutions into its OWN commit, and
  a second sibling's tree operation then reverted the scrub lane's remaining files before they could
  commit — so `git status` read CLEAN and `git commit` read "nothing to commit" while the work was
  gone. **The edit-to-commit-to-push window in a shared clone is ONE command**, a clean status there is
  never evidence that work landed (read the pushed TIP), and a post instrument stages ONLY the file it
  owns — never `-A`, which IS the sweep mechanism — and restores ONLY that file on its own failure
  path: the coordinator's own post script ran `git reset --hard` in the shared clone when a commit did
  not land, which is the revert mechanism, scoped to its own file the same day. ⚠ And **a `git push`
  reporting `remote rejected` with exit 1 had LANDED** — `ls-remote` settles a push, so "read the
  pushed tip, not the exit code" cuts both ways.
  ⚠ **Two more, 2026-09-06.** **A guard whose INPUT the caller can derive from the same source it
  checks against is not a guard**: a mailbox read-confirmation passed as `$(git rev-parse
  origin/<mailbox>)` makes the comparison tip == tip, always true, so it catches a STALE confirmation
  and CANNOT catch a freshly computed one — and a lane advanced its anchor past an unread post exactly
  that way, in the one tool whose whole job is to prevent it. **Anchor such a check on state THE TOOL
  REMEMBERS** (it writes its own anchor after each successful post and derives the absorbed range from
  that) **and treat the caller's argument as a CLAIM to cross-check, never as the anchor.** And the
  ls-remote rule's mirror: **a REFUSED branch DELETE answers `Everything up-to-date`** — the remote
  rejects the deletion with an HTTP 403 and git then prints the ordinary no-op line, so with stderr
  redirected the command reports three cheerful "already gone" results while all three refs sit
  untouched. The tell is `ls-remote` AFTERWARDS, never the exit code; push may work from a session
  where delete does not, and the difference is invisible without reading the refs — the same family as
  the shell eating a command interpreter's switch, an operation reporting success because it never ran.
  ⚠ **A VERIFICATION CLAIM IS POSTED ONLY FROM THE VERIFICATION'S OUTPUT, never composed in the same
  response as the command that produces it** (2026-09-04, the coordinator against itself):
  "pre-verified at the remote — five commits, zero markers, zero census hits" went out in a reply
  issued in PARALLEL with the fetch, and the fetch answered `couldn't find remote ref`; the branch had
  never been pushed. **Any post that STATES a measurement is a DEPENDENT step of that measurement** —
  parallelism is for independent items only, and a claim about a result is never independent of the
  result.
  ⚠ **A `&` BACKGROUND LAUNCH IN A COMPOUND BASH COMMAND SWALLOWS EVERYTHING AFTER IT, HEREDOCS
  INCLUDED** (2026-09-06). An urgent retraction was written as `cmd & … cat > entry <<EOF … post`:
  the launch backgrounded the rest, the entry file was never created, the post never ran, and the
  only visible output was a tool banner. **A post is CONFIRMED by reading the REMOTE — `git fetch`
  then grep for the post's own distinctive line — never by the absence of an error.** The tool's
  ENTRY-FILE-MISSING guard caught the second attempt; nothing caught the first. **Launch background
  work in its OWN call.**
- **A gate that has never been made to fail proves nothing.** Before trusting a census/self-verify that
  reports zero, regress one site deliberately, confirm it reports exactly that site, then fix and
  re-verify — and confirm the restore is byte-identical. The same principle as the positive controls
  the corpus loop uses: a green that cannot go red is not a measurement. Two refinements, both
  measured 2026-09-01: **a positive control must neuter a check no OTHER check subsumes** — under
  defense-in-depth, a broken control still reads green because a downstream check catches the
  regression it injected, so the control proves nothing about the check it targets (verify the
  control's red names the RIGHT assertion); and **a finding's PROSE is not its record** — a routed
  finding's description (a chip, a board line, a relayed diagnosis) is re-derived from the captured
  comparison/measurement record before anything is built on it, because the sweep-encoding chip's own
  description ("go pass vs C# fail") was wrong on the load-bearing detail and a rule built as
  described would have refused the very host it existed for.
  Two more, 2026-09-02. **A control only tests the AXIS YOU VARIED**: eight plausible, well-formed,
  entirely wrong findings passed BOTH of a census's controls because every repro varied box-ref and
  none varied RECEIVER KIND — so list the axes the predicate actually reads, and vary each one in a
  control. **And a control that does not use the CALLER's input shape is not a control for the
  caller**: a helper's self-test passed lines with content while every real call site passes BLANK
  lines, and a `Mandatory [string[]]` parameter rejects an empty ELEMENT (`[AllowEmptyCollection]`
  does not cover it), so the helper threw on every real invocation while the step's verdict and its
  artifact both looked normal — a guard that could never go green, caught only because a dispatch's
  annotations arrived from something else. Test with the exact type and shape the call sites pass.
  Three more, 2026-09-02. **Isolate by the RELATION the defect travels on, not by textual mention**:
  "classes that mention `flag`/`testing`" found four, "classes that drive `TestHost.Run`" five, and two
  single-cause fixes each passed a green full suite while an each-class-ALONE control still aborted — run
  every member alone. **A probe green on one binder path says nothing about the other**:
  `Delegate.CreateDelegate`'s static overload refuses a `DynamicMethod`, so a row came back
  infrastructure-error where the bound-path probe was green — exercise BOTH paths or NAME the one you
  skipped. And a committed disclosure quoted a 125/250/500 ms ladder its source does not contain (the
  rungs are 250/500/1000): re-derive a disclosure's mechanism from the line it cites, and post the RAW
  numbers beside any reading, since a measurement outlives the interpretation attached to it.
  ⚠ **TO TELL A ROOT FROM A CASCADE, RUN THE SUSPECTED DOWNSTREAM MEMBER ALONE — not just the
  suspected root** (2026-09-06). A 38-verdict cluster looked like one failing test leaving a resource
  open with every later test reporting `already in use`; a single-test gated run of the DOWNSTREAM
  test failed on the SAME unimplemented primitive as the head test, so the `already in use` text was
  a symptom of the START PATH throwing, not of a predecessor's leak. One deep root, not a cascade —
  which is better news, because it makes the row answerable by one piece of work and it relocates
  that work out of the package and into the runtime. Two gated runs at ~90 s each replaced a
  plausible story. **A cascade shows as a fixed ORDER rather than a set: count the ROOTS before
  sizing the number.**
  Four ATTRIBUTION rules from one night of probe work, 2026-09-02. **A variant table names what each
  variant REMOVES and the attribution line is DERIVED from that column** — a swapped label on a correct
  measurement survives review by looking self-consistent. **An attribution is a ONE-AXIS pair**: a pair
  differing on two axes (container AND assembly) read 2.7x where the one-axis pair read 4.17x, and the
  design is cut against the one-axis number. **A gap between two arms of the SAME code with the SAME
  attribute is a CONFOUND TELL, never a boundary cost** — identical IL inlined from two assemblies
  yields identical machine code, so 4.0 vs 11.1 ns/word means an unoptimized callee or a declined
  inline: read `DebuggableAttribute.IsJITOptimizerDisabled` INSIDE the probe process, and on a release
  runtime read inlining from `DOTNET_JitDisasmSummary=1` (an inlined callee is absent from the list),
  since `DOTNET_JitPrintInlinedMethods` prints nothing there. **A hand-transcribed proxy is diffed
  against the emission before its number is quoted** — one token moved a 2.75x reading — and a
  retraction's positive claim owes the same measurement as the claim it retracts. Three control-FORM
  rules beside them: **a gate is ruled only after its BEFORE shows it can MOVE** (the TZ-pin gate row
  was green before the pin existed; calibrate with the variable genuinely ABSENT, since `TZ=` empty
  means UTC in Go and reads exactly like the pin); **a body's own failure is earned by a control in a
  SEPARATE worktree at the same SHA**, never by splitting the cut into commits; and **count a guard's
  DISCRIMINATING lines, not its lines** — a loopback receiver on 127.0.0.1 was GREEN against the body
  it guarded, a destination zeroed to 0.0.0.0 arriving anyway (bind 127.0.0.2 so arrival depends on
  the octets, and exercise the OLD path in the control).
  ⚠ **Count a guard's DISCRIMINATING ARMS the same way** (2026-09-04): a control forcing the old
  behaviour reddened 3 of 7, not the 4 claimed, because two of the arms are cases where the old and
  the new behaviour COINCIDE — must-not-regress arms, not evidence about the mechanism.
  ⚠ **A BILL LINE READ FROM THE TRACE CAN BE THE WRONG WAY ROUND** (2026-09-05): "every
  `StructField.Offset` reads 0 — offsets are never synthesized" was billed off a stack, while the
  failing line's own printed operands (`mismatched offsets: 8 0`, i.e. `f.Offset` then `offs`) said
  the SYNTHESIZED offsets were Go's and the TEST's expectation — raw address arithmetic over managed
  storage — was the zero. **Read the failing assertion's PRINTED OPERANDS before classifying a row**;
  a row re-billed by its own line moves classes without a cut.
  Four more control-design rules, 2026-09-02. **A ruling's load-bearing assumption is MEASURED before
  any code exists, with a negative control that fires** — libc `setegid` reaches an already-parked .NET
  thread (glibc's setxid broadcast) where the raw `setresgid` syscall does not, so the design records
  what a plain `DllImport` buys and what it does not, and the keystone's reason stays the structural
  one; a probe that changes PROCESS CREDENTIALS lands as a guard only with a privilege check and a LOUD
  unprivileged skip. **A probe proving a fix's SHAPE includes the case the NAIVE fix would OVER-CLAIM**
  — minting `Promoted=true` flipped the 35 target verdicts and made a VALUE store assert true where Go
  says false — and a generator that emits NOTHING for a type under the alternative flag is route #7's
  neighbour and gets a guard note. **A control that needs a 25-minute CNR to run is a control nobody
  runs** — give it a check-only switch — and its arms must reach EVERY step: two positive arms never
  reached a strip step, so a `[char]` `Replace` overload bug lived there until a negative arm (drift
  PLUS one unrelated hunk) threw. **And when every synthetic axis comes back clean, the differentiator
  is INSIDE the row**: the next measurement goes inside the failing test (a `t.Logf` before the
  `Fatalf`, one gated run on a host that has the package), not beside it — the same reason an
  IN-CONTEXT ratio understates an isolated one (2.7x against 7.5–11x, the surrounding loop diluting the
  cost), and the reason a reduction is trusted only once its assertion string appears VERBATIM in the
  real row's output.
  Two more, 2026-09-02, both guards that could never FIRE. **A `[string]`-typed PowerShell parameter
  coerces `$null` to `''`**, so a refusal written as "no readable tail" could not trigger on any input
  — an unreadable deadline tail would have read as clean. Untype the parameter and assert BOTH
  spellings, absent and empty. **And a BEFORE arm that produces NO output makes every arm read
  DIFFERS** — an instrument failure wearing a finding's clothes (the extracted copy was correctly
  throwing on a missing `Directory.Build.props`). A comparison that cannot report IDENTICAL on a
  known-identical arm proves nothing: control that the BEFORE arm prints at all, THEN positive-control
  the arm that must go red.
  ⚠ **A control's FLOOR is DERIVED from a text bound BEFORE it is committed to, never set from feel**
  (2026-09-04): a mis-sized control fires on the POPULATION rather than on the instrument, and the
  number alone cannot tell the reader which — the resolution is a SECOND derivation (the instrument's
  arm against a build-tag-blind text upper bound) plus the known-count guard sources. Corollary:
  **name falsifiers in BOTH directions** — the two that fired on that census resolved opposite ways,
  one on the population and one on the prediction.
  ⚠ **BEFORE sending a probe at two candidate mechanisms, ask which ARGUMENTS the failing call READS**
  (2026-09-05): a struct the callee only WRITES cannot produce an errno about BUFFER SIZE
  (`getpwuid_r`'s ERANGE), so a wrong layout there corrupts or faults rather than explaining the
  symptom — the DISCRIMINATING arm is the one on a READ argument, and a candidate settled STATICALLY
  (a managed struct with reference fields handed by address) is owed without a measurement at all.
  State the prediction in the sharper form for the same cost: **"if X is the cause this arm CHANGES the
  errno; an arm leaving it unchanged has FALSIFIED its own candidate"**, rather than pass/fail.
  ⚠ **Three more, 2026-09-05.** **A cost canary's WALL that sits in a band ~100 s above the same
  box's reading hours earlier is ATTRIBUTED BEFORE IT IS QUOTED**, and the attribution instrument is
  a FOURTH ARM that re-runs an OLDER known tip TODAY: cut ×3, its own base, and the earlier tip all
  landing in one band means the move is the HOST'S DAY (load, disk, session age), not the code — **a
  canary compared against a number taken on a different day compares two hosts.** **A negative
  control whose red lands at a DIFFERENT ARM than the defect's symptom STATES the offset rather than
  smoothing it**: a count assertion firing one step before the poll that would have hung still names
  its own assertion, but the reader must know it did not reproduce the symptom's SHAPE — say which
  arm went red and why it precedes the symptom. And **a CAS on a managed slot holding boxes compares
  box IDENTITY** — the comparand is the exact INSTANCE observed in the same iteration — guarded by a
  racing-push arm and a distinct-box-same-address arm, never left unstated as "compared by address".
  ⚠ **The publish shape that CAS belongs to, and the control that proves it** (2026-09-05, the
  field-view cache): **a lock-free publish by compare-exchange reads the head ONCE, scans THAT exact
  list, and CASes against THAT same head** — scanning for a racer's entry only AFTER a failed CAS
  admits a racer whose publish lands between the caller's miss and its head read, and the result is
  two views of one field. The guard's concurrency arm (`Barrier` + `Parallel.For`, 50 × 16) was RED on
  its first run in BOTH configurations and it was exactly that defect. Its control half: **a positive
  control neuters the MECHANISM, never a switch every arm resets** — neutering the Disabled default was
  measured VACUOUS first, while the mechanism neuter read 5 RED / 2 GREEN with each red naming its own
  assertion.
  **⚠ SIX MORE, 2026-09-04, four of them retractions.** **The neuter rule met from the ARM's own
  side**: a wiring arm asserting "the cache is empty after `runtime.GC()`" stayed GREEN with the
  synchronous clear DELETED, because `GC()`'s own tail drains finalizers and the registry's sentinel
  clears the cache by the ASYNCHRONOUS route — emptiness cannot discriminate the two paths, so the arm
  guarded neither, and only making it fail exposed that. The discriminating property is WHERE and WHEN
  the clear ran (caller thread at the head of `GC()` versus the finalizer thread; the gen2 count at
  the first clear), timing-free — and the refuted control also taught something TRUE about the line it
  guards: it is a GUARANTEE of Go's contract (`clearpools` at `gcStart`, synchronous), not the only
  route to the outcome. Beside it, **a lane measuring a suite RED at master names the FIVE-MINUTE
  CONTROL (the suite without its file) before attributing**, and checks whether a SEATED cut already
  owns the reds. **A control arm that measures a DIFFERENCE over a WINDOW can pass for the wrong
  reason**: a before/after `NumGoroutine` delta read GREEN against a neutered predicate because a
  sibling test's goroutine exited inside the window and cancelled the +1 — assert the RELATION at a
  MOMENT (the count while the goroutine is registered, against the total that sees it); and when a
  control goes red, read WHICH arms went red and whether each names its OWN assertion, because "the
  control failed" is not the reading and "these arms failed on these assertions" is. **A COUNT that
  matches its prediction is not a SET that matches**: 19 admitted declarations equalled the predicted
  19 while two MEMBERS differed — one in that should have been out, one out that should have been
  in — and only a build failure on the first exposed the cancellation, so **a prediction names
  MEMBERS and its scorecard compares the SET**, and a "to the digit" claim on a count is retracted the
  moment the membership is read. **A falsified EXPLANATION does not falsify the MEASUREMENT it was
  invented for**: a delta measured at 510.1 B was explained by a side table, the explanation was
  refuted by segmentation, and BOTH the lane and the coordinator then retired the NUMBER with it —
  while the number was right (512 = 384 + 128, two surviving boxes un-escaped). The measurement and
  the story are independent claims: when a mechanism is refuted, re-derive what the measurement
  OBLIGES and leave the number standing as an unexplained residue (a ladder to which no story was
  attached — the count, 17/11/10 — survived every revision in that arc). **A defect REPORT is measured
  at the REPORTING BRANCH'S OWN BASE converter as well as at master before anything is built for it**:
  a routed emission-mangling chip reproduced at NEITHER (six conversions, byte-identical), both of its
  diagnoses fell on rows written for each, and the standing population — thousands of compiling
  formats of the same shape — had said so at one grep; `CS1010` beside `CS1003` is the signature of a
  TEXT-CORRUPTED file (the r41 overlap family), never of an emission decision, an elimination
  comparing two calls that differ by file POSITION and line FORM has isolated nothing, and the
  negative result banks as a GUARD pinning the emitted form plus a dated record, never as a fix that
  cannot be made to fail. And **the differential control has an ARITHMETIC form**: under standing
  corpus drift an ABSOLUTE byte-identity leg (committed cut against fresh emission) fails BY
  CONSTRUCTION, and the cut is exonerated when `D(master, emission) = D(cut, emission) + the cut's own
  lines` closes FILE BY FILE, the residue being the standing forced-init/relocation debt named per
  file — a chain's FAIL flag for such a leg is READ with that meaning rather than re-run green, and
  the result post states which FORM each leg took.
  **⚠ THE ORACLE'S CAPTURED STRINGS ARE THE SPECIFICATION — its FALLBACKS included** (2026-09-03).
  Go's own `valueMethodName` climb fails on the package-level `reflect.Append` path and Go itself
  prints `call of unknown method on int Value`, so threading the public name there would have "fixed"
  a string Go DELIBERATELY prints and broken the byte-compare gate. **Our climb failing where Go's
  SUCCEEDS is the defect; where Go's own fails is contract.** Capture the oracle's text through the
  PUBLIC entry point before choosing what to thread — and prefer a SOLE-CALLER proof to a capture (a
  panic you fail to provoke proves nothing about reachability), with a composer whose own test on the
  threaded name preserves the fallback BY CONSTRUCTION rather than by a special case. ⚠ Its
  construction rule: **an expectation read off the thing under test is not a test of it** — a
  formatter-delegation guard's eight expected strings were taken FROM GO under the pinned toolchain,
  and it went red on its first run for a REAL reason (a C# `int` is Go's `int32`, not `int`): the
  mapping is PINNED, not one answer. ⚠ **The TIDY-LOOKING change is measured against Go's own output
  BEFORE it is written** (2026-09-04): once interface method names were package-qualified, sorting the
  RENDERED (qualified) strings looked like the obvious completion — and Go sorts by the BARE name
  (`interface { zlib.aaa(); main.zzz() }`), so the tidy sort would have REVERSED Go's order. The sort
  stays untouched with the evidence in a comment AT THE SITE, because the next reader will see the
  qualification and reach for it; and a row that cannot be built (it needs a sibling package) is
  recorded in the guard's comment rather than left unmentioned.
  **⚠ PREDICATE DISCIPLINE FOR A CENSUS — every count in one table is derived under ONE stated
  predicate with its exclusions named by file and line** (four instances, 2026-09-03). "Followed by
  `(`" as a proxy for "is code" is a predicate of its own and needs its own control: a comment can
  QUOTE a call, so one symbol read 93 under the paren proxy against 95 raw / 91 code / 89 call sites,
  and a "91 actionable" figure was nearly published RIGHT BY ARITHMETIC AND WRONG BY DERIVATION. A
  reconciliation that merely FITS ("subtract the bookkeeping") is refused exactly as a disclosure on
  resemblance is. A name-keyed census read 14 sites in 2 files where the CONSTRAINT-keyed derivation
  read 23 in 4 — two of the missed files banked rows. **A gate is stated as the PROPERTY the emission
  needs ("names a library"), never as a spelling** — a literal `.dylib` gate would have excluded the
  28 framework records the ruling counted IN — and a shipped comment claiming an invariant is
  falsified by the same census and fixed in the SAME cut. **A sizing asserts a population by a
  predicate the emission gates on, names the excluded shapes, and still runs the diff that would have
  caught a wrong assertion** ("windows and linux ZERO by construction" was split by a count: the
  pragma is absent on linux and present 51 times on windows in a DIFFERENT SHAPE) — correct the
  sizing, not the commit. ⚠ And **re-read the CITED LINE's own notation before claiming to falsify
  it**: a "0 of 345" headline measured a proposition the design never asserted (it compared against a
  remembered PARAPHRASE; the design's own notation holds 344 of 345, and what was wrong was the SCOPE).
  ⚠ **A CENSUS NUMBER TRAVELS WITH ITS UNIT, and a reconciliation RE-DERIVES the other instrument's
  number rather than POSITIONING it** (2026-09-05). One lane's 46 — a whole-flavour count of
  address-taken scalar VARIABLES, deduplicated by variable — was placed into another lane's SITE chain
  (27 < 39 < 48 < 61) where it cannot sit; re-derived from the instrument's own file, 10 of the 46 are
  lift-shaped and 0 are `&args` structs, so the operative conclusion survived and the MAPPING did not.
  A number quoted without its unit reads as a member of whatever series it is placed in.
  **⚠ ELIDED-vs-TYPED SIBLING DRIFT: when a helper documents the N renderers that must spell a thing
  ONE way, census all N** (2026-09-04). A converter fix landing on the TYPED renderer of a construct
  never reached its ELIDED twin — the arm that renders the same shape when the literal's type is
  INFERRED — so the elided form kept the pre-fix emission and a keyed sibling kept the same hole: the
  renderer that never got the spelling IS the defect. Its neighbour: **a switch arm that returns only
  for ONE pointee kind lets every other kind FALL OUT to a generic fallback that cannot compile**
  (CS0144 against an abstract box type), so the class is every kind the arm does not name, reached
  through every literal shape that routes an elided element there. Two defects that are two ARMS of
  one switch take controls on SEPARATE assertions when one half's compile failure MASKS the other (a
  third binary carrying only one half isolates it); the fix's own assertion is stronger than "it
  compiles" when the elided spelling emits BYTE-IDENTICALLY to the explicit one; and a residual
  deliberately NOT fixed is recorded at the call site, in the reference doc AND in the guard's
  comment, with its honest fix named as its own item.
  **⚠ A NULL IS A RESULT ONLY AFTER THE INSTRUMENT IS SHOWN TO HAVE FIRED** (three shapes,
  2026-09-03/04). A spike's null at the CALL SITES was an instrument artifact — the DECLARATION had
  never lowered, so nothing had fired — and the real blocker was the pass's own stated SCOPE, which
  neither hypothesis had read: **read a pass's documented scope before predicting what it will do to a
  site.** A COMPILE BLOCKER masks a whole flag-on emission (once the package compiled the census read
  ZERO reduction, 98 = 98, because one predicate pinned the leaves and a chain is pinned by its
  leaves) — so a blocker post is censused across the WHOLE build output, not the file its first error
  names (four errors reported, 99 CS0103 missed in the sibling). And **"the stamp is there" is not
  "the stamp is read"**: after three landed halves a positive control was STILL red with the stamp
  visibly present in the emitted C#, because a FOURTH site discarded it — four sites carried the same
  implicit membership rule and each was widened separately; the remedy is ONE named predicate they all
  call, and the control that finds it asserts the OUTPUT, not the artifact. Its golib twin: **a fix
  can silently do NOTHING** when an accessor materializes a DETACHED COPY of the storage — every write
  lands on a throwaway object and every read misses, with no error anywhere.
  **⚠ A CONTROL WHOSE MODIFICATION CANNOT BE EXHIBITED PROVES NOTHING** (2026-09-04): an LF-anchored
  patch against CRLF source did not apply, the script's own assertion fired, and the run that followed
  read as a PASSED control while testing unmodified code — the census-instrument-that-never-compiled-in
  species, met inside a guard. The corrective is structural, not attentional: **the load-bearing row
  must fail ALONE when its mechanism is removed while every neighbour stays green** (so no other check
  subsumes it), and the restore must be byte-identical. Three mechanical siblings: an APPLIER gated by
  LINE RANGES silently applied 2 of 3 sentinels when the emission shifted — an applier asserts its own
  SITE COUNTS; a rewrite gated on registry MEMBERSHIP assumed every registered method takes the box
  (true of the members it was written from — an assumption, not a derivation), so a gate asks the
  question the emission depends on ("is a receiver box in scope"), and a guard over glyph-prefixed
  identifiers compares WHOLE TOKENS (`Ꮡs.assign(` contains `s.assign(`); an emission FLOOR that is a
  GUESS rejects healthy emissions — derive the population. ⚠ And **a predicted CNR drift is SPENT
  before the run** (re-baseline the predicted project first, then require byte-identical) — a stronger
  gate than predicting the drift and then explaining a dirty verdict; while **a count matching a
  documented failure count is not a matching CAUSE** until the exception itself is read (three
  failures, all in one fixture, all wanting a Windows symlink privilege: count-matched AND
  identity-matched).
  **⚠ VACUOUS ARMS, INSURANCE ARMS AND FIXPOINTS.** A **vacuous TRUE inside an auto arm** is a false
  green that reads as coverage: an arm iterating a descriptor's `Fields` (EMPTY on a synthesized
  descriptor) returned success, so a struct was "assigned to registers" in zero steps and five
  assertions failed from one silent yes. The interim remedy is that **empty-means-cannot-see arms
  become LOUD, never successful** — but ⚠ **an insurance arm is measured against the LEGAL values it
  must pass BEFORE it is written to throw**: `struct{}` legitimately has zero fields and `[0]T` is
  legal Go, so the discriminator is SIZE (nonzero size with zero fields is unseeable; 0/0 is
  `struct{}`), and where no discriminator exists **a half-rule that says why its other half is absent
  beats a whole one that throws on `struct{}`.** ⚠ Before writing a claim about what a model CANNOT
  express, **read the DECLARATION of the field the claim concerns** — a model was asserted unable to
  distinguish unknown from zero while its own declaration said `null = unknown`, and the
  self-correction came from a comment rather than a probe, meaning the inference was the weak link.
  ⚠ **A row's worth is stated at its REACH**: a function reachable only from `export_test` exercises
  its arm only through one test and leaves the sibling arm LATENT, so fixing those rows makes the row
  honest and is not a production behaviour — the record says so in those words rather than letting
  test-only rows borrow a production finding's justification. ⚠ And a **fixpoint over a two-step
  admit/demote rule needs the demotion to be STICKY or the iteration is not monotone**: one
  classification fixpoint OSCILLATED for 18 passes (admit, demote, re-admit) and was caught by the
  monotonicity guard the ruling had asked for. Put a pass-count/oscillation guard in every fixpoint and
  read a firing as a bug, not slow convergence — and note that the prediction ("2–3 passes") missed
  because the cascade was filtered one LAYER earlier than modelled: **predictions name the layer they
  model.** ⚠ Finally, a prediction made from a test's ERROR STRING without reading what FEEDS the
  assertion aimed a check at a branch that cannot fire on any platform, and the pass that arrived was
  VACUOUS — every assertion iterating an empty set. **A vacuous pass is never a match**; where the
  pipeline cannot make the test fail honestly, the per-declaration capability GATE already carried in
  the comparison record lists it with its capability and its lifting condition. (Displacement of a
  generated stub is proven by WRITE-EVIDENCE — the fresh generator output lacks it, the old stub file
  is stale — never by absence.) ⚠ **A PASS THAT CANNOT FAIL IS NOT A PASS, and the honest answer
  names WHICH** (2026-09-06): asked whether a silent degradation showed up in a row, the honest answer
  was not "no" but "this row structurally CANNOT see it" — the two tests that exercise the call are
  capability-GATED, absent from both verdict maps and never compared, so the passing neighbours never
  reach it. Say which, because the next reader will see the green rows and conclude coverage, and
  **record it beside the CAPABILITY, not beside the row.**
  **⚠ ATTRIBUTION AND ARMS, five rules from 2026-09-03.** **A prediction is stated in a currency the
  predictor has MEASURED**: a row-level triple was arithmetic on another lane's accounting and the
  measured decomposition reached neither triple — the MOVED SET (FIXED/BROKEN derived from both
  records under one accounting) is the verdict. **Each arm runs its OWN binary, stated** — the mirror
  of the re-converting-sweep rule: the CUT's converter run against the BASE tree emits placeholders
  with no bodies, a guaranteed red that reads as "the baseline is broken". **A canary row that is NOT
  MEASURABLE on the box by standing ruling is not an A/B instrument there** — red on both arms with
  two different signatures under load says nothing about the cut. **A two-point comparison states its
  WITHIN-ARM spread before it states a direction** (a spread of 90 s and 154 s inside one arm exceeded
  the 145 s arm-versus-control gap, so "faster on both readings" was variance read as merit; the
  verdict was unchanged and only the wording was over-read, which is the cheap kind of correction).
  And **a control for a suite's row keeps the SUITE's configuration** — a stack-walk hand-own's motive
  was mis-attributed by BOTH the lane ("identical at both tierings" — tiering was never the axis) and
  the coordinator ("the call shape, not a lambda" — the shape was the same) while the stack trace named
  the real axis: list the axes (configuration, tiering, shape, tree) and vary each.
  ⚠ **An AGREEING confounded arm is the most dangerous kind, because nothing about the result invites
  the question** (2026-09-06): an arm varied the callee axis while the DOMINANT axis — the caller's
  slot, already proven to pin — stayed fixed, so it could not have informed the question either way,
  and it AGREED with its author's prediction. Scored VOID rather than as a hit, by the author, citing
  their own rule from one day earlier that a control only tests the axis you varied.
  **⚠ WHAT AN ARM HOLDS, AND THE ONE COMMAND THAT ANSWERS IT — five rules from one night, 2026-09-06.**
  **A CONTROL'S BASE DECIDES WHAT IT CAN DISTINGUISH.** A lane proved by SET comparison — name lists
  kept, only-at-mine EMPTY, gone-now EMPTY, the arithmetic closing across three arms with every
  addition accounted for — that neither of its later commits added a single failure, and it was
  exactly right about what it claimed; but its base was its OWN first seat's tip, so the 42 standing
  failures could equally be the corpus's OR that seat's. **"Pre-existing at MY BASE" is not
  "pre-existing at MASTER" when the base already contains the commit under suspicion**, the tell was
  a sibling lane reading a very different failure count on a different tree, and when the question
  widens past a control the base moves with it — the extra arm is one run. **A "PRE-CHANGE" CONTROL
  TREE MUST DIFFER FROM THE TEST TREE ON ONE AXIS, AND A TREE CARRYING SOME OTHER SEAT IS NOT THAT**:
  a package regression was attributed to a train's first seat from four readings whose "without it"
  arm was master plus ONE UNRELATED seat rather than master plus the other fourteen — **a one-axis
  control in appearance and a two-axis comparison in fact** — refuted by BUILDING the alternative, an
  assembly with that seat and its repairs removed reproducing the failure EXACTLY (same 221 empty
  verdicts, same first and last name in the span), so the seat was never the cause. Name what each arm
  holds, and when an arm is "the tree before X", say which OTHER commits it also lacks. **VERIFY EACH
  ARM'S CONTENTS BY ANCESTRY (`git merge-base --is-ancestor <accused> <arm>`), never by the merge
  order you intended**: a branch cut from INSIDE a train's chain drags the whole chain in, so a rung
  labelled "add these three files" added 111 files and 6,666 lines with the accused seat among them,
  and "the cause is mine" was published off it — one command per arm. The tell that forced the check
  was an emission diff coming back EMPTY, and **a result that does not fit the mechanism is a reason
  to re-examine the ARM, not to invent a mechanism.** Then the pair that closes the class: **a banked
  lesson that is not MECHANISED is a lesson that will be paid for again** — the same coordinator
  retracted a two-axis-labelled-one-axis attribution, banked "name what each arm holds", and committed
  the identical error one bisect rung later, because the lesson was written down and the one command
  that applies it was not run. Put the check IN the probe script (the arm prints its own ancestry
  answer before it runs) so applying it costs nothing and skipping it is visible: three lines, and it
  **printed unasked in the very next unrelated run**, on a probe whose author was not thinking about
  it. That is the difference between a lesson in a document and a lesson in a tool.
  **⚠ THE AXIS NOBODY VARIED — six hours of fleet reasoning, and what each correction cost
  (2026-09-06).** **AN ARM THAT MEASURES A SEQUENCE ATTRIBUTES THE WHOLE SEQUENCE'S FAILURE TO ITS
  FIRST STEP**: a table reading array/pointer/func as "caught panic at master" carried the whole
  investigation until its own author built an arm separating the WRITE from the WALK that follows and
  measured that **every write lands and reads back Go's answer on all eight kinds** — the panics
  belonged to the walk. The consequence was total, because a refuse-by-name ruling stood entirely on
  "those kinds are silently writing to the wrong field", which was false, and the accused commit is a
  REGRESSION against measured-correct behaviour rather than a fix exposing a latent fault.
  **Decompose the operation before attributing its failure**, and note who caught it: the instrument's
  author, still testing after everyone else had accepted the reading. Two earlier corrections to that
  same table, each by measurement rather than by care. **An acceptance criterion is derived from a
  measured BASELINE, never from the shape of the failure** — an eight-kind arm found all seven
  reference kinds dying at a seat and published a criterion requiring all seven to become CATCHABLE,
  while the properly measured PRE-SEAT baseline had only THREE failing (catchably) and the other FOUR
  SURVIVING, so building to the published criterion would have made a fix REFUSE four shapes that
  previously worked — and no gate could show it, because the row would still report and the package
  would still pass. **And a criterion derived on ONE HOST is a criterion for that host until a second
  host reads it**: on Linux master all eight kinds SURVIVE where Windows fails three, so "make it fail
  the way it failed before" is per-PLATFORM — Go itself is platform-independent here and we are not —
  and neither the author nor the coordinator who RATIFIED it as *the* acceptance test asked which host
  it came from. **Print the host in the instrument's output.** Beside them: **two failures narrowing
  to one commit are not necessarily the same failure** — a common CAUSE is not a common DEATH — so the
  fix's acceptance reports BOTH readings from the SAME tree (both recover together, or the arm
  recovers and the row does not, the row's cause still inside that commit but not what the arm
  measures), asked BEFORE the fix runs. The mechanical corrective for all of it: **before ruling on a
  defect, name the axis every reading in front of you SHARES, and ask what sits on the axis nobody
  varied.** Here survived / caught / died are all LIVENESS, not one measurement asked whether the
  surviving write produced GO'S ANSWER, and one added assertion settled in minutes what six hours of
  argument could not — it was available the whole time. **Rule at the speed of the EVIDENCE, not the
  speed of the conversation**: five coordinator attributions or rulings in one night, four corrected,
  none wrong for want of care in the argument, and every refutation came from somebody BUILDING the
  thing — an assembly, a baseline run, a real merge — rather than reasoning about it. The build was
  cheaper than the argument.
  **⚠ A FALSIFICATION IS BANKED AS A LIVE ASSERTION, and a finding SURVIVES its mechanism**
  (2026-09-03). A candidate root posted by the coordinator was measured FALSE twice over (the predicate
  never reaches the token; the fallback race produced 0 wrong answers in 200k+ takes under 200k+ forced
  compacting collections), so the FINDING stands — the row dies — while its MECHANISM is retracted the
  moment the falsification lands, the compiled remedy is REVERTED under the non-reproducible-motivating-
  failure rule with its design kept, and the falsification carries a positive control that its premise
  is real (the hash does collide) before its consequence is asserted false. A "found in passing, read
  from code, not measured" item is LABELLED exactly that. ⚠ Its constructive twin: **a finding is
  rooted to a CLASS by repetition with variation, not by hypothesis** — four deaths, two host classes,
  two different racy tests first, one message and one call chain, the death point moving
  286/373/656/2124 s — which establishes "not host-conditional, not test-specific, a race" without
  asserting any mechanism, and a frame-by-frame VERIFICATION post follows the ones that merely CLAIMED
  the chain.
  **⚠ A GUARD'S ROWS ARE ENUMERATED OVER THE AXES ITS PREDICATE READS, not the axes the change
  targets** (measured 2026-09-03, twice on one golib predicate). A fix's identity guard held only slice
  rows with ARRAY elements, so `ISlice<T> : IArray<T>` let a slice element's runtime LENGTH be stamped
  as an array dimension on a map's descriptor and two equal `http.Header` values with different
  insertion order interned as different `reflect.Type`s — `DeepEqual` false on textually identical
  values, three banked verdicts pass→fail at a train head. The same latent predicate deep-cloned a
  named-slice element and threw an `InvalidCastException` in the array-range copy the same week:
  **`ISlice : IArray` makes every "is this an array" test a trap unless it excludes slices
  explicitly.** The fix goes at the PREDICATE's door so no caller can reach the hole again.
  **⚠ ASSERTING AN ARTIFACT'S CONTENT WITHOUT READING IT — four instances on one arc, one of them the
  coordinator's** (2026-09-03): a design section's notation, a shipped comment's claim, a test file's
  existence (the guard a design "proposed" already existed and covered its whole tier), and a
  "dependency on another lane's branch" for a class that exists at master. **The rule already written
  for declarations extends to every artifact a claim is about: read it before the sentence.** Its
  routing twin: a finding relayed from a lane's PROSE ("four guards carry no marker") was routed as a
  cut without re-deriving it from the record — all four already carried the marker and the "red" was a
  ruled criterion working as designed; three derivations plus a positive control, empty branch deleted,
  no SHA, is the correct shape of a stop-and-post. ⚠ And **a check is quoted WITH its counting
  method**: two lanes quoting different numbers for one named check (38 against 51 — parenthesised call
  sites against the bare token) both satisfied the relation the check asserts, so a discrepancy in a
  PASSING check is stated the moment it is seen rather than left to become the next carried figure.
  ⚠ **A COMMENT THAT CLAIMS A BEHAVIOUR THE CODE LACKS READS AS THE CENSUS to the next reader, and a
  faithful PORT propagates the claim** (2026-09-05): the linux exec seam's Foreground-failure path
  SIGKILLs the child and returns without Go's `Wait4`/EINTR loop — one zombie per failed transfer —
  and the darwin twin is identical, because the twin was written from the comment. **A failure path
  that discards a LIVE child owes its reap even when no roster row reaches it**; the roster's silence
  is exactly why it survived review. Routing: the design-of-record's OWNER fixes a LANDED seam as its
  own seat with a guard, while a twin inside an UNANNOUNCED cut is fixed inside that cut.
  ⚠ **Its constructive twin, 2026-09-04:** a hand-owned host that must hand a converted package an
  INSTANCE of an interface it may not reference had a supported answer already in the tree —
  `golib.AdapterBinder.TryCreate` (public, its own header stating exactly that case: a dynamic type
  may live in an assembly converted AFTER the interface's own) builds the duck-typing shell over a
  Go-shaped box whose methods are written on the BOX receiver (the one form the binder takes;
  `ResolveReceiverMethods` skips a by-ref receiver), with the interface type obtained by
  `Type.GetType` — no assembly, no project reference, no dynamic codegen. **`Reflection.Emit` and a
  satellite assembly were cancelled on the tree's own TEXT before either was built.**
  ⚠ **THREE WAYS A GUARD IS GREEN WITHOUT MEASURING ANYTHING, all 2026-09-06.** **A guard can test the
  COMMENT instead of the CONDITION**: all three arms of a pointer-token guard built their box as a heap
  box of a POINTER-to-struct — a reference-bearing POINTEE, which is what the arm's comment describes —
  while the arm's actual condition is "no pinnable storage", which is ALSO true of every field or
  element reference rooted in a reference-bearing CONTAINER whose pointee is reference-FREE. That
  second class was unguarded, and it is the class that regressed every Windows dial. **When a guard's
  arms all match the prose, ask what ELSE satisfies the CODE.** **A guard whose POPULATION IS ZERO has
  "no row moves" as its PREDICTION, not its hedge** — and it is exactly the guard that can be green
  because it is BROKEN: its payoff is a future defect's failure mode, never a moving row, so its
  acceptance criterion is stated in the only direction it can be measured (a row that DID move
  falsifies the population census), and it ships with a TWO-ARMED control — it must FIRE on a planted
  instance of the shape and stay SILENT across every real producer — or it asserts the population
  instead of measuring it. Board debt for such a guard carries the producer TABLE and the control
  requirement, not just the title, so a later lane inherits the measurement rather than trusting it.
  And **when the PRE-FIX behaviour is a REFUSED CALL rather than a crash, a red control asserting a
  fault asserts something FALSE**: under the token arm three of four boxes are order tokens, so the
  control asserts a nil error and a byte count instead. **Put that reasoning in the GUARD's header, not
  only in the design record** — the next reader meets the guard.
  ⚠ **The read-it-before-the-sentence rule met from two more directions, 2026-09-06.** **An
  "unverified" written WITHOUT LOOKING is the same failure as an unread anchor, and it hides better**:
  a design record's section said a registry gate's composition with a per-GOOS body was unverified
  "since every existing member is a flat file", and FIVE of the six members emit per-GOOS with the
  accessibility flip, one is flat, and the exact analogue sits in the same package — **a ruled
  measurement can be satisfied by a READ**, and the production mechanism already working is stronger
  evidence than a synthetic probe (the author's own correction, before the train landed it). And **the
  CORPUS can refute a prediction before it is made**: a row failed to move for a reason its own
  hand-own HEADER already recorded — those rows sit behind a host-killer first, so bodies there move
  nothing measurable. Third instance in one evening of a claim asserted without reading the file it was
  about, and the first where the file was OURS rather than Go's.
  ⚠ **A COORDINATOR'S LEAD IS A HYPOTHESIS, and the coordinator's share of a bad rule is the larger
  one** (2026-09-05/06). A lead is **RETRACTED IN PUBLIC the moment it is measured false, with the real
  site in the same message** — the integer-setter chain a lane was dispatched to was measured SAFE (a
  reference-free pointee gets the pinnable slot, the reinterpret takes the aliasing arm, the new token
  arm never fires) and the option named was not even on that path: **a lane sent to the WRONG file
  loses more time than one sent nowhere.** Before routing at all, **measure a claim's EXTENSION**:
  seventeen alleged allocation entries whose reason asserts pointer semantics were refuted by measuring
  exactly that set — EMPTY — alongside three others, two built to fail if the refuter were wrong, while
  a bucketing that treats the capability CLASS as an allocation family reproduces the reported number
  exactly, seventeen included. **A lane's observation becomes fleet DOCTRINE only after surviving an
  attempt to break it**: a lane reported its mechanism as explicitly UNESTABLISHED and its correlate as
  a place to look, and the coordinator published it as a reading rule with an imperative — because
  **two runs agreeing to three decimals is a SUGGESTIVE NUMBER, not a mechanism** (a hosted-runner leg
  died at 47.017 minutes twice; the THIRD dispatch of the same stage completed in 3.2 minutes with its
  compile genuinely running, and the measurement survived as one open transient while "deterministic,
  reproducible on this stage" did not). Two agreeing runs is not the attempt; the third run was, and
  nobody had run it. The other direction is safe by a recognisable tell: **when a lane asks a
  coordinator to overturn a ruling in the LANE'S OWN FAVOUR, the tell that it is safe is that the lane
  NAMES the asymmetry and hands over ARMS rather than a conclusion — and that the control which would
  have caught a self-serving reading is the one the lane RAN** (the coordinator was right about the
  fact and wrong about the cause, and one one-axis pair settled it).
- **The warm-design trap:** the speculative branch is easiest to write while the design is still warm
  — and twice in one day (2026-09-01) a lane built guard/fix machinery, could not make it FAIL under
  its own control, and deleted it with the measurement recorded in a comment at the site. An
  unexercisable branch in a guard is a false-green seed; deleting it with its evidence is the
  deliverable, not a loss.
  **Its positive twin: a negative result is BANKED — in CODE at the gate, or in the RECORD**
  (both 2026-09-01/02). A measured-wrong next step recorded where the next reader will stand — the
  `MapIndex` follow-up marked *measured wrong: 0 fixed, 1 broken*, in the code at the site it would
  be attempted from — and a commissioned fix **cancelled with its measurement attached** each cost
  one line and save the next lane the whole attempt. The cancellation carries its own rule:
  **a predicate the converter already holds beats a metadata field nothing reads** (the proposed
  flag was dropped because an existing classifier already answers the same question, counts and all).
  ⚠ **Its narrowing form: a guard asserting MORE than its increment delivers is narrowed to the
  DELIVERED reach, and the removed assertion is banked as a NEGATIVE** (2026-09-05) — never left red
  on a landed seat, and never quietly deleted.
  **And a cut whose only demonstrated motivating failure is NON-REPRODUCIBLE is HELD** (2026-09-02,
  an L3 alias cut withdrawn): the mechanism read from the code and the emission actually measured
  disagreed — `mergeExisting=true` at the write sites READ as "preserves a windows alias into a linux
  run", while the merge is seeded per flavour and re-derives the whole imported-alias section — so a
  275-line filter nothing can exercise shipped on a static census with no dynamic measurement.
  **Measure the path once before building on a flag**, and withdraw the predicate with its census
  kept, which is the warm-design rule paid forward.
  ⚠ **A BOARD ROW THAT SURVIVES ITS OWN REFUTATION costs a fleet a night** (2026-09-06): a row dated
  three weeks earlier said a set of profile bodies does not exist; they exist, they are internal, and
  the row is an un-performed push — measured line by line. **A refuted row takes a DATED amendment the
  day it is refuted, naming the file and line that refutes it.** Its twin: **a RETIRED blocker is not a
  banked row** — the tracer row's stated blocker was retired by an equivalence that DOES reach it, and
  what stands behind it is an honest refusal by name, which is a capability EXCLUSION with a proof
  rather than remaining work.
  **And a CORRECT cut with zero measured payoff is WITHDRAWN, not banked on fidelity** (2026-09-02, the
  `math/bits` hand-own over the BCL intrinsics): three nulls in one arc — the RSA-2048 signature moved
  0.1% (64.59 → 64.65 ms, one variable, the after-assembly proven to carry the intrinsics),
  `hash/maphash` −3.7%, the handshake by construction — against sixteen hand-owned functions that are a
  permanent maintenance obligation. The primitives ARE faster per call (Mul64 5.76 → 3.03 ns, OnesCount
  5.18 → 2.91, RotateLeft 4.18 → 2.62, and Add64 TIED at 4.88 → 4.81 because `UInt128` is not lowered
  to `adc`), yet even the fast form is 6.4x Go's 0.47 ns for what is ONE instruction on both sides — so
  the residual is the emission's call/return, tuple-return and value plumbing, a golib/emission
  question no leaf hand-own can reach. Post the PREDICTION before the numbers and note which kind it
  was: the op-count prediction (2–4x) failed publicly, the one made after a measured mechanism held.
  The withdrawal keeps the census, records the nulls in the file's own header, and leaves the chain on
  the board so nobody re-walks the eliminations — and the arc's LATER, narrower cut (word-size
  Mul/Add/Sub plus one inlining attribute, RSA-2048 66.4 → 20.2 ms measured) is the one that banked,
  because it was cut at the seam the nulls located: the emitted body, not the leaf.
  ⚠ **That withdrawal rule is about a LEAF, and a PREREQUISITE with a measured null is a different
  thing** (2026-09-06, the profile linkname push): the rule was written for a leaf optimisation —
  correct, faster per call, three measured nulls, nothing behind it, a permanent maintenance obligation.
  **A cut that moves ZERO verdicts but sits on the measured critical path of a NAMED remaining blocker
  LANDS instead**, because withdrawing it makes whoever takes that blocker redo it first. State the
  distinction at the ruling, or it reads as a precedent for landing null cuts.
  **Where a rule is PLACED decides which cases can reach it** (measured 2026-09-02, both directions):
  the same rule at a HELPER's arms — after the identity arm has already returned — cannot break an
  identity case, while as a CALLER-side gate it runs ahead of identity and did (0 fixed, 1 broken).
  Put a rule where the cases that must not reach it have already returned. Its retirement half: when a
  rule is spelled once at a helper, a caller-side copy is retired only if it duplicated the RULE — a
  copy that enforces ORDER (`SetMapIndex` checking the key BEFORE its nil-map panic, Go's own sequence)
  is load-bearing and STAYS, with the reason at the site, because the helper answers the question but
  cannot express when the caller must ask it; and each retirement carries the row its copy used to
  catch as a positive control. Mechanically: a REBASE leaves `go2cs.exe` stale (route #1) — rebuild
  before re-transpiling a golden.
  ⚠ **An equality rule that answers by REFERENT when both sides resolve and by NUMBER otherwise has
  an `Equals`/`GetHashCode` contract hole in the ASYMMETRIC case** (2026-09-05, the `MapIndex`
  identity rule): one side resolves, the other does not, and the numbers are equal. **Name the hole
  in the code with the reason it is accepted, guard the symmetric cases, and census the population
  that could see it** (`map[unsafe.Pointer]` keys) — **a hash rule is stated WITH its equality rule,
  never after it.**
  ⚠ **And a pointer-identity rule that answers by REFERENT compares ORDER TOKENS — the allocation
  base plus the Go field offset — never box OBJECTS** (2026-09-05, the same arc's root 4): a field or
  element reference is MINTED AFRESH at every `&l.p`, so two boxes over one field are two objects
  carrying one token, and a `ReferenceEquals`-only rule read `fp == unsafe.Pointer(&l.p)` FALSE where
  Go reads true. Equality is `ReferenceEquals` OR equal tokens, and `GetHashCode` hashes the token —
  one fact, stated together, per the rule above. The `reflect` acceptance could not see it; the FULL
  behavioral suite did, at row 8 of `ManagedAtomicPointer` (650/651) — **the cross-assembly consumer
  gate a golib EQUALITY change owes** (route #7's behavioral twin, above).
  **A cut owes its OWN behavioral guard, and an acceptance table for a row with TWO independent
  failures cannot be built from one of them** (measured 2026-09-02). Three outcomes were enumerated
  for a row that carried a second, unrelated failure and none of them admitted the one that happened —
  "the named failure resolves and the other remains"; enumerate outcomes per FAILURE, not per row.
  Borrowing a lane's roster row as a cut's acceptance test also couples the cut's evidence to that
  row's other defects: it cost a 55-minute run on a restarting host and proved nothing about the cut.
  Pin both acceptance directions in a guard the cut owns — including the direction no consumer
  exercises. ⚠ The same arithmetic runs forward: **a row can be gated on TWO independent defects, and
  that is SAID before an arc lands** — otherwise the row "should have moved" and the arc reads as
  having failed. ⚠ And it runs forward once more (2026-09-06): **a row can be BLOCKED TWICE, and
  clearing one blocker is progress even when the other stands.** A profile row's four capability rungs
  — what the runtime can OBSERVE — and its eight cross-assembly-unreachable linkname destinations are
  different blockers on one row, so "leave that row alone, its rungs are a real frontier" does not
  exclude the mechanical half. **State the acceptance PER BLOCKER**: the downstream row may BANK on the
  push, the blocked row's failure mode must MOVE to the rungs, and if it banks instead then the rung
  reading was wrong.
  ⚠ **An acceptance table that enumerates WHERE a dispatch dies must first ask whether the row dies
  BEFORE dispatch** (2026-09-04): an increment predicted to move a guard from a mute exit 138 to a
  speaking failure read byte-identical on one leg — 138 is 128 + SIGBUS, a signal death that never
  reaches the managed throw path, so no downstream fix can make it print (on the other leg the
  prediction HELD, which is the two-leg scoring rule paid forward). **A mute exit code is the ABSENCE
  of evidence**: the stage that keeps whole stderr is where the evidence is, and it is dispatched
  before a mechanism is argued. The lane posted the falsification FIRST, its hypothesis labelled as
  such and the scope of what it had and had not re-read stated — which is what let the second leg's
  correction land the same day.
  **⚠ AN ARC IS WITHDRAWN ON MEASUREMENT EXACTLY AS A COMMIT IS** (2026-09-03/04). Two increments
  measured zero reduction (the second also uncompilable), the wall was NAMED (a chain pinned by methods
  taking aliasing field addresses, which the correctness fix rightly excludes), and the arc's real
  product became the correctness fix plus a DESIGN RECORD of the measured wall — so the next lane
  starts from a wall that was measured rather than a hypothesis, and a performance arc yields the lane
  to the stated objective when it has no acceptance case left. ⚠ **A design record's increment ORDER is
  a prediction like any other**: a "cheapest-looking first" ordering was falsified by READING the
  mechanism before spending a battery — the target was excluded at the SELECTION stage, its body pinned
  an identity-keyed leaf, the selection fixpoint cascaded that wall upward, and the acceptance chain
  crossed a package boundary in the emission: **zero reachable population on both named targets.** The
  increment is RETIRED with its measurement attached, the record's arithmetic corrected in a DATED
  block, and a boundary the record filed as "awaiting a case" is recognised as HAVING its case the
  moment a row's own bank condition depends on it. **A scoping with no case earns no instrument** — the
  census that would narrow a retired scoping is not built. ⚠ Same shape, three more: three would-be
  hardware-free increments were measured OUT OF EXISTENCE (a dormant caller, an empty class after its
  one member was taken, a remedy whose declarations exist only under another GOOS), each cancelled with
  its measurement attached so no lane re-walks them — and **"a guard that can only run on <platform>
  never runs" is true of STANDING gates and understates a DISPATCH**: a dispatch can MEASURE a
  keystone's payoff even though nothing gates on it, so **measurable-but-not-gated is a real position.**
  A stop-and-post against one's OWN sizing record (a section's "0 new markers" falsified by a function
  being bodied) is the record doing its job.
  ⚠ **A SIZING PROBE THAT STOPS AT THE DOOR IT IS SIZING CANNOT PREDICT WHAT LIES BEHIND IT**
  (2026-09-05, two increments in one week). A "+3 rows PASS" prediction was made from a probe that died
  at its first wall, and every row moved exactly ONE door onto walls the probe could not see: **predict
  the next WALL unless the probe ran THROUGH**, and score a prediction against the CUT's stubs rather
  than the probe's — a synthetic `getcallerpc` in the probe made "past `usleep`" true there and false
  in the cut. Its sibling: **a probe that dies BEFORE its first measurement proves only the wall it
  died on** — the prediction it was to score stays a prediction (UNMEASURED, never "met"), the
  increment lands on its build gates with the acceptance stated as OWED to the NAMED increment that
  opens the wall, and the new wall is sized as its OWN door and ordered by what it opens, ahead of a
  correctness increment that moves no row. And the zero-payoff withdrawal rule above bites an objective
  that is a NUMBER: **a DOOR increment on a capability frontier is banked on the DOOR measurement**
  (the site retired on every row, the next wall named), with the failed pass prediction recorded as
  failed.
  **⚠ AN INCREMENT'S ORDER IS A PREDICTION WITH TWO AXES — the reduction AND the FOOTPRINT — and
  "smallest first" reasoned from the reduction alone INVERTS the moment the footprint is measured**
  (2026-09-04, sharpening the ordering rule above). "One box on the os row" was a property of the
  measured ROW, while the RULE the increment needed — publishing a ref primary for the corpus's
  most-used lock — rebinds every boxed call on a ref-addressable base: ~667 sites in 73 files, ~500
  even scoped to the increment's own name. The cut chosen as the cheap first exercise of a new
  contract was the EXPENSIVE one, and the CONTAINED cut went first. **Measure the footprint BEFORE the
  battery, not after.** A large footprint is AFFORDABLE exactly when a mis-bound site fails LOUDLY at
  compile (the multi-target build becomes the load-bearing gate and the predicted path set the
  falsifier); a cut's REDUCTION and its REBIND COUNT are two different numbers, stated separately —
  one is an alloc-row acceptance, the other a count of call sites; and **a bound stated as a bound is
  scored by the MEASURED number AND the measured REASON** (667 became 365 because the binding
  condition — a ref-lvalue base and the specific callee — decides, not the callee count).
  **⚠ WHERE A BOX IS FORMED DECIDES WHICH INCREMENT REMOVES IT, and the prediction is corrected on
  that reading BEFORE the cut** (2026-09-04): of five seam boxes behind one boundary, two are formed
  inside the callee's OWN body (a ref-receiver hand-own of the callee removes them), two at the
  CALLERS' call sites (a converter call-site rule removes them), and one a package away behind a
  promotion cascade — so the hand-own increment's honest deliverable is two boxes PLUS a PRECONDITION
  (the callee made promotable), and the general increment collects the rest for the whole corpus. The
  tempting extension — hand-own the six tiny callers to reach four of five — takes a file from two
  hand-owned functions to eight of eleven: a whole-file hand-own in disguise, declined by the
  minimal-footprint rule because the general rule collects those same boxes across 73 files at once.
  The falsifier for the split is **"any THIRD box moving on the row"**. Two companions: a prediction is
  checked against the PREDICTOR'S OWN exclusion clauses before it is posted (one lane wrote both
  constraints and then predicted across them; the count falsifier fired exactly as written, and the
  miss LOCATED the remaining boxes instead of leaving them unexplained), and **every emission finding a
  prediction did not carry becomes an acceptance ROW** (a minted deref-or-null entry alias must not
  move a nil-receiver panic earlier than Go's; a lock through a ref-returning `.Value` on a raw box
  must contend on the SAME mutex, since a by-value copy-lock compiles and never contends).
  **⚠ "RETIRED FOR NO POPULATION" IS A CLAIM ABOUT A TREE, NEVER A PERMANENT PROPERTY** — the
  ruling-SCOPE rule's twin (2026-09-04): an increment retired on the measurement that a lock's method
  could never take a `ref` receiver was re-opened by the LATER increment that MADE it one and created
  exactly the population the retirement said could not exist. **The increment that changes a
  retirement's premise re-opens it, with a dated amendment rather than a rewrite.** Beside it: **a
  design record's STATED remedy is checked against the LANGUAGE before an increment is cut against
  it** — "the deferred call emitted as a local function taking `ref` to the frame's state" had been
  carried since the record was cut and is not expressible (a ref-capturing local function cannot
  become a delegate, CS8175, and the frame stores a delegate), so the obstruction is the FRAME'S
  STORAGE and the candidate is retired IN the record with that sentence rather than quietly rewritten.
  ⚠ **SEPARATE THE NAMING SURFACE FROM THE IDENTITY SURFACE BEFORE SIZING A TYPE-ERASURE REMEDY**
  (2026-09-05, the `unique` blocker): a descriptor READ FOR ITS NAME and a descriptor USED AS A MAP KEY
  or compared are two populations, and a Stage-B sizing (a call-graph fixed point plus generic-
  signature churn) collapsed to ONE increment once five bare-type-parameter `TypeFor[T]` sites split
  into two naming reads and three identity uses. Threading identity would have changed a PUBLIC
  generic's SIGNATURE — moving banked consumers — AND risked a FALSE GREEN, since a lookup keyed on
  the carrier while the store is keyed on the object finds nothing and returns early through the test's
  own `if !ok { return }`, turning two subtests green for no reason. And **a record's PROJECTED count
  is re-measured before it is quoted**: intervening arcs had already closed the larger half of a
  "7 of 20".
  **⚠ A RECOMMENDATION BUILT ON A SHAPE ARGUMENT IS MEASURED ON ITS POPULATION — busiest shape first**
  (2026-09-03): a per-package registry looked right ("a package declares one `[][N]T` per element
  type") until the stdlib census showed `byte`/`uint8` carrying three lengths, so the heuristic was
  right BY LUCK on exactly the shape most likely to be asked; withdrawn by its author before the
  ruling. Its constructive half: **recover a fact the converter DROPPED at the LAST site where it is
  still statically known**, rather than by observation downstream — and a boundary with no REACHING
  case is RECORDED in the design record, never built ahead of its case. (A "complementary piece" that
  shares the same reach gap is DOMINATED, not complementary; sizing it was the warm-design trap and was
  retracted.) ⚠ **A zero-cost "never worse" heuristic that the durable fix retires the day it lands is
  THROWAWAY and is declined even though it would buy the row back sooner** — the row is carried instead
  as a NAMED known red on the union battery until the durable cut seats.
  **⚠ A PER-VALUE CORPUS-WIDE COST IS WEIGHED AGAINST THE POPULATION IT SERVES, by a second
  derivation** (2026-09-04, the `ж<T>` byte rule's slice sibling): a dims field on `slice<T>`
  (40 → 48 B, +20% on every slice value the corpus holds) was DEMANDED by 130 creation sites and PAID
  FOR by 27,143, and over a zero-byte side table it bought exactly one row class — the nil slice — for
  which no reaching case exists in the corpus or the roster. **"Always right for a caseless row" does
  not outweigh a permanent tax on the most common type**; the ruling took the zero-byte form with the
  boundary RECORDED and the field named as its remedy, and the guard rows kept as *expected-today* rows
  that flip when a case earns the field. This is DISTINCT from declining a heuristic that was right by
  LUCK: the side table is right by construction on every measured site, so the deciding criterion —
  correctness on what exists — is met at zero cost. ⚠ Two mechanics from the same cut. **A
  `ConditionalWeakTable` keyed on a slice's BACKING ARRAY is sound only while empty backings are
  DISTINCT objects** — `new T[0]` allocates fresh (measured), `Array.Empty<T>()` is the shared
  singleton, and `make(x, 0)` is exactly where golib hands out the singleton — so the rule is enforced
  by SUBSTITUTION in the write path, never by an assertion that would turn a legal Go program into a
  runtime throw (the assertion is a test-time guard with a positive control that feeds the singleton,
  plus a census that the emission never spells `Array.Empty` at a creation site).
  ⚠ **The same singleton in a NEW costume, and the rule that generalises it** (2026-09-05): a
  predicate SOUND until a new backing existed becomes a trap the day the backing arrives — `ElemRefBox`
  read `m_array is not null` and took a NATIVE-backed slice's EMPTY managed array as the backing
  (`IndexOutOfRange` on every element), and because `[]` is the shared `Array.Empty<T>()` singleton,
  `Canonical()` would have given EVERY native block in the process ONE identity: the CWT
  pointer-identity hazard again. **Use the type's OWN predicate** (`IsNativeBacked`), **treat a
  new-kind guard going RED on OLD code as the guard's second job**, and SPLIT a live master defect
  found that way into its own seat with its own control rather than riding the increment that found it.
  And **an "expected-today" guard row lives only in a harness that can STATE an expected value
  (GolibTests)** —
  in a stdout-compared behavioral project any row that differs from Go reds the whole project, straight
  into the full-behavioral leg built to catch red projects; the behavioral guard carries the rows that
  must be green plus a documented non-printing block for the boundary.
  **⚠ CANDIDATES ARE RE-SCORED AFTER THE ROUTE IS CHOSEN**, because a route changes the premises the
  others were scored on (2026-09-04): a candidate scored 0 "because the callee receives only the
  primitive, never the containing struct" revived and won once the chosen route displaced the CALLER as
  a hand-own that holds the struct. Two axes from it generalise: **a table with `GetOrAdd` and NO
  removal path is a per-process accumulation defect a redesign should RETIRE, never re-key** (a gate
  that lives and dies with its struct has no table); and **a per-value cost bounded by an EXTERNAL
  population is a different rule from the corpus-wide per-box byte rule** and is stated in its own
  terms. Companions from the same hold: a sizing that omits the LOAD-BEARING step is a HOLD, not a
  footnote; a converter capability serving 16 sites is recorded as a CANDIDATE, not built; and a "twin"
  fix with zero production population is dropped as a reduction and named as uniformity work. ⚠ And
  **before dissolving a wall, measure what the wall is MADE OF**: an identity boundary can be a property
  of the PORT's representation rather than of the source semantics — a table keyed on a box because the
  box carried "same field of same object" dissolved once the underlying word was measured to be DEAD
  STORAGE in the port, free to BECOME the identity as a lazily CAS-assigned handle. That is measured by
  the falsifier that would retire it (any read of the word as a VALUE, anywhere in source or corpus)
  and cut with a concurrency guard proven RED first, its one semantic divergence stated in the record.
  **⚠ THE ORDER OF A FIX IS LOAD-BEARING, and a fix at the wrong layer reads as "the fix does not
  work"** (2026-09-03): a root moved from the renderer to the container CONSTRUCTOR, and a renderer
  fixed FIRST threads a null, changes nothing observable, and sends the next reader into the layer that
  is already correct — populate first, then thread. It moved a THIRD time, into a DELIBERATE, documented
  REFUSAL, which is why **you read the site's own comment before editing the line beneath it**: an
  increment that skipped that step would have passed a nine-shape guard and broken a banked consumer's
  IDENTITY that nothing in the gate list measures. Three companions. **A guard that prints NAMES must
  also assert IDENTITY where identity is the contract.** **A model fix is checked for its EMISSION TWIN
  before it is scoped as golib-only** — the same element positions the runtime model dropped were also
  unwalked by the converter's own stamping pass, and the twin's gates (converter suite, two-seeded diff
  by hunk, CNR with predicted golden drift) join the increment. And an increment can be **three halves
  in a FORCED ORDER**, any one or two of which produce nothing observable — so **re-reason a tested
  decision line by line** rather than deleting a line that was right when written (an existing row
  asserting "slice of arrays emits nothing" was DELIBERATE and correct under the old accessor). ⚠ **A
  consumer's exposure is READ from its first substantive line, never guessed from its name** — a "probably
  not a consumer" guess was wrong twice in one arc.
  **⚠ READ THE FLAG-OFF EMISSION OF THE SAME SITE before cutting a fix at a call-site ARM**
  (2026-09-03): a classification pass had demoted a local's box by weighing its RECEIVER-use and missing
  its RESULT-use, and the arm-level fix compiled while making the site WORSE than flag-off (7 boxes
  against 1). **A wrong classification is corrected where it is MADE, never papered at the arm**, and
  the rejected form is written at the site as measured-wrong-by-reading. ⚠ Two neighbours: **a
  predicted-then-confirmed baseline is still MEASURED once**, because it is the before-arm of the next
  increment's delta; and **a "perf" item is re-measured for CORRECTNESS against `go run` before it is
  priced** — a backlog item filed as allocation hygiene was a Go-SEMANTICS divergence (`for i, v := range a`
  over an array VALUE observed the body's own writes, seven of seven shapes), which is why no sweep
  caught it: the copy belongs to the range EXPRESSION where `gc` puts it, scoped exactly as `gc` scopes
  it, and that is what then lets the enumerator be cheap. ⚠ And **a three-row filtered acceptance cannot
  falsify "no OTHER row moved"** — say so, and let the union gate carry it.
  **⚠ WHEN SIZING A CORPUS-WIDE BEHAVIOUR CHANGE, PICK THE DEFAULT THAT MAKES THE ARC MONOTONIC**
  (2026-09-06) — the default changes the arc's SHAPE, not just its safety. Default-FATAL for
  unimplemented stubs (list the CAPABILITY ones, leave everything else exactly as today) means every
  increment can only convert a host death into a reported verdict and never the reverse: no
  full-roster blast-radius gate before the first landing, landable one package at a time with a
  per-increment row measurement, and safe to stop between any two — where the opposite default would
  have shipped 24 unclassified stubs with a kind nobody decided. **A sizing whose author picks the
  harder default against their own convenience is one that does not need second-guessing.** Two rules
  ride with it. **PUT THE DECISION WHERE THE KNOWLEDGE IS**: a stub's kind (capability versus
  memory-moving / address-returning / atomic) is SEMANTIC and a structural predicate provably cannot
  recover it — measured, not argued, since the unsafe thirteen return void, bool and a pointer so no
  return-type rule spans them, while "takes an unsafe pointer" sweeps in the 140 capability stubs that
  must stay recoverable — and a curated symbol table inside a Roslyn analyzer is the OTHER wrong home:
  **the converter knows what the symbol IS, the generator knows only what it LOOKS LIKE, so the
  converter stamps an attribute and the generator reads it.** And **for a change that makes failures
  RECOVERABLE the acceptance criterion is the FALSE-GREEN direction, named**: a test that stops dying
  and starts FAILING is the point, while a test that stops dying and starts PASSING may be passing on
  a RECOVERED missing capability — the only way such an arc can do damage — so each increment's
  measurement rules that out explicitly rather than reporting a net verdict improvement.
- **⚠ A merge that touches `package_info.cs` must carry the matching `stdlib-metadata.txt` change —
  check it in the PREFLIGHT.** `stdlib-metadata.txt` is generated FROM the corpus (`go generate .` in
  `src/go2cs`, gated by `TestStdLibMetadataInSync` under the converter's own `go test`), and a corpus
  bank that moves `GoImplement` records without it leaves that guard red for whoever runs the
  converter suite next. Three banked regens missed it in two days (2026-08-24/25) — the step was
  documented and still skipped, because no MERGE checked for it: if
  `git diff --name-only <base>..<branch>` lists a `package_info.cs` but no `stdlib-metadata.txt`,
  stop and have the branch run the generate before it merges.
- **⚠ Two branches writing the SAME wrong number auto-merge CLEANLY.** The roster's header is the
  measured case (2026-08-29, the banking window): master and an incoming bank both moved the row
  count 189 → 190 — identical text on both sides, so git folded them without a conflict while the
  union's truth was 191. The silent-duplication rule's arithmetic twin: at any multi-branch window,
  header/summary numbers are RECOMPOSED from the merged table, never accepted from either side, and
  the format guard (guard-as-calculator) runs after EVERY resolution — it caught this one and a
  hand-composed Linux-denominator slip the same evening.
  ⚠ **A COUNT IS NOT A SET, AND A COPIED NUMBER IS NOT A DERIVED ONE — three readings of one roster,
  2026-09-06.** **QUOTE THE SET, NEVER THE COUNT**: an `os` row read 685 = 683 + 1 + 1, its board
  record 683 = 681 + 1 + 1, and a summary 682 of 686 counting the capability-GATED rows against the
  total — three internally consistent compositions over ONE failure set, so a dispatch quoting a count
  ("four failing verdicts") can be wrong while every number it came from is right. The invariant
  across compositions is the SET. **A TRACKER THAT COPIES A DERIVED NUMBER GOES STALE SILENTLY, AND IN
  THE FLATTERING DIRECTION** — the coordinator's remaining-rows list named a row unowned that had
  banked four days earlier, and a probe spent on it measured nothing but the bookkeeping. ⚠ And the
  obvious remedy is itself a trap, which is the correction worth carrying: **read the
  guard-recomputed HEADER, or count the table rows — never the roster's PROSE derivation.** A
  document's derivation and its computed figure look alike on the page and go stale differently: the
  roster carries a DATED prose derivation ("202 banked, eight remaining", correct on the day it was
  written) beside a header the format guard recomputes from the table on every change (203/210), and
  a coordinator who wrote "do not carry a list, re-read the roster's derived section" then published
  the stale number TWICE — in the post whose own point was that stale counts are dangerous. The prose
  explains HOW a number was reached and is a record of one day's reasoning. Its constructive note:
  every candidate queued for an exclusion ruling that has actually reached a MEASUREMENT has come back
  implementable, three for three, which is the argument for measuring the remainder rather than
  reasoning about it.
- **A liveness/health probe must be able to OBSERVE the thing it asks about** (2026-08-29, the iter
  lane): a process filter on the worktree path can never match `dotnet.exe` running from Program
  Files, so a healthy 18-minute build read as reaped and was reported as owed. Silence is not
  evidence of death any more than exit 0 is evidence of success — the rule cuts both directions:
  read the output, and first verify the check CAN see its target (positive-control the probe the
  way gates are positive-controlled). ⚠ Met again 2026-09-04 in its simplest form: a probe looking for
  `go2cs`/`dotnet` while the suite runs as `BehavioralRunner.exe` reported a healthy run as dead —
  **name the process you are actually waiting on.**
  ⚠ **"Armed" is a claim about a task verifiably STILL RUNNING** (2026-09-02): a task id that has
  EXITED is evidence of a PAST arming, and a lane went silent for hours with BOTH legs down — its
  exit-on-change watcher had fired on the lane's own post and was never re-armed, while the backstop
  that exists to catch exactly that first failure was itself gone. A protocol step that must be
  remembered at the end of the busiest turn, and whose failure is silent, fails on a schedule: DELETE
  the step (a persistent monitor needs no re-arm on a local lane; on the cloud-container class it is
  hard-capped at ~30 min, so there the relaunch leg is load-bearing) rather than reminding harder, and
  back it with a leg that verifies LIVENESS, not existence, and checks its own existence on every
  firing. Its reading
  half: a filter built from expectations can be simply where you stopped reading — read every numbered
  item of a post addressed to you, and read anchor..tip before starting the next one.
- **Positive-control the DETECTOR, not just the gate** (2026-08-30, the pinning census guard): a
  BOM-less `.ps1` under Windows PowerShell 5.1 mis-reads non-ASCII literals through the system
  codepage, so a guard's `ᴋ`-matching regex was silently broken and its "0 findings" red was
  accidentally right for the wrong reason. A new false-signal species: a red whose detector is
  dead. Any regex-bearing guard on PS 5.1 gets a BOM if it carries non-ASCII, and gets its
  detection deliberately regressed once before its verdicts are believed.
  ⚠ Same species one layer up (2026-09-02, met independently by two lanes): a checker printed
  **PARSES CLEAN** while its own `[ref]` binding had thrown on an undeclared variable — the `else`
  branch prints clean regardless. Declare a checker's ref targets, and run it once against a
  deliberately BROKEN copy before believing any "clean".
- **GC/liveness probes: ONE ARM PER PROCESS** (2026-08-30, the StringData lane): running probe
  arms back-to-back contaminates them — an in-frame arm's object collects as soon as a LATER arm
  clobbers the frame, so only the last arm's reading is honest; three arms flipped verdicts on
  run order before isolation. Same family as the tier-0 finding: what the frame holds decides
  what collects, so each measurement gets a fresh process.
  ⚠ **A FITTING story is not a root — and this family's most convincing one was measured FALSE**
  (2026-09-02). A non-optimizing JIT roots every local for its method's life, so a test looping
  `runtime.GC()` for finalizers cannot see them become due at Debug; the mechanism is real,
  `mfinal.cs`'s own comment predicted it, and it fit `TestSplicePipePool`'s symptom perfectly — total,
  permanent, immune to repeated GC. One one-axis run killed it: `internal/poll` at Release+TC0 fails
  IDENTICALLY to Debug (zero rows moved, identical fd set, 2.6 s across a 54-minute window). Four
  candidates are measured out now — SetFinalizer keying, `sync.Pool` aging, the `runtime.GC` sequence,
  the JIT tier — and after four the next step is an INSTRUMENT (a heap root-path read), never a fifth
  hypothesis. Prediction-on-record is what made that run decisive, and what makes a falsification
  cheap.
  ⚠ **The instrument arrived, and it named a GC-LIVENESS DIVERGENCE CLASS** (2026-09-03, rooted
  verbatim with `dotnet-dump gcroot`): `TestSplicePipePool`'s 64 pipe boxes were rooted from **three
  slots of the test's OWN frame** — slice-header copies made at `append` and at the range loop, with
  the pool chains empty and the finalizers unreached. Go's precise stack maps report those copies dead
  after the loop; the CLR reports untracked struct locals live for the whole method, **at every
  configuration**. That is the `sync` `TestOnceXGC` class: disclose by SIGNATURE with the mechanism,
  after checking whether any slot is a converter-minted local a `= default` after the loop could null.
  ⚠ The addendum is what makes it a class rather than an emission bug: the three roots are
  append-result and JIT-spill copies with NO source-level name, and removing the one EMISSION-level
  copy (the range enumeration, replaced by an index loop, dll verified newer than the patched source)
  left 64 boxes / 67 sentinels UNCHANGED — the emission remedy FALSIFIED, not merely unchosen.
  **"Right about WHERE, wrong about WHY" is still a wrong story until the arm runs.**
  ⚠ **The ONE-AXIS PAIR is what earns the MECHANISM sentence, and three 2026-09-06 readings say what
  it bought.** Retained with the frame slot LIVE, collected with it OVERWRITTEN, at the configuration of
  record — so the pin is the caller's FRAME SLOT and the by-value hand-off adds none, a fourth arm
  tracking the overwrite arm to the letter proving the second half. Had both arms read retained, a
  sentence copied from the neighbouring disclosed family would have been WRONG, which is exactly what
  the pair existed to prevent, and it cost one process. **Optimization honours an OVERWRITTEN slot and
  does not rescue a LIVE one**: measured across three configurations, a slot that is merely DEAD — still
  in scope, never read again — is freed at NONE of them, while an overwritten slot is freed only at the
  optimizing configuration; that is the mechanism under the claim that a family of rows joins once
  conservative liveness is optimized away. And **a SOURCE-LEVEL exoneration is PROVISIONAL until a
  one-axis arm carries it**: the coordinator exonerated an intern path by reading the code, the lane
  treated that as provisional and added an arm — the same body plus the real call, the handle kept alive
  so the map is genuinely live — which read IDENTICAL to its reference in all four columns. Six readings
  then partitioned into two families with nothing left over, every candidate but the frame slot
  eliminated by a control differing in exactly one axis. **A source reading and a live measurement are
  different evidence, and the gap between them is where an exoneration hides a real retention if the
  code has drifted.**
- **The `-tests` graph invariant (ruled 2026-08-30, from the W1 arc):** a `-tests` conversion's
  production emission may differ from `-stdlib`'s only in ways that do NOT change the project
  GRAPH. The documented closure families all change file text and no reference; the
  `canUseLongPaths` csproj flip was the first edge-mover and it was fatal (6 cycles), which is
  the boundary's proof. Mechanical form: `check-solution-integrity.ps1`'s per-GOOS cycle
  assertion (G2), whose positive control injects the historical edge and requires exactly the
  six named cycles.
  ⚠ **MEASURE THE PRECONDITION EVEN WHEN THE ANSWER IS THE ONE THE SYSTEM ALREADY IMPLEMENTS**
  (2026-09-06): a profile push's graph invariant came back **38/36/36 cycles for the direction Go's
  directive names and 0 for the inverse** — a shape the converter would never emit — so the design was
  unchanged in OUTCOME and would have been ASSUMING ITS OWN ANSWER without the run. Go's own directive
  ARITY then split the eight destinations one-to-one onto two existing registries (a one-argument
  handle authorizing a PULL, the two-argument form PUSHING), which came from reading Go's text rather
  than designing around it.

## Git anchors

| Commit | Date | Meaning |
|---|---|---|
| `9792eeea2` | 2020-07-09 | Original hand-converted stub created (`src/gocore`). |
| `ba6fef6c9` | 2025-03-08 | Renamed `src/gocore` → `src/core`. |
| `3426298eb` | 2025-05-05 01:51 | Last clean stub baseline — **restored into `src/core`** on 2026-06-25. |
| `6ca1c45b7` | 2025-05-05 01:59 | First full stdlib conversion — overwrote the baseline. |
| `cc14584c7` | 2025-05-11 | Full-conversion work; tagged `full-conversion-2025-05`. |
| `3c8b3a848` | 2026-06-25 | Separation + stub-baseline restore + converter fixes → green baseline. |
| `05a53e8c0` | 2026-06-26 | First full-conversion package promoted — `sync/atomic` into `core`. |
| `914d4bd72` | 2026-06-27 | `math` compiles clean (tag `math-green-2026-06-27`). |
| `51ba5d9cf` | 2026-07-10 | **First clean full-standard-library compile** — all 302 converted packages (then at `src/go-src-converted`; tag `stdlib-green-2026-07-10`); Phase-3 milestone. |
| `337a928df` | 2026-07-17 | **First real Go test suite validated in C#** — `unicode/utf8` 14/14 vs `go test -json` through the Phase-4 `-tests` pipeline (tag `utf8-tests-green-2026-07-17`); §12.8 opened. |
| `f999c8f78` | 2026-07-18 | **Second validated package** — `sort` 63/63 vs `go test` (tag `sort-tests-green-2026-07-18`); first with real algorithmic depth (interface-driven sort, `sort.Slice` reflection, NaN ordering). |
| `40f39d2be` | 2026-07-18 | **Packages #3 and #4 validate** — `bytes` 81, `strings` 68 (tag `bytes-strings-tests-green-2026-07-18`), via the hand-owned signature-pinned **disclosed-divergence manifest** (`go2cs_test_disclosures.json`) for the alloc-count asserts the managed CLR provably cannot satisfy. |
| `2e8066da6` | 2026-08-01 | **The two trees become one** — the stub baseline retires and the converted stdlib moves to `src/core`; every rewrite/remap path is deleted, `testing` joins `unsafe` as hand-owned, the generated `go2cs-stdlib.slnx` becomes adoptable verbatim. |
| `f6e9c0cf0` | 2026-08-04 | **The whole-corpus rebank** (r40) — the one deliberate regeneration that levels the accumulated intended drift of every converter arc that landed without its corpus regen: 1,316 files across sixteen named families, zero unclassified. `bf1458b5d` banks the matching test-source + proof-page refresh behind a 73/73 sweep. The `GoUntyped` alias becomes `GoBigConst` in `4d71935ff` on the way in. |
| `10c78227a` | 2026-08-22 | **Over 75% of the testable stdlib validates** — 162/215 packages, 18,569 matching verdicts, 85 disclosed (tag `stdlib-tests-75pct-2026-08-22`); Go 1.23.1's TERMINAL validation marker — `release/go1.23` cut, the campaign continues on 1.23.12. |
| `4f0fd0b5c` | 2026-08-23 | **The anchor NuGet release** — the 1.23.1 corpus publishes as **`1.23.1.7`** (tag `nuget-1.23.1.7`), the over-75% roster's pre-hop .NET 9 anchor, Windows + Linux in one combined story; `docs/validation/1.23.1.7/` freezes the 162 proof pages the packed badges link. |
| `925e48067` | 2026-08-24 | **Master moves to .NET 10** -- Stage 2 of the framework hop merges: 955 project files to net10.0 with zero corpus-emission drift, three OS flavors green, carrying the C#14 params-flip converter fix the hop itself exposed (under C#13 the corpus's variadic-slice binding was correct *by accident*). |
| `a2e079259` | 2026-08-25 | **The roster re-banks at Go 1.23.12** -- the corpus hop completes: 162/162 rows re-validated from the new release's own test sources, **18,598** matching verdicts (+29 = exactly the four re-derived rows), three machines' shards reconciling to the digit. With 925e48067 two days earlier, both runtime pins (net10.0, go1.23.12) moved in one campaign, each through a runbook that led. |
