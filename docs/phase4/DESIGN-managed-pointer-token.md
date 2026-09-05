# DESIGN — the managed pointer TOKEN for reference-bearing boxes (Q44)

**STATUS: DESIGN, for ruling (C2, 2026-09-04). Cut after SUB-Q42's witness lands (train 26).**
Coordinator-minted from SUB-Q42's measured mechanism (mailbox `5984d7dbc`), dispatched at
`23aa5ffd9`. Parent: [`DESIGN-pointer-provenance.md`](DESIGN-pointer-provenance.md) (RATIFIED —
the `ManagedPointerTokens` record and its validate-on-read); sibling: `DESIGN-darwin-run-layer-2.md`
§7.2/§7.6 (the reference-bearing bound this design closes from the other direction). Every claim
below marked *measured* was read from the corpus at `8f82b3f63`; every prediction is on record here
before any code exists.

---

## 0. The defect in one paragraph, measured

A `ж<T>` whose `T` carries a managed reference gets **no pinnable slot**: `StandardBox` stores it in
`m_val`, a field of the box object, and allocates the one-element `m_slot` only for an unmanaged `T`
(`ж.StandardBox.cs:54–68`). `PinnableStorage` is therefore null, `EnsureStableAddress` pins nothing
(`ж.cs:444–451`), and the `ж<T> → uintptr` / `void*` operators fall through to
`fixed (void* ptr = &value.Value)` — the address of a field **inside a movable heap object** —
register it as a pin (`RegisterPinned`, `ж.cs:668`) and hand it out. Validate-on-read then refuses it
by design (`IsPinnedAt` is false the moment `m_pin` is null, `ж.cs:460–465`), so the reverse
conversion `(ж<T>)(uintptr)` misses in `Resolve` and mints a `NativeBox<T>` over a number the
collector was never asked to hold still. SUB-Q42 made it deterministic (10 of 10 under
`GO2CS_PIN_STALENESS_STRICT=1`, both configurations); SUB-Q27 measured the consequence — one of 91
`labelMap` pointers reading `len == 1885431144` after two collections, an `OutOfMemoryException`,
and the label half of `runtime/pprof`'s goroutine profile withheld.

**The information is never actually lost.** The box exists and is reachable from whoever holds it;
only the projection to a scalar discards the association. That is the exact premise of the ratified
`ManagedPointerTokens` record, which already carries it out of band for reflect's projections — and
the mechanism this design proposes is that record applied one class over.

## 1. Mechanism — what changes, and the larger part that does not

### 1.1 The mint (changes)

In the three address-take paths — `implicit operator uintptr(ж<T>)`, `implicit operator void*(ж<T>)`
(`ж.cs:624–705`) and, through them, `unsafe.Pointer.FromPinnedBox` (`unsafe.cs:399`) — insert one
arm **between** the fixed-array arm and the `EnsureStableAddress`/`fixed` arm:

```
if (value.PinnableStorage is null)          // reference-bearing T: nothing pins, nothing to fix
{
    nuint token = value.PointerOrderToken;  // AllocationBase(identityHash): stable for the box's life
    ManagedPointerTokens.Register(token, value);
    return (uintptr)token;
}
```

`FromPinnedBox` keeps retaining the box in `m_retainedSource` exactly as today (`unsafe.cs:404`), so
a `Pointer` that is never bridged to a bare number still recovers its referent without the registry;
the registry is for the number that leaves through the `(uintptr)` bridge (`unsafe.cs:337`), which is
the SUB-Q27 shape.

*Why the order token and not a fresh handle:* `MintOpaque<T>` (`ж.PointerTokens.cs:233`) already
mints exactly this — `box.PointerOrderToken`, registered, resolved by `CurrentToken(box) == token`
(`:327`) — for the opaque-box path, and it is the same value reflect's projection reports
(`reflect/value_impl.cs:1184`, `%p` and pointer-keyed map order). Keeping the one value per box keeps
Go's `uintptr(unsafe.Pointer(p)) == reflect.ValueOf(p).Pointer()` identity true, which a fresh handle
would break.

### 1.2 The recovery (does NOT change)

