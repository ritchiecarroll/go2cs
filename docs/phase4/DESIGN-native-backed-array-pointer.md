# DESIGN — Q58 / W2b: a pointer-to-array that names NATIVE memory

**Status:** design only. No code in this commit. Cut as increment 8's read half, paired with the W1
write form (below). Measured on Linux at `8432f9cbb` (increment 7's rows); the same shape is
platform-neutral.

## The wall, measured rather than argued

Eight `runtime` page-alloc rows exit **139 with a blank stderr** — the mute class, no handler, no
deadline, no results file. A dump reads the frame (framework-dependent host, `createdump` beside
`libcoreclr`, `dotnet-dump … clrstack`; `gdb` for registers):

```
grow (mpagealloc.cs:392) -> chunkOf -> [PrestubMethodFrame]
    ж<array<pallocData>>.at<pallocData>
```

`rdi = 0`, `si_addr = 0`: a CLR **prestub null read**, not a Go nil-pointer panic. The chunk-table
element it dereferences was filled one line earlier, at `mpagealloc.cs:390`:

```csharp
(Ꮡ(Δp.chunks, (int)(c.l1())).Reinterpret<ж<array<pallocData>>, uintptr>()).Value = (uintptr)r;
```

which is the faithful conversion of Go's own store (`mpagealloc.go:420`):

```go
// Store the new chunk block but avoid a write barrier.
// grow is used in call chains that disallow write barriers.
*(*uintptr)(unsafe.Pointer(&p.chunks[c.l1()])) = uintptr(r)
```

Go writes through a `*uintptr` view **only** to dodge the write barrier — its comment says so. The
value stored is an ordinary pointer to `sysAlloc`'d, off-heap memory, and Go's GC never tracks it,
which is exactly why the barrier can be skipped. The conversion reproduces the store literally, and
that is the defect: the slot's static type is `ж<array<pallocData>>`, a **managed reference**, and a
raw native address has been written into it.

## Why the obvious shape cannot work — and this refutes my own earlier shorthand

My increment-7 note sized W2b as "a `NativeBox<array<pallocData>>` over native memory". Reading the
types refutes it:

- `array<T>` is `public readonly struct array<T>` whose first field is `internal readonly T[]
  m_array` — a **managed array reference**.
- Go's `*[N]T` points **directly at N contiguous elements**, with no header of any kind.

`NativeBox<T>.Value` is `ref Unsafe.AsRef<T>((void*)m_nativeAddr)`, so `NativeBox<array<pallocData>>`
would reinterpret the first bytes of the element block **as an `array<T>` struct** — i.e. read a
`pallocData` bit-pattern as a `T[]` reference. That reference is garbage, and the first use of it is
the prestub read the dump shows. `NativeBox<array<T>>` is not merely unhelpful here; it is precisely
the crash. A native-backed *pointer-to-array* is a different thing from a native box *of* an array.

Contrast the slice case, which already works: `slice<T>` is a header (base, len, cap), so it CAN be
rebased onto native memory — `slice<T>.OverNativeMemory(address, len, cap)`, and increment 7's
`HeaderSliceBox<T, TDst>` does exactly that for the three-word header shape. There is no analogous
move for `array<T>`, because there is nothing in an `array<T>` to rebase.

## The mechanism, end to end

```
at<Telem>(index)                    ж.cs:357   — public, NOT virtual
  -> arrayView<Telem>()             ж.cs:290   — PRIVATE
       -> this.Value                            — reads native element bytes as array<Telem>
            -> m_array               garbage managed reference
  -> array.IndexIsValid(index)                  — first touch of the garbage reference
  -> [PrestubMethodFrame] -> SIGSEGV -> exit 139, blank stderr
```

The bounds check is the first *use*, which is why the death lands inside `at` rather than at the
store: the corrupted slot is written at `:390` and only detonates when `chunkOf` reads it at `:392`.

## The design question this actually poses

Not "which existing box do we use" — none of them fits — but: **how does a `ж<array<T>>` supply an
element view that is native-backed?** Today it cannot. `at<Telem>` is non-virtual and
`arrayView<Telem>` is private, so no `ж<T>` subclass can intercept the element door. That is the one
thing Q58 must change, and it should change it as narrowly as possible.

### Options

**(A) Make `at<Telem>` virtual; a new `NativeArrayBox<T> : ж<array<T>>` overrides it.**
Direct, but it makes the corpus's hottest element door a virtual call for *every* box kind, and it
duplicates the bounds check and the `ElemRefBox` construction in the override — two implementations
of one rule, which is how the two drift.

**(B) One virtual consultation inside `arrayView<Telem>()`; `at` stays non-virtual.** RECOMMENDED.
A new `internal virtual IArray<Telem>? nativeArrayView<Telem>(nint minLength) => null;` on `ж<T>`,
consulted **before** the managed-view path. The native-backed box answers it with
`slice<T>.OverNativeMemory(base, len, len)` — which is already an `IArray<T>`, since
`ISlice<T> : IArray<T>` — and `at` keeps its single implementation, its single bounds check, and its
single `ElemRefBox` construction. This mirrors the virtual-seam idiom `ж<T>` already uses
(`TryGetElementStorage`, `TryGetElementWindow`, `TryPinnedReinterpret`, `ArrayRef`), and it reuses
W2a's native-slice machinery instead of minting a second native array implementation.

**(C) Change the converted TYPE of the chunk-table slot** so it is native-capable by construction
(e.g. the table holds a native-pointer-shaped value rather than `ж<array<T>>`). Cleanest in
principle and the widest blast radius in practice — a converter type-mapping change reaching every
`*[N]T` field in the corpus. Not for this increment; recorded so the next reader knows it was
considered and why it was declined.

### The byte rule binds the choice

`ж<T>` is the corpus's per-box base, and CLAUDE.md's measured rule is explicit: **instance state
added to `ж<T>` (or any per-box base) is a corpus-wide byte cost**, +8 B on every pointer box.
Option (B) therefore adds a **virtual method returning null**, never a field: the address and length
live on the new subclass alone, and boxes that are not native-backed pay nothing but the (already
present) vtable slot. Any variant of this design that puts the address on the base is refused on
that rule.

## Increment 8 is a PAIR, and the halves fail differently

- **Write half (W1 form).** The store at `:390` must MINT a native-backed box into the slot rather
  than punching a raw address through a `uintptr` view. Until it does, the read half has nothing
  correct to read: a native-backed `at` over a slot still holding a raw address is the same crash
  one frame later.
- **Read half (W2b).** Option (B) above.

Landing only one half is not a partial improvement — it is the same exit 139 with a different frame,
so both halves land together or neither does.

## Acceptance, and what would falsify it

**Acceptance:** the eight page-alloc rows that today exit **139 with a blank stderr** produce
verdicts. That is the whole claim; passing is a separate question, since a verdict may still be a
fail for an unrelated reason and would then be attributed on its own evidence.

**Falsifiers, named before the cut:**
1. A row that still exits 139 with a blank stderr — the pair did not reach the seam; re-dump before
   theorising.
2. A row that reaches a verdict but dies inside `arrayView` on a *different* frame — the native view
   is being consulted but is mis-sized (`len` wrong), i.e. the length source is wrong rather than the
   mechanism.
3. `runtime`'s other rows moving at all — the virtual seam is being taken by boxes that should have
   fallen through to the managed path; the `=> null` default is not doing its job.
4. Any measurable byte movement on an alloc-assert row — a field reached the base after all.

## Cost, stated rather than assumed

One virtual call is added to `arrayView`'s path — not to `at`'s fast path *as such*, but `arrayView`
is on every `at`. The honest position is that this is **unmeasured**, and the increment owes an
alloc/timing reading on a row that indexes through a pointer-to-array in a loop, compared against the
same row at the base. It is not obviously free and should not be described as free.

## Open, and deliberately not decided here

- Where the native view's **length** comes from. Go knows it statically (`[1 << pallocChunksL2Bits]`)
  and the converted type carries it in `array<T>`'s type identity, not in the box. The write half
  mints the box and therefore knows the length at the store; whether that is the general answer for
  every `*[N]T` over native memory, or only for this seam, is the first thing the cut must settle.
- Whether the seam should refuse (loudly) a native view whose requested index exceeds the minted
  length, or fall through. A refusal is the safer direction and matches the corpus's habit of failing
  by name rather than reading a stranger's memory.

---

## Amendment, 2026-09-05 (same day, before the cut): the write half CANNOT be golib-side

The record above left the write half as "must MINT a native-backed box into the slot" without saying
*where* that happens, and named it the first thing the cut must settle. Settled here, by reading the
three pieces rather than by running anything — so it is a **structural** finding, not a measurement,
and it is recorded as such.

**The store is a `ref` write, and a `ref` write is invisible to golib.**

1. The Go LHS `*(*uintptr)(unsafe.Pointer(&p.chunks[c.l1()]))` is a star-expr, and the converter's
   deref path emits `.Value` for it (`convStarExpr.go:26`, `derefAccessor := ".Value"`).
2. `ж<T>.Value` is a `ref T` property. The assignment therefore writes **through the ref**, directly
   into whatever storage the ref names.
3. `Reinterpret<ж<array<T>>, uintptr>()` takes its aliasing arm and returns a `FieldRefBox<uintptr>`
   over the *same managed storage* (`ж.PointerExtensions.cs`, the `ReinterpretAliasesStorage` branch)
   — so the ref names the chunk-table slot itself, and `= (uintptr)r` overwrites a managed reference
   with a raw address.

There is no hook in between. A box cannot observe a write performed through a `ref` it handed out, so
**no golib change can convert this store into a box mint**. The tempting shape — have the reinterpret
seam notice that the destination slot is pointer-typed and mint a native box instead of aliasing —
fails on the same fact: the seam runs when the *view* is taken, not when the value is *stored*, and
by then it has already surrendered a `ref`.

**Consequence for increment 8, and it is a scope statement, not a detail.** The write half is a
**converter emission change** (W1's form, extended to a pointer-typed destination), not a golib one.
So increment 8 spans both sides of the tree:

- converter: recognize this store shape and emit a box-minting assignment to the element instead of a
  `uintptr` ref-write — which means the increment owes the converter's own gates (converter suite,
  CNR, and a two-seeded three-target `-stdlib` diff for the emission footprint);
- golib: the option-(B) read seam and the native-backed array box, which owe the full behavioral suite
  and GolibTests per route #7's twin.

That is a materially larger increment than the read half alone, and the halves still land together —
a converter that mints a box the read seam cannot serve, or a read seam over a slot still holding a
raw address, are both the same exit 139 one frame over.

**What this does NOT settle**, and the cut still owns: where the minted box's **length** comes from
(the write site knows it — Go's `l2Size` is right there — but whether that generalises to every
`*[N]T` over native memory is unanswered), and whether an over-length index refuses loudly or falls
through.

---

## Amendment, 2026-09-05 (after the cut): both halves landed, the acceptance MET, and a WALL ONE LAYER DEEPER named

Increment 8 is cut as two commits on one branch — the read half (option (B)'s virtual seam plus
`NativeArrayBox`) and the write half (the converter's recognition of the write-barrier-dodging store
plus the `builtin.NativeArrayPointer` door) — with the ElemRefBox native-slice predicate split out
ahead of them as its own seat, because the read half's guard exposed it and it is golib's own defect
rather than this increment's.

### The emission, measured on all three targets

A two-seeded three-target `-stdlib` A/B (both arms built from frozen `git archive` snapshots, every
seed taken from one frozen snapshot BEFORE any arm converted, each arm's write-evidence recorded):

| target | differing files | only-in | removed / added lines | `GoPositionMap` lines |
|---|---|---|---|---|
| windows | 1 (`runtime/mpagealloc.cs`) | 0 | 1 / 1 | 0 |
| linux | 1 (`runtime/mpagealloc.cs`) | 0 | 1 / 1 | 0 |
| darwin | 1 (`runtime/mpagealloc.cs`) | 0 | 1 / 1 | 0 |

The content witness — a hand-owned file neither converter writes — is byte-identical in all six
roots, so no arm seeded from a tree the other had moved. Zero position-map lines is the right
reading for a change that replaces one line with one line.

### The acceptance, measured as a before/after ON ONE BOX

The design's acceptance was "the eight page-alloc rows that today exit 139 with a blank stderr
produce verdicts". Measured with the same gated filter, deadline and configuration on both arms,
rather than quoted from an earlier host's record:

| arm | rows | EMPTY C# verdicts | record status | results file |
|---|---|---|---|---|
| BASE (the split seat, increment 8 absent) | 88 | **88** | `conversion-blocked` | **none written** |
| HEAD (both halves) | 88 | **0** | `failing` | written |

**The acceptance is MET**, and over a wider family than the eight the design named: the gated set
`^Test(PageAlloc|PageCache)` is 88 rows, every one of which now produces a verdict where every one
was previously mute. Twelve of them (the `TestPageCacheAlloc` family) pass on both sides.

### THE ROWS STILL FAIL, and the reason is now VISIBLE — which is the finding

The remaining rows read `Go="pass" C#="fail"`, and the failure names itself:

```
panic: native-backed slice: element type pallocData contains managed references
       and cannot alias native memory
    at go.slice`1.OverNativeMemory(...)
```

That refusal is CORRECT and golib is right to make it. `pallocData` embeds `pallocBits` and holds
`scavenged pageBits`, and `pageBits` converts as `[GoType("[8]uint64")] partial struct pageBits` —
whose managed representation is an `array<uint64>`, i.e. **a managed `uint64[]` reference**. So the
converted `pallocData` is not layout-compatible with the native block at all, and no native view of
a `*[1 << pallocChunksL2Bits]pallocData` can be sound however the pointer is minted.

**So increment 8 converts a MUTE crash into a NAMED refusal on 88 rows. It does not, and cannot,
make them pass.** The design separated those two claims ("passing is a separate question") and this
is exactly that separation being cashed — but the residual is not an unrelated failure, it is the
same seam one layer down, and it is recorded here rather than left for the next reader to rediscover.

### Falsifiers, scored

1. *"A row that still exits 139 with a blank stderr"* — **did not fire**. No row is empty and the
   results file was written on the HEAD arm; it is the BASE arm that has no results file.
2. *"A row that reaches a verdict but dies inside `arrayView` on a different frame — the native view
   is mis-sized (`len` wrong)"* — **half right, and the half it got wrong is the interesting half**.
   The rows do die under `arrayView`, in its callee `OverNativeMemory`; the length is NOT wrong (the
   mint carries Go's own `N`, 8192, read from the destination type). The cause is the element type,
   which the falsifier did not anticipate.
3. *"`runtime`'s other rows moving at all"* — **NOT MEASURED**. The run was gated to the page family,
   which is diagnostic only and cannot answer a question about rows it did not run.
4. *"Any measurable byte movement on an alloc-assert row"* — **NOT MEASURED here**.

### The two OPEN questions above are now settled

- **Where the native view's length comes from.** From the **destination type**, not the store site.
  The write site happens to know it (`l2Size` is one line up), but the type knows it everywhere, so
  the rule is general for every `*[N]T` rather than a transcription of this one call.
- **Whether an over-length index refuses or falls through.** It **refuses by name**, and the read
  half's guard holds an arm for it.

### What is NOT built, and why

The 20 occurrences of the dodging store in the pinned GOROOT have a pointer-to-**array**
destination at exactly one — this seam. The other 19 store into pointer-to-struct, pointer-to-byte
and `unsafe.Pointer` slots, and 12 of those reach the same aliasing reinterpret in today's emission:
the same class, one type kind over, latent in the same way. None has a measured failing row, so none
is served, and the converter's predicate keys on the DESTINATION TYPE so that widening it is a
change of scope rather than a tweak.

The wall this amendment names — the converted representation of a fixed-size array VALUE field, and
therefore of every struct that holds one — is a golib and emission MODEL question far larger than
this increment, with its own evidence still to gather. It is recorded, not built.
