namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt SQLITE_BIGENDIAN => 0;

public static UntypedInt SQLITE_BYTEORDER => 1234;

public static UntypedInt SQLITE_LITTLEENDIAN => 1;

public static UntypedInt SQLITE_UTF16NATIVE => 2;

public static int32 /*r*/ Xsqlite3_complete16(ж<libc.TLS> tls, uintptr zSql) {
    uintptr pVal = default;
    uintptr zSql8 = default;
    int32 rc = default;
    _ = pVal;
    _ = rc;
    _ = zSql8;
    rc = Xsqlite3_initialize(tls);
    if (rc != 0) {
        return rc;
    }
    pVal = _sqlite3ValueNew(tls, (uintptr)0);
    _sqlite3ValueSetStr(tls, pVal, -(int32)1, zSql, (uint8)SQLITE_UTF16LE, libc.UintptrFromInt32(0));
    zSql8 = _sqlite3ValueText(tls, pVal, (uint8)SQLITE_UTF8);
    if (zSql8 != 0) {
        rc = libc.BoolInt32(Xsqlite3_incomplete(tls, zSql8) == 0);
    } else {
        rc = (int32)SQLITE_NOMEM;
    }
    _sqlite3ValueFree(tls, pVal);
    return rc & (int32)0xff;
}

public static int32 /*r*/ Xsqlite3_create_function16(ж<libc.TLS> tls, uintptr db, uintptr zFunctionName, int32 nArg, int32 eTextRep, uintptr p, uintptr __ccgo_fp_xSFunc, uintptr __ccgo_fp_xStep, uintptr __ccgo_fp_xFinal) {
    int32 rc = default;
    uintptr zFunc8 = default;
    _ = rc;
    _ = zFunc8;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    zFunc8 = _sqlite3Utf16to8(tls, db, zFunctionName, -(int32)1, (uint8)SQLITE_UTF16LE);
    rc = _sqlite3CreateFunc(tls, db, zFunc8, nArg, eTextRep, p, __ccgo_fp_xSFunc, __ccgo_fp_xStep, __ccgo_fp_xFinal, (uintptr)0, (uintptr)0, (uintptr)0);
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zFunc8);
    rc = _sqlite3ApiExit(tls, db, rc);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

