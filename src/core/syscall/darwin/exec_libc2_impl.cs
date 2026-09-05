// exec_libc2_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The darwin exec seam -- forkExec over posix_spawn(3), Exec over an unmanaged execve(2), and the
// pipe both stand on -- darwin increment 10 (b), 2026-09-05: the darwin companion of the linux
// hand-own syscall/linux/exec_unix.cs (whole-file, 2026-08-22 / 09-02), whose header carries the
// design of record (docs/phase4/DESIGN-linux-exec.md §2, §3, §5.1) and the measured history: a child
// half that runs MANAGED code between fork() and execve() is unsound by rule in a multithreaded CLR
// process, and an execve handed managed argv/envp comes up with garbage argv and an EMPTY environ
// (the fork-bomb class, 96 processes in 7 minutes). Darwin's auto conversion has both walls --
// exec_libc2.cs's forkAndExecInChild runs rawSyscall(libc_fork_trampoline) and then managed code in
// the child; exec_unix.cs's Exec hands execveDarwin the boxes SlicePtrFromStrings built -- and a
// third of its own in the pipe every capture stands on. The SigIgnoreDisposition probe reached the
// first of them the moment increment 10 (a) opened LookPath.
//
// THREE DISPLACEMENTS, all through manualConversionFuncs ("syscall": forkExec, Exec, pipe ->
// goosDarwin -- a BODIED converted function is displaced only through the registry), each body here:
//
//   forkExec -- posix_spawn, the linux seam's mapping verbatim where darwin's ABI agrees and stated
//               where it differs. posix_spawnattr_t and posix_spawn_file_actions_t are POINTER-sized
//               opaque handles on darwin (init allocates behind them; the over-sized blocks below
//               hold the handle, as the linux blocks hold glibc's structs); sigset_t is a 32-bit
//               word; the flag values are Apple's (sys/spawn.h: SETPGROUP 0x0002, SETSIGMASK 0x0008,
//               SETSID 0x0400); pthread_sigmask's how-values are 1/2/3 where linux has 0/1/2;
//               TIOCSPGRP is the package's own darwin constant (0x80047476); a failed Foreground
//               transfer kills the child through kill(2), darwin having no syscall(2)-by-number
//               keystone; and the chdir action is posix_spawn_file_actions_addchdir_np (macOS
//               10.15+), probed by CALL exactly as the linux seam probes glibc 2.29+. SysProcAttr
//               requests posix_spawn cannot express fail with a NAMED error joined to ENOTSUP --
//               Chroot, Credential, Ptrace, Setctty, Noctty -- the linux wall's shape on darwin's
//               field set (no Pdeathsig, Cloneflags, namespaces, caps or cgroups to refuse here).
//   Exec     -- the linux hand-own's marshalling body: BytePtrFromString / SlicePtrFromStrings kept
//               as the EINVAL-on-embedded-NUL validators they also are, their boxes discarded, and
//               execve(2) given unmanaged copies; the rlimit restore and the BeforeExec/AfterExec
//               bracket keep their positions. The converted body's solaris/darwin/openbsd branch
//               ladder is dead under layout L3 (this file is darwin/) and is not carried.
//               runtime_BeforeExec / runtime_AfterExec: the empty bodies the linux seam argues for
//               (both are func(), nothing is computed, and the runtime state they would guard does
//               not exist in this model).
//   pipe     -- the shape runtime's increment 4 fixed one package over: the converted wrapper hands
//               the keystone `(uintptr)Ꮡp` for a ж<array<int32>>, a box whose storage is a managed
//               Int32[] -- under Q44 a TOKEN, never an address -- so pipe(2) would write through
//               junk. Here an 8-byte native pair is handed to libc and copied back into the box.
//               os.Pipe (os/exec's stdout/stderr capture) and Go's own forkExecPipe stand on it.
//
// SOUNDNESS RULE, the linux seam's and exec_windows.cs's: every buffer a native call receives lives
// in UNMANAGED memory for the duration of the call and is freed in a finally.
//
// THE FILE NAME IS LOAD-BEARING: exec_libc2.cs is an emitted darwin-only principal (Go's
// exec_libc2.go is darwin/openbsd), so this companion is routed by the L3 merge to darwin/ alone.
// exec_unix.cs exists on both unix flavours, and a companion named after it would be copied into
// linux/ beside the whole-file hand-own there.

