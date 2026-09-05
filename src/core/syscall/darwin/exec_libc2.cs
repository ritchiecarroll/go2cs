// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || (openbsd && !mips64)
namespace go;

using abi = @internal.abi_package;
using Δruntime = runtime_package;
using @unsafe = unsafe_package;
using @internal;
using go.sync;

partial class syscall_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() {
    builtin.initPackage(typeof(@internal.abi_package));
}

[GoType] partial struct SysProcAttr {
    public @string Chroot;     // Chroot.
    public ж<Credential> Credential; // Credential.
    public bool Ptrace;        // Enable tracing.
    public bool Setsid;        // Create session.
    // Setpgid sets the process group ID of the child to Pgid,
    // or, if Pgid == 0, to the new child's process ID.
    public bool Setpgid;
    // Setctty sets the controlling terminal of the child to
    // file descriptor Ctty. Ctty must be a descriptor number
    // in the child process: an index into ProcAttr.Files.
    // This is only meaningful if Setsid is true.
    public bool Setctty;
    public bool Noctty; // Detach fd 0 from controlling terminal
    public nint Ctty; // Controlling TTY fd
    // Foreground places the child process group in the foreground.
    // This implies Setpgid. The Ctty field must be set to
    // the descriptor of the controlling TTY.
    // Unlike Setctty, in this case Ctty must be a descriptor
    // number in the parent process.
    public bool Foreground;
    public nint Pgid; // Child's process group ID if Setpgid.
}

// Implemented in runtime package.
internal static partial void runtime_BeforeFork();

internal static partial void runtime_AfterFork();

internal static partial void runtime_AfterForkInChild();

