# PREREAD — H6 runtime share, go1.24.13 → go1.25.14 → go1.26.9 (DRAFT)

> **Record type:** a point-in-time pre-read for the Go 1.26 hop's H6, the C1 + G runtime share
> ([`PLAN-hop-go126.md`](../PLAN-hop-go126.md), the H6 row). **State: DRAFT.** It is a map, not the H6
> review: every classification below is a *first* reading from declarations and diffs, and every row is
> re-read at H6 against the H5 `.auto` emission. Written by C1 on 2026-10-14, at COORD's dispatch.
> **Measured at the campaign tip `31a53399cc`** (`claude/hop-go126`). The upstream sources are the release
> toolchain modules `golang.org/toolchain@v0.0.1-go1.24.13`, `-go1.25.14` and `-go1.26.9` (`linux-amd64`).
> Code reading and docs only: nothing here was built, converted or run.

## 1. What was measured, and how

- **The population** is the instrument's own: `src/handown-census.ps1`'s line-anchored marker scan at this
  tip, **184 marked files**. The census was run three times: **leg 1** 1.24.13 → 1.25.14, **leg 2** 1.25.14 →
  1.26.9, and **end to end** 1.24.13 → 1.26.9. That is runbook §4 step 6's two-leg attribution, with the
  end-to-end run as the cross-check.

  | census | no-upstream | substantive | trivial | untouched | total |
  |---|--:|--:|--:|--:|--:|
  | leg 1, 1.24.13 → 1.25.14 | 64 | 57 | 4 | 59 | 184 |
  | leg 2, 1.25.14 → 1.26.9 | 66 | 58 | 5 | 55 | 184 |
  | end to end, 1.24.13 → 1.26.9 | 64 | **78** | 6 | 36 | 184 |
  | end to end, 1.24.13 → 1.26.4 | 64 | 78 | 6 | 36 | 184 |

- **The share** is the plan's C1 + G list (`runtime`, `runtime/pprof`, `internal/runtime/{syscall,maps}`,
  `internal/abi`, `internal/cpu`). Of its 69 marked files, **31 are substantive end to end**: `runtime` 26,
  `runtime/pprof` 1, `internal/runtime/maps` 1, `internal/runtime/syscall` 1, `internal/abi` 1,
  `internal/cpu` 1. They map to 30 upstream files, because `runtime2.cs` and `runtime2_impl.cs` both
  replace `runtime/runtime2.go`.
- **Per file and per leg**, beyond the census's class:
  - **lines** are `git diff --no-index --numstat` of the upstream file;
  - **declarations** are the top-level Go declarations whose comment-stripped, whitespace-collapsed
    source differs, read with a small `go/ast` tool (`declchg`, source in the comment below);
  - **hand-owned member touched** means a changed declaration whose name the hand-own `.cs` *declares*:
    a C# method, type, field, constant or property of that name on a line that opens with a modifier.
    The match is by name and errs in both directions. A same-named local helper over-counts. A member
    the converter renamed, or a whole-file replacement whose members are all hand-owned, under-counts.
    It is a pointer for the reviewer, not a verdict.
- **The plan's 32 against 31 here (and 79 against 78)** is the tree, not the release. The 1.26.4 and
  1.26.9 censuses read the same 78 at this tip. Between the plan's census commit `16e02568d5`
  (2026-10-11) and this tip, the marked set changed in two ways:
  - three per-GOOS `runtime/{darwin,linux,windows}/trace_impl.cs` were folded into one
    `runtime/trace_impl.cs`. All three mapped to `runtime/trace.go`, so three substantive rows became one;
  - `runtime/darwin/os_darwin_impl.cs` became a hand-own.

  The net is −1, in `runtime`.

  **2026-10-15, end to end now reads 79.** The census relocation fix (S1, `src/handown-upstream.ps1`, on
  the tip at `d971812478`) also closed the go124 audit's OQ-6 hole. `internal/syscall/windows/exec_windows_test.cs`
  sits in a package whose own last segment is `windows`, so the old fold mapped it to
  `internal/syscall/exec_windows_test.go`, a file in no release, and it read no-upstream-counterpart. It
  now maps to its real upstream: substantive on leg 1 (+3/−2), untouched on leg 2.
  - The counts at `d971812478`, end to end, are substantive 79 and no-upstream 63.
  - The row is in P2's share (`internal/syscall/windows`). The C1 + G share stays 31.
  - The same fix reads `internal/runtime/syscall/linux/syscall_linux_impl.cs` as RELOCATED (§2.1).

