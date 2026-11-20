namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _SplitNode(ж<libc.TLS> tls, uintptr pRtree, uintptr pNode, uintptr pCell, int32 iHeight) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        uintptr aCell = default;
        uintptr aiUsed = default;
        uintptr pLeft = default;
        uintptr pParent = default;
        uintptr pRight = default;
        int32 i = default;
        ref var iCell = ref heap(new int32(), out var ᏑiCell);
        int32 nCell = default;
        int32 newCellIsRight = default;
        int32 rc = default;
        int32 v2 = default;
        int32 v3 = default;
        Ti64 iRowid = default;
        Ti64 iRowid1 = default;
        bool v4 = default;
        bool v5 = default;
        TRtreeCell _ᴛ1 = new();
        TRtreeCell _ᴛ2 = new();
        _ = aCell;
        _ = aiUsed;
        _ = i;
        _ = iCell;
        _ = iRowid;
        _ = iRowid1;
        _ = nCell;
        _ = newCellIsRight;
        _ = pLeft;
        _ = pParent;
        _ = pRight;
        _ = rc;
        _ = v2;
        _ = v3;
        _ = v4;
        _ = v5;
        newCellIsRight = 0;
        rc = SQLITE_OK;
        nCell = _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
        pLeft = (uintptr)0;
        pRight = (uintptr)0;
        aCell = Xsqlite3_malloc64(tls, (uint64)((libc.Uint64FromInt64(48) + libc.Uint64FromInt64(4)) * libc.Uint64FromInt32(nCell + libc.Int32FromInt32(1))));
        if (!(aCell != 0)) {
            rc = (int32)SQLITE_NOMEM;
            goto splitnode_out;
        }
        aiUsed = aCell + (uintptr)(nCell + (int32)1) * 48;
        libc.Xmemset(tls, aiUsed, 0, (uint64)4 * libc.Uint64FromInt32(nCell + libc.Int32FromInt32(1)));
        i = 0;
        for (;;) {
            if (!(i < nCell)) {
                break;
            }
            _nodeGetCell(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, i, (@unsafe.Pointer)(aCell + (uintptr)i * 48));
            goto _1;
_1:;
            i = i + 1;
        }
        _nodeZero(tls, pRtree, pNode);
        libc.Xmemcpy(tls, aCell + (uintptr)nCell * 48, pCell, (uint64)48);
        nCell = nCell + 1;
        if ((@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode == (int64)1) {
            pRight = _nodeNew(tls, pRtree, pNode);
            pLeft = _nodeNew(tls, pRtree, pNode);
            (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth + 1;
            (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FisDirty = (int32)1;
            _writeInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth);
        } else {
            pLeft = pNode;
            pRight = _nodeNew(tls, pRtree, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FpParent);
            (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FnRef = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FnRef + 1;
        }
        if (!(pLeft != 0) || !(pRight != 0)) {
            rc = (int32)SQLITE_NOMEM;
            goto splitnode_out;
        }
        libc.Xmemset(tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FzData, 0, libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize));
        libc.Xmemset(tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRight)).Value.FzData, 0, libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize));
        rc = _splitNodeStartree(tls, pRtree, aCell, nCell, pLeft, pRight, bp, bp + 48);
        if (rc != SQLITE_OK) {
            goto splitnode_out;
        }
        v2 = _nodeWrite(tls, pRtree, pRight);
        rc = v2;
        v5 = SQLITE_OK != v2; if (!v5) {
            v4 = 0 == (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FiNode; if (v4) {
                v3 = _nodeWrite(tls, pRtree, pLeft);
                rc = v3;
            }
        }
        if (v5 || v4 && SQLITE_OK != v3) {
            goto splitnode_out;
        }
        (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp + 48))).Value.FiRowid = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRight)).Value.FiNode;
        (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp))).Value.FiRowid = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FiNode;
        if ((@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode == (int64)1) {
            rc = _rtreeInsertCell(tls, pRtree, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FpParent, bp, iHeight + (int32)1);
            if (rc != SQLITE_OK) {
                goto splitnode_out;
            }
        } else {
            pParent = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FpParent;
            rc = _nodeParentIndex(ref (tls).DerefOrNull(), pRtree, pLeft, @unsafe.Pointer.FromPinnedBox(ᏑiCell));
            if (rc == SQLITE_OK) {
                _nodeOverwriteCell(ref (tls).DerefOrNull(), pRtree, pParent, __ccgo_ptr(bp), iCell);
                rc = _AdjustTree(ref (tls).DerefOrNull(), pRtree, pParent, __ccgo_ptr(bp));
            }
            if (rc != SQLITE_OK) {
                goto splitnode_out;
            }
        }
        v2 = _rtreeInsertCell(tls, pRtree, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRight)).Value.FpParent, bp + 48, iHeight + (int32)1);
        rc = v2;
        if (v2 != 0) {
            goto splitnode_out;
        }
        i = 0;
        for (;;) {
            if (!(i < _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pRight)).Value.FzData + 2))) {
                break;
            }
            iRowid = _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pRight, i);
            rc = _updateMapping(tls, pRtree, iRowid, pRight, iHeight);
            if (iRowid == (@unsafe.Pointer.To<TRtreeCell>((@unsafe.Pointer)pCell)).Value.FiRowid) {
                newCellIsRight = (int32)1;
            }
            if (rc != SQLITE_OK) {
                goto splitnode_out;
            }
            goto _7;
_7:;
            i = i + 1;
        }
        if ((@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FiNode == (int64)1) {
            i = 0;
            for (;;) {
                if (!(i < _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pLeft)).Value.FzData + 2))) {
                    break;
                }
                iRowid1 = _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pLeft, i);
                rc = _updateMapping(tls, pRtree, iRowid1, pLeft, iHeight);
                if (rc != SQLITE_OK) {
                    goto splitnode_out;
                }
                goto _8;
_8:;
                i = i + 1;
            }
        } else {
            if (newCellIsRight == 0) {
                rc = _updateMapping(tls, pRtree, (@unsafe.Pointer.To<TRtreeCell>((@unsafe.Pointer)pCell)).Value.FiRowid, pLeft, iHeight);
            }
        }
        goto splitnode_out;
splitnode_out:;
        _nodeRelease(tls, pRtree, pRight);
        _nodeRelease(tls, pRtree, pLeft);
        Xsqlite3_free(tls, aCell);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _accessPayload(ж<libc.TLS> tls, uintptr pCur, Tu32 offset, Tu32 amt, uintptr pBuf, int32 eOp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 a = default;
        int32 a1 = default;
        int32 iIdx = default;
        int32 rc = default;
        int32 v2 = default;
        uintptr aNew = default;
        uintptr aPayload = default;
        uintptr aWrite = default;
        uintptr fd = default;
        uintptr pBt = default;
        uintptr pBufStart = default;
        uintptr pPage = default;
        uintptr v1 = default;
        Ti64 nOvfl = default;
        ref var nextPage = ref heap(new TPgno(), out var ᏑnextPage);
        Tu32 ovflSize = default;
        array<Tu8> _ᴛ1 = new(4);
        uintptr _ᴛ2 = default;
        _ = a;
        _ = a1;
        _ = aNew;
        _ = aPayload;
        _ = aWrite;
        _ = fd;
        _ = iIdx;
        _ = nOvfl;
        _ = nextPage;
        _ = ovflSize;
        _ = pBt;
        _ = pBufStart;
        _ = pPage;
        _ = rc;
        _ = v1;
        _ = v2;
        rc = SQLITE_OK;
        iIdx = 0;
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        pBt = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt;
        pBufStart = pBuf;
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix) >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
            r = _sqlite3CorruptError(tls, (int32)78321); goto ᒐdone;
        }
        _getCellInfo(tls, pCur);
        aPayload = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FpPayload;
        if (libc.Uint64FromInt64((int64)aPayload - (int64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData) > (uint64)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal)) {
            r = _sqlite3CorruptError(tls, (int32)78336); goto ᒐdone;
        }
        if (offset < (uint32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal) {
            a = libc.Int32FromUint32(amt);
            if (libc.Uint32FromInt32(a) + offset > (uint32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal) {
                a = libc.Int32FromUint32((uint32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal - offset);
            }
            rc = _copyPayload(tls, aPayload + (uintptr)offset, pBuf, a, eOp, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
            offset = (uint32)0;
            pBuf = pBuf + (uintptr)a;
            amt = amt - libc.Uint32FromInt32(a);
        } else {
            offset = offset - (uint32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal;
        }
        if (rc == SQLITE_OK && amt > (uint32)0) {
            ovflSize = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)4;
            nextPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPayload + (uintptr)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal));
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_ValidOvfl) == 0) {
                nOvfl = libc.Int64FromUint32((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnPayload);
                nOvfl = quo((nOvfl - libc.Int64FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal) + libc.Int64FromUint32(ovflSize) - (int64)1), libc.Int64FromUint32(ovflSize));
                if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow == (uintptr)0 || nOvfl * (int64)libc.Int32FromInt64(4) > (int64)_sqlite3MallocSize(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow)) {
                    if (_sqlite3FaultSim(tls, (int32)413) != 0) {
                        aNew = (uintptr)0;
                    } else {
                        aNew = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow, libc.Uint64FromInt64(nOvfl * (int64)2) * (uint64)4);
                    }
                    if (aNew == (uintptr)0) {
                        r = (int32)SQLITE_NOMEM; goto ᒐdone;
                    } else {
                        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow = aNew;
                    }
                }
                libc.Xmemset(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow, 0, (uint64)(libc.Uint64FromInt64(nOvfl) * (uint64)4));
                v1 = pCur + 1;
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTCF_ValidOvfl));
            } else {
                if (~@unsafe.Pointer.To<TPgno>(__ccgo_ptr((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow + (uintptr)(offset / ovflSize) * 4)) != 0) {
                    iIdx = libc.Int32FromUint32(offset / ovflSize);
                    nextPage = ~@unsafe.Pointer.To<TPgno>(__ccgo_ptr((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow + (uintptr)iIdx * 4));
                    offset = offset % ovflSize;
                }
            }
            while (nextPage != 0) {
                if (nextPage > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage) {
                    r = _sqlite3CorruptError(tls, (int32)78409); goto ᒐdone;
                }
                (@unsafe.Pointer.To<TPgno>(__ccgo_ptr((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow + (uintptr)iIdx * 4))).Value = nextPage;
                if (offset >= ovflSize) {
                    if (~@unsafe.Pointer.To<TPgno>(__ccgo_ptr((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow + (uintptr)(iIdx + (int32)1) * 4)) != 0) {
                        nextPage = ~@unsafe.Pointer.To<TPgno>(__ccgo_ptr((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow + (uintptr)(iIdx + (int32)1) * 4));
                    } else {
                        rc = _getOverflowPage(tls, pBt, nextPage, nil, @unsafe.Pointer.FromPinnedBox(ᏑnextPage));
                    }
                    offset = offset - ovflSize;
                } else {
                    a1 = libc.Int32FromUint32(amt);
                    if (libc.Uint32FromInt32(a1) + offset > ovflSize) {
                        a1 = libc.Int32FromUint32(ovflSize - offset);
                    }
                    if (eOp == 0 && offset == (uint32)0 && _sqlite3PagerDirectReadOk(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, nextPage) != 0 && pBuf + (uintptr)(-libc.Int32FromInt32(4)) >= pBufStart) {
                        fd = _sqlite3PagerFile(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager);
                        aWrite = pBuf + (uintptr)(-libc.Int32FromInt32(4));
                        libc.Xmemcpy(tls, bp, aWrite, (uint64)4);
                        rc = _sqlite3OsRead(tls, fd, aWrite, a1 + (int32)4, libc.Int64FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize) * libc.Int64FromUint32(nextPage - libc.Uint32FromInt32(1)));
                        nextPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aWrite);
                        libc.Xmemcpy(tls, aWrite, bp, (uint64)4);
                    } else {
                        if (eOp == 0) {
                            v2 = (int32)PAGER_GET_READONLY;
                        } else {
                            v2 = 0;
                        }
                        rc = _sqlite3PagerGet(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, nextPage, bp + 8, v2);
                        if (rc == SQLITE_OK) {
                            if (eOp != 0 && (_sqlite3PagerPageRefcount(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))) != (int32)1 || (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)_sqlite3PagerGetExtra(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))))).Value.FisInit != 0) && _sqlite3FaultSim(tls, (int32)411) == SQLITE_OK) {
                                _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)));
                                r = _sqlite3CorruptError(tls, (int32)78481); goto ᒐdone;
                            }
                            aPayload = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)));
                            nextPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aPayload);
                            rc = _copyPayload(tls, aPayload + (uintptr)(offset + (uint32)4), pBuf, a1, eOp, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)));
                            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)));
                            offset = (uint32)0;
                        }
                    }
                    amt = amt - libc.Uint32FromInt32(a1);
                    if (amt == (uint32)0) {
                        r = rc; goto ᒐdone;
                    }
                    pBuf = pBuf + (uintptr)a1;
                }
                if (rc != 0) {
                    break;
                }
                iIdx = iIdx + 1;
            }
        }
        if (rc == SQLITE_OK && amt > (uint32)0) {
            r = _sqlite3CorruptError(tls, (int32)78501); goto ᒐdone;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _allocateBtreePage(ж<libc.TLS> tls, uintptr pBt, @unsafe.Pointer ppPage, @unsafe.Pointer pPgno, TPgno nearby, Tu8 eMode) {
    uintptr aData = default;
    ref var pNewTrunk = ref heap(new uintptr(), out var ᏑpNewTrunk);
    uintptr pPage1 = default;
    ref var pPg = ref heap(new uintptr(), out var ᏑpPg);
    uintptr pPrevTrunk = default;
    ref var pTrunk = ref heap(new uintptr(), out var ᏑpTrunk);
    int32 bNoContent = default;
    int32 d2 = default;
    int32 dist = default;
    int32 noContent = default;
    int32 rc = default;
    int32 v5 = default;
    Tu32 closest = default;
    Tu32 i = default;
    Tu32 k = default;
    Tu32 n = default;
    Tu32 nSearch = default;
    Tu32 v1 = default;
    ref var eType = ref heap(new Tu8(), out var ᏑeType);
    Tu8 searchList = default;
    TPgno iNewTrunk = default;
    TPgno iPage = default;
    TPgno iTrunk = default;
    TPgno mxPage = default;
    bool v2 = default;
    _ = aData;
    _ = bNoContent;
    _ = closest;
    _ = d2;
    _ = dist;
    _ = eType;
    _ = i;
    _ = iNewTrunk;
    _ = iPage;
    _ = iTrunk;
    _ = k;
    _ = mxPage;
    _ = n;
    _ = nSearch;
    _ = noContent;
    _ = pNewTrunk;
    _ = pPage1;
    _ = pPg;
    _ = pPrevTrunk;
    _ = pTrunk;
    _ = rc;
    _ = searchList;
    _ = v1;
    _ = v2;
    _ = v5;
    pTrunk = (uintptr)0;
    pPrevTrunk = (uintptr)0;
    pPage1 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1;
    mxPage = _btreePagecount(ref (tls).DerefOrNull(), pBt);
    n = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 36));
    if (n >= mxPage) {
        return _sqlite3CorruptError(tls, (int32)79716);
    }
    if (n > (uint32)0) {
        searchList = (uint8)0;
        nSearch = (uint32)0;
        if (libc.Int32FromUint8(eMode) == (int32)BTALLOC_EXACT) {
            if (nearby <= mxPage) {
                rc = _ptrmapGet(tls, pBt, nearby, @unsafe.Pointer.FromPinnedBox(ᏑeType), nil);
                if (rc != 0) {
                    return rc;
                }
                if (libc.Int32FromUint8(eType) == (int32)PTRMAP_FREEPAGE) {
                    searchList = (uint8)1;
                }
            }
        } else {
            if (libc.Int32FromUint8(eMode) == (int32)BTALLOC_LE) {
                searchList = (uint8)1;
            }
        }
        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FpDbPage);
        if (rc != 0) {
            return rc;
        }
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 36), n - (uint32)1);
        for (var cond = true; cond; cond = searchList != 0) {
            pPrevTrunk = pTrunk;
            if (pPrevTrunk != 0) {
                iTrunk = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPrevTrunk)).Value.FaData);
            } else {
                iTrunk = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 32));
            }
            v2 = iTrunk > mxPage; if (!v2) {
                v1 = nSearch;
                nSearch = nSearch + 1;
            }
            if (v2 || v1 > n) {
                rc = _sqlite3CorruptError(tls, (int32)79772);
            } else {
                rc = _btreeGetUnusedPage(tls, pBt, iTrunk, @unsafe.Pointer.FromBox(ᏑpTrunk), 0);
            }
            if (rc != 0) {
                pTrunk = (uintptr)0;
                goto end_allocate_page;
            }
            k = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData + 4));
            if (k == (uint32)0 && !(searchList != 0)) {
                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FpDbPage);
                if (rc != 0) {
                    goto end_allocate_page;
                }
                (@unsafe.Pointer.To<TPgno>(pPgno)).Value = iTrunk;
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 32, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData, (uint64)4);
                (@unsafe.Pointer.To<uintptr>(ppPage)).Value = pTrunk;
                pTrunk = (uintptr)0;
            } else {
                if (k > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize / libc.Uint32FromInt32(4) - libc.Uint32FromInt32(2)) {
                    rc = _sqlite3CorruptError(tls, (int32)79801);
                    goto end_allocate_page;
                } else {
                    if (searchList != 0 && (nearby == iTrunk || iTrunk < nearby && libc.Int32FromUint8(eMode) == (int32)BTALLOC_LE)) {
                        (@unsafe.Pointer.To<TPgno>(pPgno)).Value = iTrunk;
                        (@unsafe.Pointer.To<uintptr>(ppPage)).Value = pTrunk;
                        searchList = (uint8)0;
                        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FpDbPage);
                        if (rc != 0) {
                            goto end_allocate_page;
                        }
                        if (k == (uint32)0) {
                            if (!(pPrevTrunk != 0)) {
                                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 32, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData, (uint64)4);
                            } else {
                                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPrevTrunk)).Value.FpDbPage);
                                if (rc != SQLITE_OK) {
                                    goto end_allocate_page;
                                }
                                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPrevTrunk)).Value.FaData, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData, (uint64)4);
                            }
                        } else {
                            iNewTrunk = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData + 8));
                            if (iNewTrunk > mxPage) {
                                rc = _sqlite3CorruptError(tls, (int32)79835);
                                goto end_allocate_page;
                            }
                            rc = _btreeGetUnusedPage(tls, pBt, iNewTrunk, @unsafe.Pointer.FromBox(ᏑpNewTrunk), 0);
                            if (rc != SQLITE_OK) {
                                goto end_allocate_page;
                            }
                            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNewTrunk)).Value.FpDbPage);
                            if (rc != SQLITE_OK) {
                                _releasePage(tls, pNewTrunk);
                                goto end_allocate_page;
                            }
                            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNewTrunk)).Value.FaData, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData, (uint64)4);
                            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNewTrunk)).Value.FaData + 4), k - (uint32)1);
                            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNewTrunk)).Value.FaData + 8, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData + 12, (uint64)((k - (uint32)1) * (uint32)4));
                            _releasePage(tls, pNewTrunk);
                            if (!(pPrevTrunk != 0)) {
                                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 32), iNewTrunk);
                            } else {
                                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPrevTrunk)).Value.FpDbPage);
                                if (rc != 0) {
                                    goto end_allocate_page;
                                }
                                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPrevTrunk)).Value.FaData, iNewTrunk);
                            }
                        }
                        pTrunk = (uintptr)0;
                    } else {
                        if (k > (uint32)0) {
                            aData = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData;
                            if (nearby > (uint32)0) {
                                closest = (uint32)0;
                                if (libc.Int32FromUint8(eMode) == (int32)BTALLOC_LE) {
                                    i = (uint32)0;
                                    for (;;) {
                                        if (!(i < k)) {
                                            break;
                                        }
                                        iPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + (uintptr)((uint32)8 + i * (uint32)4)));
                                        if (iPage <= nearby) {
                                            closest = i;
                                            break;
                                        }
                                        goto _3;
_3:;
                                        i = i + 1;
                                    }
                                } else {
                                    dist = _sqlite3AbsInt32(ref (tls).DerefOrNull(), libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + 8)) - nearby));
                                    i = (uint32)1;
                                    for (;;) {
                                        if (!(i < k)) {
                                            break;
                                        }
                                        d2 = _sqlite3AbsInt32(ref (tls).DerefOrNull(), libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + (uintptr)((uint32)8 + i * (uint32)4))) - nearby));
                                        if (d2 < dist) {
                                            closest = i;
                                            dist = d2;
                                        }
                                        goto _4;
