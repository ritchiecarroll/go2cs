# DATA — linux/arm64 S2: the corpus apply's footprint, predicted file by file

> **STATUS: RECORD (lane G, 2026-11-09).** It is the prediction the S2 seat of
> [DESIGN-linux-arm64-flavour](DESIGN-linux-arm64-flavour.md) (§9) is scored against, written and posted BEFORE the
> apply ran. Measured on base `c45b1a4308` (master batch 279 + S1b `caf0fd60e2` + S4 `463a4d3283`), go1.26.10,
> .NET 10.0.400, by four single-target conversions into four seeded roots and the design's M1b classifier over them.
> The apply's score against it is appended below as a dated block. Amended by dated blocks, never rewritten.
>
> The files: [`delta.tsv`](data/la64-s2/delta.tsv) (every converter-written path that changes),
> [`extra.tsv`](data/la64-s2/extra.tsv) (project files, hand-owned files, the build file),
> [`manifest.tsv`](data/la64-s2/manifest.tsv) (every converter-written path of the applied tree, with the SHA-256 of
> its CR-stripped bytes, its key and its sharing set) and [`handown-moves.tsv`](data/la64-s2/handown-moves.tsv) (the
> hand-owned files that change home).

## 1. The prediction

**271 paths change under `src/core`, in 20 packages. No file changes in place.**

| Class | Paths | Rows |
|---|---|---|
| New at arm64: `linux-arm64/` | 74 | 74 A |
| Move, flat to `amd64/` (arm64 differs or does not emit) | 50 | 50 D + 50 A |
| Move, `linux/` to `linux-amd64/` | 32 | 32 D + 32 A |
| Project file gains its layout block rows | 20 | 20 M |
| Hand-owned, routed by its principal (§2.7) | 4 | 4 D + 4 A |
| Hand-owned, homed by hand (the two stand-ins without a principal) | 2 | 2 D + 2 A |
| `src/core/Directory.Build.props`: the universe check names four targets | 1 | 1 M |

The applied tree holds 2,650 converter-written paths for 2,331 logical files: 1,736 flat, 218 `windows/`, 260 `linux/`,
280 `darwin/`, 50 `amd64/`, 32 `linux-amd64/`, 74 `linux-arm64/`. The base holds 2,576.

Per package (new at arm64 / to `amd64/` / to `linux-amd64/`):

| Package | arm64 | `amd64/` | `linux-amd64/` |
|---|---|---|---|
| crypto/internal/fips140deps/cpu | 1 | 1 | 0 |
| hash/crc32 | 3 | 5 | 0 |
| internal/abi | 1 | 1 | 0 |
| internal/buildcfg | 1 | 0 | 1 |
| internal/bytealg | 3 | 3 | 0 |
| internal/chacha8rand | 1 | 1 | 0 |
| internal/cpu | 6 | 2 | 2 |
| internal/goarch | 3 | 3 | 0 |
| internal/runtime/atomic | 2 | 2 | 0 |
| internal/runtime/gc/scan | 2 | 4 | 0 |
| internal/runtime/startlinetest | 0 | 2 | 0 |
| internal/runtime/sys | 3 | 3 | 0 |
| internal/runtime/syscall/linux | 2 | 0 | 2 |
| internal/syscall/unix | 4 | 0 | 4 |
| math | 4 | 5 | 0 |
| runtime | 25 | 13 | 12 |
| runtime/race | 0 | 1 | 0 |
| runtime/race/internal/amd64v1 | 0 | 2 | 0 |
| syscall | 8 | 0 | 8 |
| vendor/golang.org/x/sys/cpu | 5 | 2 | 3 |
| **Total** | **74** | **50** | **32** |