[module: go.GoManualConversion]

namespace go;

using System;
using System.Runtime.InteropServices;
using errorspkg = errors_package;
using @internal;
// atomic.Pointer<Rlimit>.Load() is an extension on ж<atomic.Pointer<T>> declared in go.sync;
// without this import the only Load in scope is sync_package's map overload and Exec's rlimit
// restore binds to it (CS7036). rlimit.cs and the principal exec_libc2.cs carry the same line.
using go.sync;

partial class syscall_package {

// ============================== forkExec ==============================

// forkExec -- Go's validation preserved verbatim ahead of the spawn (exec_unix.go), then the seam.
internal static (nint pid, error err) forkExec(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    ref var attr = ref Ꮡattr.DerefOrNull();
    if (Ꮡattr == nil) {
        Ꮡattr = ᏑzeroProcAttr; attr = ref Ꮡattr.DerefOrNull();
    }
    var sys = attr.Sys;
    if (sys == nil) {
        sys = ᏑzeroSysProcAttr;
    }

    // Go's EINVAL-on-embedded-NUL contract, observable and kept: the validators run, their boxes
    // are discarded, and the buffers posix_spawn receives are the unmanaged ones the seam builds.
    error err = default!;
    (_, err) = BytePtrFromString(argv0);
    if (err != default!) {
        return (0, err);
    }
    (_, err) = SlicePtrFromStrings(argv);
    if (err != default!) {
        return (0, err);
    }
    (_, err) = SlicePtrFromStrings(attr.Env);
    if (err != default!) {
        return (0, err);
    }
    if (attr.Dir != ""u8) {
        (_, err) = BytePtrFromString(attr.Dir);
        if (err != default!) {
            return (0, err);
        }
    }

    // Both Setctty and Foreground use the Ctty field, but they give it slightly different meanings.
    // The texts are spelled here rather than referenced: the converter hoists a body's string
    // literals WITH the body, so forkExec's `…ˢ` hoists cease to exist the moment its body is
    // displaced, and a hand-own that referenced one would dangle.
    if ((~sys).Setctty && (~sys).Foreground) {
        return (0, errorspkg.New("both Setctty and Foreground set in SysProcAttr"u8));
    }
    if ((~sys).Setctty && (~sys).Ctty >= len(attr.Files)) {
        return (0, errorspkg.New("Setctty set but Ctty not valid in child"u8));
    }

    // The honest wall (the linux seam's §3.3): every SysProcAttr field posix_spawn cannot express
    // fails by NAME before anything is spawned, joined to ENOTSUP so errors.Is reaches
    // errors.ErrUnsupported -- Go's own currency, the one testenv.SyscallIsNotSupported reads.
    {
        @string unsupported = unsupportedSysProcAttrField(ref (sys).DerefOrNull());
        if (unsupported != ""u8) {
            return (0, errorspkg.Join(errorspkg.New(unsupported), ENOTSUP));
        }
    }

    return posixSpawnForkExec(argv0, argv, ref attr, ref (sys).DerefOrNull());
}

// unsupportedSysProcAttrField names the first SysProcAttr request posix_spawn cannot express on
// darwin, or returns "" when the whole request is expressible. Each branch is a REQUESTED semantic
// -- failing it loudly is the design's honest wall; dropping it silently would be a wrong program.
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
    return ""u8;
}

