namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_bind_text16(ж<libc.TLS> tls, uintptr pStmt, int32 i, uintptr zData, int32 n, uintptr __ccgo_fp_xDel) {
    return _bindText(tls, pStmt, i, zData, libc.Int64FromUint64(libc.Uint64FromInt32(n) & ~libc.Uint64FromInt32(1)), __ccgo_fp_xDel, (uint8)SQLITE_UTF16LE);
}

public static int32 /*r*/ Xsqlite3_bind_text64(ж<libc.TLS> tls, uintptr pStmt, int32 i, uintptr zData, Tsqlite3_uint64 nData, uintptr __ccgo_fp_xDel, uint8 enc) {
    if (libc.Int32FromUint8(enc) != (int32)SQLITE_UTF8 && libc.Int32FromUint8(enc) != (int32)SQLITE_UTF8_ZT) {
        if (libc.Int32FromUint8(enc) == (int32)SQLITE_UTF16) {
            enc = (uint8)SQLITE_UTF16LE;
        }
        nData = (Tsqlite3_uint64)(nData & ~libc.Uint64FromInt32(1));
    }
    return _bindText(tls, pStmt, i, zData, libc.Int64FromUint64(nData), __ccgo_fp_xDel, enc);
}

public static int32 /*r*/ Xsqlite3_create_collation16(ж<libc.TLS> tls, uintptr db, uintptr zName, int32 enc, uintptr pCtx, uintptr __ccgo_fp_xCompare) {
    int32 rc = default;
    uintptr zName8 = default;
    _ = rc;
    _ = zName8;
    rc = SQLITE_OK;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    zName8 = _sqlite3Utf16to8(tls, db, zName, -(int32)1, (uint8)SQLITE_UTF16LE);
    if (zName8 != 0) {
        rc = _createCollation(tls, db, zName8, libc.Uint8FromInt32(enc), pCtx, __ccgo_fp_xCompare, (uintptr)0);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zName8);
    }
    rc = _sqlite3ApiExit(tls, db, rc);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

public static void Xsqlite3_result_text16(ж<libc.TLS> tls, uintptr pCtx, uintptr z, int32 n, uintptr __ccgo_fp_xDel) {
    _setResultStrOrError(tls, pCtx, z, libc.Int32FromUint64(libc.Uint64FromInt32(n) & ~libc.Uint64FromInt32(1)), (uint8)SQLITE_UTF16LE, __ccgo_fp_xDel);
}

public static void Xsqlite3_result_text64(ж<libc.TLS> tls, uintptr pCtx, uintptr z, Tsqlite3_uint64 n, uintptr __ccgo_fp_xDel, uint8 enc) {
    if (libc.Int32FromUint8(enc) != (int32)SQLITE_UTF8 && libc.Int32FromUint8(enc) != (int32)SQLITE_UTF8_ZT) {
        if (libc.Int32FromUint8(enc) == (int32)SQLITE_UTF16) {
            enc = (uint8)SQLITE_UTF16LE;
        }
        n = (Tsqlite3_uint64)(n & ~libc.Uint64FromInt32(1));
    }
    if (n > (uint64)0x7fffffff) {
        _invokeValueDestructor(tls, z, __ccgo_fp_xDel, pCtx);
    } else {
        _setResultStrOrError(tls, pCtx, z, libc.Int32FromUint64(n), enc, __ccgo_fp_xDel);
        _sqlite3VdbeMemZeroTerminateIfAble(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpOut);
    }
}

internal static int32 /*r*/ _btreeCellSizeCheck(ж<libc.TLS> tls, uintptr pPage) {
    int32 cellOffset = default;
    int32 i = default;
    int32 iCellFirst = default;
    int32 iCellLast = default;
    int32 pc = default;
    int32 sz = default;
    int32 usableSize = default;
    uintptr data = default;
    _ = cellOffset;
    _ = data;
    _ = i;
    _ = iCellFirst;
    _ = iCellLast;
    _ = pc;
    _ = sz;
    _ = usableSize;
    iCellFirst = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FcellOffset) + (int32)2 * libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
    usableSize = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize);
    iCellLast = usableSize - (int32)4;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
    cellOffset = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FcellOffset);
    if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
        iCellLast = iCellLast - 1;
    }
    i = 0;
    for (;;) {
        if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell))) {
            break;
        }
        pc = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(cellOffset + i * (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(cellOffset + i * (int32)2) + 1)));
        if (pc < iCellFirst || pc > iCellLast) {
            return _sqlite3CorruptError(tls, (int32)75374);
        }
        sz = libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxCellSize)(tls, pPage, data + (uintptr)pc));
        if (pc + sz > usableSize) {
            return _sqlite3CorruptError(tls, (int32)75379);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return SQLITE_OK;
}

internal static void _btreeParseCell(ж<libc.TLS> tls, uintptr pPage, int32 iCell, uintptr pInfo) {
    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * iCell)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * iCell) + 1))))), pInfo);
}

