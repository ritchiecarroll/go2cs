// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Support for pidfd was added during the course of a few Linux releases:
//  v5.1: pidfd_send_signal syscall;
//  v5.2: CLONE_PIDFD flag for clone syscall;
//  v5.3: pidfd_open syscall, clone3 syscall;
//  v5.4: P_PIDFD idtype support for waitid syscall;
//  v5.6: pidfd_getfd syscall.
//
// N.B. Alternative Linux implementations may not follow this ordering. e.g.,
// QEMU user mode 7.2 added pidfd_open, but CLONE_PIDFD was not added until
// 8.0.
namespace go;

using unix = @internal.syscall.unix_package;
using runtime = runtime_package;
using Δsync = sync_package;
using syscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal.syscall;

partial class os_package {

// ensurePidfd initializes the PidFD field in sysAttr if it is not already set.
// It returns the original or modified SysProcAttr struct and a flag indicating
// whether the PidFD should be duplicated before using.
internal static (ж<syscall.SysProcAttr>, bool) ensurePidfd(ж<syscall.SysProcAttr> ᏑsysAttr) {
    ref var sysAttr = ref ᏑsysAttr.DerefOrNull();

    if (!pidfdWorks()) {
        return (ᏑsysAttr, false);
    }

    ref var pidfd = ref heap(new nint(), out var Ꮡpidfd);

    if (ᏑsysAttr == nil) {
        return (Ꮡ(new syscall.SysProcAttr(
            PidFD: Ꮡpidfd
        )), false);
    }
    if (sysAttr.PidFD == nil) {
        ref var newSys = ref heap<syscall.SysProcAttr>(out var ᏑnewSys);
        newSys = sysAttr; // copy
        newSys.PidFD = Ꮡpidfd;
        return (ᏑnewSys, false);
    }

    return (ᏑsysAttr, true);
}

// getPidfd returns the value of sysAttr.PidFD (or its duplicate if needDup is
// set) and a flag indicating whether the value can be used.
internal static (uintptr, bool) getPidfd(ref syscall.SysProcAttr sysAttr, bool needDup) {
    if (!pidfdWorks()) {
        return (0, false);
    }

    nint h = sysAttr.PidFD.Value;
    if (needDup) {
        var (dupH, e) = unix.Fcntl(h, syscall.F_DUPFD_CLOEXEC, 0);
        if (e != default!) {
            return (0, false);
        }
        h = dupH;
    }
    return ((uintptr)h, true);
}

// pidfdFind returns the process handle for pid.
internal static (uintptr, error) pidfdFind(nint pid) {
    if (!pidfdWorks()) {
        return (0, syscall.ENOSYS);
    }

    var (h, err) = unix.PidFDOpen(pid, 0);
    if (err != default!) {
        return (0, convertESRCH(err));
    }
    return (h, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string waitidˢ = "waitid"u8;

// pidfdWait waits for the process to complete,
// and updates the process status to done.
internal static (ж<ProcessState>, error) pidfdWait(this ж<Process> Ꮡp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        // When pidfd is used, there is no wait/kill race (described in CL 23967)
        // because the PID recycle issue doesn't exist (IOW, pidfd, unlike PID,
        // is guaranteed to refer to one particular process). Thus, there is no
        // need for the workaround (blockUntilWaitable + sigMu) from pidWait.
        //
        // We _do_ need to be careful about reuse of the pidfd FD number when
        // closing the pidfd. See handle for more details.
        var (handle, status) = Ꮡp.handleTransientAcquire();
        var exprᴛ1 = status;
        if (exprᴛ1 == statusDone) {
            // Process already completed Wait, or was not found by
            // pidfdFind. Return ECHILD for consistency with what the wait
            // syscall would return.
            return (default!, NewSyscallError(waitˢ, syscall.ECHILD));
        }
        if (exprᴛ1 == statusReleased) {
            return (default!, syscall.EINVAL);
        }
        ᒐd1 = true;

        ref var info = ref heap(new unix.SiginfoChild(), out var Ꮡinfo);
        ref var rusage = ref heap(new syscall.Rusage(), out var Ꮡrusage);
        var err = ignoringEINTR(() => unix.Waitid(unix.P_PIDFD, (nint)handle, Ꮡinfo, syscall.WEXITED, Ꮡrusage));
        if (err != default!) {
            return (default!, NewSyscallError(waitidˢ, err));
        }

        // Update the Process status to statusDone.
        // This also releases a reference to the handle.
        Ꮡp.doRelease(statusDone);

        return (Ꮡ(new ProcessState(
            pid: (nint)info.Pid,
            status: info.WaitStatus(),
            rusage: Ꮡrusage
        )), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) p.handleTransientRelease(); ᒐ.Run(); }
}

// pidfdSendSignal sends a signal to the process.
internal static error pidfdSendSignal(this ж<Process> Ꮡp, syscallꓸSignal s) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        var (handle, status) = Ꮡp.handleTransientAcquire();
        var exprᴛ1 = status;
        if (exprᴛ1 == statusDone) {
            return ErrProcessDone;
        }
        if (exprᴛ1 == statusReleased) {
            return errProcessReleased;
        }
        ᒐd1 = true;

        return convertESRCH(unix.PidFDSendSignal(handle, s));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) p.handleTransientRelease(); ᒐ.Run(); }
}

