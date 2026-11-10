# DESIGN — a linux/arm64 standard-library flavour

> **STATUS: DESIGN, REVISION 2, for ruling (lane G, 2026-11-09; branch `claude/g-la64-design`).**
> The owner ruled a linux/arm64 flavour into `.4` ("also in .4 arm64 on Linux -- OK with date slip", relayed by COORD
> 2026-11-09). Revision 1 (`7831a23193`) was reviewed adversarially before any seat was cut. COORD's rulings on it:
> **R1 REJECTED as written** (a fourth GOOS-style key cannot subtract a flat amd64 file), **R2 and R3 APPROVED WITH
> CHANGES**. This revision replaces §2 with the orthogonal-folder layout rule and a four-target census (M1b), widens
> S1 and S3, and folds in the review's F2-F8. It is one commit on top of revision 1; the unlanded draft is rewritten,
> not amended by blocks, by COORD's leave.
>
> Every number below is MEASURED here (M1, M1b, M2, the censuses), CITED at file:line in this branch's tree, or
> labelled an ESTIMATE or a PREDICTION. **No seat after S0 is cut before §10's rulings.**
>
> **Amended 2026-11-09 (§9, the S2 line):** the escape-registry `IsPerOS` predicate joins S2's scope, by COORD's
> routing of a gap found while cutting S1a. Nothing else in this revision changes.
>
> **Amended 2026-11-10 (§11, new):** what cutting S2 corrected. Class (iii) of §9's S3 row is 18 types in 4
> files; the metadata record is keyed by layout folder; every layout-reading guard reads per target; four
> hand-owned syscall bodies are owed at linux/arm64; the arch binding seats with S2; R1e is not delivered
> (§11.6); six readers outside the converter suite named the GOOS folders themselves (§11.7). The text above §11
> is unchanged.
>
> **Amended 2026-11-10 (§11.8, new):** R1e is delivered by a converter seat on batch 283. The arch-divergent
> review sibling of `crypto/internal/fips140/subtle` is split into `amd64/` and `linux-arm64/`, folders that hold
> nothing else. §11.6 stays as written: it records what S2 seated.

## 0. The standing defect this closes, and the first seat (SEAT 0)

Today a linux-arm64 consumer silently compiles against, and LOADS, the Windows flavour of every RID-split go.*
package (the BOARD entry of 2026-11-09):
- `go.lib.targets` swaps a compile item to `runtimes/<rid>/` only when that file exists. On linux-arm64 nothing is
  swapped (`src/core/golib/buildTransitive/go.lib.targets`:38-41).
- At run time the host prefers a `runtimeTargets` asset only for a matching RID, so `lib/`, the Windows reference
  flavour, loads (`src/push-nuget.ps1`:1573-1576).
- The only signal is one stderr line from `GoHostOfRecord.Verify` (`src/core/golib/GoHostOfRecord.cs`, class comment
  18-26; called from `src/core/runtime/hostofrecord_impl.cs`:42-44).

**SEAT 0, "refuse loudly"** is APPROVED WITH CHANGES (R2) and is being cut by C2. It ships in .4 even if the flavour
slips. As ruled:
- **Its own target, with its own condition.** Not the else-branch of `GoSelectRidCompileAssets`, which runs only when
  `GoRidCompileAssets` is true (:49-51): a consumer with the swap off would skip the error.
- **"RID-split" is decided by `Exists(<pkgroot>/runtimes/)`,** so a platform-neutral package never fires it.
- **RID-graph aware.** The host walks the graph (linux-musl-arm64 to linux-arm64, linux-musl-x64 to linux-x64,
  linux-bionic-arm64 to linux-arm64), so the check resolves through `$(RuntimeIdentifierGraphPath)` or
  ResolvePackageAssets' own resolution. The gate gains an Alpine arm.
- **Same-OS unshipped RID is a WARNING** (win-arm64 compiles the Windows flavour, the right OS); **cross-OS or
  cross-ABI is an ERROR.**
- **The opt-out** `GoAllowReferenceFlavor=true` is a global property, documented as "the program is then the Windows
  flavour".
- **The run-time half:** `GoHostOfRecord.Verify` also takes the flavour's GOARCH and compares it with
  `RuntimeInformation.ProcessArchitecture`. It is vacuous on windows and darwin, whose GOARCH is displaced at run
  time, and loud on an arm64 host that loaded an amd64 linux flavour: the portable-app gap build time cannot see.
- **Gate:** `src/tests/PackageTests/RidCompileAsset/test-rid-compile-asset.ps1` gains the arms. Red first: today a
  `-r linux-arm64` consumer builds.