internal static int32 /*r*/ _btreePrevious(ж<libc.TLS> tls, uintptr pCur) {
    int32 idx = default;
    int32 rc = default;
    int32 v1 = default;
    uintptr pPage = default;
    _ = idx;
    _ = pPage;
    _ = rc;
    _ = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) != CURSOR_VALID) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
            v1 = _btreeRestoreCursorPosition(tls, pCur);
        } else {
            v1 = SQLITE_OK;
        }
        rc = v1;
        if (rc != SQLITE_OK) {
            return rc;
        }
        if ((int32)CURSOR_INVALID == libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState)) {
            return (int32)SQLITE_DONE;
        }
        if ((int32)CURSOR_SKIPNEXT == libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState)) {
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_VALID;
            if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext < 0) {
                return SQLITE_OK;
            }
        }
    }
    pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
    if (_sqlite3FaultSim(tls, (int32)412) != 0) {
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = (uint8)0;
    }
    if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit != 0)) {
        return _sqlite3CorruptError(tls, (int32)79626);
    }
    if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
        idx = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix);
        rc = _moveToChild(tls, pCur, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx) + 1))))))));
        if (rc != 0) {
            return rc;
        }
        rc = _moveToRightmost(tls, pCur);
    } else {
        while (libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix) == 0) {
            if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage == 0) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
                return (int32)SQLITE_DONE;
            }
            _moveToParent(tls, pCur);
        }
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = (Tu16)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix - 1);
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey != 0 && !((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            rc = _sqlite3BtreePrevious(tls, pCur, 0);
        } else {
            rc = SQLITE_OK;
        }
    }
    return rc;
}

internal static void _btreeSetNPage(ref libc.TLS tls, uintptr pBt, uintptr pPage1) {
    ref var nPage = ref heap(new int32(), out var ᏑnPage);
    _ = nPage;
    nPage = libc.Int32FromUint32(_sqlite3Get4byte(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 28)));
    if (nPage == 0) {
        _sqlite3PagerPagecount(ref tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, @unsafe.Pointer.FromPinnedBox(ᏑnPage));
    }
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = libc.Uint32FromInt32(nPage);
}

internal static void _callCollNeeded(ж<libc.TLS> tls, uintptr db, int32 enc, uintptr zName) {
    uintptr pTmp = default;
    uintptr zExternal = default;
    uintptr zExternal1 = default;
    _ = pTmp;
    _ = zExternal;
    _ = zExternal1;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxCollNeeded != 0) {
        zExternal = _sqlite3DbStrDup(tls, db, zName);
        if (!(zExternal != 0)) {
            return;
        }
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, int32, uintptr>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxCollNeeded)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpCollNeededArg, db, enc, zExternal);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zExternal);
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxCollNeeded16 != 0) {
        pTmp = _sqlite3ValueNew(tls, db);
        _sqlite3ValueSetStr(tls, pTmp, -(int32)1, zName, (uint8)SQLITE_UTF8, libc.UintptrFromInt32(0));
        zExternal1 = _sqlite3ValueText(tls, pTmp, (uint8)SQLITE_UTF16LE);
        if (zExternal1 != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, int32, uintptr>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxCollNeeded16)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpCollNeededArg, db, libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc), zExternal1);
        }
        _sqlite3ValueFree(tls, pTmp);
    }
}

internal static int32 /*r*/ _clearDatabasePage(ж<libc.TLS> tls, uintptr pBt, TPgno pgno, int32 freePageFlag, @unsafe.Pointer pnChange) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 hdr = default;
        int32 i = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        int32 v2 = default;
        uintptr pCell = default;
        ref var pPage = ref heap(new uintptr(), out var ᏑpPage);
        TCellInfo _ᴛ1 = default;
        _ = hdr;
        _ = i;
        _ = pCell;
        _ = pPage;
        _ = rc;
        _ = v2;
        if (pgno > _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
            r = _sqlite3CorruptError(tls, (int32)83413); goto ᒐdone;
        }
        if (freePageFlag >> (int)((int32)1) > (int32)BTCURSOR_MAX_DEPTH) {
            r = _sqlite3CorruptError(tls, (int32)83416); goto ᒐdone;
        }
        rc = _getAndInitPage(tls, pBt, pgno, @unsafe.Pointer.FromBox(ᏑpPage), 0);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FopenFlags) & (int32)BTREE_SINGLE) == 0 && _sqlite3PagerPageRefcount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage) != (int32)1 + libc.BoolInt32(pgno == (uint32)1)) {
            rc = _sqlite3CorruptError(tls, (int32)83423);
            goto cleardatabasepage_out;
        }
        hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset);
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell))) {
                break;
            }
            pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * i)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * i) + 1)))));
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
                rc = _clearDatabasePage(tls, pBt, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell), freePageFlag + (int32)2 | (int32)1, pnChange);
                if (rc != 0) {
                    goto cleardatabasepage_out;
                }
            }
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, pCell, bp);
            if ((uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal != (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload) {
                rc = _clearCellOverflow(tls, pPage, pCell, __ccgo_ptr(bp));
            } else {
                rc = SQLITE_OK;
            }
            if (rc != 0) {
                goto cleardatabasepage_out;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            rc = _clearDatabasePage(tls, pBt, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(hdr + (int32)8))), freePageFlag + (int32)2 | (int32)1, pnChange);
            if (rc != 0) {
                goto cleardatabasepage_out;
            }
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey != 0) {
                pnChange = default;
            }
        }
        if (pnChange != nil) {
            (@unsafe.Pointer.To<Ti64>(pnChange)).Value += libc.Int64FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
        }
        if ((freePageFlag & (int32)1) != 0) {
            _freePage(tls, pPage, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        } else {
            v2 = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
            rc = v2;
            if (v2 == 0) {
                _zeroPage(tls, pPage, libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)hdr))) | (int32)PTF_LEAF);
            }
        }
        goto cleardatabasepage_out;
