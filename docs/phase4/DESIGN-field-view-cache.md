# DESIGN — the field-view cache: one `FieldRefBox` per (box, field) for the box's life, type-gated

> **Status: design record for a ruled mechanism, not a cut (COORD ruling 2026-09-05, mailbox
> `f1083bff5`).** The seg-3 sizing (`fb5e64a45`) censused the population, a three-arm golib SPIKE
> measured the candidates side by side (`b831004e6`), and COORD ruled **arm 3 — the type-gated view
> slot, marking as built** — to a design first, then a cut with its own guard. Every number below was
> measured on G-LAPTOP (go1.23.12, .NET 10.0.400, Release, `DOTNET_TieredCompilation=0`) on
> 2026-09-04/05 against master `dde657009` (train 26, which does NOT carry B2's lowering); the spike
> branch `claude/g-seg3-spike` is the instrument and is not banked.

## 0. The one-paragraph version

Every call of the shape `recv.field.Method()` whose callee takes a `ж<FieldT>` receiver mints a
`FieldRefBox` at the call — `Ꮡf.of(File.Ꮡpfd).Write(b)` — 64 B and one counted object per call, plus
the accessor-wrapper weak-table lookup the typed `of()` overload already pays. The census found
**965 receiver-base call sites and 218 parameter-base sites in 52 packages** paying that box, after
the ref-primary machinery had already freed 1,023 of the 2,036 shape sites; the os row's seg 3 is one
of them and the ref-primary route cannot reach it (the callee, `FD.Write`, is unpromotable by the
identity-boundary cascade the I1 hold measured). The mechanism: the box's `of()` returns **one cached
view per (box, accessor token), minted on the first call and reused for the box's life** — identity
preserved by construction, because `FieldRefBox` equality already IS (source, token). The cache lives
in a slot on boxes whose pointee TYPE is a consumer (a `SlottedStandardBox<T>` minted when
`BoxShape<T>.Slotted` is set), with a per-`T` weak table catching every box that has no slot; the
byte cost is +8 B per consumer-type box and +0 on every other box, the per-call cost 20.2 ns against
33.8 today, and the os row reads −128 B / −2 objects on this base with all three arms on prediction.

## 1. The population, by structure (from the sizing `fb5e64a45`)

Two derivations, reconciled site by site on `os` (29 / 26, the three explained) and over std:

| | count | source |
|:--|--:|:--|
| shape sites, Go side — own pointer receiver, struct-typed field, pointer-receiver callee, not deferred, not in a literal | **1,878** in 75 packages | `fieldcallcensus` (go/types, explicit and implicit embedded-promotion shapes) |
| the second box-minting cell — a named non-struct field (array or scalar with pointer methods) | **158** | same |
| of those 2,036: **freed** — the emission carries the direct call because the callee already has a `ref` primary | **1,023** | `seg3recon`, forward |
| **boxed** | **892** sites → 908 boxes (16 second hops) | forward |
| displaced (placeholders 7, hand-own files 88, the skip-listed `testing` 21) | 116 | forward |
| unexplained, named | 5 | forward |
| receiver-base boxes in the emission (windows flavour, chains counted per hop) | **965** in 48 packages | `g-seg3-emit-scan.sh` and the reverse walk, agreeing to the digit |
| parameter-base boxes (the same box, a pointer PARAMETER as the base) | **218** | the scanner's second cell |

The top ten packages hold 766 of the 965 (runtime 326, net 84, crypto/internal/edwards25519 71,
net/http 58, database/sql 54, crypto/tls 52, go/internal/gcimporter 41, net/internal/socktest 28,
expvar 26, os 26). The population is concentrated, which is what makes the type gate the right
placement of the cost.

**Chains.** A method promoted through an embedded field OF a struct field emits two hops —
`Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex).Lock()` — two boxes per call; 16 such sites (crypto/tls's
`halfConn` family). Each hop is an `of()` call on the previous hop's result, so each caches on its own
source: the outer view on the box, the inner view on the outer view.

## 2. The contract — identity, stronger than today's

`FieldRefBox<T>.Equals` is (same source, `m_token.Equals`) and its hash is the source's — the
address-keyed semaphores in the hand-owned `sync`/`internal/poll` implementations key on exactly that
(`ж.FieldRefBox.cs`'s own remarks; `DESIGN-zh-box-three-capabilities.md` §2.3/§6). Today two calls
mint two boxes that compare EQUAL; under the cache two calls return the SAME object. Every consumer
that keyed on equality keeps working and every consumer that keyed on reference identity starts
working. Nothing about `Value`, `ValueSlot`, nil semantics or the pin changes: the view is the same
`FieldRefBox` class, minted by the same constructor, once.

**The nil box never caches.** `NilBox` is a shared static per `T`; a view cached on it would be
retained process-wide and would alias nothing. `of()` on a nil box mints as it does today.

## 3. The mechanism, as built and measured (spike `claude/g-seg3-spike @ 7050417a5`)

`ж<T>.of()` — both overloads — becomes: find a cached view by accessor token; on a miss, mint the
`FieldRefBox` (resolving the typed overload's wrapper only there), publish it by compare-exchange onto
an immutable linked list of `ViewEntry { Token, View, Next }`, and return it. Readers never lock; a
racing publisher that cached the same token is found on the re-scan and shared.

**Where the list lives — the gate.**
- `SlottedStandardBox<T> : StandardBox<T>` carries one `ViewEntry?` field (+8 B). `Ꮡ<T>(in T)` mints
  it instead of `StandardBox<T>` when `BoxShape<T>.Slotted` is true.
- `BoxShape<T>.Slotted` is set at type init by a `[GoBoxViews]` attribute on `T` if present, and
  **flipped lazily by the first `of()` on any `ж<T>`** otherwise.
- Every box without a slot — a `StandardBox<T>` minted before its type flipped, an `ElemRefBox`, a
  `NativeBox`, and a `FieldRefBox` (a chain's inner hop) — uses the per-`T`
  `ConditionalWeakTable<ж<T>, ViewTable>` fallback: the same list, one weak lookup per call.

**The non-determinism, named (COORD's condition).** Correctness is identical on both paths — the
same view object comes back from the slot or from the weak table — and only the COST PLACEMENT varies
across a process's early life: a `File` minted before the first `.of(File.Ꮡpfd)` anywhere in the process
pays a weak-table lookup per call for its lifetime; a `File` minted after pays a slot read. A row that
mints its hot boxes before their type flips reads the weak table's per-call cost (37.0 ns) where a
steady-state row reads the slot's (20.2 ns). The deterministic form — the converter emitting
`[GoBoxViews]` on every consumer type from the census predicate (one attribute per type in a corpus
footprint over the 48 packages' types, cross-package by construction since 691 of the 892 boxed
sites call into another package) — is NOT built here; it is built only if a row shows the early boxes
mattering, per the ruling.

**One refinement the cut carries, predicted here:** the slot goes on `FieldRefBox<T>` as well as on
`SlottedStandardBox<T>`. A view exists only because an `of()` happened, so a slot on it costs 8 B per
distinct (box, field) pair — not per box — and it takes a chain's inner hop off the weak table. The
guard's chain arm measures it: predicted per-call cost of the inner hop = the slot's, not the table's.

## 4. Cost, in both units

**Per call (the hot loop: 10,000,000 `Ꮡx.of(T.ᏑF).M()` on an empty callee, 3 windows, floor of 2–3):**

| arm | ns / call | B / call | objects / call |
|:--|--:|--:|--:|
| today | 33.81 | 64.00 | 1.00 |
| slot on every box (arm 1) | 18.03 | 0 | 0 |
| weak table only (arm 2) | 37.00 | 0 | 0 |
| **type-gated slot (arm 3, ruled)** | **20.21** | **0** | **0** |

Today's 33.8 ns is the allocation plus the accessor-wrapper weak-table lookup; the cache removes both
and leaves one short list walk (arm 1), plus a type test (arm 3). The weak table's +3.2 ns over today
is what a second weak lookup costs once the first and the allocation are gone — real, and paid at
every site forever for no byte saved, which is why it is the fallback and not the mechanism.

**Per box:** +8 B on every box of a consumer type minted after the flip (or of an attributed type);
+0 on every other box. Per view: +8 B with §3's refinement.

**Per row — the formula, and two predictions:** a row's byte delta under this design is
**−64 B and −1 counted object per `.of()` mint on the measured path, +8 B per consumer-type box
allocated per op.**

- **The os row (`TestWriteStringAlloc`, SUB-Q32's protocol).** Two `.of()` mints on the path at
  `dde657009` (seg 3 `Ꮡf.of(File.Ꮡpfd)` and seg 61 `Ꮡfd.of(FD.Ꮡl)`), zero consumer-type boxes per op
  (the three per-op boxes are `ElemRefBox<byte>` ×2 and `StandardBox<uint32>`): predicted
  744.25 − 128 + 0 = **616.25 B / 6 objects — measured 616.2500 / 6.0000**, the control at +64.00.
  On B2's base (train 27), where seg 61's mint is lowered away with its delegates, the same formula
  gives **552.25 − 64 = 488.25 B / 6** — the acceptance reading, taken after the rebase.
- **A consumer-heavy row — `net`'s `TestTCPReadWriteAllocs` (disclosed alloc-profile).** Its consumer
  boxes (`conn`, `netFD`) are minted per CONNECTION, not per read/write op, and its per-op boxes are
  element and owning boxes of non-consumer types: predicted **+0 B per op**, and **+16 B per
  `Dial`/`Accept`** (two consumer boxes per connection; the `FD` inside `netFD` is a field, its view
  cached once per connection at −64 B per subsequent `.of()` call). The row's per-op count falls by
  one for each `.of()` mint on its read/write path; the number is censused before the cut, not
  guessed here. Falsifier: a per-op byte delta that is not a multiple of 8 above −64·k.

**Canaries measured at the spike, all three arms within spread:** nistec 134 / 135 / 136 / 137 s
(cold warm-up 166 s), all PASS 2,195; `PerfTlsHandshake` 2,659.8 / 2,557.3 / 2,642.0 / 2,610.0 ms in
no consistent order — a per-record lookup of a few nanoseconds is invisible under a handshake, as
predicted before the run.

**Retention, the cost that is not per call:** a parent keeps one view per distinct field asked of
it for its lifetime (the parent → view → parent cycle collects together). On the os row one `File`
holds two.

## 5. The guard (GolibTests, the cut's own)

1. **Identity across calls:** two `of()` calls with the same accessor on one box return the SAME
   object (`ReferenceEquals`) and compare equal; different fields give different views; different
   boxes give different views for the same field.
2. **The +8/+0 split by type:** the allocated bytes of `Ꮡ(new T())` after `T` has flipped read
   exactly 8 more than before the flip for a consumer type, and unchanged for a type that never sees
   an `of()` (a `[GoBoxViews]`-attributed type reads the slotted size from its first mint).
3. **The chain:** `Ꮡa.of(A.Ꮡb).of(B.Ꮡc)` returns the same inner view across calls, and the inner
   hop's per-call cost reads the slot's, not the table's (§3's refinement).
4. **The fallback:** a box minted BEFORE its type's first `of()` still returns one stable view per
   field (the weak table), and a box minted after carries the slot (the type test).
5. **The nil box:** `of()` on a nil box mints fresh each time and retains nothing.
6. **Negative arm:** with the cache disabled (a test hook, not a build symbol), arm 1's identity
   assertion goes RED naming itself; restored, green.
7. **Acceptance:** the os row on B2's base under SUB-Q32's protocol — **488.25 B / 6 objects**,
   anything else the falsifier — beside the earlier table's slot and weak-table columns for the
   record (512.25 / 488.25).

Gates beyond the guard, per the golib rules: `go2cs.slnx` Debug `--no-incremental` (a golib API
change), GolibTests count-matched in both configurations, the stdlib solution on three flavours, the
full behavioral suite, the nistec cost canary as an A/B, CNR (no emission change is predicted — the
converter is untouched — proven by an unfiltered status).

## 6. What this design does NOT do

- **Delegates.** The two coupled defer delegates on the os row (segments 5 and 62) are not `.of()`
  mints; B2's lowering removes them on train 27.
- **The os row's remaining six objects** after this design are segments 1 and 11 (element box +
  companion, the element-address publish gate) and segment 10 (owning box + pinnable slot) — the
  next capability, censused before it is sized (COORD `f1083bff5`).
- **The converter.** No emission changes. The deterministic marking (§3) is the only converter work
  this design could ever ask for, and it is gated on a measured need.

## 7. The spike, for the record

Branch `claude/g-seg3-spike` (one golib commit, three arms behind compile symbols selected by
`-p:GoViews=slot|cwt|gated`, unbanked). Predictions on record at `ecf5e9277` (on B2's base) and
re-based at `44e7fedda` when the warm-up arm read the os row at 744.25 — the I1 floor — because the
spike ran on `dde657009`, a base without B2. Readings, every arm's +64.00 control fired first:

| arm | os row B / obj | hot loop ns / B / obj | nistec | TLS C# ms |
|:--|--:|--:|--:|--:|
| PRE | 744.25 / 8 | 33.81 / 64 / 1 | 134 s | 2,659.8 |
| slot | 640.25 / 6 (N = 3 confirmed: the companions are pin objects) | 18.03 / 0 / 0 | 135 s | 2,557.3 |
| weak table | 616.25 / 6 | 37.00 / 0 / 0 | 136 s | 2,642.0 |
| type-gated | 616.25 / 6 | 20.21 / 0 / 0 | 137 s | 2,610.0 |

Scored: every os-row and hot-loop prediction held except the weak table's per-call cost, predicted
20–60 ns over today and read +3.2 (the favourable miss, explained above). Script defect, stated: the
chain ran PRE twice (a cold warm-up, then the reading) into the same per-arm logs, so the warm-up's
TLS median was overwritten and the TLS column has no independent PRE pair; the re-run numbers each
arm's logs.

## 8. Nothing-throwaway

The cut is the spike's arm 3 with the other two arms and their symbols deleted, the slot added to
`FieldRefBox`, and the guard of §5; the spike branch is the measurement and stays as such.

## 9. AMENDMENT 2026-09-05 — the cut MEASURED (seated train 29 as `GFVC`, `a5d40fdfc`), and the residue it leaves

**Nothing above is rewritten.** The cut landed as §3's arm 3 with the type gate (`golib/ж.Views.cs`; a `SlottedStandardBox<T>` minted by `Ꮡ<T>()` / `@new<T>()` once `BoxShape<T>.Slotted`, the slot also on `FieldRefBox<T>` for chain hops, the per-T weak table for every other kind, the nil box never caching, immutable list nodes published by compare-exchange). What was measured on this box (Release, `DOTNET_TieredCompilation=0`, 3 × 1,000,000 runs, floor of windows), scored against §4's prediction:

| row | base (golib at `bc8973259`, train 27's master) | cut | prediction |
|:--|:--|:--|:--|
| `os` want-zero row (`TestWriteStringAlloc` shape) | 552.25 B / 7 obj | **488.25 B / 6 obj** | **MET to the byte and the object** (−64 B / −1 obj, the per-op `FieldRefBox`) |
| +64 B positive control (`new byte[40]`) | 616.25 / 7 | 552.25 / 6 | bytes MET; the OBJECT prediction (8 / 7) was WRONG on both arms — `new byte[40]` is not a golib site, so the counter never charges it — owned, not smoothed |
| hot loop (`Ꮡrecv.of(f).M()` per call, the seg-3 shape) | 33.99 ns / 64 B / 1 obj | **20.44 ns / 0 B / 0 obj** | MET (§7's arm 3 read 20.2) |
| `crypto/internal/nistec` cost canary | PASS 2195 [163 s cold] | PASS 2195 [85 s warm re-run; 140 s like-for-like] | count MET; walls stated as walls, the train's alternating-arm pair on a quiet box is the cost measurement of record |

Gates at the seat: GolibTests 630/3/6/639 Debug and 633/3/3/639 Release+TC0, count-matched, the three the box's identity-matched symlink-privilege trio; `go2cs.slnx` 0 errors / 760 s; stdlib windows / linux / darwin 0 errors; CNR byte-identical across 715, 0 NOT MEASURED; the full behavioral suite PASS 679 (Output 653 / 26 skip). Two things the guard caught before the announce: a real publish-by-CAS race (the first `publishView` scanned for a racer's entry only AFTER a failed CAS; fixed to read the head once, scan that list, CAS against that same head), and an EXISTING counting arm whose counted call was the second `of()` on one box, which the cache answers for free — re-pinned on the constructor row, a repeat-`of()` arm added at 0 objects / 0 B (see `DESIGN-allocation-counting.md`'s dated amendment). One instrument breach, mine, stated: the chain's cleanup glob deleted nistec's tracked disclosure manifest after the master arm, so the cut arm swept without it and read a phantom red on a disclosed want-zero row; restored, re-run solo with the manifest present.

**The residue, located by arithmetic and confirmed by the next cut.** SUB-Q5's per-frame table at the pre-B base and the cut's reading agree to the byte: the six remaining objects are segments 1 and 32 (the two element-address sites, an `ElemRefBox` plus the caller's `IArray<T>` boxing temp each, 120 B / 2 obj) and segment 14 (`heap(new uint32(), out var Ꮡdone)`, the owning box plus its eager pinnable slot, 88 B / 2 obj); the two PIN segments (56 + 104 B) carry bytes and no counted object. Candidate A — the concrete-header element takes `Ꮡ<T>(slice<T>, int)` / `Ꮡ<T>(array<T>, int)` and their `nint` twins, no boxing temp — was cut and measured the same night: **488.25 / 6 → 376.25 / 4, exactly as predicted** (seated train 29 as `GA`, behind this seat). What remains after A is segment 14 (candidate B, the syscall family: 88 / 52 / 91 variables by flavour, its design priced against Q49's pin machinery) and the two element boxes themselves (E, a pinned element address for a syscall argument without a box, after Q49; C, the `unsafe.Slice(unsafe.StringData(s), n)` idiom, population two, last). The bank condition needs all of them; each step's number is on the mailbox beside its prediction.

**The lazy-flip non-determinism §3 named** was not observed to matter on any measured row; the converter-emitted `[GoBoxViews]` attribute stays unbuilt until a row shows early-minted boxes paying the weak-table lookup.

