// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using mbits = go.math.bits_package;
using runtime = runtime_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using go.sync;

partial class libc_package {

internal static int32 _a_ctz_l(ref TLS tls, @ulong x) {
    return (int32)mbits.TrailingZeros64(x);
}

internal static int32 _a_cas(ref TLS tls, uintptr p, int32 t, int32 s) {
    return casInt32(p, t, s);
}

internal static uintptr _a_cas_p(ref TLS tls, uintptr p, uintptr t, uintptr s) {
    var w = @unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
    for (;;) {
        var old = atomic.LoadUintptr(w);
        if (old != t) {
            return old;
        }

        if (atomic.CompareAndSwapUintptr(w, t, s)) {
            return t;
        }
    }
}

internal static int32 _a_swap(ref TLS tls, uintptr p, int32 v) {
    return atomic.SwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), v);
}

internal static int32 _a_fetch_add(ref TLS tls, uintptr p, int32 v) {
    return atomic.AddInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), v) - v;
}

internal static void _a_and(ref TLS tls, uintptr p, int32 v) {
    var w = @unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
    for (;;) {
        var old = atomic.LoadInt32(w);
        if (atomic.CompareAndSwapInt32(w, old, old & v)) {
            return;
        }
    }
}

internal static void _a_or(ref TLS tls, uintptr p, int32 v) {
    var w = @unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
    for (;;) {
        var old = atomic.LoadInt32(w);
        if (atomic.CompareAndSwapInt32(w, old, old | v)) {
            return;
        }
    }
}

internal static void _a_and_64(ref TLS tls, uintptr p, uint64 v) {
    var w = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    for (;;) {
        var old = atomic.LoadUint64(w);
        if (atomic.CompareAndSwapUint64(w, old, old & v)) {
            return;
        }
    }
}

internal static void _a_or_64(ref TLS tls, uintptr p, uint64 v) {
    var w = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    for (;;) {
        var old = atomic.LoadUint64(w);
        if (atomic.CompareAndSwapUint64(w, old, old | v)) {
            return;
        }
    }
}

internal static void _a_store(ref TLS tls, uintptr p, int32 x) {
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), x);
}

internal static readonly ж<atomic.Int32> ᏑatomicBarrier = new StaticBox<atomic.Int32>(default(atomic.Int32));
internal static ref atomic.Int32 atomicBarrier => ref ((StaticBox<atomic.Int32>)ᏑatomicBarrier).ValueSlot;

internal static void _a_barrier(ref TLS tls) {
    ᏑatomicBarrier.Add(1);
}

internal static void _a_spin(ref TLS tls) {
    runtime.Gosched();
}

internal static void _a_crash(ref TLS tls) {
    throw panic("crash");
}

internal static int32 _a_clz_64(ref TLS tls, uint64 x) {
    return (int32)mbits.LeadingZeros64(x);
}

} // end libc_package
