# go2cs Roadmap

<!-- Rewritten 2026-09-29 from master 2ff42f7a16 to hold only what is planned. The completed phases
     (0 to 4), the Platforms record, the progress table and the old converter-items list moved
     VERBATIM to docs/RoadmapHistory.md. Carried forward from the old page and restated here: Phase 4D
     (old Phase 4C, last bullet), the host-limit retirement path (rewritten from a proposal into a
     record, as DotNetMigration.md stage 7 asks), the two remaining sstring increments (old 1 and 4),
     Phase 5, the macOS and other-architecture flavor gap, and the still-open ToDo items. The
     single-file test-host proposal is not carried: the test host publishes as a self-contained
     single-file executable and the os/exec host-limit verdicts pass (ValidatedTestPackages.md,
     host-limit class). The converter-loop sentence under "How progress is tracked" is an editable
     site of src/migrate-gorelease.ps1 (Expect = 1): keep exactly one copy of it on this page.
     Amended 2026-10-03: the converter-loop sentence no longer names a release; its migrate-gorelease
     site is retired. -->

<!-- Amended 2026-09-29 after three verification passes. Sources: the post-100% roadmap brief of
     2026-09-28 (sections 3-6, its owner-ruling block and appendix A's timing table) and the
     2026-09-28 and 2026-09-29 owner rulings. Changes:
     "Where things stand" cut to one sentence with no forecast; the jwt row says "tests span several of
     its own packages" (gojq's module already has several packages, and the several-package ruling
     comes due at gojq); hop cadence is a Q4 2026 decision (brief s3 and critic item 8); interface
     items are unscheduled (banked for after Go 1.24, no date); the allocation work is the retirement
     plan named by the deferred allocation-count disclosures, a committed campaign whose design
     kickoff appendix A places in Jan-Mar 2027, and it is NOT the test-kinds Phase 4D; the Go 1.25 row
     names the behavioral cost PLAN-corpus-upgrade.md records (hop C row, R11/R12, OQ-2); Phase 4D's
     kickoff is Q1 2027 (brief s3 phase 5); net10.0 stays the published target until net12.0 as ruled,
     so the .NET 11 stage informs that move rather than deciding publication; the pin and escape are
     dated only as "before .NET 11 ships"; the cgo, uuid and macOS starts are October targets that
     begin as capacity frees (the 2026-09-29 ramp), not ruled dates; PLAN-corpus-upgrade.md frames
     how hops run, and this page states the releases (the 2026-09-28 hop ruling is not yet in PLAN);
     the cross-package address-of-global gap is cited to the struct-types reference, since ToDo.md
     does not carry it; the dynamic-struct cast list regains TypeSwitchStmt and ValueSpec, which the
     old page and ToDo.md both list; the Target Atlas is one input to module selection (gojq is an
     off-atlas pick); Phase 5 regains the leaf-first priority, the decl/impl pairing, the
     stub-is-not-support rule, the blocked-status rule and the "every applicable package validated"
     exit clause; Phase 4D regains "without delaying Phase 5". -->

<!-- Refreshed 2026-10-10 at master 4c42cc7d3a, for accuracy (owner request). Sources: the roster and
     ValidatedModules.md at that master; NEWS.md's October 9 entry (1.24.13.5); KnownIssues.md's Native
     AOT entry; Limitations.md (the cgo refusal, GO2CS2001, on master since b8ffb7f192);
     PLAN-nugetgo.md open item 3 (the prefix reservation, undecided as of its 2026-10-04 amendment).
     Changes: Phase 4 gets its own section (4A to 4C complete, macOS validation in progress, 4D planned);
     Phase 5 is reframed as work that proceeds per package as validation demands, with its three
     sub-phase headings folded into one description, and is told apart from the trim work's own
     numbered stages; a Native AOT and trimming section is new; the NuGet section states what is
     published; the module table drops jwt (validated) and keeps gojq; cgo's loud failure is stated as
     done; the Go 1.25 hop is a decision with no month; the timeline is rewritten to match. No count
     is quoted for macOS: the roster's rows carry it. The trim plan is not linked because
     docs/PLAN-golib-full-trim.md is not on master at this commit.
     Amended the same day at master 2ddabcc67a: the Phase 5 section links the stub inventory
     (docs/phase4/CENSUS-phase5-stub-inventory.md, landed that day) in place of its placeholder, in words,
     with no count.
     Owner review 1, the same day: every ## section opens with an "In short" paragraph (what it is, why it
     matters to a user, where it stands), and Phase 5's says it is about completeness, not speed, with
     a placeholder for the measurement of which exported functions can reach a stub. -->

