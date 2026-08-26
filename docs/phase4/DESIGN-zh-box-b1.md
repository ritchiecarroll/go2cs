# B1 — the box itself: per-kind representation, measured before designed

**Status: DESIGN — awaiting ratification. Nothing here is implemented; no corpus file moves until
the coordinator ratifies (the position-map/crash-report pattern the commissioning ruling names).**

Commissioned 2026-08-26 (the JOB-024 fold ruling, board `7bc998da1`): mechanism B/C — the box
itself — resolving **P-F5** (`unsafe.Pointer`-subclassing vs kind-as-type) in-design, carrying
**P-F2's three-variant microbench as a precondition**, with the **`Reinterpret` source-retention
shape (NetShareAdd)** as a named input and the arc's measured exhibits as the acceptance set.
Parent design: [`DESIGN-zh-box-reduction.md`](DESIGN-zh-box-reduction.md) §4 (Phase B's four
items) and §11 (the panel findings). Two **binding conditions** carried from S0b's ruling text:

- **R1 — every yield claim is priced by EMISSION, never by census counts.** A census figure is a
  constituency bound; S0b measured the gap at two orders of magnitude (560 census verdicts, 5
  emitted boxes). Acceptance below is written in emission and counter terms only.
- **R2 — every corpus measurement carries the byte-identical baseline control.** A seeded A/B
  reconvert whose BASELINE half reproduces the committed corpus byte-for-byte is what makes the
  changed half attributable. S0b's instrument (two roots, one hash on the control side) is the
  required shape; a yield number without its control is not a number.

## 1. The precondition, discharged — the microbench, grown from three variants to five

§4's item 2 left the dispatch mechanism open with a named risk (P-F2): `Value`/`ValueSlot` are
today non-virtual branch chains the JIT folds; subclass-virtual dispatch is an indirect call AOT
cannot devirtualize at sites typed as the base (the hierarchy is open — `unsafe.Pointer` derives
from it). The panel's rule: **the virtual variant lands only if it is ≤ the current form on both
runtimes.**

The bench models five layouts, not three, because building the mandated three exposed two more
worth measuring — a dispatch shape the doc had not considered (V4), and a one-field fix to the
virtual shape's only measured loss (V5):

| | layout | dispatch | takes the tuple bytes | takes the dead-`m_val` bytes |
|:--|:--|:--|:--:|:--:|
| **V1 current** | one class, two nullable tuples | null-test branch chain (transcribed from `ж.cs` field-for-field, branch-for-branch) | — | — |
| **V2 flattened-switch** | one class, plain fields | byte kind + switch | ✔ | ✘ — a field of type `T` cannot be conditionally present, so every kind still carries `sizeof(T)` inline |
| **V3 subclass-virtual** | per-kind sealed subclasses | virtual `Value`/`ValueSlot` (and a virtual nil probe) | ✔ | ✔ |
| **V4 kindbyte-downcast** | per-kind sealed subclasses | byte kind in the base + `Unsafe.As` unchecked downcast (no isinst, no virtual call) | ✔ | ✔ |
| **V5 landing** | V3's storage | virtual accessors, but `m_isNull` is a NON-virtual base field, so `DerefOrNull` pays one indirect call, not two | ✔ | ✔ |

Workloads per P-F2 — standard-box-dominant (`Value` read/write and `DerefOrNull`), a field-ref
hop, mixed-kind at the 90/8/1.5/0.5 ratio through the base type, and the reinterpret/native
case. Protocol per P-F4 — same machine (GRETCHEN-LAPTOP, Ryzen 5 PRO 6650U), interleaved rounds,
N = 12, medians; JIT (CoreCLR 10.0.11, warmed, PGO) and Native AOT published from the same
source. Noise threshold, stated from the harness's own behavior: cross-**process** medians of the
same variant swing up to ~8 %, within-run interleaved medians are stable to ~1–3 % — so
comparisons are within-run only, and a ratio within **±3 %** of 1.00 is read as parity.

