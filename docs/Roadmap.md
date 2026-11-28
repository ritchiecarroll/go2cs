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

<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2 (owner items UX-1, UX-4, UX-8; owner rule 2026-11-22: the page
     leads with the current state, and text about older versions moves to a section at the bottom). Every change is
     read from a committed record at that master:
     (1) UX-1: 1.26.11.1 shipped on 2026-11-19 with both of its gates met (NEWS.md, the November 19 entry; tag
         nuget-1.26.11.1, ec3e170e4b). "The next two releases" becomes "1.26.11.1, and the release after it" (the old id
         is kept on the heading for any outside bookmark; no page in docs/, README.md or src/ links it): what 1.26.11.1
         shipped in two sentences, what master carries since (git log --first-parent nuget-1.26.11.1..97cca467a2, trains
         1 to 7), and what is not on master yet. The release after 1.26.11.1 has no version number or date, and none is
         given. Each item of the old "release after 1.26.11.1" list, checked at this master:
         - goroutine scheduling: ON MASTER as an experimental, opt-in M:N scheduler, off by default (KnownIssues.md, the
           M:N entry; docs/phase4/DESIGN-mn-n5-invisible-blocking.md section 4 "stays OFF by default"); the work goes on
           (trains 3 to 7, the g-mn-* merges);
         - the next cgo steps: PARTLY ON MASTER. -recurse -cgo and -recurse=nuget -cgo (8de0f7ece2, f74b4d7385, train 3;
           Limitations.md and CommandLine.md -cgo), a -cgo pack for four RIDs that is not pushed until a published go.lib
           carries the run-time target check (ef9be9bd32 pack-2b, c9143ae615 a6, 54b02b555d gopointer; NugetgoPublish.md
           "Cgo"; docs/phase4/REHEARSAL-cgo-nuget-e2e-go-pointer.md), and reading C memory through a C struct's pointer
           field and through `**C.T` (1f2a9afdfc sb2a, 5f2f2d0aa3 1c; DESIGN-cgo-phase3-marshaling.md 18.9 and 18.11).
           NOT ON MASTER: Go memory handed to C without a copy (1b) and the net resolver, which the owner placed after
           the next release (DESIGN-cgo-phase3-marshaling.md 18.9, 18.10);
         - the kept array pointer (door C): ON MASTER for an unnamed array type (52d3547def, train 1;
           src/go2cs/keptArrayDoor.go; DESIGN-array-pointer-doors.md section 20). It did not move into 1.26.11.1. The
           named form is not taken;
         - start-up method sets: arm A1 ON MASTER, a gain under the JIT only (2506d6d48f; the BOARD row of 2026-11-18;
           DATA-startup-method-sets-a1.md); the Native AOT half and arm A3 are not;
         - lever E (the Native AOT option): ON MASTER (61b23a9888, train 1; its BOARD row).
         The Timeline's Now and Next rows are rewritten to match.
     (2) UX-4: the cgo section now agrees with Limitations.md's cgo section and with NugetgoPublish.md at this master
         (-recurse -cgo, cross-target -platforms with a target C compiler, the pack and its push gate). Limitations.md's
         "Packing a cgo package against the go.* packages for every platform is still to come" is stale on one point
         (packing exists; publishing waits); reported to the docs pass, not edited here.
     (3) UX-8: every "the release after 1.26.11.1" (old lines 163, 279, 385, 471, 474) is restated in present tense
         (what is on master) or names 1.26.11.1 as the release it follows.
     (4) Go releases: the published packages are 1.26.11.1. The Go 1.24 and 1.25 sentence moves to "Older versions"
         at the bottom.
     (5) .NET: .NET 11 shipped (SDK 11.0.100 on the hosted runners, ccca03abea); the repository builds with any SDK
         from 10.0.400 up (global.json latestMajor, 013992ccbb); a converted executable rolls forward to a newer runtime
         when no .NET 10 runtime is installed (69feb33d6b; docs/README.md Requirements; DATA-net11-sdk-usability.md).
     (6) Native AOT: lever E and the `error` assertion fix (a9ee17b1eb, KnownIssues.md's entry) as on master.
     (7) Platforms: macOS's standard library is the darwin/amd64 build on both chips (KnownIssues.md, "On Apple silicon,
         the standard library is the amd64 build").
     (8) Real-world modules: the two gate programs shipped; four newer rows are Go 1.26.11 readings (ValidatedModules.md),
         and modules whose tests use testify validate. -->

This page is the plan for what comes next in go2cs: the Go and .NET releases the converted standard
library moves to, the platforms it runs on, the real-world Go code it converts beyond the standard
library, and the runtime and converter work still ahead. The completed phases are in
[Roadmap history](RoadmapHistory.md). Dates are month or quarter targets.

**Where things stand.** The published release is 1.26.11.1, built from Go 1.26.11
([News](NEWS.md)). Every implementable package of the converted standard library validates
against Go's own tests on Windows, on Linux (x64 and Arm64) and on macOS (Intel and Apple silicon).
`go2cs -tests -recurse` validates third-party modules against their own tests the same way,
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

## 1.26.11.1, and the release after it {#the-next-two-releases}
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: rewritten from "The next two releases" (see the provenance
     block at the top). The old section planned 1.26.11.1 and listed the release after it; the 2026-11-14 owner
     rulings it cited are in the 2026-11-15 block above. -->

**In short.** 1.26.11.1, built from Go 1.26.11, is the published release. The main line of development carries
the work for the next release, 1.26.11.2, which has no date yet: goroutine scheduling, more of cgo,
and Native AOT and start-up work.

**1.26.11.1** shipped on November 19, 2026, with both of the programs that gated it converting and running as Go
runs them: a `modernc.org/sqlite` program prints exactly what native Go prints, on Windows and on Linux, and the
converted TypeScript compiler (written in Go) emits what the Go build emits. It also brought `go2cs -cgo` and
linux-arm64 packages. [News](NEWS.md) has the full list.

**On the main line now**, for 1.26.11.2:
- goroutine scheduling, many goroutines over a few threads, as an experimental mode that is off unless you build
  and run for it ([Known issues](KnownIssues.md#goroutines-on-a-shared-pool-of-threads-mn-are-experimental-and-off-unless-you-build-and-run-for-them));
  work on it continues ([Performance](#performance));
- cgo packages converted with their module, against the `go.*` packages, and C memory read through a C struct's
  pointer fields ([cgo](#cgo));
- a pointer to an unnamed array type made from an `unsafe.Pointer` and kept in a variable, which now reaches C
  memory, as SQLite's page cache needs;
- faster start-up under the JIT for programs that ask many method-set questions ([Performance](#performance));
- a smaller Native AOT publish of a large program, and a type assertion to `error` that answers under Native AOT as
  it does under the JIT ([Native AOT and trimming](#native-aot-and-trimming));
- among other changes: `GOMAXPROCS` follows Go's rule, `sync.RWMutex` is Go's own code, and more generics under
  a method-set constraint convert.
  <!-- trains 1 to 7, git log --first-parent nuget-1.26.11.1..97cca467a2: M:N g-mn-* (trains 3-7); cgo 8de0f7ece2,
       f74b4d7385, ef9be9bd32, 1f2a9afdfc, 5f2f2d0aa3; door C 52d3547def; A1 2506d6d48f; lever E 61b23a9888; the AOT
       error shell a9ee17b1eb; GOMAXPROCS ebbb1c850f and 0f96e28773; RWMutex bfdb234ea5; the shape witness 5cd4536a09.
       Breaking changes for hand-written C# (array literals, package main's methods, named slice and array methods)
       are listed in docs/DRAFT-news-next.md for the release notes. -->

**Not on the main line yet:** handing Go memory to C without a copy, and converting the `net` resolver with cgo,
are planned for a release after 1.26.11.2 ([cgo](#cgo)).

## Phase 4: validation against Go's own tests

**In short.** Phase 4 checks the converted standard library against Go's own tests, package by package,
so that "it works" is measured and not claimed. It is done on Windows, Linux and macOS, and has one
planned extension.

- **Phases 4A to 4C are complete.** The test pipeline, subtests and parallelism, and the package-by-package
  validation are done: every implementable package validates on Windows and on Linux, x64 and Arm64
  ([Roadmap history](RoadmapHistory.md#phase-4--convert-and-run-go-package-tests)).
- **macOS parity is reached.** Every package that applies to macOS validates on Apple silicon and on Intel, each
  chip compared with Go built for that chip; where the two chips' counts differ, the
  [roster](ValidatedTestPackages.md) row shows both. See [Platforms](#platforms).
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

The published packages are 1.26.11.1, from Go 1.26.11
([1.26.11.1, and the release after it](#the-next-two-releases)). The main line of development carries the current
Go line, and the next Go minor is prepared separately until its packages ship. Older Go lines are under
[Older versions](#older-versions).
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: 1.26.10.3 -> 1.26.11.1 as the published packages (NEWS.md,
     November 19); the Go 1.24 and 1.25 sentence and its comment moved to "Older versions" at the bottom (owner rule
     2026-11-22). -->

| Hop | When | What it involves |
|:--|:--|:--|
| **Go 1.27** | Q1 2027, by about February | Generic methods, which need a conversion design first. The 1.26 series leaves support when Go 1.28 ships, around February 2027. |

How often later hops run is decided ahead of the Go 1.27 hop.

## .NET

**In short.** Converted code targets .NET 10, a long-term-support release, until .NET 12. The .NET 10 SDK
builds it, a newer SDK builds it too, and neither changes the C# it compiles under.

**The published target stays `net10.0`** until `net12.0`, the next long-term-support release. .NET 10 is
supported until November 2028.

**The C# language version is pinned.** Every project, the converter's template included, fixes
`LangVersion`, so a newer SDK does not compile converted code under a newer C#. The repository's own
`global.json` takes any .NET SDK from 10.0.400 up, newer majors included. On the main line, a converted program
also runs on a newer .NET runtime when no .NET 10 runtime is installed; the `go.*` packages are validated on
.NET 10.
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: "A repository global.json pins the SDK" was stale since
     013992ccbb (rollForward latestMajor, floor 10.0.400; owner ruling 2026-11-20). The roll-forward is go.lib's
     buildTransitive RollForward=Major for an executable (69feb33d6b, train 1; docs/README.md Requirements;
     docs/phase4/DATA-net11-sdk-usability.md); go.lib 1.26.11.1 does not carry it. --> The converter also escapes Go identifiers that newer C# gives new meaning,
such as `closed`, `union` and `safe` when they name a type.

**A .NET 11 measurement stage is planned.** .NET 11 shipped in November 2026; the stage never overlaps a Go
hop.
<!-- 2026-11-23 .5 docs pass (P1): .NET 11 has shipped (SDK 11.0.100 on GitHub's runner images, ccca03abea), and the
     Go 1.26 hop it was to follow is done (October 26, NEWS.md). --> It compares JIT and allocation behavior on .NET 11 over identical code and reviews
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
- On the main line since 1.26.11.1: the runtime library's pointer types share their per-type tables, so a Native
  AOT publish of a large program is smaller and the compiler needs less memory for it (`win-x64`). A type
  assertion to `error` that the method set decides answers as it does under the JIT
  ([Known issues](KnownIssues.md#under-native-aot-a-type-assertion-to-error-that-the-method-set-decides-can-answer-false)).
  <!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: lever E, 61b23a9888 (train 1), its BOARD row: the converted
       TypeScript parser demo, win-x64, about 29% fewer symbol names, a 13% smaller image, about 25% lower ILC peak
       memory. The error shell: a9ee17b1eb (train 3), claude/p2-aot-structural-hit 05728981b3. Replaces "as an option
       for the release after 1.26.11.1" (UX-8). -->

**Later stages** cover method sets and stack frames.

Converted packages are built for the default x64 instruction set: `x86-64-v3` gives no gain across the
benchmark suite on `win-x64`, and an image built that way does not start without AVX2.

The release check publishes a program with Native AOT and runs it on `linux-x64`, `linux-arm64`, `win-x64` and
`osx-arm64`.
<!-- 2026-11-15 (P4, docs audit Q5): linux-arm64 added (os-matrix.yml aot-smoke, all five shipped RIDs since seat
     S5c); the rest of the Native AOT lines wait for P1's PublishAot reading. Kept word-identical with KnownIssues. --> On `osx-x64` the publish is read when it finishes inside the hosted runner's time limit, and
reported as not measured when it does not.

**The cost today is real.** With the default trim, Native AOT compiles every `go.*` assembly a program
references in full, which costs publish time and executable size
([Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)).
A build, `dotnet run` and a publish without Native AOT are not affected.

## Platforms

**In short.** go2cs is validated on x64 Windows, on Linux, x64 and Arm64, and on macOS, Intel and Apple
silicon. Other platforms are not supported yet.

**Windows and Linux.** The converter runs on both, and the standard library is multi-target on disk:
platform-shared files sit flat, and a package whose Go source varies by platform keeps one folder per target.
<!-- design: [design](phase4/DESIGN-multiplatform-corpus.md) --> Each platform-varying package ships `win-x64`,
`linux-x64` and `linux-arm64` flavors, and roster rows record each Linux flavor's verdict counts beside the
Windows record. A few Linux operational items remain open.
<!-- Remaining Linux operational items are tracked in [`PLAN-linux-operation.md`](PLAN-linux-operation.md)
     and the multi-target design (internal records, unlinked 2026-10-26, OQ-1). -->

**macOS (Intel and Apple silicon):** packages ship for both chips, the behavioral suite passes on both,
and the standard library validates on both: each chip is compared with Go built for that chip, and where
the two chips' counts differ the roster row shows both. The standard library for macOS is one build, for
`darwin/amd64`, that serves both chips; on Apple silicon it runs as a native arm64 process
([Known issues](KnownIssues.md#on-apple-silicon-runtimegoarch-reports-amd64)).
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: "packages ship for both chips" read as two builds; KnownIssues.md
     ("On Apple silicon, the standard library is the amd64 build") and NugetgoPublish.md ("Cgo": osx-arm64 waits for
     true darwin/arm64 go.* assets) say there is one. --> The two Windows-only packages are the only rows
without a macOS reading. The compile is checked daily in CI on macOS arm64 and x64 runners
([`CIMatrix.md`](CIMatrix.md)).

**Other platforms.** A build for a platform the packages do not ship is refused, or, on `win-arm64`, warned
about ([Known issues](KnownIssues.md#platforms-the-packages-do-not-ship)).

## Real-world Go modules

**In short.** Beyond the standard library, go2cs converts third-party Go modules and holds each to the
same bar: its own tests must match Go's. The modules on [Validated Modules](ValidatedModules.md) pass,
among them modules whose tests use testify. Two large programs, SQLite and the TypeScript compiler, convert and
run as Go runs them.

Each module is converted, validated against its own test suite exactly as a standard-library package is, and
given a proof page beside the conversion. `go2cs -tests -recurse` runs a module's tests across all of its
packages, and a validated module packs as one NuGet package.

The validated modules include [`google/uuid`](https://github.com/google/uuid), which brings Go's v1, v3, v5
and v6 UUIDs to .NET (where `System.Guid` generates v4 and v7); [`golang-jwt/jwt`](https://github.com/golang-jwt/jwt)
v5, which exercises the converted crypto stack; [`spf13/cobra`](https://github.com/spf13/cobra) with its
dependencies `pflag` and `mousetrap`; and [`itchyny/gojq`](https://github.com/itchyny/gojq) with
[`itchyny/timefmt-go`](https://github.com/itchyny/timefmt-go), an in-process, fully managed jq. gojq's
command-line package and its conformance suite come later, once that package's own dependencies convert.

`modernc.org/sqlite` and the TypeScript compiler, the two programs that gated 1.26.11.1, are published with their
recipes in [sqlite-cs](https://github.com/ritchiecarroll/sqlite-cs) and
[typescript-go-cs](https://github.com/ritchiecarroll/typescript-go-cs).
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: the gate is met (NEWS.md, November 19, which links both
     repositories). The In short's "measured with Go 1.26.10" is dropped: four newer rows (go-difflib, objx v0.5.2 and
     v0.5.3, go.yaml.in/yaml/v3) are Go 1.26.11 readings on the tree's standard library, and objx's tests use testify
     (ValidatedModules.md, bb6267d0e4, train 6). --> Further modules follow, libraries first: a converted module
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
compiler, alone or with the module it belongs to. Next come publishing cgo packages as NuGet packages, Go memory
handed to C without a copy, and a real standard-library cgo package; C code written inline in the Go file and
callbacks from C into Go follow. [Limitations](Limitations.md#cgo) has the detail of what converts and what is
refused.

The standard library is converted with `CGO_ENABLED=0`.
<!-- The plan is [`PLAN-cgo-interop.md`](PLAN-cgo-interop.md) (internal record, unlinked 2026-10-26, OQ-1). -->
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2 (UX-4): rewritten to agree with Limitations.md's cgo section,
     CommandLine.md's -cgo row, DotnetTool.md and NugetgoPublish.md "Cgo" at this master. Sources per item in the
     provenance block at the top of the page. The old "Next, in the release after 1.26.11.1" list is restated as what
     is on the main line and what is not (UX-8). -->

**What converts:**
- One package, when every C use it makes is a call of a C function a shared library defines. Integers,
  floating-point numbers, pointers, and structs of integers and pointers cross, as do a `void *` that is nil or
  points at C memory, the `C.CString` family, `C.malloc`, `C.free` and `defer C.f(…)`. A program that uses it needs
  only the C library. This shipped in 1.26.11.1.
- A converted cgo program printed what the Go build printed on linux/amd64, darwin/amd64, darwin/arm64 and
  windows/amd64, apart from the differences [Limitations](Limitations.md#cgo) lists.
- Without `-cgo`, a file the build selects that imports `"C"` stops the conversion with error `GO2CS2001`, which
  names the file. Test-only and platform-deselected files are skipped as usual.
<!-- 2026-11-14 (H, the release-notes audit): cgo Phase 2's real route and G1 gate (docs/phase4/DESIGN-cgo-phase2-pinvoke.md
     section 11.22: hosted legs on linux/amd64, darwin/amd64, darwin/arm64 and windows/amd64) and Phase 3 increment 3.1
     (43ac9e33fa, batch 305; DESIGN-cgo-phase3-marshaling.md section 16; its hosted cgo-e2e legs 43638330006 linux-x64,
     43638338397 win-x64, 43638346986 osx-arm64 and osx-x64, each 50 of 50 arms as Go does or refused as the arm's table
     says). The owner's scope ruling 4 (2026-11-14): 1.26.11.1 = Phases 1-2 + 3.1 + H's pin-extent fix. -->

**On the main line since 1.26.11.1:**
- With `-recurse -cgo`, a module's cgo packages convert with the module, and with `-recurse=nuget -cgo` they build
  against the `go.*` packages. A conversion is for the machine's own operating system and architecture, or for
  another `-platforms` target when `CC` names that target's C compiler.
- A cgo module packs as one NuGet package for `win-x64`, `linux-x64`, `osx-x64`, `linux-arm64` and `osx-arm64`, each
  with its own C binding, and a build or a run on any other platform is refused by name. A real third-party cgo module
  was packed this way and printed what Go printed on each of the five.
  <!-- 2026-11-28 (P5): cgo-nuget-e2e run 46117314414 at 5b6ef889d0, J3 PASS on all five RIDs, osx-arm64 included
       (docs/phase4/REHEARSAL-cgo-nuget-e2e-go-pointer.md, amended 2026-11-28). Before that reading the sentence ended
       "printed what Go printed on win-x64, linux-x64, osx-x64 and linux-arm64, before osx-arm64 joined." -->
  <!-- 2026-11-28 (P5, the cgo arm on claude/p5-cgo-osx-arm64, stacked on D6 of DESIGN-darwin-arm64-flavour.md): osx-arm64
       joins with its own darwin/arm64 flavor (zig aarch64-macos); its reading is cgo-nuget-e2e's J3 osx-arm64 leg at the
       cgo arm. Before it: "for win-x64, linux-x64, osx-x64 and linux-arm64 ... printed what Go printed on each of those
       four, and was refused by name on osx-arm64", and the bullet ended "osx-arm64 waits for a standard-library build of
       its own for Apple silicon." -->
  <!-- docs/phase4/REHEARSAL-cgo-nuget-e2e-go-pointer.md: github.com/mattn/go-pointer v0.0.1, final reading run
       45016867041 and the re-read 45033824268, J3 PASS on win-x64, linux-x64, osx-x64, linux-arm64; R-b NUGETGOCGO001 on
       osx-arm64; R-a, the load-time target check on win-x64. Publishing it is after the release that follows
       1.26.11.1, "by construction" (its appendix).
     2026-11-23 (P1, P3 nit 3 relayed by COORD 224946Z): the next release's number is ruled, 1.26.11.2; the page names it
     and keeps "no date yet". --> Such a package is published only against a `go.lib` that checks, when
  the package loads, that its C binding is for the platform it runs on: `go.lib` 1.26.11.1 does not, and
  1.26.11.2 does.
- C memory can be read and written through a C struct's pointer field and through a pointer to a C pointer
  (`**C.T`), as code that walks a list C allocated does.

**Next, not on the main line yet:**
- Go memory handed to C without a copy, and a pointer to a pointer that C fills in (`&db`), the rest of Phase 3.
- A real standard-library cgo package, the `net` resolver, converted and validated against its own tests as the
  proof.

Both are planned for a release after 1.26.11.2.

**Later:** C code written inline in the Go file (Phase 4), then `//export` and callbacks from C into Go
(Phase 5). Until each lands, it is refused by name.

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
  start and run one handshake (`linux-x64`), most of it loading packages. On the main line, a method-set question
  at start-up reads only the classes that can answer it, under the JIT; the same gain under Native AOT, and further
  steps in how converted types find their methods, are planned. This cost also keeps the `crypto/tls`
  [host-limit entry](#declared-host-limits-and-their-retirement-path) open.
  <!-- DATA-bogo-startup.md; the B3 items A1/A3 (start-up method sets) are .5 by the owner's ruling 6 of 2026-11-14. -->
  <!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2 (UX-8): arm A1 landed, 2506d6d48f (train 1); its BOARD row
       of 2026-11-18 says AOT answers do not change and AOT gets none of A1's gain; DATA-startup-method-sets-a1.md K1
       BigAst about -0.3 s, K2 wide about -35 ms (paired medians). A3 and the AOT keys are not on master. -->
- **Goroutine scheduling** (in progress): by default each live goroutine runs on its own OS thread, so a
  program can hold only as many goroutines alive at once as the system allows threads, a limit macOS reaches first
  ([Limitations](Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)). The main line has an
  experimental scheduler that runs many goroutines over a few threads, as Go does. It is off unless you build and
  run for it
  ([Async twins](AsyncTwins.md), [Known issues](KnownIssues.md#goroutines-on-a-shared-pool-of-threads-mn-are-experimental-and-off-unless-you-build-and-run-for-them)),
  and the work continues on the kinds of blocking it can see.
  <!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2 (UX-8): restated from "planned for the release after
       1.26.11.1". The scheduler: the g-mn-* merges of trains 3 to 7; KnownIssues.md's M:N entry; "the scheduler stays
       OFF by default", docs/phase4/DESIGN-mn-n5-invisible-blocking.md (row 12 of the M:N plan). -->
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
| Now | On the main line, for 1.26.11.2: goroutine scheduling, cgo packages built with their module and against the `go.*` packages, the trimming work for Native AOT, and the start-up cost of a converted process ([1.26.11.1, and the release after it](#the-next-two-releases)). |
| Next | Publishing cgo packages, once a `go.lib` with the load-time platform check is published. Go memory handed to C without a copy, and the `net` resolver converted with cgo. The .NET 11 measurement stage. The generic-methods design for Go 1.27. The decision on how often later hops run. |
| Q1 2027 | Go 1.27 hop, by about February. The Phase 4D design kickoff. The allocation work, whose design is written. |
| Unscheduled | Inline C and callbacks from C (cgo Phases 4 and 5). The Phase 5 stubs that no exported call reaches, each closed when a real use needs it. Interface-conversion performance work. The remaining stack-string increments. Open converter items. |

## How progress is tracked

**In short.** Where to check each claim on this page for yourself.

| What | Where |
|:--|:--|
| Validation | [Validated Test Packages](ValidatedTestPackages.md), the authoritative roster: each package's verdict counts on Windows, Linux and macOS, its disclosures with their stated reasons, and its proof page. Its header carries the current totals. |
| Third-party modules | [Validated Modules](ValidatedModules.md): each validated module, its version, the platforms it was measured on, and its proof pages. |
| Compilation | The whole converted standard library builds with zero errors (`src/go2cs-stdlib.slnx`) for Windows and Linux; the macOS compile runs daily in CI ([`CIMatrix.md`](CIMatrix.md)). |
| The converter | Hundreds of Go-vs-C# behavioral projects under `src/tests/Behavioral`, each transpiled, compiled, compared against a committed golden and, where it is a runnable program, output-compared against Go. |
| Performance | The [performance comparison](Performance.md). |
| Releases | [News](NEWS.md). |

Every converter change runs the same loop: edit `src/go2cs/*.go` → `go build` (with the Go release
`src/go2cs/go.mod` names) →
re-transpile → `dotnet build`, and a whole-corpus reconvert shows no unintended drift before the change
lands.

## Older versions

**Go 1.24 and Go 1.25.** Go 1.24 and 1.25 have no further releases: packages built from either carry none of the
Go security fixes issued since.
<!-- Go 1.24 left upstream support in February 2026 and 1.25 in August 2026; 1.24.13.7 is the last 1.24 release, and
     there is no Go 1.25 release (docs/PLAN-version-lines.md). -->
<!-- 2026-11-23 .5 docs pass (P1), at master 97cca467a2: moved here from "Go releases", text unchanged (owner rule
     2026-11-22: text about older versions sits at the bottom of the page). -->
