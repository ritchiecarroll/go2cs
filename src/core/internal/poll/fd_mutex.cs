// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using atomic = go.sync.atomic_package;
using go.sync;

partial class poll_package {

// fdMutex is a specialized synchronization primitive that manages
// lifetime of an fd and serializes access to Read, Write and Close
// methods on FD.
partial struct fdMutex {
    internal uint64 state;
    internal uint32 rsema;
    internal uint32 wsema;
}

// fdMutex.state is organized as follows:
// 1 bit - whether FD is closed, if set all subsequent lock operations will fail.
// 1 bit - lock for read operations.
// 1 bit - lock for write operations.
// 20 bits - total number of references (read+write+misc).
// 20 bits - number of outstanding read waiters.
// 20 bits - number of outstanding write waiters.
internal static UntypedInt mutexClosed => /* 1 << 0 */ 1;

internal static UntypedInt mutexRLock => /* 1 << 1 */ 2;

internal static UntypedInt mutexWLock => /* 1 << 2 */ 4;

internal static UntypedInt mutexRef => /* 1 << 3 */ 8;

internal static UntypedInt mutexRefMask => /* (1<<20 - 1) << 3 */ 8388600;

internal static UntypedInt mutexRWait => /* 1 << 23 */ 8388608;

internal static UntypedInt mutexRMask => /* (1<<20 - 1) << 23 */ 8796084633600;

internal static UntypedInt mutexWWait => /* 1 << 43 */ 8796093022208;

internal static UntypedInt mutexWMask => /* (1<<20 - 1) << 43 */ 9223363240761753600;

internal static readonly @string overflowMsg = "too many concurrent operations on a single file or socket (max 1048575)"u8;

// Read operations must do rwlock(true)/rwunlock(true).
//
// Write operations must do rwlock(false)/rwunlock(false).
//
// Misc operations must do incref/decref.
// Misc operations include functions like setsockopt and setDeadline.
// They need to use incref/decref to ensure that they operate on the
// correct fd in presence of a concurrent close call (otherwise fd can
// be closed under their feet).
//
// Close operations must do increfAndClose/decref.

// incref adds a reference to mu.
// It reports whether mu is available for reading or writing.
internal static bool incref(this ж<fdMutex> Ꮡmu) {
    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & (uint64)mutexClosed) != 0) {
            return false;
        }
        var @new = old + (uint64)mutexRef;
        if ((uint64)(@new & (uint64)mutexRefMask) == 0) {
            throw panic(overflowMsg);
        }
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            return true;
        }
    }
}

// go2cs generated this placeholder — func increfAndClose is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// decref removes a reference from mu.
// It reports whether there is no remaining reference.
internal static bool decref(this ж<fdMutex> Ꮡmu) {
    while (ᐧ) {
        var old = atomic.LoadUint64(Ꮡmu.of(fdMutex.Ꮡstate));
        if ((uint64)(old & (uint64)mutexRefMask) == 0) {
            throw panic("inconsistent poll.fdMutex");
        }
        var @new = old - (uint64)mutexRef;
        if (atomic.CompareAndSwapUint64(Ꮡmu.of(fdMutex.Ꮡstate), old, @new)) {
            return (uint64)(@new & ((uint64)((uint64)mutexClosed | (uint64)mutexRefMask))) == mutexClosed;
        }
    }
}

// go2cs generated this placeholder — func rwlock is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func rwunlock is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Implemented in runtime package.
internal static partial void runtime_Semacquire(ж<uint32> sema);

internal static partial void runtime_Semrelease(ж<uint32> sema);

// incref adds a reference to fd.
// It returns an error when fd cannot be used.
internal static error incref(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (!Ꮡfd.of(FD.Ꮡfdmu).incref()) {
        return errClosing(fd.isFile);
    }
    return default!;
}

// decref removes a reference from fd.
// It also closes fd when the state of fd is set to closed and there
// is no remaining reference.
internal static error decref(this ж<FD> Ꮡfd) {
    if (Ꮡfd.of(FD.Ꮡfdmu).decref()) {
        return Ꮡfd.destroy();
    }
    return default!;
}

// readLock adds a reference to fd and locks fd for reading.
// It returns an error when fd cannot be used for reading.
internal static error readLock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (!fd.fdmu.rwlock(true)) {
        return errClosing(fd.isFile);
    }
    return default!;
}

// readUnlock removes a reference from fd and unlocks fd for reading.
// It also closes fd when the state of fd is set to closed and there
// is no remaining reference.
internal static void readUnlock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (fd.fdmu.rwunlock(true)) {
        Ꮡfd.destroy();
    }
}

// writeLock adds a reference to fd and locks fd for writing.
// It returns an error when fd cannot be used for writing.
internal static error writeLock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (!fd.fdmu.rwlock(false)) {
        return errClosing(fd.isFile);
    }
    return default!;
}

// writeUnlock removes a reference from fd and unlocks fd for writing.
// It also closes fd when the state of fd is set to closed and there
// is no remaining reference.
internal static void writeUnlock(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (fd.fdmu.rwunlock(false)) {
        Ꮡfd.destroy();
    }
}

} // end poll_package