internal static (nint pid, error err) posixSpawnForkExec(@string argv0, slice<@string> argv, ref ProcAttr attr, ref SysProcAttr sys) {
    // libSystem's opaque handles, driven only through their init/destroy/add functions -- never by
    // layout knowledge. On darwin each block holds a pointer the init call allocates behind; the
    // allocations are the linux seam's sizes, generous over the 8 bytes darwin needs.
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

        fileActions = Marshal.AllocHGlobal(128);
        int rc = posix_spawn_file_actions_init(fileActions);
        if (rc != 0) {
            Marshal.FreeHGlobal(fileActions);
            fileActions = IntPtr.Zero;
            return (0, (Errno)(uintptr)rc);
        }

        spawnAttr = Marshal.AllocHGlobal(512);
        rc = posix_spawnattr_init(spawnAttr);
        if (rc != 0) {
            Marshal.FreeHGlobal(spawnAttr);
            spawnAttr = IntPtr.Zero;
            return (0, (Errno)(uintptr)rc);
        }

        // The child's working directory rides an action so it happens child-side, in order, before
        // exec -- macOS 10.15+; probed by call, never by version string, the miss a NAMED error.
        if (attr.Dir != ""u8) {
            dirz = MarshalStringZ(attr.Dir);
            try {
                rc = posix_spawn_file_actions_addchdir_np(fileActions, dirz);
            }
            catch (EntryPointNotFoundException) {
                return (0, errorspkg.New("posix_spawn seam: ProcAttr.Dir needs posix_spawn_file_actions_addchdir_np (macOS 10.15+)"u8));
            }
            if (rc != 0) {
                return (0, (Errno)(uintptr)rc);
            }
        }

        // Go's child-side fd shuffle, expressed as data (the linux seam's §3): pass 1 lifts any
        // source that sits inside the already-written target zone up to a scratch fd; pass 2 dup2s
        // every source into its child slot -- adddup2(i,i) is POSIX's defined way to CLEAR
        // close-on-exec on an inherited fd; pass 3 closes the scratch fds. Everything else in the
        // parent is close-on-exec by ForkLock discipline, exactly as in Go. Child fds the caller did
        // NOT provide below the std triple are closed, matching Go's guarantee that a short Files
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

        // Attributes: an EMPTY child signal mask (exec itself resets caught handlers to default, so
        // the mask is the only signal state that survives into the new image -- an inherited CLR
        // mask must not leak into a Go child), plus the billed pgid/sid requests.
        sigsetEmpty = Marshal.AllocHGlobal(128);
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

        // libSystem reports child-setup and exec failures synchronously in rc (a missing binary is
        // ENOENT HERE) and reaps any partially-created child itself -- no zombie to wait for.
        if (rc != 0) {
            return (0, (Errno)(uintptr)rc);
        }

        // Foreground: place the child's process group in the terminal's foreground, from the PARENT.
        // Go's child performs ioctl(Ctty, TIOCSPGRP, &pgrp) between fork and exec with every signal
        // blocked; posix_spawn has no such action, so the mapping is SETPGROUP above plus this call
        // after the spawn returns. The residual window -- the exec'd image can run before the
        // transfer lands -- is stated rather than hidden; the transfer itself is exact: the group is
        // the child's own pid when Pgid is 0, as in Go. SIGTTOU is blocked on THIS thread for the
        // call: the kernel stops a background caller with SIGTTOU deliverable, and refuses it with
        // the signal blocked.
        if (sys.Foreground) {
            int pgrp = sys.Pgid != 0 ? (int)sys.Pgid : childPid;
            IntPtr pgrpBuf = Marshal.AllocHGlobal(sizeof(int));
            IntPtr blockSet = Marshal.AllocHGlobal(128);
            IntPtr savedSet = Marshal.AllocHGlobal(128);
            try {
                Marshal.WriteInt32(pgrpBuf, pgrp);
                sigemptyset(blockSet);
                sigaddset(blockSet, (int)SIGTTOU);
                pthread_sigmask(SIG_BLOCK, blockSet, savedSet);
                int rcIoctl, errnoIoctl = 0;
                try {
                    rcIoctl = ioctl((int)sys.Ctty, (ulong)TIOCSPGRP, pgrpBuf);
                    if (rcIoctl < 0) {
                        errnoIoctl = Marshal.GetLastPInvokeError();
                    }
                }
                finally {
                    pthread_sigmask(SIG_SETMASK, savedSet, IntPtr.Zero);
                }
                if (rcIoctl < 0) {
                    // Go's child reports the ioctl failure as the spawn's error (childerror); the
                    // child here already exists, so it is killed AND REAPED before the error is
                    // returned -- Go's own shape (exec_unix.go: "wait for it to exit, to make sure
                    // the zombies don't accumulate", retried over EINTR). The reap is load-bearing
                    // precisely HERE and nowhere else on this seam: forkExec returns an ERROR on
                    // this path, so os/exec never builds a Process and nothing ever waits. The
                    // header's "this process's own first wait is the only reaper" is true of a
                    // child that SUCCEEDS; it cannot absorb one discarded here. The status pointer
                    // is NULL rather than Go's &wstatus because Go discards it on this path too,
                    // and NULL keeps the call clear of the box-address question the pipe body
                    // above exists for.
                    //
                    // DIVERGENCE from the linux twin, stated rather than left to be found:
                    // syscall/linux/exec_unix.cs -- the design of record -- kills without reaping
                    // while carrying the same "as Go's parent reaps" comment. That half is its
                    // owner's to route and is not touched from here; this seam diverges toward Go.
                    kill(childPid, (int)SIGKILL);
                    var (_, werr) = wait4(childPid, nil, 0, nil);
                    while (AreEqual(werr, EINTR)) {
                        (_, werr) = wait4(childPid, nil, 0, nil);
                    }
                    return (0, (Errno)(uintptr)errnoIoctl);
                }
            }
            finally {
                Marshal.FreeHGlobal(pgrpBuf);
                Marshal.FreeHGlobal(blockSet);
                Marshal.FreeHGlobal(savedSet);
            }
        }

        return (childPid, default!);
    }
    finally {
        if (sigsetEmpty != IntPtr.Zero) {
            Marshal.FreeHGlobal(sigsetEmpty);
        }
        if (spawnAttr != IntPtr.Zero) {
            posix_spawnattr_destroy(spawnAttr);
            Marshal.FreeHGlobal(spawnAttr);
        }
        if (fileActions != IntPtr.Zero) {
            posix_spawn_file_actions_destroy(fileActions);
            Marshal.FreeHGlobal(fileActions);
        }
        FreeStringVector(argvVec);
        FreeStringVector(envVec);
        if (dirz != IntPtr.Zero) {
            Marshal.FreeHGlobal(dirz);
        }
        if (pathz != IntPtr.Zero) {
            Marshal.FreeHGlobal(pathz);
        }
    }
}

