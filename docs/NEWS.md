# 📰 go2cs News Archive

![go2cs](images/go2cs-small.png)

All project announcements, newest first. The [README](README.md) summarizes where the project stands
today; every announcement is recorded here, and the detail-heavy ones link to a companion page carrying
their full text.

---

## September 24, 2026 — The converted standard library moves to Go 1.24.13, and 218 packages validate against it

**go2cs now converts Go 1.24.13's standard library**, and the validated roster crossed the hop
re-derived, not carried: **218 of the 230 testable standard-library packages validate their own
Go 1.24.13 test suites in C#** — **56,974 matching verdicts** against `go test -json`, with **283**
divergences disclosed by exact failure signature — and, measured against the 224 packages a faithful
managed conversion can honestly validate at all, **97.3%**. On Linux, 187 of the 216 applicable rows
validate at their own Linux counts, at 53,048 matching verdicts. Every validated row is proved by a
run at Go 1.24.13.

The count rises by fourteen from the Go 1.23.12 record's 204 while the testable set grows by fifteen,
so both percentages dip — 97.6% to 97.3% of the implementable set, 94.9% to 94.8% of the testable
one. **One package validated at Go 1.23.12 is not validated at Go 1.24.13: `net/http`.** It matches
1,370 of its 1,387 verdicts, and all 17 divergences trace to tests run under Go 1.24's new
`internal/synctest`, the runtime support behind the experimental `testing/synctest`, which go2cs does
not yet support; it still ships as `go.net.http`. Go 1.24 moved ten validated packages to new import
paths, and their tests validate under the packages that now hold them. Counting those, twenty-five
rows join, among them `unique` and Go 1.24's new `crypto/hkdf`, `crypto/mlkem`, `crypto/pbkdf2`,
`crypto/sha3` and `weak`. Six implementable packages are not yet validated: `reflect`, `runtime`,
`runtime/pprof`, `net/http/pprof`, `net/http` and Go 1.24's new `internal/synctest`.

On Linux the count falls from 198 to 187: the relocated packages' tests are not yet run on Linux at
their new paths, and `go/internal/srcimporter` has no Linux result, because Go's own test of it fails
on the Linux reference machine; it stays validated on Windows. The verdict total doubles because Go's
suites grew — `crypto/cipher` alone matches 27,272 verdicts, against 13 at Go 1.23.12 — so verdict
totals do not compare across Go releases.

Converted programs get Go 1.24's APIs — `os.Root`, `weak.Pointer`, `crypto/mlkem` and
`strings.Lines`, among others — and `//go:embed` support. go2cs itself now builds with Go 1.24.13, and
a `-recurse=nuget` conversion needs its module to resolve to Go 1.24; to stay on Go 1.23, build the
converter at the `nuget-1.23.12.3` tag with Go 1.23.12 and use the 1.23.12.3 packages. Go 1.24's
FIPS 140-3 module converts and its tests validate, but go2cs makes no FIPS 140-3 claim: the module's
integrity self-check hashes a binary layout that a .NET assembly does not have, so under
`GODEBUG=fips140=on` the converted check reports success without verifying anything.

Fifty-one package IDs are new, six of them for packages a Go program can import: `go.crypto.fips140`,
`go.crypto.hkdf`, `go.crypto.mlkem`, `go.crypto.pbkdf2`, `go.crypto.sha3` and `go.weak`. Fourteen end
at 1.23.12.3, their last release, because Go 1.24 moved or deleted their packages; none is importable
outside the standard library, and each stays restorable at 1.23.12.3 and is never unlisted.

The converted standard library publishes as **NuGet 1.24.13.1** — 344 packages, author-signed, still
targeting .NET 10 — with every proof page frozen at `validation/1.24.13.1` for the badges the packed
READMEs link, and the exact shipped tree browsable at the `nuget-1.24.13.1` tag. The Go 1.23.12
record stays frozen as it shipped, at `validation/1.23.12.3`.

*Full story: [The converted standard library moves to Go 1.24.13](news/2026-09-24-stdlib-moves-to-go-1-24-13.md)
· tag `nuget-1.24.13.1`*

## September 7, 2026 — The Go 1.23.12 record closes at its anchor; the corpus hops to Go 1.24

