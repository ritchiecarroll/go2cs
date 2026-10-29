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
// errProcessReleased indicates a [Process] has been released.
internal static error errProcessReleased = errors.New("os: process already released"u8);
// ErrNoHandle indicates a [Process] does not have a handle.
public static error ErrNoHandle = errors.New("os: process handle unavailable"u8);

// processStatus describes the status of a [Process].
public partial struct processStatus /*num:uint32*/;

// statusOK means that the Process is ready to use.
internal static processStatus statusOK => /* iota */ 0;

// statusDone indicates that the PID/handle should not be used because
// the process is done (has been successfully Wait'd on).
internal static processStatus statusDone => 1;

// statusReleased indicates that the PID/handle should not be used
// because the process is released.
internal static processStatus statusReleased => 2;

// Process stores the information about a process created by [StartProcess].
partial struct Process {
    public nint Pid;
    // state contains the atomic process state.
    //
    // This consists of the processStatus fields,
    // which indicate if the process is done/released.
    internal atomic.Uint32 state;
    // Used only when handle is nil
    internal Δsync.RWMutex sigMu; // avoid race between wait and signal
    // handle, if not nil, is a pointer to a struct
    // that holds the OS-specific process handle.
    // This pointer is set when Process is created,
    // and never changed afterward.
    // This is a pointer to a separate memory allocation
    // so that we can use runtime.AddCleanup.
    internal ж<processHandle> handle;
    // cleanup is used to clean up the process handle.
    internal runtime.Cleanup cleanup;
}

// processHandle holds an operating system handle to a process.
// This is only used on systems that support that concept,
// currently Linux and Windows.
// This maintains a reference count to the handle,
// and closes the handle when the reference drops to zero.
partial struct processHandle {
    // The actual handle. This field should not be used directly.
    // Instead, use the acquire and release methods.
    //
    // On Windows this is a handle returned by OpenProcess.
    // On Linux this is a pidfd.
    internal uintptr handle;
    // Number of active references. When this drops to zero
    // the handle is closed.
    internal atomic.Int32 refs;
}

// acquire adds a reference and returns the handle.
// The bool result reports whether acquire succeeded;
// it fails if the handle is already closed.
// Every successful call to acquire should be paired with a call to release.
internal static (uintptr, bool) acquire(this ж<processHandle> Ꮡph) {
    ref var ph = ref Ꮡph.DerefOrNull();

    for (;;) {
        var refs = Ꮡph.of(processHandle.Ꮡrefs).Load();
        if (refs < 0) {
            throw panic("internal error: negative process handle reference count");
        }
        if (refs == 0) {
            return (0, false);
        }
        if (Ꮡph.of(processHandle.Ꮡrefs).CompareAndSwap(refs, refs + 1)) {
            return (ph.handle, true);
        }
    }
}

// release releases a reference to the handle.
internal static void release(this ж<processHandle> Ꮡph) {
    ref var ph = ref Ꮡph.DerefOrNull();

    for (;;) {
        var refs = Ꮡph.of(processHandle.Ꮡrefs).Load();
        if (refs <= 0) {
            throw panic("internal error: too many releases of process handle");
        }
        if (Ꮡph.of(processHandle.Ꮡrefs).CompareAndSwap(refs, refs - 1)) {
            if (refs == 1) {
                ph.closeHandle();
            }
            return;
        }
    }
}

// newPIDProcess returns a [Process] for the given PID.
internal static ж<Process> newPIDProcess(nint pid) {
    var p = Ꮡ(new Process(
        Pid: pid
    ));
    return p;
}

// newHandleProcess returns a [Process] with the given PID and handle.
internal static ж<Process> newHandleProcess(nint pid, uintptr handle) {
    var ph = Ꮡ(new processHandle(
        handle: handle
    ));

    // Start the reference count as 1,
    // meaning the reference from the returned Process.
    ph.of(processHandle.Ꮡrefs).Store(1);

    var p = Ꮡ(new Process(
        Pid: pid,
        handle: ph
    ));

    p.cleanup = runtime.AddCleanup<Process, ж<processHandle>>(p, ((Action<ж<processHandle>>)(release)), ph);

    return p;
}

