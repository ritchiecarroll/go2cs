// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goexperiment = @internal.goexperiment_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// These functions cannot have go:noescape annotations,
// because while ptr does not escape, new does.
// If new is marked as not escaping, the compiler will make incorrect
// escape analysis decisions about the pointer value being stored.

// atomicwb performs a write barrier before an atomic pointer write.
// The caller should guard the call with "if writeBarrier.enabled".
//
// atomicwb should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname atomicwb
//go:nosplit
internal static void atomicwb(ж<@unsafe.Pointer> Ꮡptr, @unsafe.Pointer @new) {
    ref var ptr = ref Ꮡptr.DerefOrNull();

    var slot = Ꮡptr.Reinterpret<@unsafe.Pointer, uintptr>();
    var buf = (~(~getg()).m).p.ptr().of(runtime_package.Δp.ᏑwbBuf).get2();
    buf.Value[0] = slot.Value;
    buf.Value[1] = (uintptr)@new;
}

// atomicstorep performs *ptr = new atomically and invokes a write barrier.
//
//go:nosplit
internal static void atomicstorep(@unsafe.Pointer ptr, @unsafe.Pointer @new) {
    if (writeBarrier.enabled) {
        atomicwb((ж<@unsafe.Pointer>)(uintptr)(ptr), @new);
    }
    if (goexperiment.CgoCheck2) {
        cgoCheckPtrWrite((ж<@unsafe.Pointer>)(uintptr)(ptr), @new);
    }
    atomic.StorepNoWB((uintptr)noescape(ptr), @new);
}

// atomic_storePointer is the implementation of internal/runtime/atomic.UnsafePointer.Store
// (like StoreNoWB but with the write barrier).
//
//go:nosplit
//go:linkname atomic_storePointer internal/runtime/atomic.storePointer
internal static void atomic_storePointer(ж<@unsafe.Pointer> Ꮡptr, @unsafe.Pointer @new) {
    atomicstorep(@unsafe.Pointer.FromPinnedBox(Ꮡptr), @new);
}

// atomic_casPointer is the implementation of internal/runtime/atomic.UnsafePointer.CompareAndSwap
// (like CompareAndSwapNoWB but with the write barrier).
//
//go:nosplit
//go:linkname atomic_casPointer internal/runtime/atomic.casPointer
internal static bool atomic_casPointer(ж<@unsafe.Pointer> Ꮡptr, @unsafe.Pointer old, @unsafe.Pointer @new) {
    if (writeBarrier.enabled) {
        atomicwb(Ꮡptr, @new);
    }
    if (goexperiment.CgoCheck2) {
        cgoCheckPtrWrite(Ꮡptr, @new);
    }
    return atomic.Casp1(Ꮡptr, old, @new);
}

// Like above, but implement in terms of sync/atomic's uintptr operations.
// We cannot just call the runtime routines, because the race detector expects
// to be able to intercept the sync/atomic forms but not the runtime forms.

//go:linkname sync_atomic_StoreUintptr sync/atomic.StoreUintptr
internal static partial void sync_atomic_StoreUintptr(ж<uintptr> ptr, uintptr @new);

//go:linkname sync_atomic_StorePointer sync/atomic.StorePointer
//go:nosplit
internal static void sync_atomic_StorePointer(ж<@unsafe.Pointer> Ꮡptr, @unsafe.Pointer @new) {
    if (writeBarrier.enabled) {
        atomicwb(Ꮡptr, @new);
    }
    if (goexperiment.CgoCheck2) {
        cgoCheckPtrWrite(Ꮡptr, @new);
    }
    sync_atomic_StoreUintptr(Ꮡptr.Reinterpret<@unsafe.Pointer, uintptr>(), (uintptr)@new);
}

//go:linkname sync_atomic_SwapUintptr sync/atomic.SwapUintptr
internal static partial uintptr sync_atomic_SwapUintptr(ж<uintptr> ptr, uintptr @new);

//go:linkname sync_atomic_SwapPointer sync/atomic.SwapPointer
//go:nosplit
internal static @unsafe.Pointer sync_atomic_SwapPointer(ж<@unsafe.Pointer> Ꮡptr, @unsafe.Pointer @new) {
    if (writeBarrier.enabled) {
        atomicwb(Ꮡptr, @new);
    }
    if (goexperiment.CgoCheck2) {
        cgoCheckPtrWrite(Ꮡptr, @new);
    }
    @unsafe.Pointer old = (@unsafe.Pointer)sync_atomic_SwapUintptr(Ꮡptr.Reinterpret<@unsafe.Pointer, uintptr>(), (uintptr)@new);
    return old;
}

//go:linkname sync_atomic_CompareAndSwapUintptr sync/atomic.CompareAndSwapUintptr
internal static partial bool sync_atomic_CompareAndSwapUintptr(ж<uintptr> ptr, uintptr old, uintptr @new);

//go:linkname sync_atomic_CompareAndSwapPointer sync/atomic.CompareAndSwapPointer
//go:nosplit
internal static bool sync_atomic_CompareAndSwapPointer(ж<@unsafe.Pointer> Ꮡptr, @unsafe.Pointer old, @unsafe.Pointer @new) {
    if (writeBarrier.enabled) {
        atomicwb(Ꮡptr, @new);
    }
    if (goexperiment.CgoCheck2) {
        cgoCheckPtrWrite(Ꮡptr, @new);
    }
    return sync_atomic_CompareAndSwapUintptr(Ꮡptr.Reinterpret<@unsafe.Pointer, uintptr>(), (uintptr)old, (uintptr)@new);
}

} // end runtime_package
