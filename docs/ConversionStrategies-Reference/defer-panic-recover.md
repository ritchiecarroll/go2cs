# Defer / Panic / Recover

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#defer--panic--recover)

## A function that defers or recovers emits its body INLINE, inside a frame

A Go function is a stack frame: it registers `defer` records in its own frame and runs them on the
way out, whatever the way out is. The converted C# says the same thing with statements — the body is
emitted **inline** in the method, wrapped in `try`/`catch`/`finally`, beside a `GoFrame` local that
holds this call's defer list and nothing else:

<!-- source: src/tests/Behavioral/DeferSimple/DeferSimple.cs:10-19 -->
```csharp
internal static void Main() {
    GoFrame ᒐ = default;
    try {
        fmt.Println("Open file");
        defer(ᴛ1 => fmt.Println(ᴛ1), closeFileˢ, ref ᒐ);
        fmt.Println("Write data to file");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}
```

Each part does one of the three things Go's runtime does for free:

- the **`catch`** parks a panic — an explicit `panic()` or a .NET exception that maps to a Go runtime
  panic — where `recover()` can read it. The filter is the single adoption point that also snapshots
  the panic's origin, so a non-panic exception (and `GoexitException`, deliberately) fails it and
  propagates unchanged;
- the **`finally`** drains the deferred calls, which is Go's guarantee that they run on *every* exit
  path — normal return, panic, `runtime.Goexit`, or a mapped runtime fault. They run **after** the
  panic has been parked, which is exactly what lets a deferred call recover the panic raised by the
  body it was registered in;
- the **frame** is the defer list. It is a `ref struct`, so it lives in the method's own stack frame,
  the JIT can enregister its four inline slots, and the machinery allocates **nothing**.

`GoFrame.Run()` is the whole of the ordering contract: LIFO drain, the `HandledPanic` save/restore
that keeps a traceback honest for the length of the deferred sequence, the re-panic origin
inheritance behind Go's `defer func(){ panic(recover()) }()` idiom, and the final re-throw of a panic
no deferred call recovered.

Guards: `DeferSimple`, `DeferCallOrder`, `DeferClosure`, `PanicRecover`, `GoexitDefers`,
`DeferFrameScopes`, and golib's `GoFrameTests` (which A/B every scenario against the machinery this
replaced).

## Why the body is not a lambda

The obvious alternative models the same three things as an *object* that owns the body — a
`func((defer, recover) => …)` execution context supplying a catch, a finally and a `Stack<Action>`.
Owning the body forces the body to be a delegate; a delegate forces a display class for everything
the body touches; and a display class forces a generic ladder for everything a delegate *cannot*
capture (a `ref` local, a `Span`). The frame form needs none of it, and avoids two things beyond the
machinery:

- **a capture-semantics divergence class, by construction.** A body-owning lambda closes over
  variables the Go original never closed over; an inline body closes over nothing at all, and only
  the deferred closures capture — exactly as Go's deferred closures do.
- **the ref-parameter ladder.** A variadic deferring function would have to thread its `params Span`
  through the wrapper, because a lambda cannot capture one. An inline body has no parameters to
  thread, so it simply uses the one it has (`GenericVariadicFunc`, `VariadicPointerParam`).

Measured with `GC.GetAllocatedBytesForCurrentThread` over 5,000 Release calls, the frame allocates
**nothing** with no defers and **nothing** with one or two whose targets are cached static method groups;
for the two-capturing-defer shape of `internal/poll.FD.Write` the only allocation is the
display class and delegate of each defer that genuinely closes over something. The body-owning
alternative allocates in all three shapes.
<!-- The full as-built record of the frame's design lives in
     [`phase4/DESIGN-closure-emission.md`](../phase4/DESIGN-closure-emission.md) (internal record, unlinked 2026-10-26, OQ-1). -->
<!-- measured at the design (platform not recorded): frame 0 B / 0 B / 192 B; body-owning alternative 160 B / 248 B / 440 B. -->

## The named-result form: results outside the try, exits through a label

`func f() (r int) { defer func(){ r++ }(); return 1 }` returns **2** in Go: the deferred call runs
after the result parameter is assigned and before the caller sees it. A C# `finally` cannot change a
value the `return` has already evaluated, so the results are declared **before** the `try` and read
back **after** the `finally`, and every exit inside the `try` leaves through a `goto` — which runs the
`finally` exactly as a `return` would, without freezing a result the deferred calls may still change:

<!-- source: src/tests/Behavioral/NamedReturnDefer/main.cs:73-90 -->
```csharp
internal static (nint @out, @string label) compute(nint x) {
    nint @out = default;
    @string label = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            @out += 1000;
        }, ref ᒐ);
        if (x < 0) {
            (@out, label) = (-1, "neg");
            goto ᒐdone;
        }
        (@out, label) = (@double(x), fmt.Sprintf("v=%d"u8, x));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (@out, label);
}
```

The label is emitted only when something jumps to it — a body that simply falls off the end of the
`try` reaches the return anyway. A heap-box-backed named result keeps the split it already had: the
box is declared outside the `try` and the value alias re-derived inside it, because the deferred
closures are still lambdas and a lambda cannot capture a `ref` local.

Guards: `NamedReturnDefer`, `NamedResultDeferCapture`, `DeferFrameScopes` (which exits from inside a
loop, a switch arm and an `if`).

## The catch arm returns Go's zero results

For a value-returning function whose results are unnamed, the catch arm ends with `return default!;`.
A panic a deferred call **recovered** leaves the function returning the zero results, which is Go's
rule; one **no** deferred call recovered never reaches that return at all, because `Run()` re-throws
it from the `finally` and a throw from a `finally` overrides a pending return. It is also what keeps
the method's endpoint unreachable, so nothing is needed after the `try` statement.

## `recover()` is a static call, not a parameter

`recover()` resolves to `builtin.recover()`, which reads a thread-local slot: the panic the deferred
sequence now running on the thread is handling, which `GoFrame.Run` sets from the panic the emitted
`catch` parked (`GoFrame.Capture`). That it resolves **statically** is not an
optimization — it is the load-bearing fact of the whole design: a deferred closure can therefore
recover without holding any handle on the frame that registered it, which is what allows the frame to
be a `ref struct` in the first place.

