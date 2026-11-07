# DATA — the P-384 field multiply: converted fiat arithmetic, root-caused and cured

> **State: MEASURED (2026-11-06/07), win-x64, one laptop (R-LAPTOP), branch `claude/r-p384-field` from `7fbb8fabfa`.**
> The P-384 row H sized in [DATA-p521-table.md](DATA-p521-table.md) ("P-384's fiat Mul has P-521's shape"), scouted
> by R and ruled GO by COORD on 2026-11-06 after the prediction in §5 was posted. Sections are appended and
> superseded, never overwritten. Raw readings, the benchmark sources, the generator and the compiled listings are in
> [`data/p384-field/`](data/p384-field/).

## 1. Why this was measured

H's P-521 hand-own (DATA-p521-table) left one sibling row open: fiat's `p384Mul` keeps most of its calls too, off the
start-up path but on every P-384 ECDH, ECDSA and `crypto/elliptic` operation. COORD's brief: measure Go against the
converted `p384Mul`/`p384Square` under the JIT and ReadyToRun, read the disassembly, and predict the cure in H's
shape or name a cheaper lever (460 locals is under the JIT's 512-local cut-off, so P-384 might differ).

## 2. Host, toolchain, method

| | |
|:--|:--|
| box | R-LAPTOP: AMD Ryzen 7 PRO 6850U, 8 cores / 16 threads, a laptop on mains, sleep disabled |
| RID | win-x64 for every arm |
| toolchains | go1.26.10 windows/amd64 (purego tags for Go's own benchmarks, the corpus flavour); .NET SDK 10.0.400, runtime 10.0.11 |
| C# arms | DOTNET_TieredCompilation=0 on both. **JIT**: a Release build. **ReadyToRun**: self-contained, single file, PublishReadyToRun, `PublishReadyToRunComposite=false` (the converted template's setting since batch 214) |

- **The microbenchmark** ([`bench-Program.cs.txt`](data/p384-field/bench-Program.cs.txt)), after H's: the converted
  public API called directly — `P384Element.Mul`, `Square`, `Add`, `P384Point.Add`, `Double`, `ScalarBaseMult`,
  `ScalarMult` — plus P-256's Mul and Square as controls and P-521's hand-owned Mul as a reference. Each reading is the
  median of 7 rounds after 2 warm-up calls; the first `ScalarBaseMult` (the generator-table build) is timed cold.
  Go's figures come from its own fiat and nistec `benchmark_test.go` via `go test -bench`.
- **The differential** ([`bench-RealCheck.cs.txt`](data/p384-field/bench-RealCheck.cs.txt)): public-API Mul, Square
  and aliased Mul against `System.Numerics.BigInteger`, 36 edge pairs and 20,000 seeded pairs.
- **Disassembly**: the runtime's `DOTNET_JitDisasm` for the JIT, crossgen2's `--codegenopt JitDisasm` for
  ReadyToRun. Which methods the runtime JIT-compiles comes from `DOTNET_JitDisasmSummary`.

## 3. Before: where the time goes

Microbenchmark at the seat's base, 2 processes per arm ([`bench-base-jit.txt`](data/p384-field/bench-base-jit.txt),
[`bench-base-r2r.txt`](data/p384-field/bench-base-r2r.txt), [`go-fiat.txt`](data/p384-field/go-fiat.txt),
[`go-nistec.txt`](data/p384-field/go-nistec.txt)):

| operation | Go | C# JIT | C# ReadyToRun |
|:--|--:|--:|--:|
| field Mul | 75 to 79 ns | 2.07 to 2.12 us | 5.06 to 5.28 us |
| field Square | 70 to 73 ns | 2.00 to 2.02 us | 4.96 to 5.12 us |
| field Add | | 80 to 83 ns | 330 to 385 ns |
| point Add | | 33.0 to 33.1 us | 82.5 to 83.8 us |
| point Double | | 30.6 us | 76.1 to 76.2 us |
| ScalarBaseMult | 173 to 177 us | 3.44 to 3.45 ms | 9.04 to 9.20 ms |
| ScalarMult | 639 to 661 us | 15.5 to 15.6 ms | 39.1 to 39.9 ms |
| first ScalarBaseMult (table build, cold) | | 231 to 334 ms (146 to 269 ms of it JIT) | 167 to 182 ms |

A point Add is 14 field Mul and 20 Add/Sub (Go's `p384.go`), so the multiplies are about 88% of it under the JIT
(14 × 2.07 of 33 us) and 85% under ReadyToRun. ScalarBaseMult is 96 point Adds plus table selects.

## 4. Root cause, from the disassembly

**C1: the inliner stops early (JIT).** The converted `p384Mul` starts at about 460 named locals, under the 512 cut-off,
but every inlined callee adds temporaries. The JIT listing ([`jit-base-p384Mul.txt`](data/p384-field/listings/jit-base-p384Mul.txt))
holds 74 inlinees and then **590 calls per field multiply**:
- 292 `p384Uint1` implicit conversions, 137 `bits.Add64`, 73 `bits.Mul64`, 7 `bits.Sub64`;
- 36 limb-wrapper `get_Item`, 6 `p384CmovznzU64`, and the throw helpers.
`bits.Mul64` already carries AggressiveInlining (item 7), and 73 of them still stay calls. So "460 is under 512"
does not save it: the budget runs out inside the method.

**C2: ReadyToRun's version bubble.** Calls into math.bits, golib and CoreLib never inline in a precompiled body, so
all 590 stay, and the ReadyToRun multiply is 2.5x the JIT's.

**No cheaper lever.** Nothing on the visible-code side lowers the caller's local count, and `bits.*` stay visible at
call sites by the intrinsic-granularity ruling (2026-10-22). The seam is a non-visible hand-own, as for P-521.

## 5. The prediction, as posted before any fix

Posted to COORD at `327af355ca`, after an in-tree scratch measurement of the candidate body (the worktree restored
afterwards; [`zz_p384_scratch_impl.cs.txt`](data/p384-field/zz_p384_scratch_impl.cs.txt)):

**Lever:** a hand-owned body for fiat's `p384Mul` and `p384Square` in H's shape — a straight-line six-limb CIOS with
m' = 0x100000001, the four 32x32 products of p521_impl.cs's `Mul64` (reused, not duplicated), bitwise carries, no
Span, and a mask-selected final subtraction. Square is the same multiply.

**Scratch readings:** 60,108 BigInteger checks, 0 mismatches, under both arms; field Mul JIT 575 to 644 ns, ReadyToRun
653 to 676 ns; ScalarBaseMult JIT 1.38 to 1.55 ms, ReadyToRun 2.74 to 2.83 ms; ReadyToRun table build 58 ms; no fiat
P-384 method JIT-compiled at run time in the ReadyToRun host. A bare `Math.BigMul` spelling is also kept with
composite off, but ran slower under the JIT (656 against 435 ns in a prototype), since its out-parameter low word goes
through the stack.

**Files:** the manualConversionFuncs entry and its comment, `fiat/p384_impl.cs` and its generator here, the
regenerated `fiat/p384_fiat64.cs` and fiat `package_info.cs` (exactly those two, identical on three targets),
GolibTests `P384FieldDifferentialTests`, behavioral `P384CurveBytes`, the source pin, the BOARD row.

**Predicted at landing** (R-LAPTOP, the same bench, at least 3 processes per arm, base and cut in one session):
- field Mul JIT 0.55 to 0.70 us, ReadyToRun 0.62 to 0.75 us;
- ScalarBaseMult JIT 1.3 to 1.6 ms, ReadyToRun 2.6 to 3.0 ms;
- ReadyToRun table build 50 to 65 ms.

**Falsifiers:** JIT Mul above 0.75 us or ReadyToRun Mul above 0.85 us; any fiat P-384 method in the ReadyToRun
run-time JIT list; any BigInteger or Go<->C# byte mismatch; any other corpus file moving, or the three targets
differing; a conditional jump on a limb value in either listing; the source pin not going red when `p384Mul`'s Go body
changes.

COORD ruled GO at `d8aa145096`, the prediction and falsifiers accepted as worded.

## 6. After: the landed body

**The body** ([`listings/`](data/p384-field/listings/), the JIT and crossgen2):
- JIT ([`jit-final-p384Mul.txt`](data/p384-field/listings/jit-final-p384Mul.txt)): 19.1 KB of code, 162 inlinees, every
  product inlined (156 IMUL, no MUL or MULX). Its only calls are 18 cold limb-wrapper `materializeValue` calls and the
  throw helpers (`IndexOutOfRange`, `THROW`, `RNGCHKFAIL`).
- ReadyToRun ([`r2r-final-p384Mul.txt`](data/p384-field/listings/r2r-final-p384Mul.txt), crossgen2 10.0.11 with
  `--codegenopt:JitDisasm`): 18.8 KB of code, 108 inlinees, every product inlined (156 IMUL, no MUL or MULX). Its only
  calls are 18 golib `array.get_Item` (the six limb reads of each argument and the six writes, at constant indexes,
  across the version bubble) and 18 cold `materializeValue`. Unlike the P-521 body, no `Mul64` is left as a call.
- **Constant time, checked in both listings.** The JIT's conditional jumps are 18 `je` (wrapper null tests to
  `materializeValue`), 18 `jae` and 18 `jle` (bounds checks of constant indexes against the array's length) and 1 `jne`
  (the prolog's fixed-count frame-zeroing loop). crossgen2's are 18 `je` and 1 `jne`; its bounds checks live inside
  `array.get_Item`, also on constant indexes. None tests a limb value; the final reduction is a mask select.
- **Kept by the runtime:** in the ReadyToRun host with tiering off, `DOTNET_JitDisasmSummary` lists no fiat method
  JIT-compiled at run time, base or cut ([`r2r-jitsum-cut.txt`](data/p384-field/r2r-jitsum-cut.txt)).

**Microbenchmark**, base and cut interleaved in every round, 3 rounds, alone on the box
([`landing-ab.sh`](data/p384-field/landing-ab.sh), [`ab.log`](data/p384-field/ab.log), `landing-bench-*.txt`;
process medians, low to high):

| operation | Go | base JIT | cut JIT | base ReadyToRun | cut ReadyToRun |
|:--|--:|--:|--:|--:|--:|
| field Mul | 75 to 79 ns | 2.10 to 2.22 us | 542 to 566 ns | 4.99 to 5.10 us | 659 to 694 ns |
| field Square | 70 to 73 ns | 1.93 to 2.10 us | 556 to 584 ns | 4.91 to 4.94 us | 662 to 690 ns |
| field Add (untouched) | | 81 to 88 ns | 81 to 86 ns | 326 to 333 ns | 323 to 326 ns |
| point Add | | 32.9 to 35.1 us | 11.2 to 11.6 us | 81.1 to 82.8 us | 19.9 to 20.5 us |
| point Double | | 30.2 to 31.3 us | 10.0 to 10.3 us | 74.4 to 75.2 us | 16.4 to 17.2 us |
| ScalarBaseMult | 176 to 190 us | 3.43 to 3.53 ms | 1.35 to 1.38 ms | 8.70 to 8.87 ms | 2.79 to 2.85 ms |
| ScalarMult | 646 to 658 us | 15.5 to 15.6 ms | 5.18 to 5.41 ms | 38.3 to 38.7 ms | 9.31 to 9.64 ms |
| first ScalarBaseMult (table build, cold) | | 189 to 298 ms | 137 to 243 ms | 164 to 176 ms | 55.4 to 55.8 ms |
| P-256 field Mul (control) | | 326 to 334 ns | 312 to 347 ns | 1.56 to 1.58 us | 1.61 to 1.65 us |
| P-521 field Mul (control) | | 1.35 to 1.57 us | 1.34 to 1.43 us | 1.33 to 1.34 us | 1.34 to 1.37 us |

- Cut over base: field Mul 0.264 under the JIT and 0.136 under ReadyToRun; ScalarBaseMult 0.391 and 0.321; ScalarMult
  0.346 and 0.248.
- The untouched rows (field Add, the P-256 and P-521 controls) stay within 4%, so the arms are comparable.
- The JIT's cold table build is dominated by JIT time itself (113 to 233 ms of it in both arms), so its spread is the
  JIT's, not the multiply's.
- Still about 7x Go per multiply: a kept precompiled body cannot use MULX or ADC (DATA-p521-table §6), and the carries
  are bitwise.
- The differential ([`check-cut-jit.txt`](data/p384-field/check-cut-jit.txt), [`check-cut-r2r.txt`](data/p384-field/check-cut-r2r.txt))
  reads 60,108 checks and 0 mismatches in both arms.

## 7. The prediction, scored as worded

| predicted | measured | score |
|:--|:--|:--|
| field Mul JIT 0.55 to 0.70 us | process medians 542, 558, 567 ns | **MISSED**, by one process of three at the band's low edge (542 ns, faster) |
| field Mul ReadyToRun 0.62 to 0.75 us | 659 to 694 ns | MET |
| ScalarBaseMult JIT 1.3 to 1.6 ms | 1.35 to 1.38 ms | MET |
| ScalarBaseMult ReadyToRun 2.6 to 3.0 ms | 2.79 to 2.85 ms | MET |
| ReadyToRun table build 50 to 65 ms | 55.4 to 55.8 ms | MET |
| exactly `p384_fiat64.cs` and fiat `package_info.cs` move, identical on three targets | exactly those two, identical on windows, linux and darwin (§8) | MET |
| Falsifier: JIT Mul above 0.75 us or ReadyToRun Mul above 0.85 us | 566 / 694 ns at most | HELD |
| Falsifier: any fiat P-384 method in the ReadyToRun run-time JIT list | none | HELD |
| Falsifier: any BigInteger or Go<->C# byte mismatch | none (§9) | HELD |
| Falsifier: any other corpus file moves, or the three targets differ | none, identical | HELD |
| Falsifier: a conditional jump on a limb value in either listing | none (§6) | HELD |
| Falsifier: the source pin not going red when `p384Mul`'s Go body changes | it goes red (§9) | HELD |

The JIT band was set from the scratch body's 575 to 644 ns. The landed body is the same statements with p521_impl.cs's
`Mul64` in place of an identical private copy; the 30 to 80 ns gain is not attributed (the scratch and landing runs were
different sessions, and the cross-session spread on this box is of that size).

## 8. Footprint

A two-seeded three-target `-stdlib` emission ([`regen.sh`](data/p384-field/regen.sh),
[`emit-summary.txt`](data/p384-field/emit-summary.txt)):
- the arms were the converter at `88ebcc04e9` (base) and at `339e1f419a` (cut), both roots seeded from one frozen
  snapshot of the cut tree, run serially (2,400 s and 2,259 s);
- each arm staged 4,564 `.cs` per target, writing 1,949 (windows), 2,024 (linux) and 2,021 (darwin) fresh files;
- the base arm rewrote 0 corpus files (its emission equals the committed tree, CR-stripped), the cut arm exactly 2;
- rootA and rootB differ in exactly `fiat/p384_fiat64.cs` (+3/-1,414: the two bodies become placeholders) and fiat's
  `package_info.cs` (its `GoPositionMap` record re-encodes). Each file's staged copies are identical across windows,
  linux and darwin in both arms;
- 0 DYNTYPE markers in either arm. The whole-tree diff covers every hand-owned file and `.cs.auto` sibling, and none
  moved.

## 9. Tests and controls

- **GolibTests `P384FieldDifferentialTests`** against BigInteger: every ordered pair of 11 limb-pattern edges (0, 1,
  2, p-1, p-2, R mod p, R^2 mod p, 2^64-1, 2^320-1, p mod 2^320, 2^383) and every edge squared; 20,000 seeded random
  pairs; the aliased forms; and `RawLimbsAreFullyReduced`, which reads the raw Montgomery limbs on edges whose
  pre-reduction t lands in [p, 2p) (25 Mul pairs and 5 squares do; it fails as VACUOUS below 4 and 1). 4 of 4 green
  against the converted bodies, which set the oracle, and 4 of 4 against the hand-own.
- **Behavioral `P384CurveBytes`**: P-384 ECDH (four key pairs, public keys and shared secrets), an ECDSA public key
  from a fixed private key, verification of a fixed signature and of two tampered inputs, and `crypto/elliptic` base
  and point multiplies, byte for byte against Go. Green on all four phases before and after. Its csproj was minted by
  `UpdateTestTargets --createTargetFiles --only` and carries batch 214's composite-off lines.
- **Planted defect A** ([`plantA-golib.log`](data/p384-field/plantA-golib.log), [`plantA-beh.log`](data/p384-field/plantA-beh.log)):
  round 0's `m' = 0x100000001` planted as `0x100000000`. The differential failed 4 of 4 and the behavioral test failed
  its Output phase.
- **Planted defect B** ([`plantB-golib.log`](data/p384-field/plantB-golib.log)): the final select skipped
  (`keepD = 0`). All four differential tests failed, not only the raw-limb one: on P-384 an unreduced Montgomery value
  does not survive fiat's from-Montgomery conversion intact (the read values were off by one), where on P-521 it did.
  Both plants restored md5-identical (`210bcb8f…`).
- **repoguard `TestHandOwnedReimplementationsMatchPinnedGoSource`** pins go1.26.10's `p384Mul` (`2e891736…`) and
  `p384Square` (`471ea92f…`). The same hasher reproduces H's two P-521 pins. Red before the hand-own existed, naming
  the missing file. A copy of `p384_fiat64.go` with one constant changed in `p384Mul`'s body hashes to `d7279e4e…`, and
  one with only a doc-comment line added keeps the pin.

## 10. Gates

The battery ran serially at `8e3027d344`, alone on the box ([`battery.sh`](data/p384-field/battery.sh)):

| gate | result |
|:--|:--|
| `go test ./...` (src/go2cs) | 2 packages red, both this seat's own bookkeeping, fixed below |
| GolibTests Release | 2,394 passed, 0 failed, 32 skipped |
| behavioral `P384CurveBytes` and `P521CurveBytes` | 4 of 4 phases each |
| `go2cs.slnx` Debug | 0 errors |
| `check-no-regression.ps1` | NO REGRESSION: byte-identical across all 965 behavioral packages (8 platform-exclusive skipped) |
| sweep `crypto/internal/fips140/nistec` | PASS 44 of 44 banked (114 s) |
| sweep `crypto/ecdh` | PASS 47 of 47 (77 s) |
| sweep `crypto/ecdsa` | PASS 125 of 125 (130 s) |
| sweep `crypto/internal/fips140test` | PASS 2,270 of 2,270 (378 s) |
| sweep `crypto/tls` | PASS 1,393 of 1,393 (1,231 s) |

Every sweep left 0 tracked deletions. The four sweeps after nistec left content drift in their own converted
`*_test.cs` and test `package_info` files (25 files, all test files, none in fiat; the two lines naming "p384" or
"fiat" are base64 inside position maps) and rewrote `docs/validation/current/crypto.tls.md`. That is the committed test
emission's staleness against the base converter, the class DATA-p521-table §11 recorded, not this seat; all 26 were
restored from HEAD by file list.

The two reds, each fixed in this branch and re-run green:
- `TestDocsProvenanceNotesResolve` and its two controls: the new list entries in `manualTypeOperations.go` moved the
  `math/bits` entry 8 lines, so two notes cited it at `:2798`. They now cite `:2806`.
- `TestCommentParityCensus`: `partial:decl-doc` 56 -> 62 and `top:unmapped:func` 359 -> 361. Both are
  `p384_fiat64.cs`: the two displaced functions' doc comments now live in `p384_impl.cs`, the same mechanism that
  already puts `p521_fiat64.cs` at 6 in `partial:decl-doc` since batch 220 read placeholder files. Re-baselined; no
  other class moved.
