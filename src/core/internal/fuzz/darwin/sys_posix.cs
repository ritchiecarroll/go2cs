// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || freebsd || linux || openbsd
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using exec = global::go.os.exec_package;
using Δsyscall = syscall_package;
using fs = global::go.io.fs_package;
using global::go.io;
using global::go.os;

partial class fuzz_package {

partial struct sharedMemSys {
}

internal static (ж<sharedMem>, error) sharedMemMapFile(ж<os.File> f, nint size, bool removeOnClose) {
    nint prot = (nint)((nint)Δsyscall.PROT_READ | (nint)Δsyscall.PROT_WRITE);
    nint flags = (nint)((nint)Δsyscall.MAP_FILE | (nint)Δsyscall.MAP_SHARED);
    var (region, err) = Δsyscall.Mmap((nint)f.Fd(), 0, size, prot, flags);
    if (err != nil) {
        return (default, err);
    }

    return (Ꮡ(new sharedMem(f: f, region: region, removeOnClose: removeOnClose)), default);
}

// Close unmaps the shared memory and closes the temporary file. If this
// sharedMem was created with sharedMemTempFile, Close also removes the file.
internal static error Close(this ref sharedMem m) {
    var errsᴷ = new stackAppend<error>();

    // Attempt all operations, even if we get an error for an earlier operation.
    // os.File.Close may fail due to I/O errors, but we still want to delete
    // the temporary file.
    slice<error> errs = default;
    errs = append(errsᴷ, errs,
        Δsyscall.Munmap(m.region),
        m.f.Close());
    if (m.removeOnClose) {
        errs = append(errs, os.Remove(m.f.Name()));
    }
    foreach (var (_, err) in errs) {
        if (err != nil) {
            return err;
        }
    }
    return default;
}

// setWorkerComm configures communication channels on the cmd that will
// run a worker process.
internal static void setWorkerComm(ref exec.Cmd cmd, workerComm comm) {
    var mem = ᐸꟷ(comm.memMu);
    var memFile = mem.f;
    comm.memMu.ᐸꟷ(mem);
    cmd.ExtraFiles = slice<ж<os.File>>([comm.fuzzIn, comm.fuzzOut, memFile]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fuzzInˢ = "fuzz_in"u8;
internal static readonly @string fuzzOutˢ = "fuzz_out"u8;
internal static readonly @string fuzzMemˢ = "fuzz_mem"u8;

// getWorkerComm returns communication channels in the worker process.
internal static (workerComm comm, error err) getWorkerComm() {
    error err = default;

    var fuzzIn = os.NewFile(3, fuzzInˢ);
    var fuzzOut = os.NewFile(4, fuzzOutˢ);
    var memFile = os.NewFile(5, fuzzMemˢ);
    (var fi, err) = memFile.Stat();
    if (err != nil) {
        return (new workerComm(), err);
    }
    nint size = (nint)fi.Size();
    if ((int64)size != fi.Size()) {
        return (new workerComm(), fmt.Errorf("fuzz temp file exceeds maximum size"u8));
    }
    var removeOnClose = false;
    (var mem, err) = sharedMemMapFile(memFile, size, removeOnClose);
    if (err != nil) {
        return (new workerComm(), err);
    }
    var memMu = new channel<ж<sharedMem>>(1);
    memMu.ᐸꟷ(mem);
    return (new workerComm(fuzzIn: fuzzIn, fuzzOut: fuzzOut, memMu: memMu), default);
}

// isInterruptError returns whether an error was returned by a process that
// was terminated by an interrupt signal (SIGINT).
internal static bool isInterruptError(error err) {
    var (exitErr, ok) = err._<ж<exec.ExitError>>(ᐧ);
    if (!ok || exitErr.Value.ProcessState.ExitCode() >= 0) {
        return false;
    }
    var status = exitErr.Value.ProcessState.Value.Sys()._<Δsyscall.WaitStatus>();
    return status.Signal() == Δsyscall.SIGINT;
}

// terminationSignal checks if err is an exec.ExitError with a signal status.
// If it is, terminationSignal returns the signal and true.
// If not, -1 and false.
internal static (osꓸSignal, bool) terminationSignal(error err) {
    var (exitErr, ok) = err._<ж<exec.ExitError>>(ᐧ);
    if (!ok || exitErr.Value.ProcessState.ExitCode() >= 0) {
        return (new syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)(-1))), false);
    }
    var status = exitErr.Value.ProcessState.Value.Sys()._<Δsyscall.WaitStatus>();
    return (new syscall_ΔSignalᴠΔSignal(status.Signal()), status.Signaled());
}

// isCrashSignal returns whether a signal was likely to have been caused by an
// error in the program that received it, triggered by a fuzz input. For
// example, SIGSEGV would be received after a nil pointer dereference.
// Other signals like SIGKILL or SIGHUP are more likely to have been sent by
// another process, and we shouldn't record a crasher if the worker process
// receives one of these.
//
// Note that Go installs its own signal handlers on startup, so some of these
// signals may only be received if signal handlers are changed. For example,
// SIGSEGV is normally transformed into a panic that causes the process to exit
// with status 2 if not recovered, which we handle as a crash.
internal static bool isCrashSignal(osꓸSignal signal) {
    switch (signal) {
    case var ᴛ when AreEqual(ᴛ, Δsyscall.SIGILL) || AreEqual(ᴛ, Δsyscall.SIGTRAP) || AreEqual(ᴛ, Δsyscall.SIGABRT) || AreEqual(ᴛ, Δsyscall.SIGBUS) || AreEqual(ᴛ, Δsyscall.SIGFPE) || AreEqual(ᴛ, Δsyscall.SIGSEGV) || AreEqual(ᴛ, Δsyscall.SIGPIPE): { // sent data to closed pipe or socket
        // illegal instruction
        // breakpoint
        // abort() called
        // invalid memory access (e.g., misaligned address)
        // math error, e.g., integer divide by zero
        // invalid memory access (e.g., write to read-only)
        return true;
    }
    default: {
        return false;
    }}
}

} // end fuzz_package
