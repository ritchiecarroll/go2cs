# ж-box arc — stage A1 census report

> **STATUS: CENSUS — stage A1 of [`DESIGN-zh-box-reduction.md`](DESIGN-zh-box-reduction.md) (§9),
> lane L3.** Analysis only; **zero emission change** (CNR byte-identical is the gate). The verdict
> this report exists to deliver: **the §3.6 projection is CONFIRMED on the recommended branch**
> (§10.3's hoisted-temp rule) — every site class the ~7k-residual projection zeroes is confirmed
> lowered/reverting by static census against the real corpus, the §3.3 emission-shape table covers
> the corpus with **zero** unclassifiable argument shapes at lowered positions, and the per-GOOS
> delta adds **no new layout-L3 memberships**. Findings that reprice details (not the branch) are
> in §6.
>
> ⚠ **Toolchain caveat, stated first:** these numbers were measured on **go1.23.2** (lane
> machine laptop-1); the corpus is pinned to **1.23.1**. Per the standing coordinator ruling,
> laptop re-measures are developmental until the repin — the durable deliverable is the
> INSTRUMENT, and re-derivation on the pinned machine is one command (§1). Every number in this
> report should be re-derived there at merge; deviations from the design's 2026-08-10
> pinned-machine inputs are reported in §5 and never forced into agreement.

## 1. The instrument, and the one-command re-derivation

The classification pass is `src/go2cs/refLoweringAnalysisOperations.go` (the design §8's named
file): a package-scope, whitelist-shaped D1/D1′/D2 vs X1–X5 classifier over every package-level
function's pointer parameters, the two-sided fixed point (§3.2 — forward strips AND caller-side
emittability strips), the §3.3 per-call-site argument-shape census (rows 1–7 / defer-go /
other-veto), and the §3.3 address-taken-local reversion census. It runs in **all three conversion
drivers** (normal, `-tests` on production files only, hand-owned-sibling — §3.5's three-driver
rule), records its verdict on the package context (`packageRefLoweringResult`; nothing
emission-side reads it at A1), and prints a per-package census block under `-debug`.

The corpus-scale instrument is **`-ref-census`** (`src/go2cs/refLoweringCensus.go`): with
`-stdlib`, it loads the standard library **once per `-platforms` target** with full syntax (the
same `std` pattern, `GO111MODULE=off`, GOOS/GOARCH environment and purego-default tags the
`-stdlib` scanner uses), classifies every convert-set package, resolves the Phase-A and A′-world
fixed points globally, cross-references candidates against the hand-owned file set (read-only),
diffs classification across GOOS targets, and writes one JSON report plus a stdout summary. **It
emits nothing, anywhere** — no seeded staging roots exist because no conversion runs; `-go2cspath`
is read only to locate `src/core` for the hand-own scan.

The one-command re-derivation (run from a clone, pinned machine):

```
go2cs -stdlib -ref-census <out.json> -platforms windows/amd64,linux/amd64,darwin/amd64 -go2cspath <repo>\src
```

Wall time on laptop-1: **~10 s for all three targets** (one `std` load each). Unit guards:
`refLoweringAnalysis_test.go` (D/X families, both strip directions of the fixed point, the shape
rows, the locals census, the §3.5 production-only determinism invariant with an `export_test.go`
func-value alias, bodiless/linkname X5).

