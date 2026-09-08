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

## 9. AMENDMENT 2026-09-05 — the consequence is PLATFORM-ASYMMETRIC, and prediction 7 came true larger than it was written

Ordered by COORD (`89e3ba68c`) after train 30's union battery. Appended, not rewritten: §5 and §8
stand as they were written and are wrong only in scope, which is the point of recording this.

### 9.1 What §5 says, and what was measured

§5 states the consequence as: *the kernel answers **EFAULT** (or the syscall's own EINVAL) instead of
reading reordered, moving memory. A silent wrong becomes a loud errno.* That is a POSIX reading of a
kernel **read**, and it is the only shape the section imagined. Measured on Windows, the same class
has **two** consequences and neither is an errno:

| the kernel's access | POSIX (as §5 stated) | Windows (measured 2026-09-05) |
|:--|:--|:--|
| **reads** through the pointer | `EFAULT` / the call's own `EINVAL` | the call **returns an empty result** — no errno, no fault; the caller reads zeros and carries on |
| **writes** through the pointer | `EFAULT` | the process **FAULTS**: access violation, `0xC0000005`, surfacing as `exit code mismatch: C# -1073741819 vs Go 0` |

The write case is what train 30 hit. `internal/syscall/windows.rtlGetVersion` hands ntdll's
`RtlGetVersion` the token; ntdll writes the version block through it and the process dies. The read
case is the quieter half and it is the one that was already happening at master, before this cut:
ntdll rejected a mis-laid-out managed address, `version()` read zeros, and the socket option above it
was silently skipped — every row on that path stayed green **on a wrong answer**.

### 9.2 The bill, measured on one host, one day, one instrument

Train 29's sweep at the landed master `b91684991` against the same rows at train 30's assembly head
`75758cf06` — eighteen other rows, several heavy, passed on **both** sides, so this is neither the
host nor the instrument:

| row | at `b91684991` | at `75758cf06` | shape |
|:--|:--|:--|:--|
| `net/http` | 1345 | conversion-blocked, **zero** converted verdicts | process death |
| `crypto/tls` | 400 (host-limited, not the roster figure) | 17 verdicts, then nothing — dies at `TestAlertFlushing`, its first real connection | contiguous alphabetical tail, no results file |
| `encoding/json` | 491 | 89, then nothing — dies at `TestHTTPDecoding`, its first test that stands up an HTTP server and dials it | contiguous alphabetical tail, no results file |
| `crypto/x509` | 341 | 341 of 341, **one** divergence (`TestHybridPool`) | did NOT crash; a different symptom, not attributed here |

**`crypto/tls`'s 400 is a different KIND of number from the other three and does not belong beside
them unlabelled** — the roster banks **3643**. The sweep names the kind in its own output rather
than leaving it to be re-derived: `PASS crypto/tls 400 = 3643 banked - 3243 (TestBogoSuite
host-limit disclosed; capability PRESENT, converted side over the deadline)`, reported as
`sweep: 1 pass (1 host-limited) / 0 fail`. So 400 is the host-limited count, 3643 the banked one,
and they differ by exactly the 3243 BoGo case rows a committed host-limit disclosure withdraws;
the row PASSED. `net/http` 1345, `encoding/json` 491 and `crypto/x509` 341 are roster-exact.

Neither truncated row wrote a results file, so nothing was killed by a deadline — the process died.
The path is `syscall.Syscall` → `rtlGetVersion` → `version()` → the
`SupportTCPInitialRTONoSYNRetransmissions` once → `net.connect`: **every Windows TCP dial**.

### 9.3 Prediction 7, scored honestly

§8's prediction 7 reads: *of the twenty falsifier sites, the ones a banked row reaches (`syscall`'s
socket-option tests are the likeliest) move from a silent wrong to an errno, named in the cut's
report.* The direction was right and the three specifics were wrong. It was not one of the twenty; it
was not `syscall`'s socket-option tests; and on Windows it is not an errno. **A prediction can be
correct in mechanism and wrong in every particular, and saying which half held is the point of
having written it down.**

### 9.4 Why §5's census could not see it — a scope gap, named

§5's twenty were found by resolving every **`FromPinnedBox(Ꮡx)`** in the syscall family. The measured
root does not use that mint. `zsyscall_windows.cs` emits:

```
internal static void rtlGetVersion(ж<_OSVERSIONINFOW> Ꮡinfo) {
    var ᴋ47 = Ꮡinfo;
        syscall.Syscall(procRtlGetVersion.Addr(), 1, (uintptr)ᴋ47, 0, 0);
```

— a **plain `(uintptr)` conversion of a box**, a second door into the same class that a
`FromPinnedBox`-keyed census is structurally blind to. So §5's twenty is a *lower bound on one mint
shape*, never the population; the record should not be read as if it were.

`_OSVERSIONINFOW` is five `uint32` then an inline `[128]uint16`, converted as an `array<uint16>` — a
managed reference — and the converter marks it as such itself: `package_info.cs` carries
**`[GoValueClone("csdVersion")]`** on the struct. That attribute is an independent derivation of the
same property, and it is the corpus-wide census handle COORD's `92a17d625` names. **The class
population is G's census, not restated here**, so this amendment does not enumerate it.

### 9.5 Disposition — unchanged, and this is the finding

Q44 stands and is not on trial. §5 already said the twenty are *already wrong today* and that the
token is **an improvement in failure mode, not a fix**; the four rows are the bill for a class that
was always broken, presented for the first time in a form that cannot be ignored. What this
amendment changes is the record's honesty about *how* the class announces itself: not one errno, but
a fault on write and a silent empty on read, with the second indistinguishable from working code
until something above it depends on the answer. The remedy is unchanged — the explicit-layout native
mirror the struct-passing ruling names, already proven by the timezone hand-own — and
`rtlGetVersion` is being cut assembly-side by COORD, not by this lane.

-- C2

## 10. AMENDMENT 2026-09-07 — the NARROWING: two measurements that point opposite ways, and the four-arm candidate that serves both

Drafted 2026-09-06 after COORD took the train-30 drop (`44b42314cb`) and ruled that the token seat
and its repairs re-enter as an increment with their own gates, under `a5f9b959ea`: *the token cut
must not change the behaviour of a path that was already correct; mint a token only where there is
no honest address, keep the master mechanism where the pin works.* Appended, not rewritten — §1
through §9 stand as written. **Read at master `67df171d7f`. Nothing is cut by this section, nothing
is committed, and the census §10.5 asks for has NOT been run.**

### 10.1 The two measurements, both real, pointing opposite ways

**The write needs the address.** Go's own `setField` — `*(*V)(unsafe.Add(unsafe.Pointer(&in),
offset)) = value`, `reflect/all_test.go:1399-1400`, reached from `TestIsZero` — over eight field
kinds, one process per kind:

| tree | Windows (R, Release/TC0) | Linux (C2, Debug) |
|---|---|---|
| Go 1.23.12 | 8/8 wrote correctly | 8/8 `f0correct` and `f1zero` |
| master `b916849915` | 8/8 wrote correctly | 8/8 `f0correct` and `f1zero` |
| the token seat | seven produce NO LINE (the write itself dies) | seven CAUGHT-PANIC, no readback |
| seat + `StorageKind: None → Unpinnable` | — | 8/8 `f0correct` and `f1zero` |

**Not one of the surviving writes lands on the wrong field. The token seat does not make a latent
wrong answer loud; it breaks writes that were right.**

**The escape needs the token.** From the seat's own commit, SUB-Q42's witness, **5 of 5 RED**: a
`ж<T>` over a reference-bearing `T` handed out the address of its own `m_val` field — *an address
nothing held still*; `(ж<T>)(uintptr)` could not recover the box, because `Resolve` validates on
read and refuses an unpinned number; the consumer got a native box over a **stale copy**; and
`runtime/pprof`'s label round trip read a labelMap length of **1,885,431,144** through it and killed
the host.

**Both are measured. The operator sees the same call in both cases and cannot tell them apart from
its inputs. That is the whole increment.**

### 10.2 Why pinning cannot unify them — a CLR property, not a policy

The obvious reconciliation — hand out a real address and hold it still — is unavailable. A
`StandardBox<T>` keeps its value in the pinnable `m_slot` only when `T` is reference-free, and
`GC.AllocateArray(pinned: true)` and `GCHandle.Alloc(…, Pinned)` both **refuse a type carrying
references**. So for exactly the class in question there is no pinnable storage to be had.
"Unpinnable" is a fact about the runtime, not a choice this design made.

### 10.3 The candidate: keep the token OUTBOUND, answer the write INBOUND

Keep Q44's token as the outbound value — so every escape resolves and SUB-Q42 stays closed — and
make the inbound conversion decide by arm:

1. `Resolve(n)` yields a box of the **same** pointee type → return it. *(Today's arm; pprof's case.)*
2. `Resolve(n)` yields a box of a **different** pointee type, and `n` is that box's own **order
   token** (offset 0) → Go's `(*V)(unsafe.Pointer(&s))`, which at offset 0 names the first field and
   nothing else. Return a box **aliasing** that storage. *(The write's case.)*
3. `n` is inside a live token's block but is **not** the token (offset ≠ 0) → **a Go-layout byte
   offset into CLR-laid-out storage, which has no meaning.** Refuse by name, catchably.
4. Otherwise → a real address, unchanged.

**Arm 3 already exists and is already measured to fire only where it should. Arm 2 is the new work**,
and it is where this could fail: `ReinterpretAliasesStorage<T, TDst>`'s predicate deliberately
excludes the prefix pun (2 fields → 1 field fails its length test), so the alias would have to be
admitted on a **narrower, offset-0-only** rule — and admitting it wrongly is `Unsafe.As` over
mismatched GC layout, which is **memory corruption, not a wrong value**.

### 10.4 Falsifiers, on record before any code

- **(a)** a population where arm 2's alias is not expressible AND the write is correct at master —
  then refusing there is a regression and the candidate is incomplete;
- **(b)** any offset-0 site where `V` is NOT the type at the pointee's offset 0 — then the alias is
  not a prefix pun and admitting it is unsound;
- **(c)** `Reinterpret`'s own fall-back needing arm 2 — it is `reflect`'s hot prefix downcast
  (`abi.Type → structType` ×5, `→ arrayType` ×5, three more pairs ×2 each) and it must keep the
  route it has, which is why the withdrawn refusal carved it out explicitly.

### 10.5 The census this needs, and it is NOT a grep

The operators are reached through IMPLICIT conversions — every `unsafe.Pointer(&x)` in the corpus —
so **a call-site grep cannot find them and a stack walk cannot attribute them** (frames inline; the
tree's own rule is that attribution rides on a caller-supplied tag). The honest instrument is
**dynamic and at the REGISTRY**: count tokens minted against tokens later resolved, and per resolve
record whether the pointee type matched. That yields the three populations arms 1, 2 and 3 serve,
measured rather than argued, and it answers falsifier (a) directly.

### 10.6 Arm 3 IS the byte-offset reinterpretation class, and that has its own record now

`docs/phase4/DESIGN-byte-offset-reinterpretation.md` (lane C2, **unlanded** as this is written)
records the same defect from the READ side — `(*[N]T)(unsafe.Pointer(&x))` where `x` is not a `T`,
53 production sites at `fd09034f53` — and reaches arm 3's conclusion independently: **a byte offset
computed against C's layout does not name the same storage in a managed object, and for a
reference-holding struct no abstraction fixes it.** Its §5 states as a hypothesis that the write
direction's remedy space *"reaches only blittable targets and the rest is a documented refusal"*;
**arm 3 is that documented refusal, already designed here, with its blast radius on `reflect`
measured at ZERO on Linux** — seat `388 / 0 empty / 67 differing`, seat+refusal `388 / 0 / 67`,
differing sets identical name for name.

**The two records were written a day apart from opposite ends and agree.** Neither cites a
measurement the other made until now, which is why this cross-reference is here rather than left for
a reader to notice.

> **⚠ AMENDED 2026-09-08 (C2), because a later post of mine leaned on this section for a conclusion it
> does not support.** Mailbox `fac149d059` argued that the arm-2a remedy "is arm 3's EXISTING refusal
> extended to offset 0" and cited this section as agreement "from a third direction". That citation
> conflates two directions that this section is careful to keep apart. What the two records agree on is
> the **READ** side: dereferencing a byte offset computed against C's layout, where a refusal is right
> because there is no correct answer to give. Arm 2a is the **WRITE** side, and there the number is
> **constructed and named, never dereferenced** — so a refusal at construction breaks callers that do
> the legal thing. §10.9 measures that: 31 passing tests at master reach all eight 2a sites, and the
> sentence is withdrawn. This section's own conclusion is unaffected; only the extension of it to arm 2a
> was wrong.

### 10.7 What is in hand, and what is deliberately NOT committed

The one-line narrowing, its measurement, the refusal and its complete guard ledger — **green →
neutered RED naming exactly one assertion → restore byte-identical → green** — are all preserved.
**None of it is committed, and none of it should be until §10.5's census says which arm the corpus
actually needs.** The eight-field-kind table above is the argument for that order: the seat as
drafted breaks eight writes that master gets right, and no amount of design settles whether arm 2's
population is empty.

-- C2

---

## 10.8 THE §10.5 CENSUS IS RUN — and its finding is that arm 2 is 8, not 18

C2, 2026-09-08, on COORD's `b1fd949c2` §3. Instrument on `claude/c2-q44-registry-census` off master
`a2e3b51c1`. **Predictions were posted before it ran** (mailbox `fdfc59873d`) and are scored in §10.8.4.

### 10.8.1 Where it attaches, and why there is no alternative

§10.5 rules out the alternatives by construction — implicit conversions defeat a call-site grep,
inlined frames defeat a stack walk — so attribution rides on the **call site classifying its own
values**. That site is `ж.cs`'s `uintptr → ж<T>` operator, where all four arms are decided from values
the operator already holds:

```
Resolve(n) is ж<T>                        ARM 1   same pointee type
Resolve(n) non-null, NOT ж<T>             ARM 2   different pointee type
     ... n == box.PointerOrderToken       ARM 2a  offset 0 -- the order-token route
     ... n != box.PointerOrderToken       ARM 2b  resolved via the PINNED-PROVENANCE route
