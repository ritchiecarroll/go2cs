# DESIGN — a linux/arm64 standard-library flavour

> **STATUS: DESIGN, for ruling (lane G, 2026-11-09; branch `claude/g-la64-design`, cut from master `0bbe3ab6e5`).**
> The owner ruled a linux/arm64 flavour into `.4` ("also in .4 arm64 on Linux -- OK with date slip", relayed by COORD
> 2026-11-09). This note sizes it. Its input is P2's census of the linux and windows flavours
> (`CENSUS-goarch-linux-windows.md`, `claude/p2-goarch-census`), whose verdict for linux is NO for a run-time GOARCH:
> the flavour's syscall numbers and struct layouts are linux/amd64's, so it cannot run on linux/arm64 whatever
> `runtime.GOARCH` says.
>
> Every number below is MEASURED here (M1, M2, the census), CITED at file:line, or labelled an ESTIMATE.
> Amended by dated blocks, never rewritten.

## 0. The standing defect this closes, and the first seat (SEAT 0)

Today a linux-arm64 consumer silently compiles against, and LOADS, the Windows flavour of every RID-split go.*
package. This is the BOARD entry of 2026-11-09, "a linux-arm64 consumer silently compiles against and LOADS the
Windows flavour":
- `go.lib.targets` swaps a compile item to `runtimes/<rid>/` only when that file exists. On linux-arm64 nothing is
  swapped (`src/core/golib/buildTransitive/go.lib.targets`:38-41).
- At run time the host prefers a `runtimeTargets` asset only for a matching RID, so `lib/`, the Windows reference
  flavour, loads (`src/push-nuget.ps1`:1573-1576).
- The only signal is one stderr line from `GoHostOfRecord.Verify` (`src/core/golib/GoHostOfRecord.cs`, class comment
  18-26; called from `src/core/runtime/hostofrecord_impl.cs`:42-44).

**SEAT 0, "refuse loudly" (independent of the flavour; ships in .4 even if the flavour slips).**
- **What:** a build-time ERROR in `go.lib.targets`, when a go.* package is RID-split and the consumer's RID has no
  `runtimes/<rid>/` asset for it.
  - The RID is `$(GoCompileRuntimeIdentifier)`, which already resolves `$(RuntimeIdentifier)`, then
    `$(NETCoreSdkRuntimeIdentifier)`; that is :44-46.
  - The error names the RID, the first package, and KnownIssues.
- **Where:** beside `GoSelectRidCompileAssets` (:49-75), which already walks every go.* compile item and knows whether
  `runtimes/$(GoCompileRuntimeIdentifier)/` exists. The check is that walk's else-branch, counted, failing once.
- **The opt-out, argued:** `GoAllowReferenceFlavor=true`, documented for one honest case. A consumer cross-building
  for a shipped RID from an unshipped host sets `RuntimeIdentifier`, and so never hits the error; the opt-out is for
  someone who knowingly wants the reference flavour on an unshipped RID (for example, a test harness that only
  compiles). Without an opt-out, the error would block a compile-only use the error itself cannot tell apart.
  Recommended: the opt-out exists, and its doc says the program is then the Windows flavour.
- **What it does not cover:** a portable app built on a shipped RID and RUN on linux-arm64. Build time cannot see the
  run host. GoHostOfRecord's warning stays the run-time signal. Making it fatal is a separate ruling: it would also
  fail every unshipped-RID test harness.
- **Gate:** `src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1` gains an arm. A consumer with
  `-r linux-arm64` must FAIL to build, naming the RID; with the opt-out it must build. Red first: today it builds.
- **The flavour seat (S6) turns it into a no-op** for linux-arm64 by shipping `runtimes/linux-arm64/`, and the
  same check then guards the next unshipped RID (linux-musl-x64, win-arm64).
- **Size: about 0.5 lane-day.** Any lane with PackageTests; it needs no converter or corpus knowledge.

## 1. What a flavour is today

- **One conversion per GOOS, at amd64.** `go2cs -stdlib -platforms <goos>/amd64` writes the varying files of a package
  into `<pkg>/{windows,linux,darwin}/` (layout L3). Every L3 `.csproj` compiles `$(GoTargetOS)/*.cs` and defaults
  `GoTargetOS` to windows (for example `src/core/syscall/syscall.csproj`:179-181, :198, :203).
