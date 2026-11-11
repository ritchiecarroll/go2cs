# DATA — the linux/arm64 S3 census: syscall numbers, layouts and atomics in the hand-owns (i9, 2026-11-09)

> **A point-in-time record.** This is the census half of seat S3 of `DESIGN-linux-arm64-flavour.md` (§9), against
> the review's F2 and F7 (`REVIEW-linux-arm64-flavour.md`). It was read at master
> `9b8af3bd3017e52c1e1f161dd9fbf0dc2a58ebb9`, src/core only. **Reading only: nothing here changes code.** COORD
> accepted it on 2026-11-09 as the basis for S3's sites and sizing.

## 0. Method, and how to re-derive it

- **Layouts and syscall numbers come from Go's own go1.26.10 source,** never from memory. `go/types` checks
  package syscall at linux/amd64 and at linux/arm64 with `types.SizesFor("gc", arch)`. Nothing is built or run for
  the target.
- **The instrument:** [`data/la64-s3/layoutprobe/main.go`](data/la64-s3/layoutprobe/main.go). Re-run it from that
  folder with the pins (go1.26.10, `GOTOOLCHAIN=local`):

  ```
  go run . Stat_t,Rusage,Utsname,FdSet,Flock_t,Statfs_t,Sysinfo_t,Timex,Timeval,Timespec,EpollEvent,RawSockaddrAny,IPMreq,IPMreqn,IPv6Mreq,ICMPv6Filter,SockFilter,Msghdr,Iovec,Cmsghdr,Linger,Ucred,SysProcAttr > ../layouts-linux-amd64-arm64.tsv
  ```

  The run reproduced all three tables byte for byte.
- **Outputs:**
  - [`layouts-linux-amd64-arm64.tsv`](data/la64-s3/layouts-linux-amd64-arm64.tsv): size and every field offset of 23
    syscall types, at each arch;
  - [`sysnum-linux-amd64.tsv`](data/la64-s3/sysnum-linux-amd64.tsv) and
    [`sysnum-linux-arm64.tsv`](data/la64-s3/sysnum-linux-arm64.tsv): every `SYS_*` constant of package syscall,
    303 at amd64 and 268 at arm64.
- **The code predicates** are `git grep`/`grep -E` over the population in §1. Each section names its own.

## 1. Population

- **Linux-folder hand-owns: 25.** 20 `*_impl.cs` under `**/linux/`, plus 5 whole-file `GoManualConversion` files:
  - `internal/poll/linux/fd_writev_unix.cs`
  - `internal/syscall/unix/linux/siginfo_linux.cs`
  - `os/linux/wait_waitid.cs`
  - `runtime/linux/set_vma_name_linux.cs`
  - `syscall/linux/exec_unix.cs`
- **Flat `*_impl.cs` (in no GOOS folder): 95.**
- **Adjunct:** golib (206 files) is outside F2's population, so it was swept only for raw syscall numbers. Its only
  site is `golib/runtime/ThreadCpuTime.cs:145-150`, which is already an arch switch.
- **RECONCILE: 25 here, 24 in the design (§9: "20 `_impl.cs`, 4 whole-file").** The one is
  `runtime/linux/set_vma_name_linux.cs`.
  - It became a hand-own in `a909eb1daf` (2026-11-09 08:47), for runtime's TestDecorateMappings.
  - The design branch `claude/g-la64-design` is based on `0bbe3ab6e5`, which does not contain that commit.
  - So both counts are right for their trees. S2 and S3 use 25.
  - It is also not yet in repoguard's `testdata/handowns-baseline.txt`, which is a union taken at two older SHAs.
    That is expected for a hand-own newer than both, and is not a finding.

## 2. (i) Syscall numbers: 12 constants in 5 files

**Predicate:** every integer `const` in the population whose name contains "sys" (any case), read by eye; plus calls
with a literal first argument to `RawSyscall*`, `Syscall*` or libc `syscall`. There were none of the latter.

