# Can C# interceptors replace hoisted string literals (`helloˢ`) with Go-style `"hello"`?

Design scout, 2026-10-17. Research plus probes only. No repo edits. Probe sources are in
`H:\go2cs-tmp-coord\interceptor-probe` (bin/obj deleted; run logs kept).

## 1. Answer in one paragraph

**Mostly no.** Interceptors are stable and work under JIT and Native AOT, with no experimental
warnings, as long as the generator references Roslyn 4.13 or later. But an interceptor replaces only
the **method being called**. The call site still binds and evaluates every argument conversion against
the original signature. A literal passed to an `@string` parameter is therefore still converted, and
the conversion allocates, before the interceptor runs. The probe measures this: intercepted, the call
costs **40 B per call, the same as the naive form**. `@string` parameter slots are **62% of all hoisted
uses** and **82% of uses in production stdlib code**, so interceptors cannot replace hoisting while
keeping today's performance.

They do work, at full hoisted performance (0 B, the same ns), in one class: **`any` / `...any` call
arguments**. The typical case is `fmt.Println("hello")`, where a C# string literal reaches an `object`
slot as a free reference and the interceptor swaps in a cached, pre-boxed `@string`. That is **25% of
hoisted uses corpus-wide, 66% in the behavioral goldens, 1% in production stdlib**.

Recommendation: **NO-GO** on interceptors as a replacement for hoisting. An **optional narrow GO** (an
`any`-slot pilot) is possible if the readability of `fmt.Println("hello")`-style code is worth about a
week and a Roslyn bump. Section 7 has the design.

## 2. Interceptor status, verified from primary sources

| Claim | Status | Source |
|:--|:--|:--|
| Stable | **Yes.** The spec says "first shipped experimentally in .NET 8, with stable support in .NET 9.0.2xx SDK and later." This is a compiler feature, not a LangVersion feature: the C# 14 "what's new" page does not mention interceptors. | [roslyn docs/features/interceptors.md](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md) line 6; [PR #76312](https://github.com/dotnet/roslyn/pull/76312), "Mark interceptors feature as stable and deprecate file path based attributes", merged 2024-12-13 (milestone 17.13); [C# 14 what's new](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14) |
| Microsoft Learn | **Stale.** The C# 12 page still calls interceptors "an experimental feature" and names `<InterceptorsPreviewNamespaces>`. | [What's new in C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#interceptors) |
| Attribute form | `InterceptsLocationAttribute(int version, string data)`. Version 1 data is base64 of a 16-byte xxHash128 checksum of the file, an int32 position, and the display **file name only** (decoded in the probe: `Program.cs`, so no absolute path leaks). It is **not in the BCL**: no .NET 10 ref assembly defines it (checked), and the spec says to declare it `file`-local. | spec "Location encoding" |
| Legacy `(string filePath, int line, int column)` form | Deprecated. PR #76312 adds `WRN_InterceptsLocationAttributeUnsupportedSignature` (a downstream log shows it as CS9270), and follow-up PR #76642 moves it into the .NET 9 warning wave. I did not reproduce this one locally. | PR #76312 |
| Opt-in property | **Still required.** Measured on SDK 10.0.400: without it, the build fails with `CS9137: The 'interceptors' feature is not enabled in this namespace`. `InterceptorsPreviewNamespaces` still works as an alias. A **parent namespace entry covers its children** (`go2cs` admits `go2cs.Interceptors`; `global` admits everything; the partial segment `go2cs.Inter` does not). | probe; spec "User opt-in" |
| Generator API | `SemanticModel.GetInterceptableLocation(InvocationExpressionSyntax)` plus `GetInterceptsLocationAttributeSyntax()`. Roslyn **4.10.0**, go2cs-gen's current pin (`src/gen/go2cs-gen/go2cs-gen.csproj:33`), **does not have it**. **4.11.0 and 4.12.0** fail with `error RSEXPERIMENTAL002`. **4.13.0 and 4.14.0** build with 0 warnings. The SDK's own compiler is 5.x. | probe |
| What can be intercepted | Only ordinary method invocations: static, instance, extension, `?.`. **Not** constructors, delegate invocations, properties, local functions, operators, or method-group conversions. | spec "Non-invocation method usages" |
| Signature rule | "The returns and parameters, including the `this` parameter, must have the same ref kinds and types." | spec "Signature matching" |

