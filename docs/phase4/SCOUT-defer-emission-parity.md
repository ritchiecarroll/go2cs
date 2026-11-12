# SCOUT: can deferring functions read like Go and stay zero-allocation?

> **Status: a scout, 2026-11-12. Docs and scratch probes only**; no converter, golib or generator change. Branch
> `claude/c2-defer-parity-scout` on master `06610aaa65`. Commissioned by the owner: "a scout that estimates the
> viability of using an interceptor type function to keep the zero-allocating try/catch/finally pattern for
> defer/recover operations in generated code for better visual parity on defer type calls." A review follows.

<!-- Evidence: the probes live in lane C2's scratch (not pushed): a console probe referencing this tree's golib
     (each shape as its own method, measured on goroutine-less threads with GC.GetAllocatedBytesForCurrentThread over
     2,000,000 calls, median of 7 runs), a probe generator (three IIncrementalGenerators: call-site interceptors, a
     body copier, a self-interceptor), and a compile-only project whose cases each build under their own
     DefineConstants. Host: linux-x64, 4 cores, .NET SDK 10.0.403, runtime 10.0.14, Roslyn 4.14 (go2cs-gen's
     version), C# 14; Native AOT through ILCompiler 10.0.14. Every number below is read from those runs. The
     predictions were posted before the first probe (mailbox 3b3c1bdab); they are scored in section 8. -->

## 1. The question

Today a Go function that defers emits its frame around the body. `AddCleanupClosesOverPtr`'s `try`:

```go
func try(name string, register func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: panic: %v\n", name, r)
		}
	}()
	register()
	fmt.Printf("%s: registered\n", name)
}
```

```csharp
internal static void @try(@string name, Action register) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: panic: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        register();
        fmt.Printf("%s: registered\n"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

The frame (`GoFrame`, a `ref struct` of four inline defer slots) allocates nothing, which is why it replaced the
old `func<T>((defer, recover) => …)` execution context (DESIGN-closure-emission section 4). The question is
whether the visible C# can read like the Go, ideally `defer(() => {...});` with no frame, `try`, `catch` or
`finally` on the page, without giving up the zero-allocation frame or Go's semantics.

## 2. What any answer must keep

- **0 B per call for the frame.** A deferred call that captures still allocates its own closure today (192 B for
  two capturing defers, section 4.10); what must not come back is a per-call cost for the FRAME.
- **Go's semantics**: LIFO order, `recover` only in a deferred call, re-panic, `Goexit`, defers in a loop, a panic
  inside a deferred call, and named results a defer modifies.
- **The `catch` filter.** `GoFrame.IsPanic` runs in the first pass of exception handling, before any `finally`. It
  is where a panic is adopted and where a foreign exception is preserved before the defers run. A `recover` can
  stop a panic only if a `catch` exists to stop it.
- **The twin path.** G's M:N work twins a function by copying its body text into an `async ValueTask` method
  (DATA-mn-spike-2 section 4, on `claude/g-mn-spike2-b5`). The twin generator refuses ref parameters and ref-like
  types, and the prototype makes `GoFrame` a plain struct (section 3, B1). `claude/g-mn-proto-a0`, named in the
  commission, does not exist on origin; the spike record is what was read. **An option that breaks the twin
  compile path is out.**

## 3. Verdicts

| # | Option | Verdict | Frame cost (JIT / AOT) | The deciding evidence |
|---|---|---|---|---|
| 1a | An interceptor on the `defer(...)` call | **NOT VIABLE** | 0 / 0 B, but it does nothing | The deferred call never runs (`ran 0`) and a panic escapes; an interceptor replaces one call, and nothing runs at the caller's exit |
| 1b | An interceptor on a whole-body wrapper call | **NOT VIABLE** | 88 / 72 B | The body lambda's closure and delegate are built at the call site, before any interceptor runs |
| 2 | Generator split (a visible body, a generated framed wrapper) | **NOT VIABLE** | 0 / 0 B | The body needs the frame by `ref`; its twin is CS1988 (async methods take no ref parameters). Also +1 stack frame |
| 3 | A `using` frame | **NOT VIABLE** | 0 / 0 B | CS1655: a `using` variable cannot be passed by `ref`; with an instance `Defer` instead, the deferred call runs but the panic escapes unrecovered (`recovered 0`): no `catch`, no recovery |
| 4 | Today's shape, compacted | **VIABLE** | 0 / 0 B | IL byte-identical to today's (75 B both); the same 10 sequence points; the twin path unchanged |
| 5 | An ambient frame (a thread-static pointer, `defer(x)` with no `ref`) | **NOT VIABLE** | not measured | Compiles in a blocking method (CS8500 warning). In an async twin it compiles with only CS9123, and the pointer dangles once the state machine moves to the heap |
| 6a | The whole-body lambda `func((ref GoFrame ᒐ) => …)`, plain or intercepted | **NOT VIABLE** | 24 / 72 B with any capture | JIT escape analysis removes the 64 B delegate but never the closure; AOT removes neither. And 6c's costs return |
| 6b | The body copied under `#line`, every CALLER intercepted | **NOT VIABLE** | 0 B direct, 24 / 72 B via a method group | A call that is not a direct invocation in the same compilation runs the visible lambda; and the visible form must still compile, so 6c's costs return |
| 6e | The body copied under `#line`, the visible method's OWN `func` call intercepted, every input a lambda parameter | **VIABLE WITH COST** | 0 / 0 B; a named result written by a defer +32 / +80 B | Value, `ref` and Span parameters at 0 B with Go's results; the cost list in section 6 |

## 4. Measurements

