# The go2cs runtime allocation counter — site census and coverage statement

> Landed r58a (2026-08-09); the `@string` census §5 item 3 deferred was taken in the same lane once
> r57c's `@string` window merged (2026-08-10). Companion to `src/core/golib/AllocationCounter.cs`
> (the mechanism), `testing.AllocsPerRun` in `src/core/testing/testing.cs` (its only consumer) and
> `src/tests/GolibTests/AllocationCounterTests.cs` (the guard that holds §4 to its word). Read
> [`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md)'s AllocsPerRun rows
> for the packages this unblocks and the ones it does not.

## 1. Why a counter in golib at all

Go's `testing.AllocsPerRun` reports a malloc COUNT — a delta of `runtime.MemStats.Mallocs`. r56d
established, by measurement rather than assertion (net9.0/9.0.18, x64), that the CLR publishes no
in-process allocation count: the whole public GC surface is byte totals, the one event whose payload
would be a count (`GCSampledObjectAllocation`) raises nothing to an in-process `EventListener` under
any configuration tried, and runtime events arrive asynchronously besides — fatal for a synchronous
call regardless. The full survey lives on the `AllocsPerRun` declaration.

The decisive observation r56d left on the table is that **Go's Mallocs is not a platform facility
either**. It is a counter the Go *runtime* keeps at its own allocation sites. golib is that runtime
here. Counting in golib is therefore the structural mirror of what Go already does, not an
approximation of it.

r56d also proved the trap: a partial counter covering `ж`/`array`/`slice` only reported "≥2 objects"
for a `log` path that had allocated 424 bytes — arithmetic that cannot close, so the instrument was
demonstrably blind to sites on that path. **A count that silently omits allocation sites is worse
than an honest byte figure.** What makes the number honest is not totality — which is unreachable,
for the reasons in §4 — but a STATED coverage boundary plus a cross-check that refuses to report a
number the boundary invalidates (§5).

## 2. The unit: CLR heap objects, not "Go allocations"

The counter counts **managed objects handed to the CLR heap**, because that is the quantity that can
be audited against `GC.GetAllocatedBytesForCurrentThread`. A "logical Go allocation" would be a
model, and a model is precisely the quiet fiction the counter exists to replace.

Where one Go malloc becomes two CLR objects, the count says two. The canonical case is `ж<T>` over an
unmanaged `T`: Go's `&x` is one malloc, while the box additionally allocates the one-element pinnable
slot its address stability requires (see the `m_slot` commentary in `ж.cs` for why that allocation
cannot be deferred or migrated). That difference is real allocation behavior and belongs in the
number.

## 3. Thread scoping — and why not interlocked

The count is **per-thread** (`[ThreadStatic]`), matching the byte instrument it accompanies:
`GC.GetAllocatedBytesForCurrentThread` is inherently thread-scoped, and the shim's own documentation
calls that scoping the stand-in for the `runtime.GOMAXPROCS(1)` pinning Go's `AllocsPerRun` uses to
keep other goroutines out of its measurement.

A process-global interlocked counter would have been the obvious shape and is the wrong one twice
over: it reintroduces exactly the cross-thread pollution the byte figure is already free of, and it
pays a lock-prefixed round trip per allocation to do it. One thread is the sole writer of its own
slot, so no synchronization is owed at all.

The same caveat the byte measurement carries applies unchanged: `f` is assumed single-threaded, and
allocations made by goroutines `f` spawns run on other threads (converted goroutines share the thread
pool) and are not observed.

## 4. The census — what IS counted

Every site below charges the number of objects stated, at the site, with the arithmetic in a comment.
Backing stores go through `AllocationCounter.NewArray`/`CopyOf` so that counting is a **substitution
rather than an insertion**: a site cannot allocate through them and forget to charge.

| File | Site | Charge |
|---|---|---|
| `ж.cs` | `ж(in T)` — managed `T` | 1 (the box) |
| `ж.cs` | `ж(in T)` — unmanaged `T` | 2 (box + eager `T[1]` pinnable slot) |
| `ж.cs` | `ж(in T, bool isNull)` | 1 / 2, same two shapes |
| `ж.cs` | `ж(object, FieldRefFunc<T>, Delegate?)` | 1 (the box; the tuple is inline, the accessor is the caller's) |
| `ж.cs` | `ж(IArray, int)` | 1 (the box; the array is the caller's) |
| `ж.cs` | `ж(nuint, PinnedBuffer?)` | 1 (the box; native memory is not CLR heap) |
| `ж.cs` | `ж(NilType)` | 1 (Go's nil pointer allocates nothing — ours allocates an object, and the count says so) |
| `builtin.cs` | `Ꮡ(IArray<T>, int/nint)` | +1 for the caller's boxing temp (see note below) |
| `builtin.cs` | `@new<T>(params object[])` | +1 for the boxed instance `Activator` returns |
| `array.cs` | length ctors (`int`/`nint`/`ulong`), element-factory ctor | 1 backing array |
| `array.cs` | slice→array conversion ctor, `Span`/`ReadOnlySpan`/`Memory`/`ReadOnlyMemory` ctors | 1 copy |
| `array.cs` | `WindowArray` (alias window only), `ToArray`, `Clone` | 1 copy |
| `array.cs` | `array(IEnumerable<T>[, int])` extensions | 1 materialized array |
| `slice.cs` | `slice(nint length, nint capacity, nint low)` — i.e. `make([]T, …)` | 1 backing array |
| `slice.cs` | `Span`/`ReadOnlySpan`/`Memory`/`ReadOnlyMemory` ctors, `ISlice<T>` foreign copy, `IByteSeq<T>` copy | 1 copy |
| `slice.cs` | `Source` (and `ToArray` through it) | 1 copy |
| `slice.cs` | `Append` — nil-source path and the beyond-capacity grow path | 1 backing array |
| `slice.cs` | `From<TSource>` converting path, `ISlice.Append(object[])`, `slice(this Span/IEnumerable)` | 1 |
| `map.cs` | all four constructors | 1 (the store) |
| `channel.cs` | `ChanCore(nint)` base ctor | 4 (the core, its lock, both wait queues) |
| `channel.cs` | `ChanCore<T>(nint)` | +1 ring buffer when buffered |
| `channel.cs` | blocking send / blocking receive park | 2 (waiter + its park semaphore) |
| `channel.cs` | blocking select | 2 (select state + semaphore) + 1 waiter array + 2 per live case |
| `string.cs` | `@string(ReadOnlySpan<byte>)`, `@string(IByteSeq<byte>)` | 1 backing copy |
| `string.cs` | `@string(char[])` | 2 (the intermediate UTF-16 `string`, then its UTF-8 backing) |
| `string.cs` | `@string(string?)` — the site every C# literal passes through | 1 UTF-8 backing |
| `string.cs` | `ToString()` / `implicit operator string` | 1 materialized `System.String` |
| `string.cs` | `ToRunes()` | 1 copy, +1 when the estimate exceeds the stackalloc threshold |
| `string.cs` | `buffer` (`unsafe.StringData`) | 1 `PinnedBuffer`, +1 materialized copy on a windowed string |
| `string.cs` | `operator +` ×3 (`@string` / `ReadOnlySpan<byte>` operands) | 1 result buffer |
| `string.cs` | `implicit operator slice<byte>`, `implicit operator byte[]` — Go's copying `[]byte(s)` | 1 copy |
| `string.cs` | `slice<rune>` / `slice<char>` / `char[]` conversions | 1 materialized array |
| `string.cs` | `GetEnumerator()` and the explicit `IEnumerable<rune>` / `IEnumerable<char>` forms | 1 enumerator |
| `builtin.cs` | `ToUTF8Bytes(ReadOnlySpan<rune>)` | 1 copy, +1 when the estimate exceeds the stackalloc threshold |

**`this[Range]` — Go's `s[a:b]` — charges NOTHING, and that is a RESULT rather than an omission.**
Since r57c an `@string` is a WINDOW (backing + offset + length), so slicing one allocates nothing and
copies nothing, exactly as in Go. Before that rewrite every sub-string copied, and this census would
have carried its busiest site here.

**`tmpstring(slice<byte>)` — Go's `m[string(b)]` map-READ key — charges NOTHING, the second
zero-by-design row (L11).** The Go compiler skips the string copy for a map-lookup key because the
key provably does not outlive the lookup (`runtime.slicebytetostringtmp`); the converter emits
golib's `tmpstring` for exactly that shape, which windows the slice's LIVE backing through
`@string.TransientAliasOf` — no copy, no charge, in either unit. Every path where the string
ESCAPES (return, store, map WRITE key) keeps the copying conversion above. net/textproto's
`canonicalMIMEHeaderKey` common-header probe is the shape that forced it: a want-ZERO
`AllocsPerRun` assert over a path whose only allocation was this key's backing copy.

`Ꮡ(IArray<T>, index)` is the one site that charges an allocation emitted *outside* golib. A
`slice<T>`/`array<T>` header is a struct, so `Ꮡ(s, i)` — the only shape the converter emits for Go's
`&s[i]` — boxes one on every call. The box is created at the call site, but this overload is the only
thing it can be handed to, so charging it here attributes it exactly. A caller that already held an
`IArray<T>` reference would be overcharged by one; the converter never emits that shape, and
overstating a cost is the safe direction for a budget assert.

`Ꮡ`, `heap`, `@new` and `of`/`at` all route through the `ж<T>` constructors, so they are charged
exactly once and never twice. An empty `ReadOnlySpan.ToArray()` returns `Array.Empty<T>()` and is
charged nothing, as is the `[]` collection-expression form — reporting an allocation that did not
happen is the same class of untruth as missing one that did.

> **2026-09-05 amendment (the field-view cache, `golib/ж.Views.cs`, design `DESIGN-field-view-cache.md`):** the
> `ж(object, FieldRefFunc<T>, Delegate?)` row is UNCHANGED -- the constructor still charges one object -- but
> `of()` now reaches it ONCE per (box, accessor token) and answers the cached view afterwards, so a per-CALL
> census of `of()` reads 1 on a box's first call for a field and 0 on every repeat.
> `AllocationCounterTests.FieldReferenceMintChargesBoxAlone` pins the constructor row directly (its earlier
> spelling counted the SECOND `of()` on one box -- the harness warms up first -- which the cache answers for
> free, and read 0 against its 1 on the cut's first gate run); `FieldReferenceRepeatOfChargesNothing` pins
> the repeat at 0 objects / 0 B.

## 5. What is NOT counted — the coverage boundary

This is the load-bearing half of the document. Four classes are outside the counter, three of them
structurally:

1. **Allocations the C# compiler emits in CONVERTED code.** Closure display classes and delegate
   objects for func values and deferred calls, `params object[]` arrays at variadic call sites, and
   boxing at interface-conversion sites. golib never sees these; they are emitted by the compiler in
   the converted package's own assembly. This is the largest and least tractable class.
2. **BCL internals behind a golib call.** `Dictionary`'s bucket and entry arrays, a `List<T>`'s
   growth buffers, `Encoding.GetString`'s working storage, a `SemaphoreSlim`'s internal objects, a
   `Task` the thread pool mints. golib charges the object it asked for, not the ones that object
   allocates for itself.
3. ~~**`@string` materialization — a DEFERRED gap, not a structural one.**~~ **CLOSED** once r57c's
   `@string` window landed, exactly as this section said it should be: the census above now covers
   `string.cs`'s materializing constructors, `Encoding.UTF8.GetBytes`/`GetString`, `ToRunes`, the
   three `operator +` overloads, the defensive `[]byte(s)` copies, the enumerator entry points and
   `builtin.ToUTF8Bytes`. `this[Range]` needed no instrumentation at all — after r57c it allocates
   nothing. What this changed in practice is recorded in §7.
4. **Hand-owned `*_impl.cs` package code that does not route through golib.**

golib's own reflection, adapter and enumerator machinery (the `ConcurrentDictionary` caches, the
per-call capturing lambdas in `TypeExtensions`, the iterator state machines) is likewise uncharged.
Those are cold or memoized in the cases that matter, but "cold" is a judgement and the boundary
statement should not rest on it: treat them as uncounted.

**Consequence: the count is a LOWER BOUND on the true object count**, and every note it emits says
so.

## 6. The cross-check that makes it safe

`AllocsPerRun` keeps measuring bytes alongside the count and decides between them:

- **zero bytes** ⟹ zero allocations, exactly, in both units. Returns `0` and emits no note, so every
  assert-zero test — the dominant stdlib shape — keeps its output byte-identical to the byte-only
  shim. This is why no banked row's terminal text moves.
- **nonzero bytes, nonzero count** ⟹ the COUNT is reported, floored at 1.
- **nonzero bytes, ZERO count** ⟹ allocations happened that the counter did not see, so the
  byte-derived figure is reported instead. Reporting the zero would be a **false pass**, which is
  worse than the byte figure it would replace.

The floor at 1 is inherited from the byte-only shim and kept for the same reason (amortized
sub-one-per-run allocation must never masquerade as exact zero). It also makes the change
**monotone**: a counted object always costs at least the CLR's 24-byte object minimum, so
`count < bytes` always holds and the reported value can only fall, never rise. **No test that passed
on bytes can fail on the count** — which is what bounds the sweep risk of this change to zero.

## 7. What this does and does not unblock

The count is decision-grade wherever the true figure is orders of magnitude from the assert's budget,
because no amount of under-counting can flip the verdict there. It is **not** decision-grade where
the budget is one or two objects and the residual could be the difference:

| Package | Assert | Verdict |
|---|---|---|
| `crypto/rsa` `TestAllocations` | `allocs > 10` | Decision-grade. Five orders from the budget. |
| `crypto/internal/nistec` `TestAllocations` | `want 0` | Decision-grade. r56d's byte-exact bill closed with `ж`/`array`/`slice` alone, so the uncounted classes are provably ~absent on this path. |
| `log` `TestDiscard` | `at most 1` | Re-measured with the `@string` census — see the row below. |
| `net/http/internal` `TestChunkReaderAllocs` | `want 1` | Re-measured with the `@string` census. |
| `log/slog/internal/buffer` `TestAlloc` | small budget | Re-measured with the `@string` census. |

Reporting the bottom three as bankable on a *lower-bound* count near a budget of one would be exactly
the laundering r56d refused to do, which is why they waited for the `@string` census rather than being
disclosed on the partial one. With that census taken, what remains uncounted on their paths is the
compiler-emitted class (§5 item 1) — structural, and the reason each is re-measured and reported
rather than self-ruled.

**The census gap the guard found.** `ByteSeqExtensions.ToSlice`/`ToGoString` — the `[]byte(s)` and
`string(s)` conversions the converter emits for a `string | []byte`-constrained value — materialized
their copies without charging them. A `parseRFC3339`-shaped body measured **192 B/parse and ZERO
counted objects**, arithmetic that cannot close, which is precisely the failure mode r56d warned
about. It is instrumented now and the shape charges exactly its six conversions. The lesson is that
the census is only as good as what checks it, which is what
`GolibTests/AllocationCounterTests.cs` is for: every row of §4 is asserted there as an exact object
count, and the monotonicity invariant (`count ≤ bytes / 24`) that makes reporting the count in place
of the byte figure safe is asserted across twelve shapes.

## 8. Overhead

Measured r58a on the desktop (net9.0 Release, 3,000,000 iterations, best-of-5, medians of three
interleaved rounds against master's uninstrumented golib):

| shape | master | counting OFF | counting ON |
|---|---|---|---|
| `new ж<long>` (box + slot) | 6.249 ns/op | 6.223 (−0.4 %) | 6.962 (+11.4 %) |
| `new slice<byte>` (make) | 2.284 ns/op | 2.328 (+1.9 %) | 2.624 (+14.9 %) |
| append past capacity (grow) | 3.220 ns/op | 3.320 | 3.138 (within noise) |

**Counting off is within noise of master**, which is what the gate is for: a converted application
that never runs a test pays one static `bool` read and one never-taken, perfectly-predicted branch,
and never touches the thread-static slot. **Counting on costs 11–15 %** on the tightest allocation
loops — real, and the reason the counter is off by default and enabled only by
`TestHost.Run`. The overhead does not distort the measurement, which is a delta.

The table was taken on the `ж`/`slice` shapes before the `@string` census, and the census does not
change its shape: every added site is the same one static read plus a never-taken branch when
counting is off, and every one of them sits next to an allocation that already dominates it. The
`@string` sites are if anything cheaper relative to their work — a `GetBytes` or a backing copy is
far more expensive than the `ж` box the 11 % was measured against.

The benchmark defeats .NET 9's stack-allocation optimization deliberately: with a literal `16` the
`make` loop measured 0.187 ns/op because the backing array was never heap-allocated at all. The size
is a non-const static for that reason.
