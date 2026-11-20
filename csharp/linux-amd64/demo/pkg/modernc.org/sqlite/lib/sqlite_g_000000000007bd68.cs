namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _datetimeError(ж<libc.TLS> tls, uintptr p) {
    libc.Xmemset(tls, p, 0, (uint64)48);
    libc.SetBitFieldPtr8Uint32(p + 44, libc.Uint32FromInt32(1), 1, 0x2);
}

internal static int32 /*r*/ _renameResolveTrigger(ж<libc.TLS> tls, uintptr pParse) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        uintptr pNew = default;
        uintptr pSel = default;
        uintptr pSrc = default;
        uintptr pStep = default;
        uintptr pUpsert = default;
        uintptr pUpsertSet = default;
        int32 i = default;
        int32 rc = default;
        int32 v2 = default;
        TNameContext _ᴛ1 = new();
        _ = db;
        _ = i;
        _ = p;
        _ = pNew;
        _ = pSel;
        _ = pSrc;
        _ = pStep;
        _ = pUpsert;
        _ = pUpsertSet;
        _ = rc;
        _ = v2;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pNew = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTrigger;
        rc = SQLITE_OK;
        libc.Xmemset(tls, bp, 0, (uint64)56);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab = _sqlite3FindTable(tls, db, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pNew)).Value.Ftable, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)_sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pNew)).Value.FpTabSchema) * 32))).Value.FzDbSName);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeTriggerOp = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pNew)).Value.Fop;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab != 0) {
            rc = libc.BoolInt32(_sqlite3ViewGetColumnNames(tls, pParse, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab) != 0);
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pNew)).Value.FpWhen != 0) {
            rc = _sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pNew)).Value.FpWhen);
        }
        pStep = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pNew)).Value.Fstep_list;
        for (;;) {
            if (!(rc == SQLITE_OK && pStep != 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSelect != 0) {
                _sqlite3SelectPrep(tls, pParse, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSelect, bp);
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                    rc = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc;
                }
            }
            if (rc == SQLITE_OK && (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc != 0) {
                pSrc = _sqlite3SrcListDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc, 0);
                if (pSrc != 0) {
                    pSel = _sqlite3SelectNew(tls, pParse, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList, pSrc, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uint32)0, (uintptr)0);
                    if (pSel == (uintptr)0) {
                        (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList = (uintptr)0;
                        pSrc = (uintptr)0;
                        rc = (int32)SQLITE_NOMEM;
                    } else {
                        _renameSetENames(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList, (int32)ENAME_SPAN);
                        _sqlite3SelectPrep(tls, pParse, pSel, (uintptr)0);
                        _renameSetENames(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList, ENAME_NAME);
                        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                            v2 = (int32)SQLITE_ERROR;
                        } else {
                            v2 = SQLITE_OK;
                        }
                        rc = v2;
                        if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList != 0) {
                            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpEList = (uintptr)0;
                        }
                        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpSrc = (uintptr)0;
                        _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSel);
                    }
                    if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc != 0) {
                        i = 0;
                        for (;;) {
                            if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc)).Value.FnSrc && rc == SQLITE_OK)) {
                                break;
                            }
                            p = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc + 8 + (uintptr)i * 80;
                            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(p + 24 + 4)) & 0x4) >> 2) != 0) {
                                _sqlite3SelectPrep(tls, pParse, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 72))))).Value.FpSelect, (uintptr)0);
                            }
                            goto _3;
_3:;
                            i = i + 1;
                        }
                    }
                    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                        rc = (int32)SQLITE_NOMEM;
                    }
                    (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = pSrc;
                    if (rc == SQLITE_OK && (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpWhere != 0) {
                        rc = _sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpWhere);
                    }
                    if (rc == SQLITE_OK) {
                        rc = _sqlite3ResolveExprListNames(tls, bp, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList);
                    }
                    if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpUpsert != 0 && rc == SQLITE_OK) {
                        pUpsert = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpUpsert;
                        (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertSrc = pSrc;
                        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp)).of(TNameContext.ᏑFuNC)), 0))).Value = pUpsert;
                        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags = (int32)NC_UUpsert;
                        rc = _sqlite3ResolveExprListNames(tls, bp, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTarget);
                        if (rc == SQLITE_OK) {
                            pUpsertSet = (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertSet;
                            rc = _sqlite3ResolveExprListNames(tls, bp, pUpsertSet);
                        }
                        if (rc == SQLITE_OK) {
                            rc = _sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertWhere);
                        }
                        if (rc == SQLITE_OK) {
                            rc = _sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTargetWhere);
                        }
                        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags = 0;
                    }
                    (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = (uintptr)0;
                    _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pSrc);
                } else {
                    rc = (int32)SQLITE_NOMEM;
                }
            }
            goto _1;