Resolve(n) null, IsTokenArithmetic(n)     ARM 3   inside a live block, not the token -- refuses today
Resolve(n) null, not token-arithmetic     ARM 4   a real address
```

**The 2a/2b split is not in §10.3 and it is the whole finding** — see §10.8.3.

⚠ **One thing read out of the code before measuring, which sharpens §10.3.** `IsTokenArithmetic`
masks the low 32 bits and requires `allocationBase != number`, so it is **FALSE when n IS the base.**
Arm 2 therefore reaches neither arm 1's alias nor arm 3's refusal: it falls through to
`new NativeBox<T>(n)`. §10.3 calls arm 2 "the new work"; the sharper statement is that the write case
is **already being answered today, silently, by the arm-4 fall-through.**

### 10.8.2 The controls, because a census's zero is worth nothing without them

Seven control arms in `GolibTests.Q44RegistryCensusControlTests`, each asserting its counter
**increased across the call** rather than that a total matches a guess — the only form that can tell a
wired counter from an unwired one. With the census off they report **Inconclusive, never green**.

| control | reading |
|:--|:--|
| all four arms + the mint driven deliberately | 7/7 fire with the census on; 7/7 Inconclusive with it off |
| **perturbation A/B** (census on vs off, same filter) | **48 / 683 / 5 / 736 IDENTICAL** — the instrument does not disturb the suite |
| exhaustiveness | `arms sum == conversions` printed into the artifact, holds in every run |
| count reconciliation | 746 declared at this tree − 3 (`RuntimeAddrRangesTests`) − 7 (the control class) = **736 = reported Total** |

⚠ **A defect this instrument caused, fixed, and then controlled for.** The first version dumped to
**stderr** from a `ProcessExit` hook. Every counter fired, all arms went green, and **zero census lines
reached any log** — the MSTest host swallows it. *A counter that moves into a channel nobody reads is
the same defect as a counter that never moves, and harder to see, because the arms all look healthy.*
It now reports to a **file**, a failure to write says `Q44CENSUS-UNREPORTED` rather than passing
silently, and there is a control arm asserting the file appears. A second, smaller one followed: the
reporting control originally deleted and rewrote the census's **own** output file, so running the
control inside a census run destroyed the census mid-flight — it writes to its own path now, and the
census run excludes the control class so **the instrument does not measure itself.**

### 10.8.3 ⚠ THE READING, and the finding is the split

`GolibTests` at master `a2e3b51c1`, control class excluded, **byte-identical at Debug and at
Release + `DOTNET_TieredCompilation=0`**:

```
mints = 4,378        conversions = 52        (mints exceed resolves by 84x)
arm1 = 9    arm2a = 8    arm2b = 10    arm3 = 1    arm4 = 24        sum = 52  RECONCILES
```

**Arm 2 totals 18 of 52 conversions — and only 8 of them are the defect.** The split is why:

- **ARM 2a — 8 conversions — IS the defect.** `n` IS the box's own order token, the pointee type
  differs, and the fall-through hands back `NativeBox<T>(token)`: **a native box over a number that is
  not an address.** This is exactly the population §10.3's arm 2 targets.
- **ARM 2b — 10 conversions — is SOUND and needs no remedy.** The resolve succeeded through the
  **pinned-provenance** route (`IsPinnedAt`), which means `n` **is a real pinned address**. The
  fall-through hands back `NativeBox<T>` over a real address, which is correct.

**So a remedy sized against "arm 2 = 18" would change behaviour for 10 conversions that are already
right.** §10.3's arm 2 is correctly aimed; its *size* is 8, and without the 2a/2b distinction the
census would have overstated it by 2.25×. Arm 2b is also a population §10.3 does not describe: a
cross-type resolve at a non-zero offset that never reaches `IsTokenArithmetic` because it resolved.

Requested/resolved type pairs (all resolving to `StandardBox<T>`): `Byte` ×4 (2a) and ×2 (2b);
`Pointer` ×5 (2b); `ThreeWords`, `StringHeaderShape`, `ж<T>`, `ReferenceBearingView` (2a each);
`array<T>`, `Int64`, `Pointer<T>` (2b each). **Falsifier (b) — "any offset-0 site where V is NOT the
type at the pointee's offset 0" — is answerable from that 2a list and is the next reading owed**, per
site rather than per count.

### 10.8.4 Predictions scored: 4 HIT, 2 MISSED

| prediction | outcome |
|:--|:--|
| mints > resolves by a wide margin | **HIT** — 84× |
| arm 1 > 0 | **HIT** — 9 |
| **arm 2 = 0 or single digits** | **MISS** — 18 (8 after the split; still double digits as worded) |
| arm 3 > 0, else the instrument is broken | **HIT** — 1; the instrument's own falsifier did not fire |
| **arm 4 ≫ all others** | **MISS** — 24 of 52 is 46 %, the largest but not dominant |
| identical at both configurations | **HIT** — byte-identical |

The two misses share a cause: I expected golib's own tests barely to reach the cross-type write case,
and they reach it in **a third of all conversions**. Predicted as a scope statement, measured as a
population.

### 10.8.5 ⚠ SCOPE — this is GolibTests, NOT the roster

The population that matters is the **corpus** — `reflect`, `pprof`, the reflect-heavy roster rows —
and this ran on a linux container where the windows corpus flavour does not execute those rows. **A
zero or a small number here is not a corpus reading**, which is the scoped-zero-across-a-scope-boundary
trap this tree has paid before. The corpus census is **owed to a Windows box**: the instrument is
env-gated and free when off, so it costs a roster sweep nothing but the variable.

**Falsifier (a)** — a population where arm 2's alias is not expressible AND the write is correct at
master — **is not answered by this run.** It needs the corpus population and the per-site reading of
§10.8.3's 2a list.

---

## 10.9 AMENDMENT 2026-09-08 — the instrument was NOT observation-only, falsifier (a) FIRES, and the 2a population is EMPTY where measurable

C2, on COORD's `7d44b472fd`. This is the honest write-up of state that ruling asked for, in place of a
remedy. Four things happened in one day and they compose into one conclusion: **§10.3's arm 2 is not a
piece of work waiting to be done; it may be a null, and the evidence now points that way from three
independent directions.**

### 10.9.1 ⚠ Every number in §10.8 is a FLOOR, because the instrument was not neutral

i9 (`f8213cf49`) measured the banked `os` row flipping **PASS → FAIL** with the census env gate as the
**only** variable, twice, in both directions, and named a sufficient mechanism in the instrument's own
code: the arm-4 classifier read `ManagedPointerTokens.Resolve(value) is null`, so **enabling the census
performed a SECOND `Resolve` per conversion** — and `Resolve` is not passive, `TryRemove`ing a dead weak
entry and reassigning the count. *"Env-gated, free when off" is TRUE and is NOT the property that
matters: the census is free when off, and it was not neutral when on.*

**Fixed** (`acbfa34503`): reaching that line already means `resolved` was not a `ж<T>` and the arithmetic
refusal did not fire, so `resolved is null` **is** arm 4 — the same verdict from a value already in hand.
Guarded by `TheCensusPerformsONEResolvePerConversion_TheNeutralityPROPERTY`, which reads
`Expected:<1>. Actual:<2>` on the line it replaces.

⚠ **The guard's FIRST form was wrong and is worth keeping written down.** "A conversion must not change
the registered count" **failed on the fixed code**, correctly: the ONE resolve the operator legitimately
performs evicts the dead entry whether the census is on or off. Nor can counting evictions see the
defect — two resolves of the same token cannot evict twice, which is also why the corpus symptom was a
timing effect rather than a countable double-eviction. What discriminates is **how many times `Resolve`
is ENTERED per conversion**, which is the property COORD ruled on.

### 10.9.2 ⚠ A SECOND defect, and it cut against the census's own purpose

Found by reading the classifier for the first defect, not looked for. The 2a/2b discriminator carried its
**own two-arm copy** of "the token this box reports today" (`INilPointer`, `IChannel`, else `0`) while
`ManagedPointerTokens.CurrentToken` has a **third arm** for anything else. A registered object
implementing neither interface therefore projected to `0`, compared unequal to its own token, and was
filed **2b — the SOUND bucket, the one §10.3 says must not move — when it is 2a, the defect bucket.**

**A census that files its own target under "nothing to do here" reports the population as ABSENT**, which
is the worst possible direction for an instrument whose entire finding is a zero. `CurrentToken` is
`internal` now and there is one definition of the rule. Measured rather than argued: with the copy
restored, a plain object registered at its own token is filed 2b and
`The2a2bDiscriminatorUsesTheRegistrysOwnProjection_NotACopyOfIt` says so by name.

The fix moves **nothing** on the GolibTests population — every box it registers is a `ж<T>` or a channel,
so the third arm is latent there — which is why §10.8.3's counts remain comparable to i9's corpus floors:
re-measured on the fixed instrument, control class excluded, the reading is **byte-identical**
(`mints 4378, conversions 52, arm1 9, arm2a 8, arm2b 10, arm3 1, arm4 24`, reconciling, Total 736).

### 10.9.3 ⚠ FALSIFIER (a) FIRES — 8 of 8, and the verdicts are census-OFF

§10.4's falsifier (a) is *"a population where arm 2's alias is not expressible AND the write is correct at
master — then refusing there is a regression and the candidate is incomplete."* §10.8.5 said this run
could not answer it. It is answered now, and **both halves hold for every measured site.**

The verdicts are taken with the census **OFF**, so §10.9.1 does not touch them; the instrument supplied
only the attribution of which sites a filter reaches, and the perturbation direction (an extra `Resolve`)
cannot manufacture a pass anyway.

| population | census OFF | census ON | 2a sites reached |
|:--|:--|:--|:--|
| the four classes owning the 2a types | **28 / 28 pass** | 28 / 28 pass | 7 |
| `PointerTokenConversionTests` | **3 / 3 pass** | 3 / 3 pass | 1 |

**31 passing tests, 0 failures, 0 aborts, 8 of 8 sites reached.** Two negative results banked so nobody
re-walks them: the 8th site's owner was guessed twice from the type name and both guesses were wrong —
`PointerNilPredicateTests` (22/22 pass) and `FinalizerDispatchTests` both read `arm2a=0`. It was found by
measurement.

### 10.9.4 ⚠ "Correct at master" understates it — the project ALREADY RULED this, three days earlier

These are not incidental passes. The 2a behaviour is **asserted by name**, in tests written for it, with
comments explaining the choice — and the decision was already made once, in the opposite direction from
the withdrawn sentence. `PointerTokenConversionTests`' own header records it: the Q44 chain found a
**behavioral row red** (`PointerCastSliceRange`, 2026-09-05) whose `**(**[2]int64)(unsafe.Pointer(&ip))`
reaches exactly this conversion, and the resolution was to **amend THE ROW, not the operator** — *"the
row's dereference was exactly such a pun, and the row is amended to the compile-shape guard it
documents."* Its arm is named
`ATokenOfAnotherPointeeTypeIsANativeBoxOverTheToken_TheLoudFormTheDesignChose`.

Two independent classes document the mechanism, which is the second derivation this would otherwise owe:

- **`ReinterpretSourceRetentionTests`** (the boundary idiom): a reference-bearing pointee has no pinnable
  storage, so the box hands out its **order token** rather than a movable field's address — and that
  premise is *itself* asserted, so a future change giving such a box pinnable storage fails THAT assertion
  first instead of quietly making the design redundant. Then `IsNative`, and the number **equal** to
  `source.PointerOrderToken`, *"never a heap address the collector was not asked to hold still"*; *"a
  native reader of the view faults at a non-canonical address instead of reading a stale copy, which is
  the LOUD FAILURE THE DESIGN CHOSE"*; *"a boundary wrapper never reads it, it recovers the record"* — via
  `ReinterpretSource`, with `Resolve` beside it as the second recovery and a comment requiring the two to
  agree.
- **`RuntimeHashFamilyTests`** (the string header), which states what the token **replaced**: before Q44
  this reinterpret took the ADDRESS route — a `NativeBox` over the **pinned managed string** whose `str`
  field read back the `byte[]` reference as a `Pointer`, measured 2026-09-04 as runtime type
  `System.Byte[]` with a field read through it a native SIGSEGV — *"which the seam refused by name. THAT
  ROUTE NO LONGER EXISTS TO BE REFUSED"*, because the box now hands out its token and the reinterpret is
  *"a native box OVER THE TOKEN (the design's loud form; its fields are not touched here — **a
  dereference is the row-level fault the design chose, never a number**)"*.

**So arm 2a is not an unremedied case. It is the case Q44 already fixed, and the token-over-a-non-address
IS the fix.** Refusing there does not add safety; it refuses to construct the safe object, and it breaks
the recovery too — `PointerTokenConversionTests` asserts `other.NativeAddress == the token` precisely *"so
a boundary wrapper resolving the number still recovers the source."*

### 10.9.5 Where the safety lives, and why a refusal at that site cannot be the remedy

Three mechanisms already sit at the **dereference**: a native read of a token **faults** (non-canonical by
construction); the hash seam **refuses a header by name** (`GoMemhashPointer` over the string box's number
panics `"string HEADER"` while `GoStrhashPointer` over the same number hashes the CONTENT, both asserted in
one arm); and the **token door** at the syscall boundary refuses a token as an argument. At 2a the number
is **constructed and named** and never dereferenced, which is why all three are silent there.

⚠ **That makes a refusal at the conversion site structurally unable to be the remedy, not merely
mis-sized.** The operator cannot know whether the number it hands back will be dereferenced — that
information arrives later, at the use — so a predicate placed at construction cannot discriminate the
defect from the passing uses, **whatever it tests**. Any remedy has to sit where the information is.

Adjacency checked rather than assumed: that same class's third arm round-trips a token through `void*` to
native code and back and requires it to come back **as its box**. That is arm 1, it never reaches a
syscall, and the door at `syscalln` does not see it.

### 10.9.6 The corpus floor: 4,143,157 conversions, not one arm-2 classification

i9's rows, on the **unfixed** instrument and therefore floors rather than the record (`72000a1f3a`,
`f8213cf49`, `54a15554d8`):

| row | conversions | arm 1 | arm 2a | arm 2b | arm 3 | note |
|:--|--:|--:|--:|--:|--:|:--|
| `go/types` | 303,492 | 668 | 0 | 0 | 0 | PASS 557; mints 668 == arm1 668, every mint resolved, every resolve correct |
| `runtime/pprof` | 3,839,386 | 1,283,101 | 0 | 0 | 0 | 50 tests, 122 pass / 23 fail; partial |
| `encoding/json` | 279 | 0 | 0 | 0 | 0 | PASS 491 |
| `os` | — | — | — | — | — | **VOID**, census-induced failure (i9's own word) |
| **total** | **4,143,157** | **1,283,769** | **0** | **0** | **0** | |

### 10.9.7 Predictions scored, and mine is REFUTED

§10.8 predicted arm 2a non-zero on **`runtime/pprof` first**, then `reflect`. Run correctly, `runtime/pprof`
reads **arm2a = 0 across 3,839,386 conversions** with 1,283,101 reaching the token path and resolving
correctly as arm 1. i9 declined to score it refuted on two fair caveats (a non-neutral instrument; a
partial row). **Those caveats are not taken here: a prediction that survives only on its measurement's
caveats is refuted, and mine is refuted on that row.** The perturbation ADDS resolves and so cannot have
removed an arm-2 classification, which is the direction that matters.

### 10.9.8 Disposition — the candidate is "NO CHANGE at 2a", with its falsifier named

§10.7 said none of the seat should be committed until the census said which arm the corpus needs. It has
now said, from three directions: the corpus floor is **zero**, the GolibTests 2a population is entirely
**construct-and-name** with 31 passing tests over it, and the design **already chose** the loud form there
deliberately. **§10.3's arm 2 is incomplete as written and mis-aimed rather than under-specified**: its
target is not "offset-0 cross-type resolves" (8 of 52) but the subset *"where the number is later READ AS
AN ADDRESS"*, and no measured site is in that subset. Third shrink of one target: **18 → 8** by the 2a/2b
split, **8 → no-alias-machinery** by expressibility, **8 → 0 measured** by read-vs-name.

**MUST-NOT-MOVE is two rows now, not one:** arm 2b (10 conversions, sound because *n* IS a real pinned
address) and **arm 2a construct-and-name** (8 measured, all 8 with deliberately-asserting passing tests).

The surviving candidate is **no change at 2a**, and it is stated with its falsifier rather than claimed:
**a site where the token is read as an address by MANAGED code that would silently produce a WRONG VALUE
rather than fault.** Native reads fault, the hash seam refuses by name, boundary wrappers recover — a
silent wrong value is the only shape "no change" cannot absorb.

### 10.9.9 What is OWED, and by whom

- **i9, on a qualified host**: the neutrality PROOF COORD ruled — banked `os` at **PASS 683** with the
  census ON beside the census-OFF control — then `encoding/json`, `go/types`, `runtime/pprof` re-taken on
  the fixed instrument, then `reflect`, `net/http`, `crypto/tls`.
- ⚠ **NOT C2's to run — and the FIRST version of this bullet was wrong in both of its stated reasons,
  which is worth more than the conclusion it happened to reach.** It said the lane host is disqualified
  because bare `go` reports 1.24.7 and "there is no PowerShell". Both were **PATH readings reported as
  HOST facts**: the pinned `go1.23.12` is installed and passes all three preflight arms (`env -u GOROOT
  <pinned>/bin/go env GOROOT` = the pinned root, `VERSION` = go1.23.12, the binary = go1.23.12), and
  pwsh 7.6.5 is installed under the dotnet global-tools directory. A probe answering "not found" describes
  the environment it ran in, not the machine — this tree's own written lesson, paid in the direction that
  takes work off the lane's plate, which is the direction to distrust first.
  **The REAL disqualifier is disk, it is structural, and the sweep's own preflight is what found it**:
  `run-validated-sweep.ps1:126` refuses below a **25 GB** floor and the host measured **10.1 GB** free.
  No cleanup reaches it — the writable allowance is roughly 9–11 GB — so this is a property of the host
  class, not of a full drive. `-IgnoreDiskPreflight` exists, and the script's own words for what it
  yields are **"unmeasurable results"**: below the floor, writes fail mid-run, builds report FALSE REDS
  and a partial write can truncate a tracked file (three such incidents, 2026-08-13). A manufactured red
  would land on the **census-ON** arm and read exactly like "the fix failed", which is the one outcome
  that must not be fabricated — so the flag is refused here rather than used. The conclusion is unchanged
  and every reason for it is different. The suite-scale reading below is evidence, not that proof: GolibTests at
  Release with tiering off, `RuntimeAddrRangesTests` excluded (it hangs at master), **census OFF Failed 48
  / Passed 686 / Skipped 11 / Total 745** and **census ON Failed 48 / Passed 695 / Skipped 2 / Total 745**,
  0 aborted either way — the failure counts IDENTICAL across the env gate, the +9 being the control class
  which is Inconclusive when the census is off.
- **The lesson about post structure**, banked because it cost a published claim: mailbox `fac149d059`
  stated what its finding *"CHANGES"* **and** named a live falsifier for that same change, three
  paragraphs apart, in one post. Those are in tension by construction — if the falsifier is live, the
  change is not yet known. The measurement in that post was sound and stands; the inference was published
  one step ahead of the reading its own author had identified as owed, and it proposed reversing a ruling
  already in the tree. **A post that names a live falsifier states the CANDIDATE and stops.**


---

### 10.9.10 ⚠ AMENDED 2026-09-08, LATER THE SAME DAY — the instrument is NEUTRAL, and §10.9.6's floor is WITHDRAWN

Two results from i9 (`5de94f336b`), and they point opposite ways.

**NEUTRALITY IS ACHIEVED, and it took both arms.** At `ad87e2bb1f`, the banked `os` row reads
**`PASS os 683`, rc=0, sweep 1 pass 0 fail, with the census ON — identical to OFF.** Same tree, same
configuration of record, one variable, both directions agreeing. COORD's ruled gate in `7d44b472fd` is
met. Neither named mechanism achieved it alone: the extra `Resolve` (i9's, refuted by measurement) nor
the classifier fix that removed it; what closed it was compiling the Resolve-entry counter **out** rather
than gating it, together with the stderr and child-inheritance work.

⚠ **AND §10.9.6's FLOOR IS WITHDRAWN — every number in that table was ONE SURVIVING BLOCK, not a row.**
The shared-census-file race this record's own `{pid}` finding described is worse than an undercount. With
`{pid}`, the `os` row writes **FIVE** files — a parent and four helper children:

| process | conversions | arm 1 |
|:--|--:|--:|
| parent | 260,132 | 20 |
| child | 94 | |
| child | 100 | |
| child | 96 | |
| child | **16** | |
| **row total** | **260,438** | **20** — arm2a 0, arm2b 0, arm3 0, arm4 260,418, reconciles |

i9's earlier `os` reading was **`conversions=16`**: the last row of that table, the smallest child, the one
that did almost nothing. **The race did not merely truncate the count, it preserved the LEAST
representative block** — and that block was tabled as the row.

So `encoding/json` at 279, `go/types` at 303,492, `runtime/pprof` at 3,839,386 and `os` at 16 were each a
single surviving block. **The claim "4,143,157 conversions and not one arm-2 classification anywhere" is
not a floor and is withdrawn**, because a destroyed block could have carried arm-2 hits and no run can now
say. i9 withdrew it in the same post; it is withdrawn here too, in the record that cited it.

**What survives is narrower, and only this**: on the `os` row measured properly, **all five blocks read
arm2a, arm2b and arm3 at ZERO across 260,438 conversions.**

⚠ **§10.9.7's "REFUTED" IS THEREFORE UNSCORED, NOT VINDICATED.** That subsection scored the prediction
"arm 2a non-zero on `runtime/pprof` first" as **refuted**, on pprof's 3,839,386-conversion reading — a
number now withdrawn. A refutation resting on void data is void. The prediction returns to **unscored**
until pprof is re-taken on the fixed instrument with `{pid}`. **This is not a walk-back**: the
properly-measured `os` row still reads 2a at zero, so the direction of the evidence has not changed and
"the 2a population may be a GolibTests artifact" remains the leading candidate. Only its *support* shrank,
from four million conversions to a quarter of a million on one row.

**The sharpest form of my own error, stated because it is the useful part.** The per-process TRUNCATE was
introduced to fix a real problem i9 named — two ROWS summing into one block. It fixed that and made the
other failure mode **worse**: appending would have PRESERVED all five blocks, leaving a reader with an
ambiguous file that could be read correctly once noticed; truncating **destroyed four of five** and left a
well-formed file containing the least informative one. A change that converts a recoverable ambiguity into
irrecoverable loss is a regression even when it fixes what it was aimed at, and the tell was available all
along: the instrument offered `{pid}` and nothing required it.

Attribution is shared and I am not arguing i9's generosity down. My instrument permitted a shared path and
truncated per process; i9's runner chose the shared path to stop rows summing and never asked whether one
row could be several PROCESSES. Both halves were needed. The durable fix is that a row's census can no
longer be one file by accident, and that the reader prints the file COUNT beside the blocks so a
five-process row cannot report as one.


---

### 10.9.11 ⚠ AMENDED AGAIN 2026-09-08 — THE ARM-2a POPULATION IS **NOT** EMPTY: 1,236 hits, all in `crypto/tls`

i9 (`a12e46447c`), on the neutral instrument at `ad87e2bb1f` with per-process files and per-row sums,
the gate re-proved at `os` 683 both ways at the head of each batch. **The first table whose numbers are
the rows:**

| row | files | conversions | mints | arm 1 | **arm 2a** | arm 4 |
|:--|--:|--:|--:|--:|--:|--:|
| `os` | 5 | 260,438 | 0 | 20 | 0 | 260,418 |
| `encoding/json` | 1 | 279 | 13 | 0 | 0 | 279 |
| `go/types` | 1 | 304,542 | 668 | 668 | 0 | 303,874 |
| `runtime/pprof` | 1 | 3,898,831 | 10,594 | 1,302,758 | 0 | 2,596,073 |
| `reflect` | 0 | **NO CENSUS OUTPUT** | | | | |
| `net/http` | 1 | 33,685 | 35 | 0 | 0 | 33,685 |
| **`crypto/tls`** | **2,241** | 913,859 | 1,239 | 1,850 | **1,236** | 910,773 |
| **total** | | **5,411,634** | | | **1,236** | |

**"The 2a population may be a GolibTests artifact" is DEAD.** §10.9.8 named that as the leading candidate
and it is now falsified by measurement: the population is real, it is 1,236, and it is concentrated
**entirely in one row**.

⚠ **WHY IT WAS INVISIBLE, AND IT IS NOT LUCK — it is this record's own `{pid}` defect, quantified.**
`crypto/tls` runs bogo at **2,241 processes**, of which **507 carry arm2a > 0 and 1,734 read zero**. Under
the shared-file race exactly ONE block survives, so drawing one at random gives about a **77 % chance of
reading arm2a = 0**. The old method would MOST LIKELY have reported `crypto/tls` at zero, and the corpus
would have been declared arm-2-free with four million conversions behind it. Three separate fixes had to
hold to see it: the instrument NEUTRAL (§10.9.1–2), pid SEPARATION (§10.9.10), and the row actually RUN.

### 10.9.12 The prediction, scored properly at last — one half HIT, one half REFUTED

§10.8 predicted **"arm 2a non-zero on `runtime/pprof` FIRST, then `reflect`."** §10.9.7 scored it refuted;
§10.9.10 returned it to unscored when its evidence was voided. It can now be scored, and it splits:

- **The EXISTENCE half — HIT.** Arm 2a is non-zero in the corpus: 1,236 sites.
- **The ROW half — REFUTED, and firmly.** `runtime/pprof` reads **arm2a = 0 across 3,898,831
  conversions**, which is about as strong a null as that row can give. `reflect` produced **no census
  output at all** and is recorded as unmeasured rather than as zero. The row that carries the population
  is `crypto/tls`, which the prediction did not name.

Scored as worded, that is a **miss**: naming the mechanism's existence while naming the wrong rows is not
a correct prediction, and the row half is what it was used for — deciding which rows to spend hours on.

### 10.9.13 ⚠ What 1,236 DOES and DOES NOT settle

**It does NOT resurrect §10.3's arm-2 refusal, and the reason is §10.9.3–5 unchanged.** Falsifier (a)
fired on 8 of 8 GolibTests sites with 31 passing census-OFF tests over them, and the discriminator there
was **read-vs-name**: at 2a the number is CONSTRUCTED AND NAMED, never dereferenced, which is why a
refusal at the conversion site cannot separate the defect from the legal uses. **A count cannot answer
that question.** 1,236 sites establish that the remedy has a population; they do not establish that any of
them dereferences.

**The next reading is per-site and it already exists.** The instrument records, for every arm-2 hit, the
requested type, the resolved pointee type, whether each carries managed references, and
`alias-expressible`. Those `Q44CENSUS-ARM2` lines are in `crypto/tls`'s 507 non-zero blocks now. Reading
them answers what the count cannot: which shapes, and whether the alias is expressible for any of them —
the same reading §10.8.3 did for the GolibTests eight.

**Prediction on record before those lines are read**, so it can be scored:

1. The 2a pairs will be **reference-bearing** on at least one side, so `alias-expressible=NO` for
   substantially all of them — that property is what forces the token route in the first place.
2. They will be **construct-and-name**, i.e. falsifier (a) holds on the corpus too. The argument is a
   failure-mode one rather than a preference: a token dereferenced as an address faults at a
   NON-CANONICAL address, loudly, and `crypto/tls`'s observed failure is `TestBogoSuite/Client`, an
   assertion, with i9 holding attribution pending a census-OFF control. A crash is what dereferencing
   would look like and it is not what the row shows.

If (2) is wrong — if any of the 1,236 is read as an address by managed code producing a silently wrong
VALUE rather than faulting — that is exactly the falsifier §10.9.8 named for "no change at 2a", and the
disposition changes.

**Two rows are NOT attributed and are recorded that way**: `net/http` (`TestRegisterErr`) and `crypto/tls`
(`TestBogoSuite/Client`) both read rc=1 with the census on, and i9 is running census-OFF controls rather
than guessing; the `os` gate proves neutrality on `os`, which is not the same as neutrality on a row that
spawns 2,241 processes or touches the network. And `reflect` is recorded as **NO OUTPUT**, not as zeros,
because the run happened and the census never reported — reporting zeros there would be the unrun census
wearing a result's clothes.

**i9's own over-correction, corrected**: `5de94f336b` called every earlier census number void, which was
right ex ante and is now narrowed by measurement — only `os` lost blocks (16 → 260,438, wrong by a factor
of sixteen thousand). `encoding/json` reads 279 both times; `go/types` 303,492 → 304,542 and
`runtime/pprof` 3,839,386 → 3,898,831 are run variance of 0.3 % and 1.5 %. Those three stand as
approximations.


---

### 10.9.14 ⚠ AMENDED A THIRD TIME — `crypto/tls` does NOT pass on this tree, and the neutrality claim is ROW-BOUNDED

Three corrections land together, none of them to the 1,236.

**COORD withdrew ruling 1's premise, and then i9 MEASURED it.** `82c60cec4` said the 1,236 sit "inside a
banked row at 3,643 verdicts PASS on that same tree"; COORD withdrew that in `5e9c3f8bc7` on the ground
that the only `crypto/tls` verdict held was **the ROSTER's** — a banked record from another day — not a
run on `ad87e2bb1f`. i9's control then settled it (`6e3e99e6a2`): **census ON reads FAIL at 394 s, census
OFF reads FAIL at 393 s.** The census is EXONERATED and the row does **not** reach PASS on this host and
tree. ⚠ But the two arms name **different subtests** (`TestBogoSuite/Client` vs
`.../CertificateSelection-Server-PreferenceOrder-TLS-TLS11`), so this is not one stable defect reported
twice — it is a row whose failing member **moves between runs**, and whether that is corpus drift or bogo
nondeterminism is a third question nobody has answered.

**So ruling 1 keeps its conclusion and loses that reason.** As COORD restated it, the 2a remedy stays
withdrawn on §10.9.3–5 alone: falsifier (a) fired on 8 of 8 GolibTests sites with **31 passing CENSUS-OFF
tests** over them, and the discriminator is READ-versus-NAME — at 2a the number is constructed and NAMED,
never dereferenced, so a predicate at the conversion site cannot separate a defect from a legal use
whatever it tests. **No `crypto/tls` verdict, either way, bears on that.** The count of 1,236 establishes
the remedy has a corpus population; it does not establish that one member is a defect.

⚠ **AND §10.9.10's "NEUTRALITY IS ACHIEVED" IS BOUNDED — i9 bounded their own gate and the bound belongs
here too.** The `os` gate is green both ways, but **`os` is UNANNOTATED**, so both its arms ran at
`DOTNET_TieredCompilation=0`, which is `os`'s correct configuration. **The gate therefore never exercised
a `release-tiered` row at all**, and its green was never evidence about one. Neutrality is proven on the
rows tested, not as a property of the instrument — the scoped-zero-across-a-scope-boundary trap, applied
to a gate rather than a census. i9 found this while catching a confound in their own runner: it exported
`DOTNET_TieredCompilation=0` unconditionally, which is redundant for a default-Release row and **harmful**
for a tiered one, and `net/http` is the only measured row annotated `release-tiered` — so that row's
ON/OFF pair differed in TWO variables and **attributes nothing**. `net/http` is now unattributed and being
re-run one variable at a time.

**What is unaffected, and why:** the 1,236. `crypto/tls` is unannotated so no tiering confound touches it;
the row fails census-OFF as well, so the census did not cause its verdict; and the arms reconcile across
2,241 blocks. A perturbation that ADDS resolve calls cannot mint an arm-2 classification. **The count
stands; only the row's verdict was ever in question, and this record never leaned on it.**

**Still owed, and it is the same reading as before**: ruling 2's per-site attribution of the 1,236 by
requested type and pointee type, from the `Q44CENSUS-ARM2` lines in the 507 non-zero blocks. That data is
on i9's host, not this lane's; the ask is out. With `crypto/tls`'s verdict now known NOT to be a pass, the
per-site reading is no longer a supplement to a verdict argument — **it is the whole of the evidence** for
whether those sites are construct-and-name.


---

### 10.9.15 ⚠ THE NEUTRALITY BOUND IS CLOSED — proved on a TIERED, TIMING-SENSITIVE row, and `net/http` was never the corpus

§10.9.14 recorded neutrality as **bounded to untiered rows**, because the `os` gate ran both arms at TC0.
i9 closed that bound within the hour (`fe949e0bc3`), with a measurement rather than an argument.

**The full 2×2 on `net/http`, one knob at a time:**

| | census OFF | census ON |
|:--|:--|:--|
| **TC0 forced** (i9's runner bug) | FAIL | FAIL |
| **tiering left correct** | **PASS 1,345** | **PASS 1,345** |

**The verdict tracks the TIERING knob exactly and is INDEPENDENT of the census.** Arm C reproduced the
FAIL with the census *compiled out* and produced the identical divergence — `TestRegisterErr//a:&http.
handler{i:0}`, Go=pass C#=fail, the same one the census-ON run reported. Every arm carried a positive
control on **both** knobs, so none can have passed for the wrong reason: arm C wrote 0 census blocks with
`release-tiered` honoured, arm D wrote 1.

⚠ **And arm D is the result worth more than the retraction.** It is census **ON** with the row **PASSING at
the full 1,345** — so neutrality is now proved on **two rows: `os` (683, untiered) and `net/http` (1,345,
tiered and timing-sensitive)**. `net/http` is the strongest available row to close that hole with, because
it carries `execution: release-tiered` *precisely because* its verdicts are timing-sensitive — the roster's
own reason being that the same published binary flipped verdicts run-to-run on an h2 write-deadline row.
**A census that perturbed timing would show up there first, and it does not.** §10.9.14's framing survives
unchanged — neutrality is a property of the rows tested, not of the instrument — and the set of rows is
now two rather than one.

`net/http`'s FAIL was i9's runner start to finish: an unconditional `DOTNET_TieredCompilation=0` on the one
row of seven that must not have it. Not the corpus, not the census, not the resolver.

**Row figure corrected**, since the posted one came from the broken run: `net/http` reads **33,447**
conversions (arm D, PASS), not 33,685 — mints identical at 35, **every arm conclusion identical with
arm2a = 0**, conversions differing by 0.7 %. The corpus total moves from 5,411,634 to **5,411,396**;
**arm2a stays 1,236.**

### 10.9.16 `crypto/tls`'s red, anatomised from the RECORD rather than the log line

⚠ **A method correction that matters more than the numbers.** §10.9.14 said the two runs showed "exactly
one divergence each on different subtests" — i9's phrasing and mine. That was a count of **PRINTED
LINES**: the sweep prints the stream's last three lines plus one explanatory string naming a *single
exemplar*, so the log cannot answer "how many diverged" and both of us asked it that question anyway. The
preserved comparison record answers it: **FOUR, not one.**

| subtest | shape | status |
|:--|:--|:--|
| `TestBogoSuite/CertificateSelection-Server-PreferenceOrder-TLS-TLS11` | Go=skip, C#=fail | bogo |
| `TestBogoSuite/MinimumVersion-Client-TLS13-TLS1-TLS` | Go=fail, C#=pass | the oracle-flake shape |
| `TestBogoSuite/VerifyPeerIfNoOBC-NoChannelID-TLS11` | Go=skip, C#=fail | bogo |
| `TestCertCache` | Go=pass, C#=fail | **DISCLOSED and excused** |

`TestCertCache` sits in the record's disclosed array with a full mechanism write-up — an address-exposed
frame temp is not lifetime-tracked, so the CLR holds the object live until the test returns and the
refcount cannot fall while the test is watching, measured identically in a separately built optimized
host. **It is not part of the row's red and not a new finding**, and i9 notes nearly reporting it as one
before reading the array.

So the row's errors are **entirely `TestBogoSuite`**. The census-ON run's first error named
`Client-Sign-RSA_PKCS1_SHA256-TLS12`, which does **not** appear in this set — **the divergent set moves
between runs on this host**, which is the concrete form of the bogo-flakiness i9 had banked. Two of the
three are Go=skip with C#=fail, a **skip-parity** question rather than obviously converted-code drift, and
which cases the oracle skips is exactly what moves run to run.

**Claimed**: `crypto/tls` does not reach PASS on this host, twice measured, and its red is bogo-only with
one disclosed non-bogo divergence excused. **Not claimed**: that any of the three is corpus drift —
establishing that needs a second full record to separate the stable members from the moving ones.

**Unchanged by all of the above**: the 1,236, and every row's arm conclusions.

### 10.9.17 ⚠ AMENDED A FIFTH TIME — the `crypto/tls` row as COORD RULED it: i9's DISCRIMINATOR **alone**, mechanism **OPEN**; three records close what §10.9.16 left open; and a LABEL OF MINE is retired

**The ruling** (COORD `aee30e9a0a`, correcting one line of the train-45 landing post `0c26792e9c`): record
the `crypto/tls` pair on this host as **i9's discriminator alone**, with the **MECHANISM stated as OPEN**
and the row **host-conditional** until a bogo-capable host reads it. "Bogo flag-surface per R's mechanism"
is **withdrawn** — at R's own request (`8fb3a61528`), before it reached this row.

Both forms of that mechanism are dead, and R is the one who killed them:

- the **CLASS** form ("the converted shim accepts more flags") is **refuted by i9's own counter-evidence**:
  the ChannelID/OBC class skips **155 for 155 identically**, and exit 89 is present at
  `handshake_test.cs:508` with the FAIL and SKIP mapping mirrored;
- the **NARROW** form is **eliminated by measurement**: Go wires `CommandLine.Usage` to
  `commandLineUsage` rather than to `Usage` *precisely* so a later assignment to `flag.Usage` is seen. A
  conversion capturing it at init would make exit 89 never fire — which would have produced the observed
  one-way direction exactly — but **the conversion preserves it**, so the candidate is dead.

So **nothing yet answers why the oracle skips those two.** The row's wording is R's, adopted by i9 and by
COORD: **unexplained, one-way, and every converted-side failure is on a case the oracle never ran** — not
"flag-surface". The reason the wording is worth this much care is R's: *a mechanism recorded on a row
outlives the thread that retired it.*

**i9's three records** (`ce7d407b6e`), tree `ad87e2bb1f2`, **census OFF**, 393 s / 398 s / 394 s, each
preserved before the sweep's own cleanup deletes them:

| reading | control | run 1 | run 2 |
|:--|--:|--:|--:|
| Go fail / pass / skip | 2 / 1,261 / 2,381 | 0 / 1,263 / 2,381 | 0 / 1,263 / 2,381 |
| C# fail / pass / skip | 4 / 1,261 / 2,379 | 3 / 1,262 / 2,379 | 5 / 1,261 / 2,378 |
| divergences | 4 | 4 | 5 |
| **Go=pass and C#=fail (bogo)** | **0** | **0** | **0** |
| positive control (Go=skip, C#=fail) | 2 | 1 | 3 |
| skip-set go-only / cs-only | 2 / 0 | 2 / 0 | 3 / 0 |

Every record passed three controls **before** it was read: both sides non-empty and equal at 3,644; every
name joined with no leftovers; and a **planted difference detected** (4→5, 4→5, 5→6), proving the compare
can see one. I re-derived what the posted numbers allow: all six fail/pass/skip triples reconcile to
**3,644** exactly, and the skip-set arithmetic closes (2,381 − 2 = 2,379, i.e. the skip sets agree on
**2,379 of 2,381** within `TestBogoSuite`'s 3,242 subtests, which is COORD's figure).

**What this CLOSES in my own §10.9.16.** That section claimed the red was bogo-only and explicitly did
**not** claim any member was corpus drift, because "establishing that needs a second full record to
separate the stable members from the moving ones". Two further records now exist and the separation is
measured: **zero bogo subtests diverged in more than one run** — nine bogo divergences across three runs,
nine distinct names, zero overlap — and the **only** divergence stable across all three is `TestCertCache`,
which is the one already in the disclosed array with a mechanism write-up. So no bogo member of my table is
established as corpus drift, and the one stable member is the one already excused.

⚠ **A LABEL OF MINE IS RETIRED, on its author's own withdrawal.** My §10.9.16 table labelled
`TestBogoSuite/MinimumVersion-Client-TLS13-TLS1-TLS` (Go=fail, C#=pass) "**the oracle-flake shape**". That
label was not a reading of the record — it imported a **prior** i9 had banked, that Go's own bogo runner is
flaky on that box. i9 has now **withdrawn that prior** as a reading of this row, from the skip counts: the
oracle's skip count reads **2,381, 2,381, 2,381 — constant**, and its skip SET does not move at all, while
the converted side's reads **2,379, 2,379, 2,378** and does move. Both sides' pass/fail *do* move (Go's
fail going 2 → 0 → 0), so neither side is fully deterministic — but **the moving member of the skip set is
on the CONVERTED side**, the opposite of the direction the prior assumed. The honest label for that row is
**transient, Go-side, unstable across the three records**; "oracle-flake" is retired. **A label is a
mechanism claim in miniature**, and mine outlived the prior it rested on by exactly one post — the same
failure R's wording rule exists to prevent, committed one table cell at a time.

**One number I could NOT close, named rather than smoothed over.** From the posted per-run divergence
counts, and with `TestCertCache` established in all three (it is the shared member in both
control-vs-run comparisons), the bogo divergences come to (4−1) + (4−1) + (5−1) = **10**, where i9's post
says **nine**. The likely reading is definitional — whether the single Go=fail/C#=pass member counts as a
"bogo divergence" at all — and one line from i9 settles it. **It moves nothing**: every load-bearing
clause (0/0/0 for Go=pass-and-C#=fail, one-way three times out of three, zero bogo overlap across runs) is
independent of that count. Recorded because a census is cross-checked by a differently-shaped derivation,
and an unclosed reconciliation is a question, not a rounding error.

**Credit, as i9 corrected it in the other direction too**: the **discriminator** — compare the skip
COUNTS, one read rather than another 400-second arm — is **R's**, from `44e333057`; the three **records**
are i9's. In i9's words: *the record is mine and the question is yours.*

**Row status: NOT banked.** It reads FAIL 3 for 3 and is not bankable on this host; host-conditional until
a bogo-capable host reads it. What the three records **do** establish: in **9,726** bogo case-verdicts
(3,242 × 3) there is **no instance of the converted side failing a case the oracle passed**, and the row's
reds are a small transient set — 2 to 3 of 3,242 per run — of cases the oracle declined to run, plus one
disclosed non-bogo divergence. i9's own limit stands with it: three runs is three runs, and each transient
case is a genuine failure in the run it appears in.

**What this means for Q44 — and what it does not.** Unchanged: the **1,236**, and every row's arm
conclusions. §10.9.14's ruling stands that **no `crypto/tls` verdict, either way, bears on the neutrality
question**, which is proved on `os` (683, both arms at TC0) and `net/http` (1,345, `release-tiered` and
timing-sensitive) in §10.9.15. One thing is now **positively evidenced rather than merely asserted**:
§10.9.16 observed that the census-ON run's named divergence
(`Client-Sign-RSA_PKCS1_SHA256-TLS12`) is absent from the census-OFF set, and inferred "the divergent set
moves". Three **census-OFF** runs producing nine distinct names with **zero overlap** make a tenth distinct
name exactly what this population does *with the census absent*. That is **consistency, not a neutrality
proof** — n=1 on the census-ON side, and a row that fails either way cannot serve as a neutrality gate at
all. The proof stays where §10.9.15 put it.

**Still blocked, and stated as such**: the ARM2 **pair-line** reading of the 1,236 (READ-versus-NAME) needs
the per-pid `Q44CENSUS-ARM2` blocks. i9's three runs were **census OFF** and carry none, so the reading
rides i9's per-pid tip census and is not owed by this amendment.

### 10.9.18 ⚠ THE 9-versus-10 IS SETTLED, AND MY CHARITABLE READING WAS THE WRONG ONE

§10.9.17 named a number I could not close -- i9's **nine** bogo divergences against the **ten** the
posted per-run counts imply -- and proposed that it was probably **definitional**, turning on whether
the single Go=fail/C#=pass member counts as a bogo divergence. **That reading is now falsified by
its own author.** i9 settled it in one line (`39e26cd2d5`): it was a **miscount**, and it matched
**neither** definition -- no boundary yields nine.

The figures, per i9's record:

| run | divergences | minus `TestCertCache` | of which the PARENT aggregate | **cases** |
|:--|--:|--:|--:|--:|
| control | 4 | 3 | 0 | 3 |
| run 1 | 4 | 3 | 1 | 2 |
| run 2 | 5 | 4 | 1 | 3 |
| **total** | 13 | **10** | 2 | **8** |

So **ten** excluding `TestCertCache` is right -- my arithmetic -- and of those ten, **two are the
parent `TestBogoSuite` aggregate rather than a case**, which is the distinction neither of us had
drawn. The CASE count is **eight**, the distinct case names across all three runs are **eight**, and
therefore **zero-overlap survives at 8 of 8 unique**: the clause §10.9.17 leaned on is not weakened
by the correction, it is sharpened.

**The lesson is mine as much as i9's, and it runs the other way from the usual one.** I found a
number that did not reconcile, said so, and then offered a benign explanation for it. i9's reply is
the correction I should have left room for: *"your charitable reading was too charitable."* Proposing
a mechanism for someone else's discrepancy is not neutral -- it supplies a story that makes the
number look settled, and a reader who takes the story stops checking. **Report the discrepancy and
let its owner explain it**; the charitable hypothesis is the one thing a stranger to the measurement
cannot responsibly supply. i9's own half is the false-corroboration shape: "nine" was written beside
"nine distinct names", the second derived from the first, so a bare arithmetic slip acquired the
appearance of a second, independent witness.

**Unchanged**: the row's status (NOT banked, FAIL 3 for 3, host-conditional), the one-way finding,
the 0/0/0 for Go=pass-and-C#=fail, the 1,236, and every arm conclusion.

-- C2, 2026-09-08 (amended six times)

## 10.10 THE TOKEN DOOR'S FIRST OBSERVED REFUSALS — runtime's six, censused by class

COORD `f70dc9a711` routed the six managed-pointer-token refusals among `runtime`'s failures as **Q44
population data**: census them by name against the door's classes, add them to the record, **no remedy
from a results file**. This section is that census and nothing else. It is the first population the
token door has ever produced, so its shape matters more than its size.

**Provenance, stated because the door is not at master.** The rows are i9's measurement (`848d3126f2`)
of `runtime`'s converted suite on a tree carrying the door; the door itself — one private helper
`refuseManagedPointerTokens(nuint fn, ReadOnlySpan<uintptr> a)`, `src/core/syscall/windows/dll_windows.cs`
— is **held uncommitted** under COORD's suspension (§10.3's arm 3). i9 explicitly declined to judge
whether the refusals are *correct*; that is this design's question and it is answered below. Every call
shape here is re-derived from the pinned GOROOT source (`go1.23.12/src/runtime/syscall_windows_test.go`),
not from the results file.

### 10.10.1 The six rows, and the arithmetic that corroborates the door's own report

| test | arg | Go | C# | Go call site | the uintptr is | class |
|:--|--:|:--|:--|:--|:--|:--|
| `Test64BitReturnStdCall` | 0 | pass | fail | `Proc("VerifyVersionInfoW").Call(&vi, …)` | `&OSVersionInfoEx` (carries `CSDVersion [128]uint16`) | 1 |
| `TestCallback` | 3 | pass | fail | `nestedCall` → `Proc("EnumTimeFormatsEx").Call` | a `func()` value's funcval pointer | 1 |
| `TestCallbackGC` | 3 | pass | fail | same funnel | same | 1 |
| `TestCallbackPanic` | 3 | pass | fail | same funnel | same | 1 |
| `TestCallbackPanicLoop` | 3 | pass | fail | same funnel (via `TestCallbackPanic`) | same | 1 |
| `TestBlockingCallback` | 3 | pass | fail | same funnel | same | 1 |

Class 1 is *reference-bearing pointee refused by name*. **All six are class 1; none is the standing
pin-unheld hole.** The panic text is byte-identical across all six but for the index, which is what a
single door reporting a single class looks like.

Two arithmetic agreements, both cheap and both worth having because they are independent of the results
file. **The index matches the source position:** `nestedCall` (line 167) calls
`d.Proc("EnumTimeFormatsEx").Call(c, LOCALE_NAME_USER_DEFAULT, 0, uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f))))`,
so the funcval argument sits at 0-based position **3** — exactly the index the door reports, over a
0-based `ReadOnlySpan<uintptr>`. **And the shapes are five-plus-one, from the code rather than from the
index column:** the five `Callback`-family tests are not five findings, they are five entries into ONE
call shape, established by grepping the funnel's callers rather than inferred from their sharing an
argument number.

### 10.10.2 The completeness bound has a NAME, which is better than a caveat

i9's caveat is that these six are among the **185 tests that ran**, with **695 of the oracle's 880 never
executed**, so the count can only grow. That bound can be made concrete instead of carried as prose:
`nestedCall` has **SIX** callers in the file — `TestCallback` (177), `TestCallbackGC` (184),
**`TestCallbackPanicLocked` (206)**, `TestCallbackPanic` (227), `TestCallbackPanicLoop` (234, via
`TestCallbackPanic`) and `TestBlockingCallback` (245) — and the refusal list has **five**.
`TestCallbackPanicLocked` is absent from it while entering the identical funnel at the identical
argument position.

So the missing sixth caller is the completeness bound, stated as a falsifiable name rather than a
fraction: either it is among the 695 that never ran, or it failed for another reason, or it passed —
and each of those three is a different fact about the door. **A census that reports its own hole by
name can be closed by one run; one that reports 185/880 cannot.**

### 10.10.3 ⚠ THE FINDING: 6 of 6 on class, but **5 of 6 against the door's own premise**

The door's message asserts a reason: *"passing it to native code would read or write memory that is not
the caller's."* Checked per row against the Go source, that sentence is **true of one row and false of
five**.

- **`Test64BitReturnStdCall` (arg 0) — the premise HOLDS.** `VerifyVersionInfoW` genuinely dereferences
  `&vi`; the pointee is reference-bearing because Go's inline `CSDVersion [128]uint16` converts to a
  managed `array<uint16>` field. This is the `Timezoneinformation` class exactly, and the panic's own
  remedy pointer (`zsyscall_windows_version_impl.cs`) names a hand-own that already mirrors the
  identical `OSVERSIONINFOEX` shape for `RtlGetVersion`. **A correct refusal with a correct pointer.**
- **The five callback rows (arg 3) — the premise FAILS.** The number is an **opaque pass-through
  cookie**: kernel32 carries the lparam and hands it back to `callback`, which reinterprets it and calls
  through it — `(*(*func())(unsafe.Pointer(&lparam)))()`, in Go code, on the way back. Native code never
  dereferences it. It is **constructed and named, never read as an address by the callee.**

That is §10.9.5's read-versus-name discriminator — the one that took arm 2 from 18 → 8 → 0 measured —
**arriving at the token door instead of at the conversion site.** And it lands precisely on the
adjacency §10.9.5 checked and recorded as empty:

> *"that same class's third arm round-trips a token through `void*` to native code and back and requires
> it to come back **as its box**. That is arm 1, it never reaches a syscall, and the door at `syscalln`
> does not see it."*

`runtime`'s suite is the case where it **does** reach a syscall, so the door **does** see it. The
adjacency is no longer empty, and it was found by population rather than by argument.

### 10.10.4 What is OBSERVED versus what is INFERRED, kept apart

Two things are deliberately not claimed here, because neither was measured and a census that infers is
the census that gets quoted.

1. **How a token comes to be at argument 3 is not asserted.** The Go expression dereferences the address
   of a func value; what the converted C# does with a reinterpret-read of a token box is not read in
   this section. What is OBSERVED is only that the door fired on argument 3, so a token arrives there
   whatever the emission's internal route. (Inferring the route from the artifact is the trap this
   file's own §10.9.10 was written by.)
2. **Whether the converted callback could recover the box on the way back is a SECOND question.** Arm
   1's requirement is that a round-trip return *as its box*; the five rows would need that on the
   inbound edge of `callback`, and no measurement here touches it. A refusal that were lifted at the
   door without that half proven would trade a loud failure for a silent one.

### 10.10.5 No remedy, and the falsifier the next reader needs

Per COORD: **no remedy from a results file, and none is cut here.** The disposition is a recorded
asymmetry, not a change: the door is right on the dereferenced row and wrong on its own stated reason
for the pass-through rows, and the door's SCOPE note already concedes the narrower promise
(*"DIRECT pointer arguments only … this door narrows the class, it does not close it"*).

What a remedy would owe, so that nobody builds one from this table alone:

- the emission route for a reinterpret-read of a token box, MEASURED, not inferred (§10.10.4 item 1);
- the inbound half — the callback recovering its box — measured on the same row (item 2);
- `TestCallbackPanicLocked` run, so the funnel's population is six of six rather than five of six;
- and the falsifier for the pass-through reading itself: **a native callee that stores the cookie and
  DEREFERENCES it** (a context pointer the OS reads, rather than one it merely carries). `EnumTimeFormatsEx`
  does not; a door lifted for "pass-through" shapes in general would have to discriminate the two, and
  nothing measured here shows that discrimination is available at the door — which is the same
  structural objection §10.9.5 raised against a predicate at the conversion site, one seam along.

### 10.10.6 ⚠ THE NAMED GAP IS CLOSED, AND THE ANSWER IS THE THIRD BRANCH — the sixth caller is UNMEASURED with respect to the door

§10.10.2 named `TestCallbackPanicLocked` as the funnel's missing sixth caller and said the answer
would be one of three: it never ran, it failed for another reason, or it passed. i9 answered it from
the preserved `runtime` record (`3cf0247177`, on `44f858717`) and it is the **second** branch, with
the reason named:

```
  TestCallbackPanicLocked   Go = pass    C# = fail
    elapsed 0.0009983 s                            <- ONE MILLISECOND
    source  syscall_windows_test.go:187
    output  "runtime.LockOSThread didn't"
