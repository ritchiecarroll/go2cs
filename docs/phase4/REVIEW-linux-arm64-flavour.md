# REVIEW — DESIGN-linux-arm64-flavour (G, claude/g-la64-design 7831a23193), adversarial


> **Status: RECORD.** Adversarial review requested by COORD before any seat is cut (owner-provided Fable 5.2,
> 2026-11-09), read against master 0c553534df. The findings are ranked, most severe first. COORD's rulings follow
> the findings.

## F1 — BLOCKER (design): a fourth GOOS-style key cannot subtract flat amd64 files

- 16 amd64-only converted files are FLAT today, byte-identical on windows/linux/darwin at amd64. Examples:
  `internal/abi/abi_amd64.cs`, `internal/goarch/{goarch,zgoarch}_amd64.cs`, `internal/runtime/atomic/atomic_amd64.cs`,
  `internal/runtime/gc/scan/{expand,filter,scan}_amd64.cs`, `math/exp_amd64.cs`, `hash/crc32/crc32_amd64.cs`,
  `runtime/{cpuflags,preempt,stubs,test}_amd64.cs` and `internal/bytealg/index_amd64.cs`.
- Layout L3 compiles flat + `$(GoTargetOS)/*.cs` (`platformProject.go:11-42`), and `platformLayoutDir` routes per FILE
  (`platformLayout.go:123-140`). A file present on 3 of 4 targets has no home: a `linux-arm64/` folder cannot remove a
  flat file. Under R1(a), those files, and the flat hand-owns they collide with, get TRIPLICATED into `windows/`,
  `linux/` and `darwin/`. That rewrites the layout of all three existing flavours and breaks the seeded-control
  invariant. `platformEmit.go:72-81` already says the GOARCH axis is "a later increment", and `reconcileLayoutDuplicates`
  is GOOS-keyed.
- M1 omits `internal/abi`, although `abi_arm64.go` exists and `abi_amd64.cs` is flat. Reconcile the 18 and 44 lists.
- The two amd64-only packages (`runtime/race/internal/amd64v1`, `internal/runtime/startlinetest`) would be built into
  the arm64 pass. State their disposition.
- **Change:** use orthogonal folders: `<pkg>/<goos>/` as now, plus `<pkg>/amd64/` for files shared by every amd64
  target, plus `<pkg>/linux-arm64/` (or `<pkg>/arm64/` + `<pkg>/linux-arm64/`). The csproj includes flat +
  `$(GoTargetOS)/` + `$(GoTargetArch)/` + `$(GoTargetOS)-$(GoTargetArch)/`, and the 16 flat amd64 files move ONCE.
  Re-measure M1 as a four-target census, with a classification rule for every presence subset.

## F2 — BLOCKER (S3 sizing): hard-coded amd64 syscall numbers, and a semantic gap

- Numbers baked as constants:
  - `internal/poll/linux/runtime_netpoll_impl.cs:96-98` (epoll_wait 232, epoll_ctl 233, epoll_create1 291);
  - `runtime/linux/signal_posix_impl.cs:701-704` (getpid 39, gettid 186, tgkill 234, exit_group 231), plus
    `SigactionBytes=160` / `SaFlagsOffset=136` at :162-163;
  - `runtime/linux/sigprocmask_impl.cs:55` (rt_sigprocmask 14);
  - `internal/syscall/unix/linux/net_linux_impl.cs:105-106` (recvfrom 45, sendto 44).
- Not only numbers: arm64 has NO epoll_wait (only epoll_pwait), and no open/pipe/dup2/fork/getdents/rename/unlink. A
  twin that only renumbers compiles and returns ENOSYS at run time: a silent wrong-answer class.
- The linux hand-own population is 21 `_impl.cs` + 2 GoManualConversion files under `**/linux/`, not 8.
  `mem_linux_impl.cs:123-128` and `mranges_impl.cs:191` hard-code 4096 (arm64 kernels may use 16K/64K pages).
- The good pattern exists: `golib/runtime/ThreadCpuTime.cs:145-150` switches gettid's number on ProcessArchitecture.
  Mandate it: every linux hand-own takes its number from the compiled flavour's `zsysnum_linux_*` constants, or
  switches on arch.
- **Change:** S3's population is every file under `**/linux/` plus every flat `_impl.cs` that compiles on linux,
  censused for (i) numeric literals passed to a syscall entry, (ii) SizeOf/offset constants, (iii) explicit-layout
  mirrors (Stat_t, EpollEvent). S3 sizes at 5-8 days, with a GolibTests arm per renumbered site run on the arm runner.

## F3 — BLOCKER (converter): -tests and single-package conversions are pinned to amd64 on an arm64 host

- `testsCorpusArch.go:19` (`const corpusArch = "amd64"`) and `testsTargetPlatform` (:33-48) rewrite a GOROOT -tests
  conversion on an arm64 host to `<goos>/amd64` (main.go:816-822). On ubuntu-24.04-arm, every roster row would convert
  at linux/amd64 and compile against the linux-arm64 flavour, so the S7 bank is unmeasurable until this changes.
