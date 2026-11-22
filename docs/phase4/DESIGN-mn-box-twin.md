# N2: the box twin for by-ref receivers and parameters

**Status: DESIGN, for COORD's review (2026-11-16). No code.** Order from COORD after stage (c): size it against the
closure's refusals and name its aliasing story for `this ref`. Sizing is the census of section 5, read on
MnHttpClosure (stage (c)'s closure, 186 packages) built as arm B at 2414b35286 with a scratch census (never
committed).

## 1. What is refused, and why it matters

The generator refuses a twin to a function with a by-reference receiver or parameter (`byref-recv`, `byref-param`).
C# cannot hold a `ref` across an await, and an async method cannot take one. A Go method with a pointer receiver
converts to `M(this ref T t, …)`, so most Go methods are in the set. Stage (c), arm B: 2,377 `byref-recv` refusals and
395 `byref-param`; **539 of them park** (479 and 60). The all-members interface rule moved the park set by 2 points;
this set caps it. Each refused parker is a blocking call: a carrier held for the park, answered by the hand-off.

## 2. What the converter and golib already give

- **A box overload exists.** For every pointer-receiver method, RecvGenerator emits
  `M(this ж<T> Ꮡt, …) { ref var t = ref Ꮡt.DerefOrNull(); return t.M(…); }`.
- **The twin shape is already proved.** A function with a Go pointer parameter is emitted as `F(ж<T> Ꮡp)` with the
  prologue `ref var p = ref Ꮡp.DerefOrNull();`, and is twinned today: Rule R derives `p` again after each await,
  because its root is never reassigned (C1, 79437adb79).
- **golib's pointers alias; they never copy.** A root box `ж<T>`, a field pointer `Ꮡx.of(T.ᏑF)` and an element
  pointer `.at<E>(…)` read and write the storage they were made from.

## 3. The design

### 3.1 The twin (generator)

For a refused parker `M(this ref T t, args)`, emit `Mꞏtwin(this ж<T> Ꮡt, args)`: M's body under the prologue
`ref var t = ref Ꮡt.DerefOrNull();`, rewritten as every twin is. A by-ref PARAMETER `ref U u` becomes `ж<U> Ꮡu` the
same way. The body is then exactly the shape C1 already handles: a ref local over a root never reassigned. The plain
method and its RecvGenerator overload are untouched. The twin is named by its box signature, so it never collides
with the plain `this ref` method.

### 3.2 The call (generator, in a twin body)

`x.M(a)` with a box twin is awaited as `await <box of x>.Mꞏtwin(a)` where the twin can name a box for `x` WITHOUT A
COPY. Census rows (section 5) in brackets:

| x, as converted | box | sites |
|---|---|---|
| a ref local over a box root (`ref var x = ref Ꮡx.DerefOrNull()`) | `Ꮡx` (the root RefAliases already tracks) | 143 |
| `Ꮡx.DerefOrNull()` / `Ꮡx.Value` as the operand | `Ꮡx` | 55 |
| a field of such a local (`x.f`) | `Ꮡx.of(T.Ꮡf)` | 8 |
| a package variable: a ref property over its static box (`cpuprof` => `Ꮡcpuprof`) | `Ꮡcpuprof` | 2 |
| a stack local of a struct type | none today: section 3.3 | 5 |
| a ref local not over a box, a field of a call's result, an `out` declaration | none: stays plain, named | 8 |

Where no box is named the call stays the plain blocking call it is today, with a report row and GO2CS0013-style
warning naming it. Nothing is ever copied out and back.

### 3.3 Stack locals (decision asked)

Go takes `&v` for `v.M()`. The converter keeps `v` on the stack when no pointer to it escapes, so nothing else can
reach it. The twin body (generated, separate from the plain one) may declare `v` as a heap local
(`heap<T>(out var Ꮡv); ref var v = ref Ꮡv.ValueSlot;`) and pass `Ꮡv`. One allocation per twin call that reaches
such a local; 5 sites on this closure. A first cut can leave them plain and named instead.

### 3.4 What follows from it

- **The cascade.** Most refused parkers are called only from other refused by-ref methods (`c.serve()` calls
  `c.readRequest()`): 408 of the 539 are not called from any twin today. Inside a box twin the receiver `t` is a ref
  local over the box root, the first row of 3.2, so the chain twins through.
- **Interface forwarders.** A pointer implementer's adapter forwards to the RecvGenerator box overload; with a box twin
  behind it, the twin generator's forwarder pass finds a twin to forward to, and stage (c)'s 658 sync forwarders
  (arm B) become real ones where the implementer parks.
- **The freeze (10.4).** The closed definition of "signature admits a twin" loses its by-ref receiver and by-ref
  parameter exclusions. Lifting an exclusion adds twins, which the definition allows.

## 4. The aliasing story (COORD's question)

What a twin holding a box sees when another goroutine writes through the same pointer:

1. **It holds the box, never a copy.** Between two awaits (one carrier step) it reads and writes through
   `ref var t = ref Ꮡt.DerefOrNull()`, the plain method's own access path.
2. **After each await it derives `t` again from the box** (Rule R). A write another goroutine made through the same
   pointer while the twin was parked is in the box's storage, so the twin reads it. That is Go's model: a pointer
   receiver shares memory, and nothing is copied at a park.
3. **Within one step** concurrent access is what it is in Go and in the plain code: a data race unless synchronized.
   The scheduler adds no interleaving inside a step (a twin is not preempted between awaits).
4. **A field pointer reads through its parent** (`Ꮡx.of(T.Ꮡf)`): a write to that field by another goroutine is seen.
   **To verify before code**: golib's element pointer over a SLICE must keep the backing array it was made from, as
   Go's `&s[i]` does after an append. No census row needs it; it gates any later row that would.
5. **The promoted stack local (3.3)** has no other alias by construction; boxing it is unobservable.
6. **A nil receiver.** `Ꮡt` may be nil, as in Go: `DerefOrNull` already panics at the first dereference the same way
   the plain method does, so the twin keeps Go's nil-receiver behaviour (a method that never dereferences runs).
7. **Not covered**: a hand-written `this ref` caller outside the generator (golib) keeps calling the plain method.
   That is 10.3's cost case, a held carrier, never a wrong answer.

## 5. Sizing (census, MnHttpClosure, arm B, 2414b35286)

