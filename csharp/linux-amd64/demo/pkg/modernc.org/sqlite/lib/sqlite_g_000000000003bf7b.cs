namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _rtreeStepToLeaf(ж<libc.TLS> tls, uintptr pCur) {
    int32 eInt = default;
    ref var eWithin = ref heap(new int32(), out var ᏑeWithin);
    int32 ii = default;
    int32 nCell = default;
    int32 nConstraint = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    uintptr p = default;
    uintptr pCellData = default;
    uintptr pConstraint = default;
    uintptr pNode = default;
    uintptr pRtree = default;
    uintptr v1 = default;
    ref var rScore = ref heap(new Tsqlite3_rtree_dbl(), out var ᏑrScore);
    TRtreeSearchPoint x = default;
    _ = eInt;
    _ = eWithin;
    _ = ii;
    _ = nCell;
    _ = nConstraint;
    _ = p;
    _ = pCellData;
    _ = pConstraint;
    _ = pNode;
    _ = pRtree;
    _ = rScore;
    _ = rc;
    _ = x;
    _ = v1;
    pRtree = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.Fbase.FpVtab;
    rc = SQLITE_OK;
    nConstraint = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FnConstraint;
    eInt = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == (int32)RTREE_COORD_INT32);
    for (;;) {
        v1 = _rtreeSearchPointFirst(ref (tls).DerefOrNull(), pCur);
        p = v1;
        if (!(v1 != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiLevel) > 0)) {
            break;
        }
        pNode = _rtreeNodeOfFirstSearchPoint(tls, pCur, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (rc != 0) {
            return rc;
        }
        nCell = _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
        if (nCell > (int32)RTREE_MAXCELLS) {
            return libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        }
        pCellData = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + (uintptr)(libc.Int32FromInt32(4) + libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell) * libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell));
        while (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell) < nCell) {
            rScore = (float64)(-libc.Int32FromInt32(1));
            eWithin = (int32)FULLY_WITHIN;
            ii = 0;
            for (;;) {
                if (!(ii < nConstraint)) {
                    break;
                }
                pConstraint = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FaConstraint + (uintptr)ii * 24;
                if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)pConstraint)).Value.Fop >= (int32)RTREE_MATCH) {
                    rc = _rtreeCallbackConstraint(tls, pConstraint, eInt, pCellData, p, @unsafe.Pointer.FromPinnedBox(ᏑrScore), @unsafe.Pointer.FromPinnedBox(ᏑeWithin));
                    if (rc != 0) {
                        return rc;
                    }
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiLevel) == (int32)1) {
                        _rtreeLeafConstraint(tls, pConstraint, eInt, pCellData, @unsafe.Pointer.FromPinnedBox(ᏑeWithin));
                    } else {
                        _rtreeNonleafConstraint(tls, pConstraint, eInt, pCellData, @unsafe.Pointer.FromPinnedBox(ᏑeWithin));
                    }
                }
                if (eWithin == NOT_WITHIN) {
                    (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell = (Tu8)((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell + 1);
                    pCellData = pCellData + (uintptr)(@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell;
                    break;
                }
                goto _2;
_2:;
                ii = ii + 1;
            }
            if (eWithin == NOT_WITHIN) {
                continue;
            }
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell = (Tu8)((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell + 1);
            x.FiLevel = libc.Uint8FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiLevel) - (int32)1);
            if (x.FiLevel != 0) {
                x.Fid = _readInt64(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCellData);
                ii = 0;
                for (;;) {
                    if (!(ii < (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FnPoint)) {
                        break;
                    }
                    if ((@unsafe.Pointer.To<TRtreeSearchPoint>(__ccgo_ptr((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FaPoint + (uintptr)ii * 24))).Value.Fid == x.Fid) {
                        return libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
                    }
                    goto _3;
_3:;
                    ii = ii + 1;
                }
                x.FiCell = (uint8)0;
            } else {
                x.Fid = (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.Fid;
                x.FiCell = libc.Uint8FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell) - (int32)1);
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell) >= nCell) {
                _rtreeSearchPointPop(tls, pCur);
            }
            if (rScore < (float64)0D) {
                rScore = (float64)0D;
            }
            p = _rtreeSearchPointNew(tls, pCur, rScore, x.FiLevel);
            if (p == (uintptr)0) {
                return (int32)SQLITE_NOMEM;
            }
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FeWithin = libc.Uint8FromInt32(eWithin);
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.Fid = x.Fid;
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell = x.FiCell;
            break;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell) >= nCell) {
            _rtreeSearchPointPop(tls, pCur);
        }
    }
    (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCur)).Value.FatEOF = libc.BoolUint8(p == (uintptr)0);
    return SQLITE_OK;
}

} // end sqlite3_package