## 3. What go2cs does today (and why)

- Go strings are golib `@string`: a private `byte[]` plus offset and length
  (`src/core/golib/string.cs:32`, `:69-71`). It is immutable and shared safely. Every conversion out to
  mutable storage copies (`:549-552`, `:608-616`).
- A `u8` literal is a `ReadOnlySpan<byte>` over PE/RVA data. Converting it to `@string` **copies**
  (`string.cs:102-107`, `:531-534`). Converting a C# `string` **transcodes and allocates**
  (`:222-227`, `:521-524`).
- To restore Go's RODATA cost model, the converter hoists each value-materializing literal into one
  `static readonly` field per package (`src/go2cs/hoistedLiteralOperations.go:21-40`). Name =
  ASCII-word slug + `ˢ` (`symbols.go:69`; `literalSlug`, `:795`). Fields are pre-boxed `object` when
  every use is an `any` slot (`:76-79`, `:1027-1034`). Emission is a pure substitution
  (`convExpr.go:533`).
- What hoists is decided by the literal's immediate Go parent: return, `=`/`:=` assignment, value
  spec, send, map index, call argument (`hoistTarget`, `:504-600`; `callArgTarget`, `:602-677`).
  Excluded: comparisons, concatenation, `*f` format positions, composite elements, `init`, builtins,
  degenerate slugs, `""`, and raw-byte literals (`:444-495`, `:649-658`).
- Rationale and measurements: `docs/phase4/DESIGN-string-literal-allocation.md` §1 and §4. User-facing
  rules: `docs/ConversionStrategies-Reference/strings.md` (lines 121-171).

## 4. The decisive constraint, proven

The C# call `F("hello")` with `F(@string s)` binds `implicit operator @string(string)` **at the call
site**. An interceptor must declare the same `(@string s)` signature, so it receives an
already-converted value. It can ignore that value and pass a cached one, but the allocation has
already happened. Neither `(string)` nor `(ReadOnlySpan<byte>)` is a legal interceptor for an
`(@string)` method. The same holds for `"hello"u8`, which copies through `op_Implicit(ReadOnlySpan<byte>)`
(`string.cs:531`). Conversion operators cannot themselves be intercepted.

An interceptor removes the cost only where the **unconverted** literal is already free at the call
site:
- an `object` / `params Span<object>` slot, where a `string` to `object` reference conversion is
  free (go2cs's `params ꓸꓸꓸany` is `Span<object>`: `src/core/fmt/print.cs:16`, `:348`, so the C#
  compiler stack-allocates the params span);
- a `string`-typed parameter, which go2cs APIs do not have. Adding them means a "string twin" on
  every callee.

## 5. Coverage census: hoisted `ˢ` names by use context

Script: a regex classifier over the current tree (`master` `d0dfcbf2f8`), `src/core/**/*.cs` (bin/obj
excluded; `*_test.cs` and the `go2cs_test_*` files counted as "core-test") and
`src/tests/Behavioral/**/*.cs.target`. Context is read from the C# text: `return x;`, `= x;`, `[x]`,
`ᐸꟷ(x)`, a call argument, or `new T(x)`. The slot type comes from the field's declared type (`object`
= `any` slot). **"Call-arg" is an upper bound for "interceptable":** it includes delegate (func-value)
invocations and golib generic helpers (`goǃ`, `deferǃ`), which cannot be intercepted, or which infer a
different `T` from a C# literal.