_4:;
                                        i = i + 1;
                                    }
                                }
                            } else {
                                closest = (uint32)0;
                            }
                            iPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + (uintptr)((uint32)8 + closest * (uint32)4)));
                            if (iPage > mxPage || iPage < (uint32)2) {
                                rc = _sqlite3CorruptError(tls, (int32)79900);
                                goto end_allocate_page;
                            }
                            if (!(searchList != 0) || (iPage == nearby || iPage < nearby && libc.Int32FromUint8(eMode) == (int32)BTALLOC_LE)) {
                                (@unsafe.Pointer.To<TPgno>(pPgno)).Value = iPage;
                                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FpDbPage);
                                if (rc != 0) {
                                    goto end_allocate_page;
                                }
                                if (closest < k - (uint32)1) {
                                    libc.Xmemcpy(tls, aData + (uintptr)((uint32)8 + closest * (uint32)4), aData + (uintptr)((uint32)4 + k * (uint32)4), (uint64)4);
                                }
                                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + 4), k - (uint32)1);
                                if (!(_btreeGetHasContent(ref (tls).DerefOrNull(), pBt, ~@unsafe.Pointer.To<TPgno>(pPgno)) != 0)) {
                                    v5 = (int32)PAGER_GET_NOCONTENT;
                                } else {
                                    v5 = 0;
                                }
                                noContent = v5;
                                rc = _btreeGetUnusedPage(tls, pBt, ~@unsafe.Pointer.To<TPgno>(pPgno), ppPage, noContent);
                                if (rc == SQLITE_OK) {
                                    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(ppPage)))).Value.FpDbPage);
                                    if (rc != SQLITE_OK) {
                                        _releasePage(tls, ~@unsafe.Pointer.To<uintptr>(ppPage));
                                        (@unsafe.Pointer.To<uintptr>(ppPage)).Value = (uintptr)0;
                                    }
                                }
                                searchList = (uint8)0;
                            }
                        }
                    }
                }
            }
            _releasePage(tls, pPrevTrunk);
            pPrevTrunk = (uintptr)0;
        }
    } else {
        if (0 == libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbDoTruncate)) {
            v5 = (int32)PAGER_GET_NOCONTENT;
        } else {
            v5 = 0;
        }
        bNoContent = v5;
        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FpDbPage);
        if (rc != 0) {
            return rc;
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage + 1;
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage + 1;
        }
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0 && _ptrmapPageno(ref (tls).DerefOrNull(), pBt, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage) == (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage) {
            pPg = (uintptr)0;
            rc = _btreeGetUnusedPage(tls, pBt, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage, @unsafe.Pointer.FromBox(ᏑpPg), bNoContent);
            if (rc == SQLITE_OK) {
                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FpDbPage);
                _releasePage(tls, pPg);
            }
            if (rc != 0) {
                return rc;
            }
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage + 1;
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
                (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage + 1;
            }
        }
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(libc.UintptrFromInt32(28) + (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage);
        (@unsafe.Pointer.To<TPgno>(pPgno)).Value = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage;
        rc = _btreeGetUnusedPage(tls, pBt, ~@unsafe.Pointer.To<TPgno>(pPgno), ppPage, bNoContent);
        if (rc != 0) {
            return rc;
        }
        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(ppPage)))).Value.FpDbPage);
        if (rc != SQLITE_OK) {
            _releasePage(tls, ~@unsafe.Pointer.To<uintptr>(ppPage));
            (@unsafe.Pointer.To<uintptr>(ppPage)).Value = (uintptr)0;
        }
    }
    goto end_allocate_page;
end_allocate_page:;
    _releasePage(tls, pTrunk);
    _releasePage(tls, pPrevTrunk);
    return rc;
}

internal static int32 /*r*/ _backupOnePage(ж<libc.TLS> tls, uintptr p, TPgno iSrcPg, uintptr zSrcData, int32 bUpdate) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        TPgno iDest = default;
        Ti64 iEnd = default;
        Ti64 iOff = default;
        int32 nCopy = default;
        int32 nDestPgsz = default;
        int32 nSrcPgsz = default;
        int32 rc = default;
        int32 v1 = default;
        int32 v3 = default;
        uintptr pDestPager = default;
        uintptr zDestData = default;
        uintptr zIn = default;
        uintptr zOut = default;
        bool v5 = default;
        uintptr _ᴛ1 = default;
        _ = iDest;
        _ = iEnd;
        _ = iOff;
        _ = nCopy;
        _ = nDestPgsz;
        _ = nSrcPgsz;
        _ = pDestPager;
        _ = rc;
        _ = zDestData;
        _ = zIn;
        _ = zOut;
        _ = v1;
        _ = v3;
        _ = v5;
        pDestPager = _sqlite3BtreePager(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDest);
        nSrcPgsz = _sqlite3BtreeGetPageSize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc);
        nDestPgsz = _sqlite3BtreeGetPageSize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDest);
        if (nSrcPgsz < nDestPgsz) {
            v1 = nSrcPgsz;
        } else {
            v1 = nDestPgsz;
        }
        nCopy = v1;
        iEnd = libc.Int64FromUint32(iSrcPg) * (int64)nSrcPgsz;
        rc = SQLITE_OK;
        iOff = iEnd - (int64)nSrcPgsz;
        for (;;) {
            if (!(rc == SQLITE_OK && iOff < iEnd)) {
                break;
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
            iDest = libc.Uint32FromInt64(quo(iOff, (int64)nDestPgsz)) + (uint32)1;
            if (iDest == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpDest)).Value.FpBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
                goto _2;
            }
            v1 = _sqlite3PagerGet(tls, pDestPager, iDest, bp, 0);
            rc = v1;
            v5 = SQLITE_OK == v1; if (v5) {
                v3 = _sqlite3PagerWrite(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                rc = v3;
            }
            if (v5 && SQLITE_OK == v3) {
                zIn = zSrcData + (uintptr)(rem(iOff, (int64)nSrcPgsz));
                zDestData = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                zOut = zDestData + (uintptr)(rem(iOff, (int64)nDestPgsz));
                libc.Xmemcpy(tls, zOut, zIn, libc.Uint64FromInt32(nCopy));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(_sqlite3PagerGetExtra(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)))))).Value = (uint8)0;
                if (iOff == 0 && bUpdate == 0) {
                    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(zOut + 28), _sqlite3BtreeLastPage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FpSrc));
                }
            }
            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            goto _2;
_2:;
            iOff = iOff + (int64)nDestPgsz;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _balance_deeper(ж<libc.TLS> tls, uintptr pRoot, uintptr ppChild) {
    uintptr pBt = default;
    ref var pChild = ref heap(new uintptr(), out var ᏑpChild);
    ref var pgnoChild = ref heap(new TPgno(), out var ᏑpgnoChild);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = pBt;
    _ = pChild;
    _ = pgnoChild;
    _ = rc;
    pChild = (uintptr)0;
    pgnoChild = (uint32)0;
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FpBt;
    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FpDbPage);
    if (rc == SQLITE_OK) {
        rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpChild), @unsafe.Pointer.FromPinnedBox(ᏑpgnoChild), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.Fpgno, (uint8)0);
        _copyNodeContent(tls, pRoot, pChild, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
            _ptrmapPut(tls, pBt, pgnoChild, (uint8)PTRMAP_BTREE, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        }
    }
    if (rc != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppChild))).Value = (uintptr)0;
        _releasePage(tls, pChild);
        return rc;
    }
    libc.Xmemcpy(tls, pChild + 28, pRoot + 28, (uint64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FnOverflow * (uint64)2);
    libc.Xmemcpy(tls, pChild + 40, pRoot + 40, (uint64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FnOverflow * (uint64)8);
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pChild)).Value.FnOverflow = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FnOverflow;
    _zeroPage(tls, pRoot, libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pChild)).Value.FaData))) & ~libc.Int32FromInt32(PTF_LEAF));
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FhdrOffset) + (int32)8)), pgnoChild);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppChild))).Value = pChild;
    return SQLITE_OK;
}

internal static int32 /*r1*/ _balance_nonroot(ж<libc.TLS> tls, uintptr pParent, int32 iParentIdx, uintptr aOvflSpace, int32 isRoot, int32 bBulk) {
    int32 r1 = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(192);
        defer(tls.Free, (nint)192, ref ᒐ);
        uintptr aData = default;
        uintptr aSpace1 = default;
        uintptr p = default;
        uintptr pBt = default;
        uintptr pCell = default;
        uintptr pCell1 = default;
        ref var pNew = ref heap(new uintptr(), out var ᏑpNew);
        uintptr pNew1 = default;
        uintptr pNew2 = default;
        uintptr pOld = default;
        uintptr pOld1 = default;
        uintptr pOld2 = default;
        uintptr pRight = default;
        uintptr pSrcEnd = default;
        uintptr pTemp = default;
        uintptr pTemp1 = default;
        uintptr piCell = default;
        uintptr piEnd = default;
        uintptr v17 = default;
        array<TPgno> aPgno = new(5);
        array<uintptr> apDiv = new(2);
        array<uintptr> apNew = new(5);
        array<int32> cntNew = new(5);
        array<int32> cntOld = new(5);
        int32 cntOldNext = default;
        int32 d = default;
        int32 i = default;
        int32 iB = default;
        int32 iNew = default;
        int32 iNew1 = default;
        int32 iOff = default;
        int32 iOld = default;
        int32 iOld1 = default;
        int32 iOvflSpace = default;
        int32 iPg = default;
        int32 iSpace1 = default;
        int32 j = default;
        int32 k = default;
        int32 leafData = default;
        int32 limit = default;
        int32 nMaxCells = default;
        int32 nNew = default;
        int32 nNewCell = default;
        int32 nOld = default;
        int32 nxDiv = default;
        int32 pageFlags = default;
        int32 r = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        int32 sz1 = default;
        int32 sz2 = default;
        int32 szD = default;
        int32 szLeft = default;
        int32 szR = default;
        int32 szRight = default;
        int32 usableSpace = default;
        int32 v1 = default;
        Tu16 fgA = default;
        Tu16 fgB = default;
        Tu16 leafCorrection = default;
        Tu16 maskPage = default;
        Tu16 sz = default;
        Tu32 key = default;
        ref var pgno = ref heap(new TPgno(), out var Ꮡpgno);
        TPgno pgnoA = default;
        TPgno pgnoB = default;
        TPgno pgnoTemp = default;
        Tu64 szScratch = default;
        bool v13 = default;
        bool v14 = default;
        uint32 v18 = default;
        array<Tu8> _ᴛ1 = new(5);
        array<uintptr> _ᴛ2 = new(3);
        TCellArray _ᴛ3 = new();
        TCellInfo _ᴛ4 = default;
        array<int32> _ᴛ5 = new(5);
        _ = aData;
        _ = aPgno;
        _ = aSpace1;
        _ = apDiv;
        _ = apNew;
        _ = cntNew;
        _ = cntOld;
        _ = cntOldNext;
        _ = d;
        _ = fgA;
        _ = fgB;
        _ = i;
        _ = iB;
        _ = iNew;
        _ = iNew1;
        _ = iOff;
        _ = iOld;
        _ = iOld1;
        _ = iOvflSpace;
        _ = iPg;
        _ = iSpace1;
        _ = j;
        _ = k;
        _ = key;
        _ = leafCorrection;
        _ = leafData;
        _ = limit;
        _ = maskPage;
        _ = nMaxCells;
        _ = nNew;
        _ = nNewCell;
        _ = nOld;
        _ = nxDiv;
        _ = p;
        _ = pBt;
        _ = pCell;
        _ = pCell1;
        _ = pNew;
        _ = pNew1;
        _ = pNew2;
        _ = pOld;
        _ = pOld1;
        _ = pOld2;
        _ = pRight;
        _ = pSrcEnd;
        _ = pTemp;
        _ = pTemp1;
        _ = pageFlags;
        _ = pgno;
        _ = pgnoA;
        _ = pgnoB;
        _ = pgnoTemp;
        _ = piCell;
        _ = piEnd;
        _ = r;
        _ = rc;
        _ = sz;
        _ = sz1;
        _ = sz2;
        _ = szD;
        _ = szLeft;
        _ = szR;
        _ = szRight;
        _ = szScratch;
        _ = usableSpace;
        _ = v1;
        _ = v13;
        _ = v14;
        _ = v17;
        _ = v18;
        nMaxCells = 0;
        nNew = 0;
        rc = SQLITE_OK;
        iSpace1 = 0;
        iOvflSpace = 0;
        libc.Xmemset(tls, bp + 45, 0, (uint64)5);
        libc.Xmemset(tls, bp + 56, 0, libc.Uint64FromInt64(104) - libc.Uint64FromInt64(4));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 56 + 80 + (uintptr)(libc.Int32FromInt32(NB) * libc.Int32FromInt32(2) - libc.Int32FromInt32(1)) * 4))).Value = (int32)0x7fffffff;
        pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FpBt;
        if (!(aOvflSpace != 0)) {
            r1 = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        i = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow) + libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnCell);
        if (i < (int32)2) {
            nxDiv = 0;
        } else {
            if (iParentIdx == 0) {
                nxDiv = 0;
            } else {
                if (iParentIdx == i) {
                    nxDiv = i - (int32)2 + bBulk;
                } else {
                    nxDiv = iParentIdx - (int32)1;
                }
            }
            i = (int32)2 - bBulk;
        }
        nOld = i + (int32)1;
        if (i + nxDiv - libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow) == libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnCell)) {
            pRight = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FhdrOffset) + (int32)8);
        } else {
            pRight = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaCellIdx + (uintptr)((int32)2 * (i + nxDiv - libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow)))))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaCellIdx + (uintptr)((int32)2 * (i + nxDiv - libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow))) + 1)))));
        }
        pgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRight);
        while ((int32)1 != 0) {
            if (rc == SQLITE_OK) {
                rc = _getAndInitPage(tls, pBt, pgno, (@unsafe.Pointer)(bp + (uintptr)i * 8), 0);
            }
            if (rc != 0) {
                libc.Xmemset(tls, bp, 0, libc.Uint64FromInt32(i + libc.Int32FromInt32(1)) * (uint64)8);
                goto balance_cleanup;
            }
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3))).Value.FnFree < 0) {
                rc = _btreeComputeFreeSpace(tls, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3));
                if (rc != 0) {
                    libc.Xmemset(tls, bp, 0, libc.Uint64FromInt32(i) * (uint64)8);
                    goto balance_cleanup;
                }
            }
            nMaxCells = nMaxCells + (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3))).Value.FnCell) + libc.Int32FromUint64(libc.Uint64FromInt64(32) / libc.Uint64FromInt64(8)));
            v1 = i;
            i = i - 1;
            if (v1 == 0) {
                break;
            }
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow != 0 && i + nxDiv == libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pParent + 28)))) {
                apDiv[i] = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pParent + 40));
                pgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)apDiv[i]);
                @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) = libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FxCellSize)(tls, pParent, apDiv[i]));
                (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow = (uint8)0;
            } else {
                apDiv[i] = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaCellIdx + (uintptr)((int32)2 * (i + nxDiv - libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow)))))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaCellIdx + (uintptr)((int32)2 * (i + nxDiv - libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow))) + 1)))));
                pgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)apDiv[i]);
                @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) = libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FxCellSize)(tls, pParent, apDiv[i]));
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_FAST_SECURE) != 0) {
                    iOff = (int32)(int64)apDiv[i] - (int32)(int64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaData;
                    if (iOff + @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) <= libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize)) {
                        libc.Xmemcpy(tls, aOvflSpace + (uintptr)iOff, apDiv[i], libc.Uint64FromInt32(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5)));
                        apDiv[i] = aOvflSpace + (uintptr)((int64)apDiv[i] - (int64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaData);
                    }
                }
                _dropCell(tls, pParent, i + nxDiv - libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnOverflow), @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            }
        }
        nMaxCells = (nMaxCells + (int32)3) & ~libc.Int32FromInt32(3);
        szScratch = (uint64)(libc.Uint64FromInt32(nMaxCells) * (uint64)8 + libc.Uint64FromInt32(nMaxCells) * (uint64)2 + (uint64)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize);
        (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell = _sqlite3DbMallocRaw(tls, (uintptr)0, szScratch);
        if ((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
            goto balance_cleanup;
        }
        (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)nMaxCells * 8;
        aSpace1 = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)nMaxCells * 2;
        (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FpRef = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), 0, 3);
        leafCorrection = libc.Uint16FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FpRef)).Value.Fleaf) * (int32)4);
        leafData = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FpRef)).Value.FintKeyLeaf);
        i = 0;
        for (;;) {
            if (!(i < nOld)) {
                break;
            }
            pOld = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3);
            limit = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FnCell);
            aData = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FaData;
            maskPage = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FmaskPage;
            piCell = aData + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FcellOffset;
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FaData))) != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), 0, 3))).Value.FaData)))) {
                rc = _sqlite3CorruptError(tls, (int32)81619);
                goto balance_cleanup;
            }
            libc.Xmemset(tls, (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2, 0, (uint64)2 * libc.Uint64FromInt32(limit + libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FnOverflow)));
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FnOverflow) > 0) {
                if (limit < libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pOld + 28)))) {
                    rc = _sqlite3CorruptError(tls, (int32)81643);
                    goto balance_cleanup;
                }
                limit = libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pOld + 28)));
                j = 0;
                for (;;) {
                    if (!(j < limit)) {
                        break;
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 8))).Value = aData + (uintptr)(libc.Int32FromUint16(maskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(piCell))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(piCell + 1)))));
                    piCell = piCell + (uintptr)2;
                    (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell + 1;
                    goto _3;
_3:;
                    j = j + 1;
                }
                k = 0;
                for (;;) {
                    if (!(k < libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FnOverflow))) {
                        break;
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pOld + 40 + (uintptr)k * 8))).Value;
                    (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell + 1;
                    goto _4;
_4:;
                    k = k + 1;
                }
            }
            piEnd = aData + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FcellOffset + (uintptr)((int32)2 * libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FnCell));
            while (piCell < piEnd) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 8))).Value = aData + (uintptr)(libc.Int32FromUint16(maskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(piCell))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(piCell + 1)))));
                piCell = piCell + (uintptr)2;
                (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell + 1;
            }
            cntOld[i] = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell;
            if (i < nOld - (int32)1 && !(leafData != 0)) {
                sz = libc.Uint16FromInt32(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5));
                (@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2))).Value = sz;
                pTemp = aSpace1 + (uintptr)iSpace1;
                iSpace1 = iSpace1 + libc.Int32FromUint16(sz);
                libc.Xmemcpy(tls, pTemp, apDiv[i], (uint64)sz);
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 8))).Value = pTemp + (uintptr)leafCorrection;
                (@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2))).Value = libc.Uint16FromInt32(libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2))) - libc.Int32FromUint16(leafCorrection));
                if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.Fleaf != 0)) {
                    libc.Xmemcpy(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 8)), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld)).Value.FaData + 8, (uint64)4);
                } else {
                    while (libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2))) < (int32)4) {
                        v1 = iSpace1;
                        iSpace1 = iSpace1 + 1;
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aSpace1 + (uintptr)v1))).Value = (uint8)0x00;
                        (@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2))).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)(@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell * 2)) + 1);
                    }
                }
                (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell + 1;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        usableSpace = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)12 + (uint32)leafCorrection);
        v1 = libc.Int32FromInt32(0);
        k = v1;
        i = v1;
        for (;;) {
            if (!(i < nOld)) {
                break;
            }
            p = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 56 + 32 + (uintptr)k * 8))).Value = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)p)).Value.FaDataEnd;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 56 + 80 + (uintptr)k * 4))).Value = cntOld[i];
            if (k != 0 && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 56 + 80 + (uintptr)k * 4)) == ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 56 + 80 + (uintptr)(k - (int32)1) * 4))) {
                k = k - 1;
            }
            if (!(leafData != 0)) {
                k = k + 1;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 56 + 32 + (uintptr)k * 8))).Value = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaDataEnd;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 56 + 80 + (uintptr)k * 4))).Value = cntOld[i] + (int32)1;
            }
            @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) = usableSpace - (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)p)).Value.FnFree;
            j = 0;
            for (;;) {
                if (!(j < libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)p)).Value.FnOverflow))) {
                    break;
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 24 + (uintptr)i * 4))).Value += (int32)2 + libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)p)).Value.FxCellSize)(tls, p, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 40 + (uintptr)j * 8))));
                goto _8;