<!-- declchg, the declaration differ (Go, go/ast; built with the 1.24.13 toolchain, GOTOOLCHAIN=local):
  load(path): parse without comments; for each top-level decl record a key and a rendering:
    FuncDecl  -> "func " + Recv + "." + Name   (receiver type unwrapped through *, [T], [T,U])
    TypeSpec  -> "type " + Name
    ValueSpec -> "var|const " + Name   (one key per name; the whole spec is the rendering)
  rendering = go/printer output with every whitespace run collapsed to one space.
  main(a, b): for every key in either file print "<key> added|removed|changed <lines-a> <lines-b>",
  where changed means the renderings differ. A moved-but-identical declaration is not reported.
  The C# matcher (python): a changed declaration counts as a hand-owned member when the .cs has
  `struct|class|interface|enum|record <name>`, or a line that opens with a C# modifier and, before its
  first `=>`, `{` or `//`, carries `<name>(` (a method) or `<name> =`, `<name>;` (a field, constant or
  property). The first matcher rejected tuple return types, `(uintptr r1, ...) Syscall6(`, and was
  loosened after a spot check found Syscall6 declared in syscall_linux_impl.cs but not matched. -->

## 2. Structural findings (before any per-file reading)

1. **`internal/runtime/syscall` is split into subpackages in 1.26.** At 1.24.13 and 1.25.14 it is one
   package of `syscall_linux.go` and `defs_linux*.go`. At 1.26.9 the directory holds no Go file, and
   the code lives in two new packages:
   - `internal/runtime/syscall/linux`, which declares `package linux`;
   - `internal/runtime/syscall/windows`, which receives the `defs_windows*.go` that leave `runtime`.

   Two consequences:
   - **The census misreads this row.** `src/handown-census.ps1`'s layout rule drops a trailing GOOS
     folder, so it maps `internal/runtime/syscall/linux/syscall_linux_impl.cs` to
     `internal/runtime/syscall/syscall_linux.go`. That file is absent at 1.26.9, so the row reads
     "vanished". The class (substantive) is the safe one, but the leg-2 figure (−89) is a deletion that
     did not happen. The real counterpart is `internal/runtime/syscall/linux/syscall_linux.go`: **+17/−4
     on leg 2** (`Uname` and `Utsname` added), and +62/−4 end to end.
   - **The folder collides with the corpus layout.** `src/core/internal/runtime/syscall/linux/` is today
     the *linux flavour folder* of package `internal/runtime/syscall` (layout L3). At 1.26 a real package
     has that path. That is an H5 conversion-layout question as much as an H6 one, and it is raised here
     because the hand-own sits in that folder. I found no record of it under `docs/phase4` or in the plan.
2. **Upstream files move out of `runtime`**, so hand-owns that call into them change consumer, not text:
   - `runtime/sizeclasses.go` moves to `internal/runtime/gc/sizeclasses.go` (+18/−15);
   - `runtime/defs_windows*.go` moves to `internal/runtime/syscall/windows`;
   - `runtime/map_swiss.go` and the `map_fast*_swiss.go` files become `runtime/map*.go`, with the
     swiss-or-not split gone;
   - `runtime/typekind.go`, `traceexp.go` and the `lock_*_tristate.go` files are removed.

   `internal/runtime/maps/runtime.go`'s +341 on leg 2 is `runtime_mapaccess1`, `runtime_mapaccess2` and
   `runtime_mapassign` arriving from `runtime`.
3. **The experiment defaults change at 1.26** (`internal/buildcfg/exp.go`, `baseline`):

   | experiment | 1.25.14 | 1.26.9 |
   |---|---|---|
   | `GreenTeaGC` | off | **on** |
   | `RandomizedHeapBase64` | off | **on** |
   | `SwissMap` | on | no longer an experiment |

   `SizeSpecializedMalloc`, `RuntimeFreegc`, `GoroutineLeakProfile` and `RuntimeSecret` are new and
   **off**. If the converter takes the toolchain's default tag set, as `go list` does, then at 1.26.9:
   - the converted corpus selects `mgcmark_greenteagc.go`, not `mgcmark_nogreenteagc.go`;
   - `randomizeHeapBase` is live in `mallocinit`;
   - the off experiments' code is present but behind false constants.

   **To confirm at H5.**
