namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using @unsafe = unsafe_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _geopolyRegularFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Ti64 n = default;
        uintptr p = default;
        float64 r = default;
        float64 rAngle = default;
        float64 x = default;
        float64 y = default;
        int32 _ᴛ1 = default;
        _ = n;
        _ = p;
        _ = r;
        _ = rAngle;
        _ = x;
        _ = y;
        x = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        y = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
        r = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)));
        n = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 3 * 8)));
        _ = argc;
        if (n < (int64)3 || r <= (float64)0D || r >= (float64)1e+300D) {
            return;
        }
        if (n > (int64)1000) {
            n = (int64)1000;
        }
        p = Xsqlite3_malloc64(tls, (uint64)40 + libc.Uint64FromInt64((n - (int64)1) * (int64)2) * (uint64)4);
        if (p == (uintptr)0) {
            Xsqlite3_result_error_nomem(tls, context);
            return;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)1;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 4))).Value = (@unsafe.Pointer.To<uint8>(__ccgo_ptr(bp))).Value;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 4 + 1))).Value = (uint8)0;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 4 + 2))).Value = libc.Uint8FromInt64((Ti64)(n >> (int)(libc.Int32FromInt32(8)) & (int64)0xff));
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 4 + 3))).Value = libc.Uint8FromInt64((Ti64)(n & (int64)0xff));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
        for (;;) {
            if (!((int64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))) < n)) {
                break;
            }
            rAngle = (float64)((float64)(libc.Float64FromFloat64(2D) * libc.Float64FromFloat64(3.141592653589793D)) * (float64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)))) / (float64)n;
            (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) * (int32)2) * 4))).Value = (float32)(x - (float64)(r * _geopolySine(ref (tls).DerefOrNull(), rAngle - (float64)(libc.Float64FromFloat64(0.5D) * libc.Float64FromFloat64(3.141592653589793D)))));
            (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) * (int32)2 + (int32)1) * 4))).Value = (float32)(y + (float64)(r * _geopolySine(ref (tls).DerefOrNull(), rAngle)));
            goto _1;
_1:;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + 1;
        }
        Xsqlite3_result_blob(tls, context, p + 4, (int32)((int64)4 + (int64)8 * n), (uintptr)(-libc.Int32FromInt32(1)));
        Xsqlite3_free(tls, p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