**Time (ns/op, medians of 12; ratio vs V1):**

| workload · JIT | V1 | V2 | V3 | V4 | **V5** |
|:--|--:|--:|--:|--:|--:|
| std `Value` (rw) | 1.626 | 5.524 (3.40×) | 0.944 (0.58×) | 2.509 (1.54×) | **0.944 (0.58×)** |
| std `DerefOrNull` | 2.064 | 1.381 (0.67×) | 2.075 (1.01×) | 1.606 (0.78×) | **1.150 (0.56×)** |
| fieldRef `Value` | 1.604 | 1.396 (0.87×) | 1.609 (1.00×) | 2.301 (1.43×) | **1.605 (1.00×)** |
| mixed 90/8/1.5/.5 | 3.360 | 3.316 (0.99×) | 2.174 (0.65×) | 2.969 (0.88×) | **2.053 (0.61×)** |
| native `Value` | 0.686 | 2.058 (3.00×) | 0.230 (0.34×) | 1.829 (2.67×) | **0.230 (0.33×)** |

| workload · Native AOT | V1 | V2 | V3 | V4 | **V5** |
|:--|--:|--:|--:|--:|--:|
| std `Value` (rw) | 4.621 | 4.637 (1.00×) | 3.933 (0.85×) | 4.635 (1.00×) | **3.942 (0.85×)** |
| std `DerefOrNull` | 2.772 | 2.777 (1.00×) | 3.239 (1.17×) | 2.781 (1.00×) | **1.866 (0.67×)** |
| fieldRef `Value` | 2.082 | 2.314 (1.11×) | 1.858 (0.89×) | 2.313 (1.11×) | **1.844 (0.89×)** |
| mixed 90/8/1.5/.5 | 4.272 | 5.183 (1.21×) | 4.310 (1.01×) | 5.079 (1.19×) | **4.332 (1.01×)** |
| native `Value` | 1.856 | 1.854 (1.00×) | 1.618 (0.87×) | 1.848 (1.00×) | **1.613 (0.87×)** |

**Bytes per box (identical on both runtimes; standard kind includes its pinnable slot):**

| kind / T | V1 | V2 | V3 | V4 | **V5** |
|:--|--:|--:|--:|--:|--:|
| standard, `long` | 144 | 112 | 80 | 80 | **80** |
| standard, 560 B struct | 672 | 640 | 608 | 608 | **608** |
| fieldRef, `long` | 112 | 80 | 40 | 48 | **48** |
| fieldRef, 560 B struct | **672** | 640 | 40 | 48 | **48 (−93 %)** |
| elemRef, `long` | 112 | 80 | 32 | 40 | **40** |
| native, 560 B struct | 672 | 640 | 40 | 48 | **48** |

