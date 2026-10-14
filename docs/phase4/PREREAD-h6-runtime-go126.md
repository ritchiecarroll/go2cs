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
| `runtime/mcleanup.cs` (whole file) | `runtime/mcleanup.go` | +570/−4 | +106/−27 | +660/−15 | 31/9 | L1, L2: `AddCleanup`, `Cleanup.Stop` | **new subsystem** (1.25 cleanup queue and its goroutines, beside the A11 managed cleanup) + semantic (1.26 `cleanupFn`, `callCleanup`) | both |
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