Rulings honored: §10.1 (Phase-A scope = unexported package-level functions; exported classified
under the one A′ flag), §10.3 (row 5 = hoisted-temp rule; the projection below is priced on it),
§10.4 (nil doctrine — shapes only at A1; row 7 counted), §10.5 (A before B — nothing golib-side
was touched), §10.6 (started on the coordinator's post-harvest go signal).

## 2. The census, per §9 items (a)–(f)

All numbers windows/amd64 (the primary corpus target) unless stated; linux/darwin in §2.6.

### (a) How many params/locals lower

| | corpus-wide | `nistec/fiat` | `edwards25519` | `edwards25519/field` | `nistec` |
|:--|--:|--:|--:|--:|--:|
| package-level funcs | 7,736 | 48 | 18 | 9 | 25 |
| pointer params | 2,819 | 96 | 24 | 10 | 32 |
| **lowered (Phase A)** | **564** | **96 (100 %)** | **22** | 6 | 0 |
| address-taken locals | 2,771 | 158 | 74 | 15 | 20 |
| **locals reverting** | **236** | **150 (95 %)** | 21 | 0 | 0 |

The corpus-wide picture is exactly the design's claim (§3.6: "the count win is concentrated where
the bill is"): 8.5 % of the corpus's address-taken locals revert, but **95 % of fiat's do**, and
the 8 fiat keeps are all in the wrapper file (`Bytes`'s `&out` into the `bytes` *method* —
non-candidate position; `SetBytes`'s `in`, kept by the design's own implicit-address predicate for
`in[:]` — see §6.3). The method-side counts for B′ context: 1,609 method pointer-params
corpus-wide; veto/kept tallies are in the JSON.

### (b) Per-call-site argument shapes (§3.3 rows, at Phase-A-lowered positions)

| Shape | corpus | fiat | edwards25519 | notes |
|:--|--:|--:|--:|:--|
| row 1 `&e.x` field addr | 281 | 56 | 16 | the fiat op-argument class |
| row 2 `&x` local addr | 286 | 154 | 22 | the fiat64 CmovznzU64 feeds |
| row 3 pointer var | 806 | 4 | 1 | corpus-dominant; carries boxes, mints nothing |
| row 4 `&s[i]` elem addr | 17 | — | — | |
| row 5 pointer conversion | 31 | 20 | 7 | see below |
| row 6 temp (composite/new/call) | 11 | — | — | |
| row 7 literal nil | 1 | — | — | 3 on darwin |
| defer-go (boxed carve-out) | 13 | — | — | gob 3, go/parser 5, fmt 2, net/http, runtime, text/template 1 each |
| **other-veto (no emission row)** | **0** | **0** | **0** | **the §3.3 table covers the corpus** |

**Zero `other-veto` shapes on every target** is the completeness proof A1 existed to obtain: no
call-site argument shape at any lowered position falls outside the seven rows plus the defer/go
carve-out, so the caller-side strip of the two-sided fixed point never fired on the real corpus
(it is unit-guarded synthetically). The 31 lowered-position row-5 sites decompose completely:
**20 fiat + 7 edwards25519-scalar + 4 elsewhere** — `crypto/internal/mlkem768.pkeEncrypt#2`
(conv-of-value), `os.newFileStatFromFileIDBothDirInfo#0`/`…FullDirInfo#0` (conv-of-value,
dir_windows.go), and `runtime.semawakeup#0` (conv-chain, lock_sema windows — moot: that function
falls to the declared-in-hand-own X5 arm of §2 c). Sub-shapes to carry into A2:
**`conv-of-address`** (the priced `(*T2)(&v.x)` fiat shape — all 20 fiat sites and 6 of 7
edwards25519 sites) and **`conv-of-value`** (4 sites — the temp rule hoists an already-computed
value; same mechanics, listed so A2 prices them). The remaining syscall-seam `conv-chain` sites
(`WSAIoctl` etc.) target X3-vetoed positions and are inert.

Row-5 count vs the panel's: the panel priced "16 sites across the four curve files" (C-F1); the
census measures **20** in fiat (per curve: `Select` 3 + `ToBytes` 1 + `FromBytes` 1) plus **7** in
edwards25519's scalar family. Favorable deviation — more conversion-shape sites are covered by the
ruled temp rule than were priced.

### (c) Hand-own + linkname cross-references

**Marker census, re-measured** (line-anchored `^\s*\[module:\s*(go\.)?GoManualConversion\]`,
cross-checked against `git grep -P` — identical): **49 marked files, 41 `*_impl.cs` companions, 31
files in both lists, 59 distinct hand-owned files.** The design's §3.5 asserted "44 marked + 26
`*_impl.cs`" (r51b-era); the counts moved with r52–r60 and with framing (the 31-file overlap makes
"marked" and "companion" non-disjoint categories). Full lists in the census JSON.

