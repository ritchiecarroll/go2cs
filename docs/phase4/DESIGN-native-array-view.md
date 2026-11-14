# DESIGN — the native-backed `array<T>`: closing the raw-metal fork that fabricates a managed reference

**STATUS: RATIFIED (coordinator, 2026-08-23).** Rulings: **(1)-as-symmetry RATIFIED** — the slice design’s rejection of a second type transfers verbatim, and the m_array census (one hot path, one unsatisfiable escape hatch, one rider) makes array<T> the family’s cheapest conversion; (2) withdrawn by its own proposer on that evidence; (3) stays the constant-index peephole. **The §4 SAFETY FLOOR was ratified floor-first, then WITHDRAWN AS SPECIFIED (coordinator, 2026-08-23, on lane R’s measured disproof)**: a type-tested panic at the raw fork fails 6/609 behavioral — three legitimate native-fork uses measured (pinned-managed round-trips, pointer-shaped T, container-shape over pinned storage), and even narrowed to the defect’s own `array<U>` shape it fires on Go-legal `*(*[2]uintptr)(p)` over pinned managed storage. **The discriminator is the ADDRESS’S PROVENANCE — pinned-managed and genuinely-native addresses arrive indistinguishable in `m_nativeAddr` — and no test on T can separate them.** The withdrawal’s ruling: (i) floor-first ordering is REVOKED; (ii) a PROVENANCE design amendment is the arc’s next increment (extend the `ManagedPointerTokens` record to pinned-managed addresses; hot-path registration cost MEASURED per the counting doctrine; must also reconcile the slice dual-mode’s handling of the same ambiguity, and it answers ⟨OQ-2⟩ properly); (iii) **§3’s emission work HOLDS until provenance rules** — building the native mode over an ambiguous address field would rebuild the class one level down. Cost of the disproof: one suite run and an A/B — cheap for retiring a ratified increment before it shipped a corpus-wide false red. **⟨OQ-6⟩: panic by name** — a silent copy is the snapshot class’s foothold, the slice arc’s founding lesson; the array-typed subset census is owed before it can fire, as stated. **⟨OQ-4⟩ ANSWERED EARLY, from layout (R, mailbox, 2026-08-23), superseding the scheduled run**: `internal/poll` declares 116 bytes to the kernel over a 40-byte managed `RawSockaddrAny` of reference fields — a completed Windows UDP read overflows the managed object by 76 bytes into the GC heap, and `acceptOne` carries the identical shape. **§4.8 proceeds as ratified — its staging is what recv needs — and this design does NOT unblock recv**: fixing only the byte-view decode would read plausible values off corrupted heap, turning a loud failure silent. The two arcs are independent and parallel. ⟨OQ-G⟩’s send staging stands regardless on the documented contract. **⟨OQ-2⟩: the one-lifetime-story route is ratified WITH an escape-audit obligation** — the native door covers the short-lived in-helper views the corpus exhibits; any site whose view ESCAPES its function is named at implementation as its own question rather than silently pinned indefinitely (pinned-object-heap hostility is the hazard the audit guards). G corrects the record if the slice numbers were leaned on further than they reach; silence by G’s next entry is concurrence.** Originally:** PROPOSED (lane R, 2026-08-23).** Commissioned by the coordinator (mailbox, 2026-08-23)
after this lane measured `(ж<array<T>>)(uintptr)` producing a fabricated managed reference:
*"the DESIGN is yours, the fix waits for it… weighing your (2) against (1)-as-symmetry — the
RATIFIED native-backed `slice<T>` dual-mode is this family's precedent and already MEASURED the
hot-path branch cost that is (1)'s objection, so pull that arc's evidence in rather than
re-arguing it."* Sibling and precedent:
[`DESIGN-native-backed-slice.md`](DESIGN-native-backed-slice.md) (RATIFIED 2026-08-22, lane G).
§7 collects the open questions.

---

## 1. The measured bill

### 1.1 The defect, measured in both regimes

`array<T>` is a MANAGED struct whose first field is a `T[]` reference (`array.cs:46`,
`Backing => m_array ?? []`), and a native-backed `ж<T>` materialises its value with
`Unsafe.AsRef<T>((void*)m_nativeAddr)` (`ж.cs:250`). Composed, `(ж<array<byte>>)(uintptr)(…)`
**reinterprets whatever bytes live at that address as a managed array reference and dereferences
it.** Measured in GolibTests against golib directly — no kernel, no socket, no async:

| memory at the address | result | reading |
|:--|:--|:--|
| zeroed | `Length=0` — the reference reads null, `?? []` yields the empty array | a SILENT WRONG ANSWER |
| filled `0xAB` | `Length=-1414812757` — i.e. `0xABABABAB` | it **fabricated a managed reference out of the data bytes and dereferenced it**, returning a number instead of faulting BY LUCK |

The second row is the finding. This is a type-safety hole, not a wrong result: any real data — a
filled sockaddr, a `siginfo`, a register block — takes that path, and the CLR is handed a pointer
the program invented.

### 1.2 It is a KNOWN fork, and that is the useful part

golib says so itself, in `array<T>.AliasPointer`'s remarks:

