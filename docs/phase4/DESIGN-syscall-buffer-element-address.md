# DESIGN -- the syscall buffer's element address: candidate E of the `os` want-zero residue (segments 32 and 35)

> Drafted 2026-09-05 by lane G against the GA tip (`claude/g-elem-take-concrete` @ `955e271c0`, seated train 29) and Q49's verified branch (`claude/c2-q49-cut` @ `d5645ab97`, seated train 29), as COORD ruled in `806cde93a` -- "the same way" as candidate B's record (`DESIGN-syscall-out-parameter.md`). Every price that depends on either seat's LANDED code is marked **re-read at the landing** and takes a dated block here when train 29 lands. Nothing is cut by this record.

## 0. The site, read at the emission on the GA tip

The generated `//sys` wrapper `writeFile(ΔHandle handle, slice<byte> buf, ж<uint32> Ꮡdone, ж<Overlapped> Ꮡoverlapped)` opens with `mksyscall`'s one buffer idiom -- Go's `var _p0 *byte; if len(buf) > 0 { _p0 = &buf[0] }` -- emitted as

```
ж<byte> _p0 = default!;
if (len(buf) > 0) {
    _p0 = Ꮡ(buf, 0);
}
var ᴋ154 = _p0;
var (r1, _, e1) = Syscall6(procWriteFile.Addr(), 5, (uintptr)handle, (uintptr)ᴋ154, (uintptr)len(buf), (uintptr)ᴋ155, (uintptr)ᴋ156, 0);
System.GC.KeepAlive(ᴋ154);
```