This page is the plan for what comes next in go2cs: the Go and .NET releases the converted standard
library moves to, the platforms it runs on, the real-world Go code it converts beyond the standard
library, and the runtime and converter work still ahead. It states what is planned. The completed
phases, with the git commits and tags that anchor them, are in [Roadmap history](RoadmapHistory.md).
Dates are month or quarter targets.

**Where things stand.** Every implementable package of the converted standard library validates
against Go's own tests on Windows and Linux, and the same validation is under way on macOS, package
by package. `go2cs -tests -recurse` validates real third-party modules against their own tests the
same way, and the first converted module is published as a NuGet package.
[Validated Test Packages](ValidatedTestPackages.md), the validation roster, carries the current state
of the standard library, and [Validated Modules](ValidatedModules.md) that of third-party modules.

## Phase 4: validation against Go's own tests

**In short.** Phase 4 checks the converted standard library against Go's own tests, package by package,
so that "it works" is measured and not claimed. It is done on Windows and Linux, under way on macOS, and
has one planned extension.

Phase 4 converts each package's `_test.go` suite and compares it, verdict for verdict, with `go test`.
It is not finished, and it stands in three parts:

- **Phases 4A to 4C are complete.** The test pipeline, subtests and parallelism, and the package-by-package
  campaign are done: every implementable package validates on Windows and Linux. Their record is in
  [Roadmap history](RoadmapHistory.md#phase-4--convert-and-run-go-package-tests).
- **macOS parity is in progress.** Each roster row gains a macOS count when the package validates on
  both Apple silicon and Intel with the two agreeing test for test. The
  [roster](ValidatedTestPackages.md) rows carry the live state; see [Platforms](#platforms).
- **Phase 4D is planned.** It widens validation to examples, fuzz seed corpora and benchmarks; see
  [below](#phase-4d-examples-fuzz-seed-corpora-and-benchmarks).

## How the project moves: release hops

**In short.** go2cs follows Go and .NET one release at a time, and every move proves every package
again. For a user that means each go2cs release names exactly one Go release and one .NET release.

The project advances by release migrations, called [hops](Glossary.md#hop): moving the converted
standard library (the *corpus*) to a new Go release or to a new .NET runtime, one variable at a time
and never both in one hop. Each Go hop re-validates every package against that release's own tests.
This page states which releases come next. Three documents carry the work itself, and this roadmap
links to them rather than restating them: [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md), which
frames how hops are run, and the two standing runbooks that lead on procedure,
[`GoCorpusMigration.md`](GoCorpusMigration.md) for a Go release migration and
[`DotNetMigration.md`](DotNetMigration.md) for a .NET one. What each document type is for, and which
one wins when two disagree, is in [`Glossary.md`](Glossary.md#document-types).

## Go releases

**In short.** The converted standard library is on Go 1.24.13, a release Go no longer patches, so it
lacks the security fixes Go has issued since. Three planned moves bring it to a supported Go release
and keep it there; the first starts when the project owner closes the Go 1.24 line.

Go 1.24 left upstream support in February 2026, and the 1.25 series left it in August 2026, so
packages built from either carry none of the Go security fixes issued since. The next three hops
return the corpus to a supported Go release and keep it there. The Go 1.24 line is not closed: at
least one more Go 1.24 package release is expected first, and the Go 1.25 hop starts when the project
owner decides that line is done, in November 2026 at the earliest:

| Hop | When | What it involves |
|:--|:--|:--|
| **Go 1.25** (1.25.14, the final 1.25 patch) | After the last Go 1.24 package release; no earlier than November 2026 | No language changes. The cost is behavioral (Go 1.25's nil-check fix, updated `unicode` tables, the `AllocsPerRun` change), plus Go's testing-API additions in the converted test host and `testing/synctest` joining the corpus; then every package is re-validated. |
| **Go 1.26** (the current 1.26.x patch) | After the Go 1.25 hop, as a separate hop | Language additions: `new(expr)` and self-referential generic constraints. |
| **Go 1.27** | Q1 2027, by about February | Generic methods, which need a conversion design first. The 1.26 series leaves support when Go 1.28 ships, around February 2027, so the corpus reaches 1.27 before then. |

How often later hops run is decided ahead of the Go 1.27 hop.

## .NET

**In short.** Converted code targets .NET 10, a long-term-support release, and stays there until
.NET 12. You need the .NET 10 SDK, and a newer SDK on your machine does not change what you get.

**The published target stays `net10.0`.** .NET 10 is a long-term-support release, supported until
November 2028, and converted packages stay on it until `net12.0`, the next long-term-support release.

**The SDK and the C# language version are pinned.** A repository `global.json` pins the .NET SDK,
and every project, including the template the converter emits, fixes `LangVersion` instead of
following `latest`, so a machine whose default SDK moves to a newer .NET does not silently compile
converted code under a newer C#. The converter also escapes Go identifiers that newer C# gives new
meaning, such as `closed`, `union` and `safe` when they name a type.

**A .NET 11 measurement stage follows the Go 1.25 hop**, once .NET 11 has shipped (November 2026), and
never overlaps a Go hop. It compares JIT and allocation behavior on .NET 11 over identical code, and
reviews the deployment shape, including the full-trim publish floor, which depends on the runtime
library's trim safety ([Native AOT and trimming](#native-aot-and-trimming);
[`PLAN-bflat-perf-exploration.md`](PLAN-bflat-perf-exploration.md)). Its findings inform the move
to `net12.0`; `net10.0` stays the published target until then.

### Declared host limits and their retirement path

The roster's **`host-limit`** disclosure class, defined in
[`ValidatedTestPackages.md`](ValidatedTestPackages.md), covers tests whose premise rests on a property
of the test *binary* that the converted host's deployment shape structurally lacks. An entry must
name that structural property, never a fixable defect, and it is written to retire itself when the
shape gains the property: disclosed tests keep running in every validation run, so an entry that
starts passing breaks the roster's disclosure arithmetic loudly and must be removed. The converted
test host publishes as a self-contained, relocatable single-file executable, so Go tests that copy
and relaunch their own binary pass.

Entries remain in `crypto/tls`, `os`, `os/exec` and `syscall`, and not every one retires through the
deployment shape; the roster's class text names what each one needs. In `crypto/tls`'s
`TestBogoSuite`, BoringSSL's test runner starts the host once per case inside Go's own 10-minute test
deadline. The interop itself passes, and the run is bound by throughput rather than start-up time, so
no start-up work alone retires it. The entry retires when managed steady-state TLS throughput brings
the run inside the runner's deadline, which is what the TLS work under [Performance](#performance) targets.

## Native AOT and trimming

**In short.** You can publish converted Go as a native executable today, and it runs, but the publish
can take hours and the file is large. This work makes that publish smaller, quicker and free of
warnings, in stages.

A converted program, and a C# program that references the `go.*` packages, can be published with Native
AOT. Making that publish smaller, faster and free of trim warnings is staged work on the runtime
library and the source generators:

- **Shipped in 1.24.13.5.** Every package registers the Go types it defines, and the runtime library's
  trim warnings are more than halved. A Native AOT publish runs on `linux-x64`, `win-x64` and `osx-x64`,
  which the check that gates each release measures with the default trim. A fully trimmed build
  (`TrimMode` set to `full`) runs for the first time, measured on `win-x64` on one development machine.
- **In progress.** Making `reflect` work under Native AOT without generating code at run time, which
  is what most of the remaining warnings are about.
- **Later stages** cover method sets and stack frames.

**The cost today is real.** With the default trim, Native AOT compiles every `go.*` assembly a program
references in full. A program that uses `fmt` and `reflect` took about five and a half hours to publish
on a six-core `win-x64` machine and produced an executable of about 370 MB, which ran correctly.
[Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)
has the measurements. A build, `dotnet run` and a publish without Native AOT are not affected.

## Platforms

**In short.** go2cs is validated on x64 Windows and x64 Linux. macOS, on Intel and on Apple silicon,
has packages and is being validated package by package now. Other platforms are not supported yet.

**Windows and Linux.** The converter runs on both, and the standard library is multi-target on disk:
platform-shared files sit flat, and a package whose Go source varies by platform keeps one folder per
target OS ([design](phase4/DESIGN-multiplatform-corpus.md)). Each platform-varying package ships a
`win-x64` and a `linux-x64` flavor, and roster rows record their Linux verdict counts beside the
Windows record. Remaining Linux operational items are tracked in
[`PLAN-linux-operation.md`](PLAN-linux-operation.md) and the multi-target design.

**macOS (Intel and Apple silicon):** packages ship for both chips, the behavioral suite passes on both,
and the README walkthrough runs. The standard library is being validated package by package on macOS
now. A roster row gains its macOS count only when both chips agree with each other and with Go, and
the [roster](ValidatedTestPackages.md) rows show which packages have one. Until a package has its
count, don't expect it to be fully operational there. The work also finds macOS-only gaps, each fixed
as it is found. The compile is checked daily in CI on macOS arm64 and x64 runners
([`CIMatrix.md`](CIMatrix.md)).

**Other architectures** receive the Windows flavor until a flavor ships for them: a platform-varying
package's Windows flavor is what compiles and loads, and its platform-entangled behavior (local time
zones, sockets, the `syscall` surface) follows Windows semantics or fails.

## Real-world Go modules

**In short.** Beyond the standard library, go2cs converts third-party Go modules and holds each to the
same bar: its own tests must match Go's. Three modules pass today, and a larger one is next.

Third-party modules are the proving ground after the standard library. Each module is converted,
validated against its own Go test suite exactly as a standard-library package is, and given a proof
page beside the conversion rather than on the standard-library roster. `go2cs -tests -recurse` runs a
module's tests across all of its packages, and a validated module packs as one NuGet package.

Three modules validate today, each on the platforms [Validated Modules](ValidatedModules.md) names:
`golang.org/x/sync`, [`google/uuid`](https://github.com/google/uuid) and
[`golang-jwt/jwt`](https://github.com/golang-jwt/jwt) v5. `uuid` is one package with no dependencies; it
brings Go's v1, v3, v5 and v6 UUIDs to .NET, where `System.Guid` generates v4 and v7. `jwt` is the first
module whose tests span several of its own packages, and it exercises the converted crypto stack in
real code. The next candidate:

| Module | Why it comes here | When |
|:--|:--|:--|
| [`itchyny/gojq`](https://github.com/itchyny/gojq) v0.12.19, with its dependency [`itchyny/timefmt-go`](https://github.com/itchyny/timefmt-go) v0.1.8 | The first conversion with a dependency. It gives .NET an in-process, fully managed jq, with no native binary per platform. | Q4 2026 |

gojq's full command-line conformance suite is a later second stage, once its command-line package's
own dependencies convert. Further modules follow; the
[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html) is one input to their selection.

## NuGet packages of converted modules

**In short.** A Go module that someone has already converted can arrive as a NuGet package, so your
conversion restores it and does not convert it again. The first such package is published, and the
registry that maps Go modules to their packages is live.

With `-recurse=nuget`, a converted program references the standard library, the runtime and the
source generators as published NuGet packages, and the same substitution reaches third-party modules:
the [nugetgo.net](https://nugetgo.net) community registry maps Go module paths to published NuGet
packages of their conversions, so a dependency someone has already converted becomes a package restore
instead of a local transpile. The converter side is built and on by default (see the README's
[Mapping modules to NuGet packages](README.md#mapping-modules-to-nuget-packages)). The design is
[`PLAN-nugetgo.md`](PLAN-nugetgo.md).

- **Package IDs:** a converted module publishes as `nugetgo.<dotted module path>`; the `go.` prefix is
  the converted standard library's alone.
- **Proof packages rehearse first**, to a local feed and NuGet's test gallery, before anything reaches
  nuget.org.
- **The first package is published.** [`hashset`](https://github.com/ritchiecarroll/hashset) is on
  nuget.org as `nugetgo.github.com.ritchiecarroll.hashset`, the registry lists it, and a program that
  imports it converts with `-recurse=nuget` to a project that restores the package instead of
  converting the module again. `google/uuid` and `golang-jwt/jwt` follow.
- **The package-ID prefixes are not reserved yet.** The reservation requests are with NuGet, which has
  not decided them.
- **Packages built from an unsupported Go release are labelled as proofs**, and are rebuilt once the
  converted standard library is on a supported release.

## cgo

**In short.** Go code that calls C through cgo cannot be converted yet. The converter says so and
stops, naming the file, and a bridge to native code is planned from 2027.

A Go file that imports `"C"` is not converted, and the standard library is converted with
`CGO_ENABLED=0`. The plan is [`PLAN-cgo-interop.md`](PLAN-cgo-interop.md):

- **It fails loudly today.** When a file the build selects imports `"C"`, the conversion stops with
  error `GO2CS2001`, which names the file and the cause, instead of skipping the file silently
  ([Limitations](Limitations.md)). Test-only and platform-deselected files are skipped as usual.
- **Next, a phased P/Invoke bridge** (from Q1 2027, as capacity allows): recognize and extract the C
  declarations, generate P/Invoke calls and blittable structs, then marshal pointers, strings and
  slices, starting with a slice limited to the C library. Platform order: Linux, then Windows, then
  macOS. Each target needs a C toolchain on the converting machine.

## Phase 4D: examples, fuzz seed corpora and benchmarks

**In short.** Validation counts Go's `Test` functions today. Phase 4D adds the other kinds of test a
Go package carries: examples, fuzz seeds and benchmarks. It is planned and not yet started.

Validation counts a package's `Test` functions. Phase 4D, the next validation slice, widens what
"validated" covers:

- runnable `Example` functions, with their declared output compared;
- fuzz seed corpora, run as deterministic tests before any active fuzzing is considered;
- benchmarks, compiled and executed for compatibility, with their speed kept outside the correctness
  gate.

Active fuzzing, race-detector equivalence and coverage-percentage equivalence stay outside the
correctness gate. The design kickoff is planned for Q1 2027, and the work proceeds without delaying
Phase 5. Detail:
[`TestingInfrastructureRequirements.md`](TestingInfrastructureRequirements.md#phase-4d--later-test-kinds).

## Phase 5 — Implement assembly-backed declarations in C#

**In short.** Phase 5 is about completeness, not speed. A small set of standard-library declarations
has no portable Go body, because Go implements them in assembly, in cgo or inside its runtime. Today
each of those compiles to a stub that throws a `NotImplementedException`, naming the declaration, if
it is ever called. Phase 5 gives each one a real C# body. Making portable code that already works
run faster is a different subject, under [Performance](#performance).

**What it means for you.** No validated package's tests reach a stub, and a call that does reach one
fails loudly and by name, never silently. Which exported functions can reach a stub is being
measured, and this page links that result when it is published.

The converted library reproduces Go built with `-tags purego`, so most assembly-backed code takes its
portable Go path and needs nothing from Phase 5. The declarations it is about are the ones with no Go
body at all: they are backed by assembler, cgo, runtime or compiler intrinsics, or platform services.
The `PartialStubGenerator` writes the throwing stub for each until a C# implementation exists.

**It proceeds package by package, as validation demands, not as one campaign.** When a package's own
tests need a declaration, it gets a hand-written C# body and the package validates with it. That is how
most of the surface that matters has been implemented so far, and it is how the macOS validation is
closing the gaps it finds now: `os.Executable`, the system page size, and an arm64 path for
`hash/crc32`. A declaration no validated test reaches can still be a throwing stub, and a program that
calls one stops with a `NotImplementedException` that names it.

**The remaining stubs are measured.** The
[Phase 5 stub inventory](phase4/CENSUS-phase5-stub-inventory.md) is a measured inventory of the
declarations that still compile against throwing stubs, per package and per operating system. None of
them is reached by a validated package's tests. The record carries the counts, so this page quotes
none.

**How a declaration gets its implementation.**

- The converter keeps the bodyless declaration as a `partial` method, and a hand-owned companion such
  as `*_impl.cs` supplies the body; where the Go source uses a declaration-oriented file, the
  converted `*_decl.cs` is paired with a `*_impl.cs`. `sync/atomic`'s `doc.cs` and `doc_impl.cs` are
  the model. Regenerable declaration output is never hand-edited, and the generated stub disappears
  for a member once its real implementation is present.
- Behaviorally equivalent managed code (`Interlocked`, `Volatile`, `BitOperations`, spans, BCL crypto)
  is preferred over instruction-by-instruction translation, preserving Go's overflow, alignment,
  memory-ordering, pointer and GC, and platform semantics.
- Platform implementations are isolated by target and fail clearly on unsupported targets, and a
  generic throwing stub never passes for completed platform support. Focused Go/C# behavioral
  fixtures cover declarations that upstream tests do not reach.
- Compiling alone is not completion. An implementation counts when the package's converted tests run
  against it and match Go, with no silent exclusions, and stress and edge tests are added where unit
  tests are not enough (concurrency, atomics, unsafe pointers, cryptography, platform calls).

**What closing Phase 5 means:** for every supported target, no unexplained throwing stub remains; each
implemented member has a real companion or a documented target exclusion; and every applicable package
is validated. Closing it systematically, rather than as tests demand, is unscheduled; the inventory
is what it starts from.

Phase 5 is not the "stage 5" of the trimming work under
[Native AOT and trimming](#native-aot-and-trimming); the two numberings are unrelated.

## Performance

**In short.** Converted code is usually slower than Go and uses more memory, and the published
comparison says by how much. This section lists the work planned to narrow the gap. None of it holds
up anything else on this page.

The published [performance comparison](Performance.md) measures converted C# against Go on the JIT
and under Native AOT. The follow-ups run off the critical path:

- **A re-baseline at the last Go 1.24 package release**, which is also the before
  picture for the Go 1.25 hop and the .NET 11 measurement stage. It brings the TLS-handshake
  benchmark, already in the suite, into the published table, and profiles the crypto paths behind the
  handshake gap; TLS throughput is also what retires the
  `crypto/tls` [host-limit entry](#declared-host-limits-and-their-retirement-path).
- **Allocations** (committed; design kickoff planned for Q1 2027): the retirement plan named by each
  deferred allocation-count disclosure on the roster. Go meets those assertions through compiler
  optimizations, chiefly escape analysis keeping values off the heap, that the converted code does
  not yet reproduce. The work closes the gap by construction (storage for Go array values,
  frame-local allocations kept off the heap, and ref-receiver method emission), and each disclosure
  retires when its assertion matches.
- **Interface conversions** (unscheduled, after the last Go 1.24 package release): the remaining
  items from the [interface-shell caching design](phase4/DESIGN-iface-shell-caching.md), a per-value
  shell-cache experiment and a base-class marker. A converter-emitted per-call-site inline cache
  stays deferred.
- **Stack strings**: the two remaining increments below, unscheduled.

### Expand the `sstring` stack-string MVP: the remaining increments

A non-escaping `string([]byte)` conversion emits a zero-copy `sstring` view instead of a heap copy:
for a named local, a comparison or switch operand, a concatenation operand, and a repeated conversion
hoisted to one view
([strings reference](ConversionStrategies-Reference/strings.md#a-non-escaping-stringbyte-local-emits-the-stack-string-sstring);
the completed increments are in
[Roadmap history](RoadmapHistory.md#phase-4-follow-up-deferred--expand-the-sstring-stack-string-mvp)).
Two increments remain:

- **Non-retaining callee arguments.** `string(x)` passed to a callee that never retains it, such as
  `strconv.ParseUint(string(s), …)`. A `@string` parameter copies the view back at the boundary, so
  this pays only where the callee accepts a view (an
  [`sstring` twin](ConversionStrategies-Reference/strings.md#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind)
  gives a registered function that overload), and it needs a curated list of non-retaining callees.
- **A positional, loop-carried liveness guard.** Relax "the source is never written" to "the source is
  not written between the conversion and the local's last read, and is not loop-shared". This is the
  only increment that reaches the `PerfString` hot loop and the one with the highest silent risk, so
  it gets the heaviest adversarial pass.

**Why widening is safe to try.** `sstring` is a `ref struct`, so the .NET compiler rejects almost
every over-reach (storing it in a field, array or map, boxing it, sending it on a channel, capturing
it in a closure) at build time. The only silently-wrong vectors are escape through `return` and
mutation of the source buffer during the view's lifetime, and both stay explicitly guarded. Each
increment extends the `SStringElision` behavioral test, inspects every newly emitted site in a full
standard-library reconvert, and re-runs the performance suite.

**Not pursued:** `for range string(x)`, which has no standard-library sites; and `m[string(x)]` map
keys, where a zero-allocation lookup would need a custom comparer that slows every `@string`-keyed map.

## Open converter items

**In short.** Known, smaller gaps in the converter, none of them scheduled. The validated standard
library and modules convert despite them; one matters to you only if your code uses that construct.

Smaller converter items, unscheduled. The first is recorded in the
[struct-types reference](ConversionStrategies-Reference/struct-types.md#a-global-addressed-only-by-the-packages-own-_testgo-is-still-heap-boxed);
the rest are tracked in `src/go2cs/ToDo.md`:

- **Cross-package address of a global.** `&otherpkg.Var` does not box the global, because the
  addressed-global scan covers only the package under conversion.
- **The remaining dynamic-struct implicit-cast checks** in the `AssignStmt`, `CompositeLit`,
  `IndexExpr`, `BinaryExpr`, `UnaryExpr`, `SelectorExpr`, `TypeSwitchStmt` and `ValueSpec` visitors.
- **Complete comment conversion**, free-floating comments included, which depends on
  [golang/go#20744](https://github.com/golang/go/issues/20744) or the DST package.
- **Go assembler (`.s`) targets.** One idea is to compile them to object code and wrap it as a native
  library; the standard library's pure-Go paths cover the need in the meantime.
- **Accessibility "publicizing" as a source-generator pass**, a cosmetic change.

## Timeline

**In short.** The order the work on this page is planned in. Few rows carry a date; most wait on
something else finishing first.

| When | Planned |
|:--|:--|
| Now | macOS validation of the standard library, package by package. The trimming work for Native AOT. Further converted modules as NuGet packages: `uuid` and `jwt` after `hashset`. |
| Next | At least one more Go 1.24 package release, carrying that work. The performance re-baseline at the last one. |
| When the project owner closes the Go 1.24 line, no earlier than November 2026 | The Go 1.25 hop. |
| After the Go 1.25 hop | The .NET 11 measurement stage (.NET 11 ships in November 2026). The Go 1.26 hop, as a separate hop. `timefmt-go` and `gojq`, as their prerequisites land. The generic-methods design for Go 1.27. The decision on how often later hops run. |
| Q1 2027 | Go 1.27 hop, by about February. The cgo bridge's first phases (C library only; Linux, then Windows, then macOS). The Phase 4D design kickoff. The allocation work's design kickoff. |
| Unscheduled | Closing Phase 5 systematically, from its inventory. Interface-conversion performance work. The remaining stack-string increments. Open converter items. |

## How progress is tracked

**In short.** Where to check each claim on this page for yourself.

| What | Where |
|:--|:--|
| Validation | [Validated Test Packages](ValidatedTestPackages.md), the authoritative roster: each package's verdict counts on Windows and Linux, its macOS count once it has one, its disclosures with their stated reasons, and its proof page. Its header carries the current totals. |
| Third-party modules | [Validated Modules](ValidatedModules.md): each validated module, its version, the platforms it was measured on, and its proof pages. |
| Compilation | The whole converted standard library builds with zero errors (`src/go2cs-stdlib.slnx`) for Windows and Linux; the macOS compile runs daily in CI ([`CIMatrix.md`](CIMatrix.md)). |
| The converter | Hundreds of Go-vs-C# behavioral projects under `src/tests/Behavioral`, each transpiled, compiled, compared against a committed golden and, where it is a runnable program, output-compared against Go. |
| Performance | The [performance comparison](Performance.md). |
| Releases | [News](NEWS.md). |

Every converter change runs the same loop: edit `src/go2cs/*.go` → `go build` (with the Go release
`src/go2cs/go.mod` names) →
re-transpile → `dotnet build`, and a whole-corpus reconvert shows no unintended drift before the change
lands.
