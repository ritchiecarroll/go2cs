# Stack-allocation parity — a design

> **STATUS: PROPOSED (lane C2, 2026-10-25; branch `claude/c2-design-stack-alloc`, cut from the 1.26 hop tip
> `b5fc1294db`).** Design only: no code lands from this record, and nothing in it is for the hop. The owner rules
> stack-allocation parity post-hop. The record covers Go's stack-allocated `make`, `new`, composite literals and
> addressed locals; R's lowering of `&T{}` temporaries across the §10.1 boundary; and G's allocation-free
> element-identity compare for math/big. It asks for the rulings listed in [Open questions](#open-questions).
>
> It builds on [REC-B, the non-escaping locals](phase4/DESIGN-nonescaping-locals.md), which names the population and
> the escape oracle. This record supplies the storage model and the per-call-site seam that REC-B leaves open, puts
> the increments in order, and predicts what each one does to the go1.26.10 deferred entries.

## The problem

Go's escape analysis keeps an allocation in the function's frame when no pointer to it outlives the call. A
`make([]byte, 0, 108)`, a `new(Digest)`, an `&Int{...}`, a `[]Word{...}` or a `var b Builder` whose address
is taken costs nothing on Go's heap when it stays in the frame. go2cs allocates every one of them on the managed
heap, so any test that counts allocations reads more than Go does.

**What a test measures.** `testing.AllocsPerRun` reports zero only when the run allocates zero **bytes**. Any managed
allocation reports at least 1, whether or not golib's counter sees it. So parity means removing the allocation. Taking
it out of the counter changes nothing: an allocation the counter does not charge still fails a want-zero assert on
its bytes. `AllocsPerRun` also calls the function once before it measures, so storage that is created on first use
and reused afterwards does not show in the reading. <!-- src/core/testing/testing.cs:1121 (`if (allocated == 0L)`),
:1104 (`initialization — and JIT compilation here — must not count against f.`). The floor: src/core/testing/testing.cs:1026
(`nonzero bytes with a nonzero count ⟹ the COUNT is reported, floored at 1;`). -->

**Where Go decides.** Go makes the decision at one of two places:

- **OWN.** Go decides at the allocation, in the function's own compiled copy. A per-function conversion can mark the
  site.
- **INLINED.** The allocation stays on the stack only where the callee is inlined into a caller that does not leak
  it. The same callee allocates on the heap when it is called anywhere else. go2cs keeps Go's function boundaries one
  for one and never inlines, so it has no place to make a per-call-site decision.

## The entries

These are the go1.26.10 deferred entries that cite this record as their retirement plan. The readings are
[lane H's](phase4/DATA-hop126-deferred-allocs.md), on windows/amd64 at Release with tiering off.

| Entry | Go's limit | C# per run | Where the allocations come from |
|:--|:--|:--|:--|
| `crypto/sha256` TestAllocatonsWithTypeAsserts | 0 | 4 allocs, 800 B | INLINED: `New()`'s `new(Digest)`, and the `make([]byte, 0, 108)` from `MarshalBinary`'s devirtualized inline |
| `crypto/internal/fips140test` TestMLDSAAllocations | at most 5 | 4,071–4,642 allocs, about 1.9 MB | both: 30 OWN constant-cap makes, 8 INLINED `&sha3.SHAKE{}`, and the `ringElement` values |
| `math/big` TestNewIntAllocs | 0 | 1 alloc, 208 B | INLINED: `NewInt`'s `&Int{}` and `[]Word{}` at the call |
| `math/big` TestSqrAlloc | under 1 | 4 allocs, 424 B (length 80 only) | OWN: `karatsubaSqr`'s `&Int{...}` temporaries, passed to methods with pointer receivers |
| `math/big` TestMulUnbalanced | allocated bytes at most 10× the inputs | ratio 14 | element address-of used only for an identity compare, and `karatsuba`'s `&Int{...}` locals |
| `log/slog` TestAlloc/2_pairs_disabled_inline | 0 | 2 allocs, 184 B | not attributed |
| `log/slog` TestJSONAllocs/attrs | 1 | 28 allocs, 15,040 B | not attributed |
| `log/slog` TestJSONAllocs/message | 0 | 12 allocs, 8,904 B | not attributed |
| `strings` TestReplace | at most 1 per case | 3 allocs, 200 B (13 cases); 6 (one case) | OWN: the addressed `var b Builder`; the rest are not attributed |

<!-- Readings: docs/phase4/DATA-hop126-deferred-allocs.md:42 (`TestAllocatonsWithTypeAsserts`…`4 allocs, 800 B`),
:43 (`TestMLDSAAllocations`…`4,081.17 allocs, 1,907,607 B`), :44 (`TestNewIntAllocs`…`1 alloc, 208 B`), :45
(`TestSqrAlloc`…`4 allocs, 424 B`), :46 (`TestMulUnbalanced`…`ratio 14`), :47 (`2_pairs_disabled_inline`…`2 allocs, 184 B`),
:48 (`TestJSONAllocs/attrs`…`28 allocs, 15,040 B`), :49 (`TestJSONAllocs/message`…`12 allocs, 8,904 B`), :50
(`TestReplace`…`3 allocs, 200 B (first case)`). The mldsa range is the same record's amendment,
:106 (`4,081 to 4,432 per run at batch 19`…`4,071 to 4,642 at batch 24`). The OWN/INLINED attribution of the sha256,
NewInt and mldsa rows is C2's sizing census (mailbox, COORD inbox, 2026-10-21T13:07:46Z, go1.26.10, three amd64
targets); the karatsuba rows are R's (COORD inbox, 2026-10-21T07:59:24Z, windows) and G's (math/big's disclosure
plan, src/core/math/big/go2cs_test_disclosures.json:12 (`INCREMENT 1 (queued post-publish, COORD 2026-10-25)`)). The
TestReplace attribution is read at the emission, src/core/strings/strings.cs:1195
(`ref var b = ref heap(new Builder(), out var Ꮡb);`). -->

## The shapes and their counts

Each count is a census of Go's own decisions. The census runs `-gcflags='-m -d=variablemakehash=vy'` with the pinned
go1.26.10 toolchain over std, the test list's `_test.go` files and the behavioral corpus, and classifies each site
against the source with `go/types`. Counts are unique sites/files/packages on amd64, taken from the census of
2026-10-21; a cut re-measures them at its own base.

**OWN sites in std:**

| Class | linux | windows | darwin |
|:--|:--|:--|:--|
| `make`, constant cap | 131/61/43 | 142/66/47 | 131/62/44 |
| `make`, variable cap (on the stack only when cap ≤ K) | 148/100/63 | 158/105/66 | 154/102/64 |
| `new(T)` | 302/53/33 | 304/54/33 | 302/53/33 |
| `&T{}` | 153/72/40 | 183/75/40 | 149/69/38 |
| `[]T{}` literal | 194/113/70 | 192/111/69 | 196/115/71 |
| `make(map)` / map literal | 107 / 18 | 107 / 19 | 107 / 18 |
| **all OWN** | **1053/320/145** | **1105/327/146** | **1057/321/145** |
| of which golib's counter charges today | 859/272 | 913/281 | 861/271 |

- The three targets' file union is 355 std files.
- In the tests, OWN sites are 4382/658 (linux), and the file union is 675.
- In the behavioral corpus, OWN sites are 1119/391, and the union is 397 goldens.
- A slice literal's backing is a C# array creation, which the counter does not charge. It still costs bytes, so it
  still fails a want-zero assert.
- The census excludes two kinds of site that Go does not move to the stack:
  - `append(x, make([]T, n)...)`, which go2cs already emits as `makeꓸꓸꓸ`;
  - variable makes whose elements are larger than 32 bytes (K = 0).

**INLINED-only call positions:** std 693/698/693, tests 1453/1439/1442, behavioral 150/158/149. On linux the std
positions split into `&T{}` 422, `make` 282, `new` 118, slice literal 34 and `make(map)` 20.

**Three shapes outside the census:**

- **Addressed locals.** `var b Builder` used through pointer methods emits as `ref var b = ref heap(new Builder(), out
  var Ꮡb)`. Go prints nothing for an addressed local that stays in the frame and prints `moved to heap: b` for one
  that does not. The population is therefore every `heap(` site minus Go's `moved to heap` lines. It is not yet
  counted.
- **`&T{}` temporaries handed to a method with a pointer receiver.** Examples are `karatsuba`'s nine `Int`
  temporaries and `karatsubaSqr`'s six, each emitted as `Ꮡ(new ΔInt(...))`. This shape sits inside the `&T{}` rows.
  Phase A's ref-lowering never lowers a method's pointer receiver or parameter (the §10.1 boundary), so these stay
  boxed.
- **Element address-of used only for an identity compare.** `alias()` compares `&x[cap(x)-1] == &y[cap(y)-1]` and
  `addVW` compares `&z[0] != &x[0]`. In Go an element address is a plain interior pointer and costs nothing. In
  golib each take mints an `ElemRefBox`. This is not a stack allocation in Go, but it is the largest share of
  TestMulUnbalanced's excess.

<!-- src/core/math/big/natmul.cs:229-238 (`var (x0, x1) = (Ꮡ(new ΔInt(abs: x.slice(0, n2).norm()))`…`var ty =
Ꮡ(new ΔInt(abs: z.slice(n2, 2 * n2)));`), :324-331 (`var z0 = Ꮡ(new ΔInt(abs: z.slice(0, 2 * n2)));`…`var tx =
Ꮡ(new ΔInt(abs: z.slice(0, n2)));`). src/core/math/big/nat.cs:240
(`Ꮡ(x.slice(0, cap(x)), cap(x) - 1) == Ꮡ(y.slice(0, cap(y)), cap(y) - 1)`). src/core/math/big/arith.cs:115 (`if (Ꮡ(z, 0) != Ꮡ(x, 0)) {`).
src/core/golib/builtin.cs:2505-2506 (`AllocationCounter.Count();`…`return new ElemRefBox<T>(target, index);`).
src/go2cs/escapeAnalysisOperations.go:40-41 (`Phase A never lowers a METHOD's pointer`…`parameters (the §10.1
boundary in the ref-lowering doc)`). -->

**Named, not sized.** Go also keeps these in the frame. Counts are linux std:

| Shape | Sites |
|:--|:--|
| closures | 1122 |
| `...` argument slices | 3549 |
| `[]byte`/`string` conversions | 701 |
| interface conversions | 216 |
| concatenations | 151 |
| other (method values and similar) | 183 |

## What the C# model allows

Three facts about C# decide the design:

1. **`ж<T>`, `slice<T>`, interface values and delegates are heap references.** None of them can point into a C#
   stack frame. A frame local can be reached only through `ref`, `Span<T>` or a `ref struct`.
2. **Frame-local storage serves only `ref` and `Span` consumers.** `stackalloc`, `[InlineArray(N)]` and struct
   locals work only when every consumer takes `ref` or `Span`. A value that becomes a `ж<T>`, is sliced into a
   `slice<T>` or is stored in an interface needs a heap object.
3. **Zero bytes per run means reusing a heap object.** Allocating one each run is what this record removes.

An `array<T>` value is a wrapper over its own `T[]`, so every `[N]T` value Go keeps inline needs a separate array on
the managed heap. That is [REC-A, the array-value storage record](phase4/DESIGN-array-value-storage.md), and this
record does not replace it. <!-- src/core/golib/array.cs:152 (`public readonly struct array<T>`), :161
(`internal readonly T[] m_array;`). -->

## The options

### Storage

**S1 — frame locals reached by `ref`.** The design lowers a pointer to a C# local passed by `ref`, a constant-size
unmanaged buffer to `stackalloc`, and a fixed array to an `[InlineArray(N)]` local.

- *For:* zero bytes even on the first call, no bookkeeping, and the closest match to what Go does.
- *Against:* every consumer must take `ref` or `Span`. Methods (§10.1), exported callees, interface flows and any
  `slice<T>` view are out of reach, and each lowering ripples through signatures. Inline layouts reopen the 64 KB
  null-byref audit.

**S2 — a goroutine's stack slots.** Each goroutine keeps a store of reusable boxes and backing arrays. A function
with stack sites reserves a fixed number of slots on entry and releases them on exit. Each site takes its own slot
and zeroes it, as Go zeroes a stack object.

- *For:* it works for any `T`, and `ж<T>`, `slice<T>` and interface values keep their types. Callee bodies do not
  change. One slot per site matches Go's one stack object per site, inside loops too: Go moves an object to the heap
  when it would outlive its iteration, so a site the oracle names never needs two live copies in one frame. Recursion
  gets fresh slots per frame, because reservation is a stack.
- *Against:* every call pays for entry, exit and zeroing. Slots hold dead objects, so references must be cleared on
  release. The first call allocates the slots. AllocsPerRun's warm-up call hides that cost, but `runtime.MemStats`
  shows it once per goroutine. A wrong oracle answer turns into aliasing, because a pointer that escaped would see
  its slot reused (see [Risks](#risks)).
- *Keying:* in go2cs a goroutine is a thread, so the store hangs off the per-thread holder. A pooled thread that
  starts its next goroutine resets the store. If goroutines stop owning threads (the
  [M:N sizing note](DESIGN-mn-scheduler-sizing.md)), the store moves with the goroutine.
  <!-- src/core/golib/GoThreadState.cs:27-28 (`a goroutine is a thread, a coroutine's body`…`a pooled thread's next goroutine starts clean`). -->

**S3 — an allocation the counter does not charge.** This is `stackAppend`'s model today: the buffer is allocated
when it is taken and is left out of `AllocationCounter`. **Rejected for parity**, because the bytes are still
allocated and a want-zero assert still fails. A `stackAppend` site on a want-zero path keeps reading nonzero until
its buffer comes from S2. <!-- src/core/golib/stackAppend.cs:35-36
(`is allocated when it is taken and is not charged to`…`because Go's lives on the stack`). -->

**S4 — inline the callee in the converter.** **Rejected**, because it breaks the one-for-one function boundaries the
emitted code keeps so it reads like the Go source. reflect's TestMapIterSet disclosure argues from the same premise.
<!-- src/core/reflect/go2cs_test_disclosures.json:97 (`The only route is inlining MapRange into its callers`). -->

### The INLINED seam

**B1 — a frame twin.** The callee gets a second overload that takes the caller's slots. Its stack sites draw from
the caller's reservation, so the object lives exactly as long as the caller's frame, which is where Go puts it. A
call that Go reports as inlined and non-escaping calls the twin; every other call keeps the original. A chain of
inlines (crypto/sha256 `New` → fips `New` → `new(Digest)`) is a chain of twins. The visible file keeps one body, and
go2cs-gen generates the twin from a marker on the stack sites. That follows the readability principle even though
it costs generator work.

**B2 — an ambient frame**, where the callee draws from "the current caller" with no parameter. **Rejected**: any call
in between would take the slots, and reentrancy breaks it.

An illustrative shape for both seams. The final spelling belongs to the cut:

```csharp
// OWN: strings.Replace's `var b Builder` stays in Replace's frame
using var Ꮡs = stackSlots.Enter(1);
ref var b = ref stack(new Builder(), out var Ꮡb, Ꮡs, 0);

// INLINED: the twin go2cs-gen writes beside fips sha256.New, and a caller that inlines it in Go
public static ж<Digest> New(in stackSlots Ꮡs, int slot) { var d = Ꮡs.@new<Digest>(slot); d.Reset(); return d; }
var h = sha256.New(Ꮡs, 0);
```

`stack(` mirrors `heap(` at the same site, so a reader can see Go's decision in the emitted code.

### The two non-storage increments

**C — ref-lowering across §10.1, for same-package callees (R).** This extends Phase A to a method's pointer
receiver and parameters where the callee's uses permit `ref`, so an `&T{}` temporary becomes a plain local. It is S1
for exactly the shape where S1 is cheap. It needs a ruling, because §10.1 limits Phase A to unexported package-level
functions and the karatsuba temporaries reach exported `Int` methods. The precedent for a `ref` form beside the boxed
form is zh-box B′'s dual emission. <!-- docs/phase4/DESIGN-zh-box-reduction.md:674
(`Phase-A scope — RULED: unexported package-level functions only`). -->

**D — an allocation-free element-identity compare (G).** When both operands of `==` or `!=` are element address-ofs
(`&a[i] == &b[j]`), the converter emits a golib compare of the backing storage and absolute index, with `ж==`'s
semantics, and no box is taken. It is independent of S1 and S2.

## Recommended increments

The increments are in order. Each prediction is in counted objects per run (TestMulUnbalanced in its byte ratio),
against the windows/amd64 readings above. **UNMEASURED** marks a prediction drawn from the shapes rather than a
per-site split.

| # | Increment | Entry | Today | Predicted after | Retires? |
|:--|:--|:--|:--|:--|:--|
| 1 | D, element-identity compare | math/big TestMulUnbalanced | ratio 14 | about 5 (G's prediction) | yes (limit 10) |
| 2 | C, ref-lowering across §10.1 | math/big TestSqrAlloc | 4 | 0 (UNMEASURED) | yes, at zero bytes |
| 2 | | math/big TestMulUnbalanced | about 5 | about 2 (the nine `Int` locals; derived from G's split) | already retired |
| 3 | S2 slots + the OWN marker | strings TestReplace | 3 / 6 | 2 / 5 (the Builder) (UNMEASURED) | no |
| 3 | | fips140test TestMLDSAAllocations | 4,071–4,642 | lower by the OWN makes and array locals per loop iteration (UNMEASURED) | no |
| 4 | B1 frame twins | math/big TestNewIntAllocs | 1 | 0 (UNMEASURED) | yes, at zero bytes |
| 4 | | crypto/sha256 TestAllocatonsWithTypeAsserts | 4 | 1, then 0 with a devirtualized twin call (UNMEASURED) | with the devirtualized call |
| 4 | | fips140test TestMLDSAAllocations | — | lower by the 8 SHAKE states | no |
| 5 | the named classes on S2 | log/slog TestAlloc/2_pairs_disabled_inline, TestJSONAllocs/attrs, /message | 2; 28; 12 | owed an attribution first | not predicted |

**Why each prediction:**

1. **D.** G puts 74% of the excess of each of about 1,249 40×40 products on `alias()`'s two element boxes per call
   and `addVW`'s compare. D removes those, which brings about 4.2 KB per product to about 1.2 KB. The 21% from
   `karatsuba`'s `Int` locals and the defer frame remain; C then takes the locals' 864 B per product.
2. **C on TestSqrAlloc.** R attributes the four counted objects to `karatsubaSqr`'s `Int` temporaries. The function
   makes six, so the per-site split is owed. The two shorter lengths already read zero through `sqr`'s `getStack` and
   `defer stk.free()` and `basicSqr`'s `defer stk.restore(stk.save())`, the idiom `karatsubaSqr` also uses, so those
   paths allocate nothing. Retirement still needs zero bytes, not just a zero count.
3. **S2 on TestReplace and mldsa.**
   - TestReplace: the strings.Builder records its own address (`copyCheck`), so ref-lowering cannot reach it. A slot
     box can.
   - TestReplace's remaining allocations are `Grow`'s buffer, which Go also allocates, and the element take in
     `String()`'s `unsafe.SliceData`. That element take escapes into `unsafe.String`, so D does not cover it.
   - mldsa: ML-DSA's signing loop makes `[]ringElement` slices with constant caps on every iteration. A slot removes
     a make's allocations only if zeroing keeps each element's nested array storage, which is a precondition of the
     cut.
   - mldsa: most of its 1.9 MB is `ringElement` values copied and returned by value. That share is REC-A's, so mldsa
     retires only with both records. The split between them is owed.
4. **B1.**
   - NewInt: the twin takes both the `ж<Int>` box and the `[]Word{}` backing from the caller's slots. The backing is
     uncounted today but costs bytes.
   - sha256: the `Digest` box and its two field arrays (`h`, `x`) come from a slot, and the box keeps its field
     storage when zeroed. `MarshalBinary`'s make sits behind an interface call, which Go devirtualizes once it knows
     `h`'s concrete type. A twin cannot be reached through the interface, so the last allocation goes only when the
     converter emits Go's devirtualized call.
5. **The named classes.** The slog rows' likely sources are the `...any` pack and the boxing of `i` to `any`. Both are
   named-not-sized classes (and the pack is [REC-C's](phase4/DESIGN-slice-idiom-allocations.md)), so no delta is
   predicted until a per-site split exists.

**Beyond the cited entries.** The same mechanisms also reach three other entries:

- B1: unicode/utf16 `Decode`'s buffer, which is REC-B's inlined-call-site stage;
- S2: unique's `canonMap` key, an addressed key that does not escape the lookup;
- B1, as a hand-owned twin: reflect TestMapIterSet's `*MapIter` box, whose structural proof rests on S4 being the
  only route.

Each is re-examined when this record is accepted. TestMapIterSet's other 3,088 uncounted bytes still have to go
before it can read zero.

## Risks

- **A wrong or drifted oracle aliases memory.** Under S2, a site that escapes in fact but is treated as stack-kept
  shares its slot with the next evaluation. That corrupts memory silently, which is the worst failure in this design.
  The defences:
  - the oracle is read per target at the pinned toolchain;
  - a site the oracle does not name stays on the heap;
  - a **checked mode** runs the battery with released slots poisoned, so a stale pointer throws instead of reading a
    reused object.
- **Targets and instantiations disagree.**
  - Decisions differ per GOOS (355 files in the union, 303 in all three targets), so emission is per target, as
    `stackAppend`'s is.
  - Go decides per generic shape, and one std site in `crypto/ecdsa` both escapes and stays across instantiations.
    A mixed site stays on the heap.
- **Reachability.** A released slot that still references objects keeps them alive where Go's dead stack does not.
  Slots whose type holds references are cleared on release, and runtime's reachability tests are the gate.
- **Cost.**
  - Entry, exit and zeroing are paid on every call of a function with stack sites, and zeroing a 1 KB
    `ringElement` slot is not free.
  - The Performance suite (JIT and Native AOT) is read before and after each increment.
  - A function with no stack sites pays nothing.
- **Unwinding.** The release runs from a `using` scope, so panics, `recover` and `Goexit` release slots in order. A
  frame that recovers keeps its own reservation.
- **Aliasing under C.** A `ref` alias must never outlive the callee or be stored. These are Phase A's rules, applied
  to methods, and the differential guard holds both forms to Go.
- **The 64 KB null-byref audit** applies to any `[InlineArray]` layout under S1, as the zh-box record requires.
- **Footprint.**
  - The OWN marker touches up to 355 std files, 675 test files and 397 goldens.
  - B1 adds the callers' files and the generated twins.
  - Each increment rides one deliberate re-baseline, measured with the two-seeded corpus diff before it is cut.

## Measurement plan

1. **Attribute before cutting.** Read a per-site split for every cited entry on windows and linux, at Release with
   tiering off. Do this first for the unattributed slog rows, TestReplace's six-allocation case and mldsa's split
   between this record and REC-A.
2. **Control the oracle both ways**, as REC-B requires:
   - a planted escaping site must keep its allocation;
   - only "does not escape" sites may be admitted;
   - a probe with one site of each class must give each class at its own position and at an inlined call.
3. **Predict, then measure.** Before each cut, predict the file and golden footprint, then measure it with the
   two-seeded diff and CNR.
4. **Read the entries before and after** on both OSes.
   - A want-zero entry retires only at zero bytes. A bounded entry retires on its count.
   - mldsa is read as a range over runs, because its loop count is random.
5. **Run a negative control for each retirement.** Revert the increment, confirm the entry turns red and names its
   site, then restore and verify byte-identical output.
6. **Run checked mode** over std's tests and the behavioral corpus before S2 seats, and again before B1.

## Open questions

1. **Who writes the twins?** go2cs-gen (the visible file keeps one body), or the converter (simpler, but two bodies
   in the emitted file). This record recommends the generator.
2. **Does §10.1 widen?** Increment C needs Phase A's `ref` form on same-package methods, exported ones included, as a
   dual emission beside the boxed form.
3. **Does the converter devirtualize?** sha256's last allocation needs Go's devirtualized call, where Go knows the
   concrete type after inlining.
4. **Is checked mode a battery default** or a per-increment gate?
5. **Does `stackAppend`'s buffer move onto S2?** Its buffer allocates bytes on each call today, so it fails
   want-zero asserts.

<!-- Provenance. Lane C2, 2026-10-25, worktree at b5fc1294db (origin/claude/hop-go126). Sources: C2's sizing census
(COORD inbox 2026-10-21T13:07:46Z: go1.26.10, three amd64 targets, method -gcflags='-m -d=variablemakehash=vy' with
go/types classification, controlled by a one-site-per-class probe); R's karatsuba finding (COORD inbox
2026-10-21T07:59:24Z, windows); G's TestMulUnbalanced rooting (src/core/math/big/go2cs_test_disclosures.json:11
(`5,723,096 B for a 400,320 B input, a ratio of 14`)); lane H's readings (docs/phase4/DATA-hop126-deferred-allocs.md).
The harvest the oracle reuses: src/go2cs/stackAppendHints.go:695 (`"-gcflags=-m -d=variablemakehash=vy"`). Go
sources read at the pinned toolchain: GOROOT/src/math/big/natmul.go:164 and GOROOT/src/math/big/natmul.go:257 (karatsuba, karatsubaSqr),
GOROOT/src/math/big/escape_test.go:43-50 (TestSqrAlloc's three sizes), GOROOT/src/crypto/sha256/sha256_test.go:474-487,
GOROOT/src/crypto/internal/fips140/sha256/sha256.go:149-153 (New), GOROOT/src/crypto/internal/fips140/mldsa/mldsa.go:451-540
(the signing loop's constant-cap makes), GOROOT/src/log/slog/logger_test.go:358-367, GOROOT/src/strings/strings.go:1151
(`var b Builder`). The sha256 attribution of four (one Digest box, its two field arrays, one make) is an inference
from the shapes and the 800 B reading; the per-site split is owed. src/core/crypto/internal/fips140/sha256/sha256.cs:153-154
(`public static ж<Digest> New() {`…`var d = @new<Digest>();`). -->
