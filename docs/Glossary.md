# Glossary
<!-- {% raw %} — Jekyll/Liquid guard: this doc contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

> Terms the go2cs documentation uses for how the converter is tested and validated, the names that
> appear in converted code, and the .NET and tooling acronyms the conversion pages assume. For
> *conversion* terminology (how Go constructs map to C#: `ж<T>`, heap boxes, adapters, direct-ж,
> shadow renames) see [`ConversionStrategies.md`](ConversionStrategies.md) (summary) and
> [`ConversionStrategies-Reference/`](ConversionStrategies-Reference/README.md) (full detail).
> Companion docs: [`Architecture.md`](Architecture.md) and
> [`Baseline-vs-FullConversion.md`](../src/archived/Baseline-vs-FullConversion.md).
<!-- Split on 2026-10-26 by owner ruling OQ-1 (pages reachable from go2cs.net carry no fleet language). This
page was "Glossary — process terms used in commit messages, memory logs, and reviews"; the development-process
vocabulary (CNR, A/B, overlay, chip, coordinator, batch, seat, train, battery, re-bank, harvest, OWED, the
document types, board, ledger, mailbox and lanes) moved verbatim to docs/doctrine/ProcessGlossary.md, which the
site excludes and CLAUDE.md links. The entries kept here are reworded for visitors; their full process text is
in that page. -->

## Testing and validation

<a id="behavioral-test"></a>**Behavioral test.**
A small Go program under `src/tests/Behavioral/` that exercises one construct, together with its converted C#.
The behavioral suite converts each one, compares the converted C# with its **golden** (below), compiles both the
Go and the C#, and, for the tests that opt in, runs both and compares their output.

<a id="guard"></a>**Guard.**
A behavioral test that locks in a fix: a minimal Go program exercising the exact construct, its converted
`.cs`, and a **golden**. Where it can, a guard is shown to fail on the converter from before the fix, and a
guard for a pointer or box fix observes a write through the fixed path in its output, compared against Go, so
a conversion that compiles but silently copies cannot pass it.

<a id="golden"></a>**Golden / re-baseline.**
The committed expected converter output (`*.cs.target`), compared byte for byte (ignoring line endings) with
what the converter emits now. When a change is meant to alter emission, the affected goldens are
**re-baselined**: regenerated from the converter's output, never edited by hand, while the run-and-compare
phase stays green to show the behavior is unchanged.

<a id="census"></a>**Census.**
Building the whole converted standard library (`dotnet build src/go2cs-stdlib.slnx`) and counting the projects
that **emit their own assembly** (`bin/Debug/net10.0/<AssemblyName>.dll` exists after the build). It counts
packages that compile, not errors: fixing one family of errors can raise the error count by uncovering the
errors behind it.
<!-- Phase-3 history (302 at the Phase-3 milestone, Go 1.23.1) and the GREENED/REGRESSED reporting, the
own-errors/blocked-dependents reading and the overlay ritual: docs/doctrine/ProcessGlossary.md#census. -->

<a id="banked"></a>**Banked.**
A standard-library package whose converted Go test suite **validates** — every test reaches the same verdict as
`go test -json` on the same sources, or carries a disclosed reason — has its converted test artifacts committed
into `src/core/<pkg>` (test sources, host, `package_test_info.cs`, `<pkg>.tests.csproj` and any disclosures
manifest) together with its row in [`ValidatedTestPackages.md`](ValidatedTestPackages.md). From then on those
artifacts are the package's own regression check.
<!-- The other sense of "banked" (a verified finding recorded for later rather than fixed now) is a process
term: docs/doctrine/ProcessGlossary.md#banked. -->

<a id="banked-roster"></a>**Banked roster.**
The set of banked packages: whatever [`ValidatedTestPackages.md`](ValidatedTestPackages.md)'s table lists, each
row carrying the package's matching-verdict and disclosed counts. The table is the single source: the scripts
that re-validate the roster read it rather than keeping a list of their own.

<a id="validated-sweep"></a>**Validated sweep.**
The re-validation run (`src/run-validated-sweep.ps1`) that takes **every** package on the banked roster through
the whole test pipeline again with the current converter (convert the tests, build the test host, run it,
compare the verdicts with `go test -json`) and requires each package to land on **exactly** its recorded counts.
A package that passes with different counts fails the sweep, because the table and the converter no longer
agree. It is the check that re-proves converted test code, not just converted library code.
<!-- Process detail (the content-drift report, the per-package deadline floors and why the sweep is serial):
docs/doctrine/ProcessGlossary.md#validated-sweep. -->

