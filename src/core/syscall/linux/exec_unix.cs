// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
// Fork, exec, wait, etc.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted exec_unix.go output).
// Everything in this file except forkExec and Exec is the converted output verbatim — ForkLock
// and its acquire/release discipline, the ProcAttr/SysProcAttr shapes, argument validation and
// StartProcess are pure Go logic that converts faithfully. TWO functions cannot work as literally
// converted, for the same underlying reason — what the managed heap means to the kernel — and each
// carries its own block at its own site. Exec (2026-09-02) handed execve MANAGED argv/envp and
// produced a fork bomb; it now marshals into unmanaged memory like everything else here. forkExec
// is the older and larger of the two: its child half (exec_linux.cs's forkAndExecInChild, now
// unreachable dead code) runs
// MANAGED code between clone() and execve(), and no managed instruction is async-signal-safe in a
// multithreaded CLR process — the impossibility is by rule, not by measurement
// (docs/phase4/DESIGN-linux-exec.md §2, ratified 2026-08-22 with all seven OQs).
//
// The replacement maps the spawn onto posix_spawn(3), the one primitive whose child side is
// someone else's sound native code: Go's child-side fd shuffle is computed PARENT-side as a
// posix_spawn_file_actions list (the same shift-up-then-dup2 plan Go runs as code, expressed as
// data), pgid/sid ride posix_spawnattr, the child signal mask is reset to empty (exec itself
// resets caught handlers to default, so SETSIGDEF is not needed for the exec'd image), and
// glibc's synchronous error reporting subsumes Go's status-pipe protocol — spawn and exec
// failures return as errno from the call, so forkExecPipe/readlen and the child-status dance are
// simply gone. SysProcAttr fields outside the mapped set fail with a NAMED error (§3's honest
// wall), never a silent drop. SysProcAttr.PidFD is filled post-spawn via pidfd_open(pid) — sound
// here because the child cannot be reaped before this process's own first wait, which is the only
// reaper (OQ-4's v2 door, opened early because the r53a keystone made the syscall one line).
// Every buffer handed to the native call (argv, envp, the file_actions and attr blocks, the
// sigset) lives in UNMANAGED memory for the duration of the call and is freed in a finally —
// the exec_windows.cs soundness rule verbatim.

// Hand-owned native replacement of the converted exec_unix.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

using errorspkg = errors_package;
using bytealg = @internal.bytealg_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using @unsafe = unsafe_package;
using @internal;
using go.sync;