// ============================== Exec ==============================

// The bodies the linux seam argues for. Empty is the whole implementation: both are func(), nothing
// is computed, and the runtime state they would guard does not exist in this model.
internal static partial void runtime_BeforeExec() {
}

internal static partial void runtime_AfterExec() {
}

// Exec invokes the execve(2) system call -- the linux hand-own's marshalling body, one flavour over.
public static error /*err*/ Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    error err = default!;

    // Validation only -- Go's EINVAL-on-embedded-NUL contract. The boxes these produce are
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
        int rc = execve(argv0ʋ, argvʋ, envvʋ);
        // Reached only when execve FAILED -- on success this process is already gone, and the
        // finally below never runs, which is correct: the image that owned the memory is gone too.
        Errno err1 = rc < 0 ? (Errno)(uintptr)Marshal.GetLastPInvokeError() : (Errno)0;
        runtime_AfterExec();
        return err1;
    }
    finally {
        FreeStringVector(envvʋ);
        FreeStringVector(argvʋ);
        if (argv0ʋ != IntPtr.Zero) {
            Marshal.FreeHGlobal(argv0ʋ);
        }
    }
}

// ============================== pipe ==============================

// pipe(2) -- Go: `func pipe(p *[2]int32) (err error)`, the //sys wrapper zsyscall_darwin_amd64.go
// generates. The native pair is written by libc and copied into the caller's box, which never
// leaves managed memory.
internal static error /*err*/ pipe([GoArrayDims(2)] ж<array<int32>> Ꮡp) {
    IntPtr fds = Marshal.AllocHGlobal(8);
    try {
        Marshal.WriteInt32(fds, 0, -1);
        Marshal.WriteInt32(fds, 4, -1);
        if (pipe_libc(fds) != 0) {
            return errnoErr((Errno)(uintptr)Marshal.GetLastPInvokeError());
        }
        ref var p = ref Ꮡp.Value;
        p[0] = Marshal.ReadInt32(fds, 0);
        p[1] = Marshal.ReadInt32(fds, 4);
        return default!;
    }
    finally {
        Marshal.FreeHGlobal(fds);
    }
}

