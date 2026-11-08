# DATA — the P-256 field multiply under ReadyToRun: the version bubble, root-caused and cured

> **State: MEASURED (2026-11-07), win-x64, one laptop (R-LAPTOP), branch `claude/r-p256-field` on master `31dc1870cb`
> (batch 238, the P-384 seat landed).** The OPEN row R filed while scouting P-384 ("P-256's fiat Mul at about 5x its JIT under ReadyToRun, on
> the purego path TLS uses"), scouted and ruled GO by COORD on 2026-11-07 after the prediction in §5 was posted. Built on
> the P-384 seat as ruled, since both touch fiat's registration, `package_info.cs` and the comment-census baseline, and
> re-derived on master once that seat landed.
> Sections are appended and superseded, never overwritten. Raw readings, the benchmark and handshake sources, the
> generator and the compiled listings are in [`data/p256-field/`](data/p256-field/).

## 1. Why this was measured

P-256 is in Go 1.26's default TLS key exchanges twice (the SecP256r1MLKEM768 hybrid, second in `defaults.go`, and
plain P-256) and is the curve most ECDSA certificates carry, and the corpus is Go's purego flavour, so ECDHE and ECDSA on P-256 run through fiat's `p256Mul` on every such handshake. The
P-384 seat's bench showed P-256's field multiply at 0.33 us under the JIT and 1.6 us under ReadyToRun, a profile unlike
P-384's and P-521's, where both arms were slow.

## 2. Host, toolchain, method