partial class syscall_package {

// ForkLock is used to synchronize creation of new file descriptors
// with fork.
//
// We want the child in a fork/exec sequence to inherit only the
// file descriptors we intend. To do that, we mark all file
// descriptors close-on-exec and then, in the child, explicitly
// unmark the ones we want the exec'ed program to keep.
// Unix doesn't make this easy: there is, in general, no way to
// allocate a new file descriptor close-on-exec. Instead you
// have to allocate the descriptor and then mark it close-on-exec.
// If a fork happens between those two events, the child's exec
// will inherit an unwanted file descriptor.
//
// This lock solves that race: the create new fd/mark close-on-exec
// operation is done holding ForkLock for reading, and the fork itself
// is done holding ForkLock for writing. At least, that's the idea.
// There are some complications.
//
// Some system calls that create new file descriptors can block
// for arbitrarily long times: open on a hung NFS server or named
// pipe, accept on a socket, and so on. We can't reasonably grab
// the lock across those operations.
//
// It is worse to inherit some file descriptors than others.
// If a non-malicious child accidentally inherits an open ordinary file,
// that's not a big deal. On the other hand, if a long-lived child
// accidentally inherits the write end of a pipe, then the reader
// of that pipe will not see EOF until that child exits, potentially
// causing the parent program to hang. This is a common problem
// in threaded C programs that use popen.
//
// Luckily, the file descriptors that are most important not to
// inherit are not the ones that can take an arbitrarily long time
// to create: pipe returns instantly, and the net package uses
// non-blocking I/O to accept on a listening socket.
// The rules for which file descriptor-creating operations use the
// ForkLock are as follows:
//
//   - [Pipe]. Use pipe2 if available. Otherwise, does not block,
//     so use ForkLock.
//   - [Socket]. Use SOCK_CLOEXEC if available. Otherwise, does not
//     block, so use ForkLock.
//   - [Open]. Use [O_CLOEXEC] if available. Otherwise, may block,
//     so live with the race.
//   - [Dup]. Use [F_DUPFD_CLOEXEC] or dup3 if available. Otherwise,
//     does not block, so use ForkLock.
public static ж<Δsync.RWMutex> ᏑForkLock = new StandardBox<Δsync.RWMutex>(default(Δsync.RWMutex));
public static ref Δsync.RWMutex ForkLock => ref ᏑForkLock.Value;

// StringSlicePtr converts a slice of strings to a slice of pointers
// to NUL-terminated byte arrays. If any string contains a NUL byte
// this function panics instead of returning an error.
//
// Deprecated: Use [SlicePtrFromStrings] instead.
public static slice<ж<byte>> StringSlicePtr(slice<@string> ss) {
    var bb = new slice<ж<byte>>(len(ss) + 1);
    for (nint i = 0; i < len(ss); i++) {
        bb[i] = StringBytePtr(ss[i]);
    }
    bb[len(ss)] = default!;
    return bb;
}

// SlicePtrFromStrings converts a slice of strings to a slice of
// pointers to NUL-terminated byte arrays. If any string contains
// a NUL byte, it returns (nil, [EINVAL]).
public static (slice<ж<byte>>, error) SlicePtrFromStrings(slice<@string> ss) {
    nint n = 0;
    foreach (var (_, s) in ss) {
        if (bytealg.IndexByteString(s, 0) != -1) {
            return (default!, EINVAL);
        }
        n += len(s) + 1; // +1 for NUL
    }
    var bb = new slice<ж<byte>>(len(ss) + 1);
    var b = new slice<byte>(n);
    n = 0;
    foreach (var (i, s) in ss) {
        bb[i] = Ꮡ(b, n);
        copy(b[(int)(n)..], s);
        n += len(s) + 1;
    }
    return (bb, default!);
}

public static void CloseOnExec(nint fd) {
    fcntl(fd, F_SETFD, FD_CLOEXEC);
}

public static error /*err*/ SetNonblock(nint fd, bool nonblocking) {
    error err = default!;

    (var flag, err) = fcntl(fd, F_GETFL, 0);
    if (err != default!) {
        return err;
    }
    if (((nint)(flag & (nint)O_NONBLOCK) != 0) == nonblocking) {
        return default!;
    }
    if (nonblocking){
        flag |= (nint)(O_NONBLOCK);
    } else {
        flag &= ~(nint)(O_NONBLOCK);
    }
    (_, err) = fcntl(fd, F_SETFL, flag);
    return err;
}

// Credential holds user and group identities to be assumed
// by a child process started by [StartProcess].
[GoType] partial struct Credential {
    public uint32 Uid;   // User ID.
    public uint32 Gid;   // Group ID.
    public slice<uint32> Groups; // Supplementary group IDs.
    public bool NoSetGroups;     // If true, don't set supplementary groups
}

// ProcAttr holds attributes that will be applied to a new process started
// by [StartProcess].
[GoType] partial struct ProcAttr {
    public @string Dir;   // Current working directory.
    public slice<@string> Env; // Environment.
    public slice<uintptr> Files; // File descriptors.
    public ж<SysProcAttr> Sys;
}

internal static ж<ProcAttr> ᏑzeroProcAttr = new StandardBox<ProcAttr>(default(ProcAttr));
internal static ref ProcAttr zeroProcAttr => ref ᏑzeroProcAttr.Value;

internal static ж<SysProcAttr> ᏑzeroSysProcAttr = new StandardBox<SysProcAttr>(default(SysProcAttr));
internal static ref SysProcAttr zeroSysProcAttr => ref ᏑzeroSysProcAttr.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bothSetcttyAndForegroundˢ = "both Setctty and Foreground set in SysProcAttr"u8;
internal static readonly @string setcttySetButCttyNotˢ = "Setctty set but Ctty not valid in child"u8;

internal static (nint pid, error err) forkExec(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    ref var attr = ref Ꮡattr.DerefOrNull();
    if (Ꮡattr == nil) {
        Ꮡattr = ᏑzeroProcAttr; attr = ref Ꮡattr.DerefOrNull();
    }
    var sys = attr.Sys;
    if (sys == nil) {
        sys = ᏑzeroSysProcAttr;
    }

    // Go's own validation, preserved verbatim ahead of the spawn.
    if ((~sys).Setctty && (~sys).Foreground) {
        return (0, errorspkg.New(bothSetcttyAndForegroundˢ));
    }
    if ((~sys).Setctty && (~sys).Ctty >= len(attr.Files)) {
        return (0, errorspkg.New(setcttySetButCttyNotˢ));
    }

    // The honest wall (design §3): every SysProcAttr field posix_spawn cannot express fails by
    // NAME before anything is spawned — never a silent drop of a requested semantic. §3.3 also
    // fixes the error's KIND: "returns ENOTSUP naming the field ... in Go's own error currency
    // (reach Go's own gate, fail Go's own way)". Both halves are load-bearing, and the kind is
    // the half that is easy to lose — a named-but-kindless error reads correct to a human and is
    // invisible to Go's own predicates.
    //
    // Joining ENOTSUP is what supplies the currency: errors.Is walks the joined chain and Errno.Is
    // maps ENOTSUP/ENOSYS/EOPNOTSUPP onto errors.ErrUnsupported, which is one of the three things
    // testenv.SyscallIsNotSupported accepts (the others being an Errno of EPERM/EROFS/EINVAL and
    // fs.ErrPermission). Eight tests in Go's own syscall suite ride on that predicate: on an
    // unprivileged host Go attempts the operation, the kernel answers EPERM, the guard fires and
    // Go SKIPS. Without the kind those eight FAIL here instead — the same regression the sibling
    // hand-own already fixed for runtime_doAllThreadsSyscall, where a throwing stub "turned that
    // skip into an infrastructure-error" until ENOTSUP restored it (syscall_linux_impl.cs).
    //
    // errors.Join rather than a wrapping type because syscall cannot import fmt (Go's own rule —
    // only its _test.go files do), so fmt.Errorf("%w") is unavailable here, and a new error type
    // would owe GoType/witness machinery for a two-line refusal. Join needs nothing this file does
    // not already import, and keeps the field name first in the rendered message.
    {
        @string unsupported = unsupportedSysProcAttrField(ref (sys).DerefOrNull());
        if (unsupported != ""u8) {
            return (0, errorspkg.Join(errorspkg.New(unsupported), ENOTSUP));
        }
    }

    return posixSpawnForkExec(argv0, argv, ref attr, ref (sys).DerefOrNull());
}

// Combination of fork and exec, careful to be thread safe.
public static (nint pid, error err) ForkExec(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    return forkExec(argv0, argv, Ꮡattr);
}

// StartProcess wraps [ForkExec] for package os.
public static (nint pid, uintptr handle, error err) StartProcess(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    nint pid = default!;
    error err = default!;

    (pid, err) = forkExec(argv0, argv, Ꮡattr);
    return (pid, 0, err);
}

// The Exec bracket's two runtime pulls, given the empty bodies the managed model owes them. HELD
// once (2026-09-02) and released only after the prerequisite landed -- the history is kept below
// because the ORDER was the whole lesson: these bodies are sound on their own and were still
// unsafe to ship, because they unmask what runs after them.
//
// exec_unix.go declares the pair as //go:linkname pulls, so they emit bodyless and
// PartialStubGenerator supplies a throwing stub. That makes syscall.Exec die before the kernel,
// and Go's TestExec (whose helper child calls Exec) reports it as one clean infrastructure-error.
//
// The obvious fix is an empty body, and the ARGUMENT for it is sound: Go's bodies
// (runtime/proc.go:4992, :5008) are execLock.lock()/unlock() plus a darwin-only preempt drain, and
// execLock's only two readers are newosproc (proc.go:2839/2844 -- Go's own thread creation, absent
// here since threads come from the CLR, which never consults runtime_package, so the write lock
// could not serialize against them) and preemptM (signal_unix.go:372/389, guarded
// `GOOS == "darwin" || "ios"`, so unreachable on linux -- and the same guard makes the drain
// unreachable twice over). Both are func(); nothing is computed. Exec then replaces the image, so
// no bookkeeping survives to be inconsistent.
//
// WHAT THE MEASUREMENT SAID ANYWAY. With the bodies in place the syscall row does not improve --
// it FORK BOMBS. 96 syscall.tests processes in ~7 minutes, each a child of the last, growing about
// one per 3 s. The chain of children is itself the proof that execve did NOT replace the image
// (execve keeps the pid). Three sampled generations had GARBAGE /proc/<pid>/cmdline and an EMPTY
// /proc/<pid>/environ; a fourth was an ordinary spawn from TestDeathSignal, i.e. unfiltered suites
// were running. The run filter is not at fault -- positive control: the host honors
// `-test.run=^TestZeroSysProcAttr$` and runs that test alone.
//
// The reading (INFERENCE, stated as one -- the mechanism is not yet proven): Exec hands execve
// MANAGED memory. It builds argv0p/argvp/envvp with BytePtrFromString/SlicePtrFromStrings and
// passes `(uintptr)@unsafe.Pointer.FromRef(ref (Ꮡ(argvp, 0)).Value)` -- a **byte into the managed
// heap. The exec'd image therefore comes up with a corrupted argv and environ, loses
// `-test.run=^TestExecHelper$` and GO_WANT_HELPER_PROCESS, runs the WHOLE suite including TestExec,
// and spawns the next generation. This is the open "wrapper passes managed memory by address"
// class the project already tracks, met through a new door.
//
// So the throwing stub WAS, accidentally, the recursion brake -- and one honest
// infrastructure-error is strictly better than a fork bomb on every host that sweeps this row. The
// bodies were withheld until Exec marshalled argv/envp into UNMANAGED memory.
//
// THAT PREREQUISITE LANDED -- see Exec's own block above -- and it landed as its OWN commit measured
// on its OWN pair: with the stub still braking, the row came back 55 rows / 37 agreeing / 13
// disclosed / 5 errors, identical to the baseline, TestExec still failing with the same stack at the
// same stub. A fix that changes nothing visible is exactly what that commit should produce, and
// proving it changed nothing is why it was not folded into this one.
//
// WHY NOT FORWARD to runtime's real body, since runtime/linux/proc.cs:4983 carries one: the
// converter emits a linkname target public only for linknameForwardTargets rows
// (visitFuncDecl.go), and this pair is not one, so the body is internal and unreachable across the
// assembly boundary -- a CONVERTER change, not a corpus one. (The direction would be fine:
// syscall -> runtime is an edge Go's imports already carry, so no W1-style graph cycle.) It would
// also not help here: forwarding runs execLock.lock() and still reaches the same execve.
//
// Implemented in runtime package.
internal static partial void runtime_BeforeExec();

internal static partial void runtime_AfterExec();

// The bodies the block above argues for. Empty is the whole implementation: both are func(),
// nothing is computed, and the runtime state they would guard does not exist in this model.
internal static partial void runtime_BeforeExec() {
}

internal static partial void runtime_AfterExec() {
}

// execveLibc is non-nil on OS using libc syscall, set to execve in exec_libc.go; this
// avoids a build dependency for other platforms.
internal static Func<uintptr, uintptr, uintptr, Errno> execveLibc;

internal static Func<ж<byte>, ж<ж<byte>>, ж<ж<byte>>, error> execveDarwin;

internal static Func<ж<byte>, ж<ж<byte>>, ж<ж<byte>>, error> execveOpenBSD;

// Exec invokes the execve(2) system call.
// go2cs NATIVE half (hand-owned; see the block below). The converted body was Go's verbatim,
// and on this runtime that body cannot work: it handed execve MANAGED memory.
//
// WHAT IT COST, measured 2026-09-02. Go's exec_unix.go builds argv0p/argvp/envvp with
// BytePtrFromString/SlicePtrFromStrings -- in Go, pointers into pinned-by-construction storage --
// and the converted output passed them straight through as
// `(uintptr)@unsafe.Pointer.FromRef(ref (Ꮡ(argvp, 0)).Value)`, i.e. a **byte into the managed
// heap. execve then read whatever those addresses meant to the KERNEL, which is not what they mean
// to the CLR. The exec'd image came up with a corrupted argv and environ: /proc/<pid>/cmdline was
// garbage and /proc/<pid>/environ was EMPTY on every sampled generation. Losing argv loses
// `-test.run`, and losing environ loses GO_WANT_HELPER_PROCESS, so Go's own TestExec helper re-ran
// the WHOLE suite -- including TestExec, which spawns -- and the row FORK BOMBED: 96 processes in
// ~7 minutes, each a child of the last. (That chain is itself the proof execve never replaced the
// image, since execve keeps the pid.) The run filter was exonerated by positive control: the host
// honors -test.run and runs a single named test alone.
//
// This is the project's open "wrapper hands managed memory to a native call by address" class,
// reached through a Linux door. The remedy is the rule this file's own header already states for
// the posix_spawn seam -- every buffer handed to a native call lives in UNMANAGED memory for the
// duration of the call and is freed in a finally -- served by the same three helpers the seam uses.
//
// WHAT IS PRESERVED VERBATIM. Go returns EINVAL when any argument contains an embedded NUL, and
// that contract is observable, so the BytePtrFromString/SlicePtrFromStrings calls STAY -- they are
// kept purely as the validators they also are, their pointers discarded. The rlimit restore and the
// BeforeExec/AfterExec bracket keep their exact positions. The non-linux execve branches
// (solaris/illumos/aix, darwin/ios, openbsd) are dropped rather than carried: this file is
// linux/exec_unix.cs under layout L3, so Δruntime.GOOS is "linux" and every one of them was dead
// code guarded by a constant. Dropping them is what lets the unmanaged buffers be typed IntPtr
// instead of being laundered back through ж<byte> for callees that cannot run here.
public static error /*err*/ Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    error err = default!;