| Context | core-prod | core-test | behavioral | all | share |
|:--|--:|--:|--:|--:|--:|
| declarations (`@string` / `object`) | 4,317 / 62 | 5,785 / 1,939 | 1,337 / 2,917 | 16,357 | |
| **uses, total** | 5,683 | 12,598 | 4,573 | 22,854 | 100% |
| call arg, `any` slot (**interceptable, 0 B**) | 66 | 2,618 | 3,041 | **5,725** | **25.0%** |
| call arg, `@string` slot (**not helped by interception**) | 4,652 | 8,440 | 983 | **14,075** | **61.6%** |
| call arg, slot type unresolved | 29 | 144 | 0 | 173 | 0.8% |
| return | 470 | 94 | 227 | 791 | 3.5% |
| assignment / initializer | 300 | 906 | 170 | 1,376 | 6.0% |
| map-index key | 69 | 129 | 56 | 254 | 1.1% |
| channel send / constructor arg / other | 97 | 267 | 96 | 460 | 2.0% |
| distinct call sites carrying a hoisted arg | 4,627 | 10,056 | 3,731 | 18,414 | |

(The earlier survey's "~12K declarations / ~30K uses" used a different scope. This census measures
16,357 / 22,854 with converted tests included.)

The not-interceptable classes from the brief:
- **Assignments and initializers, returns, map keys, sends** are 13% of hoisted uses. They are not
  invocations.
- **`==` and `switch` labels** are not hoisted at all (Tier A span operators, zero cost), and they are
  operators.
- **Constants** are already their own fields.
- **Field initializers and composite literals** are not invocations (constructors and collection
  initializers), and they are excluded from hoisting anyway.
- **Concatenation** is an operator and is excluded already.

Separately, about **1,669** non-hoisted inline `(@string)"…"u8` call-arg or element forms in
production `src/core`, and **397** in behavioral goldens, allocate 72 B per evaluation today. Those
are degenerate-slug, composite and init-excluded sites. Interceptors would make the invocation-arg
subset both readable and zero-alloc.

## 6. Probe results

Setup: generator `gen/LiteralInterceptorGenerator.cs`, an `IIncrementalGenerator` on Roslyn 4.14.0
that uses `GetInterceptableLocation` and emits `file`-local `InterceptsLocationAttribute(int, string)`.
It runs against the **real golib** (`src/core/golib/bin/Release/net10.0/golib.dll`, read-only). The
SDK is 10.0.400 on runtime 10.0.11. Each row is 2,000,000 calls in a loop, best of 5, with allocation
from `GC.GetAllocatedBytesForCurrentThread`. **Build: 0 warnings, 0 errors.**

Generated form (excerpt):
```csharp
file static class ˢLits { internal static readonly object B0 = (global::go.@string)"hello, world"u8; … }
file static class ˢInterceptors {
    [GeneratedCode("go2cs-gen", "probe")] [DebuggerStepThrough] [MethodImpl(AggressiveInlining)]
    [InterceptsLocationAttribute(1, "E5t4pmDSenV+S/LwXswZRP0JAABQcm9ncmFtLmNz")]
    public static nint ˢI0(params Span<object> a) { a[0] = ˢLits.B0; return probe.Fx.Println(a); }
```