public static void Xsqlite3_result_error16(ж<libc.TLS> tls, uintptr pCtx, uintptr z, int32 n) {
    (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FisError = (int32)SQLITE_ERROR;
    _sqlite3VdbeMemSetStr(tls, (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpOut, z, (int64)n, (uint8)SQLITE_UTF16LE, (uintptr)(-libc.Int32FromInt32(1)));
}

public static int32 /*r*/ Xsqlite3_value_bytes16(ж<libc.TLS> tls, uintptr pVal) {
    return _sqlite3ValueBytes(tls, pVal, (uint8)SQLITE_UTF16LE);
}

public static uintptr /*r*/ Xsqlite3_value_text16(ж<libc.TLS> tls, uintptr pVal) {
    return _sqlite3ValueText(tls, pVal, (uint8)SQLITE_UTF16LE);
}

internal static int32 /*r*/ _AdjustTree(ref libc.TLS tls, uintptr pRtree, uintptr pNode, @unsafe.Pointer pCell) {
    ref var cell = ref heap(new TRtreeCell(), out var Ꮡcell);
    int32 cnt = default;
    ref var iCell = ref heap(new int32(), out var ᏑiCell);
    int32 rc = default;
    uintptr p = default;
    uintptr pParent = default;
    _ = cell;
    _ = cnt;
    _ = iCell;
    _ = p;
    _ = pParent;
    _ = rc;
    p = pNode;
    cnt = 0;
    while ((@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)p)).Value.FpParent != 0) {
        pParent = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)p)).Value.FpParent;
        cnt = cnt + 1;
        if (cnt > (int32)100) {
            return libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        }
        rc = _nodeParentIndex(ref tls, pRtree, p, @unsafe.Pointer.FromPinnedBox(ᏑiCell));
        if (rc != SQLITE_OK) {
            return libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        }
        _nodeGetCell(ref tls, (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pParent, iCell, @unsafe.Pointer.FromPinnedBox(Ꮡcell));
        if (!(_cellContains(ref tls, pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell), pCell) != 0)) {
            _cellUnion(ref tls, pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell), pCell);
            _nodeOverwriteCell(ref tls, pRtree, pParent, @unsafe.Pointer.FromPinnedBox(Ꮡcell), iCell);
        }
        p = pParent;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _ChooseLeaf(ж<libc.TLS> tls, uintptr pRtree, @unsafe.Pointer pCell, int32 iHeight, @unsafe.Pointer ppLeaf) {
    TRtreeDValue area = default;
    TRtreeDValue area1 = default;
    TRtreeDValue fMinArea = default;
    TRtreeDValue fMinGrowth = default;
    TRtreeDValue growth = default;
    int32 bFound = default;
    int32 iCell = default;
    int32 ii = default;
    int32 nCell = default;
    int32 rc = default;
    ref var cell = ref heap(new TRtreeCell(), out var Ꮡcell);
    ref var cell1 = ref heap(new TRtreeCell(), out var Ꮡcell1);
    Tsqlite3_int64 iBest = default;
    ref var pChild = ref heap(new uintptr(), out var ᏑpChild);
    ref var pNode = ref heap(new uintptr(), out var ᏑpNode);
    _ = area;
    _ = area1;
    _ = bFound;
    _ = cell;
    _ = cell1;
    _ = fMinArea;
    _ = fMinGrowth;
    _ = growth;
    _ = iBest;
    _ = iCell;
    _ = ii;
    _ = nCell;
    _ = pChild;
    _ = pNode;
    _ = rc;
    pNode = (uintptr)0;
    rc = _nodeAcquire(tls, pRtree, (int64)1, (uintptr)0, @unsafe.Pointer.FromBox(ᏑpNode));
    ii = 0;
    for (;;) {
        if (!(rc == SQLITE_OK && ii < (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth - iHeight)) {
            break;
        }
        iBest = 0;
        bFound = 0;
        fMinGrowth = (float64)0D;
        fMinArea = (float64)0D;
        nCell = _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
        pChild = (uintptr)0;
        iCell = 0;
        for (;;) {
            if (!(iCell < nCell)) {
                break;
            }
            _nodeGetCell(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, iCell, @unsafe.Pointer.FromPinnedBox(Ꮡcell));
            if (_cellContains(ref (tls).DerefOrNull(), pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell), pCell) != 0) {
                area = _cellArea(ref (tls).DerefOrNull(), pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell));
                if (bFound == 0 || area < fMinArea) {
                    iBest = cell.FiRowid;
                    fMinArea = area;
                    bFound = (int32)1;
                }
            }
            goto _2;
_2:;
            iCell = iCell + 1;
        }
        if (!(bFound != 0)) {
            iCell = 0;
            for (;;) {
                if (!(iCell < nCell)) {
                    break;
                }
                _nodeGetCell(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, iCell, @unsafe.Pointer.FromPinnedBox(Ꮡcell1));
                area1 = _cellArea(ref (tls).DerefOrNull(), pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell1));
                _cellUnion(ref (tls).DerefOrNull(), pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell1), pCell);
                growth = _cellArea(ref (tls).DerefOrNull(), pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡcell1)) - area1;
                if (iCell == 0 || growth < fMinGrowth || growth == fMinGrowth && area1 < fMinArea) {
                    fMinGrowth = growth;
                    fMinArea = area1;
                    iBest = cell1.FiRowid;
                }
                goto _3;
_3:;
                iCell = iCell + 1;
            }
        }
        rc = _nodeAcquire(tls, pRtree, iBest, pNode, @unsafe.Pointer.FromBox(ᏑpChild));
        _nodeRelease(tls, pRtree, pNode);
        pNode = pChild;
        goto _1;
_1:;
        ii = ii + 1;
    }
    (@unsafe.Pointer.To<uintptr>(ppLeaf)).Value = pNode;
    return rc;
}

internal static UntypedInt __BYTE_ORDER__ => 1234;