    // Validation only -- Go's EINVAL-on-embedded-NUL contract. The pointers these produce are
    // deliberately discarded; the buffers execve actually receives are the unmanaged ones below.
    (_, err) = BytePtrFromString(argv0);
    if (err != default!) {
        return err;
    }
    (_, err) = SlicePtrFromStrings(argv);
    if (err != default!) {
        return err;
    }
    (_, err) = SlicePtrFromStrings(envv);
    if (err != default!) {
        return err;
    }

    IntPtr argv0ʋ = IntPtr.Zero;
    IntPtr argvʋ = IntPtr.Zero;
    IntPtr envvʋ = IntPtr.Zero;

    try {
        argv0ʋ = MarshalStringZ(argv0);
        argvʋ = MarshalStringVector(argv);
        envvʋ = MarshalStringVector(envv);

        runtime_BeforeExec();
        var rlim = ᏑorigRlimitNofile.Load();
        if (rlim != nil) {
            Setrlimit(RLIMIT_NOFILE, rlim);
        }
        var (_, _, err1) = RawSyscall(SYS_EXECVE, (uintptr)argv0ʋ, (uintptr)argvʋ, (uintptr)envvʋ);
        // Reached only when execve FAILED -- on success this process is already gone, and the
        // finally below never runs, which is correct: the image that owned the memory is gone too.
        runtime_AfterExec();
        return err1;
    }
    finally {
        FreeStringVector(envvʋ);
        FreeStringVector(argvʋ);
        if (argv0ʋ != IntPtr.Zero) {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(argv0ʋ);
        }
    }
}

