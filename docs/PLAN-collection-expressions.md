# PLAN — Go slice literals as C# collection expressions, `[1, 2, 3]`
<!-- {% raw %} — Jekyll/Liquid guard: this plan quotes Go composite-literal syntax that Liquid would otherwise parse. -->

> **STATUS: PROPOSED, 2026-11-05 (P2), for the owner. Design and census only: no converter, golib, generator or
> corpus file was changed.** Measured at master `8aaa6cebc8` against go1.26.10. The owner approved the feature on
> 2026-10-27 and confirmed on 2026-11-05 that the 1.26.10.3 release is its conversion boundary (COORD's dispatch).
> It is the largest .3 item, so it goes to the owner with a size before any build.
>
> **Headline.** `[1, 2, 3]` needs `slice<T>`'s `foreach` enumerator to yield `T` instead of Go's range pair
> `(nint, T)`. That change breaks every C# `foreach (var (i, v) in s)` over a slice or array: about 6,000 in the
> standard library (2,115 in production files), 349 in 189 behavioral tests, about 200 in hand-owned files and about
> 20 in GolibTests. golib's own build is unaffected (measured: 0 errors). The pair loop gets an explicit spelling,
> recommended `foreach (var (i, v) in range(s))`. Then 81% of Go's slice literals in the standard library
> (11,423 of 14,035) sit where C# knows the target type and become `[1, 2, 3]`; the rest keep today's
> `slice<T>([1, 2, 3])`. **Size: 7 to 10 lane-days**, plus a nugetgo rebuild wave.

## 1. Why the enumerator has to change

C# gives a collection expression the element type of its target's iteration type, which `foreach` reads from the
public `GetEnumerator()`. `slice<T>.GetEnumerator()` (`src/core/golib/slice.cs:1010`) yields `(nint, T)`, so a
`[CollectionBuilder]` on `slice<T>` would have to build from `ReadOnlySpan<(nint, T)>`. Seat #30 measured that route
(CS9187 at golib's build, and CS0029 for the extension-`Add` route; BOARD, 2026-10-29) and shipped
`slice<T>([1, 2, 3])` through a separate builder type, `SliceLiteral<T>`, instead.

C# 14 (`src/Directory.Build.props:45`) gives a collection expression no natural type, so no other door exists: an
implicit conversion from `SliceLiteral<T>` is never applied to a bare `[…]`. The enumerator is the only change that
makes `slice<T> x = [1, 2, 3];` compile.

What already works in our favour:

- LINQ already sees a slice as its values. `IArray<T>` keeps the pairs as its own member so that `IEnumerable<T>`
  is the only generic sequence interface (`src/core/golib/array.cs:40-50`).
- Index-only ranges already avoid the pair enumerator: `for i := range s` is `foreach (var i in range(s))`, which
  reads the length once and no element (`src/go2cs/visitRangeStmt.go:745-807`, `src/core/golib/RangeIndexOnly.cs`).
- Inside today's `slice<T>([…])`, the elements are already a collection expression typed by `T`. Every element
  conversion the bare form needs (untyped constants, `any` elements, the `@with(` rule) is already proven by seat #30.

## 2. Census

**Instruments.** A Go-side census (`go/packages` and `go/types`, about 400 lines, [`probes/p2-collection-expressions-census`](phase4/probes/p2-collection-expressions-census/main.go); its outputs are in [`phase4/data/collection-expressions`](phase4/data/collection-expressions/)) counts
range statements by operand type and iteration-variable shape, and every composite literal by form and by whether
its syntactic position gives C# a target type. It was red-tested on a planted package with one site of each class
(each class read exactly as planted). A C#-side text count over the committed corpus and goldens reconciles it.
golib was spiked (§2.4) and the spike's build was made to fail on a planted pair loop before its 0 was trusted.

**Two derivations agree.** Production files on linux: 1,370 unkeyed literals of unnamed slice types (Go side)
against 1,425 `slice<…>([` in the committed converted files, which carry three flavours of the L3 packages (+4%).
Pair loops: 2,429 Go-side pattern loops of every kind against 2,515 committed (+3.5%). Behavioral tests: 335
directories with a pair loop or a slice literal, by the Go side and by the golden text alike. The windows and darwin runs read the
same shape: 81.5% and 81.4% of literals targeted (linux 81.4%), 5,976 and 5,999 sequence pair loops (linux 6,041).

### 2.1 Range statements (std, linux, with tests; production only in brackets)

| Operand | `for i, v := range x` (pair) | `for i := range x` (index only) |
|:--|--:|--:|
| slice | 5,780 (2,043) | 680 (300) |
| array | 234 (46) | 170 (91) |
| pointer to array | 16 (15) | 26 (12) |
| type parameter (`S ~[]E`) | 11 (11) | 8 (8) |
| **sequence total** | **6,041 (2,115)** | **884 (411)** |
| map, for reference | 357 (165) | 128 (67) |
| string, for reference | 117 (82) | 5 (4) |

4,626 of the sequence pair loops discard the key (`for _, v := range s`). Maps and strings keep their enumerators.

### 2.2 Slice literals (std, linux, with tests)

| Form | Count | Proposed |
|:--|--:|:--|
| unkeyed, unnamed `[]T{…}`, at a **targeted** position | 11,423 | `[…]` |
| unkeyed, unnamed, at an **untargeted** position | 2,612 | `slice<T>([…])`, as today |
| keyed `[]T{5: x}` | 34 | unchanged (a collection expression has no index keys) |
| named slice type `Names{…}` | 745 | unchanged (its constructor) |
| arrays `[N]T{…}`, unnamed and named | 2,135 | unchanged (§4.3) |
| maps | 1,313 | unchanged (no map form in C# 14) |

Targeted positions: inside another literal (3,326 elements, 4,322 field or map values), a package-level var
(2,326, a C# field with its type), a slice argument (955),
an assignment (349), a return (140), a typed local var (5).

Untargeted positions: `x := []T{…}` (1,456), a range operand (654), an interface argument (178), `append`, `copy`
and other builtins (89), a conversion (46), an untyped local `var` (128), a type-parameter argument (35), an operand
(24), other (2).

Behavioral tests, for comparison: 730 literals, 288 targeted and 442 untargeted, because `:=` dominates small
programs (285).

### 2.3 Where the pair loops are

| Population | Pair loops over a slice or array | How they change |
|:--|--:|:--|
| converted corpus, committed (three flavours, production plus banked tests) | about 5,450 | regenerated |
| behavioral goldens | 349 loops in 189 of 898 directories | regenerated, re-baselined |
| hand-owned files in `src/core` (330 `GoManualConversion` files, 174 `*_impl.cs`) | about 200, plus 16 in hand-owned tests | by hand, using the compiler's errors as the list |
| GolibTests | about 20 of its 58 deconstructing loops | by hand |
| golib | 0 (measured) | none |
| go2cs-gen templates | 0 emitted (its 27 loops are over Roslyn lists) | none |
| consumers' C# | unknown | release notes (§5) |

The committed-corpus figure is derived, not counted: 6,274 deconstructing pattern loops, times the slice-and-array
share of Go's pattern loops (2,115 of 2,429, 87%). The hand-owned figure is by the first tuple element: 200 of 222
loops start with an index (`(i, v)`, `(_, v)`, `(nint _, byte v)`), 2 with a key, 20 unclear.

### 2.4 The golib spike

`slice<T>` and `array<T>` were given a public `GetEnumerator()` that yields `T`, with the pair enumerator moved to a
named method. golib built with **0 errors and 0 warnings**. A planted `foreach (var (i, x) in default(slice<int>))`
then failed the same build with CS8130 and CS0411, so the build does see the change. The spike was reverted.

## 3. The design

### 3.1 golib

1. `slice<T>.GetEnumerator()` and `array<T>.GetEnumerator()` yield `T`, matching `IEnumerable<T>`.
2. The pair enumerator stays, allocation-free as today, behind `range(x)` overloads for `slice<T>`, `array<T>`,
   `ж<TArray>` and `IArray<T>` (for type-parameter ranges); `IArray<T>`'s pair member is renamed to match.
3. `[CollectionBuilder]` on `slice<T>`, building from `ReadOnlySpan<T>` with the copy uncounted, as seat #30 ruled
   for `SliceLiteral<T>`. `slice<T>([…])` stays for untargeted positions.
4. An empty `[]` builds Go's non-nil empty slice, as `slice<T>([])` does today. A bare `[]` in hand-written C# that
   targets `slice<T>` would change from nil to non-nil: unsafe.Slice's nil case already returns `default`
   (`src/core/unsafe/unsafe.cs:1014-1019`), and the build lists every other such site by marking the builder
   `[Obsolete]` for one build, which makes the compiler report each bound site (103 bare `[]` exist in hand-written
   C#; the ones read here target arrays, lists and dictionaries).

### 3.2 The converter

| Go | C# today | Proposed |
|:--|:--|:--|
| `for i, v := range s` | `foreach (var (i, v) in s)` | `foreach (var (i, v) in range(s))` |
| `for _, v := range s` | `foreach (var (_, v) in s)` | `foreach (var (_, v) in range(s))` |
| `for i := range s` | `foreach (var i in range(s))` | `foreach (var i in range(len(s)))`; an array's or pointer-to-array's constant length stays `range(N)` |
| `x = []int{1, 2}`, `return []int{1, 2}`, an argument, a field, an element | `slice<nint>([1, 2])` | `[1, 2]` |
| `x := []int{1, 2}` and the other untargeted positions | `slice<nint>([1, 2])` | unchanged |
| `[][]int{{1}, {2, 3}}`, targeted | `slice<slice<nint>>([[1], [2, 3]])` | `[[1], [2, 3]]` |
| `[]T{}`, targeted | `slice<T>([])` | `[]` |

The index-only form moves to `range(len(s))` because `range(s)` becomes the pair form. Go 1.22 accepts
`for i := range len(s)` with the same meaning, and it reads no element, which the fips140cache fix requires.

### 3.3 Risks the build must close

- **Overload binding.** C# prefers a `ReadOnlySpan<T>` overload for a collection expression. A converted call whose
  callee has both a `slice<T>` and a span overload could bind the span one. Today's wrapper form never does. The build
  diffs the bound overloads (CNR plus the stdlib build) and keeps the wrapper at any call site that rebinds.
- **Generic inference.** A bare literal passed to a generic `[]T` parameter (13 behavioral sites, none in std) relies
  on C# inferring `T` from the elements. Where it cannot, the wrapper stays.
- **Every ranged loop changes its enumerator.** The banked rows re-validate on the full sweep, not a sample.

## 4. Options for the owner

### 4.1 The pair loop's spelling

- **(a) `range(s)` for pairs, `range(len(s))` for index only** (recommended). Closest to Go's `range s`; costs the
  884 index-only sites a re-spelling, and changes what golib's public `range(slice)` means.
- **(b) `range(s)` stays index only; pairs get a new name**, such as `foreach (var (i, v) in pairs(s))`. No index-only
  churn, but the pair loop no longer says `range`.

### 4.2 Where a bare `[…]` goes

- **(a) Every targeted position** (recommended): 11,423 of 14,035 in std.
- **(b) Only declarations, assignments, returns and elements**, keeping the wrapper in call arguments (955): removes
  the overload risk of §3.3 at the cost of a mixed look.

### 4.3 Scope

- **(a) Slices only** (recommended). Arrays keep `new array<T>(…)`: a builder gets a span, not Go's length, so it
  could not pad `[5]int{1, 2}` or refuse a long literal. `array<T>`'s enumerator still changes, so ranges read the same
  over both.
- **(b) Slices, and later named slice types** through a generator-emitted builder (745 literals). Not sized here.

Maps stay as they are: C# 14 has no map form. `x := []T{…}` keeps the wrapper, because Go names the type on the
right too, and `slice<nint> x = [1, 2]` would move it to the left.

## 5. Consumers and nugetgo

**Source.** Hand-written C# over a go.* slice changes in three ways, all for the release notes:

1. `foreach (var (i, v) in s)` over a slice or array stops compiling (CS8130). It becomes `range(s)`.
   `docs/ConsumingGoFromCSharp.md:420-436` and `src/tests/CSharpConsumer/Program.cs:185,191` teach this form today.
2. `foreach (var v in s)` gives the value, not a tuple. It compiles either way, so code that printed or stored the
   tuple changes silently; code that read `.Item2` stops compiling. This removes the pitfall
   `ConsumingGoFromCSharp.md:673` warns about.
3. golib's `range(s)` gives pairs under option 4.1 (a).

**Binary.** An assembly compiled against 1.26.10.2 calls `slice<T>.GetEnumerator()` returning the pair enumerator,
which no longer exists in .3, so it fails at run time with a missing method. NuGet does not catch it: B4's range,
`[R, next Go minor)`, lets a .2-built nugetgo package restore under .3, the failure mode the .2 release note already
describes. So .3 needs a nugetgo rebuild wave, as .2 did (`PLAN-nugetgo-wave-1-26-10-2.md`), with the same runbook.
If .3 already has a wave for another reason, this adds nothing to it.

## 6. Size and footprint

| Step | Lane-days |
|:--|--:|
| golib: enumerators, `range` overloads, builder, the `[Obsolete]` listing, GolibTests | 1 |
| converter: the range arm, literal positions, red tests | 1.5–2 |
| hand-owned files: about 200 loops plus the index-only `range(s)` sites, from the compiler's error list | 1–1.5 |
| corpus: three-target two-seeded emission, regen, stdlib build, full sweep of the banked rows | 2–3 |
| goldens (335 to 344 directories) and CNR | 0.5–1 |
| release notes, `ConsumingGoFromCSharp.md`, CSharpConsumer | 0.5 |
| nugetgo rebuild wave, lane side | 0.5 |
| **total** | **7–10** |

The corpus regen touches most converted files that hold a ranged loop or a literal. It collides with any other
corpus-wide regen, so it is sequenced after class K's and H's init-scope seat have landed.

## 7. Predictions, scored

Written before the census output was read ([`predictions.txt`](phase4/data/collection-expressions/predictions.txt)):

| # | Prediction | Measured | |
|:--|:--|:--|:--|
| P1 | 6,000–12,000 sequence pair loops in std, about 2/3 key-blank | 6,041; 77% key-blank | count met, share missed |
| P2 | 3,000–6,000 index-only | 884 | missed |
| P3 | 10,000–20,000 unkeyed unnamed slice literals | 14,035 | met |
| P4 | 70–85% targeted; `:=` the largest untargeted group | 81%; `:=` 1,456 | met |
| P5 | corpus text within 15% of the Go side | +4% (production) | met |
| P6 | 20–60 golib errors | 0 | missed |
| P7 | 300–450 behavioral directories with a pair loop | 189 | missed |

## 8. What the owner rules

4.1 (a or b), 4.2 (a or b), 4.3 (a or b), and whether to build at 7 to 10 lane-days for .3.
<!-- {% endraw %} -->
