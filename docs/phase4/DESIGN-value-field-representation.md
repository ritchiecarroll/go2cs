# Q74 — the converted representation of a VALUE-AGGREGATE struct field

**STATUS: PROPOSED — design only.** Nothing is cut against this record until it is seated
(coordinator ruling, 2026-09-06: *"Q74's design record: YES, and before any cut … Mint it; nothing
is cut against it until it is seated."*). No converter change, no golib change, no corpus change
ships with this document.

**Lane:** C1 (Linux container). **Date:** 2026-09-06.
**Tree read:** `origin/master` @ `69136ef1a`; toolchain go1.23.12, .NET SDK 10.
**Minted:** coordinator, 2026-09-05, as *"the converted representation of a fixed-size array VALUE
field"* — a golib/emission MODEL question raised when the page allocator's own bit block could not
alias native memory.
**Siblings:** [`DESIGN-native-array-view.md`](DESIGN-native-array-view.md) (RATIFIED; §3 HELD on
provenance), [`DESIGN-native-backed-slice.md`](DESIGN-native-backed-slice.md) (RATIFIED),
[`DESIGN-managed-pointer-token.md`](DESIGN-managed-pointer-token.md). §2 states what those settle
and — the load-bearing part — what they do **not**.

---

## 1. The measured bill

### 1.1 What the converter emits, read from the files

A Go fixed-size array field converts to golib's `array<T>`, and `array<T>` is a managed struct
carrying a **reference** (`src/core/golib/array.cs:47–57`):

```csharp
public readonly struct array<T> : IArray<T>, IList<T>, IReadOnlyList<T>, IEquatable<IArray>, IGoZeroShaped
{
    internal readonly T[] m_array;      // a MANAGED REFERENCE
    private readonly int m_low;         // the window, so (*[N]T) conversions can alias
    private readonly int m_length;
```

So Go's `[8]uint64` — **64 inline bytes** — becomes **16 bytes on x64, of which 8 are a pointer**.
Three consequences, and they are distinct:

1. **SIZE.** The containing struct's size is not Go's size.
2. **LAYOUT.** The CLR gives **auto layout** to any struct holding a reference-typed field and
   **reorders** it. This is measured, not inferred: `CLAUDE.md` records converted `Msghdr`'s
   `Namelen` at managed offset **40** where the kernel reads `msg_namelen` at **8** — where it finds
   `Iov`, an object reference and therefore never zero (EISCONN on a connected stream, EINVAL on a
   datagram).
3. **CONTENTS.** The bytes at the field are a pointer, not the elements. Reading them as elements
   fabricates a managed reference; writing elements over them corrupts what the collector reads.

### 1.2 Three faces, each found by a different lane from a different direction

**(a) The READ side — Q74's own origin.** `runtime/mpallocbits.cs:11` declares
`[GoType("[8]uint64")] partial struct pageBits`, which `pallocData` carries twice. A `*[N]T` over
native memory cannot be **viewed** as that struct, because the element struct's managed form carries
a reference where the native block has bytes. Measured at increment 8 (`b7a58eda0`): **88 rows mute
before, 88 answering after**, and the rows **still fail** — golib refusing by name because
`pallocData` carries a managed `uint64[]` reference where a native block needs a layout-compatible
value. Turning a mute crash into a named refusal is what made the diagnosis possible.

**(b) The WRITE side.** Such a struct cannot be **passed to the kernel by address**.
`internal/syscall/windows/windows`'s `_OSVERSIONINFOW` holds `internal array<uint16> csdVersion =
new(128);` — Go's inline `[128]uint16` — and `rtlGetVersion` faults on it. Measured population at
the time: **38 sites** on the Windows lane.

**(c) The REFLECT side.** A Go-layout byte offset computed into a CLR auto-laid-out struct —
`unsafe.Add(unsafe.Pointer(&in), offset)` — lands on a managed reference slot. An integer written
over a reference slot corrupts what the collector reads, which is **uncatchable by construction**.
That is the shape behind `reflect`'s 388 → 167 reported verdicts.

**These are one representation decision with three failure modes**: the collector, the kernel, and
the field-offset arithmetic each meet it differently.

### 1.3 R's control WIDENED the class, and this record is cut at the WIDE boundary

Eight variants, one axis, one process each: **array, slice, pointer, string, map, interface and func
all die; `uintptr` survives; Go survives all eight.**

**The root is a reference slot ANYWHERE in the struct. Fixed-size arrays were the first shape anyone
met, not the shape that matters.** A design cut at "fixed-size array field" would be cut at a
symptom.

**⚠ RETRACTION, mine, recorded here because the sizing sentence is quoted in the ruling that
commissioned this record.** On 2026-09-05 I posted that whoever sizes Q74 *"does not need a fresh
population census"* because `[GoValueClone]` is *"the converter's OWN recorded decision for exactly
Q74's population."* **That was written before R's control came back broad, and it is wrong.**
`[GoValueClone]` records which fields a **by-value copy must deep-copy** — its own summary says *"the
fields whose type is a fixed-size array, or another struct that itself carries one"*
(`golib/GoValueCloneAttribute.cs:12–15`). That is the **array subfamily**, not the class. The
coordinator's 540 and the ruling built on it inherit the same scope, and §1.4 states both numbers
with their scopes named rather than folding them together.

### 1.4 The populations, re-derived at `69136ef1a`

**The array subfamily, measured here** (`git grep`, so `src/core/.gitignore` cannot under-count):

| reading | value |
|---|---|
| `[GoValueClone]` occurrences in `package_info.cs` | **493** across **95** files |
| …of those, on a `partial struct` declaration | **493** (all of them) |
| **DISTINCT struct names** carrying it | **321** |
| occurrences in other files | **67** across **37** files |

Top of the distribution: `runtime/{darwin,linux,windows}` at **69 / 62 / 54**, then
`syscall/{darwin,linux,windows}` at **39 / 34 / 28**.

**Reconciliation with the coordinator's figure, stated rather than folded.** The 493/95 and the
top-six distribution reproduce the coordinator's reading **exactly**, computed independently. The
"other files" reading does not: mine is **67 in 37**, theirs was **47 in 27**. I have not reconciled
the difference and I am not adopting either as the joint number. My 67 includes **9 occurrences that
are golib's own declaration of the attribute** (`GoValueCloneAttribute.cs`, `IGoValueClone.cs`,
`array.cs`) and are not decisions at all; the rest are test-side metadata (`package_test_info.cs`),
banked test emission (`*_test.cs`) and `.cs.auto` review siblings. **The decision count that matters
is 493 occurrences over 321 distinct structs, and the honest headline is "321 structs", not "540
decisions".**

**The BROAD class's population is NOT MEASURED, and that is this record's largest gap.** The
instrument that would measure it: a `go/types` pass over every converted struct asking whether any
field's **converted** type carries a managed reference — array, slice, string, map, interface, func,
pointer, or a struct that transitively carries one. It does not exist. Option (C) in §3 cannot be
sized without it, and **§5 makes building it the first gate rather than an afterthought.**

### 1.5 The point-remedy wave already paid for

**26 files under `src/core` (excluding golib) carry an explicit `[StructLayout]`.** They split three
ways, by reading each file's own header:

**Nineteen are this class's blittable-mirror hand-owns** — every one names the same remedy in its own
words (*"the class's established remedy: a blittable mirror"*, *"cannot cross the managed/native
boundary by address"*, *"golib `array<T>` fields, which are MANAGED CLASS REFERENCES"*):

```
internal/syscall/unix/linux/siginfo_linux.cs
internal/syscall/windows/windows/net_windows_impl.cs
internal/syscall/windows/windows/syscall_windows_impl.cs
internal/syscall/windows/windows/zsyscall_windows_impl.cs
internal/syscall/windows/windows/zsyscall_windows_module_impl.cs
internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs
net/windows/interface_windows_impl.cs
os/user/windows/lookup_windows_impl.cs
syscall/darwin/sockaddr_darwin_impl.cs
syscall/linux/sockaddr_linux_impl.cs
syscall/linux/structclass_linux_impl.cs
syscall/linux/zsyscall_linux_amd64_impl.cs
syscall/windows/exec_windows.cs
syscall/windows/syscall_windows_impl.cs
syscall/windows/zsyscall_windows_addrinfo_impl.cs
syscall/windows/zsyscall_windows_certchain_impl.cs
syscall/windows/zsyscall_windows_dnsrecord_impl.cs
syscall/windows/zsyscall_windows_impl.cs
syscall/windows/zsyscall_windows_wsa_impl.cs
```

**One is the class's THIRD FORK and takes a different remedy** —
`syscall/windows/security_windows.cs`, whose own header says it is *"not a wrapper handing the kernel
a non-blittable struct by [address] … so no mirror-the-wrapper remedy applies"*: the kernel memory is
a byte buffer the CALLER reinterprets. It is in the class and **not** retired by a mirror.

**Six are NOT this class and must not be counted as debt:** `sync/atomic/type.cs` (explicit layout on
*unmanaged* types because .NET forbids overlapping a managed reference — the opposite concern),
`runtime/{darwin,linux,windows}/mheap.cs` (the converter's OWN emission for a `sys.NotInHeap` union:
`[StructLayout(LayoutKind.Explicit, Size = 1)] partial struct gcBits`), and two `*_test.cs`.

---

## 2. What the sibling records settle, and what they do NOT

`DESIGN-native-backed-slice.md` and `DESIGN-native-array-view.md` are both RATIFIED and both make a
**standalone** `slice<T>` / `array<T>` able to **alias** native memory instead of fabricating a
managed reference from it. `DESIGN-native-array-view.md`'s §3 emission work is **HELD** pending a
provenance amendment, because pinned-managed and genuinely-native addresses arrive indistinguishable
in `m_nativeAddr`.

**Neither touches Q74, and the distinction is the reason this record exists.** Making `array<T>`
dual-mode changes what an array VALUE can **see**. It does not change what a struct **carrying** one
**is**: `pallocData` still holds a 16-byte struct whose first field is a reference where Go has 64
inline bytes, so the containing struct is still auto-laid-out, still the wrong size, and still
unpassable by address. **The view arc fixes the leaf; Q74 is the container.** Neither record's §
addresses a field position — checked by reading their section lists and searching both for the
containing-struct case.

**What they DO transfer:** the precedent that a representation change of this family is decided by a
measured hot-path branch cost rather than by argument, and the standing rule that an ambiguous
address is a provenance question before it is a representation question. Option (C) below inherits
both.

---

## 3. The options, with their costs

**(A) Point remedies — the status quo.** A blittable `[StructLayout(Sequential)]` mirror per site,
hand-owned, with `fixed` buffers where Go has inline arrays and a size check at the boundary.
*Cost:* **19 files today**, growing linearly with each new native boundary; each is a permanent
hand-merge obligation and a file frozen against reconversion. *Retires:* nothing. *Virtue:* it works,
it is measured, and the rows it unblocks bank now.

**(B) REFUSE loudly at every boundary.** Generalise the ruling already made for the reflect face: an
address is minted for a struct only when its converted form is blittable; otherwise a **catchable
panic naming the field**. *Cost:* bounded, no representation change, no corpus regeneration.
*Recovers:* **catchability**, not capability — the defect stays exactly as open as today, which is
honest, and a caller that would have corrupted the heap instead gets a Go-visible panic. *Precedent:*
this is what `PointerStorage` was built to express, and it is the shape ruled for C2's seat.

**(C) Represent a value-aggregate field INLINE.** Emit the containing struct with a layout compatible
with Go's — `fixed` buffers or explicit layout — whenever every field is blittable. *Cost:*
corpus-wide, and it is the reason the coordinator ruled this a design record rather than a lane's
evening. It touches: the per-box byte rule (a representation change lands on every value of the
affected types); the reflect bridge's view of such fields; `[GoValueClone]`'s entire reason to exist
(an inline array needs no deep copy, so **321 structs' recorded deep-copy decisions become dead**);
and every one of the 19 mirrors. *Blocked on:* the §1.4 census that does not exist.

**(D) HYBRID — (B) as the floor everywhere, (C) scoped to the structs the corpus actually passes to
the kernel or views over native memory.**

**RECOMMENDATION: (D).** (B) is the honest floor, is already ruled for one face, and is the only
option that can land without a census. (C) is the only option that RETIRES the 19. And scoping (C)
by the **measured** native-boundary population — rather than by "every struct with a reference
field", which is most of the corpus — is what makes it sizeable at all. **This record recommends;
it does not cut, and (C) is not sized here because its census does not exist.**

---

## 4. What (C) would RETIRE — the coordinator's explicit ask

The ruling asked that the record *"state explicitly which of the by-address hand-owns it would
RETIRE, so the wave's cost is visible as debt rather than discovered later."* Read per file:

| group | count | disposition under (C) |
|---|---|---|
| blittable-mirror hand-owns (§1.5 list) | **19** | **RETIRE** — the mirror exists only because the converted struct is not layout-compatible; make it compatible and the mirror is the struct |
| the third fork (`security_windows.cs`) | **1** | **STAYS** — the caller reinterprets a kernel byte buffer; no wrapper is at fault and no mirror-the-wrapper remedy applies |
| `[GoValueClone]` decisions | **493 occurrences / 321 structs** | **DEAD for the array subfamily** — an inline array is copied by the C# struct copy, exactly as Go does; the attribute, its generator arm and its emission would lose their population |
| not this class | **6 files** | unaffected (`sync/atomic/type.cs`, three `mheap.cs`, two tests) |

**The debt is therefore 19 files plus a whole attribute's machinery, and it is a CREDIT under (C) and
a permanent LIABILITY under (A).** That is the planning fact the ruling asked to be visible: (A) is
not free, it is a subscription.

**What (C) would COST that (A) does not:** every struct whose layout changes is a struct whose size
changes, and the byte rule's own doctrine says a per-value size change is a corpus-wide cost stated
as a per-row formula, not a verdict. `pallocData` alone goes from two 16-byte references to two
64-byte inline blocks — **+96 bytes per instance** — and that direction is unfavourable. **A
representation change that makes structs BIGGER must state that in its own commit**, which is why it
is an increment with its own cost pair and not a corollary of this record.

---

## 5. Gates, spec'd here and run by whichever lane implements

1. **THE CENSUS FIRST, and it is a gate rather than a preliminary.** A `go/types` pass over every
   converted struct, per target, counting fields whose converted type carries a managed reference,
   split by kind (array / slice / string / map / interface / func / pointer / transitive struct).
   **Positive control:** `pallocData` and `_OSVERSIONINFOW` must both appear; a struct of only
   fixed-width scalars must read zero. **Second derivation required** before its number is quoted —
   the `[GoValueClone]` count is *not* a second derivation of it, it is a subset by construction.
2. **The one-axis A/B for any (C) increment:** the same struct, converted both ways, on one box, one
   build, with the size read by `Unsafe.SizeOf<T>` — and a control that grows the struct by one word
   and reads RED, because a size row is a baseline until something makes it fail.
3. **Route #7's neighbourhood:** any change under `src/gen/` (the `[GoValueClone]` generator arm) owes
   a full behavioral COMPILE and a cross-assembly consumer gate.
4. **The reflect-bridge gate list** applies in full to (C) and to the reflect face of (B): the five
   largest banked reflect-importing rows **recomputed at gate time**, plus the FULL behavioral suite,
   since a bridge change can emit byte-identical `.cs` and still move runtime behaviour.
5. **`encoding/gob` is a canary by MECHANISM, not by rank** — it keys its type caches directly on
   `reflect.Type` identity, so a field-representation change can break it while a largest-importer
   ranking would never select it.
6. **CNR + the two-seeded three-target `-stdlib` diff by HUNK** for any converter emission change,
   with the footprint stated per target.

---

## 6. Open questions, each with a recommendation

**⟨Q74-1⟩ Is the class "a reference slot anywhere" or "a reference slot the corpus takes the address
of"?** R's control answers the first; only the missing census answers the second, and the second is
what (D) needs. **Recommendation: build the census before anything else, and treat R's eight-variant
control as its positive control.**

**⟨Q74-2⟩ Does (C) need provenance first, as `DESIGN-native-array-view.md`'s §3 did?**
**Recommendation: NO for the inline-layout half** — an inline field has no address ambiguity, which
is the whole point — **but YES the moment a mirror is replaced by an address handed outward**, and
that boundary is stated per increment rather than assumed.

**⟨Q74-3⟩ Does (B) alone unblock any banked row?** Unknown. It restored catchability for the reflect
face; whether the page-allocator rows and the by-address rows report anything more under a named
refusal than they do today is measurable and unmeasured. **Recommendation: measure it as (B)'s
acceptance rather than assuming it, because "the defect stays open and LOUD" is a real outcome and
should not be dressed as a fix.**

**⟨Q74-4⟩ What happens to `[GoValueClone]` under a PARTIAL (C)?** If some structs go inline and some
do not, the attribute's population shrinks but does not vanish, and a half-populated deep-copy
attribute is exactly the shape that goes silently vacuous (route #8). **Recommendation: any (C)
increment states, per struct it changes, that the matching `[GoValueClone]` entry is removed — and
the registry guard asserts the two cannot disagree.**

**⟨Q74-5⟩ Is `array<T>`'s window (`m_low`/`m_length`) load-bearing for an inline representation?**
The window exists so `(*[N]T)(s)` and `(*[N]T)(unsafe.Pointer(&s[i]))` can alias existing storage
rather than snapshot it. An inline field has no backing to window into. **Recommendation: an inline
field and an aliasing view are two representations of one Go type, and any (C) increment must say
which sites get which — this is the question most likely to make (C) larger than it looks.**

---

## 7. What this record does NOT propose

It does not propose a converter change, a golib change, a corpus regeneration, or a retirement of any
existing hand-own. It does not size (C). It does not adopt the coordinator's 540 as a joint figure or
my own retracted sizing sentence. **It fixes the class's boundary at R's measured control rather than
at the symptom that named it, states the population that IS measured with its scope, names the one
that is not, and makes building that census the first gate.**