// newDoneProcess returns a [Process] for the given PID
// that is already marked as done. This is used on Unix systems
// if the process is known to not exist.
internal static ж<Process> newDoneProcess(nint pid) {
    var p = Ꮡ(new Process(
        Pid: pid
    ));
    p.of(Process.Ꮡstate).Store((uint32)statusDone); // No persistent reference, as there is no handle.
    return p;
}

// handleTransientAcquire returns the process handle or,
// if the process is not ready, the current status.
internal static (uintptr, processStatus) handleTransientAcquire(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.handle == nil) {
        throw panic("handleTransientAcquire called in invalid mode");
    }

    var status = (processStatus)Ꮡp.of(Process.Ꮡstate).Load();
    if (status != statusOK) {
        return (0, status);
    }
    var (h, ok) = p.handle.acquire();
    if (ok) {
        return (h, statusOK);
    }

    // This case means that the handle has been closed.
    // We always set the status to non-zero before closing the handle.
    // If we get here the status must have been set non-zero after
    // we just checked it above.
    status = (processStatus)Ꮡp.of(Process.Ꮡstate).Load();
    if (status == statusOK) {
        throw panic("inconsistent process status");
    }
    return (0, status);
}

// handleTransientRelease releases a handle returned by handleTransientAcquire.
internal static void handleTransientRelease(this ref Process p) {
    if (p.handle == nil) {
        throw panic("handleTransientRelease called in invalid mode");
    }
    p.handle.release();
}

// pidStatus returns the current process status.
internal static processStatus pidStatus(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.handle != nil) {
        throw panic("pidStatus called in invalid mode");
    }

    return (processStatus)Ꮡp.of(Process.Ꮡstate).Load();
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
public static (ж<Process>, error) StartProcess(@string name, slice<@string> argv, ж<ProcAttr> attr) {
    testlog.Open(name);
    return startProcess(name, argv, attr);
}

// Release releases any resources associated with the [Process] p,
// rendering it unusable in the future.
// Release only needs to be called if [Process.Wait] is not.
public static error Release(this ж<Process> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // Unfortunately, for historical reasons, on systems other
    // than Windows, Release sets the Pid field to -1.
    // This causes the race detector to report a problem
    // on concurrent calls to Release, but we can't change it now.
    if (runtime.GOOS != "windows"u8) {
        p.Pid = -1;
    }

    var oldStatus = Ꮡp.doRelease(statusReleased);

    // For backward compatibility, on Windows only,
    // we return EINVAL on a second call to Release.
    if (runtime.GOOS == "windows"u8) {
        if (oldStatus == statusReleased) {
            return syscall.EINVAL;
        }
    }

    return default;
}

// doRelease releases a [Process], setting the status to newStatus.
// If the previous status is not statusOK, this does nothing.
// It returns the previous status.
internal static processStatus doRelease(this ж<Process> Ꮡp, processStatus newStatus) {
    ref var p = ref Ꮡp.DerefOrNull();

    for (;;) {
        var state = Ꮡp.of(Process.Ꮡstate).Load();
        var oldStatus = (processStatus)state;
        if (oldStatus != statusOK) {
            return oldStatus;
        }

        if (!Ꮡp.of(Process.Ꮡstate).CompareAndSwap(state, (uint32)newStatus)) {
            continue;
        }

        // We have successfully released the Process.
        // If it has a handle, release the reference we
        // created in newHandleProcess.
        if (p.handle != nil) {
            // No need for more cleanup.
            // We must stop the cleanup before calling release;
            // otherwise the cleanup might run concurrently
            // with the release, which would cause the reference
            // counts to be invalid, causing a panic.
            p.cleanup.Stop();

            p.handle.release();
        }

        return statusOK;
    }
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

// WithHandle calls a supplied function f with a valid process handle
// as an argument. The handle is guaranteed to refer to process p
// until f returns, even if p terminates. This function cannot be used
// after [Process.Release] or [Process.Wait].
//
// If process handles are not supported or a handle is not available,
// it returns [ErrNoHandle]. Currently, process handles are supported
// on Linux 5.4 or later (pidfd) and Windows.
public static error WithHandle(this ж<Process> Ꮡp, Action<uintptr> f) {
    return Ꮡp.withHandle(f);
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
