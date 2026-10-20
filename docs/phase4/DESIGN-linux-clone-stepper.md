# DESIGN — the Linux clone stepper: SysProcAttr fields posix_spawn cannot express

**STATUS: PROPOSED (P2, 2026-10-20).** Shape (A) of the sizing was ruled the right shape and scheduled
AFTER the go1.26.10 hop (coordinator, 2026-10-20): it brings hand-assembled machine code mapped executable,
per architecture, which deserves G's full design review and its own arc, and no production behavior in the
hop depends on it. Nothing here is implemented. Template per `DESIGN-linux-exec.md`: the measured bill
first, the design second, the adversarial review third, open questions with recommendations last.

This document extends `DESIGN-linux-exec.md` (ratified 2026-08-22) and changes none of its rulings.
posix_spawn(3) stays the spawn path for every request it can express. This design covers only the requests
that seam refuses today with a named ENOTSUP.

---

## 1. The measured bill

Measured at go1.26.10 on linux (uid 0, a container that permits user namespaces), 2026-10-20.

**Population.** Every `*_test.go` and production file in std (cmd excluded) that sets a `SysProcAttr`
field the seam refuses:

| Package | Tests | Verdicts | Fields | Today |
|---|---|---|---|---|
| `syscall` | 15 functions in `exec_linux_test.go`, `exec_pdeathsig_test.go`, `syscall_ptrace_test.go` | 17 | Cloneflags with Uid/GidMappings, Unshareflags, Credential, Chroot, Pdeathsig, AmbientCaps, Ptrace, UseCgroupFD | 14 platform-skip + 1 host-limit disclosed; `TestUseCgroupFD` is the Q15 host condition |
| `crypto/x509` | `TestSetFallbackRoots` | 1 + 9 | Cloneflags NEWNS\|NEWUSER, one-ID maps | 9 platform-skip disclosed for the hop |
| `os/exec` | `TestCredentialNoSetGroups` | 1 | Credential | host-limit (linux, darwin) |
| `os/signal` | one `Setctty` site | — | Setctty | cgo-only, never built here |

No production std code sets a refused field (`os` sets `PidFD`, which the seam already serves through
`pidfd_open`). Every verdict above is a TEST.

**Go's verdicts on that host:** 14 of the 17 `syscall` verdicts pass (`TestUnshareUidGidMapping`,
`TestUseCgroupFD` and `TestDeathSignalSetuid` skip); `TestSetFallbackRoots` runs all nine subtests, and
its result depends on `SSL_CERT_FILE` (set, Go fails the parent and four subtests); `TestCredentialNoSetGroups`
runs as root. About 25 linux verdicts where C# skips today would become comparable on such a host.

**When the kernel refuses.** Go's child writes the errno to the status pipe and `ForkExec` returns it.
Every test above skips through `testenv.SyscallIsNotSupported` (EPERM, EINVAL, EROFS, `fs.ErrPermission`,
`errors.ErrUnsupported`). The seam's ENOTSUP rides the same skip, which is why C# reads "skip" everywhere.

## 2. The constraint

Some native code must run single-threaded between `clone` and `execve`. Creating a user namespace with
`unshare` or `setns` is refused in a multithreaded process (EINVAL), and the CLR is multithreaded from
start, so the namespace must come from `clone` itself or from a child that is single-threaded. The child
of a CLR `clone` cannot run managed code (`DESIGN-linux-exec.md` §2: unsound by rule, not by
measurement). posix_spawn's child is glibc's native code, but glibc takes no clone flags, credentials,
ptrace, pdeathsig, chroot or cgroup.

Rejected shapes (from the sizing):
- **Host tools** (`unshare(1)`, `setpriv(1)` as an exec chain): the pid survives each exec, but a refusal
  arrives as the tool's exit status after the child has started, not as an errno from `StartProcess`, so
  on a host that refuses, C# would FAIL where Go skips.
- **A C shim compiled at build, or a Go helper binary**: `DESIGN-linux-exec.md` §2(b)'s toolchain ruling;
  and a Go helper cannot create a user namespace with `unshare` either (its runtime is multithreaded), so
  it would have to clone a grandchild, breaking the pid and wait contract.

## 3. The design

### 3.1 Dispatch

`forkExec` keeps posix_spawn as the default. When `unsupportedSysProcAttrField` names a field, the request
goes to the stepper path instead of failing; the function becomes the dispatch predicate and its message
stays the ENOTSUP text for hosts where the stepper is unavailable (§3.6). Rows that spawn without these
fields never reach the new path, so no banked row moves.

### 3.2 The parent half is Go's