<a id="mvp"></a>**MVP — minimum viable [increment].**
The smallest *correct and fully tested* first version of a converter or runtime feature: the narrowest scope
that proves the whole mechanism end to end, with the broader coverage added later in separately tested steps.
For example, the stack-string `sstring` emission (`s := string([]byte)` → a zero-copy view instead of a heap
`@string` copy) first fired on exactly **one** standard-library site, under a deliberately conservative rule. An
MVP is **not** a prototype or a shortcut: it ships behind the full set of checks, including its own **guard**,
and is correct within its scope; only its reach is small.

<a id="hop"></a>**Hop.**
Moving the converted standard library to a new release of one of its two inputs: a **Go hop** moves the Go
release the standard library is converted from, and a **.NET hop** moves the .NET runtime and target
framework. A hop changes one of the two at a time, never both.
<!-- Process detail (targets freeze at hop start, ⟨OQ-1⟩; per-campaign lettering, Hop N = .NET 10 and
Hop A = Go 1.23.12): docs/doctrine/ProcessGlossary.md#hop. -->

## Names in converted code

<a id="marker-glyphs"></a>**Marker glyphs / `Symbols.cs` / `symbols.json`.**
The emitted C# uses reserved glyphs (`ж` pointer box, `Ꮡ` address-of, `Δ` shadow/collision rename,
`ꓸ` type-alias dot, `ᴛ` temp, `ᒐ` the per-function `GoFrame` defer local, …).
Converter and generator **source** reference named constants, never literal glyphs: the C# side (golib,
go2cs-gen) uses `Symbols.cs` (`PointerPrefix`, `AddressPrefix`, `ShadowVarMarker`, …, via
`using static go2cs.Symbols`); the Go converter uses the same-named constants in `src/go2cs/symbols.go`. Both
files are **generated projections of the canonical symbol table `src/core/go2cs/symbols.json`** (kept pure ASCII,
every glyph a `\uXXXX` escape, so no tool can mangle it): edit the JSON and regenerate with `go generate .` from
`src/go2cs`, or run `src/check-symbol-sync.ps1`, which regenerates and exits 1 on drift. Never hand-edit the two
generated files. Glyphs inside goldens, docs examples, and comments are fine: they *are* the output.
<!-- What is deliberately outside the table (the converter's Go-only reserved list, the Go* attribute names and the
go2cs-gen template text; deferred 2026-07-16): docs/doctrine/ProcessGlossary.md, under "Marker glyphs". -->

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
`src/go2cs/interfaceConversion.go`) and generator (`ImplementGenerator`) compose the name independently and must
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
closure, ~25 min per benchmark on an older six-core desktop, which is why a full perf run takes hours. **ILC
arrives as a runtime pack selected by the [TFM](#tfm), not by the SDK performing the publish** — so a new SDK
publishing an old TFM silently resolves the OLD ILC and produces the previous generation's codegen, which
looks like "the new runtime changed nothing" rather than like an error. So no AOT measurement of a new runtime
exists before the TFM moves.
<!-- Source: trap 1 of the .NET migration runbook, [`DotNetMigration.md`](DotNetMigration.md) §3. -->

<a id="tfm"></a>**TFM — Target Framework Moniker.**
The identifier a `.csproj` uses to declare which framework a project builds against: `<TargetFramework>net10.0</TargetFramework>`
— the go2cs corpus's own value today. It selects the reference assemblies compiled against, the runtime the
output binds to, and the **Native-AOT compiler**, since ILC arrives as a runtime pack chosen by the TFM rather
than by the SDK performing the publish. That is why moving go2cs to a new .NET release happens in two steps:
the SDK alone moves first (the compiler and build tooling change while the TFM stays put, so the C# language
level and diagnostics move but the runtime does not), then the TFM moves (one line per project, after which the
new runtime and its ILC are what is being measured).
<!-- Source: the .NET migration runbook, [`DotNetMigration.md`](DotNetMigration.md) (trap 1; §4 the SDK stage,
§5 the TFM stage). -->

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