_8:;
                j = j + 1;
            }
            cntNew[i] = cntOld[i];
            goto _6;
_6:;
            i = i + 1;
            k = k + 1;
        }
        k = nOld;
        i = 0;
        for (;;) {
            if (!(i < k)) {
                break;
            }
            while (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) > usableSpace) {
                if (i + (int32)1 >= k) {
                    k = i + (int32)2;
                    if (k > libc.Int32FromInt32(NB) + libc.Int32FromInt32(2)) {
                        rc = _sqlite3CorruptError(tls, (int32)81744);
                        goto balance_cleanup;
                    }
                    @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), k - (int32)1, 5) = 0;
                    cntNew[k - (int32)1] = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell;
                }
                sz1 = (int32)2 + libc.Int32FromUint16(_cachedCellSize(tls, __ccgo_ptr(bp + 56), cntNew[i] - (int32)1));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 24 + (uintptr)i * 4))).Value -= sz1;
                if (!(leafData != 0)) {
                    if (cntNew[i] < (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell) {
                        sz1 = (int32)2 + libc.Int32FromUint16(_cachedCellSize(tls, __ccgo_ptr(bp + 56), cntNew[i]));
                    } else {
                        sz1 = 0;
                    }
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 24 + (uintptr)(i + (int32)1) * 4))).Value += sz1;
                cntNew[i] = cntNew[i] - 1;
            }
            while (cntNew[i] < (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell) {
                sz1 = (int32)2 + libc.Int32FromUint16(_cachedCellSize(tls, __ccgo_ptr(bp + 56), cntNew[i]));
                if (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) + sz1 > usableSpace) {
                    break;
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 24 + (uintptr)i * 4))).Value += sz1;
                cntNew[i] = cntNew[i] + 1;
                if (!(leafData != 0)) {
                    if (cntNew[i] < (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell) {
                        sz1 = (int32)2 + libc.Int32FromUint16(_cachedCellSize(tls, __ccgo_ptr(bp + 56), cntNew[i]));
                    } else {
                        sz1 = 0;
                    }
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 24 + (uintptr)(i + (int32)1) * 4))).Value -= sz1;
            }
            if (cntNew[i] >= (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell) {
                k = i + (int32)1;
            } else {
                if (i > 0) {
                    v1 = cntNew[i - (int32)1];
                } else {
                    v1 = 0;
                }
                if (cntNew[i] <= v1) {
                    rc = _sqlite3CorruptError(tls, (int32)81777);
                    goto balance_cleanup;
                }
            }
            goto _9;
_9:;
            i = i + 1;
        }
        i = k - (int32)1;
        for (;;) {
            if (!(i > 0)) {
                break;
            }
            szRight = @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5);
            szLeft = @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i - (int32)1, 5);
            r = cntNew[i - (int32)1] - (int32)1;
            d = r + (int32)1 - leafData;
            _cachedCellSize(tls, __ccgo_ptr(bp + 56), d);
            for (var cond = true; cond; cond = r >= 0) {
                szR = libc.Int32FromUint16(_cachedCellSize(tls, __ccgo_ptr(bp + 56), r));
                szD = libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)d * 2)));
                v14 = szRight != 0; if (v14) {
                    v13 = bBulk != 0; if (!v13) {
                        if (i == k - (int32)1) {
                            v1 = 0;
                        } else {
                            v1 = (int32)2;
                        }
                    }
                }
                if (v14 && (v13 || szRight + szD + (int32)2 > szLeft - (szR + v1))) {
                    break;
                }
                szRight = szRight + (szD + (int32)2);
                szLeft = szLeft - (szR + (int32)2);
                cntNew[i - (int32)1] = r;
                r = r - 1;
                d = d - 1;
            }
            @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i, 5) = szRight;
            @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), i - (int32)1, 5) = szLeft;
            if (i > (int32)1) {
                v1 = cntNew[i - (int32)2];
            } else {
                v1 = 0;
            }
            if (cntNew[i - (int32)1] <= v1) {
                rc = _sqlite3CorruptError(tls, (int32)81821);
                goto balance_cleanup;
            }
            goto _11;
_11:;
            i = i - 1;
        }
        pageFlags = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), 0, 3))).Value.FaData)));
        i = 0;
        for (;;) {
            if (!(i < k)) {
                break;
            }
            if (i < nOld) {
                v17 = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3);
                apNew[i] = v17;
                pNew = v17;
                @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3) = (uintptr)0;
                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew)).Value.FpDbPage);
                nNew = nNew + 1;
                if (_sqlite3PagerPageRefcount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew)).Value.FpDbPage) != (int32)1 + libc.BoolInt32(i == iParentIdx - nxDiv) && rc == SQLITE_OK) {
                    rc = _sqlite3CorruptError(tls, (int32)81854);
                }
                if (rc != 0) {
                    goto balance_cleanup;
                }
            } else {
                if (bBulk != 0) {
                    v18 = (uint32)1;
                } else {
                    v18 = pgno;
                }
                rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpNew), @unsafe.Pointer.FromPinnedBox(Ꮡpgno), v18, (uint8)0);
                if (rc != 0) {
                    goto balance_cleanup;
                }
                _zeroPage(tls, pNew, pageFlags);
                apNew[i] = pNew;
                nNew = nNew + 1;
                cntOld[i] = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell;
                if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
                    _ptrmapPut(tls, pBt, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew)).Value.Fpgno, (uint8)PTRMAP_BTREE, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    if (rc != SQLITE_OK) {
                        goto balance_cleanup;
                    }
                }
            }
            goto _16;
_16:;
            i = i + 1;
        }
        i = 0;
        for (;;) {
            if (!(i < nNew)) {
                break;
            }
            aPgno[i] = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.Fpgno;
            goto _19;
_19:;
            i = i + 1;
        }
        i = 0;
        for (;;) {
            if (!(i < nNew - (int32)1)) {
                break;
            }
            iB = i;
            j = i + (int32)1;
            for (;;) {
                if (!(j < nNew)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[j])).Value.Fpgno < (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[iB])).Value.Fpgno) {
                    iB = j;
                }
                goto _21;
_21:;
                j = j + 1;
            }
            if (iB != i) {
                pgnoA = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.Fpgno;
                pgnoB = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[iB])).Value.Fpgno;
                pgnoTemp = libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + (uint32)1;
                fgA = (@unsafe.Pointer.To<TDbPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.FpDbPage)).Value.Fflags;
                fgB = (@unsafe.Pointer.To<TDbPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[iB])).Value.FpDbPage)).Value.Fflags;
                _sqlite3PagerRekey(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.FpDbPage, pgnoTemp, fgB);
                _sqlite3PagerRekey(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[iB])).Value.FpDbPage, pgnoA, fgA);
                _sqlite3PagerRekey(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.FpDbPage, pgnoB, fgB);
                (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.Fpgno = pgnoB;
                (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[iB])).Value.Fpgno = pgnoA;
            }
            goto _20;
_20:;
            i = i + 1;
        }
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRight, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[nNew - (int32)1])).Value.Fpgno);
        if ((pageFlags & (int32)PTF_LEAF) == 0 && nOld != nNew) {
            if (nNew > nOld) {
                pOld1 = apNew[nOld - (int32)1];
            } else {
                pOld1 = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), nOld - (int32)1, 3);
            }
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[nNew - (int32)1])).Value.FaData + 8, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld1)).Value.FaData + 8, (uint64)4);
        }
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
            v17 = apNew[0];
            pOld2 = v17;
            pNew1 = v17;
            cntOldNext = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew1)).Value.FnCell) + libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew1)).Value.FnOverflow);
            iNew = 0;
            iOld = 0;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell)) {
                    break;
                }
                pCell = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)i * 8));
                while (i == cntOldNext) {
                    iOld = iOld + 1;
                    if (iOld < nNew) {
                        v17 = apNew[iOld];
                    } else {
                        v17 = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), iOld, 3);
                    }
                    pOld2 = v17;
                    cntOldNext = cntOldNext + (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld2)).Value.FnCell) + libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld2)).Value.FnOverflow) + libc.BoolInt32(!(leafData != 0)));
                }
                if (i == cntNew[iNew]) {
                    iNew = iNew + 1;
                    v1 = iNew;
                    pNew1 = apNew[v1];
                    if (!(leafData != 0)) {
                        goto _23;
                    }
                }
                if (iOld >= nNew || (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew1)).Value.Fpgno != aPgno[iOld] || !((uint64)pCell >= (uint64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld2)).Value.FaData && (uint64)pCell < (uint64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOld2)).Value.FaDataEnd)) {
                    if (!(leafCorrection != 0)) {
                        _ptrmapPut(tls, pBt, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell), (uint8)PTRMAP_BTREE, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew1)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    }
                    if (libc.Int32FromUint16(_cachedCellSize(tls, __ccgo_ptr(bp + 56), i)) > libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew1)).Value.FminLocal)) {
                        _ptrmapPutOvflPtr(tls, pNew1, pOld2, pCell, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    }
                    if (rc != 0) {
                        goto balance_cleanup;
                    }
                }
                goto _23;
_23:;
                i = i + 1;
            }
        }
        i = 0;
        for (;;) {
            if (!(i < nNew - (int32)1)) {
                break;
            }
            pNew2 = apNew[i];
            j = cntNew[i];
            pCell1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)j * 8));
            sz2 = libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)j * 2))) + libc.Int32FromUint16(leafCorrection);
            pTemp1 = aOvflSpace + (uintptr)iOvflSpace;
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew2)).Value.Fleaf != 0)) {
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew2)).Value.FaData + 8, pCell1, (uint64)4);
            } else {
                if (leafData != 0) {
                    j = j - 1;
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew2)).Value.FxParseCell)(tls, pNew2, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell + (uintptr)j * 8)), bp + 160);
                    pCell1 = pTemp1;
                    sz2 = (int32)4 + _sqlite3PutVarint(tls, (@unsafe.Pointer)(pCell1 + 4), libc.Uint64FromInt64((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 160))).Value.FnKey));
                    pTemp1 = (uintptr)0;
                } else {
                    pCell1 = pCell1 - (uintptr)4;
                    if (libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr((@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FszCell + (uintptr)j * 2))) == (int32)4) {
                        sz2 = libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FxCellSize)(tls, pParent, pCell1));
                    }
                }
            }
            iOvflSpace = iOvflSpace + sz2;
            k = 0;
            for (;;) {
                if (!(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 56 + 80 + (uintptr)k * 4)) <= j)) {
                    break;
                }
                goto _27;
_27:;
                k = k + 1;
            }
            pSrcEnd = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 56 + 32 + (uintptr)k * 8));
            if ((uint64)pCell1 < (uint64)pSrcEnd && (uint64)(pCell1 + (uintptr)sz2) > (uint64)pSrcEnd) {
                rc = _sqlite3CorruptError(tls, (int32)82060);
                goto balance_cleanup;
            }
            rc = _insertCell(tls, pParent, nxDiv + i, pCell1, sz2, pTemp1, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew2)).Value.Fpgno);
            if (rc != SQLITE_OK) {
                goto balance_cleanup;
            }
            goto _26;
_26:;
            i = i + 1;
        }
        i = (int32)1 - nNew;
        for (;;) {
            if (!(i < nNew)) {
                break;
            }
            if (i < 0) {
                v1 = -i;
            } else {
                v1 = i;
            }
            iPg = v1;
            if (@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp + 45), iPg, 5) != 0) {
                goto _28;
            }
            if (i >= 0 || cntOld[iPg - (int32)1] >= cntNew[iPg - (int32)1]) {
                if (iPg == 0) {
                    v1 = libc.Int32FromInt32(0);
                    iOld1 = v1;
                    iNew1 = v1;
                    nNewCell = cntNew[0];
                } else {
                    if (iPg < nOld) {
                        v1 = cntOld[iPg - (int32)1] + libc.BoolInt32(!(leafData != 0));
                    } else {
                        v1 = (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FnCell;
                    }
                    iOld1 = v1;
                    iNew1 = cntNew[iPg - (int32)1] + libc.BoolInt32(!(leafData != 0));
                    nNewCell = cntNew[iPg] - iNew1;
                }
                rc = _editPage(tls, apNew[iPg], iOld1, iNew1, nNewCell, __ccgo_ptr(bp + 56));
                if (rc != 0) {
                    goto balance_cleanup;
                }
                @unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp + 45), iPg, 5) = (Tu8)(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp + 45), iPg, 5) + 1);
                (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[iPg])).Value.FnFree = usableSpace - @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 24), iPg, 5);
            }
            goto _28;
_28:;
            i = i + 1;
        }
        if (isRoot != 0 && libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnCell) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FhdrOffset) <= (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[0])).Value.FnFree) {
            rc = _defragmentPage(tls, apNew[0], -(int32)1);
            _copyNodeContent(tls, apNew[0], pParent, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            _freePage(tls, apNew[0], @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        } else {
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0 && !(leafCorrection != 0)) {
                i = 0;
                for (;;) {
                    if (!(i < nNew)) {
                        break;
                    }
                    key = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.FaData + 8));
                    _ptrmapPut(tls, pBt, key, (uint8)PTRMAP_BTREE, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)apNew[i])).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    goto _32;
_32:;
                    i = i + 1;
                }
            }
        }
        i = nNew;
        for (;;) {
            if (!(i < nOld)) {
                break;
            }
            _freePage(tls, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            goto _33;
_33:;
            i = i + 1;
        }
        goto balance_cleanup;
balance_cleanup:;
        _sqlite3DbFree__ccgo_gp(tls, nil, (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 56))).Value.FapCell);
        i = 0;
        for (;;) {
            if (!(i < nOld)) {
                break;
            }
            _releasePage(tls, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 3));
            goto _34;
_34:;
            i = i + 1;
        }
        i = 0;
        for (;;) {
            if (!(i < nNew)) {
                break;
            }
            _releasePage(tls, apNew[i]);
            goto _35;
_35:;
            i = i + 1;
        }
        r1 = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r1;
}

internal static void _existsToJoin(ж<libc.TLS> tls, uintptr pParse, uintptr p, uintptr pWhere) {
    uintptr aCsrMap = default;
    uintptr db = default;
    uintptr pRight = default;
    uintptr pSub = default;
    uintptr pSubWhere = default;
    _ = aCsrMap;
    _ = db;
    _ = pRight;
    _ = pSub;
    _ = pSubWhere;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0 && pWhere != (uintptr)0 && !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_InnerON))) != libc.Uint32FromInt32(0)) && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc != (uintptr)0 && (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc < libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) && ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit == (uintptr)0 || (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit)).Value.FpRight == (uintptr)0)) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.Fop) == (int32)TK_AND) {
            pRight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.FpRight;
            _existsToJoin(tls, pParse, p, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.FpLeft);
            _existsToJoin(tls, pParse, p, pRight);
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.Fop) == (int32)TK_EXISTS) {
                pSub = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWhere + 32));
                pSubWhere = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpWhere;
                if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc)).Value.FnSrc == (int32)1 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & (uint32)SF_Aggregate) == (uint32)0 && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc + 8 + 24 + 4)) & 0x4) >> 2) != 0) && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit == (uintptr)0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior == (uintptr)0) {
                    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
                    aCsrMap = _sqlite3DbMallocZero(tls, db, (uint64)(libc.Uint64FromInt32((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab + libc.Int32FromInt32(2)) * (uint64)4));
                    if (aCsrMap == (uintptr)0) {
                        return;
                    }
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap))).Value = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab + (int32)1;
                    _renumberCursors(tls, pParse, pSub, -(int32)1, aCsrMap);
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aCsrMap);
                    libc.Xmemset(tls, pWhere, 0, (uint64)72);
                    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.Fop = (uint8)TK_INTEGER;
                    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere).of(TExpr.ᏑFu).Reinterpret<TExprᴛ1_Fu, int32>()).Value = (int32)1;
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWhere + 4))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(EP_IntValue));
                    libc.SetBitFieldPtr32Uint32((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc + 8 + 24 + 4, libc.Uint32FromInt32(1), 18, 0x40000);
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc = _sqlite3SrcListAppendList(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc);
                    if (pSubWhere != 0) {
                        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere = _sqlite3PExpr(tls, pParse, (int32)TK_AND, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere, pSubWhere);
                        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpWhere = (uintptr)0;
                    }
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc = (uintptr)0;
                    _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3SelectDeleteGeneric), pSub);
                    _recomputeColumnsUsed(tls, p, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + (uintptr)((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc - (int32)1) * 80);
                }
            }
        }
    }
}