- **The flavour (S6) turns the error into a no-op** for linux-arm64 by shipping `runtimes/linux-arm64/`.
- **Size: about 1 lane-day** (0.5 as first sized, +0.5 for the review's changes).

## 1. What a flavour is today

- **One conversion per GOOS, at amd64.** `go2cs -stdlib -platforms <goos>/amd64` runs once per target; the merge
  (`mergePlatformEmissions`, `src/go2cs/platformEmit.go`:375) homes each file **flat** when every target emitted the
  same bytes, and otherwise writes one copy per emitting target into `<pkg>/{windows,linux,darwin}/` (layout L3).
- **The project file.** An L3 `.csproj` defaults `GoTargetOS` to windows and compiles `package_info.cs` first, then
  `*.cs`, then `$(GoTargetOS)/*.cs` (`src/core/syscall/syscall.csproj`:179-203). 41 project files carry that block
  today (`grep 'GoTargetOS)'` over `src/core/**/*.csproj`).
- **No architecture axis exists.** "There is no GoTargetArch override to honor -- none exists anywhere in the tree"
  (`src/_paths.ps1`:60-61). `runPlatformEmission` refuses two targets that share a GOOS and names the reason: "The
  GOARCH axis is real ... and is increment 5's subject" (`platformEmit.go`:72-81). This design is that increment.
- **Packing.** `push-nuget.ps1` builds once per distinct GoTargetOS and copies each flavour's `lib/` into
  `runtimes/<rid>/` for every RID mapped to it. `$ridFlavors` (:1725-1730) maps win-x64 to windows, linux-x64 to
  linux, and osx-x64 and osx-arm64 both to darwin: ONE darwin build ships under two Mac RIDs.
- **Why darwin's arm64 works on one build and linux's cannot.** darwin calls the kernel through libc trampolines, by
  name, so its amd64 and arm64 syscall surfaces agree. Linux calls the kernel by number. P2's census
  (`CENSUS-goarch-linux-windows.md`) found linux's `syscall` differing between the arches in its call numbers,
  kernel struct layouts, errors and generated wrappers, with `internal/runtime/syscall/linux` and
  `internal/syscall/unix` beside it.

## 2. The layout rule: orthogonal folders

### 2.1 The rule

- **The universe.** `U = {W, L, D, A}`: windows/amd64, linux/amd64, darwin/amd64 and linux/arm64. It is ONE declared
  constant in the converter. The merge runs over exactly `U`; it refuses a strict subset and a repeated
  (GOOS, GOARCH). That replaces the shared-GOOS refusal at `platformEmit.go`:72-81.
- **Keys and covers.** A key is a folder; its cover is the set of targets whose build compiles it.

  | Key | Cover | In `U` today |
  |---|---|---|
  | flat (the package root) | every target | {W, L, D, A} |
  | `<goos>/` | every target of that GOOS | `windows/` {W}, `linux/` {L, A}, `darwin/` {D} |
  | `<arch>/` | every target of that GOARCH | `amd64/` {W, L, D} |
  | `<goos>-<arch>/` | one target | `linux-amd64/` {L}, `linux-arm64/` {A} |

- **An arch key is admitted only when `U` holds two or more targets of that arch.** `amd64/` is admitted; `arm64/`
  is not, while linux/arm64 is the only arm64 target (§2.2 argues this).
- **Classification.** For one (package, file name), the targets that emitted it are partitioned into content classes
  (identical bytes, CR-normalised). A target that did not emit the name is in no class. A class with sharing set `S`
  is homed:
  1. at the **coarsest key whose cover equals `S`**, tried in the order flat, GOOS, arch, pair;
  2. otherwise it is **split**: take every GOOS key whose cover lies inside `S`, then every arch key inside the
     remainder, then a pair key for each target left. One identical copy is written per key.
- **Why no target ever sees a name twice.** Every key used has its cover inside `S`, the keys of one class are
  disjoint, and the classes of one name are disjoint. So each target compiles exactly one copy, and it is its own.

**Every presence subset of the four targets:**

| `S` | Home | Measured in M1b (versions, packages) |
|---|---|---|
| {W, L, D, A} | flat | 1692, 345 |
| {W, L, D} | `amd64/` | 48, 16 |
| {L, A} | `linux/` | 120, 18 |
| {W} | `windows/` | 172, 33 |
| {D} | `darwin/` | 133, 20 |
| {L} | `linux-amd64/` | 24, 6 |
| {A} | `linux-arm64/` | 73, 17 |
| {L, D, A} | `linux/` + `darwin/` | 115, 24 |
| {W, L, A} | `windows/` + `linux/` | 11, 5 |
| {W, D, A} | `windows/` + `darwin/` + `linux-arm64/` | does not occur |
| {W, L} | `windows/` + `linux-amd64/` | 4, 3 |
| {W, D} | `windows/` + `darwin/` | 19, 6 |
| {L, D} | `darwin/` + `linux-amd64/` | 4, 1 |
| {W, A} | `windows/` + `linux-arm64/` | does not occur |
| {D, A} | `darwin/` + `linux-arm64/` | 1, 1 |

### 2.2 The three choices inside the rule, argued

- **Ties go to the coarser key.** `windows/` and `windows-amd64/` both cover {W}, and likewise darwin's. The GOOS key
  wins, so `windows-amd64/` and `darwin-amd64/` are never written and nothing in `windows/` or `darwin/` moves.
- **The split takes GOOS keys first.** In `U` this order is unobservable: the only set containing `amd64/`'s cover
  that is not `amd64/` itself is `U`, which is flat. It becomes observable at five targets (with a darwin/arm64
  conversion, {W, L, D, darwin/arm64} is three keys GOOS-first and two arch-first). GOOS-first is chosen because it
  keeps every existing GOOS placement where it is. A fifth target's design re-verifies it against that universe.
- **{A} homes at `linux-arm64/`, not `arm64/`.** With one arm64 target the two are indistinguishable by measurement,
  so the rule under-claims:
  - a later arm64 target then PROMOTES files it is proven to share up into `arm64/`, on evidence. The other choice
    would DEMOTE linux-specific files out of `arm64/`, and until that merge ran, a single-target conversion at the
    new target would compile linux/arm64's syscall tables for another OS (§2.4);
  - the folder name equals the RID, which is what a reader of `runtimes/linux-arm64/` looks for.
  - The alternative, admitting `arm64/` at one target, is one line of the constant. COORD may overrule it (R1b).

### 2.3 The invariant for the three existing flavours

For each `T` in {W, L, D}:
- **I1, the source set.** The set of (file name, bytes) `T`'s project compiles is the same before and after, for
  every package. This is the rule's own no-duplicate property read at `T`, since `T`'s emission is unchanged.
- **I2, the moves.** A file's path changes in exactly two ways. Flat to `amd64/`: a file all three amd64 targets
  share that linux/arm64 does not emit or emits differently. `linux/` to `linux-amd64/`: a file homed in `linux/`
  that linux/arm64 does not emit or emits differently. Nothing in `windows/` or `darwin/` moves. The derivation is
  the table: joining a class, `A` leaves its key unchanged ({L} to {L, A} is still `linux/`; {W, L, D} to `U` is
  still flat); staying out of it, `A` narrows only flat and `linux/`, the two keys whose cover holds `A`.
- **I3, the project files.** Only a package that gains an arch or pair folder has its project file edited (§2.5).
  Every other project file is byte-identical.
- **I4, compile-item order.** `package_info.cs` stays in the first group. A moved file leaves the flat (or GOOS) glob
  for the arch (or pair) glob, which follows it. Roslyn orders module initializers by item order
  (`syscall.csproj`:186-190), so this is the one place the three flavours are not literally unchanged. Two channels:
  - **Module initializers.** Measured: of the 80 moved files, 8 carry one; 6 are `package_info.cs`. The other two
    are `runtime/cpuflags_amd64.cs`, whose `init` is emitted commented out, and `internal/bytealg/index_amd64.cs`,
    the only carrier in its package. Of the four compiled hand-owns that move with them (§2.7), one carries one,
    `internal/cpu/cpu_x86_impl.cs`; the package's only other is `cpu_arm64_impl.cs`, which stays ahead of it, and
    each returns at once on the other's architecture. So no initializer changes place relative to another.
  - **Static field initializers across files.** C# leaves their cross-file order unspecified, and the converter
    already makes it immaterial: an initializer that depends on another file's var is relocated into the ordered
    static constructor (`src/go2cs/initOrderOperations.go`:33-45). The same holds for every file that moved into
    a GOOS folder when L3 was introduced.
- **The general form,** for any later target `T'`: adding `T'` moves a file only out of a key `T'` joins and does
  not reproduce (demotion), or into a coarser key whose cover `T'` completes (promotion). Every other file stays.

"Byte-identical" here means I1-I3: the same sources under the same names. The assemblies are not claimed
byte-identical, because 80 item paths change.

### 2.4 Enforcement

**The merge (`mergePlatformEmissions`, `classifyPlatformEmissions` at `platformManifest.go`:166).**
- It takes exactly `U` and classifies every (package, name) by §2.1. It writes each class's copies and removes the
  name from every other key folder, admitted or not.
- It refuses a package that carries the layout block and holds a nested PACKAGE named after one of its keys, since
  the project's `<key>/*.cs` glob would compile the child's sources. None exists today: the three packages whose
  last element is a key name (`internal/runtime/syscall/{linux,windows}`, `internal/syscall/windows`) have no
  package for a parent.
- `retireUnemittedPackageInitFiles` (:1541) and `mergeCompanionArtifacts` (:826) walk every key, not every GOOS.

**The writer (a single-target conversion; `platformLayoutDir`, `platformLayout.go`:123).**
- It ADOPTS the tree's layout, as today. Generalised to (goos, arch), it probes `<goos>-<arch>/`, `<arch>/`,
  `<goos>/`; the first folder holding the name wins; otherwise the file is flat. On a canonical tree at most one of
  them holds the name for that target, so the probe order cannot matter.
- `isPlatformSourceFolder` (:88) admits GOOS, GOARCH and pair names, with the same discriminator (a child folder
  holding no `.csproj`). `platformPackageInfoPath` (:175) and `packageCarriesPlatformLayout` (:197) follow.
- **The contract is unchanged in kind.** A single-target conversion that writes a shared key (flat today, `amd64/`
  or `linux/` tomorrow) writes that target's bytes for every target the key covers. Only the full-universe merge
  re-classifies. This is exactly flat's contract today.
- **So a target enters the layout only through the merge.** A linux/arm64 single-target conversion into today's
  three-target tree would write arm64 bytes into flat and `linux/`. The seat that adds linux/arm64 to the constant
  therefore commits its corpus apply in the same seat (§9, S2): the constant and the tree agree at every commit.

**`reconcileLayoutDuplicates` (`platformEmit.go`:676), generalised from "flat versus GOOS" to cover containment.**
For two keys holding one name in one package:

| The two covers | Bytes | Action |
|---|---|---|
| disjoint (`windows/`, `linux/`) | any | none: no target sees both |
| nested (`linux-arm64/` inside `linux/`) | identical | remove the narrower copy |
| nested | different | CONFLICT, refused: the shared target would compile two bodies |
| overlapping, neither inside the other (`linux/`, `amd64/`) | any | CONFLICT, refused, even when identical |

The last row is refused even for identical bytes because no removal repairs it: each copy serves a target the other
does not.

**The canonical-form guard (a Go test over the tree; no conversion).** For every package and name it asserts:
- **one location:** no two keys with intersecting covers hold the name;
- **canonical:** identical copies sit in exactly the keys §2.1 gives for the union of their covers.

It already passes on today's tree at `U` = {W, L, D}, hand-owned files included: **4278 names, 0 one-location
violations, 0 non-canonical groups.** It has been made to fail: on a fabricated tree with one flat + `linux/`
duplicate and one name copied identically into all three GOOS folders, it named exactly those two (and passed an
identical linux + darwin pair, which is canonical). So it can be switched on before any layout change.

### 2.5 The project file and its references

- **The arch block is added only to a package that carries an arch or pair folder.** It adds a `GoTargetArch`
  default of `amd64` (and the `GoTargetOS` default where the package had no L3 block), and four items:

  | Item | Position |
  |---|---|
  | `$(GoTargetArch)/package_info.cs`, `Exists`-guarded | in the first group, after the GOOS one |
  | `$(GoTargetOS)-$(GoTargetArch)/package_info.cs`, `Exists`-guarded | in the first group, last |
  | `$(GoTargetArch)/*.cs` | after `$(GoTargetOS)/*.cs` |
  | `$(GoTargetOS)-$(GoTargetArch)/*.cs` | last |

- **`GoTargetArch` names the arch the flavour was CONVERTED at, not the host's.** It is `amd64` for windows and for
  darwin under both Mac RIDs, exactly as today. The pairs are `U`'s; one check in the corpus's shared build file,
  generated from the constant, fails a build whose (`GoTargetOS`, `GoTargetArch`) is not in `U` (a windows build at
  arm64 would otherwise compile with its `amd64/` files silently missing).
- **References use the same lattice:** shared references unconditioned, then groups conditioned on `GoTargetOS`,
  on `GoTargetArch`, and on the pair (`splitPlatformReferenceSets`, `platformProject.go`:219). An arch-varying
  reference implies an arch-varying source file, so only packages already carrying the arch block can need one.
  Measured from Go's own import sets (§2.6): 6 packages.
- **The single-target round trip is the gate for the reference groups.** Today every participating platform gets a
  group, even an empty one, because a single-target conversion can only honour what the tree carries. S1 keeps that
  property over the lattice and proves it: converting any one target of `U` into the merged tree leaves every
  project file byte-identical.

### 2.6 M1b, the four-target census (measured 2026-11-09)

- **Method.** The converter built at `7831a23193` (sha256 `a670bffa7b0ca799`); one seed snapshot (4569 `.cs`); four
  seeded roots, each `-stdlib -comments -platforms <target>`. All four conversions exited 0 (walls 370-374 s;
  360, 358, 358 and 356 packages). The logical files are Go's own selection per target (`go list` with
  `CGO_ENABLED=0` and tags `purego,math_big_pure_go`) plus one `package_info.cs` per package.