_1:;
            pStep = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpNext;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _rtreeLeafConstraint(ж<libc.TLS> tls, uintptr p, int32 eInt, uintptr pCellData, @unsafe.Pointer peWithin) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        TRtreeDValue xN = default;
        Tsqlite3_rtree_dbl v1 = default;
        TRtreeCoord _ᴛ1 = new();
        _ = xN;
        _ = v1;
        pCellData = pCellData + (uintptr)((int32)8 + (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p)).Value.FiCoord * (int32)4);
        libc.Xmemcpy(tls, bp, pCellData, (uint64)4);
        (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>()).Value = (Tu32)((Tu32)((Tu32)((Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(24))) & (uint32)0xff) | (Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(8))) & (uint32)0xff00)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff) << (int)((int32)24)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff00) << (int)((int32)8));
        if (eInt != 0) {
            v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, int32>());
        } else {
            v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, TRtreeValue>());
        }
        xN = v1;
        switch ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p)).Value.Fop) {
        case var ᴛ when ᴛ == (int32)RTREE_TRUE: {
            return;
        }
        case var ᴛ when ᴛ == (int32)RTREE_FALSE: {
            break;
        }
        case var ᴛ when ᴛ == (int32)RTREE_LE: {
            if (xN <= ~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8))) {
                return;
            }
            break;
        }
        case var ᴛ when ᴛ == (int32)RTREE_LT: {
            if (xN < ~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8))) {
                return;
            }
            break;
        }
        case var ᴛ when ᴛ == (int32)RTREE_GE: {
            if (xN >= ~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8))) {
                return;
            }
            break;
        }
        case var ᴛ when ᴛ == (int32)RTREE_GT: {
            if (xN > ~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8))) {
                return;
            }
            break;
        }
        default: {
            if (xN == ~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8))) {
                return;
            }
            break;
            break;
        }}

        (@unsafe.Pointer.To<int32>(peWithin)).Value = NOT_WITHIN;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _rtreeNonleafConstraint(ж<libc.TLS> tls, uintptr p, int32 eInt, uintptr pCellData, @unsafe.Pointer peWithin) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tsqlite3_rtree_dbl val = default;
        Tsqlite3_rtree_dbl v1 = default;
        TRtreeCoord _ᴛ1 = new();
        TRtreeCoord _ᴛ2 = new();
        TRtreeCoord _ᴛ3 = new();
        TRtreeCoord _ᴛ4 = new();
        _ = val;
        _ = v1;
        pCellData = pCellData + (uintptr)((int32)8 + (int32)4 * ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p)).Value.FiCoord & (int32)0xfe));
        var exprᴛ1 = (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p)).Value.Fop;
        var matchᴛ1 = false;
        if (exprᴛ1 == (int32)RTREE_TRUE) { matchᴛ1 = true;
            return;
        }
        if (exprᴛ1 == (int32)RTREE_FALSE) { matchᴛ1 = true;
        }
        else if (exprᴛ1 == (int32)RTREE_EQ) { matchᴛ1 = true;
            libc.Xmemcpy(tls, bp, pCellData, (uint64)4);
            (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>()).Value = (Tu32)((Tu32)((Tu32)((Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(24))) & (uint32)0xff) | (Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(8))) & (uint32)0xff00)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff) << (int)((int32)24)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff00) << (int)((int32)8));
            if (eInt != 0) {
                v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, int32>());
            } else {
                v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp)).nilcheck().Reinterpret<TRtreeCoord, TRtreeValue>());
            }
            val = v1;
            if (~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8)) >= val) {
                pCellData = pCellData + (uintptr)4;
                libc.Xmemcpy(tls, bp + 4, pCellData, (uint64)4);
                (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, Tu32>()).Value = (Tu32)((Tu32)((Tu32)((Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(24))) & (uint32)0xff) | (Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(8))) & (uint32)0xff00)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff) << (int)((int32)24)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff00) << (int)((int32)8));
                if (eInt != 0) {
                    v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, int32>());
                } else {
                    v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 4)).nilcheck().Reinterpret<TRtreeCoord, TRtreeValue>());
                }
                val = v1;
                if (~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8)) <= val) {
                    return;
                }
            }
        }
        else if (exprᴛ1 == (int32)RTREE_LE) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)RTREE_LT) {
            libc.Xmemcpy(tls, bp + 8, pCellData, (uint64)4);
            (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, Tu32>()).Value = (Tu32)((Tu32)((Tu32)((Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(24))) & (uint32)0xff) | (Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(8))) & (uint32)0xff00)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff) << (int)((int32)24)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff00) << (int)((int32)8));
            if (eInt != 0) {
                v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, int32>());
            } else {
                v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 8)).nilcheck().Reinterpret<TRtreeCoord, TRtreeValue>());
            }
            val = v1;
            if (~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8)) >= val) {
                return;
            }
        }
        else if (!matchᴛ1) { /* default: */
            do {
                pCellData = pCellData + (uintptr)4;
                libc.Xmemcpy(tls, bp + 12, pCellData, (uint64)4);
                (@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, Tu32>()).Value = (Tu32)((Tu32)((Tu32)((Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(24))) & (uint32)0xff) | (Tu32)((~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() >> (int)(libc.Int32FromInt32(8))) & (uint32)0xff00)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff) << (int)((int32)24)) | (Tu32)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, Tu32>() & (uint32)0xff00) << (int)((int32)8));
                if (eInt != 0) {
                    v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, int32>());
                } else {
                    v1 = (float64)(~@unsafe.Pointer.To<TRtreeCoord>(__ccgo_ptr(bp + 12)).nilcheck().Reinterpret<TRtreeCoord, TRtreeValue>());
                }
                val = v1;
                if (~@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p + 8)) <= val) {
                    return;
                }
                break;
            } while (false);
        }

        (@unsafe.Pointer.To<int32>(peWithin)).Value = NOT_WITHIN;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3RenameExprlistUnmap(ж<libc.TLS> tls, uintptr pParse, uintptr pEList) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 i = default;
        TWalker _ᴛ1 = new();
        _ = i;
        if (pEList != 0) {
            libc.Xmemset(tls, bp, 0, (uint64)48);
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
            (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_renameUnmapExprCb);
            _sqlite3WalkExprList(tls, bp, pEList);
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                    break;
                }
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x3) >> 0) == ENAME_NAME) {
                    _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, (uintptr)0, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FzEName);
                }
                goto _1;