internal static int32 /*r*/ _fillInCell(ж<libc.TLS> tls, uintptr pPage, uintptr pCell, @unsafe.Pointer pX, @unsafe.Pointer pnSize) {
    Tu8 eType = default;
    int32 mn = default;
    int32 n = default;
    int32 nHeader = default;
    int32 nPayload = default;
    int32 nSrc = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 spaceLeft = default;
    int32 v1 = default;
    uintptr pBt = default;
    ref var pOvfl = ref heap(new uintptr(), out var ᏑpOvfl);
    uintptr pPayload = default;
    uintptr pPrior = default;
    uintptr pSrc = default;
    uintptr pToRelease = default;
    ref var pgnoOvfl = ref heap(new TPgno(), out var ᏑpgnoOvfl);
    TPgno pgnoPtrmap = default;
    _ = eType;
    _ = mn;
    _ = n;
    _ = nHeader;
    _ = nPayload;
    _ = nSrc;
    _ = pBt;
    _ = pOvfl;
    _ = pPayload;
    _ = pPrior;
    _ = pSrc;
    _ = pToRelease;
    _ = pgnoOvfl;
    _ = pgnoPtrmap;
    _ = rc;
    _ = spaceLeft;
    _ = v1;
    nHeader = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize);
    if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey != 0) {
        nPayload = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 36)) + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 40));
        pSrc = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 16));
        nSrc = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 36));
        if (libc.Uint32FromInt32(nPayload) < libc.Uint32FromInt32(0x80)) {
            (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pCell + (uintptr)nHeader))).Value = libc.Uint8FromInt32(nPayload);
            v1 = libc.Int32FromInt32(1);
        } else {
            v1 = _sqlite3PutVarint(tls, (@unsafe.Pointer)(pCell + (uintptr)nHeader), libc.Uint64FromInt32(nPayload));
        }
        nHeader = nHeader + libc.Int32FromUint8(libc.Uint8FromInt32(v1));
        nHeader = nHeader + _sqlite3PutVarint(tls, (@unsafe.Pointer)(pCell + (uintptr)nHeader), ~@unsafe.Pointer.To<Tu64>(@unsafe.Add(pX, 8)));
    } else {
        v1 = (int32)(~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8)));
        nPayload = v1;
        nSrc = v1;
        pSrc = ~@unsafe.Pointer.To<uintptr>(pX);
        if (libc.Uint32FromInt32(nPayload) < libc.Uint32FromInt32(0x80)) {
            (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pCell + (uintptr)nHeader))).Value = libc.Uint8FromInt32(nPayload);
            v1 = libc.Int32FromInt32(1);
        } else {
            v1 = _sqlite3PutVarint(tls, (@unsafe.Pointer)(pCell + (uintptr)nHeader), libc.Uint64FromInt32(nPayload));
        }
        nHeader = nHeader + libc.Int32FromUint8(libc.Uint8FromInt32(v1));
    }
    pPayload = pCell + (uintptr)nHeader;
    if (nPayload <= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaxLocal)) {
        n = nHeader + nPayload;
        if (n < (int32)4) {
            n = (int32)4;
            (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pPayload + (uintptr)nPayload))).Value = (uint8)0;
        }
        (@unsafe.Pointer.To<int32>(pnSize)).Value = n;
        libc.Xmemcpy(tls, pPayload, pSrc, libc.Uint64FromInt32(nSrc));
        libc.Xmemset(tls, pPayload + (uintptr)nSrc, 0, libc.Uint64FromInt32(nPayload - nSrc));
        return SQLITE_OK;
    }
    mn = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FminLocal);
    n = libc.Int32FromUint32(libc.Uint32FromInt32(mn) + libc.Uint32FromInt32(nPayload - mn) % ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize - (uint32)4));
    if (n > libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaxLocal)) {
        n = mn;
    }
    spaceLeft = n;
    (@unsafe.Pointer.To<int32>(pnSize)).Value = n + nHeader + (int32)4;
    pPrior = pCell + (uintptr)(nHeader + n);
    pToRelease = (uintptr)0;
    pgnoOvfl = (uint32)0;
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
    while ((int32)1 != 0) {
        n = nPayload;
        if (n > spaceLeft) {
            n = spaceLeft;
        }
        if (nSrc >= n) {
            libc.Xmemcpy(tls, pPayload, pSrc, libc.Uint64FromInt32(n));
        } else {
            if (nSrc > 0) {
                n = nSrc;
                libc.Xmemcpy(tls, pPayload, pSrc, libc.Uint64FromInt32(n));
            } else {
                libc.Xmemset(tls, pPayload, 0, libc.Uint64FromInt32(n));
            }
        }
        nPayload = nPayload - n;
        if (nPayload <= 0) {
            break;
        }
        pPayload = pPayload + (uintptr)n;
        pSrc = pSrc + (uintptr)n;
        nSrc = nSrc - n;
        spaceLeft = spaceLeft - n;
        if (spaceLeft == 0) {
            pOvfl = (uintptr)0;
            pgnoPtrmap = pgnoOvfl;
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
                for (var cond = true; cond; cond = _ptrmapPageno(ref (tls).DerefOrNull(), pBt, pgnoOvfl) == pgnoOvfl || pgnoOvfl == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
                    pgnoOvfl = pgnoOvfl + 1;
                }
            }
            rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpOvfl), @unsafe.Pointer.FromPinnedBox(ᏑpgnoOvfl), pgnoOvfl, (uint8)0);
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0 && rc == SQLITE_OK) {
                if (pgnoPtrmap != 0) {
                    v1 = (int32)PTRMAP_OVERFLOW2;
                } else {
                    v1 = (int32)PTRMAP_OVERFLOW1;
                }
                eType = libc.Uint8FromInt32(v1);
                _ptrmapPut(tls, pBt, pgnoOvfl, eType, pgnoPtrmap, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                if (rc != 0) {
                    _releasePage(tls, pOvfl);
                }
            }
            if (rc != 0) {
                _releasePage(tls, pToRelease);
                return rc;
            }
            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPrior, pgnoOvfl);
            _releasePage(tls, pToRelease);
            pToRelease = pOvfl;
            pPrior = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOvfl)).Value.FaData;
            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPrior, (uint32)0);
            pPayload = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOvfl)).Value.FaData + 4;
            spaceLeft = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)4);
        }
    }
    _releasePage(tls, pToRelease);
    return SQLITE_OK;
}

internal static int32 /*r*/ _flattenSubquery(ж<libc.TLS> tls, uintptr pParse, uintptr p, int32 iFrom, int32 isAgg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr aCsrMap = default;
        uintptr db = default;
        uintptr pItem = default;
        uintptr pItemTab = default;
        uintptr pLimit = default;
        uintptr pNew = default;
        uintptr pOrderBy = default;
        uintptr pOrderBy1 = default;
        uintptr pParent = default;
        uintptr pPrior = default;
        uintptr pSrc = default;
        uintptr pSub = default;
        uintptr pSub1 = default;
        uintptr pSubSrc = default;
        uintptr pSubitem = default;
        uintptr pTabToDel = default;
        uintptr pToplevel = default;
        uintptr pWhere = default;
        uintptr zSavedAuthContext = default;
        uintptr v5 = default;
        int32 i = default;
        int32 iNewParent = default;
        int32 iParent = default;
        int32 ii = default;
        int32 isOuterJoin = default;
        int32 nSubSrc = default;
        int32 v4 = default;
        Tu8 jointype = default;
        ref var x = ref heap(new TSubstContext(), out var Ꮡx);
        TWalker _ᴛ1 = new();
        _ = aCsrMap;
        _ = db;
        _ = i;
        _ = iNewParent;
        _ = iParent;
        _ = ii;
        _ = isOuterJoin;
        _ = jointype;
        _ = nSubSrc;
        _ = pItem;
        _ = pItemTab;
        _ = pLimit;
        _ = pNew;
        _ = pOrderBy;
        _ = pOrderBy1;
        _ = pParent;
        _ = pPrior;
        _ = pSrc;
        _ = pSub;
        _ = pSub1;
        _ = pSubSrc;
        _ = pSubitem;
        _ = pTabToDel;
        _ = pToplevel;
        _ = pWhere;
        _ = x;
        _ = zSavedAuthContext;
        _ = v4;
        _ = v5;
        zSavedAuthContext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext;
        iNewParent = -(int32)1;
        isOuterJoin = 0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        aCsrMap = (uintptr)0;
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_QueryFlattener))) != (uint32)0) {
            r = 0; goto ᒐdone;
        }
        pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        pSubitem = pSrc + 8 + (uintptr)iFrom * 80;
        iParent = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FiCursor;
        pSub = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSubitem + 72))))).Value.FpSelect;
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin != 0 || (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpWin != 0) {
            r = 0; goto ᒐdone;
        }
        pSubSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc;
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit != 0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0 && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit)).Value.FpRight != 0) {
            r = 0; goto ᒐdone;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Compound) != (uint32)0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSubSrc)).Value.FnSrc == 0) {
            r = 0; goto ᒐdone;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & (uint32)SF_Distinct) != 0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0 && ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc > (int32)1 || isAgg != 0)) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != 0) {
            r = 0; goto ᒐdone;
        }
        if (isAgg != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != 0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere != 0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Distinct) != (uint32)0) {
            r = 0; goto ᒐdone;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_Recursive))) != 0) {
            r = 0; goto ᒐdone;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_LTORJ))) != 0) {
            if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSubSrc)).Value.FnSrc > (int32)1 || (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Distinct) != (uint32)0 || (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0) {
                r = 0; goto ᒐdone;
            }
            isOuterJoin = (int32)1;
        }
        if (iFrom > 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSubSrc + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior != 0) {
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != 0) {
                r = 0; goto ᒐdone;
            }
            if (isAgg != 0 || (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Distinct) != (uint32)0 || isOuterJoin > 0) {
                r = 0; goto ᒐdone;
            }
            pSub1 = pSub;
            for (;;) {
                if (!(pSub1 != 0)) {
                    break;
                }
                if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_Distinct) | libc.Int32FromInt32(SF_Aggregate))) != (uint32)0 || (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpPrior != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.Fop) != (int32)TK_ALL || (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpSrc)).Value.FnSrc < (int32)1 || (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpWin != 0) {
                    r = 0; goto ᒐdone;
                }
                if (iFrom > 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpSrc + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
                    r = 0; goto ᒐdone;
                }
                goto _1;
_1:;
                pSub1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpPrior;
            }
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != 0) {
                ii = 0;
                for (;;) {
                    if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy)).Value.FnExpr)) {
                        break;
                    }
                    if (libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy + 8 + (uintptr)ii * 32 + 24))) == 0) {
                        r = 0; goto ᒐdone;
                    }
                    goto _2;
_2:;
                    ii = ii + 1;
                }
            }
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Recursive) != 0) {
                r = 0; goto ᒐdone;
            }
            if (_compoundHasDifferentAffinities(tls, pSub) != 0) {
                r = 0; goto ᒐdone;
            }
            if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc > (int32)1) {
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnSelect > (int32)500) {
                    r = 0; goto ᒐdone;
                }
                if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FlttnUnionAll))) != (uint32)0) {
                    r = 0; goto ᒐdone;
                }
                aCsrMap = _sqlite3DbMallocZero(tls, db, libc.Uint64FromInt64((int64)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab + libc.Int64FromInt32(1)) * (uint64)4);
                if (aCsrMap != 0) {
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap))).Value = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
                }
            }
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FzName;
        _sqlite3AuthCheck(tls, pParse, (int32)SQLITE_SELECT, (uintptr)0, (uintptr)0, (uintptr)0);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = zSavedAuthContext;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSubitem + 24 + 4)) & 0x4) >> 2) != 0) {
            pSub1 = _sqlite3SubqueryDetach(tls, db, pSubitem);
        } else {
            pSub1 = (uintptr)0;
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FzName);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FzAlias);
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FzName = (uintptr)0;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FzAlias = (uintptr)0;
        pSub = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior;
        for (;;) {
            if (!(pSub != 0)) {
                break;
            }
            pOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy;
            pLimit = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit;
            pPrior = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior;
            pItemTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FpSTab;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FpSTab = (uintptr)0;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = (uintptr)0;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior = (uintptr)0;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit = (uintptr)0;
            pNew = _sqlite3SelectDup(tls, db, p, 0);
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit = pLimit;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = pOrderBy;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.Fop = (uint8)TK_ALL;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FpSTab = pItemTab;
            if (pNew == (uintptr)0) {
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior = pPrior;
            } else {
                v5 = pParse + 132;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5) + 1;
                v4 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5);
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FselId = libc.Uint32FromInt32(v4);
                if (aCsrMap != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed) == 0) {
                    _renumberCursors(tls, pParse, pNew, iFrom, aCsrMap);
                }
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpPrior = pPrior;
                if (pPrior != 0) {
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pPrior)).Value.FpNext = pNew;
                }
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpNext = p;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior = pNew;
            }
            goto _3;
_3:;
            pSub = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior;
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aCsrMap);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            _sqlite3SrcItemAttachSubquery(tls, pParse, pSubitem, pSub1, 0);
            r = (int32)1; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FpSTab != (uintptr)0) {
            pTabToDel = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FpSTab;
            if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTabToDel)).Value.FnTabRef == (uint32)1) {
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) {
                    v5 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel;
                } else {
                    v5 = pParse;
                }
                pToplevel = v5;
                _sqlite3ParserAddCleanup(tls, pToplevel, __ccgo_fp(_sqlite3DeleteTableGeneric), pTabToDel);
            } else {
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTabToDel)).Value.FnTabRef = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTabToDel)).Value.FnTabRef - 1;
            }
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSubitem)).Value.FpSTab = (uintptr)0;
        }
        pSub = pSub1;
        pParent = p;
        for (;;) {
            if (!(pParent != 0)) {
                break;
            }
            jointype = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpSrc + 8 + (uintptr)iFrom * 80))).Value.Ffg.Fjointype;
            pSubSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpSrc;
            nSubSrc = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSubSrc)).Value.FnSrc;
            pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpSrc;
            if (nSubSrc > (int32)1) {
                pSrc = _sqlite3SrcListEnlarge(tls, pParse, pSrc, nSubSrc - (int32)1, iFrom + (int32)1);
                if (pSrc == (uintptr)0) {
                    break;
                }
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpSrc = pSrc;
            }
            iNewParent = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSubSrc + 8))).Value.FiCursor;
            i = 0;
            for (;;) {
                if (!(i < nSubSrc)) {
                    break;
                }
                pItem = pSrc + 8 + (uintptr)(i + iFrom) * 80;
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x800) >> 11) != 0) {
                    _sqlite3IdListDelete(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64)));
                }
                (@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(pItem))).Value = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSubSrc + 8 + (uintptr)i * 80))).Value;
                v5 = pItem + 24;
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v5)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v5)) | libc.Int32FromUint8(jointype) & libc.Int32FromInt32(JT_LTORJ));
                libc.Xmemset(tls, pSubSrc + 8 + (uintptr)i * 80, 0, (uint64)80);
                goto _8;
_8:;
                i = i + 1;
            }
            v5 = pSrc + 8 + (uintptr)iFrom * 80 + 24;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v5)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v5)) | libc.Int32FromUint8(jointype));
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != 0) {
                pOrderBy1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy1)).Value.FnExpr)) {
                        break;
                    }
                    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)(pOrderBy1 + 8 + (uintptr)i * 32 + 24))).Value = (uint16)0;
                    goto _11;
_11:;
                    i = i + 1;
                }
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpOrderBy = pOrderBy1;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy = (uintptr)0;
            }
            pWhere = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpWhere;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpWhere = (uintptr)0;
            if (isOuterJoin > 0) {
                _sqlite3SetJoinExpr(ref (tls).DerefOrNull(), pWhere, iNewParent, (uint32)EP_OuterON);
            }
            if (pWhere != 0) {
                if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpWhere != 0) {
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpWhere = _sqlite3PExpr(tls, pParse, (int32)TK_AND, pWhere, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpWhere);
                } else {
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpWhere = pWhere;
                }
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed) == 0) {
                x.FpParse = pParse;
                x.FiTable = iParent;
                x.FiNewTable = iNewParent;
                x.FisOuterJoin = isOuterJoin;
                x.FnSelDepth = 0;
                x.FpEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpEList;
                x.FpCList = _findLeftmostExprlist(ref (tls).DerefOrNull(), pSub);
                _substSelect(tls, @unsafe.Pointer.FromPinnedBox(Ꮡx), pParent, 0);
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pParent + 4))).Value |= (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & (uint32)SF_Compound);
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0) {
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpLimit = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit = (uintptr)0;
            }
            i = 0;
            for (;;) {
                if (!(i < nSubSrc)) {
                    break;
                }
                _recomputeColumnsUsed(tls, pParent, pSrc + 8 + (uintptr)(i + iFrom) * 80);
                goto _12;
_12:;
                i = i + 1;
            }
            goto _7;
_7:;
            pParent = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pParent)).Value.FpPrior;
            pSub = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior;
        }
        _sqlite3AggInfoPersistWalkerInit(tls, bp, pParse);
        _sqlite3WalkSelect(tls, bp, pSub1);
        _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSub1);
        r = (int32)1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _freePage2(ж<libc.TLS> tls, uintptr pBt, uintptr pMemPage, TPgno iPage) {
    TPgno iTrunk = default;
    Tu32 nFree = default;
    Tu32 nLeaf = default;
    ref var pPage = ref heap(new uintptr(), out var ᏑpPage);
    uintptr pPage1 = default;
    ref var pTrunk = ref heap(new uintptr(), out var ᏑpTrunk);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 v1 = default;
    int32 v3 = default;
    bool v2 = default;
    bool v4 = default;
    _ = iTrunk;
    _ = nFree;
    _ = nLeaf;
    _ = pPage;
    _ = pPage1;
    _ = pTrunk;
    _ = rc;
    _ = v1;
    _ = v2;
    _ = v3;
    _ = v4;
    pTrunk = (uintptr)0;
    iTrunk = (uint32)0;
    pPage1 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1;
    if (iPage < (uint32)2 || iPage > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage) {
        return _sqlite3CorruptError(tls, (int32)80027);
    }
    if (pMemPage != 0) {
        pPage = pMemPage;
        _sqlite3PagerRef(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
    } else {
        pPage = _btreePageLookup(tls, pBt, iPage);
    }
    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FpDbPage);
    if (rc != 0) {
        goto freepage_out;
    }
    nFree = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 36));
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 36), nFree + (uint32)1);
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_SECURE_DELETE) != 0) {
        v2 = !(pPage != 0); if (v2) {
            v1 = _btreeGetPage(tls, pBt, iPage, @unsafe.Pointer.FromBox(ᏑpPage), 0);
            rc = v1;
        }
        v4 = v2 && v1 != 0; if (!v4) {
            v3 = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
            rc = v3;
        }
        if (v4 || v3 != 0) {
            goto freepage_out;
        }
        libc.Xmemset(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData, 0, (uint64)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FpageSize);
    }
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
        _ptrmapPut(tls, pBt, iPage, (uint8)PTRMAP_FREEPAGE, (uint32)0, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (rc != 0) {
            goto freepage_out;
        }
    }
    if (nFree != (uint32)0) {
        iTrunk = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 32));
        if (iTrunk > _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
            rc = _sqlite3CorruptError(tls, (int32)80074);
            goto freepage_out;
        }
        rc = _btreeGetPage(tls, pBt, iTrunk, @unsafe.Pointer.FromBox(ᏑpTrunk), 0);
        if (rc != SQLITE_OK) {
            goto freepage_out;
        }
        nLeaf = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData + 4));
        if (nLeaf > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize / (uint32)4 - (uint32)2) {
            rc = _sqlite3CorruptError(tls, (int32)80085);
            goto freepage_out;
        }
        if (nLeaf < (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize / (uint32)4 - (uint32)8) {
            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FpDbPage);
            if (rc == SQLITE_OK) {
                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData + 4), nLeaf + (uint32)1);
                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTrunk)).Value.FaData + (uintptr)((uint32)8 + nLeaf * (uint32)4)), iPage);
                if (pPage != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_SECURE_DELETE) == 0) {
                    _sqlite3PagerDontWrite(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
                }
                rc = _btreeSetHasContent(tls, pBt, iPage);
            }
            goto freepage_out;
        }
    }
    v2 = pPage == (uintptr)0; if (v2) {
        v1 = _btreeGetPage(tls, pBt, iPage, @unsafe.Pointer.FromBox(ᏑpPage), 0);
        rc = v1;
    }
    if (v2 && SQLITE_OK != v1) {
        goto freepage_out;
    }
    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
    if (rc != SQLITE_OK) {
        goto freepage_out;
    }
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData, iTrunk);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + 4), (uint32)0);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 32), iPage);
    goto freepage_out;
freepage_out:;
    if (pPage != 0) {
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = (uint8)0;
    }
    _releasePage(tls, pPage);
    _releasePage(tls, pTrunk);
    return rc;
}