| Scenario (ns/call, B/call) | JIT, tiering off | Native AOT | JIT, default tiering* |
|:--|--:|--:|--:|
| `any`: hoisted pre-boxed `Println(helloˢ)` (today) | 2.47, 0 | 2.44, 0 | 7–19, 0 |
| `any`: naive `Println((@string)"…"u8)` | 13.03, **72** | 14.80, **72** | 13.24, 72 |
| `any`: **intercepted** `Println("hello, world")` | **2.56, 0** | **2.75, 0** | 2.69, 0 |
| `@string`: hoisted `LenOnly(helloˢ)` (today) | 1.28, 0 | 1.28, 0 | 3.11, 0 |
| `@string`: naive `"…"u8` | 7.04, 40 | 5.71, 40 | 7.45, 40 |
| `@string`: naive C# `"…"` | 25.14, 40 | 20.07, 40 | 20.48, 40 |
| `@string`: **intercepted** `LenOnly("…")` | **25.42, 40** | **19.91, 40** | 22.29, 40 |
| `string` twin overload, intercepted | 1.28, 0 | 1.51, 0 | 3.03, 0 |
| `string` twin overload, not intercepted (user code) | 25.30, 40 | 21.15, 40 | 24.07, 40 |
| golib identity cache in `op_Implicit(string)` (sketch) | 4.57, 0 | 5.18, 0 | 3.36, 0 |
| same cache, frozen-heap literals only | 7.02, 0 | 8.02, 0 | 5.45, 0 |

\* Default tiering leaves these run-few-times loop methods at tier-0 or OSR. Whichever method is
measured first is penalized, so read the tiering-off column. Logs: `run-jit-tc0.txt`, `run-aot.txt`,
`run-jit-default.txt`.

Correctness checks (JIT and AOT agree):
- `"héllo, 世界 😀"` intercepted has length **19**, the Go UTF-8 byte length.
- `"a\0b"` has length **3**.
- Two sites with the same literal share **one** box: the dynamic type is `go.string`, and the values
  compare equal.