The P-384 record's method ([DATA-p384-field.md §2](DATA-p384-field.md#2-host-toolchain-method)): R-LAPTOP, win-x64,
go1.26.10 (purego tags for Go's own benchmarks), .NET SDK 10.0.400 and runtime 10.0.11, tiering off in both C# arms; the
ReadyToRun arm self-contained, single file, composite off. Two programs:
- **the microbenchmark** ([`bench-Program.cs.txt`](data/p256-field/bench-Program.cs.txt)): the P-384 bench with P-256
  ScalarBaseMult and ScalarMult rows and a P-256 differential mode
  ([`bench-RealCheck256.cs.txt`](data/p256-field/bench-RealCheck256.cs.txt));
- **a TLS handshake program** ([`tls-main.go.txt`](data/p256-field/tls-main.go.txt)), derived from the Perf suite's
  `PerfTlsHandshake`: per round, 32 TLS 1.3 handshakes over loopback in two arms. The DEFAULT arm is PerfTlsHandshake's
  handshake (its RSA certificate, default curve preferences, which negotiate X25519MLKEM768, curve ID 4588). The P-256
  arm uses a self-signed ECDSA P-256 certificate made before any clock starts and `CurvePreferences = [P256]` on both
  sides (curve ID 23). It prints microseconds per handshake and a checksum of count, version, suite and curve. Converted
  by go2cs and built for both C# arms like the bench.

Base and cut are interleaved in every round, 3 rounds; the handshake program runs its two arms three times per process,
so each handshake row holds 9 readings per C# arm.

## 3. Before: where the time goes

| P-256 | Go | C# JIT | C# ReadyToRun |
|:--|--:|--:|--:|
| field Mul | (no Go fiat benchmark) | 316 to 335 ns | 1.54 to 1.60 us |
| field Square | | 321 to 360 ns | 1.57 to 1.59 us |
| ScalarBaseMult | 44.0 to 44.6 us | 827 to 853 us | 2.07 to 2.17 ms |
| ScalarMult | 218 to 221 us | 2.93 to 3.06 ms | 9.91 to 10.15 ms |
| TLS 1.3 handshake, P-256 arm | 2.39 to 2.67 ms | 19.9 to 22.9 ms | 59.3 to 61.5 ms |
| TLS 1.3 handshake, default arm | 4.94 to 5.92 ms | 38.3 to 49.8 ms | 155 to 162 ms |

Go's handshake figures are the same program under `go run -tags purego,math_big_pure_go`
([`go-tls.txt`](data/p256-field/go-tls.txt)); its ScalarBaseMult and ScalarMult are nistec's own benchmarks
([`go-nistec-p256.txt`](data/p256-field/go-nistec-p256.txt)).

## 4. Root cause, from the disassembly

**Not the local count.** The converted `p256Mul` has 182 locals. Under the JIT it inlines fully: 6.3 KB of code, 28
MULX, and its only calls are cold throw paths and 4 `p256CmovznzU64`
([`jit-base-p256Mul.txt`](data/p256-field/listings/jit-base-p256Mul.txt)).

**The version bubble.** crossgen2 precompiles fiat alone, so no call into math/bits or golib inlines: 6.2 KB of code and
**136 calls per field multiply**, 51 `bits.Add64`, 28 `bits.Mul64`, 5 `bits.Sub64`, 24 golib `array.get_Item`, 24 cold
`materializeValue` and 4 `p256CmovznzU64`, and no MUL or MULX at all
([`r2r-base-p256Mul.txt`](data/p256-field/listings/r2r-base-p256Mul.txt)). The runtime keeps that body (no fiat method
is JIT-compiled at run time with composite off), so the cost is the calls, not a rejected body.

**No smaller lever.** The 84 `bits.*` calls are the cost, and only a body inside fiat's own assembly inlines them. A
larger ReadyToRun bubble (composite or `--inputbubble`) is the template shape batch 214 ruled out.

## 5. The prediction, as posted before any fix

Posted to COORD at `b2e6719535`, after an in-tree scratch measurement (the worktree restored afterwards;
[`zz_p256_scratch_impl.cs.txt`](data/p256-field/zz_p256_scratch_impl.cs.txt), [`scout/`](data/p256-field/scout/)):

**Lever:** a hand-owned `p256Mul`/`p256Square` in the P-384 body's shape: a straight-line four-limb CIOS with m' = 1
(p's low limb is 2^64 - 1), p's zero limb left out of the reduction, p521_impl.cs's four-product `Mul64`, bitwise
carries and a mask-selected final subtraction, in a new `fiat/p256_field_impl.cs` (`fiat/p256_impl.cs` is the
Montgomery-limb loader and displaces nothing).

**Scratch readings:** 60,108 BigInteger checks, 0 mismatches, in both arms; field Mul JIT 212 to 222 ns, ReadyToRun 303
to 350 ns; P-256 handshake ReadyToRun 38.0 to 41.4 ms; the default handshake unmoved.

**Predicted at landing:** field Mul JIT 0.19 to 0.26 us, ReadyToRun 0.28 to 0.38 us; ScalarBaseMult JIT 0.70 to 0.82 ms,
ReadyToRun 1.30 to 1.55 ms; ScalarMult ReadyToRun 4.2 to 5.3 ms; P-256 handshake ReadyToRun 34 to 44 ms (the JIT
handshake gain declared too small against its spread to score); the default handshake and PerfTlsHandshake within 3%.

**Falsifiers:** JIT Mul above 0.30 us or ReadyToRun Mul above 0.45 us; any fiat P-256 method in the ReadyToRun run-time
JIT list; any BigInteger or Go<->C# byte mismatch; any other corpus file moving or the three targets differing; a
conditional jump on a limb value in either listing; the source pin not going red on a body edit; the default handshake
or PerfTlsHandshake moving more than 3%.

COORD ruled GO, the prediction and falsifiers accepted as worded.

## 6. After: the landed body

**The body** ([`listings/`](data/p256-field/listings/)):
- JIT ([`jit-final-p256Mul.txt`](data/p256-field/listings/jit-final-p256Mul.txt)): 8.1 KB of code, 64 IMUL, no MUL or
  MULX, no arithmetic call. The 16 products of a * b take four IMUL each; the reduction's products by p's constant
  limbs fold to shifts and subtractions. Its only calls are 12 cold `materializeValue` and the throw helpers.
- ReadyToRun ([`r2r-final-p256Mul.txt`](data/p256-field/listings/r2r-final-p256Mul.txt)): 8.0 KB of code, 64 IMUL, no
  MUL or MULX; its only calls are 12 golib `array.get_Item` (the eight limb reads and four writes, at constant indexes)
  and 12 cold `materializeValue`.
- **Constant time, checked in both listings.** The JIT's conditional jumps are 12 `je` (wrapper null tests), 12 `jae`
  and 12 `jle` (constant-index bounds checks) and 1 `jne` (the prolog's frame-zeroing loop); crossgen2's are 12 `je` and
  1 `jne`. None tests a limb value; the final reduction is a mask select.
- **Kept:** no fiat method JIT-compiled at run time in the ReadyToRun host, base or cut
  ([`r2r-jitsum-cut.txt`](data/p256-field/r2r-jitsum-cut.txt)).

**Measured at the union**, base (master `31dc1870cb`) and cut (`933db85492`) interleaved
([`landing-ab.sh`](data/p256-field/landing-ab.sh), `landing-*.txt`):

| P-256 | Go | base JIT | cut JIT | base ReadyToRun | cut ReadyToRun |
|:--|--:|--:|--:|--:|--:|
| field Mul | | 308 to 340 ns | 216 to 232 ns | 1.55 to 1.60 us | 299 to 307 ns |
| field Square | | 314 to 355 ns | 224 to 243 ns | 1.54 to 1.61 us | 296 to 307 ns |
| ScalarBaseMult | 44.0 to 44.6 us | 824 to 896 us | 762 to 834 us | 2.13 to 2.20 ms | 1.42 to 1.47 ms |
| ScalarMult | 218 to 221 us | 2.91 to 2.98 ms | 2.34 to 2.44 ms | 9.89 to 10.18 ms | 4.46 to 4.58 ms |
| TLS handshake, P-256 arm | 2.39 to 2.67 ms | 19.1 to 21.4 ms | 17.2 to 21.1 ms | 59.3 to 65.0 ms | 38.2 to 39.6 ms |
| TLS handshake, default arm (control) | 4.94 to 5.92 ms | 38.2 to 43.2 ms | 38.3 to 49.5 ms | 158 to 172 ms | 155 to 162 ms |
| P-384 field Mul (control) | | 556 to 586 ns | 629 to 665 ns | 653 to 659 ns | 648 to 658 ns |
| P-521 field Mul (control) | | 1.32 to 1.37 us | 1.21 to 1.39 us | 1.25 to 1.28 us | 1.31 to 1.58 us |

- Cut over base (medians): field Mul 0.666 under the JIT and 0.195 under ReadyToRun; ScalarBaseMult 0.929 and 0.664;
  ScalarMult 0.801 and 0.448; the P-256 handshake 0.883 and 0.634 (about 22 ms saved per handshake under ReadyToRun).
- Two earlier A/Bs, on `e068b368a6` (before the rebase) and on `8a4aaba56f` (the P-384 tip), read the same within a few
  percent: ReadyToRun field Mul 0.193 and 0.189, the P-256 handshake 0.640 and 0.629 ([`ab-prior/`](data/p256-field/ab-prior/)).
- **The controls, read honestly.** The default handshake moves 0.989 (JIT) and 0.985 (ReadyToRun). P-384's field Mul
  read slower in the cut under the JIT in all three A/Bs (+2%, +13%, +9%) and flat under ReadyToRun (+1%, +3%, 0%);
  P-256's change does not touch `p384Mul`, MEASURED (amended 2026-11-07, G's note): `src/tools/ildiff` over the two
  arms' fiat assemblies reads `methods=1070 differing=2`, exactly `p256Mul` and `p256Square`
  ([`ildiff-fiat.txt`](data/p256-field/ildiff-fiat.txt)). `p384Mul` is IL-identical, so its JIT drift is JIT placement,
  not code.
  P-521's ReadyToRun median read 1.164 in the union run, from one process whose readings reached 1.58 us; the two earlier
  runs read 0.991 and 1.014.
- The JIT gains too, though the converted body already inlined: the hand-own replaces 28 MULX plus tuple-returning
  carries with 64 IMUL and flat carries, and its reduction products by p's constants fold.
- What remains of ReadyToRun ScalarBaseMult over the JIT (1.40 against 0.77 ms) is the same bubble in `p256Add`,
  `p256Sub`, `p256Selectznz` and the domain conversions: R's OPEN Add/Sub row, not this seat.
- The differential ([`check-cut.txt`](data/p256-field/check-cut.txt)) reads 60,108 checks and 0 mismatches in both arms;
  the base arm, the oracle, reads the same.

## 7. The prediction, scored as worded

Scored at the union A/B (§6):

| predicted | measured | score |
|:--|:--|:--|
| field Mul JIT 0.19 to 0.26 us | 216 to 232 ns | MET |
| field Mul ReadyToRun 0.28 to 0.38 us | 299 to 307 ns | MET |
| ScalarBaseMult JIT 0.70 to 0.82 ms | 762 to 834 us (process medians 762, 777, 834; their median 0.78 ms) | MET, one process 14 us above the band's top |
| ScalarBaseMult ReadyToRun 1.30 to 1.55 ms | 1.42 to 1.47 ms | MET |
| ScalarMult ReadyToRun 4.2 to 5.3 ms | 4.46 to 4.58 ms | MET |
| P-256 handshake ReadyToRun 34 to 44 ms | 38.2 to 39.6 ms | MET |
| the default handshake within 3% | 0.989 (JIT), 0.985 (ReadyToRun) | MET |
| PerfTlsHandshake within 3% | not run as its own program; the default arm IS its handshake (its certificate and configuration) | MET through the default arm, stated |
| exactly `p256_fiat64.cs` and fiat `package_info.cs` move, identical on three targets | exactly those two, identical (§8) | MET |
| Falsifier: JIT Mul above 0.30 us or ReadyToRun Mul above 0.45 us | 232 / 307 ns at most | HELD |
| Falsifier: any fiat P-256 method in the ReadyToRun run-time JIT list | none | HELD |
| Falsifier: any BigInteger or Go<->C# byte mismatch | none (§9), and crypto/tls's TestBogoSuite matched in full (§10) | HELD |
| Falsifier: any other corpus file moves, or the targets differ | none | HELD |
| Falsifier: a conditional jump on a limb value in either listing | none (§6) | HELD |
| Falsifier: the source pin not red on a body edit | red (§9) | HELD |

The ScalarBaseMult JIT row is MET on the median and on two of three processes; the third process read 834 us against the
band's 820 us top, inside the run's own spread (824 to 896 us on the untouched base). It is stated rather than rounded.

## 8. Footprint

Three two-seeded three-target `-stdlib` emissions, one per base the seat stood on, each read the same footprint
([`regen.sh`](data/p256-field/regen.sh), [`emit-summary.txt`](data/p256-field/emit-summary.txt) for the last):
- on `e068b368a6` (the P-384 seat before its re-derive), on `8a4aaba56f` (the P-384 tip) and on master `31dc1870cb`
  (P-384 landed, 235-238), each with the red-first commit's converter as the base arm and the seat's as the cut;
- every time: rootA and rootB differ in exactly `fiat/p256_fiat64.cs` (+2/-573, the two bodies become placeholders) and
  fiat's `package_info.cs` (+1/-1, its `GoPositionMap` record re-encodes), each identical on windows, linux and darwin;
- the base arm rewrote 0 corpus files and equals the committed files; the cut arm rewrote exactly the two; 0 DYNTYPE;
- the last two emissions produced byte-identical files (same hashes), so 235-237's converter changes do not reach fiat.

## 9. Tests and controls

- **GolibTests `P256FieldDifferentialTests`** against BigInteger: every ordered pair of 11 limb-pattern edges (0, 1,
  2, p-1, p-2, R mod p, R^2 mod p, 2^64-1, 2^192-1, p mod 2^192, 2^255) and every edge squared; 20,000 seeded random
  pairs; the aliased forms; and `RawLimbsAreFullyReduced` on edges whose pre-reduction t lands in [p, 2p) (26 Mul pairs
  and 4 squares do; it fails as VACUOUS below 4 and 1). 4 of 4 green against the converted bodies (the oracle) and
  against the hand-own.
- **Behavioral `P256CurveBytes`**: P-256 ECDH (four key pairs), an ECDSA public key from a fixed private key, a fixed
  signature and two tampered inputs, and `crypto/elliptic` base and point multiplies, byte for byte against Go; green on
  all four phases before and after. Minted before B3 and re-minted on each base by `UpdateTestTargets --only`: the one
  `range(slice<T>([...]))` line B3 moved in every curve golden, nothing else.
- **Planted defect A** ([`plantA256-golib.log`](data/p256-field/plantA256-golib.log),
  [`plantA256-beh.log`](data/p256-field/plantA256-beh.log)): round 0's quotient digit `m = t0` planted as `m = t0 ^ 1`.
  The differential failed 4 of 4 and the behavioral test failed its Output phase.
- **Planted defect B** ([`plantB256-golib.log`](data/p256-field/plantB256-golib.log)): the final select skipped
  (`keepD = 0`). All four differential tests failed, as on P-384. Both plants restored md5-identical.
- **repoguard `TestHandOwnedReimplementationsMatchPinnedGoSource`** pins go1.26.10's `p256Mul` (`fd863357…`) and
  `p256Square` (`9ca197b4…`); the hasher reproduces the four existing pins. Red before the hand-own existed, naming the
  missing file. A copy of `p256_fiat64.go` with one literal changed in `p256Mul`'s body hashes to `9c52d000…`, and one
  with only a doc-comment line added keeps the pin.

## 10. Gates

At `933db85492` on master `31dc1870cb`, serially, alone on the box ([`union-all.sh`](data/p256-field/union-all.sh)):

| gate | result |
|:--|:--|
| `go test ./...` (src/go2cs) | rc 0 (the provenance re-cite and census re-derive applied beforehand, §8) |
| GolibTests Release | 2,419 passed, 0 failed, 32 skipped |
| behavioral `P256CurveBytes`, `P384CurveBytes`, `P521CurveBytes` | 4 of 4 phases each |
| `go2cs.slnx` Debug | 0 errors |
| `check-no-regression.ps1` | NO REGRESSION: byte-identical across all 968 behavioral packages (8 platform-exclusive skipped) |
| sweep `crypto/internal/fips140/nistec` | PASS 44 of 44 (135 s) |
| sweep `crypto/ecdh` | PASS 47 of 47 (85 s) |
| sweep `crypto/ecdsa` | PASS 125 of 125 (144 s) |
| sweep `crypto/internal/fips140test` | PASS 2,270 of 2,270 (388 s) |
| sweep `crypto/tls` | COUNT, not a divergence: 4,920 matched against the banked 1,393 (1,133 s) |

**The crypto/tls row, read whole.** The comparison record reads `status: validated`, `matched: true`, 0 errors: every
verdict matched Go. The count is ABOVE the floor because `TestBogoSuite` ran on this host in this run (it did not in the
P-384 seat's battery on the same box, which read PASS at 1,393), and the gate's capability checks are written for a
shortfall, not a surplus. Why BoGo became reachable here is not this seat's question and is not measured; what the row
does show is that the BoGo suite, which exercises P-256 ECDH and ECDSA heavily, matches Go in full with the hand-own.

Every sweep left 0 tracked deletions. The sweeps after nistec left content drift in their own converted `*_test.cs` and
test `package_info` files, crypto/tls's README badge and its proof page (34 files, none in fiat, no production source):
the committed test emission's staleness and the BoGo-reachable count, not this seat; all restored from HEAD by list.

## 11. osx-arm64 under ReadyToRun (C1, 2026-11-08, appended)

COORD's ruling of 2026-11-08 (the 1.26.10.3 re-bank's darwin leg): the fiat multiply judged constant time per RID, with
this record's win-x64 listings as the reference shape.

**Method.** crossgen2 10.0.14 (`microsoft.netcore.app.crossgen2.linux-x64`), cross-targeting `--targetos osx
--targetarch arm64 -O`, the default version bubble (fiat alone, as a single-file publish compiles it), with
`--codegenopt JitDisasm=<method>` and `JitStdOutFile`. Inputs: fiat built Release for the darwin flavour at master
`dab0675bd`, referenced against its own build output and the osx-arm64 runtime pack 10.0.14
(`microsoft.netcore.app.runtime.osx-arm64`). The listing reads "ReadyToRun compilation" and "ARM64 on Apple". This is
the code a ReadyToRun single-file publish ships for osx-arm64. The tier-1 JIT form needs an arm64 host and is read on
the hosted runner (COORD's ruling, 2026-11-08).

**The body** ([`listings/r2r-osx-arm64-p256Mul.txt`](data/p256-field/listings/r2r-osx-arm64-p256Mul.txt)):
- 4,988 bytes of code (win-x64's [`r2r-final-p256Mul.txt`](data/p256-field/listings/r2r-final-p256Mul.txt): 8,005).
- 72 multiply instructions (`mul` and `umulh` pairs, where x64 has one IMUL).
- 24 calls, all indirect `blr` through ReadyToRun cells, one `ret`. That matches win-x64's 24: half golib
  `array.get_Item` (the limb reads and writes at constant indexes), half cold `materializeValue`, the null-reference
  path, which is where every branch below lands.

**Constant time, checked.**
- The only conditional branches are 12 `cbz`. Each tests `x0` just loaded from `[x19]`, `[x20]` or `[x21]`, the
  three byref arguments' wrapper references (4 limb reads each): a null test, as win-x64's `je`.
- None tests a limb value.
- No `b.cond`, `tbz`, `tbnz` or `cbnz`.
- No `udiv` or `sdiv`.
- No register-indexed load or store.
- No `csel` or `cset`: the final reduction's select is arithmetic.
- win-x64 reads 13 conditional jumps: the same null tests, plus one `jne` for its prolog's frame-zeroing loop,
  which arm64 does not emit.

**Corrected 2026-11-08 (C1), section 11 above.** The branch tally there searched only the dotted spelling `b.cond`, and
the .NET JIT's arm64 listings spell a conditional branch without the dot (`bge`, `ble`, `bhs`). Each ReadyToRun body has
ONE more conditional branch than section 11 says: a `bge` closing the prolog's frame-zeroing loop, which counts `x10`
down from a constant 320 bytes by 64. It is the arm64 form of win-x64's `jne`, so section 11's sentence that arm64 does
not emit that loop is wrong. The verdict stands: the loop's trip count is a compile-time constant, and no branch tests a
limb value. Re-counted with both spellings, the ReadyToRun body's conditional branches are 12 `cbz` and 1 `bge`, and
nothing else.

## 12. osx-arm64 under the JIT (C1, 2026-11-08, appended)

**Method.** The os-matrix `golib-tests` stage's `jit_disasm` input (batch 249) on the hosted osx-arm64 runner, run
42438679073: GolibTests' `P256FieldDifferential` and `P384FieldDifferential` classes (8 tests, all passing) under
`DOTNET_TieredCompilation=0` with `DOTNET_JitDisasm="p256Mul p384Mul"`. That is FullOpts code, the form the corpus's
Release default runs. A tiered program's tier-1 body, which dynamic PGO can shape differently, was not read. The
listing reads "ARM64 on Apple", and it was taken from the job log, which reads it from the host's `JitStdOutFile`.

**The body** ([`listings/jit-osx-arm64-p256Mul.txt`](data/p256-field/listings/jit-osx-arm64-p256Mul.txt)):
- 6,108 bytes of code, 60 `mul`/`umulh`, and 49 calls (the JIT keeps the golib array accessors' slow paths and the
  static-base helper out of line).

**Constant time, checked.** Every conditional branch, by what it tests:
- 12 `tbz` after an `ldapr`: the class-initialization flag of a static base, read once per access site.
- 12 `cbz` on `x0` just loaded from `[x19]`, `[x20]` or `[x21]`: the three byref arguments' wrapper
  references, as in the ReadyToRun body.
- 12 `ble` comparing an array's length with a constant index (0 to 3), and 12 `cbnz` on that array's
  reference right after: the golib array accessor's length check and null test.
- 12 `bhs` comparing a slice offset plus a constant with the array's length (`[ref, #8]`): the bounds check.
- 1 `bge`: the prolog's frame-zeroing loop (a `subs x10, x10, #64` countdown), to a constant bound.
- None tests a limb value. There is no `udiv` or `sdiv`, no `csel` or `cset`, and no register-indexed load of limb
  data. The limb arithmetic is straight-line.
