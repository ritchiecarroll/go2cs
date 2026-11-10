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

## 6. The apply, scored (dated block, 2026-11-10)

**The base moved, the prediction did not.** §1 was measured on `c45b1a4308`. The seat was cut on master `66a2e023ca`
(batch 280, which carries S1a, S1b and S4), and the same 271 paths were predicted there. The apply ran with the
converter built from the seat, `780cb9b34f`, on go1.26.10 and .NET 10.0.400.

| §4 reading | Predicted | Read at `780cb9b34f` |
|---|---|---|
| Changed paths under `src/core` | every one a HIT | 271 predicted, 271 changed, 271 HIT; 0 CONTENT, STATUS, UNPREDICTED, MISSING |
| Hand-owned bytes | kept; seven files change home | PASS |
| Compiled item sets at the three amd64 targets | unchanged by name and bytes | 1,311 rows a side; 58 keys, 58 set-equal, 0 differ (35 in order only) |
| A second four-target apply | writes nothing | rc 0 in 1,469 s, four `Failed: 0`, 0 files newer, 0 status rows |
| Each single-target conversion | writes nothing | **not as predicted**: §6.1 |

The rehearsal on `c45b1a4308` read the same: 271 of 271 HIT, and a second apply with 0 status rows.

### 6.1 The single-target reading: red, one row of it S2's

Four single-target `-stdlib -comments -platforms <target>` conversions into the applied tree, each from a clean tree,
restored by status between targets. Every one ended rc 0 with `Failed: 0`.

| Target | Converted | Status rows | Rows whose content changed |
|---|---|---|---|
| windows/amd64 | 360 | 7 | 1: `log/syslog/log.syslog.csproj` (7 lines removed) |
| linux/amd64 | 358 | 6 | 0 |
| darwin/amd64 | 358 | 6 | 0 |
| linux/arm64 | 356 | 7 | 1: `crypto/internal/fips140/subtle/xor_generic.cs.auto` (1 line) |

**Six rows at every target, and the project file at windows/amd64, are master's.** The six are the root attribution
files (`VERSION`, `LICENSE`, `PATENTS`, `README.md`, `SECURITY.md`, `CONTRIBUTING.md`): a single-target conversion
copies them from GOROOT with their LF line endings over a CRLF checkout, and no content changes. The project file loses
its `InternalsVisibleTo` group when windows/amd64 alone reconverts `log/syslog`. **Control:** the same one-package
conversion (`-platforms windows/amd64 log/syslog`) in a tree at master `66a2e023ca` with a converter built from that
tree left the same seven rows and the same seven removed lines. §4's last bullet claimed more than master delivers; it
was written without that control.

**One row is S2's.** `xor_generic.go` folds `supportsUnaligned` to `true` at amd64 and to `false` at arm64, so the
review sibling of its flat whole-file hand-own differs by architecture, and its package has no key folder to split it
into. The merge then keeps the first target's flavour flat and names the package, as it is written to
(`mergeReviewSiblings`). Every four-target apply of this seat printed it:

    ⚠ 1 package(s) emitted a flat companion that differs by target in a way layout L3 does not express
      crypto/internal/fips140/subtle — review sibling xor_generic.cs.auto differs by target, and the package has no
      source folder for every flavour's key, so windows/amd64's was kept flat

Master's three-target applies name no package: this is the corpus's first. A single-target linux/arm64 conversion
writes its own flavour over the flat sibling, which is the row above. The four-target apply is a fixed point, and a
two-seeded single-target footprint carries the row in both arms, where it cancels.

- **§2 had the cause and §4 did not draw the consequence.** §2 records the sibling staying flat and the note. §4's
  bullet was written beside it and never run before the seat was posted.
- **The design's R1e is unmet.** It homes this sibling by its own content, in `amd64/` and `linux-arm64/`. The merge
  splits a sibling only into folders that already hold a source file, so that a single-target conversion can find
  them again. Delivering R1e is a converter change of its own, routed for a ruling.
- **§2 called the hand-own an S3 site; it owes a comment at most.** `xor_generic.cs` takes the word path with no
  alignment test, on a runtime whose targets do not fault on an unaligned load. It is right at both architectures.
- **The apply script's counter was dead.** It counted the note with a filter on the word `residual`, which the note
  does not print. It read 0 on every run and was never made to match.

### 6.2 Gates at `780cb9b34f`