internal static int32 /*r*/ _insertCell(ж<libc.TLS> tls, uintptr pPage, int32 i, uintptr pCell, int32 sz, uintptr pTemp, TPgno iChild) {
    uintptr data = default;
    uintptr pIns = default;
    uintptr v2 = default;
    ref var idx = ref heap(new int32(), out var Ꮡidx);
    int32 j = default;
    int32 rc = default;
    ref var rc2 = ref heap(new int32(), out var Ꮡrc2);
    Tu8 v1 = default;
    _ = data;
    _ = idx;
    _ = j;
    _ = pIns;
    _ = rc;
    _ = rc2;
    _ = v1;
    _ = v2;
    idx = 0;
    if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnOverflow != 0 || sz + (int32)2 > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree) {
        if (pTemp != 0) {
            libc.Xmemcpy(tls, pTemp, pCell, libc.Uint64FromInt32(sz));
            pCell = pTemp;
        }
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell, iChild);
        v2 = pPage + 12;
        v1 = ~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)).Value = (Tu8)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2) + 1);
        j = libc.Int32FromUint8(v1);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pPage + 40 + (uintptr)j * 8))).Value = pCell;
        (@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pPage + 28 + (uintptr)j * 2))).Value = libc.Uint16FromInt32(i);
    } else {
        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
        if (rc != SQLITE_OK) {
            return rc;
        }
        data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
        rc = _allocateSpace(tls, pPage, sz, @unsafe.Pointer.FromPinnedBox(Ꮡidx));
        if (rc != 0) {
            return rc;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pPage + 20))).Value -= libc.Int32FromUint16(libc.Uint16FromInt32(libc.Int32FromInt32(2) + sz));
        libc.Xmemcpy(tls, data + (uintptr)(idx + (int32)4), pCell + (uintptr)4, libc.Uint64FromInt32(sz - (int32)4));
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(data + (uintptr)idx), iChild);
        pIns = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)(i * (int32)2);
        libc.Xmemmove(tls, pIns + (uintptr)2, pIns, libc.Uint64FromInt32((int32)2 * (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - i)));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pIns))).Value = libc.Uint8FromInt32(idx.Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pIns + 1))).Value = libc.Uint8FromInt32(idx);
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell = (Tu16)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell + 1);
        v2 = data + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)4);
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)).Value = (Tu8)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2) + 1);
        v1 = ~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2);
        if (libc.Int32FromUint8(v1) == 0) {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)3)))).Value = (Tu8)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)3))) + 1);
        }
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FautoVacuum != 0) {
            rc2 = SQLITE_OK;
            _ptrmapPutOvflPtr(tls, pPage, pPage, pCell, @unsafe.Pointer.FromPinnedBox(Ꮡrc2));
            if (rc2 != 0) {
                return rc2;
            }
        }
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _loadStatTbl(ж<libc.TLS> tls, uintptr db, uintptr zSql1, uintptr zSql2, uintptr zDb) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Ti64 i = default;
        Ti64 nByte = default;
        int32 nCol = default;
        int32 nIdxCol = default;
        int32 nSample = default;
        int32 rc = default;
        uintptr pIdx = default;
        uintptr pIdx1 = default;
        uintptr pPrevIdx = default;
        uintptr pPtr = default;
        uintptr pSample = default;
        uintptr pSpace = default;
        ref var pStmt = ref heap(new uintptr(), out var ᏑpStmt);
        uintptr zIndex = default;
        uintptr zIndex1 = default;
        uintptr zSql = default;
        _ = i;
        _ = nByte;
        _ = nCol;
        _ = nIdxCol;
        _ = nSample;
        _ = pIdx;
        _ = pIdx1;
        _ = pPrevIdx;
        _ = pPtr;
        _ = pSample;
        _ = pSpace;
        _ = pStmt;
        _ = rc;
        _ = zIndex;
        _ = zIndex1;
        _ = zSql;
        pStmt = (uintptr)0;
        pPrevIdx = (uintptr)0;
        zSql = _sqlite3MPrintf(tls, db, zSql1, libc.VaList(bp + 8, zDb));
        if (!(zSql != 0)) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        rc = _sqlite3_prepare__ccgo_gp(tls, db, zSql, -(int32)1, @unsafe.Pointer.FromBox(ᏑpStmt), (uintptr)0);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zSql);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        while (Xsqlite3_step(tls, pStmt) == (int32)SQLITE_ROW) {
            nIdxCol = (int32)1;
            zIndex = Xsqlite3_column_text(tls, pStmt, 0);
            if (zIndex == (uintptr)0) {
                continue;
            }
            nSample = Xsqlite3_column_int(tls, pStmt, (int32)1);
            pIdx = _findIndexOrPrimaryKey(tls, db, zIndex, zDb);
            if (pIdx == (uintptr)0) {
                continue;
            }
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample != (uintptr)0) {
                continue;
            }
            if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
                nIdxCol = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
            } else {
                nIdxCol = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
            }
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnSampleCol = nIdxCol;
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FmxSample = nSample;
            nByte = (libc.Int64FromInt64(40) * (int64)nSample + libc.Int64FromInt32(7)) & (int64)(~libc.Int32FromInt32(7));
            nByte = nByte + libc.Int64FromInt64(8) * (int64)nIdxCol * (int64)3 * (int64)nSample;
            nByte = nByte + (int64)nIdxCol * libc.Int64FromInt64(8);
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample = _sqlite3DbMallocZero(tls, db, libc.Uint64FromInt64(nByte));
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample == (uintptr)0) {
                Xsqlite3_finalize(tls, pStmt);
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
            pPtr = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample;
            pPtr = pPtr + (uintptr)(((int64)nSample * libc.Int64FromInt64(40) + libc.Int64FromInt32(7)) & (int64)(~libc.Int32FromInt32(7)));
            pSpace = pPtr;
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaAvgEq = pSpace;
            pSpace = pSpace + (uintptr)nIdxCol * 8;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable + 48))).Value |= (uint32)TF_HasStat4;
            i = 0;
            for (;;) {
                if (!(i < (int64)nSample)) {
                    break;
                }
                (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample + (uintptr)i * 40))).Value.FanEq = pSpace;
                pSpace = pSpace + (uintptr)nIdxCol * 8;
                (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample + (uintptr)i * 40))).Value.FanLt = pSpace;
                pSpace = pSpace + (uintptr)nIdxCol * 8;
                (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample + (uintptr)i * 40))).Value.FanDLt = pSpace;
                pSpace = pSpace + (uintptr)nIdxCol * 8;
                goto _1;
_1:;
                i = i + 1;
            }
        }
        rc = Xsqlite3_finalize(tls, pStmt);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        zSql = _sqlite3MPrintf(tls, db, zSql2, libc.VaList(bp + 8, zDb));
        if (!(zSql != 0)) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        rc = _sqlite3_prepare__ccgo_gp(tls, db, zSql, -(int32)1, @unsafe.Pointer.FromBox(ᏑpStmt), (uintptr)0);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zSql);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        while (Xsqlite3_step(tls, pStmt) == (int32)SQLITE_ROW) {
            nCol = (int32)1;
            zIndex1 = Xsqlite3_column_text(tls, pStmt, 0);
            if (zIndex1 == (uintptr)0) {
                continue;
            }
            pIdx1 = _findIndexOrPrimaryKey(tls, db, zIndex1, zDb);
            if (pIdx1 == (uintptr)0) {
                continue;
            }
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FnSample >= (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FmxSample) {
                continue;
            }
            nCol = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FnSampleCol;
            if (pIdx1 != pPrevIdx) {
                _initAvgEq(ref (tls).DerefOrNull(), pPrevIdx);
                pPrevIdx = pIdx1;
            }
            pSample = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FaSample + (uintptr)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FnSample * 40;
            _decodeIntArray(tls, Xsqlite3_column_text(tls, pStmt, (int32)1), nCol, (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.FanEq, (uintptr)0, (uintptr)0);
            _decodeIntArray(tls, Xsqlite3_column_text(tls, pStmt, (int32)2), nCol, (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.FanLt, (uintptr)0, (uintptr)0);
            _decodeIntArray(tls, Xsqlite3_column_text(tls, pStmt, (int32)3), nCol, (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.FanDLt, (uintptr)0, (uintptr)0);
            (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fn = Xsqlite3_column_bytes(tls, pStmt, (int32)4);
            (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fp = _sqlite3DbMallocZero(tls, db, libc.Uint64FromInt32((@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fn + (int32)8));
            if ((@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fp == (uintptr)0) {
                Xsqlite3_finalize(tls, pStmt);
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
            if ((@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fn != 0) {
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fp, Xsqlite3_column_blob(tls, pStmt, (int32)4), libc.Uint64FromInt32((@unsafe.Pointer.To<TIndexSample>((@unsafe.Pointer)pSample)).Value.Fn));
            }
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FnSample = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FnSample + 1;
        }
        rc = Xsqlite3_finalize(tls, pStmt);
        if (rc == SQLITE_OK) {
            _initAvgEq(ref (tls).DerefOrNull(), pPrevIdx);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _newDatabase(ж<libc.TLS> tls, uintptr pBt) {
    uintptr data = default;
    uintptr pP1 = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = data;
    _ = pP1;
    _ = rc;
    _ = v1;
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage > (uint32)0) {
        return SQLITE_OK;
    }
    pP1 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pP1)).Value.FaData;
    rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pP1)).Value.FpDbPage);
    if (rc != 0) {
        return rc;
    }
    libc.Xmemcpy(tls, data, (uintptr)Ꮡ_zMagicHeader, (uint64)16);
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 16))).Value = (uint8)((Tu32)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize >> (int)(libc.Int32FromInt32(8)) & libc.Uint32FromInt32(0xff)));
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 17))).Value = (uint8)((Tu32)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize >> (int)(libc.Int32FromInt32(16)) & libc.Uint32FromInt32(0xff)));
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 18))).Value = (uint8)1;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 19))).Value = (uint8)1;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 20))).Value = (uint8)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize - (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize);
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 21))).Value = (uint8)64;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 22))).Value = (uint8)32;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 23))).Value = (uint8)32;
    libc.Xmemset(tls, data + 24, 0, libc.Uint64FromInt32(libc.Int32FromInt32(100) - libc.Int32FromInt32(24)));
    _zeroPage(tls, pP1, libc.Int32FromInt32(PTF_INTKEY) | libc.Int32FromInt32(PTF_LEAF) | libc.Int32FromInt32(PTF_LEAFDATA));
    v1 = pBt + 40;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTS_PAGESIZE_FIXED));
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(data + (uintptr)(libc.Int32FromInt32(36) + libc.Int32FromInt32(4) * libc.Int32FromInt32(4))), (uint32)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(data + (uintptr)(libc.Int32FromInt32(36) + libc.Int32FromInt32(7) * libc.Int32FromInt32(4))), (uint32)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FincrVacuum);
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = (uint32)1;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + 31))).Value = (uint8)1;
    return SQLITE_OK;
}

internal static int32 /*r*/ _resizeIndexObject(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, int32 N) {
    uintptr db = default;
    uintptr zExtra = default;
    Tu64 nByte = default;
    _ = db;
    _ = nByte;
    _ = zExtra;
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn) >= N) {
        return SQLITE_OK;
    }
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    nByte = (uint64)(libc.Uint64FromInt64(8) + libc.Uint64FromInt64(2) + libc.Uint64FromInt64(2) + libc.Uint64FromInt32(1)) * libc.Uint64FromInt32(N);
    zExtra = _sqlite3DbMallocZero(tls, db, nByte);
    if (zExtra == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemcpy(tls, zExtra, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl, (uint64)8 * (uint64)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl = zExtra;
    zExtra = zExtra + (uintptr)((uint64)8 * libc.Uint64FromInt32(N));
    libc.Xmemcpy(tls, zExtra, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiRowLogEst, (uint64)2 * libc.Uint64FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol) + libc.Int32FromInt32(1)));
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiRowLogEst = zExtra;
    zExtra = zExtra + (uintptr)((uint64)2 * libc.Uint64FromInt32(N));
    libc.Xmemcpy(tls, zExtra, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn, (uint64)2 * (uint64)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn = zExtra;
    zExtra = zExtra + (uintptr)((uint64)2 * libc.Uint64FromInt32(N));
    libc.Xmemcpy(tls, zExtra, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSortOrder, (uint64)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSortOrder = zExtra;
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn = libc.Uint16FromInt32(N);
    libc.SetBitFieldPtr16Uint32(pIdx + 100, libc.Uint32FromInt32(1), 4, 0x10);
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3BtreeIndexMoveto(ж<libc.TLS> tls, uintptr pCur, uintptr pIdxKey, @unsafe.Pointer pRes) {
    int32 c = default;
    int32 c1 = default;
    int32 idx = default;
    int32 lwr = default;
    int32 nCell = default;
    int32 nOverrun = default;
    int32 rc = default;
    int32 upr = default;
    int32 v1 = default;
    TPgno chldPg = default;
    uintptr pCell = default;
    uintptr pCellBody = default;
    uintptr pCellKey = default;
    uintptr pPage = default;
    uintptr v3 = default;
    ref var xRecordCompare = ref heap(new TRecordCompare(), out var ᏑxRecordCompare);
    Ti8 v10 = default;
    bool v2 = default;
    _ = c;
    _ = c1;
    _ = chldPg;
    _ = idx;
    _ = lwr;
    _ = nCell;
    _ = nOverrun;
    _ = pCell;
    _ = pCellBody;
    _ = pCellKey;
    _ = pPage;
    _ = rc;
    _ = upr;
    _ = xRecordCompare;
    _ = v1;
    _ = v10;
    _ = v2;
    _ = v3;
    xRecordCompare = _sqlite3VdbeFindCompare(ref (tls).DerefOrNull(), pIdxKey);
    (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FerrCode = (uint8)0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == CURSOR_VALID && (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.Fleaf != 0 && _cursorOnLastPage(ref (tls).DerefOrNull(), pCur) != 0) {
        v2 = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix) == libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FnCell) - (int32)1; if (v2) {
            v1 = _indexCellCompare(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage, libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix), pIdxKey, xRecordCompare);
            c = v1;
        }
        if (v2 && v1 <= 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FerrCode) == SQLITE_OK) {
            (@unsafe.Pointer.To<int32>(pRes)).Value = c;
            return SQLITE_OK;
        }
        if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage > 0 && _indexCellCompare(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage, 0, pIdxKey, xRecordCompare) <= 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FerrCode) == SQLITE_OK) {
            v3 = pCur + 1;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)) & ~(libc.Int32FromInt32(BTCF_ValidOvfl) | libc.Int32FromInt32(BTCF_AtLast)));
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FisInit != 0)) {
                return _sqlite3CorruptError(tls, (int32)79268);
            }
            goto bypass_moveto_root;
        }
        (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FerrCode = (uint8)SQLITE_OK;
    }
    rc = _moveToRoot(tls, pCur);
    if (rc != 0) {
        if (rc == (int32)SQLITE_EMPTY) {
            (@unsafe.Pointer.To<int32>(pRes)).Value = -(int32)1;
            return SQLITE_OK;
        }
        return rc;
    }
    goto bypass_moveto_root;
bypass_moveto_root:;
    for (;;) {
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        lwr = 0;
        upr = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - (int32)1;
        idx = upr >> (int)((int32)1);
        for (;;) {
            pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataOfst + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx) + 1)))));
            nCell = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCell)));
            if (nCell <= libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fmax1bytePayload)) {
                if (pCell + (uintptr)nCell >= (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
                    rc = _sqlite3CorruptError(tls, (int32)79327);
                    goto moveto_index_finish;
                }
                c1 = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, uintptr, int32>>(xRecordCompare)(tls, nCell, pCell + 1, pIdxKey);
            } else {
                v2 = !((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCell + 1))) & libc.Int32FromInt32(0x80)) != 0); if (v2) {
                    v1 = ((nCell & libc.Int32FromInt32(0x7f))).Lsh((int64)(libc.Int32FromInt32(7))) + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCell + 1)));
                    nCell = v1;
                }
                if (v2 && v1 <= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaxLocal) && pCell + (uintptr)nCell < (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
                    c1 = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, uintptr, int32>>(xRecordCompare)(tls, nCell, pCell + 2, pIdxKey);
                } else {
                    pCellBody = pCell - (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize;
                    nOverrun = (int32)18;
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, pCellBody, pCur + 48);
                    nCell = (int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey;
                    if (nCell < (int32)2 || libc.Uint32FromInt32(nCell) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt)).Value.FusableSize > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt)).Value.FnPage) {
                        rc = _sqlite3CorruptError(tls, (int32)79358);
                        goto moveto_index_finish;
                    }
                    pCellKey = _sqlite3Malloc(tls, libc.Uint64FromInt32(nCell) + libc.Uint64FromInt32(nOverrun));
                    if (pCellKey == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                        goto moveto_index_finish;
                    }
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(idx);
                    rc = _accessPayload(tls, pCur, (uint32)0, libc.Uint32FromInt32(nCell), pCellKey, 0);
                    libc.Xmemset(tls, pCellKey + (uintptr)nCell, 0, libc.Uint64FromInt32(nOverrun));
                    v3 = pCur + 1;
                    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)) & ~libc.Int32FromInt32(BTCF_ValidOvfl));
                    if (rc != 0) {
                        Xsqlite3_free(tls, pCellKey);
                        goto moveto_index_finish;
                    }
                    c1 = _sqlite3VdbeRecordCompare(tls, nCell, pCellKey, pIdxKey);
                    Xsqlite3_free(tls, pCellKey);
                }
            }
            if (c1 < 0) {
                lwr = idx + (int32)1;
            } else {
                if (c1 > 0) {
                    upr = idx - (int32)1;
                } else {
                    (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
                    rc = SQLITE_OK;
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(idx);
                    if ((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FerrCode != 0) {
                        rc = _sqlite3CorruptError(tls, (int32)79390);
                    }
                    goto moveto_index_finish;
                }
            }
            if (lwr > upr) {
                break;
            }
            idx = (lwr + upr) >> (int)((int32)1);
            goto _5;
_5:;
        }
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0) {
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(idx);
            (@unsafe.Pointer.To<int32>(pRes)).Value = c1;
            rc = SQLITE_OK;
            goto moveto_index_finish;
        }
        if (lwr >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
            chldPg = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8)));
        } else {
            chldPg = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * lwr)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * lwr) + 1)))))));
        }
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
        v3 = pCur + 1;
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)) & ~(libc.Int32FromInt32(BTCF_ValidNKey) | libc.Int32FromInt32(BTCF_ValidOvfl)));
        if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage >= libc.Int32FromInt32(BTCURSOR_MAX_DEPTH) - libc.Int32FromInt32(1)) {
            return _sqlite3CorruptError(tls, (int32)79421);
        }
        (@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pCur + 88 + (uintptr)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage * 2))).Value = libc.Uint16FromInt32(lwr);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage * 8))).Value = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = (uint16)0;
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage = (Ti8)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage + 1);
        rc = _getAndInitPage(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt, chldPg, (@unsafe.Pointer)(pCur + 136), libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurPagerFlags));
        if (rc == SQLITE_OK && (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FnCell) < (int32)1 || libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FintKey) != libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurIntKey))) {
            _releasePage(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage);
            rc = _sqlite3CorruptError(tls, (int32)79432);
        }
        if (rc != 0) {
            v3 = pCur + 84;
            (@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v3)).Value = (Ti8)(~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v3) - 1);
            v10 = ~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v3);
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)v10 * 8));
            break;
        }
        goto _4;
_4:;
    }
    goto moveto_index_finish;