cleardatabasepage_out:;
        _releasePage(tls, pPage);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _getOverflowPage(ж<libc.TLS> tls, uintptr pBt, TPgno ovfl, @unsafe.Pointer ppPage, @unsafe.Pointer pPgnoNext) {
    ref var eType = ref heap(new Tu8(), out var ᏑeType);
    TPgno iGuess = default;
    TPgno next = default;
    ref var pgno = ref heap(new TPgno(), out var Ꮡpgno);
    ref var pPage = ref heap(new uintptr(), out var ᏑpPage);
    int32 rc = default;
    int32 v1 = default;
    _ = eType;
    _ = iGuess;
    _ = next;
    _ = pPage;
    _ = pgno;
    _ = rc;
    _ = v1;
    next = (uint32)0;
    pPage = (uintptr)0;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
        iGuess = ovfl + (uint32)1;
        while (_ptrmapPageno(ref (tls).DerefOrNull(), pBt, iGuess) == iGuess || iGuess == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
            iGuess = iGuess + 1;
        }
        if (iGuess <= _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
            rc = _ptrmapGet(tls, pBt, iGuess, @unsafe.Pointer.FromPinnedBox(ᏑeType), @unsafe.Pointer.FromPinnedBox(Ꮡpgno));
            if (rc == SQLITE_OK && libc.Int32FromUint8(eType) == (int32)PTRMAP_OVERFLOW2 && pgno == ovfl) {
                next = iGuess;
                rc = (int32)SQLITE_DONE;
            }
        }
    }
    if (rc == SQLITE_OK) {
        if (ppPage == nil) {
            v1 = (int32)PAGER_GET_READONLY;
        } else {
            v1 = 0;
        }
        rc = _btreeGetPage(tls, pBt, ovfl, @unsafe.Pointer.FromBox(ᏑpPage), v1);
        if (rc == SQLITE_OK) {
            next = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData);
        }
    }
    (@unsafe.Pointer.To<TPgno>(pPgnoNext)).Value = next;
    if (ppPage != nil) {
        (@unsafe.Pointer.To<uintptr>(ppPage)).Value = pPage;
    } else {
        _releasePage(tls, pPage);
    }
    if (rc == (int32)SQLITE_DONE) {
        v1 = SQLITE_OK;
    } else {
        v1 = rc;
    }
    return v1;
}

