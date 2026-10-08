// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using os = os_package;
using @unsafe = unsafe_package;

partial class fuzz_package {

// sharedMem manages access to a region of virtual memory mapped from a file,
// shared between multiple processes. The region includes space for a header and
// a value of variable length.
//
// When fuzzing, the coordinator creates a sharedMem from a temporary file for
// each worker. This buffer is used to pass values to fuzz between processes.
// Care must be taken to manage access to shared memory across processes;
// sharedMem provides no synchronization on its own. See workerComm for an
// explanation.
partial struct sharedMem {
    // f is the file mapped into memory.
    internal ж<os.File> f;
    // region is the mapped region of virtual memory for f. The content of f may
    // be read or written through this slice.
    internal slice<byte> region;
    // removeOnClose is true if the file should be deleted by Close.
    internal bool removeOnClose;
    // sys contains OS-specific information.
    internal sharedMemSys sys;
}

// sharedMemHeader stores metadata in shared memory.
partial struct sharedMemHeader {
    // count is the number of times the worker has called the fuzz function.
    // May be reset by coordinator.
    internal int64 count;
    // valueLen is the number of bytes in region which should be read.
    internal nint valueLen;
    // randState and randInc hold the state of a pseudo-random number generator.
    internal uint64 randState, randInc;
    // rawInMem is true if the region holds raw bytes, which occurs during
    // minimization. If true after the worker fails during minimization, this
    // indicates that an unrecoverable error occurred, and the region can be
    // used to retrieve the raw bytes that caused the error.
    internal bool rawInMem;
}

// sharedMemSize returns the size needed for a shared memory buffer that can
// contain values of the given size.
internal static nint sharedMemSize(nint valueSize) {
    // TODO(jayconrod): set a reasonable maximum size per platform.
    return (nint)/* unsafe.Sizeof(sharedMemHeader{}) */ (uintptr)40 + valueSize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fuzzˢ = "fuzz-*"u8;

// sharedMemTempFile creates a new temporary file of the given size, then maps
// it into memory. The file will be removed when the Close method is called.
internal static (ж<sharedMem> m, error err) sharedMemTempFile(nint size) {
    ж<sharedMem> m = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        // Create a temporary file.
        (var f, err) = os.CreateTemp(""u8, fuzzˢ);
        if (err != default!) {
            (m, err) = (default!, err); goto ᒐdone;
        }
        var errʗ1 = err;
        var fʗ1 = f;
        defer(() => {
            if (errʗ1 != default!) {
                fʗ1.Close();
                os.Remove(fʗ1.Name());
            }
        }, ref ᒐ);
        // Resize it to the correct size.
        nint totalSize = sharedMemSize(size);
        {
            var errΔ1 = f.Truncate((int64)totalSize); if (errΔ1 != default!) {
                (m, err) = (default!, errΔ1); goto ᒐdone;
            }
        }
        // Map the file into memory.
        var removeOnClose = true;
        (m, err) = sharedMemMapFile(f, totalSize, removeOnClose);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (m, err);
}

// header returns a pointer to metadata within the shared memory region.
internal static ж<sharedMemHeader> header(this ref sharedMem m) {
    return Ꮡ(m.region, 0).Reinterpret<byte, sharedMemHeader>();
}

// valueRef returns the value currently stored in shared memory. The returned
// slice points to shared memory; it is not a copy.
internal static slice<byte> valueRef(this ref sharedMem m) {
    nint length = m.header().Value.valueLen;
    nint valueOffset = (nint)/* unsafe.Sizeof(sharedMemHeader{}) */ (uintptr)40;
    return m.region.slice(valueOffset, valueOffset + length);
}

// valueCopy returns a copy of the value stored in shared memory.
internal static slice<byte> valueCopy(this ref sharedMem m) {
    var @ref = m.valueRef();
    return bytes.Clone(@ref);
}

// setValue copies the data in b into the shared memory buffer and sets
// the length. len(b) must be less than or equal to the capacity of the buffer
// (as returned by cap(m.value())).
internal static void setValue(this ref sharedMem m, slice<byte> b) {
    var v = m.valueRef();
    if (len(b) > builtin.cap(v)) {
        throw panic(fmt.Sprintf("value length %d larger than shared memory capacity %d"u8, len(b), builtin.cap(v)));
    }
    m.header().Value.valueLen = len(b);
    copy(v.slice(0, builtin.cap(v)), b);
}

// setValueLen sets the length of the shared memory buffer returned by valueRef
// to n, which may be at most the cap of that slice.
//
// Note that we can only store the length in the shared memory header. The full
// slice header contains a pointer, which is likely only valid for one process,
// since each process can map shared memory at a different virtual address.
internal static void setValueLen(this ref sharedMem m, nint n) {
    var v = m.valueRef();
    if (n > builtin.cap(v)) {
        throw panic(fmt.Sprintf("length %d larger than shared memory capacity %d"u8, n, builtin.cap(v)));
    }
    m.header().Value.valueLen = n;
}

// TODO(jayconrod): add method to resize the buffer. We'll need that when the
// mutator can increase input length. Only the coordinator will be able to
// do it, since we'll need to send a message to the worker telling it to
// remap the file.

} // end fuzz_package