**Cross-reference of every Phase-A-lowered candidate against same-package hand-owned files**
(word-anchored textual scan; same-package is the complete exposure set — a lowered candidate emits
`internal`, foreign assemblies cannot bind it, and the linkname escape is already X5):
**17 textual hits, resolved per instance:**

| Verdict | Instances |
|:--|:--|
| **REAL — resolve at A2 (4)** | `hash/crc32.castagnoliShift` (declared *and* called in the marked `crc32_amd64.cs`); `hash/crc32.slicingUpdate` (declared in a regenerated file, **called from** the frozen hand-own, line 371); `runtime.getLockRank` + `runtime.lockWithRankMayAcquire` (both called from marked `mfinal.cs` line 112: `lockWithRankMayAcquire(Ꮡfinlock, getLockRank(Ꮡfinlock));`) |
| Comment-only word collisions, dismissed (13) | `runtime.noteclear`/`full`/`open`/`semacreate`/`semawakeup` (doc comments in `runtime2.cs` and the lock impls), `internal/chacha8rand.setup`, `os.newFileStatFromFileFullDirInfo`/`…IDBothDirInfo`, `syscall.copyFindData` (all comment lines) |

**The A2 remedy this resolves to** (small and mechanizable): X5 gains one arm — *"declared in a
file whose emission target is hand-owned"* — checkable at conversion time with the existing
per-file `containsManualConversionMarker` probe (this alone closes `castagnoliShift` and the
whole `semacreate`-class of frozen-declaration functions wholesale, on every GOOS via the L3
routing the probe already follows); plus a curated 3-function called-from-hand-own exclusion
(`slicingUpdate`, `getLockRank`, `lockWithRankMayAcquire`). **Zero hand-own edits are required
for Phase A.** Linkname exclusions ran inside the pass (X5-linkname: 177 params corpus-wide) —
handles scanned per package, the curated forward/push registries consulted both sides.

### (d) Per-GOOS classification deltas (the layout-L3 propagation)

**25 positions across 5 packages differ in Phase-A verdict between targets that declare the
function — and all five packages are ALREADY layout-L3** (`runtime` 14, `net` 7, `os` 2,
`internal/filepathlite` 1, `syscall` 1): **the L3-set delta is ZERO new packages.** The variance
is the expected OS-conditional-body class (`net.fileConn` lowered on windows only,
`os.sameFile` lowered on unix only, runtime's os-layer helpers, …; full table in the JSON). A2's
merge churn from classification is therefore bounded to within-package file movement in packages
that already carry per-GOOS folders — priced, not discovered. Functions present on a single
target were excluded from the delta by definition (platform-exclusive, no new variance).

### (e) Both fiat families

- **`crypto/internal/nistec/fiat` — the flagship holds completely.** All 96 pointer params of
  all 48 package-level functions lower, including every §7 unit-target dependency:
  `p224Sub/Mul/Add/Square` 3+3+3+2 row-1 feeds, `p224CmovznzU64#0` (the class-2 sink),
  `p224Selectznz#0/#2/#3` through row-5 conversions, `p224ToBytes`/`p224FromBytes`. 150 of 158
  address-taken locals revert; the 8 keeps are the `Bytes`/`SetBytes` wrapper sites (§6.3).
  `nistec` itself lowers 0 of 32 — **scope-consistent, not a finding**: its helpers
  (`p224Polynomial`, `p224Sqrt`, …) route everything through `Element` METHODS (X3/forward-
  unlowered), and the projection never claimed them; their `new(P224Element)` traffic is class 3b.
- **`crypto/internal/edwards25519` — the scalar half lowers fully, the field half partially.**
  The main package lowers 22/24 (all of `scalar_fiat.go`'s `fiatScalar*` functions — the second
  fiat family's generated core — plus 7 row-5 conversion sites; the two vetoes are
  `copyFieldElement`: a pointer-slice use `buf[:]` (§6.2) and a method call `v.Bytes()`).
  `edwards25519/field` lowers 6/10: **`feMulGeneric`/`feSquareGeneric` X3-veto on their trailing
  `v.carryPropagate()` method call, stripping `feMul`/`feSquare` through the fixed point.** The
  allocation consequence is ~nil for Phase A's prize — those params carry existing boxes (row-3
  traffic, no per-call mints) — but it means the field family's hot path keeps the boxed
  convention until B′ (methods) or until `carryPropagate` is reconsidered. edwards25519's
  `TestAllocations` residual (109 objects, r60) decomposes as Element-METHOD field-ref traffic +
  class 3b — **B′/Phase-C territory, not Phase A's**, consistent with the design's no-banked-rows
  honesty.