internal static int32 /*r*/ _incrVacuumStep(ж<libc.TLS> tls, uintptr pBt, TPgno nFin, TPgno iLastPg, int32 bCommit) {
    TPgno dbSize = default;
    ref var iFreePg = ref heap(new TPgno(), out var ᏑiFreePg);
    ref var iFreePg1 = ref heap(new TPgno(), out var ᏑiFreePg1);
    TPgno iNear = default;
    ref var iPtrPage = ref heap(new TPgno(), out var ᏑiPtrPage);
    TPgno nFreeList = default;
    Tu8 eMode = default;
    ref var eType = ref heap(new Tu8(), out var ᏑeType);
    ref var pFreePg = ref heap(new uintptr(), out var ᏑpFreePg);
    ref var pFreePg1 = ref heap(new uintptr(), out var ᏑpFreePg1);
    ref var pLastPg = ref heap(new uintptr(), out var ᏑpLastPg);
    int32 rc = default;
    _ = dbSize;
    _ = eMode;
    _ = eType;
    _ = iFreePg;
    _ = iFreePg1;
    _ = iNear;
    _ = iPtrPage;
    _ = nFreeList;
    _ = pFreePg;
    _ = pFreePg1;
    _ = pLastPg;
    _ = rc;
    if (!(_ptrmapPageno(ref (tls).DerefOrNull(), pBt, iLastPg) == iLastPg) && iLastPg != libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
        nFreeList = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 36));
        if (nFreeList == (uint32)0) {
            return (int32)SQLITE_DONE;
        }
        rc = _ptrmapGet(tls, pBt, iLastPg, @unsafe.Pointer.FromPinnedBox(ᏑeType), @unsafe.Pointer.FromPinnedBox(ᏑiPtrPage));
        if (rc != SQLITE_OK) {
            return rc;
        }
        if (libc.Int32FromUint8(eType) == (int32)PTRMAP_ROOTPAGE) {
            return _sqlite3CorruptError(tls, (int32)77235);
        }
        if (libc.Int32FromUint8(eType) == (int32)PTRMAP_FREEPAGE) {
            if (bCommit == 0) {
                rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpFreePg), @unsafe.Pointer.FromPinnedBox(ᏑiFreePg), iLastPg, (uint8)BTALLOC_EXACT);
                if (rc != SQLITE_OK) {
                    return rc;
                }
                _releasePage(tls, pFreePg);
            }
        } else {
            eMode = (uint8)BTALLOC_ANY;
            iNear = (uint32)0;
            rc = _btreeGetPage(tls, pBt, iLastPg, @unsafe.Pointer.FromBox(ᏑpLastPg), 0);
            if (rc != SQLITE_OK) {
                return rc;
            }
            if (bCommit == 0) {
                eMode = (uint8)BTALLOC_LE;
                iNear = nFin;
            }
            for (var cond = true; cond; cond = bCommit != 0 && iFreePg1 > nFin) {
                dbSize = _btreePagecount(ref (tls).DerefOrNull(), pBt);
                rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpFreePg1), @unsafe.Pointer.FromPinnedBox(ᏑiFreePg1), iNear, eMode);
                if (rc != SQLITE_OK) {
                    _releasePage(tls, pLastPg);
                    return rc;
                }
                _releasePage(tls, pFreePg1);
                if (iFreePg1 > dbSize) {
                    _releasePage(tls, pLastPg);
                    return _sqlite3CorruptError(tls, (int32)77287);
                }
            }
            rc = _relocatePage(tls, pBt, pLastPg, eType, iPtrPage, iFreePg1, bCommit);
            _releasePage(tls, pLastPg);
            if (rc != SQLITE_OK) {
                return rc;
            }
        }
    }
    if (bCommit == 0) {
        for (var cond = true; cond; cond = iLastPg == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1) || _ptrmapPageno(ref (tls).DerefOrNull(), pBt, iLastPg) == iLastPg) {
            iLastPg = iLastPg - 1;
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbDoTruncate = (uint8)1;
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = iLastPg;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _indexCellCompare(ж<libc.TLS> tls, uintptr pPage, int32 idx, uintptr pIdxKey, TRecordCompare __ccgo_fp_xRecordCompare) {
    int32 c = default;
    int32 nCell = default;
    int32 v1 = default;
    uintptr pCell = default;
    bool v2 = default;
    _ = c;
    _ = nCell;
    _ = pCell;
    _ = v1;
    _ = v2;
    pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataOfst + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx) + 1)))));
    nCell = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCell)));
    if (nCell <= libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fmax1bytePayload)) {
        if (pCell + (uintptr)nCell >= (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
            return (int32)99;
        }
        c = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, uintptr, int32>>(__ccgo_fp_xRecordCompare)(tls, nCell, pCell + 1, pIdxKey);
    } else {
        v2 = !((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCell + 1))) & libc.Int32FromInt32(0x80)) != 0); if (v2) {
            v1 = ((nCell & libc.Int32FromInt32(0x7f))).Lsh((int64)(libc.Int32FromInt32(7))) + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCell + 1)));
            nCell = v1;
        }
        if (v2 && v1 <= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaxLocal)) {
            if (pCell + (uintptr)nCell >= (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
                return (int32)99;
            }
            c = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, uintptr, int32>>(__ccgo_fp_xRecordCompare)(tls, nCell, pCell + 2, pIdxKey);
        } else {
            c = (int32)99;
        }
    }
    return c;
}

internal static int32 /*r*/ _modifyPagePointer(ж<libc.TLS> tls, uintptr pPage, TPgno iFrom, TPgno iTo, Tu8 eType) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 i = default;
        int32 nCell = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr pCell = default;
        TCellInfo _ᴛ1 = default;
        _ = i;
        _ = nCell;
        _ = pCell;
        _ = rc;
        _ = v1;
        if (libc.Int32FromUint8(eType) == (int32)PTRMAP_OVERFLOW2) {
            if (_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData) != iFrom) {
                r = _sqlite3CorruptError(tls, (int32)77062); goto ᒐdone;
            }
            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData, iTo);
        } else {
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit != 0) {
                v1 = SQLITE_OK;
            } else {
                v1 = _btreeInitPage(tls, pPage);
            }
            rc = v1;
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
            nCell = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
            i = 0;
            for (;;) {
                if (!(i < nCell)) {
                    break;
                }
                pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * i)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * i) + 1)))));
                if (libc.Int32FromUint8(eType) == (int32)PTRMAP_OVERFLOW1) {
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, pCell, bp);
                    if ((uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal < (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload) {
                        if (pCell + (uintptr)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize) {
                            r = _sqlite3CorruptError(tls, (int32)77081); goto ᒐdone;
                        }
                        if (iFrom == _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pCell + (uintptr)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize - libc.UintptrFromInt32(4)))) {
                            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pCell + (uintptr)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize - libc.UintptrFromInt32(4)), iTo);
                            break;
                        }
                    }
                } else {
                    if (pCell + (uintptr)4 > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize) {
                        r = _sqlite3CorruptError(tls, (int32)77090); goto ᒐdone;
                    }
                    if (_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell) == iFrom) {
                        _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell, iTo);
                        break;
                    }
                }
                goto _2;