`explicit operator ж<T>(uintptr)` already consults the registry first and mints a `NativeBox` only on
a miss (`ж.cs:612–622`), and `Resolve` already validates a projection entry by **order token** before
falling back to the pinned-address check (`ж.PointerTokens.cs:327–330`). A token minted by §1.1
therefore resolves to its own box through code that exists and is banked. The same is true of every
other `Resolve` caller — *measured*, the complete set outside the token file: `unsafe.cs:418/451/839`
(referent recovery, store-through, and `unsafe.Slice`'s arm selection), `runtime/darwin/libccall_impl.cs:66`
(the darwin keystone's args-box recovery), `internal/syscall/windows/…/syscall_windows_impl.cs:250/353`,
`syscall/windows/zsyscall_windows_certchain_impl.cs:457`, `runtime/managed_impl.cs:1738`. All are
Resolve-first; none needs a change. **The cut is the mint arm and nothing on the read side.**

### 1.3 What is untouched

`ElemRefBox` and `FieldRefBox` have pinnable storage (the canonical backing / the container) and
never take the new arm. Unmanaged-`T` standard boxes keep the pin path byte for byte. The box
constructor is untouched — no field, no allocation, no per-box byte (the +8 B instance-state rule
is not triggered).

## 2. Encoding, and its proof — stated as a bound, not oversold

The token is `AllocationBase(RuntimeHelpers.GetHashCode(box))` = the 32-bit identity hash **shifted
into the high word, low 32 bits zero** (`ж.cs:418–422`, `ж.StandardBox.cs:137`). Nil is 0.

**What that gives:** a token is a 4 GiB-aligned 64-bit value. A native address a call could return —
a malloc/libSystem/CLR-heap object, a stack slot, a kernel-filled buffer — is never 4 GiB-aligned for
anything struct-sized, so the two spaces are disjoint **in practice**. **What it does not give:** a
proof. An `mmap`'d region base can be 4 GiB-aligned, so the window is not zero. The ratified record
already carries exactly this window for reflect's projections and names its backstop: a collision
resolves to a box only if that box is **alive and its current token equals the number**, in which
case the consumer gets a managed access in place of a wild one — *fails safe relative to what it
replaces* (`ж.PointerTokens.cs` header, COLLISION). This design adds no new window; it widens the
population that lives inside the existing one.

**The provable alternative, and why it is the fallback rather than the choice:** tagging bit 63
(`0x8000_0000_0000_0000 | token`) is disjoint from every canonical user-space address on x64 and
arm64 by construction. It costs the identity in §1.1 — the address-take value would differ from
`%p`'s — which is an observable Go program behaviour. Chosen: the order token, with the window
stated. If a collision is ever *measured*, bit 63 is the remedy and this section is where it is
priced.

### 2.1 Addendum (2026-09-05, as built and rebased onto train 27): the token FORMS per box kind

The paragraph above states the token for a `StandardBox`; the cut hands out `value.PointerOrderToken`
for EVERY box kind that reaches the address-take arm, and the forms differ by kind — read from golib at
the cut, cited so the §2 window argument is checked against each:

| box kind | `PointerOrderToken` | source |
|---|---|---|
| `StandardBox<T>` | `AllocationBase(RuntimeHelpers.GetHashCode(box))` — the identity hash in the high word, low 32 bits zero | `ж.StandardBox.cs:137` |
| `ElemRefBox<T>` (`&s[i]`) | `AllocationBase(hash(canonical backing)) + absolute element index` — the backing's identity in the high word, the ABSOLUTE index in the low word, so same-storage element pointers order by index exactly like Go addresses | `ж.ElemRefBox.cs:119` |
| `FieldRefBox<T>` (`&x.f`, `of()` chains) | `AllocationBase(SourceIdentityHash(source)) + GoFieldDisplacement(source, field)` — the source identity resolved through the `of()` chain (SameSource), the displacement that of the field within its IMMEDIATE parent, so nested chains compose | `ж.FieldRefBox.cs:64` |
| `SliceHeaderBox<T>` | its source's token | `ж.SliceHeaderBox.cs:195` |
| `NativeBox<T>` | the native address it aliases; nil 0 | `ж.NativeBox.cs:85` |
| a channel | `hash(core)`, nil 0 | `channel.cs:1207` |

**What this does to the window.** Only the `StandardBox` form is 4 GiB-aligned. An element or field token
carries its index or displacement in the LOW word, so it is a 4 GiB-aligned base PLUS a small offset — the
same 4 GiB window as the base, one object's worth of offsets inside it. The disjointness argument of §2
therefore reads: a native address collides with a token only if it falls inside a live box's 4 GiB window
at exactly that box's offset for that field or element — the same window, the same backstop (the box must
be alive and its CURRENT token equal the number), no new window. The bit-63 fallback prices identically
for every form.

**The measured member this addendum exists for, and its measurement.** `runtime/pprof`'s label round
trip is the `StandardBox` form: `SetGoroutineLabels` takes `unsafe.Pointer(&labels)` on a heap-escaped
`labelMap` — a struct wrapping a map, so a reference-bearing box with no pinnable slot — and the profile
reads it back as `(*labelMap)(p.labels[i])`. Before the cut `(uintptr)box` was the `fixed` address of the
box's own value field, a movable heap object, and `(ж<T>)(uintptr)` handed back a `NativeBox` over that raw
address; after a collection the round trip read a `labelMap` length of 1885431144 (SUB-Q27's witness,
2026-09-04), which is why the goroutine profile WITHHELD labels and why `TestGoroutineProfileLabelRace`
— whose `/reset` loop waits for a label to appear in the profile text — sat in
`runtime/pprof/go2cs_test_disclosures.json` as the host-fatal class's first HANG (Q43). With this token the
number is the box's registered order token and resolves to the box for as long as the goroutine's slot
holds the pointer, so the second commit of this series fills `labels[i]` from the registry's slot
(`pprof_impl.cs`, under Go's own length guard) and re-measures the row at that union, gated
`^(TestGoroutineCounts|TestGoroutineProfileLabelRace)$`, Release, tiering off, oracle go1.23.12 on linux:
`TestGoroutineProfileLabelRace` PASS in 58 ms (`/reset` 42 ms, `/churn` 6 ms) where it consumed the 182 s
deadline the day before, and `TestGoroutineCounts` PASS in 10.7 s with its label half reached — 4/4
matching. The disclosure entry is retired by that measurement (the file's last note keeps the record),
not by the reasoning.

## 3. Lifetime and release — weak, and what "weak" means for the consumer

`Register` stores a `WeakReference` and sweeps dead entries as the table grows
(`ж.PointerTokens.cs:126–148, 346–373`); the record's LIFETIME rule is that the table must never be
the reason a box stays alive. This design keeps that rule: **the token resolves for exactly as long as
something else holds the box.** For SUB-Q27's consumer that is the goroutine registry, which keeps
the `labelMap` reachable through the `Pointer`'s retained source for the goroutine's life — so the
number stored at `runtime_setProfLabel` resolves at profile time, which is the whole fix.

A token whose box has died resolves to nothing and the recovery mints a `NativeBox` over the number —
today's behaviour. That is not a hole this design leaves open; it is Go's own rule: a `uintptr` kept
past its pointer's life is not a pointer, and dereferencing it is undefined. What changes is that the
window is now "the box died" rather than "the box moved", and the box dies only when **nothing**
holds it, which is exactly when no correct program could dereference it.

Release is therefore the existing sweep — no explicit release API, no finalizer, no new entry point.
Cost per address-taken reference-bearing box: one dictionary slot plus one `WeakReference`, the same
~163 B per DISTINCT entry gate #1 measured for pins, ~0 per repeat (the steady-state early return).

## 4. Population and cost — cited, and one number stated as unmeasured

**The address-take population is small, measured.** The Q30 ratio census (mailbox `83a55415a`,
doctrine 475) counted address-takes of slot-allocating boxes at **21 of 2,361 on `syscall`, 379 of
581,139 on `os`, 1 of 110 on `sort`** — under 1% on the most address-take-heavy row, one in 1,500 on
`os`. Reference-bearing boxes have no slot and were not in that census's A; the same instrument
(per-kind attribution at the operator site) extends to count them, and that run is in flight as this
is written. **Until it reads, the design cites the slot-box ratio as the shape of the population and
states the reference-bearing count as UNMEASURED** — the mechanism does not depend on it beyond "an
entry per distinct address-taken box, weakly held".

**The static population, measured at `8f82b3f63`:** 706 `FromPinnedBox` mint sites corpus-wide
(runtime 260, runtime/{darwin,linux,windows} 232, syscall/{darwin,linux,windows} 153, iter 22,
reflect 7, the rest ≤ 7 each) and 702 `(ж<T>)(uintptr)` recovery sites over 120 distinct pointee
types, 350 of them in `runtime` proper (type-descriptor and heap-metadata walking — `bmap`, `_type`,
`mspan`, `arenaHint`, `notInHeap` — the paths the managed model never runs). The two live consumers
are named: SUB-Q27's `labelMap` (2 sites) and the darwin keystone's three reference-bearing args
structs (`mmap_args`, `mach_vm_region_args`, `proc_regionfilename_args` — `sys_darwin.cs:373/776/799`,
consumed only by `libcCall`, whose hand-own resolves the box: §7.6's "resolves to nothing" bound
becomes a hit, the second consumer COORD named).

## 5. The falsifier — censused, and it is NOT empty

COORD's falsifier: *a reader that hands the number to NATIVE code expecting a real address; census
those sites for reference-bearing T — there should be none.* **There are twenty.** Every
`FromPinnedBox(Ꮡx)` in the syscall family was resolved to its pointee type from the enclosing
declaration and classified against a brace-bounded field index (a `-A12` grep bled across struct
boundaries and over-reported; the bounded index is the number): **61 sites, 40 reference-free, 20
reference-bearing, 1 unclassified** (`internal/poll/windows/fd_windows.cs:1165`, `FILE_BASIC_INFO`,
four `int64` and a `uint32` by its Go definition — reference-free, unconfirmed by the index only
because its declaration sits in a file the index did not walk).

| shape | why reference-bearing | sites |
|:--|:--|:--|
| `BpfProgram` | `ж<BpfInsn> Insns` + `array<byte>` pad | `syscall/darwin/bpf_bsd.cs:155` |
| `SockFprog` | `ж<SockFilter> Filter` + pad | `syscall/linux/lsf_linux.cs:86` |
| `Iovec` | `ж<byte> Base` | `syscall/linux/syscall_linux.cs:787, 796` (ptrace) |
| `IPMreq`, `IPv6Mreq`, `ICMPv6Filter`, `IPv6MTUInfo` | fixed-array fields (`array<byte>` / `array<uint32>` / a `RawSockaddrInet6`) | 8 sites, `syscall/{darwin,linux}` get/setsockopt |
| `Timeval`, `Flock_t`, `ivalue`, `machVMRegionBasicInfoData` | a `Pad_cgo_0 [N]byte` → `array<byte>` | 8 sites, `bpf_bsd.cs`, `flock_bsd.cs`, `syscall_unix.cs`, `pprof/darwin/vminfo_darwin.cs` |

**Disposition, and it is the finding that matters:** every one of these twenty is **already wrong
today**, by the mechanism the doctrine names as the struct-passing root — the CLR gives AUTO layout
to any struct holding a reference and REORDERS it, so the kernel reads the wrong field
(`Msghdr.Namelen` at managed offset 40 where the kernel reads 8). These sites hand the kernel the
address of `m_val` inside a managed object; a `Pad_cgo_0` is enough to put a struct in that class.
Under this design they hand the kernel a **token** — a 4 GiB-aligned non-address — and the kernel
answers **EFAULT** (or the syscall's own EINVAL) instead of reading reordered, moving memory. A silent
wrong becomes a loud errno. **That is an improvement in failure mode, not a fix, and the design must
not claim it as one:** the remedy for all twenty is the explicit-layout native mirror the
struct-passing ruling already names, which is a separate arc with its own population — this table.

The raw-syscall keystone passes the number straight through (the resolve-based tether was retired
2026-08-30 at `internal/runtime/syscall/linux/syscall_linux_impl.cs:104–116`; `libc_syscall` receives
`ToNative(a1..a6)` with no lookup), so there is no seam at which a token could be refused *before* the
kernel sees it without re-introducing a per-call resolve the measurement rejected (68% miss). Chosen:
let the errno be the signal, and hand the mirror arc its twenty by name.

## 6. Coupling — the guards that move, with one correction to the queue text

| guard | today | after the cut |
|:--|:--|:--|
| SUB-Q42 `PinnedBoxStalenessWitnessTests` arms 3, 4 | INCONCLUSIVE ungated / RED under `GO2CS_PIN_STALENESS_STRICT=1` | **PASS, ungated**; the gate variable is deleted — *the acceptance*, 10 of 10 across both configurations |
| SUB-Q42 arm 1 (the six-step bisect) | passes, asserting `Resolve` null and the recovery `IsNative` | steps 5–6 **flip**: `Resolve` returns the box, the recovery is the box — updated in the same cut, stated as a changed prediction |
| `DarwinKeystoneArgsRecoveryTests` arm 4 | PASS on the null branch | takes the **other** branch it already carries — `Assert.AreSame` then `Inconclusive("…the bound is narrower than stated")` — so it does not go red; the cut turns that Inconclusive into a PASS assertion, recording the mechanism |
| `NativeAddressStabilityTests.ReferenceBearingPointeeIsLeftAlone` | asserts `PinnableStorage` is null and the value round-trips | **does not flip** — both assertions stay true (the fix adds no storage; it mints a token). The queue text lists it among the guards that go red with the fix; *predicted otherwise here*, and the cut's run of it is the measurement |
| SUB-Q27 `pprof_impl.cs` | labels withheld | the one line re-enters (`labels[i]` from `entry.Labels`); `TestGoroutineCounts` predicted PASS |
| `AliasOverlapRaceTests`, `PinLifetimeAtTheNativeBoundaryTests` | pin-class guards | **unchanged** — they measure boxes that DO pin; the new arm is taken only when nothing pins |

## 7. Gates, as ruled, and one added

GolibTests both configurations, count-matched; `go2cs.slnx` Debug `--no-incremental` (a golib API
surface change — no signature changes, but the rule is by file, not by signature); the full
behavioral suite (route #7's twin for golib); the nistec cost canary (the constructor is untouched,
so the prediction is *within noise* — measured, not assumed); CNR **run to confirm zero emission
change** rather than skipped on the argument that golib emits nothing (an argument is not a
measurement). Added: the twenty falsifier sites are checked for a banked roster row that exercises
one — if any row's verdict moves from a silent pass to an errno, that is the §5 disposition
*measured*, and it is reported as such rather than as a regression.

## 8. Predictions on record

1. SUB-Q42's witness: **10 of 10 green** with the gate variable set, Debug and Release+TC0.
2. `ReferenceBearingPointeeIsLeftAlone`: **unchanged, still passing** (a correction to the queue
   text's coupling; if it goes red, the mechanism did something this design did not intend).
3. Keystone arm 4: **resolves to its own box** — `AreSame` holds; the Inconclusive becomes a PASS.
4. `TestGoroutineCounts` with the label line re-entered: **PASS**.
5. nistec: **within run-to-run noise** — the constructor and the pin path are untouched.
6. CNR: **byte-identical** — no emission changes.
7. Of the twenty falsifier sites, the ones a banked row reaches (`syscall`'s socket-option tests are
   the likeliest) **move from a silent wrong to an errno**, named in the cut's report.
8. The reference-bearing address-take count, when the instrument reads: **under 1,000 per row on
   every row measured**, dominated by `syscall`/`os`.

**Falsifiers that would send this back to design:** (a) prediction 2 failing — the arm taken by a
box with pinnable storage; (b) any `Resolve` caller in §1.2 needing a change — the read side is
supposed to be done; (c) a measured collision (§2) — bit 63 becomes the encoding; (d) nistec moving
outside noise — something touched the constructor after all.

-- C2