| File:line | Name | amd64 | arm64 | Action |
|---|---|---|---|---|
| internal/poll/linux/runtime_netpoll_impl.cs:96 | SYS_EPOLL_WAIT | 232 | **absent** | REPLACE: `epoll_pwait` 22, nil sigmask |
| :97 | SYS_EPOLL_CTL | 233 | 21 | renumber |
| :98 | SYS_EPOLL_CREATE1 | 291 | 20 | renumber |
| internal/syscall/unix/linux/net_linux_impl.cs:105 | sysRecvfrom | 45 | 207 | renumber |
| :106 | sysSendto | 44 | 206 | renumber |
| runtime/linux/signal_posix_impl.cs:701 | SYS_getpid | 39 | 172 | renumber |
| :702 | SYS_gettid | 186 | 178 | renumber |
| :703 | SYS_tgkill | 234 | 131 | renumber |
| :704 | SYS_exit_group | 231 | 94 | renumber (amd64's 231 is `munlockall` at arm64: it returns 0 and the caller carries on) |
| runtime/linux/sigprocmask_impl.cs:55 | SYS_rt_sigprocmask | 14 | 135 | renumber |
| syscall/linux/exec_unix.cs:915 | SYS_kill | 62 | 129 | renumber |
| syscall/linux/exec_unix.cs:852 | SYS_pidfd_open | 434 | 434 | none: 424 and up are the same on both arches (Go 1.26's syscall zsysnum carries it at neither) |

- **How they reach the kernel:** the three runtime files call libc's `syscall(2)` (`EntryPoint = "syscall"`). The
  others call the flavour's `RawSyscall6`/`Syscall6`.
- **netpoll is unreachable at arm64 today.** `runtime_pollServerInit` (:244-248) throws
  PlatformNotSupportedException for any process that is not X64, before the first number is used. This is why C2's
  arm64 calibration runs read no ENOSYS.
- **Disclosed:** a first, narrower predicate (`SYS_*` names) missed net_linux's two lower-case names. The predicate
  above finds all 12.

### 2.1 The flavour's SYS_ constants in hand-owns

- 28 mentions in 9 files name the converted package's `SYS_*`. Some are in comments, and netpoll's local constants
  are not counted. The arm64 flavour's zsysnum renumbers these with no hand edit.
- **Two do not exist at arm64.** Each is a compile error there, which is loud, but each needs a REPLACEMENT:
  - `syscall/linux/structclass_linux_impl.cs:116`, Select, uses `SYS_SELECT`, which is absent. Go's arm64 Select is
    `pselect6` (72), with the Timeval converted to a Timespec.
  - `syscall/linux/zsyscall_linux_amd64_impl.cs:181`, fstatat, uses `SYS_NEWFSTATAT`, which is named `SYS_FSTATAT`
    (79) at arm64. The file moves to `linux-amd64/` (design §2.7), and arm64 needs its own twin (§3).
- No hand-own P/Invokes a libc entry for a syscall arm64 lacks. `runtime/linux/stubs2_impl.cs:43` binds libc `open`,
  which glibc serves through `openat`.

### 2.2 The 57 amd64 syscall names absent at arm64, with replacements

- **Reached by the population today:**
  - epoll_wait → epoll_pwait 22 (sigmask NULL);
  - select → pselect6 72 (Timeval → Timespec, sigmask NULL);
  - newfstatat → fstatat 79 (the same arguments).
- **The rest**, for any future hand-own:
  - open → openat(AT_FDCWD) 56; creat → openat(O_CREAT|O_WRONLY|O_TRUNC); pipe → pipe2 59.
  - dup2 → dup3 24. dup3 returns EINVAL when old == new, which dup2 does not, so check first, as Go's arm64 Dup2
    does.
  - fork → clone 220 (SIGCHLD); vfork → clone(CLONE_VM|CLONE_VFORK|SIGCHLD).
  - getdents → getdents64 61.
  - rename → renameat 38 / renameat2 276; unlink → unlinkat 35; rmdir → unlinkat(AT_REMOVEDIR);
    mkdir → mkdirat 34; mknod → mknodat 33; link → linkat 37; symlink → symlinkat 36; readlink → readlinkat 78.
  - chmod → fchmodat 53; chown and lchown → fchownat 54; access → faccessat 48; stat and lstat → fstatat 79.
  - poll → ppoll 73; epoll_create → epoll_create1 20; inotify_init → inotify_init1 26; eventfd → eventfd2 19;
    signalfd → signalfd4 74.
  - alarm → setitimer 103; pause → ppoll(NULL) or rt_sigsuspend 133; time → clock_gettime 113;
    utime, utimes and futimesat → utimensat 88; getpgrp → getpgid(0) 155.
  - No replacement: ustat, uselib, sysfs and the x86-only calls (arch_prctl, ioperm, iopl, modify_ldt,
    get/set_thread_area).

## 3. (ii) Size and offset constants, and (iii) layout mirrors

**Predicates:**
- (ii): `const <int type> <name containing Size|Offset|Bytes|Len|Off> = <literal>`. That gives 23 constants in 5
  files. `reflect/deepequal_impl.cs:600` (Length = 8) also matches, but it is not a kernel layout.
- (iii): mirror TYPES. `[StructLayout(...)]` declarations give 18 in 4 files:
  - `syscall/linux/sockaddr_linux_impl.cs`: 6;
  - `syscall/linux/structclass_linux_impl.cs`: 6;
  - `syscall/linux/zsyscall_linux_amd64_impl.cs`: 5;
  - `internal/syscall/unix/linux/siginfo_linux.cs`: 1.
  Add 2 Marshal-offset image mirrors with no StructLayout: netpoll's epoll_event and signal_posix's sigaction.

**Each mirrored type was checked against Go's own type at both arches** (`layouts-linux-amd64-arm64.tsv`). ONLY TWO
DIFFER:

- **epoll_event** (`runtime_netpoll_impl.cs:113-115`): 12 bytes packed at amd64, with the data at 4. At arm64 it is
  16 bytes, with a pad at 4 and the data at 8 (Go's EpollEvent at arm64: Events@0 _@4 Fd@8 Pad@12).
- **Stat_t** (`zsyscall_linux_amd64_impl.cs:139`): 144 bytes at amd64. At arm64 it is 128 bytes, in a different
  order: Mode@16 comes before Nlink@20, and Blksize is an int32 plus a pad. It needs an arm64 twin mirror, not a
  constant change.

**Identical at both arches, size and every field offset:** Rusage 144, Utsname 390, FdSet 128, Flock_t 32,
Statfs_t 120, Sysinfo_t 112, Timex 208, RawSockaddrAny 112 (the hand-own's sockaddr_storage image is 128), IPMreq 8,
IPMreqn 12, IPv6Mreq 20, ICMPv6Filter 32 and Msghdr 56. `siginfo_linux.cs`'s `Size = 128` is the kernel's fixed
siginfo size on both arches, but it was not in the probe's type list.

**Not decidable from Go source:**
- libc `struct sigaction` (`signal_posix_impl.cs:162-163`: 160 bytes, sa_flags at 136). glibc's x86_64 and aarch64
  share the generic shape {handler, sigset_t of 128 bytes, int flags, restorer}, so I expect it to be identical. An
  arm-runner round trip (set sa_flags, read them back) must prove it.

**Page size:** `runtime/linux/mem_linux_impl.cs:123-128` and `runtime/mranges_impl.cs:191` hard-code 4096, and arm64
kernels may run 16K or 64K pages. Read the page size instead; `syscall/syscall_impl.cs:38` already does.
`runtime/linux/set_vma_name_linux.cs:66` uses `(uintptr)4096` as a length, not as a page assumption: the seat judges
it.

- **Site 8, done (i9, 2026-11-10, `aaef00bef6`; COORD-accepted).** mem_linux's probe strides by `physPageSize`, and
  its test asserts alignment to the OS page. mranges' 4096 is a range length in pure arithmetic, so it is judged and
  kept. set_vma_name's 4096 is the probe's mmap/prctl/munmap length; the kernel rounds all three to a whole page, so
  it is kept too. **Unmeasured: a 16 or 64 KiB kernel.** linux-x64 has 4 KiB pages, and so, unmeasured, does the arm
  runner's generic Ubuntu kernel. On a 4 KiB kernel the old and new assertions agree, so no red is possible there.

- **RECONCILE: 18 + 2 mirror types here, "30 explicit-layout mirrors in 7 files" in the design (§9).** The design
  does not state its predicate. The candidates tried at the design's own base `0bbe3ab6e5` do not reproduce 30 in 7:
  - StructLayout lines: 20 in 4 files (2 of them in comments);
  - StructLayout or `fixed` buffers: 43 in 4;
  - `unsafe struct` or StructLayout: 33 in 4;
  - struct declarations: 19 in 4;
  - `Marshal.Read/Write*` calls: 20 in 5.

  Both counts stand until G records the design's predicate. The sizing in §6 is driven by the two types that DIFFER,
  not by either count.

## 4. F7: unaligned 64-bit atomics

- No hand-own in the population applies Interlocked or Volatile to a field of a packed or explicit mirror. The four
  files that declare layouts have zero uses, and `Pack=` appears nowhere outside tests.
- The converted explicit layouts carry Go's offsets, for example `internal/runtime/atomic/types.cs` (Int64 with
  align64 at FieldOffset 0) and `internal/fuzz/pcg.cs`. Go aligns 64-bit fields to 8 at amd64 and at arm64, so the
  offsets coincide.
- **Not provable by reading:** that the CLR places such a struct at an 8-aligned base when it is embedded or boxed on
  arm64. Run `internal/runtime/atomic`'s own tests on the arm runner after S4; they exercise Int64 and Uint64 inside
  structs.

## 5. The mandated pattern

- **Every hand-written syscall number takes one of two forms:**
  - **(a) a `RuntimeInformation.ProcessArchitecture` switch**, each arch's number explicit and an unknown arch
    refused, as `golib/runtime/ThreadCpuTime.cs:145-150` does. This is for files that compile in every flavour,
    such as the libc-syscall seams in runtime/linux.
  - **(b) the flavour's converted `syscall_package.SYS_*`**, for files inside the syscall flavour.
- Never a bare literal.
- **A syscall arm64 lacks is REPLACED** by its arm64 equivalent, with any argument conversion. It is never
  renumbered.
- **A mirror whose layout differs** gets an arch twin, or an arch switch on its constants, with a GolibTests arm that
  round-trips it on the arm runner.

## 6. Sites and size

| # | Site | Work | Lane-days |
|---|---|---|---|
| 1 | netpoll | epoll_pwait in place of epoll_wait; create1 and ctl through the switch; the 16-byte event; an unknown arch refused; the X64 guard stays until the arm read exists | 1-1.5 |
| 2 | net_linux | recvfrom and sendto through the switch | 0.25 |
| 3 | signal_posix | 4 numbers through the switch; prove sigaction 160/136 on aarch64 glibc | 0.5 |
| 4 | sigprocmask | 1 number through the switch | 0.1 |
| 5 | exec_unix | SYS_kill through the switch (pidfd_open is unchanged) | 0.25 |
| 6 | structclass Select | pselect6 at arm64, with the Timeval → Timespec conversion | 0.5 |
| 7 | the zsyscall twin | `zsyscall_linux_arm64_impl.cs`: the 128-byte Stat_t mirror, fstatat 79 and fstat 80; Rusage and Utsname are reused | 0.75-1 |
| 8 | page size | mem_linux and mranges read it | 0.25 |
| 9 | design §2.7's "stay" list | bytealg, chacha8, cpuflags, mranges, panic, sema, malloc, netpoll_epoll and syscall_linux, each judged against its arm64 principal; plus M2's wall (testSPWrite, archLog, archHypot) | 1.5-2.5 |
| 10 | F7 | the atomic-alignment arm on the arm runner | 0.25 |
| | **Total** | | **5.3-7.1** |

- **Ordering, as ruled (COORD, 2026-11-09):** sites 1-8 are written behind the arch switch, each in its own commit,
  and gated at amd64 for no regression.
- **Their arm readings follow S2 (G's flip) and S4 (osInitArm64).** netpoll's X64 guard is lifted only in the commit
  that carries the passing arm64 read.

## Amendment, 2026-11-09: three sites from G's S2 prediction

**Where they come from.** G's S2 prediction (`claude/g-la64-s2`, record `DATA-la64-s2-prediction.md`, not yet
pushed when this was written) shows that once S2 homes the amd64-only files in `amd64/` and `linux-amd64/`, the
linux/arm64 flavour compiles its OWN files. Three hand-owns then lose something they depend on.

**Each was checked in code at master 9b8af3bd30.** None of them is in §2-§3's predicates: they are not numbers or
layouts, but a dependency on a file S2 moves.

| # | Site | What breaks at linux/arm64 after S2 | Work | Lane-days |
|---|---|---|---|---|
| 0 | internal/cpu initializer | The package's only two `[ModuleInitializer]`s are `cpu_x86_impl.cs:123` and the darwin stand-in `cpu_arm64_impl.cs:60`, and §2.7 homes both in `amd64/`. At linux/arm64 nothing runs `doinit` or fills `cpu.HWCap`/`HWCap2`, so every `cpu.ARM64` flag reads false: the hash, crypto and bytealg CPU branches. | A `linux-arm64/` initializer hand-own: getauxval(AT_HWCAP/AT_HWCAP2), then the converted `doinit`. It reuses S4's mapping (C2, `463a4d3283`), which today serves the amd64-converted flavour on an arm64 host, a path S2 retires. The arm-runner cross-check reads .NET's `Arm.*.IsSupported`, which is never the source. | 0.5-0.75 |
| a | runtime/hash_impl.cs:185 | `initHashAlgorithm` reads `cpu.GoIsArm64Process`, declared only by the stand-in (`cpu_arm64_impl.cs:54`), now in `amd64/`. That is CS0117 at linux/arm64. | Test the arch the way Go's hash selection does, by GOARCH, so that it holds for every flavour on an arm64 process (the darwin arm64 path included). | 0.1-0.25 |
| b | syscall/linux/structclass_linux_impl.cs:97, :416 | Select's timeout and Timex's Time name `NativeTimevalLinuxAmd64`, declared in `zsyscall_linux_amd64_impl.cs:238`, which moves to `linux-amd64/`. | Timeval is 16 bytes at both arches (§3), so hoist the mirror into `structclass_linux_impl.cs` under an arch-neutral name, used by both twins. It lands with site 6, where Select's arm64 path takes a Timespec. | 0.25 |

- **Ordering:** site 0 goes ahead of site 1, since without it every arm64 CPU-feature branch is dark. Sites 0, a
  and b are compile-level at linux/arm64, so they are testable as soon as S2 lands.
- **Revised total: 6.15-8.35 lane-days** (§6's 5.3-7.1, plus 0.85-1.25).

### The mirror reconcile of §3, resolved (G, 2026-11-09)

- **The design's predicate:** matching LINES of the unanchored extended regex `StructLayout|FieldOffset|Pack *= *[0-9]`
  over the same population, at `0bbe3ab6e5`. It reads 30 lines in 7 files:
  - 20 StructLayout lines in the 4 files of §3. Dropping the two that are not declarations gives this record's 18
    types.
  - 10 lines in 3 flat files (`reflect/value_impl.cs` 6, `internal/reflectlite/type_impl.cs` 2,
    `internal/abi/type_impl.cs` 2), which match FieldOffset only as a substring of the helper name
    `GoReflect.GoFieldOffsets` (calls and comments). None is a layout attribute: the population holds no
    `[FieldOffset]` and no `Pack=` line.
- So the 10 extra lines and the 3 extra files are false positives of the unanchored alternation, and class (iii) is
  18 + 2 types, as §3 reads. G amends the design's S3 row.
- **Named, not claimed (G):** GoFieldOffsets computes Go's amd64 field offsets, so those three files may belong on
  a list of their own for arm64 alignment rules. Go's gc sizes align every field the same way at amd64 and arm64
  (§3's probe found no offset difference in any type it read), but nobody has measured that for GoFieldOffsets'
  callers.

## Amendment, 2026-11-10: the arm64-only stubbed declarations (S3 site 9, COORD-ordered)

**Why.** PartialStubGenerator gives every bodyless `partial` with no implementing part a THROWING body. The project
then compiles clean and dies at the first call. At linux/arm64 that can happen to a declaration that only arm64
emits: `math.archExp2` was one (9c), and no amd64 reading could see it. COORD made the class a site 9 deliverable:
triage every arm64-only stubbed declaration as (a) bodied, (b) a legitimate Go-side stub (cite Go's file), or
(c) a named refusal. Leave the count explained row by row.

**The instrument, and why the census alone cannot answer.** G's declared-not-implemented census
(`declaredNotImplemented_test.go`, read with every key folder since S2's `25bc6c1720`) POOLS a package's key folders
into one population. A body that exists in only one flavour's folder therefore satisfies the name for every flavour.
The seat ran the UNCHANGED census function over per-flavour views of `src/core`: the flat files plus only that
flavour's key folders (goos, arch and goos-arch, the set its csproj compiles), selected with the converter's own
`isLayoutKeyFolderName`. Control: the unfiltered view equals the census test's own reading on the same tree.

| reading at | pooled (control = census test) | linux/arm64 | linux/amd64 | windows/amd64 | darwin/amd64 |
|---|---|---|---|---|---|
| `a719a19546` (site 9a-9d, before this triage) | 843 decl, 501 stubbed, 43 pkgs (= census) | 214 | 231 | 213 | 427 |
| after this triage | 843 decl, 495 stubbed, 43 pkgs (= census) | 208 | 231 | 213 | 427 |

**S2's +23 stubbed, explained.** S2 read 503 stubbed against master's 480 (COORD).
- Two of the 23 were bodied before this triage: `internal/cpu.getMIDR` (site 0, `8ca192fb77`) and `math.archExp2`
  (9c, `ebe1b5ea34`).
- The other 21 are the pooled census's stubs in `linux-arm64/` files at `a719a19546`, one row each below.
- 15 of the 21 are stubbed at linux/arm64 and at no other flavour; the per-flavour views compute this set
  difference against linux/amd64 by (package, member). The other 6 are each arch's own assembly, stubbed at
  linux/amd64 too, and S3 leaves them as they are.

| # | member | Go's declaration | class | reach and disposition |
|---|---|---|---|---|
| 1 | hash/crc32 `castagnoliUpdate` | crc32_arm64.go:13 | (a) bodied | Reached whenever `cpu.ARM64.HasCRC32` (crc32_arm64.go:16-31), which site 0 fills from AT_HWCAP. `linux-arm64/crc32_arm64_impl.cs`: Arm.Crc32 CRC32CX/CRC32CB, the instructions crc32_arm64.s issues. |
| 2 | hash/crc32 `ieeeUpdate` | crc32_arm64.go:14 | (a) bodied | The same for IEEE (crc32_arm64.go:34-49): CRC32X/CRC32B. |
| 3 | internal/runtime/sys `EnableDIT` | dit_arm64.go:15 | (a) bodied | Go's own no_dit.go:11 body (false). `DITSupported` is false because HasDIT is held false (site 0, darwin ruling). The unguarded callers are GODEBUG `dataindependenttiming=1` in mstart (proc.go:1930), which never runs here, and cgocallbackg on a C thread (cgocall.go:443), which has no cgo. |
| 4 | internal/runtime/sys `DITEnabled` | dit_arm64.go:16 | (a) bodied | no_dit.go:12 (false), as row 3. |
| 5 | internal/runtime/sys `DisableDIT` | dit_arm64.go:17 | (a) bodied | no_dit.go:13 (no-op), as row 3. |
| 6 | runtime `walltime` | timestub2.go:10 | (a) bodied | timestub.go:27's `time_now` calls it. time.Now does not (time/time_impl.cs reads the clock itself). `linux-arm64/timestub2_impl.cs`: DateTime.UtcNow, read as time_impl's now() reads it. |
| 7 | runtime `load_g` | stubs_arm64.go:10 | (b) Go-side | "Called from assembly only; declared for go vet" (stubs_arm64.go:9). No Go caller, and no assembly is converted. |
| 8 | runtime `save_g` | stubs_arm64.go:11 | (b) Go-side | As row 7. |
| 9 | runtime `emptyfunc` | stubs_arm64.go:16 | (c) refused | No caller anywhere in Go's non-test tree. |
| 10 | internal/cpu `getisar0` | cpu_arm64.go:30 | (c) refused | Its callers are freebsd's and openbsd's osInit (cpu_arm64_freebsd.go:11-14, cpu_arm64_openbsd.go:34), never linux's, which maps AT_HWCAP (site 0). |
| 11 | internal/cpu `getpfr0` | cpu_arm64.go:32 | (c) refused | As row 10. |
| 12 | x/sys/cpu `getisar0` | cpu_gc_arm64.go:9 | (c) refused | Reached only through readARM64Registers (x/sys/cpu cpu_arm64.go:65-68), which doinit calls only when readHWCAP fails on a 4.11+ kernel (cpu_linux_arm64.go:63-82). readHWCAP fails only when both the runtime's auxv AND /proc/self/auxv are unreadable. The runtime copy is always empty here (sysargs never runs), so /proc/self/auxv is the path, and only a setcap'd binary loses it. Managed code cannot issue MRS, and the kernel publishes no sysfs copy of these registers. |
| 13 | x/sys/cpu `getisar1` | cpu_gc_arm64.go:10 | (c) refused | As row 12. |
| 14 | x/sys/cpu `getpfr0` | cpu_gc_arm64.go:11 | (c) refused | As row 12. |
| 15 | x/sys/cpu `getzfr0` | cpu_gc_arm64.go:12 | (c) refused | As row 12, and further behind SVE in pfr0 (cpu_arm64.go:176). |
| 16 | runtime `asmcgocall_no_g` | stubs_arm64.go:14 | shared | Each arch's own assembly. Stubbed at linux/amd64 too, and unchanged by S3. |
| 17 | runtime `panicBounds` | arm64 assembly | shared | As row 16. |
| 18 | runtime `panicExtend` | arm64 assembly | shared | As row 16. |
| 19 | runtime `spillArgs` | arm64 assembly | shared | As row 16. |
| 20 | runtime `unspillArgs` | arm64 assembly | shared | As row 16. |
| 21 | syscall `rawVforkSyscall` | arm64 assembly | shared | As row 16. |

- **After the triage:** the pooled census's `linux-arm64/` stubs are 15 (21 less rows 1-6). 9 stubs exist only at
  linux/arm64 (rows 7-15), all class (b) or (c).
- **The arm leg's sweeps read the bodies:** hash/crc32 (rows 1-2), math (9c's archExp2), and crypto/subtle and
  time (rows 3-6). A compile shows none of them, because a throw is a body.
- **Not done here:** the instrument is a scratch test of the seat's, run and then deleted. Making the per-flavour
  view a standing guard beside G's census is a proposal to COORD, not part of this commit.

### The complete linux-arm64 declaration table (COORD ruling (1), 2026-11-10)

Every bodyless `partial` declaration the census counts in a `linux-arm64/` file, at `1f2f6f82fd`. These are **57 in 10 packages**, which is S2's +57 over master. The list re-walks with the census's own predicates
(`partialDeclRe`, `partialDefnRe`, `leadingTrivia` and its two exclusions) and is held to it in every view:
the declaration count equals the census's, and the stubbed subset equals the census's stubs member for member.
**42 have bodies and 15 are stubbed.** The stubbed rows are the 15 the table above leaves: rows 7-21, all (b), (c) or shared.

- **A body in a flat file serves every flavour** (atomic, bytealg, chacha8rand, the runtime and syscall rows).
- **That is what design §2.7's stay list judges** for arm64, the next S3 item.
- **The pooled census cannot tell** whether such a body is right for arm64, only that it exists.

| # | package | member | declared in | status | body in |
|---|---|---|---|---|---|
| 1 | hash/crc32 | `castagnoliUpdate` | `hash/crc32/linux-arm64/crc32_arm64.cs` | bodied | `hash/crc32/linux-arm64/crc32_arm64_impl.cs` |
| 2 | hash/crc32 | `ieeeUpdate` | `hash/crc32/linux-arm64/crc32_arm64.cs` | bodied | `hash/crc32/linux-arm64/crc32_arm64_impl.cs` |
| 3 | internal/bytealg | `MakeNoZero` | `internal/bytealg/linux-arm64/bytealg.cs` | bodied | `internal/bytealg/bytealg_impl.cs` |
| 4 | internal/chacha8rand | `block` | `internal/chacha8rand/linux-arm64/chacha8.cs` | bodied | `internal/chacha8rand/chacha8_impl.cs` |
| 5 | internal/cpu | `getMIDR` | `internal/cpu/linux-arm64/cpu_arm64.cs` | bodied | `internal/cpu/linux-arm64/cpu_arm64_hwcap_impl.cs` |
| 6 | internal/cpu | `getisar0` | `internal/cpu/linux-arm64/cpu_arm64.cs` | stubbed | - |
| 7 | internal/cpu | `getpfr0` | `internal/cpu/linux-arm64/cpu_arm64.cs` | stubbed | - |
| 8 | internal/runtime/atomic | `And` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 9 | internal/runtime/atomic | `And32` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 10 | internal/runtime/atomic | `And64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 11 | internal/runtime/atomic | `And8` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 12 | internal/runtime/atomic | `Anduintptr` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 13 | internal/runtime/atomic | `Cas64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 14 | internal/runtime/atomic | `CasRel` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 15 | internal/runtime/atomic | `Or` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 16 | internal/runtime/atomic | `Or32` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 17 | internal/runtime/atomic | `Or64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 18 | internal/runtime/atomic | `Or8` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 19 | internal/runtime/atomic | `Oruintptr` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 20 | internal/runtime/atomic | `Store` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 21 | internal/runtime/atomic | `Store64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 22 | internal/runtime/atomic | `Store8` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 23 | internal/runtime/atomic | `StoreRel` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 24 | internal/runtime/atomic | `StoreRel64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 25 | internal/runtime/atomic | `StoreReluintptr` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 26 | internal/runtime/atomic | `StorepNoWB` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 27 | internal/runtime/atomic | `Xadd` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 28 | internal/runtime/atomic | `Xadd64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 29 | internal/runtime/atomic | `Xadduintptr` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 30 | internal/runtime/atomic | `Xchg` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 31 | internal/runtime/atomic | `Xchg64` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 32 | internal/runtime/atomic | `Xchg8` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 33 | internal/runtime/atomic | `Xchguintptr` | `internal/runtime/atomic/linux-arm64/atomic_arm64.cs` | bodied | `internal/runtime/atomic/atomic_impl.cs` |
| 34 | internal/runtime/sys | `DITEnabled` | `internal/runtime/sys/linux-arm64/dit_arm64.cs` | bodied | `internal/runtime/sys/linux-arm64/dit_arm64_impl.cs` |
| 35 | internal/runtime/sys | `DisableDIT` | `internal/runtime/sys/linux-arm64/dit_arm64.cs` | bodied | `internal/runtime/sys/linux-arm64/dit_arm64_impl.cs` |
| 36 | internal/runtime/sys | `EnableDIT` | `internal/runtime/sys/linux-arm64/dit_arm64.cs` | bodied | `internal/runtime/sys/linux-arm64/dit_arm64_impl.cs` |
| 37 | math | `archExp2` | `math/linux-arm64/exp2_asm.cs` | bodied | `math/linux-arm64/exp2_asm_impl.cs` |
| 38 | runtime | `asmcgocall_no_g` | `runtime/linux-arm64/stubs_arm64.cs` | stubbed | - |
| 39 | runtime | `emptyfunc` | `runtime/linux-arm64/stubs_arm64.cs` | stubbed | - |
| 40 | runtime | `getfp` | `runtime/linux-arm64/stubs_arm64.cs` | bodied | `runtime/stubs_impl.cs` |
| 41 | runtime | `load_g` | `runtime/linux-arm64/stubs_arm64.cs` | stubbed | - |
| 42 | runtime | `panicBounds` | `runtime/linux-arm64/panic.cs` | stubbed | - |
| 43 | runtime | `panicExtend` | `runtime/linux-arm64/panic.cs` | stubbed | - |
| 44 | runtime | `save_g` | `runtime/linux-arm64/stubs_arm64.cs` | stubbed | - |
| 45 | runtime | `spillArgs` | `runtime/linux-arm64/stubs_arm64.cs` | stubbed | - |
| 46 | runtime | `unspillArgs` | `runtime/linux-arm64/stubs_arm64.cs` | stubbed | - |
| 47 | runtime | `walltime` | `runtime/linux-arm64/timestub2.cs` | bodied | `runtime/linux-arm64/timestub2_impl.cs` |
| 48 | syscall | `cgocaller` | `syscall/linux-arm64/syscall_linux.cs` | bodied | `syscall/linux/cgocaller_linux_impl.cs` |
| 49 | syscall | `rawSyscallNoError` | `syscall/linux-arm64/syscall_linux.cs` | bodied | `syscall/linux/syscall_linux_impl.cs` |
| 50 | syscall | `rawVforkSyscall` | `syscall/linux-arm64/syscall_linux.cs` | stubbed | - |
| 51 | syscall | `runtime_doAllThreadsSyscall` | `syscall/linux-arm64/syscall_linux.cs` | bodied | `syscall/linux/syscall_linux_impl.cs` |
| 52 | syscall | `runtime_entersyscall` | `syscall/linux-arm64/syscall_linux.cs` | bodied | `syscall/linux/syscall_linux_impl.cs` |
| 53 | syscall | `runtime_exitsyscall` | `syscall/linux-arm64/syscall_linux.cs` | bodied | `syscall/linux/syscall_linux_impl.cs` |
| 54 | vendor/golang.org/x/sys/cpu | `getisar0` | `vendor/golang.org/x/sys/cpu/linux-arm64/cpu_gc_arm64.cs` | stubbed | - |
| 55 | vendor/golang.org/x/sys/cpu | `getisar1` | `vendor/golang.org/x/sys/cpu/linux-arm64/cpu_gc_arm64.cs` | stubbed | - |
| 56 | vendor/golang.org/x/sys/cpu | `getpfr0` | `vendor/golang.org/x/sys/cpu/linux-arm64/cpu_gc_arm64.cs` | stubbed | - |
| 57 | vendor/golang.org/x/sys/cpu | `getzfr0` | `vendor/golang.org/x/sys/cpu/linux-arm64/cpu_gc_arm64.cs` | stubbed | - |

## Amendment, 2026-11-10: design §2.7's stay list, judged (S3, after site 9)

Design §2.7 keeps nine hand-owned files FLAT (or in `linux/`), so they compile unchanged at linux/arm64 against a
principal that differs there, and asks S3 to judge each one. **Compiling is settled:** GolibTests' whole closure
compiles at linux/arm64 since 9d. The question is semantic: does the body assume amd64?

**Method.** For each file:
- the members it bodies were listed from the per-flavour declaration tables (the 57-row table above, and the same
  table at linux/amd64), and each member's arm64 declaration was compared with its amd64 one;
- its converted principal was diffed between `amd64/` (or `linux-amd64/`) and `linux-arm64/`, because these principals
  are PER-ARCH emissions, so S2 found that they differ;
- the body was read for whether it touches what differs, or anything else arch-bound: registers, sizes, layouts, x86
  features, syscall numbers.

| file | bodies at arm64 / amd64 | arm64 principal | judgment |
|---|---|---|---|
| `internal/bytealg/bytealg_impl.cs` | 9 / 9 (the same members) | `linux-arm64/bytealg.cs` | **Stays.** Go's arm64 assembly covers the same family (compare, count, index, indexbyte). Each body implements the arch-neutral contract for any length. The one per-arch knob, `MaxLen` (32 at arm64), is set by the converted `index_arm64.go` init, and callers check it before calling `Index`. |
| `internal/chacha8rand/chacha8_impl.cs` | 1 / 1 (`block`) | `linux-arm64/chacha8.cs` | **Stays.** The two assembly headers name different array shapes, but Go's own `TestBlockGeneric` holds `block` equal to `block_generic` on every arch, so the stored layout is the generic one, and that is what this body writes. |
| `runtime/cpuflags_impl.cs` | 0 (a module initializer) | `linux-arm64/cpuflags.cs` + `cpuflags_arm64.cs` | **Stays.** The per-arch difference is the X86 field offsets, which it never reads. Its `GOARCH == "arm64"` branch is cpuinit's arm64 line (`arm64HasATOMICS = cpu.ARM64.HasATOMICS`), and site 0's initializer fills `cpu.ARM64` first. `arm64UseAlignedLoads` comes from `cpuflags_arm64.go`'s own converted init. |
| `runtime/mranges_impl.cs` | 0 | `linux-arm64/mranges.cs` | **Stays.** The per-arch difference is `maxOffAddr` (a 47- versus 48-bit address space). The body has no address-space constant and no `offAddr` arithmetic of its own: it builds ranges through the converted `makeAddrRange`. It is addrRanges arithmetic over 16-byte ranges and `goarch.PtrSize`, which is 8 at both arches. Its 4096 is a range length (site 8). |
| `runtime/panic_impl.cs` | 0 (throw/fatal displaced) | `linux-arm64/panic.cs` | **Stays.** The per-arch difference is one folded `goarch.IsAmd64` condition. It forwards to golib's FatalReport, with no arch content. The arm64 `panic.cs` carries the displacement placeholders, and the 0-error compile rules out a second body. |
| `runtime/sema_impl.cs` | 0 (semacquire1/semrelease1) | `linux-arm64/sema.cs` | **Stays.** The per-arch difference is the cache-line pad (40 versus 104 bytes), which it never touches. It forwards to golib's RuntimeSemaphore, with no arch content. |
| `runtime/malloc_impl.cs` | 0 (`zerobase`) | `linux-arm64/malloc.cs` | **Stays.** The per-arch difference is `arenaBaseOffset` (0xffff8… at amd64, 0 at arm64), which `zerobase` does not involve. `zerobase` is a `uintptr` box over golib's GoZeroBase, 8 bytes at both arches. |
| `runtime/linux/netpoll_epoll_impl.cs` | 0 (`netpollBreak`) | `linux-arm64/netpoll_epoll.cs` | **Stays.** The per-arch difference is `EpollEvent.Data`'s type, which this body never reads. It refuses `netpollBreak` by name (there is no runtime poller) and touches no `epoll_event`, so the 12-versus-16-byte layout never reaches it. The layout-bearing epoll code is internal/poll's (site 1). |
| `syscall/linux/syscall_linux_impl.cs` | 4 / 4 (the same members) | `linux-arm64/syscall_linux.cs` | **Stays.** The four declarations are byte-identical in both arches' `syscall_linux.cs`. entersyscall and exitsyscall forward to the managed bracket. `rawSyscallNoError` goes through `internal/runtime/syscall/linux.Syscall6` with the trap number its caller takes from that flavour's own table. `doAllThreadsSyscall` answers ENOTSUP. Only its header comment speaks of amd64's `SYSCALL` instruction. |

**Not on the list, but the table shows it serving arm64:** `internal/runtime/atomic/atomic_impl.cs` bodies 26 arm64
declarations. The member set is identical at both arches (43 names). Its primitives are `Interlocked.*`, which is
sequentially consistent on arm64, and `Volatile.Read`/`Write`, which emit LDAR/STLR there, the same instructions Go's
arm64 atomics use. **It stays.**
- **One latent hazard, NOT arch-specific and NOT S3's:** `Store8` is a plain `Volatile.Write`, while `And8`, `Or8`
  and `Xchg8` take `s_wideLatch`. A `Store8` landing inside an `And8`'s window on the same byte would be lost.
- That is the same at amd64; the latch's fences make arm64 no different. It is recorded here and routed, not changed.

**Result: §2.7's stay list needs no code change at linux/arm64.** The arm leg's runs remain the behavioural read of
every row above. Nothing here substitutes for them. The site 0 reading COORD ruled in rides with this amendment:
`LinuxArm64CpuStartupTests`, which runs at linux/arm64 only.