_2:;
                i = i + 1;
            }
            if (i == nCell) {
                if (libc.Int32FromUint8(eType) != (int32)PTRMAP_BTREE || _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8))) != iFrom) {
                    r = _sqlite3CorruptError(tls, (int32)77102); goto ᒐdone;
                }
                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8)), iTo);
            }
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _moveToLeftmost(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pPage = default;
    uintptr v1 = default;
    TPgno pgno = default;
    int32 rc = default;
    bool v2 = default;
    _ = pPage;
    _ = pgno;
    _ = rc;
    _ = v1;
    _ = v2;
    rc = SQLITE_OK;
    for (;;) {
        v2 = rc == SQLITE_OK; if (v2) {
            v1 = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
            pPage = v1;
        }
        if (!(v2 && !((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)v1)).Value.Fleaf != 0))) {
            break;
        }
        pgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix))))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix)) + 1)))))));
        rc = _moveToChild(tls, pCur, pgno);
    }
    return rc;
}

internal static int32 /*r*/ _moveToRightmost(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pPage = default;
    uintptr v1 = default;
    TPgno pgno = default;
    int32 rc = default;
    _ = pPage;
    _ = pgno;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    pPage = (uintptr)0;
    for (;;) {
        v1 = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        pPage = v1;
        if (!!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)v1)).Value.Fleaf != 0)) {
            break;
        }
        pgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8)));
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell;
        rc = _moveToChild(tls, pCur, pgno);
        if (rc != 0) {
            return rc;
        }
    }
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - (int32)1);
    return SQLITE_OK;
}

internal static int32 /*r*/ _ptrmapGet(ж<libc.TLS> tls, uintptr pBt, TPgno key, @unsafe.Pointer pEType, @unsafe.Pointer pPgno) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 iPtrmap = default;
        int32 offset = default;
        int32 rc = default;
        uintptr pPtrmap = default;
        uintptr _ᴛ1 = default;
        _ = iPtrmap;
        _ = offset;
        _ = pPtrmap;
        _ = rc;
        iPtrmap = libc.Int32FromUint32(_ptrmapPageno(ref (tls).DerefOrNull(), pBt, key));
        rc = _sqlite3PagerGet(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, libc.Uint32FromInt32(iPtrmap), bp, 0);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        pPtrmap = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        offset = libc.Int32FromUint32(libc.Uint32FromInt32(5) * (key - libc.Uint32FromInt32(iPtrmap) - libc.Uint32FromInt32(1)));
        if (offset < 0) {
            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            r = _sqlite3CorruptError(tls, (int32)74311); goto ᒐdone;
        }
        (@unsafe.Pointer.To<Tu8>(pEType)).Value = (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtrmap + (uintptr)offset))).Value;
        if (pPgno != nil) {
            (@unsafe.Pointer.To<TPgno>(pPgno)).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pPtrmap + (uintptr)(offset + (int32)1)));
        }
        _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pEType)) < (int32)1 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pEType)) > (int32)5) {
            r = _sqlite3CorruptError(tls, (int32)74319); goto ᒐdone;
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static Ti64 /*r*/ _readInt64(ref libc.TLS tls, @unsafe.Pointer p) {
    return libc.Int64FromUint64(_sqlite3Get8byte(ref tls, p));
}

internal static int32 /*r*/ _relocatePage(ж<libc.TLS> tls, uintptr pBt, uintptr pDbPage, Tu8 eType, TPgno iPtrPage, TPgno iFreePage, int32 isCommit) {
    TPgno iDbPage = default;
    TPgno nextOvfl = default;
    uintptr pPager = default;
    ref var pPtrPage = ref heap(new uintptr(), out var ᏑpPtrPage);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = iDbPage;
    _ = nextOvfl;
    _ = pPager;
    _ = pPtrPage;
    _ = rc;
    iDbPage = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pDbPage)).Value.Fpgno;
    pPager = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager;
    if (iDbPage < (uint32)3) {
        return _sqlite3CorruptError(tls, (int32)77137);
    }
    rc = _sqlite3PagerMovepage(tls, pPager, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pDbPage)).Value.FpDbPage, iFreePage, isCommit);
    if (rc != SQLITE_OK) {
        return rc;
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pDbPage)).Value.Fpgno = iFreePage;
    if (libc.Int32FromUint8(eType) == (int32)PTRMAP_BTREE || libc.Int32FromUint8(eType) == (int32)PTRMAP_ROOTPAGE) {
        rc = _setChildPtrmaps(tls, pDbPage);
        if (rc != SQLITE_OK) {
            return rc;
        }
    } else {
        nextOvfl = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pDbPage)).Value.FaData);
        if (nextOvfl != (uint32)0) {
            _ptrmapPut(tls, pBt, nextOvfl, (uint8)PTRMAP_OVERFLOW2, iFreePage, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            if (rc != SQLITE_OK) {
                return rc;
            }
        }
    }
    if (libc.Int32FromUint8(eType) != (int32)PTRMAP_ROOTPAGE) {
        rc = _btreeGetPage(tls, pBt, iPtrPage, @unsafe.Pointer.FromBox(ᏑpPtrPage), 0);
        if (rc != SQLITE_OK) {
            return rc;
        }
        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPtrPage)).Value.FpDbPage);
        if (rc != SQLITE_OK) {
            _releasePage(tls, pPtrPage);
            return rc;
        }
        rc = _modifyPagePointer(tls, pPtrPage, iDbPage, iFreePage, eType);
        _releasePage(tls, pPtrPage);
        if (rc == SQLITE_OK) {
            _ptrmapPut(tls, pBt, iFreePage, eType, iPtrPage, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        }
    }
    return rc;
}