Per call, normal return, a static deferred target that does not recover (so the row reads the frame alone;
`GoFrame.Recovering` allocates a `RecoveringCall` for a recovering defer in every shape alike). The control is an
empty `NoInlining` call.

| Shape | JIT ns | JIT B | AOT ns | AOT B |
|---|---|---|---|---|
| control: an empty call | 3.8 | 0 | 2.8 | 0 |
| 0 today | 26.1 | 0 | 33.2 | 0 |
| 4 today, compacted | 28.1 | 0 | 33.4 | 0 |
| 1a interceptor at the defer | 6.5 | 0 | 5.5 | 0 |
| 1b interceptor on `body(...)` | 36.9 | 88 | 32.6 | 72 |
| 2 generator split | 33.0 | 0 | 30.2 | 0 |
| 3 using frame | 29.7 | 0 | 28.8 | 0 |
| 6a `func(lambda)`, a captured parameter | 38.7 | 24 | 46.2 | 72 |
| 6a intercepted, a captured parameter | 40.9 | 24 | 44.7 | 72 |
| 6a `func(static lambda)` | 33.3 | 0 | 30.0 | 0 |
| 6a a captured local | 40.6 | 24 | 43.7 | 72 |
| 6a a defer in a loop, x3 | 71.3 | 24 | 84.3 | 72 |
| 6b copy, a direct call | 27.5 | 0 | 36.5 | 0 |
| 6b copy, through a method group | 30.9 | 24 | 50.2 | 72 |
| 6e a value and a `ref` parameter | 24.2 | 0 | 30.5 | 0 |
| 6e a Span parameter | 37.1 | 0 | 33.0 | 0 |
| today, a named result a defer increments | 44.3 | 88 | 48.3 | 72 |
| 6e, the same named result | 47.2 | 120 | 57.1 | 152 |

The JIT figures are medians of 7; their ranges overlap for every pair this document calls equal (today 24.0-101.3,
compacted 23.7-35.2). 6a under `DOTNET_JitObjectStackAllocation=0` reads 88 B, which is how the 24 B is known to be
the closure (the delegate is the other 64): with stack allocation on, neither `JitObjectStackAllocationTrackFields`
nor `JitObjectStackAllocationConditionalEscape` moves it.

**Semantics**, the same probe, a recovering deferred call, a panic from the body: today, 4, 2, 6a (all forms), 6b and
6e run the deferred call and recover the panic on both JIT and AOT; 6e's `ref` result reads 1, its Span element 1,
and the named result 2, as Go's do. 1a and 1b never run the deferred call and the panic escapes; 3 runs it and the
panic escapes.

**Compile results** (each case alone):

