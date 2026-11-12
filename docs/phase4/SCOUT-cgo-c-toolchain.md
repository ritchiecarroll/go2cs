# SCOUT — a pinned C toolchain for cgo, provisioned on demand: candidates, measured

> **State: MEASURED (2026-11-10), lane H, for COORD.** Measurement only: nothing here changes the converter, golib,
> the generator or `src/core`. The one piece of code is CI instrument code (COORD ruling C1): the `converter-cgo`
> stage of the OS matrix gained a `;cc=` option that downloads a pinned toolchain, checks its hash, and only then
> hands it to go as `CC` (branch `claude/h-cgo-toolchain-scout`, commit `e0c08fa24c`). The direction comes from the
> owner, quoted in PLAN-cgo-interop §5.1's dated amendment (`claude/coord-cgo-toolchain-direction`, `8bd61cfccc`):
> the default is to auto-provision a pinned C toolchain on demand, with CLI overrides. Every reading below ran on
> GitHub-hosted runners at `bffa39a110` (the scout branch) or `6dfe400637` (cgo Phase 1's exit gate); H itself
> downloaded and ran nothing but release metadata.

## 1. Answer

- **Every candidate reproduces the fixture.** Six C compilers on nine hosted jobs read the Phase 1 fixture the same
  way: 21 C declarations, and the same counts by kind. The only difference is one type name, and it follows the
  TARGET's C headers rather than the compiler. Windows reads `longlong` where linux and darwin read `long` (§4).
- **zig is the recommended default** on all three host systems: one download of 54 to 100 MB per host, MIT
  licensed, signed releases, and a single toolchain that can also target the other systems. It reproduced the
  fixture on windows, linux and both darwin architectures, and it linked the converter's own cgo test binary on
  all four. Its costs are a slower first compile and a download policy that sends automation to mirrors (§3).
- **llvm-mingw is the windows alternative.** It reproduced the fixture too, with a newer clang. But it is 191 MB, it
  targets windows only, and its releases are hashed but not signed.
- **A wrong hash is refused before anything is unpacked.** That was the instrument's red, and it held (§2).
- **The CLI shape** (§6) mirrors how go2cs already provisions Go (`docs/DotnetTool.md`): a resolution order, pinned
  hashes built into the converter, a per-user cache, `toolchain install -from FILE` for offline hosts, plus the two
  overrides the owner named. The one rule of its own is that a reproducible conversion asks for the pin explicitly.

## 2. The instrument, and its red

`converter-cgo`'s filter may end `;cc=zig|llvm-mingw[;sha=HEX]`. The plan refuses each misuse by name: an unknown
toolchain, `sha=` without `cc=`, a `sha=` that is not 64 hex digits, llvm-mingw off windows, and either option on
any other stage. A dry run over 11 cases read 11 of 11 as expected. The new step downloads one archive for the
runner from a pin table and refuses it unless its sha256 matches the pin. `sha=` replaces the pin's hash, and it
exists only so a wrong hash can be shown to refuse. Only a verified archive is unpacked and set as `CC`. A refusal
fails the leg in that step, so the tests never run on an unverified compiler.

**Red, run 42756314004** (linux, `;cc=zig;sha=` set to 64 zeros), predicted before dispatch and MET:

- the plan accepted the options;
- the step downloaded 57,332,648 bytes in 3.9 s and read sha256 `1cbe9df9…4e2026`;
- it REFUSED the archive, naming the hash source as "REPLACED by the plan's sha= option";
- nothing was unpacked, the tests step was skipped, and the run failed.

## 3. The candidates

| | **zig 0.17.0** (`zig cc`) | **llvm-mingw 20261020** (ucrt) | w64devkit 2.11.0 | WinLibs gcc 16.2.0 | MSYS2 |
|:--|:--|:--|:--|:--|:--|
| Hosts it runs on | windows, linux, macOS (x64 and arm64) | windows; linux and macOS builds exist but compile FOR windows | windows | windows | windows |
| Targets | all three systems from any host (`-target`) | windows only | windows | windows | windows |
| Download | win-x64 100.3 MB · linux-x64 57.3 MB · osx-x64 59.3 MB · osx-arm64 54.0 MB | win-x64 190.6 MB | 69 MB | 112 MB (gcc), 189 MB (gcc+llvm) | 43 MB base, then gcc through pacman |
| Archive | `.zip` / `.tar.xz`, built-in `tar` unpacks it | `.zip` | self-extracting `.7z.exe` | `.7z` (needs 7-Zip) or a 424 MB `.zip` | installer, then a package manager |
| Integrity | sha256 in ziglang.org's JSON index, and a minisign signature per archive (key on ziglang.org/download) | sha256 as GitHub's release-asset digest only. No signature asset, and no attestation (the API answers 404) | GitHub asset digest | GitHub asset digest | pacman's own package signatures |
| One pinnable archive | yes | yes | yes, but it must be executed to unpack | yes | no |
| Licence | MIT. Bundled C headers keep their own licences: musl (MIT), glibc (LGPL), mingw-w64, and Apple's darwin headers (APSL 2.0) | the scripts ISC; the toolchain LLVM's Apache-2.0 with LLVM exception, and mingw-w64's | the kit Unlicense; GCC GPL-3.0 with runtime exception | per component (GCC, LLVM, mingw-w64) | BSD-3-Clause installer; per package |
| Measured here | yes, all four hosts | yes, windows | no | no | no |

Notes that decide between them:

- **zig's download policy.** ziglang.org offers no uptime guarantee and asks automated downloads such as CI to use
  its community mirrors. Mirrors are untrusted, so the minisign signature MUST then be checked against the Zig
  Software Foundation's key. The instrument downloads from ziglang.org directly and checks the index's sha256. A
  converter that provisions zig for users should take the mirror list
  (`https://ziglang.org/download/community-mirrors.txt`), try mirrors in random order, and verify the signature as
  well as the pinned hash.
- **zig's darwin headers.** zig 0.17.0 ships Apple's libc headers (`lib/libc/include/any-darwin-any`, 143 entries,
  under the Apple Public Source License 2.0), plus `lib/libc/darwin/libSystem.tbd` and `SDKSettings.json`. Their
  provenance is not stated in the files read here. go2cs would download zig from its own publishers and
  redistribute nothing, as it does with Go. Whether that settles it is for the owner, not this scout.
- **zig moved to Codeberg.** The GitHub repository says "Moved to Codeberg" and was last pushed 2025-11-27, so the
  0.17.0 sources and licences were read from Codeberg.
- **The pins** are at least two weeks old: zig 0.17.0 was released 2026-10-01 (0.17.1 followed on 2026-11-02), and
  llvm-mingw 20261020 on 2026-10-21 (20261104 is newer).
- **w64devkit, WinLibs and MSYS2** were read from release metadata only. None improves on zig or llvm-mingw for
  this use. w64devkit unpacks by executing a downloaded `.exe`. WinLibs needs a 7-Zip extractor or a 424 MB zip.
  MSYS2 cannot be pinned as one archive hash.

## 4. Does it reproduce the fixture? Every reading

The fixture is `src/go2cs/testdata/cgo/phase1/fixture.go`, read through the real driver under `-cgo` by the exit
gate, which logs the recognized names by kind. Each job ran the same 40 tests: 38 passed, the 2 that need a host
with NO compiler skipped, and none failed. The compiler-gated mixed-package test passed everywhere.

| Run | Host | C compiler (`go env CC`) | Provision | `go test` step |
|:--|:--|:--|--:|--:|
| 42752991699 | linux-x64 | gcc 15.2.0 (Ubuntu), preinstalled | — | 38 s |
| 42753284719 | osx-arm64 | Apple clang 17.0.0, preinstalled | — | 52 s |
| 42753284719 | osx-x64 | Apple clang 17.0.0, preinstalled | — | 136 s |
| 42755151815 | win-x64 | gcc 15.2.0 (MinGW-Builds), preinstalled | — | 58 s |
| 42756542895 | win-x64 | zig 0.17.0 (clang 22.1.8) | 10 s | 118 s |
| 42756555677 | win-x64 | llvm-mingw 20261020 (clang 23.1.4) | 5 s | 53 s |
| 42756569166 | linux-x64 | zig 0.17.0 (clang 22.1.8) | 8 s | 33 s |
| 42756583107 | osx-arm64 | zig 0.17.0 (clang 22.1.8) | 13 s | 104 s |
| 42756583107 | osx-x64 | zig 0.17.0 (clang 22.1.8) | 26 s | 182 s |

Provisioning means the download, the hash check and the unpack together. The downloads took 1.6 to 6.1 s; unpacking
took 2.3 to 19.4 s, the most on osx-x64. Every job ended well inside the stage's 15-minute budget; the slowest whole
job was 336 s.

Compared line by line, the nine jobs read exactly **two** sets:

- **Set 1, six jobs** (gcc and zig on linux; Apple clang and zig on both darwin architectures):
  `type: char, double, int, intgo, long, ptrdiff_t, uchar, void, xptr`
- **Set 2, three jobs** (MinGW gcc, zig and llvm-mingw, all on windows):
  `type: char, double, int, intgo, longlong, ptrdiff_t, uchar, void, xptr`

Every other line is identical across all nine jobs:

- the count, `recognized 21 C declarations (5 func, 9 type, 1 struct, 1 enum, 3 int const, 1 float const, 1 string const)`;
- the roles, `1 generated file -> fixture.go; 2 generated by cmd/cgo with no source`;
- `func: CString, GoString, add, free, scale`;
- `struct: struct_x` and `enum: enum_color`;
- `int const: BLUE, CONST, GREEN`, `float const: FCONST` and `string const: SCONST`.

The difference is mingw-w64's `ptrdiff_t`, which is `__int64` (`long long`) under windows' LLP64 data model; LP64
linux and darwin give `long`. It was predicted before the windows readings and matched each time.

**What this fixture can and cannot tell apart.** It separates the two data models. It cannot tell glibc from Apple's
libc, or one zig release from another, because its declarations are too light on headers. Set 1 being identical on
linux and darwin is a fact about this fixture, not about those C libraries.

## 5. Determinism (PLAN §5.2)

- **What the result depends on, measured.** Recognition depends on the target's C headers, not on which compiler
  read them. Three compilers on windows agree, and four on LP64 hosts agree. So under a pinned toolchain the header
  set is part of the pin: zig bundles its headers for every target, and llvm-mingw bundles mingw-w64's. Under a
  host compiler the headers are whatever that host has installed. §5.2's equivalence class (Go version, converter
  version, target platform) closes only when the C toolchain is pinned too.