internal static int32 /*r*/ _rtreeCallbackConstraint(ж<libc.TLS> tls, uintptr pConstraint, int32 eInt, uintptr pCellData, uintptr pSearch, @unsafe.Pointer prScore, @unsafe.Pointer peWithin) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        ref var c = ref heap(new TRtreeCoord(), out var Ꮡc);
        int32 nCoord = default;
        int32 rc = default;
        int32 v2 = default;
        uintptr pInfo = default;
        Tsqlite3_rtree_dbl v1 = default;
        array<Tsqlite3_rtree_dbl> _ᴛ1 = new(10);
        int32 _ᴛ2 = default;
        _ = c;
        _ = nCoord;
        _ = pInfo;
        _ = rc;
        _ = v1;
        _ = v2;
        pInfo = (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)pConstraint)).Value.FpInfo;
        nCoord = (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FnCoord;
        if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)pConstraint)).Value.Fop == (int32)RTREE_QUERY && libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pSearch)).Value.FiLevel) == (int32)1) {
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FiRowid = _readInt64(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCellData);
        }
        pCellData = pCellData + (uintptr)8;
        if (eInt == 0) {
            var matchᴛ1 = false;
            if (nCoord == (int32)10) { matchᴛ1 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)36, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)9, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)32, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)8, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && nCoord == (int32)8) { matchᴛ1 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)28, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)7, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)24, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)6, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && nCoord == (int32)6) { matchᴛ1 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)20, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)5, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)16, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)4, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && nCoord == (int32)4) { matchᴛ1 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)12, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)3, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)8, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)2, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)4, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)1, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
                _readCoord(ref (tls).DerefOrNull(), pCellData, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), 0, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>());
            }

        } else {
            var matchᴛ2 = false;
            if (nCoord == (int32)10) { matchᴛ2 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)36, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)9, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)32, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)8, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && nCoord == (int32)8) { matchᴛ2 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)28, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)7, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)24, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)6, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && nCoord == (int32)6) { matchᴛ2 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)20, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)5, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)16, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)4, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && nCoord == (int32)4) { matchᴛ2 = true;
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)12, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)3, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)8, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)2, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2) { /* default: */
                _readCoord(ref (tls).DerefOrNull(), pCellData + (uintptr)4, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), (int32)1, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
                _readCoord(ref (tls).DerefOrNull(), pCellData, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                @unsafe.Pointer.ElementRef<Tsqlite3_rtree_dbl>(__ccgo_ptr(bp), 0, 10) = (float64)(~Ꮡc.Reinterpret<TRtreeCoord, int32>());
            }

        }
        if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)pConstraint)).Value.Fop == (int32)RTREE_MATCH) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 80))).Value = 0;
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, uintptr, int32>>(~@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)pConstraint).of(TRtreeConstraint.ᏑFu).Reinterpret<TRtreeConstraintᴛ1_Fu, uintptr>())(tls, pInfo, nCoord, bp, bp + 80);
            if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 80)) == 0) {
                (@unsafe.Pointer.To<int32>(peWithin)).Value = NOT_WITHIN;
            }
            (@unsafe.Pointer.To<Tsqlite3_rtree_dbl>(prScore)).Value = (float64)0D;
        } else {
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FaCoord = bp;
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FiLevel = libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pSearch)).Value.FiLevel) - (int32)1;
            v1 = (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pSearch)).Value.FrScore;
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FrParentScore = v1;
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FrScore = v1;
            v2 = libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pSearch)).Value.FeWithin);
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FeParentWithin = v2;
            (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FeWithin = v2;
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(~@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)pConstraint).of(TRtreeConstraint.ᏑFu).Reinterpret<TRtreeConstraintᴛ1_Fu, uintptr>())(tls, pInfo);
            if ((@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FeWithin < ~@unsafe.Pointer.To<int32>(peWithin)) {
                (@unsafe.Pointer.To<int32>(peWithin)).Value = (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FeWithin;
            }
            if ((@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FrScore < ~@unsafe.Pointer.To<Tsqlite3_rtree_dbl>(prScore) || ~@unsafe.Pointer.To<Tsqlite3_rtree_dbl>(prScore) < (float64)0D) {
                (@unsafe.Pointer.To<Tsqlite3_rtree_dbl>(prScore)).Value = (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)pInfo)).Value.FrScore;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _rtreeRowid(ж<libc.TLS> tls, uintptr pVtabCursor, uintptr pRowid) {
    uintptr p = default;
    uintptr pCsr = default;
    uintptr pNode = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = p;
    _ = pCsr;
    _ = pNode;
    _ = rc;
    pCsr = pVtabCursor;
    p = _rtreeSearchPointFirst(ref (tls).DerefOrNull(), pCsr);
    rc = SQLITE_OK;
    pNode = _rtreeNodeOfFirstSearchPoint(tls, pCsr, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
    if (rc == SQLITE_OK && p != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell) >= _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2)) {
            rc = (int32)SQLITE_ABORT;
        } else {
            (@unsafe.Pointer.To<Tsqlite_int64>(__ccgo_ptr(pRowid))).Value = _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab, (@unsafe.Pointer)pNode, libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell));
        }
    }
    return rc;
}