| Case | Result |
|---|---|
| a `ref` parameter used inside the body lambda | CS1628 |
| a Span parameter used inside it | CS9108 |
| a Span local captured from outside it | CS8175 |
| `goto` out of the body (the named-result exit) | CS0159 |
| `return` of a value through a void body | CS8030 |
| an async body lambda with the frame by `ref` (6a's twin) | CS1988 |
| the split body's twin, frame by `ref` (option 2) | CS1988 |
| today's shape in an async twin, `GoFrame` a ref struct, an await in the body | CS4007 |
| today's shape (and 4's) in an async twin with a plain-struct frame (G's B1) | compiles |
| an ambient frame pointer, blocking (option 5) | compiles, CS8500 |
| the same pointer inside an async twin | compiles, CS9123 only |

**Stack frames** above a callee (`new StackTrace().FrameCount`): today 3 on the JIT (4 under AOT); the split 4 (5).
6e has the same extra frame by construction: the visible method calls its copy.

**Line mapping** (portable PDB sequence points): today's method has 10, on its own lines. The compacted shape has the
same 10, with the frame's on the signature line and the catch, filter and finally on the closing line. 6b's and 6e's
copies carry the copied statements' points in the VISIBLE file, on the visible lines (`V6eʗrun`: lines 209-211 of
the visible file, which hold the lambda's statements), so a breakpoint there binds in the copy. The visible lambda
compiles too and carries points on the same lines, so a breakpoint binds in both and fires in whichever runs.

## 5. Option 4: today's shape, compacted (VIABLE)

The frame opens on the signature line and closes on the last line; the body sits at Go's indentation, one Go line
to one C# line:

```csharp
internal static void @try(@string name, Action register) { GoFrame ᒐ = default; try {
    defer(() => {
        var r = recover(); if (r != nil) {
            fmt.Printf("%s: panic: %v\n"u8, name, r);
        }
    }, ref ᒐ);
    register();
    fmt.Printf("%s: registered\n"u8, name);
} catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } }
```

The named-result form, `func n() (r int) { defer func() { r++ }(); return 1 }`:

```csharp
public static nint n() { nint r = default!; GoFrame ᒐ = default; try {
    defer(() => { r++; }, ref ᒐ);
    r = 1; goto ᒐdone;
} catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); } finally { ᒐ.Run(); } ᒐdone: return r; }
```

- **Cost:** none at run time. The IL is byte-identical (only whitespace moves), so JIT, AOT and every golib
  contract (the `Run` pairing runtime.Callers relies on, `NoInlining`, `DebuggerNonUserCode`) are untouched. The
  twin path is today's.
- **What stays visible:** the frame's first line and last line, and `ref ᒐ` in each `defer`. Nothing else.
- **Debugging:** a breakpoint on the signature line now lands on the frame's initialization, and the closing line
  holds three points (filter, catch, finally); stepping over it runs the deferred calls, as Go's return does.
- **The change:** emission only, in the converter's frame writer (and a nested frame inside a literal the same way).

## 6. Option 6e: the visible lambda, self-intercepted (VIABLE WITH COST)

The owner's option. The visible method keeps the old whole-body shape, with every input passed to a STATIC lambda,
so the lambda captures nothing. go2cs-gen intercepts that `func(...)` call (inside the visible method, so every
caller, a method group or another assembly included, goes through it) and forwards the call's own arguments to a
generated copy of the lambda's statements in today's frame, under `#line` to the visible lines. The lambda never runs.

```csharp
// visible
internal static void @try(@string name, Action register) => func(name, register, static (@string name, Action register, ref GoFrame ᒐ) => {
    defer(() => {
        var r = recover(); if (r != nil) {
            fmt.Printf("%s: panic: %v\n"u8, name, r);
        }
    }, ref ᒐ);
    register();
    fmt.Printf("%s: registered\n"u8, name);
});

// generated (go2cs-gen), what runs
internal static void @tryʗrun(@string name, Action register) {
    GoFrame ᒐ = default;
    try {
#line 2 "main.cs"
        defer(() => { ... }, ref ᒐ);
        register();
        fmt.Printf("%s: registered\n"u8, name);
#line default
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
[InterceptsLocation(1, "…")] static void S0(@string a0, Action a1, Body2<@string, Action> a2) => @tryʗrun(a0, a1);
```

**What it removes.** Captures (0 B for value, `ref` and Span parameters, measured on both JIT and AOT), and with them
CS1628, CS9108 and CS8175: the copy is ordinary method code. `goto` and `return` are the copy's; the visible lambda
needs neither (Go has no `goto` out of a function; the converter's `ᒐdone` exit is a lowering, written only in the
copy).

**The ladder comes back, as stubs.** The visible call must bind to SOME `func` overload that takes those parameters,
one shape per parameter list (value or `ref` at each position, the result type). That is the old ladder the owner
links (`func<TRef1..TRefN, T>(ref TRef1, …, GoFunc<…>.GoRefFunction)`, about 50 overloads at `86566b9ef2`), with two
differences: the overloads never execute (every call is intercepted, so they can throw), and `allows ref struct`
(C# 13) lets a Span be a type argument, so they are generic. Per-position `ref`/value choices make the count
exponential in arity, so the stubs belong in go2cs-gen, emitted per shape a package uses, not in golib.

**The costs.**

1. **A named result a defer writes** cannot be a lambda `ref` parameter, because a deferred closure cannot reach one
   (CS1628). The visible method must capture it, and the capture is built on entry even though the lambda never
   runs: **+32 B per call on the JIT, +80 B under AOT** over today's same function (120 against 88, 152 against 72).
   148 standard-library functions, 28 test functions and 10 behavioral functions use the named-result form today.
2. **One more stack frame** (the visible method, then its copy), as option 2 measures. runtime.Callers' pairing and
   the frame namer (`goFrameName`) must learn the `ʗrun` copy.
3. **The visible lambda's parameter list repeats the signature**: `static (@string name, Action register, ref
   GoFrame ᒐ) =>` on the method line. That is the old shape's look, and noisier than option 4's first line.
4. **Each deferring function's body compiles twice** (the dead lambda and the copy): IL and metadata grow, and a
   breakpoint binds in both.
5. **Twins**: the copy is today's shape, so it twins as today's does once `GoFrame` is a plain struct (compiles). The
   visible lambda cannot be twinned (an async body lambda with the frame by `ref` is CS1988), so the twin generator
   must pair a visible method with its copy and twin the copy. That is a change to G's generator; not built here.
6. **The machinery**: a body copier with `#line`, a self-interceptor per deferring function, generated ladder stubs,
   and a converter that emits every input as a lambda parameter. None of it exists today.

## 7. The corpus footprint (an estimate)

Every deferring function emits one frame. Counted at `06610aaa65` (`GoFrame ᒐ = default;`, with the catch line as a
cross-check; both read the same):

| Population | Frames | Files |
|---|---|---|
| standard library, production code (every per-OS copy counted; windows 126, linux 119, darwin 100 in key folders) | 990 | 322 |
| standard library, converted tests | 1,822 | 340 |
| behavioral goldens | 355 | 166 |
| **all** | **3,167** | **828** |

Of those, the named-result form (results before the `try`, an exit label): 148 production, 28 test, 10 behavioral.

- **Option 4** moves every frame's first and last lines: all 3,167 sites, 828 files (166 goldens re-baselined), with
  no IL change.
- **Option 6e** rewrites every deferring function into the lambda form plus a generated copy: the same 3,167 sites,
  each about twice the IL, and the 186 named-result functions pay the capture.

## 8. The predictions, scored

Posted before the first probe (3b3c1bdab):

- 1, NOT VIABLE: **MET**. The interceptor compiles (go2cs-gen already ships two), the call-site form never runs the
  deferred call, and the whole-body form allocates 88 B (JIT) and 72 B (AOT), inside the predicted 64-160 B.
- 2, VIABLE WITH COST for blocking code and NOT VIABLE under M:N: **MET** (CS1988; +1 frame; 0 B). Scored NOT VIABLE
  overall, because the commission puts the twin path first.
- 3, NOT VIABLE alone: **MET**, by a reason the prediction did not name: `using` variables are read-only (CS1655), so
  the frame cannot be passed by `ref`. With an instance method, the panic escapes, as predicted.
- 4, VIABLE, IL byte-equal and 0 B: **MET**.
- 5, NOT VIABLE under M:N: **MET**, but by a weaker mechanism than predicted. The compiler does not refuse the async
  pointer; it warns (CS9123), so the failure would be silent at run time.
- Recommendation 4: **MET**. 6a, 6b and 6e were added after the predictions (the owner's option 6), so they carry no
  prediction; 6e is the one shape that keeps the old visuals at 0 B for ordinary functions.

## 9. Recommendation

1. **Option 4 now**, if visual parity is wanted: it is the whole Go body at Go's indentation, at zero run-time cost,
   an emission-only change with a measurable footprint (3,167 sites, IL byte-identical). It needs no generator,
   no golib change and nothing from M:N.
2. **6e only if the lambda look matters more than its costs**: the stub ladder, an extra stack frame, double IL,
   +32/+80 B on every named-result function, and a twin-generator change. Option 4 reads closer to Go line for line:
   6e's first line carries the parameter echo and `ref GoFrame ᒐ`, and only the frame's `try`, `catch` and `finally`
   move off the page.
3. **No interceptor-only route exists.** An interceptor replaces a call; zero-allocation frame semantics need a `try`
   with a `catch` and a `finally` in the method that runs, and the only way to keep that method off the page is to
   generate it (6e).

## 10. Amendment, 2026-11-12: the review of `c9376671fb`

A review of this scout agreed with 1a, 1b, 2, 3, 4, 5 and 6b, and with option 4 as the recommendation, and asked for
five changes. Sections 1-9 stand as written above; this block corrects and extends them. The new numbers are lane
C2's, from the same probe and host as section 4, with golib's real `GoFrame`.

### 10.1 Option 6f, missed above: a static whole-body lambda that really runs, through a real ladder

No interceptor and no copy. The visible method passes every input to a STATIC lambda, and a real generic ladder
rung (`allows ref struct`, so a Span is a type argument) holds the frame and invokes the cached delegate:

```csharp
internal static void @try(@string name, Action register) => func(name, register, static (@string name, Action register, ref GoFrame ᒐ) => {
    defer(() => {
        var r = recover(); if (r != nil) {
            fmt.Printf("%s: panic: %v\n"u8, name, r);
        }
    }, ref ᒐ);
    register();
    fmt.Printf("%s: registered\n"u8, name);
});

// golib, one rung of the ladder
public static void func<T1, T2>(T1 a1, T2 a2, L2<T1, T2> body) where T1 : allows ref struct where T2 : allows ref struct
{
    GoFrame ᒐ = default;
    try { body(a1, a2, ref ᒐ); }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

| Shape | JIT ns | JIT B | AOT ns | AOT B |
|---|---|---|---|---|
| control: an empty call | 4.7 | 0 | 2.0 | 0 |
| today (one value parameter) | 32.3 | 0 | 33.3 | 0 |
| 6f, a value and a `ref` parameter | 24.6 | 0 | 33.8 | 0 |
| 6f, a Span parameter | 36.1 | 0 | 37.2 | 0 |
| 6f, a result | 23.9 | 0 | 35.2 | 0 |
| today, a named result a defer increments | 50.8 | 88 | 47.5 | 72 |
| 6f, the same named result | 61.2 | 160 | 67.7 | 128 |

The `ref` result reads 1, the Span element 1, the result 42 and the named result 2, as Go's do; a panic in the body
is recovered by the deferred call on both JIT and AOT. Stack frames above a callee: today 3, 6f 5 on the JIT (the
ladder rung and the lambda); 4 and 5 under AOT. This JIT run's today row is high against its neighbours (the
probe's own noise); the review, on win-x64 with a stand-in frame, read today at 6.7 ns and 6f at 8.8 ns for value
and `ref`, an indirect call's ~2 ns, and this probe does not resolve a difference that small.

**Verdict: VIABLE WITH COST.** Its costs against today:
- a real ladder (section 10.5 measures it: 31 shapes);
- two more stack frames, which runtime.Callers' pairing and the frame namer (`goFrameName`) must collapse;
- a named result a defer writes is captured by the visible method: +72 B JIT, +56 B AOT per call;
- the twin generator must lift the lambda's body into the twin (an async lambda with a `ref` frame is CS1988);
- every input is repeated as a lambda parameter on the method line, and `ref ᒐ` stays on each defer.

**It dominates 6e**: no interceptor, no body copy, no double IL, and the visible code is the code that runs.

### 10.2 Option 6a, restated, and a cite fixed

Section 3's "6a NOT VIABLE" is too broad, and section 4's own "6a `func(static lambda)`" row (0 / 0 B) says so. Restated:
**a 6a body that captures is NOT VIABLE (24 / 72 B); a static 6a body costs 0 B, and that is 6f's mechanism.** Every
Go function with parameters captures unless its inputs become lambda parameters, which is what 6f does.

"6c's costs", cited in sections 3 and 6 and never defined, means the compile results of section 4: CS1628 (a `ref`
parameter used inside the body lambda), CS9108 (a Span parameter), CS8175 (a captured Span local), CS0159 (`goto` out
of the body) and CS8030 (a value returned through a void body).

### 10.3 Option 6e, corrected

- **A real delegate exists at run time.** The visible call site materialises the static lambda's delegate (cached in a
  static field: 64 B once per function, and a null check per call), and the dead body's IL ships. Section 6's
  "the lambda never runs" stands; "no lambda at run time" was not claimed and is not true.
- **Costs section 6 did not list:** step-in passes through the expression-bodied visible method and the interceptor
  stub unless both are hidden from the debugger; a diagnostic on a body line is reported twice (the lambda and the
  copy compile the same text); Hot Reload works only if the generator re-runs.
- **A refinement not probed:** the visible lambda never executes, so a named result could be declared INSIDE it and
  hoisted only in the copy, which would remove 6e's +32 / +80 B.
- **6e is superseded by 6f** (10.1), which reaches the same visible shape without the interceptor, the copy or the
  double IL.

### 10.4 Option 4: its caveats, and a shorter tail

- **Caveats.** The body sits at Go's indentation, line for line. The boilerplate does not disappear: it becomes a
  29-character signature suffix (` { GoFrame ᒐ = default; try {`) and a 123-character closing line, 152 in all.
  `ref ᒐ` stays on every defer. An auto-formatter (`dotnet format`, an IDE's format-document) re-flows the layout
  unless the emitted files are excluded from formatting.
- **A merged filter is WRONG.** `catch (Exception ᒐe) when (Tail.Merged(ᒐe)) { }`, where the filter adopts and parks
  the panic, costs 0 B and passes the plain semantics row. Then a nested case: a panic P1 whose catching frame is
  OUTER, raised under a plain `try`/`finally` whose `finally` calls a function that panics P2 and recovers it.
  Today's frame recovers P1. The merged filter recovers **nil**, with no exception escaping, on JIT and AOT: P1 is
  lost silently. The filter runs in the first pass, before that `finally`; parking the panic there lets the inner
  frame overwrite the parked panic and its claim. That is why `GoFrame.Capture` lives in the catch body.
- **A frame-local tail is right.** `} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }`:
  the filter stores the panic in the frame, the catch body parks it. Today's ordering is kept, the nested case
  recovers P1, the cost is 0 B, and the closing line is 78 characters instead of 123 (107 instead of 152 with the
  signature suffix). It needs two golib members on `GoFrame` (a panic field written by an `Is` instance method, and an
  instance `Capture`), which the probe built as a wrapper struct, so it is a golib change with its own review.

### 10.5 The ladder's real size, measured

Every method-level frame's signature, classified per parameter as value, `ref`, `in`, `out` or Span, with the
receiver marked, plus whether it returns a result (a parser over the emitted files at `06610aaa65`; 1 signature of
2,496 not parsed). A shape is what one ladder rung must match.

| Population | Method frames | Frames in func literals | Shapes | Folded (Span and a ж receiver as value) | All-value | With a `ref`/`in`/`out` position | Max arity |
|---|---|---|---|---|---|---|---|
| standard library, production | 938 | 51 | 40 | 28 | 20 | 8 | 10 |
| standard library, tests | 1,261 | 561 | 21 | 13 | 12 | 1 | 10 |
| behavioral goldens | 297 | 58 | 16 | 10 | 7 | 3 | 3 |
| **union** | | | | **31** | **21** | **10** | 10 |

- The all-value shapes are arities only: void or a result, at 0 to 10 parameters, 22 rungs at most.
- The `ref`-bearing shapes found: `void:R` (5), `ret:R.V.V.V.V` (3), `ret:V.R.V` (2), `void:V.R` (2), and one each of
  `ret:V.R`, `ret:R.V` (2 with behavioral), `ret:rR.V.V.V.V.V`, `void:V.R.V.V.V.V`, `ret:R`, `void:rR.V.V` (`rR` is a
  `this ref T` receiver).
- So the ladder is about 32 rungs for the whole corpus. Section 6's "exponential in arity" is wrong in practice:
  that is the bound, and the corpus uses 10 of its `ref` positions. A generator could still emit them per package,
  but a fixed golib ladder is affordable.
- Frames inside func literals (a literal that defers) would nest: the literal's own body becomes a 6f call, and
  whatever the literal captures becomes that call's arguments.

### 10.6 The recommendation, amended

The owner chooses between two shapes, both at 0 B for an ordinary function:

1. **Option 4**, today's IL with the frame on the first and last lines. Optionally with the frame-local tail (10.4),
   a separate golib change.
2. **A 6f prototype**: the old whole-body look, with the visible code running. It needs a ~32-rung ladder, the two
   extra frames collapsed in runtime.Callers and the frame namer, a twin-generator change, and +72 / +56 B on
   named-result functions (148 production, 28 test and 10 behavioral, section 7).

6e is withdrawn in favour of 6f. The interceptor-only routes (1a, 1b) remain NOT VIABLE.

## 11. Five corpus functions, side by side, and 6f built and run (2026-11-12)

The owner asked for real examples of option 4 and for a 6f prototype that builds and runs. Below are five real
functions, one per shape: a plain defer and recover; a deferred `Unlock` in a pointer-receiver method; a defer inside
a loop; a defer that writes the named results; and a `ref` parameter with a defer. Each comes in four forms: the Go
source; today's emission, verbatim from the committed file; option 4 with the frame-local tail (10.4), laid out as
the converter would write it; and 6f (10.1). Lane C2 measured all of it in one scratch project; nothing on a shared
branch changes.

**The prototype.** A copy of golib at `d6b471d7eb`, with two changes:

- `GoFrame` gains a field and the frame-local tail's two members:

  ```csharp
  private PanicException? m_panic;
  public bool Is(Exception ex) => IsPanic(ex, out m_panic);
  public void Capture() => Capture(m_panic!);
  ```

- A ladder file adds the five rungs these functions need: `void:0`, `void:V`, `void:V.V`, `void:V.R` and `ret:V.V`,
  plus `ret:V` for the control below. Each one is generic with `allows ref struct` and has the shape 10.1 shows.

The harness compiles go2cs-gen as its analyzer, so `Ꮡw.Ꮡmu` and the literal interceptor are the real generated
code. `fmt`, `errors` and `sync` are golib-only stand-ins with the same call shapes: `fmt` takes `any` slots at
fixed arity, and `sync.Mutex` is the uncontended compare-and-swap. `Write`'s `fw` is a small `flateWriter` whose
`Write` counts the bytes. The today column is copied byte for byte from the lines cited. A fourth C# rendering is a
control: option 4's layout with today's tail, whose IL is today's.

**Results equal Go's.** A Go program holding the same five functions (go1.26.10) and the harness's verbose pass run
the same `main`. It calls each function on a normal path and on a panicking path: `try` with a panicking
`register`, `Write` on an open and on a closed writer, `guarded(true)`, and `tryEager` with a nil `e`, which recovers
Go's nil-dereference runtime error. On the JIT and under Native AOT, today, the layout control, option 4 and 6f each
print Go's 23 lines byte for byte. A wrong-on-purpose 6f `guarded` keeps its named results inside the static lambda.
It fails the same comparison on both runtimes, at the one line expected (`0` where Go prints `-1 recovered`): a
recovered panic returns the rung's default, not what the deferred call wrote. That is the rule 6f must follow:
results stay in the visible method whenever a deferred call can recover.

### 11.1 A plain defer and recover: AddCleanupClosesOverPtr's `try`

Go (`src/tests/Behavioral/AddCleanupClosesOverPtr/main.go:47-55`):

```go
func try(name string, register func()) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s: panic: %v\n", name, r)
		}
	}()
	register()
	fmt.Printf("%s: registered\n", name)
}
```

Today, verbatim (`main.cs.target:27-40`):

```csharp
internal static void @try(@string name, Action register) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Printf("%s: panic: %v\n"u8, name, r);
            }
        }, ref ᒐ);
        register();
        fmt.Printf("%s: registered\n"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

Option 4 with the frame-local tail:

```csharp
internal static void @try(@string name, Action register) { GoFrame ᒐ = default; try {
    defer(() => {
        var r = recover(); if (r != nil) {
            fmt.Printf("%s: panic: %v\n"u8, name, r);
        }
    }, ref ᒐ);
    register();
    fmt.Printf("%s: registered\n"u8, name);
} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }
```

6f:

```csharp
internal static void @try(@string name, Action register) => func(name, register, static (@string name, Action register, ref GoFrame ᒐ) => {
    defer(() => {
        var r = recover(); if (r != nil) {
            fmt.Printf("%s: panic: %v\n"u8, name, r);
        }
    }, ref ᒐ);
    register();
    fmt.Printf("%s: registered\n"u8, name);
});
```

**Today:** the Go body sits two levels deeper, inside `try { }`, and the frame's three statements bracket it; the
statements themselves read as Go's. **Option 4:** the body is at Go's indentation, line for line; the frame is a
27-character suffix on the signature line and a 78-character closing line. **6f:** the body is at Go's indentation
too, but the signature line (139 characters) repeats both parameters with their types inside `static (...)`, and a Go
reader has to know that `func(name, register, ...)` is the frame, not a call the Go code makes.

### 11.2 A deferred `Unlock` in a pointer-receiver method: archive/zip's `(*pooledFlateWriter).Write`

Go (`archive/zip/register.go:45-52 (Go 1.26.10)`):

```go
func (w *pooledFlateWriter) Write(p []byte) (n int, err error) {
	w.mu.Lock()
	defer w.mu.Unlock()
	if w.fw == nil {
		return 0, errors.New("Write after Close")
	}
	return w.fw.Write(p)
}
```

Today, verbatim (`src/core/archive/zip/register.cs:50-67`):

```csharp
internal static (nint n, error err) Write(this ж<pooledFlateWriter> Ꮡw, slice<byte> p) {
    nint n = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        defer(Ꮡw.Ꮡmu.Unlock, ref ᒐ);
        if (w.fw == nil) {
            (n, err) = (0, errors.New(writeAfterCloseˢ)); goto ᒐdone;
        }
        (n, err) = w.fw.Write(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}
```

Option 4 with the frame-local tail:

```csharp
internal static (nint n, error err) Write(this ж<pooledFlateWriter> Ꮡw, slice<byte> p) { nint n = default; error err = default; GoFrame ᒐ = default; try {
    ref var w = ref Ꮡw.DerefOrNull();
```

6f:

```csharp
internal static (nint n, error err) Write(this ж<pooledFlateWriter> Ꮡw, slice<byte> p) => func(Ꮡw, p, static (nint n, error err) (ж<pooledFlateWriter> Ꮡw, slice<byte> p, ref GoFrame ᒐ) => {
    ref var w = ref Ꮡw.DerefOrNull();
```

**Today:** the named results become locals declared above the frame, and Go's two `return`s become a tuple assignment
plus `goto ᒐdone`. **Option 4:** the same assignment-plus-`goto`, with the three declarations moved onto a
154-character signature line. **6f:** the closest to Go of the three: both `return`s read as Go's, because no deferred
call here can recover, so the results stay inside the lambda; the price is a 189-character signature line that spells
the result tuple twice and every parameter twice.

### 11.3 A defer inside a loop: DeferLoopCapture's `closureCapture3Clause`

Go (`src/tests/Behavioral/DeferLoopCapture/main.go:160-165`):

```go
func closureCapture3Clause() {
	fmt.Println("-- control: closure capture (3-clause) --")
	for i := 0; i < 3; i++ {
		defer func() { fmt.Println("closure", i) }()
	}
}
```

Today, verbatim (`main.cs.target:176-189`):

```csharp
internal static void closureCapture3Clause() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("-- control: closure capture (3-clause) --");
        for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
            var i = iᴛ1;
            defer(() => {
                fmt.Println("closure", i);
            }, ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

Option 4 with the frame-local tail:

```csharp
internal static void closureCapture3Clause() { GoFrame ᒐ = default; try {
    fmt.Println("-- control: closure capture (3-clause) --");
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        defer(() => {
            fmt.Println("closure", i);
        }, ref ᒐ);
    }
} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }
```

6f:

```csharp
internal static void closureCapture3Clause() => func(static (ref GoFrame ᒐ) => {
    fmt.Println("-- control: closure capture (3-clause) --");
    for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
        var i = iᴛ1;
        defer(() => {
            fmt.Println("closure", i);
        }, ref ᒐ);
    }
});
```

**Today:** two levels of extra nesting around a loop that is already the converter's (`iᴛ1` and the per-iteration `var
i`). **Option 4:** Go's indentation; nothing else changes. **6f:** with no parameters the prefix is short (`=>
func(static (ref GoFrame ᒐ) => {`) and the body reads as option 4's does.

### 11.4 A defer that writes the named results: NamedReturnDefer's `guarded`

Go (`src/tests/Behavioral/NamedReturnDefer/main.go:67-80`):

```go
func guarded(boom bool) (code int, msg string) {
	defer func() {
		if r := recover(); r != nil {
			code = -1
			msg = "recovered"
		}
	}()
	if boom {
		panic("kaboom")
	}
	code = 0
	msg = "ok"
	return code, msg
}
```

Today, verbatim (`main.cs.target:90-111`):

```csharp
internal static (nint code, @string msg) guarded(bool boom) {
    nint code = default;
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                code = -1;
                msg = "recovered";
            }
        }, ref ᒐ);
        if (boom) {
            throw panic("kaboom");
        }
        code = 0;
        msg = "ok";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (code, msg);
}
```

Option 4 with the frame-local tail:

```csharp
internal static (nint code, @string msg) guarded(bool boom) { nint code = default; @string msg = default; GoFrame ᒐ = default; try {
    defer(() => {
        var r = recover(); if (r != nil) {
            code = -1;
            msg = "recovered";
        }
    }, ref ᒐ);
    if (boom) {
        throw panic("kaboom");
    }
    code = 0;
    msg = "ok";
} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } return (code, msg); }
```

6f:

```csharp
internal static (nint code, @string msg) guarded(bool boom) {
    nint code = default;
    @string msg = default;
    func(boom, (bool boom, ref GoFrame ᒐ) => {
        defer(() => {
            var r = recover(); if (r != nil) {
                code = -1;
                msg = "recovered";
            }
        }, ref ᒐ);
        if (boom) {
            throw panic("kaboom");
        }
        code = 0;
        msg = "ok";
    });
    return (code, msg);
}
```

**Today and option 4** read alike: results declared first, returned after the frame. **6f:** the least Go-like and the
most expensive of the five. The results must stay in the visible method, because the deferred call writes them after a
recovered panic, so the lambda captures them and cannot be `static`. It also repeats `boom`, and the visible method is
a declaration, a `func(...)` call and a `return`, which is neither Go's shape nor today's.

### 11.5 A `ref` parameter and a defer: RefLoweredNilTiming's `tryEager`

Go (`src/tests/Behavioral/RefLoweredNilTiming/main.go:76-80`):

```go
func tryEager(label string, e *elem) {
	defer func() { fmt.Println(label, "recovered:", recover()) }()
	eagerFrom(e)
	fmt.Println(label, "no panic")
}
```

Today, verbatim (`main.cs.target:43-54`):

```csharp
internal static void tryEager(@string label, ref elem e) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            fmt.Println(label, "recovered:", recover());
        }, ref ᒐ);
        eagerFrom(ref e);
        fmt.Println(label, "no panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

Option 4 with the frame-local tail:

```csharp
internal static void tryEager(@string label, ref elem e) { GoFrame ᒐ = default; try {
    defer(() => {
        fmt.Println(label, "recovered:", recover());
    }, ref ᒐ);
    eagerFrom(ref e);
    fmt.Println(label, "no panic");
} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } }
```

6f:

```csharp
internal static void tryEager(@string label, ref elem e) => func(label, ref e, static (@string label, ref elem e, ref GoFrame ᒐ) => {
    defer(() => {
        fmt.Println(label, "recovered:", recover());
    }, ref ᒐ);
    eagerFrom(ref e);
    fmt.Println(label, "no panic");
});
```

**Today and option 4:** `e` is `ref elem` and the body forwards it, as the ref-lowering emits it everywhere. **6f:**
`ref elem e` appears twice on the 133-character signature line, and the rung passes the reference through
(`void:V.R`), so a nil `e` still faults in `eagerFrom` and is recovered, as Go's is.

### 11.6 The numbers

The same harness runs all four renderings on linux-x64, .NET 10.0.14, with the call-counting delay a converted
program sets (0). The method:

- Each rendering runs in its own process: 5 processes per rendering, interleaved.
- Each process runs 7 rounds of 2,000,000 calls (10,000 on a panic path), after a warm-up.
- A cell is the median of the 5 per-process medians, with their range in parentheses. B is allocated bytes per call.

Two notes on the method:

- **A single process misleads on the JIT.** Run that way, today's `Write` read 113.7, 123.4 and 109.2 ns in three
  runs, against 77.1, 83.5 and 73.9 for the layout control, whose IL is identical. With tiered compilation off,
  the two read 129.1 and 129.9 ns; with dynamic PGO off, 159.5 and 160.5. So the gap is the order in which the
  renderings tier up and collect profile, not their code. Section 10.1's JIT rows came from a single-process
  harness of the same kind, and this section does not re-read them.
- **One process per rendering removes that effect but keeps the JIT's run-to-run spread.** The IL-identical
  control differs from today by up to ±30 ns on the JIT.

JIT:

| Function, path | today ns | B | option 4 ns | B | 6f ns | B | control (option 4 layout, today's tail) ns | B |
|---|---|---|---|---|---|---|---|---|
| an empty call | 1.7 | 0 | | | | | | |
| 1 try, no panic | 172 (161-193) | 128 | 144 (124-176) | 128 | 170 (148-183) | 128 | 143 (130-150) | 128 |
| 1 try, panic recovered | 5294 (4884-7482) | 1576 | 5663 (4365-7125) | 1576 | 6897 (5390-7436) | 1576 | 6913 (5570-7373) | 1576 |
| 2 Write | 104 (80-116) | 64 | 89 (76-115) | 64 | 81 (77-111) | 64 | 113 (62-122) | 64 |
| 3 closureCapture3Clause | 152 (143-159) | 336 | 154 (150-157) | 336 | 170 (155-178) | 336 | 147 (142-156) | 336 |
| 4 guarded, no panic | 68 (48-71) | 104 | 68 (63-70) | 104 | 87 (86-92) | 169.6 | 70 (59-89) | 104 |
| 4 guarded, panic recovered | 3966 (3474-4894) | 1240 | 4392 (3019-4753) | 1240 | 4844 (2770-5017) | 1305.6 | 4765 (3108-5011) | 1240 |
| 5 tryEager, no panic | 98 (76-111) | 192 | 104 (86-127) | 192 | 112 (78-115) | 192 | 108 (94-114) | 192 |
| 5 tryEager, nil panic recovered | 3252 (2711-4839) | 1416 | 4310 (3339-4636) | 1416 | 4792 (2699-5138) | 1416 | 4663 (3580-4824) | 1416 |

Native AOT:

| Function, path | today ns | B | option 4 ns | B | 6f ns | B | control (option 4 layout, today's tail) ns | B |
|---|---|---|---|---|---|---|---|---|
| an empty call | 1.5 | 0 | | | | | | |
| 1 try, no panic | 132 (110-148) | 112 | 140 (104-158) | 112 | 145 (103-146) | 112 | 142 (106-149) | 112 |
| 1 try, panic recovered | 3813 (2843-3822) | 768 | 3756 (2998-4020) | 768 | 5870 (4705-6009) | 872 | 3434 (3032-3948) | 768 |
| 2 Write | 174 (168-180) | 48 | 172 (122-175) | 48 | 192 (156-195) | 48 | 169 (134-175) | 48 |
| 3 closureCapture3Clause | 123 (122-129) | 288 | 128 (123-134) | 288 | 134 (126-136) | 288 | 129 (114-133) | 288 |
| 4 guarded, no panic | 62 (41-77) | 88 | 63 (46-64) | 88 | 86 (84-87) | 144 | 58 (42-63) | 88 |
| 4 guarded, panic recovered | 1713 (1279-1804) | 640 | 1846 (1331-1907) | 640 | 3964 (2904-4040) | 800 | 1782 (1199-1884) | 640 |
| 5 tryEager, no panic | 96 (81-111) | 176 | 116 (112-117) | 176 | 118 (112-122) | 176 | 106 (82-112) | 176 |
| 5 tryEager, nil panic recovered | 4048 (2888-4223) | 920 | 4068 (3949-4230) | 920 | 6214 (5739-6326) | 1024 | 4009 (2888-4275) | 920 |

### 11.7 The reading

- **B is deterministic, and it settles most of the comparison.** Option 4 with the frame-local tail allocates
  exactly today's bytes in every case on both runtimes, as the control does. 6f allocates today's bytes in every case
  except two.
  - `guarded`: +65.6 B on the JIT (169.6 against 104; 64 B of it is the body delegate) and +56 B under AOT (144
    against 88).
  - Every recovered panic under AOT: +104 B in `try` and `tryEager`, +160 B in `guarded`. On the JIT the same
    paths allocate today's bytes, except `guarded`'s +65.6.
- **Option 4 costs nothing measurable.** Every option 4 cell lies within the spread of today's and of the control's.
  The one cell to re-read is `tryEager`'s normal path under AOT: 116 ns (112-117) against today's 96 ns (81-111).
  The two ranges barely overlap, and the 5 processes do not resolve it.
- **6f costs time on recovered panics under AOT:** 1.5 to 2.3 times today, in all three functions that recover.
  `try` takes 5.9 µs against 3.8, `guarded` 4.0 against 1.7, and `tryEager` 6.2 against 4.0.
  - The rung is generic code that the panic unwinds through, an extra frame; this harness does not attribute the
    cost further.
  - On the JIT the same paths spread 2.7 to 7.5 µs and resolve nothing.
- **6f costs time on three normal paths.** `guarded` reads 87 against 68 ns on the JIT and 86 against 62 under AOT.
  Under AOT, `Write` reads 192 against 174. The `closureCapture3Clause` loop reads 170 against 152 on the JIT.
  Everything else is within the spread.

### 11.8 What this adds to the recommendation

10.6 stands, with three additions measured here:

1. **Option 4 with the frame-local tail** reads as Go does, line for line, on all five shapes, at today's bytes and
   within today's time. Its golib change is the field and two members above.
2. **6f** reads closest to Go where results can live inside the lambda (`Write`'s two `return`s), and furthest from
   Go where they cannot (`guarded`). It costs a 64 B delegate whenever named results stay outside. Under AOT, a
   recovered panic takes 1.5 to 2.3 times as long and allocates 104 to 160 B more.
3. **A 6f converter makes one more decision per function: can any deferred call recover?** Results may move inside
   the lambda only when none can. The converter already answers that for a deferred literal and for a callee it can
   see (it emits `recovers: true`). A deferred func value or interface method must be assumed to recover, which
   keeps the results in the visible method.