| Gate | Reading |
|---|---|
| Converter suite, `go test -timeout 60m .` | ok in 987 s, 0 fails |
| repoguard | ok in 156 s, 0 fails |
| go2cs-gen tests | 464 passed of 464 |
| `go2cs.slnx`, Debug | rc 0 in 495 s, 0 distinct errors |
| Standard library at linux/arm64 (`GoTargetOS=linux`, `GoTargetArch=arm64`) | rc 1 as predicted: 362 projects, 71 built, 3 distinct errors, all CS0111 |
| CI census, linux/arm64 runner (run 42777627196) | fails as predicted in 242 s, on the same three sites |
| CI census, linux, windows, darwin (both runtimes) | success (runs 42777652880, 42777640266, 42777665081) |
| CI release-smoke (run 42777677526) | pack fails on master's two `CP0006` lines (`IByteSeq<T>`), and no line master's run 42752322043 does not print |

The arm64 legs ran on a scratch branch that adds the `GoTargetArch` binding to the workflow; the seat itself does not
carry it, and by ruling S2 seats together with the seat that does.

### 6.3 §5's sites

The three errors are §5's three members and nothing else: `archLog` and `archHypot` (math) and `testSPWrite`
(runtime), each CS0111. The two body-level sites (`runtime/hash_impl.cs` reading `cpu.GoIsArm64Process`, and
`syscall/linux/structclass_linux_impl.cs` naming `NativeTimevalLinuxAmd64`) are still in the tree and were not
reached: runtime stops at its declaration errors and syscall is not built behind it. They are unread, not refuted.

### 6.4 After the posting: the readers of the layout

The seat was posted at `780cb9b34f` on gates that ran the converter's root package and repoguard: not
`go test ./...`, and no GolibTests. Lane i9, stacking S3 on the posted commit, read three things those gates had not.
Two are one class, a reader of `src/core` that names the windows, linux and darwin folders itself, and an audit of
every such reader found four more. The third is a property of the host (§6.7).

| Commit | What it carries |
|---|---|
| `51abda2e4b` | The bytes proof page, regenerated: two cites re-pointed to `internal/bytealg/amd64/bytealg.cs` |
| `8553da5448` | Merge of master `ae69e4ce74` (batch 281): one conflict, three roster cites re-pointed |
| `d2c32ec082` | The GolibTests walker guard |
| `d211483a97` | The comment census and its baseline (§6.5) |
| `8311de6326` | The pack's props target |
| `cfa79316a2` | The hand-own census, and a guard over the two key lists the tools own |
| `25bc6c1720` | The declared-not-implemented census (§6.6) and the ref-lowering census's layout probe |
| `e0a80cf3a0` | The licence header and the two projitems entries of the test files added above (§6.9) |

**Six readers were blind or broken, and four of them kept passing.**

| Reader | What it read after the apply | Proof of the fix |
|---|---|---|
| `EveryHandOwnedSkipCountedWalkerKeepsItsFrame` (GolibTests) | red: no declaration of `runtime.gcallers`, which moved to `runtime/amd64/` | red with the old list, green with the folders parsed from `platformUniverse`; suite 2,476 passed, 38 skipped, 0 failed |
| `TestCommentParityCensus` | red: 57 of 1,825 files unread, 21 classes FELL | §6.5 |
| `go.lib.pack.targets`, `build/<id>.props` | passing, and wrong: nine packages shipped no props | a pack of hash/crc32 held no `build/` entry before and holds one after; four control packages write the same bytes |
| `handown-upstream.ps1` | passing: seven hand-owns mapped to a Go file no release has | self-test red on a per-GOARCH and a per-pair case, then 10 cases clean |
| `TestDeclaredNotImplementedCensus` | passing, and wrong: 60 packages, 576 stubs | §6.6 |
| `packageIsLayoutL3`, a report field | nine packages laid out by architecture alone read as flat | red with the old list (every arch and pair folder named), green as committed |

**The pack is the one that would have reached a consumer.** Nine packages hold `package_info.cs` in key folders only:
hash/crc32, internal/bytealg, internal/runtime/atomic, internal/runtime/gc/scan, internal/runtime/startlinetest,
internal/runtime/sys, internal/runtime/syscall/linux, math and runtime/race/internal/amd64v1. Without the props a
consumer of `go.hash.crc32` has no `using go.hash;`, and a Go method call on it fails with CS1929.

`TestLayoutKeyListsAreTheUniverses` (repoguard) now holds the two lists that MSBuild and PowerShell own equal to the
key folders of `platformUniverse`. It was red with one key dropped from each list, naming each file.