// ============================== the posix_spawn seam ==============================
// Everything below is the hand-own's native half: the spawn body, its named-wall triage, and the
// libc surface it stands on. See the file header for the design pointer and the soundness rules.

// unsupportedSysProcAttrField names the first SysProcAttr request posix_spawn cannot express, or
// returns "" when the whole request is expressible. Each branch is a REQUESTED semantic — failing
// it loudly is the design's honest wall; dropping it silently would be a wrong program.
internal static @string unsupportedSysProcAttrField(ref SysProcAttr sys) {
    if (sys.Chroot != ""u8) {
        return "posix_spawn seam: SysProcAttr.Chroot is not supported"u8;
    }
    if (sys.Credential != nil) {
        return "posix_spawn seam: SysProcAttr.Credential is not supported"u8;
    }
    if (sys.Ptrace) {
        return "posix_spawn seam: SysProcAttr.Ptrace is not supported"u8;
    }
    if (sys.Setctty) {
        return "posix_spawn seam: SysProcAttr.Setctty is not supported"u8;
    }
    if (sys.Noctty) {
        return "posix_spawn seam: SysProcAttr.Noctty is not supported"u8;
    }
    if (sys.Pdeathsig != 0) {
        return "posix_spawn seam: SysProcAttr.Pdeathsig is not supported"u8;
    }
    if (sys.Cloneflags != 0) {
        return "posix_spawn seam: SysProcAttr.Cloneflags is not supported"u8;
    }
    if (sys.Unshareflags != 0) {
        return "posix_spawn seam: SysProcAttr.Unshareflags is not supported"u8;
    }
    if (len(sys.UidMappings) > 0 || len(sys.GidMappings) > 0 || sys.GidMappingsEnableSetgroups) {
        return "posix_spawn seam: SysProcAttr user-namespace ID mappings are not supported"u8;
    }
    if (len(sys.AmbientCaps) > 0) {
        return "posix_spawn seam: SysProcAttr.AmbientCaps is not supported"u8;
    }
    if (sys.UseCgroupFD) {
        return "posix_spawn seam: SysProcAttr.UseCgroupFD is not supported"u8;
    }
    return ""u8;
}

