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
