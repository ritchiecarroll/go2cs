namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_clear_bindings(ж<libc.TLS> tls, uintptr pStmt) {
    int32 i = default;
    int32 rc = default;
    uintptr mutex = default;
    uintptr p = default;
    _ = i;
    _ = mutex;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    p = pStmt;
    mutex = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex;
    Xsqlite3_mutex_enter(tls, mutex);
    i = 0;
    for (;;) {
        if (!(i < (int32)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnVar)) {
            break;
        }
        _sqlite3VdbeMemRelease(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar + (uintptr)i * 56);
        (@unsafe.Pointer.To<TMem>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar + (uintptr)i * 56))).Value.Fflags = (uint16)MEM_Null;
        goto _1;
_1:;
        i = i + 1;
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fexpmask != 0) {
        libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 0, 0x3);
    }
    Xsqlite3_mutex_leave(tls, mutex);
    return rc;
}

public static int32 /*r*/ Xsqlite3_expired(ж<libc.TLS> tls, uintptr pStmt) {
    int32 iRet = default;
    uintptr p = default;
    _ = iRet;
    _ = p;
    iRet = (int32)1;
    if (pStmt != 0) {
        p = pStmt;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
        iRet = (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x3) >> 0));
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
    }
    return iRet;
}

public static int32 /*r*/ Xsqlite3_stmt_isexplain(ж<libc.TLS> tls, uintptr pStmt) {
    int32 v1 = default;
    _ = v1;
    if (pStmt != 0) {
        v1 = (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pStmt + 200)) & 0xc) >> 2));
    } else {
        v1 = 0;
    }
    return v1;
}

public static int32 /*r*/ Xsqlite3_stmt_readonly(ж<libc.TLS> tls, uintptr pStmt) {
    int32 v1 = default;
    _ = v1;
    if (pStmt != 0) {
        v1 = (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pStmt + 200)) & 0x40) >> 6));
    } else {
        v1 = (int32)1;
    }
    return v1;
}

public static int32 /*r*/ Xsqlite3_transfer_bindings(ж<libc.TLS> tls, uintptr pFromStmt, uintptr pToStmt) {
    uintptr pFrom = default;
    uintptr pTo = default;
    _ = pFrom;
    _ = pTo;
    pFrom = pFromStmt;
    pTo = pToStmt;
    if ((int32)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pFrom)).Value.FnVar != (int32)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pTo)).Value.FnVar) {
        return (int32)SQLITE_ERROR;
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pTo)).Value.Fexpmask != 0) {
        libc.SetBitFieldPtr16Uint32(pTo + 200, libc.Uint32FromInt32(1), 0, 0x3);
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pFrom)).Value.Fexpmask != 0) {
        libc.SetBitFieldPtr16Uint32(pFrom + 200, libc.Uint32FromInt32(1), 0, 0x3);
    }
    return _sqlite3TransferBindings(tls, pFromStmt, pToStmt);
}

internal static TBitmask /*r*/ _exprSelectUsage(ref libc.TLS tls, uintptr pMaskSet, uintptr pS) {
    int32 i = default;
    TBitmask mask = default;
    uintptr pSrc = default;
    _ = i;
    _ = mask;
    _ = pSrc;
    mask = (uint64)0;
    while (pS != 0) {
        pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpSrc;
        mask = (TBitmask)(mask | _sqlite3WhereExprListUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpEList));
        mask = (TBitmask)(mask | _sqlite3WhereExprListUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpGroupBy));
        mask = (TBitmask)(mask | _sqlite3WhereExprListUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpOrderBy));
        mask = (TBitmask)(mask | _sqlite3WhereExprUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpWhere));
        mask = (TBitmask)(mask | _sqlite3WhereExprUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpHaving));
        if (pSrc != (uintptr)0) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
                    break;
                }
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 24 + 4)) & 0x4) >> 2) != 0) {
                    mask = (TBitmask)(mask | _exprSelectUsage(ref tls, pMaskSet, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 72))))).Value.FpSelect));
                }
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 24 + 4)) & 0x800) >> 11) == 0) {
                    mask = (TBitmask)(mask | _sqlite3WhereExprUsage(ref tls, pMaskSet, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 64))));
                }
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 24 + 4)) & 0x8) >> 3) != 0) {
                    mask = (TBitmask)(mask | _sqlite3WhereExprListUsage(ref tls, pMaskSet, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 48))));
                }
                goto _1;
_1:;
                i = i + 1;
            }
        }
        pS = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpPrior;
    }
    return mask;
}

