// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using exec = global::go.os.exec_package;
using Δsyscall = syscall_package;
using @unsafe = unsafe_package;
using fs = global::go.io.fs_package;
using global::go.io;
using global::go.os;

partial class fuzz_package {

partial struct sharedMemSys {
    internal syscallꓸHandle mapObj;
}

internal static (ж<sharedMem> mem, error err) sharedMemMapFile(ж<os.File> Ꮡf, nint size, bool removeOnClose) {
    ж<sharedMem> mem = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        defer(() => {
            if (Ꮡerr.ValueSlot != default!) {
                Ꮡerr.ValueSlot = fmt.Errorf("mapping temporary file %s: %w"u8, Ꮡf.Value.Name(), Ꮡerr.ValueSlot);
            }
        }, ref ᒐ);
        // Create a file mapping object. The object itself is not shared.
        (var mapObj, err) = Δsyscall.CreateFileMapping(
            ((syscallꓸHandle)Ꮡf.Fd()), // fhandle

            nil, // sa

            Δsyscall.PAGE_READWRITE, // prot

            0, // maxSizeHigh

            0, // maxSizeLow

            nil);
        // name
        if (err != default!) {
            (mem, err) = (default!, err); goto ᒐdone;
        }
        // Create a view from the file mapping object.
        var access = (uint32)((uint32)((uint32)Δsyscall.FILE_MAP_READ | (uint32)Δsyscall.FILE_MAP_WRITE));
        (var addr, err) = Δsyscall.MapViewOfFile(
            mapObj, // handle

            access, // access

            0, // offsetHigh

            0, // offsetLow

            (uintptr)size);
        // length
        if (err != default!) {
            Δsyscall.CloseHandle(mapObj);
            (mem, err) = (default!, err); goto ᒐdone;
        }
        var region = @unsafe.Slice((ж<byte>)(uintptr)((@unsafe.Pointer)addr), size);
        (mem, err) = (Ꮡ(new sharedMem(
            f: Ꮡf,
            region: region,
            removeOnClose: removeOnClose,
            sys: new sharedMemSys(mapObj: mapObj)
        )), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (mem, Ꮡerr.ValueSlot);
}

// Close unmaps the shared memory and closes the temporary file. If this
// sharedMem was created with sharedMemTempFile, Close also removes the file.
internal static error Close(this ref sharedMem m) {
    // Attempt all operations, even if we get an error for an earlier operation.
    // os.File.Close may fail due to I/O errors, but we still want to delete
    // the temporary file.
    slice<error> errs = default!;
    errs = append(errs,
        Δsyscall.UnmapViewOfFile((uintptr)Ꮡ(m.region, 0)),
        Δsyscall.CloseHandle(m.sys.mapObj),
        m.f.Close());
    if (m.removeOnClose) {
        errs = append(errs, os.Remove(m.f.Name()));
    }
    foreach (var (_, err) in errs) {
        if (err != default!) {
            return err;
        }
    }
    return default!;
}

// setWorkerComm configures communication channels on the cmd that will
// run a worker process.
internal static void setWorkerComm(ref exec.Cmd cmd, workerComm comm) {
    var mem = ᐸꟷ(comm.memMu);
    var memFD = (~mem).f.Fd();
    comm.memMu.ᐸꟷ(mem);
    Δsyscall.SetHandleInformation(((syscallꓸHandle)comm.fuzzIn.Fd()), Δsyscall.HANDLE_FLAG_INHERIT, 1);
    Δsyscall.SetHandleInformation(((syscallꓸHandle)comm.fuzzOut.Fd()), Δsyscall.HANDLE_FLAG_INHERIT, 1);
    Δsyscall.SetHandleInformation(((syscallꓸHandle)memFD), Δsyscall.HANDLE_FLAG_INHERIT, 1);
    cmd.Env = append(cmd.Env, fmt.Sprintf("GO_TEST_FUZZ_WORKER_HANDLES=%x,%x,%x"u8, comm.fuzzIn.Fd(), comm.fuzzOut.Fd(), memFD));
    cmd.SysProcAttr = Ꮡ(new Δsyscall.SysProcAttr(AdditionalInheritedHandles: new syscallꓸHandle[]{((syscallꓸHandle)comm.fuzzIn.Fd()), ((syscallꓸHandle)comm.fuzzOut.Fd()), ((syscallꓸHandle)memFD)}.slice()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goTestFuzzWorkerHandlesˢ = "GO_TEST_FUZZ_WORKER_HANDLES"u8;
internal static readonly @string fuzzInˢ = "fuzz_in"u8;
internal static readonly @string fuzzOutˢ = "fuzz_out"u8;
internal static readonly @string fuzzMemˢ = "fuzz_mem"u8;

// getWorkerComm returns communication channels in the worker process.
internal static (workerComm comm, error err) getWorkerComm() {
    error err = default!;

    @string v = os.Getenv(goTestFuzzWorkerHandlesˢ);
    if (v == ""u8) {
        return (new workerComm(nil), fmt.Errorf("GO_TEST_FUZZ_WORKER_HANDLES not set"u8));
    }
    ref var fuzzInFD = ref heap(new uintptr(), out var ᏑfuzzInFD);
    ref var fuzzOutFD = ref heap(new uintptr(), out var ᏑfuzzOutFD);
    ref var memFileFD = ref heap(new uintptr(), out var ᏑmemFileFD);
    {
        var (_, errΔ1) = fmt.Sscanf(v, "%x,%x,%x"u8, ᏑfuzzInFD, ᏑfuzzOutFD, ᏑmemFileFD); if (errΔ1 != default!) {
            return (new workerComm(nil), fmt.Errorf("parsing GO_TEST_FUZZ_WORKER_HANDLES=%s: %v"u8, v, errΔ1));
        }
    }
    var fuzzIn = os.NewFile(fuzzInFD, fuzzInˢ);
    var fuzzOut = os.NewFile(fuzzOutFD, fuzzOutˢ);
    var memFile = os.NewFile(memFileFD, fuzzMemˢ);
    (var fi, err) = memFile.Stat();
    if (err != default!) {
        return (new workerComm(nil), fmt.Errorf("worker checking temp file size: %w"u8, err));
    }
    nint size = (nint)fi.Size();
    if ((int64)size != fi.Size()) {
        return (new workerComm(nil), fmt.Errorf("fuzz temp file exceeds maximum size"u8));
    }
    var removeOnClose = false;
    (var mem, err) = sharedMemMapFile(memFile, size, removeOnClose);
    if (err != default!) {
        return (new workerComm(nil), err);
    }
    var memMu = new channel<ж<sharedMem>>(1);
    memMu.ᐸꟷ(mem);
    return (new workerComm(fuzzIn: fuzzIn, fuzzOut: fuzzOut, memMu: memMu), default!);
}

internal static bool isInterruptError(error err) {
    // On Windows, we can't tell whether the process was interrupted by the error
    // returned by Wait. It looks like an ExitError with status 1.
    return false;
}

// terminationSignal returns -1 and false because Windows doesn't have signals.
internal static (osꓸSignal, bool) terminationSignal(error err) {
    return (new syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)(-1))), false);
}

// isCrashSignal is not implemented because Windows doesn't have signals.
internal static bool isCrashSignal(osꓸSignal signal) {
    throw panic("not implemented: no signals on windows");
}

} // end fuzz_package