- **No architecture axis exists.** "There is no GoTargetArch override to honor -- none exists anywhere in the tree"
  (`src/_paths.ps1`:60-61). The converter's `-recurse=nuget` RID map sends linux/arm64 to linux-x64
  (`compileRuntimeIdentifierForTarget`, `src/go2cs/moduleConverter.go`:1048-1063).
- **Packing.** `push-nuget.ps1` builds once per distinct GoTargetOS, under its first RID. It then copies each
  flavour's `lib/` into `runtimes/<rid>/` for every RID mapped to it. `$ridFlavors` (:1725-1730) maps win-x64 to
  windows, linux-x64 to linux, and osx-x64 and osx-arm64 both to darwin. That is how ONE darwin build ships under two
  Mac RIDs. `NugetgoFlavors.psm1`:29-35 carries the same map, and `Test-NugetgoFlavors.ps1`:30-37 holds the two
  equal.
- **Why darwin's arm64 works on one build and linux's cannot.** darwin calls the kernel through libc trampolines,
  by name, so its amd64 and arm64 syscall surfaces agree. Linux calls the kernel by number. P2's layer 1 found that
  linux's `syscall` differs between the arches in `zsysnum_linux_*` (call numbers), `ztypes_linux_*` (kernel struct
  layouts), `zerrors_linux_*`, `zsyscall_linux_*` and `syscall_linux_*`. `internal/runtime/syscall/linux` differs in
  its call numbers, and `internal/syscall/unix` in `sysnum_linux_*`.

## 2. The smallest change: a fourth flavour folder, `linux-arm64/`, with linux fallback

**M1, the footprint (measured 2026-11-09).**
- **Method:** master's converter (`0bbe3ab6e5`, sha256 `a670bffa7b0ca799`), one seed snapshot (4569 `.cs`), and two
  seeded `-stdlib -comments` roots at `-platforms linux/amd64` and `linux/arm64`. Converted: 358 packages at amd64,
  356 at arm64 (the two amd64-only packages, `runtime/race/internal/amd64v1` and `internal/runtime/startlinetest`).
- **83 files differ, +6502 / -139 lines,** in 18 packages:
  - 82 written at arm64: 37 files new at arm64 and 45 changed. The new files are mostly syscall's z-tables
    (+4666), runtime (+640), vendor/golang.org/x/sys/cpu (+435) and internal/cpu (+237);
  - 1 written only at amd64 (`hash/crc32/package_init.cs`).
- **The deletion list:** 44 Go files only amd64 selects (`go list` linux, amd64 vs arm64). A seeded root keeps them,
  so the deletion list comes from the file selection, never from the diff (corpus-reconvert skill, §2).
- **Packages:** syscall, runtime, internal/cpu, internal/goarch, internal/runtime/{atomic,sys,gc/scan,syscall/linux},
  internal/syscall/unix, internal/bytealg, internal/chacha8rand, internal/buildcfg, math, hash/crc32, runtime/race,
  crypto/internal/fips140deps/cpu, crypto/internal/fips140/subtle, vendor/golang.org/x/sys/cpu.
- **The shape it implies.** 18 of 358 packages vary by arch, and only on linux. So the axis is not a full
  `<goos>/<arch>/` tree:
  - **(a) RECOMMENDED: a fourth flavour key, `linux-arm64`.** A package whose linux/arm64 emission differs from
    linux/amd64 gains `<pkg>/linux-arm64/`, holding its COMPLETE arm64 set of varying files, as each L3 folder holds
    its flavour's whole set. Every other package keeps `linux/` for both arches.
  - The `.csproj` template's L3 block selects `$(GoFlavor)/*.cs`, where `GoFlavor` is `$(GoTargetOS)`, or
    `linux-arm64` when `GoTargetOS=linux`, `GoTargetArch=arm64` and that folder exists. Otherwise it falls back to
    `linux/`.
  - That is one new MSBuild property (`GoTargetArch`, default amd64) and one condition. Nothing changes for the
    three existing flavours.
  - (b) A nested `linux/arm64/` overlay, overriding files by name: rejected. It needs per-file `Remove` logic
    MSBuild makes fragile, and an overlay hides which files the flavour really compiles.
  - (c) A full `<goos>/<arch>/` axis everywhere: rejected. It would rewrite 37 L3 packages' layout to serve 18
    packages on one OS.
- **The converter half.** A `-stdlib` run at `-platforms linux/arm64` writes into `<pkg>/linux-arm64/` where its
  emission differs from linux/amd64's. This is the L3 writer's existing flavour comparison with a fourth member.
  `-platforms linux/amd64,linux/arm64` emits both in one invocation.