// pidfdWorks returns whether we can use pidfd on this system.
internal static bool pidfdWorks() {
    return checkPidfdOnce() == default!;
}

// checkPidfdOnce is used to only check whether pidfd works once.
internal static Func<error> checkPidfdOnce;
internal static void initᴛcheckPidfdOnce() { checkPidfdOnce = Δsync.OnceValue<error>(checkPidfd); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pidfdOpenˢ = "pidfd_open"u8;
internal static readonly @string pidfdWaitˢ = "pidfd_wait"u8;
internal static readonly @string pidfdSendSignalˢ = "pidfd_send_signal"u8;

// checkPidfd checks whether all required pidfd-related syscalls work. This
// consists of pidfd_open and pidfd_send_signal syscalls, waitid syscall with
// idtype of P_PIDFD, and clone(CLONE_PIDFD).
//
// Reasons for non-working pidfd syscalls include an older kernel and an
// execution environment in which the above system calls are restricted by
// seccomp or a similar technology.
internal static error checkPidfd() {
    GoFrame ᒐ = default;
    try {
        // In Android version < 12, pidfd-related system calls are not allowed
        // by seccomp and trigger the SIGSYS signal. See issue #69065.
        if (runtime.GOOS == "android"u8) {
            ignoreSIGSYS();
            defer(restoreSIGSYS, ref ᒐ);
        }

        // Get a pidfd of the current process (opening of "/proc/self" won't
        // work for waitid).
        var (fd, err) = unix.PidFDOpen(syscall.Getpid(), 0);
        if (err != default!) {
            return NewSyscallError(pidfdOpenˢ, err);
        }
        defer(syscall.Close, (nint)fd, ref ᒐ);

        // Check waitid(P_PIDFD) works.
        err = ignoringEINTR(() => {
            ref var info = ref heap(new unix.SiginfoChild(), out var Ꮡinfo);
            // We don't actually care about the info, but passing a nil pointer
            // makes valgrind complain because 0x0 is unaddressable.
            return unix.Waitid(unix.P_PIDFD, (nint)fd, Ꮡinfo, syscall.WEXITED, nil);
        });
        // Expect ECHILD from waitid since we're not our own parent.
        if (!AreEqual(err, syscall.ECHILD)) {
            return NewSyscallError(pidfdWaitˢ, err);
        }

        // Check pidfd_send_signal works (should be able to send 0 to itself).
        {
            var errΔ1 = unix.PidFDSendSignal(fd, 0); if (errΔ1 != default!) {
                return NewSyscallError(pidfdSendSignalˢ, errΔ1);
            }
        }

        // Verify that clone(CLONE_PIDFD) works.
        //
        // This shouldn't be necessary since pidfd_open was added in Linux 5.3,
        // after CLONE_PIDFD in Linux 5.2, but some alternative Linux
        // implementations may not adhere to this ordering.
        {
            var errΔ2 = checkClonePidfd(); if (errΔ2 != default!) {
                return errΔ2;
            }
        }

        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Provided by syscall.
//
//go:linkname checkClonePidfd
internal static partial error checkClonePidfd();

// Provided by runtime.
//
//go:linkname ignoreSIGSYS
internal static partial void ignoreSIGSYS();

//go:linkname restoreSIGSYS
internal static partial void restoreSIGSYS();

} // end os_package