// Fork, dup fd onto 0..len(fd), and exec(argv0, argvv, envv) in child.
// If a dup or exec fails, write the errno error to pipe.
// (Pipe is close-on-exec so if exec succeeds, it will be closed.)
// In the child, this function must not acquire any locks, because
// they might have been locked at the time of the fork. This means
// no rescheduling, no malloc calls, and no new stack segments.
// For the same reason compiler does not race instrument it.
// The calls to rawSyscall are okay because they are assembly
// functions that do not grow the stack.
//
//go:norace
internal static (nint pid, Errno err1) forkAndExecInChild(ж<byte> Ꮡargv0, slice<ж<byte>> argv, slice<ж<byte>> envv, ж<byte> Ꮡchroot, ж<byte> Ꮡdir, ref ProcAttr attr, ref SysProcAttr sys, nint pipe) {
    ref var err1 = ref heap(new Errno(), out var Ꮡerr1);

    // Declare all variables at top in case any
    // declarations require heap allocation (e.g., err1).
    uintptr r1 = default!;
    
    nint nextfd = default!;
    
    nint i = default!;
    
    error err = default!;
    
    ref var pgrp = ref heap(new _C_int(), out var Ꮡpgrp);
    
    ж<Credential> cred = default!;
    
    uintptr ngroups = default!;
    uintptr groups = default!;
    var rlim = ᏑorigRlimitNofile.Load();
    // guard against side effects of shuffling fds below.
    // Make sure that nextfd is beyond any currently open files so
    // that we can't run the risk of overwriting any of them.
    var fd = new slice<nint>(len(attr.Files));
    nextfd = len(attr.Files);
    foreach (var (iΔ1, ufd) in attr.Files) {
        if (nextfd < (nint)ufd) {
            nextfd = (nint)ufd;
        }
        fd[iΔ1] = (nint)ufd;
    }
    nextfd++;
    // About to call fork.
    // No more allocation or calls of non-assembly functions.
    runtime_BeforeFork();
    (r1, _, err1) = rawSyscall(abi.FuncPCABI0(libc_fork_trampoline), 0, 0, 0);
    if (err1 != 0) {
        runtime_AfterFork();
        return (0, err1);
    }
    if (r1 != 0) {
        // parent; return PID
        runtime_AfterFork();
        return ((nint)r1, 0);
    }
    // Fork succeeded, now in child.
    // Enable tracing if requested.
    if (sys.Ptrace) {
        {
            err = ptrace(PTRACE_TRACEME, 0, 0, 0); if (err != default!) {
                err1 = err._<Errno>();
                goto childerror;
            }
        }
    }
    // Session ID
    if (sys.Setsid) {
        (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_setsid_trampoline), 0, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Set process group
    if (sys.Setpgid || sys.Foreground) {
        // Place child in process group.
        (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_setpgid_trampoline), 0, (uintptr)sys.Pgid, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    if (sys.Foreground) {
        // This should really be pid_t, however _C_int (aka int32) is
        // generally equivalent.
        pgrp = ((_C_int)(int32)sys.Pgid);
        if (pgrp == 0) {
            (r1, _, err1) = rawSyscall(abi.FuncPCABI0(libc_getpid_trampoline), 0, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
            pgrp = ((_C_int)(int32)r1);
        }
        // Place process group in foreground.
        var ᴋ0 = Ꮡpgrp;
                (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_ioctl_trampoline), (uintptr)sys.Ctty, (uintptr)TIOCSPGRP, (uintptr)ᴋ0);
        System.GC.KeepAlive(ᴋ0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Restore the signal mask. We do this after TIOCSPGRP to avoid
    // having the kernel send a SIGTTOU signal to the process group.
    runtime_AfterForkInChild();
    // Chroot
    if (Ꮡchroot != nil) {
        var ᴋ1 = Ꮡchroot;
                (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_chroot_trampoline), (uintptr)ᴋ1, 0, 0);
        System.GC.KeepAlive(ᴋ1);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // User and groups
    {
        cred = sys.Credential; if (cred != nil) {
            ngroups = (uintptr)len((~cred).Groups);
            groups = (uintptr)0;
            if (ngroups > 0) {
                groups = (uintptr)Ꮡ((~cred).Groups, 0);
            }
            if (!(~cred).NoSetGroups) {
                (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_setgroups_trampoline), ngroups, groups, 0);
                if (err1 != 0) {
                    goto childerror;
                }
            }
            (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_setgid_trampoline), (uintptr)(~cred).Gid, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
            (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_setuid_trampoline), (uintptr)(~cred).Uid, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
        }
    }
    // Chdir
    if (Ꮡdir != nil) {
        var ᴋ2 = Ꮡdir;
                (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_chdir_trampoline), (uintptr)ᴋ2, 0, 0);
        System.GC.KeepAlive(ᴋ2);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Pass 1: look for fd[i] < i and move those up above len(fd)
    // so that pass 2 won't stomp on an fd it needs later.
    if (pipe < nextfd) {
        if (Δruntime.GOOS == "openbsd"u8){
            (_, _, err1) = rawSyscall(dupTrampoline, (uintptr)pipe, (uintptr)nextfd, O_CLOEXEC);
        } else {
            (_, _, err1) = rawSyscall(dupTrampoline, (uintptr)pipe, (uintptr)nextfd, 0);
            if (err1 != 0) {
                goto childerror;
            }
            (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_fcntl_trampoline), (uintptr)nextfd, F_SETFD, FD_CLOEXEC);
        }
        if (err1 != 0) {
            goto childerror;
        }
        pipe = nextfd;
        nextfd++;
    }
    for (i = 0; i < len(fd); i++) {
        if (fd[i] >= 0 && fd[i] < i) {
            if (nextfd == pipe) {
                // don't stomp on pipe
                nextfd++;
            }
            if (Δruntime.GOOS == "openbsd"u8){
                (_, _, err1) = rawSyscall(dupTrampoline, (uintptr)fd[i], (uintptr)nextfd, O_CLOEXEC);
            } else {
                (_, _, err1) = rawSyscall(dupTrampoline, (uintptr)fd[i], (uintptr)nextfd, 0);
                if (err1 != 0) {
                    goto childerror;
                }
                (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_fcntl_trampoline), (uintptr)nextfd, F_SETFD, FD_CLOEXEC);
            }
            if (err1 != 0) {
                goto childerror;
            }
            fd[i] = nextfd;
            nextfd++;
        }
    }
    // Pass 2: dup fd[i] down onto i.
    for (i = 0; i < len(fd); i++) {
        if (fd[i] == -1) {
            rawSyscall(abi.FuncPCABI0(libc_close_trampoline), (uintptr)i, 0, 0);
            continue;
        }
        if (fd[i] == i) {
            // dup2(i, i) won't clear close-on-exec flag on Linux,
            // probably not elsewhere either.
            (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_fcntl_trampoline), (uintptr)fd[i], F_SETFD, 0);
            if (err1 != 0) {
                goto childerror;
            }
            continue;
        }
        // The new fd is created NOT close-on-exec,
        // which is exactly what we want.
        (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_dup2_trampoline), (uintptr)fd[i], (uintptr)i, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // By convention, we don't close-on-exec the fds we are
    // started with, so if len(fd) < 3, close 0, 1, 2 as needed.
    // Programs that know they inherit fds >= 3 will need
    // to set them close-on-exec.
    for (i = len(fd); i < 3; i++) {
        rawSyscall(abi.FuncPCABI0(libc_close_trampoline), (uintptr)i, 0, 0);
    }
    // Detach fd 0 from tty
    if (sys.Noctty) {
        (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_ioctl_trampoline), 0, (uintptr)TIOCNOTTY, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Set the controlling TTY to Ctty
    if (sys.Setctty) {
        (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_ioctl_trampoline), (uintptr)sys.Ctty, (uintptr)TIOCSCTTY, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Restore original rlimit.
    if (rlim != nil) {
        var ᴋ3 = rlim;
                rawSyscall(abi.FuncPCABI0(libc_setrlimit_trampoline), (uintptr)RLIMIT_NOFILE, (uintptr)ᴋ3, 0);
        System.GC.KeepAlive(ᴋ3);
    }
    // Time to exec.
    var ᴋ4 = Ꮡargv0;
    var ᴋ5 = @unsafe.Pointer.FromBox(Ꮡ(argv, 0));
    var ᴋ6 = @unsafe.Pointer.FromBox(Ꮡ(envv, 0));
        (_, _, err1) = rawSyscall(abi.FuncPCABI0(libc_execve_trampoline), (uintptr)ᴋ4, (uintptr)ᴋ5, (uintptr)ᴋ6);
    System.GC.KeepAlive(ᴋ4);
    System.GC.KeepAlive(ᴋ5);
    System.GC.KeepAlive(ᴋ6);
childerror:
    var ᴋ7 = @unsafe.Pointer.FromBox(Ꮡerr1);
        rawSyscall(abi.FuncPCABI0(libc_write_trampoline), (uintptr)pipe, (uintptr)ᴋ7, /* unsafe.Sizeof(err1) */ (uintptr)8);
    System.GC.KeepAlive(ᴋ7);
    // send error code on pipe
    while (ᐧ) {
        rawSyscall(abi.FuncPCABI0(libc_exit_trampoline), 253, 0, 0);
    }
}

// forkAndExecFailureCleanup cleans up after an exec failure.
internal static void forkAndExecFailureCleanup(ref ProcAttr attr, ref SysProcAttr sys) {
}

// Nothing to do.

} // end syscall_package