### (f) The exported-candidate classification (§10.1's A′ decision input)

The strict A′ flag (exported package-level function, ≥1 param lowered under the A′-world global
fixed point): **64 functions — with a corpus-wide call-site traffic of just 69 argument records**
(top: `unicode.Is` 9, `internal/runtime/atomic.LoadAcq` 8, `image/internal/imageutil.DrawYCbCr`
6; everything else ≤ 6). A′ widens lowered positions 564 → 632 (+68).

Against the design's 2026-08-10 measurement (347: 115 `internal/*`, 80 `sync`, 46 `syscall`, 26
constructor-shaped/X2) — **reported, not reconciled**, at three strictness levels:

| Level | total | `internal/*` | `sync` | `syscall` | other |
|:--|--:|--:|--:|--:|--:|
| L0: exported, ≥1 pointer param | 444 | 122 | 39 | 104 | 179 |
| L1: minus func-level X5 (bodiless/linkname/func-value) | 354 | 78 | 0 | 104 | 172 |
| **L2: strict A′ candidates** | **64** | **24** | **0** | **6** | **34** |
| exported with an X2-return (constructor-shaped) veto | 3 | | | | |

L1's total (354) sits near the design's 347, and the design's own annotations forecast the drops
my stricter levels apply (`sync` "largely the hand-owned atomic surface" → all func-vetoed at L1;
`syscall` "X3-dominated" → 104 → 6 at L2). The `constructor-shaped` bucket differs sharply (3 vs
26) — likely an instrument-definition difference (mine counts X2-*return* vetoes on exported
functions; the design's 26 may include store-shaped X2). Toolchain (1.23.2 vs 1.23.1) and
instrument definitions both contribute; **re-derive on the pinned machine before any A′ decision.**
On these numbers the census's input to the checkpoint is plain: **the A′ prize is small** (+68
positions, 69 traffic records, no bucket concentration worth an arc).

### §9 (a)–(f) per-target roll-up

| | windows/amd64 | linux/amd64 | darwin/amd64 |
|:--|--:|--:|--:|
| packages / funcs | 304 / 7,736 | 302 / 7,774 | 303 / 8,028 |
| pointer params / lowered A / A′ | 2,819 / 564 / 632 | 2,532 / 558 / 623 | 2,590 / 568 / 634 |
| locals address-taken / revert | 2,771 / 236 | 2,687 / 233 | 2,783 / 237 |
| exported candidates (A′) | 64 | 61 | 62 |
| other-veto shapes at lowered positions | 0 | 0 | 0 |

## 3. The §3.6 projection, re-derived from the real corpus

A static census cannot re-run r56d's dynamic counters; the honest re-derivation is per-class
**static coverage of the sites that generate the dynamic bill**, priced against r56d's measured
per-run counts (P256):