- Refused parkers: 539 (byref-recv 479, byref-param 60). Called from twins today: 131, at 221 call sites.
- Of the 221: a box without a copy at 208 (94%: 143 ref locals over a box, 55 box operands, 8 fields, 2 package vars);
  5 stack locals (3.3); 8 named-plain (3 ref locals not over a box, 4 fields of a call's result, 1 `out` declaration).
- The cascade (3.4) reaches the other 408 through their own receivers, which inside a box twin are the first row.
- Expected size: up to 539 new twins plus callers that parked only through them; at stage (c)'s measured ~1.5 KB a
  twin, under +1 MB on the closure's 82.8 MB (arm B).

## 6. Predictions (before any code)

- PN-1 (80%): no new rewriter rule beyond the signature move and the call's box: C1 handles the prologue.
- PN-2 (75%): on the closure under arm B, twinned rises from 22.2% to 25-29%; go.*.dll bytes +0.5-2% over arm B.
- PN-3 (60%): the closure builds with no error outside the ref-across-await family on its first build.
- PN-4 (70%): MnHttpClosure prints Go's 9 lines; hand-offs fall below arm B's 112-116 (a parking pointer method parks
  as a twin instead of holding a carrier).
- PN-5 (85%): the 29-program sweep stays 29/29; plain output is byte-identical (the box twin touches only twins).

## 7. Stop lines

- SN-1: any site where the twin would need a COPY of the receiver to proceed. Stop: the design is wrong there.
- SN-2: a new error class (post first, as stage (c)).
- SN-3: arm B's bytes over P above +15%, or wall above 3x P.
- SN-4: an aliasing difference from Go found by a probe (a slice element pointer that follows a new backing array).
- SN-5: a change to plain emission (the plain method, its box overload, any non-twin file).

## 8. Decisions asked

1. Stack-local promotion (3.3) in the first cut, or named-plain?
2. `byref-param` in the same cut as `byref-recv` (same shape; 60 of 539)?
3. The forwarder consequence (3.4) in the same cut, or the forwarders stay sync until measured?

## 9. Revision 1, after the review (dated block, 2026-11-16)

**The review** (COORD's Fable sub-agent, at a7a52e0479): APPROVE WITH CHANGES; COORD adopted it. The core holds: the
twin holds the box and derives the ref again after each await, and the review found no receiver copy. The sizing and
three aliasing edges needed work. This block supersedes 3.2's table, 3.3, 5, 6 and 7 where they differ; 1, 2, 3.1,
3.4 and 4 stand, with 9.5's additions.

**Section 8, ruled.** (1) Stack locals stay named-plain in the first cut; revisit after this census. (2) `byref-param`
uses the same machinery as a SEPARATE measured step after receivers (the same train is fine). (3) Forwarders go in the
same cut, behind a switch read as an arm of its own.

### 9.1 The re-census (v2)

Scratch, never committed; reverted after (the tree read clean at a7a52e0479). MnHttpClosure, arm B, a7a52e0479. Three
changes from section 5's census, one for each of the review's sizing findings:

- **A hypothetical fixpoint.** The eligible receivers (9.3) are unrefused, every park mark is computed again from
  scratch, the classification runs over EVERY function twinned then (today's 2,999 and the new ones), and every
  refusal and mark is restored before the emission, so the build is today's (it built, and printed Go's 9 lines).
- **Calls through a ж<T> expression.** On this closure they do not read as Unbound (the reports' in-twins `unbound=`
  is 0 in every arm): `cc.t.disableKeepAlives()` BINDS, to RecvGenerator's box overload `M(this ж<T>)`, which is no
  candidate. Section 5 missed them for that reason. The census follows the overload to the `this ref T` method it
  forwards to: 218 such calls.
- **The currency guard** (9.4) applied to every ref-local row.

| the by-ref receiver, as converted | box the twin names (no copy) | today | new |
|---|---|---|---|
| the box twin's own receiver `t` | `Ꮡt` | 0 | 352 |
| a ж<T> expression (a field `cc.t`, a local pointer `p`, a call's result) | its value, read once at the call | 183 | 35 |
| a ref local over a box, CURRENT (9.4) | `Ꮡx` | 182 | 59 |
| a field of either of those (`x.f`, `t.f`) | `Ꮡx.of(T.Ꮡf)` | 11 | 11 |
| `Ꮡx.DerefOrNull()` as the operand | `Ꮡx` | 3 | 1 |
| a slice element of a field of the own receiver (`t.s[i]`) | the element pointer over `t.s` at i, index evaluated once | 0 | 6 |
| a package variable (a ref property over its static box) | `Ꮡname` | 2 | 0 |
| a field of an array element of a package variable's field | `Ꮡname.of(…).at(…).of(…)` | 0 | 3 |
| **boxable without a copy** | | **381** | **467** |
| a stack local (named-plain, ruled) | none | 4 | 10 |
| a ref local not over a box | none | 1 | 4 |
| a field of a value parameter | none | 1 | 0 |
| **named-plain** | | **6** | **14** |

848 of 868 call sites (97.7%) get a box without a copy. The currency guard rejected none (no ref local over a box
fails Rederivable on this closure). Section 5's 221 was an undercount by the factor the review expected: the calls
inside the newly twinned bodies (481) and the box-overload calls (218).

### 9.2 Projected size

Twinned 2,999 -> 3,718 (+719; 22.2% -> 27.5% of 13,502). Eligible receivers that park: 479 -> 610 (131 more park once
their callees have twins). **Refused parkers 647 -> 172**, the residue named: byref-param 63, unsafe 55, reflike 43,
native 5, await-in-lock 3, host-not-partial 2, ref-unrederivable 1.

### 9.3 Who is eligible (the review's finding 5)

A box twin SHARES its receiver. An `in`, `out` or `ref readonly` receiver, or a value-method-set forwarder marked
`[GoCopyBound]`, is bound through a COPY in Go, so it is never boxed: the box twin is for a plain `ref` receiver
without `[GoCopyBound]` only. The split on this closure: byref-recv 2,377, all plain `ref`, none `[GoCopyBound]` (the
attribute is written only on sibling forwarders, which are not candidates), so the rule excludes nothing here and
stays in the definition. byref-param 395: `ref` 380 (56 park), `out` 14 (4 park), both 1: the later step.

### 9.4 The currency guard (the review's finding 3)

A ref local names its box only when its declaration is EXACTLY `ref var x = ref Ꮡx.DerefOrNull();` or
`… = ref Ꮡx.Value;` over a plain root AND `Rederivable(Ꮡx, x)` holds (RefAliases): a root reassigned before `x.M()`
would hand M a different object than `x` names. Otherwise the call is named-plain. The same applies to a use AT the
awaited call itself, which UnrederivableLive does not see: the guard is read at the call. **Shared-member chains**
(the review's finding 4: `ref var x = ref t.next.DerefOrNull()`, which Idempotent would re-read through a replaced
`t.next` after a park, where Go read it once): 0 in the hypothetically twinned bodies, and 0 in the source (a grep
control finds 1,266 plain-root declarations in net/http alone and none rooted in a member). If one appears, the twin
snapshots the intermediate box into a local once, or the function is refused.

### 9.5 Aliasing, two edges answered (the review's finding 6, and 3.3's nit)

- **Element pointers** keep the backing array they were made from: ElemRefBox's fast arm captures `m_backing` at
  creation (ж.ElemRefBox.cs:78-81, 125-126, 213-229), as Go's `&s[i]` keeps the old array after an append; FieldRefBox
  reads through its parent. SN-4 narrows to the FOREIGN arm (a pinned buffer, a null source, native-backed storage).
- **Stack-local promotion** (3.3) is not unobservable after all: it adds an allocation per twin call, which
  `testing.AllocsPerRun` counts. Named-plain in the first cut, as ruled.
- GC and lifetime are as designed (the box is a GC root while the twin holds it).

### 9.6 Predictions (replacing 6)

- PN-1 (80%): no new rewriter rule beyond the signature move, the call's box and the guard: C1 handles the prologue.
- PN-2 (75%): arm B twins within 3% of the projected 3,718; refused parkers at most 180, each named by reason.
- PN-3 (70%): go.*.dll bytes +0.8 to +1.5 MB over arm B's 82.8 MB (about 1.5 KB a twin).
- PN-4 (60%): MnHttpClosure prints Go's 9 lines; hand-offs, median of 5 runs, at most 105 (arm B read 112-117 over
  3 runs, DATA-mn-stage-c.md).
- PN-5 (85%): the 29-program sweep 29/29; plain emission byte-identical.
- PN-6 (80%): named-plain at most 25 call sites (the census reads 20).

### 9.7 Stop lines (replacing 7)

- SN-1 (mechanical): a generator diagnostic for any box expression outside the enumerated forms of 9.1; any receiver
  COPY is a stop.
- SN-2: a new error class (post first).
- SN-3: arm B's bytes over P above +15%, or a wall above 3x P.
- SN-4: an aliasing difference from Go in the FOREIGN element arm (9.5).
- SN-5: a change to plain emission (the plain method, its box overload, any non-twin file).
- SN-6: the ALIASING PROBE, red first: a behavioral program where another goroutine writes through the same pointer
  while the twin is parked, and the twin reads the write after the park -- through a root box, a field pointer, a
  slice element after an append, and with a nil receiver -- WITH a negative control: a deliberately copying twin must
  turn it red. Green with N2 and red with the control, or stop.
- SN-7: named-plain above 25 call sites (the re-census figure, 20, plus a margin).
- SN-8: any deadlock or park regression in the 29-program sweep.
- SN-9: GenTests not green.

### 9.8 The order of the cut

Receivers first (plain `ref`, no `[GoCopyBound]`), with the currency guard and SN-1's diagnostic; the forwarders in
the same cut behind a switch, read as an arm of their own; `byref-param` after, as its own measured step. Red first:
the aliasing probe (SN-6) and a GenTests unit per box row of 9.1.

## 10. Before the measured arms: the re-review's changes and the forwarder prediction (dated block, 2026-11-16)

COORD's re-review of revision 1 cleared the cut to code, with two must-fixes, four should-fixes and a nit. Recorded
here before any arm of the cut is read.

- **9.1, the post-pass (should-fix).** The v2 census did NOT run UnrederivableLive or await-in-lock on the
  hypothetically twinned bodies: its 3,718 counts twins before either refusal. The cut runs both, and the
  shared-member detector below, after the fixpoint on every twin, so PN-2 is read against 3,718 less whatever those
  three refuse.
- **Must-fix 1, shared-member chains.** A twin with a ref local whose source crosses a ж-typed member
  (`ref var m = ref n.next.DerefOrNull()`) is refused by name, `ref-shared-member`; a GenTests unit was red first.
- **Must-fix 2, the value-receiver forwarder.** The box overload is followed only to a plain `ref`,
  non-`[GoCopyBound]` method of the same name, matched by its FULL signature (every other parameter by ref kind and
  type, and the return type); a value method's box overload, which copies, is never followed. A GenTests unit was
  red first, and the probe carries a value method called through a pointer: the write during its park is not seen,
  and its own mutation does not reach `*p`.
- **The field row, as built.** `parent.Ꮡf` (the field-access generator's property) where the type is recorded
  `[GoFieldAccess]`, else `parent.of(T.Field‿Tokens.Ꮡf‿)`, the token every converted struct carries (TypeGenerator
  emits it by rule): the same FieldRefBox either way. **SN-1, as built:** every box form is bound speculatively at
  the call and must type as ж<the receiver's type>; a form that does not is named-plain (a `// P` report row), never
  emitted. The forms are enumerated by construction, so a diagnostic for a form "outside" them could never fire; the
  P rows are SN-7's count, including those of a function that parks nowhere else (it gets no twin, so no rewriter sees
  the call).
- **The nit.** An element under a package variable (`name.f[i]`, 3 sites) is named-plain in this cut.
- **SN-6, widened.** The probe also covers a nil pointer's field (`nh.c.M()` panics as Go) and the `t.s[i]` row (an
  element of the own receiver's slice field).
- **PN-4, the base (should-fix).** Arm B's hand-offs are read again, 5 runs, at the cut's base, beside the cut's 5.
- **Found by the first closure arms, each cured with a GenTests unit red first** (the arms were read again after):
  (1) arm N did not build, 4 CS9217 in runtime's `gcWork.putObjBatch`, `(w.wbuf1, w.wbuf2) = (w.wbuf2, getempty())`:
  the rewriter's ref-local tests (a store through a ref, a ref read after the call) bind with the PLAIN method's
  model, where a box twin's receiver is the `this ref` parameter, not the ref local it becomes; it now counts as one,
  so such a call spills or stays plain (ref-blocked), as C1 does for any ref local. (2) arm O, the switch off, was
  not the base: sync's hand-written `Lockꞏtwin(this ref Mutex)` sits beside stage (c)'s interface-forwarder twin
  `Lockꞏtwin(this ж<Mutex>)`, which read as a box twin, so calls the base awaits in place were routed through boxes
  and 5 left plain. A method with a twin of its own shape now keeps the base form, and with the switch off no
  referenced box twin is known at all. (3) the shared-member detector refused runtime's `traceRegionAlloc.alloc` for
  a HEAP local, `ref heap<ж<…>>(…a.current…, out var Ꮡblock)`, which is derived again from its own box, never through
  the member: heap locals are exempt. So the census's "0 shared-member chains" stands. (4) arm N, once built, lost
  528 of the base's twins (syscall 199 -> 1, and every package above it): a call through RecvGenerator's box overload
  (`Ꮡx.M()`) was remapped to the `this ref` method and, where that method got no box twin, left PLAIN, though the
  base awaited it through the overload's own twin (stage (c)'s forwarder twin). Such a call now parks as at the base.
- **PN-7 (55%), the forwarders (should-fix).** With GoTwinsBoxForwarders on, arm B's 658 sync forwarders fall to
  450-600 (the rest forward to implementers that cannot park, or to refused ones); with it off they stay at the
  receivers-only arm's count. Hand-offs, median of 5: forwarders on at most forwarders off.

## 11. Measured (dated block, 2026-11-16)

The cut's arms, scores and stop lines: [DATA-mn-n2-receivers.md](DATA-mn-n2-receivers.md). In short: twinned 2,999
-> 3,806, refused parkers 175, +1.18 MB, Go's 9 lines in every run, the probe green with N2 and red with the copy
control; PN-1 and PN-4 missed (four rules found by the arms; hand-offs rose as more goroutines run on carriers).
`byref-param` (395, 63 parking) is the next measured step, as ruled.

## 12. byref-param: design, census and predictions (dated block, 2026-11-16, before any arm)

**Where the parameters come from.** The converter's ref-lowering pass (refLoweringAnalysisOperations.go) lowers a
pointer parameter `p *T` of an unexported package-level function to `ref T p` when every use only dereferences or
forwards it. Go still passes a POINTER: the callee shares the caller's object. So the twin takes the parameter's box,
exactly as a receiver's: `fꞏtwin(..., global::go.ж<T> Ꮡp, ...)` under `ref var p = ref Ꮡp.DerefOrNull();`, which
Rule R derives again after each await. Nothing new in the rewriter beyond what N2 made for the receiver.

**Eligible:** a plain `ref` parameter, any position, several per function (each boxed on its own; two may name one
box, as two Go pointers may). `out`, `in` and `ref readonly` stay refused, named `byref-out` (the 4 parking `out`
functions are all hand-owned golib-adjacent files: deepequal_impl, value_impl, syscall_windows_impl).

**Census** (closure arm N at 3b52c5c216, and src/core's call sites, read only): 63 refused byref-param functions park
(58 with one `ref`, 1 with two, 4 with one `out`; none is a method). Their 194 by-ref arguments, by shape:

| argument, as converted | box the twin is handed | count |
|---|---|---|
| `ref (x).DerefOrNull()`, x a ж local or param | `x`, read once at the call, as Go reads it | 59 |
| `ref (Ꮡx).DerefOrNull()` / `ref Ꮡx.Value` | `Ꮡx` | 41 |
| `ref x`, a bare name | own box `Ꮡx`, a current ref local's root, or none (stack) | 64 |
| `ref (e.f).DerefOrNull()` (a ж field), other | `e.f` if side-effect free and bound (SN-1), else none | 21 |
| `ref x.f` (a field) | the field pointer (9.1's row) | 3 |
| `out var x` | (refused: byref-out) | 6 |

**The instrument, split first** (56baaf2de4, COORD's order; MnHttpClosure, 5 runs): `handoffs` counts a
carrier-mounted goroutine's plain block, `thread-blocks` a goroutine with no carrier blocking its own thread. Base O:
110-115 + 81-86 (about 197 plain blocks), peak OS threads 57-61. N2: 132-135 + 0 (about 133), threads 39 in all five.
N2 moved every off-carrier block onto a carrier and cut the total by a third; PN-4 read only half of it.

**Predictions** (scored on MnHttpClosure, arm N + byref-param, 5 runs, against arm N):

- PB-1 (70%): the plain-block TOTAL (handoffs + thread-blocks), median, falls from about 133 to 105-128; thread-blocks
  stay 0; peak OS threads at most 39 in every run. THE HEADLINE.
- PB-2 (70%): twinned 3,806 -> 3,875-3,990 (the 59 ref-parameter parkers and the callers they unblock).
- PB-3 (65%): refused parkers 175 -> 110-125 (byref-param 63 -> at most 6; a few new parkers surface behind them).
- PB-4 (75%): go.*.dll +0.05 to +0.35 MB over arm N.
- PB-5 (70%): byref-param's own named-plain call sites at most 30 (the bare-name stack locals and call results).
- PB-6 (85%): the 30-program twin sweep 30/30; Go's 9 lines in every closure run.

**Stop lines:** SB-1 any parameter COPY (a twin holding `T` where Go holds `*T`); SB-2 a new error class; SB-3
go.*.dll over P above +15%; SB-4 the probe's new rows (an unexported pointer-parameter function parking while another
goroutine writes through the pointer) not green with byref-param and red with GoTwinsBoxCopy; SB-5 a change to plain
emission; SB-6 GenTests red; SB-7 byref-param named-plain above 40; SB-8 a sweep regression.

**Order:** after the code review's changes land (COORD's ruling): the probe rows red first, the units red first, the
switch `GoTwinsBoxParam` (default on) beside `GoTwinsBoxRecv`, then the arms from a per-run clone.

### 12.1 Revision 1, after the design review (dated block, 2026-11-16)

COORD's Fable design review of section 12 (458941b71b, the implementation at cb38d22455): APPROVE WITH CHANGES. The
callee side carries over (it holds the box; Rule R derives the parameter again). The call side was under-specified,
and a silent copy enters there. This block supersedes section 12 where they differ.

**The call side, as rules.**

- **Only enumerated aliasing forms hand a box (must-fix 1).** A box argument is one of: (a) `(E).DerefOrNull()`,
  `E.DerefOrNull()` or `E.Value` with E a side-effect-free ж expression (a name, a field chain, a `~` deref) or the
  element pointer `Ꮡ(x, i)`: E itself; (b) a bare name, decided by its SYMBOL (must-fix 3): the twin's own boxed
  parameter (`Ꮡp`), a ref local passing BoxRoot + Rederivable + the re-point check (its root), a heap local (`Ꮡv`),
  or a static ref property (`Ꮡname`), never a stack local and never an unrelated `Ꮡx` that happens to be in scope;
  (c) 9.1's field and element rows, including ruling 10.4's nil-checked `nonnil(ref e).f` (the field pointer from
  e's box). Anything else, `builtin.Ꮡ(v)` above all (it COPIES v into a new box), is never handed.
- **All or nothing per call (must-fix 2).** A call awaits the twin only if EVERY by-ref argument names a box;
  otherwise the whole call is named-plain. One twin shape per function (every plain-ref parameter boxed), never 2^n.
  ShouldAwait and BoxCallable generalise from one box (`site.BoxOf`) to one per by-ref position.
- **Evaluated once (should-fix 5).** Each box value is SPILLED to a temp ahead of any await in the argument list and
  never read again: `f(p, <-ch)` then `p = p.next` hands the p read before the receive; `Ꮡ(s, i)` takes i at that
  point too.
- **Nil (should-fix 4).** Go panics at `&e.x` with e nil, at the call, with later arguments unevaluated. The box form
  (`e.Ꮡx`, `e.of(…)`) faults there too, as golib's nil dereference, in argument order. A nil box value
  (`ref ((ж<T>)default).DerefOrNull()`) is handed as is; the callee's prologue faults at its first dereference, as
  Go's does.
- **Thunks.** A go literal's call is followed when the literal is twinned: `goǃ((ᴛ1, ᴛ2, ᴛ3) =>
  awaitLegacyCancel(ᴛ1, ᴛ2, ref ᴛ3.DerefOrNull()), …)` (2 sites, net/http) hands ᴛ3, bound once at the go statement
  as Go evaluates `&req`. A deferred call runs plain (`defer(ᴛ1 => unwindm(ref ᴛ1.DerefOrNull()), …)`, 3 sites): not
  followed, not counted named-plain.
- **Carried over for every boxed parameter (must-fix 3):** the refusal when the body already names `Ꮡ<name>`, and
  Local()'s keyword escape.

**Scope facts the design relies on (nit 8).** Ref-lowering never applies to methods, exported functions or func
values (its X5 class), so a receiver with a by-ref parameter, a cross-assembly callee, an interface member and a
delegate cannot arise from the converter: every callee is an unexported function of the same compilation, found
among this run's candidates. A hand-written `this ref` method with another `ref` parameter stays refused by name
(`byref-mixed`). A generic `f<T>(ref T p)` gets a unit for its twin and its call binding. The four parking `out`
functions are all hand-owned `_impl.cs` companions: syscall_windows_impl.cs `lookupExtensionFunction`,
deepequal_impl.cs `tryScalarSliceEqual` and `tryNestedSequenceEqual`, value_impl.cs `tryConvertOnlyShape`.

**Rules already known to be needed (should-fix 6; section 12's "nothing new in the rewriter" was false).** The
rewriter's ref-local test that cured CS9217 knows only the receiver (Ordinal 0 of an extension); BoxReceiverOf
refuses every other ref parameter; ShouldAwait and BoxCallable read one box. All three generalise. (IsBoxTwinOf's
metadata lookup does not: no byref-param callee is in another assembly.)

**Census, split (should-fix 7).** The 64 bare names: 55 forwarded parameters (boxable when the enclosing function is
boxed too), 4 heap locals (boxable), 5 stack locals (named-plain: two deepequal_impl locals, two copies of a
traceback local, a composite-literal temp in crypto/x509, `parsePublicKey(ref ᴛ1)`). The 21 "other": 13 hand a ж
value read once (a field of a local or parameter, a `~` deref, an element pointer `Ꮡ(x, i)`), 8 are nil-checked
fields (`nonnil(ref priv).PublicKey`). So of 194 by-ref arguments: 6 `out` (refused), 5 named-plain, 183 boxable.
This is a census of today's call sites, NOT a hypothetical fixpoint: section 5 counted that way and 9.1 found it
short by about 4x once callers twinned in turn. PB-2 and PB-3 are widened for it.

**Allocation (nit 9).** A field pointer (a FieldRefBox) or an element pointer the twin's call builds allocates per
call where the plain `ref e.f` / `ref xs[i]` does not. On today's sites that is the 11 field arguments (8 nil-checked,
3 plain); the 5 element arguments are `(Ꮡ(x, i)).DerefOrNull()`, whose box the PLAIN call already allocates.
`testing.AllocsPerRun` would see it on a twinned path; the closure runs no test, so the exposure is named here and
read later by a `-tests` row under twins.

**Predictions, revised** (PB-1, PB-4 and PB-6 stand):

- PB-2 (65%): twinned 3,806 -> 3,875-4,100.
- PB-3 (60%): refused parkers 175 -> 105-130.
- PB-5 (75%): byref-param's own named-plain call sites at most 12 (the 5 stack locals and what the cascade adds).
- PB-7 (60%): at most two rules beyond the three named above, each with a unit red first.
- PB-8 (80%): every probe row green with byref-param and red under BOTH controls.

**Stop lines, revised.** SB-1: any box ARGUMENT not produced by an enumerated aliasing form above (a call-site copy),
or any parameter COPY in the callee. SB-4: the probe rows below, each a callee that writes AFTER its park while the
caller reads AFTER the return, green with byref-param and red under GoTwinsBoxCopy (the callee copies) AND under a
new call-site control, GoTwinsBoxArgCopy (each handed box replaced by a fresh copy `builtin.Ꮡ(arg)`). SB-7: named-
plain above 20. The rest stand.

**Probe rows (SB-4), one per boxable shape:** `(x).DerefOrNull()` (a pointer local); a forwarded parameter; a heap
local (`&v` of a local whose address escapes); an element `&s[i]`; a field `&e.x`; a ж field `f(e.p)`; two over one
storage `f(&s, &s.x)`; an array local and its element `f(&a, &a[1])`; a nil field `f(&e.x)` with e nil, under
recover (Go's line, at the same point); a later parking argument `f(p, <-ch)` with `p` reassigned meanwhile.

**Order.** Units red first (written at 458941b71b's successor: P1-P6), this block, COORD's GO on it; then the code,
the probe rows red first under both controls, the arms from a per-run clone of equal path length.

### 12.2 As built, after the code review (dated block, 2026-11-17)

P3's Fable review of the byref-param code (1ac7dcf40f..dd7627dc97): APPROVE WITH CHANGES, no must-fix. This block
records where the code departs from 12.1, as ruled. It supersedes 12.1 on these points only.

- **Evaluated once, by blocking, not by a spill (G's ruling 1a, 2026-11-16).** 12.1's "each box value is SPILLED to a
  temp ahead of any await in the argument list" is not built. A by-ref argument earlier in the list keeps every later
  awaitable in place, plain (nested ones too), so each box is read where the plain emission reads it: unit P13.
  **Cost:** that later call blocks its carrier, handing its P off as a thread park does, instead of awaiting. On
  MnHttpClosure at bf4670ec47 the count is **5** calls. All five are interface calls in a later argument: net/http
  fs.cs:850 and 862 (2), and crypto/tls handshake_client.cs:848 and handshake_server.cs:831. The spill step stays open.
- **Form (a) drops `E.Value` (P3's finding 5).** `ref E.Value` faults at the call when E is nil, while a handed box
  faults at the callee's first dereference, possibly after a park. Such a call is named-plain (unit P16). The
  converter emits `(p).DerefOrNull()` for a pointer argument; `ref E.Value` comes from hand-owned code.
- **A heap local is refused only when re-pointed** (`v = ref ...`) or its box is reassigned (`Ꮡv = ...`). A
  whole-value store `v = ...` writes through the box, which still names v (P19, P20; finding 7).
- **By symbol (finding 6):** `nonnil` and `heap` bind to golib's builtins, and a package variable's `Ꮡname` binds to
  the static of the property's own type.
- **Thunks (G's ruling 2):** a converter go wrapper over a box twin launches the twin when every argument is what the
  go statement evaluated. That means each by-ref argument is `ref ᴛk.DerefOrNull()` over a literal parameter, and each
  by-value name is a literal parameter, a constant or a type member (finding 3). Otherwise it stays plain and warns
  `ref-wrapper`.
- **The later-parking-argument probe row** (`f(p, <-ch)`) moved to src/tools/mnspike2/MnLaterArg (COORD 2026-11-16
  15:57Z). gc reads p after the receive, while the plain emission reads it before. That is a pre-existing divergence
  (a .5 BOARD row), and byref-param equals plain there.

### 12.3 Step 1, the two-level field chain, and the re-read's notes (dated block, 2026-11-17)

- **The chain.** An argument `nonnil(ref e).f.g…` (Go's `&e.f.g`) hands FieldBox applied once per level over e's box,
  outermost first: `Ꮡe.of(F.Field‿Tokens.Ꮡf‿).of(G.Field‿Tokens.Ꮡg‿)`, or the `Ꮡf.Ꮡg` members where the converter
  recorded the type. Every level but the last must be an instance field of struct type, so each is a field of e's own
  object. The nil check stays on the first level only: the first token faults at a nil e, as Go panics at the outer
  nil, and a deeper level is not a pointer and cannot be nil. A chain through a POINTER field is not this shape; its
  second level is a dereference, and it stays named-plain (unit P22).
- **Where the converter emits it.** Inside a byref-param body, where the pointer is the ref-lowered parameter
  (`chainRead(ref nest n)` emits `readCell(ref nonnil(ref n).h.c)`). Over a plain pointer local the converter emits
  form (a) instead (`(nc.Ꮡh.Ꮡc).DerefOrNull()`), which was already handed. In the corpus the one row is x509's
  parsePublicKey `nonnil(ref @in).TBSCSR.PublicKey`, one of PB-5's 13.
- **Probe rows.** param-chain (form (a), 16), param-chain-param (the nonnil chain in a twin, 17) and param-nil-chain (the
  outer nil, after param-nil-field, so neither control reaches it). Before the rule the twin arm read box plain=2 (the
  two chain calls); after it plain=0. Both controls read param-chain 1 and param-chain-param 16.
- **After P3's delta read (same day).** Row param-chain-ref (18) runs the chain over structs that hold reference fields
  at both levels (a slice and a string beside the nested struct): twins EQUAL Go, both controls read 1. P22 is renamed
  for what it asserts (the pointer-field chain stays named-plain) and asserts its `|ppS|` row.
- **P3's re-read, note 4 (coverage).** A by-value `nil` in a by-ref wrapper is a static readonly field, neither a
  constant nor a bound parameter, so that thunk stays plain and warns ref-wrapper (unit
  AThunkWithAByValueNilStaysPlainAndWarnsByName). No such wrapper exists in src/core.
- **Step 2 is a question to G.** The three casLoop rows `ref ptr.Value.Value` are not a box read through a box:
  `ptr.Value` is ж's `ref T Value` over golib's `uintptr` STRUCT, and the second `.Value` is that struct's `nuint` field.
  No ж<ж<T>> `.Value.Value` shape exists in src/core. casLoop's only await site is a delegate call that its callers'
  static lambdas never park in.

### 12.4 The spill, resumed (dated block, 2026-11-19)

COORD's step 3 (2026-11-19 17:44Z): the 5 inner calls of G 1(c) that blocked instead of awaiting are a correctness gap
in this design; P13 stays the control. This block supersedes 12.2's first bullet on the cost it names: all 5 now await.

**Rule 1, the box argument.** A by-ref argument that a box call hands as a box is a value in the rewritten call. A
later awaitable in the same argument list therefore no longer counts it as a reference held across its await
(RefLikeBefore). C# evaluates the box once, in argument order, and keeps it in a compiler temp over the await: that is
12.1 item 5's spill, done by the compiler, with no temp of ours. A by-ref argument with no box (named-plain) keeps its
`ref`, so a later call stays plain there (CS8178). Units P25-P27 and P29 were red first; P28 is a guard.

**Rule 2, a call inside a moved call.** When the walk up from an awaited call crosses the argument list of an enclosing
call that itself moves ahead of its statement (`var ᴛ = await outer(…)`), the walk stops there. Nothing outside the
moved call, such as a store through a ref local, is evaluated before this await, and only the moved call's own text is
scanned for a later ref read. This rule is not limited to box calls. crypto/tls's
`hs.masterSecret = extMasterFromPreMasterSecret(c.vers, ref (hs.suite).DerefOrNull(), preMasterSecret,
hs.finishedHash.Sum())` (client and server) is the case: the box call moved already, and Sum() was judged against the
original store. Units P30 and P31 were red first; P32 is a guard.

**P13, the control.** The box is still read first, where the plain emission reads it, so the f(p, <-ch) divergence
(MnLaterArg) is unchanged and nothing moves ahead of the statement. One assertion changed: its receive now awaits in
place.

**Probe row** `param-later-call`: `readCellK(pk, parkK())`, where parkK parks and readCellK parks again and reads the
write made meanwhile. Go 1903; twins 1903 (3 of 3 equal to Go's whole output); plain 1903; GoTwinsBoxCopy and
GoTwinsBoxArgCopy 103, exit 2 as before, every other row unchanged. The row is not red before the spill: a blocking inner
call gives the same values. The units are the red-first evidence; the row guards the box read order.

**Arms** (windows/amd64 count, MnHttpClosure merged at 870b0f04ad, 185 reports, rc 0, 0 errors in every arm):

| arm | tip | twinned | await | refused | ref-blocked |
|---|---|---|---|---|---|
| M-P | 870b0f04ad | 3,946 | 10,165 | 109 | 129 |
| S-P | rule 1 | 3,946 | 10,168 | 109 | 126 |
| R-P | rules 1+2 | 3,946 | 10,171 | 109 | 123 |
| M-N | 870b0f04ad | 3,844 | 9,665 | 169 | 127 |
| S-N | rule 1 | 3,844 | 9,665 | 169 | 127 |
| R-N | rules 1+2 | 3,844 | 9,666 | 169 | 126 |

Rule 1 moved net/http only (fs.cs, 3 calls). Rule 2 moved crypto/tls (the 2 Sum() calls) and one site outside G's
five: encoding/asn1's `(m[i], err) = makeField(v.Field(…), parseFieldParameters(t.Field(…).Tag.Get(…)))`, whose
`t.Field` now awaits inside the moved makeField call; it moves in N too, since it holds no box. S-N's reports are
byte-identical to M-N's. Every prediction of both posts was met (S2 at its band's edge, with the 2 tls calls named).

**Base, for the record.** Step 2's O band came from the 11-16 O arm at 1ac7dcf40f, before param boxes; one discriminating
arm at the pre-merge tip read 3,102 / 7,615, as predicted for that reading. The merged O baseline is 3,142 / 7,818 / 584.

### 12.5 PB-5: a stack local becomes a heap local in the twin (dated block, 2026-11-19)

COORD's GO (2026-11-19 20:05Z): twin-only, as Go heap-allocates an address that escapes. A STACK local whose reference
reaches a box twin, as an argument of a byref-param twin or as a receiver, is declared in the twin the way the
converter declares a heap local: `ref var x = ref global::go.builtin.heap<T>(init, out var Ꮡx);`. The call hands Ꮡx,
and Rule R derives x again after each await (`x = ref Ꮡx.ValueSlot;`). The plain method is untouched: the generator
adds twin files and never edits a source body. Gated on GoTwinsBoxParam, so arm N is unchanged (T-N's 185 reports
byte-identical to R-N's).

**Refused, named-plain with the reason in the `// P` row's argument text:** `captured` (a lambda or local function names
it; a ref local cannot be captured), `declaration` (not the one variable of a local declaration directly in a block of
the function), `loop` (declared in a loop body; fresh-per-iteration aliasing is refused in this first cut), `name` (Ꮡx
is taken), and `shadow` (another local of the same name: the promotion is keyed by name). `shadow` is a fifth reason
beyond the four in the plan; it guards the name key. On MnHttpClosure `loop` fires 3 times (dnsmessage message.cs
GoString `a`) and `declaration` 5 times (runtime's deconstructed `sΔ` in mcentral, mgcsweep and mheap). Two defects were
found by the arms and fixed, each with a unit red first: a receiver row carried no reason (0b03ace1fc, P39; arm T2-P
showed the 8 reasons with every count unchanged), and the loop test missed the converter's range loops,
`foreach (var (_, v) in range(…))`, a ForEachVariableStatement (5303b1777e, P40; T2-P read `a (shadow)`, arm T3-P reads
`a (loop)`, every count unchanged, only dnsmessage's report changed, as predicted before the run).

**Cost (named as nit 9 is):** one box allocation per twinned call of each function that declares a promoted local.
Plain paths are unchanged. `testing.AllocsPerRun` would see it only on a twinned path. M:N is opt-in, so no banked row
moves.

**Units** P33-P38, red first. Four older fixtures used an uncaptured stack local as their unboxable example (P3's
runStack, P3's review runMixed, P28's runStack, OnlyUnboxed's onlyF); each now captures it in a lambda, so they keep
their intent. **Probe row** `param-stack`: a stack cell handed to bumpCell, which parks and then adds 20. Go 21; twins
21 (3 of 3 equal to Go's whole output); plain 21; GoTwinsBoxCopy and GoTwinsBoxArgCopy 1. Before PB-5 the call was plain
and the controls could not reach it.

**Arm T-P** (ea5cae22f7) against R-P: twinned 3,946 -> 3,960; await 10,171 -> 10,207; refused 109 unchanged; box twins
632 -> 633, calls 1,203 -> 1,219, P rows 31 -> 16; rc 0, 0 errors. The prediction (P rows 22, calls +9+-2, twinned +0 to
+6) MISSED on all three counts, for one reason: it counted only PB-5's 9 byref-param rows, while the rule covers every
stack local a box twin is handed, receivers included. Six more rows moved, all stack locals: net dial.cs Listen and
ListenPacket `lc` (a receiver), crypto/x509 oid.cs `o`, net/http transport.cs cleanFrontNotWaiting `q` x2 (a copy of a
map value, as Go's `q := m[k]` is), and runtime mcentral.cs sweep `ss`. The 14 new twins are functions that parked only
through those calls: choosePivot, DeepEqual, deepValueEqual, ParseOID, net Listen, ListenPacket, tls Listen, traceback,
tracebacktrap, traceback1, and x/text bidi's resolvePairedBrackets, newParagraph, run and Line. The 16 rows left are
casLoop x3 (ruled no code), chunked.cs `bw.Value.Writer.Value`, filetransport.cs `t.fh`, runtime mcache x3 (an element
under a package variable), and the 8 refused above.

**The seat's disclosure list (as of 2026-11-19; M:N is opt-in, so none moves a banked row):**
- PB-5's box per twinned call of a function that declares a promoted stack local (this block).
- 12.1 nit 9's box per call for a field or element pointer the twin builds.
- The f(p, <-ch) divergence (MnLaterArg): gc reads p after the receive, the converter before; byref-param equals plain.
- RecvGenerator CS1620 on a hand-written `this ref` method with another `ref` parameter (byref-mixed).

**G's review of 12.4 and 12.5 (dated block, 2026-11-20).** G concurred with both blocks and named two unit gaps; no
design changed. Both are closed with units that went red when their mechanism was neutered and green when it was restored
byte-identical:
- GAP 1, a promoted local whose initializer parks (`x := parks(); bumpCell(&x)`). P41: the twin declares
  `ref var pi = ref global::go.builtin.heap<nint>((await parksIꞏtwin()), out var Ꮡpi);`, so the await completes before
  the ref exists, hands `Ꮡpi`, and Rule R derives the ref again after the call's await. Red when the declaration took
  the unvisited initializer (a plain `parksI()`). Probe row `param-stack-init` in MnBoxAlias (`si := cell{v: parkK()};
  bumpCell(&si)`): Go 23; twins 23 and plain 23 (3 of 3 equal to Go's whole output); GoTwinsBoxCopy and
  GoTwinsBoxArgCopy 3, as predicted.
- GAP 2, the `shadow` refusal. P42: two locals named `sh` in sibling scopes, one handed to the box twin; neither becomes a
  heap local and the P row reads `|bump|sh (shadow)`. Red when the refusal returned null: the twin then promoted `sh`.

## 13. HANDOFF to P4, a brief (dated block, 2026-11-16)

Owner ruling, as amended: M:N works on account B until account A's reset (2026-11-20 06:00Z), then comes back to G,
whatever the .4 status. **G remains the authority**: P4 works as G's sub-agent, a fresh set of eyes mainly on the
code (byref-param per 12.1), with timed arms only as its hardware allows. G sets P4's next step and rules on design,
by mailbox (`docs/phase4/inbox/P4/`, short); P4 asks G the same way. **P4 leaves a clean HANDBACK block here
(section 14, dated) for Friday**: the tip, which units and probe rows are green, what each arm read and on which
host, and anything ruled or open. 12.1 is ACCEPTED by COORD; code past the units has COORD's GO; **arms wait for
P3's short Fable re-review of 12.1 plus the code** (account B), then COORD's GO.

**P4's next step:** the code step below, to units P1-P6 green with the rest of GenTests green, then the eight probe
rows red first under both controls; post the code tip to G and COORD for P3's re-review. No arm before that GO.

**Where it stopped.** Branch `claude/g-mn-proto-b0`, WIP commit 244dffb9f4. No byref-param code is written.
TwinBoxTests P1, P2, P3, P4 and P6 are RED BY DESIGN: they assert the parameter box twin (`ppꞏtwin(global::go.ж<T>
Ꮡp)`), the call boxes (`await ppꞏtwin(px)`, `(Ꮡp)`, `(Ꮡx)`, the element pointer), two parameters over one box, a
stack local named-plain (`|pp|st`), `out` refused `byref-out`, and the copy control on parameters. P5 (switch off)
is green and must stay so. Every other unit is green (588 before P1-P6). MnBoxAlias has the first two probe rows
(readCell, forward; Go prints 11 lines); they equal Go today because both functions are still refused and run plain.

**The next code step** (all in src/gen/go2cs-gen):
1. TwinGenerator.BoxTwin.cs: switches `BoxParam` (GoTwinsBoxParam, default on) and `BoxArgCopy` (GoTwinsBoxArgCopy,
   the call-site control); `BoxParamsOf(method, symbol)`: every plain `ref` parameter of a non-extension function
   (null on any `out`/`in`, or when the body names `Ꮡ<name>`); the argument box per 12.1 (forms (a)-(c)), each bound
   by `Binds` to ж<parameter type>; a `nonnil(ref e).f` row; the nil `((ж<T>)default)` operand side-effect free.
2. TwinGenerator.cs: PreRefusal skips boxed positions, names `byref-out` and `byref-mixed`; the candidate keeps the
   boxed parameter names; the Collector computes one box per by-ref argument; ShouldAwait/BoxCallable are all or
   nothing per call; read the switches beside GoTwinsBoxRecv (Execute) and add them to src/Directory.Build.targets.
3. TwinGenerator.Emit.cs and .Rewriter.cs: each boxed parameter becomes `global::go.ж<T> Ꮡp` with its prologue
   (Local() escape; the copy control per parameter); IsRefLocal admits every boxed parameter (it knows only the
   receiver today); Statements seeds the re-derive per parameter; an awaited call replaces each `ref` argument by its
   box; a `// P` row names a plain call by its first unboxable argument. **12.1 item 5 (spill each box ahead of any
   await in the argument list) is the hard part.** G's suggestion, NOT ruled: if it proves costly, leave such a call
   named-plain (`arg-await`) in the first cut and ask COORD.
4. Then the other eight probe rows of 12.1, each red under GoTwinsBoxCopy AND GoTwinsBoxArgCopy before the code is
   called done.

**The arm recipe** (MnHttpClosure, arm B rule; the program, its 9-line Go output and its 186-project list are now in
`src/tools/mnspike2/MnHttpClosure/`):

- Convert once per tree: build the converter from the tree (`go -C src/go2cs build -o go2cs.exe .`), then
  `go2cs.exe -recurse -go2cspath <tree>/src <MnHttpClosure> <out>` (the output positional is required). The generated
  output root imports `$(go2csPath)Directory.Build.targets`; without it twins are off (DATA-mn-stage-c.md).
- Per arm: delete obj, bin and Generated under `<tree>/src/core/<pkg>` for each of the 186 packages and the program;
  touch a stamp; `dotnet build <out>/src/MnHttpClosure/MnHttpClosure.csproj -c Release -p:go2csPath=<tree>/src/
  -p:GoTwinsFor=<the 186 names joined by %3B>%3BMnHttpClosure -p:GoTwinsForIface=default -p:GoTwinsForIfaceAll=true
  -p:EmitCompilerGeneratedFiles=true` plus the three CreateHardLinks* properties (disk) and the arm's switches:
  O = `GoTwinsBoxRecv=false` (the base, stage (c)'s arm B), N = defaults (N2), the byref-param arm = defaults once
  GoTwinsBoxParam exists, and `GoTwinsBoxParam=false`, which must equal N.
- Read every `go2cs.twins.report.g.cs` newer than the stamp, summed: `// candidates= twinned=`, `// await-sites=`,
  `// iface: ... forwarders=`, `// forwarders-sync=`, `// refused <reason>=N blocked=M`, `// box: twins= calls=
  plain=`, and the `// P`, `// V`, `// R` rows. Bytes: the sum of go.*.dll beside the exe.
- Run 5 times from the exe's directory: `GOMAXPROCS=12 GO2CS_MN=1 GO2CS_MN_READING=1 MnHttpClosure.exe`, with a
  120 s timeout. stdout must equal go-output.txt byte for byte; stderr's `READING go-scheduler` line gives
  `handoffs`, `thread-blocks`, `peak-os-threads`, `parks`, `steps`, `steps-over-10ms`, `peak-carriers`.
- Floors: build from a per-run CLONE at a committed SHA (an edit can never reach a running arm); one closure job at
  a time (the output root is shared); at least 25 GB free; never two conversions at once.
- **PDB-path rule:** go.*.dll embeds each PDB path; compare byte totals only between trees whose paths are as long as
  each other (on G's box, `g-run-rev` read +1,536 against `g-mn3`).
- The probe: the behavioral runner, `GoTwinsFor=MnBoxAlias GO2CS_MN=1 run-behavioral.ps1 --filter MnBoxAlias`, then
  the same with `GoTwinsBoxCopy=true` and with `GoTwinsBoxArgCopy=true` (both must FAIL). The sweep: the 30 programs
  of DATA-mn-n2-receivers.md through the same runner (30/30). GenTests: `dotnet test src/tests/GenTests -c Release`.

**Host dependencies.** G's box: 12 cores, GOMAXPROCS=12, Windows, .NET SDK 10.0.400, Go 1.26.10 for this tree's
converter. Host-SENSITIVE readings: hand-offs, thread-blocks, peak OS threads, parks, steps, steps over 10 ms, walls,
and bytes (path length, SDK). Host-INDEPENDENT (the generator is deterministic): twinned, await sites, refusals,
box counts, named-plain, V rows, the sweep's verdicts, Go's output. On another host, read arms O and N first (5 runs
each) and restate PB-1 relative to that N before the byref-param arm: on G's box N read 130-137 hand-offs + 0
thread-blocks (about 133) against O's 110-116 + 81-86 (about 197); PB-1's 105-128 is a fall of 4-21% from N.

**The predictions, as revised in 12.1:** PB-1 (host-sensitive) the plain-block total falls 4-21% from N,
thread-blocks stay 0, peak OS threads at most N's; PB-2 (independent) twinned 3,806 -> 3,875-4,100; PB-3 (independent)
refused parkers 175 -> 105-130; PB-4 (host- and path-sensitive) +0.05 to +0.35 MB; PB-5 (independent) byref-param
named-plain at most 12; PB-6 (independent) sweep 30/30 and Go's 9 lines every run; PB-7 at most two rules beyond the
three of 12.1; PB-8 every probe row green and red under both controls. Stop lines SB-1..SB-8 as in 12 and 12.1 (SB-1
and SB-4 reworded, SB-7 at 20).

**Read first:** this document's sections 9-12.1; DATA-mn-n2-receivers.md (the arms, the scores, the re-gates, the
PDB-path rule); DATA-mn-stage-c.md (the closure and its harness); the commit messages of 3b52c5c216 (the cut) and
cb38d22455 (the review's seven changes); TwinBoxTests.cs (the units, including P1-P6).

## 14. HANDBACK from P4 (dated block, 2026-11-17)

**Tip.** `claude/p4-mn-byref` at this block's commit, on 2a1199942f (2a1199942f, 5bd877cfb6, b98cb51ecc, dd7627dc97,
bf4670ec47 and 436627d8f4 sit on G's WIP 244dffb9f4 line). P3 reviewed to dd7627dc97, re-read to b98cb51ecc (findings 1-7
CLOSED) and to 5bd877cfb6 (the chain rule SOUND, nits CLOSED).

**Commits.**
- 436627d8f4: the parameter box twin per 12.1 (units P1-P12).
- bf4670ec47: field-access and nil-cast box rules, P13 (no spill), probe rows, the MnLaterArg repro.
- dd7627dc97: by-ref go wrapper thunks followed (G ruling 2).
- b98cb51ecc: P3 review findings 1-7 (P14-P20; E.Value dropped from form (a); heap rule; binding by symbol).
- 5bd877cfb6: G step 1, the two-level field chain (P21, P22), and P3's re-read nits (P23, P24, named constant, nil).
- 2a1199942f: P3's delta-read nit (P22 asserts its plain row) and the reference-field chain row.

**Green.**
- GenTests 619/619 at 5bd877cfb6.
- MnBoxAlias (linux, Release, GO2CS_MN=1, 3 runs each) EQUALS Go under plain, under twins (-p:GoTwinsFor=MnBoxAlias),
  and under twins with GoTwinsBoxParam=false.
- Under GoTwinsBoxCopy and GoTwinsBoxArgCopy every param row is red, and both controls exit 2 at param-nil-field.
  The red rows: param 1, param-forward 5, param-heap 1, param-element 1, param-field 1, param-ptr-field 1,
  param-two 101, param-array 1300, param-thunk 1, param-package 1, param-chain 1, param-chain-param 16,
  param-chain-ref 1. Under copy the receiver rows are red too. param-nil-chain sits after param-nil-field, so no control
  reaches it; under twins it panics at the outer nil, as Go does.
- P3's one unconfirmed aliasing claim, `.of(token)` over structs that hold a reference field and a nested struct: rows
  param-chain-ref and param-chain-param, EQUAL under twins and red under both controls.

**Arms.** All count-only: windows/amd64 flavour, convert plus build, run on a 4-core linux box. These are
host-independent readings.

| Arm | SHA | twinned | await | refused parkers | box twins/calls/plain/param | note |
|---|---|---|---|---|---|---|
| O | 1ac7dcf40f | 2,999 | 7,167 | 658 (byref-param 395/60) | - | EXACT to G's |
| N | 1ac7dcf40f | 3,806 | 9,495 | 175 | 634/987/24/- | |
| PO (BoxParam=false) | 436627d8f4 | = N | = N | = N | = N | EXACT |
| P | 436627d8f4 | 3,907 | 9,959 | 116 | 634/1,202/32/76 | PB-2 HIT, PB-3 HIT, PB-5 MISS (13 > 12), SB-7 not fired |
| P2 | bf4670ec47 | = P | = P | = P | = P | 184 reports byte-identical to P |
| P3 | dd7627dc97 | 3,907 | 9,961 | 116 | 634/1,204/32/76 | GO2CS0014 ref-wrapper 2 -> 0; only net/http moved |
| PO2 (BoxParam=false) | b98cb51ecc | = N | = N | = N | = N | EXACT |
| P4 | b98cb51ecc | = P3 | = P3 | = P3 | = P3 | 185 kept reports byte-identical to P3 |
| S1 | 5bd877cfb6 | 3,907 | 9,962 | 116 | 634/1,205/31/76 | every prediction HIT; only crypto/x509 moved |

- PB-5's 13 rows at arm P: stack locals 7 (traceback cgoCallers, deepequal visited/inProgress, sort swaps x4);
  composite temp 1 (parsePublicKey); cascade receiver 1 (locateBrackets p); un-enumerated forms 4 (casLoop
  ptr.Value.Value x3, x509 nonnil(ref in).TBSCSR.PublicKey).
- At arm S1 the x509 row is handed (step 1), so 12 stay named-plain: the 7, the 1, the 1, and casLoop x3.
- casLoop x3 (G ruling 11-17 11:56Z, option (a), no code): internal/runtime/atomic/atomic_impl.cs:209, 218 and 221, in
  Xadduintptr, Anduintptr and Oruintptr. `ptr.Value.Value` is the `nuint` field of golib's `uintptr` struct in a
  ж<uintptr>'s pointee (Go's `&(*ptr).Value`), not a ж in a ж. A box for it needs a field-pointer door on a golib
  struct, a public-surface change, and casLoop's only await is a delegate call that every caller fills with a static,
  non-parking lambda. A census of `ref X.Value.Value` in src/core finds 20 sites (ptr x10, addr x10), all golib uintptr
  fields; none is ж<ж<T>>. The true box-through-a-box form, `~ppx`, is form (a) already (P17).
- G 1(c): inner calls left blocking under box-param calls = 5. All are interface calls in a later argument:
  net/http fs.cs:850 and 862 (2), and crypto/tls handshake_client.cs:848 and handshake_server.cs:831.
  Read by a probe-only build that was never committed.

**Kit rules.** Read every arm from its kept copies, listed with `ls -A` (one kept name starts with a dot); the live
reports are overwritten by the next arm. The arm purge locates each project's csproj. A name-to-path mapping (`tr . /`) misses every dotted
vendor path and leaves stale generator output for an arm whose properties equal the previous arm's.

**Named limits.**
- RecvGenerator CS1620 on a hand-written `this ref` method with another `ref` parameter (byref-mixed fixture).
  G routes it to the BOARD.
- The GenTests harness parses at C# 13, and its Roslyn cannot parse FieldAccessGenerator's C# 14 `extension(...)`
  blocks. So the GoFieldAccess box rule has no unit and is evidenced by the probe only.
- The thunk switch-off unit does not depend on the BoxParam gate.
- The spill (12.1 item 5) stays open: no spill, and a later parking argument stays plain after the box (P13).

**Known divergence (.5 BOARD row).** f(p, <-ch): gc reads p after the receive, while the converter's plain emission
reads it before. Repro src/tools/mnspike2/MnLaterArg: go 1405, plain 1505, twins 1505, both controls 105.
byref-param equals plain.

**G's two steps (16:19Z).** (1) The two-level field chain: DONE at 5bd877cfb6 (12.3), arm S1 every prediction HIT.
(2) The box read through a box: CLOSED with no code (G ruling 11-17, above).

**Open, for G.**
- The spill (12.1 item 5): the 5 blocking inner calls of G 1(c).
- PB-5's remaining 9 non-casLoop rows (stack locals, the composite temp, the cascade receiver).
- Timed arms and an A/A pair: none run here, by COORD's ruling; they wait for G's laptop.
- The BOARD rows: RecvGenerator CS1620, and the f(p, <-ch) divergence.

## 15. HANDBACK from P4, second (dated block, 2026-11-20)

Section 14 handed back to 4216b1be70 (2026-11-17). This block covers what landed on `claude/p4-mn-byref` after it, to
the account-A reset; section 13's arrangement ends here and M:N returns to G.

**Tip.** `claude/p4-mn-byref` at this block's commit, on 85d10c5215. Master is merged in twice: 39595510a4 (128f9685bc,
behavioral projects re-converted at 870b0f04ad) and a98c5175be, the 1.26.11.1 release record (1aa85ac296). M:N stays
opt-in; COORD seats it after G's review, and it rides none of the first post-.4 batches.

**Commits since section 14.**
- 1771d2347a, 67b2e00be0, e86ebb9b7f: the spill, rules 1 and 2 (12.4), units P25-P32, probe row `param-later-call`.
- ea5cae22f7, 0b03ace1fc, 5303b1777e, a6c2409581: PB-5, a stack local handed to a box twin is a heap local in the twin
  (12.5), units P33-P40, probe row `param-stack`.
- 85d10c5215: G's review gaps 1 and 2 (end of 12.5), units P41 and P42, probe row `param-stack-init`.

**Green, at 85d10c5215.**
- GenTests 691/691 (`dotnet test src/tests/GenTests -c Release`, GoTargetOS and GoTargetArch unset).
- MnBoxAlias (linux, Release, GO2CS_MN=1, 3 runs per arm): plain and twins (`-p:GoTwinsFor=MnBoxAlias`) each EQUAL Go's
  whole output 3 of 3. Under GoTwinsBoxCopy and under GoTwinsBoxArgCopy the program DIFFERS and exits 2 at
  param-nil-field, as in section 14. The three rows added since then read: `param-later-call` 1903 (controls 103),
  `param-stack` 21 (controls 1), `param-stack-init` 23 (controls 3).
- Every unit since section 14 was red first: its mechanism neutered, the unit red, the source restored sha-identical.

**Arms.** Count only: windows/amd64 flavour, MnHttpClosure convert plus build, 185 reports, rc 0, 0 errors, on the same
4-core linux box as section 14. Host-independent readings; full tables in 12.4 and 12.5.

| Arm | Tip | twinned | await | refused | box twins/calls/P rows |
|---|---|---|---|---|---|
| M-P | 870b0f04ad | 3,946 | 10,165 | 109 | - |
| R-P | spill rules 1+2 | 3,946 | 10,171 | 109 | 632/1,203/31 |
| T-P | ea5cae22f7 (PB-5) | 3,960 | 10,207 | 109 | 633/1,219/16 |
| M-N, R-N, T-N | as above, BoxParam=false | 3,844 | 9,665 / 9,666 / 9,666 | 169 | T-N's 185 reports byte-identical to R-N's |

12.4's predictions were all met. 12.5's T-P prediction MISSED on all three counts, for the reason stated there (it
counted PB-5's byref-param rows only; the rule covers receivers too). The arms T2-P and T3-P that followed the two
PB-5 fixes moved only the reports their prediction named.

**Closed since section 14.**
- The spill (12.1 item 5, section 14's first open item): all 5 inner calls of G 1(c) now await (12.4).
- PB-5's stack-local rows: P rows 31 -> 16 at arm T-P, the 15 handed being 9 byref-param rows and 6 more stack locals
  (receivers included); 8 locals stay plain with a named reason (12.5).
- G's review of 12.4 and 12.5: CONCUR, gaps 1 and 2 closed (end of 12.5).

**Disclosures.** Unchanged from the list at the end of 12.5: PB-5's box per twinned call, nit 9's box per built field
or element pointer, the f(p, <-ch) divergence (MnLaterArg), RecvGenerator CS1620 (byref-mixed).

**Open, for G.**
- The 16 P rows left at arm T-P (12.5): casLoop x3 (ruled no code), chunked.cs `bw.Value.Writer.Value`,
  filetransport.cs `t.fh`, runtime mcache x3, and the 8 refused locals (`loop` 3, `declaration` 5).
- Timed arms (PB-1, PB-4) and an A/A pair: none run on this box; they wait for G's laptop.
- The BOARD rows: RecvGenerator CS1620, and the f(p, <-ch) divergence.
- The GoFieldAccess box rule still has no unit (the GenTests Roslyn parses C# 13; section 14's named limit).