4. **New upstream subsystems a hand-own touches.** None is a new hand-own; each enters through a file
   in the table:
   - **1.25:**
     - the **cleanup queue** with its own goroutines (`mcleanup.go` +570);
     - the **finalizer queue** rewrite (`mfinal.go`: `finBlock`, `runFinalizers`);
     - **Green Tea**'s mark-bit plumbing (`mbitmap.go`, `mgc.go`, then off by default);
     - **cgroup-aware GOMAXPROCS** (`getproccount` → `getCPUCount` on all three OS files, `cgroup_linux.go`).
   - **1.26:**
     - the **goroutine leak profile and GC** (`mgc.go` `goroutineLeakGC`, `mprof.go`, `runtime2.go`
       `_Gleaked`);
     - **bounds panics by encoded PCDATA** (`panic.go`: `panicBounds`, `panicBounds32` and `panicBounds64`
       replace the 17 `panicIndex` and `panicSlice*` entry points, and the `goPanic*` family is re-routed);
     - the **`_Psyscall` P state is retired** (`runtime2.go` `_Psyscall_unused`);
     - **`maybeTraceablePtr`** and **`mWeakPointer`** (`runtime2.go`);
     - **windows `stdcall0..8` collapsed to one variadic `stdcall`**, with `asmstdcall` removed
       (`os_windows.go`);
     - **`freegc`** and **size-specialized malloc** (`malloc.go`, both off);
     - **`internal/runtime/gc/scan`**, the vectorised scan kernels that Green Tea uses.

## 3. Per file

Columns:
- **L1** and **L2** are the per-leg upstream line deltas (+added/−removed).
- **e2e** is 1.24.13 → 1.26.9.
- **decl** is the number of changed top-level declarations, given as L1/L2.
- **owned** names the changed declarations the hand-own declares, per leg.
- **first class**:
  - *mechanical*: a rename, move, signature shape, new constants, or a call that follows a relocation;
  - *semantic*: behaviour the hand-own models changes;
  - *new subsystem*: an upstream mechanism the managed runtime has no model of yet.
- **born** is the release or releases that moved the file.

### C1 (17 files)

| hand-own (kind) | upstream | L1 | L2 | e2e | decl | owned | first class | born |
|---|---|--:|--:|--:|--:|---|---|---|
| `runtime/mcleanup.cs` (whole file) | `runtime/mcleanup.go` | +570/−4 | +106/−27 | +660/−15 | 31/9 | L1, L2: `AddCleanup`, `Cleanup.Stop` | **new subsystem** (1.25 cleanup queue and its goroutines, beside the A11 managed cleanup) + semantic (1.26 `cleanupFn`, `callCleanup`). The hand-own also carries two bodies P2 pushed at the hop, `unique_runtime_blockUntilEmptyCleanupQueue` and `sync_test_runtime_blockUntilEmptyCleanupQueue`. The H6 rewrite keeps both (§6.1) | both |
| `runtime/mgc_impl.cs` | `runtime/mgc.go` | +75/−60 | +396/−63 | +464/−116 | 15/20 | — | **new subsystem** (1.26 goroutine leak GC, `gcBeginWork`/`gcEndWork`); semantic (1.25 `workType`, unique-map cleanup moved out) | both, 1.26-heavy |
| `runtime/runtime2.cs` (whole file) | `runtime/runtime2.go` | +85/−68 | +251/−60 | +336/−128 | 15/27 | L1: `_panic`, `g`, `gobuf`, `m`, `p`, `schedt`, `isIdleInSynctest`, `ncpu`, `waitReasonStrings`; L2: `_Psyscall`, `g`, `m`, `p`, `schedt`, `sudog`, … | **semantic** (struct layouts on both legs; 1.26 `_Psyscall` retired, `_Gleaked`/`_Gdeadextra`, `maybeTraceablePtr`, `mWeakPointer`) | both |
| `runtime/runtime2_impl.cs` | `runtime/runtime2.go` | (same upstream) | | | 15/27 | L2: `maybeTraceableChan.set`, `maybeTraceablePtr.set`, `.uintptr` | semantic, read with `runtime2.cs` as one review | both |
| `runtime/panic_impl.cs` | `runtime/panic.go` | +56/−42 | +230/−72 | +285/−113 | 11/47 | L2: `fatal` | **semantic** (1.26 bounds panics by `abi.BoundsDecode`; 1.25 deferproc family, `cgroup_throw`) | both, 1.26-heavy |
| `runtime/trace_impl.cs` | `runtime/trace.go` | +56/−23 | +213/−101 | +251/−106 | 7/5 | L1, L2: `ReadTrace`, `StartTrace` | **semantic** (the trace reader and advance, both legs) | both |
| `runtime/mprof_impl.cs` | `runtime/mprof.go` | +61/−133 | +78/−7 | +139/−140 | 13/8 | L1: `stkbucket` | semantic (1.25 lock-profile rewrite) + **new subsystem** (1.26 goroutine leak profile) | both |
| `runtime/mfinal.cs` (whole file) | `runtime/mfinal.go` | +48/−41 | +5/−5 | +53/−46 | 15/2 | L1: `SetFinalizer`, `createfing`, `queuefinalizer`, `runfinq`, `finblock`, `allfin`, `finc`, `finptrmask`, `finq`; L2: `SetFinalizer` | **semantic** (1.25 finalizer queue rewrite: `finBlock`, `runFinalizers`) | 1.25 |
| `runtime/linux/os_linux_impl.cs` | `runtime/os_linux.go` | +6/−4 | +101/−17 | +105/−19 | 5/14 | L2: `getHugePageSize` | mechanical (1.25 `getCPUCount`; 1.26 `kernelVersion` parse, `fixSigactionForCgo`, futex moved) | both, 1.26-heavy |
| `runtime/debuglog_impl.cs` | `runtime/debuglog.go` | +5/−14 | +52/−4 | +57/−18 | 4/8 | L1: `dlogImpl`, `printDebugLogImpl`; L2: `debugLogReader.printVal`, `dloggerImpl.s` | mechanical (1.26 hexdump records) | both |
| `runtime/linux/netpoll_epoll_impl.cs` | `runtime/netpoll_epoll.go` | +1/−1 | +37/−24 | +38/−25 | 1/6 | — | mechanical (follows the syscall split; `netpollPackData`) | 1.26 |
| `runtime/profbuf_impl.cs` | `runtime/profbuf.go` | 0 | +28/−2 | +28/−2 | 0/2 | L2: `profBuf.write` | semantic (write and wakeup path) | 1.26 |
| `runtime/darwin/os_darwin_impl.cs` | `runtime/os_darwin.go` | +3/−2 | +19/−2 | +22/−4 | 3/6 | — | mechanical (`getncpu` → `getCPUCount`; `sysctlbyname*` reshaped) | both |
| `runtime/linux/mem_linux_impl.cs` | `runtime/mem_linux.go` | +12/−4 | +4/0 | +16/−4 | 5/1 | — | mechanical (1.25 `sys*OS` signatures; 1.26 `needZeroAfterSysUnusedOS`) | both |
| `runtime/stubs_impl.cs` | `runtime/stubs.go` | 0/−7 | +10/−1 | +10/−8 | 1/2 | L2: `procyield` | mechanical (`procyield` → `procyieldAsm`) | both |
| `runtime/darwin/sigaction_impl.cs` | `runtime/sigaction.go` | +1/−1 | +1/−1 | +1/−1 | 0/0 | — | mechanical (a build-constraint line; no declaration moves) | both |
| `runtime/pprof/windows/proto_windows_impl.cs` | `runtime/pprof/proto_windows.go` | 0 | +1/−2 | +1/−2 | 0/1 | — | mechanical (`createModuleSnapshot`) | 1.26 |

