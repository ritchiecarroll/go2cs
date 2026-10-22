# SIZING — the generated ToString's Native AOT cost, and a thinner seam (PROPOSED, post-hop)

> **STATUS: PROPOSED (C1, 2026-10-21). No cut.** COORD ruled the size a POST-HOP item on 2026-10-21: size never gates
> parity, and the behavior is right. This note sizes a thinner seam for the post-hop list. Nothing here is measured
> beyond P1's S0 attribution below. Each claim that rests on a reading rather than a run says so.

## The measured cost

P1's S0 attribution, relayed by COORD on 2026-10-21, at `bb3c751137`, under full-trim Native AOT:

- The program grew by **+11.36 MB**.
- Reverting the generated `ToString` BODIES recovers **11,248,240 B**, about 99% of the growth.
- The forwarder keep in `MethodSetKeep.cs` accounts for about 4 KB, so it stays as it is.

The bodies are:

- every struct's and every inherited type's `ToString`, which call `GoMethodText.Of(this)` and fall back to the
  structural text (`StructTypeTemplate.cs`, `InheritedTypeTemplate.cs`);
- `ж<T>.ToString`, which calls `GoMethodText.OfPointer(this)` and falls back to `PrintPointer()` (`golib/ж.cs`);
- the two interface-shell overrides, which forward to the wrapped value's or box's `ToString`
  (`InterfaceShellEmitter.cs`).

## What these bodies are for, and what they are not for

`GoMethodText` gives a converted value's C# `ToString()` the text Go's `fmt` would print for `%v`: `Error()` before
`String()` across the operand's method set, `%!v(PANIC=…)` for a method that panics, and `<nil>` for a nil pointer
whose method panics. Its callers are **C# callers**: string interpolation, a debugger display, a log line, and a test
assertion message.

**The converted `fmt` never calls `ToString`.** `GoMethodText`'s own remarks say so, and a source read of
`src/core/fmt` at the hop tip finds no `.ToString()` call. `fmt` reaches `Error` and `String` through its own
converted `handleMethods`. So no seam choice below moves what a Go program prints through `fmt`.

`PanicException` reaches a Stringer through the extension registry DIRECTLY, not through `ToString`, so the crash
text Go prints for `panic(v)` does not depend on these bodies either. That is a source reading at
`golib/PanicException.cs`. A census of every other golib site that calls `ToString()` on a Go value for Go-visible
text has NOT been run. It is the first step of option B below.

## Where the bytes most likely go (a reading, NOT measured)

`GoMethodText.Of<T>` and `OfPointer<T>` are generic over the value type. Each instantiation carries its own copy of:

- the binding class, `ValueMethods<T>` or `PointerMethods<T>`, with its static cache;
- `Bound<T, …>` and `Resolve<T, …>`, including the LINQ closures, whose display classes capture `typeof(T)`;
- the delegate binding: `CreateStaticDelegate`, `Delegate.CreateDelegate`, and the `ref T` adapter.

Native AOT shares one canonical body across reference-type instantiations, but **every value-type instantiation gets
its own native code**. Every converted Go struct is a C# struct, and every struct's `ToString` instantiates
`Of<T>` over itself. `ж<T>.ToString` adds an `OfPointer<T>` instantiation for every boxed value type. So the
resolver is plausibly compiled once per struct type, which would make the cost scale with the program's type count
rather than with the formatting machinery.