moveto_index_finish:;
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeInsert(ж<libc.TLS> tls, uintptr pCur, @unsafe.Pointer pX, int32 flags, int32 seekResult) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        int32 idx = default;
        ref var loc = ref heap(new int32(), out var Ꮡloc);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        ref var szNew = ref heap(new int32(), out var ᏑszNew);
        uintptr newCell = default;
        uintptr oldCell = default;
        uintptr p = default;
        uintptr pPage = default;
        uintptr v1 = default;
        TPgno ovfl = default;
        ref var x2 = ref heap(new TBtreePayload(), out var Ꮡx2);
        Tu16 v2 = default;
        TCellInfo _ᴛ1 = default;
        TCellInfo _ᴛ2 = default;
        TUnpackedRecord _ᴛ3 = new();
        _ = idx;
        _ = loc;
        _ = newCell;
        _ = oldCell;
        _ = ovfl;
        _ = p;
        _ = pPage;
        _ = rc;
        _ = szNew;
        _ = x2;
        _ = v1;
        _ = v2;
        loc = seekResult;
        szNew = 0;
        p = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBtree;
        newCell = (uintptr)0;
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_Multiple) != 0) {
            rc = _saveAllCursors(tls, (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot, pCur);
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
            if (loc != 0 && (int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage < 0) {
                r = _sqlite3CorruptError(tls, (int32)82625); goto ᒐdone;
            }
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
            rc = _moveToRoot(tls, pCur);
            if (rc != 0 && rc != (int32)SQLITE_EMPTY) {
                r = rc; goto ᒐdone;
            }
        }
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo == (uintptr)0) {
            if ((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FhasIncrblobCur != 0) {
                _invalidateIncrblobCursors(ref (tls).DerefOrNull(), p, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot, ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8)), 0);
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_ValidNKey) != 0 && ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8)) == (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey) {
                if (libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize) != 0 && (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnPayload == libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 36))) + libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 40)))) {
                    r = _btreeOverwriteCell(tls, pCur, pX); goto ᒐdone;
                }
            } else {
                if (loc == 0) {
                    rc = _sqlite3BtreeTableMoveto(tls, pCur, ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8)), libc.BoolInt32((flags & (int32)BTREE_APPEND) != 0), @unsafe.Pointer.FromPinnedBox(Ꮡloc));
                    if (rc != 0) {
                        r = rc; goto ᒐdone;
                    }
                }
            }
        } else {
            if (loc == 0 && (flags & (int32)BTREE_SAVEPOSITION) == 0) {
                if (~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pX, 32)) != 0) {
                    (@unsafe.Pointer.To<TUnpackedRecord>(__ccgo_ptr(bp))).Value.FpKeyInfo = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo;
                    (@unsafe.Pointer.To<TUnpackedRecord>(__ccgo_ptr(bp))).Value.FaMem = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 24));
                    (@unsafe.Pointer.To<TUnpackedRecord>(__ccgo_ptr(bp))).Value.FnField = ~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pX, 32));
                    (@unsafe.Pointer.To<TUnpackedRecord>(__ccgo_ptr(bp))).Value.Fdefault_rc = 0;
                    (@unsafe.Pointer.To<TUnpackedRecord>(__ccgo_ptr(bp))).Value.FeqSeen = (uint8)0;
                    rc = _sqlite3BtreeIndexMoveto(tls, pCur, bp, @unsafe.Pointer.FromPinnedBox(Ꮡloc));
                } else {
                    rc = _btreeMoveto(tls, pCur, ~@unsafe.Pointer.To<uintptr>(pX), ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8)), libc.BoolInt32((flags & (int32)BTREE_APPEND) != 0), @unsafe.Pointer.FromPinnedBox(Ꮡloc));
                }
                if (rc != 0) {
                    r = rc; goto ᒐdone;
                }
            }
            if (loc == 0) {
                _getCellInfo(tls, pCur);
                if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey == ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8))) {
                    x2.FpData = ~@unsafe.Pointer.To<uintptr>(pX);
                    x2.FnData = (int32)(~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8)));
                    x2.FnZero = 0;
                    r = _btreeOverwriteCell(tls, pCur, @unsafe.Pointer.FromPinnedBox(Ꮡx2)); goto ᒐdone;
                }
            }
        }
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree < 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) > (int32)CURSOR_INVALID) {
                rc = _sqlite3CorruptError(tls, (int32)82748);
            } else {
                rc = _btreeComputeFreeSpace(tls, pPage);
            }
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
        }
        newCell = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FpTmpSpace;
        if ((flags & (int32)BTREE_PREFORMAT) != 0) {
            rc = SQLITE_OK;
            szNew = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FnPreformatSize;
            if (szNew < (int32)4) {
                szNew = (int32)4;
                (@unsafe.Pointer.To<uint8>(__ccgo_ptr(newCell + 3))).Value = (uint8)0;
            }
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FautoVacuum != 0 && szNew > libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaxLocal)) {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, newCell, bp + 40);
                if ((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 40))).Value.FnPayload != (uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 40))).Value.FnLocal) {
                    ovfl = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(newCell + (uintptr)(szNew - (int32)4)));
                    _ptrmapPut(tls, (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt, ovfl, (uint8)PTRMAP_OVERFLOW1, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    if (rc != 0) {
                        goto end_insert;
                    }
                }
            }
        } else {
            rc = _fillInCell(tls, pPage, newCell, pX, @unsafe.Pointer.FromPinnedBox(ᏑszNew));
            if (rc != 0) {
                goto end_insert;
            }
        }
        idx = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix);
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
        if (loc == 0) {
            if (idx >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
                r = _sqlite3CorruptError(tls, (int32)82790); goto ᒐdone;
            }
            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
            if (rc != 0) {
                goto end_insert;
            }
            oldCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx) + 1)))));
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
                libc.Xmemcpy(tls, newCell, oldCell, (uint64)4);
            }
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, oldCell, bp + 64);
            if ((uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 64))).Value.FnLocal != (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 64))).Value.FnPayload) {
                rc = _clearCellOverflow(tls, pPage, oldCell, __ccgo_ptr(bp + 64));
            } else {
                rc = SQLITE_OK;
            }
            v1 = pCur + 1;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(BTCF_ValidOvfl));
            if (libc.Int32FromUint16((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 64))).Value.FnSize) == szNew && (uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 64))).Value.FnLocal == (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 64))).Value.FnPayload && (!((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FautoVacuum != 0) || szNew < libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FminLocal))) {
                if (oldCell < (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset + (uintptr)10) {
                    r = _sqlite3CorruptError(tls, (int32)82817); goto ᒐdone;
                }
                if (oldCell + (uintptr)szNew > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
                    r = _sqlite3CorruptError(tls, (int32)82820); goto ᒐdone;
                }
                libc.Xmemcpy(tls, oldCell, newCell, libc.Uint64FromInt32(szNew));
                r = SQLITE_OK; goto ᒐdone;
            }
            _dropCell(tls, pPage, idx, libc.Int32FromUint16((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp + 64))).Value.FnSize), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            if (rc != 0) {
                goto end_insert;
            }
        } else {
            if (loc < 0 && libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) > 0) {
                v1 = pCur + 86;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1) + 1);
                v2 = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1);
                idx = libc.Int32FromUint16(v2);
                v1 = pCur + 1;
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTCF_ValidNKey) | libc.Int32FromInt32(BTCF_ValidOvfl)));
            } else {
            }
        }
        rc = _insertCellFast(tls, pPage, idx, newCell, szNew);
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnOverflow != 0) {
            v1 = pCur + 1;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTCF_ValidNKey) | libc.Int32FromInt32(BTCF_ValidOvfl)));
            rc = _balance(tls, pCur);
            (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FnOverflow = (uint8)0;
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
            if ((flags & (int32)BTREE_SAVEPOSITION) != 0 && rc == SQLITE_OK) {
                _btreeReleaseAllCursorPages(tls, pCur);
                if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo != 0) {
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey = _sqlite3Malloc(tls, libc.Uint64FromInt64(~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8))));
                    if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                    } else {
                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey, ~@unsafe.Pointer.To<uintptr>(pX), libc.Uint64FromInt64(~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8))));
                    }
                }
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_REQUIRESEEK;
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey = ~@unsafe.Pointer.To<Tsqlite3_int64>(@unsafe.Add(pX, 8));
            }
        }
        goto end_insert;
end_insert:;
        r = rc; goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3BtreeTransferRow(ж<libc.TLS> tls, uintptr pDest, uintptr pSrc, Ti64 iKey) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aIn = default;
        uintptr aOut = default;
        uintptr pBt = default;
        ref var pNew = ref heap(new uintptr(), out var ᏑpNew);
        uintptr pPageOut = default;
        uintptr pPgnoOut = default;
        uintptr pSrcPager = default;
        uintptr v1 = default;
        int32 nCopy = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        Tu32 nIn = default;
        Tu32 nOut = default;
        Tu32 nRem = default;
        TPgno ovflIn = default;
        ref var pgnoNew = ref heap(new TPgno(), out var ᏑpgnoNew);
        uint32 v2 = default;
        uintptr _ᴛ1 = default;
        _ = aIn;
        _ = aOut;
        _ = nCopy;
        _ = nIn;
        _ = nOut;
        _ = nRem;
        _ = ovflIn;
        _ = pBt;
        _ = pNew;
        _ = pPageOut;
        _ = pPgnoOut;
        _ = pSrcPager;
        _ = pgnoNew;
        _ = rc;
        _ = v1;
        _ = v2;
        pBt = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pDest)).Value.FpBt;
        aOut = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace;
        _getCellInfo(tls, pSrc);
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnPayload < (uint32)0x80) {
            v1 = aOut;
            aOut = aOut + 1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))).Value = (uint8)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnPayload;
        } else {
            aOut = aOut + (uintptr)_sqlite3PutVarint(tls, (@unsafe.Pointer)aOut, (uint64)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnPayload);
        }
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pDest)).Value.FpKeyInfo == (uintptr)0) {
            aOut = aOut + (uintptr)_sqlite3PutVarint(tls, (@unsafe.Pointer)aOut, libc.Uint64FromInt64(iKey));
        }
        nIn = (uint32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnLocal;
        aIn = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FpPayload;
        if (aIn + (uintptr)nIn > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.FpPage)).Value.FaDataEnd) {
            r = _sqlite3CorruptError(tls, (int32)82922); goto ᒐdone;
        }
        nRem = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnPayload;
        if (nIn == nRem && nIn < (uint32)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pDest)).Value.FpPage)).Value.FmaxLocal) {
            libc.Xmemcpy(tls, aOut, aIn, (uint64)nIn);
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPreformatSize = libc.Int32FromUint32(nIn + libc.Uint32FromInt32((int32)((int64)aOut - (int64)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace)));
            r = SQLITE_OK; goto ᒐdone;
        } else {
            rc = SQLITE_OK;
            pSrcPager = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.FpBt)).Value.FpPager;
            pPgnoOut = (uintptr)0;
            ovflIn = (uint32)0;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
            pPageOut = (uintptr)0;
            nOut = libc.Uint32FromInt32(_btreePayloadToLocal(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pDest)).Value.FpPage, libc.Int64FromUint32((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnPayload)));
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPreformatSize = libc.Int32FromUint32(nOut) + (int32)((int64)aOut - (int64)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace);
            if (nOut < (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FnPayload) {
                pPgnoOut = aOut + (uintptr)nOut;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBt + 144))).Value += (int32)4;
            }
            if (nRem > nIn) {
                if (aIn + (uintptr)nIn + (uintptr)4 > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.FpPage)).Value.FaDataEnd) {
                    r = _sqlite3CorruptError(tls, (int32)82947); goto ᒐdone;
                }
                ovflIn = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.Finfo.FpPayload + (uintptr)nIn));
            }
            for (var cond = true; cond; cond = nRem > (uint32)0 && rc == SQLITE_OK) {
                nRem = nRem - nOut;
                for (var condΔ1 = true; condΔ1; condΔ1 = rc == SQLITE_OK && nOut > (uint32)0) {
                    if (nIn > (uint32)0) {
                        if (nOut < nIn) {
                            v2 = nOut;
                        } else {
                            v2 = nIn;
                        }
                        nCopy = libc.Int32FromUint32(v2);
                        libc.Xmemcpy(tls, aOut, aIn, libc.Uint64FromInt32(nCopy));
                        nOut = nOut - libc.Uint32FromInt32(nCopy);
                        nIn = nIn - libc.Uint32FromInt32(nCopy);
                        aOut = aOut + (uintptr)nCopy;
                        aIn = aIn + (uintptr)nCopy;
                    }
                    if (nOut > (uint32)0) {
                        _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
                        rc = _sqlite3PagerGet(tls, pSrcPager, ovflIn, bp, (int32)PAGER_GET_READONLY);
                        if (rc == SQLITE_OK) {
                            aIn = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                            ovflIn = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aIn);
                            aIn = aIn + (uintptr)4;
                            nIn = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pSrc)).Value.FpBt)).Value.FusableSize - (uint32)4;
                        }
                    }
                }
                if (rc == SQLITE_OK && nRem > (uint32)0 && pPgnoOut != 0) {
                    pgnoNew = (uint32)0;
                    pNew = (uintptr)0;
                    rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpNew), @unsafe.Pointer.FromPinnedBox(ᏑpgnoNew), (uint32)0, (uint8)0);
                    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPgnoOut, pgnoNew);
                    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0 && pPageOut != 0) {
                        _ptrmapPut(tls, pBt, pgnoNew, (uint8)PTRMAP_OVERFLOW2, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPageOut)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    }
                    _releasePage(tls, pPageOut);
                    pPageOut = pNew;
                    if (pPageOut != 0) {
                        pPgnoOut = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPageOut)).Value.FaData;
                        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPgnoOut, (uint32)0);
                        aOut = pPgnoOut + 4;
                        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)4 < nRem) {
                            v2 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)4;
                        } else {
                            v2 = nRem;
                        }
                        nOut = v2;
                    }
                }
            }
            _releasePage(tls, pPageOut);
            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            r = rc; goto ᒐdone;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3ExprNullRegisterRange(ж<libc.TLS> tls, uintptr pParse, int32 iReg, int32 nReg) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        Tu8 okConstFactor = default;
        TExpr _ᴛ1 = new();
        _ = okConstFactor;
        okConstFactor = libc.Uint8FromInt32((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x80) >> 7)));
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp))).Value.Fop = (uint8)TK_NULLS;
        (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp)).of(TExpr.ᏑFy)), 0))).Value = nReg;
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 7, 0x80);
        _sqlite3ExprCodeRunJustOnce(tls, pParse, bp, iReg);
        libc.SetBitFieldPtr16Uint32(pParse + 40, (uint32)okConstFactor, 7, 0x80);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3SelectCheckOnClauses(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        int32 ii = default;
        int32 jj = default;
        uintptr pItem = default;
        uintptr pItem2 = default;
        TCheckOnCtx _ᴛ1 = default;
        TWalker _ᴛ2 = new();
        _ = ii;
        _ = jj;
        _ = pItem;
        _ = pItem2;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_selectCheckOnClausesExpr);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_selectCheckOnClausesSelect);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = bp + 48;
        libc.Xmemset(tls, bp + 48, 0, (uint64)24);
        (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FpSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc;
        (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FiJoin = -(int32)1;
        _sqlite3WalkExpr(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWhere);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSelect + 4))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(SF_OnToWhere));
        (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FbFuncArg = (int32)1;
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc)).Value.FnSrc)) {
                break;
            }
            pItem = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8 + (uintptr)ii * 80;
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x8) >> 3) != 0) {
                (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FiJoin = -(int32)1;
                if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0) {
                    (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FiJoin = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor;
                } else {
                    jj = ii + (int32)1;
                    for (;;) {
                        if (!(jj < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc)).Value.FnSrc)) {
                            break;
                        }
                        pItem2 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8 + (uintptr)jj * 80;
                        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem2)).Value.Ffg.Fjointype) & (int32)JT_OUTER) != 0) {
                            (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FiJoin = (@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(pItem2 + (uintptr)(-libc.Int32FromInt32(1)) * 80))).Value.FiCursor;
                            break;
                        }
                        goto _2;
_2:;
                        jj = jj + 1;
                    }
                }
                if ((@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp + 48))).Value.FiJoin >= 0) {
                    _sqlite3WalkExprList(tls, bp, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48)));
                }
            }
            goto _1;
_1:;
            ii = ii + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _sqlite3SrcListDup(ж<libc.TLS> tls, uintptr db, uintptr p, int32 flags) {
    int32 i = default;
    uintptr pNew = default;
    uintptr pNewItem = default;
    uintptr pNewSubq = default;
    uintptr pOldItem = default;
    uintptr pTab = default;
    uintptr v3 = default;
    Tu32 v1 = default;
    _ = i;
    _ = pNew;
    _ = pNewItem;
    _ = pNewSubq;
    _ = pOldItem;
    _ = pTab;
    _ = v1;
    _ = v3;
    if (p == (uintptr)0) {
        return (uintptr)0;
    }
    pNew = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)(libc.UintptrFromInt32(0) + 8) + libc.Uint64FromInt32((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p)).Value.FnSrc) * libc.Uint64FromInt64(80)));
    if (pNew == (uintptr)0) {
        return (uintptr)0;
    }
    v1 = libc.Uint32FromInt32((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p)).Value.FnSrc);
    (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pNew)).Value.FnAlloc = v1;
    (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pNew)).Value.FnSrc = libc.Int32FromUint32(v1);
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p)).Value.FnSrc)) {
            break;
        }
        pNewItem = pNew + 8 + (uintptr)i * 80;
        pOldItem = p + 8 + (uintptr)i * 80;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.Ffg = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.Ffg;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pOldItem + 24 + 4)) & 0x4) >> 2) != 0) {
            pNewSubq = _sqlite3DbMallocRaw(tls, db, (uint64)24);
            if (pNewSubq == (uintptr)0) {
                libc.SetBitFieldPtr32Uint32(pNewItem + 24 + 4, libc.Uint32FromInt32(0), 2, 0x4);
            } else {
                libc.Xmemcpy(tls, pNewSubq, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 72)), (uint64)24);
                (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pNewSubq)).Value.FpSelect = _sqlite3SelectDup(tls, db, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pNewSubq)).Value.FpSelect, flags);
                if ((@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pNewSubq)).Value.FpSelect == (uintptr)0) {
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pNewSubq);
                    pNewSubq = (uintptr)0;
                    libc.SetBitFieldPtr32Uint32(pNewItem + 24 + 4, libc.Uint32FromInt32(0), 2, 0x4);
                }
            }
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 72))).Value = pNewSubq;
        } else {
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pOldItem + 24 + 4)) & 0x10000) >> 16) != 0) {
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 72))).Value = (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 72))).Value;
            } else {
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 72))).Value = _sqlite3DbStrDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 72)));
            }
        }
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.FzName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.FzName);
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.FzAlias = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.FzAlias);
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.FiCursor = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.FiCursor;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pNewItem + 24 + 4)) & 0x2) >> 1) != 0) {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 48))).Value = _sqlite3DbStrDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 48)));
        } else {
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pNewItem + 24 + 4)) & 0x8) >> 3) != 0) {
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 48))).Value = _sqlite3ExprListDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 48)), flags);
            } else {
                (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem).of(TSrcItem.ᏑFu1).Reinterpret<TSrcItemᴛ1_Fu1, Tu32>()).Value = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem).of(TSrcItem.ᏑFu1).Reinterpret<TSrcItemᴛ1_Fu1, Tu32>()).Value;
            }
        }
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.Fu2 = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.Fu2;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pNewItem + 24 + 4)) & 0x200) >> 9) != 0) {
            (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 56))))).Value.FnUse = (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 56))))).Value.FnUse + 1;
        }
        v3 = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.FpSTab;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.FpSTab = v3;
        pTab = v3;
        if (pTab != 0) {
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef + 1;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pOldItem + 24 + 4)) & 0x800) >> 11) != 0) {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 64))).Value = _sqlite3IdListDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 64)));
        } else {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNewItem + 64))).Value = _sqlite3ExprDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOldItem + 64)), flags);
        }
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pNewItem)).Value.FcolUsed = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pOldItem)).Value.FcolUsed;
        goto _2;
_2:;
        i = i + 1;
    }
    return pNew;
}

