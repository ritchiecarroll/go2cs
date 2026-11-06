# The runtime's AES hash on an arm64 process — a design scout

> **STATUS: PROPOSED (C1, 2026-11-06; branch `claude/c1-aes-trio-scout` from master `4aebdadc1a`), design only.**
> COORD dispatched this scout for the AES trio on osx-arm64, recorded in the BOARD's 2026-11-05 GOARCH row
> (amendment of 2026-11-06). There is no implementation until COORD rules. The recommendation is
> [option 3](#option-3-recommended-gos-arm64-branch-for-an-arm64-process). The rulings it asks for are listed in
> [Open questions](#open-questions).

## The divergence

The corpus is converted once for `darwin/amd64`, and both mac RIDs run that one build. On osx-arm64, with the oracle at
arm64 (run 42138564497), three runtime verdicts differ and no oracle choice moves them:

| Test | Go (native arm64) | C# (osx-arm64) |
|:--|:--|:--|
| TestMemHash32Equality | skip | pass |
| TestMemHash64Equality | skip | pass |
| TestMemHashGlobalSeed/aes | pass | skip |

Go's `alginit` (`alg.go`) installs the AES hash in two branches: amd64/386 when `cpu.X86.HasAES && HasSSSE3 &&
HasSSE41`, and arm64 when `cpu.ARM64.HasAES`. Apple silicon has AES, and `cpu_arm64_darwin.go`'s `osInit` sets
`ARM64.HasAES = true` outright, so Go on these machines always takes the arm64 branch.

The converted `alginit` (`src/core/runtime/alg.cs:439`) never runs, because schedinit never runs. The selection that
actually runs is the hand-owned `initHashAlgorithm` (`src/core/runtime/hash_impl.cs:156`). It tests the x86 branch
only:

```csharp
if ((GOARCH == "386"u8 || GOARCH == "amd64"u8) && cpu.X86.HasAES && cpu.X86.HasSSSE3 && cpu.X86.HasSSE41 &&
    System.Runtime.Intrinsics.X86.Aes.IsSupported && System.Runtime.Intrinsics.X86.Sse41.IsSupported) {
```

On an arm64 process every `cpu.X86` flag is false, because `initX86FeatureDetection`
(`src/core/internal/cpu/cpu_x86_impl.cs`) returns at once when `X86Base.IsSupported` is false. `cpu.ARM64` is never
filled by anything, so the converted runtime takes the fallback hash. Its AES bodies (`GoAeshash`, `GoAeshash32`,
`GoAeshash64`) are written against `System.Runtime.Intrinsics.X86.Aes` and could not run on Arm in any case.

## Census: the corpus's CPU-feature reads

Taken at `4aebdadc1a` over `src/core/**/*.cs`, the darwin flavour (linux/ and windows/ subtrees excluded),
`internal/cpu` itself excluded.

**`cpu.X86.Has*`: 89 lines in 14 files.** 49 are in `vendor/golang.org/x/sys/cpu/cpu_x86.cs`, which has its own `X86`
struct and is not internal/cpu's. Of the other 40, four are comments (`hash_impl.cs:69`, three in `crc32_amd64.cs`);
the code sites are:

| Consumer | Flags | On an arm64 process today |
|:--|:--|:--|
| `runtime/hash_impl.cs:157` (`initHashAlgorithm`) and `runtime/alg.cs:441-443` (unreached) | AES, SSSE3, SSE41 | fallback hash (the trio) |
| `runtime/cpuflags_impl.cs:40-43` (cpuinit's copy half) and `runtime/darwin/proc.cs:802-805` (the converted cpuinit, unreached) | AVX, FMA, POPCNT, SSE41 | all false |
| `runtime/cpuflags_amd64.cs:29-30` | ERMS, FSRM, AVX | false |
| `runtime/cpuflags.cs:13-17`, `internal/bytealg/bytealg.cs:22-24` | `Offsetof` constants, not reads | n/a |
| `internal/bytealg/index_amd64.cs:14` | AVX2 | false |
| `math/exp_amd64.cs:12` | AVX, FMA | `useFMA` false |
| `internal/runtime/gc/scan/scan_amd64.cs:37-38` | AVX512 set, GFNI | false |
| `crypto/tls/cipher_suites.cs:368` (`hasGCMAsmAMD64`) | AES, PCLMULQDQ, SSE41, SSSE3 | false |
| `crypto/internal/fips140deps/cpu/cpu.cs:53-69` | nine re-exports | **0 consumers** (purego excludes the asm users) |
| `hash/crc32/crc32_amd64.cs` | comments only; the file probes locally | its arm64 arm runs |

**`cpu.ARM64.Has*`: 16 lines outside vendor, 13 of them code** (the other three are comments in `hash_impl.cs` and
`crc32_amd64.cs`).

| Consumer | Flag | On an arm64 process today | Go on arm64 |
|:--|:--|:--|:--|
| `runtime/alg.cs:447` (unreached) | AES | n/a | AES hash |
| `runtime/cpuflags_impl.cs:49`, `runtime/darwin/proc.cs:813` | ATOMICS | not taken: keyed on the constant `GOARCH` | set |
| `crypto/tls/cipher_suites.cs:369` (`hasGCMAsmARM64`) | AES, PMULL | **false** | true |
| `crypto/internal/fips140deps/cpu/cpu.cs:18-26` | five re-exports | 0 consumers | n/a |
| `crypto/subtle/dit_test.cs:25, :59, :92` | DIT | false: three tests skip | from the `hw.optional.arm.FEAT_DIT` sysctl |

Two findings sit beside the trio, in the same class, and are not in it:

- **TLS cipher preference.** On osx-arm64, `hasAESGCMHardwareSupport` is false. The x86 term is false on Arm, and the
  arm64 term reads an unfilled `cpu.ARM64`. So the converted stack prefers ChaCha20-Poly1305 where Go on the same Mac
  prefers AES-GCM. This is the same divergence `cpu_x86_impl.cs` measured on x86 before `cpu.X86` was filled (Go
  0x1301 against converted 0x1303). It is user-visible and unmeasured on osx-arm64.
- **The DIT tests.** The three crypto/subtle DIT tests skip in C# on osx-arm64, where Go runs them.

## The options

### Option 1 (as dispatched): answer `X86.HasAES` from Arm's AES

Map `X86.HasAES` from `System.Runtime.Intrinsics.Arm.Aes.IsSupported` on an arm64 process.

- **Moves nothing on its own.** alginit's x86 branch also needs `HasSSSE3` and `HasSSE41`, and `initHashAlgorithm`
  also tests `X86.Aes.IsSupported`, which is false on Arm. To move the trio, option 1 must also claim SSSE3 and SSE41,
  and drop or Arm-map the intrinsic test.
- **The claims spread.** SSE41 feeds `x86HasSSE41` and `hasGCMAsmAMD64`; `hasGCMAsmAMD64` also needs PCLMULQDQ. Any
  claimed flag that gates a hand-own written against `System.Runtime.Intrinsics.X86` takes that hand-own's x86 path on
  Arm, where it throws `PlatformNotSupportedException`. The AVX2 path in `index_amd64.cs` is the nearest case if the
  mapping ever grows.
- **It contradicts Go.** On arm64, Go's `cpu.X86` is all false, because x86's `doinit` is not built. Option 1 makes
  internal/cpu report x86 hardware that the process does not have.
- **The AES bodies still need an Arm backend.** That is the same work option 3 does.
- **Footprint:** two hand-owns (`cpu_x86_impl.cs`, `hash_impl.cs`), zero emission. **Not recommended.**

### Option 2: a per-arch darwin flavour (`darwin/arm64`)

Convert the darwin corpus a second time for `darwin/arm64` and ship it under osx-arm64 only. This is the honest
constant: `GOARCH` reads `arm64`, alginit's arm64 branch is the code that runs, and the GOARCH row closes as a whole.

- **Corpus.** `go list std` at go1.26.10 (`CGO_ENABLED=0`, tags `purego,math_big_pure_go`) gives 1,715 production
  files for darwin/amd64 and 1,705 for darwin/arm64. Moving to arm64 adds 34 Go files and 27 assembly files across 16
  packages (both kinds per package: runtime 19, syscall 7, internal/bytealg 6, math 6, internal/cpu 5, vendor
  x/sys/cpu 4, and others), and drops 39 amd64 Go files. Every function those 27 `.s` files define becomes a bodyless
  partial. Each needs a hand-own, as the amd64 set has (`hash_impl.cs`, `crc32_amd64.cs`, `cpu_x86_impl.cs`, ...),
  or it throws. That includes asm_arm64.s's own `aeshashbody`, `memhash32` and `memhash64`.
- **Layout.** L3 has per-OS subtrees but no per-arch dimension. The 31 flat `*_amd64*.cs` files in the shared tree
  would need an arm64 sibling set and an arch axis in the layout, `platformEmit` and every pairing reader.
- **Pack.** `push-nuget.ps1` ships one darwin build under both mac RIDs (`runtimes/osx-arm64/lib/...` holds the same
  bytes as osx-x64). It would need two darwin builds.
- **Bank.** The roster's `darwin:` key covers both architectures, and a row banks only when arm64 and x64 agree
  verdict for verdict. 230 roster lines carry that key. Splitting it means a full osx-arm64 re-bank of every darwin
  row against the new flavour, and seat #8 (a)'s target-arch withdrawal changes meaning.
- **Hash risk:** none beyond the port of asm_arm64.s's schedule, which is Go's own.
- **Footprint:** converter (an arch axis), corpus (a second darwin tree), pack, and every darwin row.
  **A release-scale project. Not recommended for this trio**; it stays shape 2 of the GOARCH row.

### Option 3 (recommended): Go's arm64 branch for an arm64 process

Do on an arm64 process what Go's arm64 build does, in the two hand-owns that already stand in for schedinit's slots.
No `cpu.X86` claim changes, `GOARCH` stays as converted, and nothing is emitted.

**(a) internal/cpu: fill `cpu.ARM64` and register arm64's option table.** This is the arm64 `doinit` plus darwin's
`osInit`. `cpu_x86_impl.cs`'s module initializer already returns at once on a non-x86 process; its Arm counterpart
runs only when `RuntimeInformation.ProcessArchitecture` is `Arm64`.

- It sets the flags Go's darwin `osInit` sets:
  - `AES`, `PMULL`, `SHA1` and `SHA2` are unconditional, as Go sets them.
  - `ATOMICS`, `CRC32`, `SHA512`, `SHA3` and `DIT` come from the same `hw.optional.*` sysctls Go reads. Go's
    `sysctlEnabled` is in `cpu_arm64_darwin.go`, which is not converted, so the hand-own calls `sysctlbyname`
    directly. Only darwin ships an arm64 process; a linux-arm64 or windows-arm64 process would need its own OS arm.
    Neither is shipped.
- It registers `cpu_arm64.go`'s option table (aes, pmull, sha1, sha2, sha512, sha3, crc32, atomics, cpuid,
  isNeoverse) in place of the x86 table. Off Windows it then applies GODEBUG through the existing `processOptions`, so
  `GODEBUG=cpu.aes=off` clears `ARM64.HasAES` as Go's does. TestIssue66841 and TestMemHashGlobalSeed/noaes re-exec
  with exactly that knob to reach the fallback.

**(b) runtime: alginit's arm64 branch in `initHashAlgorithm`.** It runs when the process is Arm64 and `cpu.ARM64.HasAES`
is set, guarded by `System.Runtime.Intrinsics.Arm.Aes.IsSupported`. It fills `aeskeysched` with 128 CSPRNG bytes, as
the x86 branch does, and sets `useAeshash`. The AES bodies gain an Arm backend.

`hash/crc32/crc32_amd64.cs` is the precedent: an arm64 arm inside the amd64 flavour, chosen by the process, with x86
keeping precedence, so an x64 process is unchanged.

**Which schedule.** Go's arm64 `aeshashbody` (asm_arm64.s) is a different schedule from amd64's. The key is random
per process, so no Go program and no test can tell the two schedules apart; what they can observe is the
skip/pass pattern and hash quality. Two shapes:

- **3-i (recommended): keep amd64's schedule on Arm.** x86's `AESENC s, k` is `MixColumns(ShiftRows(SubBytes(s))) ^ k`.
  Arm's `AESE s, k` is `ShiftRows(SubBytes(s ^ k))`, and `AESMC` is `MixColumns`. So `AESENC(s, k)` equals
  `AESMC(AESE(s, 0)) ^ k`, which is `Arm.Aes.MixColumns(Arm.Aes.Encrypt(s, zero)) ^ k`. Only `aesSelf` and the three
  `Aes.Encrypt` call sites change, behind one helper. The existing bit-reproducibility guard
  (`src/tests/GolibTests/RuntimeAesHashTests.cs`, a table taken from Go amd64) then holds unchanged on an osx-arm64
  host. It is the one guard that can show the Arm backend matches Go bit for bit.
- **3-ii: port asm_arm64.s's schedule.** This is the most literal "do what Go on arm64 does". It needs a second guard
  table taken from Go on an arm64 Mac, and a second body family to keep. It changes no verdict that 3-i leaves
  unchanged.

**Footprint:** two hand-owned files in converted packages (`src/core/internal/cpu/cpu_x86_impl.cs`, or a new
`cpu_arm64_impl.cs` beside it, and `src/core/runtime/hash_impl.cs`), plus GolibTests guards. Zero converter change,
zero emission, golib untouched.

**What moves on osx-arm64 (predicted, unmeasured):**

| Row | Move | Toward Go? |
|:--|:--|:--|
| runtime | the trio matches: MemHash32/64Equality skip, GlobalSeed/aes passes. TestIssue66841 now re-execs with `cpu.aes=off`, as Go does. The Smhasher family runs under AES, as Go's does. | yes |
| crypto/tls | `hasGCMAsmARM64`, and with it `hasAESGCMHardwareSupport`, become true: AES-GCM preferred, as Go prefers it on the same Mac | yes, and user-visible |
| internal/cpu | the option table is arm64's 10 entries, where today it is empty on Arm. TestDisableAllCapabilities and TestAllCapabilitiesDisabled now exercise real options. Verdict count expected unchanged. | yes |
| crypto/subtle | **only if `HasDIT` is filled** (Q2): TestWithDataIndependentTiming and the other two DIT tests go from skip to **fail**. `internal/runtime/sys/no_dit.cs` reports DIT unsupported, so `WithDataIndependentTiming` cannot enable it. | an honest red against a skip that hides it |
| everything else | `runtime.x86HasAVX` and its group, `math.useFMA`, bytealg, gc/scan: unchanged, since no `cpu.X86` flag moves. osx-x64, linux and windows are unchanged, since the arm64 arm is not taken. | n/a |

**Hash and seed risk.** The seed is per-process CSPRNG on both branches, as Go's is. TestMemHashGlobalSeed's
cross-process property rests on it and is unchanged. 3-i's bit-exactness is checkable on the mac leg against the
existing Go table. A wrong `AESE`/`AESMC` composition would fail that guard on osx-arm64 by name; it would not show
up as a weak hash. Smhasher wall time on arm64 under AES is unmeasured; it is a figure to read on the first
osx-arm64 run, not to predict.

**Its limit.** It fixes alginit's selection, not `runtime.GOARCH`. It is the narrow, process-keyed instance of the
GOARCH row's shape 1, for one consumer whose Go behaviour depends on the architecture. The row stays OPEN for
`GOARCH` itself and for every compile-time-folded `GOARCH ==` branch the shape-1 census will find.

## Recommendation

**Option 3 with schedule 3-i.** It moves the trio to Go's verdicts, and the TLS preference with it. It claims nothing
the process lacks, emits nothing, and its one risk (bit-exactness of the Arm AES composition) is caught by a guard that
already exists. Option 1 cannot move the trio without claiming x86 hardware on Arm. Option 2 is right in the end, but
it is a release project, and the trio is too small a reason to start it.

## Open questions

- **Q1. Scope of (a).** Fill all of Go's darwin `osInit` set (recommended: nothing is left out for being unreached),
  or only `AES` and `PMULL`, the two flags whose consumers move a verdict here?
- **Q2. `HasDIT`.** Go fills it from `hw.optional.arm.FEAT_DIT`. Filling it turns three crypto/subtle skips into
  reds, because DIT is not implemented (`no_dit.cs`). Fill it and record the reds as a named OPEN gap (recommended:
  a skip that hides an unimplemented feature is the worse reading), or hold it false with a BOARD note?
- **Q3. Schedule.** 3-i (amd64's schedule on Arm, the existing guard) or 3-ii (asm_arm64.s's schedule, a new guard
  table from an arm64 Mac)?
- **Q4. Freeze class.** The change is in two hand-owned files under `src/core`, with zero emission. Does it gate
  as a golib-class seat (GolibTests plus the osx-arm64 runtime, crypto/tls and internal/cpu rows), or as a corpus
  seat?