- **Scope: 2265 logical (package, file) names.** Outside it: the hand-owned `testing` package, `package_init.cs`
  and `.cs.auto` (both censused separately below).
- **Control: the rule reproduces today's tree.** Run at {W, L, D}, it places 5976 (file, target) pairs with **0
  mismatches** against the tree as committed.
- **The versions by sharing set** are the right-hand column of §2.1's table.
- **The moves for the three existing targets are the two I2 predicts, and nothing else:**

  | Move | Files | Packages | Absent on linux/arm64 | Present, differing |
  |---|---|---|---|---|
  | flat to `amd64/` | 48 | 16 | 29 | 19 |
  | `linux/` to `linux-amd64/` | 32 | 7 | 17 | 15 |

  - The 16 packages with `amd64/` are DESIGN-multiplatform-corpus §5's "16 packages".
  - Of the 48, 16 are `_amd64`-named, 8 are `package_info.cs` and 24 are others (`runtime/cputicks.cs`,
    `math/log_asm.cs`, `internal/cpu/cpu_x86.cs`, `runtime/panic.cs`, ...). A name-based rule would have missed them.
  - Of the 32, 5 are `package_info.cs`; the rest include syscall's five `*_linux_amd64` tables and runtime's
    `defs_linux_amd64`, `signal_linux_amd64`, `vdso_linux_amd64`, `malloc`, `mheap` and `stack`.