### G (14 files)

| hand-own (kind) | upstream | L1 | L2 | e2e | decl | owned | first class | born |
|---|---|--:|--:|--:|--:|---|---|---|
| `runtime/malloc_impl.cs` | `runtime/malloc.go` | +173/−225 | +490/−28 | +654/−244 | 22/21 | — (`mallocgc` is named, not declared) | mechanical (1.25 page constants and size classes to `internal/runtime/gc`) + **new subsystem** (1.26 heap-base randomisation **on**; `freegc` and size-specialized malloc **off**) | both, 1.26-heavy |
| `runtime/windows/os_windows_impl.cs` | `runtime/os_windows.go` | +5/−39 | +159/−222 | +162/−259 | 4/106 | — (`stdcall`, `asmstdcall` are named) | mechanical but wide (1.26 `stdcall0..8` → variadic `stdcall`, `asmstdcall` removed, defs to `internal/runtime/syscall/windows`) | 1.26 |
| `internal/runtime/maps/runtime_impl.cs` | `internal/runtime/maps/runtime.go` | 0 | +341/0 | +341/0 | 0/5 | — | mechanical, a relocation (map access and assign from `runtime/map_swiss.go`) | 1.26 |
| `runtime/windows/signal_windows_impl.cs` | `runtime/signal_windows.go` | +4/−12 | +87/−96 | +89/−106 | 1/17 | L2: `enableWER` | mechanical (SEM_/WER constants move out; handlers re-signed) | 1.26 |
| `runtime/mbitmap_impl.cs` | `runtime/mbitmap.go` | +61/−63 | +45/−3 | +106/−66 | 15/4 | L2: `pointerMask` | **semantic** (1.25 mark-bit plumbing for Green Tea, default-on at 1.26) | both |
| `internal/cpu/cpu_x86_impl.cs` | `internal/cpu/cpu_x86.go` | 0 | +82/−19 | +82/−19 | 0/16 | — | mechanical (AVX-512 and GFNI feature bits; `doinit`) | 1.26 |
| `internal/runtime/syscall/linux/syscall_linux_impl.cs` | `internal/runtime/syscall/syscall_linux.go` → `…/syscall/linux/syscall_linux.go` at 1.26 | +45/0 | +17/−4 (census reads 0/−89, §2.1) | +62/−4 | 4/2 | L2: `Syscall6` (moves, not removed) | mechanical (1.25 `Open`/`Close`/`Read`/`Pread`; 1.26 `Uname`) + **package relocation** | both |
| `internal/abi/type_impl.cs` | `internal/abi/type.go` | +2/−6 | +20/−22 | +22/−28 | 1/9 | L2: `Type.Elem`, `Type.Key`, `Type.MapType` | **semantic** (1.26: `KindMask` and `IfaceIndir` out, `TFlagDirectIface` in: the kind byte no longer carries direct-interface) | 1.26 |
| `runtime/sema_impl.cs` | `runtime/sema.go` | +18/−9 | +21/−6 | +39/−15 | 4/3 | L1: `semrelease1` | semantic, small (queueing) | both |
| `runtime/pinner_impl.cs` | `runtime/pinner.go` | 0 | +24/−4 | +24/−4 | 0/1 | — | semantic, small (`pinnerGetPtr`) | 1.26 |
| `runtime/arena_impl.cs` | `runtime/arena.go` | +5/−2 | +13/−7 | +18/−9 | 2/5 | L2: `arena_heapify` | mechanical | both |
| `runtime/windows/netpoll_windows_impl.cs` | `runtime/netpoll_windows.go` | 0 | +17/−25 | +17/−25 | 0/10 | L2: `netpollBreak` | mechanical (IOCP types and constants move with the windows split) | 1.26 |
| `runtime/windows/mem_windows_impl.cs` | `runtime/mem_windows.go` | +3/−3 | +12/−8 | +15/−11 | 3/6 | L1: `sysAllocOS`, `sysReserveOS`; L2: `sysAllocOS`, `sysFreeOS`, `sysReserveOS`, `sysUnusedOS`, `sysUsedOS` | mechanical (`sys*OS` signatures) | both |
| `runtime/netpoll_impl.cs` | `runtime/netpoll.go` | +7/−3 | 0 | +7/−3 | 2/0 | — | mechanical (`pollCache.alloc`/`free`) | 1.25 |

