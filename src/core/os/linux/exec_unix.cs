// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1
namespace go;

using errors = errors_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;

partial class os_package {

internal static UntypedInt pidUnset => 0;
internal static UntypedInt pidReleased => -1;

internal static (ж<ProcessState> ps, error err) wait(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Which type of Process do we have?
    var exprᴛ1 = p.mode;
    if (exprᴛ1 == modeHandle) {
        return Ꮡp.pidfdWait();
    }
    if (exprᴛ1 == modePID) {
        return Ꮡp.pidWait();
    }
    { /* default: */
        throw panic("unreachable");
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string waitˢ = "wait"u8;

// pidfd
// Regular PID
internal static (ж<ProcessState>, error) pidWait(this ж<Process> Ꮡp) {
    // TODO(go.dev/issue/67642): When there are concurrent Wait calls, one
    // may wait on the wrong process if the PID is reused after the
    // completes its wait.
    //
    // Checking for statusDone here would not be a complete fix, as the PID
    // could still be waited on and reused prior to blockUntilWaitable.
    var exprᴛ1 = Ꮡp.pidStatus();
    if (exprᴛ1 == statusReleased) {
        return (default!, syscall.EINVAL);
    }

    // If we can block until Wait4 will succeed immediately, do so.
    var (ready, err) = Ꮡp.blockUntilWaitable();
    if (err != default!) {
        return (default!, err);
    }
    if (ready) {
        // Mark the process done now, before the call to Wait4,
        // so that Process.pidSignal will not send a signal.
        Ꮡp.pidDeactivate(statusDone);
        // Acquire a write lock on sigMu to wait for any
        // active call to the signal method to complete.
        Ꮡp.of(Process.ᏑsigMu).Lock();
        Ꮡp.of(Process.ᏑsigMu).Unlock();
    }
    ref var status = ref heap(new syscall.WaitStatus(), out var Ꮡstatus);
    ref var rusage = ref heap(new syscall.Rusage(), out var Ꮡrusage);
    ref var pid1 = ref heap<nint>(out var Ꮡpid1);
    (pid1, err) = ignoringEINTR2((nint, error) () => syscall.Wait4(Ꮡp.Value.Pid, Ꮡstatus, 0, Ꮡrusage));
    if (err != default!) {
        return (default!, NewSyscallError(waitˢ, err));
    }
    Ꮡp.pidDeactivate(statusDone);
    return (Ꮡ(new ProcessState(
        pid: pid1,
        status: status,
        rusage: Ꮡrusage
    )), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string osUnsupportedSignalTypeˢ = "os: unsupported signal type"u8;

internal static error signal(this ж<Process> Ꮡp, ΔSignal sig) {
    ref var p = ref Ꮡp.DerefOrNull();

    var (s, ok) = sig._<syscallꓸSignal>(ᐧ);
    if (!ok) {
        return errors.New(osUnsupportedSignalTypeˢ);
    }
    // Which type of Process do we have?
    var exprᴛ1 = p.mode;
    if (exprᴛ1 == modeHandle) {
        return Ꮡp.pidfdSendSignal(s);
    }
    if (exprᴛ1 == modePID) {
        return Ꮡp.pidSignal(s);
    }
    { /* default: */
        throw panic("unreachable");
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string osProcessAlreadyReleasedˢ = "os: process already released"u8;
internal static readonly @string osProcessNotInitializedˢ = "os: process not initialized"u8;

// pidfd
// Regular PID
internal static error pidSignal(this ж<Process> Ꮡp, syscallꓸSignal s) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        if (p.Pid == pidReleased) {
            return errors.New(osProcessAlreadyReleasedˢ);
        }
        if (p.Pid == pidUnset) {
            return errors.New(osProcessNotInitializedˢ);
        }
        Ꮡp.of(Process.ᏑsigMu).RLock();
        ᒐd1 = true;
        var exprᴛ1 = Ꮡp.pidStatus();
        if (exprᴛ1 == statusDone) {
            return ErrProcessDone;
        }
        if (exprᴛ1 == statusReleased) {
            return errors.New(osProcessAlreadyReleasedˢ);
        }

        return convertESRCH(syscall.Kill(p.Pid, s));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡp.of(Process.ᏑsigMu).RUnlock(); ᒐ.Run(); }
}

internal static error convertESRCH(error err) {
    if (AreEqual(err, syscall.ESRCH)) {
        return ErrProcessDone;
    }
    return err;
}

internal static error release(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // We clear the Pid field only for API compatibility. On Unix, Release
    // has always set Pid to -1. Internally, the implementation relies
    // solely on statusReleased to determine that the Process is released.
    p.Pid = pidReleased;
    var exprᴛ1 = p.mode;
    if (exprᴛ1 == modeHandle) {
        Ꮡp.handlePersistentRelease(statusReleased);
    }
    else if (exprᴛ1 == modePID) {
        Ꮡp.pidDeactivate(statusReleased);
    }

    // Drop the Process' reference and mark handle unusable for
    // future calls.
    //
    // Ignore the return value: we don't care if this was a no-op
    // racing with Wait, or a double Release.
    // Just mark the PID unusable.
    // no need for a finalizer anymore
    runtime.SetFinalizer(Ꮡp.OrTypedNil(), default!);
    return default!;
}

internal static (ж<Process> p, error err) findProcess(nint pid) {
    error err = default!;

    (var h, err) = pidfdFind(pid);
    if (AreEqual(err, ErrProcessDone)){
        // We can't return an error here since users are not expecting
        // it. Instead, return a process with a "done" state already
        // and let a subsequent Signal or Wait call catch that.
        return (newDoneProcess(pid), default!);
    } else 
    if (err != default!) {
        // Ignore other errors from pidfdFind, as the callers
        // do not expect them. Fall back to using the PID.
        return (newPIDProcess(pid), default!);
    }
    // Use the handle.
    return (newHandleProcess(pid, h), default!);
}

internal static time.Duration userTime(this ref ProcessState p) {
    return ((time.Duration)p.rusage.of(syscall.Rusage.ᏑUtime).Nano()) * time.ΔNanosecond;
}

internal static time.Duration systemTime(this ref ProcessState p) {
    return ((time.Duration)p.rusage.of(syscall.Rusage.ᏑStime).Nano()) * time.ΔNanosecond;
}

} // end os_package