| Class | dynamic bill (r56d) | census coverage | verdict |
|:--|--:|:--|:--|
| address-taken locals (box + slot) | 153,026 | fiat64's CmovznzU64-fed locals revert (150/158 fiat-wide; the generated op locals ALL revert — the 8 keeps are wrapper-file sites whose phases are ≤ 1 % of the bill each) | **→ ~0** ✓ |
| field-ref boxes at plain lowered positions | ~46,506 | all 56 fiat row-1 + 154 row-2 sites sit at lowered positions (`Sub/Mul/Add/Square` arg feeds, `&tmp`/`&in` wrapper feeds) | **→ ~0** ✓ |
| conversion-shape boxes (3a) | ~34,560 | all 20 fiat row-5 sites classify `conv-of-address` at lowered positions; §10.3's temp rule applies to every one | **→ ~0** ✓ |
| `@new<T>()` (3b) | ~3,600 | nistec's point helpers unlowered by scope; `new(…)` sites are row-6/X2 — untouched, as designed | unchanged ✓ |
| `array<T>` backings (4) | 3,386 | untouched | unchanged ✓ |
| **projected residual** | | | **~7,000 (−97 %) — the recommended branch** |

**BRANCH VERDICT: projection CONFIRMED; §10.3's hoisted-temp branch stands.** No argument-shape
class is bigger than priced (row 5 measured *larger* in coverage, 27 vs 16, in the favorable
direction), no unclassifiable shape exists at any lowered position, and the veto branch's
re-opening condition (a materially-below census) did not occur. The §7 unit-target statics all
hold: every parameter of `Mul`/`Add`/`Sub`/`Square`/`Select`'s call chains lowers, `Select`'s
conversion feeds classify row 5, and `CmovznzU64`'s class-2 sink lowers. One honest amendment to
§7's table: the `SetBytes` probe's residual will include `in`'s kept box (the `in[:]` implicit
address — §6.3) and the `Bytes` probe's `out` box (method position) — small named terms, not
projection movement.

## 4. Gate results (A1 gates per §9)

Recorded by the lane at commit time — see the lane report; the coordinator re-gates at merge:

- Converter `go test ./...` from `src/go2cs` — includes `projitemsIntegrity_test.go` (the three
  new files registered) and the new classification unit guards.
- `check-no-regression.ps1` — byte-identical (the pass changes no emission anywhere; the census
  never writes).

## 5. Deviations from the design's measured inputs (reported, never forced)