internal static int32 /*r*/ _setChildPtrmaps(ж<libc.TLS> tls, uintptr pPage) {
    TPgno childPgno = default;
    TPgno childPgno1 = default;
    TPgno pgno = default;
    int32 i = default;
    int32 nCell = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 v1 = default;
    uintptr pBt = default;
    uintptr pCell = default;
    _ = childPgno;
    _ = childPgno1;
    _ = i;
    _ = nCell;
    _ = pBt;
    _ = pCell;
    _ = pgno;
    _ = rc;
    _ = v1;
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
    pgno = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fpgno;
    if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit != 0) {
        v1 = SQLITE_OK;
    } else {
        v1 = _btreeInitPage(tls, pPage);
    }
    rc = v1;
    if (rc != SQLITE_OK) {
        return rc;
    }
    nCell = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
    i = 0;
    for (;;) {
        if (!(i < nCell)) {
            break;
        }
        pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * i)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * i) + 1)))));
        _ptrmapPutOvflPtr(tls, pPage, pPage, pCell, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            childPgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell);
            _ptrmapPut(tls, pBt, childPgno, (uint8)PTRMAP_BTREE, pgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        }
        goto _2;
_2:;
        i = i + 1;
    }
    if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
        childPgno1 = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8)));
        _ptrmapPut(tls, pBt, childPgno1, (uint8)PTRMAP_BTREE, pgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeBeginTrans(ж<libc.TLS> tls, uintptr p, int32 wrflag, @unsafe.Pointer pSchemaVersion) {
    int32 r = default;

    uintptr pBt = default;
    _ = pBt;
    if ((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fsharable != 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == TRANS_NONE || libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_READ && wrflag != 0) {
        return _btreeBeginTrans(tls, p, wrflag, pSchemaVersion);
    }
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    if (pSchemaVersion != nil) {
        (@unsafe.Pointer.To<int32>(pSchemaVersion)).Value = libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 40)));
    }
    if (wrflag != 0) {
        return _sqlite3PagerOpenSavepoint(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb)).Value.FnSavepoint);
    } else {
        return SQLITE_OK;
    }
    return r;
}

internal static int32 /*r*/ _sqlite3BtreeTableMoveto(ж<libc.TLS> tls, uintptr pCur, Ti64 intKey, int32 biasRight, @unsafe.Pointer pRes) {
    int32 c = default;
    int32 idx = default;
    int32 lwr = default;
    int32 rc = default;
    int32 upr = default;
    TPgno chldPg = default;
    Ti64 nCellKey = default;
    uintptr pCell = default;
    uintptr pPage = default;
    uintptr v3 = default;
    _ = c;
    _ = chldPg;
    _ = idx;
    _ = lwr;
    _ = nCellKey;
    _ = pCell;
    _ = pPage;
    _ = rc;
    _ = upr;
    _ = v3;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == CURSOR_VALID && (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_ValidNKey) != 0) {
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey == intKey) {
            (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
            return SQLITE_OK;
        }
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey < intKey) {
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_AtLast) != 0) {
                (@unsafe.Pointer.To<int32>(pRes)).Value = -(int32)1;
                return SQLITE_OK;
            }
            if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey + (int64)1 == intKey) {
                (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
                rc = _sqlite3BtreeNext(tls, pCur, 0);
                if (rc == SQLITE_OK) {
                    _getCellInfo(tls, pCur);
                    if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey == intKey) {
                        return SQLITE_OK;
                    }
                } else {
                    if (rc != (int32)SQLITE_DONE) {
                        return rc;
                    }
                }
            }
        }
    }
    rc = _moveToRoot(tls, pCur);
    if (rc != 0) {
        if (rc == (int32)SQLITE_EMPTY) {
            (@unsafe.Pointer.To<int32>(pRes)).Value = -(int32)1;
            return SQLITE_OK;
        }
        return rc;
    }
    for (;;) {
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        lwr = 0;
        upr = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - (int32)1;
        idx = upr.Rsh((int64)((int32)1 - biasRight));
        for (;;) {
            pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataOfst + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx) + 1)))));
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKeyLeaf != 0) {
                for (;;) {
                    v3 = pCell;
                    pCell = pCell + 1;
                    if (!((int32)0x80 <= libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3))))) {
                        break;
                    }
                    if (pCell >= (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
                        return _sqlite3CorruptError(tls, (int32)79073);
                    }
                }
            }
            nCellKey = _sqlite3VarintValue(tls, pCell);
            if (nCellKey < intKey) {
                lwr = idx + (int32)1;
                if (lwr > upr) {
                    c = -(int32)1;
                    break;
                }
            } else {
                if (nCellKey > intKey) {
                    upr = idx - (int32)1;
                    if (lwr > upr) {
                        c = +libc.Int32FromInt32(1);
                        break;
                    }
                } else {
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(idx);
                    if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
                        lwr = idx;
                        goto moveto_table_next_layer;
                    } else {
                        v3 = pCur + 1;
                        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v3)) | libc.Int32FromInt32(BTCF_ValidNKey));
                        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey = nCellKey;
                        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
                        (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
                        return SQLITE_OK;
                    }
                }
            }
            idx = (lwr + upr) >> (int)((int32)1);
            goto _2;