internal static int32 /*r*/ _fixBoundingBox(ж<libc.TLS> tls, uintptr pRtree, uintptr pNode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var box = ref heap(new TRtreeCell(), out var Ꮡbox);
        ref var cell = ref heap(new TRtreeCell(), out var Ꮡcell);
        ref var ii = ref heap(new int32(), out var Ꮡii);
        int32 nCell = default;
        int32 rc = default;
        uintptr pParent = default;
        _ = box;
        _ = cell;
        _ = ii;
        _ = nCell;
        _ = pParent;
        _ = rc;
        pParent = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FpParent;
        rc = SQLITE_OK;
        if (pParent != 0) {
            nCell = _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
            _nodeGetCell(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, 0, @unsafe.Pointer.FromPinnedBox(Ꮡbox));
            ii = (int32)1;
            for (;;) {
                if (!(ii < nCell)) {
                    break;
                }
                _nodeGetCell(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, ii, @unsafe.Pointer.FromPinnedBox(Ꮡcell));
                _cellUnion(ref (tls).DerefOrNull(), pRtree, @unsafe.Pointer.FromPinnedBox(Ꮡbox), @unsafe.Pointer.FromPinnedBox(Ꮡcell));
                goto _1;
_1:;
                ii = ii + 1;
            }
            box.FiRowid = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode;
            rc = _nodeParentIndex(ref (tls).DerefOrNull(), pRtree, pNode, @unsafe.Pointer.FromPinnedBox(Ꮡii));
            if (rc == SQLITE_OK) {
                _nodeOverwriteCell(ref (tls).DerefOrNull(), pRtree, pParent, @unsafe.Pointer.FromPinnedBox(Ꮡbox), ii);
                rc = _fixBoundingBox(tls, pRtree, pParent);
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _freeCursorWithCache(ж<libc.TLS> tls, uintptr p, uintptr pCx) {
    uintptr pCache = default;
    _ = pCache;
    pCache = (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCx)).Value.FpCache;
    libc.SetBitFieldPtr8Uint32(pCx + 8, libc.Uint32FromInt32(0), 4, 0x10);
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCx)).Value.FpCache = (uintptr)0;
    if ((@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue != 0) {
        _sqlite3RCStrUnref(tls, (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue);
        (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue = (uintptr)0;
    }
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, pCache);
    _sqlite3VdbeFreeCursorNN(tls, p, pCx);
}

internal static int32 /*r*/ _nodeRowidIndex(ref libc.TLS tls, uintptr pRtree, uintptr pNode, Ti64 iRowid, @unsafe.Pointer piIndex) {
    int32 ii = default;
    int32 nCell = default;
    _ = ii;
    _ = nCell;
    nCell = _readInt16(ref tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
    ii = 0;
    for (;;) {
        if (!(ii < nCell)) {
            break;
        }
        if (_nodeGetRowid(ref tls, (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, ii) == iRowid) {
            (@unsafe.Pointer.To<int32>(piIndex)).Value = ii;
            return SQLITE_OK;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    return libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
}

internal static int32 /*r*/ _read32bits(ж<libc.TLS> tls, uintptr fd, Ti64 offset, @unsafe.Pointer pRes) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        array<uint8> _ᴛ1 = new(4);
        _ = rc;
        rc = _sqlite3OsRead(tls, fd, bp, (int32)4, offset);
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<Tu32>(pRes)).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _readCoord(ref libc.TLS tls, uintptr p, @unsafe.Pointer pCoord) {
    (@unsafe.Pointer.To<Tu32>(pCoord)).Value = ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p)))).Lsh((int64)(libc.Int32FromInt32(24))) + ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1)))).Lsh((int64)(libc.Int32FromInt32(16))) + ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 2)))).Lsh((int64)(libc.Int32FromInt32(8))) + ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 3)))).Lsh((int64)(libc.Int32FromInt32(0)));
}