// ============================== the native surface ==============================

// MarshalStringZ copies a Go string into unmanaged memory as NUL-terminated UTF-8 bytes.
internal static IntPtr MarshalStringZ(@string value) {
    byte[] bytes = ((slice<byte>)value).ToArray();
    IntPtr buffer = Marshal.AllocHGlobal(bytes.Length + 1);
    Marshal.Copy(bytes, 0, buffer, bytes.Length);
    Marshal.WriteByte(buffer, bytes.Length, 0);
    return buffer;
}

// MarshalStringVector builds a NULL-terminated char** in unmanaged memory. A nil slice yields an
// empty vector -- Go's own SlicePtrFromStrings semantics (an empty child environment, never an
// inherited one).
internal static IntPtr MarshalStringVector(slice<@string> values) {
    nint count = len(values);
    IntPtr vector = Marshal.AllocHGlobal((int)((count + 1) * IntPtr.Size));
    for (nint i = 0; i < count; i++) {
        Marshal.WriteIntPtr(vector, (int)(i * IntPtr.Size), MarshalStringZ(values[i]));
    }
    Marshal.WriteIntPtr(vector, (int)(count * IntPtr.Size), IntPtr.Zero);
    return vector;
}

internal static void FreeStringVector(IntPtr vector) {
    if (vector == IntPtr.Zero) {
        return;
    }
    for (int i = 0; ; i += IntPtr.Size) {
        IntPtr entry = Marshal.ReadIntPtr(vector, i);
        if (entry == IntPtr.Zero) {
            break;
        }
        Marshal.FreeHGlobal(entry);
    }
    Marshal.FreeHGlobal(vector);
}

// Apple's flag values (sys/spawn.h) and signal-mask how-values (sys/signal.h) -- darwin's, not
// glibc's: SIG_BLOCK/SIG_UNBLOCK/SIG_SETMASK are 1/2/3 here where linux has 0/1/2.
internal const short POSIX_SPAWN_SETPGROUP = 0x0002;
internal const short POSIX_SPAWN_SETSIGMASK = 0x0008;
internal const short POSIX_SPAWN_SETSID = 0x0400;
internal const int SIG_BLOCK = 1;
internal const int SIG_SETMASK = 3;

private const string libSystemB = "/usr/lib/libSystem.B.dylib";

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawn(out int pid, IntPtr path, IntPtr fileActions, IntPtr attrp, IntPtr argv, IntPtr envp);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawn_file_actions_init(IntPtr fileActions);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawn_file_actions_destroy(IntPtr fileActions);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawn_file_actions_adddup2(IntPtr fileActions, int fd, int newFd);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawn_file_actions_addclose(IntPtr fileActions, int fd);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawn_file_actions_addchdir_np(IntPtr fileActions, IntPtr path);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawnattr_init(IntPtr attrp);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawnattr_destroy(IntPtr attrp);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawnattr_setflags(IntPtr attrp, short flags);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawnattr_setpgroup(IntPtr attrp, int pgroup);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int posix_spawnattr_setsigmask(IntPtr attrp, IntPtr sigmask);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int sigemptyset(IntPtr set);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int sigaddset(IntPtr set, int signum);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int pthread_sigmask(int how, IntPtr set, IntPtr oldset);

// ioctl(2) returns -1 and SETS errno, so the errno is read back through SetLastError.
[DllImport(libSystemB, SetLastError = true)]
internal static extern int ioctl(int fd, ulong request, IntPtr arg);

[DllImport(libSystemB, SetLastError = false)]
internal static extern int kill(int pid, int sig);

[DllImport(libSystemB, SetLastError = true)]
internal static extern int execve(IntPtr path, IntPtr argv, IntPtr envp);

[DllImport(libSystemB, EntryPoint = "pipe", SetLastError = true)]
private static extern int pipe_libc(IntPtr fds);

} // end syscall_package
