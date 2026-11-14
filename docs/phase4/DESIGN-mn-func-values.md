# DESIGN: a func value's way to its twin (M:N prototype stage (b), N3)

> - **Status:** for COORD's review. Prototype record: nothing on this branch lands on master as it is. No code yet.
> - **Written:** 2026-11-13 18:33 CST by G, at branch `claude/g-mn-proto-b0` (A5's tip `df130d5f03` plus the
>   freeze sheet's rulings block). Windows x64, SDK 10.0.400 (runtime 10.0.11), go1.26.10.
> - **Settled, cited not reopened:** the freeze sheet's four rulings and the owner's fallback ruling (the dated
>   block on `src/tools/mnspike2/TwinRefs/FREEZE-SHEET.md`); seat S1's answer G1 (a twin belongs to the METHOD and
>   is bound to the delegate's target at the call), accepted by COORD.

## 1. The gap

A Go func value is a delegate, and the delegate holds the PLAIN body. A twin that calls through a func value runs
that plain body, so a park behind it holds its carrier thread (`DATA-mn-spike-2.md`, N3 and 6.9):
- net/http's twins hold 69 calls through a func value and 26 parks inside func literals; a handler registered as a
  func value (`HandlerFunc`) is that package's common shape;
- the TypeScript closure has 124 of 2,197 func-value call sites, and 127 of 1,466 literals, that reach a park.

Closing it asks a way from a func value to its twin that does not change the func value's visible type. Phase 1
measured one form, a registration at the literal (+609 ns). The call side was never measured.

## 2. The rules any answer keeps

- **No visible change.** The converted source a reader sees is untouched, and a func value's C# type stays the
  delegate it is today. A change to that type (option D2) would be a tier 1 mapping change: it returns to the owner
  as a visible-type change, and only if N3 cannot close without it. It does not need to (section 4).
- **S1's word.** `GoFuncCookie` answers one word per static function and one per closure value
  (`docs/phase4/DESIGN-sqlite-cs-funcptr.md` section 3.1).
- **Names.** reflect's func names and `runtime.FuncForPC` name the Go function, as today.
- **Nil.** A nil func value stays a null delegate, so every nil test reads as today.
- **AOT.** Nothing new reads `Delegate.Method` on a hot path (498 ns under Native AOT, section 3).

## 3. Measured

A microbenchmark of each candidate's costs, in scratch until this design is ruled. JIT build and Native AOT publish
(win-x64); five processes each; each arm's floor of 7 passes of 2,000,000 ops, then the floor over the processes;
bytes from the thread's allocation counter. Read 2026-11-13 18:31 CST.

| arm | what | JIT ns | JIT B | AOT ns | AOT B |
|---|---|--:|--:|--:|--:|
| A0 | a closure literal created and called, today | 10.40 | 88 | 11.81 | 72 |
| D3c | the same literal created as a pair (section 4), then called | 26.36 | 248 | 30.70 | 200 |
| A0call | a call through a closure delegate | 0.39 | 0 | 1.60 | 0 |
| D3call | a call through the pair-bound delegate | 0.74 | 0 | 1.83 | 0 |
| D3s | a call through a static function's cached pair | 2.53 | 0 | 2.06 | 0 |
| D3t | the twin-side test: is the delegate's target a pair | 0.81 | 0 | 8.30 | 0 |
| D1hit | a ConditionalWeakTable lookup of a known delegate | 12.44 | 0 | 11.74 | 0 |
| M | `Delegate.Method` read on a fresh delegate | 81.29 | 184 | 498.15 | 296 |
| D1reg | a fresh delegate's first lookup: miss, Method read, method table, registration | 541.21 | 213 | 1049.89 | 319 |

Predictions posted before the run, scored: D3's creation +10 to 30 ns over A0, TRUE (+16 JIT, +19 AOT); its
call-side test under 3 ns, TRUE on the JIT (0.8) and FALSE under AOT (8.3, the test of a generic class);
`Delegate.Method` under AOT over 200 ns, TRUE (498). D3's creation stop line (100 ns) is not tripped.

## 4. The answer: D3, a pair made in the twin

**Where pairs are made.** Under M:N, goroutine 1 runs `main`'s twin (`Goroutine.RunMainTwin`), and a goroutine
started from a twin runs twins. So a func value a running program creates is, wherever it matters, created inside
a TWIN body, which the generator writes. The generator makes the pair there, and nowhere else:
- a func literal in a twin body whose body can park becomes `GoFuncPair` over (its plain lambda, its twin lambda).
  Both lambdas are written in the same scope, so C# gives them one closure object: a write through either is seen by
  both, as Go's single closure does;
- a can-park named function or method value converted to a func value in a twin body becomes that function's pair,
  cached once per function (D3s: no allocation per conversion);
- the visible source, and the plain bodies, make plain delegates exactly as today.

The goǃ site already does this (`goǃ(() => {...})` becomes `goǃtwin(literal, async literal)`); D3 extends it from
the go statement to every func value a twin creates.

**What a pair is.** A sealed class the generator emits per delegate type it pairs, internal to the assembly,
derived from one non-generic golib base, `go.golib.GoFuncPair` (tier 2, `[go-tier2]`), which exposes the plain
and the twin as `Delegate`. The func value handed out is the PLAIN delegate type, bound to the pair's `Invoke`,
which calls the plain body. So plain code that receives it calls it as today (D3call: +0.35 ns JIT, +0.23 ns AOT).

**The call.** A twin calling through a func value tests the delegate's target: a pair's twin is awaited, anything
else is called as today and holds its carrier as today (the fallback, named by the generator's diagnostic per the
owner's ruling). The test is one type test of the NON-generic base, to avoid D3t's 8.3 ns AOT generic test; that
cost is re-read in the implementation (prediction P3 below).

**The one unwrap rule.** A pair-bound delegate's `Method` is `Invoke` and its target is the pair, so every golib
reader of `Delegate.Method` or `Delegate.Target` sees through a pair to its plain delegate, by one golib helper:
- `GoFuncCookie.Of` (S1's one word per static function: a pair stands for its plain delegate, as S1's `new F(g)`
  wrapper does today);
- reflect's func-name reader (`GoReflect.TypeLayout`, which already unwraps a `new F(g)` wrapper), `GoSyntheticPC`
  and `runtime.FuncForPC`;
- `ж.PointerTokens`' delegate arm and `builtin.DelegateConversions` (a rebind over a pair keeps the pair: right).
The helper and the two existing wrapper loops become ONE rule (a wrapper over a delegate, or a pair over its plain
delegate, stands for what it wraps).

**Cost, per program.** Only literals created in twin bodies whose body can park pay: +16 to 19 ns and +128 to 160
bytes a creation. A static function pays nothing per conversion. A twin's dynamic call pays one type test. Plain
code pays nothing.

## 5. Why not the others

- **D0, leave it:** a thread per goroutine parked behind a func value, the generator's widest gap.
- **D1, a side table by method:** the twin's call needs the delegate's method, which is 498 ns under AOT, so every
  delegate pays about 1 us at its first twin call (D1reg) and 12 ns at each later one (D1hit); a closure needs a
  registration at its literal, the +609 ns phase 1 measured. D3 is an order cheaper at creation and at the call.
- **D2, a golib func type:** visible, a tier 1 change, and not needed.

## 6. Predictions for the implementation (stage (b)'s code, after this is ruled)

- P1 A program whose goroutines park behind func values (the HandlerFunc shape and a WaitGroup.Go literal) parks
  them on the scheduler: its thread count stays at the carrier count, where today it grows by one per park.
- P2 The converted source a reader sees does not change: 0 tracked .cs files differ (the generator's output only).
- P3 A twin's call through a func value costs at most 3 ns more than a direct twin call under AOT, with the
  non-generic test.
- P4 S1's tracked program FuncWordShapes and N1/N2's nil-func programs keep their output with twins on; reflect's
  func names and FuncForPC read the same Go names with twins on as off.
- P5 A literal created in plain code and called from a twin takes the fallback, and the generator names the site.

## 7. Stop lines

- S1 any visible .cs change; S2 a func value's C# type changes; S3 a word, a name or a nil test that differs with
  twins on; S4 a pair creation over 100 ns or over 256 bytes; S5 a second unwrap rule instead of one.

## 8. Not covered

- A func value created in plain code (a package initializer's table of handlers, say) keeps its plain body; P5
  names each such site. Whether initializers should run as twins is stage (c)'s census, not this.
- Func values crossing into hand-owned C# that stores them in its own collections: they stay pair-bound delegates,
  so a twin that later calls one still finds the pair.
- Generic func literals and func values of generic signatures: the per-signature pair class is per closed delegate
  type; open generic delegates in twin bodies are read in the implementation.

## 9. Amendment after the review (dated block, 2026-11-13 23:06 CST, G)

> COORD's Fable review of `3d0b84d92d` ruled APPROVE-WITH-FIXES: seven must-fixes, seven shoulds, and a census owed
> before code. This block answers each; the sections above stand as written unless a line below supersedes them.
> Data: [`data/mn-func-values/`](data/mn-func-values/) (the census tool, its two readings and the control program;
> the bench, both runs, and the bound-method arms' predictions as written before the run).

### 9.1 The base, and the order

The prototype line does not hold seat S1: its `GoFuncCookie.Of` is per instance, ONE wrapper loop exists in golib
(`GoReflect.TypeLayout`), not two as section 4 said, and `FuncWordShapes` is not there. ORDER: S1 is merged into the
prototype line (after batch 302 lands) before any unwrap work; the unwrap rule is written against S1's
`GoFuncCookie`.

### 9.2 The census (owed before code)

The question: of the call sites through a func value that can park, how many get their value from a CREATION site
inside a twin, where D3 makes its pairs? The tool is phase 1's `canpark` (B5's build, with `-no-func-values`) plus a
creation pass: for each can-park func-value call site it finds every place each parking callee is created as a value
(a MakeClosure for a literal or a bound method, an operand for a named function or method expression) and classifies
the creating function: twinned (it can park through its own body, the generator's rule), plain, a closure in either,
or a package initializer. Windows, long parks only, runtime family excluded, no cuts.

| program | can-park func-value call sites | covered (every creation site in a twin) | partial | uncovered |
|---|--:|--:|--:|--:|
| net/http alone | 31 | 18 | 2 | 11 |
| a control server (a handler literal and a named handler in main, a logging middleware factory) | 31 | 18 | 3 | 10 |

Read from it:
- **The call side is the larger hole, and the review did not name it.** `(net/http.HandlerFunc).ServeHTTP`, the site
  every handler is reached through, CANNOT PARK under the generator's reach: its only park is the call through the
  func value, and that edge does not propagate. So it is never twinned, and no pair would ever be awaited there. N3
  needs a twinning reason on the call side too (9.3, fix 1').
- **Factories are real (fix 1).** The control's middleware literal is created in `logging`, which does not park:
  plain. So are `ServeMux.findHandler`'s and `NotFoundHandler`'s values. `main`'s two handlers are created in a twin.
- **Package initializers create some values** (`net/http.init`); they stay plain (9.3, fix 7).
- **Not read: the TypeScript closure.** Its checkout is not on this host, and B5's two cuts are named in the record
  but not as arguments. Re-reading it needs the clone at `ba29a56299` and its 23 modules: owed, with COORD's word on
  whether the net/http reading is enough to rule fixes 1 and 1'.

### 9.3 The must-fixes

1. **Factories.** A function that creates a can-park literal or method value gets a twin even when nothing in it
   parks: a SYNCHRONOUS twin, the same body with its pairs. A twin calls the twin of every callee that has one, so
   main's twin reaches `loggingꞏtwin` and the literal it returns is a pair. Refused functions (byref-param, reflike,
   unsafe, iterator) keep plain literals: the coverage loss, named at the literal (fix 7).
   1'. **Callers of func values.** A function that calls a func value of a delegate type that has a can-park pair
   anywhere in the program gets a twin, which awaits the call. The generator sees one assembly, so each assembly's
   package metadata records the delegate types it pairs, and a caller's assembly reads its references' records.
   This is what twins `HandlerFunc.ServeHTTP`.
2. **Method values.** A bound `x.M` is never cached per function. Its pair is made per value: the receiver is
   evaluated once into a temporary, with exactly the evaluation the plain emission does today (Go's panics at
   evaluation stay where they are), and both halves bind that temporary. A value receiver is copied once and both
   halves share the copy. Static functions and method expressions cache one pair per function. Benched (the
   bench's run 2, predictions written first): a bound pair costs +15 ns and +160 B over today's bound value on the
   JIT, +19 ns and +128 B under Native AOT, for a pointer receiver and a value receiver alike (the shared copy is not
   doubled); no arm over 43 ns.
3. **The unwrap readers, in full.** Section 4's list gains, per the review: `GoFrame.ReRaisedByTheDeferredDelegate`
   (`deferred.Method == catcher`); `Goroutine.Start`'s entry and "created by" names, here and in
   `Goroutine.Multiplexed`; `ж.PointerTokens.IsMethodValue`, where the unwrap must come BEFORE the method-value test
   or a pair-bound closure gains `-fm`, and its line 465; `GoReflect.FuncParamDims`; `GoTimer`'s diagnostic name.
   Not affected: `ж.Views` and the two `ConditionalWeakTable<Delegate>` in `ж.FieldRefBox` (field accessors).
4. **Resolve returns the pair.** A word recovered as a func must keep its twin (every ccgo indirect call in
   sqlite-cs goes through a word). Rule: a static function's row upgrades to its pair-bound delegate when one is
   minted, and `Resolve`/`Func<F>` return the row's delegate; a closure's per-instance word is minted from whichever
   delegate was read, so a pair-bound closure's word resolves to the pair. A word minted before any pair exists
   resolves plain until the first pair-bound mint.
5. **Wrappers at the call test.** Read at the tree: the converter emits no delegate over a delegate. Its four
   `new F(expr)` sites take a func literal (`convCallExpr.go:1199`), a declared function (`:1207`), a string (`:1092`)
   and a manual pointer type (`:1292`); a func variable converts through golib's `NilSafeDelegateConversion`, which
   rebinds over the same target, so a pair survives it. Only hand-written C# makes `new F(g)`; a twin's call through
   one takes the fallback. The call test stays one type test; golib's unwrap (a reader, off the hot path) walks the
   chain.
6. **go and defer on a func value in a twin.** `go f()` launches `GoFuncPair.TwinOf(f)` when `f` is pair-bound and the
   plain body otherwise: one launch helper, no name to rename. `defer f()` keeps today's plain call: a deferred call
   runs in the frame's synchronous unwind, so a park in it holds the carrier, named at the defer (fix 7). P1's
   `WaitGroup.Go` shape is the `go` case.
7. **Which site warns.** Only creation sites warn, never call sites: a can-park literal or method value created in a
   body that cannot have a twin (a refused function, a package initializer, a deferred call's operand) gets one
   warning at the literal, per the owner's default. The census bounds the count: net/http's uncovered creation sites
   are a handful, not every call.

### 9.4 The shoulds

- **S4 becomes a delta:** a pair costs at most 160 B over today's value: over A0 for a literal and over the bound
  method value for `x.M` (measured +128 AOT, +160 JIT in both; captures count).
- **P3 includes the typed twin.** Its retrieval is a cast to the twin delegate type, which is canonical per
  signature after a named type's conversion (a methodless named func type is its base delegate, seat N2); a ref or out
  parameter gets no pair (a twin cannot be async over it), named at the literal.
- **P1 gets numbers:** 10,000 goroutines parked behind func values hold at most 64 threads (the prototype reads 36
  to 43 with its injection budget); today they hold one thread each.
- **P2 is a gate on the converter, not the generator:** the two-seeded footprint reads 0 files and the no-regression
  check 0 goldens. P4's programs make can-park literals and method values in twin bodies and read each through
  reflect, `FuncForPC`, `%p` and the cookie, twins on and off; each read is made to fail once (the unwrap disabled).
- **S5 becomes a grep gate:** exactly one `Method.Name == "Invoke"` chase in golib.
- **Cost sites:** can-park literals in loops (`defer func(){ <-done }()`, `wg.Go`, `once.Do`, range-over-func yields
  that park) pay a pair per iteration; the review saw no cheaper lazy shape, and none is proposed.
- **Equality:** a static function's plain-made and twin-made values are no longer `Delegate.Equals`. No golib reader
  relying on it was found; the search was not exhaustive.
- **Not verified yet:** open generic delegates in twin bodies and trimming of the emitted pair classes; each is read
  in the code stage. The bench itself is now in the data folder (source, runner, both runs), so it can be re-read.