- **An un-intercepted `any` call passes `System.String`** ("dynamic type is System.String, Go would
  say string"). That is the silent-failure mode if interception is ever skipped.
- `GC.GetGeneration(literal)` returns `int.MaxValue` (the non-GC heap) on both JIT and AOT, and a
  dynamic string returns 0.

**Native AOT:** works. `PublishAot` must be set inside the app project, because a global `-p:` flows
into the netstandard generator and fails with NETSDK1207. Interception is fully compile-time, so AOT
has nothing extra to do.

**Build cost** (10,000 call sites, about 3,673 distinct literals, incremental rebuild after touching
the source):

| Form | Wall | Generator time (`ReportAnalyzer`) | DLL |
|:--|--:|--:|--:|
| plain literals, no interceptors (semantically wrong for `any`) | 1.9 s | – | 0.52 MB |
| today's hoisted fields | 2.6 s | – | 0.90 MB |
| intercepted (1 interceptor per site) | 7.3 s | 0.44–0.47 s | 2.60 MB |

That is about **0.5 ms and about 170 B per intercepted site**, mostly compiling and emitting the
interceptor methods and their attributes rather than the generator itself. Sharing one interceptor
across sites with the same (target, literal) pair (`AllowMultiple = true`) would cut the method
count. The attributes are still emitted per site.

`string`-twin ambiguity, proven: with both `Len(@string)` and `Len(string)` present, `Fx.Len(default!)`
is **CS0121**. go2cs uses `default!` for a zero string (strings.md:32).

## 7. Design

### 7.1 Recommended: keep hoisting for everything except the `any`-slot invocation argument (optional pilot)

1. **Converter.** Where `callArgTarget` (`hoistedLiteralOperations.go:602`) classifies a literal as
   `hoistUseAny`, the callee is an ordinary method resolved statically (not a func value, not a
   universe builtin, not a generic helper whose `T` the argument infers), and the literal is not
   raw-byte, emit the **plain C# literal** `"hello"` instead of `helloˢ` / `(@string)"hello"u8`. The
   literal no longer counts toward that field's uses, so a literal used only there gets no field.
   Raw multi-line literals keep `.Raw()` (see 8.4).
2. **go2cs-gen InterceptGenerator.** For each invocation in a converted project where a
   `string`-typed constant argument (`SemanticModel.GetConstantValue`, which also covers C#-folded
   `"a" + "b"`) binds an `object` parameter or a `params Span<object>` element, emit an interceptor in
   a `file static class` under namespace `go2cs.Interceptors`. It writes a cached pre-boxed `@string`
   into the slot and forwards. Mark it `[GeneratedCode("go2cs-gen", …)]`, which the Go traceback
   already skips (`src/core/runtime/managed_impl.cs:4079-4083`), plus `[StackTraceHidden]`,
   `[DebuggerStepThrough]` and `AggressiveInlining`. Put the cache in a separate `file static class`
   with `u8` initializers. A field of another class is initialized before first access, so the
   §4.4 init-order hazard of package-class fields does not arise.
3. **Fail closed.** The generator reports an **error** for any converted-code invocation (only in
   `[module: GoPackage]`-marked sources) that passes a C# string constant into an `object` slot and
   could not be intercepted: a delegate target, a constructor, a lone surrogate. Without that error,
   such a site silently delivers `System.String` (proven above). An analyzer can verify coverage with
   `GetInterceptorMethod`.
4. **Plumbing.** Set `<InterceptorsNamespaces>$(InterceptorsNamespaces);go2cs.Interceptors</InterceptorsNamespaces>`
   in `src/core/golib/buildTransitive/go.lib.targets`, beside the existing `NoWarn` block (`:101-103`),
   so no consumer edits a project. Bump go2cs-gen's Roslyn reference from 4.10.0 to **≥ 4.13.0**,
   which floors the build at the 9.0.2xx SDK / VS 17.13. Converted projects already need the
   .NET 10 SDK.
5. **Determinism and incrementality.** Emit **one generated file per source file**, ordered by
   position. Name cache fields by first-use ordinal within that file. Never commit generated output:
   the location data embeds a content checksum, which changes on any byte edit, including a CRLF/LF
   checkout difference. Pipeline values must be equatable records, unlike `Collect()` over the whole
   compilation, which the probe used for brevity.

Before → after, at an `any` site:
```csharp
// before
private static readonly object countAtInitˢ = (@string)"count at init:"u8;
fmt.Println(countAtInitˢ, countAtInit);
// after (a generated interceptor supplies the cached @string)
fmt.Println("count at init:", countAtInit);
```

### 7.2 `@string` slots (62% of uses): no interceptor design keeps today's performance

| Option | Readability | Cost vs hoisted (1.28 ns, 0 B) | Verdict |
|:--|:--|:--|:--|
| Keep hoisting (today) | `helloˢ` | equal | **keep** |
| String-twin overload on every callee + interceptor | `"hello"` | equal when intercepted; 40 B when called from user code | **reject**: overloads on every API with a string parameter, CS0121 on `default!` (proven), CS0123 for method groups (the sstring-twin experience, strings.md:299), and 2^n overloads for n string parameters |
| golib identity cache in `implicit operator @string(string)`, keyed by reference, admitting only non-GC-heap (literal) strings | `"hello"` everywhere: args, returns, assignments, map keys | 0 B, +3.3–6.7 ns per evaluation (7.0 JIT / 8.0 AOT vs 1.28) | **experiment only.** It is the one lever that covers every context, but it regresses hot loops. `GC.GetGeneration` "might return MaxValue for objects allocated on non-GC heaps" ([docs](https://learn.microsoft.com/en-us/dotnet/api/system.gc.getgeneration?view=net-10.0)): a heuristic whose false negative falls back to today's correct allocating path. Gate on `PerfStringMatch` before considering it. |
| Per-literal generic cache (`Lit<T>`) | – | – | impossible: C# cannot key a generic by a literal value without a per-site type, which is a hoisted field under another name |
| Span-backed `@string`, no copy | – | – | not possible without a representation change: `@string` is a non-ref struct over a private `byte[]` (`string.cs:69`), so it cannot hold a span. A `ReadOnlyMemory`/`MemoryManager` over RVA data needs one object per literal (that is, a hoisted field), and it widens every `@string` access |

A readability-only improvement that does not touch performance (out of scope, noted): move the
hoisted declarations out of function prefixes into one per-file or per-package region. The call
sites still read `helloˢ`, but the bodies stop interleaving declaration blocks.

## 8. Behavioral edge cases (must stay exactly Go)

1. **Invalid UTF-8 (`"\xff"`, octal `\377`)**: a UTF-16 C# literal cannot represent these. They stay
   on today's byte-array `@string` path (`stringLiteralNeedsByteArray`, `convBasicLit.go:47`; strings.md
   §raw-byte). The converter must never emit them as a plain C# literal, so they are never intercepted.
2. **NUL**: fine. `"a\0b"` gives length 3 (probe). The generator writes `\u0000` into the `u8`
   initializer.
3. **Surrogate-range escapes**: Go rejects `\uD800` in literals. Byte-escaped surrogate encodings
   (`\xed\xa0\x80`) take the byte-array path. The generator also skips any constant with a lone UTF-16
   surrogate, because `u8` would be CS9026. Fail closed (7.1.3) turns that skip into an error.
4. **Raw strings with CR**: Go drops every CR inside backquotes. A C# raw literal takes CRLF from the
   checkout (`eol=crlf`), so today's `"""…"""u8.Raw()` (`string.cs:985-1024`) must stay. A plain
   multi-line C# literal handed to the generator would bake the checkout's CRs into the cache. Either
   keep `.Raw()` sites out of interception, or have the generator strip `\r` from
   `MultiLineRawStringLiteralToken` arguments only. Never strip it from interpreted `"\r\n"`, which
   keeps its CR.
5. **Identity and aliasing**: one cached box is shared by every site with that literal (probe: True).
   This is unobservable: golib `any` equality, map-of-any keys and `DeepEqual` are value-based, and
   `@string` is immutable with copying exits (DESIGN §1 "Safety preconditions"). Hoisting already
   shares instances, so this is no new exposure class.
6. **Constant folding**: Go folds `"a" + "b"` in go/types, and C# folds `"a" + "b"` to one constant.
   The generator reads `GetConstantValue`, so folded concatenations and C# `const` references work like
   literals.
7. **`len("héllo")`**: a Go constant `len` is folded by go/types and does not depend on the literal's
   form. For a value, `@string.Length` is the UTF-8 byte count (probe: 19 for a mixed string,
   including a 4-byte emoji).
8. **Named string types** (`type Token string`): a C# `string` cannot reach a wrapper through two
   user-defined conversions. Those slots keep today's form and are not `any` slots anyway.

## 9. Risks

- **Silent wrong dynamic type** if interception is skipped: generator disabled, a missed shape, or an
  IDE design-time build that differs. Mitigation: the fail-closed error in 7.1.3. This is a
  correctness dependency on a generator with no compile-error backstop, unlike go2cs-gen's other
  outputs.
- **User C# calling converted APIs**: user code has no interceptors, and today's implicit conversions
  stay correct. `fmt.Println("x")` written by hand in user C# passes `System.String`. That is
  unchanged from today, so not a regression, but converted code would now rely on the generator for
  the same spelling.
- **Tooling**: the spec says some interceptor diagnostics are "only given in the command-line build
  and not in the IDE", and there is "limited traceability in the editor". F12 goes to the original
  method. Step-into lands in the generated interceptor unless it is `[DebuggerStepThrough]`. In a
  Debug build (no inlining) the interceptor is a real frame: the Go traceback skips it through
  `[GeneratedCode("go2cs-gen")]`, and .NET stack traces skip it through `[StackTraceHidden]`.
  `runtime.Caller` depth must be re-tested with `log.Lshortfile` and `t.Helper` shapes.
- **Determinism and incrementality**: an edit to a file re-keys every interceptor in it (checksum),
  so the IDE re-runs the per-file transform on every keystroke. Measured cost is about 45 µs per site
  in the generator, which is acceptable with per-file outputs.
- **Attribute format versioning**: `version` exists so Roslyn can add encodings. Depending on the
  `GetInterceptsLocationAttributeSyntax()` API instead of hand-encoding insulates go2cs-gen. The
  file-path form is already deprecated, so never emit it.
- **Size**: about +170 B per site, about 3x the hoisted DLL in the 10K-site bench. The owner's
  standing ruling says size is an optimization target, not a gate. Per-(target, literal) sharing
  reduces it.
- **Roslyn bump 4.10 → ≥ 4.13**: touches every generator in go2cs-gen, so a full suite plus seeded
  corpus reconvert/build gate applies.

## 10. Go / no-go and effort

- **NO-GO**: interceptors as the general replacement for hoisted `ˢ` literals. The `@string` slots
  cannot keep today's 0 B per evaluation (measured 40 B per call intercepted).
- **Optional narrow GO (owner's call)**: the `any`-slot pilot in 7.1. Equal performance (2.56 vs
  2.47 ns JIT; 2.75 vs 2.44 ns AOT; 0 B), and it also removes the 72 B per call of non-hoisted
  `(@string)"…"u8` `any` arguments. It mostly changes behavioral goldens and converted tests, not
  production stdlib (66 of 5,683 production uses). Effort is roughly 6-8 working days:
  - converter arm: 1-2 days;
  - InterceptGenerator with per-file outputs and the fail-closed diagnostic: 2-3 days;
  - Roslyn bump and `go.lib.targets` property: half a day;
  - re-baselining about 3K behavioral golden lines plus the corpus reconvert/build and suite gates:
    2 days;
  - docs (strings.md hoisting section, symbols table): half a day.
  Pilot it on the behavioral tree first (`StringLiteralHoisting` already pins the hoist set).
- **Experiment, not adoption**: the frozen-literal identity cache in golib, the only route to
  `"hello"` at `@string` slots. Measure `PerfStringMatch` before any decision. Expect +3–7 ns per
  evaluation versus hoisted fields.

## 11. Reproduce

```
H:\go2cs-tmp-coord\interceptor-probe\
  gen\LiteralInterceptorGenerator.cs, ProbeGen.csproj (Roslyn 4.14.0; -p:ProbeRoslynVersion=4.11.0|4.12.0|4.13.0 for the API check)
  app\Program.cs, App.csproj   (dotnet build -c Release; DOTNET_TieredCompilation=0 dotnet bin\Release\net10.0\App.dll;
                                dotnet publish -c Release -r win-x64 -p:ProbeAot=true  [needs vswhere.exe on PATH])
  bench\Big.cs (10,000 sites), bench-hoisted\Big.cs (same, hoisted)   (-p:ProbeGen=false|true, -p:ReportAnalyzer=true -v:d)
  run-jit-tc0.txt, run-aot.txt, run-jit-default.txt
```
Opt-in checks: `-p:ProbeNoOptIn=true` reproduces CS9137, combined with
`-p:InterceptorsPreviewNamespaces=…` (alias) or `-p:InterceptorsNamespaces=go2cs|global|go2cs.Inter`.
Use `--no-incremental`: CoreCompile may not re-run for a changed property alone, so an incremental
build can report a stale result. The table above was re-measured with `--no-incremental`.

Sources: [roslyn interceptors spec](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md) ·
[roslyn PR #76312](https://github.com/dotnet/roslyn/pull/76312) ·
[roslyn CSharpExtensions.cs (GetInterceptableLocation, GetInterceptorMethod)](https://github.com/dotnet/roslyn/blob/main/src/Compilers/CSharp/Portable/CSharpExtensions.cs) ·
[Learn: What's new in C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12) ·
[Learn: What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14) ·
[Learn: GC.GetGeneration](https://learn.microsoft.com/en-us/dotnet/api/system.gc.getgeneration?view=net-10.0)
