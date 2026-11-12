# PLAN: public-surface stability before outside use

Status: **RULED, 2026-11-11 (owner), all five OQs as recommended.** See the ruling block at the end.

## Why this plan exists

The converted TypeScript compiler now type-checks matching native, and the project is close to a soft
release. Outside C# consumers will bind to what go2cs emits as soon as it is visible. The owner (2026-11-11,
typos fixed): *"Let's come up with a recommendation and get these types of issues on a more solid foundation.
A scout that could enumerate other issues of this nature that could produce breaking future changes that a
decision now could alleviate would be a good idea. Let's steady the ship for a long haul."*

A break is cheap now and expensive later. This plan lists every place where the consumer-visible surface is
not yet settled and a plausible future change would break consumers, recommends a decision for each, and
proposes a standing mechanism so the surface stays settled.

Source: a read-only scout against `origin/master` 742a3dbb24. Counts are `git grep` counts at that commit and
state what they counted. Items marked *not verified* were read, not measured. The M:N scheduler is being
spiked separately (G, R5b, owner-ordered the same day) and is listed here only for its surface.

## The decisions (open questions for the owner)

### OQ1. Go `internal` packages are fully public to .NET

**Today.**
- An exported Go name becomes `public` whatever its path (ConversionStrategies.md, naming).
- Of the non-test projects under `src/core`, 170 of 362 sit on an `internal/` path and 17 on `vendor/`.
- Each ships as its own `go.internal.*` package (`go.$(AssemblyName)`).

**The break.**
- Enforcing Go's path rule later removes access consumers have.
- Go itself changes internal packages freely between minors, so a consumer bound to one breaks at each hop anyway.

**Options.**
1. C# `internal` plus `InternalsVisibleTo` per allowed importer.
   - Internal types appear in public signatures of public packages (CS0050).
   - The grant lists are large.
   - Unsigned simple-name grants are spoofable.
   - It is a large converter change.
2. **Stay public, mark them now, and warn now.**
   - An assembly attribute carries the internal root path.
   - The package class gets `[EditorBrowsable(Never)]`.
   - An analyzer warning (a new `GO2CS0xxx`) fires when the referencing assembly's Go path is outside that root, or when it is hand-written C#.
3. Run-time type-loader interception: .NET has no hook for statically bound member access. Rejected.
4. Unlist `go.internal.*` on nuget.org. It can be combined with option 2; unlisted packages still restore as dependencies.

**Recommendation: option 2 now, with documented wording** "not covered by any compatibility promise; the
warning may become an error". Later enforcement is then an announced tightening, not a surprise. Add option 4
if the publishing workflow allows. *Not verified:* whether a pure C# consumer of `go.*` receives the go2cs-gen
analyzer. If not, the warning needs another carrier, such as a go.lib build target.

### OQ2. Δ collision renames make exported names unstable across Go releases

**Today.**
- `performNameCollisionAnalysis` (src/go2cs/nameCollisionAnalysisOperations.go:43) renames a package-level const, var or type whose name equals any method name in the package. Example: `time.ΔMonth`.
- Consumer docs already show `time.ΔSecond` and `uuid.ΔVersion`.
- There are 161 distinct `Δ` type declarations in `src/core` outside golib; 114 have exported names, 63 of them outside `internal`, `vendor` and `cmd`.

**The break.**
- A Go minor that adds a method `X` anywhere in a package renames an existing exported `X` to `ΔX`. That is a source and binary break Go's own compatibility promise never predicts.
- *Not measured:* whether the rename set can differ per GOOS flavour.

**Options.**
1. **Move methods out of the package class into a sibling holder class**, as the .4 promoted-forwarder move did (DESIGN-promoted-forwarder-placement.md §2). Types and constants keep their Go names, which is the best readability and parity outcome. It costs one large break now.
2. Keep the renames and document `Δ` names as unstable across Go minors.
3. A committed rename ledger. It cannot stop a new method forcing a rename, so it is not durable.

**Recommendation: scout option 1 now** (its footprint and the visual-parity cost of `using static pkg` no
longer bringing methods) and decide on the numbers. If it is rejected, take option 2 and document it.

### OQ3. API versus implementation detail in golib and go2cs-gen output