internal static (nint pid, error err) posixSpawnForkExec(@string argv0, slice<@string> argv, ref ProcAttr attr, ref SysProcAttr sys) {
    // glibc's opaque control blocks, driven only through their init/destroy/add functions —
    // never by layout knowledge. The allocations are generous over the real glibc sizes
    // (file_actions 80, spawnattr 336, sigset_t 128 on linux-x64).
    IntPtr fileActions = IntPtr.Zero;
    IntPtr spawnAttr = IntPtr.Zero;
    IntPtr sigsetEmpty = IntPtr.Zero;
    IntPtr pathz = IntPtr.Zero;
    IntPtr dirz = IntPtr.Zero;
    IntPtr argvVec = IntPtr.Zero;
    IntPtr envVec = IntPtr.Zero;

    try {
        pathz = MarshalStringZ(argv0);
        argvVec = MarshalStringVector(argv);
        envVec = MarshalStringVector(attr.Env);

        fileActions = System.Runtime.InteropServices.Marshal.AllocHGlobal(128);
        int rc = posix_spawn_file_actions_init(fileActions);
        if (rc != 0) {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(fileActions);
            fileActions = IntPtr.Zero;
            return (0, (Errno)(uintptr)rc);
        }

        spawnAttr = System.Runtime.InteropServices.Marshal.AllocHGlobal(512);
        rc = posix_spawnattr_init(spawnAttr);
        if (rc != 0) {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(spawnAttr);
            spawnAttr = IntPtr.Zero;
            return (0, (Errno)(uintptr)rc);
        }

        // The child's working directory rides an action so it happens child-side, in order,
        // before exec — glibc ≥ 2.29; probed by call per the design's OQ-5 (never by version
        // string), with the miss surfacing as a NAMED error.
        if (attr.Dir != ""u8) {
            dirz = MarshalStringZ(attr.Dir);
            try {
                rc = posix_spawn_file_actions_addchdir_np(fileActions, dirz);
            }
            catch (EntryPointNotFoundException) {
                return (0, errorspkg.New("posix_spawn seam: ProcAttr.Dir needs posix_spawn_file_actions_addchdir_np (glibc 2.29+)"u8));
            }
            if (rc != 0) {
                return (0, (Errno)(uintptr)rc);
            }
        }

        // Go's child-side fd shuffle, expressed as data (design §3): pass 1 lifts any source that
        // sits inside the already-written target zone up to a scratch fd; pass 2 dup2s every
        // source into its child slot — adddup2(i,i) is POSIX's defined way to CLEAR close-on-exec
        // on an inherited fd; pass 3 closes the scratch fds. Everything else in the parent is
        // close-on-exec by ForkLock discipline, exactly as in Go. Child fds the caller did NOT
        // provide below the std triple are closed, matching Go's guarantee that a short Files
        // list yields CLOSED std descriptors, not inherited ones.
        nint childCount = len(attr.Files);
        nint scratchBase = childCount;
        for (nint fi = 0; fi < childCount; fi++) {
            nint parentFd = ((nint)attr.Files[fi]);
            if (parentFd >= scratchBase) {
                scratchBase = parentFd + 1;
            }
        }

        var sources = new nint[childCount];
        for (nint i = 0; i < childCount; i++) {
            sources[i] = ((nint)attr.Files[i]);
        }

        nint nextScratch = scratchBase;
        var scratches = new System.Collections.Generic.List<nint>();
        for (nint i = 0; i < childCount; i++) {
            if (sources[i] >= 0 && sources[i] < i) {
                rc = posix_spawn_file_actions_adddup2(fileActions, (int)sources[i], (int)nextScratch);
                if (rc != 0) {
                    return (0, (Errno)(uintptr)rc);
                }
                sources[i] = nextScratch;
                scratches.Add(nextScratch);
                nextScratch++;
            }
        }
        for (nint i = 0; i < childCount; i++) {
            if (sources[i] < 0) {
                continue;
            }
            rc = posix_spawn_file_actions_adddup2(fileActions, (int)sources[i], (int)i);
            if (rc != 0) {
                return (0, (Errno)(uintptr)rc);
            }
        }
        foreach (nint scratch in scratches) {
            rc = posix_spawn_file_actions_addclose(fileActions, (int)scratch);
            if (rc != 0) {
                return (0, (Errno)(uintptr)rc);
            }
        }
        for (nint i = childCount; i < 3; i++) {
            rc = posix_spawn_file_actions_addclose(fileActions, (int)i);
            if (rc != 0) {
                return (0, (Errno)(uintptr)rc);
            }
        }

        // Attributes: an EMPTY child signal mask (exec itself resets caught handlers to default,
        // so the mask is the only signal state that survives into the new image — an inherited
        // CLR mask must not leak into a Go child), plus the billed pgid/sid requests.
        sigsetEmpty = System.Runtime.InteropServices.Marshal.AllocHGlobal(128);
        sigemptyset(sigsetEmpty);
        short flags = POSIX_SPAWN_SETSIGMASK;
        rc = posix_spawnattr_setsigmask(spawnAttr, sigsetEmpty);
        if (rc != 0) {
            return (0, (Errno)(uintptr)rc);
        }
        if (sys.Setpgid || sys.Foreground) {
            flags |= POSIX_SPAWN_SETPGROUP;
            rc = posix_spawnattr_setpgroup(spawnAttr, (int)sys.Pgid);
            if (rc != 0) {
                return (0, (Errno)(uintptr)rc);
            }
        }
        if (sys.Setsid) {
            flags |= POSIX_SPAWN_SETSID;
        }
        rc = posix_spawnattr_setflags(spawnAttr, flags);
        if (rc != 0) {
            return (0, (Errno)(uintptr)rc);
        }

        // The spawn window itself keeps Go's ForkLock discipline: fds created elsewhere stay
        // close-on-exec-atomic relative to the child's inheritance snapshot.
        acquireForkLock();
        int childPid;
        try {
            rc = posix_spawn(out childPid, pathz, fileActions, spawnAttr, argvVec, envVec);
        }
        finally {
            releaseForkLock();
        }

        // glibc reports child-setup and exec failures synchronously in rc (design §5.1; the
        // implementation gate spawns a missing binary and asserts ENOENT arrives HERE) and reaps
        // any partially-created child itself — there is no zombie to wait for on this path.
        if (rc != 0) {
            return (0, (Errno)(uintptr)rc);
        }

        // The pidfd door, closed once and reopened by measurement — the full story, because both
        // wrong turns were paid for. os's ensurePidfd plants this field whenever ITS OWN kernel
        // probe passes, and getPidfd then uses the value UNCHECKED — Go's clone always delivers a
        // real fd on that path, so -1 here is not "unsupported", it is a poisoned handle:
        // strace showed `waitid(P_PIDFD, -1, …) = EINVAL` and every wait failing (exit codes -1).
        // The first attempt DID fill a real pidfd and read every child exit as 0 — which
        // implicated this fill, but the true root was SiginfoChild's converted layout (managed
        // array-reference padding shifting every kernel offset; see
        // internal/syscall/unix/linux/siginfo_linux.cs, now a blittable hand-own). With that
        // mirror fixed the fill is correct AND race-free: the child cannot be reaped before this
        // process's own first wait, which is the only reaper.
        // Foreground: place the child's process group in the terminal's foreground, from the PARENT.
        // Go's child performs ioctl(Ctty, TIOCSPGRP, &pgrp) between fork and exec with every signal
        // blocked (exec_linux.go: "Restore the signal mask. We do this after TIOCSPGRP to avoid having
        // the kernel send a SIGTTOU"); posix_spawn has no such action, so the design's section 3.3
        // mapping is SETPGROUP above plus this call after the spawn returns. The residual window --
        // the exec'd image can run before the transfer lands -- is stated rather than hidden; the
        // transfer itself is exact: the group is the child's own pid when Pgid is 0, as in Go.
        // SIGTTOU is blocked on THIS thread for the call: the kernel refuses tcsetpgrp from a
        // background caller with SIGTTOU deliverable (it stops the caller), and blocks it otherwise.
        if (sys.Foreground) {
            int pgrp = sys.Pgid != 0 ? (int)sys.Pgid : childPid;
            IntPtr pgrpBuf = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(int));
            IntPtr blockSet = System.Runtime.InteropServices.Marshal.AllocHGlobal(128);
            IntPtr savedSet = System.Runtime.InteropServices.Marshal.AllocHGlobal(128);
            try {
                System.Runtime.InteropServices.Marshal.WriteInt32(pgrpBuf, pgrp);
                sigemptyset(blockSet);
                sigaddset(blockSet, SIGTTOU_NUMBER);
                pthread_sigmask(SIG_BLOCK, blockSet, savedSet);
                int rcIoctl, errnoIoctl = 0;
                try {
                    rcIoctl = ioctl((int)sys.Ctty, TIOCSPGRP_REQUEST, pgrpBuf);
                    if (rcIoctl < 0) {
                        errnoIoctl = System.Runtime.InteropServices.Marshal.GetLastPInvokeError();
                    }
                }
                finally {
                    pthread_sigmask(SIG_SETMASK, savedSet, IntPtr.Zero);
                }
                if (rcIoctl < 0) {
                    // Go's child reports the ioctl failure as the spawn's error (childerror), and its
                    // PARENT then WAITS for that child before returning the error -- Wait4 in an EINTR
                    // retry loop, "to make sure the zombies don't accumulate" (exec_unix.go:234-239).
                    // The child HERE already exists and is past exec, so it is killed first and then
                    // reaped in the same loop.
                    //
                    // The comment this replaces CLAIMED that reap while the code only killed, which is
                    // the worse half of the defect: a comment asserting a behaviour the code does not
                    // have reads as the census to the next reader. The discarded child stayed a zombie
                    // for the life of the process and nothing else absorbed it -- no Process is ever
                    // built on this path, so the caller has no Wait to reach it. Narrow to reach (an
                    // ioctl failure under Foreground, e.g. a non-tty Ctty giving ENOTTY) and unreached
                    // by any roster row, which is why it survived review; found by C2 in the darwin
                    // twin of this seam, 2026-09-05.
                    //
                    // The successful-spawn argument in this file's header -- the caller's Wait is the
                    // only reaper, so the post-spawn pidfd_open is race-free -- is about the path that
                    // RETURNS a pid and stays true. This path returns none.
                    syscallʟ(SYS_kill, childPid, SIGKILL_NUMBER, 0);
                    error waitErr = default!;
                    while (ᐧ) {
                        ref var wstatus = ref heap(new WaitStatus(), out var Ꮡwstatus);
                        (_, waitErr) = Wait4((nint)childPid, Ꮡwstatus, 0, nil);
                        if (!AreEqual(waitErr, EINTR)) {
                            break;
                        }
                    }
                    return (0, (Errno)(uintptr)errnoIoctl);
                }
            }
            finally {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(pgrpBuf);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(blockSet);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(savedSet);
            }
        }

        if (sys.PidFD != nil) {
            long fdOrErr = syscallʟ(SYS_pidfd_open, childPid, 0, 0);
            sys.PidFD.Value = fdOrErr >= 0 ? ((nint)fdOrErr) : -1;
        }

        return (childPid, default!);
    }
    finally {
        if (sigsetEmpty != IntPtr.Zero) {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(sigsetEmpty);
        }
        if (spawnAttr != IntPtr.Zero) {
            posix_spawnattr_destroy(spawnAttr);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(spawnAttr);
        }
        if (fileActions != IntPtr.Zero) {
            posix_spawn_file_actions_destroy(fileActions);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(fileActions);
        }
        FreeStringVector(argvVec);
        FreeStringVector(envVec);
        if (dirz != IntPtr.Zero) {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(dirz);
        }
        if (pathz != IntPtr.Zero) {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pathz);
        }
    }
}