One consequence: `recover` is a Go *predeclared identifier*, not a keyword, so a package may declare
its own — `text/template/parse` has `func (t *Tree) recover(errp *error)`. Inside that package's
class the extension method wins over the `using static go.builtin` import, so such a package emits
its built-in calls qualified as `builtin.recover()`, exactly like every other shadowed built-in, and the
[direct-call forms](#only-the-function-the-defer-calls-directly-recovers) the same way
(`builtin.recover(calledByDefer)`).

## `defer` registration: the arity ladder, and no bang

Go evaluates a deferred call's ARGUMENTS at the `defer` statement and runs the call later, so every
argument is captured at registration. `defer` is an arity ladder of generic rungs (`Action` and a
result-discarding `Func` twin per arity, 1 through 16, plus a nullary rung) whose last required
parameter is `ref GoFrame` — the frame to register into (an optional `recovers` flag follows it; see
[below](#only-the-function-the-defer-calls-directly-recovers)):

<!-- illustration: not converter output -->
```csharp
defer(Ꮡfd.writeUnlock, ref ᒐ);                      //  Go: defer fd.writeUnlock()
defer(ᴛ1 => fmt.Println(ᴛ1), closeFileˢ, ref ᒐ);    //  Go: defer fmt.Println("Close file")
```

The rungs are generic rather than `params object[]` so a value argument is captured without boxing on
a path that runs on function exit throughout the corpus.

The registration reads as plain `defer` because nothing else in scope claims that name: `defer` is a
Go **keyword**, so no Go identifier can ever be spelled that way, and no C# keyword collides. Its
siblings carry a `ǃ` (U+01C3, a legal C# identifier character where `!` is not) for reasons of their
own: `goǃ` cannot be `go`, which is the root namespace every converted file sits in, and `makeǃ`
cannot be `make`, which is a predeclared Go identifier a package may shadow.

## A deferred RECEIVER-FIELD call with no arguments is lowered INTO the frame's `finally`

A deferred call on a field of the receiver — `defer c.mu.Unlock()` — costs a ж-box per call under the
registration form above, because the registration has to hold the receiver and holding `c.mu` means
boxing it (`Ꮡc.of(counter.Ꮡmu)`, a `FieldRefBox<T>`). The call already runs on every exit path, and a
C# `finally` already runs on every exit path, so where the two provably coincide the call is emitted
directly into the frame's `finally` and nothing is allocated at all. For this method:

<!-- source: src/tests/Behavioral/DeferFinallyLowering/main.go:36-42 -->
```go
func (x *box) two() {
	x.a.touch()
	x.b.touch()
	defer x.a.done()
	defer x.b.done()
	fmt.Println("body two")
}
```

converts to:

<!-- source: src/tests/Behavioral/DeferFinallyLowering/main.cs.target:24-39 -->
```csharp
internal static void two(this ж<box> Ꮡx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var x = ref Ꮡx.DerefOrNull();

        x.a.touch();
        x.b.touch();
        ᒐd1 = true;
        ᒐd2 = true;
        fmt.Println("body two");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd2) Ꮡx.DerefOrNull().b.done(); if (ᒐd1) Ꮡx.DerefOrNull().a.done(); ᒐ.Run(); }
}
```

Three properties of Go's `defer` are what the shape has to preserve, and each one is a gate rather
than an argument:

- **Go binds the receiver at REGISTRATION; a `finally` binds it at unwind.** So the receiver name must
  not be reassigned or addressed anywhere in the body. `defer c.mu.Unlock(); c = other` unlocks the
  ORIGINAL `c`'s mutex in Go and would unlock the NEW one here — no compile error, no panic, just the
  wrong object. Arguments are excluded for the same reason: capturing one is the box again.
- **A defer that was never REACHED never runs.** Each site carries a `bool` set at the defer's own
  source position, so the `finally` calls only what the body actually reached. The flag is emitted even
  for a defer that is the body's first statement, because the function preamble sits inside the `try`.
- **Go's defers are LIFO**, which for unconditional top-level defers is reverse source order — the
  order the `finally` emits them in, ahead of `ᒐ.Run()`, since `Run()` re-throws an unrecovered panic
  and would otherwise leave them unexecuted.

The receiver is re-rooted on its box (`Ꮡx.DerefOrNull()`) rather than on the entry deref alias `x`,
which is declared inside the `try` and is not in scope in the `finally`. That allocates nothing: the box
is the method's own receiver parameter.

A fourth gate has no counterpart in the emission and exists purely because the measurement found it:
**some UNCONDITIONAL statement ahead of the defer must already dereference the same prefix.** Go
evaluates `c.mu` AT the defer, so a nil `c` panics there and registers nothing; a lowered `finally`
evaluates it at exit, so the body would run to completion first and the panic would surface late with
the body's effects already committed. The dominant `c.mu.Lock(); defer c.mu.Unlock()` idiom satisfies
it trivially — but a few sites in the standard library have no earlier dereference at all,
and a few more have one only inside an `if`, a loop or a func literal, which witnesses nothing
because that branch may not have run. Both are refused. The conditional half is the sharper of the two:
the first form of this gate scanned preceding statements without asking whether they execute, and
accepted every one of them.
<!-- census at the time, over Go 1.23.12's standard library: three sites with no earlier dereference, four with one
     only inside a branch. -->

Everything else keeps the registration form, including any function that mixes a qualifying defer with
a non-qualifying one: the lowering is all-or-nothing per function, because interleaving a lowered LIFO
order with a registered one is not expressible.

**Two widenings, each a gate that was stricter than correctness required.** The first cut admitted
only defers that were direct children of the body, and only calls on a receiver *field*. Both were
sizing proxies carried into the emission unexamined, and together they refused the very function the
capability exists to reach — `internal/poll`'s `FD.Write`, whose `defer fd.writeUnlock()` is a method on
the receiver itself and whose `defer fd.l.Unlock()` sits inside `if fd.isFile`, each failing a
different gate so that all-or-nothing rejected the pair.

- A **conditional** defer — nested in an `if`, a `switch` or a block, but not a loop and not a function
  literal — lowers behind the same flag, set at its own source position. The LIFO argument never
  needed the defers to be unconditional, only control to flow *forward*: with no loops and no backward
  jumps (`goto`/labels, measured at zero sites) the CFG is a DAG that visits structured statements in
  source order, so registration order is source order restricted to the reached defers, and the flag
  makes an unreached one a no-op. The prefix gate then asks about the defer's **own** scope, walking
  out through enclosing blocks and counting an `if`/`switch` INIT and CONDITION — which always execute
  when the statement is reached — while refusing their bodies; `if err := fd.writeLock(); err != nil`
  is the dominant Go spelling of exactly the dereference the gate looks for.
- A method on the **receiver itself** (`defer fd.writeUnlock()`) lowers too. Its registration allocates
  a delegate rather than a `FieldRefBox` — the receiver's box is the method's own parameter — so the
  saving is smaller, but refusing the shape is what disqualified every function pairing one with a
  field defer. One sub-class the census cannot see is an emission property: a *promoted* method on an
  embedded field (`onceError` embedding `sync.Mutex`, `defer a.Unlock()`) is reached through
  `Ꮡa.of(onceError.ᏑMutex)`, so lowering moves that box into the `finally` rather than removing it and
  saves only the delegate. Correct, and measured as a minority of the lowered calls on the windows
  corpus; removing the box there needs an aliasing field pointer, a different capability.

Most such sites in the standard library qualify, including conditional and receiver-method ones.
The census runs the converter's own `provablyBefore` verbatim, so the two predicates cannot drift.
Guarded by `tests/Behavioral/DeferFinallyLowering`, whose refusal rows are asserted by the golden (a
refusal is invisible in stdout), whose `rebound` row fails loudly if the receiver-stability gate is
removed, and whose `writeShape` row — `FD.Write`'s exact shape — prints a spurious `done b` for the
untaken branch if the reached-flag is removed.
<!-- census at the time, over Go 1.23.12's standard library:
     332 sites, 225 qualifying (10 conditional, 60 receiver-method); promoted-method lowerings were ten of sixty-five
     lowered calls on the windows corpus. -->

## A VARIADIC deferred/spawned func literal is cast to its golib family delegate

A deferred func LITERAL is normally handed to the arity rung directly — `defer((nint cnt) => { … },
count, ref ᒐ)` — because a non-variadic literal emits explicitly-typed parameters that convert to
`Action<T1, …>`, from which the rung's type arguments infer. A **variadic** literal emits
`params ꓸꓸꓸ@string dirsʗp` and converts to *nothing*: it is neither an `Action<…>` (so `defer<T1, T2>`
cannot infer — **CS0411**) nor a method group (so the nullary rung's `Action` slot rejects it), and it
cannot even be invoked where it stands (**CS0149**). C# 13 does give it a natural type, but that is a
compiler-synthesized `<>f__AnonymousDelegate<N>`, unrelated to anything golib declares.

golib already has the right type — the `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` family whose tail is a
`params Span<TArg>` — and `iifeDelegateType` already renders it from a signature, because the
immediately-invoked-literal path needs the same thing. So a variadic literal callee is CAST to its
family delegate and then invoked, exactly the `((<delegate>)(<lambda>))(<args>)` shape a non-variadic
IIFE already uses, and the registration takes the temp-parameter form so there is something to invoke. For
`defer func(dirs ...string) { for _, dir := range dirs { os.RemoveAll(dir) } }(dir1, dir2)`:

<!-- source: src/core/html/template/examplefiles_test.cs:117-122 -->
```csharp
defer((ᴛ1, ᴛ2) => ((Actionꓸꓸꓸ<@string>)((params ꓸꓸꓸstring dirsʗp) => {
    var dirs = dirsʗp.sslice();
    foreach (var (_, dir) in dirs) {
        os.RemoveAll(dir);
    }
}))(ᴛ1, ᴛ2), dir1, dir2, ref ᒐ);
```

Go's defer-TIME argument evaluation is untouched: `dir1`/`dir2` are still the eager arguments the rung
snapshots, and `ᴛ1`/`ᴛ2` are what the thunk receives back at unwind. A **result**-returning literal
takes the `Funcꓸꓸꓸ` half and the rung discards the result, as every value-returning deferred callee
does. The **nullary** form additionally suppresses the method-group trim: `defer f()` normally emits
the callee alone (`defer(Ꮡfd.writeUnlock, ref ᒐ)`), but trimming `((Actionꓸꓸꓸ<nint>)(<literal>))()`
back to the cast delegate hands the `Action` rung a family delegate (**CS1503**), so the invocation is
kept and wrapped — `defer(() => ((Actionꓸꓸꓸ<nint>)(<literal>))(), ref ᒐ)`. `go` takes all of the same
arms for the same reasons.

**Reach.** The shape is `html/template`'s `examplefiles_test.go:90`, which was that package's last
build wall. The same cast
also closes the immediately-invoked variadic literal (`func(parts ...int) int { … }(1, 2, 3)`), which
`convCallExpr`'s IIFE interception had explicitly excluded on the stale reasoning that "delegate type
would need a params array" — `iifeDelegateType` has rendered the family form for a variadic signature
all along. <!-- measured on the Go 1.23 tree: that build wall stood in front of 243 verdicts;
     GOROOT/src/html/template/examplefiles_test.go:90 holds the shape at go1.26.10 too. -->
(Guarded by `DeferLambdaParam`, extended from its one non-variadic row to cover the
variadic literal at every arity around the shape: no fixed parameter with one argument, a fixed
parameter ahead of the tail, none at all, a result-returning literal, and the immediately-invoked
form — plus the defer-time snapshot itself, whose arguments are reassigned after the `defer` and must
not change what the thunk prints. Counter-proven failing-first by neutering each half separately: the
temp-parameter force alone gives CS0411 ×4, the delegate cast alone gives CS0149/CS1503 ×6.)

**An adjacent wall this deliberately does NOT close, measured here and not caused by it:**

**A SPREAD argument to any deferred variadic call.** `defer f(nums...)` emits `nums.ꓸꓸꓸ`, a
`Span<T>`, as the type argument of `defer<T>` — and C# forbids a ref struct there (**CS9244**).
Proven independent: a NAMED variadic callee with no func literal anywhere emits the identical
`defer(ᴛ1 => f(ᴛ1), nums.ꓸꓸꓸ, ref ᒐ)` and fails the same way. Closing it means passing the SLICE
and spreading inside the thunk, at every variadic deferred call in the corpus.

An empty variadic call is NOT such a wall: `f()` on `func f(parts ...int)` answers `parts == nil` as `true`, as in
Go, because the variadic pack's `Span<T>.slice()` keeps a null span nil.
<!-- src/core/golib/slice.cs:1878 `if (IsNilSpan(source))`. history: this was listed here as a second wall (an
     empty slice where Go passes nil); golib's Span-to-slice copy has since kept nil-ness,
     slice.cs:1848 `NIL-NESS CROSSES THIS BOUNDARY`. -->

## A ZERO-ARG deferred/spawned call of a NAMED variadic callee keeps the lambda — the group carries `params`

The named-callee sibling of the literal cast above (`defer Reset()` on `func Reset(sig ...os.Signal)`, from
os/signal's tests). <!-- found blocking os/signal's test-host compile, 2026-08-27: the same variadic-binding family as
the C#14 params-flip fix. --> The zero-argument arm of `visitDeferStmt`/`visitGoStmt` trims `f()` back
to the method group `f` so golib's arity-0 `defer`/`goǃ` take it as an `Action` — valid only when the
callee's C# arity is genuinely zero. A variadic callee's C# form **always** carries the `params`
parameter, so its method group converts to no `Action` (defer) and no `WaitCallback` (go) —
**CS1503** at both statements, measured as the failing-first red of the guard below. The with-args
forms were never exposed: `getFunctionParamCount` answers `-1` for a variadic signature, which
already forces the temp-parameter ladder.

The guard is signature-level (`types.Signature.Variadic()`) in both statements' zero-arg arms, and it
covers the pointer-receiver **box method group** too — a variadic method's box overload carries the
same `params` parameter (`defer c.bump()` on `func (c *counter) bump(deltas ...int)`). The emission
keeps the invocation and wraps it: `defer(() => Reset(), ref ᒐ)`. Wrapping a zero-operand call
disturbs no defer-time evaluation — there are no operands to evaluate. Emission-inert corpus-wide by
construction: an existing zero-arg variadic defer/go site would have been a compile error, and the
corpus compiles. Guarded by `DeferVariadicCallee` (both statements, both arities, plain func and
pointer-receiver method, output-compared vs `go run`).

## `defer f(g())` spreads a MULTI-VALUE call: the tuple is the eager argument, the thunk expands it

Go lets a call whose arguments come entirely from one multi-value call omit the intermediate
variables — `f(g())` passes `g`'s results as `f`'s parameters, and the language permits it *only*
when that call is the sole argument. Under `defer`/`go` the two halves of the semantics split:
`g()` is evaluated **at the statement**, on the current goroutine, and `f` runs later (at unwind,
or on the new goroutine) with the results `g` produced back then.

The eager half was already right — argument capture happens in exactly one place, the argument-list
renderer, which hands the eager expression to the registration and substitutes a temp parameter
into the thunk body. What was missing is the expansion: `len(Call.Args)` is **1** for this shape,
so one `ᴛ1` marker went to a callee wanting N parameters.

<!-- illustration: not converter output -->
```csharp
defer(ᴛ1 => show(ᴛ1), two(), ref ᒐ);   // BEFORE — two() is (int, string), show takes both: CS7036
```

C# has no splat, and it needs none: the tuple `g()` returns is a perfectly good single eager
argument. The arity-1 rung captures it at exactly Go's moment and exactly once, and the thunk
spreads its components when the call actually runs:

```go
func two() (int, string)              //  Go:
defer show(two())                     //      two() runs at the defer; show runs at unwind
```

<!-- source: src/tests/Behavioral/DeferMultiValueSpread/DeferMultiValueSpread.cs.target:66 -->
```csharp
defer(ᴛ1 => show(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
```

`Item1…ItemN` are `System.ValueTuple`'s own fields, so NAMED Go results (`(n int, s string)`, which
emit a named C# tuple) address identically — the element names are compiler aliases, never a
replacement. Hoisting the results into statement-time locals instead would also be correct, but it
buys nothing and costs a name: the thunk's parameters are already `ᴛ1…ᴛN`, so the hoisted temps
would collide with them in the enclosing scope (CS0136).

**Three pieces, each independently red-proven.** The component-wise substitution
(`convExprList`) is the fix proper. The **temp-parameter force** in `visitDeferStmt`/`visitGoStmt`
matters only for a FUNC-LITERAL callee — an ordinary callee already takes that form from the
existing arity test (one argument against N>1 declared parameters), but a literal reaches neither
that test nor the variadic one (CS0411 without it). And a non-variadic **func-literal callee** is
the one defer/go shape rendered as an INVOCATION rather than handed to the rung as a delegate, so
it additionally needs the immediately-invoked-literal delegate cast that `convCallExpr`'s intercept declines for
every other defer/go callee (CS0149 without it):

<!-- illustration: not converter output -->
```csharp
defer(ᴛ1 => ((Action<nint, @string>)((nint n, @string s) => { … }))(ᴛ1.Item1, ᴛ1.Item2), two(), ref ᒐ);
```

**Reach.** No production source used it when this was measured — the idiom is a TEST one, which is where it was
found: Go's own save-and-restore hook `defer reflect.SetArgRegs(reflect.SetArgRegs(a, b, c))`
(three results into three parameters, the callee returning values so the thunk binds the
result-discarding `Func` rung) appears four times in reflect's tests, `abi_test.go` ×3 and `all_test.go` ×1.
<!-- At the fix those four sites were four of the reflect test host's errors, and a converted-test emission diff over
     that host moved exactly those four lines and nothing else. -->
(Guarded by `DeferMultiValueSpread`: arity 2 and 3, the
capture-not-re-read case, LIFO ordering, a pointer-receiver method callee, a per-iteration loop, a
func-literal callee, a variadic callee, and the result-returning save/restore shape — plus two
CONTROLS that must keep their existing emission, plain matching-arity arguments and a
SINGLE-value call as the sole argument, both of which stay bare method groups. `go` mirrors every
arm and is covered by two of its own.)

## `defer panic(v)` captures its value at the defer, and the sequence survives it

`panic` is the one built-in emitted as a `throw` **statement** rather than as a call, and that made it
the one built-in the deferred-argument machinery could not see. Argument capture happens in exactly one
place — the argument-list renderer substitutes a temp parameter (`ᴛN`) into the thunk body and hands the
eager expression to the registration — and the `panic` arm returns `throw panic(<expr>)` before reaching
it. So the thunk inlined the ORIGINAL expression and the registration's argument slot was left empty:

<!-- illustration: not converter output -->
```csharp
defer(ᴛ1 => throw panic(errΔ2), , ref ᒐ);   // CS0839: Argument missing
```

The arm now performs the substitution itself, so the value is evaluated at the `defer` and thrown from
the thunk's parameter:

```go
err := fmt.Errorf("first")
defer panic(err)
err = fmt.Errorf("second")     // Go recovers "first" — arguments evaluate at the defer
```

<!-- source: src/core/go/types/check_test.cs:209 -->
```csharp
defer(ᴛ1 => throw panic(ᴛ1), err, ref ᒐ);
```

Capturing the expression in the thunk body instead — dropping the parameter — also compiles, and is
**wrong** for exactly the shape above: it would report whatever the variable held when the frame
unwound. The same rule reaches `go panic(v)`; `visitGoStmt` now forces the temp-param form for a
built-in callee just as `visitDeferStmt` does, which also repairs `go close(ch)` (a built-in's method
group is generic with `in` parameters and never converted to `Action<T>` — CS1503).

Compiling was only half of it. A deferred `panic` is also the smallest case of a panic raised **by** a
deferred call, and `GoFrame.Run` treated that as the end of the sequence: the panic escaped the loop
and the frame's remaining deferred calls never ran, so the `recover()` thunk registered *before* the
`panic` thunk never saw it. Go continues the sequence — the new panic joins the one unwinding and
becomes what a later `recover()` answers — so `Run` now parks the raised panic where `recover()` reads
it and keeps going, re-raising it at the end only if nothing recovered it. The catch filter is `IsPanic`,
matching the emitted frame's own catch, so a runtime fault in a deferred call is recoverable exactly as
one in the body is; `GoexitException` deliberately fails that filter and still unwinds.

Guarded by `DeferPanicArg`, which output-compares six shapes against `go run`: a plain value, an error
variable reassigned after the defer, a computed expression, a pointer value round-tripping the `any`
boundary and answering a type assertion, a deferred panic replacing one already in flight, and two
deferred panics in one frame (Go keeps the LAST one to run, i.e. the FIRST registered).

## The re-raise of an unrecovered panic belongs to the frame that CAUGHT it, not to the thread

`GoFrame.Run` ends by re-raising a panic no deferred call recovered. The panic itself is parked in a
thread slot (`GoThreadState.CapturedPanic`, reached as `GoFuncRoot.CapturedPanicValue`), so the obvious tail — *if the
slot is non-empty, throw it* — reads correctly and is wrong, because the slot is the THREAD's and
`Run` is a FRAME's. It stays non-empty for the whole of the panicking frame's deferred sequence, and
every ordinary function that sequence calls runs its own `Run` from its own `finally`. Each of those
callees caught nothing; each of them found a panic parked; each of them threw it. The caller's
deferred cleanup was therefore abandoned at whatever statement happened to follow the first callee
that had a `defer` of its own — with no diagnostic, because a panic escaping a deferred sequence is
exactly what is supposed to happen next.

Go has no such rule. A panic resumes unwinding when the frame that is panicking has finished its
deferred calls; a function *called* during that sequence returns normally, runs its own defers, and
resumes the caller.

So the claim is made explicit rather than inferred. The emitted `catch` body already calls
`GoFrame.Capture`, and a catch body and its `finally` are adjacent — nothing runs between them — so
`Capture` ARMS a claim that the next `Run` on the thread CLAIMS, and that next `Run` is always the
same frame's. A frame that caught nothing claims null and leaves the in-flight panic alone. The
emission is unchanged: this is entirely inside golib.

<!-- illustration: adapted, owed a real excerpt -->
```csharp
public void Run()
{
    GoThreadState state = GoThreadState.Current;
    PanicException? owned = GoFuncRoot.ClaimPanic(state);   // null unless MY catch just captured
    …                                                       // deferred calls, LIFO
    if (owned is { Recovered: false })
        throw owned;
    …                                                       // a foreign exception's own rethrow
}
```

Two cases keep the rule from becoming a swallow. A panic raised by **this** frame's own deferred call
becomes owned even though nothing was claimed on entry (`owned = raised` in the sequence's catch), so
`defer func(){ panic(v) }()` still escapes a frame that was never panicking; and a callee that panics
while an outer panic is unwinding still replaces it, which is Go's own behaviour.

Measured on `database/sql`'s `TestConnRaw`. `Conn.Raw`'s deferred cleanup calls `release` →
`closemuRUnlockCondReleaseConn` → `Conn.close`, and `close` reaches `c.dc = nil` only after
`dc.releaseConn` → `db.putConn` → `dc.Close` → `finalClose` → `withLock` — a two-line helper holding
one `defer` and panicking nothing, whose `Run` threw the callback's panic on the way out. The
connection was left open, and the test's five-second `waitCondition` poll (which sizes itself from
`t.Deadline()`) burned the package's ENTIRE deadline, which read as a
hang rather than as the assertion failure it was. With the rule in place its suite finishes in seconds.
<!-- measured at the fix (platform not recorded): 3,418 s against Go's 0.005 s before, about three seconds after;
     the package validated at 137 of 139. -->

Guarded twice, both neuter-verified. `PanicDeferCalleeFrame` output-compares the shape against
`go run` — the reduced acquire/cleanup/release chain, three deferring callees stacked below one
deferred call, and the two negatives. `GolibTests.GoFrameTests` pins it at the frame:
`AFrameCalledFromADeferredCallDoesNotReRaiseTheOuterPanic`,
`AFrameWithNoDefersCalledDuringAPanicDoesNotReRaiseItEither` (the `m_count == 0` path, which skips
the sequence entirely and reaches the tail directly),
`APanicRaisedByADeferredCallStillEscapesAFrameThatCaughtNothing` and
`APanicRaisedInsideADeferredCleanupReplacesTheOneUnwinding`.

**The adjacent shared-slot case is closed too.** The tail re-raises the OWNED panic unless that panic
itself was recovered (`owned is { Recovered: false }`), rather than reading the thread's captured-panic
slot, which a nested panic overwrites and a nested `recover()` clears (runtime's `TestIssue43920` and
`TestIssue43921`). And what a `recover()` may stop is the panic of the deferred sequence running on the
thread: `GoFrame.Run` sets a per-thread `RecoverablePanic` to the panic it is handling (null for a
normal-return sequence) and restores the outer value on exit, so a `recover()` in the defers of a
deferred function's own normal return answers nil, as Go's does (runtime's `TestRecoverMatching`).

## Only the function the defer calls DIRECTLY recovers

Go's `recover()` stops a panic only when it is called directly by the deferred function. Called from a helper
that the deferred function calls, it returns nil and the panic goes on. Whether a function IS the deferred
function depends on how it was called, not where it was written: `defer catchError(&err)` recovers, while
`defer func() { catchError(&err) }()` reads nil in `catchError` and the panic continues. So the decision is made at
run time, and the converter marks its two ends:

<!-- source: src/tests/Behavioral/RecoverInHelper/RecoverInHelper.cs.target:76-83 -->
```csharp
internal static void catchError(ж<error> Ꮡerr) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
        ref var err = ref Ꮡerr.DerefOrNull();

        {
            var e = recover(calledByDefer); if (e != nil) {
```

<!-- source: src/tests/Behavioral/RecoverInHelper/RecoverInHelper.cs.target:98 -->
```csharp
        defer(catchError, Ꮡerr, ref ᒐ, recovers: true);
```

- A named function whose body calls `recover()` takes golib's mark as its first line, and its recover calls pass
  what it took. `recover(false)` returns nil and leaves the panic running.
- A defer whose callee may be such a function registers with `recovers: true`. golib then sets the mark on the
  thread just before the call (`GoFrame.Recovering`) and clears it after. A converter thunk between the
  registration and the function (`ᴛ1 => catchError(ref ᴛ1.DerefOrNull())`) is transparent, because the
  function's own first line is where the mark is taken. A frame with such a defer is never
  [lowered into its `finally`](#a-deferred-receiver-field-call-with-no-arguments-is-lowered-into-the-frames-finally),
  where nothing would set the mark.
- The deferred literal itself (`defer func() { … recover() … }()`) is direct by construction and keeps
  `recover()`.
- A literal that is NOT the deferred literal (it reaches a defer through a func value) calls `recoverChecked()`.
  That reads the calling frame from the stack, and only while a panic is waiting to be recovered.
- `defer recover()` defers recover itself, which no deferred function calls, so it reads
  `recover(calledByDefer: false)` and the panic continues, as in Go.

**Stated limits.**
- **Exported functions keep `recover()`.** An EXPORTED function that calls `recover()` emits plain `recover()`. Another
  package may defer it, and that defer site cannot know to set the mark, so the function cannot tell a deferred
  call from a helper call. It recovers in both, a false true where Go would read nil. Package `main` exports
  nothing and is exempt.
- **Dynamic callees are matched by signature.** A defer whose callee is a func value, a func-typed field or an
  interface method is marked only when its signature matches a recovering function of the package (for an
  interface method, a recovering method of the same name). A matched callee that does not itself recover leaves
  the mark for the first recovering function it calls, which reads it only while a panic is waiting.
- **Unexported recoverers that escape their package are not marked.** An unexported recovering function handed
  to another package as a func value and deferred there is not marked at that defer, so it reads nil.
<!-- The shared-slot case above is closed: GoFrame.cs and GoFuncRoot.cs cite runtime's TestIssue43920, TestIssue43921 and TestRecoverMatching. The direct-call rule was NOT closed then: builtin.cs at 2b51b02d50 said `Known divergence, accepted: a recover() in a HELPER`. An earlier comment here implied both were resolved. Probe 2026-10-22, Go 1.26.10 on linux/amd64: `func helper() any { return recover() }` called from `defer func() { … helper() … }()` in a panicking function, under an outer deferred recover, printed `outer: boom` in Go (the helper read nil).
     CLOSED 2026-10-22 by the recover-in-helper seat (COORD ruling, Option B): the "One divergence is accepted" paragraph that stood here is replaced by the section above. Guard: src/tests/Behavioral/RecoverInHelper, Go-compared, arms 1-7 (helper reads nil; direct; defer f(&err) and the gob ref-lowered thunk; method value with and without args; defer recover(); a named recoverer called normally from the deferred literal; a func value). Red at hop tip 2b51b02d50 on arms 1, 5 and 6; green under Release, Release TC0 and Debug. Plants, each restored byte-identical: no mark (arm 3 onward panics), TakeDeferredCall always true (arms 1 and 6), recover(bool) ignoring its argument (arms 1, 5 and 6). -->

## A nested defer scope gets a frame of its own

A `ref struct` cannot be captured by a lambda, and it does not need to be: a function literal that
defers carries its own frame inside the lambda (or local function) it emits as.

Every frame reads under the **same name**, at every nesting depth. A C# lambda or local function may
declare a local spelled like one in the enclosing method — the pre-C# 8 CS0136 rule does not fire —
and the inner declaration precedes every inner use, so an inner `GoFrame ᒐ = default;` simply shadows
the outer one. The one name that *cannot* repeat is the named-result exit **label**: labels do not
shadow (CS0158, *"the label shadows another label by the same name in a contained scope"*), so
`ᒐdone` alone carries a nesting-depth suffix. Both facts were settled by compiling the shapes rather
than reasoning about them.

That is also what makes a **deferred literal that defers on its own account** expressible.
`defer func(){ defer cleanup(); … }()` scopes the inner `defer` to the literal in Go; the literal is
a deferred-call target, so it gets no *recover* scope of its own (its `recover()` recovers the
enclosing function — the whole point of the idiom), but it does get a frame for its own defers, and
the inner registration lands there rather than in the enclosing function's. Guarded by
`DeferFrameScopes`.

## An unrecovered panic crashes the process Go-style: report on stderr, exit code 2
`throw panic(x)` unwinds until some enclosing frame's deferred sequence recovers it. When nothing
does — including in a **goroutine**, since `goǃ` runs the body at the root of a goroutine thread (golib
reuses idle ones) with no frame around it, and a frame's exception filter only adopts panic-convertible
exceptions — the exception reaches golib's `AppDomain.UnhandledException` backstop (registered in
`builtin.InitializeGoLib`). The backstop matches Go: it writes the report to **stderr** and terminates
with **exit code 2**. The report is the one Go prints (golib's `CrashReport`):
- the panic chain, as Go's `printpanics` writes it: every panic still linked beneath the one that escaped, oldest
  first, each later one on a tab-indented line, with ` [recovered]` on a panic a `recover()` stopped and
  ` [recovered, repanicked]` where a recovered value was panicked again. Each line reads `panic: <value>`; a
  runtime-error exception is first mapped through `RuntimeErrorPanic.TryAsPanic`, so an integer divide by zero
  reports Go's `panic: runtime error: …` form;
- a blank line, the `goroutine N [running]:` header, and the traceback with Go's frame names and Go source
  positions. A frame the JIT inlined away is put back for an explicit `panic`
  ([how](../ConversionStrategies.md#a-partial-method-with-a-body-keeps-its-own-frame)). In a few shapes the
  traceback is shorter than Go's
  ([Limitations](../Limitations.md#what-converts-but-doesnt-behave-exactly-like-go)).
<!-- src/core/golib/runtime/CrashReport.cs:192 `oldest first, each after the first on a TAB, with <c> [recovered]</c>`;
     :275 `text.Append(repanicked[i] ? " [recovered, repanicked]" : " [recovered]");`. The traceback renderer is the
     converted runtime's (src/core/runtime/managed_impl.cs:171 `CrashReport.TracebackRenderer = crashTraceback;`).
     Guards: GolibTests CrashReportTests and CrashReportChainTests. Design record: docs/phase4/DESIGN-crash-report.md.
     history: the backstop once printed to stdout and called Environment.Exit(0), which polluted compared output and
     signaled false success to every caller; later it printed the first line alone, without the traceback. -->

The behavioral output-comparison harness validates this
differentially: the Go binary is the oracle, so exit codes must **match** (not be zero), stdout must
match, and the **first stderr line** must match — the remainder of Go's panic stderr is a
machine-specific goroutine stack trace, so only the first line is compared. (Guarded by the
`GoroutinePanicExitCode` behavioral test — a goroutine panics unrecovered while `main` blocks on a
channel receive; both binaries must exit 2 with `panic: goroutine boom` as the first stderr line and a
clean stdout.)

**A `main` that returns while a goroutine is panicking still gets the report.** `defer wg.Done(); panic(v)`
wakes `wg.Wait()` from inside the panicking goroutine's deferred calls, so `main` returns while that
goroutine has not reported yet. Go's `runtime.main` does not exit then. It waits while any goroutine runs a
panic's deferred calls (`runningPanicDefers`), and once a report has begun (`panicking`) it waits for the
report, so the program exits 2 with it (runtime/proc.go, issues 3934 and 20018).

A converted `main` is the CLR entry point and goroutines run on background threads, so left alone the runtime would
shut down under the panicking goroutine: exit 0 with an empty stderr. golib's `MainExit` runs Go's tail from
`AppDomain.ProcessExit`, which the CLR raises when `Main` returns:
- **The count.** A goroutine counts from the moment a panic starts running its deferred calls
  (`GoFrame.Capture`). It stops counting when its frame returns with no panic in flight (a recovery), when
  the goroutine ends, or when its fatal report begins.
- **The wait.** `main`'s return waits while the count is non-zero, for at most 20 ms. That is as short as Go's
  1000 yields in spirit: it covers the goroutine's rethrow to its root and no more.
  <!-- src/core/golib/runtime/MainExit.cs:44 `which takes 1.2 to 1.8 ms from <c>GoFrame.Capture</c> (Release, 30 runs`
       (the platform measured is not recorded); the bound
       :72 `internal static readonly TimeSpan PanicDefersWait = TimeSpan.FromMilliseconds(20);` -->
  A goroutine that is still running a slow recovering call, or blocks in a deferred call, is not waited for, and
  neither does Go wait for one. If a report has begun, `main` waits for the report and exits with its status.
- **Which report** (Go's `paniclk`): the first fatal start owns it. When `main` panics while a goroutine's crash
  is already under way, `main`'s backstop waits for the goroutine's report, which then ends the process, so the
  first line is the goroutine's, as in Go. Once the owner has reported, the waiting thread parks for good and never
  prints a second report; only an owner that never reports (60 s) lets it print its own.
- **An exit taken on purpose** (`os.Exit`, a fatal error, the backstop) goes through `MainExit.Exit` and owns its
  status, as Go's `exit` never consults the count.

(Guarded by the `GoroutinePanicAfterDeferredDone` behavioral test, which must exit 2 with Go's report, and by
`GoroutineRecoveringWhenMainReturns`, which prints nothing after `main` returns. GolibTests `MainExitPanicDefersTests`
pins the count and the bounded wait.)

## A panic VALUE renders through Go's `preprintpanics` rule — an `error` prints its message, not its address

The report above is only as useful as the value in it, and rendering that value is a rule of its own.
Go's runtime does not print the panic value directly: `preprintpanics` (runtime/panic.go) SUBSTITUTES
first — an `error` panic value becomes its `Error()`, a `Stringer` its `String()` — and only then is
the result printed. `PanicException` rendered `state?.ToString()`, so a converted `panic(err)` whose
value is a pointer-held error printed its ADDRESS:

```
panic: 0x211163e3340          // was
panic: open final.txt: code 13 // Go, and now
```

That is not a cosmetic divergence: a traceback exists to carry exactly the information the address
destroys. <!-- it cost a diagnostic round-trip on text/template's goodFunc rejection, whose message had to be
recovered by instrumenting the callee. --> The rule lives in `PanicException.PanicText` and both readers of a panic
value go through it (as `PanicException.Message`) — the unhandled-exception backstop above, and `debug.Stack`'s panic
line in `runtime/managed_impl.cs`.

**The `Stringer` arm is not redundant with `ToString()`.** A Go named type's generated `ToString()`
forwards to its UNDERLYING value (go2cs-gen's `InheritedTypeTemplate`), so `panic(2 * time.Second)`
would print `2000000000` where Go prints `2s`. The method is found the way golib's `error<T>` finds
`Error` — through the extension-method registry, which is where a converted Go method lives — and
the receiver shape is re-checked before the call, since the registry's precedence comparer can hand
back a `ж<T>`-declared method for a value receiver.

**Computed on first READ, not at construction**, because that is when Go computes it: `preprintpanics`
runs only once a panic has gone unrecovered and is about to print. A RECOVERED panic — `fmt`'s
catchPanic, `text/template`'s errRecover, every `defer func(){ recover() }()` in the corpus — must
therefore never call a user `Error()`/`String()` at all, which an eager render would do on every
panic in the corpus. `recover()` still hands back the value itself: the substitution is a PRINTING
rule, not a value rewrite. Go throws a fatal `"panic while printing panic value"` when the
substitution itself panics; reproducing the FATALITY from a `Message` getter would be worse than the
divergence it reports, so the text is returned instead. (Guarded by the `PanicValueRendering`
behavioral test — an unrecovered `panic(err)` whose first stderr line is compared against `go run`,
over a stdout half proving the recovered path is unchanged — plus `PanicValueTextTests` for the arms
one process cannot reach: Stringer, the failure text, and the laziness rule.)

## An IMPLICIT divide-by-zero panics with the runtime's OWN value, so it satisfies `runtime.Error`

Go's compiler lowers an integer division to a zero check plus `runtime.panicdivide()`, which panics
with `runtime.divideError` — a value whose dynamic type is the unexported `runtime.errorString`, and
which therefore satisfies the `runtime.Error` interface. go2cs instead lets the CLR raise
`DivideByZeroException` and maps it to a Go panic at the recover boundary
(`RuntimeErrorPanic.TryAsPanic`), so the panic carried only the message TEXT. That is invisible to
code which merely prints the recovered value, but it fails a type assertion — math/bits'
`TestDiv32PanicZero` asserts exactly that:

```go
} else if e, ok := err.(runtime.Error); !ok || e.Error() != divZeroError {
```

`Div32` divides without an explicit zero guard (unlike `Div`/`Div64`, which `panic(divideError)`
themselves), so its panic came from the hardware trap: `ok` was false, `e` was nil, and the test's
`e.Error()` then NRE'd.

golib sits UNDER the converted `runtime` package and so cannot name `divideError`, so the dependency
is **inverted**: golib exposes `RuntimeErrorPanic.IntegerDivideByZeroValue` and the runtime package
registers its own canonical value through a `[ModuleInitializer]` in the hand-owned
`runtime/panicvalues_impl.cs` bridge. An implicit (trapped) divide-by-zero then carries the *same*
value an explicit `panicdivide()` would — which is precisely Go's own invariant — and
`err.(runtime.Error)` resolves through the generated `errorString`→`ΔError` adapter. A converted
program that never links `runtime` keeps the plain-message fallback, which reads and prints
identically and loses only the assertion.

(The `DivideByZeroPanic` behavioral test prints the recovered value, which reads the same on either
path; the `runtime.Error`-typed path is guarded by math/bits' own Go test suite, `TestDiv32PanicZero`,
run through the converted test pipeline.)
<!-- history: the typed path was untestable in a behavioral test while those built against a src/core without a
     runtime package; src/core/runtime/panicvalues_impl.cs is in the one tree now, but no behavioral test asserts
     err.(runtime.Error). -->

## `make([]T, len[, cap])` out-of-range panics are RECOVERABLE, with Go's messages

Go's `makeslice` panics recoverably for a negative or over-allocatable length/capacity — the
recovered value's text is `runtime error: makeslice: len out of range` (or `cap`; probed vs
`go run` — the recovered value is a `runtime.errorString`). golib's make path (the
`slice<T>(nint length, nint capacity, nint low)` constructor) raised
`ArgumentOutOfRangeException`/`OverflowException` for the same inputs — .NET exceptions
`recover()` cannot catch, so a deferred recover never ran and the process died. The constructor
validates first and throws `RuntimeErrorPanic.MakeSliceLenOutOfRange()` /
`MakeSliceCapOutOfRange()` (recoverable `PanicException`s carrying Go's message text), using
`Array.MaxLength` as .NET's `maxAlloc` equivalent. The same validation class applies to the
hand-owned `internal/bytealg.MakeNoZero` (`bytealg_impl.cs`) — Go's runtime implementation of it
panics `len out of range` before allocating, and strings/bytes `TestRepeatCatchesOverflow`
recovers that panic and matches on `"out of range"` (`strings.Repeat` of a
near-`maxInt` product reaches `MakeNoZero` after passing Repeat's own overflow pre-checks).
Like the established golib runtime-panic convention, the panic STATE is the message string, not
an `error` value — a recovering type switch takes Go's `case error:` arm only in Go; both sides
converge on the same `err.Error()` text through the `fmt.Errorf("%s", v)` default arm. (Guarded
by the `MakeSlicePanicRange` behavioral test — in-range, negative, huge-length, and huge-capacity
`make` under `recover()`, messages compared vs Go.)

## A panicked C# `string` boxes as Go `string` at golib's boxing boundary
Go's `panic` takes an `any`, so the panicked value's **dynamic type** is observable on the recover
side — `if p != "x"`, `err.(string)`, and `case string:` all test it. Two converted spellings hand
golib a bare C# `System.String` rather than a Go `@string`: a **string literal** (`panic("x")` — the
emission deliberately suppresses the `u8` suffix there, so the argument stays a C# literal) and a
**computed** C# `string` from hand-written C# code. Boxed as
`System.String`, such a value would match nothing on the recover side, which compares against `@string`:
sync's `testOncePanicX` reported the self-contradictory `want panic x, got x`.
<!-- history: the computed case was first met from a stub fmt.Sprintf returning C# string, since replaced by the
     converted fmt; three sync tests failed on it. -->

`builtin.panic(object)` — the single boxing boundary for the builtin — normalizes `string` to
`@string`. Choosing that layer over an emission-site `(@string)` cast is deliberate: the cast would
fix only the literal spelling, while the boundary covers literal, computed, and hand-owned callers
alike. It is a narrow normalization, not a coercion — a NAMED string type keeps its own identity
(a `/*@string*/` wrapper is not a C# `string`), and non-string values are untouched. golib's
own `RuntimeErrorPanic` values are unaffected: those construct `PanicException` directly, and Go's
dynamic type for them is a runtime error, not a string. (Guarded by `PanicRecover`, extended with a
recover-side type switch over a literal, a computed, a variable, a named-string-type, an int, and a
no-panic case, output-compared vs Go.)

## A NIL-POINTER dereference is a RECOVERABLE panic, with Go's message
The same class as the `makeslice` arm above, for the most common Go runtime panic of all. Go's nil
dereference is recoverable and real code depends on it — sync's `TestNilPool` calls `Get`/`Put` on a
`var p *Pool` and asserts that `recover()` catches the panic. The converted equivalent (reading
`Ꮡp.Value` through a nil `ж<T>`, or any nil reference deref in emitted code) raises .NET's
`NullReferenceException`, which `recover()` could not see: the panic escaped past every deferred
recover and surfaced as an unrecoverable host error. `RuntimeErrorPanic.TryAsPanic` — the single
predicate every emitted frame's exception filter (`GoFrame.IsPanic`) and the process-level unhandled
handler share — maps it to `RuntimeErrorPanic.NilPointerDereference()`, whose text is Go's
verbatim `runtime error: invalid memory address or nil pointer dereference`.

This is faithful rather than lenient: Go recovers a genuine nil-deref bug exactly the same way, and
an *unrecovered* one still prints Go's message on stderr and exits 2 (the unrecovered-panic arm
above). (Guarded by the `NilPointerPanic` behavioral test — a nil pointer-receiver method call, a
nil struct-pointer field read, and a nil map/slice-of-pointer element deref, each under `recover()`
with the recovered text compared vs Go, plus an unrecovered control.)

## Named-delegate and builtin callees keep the lambda form
A zero-argument deferred/goroutine'd call whose callee is a **named func type** (`defer cancel()`
with `cancel context.CancelFunc`, net dial) cannot take the bare trimmed method-group form —
the named type is a DISTINCT C# delegate with no conversion to the `Action` golib expects
(CS1503) — so the invocation stays wrapped: `defer(() => cancelʗ1(), ref ᒐ)` / `goǃ(() => f())`.
A **builtin** deferred WITH arguments (`defer close(returned)`) is generic with `in` parameters,
so its method group neither infers nor converts to `Action<T>`; the temp-param lambda keeps
`defer`'s eager-argument evaluation: `defer(ᴛ1 => builtin.close(ᴛ1), returned, ref ᒐ);` (net
`dial.cs`). (Guarded by `DeferCallOrder`'s stopFn + close(drained) shapes, output-compared vs Go.)

## A value-returning goroutine callee is wrapped in a discarding lambda
Go's `go f(…)` discards `f`'s result. Every `goǃ` runtime overload takes a **void** `Action<…>`
delegate, so a value-returning callee passed as a bare method group binds no overload (CS0407 "no
overload matches the delegate" — x/net/nettest `conntest.go`'s `go chunkedCopy(c2, c2)`, where
`chunkedCopy(io.Writer, io.Reader) error` returns `error`). `visitGoStmt` resolves the callee
signature and, when it returns a value, keeps the invocation inside a lambda so the result is
discarded — an expression-bodied lambda over a value-returning call converts to `Action` (the same
form the variadic path, e.g. `go fmt.Println(…)`, already emits):

<!-- illustration: not converter output -->
```csharp
go chunkedCopy(c2, c2)          -> goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), c2, c2);   // param callee
go q.conn.HandshakeContext(ctx) -> goǃ((ᴛ0, ᴛ1) => ᴛ0.HandshakeContext(ᴛ1), Ꮡq.Value.conn, ctx); // selector method
go c.Close()                    -> goǃ(() => c.Close());                             // nullary callee
```

This parallels the **defer** case. Both `defer` and `goǃ` carry sixteen `Func<…, TResult>` twins (arities 1 through 16)
alongside their `Action` rungs, so a value-returning method group *with arguments* binds either
directly; neither has a nullary `Func<TResult>` rung, so a nullary value-returning callee takes the
`() => call()` discard on both sides (see *Deferred calls whose callee returns a value* below). The
converter's remaining discarding wraps cover the shapes a method group cannot express here — a
value-returning callee reached through a selector or parameter, and the CS1113
value-receiver-extension case below, whose reason is delegate *creation* rather than the callee's
result. Func-literal callees and `void`-returning method groups are untouched (`goǃ(() => { … })`,
`goǃ(emit, out)`).

The runtime's `Func` twins are what make one shape expressible at all: a func-literal callee that
*returns a value*. `go func(ln Listener) (retErr error) { … }(ln)` (net `sendfile_test`) emits its
literal with an explicit `error` return type, because a named result set by a `defer` needs one, and
no `Action<Listener>` overload accepts it (CS8934). Wrapping it in the converter would mean
suppressing the literal's own return type and rewriting its trailing `return retErr;` — rewriting a
correct emission to fit a runtime gap. The `Func` siblings fix that shape, and every other one, at
the seam where Go's rule actually lives: `go f(…)` discards results, for **any** `f`. A void lambda
or method group cannot bind `Func<TResult>` at all, so no existing call site is affected. (Guarded by
the `GoStmtValueReturn` behavioral test — value-returning nullary, single-, multi-param, multi-result
and func-literal goroutine callees, output-compared vs Go.)

A **VALUE-receiver method callee** forces the same lambda forms even when void and
arity-matching: every Go named type emits a C# struct and the method an extension on it, and C#
forbids constructing a delegate from an extension method over a **value-type receiver** (CS1113 —
net/http/httputil's `go spc.copyToBackend(errc)`, `switchProtocolCopier`). So
`goǃ(spcʗ1.copyToBackend, errc)` becomes `goǃ(ᴛ1 => spcʗ1.copyToBackend(ᴛ1), errc)` and a nullary
`go vs.ping()` keeps its invocation (`goǃ(() => vsʗ1.ping())`). The receiver snapshot (`spcʗ1`)
still evaluates at go-statement time. A value receiver that is not a plain identifier (`go v[i].ping()`)
is passed as the thunk's `ᴛ0` instead (see
[the element and field receiver section](#a-deferred-or-spawned-call-evaluates-an-element-or-field-receiver-at-the-statement)). An INTERFACE-receiver method group is excluded (a genuine C#
instance method binds delegates fine), and pointer receivers keep the box-group machinery
(`pointerReceiverBoxMethodGroup` — `ж<T>` is a class, so its group is delegate-legal). (Guarded by
`GoStmtReceiverLambda`'s `valueSender` arms — value-receiver argument and nullary go-statements
with blocking-receive completion proof, output-compared vs Go.)

## A func-literal ARGUMENT of a deferred call hoists its captures before the call
When a deferred call's **callee** is itself a func literal (`defer func() { … }()`), that literal's
lambda-capture snapshots (`var sʗ1 = s;`) are threaded to a builder emitted *before* the `defer(…)`
call. But when the deferred callee is an ordinary call whose **argument** is a capturing func literal —
x/net/nettest `conntest.go`'s `defer once.Do(func() { stop() })` — the argument literal's snapshot
declarations were dumped inline into the deferred call's argument list, an invalid statement
mid-expression (`defer(Ꮡonce.Do,` `var stopʗ1 = stop;` `() => …)` → CS1001/CS1002/CS1003/CS1026).
The hoist sink (`lambdaContext.deferredDecls`) is provided **unconditionally** in `visitDeferStmt`,
not only for the func-literal-callee case, so `convFuncLit` (reached via convCallExpr → convExprList →
the argument's `LambdaContext`) routes any argument literal's captures to it, and they are emitted before
the call — the guard's `defer run(func(){ pf.x = 77 })` shape:

<!-- source: src/tests/Behavioral/FuncLitArgCapture/main.cs:71-74 -->
```csharp
var pfʗ1 = pf;
defer(run, () => {
    pfʗ1.Value.x = 77;
}, ref ᒐ);
```

The empty builder is inert for a deferred call with no capturing func-literal argument (zero golden
churn — the behavioral corpus is byte-identical), and a deferred call whose own arguments are plain
captures keeps its existing pre-call `generateCaptureDeclarations()` emission. (Guarded by the
`FuncLitArgCapture` extension case 14 — a func literal passed as the argument of a deferred `run(…)`
call capturing a local pointer, whose deferred write lands through the shared pointer box, output-compared
vs Go.)

## A PARENTHESIZED func-literal callee is the bare literal
`go (func() { … })()` (gopkg.in/check.v1's `forkCall`) and `defer (func() { … })()` name the same callee
as their unparenthesized forms, and emit the same C#. Three places used to test the callee for a func
literal without looking through the parentheses: `visitGoStmt`/`visitDeferStmt` prepared the statement's
captures as for an ordinary callee, the capture analysis did not treat the literal's body as a closure
body, and `convCallExpr` rendered the callee through `convParenExpr`, which drops the hoist sink. The
literal's capture copies were then written inside the call (`goǃ((` `var doneʗ2 = doneʗ1;` `() => …`,
CS1002/CS1003/CS1026/CS1513, with `doneʗ1` never declared). All three unwrap the parentheses, so
the copy is a statement before the call and the deferred literal reads its variables by reference:

<!-- source: src/tests/Behavioral/ParenFuncLitGoDefer/ParenFuncLitGoDefer.cs.target:16-28 -->
```csharp
    var doneʗ1 = done;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var doneʗ2 = doneʗ1;
            defer(() => {
                doneʗ2.ᐸꟷ(true);
            }, ref ᒐ);
            dispatch(c);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
```

(Guarded by `parenFuncLitGoDeferCallee_test.go` and the `ParenFuncLitGoDefer` behavioral test, which
runs the `forkCall` shape and a deferred literal that observes a later reassignment, output-compared vs
Go.)

## Defer/go EAGER arguments follow the enclosing closure's capture renames
Go evaluates a deferred (or spawned) call's function value and arguments **at statement time, in the
enclosing scope**. The defer/go emission enters its own lambda-conversion state (its callee snapshots
need a fresh remap set), but that fresh state previously hid the ENCLOSING lambda's capture renames
while the eager arguments rendered: inside an IIFE that snapshot-captured a heap-boxed outer local
(`var baseʗ1 = @base;`), the argument of `defer func(t Tally) { … }(base)` emitted the raw ref-local
`@base` — uncapturable in the IIFE's C# lambda (CS8175) — with the snapshot left declared but unused;
where the raw name IS capturable (a reference-typed local — net/http `transport.go`'s
`defer close(didReadResponse)` inside a `go func() { … }` lambda), it silently bypassed the snapshot
every other read in that body uses. `visitDeferStmt`/`visitGoStmt` enter through a **seeded**
variant (`enterDeferGoLambdaConversion`) that copies the enclosing lambda's renames into the fresh
state, so the arguments render exactly like any other expression in the enclosing body:

<!-- source: src/tests/Behavioral/DeferArgEnclosingCapture/main.cs:41-51 -->
```csharp
var baseʗ1 = @base;
((Action)(() => {
    GoFrame ᒐ = default;
    try {
        defer((Tally t) => {
            report("deferred:", t, 4);
        }, baseʗ1, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}))();
```

`prepareStmtCaptures` still OVERRIDES the statement's own captured-callee entries afterward (their
defer-time snapshots), and a function-level defer/go — no enclosing lambda — is untouched (the seed
set is empty). <!-- Whole-stdlib footprint at landing: net/http transport.cs alone, where the deferred
close argument became the goroutine lambda's didReadResponseʗ1 (semantically neutral there: both
names alias one channel object; the fix matters for ref-local-boxed value locals). --> (Guarded by the
`DeferArgEnclosingCapture` behavioral test — a heap-boxed struct local passed eagerly to a deferred
func literal, a deferred NAMED callee, and a go-statement literal, each inside an IIFE, with the
mutations landing on the deferred copies and the source read back untouched, output-compared vs Go.)

## A func-literal ARGUMENT inside an `if`/`for` condition hoists its captures before the statement
The same capture-snapshot hazard occurs when a capturing func literal is passed as a call argument
**inside a condition**. `go/types` is dense with this shape — `underIs(t, func(u Type) bool { … })`,
`typeSet().is(func(t *term) bool { … })` — and the literal's snapshot declarations (`var suʗ1 = su;`)
are statements, invalid inside the condition expression. `visitExprStmt` / `visitAssignStmt` already
route such decls to a pre-statement hoist buffer (`v.hoistedDecls`), but `visitIfStmt` and
`visitForStmt` converted the condition with `convExpr(cond, nil)` and no hoist target, so the decls
were dumped inline into the condition (`if (tpar.underIs(` `var suʗ1 = su;` `(ΔType u) => { … }))` →
CS1003/CS1026/CS1002/CS1022/CS1513). Both statement emitters
convert the condition into a hoist buffer and write any collected decls on their own lines **before**
the `if`/`for`, mirroring `visitExprStmt`:

<!-- source: src/tests/Behavioral/FuncLitCaptureInCondition/main.cs.target:20-23 -->
```csharp
    var lookupʗ1 = lookup;
    if (anyMatch(vals, (nint u) => lookupʗ1[u])) {
        fmt.Println("if:", true);
    }
```

The condition is converted **after** an `if`/`for` init clause (preserving capture-counter ordering),
and the `if`-with-init sub-block hoists between the init and the `if`. The traditional `for` reuses the
existing `ForVarInitMarker` slot — the hoisted condition decls are emitted at the same pre-`for` position
as the for-init heap allocations. The hoist buffer is empty for a condition with no capturing func-literal
argument, so the behavioral corpus is byte-identical.
<!-- At landing: the stdlib deltas were go/types files (under.cs, builtins.cs, expr.cs, index.cs, instantiate.cs) and
     a crypto/tls slices.ContainsFunc call; go/types had ~63 CS100x/CS1026 from this one construct. Clearing that
     syntax-error layer surfaced deeper semantic defects in go/types then (a map[token.Token]func() mis-lowered to a
     malformed explicit-interface IDictionary/ICollection implementation, named-slice wrappers not satisfying
     IArray.Source, token resolution). --> (Guarded by `FuncLitCaptureInCondition` — a func literal
capturing an enclosing map, passed as an argument inside a plain `if` condition, an `if` condition with
an init clause, a traditional `for` condition, and a while-style `for` condition, all output-compared vs
Go.)

## A func-literal ARGUMENT inside a return expression hoists its captures before the `return`

The third statement position with the same hazard: a capturing func literal passed as a call argument
**inside a return expression** — net/http's `findHandler` returns
`HandlerFunc(func(w ResponseWriter, r *Request) { … allowedMethods … }), "", nil, nil`, and traceviewer's
`MainHandler` returns `http.HandlerFunc(func(){ … views … })`. A **direct** func-literal result threads
`lambdaContext.deferredDecls` (the go/defer/return channel in `convFuncLit`), but a literal nested as a
call **argument** falls back to the pre-statement hoist sink, which `visitReturnStmt` never provided —
the snapshot declaration was dumped inline inside the return expression (10 syntax errors in `server.cs`,
a 4-error cascade in traceviewer). `visitReturnStmt` provides the same hoist buffer as
`visitExprStmt`/`visitIfStmt`/`visitForStmt` and splices it before the `return` through its existing
`DeferredDeclsMarker` slot (ahead of any deferred tuple-deconstruction temps):

<!-- source: src/tests/Behavioral/ReturnTupleFuncLitArg/main.cs.target:19-22 -->
```csharp
    var allowedʗ1 = allowed;
    return (new handler((@string msg) => {
        fmt.Println(allowedʗ1[0] + ":" + msg, len(allowedʗ1));
    }), "label", default);
```

The buffer is empty for a return with no capturing-literal argument, so the behavioral corpus is
byte-identical. (Guarded by `ReturnTupleFuncLitArg` — a slice-capturing literal as a call argument inside
a three-result return tuple, and a map-capturing one inside a single-result return, output-compared vs
Go.)

## A func literal inside a RANGE expression hoists its captures before the loop

The fourth statement position, and the one the table-driven test idiom lands on constantly:

```go
for _, test := range []struct {
    desc string
    f    func()
}{
    {desc: "WithCancel(bg)", f: func() { c, cancel := WithCancel(bg); cancel(); <-c.Done() }},
    …
} {
```

The literal's snapshot declaration (`var bgʗ1 = bg;`) is a **statement**, and the composite-literal
element position it would be written into is pure expression context. `visitRangeStmt` converted the
range expression with `convExpr(rangeStmt.X, nil)` — no hoist target — so the decl was dumped inline
after the `f:` argument name, and the whole file died in a syntax cascade (`context`'s `x_test.cs`:
CS1003/CS1026/CS1002/CS1513/CS0106 ×195, from `TestAllocs` and `TestCause` alone).

`visitRangeStmt` converts the range expression into a hoist buffer and **splices** the collected
decls in at the statement's own start — it records that offset before conversion and inserts there
afterwards (`spliceOutput`, the positional twin of `replaceMarker`), because the `foreach` header is
emitted much further down through a dozen different arms:

<!-- source: src/core/context/x_test.cs:404-405, 409, 412, 439-448, 459 -->
```csharp
    var bg = Background();
            var bgʗ1 = bg;
            …
            var bgʗ3 = bg;
    …
    foreach (var (_, test) in new TestAllocs_type[]{
        …
        new(
            desc: "WithCancel(bg)"u8,
            f: () => {
                var (c, cancel) = WithCancel(bgʗ3);
                cancel();
                ᐸꟷ(c.Done());
            },
            limit: 5D,
            gccgoLimit: 8D
        ),
    …
    }.slice()) {
```
<!-- excerpt of src/core/context/x_test.cs (TestAllocs); the hoisted snapshots keep the splice's own indentation. -->

The buffer is empty for a range expression with no capturing func literal, so the behavioral corpus is
byte-identical. (Guarded by `RangeExprFuncLitCapture` — slice- and map-capturing literals as struct
fields of a ranged composite literal, plus a bare `[]func(string)` element list, output-compared vs Go;
its A/B reproduces the cascade exactly.)

**The class, stated once:** `visitExprStmt`, `visitAssignStmt`, `visitIfStmt`, `visitForStmt`,
`visitReturnStmt`, `visitValueSpec`, `visitRangeStmt`, `visitSendStmt` and the `select` comm-clause
operands each provide the pre-statement sink (the send and `select` sinks are described in
[Shadowing](shadowing.md#the-capture-snapshot-is-a-statement-so-every-position-that-can-hold-a-func-literal-owes-it-a-hoist-target)).
The statement kind that still does not — a `switch` tag — has no
demonstrated corpus site, and would repeat this failure exactly. It is deliberately not widened
speculatively: the tell that one has been reached is a syntax cascade whose first error sits on the line
after a `<name>:` argument label.
## The enclosing statement's hoist buffer does NOT extend into a literal's BODY

Those four positions all work the same way: the enclosing statement opens a hoist buffer, and a
capturing func literal inside it writes its snapshot declarations there. The buffer is a valid position
for **that literal's own** captures — they name bindings from the enclosing scope, which exists before
the statement. It is not a valid position for anything the literal's **body** hoists: a statement inside
the body opens its own buffer, and a nested literal whose captures name a binding declared *inside* this
body would be declared outside it.

`time`'s `BenchmarkStaggeredTickerLatency` nests three levels of `b.Run(…, func(b *testing.B){…})`. The
middle literal `make`s a `stats` slice; the innermost `go func(…)` captures it. The snapshots landed in
the OUTER literal's `b.Run(…)` statement buffer — two blocks above the declaration:

<!-- illustration: not converter output -->
```csharp
for (nint tickersPerP = 1; …; tickersPerP++) {
    nint tickerCount = gmp * tickersPerP;
    var statsʗ1 = stats;                      // CS0103 — `stats` is declared below, inside bΔ2
    bΔ1.Run(…, (ж<Δtesting.B> bΔ2) => {
        var stats = new slice<…>(tickerCount);
```

`convFuncLit` detaches `v.hoistedDecls` for the duration of the body walk and restores it after, so
a nested hoist can only reach a position inside the body. The literal's own captures are unaffected —
they are flushed before the body is converted, while the enclosing buffer is still installed. (Guarded
by `FuncLitArgCapture` case 15.)

Handling Go `defer` / `panic` / `recover` is what the FRAME above is for: the body is emitted inline in `try`/`catch`/`finally` beside a [`GoFrame`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/GoFrame.cs) local that holds this call's defer list. `panic` is the global [`panic`](https://golang.org/pkg/builtin/#panic) built-in and `recover` the global [`recover`](https://golang.org/pkg/builtin/#recover) (both a `using static go.builtin`). A function that neither directly nor indirectly (through a deferred lambda) uses `defer`/`recover` gets no frame at all -- the scope is per function, so a `main` that merely calls `f()` is emitted as a plain method body.

* **Named results + defer.** See *The named-result form* above: the results are declared before the `try` and read back after the `finally`, and every exit inside leaves through a `goto`.
* **IIFEs.** An immediately-invoked function literal that itself uses defer/recover carries its own frame inside its own delegate-cast invocation (`((Action)(() => { GoFrame ... }))()`), so its defers are its own -- while its `recover()`, being a static call, still reads the thread's panic state rather than any handle on a frame.
* **A `return` emits against ITS OWN function's results, not the enclosing function's.** A bare `return` in a function with named results emits `return (n, ok);` (the named results). A *nested function literal* must be converted against its **own** signature -- otherwise a bare `return` inside a **void** closure would inherit the enclosing function's named results and emit `return (n, ok);` into a `void` lambda (CS8030, "anonymous function converted to a void-returning delegate cannot return a value"). Runtime `mprof.goroutineProfileWithLabelsSync` (named `(n, ok)`) passes `forEachGRace(func(gp1 *g) { ...; return; ... })` -- the void closure's bare returns must stay `return;`. The return signature is tracked separately from `currentFuncSignature` (which stays the *enclosing* function's, so the receiver/parameter detection still resolves a **captured** pointer parameter -- an outer parameter -- correctly): `convFuncLit` sets a dedicated return-signature to the literal's own signature with save/restore, and `visitReturnStmt` emits results against it. (Guarded by the `ClosureBareReturnNamedResults` behavioral test -- a void closure with bare returns nested in a named-results function, output verified vs Go.) <!-- the fix cleared runtime's 4 CS8030. -->
* **Return-type INFERENCE is not a concern.** An inline body returns against the METHOD's own declared result type, so no inference runs over the return statements and the two shapes that would defeat one cannot arise: every return carrying an untyped `default!` (Go `nil` -- syscall's `getProcessEntry`), and returns of two unrelated concrete types sharing only the declared interface (go/parser's `parseTypeName` returning `&ast.SelectorExpr{...}` beside a plain `*ast.Ident`). Both are pinned as guards: `DeferTypelessReturns` (unnamed results, a defer, and every return carrying nil) and `DeferInterfaceReturn` (a defer/recover func returning `Shape` via `Circle` vs `Square`, plus a heterogeneous `(Shape, bool)` tuple return).

## A BLANK result mixed with a named one still needs the named-return-defer handling

Go permits mixing the blank identifier and real names in one result list — `func parse(s string, flags Flags) (_ *Regexp, err error)` (regexp/syntax) — and deferred code can still mutate `err`. The detection required **every** result to be named and non-blank, so the first `_` rejected the whole signature and the function fell back to the unnamed-result form, whose catch arm returns Go's zero results. On a recovered panic that arm returned `default!`: the deferred handler assigned `err`, the catch arm discarded it, and the function reported **(nil, nil)** — a *successful* parse of an expression that must fail. Every "expression too large" / "nesting depth exceeded" input (`a{100000}`, `strings.Repeat("(", 1000)+…`) came back as a valid parse, and the caller's `dump(re)` on the nil pointer then panicked.

A blank result is a real result **slot** — only a `return` statement can write it, the body cannot name it — so it needs a declaration alongside the named ones. C#'s `_` is the **discard**: declaring it would capture every later `_ = expr` in scope, and two blank results would collide outright, so `namedResultName` mints a generated slot name (interned per result object, so the declaration, each return's assignment and the post-defer read all agree):

<!-- illustration: adapted, owed a real excerpt -->
```csharp
internal static (ж<Regexp>, error err) parse(@string s, Flags flags) {
    ж<Regexp> _ᴛ1 = default!;          // the BLANK slot
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => { … err = new ΔErrorжerror(…); … }, ref ᒐ);   // recover assigns the named result
        …
        (_ᴛ1, err) = (literalRegexp(s, flags), default!); goto ᒐdone;   // `return literalRegexp(s, flags), nil`
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, err);          // reads BOTH slots after the defers ran
}
```

A result list that is entirely **unnamed** (`func f() (int, error)`) or entirely blank keeps the plain form: there is nothing deferred code could mutate, and Go likewise returns the zero results after a recover. Go forbids mixing named and unnamed results, so seeing one truly unnamed result settles the whole signature. (Guarded by the `NamedReturnDefer` extension — a `(_ *box, err error)` function whose recover sets `err`; the pre-fix converter compiles it and returns `(nil, nil)`.)

## Function-literal named results

A func **literal** with named results declares them at the top of its emitted block, zero-initialized — Go's semantics for `next = func() (v1 V, ok1 bool) { …; return }` (the `iter.Pull` shape): a bare `return` yields the named results as currently assigned, so the lambda emits `() => { V v1 = default!; bool ok1 = default!; …; return (v1, ok1); }`. Without the declarations the emitted tuple would reference undeclared names (CS0103, met in the `iter` package). Two interactions: a named-results literal whose *first* statement is a bare `return` must NOT collapse to an expression-bodied lambda (the names exist only as block declarations), and the `namedReturnDefer` path (named results that deferred code mutates) keeps its own arrangement — declarations *before* the `try`, returned after the `finally`. Declarations reuse the shadow-aware naming, so a literal result shadowing an outer local renames consistently in both the declaration and the return (`nΔ1`). (Guarded by the `FuncLitArgCapture` extension — bare returns with assigned and zero named results, plus the first-statement-bare-return shape, values vs Go.)

Because a named result lives in the literal's OWN scope, a reference to it in the body is the result, never an outer-scope capture — so named results are excluded from the lambda-capture set (`convFuncLit`) exactly as parameters are. text/template's `readFileFS` returns `func(file string) (name string, b []byte, err error)`, whose closure captures the enclosing `fsys` AND writes `b` via the captured tuple call `b, err = fs.ReadFile(fsys, file)`. Because the closure genuinely captures `fsys`, the capture analysis ran and mis-flagged `b` too — hoisting `var bʗ1 = b;` into the enclosing function, where `b` does not exist (CS0103), and renaming the body's `b` to the captured `bʗ1`. Filtering the named-result names out of the capture set (alongside the parameter names) leaves `b` a plain in-block declaration. (Guarded by `CrossPkgUser`'s `makeScanner` — a captured closure returning named results, one written via a tuple call whose RHS uses the capture, output-compared vs Go; crypto/x509 and html/template shared the same latent shape.)

## Deferred calls whose callee returns a value take the lambda form
The no-arg defer arm passes a bare method group (`defer(k.Close, ref ᒐ)`) only when the callee returns VOID -- an error-returning method (`defer k.Close()`, registry `Key.Close`) is a `Func<error>` method group that cannot bind the golib `defer(Action, ref GoFrame)` (CS1503). The lambda form discards the result, exactly Go's deferred-call semantics:
<!-- source: src/tests/Behavioral/DeferTypelessReturns/DeferTypelessReturns.cs:79 -->
```csharp
defer(() => hʗ1.close(), ref ᒐ);
```
Guarded by `DeferTypelessReturns`.

## Deferred pointer-receiver nullary calls bind the box method group
`defer conf.releaseSema()` with `conf *resolverConfig` (net nss.go / dnsclient_unix.go) trimmed to the deref-alias method group `Ꮡconf.Value.releaseSema` — a struct VALUE against the `this ref` extension, which cannot create a delegate (CS1113). The emission binds the BOX method group instead:
<!-- illustration: not converter output -->
```csharp
defer(Ꮡs.release, ref ᒐ);                 // DeferCallOrder's acquireAndWork: defer s.release()
```
(net's own `defer conf.releaseSema()` sites take the receiver-call lowering into the `finally`, described above, when its gates pass.) The `ж<T>` overload is class-typed and delegate-legal, and the method-group conversion captures the receiver when the delegate is created — exactly Go's binding time. Mirrored in the go-statement arm. Gated to methods declared DIRECTLY on the pointee — a PROMOTED method (net interface.go's `defer zc.Unlock()`, declared on the embedded `sync.RWMutex`) has no extension on the outer box (CS1061) and keeps the lambda emission — and to void results (a `Func<>` group binds neither `defer(Action)` nor `go(Action)`). (Guarded by `DeferCallOrder`'s `acquireAndWork`, output-compared.)

The same box-method-group emission also covers a **value receiver** whose type is exactly the pointer-receiver's pointee — `defer b.deck.reset()` (runtime/pprof; also database/sql, log/slog), where `deck pcDeck` is a value FIELD reached through a nested selector and `reset` has a `*pcDeck` receiver, so Go auto-takes `&b.deck`. The original arm required the receiver be an already-pointer *ident*; the value case renders `&receiver` through the shared address machinery (the same `&ast.UnaryExpr{AND}` → `convUnaryExpr` synthesis used elsewhere) — a boxed base gives the aliasing field-ref `Ꮡb.of(profileBuilder.Ꮡdeck)`, an escaping value local gives its box `Ꮡx`, a plain value gives the `Ꮡ(value)` copy — then binds the method: `defer(Ꮡb.of(builder.Ꮡc).reset, ref ᒐ)` for `defer b.c.reset()`, the `ж<T>` overload captured at defer time and mutating the real field. (pprof's own `defer b.deck.reset()` passes the gates of the [receiver-call lowering](#a-deferred-receiver-field-call-with-no-arguments-is-lowered-into-the-frames-finally) and is emitted into the frame's `finally` instead.) <!-- src/tests/Behavioral/DeferValueFieldPtrReceiver/main.cs.target:28 `defer(Ꮡb.of(builder.Ꮡc).reset, ref ᒐ);`; src/core/runtime/pprof/proto.cs:694 `finally { if (ᒐd1) Ꮡb.DerefOrNull().deck.reset(); ᒐ.Run(); }` --> Gated the same way (void result, a NAMED value type whose RecvGenerator box overload exists, matching the pointee exactly so a promoted/embedded method is excluded). (Guarded by the `DeferValueFieldPtrReceiver` behavioral test — a pointer receiver deferring `b.c.reset()` on a value field, and a pointer local deferring the same in a closure, with the reset observed through the same box after return, output-compared vs Go.)

## A deferred VALUE-receiver method passes its receiver as the thunk's first eager argument

A value-receiver method emits as a C# extension over a value type, which no delegate can be created from (CS1113),
so `defer s.curPtrs.Pop(px, py)` (go-cmp's `compare.go`, in production) cannot hand golib's `defer` the method
group `Ꮡs.Value.curPtrs.Pop`. A plain lambda over the call would compile but read the receiver at UNWIND, where Go
copies a method value's receiver at the DEFER statement: after `s.cur = …` the deferred call must still see the old
value. The receiver therefore becomes the thunk's first eager argument, `ᴛ0`, copied when the registration runs:

<!-- illustration: not converter output -->
```csharp
defer((ᴛ0, ᴛ1) => ᴛ0.Pop(ᴛ1), Ꮡs.Value.cur, k, ref ᒐ);   // defer s.cur.Pop(k)
defer(ᴛ0 => ᴛ0.Done(), Ꮡs.Value.cur, ref ᒐ);              // defer s.cur.Done()
defer(ᴛ0 => ᴛ0.Done(), Ꮡp.Value, ref ᒐ);                  // defer p.Done(), p *path: Go copies *p
```

The receiver text is split off the rendered call itself (`<receiver>.<method>(ᴛ1, …)` under the temp-parameter
form), so it is exactly the expression the call would have evaluated. It applies where the method group was the
form (a void nullary callee, or a non-variadic callee at arity N with no other reason to take the lambda) and also
where the LAMBDA was already the form — a RESULT-returning nullary callee (`defer s.cur.Close()`) or a variadic one
— over a FIELD or DEREFERENCED receiver. Those had read the receiver at unwind and compiled, silently wrong (`close
replaced` where Go prints `close orig`); they now take `defer(ᴛ0 => ᴛ0.Close(), Ꮡs.Value.cur, ref ᒐ)`. An
IDENTIFIER receiver keeps the capture hoist that already copies it at the defer (`var kʗ1 = k; defer(() =>
kʗ1.Close(), ref ᒐ)`), which covered every lambda-form site measured in the standard library and the behavioral
corpus. An interface receiver keeps its method group, and a pointer receiver keeps the box group above;
under the lambda form, an interface or pointer receiver that is not a plain identifier takes `ᴛ0` too (next
section).
The census before the change found no site in the converted standard library (production on three targets, tests on two)
or in the behavioral modules, because every site failed to compile. (Guarded by
`deferValueReceiverSnapshot_test.go` and the `DeferValueReceiverSnapshot` behavioral test: a field of a pointer, a
field of a value, a nullary call, a local and a dereferenced pointer, each reassigned after the defer, output-compared
vs Go; CS1113 on the pre-change converter. The lambda-form half is guarded by `deferLambdaReceiverCopy_test.go` and
the `DeferLambdaReceiverCopy` behavioral test, which COMPILES on the pre-change converter and fails its output
comparison: `log replaced`, `close replaced` and `close replaced` against Go's `orig`, `orig` and `pointee`.)
<!-- census at the change: 24 lambda-form sites with an identifier receiver; 757 behavioral modules. -->

## A deferred or spawned call evaluates an element or field receiver at the statement

Go evaluates the receiver of `defer c[i].Close()` and `go c[i].Close()` when the statement runs. The method-group
form binds it there. The LAMBDA form, which a result-returning or variadic callee takes, read a receiver that is
not a plain identifier when the thunk ran: `defer(() => cʗ1[i].Close(), ref ᒐ)` indexed the array copy at unwind,
and an array copy shares its elements (`array<T>` is a struct over a shared backing array), so a `c[i] = other`
after the defer closed the new element. The same held for a slice or map element, a field (`res.Body`), a
dereference and a call result. Such a receiver is the thunk's first eager argument, `ᴛ0`, in `defer` and `goǃ`
alike:

<!-- illustration: not converter output -->
```csharp
defer(ᴛ0 => ᴛ0.Close(), cʗ1[i], ref ᒐ);                // defer c[i].Close(), c a [N]closer
defer(ᴛ0 => ᴛ0.Close(), (~resʗ1).Body, ref ᒐ);         // defer res.Body.Close()
goǃ(ᴛ0 => ᴛ0.Close(), cʗ1[0]);                         // go c[0].Close()
defer(ᴛ0 => ᴛ0.Close(), Ꮡv.at<conn>(0), ref ᒐ);        // defer v[0].Close(), v a [N]conn, Close on *conn
defer(Ꮡv.at<conn>(0).Reset, ref ᒐ);                    // defer v[0].Reset(), Reset on *conn, no result
```

A pointer method on an addressable VALUE (`v[0].Close()` with `v` a `[N]conn`) is bound by ADDRESS in Go, so the
deferred call does see a later write to `v[0]`; the snapshot is that address, `Ꮡv.at<conn>(0)`, not the element's
value. Every form of such a call evaluates the address at the statement, so a heap-boxed variable at the root of
the receiver takes no capture copy: with one, the box method group named the copy's box, which does not exist
(`Ꮡvʗ2.at<conn>(0).Reset`, CS0103). Any other root keeps its copy, through which the address reaches the same
storage.
A pointer method PROMOTED to a value through an embedded field, and another package's variable (whose address
binds a copy, a documented limitation), keep their earlier emission.

Not covered: a nil interface receiver panics in Go at the statement, and the snapshot passes it to the thunk, which
panics when it runs. An IDENTIFIER receiver keeps the capture copy above. (Guarded by the `DeferLoopCapture` behavioral test's red 3, 4 and 5: an
interface, pointer and value element reassigned after a `defer` or `go` on it, and a pointer method on a value
element, output-compared vs Go; before the change red 3 and red 5 printed the new element and red 4 did not compile.)

## A deferred pointer-receiver method on an escaping value local captures by-box, not by-copy
The emission above binds the box (`Ꮡstate.free`) for a `defer state.free()` on a value local — but the CAPTURE analysis must cooperate. `defer`/`go`/closure bodies are lambda-conversion scopes: a variable used inside them that escapes to the heap is normally snapshot-copied into a `var stateʗ1 = state;` declaration so the C# closure captures a value, not an uncapturable ref-local. For an escaping value local used **only** as the receiver of a pointer-receiver method call (`state` a `handleState` value, `free` a `*handleState` method — log/slog handler.go's `defer state.free()`), that snapshot is doubly wrong: the address-taking is *implicit* (Go auto-takes `&state`), so the emission still binds the box — but of the *snapshot name* `Ꮡstateʗ1`, which is a plain value with no `Ꮡ` companion:
<!-- illustration: adapted, owed a real excerpt -->
```csharp
ref var state = ref heap<handleState>(out var Ꮡstate);
state = h.ch.newHandleState(buf, true, " "u8);
var stateʗ1 = state;         // snapshot copy — WRONG
defer(Ꮡstateʗ1.free, ref ᒐ);        // Ꮡstateʗ1 never declared → CS0103
```
The capture analysis recognizes this implicit address-of (a value receiver of a pointer-receiver method, matching the pointee exactly and NAMED — the same guard the emission uses) as a reason to treat the local as a **box-ref var**, exactly like an explicit `&state`: it skips the snapshot, and the emission binds the original heap box:
<!-- illustration: adapted, owed a real excerpt -->
```csharp
ref var state = ref heap<handleState>(out var Ꮡstate);
state = h.ch.newHandleState(buf, true, " "u8);
defer(Ꮡstate.free, ref ᒐ);          // binds the live variable's box
```
This is not merely a compile fix — a value snapshot is taken at defer time, so it would miss any mutation the body makes to `state` before the deferred call runs; binding `Ꮡstate` matches Go's semantics of deferring against the *live* variable. Gated to an escaping local (a non-escaping one has no `Ꮡ` box and keeps the compiling `Ꮡ(copy)` form) used as a value receiver whose type is exactly the method's pointer-receiver pointee (an already-pointer receiver's box group is the pointer variable itself, whose snapshot name IS declared, so it is excluded). The same generalization silently corrects the closure form (`func(){ x.mutate() }` on an escaping value local previously mutated a lost copy — go/types conversions.go/typeset.go) and removes now-dead `var xʗ1 = x;` snapshots wherever the box was already used. (Guarded by the `DeferHeapLocalPtrMethod` behavioral test — a value local deferring a pointer-receiver method, mutated after the defer, with the deferred method observing the final value, output-compared vs Go.)

The same box-ref treatment covers a **promoted** pointer-receiver method reached through **value embeds** — `lazyCert.Do(…)` on `var lazyCert struct { sync.Once; v *Certificate }` (crypto/x509 `AppendCertsFromPEM`): Go takes `&lazyCert.Once`, an address into the variable's own storage, so the closure must share the original variable. The detection resolves the call through `info.Selections` and walks the selection's embedded-field index path — only **value** embeds along the path root the address at the variable (a **pointer** embed re-roots it at that pointer's target, where the snapshot, which copies the pointer, stays sound). Emission then renders the promoted call through the box's field projection and field uses through the box read; the snapshot form had referenced a never-declared snapshot box (`ᏑlazyCertʗ1`, CS0103) *and* divorced the closure's writes from the original:
<!-- source: src/core/crypto/x509/cert_pool.cs:265-270 -->
```csharp
ᏑlazyCert.of(AppendCertsFromPEM_lazyCert.ᏑOnce).Do(() => {
    // This can't fail, as the same bytes already parsed above.
    (ᏑlazyCert.Value.v, _) = ParseCertificate(ᏑcertBytes.ValueSlot);
    ᏑcertBytes.ValueSlot = default!;
});
return (ᏑlazyCert.Value.v, default!);
```
A variable marked box-ref is also never snapshot-copied by a **nested** literal — the box is a plain reference local that closures at any nesting depth capture directly, so the per-layer `var lazyCertʗ2 = lazyCertʗ1;` chains disappear with it. (Guarded by the `ClosureEmbeddedPromotedPtrMethod` behavioral test — an anonymous-struct local with a value embed whose pointer-receiver method is called from sibling and nested closures interleaved with field writes, cumulative counts observed vs Go.)

The same box-ref treatment covers a pointer-receiver method on a **value-struct FIELD projection** of the escaping local — `defer pr.fake.setLines()` (go/internal/gcimporter ureader.go), where `pr` is an escaping `pkgReader` value local and `setLines` a `*fakeFileSet` method on the value field `pr.fake`: Go takes `&pr.fake`, an address INTO `pr`'s own storage, and the emission renders it through the box's field view — but the snapshot path renamed the base first, referencing a never-declared snapshot box:
<!-- illustration: not converter output -->
```csharp
var prʗ1 = pr;                                 // snapshot copy — WRONG
defer(Ꮡprʗ1.of(pkgReader.Ꮡfake).setLines, ref ᒐ);    // Ꮡprʗ1 never declared → CS0103
```
The capture analysis matches the single field-projection receiver (a FIELD selected on the var's own value-struct storage — the same `&m.field` form `lambdaBoxRefAddressForm` emits — whose type is exactly the method's pointer-receiver pointee and NAMED) as the same implicit address-of, marks the local box-ref, and the defer binds the live box's field view:
<!-- source: src/core/go/internal/gcimporter/ureader.cs:66 -->
```csharp
defer(Ꮡpr.of(pkgReader.Ꮡfake).setLines, ref ᒐ);
```
As with the direct case this is a write-visibility fix, not merely a compile fix: gcimporter registers the defer *before* importing (which populates `pr.fake.files`), so a snapshot would flush an empty file set. The same generalization corrects deferred **closures** that read such a variable — go/parser's `defer func(){ …; err = p.errors.Err() }()` snapshot-copied `p` at defer time, so the closure read the parser state from *before* parsing (errors always empty); box-ref renders those reads `Ꮡp.Value.errors…` against the live variable. A deeper chain (`p.a.b.m()`), a pointer field hop, or a method promoted through the field's own embeds keeps the existing snapshot handling. (Guarded by the `DeferHeapFieldPtrMethod` behavioral test — a heap-boxed value local deferring a pointer-receiver method on its value field, with lines appended after the defer observed by the deferred flush, output-compared vs Go.)

---

[← Delegates to Value Receiver Instances](value-receiver-delegates.md) · [Index](README.md) · [Expression Switch Statements →](expression-switch.md)