> *"A pointer with no managed element storage behind it — a heap box, a struct field, a native
> address — keeps the raw-address route: no `T[]` exists to window, and an `array<T>` can neither
> view native memory nor be fabricated from a scalar's bytes. **That is the raw-metal fork,
> unchanged here.**"*

And the converter says so, in `arrayPointerAliasEmission`'s header, naming the exact mechanism:

> *"the address route… builds a native-backed `ж<array<T>>` whose deref reads an `array<T>` STRUCT
> (a backing reference plus bounds) out of the pointed-at DATA, i.e. **a fabricated managed
> reference**"*

**So nothing here is newly discovered except the SEVERITY.** The fork was documented as
*unsupported*; the measurement shows it is *unsafe* — it does not decline, it invents a reference.
That distinction is this design's whole reason to exist, and it is why the safety floor (§4) is
separable from the representation fix (§3).

### 1.3 Half the class is already solved, and by the shape this design should copy

`arrayPointerAliasEmission` already handles `(*[N]T)(unsafe.Pointer(p))` **when `p`'s element type
equals `T`**, emitting `array<T>.AliasPointer(p, N)` — carrying the length, windowing real managed
storage, and falling back at RUNTIME when the pointer has no element storage. The gap is precisely
the **differently-typed** conversion, where the target array's element type is not the source
pointer's:

```go
p := (*[2]byte)(unsafe.Pointer(&pp.Port))   // Port is uint16; target elem is byte
```

`types.Identical(srcPtr.Elem(), targetArr.Elem())` is false, the alias arm declines, and the raw
route runs. **The length is NOT the blocker** — I said it was in my first report and that was
wrong: Go's `*[2]byte` carries N in the type and the converter already has it (`csNintLiteral`
exists to render exactly this). The blocker is that `array<T>` has nowhere to put an ADDRESS.

### 1.4 Census: 61 sites

`(ж<array<…>>)(uintptr)` across `src/core`:

| package | sites |
|:--|--:|
| `runtime` | 17 |
| `runtime/linux` | 10 |
| `syscall/darwin` | 8 |
| `syscall/linux` | 6 |
| `runtime/darwin` | 5 |
| `internal/poll/windows` | 4 |
| `runtime/windows` | 3 |
| `reflect` | 2 |
| `net/darwin` | 2 |
| `internal/syscall/windows/registry/windows` | 2 |
| `vendor/…/route/darwin`, `vendor/…/sha3` | 1 each |

**AMENDED 2026-09-16 (C1, q100) — the 61 above is a 2026-08-23 number and the hop added nine sites.
Re-taken at `claude/version-go1.24.13` `46307b4704`, and the predicate is stated because the obvious
one is wrong:**

```
  PREDICATE   the text `(ж<array<` … `>)(uintptr)` where the `>` closing the OUTER `ж<` is found by
              DEPTH MATCHING, never a character class — the element type can itself be
              `ж<array<uint16>>` (syscall/windows/zsyscall_windows.cs:1694), which a `[^>]*`
              predicate cuts in half
  READING     79 textual sites in 43 files · 9 inside a `//` comment · 70 CODE SITES
```

| package dir | code sites |   | package dir | code sites |
|:--|--:|---|:--|--:|
| `runtime` | 16 | | `runtime/linux` | 16 |
| `runtime/darwin` | 9 | | `runtime/windows` | 7 |
| `syscall/linux` | 5 | | `internal/runtime/maps` | 4 |
| `syscall/darwin` | 4 | | `crypto/internal/fips140/nistec` | 2 |
| `net/darwin` | 2 | | `crypto/internal/fips140/sha3` | 1 |
| `golib` | 1 | | `internal/reflectlite` | 1 |
| `reflect` | 1 | | `syscall/windows` | 1 |

**And the axis §4's withdrawal turns on — what feeds the `uintptr`:**

```
   25  A   PINNED-MANAGED — `@unsafe.Pointer.FromPinnedBox(…)`, e.g. syscall/darwin/syscall_unix.cs:335
           `(ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(pp.of(RawSockaddrInet4.ᏑPort)))`
           ⚠ this IS lane R's counter-example class, and it is 36% of the population
   15  B   a value cast to `@unsafe.Pointer` (`(@unsafe.Pointer)gp.sigpc`, `(@unsafe.Pointer)pc`)
   30  C   everything else — `sysAllocOS(…)`, `atomic.Loadp(…)`, `funcdata(…)`, bare locals