**The verdict, and the panel's risk inverted.** V5 is ≤ V1 on **every row of both runtimes**
(the one 1.01× is inside the stated ±3 % band), and is *faster* on most — JIT 0.33–1.00×, AOT
0.67–1.01×. P-F2's fear was that an indirect call would lose to a foldable branch chain; the
measurement says the opposite, and says why: the current merged getter is a LARGE method whose
branch chain resists inlining, while the per-kind bodies are two or three instructions — on JIT,
PGO's guarded devirtualization specializes the skewed sites; on AOT, the indirect-call predictor
handles them and the tiny bodies win on code quality. The two non-virtual alternatives are
eliminated by the same table: V2 (the §4 "alternative that takes the bytes without the
indirection") takes almost none of the bytes and is 3× slower on the hottest JIT rows; V4 gets
V5's bytes but its byte-load + jump-table + downcast chain loses to the virtual call on both
runtimes. V3's only loss (AOT `DerefOrNull` +17 %) is its virtual nil probe — two indirect calls
where V5 pays one — which is exactly the fix V5 is.

Model caveats, stated: V1 is a transcription, not the linked golib (absolute ns are not corpus
ns; the DECISION is relative, model-vs-model, which controls everything except layout and
dispatch); the mixed workload is one call site seeing four types, the corpus is thousands of
mostly monomorphic sites (JIT specializes per-site; AOT's worst case is the mixed row, measured
at parity); the managed-`T` nil-peek chain (`s_valueCanBeNull`/`HeldValueIsNull`) is not
modeled — it moves into the standard subclass unchanged, where it folds per-instantiation
exactly as today. Probe source and both raw outputs:
[`probes/b1-box-dispatch/`](probes/b1-box-dispatch/) — a point-in-time record, not a gate; B2's
acceptance re-measures on the real golib (§6), not on the model.

## 2. The landing shape

`ж<T>` becomes the **abstract base** — it keeps the NAME, so every typed position in the corpus
(`ж<T>` parameters, fields, locals: the entire emitted surface) is unchanged — carrying the one
field every kind owns and the whole public surface:

```
public abstract class ж<T> : IPointer<T>, IEquatable<ж<T>>, INilPointer
    m_isNull                      (non-virtual bool — DerefOrNull's fast path, V5's one fix)
    abstract Value / ValueSlot
    equality, hashing, PointerOrderToken, operators, of()/at() minting, ToString —
    the existing public surface, unchanged signatures, virtuals staying virtual

public class StandardBox<T> : ж<T>        (UNSEALED — §3)
    m_val; m_slot (the pinnable T[1], eager, unchanged doctrine); m_pin;
    EnsureStableAddress / pinnedArrayData move here (they are standard-kind machinery)

public sealed class FieldRefBox<T> : ж<T>
    m_source (object); m_accessor (FieldRefFunc<T>); m_token (Delegate — identity, unchanged)

public sealed class ElemRefBox<T> : ж<T>
    m_backing (T[]); m_index (nint)       — §5, the typed element-ref path

public sealed class NativeBox<T> : ж<T>
    m_nativeAddr (nuint); m_pin; m_retainedSource (object? — §4, the NetShareAdd shape)
```

§4-item-1 (flatten the nullable tuples) stops being an item — there are no tuples left to
flatten; the ~28 B they cost is inside the per-kind table above. The four-kind partition is
exactly today's four disjoint cases; no fifth kind is introduced.

## 3. P-F5, resolved: kind-as-type, and `unsafe.Pointer` derives from the standard kind

**The census** (this worktree, master `7394d6076`): `unsafe.Pointer` is the **only** subclass of
`ж<T>` in the corpus, and it is constructed at **six sites, all in `unsafe.cs`, all
standard-kind** — the `uintptr`-value ctor and the nil ctor; nothing anywhere mints a Pointer
carrying field-ref, element-ref or native kind. (The go/types `Pointer` and the generated
`syscall.Pointer` in the same grep are different types — a Go type object and a TypeGenerator
wrapper over `ж<EmptyStruct>` respectively.) Kind is per-instance data today only in the sense
that the FIELDS admit it; no construction path exercises it for Pointer.

**Therefore: `Pointer : StandardBox<uintptr>`.** Its `base(value, value == 0)` ctor maps onto
the standard ctor with the nil mark; its overrides — `PointerOrderToken`, `Equals`,
`GetHashCode`, the address-is-identity doctrine — stay overrides of base virtuals exactly as
today. The two named teeth are answered by the same fact:

- *Perf teeth (syscall-heavy packages):* a Pointer-typed call site dispatches exactly as today —
  the virtuals it overrides are already virtual, and its `Value` is the standard slot path V5
  measured at 0.85×/0.58×. Nothing on the uintptr round-trip gains an indirection it does not
  already have.
- *Correctness teeth (reflect bridge):* the bridge reads boxes through the base surface (`Value`,
  `PointerOrderToken`, equality) and already special-cases the concrete-class shape
  (`GoReflect.cs`'s "@unsafe.Pointer is a CONCRETE class `: ж<uintptr>`" arm) — subclass-of-a-
  subclass changes neither test. `RecvGenerator`'s `this ж<T>` twins accept any derived instance
  by ordinary subsumption.

One consequence is load-bearing: **`StandardBox<T>` cannot be sealed** (Pointer derives from its
`uintptr` instantiation, and sealing is not per-instantiation). The other three kinds seal. The
bench's V5Standard was sealed; at base-typed call sites — which is all of them — sealing a leaf
does not change the emitted dispatch, so the measurement carries.

## 4. The `Reinterpret` source-retention shape (the NetShareAdd input)

The board's NetShareAdd entry names the durable remedy this design must carry: when
`Reinterpret<TFrom, TTo>` refuses to alias (a reference-bearing struct viewed as `byte`), the
fallback `(ж<byte>)(uintptr)box` mints a NATIVE-kind box whose managed identity is **gone** — a
hand-owned wrapper receiving it has nothing left to copy from, which is why `NetShareAdd` is a
declared capability limit today rather than a blittable-mirror copy.

`NativeBox<T>.m_retainedSource` is that remedy's storage: the non-aliasing fallback populates it
with the source box, and a hand-owned wrapper recovers the struct by an ordinary typed read —
`(box as NativeBox<byte>)?.RetainedSource as ж<SHARE_INFO_2>` — making the established
field-for-field boundary copy reachable with **no fabrication of managed references from a raw
address** (the rejected remedy 1) and no change for any other native box: the field is null for
every native box a kernel handed us, and it adds one reference slot to a kind measured at 48 B.
Retention also pins nothing and roots only what the caller's own frame already rooted — the
address's validity window is unchanged (it remains the pin's, exactly as documented on `m_pin`).
Un-displacing the `NetShareAdd` hand-own is then B2 work with its own gate (`os`'s
`TestNetworkSymbolicLink` on a Server-service host, per the board entry's probe A oracle).

## 5. The typed element-ref path (§4 item 4, unchanged in intent, now with its layout)

`ElemRefBox<T>` holds `(T[] m_backing, nint m_index)` directly. The `Ꮡ(s, i)` /
`Ꮡ(arr, i)` overloads take `slice<T>`/`array<T>` and extract the backing — deleting the
`IArray<T>` interface boxing of the header, which is the census's **one caller-side charge:
−1 counted object per `&s[i]` corpus-wide**. Canonicalization (`CanonicalElement`) becomes
construction-time normalization of `(backing, index)`, per the parent design's accepted trade.
The bench's elemRef row (112 → 40 B) understates this kind's win: it excludes the header box the
typed path deletes at the call site.

## 6. Blast radius and acceptance — priced by emission, gated by the instrument

**Blast radius of the abstract base, measured:** `new ж<T>(…)` appears at **344 sites** —
**310 converter-regenerable** (emission change + deliberate corpus regen, proven by R2's A/B
instrument: baseline byte-identical, then the regen diff is the change), **22 in golib** and
**12 in four hand-owned files** (`internal/abi/type_impl.cs`, `os/darwin/dir_darwin_impl.cs`,
`time/tick.cs`, `unsafe/unsafe.cs`) — 34 hand edits, in-arc. Behavioral goldens re-baseline
mechanically (`UpdateTestTargets`). The TypeGenerator's `syscall.Pointer` emission constructs
`ж<EmptyStruct>` (2 generated sites) and follows the same rename. The safety property is the
compiler's: **an abstract base turns every missed site into CS0144 at build time** — the failure
mode is a build error, never a wrong-kind box, the same never-silent property §4.2's selection
table was designed around.

**Acceptance set (B2's gates, stated now):** the four ruling exhibits, measured on the real
golib with the pipeline's own instruments, each with its R2 control —

| exhibit | current (provenance) | B1's claim |
|:--|:--|:--|
| `os.File.WriteString` | **17.00 obj/op** (golib counter, 3 runs, 2026-08-26) | objects: −1 per `&s[i]` site in the path, else unchanged; **bytes/op down** (field-ref/native kinds shed dead `m_val`); the counter's per-site asserts move deliberately with the census updates |
| `math/big` `TestMulUnbalanced` | **59× vs the 10× bound** (R, 2× at the new pins, 0.07 % apart) | bytes down where kind-slimming reaches `nat`'s box traffic; the ratio's decomposition probe rides this arc (the +15.9 % hop delta is still unattributed and is B1's to decompose, not to assume) |
| `net/netip` gradient | 49 want-zero rows reading 1–10; 5 want-one; `Addr.String()` IPv6 at 106 | counts: only the `&s[i]` term moves (R1 — no count claim beyond it); the gradient re-measures as the do-no-harm control |
| nistec four curves | P224 8,484 · P256 8,528 · P384 12,572 · P521 17,090 obj/run (A3) | counts within the element-ref delta; **must not regress** — plus the four `Perf*` pointer-family benchmarks and `PerfRefLower`, within the P-F4 protocol |

plus the byte instruments (`ж<FD>`-class boxes via the os probe's box line; the per-kind size
table above re-measured on real types), `TestAllocations` rows moving only by the element-ref
term, and the standing wall-clock protocol (P-F3/P-F4). Count-neutrality **except the
element-ref row** is R1's own statement of §4's claim — and it is a *claim to verify by
emission*, not a property to assert: the S0b lesson applies to this design's own numbers.

## 7. Adversarial self-review

1. *"The model lied — real golib will disagree."* Possible in absolute terms, controlled in
   relative ones: the decision rule compares layouts under identical harness, and B2's gates
   re-measure every exhibit on the real thing before anything banks. The one structural
   difference (managed-`T` nil peek) lives in the standard subclass on both sides of the compare.
2. *"An open hierarchy invites a fifth kind nobody priced."* The base's ctor is
   internal-protected to golib + the declared `unsafe.Pointer` seam; kinds are a closed set by
   construction review, not by `sealed` alone.
3. *"Retained sources root large object graphs."* Only the reinterpret-fallback path populates
   the field; kernel-returned native boxes carry null. The retained box was live in the caller's
   frame at mint time anyway; the delta is lifetime extension to the box's own, which is the
   documented pin lifetime already.
4. *"344 sites is a regen, and regens have burned us."* Hence R2 as a binding condition: the
   emission change lands only through the seeded A/B whose baseline reproduces the committed
   corpus byte-identically, with CNR and the full ladder over it — and the abstract base makes
   any missed site a compile error rather than drift.
5. *"DerefOrNull's base-field read is a hidden semantic change."* It is today's semantics moved,
   not changed: `IsNilStandardPointer` already answers `m_isNull` only; non-standard kinds carry
   `false` in the base exactly as they answer today.

## 8. Open questions for ratification

- **OQ-1 — kind-class naming and minting spelling.** Recommendation: base keeps `ж<T>` (zero
  typed-position churn); subclasses `StandardBox/FieldRefBox/ElemRefBox/NativeBox<T>`; the 310
  emitted `new ж<T>(…)` become `new StandardBox<T>(…)` via the emission template (mechanical),
  golib's `Ꮡ`/`heap`/`of`/`at` mint the right kind internally as today.
- **OQ-2 — does B2 (implementation) ride one lane or split golib-first?** Recommendation: one
  lane, golib + emission + regen together — the abstract base does not compile against the old
  emission, so a split would hold a broken intermediate state.
- **OQ-3 — the probe record's home.** Recommendation: `docs/phase4/probes/b1-box-dispatch/`
  (source + both raw outputs), a point-in-time record like RECON/DATA files, not built by any
  gate — struck or kept at the coordinator's preference.

---

*Inputs: the JOB-024 fold ruling (board `7bc998da1`); DESIGN-zh-box-reduction §4/§5/§11
(P-F2/P-F4/P-F5); the NetShareAdd board entry and its remedy-3 lineage; S0b's measured
emission-vs-census gap and its two riders; the five-variant microbench, this machine,
CoreCLR/NativeAOT 10.0.11, 2026-08-26; construction censuses over master `7394d6076`.*