### 6.5 The comment census: re-minted for the linux/arm64 part alone

`findPairs` took a file's package from its folder unless the folder was windows, linux or darwin. Files in `amd64/`
and `linux-amd64/` were paired as a package that has no Go source, and dropped.

| Reading | Files | Comment groups | Classes off master's baseline |
|---|---|---|---|
| Master's baseline | 1,825 | 54,005 | n/a |
| Old folder list, on the applied tree | 1,768 | not minted | 21 FELL |
| Every key folder, linux-arm64 pairs left out | 1,825 | 54,005 | 0 of 58 |
| Every key folder | 1,864 | 54,309 | 9 |

The 39 files that only linux/arm64 has add 304 groups and 64 counts over nine classes:
`body:extra:after-named-result-locals` +25, `top:unmapped:func` +16, `misplaced:free-top-level/anchor-differs` +8,
`missed:directive` +6, `body:extra:after-lowered-switch` +4, `body:go-blank-cs-none` +2, `body:extra:unexplained` +1,
`body:go-blank-unmapped` +1, `missed:decl-doc` +1. The baseline is re-minted for that part and no other.

The red was neither the host nor the checkout: the seat's gate did not run this package.

### 6.6 The declared-not-implemented census: a passing count that moved under a rename

The census folded a file into its package by dropping a trailing GOOS folder, so a file in a GOARCH or pair folder
became a package of its own and its declarations never met the implementing parts beside them.

| Reading | Declarations | Stubbed | Packages | Pushed stubs |
|---|---|---|---|---|
| Master `ae69e4ce74` | 786 | 480 | 42 | 54 |
| The applied tree, old fold | 843 | 576 | 60 | 54 |
| The applied tree, every key folder | 843 | 503 | 43 | 54 |

Predicted for the last row: 843, a stubbed count in [480, 537], 42 packages, 54. The package count missed by one,
and the one is hash/crc32: its linux/arm64 file declares `castagnoliUpdate` and `ieeeUpdate` without a body, where the
amd64 package is a whole-file hand-own with no such partial. The 57 declarations are all linux/arm64's, in ten
packages (internal/runtime/atomic 26, runtime 10, syscall 6, vendor/golang.org/x/sys/cpu 4, internal/cpu 3,
internal/runtime/sys 3, hash/crc32 2, internal/bytealg 1, internal/chacha8rand 1, math 1), and 23 of them are stubbed.
The old fold had counted 73 stubs and 17 packages that do not exist. The 54 pushed lines are the same lines in all
three readings.

### 6.7 The syscall row at 50 + 9: the host, not the seat

A CI sweep of the seat read syscall on linux as 50 matched and 9 disclosed, against a banked 42 and 17. **Control:**
the same leg on master `ae69e4ce74` (run 42788267168) reads 50 and 9, with the same nine names. The seat changes no
compile membership in the package, so nothing is re-banked and no disclosure retires.