After candidate A the element take `Ꮡ(buf, 0)` is ONE object -- the 64 B `ElemRefBox` (segment 32; before A it was two, the box and the caller's interface temp) -- and its conversion `(uintptr)ᴋ154` goes through the retaining `uintptr` operator: `EnsureStableAddress`, a `PinnedBuffer` over a `GCHandle` (segment 35, 56 B, uncounted), released on the box's finalizer schedule, with `KeepAlive` (Q49's predicate) holding the box across the call. On the unix flavours the same idiom is spelled through the unsafe assembly -- `_p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0))` (the retaining door of `f349b3499`), `_p0 = @unsafe.Pointer.FromBox(Ꮡ_zero)` for the empty case -- and the box is the same. One element box and one pin holder per kernel call whose only purpose is to hand the kernel a stable address for the buffer's first byte, for exactly the duration of that call.

## 1. The population, by two instruments

**The element-take census** (the scalar-escape census's `-elem` mode: every `&s[i]` whose `s` is a slice or array variable of scalar element type, classified by the address's consumer and aggregated per VARIABLE; the `noescape` walk-through included; production files, `CGO_ENABLED=0`, one run per `GOOS`):

| flavour | sites | variables | call-only | call-only AND every callee in the syscall family |
|:--|--:|--:|--:|:--|
| windows | 218 | 143 | 78 | **41** -- runtime 14, syscall 13, `internal/syscall/windows/registry` 5, `os` 5, `internal/poll` 2, `internal/syscall/windows` 1, `os/user` 1 |
| linux | 232 | 145 | 65 | **26** -- runtime 21, syscall 4, `internal/poll` 1 |
| darwin | 230 | 145 | 69 | **28** -- runtime 19, syscall 7, `internal/poll` 1, `net` 1 |

Exclusions on windows, each counted: `conv-other` 42 (the reinterpret family, `*(*T)(unsafe.Pointer(&b[i]))`), `return` 25, `assign/store` 18, `binary-expr` 11 (pointer arithmetic), `composite-lit` 6; 98 `unsafe.Pointer` conversions admitted on the way to a call. The largest call-arg callee packages beside the syscall family are `crypto/aes` (12) and `crypto/internal/bigmod` (12) -- pure-Go element takes into storage-only leaves, capability 3's population and not E's. **Runtime's share (14 / 21 / 19) is set aside exactly as in B's record**: its callees are runtime's own functions, read at the cut, with darwin's read as the libc `&args` shape that is C2's Q56 lift.

**The mksyscall buffer idiom by emission grep**, because the census's per-expression classifier sees `_p0 = unsafe.Pointer(&b[0])` as an `assign/store` and stops: `_pN = … Ꮡ(<s>, 0)` in the generated wrappers reads **windows 4, linux 20, darwin 14** -- the `_pN` local's only consumer is the funnel conversion, so it is E's population by construction of `mksyscall`, and the store the census excludes is this shape. The corpus-wide `(uintptr)Ꮡ(` direct idiom reads 21 (production, all flavours), a subset of the census's call-only rows.

So E's population is **the syscall-family element takes -- 41 / 26 / 28 direct plus 4 / 20 / 14 through the `_pN` local -- with runtime's share re-read at the cut**; the `os` row's segment 32 is windows' `writeFile`, one of the 4.

## 2. The mechanism -- the same `fixed`-scope emission as B, from the caller's side

B lowers a `*T` PARAMETER to `ref T` and pins it inside the wrapper. E is the element take the wrapper makes ITSELF: `&buf[0]` whose only consumer is the funnel conversion. The emission is a `fixed` scope over the slice's storage spanning the funnel call:

```
fixed (byte* ᴘ0 = len(buf) > 0 ? buf : default) {            // form (b): the pattern-based door, empty -> null
    var (r1, _, e1) = Syscall6(procWriteFile.Addr(), 5, (uintptr)handle, (uintptr)ᴘ0, (uintptr)len(buf), …);
    …
}
```

Two forms, both **measured on this box before this record** (a scratch probe, .NET 10, Release, a slice-shaped struct with a `ref`-returning indexer and a pattern-based `GetPinnableReference` that yields a null ref for an empty window): (a) `fixed (byte* p = &s[0])` over the `ref`-returning indexer -- golib's `slice<T>`/`array<T>` indexers ARE `ref T` today, so this form needs no golib change but cannot express the empty case without a branch; (b) `fixed (byte* p = s)` over the header itself through `GetPinnableReference()` -- a golib API ADDITION of one member on `slice<T>` and one on `array<T>` (`ref m_array[m_low]`, or `ref Unsafe.NullRef<T>()` for an empty window; a native-backed slice returns a ref over its native base, where `fixed` is a no-op pin) -- which encodes Go's `len > 0` branch in the door and takes element `i` by pointer arithmetic (`p + i`). Both forms' writes landed across a forced compacting collection; the empty window read address 0, which is what Go's `nil` buffer hands the kernel. **Form (b) is the recommendation**: one door, the empty case by construction, the converter emitting one shape for `&s[0]` and `&s[i]` alike.

Retention and boundary, per B's predicate: the element take is admitted when its EVERY consumer is the funnel conversion (directly, or through a local like `_pN` whose every use is the funnel conversion, or through an admissible pass-through callee); a take that is stored, returned, compared, arithmetic'd or handed to a non-admissible callee keeps its `ElemRefBox` and its pin -- the census's exclusions are that class. The `fixed` scope replaces BOTH segment 32 (no box) and segment 35 (no `GCHandle`, no holder, no finalizer: the JIT's pin table for the call's duration), and `KeepAlive` is dropped for that argument (the scope is the liveness). Q49's keep-alive census guard stays green by construction -- a `fixed` scope is a stronger statement than `KeepAlive` -- and gains a sibling asserting a lowered wrapper's scope holds the buffer across a forced compacting collection while a native writer writes through it (**re-read at the landing**: the guard's shape follows Q49's landed arm).

## 3. Prediction, on record

- **The os row (Release, TC0, the same-tree A/B), AFTER B: 184.25 B / 2 obj → 64.25 B / 1 obj** -- segment 32 (64 B / 1) and segment 35 (56 B / 0) both retired; the one remaining object is segment 1's `ElemRefBox` (`unsafe.StringData(s)`'s take, candidate C's population). If E is cut BEFORE B: 376.25 / 4 → 256.25 / 3. Falsifiers: a count other than the predicted one; bytes reading 128.25 (after B) or 320.25 (before B) with the right count -- the box gone but the pin holder still minted, the `fixed` scope not reached, a partial to explain; any change to the control's +64 B.
- Hot loop unchanged; the alloc-assert banked rows unchanged; the corpus footprint (two-seeded three-target diff, by hunk) confined to the generated `zsyscall_*.cs` bodies that carry the `_pN` idiom plus the direct sites the census names, in the census's packages and nowhere else; no `GoPositionMap` line moves except where a body's line count changes.
- C then takes the last object: the `unsafe.Slice(unsafe.StringData(s), len(s))` idiom (population two in std) as a string byte-window view, which brings the row to **0.25 B / 0 obj** -- the bank condition -- and is deferred last as ruled.

## 4. Gates (if ruled) and order

The converter suite with the predicate's guard (a `_pN` wrapper lowered; a stored or compared take refused; the empty-window case); golib's two `GetPinnableReference` members with a GolibTests arm (the probe's three cases as assertions) and therefore the golib gate list (`go2cs.slnx` once, GolibTests both configurations count-matched, the FULL behavioral suite as the cross-assembly consumer); the two-seeded three-target diff applied by hunk; the syscall closure on three flavours; the alloc-assert sweeps and the banked syscall-family rows; the os-row A/B on this box against the table above; C2's keep-alive census guard at the union. Order: after B (the wrapper's `ref` parameters and its `fixed` scopes are one emission change, and B's scope is the same statement), both priced against Q49 AS LANDED; then C.