internal static void _sqlite3VdbeMakeReady(ж<libc.TLS> tls, uintptr p, uintptr pParse) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr v1 = default;
        int32 n = default;
        ref var nArg = ref heap(new int32(), out var ᏑnArg);
        int32 nCursor = default;
        int32 nMem = default;
        int32 nVar = default;
        TReusableSpace _ᴛ1 = default;
        _ = db;
        _ = n;
        _ = nArg;
        _ = nCursor;
        _ = nMem;
        _ = nVar;
        _ = v1;
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpVList = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVList;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVList = (uintptr)0;
        db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
        nVar = (int32)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnVar;
        nMem = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem;
        nCursor = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
        nArg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMaxArg;
        nMem = nMem + nCursor;
        if (nCursor == 0 && nMem > 0) {
            nMem = nMem + 1;
        }
        n = libc.Int32FromUint64(libc.Uint64FromInt64(24) * libc.Uint64FromInt32((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp));
        (@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FpSpace = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)n;
        (@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FnFree = (int64)(((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FszOpAlloc - n) & ~libc.Int32FromInt32(7));
        _resolveP2Values(tls, p, @unsafe.Pointer.FromPinnedBox(ᏑnArg));
        libc.SetBitFieldPtr16Uint32(p + 200, (uint32)libc.BoolUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FisMultiWrite != 0 && (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x2) >> 1)) != 0), 5, 0x20);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fexplain != 0) {
            if (nMem < (int32)10) {
                nMem = (int32)10;
            }
            libc.SetBitFieldPtr16Uint32(p + 200, (uint32)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fexplain, 2, 0xc);
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnResColumn = libc.Uint16FromInt32((int32)12 - (int32)4 * (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)));
        }
        libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(0), 0, 0x3);
        (@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FnNeeded = 0;
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem = _allocSpace(ref (tls).DerefOrNull(), bp, (uintptr)0, libc.Int64FromUint64(libc.Uint64FromInt32(nMem) * (uint64)56));
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar = _allocSpace(ref (tls).DerefOrNull(), bp, (uintptr)0, libc.Int64FromUint64(libc.Uint64FromInt32(nVar) * (uint64)56));
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapArg = _allocSpace(ref (tls).DerefOrNull(), bp, (uintptr)0, libc.Int64FromUint64(libc.Uint64FromInt32(nArg) * (uint64)8));
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr = _allocSpace(ref (tls).DerefOrNull(), bp, (uintptr)0, libc.Int64FromUint64(libc.Uint64FromInt32(nCursor) * (uint64)8));
        if ((@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FnNeeded != 0) {
            v1 = _sqlite3DbMallocRawNN(tls, db, libc.Uint64FromInt64((@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FnNeeded));
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpFree = v1;
            (@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FpSpace = v1;
            (@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FnFree = (@unsafe.Pointer.To<TReusableSpace>(__ccgo_ptr(bp))).Value.FnNeeded;
            if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem = _allocSpace(ref (tls).DerefOrNull(), bp, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem, libc.Int64FromUint64(libc.Uint64FromInt32(nMem) * (uint64)56));
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar = _allocSpace(ref (tls).DerefOrNull(), bp, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar, libc.Int64FromUint64(libc.Uint64FromInt32(nVar) * (uint64)56));
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapArg = _allocSpace(ref (tls).DerefOrNull(), bp, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapArg, libc.Int64FromUint64(libc.Uint64FromInt32(nArg) * (uint64)8));
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr = _allocSpace(ref (tls).DerefOrNull(), bp, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr, libc.Int64FromUint64(libc.Uint64FromInt32(nCursor) * (uint64)8));
            }
        }
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnVar = 0;
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnCursor = 0;
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnMem = 0;
        } else {
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnCursor = nCursor;
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnVar = (int16)nVar;
            _initMemArray(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar, nVar, db, (uint16)MEM_Null);
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnMem = nMem;
            _initMemArray(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem, nMem, db, (uint16)MEM_Undefined);
            libc.Xmemset(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr, 0, libc.Uint64FromInt32(nCursor) * (uint64)8);
        }
        _sqlite3VdbeRewind(ref (tls).DerefOrNull(), p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _statDecodePage(ж<libc.TLS> tls, uintptr pBt, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aData = default;
        uintptr aHdr = default;
        uintptr pCell = default;
        ref var dummy = ref heap(new Tu64(), out var Ꮡdummy);
        int32 i = default;
        int32 iNext = default;
        int32 iOff = default;
        int32 isLeaf = default;
        int32 j = default;
        int32 nHdr = default;
        int32 nLocal = default;
        int32 nOvfl = default;
        int32 nUnused = default;
        int32 nUsable = default;
        int32 rc = default;
        int32 szPage = default;
        int32 v1 = default;
        Tu32 iPrev = default;
        ref var nPayload = ref heap(new Tu32(), out var ᏑnPayload);
        uint32 v2 = default;
        uintptr _ᴛ1 = default;
        _ = aData;
        _ = aHdr;
        _ = dummy;
        _ = i;
        _ = iNext;
        _ = iOff;
        _ = iPrev;
        _ = isLeaf;
        _ = j;
        _ = nHdr;
        _ = nLocal;
        _ = nOvfl;
        _ = nPayload;
        _ = nUnused;
        _ = nUsable;
        _ = pCell;
        _ = rc;
        _ = szPage;
        _ = v1;
        _ = v2;
        aData = (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaPg;
        if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FiPgno == (uint32)1) {
            v1 = (int32)100;
        } else {
            v1 = 0;
        }
        aHdr = aData + (uintptr)v1;
        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr));
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags) == (int32)0x0A || libc.Int32FromUint8((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags) == (int32)0x0D) {
            isLeaf = (int32)1;
            nHdr = (int32)8;
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags) == (int32)0x05 || libc.Int32FromUint8((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags) == (int32)0x02) {
                isLeaf = 0;
                nHdr = (int32)12;
            } else {
                goto statPageIsCorrupt;
            }
        }
        if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FiPgno == (uint32)1) {
            nHdr = nHdr + (int32)100;
        }
        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 3))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 3 + 1)));
        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnMxPayload = 0;
        szPage = _sqlite3BtreeGetPageSize(ref (tls).DerefOrNull(), pBt);
        nUnused = (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 5))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 5 + 1)))) - nHdr - (int32)2 * (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell;
        nUnused = nUnused + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 7)));
        iOff = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 1))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aHdr + 1 + 1)));
        while (iOff != 0) {
            if (iOff >= szPage) {
                goto statPageIsCorrupt;
            }
            nUnused = nUnused + (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(iOff + (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(iOff + (int32)2) + 1))));
            iNext = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)iOff))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)iOff + 1)));
            if (iNext < iOff + (int32)4 && iNext > 0) {
                goto statPageIsCorrupt;
            }
            iOff = iNext;
        }
        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnUnused = nUnused;
        if (isLeaf != 0) {
            v2 = (uint32)0;
        } else {
            v2 = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aHdr + 8));
        }
        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FiRightChildPg = v2;
        if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell != 0) {
            _sqlite3BtreeEnter(tls, pBt);
            nUsable = szPage - _sqlite3BtreeGetReserveNoMutex(ref (tls).DerefOrNull(), pBt);
            _sqlite3BtreeLeave(tls, pBt);
            (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell = Xsqlite3_malloc64(tls, (uint64)(libc.Uint64FromInt32((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell + libc.Int32FromInt32(1)) * (uint64)32));
            if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell == (uintptr)0) {
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
            libc.Xmemset(tls, (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell, 0, libc.Uint64FromInt32((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell + libc.Int32FromInt32(1)) * (uint64)32);
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnCell)) {
                    break;
                }
                pCell = (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FaCell + (uintptr)i * 32;
                iOff = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(nHdr + i * (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(nHdr + i * (int32)2) + 1)));
                if (iOff < nHdr || iOff >= szPage) {
                    goto statPageIsCorrupt;
                }
                if (!(isLeaf != 0)) {
                    (@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FiChildPg = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + (uintptr)iOff));
                    iOff = iOff + (int32)4;
                }
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags) == (int32)0x05) {
                } else {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)iOff))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
                        nPayload = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)iOff)));
                        v1 = libc.Int32FromInt32(1);
                    } else {
                        v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref (tls).DerefOrNull(), aData + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(ᏑnPayload)));
                    }
                    iOff = iOff + libc.Int32FromUint8(libc.Uint8FromInt32(v1));
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags) == (int32)0x0D) {
                        iOff = iOff + libc.Int32FromUint8(_sqlite3GetVarint(ref (tls).DerefOrNull(), aData + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(Ꮡdummy)));
                    }
                    if (nPayload > libc.Uint32FromInt32((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnMxPayload)) {
                        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.FnMxPayload = libc.Int32FromUint32(nPayload);
                    }
                    nLocal = _getLocalPayload(ref (tls).DerefOrNull(), nUsable, (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags, libc.Int32FromUint32(nPayload));
                    if (nLocal < 0) {
                        goto statPageIsCorrupt;
                    }
                    (@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FnLocal = nLocal;
                    if (nPayload > libc.Uint32FromInt32(nLocal)) {
                        nOvfl = libc.Int32FromUint32((nPayload - libc.Uint32FromInt32(nLocal) + libc.Uint32FromInt32(nUsable) - (uint32)4 - (uint32)1) / libc.Uint32FromInt32(nUsable - libc.Int32FromInt32(4)));
                        if (iOff + nLocal + (int32)4 > nUsable || nPayload > (uint32)0x7fffffff) {
                            goto statPageIsCorrupt;
                        }
                        (@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FnLastOvfl = libc.Int32FromUint32(nPayload - libc.Uint32FromInt32(nLocal) - libc.Uint32FromInt32((nOvfl - (int32)1) * (nUsable - (int32)4)));
                        (@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FnOvfl = nOvfl;
                        (@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FaOvfl = Xsqlite3_malloc64(tls, (uint64)((uint64)4 * libc.Uint64FromInt32(nOvfl)));
                        if ((@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FaOvfl == (uintptr)0) {
                            r = (int32)SQLITE_NOMEM; goto ᒐdone;
                        }
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FaOvfl))).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aData + (uintptr)(iOff + nLocal)));
                        j = (int32)1;
                        for (;;) {
                            if (!(j < nOvfl)) {
                                break;
                            }
                            iPrev = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FaOvfl + (uintptr)(j - (int32)1) * 4));
                            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
                            rc = _sqlite3PagerGet(tls, _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt), iPrev, bp, 0);
                            if (rc != SQLITE_OK) {
                                r = rc; goto ᒐdone;
                            }
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TStatCell>((@unsafe.Pointer)pCell)).Value.FaOvfl + (uintptr)j * 4))).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)_sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))));
                            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                            goto _5;
_5:;
                            j = j + 1;
                        }
                    }
                }
                goto _3;
_3:;
                i = i + 1;
            }
        }
        r = SQLITE_OK; goto ᒐdone;
        goto statPageIsCorrupt;
statPageIsCorrupt:;
        (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)p)).Value.Fflags = (uint8)0;
        _statClearCells(tls, p);
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _syncJournal(ж<libc.TLS> tls, uintptr pPager, int32 newHdr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 iDc = default;
        int32 rc = default;
        int32 v1 = default;
        Ti64 iNextHdrOffset = default;
        array<Tu8> _ᴛ1 = new(8);
        array<Tu8> _ᴛ2 = new(12);
        _ = iDc;
        _ = iNextHdrOffset;
        _ = rc;
        _ = v1;
        rc = _sqlite3PagerExclusiveLock(tls, pPager);
        if (rc != SQLITE_OK) {
            r = rc; goto ᒐdone;
        }
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoSync != 0)) {
            if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) != (int32)PAGER_JOURNALMODE_MEMORY) {
                iDc = _sqlite3OsDeviceCharacteristics(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd);
                if (0 == (iDc & (int32)SQLITE_IOCAP_SAFE_APPEND)) {
                    libc.Xmemcpy(tls, bp + 8, (uintptr)Ꮡ_aJournalMagic, (uint64)8);
                    _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 16), libc.Uint32FromInt32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnRec));
                    iNextHdrOffset = _journalHdrOffset(ref (tls).DerefOrNull(), pPager);
                    rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, bp, (int32)8, iNextHdrOffset);
                    if (rc == SQLITE_OK && 0 == libc.Xmemcmp(tls, bp, (uintptr)Ꮡ_aJournalMagic, (uint64)8)) {
                        rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, (uintptr)Ꮡ_zerobyte, (int32)1, iNextHdrOffset);
                    }
                    if (rc != SQLITE_OK && rc != (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))))) {
                        r = rc; goto ᒐdone;
                    }
                    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FfullSync != 0 && 0 == (iDc & (int32)SQLITE_IOCAP_SEQUENTIAL)) {
                        rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsyncFlags));
                        if (rc != SQLITE_OK) {
                            r = rc; goto ᒐdone;
                        }
                    }
                    rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, bp + 8, (int32)12, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr);
                    if (rc != SQLITE_OK) {
                        r = rc; goto ᒐdone;
                    }
                }
                if (0 == (iDc & (int32)SQLITE_IOCAP_SEQUENTIAL)) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsyncFlags) == (int32)SQLITE_SYNC_FULL) {
                        v1 = (int32)SQLITE_SYNC_DATAONLY;
                    } else {
                        v1 = 0;
                    }
                    rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsyncFlags) | v1);
                    if (rc != SQLITE_OK) {
                        r = rc; goto ᒐdone;
                    }
                }
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
                if (newHdr != 0 && 0 == (iDc & (int32)SQLITE_IOCAP_SAFE_APPEND)) {
                    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnRec = 0;
                    rc = _writeJournalHdr(tls, pPager);
                    if (rc != SQLITE_OK) {
                        r = rc; goto ᒐdone;
                    }
                }
            } else {
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
            }
        }
        _sqlite3PcacheClearSyncFlags(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_WRITER_DBMOD;
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _vdbeSorterCompareReal(ж<libc.TLS> tls, uintptr pTask, uintptr pbKey2Cached, uintptr pKey1, int32 nKey1, uintptr pKey2, int32 nKey2) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr p1 = default;
        uintptr p2 = default;
        float64 _ᴛ1 = default;
        float64 _ᴛ2 = default;
        Tu64 _ᴛ3 = default;
        _ = p1;
        _ = p2;
        p1 = pKey1;
        p2 = pKey2;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p1 + 1))) != (int32)7 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p2 + 1))) != (int32)7) {
            r = _vdbeSorterCompareRealInt(tls, pTask, pbKey2Cached, pKey1, nKey1, pKey2, nKey2); goto ᒐdone;
        }
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = _sqlite3Get8byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(p1 + (uintptr)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p1)))));
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = _sqlite3Get8byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(p2 + (uintptr)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p2)))));
        libc.Xmemcpy(tls, bp + 16, bp, (uint64)8);
        r = _vdbeSorterFinishRealCompare(tls, pTask, pbKey2Cached, pKey1, nKey1, pKey2, nKey2, ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 8)), ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 16)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static float64 /*r*/ _vdbeSorterGetReal(ж<libc.TLS> tls, uintptr p) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        TMem _ᴛ1 = new();
        float64 _ᴛ2 = default;
        Tu64 _ᴛ3 = default;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1))) == (int32)7) {
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8))).Value = _sqlite3Get8byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(p + (uintptr)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p)))));
            libc.Xmemcpy(tls, bp, bp + 8, (uint64)8);
        } else {
            (@unsafe.Pointer.To<Ti64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp + 16)).of(TMem.ᏑFu)), 0))).Value = 0;
            _sqlite3VdbeSerialGet(tls, p + (uintptr)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))), (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1))), bp + 16);
            (@unsafe.Pointer.To<float64>(__ccgo_ptr(bp))).Value = (float64)(~@unsafe.Pointer.To<Ti64>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp + 16)).of(TMem.ᏑFu)), 0)));
        }
        r = ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _walEncodeFrame(ж<libc.TLS> tls, uintptr pWal, Tu32 iPage, Tu32 nTruncate, uintptr aData, uintptr aFrame) {
    uintptr aCksum = default;
    int32 nativeCksum = default;
    _ = aCksum;
    _ = nativeCksum;
    aCksum = pWal + 72 + 24;
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aFrame, iPage);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aFrame + 4), nTruncate);
    if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum == (uint32)0) {
        libc.Xmemcpy(tls, aFrame + 8, pWal + 72 + 32, (uint64)8);
        nativeCksum = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FbigEndCksum) == SQLITE_BIGENDIAN);
        _walChecksumBytes(ref (tls).DerefOrNull(), nativeCksum, aFrame, (int32)8, aCksum, (@unsafe.Pointer)aCksum);
        _walChecksumBytes(ref (tls).DerefOrNull(), nativeCksum, aData, libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage), aCksum, (@unsafe.Pointer)aCksum);
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aFrame + 16), ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aCksum)));
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aFrame + 20), ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aCksum + 1 * 4)));
    } else {
        libc.Xmemset(tls, aFrame + 8, 0, (uint64)16);
    }
}

internal static int32 /*r*/ _walFrames(ж<libc.TLS> tls, uintptr pWal, int32 szPage, uintptr pList, TPgno nTruncate, int32 isCommit, int32 sync_flags) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        ref var aCksum = ref heap(new array<Tu32>(2), out var ᏑaCksum);
        int32 bSync = default;
        int32 nDbSize = default;
        int32 nExtra = default;
        int32 rc = default;
        int32 sectorSize = default;
        int32 szFrame = default;
        int32 v1 = default;
        Tu32 iFirst = default;
        Tu32 iFrame = default;
        ref var iWrite = ref heap(new Tu32(), out var ᏑiWrite);
        Ti64 iOff = default;
        Ti64 iOffset = default;
        Ti64 sz = default;
        uintptr p = default;
        uintptr pData = default;
        uintptr pLast = default;
        uintptr pLive = default;
        uintptr v3 = default;
        ref var w = ref heap(new TWalWriter(), out var Ꮡw);
        uint32 v4 = default;
        array<Tu8> _ᴛ1 = new(32);
        _ = aCksum;
        _ = bSync;
        _ = iFirst;
        _ = iFrame;
        _ = iOff;
        _ = iOffset;
        _ = iWrite;
        _ = nDbSize;
        _ = nExtra;
        _ = p;
        _ = pData;
        _ = pLast;
        _ = pLive;
        _ = rc;
        _ = sectorSize;
        _ = sz;
        _ = szFrame;
        _ = w;
        _ = v1;
        _ = v3;
        _ = v4;
        pLast = (uintptr)0;
        nExtra = 0;
        iFirst = (uint32)0;
        pLive = _walIndexHdr(ref (tls).DerefOrNull(), pWal);
        if (libc.Xmemcmp(tls, pWal + 72, pLive, (uint64)48) != 0) {
            iFirst = (@unsafe.Pointer.To<TWalIndexHdr>((@unsafe.Pointer)pLive)).Value.FmxFrame + (uint32)1;
        }
        v1 = _walRestartLog(tls, pWal);
        rc = v1;
        if (SQLITE_OK != v1) {
            r = rc; goto ᒐdone;
        }
        iFrame = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
        if (iFrame == (uint32)0) {
            _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp), libc.Uint32FromInt32(libc.Int32FromInt32(WAL_MAGIC) | libc.Int32FromInt32(SQLITE_BIGENDIAN)));
            _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 4), (uint32)WAL_MAX_VERSION);
            _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 8), libc.Uint32FromInt32(szPage));
            _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 12), (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt);
            if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt == (uint32)0) {
                Xsqlite3_randomness(tls, (int32)8, pWal + 72 + 32);
            }
            libc.Xmemcpy(tls, bp + 16, pWal + 72 + 32, (uint64)8);
            _walChecksumBytes(ref (tls).DerefOrNull(), (int32)1, bp, libc.Int32FromInt32(WAL_HDRSIZE) - libc.Int32FromInt32(2) * libc.Int32FromInt32(4), (uintptr)0, @unsafe.Pointer.FromPinnedBox(ᏑaCksum));
            _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 24), aCksum[0]);
            _sqlite3Put4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 28), aCksum[(int32)1]);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage = libc.Uint32FromInt32(szPage);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FbigEndCksum = (uint8)SQLITE_BIGENDIAN;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24))).Value = aCksum[0];
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4))).Value = aCksum[(int32)1];
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FtruncateOnCommit = (uint8)1;
            rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp, (int32)32, 0);
            if (rc != SQLITE_OK) {
                r = rc; goto ᒐdone;
            }
            if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FsyncHeader != 0) {
                rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, sync_flags >> (int)((int32)2) & (int32)0x03);
                if (rc != 0) {
                    r = rc; goto ᒐdone;
                }
            }
        }
        if (libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage) != szPage) {
            r = _sqlite3CorruptError(tls, (int32)71390); goto ᒐdone;
        }
        w.FpWal = pWal;
        w.FpFd = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd;
        w.FiSyncPoint = 0;
        w.FsyncFlags = sync_flags;
        w.FszPage = szPage;
        iOffset = libc.Int64FromInt32(WAL_HDRSIZE) + libc.Int64FromUint32(iFrame + libc.Uint32FromInt32(1) - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE));
        szFrame = szPage + (int32)WAL_FRAME_HDRSIZE;
        p = pList;
        for (;;) {
            if (!(p != 0)) {
                break;
            }
            if (iFirst != 0 && ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty != 0 || isCommit == 0)) {
                iWrite = (uint32)0;
                _walFindFrame(tls, pWal, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(ᏑiWrite));
                if (iWrite >= iFirst) {
                    iOff = (int64)WAL_HDRSIZE + libc.Int64FromUint32(iWrite - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE)) + (int64)WAL_FRAME_HDRSIZE;
                    if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum == (uint32)0 || iWrite < (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum) {
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum = iWrite;
                    }
                    pData = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpData;
                    rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, pData, szPage, iOff);
                    if (rc != 0) {
                        r = rc; goto ᒐdone;
                    }
                    v3 = p + 52;
                    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)) & ~libc.Int32FromInt32(PGHDR_WAL_APPEND));
                    goto _2;
                }
            }
            iFrame = iFrame + 1;
            if (isCommit != 0 && (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty == (uintptr)0) {
                v4 = nTruncate;
            } else {
                v4 = (uint32)0;
            }
            nDbSize = libc.Int32FromUint32(v4);
            rc = _walWriteOneFrame(tls, @unsafe.Pointer.FromPinnedBox(Ꮡw), p, nDbSize, iOffset);
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
            pLast = p;
            iOffset = iOffset + (int64)szFrame;
            v3 = p + 52;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)) | libc.Int32FromInt32(PGHDR_WAL_APPEND));
            goto _2;
