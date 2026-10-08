// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using testlog = @internal.testlog_package;
using runtime = runtime_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using syscall = syscall_package;
using time = time_package;
using @internal;
using go.sync;

partial class os_package {

// ErrProcessDone indicates a [Process] has finished.
public static error ErrProcessDone = errors.New("os: process already finished"u8);

partial struct processMode /*num:uint8*/;

internal static processMode modePID => /* iota */ 0;
internal static processMode modeHandle => 1;

public partial struct processStatus /*num:uint64*/;

internal static processStatus statusOK => 0;
internal static processStatus statusDone => /* 1 << 62 */ unchecked((processStatus)4611686018427387904);
internal static processStatus statusReleased => /* 1 << 63 */ unchecked((processStatus)9223372036854775808);
internal static UntypedInt processStatusMask => /* 0x3 << 62 */ 13835058055282163712;

// Process stores the information about a process created by [StartProcess].
partial struct Process {
    public nint Pid;
    internal processMode mode;
    // State contains the atomic process state.
    //
    // In modePID, this consists only of the processStatus fields, which
    // indicate if the process is done/released.
    //
    // In modeHandle, the lower bits also contain a reference count for the
    // handle field.
    //
    // The Process itself initially holds 1 persistent reference. Any
    // operation that uses the handle with a system call temporarily holds
    // an additional transient reference. This prevents the handle from
    // being closed prematurely, which could result in the OS allocating a
    // different handle with the same value, leading to Process' methods
    // operating on the wrong process.
    //
    // Release and Wait both drop the Process' persistent reference, but
    // other concurrent references may delay actually closing the handle
    // because they hold a transient reference.
    //
    // Regardless, we want new method calls to immediately treat the handle
    // as unavailable after Release or Wait to avoid extending this delay.
    // This is achieved by setting either processStatus flag when the
    // Process' persistent reference is dropped. The only difference in the
    // flags is the reason the handle is unavailable, which affects the
    // errors returned by concurrent calls.
    internal atomic.Uint64 state;
    // Used only in modePID.
    internal Δsync.RWMutex sigMu; // avoid race between wait and signal
    // handle is the OS handle for process actions, used only in
    // modeHandle.
    //
    // handle must be accessed only via the handleTransientAcquire method
    // (or during closeHandle), not directly! handle is immutable.
    //
    // On Windows, it is a handle from OpenProcess.
    // On Linux, it is a pidfd.
    // It is unused on other GOOSes.
    internal uintptr handle;
}

internal static ж<Process> newPIDProcess(nint pid) {
    var p = Ꮡ(new Process(
        Pid: pid,
        mode: modePID
    ));
    runtime.SetFinalizer(p.OrTypedNil(), ((Func<ж<Process>, error>)(Release)));
    return p;
}

internal static ж<Process> newHandleProcess(nint pid, uintptr handle) {
    var p = Ꮡ(new Process(
        Pid: pid,
        mode: modeHandle,
        handle: handle
    ));
    p.of(Process.Ꮡstate).Store(1); // 1 persistent reference
    runtime.SetFinalizer(p.OrTypedNil(), ((Func<ж<Process>, error>)(Release)));
    return p;
}

internal static ж<Process> newDoneProcess(nint pid) {
    var p = Ꮡ(new Process(
        Pid: pid,
        mode: modeHandle
    ));
    // N.B Since we set statusDone, handle will never actually be
    // used, so its value doesn't matter.
    p.of(Process.Ꮡstate).Store((uint64)statusDone); // No persistent reference, as there is no handle.
    runtime.SetFinalizer(p.OrTypedNil(), ((Func<ж<Process>, error>)(Release)));
    return p;
}

internal static (uintptr, processStatus) handleTransientAcquire(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.mode != modeHandle) {
        throw panic("handleTransientAcquire called in invalid mode");
    }
    while (ᐧ) {
        var refs = Ꮡp.of(Process.Ꮡstate).Load();
        if ((uint64)(refs & (uint64)processStatusMask) != 0) {
            return (0, ((processStatus)((uint64)(refs & (uint64)processStatusMask))));
        }
        var @new = refs + 1;
        if (!Ꮡp.of(Process.Ꮡstate).CompareAndSwap(refs, @new)) {
            continue;
        }
        return (p.handle, statusOK);
    }
}