_2:;
        }
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0) {
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(idx);
            (@unsafe.Pointer.To<int32>(pRes)).Value = c;
            rc = SQLITE_OK;
            goto moveto_table_finish;
        }
        goto moveto_table_next_layer;
moveto_table_next_layer:;
        if (lwr >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
            chldPg = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8)));
        } else {
            chldPg = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * lwr)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * lwr) + 1)))))));
        }
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(lwr);
        rc = _moveToChild(tls, pCur, chldPg);
        if (rc != 0) {
            break;
        }
        goto _1;
_1:;
    }
    goto moveto_table_finish;
moveto_table_finish:;
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
    return rc;
}

internal static Tu32 /*r*/ _sqlite3Get4byte(ref libc.TLS tls, @unsafe.Pointer p) {
    return (uint32)(~@unsafe.Pointer.To<Tu8>(p)) << (int)((int32)24) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 3)));
}

internal static void _sqlite3VdbeSerialGet(ж<libc.TLS> tls, uintptr buf, Tu32 serial_type, uintptr pMem) {
    var matchᴛ1 = false;
    if (serial_type == (uint32)10) { matchᴛ1 = true;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Null) | libc.Int32FromInt32(MEM_Zero));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = 0;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>()).Value = 0;
        return;
    }
    if (serial_type == (uint32)11) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && serial_type == (uint32)0) { matchᴛ1 = true;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Null;
        return;
    }
    if (serial_type == (uint32)1) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = (int64)libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf)));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (serial_type == (uint32)2) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = (int64)(libc.Int32FromInt32(256) * (int32)libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf))) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 1))));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (serial_type == (uint32)3) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = (int64)(libc.Int32FromInt32(65536) * (int32)libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf))) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 1))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 2))));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (serial_type == (uint32)4) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = (int64)libc.Int32FromUint32(((uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf)))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 3))));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (serial_type == (uint32)5) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = libc.Int64FromUint32(((uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + libc.UintptrFromInt32(2))))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + libc.UintptrFromInt32(2) + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + libc.UintptrFromInt32(2) + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + libc.UintptrFromInt32(2) + 3)))) + libc.Int64FromInt64(4294967296L) * (int64)(libc.Int32FromInt32(256) * (int32)libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf))) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(buf + 1))));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (serial_type == (uint32)6) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = libc.Int64FromUint64(_sqlite3Get8byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)buf));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (serial_type == (uint32)7) { matchᴛ1 = true;
        _sqlite3VdbeSerialGet7(tls, buf, pMem);
        return;
    }
    if (serial_type == (uint32)8) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && serial_type == (uint32)9) {
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = libc.Int64FromUint32(serial_type - (uint32)8);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
        return;
    }
    if (!matchᴛ1) { /* default: */
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = buf;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = libc.Int32FromUint32((serial_type - (uint32)12) / (uint32)2);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = _aFlag[(Tu32)(serial_type & (uint32)1)];
        return;
    }

    return;
}

internal static void _whereLoopClearUnion(ж<libc.TLS> tls, uintptr db, uintptr p) {
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_VIRTUALTABLE) | libc.Int32FromInt32(WHERE_AUTO_INDEX))) != 0) {
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) != (uint32)0 && (int32)((Tu32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(p + 24 + 4)) & 0x1) >> 0)) != 0) {
            Xsqlite3_free(tls, (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(p + 24))).Value.FidxStr);
            libc.SetBitFieldPtr8Uint32(p + 24 + 4, libc.Uint32FromInt32(0), 0, 0x1);
            (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(p + 24))).Value.FidxStr = (uintptr)0;
        } else {
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FwsFlags & (uint32)WHERE_AUTO_INDEX) != (uint32)0 && (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(p + 24))).Value.FpIndex != (uintptr)0) {
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)(@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(p + 24))).Value.FpIndex)).Value.FzColAff);
                _sqlite3DbFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(p + 24))).Value.FpIndex);
                (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(p + 24))).Value.FpIndex = (uintptr)0;
            }
        }
    }
}

internal static int32 /*r*/ _writeInt64(ref libc.TLS tls, uintptr p, Ti64 i) {
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(56)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(48)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 2))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(40)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 3))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(32)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 4))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(24)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 5))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(16)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 6))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(8)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 7))).Value = libc.Uint8FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(0)) & (int64)0xFF));
    return (int32)8;
}

} // end sqlite3_package