- **The same pin, run twice:** zig 0.17.0 was run again on linux-x64 (run 42757354520) and win-x64 (run
  42757368504) on fresh runners. Each second run's eight READING lines are identical to its first run's, and each
  downloaded the same bytes with the same sha256 (57,332,648 and 100,268,137). Each read 38 PASS, 2 SKIP and 0
  FAIL. The download time is the only thing that moved: 13.7 s on windows the second time, against 6.1 s.
- **Not settled here:**
  - Whether zig, targeting the native macOS, uses its bundled headers or the host's Xcode SDK when one is present.
    The runners had Xcode installed, and both darwin readings matched Apple clang's, so this fixture cannot tell.
  - Whether `zig cc` output is reproducible across zig releases (only 0.17.0 was read).
- **§5.2's recommendation stands.** Generated bindings and any native artifact are build outputs, not corpus
  content, so byte-identity gates never see them. A pinned toolchain makes them repeatable; it does not make them
  corpus.

## 6. Recommendation, and the CLI shape

**Default toolchain: zig, pinned.** One download per host, signed, cross-capable, and measured to reproduce the
fixture on every hosted system. Pin it with the same machinery as Go's pins: a generator in the style of
`gentoolchainpins` that writes per-host sha256 values from ziglang.org's index into the converter, plus the
minisign public key for the mirrors. **llvm-mingw** is a choosable pin on windows, not the default: it is twice
zig's size, targets windows only, and has no signature.