internal static int32 /*r*/ _inAnyUsingClause(ref libc.TLS tls, uintptr zName, uintptr pBase, int32 N) {
    while (N > 0) {
        N = N - 1;
        pBase += 80;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pBase + 24 + 4)) & 0x800) >> 11) == 0) {
            continue;
        }
        if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pBase + 64)) == (uintptr)0) {
            continue;
        }
        if (_sqlite3IdListIndex(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pBase + 64)), zName) >= 0) {
            return (int32)1;
        }
    }
    return 0;
}

internal static int32 /*r*/ _indexColumnNotNull(ref libc.TLS tls, uintptr pIdx, int32 iCol) {
    int32 r = default;

    int32 j = default;
    _ = j;
    j = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)iCol * 2)));
    if (j >= 0) {
        return (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FaCol + (uintptr)j * 16 + 8)) & 0xf) >> 0);
    } else {
        if (j == -(int32)1) {
            return (int32)1;
        } else {
            return 0;
        }
    }
    return r;
}

internal static uintptr /*r*/ _isSelfJoinView(ж<libc.TLS> tls, uintptr pTabList, uintptr pThis, int32 iFirst, int32 iEnd) {
    uintptr pItem = default;
    uintptr pS1 = default;
    uintptr pSel = default;
    int32 v1 = default;
    _ = pItem;
    _ = pS1;
    _ = pSel;
    _ = v1;
    pSel = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pThis + 72))))).Value.FpSelect;
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FselFlags & (uint32)SF_PushDown) != 0) {
        return (uintptr)0;
    }
    while (iFirst < iEnd) {
        v1 = iFirst;
        iFirst = iFirst + 1;
        pItem = pTabList + 8 + (uintptr)v1 * 80;
        if (!((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0)) {
            continue;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x40) >> 6) != 0) {
            continue;
        }
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName == (uintptr)0) {
            continue;
        }
        if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab)).Value.FpSchema != (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pThis)).Value.FpSTab)).Value.FpSchema) {
            continue;
        }
        if (Xsqlite3_stricmp(tls, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pThis)).Value.FzName) != 0) {
            continue;
        }
        pS1 = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect;
        if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab)).Value.FpSchema == (uintptr)0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FselId != (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS1)).Value.FselId) {
            continue;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS1)).Value.FselFlags & (uint32)SF_PushDown) != 0) {
            continue;
        }
        return pItem;
    }
    return (uintptr)0;
}

internal static void _pager_write_changecounter(ref libc.TLS tls, uintptr pPg) {
    Tu32 change_counter = default;
    _ = change_counter;
    if (pPg == (uintptr)0) {
        return;
    }
    change_counter = _sqlite3Get4byte(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager + 136)) + (uint32)1;
    _sqlite3Put4byte(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData + libc.UintptrFromInt32(24)), change_counter);
    _sqlite3Put4byte(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData + libc.UintptrFromInt32(92)), change_counter);
    _sqlite3Put4byte(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData + libc.UintptrFromInt32(96)), (uint32)SQLITE_VERSION_NUMBER);
}

internal static void _renameColumnElistNames(ж<libc.TLS> tls, @unsafe.Pointer pParse, @unsafe.Pointer pCtx, uintptr pEList, uintptr zOld) {
    int32 i = default;
    uintptr zName = default;
    _ = i;
    _ = zName;
    if (pEList != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            zName = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FzEName;
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x3) >> 0) == ENAME_NAME && zName != (uintptr)0 && 0 == Xsqlite3_stricmp(tls, zName, zOld)) {
                _renameTokenFind(ref (tls).DerefOrNull(), pParse, pCtx, zName);
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
}

internal static void _renameWalkTrigger(ж<libc.TLS> tls, uintptr pWalker, uintptr pTrigger) {
    int32 i = default;
    uintptr pSrc = default;
    uintptr pStep = default;
    uintptr pUpsert = default;
    _ = i;
    _ = pSrc;
    _ = pStep;
    _ = pUpsert;
    _sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpWhen);
    pStep = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.Fstep_list;
    for (;;) {
        if (!(pStep != 0)) {
            break;
        }
        _sqlite3WalkSelect(tls, pWalker, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSelect);
        _sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpWhere);
        _sqlite3WalkExprList(tls, pWalker, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList);
        if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpUpsert != 0) {
            pUpsert = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpUpsert;
            _sqlite3WalkExprList(tls, pWalker, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTarget);
            _sqlite3WalkExprList(tls, pWalker, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertSet);
            _sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertWhere);
            _sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTargetWhere);
        }
        if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc != 0) {
            pSrc = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
                    break;
                }
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 24 + 4)) & 0x4) >> 2) != 0) {
                    _sqlite3WalkSelect(tls, pWalker, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 72))))).Value.FpSelect);
                }
                goto _2;