**The hand-owns (the census, 2026-11-09).**
- M1 moved exactly ONE `.cs.auto` (`crypto/internal/fips140/subtle/xor_generic.cs.auto`, the folded
  `supportsUnaligned`, which is equivalent at arm64).
- Hand-owned companions with no `.cs.auto` are invisible to M1, so they were censused for arch-specific layouts and
  syscall numbers. 8 linux hand-owns touch them:
  - `syscall/linux/zsyscall_linux_amd64_impl.cs`, amd64 by name;
  - `syscall/linux/structclass_linux_impl.cs`;
  - `syscall/linux/sockaddr_linux_impl.cs`;
  - `syscall/linux/cgocaller_linux_impl.cs`;
  - `internal/poll/linux/runtime_netpoll_impl.cs`: epoll, where `EpollEvent` is 12 bytes packed on amd64 and 16 on
    arm64;
  - `internal/runtime/syscall/linux/linux/syscall_linux_impl.cs`;
  - `internal/syscall/unix/linux/net_linux_impl.cs`;
  - `runtime/linux/signal_posix_impl.cs`.
- Each needs a linux-arm64 review, and some need a twin.

**M2, the first compile wall (measured 2026-11-09).**
- **Method:** in this worktree, overlay M1's arm64 emission (82 files), remove the 44 amd64-only files' 45
  conversions, and build runtime, syscall, os and net at `GoTargetOS=linux`. Then restore by status (127 dirt rows
  back to 0, with no tracked file deleted).
- **Linux/amd64 sizes, the reference:**

  | Assembly | Bytes |
  |---|---|
  | runtime | 3,971,072 |
  | syscall | 900,608 |
  | os | 649,728 |
  | net | 1,158,144 |

- **arm64 does not compile yet.** 4 duplicate members (CS0111), each a hand-own colliding with a body Go selects
  only at arm64:
  - runtime's `cputicks` (`cputicks_impl.cs`) and `testSPWrite` (`stubs_impl.cs`);
  - math's `archLog` and `archHypot`.
  Declaration errors mask body errors, so 4 is a FLOOR. This is the hand-own re-audit (H6) for the new flavour.
- **Arm64 sizes are therefore NOT measured.** The ESTIMATE: arm64's syscall z-tables replace amd64's 44 files with
  37 of about the same content, so each assembly within a few percent of linux/amd64's (darwin's are "within ~1%
  of linux's", `DESIGN-multiplatform-corpus.md`:1400-1402).

## 3. Packaging and consumer RID selection

- **`$ridFlavors` gains `'linux-arm64' = 'linux-arm64'`,** a fifth RID with its OWN flavour key. It is NOT
  `'linux-arm64' = 'linux'`, which would copy linux-x64's bytes, as darwin's twins are copied. The build pass becomes
  keyed on (GOOS, GOARCH): `dotnet build -p:GoTargetOS=linux -p:GoTargetArch=arm64` (:1796-1848).
- **The rest of the pack path:**
  - the merge copies its `lib/*` into `runtimes/linux-arm64/` only (:2049, :2067);
  - the post-merge check's shipped set (:2087-2123) gains the RID, and its "twin folders byte-identical" rule must
    not pair linux-x64 with linux-arm64;
  - `NugetgoFlavors.psm1` and `Test-NugetgoFlavors.ps1` move in step, and `Test-GoApiCompat.ps1`:132 gains the RID.
- **Size growth (ESTIMATE, from the recorded per-RID cost):** one RID folder across the 37 L3 packages is about 4.4
  MB compressed, against a set of about 80 MB (`DESIGN-multiplatform-corpus.md`:1400-1402). So linux-arm64 adds
  about 4.4 MB (+5-6%). One more build+pack pass adds about 17 min to a pack.
- **Consumer selection needs no new mechanism.** `GoSelectRidCompileAssets` already swaps to
  `runtimes/$(GoCompileRuntimeIdentifier)/` when it exists (go.lib.targets:49-75). The host already picks
  `runtimes/linux-arm64/` from deps.json's `runtimeTargets`.
- **The converter's `-recurse=nuget` map** (`compileRuntimeIdentifierForTarget`) maps linux/arm64 to linux-arm64,
  and `stdlibMetadata_test.go`:431-439 gains that case.

## 4. runtime.GOARCH on this flavour

- On linux-arm64, `internal/goarch` is the arm64 conversion (`zgoarch_arm64`), so `runtime.GOARCH` is simply the
  converted const "arm64". No displacement is needed.