**The Go 1.23.12 validation record is closed.** It stands at **204 of the 215 testable
standard-library packages** — **28,459 matching verdicts** against `go test -json`, with **167**
divergences disclosed by exact failure signature — and, measured against the 209 packages a faithful
managed conversion can honestly validate at all, **97.6%**. On Linux, 198 of the 202 applicable rows
validate at their own Linux counts, at 23,199 matching verdicts. Those figures are an **anchor**
rather than a running total: the corpus now moves to **Go 1.24.13** instead of driving this release
to 100% first.

Five packages remain unbanked here — `reflect`, `runtime`, `unique`, `runtime/pprof` and
`net/http/pprof` — and none of them is written off. A version hop re-derives **every** roster row from
the new release's own test sources — numerator, denominator and disclosure set alike — so all five
re-validate against Go 1.24 on exactly the footing of the 204 that banked. What holds each back
differs: `reflect` is measured and under active converter and runtime work; `runtime` is measured
only as far as a host-killing crash — `TestCrashWhileTracing`, at index 104 of its 883 verdicts —
which leaves everything after it unread; `unique` is measured at 19 of its 20 verdicts under the
configuration of record and waits on a ruled runtime-model arc; and `runtime/pprof` and
`net/http/pprof` are each measured on two hosts and held by a capability or classification ruling.

The reordering is deliberate, and it is worth saying why, because the percentage means something
narrower than it looks. **The metric is package-based, not content-based**: a row is all-or-nothing,
so a package matching most of its verdicts still scores zero, exactly as one matching none of them
does — which is what keeps the roster honest, and what makes the headline a count of finished
packages rather than of passing tests. What is left is therefore a long tail of
runtime-model work — Go's own heap layout, its type descriptors, its profiling internals — that does
not stand between a user and a converted program. The project is measured against a trustable platform
that migrates packages and applications from Go to C#, and a more recent Go standard library unlocks
more of that than the tail does. This is not a compromise but a reordering: the hop that unlocks user
capabilities happens first, and the remaining guardrails are handled after it.

The 1.23.12 corpus ships one final NuGet release as that anchor before the pin moves — the release
that freezes its roster, its proof pages and every package README at the record above. It has to come
first: the pin bump resets the build counter, so once the corpus names Go 1.24 there is no longer any
version left in which the 1.23.12 record could ship.

## August 29, 2026 — Over 90% of the standard library's test suites pass in C#

**189 of the 215 testable standard-library packages validate their own Go 1.23.12 test suites in
C#** — **26,043 matching verdicts** against a clean `go test -json` baseline, with **148** divergences
disclosed by exact failure signature and nothing else waived. Seven of the 215 cannot be validated at
all — no eligible tests on this platform, a broken upstream oracle, or a suite whose whole subject is
the raw memory layout a managed runtime deliberately does not have — so against the implementable set
of 208 the roster reads **90.9%**. On Linux, 178 of the 188 applicable rows validate at their own Linux
counts, at 21,807 matching verdicts.

The push past 90% landed **`net` itself — 472 verdicts, the roster's largest networking row** — banked
the day two phantom DNS servers and a router forwarder stopped impersonating an oracle. A host whose
*own* `go test net` fails is disqualified as a reference, because the two sides of a differential would
then be running different oracles; qualifying one was the work. `reflect` compiled and executed for the
first time alongside it, and `net/netip`, `net/http/cookiejar` and `debug/pe` joined the roster in a
single day.

The converted standard library publishes as **NuGet 1.23.12.2** — 307 packages, author-signed — with
this release's 189 proof pages frozen at `validation/1.23.12.2` for the badges the packed READMEs link,
and the exact shipped tree browsable at the `nuget-1.23.12.2` tag.

*Added to the archive on September 7, 2026; the announcement itself ran on the project's front page
from August 29. Release freeze and announcement: `773afa2c2` · published: `d2da277f5`*

## August 25, 2026 — Both runtime pins move: .NET 10, Go 1.23.12 — and the whole roster re-proves itself