_2:;
                i = i + 1;
            }
        }
        goto _1;
_1:;
        pStep = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpNext;
    }
}

internal static int32 /*r*/ _sameSrcAlias(ж<libc.TLS> tls, uintptr p0, uintptr pSrc) {
    int32 i = default;
    uintptr p1 = default;
    _ = i;
    _ = p1;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
            break;
        }
        p1 = pSrc + 8 + (uintptr)i * 80;
        if (p1 == p0) {
            goto _1;
        }
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p0)).Value.FpSTab == (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p1)).Value.FpSTab && 0 == Xsqlite3_stricmp(tls, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p0)).Value.FzAlias, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p1)).Value.FzAlias)) {
            return (int32)1;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(p1 + 24 + 4)) & 0x4) >> 2) != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p1 + 72))))).Value.FpSelect)).Value.FselFlags & (uint32)SF_NestedFrom) != (uint32)0 && _sameSrcAlias(tls, p0, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p1 + 72))))).Value.FpSelect)).Value.FpSrc) != 0) {
            return (int32)1;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return 0;
}

internal static void _sqlite3AutoincrementEnd(ж<libc.TLS> tls, uintptr pParse) {
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x200) >> 9)) != 0) {
        _autoIncrementEnd(tls, pParse);
    }
}

internal static void _sqlite3ExprCodeFactorable(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 target) {
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x80) >> 7)) != 0 && _sqlite3ExprIsConstantNotJoin(tls, pParse, pExpr) != 0) {
        _sqlite3ExprCodeRunJustOnce(tls, pParse, pExpr, target);
    } else {
        _sqlite3ExprCodeCopy(tls, pParse, pExpr, target);
    }
}

internal static void _sqlite3ExprListSetSpan(ж<libc.TLS> tls, uintptr pParse, uintptr pList, uintptr zStart, uintptr zEnd) {
    uintptr db = default;
    uintptr pItem = default;
    _ = db;
    _ = pItem;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (pList != 0) {
        pItem = pList + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr - (int32)1) * 32;
        if ((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FzEName == (uintptr)0) {
            (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FzEName = _sqlite3DbSpanDup(tls, db, zStart, zEnd);
            libc.SetBitFieldPtr16Uint32(pItem + 16 + 4, libc.Uint32FromInt32(ENAME_SPAN), 0, 0x3);
        }
    }
}

internal static void _sqlite3FreeIndex(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr p) {
    _sqlite3DeleteIndexSamples(tls, db, p);
    _sqlite3ExprDelete(tls, db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FpPartIdxWhere);
    _sqlite3ExprListDelete(tls, db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FaColExpr);
    _sqlite3DbFree__ccgo_gp(tls, db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FzColAff);
    if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 100)) & 0x10) >> 4) != 0) {
        _sqlite3DbFree__ccgo_gp(tls, db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FazColl);
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FaiRowEst);
    _sqlite3DbFree__ccgo_gp(tls, db, p);
}

internal static uintptr /*r*/ _sqlite3GetVdbe(ж<libc.TLS> tls, uintptr pParse) {
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe != 0) {
        return (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel == (uintptr)0) {
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 7, 0x80);
    }
    return _sqlite3VdbeCreate(tls, pParse);
}

internal static uintptr /*r*/ _sqlite3LocateTableItem(ж<libc.TLS> tls, uintptr pParse, Tu32 flags, uintptr p) {
    int32 iDb = default;
    uintptr zDb = default;
    _ = iDb;
    _ = zDb;
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(p + 24 + 4)) & 0x10000) >> 16) != 0) {
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 72)));
        zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
    } else {
        zDb = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 72));
    }
    return _sqlite3LocateTable(tls, pParse, flags, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p)).Value.FzName, zDb);
}

internal static void _sqlite3MayAbort(ref libc.TLS tls, uintptr pParse) {
    uintptr pToplevel = default;
    uintptr v1 = default;
    _ = pToplevel;
    _ = v1;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) {
        v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel;
    } else {
        v1 = pParse;
    }
    pToplevel = v1;
    libc.SetBitFieldPtr16Uint32(pToplevel + 40, libc.Uint32FromInt32(1), 1, 0x2);
}

