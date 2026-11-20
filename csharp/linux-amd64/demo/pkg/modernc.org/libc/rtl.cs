// Copyright 2023 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using math = math_package;
using reflect = reflect_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using go.sync;
using ꓸꓸꓸany = Span<any>;

partial class libc_package {

public static uintptr /*r*/ VaList(uintptr p, params ꓸꓸꓸany argsʗp) {
    uintptr r = default;
    var args = argsʗp.sslice();

    if ((uintptr)(p & 7) != 0) {
        throw panic("internal error");
    }

    r = p;
    foreach (var (_, vᴛ1) in range(args)) {
        ref var v = ref heap<any>(out var Ꮡv);
        v = vᴛ1;

        switch (v.type()) {
        case nint x: {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = (int64)x;
            break;
        }
        case int32 x: {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = (int64)x;
            break;
        }
        case int64 x: {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case nuint x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = (uint64)x;
            break;
        }
        case uint16 x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = (uint64)x;
            break;
        }
        case uint32 x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = (uint64)x;
            break;
        }
        case uint64 x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case float64 x: {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case uintptr x: {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case @string x: {
            throw panic("libc.VaList: a Go string is not a valid C variadic argument, use CString");
            break;
        }
        default: {
            var x = v;
            var sz = reflect.TypeOf(v).Size();
            copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)p), sz), @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)@unsafe.Pointer.ElementRef<uintptr>(@unsafe.Pointer.FromPinnedBox(Ꮡv), 1, 2)), sz));
            p += roundup(sz, 8);
            continue;
            break;
        }}
        p += 8;
    }
    return r;
}

public static bool Bool(bool v) { return v; }

public static int32 Bool32(bool b) {
    if (b) {
        return 1;
    }

    return 0;
}

internal static uintptr roundup(uintptr n, uintptr to) {
    var r = n % to; if (r != 0) {
        return n + to - r;
    }

    return n;
}

public static uintptr /*r*/ VaOther(ж<uintptr> app, uint64 sz) {
    uintptr r = default;

    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    r = ap;
    ap = roundup(ap + (uintptr)sz, 8);
    (app).Value = ap;
    return r;
}

public static int32 VaInt32(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = (int32)(~@unsafe.Pointer.To<int64>((@unsafe.Pointer)ap));
    ap += 8;
    (app).Value = ap;
    return v;
}

public static uint32 VaUint32(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = (uint32)(~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ap));
    ap += 8;
    (app).Value = ap;
    return v;
}

public static int64 VaInt64(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static uint64 VaUint64(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static float32 VaFloat32(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0F;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return (float32)v;
}

public static float64 VaFloat64(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0D;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static uintptr VaUintptr(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static void AtomicStoreNUint8(uintptr ptr, uint8 val, int32 memorder) {
    a_store_8(ptr, (int8)val);
}

public static int8 AtomicStorePInt8(uintptr addr, int8 val) {
    a_store_8(addr, val);
    return val;
}

public static byte AtomicStorePUint8(uintptr addr, byte val) {
    a_store_8(addr, (int8)val);
    return val;
}

public static int16 AtomicStorePInt16(uintptr addr, int16 val) {
    a_store_16(addr, (uint16)val);
    return val;
}

public static uint16 AtomicStorePUint16(uintptr addr, uint16 val) {
    a_store_16(addr, val);
    return val;
}

public static int32 AtomicStorePInt32(uintptr addr, int32 val) {
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)addr), val);
    return val;
}

public static int64 AtomicStorePInt64(uintptr addr, int64 val) {
    atomic.StoreInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)addr), val);
    return val;
}

public static uint32 AtomicStorePUint32(uintptr addr, uint32 val) {
    atomic.StoreUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr), val);
    return val;
}

public static uint64 AtomicStorePUint64(uintptr addr, uint64 val) {
    atomic.StoreUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr), val);
    return val;
}

public static uintptr AtomicStorePUintptr(uintptr addr, uintptr val) {
    atomic.StoreUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)addr), val);
    return val;
}

public static float32 AtomicStorePFloat32(uintptr addr, float32 val) {
    atomic.StoreUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr), math.Float32bits(val));
    return val;
}

public static float64 AtomicStorePFloat64(uintptr addr, float64 val) {
    atomic.StoreUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr), math.Float64bits(val));
    return val;
}

public static int8 /*val*/ AtomicLoadPInt8(uintptr addr) {
    return a_load_8(addr);
}

public static int16 /*val*/ AtomicLoadPInt16(uintptr addr) {
    return a_load_16(addr);
}

public static int32 /*val*/ AtomicLoadPInt32(uintptr addr) {
    return atomic.LoadInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)addr));
}

public static int64 /*val*/ AtomicLoadPInt64(uintptr addr) {
    return atomic.LoadInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)addr));
}

public static byte AtomicLoadPUint8(uintptr addr) {
    return (byte)a_load_8(addr);
}

public static uint16 AtomicLoadPUint16(uintptr addr) {
    return (uint16)a_load_16(addr);
}

public static uint32 /*val*/ AtomicLoadPUint32(uintptr addr) {
    return atomic.LoadUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr));
}

public static uint64 /*val*/ AtomicLoadPUint64(uintptr addr) {
    return atomic.LoadUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr));
}

public static uintptr /*val*/ AtomicLoadPUintptr(uintptr addr) {
    return atomic.LoadUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)addr));
}

public static float32 /*val*/ AtomicLoadPFloat32(uintptr addr) {
    return math.Float32frombits(atomic.LoadUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr)));
}

public static float64 /*val*/ AtomicLoadPFloat64(uintptr addr) {
    return math.Float64frombits(atomic.LoadUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr)));
}

public static void AtomicStoreNUint16(uintptr ptr, uint16 val, int32 memorder) {
    a_store_16(ptr, val);
}

public static void AtomicStoreNInt32(uintptr ptr, int32 val, int32 memorder) {
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr), val);
}

public static void AtomicStoreNInt64(uintptr ptr, int64 val, int32 memorder) {
    atomic.StoreInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr), val);
}

public static void AtomicStoreNUint32(uintptr ptr, uint32 val, int32 memorder) {
    atomic.StoreUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr), val);
}

public static void AtomicStoreNUint64(uintptr ptr, uint64 val, int32 memorder) {
    atomic.StoreUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr), val);
}

public static void AtomicStoreNUintptr(uintptr ptr, uintptr val, int32 memorder) {
    atomic.StoreUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ptr), val);
}

public static uint8 AtomicLoadNUint8(uintptr ptr, int32 memorder) {
    return (byte)a_load_8(ptr);
}

public static uint16 AtomicLoadNUint16(uintptr ptr, int32 memorder) {
    return (uint16)a_load_16(ptr);
}

public static int32 AtomicLoadNInt32(uintptr ptr, int32 memorder) {
    return atomic.LoadInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr));
}

public static int64 AtomicLoadNInt64(uintptr ptr, int32 memorder) {
    return atomic.LoadInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr));
}

public static uint32 AtomicLoadNUint32(uintptr ptr, int32 memorder) {
    return atomic.LoadUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr));
}

public static uint64 AtomicLoadNUint64(uintptr ptr, int32 memorder) {
    return atomic.LoadUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr));
}

public static uintptr AtomicLoadNUintptr(uintptr ptr, int32 memorder) {
    return atomic.LoadUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ptr));
}

