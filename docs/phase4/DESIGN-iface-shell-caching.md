# DESIGN — Interface-shell caching and dispatch architecture

**Status:** proposal, for project-owner review — **Stage 0 executed 2026-07-26, see §10**
**Date:** 2026-07-26

> **⚠ Read §10 before acting on §0/§4/§7.** Stage 0's experiments *disproved* the P0-a hypothesis
> and *dissolved* the §7.1 AOT discrepancy. The configuration defect is real and larger in effect
> than §4 assumed for the shipped row, but its cause is not the one §1.4 inferred from reading, and
> the fix is a different one line in a different place. §10 is authoritative where it disagrees with
> anything above it.
**Instrument:** `src/tests/Performance/PerfIfaceShell`
**Scope:** `src/core/golib` (`builtin.cs`, `AdapterBinder.cs`, `AdapterRegistry.cs`), the converter
csproj template, and the perf runner. One deferred proposal touches the converter.
**Session constraints honored:** read-only on the repo. Nothing was built, no gate was run, no repo
file was created or modified. Every figure below is either quoted from a committed file (cited by
`file:line`) or taken from the two investigation inputs (labelled *measured*) or derived from those
by stated arithmetic (labelled *derived*). Nothing is estimated silently — projections are marked
**hypothesis** and carry the experiment that would settle them.

---

## 0. Executive summary

The 189× JIT figure for `PerfIfaceShell` is **not** primarily a shell-design cost. It decomposes as:

| layer | share of the shipped 474.7 ns/iteration | fixed by |
|---|---:|---|
| reflective **construction** running in its non-emitting fallback | 53.3% | **P0 — build configuration** |
| reflective **forwarded call** (object tier), same fallback | 27.4% | **P0**, then P6 |
| the `builtin._<T>` **lookup/dispatch walk** | 17.8% | **P1 / P2** |
| actual shell **allocation** | 1.1% | P3 (marginal — see §6.4) |

The single largest item is a **configuration defect, not an architecture defect**: the shipped
`Release` benchmark binary runs with `System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported = false`,
so `ConstructorInvoker`/`MethodInvoker` can never emit their IL invoke stubs. Flipping only that
switch on a byte-identical replica of the emitted code took one iteration from **474.7 ns → 86.2 ns**
(*measured*, 5.5×), i.e. **~183× → ~33× vs Go**.

Once dynamic code is enabled, the cost profile inverts: **66.8% of the remaining 86.2 ns is the
`_<T>` walk itself**, and reflection drops to 27%. That is where the durable architectural work is,
and it is a golib-only change with no converter footprint.

Recommended order — strictly by measured gain over blast radius:

| # | Proposal | Expected effect | Blast radius | Verdict |
|---|---|---|---|---|
| **P0** | Fix the JIT-row build configuration + add a measurement guard | **5.5× measured** on the row; corrects a published number | 1 MSBuild property, bin/obj hygiene, ~15 lines in the runner | **DO FIRST** |
| **P1** | Unify the assert ladder into a Go-style per-interface **itab cache** (nominal + shell in one entry, epoch-invalidated); delete the always-missing tuple lookup and the dead `GetType()` | **hypothesis** ~20–30 ns/iter of the post-P0 86.2 | golib only; CNR byte-identical | **DO** |
| **P2** | Monomorphic (single-entry) inline cache in front of the itab dictionary | **hypothesis** ~10–14 ns/iter; overlaps P1 | golib only, ~20 lines | **DO, same stage as P1** |
| **P6** | Drop the per-call `object[]` in `GoShellBinding.Invoke` via `MethodInvoker`'s fixed-arity overloads | small on JIT; matters under AOT where **both** tiers are reflective; −32 B/call | one golib method | **DO, cheap** |
| **P3** | `ConditionalWeakTable` per-referent shell instance cache | **likely a wash on time** post-P0 (~15.6 ns removed, ~8–16 ns added); −72 B/iter | golib only, but the most semantically delicate | **EXPERIMENT ONLY — do not land on faith** |
| **P5** | Pre-JIT / `RuntimeHelpers.RunClassConstructor` warming | zero steady-state effect | — | **REJECT** (§6.6) |
| **P4** | Struct / stack shells for the value tier | zero — the shell is returned as an interface and re-boxes | — | **REJECT** (§6.5) |
| **P7** | Converter-emitted per-call-site inline cache (Go's PIC analogue) | **highest ceiling** (~40–50 ns/iter) | 1,425 corpus sites + 86 behavioral goldens + readability charter | **DEFER** (§6.7) |

**Honest ceiling.** Even with everything above, a shell-resolved assert still *constructs a wrapper
object* that Go does not need: a Go interface value is two words and the itab probe is ~1.3 ns. A
realistic post-P0/P1/P2/P6 floor is **~15–25 ns/iteration** (*hypothesis*), i.e. **~6–10× Go**, not
parity. Parity on this operation is only reachable by not building a wrapper at all — nominal
adapters (which the recorders used to supply and which remain the 1.1 ns path) or
`IDynamicInterfaceCastable`, already recorded as the deferred successor in
`docs/phase4/DESIGN-named-interface-wrappers.md`.

---

## 1. Ground truth — what the code actually does today

### 1.1 The memoized assert, step by step

Entry is the emitted `values[0]._<run_type>(ᐧ)`
(`src/tests/Performance/PerfIfaceShell/PerfIfaceShell.cs:35`, emitted by
`src/go2cs/convTypeAssertExpr.go:84`), which lands in
`builtin.TryTypeAssert<T>` (`src/core/golib/builtin.cs:1536`). In steady state, for an
already-decided (dynamic type, interface) pair:

| step | `builtin.cs` line | cost |
|---|---|---|
| `while (target is IInterfaceAdapter …)` unwrap loop | 1542 | isinst |
| `target is null` | 1546 | branch |
| `case string str when typeOfT == typeof(@string)` | 1556 | isinst + Type compare |
| `case T typedTarget` — the **nominal fast path**, ≈1.1 ns | 1559 | isinst |
| `case IжAdapter { Box: T box }` | 1566 | isinst ×2 |
| `Type targetType = target.GetType()` | 1571 | **virtual call whose result is DEAD for every interface target** |
| `typeOfT.IsInterface` | 1573 | `RuntimeType` property call |
| `target is IжAdapter pointerAdapter` | 1580 | isinst |
| `AdapterRegistry.TryWrap(…)` → `s_factories.TryGetValue((value.GetType(), interfaceType))` | 1582 → `AdapterRegistry.cs:67` | **`GetType()` + `(Type,Type)` tuple hash + dictionary MISS, by construction always** |
| `typeOfT.IsValueType && targetType.IsValueType` | 1589 | short-circuits false for an interface `T` |
| `target is IжAdapter { Box: not null }` | 1623 | isinst |
| `ShellCache<T>.TryGetFactory(structuralTarget.GetType(), …)` | 1629 → 1677 | **`GetType()` + Type-keyed dictionary HIT** |
| `shellFactory(structuralTarget)` → `ConstructorInvoker.Invoke` | 1631 → `AdapterBinder.cs:239` / `:264` | **delegate hop + a FRESH shell allocation, every assert** |
| `is T memoizedShell` | 1631 | isinst |

Three `GetType()` calls, **two** dictionary lookups (one of which can never hit on this path), ~7
type tests, and one allocation — per assert.

**Line 1571 is provably dead work for an interface target.** `targetType`'s only consumers (1589,
1592, 1597, 1605) sit behind `typeOfT.IsValueType &&`, which is `false` whenever `T` is an interface,
so the `&&` short-circuits before `targetType` is read.

**Line 1582 is provably a miss for any shell-resolved pair** — a hit would have returned at 1584 and
the pair would never have reached `ShellCache`.

### 1.2 The forwarded call

- **Delegate tier** (`Δrun_type<box>`, pointer-sourced): a static-field read plus a delegate
  invocation — `Generated/go2cs-gen/go2cs.TypeGenerator/go.main_package.run_type.g.cs:80-83`.
  **4.44 ns/call** per `DESIGN-named-interface-wrappers.md:104`; **2.19–2.46 ns, 0 B** *measured* in
  the isolation harness. This tier is already essentially at the C# interface-dispatch floor
  (raw two-impl dispatch measured at 1.36–1.58 ns).
- **Object tier** (`Δrun_typeᴛObj`, value-typed): `run_type.g.cs:130` →
  `GoShellBinding.Invoke` (`AdapterBinder.cs:344-362`), which allocates an `object?[args.Length + 1]`
  (`:355`) and calls `MethodInvoker.Invoke(null, (Span<object?>)arguments)` (`:361`), boxing the
  `nint` return. **22.15 ns/call** per `DESIGN:104`; **10.51 ns, 56 B** *measured* with dynamic code
  enabled, **129.84 ns** with it disabled.

Allocation per benchmark iteration decomposes exactly to the *measured* 128 B: 32 B object shell +
32 B `object[1]` + 24 B boxed `nint` + 40 B delegate shell. Identical in both build configurations —
the switch changes speed, not allocation.

### 1.3 Cache inventory (what exists)