internal static void _sqlite3MultiValuesEnd(ж<libc.TLS> tls, uintptr pParse, uintptr pVal) {
    uintptr pItem = default;
    _ = pItem;
    if (pVal != 0 && (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pVal)).Value.FpSrc)).Value.FnSrc > 0) {
        pItem = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pVal)).Value.FpSrc + 8;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
            _sqlite3VdbeEndCoroutine(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FregReturn);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FaddrFillSub - (int32)1);
        }
    }
}

internal static uintptr /*r*/ _sqlite3PrimaryKeyIndex(ref libc.TLS tls, uintptr pTab) {
    uintptr p = default;
    _ = p;
    p = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(p != 0 && !((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 100)) & 0x3) >> 0) == libc.Int32FromInt32(SQLITE_IDXTYPE_PRIMARYKEY)))) {
            break;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FpNext;
    }
    return p;
}

internal static void _sqlite3SelectExpand(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_sqlite3ExprWalkNoop);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x4) >> 2)) != 0) {
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_convertCompoundSelectToSubquery);
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = (uintptr)0;
            _sqlite3WalkSelect(tls, bp, pSelect);
        }
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_selectExpander);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = __ccgo_fp(_sqlite3SelectPopWith);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode = (uint16)0;
        _sqlite3WalkSelect(tls, bp, pSelect);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3SrcItemColumnUsed(ref libc.TLS tls, uintptr pItem, int32 iCol) {
    uintptr pResults = default;
    _ = pResults;
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4000) >> 14) != 0) {
        pResults = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect)).Value.FpEList;
        libc.SetBitFieldPtr16Uint32(pResults + 8 + (uintptr)iCol * 32 + 16 + 4, libc.Uint32FromInt32(1), 6, 0x40);
    }
}

internal static void _sqlite3SrcListAssignCursors(ref libc.TLS tls, uintptr pParse, uintptr pList) {
    int32 i = default;
    int32 v2 = default;
    uintptr pItem = default;
    uintptr v3 = default;
    _ = i;
    _ = pItem;
    _ = v2;
    _ = v3;
    if (pList != 0) {
        i = 0;
        pItem = pList + 8;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnSrc)) {
                break;
            }
            if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor >= 0) {
                goto _1;
            }
            v3 = pParse + 56;
            v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor = v2;
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
                _sqlite3SrcListAssignCursors(ref tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect)).Value.FpSrc);
            }
            goto _1;
_1:;
            i = i + 1;
            pItem += 80;
        }
    }
}

internal static void _sqlite3SrcListDelete(ж<libc.TLS> tls, @unsafe.Pointer db, uintptr pList) {
    int32 i = default;
    uintptr pItem = default;
    _ = i;
    _ = pItem;
    if (pList == (uintptr)0) {
        return;
    }
    pItem = pList + 8;
    i = libc.Int32FromInt32(0);
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnSrc)) {
            break;
        }
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName != 0) {
            _sqlite3DbNNFreeNN(tls, db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName);
        }
        if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzAlias != 0) {
            _sqlite3DbNNFreeNN(tls, db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzAlias);
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
            _sqlite3SubqueryDelete(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)));
        } else {
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x10000) >> 16) == 0 && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)) != (uintptr)0) {
                _sqlite3DbNNFreeNN(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)));
            }
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x2) >> 1) != 0) {
            _sqlite3DbFree__ccgo_gp(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48)));
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x8) >> 3) != 0) {
            _sqlite3ExprListDelete(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48)));
        }
        _sqlite3DeleteTable(tls, db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab);
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x800) >> 11) != 0) {
            _sqlite3IdListDelete(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64)));
        } else {
            if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64)) != 0) {
                _sqlite3ExprDelete(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64)));
            }
        }
        goto _1;
_1:;
        i = i + 1;
        pItem += 80;
    }
    _sqlite3DbNNFreeNN(tls, db, pList);
}

internal static void _sqlite3SrcListFuncArgs(ж<libc.TLS> tls, uintptr pParse, uintptr p, uintptr pList) {
    uintptr pItem = default;
    _ = pItem;
    if (p != 0) {
        pItem = p + 8 + (uintptr)((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p)).Value.FnSrc - (int32)1) * 80;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48))).Value = pList;
        libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 3, 0x8);
    } else {
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pList);
    }
}

