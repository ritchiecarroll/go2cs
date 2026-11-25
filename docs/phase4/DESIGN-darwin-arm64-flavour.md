# DESIGN — a darwin/arm64 standard-library flavour

> **STATUS: DESIGN, REVISION 1, for P3's read and COORD's rulings (lane P5, 2026-11-24; branch
> `claude/p5-darwin-arm64-design`, on master `cb6def82a2`).** The owner rerouted the darwin/arm64 flavour into `.5`
> with P5 leading (COORD 2026-11-24 22:38Z). It is the fifth target of the corpus, and it follows
> [`DESIGN-linux-arm64-flavour.md`](DESIGN-linux-arm64-flavour.md) (the template, cited below as LA §n), which built
> the general machinery: orthogonal layout keys, the per-target readers, the pair-keyed pack, the arch through
> `-tests`.
>
> Every number below is MEASURED here (M1c, the censuses, the hosted probe), CITED at file:line on master
> `cb6def82a2`, or labelled an ESTIMATE or a PREDICTION. **No seat after D2's design gate is cut before §10's
> rulings.**
>
> **REVISION 2 (2026-11-25), folding P3's verdict on revision 1 (APPROVE WITH CHANGES; R1, R2 and R4 yes, R3 not as
> worded).** What changed:
> 1. **R3 is re-scoped, not removed** (P3 MUST-FIX 1): the darwin `GOARCH` displacement and its two stand-ins stay
>    for TARGET darwin/amd64, because that flavour keeps running in arm64 processes after the flip; darwin/arm64
>    takes Go's converted constant. D3 owes a behavioral row (§4.2, §10).
> 2. **The interim is stated** (P3 MUST-FIX 2): the macos-15 sweep legs pin `-platforms darwin/amd64` from D2 until
>    D7 removes the pin (§6, R5). nugetgo-behavior reads packed bytes and moves with D6, not D2.
> 3. **A new finding moves part of D4 into D2** (P3's question 4): at the flip, the cpu stand-in's routing breaks
>    E's build (CS0759) and silently strips Apple-silicon detection from darwin/amd64. §5 and R6.
> 4. M1c's control, the per-root size, the measured walls, the hosted footprint stage, the build file's error
>    text and the GoBreakFloor reading at D6 (P3 items 3, 5, 6, 7).

## 0. Summary

- **What it gives.** An osx-arm64 consumer of `go.*` today runs the darwin/amd64 emission: `internal/goarch` reads
  amd64, the files Go selects for amd64 are compiled, syscall binds the amd64 symbol names, and three hand-owns
  stand in for what an arm64 build would read natively (§1). The flavour gives that consumer Go's own darwin/arm64
  selection, and then H's cgo osx-arm64 pack.
- **Why it is smaller than linux/arm64.** linux/arm64 paid for the general machinery, and darwin calls libSystem by
  name, so there are no syscall numbers to renumber (most of LA's S3). ESTIMATE: 9 to 14 lane-days in all, about 6
  to 8 calendar days with three lanes, against 18 to 28 for linux/arm64.
- **The variadic risk is closed.** The sizing's risk 1 (Apple's arm64 ABI passes variadic arguments on the stack) is
  MEASURED green on osx-arm64: golib's `GoLibcCall` fills the stack slot, and has since S9 (§4.1, BOARD 2026-11-24).
- **The one rule this design re-verifies** is LA §2.2's: the split takes GOOS keys first. At five targets the order
  becomes observable for 7 sharing sets (§2.2). GOOS-first is kept: it leaves every existing placement in place, and
  the five-target census (M1c, §2.4) measures it as the cheaper order too (2542 copies against 2649).
- **The census in one line (M1c).** No flat file moves; linux/arm64 promotes 44 files to `arm64/`; darwin/amd64
  demotes 19 to `darwin-amd64/`; darwin/arm64 adds 16 of its own; 4 hand-owns follow their principals; 15 packages
  gain a folder and none becomes newly layout-carrying.
- **The difference that changes the seat order.** On linux, the arm leg compiled the wrong flavour unlabelled until
  the arch was bound, so the binding seated with the flip (LA §11.5). On darwin the csproj default (amd64) stays a
  correct build, the darwin/amd64 flavour, so D2 does not break any leg. But two things flip by themselves at D2
  and must be planned for (§6): the osx-arm64 `-tests` legs start converting at darwin/arm64 (S1b's
  `corpusFlavourOf`), and the darwin census compiles only darwin/amd64 until D5 binds the arch. Revision 2 pins the
  first (R5) and seats D5 with D2 for the second (R2).
- **The darwin/amd64 flavour keeps running on Apple silicon after the flip**, in a pack built before D6 and in any
  local-refs build with `GoTargetArch` empty (`src/core/Directory.Build.props`:57-58 defaults it to amd64). Every
  stand-in that serves that case is kept, scoped to the darwin/amd64 target (§4.2, §5).

## 1. What an osx-arm64 consumer runs today

- **The flavour.** `corpusFlavourOf` (`src/go2cs/testsCorpusArch.go`:30) serves any target outside the universe by
  the first universe target of its GOOS: darwin/arm64 by darwin/amd64. `src/_paths.ps1`:135 restates it for the
  harness. The pack ships the same bytes under both mac RIDs (`src/push-nuget.ps1`:1766-1767, and the twin assert at
  :2131-2136).
- **The stand-ins that exist because of it** (revision 2: each is kept and re-scoped to target darwin/amd64, §4.2
  and §5; none is retired while that flavour can run in an arm64 process):
  - `runtime/darwin/goarch_darwin_impl.cs`: `runtime.GOARCH` reads the PROCESS architecture, so it says arm64 on
    Apple silicon. The converter displaces the const for darwin (`manualConversionConsts`,
    `src/go2cs/manualTypeOperations.go`:153). `internal/goarch` keeps amd64 on purpose.
  - `internal/buildcfg/darwin/zbootstrap_darwin_impl.cs`: `defaultGOARCH` follows that member.
  - `internal/cpu/darwin/cpu_arm64_darwin_impl.cs` with `internal/cpu/amd64/cpu_arm64_process_impl.cs`: Apple-silicon
    feature detection in a flavour that does not compile Go's `cpu_arm64_darwin.go`.
- **The host-of-record exemption.** `GoHostOfRecord` (`src/core/golib/GoHostOfRecord.cs`:48) admits a darwin/amd64
  build on an arm64 process. The flavour removes the need on osx-arm64; the exemption stays for any consumer still
  on the old flavour.
- **The bank.** ValidatedTestPackages.md's macOS line reads the darwin/amd64 emission on both chips. The oracle on
  osx-arm64 already runs at arm64 (`run-validated-sweep.ps1`:143-147, ruling (A)), so the arch-count rows
  (go/internal/gcimporter 634 against 636, runtime) are the ones that move (§7).

## 2. The layout at five targets

### 2.1 The universe and its keys

`U' = {W, L, D, A, E}`, E being darwin/arm64. The rule is LA §2.1, unchanged; the lattice gains two keys and
changes one cover.

| Key | Cover in `U` (today) | Cover in `U'` |
|---|---|---|
| flat | {W, L, D, A} | {W, L, D, A, E} |
| `windows/` | {W} | {W} |
| `linux/` | {L, A} | {L, A} |
| `darwin/` | {D} | **{D, E}** |
| `amd64/` | {W, L, D} | {W, L, D} |
| `arm64/` | not admitted (one arm64 target) | **{A, E}, admitted** |
| `linux-amd64/`, `linux-arm64/` | {L}, {A} | unchanged |
| `darwin-amd64/` | never written (`darwin/` wins the tie) | **{D}**, now the only exact cover of {D} |
| `darwin-arm64/` | absent | **{E}** |

### 2.2 The split order, re-verified (LA §2.2 asked for this)

All 31 sharing sets of `U'` were homed under both orders by the same rule (script in §9's kit). They agree on 24.
On the other 7 the order is observable:

| `S` | GOOS-first (kept) | Arch-first |
|---|---|---|
| {W, L, D, A} | `windows/` + `linux/` + `darwin-amd64/` | `amd64/` + `linux-arm64/` |
| {W, L, D, E} | `windows/` + `darwin/` + `linux-amd64/` | `amd64/` + `darwin-arm64/` |
| {L, D, A, E} | `linux/` + `darwin/` | `arm64/` + `linux-amd64/` + `darwin-amd64/` |
| {W, L, A, E} | `windows/` + `linux/` + `darwin-arm64/` | `windows/` + `arm64/` + `linux-amd64/` |
| {W, D, A, E} | `windows/` + `darwin/` + `linux-arm64/` | `windows/` + `arm64/` + `darwin-amd64/` |
| {L, A, E} | `linux/` + `darwin-arm64/` | `arm64/` + `linux-amd64/` |
| {D, A, E} | `darwin/` + `linux-arm64/` | `arm64/` + `darwin-amd64/` |

- **Why GOOS-first stays (and M1c confirms it is also the cheaper order, §2.4).** LA's reason holds: it keeps every existing GOOS placement where it is. The test case is
  today's 115 `linux/` + `darwin/` splits (LA §2.1, {L, D, A}). Where E joins darwin/amd64's copy, the set becomes
  {L, D, A, E}, which GOOS-first leaves exactly where it is and arch-first spreads over three new folders.
- **What it was expected to cost.** The first row looked like the large class: a file all four of today's targets
  share and E emits differently, which GOOS-first writes three times and arch-first twice. M1c measured it EMPTY
  (§2.4).
- **Ties go to the coarser key** (LA §2.2), so `darwin/` keeps every file D and E share, and `darwin-amd64/` is
  written only for what E does not reproduce.

### 2.3 The invariants for the four existing flavours

For each `T` in {W, L, D, A}, LA §2.3's I1 (source set), I3 (project files) and I4 (compile-item order) carry over
unchanged. I2, read through LA's general form ("adding T' moves a file only out of a key T' joins and does not
reproduce, or into a coarser key whose cover T' completes"), allows exactly these moves:

- **Demotion, out of the keys E joins** (flat and `darwin/`): flat to the GOOS-first split of {W, L, D, A}; `darwin/`
  to `darwin-amd64/`.
- **Promotion, into the key E completes** (`arm64/`): `linux-arm64/` to `arm64/`, for a file E emits byte-equal.
- **Nothing in `windows/`, `linux/`, `amd64/` or `linux-amd64/` moves.** E is in none of their covers, and no move
  can promote into them.

PREDICTION (from the source census, before M1c is read): of the 81 files in `linux-arm64/` folders, the 22 that
both arm64 GOOS select move to `arm64/` where E's bytes are equal; the 9 darwin files named `*_amd64` move to
`darwin-amd64/`; flat demotions are the files whose conversion folds an arch-dependent constant differently at E.

### 2.4 M1c, the five-target census (measured 2026-11-24)

- **Method.** The converter built at `cb6def82a2`; one seed of `src/core` plus `src/version.props` (4699 `.cs`); five
  seeded roots, one on disk at a time, each `-stdlib -comments -platforms <target>` with Go 1.26.11 and the default
  tag set. All five exited 0 (walls 209-222 s). Each root was manifested by sentinel mtime and deleted. A scratch
  copy of the converter forced every source to be written (`alwaysWriteSources`, which only the census sets
  otherwise): the first run without it manifested 115 files for windows/amd64, the incremental writer's skips, and
  was discarded. The classifier applies LA §2.1 over `U` and over `U'`.
- **Scope: 2246 logical (package, name) `.cs` pairs**, plus 35 `package_init.cs`, 30 `.cs.auto` and the project
  files, read separately below. Hand-owns are placed by their principal (below).
- **Control: the rule reproduces today's tree.** Run at `U`, it places all 7883 (file, target) pairs of the four
  existing targets exactly where their single-target writers put them: 0 differ. The control used the SAME
  forced-write scratch binary as the darwin/arm64 root (one build, five roots, one after another), so the forced
  write is not a difference between the control and the reading. It is still not the master binary: D2 re-derives
  the 44 / 19 / 16 / 4 counts with the seat's own converter, and a count that moves is a finding before an apply.
- **Versions by sharing set at `U'`:**

  | `S` | Versions | Packages | Home |
  |---|---|---|---|
  | {W, L, D, A, E} | 1676 | 343 | flat |
  | {W} | 160 | 33 | `windows/` |
  | {D, E} | 120 | 20 | `darwin/` |
  | {L, A} | 117 | 18 | `linux/` |
  | {L, D, A, E} | 107 | 24 | `linux/` + `darwin/` |
  | {W, L, D} | 47 | 16 | `amd64/` |
  | {A, E} | 44 | 13 | `arm64/` |
  | {A} | 29 | 7 | `linux-arm64/` |
  | {L} | 24 | 6 | `linux-amd64/` |
  | {W, D, E} | 20 | 7 | `windows/` + `darwin/` |
  | {E} | 16 | 5 | `darwin-arm64/` |
  | {D} | 15 | 5 | `darwin-amd64/` |
  | {W, L, A} | 11 | 5 | `windows/` + `linux/` |
  | {W, L} | 4 | 3 | `windows/` + `linux-amd64/` |
  | {L, D} | 4 | 1 | `darwin-amd64/` + `linux-amd64/` |
  | {D, A, E} | 1 | 1 | `darwin/` + `linux-arm64/` |

- **No flat file moves.** {W, L, D, A} does not occur: darwin/arm64 reproduces every flat file byte for byte. So the
  first row of §2.2's table is empty, and the split order's cost is the other way round from §2.2's guess:
  GOOS-first writes **2542** copies, arch-first **2649** (today's tree: 2526). The difference is the 107 {L, D, A, E}
  files, which GOOS-first leaves in `linux/` + `darwin/` where they are.
- **The moves for the four existing targets are the two I2 allows, and nothing else:**

  | Move | Files | Packages |
  |---|---|---|
  | linux/arm64: `linux-arm64/` to `arm64/` (promotion) | 44 | 13 |
  | darwin/amd64: `darwin/` to `darwin-amd64/` (demotion) | 19 | 5 |

  - The 44 include 4 `package_info.cs` and runtime's `malloc`, `mheap`, `stack`, `panic`, `traceback`, `sema`; the
    13 packages are exactly those holding a promoted file (crypto/internal/fips140deps/cpu, hash/crc32, internal/abi,
    internal/bytealg, internal/chacha8rand, internal/cpu, internal/goarch, internal/runtime/atomic,
    internal/runtime/gc/scan, internal/runtime/sys, math, runtime, vendor/golang.org/x/sys/cpu). 29 files stay in
    `linux-arm64/` (syscall tables, the HWCAP readers).
  - The 19 are the 9 `*_amd64` files, 4 `package_info.cs` (internal/cpu, runtime, syscall, x/sys/cpu), runtime's
    `malloc`, `mheap`, `stack`, internal/cpu's `cpu_x86_darwin`, and x/sys/cpu's `cpu_darwin_x86` and
    `syscall_darwin_x86_gc`.
- **What darwin/arm64 compiles:** flat 1676, `darwin/` 248, `arm64/` 44, `darwin-arm64/` 16. The 16 are its own
  syscall tables (`zerrors`, `zsyscall`, `zsysnum`, `ztypes`, `syscall_darwin_arm64`), runtime's `defs`, `os`, `signal`
  and `sys` files, pprof's `defs`, `cpu_arm64_darwin`, x/sys's `cpu_other_arm64`, and 4 `package_info.cs`.
- **15 packages gain a folder; none becomes newly layout-carrying** (all 15 already carry the arch block): `arm64/` in
  12, `darwin-amd64/` and `darwin-arm64/` in internal/cpu, runtime, runtime/pprof, syscall and x/sys/cpu.
- **`package_init.cs` (35) and `.cs.auto` (30):** darwin/arm64's equal another target's class everywhere, so neither
  adds a class.
- **Project files:** the 7 written at E differ from the tree, as expected: a single-target writer does not lay the
  five-target arch block, which D2's merge writes.
- **Hand-owns, placed by their principal's emitter set** (`platformHandOwn.go`; the placeholder narrowing of :358 is
  not modelled here, so this is a PREDICTION for D2's apply): 64 hand-owns have a principal emitted outside flat, and
  4 move: `crc32_arm64_impl.cs`, `dit_arm64_impl.cs` and `exp2_asm_impl.cs` from `linux-arm64/` to `arm64/`, and
  `cpu_arm64_darwin_impl.cs` from `darwin/` to `darwin-arm64/`, now that its principal is emitted. Revision 2: the
  fourth move is WRONG as it stands, and §5 says why and what D2 does instead. D2 scores this PREDICTION and §4.2's
  zsyscall placeholder PREDICTION as worded.
- **Scoring §2.3's PREDICTION**, which stands above as written before M1c was read: promotions MISSED (22 predicted,
  44 measured; the prediction counted only files both arm64 GOOS select by name, and missed the shared converted files
  such as `malloc` and the `package_info.cs`); darwin demotions MISSED (9 predicted, 19 measured, for the same reason);
  flat demotions MISSED in direction (some predicted, 0 measured). The mechanism held (every move is one of I2's two);
  the specifics did not, which is why the census is the input to D2's prediction and the source census is not.

- **D2's footprint PREDICTION, from M1c:** the apply moves exactly the 44 + 19 files and the 4 hand-owns above, adds
  the 16 `darwin-arm64/` files, rewrites the project files of the 15 packages that gain a folder (the arch block's
  five-target form); nothing else in `src/core` changes. Outside it, the metadata record (`stdlib-metadata.txt`) re-keys
  the sections of those 15 packages by LA §11.2's folder rule. A path outside that list is a finding before it is an
  apply. Revision 2 adds, in `src/core`: `runtime/darwin/goarch_darwin_impl.cs` and
  `internal/buildcfg/darwin/zbootstrap_darwin_impl.cs` move to `darwin-amd64/` (the 4 hand-own moves become 6, and
  internal/buildcfg becomes the 16th package to gain a folder, PREDICTION); the cpu stand-in becomes
  `internal/cpu/darwin-amd64/cpu_arm64_process_darwin_impl.cs` (renamed, content unchanged) and a new
  `internal/cpu/darwin-arm64/cpu_arm64_darwin_impl.cs` carries E's `osInit`. Outside it: the converter's target scope
  and its two registrations, `run-validated-sweep.ps1`'s pass-through, and os-matrix.yml's two pin lines.

### 2.5 Enforcement: the readers of the universe

LA §11.3 and §11.7 found that a layout change has readers outside the merge. The universe literal or a key list sits
in these places on master; each is held to `platformUniverse` by a guard or is updated in D2:

| Reader | Where | Held by |
|---|---|---|
| the converter's constant | `src/go2cs/platformLattice.go`:48 | itself |
| the harness mirror | `src/_paths.ps1`:127 | `testsCorpusArch_test.go`:152 |
| the corpus build file's universe check | `platformLayout.go`:453-461 | `TestCorpusBuildFileCarriesTheUniverseCheck` |
| the corpus build file itself | `src/core/Directory.Build.props`:57-58 (`_GoLayoutArch` defaults to amd64) and :61-62 (the error text lists the four targets) | D2: the condition and the text admit darwin/arm64; the amd64 default stays (§1, §4.2) |
| the comment census | `internal/commentcensus/census_test.go`:537 | itself |
| line-keyed records | repoguard `lineKeyedRecords_test.go`:438 | itself |
| the stack-append harvest | `stackAppendHints.go`:67 (`= platformUniverse`) | by construction; five roots per harvest |
| the cgo route's target set | `cgoRoute.go`:1169 | by construction |
| the pack's RID map | `push-nuget.ps1`:1762-1768 | D6; its twin assert reads the map |
| the metadata record | `stdlib-metadata.txt` sections | D2: `@darwin` becomes `@darwin-amd64` and `@darwin-arm64` where a package splits by arch (LA §11.2's rule) |

The gate is LA §11.7's, as ruled in `.claude/rules/corpus.md`: `go test ./...` in full, GolibTests on every
flavour, the comment census and the behavioral harness, each read on the applied tree before D2 is posted.

## 3. Packaging and consumer RID selection (D6)

- `push-nuget.ps1`:1767 maps `osx-arm64` to `darwin/amd64`. D6 maps it to `darwin/arm64`, which gives it its own build
  pass (the pair-keyed pass of LA §3), and the twin assert stops pairing the two mac RIDs by construction.
- The RID-split set is derived from the tree (:1790-1798), so packages that split only at darwin/arm64 join it with no
  list to edit. NugetgoFlavors and its test, and ApiCompat's new-RID rule (zerrors' 55 differing lines are public
  constants) are the rest of D6.
- **Order.** D6 can lag D2 safely: until it lands, osx-arm64 keeps shipping darwin/amd64 bytes, which is today's
  behaviour, and revision 2 keeps that pack's `runtime.GOARCH` right on Apple silicon (§4.2). It must land before H's
  cgo osx-arm64 arm.
- **GoBreakFloor at D6 (P3's NIT).** It depends on whether the package already ships an osx-arm64 asset. A package
  RID-split today ships `runtimes/osx-arm64/lib` with darwin/amd64 bytes, and the guard compares that asset against
  the last published one (`src/tools/pushnuget/Test-GoApiCompat.ps1`:132-141 is the fixture: a break on the
  osx-arm64 asset alone is red). So a public member that exists at amd64 and not at arm64 (syscall's zerrors
  constants are the known case) is a BREAK, not a new-RID baseline: D6 owes the suppression entries, the floor rule
  for a reachable package, and a NEWS paragraph, and asks P2 (the floor's owner) before it packs. A package that
  becomes RID-split only at D6 gains new `runtimes/` folders with no baseline, and its `lib/` is compared as before
  (:145-146). D6's first step is the list of packages in each class.

## 4. The darwin hand-owns at arm64 (D3)

### 4.1 Variadic libc calls: closed (MEASURED)

Hosted run 45503405339 (golib-tests, filter `DarwinArm64VariadicSlotTests`, branch `claude/p5-variadic-live`
`e439efbd01`): on osx-arm64, open(O_CREAT, 0640) creates the file 0640 and fcntl(F_SETFL, O_NONBLOCK) sets the flag,
and each row's near-miss control (the real value in the register, another value at [sp]) reads the stack value on
Apple silicon. So the callee reads the slot, and `GoLibcCall.VariadicStackSlot` fills it as Go's
`syscallN_trampoline` does. osx-x64 reads the register value in both controls, as predicted. No dispatcher seat is
owed; the rows stay as the flavour's guard. BOARD entry: `claude/p5-variadic-live` `8fb9b05bd1`.

### 4.2 The population (MEASURED at `cb6def82a2`)

- 27 `*_impl.cs` under `darwin/`, and 3 whole-file `[module: GoManualConversion]` replacements (`os/darwin/dir_darwin_impl.cs`,
  `runtime/darwin/lock_sema_impl.cs`, `runtime/darwin/signal_posix_darwin_impl.cs`). The sizing's "53 whole-file"
  counted every file that names the attribute, placeholders included; it is withdrawn.
- No darwin hand-own is named for amd64, and a hand-own is placed by the targets whose emission needs it (its
  principal's placeholder, `platformHandOwn.go`:358-378). The converted `syscall/darwin/zsyscall_darwin_amd64.cs`
  carries two placeholders (pipe, writev) displaced to `*_impl.cs`. PREDICTION: its arm64 twin carries the same two,
  so `writev_darwin_impl.cs` and the pipe body stay in `darwin/` serving both arches, and nothing like LA §11.4's
  four owed bodies appears. D2's apply checks the displacement both ways (the seam rule); a miss is a D3 row.
- **The three stand-ins are RE-SCOPED to target darwin/amd64, not retired** (revision 2; R3). Revision 1 removed
  the `GOARCH` displacement at the flip. P3 refused that: the darwin/amd64 flavour keeps running in arm64 processes
  after the flip, (a) in any pack built between D2 and D6 (§3), and (b) in a local-refs build on Apple silicon with
  `GoTargetArch` empty (`Directory.Build.props`:57-58), for good. Removing the displacement would give both
  `runtime.GOARCH == "amd64"`, the BOARD 2026-11-05 defect `manualTypeOperations.go`:146-153 cures. So:
  - **The converter:** the darwin entries of `manualConversionConsts` (`runtime.GOARCH`, `internal/buildcfg`
    `defaultGOARCH`; `manualTypeOperations.go`:153-162) need a TARGET scope, since a GOOS scope covers E too. The
    scope type gains os/arch entries, matched against the conversion's target where today `isManualConstInPackage`
    receives only its GOOS (:166-180); windows keeps its GOOS entry (one windows target). The same scope serves
    §5's `osInit` entry. `manualConversionScope_test.go`'s registry walk is extended to the new entry form.
  - **The files:** `goarch_darwin_impl.cs` and `zbootstrap_darwin_impl.cs` move from `darwin/` to `darwin-amd64/`.
    PREDICTION: the placeholder narrowing (`platformHandOwn.go`:358-378) homes them there once E's emission carries
    the converted constant and no placeholder; if the narrowing reads only function placeholders (its comment names
    `funcPlaceholderLead`), D2 homes them by hand and says so.
  - **darwin/arm64** compiles Go's converted `const GOARCH = goarch.GOARCH`, "arm64", with no stand-in.
  - **When it lands:** in D2, so each file moves once and E never carries a displacement it does not need. (Left
    to D3, E would still compile the stand-ins at D2; they would read "arm64" on Apple silicon, Go's value, so the
    order is a matter of moving files once rather than of correctness.)
  - **D3's behavioral row** (P3): `runtime.GOARCH` and `go/build`'s default context (`build.Default.GOARCH`) on an
    arm64 process, in each flavour (darwin/amd64 built with `GoTargetArch` empty, and darwin/arm64), compared with
    Go on the same Mac. The same row on osx-x64 is the control.
  - **Retirement** is not planned. It would need both the D6 pack and a darwin csproj default that follows the host
    arch, and the second changes what every existing local build compiles; it is not proposed here.
- **First-pass audit for arch assumptions** (a grep for amd64, x86, arm64 and architecture terms, to be read file by
  file in D3): `internal/poll/darwin/runtime_netpoll_impl.cs` 25 lines, `runtime/darwin/goarch_darwin_impl.cs` 18,
  `internal/cpu/darwin/cpu_arm64_darwin_impl.cs` 13, `runtime/darwin/sigaction_impl.cs` 11,
  `syscall/darwin/exec_libc2_impl.cs` 9; 17 more files have 1 to 7.
- **Page size** is read at run time already (`os_darwin_impl.cs`: 16384 on Apple silicon). **mcontext**: no darwin
  hand-own or golib file reads one, though `defs_darwin` differs by 179 lines between the arches.

## 5. cpu.ARM64 on darwin (D4)

Go reads Apple-silicon features with `sysctlbyname("hw.optional.*")` in `internal/cpu/cpu_arm64_darwin.go`
(`osInit`), not HWCAP, called from `cpu_arm64.go`'s `doinit`. At the flip the corpus compiles both for E (M1c:
`arm64/cpu_arm64.cs`, `darwin-arm64/cpu_arm64_darwin.cs`). Revision 1 left the stand-in "beside it" and the sysctl
calls to D4. P3 asked which of the two supplies the feature bits between D2 and D4, and whether E builds at D2 at
all. Read at `cb6def82a2` and against M1c's placement, **it does not, and the four-flavour side breaks silently
too**, so the part of D4 that answers this seats WITH D2 (R6).

- **Today.** darwin/amd64 fills `cpu.ARM64` on an arm64 process through two hand-owns:
  `amd64/cpu_arm64_process_impl.cs` (Go's `doinit` option table, keyed on the PROCESS architecture) declares
  `static partial void osInitArm64();` (:84) and calls it (:76), and `darwin/cpu_arm64_darwin_impl.cs` implements it
  (:31) with Go's `osInit` body minus one ruled departure: `HasDIT` stays false (COORD 2026-11-06; .NET cannot set
  PSTATE.DIT).
- **At D2, as M1c places it.** `cpu_arm64_darwin_impl.cs` follows its principal, now emitted by E alone, to
  `darwin-arm64/` (§2.4). Then:
  1. **E does not build.** The file implements `osInitArm64`, whose only defining declaration is in `amd64/`, which E
     does not compile: CS0759 (an implementing partial method with no defining declaration). D2's own gate stops.
  2. **darwin/amd64 loses Apple-silicon detection, with no diagnostic.** D keeps the defining declaration and loses
     the implementation, and C# removes a call to an unimplemented `void` partial method. `cpu.ARM64` stays unfilled
     on osx-arm64 for every consumer still on that flavour (§4.2's (a) and (b)): the runtime's AES hash selection and
     crypto/tls's AES-GCM preference go back to the divergence run 42138564497 found.
  3. **E would also lose the ruled departure.** E's converted `osInit` is Go's own, and reads `HasDIT` from
     `hw.optional.arm.FEAT_DIT`, which Apple silicon reports.
- **The sysctl calls are already bound for E.** `sysctlEnabled` and `sysctlbynameInt32` are in `cpu_darwin.go`, which
  D and E emit byte-equal (`darwin/`), and `darwin/cpu_darwin_impl.cs` binds `sysctlbyname` in libSystem by name
  (:33-38, not variadic). So "binding the sysctl calls" is not D4's work; revision 1 was wrong to list it.
- **What D2 carries (R6), following LA §2.7's precedent for linux:**
  - **For darwin/amd64:** the stand-in is renamed to a name with no principal, `cpu_arm64_process_darwin_impl.cs`
    (the twin of `linux-amd64/cpu_arm64_process_linux_impl.cs`), and homed in `darwin-amd64/` beside the defining
    declaration's other users. Content unchanged. No Go file has that name, so no target's emission can move it.
  - **For darwin/arm64:** `internal/cpu` `osInit` is registered in `manualConversionFuncs` for TARGET darwin/arm64
    (§4.2's target scope), and `cpu_arm64_darwin_impl.cs` keeps its name and becomes Go's `osInit` with the `HasDIT`
    departure. Its principal homes it in `darwin-arm64/`, where the placeholder is.
  - **Gates in D2:** E builds; D's IL still carries the call to `osInitArm64` with a body (a reflection or IL read
    in GolibTests' darwin leg, or a probe row); the HasDIT departure holds at E. A CS0759 or a missing body is a
    stop, not a D4 follow-up.
- **What stays D4 (C1, from Thursday 06:00Z):** a behavioral row for `cpu.ARM64` on Apple silicon in each flavour
  against Go (every field but `HasDIT`, which is the disclosed departure); the consumers at E: the runtime hash's
  arm64 branch (`hash_impl.cs` `initHashAlgorithm` keys on the process and needs no change, PREDICTION) and
  crypto/tls's `hasGCMAsmARM64`, reusing DESIGN-aeshash-arm64.md; x/sys/cpu's `cpu_other_arm64.go` (E's one added
  vendored file, whose `doinit` detects nothing, as in Go).

## 6. CI (D5), and what flips by itself at D2

- **By itself at D2:** a `-tests` conversion on an arm64 mac targets the corpus flavour of the host default
  (`testsTargetPlatform`, `testsCorpusArch.go`:49), which becomes darwin/arm64 the moment E joins the universe, and
  `testHostPlatformProperties` hands the test host `GoTargetArch=arm64`. So the macos-15 sweep legs read
  the new flavour from D2 on, with no workflow change. `-test-oracle-goarch arm64` stays correct (the oracle's arch
  is now the flavour's).
- **The interim (revision 2, P3 MUST-FIX 2; R5).** The 236 banked rows were read on darwin/amd64, and D3's audit,
  D4's row and D7's re-read come after D2. Unpinned, the first hosted macos-15 sweep after D2 lands would compare
  those rows against darwin/arm64 emissions (gcimporter 634 against 636, syscall, runtime, crypto), and the reds
  would read as a train regression. So D2 pins the old flavour on those legs until D7 removes the pin:
  - `src/run-validated-sweep.ps1` gains a `-Platforms` pass-through threaded to every row's converter call, as
    `-TestOracleGoarch` already is (:148, :1533-1535). An explicit `-platforms` always wins over the host default
    (`testsTargetPlatform`, `testsCorpusArch.go`:43-61), and `testHostPlatformProperties` then hands the host
    `GoTargetArch=amd64`: exactly today's arguments.
  - os-matrix.yml's two sweep calls on a darwin leg (sweep-shard :2882, stack-probe :2150) pass
    `-Platforms darwin/amd64`, one line each, with a comment naming D7 as the seat that removes it.
  - **Not pinned, because they do not move at D2:** nugetgo-behavior and release-smoke consume PACKED bytes, whose
    osx-arm64 asset is darwin/amd64 until D6's RID map (§3); they move with D6, and D6's post names that.
  - The alternative P3 offered (D2 posts the predicted row moves and COORD rules the window red-tolerant) is not
    recommended: it makes every macos-15 red in the window a judgement call, for as long as D7 takes.
- **Not by itself:** the census and golib-tests stages build with the job's `GoTargetArch`, empty on both mac legs
  (os-matrix.yml:551-552), so they compile darwin/amd64 on Apple silicon. The daily darwin census would then compile
  nothing of the new flavour: the shape that once hid a darwin CS0266 for seven days (os-matrix.yml:12-16).
- **D5:** the macos-15 leg binds `arch = arm64` behind the flavour probe, as the linux arm leg does (:564-598);
  release-smoke and aot-smoke already run osx-arm64. reconvert-footprint is single-target per leg
  (`.github/reconvert-footprint.ps1`:139-141 converts `-platforms <goos>/amd64`; darwin runs one leg, os-matrix.yml
  :624-628) and does not cover linux/arm64 either (:571-572). Revision 1 said it "gains the fifth target"; D5 adds a
  darwin/arm64 arm only if COORD wants one, at one target's cost (§8).
- **Recommended (R2):** D5's binding seats WITH D2, so no hosted reading of the new flavour exists that the census
  does not also compile.

## 7. The bank (D7)

- Re-read the osx-arm64 cell of all 236 rows on the new flavour. The grammar already carries a per-RID darwin note
  (`darwin: 7 + 3 (osx-arm64; osx-x64: 10)`); no new key.
- **D7 removes §6's interim pin** in the same seat that banks the re-read, so no hosted macos-15 sweep ever reads
  darwin/arm64 against rows banked on darwin/amd64. The re-read itself runs with an explicit
  `-Platforms darwin/arm64` before the pin comes off.
- The arch-count rows can read Go's arm64 count natively: go/internal/gcimporter (634 against 636) and runtime.
  Rows that plausibly differ: syscall (zerrors and zsyscall differ between the arches), runtime, internal/cpu and
  crypto/* after D4, math and hash/crc32 (arch-selected files).
- macos-15 has 3 cores; DW-7 was a cancelled job there. D7's shard plan budgets the wall time per shard.

## 8. The hop-runbook steps it reuses

The five-root reconvert is the four-root one with one more seeded root (`docs/GoCorpusMigration.md`, the per-target
emission). Every later converter seat's footprint grows from four roots to five.

- **Measured (M1c, 2026-11-24, a 4-core cloud box, Go 1.26.11, whole corpus, one root at a time):** windows/amd64
  221 s, linux/amd64 222 s, darwin/amd64 209 s, linux/arm64 214 s, darwin/arm64 219 s. Four roots 866 s, five roots
  1085 s: +219 s, +25%. Each wall is one `-stdlib` conversion; seeding, manifesting and deleting are extra.
- **The stack-append harvest** (`stackAppendHints.go`:67, five targets per package once the constant has five) grows
  by the same one root in five.
- **The hosted reconvert-footprint stays inside its job timeout.** It converts one target per leg, never five
  (§6), so the fifth target does not reach it. Its budget is 60 minutes against measured whole jobs of 8-9 (linux),
  15 (windows) and 18 (darwin, on osx-arm64) minutes (os-matrix.yml:691-693). A darwin/arm64 arm, if D5 adds one,
  is one more single-target conversion of the same size.

## 9. Seats, owners, and where each can start

| Seat | Scope | ESTIMATE (lane-days) | Owner | Can start |
|---|---|---|---|---|
| **D1** | The arch through `-tests` for darwin. S1b already routes it through `platformUniverse` (`testsCorpusArch.go`:16-21), so D1 is `_paths.ps1`:127, the sweep's darwin arguments, and the behavioral golden policy (convert and oracle at arm64, as LA's arm leg). It lands inside D2, since the mirror guard holds `_paths.ps1` equal to the constant. | 0.5 | P5 | now (drafted on D2's branch) |
| **D2** | The flip: `platformUniverse` gains darwin/arm64; the five-root reconvert and the corpus apply in one seat (LA §2.4: a target enters the layout only through the merge); the readers of §2.5; the metadata record; H7 parity at the four existing flavours (I1-I4 measured). Revision 2 adds three things that cannot lag the flip: the target scope and the `GOARCH` stand-ins' move to `darwin-amd64/` (§4.2, R3); the cpu stand-in's rename and E's `osInit` registration (§5, R6); the sweep's `-Platforms` pin (§6, R5). G reviews. | 2-3 | P5, G reviews | after §10's rulings |
| **D3** | The darwin hand-owns at arm64 (§4): the audit, the displacement check on zsyscall's arm64 twin, the behavioral `GOARCH` row in each flavour (§4.2), the runtime stub census at E (12 arm64-only names), probes on osx-arm64. No retirement (R3). The audit and probes need nothing; the rows and the stub census need D2's tree. | 2-3 | P5 | audit now (started); rows after D2 |
| **D4** | What stays after D2 (§5): the `cpu.ARM64` behavioral row in each flavour, the consumers at E (runtime hash, crypto/tls), x/sys/cpu. The sysctl calls are already bound; the build-critical half is in D2 (R6). | 0.5 | C1 | Thursday 06:00Z: the row can be written then and run on D2's tree |
| **D5** | CI (§6): the macos-15 arch binding (a darwin/arm64 reconvert-footprint arm only if COORD asks). | 1 | C1 | Thursday 06:00Z: the binding can be written and dry-run against today's tree (it must refuse while no `darwin-arm64/` folder exists, as the linux probe does); it seats with D2 (R2) |
| **D6** | Packaging (§3). | 1-1.5 | C2 | after D2 lands; the map change and the NugetgoFlavors test can be drafted from Thursday 06:00Z |
| **D7** | The bank (§7), and the removal of §6's interim pin in the same seat. | 2-4 | bank lanes | after D2, D3, D4 and D5 land |
| then | H's cgo osx-arm64 arm (zig aarch64-macos, R-b becomes a consume leg). | 0.5 | H | after D6 |

Critical path: D2, then D3's rows, then D7, about 5 to 8 lane-days (ESTIMATE). Revision 2 moves about half a day
from D4 into D2.

**Kit** (for D2 and any re-read of M1c): one seed of `src/core` plus `src/version.props`, five roots converted one
at a time with `-stdlib -comments -platforms <target>`, each manifested by sentinel mtime and deleted before the
next; the classifier applies LA §2.1 over `U'`. Paths are in the D2 seat's DATA record.

## 10. Rulings asked

- **R1.** GOOS-first is kept at five targets (§2.2). Recommended: yes, for LA's reason; M1c's copy count is the cost.
- **R2.** D5's macos-15 binding seats with D2 (§6). Recommended: yes; the alternative leaves the daily census blind to
  the new flavour for as long as D5 lags.
- **R3 (revision 2).** The darwin `GOARCH` displacement and its two stand-ins are RE-SCOPED to target darwin/amd64
  at D2, not removed: `manualConversionConsts` gains a target scope, the two files move to `darwin-amd64/`, and
  darwin/arm64 compiles Go's converted constant. D3 owes the behavioral row in each flavour (§4.2). Recommended:
  yes; this is P3's minimal fix. <!-- Revision 1 asked: "The darwin GOARCH displacement and its two stand-ins are
  removed at the flip rather than kept (§4.2). Recommended: yes; each flavour's converted constant is then Go's own."
  P3 (verdict on a565c0290a, 2026-11-24 23:27Z, MUST-FIX 1) refused it: a pack built between D2 and D6, and any
  local-refs build on Apple silicon with GoTargetArch empty, run the darwin/amd64 flavour in an arm64 process and
  would read runtime.GOARCH "amd64" again. -->
- **R4.** The flavour lands in `.5` behind the existing darwin/amd64 flavour, so osx-x64 consumers see no change and
  osx-arm64 consumers move only when D6 ships. Recommended: yes. (P3: yes, with MUST-FIX 2 answered; R5 is the
  answer.)
- **R5 (new).** From D2 until D7, the macos-15 sweep legs pin `-Platforms darwin/amd64` (a pass-through added to
  `run-validated-sweep.ps1`, one line at each of os-matrix.yml's two darwin sweep calls), and D7 removes the pin when
  it banks the re-read (§6, §7). Recommended: yes, over a red-tolerant window.
- **R6 (new).** The build-critical half of D4 seats with D2 and is P5's: the cpu stand-in is renamed with no
  principal and homed in `darwin-amd64/`, and E's `osInit` is displaced for target darwin/arm64 to an impl that
  keeps the `HasDIT` departure (§5). C1 keeps the rest of D4. Recommended: yes; without it E does not build at D2
  (CS0759) and darwin/amd64 loses Apple-silicon detection silently.

## 11. Stop lines

- A D2 tree whose four existing flavours do not satisfy I1-I4 is not posted; the moves are scored against §2.3 and
  M1c file by file.
- A reader of §2.5 that passes on the applied tree without having been made to fail is not counted (floor rule 13).
- Disk (revision 2): the lane stop line, 10 GB free, applies to a five-root run on a lane box; the 25 GB floor of
  safety floor 12 is the validated sweep's battery, which this kit is not. The kit holds one root on disk at a time.
  MEASURED: the seed (`src/core` without `bin`, `obj` and `Generated`, plus `version.props`) is 226 MB at
  `cb6def82a2`, and a conversion rewrites it in place, so a root is about that size (M1c did not record a converted
  root's size; ESTIMATE under 300 MB). The converter build and the Go build cache are the rest. The kit checks free
  space before each root and stops under 10 GB.
- If M1c shows E differing from D in more than the predicted families (arch-folded constants, the `_arm64` file set),
  D2 stops and the difference is classified before any apply.
- D6 (P3's revision 2 nit 5): D6 does not pack a package whose osx-arm64 asset breaks against its last published
  one until P2, the GoBreakFloor's owner, has answered on the suppression entries and the floor rule for it (section
  3). Packing first and asking after is a published break with no ruling behind it.