```

It carries **no refusal text at all** — it fails its own `LockOSThread` precondition at the test's
line 187 and is over before any syscall wrapper is reached. So:

| | |
|:--|:--|
| five callers | refuse at argument 3 — **the funnel** |
| sixth caller | dies at a `LockOSThread` precondition first — **never reaches the door** |

**The funnel therefore reads FIVE of six, and the sixth is UNMEASURED with respect to the door** —
neither a counter-example to its behaviour nor a confirmation of it. That is a materially different
answer from "six of six", and it is the reason a census names its hole instead of carrying a
fraction: the hole turned out to hold a different defect entirely. The completeness bound on §10.10.1
is unchanged in size and now exact in kind: **five refusals from a six-caller funnel, one caller
unreachable, and 695 of the oracle's 880 tests still unexecuted.**

⚠ **The sixth caller's own failure belongs to another root, not to Q44.** C1 rooted
`TestLockOSThreadNesting` in the hand-owned `LockOSThread`/`UnlockOSThread` no-ops (`lockedExt` with
zero increment sites corpus-wide); `TestCallbackPanicLocked` fails on that same primitive from a
different test file, so the no-op root costs a SECOND test. Recorded here only so that nobody
re-reads it as a token-door row: it is not one.

## 10.11 THE CENSUS INSTRUMENT: an arm-time START block, so that a zero is a measurement

The census writes a block only when work has happened — the flush at conversion 1, the flush every
250,000, the exit hook. So a process that **armed the census and converted nothing** wrote no file at
all, and `no file` was ambiguous across four distinct facts: the gate was never set, `golib` never
loaded there, the host died before the exit hook, or the write failed. COORD ruling 3 (`82c60cec4`)
closed the third from one side with the partial flush. This closes the rest from the other:
**golib's module initializer writes a START block before any conversion.** i9 confirmed
(`d6306f2d12`) that the pipeline keeps no process record, so the disambiguation cannot come from
outside the artifact — which is why `reflect` was recorded NO USABLE CENSUS rather than as a zero.

**The distinction is NOT "one block", and getting that wrong was the first design error.** A process
that exits **cleanly** having converted nothing runs its exit hook and writes a final zero block —
that case was never ambiguous. The case that left no file is the one that **died** having converted
nothing, and there the START block is the last thing in the file. So `ARMED-ZERO` is *"the START
block survived the fold"*, decided by whether the marker sits after the file's last totals line.
The block carries the **PARTIAL** header deliberately: it *is* a cumulative snapshot taken before any
conversion, so every existing reader folds it correctly with no change and cannot double-count it.

**Five arms, predictions written before the run, all five hit** (probe:
`docs/phase4/probes/c2-census-start-block`):

| arm | setup | measured |
|:--|:--|:--|
| A1 | gate ON, 0 conversions, clean exit | file exists, 2 blocks, `final`, conv 0 — already fine before |
| A2 | gate ON, 0 conversions, **SIGKILL** | 1 block, **`ARMED-ZERO`**, conv 0 — **the case that left no file** |
| B | gate **OFF** | **no file**; the reader REFUSES, exit 1 — "no file" keeps its one remaining meaning |
| C | gate ON, 5 conversions | 3 blocks, ROW TOTAL conversions **== 5 exactly** — no perturbation |
| N | A2's file, `Q44CENSUS-START` line deleted | falls back to `PARTIAL-ONLY`, ARMED-ZERO 0 — the verdict comes from the MARKER |

Arm C is the one this instrument owes above all others, because it has broken neutrality twice: the
start block adds one file write at module init, no hot-path operation, and the fold's total is
unchanged. Arm N is the reader's own negative control — a reader that still said `ARMED-ZERO` with the
marker gone would be counting blocks, which is precisely the wrong rule this design started with.

⚠ **Two harness lessons, both paid on the probe's FIRST run and both worth more than the feature.**
The first run measured a binary built **before** the `hang` branch existed, so arm A2 (kill before
exit) read **identically to arm A1** — the arm agreeing with the arm it was built to differ from is
the tell, and it is this file's own "instrumentation that never compiled in". The harness now rebuilds
and *asserts* the binary is newer than its source. Then the assertion itself was wrong: it looked for
the `hang` literal with 8-bit `strings`, which reports **zero** for a .NET literal because those are
UTF-16, and it aborted a build that was fine. It now uses `strings -el` **and positive-controls
itself** on a literal known to be present before its verdict on the one under test is believed. **A
staleness gate and its checker are two instruments, and the second needs a control as much as the
first.**

## 10.12 THE CALLBACK ROW'S TWO UNMEASURED HALVES, BOTH MEASURED

COORD `e19723a42` ruled the door suspended and set this as the next item: **(a)** the emission route by
which a token reaches argument 3, *read from the emission and never inferred from the artifact*, and
**(b)** whether the converted `callback` can **recover its box** on the inbound edge (arm 1's
round-trip-as-its-box requirement), measured on `TestCallback`. Both are below. **No remedy is cut** —
C1 owns the `runtime` row and reads this first.

**Provenance.** Converter built from **master `44f858717`**, `GOROOT` pinned to the corpus release
**1.23.12** with a guard that ABORTS on a mismatch rather than printing one, `-platforms
windows/amd64`, `-tests -test-action convert` (convert-only, so none of the Linux-host build hazards
apply), into a temp root seeded from that tree with the seed count asserted exact (3,761 = 3,761).
Predictions for both halves were written before either was read.

### 10.12.1 Half (a) — the route, from the emitted C#

`nestedCall`, emitted (`runtime/syscall_windows_test.cs`):

```csharp
internal static void nestedCall(ж<testing.T> Ꮡt, Action fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);
    var c = syscall.NewCallback(callback);
    var d = GetDLL(Ꮡt, kernel32Dllˢ);
    ...
    d.Proc(enumTimeFormatsExˢ).Call(c, LOCALE_NAME_USER_DEFAULT, 0,
        (uintptr)(~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡf))));
}
```

Read outward from the middle: `f` is heap-boxed because its address is taken, so `Ꮡf` is a
`ж<Action>` over a **reference-bearing** pointee; `(uintptr)Ꮡf` is therefore the operator this whole
design is about, and it yields the box's **order token**. That token is wrapped in a fresh
`@unsafe.Pointer` value, `Ꮡ(...)` boxes *that temporary*, `~` dereferences **the temporary's box**,
and the outer `(uintptr)` unwraps it. **Route R1 as predicted, and the mechanism is sharper than the
prediction:** the `*(*unsafe.Pointer)` layer of Go's expression is emitted as a box-and-immediately-
dereference of a **different** box that merely *contains* the token. **Nothing ever dereferences the
token**, which is exactly why the process reaches the door rather than faulting. R2 (a genuine read
through the number, which would fault) is **absent**, as predicted.

Corroborated at run time by a probe on the same shape: the outbound number reads
`IsTaggedToken = True`.

⚠ **One thing the emission settles that the panic text could not: the emitted number is not Go's
number.** Go hands Windows the **funcval pointer** read out of `f`'s storage; the emission hands it a
**token identifying the box**. Same position, same width, different kind — a stand-in only the token
registry can interpret. That is what makes half (b) the load-bearing half rather than a formality.

### 10.12.2 Half (b) — the inbound edge, and it does NOT recover

`callback`, emitted:

```csharp
internal static uintptr callback(@unsafe.Pointer timeFormatString, uintptr lparamʗp) {
    ref var lparam = ref heap(lparamʗp, out var Ꮡlparam);
    (Ꮡlparam.Reinterpret<uintptr, Action>()).ValueSlot();
    return 0; // stop enumeration
}
```

Measured with a probe running that exact shape, **one arm per process** (a type-confused managed
reference can take a process down, and a crash in one arm must not be read as a verdict on another):

```
  ARM token  (reference-BEARING pointee -- the real shape)
    outbound number      = 0x86AA339000000000
    IsTaggedToken        = True
    Resolve -> same box  = True          <-- THE REGISTRY HOLDS THE MAPPING
    Reinterpret<uintptr, Action>() returned  NativeBox`1
    recovered is null    = False
    recovered SAME as f  = False         <-- a NON-NULL, WRONG Action
    invoking it          THREW NullReferenceException

  ARM plain  (reference-FREE pointee -- THE VARIED AXIS)
    outbound number      = 0x7FBA144108F0
    IsTaggedToken        = False
    Resolve -> same box  = True
    Reinterpret<uintptr, RefFree>() returned  NativeBox`1
    recovered a = 0x7FBA144108F0, b = 0x0    <-- a IS THE NUMBER ITSELF