**Resolution order** when a conversion needs cgo, mirroring Go's (`docs/DotnetTool.md`); the first that applies wins:

1. `-cc CMD`, used exactly as given. Like `CC` it may carry arguments, as `zig cc -target …` does. It never downloads.
2. `CC` from the environment, as `go` itself reads it.
3. The platform's default compiler on `PATH`, the one `go env CC` names (`gcc`, or `clang` on darwin). This is the
   host's own compiler and the host's own headers.
4. The pinned toolchain in the user cache, downloaded on first use: the owner's default when nothing above
   resolves. Its hash is checked against the pin before it is unpacked, as the instrument's red shows.

**Overrides the owner named:**

- **Use a given compiler:** `-cc CMD` (step 1).
- **Refuse to download:** `-cc-download=false`. Step 4 then fails loudly, naming steps 1 to 3 and the offline
  install, never falling back to a silently cgo-off load (§2 Finding 1's failure mode). Go's provisioning has no
  such switch today; this would be new.
- **Choose or pin the toolchain:**
  - `-cc pinned` forces step 4 even when a host compiler exists. That is the reproducible choice, and the one CI
    and any byte-comparing gate should use.
  - `-cc pinned:llvm-mingw` picks the windows alternative.
  - The pin itself is a release property, as Go's is. It moves when the converter does, never per run.

**Commands**, beside the existing `go2cs toolchain` ones:

- `go2cs toolchain install -cc` downloads the pinned C toolchain now.
- `go2cs toolchain install -cc -from FILE` installs from a copied archive, checked against the same hashes.
- `go2cs toolchain env` gains a `CC` line.
- `go2cs toolchain prune` covers C toolchains too.

**Per target**, which only Phase 4 needs: the driver would set `CC` per target load, for example
`zig cc -target x86_64-linux-gnu` for GOOS=linux, because Go reads `CC` only from the environment.
`CC_FOR_TARGET` and `CC_FOR_goos_goarch` apply only when the Go toolchain itself is built (`cmd/dist`). This is
unmeasured (§7).

## 7. What cmd/cgo needs from the compiler, and what was not measured

Read at go1.26.10:

- `go` takes `CC` from the environment and splits it like a shell would (`quoted.Split`), so a value that carries
  arguments works. Every leg here ran `zig cc` that way.
- Unset, it defaults to `clang` on darwin and `gcc` elsewhere (`cmd/go/internal/cfg/zdefaultcc.go`). Since Go 1.20,
  if `CC` is unset and that default is not on `PATH`, cgo silently defaults OFF (`cmd/go/internal/cfg/cfg.go`):
  §2 Finding 1, and the reason the `-cgo` driver asserts `CGO_ENABLED=1`.
- cmd/cgo compiles its probes with `-w -Wno-error -o<tmp> -gdwarf-2 -c -xc`, the target's machine flags
  (`-m64` on amd64, `-arch x86_64 -m64` on darwin/amd64, `-arch arm64` on darwin/arm64), and `-fno-lto`. It reads
  the macros with `-E -dM -xc` (`cmd/cgo/gcc.go`). It then reads DWARF from the object file, so the compiler must
  emit DWARF into ELF, Mach-O or PE objects. zig and llvm-mingw did both on every host measured.
- Building the converter's own test binary with cgo on also makes the compiler its linker. zig linked it on
  windows, linux and both darwin architectures.

**Not measured:**

- the unpacked size on disk;
- cross-target recognition (GOOS other than the host, through `zig cc -target`), which is §5.1's harder half and
  the next arm worth running;
- a header-heavy fixture (`struct stat`, `off_t`, `time_t`) that would tell C libraries apart;
- zig through a community mirror with minisign verification;
- arm64 windows and arm64 linux hosts;
- offline installation from a copied archive;
- zig releases other than 0.17.0.

## 8. Amendment 2026-11-12: `go build` with the pinned zig on macOS needs DWARF off

Found by cgo Phase 2's exit gate (DESIGN-cgo-phase2-pinvoke §11.22), on both macOS RIDs, run 43207288962. It is go
and the C toolchain, before go2cs is involved:

- `go build` of a cgo PROGRAM links externally. On darwin go's linker then runs Apple's `strip -S` on the result,
  unless DWARF is off.
- With the pinned zig 0.17.0 as CC, `strip` refuses the binary: `fatal error: the __LLVM segment too small (less than
  sizeof(struct xar_header))`.
- `go build -ldflags=-w` builds and runs. The gate builds go's side that way on darwin.
- `go list`, which is all a conversion runs, is unaffected: the gate program converted on both RIDs at the same
  runs.

So the pinned zig is sound as the conversion's compiler on macOS, and a user who also builds the Go program with it
there needs `-ldflags=-w`. Not measured: Apple's own clang as CC, and other zig releases.

— H (2026-11-12)