internal static void _sqlite3SrcListIndexedBy(ж<libc.TLS> tls, uintptr pParse, uintptr p, uintptr pIndexedBy) {
    uintptr pItem = default;
    _ = pItem;
    if (p != 0 && (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pIndexedBy)).Value.Fn > (uint32)0) {
        pItem = p + 8 + (uintptr)((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p)).Value.FnSrc - (int32)1) * 80;
        if ((@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pIndexedBy)).Value.Fn == (uint32)1 && !((@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pIndexedBy)).Value.Fz != 0)) {
            libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 0, 0x1);
        } else {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48))).Value = _sqlite3NameFromToken(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer)pIndexedBy);
            libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 1, 0x2);
        }
    }
}

internal static uintptr /*r*/ _sqlite3SrcListLookup(ж<libc.TLS> tls, uintptr pParse, uintptr pSrc) {
    uintptr pItem = default;
    uintptr pTab = default;
    _ = pItem;
    _ = pTab;
    pItem = pSrc + 8;
    pTab = _sqlite3LocateTableItem(tls, pParse, (uint32)0, pItem);
    if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab != 0) {
        _sqlite3DeleteTable(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab);
    }
    (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab = pTab;
    libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 10, 0x400);
    if (pTab != 0) {
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef + 1;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x2) >> 1) != 0 && _sqlite3IndexedByLookup(tls, pParse, pItem) != 0) {
            pTab = (uintptr)0;
        }
    }
    return pTab;
}

internal static uintptr /*r*/ _sqlite3SubqueryDetach(ж<libc.TLS> tls, uintptr db, uintptr pItem) {
    uintptr pSel = default;
    _ = pSel;
    pSel = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect;
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)));
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = (uintptr)0;
    libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(0), 2, 0x4);
    return pSel;
}

internal static uintptr /*r*/ _sqlite3TriggersExist(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 op, uintptr pChanges, @unsafe.Pointer pMask) {
    if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpTrigger == (uintptr)0 && !(_tempTriggersExist(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb) != 0) || (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x1) >> 0)) != 0) {
        if (pMask != nil) {
            (@unsafe.Pointer.To<int32>(pMask)).Value = 0;
        }
        return (uintptr)0;
    }
    return _triggersReallyExist(tls, pParse, pTab, op, pChanges, pMask);
}

internal static void _sqlite3VdbeCountChanges(ref libc.TLS tls, uintptr v) {
    libc.SetBitFieldPtr16Uint32(v + 200, libc.Uint32FromInt32(1), 4, 0x10);
}

internal static int32 /*r*/ _sqlite3WalkSelectFrom(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 i = default;
    uintptr pItem = default;
    uintptr pSrc = default;
    _ = i;
    _ = pItem;
    _ = pSrc;
    pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
    if (pSrc != 0) {
        i = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc;
        pItem = pSrc + 8;
        for (;;) {
            if (!(i > 0)) {
                break;
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0 && _sqlite3WalkSelect(tls, pWalker, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect) != 0) {
                return (int32)WRC_Abort;
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x8) >> 3) != 0 && _sqlite3WalkExprList(tls, pWalker, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48))) != 0) {
                return (int32)WRC_Abort;
            }
            goto _1;
_1:;
            i = i - 1;
            pItem += 80;
        }
    }
    return WRC_Continue;
}

internal static int32 /*r*/ _sqlite3WhereIsSorted(ref libc.TLS tls, uintptr pWInfo) {
    return (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pWInfo + 68)) & 0x8) >> 3);
}

internal static int32 /*r*/ _sqlite3WhereUsesDeferredSeek(ref libc.TLS tls, uintptr pWInfo) {
    return (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pWInfo + 68)) & 0x1) >> 0);
}

internal static void _srclistRenumberCursors(ref libc.TLS tls, uintptr pParse, uintptr aCsrMap, uintptr pSrc, int32 iExcept) {
    int32 i = default;
    int32 v2 = default;
    uintptr p = default;
    uintptr pItem = default;
    uintptr v3 = default;
    _ = i;
    _ = p;
    _ = pItem;
    _ = v2;
    _ = v3;
    i = 0;
    pItem = pSrc + 8;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
            break;
        }
        if (i != iExcept) {
            if (!((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x80) >> 7) != 0) || ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap + (uintptr)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor + (int32)1) * 4)) == 0) {
                v3 = pParse + 56;
                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap + (uintptr)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor + (int32)1) * 4))).Value = v2;
            }
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aCsrMap + (uintptr)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor + (int32)1) * 4));
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
                p = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect;
                for (;;) {
                    if (!(p != 0)) {
                        break;
                    }
                    _srclistRenumberCursors(ref tls, pParse, aCsrMap, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc, -(int32)1);
                    goto _4;
_4:;
                    p = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior;
                }
            }
        }
        goto _1;