// MarshalStringZ copies a Go string into unmanaged memory as NUL-terminated UTF-8 bytes.
internal static IntPtr MarshalStringZ(@string value) {
    byte[] bytes = ((slice<byte>)value).ToArray();
    IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(bytes.Length + 1);
    System.Runtime.InteropServices.Marshal.Copy(bytes, 0, buffer, bytes.Length);
    System.Runtime.InteropServices.Marshal.WriteByte(buffer, bytes.Length, 0);
    return buffer;
}

// MarshalStringVector builds a NULL-terminated char** in unmanaged memory. A nil slice yields an
// empty vector — Go's own SlicePtrFromStrings semantics (an empty child environment, never an
// inherited one).
internal static IntPtr MarshalStringVector(slice<@string> values) {
    nint count = len(values);
    IntPtr vector = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)((count + 1) * IntPtr.Size));
    for (nint i = 0; i < count; i++) {
        System.Runtime.InteropServices.Marshal.WriteIntPtr(vector, (int)(i * IntPtr.Size), MarshalStringZ(values[i]));
    }
    System.Runtime.InteropServices.Marshal.WriteIntPtr(vector, (int)(count * IntPtr.Size), IntPtr.Zero);
    return vector;
}

internal static void FreeStringVector(IntPtr vector) {
    if (vector == IntPtr.Zero) {
        return;
    }
    for (int i = 0; ; i += IntPtr.Size) {
        IntPtr entry = System.Runtime.InteropServices.Marshal.ReadIntPtr(vector, i);
        if (entry == IntPtr.Zero) {
            break;
        }
        System.Runtime.InteropServices.Marshal.FreeHGlobal(entry);
    }
    System.Runtime.InteropServices.Marshal.FreeHGlobal(vector);
}