**go2cs now targets .NET 10 and converts Go 1.23.12**, and the validated roster crossed the hop the
only honest way: re-derived, not carried. Every one of the **162 banked packages re-validated from
the new release's own `_test.go` sources** — **18,598 matching verdicts** against a clean
`go test -json` baseline at the new pins, with the 85 disclosures re-signed against the new
sources — count and per-row set unchanged. The
29 new verdicts over the 1.23.1 record come exactly from the four suites whose verdict counts
moved with the release (`time`, `os/exec`, `syscall`, `database/sql`), reconciled to the digit
across the machines that ran the re-validation shards. A version hop invalidates every row's numerator,
denominator and disclosure set by design; there is no carry-forward path, so agreement at the new
pins is measured, never assumed.

The .NET half moved 955 project files to `net10.0` with zero corpus-emission drift across all three
OS flavors — and immediately earned its keep: C# 14's new implicit array-to-span conversion
changed which overload form a variadic call binds, exposing that the corpus's variadic-slice
binding had been correct *by accident* under C# 13. The converter now emits the binding that is
correct on purpose.

The converted standard library publishes as **NuGet 1.23.12.1** — each package README freezing its
validation badges against this release's own proof snapshot, with the exact shipped tree browsable
at the `nuget-1.23.12.1` tag.

## August 22, 2026 — Over 75% of the standard library's test suites pass in C#

**162 of the 215 testable standard-library packages (75.3%) validate their own Go 1.23.1 test
suites in C#** — 18,569 matching verdicts against a clean `go test -json` baseline, with
85 signature-pinned disclosed divergences. Each suite is converted from Go's own
`_test.go` sources, built against the converted standard library, run under the Go-semantics test
host, and compared verdict for verdict; a package joins the roster only when every eligible test
agrees.

The milestone is tagged because something genuinely hard landed with it. **A converted stack frame
now reports a Go file and line.** The position is derived from conversion-time facts rather than
composed — file and line ship together, because either alone names a position that exists in
neither tree — and the identity is build-shape-faithful: the published standard library reports
the same trimpath form Go bakes into a published binary, while a converted user module reports the
source path Go would have baked for that build. `runtime/debug`, `log`, `log/slog` and `flag` all
rest on it.

Three more capabilities landed alongside. **Channel direction and map key/element dimensions ride
the type descriptor**, so `reflect` distinguishes `<-chan int` from `chan int` and a decode target
can measure an array it has never populated — which retired an entire disclosure class on its own
recorded remedy and banked both template packages. **A native pointer slot holds the pointer's
value**, not a managed reference, closing a round-trip that had also hidden a GC-invisible
dangling-reference hazard. And **an always-on GC recorder** supplies real pause and release facts
to `runtime/debug`'s statistics surface at zero cost per read.

What the campaign declined is part of the record. A disclosure names something the managed runtime
provably cannot satisfy — never something merely unimplemented — so packages that reach 105 of 106
or 210 of 267 stay off the roster with their remaining root named and priced. Four disclosure
classes carry the campaign's divergences today; a fifth retired itself the day its remedy landed,
exactly as it was written to.

This is Go 1.23.1's terminal validation marker. Every roster row re-derives from a release's own
test sources at a version hop, so the campaign continues on Go 1.23.12 — the release users would
actually choose — with the 1.23 story living on its own branch.

## August 8, 2026 — Go programs run on Linux

