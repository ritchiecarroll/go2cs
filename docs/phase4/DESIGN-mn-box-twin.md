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