**Today.**
- golib has about 305 public type declarations, with `EditorBrowsable` in 11 files.
- The go2cs-gen templates carry none.
- Generated members consumers can see include:
  - adapters (`XжI`, `XᴠI`);
  - `{pkg}_package_asserts`;
  - `…ᴛxpkg` holder classes;
  - field-access token holders;
  - `ʗp` box forwarders;
  - `Ꮡfield` properties.
- go.lib injects `global using go;` and `using static go.builtin`, so every new public golib name can collide with consumer identifiers.

**Recommendation.** Adopt a written three-tier contract now:
- **Tier 1, frozen.** The Go-declared exported surface of non-internal packages, the type mapping (OQ5), golib's consumer API, the exception types, package IDs and the version scheme.
- **Tier 2, binary-stable within a Go minor; may change only at a break-floor move.** Generated plumbing and golib's emitted-code API. Mark it `[EditorBrowsable(Never)]`.
- **Tier 3, no promise.** Go `internal` and `vendor` packages, glyph-named temporaries, and generator output names in `obj/`.

New non-Go golib names go in `go.golib`, never in `go` or `builtin`.

### OQ4. Assembly identity

**Today.** Package `go.fmt` ships `fmt.dll`; likewise `runtime.dll`, `net.dll`, `log.dll`, `testing.dll`.
They are unsigned, and friend grants are by simple name.

**The break.** Renaming later breaks every compiled reference.

**Recommendation: decide now.** Either rename the assemblies to match their package ID (`go.fmt.dll`) before
outside use, or freeze today's names explicitly. COORD leans to renaming: simple names like `net.dll` and
`log.dll` are the ones most likely to clash in a consumer's bin folder. Strong-name signing is a separate,
later question. State it as undecided rather than imply a promise.

### OQ5. Go interfaces that consumers implement in C#

**Today.** The consumer doc teaches `class CollectingWriter : io.Writer`.

**The break.** Any member go2cs later adds to a converted Go interface breaks those classes. M:N's hidden
twins (option O6) would do exactly that, and G's spike is measuring whether a default interface method avoids
it under JIT and Native AOT.

**Recommendation: rule now** that a generated addition to a Go interface is either default-implemented or lives
on a separate hidden interface, never as an abstract member on the Go-named interface.

### Smaller decisions, recommended as "freeze and document"

- **Type mapping** is frozen and listed as "will not change":
  - `int` maps to `nint`, `rune` to `int`, `string` to `@string`;
  - multiple results become named tuples, pointer methods take `ж<T>`/`ref T`, and exported value vars become ref properties.
  - Tuple element NAMES are declared non-contract: a Go result rename changes them, which is a source-only change.
- **Panics:** the `PanicException`, `GoErrorException` and `GoexitException` names and properties are frozen. Decide whether `GoexitException` is catchable API or plumbing; COORD recommends plumbing, tier 2.
- **Diagnostic codes, flags and MSBuild properties:**
  - codes are never reused, a precedent GO2CS2004 already set;
  - severities only tighten, at a release with a NEWS entry;
  - flag defaults stay stable within a Go minor;
  - the consumer MSBuild properties are published as a list: GoConsumerUsings, GoGlobalPackageAliases, GoAllowBelowBreakFloor, GoTargetOS and the rest.
- **Threading observable from C#** is declared non-contract for thread identity: which OS thread runs a goroutine, and whether code after a blocking call resumes on the same thread. M:N will change that under either path (G's spike, P3), and `LockOSThread` stays the documented opt-out.
- **Reflection-visible go2cs attributes** (`GoImplement`, `GoRecv`, `GoPromoted`, `GoType` and the rest) are tier 2. `go2cs/source-metadata.txt`, which the break floor reads, is tier 1 as a file format.
- **Per-RID surface:** `lib/` is the Windows build, and the surface differs per RID in syscall, runtime and os. Consumers compile against the Windows surface. Document it; no change proposed.

## The standing mechanism

1. **A visitor-facing contract page**, `docs/PublicSurface.md`, in present tense and linked from ConsumingGoFromCSharp.md. It states the three tiers and the frozen lists above.
2. **Guards.**
   - **Existing:** ApiCompat (`src/tools/pushnuget/GoApiCompat.psm1`) already covers the binary surface of `go.*`, `go.lib` and `nugetgo.*`, generated members included.
   - **Source-compatibility gate:** build the previous release's `src/tests/CSharpConsumer` unchanged against the new packages.
   - **golib tiering gate:** a public-API baseline file, plus a test that every public golib type is either listed or carries `[EditorBrowsable(Never)]`.
   - **Δ-name census:** diffed at each hop, so a rename is seen before it ships. Needed only if OQ2 takes option 2.
   - **Hop report:** the public members each Go minor removes or renames.
   - **Already guarded:** `TestGo2csDiagnosticCodeRegistry` covers the diagnostic codes.