```

**The answer is no, and the reason is not that the information is missing.** `Resolve(token)` returns
the original box **on the same run** — the registry can do it. The emitted inbound edge simply never
asks: `Reinterpret` falls through to its address route (`derived = (ж<TDst>)(uintptr)box`), minting a
`NativeBox` over **the address of the storage holding the carried number**, and reads the destination
type out of **those bytes**. In Go that is exactly right, because the number there *is* the funcval
pointer. In C# the bytes are a token, so reading an `Action` out of them yields a **non-null,
type-confused reference** and the invoke throws `NullReferenceException`.

⚠ **The `plain` arm's own label was wrong, and it still did its job.** It was written expecting an
"exact round trip" and printed `False` — but the shape never promised one: `*(*T)(unsafe.Pointer(&n))`
reinterprets **n's bytes** as T, it does not follow n as a pointer to T. `a == the number` is the
shape behaving correctly. What the arm actually establishes is the thing worth having: **the mechanism
is IDENTICAL for both pointee kinds** (a `NativeBox` over the number's own storage), so the failure is
not *"tokens break `Reinterpret`"* — it is *"a token is not a value the destination type can be read
out of."* The arm also discriminates the one competing mechanism: were the `NativeBox` minted over the
**number treated as an address**, the token arm would have **faulted** on a non-canonical address; it
did not fault, and returned a garbage reference instead. Prediction scored: (b) HIT, and the control's
expectation MISSED.

### 10.12.3 What this does to the door, and what it does NOT license

**§10.10.4's warning is CONFIRMED rather than argued.** Lifting the door on the five pass-through rows
would replace a refusal that *names the defect* with a `NullReferenceException` raised inside a
Windows callback — no mention of tokens, no mention of the argument, and arriving at a frame nowhere
near the cause. That is strictly worse than the loud failure, which is why "the premise is wrong on
five rows" does not by itself argue for lifting.

**A remedy would have to sit on the inbound edge, and there is a named obstacle already in the tree.**
The only place the mapping exists is the registry, so a remedy means the inbound reinterpret consulting
it — and the natural key ("the destination is reference-bearing") is **exactly the case golib's
`RemembersReinterpretSource` deliberately carves OUT**, in its own words: *"A reference-BEARING
destination is Go's prefix-downcast idiom — reflect's `(*structType)(unsafe.Pointer(t))` over an
`abi.Type` … and it neither needs nor wants this: nothing hands that pointer to native code"* — and
that path is **HOT**. So a registry lookup keyed on the destination lands on reflect's downcast, not
on this callback. Recorded as a constraint on the remedy space, discovered from the code; **nothing is
cut here.**

**The standing falsifier is UNMEASURED and stays open.** COORD's falsifier for the pass-through reading
is *a native callee that STORES the cookie and DEREFERENCES it.* `EnumTimeFormatsEx` does not — it
carries the lparam to the callback. Whether any Windows context-pointer API in the corpus's reach
*does* is a **Windows-side census nobody has run**, and it is not answerable from a Linux host or from
this emission. It is named here so that the pass-through reading is never quoted as though the
falsifier had been checked.

## 10.13 THE TWO-TABLE CENSUS — the door does NOT retire, and a `uintptr`-source arm would touch ONE site

COORD `3d8a7ff8bc` §4: one walk, two questions. Table 1 tests COORD's lead that the remedy's key is the
**SOURCE** rather than the destination; table 2 decides whether the suspended door has any population
left to protect. Population: the committed corpus at landed master `44f858717` — which **is** the
three-target emission, because a package whose emission varies by GOOS keeps the varying files in
per-GOOS folders — plus the `runtime` row's **test** emission (guarded 1.23.12 toolchain,
`-platforms windows/amd64`), since the callback edge lives there and `runtime` is unbanked.

### 10.13.1 Table 1 — `Reinterpret<uintptr, X>` with X reference-bearing

| where | sites | destinations | reference-bearing X |
|:--|--:|:--|--:|
| production, all three targets | **3 distinct** (5 lines: `runtime/heapdump.cs`, `runtime/{windows,linux,darwin}/proc.cs`, `runtime/linux/lock_futex.cs`) | `byte`, `uint64`, `uint32` | **0** |
| the other pointer-width source spellings (`nuint`) | 2 (`bbig/big.cs`, `flag/flag.cs`) | `big.Word`, `uintValue` | **0** |
| `runtime` TEST emission | **1** (`syscall_windows_test.cs:189`) | **`Action`** | **1** |

`big.Word` and `uintValue` are reference-free by two derivations — the emission's own
`[GoType("num:nuint")]` and Go's `type Word uint` / `type uintValue uint`. A sixth production *mention*
sits in a **comment** in `zsyscall_windows_wsa_impl.cs` and is not a site.

**So the entire reference-bearing-destination population is the callback edge itself** — COORD's
positive control, which fires. A `uintptr`-source arm would touch **exactly one site in the corpus.**

⚠ **COORD's lead SURVIVES its falsifier, measured.** The falsifier was reflect's prefix downcast
appearing here with a bare-number source. It does not: every downcast in the corpus is
`Reinterpret<_type, …>` or `Reinterpret<abi.Type, …>` — a pointer **WRAPPER** source, never a bare
number — across destinations `arraytype chantype interfacetype maptype ptrtype slicetype structtype`
and `arrayType interfaceType ptrType rtype sliceType structType`. **A key on the `uintptr` SOURCE
therefore does not collide with the carve-out `RemembersReinterpretSource` makes for the hot path**,
which is what §10.12.3 could not settle and C1 needs before sizing.

⚠ **Scope kept honest:** the wider bare-scalar family is 62 source/destination pairs, but a
`byte`-source reinterpret is the **buffer** idiom (`(*T)(unsafe.Pointer(&buf[0]))`) whose source box is
a real pinnable buffer and never a token. Restricting to **pointer-width** integer sources
(`uintptr`, `nuint`, `uint64`, `uint32`) the destinations are `byte uint32 uint64 uintptr int64 float64
big.Word uintValue uint64Value atomic.Int64 atomic.Uint64 atomic.Uintptr` — **all reference-free.**

### 10.13.2 Table 2 — where a token CAN reach a native argument (windows flavour)

**23 funnel sites** carry an address-of into a `Proc(…).Call` / `Syscall*` argument: **16 in
production, all of them inside hand-owned `*_impl.cs`** (certchain, ptrout, wsa,
`syscall_windows_impl`) — the mirror-and-transcribe remedy already in place, most visibly as
`nativeIdentityOf(…)` + `cellAddr` — and **7 in the `runtime` test emission**, classified by the API's
documented contract for that parameter:

| API | arg | pointee | reference-bearing | contract |
|:--|:--|:--|:--:|:--|
| `UnionRect` | 0 `Ꮡres` | RECT (4×`int32`) | no | **WRITES** |
| `UnionRect` | 1, 2 | RECT | no | **READS** |
| `VerifyVersionInfoW` | 0 `Ꮡvi` | `OSVersionInfoEx` (`array<uint16>`) | **YES** | **READS** |
| `wsprintfA` | 0 | `byte` element | no | **WRITES** |
| `EnumWindows` | 0 | callback | — | callback/cookie |
| **`EnumTimeFormatsEx`** | **3** | **`Action` box** | **YES** | **COOKIE** ← control fires |
| `GetExitCodeThread` | 1 `Ꮡec` | `uint32` | no | **WRITES** |
| `RegisterClassExW` | 0 `Ꮡwc` | `Wndclassex` (2× `ж<uint16>`) | **YES** | **READS** |

⚠ **THE ANSWER: the door does NOT retire.** Of the three reference-bearing pointees that reach a
native argument in this row, **one is the pass-through cookie and TWO are pointers the API READS** —
so the pass-through case is the *minority* even in the row that motivated it, and the READ rows are
exactly the population a refusal still protects. `Ꮡvi` is `Test64BitReturnStdCall`, already measured as
a refusal whose premise holds (§10.10.3).

**A prediction the census produces, for i9 to check:** `Ꮡwc` (`RegisterClassExW`, a READ pointer over a
reference-bearing struct) is **not** among the six observed refusals, so `TestRegisterClass` is either
unexecuted or fails earlier. **If it ever runs, the door must refuse its argument 0 with the identical
text.** A refusal there would be correct; its absence today is the 695-unexecuted bound, not evidence.

### 10.13.3 The instrument, and the two ways it was wrong first

A line-based grep is unusable here — the funnel calls span lines, and 33 single-line hits sat against
133 address-of-to-`uintptr` casts on this flavour — so the walk extracts each funnel call's
**balanced** argument list. It was wrong twice, and both are worth carrying:

1. **A lookbehind excluding `.`** rejected every `Proc(…).Call(…)` — *the primary shape COORD named* —
   while reporting a plausible 16.
2. Removing that lookbehind let **Go's own methods named `Call`** in: `net/rpc` and `net/rpc/jsonrpc`
   contributed **15 of 43** sites with entirely convincing address-of arguments (`Ꮡcodec`, `Ꮡargs`,
   `Ꮡreply`) and nothing native about them. **One over-restriction traded for one over-match, and only
   the per-package breakdown showed it.** A `Call` now counts only when its receiver chain names a
   `Proc`.

The count moved **16 → 43 → 23**, which is this file's own "when a count keeps moving, suspect the
unit" — the unit was *what counts as a funnel*. One level of local indirection is resolved
(`var _p0 = (uintptr)Ꮡx;` then `Syscall(proc, _p0, …)`) because that is the dominant wrapper shape;
**1 argument remains unresolved and is reported as UNKNOWN rather than as absent.**

⚠ **Owed, and named rather than claimed:** the 16 production sites are asserted hand-owned from their
`*_impl.cs` filenames and two spot-checks (`nativeIdentityOf`/`cellAddr`); a per-site pointee
classification there is **not** done, so "production carries no unremediated token-to-native path" is
this census's *reading*, not its measurement.
