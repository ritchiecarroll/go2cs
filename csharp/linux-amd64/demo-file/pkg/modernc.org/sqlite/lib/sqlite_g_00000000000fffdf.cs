namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static float64 /*r*/ _degToRad(ж<libc.TLS> tls, float64 x) {
    return (float64)(x * (libc.Float64FromFloat64(3.141592653589793D) / libc.Float64FromFloat64(180D)));
}

internal static float64 /*r*/ _geopolyArea(ref libc.TLS tls, uintptr p) {
    int32 ii = default;
    float64 rArea = default;
    _ = ii;
    _ = rArea;
    rArea = (float64)0D;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p)).Value.FnVertex - (int32)1)) {
            break;
        }
        rArea = rArea + (float64)((float64)((~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2) * 4)) - ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)((ii + (int32)1) * (int32)2) * 4))) * (~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4)) + ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)((ii + (int32)1) * (int32)2 + (int32)1) * 4)))) * (float64)0.5D);
        goto _1;
_1:;
        ii = ii + 1;
    }
    rArea = rArea + (float64)((float64)((~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2) * 4)) - ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2)) * 4))) * (~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4)) + ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4)))) * (float64)0.5D);
    return rArea;
}

internal static float64 /*r1*/ _geopolySine(ref libc.TLS tls, float64 r) {
    float64 r1 = default;

    float64 r2 = default;
    float64 r3 = default;
    float64 r5 = default;
    _ = r2;
    _ = r3;
    _ = r5;
    if (r >= (float64)(libc.Float64FromFloat64(1.5D) * libc.Float64FromFloat64(3.141592653589793D))) {
        r = r - (float64)(libc.Float64FromFloat64(2D) * libc.Float64FromFloat64(3.141592653589793D));
    }
    if (r >= (float64)(libc.Float64FromFloat64(0.5D) * libc.Float64FromFloat64(3.141592653589793D))) {
        return -_geopolySine(ref tls, r - (float64)3.141592653589793D);
    } else {
        r2 = (float64)(r * r);
        r3 = (float64)(r2 * r);
        r5 = (float64)(r3 * r2);
        return (float64)((float64)0.9996949D * r) - (float64)((float64)0.16567D * r3) + (float64)((float64)0.0075134D * r5);
    }
    return r1;
}

internal static int32 /*r*/ _pointBeneathLine(ref libc.TLS tls, float64 x0, float64 y0, float64 x1, float64 y1, float64 x2, float64 y2) {
    float64 y = default;
    _ = y;
    if (x0 == x1 && y0 == y1) {
        return (int32)2;
    }
    if (x1 < x2) {
        if (x0 <= x1 || x0 > x2) {
            return 0;
        }
    } else {
        if (x1 > x2) {
            if (x0 <= x2 || x0 > x1) {
                return 0;
            }
        } else {
            if (x0 != x1) {
                return 0;
            }
            if (y0 < y1 && y0 < y2) {
                return 0;
            }
            if (y0 > y1 && y0 > y2) {
                return 0;
            }
            return (int32)2;
        }
    }
    y = y1 + (float64)((y2 - y1) * (x0 - x1)) / (x2 - x1);
    if (y0 == y) {
        return (int32)2;
    }
    if (y0 < y) {
        return (int32)1;
    }
    return 0;
}

internal static float64 /*r*/ _radToDeg(ж<libc.TLS> tls, float64 x) {
    return (float64)(x * (libc.Float64FromFloat64(180D) / libc.Float64FromFloat64(3.141592653589793D)));
}

internal static int32 /*r*/ _sqlite3OsCurrentTimeInt64(ж<libc.TLS> tls, uintptr pVfs, uintptr pTimeOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        float64 _ᴛ1 = default;
        _ = rc;
        if ((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FiVersion >= (int32)2 && (@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FxCurrentTimeInt64 != 0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FxCurrentTimeInt64)(tls, pVfs, pTimeOut);
        } else {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FxCurrentTime)(tls, pVfs, bp);
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(pTimeOut))).Value = _sqlite3RealToI64(ref (tls).DerefOrNull(), (float64)(~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp)) * (float64)8.64e+07D));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end sqlite3_package