public static int8 AssignInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static int16 AssignInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static int32 AssignInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static int64 AssignInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static uint8 AssignUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static uint16 AssignUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static uint32 AssignUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static uint64 AssignUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static float32 AssignFloat32(ж<float32> Ꮡp, float32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static float64 AssignFloat64(ж<float64> Ꮡp, float64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static complex64 AssignComplex64(ж<complex64> Ꮡp, complex64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static complex128 AssignComplex128(ж<complex128> Ꮡp, complex128 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static uintptr AssignUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
    return v;
}

public static int8 AssignPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static int16 AssignPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static int32 AssignPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static int64 AssignPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static uint8 AssignPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static uint16 AssignPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static uint32 AssignPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static uint64 AssignPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static float32 AssignPtrFloat32(uintptr p, float32 v) {
    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static float64 AssignPtrFloat64(uintptr p, float64 v) {
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static complex64 AssignPtrComplex64(uintptr p, complex64 v) {
    (@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p)).Value = v;
    return v;
}
public static complex128 AssignPtrComplex128(uintptr p, complex128 v) {
    (@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static uintptr AssignPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value = v;
    return v;
}

public static int8 AssignMulInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static int16 AssignMulInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static int32 AssignMulInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static int64 AssignMulInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static uint8 AssignMulUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static uint16 AssignMulUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static uint32 AssignMulUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static uint64 AssignMulUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static float32 AssignMulFloat32(ж<float32> Ꮡp, float32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static float64 AssignMulFloat64(ж<float64> Ꮡp, float64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static complex64 AssignMulComplex64(ж<complex64> Ꮡp, complex64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static complex128 AssignMulComplex128(ж<complex128> Ꮡp, complex128 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static uintptr AssignMulUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p *= v;
    return p;
}

public static int8 AssignDivInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static int16 AssignDivInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static int32 AssignDivInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.QuoAssign(v);
    return p;
}

public static int64 AssignDivInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.QuoAssign(v);
    return p;
}

public static uint8 AssignDivUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static uint16 AssignDivUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static uint32 AssignDivUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static uint64 AssignDivUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static float32 AssignDivFloat32(ж<float32> Ꮡp, float32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static float64 AssignDivFloat64(ж<float64> Ꮡp, float64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static complex64 AssignDivComplex64(ж<complex64> Ꮡp, complex64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static complex128 AssignDivComplex128(ж<complex128> Ꮡp, complex128 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static uintptr AssignDivUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p /= v;
    return p;
}

public static int8 AssignRemInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static int16 AssignRemInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static int32 AssignRemInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RemAssign(v);
    return p;
}

public static int64 AssignRemInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RemAssign(v);
    return p;
}

public static uint8 AssignRemUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static uint16 AssignRemUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static uint32 AssignRemUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static uint64 AssignRemUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static uintptr AssignRemUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p %= v;
    return p;
}

public static int8 AssignAddInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static int16 AssignAddInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static int32 AssignAddInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static int64 AssignAddInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static uint8 AssignAddUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static uint16 AssignAddUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static uint32 AssignAddUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static uint64 AssignAddUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static float32 AssignAddFloat32(ж<float32> Ꮡp, float32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static float64 AssignAddFloat64(ж<float64> Ꮡp, float64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static complex64 AssignAddComplex64(ж<complex64> Ꮡp, complex64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static complex128 AssignAddComplex128(ж<complex128> Ꮡp, complex128 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static uintptr AssignAddUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += v;
    return p;
}

public static int8 AssignSubInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static int16 AssignSubInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static int32 AssignSubInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static int64 AssignSubInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static uint8 AssignSubUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static uint16 AssignSubUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static uint32 AssignSubUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static uint64 AssignSubUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static float32 AssignSubFloat32(ж<float32> Ꮡp, float32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static float64 AssignSubFloat64(ж<float64> Ꮡp, float64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static complex64 AssignSubComplex64(ж<complex64> Ꮡp, complex64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static complex128 AssignSubComplex128(ж<complex128> Ꮡp, complex128 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static uintptr AssignSubUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= v;
    return p;
}

public static int8 AssignAndInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (int8)(v);
    return p;
}

public static int16 AssignAndInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (int16)(v);
    return p;
}

public static int32 AssignAndInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (int32)(v);
    return p;
}

public static int64 AssignAndInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (int64)(v);
    return p;
}

public static uint8 AssignAndUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (uint8)(v);
    return p;
}

public static uint16 AssignAndUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (uint16)(v);
    return p;
}

public static uint32 AssignAndUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (uint32)(v);
    return p;
}

public static uint64 AssignAndUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (uint64)(v);
    return p;
}

public static uintptr AssignAndUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p &= (uintptr)(v);
    return p;
}

public static int8 AssignXorInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (int8)(v);
    return p;
}

public static int16 AssignXorInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (int16)(v);
    return p;
}

public static int32 AssignXorInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (int32)(v);
    return p;
}

public static int64 AssignXorInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (int64)(v);
    return p;
}

public static uint8 AssignXorUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (uint8)(v);
    return p;
}

public static uint16 AssignXorUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (uint16)(v);
    return p;
}

public static uint32 AssignXorUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (uint32)(v);
    return p;
}

public static uint64 AssignXorUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (uint64)(v);
    return p;
}

public static uintptr AssignXorUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p ^= (uintptr)(v);
    return p;
}

public static int8 AssignOrInt8(ж<int8> Ꮡp, int8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (int8)(v);
    return p;
}

public static int16 AssignOrInt16(ж<int16> Ꮡp, int16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (int16)(v);
    return p;
}

public static int32 AssignOrInt32(ж<int32> Ꮡp, int32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (int32)(v);
    return p;
}

public static int64 AssignOrInt64(ж<int64> Ꮡp, int64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (int64)(v);
    return p;
}

public static uint8 AssignOrUint8(ж<uint8> Ꮡp, uint8 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (uint8)(v);
    return p;
}

public static uint16 AssignOrUint16(ж<uint16> Ꮡp, uint16 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (uint16)(v);
    return p;
}

public static uint32 AssignOrUint32(ж<uint32> Ꮡp, uint32 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (uint32)(v);
    return p;
}

public static uint64 AssignOrUint64(ж<uint64> Ꮡp, uint64 v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (uint64)(v);
    return p;
}

public static uintptr AssignOrUintptr(ж<uintptr> Ꮡp, uintptr v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p |= (uintptr)(v);
    return p;
}

public static int8 AssignMulPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignMulPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignMulPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignMulPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignMulPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignMulPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignMulPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignMulPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static float32 AssignMulPtrFloat32(uintptr p, float32 v) {
    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)p);
}

public static float64 AssignMulPtrFloat64(uintptr p, float64 v) {
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)p);
}

public static complex64 AssignMulPtrComplex64(uintptr p, complex64 v) {
    (@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p);
}

public static complex128 AssignMulPtrComplex128(uintptr p, complex128 v) {
    (@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p);
}

public static uintptr AssignMulPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value *= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignDivPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignDivPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignDivPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value.QuoAssign(v);
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignDivPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value.QuoAssign(v);
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignDivPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignDivPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignDivPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignDivPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static float32 AssignDivPtrFloat32(uintptr p, float32 v) {
    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)p);
}

public static float64 AssignDivPtrFloat64(uintptr p, float64 v) {
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)p);
}

public static complex64 AssignDivPtrComplex64(uintptr p, complex64 v) {
    (@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p);
}

public static complex128 AssignDivPtrComplex128(uintptr p, complex128 v) {
    (@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p);
}

public static uintptr AssignDivPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value /= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignRemPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignRemPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignRemPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value.RemAssign(v);
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignRemPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value.RemAssign(v);
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignRemPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignRemPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignRemPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignRemPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static uintptr AssignRemPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value %= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignAddPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignAddPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignAddPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignAddPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignAddPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignAddPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignAddPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignAddPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static float32 AssignAddPtrFloat32(uintptr p, float32 v) {
    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)p);
}

