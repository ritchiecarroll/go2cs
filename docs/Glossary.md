# Glossary — process terms used in commit messages, memory logs, and reviews
<!-- {% raw %} — Jekyll/Liquid guard: this doc contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

> Definitions as **actually used** in this repository's development process — especially the
> Phase-3 "full stdlib compile" campaign (2026-06/07) whose commit messages, chip briefs, and
> review verdicts lean on this shorthand heavily. For *conversion* terminology (how Go constructs
> map to C#: `ж<T>`, heap boxes, adapters, direct-ж, shadow renames) see
> [`ConversionStrategies.md`](ConversionStrategies.md) (summary) and
> [`ConversionStrategies-Reference/`](ConversionStrategies-Reference/README.md) (full detail); this file covers
> the **process** vocabulary — plus a short [**.NET and tooling terms**](#net-and-tooling-terms) section at
> the end for the general .NET/toolchain acronyms the conversion docs assume (BCL, Roslyn, CRTP, …).
> Companion docs: [`CLAUDE.md`](../CLAUDE.md) (authoritative workflow),
> [`Baseline-vs-FullConversion.md`](../src/archived/Baseline-vs-FullConversion.md).

## Gates (the checks a change must pass before it lands)

<a id="cnr"></a>**CNR — check-no-regression** (`src/tests/Behavioral/check-no-regression.ps1`).
Force-rebuilds `go2cs.exe` from current source, re-transpiles **every** behavioral test project,
and reports any generated `.cs` **or `.csproj`** that differs from the committed tree (the
transpile rewrites both; the pathspec was `.cs`-only until 2026-08-08). The pass verdict is
**"byte-identical"**: the converter change produced exactly the committed output everywhere it
wasn't intended to change something. A package whose output the run could not fully regenerate
(best-effort/untyped conversion, a recovered visitor panic, or a non-zero converter exit) fails
as **NOT MEASURED** rather than counting toward the green verdict — the byte-identical claim is
never allowed to be vacuous; other converter WARNINGs are counted as advisory, never fatal.
Mandatory for any converter (`src/go2cs/*.go`) change.
Scope caveat: CNR checks *transpile output only* — it cannot see compilation, source-generator
(`go2cs-gen`) output, or runtime behavior. A new test's own files appearing as untracked (`??`)
is the expected signature of adding a guard, not a failure.

<a id="ab"></a>**A/B (full-stdlib reconvert-diff).**
Two complete stdlib conversions — one with the **base** converter (built from `HEAD` before the
fix, usually via the *stash dance*, below) and one with the **fixed** converter — written to two
scratch directories and compared with `diff -r`. The set of differing files is the change's
**footprint**. Discipline: *every* differing file is inspected line-by-line; the footprint must be
exactly the intended transform (an unexplained third file means the fix over-fires — stop and
re-derive). A second sense, **A/B determinism**, runs the *same* exe twice and requires zero
differences (the converter is byte-deterministic; any diff means introduced nondeterminism).

**Stash dance.**
How the A/B base exe is built when the fix is uncommitted or freshly committed:
`git stash push -- <fix files>` (or checkout of the parent) → `go build -o bin/go2cs_base.exe` →
restore → build the fixed `bin/go2cs.exe`. The temporary base exe is deleted afterward so a stale
binary can't produce false results later.

**Full behavioral suite** (`src/tests/Behavioral/run-behavioral.ps1`, standalone runner).
All behavioral projects through four phases: **Transpile** (converter runs), **Target** (golden
byte-compare, line-ending-insensitive), **Compile** (C# and Go), **Output** (run both, compare
stdout; only projects marked `[GoTestMatchingConsoleOutput]`). Required — in addition to CNR —
whenever a change touches `go2cs-gen` (the source generator) or `golib` (the runtime), because
those act at compile/run time where CNR is blind. A gen change additionally requires the
**forced gen rebuild** ritual first: delete `src/gen/go2cs-gen/{obj,bin}` and run
`dotnet build-server shutdown`, otherwise MSBuild serves the cached analyzer and the suite
silently tests the *old* generator.

<a id="census"></a>**Census** (`dotnet build src/go2cs-stdlib.slnx` + an own-DLL count script).
The Phase-3 progress metric: how many of the auto-converted stdlib projects **emit their own
assembly** (`bin/Debug/net10.0/<AssemblyName>.dll` exists after a full solution build). Reported as
`N / total` (302 at the Phase-3 milestone, Go 1.23.1). The metric is **packages-compiling, not error count** — clearing an error family can
*raise* the raw error count by unmasking latent errors behind it, and that is progress.
Each census diff reports **GREENED** (was red, now green) and **REGRESSED** (must be empty —
the zero-regression invariant).

**Own-errors vs blocked dependents.**
In a census build, MSBuild only compiles a project whose references succeeded; a project whose
dependency failed is **skipped**, contributing zero error lines. So the red set divides into
**leaf failures** (packages with their *own* build errors — the only ones that need fixing) and
**blocked dependents** (red only because a dependency is). Corollary: a package that is red with
*zero* error lines anywhere is either a skipped dependent or a bookkeeping artifact (historically:
a csproj on disk that was never registered in the `.sln`, so it was counted but never built).

<a id="overlay"></a>**Overlay.**
The ritual that makes a census measure the *current* converter rather than the stale committed
tree: full reconvert to a scratch dir **seeded with a copy of `src/core`** (so the
`[module: GoManualConversion]` gate can see the hand-owned files and emit `.cs.auto` review siblings
instead of clobbering them) → copy the fresh `.cs` **and** `.csproj` over `src/core/` → clear
`bin/obj/Generated` → build. Since the trees unified (2026-08-01) the copy is **straight**: the
reconvert writes the repository's own paths, so there is no reference rewriting and no exception
list. Skipping the SEED still craters the census (the `unsafe` → `runtime` cascade). The overlay is
*regenerable scaffolding*: it is never committed, and the tree is restored afterward
(`git checkout HEAD -- src/core` + `git clean`).

## Fleet coordination (multi-session development)

**Chip.**
A focused, isolated work session (its own git worktree and branch) assigned **one root family**,
with a self-contained brief: verified diagnosis, binding **file ownership** (see below), the gate
list, guard requirements, and the commit protocol (gpg-signed commits on its own branch; never
master; never push). Chips end with a **final summary** (per-root status, commit hashes, gate
results, footprints, anything escalated). A **sub-chip** is a chip spawned from within another
chip's session for an out-of-scope discovery.

**Chit.**
The clickable task card the coordinator posts to the user's panel that, when started, SPAWNS a
chip — a chit in the classic sense: a small ticket redeemable for the thing it names. The
distinction matters operationally: the chit is ephemeral (task ids do not survive app restarts, so
a reboot can eat a chit and the coordinator reposts it), while the chip ARC it describes — the
brief, the ownership lock, the branch — persists in the docs and memory regardless. One chit, one
chip; a stale chit is dismissed and superseded, never left to mislead.

**Coordinator.**
The session that runs the campaign: diagnoses (or fleets out diagnosis), spawns chips, answers
their escalations against project goals, reviews landings (adversarially where flagged),
cherry-picks branches to master in **batches**, runs the consolidated gates, and keeps the memory
log. The chip/coordinator split exists so each fix session stays small and expendable while
integration discipline stays centralized.

**File ownership.**
The concurrency rule that let many chips run in parallel safely: each converter/gen/golib file has
exactly **one** active owner; chip briefs name both the owned files and the explicitly forbidden
ones. Shared hot files (e.g. `main.go`) are partitioned by *function*, with each chip required to
name the exact functions it touched in its summary for merge planning.

**Precondition (subject-based).**
A chip whose work builds on another chip's unmerged fix starts by checking master for the
prerequisite and **stops cleanly** if absent. Checks must grep commit **subjects** (or test for
guard files), never original branch hashes — integration is by cherry-pick, which mints new
hashes, so `git merge-base --is-ancestor <original>` is never true even when the content landed.

**Batch.**
The integration unit: one or more reviewed chip branches cherry-picked (`-x`) onto master
together, followed by one consolidated gate pass (converter build → CNR → suite if gen/golib is in
the batch → reconvert → census). Completeness after a batch is verified with `git cherry`
(patch-id) *plus* a subject-level check — a lock collision can silently skip a pick mid-loop, and
conflict resolutions change patch-ids.

**Adversarial review / refuter.**
Pre-merge review whose prompt is to **refute** the work — assume a subtle semantic error and hunt
for it, with branch-specific "trap" lists (the known failure modes of that code area). Verdicts:
`APPROVE`, `APPROVE_WITH_NOTES` (concerns become banked follow-ups), `BLOCK` (demonstrated error —
the branch does not merge). Distinct from the cheaper **audit**, which checks discipline
compliance per commit: gpg signature, guard registered in the solution, no hardcoded marker
glyphs, docs updated, one-root scoping.

**Scout.**
A read-only diagnosis session in its own worktree: builds the packages *about to be un-gated*,
peels masking layers with **shims**, and reports exact roots — no fixes, no commits. Shims are
documented diagnostic byte-patches applied only to the scout's uncommitted overlay to let a
dependent compile so its *own* latent errors surface; they prove a fix *shape* compiles but are
never fixes themselves.

**Wave / frontier.**
A **wave** is a coordinated group of chips spawned from one diagnosis round. The **frontier** (or
**red set**) is the current list of red census packages; the campaign advances by collapsing the
frontier's leaf failures wave by wave.

**OQ (open question) / RULED / owner OQ.**
A design doc names its unresolved decisions as numbered **OQs** (`⟨OQ-3⟩`) *before* the design is
ratified, so what is still open is explicit rather than latent in the prose. Each OQ is closed by
a **ruling** — a recorded decision with its rationale, usually by the coordinator against
evidence — after which the doc marks it RULED and implementation may bind to it (an
implementation entry citing `⟨OQ-3⟩` means "this code does what that ruling chose"). An
**owner OQ** is one only the human project owner can rule — naming, licensing, external
registrations, spend — and is parked in the plan's owner-decisions section until the owner
answers; nothing machine-blocked ever waits on one.

<a id="banked"></a>**Banked / bank.** Two senses, disambiguated by object:
1. *(a finding is banked)* A verified-but-not-fixed item recorded durably (memory log + a
   banked-follow-ups note) instead of fixed now — typically a latent sibling with zero occurrences
   in the current corpus, or a concern from review. Banking is legitimate *only* with a written
   diagnosis; several banked items later materialized in new packages and were fixed from the bank
   without re-diagnosis.
2. *(a package is banked)* — the Phase-4 sense. A package whose Go test suite **validates**
   verdict-for-verdict against `go test -json` has its converted test artifacts **committed** into
   `src/core/<pkg>` (test sources, host, `package_test_info.cs`, `<pkg>.tests.csproj`,
   any disclosures manifest) **in the same commit as** its row and the header totals in
   [`ValidatedTestPackages.md`](ValidatedTestPackages.md) (charter §4.6). The committed artifacts
   are the package's own regression guard from then on.

<a id="banked-roster"></a>**Banked roster.**
The set of packages currently banked in sense 2 — operationally, **whatever
`ValidatedTestPackages.md`'s table lists**, each row carrying the package's exact matching-verdict
and disclosed counts. The table is the single source of truth; scripts parse it rather than
hardcoding a list, so the roster can never drift from what a banking commit just recorded.

<a id="validated-sweep"></a>**Validated sweep (full).**
The charter §5 *operational re-validation* gate, run by **`src/run-validated-sweep.ps1`**: for
**every** package on the banked roster (parsed live from the table), re-run the complete `-tests`
pipeline — reconvert the suite with the current converter, rebuild the test host, run it, and diff
the verdicts against `go test -json` — and require each package to land on its **exact banked
counts**. A package that "passes" with a *different* count fails the sweep (the table and reality
must agree; one of them is now wrong). "Full" distinguishes it from a `-Filter` run of a few
packages. Properties that make it the campaign's decisive instrument: it is the only gate that
exercises banked **test-source** emission (CNR covers behavioral projects; the
isolation-reconvert-diff covers production `.cs`); it re-generates every banked artifact from the
current binary (so mixed-vintage output cannot hide); and its post-run **content-drift report**
(`--ignore-cr-at-eol`, CRLF phantoms excluded) is what tells the coordinator whether a converter
change silently moved any banked artifact. Serial by design (concurrent runs collide on shared
dependency DLLs, CS2012), with per-package deadline **floors** for legitimately slow suites
(`$longTimeouts`: slow-host-calibrated, variance-sized for randomized workloads like
`crypto/dsa`'s prime search, and raisable — never lowerable — by a larger `-TestTimeout`) so a
slow tail is never misread as a failure.

<a id="harvest"></a>**Harvest.**
The deliberate collection pass that follows a landed shared-machinery capability: re-measure and
bank the rows that capability **unblocked**. Cheap by construction — the fixes already landed, so
the work is measurement and banking, not fixing. The third instrument in a trio: a **scout**
characterizes what is unknown, a **sweep** re-proves what is already banked, a **harvest** banks
what just *became* bankable. It institutionalizes the r57a lesson that a census taken under a
live defect measures the defect, not the package — the yield is not only in what nobody has run,
but in what nobody has **re-run since the thing blocking it got fixed**. (Named in r58a's charter
— "then the harvest: bank crypto/rsa …, then log, …, then re-measure nistec" — where the
allocation counter's landing turned five characterized near-misses into one collection pass.)

<a id="mvp"></a>**MVP — minimum viable [increment].**
The smallest *correct and fully-gated* first cut of a converter/runtime feature: the narrowest
predicate or scope that proves the whole mechanism end-to-end while broader coverage is deferred to
later, independently-gated phases (the deferred scope is **banked**, above). Reached for when a
feature has a wide but risky design space and the aggressive, silent-failure-prone part of it is best
proven safe on a tiny footprint first — e.g. the stack-string `sstring` emission (`s := string([]byte)`
→ a zero-copy view instead of a heap `@string` copy), whose MVP fires on exactly **one** Go-1.23 stdlib
site under a deliberately conservative eligibility predicate, with unnamed-temporary conversions and
the loop-carried liveness guard deferred. An MVP is **not** a prototype, spike, or shortcut: it ships
behind the full gate set (CNR, the behavioral suite, its own **guard**) and is production-correct
*within its scope* — only its **reach** is minimal, and it is widened later one gated phase at a time.

**Root / root family.**
A **root** is one distinct converter/generator/runtime defect, stated as the *general* construct
it mishandles (never as "package X fails"). **One root per commit.** A root family is a set of
roots sharing a mechanism (e.g. the u8/string-boxing family: the same gate missing at assignment,
composite-literal, tuple-return, and channel-send positions).

## Test artifacts & conventions

<a id="guard"></a>**Guard.**
A behavioral regression test locking in a fix: a minimal Go program exercising the exact
construct, its transpiled `.cs`, and a **golden**. Requirements that recur in reviews: solution
registration in `src/go2cs.slnx` **plus grep-verification** (the *silent-drop gotcha*: the harness
builds by path, so an unregistered guard still passes and nothing else catches it);
**negative-check** where feasible (prove the pre-fix converter fails the guard with the diagnosed
error); **write-visibility** proof for pointer/box fixes (the program observes a write through the
fixed path in its output, compared against Go — distinguishing a faithful fix from a
compiles-but-copies shortcut).

<a id="golden"></a>**Golden / re-baseline.**
The committed expected transpiler output (`*.cs.target`), byte-compared (line-ending-insensitive)
by the Target phase. When an *intended* emission change alters existing goldens, they are
**re-baselined** — regenerated from converter output (via the `UpdateTestTargets` utility with
`--createTargetFiles`), never hand-edited — and the Output phase must stay green to prove behavior
was preserved. Gotcha: `UpdateTestTargets` copies the **on-disk** transpiled `.cs`, it does not
run the converter — a fresh transpile (CNR does one) must precede it, or the re-baseline silently
captures stale output. **Cross-chip interaction drift**: a guard baselined on a chip branch may drift once
*sibling* chips merge (the combined converter emits differently than any single branch saw); each
drift line is attributed to a specific landed fix before re-baselining.

**TestMethods regeneration.**
The four `*Tests.cs` classes contain generated `// <TestMethods>` blocks. After any merge that
touches them, the blocks are regenerated with `UpdateTestTargets` rather than trusting textual
conflict resolution — hand-merged unions have both dropped `[TestMethod]` attributes (MSTest then
silently skips the test; the standalone runner, which discovers by directory, is unaffected) and
duplicated methods.

**gpg-G.**
`git log --format='%G?'` showing `G` — a good signature. All campaign commits are gpg-signed;
signing is never bypassed, and cherry-picks re-sign automatically.

**Marker glyphs / `Symbols.cs` / `symbols.json`.**
The emitted C# uses reserved glyphs (`ж` pointer box, `Ꮡ` address-of, `Δ` shadow/collision rename,
`ꓸ` type-alias dot, `ᴛ` temp, `ᒐ` the per-function `GoFrame` defer local, …).
Converter and generator **source** must reference named constants, never literal glyphs: the C# side (golib, go2cs-gen) uses `Symbols.cs`
(`PointerPrefix`, `AddressPrefix`, `ShadowVarMarker`, …, via `using static go2cs.Symbols`); the
Go converter uses the same-named constants in `src/go2cs/symbols.go`. Both files are **generated
projections of the canonical symbol table `src/core/go2cs/symbols.json`** (kept pure-ASCII —
every glyph a `\uXXXX` escape — so no tool can mangle it): edit the JSON and regenerate with
`go generate .` from `src/go2cs`, or run `src/check-symbol-sync.ps1`, which regenerates and
exits 1 on drift. Never hand-edit the two generated files. Deliberately OUTSIDE the table
(deferred 2026-07-16): the converter's Go-only `reserved` list and the `Go*` attribute-name
strings — the attribute/type names are C# identifiers *defined* by golib source (folding them in
would mean generating golib class names), and `reserved` has no C# twin and already consumes the
symbol constants; likewise the `{{PackageName}}_package` text inside go2cs-gen emission templates
(it is the emission shape itself). Audits flag violations. Glyphs inside
goldens, docs examples, and comments are fine — they *are* the output.

<a id="adapter-names"></a>**Adapter class names (`XжI` / `XᴠI`).**
A generated interface-implementation adapter composes its name from the concrete type, a **form
glyph**, and the interface's simple name: `errorStringжerror` reads "`*errorString` as `error`";
`net_ConnᴠWriter`, "a foreign `net.Conn` value as `io.Writer`". Three shapes exist. A same-package
**value** implementation needs no adapter at all — the generator adds the interface to the struct's
own base list (`partial struct T : I`) and the conversion is implicit. A **pointer** implementation
(`GoImplement<T, I>(Pointer = true)`, i.e. Go's method set puts the method on `*T`) emits a `TжI`
class wrapping the receiver box `ж<T>`, so the interface value aliases the pointer exactly as Go's
does — pointer identity, write-through, and type-assert-back all survive — and every conversion site
must therefore construct it (`new errorStringжerror(Ꮡ(…))`). A **value** implementation whose
concrete cannot carry a base list — a foreign struct, or a named func type (a C# delegate cannot be
a partial struct) — emits a `TᴠI` class. Converter (`adapterTypeRef` / `valueAdapterTypeRef` in
`src/go2cs/main.go`) and generator (`ImplementGenerator`) compose the name independently and must
stay in sync; a foreign concrete is package-qualified first (`os_FileжReader`).

Three properties of the scheme are load-bearing, and answer the recurring "why not `жT_I`?": the
separator must be a character **illegal in a Go identifier**, or the name stops decomposing back
into its parts — `_` is legal in Go names *and* already means package-qualifier here, so a
hypothetical `жos_File_error` is ambiguous where `os_FileжReader` is not. **Concrete-first** keeps
each adapter sorted beside its own type in IntelliSense, object browsers, and CS-error output,
where a leading glyph would instead bucket every adapter in the package together. And the **form is
part of the name** (`ж` vs `ᴠ`), so only same-form pairs can collide — which the CS0102 prune
relies on. One naming trap: the constant is `PointerPrefix` because it *prefixes* the type in
`ж<T>`; in a composed adapter name the same glyph sits **between** the two names.
See [Interfaces](ConversionStrategies-Reference/interfaces.md#interfaces).

**OWED.**
Annotation in memory logs for a known debt: an *OWED merge* (a finished chip branch not yet
integrated), an *OWED guard* (a fix landed with its regression test deferred — rare and always
tracked), or an owed documentation/log compaction.

## Document types

The repository's durable documents fall into six kinds, and which kind a document is decides who
may edit it, what may be executed from it, and which one wins when two disagree. The ladder in one
line: **runbooks lead on procedure; plans hold ruled strategy; records hold evidence; the board is
the append-only findings ledger; the mailbox is transport; the kickoff is the rewritable restart
document and never evidence.**

<a id="runbook"></a>**Runbook.**
A standing, **version-agnostic** procedure document that survives its instances — today the two
migration runbooks, [`GoCorpusMigration.md`](GoCorpusMigration.md) and
[`DotNetMigration.md`](DotNetMigration.md). A runbook **leads**: it is executed as written, and a
deviation found mid-run fixes the runbook in the same stage that found it. It names instruments,
gates and traps, never a particular release, and carries **no frozen figures** — budgets live in
CLAUDE.md's measured table, counts with the instrument that re-measures them. A runbook edit never
reopens a ruling.

<a id="plan"></a>**Plan (strategy plan / instance plan).**
A `PLAN-*.md`. A **strategy plan** ([`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md)) fixes a ruled
frame — which targets, in which order — and holds its OQ rulings; it stays live until its ladder
completes. An **instance plan** ([`PLAN-hop-campaign.md`](PLAN-hop-campaign.md)) supplies one
campaign's fleet, readings and sequencing, quotes real figures **because it is an instance**, and
**supplies no procedure** — procedure lives in the runbooks, referenced, never restated. When its
instance executes, an instance plan becomes a record. Plans never override runbooks on procedure;
runbooks never reopen plans' rulings.

<a id="hop"></a>**Hop.**
One release migration run as a unit: a **.NET hop** moves the runtime and TFM, a **Go corpus hop**
moves the release the corpus converts from — one variable at a time, never both in one hop. Targets
freeze at hop start (⟨OQ-1⟩). Lettering is per campaign (Hop N = .NET 10; Hop A = Go 1.23.12).

<a id="record"></a>**Record (recon / rehearsal / census / data / stage ledger).**
A point-in-time evidence document under `phase4/` — **RECON-** (a read-only survey before work),
**REHEARSAL-** (a dry run of a procedure), **CENSUS-** (an enumeration of a population at a date,
often later encoded into an instrument), **DATA-** (instrument output retained because it is
unrecoverable), **STAGE-/MILESTONE-** (a ledger of one stage's execution). Records are **amended,
never rewritten** — dated `⚠ AMENDMENT` blocks, findings marked with their disposition
("landed: file / commit") when acted on — and they are **never executed from**: procedure extracted
from a record moves to a runbook. Each carries a **state line** that moves:
`DRAFT → RATIFIED → EXECUTED/CLOSED → SUPERSEDED-BY <path>`. And a record's file:line citations into
other documents resolve against those files **at the record's banking commit**
(`git show <sha>:<path>`), not against today's tree — a record that cites by line says so in a
pin-note.

<a id="kickoff"></a>**Kickoff.**
[`phase4/KICKOFF-fleet.md`](phase4/KICKOFF-fleet.md) — the fleet's **current, rewritable restart
document**, and the one document kind that is **never evidence**. It holds what a session starting
on master needs in one read: preservation steps, the roster, the current state, the rulings and
records owed, and one kickoff prompt per session. It is **rewritten in place** as state moves (a
record is amended, a kickoff is replaced), it is always read at **master's tip** and never at a
cited SHA, and nothing is banked from it: a count or a SHA it carries is a pointer to where the
measurement lives, not the measurement. It supersedes the handover log's state and prompt sections;
the log stays the append-only decision record, read with `git show` when a ruling's history
matters. A kickoff is not one of the RECON-/REHEARSAL-/CENSUS-/DATA-/STAGE- records: it is the
only `phase4/` document a session executes its first steps from, which is exactly why it may hold no
evidence.

<a id="board"></a>**Board.**
[`phase4/BOARD-next-validation-candidates.md`](phase4/BOARD-next-validation-candidates.md) — the
append-only findings ledger: durable findings, measurements and campaign rulings land there, inside
the Liquid raw guard, and old entries are never edited. Delegated censuses are re-measured on the
board rather than carried elsewhere.

<a id="mailbox"></a>**Mailbox / lane.**
The mailbox ([`phase4/MAILBOX.md`](phase4/MAILBOX.md), branch `claude/mailbox`) is the fleet's async
**transport, not record**: questions, ready-signals, warnings — polled at session start and before
final gates. Durable content routes by kind: procedure → the runbook in-stage, harness/gate doctrine
→ CLAUDE.md, findings → the board. A **lane** is one worktree-plus-session executing an assigned
slice of a campaign under the worker contract ([`GoCorpusMigration.md`](GoCorpusMigration.md) §3.6).

## .NET and tooling terms

General .NET / C# / toolchain terms the conversion docs assume. (For the *emitted-code* glyphs
`ж`/`Ꮡ`/`Δ` see **Marker glyphs** above and the conversion docs:
[summary](ConversionStrategies.md) · [reference](ConversionStrategies-Reference/README.md).)

<a id="bcl"></a>**BCL — Base Class Library.**
The core class library that ships with .NET: the fundamental `System.*` types — `System.Object`,
`System.String`, the integer primitives (`System.Int32` and the native `nint`/`nuint`),
`System.Collections`, `System.Numerics`, and so on. When a Go construct maps "to the BCL" it maps onto a
built-in .NET type rather than onto `golib` or a source generator — e.g. Go `any`/`interface{}` → `object`,
the fixed-width integer aliases (`int32` = `System.Int32`), and the `System.Numerics` operator interfaces
that generic constraints lift to.

<a id="ilc"></a>**ILC — the IL Compiler (Native AOT).**
.NET's ahead-of-time compiler: it consumes IL at **publish** time and emits native machine code, so the
program starts without a JIT. go2cs measures it as the third column of the performance suite (Go binary /
C# JIT / C# **Native AOT**), and it is expensive here — each AOT publish compiles the full converted-stdlib
closure, ~25 min per benchmark on the current coordinator class, which is why a full perf run is hours and
must run solo. **ILC arrives as a runtime pack selected by the [TFM](#tfm), not by the SDK performing the
publish** — so a new SDK publishing an old TFM silently resolves the OLD ILC and produces the previous
generation's codegen, presenting as a plausible "the new runtime changed nothing" rather than as an error
(trap 1, [`DotNetMigration.md`](DotNetMigration.md) §3). Consequence: no AOT measurement of a new runtime
exists before the TFM moves.

<a id="tfm"></a>**TFM — Target Framework Moniker.**
The identifier a `.csproj` uses to declare which framework a project builds against: `<TargetFramework>net10.0</TargetFramework>`
— the go2cs corpus's own value today. It selects the reference assemblies compiled against, the runtime the
output binds to, and (per the migration runbook's trap 1) the **Native-AOT compiler**, since ILC arrives as a
runtime pack chosen by the TFM rather than by the SDK performing the publish. That is why a .NET migration
splits into two gated stages: the SDK alone moves first (§4 — the compiler and build tooling change while the
TFM stays put, so C# language level and diagnostics move but the runtime does not), then the TFM moves (§5 —
one line per project, after which the new runtime and its ILC are finally what is being measured). See
[`DotNetMigration.md`](DotNetMigration.md).

<a id="roslyn"></a>**Roslyn.**
The .NET compiler platform — the C#/VB compiler plus its analyzer/codegen APIs. Its **source generator**
feature lets code participate in compilation, and go2cs relies on it heavily: the generators in
`src/gen/go2cs-gen/` (referenced as an analyzer by every converted project) synthesize the Go semantics C#
cannot spell directly — interface satisfaction, pointer-receiver overloads, struct-embedding promotion,
named-type operators. See [Source Generators](ConversionStrategies.md#source-generators).

<a id="crtp"></a>**CRTP — Curiously Recurring Template Pattern.**
The idiom where a type is parameterized by *itself* — `T : IFoo<T>`. .NET's generic-math and comparison
interfaces use it (`IComparisonOperators<TSelf, TOther, TResult>`), and `golib`'s `comparable<T>` is a CRTP
constraint so a type can state "I am comparable with my own kind" — the shape Go's `comparable` built-in
maps to.

<a id="nre"></a>**NRE — NullReferenceException.**
The .NET exception thrown when a member is accessed through a null reference (the CLR analogue of a nil
dereference). In the conversion docs it appears mostly as a *hazard being designed out*: a zero-value
`@string` or an unallocated promoted-embed box would otherwise NRE on first use, so `golib` and the
converter make those zero values safe to touch.

<a id="msbuild"></a>**MSBuild.**
.NET's build engine — it drives `dotnet build`, project references, and analyzers. It matters to the
converter in two places: cross-package references arrive at the source generators as compiled **metadata**
references (not source syntax), which changes how embeds/interfaces must be resolved; and the converter
emits MSBuild `.csproj` project files (carrying the `$(go2csPath)` property) plus `.slnx` solutions for the
converted output.
<!-- {% endraw %} -->