_1:;
                i = i + 1;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3SrcItemAttachSubquery(ж<libc.TLS> tls, uintptr pParse, uintptr pItem, uintptr pSelect, int32 dupSelect) {
    uintptr p = default;
    uintptr v1 = default;
    _ = p;
    _ = v1;
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x10000) >> 16) != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = (uintptr)0;
        libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(0), 16, 0x10000);
    } else {
        if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)) != (uintptr)0) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)));
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = (uintptr)0;
        }
    }
    if (dupSelect != 0) {
        pSelect = _sqlite3SelectDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pSelect, 0);
        if (pSelect == (uintptr)0) {
            return 0;
        }
    }
    v1 = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)24);
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = v1;
    p = v1;
    if (p == (uintptr)0) {
        _sqlite3SelectDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pSelect);
        return 0;
    }
    libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 2, 0x4);
    (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)p)).Value.FpSelect = pSelect;
    libc.Xmemset(tls, p + (uintptr)8, 0, libc.Uint64FromInt64(24) - libc.Uint64FromInt64(8));
    return (int32)1;
}

internal static int32 /*r*/ _sqlite3VdbeSerialGet7(ж<libc.TLS> tls, uintptr buf, uintptr pMem) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu64 _ᴛ1 = default;
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = _sqlite3Get8byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)buf);
        libc.Xmemcpy(tls, pMem, bp, (uint64)8);
        if ((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) & (libc.Uint64FromInt32(0x7ff).Lsh((int64)(libc.Int32FromInt32(52))))) == libc.Uint64FromInt32(0x7ff).Lsh((int64)(libc.Int32FromInt32(52))) && (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) & (libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(52))) - libc.Uint64FromInt32(1))) != (uint64)0) {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Null;
            r = (int32)1; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Real;
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end sqlite3_package
