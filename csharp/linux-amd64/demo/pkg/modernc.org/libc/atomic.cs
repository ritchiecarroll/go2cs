// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;
using mbits = go.math.bits_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using go.sync;

partial class libc_package {

internal static int8 a_store_8(uintptr addr, int8 val) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)addr)).Value = val;
    return val;
}

internal static int8 /*val*/ a_load_8(uintptr addr) {
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)addr);
}

internal static int16 /*val*/ a_load_16(uintptr addr) {
    if ((uintptr)(addr & 1) != 0) {
        throw panic("unaligned atomic access");
    }

    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)addr);
}

internal static void a_store_16(uintptr addr, uint16 val) {
    if ((uintptr)(addr & 1) != 0) {
        throw panic("unaligned atomic access");
    }

    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)addr)).Value = val;
}

internal static int32 _a_ctz_64(ref TLS tls, uint64 x) {
    return (int32)mbits.TrailingZeros64(x);
}

public static float32 /*new*/ AtomicAddFloat32(ж<float32> addr, float32 delta) {
    for (;;) {
        var oldBits = atomic.LoadUint32(addr.Reinterpret<float32, uint32>());
        var oldVal = math.Float32frombits(oldBits);
        var newVal = oldVal + delta;
        var newBits = math.Float32bits(newVal);
        if (atomic.CompareAndSwapUint32(addr.Reinterpret<float32, uint32>(), oldBits, newBits)) {
            return newVal;
        }
    }
}

public static float32 /*val*/ AtomicLoadFloat32(ж<float32> addr) {
    return math.Float32frombits(atomic.LoadUint32(addr.Reinterpret<float32, uint32>()));
}

public static void AtomicStoreFloat32(ж<float32> addr, float32 val) {
    atomic.StoreUint32(addr.Reinterpret<float32, uint32>(), math.Float32bits(val));
}

public static float64 /*new*/ AtomicAddFloat64(ж<float64> addr, float64 delta) {
    for (;;) {
        var oldBits = atomic.LoadUint64(addr.Reinterpret<float64, uint64>());
        var oldVal = math.Float64frombits(oldBits);
        var newVal = oldVal + delta;
        var newBits = math.Float64bits(newVal);
        if (atomic.CompareAndSwapUint64(addr.Reinterpret<float64, uint64>(), oldBits, newBits)) {
            return newVal;
        }
    }
}

public static float64 /*val*/ AtomicLoadFloat64(ж<float64> addr) {
    return math.Float64frombits(atomic.LoadUint64(addr.Reinterpret<float64, uint64>()));
}

public static void AtomicStoreFloat64(ж<float64> addr, float64 val) {
    atomic.StoreUint64(addr.Reinterpret<float64, uint64>(), math.Float64bits(val));
}

public static int32 /*new*/ AtomicAddInt32(ж<int32> addr, int32 delta) { return atomic.AddInt32(addr, delta); }

public static int64 /*new*/ AtomicAddInt64(ж<int64> addr, int64 delta) { return atomic.AddInt64(addr, delta); }

public static uint32 /*new*/ AtomicAddUint32(ж<uint32> addr, uint32 delta) { return atomic.AddUint32(addr, delta); }

public static uint64 /*new*/ AtomicAddUint64(ж<uint64> addr, uint64 delta) { return atomic.AddUint64(addr, delta); }

public static uintptr /*new*/ AtomicAddUintptr(ж<uintptr> addr, uintptr delta) {
    return atomic.AddUintptr(addr, delta);
}

public static int32 /*val*/ AtomicLoadInt32(ж<int32> addr) { return atomic.LoadInt32(addr); }

public static int64 /*val*/ AtomicLoadInt64(ж<int64> addr) { return atomic.LoadInt64(addr); }

public static uint32 /*val*/ AtomicLoadUint32(ж<uint32> addr) { return atomic.LoadUint32(addr); }

public static uint64 /*val*/ AtomicLoadUint64(ж<uint64> addr) { return atomic.LoadUint64(addr); }

public static uintptr /*val*/ AtomicLoadUintptr(ж<uintptr> addr) { return atomic.LoadUintptr(addr); }

public static void AtomicStoreInt32(ж<int32> addr, int32 val) { atomic.StoreInt32(addr, val); }

public static void AtomicStoreUint32(ж<uint32> addr, uint32 val) { atomic.StoreUint32(addr, val); }

public static void AtomicStoreUint64(ж<uint64> addr, uint64 val) { atomic.StoreUint64(addr, val); }

public static void AtomicStoreUintptr(ж<uintptr> addr, uintptr val) { atomic.StoreUintptr(addr, val); }

public static void AtomicStoreInt64(ж<int64> addr, int64 val) { atomic.StoreInt64(addr, val); }

} // end libc_package
