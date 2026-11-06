# DATA — the P-521 generator-table build: converted fiat arithmetic, root-caused and cured

> **State: MEASURED (2026-11-06), win-x64, one 4-core laptop (H), branch `claude/h-p521-table` from `642621b43f`.**
> Lever 2 of [DATA-bogo-startup.md §8.4](DATA-bogo-startup.md#84-levers-ranked-by-expected-saving-none-implemented),
> ruled by COORD on 2026-11-06 after the prediction in §5 was posted. Sections are appended and superseded, never
> overwritten. Raw readings, the probe and benchmark sources, and the final compiled listings are in
> [`data/p521-table/`](data/p521-table/).

## 1. Why this was measured

crypto/tls's test package evaluates its key variables at start, as Go does, and every BoGo shim launch pays them.
P1 measured 0.26 s of them on the linux box (DATA-bogo-startup §8.2), 0.18 s of it the P-521 ECDSA key, and 164 ms
of that the P-521 generator-table build: ReadyToRun code with no JIT inside. Go builds the same table in about 6 ms.
Lazy initialization is out, because Go evaluates these at start too. So the question was why converted P-521
arithmetic is slow, and what fixes it.

## 2. Host, toolchain, method

| | |
|:--|:--|
| box | H: Intel Core i7-8665U, 4 cores / 8 threads, a laptop. Go's own P-521 multiply read 190 ns in one session and 142 ns in another, so cross-session readings carry about ±25% |
| RID | win-x64 for every arm |
| toolchains | go1.26.10 windows/amd64 with GOTOOLCHAIN=local; .NET SDK 10.0.402, runtime 10.0.13, crossgen2 10.0.13 |
| C# arms | DOTNET_TieredCompilation=0 on both. **JIT**: a Release build, so every method compiles at full optimization. **ReadyToRun**: the BoGo host's publish shape: self-contained, single file, PublishReadyToRun, `--enable-generic-cycle-detection`, no trim |

- **The probe** ([`PerfP521Probe.go.txt`](data/p521-table/PerfP521Probe.go.txt)) parses crypto/tls's own
  `testECDSAPrivateKey`, the P-521 key, with `x509.ParseECPrivateKey`: once cold, once warm, then 50 more times.
  The P-256 test key is parsed first, so one-time ECDSA set-up stays outside the P-521 numbers. The table build is
  read as cold minus warm, minus the JIT time the runtime reports (`System.Runtime.JitInfo`) across the cold parse.
  The C# side is the probe converted by go2cs, with two instrument lines added by hand to the converted file.
- **The A/B** interleaves the arms in every round: 1 warm-up and 15 timed rounds, medians
  ([`ab-final.sh`](data/p521-table/ab-final.sh), [`med.py`](data/p521-table/med.py)).
- **The microbenchmark** ([`bench-Program.cs.txt`](data/p521-table/bench-Program.cs.txt)) calls the converted
  public API directly: `P521Element.Mul`, `Square` and `Add`, `@new`, `P521Point.Add`, `Double` and `ScalarBaseMult`.
  Each reading is the median of 7 rounds after 2 warm-up calls. Go's figures come from its own fiat and nistec
  `benchmark_test.go`, via `go test -bench`.
- **Disassembly**: crossgen2's `--codegenopt JitDisasm` for ReadyToRun and the runtime's `DOTNET_JitDisasm` for
  the JIT. Which methods the runtime JIT-compiles comes from `DOTNET_JitDisasmSummary`.

## 3. Before: where the time goes

Probe, medians of 15 rounds, at the seat's base ([`ab-predict.txt`](data/p521-table/ab-predict.txt)):

| arm | table build | JIT in the cold parse | warm parse |
|:--|--:|--:|--:|
| Go | 6.4 ms | — | 0.57 ms |
| C# JIT | 95 ms | 103 ms, 88 methods | 6.9 ms |
| C# ReadyToRun | 523 ms | 0.8 ms | 36.8 ms |

Microbenchmark ([`bench-base-jit.txt`](data/p521-table/bench-base-jit.txt), [`bench-base-r2r.txt`](data/p521-table/bench-base-r2r.txt), [`go-bench-predict.txt`](data/p521-table/go-bench-predict.txt)):

| operation | Go | C# JIT | C# ReadyToRun |
|:--|--:|--:|--:|
| field Mul | 190 ns | 2.37 us | 13.7 us |
| field Square | 160 ns | 2.45 us | 14.1 us |
| field Add | | 85 ns | 85 ns |
| `@new P521Element` | | 22 ns | 31 ns |
| point Add | | 37 us | 208 us |
| point Double | | 44 us | 199 us |

A point Add is 14 field Mul and 29 Add/Sub. A point Double is 10 Mul, 3 Square and 21 Add/Sub. The table is 132 rows
of 14 Adds and 4 Doubles: 31,152 field Mul plus 1,584 Square. Those multiplies are 452 of ReadyToRun's 523 ms (86%)
and 78 of the JIT's 95 ms (82%). Allocation, boxes and value copies together are under 2%.

## 4. Root cause, from the disassembly

**C1: inlining is refused inside the method (JIT and ReadyToRun alike).**
- The converted fiat `p521Mul` declares 1,006 locals before the converter's tuple temporaries. The JIT will not
  inline into a method past 512 locals.
- So nothing inlines, and both compilers produce the same shape: 1,250 calls per field multiply.
  - 655 are `p521Uint1` implicit conversions, from `(uint64)((p521Uint1)x)`.
  - 314 are `bits.Add64`, 162 `bits.Mul64` and 10 `bits.Sub64`.
  - 99 are the limb wrapper's `get_Item`.
- The method is 41 KB of code with a 17 KB frame. Go compiles the same source to MULQ and ADCQ with no calls.
- Controls: `p256Mul` has 182 locals, and under the JIT it holds 28 `mulx` and calls only on cold throw paths.
  `p384Mul` (460 named locals) inlines part-way and keeps 522 calls.

**C2: ReadyToRun's version bubble (ReadyToRun only).**
- Each assembly is precompiled alone, so its calls into math.bits, golib and CoreLib never inline.
- Under ReadyToRun the callees are worse too: `bits.Add64` calls ValueTuple's constructor, and `bits.Mul64` calls
  `Math.BigMul` and ValueTuple's constructor.
- That nested chain is ReadyToRun's 5.8x over the JIT. Even `p256Mul` keeps 84 `bits` calls and 24 array
  `get_Item` calls under ReadyToRun.

## 5. The prediction, as posted before any fix

**Lever:** a hand-owned body for fiat's `p521Mul` and `p521Square`.
- It is a straight-line nine-limb Montgomery multiply (CIOS) with fiat's exact contract: R = 2^576, and m' = 1
  because p = -1 mod 2^64.
- Its reduction runs through p = 2^521 - 1 as a one-limb shift plus two adds, so the body has 81 products in place
  of 162.
- It is constant time, and Square is the same multiply.

**Files:** a manualConversionFuncs entry, `fiat/p521_impl.cs`, and the regenerated `fiat/p521_fiat64.cs` and fiat
`package_info.cs`.

**Predicted, on H:**
- ReadyToRun table build 523 ms -> 81 to 97 ms, from 32,736 multiplies at 0.30 to 0.80 us plus the 71 ms remainder
  the lever does not touch;
- JIT table build 95 ms -> 25 to 35 ms;
- ReadyToRun warm parse 36.8 ms -> 6 to 10 ms;
- exactly two corpus files move, identical on all three targets.

The prototype ([`bench-Proto.cs.txt`](data/p521-table/bench-Proto.cs.txt)) matched BigInteger on 20,000 random
pairs and ran 300 ns under the JIT ([`jit-proto.txt`](data/p521-table/jit-proto.txt)). It ran 305 ns in the
ReadyToRun host ([`r2r-proto.txt`](data/p521-table/r2r-proto.txt)), but there the runtime had JIT-compiled it,
disclosed at the time as "about 1 to 3 ms of JIT".

**Falsifiers:**
- the ReadyToRun table build is not at or below 120 ms;
- the ReadyToRun field Mul is not at or below 1.0 us;
- any Go<->C# byte difference;
- any other corpus file moves;
- the JIT table build is not at or below 40 ms.

## 6. Found while curing it: ReadyToRun discards a body that uses an instruction-set-dependent intrinsic

COORD's condition was that the hand-owned body be KEPT by the runtime, not JIT-compiled at first call. The first body,
which multiplied with `Math.BigMul`, was not kept: in the BoGo-shaped host the runtime JIT-compiled `p521Mul` at its
first call, 12.8 ms in the probe's cold parse. Five spellings were then measured in the same publish shape
([`variants/`](data/p521-table/variants/)):

| variant | product | precompiled body kept? | JIT in the first ScalarBaseMult | field Mul |
|:--|:--|:--|--:|--:|
| V0 | `Math.BigMul` | no | 12.8 ms (probe) | not read in this shape |
| V2 | `Math.BigMul`, limbs through two non-inlined helpers | no | 7.6 ms | 254 ns |
| V3 | `Bmi2.X64.MultiplyNoFlags` behind `IsSupported`, else four products | no | 17.6 ms | 257 ns |
| V4 | `(UInt128)x * y` | no | 18.7 ms | 703 ns |
| V1 | four 32x32 products | **yes** | 0 | 715 to 762 ns |

- Every rejected spelling uses an intrinsic whose code depends on the CPU's instruction set. The four-product
  spelling uses only IMUL.
- V2 rules out the limb wrapper's inlined `StrongBox` read as the cause.
- The same V0 publish without single file kept the body. Its run-time JIT list holds no fiat method and no `bits`
  method, against 773 methods JIT-compiled overall.
- crossgen2's map shows one X86Base instruction-set fixup, but images whose bodies are kept carry it too, so the map
  does not name the reason.
- **Rooted afterwards by the i9** (COORD's discriminator, 2026-11-06): the carrier is crossgen2's composite mode,
  which the SDK turns on for a self-contained single-file publish. With composite off, or with
  `--instruction-set:x86-64-v3`, the `Math.BigMul` body is kept. The BOARD row of 2026-11-06 carries the readings.
- **V1 landed.** In this host it costs no more at start-up than V0's body plus its JIT. It is about twice as slow
  under the plain JIT, which §7 scores.
- The same rejection already applies to `math/bits.Mul64`, `bits.Mul` and `bits.Sub`, and to fiat's `p256Mul`,
  `p256Square`, `p256FromMontgomery` and `p256ToMontgomery`. Whether the P-256 four share the cause is not known.
  This is the BOARD row of 2026-11-06.

## 7. After: the landed body

**The body** ([`listings/`](data/p521-table/listings/), crossgen2 and the JIT):
- ReadyToRun: 34.7 KB of code. 80 of its 81 products are inlined (320 IMUL, no MUL). Its 47 calls are 18 golib
  `array.get_Item`, 9 limb-wrapper `get_Item`, 18 cold `materializeValue`, 1 `Mul64` and the stack probe.
- JIT: 35.3 KB of code, 75 products inlined (300 IMUL) and 6 left as `Mul64` calls by the inliner's budget. Its
  other calls are 9 wrapper `get_Item`, 18 cold `materializeValue`, and the throw helpers.
- **Constant time, checked in both listings.**
  - The only conditional jumps are the limb wrappers' null tests (`je` to `materializeValue`), their bounds checks
    (`jae`, and under the JIT `jle` on the array's length) and the prolog's fixed-count frame-zeroing loop (`jne`).
  - None tests a limb value. Every index is a constant, and the final reduction is a mask select.

**Probe**, medians of 15 interleaved rounds, base and cut in the same rounds ([`ab-final.txt`](data/p521-table/ab-final.txt)):

| arm | table build | JIT in the cold parse | warm parse | steady parse |
|:--|--:|--:|--:|--:|
| Go | 7.3 ms | — | 0.57 ms | 0.58 ms |
| JIT, base | 93.7 ms | 101 ms, 88 methods | 7.31 ms | 6.78 ms |
| JIT, cut | **29.1 ms** | 92.0 ms, 88 methods | 2.95 ms | 2.41 ms |
| ReadyToRun, base | 520 ms | 0.76 ms, 5 methods | 37.7 ms | 36.5 ms |
| ReadyToRun, cut | **42.6 ms** | 0.75 ms, 5 methods | 3.51 ms | 3.14 ms |

In the cut ReadyToRun host no fiat P-521 method is JIT-compiled at run time
([`probe-r2r-jitsum-final.txt`](data/p521-table/probe-r2r-jitsum-final.txt)).

**Microbenchmark**, 3 processes per arm ([`bench-final-jit.txt`](data/p521-table/bench-final-jit.txt), [`bench-final-r2r.txt`](data/p521-table/bench-final-r2r.txt)):

| operation | Go (this session) | C# JIT | C# ReadyToRun |
|:--|--:|--:|--:|
| field Mul | 142 ns | 618 to 665 ns | 715 to 749 ns |
| field Square | 118 ns | 609 to 659 ns | 721 to 726 ns |
| point Add | | 11.9 to 15.2 us | 15.2 to 15.9 us |
| point Double | | 10.4 to 11.5 us | 13.4 to 13.5 us |
| ScalarBaseMult, warm | 373 to 394 us | 1.86 to 2.04 ms | 2.47 to 2.50 ms |

## 8. The prediction, scored

| predicted | measured | score |
|:--|:--|:--|
| ReadyToRun table build 81 to 97 ms | 42.6 ms | **MISSED**, below the band. The 71 ms "remainder" was assumed constant and was not: measured, the cut build is 42.6 ms against about 23.5 ms of multiplies, so the remainder fell to about 19 ms. Why is INFERRED, not measured: the base body's 41 KB of call-bound code and 17 KB frame plausibly slowed the work around each multiply in a way the field-level microbenchmark could not see |
| ReadyToRun saving 426 to 442 ms | 477 ms (520 -> 42.6) | **MISSED**, above the band, for the same reason |
| JIT table build 25 to 35 ms | 29.1 ms | MET |
| ReadyToRun warm parse 6 to 10 ms | 3.51 ms | **MISSED**, below the band |
| ReadyToRun field Mul 0.30 to 0.80 us | 0.72 us | MET, at the band's top end, because the kept body cannot use MUL (§6) |
| about 1 to 3 ms of JIT if the runtime rejects the body | 7.6 to 18.7 ms for every rejected spelling; 0 for the landed one | **MISSED** as a disclosure. COORD's condition removed it rather than letting it ride along |
| exactly `p521_fiat64.cs` and fiat `package_info.cs` move, identical on three targets | exactly those two, identical on windows, linux and darwin (§9) | MET |
| Falsifier: ReadyToRun table build at or below 120 ms | 42.6 ms | HELD |
| Falsifier: ReadyToRun field Mul at or below 1.0 us | 0.72 us | HELD |
| Falsifier: no Go<->C# byte difference | none (§10) | HELD |
| Falsifier: no other corpus file moves | none | HELD |
| Falsifier: JIT table build at or below 40 ms | 29.1 ms | HELD |

Scaled to P1's linux box by its 164 ms, the cut table build would be about 13 ms, saving about 150 ms per BoGo
launch. That is an ESTIMATE across boxes; P1's box is the one to measure it.

## 9. Footprint

A two-seeded three-target `-stdlib` emission ([`emit-summary.txt`](data/p521-table/emit-summary.txt)):
- the arms were the converter at `642621b43f` and at `5c94886d29`, both roots seeded from `5c94886d29`, run serially;
- each arm staged 5,994 `.cs` across windows, linux and darwin;
- rootA and rootB differ in exactly `fiat/p521_fiat64.cs` (+3/-3,046: the two bodies become placeholders) and
  fiat's `package_info.cs` (its `GoPositionMap` record re-encodes). The staged copies are identical on all three
  targets.

The gates also held:
- the base arm reproduces the committed files byte for byte;
- 0 of 335 marked hand-owns were written over in either arm;
- 0 DYNTYPE markers.

## 10. Tests and controls

- **GolibTests `P521FieldDifferentialTests`** compares against BigInteger:
  - every ordered pair of 11 limb-pattern edges: 0, 1, 2, p-1, p-2, R mod p, R^2 mod p, 2^64-1, 2^512-1, the
    all-ones pattern with top limb 0x1fe, and 2^520;
  - every edge squared;
  - 20,000 seeded random pairs;
  - the aliased forms the point formulas use.
  It was 3 of 3 green against the converted fiat bodies, which set the oracle, and is 3 of 3 against the hand-own.
- **Behavioral `P521CurveBytes`** compares the following with Go, byte for byte:
  - P-521 ECDH public keys and shared secrets;
  - an ECDSA public key from a fixed private key;
  - verification of a fixed signature and of two tampered inputs;
  - `crypto/elliptic` base and point multiplies.
  It is green on all four phases before and after.
- **The planted defect.** Round 0's `m << 9` was planted as `m << 8`. The differential failed 3 of 3 and the
  behavioral test failed its Output phase. The file was then restored, md5-identical.
- **repoguard `TestHandOwnedReimplementationsMatchPinnedGoSource`** pins the SHA-256 of go1.26.10's `p521Mul`
  (`542c7f42…`) and `p521Square` (`c72e1a29…`). It was red before the hand-own existed. Its positive control,
  `TestHandOwnSourcePinScannerFires`, reports a changed body and a missing function, and ignores a doc-only change.
- The `-tests` rows and the gate battery are in §11.

**Amended 2026-11-06 (G's review, non-blocking note, taken before landing).** The three tests above read values
through `Bytes()`, so a result that is congruent but not fully reduced would pass them.
`RawLimbsAreFullyReduced` reads the raw Montgomery limbs (field `x`, by reflection) and requires exactly
`(a * b * R^-1) mod p`. Its inputs reach a pre-reduction t in [p, 2p), which happens exactly when y * R < a * b:
- limbs p - 1 times p - 2^55, since R is congruent to 2^55 mod p, so y = 1;
- limbs p - 2^288 squared, since (2^288)^2 = R, so y = 1.
It counts the cases it reaches and fails as VACUOUS below 4 Mul pairs or 1 square. Planting a skipped final select
(`keepD = 0`) failed it with raw limbs p + 1 where 1 was expected. The other three tests stayed green under that
plant, which is G's point. The file was restored md5-identical.

## 11. Gates

The battery ran serially at `fe61651694`, alone on the box, with the tree clean:

| gate | result |
|:--|:--|
| `go test ./...` (src/go2cs) | 3 packages red, all three this seat's own bookkeeping, fixed below |
| `check-no-regression.ps1` | NO REGRESSION: byte-identical across all 962 behavioral packages (8 platform-exclusive skipped) |
| `go2cs.slnx` Debug | 0 errors |
| sweep `crypto/internal/fips140/nistec` | PASS 44 of 44 banked (233 s) |
| sweep `crypto/ecdh` | PASS 47 of 47 (68 s) |
| sweep `crypto/ecdsa` | PASS 125 of 125 (118 s) |
| sweep `crypto/internal/fips140test` | PASS 2,270 of 2,270 (272 s) |
| sweep `crypto/tls` | PASS 1,393 of 1,393 (1,232 s) |

Every sweep left 0 tracked deletions. The four packages after nistec left content drift in their own converted
`*_test.cs` and test `package_info` files (25 files, all test files, none in fiat). That is the committed test
emission's staleness against the base converter, not this seat, and it was restored from HEAD.

The three reds, each fixed in this branch and re-run green:
- `TestProjitemsRegistersEveryGoSource`: the new pin test was not listed in `go2cs-src.projitems`.
- `TestDocsProvenanceNotesResolve` and its two controls: the new list entry in `manualTypeOperations.go` moved
  the `math/bits` entry 20 lines, so two notes cited it at `:2778`. They now cite `:2798`.
- `TestCommentParityCensus`: `eol:split` 26 to 25 and `top:go-none-cs-blank` 1,069 to 1,068, re-baselined. This
  is not lost comments. The census skips any `.cs` whose text contains `GoManualConversion`, and a displacement
  placeholder's text does, so the whole of `p521_fiat64.cs` left the census (1,727 to 1,726 files, 46,711 to
  46,695 Go comment groups). 124 converted production files are already hidden from the census by their
  placeholders, and none of them is a hand-own. That instrument finding is filed on the BOARD.

The hand-own header was corrected afterwards to name the composite carrier (§6), comment only. fiat builds with
0 errors after it.

— H (2026-11-06)
