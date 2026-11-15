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
     a placeholder for the measurement of which exported functions can reach a stub.
     Amended the same day at master ea47fdd770: the placeholder becomes the link to
     docs/phase4/CENSUS-phase5-exported-reach.md with its findings in words, and the Phase 5 detail and
     the Timeline's Unscheduled row follow the owner's priority ruling (ledger 2026-10-10 10:15 and
     11:33): easily reached stubs first, the unreached ones when a real use needs one.
     2026-10-26: re-read at a6214f0c5d for the 1.26.10.1 release: the four exported calls the reach census
     confirmed now have bodies (internal/coverage/cfile/emit_impl.cs, runtime/stubs_impl.cs breakpoint,
     crypto/x509/internal/macos/darwin/corefoundation_impl.cs, and runtime/pprof's mach_vm_region forwarded
     through runtime/darwin/libccall_impl.cs in 7266441cf5); both Phase 5 censuses and ValidatedModules.md
     are go1.24.13 readings, so the page says so; the TestBogoSuite entry is bound by per-process start-up
     (about 10 s a launch against Go's 85 ms, the crypto/tls roster row), not throughput, and that start-up
     work is now a Performance item; the re-baseline runs on the published 1.26.10.1 packages against
     1.24.13.7 (PLAN-perf-rebaseline.md section 1); the release AOT check publishes and runs osx-arm64
     (os-matrix.yml aot-smoke, osx-x64 read NOT MEASURED on its bound); the macOS gaps named under Phase 5
     shipped in 1.24.13.6 (NEWS.md, October 12). -->

<!-- Refreshed 2026-11-07 at master 499c30c95d (owner request: "This probably needs a refresh"), G. Every change is read
     from a committed record or the mailbox ledger (docs/phase4/LEDGER.md on claude/mailbox), cited here:
     (1) Phase 4, macOS: the -tests oracle runs at the row's own GOARCH (batch 208, C1 oracle arch), and a row whose two
         chips differ banks the stricter reading with the difference disclosed (C1 AES ruling (1), ledger 2026-11-06 06:31;
         applied from the 1.26.10.3 re-bank; crypto/subtle is the named instance). The roster's darwin provenance still
         states the agree-only rule it banked under.
     (2) Go releases: the version-line policy (master carries the current Go line; the next minor is prepared on a hop
         branch; no Go 1.25 line), and 1.26.10.3 in preparation (docs/PLAN-rebank-1-26-10-3.md on claude/g-dot3-prep).
     (3) Host limits: TestBogoSuite's figure is DATA-bogo-startup.md §3 (1.18 s per launch, linux-x64, one 4-core box;
         Go about 85 ms) and its §7 (the remaining lever is the host's start-up: about 0.95 s of it loads about 240
         assemblies), replacing "about 10 s". The roster's crypto/tls class text (throughput-bound, 2026-08-28) predates
         that record; it is left to the re-bank, not edited here.
     (4) Native AOT and trimming: a 1.26.10.3 bullet from DATA-hop126-perf.md §11.7 and §11.9 (slice reads, the zero-size
         check, the 64-bit multiply), DATA-extension-host-list.md (the first interface assertion) and §11.8 plus batch
         214 (single-file per-assembly ReadyToRun); the default instruction set from §11.6 (x86-64-v3 measured no suite
         gain; the owner's ruling, ledger 2026-11-06).
     (5) Performance: the comparison is published at 1.26.10.2 (docs/Performance.md, batch 170), so the re-baseline
         bullet becomes that; the start-up bullet takes DATA-bogo-startup.md and the levers that shipped (batches 206,
         210, 218, 219); allocations: the stack-allocation parity design is written (docs/DESIGN-stack-allocation.md,
         PROPOSED) and the owner ruled the work post-hop (ledger 2026-10-21 09:55); goroutine scheduling is new, from
         docs/DESIGN-mn-scheduler-sizing.md (PROPOSED, its rulings asked) over the landed cooperative scheduler
         (phase4/DESIGN-cooperative-scheduler.md); the source-reference build fix (DATA-hop126-perf.md §11.9; batch 223
         landed, H's .slnx fix pending).
     (6) Timeline: the 1.26.10.1 re-baseline leaves "Next" (published); 1.26.10.3 and the start-up work join "Now";
         goroutine scheduling joins "Unscheduled"; the allocation row stays in Q1 2027, now "whose design is written"
         where it read "design kickoff" (the owner is asked whether Q1 2027 still holds).
     Not changed, re-read and still true: Where things stand; How the project moves; .NET; Platforms; Real-world Go
     modules (five, Go 1.24.13 readings, ValidatedModules.md); NuGet packages; cgo; Phase 4D; Phase 5; IDE mode; Open
     converter items (the cross-package address-of-global bullet matches the struct-types reference and Limitations at
     this master); How progress is tracked.
     The owner answered the three questions this refresh raised (2026-11-07, relayed by COORD): the allocation work
     "Keep Q1 2027"; gojq and timefmt-go "Keep Q4 2026"; goroutine scheduling "List as unscheduled". The page holds
     all three as drafted. -->

<!-- Tightened 2026-11-15 at master 091935257c (P4, owner request relayed by COORD: "a more concise read", with the cgo
     items grouped in one section). Every change is read from a committed record or the mailbox:
     (1) New "The next two releases" section: the owner's scope rulings of 2026-11-14 (COORD -> FLEET 20261114T234509Z):
         1.26.11.1 is gated by the sqlite-cs demo and the TypeScript emit demo (rulings 1 and 2), carries cgo Phases 1-2
         plus 3.1 and the pin-extent fix (ruling 4); the release after it carries M:N scheduling, cgo against the go.*
         packages, cgo 3.2 with the net resolver as the real-package proof, door C unless the sqlite demo needs it, and
         the start-up method-set items A1/A3 (ruling 6); lever E as a .5 option (i9's BOARD row, announced
         20261115T074730Z-i9, not yet on master). No date is given for either release; none is ruled.
     (2) cgo: the scattered cgo items (Timeline, Limitations links) gathered into the cgo section, split into what
         converts, what the next release adds, and what comes later.
     (3) Real-world Go modules: nine modules validate with Go 1.26.10 (ValidatedModules.md, refreshed in the same docs
         seat), so gojq and timefmt-go leave the "next candidate" table; the table is gone.
     (4) Platforms: linux-arm64 is validated (the roster's Linux line: 236 of 236 banked at linux-arm64) and shipped
         (KnownIssues.md, "Platforms the packages do not ship"); a build for an unshipped platform is refused or warned
         about there, replacing "other architectures receive the Windows flavor".
     (5) Figures no standing process re-measures are stated in words; the comments keep the numbers.
     (6) History dropped from the visible text (the 1.24 and 1.25 support dates, "from release N on"); it is in
         RoadmapHistory.md, NEWS.md and the records cited. -->

This page is the plan for what comes next in go2cs: the Go and .NET releases the converted standard
library moves to, the platforms it runs on, the real-world Go code it converts beyond the standard
library, and the runtime and converter work still ahead. The completed phases are in
[Roadmap history](RoadmapHistory.md). Dates are month or quarter targets.

**Where things stand.** Every implementable package of the converted standard library validates
against Go's own tests on Windows and Linux, and the same validation is under way on macOS, package
by package. `go2cs -tests -recurse` validates third-party modules against their own tests the same way,
and converted modules are published as NuGet packages, listed on [nugetgo.net](https://nugetgo.net).
[Validated Test Packages](ValidatedTestPackages.md) carries the current state of the standard library, and
[Validated Modules](ValidatedModules.md) that of third-party modules.

**What that rests on.** Hundreds of Go-versus-C# behavioral test projects guard the converter: each is
transpiled, compiled and compared against a committed expected output, and each runnable one is run and
compared against Go. The whole standard library compiles as .NET assemblies. It reproduces Go built with
`-tags purego`, since a managed runtime cannot run Go's hand-written assembly
([Conversion Strategies](ConversionStrategies.md#the-standard-library-reproduces-go--tags-purego)).
Compiling is not the same as running correctly, which is what Phase 4 is for.
<!-- "hundreds" (697 at the 2026-09-26 count) counted at master db1bd885a2: 697 directories under src/tests/Behavioral
     carry a committed .cs.target golden; the two harness directories (BehavioralRunner, BehavioralTests)
     carry none and are not counted. -->
<!-- Moved here from the README's Status section. The prose quotes no counts. They were 225 of 225 and 225 of 230 on 2026-10-01 at master
     c2591d5b95, read from the Phase 4 progress header of docs/ValidatedTestPackages.md, which carries
     the current figures. -->

## The next two releases

**In short.** 1.26.11.1, built from Go 1.26.11, ships when two real programs convert and run exactly as Go
runs them. The release after it adds the rest of the cgo work, goroutine scheduling and Native AOT work that
did not fit. Neither has a date: completeness comes first.

**1.26.11.1** ships when both of these hold:
- **SQLite.** The converted `modernc.org/sqlite` opens a database, creates a table, inserts and selects, with
  output byte-identical to Go's, on Windows and on Linux.
- **The TypeScript compiler.** The converted TypeScript compiler (written in Go) emits JavaScript byte-identical to
  what the Go build emits, at least under the JIT.

It also carries the cgo work under [cgo](#cgo) as "What converts".

**The release after 1.26.11.1** is planned to carry:
- goroutine scheduling, many goroutines over a few threads ([Performance](#performance)), which lands on the Go 1.26
  line, before the corpus moves to the next Go minor release;
  <!-- Owner ruling 2026-11-12, given in session to COORD (relayed 20261115T083540Z): M:N lands on the Go 1.26 line,
       then one Go minor hop; 1.26 patch hops continue meanwhile. -->
- the next cgo steps ([cgo](#cgo));
- viewing a Go array through a pointer to an array of another type, when that pointer is kept, unless the SQLite
  program needs it first, in which case it moves into 1.26.11.1;
- start-up work on how converted types find their methods ([Performance](#performance));
- as an option, a runtime-library change that cuts the memory and image size of a Native AOT publish of a large
  program ([Native AOT and trimming](#native-aot-and-trimming)).

## Phase 4: validation against Go's own tests

**In short.** Phase 4 checks the converted standard library against Go's own tests, package by package,
so that "it works" is measured and not claimed. It is done on Windows and Linux, under way on macOS, and
has one planned extension.

- **Phases 4A to 4C are complete.** The test pipeline, subtests and parallelism, and the package-by-package
  validation are done: every implementable package validates on Windows and on Linux, x64 and arm64
  ([Roadmap history](RoadmapHistory.md#phase-4--convert-and-run-go-package-tests)).
- **macOS parity is in progress.** A roster row gains its macOS count when the package validates on both Apple
  silicon and Intel, each chip compared with Go built for that chip. The [roster](ValidatedTestPackages.md) rows
  carry the live state; see [Platforms](#platforms).
- **Phase 4D is planned.** It widens validation to examples, fuzz seed corpora and benchmarks
  ([below](#phase-4d-examples-fuzz-seed-corpora-and-benchmarks)).

## How the project moves: release hops

**In short.** go2cs follows Go and .NET one release at a time, and every move proves every package
again. Each go2cs release names exactly one Go release and one .NET release.

The project advances by release migrations, called [hops](Glossary.md#hop): moving the converted
standard library (the *corpus*) to a new Go release or to a new .NET runtime, never both in one hop. Each
Go hop re-validates every package against that release's own tests.
<!-- How a hop is run: [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md); the step-by-step procedure:
     [`GoCorpusMigration.md`](GoCorpusMigration.md) for a Go release and
     [`DotNetMigration.md`](DotNetMigration.md) for a .NET one. Internal records, unlinked from the visible
     text by the 2026-10-26 owner ruling (OQ-1) on user-facing pages. -->

## Go releases

**In short.** The converted standard library is on Go 1.26.11, a release Go supports. The next move is
Go 1.27, before the 1.26 series leaves support.

The published packages are 1.26.10.3, from Go 1.26.10, and 1.26.11.1 is next
([The next two releases](#the-next-two-releases)). The main line of development carries the current Go line,
and the next Go minor is prepared separately until its packages ship. Go 1.24 and 1.25 have no further
releases: packages built from either carry none of the Go security fixes issued since.
<!-- Go 1.24 left upstream support in February 2026 and 1.25 in August 2026; 1.24.13.7 is the last 1.24 release, and
     there is no Go 1.25 release (docs/PLAN-version-lines.md). -->

| Hop | When | What it involves |
|:--|:--|:--|
| **Go 1.27** | Q1 2027, by about February | Generic methods, which need a conversion design first. The 1.26 series leaves support when Go 1.28 ships, around February 2027. |

How often later hops run is decided ahead of the Go 1.27 hop.

## .NET

**In short.** Converted code targets .NET 10, a long-term-support release, until .NET 12. You need the
.NET 10 SDK, and a newer SDK on your machine does not change what you get.

**The published target stays `net10.0`** until `net12.0`, the next long-term-support release. .NET 10 is
supported until November 2028.

**The SDK and the C# language version are pinned.** A repository `global.json` pins the SDK, and every
project, the converter's template included, fixes `LangVersion`, so a newer default SDK does not compile
converted code under a newer C#. The converter also escapes Go identifiers that newer C# gives new meaning,
such as `closed`, `union` and `safe` when they name a type.

**A .NET 11 measurement stage follows the Go 1.26 hop**, once .NET 11 has shipped (November 2026), and
never overlaps a Go hop. It compares JIT and allocation behavior on .NET 11 over identical code and reviews
the deployment shape, the full-trim publish included ([Native AOT and trimming](#native-aot-and-trimming)).
Its findings inform the move to `net12.0`.
<!-- plan: [`PLAN-bflat-perf-exploration.md`](PLAN-bflat-perf-exploration.md) (internal record, unlinked
     2026-10-26, OQ-1) -->

### Declared host limits and their retirement path

The roster's **`host-limit`** disclosure class, defined in
[`ValidatedTestPackages.md`](ValidatedTestPackages.md), covers tests that rest on a property of the test
*binary* the converted host's deployment shape lacks. Each entry names that property, never a fixable defect,
and retires itself when the shape gains it: disclosed tests keep running, so an entry that starts passing
breaks the roster's arithmetic loudly and is removed. The test host publishes as a self-contained, relocatable
single-file executable, so Go tests that copy and relaunch their own binary pass on Windows and Linux.

Entries remain in `crypto/tls`, `os`, `os/exec` and `syscall`, and the roster's class text names what each
needs. In `crypto/tls`'s `TestBogoSuite`, BoringSSL's runner starts a converted helper process once per case
inside Go's own 10-minute deadline, and each converted launch costs many times Go's (`linux-x64`), most of it
loading packages. That entry retires when the start-up work under [Performance](#performance) brings the run
inside the deadline.
<!-- DATA-bogo-startup.md §3: about 1.2 s per launch against Go's about 85 ms, linux-x64, one 4-core box; §7: the
     remaining lever is the host's start-up. -->

## Native AOT and trimming

**In short.** You can publish converted Go as a native executable today, and it runs, but the publish
compiles every converted package it references in full. This work makes that publish smaller, quicker and
free of warnings, in stages.

A converted program, and a C# program that references the `go.*` packages, can be published with Native
AOT. Done so far:

- Every package registers the Go types it defines. A Native AOT publish runs with the default trim, and the
  release check measures it. A fully trimmed build (`TrimMode` set to `full`) runs (`win-x64`).
- `reflect` works under Native AOT without generating code at run time: the source generators write the
  operations it needs at build time. Under a full trim, `fmt` finds a value's own `String` and `Error` methods,
  and reflection reaches a struct's fields through a pointer.
- Reading a slice element and a 64-bit multiply with a 128-bit result inline under Native AOT, as under the JIT.
  The first type assertion to an interface a type satisfies without declaring it reads a list written at build
  time. A single-file publish compiles ReadyToRun one assembly at a time, and starts several times faster than
  the SDK's compressed composite image (`win-x64`).
  <!-- About 0.47 s against about 3.5 s for a small converted program, win-x64, one development machine
       (DATA-hop126-perf.md §11.8, batch 214). -->

**Later stages** cover method sets and stack frames, and, as an option for the release after 1.26.11.1, a
runtime-library change that cuts the memory and image size of the publish of a large program.

Converted packages are built for the default x64 instruction set: `x86-64-v3` gives no gain across the
benchmark suite on `win-x64`, and an image built that way does not start without AVX2.

The release check publishes a program with Native AOT and runs it on `linux-x64`, `win-x64` and
`osx-arm64`. On `osx-x64` the publish is read when it finishes inside the hosted runner's time limit, and
reported as not measured when it does not.

**The cost today is real.** With the default trim, Native AOT compiles every `go.*` assembly a program
references in full, which costs publish time and executable size
([Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)).
A build, `dotnet run` and a publish without Native AOT are not affected.

## Platforms

**In short.** go2cs is validated on x64 Windows and on Linux, x64 and arm64. macOS, on Intel and on Apple
silicon, has packages and is being validated package by package. Other platforms are not supported yet.

**Windows and Linux.** The converter runs on both, and the standard library is multi-target on disk:
platform-shared files sit flat, and a package whose Go source varies by platform keeps one folder per target.
<!-- design: [design](phase4/DESIGN-multiplatform-corpus.md) --> Each platform-varying package ships `win-x64`,
`linux-x64` and `linux-arm64` flavors, and roster rows record each Linux flavor's verdict counts beside the
Windows record. A few Linux operational items remain open.
<!-- Remaining Linux operational items are tracked in [`PLAN-linux-operation.md`](PLAN-linux-operation.md)
     and the multi-target design (internal records, unlinked 2026-10-26, OQ-1). -->

**macOS (Intel and Apple silicon):** packages ship for both chips, the behavioral suite passes on both,
and the README walkthrough runs. A roster row gains its macOS count only when both chips agree with Go;
until a package has its count, don't expect it to be fully operational there. The work finds macOS-only gaps,
each fixed as it is found. The compile is checked daily in CI on macOS arm64 and x64 runners
([`CIMatrix.md`](CIMatrix.md)).

**Other platforms.** A build for a platform the packages do not ship is refused, or, on `win-arm64`, warned
about ([Known issues](KnownIssues.md#platforms-the-packages-do-not-ship)).

## Real-world Go modules

**In short.** Beyond the standard library, go2cs converts third-party Go modules and holds each to the
same bar: its own tests must match Go's. The modules on [Validated Modules](ValidatedModules.md) pass,
measured with Go 1.26.10, and two large programs gate the next release.

Each module is converted, validated against its own test suite exactly as a standard-library package is, and
given a proof page beside the conversion. `go2cs -tests -recurse` runs a module's tests across all of its
packages, and a validated module packs as one NuGet package.

The validated modules include [`google/uuid`](https://github.com/google/uuid), which brings Go's v1, v3, v5
and v6 UUIDs to .NET (where `System.Guid` generates v4 and v7); [`golang-jwt/jwt`](https://github.com/golang-jwt/jwt)
v5, which exercises the converted crypto stack; [`spf13/cobra`](https://github.com/spf13/cobra) with its
dependencies `pflag` and `mousetrap`; and [`itchyny/gojq`](https://github.com/itchyny/gojq) with
[`itchyny/timefmt-go`](https://github.com/itchyny/timefmt-go), an in-process, fully managed jq. gojq's
command-line package and its conformance suite come later, once that package's own dependencies convert.

`modernc.org/sqlite` and the TypeScript compiler gate 1.26.11.1
([The next two releases](#the-next-two-releases)). Further modules follow, libraries first: a converted module
is most useful as Go code a .NET program calls. The [go2cs Target Atlas](https://go2cs.net/TargetAtlas.html) is
one input to their selection.

## NuGet packages of converted modules

**In short.** A Go module that someone has already converted can arrive as a NuGet package, so your
conversion restores it and does not convert it again. [nugetgo.net](https://nugetgo.net), the registry that maps
Go modules to their packages, lists the published ones.

With `-recurse=nuget`, a converted program references the standard library, the runtime and the source
generators as published NuGet packages, and the same substitution reaches third-party modules: a dependency
someone has already converted becomes a package restore instead of a local transpile
([Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages)). The converter side is
on by default.
<!-- The design is [`PLAN-nugetgo.md`](PLAN-nugetgo.md) (internal record, unlinked 2026-10-26, OQ-1). -->

- **Package IDs:** a converted module publishes as `nugetgo.<dotted module path>`; the `go.` prefix is
  the converted standard library's alone.
- **A package rehearses first**, on a local feed, before it reaches nuget.org.
- **The package-ID prefixes are not reserved yet.** The requests are with NuGet, which has not decided them.
- **Packages built from an unsupported Go release are labelled as proofs**, and are rebuilt once the
  converted standard library is on a supported release.

## cgo

**In short.** `go2cs -cgo` converts a Go package that calls C library functions, on a machine with a C
compiler. Next come Go memory handed to C, cgo packages built against the `go.*` packages, and a real
standard-library cgo package; C code written inline in the Go file and callbacks from C into Go follow.
[Limitations](Limitations.md#cgo) has the detail of what converts and what is refused.

The standard library is converted with `CGO_ENABLED=0`.
<!-- The plan is [`PLAN-cgo-interop.md`](PLAN-cgo-interop.md) (internal record, unlinked 2026-10-26, OQ-1). -->

**What converts** (cgo Phases 1 and 2 and the first step of Phase 3, in 1.26.11.1):
- One package, when every C use it makes is a call of a C function a shared library defines. Integers,
  floating-point numbers, pointers, and structs of integers and pointers cross, as do a `void *` that is nil or
  points at C memory, the `C.CString` family, `C.malloc`, `C.free` and `defer C.f(…)`. It converts for the
  machine's own operating system and architecture, and a program that uses it needs only the C library.
- A converted cgo program printed what the Go build printed on linux/amd64, darwin/amd64, darwin/arm64 and
  windows/amd64, apart from the differences [Limitations](Limitations.md#cgo) lists.
- Without `-cgo`, a file the build selects that imports `"C"` stops the conversion with error `GO2CS2001`, which
  names the file. Test-only and platform-deselected files are skipped as usual.
<!-- 2026-11-14 (H, the release-notes audit): cgo Phase 2's real route and G1 gate (docs/phase4/DESIGN-cgo-phase2-pinvoke.md
     section 11.22: hosted legs on linux/amd64, darwin/amd64, darwin/arm64 and windows/amd64) and Phase 3 increment 3.1
     (43ac9e33fa, batch 305; DESIGN-cgo-phase3-marshaling.md section 16; its hosted cgo-e2e legs 43638330006 linux-x64,
     43638338397 win-x64, 43638346986 osx-arm64 and osx-x64, each 50 of 50 arms as Go does or refused as the arm's table
     says). The owner's scope ruling 4 (2026-11-14): 1.26.11.1 = Phases 1-2 + 3.1 + H's pin-extent fix. -->

**Next, in the release after 1.26.11.1:**
- Go memory handed to C without a copy, and a slice viewed over C memory (the rest of Phase 3).
- A cgo package built against the `go.*` packages, so a `-recurse=nuget` conversion can include one.
- A real standard-library cgo package, the `net` resolver, converted and validated as the proof.

**Later:** C code written inline in the Go file (Phase 4), then `//export` and callbacks from C into Go
(Phase 5). Until each lands, it is refused by name, with the phase that covers it.

## Phase 4D: examples, fuzz seed corpora and benchmarks

**In short.** Validation counts Go's `Test` functions today. Phase 4D adds the other kinds of test a
Go package carries. It is planned and not yet started.

Phase 4D widens what "validated" covers:

- runnable `Example` functions, with their declared output compared;
- fuzz seed corpora, run as deterministic tests before any active fuzzing is considered;
- benchmarks, compiled and run for compatibility, with their speed outside the correctness gate.

Active fuzzing, race-detector equivalence and coverage-percentage equivalence stay outside the
correctness gate. The design kickoff is planned for Q1 2027, without delaying Phase 5.
<!-- Detail:
     [`TestingInfrastructureRequirements.md`](TestingInfrastructureRequirements.md#phase-4d--later-test-kinds)
     (internal record, unlinked 2026-10-26, OQ-1). -->

## Phase 5 — Implement assembly-backed declarations in C#

**In short.** Phase 5 is about completeness, not speed. A small set of standard-library declarations
has no portable Go body, because Go implements them in assembly, in cgo or inside its runtime. Each
compiles to a stub that throws a `NotImplementedException` naming it, if it is ever called. Phase 5 gives
each one a real C# body. Making working code faster is under [Performance](#performance).

**What it means for you.** No validated package's tests reach a stub, and a call that does reach one
fails loudly and by name. Which exported functions can reach one is measured, on the Go 1.24 standard
library, and every call that measurement confirmed reaches a real body.
<!-- the measurement: [exported-reach record](phase4/CENSUS-phase5-exported-reach.md)
     (internal record, unlinked 2026-10-26, OQ-1). Bodies given since: clearing coverage counters in a program built
     without coverage, runtime.Breakpoint, and on macOS certificate verification against the system roots and CPU
     profiling. -->

**It proceeds package by package, as validation demands.** When a package's own tests need a declaration,
it gets a hand-written C# body, as `os.Executable`, the system page size and an arm64 path for
`hash/crc32` did for macOS. A measured inventory lists the declarations that still compile against throwing
stubs, per package and per operating system, on the Go 1.24 standard library; no validated package's tests
reach any of them.
<!-- The inventory: [Phase 5 stub inventory](phase4/CENSUS-phase5-stub-inventory.md) (internal record,
     unlinked 2026-10-26, OQ-1). The record carries the counts, so this page quotes none. -->

**How a declaration gets its implementation.**

- The converter keeps the bodyless declaration as a `partial` method, and a hand-owned companion such
  as `*_impl.cs` supplies the body (`sync/atomic`'s `doc.cs` and `doc_impl.cs` are the model). Generated
  output is never hand-edited, and the stub disappears once the real body is present.
- Behaviorally equivalent managed code (`Interlocked`, `Volatile`, `BitOperations`, spans, BCL crypto)
  is preferred over instruction-by-instruction translation, keeping Go's overflow, alignment,
  memory-ordering, pointer, GC and platform semantics.
- Platform implementations are isolated by target and fail clearly on unsupported targets; a generic
  throwing stub never passes for platform support. Focused behavioral fixtures cover declarations that
  upstream tests do not reach.
- Compiling is not completion: an implementation counts when the package's converted tests run against it
  and match Go, with stress and edge tests where unit tests are not enough.

**What closing Phase 5 means:** for every supported target, no unexplained throwing stub remains; each
implemented member has a real companion or a documented target exclusion; and every applicable package
is validated.

**What is closed first.** A stub an ordinary program can easily reach comes first. The stubs no exported
call reaches stay, by name, until a real use needs one; closing those has no date.
<!-- [exported-reach record](phase4/CENSUS-phase5-exported-reach.md) (internal record, unlinked
     2026-10-26, OQ-1) -->

Phase 5 is not the "stage 5" of the trimming work; the two numberings are unrelated.

## Performance

**In short.** Converted code is usually slower than Go and uses more memory, and the published
comparison says by how much. This section lists the work planned to narrow the gap. None of it holds
up anything else on this page.

The [performance comparison](Performance.md) measures converted C# against Go on the JIT and under Native
AOT, on the published 1.26.10.2 packages, and [Tuning converted code](Tuning.md) says which deployment and
runtime settings to choose. It is the before picture for the .NET 11 measurement stage.

- **Start-up cost of a converted process** (in progress). A converted process pays .NET's process start
  and loads its converted packages each time; a converted `crypto/tls` helper takes many times Go's time to
  start and run one handshake (`linux-x64`), most of it loading packages. How converted types find their
  methods at start-up is planned for the release after 1.26.11.1. This cost also keeps the `crypto/tls`
  [host-limit entry](#declared-host-limits-and-their-retirement-path) open.
  <!-- DATA-bogo-startup.md; the B3 items A1/A3 (start-up method sets) are .5 by the owner's ruling 6 of 2026-11-14. -->
- **Goroutine scheduling** (planned for the release after 1.26.11.1): each live goroutine runs on its own
  OS thread today, so a program can hold only as many goroutines alive at once as the system allows threads,
  a limit macOS reaches first ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)).
  The work moves toward Go's model of many goroutines over a few threads.
  <!-- Owner ruling 2026-11-08, relayed by COORD, verbatim: "In addition to the TypeScript issues, M:N scheduling
       right after .3 sounds appropriate." The trigger is C1's macOS thread-cap measurement (run 42497561630; see the
       Limitations bullet's comment). The design starts from docs/DESIGN-mn-scheduler-sizing.md and
       phase4/DESIGN-cooperative-scheduler.md. Its async scope, as the owner last refined it on 2026-11-08 (relayed by
       COORD; spelling corrected, wording his), superseding both the 2026-10-24 "async out" position and the same
       morning's "out of visible code, in scope inside golib and generated code": "I don't want to wholesale exclude
       ideas related to async: if there are suggestions, even if impactful to visual parity, they should be 'weighed',
       but the idea will be measured based on project tenets, parity versus behavior, for example. If behavior can be
       better matched at some visual parity expense, it will get a strong consideration." So no async option is
       excluded at any level, visible emission included. The design weighs each one: the behavioral-parity gain (which
       Go semantics it matches better), the visual-parity cost (what a reader sees change), and the other tenets' costs.
       A behavior gain bought with visual cost gets strong consideration; golib and generated-code async remains the
       cheapest level where it suffices. Previously "unscheduled" (owner, 2026-11-07: "List as unscheduled").
       2026-11-14 (owner's scope ruling 6): M:N is in the release after 1.26.11.1. -->
- **Allocations** (committed; planned for Q1 2027): the retirement plan named by each deferred
  allocation-count disclosure on the roster. Go meets those assertions through escape analysis and other
  compiler optimizations the converted code does not yet reproduce. The work closes the gap by construction
  (storage for Go array values, frame-local allocations kept off the heap, ref-receiver method emission), and
  each disclosure retires when its assertion matches. Its design, stack-allocation parity, is written.
- **Interface conversions** (unscheduled): the remaining interface-shell caching items, a per-value
  shell-cache experiment and a base-class marker; a converter-emitted per-call-site inline cache stays
  deferred.
  <!-- items from the [interface-shell caching design](phase4/DESIGN-iface-shell-caching.md) (internal
       record, unlinked 2026-10-26, OQ-1) -->
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

## IDE mode (an experiment)

A gated experiment in writing Go in an editor and running and debugging it as .NET, with the debugger
showing Go values the way a Go developer expects. Its feasibility is tested first, and nothing in it is
a commitment yet.
<!-- The plan: [IDE mode](PLAN-ide-mode.md) (internal record, unlinked 2026-10-26, OQ-1). -->

## Open converter items

**In short.** Known, smaller gaps in the converter, none of them scheduled. The validated standard
library and modules convert despite them; one matters to you only if your code uses that construct.

The first is described in [Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go);
the rest are tracked in `src/go2cs/ToDo.md`:

- **The address of another package's pointer- or interface-typed variable**, such as `&otherpkg.ErrMissing`,
  or of a variable whose storage lives somewhere else, such as `runtime.MemProfileRate`, points at a copy, so
  writes through it are lost
  ([Known issues](KnownIssues.md#writes-through-the-address-of-another-packages-pointer-or-interface-variable-are-lost)
  gives the workaround).
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
| Now | 1.26.11.1: the SQLite and TypeScript programs that gate it. macOS validation of the standard library. The trimming work for Native AOT. The start-up cost of a converted process. |
| Next | The release after 1.26.11.1: goroutine scheduling and the next cgo steps ([The next two releases](#the-next-two-releases)). The .NET 11 measurement stage (.NET 11 ships in November 2026). The generic-methods design for Go 1.27. The decision on how often later hops run. |
| Q1 2027 | Go 1.27 hop, by about February. The Phase 4D design kickoff. The allocation work, whose design is written. |
| Unscheduled | Inline C and callbacks from C (cgo Phases 4 and 5). The Phase 5 stubs that no exported call reaches, each closed when a real use needs it. Interface-conversion performance work. The remaining stack-string increments. Open converter items. |

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
