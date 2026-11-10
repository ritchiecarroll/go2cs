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