public static float64 AssignAddPtrFloat64(uintptr p, float64 v) {
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)p);
}

public static complex64 AssignAddPtrComplex64(uintptr p, complex64 v) {
    (@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p);
}

public static complex128 AssignAddPtrComplex128(uintptr p, complex128 v) {
    (@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p);
}

public static uintptr AssignAddPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value += v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignSubPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignSubPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignSubPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignSubPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignSubPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignSubPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignSubPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignSubPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static float32 AssignSubPtrFloat32(uintptr p, float32 v) {
    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)p);
}

public static float64 AssignSubPtrFloat64(uintptr p, float64 v) {
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)p);
}

public static complex64 AssignSubPtrComplex64(uintptr p, complex64 v) {
    (@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<complex64>((@unsafe.Pointer)p);
}

public static complex128 AssignSubPtrComplex128(uintptr p, complex128 v) {
    (@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<complex128>((@unsafe.Pointer)p);
}

public static uintptr AssignSubPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value -= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignAndPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignAndPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignAndPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignAndPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignAndPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignAndPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignAndPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignAndPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static uintptr AssignAndPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value &= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignXorPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignXorPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignXorPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignXorPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignXorPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignXorPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignXorPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignXorPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static uintptr AssignXorPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value ^= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignOrPtrInt8(uintptr p, int8 v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignOrPtrInt16(uintptr p, int16 v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignOrPtrInt32(uintptr p, int32 v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignOrPtrInt64(uintptr p, int64 v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignOrPtrUint8(uintptr p, uint8 v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignOrPtrUint16(uintptr p, uint16 v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignOrPtrUint32(uintptr p, uint32 v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignOrPtrUint64(uintptr p, uint64 v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static uintptr AssignOrPtrUintptr(uintptr p, uintptr v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value |= v;
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignShlPtrInt8(uintptr p, nint v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignShlPtrInt16(uintptr p, nint v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignShlPtrInt32(uintptr p, nint v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignShlPtrInt64(uintptr p, nint v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignShlPtrUint8(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignShlPtrUint16(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignShlPtrUint32(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignShlPtrUint64(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static uintptr AssignShlPtrUintptr(uintptr p, nint v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value.LshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignShrPtrInt8(uintptr p, nint v) {
    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p);
}

public static int16 AssignShrPtrInt16(uintptr p, nint v) {
    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p);
}

public static int32 AssignShrPtrInt32(uintptr p, nint v) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
}

public static int64 AssignShrPtrInt64(uintptr p, nint v) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p);
}

public static uint8 AssignShrPtrUint8(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
}

public static uint16 AssignShrPtrUint16(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
}

public static uint32 AssignShrPtrUint32(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
}

public static uint64 AssignShrPtrUint64(uintptr p, nint v) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
}

public static uintptr AssignShrPtrUintptr(uintptr p, nint v) {
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value.RshAssign((int64)(v));
    return ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
}

public static int8 AssignShlInt8(ж<int8> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static int16 AssignShlInt16(ж<int16> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static int32 AssignShlInt32(ж<int32> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static int64 AssignShlInt64(ж<int64> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static uint8 AssignShlUint8(ж<uint8> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static uint16 AssignShlUint16(ж<uint16> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static uint32 AssignShlUint32(ж<uint32> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static uint64 AssignShlUint64(ж<uint64> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static uintptr AssignShlUintptr(ж<uintptr> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.LshAssign((int64)(v));
    return p;
}

public static int8 AssignShrInt8(ж<int8> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static int16 AssignShrInt16(ж<int16> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static int32 AssignShrInt32(ж<int32> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static int64 AssignShrInt64(ж<int64> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static uint8 AssignShrUint8(ж<uint8> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static uint16 AssignShrUint16(ж<uint16> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static uint32 AssignShrUint32(ж<uint32> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static uint64 AssignShrUint64(ж<uint64> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static uintptr AssignShrUintptr(ж<uintptr> Ꮡp, nint v) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.RshAssign((int64)(v));
    return p;
}

public static int8 PreIncInt8(ж<int8> Ꮡp, int8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static int16 PreIncInt16(ж<int16> Ꮡp, int16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static int32 PreIncInt32(ж<int32> Ꮡp, int32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static int64 PreIncInt64(ж<int64> Ꮡp, int64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static uint8 PreIncUint8(ж<uint8> Ꮡp, uint8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static uint16 PreIncUint16(ж<uint16> Ꮡp, uint16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static uint32 PreIncUint32(ж<uint32> Ꮡp, uint32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static uint64 PreIncUint64(ж<uint64> Ꮡp, uint64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static float32 PreIncFloat32(ж<float32> Ꮡp, float32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static float64 PreIncFloat64(ж<float64> Ꮡp, float64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static complex64 PreIncComplex64(ж<complex64> Ꮡp, complex64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static complex128 PreIncComplex128(ж<complex128> Ꮡp, complex128 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static uintptr PreIncUintptr(ж<uintptr> Ꮡp, uintptr d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p += d;
    return p;
}

public static int32 PreIncAtomicInt32(ж<int32> p, int32 d) { return atomic.AddInt32(p, d); }
public static int64 PreIncAtomicInt64(ж<int64> p, int64 d) { return atomic.AddInt64(p, d); }
public static uint32 PreIncAtomicUint32(ж<uint32> p, uint32 d) { return atomic.AddUint32(p, d); }
public static uint64 PreIncAtomicUint64(ж<uint64> p, uint64 d) { return atomic.AddUint64(p, d); }
public static uintptr PreIncAtomicUintptr(ж<uintptr> p, uintptr d) { return atomic.AddUintptr(p, d); }

public static int32 PreIncAtomicInt32P(uintptr p, int32 d) {
    return atomic.AddInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), d);
}

public static int64 PreIncAtomicInt64P(uintptr p, int64 d) {
    return atomic.AddInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)p), d);
}

public static uint32 PreIncAtomicUint32P(uintptr p, uint32 d) {
    return atomic.AddUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p), d);
}

public static uint64 PreIncAtomicUint64P(uintptr p, uint64 d) {
    return atomic.AddUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p), d);
}

public static uintptr PreInrAtomicUintptrP(uintptr p, uintptr d) {
    return atomic.AddUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p), d);
}

public static int8 PreDecInt8(ж<int8> Ꮡp, int8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static int16 PreDecInt16(ж<int16> Ꮡp, int16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static int32 PreDecInt32(ж<int32> Ꮡp, int32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static int64 PreDecInt64(ж<int64> Ꮡp, int64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static uint8 PreDecUint8(ж<uint8> Ꮡp, uint8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static uint16 PreDecUint16(ж<uint16> Ꮡp, uint16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static uint32 PreDecUint32(ж<uint32> Ꮡp, uint32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static uint64 PreDecUint64(ж<uint64> Ꮡp, uint64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static float32 PreDecFloat32(ж<float32> Ꮡp, float32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static float64 PreDecFloat64(ж<float64> Ꮡp, float64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static complex64 PreDecComplex64(ж<complex64> Ꮡp, complex64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static complex128 PreDecComplex128(ж<complex128> Ꮡp, complex128 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static uintptr PreDecUintptr(ж<uintptr> Ꮡp, uintptr d) {
    ref var p = ref Ꮡp.DerefOrNull();

    p -= d;
    return p;
}

public static int32 PreDecAtomicInt32(ж<int32> p, int32 d) { return atomic.AddInt32(p, -d); }
public static int64 PreDecAtomicInt64(ж<int64> p, int64 d) { return atomic.AddInt64(p, -d); }
public static uint32 PreDecAtomicUint32(ж<uint32> p, uint32 d) { return atomic.AddUint32(p, ((uint32)0 - d)); }
public static uint64 PreDecAtomicUint64(ж<uint64> p, uint64 d) { return atomic.AddUint64(p, ((uint64)0 - d)); }
public static uintptr PreDecAtomicUintptr(ж<uintptr> p, uintptr d) { return atomic.AddUintptr(p, ((uintptr)0 - d)); }

public static int8 PostIncInt8(ж<int8> Ꮡp, int8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static int16 PostIncInt16(ж<int16> Ꮡp, int16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static int32 PostIncInt32(ж<int32> Ꮡp, int32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static int64 PostIncInt64(ж<int64> Ꮡp, int64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static uint8 PostIncUint8(ж<uint8> Ꮡp, uint8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static uint16 PostIncUint16(ж<uint16> Ꮡp, uint16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static uint32 PostIncUint32(ж<uint32> Ꮡp, uint32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static uint64 PostIncUint64(ж<uint64> Ꮡp, uint64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static float32 PostIncFloat32(ж<float32> Ꮡp, float32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static float64 PostIncFloat64(ж<float64> Ꮡp, float64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static complex64 PostIncComplex64(ж<complex64> Ꮡp, complex64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static complex128 PostIncComplex128(ж<complex128> Ꮡp, complex128 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static uintptr PostIncUintptr(ж<uintptr> Ꮡp, uintptr d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p += d;
    return r;
}

public static int32 PostIncAtomicInt32(ж<int32> p, int32 d) { return atomic.AddInt32(p, d) - d; }
public static int64 PostIncAtomicInt64(ж<int64> p, int64 d) { return atomic.AddInt64(p, d) - d; }
public static uint32 PostIncAtomicUint32(ж<uint32> p, uint32 d) { return atomic.AddUint32(p, d) - d; }
public static uint64 PostIncAtomicUint64(ж<uint64> p, uint64 d) { return atomic.AddUint64(p, d) - d; }
public static uintptr PostIncAtomicUintptr(ж<uintptr> p, uintptr d) { return atomic.AddUintptr(p, d) - d; }

public static int32 PostIncAtomicInt32P(uintptr p, int32 d) {
    return atomic.AddInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), d) - d;
}
public static int64 PostIncAtomicInt64P(uintptr p, int64 d) {
    return atomic.AddInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)p), d) - d;
}
public static uint32 PostIncAtomicUint32P(uintptr p, uint32 d) {
    return atomic.AddUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p), d) - d;
}
public static uint64 PostIncAtomicUint64P(uintptr p, uint64 d) {
    return atomic.AddUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p), d) - d;
}
public static uintptr PostIncAtomicUintptrP(uintptr p, uintptr d) {
    return atomic.AddUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p), d) - d;
}

public static int8 PostDecInt8(ж<int8> Ꮡp, int8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static int16 PostDecInt16(ж<int16> Ꮡp, int16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static int32 PostDecInt32(ж<int32> Ꮡp, int32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static int64 PostDecInt64(ж<int64> Ꮡp, int64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static uint8 PostDecUint8(ж<uint8> Ꮡp, uint8 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static uint16 PostDecUint16(ж<uint16> Ꮡp, uint16 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static uint32 PostDecUint32(ж<uint32> Ꮡp, uint32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static uint64 PostDecUint64(ж<uint64> Ꮡp, uint64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static float32 PostDecFloat32(ж<float32> Ꮡp, float32 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static float64 PostDecFloat64(ж<float64> Ꮡp, float64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static complex64 PostDecComplex64(ж<complex64> Ꮡp, complex64 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static complex128 PostDecComplex128(ж<complex128> Ꮡp, complex128 d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static uintptr PostDecUintptr(ж<uintptr> Ꮡp, uintptr d) {
    ref var p = ref Ꮡp.DerefOrNull();

    var r = p;
    p -= d;
    return r;
}

public static int32 PostDecAtomicInt32(ж<int32> p, int32 d) { return atomic.AddInt32(p, -d) + d; }
public static int64 PostDecAtomicInt64(ж<int64> p, int64 d) { return atomic.AddInt64(p, -d) + d; }
public static uint32 PostDecAtomicUint32(ж<uint32> p, uint32 d) { return atomic.AddUint32(p, ((uint32)0 - d)) + d; }
public static uint64 PostDecAtomicUint64(ж<uint64> p, uint64 d) { return atomic.AddUint64(p, ((uint64)0 - d)) + d; }
public static uintptr PostDecAtomicUintptr(ж<uintptr> p, uintptr d) { return atomic.AddUintptr(p, ((uintptr)0 - d)) + d; }

public static int8 Int8FromInt8(int8 n) { return n; }
public static int8 Int8FromInt16(int16 n) { return (int8)n; }
public static int8 Int8FromInt32(int32 n) { return (int8)n; }
public static int8 Int8FromInt64(int64 n) { return (int8)n; }
public static int8 Int8FromUint8(uint8 n) { return (int8)n; }
public static int8 Int8FromUint16(uint16 n) { return (int8)n; }
public static int8 Int8FromUint32(uint32 n) { return (int8)n; }
public static int8 Int8FromUint64(uint64 n) { return (int8)n; }
public static int8 Int8FromFloat32(float32 n) { return (int8)n; }
public static int8 Int8FromFloat64(float64 n) { return (int8)n; }
public static int8 Int8FromComplex64(complex64 n) { return (int8)real(n); }
public static int8 Int8FromComplex128(complex128 n) { return (int8)real(n); }
public static int8 Int8FromUintptr(uintptr n) { return (int8)n; }
public static int16 Int16FromInt8(int8 n) { return (int16)n; }
public static int16 Int16FromInt16(int16 n) { return n; }
public static int16 Int16FromInt32(int32 n) { return (int16)n; }
public static int16 Int16FromInt64(int64 n) { return (int16)n; }
public static int16 Int16FromUint8(uint8 n) { return (int16)n; }
public static int16 Int16FromUint16(uint16 n) { return (int16)n; }
public static int16 Int16FromUint32(uint32 n) { return (int16)n; }
public static int16 Int16FromUint64(uint64 n) { return (int16)n; }
public static int16 Int16FromFloat32(float32 n) { return (int16)n; }
public static int16 Int16FromFloat64(float64 n) { return (int16)n; }
public static int16 Int16FromComplex64(complex64 n) { return (int16)real(n); }
public static int16 Int16FromComplex128(complex128 n) { return (int16)real(n); }
public static int16 Int16FromUintptr(uintptr n) { return (int16)n; }
public static int32 Int32FromInt8(int8 n) { return (int32)n; }
public static int32 Int32FromInt16(int16 n) { return (int32)n; }
public static int32 Int32FromInt32(int32 n) { return n; }
public static int32 Int32FromInt64(int64 n) { return (int32)n; }
public static int32 Int32FromUint8(uint8 n) { return (int32)n; }
public static int32 Int32FromUint16(uint16 n) { return (int32)n; }
public static int32 Int32FromUint32(uint32 n) { return (int32)n; }
public static int32 Int32FromUint64(uint64 n) { return (int32)n; }
public static int32 Int32FromFloat32(float32 n) { return (int32)n; }
public static int32 Int32FromFloat64(float64 n) { return (int32)n; }
public static int32 Int32FromComplex64(complex64 n) { return (int32)real(n); }
public static int32 Int32FromComplex128(complex128 n) { return (int32)real(n); }
public static int32 Int32FromUintptr(uintptr n) { return (int32)n; }
public static int64 Int64FromInt8(int8 n) { return (int64)n; }
public static int64 Int64FromInt16(int16 n) { return (int64)n; }
public static int64 Int64FromInt32(int32 n) { return (int64)n; }
public static int64 Int64FromInt64(int64 n) { return n; }
public static int64 Int64FromUint8(uint8 n) { return (int64)n; }
public static int64 Int64FromUint16(uint16 n) { return (int64)n; }
public static int64 Int64FromUint32(uint32 n) { return (int64)n; }
public static int64 Int64FromUint64(uint64 n) { return (int64)n; }
public static int64 Int64FromFloat32(float32 n) { return (int64)n; }
public static int64 Int64FromFloat64(float64 n) { return (int64)n; }
public static int64 Int64FromComplex64(complex64 n) { return (int64)real(n); }
public static int64 Int64FromComplex128(complex128 n) { return (int64)real(n); }
public static int64 Int64FromUintptr(uintptr n) { return (int64)n; }
public static uint8 Uint8FromInt8(int8 n) { return (uint8)n; }
public static uint8 Uint8FromInt16(int16 n) { return (uint8)n; }
public static uint8 Uint8FromInt32(int32 n) { return (uint8)n; }
public static uint8 Uint8FromInt64(int64 n) { return (uint8)n; }
public static uint8 Uint8FromUint8(uint8 n) { return n; }
public static uint8 Uint8FromUint16(uint16 n) { return (uint8)n; }
public static uint8 Uint8FromUint32(uint32 n) { return (uint8)n; }
public static uint8 Uint8FromUint64(uint64 n) { return (uint8)n; }
public static uint8 Uint8FromFloat32(float32 n) { return (uint8)n; }
public static uint8 Uint8FromFloat64(float64 n) { return (uint8)n; }
public static uint8 Uint8FromComplex64(complex64 n) { return (uint8)real(n); }
public static uint8 Uint8FromComplex128(complex128 n) { return (uint8)real(n); }
public static uint8 Uint8FromUintptr(uintptr n) { return (uint8)n; }
public static uint16 Uint16FromInt8(int8 n) { return (uint16)n; }
public static uint16 Uint16FromInt16(int16 n) { return (uint16)n; }
public static uint16 Uint16FromInt32(int32 n) { return (uint16)n; }
public static uint16 Uint16FromInt64(int64 n) { return (uint16)n; }
public static uint16 Uint16FromUint8(uint8 n) { return (uint16)n; }
public static uint16 Uint16FromUint16(uint16 n) { return n; }
public static uint16 Uint16FromUint32(uint32 n) { return (uint16)n; }
public static uint16 Uint16FromUint64(uint64 n) { return (uint16)n; }
public static uint16 Uint16FromFloat32(float32 n) { return (uint16)n; }
public static uint16 Uint16FromFloat64(float64 n) { return (uint16)n; }
public static uint16 Uint16FromComplex64(complex64 n) { return (uint16)real(n); }
public static uint16 Uint16FromComplex128(complex128 n) { return (uint16)real(n); }
public static uint16 Uint16FromUintptr(uintptr n) { return (uint16)n; }
public static uint32 Uint32FromInt8(int8 n) { return (uint32)n; }
public static uint32 Uint32FromInt16(int16 n) { return (uint32)n; }
public static uint32 Uint32FromInt32(int32 n) { return (uint32)n; }
public static uint32 Uint32FromInt64(int64 n) { return (uint32)n; }
public static uint32 Uint32FromUint8(uint8 n) { return (uint32)n; }
public static uint32 Uint32FromUint16(uint16 n) { return (uint32)n; }
public static uint32 Uint32FromUint32(uint32 n) { return n; }
public static uint32 Uint32FromUint64(uint64 n) { return (uint32)n; }
public static uint32 Uint32FromFloat32(float32 n) { return (uint32)n; }
public static uint32 Uint32FromFloat64(float64 n) { return (uint32)n; }
public static uint32 Uint32FromComplex64(complex64 n) { return (uint32)real(n); }
public static uint32 Uint32FromComplex128(complex128 n) { return (uint32)real(n); }
public static uint32 Uint32FromUintptr(uintptr n) { return (uint32)n; }
public static uint64 Uint64FromInt8(int8 n) { return (uint64)n; }
public static uint64 Uint64FromInt16(int16 n) { return (uint64)n; }
public static uint64 Uint64FromInt32(int32 n) { return (uint64)n; }
public static uint64 Uint64FromInt64(int64 n) { return (uint64)n; }
public static uint64 Uint64FromUint8(uint8 n) { return (uint64)n; }
public static uint64 Uint64FromUint16(uint16 n) { return (uint64)n; }
public static uint64 Uint64FromUint32(uint32 n) { return (uint64)n; }
public static uint64 Uint64FromUint64(uint64 n) { return n; }
public static uint64 Uint64FromFloat32(float32 n) { return (uint64)n; }
public static uint64 Uint64FromFloat64(float64 n) { return (uint64)n; }
public static uint64 Uint64FromComplex64(complex64 n) { return (uint64)real(n); }
public static uint64 Uint64FromComplex128(complex128 n) { return (uint64)real(n); }
public static uint64 Uint64FromUintptr(uintptr n) { return (uint64)n; }
public static float32 Float32FromInt8(int8 n) { return (float32)n; }
public static float32 Float32FromInt16(int16 n) { return (float32)n; }
public static float32 Float32FromInt32(int32 n) { return (float32)n; }
public static float32 Float32FromInt64(int64 n) { return (float32)n; }
public static float32 Float32FromUint8(uint8 n) { return (float32)n; }
public static float32 Float32FromUint16(uint16 n) { return (float32)n; }
public static float32 Float32FromUint32(uint32 n) { return (float32)n; }
public static float32 Float32FromUint64(uint64 n) { return (float32)n; }
public static float32 Float32FromFloat32(float32 n) { return n; }
public static float32 Float32FromFloat64(float64 n) { return (float32)n; }
public static float32 Float32FromComplex64(complex64 n) { return real(n); }
public static float32 Float32FromComplex128(complex128 n) { return (float32)real(n); }
public static float32 Float32FromUintptr(uintptr n) { return (float32)n; }
public static float64 Float64FromInt8(int8 n) { return (float64)n; }
public static float64 Float64FromInt16(int16 n) { return (float64)n; }
public static float64 Float64FromInt32(int32 n) { return (float64)n; }
public static float64 Float64FromInt64(int64 n) { return (float64)n; }
public static float64 Float64FromUint8(uint8 n) { return (float64)n; }
public static float64 Float64FromUint16(uint16 n) { return (float64)n; }
public static float64 Float64FromUint32(uint32 n) { return (float64)n; }
public static float64 Float64FromUint64(uint64 n) { return (float64)n; }
public static float64 Float64FromFloat32(float32 n) { return (float64)n; }
public static float64 Float64FromFloat64(float64 n) { return n; }
public static float64 Float64FromComplex64(complex64 n) { return (float64)real(n); }
public static float64 Float64FromComplex128(complex128 n) { return real(n); }
public static float64 Float64FromUintptr(uintptr n) { return (float64)n; }
public static complex64 Complex64FromInt8(int8 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromInt16(int16 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromInt32(int32 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromInt64(int64 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromUint8(uint8 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromUint16(uint16 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromUint32(uint32 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromUint64(uint64 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromFloat32(float32 n) { return complex(n, 0F); }
public static complex64 Complex64FromFloat64(float64 n) { return complex((float32)n, 0F); }
public static complex64 Complex64FromComplex64(complex64 n) { return n; }
public static complex64 Complex64FromComplex128(complex128 n) { return (complex64)n; }
public static complex64 Complex64FromUintptr(uintptr n) { return complex((float32)n, 0F); }
public static complex128 Complex128FromInt8(int8 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromInt16(int16 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromInt32(int32 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromInt64(int64 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromUint8(uint8 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromUint16(uint16 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromUint32(uint32 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromUint64(uint64 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromFloat32(float32 n) { return complex((float64)n, 0D); }
public static complex128 Complex128FromFloat64(float64 n) { return complex(n, 0D); }
public static complex128 Complex128FromComplex64(complex64 n) { return (complex128)n; }
public static complex128 Complex128FromComplex128(complex128 n) { return n; }
public static complex128 Complex128FromUintptr(uintptr n) { return complex((float64)n, 0D); }
public static uintptr UintptrFromInt8(int8 n) { return (uintptr)n; }
public static uintptr UintptrFromInt16(int16 n) { return (uintptr)n; }
public static uintptr UintptrFromInt32(int32 n) { return (uintptr)n; }
public static uintptr UintptrFromInt64(int64 n) { return (uintptr)n; }
public static uintptr UintptrFromUint8(uint8 n) { return (uintptr)n; }
public static uintptr UintptrFromUint16(uint16 n) { return (uintptr)n; }
public static uintptr UintptrFromUint32(uint32 n) { return (uintptr)n; }
public static uintptr UintptrFromUint64(uint64 n) { return (uintptr)n; }
public static uintptr UintptrFromFloat32(float32 n) { return (uintptr)n; }
public static uintptr UintptrFromFloat64(float64 n) { return (uintptr)n; }
public static uintptr UintptrFromComplex64(complex64 n) { return (uintptr)real(n); }
public static uintptr UintptrFromComplex128(complex128 n) { return (uintptr)real(n); }
public static uintptr UintptrFromUintptr(uintptr n) { return n; }

public static int8 Int8(int8 n) { return n; }
public static int16 Int16(int16 n) { return n; }
public static int32 Int32(int32 n) { return n; }
public static int64 Int64(int64 n) { return n; }
public static uint8 Uint8(uint8 n) { return n; }
public static uint16 Uint16(uint16 n) { return n; }
public static uint32 Uint32(uint32 n) { return n; }
public static uint64 Uint64(uint64 n) { return n; }
public static float32 Float32(float32 n) { return n; }
public static float64 Float64(float64 n) { return n; }
public static complex64 Complex64(complex64 n) { return n; }
public static complex128 Complex128(complex128 n) { return n; }
public static uintptr Uintptr(uintptr n) { return n; }

public static int8 NegInt8(int8 n) { return (int8)(-n); }
public static int16 NegInt16(int16 n) { return (int16)(-n); }
public static int32 NegInt32(int32 n) { return -n; }
public static int64 NegInt64(int64 n) { return -n; }
public static uint8 NegUint8(uint8 n) { return (uint8)(((uint8)0 - n)); }
public static uint16 NegUint16(uint16 n) { return (uint16)(((uint16)0 - n)); }
public static uint32 NegUint32(uint32 n) { return ((uint32)0 - n); }
public static uint64 NegUint64(uint64 n) { return ((uint64)0 - n); }
public static uintptr NegUintptr(uintptr n) { return ((uintptr)0 - n); }

public static int8 CplInt8(int8 n) { return (int8)(~n); }
public static int16 CplInt16(int16 n) { return (int16)(~n); }
public static int32 CplInt32(int32 n) { return ~n; }
public static int64 CplInt64(int64 n) { return ~n; }
public static uint8 CplUint8(uint8 n) { return (uint8)(((uint8)(~n))); }
public static uint16 CplUint16(uint16 n) { return (uint16)(((uint16)(~n))); }
public static uint32 CplUint32(uint32 n) { return ~n; }
public static uint64 CplUint64(uint64 n) { return ~n; }
public static uintptr CplUintptr(uintptr n) { return ~n; }

public static int8 BoolInt8(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static int16 BoolInt16(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static int32 BoolInt32(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static int64 BoolInt64(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static uint8 BoolUint8(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static uint16 BoolUint16(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static uint32 BoolUint32(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static uint64 BoolUint64(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static uintptr BoolUintptr(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

public static void SetBitFieldPtr8Int8(uintptr p, int8 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Int16(uintptr p, int16 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Int32(uintptr p, int32 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Int64(uintptr p, int64 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Uint8(uintptr p, uint8 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(v.Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Uint16(uintptr p, uint16 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Uint32(uintptr p, uint32 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr8Uint64(uintptr p, uint64 v, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Int8(uintptr p, int8 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Int16(uintptr p, int16 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Int32(uintptr p, int32 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Int64(uintptr p, int64 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Uint8(uintptr p, uint8 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Uint16(uintptr p, uint16 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(v.Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Uint32(uintptr p, uint32 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr16Uint64(uintptr p, uint64 v, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
}

public static void SetBitFieldPtr32Int8(uintptr p, int8 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Int16(uintptr p, int16 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Int32(uintptr p, int32 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Int64(uintptr p, int64 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Uint8(uintptr p, uint8 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Uint16(uintptr p, uint16 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Uint32(uintptr p, uint32 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | v.Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr32Uint64(uintptr p, uint64 v, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Int8(uintptr p, int8 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Int16(uintptr p, int16 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Int32(uintptr p, int32 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Int64(uintptr p, int64 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Uint8(uintptr p, uint8 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Uint16(uintptr p, uint16 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Uint32(uintptr p, uint32 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
}

public static void SetBitFieldPtr64Uint64(uintptr p, uint64 v, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | v.Lsh((int64)(off)) & mask;
}

public static int8 AssignBitFieldPtr8Int8(uintptr p, int8 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    nint s = 8 - w;
    return (int8)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int16 AssignBitFieldPtr8Int16(uintptr p, int16 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    nint s = 16 - w;
    return (int16)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int32 AssignBitFieldPtr8Int32(uintptr p, int32 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    nint s = 32 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int64 AssignBitFieldPtr8Int64(uintptr p, int64 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    nint s = 64 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int8 AssignBitFieldPtr16Int8(uintptr p, int8 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    nint s = 8 - w;
    return (int8)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int16 AssignBitFieldPtr16Int16(uintptr p, int16 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    nint s = 16 - w;
    return (int16)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int32 AssignBitFieldPtr16Int32(uintptr p, int32 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    nint s = 32 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int64 AssignBitFieldPtr16Int64(uintptr p, int64 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    nint s = 64 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int8 AssignBitFieldPtr32Int8(uintptr p, int8 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    nint s = 8 - w;
    return (int8)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int16 AssignBitFieldPtr32Int16(uintptr p, int16 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    nint s = 16 - w;
    return (int16)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int32 AssignBitFieldPtr32Int32(uintptr p, int32 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    nint s = 32 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int64 AssignBitFieldPtr32Int64(uintptr p, int64 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    nint s = 64 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int8 AssignBitFieldPtr64Int8(uintptr p, int8 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    nint s = 8 - w;
    return (int8)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int16 AssignBitFieldPtr64Int16(uintptr p, int16 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    nint s = 16 - w;
    return (int16)((v.Lsh((int64)(s))).Rsh((int64)(s)));
}

public static int32 AssignBitFieldPtr64Int32(uintptr p, int32 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    nint s = 32 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static int64 AssignBitFieldPtr64Int64(uintptr p, int64 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    nint s = 64 - w;
    return (v.Lsh((int64)(s))).Rsh((int64)(s));
}

public static uint8 AssignBitFieldPtr8Uint8(uintptr p, uint8 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(v.Lsh((int64)(off)) & mask));
    return (uint8)(v & (mask.Rsh((int64)(off))));
}

public static uint16 AssignBitFieldPtr8Uint16(uintptr p, uint16 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    return (uint16)(v & (uint16)(mask.Rsh((int64)(off))));
}

public static uint32 AssignBitFieldPtr8Uint32(uintptr p, uint32 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    return v & (uint32)(mask.Rsh((int64)(off)));
}

public static uint64 AssignBitFieldPtr8Uint64(uintptr p, uint64 v, nint w, nint off, uint8 mask) {
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p) & ~mask) | (uint8)(((uint8)v).Lsh((int64)(off)) & mask));
    return v & (uint64)(mask.Rsh((int64)(off)));
}

public static uint8 AssignBitFieldPtr16Uint8(uintptr p, uint8 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    return (uint8)(v & (uint8)(mask.Rsh((int64)(off))));
}

public static uint16 AssignBitFieldPtr16Uint16(uintptr p, uint16 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(v.Lsh((int64)(off)) & mask));
    return (uint16)(v & (mask.Rsh((int64)(off))));
}

public static uint32 AssignBitFieldPtr16Uint32(uintptr p, uint32 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    return v & (uint32)(mask.Rsh((int64)(off)));
}

public static uint64 AssignBitFieldPtr16Uint64(uintptr p, uint64 v, nint w, nint off, uint16 mask) {
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p) & ~mask) | (uint16)(((uint16)v).Lsh((int64)(off)) & mask));
    return v & (uint64)(mask.Rsh((int64)(off)));
}

public static uint8 AssignBitFieldPtr32Uint8(uintptr p, uint8 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    return (uint8)(v & (uint8)(mask.Rsh((int64)(off))));
}

public static uint16 AssignBitFieldPtr32Uint16(uintptr p, uint16 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    return (uint16)(v & (uint16)(mask.Rsh((int64)(off))));
}

public static uint32 AssignBitFieldPtr32Uint32(uintptr p, uint32 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | v.Lsh((int64)(off)) & mask;
    return v & (mask.Rsh((int64)(off)));
}

public static uint64 AssignBitFieldPtr32Uint64(uintptr p, uint64 v, nint w, nint off, uint32 mask) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p) & ~mask | ((uint32)v).Lsh((int64)(off)) & mask;
    return v & (uint64)(mask.Rsh((int64)(off)));
}

public static uint8 AssignBitFieldPtr64Uint8(uintptr p, uint8 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    return (uint8)(v & (uint8)(mask.Rsh((int64)(off))));
}

public static uint16 AssignBitFieldPtr64Uint16(uintptr p, uint16 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    return (uint16)(v & (uint16)(mask.Rsh((int64)(off))));
}

public static uint32 AssignBitFieldPtr64Uint32(uintptr p, uint32 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | ((uint64)v).Lsh((int64)(off)) & mask;
    return v & (uint32)(mask.Rsh((int64)(off)));
}

public static uint64 AssignBitFieldPtr64Uint64(uintptr p, uint64 v, nint w, nint off, uint64 mask) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p) & ~mask | v.Lsh((int64)(off)) & mask;
    return v & (mask.Rsh((int64)(off)));
}

public static int8 /*r*/ PostDecBitFieldPtr8Int8(uintptr p, int8 d, nint w, nint off, uint8 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int16 /*r*/ PostDecBitFieldPtr8Int16(uintptr p, int16 d, nint w, nint off, uint8 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int32 /*r*/ PostDecBitFieldPtr8Int32(uintptr p, int32 d, nint w, nint off, uint8 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int64 /*r*/ PostDecBitFieldPtr8Int64(uintptr p, int64 d, nint w, nint off, uint8 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int8 /*r*/ PostDecBitFieldPtr16Int8(uintptr p, int8 d, nint w, nint off, uint16 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)((int8)(r - d))).Lsh((int64)(off)) & mask));
    return r;
}

public static int16 /*r*/ PostDecBitFieldPtr16Int16(uintptr p, int16 d, nint w, nint off, uint16 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int32 /*r*/ PostDecBitFieldPtr16Int32(uintptr p, int32 d, nint w, nint off, uint16 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int64 /*r*/ PostDecBitFieldPtr16Int64(uintptr p, int64 d, nint w, nint off, uint16 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int8 /*r*/ PostDecBitFieldPtr32Int8(uintptr p, int8 d, nint w, nint off, uint32 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((int8)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int16 /*r*/ PostDecBitFieldPtr32Int16(uintptr p, int16 d, nint w, nint off, uint32 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((int16)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int32 /*r*/ PostDecBitFieldPtr32Int32(uintptr p, int32 d, nint w, nint off, uint32 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)(r - d)).Lsh((int64)(off)) & mask;
    return r;
}

public static int64 /*r*/ PostDecBitFieldPtr32Int64(uintptr p, int64 d, nint w, nint off, uint32 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)(r - d)).Lsh((int64)(off)) & mask;
    return r;
}

public static int8 /*r*/ PostDecBitFieldPtr64Int8(uintptr p, int8 d, nint w, nint off, uint64 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((int8)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int16 /*r*/ PostDecBitFieldPtr64Int16(uintptr p, int16 d, nint w, nint off, uint64 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((int16)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int32 /*r*/ PostDecBitFieldPtr64Int32(uintptr p, int32 d, nint w, nint off, uint64 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)(r - d)).Lsh((int64)(off)) & mask;
    return r;
}

public static int64 /*r*/ PostDecBitFieldPtr64Int64(uintptr p, int64 d, nint w, nint off, uint64 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)(r - d)).Lsh((int64)(off)) & mask;
    return r;
}

public static uint8 /*r*/ PostDecBitFieldPtr8Uint8(uintptr p, uint8 d, nint w, nint off, uint8 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)(x0 & mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint16 /*r*/ PostDecBitFieldPtr8Uint16(uintptr p, uint16 d, nint w, nint off, uint8 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)((uint16)x0 & (uint16)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint32 /*r*/ PostDecBitFieldPtr8Uint32(uintptr p, uint32 d, nint w, nint off, uint8 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (((uint32)x0 & (uint32)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint64 /*r*/ PostDecBitFieldPtr8Uint64(uintptr p, uint64 d, nint w, nint off, uint8 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (((uint64)x0 & (uint64)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint8 /*r*/ PostDecBitFieldPtr16Uint8(uintptr p, uint8 d, nint w, nint off, uint16 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)((uint8)x0 & (uint8)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)((uint8)(r - d))).Lsh((int64)(off)) & mask));
    return r;
}

public static uint16 /*r*/ PostDecBitFieldPtr16Uint16(uintptr p, uint16 d, nint w, nint off, uint16 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)(x0 & mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint32 /*r*/ PostDecBitFieldPtr16Uint32(uintptr p, uint32 d, nint w, nint off, uint16 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (((uint32)x0 & (uint32)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint64 /*r*/ PostDecBitFieldPtr16Uint64(uintptr p, uint64 d, nint w, nint off, uint16 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (((uint64)x0 & (uint64)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r - d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint8 /*r*/ PostDecBitFieldPtr32Uint8(uintptr p, uint8 d, nint w, nint off, uint32 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)((uint8)x0 & (uint8)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((uint8)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint16 /*r*/ PostDecBitFieldPtr32Uint16(uintptr p, uint16 d, nint w, nint off, uint32 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)((uint16)x0 & (uint16)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((uint16)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint32 /*r*/ PostDecBitFieldPtr32Uint32(uintptr p, uint32 d, nint w, nint off, uint32 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = ((x0 & mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | (r - d).Lsh((int64)(off)) & mask;
    return r;
}

public static uint64 /*r*/ PostDecBitFieldPtr32Uint64(uintptr p, uint64 d, nint w, nint off, uint32 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = (((uint64)x0 & (uint64)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)(r - d)).Lsh((int64)(off)) & mask;
    return r;
}

public static uint8 /*r*/ PostDecBitFieldPtr64Uint8(uintptr p, uint8 d, nint w, nint off, uint64 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)((uint8)x0 & (uint8)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((uint8)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint16 /*r*/ PostDecBitFieldPtr64Uint16(uintptr p, uint16 d, nint w, nint off, uint64 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)((uint16)x0 & (uint16)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((uint16)(r - d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint32 /*r*/ PostDecBitFieldPtr64Uint32(uintptr p, uint32 d, nint w, nint off, uint64 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = (((uint32)x0 & (uint32)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)(r - d)).Lsh((int64)(off)) & mask;
    return r;
}

public static uint64 /*r*/ PostDecBitFieldPtr64Uint64(uintptr p, uint64 d, nint w, nint off, uint64 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = ((x0 & mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | (r - d).Lsh((int64)(off)) & mask;
    return r;
}

public static int8 /*r*/ PostIncBitFieldPtr8Int8(uintptr p, int8 d, nint w, nint off, uint8 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int16 /*r*/ PostIncBitFieldPtr8Int16(uintptr p, int16 d, nint w, nint off, uint8 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int32 /*r*/ PostIncBitFieldPtr8Int32(uintptr p, int32 d, nint w, nint off, uint8 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int64 /*r*/ PostIncBitFieldPtr8Int64(uintptr p, int64 d, nint w, nint off, uint8 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int8 /*r*/ PostIncBitFieldPtr16Int8(uintptr p, int8 d, nint w, nint off, uint16 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)((int8)(r + d))).Lsh((int64)(off)) & mask));
    return r;
}

public static int16 /*r*/ PostIncBitFieldPtr16Int16(uintptr p, int16 d, nint w, nint off, uint16 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int32 /*r*/ PostIncBitFieldPtr16Int32(uintptr p, int32 d, nint w, nint off, uint16 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int64 /*r*/ PostIncBitFieldPtr16Int64(uintptr p, int64 d, nint w, nint off, uint16 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static int8 /*r*/ PostIncBitFieldPtr32Int8(uintptr p, int8 d, nint w, nint off, uint32 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((int8)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int16 /*r*/ PostIncBitFieldPtr32Int16(uintptr p, int16 d, nint w, nint off, uint32 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((int16)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int32 /*r*/ PostIncBitFieldPtr32Int32(uintptr p, int32 d, nint w, nint off, uint32 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)(r + d)).Lsh((int64)(off)) & mask;
    return r;
}

public static int64 /*r*/ PostIncBitFieldPtr32Int64(uintptr p, int64 d, nint w, nint off, uint32 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)(r + d)).Lsh((int64)(off)) & mask;
    return r;
}

public static int8 /*r*/ PostIncBitFieldPtr64Int8(uintptr p, int8 d, nint w, nint off, uint64 mask) {
    int8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 8 - w - off;
    r = (int8)((((int8)((int8)x0 & (int8)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((int8)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int16 /*r*/ PostIncBitFieldPtr64Int16(uintptr p, int16 d, nint w, nint off, uint64 mask) {
    int16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 16 - w - off;
    r = (int16)((((int16)((int16)x0 & (int16)mask)).Lsh((int64)(s))).Rsh((int64)(s + off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((int16)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static int32 /*r*/ PostIncBitFieldPtr64Int32(uintptr p, int32 d, nint w, nint off, uint64 mask) {
    int32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 32 - w - off;
    r = ((((int32)x0 & (int32)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)(r + d)).Lsh((int64)(off)) & mask;
    return r;
}

public static int64 /*r*/ PostIncBitFieldPtr64Int64(uintptr p, int64 d, nint w, nint off, uint64 mask) {
    int64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    nint s = 64 - w - off;
    r = ((((int64)x0 & (int64)mask)).Lsh((int64)(s))).Rsh((int64)(s + off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)(r + d)).Lsh((int64)(off)) & mask;
    return r;
}

public static uint8 /*r*/ PostIncBitFieldPtr8Uint8(uintptr p, uint8 d, nint w, nint off, uint8 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)(x0 & mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint16 /*r*/ PostIncBitFieldPtr8Uint16(uintptr p, uint16 d, nint w, nint off, uint8 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)((uint16)x0 & (uint16)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint32 /*r*/ PostIncBitFieldPtr8Uint32(uintptr p, uint32 d, nint w, nint off, uint8 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (((uint32)x0 & (uint32)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint64 /*r*/ PostIncBitFieldPtr8Uint64(uintptr p, uint64 d, nint w, nint off, uint8 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p);
    r = (((uint64)x0 & (uint64)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p)).Value = (uint8)((uint8)(x0 & ~mask) | (uint8)(((uint8)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint8 /*r*/ PostIncBitFieldPtr16Uint8(uintptr p, uint8 d, nint w, nint off, uint16 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)((uint8)x0 & (uint8)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)((uint8)(r + d))).Lsh((int64)(off)) & mask));
    return r;
}

public static uint16 /*r*/ PostIncBitFieldPtr16Uint16(uintptr p, uint16 d, nint w, nint off, uint16 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)(x0 & mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint32 /*r*/ PostIncBitFieldPtr16Uint32(uintptr p, uint32 d, nint w, nint off, uint16 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (((uint32)x0 & (uint32)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint64 /*r*/ PostIncBitFieldPtr16Uint64(uintptr p, uint64 d, nint w, nint off, uint16 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p);
    r = (((uint64)x0 & (uint64)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p)).Value = (uint16)((uint16)(x0 & ~mask) | (uint16)(((uint16)(r + d)).Lsh((int64)(off)) & mask));
    return r;
}

public static uint8 /*r*/ PostIncBitFieldPtr32Uint8(uintptr p, uint8 d, nint w, nint off, uint32 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)((uint8)x0 & (uint8)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((uint8)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint16 /*r*/ PostIncBitFieldPtr32Uint16(uintptr p, uint16 d, nint w, nint off, uint32 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)((uint16)x0 & (uint16)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)((uint16)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint32 /*r*/ PostIncBitFieldPtr32Uint32(uintptr p, uint32 d, nint w, nint off, uint32 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = ((x0 & mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | (r + d).Lsh((int64)(off)) & mask;
    return r;
}

public static uint64 /*r*/ PostIncBitFieldPtr32Uint64(uintptr p, uint64 d, nint w, nint off, uint32 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p);
    r = (((uint64)x0 & (uint64)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint32)(r + d)).Lsh((int64)(off)) & mask;
    return r;
}

public static uint8 /*r*/ PostIncBitFieldPtr64Uint8(uintptr p, uint8 d, nint w, nint off, uint64 mask) {
    uint8 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = (uint8)(((uint8)((uint8)x0 & (uint8)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((uint8)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint16 /*r*/ PostIncBitFieldPtr64Uint16(uintptr p, uint16 d, nint w, nint off, uint64 mask) {
    uint16 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = (uint16)(((uint16)((uint16)x0 & (uint16)mask)).Rsh((int64)(off)));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)((uint16)(r + d))).Lsh((int64)(off)) & mask;
    return r;
}

public static uint32 /*r*/ PostIncBitFieldPtr64Uint32(uintptr p, uint32 d, nint w, nint off, uint64 mask) {
    uint32 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = (((uint32)x0 & (uint32)mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | ((uint64)(r + d)).Lsh((int64)(off)) & mask;
    return r;
}

public static uint64 /*r*/ PostIncBitFieldPtr64Uint64(uintptr p, uint64 d, nint w, nint off, uint64 mask) {
    uint64 r = default;

    var x0 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p);
    r = ((x0 & mask)).Rsh((int64)(off));
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x0 & ~mask | (r + d).Lsh((int64)(off)) & mask;
    return r;
}

public static slice<@string> /*r*/ GetEnviron() {
    slice<@string> r = default;

    for (var p = Xenviron; ; p += /* unsafe.Sizeof(p) */ (uintptr)8) {
        var q = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
        if (q == 0) {
            return r;
        }

        r = append(r, GoString(q));
    }
}

} // end libc_package
