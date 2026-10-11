# go2cs Roadmap History

<!-- Moved VERBATIM from docs/Roadmap.md at master 2ff42f7a16 on 2026-09-29, when the roadmap was
     rewritten to hold only what is planned. Every section below is byte-identical to its text there
     and in its original order, headings included, so a link to one of those headings resolves on
     this page. Not carried: the old intro paragraph (replaced by the new Roadmap's) and an unfilled
     status placeholder paragraph. Phase 5 stays on the Roadmap, since none of it is complete. The
     future items inside the moved sections (Phase 4C's later test kinds, the host-limit retirement
     path, the two remaining sstring increments, the macOS flavor gap, the open converter items) are
     carried forward on docs/Roadmap.md, restated in present tense. -->

<!-- Amended 2026-10-10 (the Roadmap refresh at master 4c42cc7d3a): the intro says which parts of Phase 4
     this page records. No moved section is changed. -->

This page holds the completed phases of the go2cs roadmap and the git commits and tags that anchor
them. Each section keeps the heading and text it was recorded with while the work happened, so its
dates, figures and "next" items describe that moment. Of Phase 4, the sub-phases recorded here, 4A to
4C, are complete; its macOS validation and Phase 4D are still ahead and are on the
[Roadmap](Roadmap.md#phase-4-validation-against-gos-own-tests). Phase 4's validation objective is tracked
on the [roster](ValidatedTestPackages.md), which carries its current state. What is planned next is in
the [Roadmap](Roadmap.md).

> **Status (2026-07-10): Phases 0–3 done — the full standard library compiles.** All **302** packages of
> the auto-conversion (Go 1.23.1) build clean as .NET assemblies (commit `51ba5d9cf`,
> tag `stdlib-green-2026-07-10`) — the Phase-3 milestone. **Compiling, not yet operational:** Phase 4 will
> convert and run Go's own package tests; Phase 5 will use those tests to prove the C# implementations that
> replace assembly/cgo stubs.
>
> **⚠ Path note (2026-08-01):** Phase 1's separation was **undone deliberately** once Phase 3 plus 69
> operationally-validated packages made it unnecessary. The converted standard library now lives at
> **`src/core`** and the hand-finished stub baseline is retired (commit `2e8066da6`). Everything below
> that spells `src/go-src-converted` is a period-accurate record of where the tree stood at the time —
> read it as history, not as instructions. See [`/CLAUDE.md`](../CLAUDE.md) *One tree*.
>
> **⚠ Where the road goes after Phase 4 — the hop era.** Beyond the phases below, the project moves
> by **release migrations**: the Go release the corpus converts from, and the .NET runtime it runs
> on, one variable at a time and never both in one hop. Three documents carry that work, and this
> roadmap routes to them rather than restating them — the ruled frame in
> [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) (which releases, in which order, and its ruled
> open questions), and the two standing **runbooks** that lead on procedure:
> [`GoCorpusMigration.md`](GoCorpusMigration.md) for a Go corpus migration and
> [`DotNetMigration.md`](DotNetMigration.md) for a .NET one. What each document type may be used for,
> and which one wins when two disagree: [`Glossary.md`](Glossary.md), *Document types*.
>
> **⚠ AMENDED 2026-09-07 — the hop era does not wait for Phase 4 to reach 100%.** By owner ruling the
> corpus moves to **Go 1.24.13** with Phase 4's Go 1.23.12 record **closed at its anchor** rather than
> driven to completion, and the rows still unbanked there re-validate under the new release — a hop
> re-derives every roster row from scratch regardless (`GoCorpusMigration.md` H10). Read "after
> Phase 4" above as the order these phases were written in, not as a gate on the hop.

*Phase 2's 57-package `fmt` closure, considered and rejected as a bootstrap in 2026-06, is now simply
what the behavioral suite builds: cold ~48 s, warm ~4 s.*

## Milestones

High level timeline of the project's major turning points; the tags carry the details.

| Date | Milestone | Commit / Tag | Notes |
|:--|:--|:--|:--|
| 2018-05-21 | Project inception | `929d1457f` | A C#/.NET converter built on an ANTLR4 Go grammar. |
| 2022-03-13 | [`v0.1.2` release](NEWS.md#march-13-2022--v012-release) | [`v0.1.2`](https://github.com/ritchiecarroll/go2cs/releases/tag/v0.1.2) | The mature ANTLR4-era converter, tagged. |
| 2025-01-12 | [The converter is rewritten in Go](NEWS.md#january-12-2025--the-converter-is-rewritten-in-go-go2cs-version-2) | `87465f5f5` | Rebuilt on `go/ast` + `go/types`, with the `golib` runtime library and Roslyn source generators supplying Go's semantics. |
| 2025-05-05 | [First full standard-library auto-conversion](NEWS.md#may-5-2025--first-full-standard-library-auto-conversion) | `6ca1c45b7` · [`full-conversion-2025-05`](https://github.com/ritchiecarroll/go2cs/releases/tag/full-conversion-2025-05) (`cc14584c7`) | Every Go file gets a C# file; compiling comes later. |
| 2026-07-10 | [**First clean full-standard-library compile**](NEWS.md#july-10-2026--the-entire-go-standard-library-compiles-in-net) | `51ba5d9cf` · [`stdlib-green-2026-07-10`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-green-2026-07-10) | Every package compiles with zero errors, `runtime`, `reflect` and `net/http` included ([details](StdLibCompileMilestone.md)). |
| 2026-07-14 | [Standard library on NuGet](NEWS.md#july-14-2026--the-converted-go-standard-library-is-on-nuget) | `2363af0e6` · `dd821a556` · [`nuget-stdlib-2026-07-14`](https://github.com/ritchiecarroll/go2cs/releases/tag/nuget-stdlib-2026-07-14) | A converted app references the standard library from nuget.org, with no local go2cs checkout. |
| 2026-07-17 | [**First Go test suite passing in C#**](NEWS.md#july-17-2026--gos-own-tests-now-pass-in-c) | `337a928df` · [`utf8-tests-green-2026-07-17`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf8-tests-green-2026-07-17) | `unicode/utf8` matches `go test`, test for test. |
| 2026-08-08 | [**Go programs run on Linux**](NEWS.md#august-8-2026--go-programs-run-on-linux) | [`linux-first-run-2026-08-08`](https://github.com/ritchiecarroll/go2cs/releases/tag/linux-first-run-2026-08-08) | Converted programs, the `fatih/color` walkthrough included, match `go run` on Linux. |
| 2026-10-01 | [**Every implementable standard-library package validates**](NEWS.md#october-1-2026--every-implementable-standard-library-package-validates) | `133ca704e` · `nuget-1.24.13.3` | Every implementable package passes its own Go tests in C#, `runtime` included, on Windows and Linux. |

## Phase 0 — Documentation ✅ done

Orientation docs so any task starts informed: [`/CLAUDE.md`](../CLAUDE.md),
[`Architecture.md`](Architecture.md), [`Baseline-vs-FullConversion.md`](../src/archived/Baseline-vs-FullConversion.md),
this file; plus a refresh of `README.md`.

## Phase 1 — Restore the separation (repo surgery) ✅ done

Goal: `src/core` = compiling baseline; `src/go-src-converted/` = full WIP; `golib` shared.

- Tagged the full conversion (`full-conversion-2025-05` → `cc14584c7`).
- Relocated the auto-generated stdlib out of `src/core` into `src/go-src-converted/` (2604-file rename);
  rewrote csproj/sln refs; added `.gitignore` rules for the Go `debug`/`log` package name collisions.
- Scoped `src/go2cs.sln` to the baseline; added `src/go-src-converted.sln` for the WIP.
- Fixed `deploy-core.bat` (`gocore`→`core`). `convert-gosrc.*` retarget still pending.

## Phase 2 — Green the loop ✅ done (via stub restore)

**Outcome:** rather than green the full `fmt` closure bottom-up (57 packages incl `runtime`/`syscall`/`os`),
we **restored the old hand-finished stub** (`3426298eb`) into `src/core` — it compiles cleanly against
today's `golib`, so the test loop went green immediately. Plus several converter fixes (below). The
57-package-closure analysis below is retained as context for **Phase 3** (the full conversion).

### How the baseline-restore path worked

The behavioral tests reference `golib` + the `go2cs-gen` analyzer + a few stdlib projects (`fmt` mostly,
plus `time`/`unsafe`/`strings`/`sort`/`math/rand`/`io`). The **stub** `fmt` is a minimal proxy with a tiny
closure (errors, io, strings, sync, math, …) — it avoids the runtime substrate, which is why restoring it
is the fast path to green. Restored 14 packages; excluded the stub `testing` (drifted, unused by tests).

### The full-`fmt`-closure path (NOT taken for the baseline; relevant to Phase 3)

The behavioral tests reference `golib` (all 59) + the `go2cs-gen` analyzer (all) + a few stdlib projects:
`fmt` (55 tests), `time` (5), `unsafe` (3), `strings` (1), `sort` (1), `math/rand` (1), `io` (1).

But these pull a **transitive dependency closure**. `fmt` alone references:
`errors, internal/fmtsort, io, math, os, reflect, slices, strconv, sync, unicode/utf8` (and those pull
more). So "green the loop" means **green `fmt`'s closure**, bottom-up.

1. **Compute closure + order.** Reuse `stdLibConverter.go`'s topological `sortedQueue`, restricted to the
   baseline roots, to get a leaf-first build order (`unsafe`, `internal/abi`, `unicode/utf8`, `math/bits`,
   … up to `fmt`).
2. **Green packages bottom-up.** For each package in order: `dotnet build` against current `golib`; fix the
   **converter** (preferred) — or, for genuinely out-of-band pieces like `builtin`, the **runtime** — until
   it compiles. Crib from the `go2cs-stub-ref` worktree when a hand-finished prior approach helps.
3. **Lock it in with a regression test.** Each greened package gets a behavioral/compile test so it stays
   green (mirrors the "raw code vs target file" comparison mode noted in `src/go2cs/ToDo.md`).
4. **Exit criterion.** Baseline `src/core` builds clean **and** all 59 `src/tests/Behavioral` projects pass.
   The converter-improvement loop is restored.

### Phase 2 findings (measured 2026-06-25)

- **`fmt`'s closure is 57 packages** — the *full* `fmt` pulls in the entire runtime substrate
  (`runtime`, `syscall`, `os`, `reflect`, and a deep `internal/*` tree), unlike the old 36-line stub `fmt`.
- **Baseline build status: 18 / 57 compile clean** out of the box (built in place in `go-src-converted`
  against current `golib`). The pipeline works; failures are concentrated converter defects, not intractability.
- **Prioritized converter-defect roadmap** (own-errors across a probe of failing leaf/mid packages):

  | Count | Code | Meaning | Status |
  |---|---|---|---|
  | 48 | CS0106 | invalid modifier (`static readonly` on a function-local named-type const) | **FIXED** — `visitValueSpec.go`, verified math/bits 16→0 |
  | 20 | CS1002 | `;` expected | open (syntax cluster) |
  | 18 | CS1519 | invalid token in member declaration | open (syntax cluster) |
  | 18 | CS1026 | `)` expected | open (syntax cluster) |
  | 18 | CS1003 | syntax error, X expected | open (syntax cluster) |
  | 18 | CS0051 | inconsistent accessibility (param type less accessible) | open |
  | — | CS0103 | missing package-level lookup tables (e.g. `ntz8tab`/`pop8tab` in math/bits) | open |

- **Converter-improvement loop (proven end-to-end):** edit `src/go2cs/*.go` → `go build` (Go 1.24.13) →
  re-transpile → `dotnet build`. (For behavioral tests the harness runs this loop itself — see
  [`/CLAUDE.md`](../CLAUDE.md) "Test-harness mechanics".)
- **Retarget detail:** the stdlib converter writes to **`<go2cspath>/core/<pkg>`** (hardcoded `core` subdir).
  To regenerate cleanly into `src/go-src-converted`, point `-go2cspath` accordingly or convert to a temp dir
  and move. Batch several converter fixes, then do one wholesale reconvert + re-measure.

### Converter fixes landed (2026-06-25)

Each verified by rebuild + reconvert + compile:
- `golib/UntypedInt` missing semicolon (CS1002) — blocked the whole runtime.
- `static readonly` on a function-local named-type const → emit a plain local (CS0106) — `visitValueSpec.go`.
- Multi-line type-constraint unions rendered with only the first line commented → collapse to one comment
  line (`visitInterfaceType.go`).
- `~[]E` slice constraint wrongly given `IEqualityOperators` — `*Slice`/`*Array` cases were backwards and an
  empty constraint-type set counted as a subset of every operator set (`constraintOperations.go`).
- Switch fallthrough case with a single pattern value emitted an unbalanced `)` (`visitSwitchStmt.go`).
- Negative typed const (`int8 = -1`) promoted to `GoBigConst` because the range check used `ParseUint`
  (rejects negatives) → also try `ParseInt` (`visitValueSpec.go`).

## Phase 3 — Drive the full conversion to compile ✅ done (2026-07-10)

**Outcome:** all **302** packages of `src/go-src-converted` (the full Go 1.23.1 standard-library
auto-conversion) compile clean as .NET assemblies — zero errors, zero exclusions — reached 2026-07-10
(commit `51ba5d9cf`, tag `stdlib-green-2026-07-10`). `runtime`, `reflect`, `net/http`, `go/types`,
`crypto/tls`, `database/sql` and every other package are included. Every fix was root-caused against real
emitted C#, locked in by a Go-vs-C# behavioral regression test (371 and counting), and verified byte-for-byte
against the full corpus before landing. **Compiling is the milestone, not operational** — running each
package's own tests is Phase 4. The narrative of the two-week finishing campaign is in
[About Standard Library Compile Milestone](StdLibCompileMilestone.md)
(the announcement itself is archived in [`NEWS.md`](NEWS.md)); the strategy that got there is below, followed by the early iteration
log (retained as historical record — its dated "Next:" items are long since done).

The original Phase-3 plan:

1. **Build-error roadmap.** Convert + `dotnet build` `src/go-src-converted/`, capture `build.log`; bucket
   compile errors by **frequency** and by **Go feature**. This is the prioritized work queue (the README
   already frames this log as the road map).
2. **Fix by error class, bottom-up the full DAG.** Highest-frequency converter defects first (each fix
   clears many packages). Re-convert, re-bucket, repeat. **Measure by packages-compiling and error-count**,
   never by "conversion succeeded."
3. ~~**Promote, don't fork.** When a full package compiles cleanly and matches behavior, promote it toward
   the baseline; `golib`'s hand-written core stays shared and never auto-overwritten. Track promotions
   here.~~ **SUPERSEDED (2026-07-01):** promotion `go-src-converted → core` is **deferred to post-Go-test
   (Phase 4)** and may not be needed at all. Compiling ≠ operating; a clean compile is NOT a promotion
   trigger. `core` stays the bootstrap **stub**. The hand-owned `[module: GoManualConversion]` / `*_impl.cs`
   files live in `core` and are copied **back** into `go-src-converted` (the real final state). See
   [`Baseline-vs-FullConversion.md`](../src/archived/Baseline-vs-FullConversion.md) *The corrected end-state* and its
   contract rules 4–5. `golib`'s hand-written core stays shared and never auto-overwritten.

### Phase 3 iteration 1 — converter fixes landed (2026-06-25)

Measurement workflow used (no wholesale commit of `go-src-converted`; it stays regenerable):
reconvert the stdlib to a temp dir (`go2cs -stdlib -comments -go2cspath <tmp>` — always `-comments` so the
Go authors' BSD license header survives; output lands in `<tmp>/core/<pkg>`),
overlay the fresh `.cs` onto `src/go-src-converted/<pkg>` (keeping the relocated csprojs, or regenerating
them and rewriting `$(go2csPath)core\` → `$(go2csPath)go-src-converted\` except `core\golib`), then
`dotnet build src/go-src-converted.sln` and bucket. Single packages build with
`-p:go2csPath=<repo>\src\` so the `$(go2csPath)` golib ref resolves outside the solution.

Four converter defects fixed (each verified by reconvert + compile of an affected package; **behavioral
suite stays green: 216/216**):

| Defect | Symptom (top error class) | Fix |
|---|---|---|
| Variadic of a **type parameter** (`func Or[T any](v ...T)`) emitted a namespace-level `using ꓸꓸꓸT = Span<T>` alias — `T` out of scope | CS0246 `'T' not found` | `visitFuncDecl.go`: when the variadic element is a `*types.TypeParam`, emit `params Span<T>` inline (C# 13 params-collections) instead of the alias. |
| Go built-in **`comparable`** constraint emitted as bare `comparable` (golib's type is generic `comparable<T>`) | CS0305 `requires 1 type argument` | `main.go` `getGenericDefinition`: special-case `comparable` → `comparable<T>`. |
| **Bodyless** (assembly/cgo) funcs emitted as accessibility-modified `partial` methods with no implementing half | CS8795 (49) — biggest cluster | `visitFuncDecl.go`: emit a non-partial throwing stub (`=> throw new NotImplementedException(...)`) until an asm/cgo backend exists. |
| **Filename build-constraint over-exclusion**: `isFileNameCompatible` treated *any* unknown `_word` suffix (e.g. `bits_tables.go`, `bits_errors.go`) as a failing platform tag, silently dropping the file → missing symbols | CS0103 (`pop8tab`/`ntz8tab`/`divideError`…) | `directiveOperations.go`: only a trailing recognized `_GOOS`, `_GOARCH`, or `_GOOS_GOARCH` constrains the build (full GOOS/GOARCH name tables added); descriptive suffixes impose no constraint. |

The filename fix is the highest-impact: a full reconvert went from **1472 → 1660 emitted `.cs` files** (~188
previously-dropped stdlib source files now converted). That raises the raw error count (144 → 224) because
newly-included files surface their own latent defects — so **track packages-compiling, not error count**,
this phase.

### Phase 3 iteration 2 — `internal/cpu` address-of-field (2026-06-25)

`internal/cpu/cpu_x86.cs` owned ~140 of the syntax errors, all from `&cpu.X86.HasADX` (address of a field
of the anonymous-struct package global `X86`). Two stacked bugs, both fixed; **behavioral suite stays green
216/216**:

| Bug | Symptom | Fix |
|---|---|---|
| The anonymous struct type of `X86` is lifted to `X86ᴛ1` while visiting `cpu.go`, but `liftedTypeMap` is **per-file** (each file gets its own concurrent `Visitor`), so `cpu_x86.go` couldn't resolve it — `getExprTypeName` fell back to the raw struct text, mangled to `cpu.CacheLinePad}` | syntax errors (`)`/`;` expected …) | New **package-level shared registry** (`packageDynamicTypeNames`, signature→C# name) populated by `visitStructType` for package-level lifts; `convUnaryExpr` emits a marker for unresolved anonymous structs, resolved after the file-visit barrier (`dynamicTypeOperations.go`, `main.go`). Race-free: resolution runs post-`Wait()`. |
| With the name fixed, `&X86.HasADX` emitted `ᏑX86.of(…)` (identifier form), assuming a heap-boxed pointer companion — but a package-global value var has none | CS0103 `ᏑX86 does not exist` | `convUnaryExpr`: choose the address-of form by escape state — `isHeapBoxedExpr` (mirrors the existing escape check) → `Ꮡvar.of(…)` for escaping locals, else the constructor form `Ꮡ(value).of(…)` (consistent with the existing whole-value `&global` path). |

Result: `internal/cpu` went **~140 → 8 errors**. Caveat: `Ꮡ(value)` heap-allocates a *copy* (golib
`Ꮡ<T>(in T)`), so `&global.field` currently points into a copy — a pre-existing whole-category limitation
(`&global` already did this on line 124); the proper fix is **boxed companions for package-global vars whose
address is taken** (future work).

### Phase 3 iteration 3 — `internal/cpu` compiles clean (2026-06-26)

The remaining 8 `internal/cpu` errors are fixed — **`internal/cpu` is the first full-conversion stdlib
package to compile clean** (was the ~140-error blocker). Three general fixes, all behavioral-green (228/228);
re-transpiling all 61 behavioral projects left every golden byte-identical (no converter-output regression),
and the whole solution rebuilds clean against the changed golib:

| Defect | Symptom | Fix |
|---|---|---|
| Large untyped constant typed by value-range as `(nint)…L` even in an unsigned context (`cpuid(0x80000000, 0)`) | CS1503 `nint → uint` (×6) | `convBasicLit`: in the `> int32` branch, if the literal's contextual type is unsigned (`isUnsignedType` via `info.Types`), emit an unsigned C# literal (`2147483648U` / `…UL`). |
| Slicing an `@string` returned `slice<byte>`, so `field[:4] != "cpu."` was `slice<byte> != string` | CS0019 | **golib**: `@string this[Range]` now returns `@string` (Go string slicing yields a string). Runtime-only — no `.cs` change. |
| Empty-string literal in a tuple assignment emitted `""u8` (a `ReadOnlySpan<byte>` ref struct) — illegal as a ValueTuple element | CS9244 | `visitAssignStmt`: suppress the u8 form for string literals in a multi-value (tuple) RHS (`field, env = env, ""`). |

Guarded by the `StringSliceAndUnsignedConst` behavioral test.

### Phase 3 iteration 4 — address-of-global correctness (2026-06-26)

`Ꮡ(value)` heap-allocates a **copy**, so `&global` / `&global.field` pointed into a copy — mutations never
reached the global (e.g. `internal/cpu.doinit` set feature flags on a throwaway copy of `X86`). Fixed by
backing **address-taken** package-global vars with a heap box, so the pointer references the original:

- New `globalAddressOperations.go`: a synchronous **pre-pass** (`collectAddressedGlobals`) scans all files for
  `&g` / `&g.field` / `&g[i]` rooted at a package-level var → `packageAddressedGlobals` (cross-file, since the
  global may be declared in one file and addressed in another).
- `visitValueSpec`: an addressed global is emitted as a box + ref-property —
  `static ж<T> ᏑG = new(default(T)); static ref T G => ref ᏑG.Value;` — instead of `static T G;`. Reads/writes of
  `G` are unchanged (the ref-property forwards to the box). Only address-taken globals are boxed; everything
  else keeps the plain field, so the blast radius is tiny (only `GlobalStructFieldPointers` re-transpiled).
- `convUnaryExpr` / `isHeapBoxedExpr`: for an addressed global, emit the identifier form `ᏑG` (the box) rather
  than `Ꮡ(G)` (a copy). `&X86.HasADX` → `ᏑX86.of(X86ᴛ1.ᏑHasADX)`.

`internal/cpu` still compiles clean and now mutates the real `X86`. Behavioral green; `GlobalStructFieldPointers`
strengthened to assert the global itself is mutated (would print `false/0` before the fix).

**Known limitation:** cross-package `&otherPkg.ExportedGlobal` isn't boxed (only globals addressed within
their own package are detected).

### Phase 3 iteration 5 — anonymous-struct global declarations (2026-06-26)

A package-global var whose type is inferred from an anonymous-struct composite literal
(`var S = struct{…}{…}`) emitted the raw `struct{…}` text as its C# declaration type
(`public static struct{A int; B int} S = new Δtype(…);`) — invalid C#. The value was lifted to a named type
but the declaration wasn't (the lifting happened inside the composite literal, *after* the declaration type
name was resolved). Fix in `visitValueSpec`: for a package global with an inferred anonymous-struct type, lift
the struct with the var name **before** resolving the declaration type (mirroring the explicit-type path), so
both the declaration and the value share one lifted name (`Sᴛ1`). This also unblocks **boxing** such globals,
so addressed anonymous-struct globals (`&S.field`) now work too. Behavioral green; zero existing goldens
changed (no behavioral test had an anonymous-struct global). Guarded by an extension to the `AnonymousStructs`
test (a package-global anonymous-struct var, read and mutated through a field pointer).

### Phase 3 iteration 6 — TypeGenerator CS0051 (unexported embedded marker) (2026-06-26)

A public struct embedding an unexported marker type as a blank field (`_ noCopy`, the
`sync/atomic.Bool` pattern) made the **`TypeGenerator`** (Roslyn) emit `public Bool(noCopy _)` —
a public constructor whose parameter type `noCopy` is `internal` → CS0051. Root cause:
`GetScope("_")` returns `"public"` (the `firstChar == '_'` rule), so the blank embedded field
was classified as a public member and drove the public ctor. Fix in `StructTypeTemplate.PublicStructMembers`:
exclude blank/underscore-prefixed fields (never exported in Go) from the public constructor. All CS0051
in `sync/atomic` cleared; behavioral green (232/232 + the new test). Guarded by `UnexportedEmbeddedMarker`.

### Phase 3 iteration 7 — asm-function companion source generator (2026-06-26)

Resolves the iteration-6 follow-up. Bodyless (asm/cgo) Go functions are once again emitted by the converter
as `partial` *declarations* (reverting iteration-1's non-partial throwing stub). A new **`PartialStubGenerator`**
(`go2cs-gen`) emits a throwing `partial` *implementation* for every bodyless `partial` method that has **no**
other implementing part in the compilation (`IMethodSymbol.PartialImplementationPart is null`). So:
- packages that ship a hand-written companion (`sync/atomic`'s `doc_impl.cs`, real `Interlocked` bodies) use
  those bodies — the generator detects the impl and skips them; and
- companion-less packages (`crypto/internal/boring/sig`, `crypto/subtle`, …) get a generated throwing stub,
  so they compile instead of CS8795/CS0111.

**`sync/atomic` now compiles clean** — the second full-conversion stdlib package to go green (after
`internal/cpu`). `sig` compiles too. Behavioral suite stays green (the generator is a no-op for the tests —
none contain asm functions; zero behavioral `.cs` changed). Not behaviorally testable (Go rejects a bodyless
function without an `.s` file), so verified via the full-conversion packages compiling.

### Phase 3 — promotion gate finding: `sync/atomic` typed API is broken (2026-06-26)

A behavioral validation test (atomic ops, Go-vs-C# output, referencing `go-src-converted/sync/atomic`)
was written as the gate before promoting `sync/atomic` to the baseline. **It failed, and that is the
point — "compiles" ≠ "correct".** The package-level functions (`atomic.AddInt32(&n, 3)`) work, but the
**typed atomic types are broken**: `var i atomic.Int32; i.Store(10); i.Add(5); i.Load()` yields `0` in C#
instead of `15` (Go).

Root cause: a method like `func (x *Int32) Store(v) { StoreInt32(&x.v, v) }` converts `&x.v` (address of a
field of the **pointer receiver**) to `Ꮡ(x.v)`, which **boxes a copy** (the `ж(in T)` ctor copies), so the
atomic op never touches the real field. Attempting the fix uncovered a deep stack of issues in the
**receiver-capture mechanism**, which is the only way to get a non-copying pointer to a receiver field:
1. `&recv.field` must use the captured receiver box (`Ꮡx.of(Type.ᏑField)`), not `Ꮡ(x.field)` — and the
   detection has to run *before* the struct-field gate (a pointer receiver's selector type is a pointer).
2. The capture field name (`<Method>ꓸᏑx`) **collides** across overloaded same-named methods on different
   receiver types (`Int32.Add`, `Int64.Add`, …) — needs the receiver type in the name (converter + generator).
3. The capture field is a **static `ThreadLocal`** on the (non-generic) package class, so it **cannot hold a
   generic receiver's `T`** (`atomic.Pointer[T]`).
4. Even fixed, the `ThreadLocal` is only initialized when the method is called via the `ж` (pointer) overload;
   a value-receiver-style call (`i.Store(10)`) routes through the `ref` overload and the capture is **never
   initialized** (runtime "Receiver target … is not initialized").

**Conclusion:** `sync/atomic` is **not promotable** — its primary API doesn't work, and the receiver-field
address / capture machinery needs a substantial rework (likely replacing the static-`ThreadLocal` capture
with something that works for value calls and generic receivers). `internal/cpu` likewise isn't promotable
(asm `cpuid` is a stub). Promotion stays **pull/validation-driven**; this gate correctly blocked it.

### Receiver-field address / capture rework — staged (2026-06-26)

Making `&recv.field` reference the real receiver field (the `sync/atomic` typed-type unlock). Split into two
stages:

- **Stage A — DONE (commit `36363ef16`, behavioral green 236/236).** `convUnaryExpr` emits the captured-box
  field-ref form (`<capturedName>.of(Type.ᏑField)`) for `&recv.field` on a non-generic pointer receiver (the
  detection must run *before* the struct-field gate, since the receiver's selector type is a pointer). The
  captured-receiver field name is made unique per receiver type (`<Method>_<RecvType>ꓸᏑx`) so it doesn't
  collide across overloaded same-named methods (`Int32.Add`, `Int64.Add`, …) — coordinated in the converter
  (`getCapturedReceiverName`) and the generator (`ReceiverMethodTemplate`).
- **Stage B — DONE (commit `9aeaf29e2`, behavioral green 236/236).** Value-receiver calls of capture-mode
  methods now route through the `ж` overload so the captured form references the real field:
  - `captureModeOperations.go` — a pre-pass (`collectCaptureModeMethods`) scans the package **and its
    transitive imports** (`LoadAllSyntax` provides dep ASTs) for non-generic pointer-receiver methods taking
    `&recv.field`, keyed by the interned `*types.Func` so cross-package call sites match;
  - `escapeAnalysisOperations.go` — a value var on which a capture-mode method is called is marked escaping
    (heap-boxed), so its `Ꮡname` companion exists;
  - `convSelectorExpr.go` — the call is routed through the `ж` overload (`Ꮡi.Store(10)`).
  Also fixed an **inverted `CompareAndSwap`** in the hand-written `sync/atomic` companion (`doc_impl.cs`):
  `Interlocked.CompareExchange` returns the original, so a swap succeeded iff `== old` (was `!= old`) — found
  by the validation test. **Result: `sync/atomic`'s scalar typed types (`Int32/64`, `Uint32/64/ptr`, `Bool`)
  now work end to end** — `var i atomic.Int32; i.Store(10); i.Add(5); i.Load()` → 15, CAS/Swap/etc. all match
  Go. Guarded by the self-contained `ReceiverFieldAddress` behavioral test (no `go-src-converted` dependency).
- **Generic receivers (direct-`ж`) — DONE.** Generic capture-mode receivers (`atomic.Pointer[T]`) are now
  emitted with the heap box **as the receiver** (`this ж<Box<T>> Ꮡx` + `ref var x = ref Ꮡx.Value;`), and
  `&x.field` field-refs through the box parameter (`Ꮡx.of(Box<T>.ᏑField)`) — `T` stays in scope, no static
  field needed. `[GoRecv]` is suppressed automatically (the signature is `this ж<…>`, not `this ref …`, so
  `RecvGenerator` skips it and there is no duplicate overload). Value calls heap-box and route through the ж
  overload (the Stage-B escape/routing already handles this once the generic methods are included). Converter
  changes: `captureModeOperations.go` (new `packageDirectBoxReceiverMethods`, keyed by `*types.Func.Origin()`
  so instantiated call sites match), `visitFuncDecl.go` (un-skip the receiver deref + emit `ж<T>` receiver),
  `convUnaryExpr.go` (generic branch → `Ꮡx.of(...)`), and `main.go` `getFullTypeName` (append type args for
  instantiated **cross-package** generics, else a boxed `atomic.Pointer[Config]` emits `new …Pointer()` with
  no `<Config>`). Validated by the **`GenericReceiverFieldAddress`** behavioral test (a generic `Box[T]`
  with `Set`/`Get` taking `&b.v`, exercised for `int` and `string`); behavioral suite green, zero churn.
- **Direct-`ж` extended to ALL field-address capture methods — fixes a concurrency bug.** The non-generic
  capture path used a static `ThreadLocal<ж<T>>` field reassigned per call (`new ThreadLocal<…>(() => Ꮡx)`),
  which is a **shared static** — concurrent calls on *distinct* receivers race on that field, so e.g.
  `u0.Load()` can return `u6`'s value (proven by an 8-thread stress test: each thread Loads its own
  `atomic.Uint32`, sees another's). Broken precisely for the concurrent types atomics exist for, and invisible
  to the single-threaded behavioral suite. Fix: `captureModeOperations.go` now marks *every* `&recv.field`
  capture method (generic **and** non-generic) direct-`ж`, so the box is the receiver parameter — no shared
  state, and alloc-free. `convUnaryExpr.go` always emits `Ꮡx.of(...)`; `visitReturnStmt.go` returns `Ꮡrecv`
  for a direct-box method (keeps a method that both takes `&recv.field` and returns the receiver consistent).
  Re-ran the stress test → "no race observed". `core/sync/atomic/type.cs` scalars re-emitted to direct-`ж`
  (the file is `GoManualConversion`, so re-emitted by hand from a reconvert, keeping the managed `Pointer<T>`).
  Guarded by the `AtomicValues` test; only the `ReceiverFieldAddress` golden changed (intended, still correct).
- **Return-receiver capture converted too, and the ThreadLocal mechanism DELETED.** The other capture trigger
  — `func (t *T) Common() *T { return t }` (`internal/abi`) — now also emits direct-`ж` (`captureModeOperations.go`
  `bodyReturnsReceiver` marks it direct-box; `visitReturnStmt.go` returns `Ꮡrecv`). With **both** triggers on
  direct-`ж`, nothing emits `[GoRecv("capture")]` anymore, so the whole racy mechanism is dead and was removed:
  converter (`captureReceiver` field, `getCapturedReceiverName`, the `[GoRecv("capture")]` emission) and
  generator (`ReceiverMethodTemplate`'s `CapturePointer`/`CaptureName`/`CaptureDeclarations`/`CaptureOperation`
  + the `ThreadLocal` `using`, and `RecvGenerator`'s now-unused `Options` arg) — ~55 lines gone, generated `ж`
  overloads are now a clean deref+delegate. Baseline `core/internal/abi/type.cs` `Common` surgically updated to
  direct-`ж` (promotion overloads regenerate correctly); `StdLibInternalAbi` golden updated. Full solution green;
  32/32 capture-affected behavioral phases pass; stress test still "no race observed". There is now **one** way
  a pointer-receiver method captures its box (the `ж<T>` parameter) — no thread-local, no shared state.
- **`sync/atomic` PROMOTED to `core` — first stdlib package migrated.** `src/core/sync/atomic` is now in the
  green baseline (`go2cs.slnx`, `/core/` folder), referencing `core/unsafe` + `core/golib`. Scalar typed
  types are the converter output as-is; `Pointer[T]` is **hand-rewritten** in the promoted (hand-owned) copy.
  - **The `unsafe.Pointer` finding (why the rewrite, not a global fix).** `unsafe.Pointer` is a type **alias
    to `nuint`** (`[assembly:GoTypeAlias("@unsafeꓸPointer","nuint")]`) — a raw number — used across **171
    files / ~1522 `ж↔uintptr` sites** in the full conversion, almost all *legitimately* numeric (pointer
    arithmetic, syscall, reflect). In .NET a managed reference **cannot** be stored as a number and survive a
    GC move (CLR rule), so golib's `ж↔uintptr` operators (`ж.cs:449-458`) can't round-trip a managed `ж<T>`:
    the zero case NREs (`*(T*)0`) and a real case dangles (a `fixed`-pin address read after the block closes).
    A *global* `unsafe.Pointer` redesign would be reckless (huge blast radius, breaks the correct numeric
    uses), so the contained fix lives in the one type that stores a managed pointer.
  - **The managed `Pointer<T>` (`core/sync/atomic/type.cs`).** Field `v` is a managed `ж<T>` (not
    `@unsafe.Pointer`); `Load`/`Store` use `Volatile.Read/Write`, `Swap`/`CompareAndSwap` use
    `Interlocked.Exchange`/`CompareExchange<ж<T>>`. A `nilCanon` helper collapses an explicit nil-`ж` to
    `null` so reference-based CAS treats every nil pointer as equal (matching Go's `nil == nil`). No
    `unsafe.Pointer`, GC-safe, nil-safe.
  - **Validated** end to end: `atomic.Pointer[int]`/`[Config]` Load/Store/Swap/CAS + the scalar API all match
    Go. Guarded by the **`AtomicValues`** behavioral test (scalar `Int32` + generic `Pointer[int]`, both
    paths). Solution builds green; behavioral suite green (zero churn).
  - **csproj gotcha:** `core/sync/sync.csproj` had no subfolder exclusion (it never had one); adding
    `core/sync/atomic` made it swallow atomic's generated files (`CS0579`). Fixed by mirroring `math.csproj`'s
    `<Compile Remove="rand\**" />` pattern → `<Compile Remove="atomic\**" />`.
- ~~Promote `internal/cpu` and `sync/atomic`~~ — atomic gated on the `unsafe.Pointer`/`ж` representation
  above (scalars work, `Pointer[T]` runtime-broken); cpu gated on asm `cpuid`.
- Confirm `internal/cpu` / `sync/atomic` build within the full
  `go-src-converted.sln` alongside their dependents.
- Re-bucket after a fresh full reconvert to find the next highest-frequency converter defect.

### Phase 3 iteration 8 — re-bucket + 4 converter/generator fixes (2026-06-26)

Fresh full reconvert (305 pkgs, 1659 `.cs`) + full `go-src-converted.sln` build, re-bucketed by `CS####`
frequency. **Measurement gotcha caught:** the reconvert-overlay must NOT blanket-delete `.cs` first — ~15
hand-written companion/pseudo-package files (`unsafe/unsafe.cs`, `*_impl.cs`, generator companions) are not
regenerated by the converter, and deleting them produces a phantom ~120-error `unsafe_package` CS0246 bucket.
Overlay by copying generated `.cs` over; `git checkout --` any deletions.

After clearing the phantom bucket, the real own-error defects bucketed as: reflectlite (24, CS1537 dup
`global using`), container/list (6→25 post-fix) + container/ring (12) sharing one `ж<T>` defect, unicode (4),
internal/types/errors (4), plus runtime/unsafeheader-missing-ref cascades. Four fixes landed (behavioral
suite green throughout; zero existing goldens changed by any):

| Defect | Symptom | Fix |
|---|---|---|
| **Generator dup `global using`** — `GetFullyQualifiedUsingStatements` (all 5 generators) copied `global using` alias directives from the source file as file-local `using`, colliding with the in-scope global one | CS1537 (×24 in reflectlite, via `PartialStubGenerator` asm stubs) | `go2cs-gen/Common.cs`: skip `global using` directives (already in scope everywhere, generated files included). reflectlite 24→0. Not behaviorally testable (asm-stub-only path); verified via the full conversion. Commit `9c9431b3f`. |
| **Mixed-type for-init** — `for i, e := s.Len(), s.Front()` (int + pointer) emitted two `;`-separated decls inside the C# for-init clause (invalid: the `;` ends the clause); the combined `var (a,b)` form is blocked by the int special-casing | CS1002/CS1003 (container/list + ~20 files) | `visitAssignStmt.go` (+ `forInit` flag in `FormattingContext`/`visitForStmt`): emit a tuple-deconstruction declaration with per-element types — `(nint i, var e) = (...)`. Gated on all-new, non-heap-boxed LHS. Guarded by `ForInitMixedTypes`. Commit `6d339a3d0`. |
| **Variadic-of-pointer param** — `func In(r rune, ...*RangeTable)` emitted an invalid using alias `ꓸꓸꓸж<RangeTable> = Span<…>` (alias identifier can't contain `<`/`>`) | CS1002/CS1022 (unicode) | `visitFuncDecl.go`: emit `params Span<T>` inline when the element type is generic/pointer (`Contains "<"`), extending iteration-1's type-parameter special case. |
| **Empty/spread variadic-of-pointer call** — `In(r)` (no trailing args) panicked the converter (`Args[i]` indexed past end); `f(slice...)` emitted `Ꮡslice` (element address-of applied to the spread slice) | converter panic → dropped file; CS0103 | `convCallExpr.go`: guard the element-pointer arg treatment with `paramHasArg` (empty call) and `!(hasSpreadOperator && last param)` (spread). |

The last two ship together with the `VariadicPointerParam` behavioral test (args/empty/single/spread calls).

### Phase 3 iteration 9 — blank-identifier collision (CS0102) (2026-06-26)

A package declaring blank `_` constants (skipping `iota` values) **and** a blank `func _()` (the stringer
compile-time-assertion idiom — e.g. `internal/types/errors`) emitted multiple `internal static readonly … Δ_`
fields that collided: CS0102 "already contains a definition for 'Δ_'". Root cause: `performNameCollisionAnalysis`
recorded `_` in both the named-element set (the blank consts) and the method-name set (`func _()`), flagged it
as a const↔method collision, and `getSanitizedIdentifier` Δ-prefixed every `_` to the same `Δ_` — defeating
the value-spec visitor's per-blank unique naming (`_ᴛNʗ`). Fix (`nameCollisionAnalysisOperations.go`): exclude
the blank identifier from collision analysis (it is a discard, never referenced, and already gets unique names).
internal/types/errors CS0102 4→0 (remaining: a `strconv` project-ref, separate). Guarded by `BlankIdentifierCollision`;
behavioral suite green, zero existing goldens changed. **Found-but-deferred:** a bare discard `_ = expr` *inside*
a `func _()` emits `_ = …` which binds to the method group → CS1656 ("cannot assign to '_'") — a separate edge
case (real stringer asserts use `_ = x[C-C]`); not hit by internal/types/errors' actual body.

### Phase 3 iteration 10 — accurate re-bucket + pointer-copy fix (2026-06-26)

**Measurement-methodology finding (important):** the committed `go-src-converted` csprojs are stale and lack
inter-package `ProjectReference`s that the *current* converter emits correctly. Overlaying fresh `.cs` onto those
stale csprojs inflated the bucket with phantom CS0246 "package not found". Regenerating the csprojs from the fresh
conversion (with the documented `core\`→`go-src-converted\` rewrite, golib excepted) dropped the total **95→79**
and CS0246 **23→5**. **So the measurement loop must regenerate csprojs, not keep the committed ones** — there is no
converter csproj-emission defect. Reusable overlay script: `scratchpad/overlay.sh`.

True own-defect leaders after the rewrite: container/list (25) + ring (12) = the `ж<T>` model; internal/chacha8rand
(7, mostly the same `ж<T>` pattern — `State.Init64`/`Refill` on a value needing the box); math/bits (4, unsigned-
arithmetic/shift-count coercions); a handful of 1–2-error leaves.

**The `ж<T>` model split into two sub-problems** (converter derefs a pointer param/receiver to a value alias
`ref var x = ref Ꮡx.Value`, losing pointer identity when Go uses it *as* a pointer):
- **Sub-problem B — DONE (this iteration).** `r := p` / `r = p` (pointer copy of a *T parameter) emitted `var r = p`
  (a copy of the pointed-to value); the converter already treats the walked target as a pointer (`r.Value`/`~r`), so
  it miscompiled. Fix (`visitAssignStmt.go`): a plain pointer-typed identifier on an assignment RHS now gets the
  pointer (box) form — `var r = Ꮡp` — via a new `rhsPointerCopyContext`/`appendRhsPtrContext` helper applied in both
  the declare/reassign branch and the per-variable (escaping-var) branch. A pointer *local* already holds the pointer
  directly, so it is unchanged → **zero existing behavioral goldens changed**; suite green 260/260. Guarded by the
  `PointerCopyWalk` behavioral test (copy-then-walk, for-init copy, explicit local seed).
- **Sub-problem A — TODO (the harder half).** Using the *receiver* as a pointer value — `func (n *node) selfLink() {
  n.next = n }` — still emits `n.next = n` (a value-ref receiver `this ref node n` has no box). Needs the receiver
  direct-`ж` mechanism (extend `captureModeOperations` so "receiver used as a bare pointer value" — assigned to a
  pointer field, stored, compared — triggers a `this ж<T> Ꮡn` receiver, like the existing `&recv.field`/`return recv`
  triggers). container/ring & list need **both** A and B to fully compile; B lands alone as a correctness fix.

**Other queued leaf defects** (from the 79-error bucket): plugin CS0553 (ImplicitConvGenerator emits illegal
`object↔T` conversions), runtime/internal/math CS0133/0266 (`const MaxUintptr` ulong→nuint not const), unicode
CS0051/52 (exported field of an unexported type), internal/runtime/atomic CS1526 (`new` without `()`), and the
deferred CS1656 (bare `_ = expr` discard inside a `func _()`).

### Phase 3 iteration 11 — `ж<T>` sub-problem A: receiver as a pointer value (2026-06-26)

A method that uses its pointer receiver as a bare *value* — `func (r *ring) initSelf() { r.next = r }` (assign the
receiver to a pointer field) — emitted `n.next = n`: a value-ref receiver (`this ref ring r`) has no box, so a
ring value was assigned to a `ж<ring>` field (didn't compile; would point into a copy). Fix, two parts:
- `captureModeOperations.go`: a third direct-ж trigger `bodyUsesReceiverAsPointerValue` (the receiver appears as a
  bare RHS identifier of an assignment) — alongside the existing `&recv.field` and `return recv` triggers — so such
  methods get the box-as-receiver form `this ж<ring> Ꮡr`.
- `convIdent.go`: when the receiver of a direct-ж method is used as a pointer value (its RHS context sets isPointer
  via iteration-10's `rhsPointerCopyContext`), emit the receiver box `Ꮡr` (the value-ref path has no box).

Validated by `ReceiverPointerValue` (self-link, cross-link, pointer-walk; mutating through the self-link proves the
field points at the real receiver — 42/42/3/2). Suite green 264/264, zero existing goldens changed. **Payoff:**
container/ring 12→8 errors, container/list 25→~10 (and the remainder are now *different*, narrower defects).

**Remaining `ж<T>` sub-problems** (the still-failing ring/list errors, each a distinct mechanism — follow-up work):
- **C — transitive direct-ж.** A method calling a direct-ж method on its (value) receiver, e.g. `Next`/`Prev`:
  `return r.init()` where `init` is direct-ж → `r` (a `ref Ring` value) can't call the `ж<Ring>` overload (CS1929).
  The caller must itself become direct-ж; needs a fixpoint over `bodyCallsDirectBoxMethodOnReceiver`.
- **D — reassign the receiver pointer.** `Move`'s `r = r.prev` / `r = r.next` walks the ring by repointing the
  receiver. With a direct-ж receiver `r` is `ref var r = ref Ꮡr.Value` (a value alias) → assigning a `ж<Ring>` to it
  is CS0029. Needs `Ꮡr = r.prev; r = ref Ꮡr.Value` (repoint the box, re-alias) — the receiver-pointer-reassignment case.
- **A-variant — receiver in a comparison.** `Len`/`Do`'s `for (… p != r; …)` compares a `ж<Ring>` to the value
  receiver `r` (CS0019). The comparison must emit `Ꮡr`; extend the receiver-as-pointer-value detection/emission to
  binary (==/!=) operands, not just assignment RHS.

### Phase 3 iteration 12 — container/ring + list COMPILE AND RUN; `ж<T>` identity equality (2026-06-26)

Finished the `ж<T>` self-referential-pointer model — **container/ring and container/list both compile clean**, and
(crucially) **run correctly**. Four converter pieces + one runtime fix; zero existing behavioral goldens changed:
- **A-variant** (`captureModeOperations.go`): `bodyUsesReceiverAsPointerValue` also fires when the receiver is an
  operand of a `==`/`!=` comparison (`p != r`). `convBinaryExpr` already sets isPointer on comparison operands, so
  `convIdent` then emits `Ꮡr`. (ring 8→6)
- **C — transitive direct-ж** (`captureModeOperations.go` + `convSelectorExpr.go`): a method calling a direct-ж
  method on its receiver (`return r.init()`) must itself be direct-ж. New candidate list + **fixpoint** over
  `bodyCallsDirectBoxMethodOnReceiver` (repeat until stable — the callee may only be marked in a later pass). The
  call routes through the box: `convSelectorExpr` now boxes the receiver for `exprIsCurrentDirectBoxReceiver` and
  for a deref'd pointer **parameter** (`exprIsDerefdPointerParam`, e.g. `Link`'s `s.Prev()`). (ring 6→2)
- **D — reassign the receiver pointer** (`visitAssignStmt.go`): `r = r.prev` on a direct-ж receiver emits the box on
  the LHS (`exprIsCurrentDirectBoxReceiver` + RHS-is-pointer), so the existing pointer-reassignment path produces
  `Ꮡr = r.prev; r = ref Ꮡr.Value;` (repoint + re-alias). (ring 2→0 — **container/ring compiles clean**)
- **E — chained selector through a pointer field** (`convSelectorExpr.go`): `e.list.root` / `p.next.prev` (an
  intermediate pointer field) wasn't dereferenced — the receiver/param skip-deref check used `getIdentifier(X)`,
  which digs to the *root* of the chain (`e`), so it wrongly skipped derefing `e.list`. Now it only skips when `X`
  is *directly* the receiver/param ident. (list 10→0 — **container/list compiles clean**)
- **golib `ж<T>` identity equality** (`ж.cs`): the **runtime correctness gate**. The new `RingPointerMethods` test
  *compiled* but *stack-overflowed* — `ж<T>.Equals` did pointed-to-**value** comparison, but Go pointer comparison is
  **identity**; value comparison infinitely recurses on a self-referential struct (`Ring.next *Ring`). Rewrote
  `Equals`/`GetHashCode` to compare by referenced storage (same box, same struct-field source+accessor, or same
  array+index), never the value. Correct Go semantics and recursion-free.

Guarded by the **`RingPointerMethods`** behavioral test (a full mini-ring exercising A/A-variant/B/C/D/E — build,
walk, Len, Move±, Prev — output matches Go). The golib change touches all pointer equality; validated by the full
suite. **Both container packages now compile *and* run** — closing out the general pointer-as-value problems.

### Phase 3 iteration 13 — leaf-defect sweep: 5 fixes, 3 leaves greened (2026-06-26)

Fresh full reconvert + regenerated-csproj overlay + `go-src-converted.sln` build re-bucketed the leaves: the
`ж<T>` work had cleared the big clusters, leaving **39 errors across 17 of 304 projects** (down from last
session's 79). Five fixes landed (each its own commit; all behavioral-green, zero existing-golden churn except
the one intended re-baseline noted):

| Fix | Defect | Greens | Commit |
|---|---|---|---|
| **`min`/`max` built-ins** (golib `builtin.cs`) | Go 1.21 `min`/`max` emitted verbatim but golib had no such methods → CS0103. Added generic `min`/`max` constrained to `IComparable<T>` (numeric primitives + `@string`). No converter change. | **crypto/subtle** (+ unblocks ~dozens that use the built-ins) | `daddd953d` |
| **unsafe.Pointer keyword sanitization** (`convIdent.go`) | The `name.Value` deref form for an `unsafe.Pointer` ident used the raw Go name, so a param named `new` (C# keyword) emitted `new.Value` → CS1526. Now sanitized to `@new.Value`. | (internal/runtime/atomic syntax; pkg has deeper latent ж issues) | `175e0dfd3` |
| **unsigned unary-minus + shift precedence + shift-assign count** (`convUnaryExpr`/`convBinaryExpr`/`visitAssignStmt`) | `x & -x` → CS0023 (now `(T)0 - x`); Go `x>>4 + x` / `1<<15 - 1` re-associated in C# (shift binds looser) → now shift exprs parenthesized; `y <<= s` cast count to RHS type → CS0019 (now `(int)`). | **math/bits 4→1**; corrected a *latent* `1<<15-1`→`1<<14` miscompile in the StdLibInternalAbi golden (re-baselined) | `9089b9c4b` |
| **builtin-shadowing local rename** (`variableAnalysisOperations.go`) | `len := len(buf)` — a local named like a *called* built-in shadows the `using static` method → CS0149/CS0841. Renames the local (`lenΔ1`) when that built-in is actually called in the function; built-in call stays `len`. | **hash/maphash** | `e87705650` |
| **comma-ok map access** (`convIndexExpr`/`convExpr`/`visitAssignStmt`) | `v, ok := m[k]` (+ blank, if-init, reassign forms) wasn't detected as a tuple result → indexed twice, value assigned to `ok` (CS0029). Now routed through golib's two-value indexer `m[key, ꟷ]` via a new `IndexExprContext.isTupleResult`. **High-value correctness fix** (every comma-ok map read was wrong; no behavioral test had covered it). | **internal/coverage/rtcov** | `92d832204` |

Guarded by new behavioral tests: `MinMaxBuiltin`, `UnsafePointerKeywordParam`, `ShiftPrecedenceUnsigned`,
`BuiltinShadowLocal`, `MapCommaOk`. **Leaf packages greened this iteration: crypto/subtle, hash/maphash,
internal/coverage/rtcov.** Found-but-deferred: reading a **nil map** NREs instead of yielding zero/false (golib
nil-map representation gap — a chip was filed). math/bits' last error is a local untyped `const` (`1<<32`) typed
`UntypedInt`/BigInteger in uint64 arithmetic — context-typing untyped local consts is a larger follow-up.

### Phase 3 — earlier note: the `ж<T>` self-referential-pointer-struct confusion in container/ring & container/list —
`r.next = r` (assign receiver to a pointer field → needs the box `Ꮡr`) and `r = r.prev` (reassign the Go
pointer variable, but the C# `ref var r = ref Ꮡr.Value` deref aliases the value). The deeper one (CS0019/CS1061/
CS0029/CS1929 cluster), blocking both container packages.

## Phase 4 — Convert and run Go package tests

**Goal:** validate each compiling converted package against the same `_test.go` suite used by Go before
assembly-backed implementations are attempted at scale. The detailed and authoritative design is
[`TestingInfrastructureRequirements.md`](TestingInfrastructureRequirements.md); this section tracks the
delivery sequence and exit gates.

### 🎉 First operational milestone (2026-07-12): a converted `fmt.Println("hi")` RUNS and prints

A `-recurse`-converted `fmt.Println("hi")` program builds against the full converted stdlib and prints
`hi` to stdout, byte-identical to `go run .`. Reaching it meant clearing a **ten-layer startup crash
chain** — every layer a fix committed and validated (full behavioral suite + deployed stdlib build green
at each step). This is the minimum Phase-4 operational bar. In order:

1. **Native `sync` primitives** (`e36dea3aa`) — Go's runtime sleeping semaphore cannot be emulated;
   `Mutex`/`RWMutex`/`WaitGroup` are hand-owned native rewrites on .NET primitives (see
   [DESIGN → Operational stdlib](phase3/DESIGN-recursive-enduser-conversion.md#operational-stdlib--native-sync-primitives-2026-07-11-phase-4-start)).
2. **Package-level var initialization order** (`e39855770`) — relocate initializers whose Go dependency
   order (`types.Info.InitOrder`, resolved transitively through package function bodies) C#'s static-field
   order cannot reproduce, into a generated `package_init.cs` static ctor. First crash of any `os` import.
   Guard `PackageVarInitOrder`.
3. **Operational groundwork** (`a7ea6d3b7`, `e299253be`) — native Windows syscall FFI (`dll_windows.cs`:
   LoadLibrary/GetProcAddress + a `calli` `SyscallN` trampoline), `godebug` hooks, `runtime` bootstrap-init
   suppression (`.NET is the runtime`), `efaceOf`/`gomaxprocs` seeding, native `sync.Pool`, `SetFinalizer`
   bridge. (`runtime2.cs` marker fixup `c3f907846`.)
4. **Nested promoted-embed init** (`0bba3f5f8`, go2cs-gen) — zero-value ctors construct struct-typed fields
   whose type needs construction; first crash of `fmt.newPrinter`. Guard `NestedPromotedEmbedInit`.
5. **Pointer receiver `==`/`!=` compares its box** (`78fe56a7f`) — `os.(*File).checkValid`'s `if f == nil`.
   Guard `PointerReceiverNilCompare`.
6. **Dead pointer-param value alias skipped** (`1a723d425`) — `syscall.writeFile`'s nil `*Overlapped`
   deref-alias NRE. Guard `DeadPointerParamAlias`.
7. **golib nil pointer → address 0** (`5a4ad2b1b`) — `uintptr(unsafe.Pointer(nil))` == 0 instead of
   throwing; the final layer, reaching the OS `WriteFile`. Guard `NilPointerUintptr`.

**Next:** the `fatih/color` `-recurse` sample (exercises isatty/`GetConsoleMode` + `WriteConsole`, an
untested path — a redirected/piped stdout takes the `WriteFile` path proven above), then Go's own
`_test.go` suites (Phase 4 proper, below). Still open: dereferencing an *actually-nil* receiver/param NREs
at the deref alias (the nil-safe `DerefOrNil` accessor is not yet extended from nil-walked params to
receivers / live aliases); `time`'s runtime clock linknames.

The behavioral suite proves individual converter/runtime constructs using hand-written fixtures. Phase 4
adds the complementary whole-package gate: load Go's internal and external test-package variants, convert
the eligible tests, compile them with the converted package sources, and compare their results with a clean
`go test -json -count=1` run for the same source, build tags, GOOS, and GOARCH.

### Phase 4A — colocated test projects and serial test core

- Add an opt-in test conversion mode using `go/packages` with `Tests = true`; do not burden normal
  production-only conversion.
- Emit `x_test.go` as `x_test.cs` beside the other converted files, plus
  `<package>.tests.csproj`, `package_test_info.cs`, a deterministic test manifest, and a generated package
  test host.
- Keep the production project test-free. The test project recompiles the production `.cs` sources with
  the test sources instead of referencing the production DLL; this preserves same-package access to
  unexported declarations and joins the same partial package class.
- Support both `package p` and external `package p_test` tests in one isolated package executable,
  including external self-import binding without a self-referencing project.
- Add a hand-owned `go.testing` compatibility runtime for the initial bootstrap rather than depending on
  the full converted `testing` package. Implement the serial core: pass/fail, `Error[f]`, `Fatal[f]`,
  `FailNow`, logs, skips, helpers, cleanup, panic reporting, `TempDir`, `Setenv`, and `TestMain`/`M.Run`.
- Run every package in a child process with a controlled environment, timeout, and isolated working tree
  containing its relative fixtures and `testdata`.
- Add end-to-end behavioral fixtures for discovery, same-package unexported access, external tests,
  `TestMain`, failure/skip/panic/defer paths, cleanup order, build constraints, fixtures, and unsupported
  capabilities.

**Phase 4A exit:** all harness fixtures pass and at least one already-compiling leaf standard-library
package is validated end to end.

### Phase 4B — subtests, parallelism, and differential results

- Implement `T.Run`, hierarchical names/filtering, duplicate subtest names, cleanup after child completion,
  and `T.Parallel` with Go's parent/subtest barrier semantics.
- Emit normalized package/test events and aggregate them into machine-readable output plus JUnit or TRX for
  CI. Compare eligible test identity and terminal status with Go; do not byte-compare timings, paths, stack
  formatting, or concurrent log interleaving.
- Record every discovered test as included, platform-excluded, unsupported, or failed-to-convert. No omitted
  test may silently count as a pass.
- Detect stale manifests from source inputs, target options, converter revision, and hand-written companion
  files; unchanged conversions must be byte-stable.

**Phase 4B exit:** representative leaf packages using external tests, subtests/parallelism, and file fixtures
validate reproducibly on the matched target platform.

### Phase 4C — package coverage and CI scaling

- Work bottom-up through the compiling package DAG, expanding the compatibility API from categorized real
  failures rather than speculative reimplementation of all of `testing`.
- Track each target/package as **validated**, **failing**, **conversion-blocked**,
  **infrastructure-blocked**, or **not-applicable**.
- Add batching, CI sharding, package-level timeouts, failure artifacts, and explicit flake diagnostics.
- Keep benchmarks, active fuzzing, race-detector equivalence, and coverage-percentage equivalence outside
  the initial correctness gate. Add examples and deterministic fuzz seed corpora later without delaying
  Phase 5.

**Phase 4 exit:** one documented command converts, builds, and runs a selected package's tests; the harness
semantics are guarded end to end; every compiling package attempted is honestly classified; and at least
three representative leaf packages are validated (including external-package and subtest/parallel cases).
Only **validated** satisfies the default Phase 5 behavior gate.

### Phase 4 — declared host limits and their retirement path

A small class of test verdicts is disclosed under the **`host-limit`** class defined in
[`ValidatedTestPackages.md`](ValidatedTestPackages.md): tests whose premise rests on a property of the
test *binary* that the converted host's current deployment shape structurally lacks — today, the
relocatable single-file executable that Go's static linking provides and a framework-dependent .NET
apphost (a native stub bound to a same-basename managed assembly beside it) does not. These are not
permanent exclusions, and the roadmap carries a proposed resolution: **publish the test host
self-contained and single-file**, evaluated with the .NET toolchain hop, where publish shapes are
re-priced anyway (the performance suite already proves converted code runs Native AOT, so the
capability is a packaging decision, not a porting question). The disclosure machinery is built to
force that revisit rather than allow drift: disclosed tests keep running in every sweep, so the day
the host becomes relocatable they begin passing, the sweep's disclosure arithmetic breaks loudly, and
the entries must be removed. Proof pages that carry a `host-limit` disclosure point back to this
section as the standing answer to "why not just make the host single-file today."

### Phase 4 follow-up (deferred) — expand the `sstring` stack-string MVP

The escape-analysis-driven stack-string emission landed as a deliberately conservative **MVP** (2026-07-12,
branch `golib-string-performance`; see [the strings reference](ConversionStrategies-Reference/strings.md#a-non-escaping-stringbyte-local-emits-the-stack-string-sstring)
and the [Glossary MVP entry](Glossary.md#mvp)). It emits a zero-copy [`sstring`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/sstring.cs)
view for a `s := string([]byte)` local only when the local is non-escaping, its source is a plain unnamed
`[]byte` never written after the conversion, and its every use is a safe read (`len`/`cap`, byte index, or
comparison against a string literal). That initial predicate fired on **exactly one** Go-1.23 stdlib site.
A first expansion has since landed (2026-07-12) — **unnamed `string(x)` comparison operands** against a
literal (`string(buf[:4]) == "ZLIB"`, `string(item) != "null"`), which are consumed within the comparison
and so need no analysis at all — lifting the stdlib footprint to **~23 sites across ~19 files** (the common
byte-signature-check idiom in `debug/*`, `encoding/json`, `image/gif`·`png`, …). Still far short of the
`PerfString` win (whose `s` is a `+` operand, so it stays `@string` and needs increment 4 below).

Expand the eligible surface further, one gated increment at a time. **Why this is well-suited to exploratory work:**
because `sstring` is a `ref struct`, the .NET compiler turns almost every over-reach into a **compile error**
(it cannot be stored in a field/array/map, boxed to an interface, sent on a channel, or captured by a
closure) — so a too-aggressive predicate fails **loud** at build time (caught by the behavioral suite +
full-stdlib reconvert), never silently. The **only** silently-wrong vectors are escape-via-`return` and
mutation of the source buffer during the view's lifetime; both must stay explicitly guarded. Candidate
increments, roughly by ROI:

1. **Unnamed-temporary conversions.** The comparison-operand slice (`string(b) == "literal"`, and now — see
   increment 3 — `string(b) == variable` and `string(a) == string(b)`) is **done**. The remaining slice —
   `string(x)` passed to a non-retaining callee (`strconv.ParseUint(string(s), …)`) or used as a map key
   (`m[string(b)]`) — is harder: a `@string`-typed parameter or map key copies the `sstring` back at the
   boundary (no win) unless the callee/`map<K,V>` gains an `sstring`/`ReadOnlySpan<byte>` overload. Needs a
   curated non-retaining-callee whitelist plus golib span-lookup support on `map`.
2. **`sstring operator+`** (returning `@string`) — **done (2026-07-13)** — plus widening the safe-use
   whitelist beyond len/cap/byte-index/compare-literal. Two widenings have landed:
   - **`switch string(x)` tag** (and a named local used as a switch tag). A Go string switch always lowers
     to a temp compared against each case label with `==` (never a C# `switch`/pattern — string labels are
     not C# case constants), so the tag is a zero-copy `sstring` view whenever every case label is a
     mutation-safe operand (literal, pure read, or another conversion — never a call). Covers the
     binary-format-detection idiom `switch string(magic) { case elfMagic: … }`
     (`markSStringSwitchConversions` + the switch-tag safe use in `sstringUsesAreSafe`).
   - **`string(x) + suffix` concatenation.** `golib`'s `sstring` gained `operator+` overloads (vs
     `@string`, `sstring`, and a `ReadOnlySpan<byte>` u8 literal, both orders, all → `@string`) that
     block-copy the operand span into the single result buffer, skipping the intermediate `((@string)x)`
     copy (the RESULT `@string` still allocates — the win is one fewer allocation per concatenation). A
     `string(x)` `+` operand is emitted `sstring` under the comparison rules
     (`markSStringBinaryOperandConversions` now matches `token.ADD`; a named local's `+` use is added to
     `sstringUsesAreSafe`).

   Both guarded by `SStringElision` (switch: literal/named-local/magic-const + negative call-label;
   concat: named-local/literal/variable/two-conversion + negative call-operand); full-stdlib reconvert
   stays 302/302. Sub-slicing kept in safe uses and rune-range remain. (Increment 1's
   `for range string(x)` sub-case is **not pursued** — the Go-1.23 stdlib has zero such sites, so there
   is no footprint to justify the golib rune-enumerator surface. Increment 1's `m[string(x)]` map-key
   sub-case is **not pursued** — a zero-allocation span lookup would require replacing every
   `@string`-keyed `map`'s default equality comparer with a custom `IAlternateEqualityComparer`, whose
   virtual dispatch would slow ALL `@string` map operations — a net loss dwarfing the ~16 read-only
   sites, and writes cannot be optimized at all.)
3. **Mixed `sstring == @string`** operator — **done (2026-07-13).** `golib`'s `sstring` gained all six
   comparison operators (`==`/`!=`/`<`/`<=`/`>`/`>=`, both operand orders) against `@string`, a byte-ordinal
   `SequenceCompareTo`/`SequenceEqual` with **no** heap copy of either side. No ambiguity was reintroduced: a
   `u8` literal binds the `ReadOnlySpan<byte>` operator, an `@string` variable the new mixed operator, another
   view the `sstring == sstring` operator — each an exact match. The converter now emits `sstring` for a
   non-escaping `string(x)` compared against a plain-`string` variable/field, and for two `string(bytes)`
   conversions compared directly, both for a named local (`s := string(x); s == want`) and an unnamed operand
   (`string(x) == want`). The unnamed form additionally requires the other operand to be mutation-safe (a
   literal, pure-read expression, or another conversion — never a call, which could write `x` before the lazy
   view is read); the named-local form needs no such check because its source is already proven never-written.
   This lifted the Go-1.23 stdlib footprint substantially (the common `string(b[:n]) != magic` and
   downgrade-canary idioms across `crypto/*`, `hash/*`, `html/template`, `go/internal/*importer`, …). Guarded
   by `SStringElision` (mixed-vs-variable, mixed-vs-field, two-conversion, and a negative call-operand case);
   full-stdlib reconvert stays 302/302.
4. **Positional / loop-carried liveness guard** — relax "source never written" to "not written between the
   conversion and the local's last read, not loop-shared." This is the **only** increment that reaches the
   `PerfString` hot loop, and the highest silent-risk one — gate it behind the heaviest adversarial pass.
5. **Loop-invariant / repeated-conversion hoisting** — **done (2026-07-12).** An *optimization* of
   already-eligible sites, not a widening. When an eligible `string(x)` over a never-written identifier was
   emitted **repeatedly** — several comparisons, or the same conversion inside a hot loop — the converter
   re-materialized `(sstring)x` (`new sstring(x.ToSpan())`) at every use, and the JIT will **not** hoist it.
   This was measured, not assumed: a non-throwing `MemoryMarshal.CreateReadOnlySpan` view added to `golib` (so
   the `ToSpan` bounds check could not block loop-invariant-code-motion) made **zero** difference — the JIT
   simply does not lift a `ref struct` view out of a loop. The converter now emits **one** hoisted `sstring`
   temp at function scope and reuses it — a clean back-to-back A/B took `PerfStringView` from **4.84× → 3.04×
   Go** on the JIT (35.9 → 22.5 ms) and **4.49× → 1.86×** on Native AOT (34.4 → 14.1 ms). That is about the
   practical floor within .NET: the residual is inherent (`SequenceEqual`'s per-call setup on a tiny buffer vs
   Go's inlined `memcmp`; a decomposition micro-benchmark confirmed the `sstring` `==` operator itself adds
   **zero** over a raw span compare — the whole cost is the per-use view reconstruction).

   **Implementation** ([`sstringHoistOperations.go`](https://github.com/ritchiecarroll/go2cs/blob/master/src/go2cs/sstringHoistOperations.go)):
   a per-`FuncDecl` pre-pass (`planSStringHoists`) groups eligible `string(x)` conversions **over a bare
   identifier** by that identifier's object and, when the source is a never-written function-local/parameter
   (`objectIsWritten == false`) declared before the injection point with no use inside a nested func literal,
   lifts a group of ≥2 uses (or ≥1 use in a loop) to one `sstring <temp> = ((sstring)x);` — `convCallExpr`
   returns the temp name at each use; `visitBlockStmt` injects the decl before the group's anchor (the first
   top-level body statement containing a use). The `objectIsWritten(x) == false` predicate the MVP already
   computed makes **function-top** hoisting always safe (no cross-loop mutation analysis — the safe subset of
   increment 4). The **bare-identifier** gate is load-bearing: grouping merely by *root* ident would collapse
   `string(buf[:7])` and `string(buf[8:12])` (net/http's `is408Message`, the one stdlib site an earlier draft
   wrongly hoisted) into a single incorrect view — so sub-slice / index sources stay inline.

   **Verified:** `SStringElision` extended with a loop case, a straight-line 2-use case, and the
   distinct-sub-slice case that must **not** collapse (behavioral suite green); the full-stdlib reconvert hoists
   **zero** sites and stays byte-identical to baseline (302/302, 0 errors) — confirming the ROI caveat that real
   stdlib `sstring` sites are mostly **single** comparisons where hoisting is a no-op. The idiomatic
   **named-local** form (`s := string(x)`) already hoisted (the MVP emits one `sstring s = (sstring)x`); this
   increment is a targeted win for the *inline-repeated* loop/tokenizer idiom (a scanner comparing `string(buf)`
   against several keywords), not a broad one.

Guard every increment with the `SStringElision` behavioral test (extend it) + full-stdlib reconvert (inspect
every newly-`sstring` site) + a re-run of the performance suite to quantify the actual win.

## Platforms — Linux and the multi-target corpus (in progress)

Running alongside the numbered phases rather than inside one: making the converted corpus real on
more than Windows.

**Where it stands (2026-08-10):**

- **Converting works on Windows and Linux.** Releases since **1.23.1.5** carry Linux converter
  binaries alongside Windows; a Go module converts identically on either host.
- **The corpus is multi-target on disk.** Since 2026-08-08 the standard library uses layout **L3**:
  platform-shared files sit flat, platform-varying emissions live in per-GOOS folders
  (`<pkg>/{windows,linux,darwin}/`, 37 packages vary), and each affected `.csproj` compiles exactly
  one set via `$(GoTargetOS)` — defaulting to `windows`, so a plain build still means the Windows
  corpus. Design: [`phase4/DESIGN-multiplatform-corpus.md`](phase4/DESIGN-multiplatform-corpus.md).
- **Running on Linux works today for programs whose import closure stays within the
  `fmt`/`os`/`time` class** — the first native Linux run of a converted program landed 2026-08-08
  (tag `linux-first-run-2026-08-08`).
- **The full corpus compiles for Linux — all 307 projects, zero errors** (2026-08-14; the
  three-target emission verified idempotent, with the Windows corpus byte-identical alongside it).
  Compiling is the milestone, not operational — the same distinction the Windows corpus walked
  between its Phase 3 and Phase 4: the platform-divergent `syscall` surface still needs its
  *operational* Linux counterparts (the struct-passing seam solved on Windows with blittable
  mirrors, the netpoll contracts), and a Linux validation campaign then follows with its own
  per-platform roster. The operational plan and remaining F-items live in
  [`PLAN-linux-operation.md`](PLAN-linux-operation.md);
  [`phase4/CENSUS-linux-compile-wall.md`](phase4/CENSUS-linux-compile-wall.md) records how the
  compile wall fell.

**What a published package is, platform-wise.** Three layers with different platform models ship in
every NuGet package, and knowing which is which explains what works where:

1. **The runtime library (`go.lib`) selects its platform at RUN time**, like any cross-platform
   .NET library — where it needs the OS (console fd writes, synchronization primitives, timers) it
   branches per-OS, so these paths are operational on Windows, Linux and macOS today.
2. **Platform-neutral converted packages** (~305 of ~340) contain no platform-varying code at all —
   their IL behaves identically on any OS .NET supports. This is why `fmt`-class programs — the
   Tour of Go, for instance — run correctly on Linux from today's packages.
3. **Platform-varying converted packages** select their platform at BUILD time, faithful to Go's
   build-tag model. Each ships one flavor per supported platform — `win-x64` and `linux-x64` — and,
   from packages 1.24.13.2, a consumer both compiles and runs against the flavor of the platform its
   conversion targeted (go.lib selects the compile asset; the runtime selects the loaded one). On a
   platform with no shipped flavor (macOS, other architectures) the Windows flavor is what compiles and
   loads, and its platform-entangled behavior (local timezones, sockets, the `syscall` surface) follows
   Windows semantics or fails; that is the remaining wall, not a portability defect in the layers
   below it.

**Validation is per-target.** Every row in
[`ValidatedTestPackages.md`](ValidatedTestPackages.md) is a `windows/amd64` verdict; a Linux
validation campaign begins when the `GoTargetOS=linux` corpus builds, and its roster will differ
(build tags select different test files per platform). **The packaging direction once it does:**
RID-specific assets in a single package — one `go.<pkg>` ID carrying each target's correct
emission under `runtimes/<rid>/`, selected automatically by the consumer's runtime identifier —
so Go-faithful per-platform semantics never fork the package graph.

## Progress tracking

| Metric | Source | Status |
|---|---|---|
| Baseline + tests build clean | `dotnet build src/go2cs.slnx` | ✅ green |
| Behavioral suite passing | `BehavioralTests` / `BehavioralRunner` | ✅ 555 projects transpile+compile+golden, 529 output-compared (2026-08-10) |
| Full packages compiling | `src/go2cs-stdlib.slnx` (344 projects at Go 1.24.13) | ✅ **342 / 342** packages (2026-09-23, `fa18863b9`) |
| Full-conversion error count | build-error buckets | ✅ **0** |
| Converted package tests | [`ValidatedTestPackages.md`](ValidatedTestPackages.md) — the authoritative roster | ◻ Phase 4 **in flight**. The roster's own header carries the current count, verdict total and disclosure total, recomputed from its table — read it there rather than here, so this row cannot go stale against it |
| Assembly-backed implementations | Phase 5 external-declaration ledger | ◻ Phase 5 planned — gated by Phase 4 validation |
| Linux / multi-target | Platforms section above | ◻ converts on Windows and Linux; a module reaching `golang.org/x/sys/unix` (the README walkthrough) builds and runs on `linux/amd64` with go2cs packages ≥ 1.24.13.2 and a converter from a checkout that includes them; the wider operational `syscall` surface and a Linux validation roster are in progress |

## Reference: open converter items (`src/go2cs/ToDo.md`)

`visitMapType` completion; remaining dynamic-struct implicit-cast checks across `AssignStmt`/`CompositeLit`/
`IndexExpr`/`BinaryExpr`/`UnaryExpr`/`SelectorExpr`/`TypeSwitchStmt`/`ValueSpec`; optional recursive
dependent-package conversion; map/channel `GoType` generator support (`IMap`/`IChannel`); comment
conversion; cgo + Go-assembler (`.s`) targets.