```

⚠ The instrument's own first run read **0** sites — the depth counter started after consuming the
`<` it was meant to be balancing — and was caught only because a cruder `grep` had read 79. A zero
that disagrees with a cruder instrument is the instrument's fault until proven otherwise.

### 1.5 The liveness audit — latent, with a live trigger

The raw count reads alarmingly (35 in `runtime`); the audit says otherwise, and the honest framing
matters more than the number.

The `runtime` sites sit in `mapaccess1_faststr`, `mapaccess2_faststr`, `c64hash`, `c128hash`,
`memequal128`, `readUnaligned32/64`, `initAlgAES`, `cheaprand`, `runfinq`, `printArgs`, `pkgPath` —
**the hottest paths in Go's runtime. If they were live and broken, nothing would run at all.** They
are converted-but-inert: golib implements maps (`map<K,V>`) and hashing natively, so `map_faststr.cs`
and most of `alg.cs` are faithful conversions nothing calls. Reachability spot-check: `memequal128`
**0** call sites, `mapaccess1_faststr` **1** (inside `runtime` itself), `readUnaligned64` **2**.

**The empirical argument is the strong one.** A zeroed-memory hit panics immediately and visibly —
exactly how the netpoll recv presented — and the roster is green at 146 packages / 18,569 verdicts.
No live path on today's roster reaches any of these sites.

So the class's true shape is **latent, with a live trigger**:

- 61 sites are wrong-by-construction and dormant because nothing reaches them.
- A site goes live the moment a new code path is reached — which is what the Phase-4 campaign does,
  continuously and by design.
- **The netpoll recv is arrival number one**, and it arrived because this lane's own §4.7 send fix
  un-hid it — the `LocalTimeZone` pattern one level down.
- Each future arrival presents as a plausible panic several layers from the cause. That
  re-diagnosis cost, not today's blast radius, is the argument for fixing rather than routing
  around.

`cheaprand` (`rand.cs:219`, 32 in-`runtime` callers) is the one site I would not certify by
inspection alone; that the corpus runs is the evidence it is unreached.

---

## 2. What the precedent already settles

`DESIGN-native-backed-slice.md` is the same problem one container over, and it is RATIFIED. Three of
its rulings transfer without re-argument:

1. **The dual-mode shape.** `ж<T>` carries `m_nativeAddr` beside its managed modes; `slice<T>` gained
   `m_nativeBase` with `0` as the discriminant. `array<T>` is the third member of the family and the
   only one still forked.
2. **A separate native type was REJECTED, for reasons that apply verbatim here.** §3 of that design:
   *"The converter emits `slice<T>` at every Go `[]T` site; a second type either forks the emitted
   surface (converter + corpus churn across everything) or hides behind an interface (boxing golib's
   hottest type). The polymorphism must live INSIDE the struct."* Substitute `array<T>`/`[N]T` and
   the sentence is unchanged. **This overturns my own first recommendation** — I proposed a distinct
   `NativeArray<T>` before reading the sibling arc, and I withdraw it (§6).
3. **The unmanaged-`T` precondition.** `RuntimeHelpers.IsReferenceOrContainsReferences<T>()` is a
   JIT-time constant, so the guard compiles away on every managed construction and folds to a
   constant on the native door. It states the SiginfoChild class as a precondition instead of
   discovering it as heap corruption — and here it is doubly apt, since fabricating a reference is
   the exact failure being closed.

What does NOT transfer: the slice arc's OQ-4 (*"converter involvement: none expected"*). This
design **requires** a converter emission change, because the native door needs a length the current
raw-cast emission drops (§3.2).

---

## 3. The design

### 3.1 `array<T>` grows a native backing, symmetric with its two siblings

```
internal readonly T[]  m_array;       // managed backing (empty when native)
private  readonly int  m_low;         // window start — INDEX for managed, ELEMENT OFFSET for native
private  readonly int  m_length;
private  readonly nuint m_nativeBase; // NEW: 0 = managed; else the base address
```

One added word. `m_nativeBase == 0` is the discriminant; element `i` lives at
`m_nativeBase + (m_low + i) * sizeof(T)`. The existing window fields already exist for exactly this
family of aliasing conversions (`array.cs:48–55`), so the native mode reuses them rather than adding
a second addressing scheme.

**Only unmanaged `T`**, enforced at the creation door with a named panic, per §2(3).

### 3.2 The creation door, and the converter change it needs

One door, mirroring `AliasPointer`:

```csharp
// Go: (*[N]T)(unsafe.Pointer(p)) where p's element type is NOT T.
public static ж<array<T>> ViewPointer(ж<TSrc>? p, nint length);
```

`arrayPointerAliasEmission` currently declines when
`!types.Identical(srcPtr.Elem(), targetArr.Elem())` and lets the raw cast run. It should instead
emit the view form, with the length it already computes (`csNintLiteral` is written for this). The
same-element-type arm is unchanged: `AliasPointer` stays the better answer there because it can
window real managed storage.

**Runtime disposition inside the door**, in this order:

1. The pointer addresses **managed element storage** of the same type → window it, as `AliasPointer`
   does today.
2. The pointer addresses **managed storage of a different type** (our case: a `uint16` field viewed
   as `[2]byte`) → this is the interesting one, and §7's ⟨OQ-2⟩ asks how far to go. The bytes are
   real and managed; a `Span<byte>` over the pinned field is faithful, and the pin must outlive the
   view.
3. The pointer is **genuinely native** → native-backed `array<T>` over the address.
4. `T` is managed, or the length is negative → **named panic**, never a fabricated reference.

### 3.3 Operation semantics

The slice design's table applies with one container's worth of translation; the short version:
indexer reads/writes go through the computed address, `len` is `m_length`, `Ꮡ(a, i)` yields a
native-backed `ж<T>` at the element address (the existing `ж(nuint)` ctor), bulk paths go
span-first via an `AsSpan()` that pays the discriminant once per operation rather than per element
(§2.4 of the sibling design), and `Clone()`/conversions **copy out** — as they already do — because
Go's array assignment is a copy by contract.

`Reinterpret`-style members whose math is `sizeof`-scaled keep working over a byte base. Any member
reaching `m_array` directly must take the span path or panic named; §6 makes that census a gate,
exactly as the sibling arc did.

---

## 4. The safety floor — separable, and landable before the representation

> **AMENDED 2026-09-16 (C1, q100). ⚠ READ THE STATUS BLOCK BEFORE THIS HEADING.** The section as
> written below was ratified floor-first and then **WITHDRAWN AS SPECIFIED** at master `8f7cf67cc`,
> on lane R's measured disproof (6 of 609 behavioral), with floor-first ordering REVOKED. The text
> below is kept unchanged because its *motivation* is unrefuted — the measured `Length=-1414812757`
> stands — but its *specification*, a type-tested panic, does not.
>
> **What LANDED instead, ruled by the coordinator on C1's hold:** a **PROVENANCE-tested** refusal,
> scoped to `array<U>` element types, as the FIFTH arm of `ж<T>`'s `uintptr` operator
> (`golib/ж.cs`), between the null-resolution census count and the `NativeBox` fall-through:
> *resolution null AND `T` is `array<…>`* → `RuntimeErrorPanic.NativeArrayViewWithoutElementStorage`,
> the tenth named panic beside the nine. `array.cs`'s `AliasPointer` fallback funnels through the
> same operator and needs no change of its own; a test names that site as reached.
>
> The discriminator R named — the address's provenance — did not exist when this section was
> withdrawn. [`DESIGN-pointer-provenance.md`](DESIGN-pointer-provenance.md) (RATIFIED) supplies it,
> its mechanism has landed, and `ManagedPointerTokens.Resolve` is already the operator's first act.
> A pinned-managed address therefore resolves and is ADMITTED, which is what keeps R's six green;
> a genuinely-native one carries no record and is refused by name.
>
> ⚠ A citation to a section heading is a citation to its STATUS block first. The coordinator's own
> q100 routing quoted this heading's "separable, and landable before the representation" without
> reading two paragraphs up, and the cut stopped on it. Banked as a class.
>
> **AMENDED 2026-09-19 (C1) — THE BOUND, stated because the arms do not state it.** What landed
> refuses the **no-provenance class**, not "fabricated array views" in general. The condition is
> *resolution null AND `T` is `array<…>`*, so an address that resolves to a **LIVE box of a
> different pointee type**, at an `array<U>` pointee, still falls through to `NativeBox`
> **UNREFUSED**. That is by design and is what "IT CURES NOTHING" means concretely here;
> `NativeArrayViewFloorTests` arm 3 exercises that shape but asserts `IsNotNull` only, so the bound
> is a property of the specification rather than an accident of the arms. Widening to cover the
> live-box case would be a separate cut with its own measurement, not a tightening of this one.
> (Coordinator finding (a) on the q100 gate, mailbox `1135d780c`; C2's second-lane read `a810502ff`
> reached the same bound from the code side and observed that `ж.cs`'s own "ONE BOUND" paragraph
> stated the WEAK-ENTRY bound and not this one. Both now state it, and the code paragraph reads
> "TWO BOUNDS".)
>
> ⚠ Recorded beside it, because it is the maintenance hazard this bound creates: the
> `resolved is null` conjunct is **load-bearing**, and the arm's comment as cut implied it was not.
> Arms 2 and 4 in `ж.cs` are `Q44RegistryCensus.Enabled`-gated counters that divert no control flow
> and do not execute at all on the production path, so *reaching* the floor does not imply *firing*
> it. Simplifying the condition to `if (s_isArrayShaped)` is precisely the type-tested floor
> withdrawn above at 6 of 609 red; arm 3 is the regression test that would catch the edit.


**Whatever is ruled for §3, the raw-address route must stop fabricating references.** Today it
silently invents one; a named panic converts a memory-safety hole into a diagnostic that names its
own cause, at the cost of turning latent-silent into latent-loud.

This is worth stating as its own tier because:

- It is **small and self-contained** — a guard where the native-backed `ж<array<T>>` materialises.
- It is **strictly safer** than today under every reading, and it cannot regress a working path:
  by §1.5 no live path reaches these sites, so nothing on the roster can start panicking.
- It makes every FUTURE arrival cheap to diagnose. The netpoll recv cost this lane a full
  misattribution — through a design, a ratification and four documents — because the failure
  presented as a plausible panic in unrelated code. A named panic at the fork would have ended that
  in one line.
- It is honest about what the corpus supports. The project's own doctrine is that *a stub that
  compiles is an acceptable milestone solution*; a fork that panics by name is the same principle
  applied to a hazard.

Recommended message shape: name the conversion, the element type and the fact that the pointer has
no managed element storage — enough that the reader reaches this document rather than the panic
site.

#### Amendment 2026-09-20 (C1) — ⚠ THE BOUND ON THIS FLOOR: place it at the shared terminal and it panics every converted program at startup

**The second bullet above — *"it cannot regress a working path: by §1.5 no live path reaches these
sites"* — is true of the fork's OWN population and false of the line the fork ends on.** Both are
measured; the distinction is the whole of this amendment.

`ж.cs`'s `uintptr → ж<T>` operator has four arms and **arms 2 and 4 share one terminal**, the single
`return new NativeBox<T>(...)`:

```
  arm 1   resolved is ж<T>                    aliased return          safe
  arm 2   resolved is a LIVE box, pointee != T  falls past the refusal (offset 0 is not arithmetic)
                                              -> the SHARED TERMINAL   "Counted here; not yet changed"
  arm 3   IsTokenArithmetic                    throws by name
  arm 4   resolved is null                     -> the SHARED TERMINAL   <- the raw-metal fork §4 is about