| cache | location | key | read on hot path? |
|---|---|---|---|
| `ShellCache<TInterface>.s_factories` | `builtin.cs:1673` | `Type` | **yes — the intended hot lookup** |
| `AdapterRegistry.s_factories` (nominal) | `AdapterRegistry.cs:36` | `(Type, Type)` | **yes — and always misses here** |
| `AdapterRegistry.s_shellFactories` (durable record) | `AdapterRegistry.cs:43` | `(Type, Type)` | no (cold + `Publish`) |
| `builtin.Cache<TInterface>.s_results` | `builtin.cs:2591` | `Type` | no (skipped on a `ShellCache` hit) |
| `AdapterBinder.s_shellSpecs` | `AdapterBinder.cs:86` | `Type` | no |
| generated shell statics (`s_LenByPtr`, `ᴛBoundByPtr`, …) | `run_type.g.cs:47-48, 71-72` | per closed shell | static field read per call |
| `GoShellBinding.m_invokers/m_dereference` | `AdapterBinder.cs:330-338` | per pair, shared | array read per call |

**There is no cache of shell *instances*.** Only the *factory* is memoized
(`AdapterBinder.cs:113-124`, projected in `builtin.cs:1671-1688`), so `builtin.cs:1631` calls
`shellFactory(structuralTarget)` on every assert. *Measured* confirmation: asserting the same object
to the same interface twice returns `ReferenceEquals == False` on **both** tiers.

### 1.4 The configuration defect (P0's evidence)

`src/tests/Performance/PerfIfaceShell/bin/Release/net9.0/PerfIfaceShell.runtimeconfig.json` contains:

```
"System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": false
```

and an `includedFrameworks` block (= self-contained). Corroborating evidence read this session:

- `src/tests/Performance/PerfIfaceShell/PerfIfaceShell.csproj:37` — `<PublishTrimmed>True</PublishTrimmed>`,
  inherited verbatim from the converter template `src/go2cs/csproj-template.xml:37`, so **every**
  converter-generated csproj carries it. `<PublishReadyToRun>true</PublishReadyToRun>` sits beside it
  at `:36`.
- `obj/project.assets.json` for the plain (non-AOT) restore carries
  `"frameworkReferences": { "Microsoft.NETCore.App": { "privateAssets": "all" } }` — the
  self-contained marker — plus an auto-referenced `Microsoft.NET.ILLink.Tasks` and a
  `Microsoft.NETCore.App.Crossgen2.win-x64` download dependency.
- `bin/Release/net9.0` holds **187** framework DLLs; `bin/Release/net9.0/PerfIfaceShell.deps.json`
  has `"runtimeTarget": ".NETCoreApp,Version=v9.0/win-x64"` and a dependency on
  `Microsoft.DotNet.ILCompiler` — i.e. the plain-Release output tree is also **cross-polluted with
  AOT/R2R artifacts**, the exact failure mode `src/tests/Performance/Directory.Build.props` warns
  about in its own comment.
- The SDK knob is `$(DynamicCodeSupport)`
  (`Microsoft.NET.Sdk.targets:647-650`, condition `'$(DynamicCodeSupport)' != ''`), so an explicit
  property deterministically overrides whatever default set it.

**`docs/Performance.md:72` states "JIT = framework-dependent `Release`". The artifacts on disk say
otherwise.** Whether the switch arrives from the template's `PublishTrimmed` or from a polluted
output tree is *not settled by reading* — and it does not need to be to justify the fix, because
both are wrong and Stage 0's first experiment distinguishes them (§7.1).

---

## 2. What Go does, and which parts of it transfer

| Go mechanism | go2cs today | transferable? |
|---|---|---|
| Global `(concrete type, interface) → *itab` hash cache, one entry per pair | **two** caches consulted in sequence (nominal `(Type,Type)`; shell per-interface `Type`), plus a structural gate | **Yes — P1.** One entry per pair, whichever tier produced it. This is the single most direct Go analogue available. |
| Interface value = 2 words (type ptr, data ptr) — **zero allocation** per assert | a heap shell object per assert (32/40 B) | **No.** C# has no two-word interface value; the shell *is* the itab+data fused into an object. P3 amortizes it; nothing removes it short of `IDynamicInterfaceCastable`. |
| itab built once per pair at first use, then pure hash probe (~1.3 ns) | factory built once per pair; probe is a `ConcurrentDictionary` hit | **Partly — P2.** A monomorphic slot in front of the dictionary approximates the probe cost. |
| Compiler-emitted per-site type checks for statically known pairs | nominal adapters (`case T` at `builtin.cs:1559`, ≈1.1 ns) | **Already present**, and it is the fast path the retired recorders used to widen. |
| Inline caching at the call site | none | **P7** — highest ceiling, worst blast radius (§6.7). |
| itab cache never invalidated (method sets fixed at compile time) | shell decisions deliberately never invalidated (`AdapterBinder.cs:117-122`) | **Already matched.** P1 adds an epoch only for *late nominal registration*, which Go has no analogue for (Go has no lazily-loaded assemblies). |

---

## 3. Semantic constraints any proposal must respect

These are the invariants I verified in source; each is a hard gate on the proposals below.

1. **A shell must stay invisible to Go.** `%T`, re-assert, type switch and interface equality must
   see the wrapped value, never the shell. Enforced by `IInterfaceAdapter` unwrapping in
   `builtin.AreEqual(object, object)` (`builtin.cs:2517-2521`), in `TryTypeAssert` (`:1542`) and in
   `builtin.type()`. Guarded by `src/tests/Behavioral/NamedInterfaceAdapterIdentity` (its `main.go`
   states the contract explicitly).
2. **Pointer identity is Go's `==` for pointer-sourced interfaces.** `AreEqual` unwraps `IжAdapter`
   to the box and relies on `ж<T>`'s identity-based `Equals`/`GetHashCode`
   (`src/core/golib/ж.cs:520-655`). A shell holding the same box therefore compares correctly
   regardless of shell identity.
3. **Go copies the value into the interface for a value-typed dynamic value.** The object shell
   holds `object m_targetᴛ` — *the same box reference* the `any` slot already held
   (`run_type.g.cs:117`), so a cached shell and a fresh shell are observationally identical. The
   **generic** shell over a *value* type instead copies the struct out of the box in its constructor
   (`run_type.g.cs:52`), so a cached instance would freeze a snapshot. That is the one shape where a
   per-referent cache could diverge, and only if a boxed struct is ever mutated in place. Grepping
   `src/core/golib` for `Unsafe.Unbox` returns nothing, so no in-place box mutation exists today —
   but P3 must **not depend on that**; see §6.4's eligibility rule.
