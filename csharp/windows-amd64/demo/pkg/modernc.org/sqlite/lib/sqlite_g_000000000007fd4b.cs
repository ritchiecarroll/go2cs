namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _geopolyAddOneSegment(ref libc.TLS tls, uintptr p, TGeoCoord x0, TGeoCoord y0, TGeoCoord x1, TGeoCoord y1, uint8 side, uint32 idx) {
    uintptr pEvent = default;
    uintptr pSeg = default;
    TGeoCoord t = default;
    _ = pEvent;
    _ = pSeg;
    _ = t;
    if (x0 == x1) {
        return;
    }
    if (x0 > x1) {
        t = x0;
        x0 = x1;
        x1 = t;
        t = y0;
        y0 = y1;
        y1 = t;
    }
    pSeg = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaSegment + (uintptr)(@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnSegment * 48;
    (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnSegment = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnSegment + 1;
    (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FC = (float64)((y1 - y0) / (x1 - x0));
    (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FB = (float64)y1 - (float64)((float64)x1 * (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.FC);
    (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fy0 = y0;
    (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fside = side;
    (@unsafe.Pointer.To<TGeoSegment>((@unsafe.Pointer)pSeg)).Value.Fidx = idx;
    pEvent = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaEvent + (uintptr)(@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent * 32;
    (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent + 1;
    (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pEvent)).Value.Fx = (float64)x0;
    (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pEvent)).Value.FeType = 0;
    (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pEvent)).Value.FpSeg = pSeg;
    pEvent = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FaEvent + (uintptr)(@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent * 32;
    (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent = (@unsafe.Pointer.To<TGeoOverlap>((@unsafe.Pointer)p)).Value.FnEvent + 1;
    (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pEvent)).Value.Fx = (float64)x1;
    (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pEvent)).Value.FeType = (int32)1;
    (@unsafe.Pointer.To<TGeoEvent>((@unsafe.Pointer)pEvent)).Value.FpSeg = pSeg;
}

internal static void _geopolyXformFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    float64 A = default;
    float64 B = default;
    float64 C = default;
    float64 D = default;
    float64 E = default;
    float64 F = default;
    int32 ii = default;
    uintptr p = default;
    TGeoCoord x0 = default;
    TGeoCoord x1 = default;
    TGeoCoord y0 = default;
    TGeoCoord y1 = default;
    _ = A;
    _ = B;
    _ = C;
    _ = D;
    _ = E;
    _ = F;
    _ = ii;
    _ = p;
    _ = x0;
    _ = x1;
    _ = y0;
    _ = y1;
    p = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), nil);
    A = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    B = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)));
    C = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 3 * 8)));
    D = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 4 * 8)));
    E = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 5 * 8)));
    F = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 6 * 8)));
    _ = argc;
    if (p != 0) {
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p)).Value.FnVertex)) {
                break;
            }
            x0 = ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2) * 4));
            y0 = ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4));
            x1 = (float32)((float64)(A * (float64)x0) + (float64)(B * (float64)y0) + E);
            y1 = (float32)((float64)(C * (float64)x0) + (float64)(D * (float64)y0) + F);
            (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2) * 4))).Value = x1;
            (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4))).Value = y1;
            goto _1;
_1:;
            ii = ii + 1;
        }
        Xsqlite3_result_blob(tls, context, p + 4, (int32)4 + (int32)8 * (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p)).Value.FnVertex, (uintptr)(-libc.Int32FromInt32(1)));
        Xsqlite3_free(tls, p);
    }
}

} // end sqlite3_package