3. **Deprecation policy.**
   - Where an old member can coexist with its replacement, it carries `[Obsolete]` for one release.
   - Otherwise a break takes the full set together: a break-floor move, a compatibility-suppression entry, a NEWS "Upgrading" section, and the nugetgo rebuild wave. That set is the only allowed path for a break.
4. **Homes.**
   - Policy: the contract page.
   - Procedure: the runbook (docs/GoCorpusMigration.md).
   - Rulings: this plan.

## Proposed order

1. **Owner rulings on OQ1 to OQ5:** they gate everything else.
2. **Before the soft release:**
   - the contract page;
   - the OQ1 marking and analyzer;
   - the OQ3 `[EditorBrowsable(Never)]` pass and golib baseline;
   - the OQ4 decision;
   - the OQ5 rule.
   Each lands as its own seat.
3. **OQ2:** the scout first, then a ruling. A holder-class move would be the largest break, so it is best taken while no outside code exists.
4. **M:N:** its consumer-visible list from G's spike folds into this plan when it lands.

## RULING, 2026-11-11 (owner)

The owner, verbatim (typos fixed): *"The recommendations are sound and reasonable. Go makes several assertions
like these as public language doctrine, this feels similar and appropriate. Let's make good on these now, as
much 'break' handled now as possible with a strong set of rules to lean on."*

All five OQs are RULED as recommended, and the "freeze and document" items with them:
- **OQ1:** Go `internal` and `vendor` packages stay public to .NET, are marked (an assembly attribute with the internal root, `[EditorBrowsable(Never)]` on the package class) and warned on by an analyzer now, documented as having no compatibility promise. Unlisting is to be checked against the publishing workflow.
- **OQ2:** the methods-to-holder-class move is TAKEN now, so exported Go names never take a `Δ` because of a method. Scout first for the footprint and the exact shape, then the move. A large break, deliberately taken before outside use.
- **OQ3:** the three-tier contract is adopted. Generated plumbing and golib's emitted-code API carry `[EditorBrowsable(Never)]`; new non-Go golib names go in `go.golib`; a golib public-API baseline file plus its test.
- **OQ4:** assemblies are RENAMED to match their package ID (`go.fmt.dll`), before outside use. COORD's lean is adopted as the ruling.
- **OQ5:** a generated addition to a Go interface is default-implemented or lives on a separate hidden interface, never an abstract member of the Go-named interface. This binds the M:N design.
- The standing mechanism (the PublicSurface.md contract page, a source-compatibility gate, the golib baseline, the hop report, the single deprecation path) is adopted.

## Seats (COORD, 2026-11-11)

| Item | Owner | Order |
|---|---|---|
| OQ3 contract page + EditorBrowsable pass on generator templates + golib baseline and test + the freeze-and-document items | C1, after its xxh3 fork | first: every other seat cites the page |
| OQ1 marking + analyzer warning (a new GO2CS0xxx) + unlisting check | C2, after C47 | after the page |
| OQ5 rule + its guard (a generator test that no Go-named interface gains an abstract member) | C1, inside the OQ3 seat | with the page |
| OQ4 assembly rename (AssemblyName, ProjectReferences, friend grants, nugetgo, the consumer test) | R, after -warn and the -recurse skip | its own batch |
| OQ2 scout, then the holder-class move | P1, after C48 | scout first; the move is its own batch |
| M:N's consumer-visible list folds in here | G (R5b spike, B6) | when the spike reports |

The two large emission changes (OQ2, OQ4) each land as their own batch, never stacked with other converter
seats, and each is measured by the two-seeded four-target reconvert, with its CNR and golden moves predicted.

## M:N: the consumer-visible list, folded in (G, 2026-11-12)

This block answers the last row of the Seats table and item 4 of the proposed order. Source: the R5b spike's
record, [DATA-mn-spike-2](phase4/DATA-mn-spike-2.md), section 8.5, which holds every row with how it is known.
Here the rows are sorted by this plan's tiers. The path is R2 (hidden async twins), the ruled one. Nothing of the
spike's code lands; this is what a landing form would bring, so that the contract page can say it first.

