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