Converted Go programs now **run on Linux, byte-identical to `go run`**: `fmt.Println("hello, 世界")`,
a program crossing `os.Args`, `os.Getenv` and `time.Now()`, and the README's own
[real-world walkthrough](README.md#converting-a-real-world-module) — `fatih/color` printing true ANSI
colour under a real PTY, with the `isatty` branch agreeing with Go in both directions (plain when piped,
coloured on a terminal). The whole campaign landed in one continuous arc: the repository checks out
deterministically on any filesystem, the converter and every harness instrument run natively on Linux,
the standard library compiles for **windows, linux and darwin from one tree** (per-GOOS source folders
selected by `$(GoTargetOS)`, windows the default, with 141 of 141 shared-source packages measured
IL-identical across flavors), and each Go package ships as **one NuGet package** carrying RID-specific
assemblies only where source genuinely varies.

At the bottom of it all sits **one measured keystone**: Linux's entire syscall surface crosses the
kernel through a single `libc syscall(2)` binding whose three claims were probed rather than argued —
the variadic ABI with a real six-argument `mmap`, the second return register shown to be *exact* (the
kernel preserves `RDX`), and errno round-tripped through a deliberate fault. The road there surfaced
exactly two converter defect families and a handful of linkname wiring gaps — including the lesson that
a forward alone can *pass a run and be wrong* (`os.Args` silently empty), which is why every wiring row
now pairs with its populated truth. The FFI surface simultaneously converged on source-generated
`[LibraryImport]` bindings, where non-blittable signatures fail at **compile time** — three latent
marshalling hazards surfaced during the migration, each converted explicitly.

Stated plainly: the published `1.23.1.4` packages still carry Windows-only assemblies — the Linux
experience ships with the next release; a Linux consumer of the few platform-divergent packages also
needs the compile-surface answer scheduled next; and darwin binaries are compile-proven and
IL-identity-backed but have never been executed here. The Windows lane did not move a byte through any
of it: every merge held CNR byte-identical, the behavioral suite green, and the 110-package validated
sweep at 13,628 verdicts with zero failures.

## August 8, 2026 — Over half the standard library validates; defers reach zero allocation

Three days took the validated roster from 73 packages to **110 of 215 (51.2%)** — 13,628 matching
verdicts against `go test`, 50 disclosed divergences, not one added in the climb. The largest single
advance was also the quietest: a re-scout of never-measured packages found **34 that validated with no
changes at all** — the corpus had grown past them as shared machinery landed. The loudest was
architectural: **`defer` now compiles to an inline `try`/`catch`/`finally` over a `ref struct` frame** —
no closure object, no delegate per defer, zero allocation for non-capturing defers — replacing the
execution-context lambda while *improving* behavioral fidelity (a capture-semantics divergence class
died by construction). Three runtime capabilities were hand-implemented on managed primitives following
the established pattern: Go's concurrent hash-trie map, weak pointers, and caller-frame walks. And three
user-reported `-recurse` failures — a quoted `go.mod` directive, a C# keyword inside an import path, and
a type renderer chasing its own tail through `<-chan` — were each root-caused, fixed, and answered on
the issue the same day, ending with `gopkg.in/yaml.v3` converting, compiling, and running byte-identical
to `go run`.

## July 26, 2026 — More than a quarter of the standard library's test suites pass in C#

**57 of the 215 testable standard-library packages validated their own Go test suites in C# — 26.5%
of the Phase-4 target**, at **1,459 matching test results** with **47** honestly disclosed as
divergent rather than quietly dropped. A package counted only when *every* `Test` function's verdict
matched a clean `go test -json -count=1` baseline. The set moved well past leaf packages: `sync`'s own
concurrency suite, the RE2 engine in `regexp`, `strconv`'s float formatting, the `crypto/sha*` family,
and the reflection-driven `errors`, `encoding/binary` and `go/token` — the last round-tripping a
`FileSet` through the real converted `encoding/gob`. The same push made goroutine exit, string-literal
allocation and structural interface assertion behave the way Go specifies, and corrected two published
performance figures that had been measuring the wrong artifact. Per-package counts are in
[Validated Test Packages](ValidatedTestPackages.md); the measured numbers in [Performance](Performance.md).

*Full story:
[More than a quarter of the standard library's test suites pass in C#](news/2026-07-26-quarter-of-stdlib-tests-pass.md) · commit `44fcc4f04` · reproduce any row from a
clone via [Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 18, 2026 — `unicode/utf16` validates; disclosed-divergence generalizes

**Phase-4 package #5.** [`unicode/utf16`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf16)
validated its own Go test suite in C# — **8 tests agreeing outright** against `go test -json`, plus one
honestly disclosed. The structural twin of the very first validated package (`unicode/utf8`), it
round-trips UTF-16 encode/decode with results checked by `reflect.DeepEqual`, exercised through the
converted reflection bridge. Its significance was the ninth test: `TestAllocationsDecode` asserts a
zero-allocation `Decode`, which Go reaches only through compiler escape analysis and a managed runtime
provably cannot match. `unicode/utf16` was the first package to reuse the disclosed-divergence manifest
as a **general tool** rather than a two-package special case, pinning that one `alloc-profile` row by
exact failure signature while a separate test proved the decoded output correct — a mechanism that
generalizes cleanly is a mechanism that was designed right.

*Full story:
[`unicode/utf16` validates; disclosed-divergence generalizes](news/2026-07-18-utf16-validates.md) · Phase-4 package #5 · 8 + 1 disclosed (alloc-profile)
· reproduce from a clone via [Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 18, 2026 — `bytes` and `strings` tests pass, with disclosed-divergence

**Two more standard-library packages validated their own Go test suites in C#** — and they arrived with
a new piece of Phase-4 machinery. [`bytes`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/bytes)
validated **81 tests** and [`strings`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/strings)
**68** against `go test -json`, bringing the Phase-4-validated count to four after `unicode/utf8` and
`sort`. Both contain tests that assert an **exact allocation count** via `testing.AllocsPerRun` —
unsatisfiable by design in a managed runtime, where the divergence is the allocation *model*, not the
measurement. Rather than silently skip them, go2cs began disclosing them at test level: a hand-owned,
repo-committed `go2cs_test_disclosures.json` pins `{test, divergence class, expected failure
signature}`, and the differential oracle reclassifies a result only when both the test name and the
pinned signature match — an integrity guard, not a blanket exemption. Packages without a manifest
compare strictly.

*Full story:
[`bytes` and `strings` tests pass, with disclosed-divergence](news/2026-07-18-bytes-strings-disclosed-divergence.md) · Phase-4 packages #3 and
#4 · `sort` 63/63, `bytes` 81, `strings` 68 · reproduce from a clone via
[Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 17, 2026 — Go's own tests now pass in C#

**A standard-library package's own Go test suite — converted to C# — ran and agreed with `go test`,
verdict for verdict.** [`unicode/utf8`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core/unicode/utf8)'s
real test suite (Go 1.23.1) validated **14/14** through the new converted-test pipeline: the `_test.go`
files are transpiled to C#, built against the converted standard library, executed under a Go-semantics
test host, and differentially compared against a clean `go test -json` baseline by full test name — with
every benchmark and example declaration honestly disclosed rather than silently skipped. One week after
"the whole standard library *compiles*," the answer to *"but does it **run**?"* had its first
machine-checked proof, [reproducible from a clone](README.md#try-it-yourself--validate-a-converted-test-suite).
This opened the Phase 4 operational era — **real Go tests, not compilation, are the currency of
correctness** — with `sort`, `strings` and `bytes` next in line.

*Tag: [`utf8-tests-green-2026-07-17`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf8-tests-green-2026-07-17)
· commit `337a928df`*

---

## July 14, 2026 — The converted Go standard library is on NuGet

**The converted Go standard library, the `golib` runtime, and the `go2cs-gen` analyzer are published to
[nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)** as `go.<pkg>` /
[`go.lib`](https://www.nuget.org/packages/go.lib) / [`go.gen`](https://www.nuget.org/packages/go.gen),
versioned `1.23.1.<build>` from `src/version.props`. The converter's new `-recurse=nuget` mode emits
matching `<PackageReference>` entries — defaulting `$(GoStdLibVersion)` to a floating release — so a
converted end-user app or library restores the whole go2cs stack from NuGet with **no local go2cs source
checkout**; the app's own and third-party converted packages stay project references. See
[Converting a real-world module](README.md#converting-a-real-world-module) for the end-to-end walkthrough.

*Tag: [`nuget-stdlib-2026-07-14`](https://github.com/ritchiecarroll/go2cs/releases/tag/nuget-stdlib-2026-07-14)
· commits `2363af0e6`, `2e15eec9d`, `dd821a556`*

---

## July 10, 2026 — The entire Go standard library compiles in .NET

**All 302 packages of the auto-converted Go standard library (Go 1.23.1) compile
cleanly as .NET assemblies — zero errors, zero exclusions.** Every package you'd expect to be hard is
in that number: `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`,
`encoding/json`. The transpiled output is not a demo subset — it is the standard library, end to end,
emitted by the converter, transpiled Go to C#, then compiled by Roslyn. NOTE: don't get _too_ excited,
this is _fully compilable_ not _fully runnable_ — that's the next phase (underway; see the July 17 item
above)! However, simple apps will run, try
[converting a real-world module](README.md#converting-a-real-world-module). Read more about this
[milestone's details](StdLibCompileMilestone.md) and
[current status](README.md#status) in the README.

*Tag: [`stdlib-green-2026-07-10`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-green-2026-07-10)
· commit `51ba5d9cf`*

---

## June 27, 2026 — The `math` package compiles clean

The full-conversion **`math` package compiles clean** — a core, widely-imported standard-library
package, and with it a major step in the Phase 3 drive to compile the whole auto-converted standard
library. The session that landed it greened nine full-conversion packages (`unicode`,
`internal/trace/event`, `unicode/utf16`, `internal/platform`, `image/color`, `runtime/internal/sys`,
`runtime/internal/math`, `math/bits`, and `math`) via 19 behaviorally-tested converter and generator
fixes. The dominant theme was comprehensive untyped-constant typing, plus shadowing fixes,
namespace-collision qualification, composite self-qualification, and relational-pattern guards.

*Tag: [`math-green-2026-06-27`](https://github.com/ritchiecarroll/go2cs/releases/tag/math-green-2026-06-27)
· commit `914d4bd72`*

---

## May 5, 2025 — First full standard-library auto-conversion

The rewritten Go-based converter completed its **first full standard-library auto-conversion**: the
whole Go standard library (~301 projects) converted end to end. "Converted" here meant the transpiler
did not crash and every Go source file received a corresponding C# file — not yet that the emitted C#
compiles. Driving this full conversion to a clean compile became the Phase 3 campaign, finished on
July 10, 2026 (above).

*Tag: [`full-conversion-2025-05`](https://github.com/ritchiecarroll/go2cs/releases/tag/full-conversion-2025-05)
(`cc14584c7`, May 11) · commit `6ca1c45b7`*

---

## January 12, 2025 — The converter is rewritten in Go ("go2cs" version 2)

**Major project restructuring** — the "go2cs iteration 2" generation begins: the converter is
re-implemented **in Go** on the official `go/ast` + `go/types` toolchain, replacing the original C#
converter built on an ANTLR4 Go grammar; T4 templates are replaced by raw string literals; and Roslyn
source generators take over the auto-generated ancillary code that supplies Go semantics at compile
time. The ANTLR4/C# converter is retired.

*Commit: `87465f5f5`*

---

## November 19, 2022 — .NET 7.0, C# 11, and UTF-8 string literals

From the ANTLR4-era converter's News:

* Project has been updated to use .NET 7.0 / C# 11.
* String literals are encoded using UTF-8 (C# `u8` string suffix) which uses the `ReadOnlySpan<byte>`
  ref struct. This should make Go strings faster since strings do not have to be converted to UTF-8
  from UTF-16. Also added an experimental
  [`sstring`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/sstring.cs),
  a ref struct implementation of a Go string.
* Code conversions now better match original Go code styling.

*Commit: `d90f267d4`*

---

## March 13, 2022 — `v0.1.2` release

**go2cs `v0.1.2` is released** — a tagged release of the mature ANTLR4-era converter. Converted code
now targets **.NET 6.0 / C# 10**, using file-scoped namespaces and reduced indentation to better match
the original Go code's styling, with new command-line options for pre-C#-10-compatible output and ANSI
brace style, options to skip GOOS/GOARCH- and cgo-targeted files, and the ANTLR4 grammar synchronized
to the official source.

*Tag: [`v0.1.2`](https://github.com/ritchiecarroll/go2cs/releases/tag/v0.1.2) (`289b939db`)*

---

## January 5, 2021 — Go as a scripting language for Unity and Godot

Example usages of go2cs allow [Go](https://golang.org/ref/spec) to serve as the **scripting language
for the [Unity](https://unity.com/) and [Godot](https://godotengine.org/) game-engine platforms** —
see the [GoUnity](https://github.com/ritchiecarroll/GoUnity) and
[GodotGo](https://github.com/ritchiecarroll/GodotGo) projects. The project has also been updated to
**.NET 5.0** and supports
[publishing as a self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained).

*Commits: `efb497b3a`, `e5c2d7cbc`*

---

## August 29, 2020 — First full conversion of the Go standard library (ANTLR4 era)

The initial conversion of the **full Go source library** completed without failing — the converter's first end-to-end pass over the entire standard library, committed to `src/go-src-converted`.
The warnings in that conversion's build log laid out the road map of the parsing and conversion work
remaining. Converted code at the time targeted .NET Core 3.1 / C# 8.0, and simple conversions depended
on `src/gocore` — the small, manually-converted subset of the Go library that survives today as the
curated baseline in `src/core`.

*Commit: `8e2d6e8e6`*