internal static int32 /*r*/ _reinsertNodeContent(ж<libc.TLS> tls, uintptr pRtree, uintptr pNode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 ii = default;
        int32 nCell = default;
        int32 rc = default;
        int32 rc2 = default;
        ref var pInsert = ref heap(new uintptr(), out var ᏑpInsert);
        TRtreeCell _ᴛ1 = new();
        _ = ii;
        _ = nCell;
        _ = pInsert;
        _ = rc;
        _ = rc2;
        rc = SQLITE_OK;
        nCell = _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
        ii = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && ii < nCell)) {
                break;
            }
            _nodeGetCell(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, ii, __ccgo_ptr(bp));
            rc = _ChooseLeaf(tls, pRtree, __ccgo_ptr(bp), (int32)(@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode, @unsafe.Pointer.FromBox(ᏑpInsert));
            if (rc == SQLITE_OK) {
                rc = _rtreeInsertCell(tls, pRtree, pInsert, bp, (int32)(@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode);
                rc2 = _nodeRelease(tls, pRtree, pInsert);
                if (rc == SQLITE_OK) {
                    rc = rc2;
                }
            }
            goto _1;
_1:;
            ii = ii + 1;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _rtreeDeleteRowid(ж<libc.TLS> tls, uintptr pRtree, Tsqlite3_int64 iDelete) {
    ref var iCell = ref heap(new int32(), out var ᏑiCell);
    int32 rc = default;
    int32 rc2 = default;
    int32 rc21 = default;
    Ti64 iChild = default;
    ref var pChild = ref heap(new uintptr(), out var ᏑpChild);
    ref var pLeaf = ref heap(new uintptr(), out var ᏑpLeaf);
    ref var pRoot = ref heap(new uintptr(), out var ᏑpRoot);
    _ = iCell;
    _ = iChild;
    _ = pChild;
    _ = pLeaf;
    _ = pRoot;
    _ = rc;
    _ = rc2;
    _ = rc21;
    pLeaf = (uintptr)0;
    pRoot = (uintptr)0;
    rc = _nodeAcquire(tls, pRtree, (int64)1, (uintptr)0, @unsafe.Pointer.FromBox(ᏑpRoot));
    if (rc == SQLITE_OK) {
        rc = _findLeafNode(tls, pRtree, iDelete, @unsafe.Pointer.FromBox(ᏑpLeaf), nil);
    }
    if (rc == SQLITE_OK && pLeaf != 0) {
        rc = _nodeRowidIndex(ref (tls).DerefOrNull(), pRtree, pLeaf, iDelete, @unsafe.Pointer.FromPinnedBox(ᏑiCell));
        if (rc == SQLITE_OK) {
            rc = _deleteCell(tls, pRtree, pLeaf, iCell, 0);
        }
        rc2 = _nodeRelease(tls, pRtree, pLeaf);
        if (rc == SQLITE_OK) {
            rc = rc2;
        }
    }
    if (rc == SQLITE_OK) {
        Xsqlite3_bind_int64(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleteRowid, (int32)1, iDelete);
        Xsqlite3_step(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleteRowid);
        rc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleteRowid);
    }
    if (rc == SQLITE_OK && (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth > 0 && _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRoot)).Value.FzData + 2) == (int32)1) {
        pChild = (uintptr)0;
        iChild = _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pRoot, 0);
        rc = _nodeAcquire(tls, pRtree, iChild, pRoot, @unsafe.Pointer.FromBox(ᏑpChild));
        if (rc == SQLITE_OK) {
            rc = _removeNode(tls, pRtree, pChild, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth - (int32)1);
        }
        rc21 = _nodeRelease(tls, pRtree, pChild);
        if (rc == SQLITE_OK) {
            rc = rc21;
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth - 1;
            _writeInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRoot)).Value.FzData, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth);
            (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRoot)).Value.FisDirty = (int32)1;
        }
    }
    pLeaf = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleted;
    for (;;) {
        if (!(pLeaf != 0)) {
            break;
        }
        if (rc == SQLITE_OK) {
            rc = _reinsertNodeContent(tls, pRtree, pLeaf);
        }
        (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleted = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeaf)).Value.FpNext;
        (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnNodeRef = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnNodeRef - 1;
        Xsqlite3_free(tls, pLeaf);
        goto _1;
_1:;
        pLeaf = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpDeleted;
    }
    if (rc == SQLITE_OK) {
        rc = _nodeRelease(tls, pRtree, pRoot);
    } else {
        _nodeRelease(tls, pRtree, pRoot);
    }
    return rc;
}

internal static void _sqlite3BtreeGetMeta(ж<libc.TLS> tls, uintptr p, int32 idx, @unsafe.Pointer pMeta) {
    uintptr pBt = default;
    _ = pBt;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    if (idx == (int32)BTREE_DATA_VERSION) {
        (@unsafe.Pointer.To<Tu32>(pMeta)).Value = _sqlite3PagerDataVersion(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager) + (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FiBDataVersion;
    } else {
        (@unsafe.Pointer.To<Tu32>(pMeta)).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + (uintptr)((int32)36 + idx * (int32)4)));
    }
    _sqlite3BtreeLeave(tls, p);
}