The parent runs Go's own `forkExec` protocol, which the converter already emits (`exec_unix.cs.auto` beside
the hand-own) and `exec_linux.go`'s `forkAndExecInChild` parent tail:
- `ForkLock` held for writing across the spawn, as today;
- the CLOEXEC status pipe: EOF means `execve` succeeded; eight bytes are the child's errno, after which the
  parent reaps the child and returns that errno;
- with user-namespace mappings, the map pipe: the parent writes `/proc/<pid>/uid_map`, `setgroups`
  (`deny` unless `GidMappingsEnableSetgroups`) and `gid_map`, then closes its end to release the child
  (`writeUidGidMappings`, converted as is);
- `PidFD` filled from `CLONE_PIDFD` (or `clone3`'s pidfd field), as Go does, instead of the post-spawn
  `pidfd_open` the posix_spawn path uses;
- all signals blocked around the clone (Go's `runtime_BeforeFork`), restored after; the child restores its
  mask as one of its steps.

### 3.3 The child half is Go's sequence, as data

The parent turns `forkAndExecInChild1` into a list of raw-syscall steps, in Go's order, from the
`SysProcAttr`, `ProcAttr` and the prepared argv/envp. The builder reads like the Go function, one `if` per
Go `if`, the way the seam already turns Go's fd shuffle into posix_spawn file actions. At go1.26.10 the
order is:

1. `prctl(PR_SET_KEEPCAPS, 1)` when AmbientCaps is set.
2. With mappings: `close(mapPipe[1])`, `read(mapPipe[0], &err2, 8)`; fail unless 8 bytes read and `err2 == 0`.
3. `setsid`; `setpgid(0, Pgid)`; for Foreground: `pgrp = Pgid or getpid()`, `ioctl(Ctty, TIOCSPGRP, &pgrp)`.
4. Restore the signal mask (`rt_sigprocmask(SIG_SETMASK, &old)`).
5. `unshare(Unshareflags)`; with `CLONE_NEWUSER` there, the child writes its own `setgroups`/`gid_map`/
   `uid_map` through `openat`/`write`/`close`; with `CLONE_NEWNS`, `mount("none", "/", NULL, MS_REC|MS_PRIVATE)`.
6. `chroot(Chroot)`.
7. Credential: `setgroups` (unless NoSetGroups), `setgid`, `setuid`.
8. AmbientCaps: `capget`, add each cap to permitted and inheritable, `capset`, then
   `prctl(PR_CAP_AMBIENT, PR_CAP_AMBIENT_RAISE, c)` per cap.
9. `chdir(Dir)`.
10. Pdeathsig: `prctl(PR_SET_PDEATHSIG, sig)`; if `getppid() != ppid`, `kill(getpid(), sig)`.
11. The fd plan in Go's two passes (`dup3` up past `nextfd` with O_CLOEXEC, then `dup3` down or
    `fcntl(F_SETFD, 0)`), then `close` of the unused low fds. The plan is computed parent-side, as today.
12. `ioctl(0, TIOCNOTTY)` for Noctty; `ioctl(Ctty, TIOCSCTTY, 1)` for Setctty.
13. The rlimit restore: `prlimit64(0, RLIMIT_NOFILE, NULL, &lim)`; if it failed or reads `(max-1, max)`,
    `prlimit64(0, RLIMIT_NOFILE, &orig, NULL)`.
14. `ptrace(PTRACE_TRACEME)` for Ptrace.
15. `execve(argv0, argv, envv)`.

On a failing step the child writes the errno to the status pipe and calls `exit(253)`, Go's `childerror`.
Steps Go runs without checking (the low-fd closes, the second `prlimit64`) carry an ignore flag.

**Step record.** Fixed size: the syscall number, six arguments, and three small fields:
- each argument is an immediate or a reference to a scratch cell (Foreground's `pgrp`, Pdeathsig's `pid`);
- an optional result cell, which receives the syscall's return value (`getpid`, `getppid`, `openat`);
- an optional guard, which runs the step only if a cell, or a 64-bit word at an address the parent
  prepared, compares EQ or NE to a value (the `err2`, `getppid` and rlimit tests);
- the failure policy: report-and-exit, or ignore.

Everything a step points at (paths, argv, envp, the group list, the cap header and data, the rlimit
structs, the map text) is in unmanaged memory the parent allocated, freed after the status read, as the
seam already handles its posix_spawn buffers.

### 3.4 The stepper

A fixed routine of machine code per architecture, carried as data in the hand-own, called from managed code
through an unmanaged function pointer. Its contract:

- **Entry:** `long step_spawn(const clone_args *ca, size_t ca_size, const step *steps, size_t count,
  long *cells, void *child_stack_top, int status_fd)`.
- **Clone:** `clone3(ca, ca_size)` when the request needs it (`UseCgroupFD`, `CLONE_NEWTIME`, the same
  test Go makes), else `clone(flags, child_stack_top, &pidfd, ...)` with the arguments in each
  architecture's order. In the parent it returns the pid, or `-errno` if the clone failed. No other
  parent-side effect.
- **Child:** switches to `child_stack_top`, walks the steps (guard, arguments, `syscall`, result cell,
  failure policy) and never returns: it ends in `execve` or in the error path's `write` and `exit`.
- **Touches:** only its own registers, the child stack, the step array, the cells and the status fd. No
  libc, no TLS, no signal handlers, no managed memory.
- **Size:** about 100-150 bytes per architecture; x86-64 and arm64 in the first cut.

**v1 uses `clone` without `CLONE_VM`** (copy-on-write fork). It is sound with no further care: the
child cannot write the parent's memory. Go uses `CLONE_VFORK|CLONE_VM` when no user namespace is
requested; that optimization is OQ-1, after the fork cost of a large CLR process is measured.

### 3.5 Carrying and mapping the code

- Each blob is a `ReadOnlySpan<byte>` with its assembly listing beside every instruction in a comment, one
  file per architecture, hand-owned.
- Mapped once per process on first use: `mmap(PROT_READ|PROT_WRITE)`, copy, `mprotect(PROT_READ|PROT_EXEC)`.
  Never writable and executable at once.
- No toolchain at build or at run time. An assembler is a REVIEW tool only (§4).

### 3.6 Where the stepper is unavailable

An architecture without a blob (arm, 386, riscv64, ppc64le, s390x, loong64), or a host that refuses the
executable mapping, keeps today's named ENOTSUP. The tests then skip exactly as they do now. Darwin is out
of scope: it has no Cloneflags, and its Credential and Ptrace refusals are a separate seat.

## 4. Adversarial review and the gates

- **The blobs are the risk surface.** Review plan for G: assemble each listing with a standard assembler
  (`as`, `llvm-mc`) and diff the bytes against the committed span; read the listing against the contract
  in §3.4; run the child under `strace -f` and compare its syscall trace with Go's for the same
  `SysProcAttr` (a converted program and its Go original, side by side).
- **The builder is managed and testable anywhere.** A managed reference interpreter of the step list, used
  only in tests, checks the builder against Go's sequence for every field combination the population uses
  (GolibTests or the syscall row's hand-owned tests), on any host.
- **Red first.** `syscall`'s `TestCloneNEWUSERAndRemap` and `crypto/x509`'s `TestSetFallbackRoots` read
  Go=pass/C#=skip at the base and matched at the cut, on a host that permits user namespaces.
- **Planted failures.** A step that must fail returns Go's errno through the status pipe (a `Dir` that does
  not exist gives ENOENT, as Go); a planted unprivileged `setuid` gives EPERM, and the consuming test skips
  as Go's does.
- **A kernel that refuses.** On a host with user namespaces disabled the tests must skip on both sides, as
  they do today; the row's next bank states the host context.
- **The manifests.** The disclosures this retires (`syscall`'s fifteen seam entries, `crypto/x509`'s nine,
  `os/exec`'s `TestCredentialNoSetGroups` on linux) are removed in the same seat, row by row, as each reads
  matched; an entry that can no longer match is not left behind.
- **Banked rows.** Nothing that spawns without a refused field changes path; the posix_spawn rows are
  re-run once to show it.

## 5. Open questions, with recommendations

- **OQ-1 — `CLONE_VFORK|CLONE_VM` when no user namespace is requested (Go's choice).** Recommend: not in
  v1. Measure the copy-on-write fork of a large CLR process first; adopt it only if that cost matters,
  since a vfork child shares the parent's memory and every step becomes a review question.
- **OQ-2 — one path for every spawn, as Go has.** Recommend: no. posix_spawn is ratified and banked, and
  routing everything through the stepper would move every spawning row for no verdict gained.
- **OQ-3 — hosts that refuse executable mappings.** Recommend: named ENOTSUP, as §3.6, with the refusal
  stated in the message so a skip there is attributable.
- **OQ-4 — architectures beyond x86-64 and arm64.** Recommend: none in v1; each needs its own blob and
  its own review.
- **OQ-5 — where the hand-own lives.** Recommend: beside the seam in `src/core/syscall/linux/`, one file for
  the builder and one per architecture for the blobs, so `exec_unix.cs` gains only the dispatch.

— P2 (proposed 2026-10-20)