4. **Nominal adapters must not be shadowed by a shell registered earlier.** The current design keeps
   the two dictionaries separate precisely for this
   (`AdapterRegistry.cs:38-43` comment: a memoized shell must never shadow an adapter registered
   later by a lazily-loaded assembly's module initializer). P1 unifies the caches and therefore
   **must** carry an invalidation epoch (§6.2).
5. **A miss is normal control flow, never an exception.** `TryTypeAssert` is the dispatch point for
   emitted type-switch case guards (`builtin.cs:1530-1534` remarks); `AdapterBinder.IsBindingFailure`
   (`AdapterBinder.cs:305-315`) exists for this. No proposal may introduce a throwing path.
6. **golib must stay Native-AOT-clean under `TrimMode=partial`.** No new `MakeGenericType`,
   `MakeGenericMethod`, `Delegate.CreateDelegate` over runtime-discovered types, or `Reflection.Emit`
   anywhere on these paths. The existing `MakeGenericType` (`AdapterBinder.cs:205`) is belted by the
   object shell; the remaining `MakeGenericMethod` (`builtin.cs:1714-1718`) is the reflection-bridge
   entry, out of scope here and already recorded as such (`DESIGN:166-169`).
7. **Shells are not sound map keys today, and no proposal may make that worse.** `map<K,V>` wraps a
   plain `Dictionary<TKey,TValue>` with the default comparer (`src/core/golib/map.cs:55-59`), and the
   shells override neither `Equals` nor `GetHashCode` — so two asserts of the same value already
   produce two unequal keys, which is *already* a divergence from Go. P3 would incidentally reduce
   it; nothing here should be justified on that basis, and the underlying gap deserves its own item.

---

## 4. P0 — Fix the measurement configuration (do first)

### Mechanism

Three separable pieces, smallest first:

**P0-a — WITHDRAWN by Stage 0 (§10.3): measured a no-op in every non-ILC configuration. Not
implemented.** *Original text retained for the record.* Add to the converter template
(`src/go2cs/csproj-template.xml`, beside the existing block at `:35-41`):

```xml
<!-- Trimming implies "no dynamic code" in the SDK's feature-switch defaults, but a JIT-hosted
     build (trimmed or not) can emit. Leaving it false forces every reflective invoker in golib
     onto its non-emitting fallback: measured 5.5x on PerfIfaceShell. Native AOT sets it false
     itself, which is correct there. -->
<DynamicCodeSupport Condition="'$(PublishAot)'!='true' AND '$(DynamicCodeSupport)'==''">true</DynamicCodeSupport>
```

**P0-b — output-tree hygiene.** The plain-Release `obj/`+`bin/` of the perf projects carry AOT/R2R
restore assets (§1.4). Either clean before the JIT build in `PerformanceRunner`, or extend the
existing `BaseIntermediateOutputPath`/`BaseOutputPath` separation in
`src/tests/Performance/Directory.Build.props` so the non-AOT path is equally isolated.

> **Stage 0 result (§10.1–10.2): neither.** The separation already exists and is *overridden* by the
> converter templates' `OutDir` pin, which also silently defeats the Phase-4 test-host's `bin\tests\`
> isolation. The fix is one condition in `csproj-template.xml` **and** `test-csproj-template.xml`;
> `Directory.Build.props` needs no change.

**P0-c — a measurement guard (this is the durable part).** After building the JIT variant,
`PerformanceRunner` reads `<Exe>.runtimeconfig.json` and **fails the run** (or at minimum records it
in the environment line at `PerformanceRunner/*.cs:702`) if
`System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported` is `false`, or if the
configuration is self-contained while the docs claim framework-dependent. This is the same discipline
as the false-green guards in `CLAUDE.md`: a mis-configured binary must not be able to silently ship a
published number again.

### Expected gain

*Measured* on a byte-identical replica of the emitted code, flipping only this switch:
**474.7 → 86.2 ns/iteration (5.5×)**; single-shot `Run(5M)` 527–617 → 88–93 ns/iter. Projected onto
the committed row: **2,467.1 ms → ~431 ms**, **189.18× → ~33× vs Go** (*derived*: 86.2 ns × 5M ÷ 13.0 ms).

Note this is not confined to `PerfIfaceShell`: the switch governs *every* reflective invoker in a
converted program. `docs/Performance.md:145` attributes the Sort row's 3.67× to
"reflection-created delegates" in `Interface<T>`, and golib `fmt` formatting binds members
reflectively. **Re-measuring the whole suite after P0 is part of the stage**, and other rows may move.

### Blast radius

One MSBuild property in the converter template — but that template regenerates **every** converted
csproj, so a re-transpile touches every project file in the corpus and in
`src/tests/Behavioral`. The behavioral goldens are `.cs`/`.cs.target`, **not** `.csproj`, so
`check-no-regression` stays byte-identical; the csproj churn is real and must be reviewed and landed
deliberately (this is the same footprint class as the shared-template `<Version>` churn recorded in
the NuGet memory topic — 385 csprojs — so expect that scale).

### AOT / trimming safety

The condition excludes `PublishAot`, where ILC sets the switch itself and correctly. For a trimmed
non-AOT publish the property makes the runtimeconfig **more** truthful: a JIT-hosted trimmed app *can*
emit dynamic code. The cost is that ILLink keeps dynamic-code paths it would otherwise trim → larger
output. That trade must be stated for the owner: **binary size up, throughput up**. If the owner
prefers minimum size for published apps, the narrower variant is to scope the property to the
performance suite's `Directory.Build.props` only — but then the *published* JIT figure stops
describing what a real converted app does, which I would argue against.

### Semantic risk

Behavior-affecting only where BCL code branches on `RuntimeFeature.IsDynamicCodeSupported`. Setting
it true when a JIT is present is strictly more correct; the risk is the *reverse* case (an app
published AOT), which the condition excludes.

### Verification plan

1. Clean `bin/`+`obj/` of one perf project; `dotnet build -c Release`; read the runtimeconfig.
   **This is the experiment that distinguishes "template property" from "polluted output tree"** and
   must run before the fix is written.
2. `run-performance.ps1 --filter IfaceShell --runs 5` before/after. The runner's Verify phase already
   requires identical timing-filtered stdout across Go/JIT/AOT before anything is timed, so the
   checksum (`8000000`) is a built-in correctness gate.
3. Full `run-performance.ps1 --update-readme` and copy the prior table into the *History* section per
   `docs/Performance.md:177-179`.
4. Re-sync the stale prose at `docs/Performance.md:151` and `:154-155` in the same change (§7.2).

---

## 5. P1 + P2 — The itab cache (the architectural core)

### 5.1 P1 — one entry per (dynamic type, interface), whoever produced it

**Mechanism.** Replace the sequence *[nominal `(Type,Type)` lookup] → … → [shell per-interface
lookup]* with a single per-closed-interface cache that stores the resolver for the pair, exactly as
Go's itab cache stores one itab per pair:

```csharp
private static class Itab<TInterface>
{
    // One entry per dynamic type. Value is the resolver, whatever tier produced it:
    // a generated nominal adapter factory, a runtime shell factory, or null = decided MISS.
    private static readonly ConcurrentDictionary<Type, Func<object, object>?> s_entries = [];
    private static int s_epoch = AdapterRegistry.Epoch;   // late-registration guard, §3.4
    …
}
```

Filling order on a miss preserves today's precedence exactly: nominal registry first, then the
structural gate, then `AdapterBinder`. Only the *reads* collapse.

Bundled with it, three mechanical removals that need no new structure:

- **Delete the dead `Type targetType = target.GetType()` at `builtin.cs:1571`** for the interface
  path (sink it into the value-type leg that actually uses it, `:1589`).
- **Delete the `AdapterRegistry.TryWrap` probe at `builtin.cs:1582` from the memoized path** — it is
  a `(Type,Type)` tuple hash that by construction always misses for a shell-resolved pair. It stays
  as part of the itab *fill*, not the itab *read*.
- **Hoist `typeof(T).IsInterface` / `IsValueType`** (`:1573`, `:1589`) into a per-closed-generic
  `static readonly bool`, turning two `RuntimeType` property calls into static field reads. Same
  trick, same reasoning, as the existing `Cache<TInterface>` (`builtin.cs:2586-2591`).

Net: **three `GetType()` calls → one**, **two dictionary lookups → one**, two property calls → two
field reads, and one always-failing tuple hash gone.

**Correctness — the epoch.** Unifying the caches reintroduces the hazard `AdapterRegistry.cs:38-43`
was written to avoid: a shell memoized for a pair before a lazily-loaded assembly's module
initializer registers the *nominal* adapter for the same pair. Fix: `AdapterRegistry.Register`
(`:53-56`) bumps a `static int s_epoch`; each `Itab<T>` read compares its captured epoch against the
global and clears on mismatch. Cost is one static-int read and compare (sub-nanosecond); correctness
is exact, and the common case (no registrations after startup) never clears. The durable record
stays `AdapterRegistry` — `Itab<T>` remains a *projection*, formed by reading the registry back,
never by forming a second decision (the invariant `builtin.cs:1682-1684` already states).

**Expected gain.** Attacks the *measured* 57.6 ns/iteration (66.8%) lookup/dispatch residual of the
post-P0 86.2 ns. Component estimate: a `ConcurrentDictionary<(Type,Type),…>` miss costs two
`RuntimeHelpers.GetHashCode` calls plus a combine plus a bucket probe; two `GetType()` virtual calls;
two `RuntimeType` property calls. **Hypothesis: 10–15 ns/assert, 20–30 ns/iteration** — taking 86.2
to roughly **56–66 ns/iter (~22–25× Go)**. *This number is a hypothesis and Stage 2's gate is the
measurement, not the reasoning.*

**Blast radius.** `builtin.cs` + `AdapterRegistry.cs` only. Converter untouched ⇒ CNR byte-identical
(the same argument Stage 2 of the wrappers design used successfully). The risk is not compile
breakage — it is the *tier-precedence* semantics, which the guard set in §5.3 exists to pin.

**AOT / trim safety.** Pure `ConcurrentDictionary` + static generics. No new reflection. Safe.

**Semantic risk.** Concentrated entirely in the epoch. A missed invalidation means a shell answering
where a nominal adapter should — behaviorally identical forwarding (both are `IInterfaceAdapter`
over the same receiver methods, both unwrap to the same box), so the *observable* risk is low, but
`NamedInterfaceLateAssert` is the cross-assembly shape that must stay green, and a new guard should
exercise registration *after* a shell decision (§5.3).

### 5.2 P2 — a monomorphic slot in front of the dictionary

**Mechanism.** Go's itab probe is a hash lookup; real call sites are overwhelmingly monomorphic. Put
a single-entry cache in front of `Itab<T>`'s dictionary:

```csharp
private sealed class Entry { internal readonly Type Type; internal readonly Func<object,object>? Factory; … }
private static Entry? s_last;   // ONE reference — read and written atomically
```

Read path: one static field read, one `Type` reference compare, then the delegate. On a mismatch,
fall through to the dictionary and re-arm.

**The tearing trap, and why the shape above avoids it.** Two separate static fields (`s_lastType`,
`s_lastFactory`) can be observed torn across threads — pairing type A with factory B, which would
silently construct the *wrong shell*. Storing a single immutable `Entry` reference makes the pair
atomic by construction. This must be a review checkpoint, not a comment.

**Expected gain.** Replaces a `ConcurrentDictionary` hit (~5–8 ns) with a field read + reference
compare (<1 ns). **Hypothesis: 5–7 ns/assert, 10–14 ns/iteration.** It **overlaps P1** — P1's
saving is partly the same dictionary — so the two do **not** add linearly and must be measured
together and separately.

**Blast radius.** ~20 lines inside `builtin.cs`. **Semantic risk:** the tearing hazard above; plus
thrash on a polymorphic site, which costs one extra reference compare and is bounded.

### 5.3 Verification plan for P1 + P2

- **Fast loop:** `src/tests/Behavioral/run-behavioral.ps1 --filter <Name>` over the interface guard
  set that exists today — `NamedInterfaceAdapterIdentity`, `NamedInterfaceLateAssert`,
  `NamedInterfacePointerMethodSet`, `TypeSwitchNamedInterfaceCase`, `DerivedInterfaceStructuralProbe`,
  `AnonIfaceThroughPointerAdapter`, `AnonIfaceMethodSetWidening`, `AnonInterfaceSignatureAssert`,
  `AnonInterfaceCrossFile`, `OptionalInterfaceStructuralAssertion`, `InterfaceToInterfaceAdapter`,
  `InterfaceToInterfaceAssertion`, `IfaceToIfaceNarrow`, `TypedNilInterface`,
  `InterfaceMapKeyPointer`, `DynamicInterfaceKeywordMethod`, `IfaceFieldEmbedAdapter`.
- **New guard owed by P1:** a behavioral project where a pair is resolved by a *shell* first and the
  *nominal* adapter for the same pair is registered afterwards by a second assembly — the epoch's
  reason for existing. `NamedInterfaceLateAssert`'s `latelib` shape is the starting point.
- **Central gates (after the concurrent Tier-C session lands):** full `run-behavioral.ps1` (budget
  1200 s per the `CLAUDE.md` table), `check-no-regression.ps1` (budget 480–600 s) — expected
  **byte-identical**, since no converter file is touched — and a `go-src-converted` corpus build.
- **Operational gate — mandatory for a golib change:** re-run the **43-package Phase-4 validated
  sweep**. A golib change can be compile-clean and operationally wrong; the corpus build does not
  cover interface resolution at run time, and `fmt` probes three interfaces per formatted value, so
  this path is exercised by essentially every validated package.
- **Perf gate:** `run-performance.ps1 --filter IfaceShell --runs 5`, reported as three A/Bs
  (post-P0 baseline → +P1 → +P1+P2) so the overlap in §5.2 is visible rather than assumed.

---

## 6. The remaining candidates, evaluated honestly

### 6.1 (see P0, §4)

### 6.2 (see P1, §5.1)

### 6.3 P6 — kill the per-call `object[]` on the object tier

**Mechanism.** `GoShellBinding.Invoke` (`AdapterBinder.cs:344-362`) always builds
`new object?[args.Length + 1]` to prepend the receiver. `MethodInvoker` exposes fixed-arity overloads
— the code already relies on their existence: the comment at `AdapterBinder.cs:359-360` says the
`Span` overload is chosen explicitly *because* `object?[]` "would otherwise be ambiguous with the
single-argument overload". Dispatch on `args.Length` to `Invoke(null, target)`,
`Invoke(null, target, a0)`, … for the arities the BCL provides, falling back to the `Span` path
beyond that. A zero-argument Go method — `Len()`, `Error()`, `String()`, `Read(p)` — is the common
case and needs no array at all.

**Expected gain.** Removes 32 B and its zeroing per object-tier forwarded call. On the JIT that is a
*measured* couple of ns out of `X5 = 10.51 ns`. It matters more **under Native AOT**, where the belt
fires and *both* tiers become object shells (`DESIGN:141-143`, tier names verified in that A/B), so
the AOT row pays it twice per iteration — and AOT is the configuration that legitimately cannot
escape reflection.

**Blast radius.** One golib method. **AOT/trim:** no change in reflection surface. **Semantic risk:**
none beyond arity dispatch correctness; the deref-per-call rule at `:352-353` is untouched.
**Verification:** the guard set in §5.3 plus the AOT column of `run-performance.ps1`.

*Not fixable this way:* the boxed return. `MethodInvoker` returns `object?` and the shell unboxes
(`run_type.g.cs:130`). Removing that requires a non-reflective forwarder, which requires a generic
instantiation, which is exactly what the object tier exists to avoid.

### 6.4 P3 — `ConditionalWeakTable` per-referent shell cache — **experiment only**

> **⚠ 2026-10-13: ADOPTED.** The experiment met its bar on the JIT and Native AOT legs; see §12.

**Mechanism.** `ShellInstance<TInterface>` = a `ConditionalWeakTable<object, object>` keyed on the
Go dynamic value (the `ж<T>` box, or the boxed value). On assert: probe the table, return the
existing shell, else construct via the itab factory and add. GC-safe by construction: an entry dies
with its referent.

**Eligibility rule (mandatory).** Only install a cached instance when the shell **holds a reference
to the dynamic value** rather than a copy of it:
- object shell — always eligible (`run_type.g.cs:117`, holds `object m_targetᴛ`);
- generic shell, pointer-sourced — eligible (holds `ж<ΔTTarget>`);
- generic shell over a **value** type (the belt case) — **not eligible**: its constructor copies the
  struct out of the box (`run_type.g.cs:52`), so a cached instance would freeze a snapshot. The
  binder already knows which case it is (`AdapterBinder.cs:194, 200-202`), so eligibility is decided
  once at factory-build time, not per assert.

**Expected gain — and why I do not recommend landing it on faith.** Post-P0 it removes the *measured*
construction overhead (10.3 ns/iter) plus the allocation (5.3 ns/iter) = **15.6 ns/iter**, and adds
two `ConditionalWeakTable` probes. A CWT lookup is a dependent-handle hash probe, realistically
**4–8 ns each**. Net is somewhere between **−8 ns and +1 ns per iteration** — i.e. **plausibly a
wash, plausibly a small loss**. It does remove **72 of the 128 B/iteration** (both shells; the
`object[]`+box remain until P6).

Pre-P0 the picture is completely different (it would remove ~258 ns/iter), which is precisely why
P0 must land *before* this is evaluated — otherwise P3 would be adopted for a benefit that P0 already
delivers more cheaply.

**Blast radius.** golib only, but it is the most delicate proposal here.

**Semantic risk.** Real, and worth naming even though I believe the eligibility rule closes it:
- Shell identity becomes stable where it was fresh. Verified safe against `AreEqual`
  (`builtin.cs:2517-2521` unwraps `IInterfaceAdapter` before comparing), `%T`, `type()`, and
  re-assert. The `NamedInterfaceAdapterIdentity` guard exists for exactly this contract.
- It makes shells *more* Go-like as map keys (§3.7) — a side effect, not a justification.
- Under the eligibility rule, a cached and a fresh shell hold the **same reference**, so even
  in-place mutation of a box is observed identically by both. That is what makes the rule sufficient
  rather than merely prudent.

**Verification.** Only worth building as a *measured experiment*, in the isolation harness first, and
only adopted if the A/B shows a real win post-P0. New guard if adopted: a behavioral project that
mutates through a `ж` after obtaining a shell and re-reads through the shell (pins the
"per-call deref" rule at `AdapterBinder.cs:352-353` against a cached instance), plus a re-assert
equality case.

### 6.5 P4 — struct / stack shells for the value tier — **REJECT**

`TryTypeAssert<T>` returns `T`, and `T` **is** the interface type. A `struct` shell implementing the
interface is boxed the instant it is returned as `T`, so the allocation is not removed — it is moved
and possibly duplicated (`is T` re-tests, `IInterfaceAdapter` unwraps). Avoiding the box would require
the *call site* to stay generic over the concrete shell type, which the emitted form
`values[0]._<run_type>(ᐧ)` (`PerfIfaceShell.cs:35`) cannot express: the site knows only the interface.
Beyond that, the *measured* pure allocation cost is **1.1% of the shipped iteration** and 6.1%
post-P0 — the smallest line in the attribution. Rejected on both feasibility and payoff.

### 6.6 P5 — pre-JIT / warming via `RunClassConstructor` — **REJECT for steady state**

The mechanism exists (`AdapterBinder.cs:220`) but for an unrelated purpose: forcing a *hand-written*
shell's static binder so a binding failure is decided at factory-build time rather than surfacing as
a null-dispatch. It is not a warming hook, and extending it would not touch steady state at all —
by definition warming only moves cold cost earlier.

Two honest notes on the cold path, recorded but not proposed:
- *Measured* first-hit cost is **156.9–236.6 µs** per never-before-seen pair (~1,800–2,600× a
  steady-state assert), dominated by the process-wide extension-method scan
  (`runtime/TypeExtensions.cs:49-73`, invalidated on `AssemblyLoad` at `:75-87`) and the
  `StructurallyImplements` linear probe (`:373-419`). The caches pay for themselves after roughly
  1,300 asserts of a pair. That is a *startup* concern, and pre-warming it would mean enumerating
  pairs at build time — which is re-inventing the recorders retired on 2026-07-25.
- *Measured*, dynamic-code-enabled only: the **second** assert of a pair costs ~15.5 µs and 1,432 B
  (the BCL lazily emitting its invoke stub), then drops to steady state. So each pair has *two*
  one-time costs, not one. Interesting, not actionable.

### 6.7 P7 — converter-emitted per-call-site inline cache — **DEFER**

**Mechanism.** Go's PIC analogue: each emitted assert site carries a static cache field holding the
last `(Type, Func<object,object>)` it resolved; the site tests `target.GetType() == cachedType` and
invokes the cached factory directly, calling into golib only on a miss. It would collapse the whole
`_<T>` walk to a type compare plus a delegate call — plausibly **~10–15 ns/iteration**, i.e. the
largest remaining win after P0 (**hypothesis**, attacking the *measured* 57.6 ns residual).

**Why defer.**
1. **Blast radius:** 1,425 `._<` sites in `src/go-src-converted` and 86 in the behavioral `.cs.target`
   goldens (counted this session). A converter change here re-baselines the entire golden corpus and
   makes CNR non-byte-identical by design — the largest footprint of anything in this document.
2. **It fights the project charter.** go2cs's stated goal is C# a Go developer can read and follow.
   A static mutable cache field beside every type assertion is machinery in the *visible* emitted
   code, which the architecture deliberately keeps in partial classes and generated files. This is a
   product decision for the owner, not a perf trade.
3. **P2 captures most of it without any of that.** A monomorphic slot per *interface* (in golib)
   approximates a monomorphic slot per *site* whenever a given interface is asserted against one
   dominant dynamic type — which is the same locality assumption P7 relies on. Measure P2 first; if
   P2 delivers, P7's marginal value collapses.

**Recommendation:** revisit only if, after P0/P1/P2/P6 are measured, the residual is still dominated
by the `_<T>` walk *and* the owner accepts the emitted-code footprint.

---

## 7. Documentation deltas to resolve in the same arc

These are recorded because two of them would corrupt any measurement work that trusts them.

### 7.1 The unreconciled AOT figure — **RESOLVED by Stage 0, see §10.4: no discrepancy, one label is wrong**

`docs/phase4/DESIGN-named-interface-wrappers.md:133` reports the Stage-3 Native AOT binary at
**202 ms for 10M asserts**; `:160` reports **278.6 ms** for what reads as the same measurement; the
committed table at `docs/Performance.md:90` reports the same benchmark, same date, at
**979.1 ms**. That is a 3.5–4.8× gap with no reconciliation anywhere. The two sources disagree about
whether a memoized assert+call costs ~20–28 ns or ~98 ns under AOT — a factor that changes which
proposal is worth doing. **One of those harnesses is not measuring what its label says, and Stage 0
must determine which.**

### 7.2 The `ShellCache` A/B arithmetic — **WITHDRAWN by Stage 0, see §10.4**

*The correction below is itself incorrect: it trusts the "10M asserts" label, which is the actual
defect. The harness ran 2M asserts, so the original ≈53 ns/assert is right. Retained for the record.*


`DESIGN:159-163`: "10M asserts+calls … 278.6 ms vs 384.8 ms — ≈53 ns saved per assert."
384.8 − 278.6 = 106.2 ms; over 10M that is **10.6 ns/assert**, not 53 (53 ns would imply ~2M asserts).
The direction of the result is unaffected; the magnitude is wrong by 5×.

### 7.3 Stale prose in `docs/Performance.md`

- `:151` says "IfaceShell (JIT ~193×, AOT ~72×)" and "12.9 ms for 10M asserts"; the table at `:90`
  says **189.18×** / **75.08×** / 13.0 ms. The bullet was not re-synced when the table was
  re-measured.
- `:154-155` says a memoized assert "costs a dictionary hit, an object allocation and a forwarded
  call". The code does **two** dictionary lookups (one always missing), **three** `GetType()` calls,
  ~7 type tests, the allocation, and — on the object tier — an extra `object[1]` plus a boxed return
  inside the forwarded call. Roughly a 2× understatement in operation count.
- `:72` claims the JIT variant is "framework-dependent `Release`"; the artifacts say self-contained
  with dynamic code disabled (§1.4).

### 7.4 `ConversionStrategies-Reference.md:1779`

States that `builtin.TryTypeAssert` "gates on `Cache<TInterface>.Implements` before invoking the
generated `ᴛAs` conversion". `ᴛAs` was retired the same day (`:1785`, `:5773`) and the gate order
changed — `ShellCache` (`builtin.cs:1629`) now precedes `Implements<T>` (`:1641`). Unlike its
neighbours, that paragraph carries no RETIRED/SUPERSEDED marker, so it reads as current.

Per the project's doc rule, whichever stage changes the ladder updates
`ConversionStrategies-Reference.md` in the **same** change; `ConversionStrategies.md` only if the
headline mapping moves (it does not — the emitted form `._<T>(ᐧ)` is unchanged by P0–P6).

---

## 8. Staged implementation plan

Each stage is independently gated and revertible. **All central gates (full behavioral suite, CNR,
corpus build, 43-package sweep) run only AFTER the concurrent Tier-C string-literal session lands** —
until then each stage stops at its filtered guards and its isolated perf A/B.

| Stage | Content | Exit gate |
|---|---|---|
| **0 — Diagnosis (no code)** | (a) Clean one perf project's `bin/`+`obj/`, rebuild `-c Release`, read the runtimeconfig — settles template-property vs polluted-tree. (b) Reconcile the 202/278.6/979.1 ms AOT figures (§7.1) by re-running the AOT A/B with the committed harness. (c) Record both in the design doc. | Both questions answered with a reproducible command; no repo change |
| **1 — P0** | `DynamicCodeSupport` in `csproj-template.xml` (+ perf-tree output isolation, + the `PerformanceRunner` runtimeconfig guard). Re-measure the **whole** perf suite (other reflective rows may move). Fix the `Performance.md` prose deltas §7.3 in the same commit. | Filtered: `run-performance.ps1 --filter IfaceShell --runs 5` A/B. Central: full perf suite + `--update-readme` with the prior table moved to *History*; full behavioral suite; corpus build. Csproj churn reviewed deliberately (~500 files) |
| **2 — P1 + P2** | Unified `Itab<TInterface>` with the registration epoch; delete the dead `GetType()` (`builtin.cs:1571`) and the always-missing tuple probe from the read path (`:1582`); hoist `IsInterface`/`IsValueType` to per-closed-generic statics; monomorphic `Entry` slot. New late-registration behavioral guard. | Filtered: the 17-project interface guard set (§5.3). Central: full suite; **CNR byte-identical** (converter untouched); corpus build; **43-package Phase-4 sweep**; three-point perf A/B (post-P0 → +P1 → +P1+P2) |
| **3 — P6** | Arity-overload dispatch in `GoShellBinding.Invoke`. | Filtered guard set; perf A/B **including the AOT column** (this is the tier AOT cannot escape); full suite + corpus + sweep |
| **4 — P3 (experiment)** | Build the CWT instance cache behind the eligibility rule in the isolation harness. **Adopt only if the post-P0 A/B shows a real win.** If adopted: mutation-visibility + re-assert-equality guards. | Isolation A/B first; then the full Stage-2 gate set |
| **5 — P7 (owner decision)** | Only if the residual is still walk-dominated after Stage 3 **and** the owner accepts a static cache field at every emitted assert site. | Would require full golden re-baseline; out of scope until then |

**Suggested measurement ledger.** One table, appended per stage, with the same three columns the
committed results use (Go / JIT / AOT ms) plus ns-per-iteration and B-per-iteration, so the
attribution in §0 can be re-derived at every step rather than re-argued.

---

## 9. Where this lands, honestly

| configuration | ns/iteration | × Go | source |
|---|---:|---:|---|
| Go | 2.60 | 1.0 | *derived* from `Performance.md:90` (13.0 ms / 5M) |
| shipped today (JIT) | 493.4 | 189.2 | `Performance.md:90` |
| replica, shipped config | 474.7 | ~183 | *measured* |
| **+ P0** | **86.2** | **~33** | *measured* |
| + P1 + P2 | ~56–66 | ~22–25 | **hypothesis** |
| + P6 | ~50–60 | ~19–23 | **hypothesis** |
| + P3, if it wins | ~45–55 | ~17–21 | **hypothesis** |
| + P7, if ever | ~15–25 | ~6–10 | **hypothesis** |
| nominal assert+call today (`builtin.cs:1559`) | 8.3–8.9 per assert | — | *measured* (`E1`) |
| compile-time cast+call / raw C# dispatch floor | 1.10 / 1.36–1.58 per call | — | *measured* (`E2`, `F`) |

The last two rows are the honest frame for the whole exercise: **C# can match Go's ~1.3 ns itab
probe — but only on the nominal path.** Everything in this document narrows the duck-typed fallback
from ~189× toward the low tens; it does not reach parity, because parity requires not constructing a
wrapper, and constructing a wrapper is what makes a structurally-satisfied interface expressible in
C# at all. `docs/Performance.md:148-150` already frames the row that way, and that framing survives
this design intact — it just needs the 5.5× configuration artifact taken out of the number first.

---

## 10. Stage 0 — diagnosis, executed 2026-07-26 (measured)

Both Stage-0 questions are answered. Everything in this section was run on a **clean worktree**
(`claude/r14-iface`, base `eafec005e`, no `bin/`/`obj/` present at the start) on the same
i9-13900K / .NET SDK 9.0.316 / go1.23.1 box the committed table uses. **One other agent's `dotnet`
process was resident**, so the absolute millisecond figures are *provisional*; the ratios between
configurations measured minutes apart on the same tree are not sensitive to that, and the
authoritative full-table re-measure belongs to coordinator integration on a quiet machine.

### 10.1 (a) The configuration defect — **polluted output tree, and the pollution has a root cause**

§1.4 offered two candidate causes and said reading could not distinguish them. Building distinguishes
them decisively, and the answer is **neither of the two as stated** — it is a third mechanism that
*produces* the polluted tree:

| experiment | result |
|---|---|
| clean tree, `dotnet build -c Release` on `PerfIfaceShell` | framework-dependent, **4** DLLs, `IsDynamicCodeSupported: `**`true`** |
| clean tree, `dotnet publish -c Release` (template's `PublishTrimmed=true`, self-contained, 31 DLLs, 26 MB) | `IsDynamicCodeSupported: `**`true`** |
| same tree, after one `run-performance.ps1 --filter IfaceShell` (which publishes AOT) | `bin/Release/net9.0` now holds **187** DLLs, `includedFrameworks`, `IsDynamicCodeSupported: `**`false`**, mtimes = the AOT publish's |

**The template property is exonerated.** `PublishTrimmed` does not disable dynamic code in *any*
configuration reachable without ILC — not on build, not even on a trimmed self-contained publish.
The SDK's `_DynamicCodeSupport=false` default is an ILC-side default, and ILC setting it is correct.

**What actually happens** — confirmed by MSBuild property evaluation, not inference:

```
> dotnet msbuild PerfIfaceShell.csproj -p:Configuration=Release -p:PerfAot=true \
      -getProperty:OutDir -getProperty:OutputPath -getProperty:BaseOutputPath
  OutDir         = bin\Release\net9.0\          <-- pinned by the converter template
  OutputPath     = bin\aot-build\Release\net9.0\ <-- the isolation Directory.Build.props intends
  BaseOutputPath = bin\aot-build\
```

MSBuild copies build outputs to **`$(OutDir)`**, not `$(OutputPath)`. `src/go2cs/csproj-template.xml`
pins `OutDir` unconditionally-except-if-already-set:

```xml
<PropertyGroup Condition="'$(OutDir)'==''">
  <OutDir>bin\$(Configuration)\$(TargetFramework)\</OutDir>
</PropertyGroup>
```

so the `BaseOutputPath` half of the AOT isolation in `src/tests/Performance/Directory.Build.props`
has **never taken effect**. The AOT publish's *build* step therefore writes its self-contained,
dynamic-code-disabled binary straight over the JIT binary in `bin/Release/net9.0/`, and the Measure
phase — which reads `bin\Release\<tfm>\<proj>.exe` (`PerformanceRunner/Program.cs:824`) — times
**that** binary and labels it "JIT".

**The pollution is sticky.** A subsequent JIT-only build does not repair it: re-running with
`--no-aot` on the polluted tree reported **2,514.9 ms (193.77×)** and left the runtimeconfig at
`false`, because MSBuild's incremental check saw the outputs newer than their inputs. That is why
every committed JIT figure for this row is wrong, and why it stayed wrong across re-measures.

**The real magnitude** (clean tree, `--no-aot`, `--runs 5`, so nothing can clobber the binary):

| variant | Go | C# JIT | ratio | runtimeconfig |
|---|---:|---:|---:|---|
| polluted (as shipped) | 13.0 | **2,514.9** | 193.77× | self-contained, dynamic code **off**, 187 DLLs |
| clean | 12.8 | **754.3** | **59.06×** | framework-dependent, dynamic code **on**, 4 DLLs |

**3.33×**, not the 5.5× §4 projected from the isolation replica — 754.3 ms / 5M = **150.9 ns per
iteration**, against the replica's 86.2 ns. The replica was not measuring the whole iteration; the
benchmark's own number is the one to carry forward. **~189× → ~59× is the honest P0 gain.**

### 10.2 The same defect breaks two *other* isolation intents — this is why the fix belongs in the template

`OutDir` outranking `BaseOutputPath` is not a perf-suite quirk. **872** committed csprojs carry the
pin, and the Phase-4 test-host template sets `BaseOutputPath` for a documented reason of its own:

```
> dotnet msbuild go-src-converted/cmp/cmp.tests.csproj -p:Configuration=Debug -getProperty:...
  OutDir         = bin\Debug\net9.0\        <-- production project's tree
  OutputPath     = bin\tests\Debug\net9.0\  <-- what test-csproj-template.xml asks for
  BaseOutputPath = bin\tests\
```

`src/go2cs/test-csproj-template.xml:1-15` spends a full comment paragraph explaining that the test
project and the production project **must not share output roots** (shared `obj\` caused MSB4006).
The `obj\` half of that fix works; the **`bin\` half has never worked** — every converted test host
has been writing into its production package's output directory. And any end-user converted project
that sets `BaseOutputPath` (an artifacts layout, a CI convention) is silently ignored the same way.

So three separately-authored isolation intents are defeated by one line, which settles the layer
question: **fix the templates, not the perf `Directory.Build.props`.** The minimal correct form
preserves the original intent (a stable default path, deferring to an explicit override) and simply
extends "explicit override" to the other knob that determines the same thing:

```xml
<PropertyGroup Condition="'$(OutDir)'=='' AND '$(BaseOutputPath)'==''">
```

With that, no perf-side change is needed at all — `Directory.Build.props` starts doing what its
comment already claims.

### 10.3 **P0-a is withdrawn**

§4's `DynamicCodeSupport` property is a **no-op in every configuration measured above** and would be
speculative machinery bought at ~872 csprojs of churn. It is not implemented. P0 as built is
**P0-b (the template `OutDir` condition) + P0-c (the runner guard)**. The churn is not avoided — the
`OutDir` condition is also a template change — but it is spent on a defect that demonstrably exists
in three places rather than on one that exists in none.

### 10.4 (b) The AOT figures — **there is no discrepancy; one label is wrong**

§7.1 reported an unreconciled 3.5–4.8× gap between 202 ms, 278.6 ms and 979.1 ms. The gap is an
artifact of a **mislabelled iteration count**, and the tell was already sitting in §7.2.

`PerfIfaceShell` runs `run(5000000)` and each iteration adds `3 + 5 = 8`, so its checksum is
**40,000,000** — confirmed by running both committed binaries. The A/B table at
`DESIGN-named-interface-wrappers.md:133` reports checksum **8,000,000**, i.e. **1,000,000
iterations = 2,000,000 asserts** — not the "10M asserts" its own column header claims.

Re-measured today at 1M iterations with the committed harness:

| iterations | Native AOT | ns/iteration |
|---:|---:|---:|
| 5,000,000 (committed row) | 971.2 ms | 194.2 |
| 1,000,000 (the A/B's actual scale) | **206.5 ms** | 206.5 |
| 1,000,000 — as reported at `DESIGN-named-interface-wrappers.md:133` | 202 ms | 202 |

**206.5 vs 202 ms — a 2.2% agreement.** Nothing is unreconciled; the wrappers design's AOT harness
and the committed table have always been measuring the same thing at different scales.

**§7.2 is therefore wrong in the opposite direction, and is withdrawn.** It assumed the "10M
asserts" label and concluded the "≈53 ns saved per assert" claim was 5× too large. With the true
count: (384.8 − 278.6) ms ÷ 2,000,000 asserts = **53.1 ns/assert** — the original figure is
**correct to three significant figures**. The defect is the assert-count label, not the arithmetic.

Owed to `docs/phase4/DESIGN-named-interface-wrappers.md`: relabel the A/B tables' "10M asserts" as
"2M asserts (1M iterations)". Recorded here rather than edited there, because that document is a
banked record of a completed arc.

### 10.5 Measured progression through Stage 3

The ledger §8 asked for, filled in as each stage landed. `PerfIfaceShell` is 5M iterations of two
asserts plus two forwarded calls (one per tier). All figures **provisional** — taken with another
agent resident on the box; the authoritative full-table `--update-readme` re-measure on a quiet
machine is owed at coordinator integration.

| stage | Go (ms) | JIT (ms) | × Go | ns/iter | AOT (ms) | × Go |
|---|---:|---:|---:|---:|---:|---:|
| shipped (clobbered JIT binary) | 13.0 | 2,514.9 | 193.77 | 503.0 | 971.2 | 73.09 |
| **Stage 1 — P0** (output isolation) | 13.1 | **789.8** | **60.43** | 158.0 | 976.5 | 74.72 |
| Stage 2 — +P1 (unified itab) | 13.0 | 683.1 | 52.66 | 136.6 | 866.2 | 66.77 |
| **Stage 2 — +P1+P2** (monomorphic slot) | 12.9 | **633.7** | **49.16** | 126.7 | **760.1** | **58.97** |
| **Stage 3 — +P6** (arity dispatch) | 13.3 | **588.0** | **44.36** | 117.6 | **727.8** | **54.90** |

**193.77× → 44.36× on the JIT row, 73.09× → 54.90× under Native AOT.** Against the design's
hypotheses: P1 delivered 21.4 ns/iteration (predicted 20–30), P2 a further 9.9 (predicted 10–14,
and the predicted overlap with P1 is visible), P6 a further 9.1 on JIT and 6.4 on AOT (predicted
"a couple of ns" on JIT — it is worth more than that because it removes an allocation, not just
work). §9's projected post-P0/P1/P2/P6 floor of ~50–60 ns/iteration was measured against the
replica's 86.2 ns baseline; against the benchmark's own 158.0 ns the same absolute savings land at
**117.6 ns**, so the *shape* of the projection held and only its origin was wrong.

The remaining residual is no longer dominated by the `_<T>` walk — the walk is now a static field
read, an int compare and a reference compare. What is left per iteration is two shell allocations,
one reflective forwarded call with a boxed return, and one delegate forwarded call. That changes
the case for the deferred proposals: **P7's premise ("if the residual is still walk-dominated") is
no longer true**, so its emitted-code footprint buys much less than §6.7 estimated. P3 (the CWT
instance cache) now attacks the largest remaining line, so it is the one worth the experiment.

### 10.6 What Stage 0 changes about the plan

- P0-a **withdrawn** (§10.3); Stage 1 is P0-b + P0-c.
- §0's attribution table is computed from the replica's 86.2 ns post-P0 figure. The benchmark's own
  post-P0 figure is **150.9 ns/iteration**, so the *shares* in §0 stand as a decomposition of the
  replica but the absolute targets in §9 should be read against 150.9, not 86.2. The §9 row
  "**+ P0** | 86.2 | ~33" is superseded by **150.9 | ~59 | *measured on the benchmark***.
- P1/P2/P6's expected gains were sized against the residual *within* 86.2 ns. Against a 150.9 ns
  iteration the same absolute savings are a smaller fraction — they are still worth taking, and
  Stage 2's gate remains the measurement rather than the projection.

---

## 11. The `Iface` row — root-caused 2026-07-26 (measured)

This arc's sibling row, `PerfIface`, was published at **158.24× Go on the JIT and 660.42× under
Native AOT** and marked OPEN in the performance README, which pointed here. It is now explained and
fixed. **It was never about the shells** — the nominal path this document contrasts itself against
was carrying two defects of its own.

**Why no measurement in this arc caught it.** Everything above measures assertions that **resolve**.
The defect lived exclusively on the **MISS** path: golib's ladder ends, below the matching arms, in a
tier answering Go's rule that only *anonymous* struct types convert to each other, so an assertion
that succeeds returns before ever reaching it. §9's `nominal assert+call today` row (8.3–8.9 ns per
assert) is a resolving assert and is unaffected. `PerfIface` asserts `s.(Circle)` against six shapes
of which four are not `Circle`, so two thirds of its assertions took the path nothing else exercised.

**Defect 1 — an uncached custom-attribute read per failed assertion.** That tier asked its question
with `Type.IsDynamicType()`, whose body was a bare `GetCustomAttribute<GoTypeAttribute>()`; attribute
retrieval materializes a fresh attribute instance on every call. Measured against live golib:

| | per call | allocated |
|---|---:|---:|
| JIT | **785.92 ns** | 368 B |
| Native AOT | **3,826.27 ns** | 2,017 B |

At 4 misses per 6 iterations that predicts ~524 ns and ~2,552 ns per benchmark iteration against
measured 505.9 ns (JIT) and 2,111 ns (AOT) — i.e. **it accounts for essentially the whole of both
columns**, to within the difference between an isolated call and the same call inside the loop. The
**AOT column's ~4× penalty over the JIT is that one call's own AOT/JIT ratio (4.87×)** and nothing to
do with generics, shared-generic dictionaries or ILC codegen: ILC parses the attribute blob out of
image metadata per call with no equivalent of the JIT's caching. Fixed by memoizing `IsDynamicType`
per type and hoisting the answer to an `AssertFacts<T>.IsDynamic` per-closed-generic constant, so the
ordinary named-struct miss short-circuits ahead of it.

**Defect 2 — four failing interface type tests per iteration.** `builtin.type()` probed
`IInterfaceAdapter` then `IжAdapter`; `TryTypeAssert` probed the same two. A *failing* interface
`isinst` costs ~2.9 ns on the JIT (the runtime walks the type's interface map) against a failing
sealed-class test too small to measure. Both consumers ask the same question first, so the two
markers now share an empty base, `IGoAdapter`, probed once to gate both tiers. Generated adapters
implement it transitively — `go2cs-gen` is unchanged and no emitted C# moves.

| stage | `PerfIface` JIT | × Go | source |
|---|---:|---:|---|
| published (OPEN) | 10,117.8 ms | 158.24 | full table |
| + defect 1 fixed | 458.1 ms | 7.20 | `--filter PerfIface` A/B |
| + defect 2 fixed | 379.7 ms | 5.95 | `--filter PerfIface` A/B |
| **published (final)** | **370.1 ms** | **5.86** | **full table, quiet machine** |

The Native AOT column moved further: **42,228.2 ms (660.42×) → 262.3 ms (4.15×)**, a 161× improvement,
and **AOT now beats the JIT on this row** — reversing the pre-fix order, because ILC's failing
interface type tests are markedly cheaper than the JIT's (3.73 ns for two, against 9.20 ns) so the
residual that remains is the residual AOT is best at. Peak working set fell 41.3 → 23.1 MB (JIT) and
29.6 → 11.1 MB (AOT), ~4.9 GB of per-assert attribute garbage per run that Go never allocates.
`PerfIfaceShell` held at 44.58× → 44.13× across defect 1 — it is the flatness oracle, and it does not
touch the miss tier — then improved to 40.58× on defect 2, which it shares. `StringMatch`
(9.59× → 9.17×) and `StringView` (2.93× → 2.85×) stayed within run-to-run variance, as did every
other row, which is what attributes the change to the fix rather than to machine state.

### 11.1 The remaining candidate — a base CLASS marker (not taken here)

The largest single component of what remains is the one surviving `IGoAdapter` probe in each of
`builtin.type()` and `TryTypeAssert` — ~2.9 ns apiece on the JIT, ~5.8 ns of a ~19 ns iteration.
Removing it entirely means making the marker something a *failing* test can answer for free, i.e. an
abstract base **class** that every generated adapter derives from rather than an interface it
implements. That is a `go2cs-gen` change (the adapter templates in
`Templates/InterfaceImpl/*` and `Templates/InterfaceType/InterfaceShellEmitter.cs` would gain a base
type), so its gate is the full behavioral suite **and** the 302-package corpus build rather than the
suite alone. It is recorded here as the next candidate, deliberately **not** taken in the same change
as the golib-only fixes above, whose gate is narrower and whose measurement is already banked.

Note also that under Native AOT the interface probes are markedly cheaper than on the JIT (measured:
two failing tests 3.73 ns AOT vs 9.20 ns JIT), so this candidate is a JIT-weighted win.

### 11.2 The same defect class in `GoReflect.cs` — fixed 2026-07-26 (measured)

An audit of every custom-attribute read in golib (`GetCustomAttribute`, `GetCustomAttributes`,
`IsDefined`; positive control: the `IsDynamicType` site above) found **three more uncached per-value
reads**, all in `GoReflect.cs`, all on the reflection bridge's hot surface rather than the
type-assert path:

| site | enclosing | reached uncached from |
|---|---|---|
| `GoReflect.cs:1458` | `TryGoTypeDefinitionKind` → `KindOf` | every `reflect.ValueOf`, `Value.Field`, `Value.Elem`, `MakeSlice`, `MakeMapWithSize`, `rtype.FieldByName`, and `abi.TypeOf` (the outer `KindOf` there is *outside* `s_descriptors`) |
| `GoReflect.cs:349` | `GoQualifiedName` → `GoTypeName` | every `%T` format verb and every `reflect.Type.String()`/`Name()` |
| `GoReflect.cs:870` / `:890` | `wrapperConstructorOf` / `TryUnwrapWrapperValue` | `Value.Set`, `SetMapIndex`, `Call`, `Convert`, the `SetBool/Int/Uint/Float/Complex` family, and `Value.Bool/Complex/String` |

`KindOf` has no cache at all, and its attribute fallback fires for every converted Go struct or named
type — so this was the same defect on a *more* central surface, landing on exactly the packages Phase 4
is working through (`encoding/json`, `encoding/gob`, `text/template`, plus any program that formats
`%T`). By contrast the two reads inside `collectGoFields` (`:1240`, `:1256`) are properly cached behind
`s_goFields.GetOrAdd`, and `runtime/TypeExtensions.cs:115` is a one-time appdomain scan — both fine.

**The fix.** All four reads now route through two per-type memos in `GoReflect` —
`goTypeMarkerOf` and `goLocalNameOf`, `ConcurrentDictionary<Type, …>` over the type's own
`[GoType]` / `[GoLocalName]` attribute. Both markers are `AllowMultiple = false` and have no
subclass, so "the one declared attribute" is the whole answer and the exactly-one list pattern the
call sites already used is preserved verbatim. Like `s_dynamicTypes` above, neither memo is
registered with any assembly-load cache clear: unlike an extension-method scan, a loaded type's own
attributes cannot change. `wrapperConstructorOf` is memoized **whole** rather than just its
`[GoType]` gate, because its answer is one per-type-immutable `ConstructorInfo` and the constructor
scan below the gate was being repeated per call too.

**Measured** on live golib, same harness as §11 (JIT, `DOTNET_TieredCompilation=0`, Stopwatch
best-of-5 over 200 k ops, `GC.GetAllocatedBytesForCurrentThread` on a separate pass), through each
site's **public** entry point so what is timed is what a real caller pays. Two independent
post-fix runs agreed within 4%; the control row is the raw framework API the sites are built on, so
it is unchanged by construction and its ±5% run-to-run spread is the harness's noise floor.

| site (public entry point) | ns before | ns after | B before | B after |
|---|---:|---:|---:|---:|
| *control* — `Type.GetCustomAttributes([GoType])` | 360.85 | 363.66 | 200 | 200 |
| `KindOf` — plain `[GoType]` struct | 430.03 | **55.61** | 200 | **0** |
| `KindOf` — named scalar wrapper | 470.20 | **21.20** | 312 | 40 |
| `KindOf` — `slice<nint>` | 213.65 | **19.30** | 120 | **0** |
| `KindOf` — `ж<T>` box | 165.77 | **35.22** | 72 | **0** |
| `GoTypeName` — plain struct | 131.40 | **46.07** | 128 | 80 |
| `GoTypeName` — lifted `[GoLocalName]` | 493.45 | **45.68** | 368 | 96 |
| `TryUnwrapWrapperValue` — named wrapper | 496.41 | **43.54** | 296 | 24 |
| `TryConvertTo` — `float64` → named wrapper | 557.81 | **50.42** | 448 | 112 |

Two things the table shows that the audit did not predict. The `[GoType]` probe sits **above**
`KindOf`'s container checks, so `slice<T>`, `map<K,V>`, `channel<T>` and `ж<T>` were paying it too —
a *zero-attribute* read still costs ~120 ns and allocates, which is why those rows move as much as
the marker-bearing ones. And the whole of what remains is no longer attribute work: the 40 B on the
named-scalar row is the `def[4..]` substring `TryGoTypeDefinitionKind` cuts from `"num:float64"`, and
the 55.6 ns on the plain-struct row is the five `IsAssignableFrom` container probes *below* the
attribute arm. Both are named here as the next candidates rather than folded into this change, whose
scope was the attribute reads.

**What this does NOT move, measured rather than assumed.** Unlike §11, this fix has no benchmark row
behind it, and the honest reason is that **no row in `src/tests/Performance` reaches `GoReflect` at
all**: the baseline `src/core` touches it from exactly one place — `builtin.GetGoTypeName`, under the
`%T` verb (`core/fmt/format.cs:117`) — and no benchmark formats `%T` (each prints only `%v`-shaped
ints through `fmt.Println`, `PerfIfaceCall` included). An A/B of the whole suite as it then stood
(twelve rows; JIT, `--no-aot`, median of 5) confirms it: every row moved, in **both** directions,
within ±6% — `IfaceShell` 583.7 → 550.3 ms, `String` 733.7 → 773.5 ms — while Go's own columns moved
just as much on the same pair of runs (`Sort` 118.4 → 128.3 ms, `Channel` 47.1 → 54.7 ms). That is
machine noise, and no row is attributable to this change. The sites are on the **full-conversion**
`reflect` bridge, which Phase 4 exercises and the benchmark suite does not.

The nearest real-world instrument available today is a banked test host that does drive the bridge:
`encoding/binary` (137 tests, "Reflection-driven Read/Write — the bridge's construction/write-back
surface"). Its wall time, best of 5 `Debug` runs of the built host, went **370.5 → 357.6 ms (−3.5%)**.
Read that as a **floor**, not as the effect on the bridge: a 137-test host at ~2.6 ms/test is
dominated by process startup and the test harness, and each test makes comparatively few bridge
calls. The per-call table above is the measurement of the defect; this is the measurement of how much
of one package's suite the defect was.

**No behavior changes**, which is why this carries no behavioral test: the answers are identical, so
no Go-vs-C# output can move and no golden can differ. What *is* observable at the golib level is the
allocation — an attribute read allocates and a dictionary hit does not — so the memos are pinned by
**`tests/GolibTests/GoReflectAttributeCacheTests`**, the same place and for the same reason as
`ItabEpochTests`: the paths that can reach zero assert exactly zero, and the paths that must allocate
to do their job (building a name string, boxing a scalar) assert a per-call ceiling set below what
the uncached read cost. Gated on the full behavioral suite plus an operational re-validation of every
banked package (charter §5, golib change class).

---

## 12. P3 — adopted 2026-10-13 (measured on both legs)

§6.4 called P3 *experiment only* and set the bar: adopt only if an A/B after P0 shows a real win. The
experiment ran on `claude/p2-iface-shell-cwt` and met that bar on the JIT leg and on the Native AOT
leg. The coordinator adopted it (a golib-only change with no emitted change) for the first train after
the Go 1.26 hop. §6.4 is left as written; this section is its result.

**What landed.** `AdapterBinder.BuildFactory` wraps the shell factory in `CacheShellInstances`: one
`ConditionalWeakTable<object, object>` per (dynamic type, interface) factory, keyed on the Go dynamic
value, so a re-assert of the same value returns the shell an earlier assert built. Eligibility is the
§6.4 rule, decided once at factory-build time:
- the object shell is always cached (it holds the boxed value by reference);
- a shell over a reference-typed value is cached, whichever tier built it;
- the generic shell over a **value** type is never cached, because its constructor copies the struct;
- a null value (a typed nil func) has no identity to key on, so it is built fresh on every assert.

The experiment carried a measurement switch, `GO2CS_P3_SHELL_CACHE=0`, so that one Native AOT image
could measure both arms. It is **removed** in the seated change: the cache is the behavior, and no
environment variable turns it off.

**JIT leg** (measured 2026-10-12, linux, 4 vCPU VM, Microsoft .NET 10.0.12, `--no-aot`):

| reading | base | cache | delta |
|---|---:|---:|---:|
| allocation, `PerfIfaceShell`, B/iter (in-process probe, 1 M iterations, 3 identical windows) | 96 | 24 | **−72** (prediction −72, met exactly: both shells, 32 + 40 B; the 24 B boxed return remains) |
| benchmark elapsed, median of 20 rounds (order alternated, round 0 dropped), ms | 1201.5 | 1175.7 | −25.8 (−2.1%); cache faster in 14 of 20 paired rounds |
| steady state, ns/iter, best of 15 × 1 M windows after warmup, 16 processes per arm | 144.1 | 137.2 | **−6.9** (prediction −8 to +1, met at its favourable end) |
| the same, on the one switch image (switch off vs on) | 147.1 | 139.3 | −7.8 |
| controls `PerfIface` / `PerfIfaceCall`, ms | 582.3 / 477.3 | 578.2 / 484.5 | −0.7% / +1.5% (no shell on either row) |

An 8-round first try on the switch image read +1.6 ns, the wrong sign. The effect is smaller than this
VM's cross-process spread, and fewer than about 16 rounds could not resolve it.

**Native AOT leg** (measured 2026-10-13 on the i9, one publish of `PerfIfaceShell` at `fe24c9a045`,
both arms from that image by the switch; 22 rounds, order alternated, round 0 dropped): the cache won
**21 of 21** paired rounds, median ratio cache/base **0.773** (**156.3 → 121.9 ns/iter**, −22.7%), with
no overlap between the two arms' ranges. The benchmark's checksum was asserted equal to Go's in every
run. The 15 GB linux box could not read this leg: its publish died in ILC's final phase after 7 h 54 min
("Insufficient memory to continue the execution of the program"); see `DATA-hopN-perf.md`, the
2026-10-13 row.

**Size** (JIT publish, read from the PE section table): `golib.dll`'s `.text` grows by **448 bytes**
with the cache alone (+576 with the switch, which is now removed); the file grows by one 512-byte
block. Nothing else in the publish changes size, and no assembly or dependency is added
(`ConditionalWeakTable` is in CoreLib).

**Guards** (the §6.4 list, cut red-first):
- `src/tests/Behavioral/ShellCacheMutation` (G1): a write through a pointer after its shell exists is
  seen by the old shell and by every re-assert; a second pointer of the same type is never answered by
  the first one's shell; value sources answer with their own contents.
- `src/tests/Behavioral/ShellCacheReassertEquality` (G2): interface equality across re-asserts, for
  pointer and value sources, and against the dynamic value in a plain `any`.
- `src/tests/Behavioral/ShellCacheNilFunc` (G3): a typed nil func asserts ok, beside two non-nil funcs
  of the same type.
- `src/tests/GolibTests/ShellInstanceCacheTests`: a control that the cache is engaged and keyed on the
  value, and a liveness arm that a cached shell does not keep its value alive.

Each guard was made to fail on a fault planted in `CacheShellInstances`, then pass on the real cache:

| planted fault | G1 | G2 | G3 | control | liveness |
|---|:-:|:-:|:-:|:-:|:-:|
| cache keyed on the type only (one shell for every value) | RED | RED | RED | RED | RED |
| no null guard (a nil func reaches the weak table) | pass | pass | RED | pass | pass |
| a strong `ConcurrentDictionary` in place of the weak table | pass | pass | pass | pass | RED |
| the real cache | pass | pass | pass | pass | pass |

Under the type-keyed fault, G1 printed `other box: true 2` against Go's `10` and a sum of 11000
against 20500; G2 printed `true` for two boxes with equal contents; G3 printed 7 for the value Go
prints as 8. G1 and G2 both catch only that fault, but on different observables (a stale value and a
wrong equality), so both are kept.

**Gates on the seated change**: the full GolibTests, the full behavioral corpus, and `go test ./...`
in `src/go2cs`. The results are in the seat post, not here, because they are read at the seat's tip.

**Re-measured 2026-10-20 against the batch-7 tip `0195a29022`** (linux, 4 vCPU VM, Microsoft .NET
10.0.12, JIT, Release; one `/dev/shm` copy of the seat built twice, the base arm with the tip's
`AdapterBinder.cs` and the cache arm with the seat's, which is the seat's only runtime change). Each
process runs 5 warmup windows, 3 allocation windows and 15 timed windows of 1 M iterations; 17 rounds
per arm, order alternated, round 0 dropped. The 2026-10-12 figures above were read at the seat's old
base, so they describe a tree that no longer exists; these replace them for the hop.

| reading | base | cache | delta |
|---|---:|---:|---:|
| allocation, B/iter (all 34 processes, every window) | 96 | 24 | **−72** (prediction −72, met exactly) |
| steady state, ns/iter, median over 16 processes of each one's best window | 146.2 | 141.0 | **−5.3**; cache faster in 12 of 16 paired rounds |
| controls `PerfIface` / `PerfIfaceCall`, median elapsed of 16 runs, ms | 601.8 / 469.4 | 620.3 / 453.8 | +3.1% / −3.3% (no shell on either row; opposite signs, read as this VM's spread) |

Every process's checksum was the expected value. The steady-state delta sits inside the 2026-10-12
prediction (−8 to +1 ns) and below that day's −6.9; the per-arm ranges overlap (base 136.2–160.6,
cache 130.0–157.9), so on this VM the timing gain is smaller than the cross-process
spread, as it was before. The allocation result does not depend on the VM.