**Tier 1 holds under R2. Nothing frozen changes.**
- Exported Go functions and methods keep their signatures. The spike's converter diff is 0 files.
- A Go-named interface gains no member (OQ5, kept): the twin lives on a separate hidden nested interface. A
  hand-written C# implementer compiles and runs unchanged, as class and as struct, under the JIT and under
  Native AOT.
- The type mapping, golib's channel API and the three exception types do not change.
- A synchronous call from a plain C# thread behaves as today: it runs the visible body and blocks at a park.
- `defer`, `recover`, `Goexit` and the process exit on an unrecovered panic print Go's output on the shapes
  measured.

**Tier 2 grows. Hidden additions, binary-stable within a Go minor.**
- One public `<name>ꞏtwin` beside each public function that can park, hidden from editors, listed by
  reflection.
- One public nested interface for each Go interface that gets a twin, and one more base-list entry on each
  implementer that gets one. Go's `reflect` does not see either.
- Hidden twins of golib's parking operations.
- A generated entry class in a converted program whose `main` is twinned. `Main` stays.
- These fall under the OQ3 `[EditorBrowsable(Never)]` pass and the golib baseline. The OQ5 guard (no Go-named
  interface gains an abstract member) is the guard that keeps the second item honest.

**Tier 3 behaviour that changes. No source or binary break.**
- **Thread identity.** Inside one C# method body the thread never changes. Between two calls into C# from the
  same converted goroutine it may. This is already declared non-contract above.
- **A goroutine is not a thread.** The published sentence "Each goroutine runs on its own thread for as long as
  it runs" becomes false for a goroutine that runs as a twin. The doc edit is owed with the landing.
- **Threads at rest and start-up**: one carrier for each processor and a monitor (13 more threads on 12
  processors), 11 to 16 ms.
- **`runtime.GOMAXPROCS` becomes real**: at most that many goroutines run converted code at once.
- **C# code that blocks inside a goroutine holds a carrier.** The spike does not establish "never a deadlock"
  for blocking the scheduler cannot see; that policy is a design still owed (the record's N5).
- **A long step that never parks is not preempted.**
- **.NET-format stack frames** (an exception's stack trace, a debugger's call stack, a profiler, a crash dump)
  show compiler-generated state-machine frames where a twin runs. Go-format tracebacks keep the Go names.
- **Goroutine ids**: until the main twin is made goroutine 1 (priced, not built), main's work shows another id.
- **Package size and build time**: +14% on the heaviest parking package measured, +5 to +7% build wall, nothing
  for a package that cannot park.

**What the contract page should say now**, so that a consumer who starts before M:N is not surprised by it:
1. thread identity between two calls from converted code is not a contract (already listed);
2. a goroutine is not promised a thread of its own;
3. .NET-format stack frames of converted code are tier 3;
4. ambient thread state seen from converted code (name, priority, culture, `AsyncLocal<T>`) is not a contract
   until MQ2 is ruled;
5. `runtime.LockOSThread` is named as the opt-out for thread identity. The spike did not build the pin: today
   the call only counts, and under 1:1 it has nothing to do. The landing form owes it.

**New questions for the owner, from the spike.** Each has G's recommendation; none is ruled.
- **MQ1. May a consumer implement the nested twin interface by hand?** Without it a hand implementer works and
  costs a thread for each parked call through it. *Recommendation*: tier 2 for now, so not for hand use; open
  it as tier 1 optional surface only when the task shape it exposes is itself settled.
- **MQ2. Ambient state per goroutine.** The spike flows no execution context, and the cost of flowing one was
  not read. *Recommendation*: declare it non-contract (item 4 above), with the pin as the answer for code that
  needs a thread's own state.
- **MQ3. How M:N is switched on.** A build with twins and the scheduler off reads as today's build (measured).
  *Recommendation*: opt-in for the first release that carries it, with the default changed later as an
  announced tightening.

**If R3 (visible async) were ever taken instead**, tier 1 breaks by construction: at least 1,370 exported
functions in 116 non-internal standard library packages change shape, every hand implementer of an affected
interface breaks, and the mapping changes for functions with `ref` or span parameters. That is the full
deprecation set of this plan and a flag day for every published package. Nothing the spike read calls for it.

**Not measured, and able to move a row**: a package built against today's golib run against a landing golib
(the spike changes the frame's layout and rebuilt everything); Visual Basic, F# and explicit-interface
implementers; the manual check in a debugger.