internal static void handleTransientRelease(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.mode != modeHandle) {
        throw panic("handleTransientRelease called in invalid mode");
    }
    while (ᐧ) {
        var state = Ꮡp.of(Process.Ꮡstate).Load();
        var refs = (uint64)(state & ~(uint64)processStatusMask);
        var status = ((processStatus)((uint64)(state & (uint64)processStatusMask)));
        if (refs == 0) {
            // This should never happen because
            // handleTransientRelease is always paired with
            // handleTransientAcquire.
            throw panic("release of handle with refcount 0");
        }
        if (refs == 1 && status == statusOK) {
            // Process holds a persistent reference and always sets
            // a status when releasing that reference
            // (handlePersistentRelease). Thus something has gone
            // wrong if this is the last release but a status has
            // not always been set.
            throw panic("final release of handle without processStatus");
        }
        var @new = state - 1;
        if (!Ꮡp.of(Process.Ꮡstate).CompareAndSwap(state, @new)) {
            continue;
        }
        if ((uint64)(@new & ~(uint64)processStatusMask) == 0) {
            p.closeHandle();
        }
        return;
    }
}

// Drop the Process' persistent reference on the handle, deactivating future
// Wait/Signal calls with the passed reason.
//
// Returns the status prior to this call. If this is not statusOK, then the
// reference was not dropped or status changed.
internal static processStatus handlePersistentRelease(this ж<Process> Ꮡp, processStatus reason) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.mode != modeHandle) {
        throw panic("handlePersistentRelease called in invalid mode");
    }
    while (ᐧ) {
        var refs = Ꮡp.of(Process.Ꮡstate).Load();
        var status = ((processStatus)((uint64)(refs & (uint64)processStatusMask)));
        if (status != statusOK) {
            // Both Release and successful Wait will drop the
            // Process' persistent reference on the handle. We
            // can't allow concurrent calls to drop the reference
            // twice, so we use the status as a guard to ensure the
            // reference is dropped exactly once.
            return status;
        }
        if (refs == 0) {
            // This should never happen because dropping the
            // persistent reference always sets a status.
            throw panic("release of handle with refcount 0");
        }
        var @new = (uint64)((refs - 1) | (uint64)reason);
        if (!Ꮡp.of(Process.Ꮡstate).CompareAndSwap(refs, @new)) {
            continue;
        }
        if ((uint64)(@new & ~(uint64)processStatusMask) == 0) {
            p.closeHandle();
        }
        return status;
    }
}

internal static processStatus pidStatus(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.mode != modePID) {
        throw panic("pidStatus called in invalid mode");
    }
    return ((processStatus)Ꮡp.of(Process.Ꮡstate).Load());
}

internal static void pidDeactivate(this ж<Process> Ꮡp, processStatus reason) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.mode != modePID) {
        throw panic("pidDeactivate called in invalid mode");
    }
    // Both Release and successful Wait will deactivate the PID. Only one
    // of those should win, so nothing left to do here if the compare
    // fails.
    //
    // N.B. This means that results can be inconsistent. e.g., with a
    // racing Release and Wait, Wait may successfully wait on the process,
    // returning the wait status, while future calls error with "process
    // released" rather than "process done".
    Ꮡp.of(Process.Ꮡstate).CompareAndSwap(0, (uint64)reason);
}

// ProcAttr holds the attributes that will be applied to a new process
// started by StartProcess.
partial struct ProcAttr {
    // If Dir is non-empty, the child changes into the directory before
    // creating the process.
    public @string Dir;
    // If Env is non-nil, it gives the environment variables for the
    // new process in the form returned by Environ.
    // If it is nil, the result of Environ will be used.
    public slice<@string> Env;
    // Files specifies the open files inherited by the new process. The
    // first three entries correspond to standard input, standard output, and
    // standard error. An implementation may support additional entries,
    // depending on the underlying operating system. A nil entry corresponds
    // to that file being closed when the process starts.
    // On Unix systems, StartProcess will change these File values
    // to blocking mode, which means that SetDeadline will stop working
    // and calling Close will not interrupt a Read or Write.
    public slice<ж<File>> Files;
    // Operating system-specific process creation attributes.
    // Note that setting this field means that your program
    // may not execute properly or even compile on some
    // operating systems.
    public ж<syscall.SysProcAttr> Sys;
}

// A Signal represents an operating system signal.
// The usual underlying implementation is operating system-dependent:
// on Unix it is syscall.Signal.
partial interface ΔSignal {
    @string String();
    void Signal(); // to distinguish from other Stringers
}