- The darwin and windows displacement (`manualConversionConsts`, scope `goosWindowsDarwin`, the run-time GOARCH seat)
  is keyed by GOOS, so linux is out of scope at both arches. It cannot misfire: `TestAManualConstIsDisplacedOnlyOnItsTarget`
  drives the emission per target, and its linux arms assert the const is emitted as converted.
- Seat S1 adds a linux/arm64 target to that test, so a future scope widening that reaches linux shows up there.

## 5. cpu.ARM64 from HWCAP

- **The gap.** Go's linux/arm64 `internal/cpu` reads its features from `cpu.HWCap`/`HWCap2`, which the runtime fills
  from the auxiliary vector (AT_HWCAP, AT_HWCAP2) in `sysargs`. Here `schedinit` never runs (`cpuflags_impl.cs`), so
  nothing fills them, and every `cpu.ARM64` flag stays false. The consumers: the AES hash (`hash_impl.cs`'s arm64
  branch), and crypto's `hasGCMAsmARM64` (AES-GCM preference), as darwin's
  `internal/cpu/cpu_arm64_impl.cs` records.
- **The seat (S4).** A linux osInit for an arm64 process, the twin of `darwin/cpu_arm64_darwin_impl.cs`:
  - read AT_HWCAP and AT_HWCAP2 from `/proc/self/auxv` (16-byte entries: tag, value) at module init, keyed on
    `RuntimeInformation.ProcessArchitecture == Arm64`;
  - set `cpu.HWCap` and `HWCap2`, then run the converted `doinit`.
  - The source of truth is the kernel's word, as in Go. .NET's `System.Runtime.Intrinsics.Arm.*.IsSupported` is the
    GolibTests cross-check (an AES-capable host must read HasAES both ways), never the source.
  - Like darwin, HasDIT stays false (no PSTATE.DIT control in .NET; COORD's 2026-11-06 ruling).
- **Size: about 0.5-1 lane-day.**

## 6. CI

- **os-matrix has no arm leg today.** `ubuntu-24.04-arm` appears only in `release-binaries.yml`:275 and :323.
- **The change:** `goos=linux` fans out to two runners, as darwin does (os-matrix.yml:327-333):
  `@{goos='linux'; runner='ubuntu-latest'; rid='linux-x64'}` and
  `@{goos='linux'; runner='ubuntu-24.04-arm'; rid='linux-arm64'}`. Each leg passes `GoTargetArch` from the RID.
  - The oracle needs no `-test-oracle-goarch` there: the flavour's arch IS the host's.
  - release-smoke and aot-smoke gain the fifth RID (:342-349).
- **Walls (ESTIMATES).** These are linux-x64's recorded walls; the arm runner is assumed comparable, to be read at
  the first run.

  | Stage | linux-x64 wall | Source |
  |---|---|---|
  | census | 18 min | os-matrix runs |
  | golib-tests | 10-13 min | os-matrix runs |
  | behavioral-full | 36-62 min | os-matrix runs |
  | sweep-shard, runtime row | 1945 s | run 42686722080 |
  | sweep-shard, runtime/pprof row | 426 s | run 42686722080 |
  | sweep-shard, reflect row | 101 s | run 42686722080 |
  | the whole linux bank | 12.4 h of machine time over 283 rows (mean 158 s) | `rebank-1.26.10.3/linux/*.tsv` |

## 7. The bank: a fourth reading

- **The roster grammar.** The roster records per-OS readings as `linux:` / `darwin:` segments
  (`docs/ValidatedTestPackages.md`:477-488; parser contract :501-540). darwin already carries two arches under one
  key with a per-RID note: `darwin: 7 + 3 (osx-arm64; osx-x64: 10)`.
- **The extension:** `linux: 11094 + 67 (linux-x64; linux-arm64: N + d)`.
  - **`src/_roster.ps1`:** `$RosterOsPattern`'s note group (:98-102) admits `linux-x64|linux-arm64`, and the
    darwin-only refusal (:302-323) becomes a per-key RID set. No new OS key: a hyphenated `linux-arm64` key would
    fail the key grammar (:67) by design.
  - **`check-roster-format.ps1`:** the Linux header derivation (:1334-1361) reads both RIDs.
  - **The bubbles (`DESIGN-roster-per-os-bubbles.md`):** L stays one bubble, rendered from the worse of its two RIDs'
    states. This is the same rule a darwin row with two RIDs needs, so it is stated once for both.
- **The run:** a full validation sweep on ubuntu-24.04-arm. That is about 12.4 h of machine time (ESTIMATE),
  sharded as the hop runbook's §3 campaign does (GoCorpusMigration.md §3.1-3.6).
- **Rows that plausibly differ** (from M1's packages and the darwin osx-arm64 precedent):
  - **syscall, os, net, internal/poll, internal/syscall/unix:** the new call numbers and struct layouts, with the
    8 hand-owns in §2;
  - **runtime:** its arm64 branches, as the run-time GOARCH census read them;
  - **internal/cpu, crypto/*:** HWCAP-gated paths. These go through S4 first;
  - **math and hash/crc32:** arch-selected files (`archLog`/`archHypot`, crc32's arm64 table);
  - **debug/buildinfo, go/internal/gcimporter, net/http/pprof:** the GOARCH-derived set darwin's osx-arm64 met. Here
    they should MATCH by construction, since Go and the flavour agree on arm64.

## 8. The hop-runbook steps it reuses (`docs/GoCorpusMigration.md`)

Adding a flavour is a hop with the Go version held fixed:
- **H5** (seeded full reconvert, :875): every flavour now seeds and converts four times, in four roots.
- **H6** (the hand-own re-audit, :1678): M2's 4-member compile wall and the 8 censused hand-owns are H6's input.
- **H7** (compile parity, :1926): `go2cs-stdlib.slnx` at `GoTargetOS=linux GoTargetArch=arm64`.
- **H8** (multi-platform re-emission, :2177): the L3 writer's fourth member.
- **H10** (roster, proof-page and disclosure re-derivation, :2831): the fourth reading.
- **§3** (the shardable roster campaign, :4143): the linux-arm64 bank's shards.
- **H11** (publication and compatibility guards, :3966): the fifth RID in the pack's guards.

## 9. Seats

Each seat is sized; the lanes are suggestions. The dependencies:
- **Independent:** S0, S4 and S5.
- **The spine:** S1, then S2, S3, S7.
- S6 needs S1 and S2.

| Seat | What | Size (lane-days) | Lane |
|---|---|---|---|
| **S0** | Refuse loudly (§0): the go.lib.targets error, the opt-out, the PackageTests arm, KnownIssues. Ships in .4 alone. | 0.5 | any PackageTests lane (C2) |
| **S1** | Converter: the `linux-arm64` flavour key in the L3 writer, `GoTargetArch` in the csproj template, the RID map, and the emission tests (incl. a linux/arm64 target in the manual-const test). | 2-3 | G or H (converter) |
| **S2** | Corpus: the seeded H5 reconvert with the fourth flavour; apply `linux-arm64/` folders (18 packages, M1) and the deletion list (44 files); H7 compile parity at linux/arm64. | 1-2 | the converter lane, after S1 |
| **S3** | Hand-owns (H6): the 4-member wall (cputicks, testSPWrite, archLog, archHypot), then the masked body errors, then the 8 censused linux hand-owns' arm64 twins (zsyscall, structclass, sockaddr, cgocaller, the epoll netpoller, signal_posix, net_linux, syscall_linux). | 3-5 (the 4 known members are a floor) | H (native seam) with G |
| **S4** | cpu.ARM64 from HWCAP (§5), with its GolibTests cross-check. | 0.5-1 | C1 or H |
| **S5** | CI: os-matrix's linux arm leg, GoTargetArch plumbing, release-smoke and aot-smoke at five RIDs. | 1 | C1 (CI) |
| **S6** | Packaging: `$ridFlavors` fifth RID with its own (GOOS, GOARCH) pass, the merge and post-merge checks, NugetgoFlavors and its test, the ApiCompat fixture. | 1-2 | C2 (packaging) |
| **S7** | The bank: `_roster.ps1` per-RID note for linux, `check-roster-format.ps1`, the bubbles rule; the full linux-arm64 sweep (about 12.4 h machine, sharded) and its banking. | 3-5 | the bank lanes, sharded (P1, P2, C1) |
| | **Total** | **about 12-19 lane-days**, about 2-3 weeks of calendar time with S0, S4 and S5 run beside the spine | |

## 10. Rulings asked

- **R1.** Adopt (a): a fourth flavour key `linux-arm64` with linux fallback, `GoTargetArch` default amd64.
  **Recommended: yes.**
- **R2.** SEAT 0 as specified, with the `GoAllowReferenceFlavor` opt-out. **Recommended: yes, opt-out included.**
- **R3.** The roster extends through darwin's per-RID note form, with no new OS key. **Recommended: yes.**