**Not substantive end to end (38 of the 69):**
- 37 rows: no upstream counterpart (the `*_impl` hand-additions), or untouched;
- 1 row: `runtime/tracemap_impl.cs`, trivial (+7/0 on leg 2, comments only).

The completeness gate still lists them at H6.

## 4. The split, by runtime history

**History.** Each hand-own's non-merge commits were attributed to the first lane merge that carries them
into this tip (`git log --merges --ancestry-path <c>..31a53399cc`).

- **Mostly C1:** `mfinal`, `mgc_impl`, `debuglog`, `os_linux`, `mem_linux`, `os_darwin`, `panic`,
  `stubs`, `runtime2.cs`. C1 also wrote the cleanup rewire behind `mcleanup` (A11, `7db31a5470`).
- **Mostly G:** `internal/runtime/maps`, `internal/abi`, `internal/cpu`, `internal/runtime/syscall`.
- **The rest:** came from lanes that are now idle (C2, P1), or from P2, R or i9. Those are split by
  subsystem.

**The proposal:**
- **C1 takes the GC, finalizer, cleanup, profile and trace core, and the linux and darwin OS layers.**
  17 files, 3,213 upstream lines end to end, counting the `runtime2` pair once. 6 files are large
  (≥150 lines): `mcleanup`, `mgc`, `runtime2`, `panic`, `trace` and `mprof`.
- **G takes the allocator, the type, map and CPU internals, `internal/runtime/syscall`, and the windows OS
  layer**, which G's windows host can run. 14 files, 2,431 lines. 5 are large: `malloc`, `os_windows`,
  `maps/runtime`, `signal_windows` and `mbitmap`.

**At the plan's rates**, about 1 to 1.5 lane-days per large file and 0.3 per small one:
- C1 is about 9 to 12 lane-days, against G's 8 to 10;
- the share's 5,644 lines sit beside the plan's figure of 5,060 for `runtime` alone.