_2:;
            p = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty;
        }
        if (isCommit != 0 && (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum != 0) {
            rc = _walRewriteChecksums(tls, pWal, iFrame);
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
        }
        if (isCommit != 0 && (sync_flags & (int32)0x03) != 0) {
            bSync = (int32)1;
            if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpadToSectorBoundary != 0) {
                sectorSize = _sqlite3SectorSize(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd);
                w.FiSyncPoint = quo((iOffset + (int64)sectorSize - (int64)1), (int64)sectorSize) * (int64)sectorSize;
                bSync = libc.BoolInt32(w.FiSyncPoint == iOffset);
                while (iOffset < w.FiSyncPoint) {
                    rc = _walWriteOneFrame(tls, @unsafe.Pointer.FromPinnedBox(Ꮡw), pLast, libc.Int32FromUint32(nTruncate), iOffset);
                    if (rc != 0) {
                        r = rc; goto ᒐdone;
                    }
                    iOffset = iOffset + (int64)szFrame;
                    nExtra = nExtra + 1;
                }
            }
            if (bSync != 0) {
                rc = _sqlite3OsSync(tls, w.FpFd, sync_flags & (int32)0x03);
            }
        }
        if (isCommit != 0 && (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FtruncateOnCommit != 0 && (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FmxWalSize >= 0) {
            sz = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FmxWalSize;
            if ((int64)WAL_HDRSIZE + libc.Int64FromUint32(iFrame + libc.Uint32FromInt32(nExtra) + libc.Uint32FromInt32(1) - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE)) > (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FmxWalSize) {
                sz = libc.Int64FromInt32(WAL_HDRSIZE) + libc.Int64FromUint32(iFrame + libc.Uint32FromInt32(nExtra) + libc.Uint32FromInt32(1) - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE));
            }
            _walLimitSize(tls, pWal, sz);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FtruncateOnCommit = (uint8)0;
        }
        iFrame = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
        p = pList;
        for (;;) {
            if (!(p != 0 && rc == SQLITE_OK)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fflags) & (int32)PGHDR_WAL_APPEND) == 0) {
                goto _6;
            }
            iFrame = iFrame + 1;
            rc = _walIndexAppend(tls, pWal, iFrame, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fpgno);
            goto _6;
_6:;
            p = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty;
        }
        while (rc == SQLITE_OK && nExtra > 0) {
            iFrame = iFrame + 1;
            nExtra = nExtra - 1;
            rc = _walIndexAppend(tls, pWal, iFrame, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pLast)).Value.Fpgno);
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage = libc.Uint16FromInt32(szPage & libc.Int32FromInt32(0xff00) | szPage.Rsh((int64)(libc.Int32FromInt32(16))));
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame = iFrame;
            if (isCommit != 0) {
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FiChange = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FiChange + 1;
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FnPage = nTruncate;
            }
            if (isCommit != 0) {
                _walIndexWriteHdr(tls, pWal);
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiCallback = iFrame;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _walRestartHdr(ж<libc.TLS> tls, uintptr pWal, Tu32 _salt1) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)bp)).Value = _salt1;
        uintptr aSalt = default;
        uintptr pInfo = default;
        int32 i = default;
        _ = aSalt;
        _ = i;
        _ = pInfo;
        pInfo = _walCkptInfo(ref (tls).DerefOrNull(), pWal);
        aSalt = pWal + 72 + 32;
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt + 1;
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame = (uint32)0;
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aSalt, (uint32)1 + _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aSalt));
        libc.Xmemcpy(tls, pWal + 72 + 32 + 1 * 4, bp, (uint64)4);
        _walIndexWriteHdr(tls, pWal);
        libc.AtomicStoreNUint32(pInfo, libc.Uint32FromInt32(libc.Int32FromInt32(0)), libc.Int32FromInt32(__ATOMIC_RELAXED));
        (@unsafe.Pointer.To<TWalCkptInfo>((@unsafe.Pointer)pInfo)).Value.FnBackfillAttempted = (uint32)0;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pInfo + 4 + 1 * 4))).Value = (uint32)0;
        i = (int32)2;
        for (;;) {
            if (!(i < libc.Int32FromInt32(SQLITE_SHM_NLOCK) - libc.Int32FromInt32(3))) {
                break;
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pInfo + 4 + (uintptr)i * 4))).Value = (uint32)READMARK_NOT_USED;
            goto _1;
_1:;
            i = i + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static Tu32 /*r*/ _whereIsCoveringIndex(ж<libc.TLS> tls, uintptr pWInfo, uintptr pIdx, int32 iTabCur) {
    Tu32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 i = default;
        int32 rc = default;
        TCoveringIndexCheck _ᴛ1 = default;
        TWalker _ᴛ2 = new();
        _ = i;
        _ = rc;
        if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect == (uintptr)0) {
            r = (uint32)0; goto ᒐdone;
        }
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x800) >> 11) == 0) {
            i = 0;
            for (;;) {
                if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn))) {
                    break;
                }
                if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2))) >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)) {
                    break;
                }
                goto _1;
_1:;
                i = i + 1;
            }
            if (i >= libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn)) {
                r = (uint32)0; goto ᒐdone;
            }
        }
        (@unsafe.Pointer.To<TCoveringIndexCheck>(__ccgo_ptr(bp))).Value.FpIdx = pIdx;
        (@unsafe.Pointer.To<TCoveringIndexCheck>(__ccgo_ptr(bp))).Value.FiTabCur = iTabCur;
        (@unsafe.Pointer.To<TCoveringIndexCheck>(__ccgo_ptr(bp))).Value.FbExpr = (uint8)0;
        (@unsafe.Pointer.To<TCoveringIndexCheck>(__ccgo_ptr(bp))).Value.FbUnidx = (uint8)0;
        libc.Xmemset(tls, bp + 16, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 16))).Value.FxExprCallback = __ccgo_fp(_whereIsCoveringIndexWalkCallback);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 16))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 16)).of(TWalker.ᏑFu)), 0))).Value = bp;
        _sqlite3WalkSelect(tls, bp + 16, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect);
        if ((@unsafe.Pointer.To<TCoveringIndexCheck>(__ccgo_ptr(bp))).Value.FbUnidx != 0) {
            rc = 0;
        } else {
            if ((@unsafe.Pointer.To<TCoveringIndexCheck>(__ccgo_ptr(bp))).Value.FbExpr != 0) {
                rc = (int32)WHERE_EXPRIDX;
            } else {
                rc = (int32)WHERE_IDX_ONLY;
            }
        }
        r = libc.Uint32FromInt32(rc);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _whereLoopAddBtree(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, TBitmask mPrereq) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(176);
        defer(tls.Free, (nint)176, ref ᒐ);
        int32 b = default;
        int32 iCur = default;
        int32 iSortIdx = default;
        int32 ii = default;
        int32 rc = default;
        int32 v5 = default;
        Tu32 isCov = default;
        ref var m = ref heap(new TBitmask(), out var Ꮡm);
        TLogEst nLookup = default;
        TLogEst rLogSize = default;
        TLogEst rSize = default;
        uintptr pFirst = default;
        uintptr pNew = default;
        uintptr pProbe = default;
        uintptr pSrc = default;
        uintptr pTab = default;
        uintptr pTabList = default;
        uintptr pTerm = default;
        uintptr pTerm1 = default;
        uintptr pWC = default;
        uintptr pWC2 = default;
        uintptr pWCEnd = default;
        uintptr pWInfo = default;
        uintptr v2 = default;
        Ti16 _ᴛ1 = default;
        array<TLogEst> _ᴛ2 = new(2);
        TIndex _ᴛ3 = default;
        _ = b;
        _ = iCur;
        _ = iSortIdx;
        _ = ii;
        _ = isCov;
        _ = m;
        _ = nLookup;
        _ = pFirst;
        _ = pNew;
        _ = pProbe;
        _ = pSrc;
        _ = pTab;
        _ = pTabList;
        _ = pTerm;
        _ = pTerm1;
        _ = pWC;
        _ = pWC2;
        _ = pWCEnd;
        _ = pWInfo;
        _ = rLogSize;
        _ = rSize;
        _ = rc;
        _ = v2;
        _ = v5;
        (@unsafe.Pointer.To<Ti16>(__ccgo_ptr(bp + 164))).Value = (int16)(-(int32)1);
        rc = SQLITE_OK;
        iSortIdx = (int32)1;
        pNew = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
        pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
        pTabList = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList;
        pSrc = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiTab * 80;
        pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab;
        pWC = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8));
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x2) >> 1) != 0) {
            pProbe = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 56));
        } else {
            if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
                pProbe = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
            } else {
                libc.Xmemset(tls, bp, 0, (uint64)160);
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FnKeyCol = (uint16)1;
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FnColumn = (uint16)1;
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FaiColumn = bp + 164;
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FaiRowLogEst = bp + 160;
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FonError = (uint8)OE_Replace;
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FpTable = pTab;
                (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FszIdxRow = (int16)3;
                libc.SetBitFieldPtr16Uint32(bp + 100, libc.Uint32FromInt32(SQLITE_IDXTYPE_IPK), 0, 0x3);
                @unsafe.Pointer.ElementRef<TLogEst>(__ccgo_ptr(bp + 160), 0, 2) = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnRowLogEst;
                @unsafe.Pointer.ElementRef<TLogEst>(__ccgo_ptr(bp + 160), (int32)1, 2) = 0;
                pFirst = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab)).Value.FpIndex;
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x1) >> 0) == 0) {
                    (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FpNext = pFirst;
                }
                pProbe = bp;
            }
        }
        rSize = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnRowLogEst;
        if (!(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 24)) != 0) && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (libc.Int32FromInt32(WHERE_RIGHT_JOIN) | libc.Int32FromInt32(WHERE_OR_SUBCLAUSE))) == 0 && (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_AutoIndex) != (uint64)0 && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x2) >> 1) != 0) && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x1) >> 0) != 0) && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x10) >> 4) != 0) && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x80) >> 7) != 0) && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) == 0) {
            pWCEnd = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm * 56;
            rLogSize = _estLog(ref (tls).DerefOrNull(), rSize);
            pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
            for (;;) {
                if (!(rc == SQLITE_OK && pTerm < pWCEnd)) {
                    break;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight & (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf) != 0) {
                    goto _1;
                }
                if (_termCanDriveIndex(tls, pTerm, pSrc, (uint64)0) != 0) {
                    (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq = (uint16)1;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip = (uint16)0;
                    (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FpIndex = (uintptr)0;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = (uint16)1;
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm))).Value = pTerm;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = (int16)((int32)rLogSize + (int32)rSize);
                    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VIEW)) && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Ephemeral) == (uint32)0) {
                        v2 = pNew + 18;
                        (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v2)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v2)) + libc.Int32FromInt32(28));
                    } else {
                        v2 = pNew + 18;
                        (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v2)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v2)) - libc.Int32FromInt32(25));
                    }
                    if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup < 0) {
                        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = 0;
                    }
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = (int16)43;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = _sqlite3LogEstAdd(ref (tls).DerefOrNull(), rLogSize, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut);
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = (uint32)WHERE_AUTO_INDEX;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq = (TBitmask)(mPrereq | (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight);
                    rc = _whereLoopInsert(tls, pBuilder, pNew);
                }
                goto _1;
_1:;
                pTerm += 56;
            }
        }
        for (;;) {
            if (!(rc == SQLITE_OK && pProbe != 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FpPartIdxWhere != (uintptr)0 && !(_whereUsablePartialIndex(tls, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype, pWC, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FpPartIdxWhere) != 0)) {
                goto _4;
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x100) >> 8) != 0) {
                goto _4;
            }
            rSize = ~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst));
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq = (uint16)0;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnBtm = (uint16)0;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnTop = (uint16)0;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnDistinctCol = (uint16)0;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip = (uint16)0;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = (uint16)0;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiSortIdx = (uint8)0;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = 0;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq = mPrereq;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = rSize;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FpIndex = pProbe;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FpOrderBy = (uintptr)0;
            b = _indexMightHelpWithOrderBy(tls, pBuilder, pProbe, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor);
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_IPK) {
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = (uint32)WHERE_IPK;
                if (b != 0) {
                    v5 = iSortIdx;
                } else {
                    v5 = 0;
                }
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiSortIdx = libc.Uint8FromInt32(v5);
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = (int16)((int32)rSize + (int32)16 - (int32)2 * libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasStat4) != (uint32)0));
                _whereLoopOutputAdjust(tls, pWC, pNew, rSize);
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x4) >> 2) != 0) {
                    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40) >> 6) != 0) {
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_COROUTINE;
                    }
                    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 72))))).Value.FpSelect)).Value.FselFlags & (uint32)SF_Recursive) == (uint32)0) {
                        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FpOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 72))))).Value.FpSelect)).Value.FpOrderBy;
                    }
                } else {
                    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40000) >> 18) != 0) {
                        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = 0;
                    }
                }
                rc = _whereLoopInsert(tls, pBuilder, pNew);
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = rSize;
                if (rc != 0) {
                    break;
                }
            } else {
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x20) >> 5) != 0) {
                    m = (uint64)0;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_IDX_ONLY) | libc.Int32FromInt32(WHERE_INDEXED));
                } else {
                    m = (TBitmask)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FcolUsed & (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FcolNotIdxed);
                    if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FpPartIdxWhere != 0) {
                        _wherePartIdxExpr(tls, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse, pProbe, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FpPartIdxWhere, @unsafe.Pointer.FromPinnedBox(Ꮡm), 0, (uintptr)0);
                    }
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = (uint32)WHERE_INDEXED;
                    if (m == libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1))) || (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x800) >> 11) != 0 && !((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x400) >> 10) != 0) && m != (uint64)0) {
                        isCov = _whereIsCoveringIndex(tls, pWInfo, pProbe, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor);
                        if (isCov == (uint32)0) {
                        } else {
                            m = (uint64)0;
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= isCov;
                            if ((Tu32)(isCov & (uint32)WHERE_IDX_ONLY) != 0) {
                            } else {
                            }
                        }
                    } else {
                        if (m == (uint64)0 && ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 || (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect != (uintptr)0 || _sqlite3FaultSim(tls, (int32)700) != 0)) {
                            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_IDX_ONLY) | libc.Int32FromInt32(WHERE_INDEXED));
                        }
                    }
                }
                if (b != 0 || !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) || (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FpPartIdxWhere != (uintptr)0 || (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x2) >> 1) != 0 || m == (uint64)0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x4) >> 2) == 0 && (int32)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FszIdxRow < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FszTabRow && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_ONEPASS_DESIRED) == 0 && _sqlite3Config.FbUseCis != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_CoverIdxScan))) == (uint32)0) {
                    if (b != 0) {
                        v5 = iSortIdx;
                    } else {
                        v5 = 0;
                    }
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiSortIdx = libc.Uint8FromInt32(v5);
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = (int16)((int32)rSize + (int32)1 + quo((int32)15 * (int32)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FszIdxRow, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FszTabRow));
                    if (m != (uint64)0) {
                        nLookup = (int16)((int32)rSize + (int32)16);
                        iCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor;
                        pWC2 = pWInfo + 104;
                        ii = 0;
                        for (;;) {
                            if (!(ii < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC2)).Value.FnTerm)) {
                                break;
                            }
                            pTerm1 = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC2)).Value.Fa + (uintptr)ii * 56;
                            if (!(_sqlite3ExprCoveredByIndex(tls, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FpExpr, iCur, pProbe) != 0)) {
                                break;
                            }
                            if ((int32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FtruthProb <= 0) {
                                nLookup = (int16)((int32)nLookup + (int32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FtruthProb);
                            } else {
                                nLookup = (TLogEst)(nLookup - 1);
                                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS))) != 0) {
                                    nLookup = (int16)((int32)nLookup - libc.Int32FromInt32(19));
                                }
                            }
                            goto _7;
_7:;
                            ii = ii + 1;
                        }
                        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = _sqlite3LogEstAdd(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun, nLookup);
                    }
                    _whereLoopOutputAdjust(tls, pWC, pNew, rSize);
                    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0 && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaColExpr != 0) {
                    } else {
                        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40000) >> 18) != 0) {
                            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = 0;
                        }
                        rc = _whereLoopInsert(tls, pBuilder, pNew);
                    }
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = rSize;
                    if (rc != 0) {
                        break;
                    }
                }
            }
            (@unsafe.Pointer.To<uint8>(@unsafe.Add(pBuilder, 44))).Value = (uint8)0;
            rc = _whereLoopAddBtreeIndex(tls, pBuilder, pSrc, pProbe, 0);
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pBuilder, 44))) == (int32)SQLITE_BLDF1_INDEXED) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= (uint32)TF_MaybeReanalyze;
            }
            _sqlite3Stat4ProbeFree(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 32)));
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pBuilder, 40))).Value = 0;
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 32))).Value = (uintptr)0;
            goto _4;
_4:;
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x2) >> 1) != 0) {
                v2 = (uintptr)0;
            } else {
                v2 = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FpNext;
            }
            pProbe = v2;
            iSortIdx = iSortIdx + 1;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _whereLoopXfer(ж<libc.TLS> tls, uintptr db, uintptr pTo, uintptr pFrom) {
    _whereLoopClearUnion(tls, db, pTo);
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pFrom)).Value.FnLTerm) > libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTo)).Value.FnLSlot) && _whereLoopResize(tls, db, pTo, libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pFrom)).Value.FnLTerm)) != 0) {
        libc.Xmemset(tls, pTo, 0, (uint64)(libc.UintptrFromInt32(0) + 56));
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemcpy(tls, pTo, pFrom, (uint64)(libc.UintptrFromInt32(0) + 56));
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTo)).Value.FaLTerm, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pFrom)).Value.FaLTerm, (uint64)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTo)).Value.FnLTerm * (uint64)8);
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pFrom)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) != 0) {
        libc.SetBitFieldPtr8Uint32(pFrom + 24 + 4, libc.Uint32FromInt32(0), 0, 0x1);
    } else {
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pFrom)).Value.FwsFlags & (uint32)WHERE_AUTO_INDEX) != (uint32)0) {
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pFrom + 24))).Value.FpIndex = (uintptr)0;
        }
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _writeJournalHdr(ж<libc.TLS> tls, uintptr pPager) {
    int32 ii = default;
    int32 rc = default;
    Tu32 nHeader = default;
    Tu32 nWrite = default;
    uintptr zHeader = default;
    Ti64 v2 = default;
    _ = ii;
    _ = nHeader;
    _ = nWrite;
    _ = rc;
    _ = zHeader;
    _ = v2;
    rc = SQLITE_OK;
    zHeader = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace;
    nHeader = libc.Uint32FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize);
    if (nHeader > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize) {
        nHeader = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize;
    }
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint)) {
            break;
        }
        if ((@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint + (uintptr)ii * 56))).Value.FiHdrOffset == 0) {
            (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint + (uintptr)ii * 56))).Value.FiHdrOffset = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    v2 = _journalHdrOffset(ref (tls).DerefOrNull(), pPager);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = v2;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr = v2;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoSync != 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_MEMORY || (_sqlite3OsDeviceCharacteristics(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd) & (int32)SQLITE_IOCAP_SAFE_APPEND) != 0) {
        libc.Xmemcpy(tls, zHeader, (uintptr)Ꮡ_aJournalMagic, (uint64)8);
        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(zHeader + (uintptr)8), (uint32)0xffffffffU);
    } else {
        libc.Xmemset(tls, zHeader, 0, libc.Uint64FromInt64(8) + libc.Uint64FromInt32(4));
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) != (int32)PAGER_JOURNALMODE_MEMORY) {
        Xsqlite3_randomness(tls, (int32)4, pPager + 56);
    }
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(zHeader + (uintptr)(libc.Uint64FromInt64(8) + libc.Uint64FromInt32(4))), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FcksumInit);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(zHeader + (uintptr)(libc.Uint64FromInt64(8) + libc.Uint64FromInt32(8))), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbOrigSize);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(zHeader + (uintptr)(libc.Uint64FromInt64(8) + libc.Uint64FromInt32(12))), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize);
    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(zHeader + (uintptr)(libc.Uint64FromInt64(8) + libc.Uint64FromInt32(16))), libc.Uint32FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize));
    libc.Xmemset(tls, zHeader + (uintptr)(libc.Uint64FromInt64(8) + libc.Uint64FromInt32(20)), 0, (uint64)nHeader - (libc.Uint64FromInt64(8) + libc.Uint64FromInt32(20)));
    nWrite = (uint32)0;
    for (;;) {
        if (!(rc == SQLITE_OK && nWrite < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize)) {
            break;
        }
        rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, zHeader, libc.Int32FromUint32(nHeader), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff);
        (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pPager + 96))).Value += libc.Int64FromUint32(nHeader);
        goto _3;
_3:;
        nWrite = nWrite + nHeader;
    }
    return rc;
}

} // end sqlite3_package