```

§1.5's liveness audit is an audit of **arm 4's** population. A guard written to §4's own wording —
*"where the native-backed `ж<array<T>>` materialises"* — lands on the terminal and therefore fires on
**arm 2 as well**, whose population is different and is **not** latent:

```
  the 25 resolved-different-pointee (arm 2) array sites, at the version tip d91c832543
    BOUND-LIVE (package init)  2   crypto/internal/fips140/nistec  (retired by the P-256 decode)
    RUNTIME-RESIDENT           4   runtime.initAlgAES · runtime.cheaprand · runtime.pkgPath ·
                                   reflect.pkgPath
    flavor-gated · linkname-edge · inert   19
```

⚠ **`initAlgAES` is called by `alginit()`, called from `schedinit` under Go's own comment "maps, hash,
rand must not be used before this call".** That is **program startup, every converted program, every
banked row, always** — and the site is arm 2 by construction, not by inference: `Ꮡaeskeysched` is a
`ж<array<byte>>` and the conversion's destination is `ж<array<uint64>>`, so `resolved is ж<T>` cannot
hold. `cheaprand` (18 callers across `sema`, `proc`, `mbitmap`, `iface`) and both `pkgPath` sites
(interface conversion) are the same shape.

**So the floor must be scoped, and the scope is PROVENANCE, not the destination TYPE:**

- **Scope it to arm 4** (`resolved is null`) — the classification is already in hand at that point and
  costs nothing, per the 2026-09-08 note that `resolved` must not be re-queried.
- **Never scope it by testing the destination type.** The `slice<T>` twin census reached the same
  discriminator independently and from the other side: of its three CODE sites the only LIVE one
  (`runtime/symtab.cs`, 19 callers) is provenance-BEARING — a type-tested floor refuses it, a
  provenance-tested one admits it. R's 6-of-609 reading is the third arrival at the same rule.

**Neither §3 nor §4's recommendation changes**; what changes is where the guard may be placed. A floor
at the shared terminal is not "strictly safer under every reading" — it is a startup panic.

<!-- Derivation, 2026-09-20 (C1), COORD 453af64e3 / 74be45f73. Arms read at `src/core/golib/ж.cs`
     (master 4d25779a1a): the four-arm operator with arm 2's own comment "Counted here; not yet
     changed" and the single `new NativeBox<T>` terminal. Site classification at the version tip
     d91c832543: `runtime/alg.cs:541` inside `initAlgAES`, whose callers are `alg.cs:526,530` in
     `alginit()`, called from `proc.cs:848`. `Ꮡaeskeysched` declared `ж<array<byte>>` at
     `alg.cs:535` (master numbering; the file shifts between trees, so both are cited). Liveness
     instrument: the census's own predicates called rather than copied, with the census output
     byte-compared before and after the hoist; caller counts carry a second raw derivation whose
     delta reconciles to the declaration plus one comment. The §4 floor is NOT implemented at
     master — measured: no guard at the terminal, and `array.cs:201-204` still describes the
     raw-metal fork as "unchanged here". -->

##### Note 2026-09-20 (C2's read `ebd9553dc`) — the step the amendment above ASSERTS, the citation it gets wrong, and a widening

Three additions from an independent derivation of the same bound. **None changes the conclusion**; the
first two make it rest on a mechanism rather than on a type comparison, and the third strengthens the
remedy.

**1. `RegisterPinned` is what makes the site arm 2.** The amendment argues `initAlgAES` is arm 2 from
the types — the box is `ж<array<byte>>`, the destination `ж<array<uint64>>`, so `resolved is ж<T>`
cannot hold. **True only if `resolved` is non-null at all**, which is a property of the FORWARD
conversion, not of the declaration:

```
  FromPinnedBox(Ꮡaeskeysched)   ->  the ж -> uintptr operator: value is IArray and not ISlice
                                ->  dataAddr = pinnedArrayData(arr)
                                ->  ManagedPointerTokens.RegisterPinned(dataAddr, value)   <-- HERE
  the reverse conversion        ->  Resolve(dataAddr) answers the LIVE ж<array<byte>> box
                                ->  not ж<array<uint64>>  ->  arm 1 fails  ->  ARM 2