**One coupling crosses the split.** `mbitmap` (G) and `mgc` (C1) both carry Green Tea's mark-bit
changes, and they should be reviewed against each other once H5 says which `mgcmark_*` file the corpus
selects.

## 5. What this pre-read does not do

- No judgment on any semantic row. A *first class* is a pointer for the reviewer. *Mechanical* can still
  hide a behaviour change in a call it re-shapes.
- No reading of the converted counterparts. A companion `*_impl.cs` with no declared member touched
  (`malloc`, `mgc`, `os_windows`) still owes the members its regenerated sibling calls. That surfaces at
  H5/H7 compile, not here.
- No attribution of RED25 or RED26 sites. That is §4 steps 3 to 5, on the i9.
- The H6 audit file (`AUDIT-h6-handown-go126.md`, per `check-h6-completeness.ps1`'s default) is not
  created. This record is input to it.

## 6. DRAFT H6 notes, C1's six large files (2026-10-14, read at `bc9f09bf50`)

> Added by C1 at COORD's S2 ruling. **Still DRAFT.** These are code-reading notes for each file: what the
> hand-own models today, what upstream changes in Go terms, and the member-level edits expected at H6.
> They are not the review; H6 re-reads each against the H5 emission. §6.1's design choice was put to
> COORD as a proposal before H5.

### 6.1 `runtime/mcleanup.cs` (whole file)

**What the hand-own models.** A11's managed cleanup:
- `AddCleanup` registers a sentinel whose .NET finalizer hands the body to `GoFinalizerQueue`'s single
  runner (`mfinal.cs`).
- A gate it shares with `SetFinalizer` holds a cleanup until its object's finalizer has run.
- `Stop` cancels by id.
- Its header names four divergences. The first is that cleanups run **sequentially** on the fing
  analogue, where Go runs them concurrently.
- Since the hop it also carries P2's two pushed bodies:
  - `unique_runtime_blockUntilEmptyCleanupQueue`;
  - `sync_test_runtime_blockUntilEmptyCleanupQueue`.

  Both wait on the finalizer queue.

**Upstream.**
- **1.25** adds the cleanup queue: `gcCleanups` of type `cleanupQueue`, with `cleanupBlock` lists on
  `lfnode` stacks and up to `max(GOMAXPROCS/4, 1)` dedicated goroutines (`runCleanups`), fed at sweep
  from `mspan` specials. It also adds `debug.checkfinalizers` context records.
- **1.26** stores a `cleanupFn` (`call`, `fn`, `arg`; the arg is boxed off the tiny allocator) in place
  of a bare funcval, and adds two `AddCleanup` panics, each with a test:
  - `ptr is within arg` (`TestCleanupUnreachablePointer`);
  - `cleanup function closes over ptr` (`TestCleanupUnreachableClosure`).

  `TestCleanupLost` queues 10,000 × GOMAXPROCS cleanups and needs them all within a 10 s drain.

**The fact that decides the shape: the queue is not private to `mcleanup.go`.** At 1.26.9, eight
converted runtime files reach into it:

| file | what it uses |
|---|---|
| `metrics.go` | `readQueueStats` |
| `mgc.go` | `lockInit(&gcCleanups.lock)` and `readQueueStats` |
| `mgcmark.go` | walks `gcCleanups.all` as `cleanupBlock` (`alllink`, `n`, `cleanups`); `cleanupBlockPtrMask` and `cleanupFnPtrMask` |
| `mgcsweep.go` | `flush` |
| `mheap.go` | `specialCleanup.cleanup` of type `cleanupFn`; `enqueue` |
| `mprof.go` | `running` |
| `proc.go` | `needsWake`, `wake`, and `queued += pp.cleanupsQueued` |
| `export_test.go` | `blockUntilEmpty` |

So the converted runtime does not compile at 1.25.14 or 1.26.9 until the hand-own declares these.
`runtime2.cs` owes `p.cleanupsQueued` (§6.3).

**Expected edits.** These assume option A of the proposal, the surface only; the alternatives are B
(port the queue) and C (a managed pool).
- **Declare Go's types** from `mcleanup.cs.auto`: `cleanupFn`, `cleanupBlockHeader`, `cleanupBlock`,
  `cleanupQueue`, `gcCleanups`, and both pointer masks.
- **Write the methods as managed shims:**

  | member | shim |
  |---|---|
  | `enqueue`, `flush` | unreachable: no `mspan` specials exist here |
  | `needsWake` | false |
  | `wake` | no-op |
  | `all` | stays nil, so `mgcmark`'s root loop runs zero times |
  | `readQueueStats` | real counts, from two counters on the cleanup path |
  | `blockUntilEmpty` | the same finalizer-queue wait as P2's two bodies, which are kept |

- **Panics.** The referent guard splits its message: `equal to arg` when the two pointer values are
  equal, `within arg` when they are one allocation. A delegate-`Target` scan (the closure object and
  any nested display classes) refuses a cleanup that closes over the referent.
- **Divergence 3 narrows to that split.** Divergence 1 stays, unless C is ruled.

### 6.2 `runtime/mgc_impl.cs` (companion)

**What the hand-own models.** One body, `gcTestIsReachable` over the CLR heap. The machinery under it
stays converted and dead.

**Upstream.** `gcTestIsReachable` is unchanged on both legs. mgc.go's delta is all in converted code:
- **1.25:** `workType`; the unique-map cleanup moved out (`unique_runtime_registerUniqueMapCleanup`
  removed); `gcBgMarkWorkerNodePadded`.
- **1.26:**
  - the goroutine-leak GC (`goroutineLeakGC`, `findGoroutineLeaks`, `findMaybeRunnableGoroutines`,
    `g.isMaybeRunnable`, linknamed from `runtime/pprof` and behind the off `GoroutineLeakProfile`
    experiment);
  - `gcBeginWork` and `gcEndWork`;
  - `setSyncObjectsUntraceable` and `gcRestoreSyncObjects`, which use the new `maybeTraceablePtr`
    (§6.3);
  - mark-worker scheduling (`gcShouldScheduleWorker`).

**Expected edits.** None to the companion.
- **Watch at H5:** whether the converted `mgc.cs` compiles against the managed runtime's model of `allgs`
  and `g` status, `_Gleaked` included.
- **The review pair:** `mbitmap` (G) and this file review Green Tea's mark bits together, once H5 names
  the `mgcmark_*` file (`go list runtime` at 1.26.9 selects `mgcmark_greenteagc.go`).

### 6.3 `runtime/runtime2.cs` (whole file) and `runtime2_impl.cs` (companion)

**What the hand-owns model.**
- `runtime2.cs` carries the scheduler structs (`g`, `m`, `p`, `schedt`, `sudog`, `_panic`, `gobuf`)
  as the managed runtime lays them out.
- `runtime2_impl.cs` carries the pointer-as-integer types (`guintptr`, `puintptr`, `muintptr`) and
  `setGNoWB` and `runqempty`.

**Upstream, field by field** (a `go/ast` struct differ):

| struct | 1.24.13 → 1.25.14 | 1.25.14 → 1.26.9 |
|---|---|---|
| `g` | + `syncSafePoint`, `runningCleanups`, `bubble`, `valgrindStackID`; − `syncGroup` | + `fipsOnlyBypass`, `ditWanted`, `secret`, `xRegs` |
| `m` | − two padding `_` | + `signalSecret`, `idleNode`, `ditEnabled`, `self`; − `libcall` (windows now uses `winsyscall winlibcall`) |
| `p` | + `cleanups`, `cleanupsQueued` | + `oldm`, `nextGCMarkWorker`, `goroutinesCreated`, `xRegs` |
| `schedt` | + `pollingNet`, `customGOMAXPROCS`; − `runqsize` | + `nGsyscallNoP`, `goroutinesCreated` |
| `_panic` | + `repanicked` | + `gopanicFP`; − `argp` |
| `gobuf` | − `ret` | — |
| `sudog` | — | `elem` becomes `maybeTraceablePtr`, `c` becomes `maybeTraceableChan` (types change, names do not) |

Also changed in that file:
- **1.25** adds three wait reasons, `mPadded` and `numCPUStartup` (`ncpu` goes).
- **1.26** adds the goroutine states `_Gleaked`, `_Gdeadextra` and their scan twins, and retires
  `_Psyscall` to `_Psyscall_unused`.

**Expected edits.** Port each field change into `runtime2.cs`. Every `− x` is a field that converted
code stops naming, and every `+ x` is one it starts naming. Then:
- `maybeTraceablePtr`, `maybeTraceableChan` and `mWeakPointer` arrive as new pointer-as-integer
  wrappers: an `uintptr` with a traceable bit. They belong beside `guintptr` in `runtime2_impl.cs`
  under the same managed-token rule, and are the likeliest new arm-2a site in this file.
- `sudog.elem`'s type change reaches every converted channel and select site through `.get()` and
  `.set()`. That is H5's compile, but the wrapper's semantics are decided here.
- The `_Psyscall` retirement changes the P-state model the scheduler converts against. Read it with
  G's `proc.go` reading.

### 6.4 `runtime/panic_impl.cs` (companion)

**What the hand-own models.** `throw` and `fatal`, the fatal path (DESIGN-fatal-path.md). It prints Go's
`fatal error:` line and exits through Go's own path, without `getcallerpc`.

**Upstream.**
- `throw` is unchanged.
- `fatal` (1.26) reads `getg()._panic` and calls `printPreFatalDeferPanic(p)` before the fatal line.
- The rest of `panic.go`'s delta is converted code:
  - 1.25's `deferproc` family and `cgroup_throw`;
  - 1.26's bounds panics by PCDATA. `panicBounds`, `panicBounds32` and `panicBounds64` decode
    `abi.BoundsDecode` from the caller's PC and replace the 17 `panicIndex` and `panicSlice*` entry
    points; the `goPanic*` family is re-routed through them.

**Expected edits.**
- **`fatal`:** take the `printPreFatalDeferPanic` call when `_panic` is non-nil. It prints the chain of
  panics in flight when the fatal came (a repanicked value once), which is visible output.
- **Bounds panics:** these need no hand-own, but a watch at H5. The converter emits its own bounds checks,
  so whether any converted call site still names `goPanicIndex` and the others decides if the PCDATA
  path is reachable at all. If it is, it cannot work here: there is no PCDATA. The `goPanic*` bodies
  must then keep their pre-1.26 direct form.

### 6.5 `runtime/trace_impl.cs` (companion)

**What the hand-own models.** `StartTrace`, `StopTrace` and `ReadTrace`, driving golib's
`ExecutionTracer` (Q28). It writes Go's v2 format as `go 1.23 trace` and is accepted by `go tool trace`.

**Upstream.**
- `StartTrace` (1.26) takes the clock frequency before stopping the world, and drops 1.24's long
  stop-the-world rationale.
- `ReadTrace` gains a named result.
- `internal/trace/version` adds `Go125` and `Go126`; `Current` is `Go126`. `Go123` is still parsed.
- 1.25 adds `runtime/trace`'s **flight recorder** and subscribe API, which linkname four runtime entry
  points: `traceAdvance`, `readTrace`, `traceClockNow` and `traceClockUnitsPerSecond`.

**Expected edits.**
- The three owned bodies change little.
- **The four new linkname targets are the item.** As converted, they reach the converted tracer's
  buffers, which do not run here. So `trace.FlightRecorder` and the subscriber would read nothing
  without an error. H6 routes them to `ExecutionTracer`, or refuses them by name, as the tracer seam
  did before Q28.
- **Whether to move the header to `go 1.26 trace`** is a second, smaller question. It depends only on
  whether a test or oracle reads the version.

### 6.6 `runtime/mprof_impl.cs` (companion)

**What the hand-own models.** The block, mutex and memory profile bucket store in managed storage
(DESIGN-managed-profiling.md I2):
- `newBucket`;
- the accessors `stk`, `mp` and `bp`;
- `stkbucket` and `saveblockevent`;
- the managed memory-profile recorder.

**Upstream.**
- 1.25 changes `stkbucket` only by the `sysAlloc` name argument (`"profiler hash buckets"`), which the
  managed store does not call.
- 1.25 rewrites the lock profile (`mLockProfile` start, end and `storeSlow`; `lockTimer` gone). That is
  converted code, which reaches `saveblockevent` and `stkbucket`.
- 1.26 adds the goroutine-leak profile (`goroutineLeakProfileWithLabels` and its concurrent and sync
  forms, linknamed from `runtime/pprof`), behind the off `GoroutineLeakProfile` experiment.
- 1.26 changes `mProf_Malloc`.

**Expected edits.**
- **`stkbucket`:** none, beyond reading the 1.25 call sites.
- **The lock-profile rewrite** is the one to read with this store, because its new `storeSlow` path
  calls `saveBlockEventStack`, which calls `stkbucket`.
- **`mProf_Malloc`'s 1.26 change** is checked against `memProfileAlloc`, the managed recorder's entry.
- **The leak profile** is reachable only with its experiment on. It is listed rather than ported.

### 6.7 What the six add to the H6 bill

**Unchanged from the plan's rates:**
- `mcleanup`: 1 lane-day under A, 2 under C.
- `runtime2`: 1.5. It is the widest port, and its wrapper types are a design point.
- `trace`: about 1, for the four linkname targets.
- `panic`, `mprof`, `mgc`: about 0.5 each.

That is about 5 lane-days for the six, inside §4's 9–12 for C1's 17.

**Not yet counted:** the H5-compile items §6.1 and §6.3 name. The cleanup-queue surface is a
compile-blocker for H5 itself, not only an H6 review item.