_1:;
        i = i + 1;
        pItem += 80;
    }
}

internal static void _substSelect(ж<libc.TLS> tls, @unsafe.Pointer pSubst, uintptr p, int32 doPrior) {
    int32 i = default;
    uintptr pItem = default;
    uintptr pSrc = default;
    uintptr v1 = default;
    bool v2 = default;
    _ = i;
    _ = pItem;
    _ = pSrc;
    _ = v1;
    _ = v2;
    if (!(p != 0)) {
        return;
    }
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pSubst, 20))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSubst, 20)) + 1;
    for (;;) {
        _substExprList(tls, pSubst, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList);
        _substExprList(tls, pSubst, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy);
        _substExprList(tls, pSubst, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy);
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving = _substExpr(tls, pSubst, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving);
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere = _substExpr(tls, pSubst, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere);
        pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        i = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc;
        pItem = pSrc + 8;
        for (;;) {
            if (!(i > 0)) {
                break;
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
                _substSelect(tls, pSubst, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect, (int32)1);
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x8) >> 3) != 0) {
                _substExprList(tls, pSubst, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48)));
            }
            goto _4;
_4:;
            i = i - 1;
            pItem += 80;
        }
        goto _3;
_3:;
        v2 = doPrior != 0; if (v2) {
            v1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior;
            p = v1;
        }
        if (!(v2 && v1 != (uintptr)0)) {
            break;
        }
    }
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pSubst, 20))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSubst, 20)) - 1;
}

internal static int32 /*r*/ _whereLoopInsert(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, uintptr pTemplate) {
    uintptr db = default;
    uintptr p = default;
    uintptr pIndex = default;
    uintptr pToDel = default;
    uintptr pWInfo = default;
    uintptr ppPrev = default;
    uintptr ppTail = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = db;
    _ = p;
    _ = pIndex;
    _ = pToDel;
    _ = pWInfo;
    _ = ppPrev;
    _ = ppTail;
    _ = rc;
    _ = v1;
    pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb;
    if (~@unsafe.Pointer.To<uint32>(@unsafe.Add(pBuilder, 48)) == (uint32)0) {
        if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 24)) != 0) {
            (@unsafe.Pointer.To<TWhereOrSet>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 24))))).Value.Fn = (uint16)0;
        }
        return (int32)SQLITE_DONE;
    }
    (@unsafe.Pointer.To<uint32>(@unsafe.Add(pBuilder, 48))).Value = ~@unsafe.Pointer.To<uint32>(@unsafe.Add(pBuilder, 48)) - 1;
    _whereLoopAdjustCost(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops, pTemplate);
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 24)) != (uintptr)0) {
        if ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnLTerm != 0) {
            _whereOrInsert(ref (tls).DerefOrNull(), (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 24))), (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.Fprereq, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut);
        }
        return SQLITE_OK;
    }
    ppPrev = _whereLoopFindLesser(ref (tls).DerefOrNull(), pWInfo + 80, pTemplate);
    if (ppPrev == (uintptr)0) {
        return SQLITE_OK;
    } else {
        p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPrev));
    }
    if (p == (uintptr)0) {
        v1 = _sqlite3DbMallocRawNN(tls, db, (uint64)104);
        p = v1;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPrev))).Value = v1;
        if (p == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
        _whereLoopInit(ref (tls).DerefOrNull(), p);
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FpNextLoop = (uintptr)0;
    } else {
        ppTail = p + 72;
        while (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTail)) != 0) {
            ppTail = _whereLoopFindLesser(ref (tls).DerefOrNull(), ppTail, pTemplate);
            if (ppTail == (uintptr)0) {
                break;
            }
            pToDel = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTail));
            if (pToDel == (uintptr)0) {
                break;
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTail))).Value = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pToDel)).Value.FpNextLoop;
            _whereLoopDelete(tls, db, pToDel);
        }
    }
    rc = _whereLoopXfer(tls, db, p, pTemplate);
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) == (uint32)0) {
        pIndex = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(p + 24))).Value.FpIndex;
        if (pIndex != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIndex + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_IPK) {
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(p + 24))).Value.FpIndex = (uintptr)0;
        }
    }
    return rc;
}

} // end sqlite3_package