- Behavioral goldens are amd64, and the darwin precedent (convert amd64, oracle arm64) is the opposite of what
  linux-arm64 needs.
- **Change, added to S1:** corpusArch becomes per-GOOS ({windows: amd64, linux: {amd64, arm64}, darwin: amd64}). The
  -tests csproj emission and its build args carry `-p:GoTargetArch=`. run-validated-sweep / _paths.ps1 gain
  GoTargetArch. The arm leg declares its behavioral Target-phase golden policy.

## F4 — SHOULD-FIX (R2, SEAT 0): placement and predicate

- (a) Inside GoSelectRidCompileAssets, which runs only when GoRidCompileAssets is true (go.lib.targets:49-51), a
  consumer with the swap off skips the error. Make it its own target with its own condition.
- (b) The else-branch also fires for platform-NEUTRAL packages. Decide "RID-split" by `Exists(<pkgroot>/runtimes/)`.
- (c) The twin path is the EXACT RID, while the host walks the RID graph (linux-musl-arm64 -> linux-arm64,
  linux-musl-x64 -> linux-x64, linux-bionic-arm64 -> linux-arm64). On Alpine the error would fire wrongly. Resolve
  through `$(RuntimeIdentifierGraphPath)`, or key on ResolvePackageAssets' own resolution, and add an Alpine arm.
- (d) win-arm64: a portable build compiles the Windows flavour, which is the right OS. Same-OS unshipped RID =
  warning; cross-OS or cross-ABI = error.
- Cheap run-time guard for the portable gap: `GoHostOfRecord.Verify(GOOS)` (runtime/hostofrecord_impl.cs:42-44) also
  passes GOARCH and compares it with `RuntimeInformation.ProcessArchitecture`. It is vacuous on windows/darwin
  (displaced at run time) and loud on an arm64 host that loads an amd64 linux flavour. Add it to S0.
- The opt-out is safe only as a global property, documented as "the program is the Windows flavour".

## F5 — SHOULD-FIX (R3): the roster grammar needs a "pending at this RID" state

- A plain `linux: N` today means banked on linux-x64. With the arm64 bank sharded over weeks, a plain segment must not
  read as banked on both RIDs. The Linux header derivation (check-roster-format.ps1:1341-1361) sums one population.
- Add a pending-at-RID state, or a derived evidence file, and name the linux twin of annotate_darwin.py. Say whether
  a pending arm64 reading is excluded from the bubble's "worse RID".

## F6 — SHOULD-FIX (S5/S6): pack and CI mechanics

- `$buildRidByGoos`/`$buildRidOf` (push-nuget.ps1:1728-1739) and the merge (:2049, :2067) key on GOOS, as does
  Get-NugetgoRidFlavors. Key them on (GOOS, GOARCH); $referenceRid stays win-x64.
- Test-GoApiCompat has no baseline for a first linux-arm64. Rule "new RID admitted with no prior" explicitly.
- The pack's arm64 pass is a cross-compile of AnyCPU IL on the x64 box. Native AOT probes and aot-smoke run ON the arm
  runner. GoLibcCall's darwin-arm64 variadic rule does NOT apply on Linux AAPCS64: say so at
  syscall_linux_impl.cs:104.
- TailCallFrameGuardTests is JIT behaviour; re-run it in Release on arm64 before trusting any caller line.

## F7 — NOTE: GOARCH coherence

- S4 (HWCAP) is load-bearing for hash fidelity (alg.cs prefers AES on cpu.ARM64.HasAES), not optional.
  `getauxval(AT_HWCAP)` via libc is simpler than parsing /proc/self/auxv.
- An unaligned 64-bit atomic faults on arm64: audit any Pack=1/explicit-layout mirror holding an Interlocked long in S3.

## F8 — NOTE: CI/bank structure

- ubuntu-24.04-arm is a sound oracle host. A hosted job caps at 6 h, so the sweep needs 4 or more shards, the runtime
  row may need its own job, and timing-sensitive rows are re-measured rather than inherited.

## Reviewer's verdict and sizing

- R1: reject as written; approve the direction with the layout rule rewritten (F1) and S1 widened (F3).
- R2: approve with the F4 changes.
- R3: approve with the F5 changes.
- Sizing after the changes: S1 3-5 days, S3 5-8, S0 +0.5. The total is nearer 16-24 lane-days.

## COORD rulings (2026-11-09)

- **R1: REJECTED as written.** G revises §2 with the orthogonal-folder rule (F1) and a four-target M1, and widens S1
  by F3. The revised note comes back for a ruling before S1 is cut.
- **R2: APPROVED WITH CHANGES.** SEAT 0 takes all of F4: its own target, RID-split by `runtimes/` presence, RID-graph
  aware with an Alpine arm, win-arm64 (same OS) = warning and cross-OS/ABI = error, plus the GoHostOfRecord GOARCH
  run-time check. C2 is already cutting it; the changes go to C2 now.
- **R3: APPROVED WITH CHANGES** (F5).
- F2, F6, F7 and F8 are folded into the revised seat breakdown.

