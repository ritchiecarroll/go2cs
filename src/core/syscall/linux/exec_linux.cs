// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go;

using errpkg = errors_package;
using itoa = @internal.itoa_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;
using @internal;
using System.Runtime.CompilerServices;
using go.sync;

partial class syscall_package {

// Linux unshare/clone/clone2/clone3 flags, architecture-independent,
// copied from linux/sched.h.
public static UntypedInt CLONE_VM => 0x00000100; // set if VM shared between processes

public static UntypedInt CLONE_FS => 0x00000200; // set if fs info shared between processes

public static UntypedInt CLONE_FILES => 0x00000400; // set if open files shared between processes

public static UntypedInt CLONE_SIGHAND => 0x00000800; // set if signal handlers and blocked signals shared

public static UntypedInt CLONE_PIDFD => 0x00001000; // set if a pidfd should be placed in parent

public static UntypedInt CLONE_PTRACE => 0x00002000; // set if we want to let tracing continue on the child too

public static UntypedInt CLONE_VFORK => 0x00004000; // set if the parent wants the child to wake it up on mm_release

public static UntypedInt CLONE_PARENT => 0x00008000; // set if we want to have the same parent as the cloner

public static UntypedInt CLONE_THREAD => 0x00010000; // Same thread group?

public static UntypedInt CLONE_NEWNS => 0x00020000; // New mount namespace group

public static UntypedInt CLONE_SYSVSEM => 0x00040000; // share system V SEM_UNDO semantics

public static UntypedInt CLONE_SETTLS => 0x00080000; // create a new TLS for the child

public static UntypedInt CLONE_PARENT_SETTID => 0x00100000; // set the TID in the parent

public static UntypedInt CLONE_CHILD_CLEARTID => 0x00200000; // clear the TID in the child

public static UntypedInt CLONE_DETACHED => 0x00400000; // Unused, ignored

public static UntypedInt CLONE_UNTRACED => 0x00800000; // set if the tracing process can't force CLONE_PTRACE on this clone

public static UntypedInt CLONE_CHILD_SETTID => 0x01000000; // set the TID in the child

public static UntypedInt CLONE_NEWCGROUP => 0x02000000; // New cgroup namespace

public static UntypedInt CLONE_NEWUTS => 0x04000000; // New utsname namespace

public static UntypedInt CLONE_NEWIPC => 0x08000000; // New ipc namespace

public static UntypedInt CLONE_NEWUSER => 0x10000000; // New user namespace

public static UntypedInt CLONE_NEWPID => 0x20000000; // New pid namespace

public static UntypedInt CLONE_NEWNET => 0x40000000; // New network namespace

public static UntypedInt CLONE_IO => 0x80000000; // Clone io context
// Flags for the clone3() syscall.

public static UntypedInt CLONE_CLEAR_SIGHAND => 0x100000000; // Clear any signal handler and reset to SIG_DFL.

public static UntypedInt CLONE_INTO_CGROUP => 0x200000000; // Clone into a specific cgroup given the right permissions.
// Cloning flags intersect with CSIGNAL so can be used with unshare and clone3
// syscalls only:

public static UntypedInt CLONE_NEWTIME => 0x00000080; // New time namespace

// SysProcIDMap holds Container ID to Host ID mappings used for User Namespaces in Linux.
// See user_namespaces(7).
//
// Note that User Namespaces are not available on a number of popular Linux
// versions (due to security issues), or are available but subject to AppArmor
// restrictions like in Ubuntu 24.04.
[GoType] partial struct SysProcIDMap {
    public nint ContainerID; // Container ID.
    public nint HostID; // Host ID.
    public nint Size; // Size.
}

[GoType] partial struct SysProcAttr {
    public @string Chroot;     // Chroot.
    public ж<Credential> Credential; // Credential.
    // Ptrace tells the child to call ptrace(PTRACE_TRACEME).
    // Call runtime.LockOSThread before starting a process with this set,
    // and don't call UnlockOSThread until done with PtraceSyscall calls.
    public bool Ptrace;
    public bool Setsid; // Create session.
    // Setpgid sets the process group ID of the child to Pgid,
    // or, if Pgid == 0, to the new child's process ID.
    public bool Setpgid;
    // Setctty sets the controlling terminal of the child to
    // file descriptor Ctty. Ctty must be a descriptor number
    // in the child process: an index into ProcAttr.Files.
    // This is only meaningful if Setsid is true.
    public bool Setctty;
    public bool Noctty; // Detach fd 0 from controlling terminal.
    public nint Ctty; // Controlling TTY fd.
    // Foreground places the child process group in the foreground.
    // This implies Setpgid. The Ctty field must be set to
    // the descriptor of the controlling TTY.
    // Unlike Setctty, in this case Ctty must be a descriptor
    // number in the parent process.
    public bool Foreground;
    public nint Pgid; // Child's process group ID if Setpgid.
    // Pdeathsig, if non-zero, is a signal that the kernel will send to
    // the child process when the creating thread dies. Note that the signal
    // is sent on thread termination, which may happen before process termination.
    // There are more details at https://go.dev/issue/27505.
    public ΔSignal Pdeathsig;
    public uintptr Cloneflags;        // Flags for clone calls.
    public uintptr Unshareflags;        // Flags for unshare calls.
    public slice<SysProcIDMap> UidMappings; // User ID mappings for user namespaces.
    public slice<SysProcIDMap> GidMappings; // Group ID mappings for user namespaces.
    // GidMappingsEnableSetgroups enabling setgroups syscall.
    // If false, then setgroups syscall will be disabled for the child process.
    // This parameter is no-op if GidMappings == nil. Otherwise for unprivileged
    // users this should be set to false for mappings work.
    public bool GidMappingsEnableSetgroups;
    public slice<uintptr> AmbientCaps; // Ambient capabilities.
    public bool UseCgroupFD;      // Whether to make use of the CgroupFD field.
    public nint CgroupFD;      // File descriptor of a cgroup to put the new process into.
    // PidFD, if not nil, is used to store the pidfd of a child, if the
    // functionality is supported by the kernel, or -1. Note *PidFD is
    // changed only if the process starts successfully.
    public ж<nint> PidFD;
}

internal static ж<array<byte>> Ꮡnone = new StandardBox<array<byte>>(new byte[]{(rune)'n', (rune)'o', (rune)'n', (rune)'e', 0}.array());
internal static ref array<byte> none => ref Ꮡnone.Value;
internal static ж<array<byte>> Ꮡslash = new StandardBox<array<byte>>(new byte[]{(rune)'/', 0}.array());
internal static ref array<byte> slash => ref Ꮡslash.Value;
internal static ж<bool> ᏑforceClone3 = new StandardBox<bool>(false);
internal static ref bool forceClone3 => ref ᏑforceClone3.Value; // Used by unit tests only.

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
// The calls to RawSyscall are okay because they are assembly
// functions that do not grow the stack.
//
//go:norace
internal static (nint pid, Errno err) forkAndExecInChild(ж<byte> Ꮡargv0, slice<ж<byte>> argv, slice<ж<byte>> envv, ж<byte> Ꮡchroot, ж<byte> Ꮡdir, ref ProcAttr attr, ref SysProcAttr sys, nint pipe) {
    nint pid = default!;
    Errno err = default!;

    // Set up and fork. This returns immediately in the parent or
    // if there's an error.
    (var upid, var pidfd, err, var mapPipe, var locked) = forkAndExecInChild1(Ꮡargv0, argv, envv, Ꮡchroot, Ꮡdir, ref attr, ref sys, pipe);
    if (locked) {
        runtime_AfterFork();
    }
    if (err != 0) {
        return (0, err);
    }
    // parent; return PID
    pid = (nint)upid;
    if (sys.PidFD != nil) {
        sys.PidFD.Value = (nint)pidfd;
    }
    if (sys.UidMappings != default! || sys.GidMappings != default!) {
        Close(mapPipe[0]);
        ref var err2 = ref heap(new Errno(), out var Ꮡerr2);
        // uid/gid mappings will be written after fork and unshare(2) for user
        // namespaces.
        if ((uintptr)(sys.Unshareflags & (uintptr)CLONE_NEWUSER) == 0) {
            {
                var errΔ1 = writeUidGidMappings(pid, ref sys); if (errΔ1 != default!) {
                    err2 = errΔ1._<Errno>();
                }
            }
        }
        var ᴋ0 = @unsafe.Pointer.FromBox(Ꮡerr2);
                RawSyscall(SYS_WRITE, (uintptr)mapPipe[1], (uintptr)ᴋ0, /* unsafe.Sizeof(err2) */ (uintptr)8);
        System.GC.KeepAlive(ᴋ0);
        Close(mapPipe[1]);
    }
    return (pid, 0);
}

internal static UntypedInt _LINUX_CAPABILITY_VERSION_3 => 0x20080522;

[GoType] partial struct capHeader {
    internal uint32 version;
    internal int32 pid;
}

[GoType] partial struct capData {
    internal uint32 effective;
    internal uint32 permitted;
    internal uint32 inheritable;
}

[GoType] partial struct caps {
    internal capHeader hdr;
    internal array<capData> data = new(2);
}

// See CAP_TO_INDEX in linux/capability.h:
internal static uintptr capToIndex(uintptr cap) {
    return (cap >> (int)(5));
}

// See CAP_TO_MASK in linux/capability.h:
internal static uint32 capToMask(uintptr cap) {
    return ((uint32)1).Lsh((nuint)((uintptr)(cap & 31)));
}

// cloneArgs holds arguments for clone3 Linux syscall.
[GoType] partial struct cloneArgs {
    internal uint64 flags; // Flags bit mask
    internal uint64 pidFD; // Where to store PID file descriptor (int *)
    internal uint64 childTID; // Where to store child TID, in child's memory (pid_t *)
    internal uint64 parentTID; // Where to store child TID, in parent's memory (pid_t *)
    internal uint64 exitSignal; // Signal to deliver to parent on child termination
    internal uint64 stack; // Pointer to lowest byte of stack
    internal uint64 stackSize; // Size of stack
    internal uint64 tls; // Location of new TLS
    internal uint64 setTID; // Pointer to a pid_t array (since Linux 5.5)
    internal uint64 setTIDSize; // Number of elements in set_tid (since Linux 5.5)
    internal uint64 cgroup; // File descriptor for target cgroup of child (since Linux 5.7)
}

// forkAndExecInChild1 implements the body of forkAndExecInChild up to
// the parent's post-fork path. This is a separate function so we can
// separate the child's and parent's stack frames if we're using
// vfork.
//
// This is go:noinline because the point is to keep the stack frames
// of this and forkAndExecInChild separate.
//
//go:noinline
//go:norace
//go:nocheckptr
internal static partial (uintptr pid, int32 pidfd, Errno err1, array<nint> mapPipe, bool locked) forkAndExecInChild1(ж<byte> Ꮡargv0, slice<ж<byte>> argv, slice<ж<byte>> envv, ж<byte> Ꮡchroot, ж<byte> Ꮡdir, ref ProcAttr attr, ref SysProcAttr sys, nint pipe) {
    uintptr pid = default!;
    ref var pidfd = ref heap(new int32(), out var Ꮡpidfd);
    ref var err1 = ref heap(new Errno(), out var Ꮡerr1);
    array<nint> mapPipe = new(2);
    bool locked = default!;

    // Defined in linux/prctl.h starting with Linux 4.3.
    uintptr PR_CAP_AMBIENT = 0x2f;
    
    UntypedInt PR_CAP_AMBIENT_RAISE = 0x2;
    // vfork requires that the child not touch any of the parent's
    // active stack frames. Hence, the child does all post-fork
    // processing in this stack frame and never returns, while the
    // parent returns immediately from this frame and does all
    // post-fork processing in the outer frame.
    //
    // Declare all variables at top in case any
    // declarations require heap allocation (e.g., err2).
    // ":=" should not be used to declare any variable after
    // the call to runtime_BeforeFork.
    //
    // NOTE(bcmills): The allocation behavior described in the above comment
    // seems to lack a corresponding test, and it may be rendered invalid
    // by an otherwise-correct change in the compiler.
    ref var err2 = ref heap(new Errno(), out var Ꮡerr2);
    
    nint nextfd = default!;
    
    nint i = default!;
    
    ref var caps = ref heap(new caps(), out var Ꮡcaps);
    
    uintptr fd1 = default!;
    uintptr flags = default!;
    
    slice<byte> puid = default!;
    slice<byte> psetgroups = default!;
    slice<byte> pgid = default!;
    
    slice<byte> uidmap = default!;
    slice<byte> setgroups = default!;
    slice<byte> gidmap = default!;
    
    ж<cloneArgs> clone3 = default!;
    
    ref var pgrp = ref heap(new int32(), out var Ꮡpgrp);
    
    nint dirfd = default!;
    
    ж<Credential> cred = default!;
    
    uintptr ngroups = default!;
    uintptr groups = default!;
    
    uintptr c = default!;
    
    ж<Rlimit> rlim = default!;
    
    ref var lim = ref heap(new Rlimit(), out var Ꮡlim);
    pidfd = -1;
    rlim = ᏑorigRlimitNofile.Load();
    if (sys.UidMappings != default!) {
        puid = slice<byte>("/proc/self/uid_map\u0000"u8);
        uidmap = formatIDMappings(sys.UidMappings);
    }
    if (sys.GidMappings != default!) {
        psetgroups = slice<byte>("/proc/self/setgroups\u0000"u8);
        pgid = slice<byte>("/proc/self/gid_map\u0000"u8);
        if (sys.GidMappingsEnableSetgroups){
            setgroups = slice<byte>("allow\u0000"u8);
        } else {
            setgroups = slice<byte>("deny\u0000"u8);
        }
        gidmap = formatIDMappings(sys.GidMappings);
    }
    // Record parent PID so child can test if it has died.
    var (ppid, _) = rawSyscallNoError(SYS_GETPID, 0, 0, 0);
    // Guard against side effects of shuffling fds below.
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
    // Allocate another pipe for parent to child communication for
    // synchronizing writing of User ID/Group ID mappings.
    if (sys.UidMappings != default! || sys.GidMappings != default!) {
        {
            var err = forkExecPipe(mapPipe[..]); if (err != default!) {
                err1 = err._<Errno>();
                return (pid, pidfd, err1, mapPipe, locked);
            }
        }
    }
    flags = sys.Cloneflags;
    if ((uintptr)(sys.Cloneflags & (uintptr)CLONE_NEWUSER) == 0 && (uintptr)(sys.Unshareflags & (uintptr)CLONE_NEWUSER) == 0) {
        flags |= (uintptr)((uintptr)((uintptr)CLONE_VFORK | (uintptr)CLONE_VM));
    }
    if (sys.PidFD != nil) {
        flags |= (uintptr)(CLONE_PIDFD);
    }
    // Whether to use clone3.
    if (sys.UseCgroupFD || (uintptr)(flags & (uintptr)CLONE_NEWTIME) != 0 || forceClone3) {
        clone3 = Ꮡ(new cloneArgs(
            flags: (uint64)flags,
            exitSignal: (uint64)(nint)SIGCHLD
        ));
        if (sys.UseCgroupFD) {
            clone3.Value.flags |= (uint64)(CLONE_INTO_CGROUP);
            clone3.Value.cgroup = (uint64)sys.CgroupFD;
        }
        if (sys.PidFD != nil) {
            clone3.Value.pidFD = (uint64)(uintptr)Ꮡpidfd;
        }
    }
    // About to call fork.
    // No more allocation or calls of non-assembly functions.
    runtime_BeforeFork();
    locked = true;
    if (clone3 != nil){
        (pid, err1) = rawVforkSyscall(_SYS_clone3, (uintptr)clone3, /* unsafe.Sizeof(*clone3) */ (uintptr)88, 0);
    } else {
        // N.B. Keep in sync with doCheckClonePidfd.
        flags |= (uintptr)((uintptr)(nint)SIGCHLD);
        if (runtime.GOARCH == "s390x"u8){
            // On Linux/s390, the first two arguments of clone(2) are swapped.
            (pid, err1) = rawVforkSyscall(SYS_CLONE, 0, flags, (uintptr)Ꮡpidfd);
        } else {
            (pid, err1) = rawVforkSyscall(SYS_CLONE, flags, 0, (uintptr)Ꮡpidfd);
        }
    }
    if (err1 != 0 || pid != 0) {
        // If we're in the parent, we must return immediately
        // so we're not in the same stack frame as the child.
        // This can at most use the return PC, which the child
        // will not modify, and the results of
        // rawVforkSyscall, which must have been written after
        // the child was replaced.
        return (pid, pidfd, err1, mapPipe, locked);
    }
    // Fork succeeded, now in child.
    // Enable the "keep capabilities" flag to set ambient capabilities later.
    if (len(sys.AmbientCaps) > 0) {
        (_, _, err1) = RawSyscall6(SYS_PRCTL, PR_SET_KEEPCAPS, 1, 0, 0, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Wait for User ID/Group ID mappings to be written.
    if (sys.UidMappings != default! || sys.GidMappings != default!) {
        {
            (_, _, err1) = RawSyscall(SYS_CLOSE, (uintptr)mapPipe[1], 0, 0); if (err1 != 0) {
                goto childerror;
            }
        }
        var ᴋ1 = @unsafe.Pointer.FromBox(Ꮡerr2);
                (pid, _, err1) = RawSyscall(SYS_READ, (uintptr)mapPipe[0], (uintptr)ᴋ1, /* unsafe.Sizeof(err2) */ (uintptr)8);
        System.GC.KeepAlive(ᴋ1);
        if (err1 != 0) {
            goto childerror;
        }
        if (pid != /* unsafe.Sizeof(err2) */ (uintptr)8) {
            err1 = EINVAL;
            goto childerror;
        }
        if (err2 != 0) {
            err1 = err2;
            goto childerror;
        }
    }
    // Session ID
    if (sys.Setsid) {
        (_, _, err1) = RawSyscall(SYS_SETSID, 0, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Set process group
    if (sys.Setpgid || sys.Foreground) {
        // Place child in process group.
        (_, _, err1) = RawSyscall(SYS_SETPGID, 0, (uintptr)sys.Pgid, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    if (sys.Foreground) {
        pgrp = (int32)sys.Pgid;
        if (pgrp == 0) {
            (pid, _) = rawSyscallNoError(SYS_GETPID, 0, 0, 0);
            pgrp = (int32)pid;
        }
        // Place process group in foreground.
        var ᴋ2 = Ꮡpgrp;
                (_, _, err1) = RawSyscall(SYS_IOCTL, (uintptr)sys.Ctty, (uintptr)TIOCSPGRP, (uintptr)ᴋ2);
        System.GC.KeepAlive(ᴋ2);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Restore the signal mask. We do this after TIOCSPGRP to avoid
    // having the kernel send a SIGTTOU signal to the process group.
    runtime_AfterForkInChild();
    // Unshare
    if (sys.Unshareflags != 0) {
        (_, _, err1) = RawSyscall(SYS_UNSHARE, sys.Unshareflags, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
        if ((uintptr)(sys.Unshareflags & (uintptr)CLONE_NEWUSER) != 0 && sys.GidMappings != default!) {
            dirfd = (nint)_AT_FDCWD;
            {
                var ᴋ3 = Ꮡ(psetgroups, 0);
                                (fd1, _, err1) = RawSyscall6(SYS_OPENAT, (uintptr)dirfd, (uintptr)ᴋ3, (uintptr)O_WRONLY, 0, 0, 0);
                System.GC.KeepAlive(ᴋ3); if (err1 != 0) {
                    goto childerror;
                }
            }
            var ᴋ4 = Ꮡ(setgroups, 0);
                        (pid, _, err1) = RawSyscall(SYS_WRITE, fd1, (uintptr)ᴋ4, (uintptr)len(setgroups));
            System.GC.KeepAlive(ᴋ4);
            if (err1 != 0) {
                goto childerror;
            }
            {
                (_, _, err1) = RawSyscall(SYS_CLOSE, fd1, 0, 0); if (err1 != 0) {
                    goto childerror;
                }
            }
            {
                var ᴋ5 = Ꮡ(pgid, 0);
                                (fd1, _, err1) = RawSyscall6(SYS_OPENAT, (uintptr)dirfd, (uintptr)ᴋ5, (uintptr)O_WRONLY, 0, 0, 0);
                System.GC.KeepAlive(ᴋ5); if (err1 != 0) {
                    goto childerror;
                }
            }
            var ᴋ6 = Ꮡ(gidmap, 0);
                        (pid, _, err1) = RawSyscall(SYS_WRITE, fd1, (uintptr)ᴋ6, (uintptr)len(gidmap));
            System.GC.KeepAlive(ᴋ6);
            if (err1 != 0) {
                goto childerror;
            }
            {
                (_, _, err1) = RawSyscall(SYS_CLOSE, fd1, 0, 0); if (err1 != 0) {
                    goto childerror;
                }
            }
        }
        if ((uintptr)(sys.Unshareflags & (uintptr)CLONE_NEWUSER) != 0 && sys.UidMappings != default!) {
            dirfd = (nint)_AT_FDCWD;
            {
                var ᴋ7 = Ꮡ(puid, 0);
                                (fd1, _, err1) = RawSyscall6(SYS_OPENAT, (uintptr)dirfd, (uintptr)ᴋ7, (uintptr)O_WRONLY, 0, 0, 0);
                System.GC.KeepAlive(ᴋ7); if (err1 != 0) {
                    goto childerror;
                }
            }
            var ᴋ8 = Ꮡ(uidmap, 0);
                        (pid, _, err1) = RawSyscall(SYS_WRITE, fd1, (uintptr)ᴋ8, (uintptr)len(uidmap));
            System.GC.KeepAlive(ᴋ8);
            if (err1 != 0) {
                goto childerror;
            }
            {
                (_, _, err1) = RawSyscall(SYS_CLOSE, fd1, 0, 0); if (err1 != 0) {
                    goto childerror;
                }
            }
        }
        // The unshare system call in Linux doesn't unshare mount points
        // mounted with --shared. Systemd mounts / with --shared. For a
        // long discussion of the pros and cons of this see debian bug 739593.
        // The Go model of unsharing is more like Plan 9, where you ask
        // to unshare and the namespaces are unconditionally unshared.
        // To make this model work we must further mark / as MS_PRIVATE.
        // This is what the standard unshare command does.
        if ((uintptr)(sys.Unshareflags & (uintptr)CLONE_NEWNS) == CLONE_NEWNS) {
            var ᴋ9 = Ꮡnone.at<byte>(0);
            var ᴋ10 = Ꮡslash.at<byte>(0);
                        (_, _, err1) = RawSyscall6(SYS_MOUNT, (uintptr)ᴋ9, (uintptr)ᴋ10, 0, (uintptr)((uintptr)MS_REC | (uintptr)MS_PRIVATE), 0, 0);
            System.GC.KeepAlive(ᴋ9);
            System.GC.KeepAlive(ᴋ10);
            if (err1 != 0) {
                goto childerror;
            }
        }
    }
    // Chroot
    if (Ꮡchroot != nil) {
        var ᴋ11 = Ꮡchroot;
                (_, _, err1) = RawSyscall(SYS_CHROOT, (uintptr)ᴋ11, 0, 0);
        System.GC.KeepAlive(ᴋ11);
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
            if (!(sys.GidMappings != default! && !sys.GidMappingsEnableSetgroups && ngroups == 0) && !(~cred).NoSetGroups) {
                (_, _, err1) = RawSyscall(_SYS_setgroups, ngroups, groups, 0);
                if (err1 != 0) {
                    goto childerror;
                }
            }
            (_, _, err1) = RawSyscall(sys_SETGID, (uintptr)(~cred).Gid, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
            (_, _, err1) = RawSyscall(sys_SETUID, (uintptr)(~cred).Uid, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
        }
    }
    if (len(sys.AmbientCaps) != 0) {
        // Ambient capabilities were added in the 4.3 kernel,
        // so it is safe to always use _LINUX_CAPABILITY_VERSION_3.
        caps.hdr.version = _LINUX_CAPABILITY_VERSION_3;
        {
            var ᴋ12 = Ꮡcaps.of(syscall_package.caps.Ꮡhdr);
            var ᴋ13 = Ꮡcaps.at(syscall_package.caps.Ꮡdata, 0);
                        (_, _, err1) = RawSyscall(SYS_CAPGET, (uintptr)ᴋ12, (uintptr)ᴋ13, 0);
            System.GC.KeepAlive(ᴋ12);
            System.GC.KeepAlive(ᴋ13); if (err1 != 0) {
                goto childerror;
            }
        }
        foreach (var (_, vᴛ1) in sys.AmbientCaps) {
            c = vᴛ1;

            // Add the c capability to the permitted and inheritable capability mask,
            // otherwise we will not be able to add it to the ambient capability mask.
            caps.data[capToIndex(c)].permitted |= (uint32)(capToMask(c));
            caps.data[capToIndex(c)].inheritable |= (uint32)(capToMask(c));
        }
        {
            var ᴋ14 = Ꮡcaps.of(syscall_package.caps.Ꮡhdr);
            var ᴋ15 = Ꮡcaps.at(syscall_package.caps.Ꮡdata, 0);
                        (_, _, err1) = RawSyscall(SYS_CAPSET, (uintptr)ᴋ14, (uintptr)ᴋ15, 0);
            System.GC.KeepAlive(ᴋ14);
            System.GC.KeepAlive(ᴋ15); if (err1 != 0) {
                goto childerror;
            }
        }
        foreach (var (_, vᴛ2) in sys.AmbientCaps) {
            c = vᴛ2;

            (_, _, err1) = RawSyscall6(SYS_PRCTL, PR_CAP_AMBIENT, (uintptr)PR_CAP_AMBIENT_RAISE, c, 0, 0, 0);
            if (err1 != 0) {
                goto childerror;
            }
        }
    }
    // Chdir
    if (Ꮡdir != nil) {
        var ᴋ16 = Ꮡdir;
                (_, _, err1) = RawSyscall(SYS_CHDIR, (uintptr)ᴋ16, 0, 0);
        System.GC.KeepAlive(ᴋ16);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Parent death signal
    if (sys.Pdeathsig != 0) {
        (_, _, err1) = RawSyscall6(SYS_PRCTL, PR_SET_PDEATHSIG, (uintptr)(nint)sys.Pdeathsig, 0, 0, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
        // Signal self if parent is already dead. This might cause a
        // duplicate signal in rare cases, but it won't matter when
        // using SIGKILL.
        (pid, _) = rawSyscallNoError(SYS_GETPPID, 0, 0, 0);
        if (pid != ppid) {
            (pid, _) = rawSyscallNoError(SYS_GETPID, 0, 0, 0);
            (_, _, err1) = RawSyscall(SYS_KILL, pid, (uintptr)(nint)sys.Pdeathsig, 0);
            if (err1 != 0) {
                goto childerror;
            }
        }
    }
    // Pass 1: look for fd[i] < i and move those up above len(fd)
    // so that pass 2 won't stomp on an fd it needs later.
    if (pipe < nextfd) {
        (_, _, err1) = RawSyscall(SYS_DUP3, (uintptr)pipe, (uintptr)nextfd, O_CLOEXEC);
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
            (_, _, err1) = RawSyscall(SYS_DUP3, (uintptr)fd[i], (uintptr)nextfd, O_CLOEXEC);
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
            RawSyscall(SYS_CLOSE, (uintptr)i, 0, 0);
            continue;
        }
        if (fd[i] == i) {
            // dup2(i, i) won't clear close-on-exec flag on Linux,
            // probably not elsewhere either.
            (_, _, err1) = RawSyscall(fcntl64Syscall, (uintptr)fd[i], F_SETFD, 0);
            if (err1 != 0) {
                goto childerror;
            }
            continue;
        }
        // The new fd is created NOT close-on-exec,
        // which is exactly what we want.
        (_, _, err1) = RawSyscall(SYS_DUP3, (uintptr)fd[i], (uintptr)i, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // By convention, we don't close-on-exec the fds we are
    // started with, so if len(fd) < 3, close 0, 1, 2 as needed.
    // Programs that know they inherit fds >= 3 will need
    // to set them close-on-exec.
    for (i = len(fd); i < 3; i++) {
        RawSyscall(SYS_CLOSE, (uintptr)i, 0, 0);
    }
    // Detach fd 0 from tty
    if (sys.Noctty) {
        (_, _, err1) = RawSyscall(SYS_IOCTL, 0, (uintptr)TIOCNOTTY, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Set the controlling TTY to Ctty
    if (sys.Setctty) {
        (_, _, err1) = RawSyscall(SYS_IOCTL, (uintptr)sys.Ctty, (uintptr)TIOCSCTTY, 1);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Restore original rlimit.
    if (rlim != nil) {
        // Some other process may have changed our rlimit by
        // calling prlimit. We can check for that case because
        // our current rlimit will not be the value we set when
        // caching the rlimit in the init function in rlimit.go.
        //
        // Note that this test is imperfect, since it won't catch
        // the case in which some other process used prlimit to
        // set our rlimits to max-1/max. In that case we will fall
        // back to the original cur/max when starting the child.
        // We hope that setting to max-1/max is unlikely.
        var ᴋ17 = Ꮡlim;
                (_, _, err1) = RawSyscall6(SYS_PRLIMIT64, 0, RLIMIT_NOFILE, 0, (uintptr)ᴋ17, 0, 0);
        System.GC.KeepAlive(ᴋ17);
        if (err1 != 0 || (lim.Cur == (~rlim).Max - 1 && lim.Max == (~rlim).Max)) {
            var ᴋ18 = rlim;
                        RawSyscall6(SYS_PRLIMIT64, 0, RLIMIT_NOFILE, (uintptr)ᴋ18, 0, 0, 0);
            System.GC.KeepAlive(ᴋ18);
        }
    }
    // Enable tracing if requested.
    // Do this right before exec so that we don't unnecessarily trace the runtime
    // setting up after the fork. See issue #21428.
    if (sys.Ptrace) {
        (_, _, err1) = RawSyscall(SYS_PTRACE, (uintptr)PTRACE_TRACEME, 0, 0);
        if (err1 != 0) {
            goto childerror;
        }
    }
    // Time to exec.
    var ᴋ19 = Ꮡargv0;
    var ᴋ20 = @unsafe.Pointer.FromBox(Ꮡ(argv, 0));
    var ᴋ21 = @unsafe.Pointer.FromBox(Ꮡ(envv, 0));
        (_, _, err1) = RawSyscall(SYS_EXECVE, (uintptr)ᴋ19, (uintptr)ᴋ20, (uintptr)ᴋ21);
    System.GC.KeepAlive(ᴋ19);
    System.GC.KeepAlive(ᴋ20);
    System.GC.KeepAlive(ᴋ21);
childerror:
    var ᴋ22 = @unsafe.Pointer.FromBox(Ꮡerr1);
        RawSyscall(SYS_WRITE, (uintptr)pipe, (uintptr)ᴋ22, /* unsafe.Sizeof(err1) */ (uintptr)8);
    System.GC.KeepAlive(ᴋ22);
    // send error code on pipe
    while (ᐧ) {
        RawSyscall(SYS_EXIT, 253, 0, 0);
    }
}

internal static slice<byte> formatIDMappings(slice<SysProcIDMap> idMap) {
    slice<byte> data = default!;
    foreach (var (_, im) in idMap) {
        data = append(data, ((@string)(itoa.Itoa(im.ContainerID) + " "u8 + itoa.Itoa(im.HostID) + " "u8 + itoa.Itoa(im.Size) + "\n"u8)).ꓸꓸꓸ);
    }
    return data;
}

// writeIDMappings writes the user namespace User ID or Group ID mappings to the specified path.
internal static error writeIDMappings(@string path, slice<SysProcIDMap> idMap) {
    var (fd, err) = Open(path, O_RDWR, 0);
    if (err != default!) {
        return err;
    }
    {
        var (_, errΔ1) = Write(fd, formatIDMappings(idMap)); if (errΔ1 != default!) {
            Close(fd);
            return errΔ1;
        }
    }
    {
        var errΔ2 = Close(fd); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    return default!;
}

// writeSetgroups writes to /proc/PID/setgroups "deny" if enable is false
// and "allow" if enable is true.
// This is needed since kernel 3.19, because you can't write gid_map without
// disabling setgroups() system call.
internal static error writeSetgroups(nint pid, bool enable) {
    @string sgf = "/proc/"u8 + itoa.Itoa(pid) + "/setgroups"u8;
    var (fd, err) = Open(sgf, O_RDWR, 0);
    if (err != default!) {
        return err;
    }
    slice<byte> data = default!;
    if (enable){
        data = slice<byte>("allow"u8);
    } else {
        data = slice<byte>("deny"u8);
    }
    {
        var (_, errΔ1) = Write(fd, data); if (errΔ1 != default!) {
            Close(fd);
            return errΔ1;
        }
    }
    return Close(fd);
}

// writeUidGidMappings writes User ID and Group ID mappings for user namespaces
// for a process and it is called from the parent process.
internal static error writeUidGidMappings(nint pid, ref SysProcAttr sys) {
    if (sys.UidMappings != default!) {
        @string uidf = "/proc/"u8 + itoa.Itoa(pid) + "/uid_map"u8;
        {
            var err = writeIDMappings(uidf, sys.UidMappings); if (err != default!) {
                return err;
            }
        }
    }
    if (sys.GidMappings != default!) {
        // If the kernel is too old to support /proc/PID/setgroups, writeSetGroups will return ENOENT; this is OK.
        {
            var err = writeSetgroups(pid, sys.GidMappingsEnableSetgroups); if (err != default! && !AreEqual(err, ENOENT)) {
                return err;
            }
        }
        @string gidf = "/proc/"u8 + itoa.Itoa(pid) + "/gid_map"u8;
        {
            var err = writeIDMappings(gidf, sys.GidMappings); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// forkAndExecFailureCleanup cleans up after an exec failure.
internal static void forkAndExecFailureCleanup(ref ProcAttr attr, ref SysProcAttr sys) {
    if (sys.PidFD != nil && sys.PidFD.Value != -1) {
        Close(sys.PidFD.Value);
        sys.PidFD.Value = -1;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cloneClonePidfdFailedToˢ = "clone(CLONE_PIDFD) failed to return pidfd"u8;

// checkClonePidfd verifies that clone(CLONE_PIDFD) works by actually doing a
// clone.
//
//go:linkname os_checkClonePidfd os.checkClonePidfd
internal static error os_checkClonePidfd() {
    GoFrame ᒐ = default;
    try {
        ref var pidfd = ref heap<int32>(out var Ꮡpidfd);
        pidfd = (int32)(-1);
        var (pid, errno) = doCheckClonePidfd(Ꮡpidfd);
        if (errno != 0) {
            return errno;
        }
        if (pidfd == -1) {
            // Bad: CLONE_PIDFD failed to provide a pidfd. Reap the process
            // before returning.
            error err = default!;
            while (ᐧ) {
                ref var status = ref heap(new WaitStatus(), out var Ꮡstatus);
                // WCLONE is an untyped constant that sets bit 31, so
                // it cannot convert directly to int on 32-bit
                // GOARCHes. We must convert through another type
                // first.
                nuint flags = (nuint)WCLONE;
                (_, err) = Wait4((nint)pid, Ꮡstatus, (nint)flags, nil);
                if (!AreEqual(err, EINTR)) {
                    break;
                }
            }
            if (err != default!) {
                return err;
            }
            return errpkg.New(cloneClonePidfdFailedToˢ);
        }
        // Good: CLONE_PIDFD provided a pidfd. Reap the process and close the
        // pidfd.
        defer(Close, (nint)pidfd, ref ᒐ);
        while (ᐧ) {
            uintptr _P_PIDFD = 3;
            (_, _, errno) = Syscall6(SYS_WAITID, _P_PIDFD, (uintptr)pidfd, 0, (uintptr)((uintptr)WEXITED | (uintptr)WCLONE), 0, 0);
            if (errno != EINTR) {
                break;
            }
        }
        if (errno != 0) {
            return errno;
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// doCheckClonePidfd implements the actual clone call of os_checkClonePidfd and
// child execution. This is a separate function so we can separate the child's
// and parent's stack frames if we're using vfork.
//
// This is go:noinline because the point is to keep the stack frames of this
// and os_checkClonePidfd separate.
//
//go:noinline
internal static partial (uintptr pid, Errno errno) doCheckClonePidfd(ж<int32> Ꮡpidfd) {
    uintptr pid = default!;
    Errno errno = default!;

    var flags = (uintptr)((uintptr)((uintptr)(UntypedInt)(CLONE_VFORK | CLONE_VM) | (uintptr)CLONE_PIDFD));
    if (runtime.GOARCH == "s390x"u8){
        // On Linux/s390, the first two arguments of clone(2) are swapped.
        (pid, errno) = rawVforkSyscall(SYS_CLONE, 0, flags, (uintptr)Ꮡpidfd);
    } else {
        (pid, errno) = rawVforkSyscall(SYS_CLONE, flags, 0, (uintptr)Ꮡpidfd);
    }
    if (errno != 0 || pid != 0) {
        // If we're in the parent, we must return immediately
        // so we're not in the same stack frame as the child.
        // This can at most use the return PC, which the child
        // will not modify, and the results of
        // rawVforkSyscall, which must have been written after
        // the child was replaced.
        return (pid, errno);
    }
    while (ᐧ) {
        RawSyscall(SYS_EXIT_GROUP, 0, 0, 0);
    }
}

} // end syscall_package