Hand-owned rows: `internal/cpu/cpu_x86_impl.cs` to `internal/cpu/amd64/`, `runtime/cputicks_impl.cs` to
`runtime/amd64/`, `syscall/linux/syscall_linux_amd64_impl.cs` and `syscall/linux/zsyscall_linux_amd64_impl.cs` to
`syscall/linux-amd64/` (the router, from each principal's emitter set); `internal/cpu/cpu_arm64_process_impl.cs` to
`internal/cpu/amd64/` and `internal/cpu/linux/cpu_arm64_process_linux_impl.cs` to `internal/cpu/linux-amd64/` (by
hand: the names have no principal on purpose, so the router leaves them).

## 2. Against M1b (the design's measurement)

- **Placement is M1b's, exactly:** 48 flat names to `amd64/` in 16 packages, 32 `linux/` names to `linux-amd64/` in 7,
  74 new `linux-arm64/` names; 20 packages gain a folder.
- **50, not 48, move to `amd64/`.** M1b counted Go-source principals. The path count adds the two companions of
  hash/crc32 that move with them: `package_init.cs` and the whole-file hand-own's `crc32_amd64.cs.auto`.
- **20 project files, not the sizing post's 21.** The 21st was a package whose only varying file is a review sibling
  kept flat (next item): it gains no folder, so its project is not rewritten.
- **One review sibling stays flat.** `crypto/internal/fips140/subtle/xor_generic.cs` is a flat whole-file hand-own
  whose `.cs.auto` differs at arm64. The package has no source folder for every flavour's key, so the merge keeps the
  first target's sibling and prints a residual note. No footprint. The hand-own hard-codes one flavour: an S3 site.
- **The hand-owned list differs from M1b's by S4's rename** (`cpu_arm64_impl.cs` is `cpu_arm64_process_impl.cs`) and
  S4's new `internal/cpu/linux/cpu_arm64_process_linux_impl.cs`.

## 3. The stack-append harvest follows the universe

`stackAppendHarvestTargets` is `platformUniverse` (`src/go2cs/stackAppendHints.go`:67), so adding a target adds a
compiler run whose vetoes apply to every flavour's emission. A shared append site that escapes only at arm64 would
change a flat file in place at the three existing targets. That is a route to a non-empty footprint outside the 20
packages, and M1b did not measure it.

**Measured:** four more single-target conversions with the harvest at four targets, each compared path by path with
the three-target arm's emission of the same target.

| Target | Files compared under `src/core` | In one arm only | Bytes differ |
|---|---|---|---|
| windows/amd64 | 7,700 | 0 | 0 |
| linux/amd64 | 7,700 | 0 | 0 |
| darwin/amd64 | 7,700 | 0 | 0 |
| linux/arm64 | 7,739 | 0 | 0 |

The delta is zero at every target, and the four-target arm's `manifest.tsv`, `delta.tsv` and `extra.tsv` are
byte-identical to the three-target arm's. The prediction of §1 stands.

**Liveness.** A harvest whose build fails returns nothing and is skipped silently, so a zero needs a control. Both
arms' converter sources were rebuilt with one added stderr line per harvest run and converted strconv and syscall: the
three-target list made three compiler runs per package, the four-target list four, and the linux/arm64 runs returned
24,273 and 135,979 bytes of compiler output with no error (syscall's linux/amd64 run: 135,679). The fourth run
happens and returns verdicts; they agree with the other targets' at every site the emission reads.

## 4. What the apply must show

- Every changed path under `src/core` is a row of `delta.tsv` or `extra.tsv`, with the predicted status and, for a
  converter-written path, the predicted bytes. The scorer buckets each path as HIT, CONTENT, STATUS, UNPREDICTED or
  MISSING and passes only on all HIT; each bucket was made to fail once on a planted tree before the apply.
- At windows/amd64, linux/amd64 and darwin/amd64 the compiled item set of every package is unchanged by name and
  bytes: a moved file has the same bytes and its new key's cover holds the target.
- Every hand-owned file of the base keeps its bytes: 232 files (those carrying the `[module: GoManualConversion]`
  attribute and the `*_impl.cs` companions), of which exactly the seven of `handown-moves.tsv` change home (the four
  routed, the two homed by hand, and hash/crc32's whole-file hand-own, which moves as its own principal).
- A second apply writes nothing, and each single-target conversion into the applied tree writes nothing.

## 5. Predicted readings at linux/arm64 (compile level, a FLOOR)

The arm64 flavour does not build yet; S3 owns its hand-owned files. Declaration errors mask body errors, so these are
the sites reading the tree predicts, not a ceiling:

- `runtime/hash_impl.cs`:185 reads `cpu.GoIsArm64Process`, defined in the stand-in now homed in `amd64/`.
- `syscall/linux/structclass_linux_impl.cs` (cover linux at both arches) names `NativeTimevalLinuxAmd64`, declared in
  `zsyscall_linux_amd64_impl.cs`, now in `linux-amd64/`.
- The design's M2 members without an arm64 body: `testSPWrite` (runtime), `archLog` and `archHypot` (math).
  `cputicks` dissolves: its hand-own follows its principal to `amd64/`.
- No module initializer runs internal/cpu's `doinit` at arm64: both hand-written initializers are `amd64/` files, and
  the converted `cpu_arm64.cs` reads a `cpu.HWCap` nothing fills. This is S3's (the flavour's own initializer); S4's
  HWCAP reader serves an arm64 process running the amd64-converted linux flavour, which this seat retires on arm64.
- runtime fails to compile, so nearly every package fails behind it; the census counts direct sites.