// Getpid returns the process id of the caller.
public static nint Getpid() {
    return syscall.Getpid();
}

// Getppid returns the process id of the caller's parent.
public static nint Getppid() {
    return syscall.Getppid();
}

// FindProcess looks for a running process by its pid.
//
// The [Process] it returns can be used to obtain information
// about the underlying operating system process.
//
// On Unix systems, FindProcess always succeeds and returns a Process
// for the given pid, regardless of whether the process exists. To test whether
// the process actually exists, see whether p.Signal(syscall.Signal(0)) reports
// an error.
public static (ж<Process>, error) FindProcess(nint pid) {
    return findProcess(pid);
}

// StartProcess starts a new process with the program, arguments and attributes
// specified by name, argv and attr. The argv slice will become [os.Args] in the
// new process, so it normally starts with the program name.
//
// If the calling goroutine has locked the operating system thread
// with [runtime.LockOSThread] and modified any inheritable OS-level
// thread state (for example, Linux or Plan 9 name spaces), the new
// process will inherit the caller's thread state.
//
// StartProcess is a low-level interface. The [os/exec] package provides
// higher-level interfaces.
//
// If there is an error, it will be of type [*PathError].
public static (ж<Process>, error) StartProcess(@string name, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    testlog.Open(name);
    return startProcess(name, argv, Ꮡattr);
}

// Release releases any resources associated with the [Process] p,
// rendering it unusable in the future.
// Release only needs to be called if [Process.Wait] is not.
public static error Release(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Note to future authors: the Release API is cursed.
    //
    // On Unix and Plan 9, Release sets p.Pid = -1. This is the only part of the
    // Process API that is not thread-safe, but it can't be changed now.
    //
    // On Windows, Release does _not_ modify p.Pid.
    //
    // On Windows, Wait calls Release after successfully waiting to
    // proactively clean up resources.
    //
    // On Unix and Plan 9, Wait also proactively cleans up resources, but
    // can not call Release, as Wait does not set p.Pid = -1.
    //
    // On Unix and Plan 9, calling Release a second time has no effect.
    //
    // On Windows, calling Release a second time returns EINVAL.
    return Ꮡp.release();
}

// Kill causes the [Process] to exit immediately. Kill does not wait until
// the Process has actually exited. This only kills the Process itself,
// not any other processes it may have started.
public static error Kill(this ж<Process> Ꮡp) {
    return Ꮡp.kill();
}

// Wait waits for the [Process] to exit, and then returns a
// ProcessState describing its status and an error, if any.
// Wait releases any resources associated with the Process.
// On most operating systems, the Process must be a child
// of the current process or an error will be returned.
public static (ж<ProcessState>, error) Wait(this ж<Process> Ꮡp) {
    return Ꮡp.wait();
}

// Signal sends a signal to the [Process].
// Sending [Interrupt] on Windows is not implemented.
public static error Signal(this ж<Process> Ꮡp, ΔSignal sig) {
    return Ꮡp.signal(sig);
}

// UserTime returns the user CPU time of the exited process and its children.
public static time.Duration UserTime(this ref ProcessState p) {
    return p.userTime();
}

// SystemTime returns the system CPU time of the exited process and its children.
public static time.Duration SystemTime(this ref ProcessState p) {
    return p.systemTime();
}

// Exited reports whether the program has exited.
// On Unix systems this reports true if the program exited due to calling exit,
// but false if the program terminated due to a signal.
public static bool Exited(this ref ProcessState p) {
    return p.exited();
}

// Success reports whether the program exited successfully,
// such as with exit status 0 on Unix.
public static bool Success(this ref ProcessState p) {
    return p.success();
}

// Sys returns system-dependent exit information about
// the process. Convert it to the appropriate underlying
// type, such as [syscall.WaitStatus] on Unix, to access its contents.
public static any Sys(this ref ProcessState p) {
    return p.sys();
}

// SysUsage returns system-dependent resource usage information about
// the exited process. Convert it to the appropriate underlying
// type, such as [*syscall.Rusage] on Unix, to access its contents.
// (On Unix, *syscall.Rusage matches struct rusage as defined in the
// getrusage(2) manual page.)
public static any SysUsage(this ref ProcessState p) {
    return p.sysUsage();
}

} // end os_package
