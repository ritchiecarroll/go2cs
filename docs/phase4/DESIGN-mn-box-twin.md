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