The alternative COORD named is that the bodies keep the Go-text formatting machinery reachable. That would be a
roughly fixed cost, paid once per program, and would not fit 11 MB as well as a per-type cost does. Neither reading
is measured. The measurement that decides between them is in [Measuring it](#measuring-it).

## Option A — a non-generic seam with the same behavior (recommended first)

Keep every behavior. Remove the per-type instantiation:

- The generated `ToString` becomes `global::go.GoMethodText.OfBoxed(this, typeof(T)) ?? <structural text>`. It boxes
  only on the ToString call, which is never a hot path. `ж<T>.ToString` passes the box and its pointee type to the
  same entry.
- One shared, NON-generic resolver, keyed by `Type` in a dictionary that `TypeCacheGeneration` invalidates as today.
  It finds the method with the same `Error`-then-`String`, by-kind, value-or-pointer-set rule over
  `GetGoMethodSetCandidates`, and calls it through `MethodInfo.Invoke`, boxing the receiver. A `ref T` receiver is
  passed boxed, and a `ж<T>` receiver is passed as the box.
- `TargetInvocationException` is unwrapped, so a panicking method still prints `%!v(PANIC=<method> method: …)`, and a
  nil pointer still prints `<nil>`.

**What is lost:**

- **Speed**: a reflection invoke and a boxing per call, against today's cached typed delegate. That is microseconds on
  a debugger display or log line, and nothing on `fmt`'s path.
- **One allocation per ToString** for a value type: the box.

**Risks to check at the cut:**

- `MethodInfo.Invoke` under Native AOT needs the invoked method's reflection metadata. The method-set scan already
  needs that metadata. `MethodSetKeep`'s `DynamicDependency` hangs on the generated `ToString`, which option A keeps,
  so no new keep is expected. Whether a kept method is also INVOKABLE, not only enumerable, under full trim is not
  known from a reading. The full-trim AOT assertion arms must prove it.
- A copy-bound `this ref T` value forwarder must still bind through a copy, as `IsCopyBoundReceiver` decides today.

**Prediction to score:** most of the 11.25 MB is recovered with no behavior change, if the per-type reading is right.
If it recovers little, the fixed-cost reading is right, and option B is the remaining lever.

## Option B — a hook that trimming can drop (COORD's shape), and what it costs

The generated `ToString` asks a static hook for the method text, and falls back to the structural text when the hook
is unset. Whatever registers the hook keeps the machinery, and a program without it trims the machinery away.

**Who registers it matters, and `fmt` is the wrong registrar:**

- `fmt` never calls `ToString`, so registering the hook there ties a C#-side display feature to an unrelated Go
  import.
- Nearly every Go program imports `fmt`, so the hook would almost always be set, and the trim would almost never
  happen.

**The trimmer-recognized form is a feature switch.** A `static bool` guarded by `[FeatureSwitchDefinition]`, set by
an MSBuild property (`RuntimeHostConfigurationOption` with `Trim="true"`), lets ILC remove the guarded branch when the
switch is off. Off would be opt-in for a size-sensitive AOT publish. On would stay the default for every build.

**Is it honest against Go's `%v`?** Yes for Go's own output: `fmt` is unaffected either way, as stated above. It is a
divergence on the C# surface only, and it must be stated where the property is documented. With the switch off:

- a C# `ToString()` of a converted value prints its FIELDS (`{a b}`) or its underlying value instead of its
  `Error()` or `String()`. That is the text before `GoMethodText`, which printed `2000000000` for a `time.Duration`
  where Go prints `2s`.
- a debugger display, a log line and an interpolation all show that structural text.
- a test assertion message that interpolates a converted value changes. That is visible only to C# test code.

**Open before B:** the census above, of every golib site that uses `ToString()` for Go-visible text. Any such site
would make the switch observable to Go output. That site would then need a direct call, as `PanicException` already
makes, before the switch could ship.

## Measuring it

These are P1's S0 arms, with one axis each, read at one tip under full-trim Native AOT:

1. **Today**: the baseline.
2. **Option A**: the non-generic seam.
3. **Bodies reverted**: P1's existing arm B, the floor.
4. **The per-type reading, tested directly**: a program with N ≈ 50 and N ≈ 500 struct types, each with a
   `ToString`, measured with and without the generic call. The slope per type decides between the two readings
   before any cut.

The ILC `.mstat` of arm 1 also answers this without any arm. It reports how many `GoMethodText` instantiations were
compiled and their total size.

## Size

- **Option A**: about one lane-day. It covers the resolver, the template change for two templates plus `ж<T>`, the
  GolibTests arms (`GoMethodText`'s existing tests, plus a boxed-receiver arm and a panic-unwrap arm), the four AOT
  arms above, and the corpus footprint, which is generator-only, with no converter emission change.
- **Option B**: about half a lane-day on top of option A, plus the golib census. It is not proposed unless option A
  leaves most of the cost in place.

Not attempted before the hop, as ruled.