// glibc flag values (spawn.h) and the pidfd_open syscall number (linux-x64).
internal const short POSIX_SPAWN_SETSIGMASK = 0x08;
internal const short POSIX_SPAWN_SETPGROUP = 0x02;
internal const short POSIX_SPAWN_SETSID = 0x80;
internal const long SYS_pidfd_open = 434;

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawn(out int pid, IntPtr path, IntPtr fileActions, IntPtr attrp, IntPtr argv, IntPtr envp);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawn_file_actions_init(IntPtr fileActions);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawn_file_actions_destroy(IntPtr fileActions);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawn_file_actions_adddup2(IntPtr fileActions, int fd, int newFd);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawn_file_actions_addclose(IntPtr fileActions, int fd);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawn_file_actions_addchdir_np(IntPtr fileActions, IntPtr path);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawnattr_init(IntPtr attrp);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawnattr_destroy(IntPtr attrp);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawnattr_setflags(IntPtr attrp, short flags);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawnattr_setpgroup(IntPtr attrp, int pgroup);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int posix_spawnattr_setsigmask(IntPtr attrp, IntPtr sigmask);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int sigemptyset(IntPtr set);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int sigaddset(IntPtr set, int signum);

[System.Runtime.InteropServices.DllImport("libc", SetLastError = false)]
internal static extern int pthread_sigmask(int how, IntPtr set, IntPtr oldset);

// ioctl(2) itself rather than the raw syscall(2) wrapper: that wrapper returns -1 and SETS errno, it
// never returns -errno, so the errno must be read back through SetLastError.
[System.Runtime.InteropServices.DllImport("libc", SetLastError = true)]
internal static extern int ioctl(int fd, ulong request, IntPtr arg);

// Foreground's native vocabulary (linux/amd64; the seam already asserts the architecture elsewhere).
internal const int SIG_BLOCK = 0;
internal const int SIG_SETMASK = 2;
internal const int SIGTTOU_NUMBER = 22;
internal const int SIGKILL_NUMBER = 9;
internal const long SYS_kill = 62;
internal const ulong TIOCSPGRP_REQUEST = 0x5410;

[System.Runtime.InteropServices.DllImport("libc", EntryPoint = "syscall", SetLastError = false)]
internal static extern long syscallʟ(long number, long a1, long a2, long a3);

} // end syscall_package
