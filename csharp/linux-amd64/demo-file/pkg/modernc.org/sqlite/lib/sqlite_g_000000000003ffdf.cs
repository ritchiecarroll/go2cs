namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static TRtreeDValue /*r*/ _cellArea(ref libc.TLS tls, uintptr pRtree, @unsafe.Pointer p) {
    TRtreeDValue area = default;
    _ = area;
    area = libc.Float64FromInt32(1);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
        var exprᴛ1 = libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim);
        var matchᴛ1 = false;
        if (exprᴛ1 == (int32)5) { matchᴛ1 = true;
            area = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 44)) - ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 40)));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)4) { matchᴛ1 = true;
            area = (TRtreeDValue)(area * (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 36)) - ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 32))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)3) { matchᴛ1 = true;
            area = (TRtreeDValue)(area * (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 28)) - ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 24))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)2) { matchᴛ1 = true;
            area = (TRtreeDValue)(area * (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 20)) - ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 16))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            area = (TRtreeDValue)(area * (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 12)) - ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(p, 8))));
        }

    } else {
        var exprᴛ2 = libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim);
        var matchᴛ2 = false;
        if (exprᴛ2 == (int32)5) { matchᴛ2 = true;
            area = (float64)((int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 44))) - (int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 40))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)4) { matchᴛ2 = true;
            area = (TRtreeDValue)(area * (float64)((int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 36))) - (int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 32)))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)3) { matchᴛ2 = true;
            area = (TRtreeDValue)(area * (float64)((int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 28))) - (int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 24)))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && exprᴛ2 == (int32)2) { matchᴛ2 = true;
            area = (TRtreeDValue)(area * (float64)((int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 20))) - (int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 16)))));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2) { /* default: */
            area = (TRtreeDValue)(area * (float64)((int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 12))) - (int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(p, 8)))));
        }

    }
    return area;
}

internal static TRtreeDValue /*r*/ _cellOverlap(ref libc.TLS tls, uintptr pRtree, @unsafe.Pointer p, @unsafe.Pointer aCell, int32 nCell) {
    int32 ii = default;
    int32 jj = default;
    TRtreeDValue o = default;
    TRtreeDValue overlap = default;
    TRtreeDValue x1 = default;
    TRtreeDValue x2 = default;
    float64 v3 = default;
    float64 v4 = default;
    float64 v5 = default;
    float64 v6 = default;
    float64 v7 = default;
    _ = ii;
    _ = jj;
    _ = o;
    _ = overlap;
    _ = x1;
    _ = x2;
    _ = v3;
    _ = v4;
    _ = v5;
    _ = v6;
    _ = v7;
    overlap = (float64)0D;
    ii = 0;
    for (;;) {
        if (!(ii < nCell)) {
            break;
        }
        o = libc.Float64FromInt32(1);
        jj = 0;
        for (;;) {
            if (!(jj < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2))) {
                break;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v4 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)jj * 4)));
            } else {
                v4 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)jj * 4)));
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v5 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)jj * 4)));
            } else {
                v5 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)jj * 4)));
            }
            if (v4 > v5) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                    v6 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)jj * 4)));
                } else {
                    v6 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)jj * 4)));
                }
                v3 = v6;
            } else {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                    v7 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)jj * 4)));
                } else {
                    v7 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)jj * 4)));
                }
                v3 = v7;
            }
            x1 = v3;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v4 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)(jj + (int32)1) * 4)));
            } else {
                v4 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)(jj + (int32)1) * 4)));
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v5 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)(jj + (int32)1) * 4)));
            } else {
                v5 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)(jj + (int32)1) * 4)));
            }
            if (v4 < v5) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                    v6 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)(jj + (int32)1) * 4)));
                } else {
                    v6 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)(jj + (int32)1) * 4)));
                }
                v3 = v6;
            } else {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                    v7 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)(jj + (int32)1) * 4)));
                } else {
                    v7 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(@unsafe.Add(aCell, (uintptr)ii * 48), 8), (uintptr)(jj + (int32)1) * 4)));
                }
                v3 = v7;
            }
            x2 = v3;
            if (x2 < x1) {
                o = libc.Float64FromInt32(0);
                break;
            } else {
                o = (TRtreeDValue)(o * (x2 - x1));
            }
            goto _2;
_2:;
            jj = jj + (int32)2;
        }
        overlap = overlap + o;
        goto _1;
_1:;
        ii = ii + 1;
    }
    return overlap;
}

internal static int32 /*r1*/ _exprProbability(ж<libc.TLS> tls, uintptr p) {
    ref var r = ref heap(new float64(), out var Ꮡr);
    _ = r;
    r = -libc.Float64FromFloat64(1D);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) != (int32)TK_FLOAT) {
        return -(int32)1;
    }
    _sqlite3AtoF(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)), @unsafe.Pointer.FromPinnedBox(Ꮡr));
    if (r > (float64)1D) {
        return -(int32)1;
    }
    return (int32)(float64)(r * libc.Float64FromFloat64(1.34217728e+08D));
}

} // end sqlite3_package