- **What linux/arm64 compiles:** flat 1692, `linux/` 246, `linux-arm64/` 74 (40 files only arm64 selects, and 34
  that both linux arches select and emit differently). linux/amd64 compiles `linux/` 246, `linux-amd64/` 32 and
  `amd64/` 48.
- **20 packages gain a folder** (`amd64/` 16, `linux-amd64/` 7, `linux-arm64/` 17):
  - 7 already L3: internal/buildcfg, internal/cpu, internal/runtime/syscall/linux, internal/syscall/unix, runtime,
    syscall, vendor/golang.org/x/sys/cpu;
  - 13 newly layout-carrying: crypto/internal/fips140deps/cpu, hash/crc32, internal/abi, internal/bytealg,
    internal/chacha8rand, internal/goarch, internal/runtime/atomic, internal/runtime/gc/scan,
    internal/runtime/startlinetest, internal/runtime/sys, math, runtime/race, runtime/race/internal/amd64v1.
  - This reconciles revision 1's "18 packages": M1 compared the two linux arches only, so it could not see a package
    touched only by an amd64-only flat file (`internal/abi` among them).
- **`package_init.cs`** (36 packages): linux/arm64's equals linux/amd64's everywhere except `hash/crc32`, where
  arm64 emits none. So one more file moves flat to `amd64/`. It is a static constructor, not an initializer item.
- **`.cs.auto`** (25 compared across the linux arches): one differs,
  `crypto/internal/fips140/subtle/xor_generic.cs.auto`
  ({W, L, D} against {A}). It is homed by its own content, as `runtime/<goos>/runtime2.cs.auto` already is beside a
  flat hand-own, so that package gains `amd64/` and `linux-arm64/` holding no compiled file. PREDICTION: its project
  file is the 21st to gain the arch block; S1 confirms at the emission.