- The nine: `TestAllThreadsSyscall`, `TestAllThreadsSyscallBlockedSyscall`, `TestAllThreadsSyscallError`,
  `TestCloneNEWUSERAndRemap/setgroups=false` and `=true`, `TestEmptyCredGroupsDisableSetgroups`, `TestPidFDWithUserNS`
  (Go passes, C# skips), `TestExecPtrace` and `TestPrlimitFileLimit` (Go passes, C# fails).
- The other eight come from twelve disclosures the manifest carries for tests Go itself skips without root:
  `TestAmbientCaps`, `TestAmbientCapsUserns`, `TestCloneTimeNamespace`, `TestDeathSignalSetuid`, `TestGroupCleanup`,
  `TestGroupCleanupUserNamespace`, `TestRlimitRestored`, `TestUnshare`, `TestUnshareMountNameSpace`,
  `TestUnshareMountNameSpaceChroot`, `TestUnshareUidGidMapping`, `TestUseCgroupFD`. The bank is a root host's reading
  (`docs/phase4/rebank-1.26.10.3/linux/P1-2.tsv`), where they run and differ; on the CI runner both sides skip and
  the row reads them as matched. The repository holds the bank's count of 17 and not its names, so which eight of the
  twelve is not read here.

### 6.8 Readers named and not changed here

None of these misses a file today. Each one reads a fixed folder list, and each owes a fixture that fails first, so
they are routed as one follow-up seat and not folded into this one.

| Site | What it lists | Smallest change |
|---|---|---|
| `internal/repoguard/osMemoryRegistrationCensus_test.go:212` | `runtime/` and `runtime/<goos>/` | `lineKeyedFolderTargets` |
| `internal/repoguard/handOwnPackageBoxParity_test.go:92` | a twin's principal folder | `lineKeyedFolderTargets` |
| `sstringTwinGuard_test.go:126` | `isPlatformFolderName` | `isLayoutKeyFolderName` |
| `crosspkgExportedBox_test.go:205` and `:247` | a package's folders | `isLayoutKeyFolderName` |
| `manualConversionScope_test.go:358` | hand-owns by flavour | key folders per target |
| `manualConversionVars_test.go:322` | the same (it can only fail falsely) | key folders per target |
| `internal/repoguard/nativeCallGateWindows_test.go:108` | the windows runtime tree, without `runtime/amd64/` | `platformLayoutFolders` |
| `internal/repoguard/nativeBoundaryBoxDeref_test.go:163` | per-GOOS folders | key folders per target |
| `internal/repoguard/darwinLinknamePulls_test.go:186` | per-GOOS folders (false red only) | key folders per target |
| `internal/repoguard/handOwnTypeAccessibility_test.go:190` | per-GOOS folders | key folders per target |
| `linknameForwardRegistry_test.go:347` and `:543` | GOARCH fixed at amd64 | one context per universe target |
| `packageInputDigest.go:103` | flat `*_impl.cs` only (module packages) | walk the key folders |
| `src/reconvert-deletions.ps1:643` | `$KnownGoos`, a hop-time tool | the key list, pinned like the other two |
| `src/h8-comparand.sh:440`, `:488`, `:498` | case lists, a hop-time tool | the key list |
| `src/tests/Behavioral/check-solution-integrity.ps1:87` | three per-GOOS graphs, none for linux/arm64 | a fourth graph |
| `src/push-nuget.ps1:1785` | `$goosNames` | review when a per-arch package ships |

The Go rows come from a read-only audit: only the two fixed in `25bc6c1720` were run against the tree.

### 6.9 Gates on the merged tip

Read on the box after the merge of batch 281 and the fixes above, every reader the box can run. The whole Go suite
is `go test -timeout 60m ./...`, all sixteen packages, where §6.2's reading was the root package and repoguard.

| Gate | Tip | Reading |
|---|---|---|
| Four-target apply, a second time | `25bc6c1720` | convert rc 0 in 1,925 s; four targets at 0 failed; 1 status row where 0 was predicted (below) |
| `go vet ./...` | `25bc6c1720`, `e0a80cf3a0` | rc 0 |
| `go test ./...` | `25bc6c1720` | **rc 1** in 979 s: 10 packages ok, the root package red on two tests, both naming files this seat added |
| `go test ./...` | `e0a80cf3a0` | rc 0 in 972 s: 11 packages ok, 5 without test files, 0 fails |
| go2cs-gen tests | `25bc6c1720` | 470 passed of 470 |
| GolibTests, windows flavour, Release | `25bc6c1720` | build rc 0 with 0 errors; 2,476 passed, 38 skipped, 0 failed of 2,514 |
| `go2cs.slnx`, Debug | `e0a80cf3a0` | rc 0 in 487 s, 0 distinct errors (41 GB free to 35) |

**The two reds were this seat's own.** `TestLicensingConverterHeaders` named `layoutKeyFoldReaders_test.go`, added in
`25bc6c1720` without the converter header; `TestProjitemsRegistersEveryGoSource` named that file and
`internal/repoguard/layoutKeyLists_test.go`, added in `cfa79316a2`, as unregistered. Each of those two commits was
gated on its own package's named tests. `e0a80cf3a0` adds the header and the two entries, and changes nothing a C#
build reads, so the go2cs-gen and GolibTests readings above stand for it.

**The one row of the second apply is batch 281's.** The apply wrote
`src/core/internal/goexperiment/darwin/exp_dwarf5_off.cs`, a file the tree does not hold. Batch 281 merged the seat
that resolves tool tags for the target platform (`14bea07f1e`, merge `d914d8b360`), whose stated footprint is that
one file; the merge carries five converter files and no corpus file, and master at `ae69e4ce74` holds
`exp_dwarf5_on.cs` under `linux/` and `windows/` only. Master's own apply was not run for this block; the file that
resolves the tags, `directiveOperations.go`, is byte-identical between master and this tip. The row is not committed
here: it is that seat's footprint, routed with this block.

**One reading is void, and is named so it is not read as a result.** The first `go2cs.slnx` build on `25bc6c1720`
returned rc 1 in 439 s with 0 compile errors: the disk went from 41 GB free to 0. The gate script had lost the three
copy-local hard-link properties the build of §6.2 ran with; the reading in the table is the rerun with them.

**Not read on the box.** GolibTests at the linux and darwin flavours, the behavioral harness, the census on the four
runners and the release smoke are CI legs. They are dispatched on the pushed tip and reported with their run numbers.

### 6.10 After batch 282 (dated block, 2026-11-10)

Batch 282 landed (master `38caf3558a`) before this section's results were posted, and the seat took it in as a merge,
`2c1d4e4acd`, the way it took batch 281. Every reading of §6.9 was read again on that tip, so the findings above are
scored on the base they will seat on.

**The merge.** One conflict, `docs/ValidatedTestPackages.md`: batch 282 rewrote the `go/internal/gcimporter` row and
this seat re-points one cite in it (`runtime/linux/extern.cs` to `runtime/linux-amd64/extern.cs`); resolved as
master's row with the cite re-pointed. The four corpus files batch 282 changes sit in per-GOOS folders this seat did
not move, and merged as plain modifications.

**Batch 282's own files, read for a fixed folder list.** The 54 files the batch adds or changes outside `docs/` were
read with one expression for the shapes §6.4 found (quoted GOOS names side by side, an alternation, a corpus path
that names a GOOS folder). 11 lines match, all of them Go package paths (`core/internal/syscall/windows/…`) in
`go2cs.slnx` and `GolibTests.csproj`; none is a list of layout folders. The control: the same expression matches the
lines that `d2c32ec082`, `d211483a97` and `25bc6c1720` removed (1, 2 and 3 lines). The first pass read 53 files: git
quotes a path with a non-ASCII name, and the one such file (a golib box) was skipped as missing. The count of files
read, set beside the count of files changed, showed it.

| Gate | Tip | Reading |
|---|---|---|
| `go vet ./...` | `2c1d4e4acd` | rc 0 |
| `go test ./...` | `2c1d4e4acd` | rc 0 in 974 s: 11 packages ok, 5 without test files, 0 fails |
| go2cs-gen tests | `2c1d4e4acd` | 470 passed of 470 |
| GolibTests, windows flavour, Release | `2c1d4e4acd` | build rc 0 with 0 errors; 2,484 passed, 38 skipped, 0 failed of 2,522 (§6.9 read 2,514; batch 282 adds tests) |
| `go2cs.slnx`, Debug | `2c1d4e4acd` | rc 0 in 457 s, 0 distinct errors |
| Four-target apply, a third time | `2c1d4e4acd` | convert rc 0 in 1,668 s; 7 status rows where 1 was predicted (below) |

**The apply: predicted 1 row, read 7, none of them this seat's.** The prediction, written before the run, was the
one file of §6.9 and no row under a `linux-arm64` folder. Both held. The other six rows are three flat files at
2 lines removed and 1 added each, `go/ast/resolve.cs`, `go/types/scope2.cs` and `net/http/h2_bundle.cs`, and their
three `package_info.cs` at 1 and 1. Each is one `for` header that now carries the re-alias,
`for (; Ꮡs != nil; Ꮡs = s.parent, s = ref Ꮡs.DerefOrNull())`, where the tree holds it as the body's first statement.
That is, file for file and line for line, the footprint batch 282's merge of the for-clause re-alias seat states
(`0fb8ea1fd2`, merge `7dcbfb74b4`: "6 files on every target"); the merge carries the converter change and no corpus
file, and master at `38caf3558a` holds the old lines.

The prediction missed by its reasoning, not by its measurement: it read "batch 282 commits no converted file" as
"batch 282's converter seats have no footprint", when the merge message states one. Master's own apply was not run
for this block; `visitForStmt.go`, `visitAssignStmt.go` and `visitorState.go` are byte-identical between master and
this tip.

Neither footprint is committed here: seven rows, two seats (batch 281's tool-tags seat, one file; batch 282's
for-clause seat, six), routed with this block. The tree was restored by status to 0 rows.

**Not read on the box**, as in §6.9: GolibTests at the linux and darwin flavours, the behavioral harness, the census
on the four runners and the release smoke are CI legs, dispatched on the pushed tip.

### 6.11 The readers of §6.8, swept (dated block, 2026-11-10, P2)

The follow-up seat §6.8 routes, cut on this record's tip (`012b4566e7`) as `claude/p2-layout-key-guards`. Every row
got a fixture that plants a file in a layout key folder (or, for the GOROOT readers, a file only linux/arm64 builds)
and reds at the base, then a fix that reads the folder set from the universe: `platformUniverse` and the corpus
lattice in the converter package, `lineKeyedUniverse` and `lineKeyedFolderTargets` in repoguard, and for each tool a
universe literal that `TestLayoutKeyListsAreTheUniverses` now pins (its new `layoutUniverseLists`), with the folders
derived from it. Red commits `4f50aa8872`, `8ab08a708e`, `c56da40674`; fixes `95988a3989`, `c2d5cf917d`,
`19dc65cedd`, `59ecf8c954`, `286c637ec0`.

| Site | At the base | After |
|---|---|---|
| `osMemoryRegistrationCensus` | runtime/<goos>/ at three targets | flat plus each target's key folders, four targets; files read 218, 234, 230 and 233 (linux/arm64), callers unchanged |
| `handOwnPackageBoxParity` (twinPrincipal) | a twin in a GOOS folder only | any key folder |
| `nativeCallGateWindows` | runtime/windows/ | runtime/{amd64,windows,windows-amd64}/; 76 sites over 3 files, as before |
| `nativeBoundaryBoxDeref` | syscall/windows/ | adds syscall/amd64/ and syscall/windows-amd64/; 67 files, as before |
| `darwinLinknamePulls` | a part in amd64/ never fills (false red); darwin-amd64/ unread | grouped by the darwin/amd64 build; 15 pulls, 13 filled, as before |
| `sstringTwinGuard` | `isPlatformFolderName` | `isCorpusLayoutKeyFolder` |
| `crosspkgExportedBox` (two sites) | GOOS folders | every candidate key folder |
| `manualConversionScope` | witness per GOOS, <pkg>/<goos>/ only | witness per target; a scope is unbodied at any target of its GOOS that lacks the body |
| `manualConversionVars` | <pkg>/<goos>/ (false red only) | missing per target of the scope's GOOS |
| `handOwnTypeAccessibility` | the info file's own folder | its whole assembly; marked files read 117 to 338, once per info file that compiles each; 0 duplicates |
| `linknameForwardRegistry` (two sites) | GOARCH fixed at amd64 | `corpusTargets = platformUniverse`; 27 rows checked, 35 walked |
| `packageInputDigest.go` | flat `*_impl.cs` | flat plus the target's key folders, named with the folder; a flat companion keeps its name |
| `reconvert-deletions.ps1` | `$KnownGoos` | `$layoutUniverse`, pinned; each key folder asked at its own flavour |
| `h8-comparand.sh` (three sites) | GOOS case lists | `LAYOUT_UNIVERSE`, pinned; view takes `<goos>/<arch>`; selftest 46 of 46 |
| `check-solution-integrity.ps1` | three GOOS graphs | four target graphs; `$(GoTargetArch)` conditions modelled |
| `push-nuget.ps1` `$goosNames` | reviewed, not changed | below |

**reconvert-deletions, a dry run on a copy of this tree** (source and target both go1.26.10, linux/amd64, every file
a candidate): at the base 161 files in amd64/, linux-amd64/ and linux-arm64/ folders read NOT-A-CONVERSION-TARGET,
as packages such as `syscall/linux-amd64`, so nothing checked them. With the fix they read KEEP-SELECTED (+129),
KEEP-METADATA (+25) and PROTECTED (+7). Both arms: 0 DELETE, 0 unresolved.

**check-solution-integrity's control.** `-InjectReference 'internal/cpu=math'` closes a cycle at the three amd64
targets and not at linux/arm64, because math references internal/cpu only under `'$(GoTargetArch)'=='amd64'`; the
base script read that condition as unconditional and reported the cycle on its linux graph.

**push-nuget.ps1, the review.** Fourteen packages carry key folders and no GOOS folder (math, hash/crc32,
internal/abi, internal/bytealg and the rest of the amd64/ plus linux-arm64/ set), so `$goosNames` does not split
them by RID. The RIDs this release ships are all amd64 builds (win-x64, linux-x64, osx-x64, with osx-arm64 taking
osx-x64's bytes), and an amd64 build of such a package compiles the same flat and amd64/ files on every GOOS, so
one asset is correct today. The day a linux-arm64 RID ships these packages need a split; that is S6's packaging
decision and is left to it.