1. **Toolchain**: census on go1.23.2, corpus pinned 1.23.1 — every number developmental until the
   pinned re-derivation (§1's command).
2. **347-split** (§2 f): strict A′ = 64; nearest instrument-level L1 = 354 vs 347; buckets differ
   with forecastable causes; constructor-shaped 3 vs 26 (definition difference suspected).
3. **Row-5 site count**: 27 lowered-position sites (20 fiat + 7 edwards25519) vs the panel's 16 —
   favorable (more coverage than priced).
4. **Hand-own census**: 49 marked / 41 `*_impl.cs` / 59 distinct vs the design's "44 + 26" —
   r52–r60 growth plus overlap framing; per CLAUDE.md the count is re-measured, never carried.

## 6. Findings the design did not anticipate (A2 pricing notes; none re-open the branch)

1. **`§3.2` text vs `§3.3` carve-out on defer/go, resolved toward §3.3**: §3.2's parenthetical
   "(or sits in a defer/go statement)" reads as a position-strip; §3.3 (and guard 3) make
   defer/go sites *boxed carve-outs of a still-lowered callee*. Implemented per §3.3 — defer/go
   never strips the callee — with the parameter-side mirror the design implies but does not state:
   an address-carrying use (D1′/D2) of the CALLER's own candidate parameter inside a defer/go
   argument frame vetoes that parameter (`X2-defer-arg` — the thunk stores a box a lowered
   parameter no longer has). 13 defer-go sites exist at lowered positions corpus-wide; the
   carve-out is load-bearing exactly there.
2. **The pointer-slice shape `p[:]`** (19 params corpus-wide, `edwards25519.copyFieldElement` the
   fiat-family instance): not a §3.2 D row, so it vetoes — under its own tag
   (`other-use-ptr-slice`) so A2 can decide whether a D-row for it is worth having. It never
   solely blocks a projection-relevant site.
3. **`SetBytes`'s `in` keeps its box by the design's own predicate** (`in[:]` is an implicit
   address-take per §3.3's reversion rule) — the §7 SetBytes/Bytes probe rows gain a small named
   residual term (§3 above).
4. **`edwards25519/field`'s `carryPropagate` strip** (§2 e): the second fiat family's field
   multiply keeps the boxed convention in Phase A because its core ends in a method call.
   Allocation-neutral for Phase A's measured prize; a named constituent for B′.
5. **The whitelist's default arm never decided an outcome alone**: `other-use` fired only
   co-vetoed (darwin's 16 pthread-seam params, all X3 alongside; linux 1; windows 0) — the
   classifier's recognized-shape coverage is effectively total on this corpus.
6. **A2's hand-own rule** (§2 c): X5 gains the declared-in-hand-own arm + a 3-function curated
   exclusion; zero hand-own edits owed.

## 7. Census artifacts

- Instrument: `src/go2cs/refLoweringAnalysisOperations.go` (pass), `src/go2cs/refLoweringCensus.go`
  (`-ref-census`), `src/go2cs/refLoweringAnalysis_test.go` (guards); wired in
  `conversionDriver.go` (both drivers), `testConversion.go` (`-tests`), reset in
  `packageStateOperations.go`; flag surface in `main.go`/`commandLineOptions.go`.
- The JSON report is regenerable by §1's command and is deliberately not committed (the numbers
  above are its digest; the pinned-machine re-derivation supersedes them at merge).

---

## 8. AMENDMENT 2026-08-26 — the pinned-machine re-derivation, and what the delta actually is

> Added, not rewritten (§ doc-type rule). §5.1 recorded a toolchain debt: the numbers above were
> measured on **go1.23.2** against a corpus then pinned to **1.23.1**. That debt is now paid on
> **go1.23.12** — the pin the corpus re-banked at (`a2e079259`) — on the perf-canon host
> (G-LAPTOP), converter rebuilt on 1.23.12 and stamp-verified (`go version <exe>` →
> `go1.23.12`, closing false-green route #4). Wall time **13.2 s** for all three targets, exit 0,
> empty stderr, nothing emitted.

### 8.1 The headline: the delta is the INSTRUMENT, not the hop

The numbers moved a lot, and the obvious reading — "the 1.23.1 → 1.23.12 corpus hop moved the
constituency" — is **wrong**. Six commits touched the classifier between this census and today,
`0c631dd7a` ("the A2 hand-own arms and the emittability narrowing") among them. Attributing their
effect to the hop would have mis-priced both B′ and B1, so the delta was decomposed rather than
reported.

**Method — single-variable A/B.** The A1-era instrument was rebuilt from `44f3ea609` (the parent of
`0c631dd7a`, the first A2 classifier commit) in a temporary worktree, **with the same go1.23.12
toolchain** (both binaries stamp `go1.23.12`), and run against **the same GOROOT and the same
corpus** as the current instrument. Only the instrument source differs. The opposite A/B — current
instrument at GOROOT 1.23.1 — **cannot be run, by design**: the converter refuses `-stdlib` when
GOROOT's release disagrees with `version.props`'s `<GoStdLibVersion>`, naming the exact silent
divergence it prevents. That guard is working as intended and is noted here as a positive control,
not an obstacle.

windows/amd64, all three columns on the current corpus except the first:

| | A1 as banked (go1.23.2, corpus 1.23.1) | **A1-era instrument @ 1.23.12** | **current instrument @ 1.23.12** |
|:--|--:|--:|--:|
| package-level funcs | 7,736 | 7,741 | 7,741 |
| pointer params | 2,819 | **2,819** | 2,750 |
| lowered (Phase A) | 564 | **564** | 528 |
| lowered (A′) | 632 | **632** | 590 |
| **method ptr-params (B′ context)** | **1,609** | **1,609** | **1,609** |
| address-taken locals / revert | 2,771 / 236 | 2,766 / **236** | 2,794 / 231 |
| row 1 field addr | 281 | **282** | 178 |
| row 2 local addr | 286 | **286** | 276 |
| row 3 pointer var | 806 | **808** | 768 |
| row 4 elem addr | 17 | **17** | 16 |
| row 5 pointer conv | 31 | **31** | 26 |
| row 6 temp | 11 | **11** | 11 |
| row 7 nil | 1 | **1** | 0 |
| defer-go carve-out | 13 | **13** | 13 |
| **other-veto at lowered positions** | **0** | **0** | **0** |

**Read the middle column against the first**: the A1-era instrument, run on the *new* corpus,
reproduces the *old* banked numbers — row 1 282 vs 281, row 3 808 vs 806, funcs 7,741 vs 7,736,
and `lowered`, `revert`, rows 2/4/5/6/7 and defer-go **exactly**. **The corpus hop moved essentially
nothing.** Every material difference between this census and today is A2's own deliberate
narrowing, doing what its commit messages say it does.

### 8.2 What A2 added, quantified

New veto arms present only in the current instrument (windows/amd64):

| arm | A1-era | current | note |
|:--|--:|--:|:--|
| `X5-hand-owned` | — | **96** | the A2 hand-own arm (§6.6's "declared-in-hand-own") |
| `X5-hand-own-caller` | — | **3** | its caller-side mirror |
| `caller-shape` | — | **22** | the emittability narrowing's caller-side strip |
| `X5-linkname` | 177 | **192** | +15, the curated-registry `std/` prefix normalization (`bdb703c0c`) |
| `other-use` | — | **2** | see §8.3 |

Everything else is untouched to the unit: `X1-identity` 239, `X2-capture` 51, `X2-return` 26,
`X3-representation` 1,159, `X4-repoint` 47, `X5-bodiless` 465, `X5-func-value` 545,
`other-use-ptr-slice` 19 — identical across both instruments. The classifier's core did not drift;
A2 added arms beside it.

**Hand-own census, re-measured (both instruments agree, so it is a corpus fact):** **76 marked
files, 59 `*_impl.cs` companions** — against §5.4's 49/41. Grown, per CLAUDE.md's re-measure-never-
carry rule. The A2 arms are visibly doing their job here: **candidate references still to resolve
falls 24 → 11**.

### 8.3 The completeness property still holds — and one reading trap inside it

`other-veto` **remains 0 at lowered positions on all three targets**, so §2(b)'s completeness proof
survives both the hop and the narrowing. This is the quantity B′'s S1 gate re-checks, and it is
green in advance.

⚠ The trap: the A2 instrument added a **new diagnostic array**, `otherVetoSites`, which the A1-era
instrument did not emit at all (0 occurrences in its JSON, 380 in today's). Counting raw
`"other-veto"` strings in the report therefore reads as a completeness regression when nothing
regressed. The discriminator is structural, not textual: `"other-veto"` never appears as a
**shapeCounts key** (0 occurrences — which is what a lowered-position shape would be); all 380 are
`"shape": "other-veto"` **values** inside `otherVetoSites`, recording shapes at *non-lowered*
positions for A2/B′ input. Two of them surface parameter-side as the `other-use` arm above.

### 8.4 What this means for the arc

1. **A1's conclusions stand; its numbers are superseded by A2, not by the hop.** The branch
   confirmation (§10.3's hoisted-temp rule), the zero-unclassifiable-shape proof, and the
   no-new-layout-L3 result are all re-confirmed on the pinned toolchain.
2. **B′'s constituency is invariant.** `method ptr-params (B′ context) = 1,609` is identical across
   all three measurements — unmoved by the hop *and* unmoved by A2's narrowing. B′-S0 starts from a
   stable evidence base, which is the single most useful thing this re-derivation establishes.
3. **B1 inherits the narrowed Phase-A world**, not the A1-era one: 528 lowered params and 231
   reverting locals are the current facts, and any B1 pricing that quotes 564/236 is quoting a
   superseded instrument.

Linux and darwin re-derived in the same run and move the same way; their current-instrument
figures are `2,469 / 530 / 589` and `2,522 / 537 / 597` (pointer params / lowered A / lowered A′),
with `other-veto` 0 on both. Report regenerable by §1's command; it remains deliberately
uncommitted.