- **References** (Go's direct imports per target, each edge homed by §2.1): at {W, L, D}, 2415 shared, 29 `windows/`,
  54 `linux/`, 38 `darwin/`, in 25 packages. At `U`: 2412 shared, the same three GOOS counts, 3 `amd64/`
  (internal/runtime/gc/scan, math, runtime/race) and 5 `linux-arm64/` (internal/runtime/atomic, internal/runtime/sys,
  vendor/golang.org/x/sys/cpu).
- **Seen in passing, not this design's:** Go selects `internal/goexperiment/exp_dwarf5_off.go` for darwin/amd64, and
  the darwin root holds no `exp_dwarf5_*.cs`, flat or under `darwin/`. Pre-existing; cause not investigated.

### 2.7 The amd64-only packages, and the hand-owned files

- **`runtime/race/internal/amd64v1` and `internal/runtime/startlinetest`** follow the platform-exclusive precedent:
  their files move to `<pkg>/amd64/`, and the package builds empty at linux/arm64. `runtime/race/race_v1_amd64.cs`
  moves to `runtime/race/amd64/` with an `amd64`-conditioned reference to amd64v1.
- **A hand-owned file is routed by its principal's PRESENCE,** not its content class
  (`platformHandOwn.go`:23-47, unchanged in kind): it belongs in exactly the builds its principal takes part in,
  and §2.1 then homes that set. Measured against M1b:
  - **Move with an amd64-only principal:** `hash/crc32/crc32_amd64.cs` (+ `.cs.auto`), `internal/cpu/cpu_x86_impl.cs`
    and `runtime/cputicks_impl.cs` to `amd64/`; `syscall/linux/syscall_linux_amd64_impl.cs` and
    `syscall/linux/zsyscall_linux_amd64_impl.cs` to `linux-amd64/`.
  - **Stay, and compile at arm64 against a principal that differs there** (S3 judges each):
    `internal/bytealg/bytealg_impl.cs`, `internal/chacha8rand/chacha8_impl.cs`,
    `runtime/{cpuflags,mranges,panic,sema,malloc}_impl.cs`, `runtime/linux/netpoll_epoll_impl.cs` and
    `syscall/linux/syscall_linux_impl.cs`.
  - A hand-own that needs different bytes per arch is split by hand; each version is then homed by the same rule.
- **A trap the name-based routing would spring (PREDICTION, from reading the rule; not yet run).**
  `internal/cpu/cpu_arm64_impl.cs` is a FLAT hand-written stand-in: ARM64 feature detection for an arm64 process in a
  flavour converted at amd64, with its `osInit` in `darwin/cpu_arm64_darwin_impl.cs`. Under `U`, the converted
  `internal/cpu/cpu_arm64.cs` exists at {A} only. Its name makes it the stand-in's principal, so the stand-in would
  be routed into `linux-arm64/`: it would collide there with the converted `doinit`, and darwin would lose its
  Apple-silicon detection (the TestMemHash and AES-GCM class). **Disposition (S4):** rename the stand-in so it has
  no principal and home it in `amd64/`; linux/arm64 uses the converted `doinit` with a HWCAP `osInit` (§5).
- **M2's compile wall** (revision 1: four duplicate members at arm64) is re-read under this rule. PREDICTION:
  `cputicks` dissolves, because its hand-own now follows its amd64-only principal into `amd64/`; `testSPWrite`,
  `archLog` and `archHypot` remain for S3. Declaration errors mask body errors, so the wall is still a FLOOR.

## 3. Packaging and consumer RID selection

- **`$ridFlavors` maps a RID to a (GOOS, GOARCH) pair:** win-x64 to windows/amd64, linux-x64 to linux/amd64,
  linux-arm64 to linux/arm64, and osx-x64 and osx-arm64 both to darwin/amd64. The build pass, `$buildRidByGoos` and
  `$buildRidOf` (`push-nuget.ps1`:1728-1739) and the merge (:2049, :2067) are keyed on the pair, as is
  `Get-NugetgoRidFlavors`. `$referenceRid` stays win-x64. linux-arm64 is NOT a twin of linux-x64: the post-merge
  check's "twin folders byte-identical" rule (:2087-2123) must not pair them.
- **13 more packages become RID-split.** A package flat today that gains `amd64/` or `linux-arm64/` now differs at
  linux-arm64, so it ships `runtimes/` for all five RIDs (SEAT 0 reads "RID-split" from that folder's presence, so
  a partial set would misfire it). Size (ESTIMATE): revision 1's 4.4 MB for one more RID folder across the L3
  packages, plus five folders for the 13, which are small packages apart from math. S6 measures it at the first pack.
- **ApiCompat:** `Test-GoApiCompat.ps1`:132 gains the RID, and the rule "a new RID is admitted with no prior
  baseline" is written down explicitly; the first linux-arm64 has nothing to compare against.
- **The pack's arm64 pass is a cross-compile of AnyCPU IL** on the x64 box. Native AOT probes and aot-smoke run ON
  the arm runner. `GoLibcCall`'s darwin-arm64 variadic rule does NOT apply on Linux AAPCS64; that is recorded at
  `syscall/linux/syscall_linux_impl.cs`:104. `TailCallFrameGuardTests` is JIT behaviour: it is re-run in Release on
  arm64 before any caller-line result is trusted there.
- **Consumer selection needs no new mechanism.** `GoSelectRidCompileAssets` already swaps to
  `runtimes/$(GoCompileRuntimeIdentifier)/` when it exists (go.lib.targets:49-75), and the host already picks
  `runtimes/linux-arm64/` from `runtimeTargets`.
- **The converter's `-recurse=nuget` map** (`compileRuntimeIdentifierForTarget`, `src/go2cs/moduleConverter.go`:1048)
  maps linux/arm64 to linux-arm64; `stdlibMetadata_test.go`:431-439 gains that case.

## 4. runtime.GOARCH on this flavour

- On linux-arm64, `internal/goarch` is the arm64 conversion (`zgoarch_arm64`, in `linux-arm64/`), so
  `runtime.GOARCH` is the converted const "arm64". No displacement is needed.
- The darwin and windows displacement (the run-time GOARCH seat, batch 279) is keyed by GOOS, so linux is out of its
  scope at both arches. `TestAManualConstIsDisplacedOnlyOnItsTarget` drives the emission per target; S1 adds a
  linux/arm64 target to it, so a later scope widening that reaches linux shows up there.

## 5. cpu.ARM64 from HWCAP (S4, load-bearing)

- **The gap.** Go's linux/arm64 `internal/cpu` reads its features from `cpu.HWCap`/`HWCap2`, which the runtime fills
  from the auxiliary vector in `sysargs`. Here `schedinit` never runs (`cpuflags_impl.cs`), so nothing fills them
  and every `cpu.ARM64` flag stays false.
- **It is not optional.** The AES hash is chosen on `cpu.ARM64.HasAES` (`hash_impl.cs`'s arm64 branch), so without
  S4 the flavour's map hashing diverges from Go's on the same host; crypto's `hasGCMAsmARM64` follows it.
- **The seat.** A linux `osInit` for the arm64 flavour: `getauxval(AT_HWCAP)` and `getauxval(AT_HWCAP2)` through
  libc at module init, set `cpu.HWCap` and `HWCap2`, then run the converted `doinit`. (Revision 1 parsed
  `/proc/self/auxv`; the libc call is simpler, per the review.) The kernel's word is the source of truth, as in Go.
  .NET's `System.Runtime.Intrinsics.Arm.*.IsSupported` is the GolibTests cross-check, never the source. Like
  darwin, `HasDIT` stays false (COORD's 2026-11-06 ruling).
- **It also owns the `cpu_arm64_impl.cs` disposition** of §2.7.
- **Size: about 0.5-1 lane-day.**

## 6. CI

- **os-matrix has no arm leg today.** `ubuntu-24.04-arm` appears only in `release-binaries.yml`:275 and :323.
- **The change:** `goos=linux` fans out to two runners, as darwin does (os-matrix.yml:327-333): linux-x64 on
  `ubuntu-latest` and linux-arm64 on `ubuntu-24.04-arm`. Each leg passes `GoTargetArch` from its RID through §3's
  one map. The oracle needs no `-test-oracle-goarch` there: the flavour's arch IS the host's. release-smoke and
  aot-smoke gain the fifth RID (:342-349).
- **A hosted job caps at 6 hours.** The arm64 sweep therefore runs in four or more shards, the runtime row (1945 s
  on linux-x64, run 42686722080) may need its own job, and timing-sensitive rows are re-measured on the arm runner
  rather than inherited.
- **Walls (ESTIMATES: linux-x64's recorded walls, the arm runner assumed comparable until its first run).** census
  18 min; golib-tests 10-13 min; behavioral-full 36-62 min; the whole linux bank 12.4 h of machine time over 283
  rows (`rebank-1.26.10.3/linux/*.tsv`).

## 7. The bank: a fourth reading

- **The grammar** (`docs/ValidatedTestPackages.md`:477-488; parser contract :501-540) already carries two arches
  under one OS key with a per-RID note: `darwin: 7 + 3 (osx-arm64; osx-x64: 10)`. linux extends the same way, with
  no new OS key (R3, approved): `linux: 11094 + 67 (linux-x64; linux-arm64: N + d)`.
- **A pending-at-RID state (the review's F5).** A plain `linux: N` today means "banked on linux-x64". While the
  arm64 bank is sharded over weeks it must not read as banked on both:
  - `linux: 11094 + 67 (linux-x64; linux-arm64: pending)` is a legal note, and once the arm64 bank opens, a linux
    segment with no per-RID note fails the format check. `annotate_linux.py`, the twin of `annotate_darwin.py`,
    writes the pending note onto every linux row in one mechanical commit;
  - the Linux header derivation (`check-roster-format.ps1`:1341-1361) reports two populations, one per RID;
  - the L bubble (`DESIGN-roster-per-os-bubbles.md`) renders the worse of its BANKED RIDs. A pending RID is excluded
    from "worse" and shown as pending, so an unrun arm64 row never reads green or red.
- **`src/_roster.ps1`:** `$RosterOsPattern`'s note group (:98-102) admits `linux-x64|linux-arm64` and `pending`; the
  darwin-only refusal (:302-323) becomes a per-key RID set.
- **Rows that plausibly differ:** syscall, os, net, internal/poll, internal/syscall/unix (call numbers, layouts, the
  hand-owns of S3); runtime; internal/cpu and crypto/* (HWCAP-gated, after S4); math and hash/crc32 (arch-selected
  files). debug/buildinfo, go/internal/gcimporter and net/http/pprof should MATCH by construction, since Go and the
  flavour agree on arm64 here.

## 8. The hop-runbook steps it reuses (`docs/GoCorpusMigration.md`)

Adding a flavour is a hop with the Go version held fixed: **H5** (seeded full reconvert, :875; four roots now),
**H6** (the hand-own re-audit, :1678; S3's input), **H7** (compile parity, :1926; `go2cs-stdlib.slnx` at
linux/arm64), **H8** (multi-platform re-emission, :2177; the merge over `U`), **H10** (roster, proof-page and
disclosure re-derivation, :2831), **§3** (the shardable roster campaign, :4143) and **H11** (publication and
compatibility guards, :3966). The runbook's three-target lists (:1191, :2194, :2252), `.claude/rules/converter.md`:91
and `docs/ConversionStrategies-Reference/package-conversion.md`:536 are amended in S2, in-stage.

## 9. Seats

The spine is S1, S2, S3, S7. S0, S4 and S5 are independent of it; S6 needs S2.

| Seat | What | Size (lane-days) | Lane |
|---|---|---|---|
| **S0** | Refuse loudly (§0), as ruled. Ships in .4 alone. | 1 | C2 (cutting) |
| **S1a** | Converter, the layout rule **at today's universe**: §2.1's classifier with `U` as a constant, the merge and its refusals, `reconcileLayoutDuplicates` by cover containment, the writer's probes, the arch block and reference lattice in the project writer, hand-own routing over keys, the canonical-form guard. **Gate: with `U` = {W, L, D} the corpus reconvert footprint is EMPTY** (M1b's control predicts it). Unit gate: the 15-row table, and every partition of four targets (no target sees a name twice; each sees its own bytes). | 3-4 | G |
| **S1b** | Converter and harness, the arch through `-tests` (the review's F3): `corpusArch` (`testsCorpusArch.go`:19, `testsTargetPlatform` :31) becomes per-GOOS, {windows: amd64, linux: {amd64, arm64}, darwin: amd64}, read from the same table as `U`; the `-tests` project emission and build args carry `GoTargetArch`; `run-validated-sweep` and `_paths.ps1` (:61, :119-120) gain it; `stackAppendHarvestTargets` (`stackAppendHints.go`:66) and `-platform-census` read `U`; the arm leg's behavioural golden policy is declared (convert at arm64, oracle at arm64: the opposite of darwin's). | 1.5-2.5 | G or H |
| **S2** | The flip: linux/arm64 joins `U`, and the same seat commits the corpus apply (H5 over four roots, H8). The same seat widens the escape-registry generator's per-OS predicate (`IsPerOS`, `src/gen/go2cs-gen/EscapeRegistryGenerator.cs`:358), which admits only a source file under `<GoTargetOS>/`: it must also admit `<GoTargetArch>/` and `<GoTargetOS>-<GoTargetArch>/`, with `GoTargetArch` made compiler-visible beside `GoTargetOS` (`src/core/Directory.Build.props`:40), or a package whose only platform folder is `amd64/` reads platform-neutral and gets no platform variant. Red first, on such a package. **Gates:** the moves are exactly I2's (48 + 1 and 32), their hand-owned companions (§2.7: three to `amd64/`, two to `linux-amd64/`) and the one `.cs.auto` split; for W, L and D, each project's `Compile` item set (`dotnet msbuild -getItem:Compile`, as name + hash) is unchanged; every project file outside the 20 (21) is byte-identical; the canonical-form guard passes at `U`; the single-target round trip leaves the tree clean at all four targets; H7 parity at the three existing flavours. | 1-2 | the S1a lane |
| **S3** | Hand-owns at linux/arm64 (H6; the review's F2). **Population:** every file under `**/linux/` plus every flat `_impl.cs` that compiles on linux. Censused here: 24 linux-folder hand-owns (20 `_impl.cs`, 4 whole-file) and 95 flat `_impl.cs`; (i) 12 numeric syscall-number sites in 5 files, (ii) 11 size or offset constants in 7 files, (iii) 30 explicit-layout mirrors in 7 files. **Rules:** every number comes from the compiled flavour's `zsysnum_linux_*` constants, or switches on arch as `golib/runtime/ThreadCpuTime.cs`:145-150 does. A syscall arm64 lacks is REPLACED, not renumbered: `epoll_wait`, `open`, `pipe`, `dup2`, `fork` and `getdents` do not exist there (a renumbered twin compiles and returns ENOSYS). Page size is read, not 4096 (`mem_linux_impl.cs`:123-128, `mranges_impl.cs`:191). Any packed or explicit-layout mirror holding an Interlocked 64-bit field is audited for alignment (an unaligned 64-bit atomic faults on arm64). **Gate:** a GolibTests arm per renumbered or replaced site, run on the arm runner; then H7 at linux/arm64, the wall of §2.7 first. | 5-8 | H (native seam) with G |
| **S4** | cpu.ARM64 from HWCAP (§5), the stand-in's disposition, the GolibTests cross-check. | 0.5-1 | C1 or H |
| **S5** | CI (§6): os-matrix's arm leg, four or more sweep shards, release-smoke and aot-smoke at five RIDs. | 1-2 | C1 |
| **S6** | Packaging (§3): the pair-keyed pass and merge, the 13 newly RID-split packages, NugetgoFlavors and its test, ApiCompat's new-RID rule, the Release tail-call re-run. | 1.5-2.5 | C2 |
| **S7** | The bank (§7): the pending-at-RID grammar and `annotate_linux.py`, then the sharded linux-arm64 sweep and its banking. | 3-5 | the bank lanes (P1, P2, C1) |
| | **Total** | **about 18-28 lane-days** | |

- The total is above the review's 16-24 because S1 is sized as two seats: the layout generalisation touches the
  merge, the reconcile, the writer, the project writer and the hand-own router, and S1b is separable from it.
- **S1a is the de-risking step.** It lands with the universe unchanged, so its whole footprint must be empty, and
  the guard is on before any file moves. Only S2 changes the tree.
- Known arm64 numbers for S3's first sites: `epoll_pwait` 22, `epoll_ctl` 21, `epoll_create1` 20, `recvfrom` 207,
  `sendto` 206, `getpid` 172, `gettid` 178, `tgkill` 131, `exit_group` 94, `rt_sigprocmask` 135, `kill` 129.

## 10. Rulings asked

- **R1a.** The layout rule of §2.1: coarsest exact cover in the order flat, GOOS, arch, pair; otherwise split GOOS
  first; an arch key admitted at two or more targets. **Recommended: yes.**
- **R1b.** {A} homes at `linux-arm64/`, not `arm64/` (§2.2). **Recommended: yes;** the alternative is one line.
- **R1c.** `GoTargetArch` names the conversion arch, default `amd64`; darwin stays `amd64` under both Mac RIDs, and
  a (GOOS, GOARCH) outside `U` fails the build. **Recommended: yes.**
- **R1d.** S1a lands at today's universe with an empty footprint; S2 adds linux/arm64 to the constant and commits
  its corpus apply in the same seat. **Recommended: yes.**
- **R1e.** A `.cs.auto` is homed by its own content even where its hand-own stays flat, so
  `crypto/internal/fips140/subtle` gains two folders holding no compiled file (§2.6). **Recommended: yes,** it is
  today's `runtime2.cs.auto` precedent; the alternative is a block predicate that counts only `.cs`.
- **R2, R3.** Already ruled (approved with changes); restated in §0 and §7 as ruled.

## 11. Amendment, 2026-11-10: what cutting S2 corrected

Read while cutting S2 (branch `claude/g-la64-s2`, `780cb9b34f`, on master `66a2e023ca`). The sections above stand as
written. Where this block disagrees with them, this block is the later reading. S2's file-by-file prediction and
its score (271 paths predicted, 271 hit) are in that branch's `DATA-la64-s2-prediction.md`, not restated here.

### 11.1 §9, the S3 row: class (iii) is 18 types in 4 files

The row says "(iii) 30 explicit-layout mirrors in 7 files". Re-read at `66a2e023ca` over the row's own population
(hand-owns under `**/linux/`, plus every flat `_impl.cs`; 26 + 95 files at this ref):

| File (hand-owned) | `StructLayout` types |
|---|---|
| syscall's `sockaddr_linux_impl.cs` | 6 |
| syscall's `structclass_linux_impl.cs` | 6 |
| syscall's `zsyscall_linux_amd64_impl.cs` | 5 |
| internal/syscall/unix's `siginfo_linux.cs` | 1 |
| **Total** | **18 types, on 20 matching lines (two are comments)** |

- Every one is `LayoutKind.Sequential`; none is explicit. "Explicit-layout" in the row was the wrong word for them.
- Nine of the 18 carry `LinuxAmd64` in their name (the six of `structclass_linux_impl.cs` and three of
  `zsyscall_linux_amd64_impl.cs`). That is where S3 looks first; it is not a claim that the other nine are
  arch-neutral.
- The 30-in-7 figure is not reproduced, by this predicate or by a wider one (every `.cs` under `**/linux/`, converted
  files included: 21 lines in 5 files, the fifth being a converted `gcBits`). The row recorded a count without its
  predicate. S3's audit list is the 18.
- Classes (i) and (ii) of the row were not re-read here.

### 11.2 A record the design did not cover: the standard-library metadata record

`stdlib-metadata.txt` (and the same record inside a published package's self-description) keyed a package's
copies by GOOS: one unqualified section, plus `@linux` and `@darwin` sections where a package differs there. A
package split only by arch had no section to key, and S2 makes six of them.

- A copy is now keyed by its **layout folder**. The copy the windows/amd64 build compiles is the unqualified
  section; every other copy is `##name@<folder>`. The generator refuses two copies that both serve the reference
  build.
- A lookup takes the **target** and reads the finest folder that target compiles first (pair, arch, GOOS), then the
  unqualified section. A record written with only `@linux` and `@darwin` sections reads as before.
- Measured: 417 sections become 428, as predicted before the regeneration: six new `@linux-arm64` sections for the
  packages split only by arch, and five `@linux` sections renamed `@linux-amd64` beside five new `@linux-arm64`
  ones. No unqualified section changes.
- S6 inherits this: a published package's self-description is the same record, so the pair-keyed pass of §3 reads
  folder-keyed sections.

### 11.3 §2.4, enforcement: every guard that reads the layout reads it per target

§2.4 lists the merge, the writer, the reconcile and the canonical-form guard. They are not the only readers of the
layout. Ten other guards knew a package as "its own directory, or its GOOS folder", and each was wrong the moment a
file sat in `amd64/` or a pair folder:

- **In the converter suite (eight failing tests, seven guards):** `duplicatePartialMembers`,
  `manualConversionDestination` (two tests), `platformHandOwn`, `platformLatticeLayout`, the metadata record of
  §11.2, `suppressionCompanion`, `valueCloneStampMembers`.
- **In repoguard (three guards):** `lineKeyedRecords`, `testInfoPositionMapCopies`, `golibGrowSlice`.

**The rule they now share:** a guard reads what a TARGET compiles, flat plus every key folder whose cover holds
that target, and compares two locations by the builds they take part in, never by how their folders are spelled.
`linux/` and `linux-amd64/` plus `linux-arm64/` are the same builds; `amd64/` and `linux-arm64/` share none.

Two readings moved, with zero corpus findings before and after:

- **`platformHandOwn`:** "the principal is in every folder" was tested by counting folders, so a flat principal in a
  one-folder package satisfied it by accident. It is now a cover comparison that excludes a flat principal, and
  such a principal is read as shared.
- **`valueCloneStampMembers`:** a key-shaped directory with its own project file is a package
  (`internal/runtime/syscall/linux` is one), and only a key-shaped directory without one is a folder of its parent.

**The gate lesson.** Repoguard was green on the rehearsal while the full converter suite read eight fails. A layout
change is gated by the full suite; repoguard alone does not cover it.

### 11.4 §2.7: four hand-owned bodies are owed at linux/arm64

§2.7 lists `zsyscall_linux_amd64_impl.cs` moving to `linux-amd64/` with its amd64-only principal. It does not state
the consequence: the hand-owned bodies of four syscall members live only in that file, so at linux/arm64 the four
are generated placeholders.

- The four: `Fstat`, `Uname`, `fstatat`, `wait4`. Each marshals through a mirror declared in that file (§11.1).
- They join S3's population. S2 pins them in a ledger (`owedHandOwnBodies`, in `manualConversionDestination`) that
  fails on a debt it does not list and on a listed row that is no longer owed. Both directions were made to fail.

### 11.5 §6 and §9: the arch binding is a co-requisite of S2, not independent of it

§6 already places the binding ("each leg passes `GoTargetArch` from its RID through §3's one map"). What §9 gets
wrong is the order: it says "S0, S4 and S5 are independent of [the spine]", and for this part of S5 that is false.

- The arm leg binds `GoTargetOS` only, and calls itself a calibration only while the corpus holds no `linux-arm64/`
  folder. After S2 alone it would compile the linux/amd64 flavour on the arm runner and report it, unlabelled, as
  arm64.
- **Ruled (COORD, 2026-11-10): S2 does not seat without the `GoTargetArch` binding; S2 and S5b seat together.**
- The binding is S5b's to design. What S2's reads used was a scratch: one job-environment line beside the
  `GoTargetOS` binding, keyed on the leg's RID by an inline test. It is right for today's universe; §6's map is
  the durable form.

### 11.6 §2.6 and R1e: the arch-divergent review sibling stays flat, and the package is named

§2.6 predicts that `crypto/internal/fips140/subtle/xor_generic.cs.auto` is homed by its own content, so the package
gains `amd64/` and `linux-arm64/` holding no compiled file, and R1e recommends it. The merge does not do that.

- **What the merge does.** It splits a review sibling only into key folders that are already source folders (a `.cs`
  and no project file inside). Otherwise it keeps the first target's flavour flat and names the package in the
  apply's closing note. The rule predates this design: a single-target conversion routes into a key folder only
  where it finds a source folder, so a folder holding only a sibling would never be rewritten.
- **So S2's apply names one package,** the corpus's first, and its project file is not the 21st to gain the arch
  block (20 do).
- **The cost is one row.** A single-target linux/arm64 conversion into the applied tree rewrites the flat sibling
  with arm64's flavour (`supportsUnaligned` folds to `false` there). The four-target apply is a fixed point.
- **Delivering R1e** needs the merge and the single-target writer to agree on a folder that holds only a sibling: a
  converter seat of its own, with a three-path footprint (the flat sibling out, one in each folder). It waits on a
  ruling; until then R1e reads as not delivered.

### 11.7 §2.4 again: the readers outside the converter suite

§11.3 stops at the converter suite and repoguard. Six more readers named the three GOOS folders themselves, and S2
was posted before any of them was run against the applied tree (`DATA-la64-s2-prediction.md`, §6.4).

- **Two went red:** a GolibTests guard that finds a walker's declaration, and the comment census.
- **Four kept passing and were wrong:** the pack's props target (nine packages would have shipped without their
  usings), the hand-own upstream census, the declared-not-implemented census (60 packages counted where the corpus
  holds 43), and the ref-lowering census's layout probe.
- **The spelling is not one shape.** The lists sat in a slice, a map, a `switch`, a PowerShell array and an MSBuild
  `Exists` chain. A search for one shape finds one of them.
- **A list a tool must own is pinned, not trusted.** MSBuild and PowerShell cannot read `platformLattice.go`, so
  their key lists stay where they are and a repoguard test holds them equal to the universe's key folders.

**The gate, as now ruled** (`.claude/rules/corpus.md`): a change that moves the corpus layout runs every reader of it
before it is posted: `go test ./...`, GolibTests on every flavour, the comment census, the behavioral harness. A
count that falls, or a pass that survives a rename untouched, is read against a control tree before it is believed.
§6.8 of the record lists the readers that still hold a fixed list and miss nothing today.

### 11.8 §11.6 closed: R1e is delivered, and the folder holds only the sibling

§11.6 left R1e waiting on a converter seat. That seat is cut on batch 283 (`DATA-la64-s2-prediction.md`, §6.11).

- **What the merge does now.** It splits a differing review sibling into any key folder that is not a nested
  package. `crypto/internal/fips140/subtle` gains `amd64/` and `linux-arm64/`, each holding the sibling and nothing
  else, which is §2.6's prediction as first written.
- **The single-target writer agrees with it** by reading the folder the sibling already sits in, finest first, so a
  one-target conversion rewrites the copy the merge laid and never lays a flat one beside it. The `-tests` writer
  resolves the same way.
- **The project file does not move.** A folder with no compiled file earns no arch block; §11.6's "20 do" stands.
- **What stays flat and named:** a key name that is a nested package (`internal/syscall/windows`). No package in
  the corpus is named by the apply today.
- **§11.7's gate caught one more reader.** A guard in a test file of the converter package admitted a split
  sibling only in a folder that holds source, and went red on the applied tree. The readers of the layout are not
  all in non-test sources; the full suite is the census.