```

**Without that registration `Resolve` answers null and the same site is ARM 4** — the population §1.5
audits — and the bound would collapse. The registration is what makes the type mismatch decisive.

**2. ⚠ The `proc.cs` citation above cannot be opened at the path it names.** Under layout L3 that file
is three per-GOOS copies. The call is at **`src/core/runtime/{linux,windows,darwin}/proc.cs:848`, in
all three**, each carrying Go's own comment — so the claim is *stronger* than the citation stated
(startup on every target, not one), and a reader following the single path gets "does not exist".

**3. A WIDENING: §4's phrase does not designate a line.** *"Where the native-backed `ж<array<T>>`
materialises"* resolves to **eleven** `new NativeBox` construction sites in `src/core` (seven further
hits are pure comment), of which `ж.cs`'s operator terminal is one. An implementer following the
phrase could land on any of them. **That makes the remedy stronger, not weaker: scope by PROVENANCE —
arm 4, the classification already in hand — and never by LOCATION, which is under-determined here.**

<!-- C2's read of 6e082a09c5 at mailbox ebd9553dc: the four arms and the shared terminal derived from
     ж.cs before reading C1's description, so a second derivation rather than a check; the forward
     trace through unsafe.cs FromPinnedBox into the ж -> uintptr operator's RegisterPinned call; the
     eleven-vs-seven split measured over src/core with a positive control (the terminal read by eye)
     and a negative control (a fabricated type name, 0). Every line number in the amendment above was
     confirmed correct AT THE TREE IT NAMES, including the deliberate two-tree numbering; proc.cs was
     the one path that resolves nowhere, and is corrected here rather than by rewriting the block
     above, which is append-only. Ruled additive by COORD at 791bd267d. -->

#### Amendment 2026-11-14 (P5) — the same floor for a NAMED array type, in two layers, and the nil pointer

**The floor above is keyed on `array<E>`. A named array type is a different C# type and was not covered.**
Go `type smallMem [64]byte` is emitted as a struct that go2cs-gen completes: one field, a managed reference
to a holder of the `array<byte>`, filled on first use. It is an `IArray`, it is not `array<>`, and it holds
a reference. A native box of that struct reads the struct at the address, so the bytes at the address are
taken for the reference.

Measured at master `c2919b8b16` (linux/amd64, Go 1.26.11, Release), before this amendment's code:

| Arrival | Go | Converted at master |
|:--|:--|:--|
| `(*smallMem)(p)`, `p` zeroed C memory, then an index store | stores the byte | no error; the first eight bytes of the C memory become the address of a managed array |
| `(*RawMem)(p)` over zeroed C memory, `RawMem` longer than a .NET array can be | a pointer | `OverflowException` in the lazy fill |
| the same over C memory holding data | a pointer | `NullReferenceException`: the data is read as a reference |
| `(*smallMem)(unsafe.Pointer(&x))`, `x` a Go array of another type, a slice element or a struct field | a view of `x` | no error in 5 shapes of 7; the program's write is lost and a managed address is stored into `x` |
| `(*[N]T)(unsafe.Pointer(nil))` | a nil pointer | refused by this floor |

The first row is the one that matters: a managed reference stored where the collector cannot see it, with
no error. The fourth is the same store into Go storage, through two routes the number conversion does not
decide: an address WITH a provenance record (pinned Go storage of another type, which the cross-type arm
admits), and the `Reinterpret` conversion, which makes its native box without passing the number conversion.

**What landed (branch `claude/p5-named-array-floor`, three cuts, each red first).**

1. **At the number conversion, beside arm 5** (`ж<T>.FromNumber`): a second per-instantiation flag,
   `s_isNamedArrayShaped` (a value type that is an `IArray`, not an `ISlice`, not `array<>`, holding a
   reference), refused under arm 5's own test, `resolved is null`, with arm 5's panic. Never for the number 0.
2. **In the `NativeBox<T>` constructor**: when `T` is that struct and the address is a real one (not 0, not a
   tagged order token), the same panic. This is the layer that covers the fourth row.
3. **The nil pointer**: arm 5 gains `value.Value != 0`. The number 0 at an `array<E>` pointee is a nil
   pointer again, through both doors.

**Why layer 2 does not contradict the 2026-09-20 amendment.** That amendment is a measurement about
`array<E>`: its cross-type boxes are made at startup (`initAlgAES`) and never read as arrays, so a refusal at
the place they are made panics every program. Layer 2 fires for the named-array struct only. For that struct
no value can exist at a real address at all (storage holding a reference is never pinned, and C memory never
held one), so there is no arrival for which the box is right. And nothing makes one on a path a banked row
runs: std has 8 conversions to a named array per target, 3 in `mprof.go` inside the hand-owned `stkbucket`,
4 in `mgcmark_greenteagc.go` in the collector's own scan, 1 in `internal/runtime/maps` `TestMapIndirect`,
which is disclosed and refuses earlier. The falsifier is stated with the cut: any hosted behavioral project
or sweep row failing with this floor's text is a live arrival and is read before anything else.

**What it does not change.** A pointer to a named array in Go memory, converted to a number and back at its
own type, is the same box (arm 1). An order token at a named-array pointee keeps arm 2a's refusal at the
dereference. A slice header is outside the floor. An unnamed array viewed over Go memory of another array
type (length 0) is not touched; it is a BOARD row.

**It is a floor and cures nothing.** The Go-memory shapes that stored an address now panic by name where Go
answers. The conversion that answers them, a box of the named array sharing the Go array it views, is the
array-pointer doors design (`DESIGN-array-pointer-doors.md`, in review on `claude/p5-native-view-design`).

Guards: `NamedArrayViewFloorTests` (12 arms) and `NativeArrayViewFloorTests.ANilPointerAtAnArrayPointeeIsANilPointer`
in GolibTests. Programs, Go's answers, the converted answers before and after, and the predictions with their
scoring: [`data/named-array-floor/`](data/named-array-floor/README.md).

<!-- P5, 2026-11-14. Rulings: COORD 19:35Z (the floor is its own seat, .4, red first, refuse by name keyed on
     the wrapper as arm 5 is keyed on array<>) and 19:58Z (A then B in one seat, B is what lands; the nil
     refusal fixed in the seat as its own red-first commit; made-to-fail for each). Readings, GolibTests,
     whole suite: red commit 61511a266d 2783 total / 2729 passed / 49 skipped / 5 failed (the five arms);
     cut A 9ec25dc665 2783 / 2734 / 49 / 0; red for the Go-memory arrivals 9f96ea25a3, 3 arms red at A; cut B
     6019d6275d 2786 / 2737 / 49 / 0, verdict lists differing from A's by the new file's arms only; red for
     nil d222504127, 1 arm. Made to fail, A: with `resolved is null` dropped from layer 1 exactly one arm goes
     red (the pinned-bound arm then in the file), not two as the plan predicted: arm 1 returns the round
     trip before the line. Design section 4.5 of the doors draft said a named array over Go memory "answers
     what To answers today"; the fourth row above is what To answered, so that sentence is wrong and the
     draft is corrected on its own branch. -->

---

## 5. Rejected alternatives, on the record

- **A distinct `NativeArray<T>` view type (my own first proposal).** Withdrawn on the sibling
  design's §3 reasoning: it forks the emitted surface or boxes golib's hot type. Recorded so it is
  not re-proposed.
- **Pointer arithmetic per site (my option 3).** Only covers sites indexing with constants, and
  several of the 61 pass the view onward. **Rejected as primary, retained as a peephole**: under
  either §3 or §4, a constant-index site could still emit direct offset reads and skip the view
  entirely. Cheapest possible answer for the common `p[0]`/`p[1]` shape.
- **Write-back snapshots.** The sibling design's refutation applies unchanged: there is no "when" to
  write back, and the kernel observes the memory continuously.
- **Do nothing.** Today's blast radius is genuinely NONE (§1.5), so this is defensible on today's
  evidence — and it is what the codebase has done, deliberately, with the fork documented. It stops
  being defensible the moment a second arrival lands, and arrivals are what Phase 4 manufactures.

---

## 6. Gates (spec'd here, run by the implementation lane)

1. **The `m_array` touch census** — every member of `array.cs` and every golib helper reading
   `m_array` via `internal`, enumerated and dispositioned span-path / arithmetic-safe / named-panic
   **before the first commit**. The sibling arc made this its first commit; the SiginfoChild lesson
   applied preventively.

   **Run already, because it sizes the whole proposal** (2026-08-23): **31 `m_array` touches in
   `array.cs`**, and the blast radius is much smaller than the slice arc's — the great majority are
   CONSTRUCTORS (`m_array = …`, `m_length = m_array.Length`), which are the managed path and change
   not at all. Only four places need a real disposition:

   | member | line | disposition |
   |:--|--:|:--|
   | `private T[] Backing => m_array ?? []` | 271 | **THE hot read path** — every indexer, enumerator and bulk helper funnels through it. Becomes the span/discriminant seam; this is where §3.3's "pay the discriminant once per operation" lives. |
   | `public T[] Source => m_array` | 257 | ⚠ **the one member whose contract cannot be honored natively** — it hands out the backing `T[]`, and a native-backed array has none. See ⟨OQ-6⟩. |
   | `Array IArray.Source => m_array` | 604 | the interface form of the same problem, same ruling. |
   | `builtin.GoZero` fill | 419 | writes a fresh managed array but READS `Backing[m_low + i]`, so it rides on `Backing`'s disposition. |

   `Alias(slice<T>, length)` (166) reads `source.m_array` and stays managed-only by construction.
   That is the entire census: **one hot path, one escape hatch (twice), one rider.** For comparison
   the sibling arc's equivalent census drove its whole first commit — `array<T>` is the cheaper
   member of the family to convert, which is itself an argument for (1)-as-symmetry over any new
   type.
2. **A GolibTests family** (`NativeArrayViewTests`): both regimes from §1.1 as explicit assertions
   (zeroed → correct length and contents, not `0`; `0xAB`-filled → real bytes, not a fabricated
   reference); index read/write reaching real memory verified through a second view; the
   differently-typed managed case (`uint16` field viewed as `[2]byte`) reading and WRITING the
   field's real bytes; `Ꮡ(a, i)` address exactness; the unmanaged-`T` named panic; and the
   same-element-type `AliasPointer` path UNCHANGED (regression guard for the half already working).
3. **The §4 floor has its own test** even if §3 lands with it: a pointer with no managed element
   storage panics by name rather than returning anything.
4. **Perf, measured not asserted** — the sibling arc's objection applies to `array<T>` too.
   `PerfSieve`/`PerfString`/`PerfMatMul` A/B at the merge tip within noise, plus a GolibTests
   microbench bound on the managed-path indexer.
   **Precision, from the slice arc's own author (G, mailbox concurrence, 2026-08-23), amending
   what the cited 30% means:** (a) the figure measured an INLINE-BUDGET failure, not a branch
   budget — the native branch and its `unsafe` block placed INLINE cost PerfSieve +30% because
   the indexer stopped being inlinable; the remedy was `[MethodImpl(NoInlining)]` on the
   out-of-line slow path. **The transferable lesson is the SHAPE, not the affordability** — if
   `array<T>`'s indexer takes the branch inline, expect the same 30% and this gate will find
   it. (b) "Within noise" requires a NAMED CONTROL ROW reported beside the treatment rows (the
   control-row-first doctrine): this laptop class measured a +5.0% control / +17% same-binary
   drift floor on 2026-08-23, and an indexer change reaches EVERY benchmark, so the control
   must be a row the change provably does not touch. (c) The 30% was measured on the dead
   perf-canon i9 — it is a RATIO ANCHOR, never a threshold for another host; re-measure paired,
   same session. (d) Where the claim is "no allocation was added", gate by COUNT
   (`AllocationCounter`, deterministic) — a count cannot be swallowed by a 17% noise floor.
5. **The standing envelope**: GolibTests full, behavioral suite full, CNR (a converter emission
   change IS expected here — CNR measures its footprint), Windows stdlib build, `-p:GoTargetOS=linux`
   build, and the i9 control per the usual dispatch.
6. **The §4.8-fate measurement, which this design owes** (see §7 ⟨OQ-4⟩).

---

## 7. Open questions, each with a recommendation

* **⟨OQ-1⟩ — one tier or two?** Land §4's named panic first as its own small change, or only as part
  of §3? *Recommendation:* **two tiers, floor first.** It is small, strictly safer, cannot regress a
  live path, and every day it is in place is a future arrival diagnosed in one line instead of
  through a misattribution. It also de-risks §3 by making the fork loud while the representation is
  being built.
* **⟨OQ-2⟩ — how far does the differently-typed MANAGED case go?** §3.2(2): a `uint16` field viewed
  as `[2]byte` is managed storage the view must both read and WRITE, with a pin outliving the view.
  Options: pin-and-view (faithful, but golib's pinning story for interior fields is the one `ж.cs`
  calls *"a transient address"*), or route it through the native door by taking the pinned address
  (simpler, and the pin becomes the door's problem). *Recommendation:* **the native door**, because
  it gives ONE path to audit for lifetime instead of two — but this is the question I am least
  certain of, and it is where the netpoll recv actually lives.
* **⟨OQ-3⟩ — scope of the first landing.** All 61 sites are affected by the representation, but only
  the emission change decides which SPELL differently. *Recommendation:* **land the representation
  plus the emission change wholesale** (they are one semantic unit) and let CNR measure the corpus
  footprint; do not stage by package.
* **⟨OQ-4⟩ — the §4.8 fate measurement, which this design owes.** With the byte-view fixed, whether
  Windows recv needs §4.8's staging seam is a ONE-RUN answer: re-run `UdpLoopbackRoundTrip` on
  Windows and read whether the decoded sockaddr is correct. Correct ⇒ the kernel does fill a managed
  `ж<RawSockaddrAny>` and §4.8's seam is unnecessary for recv (⟨OQ-G⟩'s send staging stands
  regardless, on the documented contract). Wrong-but-not-crashing ⇒ the struct-passing class is real
  here after all and §4.8 proceeds as ratified. *Recommendation:* **run it in this arc's gate pass
  and report the answer to the §4.8 owner** — it is one run and it retires or confirms a ratified
  design.
* **⟨OQ-6⟩ — what does `Source` do on a native-backed array?** The census (§6(1)) makes this the one
  member whose contract is unsatisfiable: it returns the backing `T[]`, and there is none. Options:
  **materialize a copy** (safe, silent, and wrong for any caller expecting aliasing — the exact
  snapshot bug the slice arc existed to kill), or **panic by name** (loud, honest, and it may fire
  on a path that only wanted to read). *Recommendation:* **panic by name**, on the same reasoning as
  §4 — a native array reaching a `T[]`-shaped consumer is a real mismatch, and silently copying it
  is how the snapshot class got its foothold in the first place. The unfiltered `.Source` reference
  count outside golib is large (582, receiver-type unseparated), so the implementation census owes
  the array-typed subset before this fires.
* **⟨OQ-5⟩ — who implements.** *Recommendation:* **a golib lane with the full envelope**, sized like
  the sibling arc, with §6(1)'s census as the first commit. This lane (R) holds the netpoll context
  and the measurements; either is defensible, and I have no preference beyond not splitting the
  §4.8 fate question away from whoever runs ⟨OQ-4⟩.

---

*Prepared by lane R (`claude/native-array-view-design`), 2026-08-23. For ratification: §3's shape
(the dual-mode array symmetric with `ж<T>`/`slice<T>`, one creation door, the converter emission
change), §4's separable safety floor, and §6's gate ladder. Evidence pulled from
`DESIGN-native-backed-slice.md` per the commission rather than re-argued; lane G cc'd as that
design's owner for the measured branch-cost numbers.*