internal static int32 /*r*/ _sqlite3BtreeIncrVacuum(ж<libc.TLS> tls, uintptr p) {
    TPgno nFin = default;
    TPgno nFree = default;
    TPgno nOrig = default;
    uintptr pBt = default;
    int32 rc = default;
    _ = nFin;
    _ = nFree;
    _ = nOrig;
    _ = pBt;
    _ = rc;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    if (!((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0)) {
        rc = (int32)SQLITE_DONE;
    } else {
        nOrig = _btreePagecount(ref (tls).DerefOrNull(), pBt);
        nFree = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 36));
        nFin = _finalDbSize(ref (tls).DerefOrNull(), pBt, nOrig, nFree);
        if (nOrig < nFin || nFree >= nOrig) {
            rc = _sqlite3CorruptError(tls, (int32)77355);
        } else {
            if (nFree > (uint32)0) {
                rc = _saveAllCursors(tls, pBt, (uint32)0, (uintptr)0);
                if (rc == SQLITE_OK) {
                    _invalidateAllOverflowCache(ref (tls).DerefOrNull(), pBt);
                    rc = _incrVacuumStep(tls, pBt, nFin, nOrig, 0);
                }
                if (rc == SQLITE_OK) {
                    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FpDbPage);
                    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 28), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage);
                }
            } else {
                rc = (int32)SQLITE_DONE;
            }
        }
    }
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeUpdateMeta(ж<libc.TLS> tls, uintptr p, int32 idx, Tu32 iMeta) {
    uintptr pBt = default;
    uintptr pP1 = default;
    int32 rc = default;
    _ = pBt;
    _ = pP1;
    _ = rc;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    pP1 = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData;
    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FpDbPage);
    if (rc == SQLITE_OK) {
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pP1 + (uintptr)((int32)36 + idx * (int32)4)), iMeta);
        if (idx == (int32)BTREE_INCR_VACUUM) {
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FincrVacuum = (uint8)iMeta;
        }
    }
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static Tu64 /*r*/ _sqlite3Get8byte(ref libc.TLS tls, @unsafe.Pointer p) {
    return ((uint64)(~@unsafe.Pointer.To<Tu8>(p))).Lsh((int64)(libc.Int32FromInt32(56))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 1)))).Lsh((int64)(libc.Int32FromInt32(48))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 2)))).Lsh((int64)(libc.Int32FromInt32(40))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 3)))).Lsh((int64)(libc.Int32FromInt32(32))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 4)))).Lsh((int64)(libc.Int32FromInt32(24))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 5)))).Lsh((int64)(libc.Int32FromInt32(16))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 6)))).Lsh((int64)(libc.Int32FromInt32(8))) + ((uint64)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 7)))).Lsh((int64)(libc.Int32FromInt32(0)));
}

internal static void _sqlite3Put4byte(ref libc.TLS tls, @unsafe.Pointer p, Tu32 v) {
    (@unsafe.Pointer.To<uint8>(p)).Value = (uint8)(v >> (int)(libc.Int32FromInt32(24)));
    (@unsafe.Pointer.To<uint8>(@unsafe.Add(p, 1))).Value = (uint8)(v >> (int)(libc.Int32FromInt32(16)));
    (@unsafe.Pointer.To<uint8>(@unsafe.Add(p, 2))).Value = (uint8)(v >> (int)(libc.Int32FromInt32(8)));
    (@unsafe.Pointer.To<uint8>(@unsafe.Add(p, 3))).Value = (uint8)v;
}

internal static void _unixepochFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TDateTime _ᴛ1 = default;
        if (_isDate(tls, context, argc, argv, bp) == 0) {
            _computeJD(tls, bp);
            if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(bp + 44)) & 0x4) >> 2) != 0) {
                Xsqlite3_result_double(tls, context, (float64)((@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD - libc.Int64FromInt32(21086676) * libc.Int64FromInt32(10000000)) / (float64)1000D);
            } else {
                Xsqlite3_result_int64(tls, context, (@unsafe.Pointer.To<TDateTime>(__ccgo_ptr(bp))).Value.FiJD / (int64)1000 - libc.Int64FromInt32(21086676) * libc.Int64FromInt32(10000));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _writeCoord(ref libc.TLS tls, uintptr p, @unsafe.Pointer pCoord) {
    Tu32 i = default;
    _ = i;
    i = ~@unsafe.Pointer.To<Tu32>(pCoord);
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))).Value = (uint8)((Tu32)(i >> (int)(libc.Int32FromInt32(24)) & (uint32)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1))).Value = (uint8)((Tu32)(i >> (int)(libc.Int32FromInt32(16)) & (uint32)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 2))).Value = (uint8)((Tu32)(i >> (int)(libc.Int32FromInt32(8)) & (uint32)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 3))).Value = (uint8)((Tu32)(i >> (int)(libc.Int32FromInt32(0)) & (uint32)0xFF));
    return (int32)4;
}

} // end sqlite3_package
