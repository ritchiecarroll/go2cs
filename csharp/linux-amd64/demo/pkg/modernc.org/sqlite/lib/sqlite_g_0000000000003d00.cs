namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using System.Runtime.InteropServices;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_bind_int64(ж<libc.TLS> tls, uintptr pStmt, int32 i, Tsqlite_int64 iValue) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu32 expireMask = default;
        uintptr p = default;
        uintptr pVar = default;
        int32 sm1 = default;
        int32 sm2 = default;
        int32 v1 = default;
        bool v2 = default;
        uint32 v3 = default;
        _ = expireMask;
        _ = p;
        _ = pVar;
        _ = sm1;
        _ = sm2;
        _ = v1;
        _ = v2;
        _ = v3;
        p = pStmt;
        if (_vdbeSafetyNotNull(tls, p) != 0) {
            r = _sqlite3MisuseError(tls, (int32)95530); goto ᒐdone;
        }
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) != (int32)VDBE_READY_STATE) {
            _sqlite3Error(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, _sqlite3MisuseError(tls, (int32)95534));
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
            Xsqlite3_log(tls, (int32)SQLITE_MISUSE, __ccgo_ts + 5759, libc.VaList(bp + 8, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql));
            r = _sqlite3MisuseError(tls, (int32)95538); goto ᒐdone;
        }
        v2 = i <= 0; if (!v2) {
            i = i - 1;
            v1 = i;
        }
        if (v2 || v1 >= (int32)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnVar) {
            _sqlite3Error(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, (int32)SQLITE_RANGE);
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
            r = (int32)SQLITE_RANGE; goto ᒐdone;
        }
        pVar = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar + (uintptr)i * 56;
        if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fexpmask != (uint32)0) {
            if (i >= (int32)32) {
                v3 = (uint32)0x80000000U;
            } else {
                v3 = libc.Uint32FromInt32(1).Lsh((int64)(i));
            }
            expireMask = v3;
            if ((Tu32)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fexpmask & expireMask) != (uint32)0) {
                if ((Tu32)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fsmimask & expireMask) != (uint32)0) {
                    sm1 = libc.BoolInt32((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVar)).Value.Fflags) & (int32)MEM_Int) != 0 && ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVar) >= 0 && ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVar) <= (int64)1);
                    sm2 = libc.BoolInt32(iValue >= 0 && iValue <= (int64)1);
                    if (sm1 != sm2) {
                        libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 0, 0x3);
                    }
                } else {
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVar)).Value.Fflags) & (int32)MEM_Int) == 0 || iValue != ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVar)) {
                        libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 0, 0x3);
                    }
                }
            }
        }
        _sqlite3VdbeMemSetInt64(tls, pVar, iValue);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static int32 /*r*/ Xsqlite3_declare_vtab(ж<libc.TLS> tls, uintptr db, uintptr zCreateTable) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(464);
        defer(tls.Free, (nint)464, ref ᒐ);
        int32 i = default;
        int32 initBusy = default;
        int32 rc = default;
        ref var tokenType = ref heap(new int32(), out var ᏑtokenType);
        uintptr pCtx = default;
        uintptr pIdx = default;
        uintptr pNew = default;
        uintptr pTab = default;
        uintptr z = default;
        uintptr v3 = default;
        Ti16 v2 = default;
        TParse _ᴛ1 = new();
        _ = i;
        _ = initBusy;
        _ = pCtx;
        _ = pIdx;
        _ = pNew;
        _ = pTab;
        _ = rc;
        _ = tokenType;
        _ = z;
        _ = v2;
        _ = v3;
        rc = SQLITE_OK;
        z = zCreateTable;
        i = 0;
        for (;;) {
            if (!(_aKeyword1[i] != 0)) {
                break;
            }
            tokenType = 0;
            for (var cond = true; cond; cond = tokenType == (int32)TK_SPACE || tokenType == (int32)TK_COMMENT) {
                z = z + (uintptr)_sqlite3GetToken(ref (tls).DerefOrNull(), z, @unsafe.Pointer.FromPinnedBox(ᏑtokenType));
            }
            if (tokenType != libc.Int32FromUint8(_aKeyword1[i])) {
                _sqlite3ErrorWithMsg(tls, db, (int32)SQLITE_ERROR, __ccgo_ts + 23754, 0);
                r = (int32)SQLITE_ERROR; goto ᒐdone;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        pCtx = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVtabCtx;
        if (!(pCtx != 0) || (@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)pCtx)).Value.FbDeclared != 0) {
            _sqlite3Error(tls, db, _sqlite3MisuseError(tls, (int32)163873));
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
            r = _sqlite3MisuseError(tls, (int32)163875); goto ᒐdone;
        }
        pTab = (@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)pCtx)).Value.FpTab;
        _sqlite3ParseObjectInit(tls, bp, db);
        (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FeParseMode = (uint8)PARSE_MODE_DECLARE_VTAB;
        libc.SetBitFieldPtr16Uint32(bp + 40, libc.Uint32FromInt32(1), 0, 0x1);
        initBusy = libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy = (uint8)0;
        (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FnQueryLoop = (int16)1;
        if (SQLITE_OK == _sqlite3RunParser(tls, bp, zCreateTable)) {
            if (!((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol != 0)) {
                pNew = (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable;
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FaCol;
                _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TTableᴛ1_Fu_Ftab>((@unsafe.Pointer)(pNew + 64))).Value.FpDfltList);
                v2 = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FnCol;
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol = v2;
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnNVCol = v2;
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FtabFlags & libc.Uint32FromInt32(libc.Int32FromInt32(TF_WithoutRowid) | libc.Int32FromInt32(TF_NoVisibleRowid)));
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FnCol = 0;
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FaCol = (uintptr)0;
                if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)(@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)pCtx)).Value.FpVTable)).Value.FpMod)).Value.FpModule)).Value.FxUpdate != (uintptr)0 && libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)_sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pNew))).Value.FnKeyCol) != (int32)1) {
                    rc = (int32)SQLITE_ERROR;
                }
                pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FpIndex;
                if (pIdx != 0) {
                    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex = pIdx;
                    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pNew)).Value.FpIndex = (uintptr)0;
                    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable = pTab;
                }
            }
            (@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)pCtx)).Value.FbDeclared = (int32)1;
        } else {
            if ((@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FzErrMsg != 0) {
                v3 = __ccgo_ts + 3982;
            } else {
                v3 = (uintptr)0;
            }
            _sqlite3ErrorWithMsg(tls, db, (int32)SQLITE_ERROR, v3, libc.VaList(bp + 448, (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FzErrMsg));
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FzErrMsg);
            rc = (int32)SQLITE_ERROR;
        }
        (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FeParseMode = (uint8)PARSE_MODE_NORMAL;
        if ((@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpVdbe != 0) {
            _sqlite3VdbeFinalize(tls, (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpVdbe);
        }
        _sqlite3DeleteTable(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable);
        _sqlite3ParseObjectReset(tls, __ccgo_ptr(bp));
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy = libc.Uint8FromInt32(initBusy);
        rc = _sqlite3ApiExit(tls, db, rc);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static int32 /*r*/ Xsqlite3_deserialize(ж<libc.TLS> tls, uintptr db, uintptr zSchema, uintptr pData, Tsqlite3_int64 szDb, Tsqlite3_int64 szBuf, uint32 mFlags) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 iDb = default;
        int32 rc = default;
        uintptr p = default;
        ref var pStmt = ref heap(new uintptr(), out var ᏑpStmt);
        uintptr pStore = default;
        uintptr zSql = default;
        _ = iDb;
        _ = p;
        _ = pStmt;
        _ = pStore;
        _ = rc;
        _ = zSql;
        pStmt = (uintptr)0;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        if (zSchema == (uintptr)0) {
            zSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb))).Value.FzDbSName;
        }
        iDb = _sqlite3FindDbName(tls, db, zSchema);
        if (iDb < (int32)2 && iDb != 0) {
            rc = (int32)SQLITE_ERROR;
            goto end_deserialize;
        }
        zSql = Xsqlite3_mprintf(tls, __ccgo_ts + 4223, libc.VaList(bp + 8, zSchema));
        if (zSql == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            rc = _sqlite3_prepare_v2__ccgo_gp(tls, db, zSql, -(int32)1, @unsafe.Pointer.FromBox(ᏑpStmt), nil);
            Xsqlite3_free(tls, zSql);
        }
        if (rc != 0) {
            goto end_deserialize;
        }
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb = libc.Uint8FromInt32(iDb);
        libc.SetBitFieldPtr8Uint32(db + 200 + 8, libc.Uint32FromInt32(1), 3, 0x8);
        Xsqlite3_step(tls, pStmt);
        libc.SetBitFieldPtr8Uint32(db + 200 + 8, libc.Uint32FromInt32(0), 3, 0x8);
        rc = Xsqlite3_finalize(tls, pStmt);
        if (rc != SQLITE_OK) {
            goto end_deserialize;
        }
        p = _memdbFromDbSchema(tls, db, zSchema);
        if (p == (uintptr)0) {
            rc = (int32)SQLITE_ERROR;
        } else {
            pStore = (@unsafe.Pointer.To<TMemFile>((@unsafe.Pointer)p)).Value.FpStore;
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.FaData = pData;
            pData = (uintptr)0;
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.Fsz = szDb;
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.FszAlloc = szBuf;
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.FszMax = szBuf;
            if ((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.FszMax < _sqlite3Config.FmxMemdbSize) {
                (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.FszMax = _sqlite3Config.FmxMemdbSize;
            }
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)pStore)).Value.FmFlags = mFlags;
            rc = SQLITE_OK;
        }
        goto end_deserialize;
end_deserialize:;
        if (pData != 0 && (mFlags & (uint32)SQLITE_DESERIALIZE_FREEONCLOSE) != (uint32)0) {
            Xsqlite3_free(tls, pData);
        }
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _allocateIndexInfo(ж<libc.TLS> tls, uintptr pWInfo, uintptr pWC, TBitmask mUnusable, uintptr pSrc, @unsafe.Pointer pmNoOmit) {
    int32 bSortByGroup = default;
    int32 eDistinct = default;
    int32 i = default;
    int32 iCol = default;
    int32 j = default;
    int32 n = default;
    int32 nLast = default;
    int32 nOrderBy = default;
    int32 nTerm = default;
    int32 v10 = default;
    Tu16 mNoOmit = default;
    Tu16 op = default;
    uintptr p = default;
    uintptr pE2 = default;
    uintptr pExpr = default;
    uintptr pExpr1 = default;
    uintptr pHidden = default;
    uintptr pIdxCons = default;
    uintptr pIdxInfo = default;
    uintptr pIdxOrderBy = default;
    uintptr pOrderBy = default;
    uintptr pParse = default;
    uintptr pPk = default;
    uintptr pTab = default;
    uintptr pTerm = default;
    uintptr pUsage = default;
    uintptr zColl = default;
    uintptr v3 = default;
    uint32 v12 = default;
    bool v7 = default;
    _ = bSortByGroup;
    _ = eDistinct;
    _ = i;
    _ = iCol;
    _ = j;
    _ = mNoOmit;
    _ = n;
    _ = nLast;
    _ = nOrderBy;
    _ = nTerm;
    _ = op;
    _ = p;
    _ = pE2;
    _ = pExpr;
    _ = pExpr1;
    _ = pHidden;
    _ = pIdxCons;
    _ = pIdxInfo;
    _ = pIdxOrderBy;
    _ = pOrderBy;
    _ = pParse;
    _ = pPk;
    _ = pTab;
    _ = pTerm;
    _ = pUsage;
    _ = zColl;
    _ = v10;
    _ = v12;
    _ = v3;
    _ = v7;
    pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
    mNoOmit = (uint16)0;
    eDistinct = 0;
    pOrderBy = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy;
    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab;
    p = pWC;
    nTerm = libc.Int32FromInt32(0);
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        i = 0;
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.Fa;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FnTerm)) {
                break;
            }
            v3 = pTerm + 18;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)) & ~libc.Int32FromInt32(TERM_OK));
            if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FleftCursor != (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor) {
                goto _2;
            }
            if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight & mUnusable) != 0) {
                goto _2;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & ~libc.Int32FromInt32(WO_EQUIV)) == 0) {
                goto _2;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_VNULL) != 0) {
                goto _2;
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0 && !(_constraintCompatibleWithOuterJoin(ref (tls).DerefOrNull(), pTerm, pSrc) != 0)) {
                goto _2;
            }
            nTerm = nTerm + 1;
            v3 = pTerm + 18;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)) | libc.Int32FromInt32(TERM_OK));
            goto _2;
_2:;
            i = i + 1;
            pTerm += 56;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FpOuter;
    }
    nOrderBy = 0;
    if (pOrderBy != 0) {
        n = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr;
        i = 0;
        for (;;) {
            if (!(i < n)) {
                break;
            }
            pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr;
            if (_sqlite3ExprIsConstant(tls, (uintptr)0, pExpr) != 0) {
                goto _5;
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.Ffg.FsortFlags) & (int32)KEYINFO_ORDER_BIGNULL) != 0) {
                break;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_COLUMN && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable == (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor) {
                goto _5;
            }
            v7 = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_COLLATE; if (v7) {
                v3 = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
                pE2 = v3;
            }
            if (v7 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)v3)).Value.Fop) == (int32)TK_COLUMN && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FiTable == (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor) {
                (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FiColumn;
                if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FiColumn < 0) {
                    goto _5;
                }
                zColl = _sqlite3ColumnColl(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FiColumn * 16);
                if (zColl == (uintptr)0) {
                    zColl = (uintptr)Ꮡ_sqlite3StrBINARY;
                }
                if (Xsqlite3_stricmp(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)), zColl) == 0) {
                    goto _5;
                }
            }
            break;
            goto _5;
_5:;
            i = i + 1;
        }
        if (i == n) {
            bSortByGroup = libc.BoolInt32((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_SORTBYGROUP) != 0);
            nOrderBy = n;
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_DISTINCTBY) != 0 && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x8000) >> 15) != 0)) {
                eDistinct = (int32)2 + bSortByGroup;
            } else {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_GROUPBY) != 0) {
                    eDistinct = (int32)1 - bSortByGroup;
                } else {
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0) {
                        eDistinct = (int32)3;
                    }
                }
            }
        }
    }
    pIdxInfo = _sqlite3DbMallocZero(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)((uint64)96 + (libc.Uint64FromInt64(12) + libc.Uint64FromInt64(8)) * libc.Uint64FromInt32(nTerm) + (uint64)8 * libc.Uint64FromInt32(nOrderBy) + ((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(nTerm) * (uint64)8)));
    if (pIdxInfo == (uintptr)0) {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 1702, 0);
        return (uintptr)0;
    }
    pHidden = pIdxInfo + 1 * 96;
    pIdxCons = pHidden + 32 + (uintptr)nTerm * 8;
    pIdxOrderBy = pIdxCons + (uintptr)nTerm * 12;
    pUsage = pIdxOrderBy + (uintptr)nOrderBy * 8;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint = pIdxCons;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaOrderBy = pIdxOrderBy;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage = pUsage;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FcolUsed = libc.Uint64FromInt64(libc.Int64FromUint64((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FcolUsed));
    if (libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) == 0) {
        pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol))) {
                break;
            }
            iCol = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2)));
            if (iCol >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)) {
                iCol = libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1);
            }
            (@unsafe.Pointer.To<Tsqlite3_uint64>(__ccgo_ptr(pIdxInfo + 88))).Value |= libc.Uint64FromInt32(1).Lsh((int64)(iCol));
            goto _8;
_8:;
            i = i + 1;
        }
    }
    (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FpWC = pWC;
    (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FpParse = pParse;
    (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FeDistinct = eDistinct;
    (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FmIn = (uint32)0;
    p = pWC;
    v10 = libc.Int32FromInt32(0);
    j = v10;
    i = v10;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        nLast = i + (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FnTerm;
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.Fa;
        for (;;) {
            if (!(i < nLast)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_OK) == 0) {
                goto _11;
            }
            (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.FiColumn = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn;
            (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.FiTermOffset = i;
            op = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_ALL);
            if (libc.Int32FromUint16(op) == (int32)WO_IN) {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_SLICE) == 0) {
                    if (j <= (int32)31) {
                        v12 = libc.Uint32FromInt32(1).Lsh((int64)(j));
                    } else {
                        v12 = (uint32)0;
                    }
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pHidden + 20))).Value |= v12;
                }
                op = (uint16)WO_EQ;
            }
            if (libc.Int32FromUint16(op) == (int32)WO_AUX) {
                (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.Fop = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeMatchOp;
            } else {
                if ((libc.Int32FromUint16(op) & (libc.Int32FromInt32(WO_ISNULL) | libc.Int32FromInt32(WO_IS))) != 0) {
                    if (libc.Int32FromUint16(op) == (int32)WO_ISNULL) {
                        (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.Fop = (uint8)SQLITE_INDEX_CONSTRAINT_ISNULL;
                    } else {
                        (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.Fop = (uint8)SQLITE_INDEX_CONSTRAINT_IS;
                    }
                } else {
                    (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.Fop = (uint8)op;
                    if ((libc.Int32FromUint16(op) & (libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) != 0 && _sqlite3ExprIsVector(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpRight) != 0) {
                        if (j < (int32)16) {
                            mNoOmit = libc.Uint16FromInt32((int32)mNoOmit | libc.Int32FromInt32(1).Lsh((int64)(j)));
                        }
                        if (libc.Int32FromUint16(op) == libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ)))) {
                            (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.Fop = libc.Uint8FromInt32(libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))));
                        }
                        if (libc.Int32FromUint16(op) == libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ)))) {
                            (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr(pIdxCons + (uintptr)j * 12))).Value.Fop = libc.Uint8FromInt32(libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))));
                        }
                    }
                }
            }
            j = j + 1;
            goto _11;
_11:;
            i = i + 1;
            pTerm += 56;
        }
        goto _9;
_9:;
        p = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)p)).Value.FpOuter;
    }
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint = j;
    v10 = libc.Int32FromInt32(0);
    j = v10;
    i = v10;
    for (;;) {
        if (!(i < nOrderBy)) {
            break;
        }
        pExpr1 = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr;
        if (_sqlite3ExprIsConstant(tls, (uintptr)0, pExpr1) != 0) {
            goto _13;
        }
        (@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr(pIdxOrderBy + (uintptr)j * 8))).Value.FiColumn = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr1)).Value.FiColumn;
        (@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr(pIdxOrderBy + (uintptr)j * 8))).Value.Fdesc = libc.Uint8FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.Ffg.FsortFlags) & (int32)KEYINFO_ORDER_DESC);
        j = j + 1;
        goto _13;
_13:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnOrderBy = j;
    (@unsafe.Pointer.To<Tu16>(pmNoOmit)).Value = mNoOmit;
    return pIdxInfo;
}

internal static void _analyzeOneTable(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, uintptr pOnlyIdx, int32 iStatCur, int32 iMem, int32 iTab) {
    uintptr aGotoChng = default;
    uintptr db = default;
    uintptr pColl = default;
    uintptr pIdx = default;
    uintptr pPk = default;
    uintptr pStat1 = default;
    uintptr pX = default;
    uintptr v = default;
    uintptr zIdxName = default;
    int32 addrGotoEnd = default;
    int32 addrIsNull = default;
    int32 addrNext = default;
    int32 addrNextRow = default;
    int32 doOnce = default;
    int32 endDistinctTest = default;
    int32 i = default;
    int32 iDb = default;
    int32 iIdxCur = default;
    int32 iTabCur = default;
    int32 j = default;
    int32 j1 = default;
    int32 j2 = default;
    int32 j3 = default;
    int32 jZeroRows = default;
    int32 k = default;
    int32 mxCol = default;
    int32 nCol = default;
    int32 nColTest = default;
    int32 nColX = default;
    int32 regChng = default;
    int32 regCol = default;
    int32 regDLt = default;
    int32 regEq = default;
    int32 regIdxname = default;
    int32 regKey = default;
    int32 regLt = default;
    int32 regNewRowid = default;
    int32 regPrev = default;
    int32 regRowid = default;
    int32 regSample = default;
    int32 regSampleRowid = default;
    int32 regStat = default;
    int32 regStat1 = default;
    int32 regTabname = default;
    int32 regTemp = default;
    int32 regTemp2 = default;
    int32 v1 = default;
    int32 v2 = default;
    int32 v3 = default;
    int32 v4 = default;
    int32 v5 = default;
    int32 v6 = default;
    int32 v7 = default;
    int32 v8 = default;
    int32 v9 = default;
    Tu8 needTableCnt = default;
    Tu8 seekOp = default;
    _ = aGotoChng;
    _ = addrGotoEnd;
    _ = addrIsNull;
    _ = addrNext;
    _ = addrNextRow;
    _ = db;
    _ = doOnce;
    _ = endDistinctTest;
    _ = i;
    _ = iDb;
    _ = iIdxCur;
    _ = iTabCur;
    _ = j;
    _ = j1;
    _ = j2;
    _ = j3;
    _ = jZeroRows;
    _ = k;
    _ = mxCol;
    _ = nCol;
    _ = nColTest;
    _ = nColX;
    _ = needTableCnt;
    _ = pColl;
    _ = pIdx;
    _ = pPk;
    _ = pStat1;
    _ = pX;
    _ = regChng;
    _ = regCol;
    _ = regDLt;
    _ = regEq;
    _ = regIdxname;
    _ = regKey;
    _ = regLt;
    _ = regNewRowid;
    _ = regPrev;
    _ = regRowid;
    _ = regSample;
    _ = regSampleRowid;
    _ = regStat;
    _ = regStat1;
    _ = regTabname;
    _ = regTemp;
    _ = regTemp2;
    _ = seekOp;
    _ = v;
    _ = zIdxName;
    _ = v1;
    _ = v2;
    _ = v3;
    _ = v4;
    _ = v5;
    _ = v6;
    _ = v7;
    _ = v8;
    _ = v9;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    jZeroRows = -(int32)1;
    needTableCnt = (uint8)1;
    v1 = iMem;
    iMem = iMem + 1;
    regNewRowid = v1;
    v2 = iMem;
    iMem = iMem + 1;
    regStat = v2;
    v3 = iMem;
    iMem = iMem + 1;
    regChng = v3;
    v4 = iMem;
    iMem = iMem + 1;
    regRowid = v4;
    v5 = iMem;
    iMem = iMem + 1;
    regTemp = v5;
    v6 = iMem;
    iMem = iMem + 1;
    regTemp2 = v6;
    v7 = iMem;
    iMem = iMem + 1;
    regTabname = v7;
    v8 = iMem;
    iMem = iMem + 1;
    regIdxname = v8;
    v9 = iMem;
    iMem = iMem + 1;
    regStat1 = v9;
    regPrev = iMem;
    doOnce = (int32)1;
    pStat1 = (uintptr)0;
    _sqlite3TouchRegister(ref (tls).DerefOrNull(), pParse, iMem);
    v = _sqlite3GetVdbe(tls, pParse);
    if (v == (uintptr)0 || pTab == (uintptr)0) {
        return;
    }
    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_NORM))) {
        return;
    }
    if (Xsqlite3_strlike(tls, __ccgo_ts + 13145, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (uint32)'\\') == 0) {
        return;
    }
    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
    if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_ANALYZE, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (uintptr)0, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName) != 0) {
        return;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxPreUpdateCallback != 0) {
        pStat1 = _sqlite3DbMallocZero(tls, db, (uint64)(libc.Uint64FromInt64(120) + libc.Uint64FromInt32(13)));
        if (pStat1 == (uintptr)0) {
            return;
        }
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pStat1)).Value.FzName = pStat1 + 1 * 120;
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pStat1)).Value.FzName, __ccgo_ts + 12948, (uint64)13);
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pStat1)).Value.FnCol = (int16)3;
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pStat1)).Value.FiPKey = (int16)(-(int32)1);
        _sqlite3VdbeAddOp4(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (int32)OP_Noop, 0, 0, 0, pStat1, -(int32)7);
    }
    _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum, (uint8)0, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
    v1 = iTab;
    iTab = iTab + 1;
    iTabCur = v1;
    v1 = iTab;
    iTab = iTab + 1;
    iIdxCur = v1;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab > iTab) {
        v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
    } else {
        v1 = iTab;
    }
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab = v1;
    _sqlite3OpenTable(tls, pParse, iTabCur, iDb, pTab, (int32)OP_OpenRead);
    _sqlite3VdbeLoadString(tls, v, regTabname, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(pIdx != 0)) {
            break;
        }
        if (pOnlyIdx != 0 && pOnlyIdx != pIdx) {
            goto _13;
        }
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere == (uintptr)0) {
            needTableCnt = (uint8)0;
        }
        if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
            nCol = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
            zIdxName = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName;
            nColTest = nCol - (int32)1;
        } else {
            nCol = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
            zIdxName = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzName;
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x8) >> 3) != 0) {
                v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol) - (int32)1;
            } else {
                v1 = nCol - (int32)1;
            }
            nColTest = v1;
        }
        _sqlite3VdbeLoadString(tls, v, regIdxname, zIdxName);
        _sqlite3TouchRegister(ref (tls).DerefOrNull(), pParse, regPrev + nColTest);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenRead, iIdxCur, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum), iDb);
        _sqlite3VdbeSetP4KeyInfo(tls, pParse, pIdx);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnAnalysisLimit, regTemp2);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, nCol, regStat + (int32)1);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol), regRowid);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Count, iIdxCur, regTemp, libc.BoolInt32((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Stat4))) != (uint32)0));
        _sqlite3VdbeAddFunctionCall(tls, pParse, 0, regStat + (int32)1, regStat, (int32)4, (uintptr)Ꮡ_statInitFuncdef, 0);
        addrGotoEnd = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, iIdxCur);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regChng);
        addrNextRow = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        if (nColTest > 0) {
            endDistinctTest = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            aGotoChng = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)4 * libc.Uint64FromInt32(nColTest)));
            if (aGotoChng == (uintptr)0) {
                goto _13;
            }
            _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
            addrNextRow = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            if (nColTest == (int32)1 && libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol) == (int32)1 && libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError) != OE_None) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_NotNull, regPrev, endDistinctTest);
            }
            i = 0;
            for (;;) {
                if (!(i < nColTest)) {
                    break;
                }
                pColl = _sqlite3LocateCollSeq(tls, pParse, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)i * 8)));
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, i, regChng);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iIdxCur, i, regTemp);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aGotoChng + (uintptr)i * 4))).Value = _sqlite3VdbeAddOp4(tls, v, (int32)OP_Ne, regTemp, 0, regPrev + i, pColl, -(int32)2);
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NULLEQ);
                goto _15;
_15:;
                i = i + 1;
            }
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, nColTest, regChng);
            _sqlite3VdbeGoto(tls, v, endDistinctTest);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrNextRow - (int32)1);
            i = 0;
            for (;;) {
                if (!(i < nColTest)) {
                    break;
                }
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aGotoChng + (uintptr)i * 4)));
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iIdxCur, i, regPrev + i);
                goto _16;
_16:;
                i = i + 1;
            }
            _sqlite3VdbeResolveLabel(tls, v, endDistinctTest);
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aGotoChng);
        }
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Stat4))) == (uint32)0) {
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IdxRowid, iIdxCur, regRowid);
            } else {
                pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable);
                regKey = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol));
                j = 0;
                for (;;) {
                    if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol))) {
                        break;
                    }
                    k = _sqlite3TableColumnToIndex(ref (tls).DerefOrNull(), pIdx, (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)j * 2))));
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iIdxCur, k, regKey + j);
                    goto _17;
_17:;
                    j = j + 1;
                }
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regKey, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol), regRowid);
                _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, regKey, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol));
            }
        }
        _sqlite3VdbeAddFunctionCall(tls, pParse, (int32)1, regStat, regTemp, libc.Int32FromInt32(2) + libc.Int32FromInt32(IsStat4), (uintptr)Ꮡ_statPushFuncdef, 0);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnAnalysisLimit != 0) {
            j1 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IsNull, regTemp);
            j2 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_If, regTemp);
            j3 = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_SeekGT, iIdxCur, 0, regPrev, (int32)1);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, j1);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iIdxCur, addrNextRow);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, j2);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, j3);
        } else {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iIdxCur, addrNextRow);
        }
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != 0) {
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrGotoEnd);
            addrGotoEnd = 0;
        }
        _callStatGet(tls, pParse, regStat, STAT_GET_STAT1, regStat1);
        _sqlite3VdbeAddOp4(tls, v, (int32)OP_MakeRecord, regTabname, (int32)3, regTemp, __ccgo_ts + 13155, 0);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, iStatCur, regNewRowid);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, iStatCur, regTemp, regNewRowid);
        _sqlite3VdbeChangeP4(tls, v, -(int32)1, pStat1, -(int32)5);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_APPEND);
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Stat4))) == (uint32)0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnAnalysisLimit == 0) {
            regEq = regStat1;
            regLt = regStat1 + (int32)1;
            regDLt = regStat1 + (int32)2;
            regSample = regStat1 + (int32)3;
            regCol = regStat1 + (int32)4;
            regSampleRowid = regCol + nCol;
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                v1 = (int32)OP_NotExists;
            } else {
                v1 = (int32)OP_NotFound;
            }
            seekOp = libc.Uint8FromInt32(v1);
            if (addrGotoEnd == 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Cast, regStat1, (int32)SQLITE_AFF_INTEGER);
                addrGotoEnd = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IfNot, regStat1);
            }
            if (doOnce != 0) {
                mxCol = nCol;
                pX = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
                for (;;) {
                    if (!(pX != 0)) {
                        break;
                    }
                    if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pX + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
                        nColX = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pX)).Value.FnKeyCol);
                    } else {
                        nColX = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pX)).Value.FnColumn);
                    }
                    if (nColX > mxCol) {
                        mxCol = nColX;
                    }
                    goto _19;
_19:;
                    pX = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pX)).Value.FpNext;
                }
                _sqlite3TouchRegister(ref (tls).DerefOrNull(), pParse, regCol + mxCol);
                doOnce = 0;
                _sqlite3ClearTempRegCache(ref (tls).DerefOrNull(), pParse);
            }
            addrNext = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            _callStatGet(tls, pParse, regStat, (int32)STAT_GET_ROWID, regSampleRowid);
            addrIsNull = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IsNull, regSampleRowid);
            _callStatGet(tls, pParse, regStat, (int32)STAT_GET_NEQ, regEq);
            _callStatGet(tls, pParse, regStat, (int32)STAT_GET_NLT, regLt);
            _callStatGet(tls, pParse, regStat, (int32)STAT_GET_NDLT, regDLt);
            _sqlite3VdbeAddOp4Int(tls, v, libc.Int32FromUint8(seekOp), iTabCur, addrNext, regSampleRowid, 0);
            i = 0;
            for (;;) {
                if (!(i < nCol)) {
                    break;
                }
                _sqlite3ExprCodeLoadIndexColumn(tls, pParse, pIdx, iTabCur, i, regCol + i);
                goto _20;
_20:;
                i = i + 1;
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regCol, nCol, regSample);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regTabname, (int32)6, regTemp);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, iStatCur + (int32)1, regNewRowid);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, iStatCur + (int32)1, regTemp, regNewRowid);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, (int32)1, addrNext);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrIsNull);
        }
        if (addrGotoEnd != 0) {
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrGotoEnd);
        }
        goto _13;
_13:;
        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
    }
    if (pOnlyIdx == (uintptr)0 && needTableCnt != 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Count, iTabCur, regStat1);
        jZeroRows = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IfNot, regStat1);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regIdxname);
        _sqlite3VdbeAddOp4(tls, v, (int32)OP_MakeRecord, regTabname, (int32)3, regTemp, __ccgo_ts + 13155, 0);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, iStatCur, regNewRowid);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, iStatCur, regTemp, regNewRowid);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_APPEND);
        _sqlite3VdbeChangeP4(tls, v, -(int32)1, pStat1, -(int32)5);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, jZeroRows);
    }
}

internal static void _attachFunc(ж<libc.TLS> tls, uintptr context, int32 NotUsed, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr aNew = default;
        uintptr db = default;
        uintptr pNew = default;
        ref var pNewBt = ref heap(new uintptr(), out var ᏑpNewBt);
        uintptr pNewSchema = default;
        uintptr pPager = default;
        ref var pVfs = ref heap(new uintptr(), out var ᏑpVfs);
        ref var zErr = ref heap(new uintptr(), out var ᏑzErr);
        uintptr zFile = default;
        uintptr zName = default;
        ref var zPath = ref heap(new uintptr(), out var ᏑzPath);
        ref var flags = ref heap(new uint32(), out var Ꮡflags);
        int32 i = default;
        int32 iDb = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = aNew;
        _ = db;
        _ = flags;
        _ = i;
        _ = iDb;
        _ = pNew;
        _ = pNewBt;
        _ = pNewSchema;
        _ = pPager;
        _ = pVfs;
        _ = rc;
        _ = zErr;
        _ = zFile;
        _ = zName;
        _ = zPath;
        rc = 0;
        db = Xsqlite3_context_db_handle(tls, context);
        zPath = (uintptr)0;
        zErr = (uintptr)0;
        pNew = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
        _ = NotUsed;
        zFile = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        zName = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
        if (zFile == (uintptr)0) {
            zFile = __ccgo_ts + 1732;
        }
        if (zName == (uintptr)0) {
            zName = __ccgo_ts + 1732;
        }
        if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x8) >> 3) != 0) {
            pNewBt = (uintptr)0;
            pNew = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb * 32;
            if (_sqlite3BtreeTxnState(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt) != SQLITE_TXN_NONE || _sqlite3BtreeIsInBackup(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt) != 0) {
                rc = (int32)SQLITE_BUSY;
                goto attach_error;
            }
            pVfs = Xsqlite3_vfs_find(tls, __ccgo_ts + 4154);
            if (pVfs == (uintptr)0) {
                return;
            }
            rc = _sqlite3BtreeOpen(tls, pVfs, __ccgo_ts + 13362, db, @unsafe.Pointer.FromBox(ᏑpNewBt), 0, (int32)SQLITE_OPEN_MAIN_DB);
            if (rc == SQLITE_OK) {
                pNewSchema = _sqlite3SchemaGet(tls, db, pNewBt);
                if (pNewSchema != 0) {
                    _detachTempTriggers(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpSchema);
                    _sqlite3BtreeClose(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt);
                    (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt = pNewBt;
                    (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpSchema = pNewSchema;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb) == 0) {
                        _sqlite3VtabEponymousTableClearAll(tls, db);
                    }
                    _sqlite3ExpirePreparedStatements(ref (tls).DerefOrNull(), db, (int32)1);
                } else {
                    _sqlite3BtreeClose(tls, pNewBt);
                    rc = (int32)SQLITE_NOMEM;
                }
            }
            if (rc != 0) {
                goto attach_error;
            }
        } else {
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb >= ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + 7 * 4)) + (int32)2) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 13365, libc.VaList(bp + 16, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + 7 * 4))));
                goto attach_error;
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
                    break;
                }
                if (_sqlite3DbIsNamed(ref (tls).DerefOrNull(), db, i, zName) != 0) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 13402, libc.VaList(bp + 16, zName));
                    goto attach_error;
                }
                goto _1;
_1:;
                i = i + 1;
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb == db + 704) {
                aNew = _sqlite3DbMallocRawNN(tls, db, (uint64)(libc.Uint64FromInt64(32) * libc.Uint64FromInt32(3)));
                if (aNew == (uintptr)0) {
                    return;
                }
                libc.Xmemcpy(tls, aNew, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb, libc.Uint64FromInt64(32) * libc.Uint64FromInt32(2));
            } else {
                aNew = _sqlite3DbRealloc(tls, db, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb, (uint64)32 * libc.Uint64FromInt64(libc.Int64FromInt32(1) + (int64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb));
                if (aNew == (uintptr)0) {
                    return;
                }
            }
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb = aNew;
            pNew = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb * 32;
            libc.Xmemset(tls, pNew, 0, (uint64)32);
            flags = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FopenFlags;
            rc = _sqlite3ParseUri(tls, (@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVfs)).Value.FzName, zFile, @unsafe.Pointer.FromPinnedBox(Ꮡflags), @unsafe.Pointer.FromBox(ᏑpVfs), @unsafe.Pointer.FromBox(ᏑzPath), @unsafe.Pointer.FromBox(ᏑzErr));
            if (rc != SQLITE_OK) {
                if (rc == (int32)SQLITE_NOMEM) {
                    _sqlite3OomFault(tls, db);
                }
                Xsqlite3_result_error(tls, context, zErr, -(int32)1);
                Xsqlite3_free(tls, zErr);
                return;
            }
            if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00020)).Lsh((int64)(libc.Int32FromInt32(32))))) == (uint64)0) {
                flags = flags & libc.Uint32FromInt32(~(libc.Int32FromInt32(SQLITE_OPEN_CREATE) | libc.Int32FromInt32(SQLITE_OPEN_READWRITE)));
                flags = flags | (uint32)SQLITE_OPEN_READONLY;
            } else {
                if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00010)).Lsh((int64)(libc.Int32FromInt32(32))))) == (uint64)0) {
                    flags = flags & libc.Uint32FromInt32(~libc.Int32FromInt32(SQLITE_OPEN_CREATE));
                }
            }
            flags = flags | (uint32)SQLITE_OPEN_MAIN_DB;
            rc = _sqlite3BtreeOpen(tls, pVfs, zPath, db, (@unsafe.Pointer)(pNew + 8), 0, libc.Int32FromUint32(flags));
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb + 1;
            (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FzDbSName = _sqlite3DbStrDup(tls, db, zName);
        }
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnoSharedCache = (uint8)0;
        if (rc == (int32)SQLITE_CONSTRAINT) {
            rc = (int32)SQLITE_ERROR;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 13432, 0);
        } else {
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpSchema = _sqlite3SchemaGet(tls, db, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt);
                if (!((@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpSchema != 0)) {
                    rc = (int32)SQLITE_NOMEM;
                } else {
                    if ((@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpSchema)).Value.Ffile_format != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpSchema)).Value.Fenc) != libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc)) {
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 13461, 0);
                        rc = (int32)SQLITE_ERROR;
                    }
                }
                _sqlite3BtreeEnter(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt);
                pPager = _sqlite3BtreePager(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt);
                _sqlite3PagerLockingMode(ref (tls).DerefOrNull(), pPager, libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdfltLockMode));
                _sqlite3BtreeSecureDelete(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt, _sqlite3BtreeSecureDelete(tls, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb))).Value.FpBt, -(int32)1));
                _sqlite3BtreeSetPagerFlags(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt, (uint32)((uint64)PAGER_SYNCHRONOUS_FULL | (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)PAGER_FLAGS_MASK)));
                _sqlite3BtreeLeave(tls, (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FpBt);
            }
        }
        (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.Fsafety_level = libc.Uint8FromInt32(libc.Int32FromInt32(SQLITE_DEFAULT_SYNCHRONOUS) + libc.Int32FromInt32(1));
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pNew)).Value.FzDbSName == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        }
        Xsqlite3_free_filename(tls, zPath);
        if (rc == SQLITE_OK) {
            _sqlite3BtreeEnterAll(tls, db);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb = (uint8)0;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 44))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(DBFLAG_SchemaKnownOk));
            if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x8) >> 3) != 0)) {
                rc = _sqlite3Init(tls, db, bp);
            }
            _sqlite3BtreeLeaveAll(tls, db);
        }
        if (rc != 0) {
            if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x8) >> 3) != 0)) {
                iDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb - (int32)1;
                if ((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt != 0) {
                    _sqlite3BtreeClose(tls, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt);
                    (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt = (uintptr)0;
                    (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema = (uintptr)0;
                }
                _sqlite3ResetAllSchemasOfConnection(tls, db);
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb = iDb;
                if (rc == (int32)SQLITE_NOMEM || rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8))))) {
                    _sqlite3OomFault(tls, db);
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 1702, 0);
                } else {
                    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) == (uintptr)0) {
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 13529, libc.VaList(bp + 16, zFile));
                    }
                }
            }
            goto attach_error;
        }
        return;
        goto attach_error;
attach_error:;
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) != 0) {
            Xsqlite3_result_error(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), -(int32)1);
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        }
        if (rc != 0) {
            Xsqlite3_result_error_code(tls, context, rc);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _cannotBeFunction(ж<libc.TLS> tls, uintptr pParse, uintptr pFrom) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x8) >> 3) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21169, libc.VaList(bp + 8, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FzName));
            r = (int32)1; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _checkTreePage(ж<libc.TLS> tls, uintptr pCheck, TPgno iPage, @unsafe.Pointer piMinKey, Ti64 maxKeyʗp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        ref var maxKey = ref builtin.heap(maxKeyʗp, out var ᏑmaxKey);

        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 cellStart = default;
        int32 d2 = default;
        int32 depth = default;
        int32 doCoverageCheck = default;
        int32 hdr = default;
        int32 i = default;
        int32 j = default;
        int32 keyCanBeEqual = default;
        int32 nCell = default;
        int32 nFrag = default;
        int32 pgno = default;
        int32 rc = default;
        int32 saved_v1 = default;
        int32 saved_v2 = default;
        int32 size1 = default;
        int32 v1 = default;
        Tu32 contentOffset = default;
        Tu32 nPage = default;
        Tu32 pc = default;
        Tu32 prev = default;
        Tu32 size = default;
        Tu32 usableSize = default;
        ref var x = ref builtin.heap(new Tu32(), out var Ꮡx);
        uintptr data = default;
        uintptr heap = default;
        uintptr pBt = default;
        uintptr pCell = default;
        uintptr pCellIdx = default;
        ref var pPage = ref builtin.heap(new uintptr(), out var ᏑpPage);
        uintptr saved_zPfx = default;
        TPgno pgnoOvfl = default;
        Tu8 savedIsInit = default;
        TCellInfo _ᴛ1 = default;
        _ = cellStart;
        _ = contentOffset;
        _ = d2;
        _ = data;
        _ = depth;
        _ = doCoverageCheck;
        _ = hdr;
        _ = heap;
        _ = i;
        _ = j;
        _ = keyCanBeEqual;
        _ = nCell;
        _ = nFrag;
        _ = nPage;
        _ = pBt;
        _ = pCell;
        _ = pCellIdx;
        _ = pPage;
        _ = pc;
        _ = pgno;
        _ = pgnoOvfl;
        _ = prev;
        _ = rc;
        _ = savedIsInit;
        _ = saved_v1;
        _ = saved_v2;
        _ = saved_zPfx;
        _ = size;
        _ = size1;
        _ = usableSize;
        _ = x;
        _ = v1;
        pPage = (uintptr)0;
        depth = -(int32)1;
        doCoverageCheck = (int32)1;
        keyCanBeEqual = (int32)1;
        heap = (uintptr)0;
        prev = (uint32)0;
        saved_zPfx = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FzPfx;
        saved_v1 = libc.Int32FromUint32((@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fv1);
        saved_v2 = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fv2;
        savedIsInit = (uint8)0;
        _checkProgress(tls, pCheck);
        if ((@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FmxErr == 0) {
            r = 0; goto ᒐdone;
        }
        pBt = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpBt;
        usableSize = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize;
        if (iPage == (uint32)0) {
            r = 0; goto ᒐdone;
        }
        if (_checkRef(tls, pCheck, iPage) != 0) {
            r = 0; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FzPfx = __ccgo_ts + 4640;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fv1 = iPage;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnAbove = (Tu8)((@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnAbove + 1);
        v1 = _btreeGetPage(tls, pBt, iPage, @unsafe.Pointer.FromBox(ᏑpPage), 0);
        rc = v1;
        if (v1 != 0) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4658, libc.VaList(bp + 32, rc));
            if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8))))) {
                (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Frc = (int32)SQLITE_NOMEM;
            }
            goto end_of_check;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnAbove) > (int32)BTCURSOR_MAX_DEPTH) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4696, libc.VaList(bp + 32, (int32)BTCURSOR_MAX_DEPTH));
            goto end_of_check;
        }
        savedIsInit = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit;
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = (uint8)0;
        v1 = _btreeInitPage(tls, pPage);
        rc = v1;
        if (v1 != 0) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4719, libc.VaList(bp + 32, rc));
            goto end_of_check;
        }
        v1 = _btreeComputeFreeSpace(tls, pPage);
        rc = v1;
        if (v1 != 0) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4757, libc.VaList(bp + 32, rc));
            goto end_of_check;
        }
        data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
        hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset);
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FzPfx = __ccgo_ts + 4779;
        contentOffset = libc.Uint32FromInt32((((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5) + 1)))) - libc.Int32FromInt32(1)) & libc.Int32FromInt32(0xffff)) + libc.Int32FromInt32(1));
        nCell = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)3)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)3) + 1)));
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey) == 0) {
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pCheck + 120))).Value += (int64)nCell;
        }
        cellStart = hdr + (int32)12 - (int32)4 * libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf);
        pCellIdx = data + (uintptr)(cellStart + (int32)2 * (nCell - (int32)1));
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            pgno = libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(data + (uintptr)(hdr + (int32)8))));
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
                (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FzPfx = __ccgo_ts + 4805;
                _checkPtrmap(tls, pCheck, libc.Uint32FromInt32(pgno), (uint8)PTRMAP_BTREE, iPage);
            }
            depth = _checkTreePage(tls, pCheck, libc.Uint32FromInt32(pgno), @unsafe.Pointer.FromPinnedBox(ᏑmaxKey), maxKey);
            keyCanBeEqual = 0;
        } else {
            heap = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fheap;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(heap))).Value = (uint32)0;
        }
        i = nCell - (int32)1;
        for (;;) {
            if (!(i >= 0 && (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FmxErr != 0)) {
                break;
            }
            (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fv2 = i;
            pc = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCellIdx))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCellIdx + 1))));
            pCellIdx = pCellIdx - (uintptr)2;
            if (pc < contentOffset || pc > usableSize - (uint32)4) {
                _checkAppendMsg(tls, pCheck, __ccgo_ts + 4835, libc.VaList(bp + 32, pc, contentOffset, usableSize - (uint32)4));
                doCoverageCheck = 0;
                goto _4;
            }
            pCell = data + (uintptr)pc;
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, pCell, bp);
            if (pc + (uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize > usableSize) {
                _checkAppendMsg(tls, pCheck, __ccgo_ts + 4865, 0);
                doCoverageCheck = 0;
                goto _4;
            }
            if ((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload != 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FpPayload))) < (int32)2) {
                _checkAppendMsg(tls, pCheck, __ccgo_ts + 4889, 0);
                doCoverageCheck = 0;
                goto _4;
            }
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey != 0) {
                if (keyCanBeEqual != 0) {
                    v1 = libc.BoolInt32((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnKey > maxKey);
                } else {
                    v1 = libc.BoolInt32((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnKey >= maxKey);
                }
                if (v1 != 0) {
                    _checkAppendMsg(tls, pCheck, __ccgo_ts + 4910, libc.VaList(bp + 32, (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnKey));
                }
                maxKey = (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnKey;
                keyCanBeEqual = 0;
            }
            if ((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload > (uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal) {
                nPage = ((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload - (uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal + usableSize - (uint32)5) / (usableSize - (uint32)4);
                pgnoOvfl = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pCell + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize) - (int32)4)));
                if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
                    _checkPtrmap(tls, pCheck, pgnoOvfl, (uint8)PTRMAP_OVERFLOW1, iPage);
                }
                _checkList(tls, pCheck, 0, pgnoOvfl, nPage);
            }
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
                pgno = libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell));
                if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
                    _checkPtrmap(tls, pCheck, libc.Uint32FromInt32(pgno), (uint8)PTRMAP_BTREE, iPage);
                }
                d2 = _checkTreePage(tls, pCheck, libc.Uint32FromInt32(pgno), @unsafe.Pointer.FromPinnedBox(ᏑmaxKey), maxKey);
                keyCanBeEqual = 0;
                if (d2 != depth) {
                    _checkAppendMsg(tls, pCheck, __ccgo_ts + 4934, 0);
                    depth = d2;
                }
            } else {
                _btreeHeapInsert(ref (tls).DerefOrNull(), heap, (Tu32)(pc << (int)(libc.Int32FromInt32(16)) | (pc + (uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize - (uint32)1)));
            }
            goto _4;
_4:;
            i = i - 1;
        }
        (@unsafe.Pointer.To<Ti64>(piMinKey)).Value = maxKey;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FzPfx = (uintptr)0;
        if (doCoverageCheck != 0 && (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FmxErr > 0) {
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
                heap = (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fheap;
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(heap))).Value = (uint32)0;
                i = nCell - (int32)1;
                for (;;) {
                    if (!(i >= 0)) {
                        break;
                    }
                    pc = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(cellStart + i * (int32)2)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(cellStart + i * (int32)2) + 1))));
                    size = (uint32)GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxCellSize)(tls, pPage, data + (uintptr)pc);
                    _btreeHeapInsert(ref (tls).DerefOrNull(), heap, (Tu32)(pc << (int)(libc.Int32FromInt32(16)) | (pc + size - (uint32)1)));
                    goto _6;
_6:;
                    i = i - 1;
                }
            }
            i = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)1)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)1) + 1)));
            while (i > 0) {
                size1 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(i + (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(i + (int32)2) + 1)));
                _btreeHeapInsert(ref (tls).DerefOrNull(), heap, libc.Uint32FromInt32(i).Lsh((int64)(libc.Int32FromInt32(16))) | libc.Uint32FromInt32(i + size1 - libc.Int32FromInt32(1)));
                j = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)i))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)i + 1)));
                i = j;
            }
            nFrag = 0;
            prev = contentOffset - (uint32)1;
            while (_btreeHeapPull(ref (tls).DerefOrNull(), heap, @unsafe.Pointer.FromPinnedBox(Ꮡx)) != 0) {
                if ((Tu32)(prev & (uint32)0xffff) >= x >> (int)(libc.Int32FromInt32(16))) {
                    _checkAppendMsg(tls, pCheck, __ccgo_ts + 4959, libc.VaList(bp + 32, x >> (int)((int32)16), iPage));
                    break;
                } else {
                    nFrag = libc.Int32FromUint32((uint32)nFrag + ((x >> (int)(libc.Int32FromInt32(16))) - (Tu32)(prev & libc.Uint32FromInt32(0xffff)) - libc.Uint32FromInt32(1)));
                    prev = x;
                }
            }
            nFrag = libc.Int32FromUint32((uint32)nFrag + (usableSize - (Tu32)(prev & libc.Uint32FromInt32(0xffff)) - libc.Uint32FromInt32(1)));
            if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(heap)) == (uint32)0 && nFrag != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7))))) {
                _checkAppendMsg(tls, pCheck, __ccgo_ts + 4996, libc.VaList(bp + 32, nFrag, libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7)))), iPage));
            }
        }
        goto end_of_check;
end_of_check:;
        if (!(doCoverageCheck != 0)) {
            (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = savedIsInit;
        }
        _releasePage(tls, pPage);
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FzPfx = saved_zPfx;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fv1 = libc.Uint32FromInt32(saved_v1);
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.Fv2 = saved_v2;
        (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnAbove = (Tu8)((@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnAbove - 1);
        r = depth + (int32)1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _columnTypeImpl(ж<libc.TLS> tls, uintptr pNC, uintptr pExpr, @unsafe.Pointer pzOrigDb, @unsafe.Pointer pzOrigTab, @unsafe.Pointer pzOrigCol) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(112);
        defer(tls.Free, (nint)112, ref ᒐ);
        int32 iCol = default;
        int32 iDb = default;
        int32 j = default;
        uintptr p = default;
        uintptr p1 = default;
        uintptr pS = default;
        uintptr pS1 = default;
        uintptr pTab = default;
        uintptr pTabList = default;
        ref var zOrigCol = ref heap(new uintptr(), out var ᏑzOrigCol);
        ref var zOrigDb = ref heap(new uintptr(), out var ᏑzOrigDb);
        ref var zOrigTab = ref heap(new uintptr(), out var ᏑzOrigTab);
        uintptr zType = default;
        TNameContext _ᴛ1 = new();
        TNameContext _ᴛ2 = new();
        _ = iCol;
        _ = iDb;
        _ = j;
        _ = p;
        _ = p1;
        _ = pS;
        _ = pS1;
        _ = pTab;
        _ = pTabList;
        _ = zOrigCol;
        _ = zOrigDb;
        _ = zOrigTab;
        _ = zType;
        zType = (uintptr)0;
        zOrigDb = (uintptr)0;
        zOrigTab = (uintptr)0;
        zOrigCol = (uintptr)0;
        switch (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop)) {
        case var ᴛ when ᴛ == (int32)TK_COLUMN: {
            pTab = (uintptr)0;
            pS = (uintptr)0;
            iCol = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn;
            while (pNC != 0 && !(pTab != 0)) {
                pTabList = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpSrcList;
                j = 0;
                for (;;) {
                    if (!(j < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc && (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8 + (uintptr)j * 80))).Value.FiCursor != (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable)) {
                        break;
                    }
                    goto _1;
_1:;
                    j = j + 1;
                }
                if (j < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc) {
                    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8 + (uintptr)j * 80))).Value.FpSTab;
                    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabList + 8 + (uintptr)j * 80 + 24 + 4)) & 0x4) >> 2) != 0) {
                        pS = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTabList + 8 + (uintptr)j * 80 + 72))))).Value.FpSelect;
                    } else {
                        pS = (uintptr)0;
                    }
                } else {
                    pNC = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpNext;
                }
            }
            if (pTab == (uintptr)0) {
                break;
            }
            if (pS != 0) {
                if (iCol < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpEList)).Value.FnExpr && libc.Bool(libc.Bool(!(libc.Int32FromInt32(ViewCanHaveRowid) != 0)) || iCol >= 0)) {
                    p = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpEList + 8 + (uintptr)iCol * 32))).Value.FpExpr;
                    (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS)).Value.FpSrc;
                    (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpNext = pNC;
                    (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpParse;
                    zType = _columnTypeImpl(tls, bp, p, @unsafe.Pointer.FromBox(ᏑzOrigDb), @unsafe.Pointer.FromBox(ᏑzOrigTab), @unsafe.Pointer.FromBox(ᏑzOrigCol));
                }
            } else {
                if (iCol < 0) {
                    iCol = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey;
                }
                if (iCol < 0) {
                    zType = __ccgo_ts + 1198;
                    zOrigCol = __ccgo_ts + 18076;
                } else {
                    zOrigCol = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16))).Value.FzCnName;
                    zType = _sqlite3ColumnType(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16, (uintptr)0);
                }
                zOrigTab = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName;
                if ((@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpParse != 0 && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema != 0) {
                    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpParse)).Value.Fdb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
                    zOrigDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpParse)).Value.Fdb)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
                }
            }
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_SELECT: {
            pS1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32));
            p1 = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS1)).Value.FpEList + 8))).Value.FpExpr;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 56))).Value.FpSrcList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pS1)).Value.FpSrc;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 56))).Value.FpNext = pNC;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 56))).Value.FpParse = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpParse;
            zType = _columnTypeImpl(tls, bp + 56, p1, @unsafe.Pointer.FromBox(ᏑzOrigDb), @unsafe.Pointer.FromBox(ᏑzOrigTab), @unsafe.Pointer.FromBox(ᏑzOrigCol));
            break;
            break;
        }}

        if (pzOrigDb != nil) {
            (@unsafe.Pointer.To<uintptr>(pzOrigDb)).Value = zOrigDb;
            (@unsafe.Pointer.To<uintptr>(pzOrigTab)).Value = zOrigTab;
            (@unsafe.Pointer.To<uintptr>(pzOrigCol)).Value = zOrigCol;
        }
        r = zType;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _constructAutomaticIndex(ж<libc.TLS> tls, uintptr pParse, uintptr pWC, TBitmask notReady, uintptr pLevel) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 addrCounter = default;
        int32 addrInit = default;
        int32 addrTop = default;
        int32 i = default;
        int32 iCol = default;
        int32 iCol1 = default;
        int32 iContinue = default;
        int32 mxBitCol = default;
        int32 n = default;
        int32 nKeyCol = default;
        int32 regBase = default;
        int32 regRecord = default;
        int32 regYield = default;
        int32 v3 = default;
        TBitmask cMask = default;
        TBitmask cMask1 = default;
        TBitmask extraCols = default;
        TBitmask idxCols = default;
        uintptr pColl = default;
        uintptr pExpr = default;
        uintptr pIdx = default;
        uintptr pLoop = default;
        uintptr pPartial = default;
        uintptr pSrc = default;
        uintptr pSubq = default;
        uintptr pTabList = default;
        uintptr pTable = default;
        uintptr pTerm = default;
        uintptr pWCEnd = default;
        uintptr pX = default;
        uintptr v = default;
        ref var zNotUsed = ref heap(new uintptr(), out var ᏑzNotUsed);
        uintptr v10 = default;
        Tu8 sentWarning = default;
        Tu8 useBloomFilter = default;
        uint64 v2 = default;
        Tu16 v4 = default;
        _ = addrCounter;
        _ = addrInit;
        _ = addrTop;
        _ = cMask;
        _ = cMask1;
        _ = extraCols;
        _ = i;
        _ = iCol;
        _ = iCol1;
        _ = iContinue;
        _ = idxCols;
        _ = mxBitCol;
        _ = n;
        _ = nKeyCol;
        _ = pColl;
        _ = pExpr;
        _ = pIdx;
        _ = pLoop;
        _ = pPartial;
        _ = pSrc;
        _ = pSubq;
        _ = pTabList;
        _ = pTable;
        _ = pTerm;
        _ = pWCEnd;
        _ = pX;
        _ = regBase;
        _ = regRecord;
        _ = regYield;
        _ = sentWarning;
        _ = useBloomFilter;
        _ = v;
        _ = zNotUsed;
        _ = v10;
        _ = v2;
        _ = v3;
        _ = v4;
        sentWarning = (uint8)0;
        useBloomFilter = (uint8)1;
        pPartial = (uintptr)0;
        iContinue = 0;
        addrCounter = 0;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        addrInit = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Once);
        nKeyCol = 0;
        pTabList = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo)).Value.FpTabList;
        pSrc = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
        pTable = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab;
        pWCEnd = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm * 56;
        pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
        idxCols = (uint64)0;
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
        for (;;) {
            if (!(pTerm < pWCEnd)) {
                break;
            }
            pExpr = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_VIRTUAL) == 0 && _sqlite3ExprIsSingleTableConstraint(tls, pExpr, pTabList, libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom), 0) != 0) {
                pPartial = _sqlite3ExprAnd(tls, pParse, pPartial, _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr, 0));
            }
            if (_termCanDriveIndex(tls, pTerm, pSrc, notReady) != 0) {
                iCol = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn;
                if (iCol >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))) {
                    v2 = libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)));
                } else {
                    v2 = libc.Uint64FromInt32(1).Lsh((int64)(iCol));
                }
                cMask = v2;
                if (!(sentWarning != 0)) {
                    Xsqlite3_log(tls, libc.Int32FromInt32(SQLITE_WARNING) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 24154, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FzName, (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FaCol + (uintptr)iCol * 16))).Value.FzCnName));
                    sentWarning = (uint8)1;
                }
                if ((TBitmask)(idxCols & cMask) == (uint64)0) {
                    if (_whereLoopResize(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pLoop, nKeyCol + (int32)1) != 0) {
                        goto end_auto_index_create;
                    }
                    v3 = nKeyCol;
                    nKeyCol = nKeyCol + 1;
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)v3 * 8))).Value = pTerm;
                    idxCols = (TBitmask)(idxCols | cMask);
                }
            }
            goto _1;
_1:;
            pTerm += 56;
        }
        v4 = libc.Uint16FromInt32(nKeyCol);
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnLTerm = v4;
        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq = v4;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags = libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_EQ) | libc.Int32FromInt32(WHERE_IDX_ONLY) | libc.Int32FromInt32(WHERE_INDEXED) | libc.Int32FromInt32(WHERE_AUTO_INDEX));
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FeTabType) == (int32)TABTYP_VIEW) {
            extraCols = libc.Uint64FromInt32(-libc.Int32FromInt32(1)) & ~idxCols;
        } else {
            extraCols = (TBitmask)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FcolUsed & ((TBitmask)(~idxCols | libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1))))));
        }
        if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
            i = 0;
            for (;;) {
                if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnCol)) {
                    break;
                }
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_PRIMKEY) == 0) {
                    goto _5;
                }
                if (i >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)) {
                    extraCols = (TBitmask)(extraCols | libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1))));
                    break;
                }
                if ((TBitmask)(idxCols & (libc.Uint64FromInt32(1).Lsh((int64)(i)))) != 0) {
                    goto _5;
                }
                extraCols = (TBitmask)(extraCols | libc.Uint64FromInt32(1).Lsh((int64)(i)));
                goto _5;
_5:;
                i = i + 1;
            }
        }
        if (libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1) < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnCol) {
            v3 = libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1);
        } else {
            v3 = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnCol;
        }
        mxBitCol = v3;
        i = 0;
        for (;;) {
            if (!(i < mxBitCol)) {
                break;
            }
            if ((TBitmask)(extraCols & (libc.Uint64FromInt32(1).Lsh((int64)(i)))) != 0) {
                nKeyCol = nKeyCol + 1;
            }
            goto _7;
_7:;
            i = i + 1;
        }
        if ((TBitmask)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FcolUsed & (libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1))))) != 0) {
            nKeyCol = nKeyCol + ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnCol - libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) + (int32)1);
        }
        pIdx = _sqlite3AllocateIndexObject(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, nKeyCol + libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0), 0, @unsafe.Pointer.FromBox(ᏑzNotUsed));
        if (pIdx == (uintptr)0) {
            goto end_auto_index_create;
        }
        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex = pIdx;
        (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzName = __ccgo_ts + 24180;
        (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable = pTable;
        n = 0;
        idxCols = (uint64)0;
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
        for (;;) {
            if (!(pTerm < pWCEnd)) {
                break;
            }
            if (_termCanDriveIndex(tls, pTerm, pSrc, notReady) != 0) {
                iCol1 = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn;
                if (iCol1 >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))) {
                    v2 = libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)));
                } else {
                    v2 = libc.Uint64FromInt32(1).Lsh((int64)(iCol1));
                }
                cMask1 = v2;
                if ((TBitmask)(idxCols & cMask1) == (uint64)0) {
                    pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
                    idxCols = (TBitmask)(idxCols | cMask1);
                    (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)n * 2))).Value = (int16)(@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn;
                    pColl = _sqlite3ExprCompareCollSeq(tls, pParse, (@unsafe.Pointer)pX);
                    if (!(_sqlite3IsBinary(ref (tls).DerefOrNull(), pColl) != 0)) {
                        useBloomFilter = (uint8)0;
                    }
                    if (pColl != 0) {
                        v10 = (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FzName;
                    } else {
                        v10 = (uintptr)Ꮡ_sqlite3StrBINARY;
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)n * 8))).Value = v10;
                    n = n + 1;
                }
            }
            goto _8;
_8:;
            pTerm += 56;
        }
        i = 0;
        for (;;) {
            if (!(i < mxBitCol)) {
                break;
            }
            if ((TBitmask)(extraCols & (libc.Uint64FromInt32(1).Lsh((int64)(i)))) != 0) {
                (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)n * 2))).Value = (int16)i;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)n * 8))).Value = (uintptr)Ꮡ_sqlite3StrBINARY;
                n = n + 1;
            }
            goto _11;
_11:;
            i = i + 1;
        }
        if ((TBitmask)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FcolUsed & (libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1))))) != 0) {
            i = libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1);
            for (;;) {
                if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnCol)) {
                    break;
                }
                (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)n * 2))).Value = (int16)i;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)n * 8))).Value = (uintptr)Ꮡ_sqlite3StrBINARY;
                n = n + 1;
                goto _12;
_12:;
                i = i + 1;
            }
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
            (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)n * 2))).Value = (int16)(-libc.Int32FromInt32(1));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)n * 8))).Value = (uintptr)Ꮡ_sqlite3StrBINARY;
        }
        v10 = pParse + 56;
        v3 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v10);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v10)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v10) + 1;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur = v3;
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenAutoindex, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, nKeyCol + (int32)1);
        _sqlite3VdbeSetP4KeyInfo(tls, pParse, pIdx);
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_BloomFilter))) == (uint32)0 && useBloomFilter != 0) {
            _sqlite3WhereExplainBloomFilter(tls, pParse, (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo, pLevel);
            v10 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v10)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v10) + 1;
            v3 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v10);
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter = v3;
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Blob, (int32)10000, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter);
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40) >> 6) != 0) {
            pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 72));
            regYield = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn;
            addrCounter = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, 0);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_InitCoroutine, regYield, 0, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub);
            addrTop = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Yield, regYield);
        } else {
            addrTop = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt);
        }
        if (pPartial != 0) {
            iContinue = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            _sqlite3ExprIfFalse(tls, pParse, pPartial, iContinue, (int32)SQLITE_JUMPIFNULL);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLoop + 48))).Value |= (uint32)WHERE_PARTIALIDX;
        }
        regRecord = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
        regBase = _sqlite3GenerateIndexKey(tls, pParse, pIdx, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, regRecord, 0, nil, (uintptr)0, 0);
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter != 0) {
            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_FilterAdd, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter, 0, regBase, libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq));
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_IdxInsert, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, regRecord);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_USESEEKRESULT);
        if (pPartial != 0) {
            _sqlite3VdbeResolveLabel(tls, v, iContinue);
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40) >> 6) != 0) {
            _sqlite3VdbeChangeP2(ref (tls).DerefOrNull(), v, addrCounter, regBase + n);
            _translateColumnToCopy(ref (tls).DerefOrNull(), pParse, addrTop, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 72))))).Value.FregResult, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur);
            _sqlite3VdbeGoto(tls, v, addrTop);
            libc.SetBitFieldPtr32Uint32(pSrc + 24 + 4, libc.Uint32FromInt32(0), 6, 0x40);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrTop);
        } else {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, addrTop + (int32)1);
            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_STMTSTATUS_AUTOINDEX);
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype) & (int32)JT_LEFT) != 0) {
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrTop);
            }
        }
        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regRecord);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrInit);
        goto end_auto_index_create;
end_auto_index_create:;
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pPartial);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _countOfViewOptimization(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    uintptr db = default;
    uintptr pCount = default;
    uintptr pExpr = default;
    uintptr pFrom = default;
    uintptr pPrior = default;
    uintptr pSub = default;
    uintptr pTerm = default;
    uintptr v1 = default;
    _ = db;
    _ = pCount;
    _ = pExpr;
    _ = pFrom;
    _ = pPrior;
    _ = pSub;
    _ = pTerm;
    _ = v1;
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Aggregate) == (uint32)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr != (int32)1) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != 0) {
        return 0;
    }
    pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList + 8))).Value.FpExpr;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_AGG_FUNCTION) {
        return 0;
    }
    if (Xsqlite3_stricmp(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)), __ccgo_ts + 17229) != 0) {
        return 0;
    }
    if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) != (uintptr)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc != (int32)1) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc))) != (uint32)0) {
        return 0;
    }
    pFrom = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8;
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x4) >> 2) == 0) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Correlated) != (uint32)0) {
        return 0;
    }
    pSub = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72))))).Value.FpSelect;
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior == (uintptr)0) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & (uint32)SF_CopyCte) != 0) {
        return 0;
    }
    for (var cond = true; cond; cond = pSub != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.Fop) != (int32)TK_ALL && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior != 0) {
            return 0;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpWhere != 0) {
            return 0;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit != 0) {
            return 0;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_Aggregate) | libc.Int32FromInt32(SF_Distinct))) != 0) {
            return 0;
        }
        pSub = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior;
    }
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pCount = pExpr;
    pExpr = (uintptr)0;
    pSub = _sqlite3SubqueryDetach(tls, db, pFrom);
    _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc);
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc = _sqlite3DbMallocZero(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)((uint64)(libc.UintptrFromInt32(0) + 8) + libc.Uint64FromInt64(80)));
    while (pSub != 0) {
        pPrior = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpPrior = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpNext = (uintptr)0;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSub + 4))).Value |= (uint32)SF_Aggregate;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSub + 4))).Value &= ~libc.Uint32FromInt32(SF_Compound);
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FnSelectRow = 0;
        _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3ExprListDeleteGeneric), (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpEList);
        if (pPrior != 0) {
            v1 = _sqlite3ExprDup(tls, db, pCount, 0);
        } else {
            v1 = pCount;
        }
        pTerm = v1;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpEList = _sqlite3ExprListAppend(tls, pParse, (uintptr)0, pTerm);
        pTerm = _sqlite3PExpr(tls, pParse, (int32)TK_SELECT, (uintptr)0, (uintptr)0);
        _sqlite3PExprAddSelect(tls, pParse, pTerm, pSub);
        if (pExpr == (uintptr)0) {
            pExpr = pTerm;
        } else {
            pExpr = _sqlite3PExpr(tls, pParse, (int32)TK_PLUS, pTerm, pExpr);
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pExpr);
            pExpr = (uintptr)0;
        }
        pSub = pPrior;
    }
    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList + 8))).Value.FpExpr = pExpr;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(SF_Aggregate);
    return (int32)1;
}

internal static int32 /*r*/ _createCollation(ж<libc.TLS> tls, uintptr db, uintptr zName, Tu8 enc, uintptr pCtx, uintptr __ccgo_fp_xCompare, uintptr __ccgo_fp_xDel) {
    uintptr aColl = default;
    uintptr p = default;
    uintptr pColl = default;
    int32 enc2 = default;
    int32 j = default;
    _ = aColl;
    _ = enc2;
    _ = j;
    _ = p;
    _ = pColl;
    enc2 = libc.Int32FromUint8(enc);
    if (enc2 == (int32)SQLITE_UTF16 || enc2 == (int32)SQLITE_UTF16_ALIGNED) {
        enc2 = (int32)SQLITE_UTF16LE;
    }
    if (enc2 < (int32)SQLITE_UTF8 || enc2 > (int32)SQLITE_UTF16BE) {
        return _sqlite3MisuseError(tls, (int32)191701);
    }
    pColl = _sqlite3FindCollSeq(tls, db, libc.Uint8FromInt32(enc2), zName, 0);
    if (pColl != 0 && (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxCmp != 0) {
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive != 0) {
            _sqlite3ErrorWithMsg(tls, db, (int32)SQLITE_BUSY, __ccgo_ts + 26216, 0);
            return (int32)SQLITE_BUSY;
        }
        _sqlite3ExpirePreparedStatements(ref (tls).DerefOrNull(), db, 0);
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.Fenc) & ~libc.Int32FromInt32(SQLITE_UTF16_ALIGNED)) == enc2) {
            aColl = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)(db + 656), zName);
            j = 0;
            for (;;) {
                if (!(j < (int32)3)) {
                    break;
                }
                p = aColl + (uintptr)j * 40;
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.Fenc) == libc.Int32FromUint8((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.Fenc)) {
                    if ((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.FxDel != 0) {
                        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.FxDel)(tls, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.FpUser);
                    }
                    (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.FxCmp = (uintptr)0;
                }
                goto _1;
_1:;
                j = j + 1;
            }
        }
    }
    pColl = _sqlite3FindCollSeq(tls, db, libc.Uint8FromInt32(enc2), zName, (int32)1);
    if (pColl == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxCmp = __ccgo_fp_xCompare;
    (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FpUser = pCtx;
    (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxDel = __ccgo_fp_xDel;
    (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.Fenc = libc.Uint8FromInt32(enc2 | libc.Int32FromUint8(enc) & libc.Int32FromInt32(SQLITE_UTF16_ALIGNED));
    _sqlite3Error(tls, db, SQLITE_OK);
    return SQLITE_OK;
}

internal static void _explainSimpleCount(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, uintptr pIdx) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 bCover = default;
        uintptr v1 = default;
        uintptr v2 = default;
        _ = bCover;
        _ = v1;
        _ = v2;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fexplain) == (int32)2) {
            bCover = libc.BoolInt32(pIdx != (uintptr)0 && ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 || !((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == libc.Int32FromInt32(SQLITE_IDXTYPE_PRIMARYKEY))));
            if (bCover != 0) {
                v1 = __ccgo_ts + 21662;
            } else {
                v1 = __ccgo_ts + 1732;
            }
            if (bCover != 0) {
                v2 = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzName;
            } else {
                v2 = __ccgo_ts + 1732;
            }
            _sqlite3VdbeExplain(tls, pParse, (uint8)0, __ccgo_ts + 21685, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, v1, v2));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _fixSelectCb(ж<libc.TLS> tls, uintptr p, uintptr pSelect) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr pFix = default;
        uintptr pItem = default;
        uintptr pList = default;
        int32 i = default;
        int32 iDb = default;
        _ = db;
        _ = i;
        _ = iDb;
        _ = pFix;
        _ = pItem;
        _ = pList;
        pFix = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 40));
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpParse)).Value.Fdb;
        iDb = _sqlite3FindDbName(tls, db, (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FzDb);
        pList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc;
        if (pList == (uintptr)0) {
            r = WRC_Continue; goto ᒐdone;
        }
        i = 0;
        pItem = pList + 8;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnSrc)) {
                break;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FbTemp) == 0 && (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) == 0) {
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x10000) >> 16) == 0 && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)) != (uintptr)0) {
                    if (iDb != _sqlite3FindDbName(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)))) {
                        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpParse, __ccgo_ts + 13678, libc.VaList(bp + 8, (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FzType, (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpName, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))));
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)));
                    libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 10, 0x400);
                    libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 17, 0x20000);
                }
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = (@unsafe.Pointer.To<TDbFixer>((@unsafe.Pointer)pFix)).Value.FpSchema;
                libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 8, 0x100);
                libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 16, 0x10000);
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 80 + 24 + 4)) & 0x800) >> 11) == 0 && _sqlite3WalkExpr(tls, pFix + 8, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 80 + 64))) != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            goto _1;
_1:;
            i = i + 1;
            pItem += 80;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWith != 0) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWith)).Value.FnCte)) {
                    break;
                }
                if (_sqlite3WalkSelect(tls, p, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWith + 16 + (uintptr)i * 48))).Value.FpSelect) != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
                goto _2;
_2:;
                i = i + 1;
            }
        }
        r = WRC_Continue;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _generateWithRecursiveQuery(ж<libc.TLS> tls, uintptr pParse, uintptr p, @unsafe.Pointer pDest) {
    int32 addrBreak = default;
    int32 addrCont = default;
    int32 addrTop = default;
    int32 eDest = default;
    int32 i = default;
    int32 iCurrent = default;
    int32 iDistinct = default;
    int32 iQueue = default;
    int32 nCol = default;
    int32 rc = default;
    int32 regCurrent = default;
    int32 regLimit = default;
    int32 regOffset = default;
    int32 v1 = default;
    uintptr apColl = default;
    uintptr pFirstRec = default;
    uintptr pKeyInfo = default;
    uintptr pKeyInfo1 = default;
    uintptr pLimit = default;
    uintptr pOrderBy = default;
    uintptr pSetup = default;
    uintptr pSrc = default;
    uintptr v = default;
    uintptr v4 = default;
    ref var destQueue = ref heap(new TSelectDest(), out var ᏑdestQueue);
    _ = addrBreak;
    _ = addrCont;
    _ = addrTop;
    _ = apColl;
    _ = destQueue;
    _ = eDest;
    _ = i;
    _ = iCurrent;
    _ = iDistinct;
    _ = iQueue;
    _ = nCol;
    _ = pFirstRec;
    _ = pKeyInfo;
    _ = pKeyInfo1;
    _ = pLimit;
    _ = pOrderBy;
    _ = pSetup;
    _ = pSrc;
    _ = rc;
    _ = regCurrent;
    _ = regLimit;
    _ = regOffset;
    _ = v;
    _ = v1;
    _ = v4;
    pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
    nCol = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    iCurrent = 0;
    iDistinct = 0;
    eDest = (int32)SRT_Fifo;
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin != 0) {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 20830, 0);
        return;
    }
    if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_RECURSIVE, (uintptr)0, (uintptr)0, (uintptr)0) != 0) {
        return;
    }
    addrBreak = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow = (int16)320;
    _computeLimitRegisters(tls, pParse, p, addrBreak);
    pLimit = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit;
    regLimit = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FiLimit;
    regOffset = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FiOffset;
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit = (uintptr)0;
    v1 = libc.Int32FromInt32(0);
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FiOffset = v1;
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FiLimit = v1;
    pOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
            break;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 24 + 4)) & 0x80) >> 7) != 0) {
            iCurrent = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80))).Value.FiCursor;
            break;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    v4 = pParse + 56;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
    iQueue = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UNION) {
        if (pOrderBy != 0) {
            v1 = (int32)SRT_DistQueue;
        } else {
            v1 = (int32)SRT_DistFifo;
        }
        eDest = v1;
        v4 = pParse + 56;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
        iDistinct = v1;
    } else {
        if (pOrderBy != 0) {
            v1 = (int32)SRT_Queue;
        } else {
            v1 = (int32)SRT_Fifo;
        }
        eDest = v1;
    }
    _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(ᏑdestQueue), eDest, iQueue);
    v4 = pParse + 60;
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
    regCurrent = v1;
    _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenPseudo, iCurrent, regCurrent, nCol);
    if (pOrderBy != 0) {
        pKeyInfo = _multiSelectByMergeKeyInfo(tls, pParse, p, (int32)1);
        _sqlite3VdbeAddOp4(tls, v, (int32)OP_OpenEphemeral, iQueue, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr + (int32)2, 0, pKeyInfo, -(int32)9);
        destQueue.FpOrderBy = pOrderBy;
    } else {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, iQueue, nCol);
    }
    if (iDistinct != 0) {
        nCol = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr;
        pKeyInfo1 = _sqlite3KeyInfoAlloc(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, nCol, (int32)1);
        if (pKeyInfo1 != 0) {
            i = 0;
            apColl = pKeyInfo1 + 32;
            for (;;) {
                if (!(i < nCol)) {
                    break;
                }
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apColl))).Value = _multiSelectCollSeq(tls, pParse, p, i);
                if ((uintptr)0 == ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apColl))) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apColl))).Value = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FpDfltColl;
                }
                goto _11;
_11:;
                i = i + 1;
                apColl += 8;
            }
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_OpenEphemeral, iDistinct, nCol, 0, pKeyInfo1, -(int32)9);
        } else {
        }
    }
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = (uintptr)0;
    pFirstRec = p;
    for (;;) {
        if (!(pFirstRec != (uintptr)0)) {
            break;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.FselFlags & (uint32)SF_Aggregate) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 20879, 0);
            goto end_of_recursive_query;
        }
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.Fop = (uint8)TK_ALL;
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.FpPrior)).Value.FselFlags & (uint32)SF_Recursive) == (uint32)0) {
            break;
        }
        goto _12;
_12:;
        pFirstRec = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.FpPrior;
    }
    pSetup = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.FpPrior;
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSetup)).Value.FpNext = (uintptr)0;
    _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 20921, 0);
    rc = _sqlite3Select(tls, pParse, pSetup, @unsafe.Pointer.FromPinnedBox(ᏑdestQueue));
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSetup)).Value.FpNext = p;
    if (rc != 0) {
        goto end_of_recursive_query;
    }
    addrTop = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iQueue, addrBreak);
    _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, iCurrent);
    if (pOrderBy != 0) {
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iQueue, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr + (int32)1, regCurrent);
    } else {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_RowData, iQueue, regCurrent);
    }
    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Delete, iQueue);
    addrCont = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
    _codeOffset(tls, v, regOffset, addrCont);
    _selectInnerLoop(tls, pParse, p, iCurrent, nil, nil, pDest, addrCont, addrBreak);
    if (regLimit != 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_DecrJumpZero, regLimit, addrBreak);
    }
    _sqlite3VdbeResolveLabel(tls, v, addrCont);
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.FpPrior = (uintptr)0;
    _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 20927, 0);
    _sqlite3Select(tls, pParse, p, @unsafe.Pointer.FromPinnedBox(ᏑdestQueue));
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pFirstRec)).Value.FpPrior = pSetup;
    _sqlite3VdbeGoto(tls, v, addrTop);
    _sqlite3VdbeResolveLabel(tls, v, addrBreak);
    goto end_of_recursive_query;
end_of_recursive_query:;
    _sqlite3ExprListDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy);
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = pOrderBy;
    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit = pLimit;
    return;
}

internal static int32 /*r*/ _lockBtree(ж<libc.TLS> tls, uintptr pBt) {
    ref var isOpen = ref heap(new int32(), out var ᏑisOpen);
    int32 rc = default;
    int32 v4 = default;
    Tu32 nPage = default;
    ref var nPageFile = ref heap(new Tu32(), out var ᏑnPageFile);
    Tu32 pageSize = default;
    Tu32 usableSize = default;
    ref var pPage1 = ref heap(new uintptr(), out var ᏑpPage1);
    uintptr page1 = default;
    uintptr v1 = default;
    _ = isOpen;
    _ = nPage;
    _ = nPageFile;
    _ = pPage1;
    _ = page1;
    _ = pageSize;
    _ = rc;
    _ = usableSize;
    _ = v1;
    _ = v4;
    nPageFile = (uint32)0;
    rc = _sqlite3PagerSharedLock(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager);
    if (rc != SQLITE_OK) {
        return rc;
    }
    rc = _btreeGetPage(tls, pBt, (uint32)1, @unsafe.Pointer.FromBox(ᏑpPage1), 0);
    if (rc != SQLITE_OK) {
        return rc;
    }
    nPage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(libc.UintptrFromInt32(28) + (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData));
    _sqlite3PagerPagecount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, @unsafe.Pointer.FromPinnedBox(ᏑnPageFile));
    if (nPage == (uint32)0 || libc.Xmemcmp(tls, (uintptr)24 + (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData, (uintptr)92 + (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData, (uint64)4) != 0) {
        nPage = nPageFile;
    }
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_ResetDatabase) != (uint64)0) {
        nPage = (uint32)0;
    }
    if (nPage > (uint32)0) {
        page1 = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData;
        rc = (int32)SQLITE_NOTADB;
        if (libc.Xmemcmp(tls, page1, (uintptr)Ꮡ_zMagicHeader, (uint64)16) != 0) {
            goto page1_init_failed;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(page1 + 18))) > (int32)2) {
            v1 = pBt + 40;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTS_READ_ONLY));
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(page1 + 19))) > (int32)2) {
            goto page1_init_failed;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(page1 + 19))) == (int32)2 && (libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_NO_WAL) == 0) {
            isOpen = 0;
            rc = _sqlite3PagerOpenWal(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, @unsafe.Pointer.FromPinnedBox(ᏑisOpen));
            if (rc != SQLITE_OK) {
                goto page1_init_failed;
            } else {
                if (isOpen == 0) {
                    _releasePageOne(tls, pPage1);
                    return SQLITE_OK;
                }
            }
            rc = (int32)SQLITE_NOTADB;
        } else {
        }
        if (libc.Xmemcmp(tls, page1 + 21, __ccgo_ts + 4390, (uint64)3) != 0) {
            goto page1_init_failed;
        }
        pageSize = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(page1 + 16))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(page1 + 17))) << (int)((int32)16));
        if ((Tu32)((pageSize - (uint32)1) & pageSize) != (uint32)0 || pageSize > (uint32)SQLITE_MAX_PAGE_SIZE || pageSize <= (uint32)256) {
            goto page1_init_failed;
        }
        usableSize = pageSize - (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(page1 + 20)));
        if (pageSize != (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize) {
            _releasePageOne(tls, pPage1);
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize = usableSize;
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize = pageSize;
            v1 = pBt + 40;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTS_PAGESIZE_FIXED));
            _freeTempSpace(tls, pBt);
            rc = _sqlite3PagerSetPagesize(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, (@unsafe.Pointer)(pBt + 52), libc.Int32FromUint32(pageSize - usableSize));
            return rc;
        }
        if (nPage > nPageFile) {
            if (_sqlite3WritableSchema(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.Fdb) == 0) {
                rc = _sqlite3CorruptError(tls, (int32)76583);
                goto page1_init_failed;
            } else {
                nPage = nPageFile;
            }
        }
        if (usableSize < (uint32)480) {
            goto page1_init_failed;
        }
        v1 = pBt + 40;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTS_PAGESIZE_FIXED));
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize = pageSize;
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize = usableSize;
        if (_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(page1 + (uintptr)(libc.Int32FromInt32(36) + libc.Int32FromInt32(4) * libc.Int32FromInt32(4)))) != 0) {
            v4 = (int32)1;
        } else {
            v4 = 0;
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum = libc.Uint8FromInt32(v4);
        if (_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(page1 + (uintptr)(libc.Int32FromInt32(36) + libc.Int32FromInt32(7) * libc.Int32FromInt32(4)))) != 0) {
            v4 = (int32)1;
        } else {
            v4 = 0;
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FincrVacuum = libc.Uint8FromInt32(v4);
    }
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FmaxLocal = (uint16)(((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(12)) * libc.Uint32FromInt32(64) / libc.Uint32FromInt32(255) - libc.Uint32FromInt32(23));
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FminLocal = (uint16)(((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(12)) * libc.Uint32FromInt32(32) / libc.Uint32FromInt32(255) - libc.Uint32FromInt32(23));
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FmaxLeaf = (uint16)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(35));
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FminLeaf = (uint16)(((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(12)) * libc.Uint32FromInt32(32) / libc.Uint32FromInt32(255) - libc.Uint32FromInt32(23));
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FmaxLocal) > (int32)127) {
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.Fmax1bytePayload = (uint8)127;
    } else {
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.Fmax1bytePayload = (uint8)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FmaxLocal;
    }
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1 = pPage1;
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = nPage;
    return SQLITE_OK;
    goto page1_init_failed;
page1_init_failed:;
    _releasePageOne(tls, pPage1);
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1 = (uintptr)0;
    return rc;
}

internal static void _renameTestSchema(ж<libc.TLS> tls, uintptr pParse, uintptr zDb, int32 bTemp, uintptr zWhen, int32 bNoDQS) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 5, 0x20);
        _sqlite3NestedParse(tls, pParse, __ccgo_ts + 8745, libc.VaList(bp + 8, zDb, zDb, bTemp, zWhen, bNoDQS));
        if (bTemp == 0) {
            _sqlite3NestedParse(tls, pParse, __ccgo_ts + 8920, libc.VaList(bp + 8, zDb, zWhen, bNoDQS));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _resolveCompoundOrderBy(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr pDup = default;
        uintptr pE = default;
        uintptr pEList = default;
        uintptr pItem = default;
        uintptr pNew = default;
        uintptr pOrderBy = default;
        uintptr pParent = default;
        int32 i = default;
        ref var iCol = ref heap(new int32(), out var ᏑiCol);
        int32 moreToDo = default;
        _ = db;
        _ = i;
        _ = iCol;
        _ = moreToDo;
        _ = pDup;
        _ = pE;
        _ = pEList;
        _ = pItem;
        _ = pNew;
        _ = pOrderBy;
        _ = pParent;
        moreToDo = (int32)1;
        pOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpOrderBy;
        if (pOrderBy == (uintptr)0) {
            r = 0; goto ᒐdone;
        }
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr > ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + 2 * 4))) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7699, 0);
            r = (int32)1; goto ᒐdone;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr)) {
                break;
            }
            libc.SetBitFieldPtr16Uint32(pOrderBy + 8 + (uintptr)i * 32 + 16 + 4, libc.Uint32FromInt32(0), 2, 0x4);
            goto _1;
_1:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpNext = (uintptr)0;
        while ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior != 0) {
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior)).Value.FpNext = pSelect;
            pSelect = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior;
        }
        while (pSelect != 0 && moreToDo != 0) {
            moreToDo = 0;
            pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList;
            i = 0;
            pItem = pOrderBy + 8;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr)) {
                    break;
                }
                iCol = -(int32)1;
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pItem + 16 + 4)) & 0x4) >> 2) != 0) {
                    goto _2;
                }
                pE = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr);
                if (pE == (uintptr)0) {
                    goto _2;
                }
                if (_sqlite3ExprIsInteger(tls, pE, @unsafe.Pointer.FromPinnedBox(ᏑiCol), (uintptr)0, 0) != 0) {
                    if (iCol <= 0 || iCol > (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr) {
                        _resolveOutOfRangeError(tls, pParse, __ccgo_ts + 7733, i + (int32)1, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr, pE);
                        r = (int32)1; goto ᒐdone;
                    }
                } else {
                    iCol = _resolveAsName(tls, pParse, pEList, pE);
                    if (iCol == 0) {
                        pDup = _sqlite3ExprDup(tls, db, pE, 0);
                        if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                            iCol = _resolveOrderByTermToExprList(tls, pParse, pSelect, pDup);
                            if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME && iCol > 0) {
                                _resolveOrderByTermToExprList(tls, pParse, pSelect, pE);
                            }
                        }
                        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pDup);
                    }
                }
                if (iCol > 0) {
                    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
                        pNew = _sqlite3ExprInt32(tls, db, iCol);
                        if (pNew == (uintptr)0) {
                            r = (int32)1; goto ᒐdone;
                        }
                        if ((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr == pE) {
                            (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr = pNew;
                        } else {
                            pParent = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr;
                            while (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pParent)).Value.FpLeft)).Value.Fop) == (int32)TK_COLLATE) {
                                pParent = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pParent)).Value.FpLeft;
                            }
                            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pParent)).Value.FpLeft = pNew;
                        }
                        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pE);
                        (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem + 24))).Value.FiOrderByCol = libc.Uint16FromInt32(iCol);
                    }
                    libc.SetBitFieldPtr16Uint32(pItem + 16 + 4, libc.Uint32FromInt32(1), 2, 0x4);
                } else {
                    moreToDo = (int32)1;
                }
                goto _2;
_2:;
                i = i + 1;
                pItem += 32;
            }
            pSelect = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpNext;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr)) {
                break;
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32 + 16 + 4)) & 0x4) >> 2) == 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7739, libc.VaList(bp + 8, i + (int32)1));
                r = (int32)1; goto ᒐdone;
            }
            goto _3;
_3:;
            i = i + 1;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _resolveFromTermToCte(ж<libc.TLS> tls, uintptr pParse, uintptr pWalker, uintptr pFrom) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 bMayRecursive = default;
        int32 i = default;
        int32 iRecTab = default;
        int32 rc = default;
        int32 v3 = default;
        uintptr db = default;
        uintptr pCte = default;
        uintptr pCteUse = default;
        uintptr pEList = default;
        uintptr pItem = default;
        uintptr pLeft = default;
        uintptr pRecTerm = default;
        uintptr pSavedWith = default;
        uintptr pSel = default;
        uintptr pSrc = default;
        uintptr pTab = default;
        ref var pWith = ref heap(new uintptr(), out var ᏑpWith);
        uintptr v1 = default;
        _ = bMayRecursive;
        _ = db;
        _ = i;
        _ = iRecTab;
        _ = pCte;
        _ = pCteUse;
        _ = pEList;
        _ = pItem;
        _ = pLeft;
        _ = pRecTerm;
        _ = pSavedWith;
        _ = pSel;
        _ = pSrc;
        _ = pTab;
        _ = pWith;
        _ = rc;
        _ = v1;
        _ = v3;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith == (uintptr)0) {
            r = 0; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            r = 0; goto ᒐdone;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x10000) >> 16) == 0 && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72)) != (uintptr)0) {
            r = 0; goto ᒐdone;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x400) >> 10) != 0) {
            r = 0; goto ᒐdone;
        }
        pCte = _searchWith(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith, pFrom, @unsafe.Pointer.FromBox(ᏑpWith));
        if (pCte != 0) {
            db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
            iRecTab = -(int32)1;
            if ((@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzCteErr != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzCteErr, libc.VaList(bp + 8, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzName));
                r = (int32)2; goto ᒐdone;
            }
            if (_cannotBeFunction(tls, pParse, pFrom) != 0) {
                r = (int32)2; goto ᒐdone;
            }
            pTab = _sqlite3DbMallocZero(tls, db, (uint64)120);
            if (pTab == (uintptr)0) {
                r = (int32)2; goto ᒐdone;
            }
            pCteUse = (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpUse;
            if (pCteUse == (uintptr)0) {
                v1 = _sqlite3DbMallocZero(tls, db, (uint64)20);
                pCteUse = v1;
                (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpUse = v1;
                if (pCteUse == (uintptr)0 || _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3DbFree), pCteUse) == (uintptr)0) {
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pTab);
                    r = (int32)2; goto ᒐdone;
                }
                (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FeM10d = (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FeM10d;
            }
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab = pTab;
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef = (uint32)1;
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzName);
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey = (int16)(-(int32)1);
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnRowLogEst = (int16)200;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(TF_Ephemeral) | libc.Int32FromInt32(TF_NoVisibleRowid));
            _sqlite3SrcItemAttachSubquery(tls, pParse, pFrom, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpSelect, (int32)1);
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                r = (int32)2; goto ᒐdone;
            }
            pSel = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72))))).Value.FpSelect;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSel + 4))).Value |= (uint32)SF_CopyCte;
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x2) >> 1) != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21192, libc.VaList(bp + 8, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 48))));
                r = (int32)2; goto ᒐdone;
            }
            libc.SetBitFieldPtr32Uint32(pFrom + 24 + 4, libc.Uint32FromInt32(1), 9, 0x200);
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 56))).Value = pCteUse;
            (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FnUse = (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FnUse + 1;
            pRecTerm = pSel;
            bMayRecursive = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.Fop) == (int32)TK_ALL || libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.Fop) == (int32)TK_UNION);
            while (bMayRecursive != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.Fop) == libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.Fop)) {
                pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.FpSrc;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
                        break;
                    }
                    pItem = pSrc + 8 + (uintptr)i * 80;
                    if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName != (uintptr)0 && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x20000) >> 17) != 0) && !((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) && ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x10000) >> 16) != 0 || ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72)) == (uintptr)0) && 0 == _sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzName)) {
                        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab = pTab;
                        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef + 1;
                        libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 7, 0x80);
                        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.FselFlags & (uint32)SF_Recursive) != 0) {
                            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21212, libc.VaList(bp + 8, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzName));
                            r = (int32)2; goto ᒐdone;
                        }
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pRecTerm + 4))).Value |= (uint32)SF_Recursive;
                        if (iRecTab < 0) {
                            v1 = pParse + 56;
                            v3 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                            iRecTab = v3;
                        }
                        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor = iRecTab;
                    }
                    goto _2;
_2:;
                    i = i + 1;
                }
                if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.FselFlags & (uint32)SF_Recursive) == (uint32)0) {
                    break;
                }
                pRecTerm = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.FpPrior;
            }
            if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab >= ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + 3 * 4))) {
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel + 1;
            if (_checkSubqueryNestingDepth(tls, pParse) != 0) {
                r = (int32)2; goto ᒐdone;
            }
            (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzCteErr = __ccgo_ts + 21255;
            pSavedWith = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = pWith;
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FselFlags & (uint32)SF_Recursive) != 0) {
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.FpWith = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FpWith;
                rc = _sqlite3WalkSelect(tls, pWalker, pRecTerm);
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRecTerm)).Value.FpWith = (uintptr)0;
                if (rc != 0) {
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = pSavedWith;
                    r = (int32)2; goto ᒐdone;
                }
            } else {
                if (_sqlite3WalkSelect(tls, pWalker, pSel) != 0) {
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = pSavedWith;
                    r = (int32)2; goto ᒐdone;
                }
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = pWith;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel - 1;
            pLeft = pSel;
            for (;;) {
                if (!((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpPrior != 0)) {
                    break;
                }
                goto _5;
_5:;
                pLeft = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpPrior;
            }
            pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpEList;
            if ((@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpCols != 0) {
                if (pEList != 0 && (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr != (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpCols)).Value.FnExpr) {
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21278, libc.VaList(bp + 8, (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzName, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpCols)).Value.FnExpr));
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = pSavedWith;
                    r = (int32)2; goto ᒐdone;
                }
                pEList = (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FpCols;
            }
            _sqlite3ColumnsFromExprList(tls, pParse, pEList, pTab + 54, pTab + 8);
            if (bMayRecursive != 0) {
                if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSel)).Value.FselFlags & (uint32)SF_Recursive) != 0) {
                    (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzCteErr = __ccgo_ts + 21316;
                } else {
                    (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzCteErr = __ccgo_ts + 21350;
                }
                _sqlite3WalkSelect(tls, pWalker, pSel);
            }
            (@unsafe.Pointer.To<TCte>((@unsafe.Pointer)pCte)).Value.FzCteErr = (uintptr)0;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith = pSavedWith;
            r = (int32)1; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _resolveSelectStep(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr pGroupBy = default;
        uintptr pItem = default;
        uintptr pItem1 = default;
        uintptr pItem2 = default;
        uintptr pLeftmost = default;
        uintptr pOuterNC = default;
        uintptr pParse = default;
        uintptr pSub = default;
        uintptr pSub1 = default;
        uintptr pWin = default;
        uintptr zSavedContext = default;
        int32 i = default;
        int32 isCompound = default;
        int32 nCompound = default;
        int32 nRef = default;
        int32 v1 = default;
        TNameContext _ᴛ1 = new();
        _ = db;
        _ = i;
        _ = isCompound;
        _ = nCompound;
        _ = nRef;
        _ = pGroupBy;
        _ = pItem;
        _ = pItem1;
        _ = pItem2;
        _ = pLeftmost;
        _ = pOuterNC;
        _ = pParse;
        _ = pSub;
        _ = pSub1;
        _ = pWin;
        _ = zSavedContext;
        _ = v1;
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Resolved) != 0) {
            r = (int32)WRC_Prune; goto ᒐdone;
        }
        pOuterNC = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
        pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Expanded) == (uint32)0) {
            _sqlite3SelectPrep(tls, pParse, p, pOuterNC);
            if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                v1 = (int32)WRC_Abort;
            } else {
                v1 = (int32)WRC_Prune;
            }
            r = v1; goto ᒐdone;
        }
        isCompound = libc.BoolInt32((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior != (uintptr)0);
        nCompound = 0;
        pLeftmost = p;
        while (p != 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_Resolved;
            libc.Xmemset(tls, bp, 0, (uint64)56);
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpWinSelect = p;
            if (_sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit) != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Converted) != 0) {
                pSub = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + 72))))).Value.FpSelect;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = (uintptr)0;
            }
            if (pOuterNC != 0) {
                (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnNestedSelect = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnNestedSelect + 1;
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc)) {
                    break;
                }
                pItem = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + (uintptr)i * 80;
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect)).Value.FselFlags & (uint32)SF_Resolved) == (uint32)0) {
                    if (pOuterNC != 0) {
                        v1 = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnRef;
                    } else {
                        v1 = 0;
                    }
                    nRef = v1;
                    zSavedContext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext;
                    if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName != 0) {
                        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName;
                    }
                    _sqlite3ResolveSelectNames(tls, pParse, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect, pOuterNC);
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = zSavedContext;
                    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    if (pOuterNC != 0) {
                        libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.BoolUint32((@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnRef > nRef), 4, 0x10);
                    }
                }
                goto _2;
_2:;
                i = i + 1;
            }
            if (pOuterNC != 0 && (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnNestedSelect > (uint32)0) {
                (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnNestedSelect = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pOuterNC)).Value.FnNestedSelect - 1;
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags = libc.Int32FromInt32(NC_AllowAgg) | libc.Int32FromInt32(NC_AllowWin);
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpNext = pOuterNC;
            if (_sqlite3ResolveExprListNames(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList) != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags &= (int32)(~libc.Int32FromInt32(NC_AllowWin));
            pGroupBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy;
            if (pGroupBy != 0 || ((@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags & (int32)NC_HasAgg) != 0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= libc.Uint32FromInt32((int32)SF_Aggregate | (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags & (libc.Int32FromInt32(NC_MinMaxAgg) | libc.Int32FromInt32(NC_OrderAgg)));
            } else {
                (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags &= (int32)(~libc.Int32FromInt32(NC_AllowAgg));
            }
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp)).of(TNameContext.ᏑFuNC)), 0))).Value = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags |= (int32)((int32)NC_UEList);
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving != 0) {
                if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Aggregate) == (uint32)0) {
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7831, 0);
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
                if (_sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving) != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags |= (int32)((int32)NC_Where);
            if (_sqlite3ResolveExprNames(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere) != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags &= (int32)(~libc.Int32FromInt32(NC_Where));
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc)) {
                    break;
                }
                pItem1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + (uintptr)i * 80;
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x8) >> 3) != 0 && _sqlite3ResolveExprListNames(tls, bp, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 48))) != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
                goto _4;
_4:;
                i = i + 1;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
                pWin = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWinDefn;
                for (;;) {
                    if (!(pWin != 0)) {
                        break;
                    }
                    if (_sqlite3ResolveExprListNames(tls, bp, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOrderBy) != 0 || _sqlite3ResolveExprListNames(tls, bp, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpPartition) != 0) {
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    goto _5;
_5:;
                    pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
                }
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags |= (int32)((libc.Int32FromInt32(NC_AllowAgg) | libc.Int32FromInt32(NC_AllowWin)));
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Converted) != 0) {
                pSub1 = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + 72))))).Value.FpSelect;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpOrderBy;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpOrderBy = (uintptr)0;
            }
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != (uintptr)0 && isCompound <= nCompound && _resolveOrderGroupBy(tls, bp, p, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy, __ccgo_ts + 7733) != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FncFlags &= (int32)(~libc.Int32FromInt32(NC_AllowWin));
            if (pGroupBy != 0) {
                if (_resolveOrderGroupBy(tls, bp, p, pGroupBy, __ccgo_ts + 7870) != 0 || (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
                i = 0;
                pItem2 = pGroupBy + 8;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr)) {
                        break;
                    }
                    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem2)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Agg))) != (uint32)0) {
                        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7876, 0);
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    goto _6;
_6:;
                    i = i + 1;
                    pItem2 += 32;
                }
            }
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpNext != 0 && (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr != (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpNext)).Value.FpEList)).Value.FnExpr) {
                _sqlite3SelectWrongNumTermsError(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpNext);
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_OnToWhere) != 0) {
                _sqlite3SelectCheckOnClauses(tls, pParse, p);
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
            }
            p = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior;
            nCompound = nCompound + 1;
        }
        if (isCompound != 0 && _resolveCompoundOrderBy(tls, pParse, pLeftmost) != 0) {
            r = (int32)WRC_Abort; goto ᒐdone;
        }
        r = (int32)WRC_Prune;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _rtreeCheckNode(ж<libc.TLS> tls, @unsafe.Pointer pCheck, int32 iDepth, uintptr aParent, Ti64 iNode) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr aNode = default;
        uintptr pCell = default;
        int32 i = default;
        int32 nCell = default;
        ref var nNode = ref heap(new int32(), out var ᏑnNode);
        Ti64 iVal = default;
        _ = aNode;
        _ = i;
        _ = iVal;
        _ = nCell;
        _ = nNode;
        _ = pCell;
        aNode = (uintptr)0;
        nNode = 0;
        aNode = _rtreeCheckGetNode(tls, pCheck, iNode, @unsafe.Pointer.FromPinnedBox(ᏑnNode));
        if (aNode != 0) {
            if (nNode < (int32)4) {
                _rtreeCheckAppendMsg(tls, pCheck, __ccgo_ts + 29657, libc.VaList(bp + 8, iNode, nNode));
            } else {
                if (aParent == (uintptr)0) {
                    iDepth = _readInt16(ref (tls).DerefOrNull(), aNode);
                    if (iDepth > (int32)RTREE_MAX_DEPTH) {
                        _rtreeCheckAppendMsg(tls, pCheck, __ccgo_ts + 29691, libc.VaList(bp + 8, iDepth));
                        Xsqlite3_free(tls, aNode);
                        return;
                    }
                }
                nCell = _readInt16(ref (tls).DerefOrNull(), aNode + 2);
                if ((int32)4 + nCell * ((int32)8 + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 28)) * (int32)2 * (int32)4) > nNode) {
                    _rtreeCheckAppendMsg(tls, pCheck, __ccgo_ts + 29721, libc.VaList(bp + 8, iNode, nCell, nNode));
                } else {
                    i = 0;
                    for (;;) {
                        if (!(i < nCell)) {
                            break;
                        }
                        pCell = aNode + (uintptr)((int32)4 + i * ((int32)8 + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 28)) * (int32)2 * (int32)4));
                        iVal = _readInt64(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCell);
                        _rtreeCheckCellCoord(tls, pCheck, iNode, i, pCell + 8, aParent);
                        if (iDepth > 0) {
                            _rtreeCheckMapping(tls, pCheck, 0, iVal, iNode);
                            _rtreeCheckNode(tls, pCheck, iDepth - (int32)1, pCell + 8, iVal);
                            (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 60))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 60)) + 1;
                        } else {
                            _rtreeCheckMapping(tls, pCheck, (int32)1, iVal, iNode);
                            (@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 56))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCheck, 56)) + 1;
                        }
                        goto _1;
_1:;
                        i = i + 1;
                    }
                }
            }
            Xsqlite3_free(tls, aNode);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _rtreenode(ж<libc.TLS> tls, uintptr ctx, int32 nArg, uintptr apArg) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(1088);
        defer(tls.Free, (nint)1088, ref ᒐ);
        int32 errCode = default;
        int32 ii = default;
        int32 jj = default;
        int32 nData = default;
        Ti64 nDim64 = default;
        uintptr pOut = default;
        TRtreeCell _ᴛ1 = new();
        TRtreeNode _ᴛ2 = default;
        TRtree _ᴛ3 = new();
        _ = errCode;
        _ = ii;
        _ = jj;
        _ = nData;
        _ = nDim64;
        _ = pOut;
        _ = nArg;
        libc.Xmemset(tls, bp, 0, (uint64)40);
        libc.Xmemset(tls, bp + 40, 0, (uint64)976);
        nDim64 = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
        if (nDim64 < (int64)1 || nDim64 > (int64)5) {
            return;
        }
        (@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnDim = libc.Uint8FromInt64(nDim64);
        (@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnDim2 = libc.Uint8FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnDim) * (int32)2);
        (@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnBytesPerCell = libc.Uint8FromInt32((int32)8 + (int32)8 * libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnDim));
        (@unsafe.Pointer.To<TRtreeNode>(__ccgo_ptr(bp))).Value.FzData = Xsqlite3_value_blob(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg + 1 * 8)));
        if ((@unsafe.Pointer.To<TRtreeNode>(__ccgo_ptr(bp))).Value.FzData == (uintptr)0) {
            return;
        }
        nData = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg + 1 * 8)));
        if (nData < (int32)4) {
            return;
        }
        if (nData < (int32)4 + _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)bp)).Value.FzData + 2) * libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnBytesPerCell)) {
            return;
        }
        pOut = Xsqlite3_str_new(tls, (uintptr)0);
        ii = 0;
        for (;;) {
            if (!(ii < _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)bp)).Value.FzData + 2))) {
                break;
            }
            _nodeGetCell(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 40), __ccgo_ptr(bp), ii, __ccgo_ptr(bp + 1016));
            if (ii > 0) {
                Xsqlite3_str_append(tls, pOut, __ccgo_ts + 11667, (int32)1);
            }
            Xsqlite3_str_appendf(tls, pOut, __ccgo_ts + 29193, libc.VaList(bp + 1072, (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp + 1016))).Value.FiRowid));
            jj = 0;
            for (;;) {
                if (!(jj < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>(__ccgo_ptr(bp + 40))).Value.FnDim2))) {
                    break;
                }
                Xsqlite3_str_appendf(tls, pOut, __ccgo_ts + 29199, libc.VaList(bp + 1072, (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 1016 + 8 + (uintptr)jj * 4)))));
                goto _2;
_2:;
                jj = jj + 1;
            }
            Xsqlite3_str_append(tls, pOut, __ccgo_ts + 27038, (int32)1);
            goto _1;
_1:;
            ii = ii + 1;
        }
        errCode = Xsqlite3_str_errcode(tls, pOut);
        Xsqlite3_result_error_code(tls, ctx, errCode);
        Xsqlite3_result_text(tls, ctx, Xsqlite3_str_finish(tls, pOut), -(int32)1, __ccgo_fp(Xsqlite3_free));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _selectExpander(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr a = default;
        uintptr db = default;
        uintptr pE = default;
        uintptr pEList = default;
        uintptr pExpr = default;
        uintptr pFrom = default;
        uintptr pLeft = default;
        uintptr pNestedFrom = default;
        uintptr pNew = default;
        uintptr pParse = default;
        uintptr pRight = default;
        uintptr pSel = default;
        uintptr pTab = default;
        uintptr pTab1 = default;
        uintptr pTabList = default;
        uintptr pUsing = default;
        uintptr pX = default;
        uintptr pX1 = default;
        uintptr zName = default;
        uintptr zSchemaName = default;
        uintptr zTName = default;
        uintptr zTabName = default;
        uintptr zUName = default;
        uintptr v2 = default;
        Tu8 eCodeOrig = default;
        Tu32 elistFlags = default;
        int32 flags = default;
        int32 i = default;
        int32 iDb = default;
        int32 iErrOfst = default;
        int32 ii = default;
        int32 j = default;
        int32 k = default;
        int32 longNames = default;
        int32 nAdd = default;
        int32 rc = default;
        int32 tableSeen = default;
        int32 v1 = default;
        Ti16 nCol = default;
        Tu16 selFlags = default;
        _ = a;
        _ = db;
        _ = eCodeOrig;
        _ = elistFlags;
        _ = flags;
        _ = i;
        _ = iDb;
        _ = iErrOfst;
        _ = ii;
        _ = j;
        _ = k;
        _ = longNames;
        _ = nAdd;
        _ = nCol;
        _ = pE;
        _ = pEList;
        _ = pExpr;
        _ = pFrom;
        _ = pLeft;
        _ = pNestedFrom;
        _ = pNew;
        _ = pParse;
        _ = pRight;
        _ = pSel;
        _ = pTab;
        _ = pTab1;
        _ = pTabList;
        _ = pUsing;
        _ = pX;
        _ = pX1;
        _ = rc;
        _ = selFlags;
        _ = tableSeen;
        _ = zName;
        _ = zSchemaName;
        _ = zTName;
        _ = zTabName;
        _ = zUName;
        _ = v1;
        _ = v2;
        pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        selFlags = (uint16)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags;
        elistFlags = (uint32)0;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_Expanded;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            r = (int32)WRC_Abort; goto ᒐdone;
        }
        if ((libc.Int32FromUint16(selFlags) & (int32)SF_Expanded) != 0) {
            r = (int32)WRC_Prune; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode != 0) {
            v2 = pParse + 132;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselId = libc.Uint32FromInt32(v1);
        }
        pTabList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_View) != 0) {
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWith == (uintptr)0) {
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWith = _sqlite3DbMallocZero(tls, db, (uint64)((uint64)(libc.UintptrFromInt32(0) + 16) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(48)));
                if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWith == (uintptr)0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
            }
            (@unsafe.Pointer.To<TWith>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWith)).Value.FbView = (int32)1;
        }
        _sqlite3WithPush(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWith, (uint8)0);
        _sqlite3SrcListAssignCursors(ref (tls).DerefOrNull(), pParse, pTabList);
        i = 0;
        pFrom = pTabList + 8;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
                break;
            }
            if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab != 0) {
                goto _3;
            }
            if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FzName == (uintptr)0) {
                pSel = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72))))).Value.FpSelect;
                if (_sqlite3WalkSelect(tls, pWalker, pSel) != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
                if (_sqlite3ExpandSubquery(tls, pParse, pFrom) != 0) {
                    r = (int32)WRC_Abort; goto ᒐdone;
                }
            } else {
                v1 = _resolveFromTermToCte(tls, pParse, pWalker, pFrom);
                rc = v1;
                if (v1 != 0) {
                    if (rc > (int32)1) {
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab;
                } else {
                    v2 = _sqlite3LocateTableItem(tls, pParse, (uint32)0, pFrom);
                    pTab = v2;
                    (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab = v2;
                    if (pTab == (uintptr)0) {
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef >= (uint32)0xffff) {
                        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21392, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
                        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab = (uintptr)0;
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef + 1;
                    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB)) && _cannotBeFunction(tls, pParse, pFrom) != 0) {
                        r = (int32)WRC_Abort; goto ᒐdone;
                    }
                    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_NORM))) {
                        eCodeOrig = (uint8)(@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode;
                        if (_sqlite3ViewGetColumnNames(tls, pParse, pTab) != 0) {
                            r = (int32)WRC_Abort; goto ᒐdone;
                        }
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW) {
                            if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_EnableView) == (uint64)0 && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema != (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema) {
                                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21431, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
                            }
                            _sqlite3SrcItemAttachSubquery(tls, pParse, pFrom, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab).of(TTable.ᏑFu).Reinterpret<TTableᴛ1_Fu, TTableᴛ1_Fu_Fview>()).Value.FpSelect, (int32)1);
                        } else {
                            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB && ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x100) >> 8) != 0 || (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FprepFlags) & (int32)SQLITE_PREPARE_FROM_DDL) != 0) && (@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(pTab + 64))).Value.Fp != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(pTab + 64))).Value.Fp)).Value.FeVtabRisk) > libc.BoolInt32((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_TrustedSchema) != (uint64)0)) {
                                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16458, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
                            }
                        }
                        nCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
                        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol = (int16)(-(int32)1);
                        (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode = (uint16)1;
                        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x4) >> 2) != 0) {
                            _sqlite3WalkSelect(tls, pWalker, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72))))).Value.FpSelect);
                        }
                        (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode = (uint16)eCodeOrig;
                        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol = nCol;
                    }
                }
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x2) >> 1) != 0 && _sqlite3IndexedByLookup(tls, pParse, pFrom) != 0) {
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            goto _3;
_3:;
            i = i + 1;
            pFrom += 80;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0 || _sqlite3ProcessJoin(tls, pParse, p) != 0) {
            r = (int32)WRC_Abort; goto ᒐdone;
        }
        k = 0;
        for (;;) {
            if (!(k < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            pE = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)k * 32))).Value.FpExpr;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fop) == (int32)TK_ASTERISK) {
                break;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fop) == (int32)TK_DOT && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.FpRight)).Value.Fop) == (int32)TK_ASTERISK) {
                break;
            }
            elistFlags = (Tu32)(elistFlags | (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fflags);
            goto _6;
_6:;
            k = k + 1;
        }
        if (k < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr) {
            a = pEList + 8;
            pNew = (uintptr)0;
            flags = libc.Int32FromUint64((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Fflags);
            longNames = libc.BoolInt32((flags & (int32)SQLITE_FullColNames) != 0 && (flags & (int32)SQLITE_ShortColNames) == 0);
            k = 0;
            for (;;) {
                if (!(k < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                    break;
                }
                pE = (@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(a + (uintptr)k * 32))).Value.FpExpr;
                elistFlags = (Tu32)(elistFlags | (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fflags);
                pRight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.FpRight;
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fop) != (int32)TK_ASTERISK && (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fop) != (int32)TK_DOT || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fop) != (int32)TK_ASTERISK)) {
                    pNew = _sqlite3ExprListAppend(tls, pParse, pNew, (@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(a + (uintptr)k * 32))).Value.FpExpr);
                    if (pNew != 0) {
                        (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pNew + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr - (int32)1) * 32))).Value.FzEName = (@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(a + (uintptr)k * 32))).Value.FzEName;
                        libc.SetBitFieldPtr16Uint32(pNew + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr - (int32)1) * 32 + 16 + 4, libc.Uint32FromInt32((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(a + (uintptr)k * 32 + 16 + 4)) & 0x3) >> 0)), 0, 0x3);
                        (@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(a + (uintptr)k * 32))).Value.FzEName = (uintptr)0;
                    }
                    (@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(a + (uintptr)k * 32))).Value.FpExpr = (uintptr)0;
                } else {
                    tableSeen = 0;
                    zTName = (uintptr)0;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fop) == (int32)TK_DOT) {
                        zTName = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.FpLeft + 8));
                        iErrOfst = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.FpRight + 52));
                    } else {
                        iErrOfst = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pE + 52));
                    }
                    i = 0;
                    pFrom = pTabList + 8;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
                            break;
                        }
                        pTab1 = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab;
                        zSchemaName = (uintptr)0;
                        v2 = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FzAlias;
                        zTabName = v2;
                        if (v2 == (uintptr)0) {
                            zTabName = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FzName;
                        }
                        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                            break;
                        }
                        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x4000) >> 14) != 0) {
                            pNestedFrom = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72))))).Value.FpSelect)).Value.FpEList;
                        } else {
                            if (zTName != 0 && _sqlite3StrICmp(ref (tls).DerefOrNull(), zTName, zTabName) != 0) {
                                goto _8;
                            }
                            pNestedFrom = (uintptr)0;
                            iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FpSchema);
                            if (iDb >= 0) {
                                v2 = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
                            } else {
                                v2 = __ccgo_ts + 6895;
                            }
                            zSchemaName = v2;
                        }
                        if (i + (int32)1 < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc && (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 1 * 80 + 24 + 4)) & 0x800) >> 11) != 0 && (libc.Int32FromUint16(selFlags) & (int32)SF_NestedFrom) != 0) {
                            pUsing = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 1 * 80 + 64));
                            ii = 0;
                            for (;;) {
                                if (!(ii < (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pUsing)).Value.FnId)) {
                                    break;
                                }
                                zUName = (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pUsing + 8 + (uintptr)ii * 8))).Value.FzName;
                                pRight = _sqlite3Expr(tls, db, (int32)TK_ID, zUName);
                                _sqlite3ExprSetErrorOffset(ref (tls).DerefOrNull(), pRight, iErrOfst);
                                pNew = _sqlite3ExprListAppend(tls, pParse, pNew, pRight);
                                if (pNew != 0) {
                                    pX = pNew + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr - (int32)1) * 32;
                                    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pX)).Value.FzEName = _sqlite3MPrintf(tls, db, __ccgo_ts + 21462, libc.VaList(bp + 8, zUName));
                                    libc.SetBitFieldPtr16Uint32(pX + 16 + 4, libc.Uint32FromInt32(ENAME_TAB), 0, 0x3);
                                    libc.SetBitFieldPtr16Uint32(pX + 16 + 4, libc.Uint32FromInt32(1), 7, 0x80);
                                }
                                goto _11;
_11:;
                                ii = ii + 1;
                            }
                        } else {
                            pUsing = (uintptr)0;
                        }
                        nAdd = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FnCol;
                        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FtabFlags & (uint32)TF_NoVisibleRowid) == (uint32)0 && (libc.Int32FromUint16(selFlags) & (int32)SF_NestedFrom) != 0) {
                            nAdd = nAdd + 1;
                        }
                        j = 0;
                        for (;;) {
                            if (!(j < nAdd)) {
                                break;
                            }
                            if (j == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FnCol) {
                                zName = _sqlite3RowidAlias(ref (tls).DerefOrNull(), pTab1);
                                if (zName == (uintptr)0) {
                                    goto _12;
                                }
                            } else {
                                zName = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FaCol + (uintptr)j * 16))).Value.FzCnName;
                                if (pNestedFrom != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pNestedFrom + 8 + (uintptr)j * 32 + 16 + 4)) & 0x3) >> 0) == (int32)ENAME_ROWID) {
                                    goto _12;
                                }
                                if (zTName != 0 && pNestedFrom != 0 && _sqlite3MatchEName(tls, pNestedFrom + 8 + (uintptr)j * 32, (uintptr)0, zTName, (uintptr)0, nil) == 0) {
                                    goto _12;
                                }
                                if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_IncludeHidden) == (uint32)0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (int32)COLFLAG_HIDDEN) != 0) {
                                    goto _12;
                                }
                                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (int32)COLFLAG_NOEXPAND) != 0 && zTName == (uintptr)0 && (libc.Int32FromUint16(selFlags) & (int32)SF_NestedFrom) == 0) {
                                    goto _12;
                                }
                            }
                            tableSeen = (int32)1;
                            if (i > 0 && zTName == (uintptr)0 && (libc.Int32FromUint16(selFlags) & (int32)SF_NestedFrom) == 0) {
                                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x800) >> 11) != 0 && _sqlite3IdListIndex(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 64)), zName) >= 0) {
                                    goto _12;
                                }
                            }
                            pRight = _sqlite3Expr(tls, db, (int32)TK_ID, zName);
                            if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc > (int32)1 && ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.Ffg.Fjointype) & (int32)JT_LTORJ) == 0 || zTName != (uintptr)0 || (libc.Int32FromUint16(selFlags) & (int32)SF_NestedFrom) != 0 || !(_inAnyUsingClause(ref (tls).DerefOrNull(), zName, pFrom, (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc - i - (int32)1) != 0)) || libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
                                pLeft = _sqlite3Expr(tls, db, (int32)TK_ID, zTabName);
                                pExpr = _sqlite3PExpr(tls, pParse, (int32)TK_DOT, pLeft, pRight);
                                if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.FpLeft != 0) {
                                    _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, pLeft, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.FpLeft);
                                }
                                if (zSchemaName != 0) {
                                    pLeft = _sqlite3Expr(tls, db, (int32)TK_ID, zSchemaName);
                                    pExpr = _sqlite3PExpr(tls, pParse, (int32)TK_DOT, pLeft, pExpr);
                                }
                            } else {
                                pExpr = pRight;
                            }
                            _sqlite3ExprSetErrorOffset(ref (tls).DerefOrNull(), pExpr, iErrOfst);
                            pNew = _sqlite3ExprListAppend(tls, pParse, pNew, pExpr);
                            if (pNew == (uintptr)0) {
                                break;
                            }
                            pX1 = pNew + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr - (int32)1) * 32;
                            if ((libc.Int32FromUint16(selFlags) & (int32)SF_NestedFrom) != 0 && !(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
                                if (pNestedFrom != 0 && libc.Bool(libc.Bool(!(libc.Int32FromInt32(ViewCanHaveRowid) != 0)) || j < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNestedFrom)).Value.FnExpr)) {
                                    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pX1)).Value.FzEName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pNestedFrom + 8 + (uintptr)j * 32))).Value.FzEName);
                                } else {
                                    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pX1)).Value.FzEName = _sqlite3MPrintf(tls, db, __ccgo_ts + 21467, libc.VaList(bp + 8, zSchemaName, zTabName, zName));
                                }
                                if (j == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FnCol) {
                                    v1 = (int32)ENAME_ROWID;
                                } else {
                                    v1 = (int32)ENAME_TAB;
                                }
                                libc.SetBitFieldPtr16Uint32(pX1 + 16 + 4, libc.Uint32FromInt32(v1), 0, 0x3);
                                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x800) >> 11) != 0 && _sqlite3IdListIndex(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 64)), zName) >= 0 || pUsing != 0 && _sqlite3IdListIndex(ref (tls).DerefOrNull(), pUsing, zName) >= 0 || j < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FnCol && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (int32)COLFLAG_NOEXPAND) != 0) {
                                    libc.SetBitFieldPtr16Uint32(pX1 + 16 + 4, libc.Uint32FromInt32(1), 8, 0x100);
                                }
                            } else {
                                if (longNames != 0) {
                                    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pX1)).Value.FzEName = _sqlite3MPrintf(tls, db, __ccgo_ts + 13747, libc.VaList(bp + 8, zTabName, zName));
                                    libc.SetBitFieldPtr16Uint32(pX1 + 16 + 4, libc.Uint32FromInt32(ENAME_NAME), 0, 0x3);
                                } else {
                                    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pX1)).Value.FzEName = _sqlite3DbStrDup(tls, db, zName);
                                    libc.SetBitFieldPtr16Uint32(pX1 + 16 + 4, libc.Uint32FromInt32(ENAME_NAME), 0, 0x3);
                                }
                            }
                            goto _12;
_12:;
                            j = j + 1;
                        }
                        goto _8;
_8:;
                        i = i + 1;
                        pFrom += 80;
                    }
                    if (!(tableSeen != 0)) {
                        if (zTName != 0) {
                            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21476, libc.VaList(bp + 8, zTName));
                        } else {
                            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21494, 0);
                        }
                    }
                }
                goto _7;
_7:;
                k = k + 1;
            }
            _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pEList);
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList = pNew;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList != 0) {
            if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr > ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + 2 * 4))) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21514, 0);
                r = (int32)WRC_Abort; goto ᒐdone;
            }
            if ((Tu32)(elistFlags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_HasFunc) | libc.Int32FromInt32(EP_Subquery))) != (uint32)0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_ComplexResult;
            }
        }
        r = WRC_Continue;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3AddPrimaryKey(ж<libc.TLS> tls, uintptr pParse, uintptr pList, int32 onError, int32 autoInc, int32 sortOrder) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        int32 iCol = default;
        int32 nTerm = default;
        uintptr pCExpr = default;
        uintptr pCExpr1 = default;
        uintptr pCol = default;
        uintptr pTab = default;
        _ = i;
        _ = iCol;
        _ = nTerm;
        _ = pCExpr;
        _ = pCExpr1;
        _ = pCol;
        _ = pTab;
        pTab = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTable;
        pCol = (uintptr)0;
        iCol = -(int32)1;
        if (pTab == (uintptr)0) {
            goto primary_key_exit;
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasPrimaryKey) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 14275, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
            goto primary_key_exit;
        }
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= (uint32)TF_HasPrimaryKey;
        if (pList == (uintptr)0) {
            iCol = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol - (int32)1;
            pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16;
            _makeColumnPartOfPrimaryKey(tls, pParse, pCol);
            nTerm = (int32)1;
        } else {
            nTerm = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr;
            i = 0;
            for (;;) {
                if (!(i < nTerm)) {
                    break;
                }
                pCExpr = _sqlite3ExprSkipCollate(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr);
                _sqlite3StringToId(ref (tls).DerefOrNull(), pCExpr);
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCExpr)).Value.Fop) == (int32)TK_ID) {
                    iCol = _sqlite3ColumnIndex(ref (tls).DerefOrNull(), pTab, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCExpr + 8)));
                    if (iCol >= 0) {
                        pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16;
                        _makeColumnPartOfPrimaryKey(tls, pParse, pCol);
                    }
                }
                goto _1;
_1:;
                i = i + 1;
            }
        }
        if (nTerm == (int32)1 && pCol != 0 && (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pCol + 8)) & 0xf0) >> 4) == (int32)COLTYPE_INTEGER && sortOrder != (int32)SQLITE_SO_DESC) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME && pList != 0) {
                pCExpr1 = _sqlite3ExprSkipCollate(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8))).Value.FpExpr);
                _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, pTab + 52, pCExpr1);
            }
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey = (int16)iCol;
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FkeyConf = libc.Uint8FromInt32(onError);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= libc.Uint32FromInt32(autoInc * (int32)TF_Autoincrement);
            if (pList != 0) {
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiPkSortOrder = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8))).Value.Ffg.FsortFlags;
            }
            _sqlite3HasExplicitNulls(tls, pParse, pList);
        } else {
            if (autoInc != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 14316, 0);
            } else {
                _sqlite3CreateIndex(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0, pList, onError, (uintptr)0, (uintptr)0, sortOrder, 0, (uint8)SQLITE_IDXTYPE_PRIMARYKEY);
                pList = (uintptr)0;
            }
        }
        goto primary_key_exit;
primary_key_exit:;
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pList);
        return;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct _sqlite3AddReturning_type /*dyn*/ {
    [FieldOffset(0)] public uintptr FpReturning;
}

internal static void _sqlite3AddReturning(ж<libc.TLS> tls, uintptr pParse, uintptr pList) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr pHash = default;
        uintptr pRet = default;
        _ = db;
        _ = pHash;
        _ = pRet;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTrigger != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 14017, 0);
        } else {
        }
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 3, 0x8);
        pRet = _sqlite3DbMallocZero(tls, db, (uint64)232);
        if (pRet == (uintptr)0) {
            _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pList);
            return;
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse).of(TParse.ᏑFu1).Reinterpret<TParseᴛ1_Fu1, _sqlite3AddReturning_type>()).Value.FpReturning = pRet;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FpReturnEL = pList;
        _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3DeleteReturning), pRet);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            return;
        }
        Xsqlite3_snprintf(tls, (int32)40, pRet + 188, __ccgo_ts + 14051, libc.VaList(bp + 8, pParse));
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.FzName = pRet + 188;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.Fop = (uint8)TK_RETURNING;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.Ftr_tm = (uint8)TRIGGER_AFTER;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.FbReturning = (uint8)1;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.FpSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.FpTabSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTrig.Fstep_list = pRet + 88;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTStep.Fop = (uint8)TK_RETURNING;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTStep.FpTrig = pRet + 16;
        (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FretTStep.FpExprList = pList;
        pHash = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema + 56;
        if (_sqlite3HashInsert(tls, pHash, pRet + 188, pRet + 16) == pRet + 16) {
            _sqlite3OomFault(tls, db);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3AnalysisLoad(ж<libc.TLS> tls, uintptr db, int32 iDb) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr i = default;
        uintptr pIdx = default;
        uintptr pIdx1 = default;
        uintptr pIdx2 = default;
        uintptr pSchema = default;
        uintptr pStat1 = default;
        uintptr pTab = default;
        uintptr zSql = default;
        uintptr v3 = default;
        int32 rc = default;
        int32 v5 = default;
        TanalysisInfo _ᴛ1 = default;
        _ = i;
        _ = pIdx;
        _ = pIdx1;
        _ = pIdx2;
        _ = pSchema;
        _ = pStat1;
        _ = pTab;
        _ = rc;
        _ = zSql;
        _ = v3;
        _ = v5;
        rc = SQLITE_OK;
        pSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema;
        i = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(pSchema + 8))).Value.Ffirst;
        for (;;) {
            if (!(i != 0)) {
                break;
            }
            pTab = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fdata;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(TF_HasStat1));
            goto _1;
_1:;
            i = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fnext;
        }
        i = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(pSchema + 32))).Value.Ffirst;
        for (;;) {
            if (!(i != 0)) {
                break;
            }
            pIdx = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fdata;
            libc.SetBitFieldPtr16Uint32(pIdx + 100, libc.Uint32FromInt32(0), 7, 0x80);
            _sqlite3DeleteIndexSamples(tls, (@unsafe.Pointer)db, pIdx);
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample = (uintptr)0;
            goto _2;
_2:;
            i = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fnext;
        }
        (@unsafe.Pointer.To<TanalysisInfo>(__ccgo_ptr(bp))).Value.Fdb = db;
        (@unsafe.Pointer.To<TanalysisInfo>(__ccgo_ptr(bp))).Value.FzDatabase = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
        v3 = _sqlite3FindTable(tls, db, __ccgo_ts + 12948, (@unsafe.Pointer.To<TanalysisInfo>(__ccgo_ptr(bp))).Value.FzDatabase);
        pStat1 = v3;
        if (v3 != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pStat1)).Value.FeTabType) == TABTYP_NORM) {
            zSql = _sqlite3MPrintf(tls, db, __ccgo_ts + 13321, libc.VaList(bp + 24, (@unsafe.Pointer.To<TanalysisInfo>(__ccgo_ptr(bp))).Value.FzDatabase));
            if (zSql == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                rc = _sqlite3_exec__ccgo_gp(tls, db, zSql, __ccgo_fp(_analysisLoader), bp, nil);
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zSql);
            }
        }
        i = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(pSchema + 32))).Value.Ffirst;
        for (;;) {
            if (!(i != 0)) {
                break;
            }
            pIdx1 = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fdata;
            if (!((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx1 + 100)) & 0x80) >> 7) != 0)) {
                _sqlite3DefaultRowEst(tls, pIdx1);
            }
            goto _4;
_4:;
            i = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fnext;
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable + 1;
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.Fsz = (uint16)0;
            rc = _loadStat4(tls, db, (@unsafe.Pointer.To<TanalysisInfo>(__ccgo_ptr(bp))).Value.FzDatabase);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable - 1;
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FbDisable != 0) {
                v5 = 0;
            } else {
                v5 = libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FszTrue);
            }
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.Fsz = libc.Uint16FromInt32(v5);
        }
        i = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(pSchema + 32))).Value.Ffirst;
        for (;;) {
            if (!(i != 0)) {
                break;
            }
            pIdx2 = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fdata;
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx2)).Value.FaiRowEst);
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx2)).Value.FaiRowEst = (uintptr)0;
            goto _6;
_6:;
            i = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)i)).Value.Fnext;
        }
        if (rc == (int32)SQLITE_NOMEM) {
            _sqlite3OomFault(tls, db);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3BeginTrigger(ж<libc.TLS> tls, uintptr pParse, uintptr pName1, uintptr pName2, int32 tr_tm, int32 op, uintptr pColumns, uintptr pTableName, uintptr pWhen, int32 isTemp, int32 noErr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        int32 code = default;
        int32 iDb = default;
        int32 iTabDb = default;
        int32 v4 = default;
        uintptr db = default;
        ref var pName = ref heap(new uintptr(), out var ᏑpName);
        uintptr pTab = default;
        uintptr pTrigger = default;
        uintptr zDb = default;
        uintptr zDbTrig = default;
        uintptr zName = default;
        uintptr v1 = default;
        TDbFixer _ᴛ1 = new();
        _ = code;
        _ = db;
        _ = iDb;
        _ = iTabDb;
        _ = pName;
        _ = pTab;
        _ = pTrigger;
        _ = zDb;
        _ = zDbTrig;
        _ = zName;
        _ = v1;
        _ = v4;
        pTrigger = (uintptr)0;
        zName = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if (isTemp != 0) {
            if ((@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pName2)).Value.Fn > (uint32)0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21973, 0);
                goto trigger_cleanup;
            }
            iDb = (int32)1;
            pName = pName1;
        } else {
            iDb = _sqlite3TwoPartName(tls, pParse, pName1, pName2, @unsafe.Pointer.FromBox(ᏑpName));
            if (iDb < 0) {
                goto trigger_cleanup;
            }
        }
        if (!(pTableName != 0) || (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            goto trigger_cleanup;
        }
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0 && iDb != (int32)1) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTableName + 8 + 72)));
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTableName + 8 + 72))).Value = (uintptr)0;
        }
        pTab = _sqlite3SrcListLookup(tls, pParse, pTableName);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy) == 0 && (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pName2)).Value.Fn == (uint32)0 && pTab != 0 && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema == (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FpSchema) {
            iDb = (int32)1;
        }
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            goto trigger_cleanup;
        }
        _sqlite3FixInit(ref (tls).DerefOrNull(), bp, pParse, iDb, __ccgo_ts + 22019, pName);
        if (_sqlite3FixSrcList(tls, bp, pTableName) != 0) {
            goto trigger_cleanup;
        }
        pTab = _sqlite3SrcListLookup(tls, pParse, pTableName);
        if (!(pTab != 0)) {
            goto trigger_orphan_error;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22027, 0);
            goto trigger_orphan_error;
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Shadow) != (uint32)0 && _sqlite3ReadOnlyShadowTables(ref (tls).DerefOrNull(), db) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22068, 0);
            goto trigger_orphan_error;
        }
        zName = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pName);
        if (zName == (uintptr)0) {
            goto trigger_cleanup;
        }
        if (_sqlite3CheckObjectName(tls, pParse, zName, __ccgo_ts + 22019, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName) != 0) {
            goto trigger_cleanup;
        }
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
            if (_sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema + 56), zName) != 0) {
                if (!(noErr != 0)) {
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22108, libc.VaList(bp + 104, pName));
                } else {
                    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0) {
                        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 1732, 0);
                    } else {
                        _sqlite3CodeVerifySchema(tls, pParse, iDb);
                    }
                }
                goto trigger_cleanup;
            }
        }
        if (Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, __ccgo_ts + 6830, (int32)7) == 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22134, 0);
            goto trigger_cleanup;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW && tr_tm != (int32)TK_INSTEAD) {
            if (tr_tm == (int32)TK_BEFORE) {
                v1 = __ccgo_ts + 22172;
            } else {
                v1 = __ccgo_ts + 22179;
            }
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22185, libc.VaList(bp + 104, v1, pTableName + 8));
            goto trigger_orphan_error;
        }
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VIEW)) && tr_tm == (int32)TK_INSTEAD) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22222, libc.VaList(bp + 104, pTableName + 8));
            goto trigger_orphan_error;
        }
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
            iTabDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
            code = (int32)SQLITE_CREATE_TRIGGER;
            zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iTabDb * 32))).Value.FzDbSName;
            if (isTemp != 0) {
                v1 = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + 1 * 32))).Value.FzDbSName;
            } else {
                v1 = zDb;
            }
            zDbTrig = v1;
            if (iTabDb == (int32)1 || isTemp != 0) {
                code = (int32)SQLITE_CREATE_TEMP_TRIGGER;
            }
            if (_sqlite3AuthCheck(tls, pParse, code, zName, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, zDbTrig) != 0) {
                goto trigger_cleanup;
            }
            if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && iTabDb == (int32)1) {
                v1 = __ccgo_ts + 6838;
            } else {
                v1 = __ccgo_ts + 6370;
            }
            if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_INSERT, v1, (uintptr)0, zDb) != 0) {
                goto trigger_cleanup;
            }
        }
        if (tr_tm == (int32)TK_INSTEAD) {
            tr_tm = (int32)TK_BEFORE;
        }
        pTrigger = _sqlite3DbMallocZero(tls, db, (uint64)72);
        if (pTrigger == (uintptr)0) {
            goto trigger_cleanup;
        }
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FzName = zName;
        zName = (uintptr)0;
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.Ftable = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTableName + 8))).Value.FzName);
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema;
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpTabSchema = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema;
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.Fop = libc.Uint8FromInt32(op);
        if (tr_tm == (int32)TK_BEFORE) {
            v4 = (int32)TRIGGER_BEFORE;
        } else {
            v4 = (int32)TRIGGER_AFTER;
        }
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.Ftr_tm = libc.Uint8FromInt32(v4);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.Ftable, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTableName + 8))).Value.FzName);
            (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpWhen = pWhen;
            pWhen = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpWhen = _sqlite3ExprDup(tls, db, pWhen, (int32)EXPRDUP_REDUCE);
        }
        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpColumns = pColumns;
        pColumns = (uintptr)0;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTrigger = pTrigger;
        goto trigger_cleanup;
trigger_cleanup:;
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zName);
        _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pTableName);
        _sqlite3IdListDelete(tls, (@unsafe.Pointer)db, pColumns);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhen);
        if (!((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTrigger != 0)) {
            _sqlite3DeleteTrigger(tls, (@unsafe.Pointer)db, pTrigger);
        } else {
        }
        return;
        goto trigger_orphan_error;
trigger_orphan_error:;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb) == (int32)1) {
            libc.SetBitFieldPtr8Uint32(db + 200 + 8, libc.Uint32FromInt32(1), 0, 0x1);
        }
        goto trigger_cleanup;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3CheckObjectName(ж<libc.TLS> tls, uintptr pParse, uintptr zName, uintptr zType, uintptr zTblName) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        _ = db;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if (_sqlite3WritableSchema(ref (tls).DerefOrNull(), db) != 0 || (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x6) >> 1) != 0 || !(_sqlite3Config.FbExtraSchemaChecks != 0)) {
            r = SQLITE_OK; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0) {
            if (Xsqlite3_stricmp(tls, zType, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FazInit))) != 0 || Xsqlite3_stricmp(tls, zName, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FazInit + 1 * 8))) != 0 || Xsqlite3_stricmp(tls, zTblName, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FazInit + 2 * 8))) != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 1732, 0);
                r = (int32)SQLITE_ERROR; goto ᒐdone;
            }
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0 && 0 == Xsqlite3_strnicmp(tls, zName, __ccgo_ts + 6830, (int32)7) || _sqlite3ReadOnlyShadowTables(ref (tls).DerefOrNull(), db) != 0 && _sqlite3ShadowTableName(tls, db, zName) != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13878, libc.VaList(bp + 8, zName));
                r = (int32)SQLITE_ERROR; goto ᒐdone;
            }
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3CreateFunc(ж<libc.TLS> tls, uintptr db, uintptr zFunctionName, int32 nArg, int32 enc, uintptr pUserData, uintptr __ccgo_fp_xSFunc, uintptr __ccgo_fp_xStep, uintptr __ccgo_fp_xFinal, uintptr __ccgo_fp_xValue, uintptr __ccgo_fp_xInverse, uintptr pDestructor) {
    int32 extraFlags = default;
    int32 rc = default;
    uintptr p = default;
    uintptr v1 = default;
    _ = extraFlags;
    _ = p;
    _ = rc;
    _ = v1;
    if (zFunctionName == (uintptr)0 || __ccgo_fp_xSFunc != (uintptr)0 && __ccgo_fp_xFinal != (uintptr)0 || libc.BoolInt32(__ccgo_fp_xFinal == (uintptr)0) != libc.BoolInt32(__ccgo_fp_xStep == (uintptr)0) || libc.BoolInt32(__ccgo_fp_xValue == (uintptr)0) != libc.BoolInt32(__ccgo_fp_xInverse == (uintptr)0) || (nArg < -(int32)1 || nArg > (int32)SQLITE_MAX_FUNCTION_ARG) || (int32)255 < _sqlite3Strlen30(tls, zFunctionName)) {
        return _sqlite3MisuseError(tls, (int32)190761);
    }
    extraFlags = enc & (libc.Int32FromInt32(SQLITE_DETERMINISTIC) | libc.Int32FromInt32(SQLITE_DIRECTONLY) | libc.Int32FromInt32(SQLITE_SUBTYPE) | libc.Int32FromInt32(SQLITE_INNOCUOUS) | libc.Int32FromInt32(SQLITE_RESULT_SUBTYPE) | libc.Int32FromInt32(SQLITE_SELFORDER1));
    enc = enc & (libc.Int32FromInt32(SQLITE_FUNC_ENCMASK) | libc.Int32FromInt32(SQLITE_ANY));
    extraFlags = extraFlags ^ (int32)SQLITE_FUNC_UNSAFE;
    var encᴛ1 = enc;
    var matchᴛ1 = false;
    if (encᴛ1 == (int32)SQLITE_UTF16) { matchᴛ1 = true;
        enc = (int32)SQLITE_UTF16LE;
    }
    else if (encᴛ1 == (int32)SQLITE_ANY) { matchᴛ1 = true;
        rc = _sqlite3CreateFunc(tls, db, zFunctionName, nArg, ((int32)SQLITE_UTF8 | extraFlags) ^ (int32)SQLITE_FUNC_UNSAFE, pUserData, __ccgo_fp_xSFunc, __ccgo_fp_xStep, __ccgo_fp_xFinal, __ccgo_fp_xValue, __ccgo_fp_xInverse, pDestructor);
        if (rc == SQLITE_OK) {
            rc = _sqlite3CreateFunc(tls, db, zFunctionName, nArg, ((int32)SQLITE_UTF16LE | extraFlags) ^ (int32)SQLITE_FUNC_UNSAFE, pUserData, __ccgo_fp_xSFunc, __ccgo_fp_xStep, __ccgo_fp_xFinal, __ccgo_fp_xValue, __ccgo_fp_xInverse, pDestructor);
        }
        if (rc != SQLITE_OK) {
            return rc;
        }
        enc = (int32)SQLITE_UTF16BE;
    }
    else if (encᴛ1 == (int32)SQLITE_UTF8) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && encᴛ1 == (int32)SQLITE_UTF16LE) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && encᴛ1 == (int32)SQLITE_UTF16BE) {
    }
    else if (!matchᴛ1) { /* default: */
        do {
            enc = (int32)SQLITE_UTF8;
            break;
        } while (false);
    }

    p = _sqlite3FindFunction(tls, db, zFunctionName, nArg, libc.Uint8FromInt32(enc), (uint8)0);
    if (p != 0 && (Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags & (uint32)SQLITE_FUNC_ENCMASK) == libc.Uint32FromInt32(enc) && (int32)(@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FnArg == nArg) {
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive != 0) {
            _sqlite3ErrorWithMsg(tls, db, (int32)SQLITE_BUSY, __ccgo_ts + 26081, 0);
            return (int32)SQLITE_BUSY;
        } else {
            _sqlite3ExpirePreparedStatements(ref (tls).DerefOrNull(), db, 0);
        }
    } else {
        if (__ccgo_fp_xSFunc == (uintptr)0 && __ccgo_fp_xFinal == (uintptr)0) {
            return SQLITE_OK;
        }
    }
    p = _sqlite3FindFunction(tls, db, zFunctionName, nArg, libc.Uint8FromInt32(enc), (uint8)1);
    if (!(p != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    _functionDestroy(tls, db, p);
    if (pDestructor != 0) {
        (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FnRef = (@unsafe.Pointer.To<TFuncDestructor>((@unsafe.Pointer)pDestructor)).Value.FnRef + 1;
    }
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64))).Value = pDestructor;
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags = (Tu32)((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags & (uint32)SQLITE_FUNC_ENCMASK) | libc.Uint32FromInt32(extraFlags));
    if (__ccgo_fp_xSFunc != 0) {
        v1 = __ccgo_fp_xSFunc;
    } else {
        v1 = __ccgo_fp_xStep;
    }
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxSFunc = v1;
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxFinalize = __ccgo_fp_xFinal;
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxValue = __ccgo_fp_xValue;
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxInverse = __ccgo_fp_xInverse;
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FpUserData = pUserData;
    (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FnArg = libc.Int16FromUint16(libc.Uint16FromInt32(nArg));
    return SQLITE_OK;
}

internal static void _sqlite3DeleteFrom(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pWhere, uintptr pOrderBy, uintptr pLimit) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        uintptr aToOpen = default;
        uintptr db = default;
        uintptr pIdx = default;
        uintptr pPk = default;
        uintptr pTab = default;
        uintptr pTrigger = default;
        uintptr pVTab = default;
        uintptr pWInfo = default;
        uintptr v = default;
        uintptr v3 = default;
        int32 addrBypass = default;
        int32 addrEphOpen = default;
        int32 addrLoop = default;
        int32 bComplex = default;
        int32 count = default;
        int32 eOnePass = default;
        int32 i = default;
        int32 iAddrOnce = default;
        ref var iDataCur = ref heap(new int32(), out var ᏑiDataCur);
        int32 iDb = default;
        int32 iEphCur = default;
        ref var iIdxCur = ref heap(new int32(), out var ᏑiIdxCur);
        int32 iKey = default;
        int32 iPk = default;
        int32 iRowSet = default;
        int32 iTabCur = default;
        int32 isView = default;
        int32 memCnt = default;
        int32 nIdx = default;
        int32 rcauth = default;
        int32 v1 = default;
        int32 v2 = default;
        Ti16 nKey = default;
        Ti16 nPk = default;
        Tu16 wcf = default;
        array<int32> _ᴛ1 = new(2);
        TAuthContext _ᴛ2 = default;
        TNameContext _ᴛ3 = new();
        _ = aToOpen;
        _ = addrBypass;
        _ = addrEphOpen;
        _ = addrLoop;
        _ = bComplex;
        _ = count;
        _ = db;
        _ = eOnePass;
        _ = i;
        _ = iAddrOnce;
        _ = iDataCur;
        _ = iDb;
        _ = iEphCur;
        _ = iIdxCur;
        _ = iKey;
        _ = iPk;
        _ = iRowSet;
        _ = iTabCur;
        _ = isView;
        _ = memCnt;
        _ = nIdx;
        _ = nKey;
        _ = nPk;
        _ = pIdx;
        _ = pPk;
        _ = pTab;
        _ = pTrigger;
        _ = pVTab;
        _ = pWInfo;
        _ = rcauth;
        _ = v;
        _ = wcf;
        _ = v1;
        _ = v2;
        _ = v3;
        iDataCur = 0;
        iIdxCur = 0;
        memCnt = 0;
        aToOpen = (uintptr)0;
        iPk = 0;
        nPk = (int16)1;
        iEphCur = 0;
        iRowSet = 0;
        addrBypass = 0;
        addrLoop = 0;
        addrEphOpen = 0;
        libc.Xmemset(tls, bp, 0, (uint64)16);
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto delete_from_cleanup;
        }
        pTab = _sqlite3SrcListLookup(tls, pParse, pTabList);
        if (pTab == (uintptr)0) {
            goto delete_from_cleanup;
        }
        pTrigger = _sqlite3TriggersExist(tls, pParse, pTab, (int32)TK_DELETE, (uintptr)0, nil);
        isView = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW);
        bComplex = libc.BoolInt32(pTrigger != 0 || _sqlite3FkRequired(tls, pParse, pTab, (uintptr)0, 0) != 0);
        if (_sqlite3ViewGetColumnNames(tls, pParse, pTab) != 0) {
            goto delete_from_cleanup;
        }
        if (_sqlite3IsReadOnly(tls, pParse, pTab, pTrigger) != 0) {
            goto delete_from_cleanup;
        }
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
        rcauth = _sqlite3AuthCheck(tls, pParse, (int32)SQLITE_DELETE, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (uintptr)0, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName);
        if (rcauth == (int32)SQLITE_DENY) {
            goto delete_from_cleanup;
        }
        v3 = pParse + 56;
        v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
        v1 = v2;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.FiCursor = v1;
        iTabCur = v1;
        nIdx = 0;
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab + 1;
            goto _4;
_4:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
            nIdx = nIdx + 1;
        }
        if (isView != 0) {
            _sqlite3AuthContextPush(ref (tls).DerefOrNull(), pParse, __ccgo_ptr(bp), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
        }
        v = _sqlite3GetVdbe(tls, pParse);
        if (v == (uintptr)0) {
            goto delete_from_cleanup;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0) {
            _sqlite3VdbeCountChanges(ref (tls).DerefOrNull(), v);
        }
        _sqlite3BeginWriteOperation(tls, pParse, bComplex, iDb);
        if (isView != 0) {
            _sqlite3MaterializeView(tls, pParse, pTab, pWhere, pOrderBy, pLimit, iTabCur);
            v1 = iTabCur;
            iIdxCur = v1;
            iDataCur = v1;
            pOrderBy = (uintptr)0;
            pLimit = (uintptr)0;
        }
        libc.Xmemset(tls, bp + 16, 0, (uint64)56);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16))).Value.FpSrcList = pTabList;
        if (_sqlite3ResolveExprNames(tls, bp + 16, pWhere) != 0) {
            goto delete_from_cleanup;
        }
        if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00001)).Lsh((int64)(libc.Int32FromInt32(32))))) != (uint64)0 && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab != 0) && !((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x8) >> 3)) != 0)) {
            v3 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
            memCnt = v1;
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, memCnt);
        }
        if (rcauth == SQLITE_OK && pWhere == (uintptr)0 && !(bComplex != 0) && !(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB)) && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxPreUpdateCallback == (uintptr)0) {
            _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum, (uint8)1, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                if (memCnt != 0) {
                    v1 = memCnt;
                } else {
                    v1 = -(int32)1;
                }
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_Clear, libc.Int32FromUint32((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum), iDb, v1, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, -(int32)1);
            }
            pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
            for (;;) {
                if (!(pIdx != 0)) {
                    break;
                }
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY && !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
                    if (memCnt != 0) {
                        v1 = memCnt;
                    } else {
                        v1 = -(int32)1;
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Clear, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum), iDb, v1);
                } else {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Clear, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum), iDb);
                }
                goto _9;
_9:;
                pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
            }
        } else {
            wcf = libc.Uint16FromInt32(libc.Int32FromInt32(WHERE_ONEPASS_DESIRED) | libc.Int32FromInt32(WHERE_DUPLICATES_OK));
            if (((@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16))).Value.FncFlags & (int32)NC_Subquery) != 0) {
                bComplex = (int32)1;
            }
            if (bComplex != 0) {
                v1 = 0;
            } else {
                v1 = (int32)WHERE_ONEPASS_MULTIROW;
            }
            wcf = libc.Uint16FromInt32((int32)wcf | v1);
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                pPk = (uintptr)0;
                v3 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                iRowSet = v1;
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, iRowSet);
            } else {
                pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
                nPk = libc.Int16FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol);
                iPk = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)nPk;
                v3 = pParse + 56;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                iEphCur = v1;
                addrEphOpen = _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, iEphCur, (int32)nPk);
                _sqlite3VdbeSetP4KeyInfo(tls, pParse, pPk);
            }
            pWInfo = _sqlite3WhereBegin(tls, pParse, pTabList, pWhere, (uintptr)0, (uintptr)0, (uintptr)0, wcf, iTabCur + (int32)1);
            if (pWInfo == (uintptr)0) {
                goto delete_from_cleanup;
            }
            eOnePass = _sqlite3WhereOkOnePass(tls, pWInfo, bp + 72);
            if (eOnePass != (int32)ONEPASS_SINGLE) {
                _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
            }
            if (_sqlite3WhereUsesDeferredSeek(ref (tls).DerefOrNull(), pWInfo) != 0) {
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_FinishSeek, iTabCur);
            }
            if (memCnt != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_AddImm, memCnt, (int32)1);
            }
            if (pPk != 0) {
                i = 0;
                for (;;) {
                    if (!(i < (int32)nPk)) {
                        break;
                    }
                    _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iTabCur, (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2))), iPk + i);
                    goto _16;
_16:;
                    i = i + 1;
                }
                iKey = iPk;
            } else {
                v3 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                iKey = v1;
                _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iTabCur, -(int32)1, iKey);
            }
            if (eOnePass != ONEPASS_OFF) {
                nKey = nPk;
                aToOpen = _sqlite3DbMallocRawNN(tls, db, libc.Uint64FromInt32(nIdx + (int32)2));
                if (aToOpen == (uintptr)0) {
                    _sqlite3WhereEnd(tls, pWInfo);
                    goto delete_from_cleanup;
                }
                libc.Xmemset(tls, aToOpen, (int32)1, libc.Uint64FromInt32(nIdx + (int32)1));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(nIdx + (int32)1)))).Value = (uint8)0;
                if (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), 0, 2) >= 0) {
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), 0, 2) - iTabCur)))).Value = (uint8)0;
                }
                if (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2) >= 0) {
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2) - iTabCur)))).Value = (uint8)0;
                }
                if (addrEphOpen != 0) {
                    _sqlite3VdbeChangeToNoop(tls, v, addrEphOpen);
                }
                addrBypass = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            } else {
                if (pPk != 0) {
                    v3 = pParse + 60;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                    iKey = v1;
                    nKey = 0;
                    _sqlite3VdbeAddOp4(tls, v, (int32)OP_MakeRecord, iPk, (int32)nPk, iKey, _sqlite3IndexAffinityStr(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pPk), (int32)nPk);
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, iEphCur, iKey, iPk, (int32)nPk);
                } else {
                    nKey = (int16)1;
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_RowSetAdd, iRowSet, iKey);
                }
                _sqlite3WhereEnd(tls, pWInfo);
            }
            if (!(isView != 0)) {
                iAddrOnce = 0;
                if (eOnePass == (int32)ONEPASS_MULTI) {
                    iAddrOnce = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Once);
                }
                _sqlite3OpenTableAndIndices(tls, pParse, pTab, (int32)OP_OpenWrite, (uint8)OPFLAG_FORDELETE, iTabCur, aToOpen, @unsafe.Pointer.FromPinnedBox(ᏑiDataCur), @unsafe.Pointer.FromPinnedBox(ᏑiIdxCur));
                if (eOnePass == (int32)ONEPASS_MULTI) {
                    _sqlite3VdbeJumpHereOrPopInst(ref (tls).DerefOrNull(), v, iAddrOnce);
                }
            }
            if (eOnePass != ONEPASS_OFF) {
                if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB)) && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(iDataCur - iTabCur))) != 0) {
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iDataCur, addrBypass, iKey, (int32)nKey);
                }
            } else {
                if (pPk != 0) {
                    addrLoop = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, iEphCur);
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iEphCur, 0, iKey);
                    } else {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_RowData, iEphCur, iKey);
                    }
                } else {
                    addrLoop = _sqlite3VdbeAddOp3(tls, v, (int32)OP_RowSetRead, iRowSet, 0, iKey);
                }
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                pVTab = _sqlite3GetVTable(ref (tls).DerefOrNull(), db, pTab);
                _sqlite3VtabMakeWritable(tls, pParse, pTab);
                _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
                if (eOnePass == (int32)ONEPASS_SINGLE) {
                    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, iTabCur);
                    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel == (uintptr)0) {
                        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FisMultiWrite = (uint8)0;
                    }
                }
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_VUpdate, 0, (int32)1, iKey, pVTab, -(int32)12);
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OE_Abort);
            } else {
                count = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0);
                _sqlite3GenerateRowDelete(tls, pParse, pTab, pTrigger, iDataCur, iIdxCur, iKey, nKey, libc.Uint8FromInt32(count), (uint8)OE_Default, libc.Uint8FromInt32(eOnePass), @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2));
            }
            if (eOnePass != ONEPASS_OFF) {
                _sqlite3VdbeResolveLabel(tls, v, addrBypass);
                _sqlite3WhereEnd(tls, pWInfo);
            } else {
                if (pPk != 0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iEphCur, addrLoop + (int32)1);
                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrLoop);
                } else {
                    _sqlite3VdbeGoto(tls, v, addrLoop);
                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrLoop);
                }
            }
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0 && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab == (uintptr)0) {
            _sqlite3AutoincrementEnd(tls, pParse);
        }
        if (memCnt != 0) {
            _sqlite3CodeChangeCount(tls, v, memCnt, __ccgo_ts + 16558);
        }
        goto delete_from_cleanup;
delete_from_cleanup:;
        _sqlite3AuthContextPop(ref (tls).DerefOrNull(), __ccgo_ptr(bp));
        _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pTabList);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhere);
        if (aToOpen != 0) {
            _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, aToOpen);
        }
        return;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3DropIndex(ж<libc.TLS> tls, uintptr pParse, uintptr pName, int32 ifExists) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 code = default;
        int32 iDb = default;
        uintptr db = default;
        uintptr pIndex = default;
        uintptr pTab = default;
        uintptr v = default;
        uintptr zDb = default;
        uintptr zTab = default;
        uintptr v1 = default;
        _ = code;
        _ = db;
        _ = iDb;
        _ = pIndex;
        _ = pTab;
        _ = v;
        _ = zDb;
        _ = zTab;
        _ = v1;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            goto exit_drop_index;
        }
        if (SQLITE_OK != _sqlite3ReadSchema(tls, pParse)) {
            goto exit_drop_index;
        }
        pIndex = _sqlite3FindIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pName + 8))).Value.FzName, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pName + 8 + 72)));
        if (pIndex == (uintptr)0) {
            if (!(ifExists != 0)) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 15984, libc.VaList(bp + 8, pName + 8));
            } else {
                _sqlite3CodeVerifyNamedSchema(tls, pParse, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pName + 8 + 72)));
                _sqlite3ForceNotReadOnly(tls, pParse);
            }
            libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
            goto exit_drop_index;
        }
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIndex + 100)) & 0x3) >> 0) != SQLITE_IDXTYPE_APPDEF) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16002, libc.VaList(bp + 8, (nint)0));
            goto exit_drop_index;
        }
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpSchema);
        code = (int32)SQLITE_DROP_INDEX;
        pTab = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable;
        zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
        if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && iDb == (int32)1) {
            v1 = __ccgo_ts + 6838;
        } else {
            v1 = __ccgo_ts + 6370;
        }
        zTab = v1;
        if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_DELETE, zTab, (uintptr)0, zDb) != 0) {
            goto exit_drop_index;
        }
        if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && iDb == (int32)1) {
            code = (int32)SQLITE_DROP_TEMP_INDEX;
        }
        if (_sqlite3AuthCheck(tls, pParse, code, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FzName, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, zDb) != 0) {
            goto exit_drop_index;
        }
        v = _sqlite3GetVdbe(tls, pParse);
        if (v != 0) {
            _sqlite3BeginWriteOperation(tls, pParse, (int32)1, iDb);
            _sqlite3NestedParse(tls, pParse, __ccgo_ts + 16075, libc.VaList(bp + 8, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FzName));
            _sqlite3ClearStatTables(tls, pParse, iDb, __ccgo_ts + 13159, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FzName);
            _sqlite3ChangeCookie(tls, pParse, iDb);
            _destroyRootPage(tls, pParse, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.Ftnum), iDb);
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_DropIndex, iDb, 0, 0, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FzName, 0);
        }
        goto exit_drop_index;
exit_drop_index:;
        _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pName);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3DropTrigger(ж<libc.TLS> tls, uintptr pParse, uintptr pName, int32 noErr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr pTrigger = default;
        uintptr zDb = default;
        uintptr zName = default;
        int32 i = default;
        int32 j = default;
        int32 v2 = default;
        _ = db;
        _ = i;
        _ = j;
        _ = pTrigger;
        _ = zDb;
        _ = zName;
        _ = v2;
        pTrigger = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            goto drop_trigger_cleanup;
        }
        if (SQLITE_OK != _sqlite3ReadSchema(tls, pParse)) {
            goto drop_trigger_cleanup;
        }
        zDb = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pName + 8 + 72));
        zName = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pName + 8))).Value.FzName;
        i = OMIT_TEMPDB;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
                break;
            }
            if (i < (int32)2) {
                v2 = i ^ (int32)1;
            } else {
                v2 = i;
            }
            j = v2;
            if (zDb != 0 && _sqlite3DbIsNamed(ref (tls).DerefOrNull(), db, j, zDb) == 0) {
                goto _1;
            }
            pTrigger = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)j * 32))).Value.FpSchema + 56), zName);
            if (pTrigger != 0) {
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (!(pTrigger != 0)) {
            if (!(noErr != 0)) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22552, libc.VaList(bp + 8, pName + 8));
            } else {
                _sqlite3CodeVerifyNamedSchema(tls, pParse, zDb);
            }
            libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
            goto drop_trigger_cleanup;
        }
        _sqlite3DropTriggerPtr(tls, pParse, pTrigger);
        goto drop_trigger_cleanup;
drop_trigger_cleanup:;
        _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pName);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3FkLocateIndex(ж<libc.TLS> tls, uintptr pParse, uintptr pParent, uintptr pFKey, @unsafe.Pointer ppIdx, @unsafe.Pointer paiCol) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr aiCol = default;
        uintptr pIdx = default;
        uintptr zDfltColl = default;
        uintptr zIdxCol = default;
        uintptr zKey = default;
        int32 i = default;
        int32 i1 = default;
        int32 j = default;
        int32 nCol = default;
        Ti16 iCol = default;
        _ = aiCol;
        _ = i;
        _ = i1;
        _ = iCol;
        _ = j;
        _ = nCol;
        _ = pIdx;
        _ = zDfltColl;
        _ = zIdxCol;
        _ = zKey;
        pIdx = (uintptr)0;
        aiCol = (uintptr)0;
        nCol = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol;
        zKey = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64))).Value.FzCol;
        if (nCol == (int32)1) {
            if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pParent)).Value.FiPKey >= 0) {
                if (!(zKey != 0)) {
                    r = 0; goto ᒐdone;
                }
                if (!(_sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pParent)).Value.FaCol + (uintptr)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pParent)).Value.FiPKey * 16))).Value.FzCnName, zKey) != 0)) {
                    r = 0; goto ᒐdone;
                }
            }
        } else {
            if (paiCol != nil) {
                aiCol = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)(libc.Uint64FromInt32(nCol) * (uint64)4));
                if (!(aiCol != 0)) {
                    r = (int32)1; goto ᒐdone;
                }
                (@unsafe.Pointer.To<uintptr>(paiCol)).Value = aiCol;
            }
        }
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pParent)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            if (libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol) == nCol && libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError) != OE_None && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere == (uintptr)0) {
                if (zKey == (uintptr)0) {
                    if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
                        if (aiCol != 0) {
                            i = 0;
                            for (;;) {
                                if (!(i < nCol)) {
                                    break;
                                }
                                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aiCol + (uintptr)i * 4))).Value = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64 + (uintptr)i * 16))).Value.FiFrom;
                                goto _2;
_2:;
                                i = i + 1;
                            }
                        }
                        break;
                    }
                } else {
                    i1 = 0;
                    for (;;) {
                        if (!(i1 < nCol)) {
                            break;
                        }
                        iCol = ~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i1 * 2));
                        if ((int32)iCol < 0) {
                            break;
                        }
                        zDfltColl = _sqlite3ColumnColl(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pParent)).Value.FaCol + (uintptr)iCol * 16);
                        if (!(zDfltColl != 0)) {
                            zDfltColl = (uintptr)Ꮡ_sqlite3StrBINARY;
                        }
                        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)i1 * 8)), zDfltColl) != 0) {
                            break;
                        }
                        zIdxCol = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pParent)).Value.FaCol + (uintptr)iCol * 16))).Value.FzCnName;
                        j = 0;
                        for (;;) {
                            if (!(j < nCol)) {
                                break;
                            }
                            if (_sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64 + (uintptr)j * 16))).Value.FzCol, zIdxCol) == 0) {
                                if (aiCol != 0) {
                                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(aiCol + (uintptr)i1 * 4))).Value = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64 + (uintptr)j * 16))).Value.FiFrom;
                                }
                                break;
                            }
                            goto _4;
_4:;
                            j = j + 1;
                        }
                        if (j == nCol) {
                            break;
                        }
                        goto _3;
_3:;
                        i1 = i1 + 1;
                    }
                    if (i1 == nCol) {
                        break;
                    }
                }
            }
            goto _1;
_1:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        }
        if (!(pIdx != 0)) {
            if (!((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x1) >> 0)) != 0)) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17413, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom)).Value.FzName, (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FzTo));
            }
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, aiCol);
            r = (int32)1; goto ᒐdone;
        }
        (@unsafe.Pointer.To<uintptr>(ppIdx)).Value = pIdx;
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3GenerateColumnNames(ж<libc.TLS> tls, uintptr pParse, @unsafe.Pointer pSelect) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        uintptr pEList = default;
        uintptr pTab = default;
        uintptr pTabList = default;
        uintptr v = default;
        uintptr z = default;
        uintptr zCol = default;
        uintptr zName = default;
        uintptr zName1 = default;
        uintptr v2 = default;
        int32 fullName = default;
        int32 i = default;
        int32 iCol = default;
        int32 srcName = default;
        _ = db;
        _ = fullName;
        _ = i;
        _ = iCol;
        _ = p;
        _ = pEList;
        _ = pTab;
        _ = pTabList;
        _ = srcName;
        _ = v;
        _ = z;
        _ = zCol;
        _ = zName;
        _ = zName1;
        _ = v2;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x20) >> 5)) != 0) {
            return;
        }
        while (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pSelect, 72)) != 0) {
            pSelect = (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pSelect, 72)));
        }
        pTabList = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pSelect, 32));
        pEList = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pSelect, 24));
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 5, 0x20);
        fullName = libc.BoolInt32((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_FullColNames) != (uint64)0);
        srcName = libc.BoolInt32((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_ShortColNames) != (uint64)0 || fullName != 0);
        _sqlite3VdbeSetNumCols(tls, v, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr);
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            p = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FpExpr;
            if ((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FzEName != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x3) >> 0) == ENAME_NAME) {
                zName = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FzEName;
                _sqlite3VdbeSetColName(tls, v, i, COLNAME_NAME, zName, (uintptr)(-libc.Int32FromInt32(1)));
            } else {
                if (srcName != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_COLUMN) {
                    iCol = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn;
                    pTab = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64));
                    if (iCol < 0) {
                        iCol = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey;
                    }
                    if (iCol < 0) {
                        zCol = __ccgo_ts + 18076;
                    } else {
                        zCol = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16))).Value.FzCnName;
                    }
                    if (fullName != 0) {
                        zName1 = (uintptr)0;
                        zName1 = _sqlite3MPrintf(tls, db, __ccgo_ts + 13747, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, zCol));
                        _sqlite3VdbeSetColName(tls, v, i, COLNAME_NAME, zName1, __ccgo_fp(_sqlite3RowSetClear));
                    } else {
                        _sqlite3VdbeSetColName(tls, v, i, COLNAME_NAME, zCol, (uintptr)(-libc.Int32FromInt32(1)));
                    }
                } else {
                    z = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FzEName;
                    if (z == (uintptr)0) {
                        v2 = _sqlite3MPrintf(tls, db, __ccgo_ts + 20769, libc.VaList(bp + 8, i + (int32)1));
                    } else {
                        v2 = _sqlite3DbStrDup(tls, db, z);
                    }
                    z = v2;
                    _sqlite3VdbeSetColName(tls, v, i, COLNAME_NAME, z, __ccgo_fp(_sqlite3RowSetClear));
                }
            }
            goto _1;
_1:;
            i = i + 1;
        }
        _generateColumnTypes(tls, pParse, pTabList, pEList);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3GenerateConstraintChecks(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, uintptr aRegIdx, int32 iDataCur, int32 iIdxCur, int32 regNewData, int32 regOldData, Tu8 pkChng, Tu8 overrideError, int32 ignoreDest, @unsafe.Pointer pbMayReplace, uintptr aiChng, uintptr pUpsert) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 addr1 = default;
        int32 addrBypass = default;
        int32 addrConflictCk = default;
        int32 addrJump = default;
        int32 addrRecheck = default;
        int32 addrRowidOk = default;
        int32 addrUniqueOk = default;
        int32 allOk = default;
        int32 b2ndPass = default;
        int32 i = default;
        int32 iField = default;
        int32 iReg = default;
        int32 iThisCur = default;
        int32 ipkBottom = default;
        int32 ipkTop = default;
        int32 isGenerated = default;
        ref var ix = ref heap(new int32(), out var Ꮡix);
        int32 jj = default;
        int32 lblRecheckOk = default;
        int32 nCol = default;
        int32 nConflictCk = default;
        int32 nGenerated = default;
        int32 nIdx = default;
        int32 nPkField = default;
        int32 nReplaceTrig = default;
        int32 nSeenReplace = default;
        int32 onError = default;
        int32 op = default;
        int32 p2 = default;
        int32 regCmp = default;
        int32 regIdx = default;
        int32 regR = default;
        int32 regRec = default;
        int32 regTrigCnt = default;
        int32 seenReplace = default;
        int32 upsertIpkDelay = default;
        int32 upsertIpkReturn = default;
        int32 x = default;
        int32 x1 = default;
        int32 v2 = default;
        Tu8 bAffinityDone = default;
        Tu8 isUpdate = default;
        uintptr bUsed = default;
        uintptr db = default;
        uintptr p4 = default;
        uintptr pCheck = default;
        uintptr pCol = default;
        uintptr pCopy = default;
        uintptr pExpr = default;
        uintptr pIdx = default;
        uintptr pPk = default;
        uintptr pTerm = default;
        uintptr pTrigger = default;
        uintptr pUpsertClause = default;
        uintptr v = default;
        uintptr zMsg = default;
        uintptr zName = default;
        uintptr zP4 = default;
        uintptr v9 = default;
        Tu64 nByte = default;
        ref var sIdxIter = ref heap(new TIndexIterator(), out var ᏑsIdxIter);
        ref var x2 = ref heap(new TVdbeOp(), out var Ꮡx2);
        _ = addr1;
        _ = addrBypass;
        _ = addrConflictCk;
        _ = addrJump;
        _ = addrRecheck;
        _ = addrRowidOk;
        _ = addrUniqueOk;
        _ = allOk;
        _ = b2ndPass;
        _ = bAffinityDone;
        _ = bUsed;
        _ = db;
        _ = i;
        _ = iField;
        _ = iReg;
        _ = iThisCur;
        _ = ipkBottom;
        _ = ipkTop;
        _ = isGenerated;
        _ = isUpdate;
        _ = ix;
        _ = jj;
        _ = lblRecheckOk;
        _ = nByte;
        _ = nCol;
        _ = nConflictCk;
        _ = nGenerated;
        _ = nIdx;
        _ = nPkField;
        _ = nReplaceTrig;
        _ = nSeenReplace;
        _ = onError;
        _ = op;
        _ = p2;
        _ = p4;
        _ = pCheck;
        _ = pCol;
        _ = pCopy;
        _ = pExpr;
        _ = pIdx;
        _ = pPk;
        _ = pTerm;
        _ = pTrigger;
        _ = pUpsertClause;
        _ = regCmp;
        _ = regIdx;
        _ = regR;
        _ = regRec;
        _ = regTrigCnt;
        _ = sIdxIter;
        _ = seenReplace;
        _ = upsertIpkDelay;
        _ = upsertIpkReturn;
        _ = v;
        _ = x;
        _ = x1;
        _ = x2;
        _ = zMsg;
        _ = zName;
        _ = zP4;
        _ = v2;
        _ = v9;
        pPk = (uintptr)0;
        seenReplace = 0;
        pUpsertClause = (uintptr)0;
        bAffinityDone = (uint8)0;
        upsertIpkReturn = 0;
        upsertIpkDelay = 0;
        ipkTop = 0;
        ipkBottom = 0;
        addrRecheck = 0;
        lblRecheckOk = 0;
        nReplaceTrig = 0;
        isUpdate = libc.BoolUint8(regOldData != 0);
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        nCol = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
            pPk = (uintptr)0;
            nPkField = (int32)1;
        } else {
            pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
            nPkField = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol);
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasNotNull) != 0) {
            b2ndPass = 0;
            nSeenReplace = 0;
            nGenerated = 0;
            while ((int32)1 != 0) {
                i = 0;
                for (;;) {
                    if (!(i < nCol)) {
                        break;
                    }
                    pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16;
                    onError = (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pCol + 8)) & 0xf) >> 0);
                    if (onError == OE_None) {
                        goto _1;
                    }
                    if (i == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                        goto _1;
                    }
                    isGenerated = libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_GENERATED;
                    if (isGenerated != 0 && !(b2ndPass != 0)) {
                        nGenerated = nGenerated + 1;
                        goto _1;
                    }
                    if (aiChng != 0 && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiChng + (uintptr)i * 4)) < 0 && !(isGenerated != 0)) {
                        goto _1;
                    }
                    if (libc.Int32FromUint8(overrideError) != (int32)OE_Default) {
                        onError = libc.Int32FromUint8(overrideError);
                    } else {
                        if (onError == (int32)OE_Default) {
                            onError = (int32)OE_Abort;
                        }
                    }
                    if (onError == (int32)OE_Replace) {
                        if (b2ndPass != 0 || libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FiDflt) == 0) {
                            onError = (int32)OE_Abort;
                        } else {
                        }
                    } else {
                        if (b2ndPass != 0 && !(isGenerated != 0)) {
                            goto _1;
                        }
                    }
                    iReg = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), pTab, (int16)i) + regNewData + (int32)1;
                    var matchᴛ1 = false;
                    if (onError == (int32)OE_Replace) { matchᴛ1 = true;
                        addr1 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_NotNull, iReg);
                        nSeenReplace = nSeenReplace + 1;
                        _sqlite3ExprCodeCopy(tls, pParse, _sqlite3ColumnExprAuth(tls, pTab, pCol, pParse), iReg);
                        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
                    }
                    else if (onError == (int32)OE_Abort) { matchᴛ1 = true;
                        _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ1 && onError == (int32)OE_Rollback) { matchᴛ1 = true;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ1 && onError == (int32)OE_Fail) {
                        zMsg = _sqlite3MPrintf(tls, db, __ccgo_ts + 13747, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName));
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_HaltIfNull, libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(5).Lsh((int64)(libc.Int32FromInt32(8))), onError, iReg);
                        _sqlite3VdbeAppendP4(tls, v, zMsg, -(int32)7);
                        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)P5_ConstraintNotNull);
                    }
                    else if (!matchᴛ1) { /* default: */
                        do {
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, iReg, ignoreDest);
                            break;
                        } while (false);
                    }

                    goto _1;
_1:;
                    i = i + 1;
                }
                if (nGenerated == 0 && nSeenReplace == 0) {
                    break;
                }
                if (b2ndPass != 0) {
                    break;
                }
                b2ndPass = (int32)1;
                if (nSeenReplace > 0 && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != (uint32)0) {
                    _sqlite3ComputeGeneratedColumns(tls, pParse, regNewData + (int32)1, pTab);
                }
            }
        }
        if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpCheck != 0 && (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_IgnoreChecks) == (uint64)0) {
            pCheck = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpCheck;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = -(regNewData + (int32)1);
            if (libc.Int32FromUint8(overrideError) != (int32)OE_Default) {
                v2 = libc.Int32FromUint8(overrideError);
            } else {
                v2 = (int32)OE_Abort;
            }
            onError = v2;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pCheck)).Value.FnExpr)) {
                    break;
                }
                pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pCheck + 8 + (uintptr)i * 32))).Value.FpExpr;
                if (aiChng != 0 && !(_sqlite3ExprReferencesUpdatedColumn(tls, pExpr, aiChng, libc.Int32FromUint8(pkChng)) != 0)) {
                    goto _3;
                }
                if (libc.Int32FromUint8(bAffinityDone) == 0) {
                    _sqlite3TableAffinity(tls, v, pTab, regNewData + (int32)1);
                    bAffinityDone = (uint8)1;
                }
                allOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                pCopy = _sqlite3ExprDup(tls, db, pExpr, 0);
                if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                    _sqlite3ExprIfTrue(tls, pParse, pCopy, allOk, (int32)SQLITE_JUMPIFNULL);
                }
                _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pCopy);
                if (onError == (int32)OE_Ignore) {
                    _sqlite3VdbeGoto(tls, v, ignoreDest);
                } else {
                    zName = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pCheck + 8 + (uintptr)i * 32))).Value.FzEName;
                    if (onError == (int32)OE_Replace) {
                        onError = (int32)OE_Abort;
                    }
                    _sqlite3HaltConstraint(tls, pParse, libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8))), onError, zName, P4_TRANSIENT, (uint8)P5_ConstraintCheck);
                }
                _sqlite3VdbeResolveLabel(tls, v, allOk);
                goto _3;
_3:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = 0;
        }
        sIdxIter.FeType = 0;
        sIdxIter.Fi = 0;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8))).Value = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 0))).Value = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        if (pUpsert != 0) {
            if ((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTarget == (uintptr)0) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FisDoUpdate) == 0) {
                    overrideError = (uint8)OE_Ignore;
                    pUpsert = (uintptr)0;
                } else {
                    overrideError = (uint8)OE_Update;
                }
            } else {
                if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex != (uintptr)0) {
                    nIdx = 0;
                    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
                    for (;;) {
                        if (!(pIdx != 0)) {
                            break;
                        }
                        goto _4;
_4:;
                        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                        nIdx = nIdx + 1;
                    }
                    sIdxIter.FeType = (int32)1;
                    (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 0))).Value = nIdx;
                    nByte = (uint64)((libc.Uint64FromInt64(16) + libc.Uint64FromInt32(1)) * libc.Uint64FromInt32(nIdx) + libc.Uint64FromInt32(nIdx));
                    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8))).Value = _sqlite3DbMallocZero(tls, db, nByte);
                    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8)) == (uintptr)0) {
                        return;
                    }
                    bUsed = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8)) + (uintptr)nIdx * 16;
                    (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpToFree = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8));
                    i = 0;
                    pTerm = pUpsert;
                    for (;;) {
                        if (!(pTerm != 0)) {
                            break;
                        }
                        if ((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pTerm)).Value.FpUpsertTarget == (uintptr)0) {
                            break;
                        }
                        if ((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pTerm)).Value.FpUpsertIdx == (uintptr)0) {
                            goto _5;
                        }
                        jj = 0;
                        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
                        while (pIdx != (uintptr)0 && pIdx != (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pTerm)).Value.FpUpsertIdx) {
                            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                            jj = jj + 1;
                        }
                        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(bUsed + (uintptr)jj)) != 0) {
                            goto _5;
                        }
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(bUsed + (uintptr)jj))).Value = (uint8)1;
                        (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8)) + (uintptr)i * 16))).Value.Fp = pIdx;
                        (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8)) + (uintptr)i * 16))).Value.Fix = jj;
                        i = i + 1;
                        goto _5;
_5:;
                        pTerm = (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pTerm)).Value.FpNextUpsert;
                    }
                    jj = 0;
                    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
                    for (;;) {
                        if (!(pIdx != 0)) {
                            break;
                        }
                        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(bUsed + (uintptr)jj)) != 0) {
                            goto _6;
                        }
                        (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8)) + (uintptr)i * 16))).Value.Fp = pIdx;
                        (@unsafe.Pointer.To<TIndexListTerm>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(ᏑsIdxIter.of(TIndexIterator.ᏑFu)), 8)) + (uintptr)i * 16))).Value.Fix = jj;
                        i = i + 1;
                        goto _6;
_6:;
                        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                        jj = jj + 1;
                    }
                }
            }
        }
        if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & libc.Uint64FromInt32(libc.Int32FromInt32(SQLITE_RecTriggers) | libc.Int32FromInt32(SQLITE_ForeignKeys))) == (uint64)0) {
            pTrigger = (uintptr)0;
            regTrigCnt = 0;
        } else {
            if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_RecTriggers) != 0) {
                pTrigger = _sqlite3TriggersExist(tls, pParse, pTab, (int32)TK_DELETE, (uintptr)0, nil);
                regTrigCnt = libc.BoolInt32(pTrigger != (uintptr)0 || _sqlite3FkRequired(tls, pParse, pTab, (uintptr)0, 0) != 0);
            } else {
                pTrigger = (uintptr)0;
                regTrigCnt = _sqlite3FkRequired(tls, pParse, pTab, (uintptr)0, 0);
            }
            if (regTrigCnt != 0) {
                if (libc.Int32FromUint8(overrideError) != (int32)OE_Replace) {
                    if (libc.Int32FromUint8(overrideError) != (int32)OE_Default) {
                        regTrigCnt = 0;
                    } else {
                        if (libc.Int32FromUint8(pkChng) == 0 || pPk != 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FkeyConf) != (int32)OE_Replace) {
                            pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
                            for (;;) {
                                if (!(pIdx != 0)) {
                                    break;
                                }
                                if (libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError) == (int32)OE_Replace) {
                                    break;
                                }
                                goto _7;
_7:;
                                pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                            }
                            if (pIdx == (uintptr)0) {
                                regTrigCnt = 0;
                            }
                        }
                    }
                }
            }
            if (regTrigCnt != 0) {
                v9 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v9)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v9) + 1;
                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v9);
                regTrigCnt = v2;
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regTrigCnt);
                lblRecheckOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                addrRecheck = lblRecheckOk;
            }
        }
        if (pkChng != 0 && pPk == (uintptr)0) {
            addrRowidOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            onError = libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FkeyConf);
            if (libc.Int32FromUint8(overrideError) != (int32)OE_Default) {
                onError = libc.Int32FromUint8(overrideError);
            } else {
                if (onError == (int32)OE_Default) {
                    onError = (int32)OE_Abort;
                }
            }
            if (pUpsert != 0) {
                pUpsertClause = _sqlite3UpsertOfIndex(ref (tls).DerefOrNull(), pUpsert, (uintptr)0);
                if (pUpsertClause != (uintptr)0) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsertClause)).Value.FisDoUpdate) == 0) {
                        onError = (int32)OE_Ignore;
                    } else {
                        onError = (int32)OE_Update;
                    }
                }
                if (pUpsertClause != pUpsert) {
                    upsertIpkDelay = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
                }
            }
            if (onError == (int32)OE_Replace && onError != libc.Int32FromUint8(overrideError) && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex != 0) {
                if (upsertIpkDelay != 0) {
                    ipkTop = upsertIpkDelay + (int32)1;
                    upsertIpkDelay = 0;
                } else {
                    ipkTop = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto) + (int32)1;
                }
            }
            if (isUpdate != 0) {
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Eq, regNewData, addrRowidOk, regOldData);
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NOTNULL);
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDataCur, addrRowidOk, regNewData);
            var onErrorᴛ1 = onError;
            var matchᴛ2 = false;
            var matchᴛ3 = onErrorᴛ1 == (int32)OE_Rollback || onErrorᴛ1 == (int32)OE_Abort || onErrorᴛ1 == (int32)OE_Fail || onErrorᴛ1 == (int32)OE_Replace || onErrorᴛ1 == (int32)OE_Update || onErrorᴛ1 == (int32)OE_Ignore;
            if (!matchᴛ3) { /* default: */
                onError = (int32)OE_Abort;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && onErrorᴛ1 == (int32)OE_Rollback) { matchᴛ2 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && onErrorᴛ1 == (int32)OE_Abort) { matchᴛ2 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && onErrorᴛ1 == (int32)OE_Fail) { matchᴛ2 = true;
                _sqlite3RowidConstraint(tls, pParse, onError, pTab);
            }
            else if (onErrorᴛ1 == (int32)OE_Replace) { matchᴛ2 = true;
                if (regTrigCnt != 0) {
                    _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
                    _sqlite3GenerateRowDelete(tls, pParse, pTab, pTrigger, iDataCur, iIdxCur, regNewData, (int16)1, (uint8)0, (uint8)OE_Replace, (uint8)1, -(int32)1);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_AddImm, regTrigCnt, (int32)1);
                    nReplaceTrig = nReplaceTrig + 1;
                } else {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Delete, iDataCur, (int32)OPFLAG_ISNOOP);
                    _sqlite3VdbeAppendP4(tls, v, pTab, -(int32)5);
                    if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex != 0) {
                        _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
                        _sqlite3GenerateRowIndexDelete(tls, pParse, pTab, iDataCur, iIdxCur, (uintptr)0, -(int32)1);
                    }
                }
                seenReplace = (int32)1;
            }
            else if (onErrorᴛ1 == (int32)OE_Update) {
                _sqlite3UpsertDoUpdate(tls, pParse, pUpsert, pTab, (uintptr)0, iDataCur);
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && onErrorᴛ1 == (int32)OE_Ignore) { matchᴛ2 = true;
                do {
                    _sqlite3VdbeGoto(tls, v, ignoreDest);
                    break;
                } while (false);
            }

            _sqlite3VdbeResolveLabel(tls, v, addrRowidOk);
            if (ipkTop != 0) {
                ipkBottom = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, ipkTop - (int32)1);
            } else {
                if (pUpsert != 0 && pUpsertClause != pUpsert) {
                    upsertIpkReturn = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
                }
            }
        }
        pIdx = _indexIteratorFirst(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(ᏑsIdxIter), @unsafe.Pointer.FromPinnedBox(Ꮡix));
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)ix * 4)) == 0) {
                goto _10;
            }
            if (pUpsert != 0) {
                pUpsertClause = _sqlite3UpsertOfIndex(ref (tls).DerefOrNull(), pUpsert, pIdx);
                if (upsertIpkDelay != 0 && pUpsertClause == pUpsert) {
                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, upsertIpkDelay);
                }
            }
            addrUniqueOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            if (libc.Int32FromUint8(bAffinityDone) == 0) {
                _sqlite3TableAffinity(tls, v, pTab, regNewData + (int32)1);
                bAffinityDone = (uint8)1;
            }
            iThisCur = iIdxCur + ix;
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)ix * 4)));
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = -(regNewData + (int32)1);
                _sqlite3ExprIfFalseDup(tls, pParse, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere, addrUniqueOk, (int32)SQLITE_JUMPIFNULL);
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = 0;
            }
            regIdx = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)ix * 4)) + (int32)1;
            i = 0;
            for (;;) {
                if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn))) {
                    break;
                }
                iField = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)));
                if (iField == -(int32)2) {
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = -(regNewData + (int32)1);
                    _sqlite3ExprCodeCopy(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr + 8 + (uintptr)i * 32))).Value.FpExpr, regIdx + i);
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = 0;
                } else {
                    if (iField == -(int32)1 || iField == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                        x = regNewData;
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_IntCopy, x, regIdx + i);
                    } else {
                        x = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), pTab, (int16)iField) + regNewData + (int32)1;
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_SCopy, x, regIdx + i);
                    }
                }
                goto _11;
_11:;
                i = i + 1;
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regIdx, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn), ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)ix * 4)));
            if (isUpdate != 0 && pPk == pIdx && libc.Int32FromUint8(pkChng) == 0) {
                _sqlite3VdbeResolveLabel(tls, v, addrUniqueOk);
                goto _10;
            }
            onError = libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError);
            if (onError == OE_None) {
                _sqlite3VdbeResolveLabel(tls, v, addrUniqueOk);
                goto _10;
            }
            if (libc.Int32FromUint8(overrideError) != (int32)OE_Default) {
                onError = libc.Int32FromUint8(overrideError);
            } else {
                if (onError == (int32)OE_Default) {
                    onError = (int32)OE_Abort;
                }
            }
            if (pUpsertClause != 0) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsertClause)).Value.FisDoUpdate) == 0) {
                    onError = (int32)OE_Ignore;
                } else {
                    onError = (int32)OE_Update;
                }
            }
            addrConflictCk = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NoConflict, iThisCur, addrUniqueOk, regIdx, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol));
            if (pIdx == pPk) {
                v2 = regIdx;
            } else {
                v2 = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nPkField);
            }
            regR = v2;
            if (isUpdate != 0 || onError == (int32)OE_Replace) {
                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IdxRowid, iThisCur, regR);
                    if (isUpdate != 0) {
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Eq, regR, addrUniqueOk, regOldData);
                        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NOTNULL);
                    }
                } else {
                    if (pIdx != pPk) {
                        i = 0;
                        for (;;) {
                            if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol))) {
                                break;
                            }
                            x1 = _sqlite3TableColumnToIndex(ref (tls).DerefOrNull(), pIdx, (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2))));
                            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iThisCur, x1, regR + i);
                            goto _13;
_13:;
                            i = i + 1;
                        }
                    }
                    if (isUpdate != 0) {
                        addrJump = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol);
                        op = (int32)OP_Ne;
                        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
                            v2 = regIdx;
                        } else {
                            v2 = regR;
                        }
                        regCmp = v2;
                        i = 0;
                        for (;;) {
                            if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol))) {
                                break;
                            }
                            p4 = _sqlite3LocateCollSeq(tls, pParse, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FazColl + (uintptr)i * 8)));
                            x1 = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2)));
                            if (i == libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol) - (int32)1) {
                                addrJump = addrUniqueOk;
                                op = (int32)OP_Eq;
                            }
                            x1 = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), pTab, (int16)x1);
                            _sqlite3VdbeAddOp4(tls, v, op, regOldData + (int32)1 + x1, addrJump, regCmp + i, p4, -(int32)2);
                            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NOTNULL);
                            goto _15;
_15:;
                            i = i + 1;
                        }
                    }
                }
            }
            nConflictCk = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) - addrConflictCk;
            var matchᴛ4 = false;
            if (onError == (int32)OE_Rollback) { matchᴛ4 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ4 && onError == (int32)OE_Abort) { matchᴛ4 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ4 && onError == (int32)OE_Fail) { matchᴛ4 = true;
                _sqlite3UniqueConstraint(tls, pParse, onError, pIdx);
            }
            else if (onError == (int32)OE_Update) { matchᴛ4 = true;
                _sqlite3UpsertDoUpdate(tls, pParse, pUpsert, pTab, pIdx, iIdxCur + ix);
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ4 && onError == (int32)OE_Ignore) {
                _sqlite3VdbeGoto(tls, v, ignoreDest);
            }
            else if (!matchᴛ4) { /* default: */
                do {
                    if (regTrigCnt != 0) {
                        _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
                        nReplaceTrig = nReplaceTrig + 1;
                    }
                    if (pTrigger != 0 && isUpdate != 0) {
                        _sqlite3VdbeAddOp1(tls, v, (int32)OP_CursorLock, iDataCur);
                    }
                    if (pIdx == pPk) {
                        v2 = (int32)ONEPASS_SINGLE;
                    } else {
                        v2 = ONEPASS_OFF;
                    }
                    _sqlite3GenerateRowDelete(tls, pParse, pTab, pTrigger, iDataCur, iIdxCur, regR, (int16)nPkField, (uint8)0, (uint8)OE_Replace, libc.Uint8FromInt32(v2), iThisCur);
                    if (pTrigger != 0 && isUpdate != 0) {
                        _sqlite3VdbeAddOp1(tls, v, (int32)OP_CursorUnlock, iDataCur);
                    }
                    seenReplace = (int32)1;
                    break;
                } while (false);
            }

            if (regTrigCnt != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_AddImm, regTrigCnt, (int32)1);
                addrBypass = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
                _sqlite3VdbeResolveLabel(tls, v, lblRecheckOk);
                lblRecheckOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != 0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, regIdx - (int32)1, lblRecheckOk);
                }
                while (nConflictCk > 0) {
                    x2 = ~@unsafe.Pointer.To<TVdbeOp>(__ccgo_ptr(_sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, addrConflictCk)));
                    if (libc.Int32FromUint8(x2.Fopcode) != (int32)OP_IdxRowid || isUpdate != 0) {
                        if ((libc.Int32FromUint8(_sqlite3OpcodeProperty[x2.Fopcode]) & (int32)OPFLG_JUMP) != 0) {
                            p2 = lblRecheckOk;
                        } else {
                            p2 = x2.Fp2;
                        }
                        if ((int32)x2.Fp4type == -(int32)3) {
                            v9 = (uintptr)(int64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡx2.of(TVdbeOp.ᏑFp4)), 0)));
                        } else {
                            v9 = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡx2.of(TVdbeOp.ᏑFp4)), 0));
                        }
                        zP4 = v9;
                        _sqlite3VdbeAddOp4(tls, v, libc.Int32FromUint8(x2.Fopcode), x2.Fp1, p2, x2.Fp3, zP4, (int32)x2.Fp4type);
                        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, x2.Fp5);
                    }
                    nConflictCk = nConflictCk - 1;
                    addrConflictCk = addrConflictCk + 1;
                }
                _sqlite3UniqueConstraint(tls, pParse, (int32)OE_Abort, pIdx);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrBypass);
            }
            _sqlite3VdbeResolveLabel(tls, v, addrUniqueOk);
            if (regR != regIdx) {
                _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, regR, nPkField);
            }
            if (pUpsertClause != 0 && upsertIpkReturn != 0 && _sqlite3UpsertNextIsIPK(ref (tls).DerefOrNull(), pUpsertClause) != 0) {
                _sqlite3VdbeGoto(tls, v, upsertIpkDelay + (int32)1);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, upsertIpkReturn);
                upsertIpkReturn = 0;
            }
            goto _10;
_10:;
            pIdx = _indexIteratorNext(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(ᏑsIdxIter), @unsafe.Pointer.FromPinnedBox(Ꮡix));
        }
        if (ipkTop != 0) {
            _sqlite3VdbeGoto(tls, v, ipkTop);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, ipkBottom);
        }
        if (nReplaceTrig != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfNot, regTrigCnt, lblRecheckOk);
            if (!(pPk != 0)) {
                if (isUpdate != 0) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Eq, regNewData, addrRecheck, regOldData);
                    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NOTNULL);
                }
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDataCur, addrRecheck, regNewData);
                _sqlite3RowidConstraint(tls, pParse, (int32)OE_Abort, pTab);
            } else {
                _sqlite3VdbeGoto(tls, v, addrRecheck);
            }
        }
        if (regTrigCnt != 0) {
            _sqlite3VdbeResolveLabel(tls, v, lblRecheckOk);
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
            regRec = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)ix * 4));
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regNewData + (int32)1, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnNVCol, regRec);
            if (!(bAffinityDone != 0)) {
                _sqlite3TableAffinity(tls, v, pTab, 0);
            }
        }
        (@unsafe.Pointer.To<int32>(pbMayReplace)).Value = seenReplace;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3HasExplicitNulls(ж<libc.TLS> tls, uintptr pParse, uintptr pList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        Tu8 sf = default;
        uintptr v2 = default;
        _ = i;
        _ = sf;
        _ = v2;
        if (pList != 0) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                    break;
                }
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x20) >> 5) != 0) {
                    sf = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.Ffg.FsortFlags;
                    if (libc.Int32FromUint8(sf) == 0 || libc.Int32FromUint8(sf) == (int32)3) {
                        v2 = __ccgo_ts + 15487;
                    } else {
                        v2 = __ccgo_ts + 15493;
                    }
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 15498, libc.VaList(bp + 8, v2));
                    r = (int32)1; goto ᒐdone;
                }
                goto _1;
_1:;
                i = i + 1;
            }
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3IndexedByLookup(ж<libc.TLS> tls, uintptr pParse, uintptr pFrom) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr pIdx = default;
        uintptr pTab = default;
        uintptr zIndexedBy = default;
        _ = pIdx;
        _ = pTab;
        _ = zIndexedBy;
        pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab;
        zIndexedBy = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 48));
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0 && _sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzName, zIndexedBy) != 0)) {
                break;
            }
            goto _1;
_1:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        }
        if (!(pIdx != 0)) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21151, libc.VaList(bp + 8, zIndexedBy, (nint)0));
            libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
            r = (int32)SQLITE_ERROR; goto ᒐdone;
        }
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 56))).Value = pIdx;
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3Insert(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pSelect, uintptr pColumn, int32 onError, uintptr pUpsert) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        uintptr aRegIdx = default;
        uintptr aTabColMap = default;
        uintptr db = default;
        uintptr pIdx = default;
        uintptr pIpk = default;
        uintptr pItem = default;
        uintptr pList = default;
        uintptr pNx = default;
        uintptr pSubq = default;
        uintptr pTab = default;
        uintptr pTrigger = default;
        uintptr pVTab = default;
        uintptr pX = default;
        uintptr v = default;
        uintptr v5 = default;
        int32 addr1 = default;
        int32 addr11 = default;
        int32 addrCont = default;
        int32 addrInsTop = default;
        int32 addrL = default;
        int32 addrTop = default;
        int32 bUseSeek = default;
        int32 endOfLoop = default;
        int32 i = default;
        ref var iDataCur = ref heap(new int32(), out var ᏑiDataCur);
        int32 iDb = default;
        ref var iIdxCur = ref heap(new int32(), out var ᏑiIdxCur);
        int32 iRegStore = default;
        int32 ipkColumn = default;
        ref var isReplace = ref heap(new int32(), out var ᏑisReplace);
        int32 isView = default;
        int32 j = default;
        int32 k = default;
        int32 nColumn = default;
        int32 nHidden = default;
        int32 nIdx = default;
        int32 rc = default;
        int32 regAutoinc = default;
        int32 regCols = default;
        int32 regData = default;
        int32 regFromSelect = default;
        int32 regIns = default;
        int32 regRec = default;
        int32 regRowCount = default;
        int32 regRowid = default;
        int32 regTempRowid = default;
        int32 regYield = default;
        int32 srcTab = default;
        ref var tmask = ref heap(new int32(), out var Ꮡtmask);
        int32 y = default;
        int32 v1 = default;
        Tu8 appendFlag = default;
        Tu8 bIdListInOrder = default;
        Tu8 useTempTable = default;
        Tu8 withoutRowid = default;
        Tu32 colFlags = default;
        Tu32 v20 = default;
        ref var dest = ref heap(new TSelectDest(), out var Ꮡdest);
        TNameContext _ᴛ1 = new();
        _ = aRegIdx;
        _ = aTabColMap;
        _ = addr1;
        _ = addr11;
        _ = addrCont;
        _ = addrInsTop;
        _ = addrL;
        _ = addrTop;
        _ = appendFlag;
        _ = bIdListInOrder;
        _ = bUseSeek;
        _ = colFlags;
        _ = db;
        _ = dest;
        _ = endOfLoop;
        _ = i;
        _ = iDataCur;
        _ = iDb;
        _ = iIdxCur;
        _ = iRegStore;
        _ = ipkColumn;
        _ = isReplace;
        _ = isView;
        _ = j;
        _ = k;
        _ = nColumn;
        _ = nHidden;
        _ = nIdx;
        _ = pIdx;
        _ = pIpk;
        _ = pItem;
        _ = pList;
        _ = pNx;
        _ = pSubq;
        _ = pTab;
        _ = pTrigger;
        _ = pVTab;
        _ = pX;
        _ = rc;
        _ = regAutoinc;
        _ = regCols;
        _ = regData;
        _ = regFromSelect;
        _ = regIns;
        _ = regRec;
        _ = regRowCount;
        _ = regRowid;
        _ = regTempRowid;
        _ = regYield;
        _ = srcTab;
        _ = tmask;
        _ = useTempTable;
        _ = v;
        _ = withoutRowid;
        _ = y;
        _ = v1;
        _ = v20;
        _ = v5;
        nHidden = 0;
        iDataCur = 0;
        iIdxCur = 0;
        ipkColumn = -(int32)1;
        srcTab = 0;
        addrInsTop = 0;
        addrCont = 0;
        useTempTable = (uint8)0;
        appendFlag = (uint8)0;
        pList = (uintptr)0;
        regFromSelect = 0;
        regAutoinc = 0;
        regRowCount = 0;
        aRegIdx = (uintptr)0;
        aTabColMap = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto insert_cleanup;
        }
        dest.FiSDParm = 0;
        if (pSelect != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FselFlags & (uint32)SF_Values) != (uint32)0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior == (uintptr)0) {
            pList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList = (uintptr)0;
            _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSelect);
            pSelect = (uintptr)0;
        }
        pTab = _sqlite3SrcListLookup(tls, pParse, pTabList);
        if (pTab == (uintptr)0) {
            goto insert_cleanup;
        }
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
        if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_INSERT, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (uintptr)0, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName) != 0) {
            goto insert_cleanup;
        }
        withoutRowid = libc.BoolUint8(!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)));
        pTrigger = _sqlite3TriggersExist(tls, pParse, pTab, (int32)TK_INSERT, (uintptr)0, @unsafe.Pointer.FromPinnedBox(Ꮡtmask));
        isView = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW);
        if (_sqlite3ViewGetColumnNames(tls, pParse, pTab) != 0) {
            goto insert_cleanup;
        }
        if (_sqlite3IsReadOnly(tls, pParse, pTab, pTrigger) != 0) {
            goto insert_cleanup;
        }
        v = _sqlite3GetVdbe(tls, pParse);
        if (v == (uintptr)0) {
            goto insert_cleanup;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0) {
            _sqlite3VdbeCountChanges(ref (tls).DerefOrNull(), v);
        }
        _sqlite3BeginWriteOperation(tls, pParse, libc.BoolInt32(pSelect != 0 || pTrigger != 0), iDb);
        if (pColumn == (uintptr)0 && pSelect != (uintptr)0 && pTrigger == (uintptr)0 && _xferOptimization(tls, pParse, pTab, pSelect, onError, iDb) != 0) {
            goto insert_end;
        }
        regAutoinc = _autoIncBegin(tls, pParse, iDb, pTab);
        v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + libc.Int32FromInt32(1);
        regIns = v1;
        regRowid = v1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + (int32)1;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
            regRowid = regRowid + 1;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + 1;
        }
        regData = regRowid + (int32)1;
        bIdListInOrder = libc.BoolUint8((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(libc.Int32FromInt32(TF_OOOHidden) | libc.Int32FromInt32(TF_HasStored))) == (uint32)0);
        if (pColumn != 0) {
            aTabColMap = _sqlite3DbMallocZero(tls, db, (uint64)(libc.Uint64FromInt16((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol) * (uint64)4));
            if (aTabColMap == (uintptr)0) {
                goto insert_cleanup;
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pColumn)).Value.FnId)) {
                    break;
                }
                j = _sqlite3ColumnIndex(ref (tls).DerefOrNull(), pTab, (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pColumn + 8 + (uintptr)i * 8))).Value.FzName);
                if (j >= 0) {
                    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(aTabColMap + (uintptr)j * 4)) == 0) {
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(aTabColMap + (uintptr)j * 4))).Value = i + (int32)1;
                    }
                    if (i != j) {
                        bIdListInOrder = (uint8)0;
                    }
                    if (j == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                        ipkColumn = i;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (libc.Int32FromInt32(COLFLAG_STORED) | libc.Int32FromInt32(COLFLAG_VIRTUAL))) != 0) {
                        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17458, libc.VaList(bp + 64, (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FzCnName));
                        goto insert_cleanup;
                    }
                } else {
                    if (_sqlite3IsRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pColumn + 8 + (uintptr)i * 8))).Value.FzName) != 0 && !(withoutRowid != 0)) {
                        ipkColumn = i;
                        bIdListInOrder = (uint8)0;
                    } else {
                        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17499, libc.VaList(bp + 64, pTabList + 8, (@unsafe.Pointer.To<TIdList_item>((@unsafe.Pointer)(pColumn + 8 + (uintptr)i * 8))).Value.FzName));
                        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
                        goto insert_cleanup;
                    }
                }
                goto _2;
_2:;
                i = i + 1;
            }
        }
        if (pSelect != 0) {
            if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc)).Value.FnSrc == (int32)1 && (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8 + 24 + 4)) & 0x40) >> 6) != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior == (uintptr)0) {
                pItem = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8;
                pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72));
                dest.FiSDParm = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn;
                regFromSelect = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregResult;
                nColumn = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FpSelect)).Value.FpEList)).Value.FnExpr;
                _sqlite3VdbeExplain(tls, pParse, (uint8)0, __ccgo_ts + 17531, libc.VaList(bp + 64, pItem));
                if (bIdListInOrder != 0 && nColumn == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol) {
                    regData = regFromSelect;
                    regRowid = regData - (int32)1;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                        v1 = (int32)1;
                    } else {
                        v1 = 0;
                    }
                    regIns = regRowid - v1;
                }
            } else {
                v5 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5);
                regYield = v1;
                addrTop = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)1;
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_InitCoroutine, regYield, 0, addrTop);
                _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡdest), (int32)SRT_Coroutine, regYield);
                if (bIdListInOrder != 0) {
                    v1 = regData;
                } else {
                    v1 = 0;
                }
                dest.FiSdst = v1;
                dest.FnSdst = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
                rc = _sqlite3Select(tls, pParse, pSelect, @unsafe.Pointer.FromPinnedBox(Ꮡdest));
                regFromSelect = dest.FiSdst;
                if (rc != 0 || (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                    goto insert_cleanup;
                }
                _sqlite3VdbeEndCoroutine(tls, v, regYield);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrTop - (int32)1);
                nColumn = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList)).Value.FnExpr;
            }
            if (pTrigger != 0 || _readsTable(tls, pParse, iDb, pTab) != 0) {
                useTempTable = (uint8)1;
            }
            if (useTempTable != 0) {
                v5 = pParse + 56;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5) + 1;
                srcTab = v1;
                regRec = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                regTempRowid = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, srcTab, nColumn);
                addrL = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Yield, dest.FiSDParm);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regFromSelect, nColumn, regRec);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, srcTab, regTempRowid);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, srcTab, regRec, regTempRowid);
                _sqlite3VdbeGoto(tls, v, addrL);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrL);
                _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regRec);
                _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regTempRowid);
            }
        } else {
            libc.Xmemset(tls, bp, 0, (uint64)56);
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
            srcTab = -(int32)1;
            if (pList != 0) {
                nColumn = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr;
                if (_sqlite3ResolveExprListNames(tls, bp, pList) != 0) {
                    goto insert_cleanup;
                }
            } else {
                nColumn = 0;
            }
        }
        if (pColumn == (uintptr)0 && nColumn > 0) {
            ipkColumn = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey;
            if (ipkColumn >= 0 && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != (uint32)0) {
                i = ipkColumn - (int32)1;
                for (;;) {
                    if (!(i >= 0)) {
                        break;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_GENERATED) != 0) {
                        ipkColumn = ipkColumn - 1;
                    }
                    goto _9;
_9:;
                    i = i - 1;
                }
            }
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(libc.Int32FromInt32(TF_HasGenerated) | libc.Int32FromInt32(TF_HasHidden))) != (uint32)0) {
                i = 0;
                for (;;) {
                    if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                        break;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_NOINSERT) != 0) {
                        nHidden = nHidden + 1;
                    }
                    goto _10;
_10:;
                    i = i + 1;
                }
            }
            if (nColumn != (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol - nHidden) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17539, libc.VaList(bp + 64, pTabList + 8, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol - nHidden, nColumn));
                goto insert_cleanup;
            }
        }
        if (pColumn != (uintptr)0 && nColumn != (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pColumn)).Value.FnId) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17591, libc.VaList(bp + 64, nColumn, (@unsafe.Pointer.To<TIdList>((@unsafe.Pointer)pColumn)).Value.FnId));
            goto insert_cleanup;
        }
        if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00001)).Lsh((int64)(libc.Int32FromInt32(32))))) != (uint64)0 && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab != 0) && !((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x8) >> 3)) != 0)) {
            v5 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5);
            regRowCount = v1;
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regRowCount);
        }
        if (!(isView != 0)) {
            nIdx = _sqlite3OpenTableAndIndices(tls, pParse, pTab, (int32)OP_OpenWrite, (uint8)0, -(int32)1, (uintptr)0, @unsafe.Pointer.FromPinnedBox(ᏑiDataCur), @unsafe.Pointer.FromPinnedBox(ᏑiIdxCur));
            aRegIdx = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)4 * libc.Uint64FromInt32(nIdx + libc.Int32FromInt32(2))));
            if (aRegIdx == (uintptr)0) {
                goto insert_cleanup;
            }
            i = 0;
            pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
            for (;;) {
                if (!(i < nIdx)) {
                    break;
                }
                v5 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4))).Value = v1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
                goto _13;
_13:;
                pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                i = i + 1;
            }
            v5 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v5);
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4))).Value = v1;
        }
        if (pUpsert != 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17616, libc.VaList(bp + 64, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
                goto insert_cleanup;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 17662, 0);
                goto insert_cleanup;
            }
            if (_sqlite3HasExplicitNulls(tls, pParse, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTarget) != 0) {
                goto insert_cleanup;
            }
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.FiCursor = iDataCur;
            pNx = pUpsert;
            for (var cond = true; cond; cond = pNx != (uintptr)0) {
                (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNx)).Value.FpUpsertSrc = pTabList;
                (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNx)).Value.FregData = regData;
                (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNx)).Value.FiDataCur = iDataCur;
                (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNx)).Value.FiIdxCur = iIdxCur;
                if ((@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNx)).Value.FpUpsertTarget != 0) {
                    if (_sqlite3UpsertAnalyzeTarget(tls, pParse, pTabList, pNx, pUpsert) != 0) {
                        goto insert_cleanup;
                    }
                }
                pNx = (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pNx)).Value.FpNextUpsert;
            }
        }
        if (useTempTable != 0) {
            addrInsTop = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, srcTab);
            addrCont = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        } else {
            if (pSelect != 0) {
                v1 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Yield, dest.FiSDParm);
                addrCont = v1;
                addrInsTop = v1;
                if (ipkColumn >= 0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Copy, regFromSelect + ipkColumn, regRowid);
                }
            }
        }
        nHidden = 0;
        iRegStore = regData;
        i = 0;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            if (i == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_SoftNull, iRegStore);
                goto _19;
            }
            v20 = (uint32)(@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags;
            colFlags = v20;
            if ((Tu32)(v20 & (uint32)COLFLAG_NOINSERT) != (uint32)0) {
                nHidden = nHidden + 1;
                if ((Tu32)(colFlags & (uint32)COLFLAG_VIRTUAL) != (uint32)0) {
                    iRegStore = iRegStore - 1;
                    goto _19;
                } else {
                    if ((Tu32)(colFlags & (uint32)COLFLAG_STORED) != (uint32)0) {
                        if ((tmask & (int32)TRIGGER_BEFORE) != 0) {
                            _sqlite3VdbeAddOp1(tls, v, (int32)OP_SoftNull, iRegStore);
                        }
                        goto _19;
                    } else {
                        if (pColumn == (uintptr)0) {
                            _sqlite3ExprCodeFactorable(tls, pParse, _sqlite3ColumnExprAuth(tls, pTab, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16, pParse), iRegStore);
                            goto _19;
                        }
                    }
                }
            }
            if (pColumn != 0) {
                j = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aTabColMap + (uintptr)i * 4));
                if (j == 0) {
                    _sqlite3ExprCodeFactorable(tls, pParse, _sqlite3ColumnExprAuth(tls, pTab, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16, pParse), iRegStore);
                    goto _19;
                }
                k = j - (int32)1;
            } else {
                if (nColumn == 0) {
                    _sqlite3ExprCodeFactorable(tls, pParse, _sqlite3ColumnExprAuth(tls, pTab, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16, pParse), iRegStore);
                    goto _19;
                } else {
                    k = i - nHidden;
                }
            }
            if (useTempTable != 0) {
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, srcTab, k, iRegStore);
            } else {
                if (pSelect != 0) {
                    if (regFromSelect != regData) {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_SCopy, regFromSelect + k, iRegStore);
                    }
                } else {
                    pX = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)k * 32))).Value.FpExpr;
                    y = _sqlite3ExprCodeTarget(tls, pParse, pX, iRegStore);
                    if (y != iRegStore) {
                        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0) {
                            v1 = (int32)OP_Copy;
                        } else {
                            v1 = (int32)OP_SCopy;
                        }
                        _sqlite3VdbeAddOp2(tls, v, v1, y, iRegStore);
                    }
                }
            }
            goto _19;
_19:;
            i = i + 1;
            iRegStore = iRegStore + 1;
        }
        endOfLoop = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        if ((tmask & (int32)TRIGGER_BEFORE) != 0) {
            regCols = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + (int32)1);
            if (ipkColumn < 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, -(int32)1, regCols);
            } else {
                if (useTempTable != 0) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, srcTab, ipkColumn, regCols);
                } else {
                    _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ipkColumn * 32))).Value.FpExpr, regCols);
                }
                addr1 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_NotNull, regCols);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, -(int32)1, regCols);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_MustBeInt, regCols);
                _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regRowid + (int32)1, regCols + (int32)1, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnNVCol - (int32)1);
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != 0) {
                _sqlite3ComputeGeneratedColumns(tls, pParse, regCols + (int32)1, pTab);
            }
            if (!(isView != 0)) {
                _sqlite3TableAffinity(tls, v, pTab, regCols + (int32)1);
            }
            _sqlite3CodeRowTrigger(tls, pParse, pTrigger, (int32)TK_INSERT, (uintptr)0, (int32)TRIGGER_BEFORE, pTab, regCols - (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol - (int32)1, onError, endOfLoop);
            _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, regCols, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + (int32)1);
        }
        if (!(isView != 0)) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regIns);
            }
            if (ipkColumn >= 0) {
                if (useTempTable != 0) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, srcTab, ipkColumn, regRowid);
                } else {
                    if (pSelect != 0) {
                    } else {
                        pIpk = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ipkColumn * 32))).Value.FpExpr;
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pIpk)).Value.Fop) == (int32)TK_NULL && !(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB))) {
                            _sqlite3VdbeAddOp3(tls, v, (int32)OP_NewRowid, iDataCur, regRowid, regAutoinc);
                            appendFlag = (uint8)1;
                        } else {
                            _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ipkColumn * 32))).Value.FpExpr, regRowid);
                        }
                    }
                }
                if (!(appendFlag != 0)) {
                    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB))) {
                        addr11 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_NotNull, regRowid);
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_NewRowid, iDataCur, regRowid, regAutoinc);
                        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr11);
                    } else {
                        addr11 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, regRowid, addr11 + (int32)2);
                    }
                    _sqlite3VdbeAddOp1(tls, v, (int32)OP_MustBeInt, regRowid);
                    _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
                }
            } else {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB || withoutRowid != 0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regRowid);
                } else {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_NewRowid, iDataCur, regRowid, regAutoinc);
                    appendFlag = (uint8)1;
                }
            }
            _autoIncStep(tls, pParse, regAutoinc, regRowid);
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != 0) {
                _sqlite3ComputeGeneratedColumns(tls, pParse, regRowid + (int32)1, pTab);
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                pVTab = _sqlite3GetVTable(ref (tls).DerefOrNull(), db, pTab);
                _sqlite3VtabMakeWritable(tls, pParse, pTab);
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_VUpdate, (int32)1, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + (int32)2, regIns, pVTab, -(int32)12);
                if (onError == (int32)OE_Default) {
                    v1 = (int32)OE_Abort;
                } else {
                    v1 = onError;
                }
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32(v1));
                _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
            } else {
                isReplace = 0;
                _sqlite3GenerateConstraintChecks(tls, pParse, pTab, aRegIdx, iDataCur, iIdxCur, regIns, 0, libc.BoolUint8(ipkColumn >= 0), libc.Uint8FromInt32(onError), endOfLoop, @unsafe.Pointer.FromPinnedBox(ᏑisReplace), (uintptr)0, pUpsert);
                if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_ForeignKeys) != 0) {
                    _sqlite3FkCheck(tls, pParse, pTab, 0, regIns, (uintptr)0, 0);
                }
                bUseSeek = libc.BoolInt32(isReplace == 0 || !(_sqlite3VdbeHasSubProgram(ref (tls).DerefOrNull(), v) != 0));
                _sqlite3CompleteInsertion(tls, pParse, pTab, iDataCur, iIdxCur, regIns, aRegIdx, 0, libc.Int32FromUint8(appendFlag), bUseSeek);
            }
        }
        if (regRowCount != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_AddImm, regRowCount, (int32)1);
        }
        if (pTrigger != 0) {
            _sqlite3CodeRowTrigger(tls, pParse, pTrigger, (int32)TK_INSERT, (uintptr)0, (int32)TRIGGER_AFTER, pTab, regData - (int32)2 - (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol, onError, endOfLoop);
        }
        _sqlite3VdbeResolveLabel(tls, v, endOfLoop);
        if (useTempTable != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, srcTab, addrCont);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrInsTop);
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, srcTab);
        } else {
            if (pSelect != 0) {
                _sqlite3VdbeGoto(tls, v, addrCont);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrInsTop);
            }
        }
        goto insert_end;
insert_end:;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0 && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab == (uintptr)0) {
            _sqlite3AutoincrementEnd(tls, pParse);
        }
        if (regRowCount != 0) {
            _sqlite3CodeChangeCount(tls, v, regRowCount, __ccgo_ts + 17683);
        }
        goto insert_cleanup;
insert_cleanup:;
        _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pTabList);
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pList);
        _sqlite3UpsertDelete(tls, (@unsafe.Pointer)db, pUpsert);
        _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSelect);
        if (pColumn != 0) {
            _sqlite3IdListDelete(tls, (@unsafe.Pointer)db, pColumn);
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aTabColMap);
        }
        if (aRegIdx != 0) {
            _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, aRegIdx);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _sqlite3LocateTable(ж<libc.TLS> tls, uintptr pParse, Tu32 flags, uintptr zName, uintptr zDbase) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        uintptr pMod = default;
        uintptr zMsg = default;
        uintptr v1 = default;
        _ = db;
        _ = p;
        _ = pMod;
        _ = zMsg;
        _ = v1;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_SchemaKnownOk) == (uint32)0 && SQLITE_OK != _sqlite3ReadSchema(tls, pParse)) {
            r = (uintptr)0; goto ᒐdone;
        }
        p = _sqlite3FindTable(tls, db, zName, zDbase);
        if (p == (uintptr)0) {
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FprepFlags) & (int32)SQLITE_PREPARE_NO_VTAB) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy) == 0) {
                pMod = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)(db + 584), zName);
                if (pMod == (uintptr)0 && Xsqlite3_strnicmp(tls, zName, __ccgo_ts + 13801, (int32)7) == 0) {
                    pMod = _sqlite3PragmaVtabRegister(tls, db, zName);
                }
                if (pMod == (uintptr)0 && Xsqlite3_strnicmp(tls, zName, __ccgo_ts + 13809, (int32)4) == 0) {
                    pMod = _sqlite3JsonVtabRegister(tls, db, zName);
                }
                if (pMod != 0 && _sqlite3VtabEponymousTableInit(tls, pParse, pMod) != 0) {
                    r = (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpEpoTab; goto ᒐdone;
                }
            }
            if ((Tu32)(flags & (uint32)LOCATE_NOERR) != 0) {
                r = (uintptr)0; goto ᒐdone;
            }
            libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)p)).Value.FeTabType) == (int32)TABTYP_VTAB && (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FprepFlags) & (int32)SQLITE_PREPARE_NO_VTAB) != 0) {
                p = (uintptr)0;
            }
        }
        if (p == (uintptr)0) {
            if ((Tu32)(flags & (uint32)LOCATE_VIEW) != 0) {
                v1 = __ccgo_ts + 13814;
            } else {
                v1 = __ccgo_ts + 13827;
            }
            zMsg = v1;
            if (zDbase != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7094, libc.VaList(bp + 8, zMsg, zDbase, zName));
            } else {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7165, libc.VaList(bp + 8, zMsg, zName));
            }
        } else {
        }
        r = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3Reindex(ж<libc.TLS> tls, uintptr pParse, uintptr pName1, uintptr pName2) {
    int32 bAll = default;
    int32 bMatch = default;
    int32 iDb = default;
    int32 iReDb = default;
    int32 isExprIdx = default;
    uintptr db = default;
    uintptr k = default;
    uintptr pDb = default;
    uintptr pIdx = default;
    ref var pObjName = ref heap(new uintptr(), out var ᏑpObjName);
    uintptr pReIndex = default;
    uintptr pReTab = default;
    uintptr pTab = default;
    uintptr z = default;
    uintptr zColl = default;
    uintptr zDb = default;
    uintptr v1 = default;
    bool v2 = default;
    _ = bAll;
    _ = bMatch;
    _ = db;
    _ = iDb;
    _ = iReDb;
    _ = isExprIdx;
    _ = k;
    _ = pDb;
    _ = pIdx;
    _ = pObjName;
    _ = pReIndex;
    _ = pReTab;
    _ = pTab;
    _ = z;
    _ = zColl;
    _ = zDb;
    _ = v1;
    _ = v2;
    z = (uintptr)0;
    zDb = (uintptr)0;
    iReDb = -(int32)1;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    bMatch = 0;
    zColl = (uintptr)0;
    pReTab = (uintptr)0;
    pReIndex = (uintptr)0;
    isExprIdx = 0;
    bAll = 0;
    if (SQLITE_OK != _sqlite3ReadSchema(tls, pParse)) {
        return;
    }
    if (pName1 == (uintptr)0) {
        bMatch = (int32)1;
        bAll = (int32)1;
    } else {
        if (pName2 == (uintptr)0 || (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pName2)).Value.Fz == (uintptr)0) {
            z = _sqlite3NameFromToken(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer)pName1);
            if (z == (uintptr)0) {
                return;
            }
        } else {
            iReDb = _sqlite3TwoPartName(tls, pParse, pName1, pName2, @unsafe.Pointer.FromBox(ᏑpObjName));
            if (iReDb < 0) {
                return;
            }
            z = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pObjName);
            if (z == (uintptr)0) {
                return;
            }
            zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iReDb * 32))).Value.FzDbSName;
        }
    }
    if (!(bAll != 0)) {
        if (zDb == (uintptr)0 && _sqlite3StrICmp(ref (tls).DerefOrNull(), z, __ccgo_ts + 16339) == 0) {
            isExprIdx = (int32)1;
            bMatch = (int32)1;
        }
        if (zDb == (uintptr)0 && _sqlite3FindCollSeq(tls, db, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc, z, 0) != (uintptr)0) {
            zColl = z;
            bMatch = (int32)1;
        }
        v2 = zColl == (uintptr)0; if (v2) {
            v1 = _sqlite3FindTable(tls, db, z, zDb);
            pReTab = v1;
        }
        if (v2 && v1 != (uintptr)0) {
            bMatch = (int32)1;
        }
        v2 = zColl == (uintptr)0; if (v2) {
            v1 = _sqlite3FindIndex(ref (tls).DerefOrNull(), db, z, zDb);
            pReIndex = v1;
        }
        if (v2 && v1 != (uintptr)0) {
            bMatch = (int32)1;
        }
    }
    if (bMatch != 0) {
        iDb = 0;
        pDb = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb;
        for (;;) {
            if (!(iDb < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
                break;
            }
            if (iReDb >= 0 && iReDb != iDb) {
                goto _5;
            }
            k = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)((@unsafe.Pointer.To<TDb>((@unsafe.Pointer)pDb)).Value.FpSchema + 8))).Value.Ffirst;
            for (;;) {
                if (!(k != 0)) {
                    break;
                }
                pTab = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)k)).Value.Fdata;
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                    goto _6;
                }
                pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
                for (;;) {
                    if (!(pIdx != 0)) {
                        break;
                    }
                    if (bAll != 0 || pTab == pReTab || pIdx == pReIndex || isExprIdx != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x800) >> 11) != 0 || zColl != (uintptr)0 && _collationMatch(ref (tls).DerefOrNull(), zColl, pIdx) != 0) {
                        _sqlite3BeginWriteOperation(tls, pParse, 0, iDb);
                        _sqlite3RefillIndex(tls, pParse, pIdx, -(int32)1);
                    }
                    goto _7;
_7:;
                    pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                }
                goto _6;
_6:;
                k = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)k)).Value.Fnext;
            }
            goto _5;
_5:;
            iDb = iDb + 1;
            pDb += 32;
        }
    } else {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16351, 0);
    }
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, z);
    return;
}

internal static int32 /*r*/ _sqlite3Select(ж<libc.TLS> tls, uintptr pParse, uintptr p, @unsafe.Pointer pDest) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        int32 addr1 = default;
        int32 addrEnd = default;
        int32 addrGosub = default;
        int32 addrOutputRow = default;
        int32 addrReset = default;
        int32 addrSetAbort = default;
        int32 addrSortingIdx = default;
        int32 addrTop = default;
        int32 addrTopOfLoop = default;
        int32 eDist = default;
        int32 eDist1 = default;
        int32 groupBySort = default;
        int32 i = default;
        int32 iAMem = default;
        int32 iAbortFlag = default;
        int32 iBMem = default;
        int32 iBreak = default;
        int32 iCont = default;
        int32 iCsr = default;
        int32 iDb = default;
        int32 iDb1 = default;
        int32 iEnd = default;
        int32 iOrderByCol = default;
        int32 iUseFlag = default;
        int32 ii = default;
        int32 isAgg = default;
        int32 j = default;
        int32 k = default;
        int32 nCol = default;
        int32 nGroupBy = default;
        int32 onceAddr = default;
        int32 orderByGrp = default;
        int32 rc = default;
        int32 regAcc = default;
        int32 regBase = default;
        int32 regGosub = default;
        int32 regOutputRow = default;
        int32 regRecord = default;
        int32 regReset = default;
        int32 sortOut = default;
        int32 sortPTab = default;
        int32 topAddr = default;
        int32 v12 = default;
        int32 v15 = default;
        uintptr db = default;
        uintptr p0 = default;
        uintptr pAggInfo = default;
        uintptr pBase = default;
        uintptr pBest = default;
        uintptr pCol = default;
        uintptr pCteUse = default;
        uintptr pCteUse1 = default;
        uintptr pDistinct = default;
        uintptr pDistinct1 = default;
        uintptr pEList = default;
        uintptr pExpr = default;
        uintptr pF = default;
        uintptr pF1 = default;
        uintptr pGroupBy = default;
        uintptr pHaving = default;
        uintptr pI2 = default;
        uintptr pIdx = default;
        uintptr pItem = default;
        uintptr pItem1 = default;
        uintptr pItem2 = default;
        uintptr pKeyInfo = default;
        uintptr pKeyInfo1 = default;
        uintptr pKeyInfo2 = default;
        ref var pMinMaxOrderBy = ref heap(new uintptr(), out var ᏑpMinMaxOrderBy);
        uintptr pPrior = default;
        uintptr pPriorSubq = default;
        uintptr pSub = default;
        uintptr pSub1 = default;
        uintptr pSubq = default;
        uintptr pTab = default;
        uintptr pTab1 = default;
        uintptr pTabList = default;
        uintptr pWInfo = default;
        uintptr pWhere = default;
        uintptr pWin = default;
        uintptr pX = default;
        uintptr v = default;
        uintptr zDb = default;
        uintptr zSavedAuthContext = default;
        uintptr v1 = default;
        uintptr v3 = default;
        ref var dest = ref heap(new TSelectDest(), out var Ꮡdest);
        Tu16 distFlag = default;
        Tu16 distFlag1 = default;
        Tu16 wctrlFlags = default;
        TPgno iRoot = default;
        Tu8 minMaxFlag = default;
        ref var sDistinct = ref heap(new TDistinctCtx(), out var ᏑsDistinct);
        TNameContext _ᴛ1 = new();
        TSortCtx _ᴛ2 = default;
        _ = addr1;
        _ = addrEnd;
        _ = addrGosub;
        _ = addrOutputRow;
        _ = addrReset;
        _ = addrSetAbort;
        _ = addrSortingIdx;
        _ = addrTop;
        _ = addrTopOfLoop;
        _ = db;
        _ = dest;
        _ = distFlag;
        _ = distFlag1;
        _ = eDist;
        _ = eDist1;
        _ = groupBySort;
        _ = i;
        _ = iAMem;
        _ = iAbortFlag;
        _ = iBMem;
        _ = iBreak;
        _ = iCont;
        _ = iCsr;
        _ = iDb;
        _ = iDb1;
        _ = iEnd;
        _ = iOrderByCol;
        _ = iRoot;
        _ = iUseFlag;
        _ = ii;
        _ = isAgg;
        _ = j;
        _ = k;
        _ = minMaxFlag;
        _ = nCol;
        _ = nGroupBy;
        _ = onceAddr;
        _ = orderByGrp;
        _ = p0;
        _ = pAggInfo;
        _ = pBase;
        _ = pBest;
        _ = pCol;
        _ = pCteUse;
        _ = pCteUse1;
        _ = pDistinct;
        _ = pDistinct1;
        _ = pEList;
        _ = pExpr;
        _ = pF;
        _ = pF1;
        _ = pGroupBy;
        _ = pHaving;
        _ = pI2;
        _ = pIdx;
        _ = pItem;
        _ = pItem1;
        _ = pItem2;
        _ = pKeyInfo;
        _ = pKeyInfo1;
        _ = pKeyInfo2;
        _ = pMinMaxOrderBy;
        _ = pPrior;
        _ = pPriorSubq;
        _ = pSub;
        _ = pSub1;
        _ = pSubq;
        _ = pTab;
        _ = pTab1;
        _ = pTabList;
        _ = pWInfo;
        _ = pWhere;
        _ = pWin;
        _ = pX;
        _ = rc;
        _ = regAcc;
        _ = regBase;
        _ = regGosub;
        _ = regOutputRow;
        _ = regRecord;
        _ = regReset;
        _ = sDistinct;
        _ = sortOut;
        _ = sortPTab;
        _ = topAddr;
        _ = v;
        _ = wctrlFlags;
        _ = zDb;
        _ = zSavedAuthContext;
        _ = v1;
        _ = v12;
        _ = v15;
        _ = v3;
        pEList = (uintptr)0;
        pAggInfo = (uintptr)0;
        rc = (int32)1;
        pMinMaxOrderBy = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        v = _sqlite3GetVdbe(tls, pParse);
        if (p == (uintptr)0 || (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            r = (int32)1; goto ᒐdone;
        }
        if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_SELECT, (uintptr)0, (uintptr)0, (uintptr)0) != 0) {
            r = (int32)1; goto ᒐdone;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pDest)) <= (int32)SRT_DistQueue) {
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != 0) {
                _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3ExprListDeleteGeneric), (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy);
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = (uintptr)0;
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(SF_Distinct);
        }
        _sqlite3SelectPrep(tls, pParse, p, (uintptr)0);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto select_end;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_UFSrcCheck) != 0) {
            p0 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8;
            if (_sameSrcAlias(tls, p0, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc) != 0) {
                if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p0)).Value.FzAlias != 0) {
                    v1 = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p0)).Value.FzAlias;
                } else {
                    v1 = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p0)).Value.FpSTab)).Value.FzName;
                }
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21765, libc.VaList(bp + 112, v1));
                goto select_end;
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(SF_UFSrcCheck);
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pDest)) == (int32)SRT_Output) {
            _sqlite3GenerateColumnNames(tls, pParse, (@unsafe.Pointer)p);
        }
        if (_sqlite3WindowRewrite(tls, pParse, p) != 0) {
            goto select_end;
        }
        pTabList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        isAgg = libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Aggregate) != (uint32)0);
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy;
        i = 0;
        for (;;) {
            if (!(!((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior != 0) && i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
                break;
            }
            pItem = pTabList + 8 + (uintptr)i * 80;
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
                v1 = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect;
            } else {
                v1 = (uintptr)0;
            }
            pSub = v1;
            pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab;
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ))) != 0 && _sqlite3ExprImpliesNonNullRow(tls, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor, libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_SimplifyJoin))) == (uint32)0) {
                if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (int32)JT_LEFT) != 0) {
                    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0) {
                        v1 = pItem + 24;
                        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(JT_LEFT));
                    } else {
                        v1 = pItem + 24;
                        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_OUTER)));
                        _unsetJoinExpr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor, 0);
                    }
                }
                if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
                    j = i + (int32)1;
                    for (;;) {
                        if (!(j < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
                            break;
                        }
                        pI2 = pTabList + 8 + (uintptr)j * 80;
                        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pI2)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0) {
                            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pI2)).Value.Ffg.Fjointype) & (int32)JT_LEFT) != 0) {
                                v1 = pI2 + 24;
                                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(JT_RIGHT));
                            } else {
                                v1 = pI2 + 24;
                                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(JT_RIGHT) | libc.Int32FromInt32(JT_OUTER)));
                                _unsetJoinExpr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pI2)).Value.FiCursor, (int32)1);
                            }
                        }
                        goto _6;
_6:;
                        j = j + 1;
                    }
                    j = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc - (int32)1;
                    for (;;) {
                        if (!(j >= 0)) {
                            break;
                        }
                        v1 = pTabList + 8 + (uintptr)j * 80 + 24;
                        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(JT_LTORJ));
                        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8 + (uintptr)j * 80))).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0) {
                            break;
                        }
                        goto _9;
_9:;
                        j = j - 1;
                    }
                }
            }
            if (pSub == (uintptr)0) {
                goto _2;
            }
            if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol != (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpEList)).Value.FnExpr) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21819, libc.VaList(bp + 112, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpEList)).Value.FnExpr));
                goto select_end;
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x200) >> 9) != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 56))))).Value.FeM10d) == M10d_Yes) {
                goto _2;
            }
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & (uint32)SF_Aggregate) != (uint32)0) {
                goto _2;
            }
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != (uintptr)0 && ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != (uintptr)0 || (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc > (int32)1) && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpLimit == (uintptr)0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_OrderByReqd) | libc.Int32FromInt32(SF_Recursive))) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_OrderByReqd) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OmitOrderBy))) == (uint32)0) {
                _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3ExprListDeleteGeneric), (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy);
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy = (uintptr)0;
            }
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != (uintptr)0 && i == 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_ComplexResult) != (uint32)0 && ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc == (int32)1 || (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8 + 1 * 80))).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_CROSS))) != 0)) {
                goto _2;
            }
            if (_flattenSubquery(tls, pParse, p, i, isAgg) != 0) {
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                    goto select_end;
                }
                i = -(int32)1;
            }
            pTabList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                goto select_end;
            }
            if (!(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pDest)) <= libc.Int32FromInt32(SRT_Fifo))) {
                (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior != 0) {
            rc = _multiSelect(tls, pParse, p, pDest);
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpNext == (uintptr)0) {
                _sqlite3VdbeExplainPop(ref (tls).DerefOrNull(), pParse);
            }
            r = rc; goto ᒐdone;
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x10) >> 4)) != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_ExistsToJoin))) == (uint32)0) {
            _existsToJoin(tls, pParse, p, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere);
            pTabList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere)).Value.Fop) == (int32)TK_AND && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_PropagateConst))) == (uint32)0 && _propagateConstants(tls, pParse, p) != 0) {
        } else {
        }
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_QueryFlattener) | libc.Int32FromInt32(SQLITE_CountOfView))) == (uint32)0 && _countOfViewOptimization(tls, pParse, p) != 0) {
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                goto select_end;
            }
            pTabList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
                break;
            }
            pItem1 = pTabList + 8 + (uintptr)i * 80;
            if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FcolUsed == (uint64)0 && (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FzName != (uintptr)0) {
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x10000) >> 16) != 0) {
                    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 72)));
                    zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
                } else {
                    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x4) >> 2) != 0) {
                        zDb = (uintptr)0;
                    } else {
                        zDb = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 72));
                    }
                }
                _sqlite3AuthCheck(tls, pParse, (int32)SQLITE_READ, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FzName, __ccgo_ts + 1732, zDb);
            }
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x4) >> 2) == 0) {
                goto _11;
            }
            pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 72));
            pSub1 = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FpSelect;
            if ((@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub != 0) {
                goto _11;
            }
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 336))).Value += _sqlite3SelectExprHeight(ref (tls).DerefOrNull(), p);
            if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_PushDown))) == (uint32)0 && ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x200) >> 9) == 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 56))))).Value.FeM10d) != M10d_Yes && (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 56))))).Value.FnUse < (int32)2) && _pushDownWhereTerms(tls, pParse, pSub1, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere, pTabList, i) != 0) {
            } else {
            }
            if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_NullUnusedCols))) == (uint32)0 && _disableUnusedSubqueryResultColumns(ref (tls).DerefOrNull(), pItem1) != 0) {
            }
            zSavedAuthContext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FzName;
            if (_fromClauseTermCanBeCoroutine(tls, pParse, pTabList, i, libc.Int32FromUint32((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags)) != 0) {
                addrTop = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)1;
                v1 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn = v12;
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_InitCoroutine, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn, 0, addrTop);
                (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub = addrTop;
                _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡdest), (int32)SRT_Coroutine, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn);
                _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 21859, libc.VaList(bp + 112, pItem1));
                _sqlite3Select(tls, pParse, pSub1, @unsafe.Pointer.FromPinnedBox(Ꮡdest));
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FpSTab)).Value.FnRowLogEst = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FnSelectRow;
                libc.SetBitFieldPtr32Uint32(pItem1 + 24 + 4, libc.Uint32FromInt32(1), 6, 0x40);
                (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregResult = dest.FiSdst;
                _sqlite3VdbeEndCoroutine(tls, v, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrTop - (int32)1);
                _sqlite3ClearTempRegCache(ref (tls).DerefOrNull(), pParse);
            } else {
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x200) >> 9) != 0 && (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 56))))).Value.FaddrM9e > 0) {
                    pCteUse = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 56));
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FregRtn, (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FaddrM9e);
                    if ((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FiCursor != (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FiCur) {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FiCursor, (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FiCur);
                    }
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FnSelectRow = (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FnRowEst;
                } else {
                    v1 = _isSelfJoinView(tls, pTabList, pItem1, 0, i);
                    pPrior = v1;
                    if (v1 != (uintptr)0) {
                        pPriorSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pPrior + 72));
                        if ((@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pPriorSubq)).Value.FaddrFillSub != 0) {
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pPriorSubq)).Value.FregReturn, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pPriorSubq)).Value.FaddrFillSub);
                        }
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FiCursor, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pPrior)).Value.FiCursor);
                        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FnSelectRow = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pPriorSubq)).Value.FpSelect)).Value.FnSelectRow;
                    } else {
                        onceAddr = 0;
                        v1 = pParse + 60;
                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                        v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                        (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn = v12;
                        topAddr = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
                        (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub = topAddr + (int32)1;
                        libc.SetBitFieldPtr32Uint32(pItem1 + 24 + 4, libc.Uint32FromInt32(1), 5, 0x20);
                        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x10) >> 4) == 0) {
                            onceAddr = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Once);
                        } else {
                        }
                        _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡdest), (int32)SRT_EphemTab, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FiCursor);
                        _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 21874, libc.VaList(bp + 112, pItem1));
                        _sqlite3Select(tls, pParse, pSub1, @unsafe.Pointer.FromPinnedBox(Ꮡdest));
                        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FpSTab)).Value.FnRowLogEst = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FnSelectRow;
                        if (onceAddr != 0) {
                            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, onceAddr);
                        }
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Return, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn, topAddr + (int32)1);
                        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, topAddr);
                        _sqlite3ClearTempRegCache(ref (tls).DerefOrNull(), pParse);
                        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x200) >> 9) != 0 && (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem1 + 24 + 4)) & 0x10) >> 4) == 0) {
                            pCteUse1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem1 + 56));
                            (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse1)).Value.FaddrM9e = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub;
                            (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse1)).Value.FregRtn = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn;
                            (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse1)).Value.FiCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem1)).Value.FiCursor;
                            (@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse1)).Value.FnRowEst = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FnSelectRow;
                        }
                    }
                }
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                goto select_end;
            }
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 336))).Value -= _sqlite3SelectExprHeight(ref (tls).DerefOrNull(), p);
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext = zSavedAuthContext;
            goto _11;
_11:;
            i = i + 1;
        }
        pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList;
        pWhere = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere;
        pGroupBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy;
        pHaving = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving;
        sDistinct.FisTnct = libc.BoolUint8((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Distinct) != (uint32)0);
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_Distinct) | libc.Int32FromInt32(SF_Aggregate))) == (uint32)SF_Distinct && _sqlite3CopySortOrder(ref (tls).DerefOrNull(), pEList, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy) != 0 && _sqlite3ExprListCompare(tls, pEList, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy, -(int32)1) == 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_GroupByOrder))) == (uint32)0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin == (uintptr)0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(SF_Distinct);
            v1 = _sqlite3ExprListDup(tls, db, pEList, 0);
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy = v1;
            pGroupBy = v1;
            if (pGroupBy != 0) {
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr)) {
                        break;
                    }
                    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)(pGroupBy + 8 + (uintptr)i * 32 + 24))).Value = libc.Uint16FromInt32(i + (int32)1);
                    goto _18;
_18:;
                    i = i + 1;
                }
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_Aggregate;
            sDistinct.FisTnct = (uint8)2;
        }
        if ((@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy != 0) {
            pKeyInfo = _sqlite3KeyInfoFromExprList(tls, pParse, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy, 0, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr);
            v1 = pParse + 56;
            v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
            (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FiECursor = v12;
            (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex = _sqlite3VdbeAddOp4(tls, v, (int32)OP_OpenEphemeral, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FiECursor, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy)).Value.FnExpr + (int32)1 + (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr, 0, pKeyInfo, -(int32)9);
        } else {
            (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex = -(int32)1;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pDest)) == (int32)SRT_EphemTab) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pDest, 4)), (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr);
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_NestedFrom) != 0) {
                ii = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr - (int32)1;
                for (;;) {
                    if (!(ii > 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pEList + 8 + (uintptr)ii * 32 + 16 + 4)) & 0x40) >> 6) == 0)) {
                        break;
                    }
                    _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)ii * 32))).Value.FpExpr);
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)ii * 32))).Value.FzEName);
                    (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr - 1;
                    goto _21;
_21:;
                    ii = ii - 1;
                }
                ii = 0;
                for (;;) {
                    if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                        break;
                    }
                    if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pEList + 8 + (uintptr)ii * 32 + 16 + 4)) & 0x40) >> 6) == 0) {
                        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)ii * 32))).Value.FpExpr)).Value.Fop = (uint8)TK_NULL;
                    }
                    goto _22;
_22:;
                    ii = ii + 1;
                }
            }
        }
        iEnd = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_FixedLimit) == (uint32)0) {
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow = (int16)320;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit != 0) {
            _computeLimitRegisters(tls, pParse, p, iEnd);
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FiLimit == 0 && (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex >= 0) {
            _sqlite3VdbeChangeOpcode(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex, (uint8)OP_SorterOpen);
            v1 = bp + 36;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(SORTFLAG_UseSorter));
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Distinct) != 0) {
            v1 = pParse + 56;
            v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
            sDistinct.FtabTnct = v12;
            sDistinct.FaddrTnct = _sqlite3VdbeAddOp4(tls, v, (int32)OP_OpenEphemeral, sDistinct.FtabTnct, 0, 0, _sqlite3KeyInfoFromExprList(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList, 0, 0), -(int32)9);
            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)BTREE_UNORDERED);
            sDistinct.FeTnctType = (uint8)WHERE_DISTINCT_UNORDERED;
        } else {
            sDistinct.FeTnctType = (uint8)WHERE_DISTINCT_NOOP;
        }
        if (!(isAgg != 0) && pGroupBy == (uintptr)0) {
            if (sDistinct.FisTnct != 0) {
                v12 = (int32)WHERE_WANT_DISTINCT;
            } else {
                v12 = 0;
            }
            wctrlFlags = (uint16)(libc.Uint32FromInt32(v12) | (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_FixedLimit));
            pWin = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin;
            if (pWin != 0) {
                _sqlite3WindowCodeInit(tls, pParse, p);
            }
            pWInfo = _sqlite3WhereBegin(tls, pParse, pTabList, pWhere, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList, p, wctrlFlags, (int32)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow);
            if (pWInfo == (uintptr)0) {
                goto select_end;
            }
            if ((int32)_sqlite3WhereOutputRowCount(ref (tls).DerefOrNull(), pWInfo) < (int32)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow) {
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow = _sqlite3WhereOutputRowCount(ref (tls).DerefOrNull(), pWInfo);
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pDest)) <= (int32)SRT_DistQueue && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(pDest)) >= (int32)SRT_DistFifo) {
                    v1 = p + 2;
                    (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v1)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v1)) - libc.Int32FromInt32(30));
                }
            }
            if (sDistinct.FisTnct != 0 && _sqlite3WhereIsDistinct(ref (tls).DerefOrNull(), pWInfo) != 0) {
                sDistinct.FeTnctType = libc.Uint8FromInt32(_sqlite3WhereIsDistinct(ref (tls).DerefOrNull(), pWInfo));
            }
            if ((@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy != 0) {
                (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FnOBSat = _sqlite3WhereIsOrdered(ref (tls).DerefOrNull(), pWInfo);
                (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FlabelOBLopt = _sqlite3WhereOrderByLimitOptLabel(ref (tls).DerefOrNull(), pWInfo);
                if ((@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FnOBSat == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy)).Value.FnExpr) {
                    (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy = (uintptr)0;
                }
            }
            if ((@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex >= 0 && (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy == (uintptr)0) {
                _sqlite3VdbeChangeToNoop(tls, v, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex);
            }
            if (pWin != 0) {
                addrGosub = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                iCont = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                iBreak = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                v1 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                regGosub = v12;
                _sqlite3WindowCodeStep(tls, pParse, p, pWInfo, regGosub, addrGosub);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, iBreak);
                _sqlite3VdbeResolveLabel(tls, v, addrGosub);
                (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FlabelOBLopt = 0;
                _selectInnerLoop(tls, pParse, p, -(int32)1, __ccgo_ptr(bp), @unsafe.Pointer.FromPinnedBox(ᏑsDistinct), pDest, iCont, iBreak);
                _sqlite3VdbeResolveLabel(tls, v, iCont);
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_Return, regGosub);
                _sqlite3VdbeResolveLabel(tls, v, iBreak);
            } else {
                _selectInnerLoop(tls, pParse, p, -(int32)1, __ccgo_ptr(bp), @unsafe.Pointer.FromPinnedBox(ᏑsDistinct), pDest, _sqlite3WhereContinueLabel(ref (tls).DerefOrNull(), pWInfo), _sqlite3WhereBreakLabel(ref (tls).DerefOrNull(), pWInfo));
                _sqlite3WhereEnd(tls, pWInfo);
            }
        } else {
            sortPTab = 0;
            sortOut = 0;
            orderByGrp = 0;
            if (pGroupBy != 0) {
                k = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr;
                pItem2 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList + 8;
                for (;;) {
                    if (!(k > 0)) {
                        break;
                    }
                    (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem2 + 24))).Value.FiAlias = (uint16)0;
                    goto _30;
_30:;
                    k = k - 1;
                    pItem2 += 32;
                }
                k = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr;
                pItem2 = pGroupBy + 8;
                for (;;) {
                    if (!(k > 0)) {
                        break;
                    }
                    (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem2 + 24))).Value.FiAlias = (uint16)0;
                    goto _31;
_31:;
                    k = k - 1;
                    pItem2 += 32;
                }
                if ((int32)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow > (int32)66) {
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow = (int16)66;
                }
                if (_sqlite3CopySortOrder(ref (tls).DerefOrNull(), pGroupBy, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy) != 0 && _sqlite3ExprListCompare(tls, pGroupBy, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy, -(int32)1) == 0) {
                    orderByGrp = (int32)1;
                }
            } else {
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow = 0;
            }
            addrEnd = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            pAggInfo = _sqlite3DbMallocZero(tls, db, (uint64)64);
            if (pAggInfo != 0) {
                _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_agginfoFree), pAggInfo);
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                goto select_end;
            }
            (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FselId = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselId;
            libc.Xmemset(tls, bp + 48, 0, (uint64)56);
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 48))).Value.FpParse = pParse;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 48))).Value.FpSrcList = pTabList;
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 48)).of(TNameContext.ᏑFuNC)), 0))).Value = pAggInfo;
            if (pGroupBy != 0) {
                v12 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr;
            } else {
                v12 = 0;
            }
            (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnSortingColumn = libc.Uint32FromInt32(v12);
            (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FpGroupBy = pGroupBy;
            _sqlite3ExprAnalyzeAggList(tls, bp + 48, pEList);
            _sqlite3ExprAnalyzeAggList(tls, bp + 48, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy);
            if (pHaving != 0) {
                if (pGroupBy != 0) {
                    _havingToWhere(tls, pParse, p);
                    pWhere = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere;
                }
                _sqlite3ExprAnalyzeAggregates(tls, bp + 48, pHaving);
            }
            (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnAccumulator = (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnColumn;
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy == (uintptr)0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving == (uintptr)0 && (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc == (int32)1) {
                minMaxFlag = _minMaxQuery(tls, db, (@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFExpr, @unsafe.Pointer.FromBox(ᏑpMinMaxOrderBy));
            } else {
                minMaxFlag = (uint8)WHERE_ORDERBY_NORMAL;
            }
            _analyzeAggFuncArgs(tls, pAggInfo, bp + 48);
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                goto select_end;
            }
            if (pGroupBy != 0) {
                pDistinct = (uintptr)0;
                distFlag = (uint16)0;
                eDist = WHERE_DISTINCT_NOOP;
                if ((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc == (int32)1 && (@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FiDistinct >= 0 && (@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFExpr != (uintptr)0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFExpr)).Value.Fflags & (uint32)EP_xIsSelect) == (uint32)0 && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFExpr + 32)) != (uintptr)0) {
                    pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFExpr + 32)) + 8))).Value.FpExpr;
                    pExpr = _sqlite3ExprDup(tls, db, pExpr, 0);
                    pDistinct = _sqlite3ExprListDup(tls, db, pGroupBy, 0);
                    pDistinct = _sqlite3ExprListAppend(tls, pParse, pDistinct, pExpr);
                    if (pDistinct != 0) {
                        v12 = libc.Int32FromInt32(WHERE_WANT_DISTINCT) | libc.Int32FromInt32(WHERE_AGG_DISTINCT);
                    } else {
                        v12 = 0;
                    }
                    distFlag = libc.Uint16FromInt32(v12);
                }
                v1 = pParse + 56;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdx = v12;
                pKeyInfo1 = _sqlite3KeyInfoFromExprList(tls, pParse, pGroupBy, 0, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnColumn);
                addrSortingIdx = _sqlite3VdbeAddOp4(tls, v, (int32)OP_SorterOpen, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdx, libc.Int32FromUint32((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnSortingColumn), 0, pKeyInfo1, -(int32)9);
                v1 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                iUseFlag = v12;
                v1 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                iAbortFlag = v12;
                v1 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                regOutputRow = v12;
                addrOutputRow = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                v1 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                regReset = v12;
                addrReset = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                iAMem = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr;
                iBMem = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr;
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, iAbortFlag);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, iAMem, iAMem + (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr - (int32)1);
                _sqlite3ExprNullRegisterRange(tls, pParse, iAMem, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, regReset, addrReset);
                if (libc.Int32FromUint8(sDistinct.FisTnct) == (int32)2) {
                    v12 = (int32)WHERE_DISTINCTBY;
                } else {
                    v12 = (int32)WHERE_GROUPBY;
                }
                if (orderByGrp != 0) {
                    v15 = (int32)WHERE_SORTBYGROUP;
                } else {
                    v15 = 0;
                }
                pWInfo = _sqlite3WhereBegin(tls, pParse, pTabList, pWhere, pGroupBy, pDistinct, p, libc.Uint16FromInt32(v12 | v15 | libc.Int32FromUint16(distFlag)), 0);
                if (pWInfo == (uintptr)0) {
                    _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pDistinct);
                    goto select_end;
                }
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr != 0) {
                    _optimizeAggregateUseOfIndexedExpr(tls, pParse, p, pAggInfo, bp + 48);
                }
                _assignAggregateRegisters(ref (tls).DerefOrNull(), pParse, pAggInfo);
                eDist = _sqlite3WhereIsDistinct(ref (tls).DerefOrNull(), pWInfo);
                if (_sqlite3WhereIsOrdered(ref (tls).DerefOrNull(), pWInfo) == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr) {
                    groupBySort = 0;
                } else {
                    if (sDistinct.FisTnct != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Distinct) == (uint32)0) {
                        v1 = __ccgo_ts + 21890;
                    } else {
                        v1 = __ccgo_ts + 21899;
                    }
                    _sqlite3VdbeExplain(tls, pParse, (uint8)0, __ccgo_ts + 20655, libc.VaList(bp + 112, v1));
                    groupBySort = (int32)1;
                    nGroupBy = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr;
                    nCol = nGroupBy;
                    j = nGroupBy;
                    i = 0;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnColumn)) {
                            break;
                        }
                        if ((@unsafe.Pointer.To<TAggInfo_col>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaCol + (uintptr)i * 32))).Value.FiSorterColumn >= j) {
                            nCol = nCol + 1;
                            j = j + 1;
                        }
                        goto _47;
_47:;
                        i = i + 1;
                    }
                    regBase = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nCol);
                    _sqlite3ExprCodeExprList(tls, pParse, pGroupBy, regBase, 0, (uint8)0);
                    j = nGroupBy;
                    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FdirectMode = (uint8)1;
                    i = 0;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnColumn)) {
                            break;
                        }
                        pCol = (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaCol + (uintptr)i * 32;
                        if ((@unsafe.Pointer.To<TAggInfo_col>((@unsafe.Pointer)pCol)).Value.FiSorterColumn >= j) {
                            _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TAggInfo_col>((@unsafe.Pointer)pCol)).Value.FpCExpr, j + regBase);
                            j = j + 1;
                        }
                        goto _48;
_48:;
                        i = i + 1;
                    }
                    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FdirectMode = (uint8)0;
                    regRecord = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regBase, nCol, regRecord);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_SorterInsert, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdx, regRecord);
                    _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regRecord);
                    _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, regBase, nCol);
                    _sqlite3WhereEnd(tls, pWInfo);
                    v1 = pParse + 56;
                    v15 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                    v12 = v15;
                    sortPTab = v12;
                    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdxPTab = v12;
                    sortOut = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenPseudo, sortPTab, sortOut, nCol);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_SorterSort, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdx, addrEnd);
                    (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FuseSortingIdx = (uint8)1;
                }
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr != 0) {
                    _aggregateConvertIndexedExprRefToColumn(tls, pAggInfo);
                }
                if (orderByGrp != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_GroupByOrder))) == (uint32)0 && (groupBySort != 0 || _sqlite3WhereIsSorted(ref (tls).DerefOrNull(), pWInfo) != 0)) {
                    (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy = (uintptr)0;
                    _sqlite3VdbeChangeToNoop(tls, v, (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FaddrSortIndex);
                }
                addrTopOfLoop = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                if (groupBySort != 0) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_SorterData, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdx, sortOut, sortPTab);
                }
                j = 0;
                for (;;) {
                    if (!(j < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr)) {
                        break;
                    }
                    iOrderByCol = libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)(pGroupBy + 8 + (uintptr)j * 32 + 24)));
                    if (groupBySort != 0) {
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, sortPTab, j, iBMem + j);
                    } else {
                        (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FdirectMode = (uint8)1;
                        _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pGroupBy + 8 + (uintptr)j * 32))).Value.FpExpr, iBMem + j);
                    }
                    if (iOrderByCol != 0) {
                        pX = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList + 8 + (uintptr)(iOrderByCol - (int32)1) * 32))).Value.FpExpr;
                        pBase = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), pX);
                        while (pBase != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pBase)).Value.Fop) == (int32)TK_IF_NULL_ROW) {
                            pX = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pBase)).Value.FpLeft;
                            pBase = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), pX);
                        }
                        if (pBase != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pBase)).Value.Fop) != (int32)TK_AGG_COLUMN && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pBase)).Value.Fop) != (int32)TK_REGISTER) {
                            _sqlite3ExprToRegister(ref (tls).DerefOrNull(), pX, iAMem + j);
                        }
                    }
                    goto _52;
_52:;
                    j = j + 1;
                }
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_Compare, iAMem, iBMem, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr, _sqlite3KeyInfoRef(ref (tls).DerefOrNull(), pKeyInfo1), -(int32)9);
                addr1 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Jump, addr1 + (int32)1, 0, addr1 + (int32)1);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, regOutputRow, addrOutputRow);
                _sqlite3ExprCodeMove(tls, pParse, iBMem, iAMem, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pGroupBy)).Value.FnExpr);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfPos, iAbortFlag, addrEnd);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, regReset, addrReset);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
                _updateAccumulator(tls, pParse, iUseFlag, pAggInfo, eDist);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, iUseFlag);
                if (groupBySort != 0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_SorterNext, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FsortingIdx, addrTopOfLoop);
                } else {
                    _sqlite3WhereEnd(tls, pWInfo);
                    _sqlite3VdbeChangeToNoop(tls, v, addrSortingIdx);
                }
                _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pDistinct);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, regOutputRow, addrOutputRow);
                _sqlite3VdbeGoto(tls, v, addrEnd);
                addrSetAbort = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, iAbortFlag);
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_Return, regOutputRow);
                _sqlite3VdbeResolveLabel(tls, v, addrOutputRow);
                addrOutputRow = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfPos, iUseFlag, addrOutputRow + (int32)2);
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_Return, regOutputRow);
                _finalizeAggFunctions(tls, pParse, pAggInfo);
                _sqlite3ExprIfFalse(tls, pParse, pHaving, addrOutputRow + (int32)1, (int32)SQLITE_JUMPIFNULL);
                _selectInnerLoop(tls, pParse, p, -(int32)1, __ccgo_ptr(bp), @unsafe.Pointer.FromPinnedBox(ᏑsDistinct), pDest, addrOutputRow + (int32)1, addrSetAbort);
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_Return, regOutputRow);
                _sqlite3VdbeResolveLabel(tls, v, addrReset);
                _resetAccumulator(tls, pParse, pAggInfo);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, iUseFlag);
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_Return, regReset);
                if (libc.Int32FromUint16(distFlag) != 0 && eDist != WHERE_DISTINCT_NOOP) {
                    pF = (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc;
                    _fixDistinctOpenEph(tls, pParse, eDist, (@unsafe.Pointer.To<TAggInfo_func>((@unsafe.Pointer)pF)).Value.FiDistinct, (@unsafe.Pointer.To<TAggInfo_func>((@unsafe.Pointer)pF)).Value.FiDistAddr);
                }
            } else {
                v1 = _isSimpleCount(ref (tls).DerefOrNull(), p, pAggInfo);
                pTab1 = v1;
                if (v1 != (uintptr)0) {
                    iDb1 = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FpSchema);
                    v3 = pParse + 56;
                    v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                    iCsr = v12;
                    pKeyInfo2 = (uintptr)0;
                    pBest = (uintptr)0;
                    iRoot = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.Ftnum;
                    _sqlite3CodeVerifySchema(tls, pParse, iDb1);
                    _sqlite3TableLock(tls, pParse, iDb1, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.Ftnum, (uint8)0, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FzName);
                    if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
                        pBest = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab1);
                    }
                    if (!((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + 24 + 4)) & 0x1) >> 0) != 0)) {
                        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FpIndex;
                        for (;;) {
                            if (!(pIdx != 0)) {
                                break;
                            }
                            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x4) >> 2) == 0 && (int32)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FszIdxRow < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FszTabRow && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere == (uintptr)0 && (!(pBest != 0) || (int32)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FszIdxRow < (int32)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pBest)).Value.FszIdxRow)) {
                                pBest = pIdx;
                            }
                            goto _56;
_56:;
                            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
                        }
                    }
                    if (pBest != 0) {
                        iRoot = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pBest)).Value.Ftnum;
                        pKeyInfo2 = _sqlite3KeyInfoOfIndex(tls, pParse, pBest);
                    }
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_OpenRead, iCsr, libc.Int32FromUint32(iRoot), iDb1, (int32)1);
                    if (pKeyInfo2 != 0) {
                        _sqlite3VdbeChangeP4(tls, v, -(int32)1, pKeyInfo2, -(int32)9);
                    }
                    _assignAggregateRegisters(ref (tls).DerefOrNull(), pParse, pAggInfo);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Count, iCsr, (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FiFirstReg + (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnColumn + 0);
                    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, iCsr);
                    _explainSimpleCount(tls, pParse, pTab1, pBest);
                } else {
                    regAcc = 0;
                    pDistinct1 = (uintptr)0;
                    distFlag1 = (uint16)0;
                    if ((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnAccumulator != 0) {
                        i = 0;
                        for (;;) {
                            if (!(i < (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc)) {
                                break;
                            }
                            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc + (uintptr)i * 32))).Value.FpFExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc))) != (uint32)0) {
                                goto _57;
                            }
                            if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)(@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc + (uintptr)i * 32))).Value.FpFunc)).Value.FfuncFlags & (uint32)SQLITE_FUNC_NEEDCOLL) != 0) {
                                break;
                            }
                            goto _57;
_57:;
                            i = i + 1;
                        }
                        if (i == (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc) {
                            v1 = pParse + 60;
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
                            v12 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
                            regAcc = v12;
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regAcc);
                        }
                    } else {
                        if ((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc == (int32)1 && (@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FiDistinct >= 0) {
                            pDistinct1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFExpr + 32));
                            if (pDistinct1 != 0) {
                                v12 = libc.Int32FromInt32(WHERE_WANT_DISTINCT) | libc.Int32FromInt32(WHERE_AGG_DISTINCT);
                            } else {
                                v12 = 0;
                            }
                            distFlag1 = libc.Uint16FromInt32(v12);
                        }
                    }
                    _assignAggregateRegisters(ref (tls).DerefOrNull(), pParse, pAggInfo);
                    _resetAccumulator(tls, pParse, pAggInfo);
                    pWInfo = _sqlite3WhereBegin(tls, pParse, pTabList, pWhere, pMinMaxOrderBy, pDistinct1, p, libc.Uint16FromInt32(libc.Int32FromUint8(minMaxFlag) | libc.Int32FromUint16(distFlag1)), 0);
                    if (pWInfo == (uintptr)0) {
                        goto select_end;
                    }
                    eDist1 = _sqlite3WhereIsDistinct(ref (tls).DerefOrNull(), pWInfo);
                    _updateAccumulator(tls, pParse, regAcc, pAggInfo, eDist1);
                    if (eDist1 != WHERE_DISTINCT_NOOP) {
                        pF1 = (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc;
                        if (pF1 != 0) {
                            _fixDistinctOpenEph(tls, pParse, eDist1, (@unsafe.Pointer.To<TAggInfo_func>((@unsafe.Pointer)pF1)).Value.FiDistinct, (@unsafe.Pointer.To<TAggInfo_func>((@unsafe.Pointer)pF1)).Value.FiDistAddr);
                        }
                    }
                    if (regAcc != 0) {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, regAcc);
                    }
                    if (minMaxFlag != 0) {
                        _sqlite3WhereMinMaxOptEarlyOut(tls, v, pWInfo);
                    }
                    _sqlite3WhereEnd(tls, pWInfo);
                    _finalizeAggFunctions(tls, pParse, pAggInfo);
                }
                (@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy = (uintptr)0;
                _sqlite3ExprIfFalse(tls, pParse, pHaving, addrEnd, (int32)SQLITE_JUMPIFNULL);
                _selectInnerLoop(tls, pParse, p, -(int32)1, nil, nil, pDest, addrEnd, addrEnd);
            }
            _sqlite3VdbeResolveLabel(tls, v, addrEnd);
        }
        if (libc.Int32FromUint8(sDistinct.FeTnctType) == (int32)WHERE_DISTINCT_UNORDERED) {
            _explainTempTable(tls, pParse, __ccgo_ts + 21890);
        }
        if ((@unsafe.Pointer.To<TSortCtx>(__ccgo_ptr(bp))).Value.FpOrderBy != 0) {
            _generateSortTail(tls, pParse, p, __ccgo_ptr(bp), (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr, pDest);
        }
        _sqlite3VdbeResolveLabel(tls, v, iEnd);
        rc = libc.BoolInt32((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr > 0);
        goto select_end;
select_end:;
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pMinMaxOrderBy);
        _sqlite3VdbeExplainPop(ref (tls).DerefOrNull(), pParse);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _sqlite3SrcListAppendFromTerm(ж<libc.TLS> tls, uintptr pParse, uintptr p, uintptr pTable, uintptr pDatabase, @unsafe.Pointer pAlias, uintptr pSubquery, uintptr pOnUsing) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr pItem = default;
        uintptr pToken = default;
        uintptr v1 = default;
        _ = db;
        _ = pItem;
        _ = pToken;
        _ = v1;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if (!(p != 0) && pOnUsing != (uintptr)0 && ((@unsafe.Pointer.To<TOnOrUsing>((@unsafe.Pointer)pOnUsing)).Value.FpOn != 0 || (@unsafe.Pointer.To<TOnOrUsing>((@unsafe.Pointer)pOnUsing)).Value.FpUsing != 0)) {
            if ((@unsafe.Pointer.To<TOnOrUsing>((@unsafe.Pointer)pOnUsing)).Value.FpOn != 0) {
                v1 = __ccgo_ts + 16171;
            } else {
                v1 = __ccgo_ts + 16174;
            }
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16180, libc.VaList(bp + 8, v1));
            goto append_from_error;
        }
        p = _sqlite3SrcListAppend(tls, pParse, p, pTable, pDatabase);
        if (p == (uintptr)0) {
            goto append_from_error;
        }
        pItem = p + 8 + (uintptr)((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p)).Value.FnSrc - (int32)1) * 80;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME && (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName != 0) {
            if (pDatabase != 0 && (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pDatabase)).Value.Fz != 0) {
                v1 = pDatabase;
            } else {
                v1 = pTable;
            }
            pToken = v1;
            _sqlite3RenameTokenMap(tls, pParse, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName, (@unsafe.Pointer)pToken);
        }
        if (~@unsafe.Pointer.To<uint32>(@unsafe.Add(pAlias, 8)) != 0) {
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzAlias = _sqlite3NameFromToken(tls, db, pAlias);
        }
        if (pSubquery != 0) {
            if (_sqlite3SrcItemAttachSubquery(tls, pParse, pItem, pSubquery, 0) != 0) {
                if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSubquery)).Value.FselFlags & (uint32)SF_NestedFrom) != 0) {
                    libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 14, 0x4000);
                }
            }
        }
        if (pOnUsing == (uintptr)0) {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64))).Value = (uintptr)0;
        } else {
            if ((@unsafe.Pointer.To<TOnOrUsing>((@unsafe.Pointer)pOnUsing)).Value.FpUsing != 0) {
                libc.SetBitFieldPtr32Uint32(pItem + 24 + 4, libc.Uint32FromInt32(1), 11, 0x800);
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64))).Value = (@unsafe.Pointer.To<TOnOrUsing>((@unsafe.Pointer)pOnUsing)).Value.FpUsing;
            } else {
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 64))).Value = (@unsafe.Pointer.To<TOnOrUsing>((@unsafe.Pointer)pOnUsing)).Value.FpOn;
            }
        }
        r = p; goto ᒐdone;
        goto append_from_error;
append_from_error:;
        _sqlite3ClearOnOrUsing(tls, db, pOnUsing);
        _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSubquery);
        r = (uintptr)0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3StartTable(ж<libc.TLS> tls, uintptr pParse, uintptr pName1, uintptr pName2, int32 isTemp, int32 isView, int32 isVirtual, int32 noErr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 addr1 = default;
        int32 fileFormat = default;
        int32 iDb = default;
        int32 reg1 = default;
        int32 reg2 = default;
        int32 reg3 = default;
        int32 v7 = default;
        int32 v8 = default;
        uintptr db = default;
        ref var pName = ref heap(new uintptr(), out var ᏑpName);
        uintptr pTable = default;
        uintptr v = default;
        uintptr zDb = default;
        uintptr zDb1 = default;
        uintptr zName = default;
        uintptr v1 = default;
        bool v6 = default;
        _ = addr1;
        _ = db;
        _ = fileFormat;
        _ = iDb;
        _ = pName;
        _ = pTable;
        _ = reg1;
        _ = reg2;
        _ = reg3;
        _ = v;
        _ = zDb;
        _ = zDb1;
        _ = zName;
        _ = v1;
        _ = v6;
        _ = v7;
        _ = v8;
        zName = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FnewTnum == (uint32)1) {
            iDb = libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb);
            if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && iDb == (int32)1) {
                v1 = __ccgo_ts + 6838;
            } else {
                v1 = __ccgo_ts + 6370;
            }
            zName = _sqlite3DbStrDup(tls, db, v1);
            pName = pName1;
        } else {
            iDb = _sqlite3TwoPartName(tls, pParse, pName1, pName2, @unsafe.Pointer.FromBox(ᏑpName));
            if (iDb < 0) {
                return;
            }
            if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && isTemp != 0 && (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pName2)).Value.Fn > (uint32)0 && iDb != (int32)1) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13920, 0);
                return;
            }
            if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && isTemp != 0) {
                iDb = (int32)1;
            }
            zName = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pName);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
                _sqlite3RenameTokenMap(tls, pParse, zName, (@unsafe.Pointer)pName);
            }
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FsNameToken = ~@unsafe.Pointer.To<TToken>(__ccgo_ptr(pName));
        if (zName == (uintptr)0) {
            return;
        }
        if (isView != 0) {
            v1 = __ccgo_ts + 11219;
        } else {
            v1 = __ccgo_ts + 9451;
        }
        if (_sqlite3CheckObjectName(tls, pParse, zName, v1, zName) != 0) {
            goto begin_table_error;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb) == (int32)1) {
            isTemp = (int32)1;
        }
        zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
        if (libc.Bool(!(libc.Int32FromInt32(OMIT_TEMPDB) != 0)) && isTemp == (int32)1) {
            v1 = __ccgo_ts + 6838;
        } else {
            v1 = __ccgo_ts + 6370;
        }
        if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_INSERT, v1, (uintptr)0, zDb) != 0) {
            goto begin_table_error;
        }
        if (!(isVirtual != 0) && _sqlite3AuthCheck(tls, pParse, libc.Int32FromUint8(_aCode[isTemp + (int32)2 * isView]), zName, (uintptr)0, zDb) != 0) {
            goto begin_table_error;
        }
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) != libc.Int32FromInt32(PARSE_MODE_NORMAL))) {
            zDb1 = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
            if (SQLITE_OK != _sqlite3ReadSchema(tls, pParse)) {
                goto begin_table_error;
            }
            pTable = _sqlite3FindTable(tls, db, zName, zDb1);
            if (pTable != 0) {
                if (!(noErr != 0)) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FeTabType) == (int32)TABTYP_VIEW) {
                        v1 = __ccgo_ts + 11219;
                    } else {
                        v1 = __ccgo_ts + 9451;
                    }
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13961, libc.VaList(bp + 8, v1, pName));
                } else {
                    _sqlite3CodeVerifySchema(tls, pParse, iDb);
                    _sqlite3ForceNotReadOnly(tls, pParse);
                }
                goto begin_table_error;
            }
            if (_sqlite3FindIndex(ref (tls).DerefOrNull(), db, zName, zDb1) != (uintptr)0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13982, libc.VaList(bp + 8, zName));
                goto begin_table_error;
            }
        }
        pTable = _sqlite3DbMallocZero(tls, db, (uint64)120);
        if (pTable == (uintptr)0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_NOMEM;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr + 1;
            goto begin_table_error;
        }
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FzName = zName;
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FiPKey = (int16)(-(int32)1);
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FpSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema;
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnTabRef = (uint32)1;
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnRowLogEst = (int16)200;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTable = pTable;
        v6 = !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0); if (v6) {
            v1 = _sqlite3GetVdbe(tls, pParse);
            v = v1;
        }
        if (v6 && v1 != (uintptr)0) {
            _sqlite3BeginWriteOperation(tls, pParse, (int32)1, iDb);
            if (isVirtual != 0) {
                _sqlite3VdbeAddOp0(tls, v, (int32)OP_VBegin);
            }
            v1 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
            v8 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
            v7 = v8;
            (@unsafe.Pointer.To<_sqlite3AddColumn_type>((@unsafe.Pointer)(pParse + 240))).Value.FregRowid = v7;
            reg1 = v7;
            v1 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
            v8 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
            v7 = v8;
            (@unsafe.Pointer.To<_sqlite3AddColumn_type>((@unsafe.Pointer)(pParse + 240))).Value.FregRoot = v7;
            reg2 = v7;
            v1 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
            v7 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
            reg3 = v7;
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_ReadCookie, iDb, reg3, (int32)BTREE_FILE_FORMAT);
            _sqlite3VdbeUsesBtree(ref (tls).DerefOrNull(), v, iDb);
            addr1 = _sqlite3VdbeAddOp1(tls, v, (int32)OP_If, reg3);
            if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_LegacyFileFmt) != (uint64)0) {
                v7 = (int32)1;
            } else {
                v7 = (int32)SQLITE_MAX_FILE_FORMAT;
            }
            fileFormat = v7;
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_SetCookie, iDb, (int32)BTREE_FILE_FORMAT, fileFormat);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_SetCookie, iDb, (int32)BTREE_TEXT_ENCODING, libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc));
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
            if (isView != 0 || isVirtual != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, reg2);
            } else {
                (@unsafe.Pointer.To<_sqlite3AddColumn_type>((@unsafe.Pointer)(pParse + 240))).Value.FaddrCrTab = _sqlite3VdbeAddOp3(tls, v, (int32)OP_CreateBtree, iDb, reg2, (int32)BTREE_INTKEY);
            }
            _sqlite3OpenSchemaTable(tls, pParse, iDb);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, 0, reg1);
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_Blob, (int32)6, reg3, 0, (uintptr)Ꮡ_nullRow, -(int32)1);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, 0, reg3, reg1);
            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_APPEND);
            _sqlite3VdbeAddOp0(tls, v, (int32)OP_Close);
        } else {
            if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x6) >> 1) != 0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTable + 48))).Value |= (uint32)TF_Imposter;
                if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x6) >> 1) >= (int32)2) {
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTable + 48))).Value |= (uint32)TF_Readonly;
                }
            }
        }
        return;
        goto begin_table_error;
begin_table_error:;
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zName);
        return;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3UniqueConstraint(ж<libc.TLS> tls, uintptr pParse, int32 onError, uintptr pIdx) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 j = default;
        int32 v2 = default;
        uintptr pTab = default;
        uintptr zCol = default;
        uintptr zErr = default;
        TStrAccum _ᴛ1 = default;
        _ = j;
        _ = pTab;
        _ = zCol;
        _ = zErr;
        _ = v2;
        pTab = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable;
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uintptr)0, 0, ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb + 136)));
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr != 0) {
            Xsqlite3_str_appendf(tls, bp, __ccgo_ts + 16316, libc.VaList(bp + 40, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzName));
        } else {
            j = 0;
            for (;;) {
                if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol))) {
                    break;
                }
                zCol = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)j * 2))) * 16))).Value.FzCnName;
                if (j != 0) {
                    Xsqlite3_str_append(tls, bp, __ccgo_ts + 16327, (int32)2);
                }
                Xsqlite3_str_appendall(tls, bp, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
                Xsqlite3_str_append(tls, bp, __ccgo_ts + 1767, (int32)1);
                Xsqlite3_str_appendall(tls, bp, zCol);
                goto _1;
_1:;
                j = j + 1;
            }
        }
        zErr = _sqlite3StrAccumFinish(tls, __ccgo_ptr(bp));
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
            v2 = libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(6).Lsh((int64)(libc.Int32FromInt32(8)));
        } else {
            v2 = libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(8).Lsh((int64)(libc.Int32FromInt32(8)));
        }
        _sqlite3HaltConstraint(tls, pParse, v2, onError, zErr, (int8)(-libc.Int32FromInt32(7)), (uint8)P5_ConstraintUnique);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3Update(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pChanges, uintptr pWhere, int32 onError, uintptr pOrderBy, uintptr pLimit, uintptr pUpsert) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        uintptr aRegIdx = default;
        uintptr aToOpen = default;
        uintptr aXRef = default;
        uintptr db = default;
        uintptr pIdx = default;
        uintptr pKeyInfo = default;
        uintptr pPk = default;
        uintptr pRowidExpr = default;
        uintptr pTab = default;
        uintptr pTrigger = default;
        uintptr pWInfo = default;
        uintptr v = default;
        uintptr v4 = default;
        int32 addrOnce = default;
        int32 addrOpen = default;
        int32 addrTop = default;
        int32 bFinishSeek = default;
        int32 bProgress = default;
        ref var bReplace = ref heap(new int32(), out var ᏑbReplace);
        int32 eOnePass = default;
        int32 flags = default;
        int32 hasFK = default;
        int32 i = default;
        int32 iBaseCur = default;
        int32 iCur = default;
        int32 iDataCur = default;
        int32 iDb = default;
        int32 iEph = default;
        int32 iIdxCur = default;
        ref var iNotUsed1 = ref heap(new int32(), out var ᏑiNotUsed1);
        ref var iNotUsed2 = ref heap(new int32(), out var ᏑiNotUsed2);
        int32 iPk = default;
        int32 iRowidExpr = default;
        int32 isView = default;
        int32 j = default;
        int32 k = default;
        int32 labelBreak = default;
        int32 labelContinue = default;
        int32 nAllIdx = default;
        int32 nChangeFrom = default;
        int32 nEphCol = default;
        int32 nIdx = default;
        int32 nKey = default;
        int32 nOff = default;
        int32 newmask = default;
        int32 rc = default;
        int32 reg = default;
        int32 regKey = default;
        int32 regNew = default;
        int32 regNewRowid = default;
        int32 regOld = default;
        int32 regOldRowid = default;
        int32 regRowCount = default;
        int32 regRowSet = default;
        ref var tmask = ref heap(new int32(), out var Ꮡtmask);
        int32 v1 = default;
        int32 v2 = default;
        Tu8 chngKey = default;
        Tu8 chngPk = default;
        Tu8 chngRowid = default;
        Tu8 v8 = default;
        Tu32 colFlags = default;
        Tu32 oldmask = default;
        Ti16 nPk = default;
        uint64 v12 = default;
        uint32 v42 = default;
        array<int32> _ᴛ1 = new(2);
        TAuthContext _ᴛ2 = default;
        TNameContext _ᴛ3 = new();
        _ = aRegIdx;
        _ = aToOpen;
        _ = aXRef;
        _ = addrOnce;
        _ = addrOpen;
        _ = addrTop;
        _ = bFinishSeek;
        _ = bProgress;
        _ = bReplace;
        _ = chngKey;
        _ = chngPk;
        _ = chngRowid;
        _ = colFlags;
        _ = db;
        _ = eOnePass;
        _ = flags;
        _ = hasFK;
        _ = i;
        _ = iBaseCur;
        _ = iCur;
        _ = iDataCur;
        _ = iDb;
        _ = iEph;
        _ = iIdxCur;
        _ = iNotUsed1;
        _ = iNotUsed2;
        _ = iPk;
        _ = iRowidExpr;
        _ = isView;
        _ = j;
        _ = k;
        _ = labelBreak;
        _ = labelContinue;
        _ = nAllIdx;
        _ = nChangeFrom;
        _ = nEphCol;
        _ = nIdx;
        _ = nKey;
        _ = nOff;
        _ = nPk;
        _ = newmask;
        _ = oldmask;
        _ = pIdx;
        _ = pKeyInfo;
        _ = pPk;
        _ = pRowidExpr;
        _ = pTab;
        _ = pTrigger;
        _ = pWInfo;
        _ = rc;
        _ = reg;
        _ = regKey;
        _ = regNew;
        _ = regNewRowid;
        _ = regOld;
        _ = regOldRowid;
        _ = regRowCount;
        _ = regRowSet;
        _ = tmask;
        _ = v;
        _ = v1;
        _ = v12;
        _ = v2;
        _ = v4;
        _ = v42;
        _ = v8;
        addrTop = 0;
        pWInfo = (uintptr)0;
        aRegIdx = (uintptr)0;
        aXRef = (uintptr)0;
        pRowidExpr = (uintptr)0;
        iRowidExpr = -(int32)1;
        iEph = 0;
        nKey = 0;
        addrOpen = 0;
        iPk = 0;
        nPk = 0;
        bReplace = 0;
        bFinishSeek = (int32)1;
        nChangeFrom = 0;
        regRowCount = 0;
        regOldRowid = 0;
        regNewRowid = 0;
        regNew = 0;
        regOld = 0;
        regRowSet = 0;
        regKey = 0;
        libc.Xmemset(tls, bp, 0, (uint64)16);
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto update_cleanup;
        }
        pTab = _sqlite3SrcListLookup(tls, pParse, pTabList);
        if (pTab == (uintptr)0) {
            goto update_cleanup;
        }
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
        pTrigger = _sqlite3TriggersExist(tls, pParse, pTab, (int32)TK_UPDATE, pChanges, @unsafe.Pointer.FromPinnedBox(Ꮡtmask));
        isView = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW);
        if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc > (int32)1) {
            v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pChanges)).Value.FnExpr;
        } else {
            v1 = 0;
        }
        nChangeFrom = v1;
        if (_sqlite3ViewGetColumnNames(tls, pParse, pTab) != 0) {
            goto update_cleanup;
        }
        if (_sqlite3IsReadOnly(tls, pParse, pTab, pTrigger) != 0) {
            goto update_cleanup;
        }
        v4 = pParse + 56;
        v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
        v1 = v2;
        iDataCur = v1;
        iBaseCur = v1;
        iIdxCur = iDataCur + (int32)1;
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
            v4 = (uintptr)0;
        } else {
            v4 = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
        }
        pPk = v4;
        nIdx = 0;
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            if (pPk == pIdx) {
                iDataCur = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab + 1;
            goto _6;
_6:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
            nIdx = nIdx + 1;
        }
        if (pUpsert != 0) {
            iDataCur = (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FiDataCur;
            iIdxCur = (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FiIdxCur;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab = iBaseCur;
        }
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.FiCursor = iDataCur;
        aXRef = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)4 * libc.Uint64FromInt32((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + nIdx + libc.Int32FromInt32(1)) + libc.Uint64FromInt32(nIdx) + (uint64)2));
        if (aXRef == (uintptr)0) {
            goto update_cleanup;
        }
        aRegIdx = aXRef + (uintptr)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol * 4;
        aToOpen = aRegIdx + (uintptr)nIdx * 4 + libc.UintptrFromInt32(1) * 4;
        libc.Xmemset(tls, aToOpen, (int32)1, libc.Uint64FromInt32(nIdx + (int32)1));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(nIdx + (int32)1)))).Value = (uint8)0;
        i = 0;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)i * 4))).Value = -(int32)1;
            goto _7;
_7:;
            i = i + 1;
        }
        libc.Xmemset(tls, bp + 16, 0, (uint64)56);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16))).Value.FpSrcList = pTabList;
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16)).of(TNameContext.ᏑFuNC)), 0))).Value = pUpsert;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 16))).Value.FncFlags = (int32)NC_UUpsert;
        v = _sqlite3GetVdbe(tls, pParse);
        if (v == (uintptr)0) {
            goto update_cleanup;
        }
        v8 = libc.Uint8FromInt32(0);
        chngPk = v8;
        chngRowid = v8;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pChanges)).Value.FnExpr)) {
                break;
            }
            if (nChangeFrom == 0 && _sqlite3ResolveExprNames(tls, bp + 16, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)i * 32))).Value.FpExpr) != 0) {
                goto update_cleanup;
            }
            j = _sqlite3ColumnIndex(ref (tls).DerefOrNull(), pTab, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)i * 32))).Value.FzEName);
            if (j >= 0) {
                if (j == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                    chngRowid = (uint8)1;
                    pRowidExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)i * 32))).Value.FpExpr;
                    iRowidExpr = i;
                } else {
                    if (pPk != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (int32)COLFLAG_PRIMKEY) != 0) {
                        chngPk = (uint8)1;
                    } else {
                        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (int32)COLFLAG_GENERATED) != 0) {
                            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22752, libc.VaList(bp + 88, (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FzCnName));
                            goto update_cleanup;
                        }
                    }
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)j * 4))).Value = i;
            } else {
                if (pPk == (uintptr)0 && _sqlite3IsRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)i * 32))).Value.FzEName) != 0) {
                    j = -(int32)1;
                    chngRowid = (uint8)1;
                    pRowidExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)i * 32))).Value.FpExpr;
                    iRowidExpr = i;
                } else {
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 12024, libc.VaList(bp + 88, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)i * 32))).Value.FzEName));
                    libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 8, 0x100);
                    goto update_cleanup;
                }
            }
            if (j < 0) {
                v4 = __ccgo_ts + 8271;
            } else {
                v4 = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FzCnName;
            }
            rc = _sqlite3AuthCheck(tls, pParse, (int32)SQLITE_UPDATE, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, v4, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName);
            if (rc == (int32)SQLITE_DENY) {
                goto update_cleanup;
            } else {
                if (rc == (int32)SQLITE_IGNORE) {
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)j * 4))).Value = -(int32)1;
                }
            }
            goto _9;
_9:;
            i = i + 1;
        }
        chngKey = libc.Uint8FromInt32(libc.Int32FromUint8(chngRowid) + libc.Int32FromUint8(chngPk));
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != 0) {
            for (var cond = true; cond; cond = bProgress != 0) {
                bProgress = 0;
                i = 0;
                for (;;) {
                    if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                        break;
                    }
                    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)i * 4)) >= 0) {
                        goto _11;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_GENERATED) == 0) {
                        goto _11;
                    }
                    if (_sqlite3ExprReferencesUpdatedColumn(tls, _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pTab, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16), aXRef, libc.Int32FromUint8(chngRowid)) != 0) {
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)i * 4))).Value = (int32)99999;
                        bProgress = (int32)1;
                    }
                    goto _11;
_11:;
                    i = i + 1;
                }
            }
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
            v12 = libc.Uint64FromInt32(-libc.Int32FromInt32(1));
        } else {
            v12 = (uint64)0;
        }
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.FcolUsed = v12;
        hasFK = _sqlite3FkRequired(tls, pParse, pTab, aXRef, libc.Int32FromUint8(chngKey));
        if (onError == (int32)OE_Replace) {
            bReplace = (int32)1;
        }
        nAllIdx = 0;
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            if (chngKey != 0 || hasFK > (int32)1 || pIdx == pPk || _indexWhereClauseMightChange(tls, pIdx, aXRef, libc.Int32FromUint8(chngRowid)) != 0) {
                v4 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                reg = v1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
            } else {
                reg = 0;
                i = 0;
                for (;;) {
                    if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol))) {
                        break;
                    }
                    if (_indexColumnIsBeingUpdated(tls, pIdx, i, aXRef, libc.Int32FromUint8(chngRowid)) != 0) {
                        v4 = pParse + 60;
                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                        reg = v1;
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
                        if (onError == (int32)OE_Default && libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError) == (int32)OE_Replace) {
                            bReplace = (int32)1;
                        }
                        break;
                    }
                    goto _16;
_16:;
                    i = i + 1;
                }
            }
            if (reg == 0) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(nAllIdx + (int32)1)))).Value = (uint8)0;
            }
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)nAllIdx * 4))).Value = reg;
            goto _13;
_13:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
            nAllIdx = nAllIdx + 1;
        }
        v4 = pParse + 60;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)nAllIdx * 4))).Value = v1;
        if (bReplace != 0) {
            libc.Xmemset(tls, aToOpen, (int32)1, libc.Uint64FromInt32(nIdx + (int32)1));
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0) {
            _sqlite3VdbeCountChanges(ref (tls).DerefOrNull(), v);
        }
        _sqlite3BeginWriteOperation(tls, pParse, libc.BoolInt32(pTrigger != 0 || hasFK != 0), iDb);
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB))) {
            regRowSet = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)nAllIdx * 4));
            v4 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
            v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            v1 = v2;
            regNewRowid = v1;
            regOldRowid = v1;
            if (chngPk != 0 || pTrigger != 0 || hasFK != 0) {
                regOld = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
            }
            if (chngKey != 0 || pTrigger != 0 || hasFK != 0) {
                v4 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                regNewRowid = v1;
            }
            regNew = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
        }
        if (isView != 0) {
            _sqlite3AuthContextPush(ref (tls).DerefOrNull(), pParse, __ccgo_ptr(bp), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
        }
        if (nChangeFrom == 0 && isView != 0) {
            _sqlite3MaterializeView(tls, pParse, pTab, pWhere, pOrderBy, pLimit, iDataCur);
            pOrderBy = (uintptr)0;
            pLimit = (uintptr)0;
        }
        if (nChangeFrom == 0 && _sqlite3ResolveExprNames(tls, bp + 16, pWhere) != 0) {
            goto update_cleanup;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
            _updateVirtualTable(tls, pParse, pTabList, pTab, pChanges, pRowidExpr, aXRef, pWhere, onError);
            goto update_cleanup;
        }
        v1 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        labelBreak = v1;
        labelContinue = v1;
        if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00001)).Lsh((int64)(libc.Int32FromInt32(32))))) != (uint64)0 && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0) && !((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x8) >> 3)) != 0) && pUpsert == (uintptr)0) {
            v4 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            regRowCount = v1;
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regRowCount);
        }
        if (nChangeFrom == 0 && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, regRowSet, regOldRowid);
            v4 = pParse + 56;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
            iEph = v1;
            addrOpen = _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenEphemeral, iEph, 0, regRowSet);
        } else {
            if (pPk != 0) {
                v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol);
            } else {
                v1 = 0;
            }
            nPk = (int16)v1;
            iPk = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)nPk;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nChangeFrom;
            v4 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            regKey = v1;
            if (pUpsert == (uintptr)0) {
                if (isView != 0) {
                    v1 = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
                } else {
                    v1 = 0;
                }
                nEphCol = (int32)nPk + nChangeFrom + v1;
                v4 = pParse + 56;
                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                iEph = v2;
                if (pPk != 0) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, iPk, iPk + (int32)nPk - (int32)1);
                }
                addrOpen = _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, iEph, nEphCol);
                if (pPk != 0) {
                    pKeyInfo = _sqlite3KeyInfoOfIndex(tls, pParse, pPk);
                    if (pKeyInfo != 0) {
                        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FnAllField = libc.Uint16FromInt32(nEphCol);
                        _sqlite3VdbeAppendP4(tls, v, pKeyInfo, -(int32)9);
                    }
                }
                if (nChangeFrom != 0) {
                    _updateFromSelect(tls, pParse, iEph, pPk, pChanges, pTabList, pWhere, pOrderBy, pLimit);
                    if (isView != 0) {
                        iDataCur = iEph;
                    }
                }
            }
        }
        if (nChangeFrom != 0) {
            _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
            eOnePass = ONEPASS_OFF;
            nKey = (int32)nPk;
            regKey = iPk;
        } else {
            if (pUpsert != 0) {
                pWInfo = (uintptr)0;
                eOnePass = (int32)ONEPASS_SINGLE;
                _sqlite3ExprIfFalse(tls, pParse, pWhere, labelBreak, (int32)SQLITE_JUMPIFNULL);
                bFinishSeek = 0;
            } else {
                flags = (int32)WHERE_ONEPASS_DESIRED;
                if (!((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0) && !(pTrigger != 0) && !(hasFK != 0) && !(chngKey != 0) && !(bReplace != 0) && (pWhere == (uintptr)0 || !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pWhere)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != libc.Uint32FromInt32(0)))) {
                    flags = flags | (int32)WHERE_ONEPASS_MULTIROW;
                }
                pWInfo = _sqlite3WhereBegin(tls, pParse, pTabList, pWhere, (uintptr)0, (uintptr)0, (uintptr)0, libc.Uint16FromInt32(flags), iIdxCur);
                if (pWInfo == (uintptr)0) {
                    goto update_cleanup;
                }
                eOnePass = _sqlite3WhereOkOnePass(tls, pWInfo, bp + 72);
                bFinishSeek = _sqlite3WhereUsesDeferredSeek(ref (tls).DerefOrNull(), pWInfo);
                if (eOnePass != (int32)ONEPASS_SINGLE) {
                    _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
                    if (eOnePass == (int32)ONEPASS_MULTI) {
                        iCur = @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2);
                        if (iCur >= 0 && iCur != iDataCur && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(iCur - iBaseCur))) != 0) {
                            eOnePass = ONEPASS_OFF;
                        }
                    }
                }
            }
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iDataCur, regOldRowid);
                if (eOnePass == ONEPASS_OFF) {
                    v4 = pParse + 60;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)nAllIdx * 4))).Value = v1;
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, iEph, regRowSet, regOldRowid);
                } else {
                    if (addrOpen != 0) {
                        _sqlite3VdbeChangeToNoop(tls, v, addrOpen);
                    }
                }
            } else {
                i = 0;
                for (;;) {
                    if (!(i < (int32)nPk)) {
                        break;
                    }
                    _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iDataCur, (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2))), iPk + i);
                    goto _39;
_39:;
                    i = i + 1;
                }
                if (eOnePass != 0) {
                    if (addrOpen != 0) {
                        _sqlite3VdbeChangeToNoop(tls, v, addrOpen);
                    }
                    nKey = (int32)nPk;
                    regKey = iPk;
                } else {
                    _sqlite3VdbeAddOp4(tls, v, (int32)OP_MakeRecord, iPk, (int32)nPk, regKey, _sqlite3IndexAffinityStr(tls, db, pPk), (int32)nPk);
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, iEph, regKey, iPk, (int32)nPk);
                }
            }
        }
        if (pUpsert == (uintptr)0) {
            if (nChangeFrom == 0 && eOnePass != (int32)ONEPASS_MULTI) {
                _sqlite3WhereEnd(tls, pWInfo);
            }
            if (!(isView != 0)) {
                addrOnce = 0;
                iNotUsed1 = 0;
                iNotUsed2 = 0;
                if (eOnePass != ONEPASS_OFF) {
                    if (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), 0, 2) >= 0) {
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), 0, 2) - iBaseCur)))).Value = (uint8)0;
                    }
                    if (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2) >= 0) {
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2) - iBaseCur)))).Value = (uint8)0;
                    }
                }
                if (eOnePass == (int32)ONEPASS_MULTI && nIdx - libc.BoolInt32(@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2) >= 0) > 0) {
                    addrOnce = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Once);
                }
                _sqlite3OpenTableAndIndices(tls, pParse, pTab, (int32)OP_OpenWrite, (uint8)0, iBaseCur, aToOpen, @unsafe.Pointer.FromPinnedBox(ᏑiNotUsed1), @unsafe.Pointer.FromPinnedBox(ᏑiNotUsed2));
                if (addrOnce != 0) {
                    _sqlite3VdbeJumpHereOrPopInst(ref (tls).DerefOrNull(), v, addrOnce);
                }
            }
            if (eOnePass != ONEPASS_OFF) {
                if (@unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), 0, 2) != iDataCur && @unsafe.Pointer.ElementRef<int32>(__ccgo_ptr(bp + 72), (int32)1, 2) != iDataCur) {
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iDataCur, labelBreak, regKey, nKey);
                }
                if (eOnePass != (int32)ONEPASS_SINGLE) {
                    labelContinue = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                }
                if (pPk != 0) {
                    v1 = regKey;
                } else {
                    v1 = regOldRowid;
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, v1, labelBreak);
            } else {
                if (pPk != 0 || nChangeFrom != 0) {
                    labelContinue = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iEph, labelBreak);
                    addrTop = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                    if (nChangeFrom != 0) {
                        if (!(isView != 0)) {
                            if (pPk != 0) {
                                i = 0;
                                for (;;) {
                                    if (!(i < (int32)nPk)) {
                                        break;
                                    }
                                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iEph, i, iPk + i);
                                    goto _41;
_41:;
                                    i = i + 1;
                                }
                                _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iDataCur, labelContinue, iPk, (int32)nPk);
                            } else {
                                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iEph, regOldRowid);
                                _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDataCur, labelContinue, regOldRowid);
                            }
                        }
                    } else {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_RowData, iEph, regKey);
                        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iDataCur, labelContinue, regKey, 0);
                    }
                } else {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iEph, labelBreak);
                    labelContinue = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                    addrTop = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iEph, regOldRowid);
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDataCur, labelContinue, regOldRowid);
                }
            }
        }
        if (chngRowid != 0) {
            if (nChangeFrom == 0) {
                _sqlite3ExprCode(tls, pParse, pRowidExpr, regNewRowid);
            } else {
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iEph, iRowidExpr, regNewRowid);
            }
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_MustBeInt, regNewRowid);
            _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
        }
        if (chngPk != 0 || hasFK != 0 || pTrigger != 0) {
            if (hasFK != 0) {
                v42 = _sqlite3FkOldmask(tls, pParse, pTab);
            } else {
                v42 = (uint32)0;
            }
            oldmask = v42;
            oldmask = (Tu32)(oldmask | _sqlite3TriggerColmask(tls, pParse, pTrigger, pChanges, 0, libc.Int32FromInt32(TRIGGER_BEFORE) | libc.Int32FromInt32(TRIGGER_AFTER), pTab, onError));
            i = 0;
            for (;;) {
                if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                    break;
                }
                colFlags = (uint32)(@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags;
                k = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), pTab, (int16)i) + regOld;
                if (oldmask == (uint32)0xffffffffU || i < (int32)32 && (Tu32)(oldmask & (libc.Uint32FromInt32(1).Lsh((int64)(i)))) != (uint32)0 || (Tu32)(colFlags & (uint32)COLFLAG_PRIMKEY) != (uint32)0) {
                    _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iDataCur, i, k);
                } else {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, k);
                }
                goto _43;
_43:;
                i = i + 1;
            }
            if (libc.Int32FromUint8(chngRowid) == 0 && pPk == (uintptr)0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Copy, regOldRowid, regNewRowid);
            }
        }
        newmask = libc.Int32FromUint32(_sqlite3TriggerColmask(tls, pParse, pTrigger, pChanges, (int32)1, (int32)TRIGGER_BEFORE, pTab, onError));
        i = 0;
        k = regNew;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            if (i == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, k);
            } else {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_GENERATED) != 0) {
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) != 0) {
                        k = k - 1;
                    }
                } else {
                    j = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)i * 4));
                    if (j >= 0) {
                        if (nChangeFrom != 0) {
                            if (isView != 0) {
                                v1 = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
                            } else {
                                v1 = (int32)nPk;
                            }
                            nOff = v1;
                            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iEph, nOff + j, k);
                        } else {
                            _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pChanges + 8 + (uintptr)j * 32))).Value.FpExpr, k);
                        }
                    } else {
                        if (0 == (tmask & (int32)TRIGGER_BEFORE) || i > (int32)31 || (libc.Uint32FromInt32(newmask) & (libc.Uint32FromInt32(1).Lsh((int64)(i)))) != 0) {
                            _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iDataCur, i, k);
                            bFinishSeek = 0;
                        } else {
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, k);
                        }
                    }
                }
            }
            goto _44;
_44:;
            i = i + 1;
            k = k + 1;
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != 0) {
            _sqlite3ComputeGeneratedColumns(tls, pParse, regNew, pTab);
        }
        if ((tmask & (int32)TRIGGER_BEFORE) != 0) {
            _sqlite3TableAffinity(tls, v, pTab, regNew);
            _sqlite3CodeRowTrigger(tls, pParse, pTrigger, (int32)TK_UPDATE, pChanges, (int32)TRIGGER_BEFORE, pTab, regOldRowid, onError, labelContinue);
            if (!(isView != 0)) {
                if (pPk != 0) {
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iDataCur, labelContinue, regKey, nKey);
                } else {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDataCur, labelContinue, regOldRowid);
                }
                i = 0;
                k = regNew;
                for (;;) {
                    if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                        break;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_GENERATED) != 0) {
                        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) != 0) {
                            k = k - 1;
                        }
                    } else {
                        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(aXRef + (uintptr)i * 4)) < 0 && i != (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                            _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iDataCur, i, k);
                        }
                    }
                    goto _46;
_46:;
                    i = i + 1;
                    k = k + 1;
                }
                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_HasGenerated) != 0) {
                    _sqlite3ComputeGeneratedColumns(tls, pParse, regNew, pTab);
                }
            }
        }
        if (!(isView != 0)) {
            _sqlite3GenerateConstraintChecks(tls, pParse, pTab, aRegIdx, iDataCur, iIdxCur, regNewRowid, regOldRowid, chngKey, libc.Uint8FromInt32(onError), labelContinue, @unsafe.Pointer.FromPinnedBox(ᏑbReplace), aXRef, (uintptr)0);
            if (bReplace != 0 || chngKey != 0) {
                if (pPk != 0) {
                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iDataCur, labelContinue, regKey, nKey);
                } else {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDataCur, labelContinue, regOldRowid);
                }
            }
            if (hasFK != 0) {
                _sqlite3FkCheck(tls, pParse, pTab, regOldRowid, 0, aXRef, libc.Int32FromUint8(chngKey));
            }
            _sqlite3GenerateRowIndexDelete(tls, pParse, pTab, iDataCur, iIdxCur, aRegIdx, -(int32)1);
            if (bFinishSeek != 0) {
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_FinishSeek, iDataCur);
            }
            if (hasFK > (int32)1 || chngKey != 0) {
                v1 = 0;
            } else {
                v1 = (int32)OPFLAG_ISNOOP;
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Delete, iDataCur, (int32)OPFLAG_ISUPDATE | v1, regNewRowid);
            if (eOnePass == (int32)ONEPASS_MULTI) {
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_SAVEPOSITION);
            }
            if (!((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0)) {
                _sqlite3VdbeAppendP4(tls, v, pTab, -(int32)5);
            }
            if (hasFK != 0) {
                _sqlite3FkCheck(tls, pParse, pTab, 0, regNewRowid, aXRef, libc.Int32FromUint8(chngKey));
            }
            if (eOnePass == (int32)ONEPASS_MULTI) {
                v1 = (int32)OPFLAG_SAVEPOSITION;
            } else {
                v1 = 0;
            }
            _sqlite3CompleteInsertion(tls, pParse, pTab, iDataCur, iIdxCur, regNewRowid, aRegIdx, (int32)OPFLAG_ISUPDATE | v1, 0, 0);
            if (hasFK != 0) {
                _sqlite3FkActions(tls, pParse, pTab, pChanges, regOldRowid, aXRef, libc.Int32FromUint8(chngKey));
            }
        }
        if (regRowCount != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_AddImm, regRowCount, (int32)1);
        }
        if (pTrigger != 0) {
            _sqlite3CodeRowTrigger(tls, pParse, pTrigger, (int32)TK_UPDATE, pChanges, (int32)TRIGGER_AFTER, pTab, regOldRowid, onError, labelContinue);
        }
        if (eOnePass == (int32)ONEPASS_SINGLE) {
        } else {
            if (eOnePass == (int32)ONEPASS_MULTI) {
                _sqlite3VdbeResolveLabel(tls, v, labelContinue);
                _sqlite3WhereEnd(tls, pWInfo);
            } else {
                _sqlite3VdbeResolveLabel(tls, v, labelContinue);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iEph, addrTop);
            }
        }
        _sqlite3VdbeResolveLabel(tls, v, labelBreak);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0 && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab == (uintptr)0 && pUpsert == (uintptr)0) {
            _sqlite3AutoincrementEnd(tls, pParse);
        }
        if (regRowCount != 0) {
            _sqlite3CodeChangeCount(tls, v, regRowCount, __ccgo_ts + 22788);
        }
        goto update_cleanup;
update_cleanup:;
        _sqlite3AuthContextPop(ref (tls).DerefOrNull(), __ccgo_ptr(bp));
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aXRef);
        _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pTabList);
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pChanges);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhere);
        return;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _sqlite3WhereBegin(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pWhere, uintptr pOrderBy, uintptr pResultSet, uintptr pSelect, Tu16 wctrlFlags, int32 iAuxArg) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        int32 addrExplain = default;
        int32 bBloomable = default;
        int32 bOnerow = default;
        int32 iCur = default;
        int32 iDb = default;
        int32 iIndexCur = default;
        int32 iOnce = default;
        int32 ii = default;
        int32 n = default;
        int32 nByteWInfo = default;
        int32 nTabList = default;
        int32 op = default;
        int32 op1 = default;
        int32 rc = default;
        int32 wsFlags = default;
        int32 wsFlags1 = default;
        int32 v1 = default;
        TBitmask b = default;
        TBitmask notReady = default;
        Tu8 bFordelete = default;
        uintptr db = default;
        uintptr p = default;
        uintptr pInfo = default;
        uintptr pIx = default;
        uintptr pJ = default;
        uintptr pLevel = default;
        uintptr pLoop = default;
        uintptr pMaskSet = default;
        uintptr pPk = default;
        uintptr pRJ = default;
        uintptr pSrc = default;
        uintptr pSubq = default;
        uintptr pT = default;
        uintptr pTab = default;
        uintptr pTabItem = default;
        uintptr pVTab = default;
        uintptr pWInfo = default;
        uintptr pX = default;
        uintptr v = default;
        uintptr v7 = default;
        uintptr v8 = default;
        bool v19 = default;
        TWhereLoopBuilder _ᴛ1 = default;
        _ = addrExplain;
        _ = b;
        _ = bBloomable;
        _ = bFordelete;
        _ = bOnerow;
        _ = db;
        _ = iCur;
        _ = iDb;
        _ = iIndexCur;
        _ = iOnce;
        _ = ii;
        _ = n;
        _ = nByteWInfo;
        _ = nTabList;
        _ = notReady;
        _ = op;
        _ = op1;
        _ = p;
        _ = pInfo;
        _ = pIx;
        _ = pJ;
        _ = pLevel;
        _ = pLoop;
        _ = pMaskSet;
        _ = pPk;
        _ = pRJ;
        _ = pSrc;
        _ = pSubq;
        _ = pT;
        _ = pTab;
        _ = pTabItem;
        _ = pVTab;
        _ = pWInfo;
        _ = pX;
        _ = rc;
        _ = v;
        _ = wsFlags;
        _ = wsFlags1;
        _ = v1;
        _ = v19;
        _ = v7;
        _ = v8;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        bFordelete = (uint8)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        libc.Xmemset(tls, bp, 0, (uint64)56);
        if (pOrderBy != 0 && (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))) {
            pOrderBy = (uintptr)0;
            wctrlFlags = libc.Uint16FromInt32((int32)wctrlFlags & ~libc.Int32FromInt32(WHERE_WANT_DISTINCT));
            wctrlFlags = libc.Uint16FromInt32((int32)wctrlFlags | libc.Int32FromInt32(WHERE_KEEP_ALL_JOINS));
        }
        if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc > libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24270, libc.VaList(bp + 64, libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))));
            r = (uintptr)0; goto ᒐdone;
        }
        if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) != 0) {
            v1 = (int32)1;
        } else {
            v1 = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc;
        }
        nTabList = v1;
        nByteWInfo = libc.Int32FromUint64(((uint64)(libc.UintptrFromInt32(0) + 856) + libc.Uint64FromInt32(nTabList) * libc.Uint64FromInt64(112) + libc.Uint64FromInt32(7)) & libc.Uint64FromInt32(~libc.Int32FromInt32(7)));
        pWInfo = _sqlite3DbMallocRawNN(tls, db, (uint64)(libc.Uint64FromInt32(nByteWInfo) + (uint64)104));
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pWInfo);
            pWInfo = (uintptr)0;
            goto whereBeginError;
        }
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList = pTabList;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy = pOrderBy;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpResultSet = pResultSet;
        v1 = -libc.Int32FromInt32(1);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pWInfo + 40 + 1 * 4))).Value = v1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pWInfo + 40))).Value = v1;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel = libc.Uint8FromInt32(nTabList);
        v1 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiContinue = v1;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiBreak = v1;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags = wctrlFlags;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiLimit = (int16)iAuxArg;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsavedNQueryLoop = (int32)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnQueryLoop;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect = pSelect;
        libc.Xmemset(tls, pWInfo + 65, 0, (uint64)(libc.UintptrFromInt32(0) + 104) - (uint64)(libc.UintptrFromInt32(0) + 65));
        libc.Xmemset(tls, pWInfo + 856, 0, (uint64)104 + libc.Uint64FromInt32(nTabList) * (uint64)112);
        pMaskSet = pWInfo + 592;
        (@unsafe.Pointer.To<TWhereMaskSet>((@unsafe.Pointer)pMaskSet)).Value.Fn = 0;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pMaskSet + 8))).Value = -(int32)99;
        (@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FpWInfo = pWInfo;
        (@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FpWC = pWInfo + 104;
        (@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FpNew = pWInfo + (uintptr)nByteWInfo;
        _whereLoopInit(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FpNew);
        _sqlite3WhereClauseInit(ref (tls).DerefOrNull(), pWInfo + 104, pWInfo);
        _sqlite3WhereSplit(tls, pWInfo + 104, pWhere, (uint8)TK_AND);
        if (nTabList == 0) {
            if (pOrderBy != 0) {
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat = (int8)(@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr;
            }
            if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_DistinctOpt))) == (uint32)0) {
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct = (uint8)WHERE_DISTINCT_UNIQUE;
            }
            if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect != 0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect)).Value.FselFlags & (uint32)SF_MultiValue) == (uint32)0) {
                _sqlite3VdbeExplain(tls, pParse, (uint8)0, __ccgo_ts + 24298, 0);
            }
        } else {
            ii = 0;
            for (;;) {
                _createMask(ref (tls).DerefOrNull(), pMaskSet, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8 + (uintptr)ii * 80))).Value.FiCursor);
                _sqlite3WhereTabFuncArgs(tls, pParse, pTabList + 8 + (uintptr)ii * 80, pWInfo + 104);
                goto _5;
_5:;
                ii = ii + 1;
                v1 = ii;
                if (!(v1 < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
                    break;
                }
            }
        }
        _sqlite3WhereExprAnalyze(tls, pTabList, pWInfo + 104);
        if (pSelect != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpLimit != 0) {
            _sqlite3WhereAddLimit(tls, pWInfo + 104, pSelect);
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto whereBeginError;
        }
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FpWC)).Value.FnBase)) {
                break;
            }
            pT = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FpWC)).Value.Fa + (uintptr)ii * 56;
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pT)).Value.FwtFlags) & (int32)TERM_VIRTUAL) != 0) {
                goto _6;
            }
            pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pT)).Value.FpExpr;
            if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pT)).Value.FprereqAll == (uint64)0 && (nTabList == 0 || _exprIsDeterministic(tls, pX) != 0) && !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_InnerON))) != (uint32)0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0)) {
                _sqlite3ExprIfFalse(tls, pParse, pX, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiBreak, (int32)SQLITE_JUMPIFNULL);
                v7 = pT + 18;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v7)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v7)) | libc.Int32FromInt32(TERM_CODED));
            }
            goto _6;
_6:;
            ii = ii + 1;
        }
        if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0) {
            if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_DistinctOpt))) != (uint32)0) {
                wctrlFlags = libc.Uint16FromInt32((int32)wctrlFlags & ~libc.Int32FromInt32(WHERE_WANT_DISTINCT));
                v7 = pWInfo + 60;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v7)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v7)) & ~libc.Int32FromInt32(WHERE_WANT_DISTINCT));
            } else {
                if (_isDistinctRedundant(tls, pParse, pTabList, pWInfo + 104, pResultSet) != 0) {
                    (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct = (uint8)WHERE_DISTINCT_UNIQUE;
                } else {
                    if (pOrderBy == (uintptr)0) {
                        v7 = pWInfo + 60;
                        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v7)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v7)) | libc.Int32FromInt32(WHERE_DISTINCTBY));
                        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy = pResultSet;
                    }
                }
            }
        }
        if (nTabList != (int32)1 || _whereShortCut(tls, __ccgo_ptr(bp)) == 0) {
            rc = _whereLoopAddAll(tls, __ccgo_ptr(bp));
            if (rc != 0) {
                goto whereBeginError;
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLoopBuilder>(__ccgo_ptr(bp))).Value.FbldFlags2) & (int32)SQLITE_BLDF2_2NDPASS) != 0) {
                while ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops != 0) {
                    p = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops;
                    (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FpNextLoop;
                    _whereLoopDelete(tls, db, p);
                }
                rc = _whereLoopAddAll(tls, __ccgo_ptr(bp));
                if (rc != 0) {
                    goto whereBeginError;
                }
            }
            _wherePathSolver(tls, pWInfo, 0);
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                goto whereBeginError;
            }
            if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy != 0) {
                _whereInterstageHeuristic(ref (tls).DerefOrNull(), pWInfo);
                if ((int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnRowOut < 0) {
                    v1 = (int32)1;
                } else {
                    v1 = (int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnRowOut + (int32)1;
                }
                _wherePathSolver(tls, pWInfo, (int16)v1);
                if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                    goto whereBeginError;
                }
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0) {
                v7 = pWInfo + 70;
                (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v7)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v7)) - libc.Int32FromInt32(30));
            }
        }
        if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy == (uintptr)0 && (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_ReverseOrder) != (uint64)0) {
            _whereReverseScanOrder(ref (tls).DerefOrNull(), pWInfo);
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto whereBeginError;
        }
        notReady = ~libc.Uint64FromInt32(0);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) >= (int32)2 && pResultSet != (uintptr)0 && 0 == (libc.Int32FromUint16(wctrlFlags) & (libc.Int32FromInt32(WHERE_AGG_DISTINCT) | libc.Int32FromInt32(WHERE_KEEP_ALL_JOINS))) && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OmitNoopJoin))) == (uint32)0) {
            notReady = _whereOmitNoopJoin(tls, pWInfo, notReady);
            nTabList = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) >= (int32)2 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_BloomFilter))) == (uint32)0) {
            _whereCheckIfBloomFilterIsUseful(ref (tls).DerefOrNull(), pWInfo);
        }
        v7 = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse + 28;
        (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v7)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v7)) + (int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnRowOut);
        if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_ONEPASS_DESIRED) != 0) {
            wsFlags = libc.Int32FromUint32((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856))).Value.FpWLoop)).Value.FwsFlags);
            bOnerow = libc.BoolInt32((wsFlags & (int32)WHERE_ONEROW) != 0);
            if (bOnerow != 0 || 0 != (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_ONEPASS_MULTIROW) && !(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.FpSTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB)) && (0 == (wsFlags & (int32)WHERE_MULTI_OR) || (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_DUPLICATES_OK) != 0) && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OnePass))) == (uint32)0) {
                if (bOnerow != 0) {
                    v1 = (int32)ONEPASS_SINGLE;
                } else {
                    v1 = (int32)ONEPASS_MULTI;
                }
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass = libc.Uint8FromInt32(v1);
                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.FpSTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 && (wsFlags & (int32)WHERE_IDX_ONLY) != 0) {
                    if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_ONEPASS_MULTIROW) != 0) {
                        bFordelete = (uint8)OPFLAG_FORDELETE;
                    }
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856))).Value.FpWLoop)).Value.FwsFlags = libc.Uint32FromInt32(wsFlags & ~libc.Int32FromInt32(WHERE_IDX_ONLY));
                }
            }
        }
        ii = 0;
        pLevel = pWInfo + 856;
        for (;;) {
            if (!(ii < nTabList)) {
                break;
            }
            pTabItem = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
            pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FpSTab;
            iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
            pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            if (ii == 0 || (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(pTabItem))).Value.Ffg.Fjointype) & (int32)JT_LEFT) != 0) {
                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk;
            } else {
                if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)(ii - (int32)1) * 112))).Value.FpRJ != 0) {
                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)(ii - (int32)1) * 112))).Value.FaddrBrk;
                } else {
                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)(ii - (int32)1) * 112))).Value.FaddrHalt;
                }
            }
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Ephemeral) != (uint32)0 || libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW) {
            } else {
                if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) != (uint32)0) {
                    pVTab = _sqlite3GetVTable(ref (tls).DerefOrNull(), db, pTab);
                    iCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor;
                    _sqlite3VdbeAddOp4(tls, v, (int32)OP_VOpen, iCur, 0, 0, pVTab, -(int32)12);
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                    } else {
                        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IDX_ONLY) == (uint32)0 && (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) == 0 || (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0) {
                            op = (int32)OP_OpenRead;
                            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass) != ONEPASS_OFF) {
                                op = (int32)OP_OpenWrite;
                                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pWInfo + 40))).Value = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor;
                            }
                            _sqlite3OpenTable(tls, pParse, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor, iDb, pTab, op);
                            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass) == ONEPASS_OFF && (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol < libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(libc.Int32FromInt32(TF_HasGenerated) | libc.Int32FromInt32(TF_WithoutRowid))) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_AUTO_INDEX) | libc.Int32FromInt32(WHERE_BLOOMFILTER))) == (uint32)0) {
                                b = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FcolUsed;
                                n = 0;
                                for (;;) {
                                    if (!(b != 0)) {
                                        break;
                                    }
                                    goto _15;
_15:;
                                    b = b >> (int)((int32)1);
                                    n = n + 1;
                                }
                                _sqlite3VdbeChangeP4(tls, v, -(int32)1, (uintptr)(int64)n, -(int32)3);
                            }
                            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)bFordelete);
                            if (ii >= (int32)2 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(pTabItem))).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_LEFT))) == 0 && (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt == (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856))).Value.FaddrHalt) {
                                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfEmpty, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiBreak);
                            }
                        } else {
                            _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum, (uint8)0, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
                        }
                    }
                }
            }
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_INDEXED) != 0) {
                pIx = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
                op1 = (int32)OP_OpenRead;
                if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY && (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) != 0) {
                    iIndexCur = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur;
                    op1 = 0;
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass) != ONEPASS_OFF) {
                        pJ = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FpSTab)).Value.FpIndex;
                        iIndexCur = iAuxArg;
                        while (pJ != 0 && pJ != pIx) {
                            iIndexCur = iIndexCur + 1;
                            pJ = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pJ)).Value.FpNext;
                        }
                        op1 = (int32)OP_OpenWrite;
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pWInfo + 40 + 1 * 4))).Value = iIndexCur;
                    } else {
                        if (iAuxArg != 0 && (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) != 0) {
                            iIndexCur = iAuxArg;
                            op1 = (int32)OP_ReopenIdx;
                        } else {
                            v7 = pParse + 56;
                            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7);
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7) + 1;
                            iIndexCur = v1;
                            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIx + 100)) & 0x800) >> 11) != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_IndexedExpr))) == (uint32)0) {
                                _whereAddIndexedExpr(tls, pParse, pIx, iIndexCur, pTabItem);
                            }
                            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIx)).Value.FpPartIdxWhere != 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) == 0) {
                                _wherePartIdxExpr(tls, pParse, pIx, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIx)).Value.FpPartIdxWhere, nil, iIndexCur, pTabItem);
                            }
                        }
                    }
                }
                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur = iIndexCur;
                if (op1 != 0) {
                    _sqlite3VdbeAddOp3(tls, v, op1, iIndexCur, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIx)).Value.Ftnum), iDb);
                    _sqlite3VdbeSetP4KeyInfo(tls, pParse, pIx);
                    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_CONSTRAINT) != (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_RANGE) | libc.Int32FromInt32(WHERE_SKIPSCAN))) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_BIGNULL_SORT) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IN_SEEKSCAN) == (uint32)0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_ORDERBY_MIN) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct) != (int32)WHERE_DISTINCT_ORDERED) {
                        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_SEEKEQ);
                    }
                }
            }
            if (iDb >= 0) {
                _sqlite3CodeVerifySchema(tls, pParse, iDb);
            }
            v19 = (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0; if (v19) {
                v7 = _sqlite3WhereMalloc(tls, pWInfo, (uint64)20);
                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ = v7;
            }
            if (v19 && v7 != (uintptr)0) {
                pRJ = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ;
                bBloomable = 0;
                v8 = pParse + 56;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8) + 1;
                (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch = v1;
                v7 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7);
                (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregReturn = v1;
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregReturn);
                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch, (int32)1);
                    bBloomable = (int32)1;
                    pInfo = _sqlite3KeyInfoAlloc(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (int32)1, 0);
                    if (pInfo != 0) {
                        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pInfo + 32))).Value = (uintptr)0;
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pInfo)).Value.FaSortFlags))).Value = (uint8)0;
                        _sqlite3VdbeAppendP4(tls, v, pInfo, -(int32)9);
                    }
                } else {
                    pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol));
                    _sqlite3VdbeSetP4KeyInfo(tls, pParse, pPk);
                    bBloomable = _sqlite3IndexBloomable(ref (tls).DerefOrNull(), pPk, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol));
                }
                if (bBloomable != 0) {
                    v7 = pParse + 60;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7) + 1;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7);
                    (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom = v1;
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Blob, (int32)65536, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom);
                } else {
                    (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom = 0;
                }
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLoop + 48))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(WHERE_IDX_ONLY));
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat = 0;
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct = (uint8)WHERE_DISTINCT_UNORDERED;
            }
            goto _14;
_14:;
            ii = ii + 1;
            pLevel += 112;
        }
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiTop = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            goto whereBeginError;
        }
        ii = 0;
        for (;;) {
            if (!(ii < nTabList)) {
                break;
            }
            if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                goto whereBeginError;
            }
            pLevel = pWInfo + 856 + (uintptr)ii * 112;
            wsFlags1 = libc.Int32FromUint32((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop)).Value.FwsFlags);
            pSrc = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x20) >> 5) != 0) {
                iOnce = 0;
                pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 72));
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x10) >> 4) == 0) {
                    iOnce = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Once);
                } else {
                    iOnce = 0;
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub);
                if (iOnce != 0) {
                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, iOnce);
                }
            }
            if ((wsFlags1 & (libc.Int32FromInt32(WHERE_AUTO_INDEX) | libc.Int32FromInt32(WHERE_BLOOMFILTER))) != 0) {
                if ((wsFlags1 & (int32)WHERE_AUTO_INDEX) != 0) {
                    _constructAutomaticIndex(tls, pParse, pWInfo + 104, notReady, pLevel);
                } else {
                    _sqlite3ConstructBloomFilter(tls, pWInfo, ii, pLevel, notReady);
                }
                if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                    goto whereBeginError;
                }
            }
            addrExplain = _sqlite3WhereExplainOneScan(tls, pParse, pTabList, pLevel, wctrlFlags);
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBody = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            notReady = _sqlite3WhereCodeOneLoopStart(tls, pParse, v, pWInfo, ii, pLevel, notReady);
            (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiContinue = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrCont;
            if ((wsFlags1 & (int32)WHERE_MULTI_OR) == 0 && (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) == 0) {
                _ = addrExplain;
            }
            goto _26;
_26:;
            ii = ii + 1;
        }
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiEndWhere = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        r = pWInfo; goto ᒐdone;
        goto whereBeginError;
whereBeginError:;
        if (pWInfo != 0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnQueryLoop = (int16)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsavedNQueryLoop;
            _whereInfoFree(tls, db, pWInfo);
        }
        r = (uintptr)0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

[global::go.golib.GoInlineLayout(Size = 16, Align = 8), StructLayout(LayoutKind.Explicit, Size = 16)] internal partial struct _sqlite3WhereCodeOneLoopStart_type /*dyn*/ {
    [FieldOffset(0)] public int32 FnIn;
    [FieldOffset(8)] public uintptr FaInLoop;
}

internal static TBitmask /*r2*/ _sqlite3WhereCodeOneLoopStart(ж<libc.TLS> tls, uintptr pParse, uintptr v, uintptr pWInfo, int32 iLevel, uintptr pLevel, TBitmask notReady) {
    TBitmask r2 = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        array<Tu8> aMoveOp = new(4);
        int32 addrBrk = default;
        int32 addrCont = default;
        int32 addrExplain = default;
        int32 addrNotFound = default;
        int32 addrNxt = default;
        int32 addrSeekScan = default;
        int32 bRev = default;
        int32 endEq = default;
        int32 iCache = default;
        int32 iCol = default;
        int32 iCol1 = default;
        int32 iCovCur = default;
        int32 iCur = default;
        int32 iFld = default;
        int32 iIdxCur = default;
        int32 iIn = default;
        int32 iLoop = default;
        int32 iLoopBody = default;
        int32 iNext = default;
        int32 iPk = default;
        int32 iPk1 = default;
        int32 iReg = default;
        int32 iReleaseReg = default;
        int32 iRetInit = default;
        int32 iRowidReg = default;
        int32 iSet = default;
        int32 iTab = default;
        int32 iTarget = default;
        int32 iTerm = default;
        int32 ii = default;
        int32 j = default;
        int32 jmp1 = default;
        int32 jmp11 = default;
        int32 k = default;
        int32 memEndValue = default;
        int32 nConstraint = default;
        int32 nConstraint1 = default;
        int32 nExtraReg = default;
        int32 nNotReady = default;
        int32 nPk = default;
        int32 nPk1 = default;
        int32 omitTable = default;
        int32 op = default;
        int32 op1 = default;
        int32 r = default;
        int32 r1 = default;
        int32 r11 = default;
        ref var rTemp = ref heap(new int32(), out var ᏑrTemp);
        int32 regBase = default;
        int32 regBignull = default;
        int32 regReturn = default;
        int32 regRowid = default;
        int32 regRowset = default;
        int32 regYield = default;
        int32 skipLikeAddr = default;
        int32 start = default;
        int32 startEq = default;
        int32 start_constraints = default;
        int32 testOp = default;
        int32 untestedTerms = default;
        int32 v1 = default;
        int32 v2 = default;
        Tu8 bSeekPastNull = default;
        Tu8 bStopAtNull = default;
        Tu8 t1 = default;
        Tu8 t2 = default;
        uintptr db = default;
        uintptr origSrc = default;
        uintptr pAlt = default;
        uintptr pAndExpr = default;
        uintptr pCompare = default;
        uintptr pCov = default;
        uintptr pDelete = default;
        uintptr pE = default;
        uintptr pE1 = default;
        uintptr pEnd = default;
        uintptr pExpr = default;
        uintptr pIdx = default;
        uintptr pLeft = default;
        uintptr pLoop = default;
        uintptr pOp = default;
        uintptr pOrExpr = default;
        uintptr pOrTab = default;
        uintptr pOrTerm = default;
        uintptr pOrWc = default;
        uintptr pPk = default;
        uintptr pPk1 = default;
        uintptr pPk2 = default;
        uintptr pPk3 = default;
        uintptr pRJ = default;
        uintptr pRJ1 = default;
        uintptr pRangeEnd = default;
        uintptr pRangeStart = default;
        uintptr pRight = default;
        uintptr pRight1 = default;
        uintptr pRight2 = default;
        uintptr pRight3 = default;
        uintptr pStart = default;
        uintptr pSubLoop = default;
        uintptr pSubWInfo = default;
        uintptr pSubq = default;
        uintptr pTab = default;
        uintptr pTab1 = default;
        uintptr pTabItem = default;
        uintptr pTerm = default;
        uintptr pWC = default;
        uintptr pX = default;
        uintptr pX1 = default;
        uintptr t = default;
        uintptr zEndAff = default;
        ref var zStartAff = ref heap(new uintptr(), out var ᏑzStartAff);
        uintptr v4 = default;
        uintptr v8 = default;
        Tu16 nBtm = default;
        Tu16 nEq = default;
        Tu16 nTop = default;
        uint32 v6 = default;
        bool v15 = default;
        TExpr _ᴛ1 = new();
        _ = aMoveOp;
        _ = addrBrk;
        _ = addrCont;
        _ = addrExplain;
        _ = addrNotFound;
        _ = addrNxt;
        _ = addrSeekScan;
        _ = bRev;
        _ = bSeekPastNull;
        _ = bStopAtNull;
        _ = db;
        _ = endEq;
        _ = iCache;
        _ = iCol;
        _ = iCol1;
        _ = iCovCur;
        _ = iCur;
        _ = iFld;
        _ = iIdxCur;
        _ = iIn;
        _ = iLoop;
        _ = iLoopBody;
        _ = iNext;
        _ = iPk;
        _ = iPk1;
        _ = iReg;
        _ = iReleaseReg;
        _ = iRetInit;
        _ = iRowidReg;
        _ = iSet;
        _ = iTab;
        _ = iTarget;
        _ = iTerm;
        _ = ii;
        _ = j;
        _ = jmp1;
        _ = jmp11;
        _ = k;
        _ = memEndValue;
        _ = nBtm;
        _ = nConstraint;
        _ = nConstraint1;
        _ = nEq;
        _ = nExtraReg;
        _ = nNotReady;
        _ = nPk;
        _ = nPk1;
        _ = nTop;
        _ = omitTable;
        _ = op;
        _ = op1;
        _ = origSrc;
        _ = pAlt;
        _ = pAndExpr;
        _ = pCompare;
        _ = pCov;
        _ = pDelete;
        _ = pE;
        _ = pE1;
        _ = pEnd;
        _ = pExpr;
        _ = pIdx;
        _ = pLeft;
        _ = pLoop;
        _ = pOp;
        _ = pOrExpr;
        _ = pOrTab;
        _ = pOrTerm;
        _ = pOrWc;
        _ = pPk;
        _ = pPk1;
        _ = pPk2;
        _ = pPk3;
        _ = pRJ;
        _ = pRJ1;
        _ = pRangeEnd;
        _ = pRangeStart;
        _ = pRight;
        _ = pRight1;
        _ = pRight2;
        _ = pRight3;
        _ = pStart;
        _ = pSubLoop;
        _ = pSubWInfo;
        _ = pSubq;
        _ = pTab;
        _ = pTab1;
        _ = pTabItem;
        _ = pTerm;
        _ = pWC;
        _ = pX;
        _ = pX1;
        _ = r;
        _ = r1;
        _ = r11;
        _ = rTemp;
        _ = regBase;
        _ = regBignull;
        _ = regReturn;
        _ = regRowid;
        _ = regRowset;
        _ = regYield;
        _ = skipLikeAddr;
        _ = start;
        _ = startEq;
        _ = start_constraints;
        _ = t;
        _ = t1;
        _ = t2;
        _ = testOp;
        _ = untestedTerms;
        _ = zEndAff;
        _ = zStartAff;
        _ = v1;
        _ = v15;
        _ = v2;
        _ = v4;
        _ = v6;
        _ = v8;
        iRowidReg = 0;
        iReleaseReg = 0;
        pIdx = (uintptr)0;
        pWC = pWInfo + 104;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
        pTabItem = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
        iCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FnotReady = (TBitmask)(notReady & ~_sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, iCur));
        bRev = libc.Int32FromUint64((TBitmask)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask >> (int)(iLevel) & (uint64)1));
        v1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrNxt = v1;
        addrBrk = v1;
        v1 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrCont = v1;
        addrCont = v1;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom) > 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(pTabItem))).Value.Ffg.Fjointype) & (int32)JT_LEFT) != 0) {
            v4 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin = v1;
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin);
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabItem + 24 + 4)) & 0x40) >> 6) != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_NullRow, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor, (int32)1);
            }
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabItem + 24 + 4)) & 0x40) >> 6) != 0) {
            pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTabItem + 72));
            regYield = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn;
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_InitCoroutine, regYield, 0, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub);
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Yield, regYield, addrBrk);
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Goto;
        } else {
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) != (uint32)0) {
                nConstraint = libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnLTerm);
                iReg = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nConstraint + (int32)2);
                addrNotFound = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk;
                j = 0;
                for (;;) {
                    if (!(j < nConstraint)) {
                        break;
                    }
                    iTarget = iReg + j + (int32)2;
                    pTerm = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8));
                    if (pTerm == (uintptr)0) {
                        goto _5;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_IN) != 0) {
                        if (j <= (int32)31) {
                            v6 = libc.Uint32FromInt32(1).Lsh((int64)(j));
                        } else {
                            v6 = (uint32)0;
                        }
                        if ((v6 & (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FmHandleIn) != 0) {
                            v4 = pParse + 56;
                            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                            iTab = v1;
                            v8 = pParse + 60;
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8) + 1;
                            v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8);
                            iCache = v2;
                            _sqlite3CodeRhsOfIN(tls, pParse, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr, iTab, 0);
                            _sqlite3VdbeAddOp3(tls, v, (int32)OP_VInitIn, iTab, iTarget, iCache);
                        } else {
                            _codeEqualityTerm(tls, pParse, pTerm, pLevel, j, bRev, iTarget);
                            addrNotFound = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrNxt;
                        }
                    } else {
                        pRight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpRight;
                        _codeExprOrVector(tls, pParse, pRight, iTarget, (int32)1);
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeMatchOp) == (int32)SQLITE_INDEX_CONSTRAINT_OFFSET && (int32)((Tu32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pLoop + 24 + 4)) & 0x2) >> 1)) != 0) {
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect)).Value.FiOffset);
                        }
                    }
                    goto _5;
_5:;
                    j = j + 1;
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FidxNum, iReg);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, nConstraint, iReg + (int32)1);
                if ((int32)((Tu32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pLoop + 24 + 4)) & 0x1) >> 0)) != 0) {
                    v1 = -(int32)7;
                } else {
                    v1 = -(int32)1;
                }
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_VFilter, iCur, addrNotFound, iReg, (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FidxStr, v1);
                libc.SetBitFieldPtr8Uint32(pLoop + 24 + 4, libc.Uint32FromInt32(0), 0, 0x1);
                if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                    (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FidxStr = (uintptr)0;
                }
                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1 = iCur;
                if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass != 0) {
                    v1 = (int32)OP_Noop;
                } else {
                    v1 = (int32)OP_VNext;
                }
                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = libc.Uint8FromInt32(v1);
                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                j = 0;
                for (;;) {
                    if (!(j < nConstraint)) {
                        break;
                    }
                    pTerm = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8));
                    if (j < (int32)16 && (libc.Int32FromUint16((@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FomitMask).Rsh((int64)(j)) & (int32)1) != 0) {
                        _disableTerm(ref (tls).DerefOrNull(), pLevel, pTerm);
                        goto _13;
                    }
                    v15 = (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_IN) != 0; if (v15) {
                        if (j <= (int32)31) {
                            v6 = libc.Uint32FromInt32(1).Lsh((int64)(j));
                        } else {
                            v6 = (uint32)0;
                        }
                    }
                    if (v15 && (v6 & (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FmHandleIn) == (uint32)0 && !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                        iIn = 0;
                        for (;;) {
                            if (!(iIn < (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FnIn)) {
                                break;
                            }
                            pOp = _sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TInLoop>(__ccgo_ptr((@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FaInLoop + (uintptr)iIn * 20))).Value.FaddrInTop);
                            if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Column && (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 == iReg + j + (int32)2 || libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Rowid && (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2 == iReg + j + (int32)2) {
                                _sqlite3VdbeAddOp3(tls, v, libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode), (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1, (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2, (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3);
                                break;
                            }
                            goto _16;
_16:;
                            iIn = iIn + 1;
                        }
                        pCompare = _sqlite3PExpr(tls, pParse, (int32)TK_EQ, (uintptr)0, (uintptr)0);
                        if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                            iFld = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FiField;
                            pLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpLeft;
                            if (iFld > 0) {
                                (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCompare)).Value.FpLeft = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pLeft + 32)) + 8 + (uintptr)(iFld - (int32)1) * 32))).Value.FpExpr;
                            } else {
                                (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCompare)).Value.FpLeft = pLeft;
                            }
                            v4 = _sqlite3Expr(tls, db, (int32)TK_REGISTER, (uintptr)0);
                            pRight1 = v4;
                            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCompare)).Value.FpRight = v4;
                            if (pRight1 != 0) {
                                (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight1)).Value.FiTable = iReg + j + (int32)2;
                                _sqlite3ExprIfFalse(tls, pParse, pCompare, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrCont, (int32)SQLITE_JUMPIFNULL);
                            }
                            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCompare)).Value.FpLeft = (uintptr)0;
                        }
                        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pCompare);
                    }
                    goto _13;
_13:;
                    j = j + 1;
                }
            } else {
                if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IPK) != (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_IN) | libc.Int32FromInt32(WHERE_COLUMN_EQ))) != (uint32)0) {
                    pTerm = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm));
                    v4 = pParse + 60;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                    iReleaseReg = v1;
                    iRowidReg = _codeEqualityTerm(tls, pParse, pTerm, pLevel, 0, bRev, iReleaseReg);
                    if (iRowidReg != iReleaseReg) {
                        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, iReleaseReg);
                    }
                    addrNxt = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrNxt;
                    if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter != 0) {
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_MustBeInt, iRowidReg, addrNxt);
                        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Filter, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter, addrNxt, iRowidReg, (int32)1);
                        _filterPullDown(tls, pParse, pWInfo, iLevel, addrNxt, notReady);
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_SeekRowid, iCur, addrNxt, iRowidReg);
                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Noop;
                } else {
                    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IPK) != (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_COLUMN_RANGE) != (uint32)0) {
                        testOp = (int32)OP_Noop;
                        memEndValue = 0;
                        j = 0;
                        v4 = libc.UintptrFromInt32(0);
                        pEnd = v4;
                        pStart = v4;
                        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_BTM_LIMIT) != 0) {
                            v1 = j;
                            j = j + 1;
                            pStart = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)v1 * 8));
                        }
                        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_TOP_LIMIT) != 0) {
                            v1 = j;
                            j = j + 1;
                            pEnd = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)v1 * 8));
                        }
                        if (bRev != 0) {
                            pTerm = pStart;
                            pStart = pEnd;
                            pEnd = pTerm;
                        }
                        if (pStart != 0) {
                            aMoveOp.Assign(new array<Tu8>(4){
                                [0] = (uint8)OP_SeekGT,
                                [1] = (uint8)OP_SeekLE,
                                [2] = (uint8)OP_SeekLT,
                                [3] = (uint8)OP_SeekGE
                            });
                            pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pStart)).Value.FpExpr;
                            if (_sqlite3ExprIsVector(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FpRight) != 0) {
                                v1 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                                rTemp = v1;
                                r11 = v1;
                                _codeExprOrVector(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FpRight, r11, (int32)1);
                                op = libc.Int32FromUint8(aMoveOp[((libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) - (int32)TK_GT - (int32)1) & (int32)0x3 | (int32)0x1)]);
                            } else {
                                r11 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FpRight, @unsafe.Pointer.FromPinnedBox(ᏑrTemp));
                                _disableTerm(ref (tls).DerefOrNull(), pLevel, pStart);
                                op = libc.Int32FromUint8(aMoveOp[libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) - (int32)TK_GT]);
                            }
                            _sqlite3VdbeAddOp3(tls, v, op, iCur, addrBrk, r11);
                            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, rTemp);
                        } else {
                            if (bRev != 0) {
                                v1 = (int32)OP_Last;
                            } else {
                                v1 = (int32)OP_Rewind;
                            }
                            _sqlite3VdbeAddOp2(tls, v, v1, iCur, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt);
                        }
                        if (pEnd != 0) {
                            pX1 = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pEnd)).Value.FpExpr;
                            v4 = pParse + 60;
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                            memEndValue = v1;
                            _codeExprOrVector(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX1)).Value.FpRight, memEndValue, (int32)1);
                            if (0 == _sqlite3ExprIsVector(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX1)).Value.FpRight) && (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX1)).Value.Fop) == (int32)TK_LT || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX1)).Value.Fop) == (int32)TK_GT)) {
                                if (bRev != 0) {
                                    v1 = (int32)OP_Le;
                                } else {
                                    v1 = (int32)OP_Ge;
                                }
                                testOp = v1;
                            } else {
                                if (bRev != 0) {
                                    v1 = (int32)OP_Lt;
                                } else {
                                    v1 = (int32)OP_Gt;
                                }
                                testOp = v1;
                            }
                            if (0 == _sqlite3ExprIsVector(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX1)).Value.FpRight)) {
                                _disableTerm(ref (tls).DerefOrNull(), pLevel, pEnd);
                            }
                        }
                        start = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                        if (bRev != 0) {
                            v1 = (int32)OP_Prev;
                        } else {
                            v1 = (int32)OP_Next;
                        }
                        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = libc.Uint8FromInt32(v1);
                        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1 = iCur;
                        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 = start;
                        if (testOp != (int32)OP_Noop) {
                            v4 = pParse + 60;
                            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                            iRowidReg = v1;
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iCur, iRowidReg);
                            _sqlite3VdbeAddOp3(tls, v, testOp, memEndValue, addrBrk, iRowidReg);
                            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32(libc.Int32FromInt32(SQLITE_AFF_NUMERIC) | libc.Int32FromInt32(SQLITE_JUMPIFNULL)));
                        }
                    } else {
                        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_INDEXED) != 0) {
                            nEq = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq;
                            nBtm = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnBtm;
                            nTop = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnTop;
                            pRangeStart = (uintptr)0;
                            pRangeEnd = (uintptr)0;
                            nExtraReg = 0;
                            zEndAff = (uintptr)0;
                            bSeekPastNull = (uint8)0;
                            bStopAtNull = (uint8)0;
                            regBignull = 0;
                            addrSeekScan = 0;
                            pIdx = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
                            iIdxCur = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur;
                            j = libc.Int32FromUint16(nEq);
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_BTM_LIMIT) != 0) {
                                v1 = j;
                                j = j + 1;
                                pRangeStart = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)v1 * 8));
                                if (nExtraReg > libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnBtm)) {
                                    v1 = nExtraReg;
                                } else {
                                    v1 = libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnBtm);
                                }
                                nExtraReg = v1;
                            }
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_TOP_LIMIT) != 0) {
                                v1 = j;
                                j = j + 1;
                                pRangeEnd = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)v1 * 8));
                                if (nExtraReg > libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnTop)) {
                                    v1 = nExtraReg;
                                } else {
                                    v1 = libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnTop);
                                }
                                nExtraReg = v1;
                                if (pRangeStart == (uintptr)0) {
                                    j = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)nEq * 2)));
                                    if (j >= 0 && (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FaCol + (uintptr)j * 16 + 8)) & 0xf) >> 0) == 0 || j == -(int32)2) {
                                        bSeekPastNull = (uint8)1;
                                    }
                                }
                            }
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_TOP_LIMIT) | libc.Int32FromInt32(WHERE_BTM_LIMIT))) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_BIGNULL_SORT) != (uint32)0) {
                                nExtraReg = (int32)1;
                                bSeekPastNull = (uint8)1;
                                v4 = pParse + 60;
                                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                                v1 = v2;
                                regBignull = v1;
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregBignull = v1;
                                if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin != 0) {
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regBignull);
                                }
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBignull = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                            }
                            if (libc.Int32FromUint16(nEq) < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn) && bRev == libc.BoolInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSortOrder + (uintptr)nEq))) == SQLITE_SO_ASC)) {
                                t = pRangeEnd;
                                pRangeEnd = pRangeStart;
                                pRangeStart = t;
                                t1 = bSeekPastNull;
                                bSeekPastNull = bStopAtNull;
                                bStopAtNull = t1;
                                t2 = (uint8)nBtm;
                                nBtm = nTop;
                                nTop = (uint16)t2;
                            }
                            if (iLevel > 0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IN_SEEKSCAN) != (uint32)0) {
                                _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, iIdxCur);
                            }
                            regBase = _codeAllEqualityTerms(tls, pParse, pLevel, bRev, nExtraReg, @unsafe.Pointer.FromBox(ᏑzStartAff));
                            if (zStartAff != 0 && nTop != 0) {
                                zEndAff = _sqlite3DbStrDup(tls, db, zStartAff + (uintptr)nEq);
                            }
                            if (regBignull != 0) {
                                v1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBignull;
                            } else {
                                v1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrNxt;
                            }
                            addrNxt = v1;
                            startEq = libc.BoolInt32(!(pRangeStart != 0) || (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pRangeStart)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) != 0);
                            endEq = libc.BoolInt32(!(pRangeEnd != 0) || (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pRangeEnd)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) != 0);
                            start_constraints = libc.BoolInt32(pRangeStart != 0 || libc.Int32FromUint16(nEq) > 0);
                            nConstraint1 = libc.Int32FromUint16(nEq);
                            if (pRangeStart != 0) {
                                pRight2 = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pRangeStart)).Value.FpExpr)).Value.FpRight;
                                _codeExprOrVector(tls, pParse, pRight2, regBase + libc.Int32FromUint16(nEq), libc.Int32FromUint16(nBtm));
                                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pRangeStart)).Value.FwtFlags) & (int32)TERM_VNULL) == 0 && _sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), pRight2) != 0) {
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, regBase + libc.Int32FromUint16(nEq), addrNxt);
                                }
                                if (zStartAff != 0) {
                                    _updateRangeAffinityStr(tls, pRight2, libc.Int32FromUint16(nBtm), zStartAff + (uintptr)nEq);
                                }
                                nConstraint1 = nConstraint1 + libc.Int32FromUint16(nBtm);
                                if (_sqlite3ExprIsVector(ref (tls).DerefOrNull(), pRight2) == 0) {
                                    _disableTerm(ref (tls).DerefOrNull(), pLevel, pRangeStart);
                                } else {
                                    startEq = (int32)1;
                                }
                                bSeekPastNull = (uint8)0;
                            } else {
                                if (bSeekPastNull != 0) {
                                    startEq = 0;
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regBase + libc.Int32FromUint16(nEq));
                                    start_constraints = (int32)1;
                                    nConstraint1 = nConstraint1 + 1;
                                } else {
                                    if (regBignull != 0) {
                                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regBase + libc.Int32FromUint16(nEq));
                                        start_constraints = (int32)1;
                                        nConstraint1 = nConstraint1 + 1;
                                    }
                                }
                            }
                            _codeApplyAffinity(tls, pParse, regBase, nConstraint1 - libc.Int32FromUint8(bSeekPastNull), zStartAff);
                            if (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnSkip) > 0 && nConstraint1 == libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnSkip)) {
                            } else {
                                if (regBignull != 0) {
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, regBignull);
                                }
                                if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter != 0) {
                                    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Filter, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregFilter, addrNxt, regBase, libc.Int32FromUint16(nEq));
                                    _filterPullDown(tls, pParse, pWInfo, iLevel, addrNxt, notReady);
                                }
                                op1 = libc.Int32FromUint8(_aStartOp[(start_constraints << (int)((int32)2)) + (startEq << (int)((int32)1)) + bRev]);
                                if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IN_SEEKSCAN) != (uint32)0 && op1 == (int32)OP_SeekGE) {
                                    addrSeekScan = _sqlite3VdbeAddOp1(tls, v, (int32)OP_SeekScan, ((int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiRowLogEst))) + (int32)9) / (int32)10);
                                    if (pRangeStart != 0 || pRangeEnd != 0) {
                                        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)1);
                                        _sqlite3VdbeChangeP2(ref (tls).DerefOrNull(), v, addrSeekScan, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)1);
                                        addrSeekScan = 0;
                                    }
                                }
                                _sqlite3VdbeAddOp4Int(tls, v, op1, iIdxCur, addrNxt, regBase, nConstraint1);
                                if (regBignull != 0) {
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2);
                                    op1 = libc.Int32FromUint8(_aStartOp[libc.BoolInt32(nConstraint1 > (int32)1) * (int32)4 + (int32)2 + bRev]);
                                    _sqlite3VdbeAddOp4Int(tls, v, op1, iIdxCur, addrNxt, regBase, nConstraint1 - startEq);
                                }
                            }
                            nConstraint1 = libc.Int32FromUint16(nEq);
                            if (pRangeEnd != 0) {
                                pRight3 = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pRangeEnd)).Value.FpExpr)).Value.FpRight;
                                _codeExprOrVector(tls, pParse, pRight3, regBase + libc.Int32FromUint16(nEq), libc.Int32FromUint16(nTop));
                                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pRangeEnd)).Value.FwtFlags) & (int32)TERM_VNULL) == 0 && _sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), pRight3) != 0) {
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, regBase + libc.Int32FromUint16(nEq), addrNxt);
                                }
                                if (zEndAff != 0) {
                                    _updateRangeAffinityStr(tls, pRight3, libc.Int32FromUint16(nTop), zEndAff);
                                    _codeApplyAffinity(tls, pParse, regBase + libc.Int32FromUint16(nEq), libc.Int32FromUint16(nTop), zEndAff);
                                } else {
                                }
                                nConstraint1 = nConstraint1 + libc.Int32FromUint16(nTop);
                                if (_sqlite3ExprIsVector(ref (tls).DerefOrNull(), pRight3) == 0) {
                                    _disableTerm(ref (tls).DerefOrNull(), pLevel, pRangeEnd);
                                } else {
                                    endEq = (int32)1;
                                }
                            } else {
                                if (bStopAtNull != 0) {
                                    if (regBignull == 0) {
                                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regBase + libc.Int32FromUint16(nEq));
                                        endEq = 0;
                                    }
                                    nConstraint1 = nConstraint1 + 1;
                                }
                            }
                            if (zStartAff != 0) {
                                _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, zStartAff);
                            }
                            if (zEndAff != 0) {
                                _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, zEndAff);
                            }
                            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                            if (nConstraint1 != 0) {
                                if (regBignull != 0) {
                                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfNot, regBignull, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)3);
                                }
                                op1 = libc.Int32FromUint8(_aEndOp[bRev * (int32)2 + endEq]);
                                _sqlite3VdbeAddOp4Int(tls, v, op1, iIdxCur, addrNxt, regBase, nConstraint1);
                                if (addrSeekScan != 0) {
                                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrSeekScan);
                                }
                            }
                            if (regBignull != 0) {
                                _sqlite3VdbeAddOp2(tls, v, (int32)OP_If, regBignull, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2);
                                op1 = libc.Int32FromUint8(_aEndOp[bRev * (int32)2 + libc.Int32FromUint8(bSeekPastNull)]);
                                _sqlite3VdbeAddOp4Int(tls, v, op1, iIdxCur, addrNxt, regBase, nConstraint1 + libc.Int32FromUint8(bSeekPastNull));
                            }
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IN_EARLYOUT) != (uint32)0) {
                                _sqlite3VdbeAddOp3(tls, v, (int32)OP_SeekHit, iIdxCur, libc.Int32FromUint16(nEq), libc.Int32FromUint16(nEq));
                            }
                            omitTable = libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IDX_ONLY) != (uint32)0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (libc.Int32FromInt32(WHERE_OR_SUBCLAUSE) | libc.Int32FromInt32(WHERE_RIGHT_JOIN))) == 0);
                            if (omitTable != 0) {
                            } else {
                                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                                    _codeDeferredSeek(tls, pWInfo, pIdx, iCur, iIdxCur);
                                } else {
                                    if (iCur != iIdxCur) {
                                        pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable);
                                        iRowidReg = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol));
                                        j = 0;
                                        for (;;) {
                                            if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol))) {
                                                break;
                                            }
                                            k = _sqlite3TableColumnToIndex(ref (tls).DerefOrNull(), pIdx, (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)j * 2))));
                                            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iIdxCur, k, iRowidReg + j);
                                            goto _40;
_40:;
                                            j = j + 1;
                                        }
                                        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iCur, addrCont, iRowidReg, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol));
                                    }
                                }
                            }
                            if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin == 0) {
                                if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != 0 && (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ == (uintptr)0) {
                                    _whereApplyPartialIndexConstraints(tls, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere, iCur, pWC);
                                }
                            } else {
                            }
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_ONEROW) != 0 || (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FnIn != 0 && regBignull == 0 && _whereLoopIsOneRow(ref (tls).DerefOrNull(), pLoop) != 0) {
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Noop;
                            } else {
                                if (bRev != 0) {
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Prev;
                                } else {
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Next;
                                }
                            }
                            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1 = iIdxCur;
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_UNQ_WANTED) != (uint32)0) {
                                v1 = (int32)1;
                            } else {
                                v1 = 0;
                            }
                            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp3 = libc.Uint8FromInt32(v1);
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_CONSTRAINT) == (uint32)0) {
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp5 = (uint8)SQLITE_STMTSTATUS_FULLSCAN_STEP;
                            } else {
                            }
                            if (omitTable != 0) {
                                pIdx = (uintptr)0;
                            }
                        } else {
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_MULTI_OR) != 0) {
                                pCov = (uintptr)0;
                                v4 = pParse + 56;
                                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                                iCovCur = v1;
                                v8 = pParse + 60;
                                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8) + 1;
                                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v8);
                                regReturn = v2;
                                regRowset = 0;
                                regRowid = 0;
                                iLoopBody = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                                untestedTerms = 0;
                                pAndExpr = (uintptr)0;
                                pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FpSTab;
                                pTerm = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm));
                                pOrWc = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTerm + 32));
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Return;
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1 = regReturn;
                                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) > (int32)1 || (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabItem + 24 + 4)) & 0x40000) >> 18) != 0) {
                                    nNotReady = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - iLevel - (int32)1;
                                    pOrTab = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)(libc.UintptrFromInt32(0) + 8) + libc.Uint64FromInt32(nNotReady + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(80)));
                                    if (pOrTab == (uintptr)0) {
                                        r2 = notReady; goto ᒐdone;
                                    }
                                    (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pOrTab)).Value.FnAlloc = (uint32)libc.Uint8FromInt32(nNotReady + libc.Int32FromInt32(1));
                                    (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pOrTab)).Value.FnSrc = libc.Int32FromUint32((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pOrTab)).Value.FnAlloc);
                                    libc.Xmemcpy(tls, pOrTab + 8, pTabItem, (uint64)80);
                                    origSrc = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8;
                                    k = (int32)1;
                                    for (;;) {
                                        if (!(k <= nNotReady)) {
                                            break;
                                        }
                                        libc.Xmemcpy(tls, pOrTab + 8 + (uintptr)k * 80, origSrc + (uintptr)(@unsafe.Pointer.To<TWhereLevel>(__ccgo_ptr(pLevel + (uintptr)k * 112))).Value.FiFrom * 80, (uint64)80);
                                        goto _46;
_46:;
                                        k = k + 1;
                                    }
                                    libc.SetBitFieldPtr32Uint32(pOrTab + 8 + 24 + 4, libc.Uint32FromInt32(0), 18, 0x40000);
                                } else {
                                    pOrTab = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList;
                                }
                                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_DUPLICATES_OK) == 0) {
                                    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                                        v4 = pParse + 60;
                                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                                        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                                        regRowset = v1;
                                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regRowset);
                                    } else {
                                        pPk1 = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
                                        v4 = pParse + 56;
                                        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                                        regRowset = v1;
                                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, regRowset, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk1)).Value.FnKeyCol));
                                        _sqlite3VdbeSetP4KeyInfo(tls, pParse, pPk1);
                                    }
                                    v4 = pParse + 60;
                                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
                                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
                                    regRowid = v1;
                                }
                                iRetInit = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regReturn);
                                if ((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm > (int32)1) {
                                    iTerm = 0;
                                    for (;;) {
                                        if (!(iTerm < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm)) {
                                            break;
                                        }
                                        pExpr = (@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iTerm * 56))).Value.FpExpr;
                                        if ((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iTerm * 56 == pTerm) {
                                            goto _53;
                                        }
                                        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iTerm * 56))).Value.FwtFlags) & (libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_CODED) | libc.Int32FromInt32(TERM_SLICE))) != 0) {
                                            goto _53;
                                        }
                                        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>(__ccgo_ptr((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)iTerm * 56))).Value.FeOperator) & (int32)WO_ALL) == 0) {
                                            goto _53;
                                        }
                                        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0) {
                                            goto _53;
                                        }
                                        if (_whereExprIsReady(ref (tls).DerefOrNull(), pWInfo, pLevel, pTabItem, pExpr) == 0) {
                                            goto _53;
                                        }
                                        pExpr = _sqlite3ExprDup(tls, db, pExpr, 0);
                                        pAndExpr = _sqlite3ExprAnd(tls, pParse, pAndExpr, pExpr);
                                        goto _53;
_53:;
                                        iTerm = iTerm + 1;
                                    }
                                    if (pAndExpr != 0) {
                                        pAndExpr = _sqlite3PExpr(tls, pParse, libc.Int32FromInt32(TK_AND) | libc.Int32FromInt32(0x10000), (uintptr)0, pAndExpr);
                                    }
                                }
                                _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 24045, 0);
                                ii = 0;
                                for (;;) {
                                    if (!(ii < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.FnTerm)) {
                                        break;
                                    }
                                    pOrTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa + (uintptr)ii * 56;
                                    if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor == iCur || (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FeOperator) & (int32)WO_AND) != 0) {
                                        pOrExpr = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr;
                                        jmp1 = 0;
                                        v4 = _sqlite3ExprDup(tls, db, pOrExpr, 0);
                                        pOrExpr = v4;
                                        pDelete = v4;
                                        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                                            _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pDelete);
                                            goto _54;
                                        }
                                        if (pAndExpr != 0) {
                                            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pAndExpr)).Value.FpLeft = pOrExpr;
                                            pOrExpr = pAndExpr;
                                        }
                                        _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 24060, libc.VaList(bp + 80, ii + (int32)1));
                                        pSubWInfo = _sqlite3WhereBegin(tls, pParse, pOrTab, pOrExpr, (uintptr)0, (uintptr)0, (uintptr)0, (uint16)WHERE_OR_SUBCLAUSE, iCovCur);
                                        if (pSubWInfo != 0) {
                                            addrExplain = _sqlite3WhereExplainOneScan(tls, pParse, pOrTab, pSubWInfo + 856, (uint16)0);
                                            _ = addrExplain;
                                            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_DUPLICATES_OK) == 0) {
                                                if (ii == (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.FnTerm - (int32)1) {
                                                    v1 = -(int32)1;
                                                } else {
                                                    v1 = ii;
                                                }
                                                iSet = v1;
                                                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                                                    _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iCur, -(int32)1, regRowid);
                                                    jmp1 = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_RowSetTest, regRowset, 0, regRowid, iSet);
                                                } else {
                                                    pPk2 = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
                                                    nPk = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk2)).Value.FnKeyCol);
                                                    r = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nPk);
                                                    iPk = 0;
                                                    for (;;) {
                                                        if (!(iPk < nPk)) {
                                                            break;
                                                        }
                                                        iCol = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk2)).Value.FaiColumn + (uintptr)iPk * 2)));
                                                        _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iCur, iCol, r + iPk);
                                                        goto _57;
_57:;
                                                        iPk = iPk + 1;
                                                    }
                                                    if (iSet != 0) {
                                                        jmp1 = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Found, regRowset, 0, r, nPk);
                                                    }
                                                    if (iSet >= 0) {
                                                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, r, nPk, regRowid);
                                                        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, regRowset, regRowid, r, nPk);
                                                        if (iSet != 0) {
                                                            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_USESEEKRESULT);
                                                        }
                                                    }
                                                    _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, r, nPk);
                                                }
                                            }
                                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, regReturn, iLoopBody);
                                            if (jmp1 != 0) {
                                                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, jmp1);
                                            }
                                            if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pSubWInfo + 68)) & 0x2) >> 1) != 0) {
                                                untestedTerms = (int32)1;
                                            }
                                            pSubLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pSubWInfo + 856))).Value.FpWLoop;
                                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pSubLoop)).Value.FwsFlags & (uint32)WHERE_INDEXED) != (uint32)0 && (ii == 0 || (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pSubLoop + 24))).Value.FpIndex == pCov) && ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 || !((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pSubLoop + 24))).Value.FpIndex + 100)) & 0x3) >> 0) == libc.Int32FromInt32(SQLITE_IDXTYPE_PRIMARYKEY)))) {
                                                pCov = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pSubLoop + 24))).Value.FpIndex;
                                            } else {
                                                pCov = (uintptr)0;
                                            }
                                            if (_sqlite3WhereUsesDeferredSeek(ref (tls).DerefOrNull(), pSubWInfo) != 0) {
                                                libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 0, 0x1);
                                            }
                                            _sqlite3WhereEnd(tls, pSubWInfo);
                                            _sqlite3VdbeExplainPop(ref (tls).DerefOrNull(), pParse);
                                        }
                                        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pDelete);
                                    }
                                    goto _54;
_54:;
                                    ii = ii + 1;
                                }
                                _sqlite3VdbeExplainPop(ref (tls).DerefOrNull(), pParse);
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel).of(TWhereLevel.ᏑFu).Reinterpret<TWhereLevelᴛ1_Fu, uintptr>()).Value = pCov;
                                if (pCov != 0) {
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur = iCovCur;
                                }
                                if (pAndExpr != 0) {
                                    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pAndExpr)).Value.FpLeft = (uintptr)0;
                                    _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pAndExpr);
                                }
                                _sqlite3VdbeChangeP1(ref (tls).DerefOrNull(), v, iRetInit, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v));
                                _sqlite3VdbeGoto(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk);
                                _sqlite3VdbeResolveLabel(tls, v, iLoopBody);
                                (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                                if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList != pOrTab) {
                                    _sqlite3DbFreeNN(tls, (@unsafe.Pointer)db, pOrTab);
                                }
                                if (!(untestedTerms != 0)) {
                                    _disableTerm(ref (tls).DerefOrNull(), pLevel, pTerm);
                                }
                            } else {
                                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabItem + 24 + 4)) & 0x80) >> 7) != 0) {
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = (uint8)OP_Noop;
                                } else {
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop = _aStep[bRev];
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1 = iCur;
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 = (int32)1 + _sqlite3VdbeAddOp2(tls, v, libc.Int32FromUint8(_aStart[bRev]), iCur, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrHalt);
                                    (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp5 = (uint8)SQLITE_STMTSTATUS_FULLSCAN_STEP;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (pIdx != 0) {
            v1 = (int32)1;
        } else {
            v1 = (int32)2;
        }
        iLoop = v1;
        for (var cond = true; cond; cond = iLoop > 0) {
            iNext = 0;
            pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
            j = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm;
            for (;;) {
                if (!(j > 0)) {
                    break;
                }
                skipLikeAddr = 0;
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_CODED))) != 0) {
                    goto _59;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll & (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FnotReady) != (uint64)0) {
                    libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 1, 0x2);
                    goto _59;
                }
                pE = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
                if (_whereExprIsReady(ref (tls).DerefOrNull(), pWInfo, pLevel, pTabItem, pE) == 0) {
                    goto _59;
                }
                if (iLoop == (int32)1 && !(_sqlite3ExprCoveredByIndex(tls, pE, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, pIdx) != 0)) {
                    iNext = (int32)2;
                    goto _59;
                }
                if (iLoop < (int32)3 && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_VARSELECT) != 0) {
                    if (iNext == 0) {
                        iNext = (int32)3;
                    }
                    goto _59;
                }
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_LIKECOND) != 0) {
                    goto _59;
                }
                _sqlite3ExprIfFalse(tls, pParse, pE, addrCont, (int32)SQLITE_JUMPIFNULL);
                if (skipLikeAddr != 0) {
                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, skipLikeAddr);
                }
                v4 = pTerm + 18;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)) | libc.Int32FromInt32(TERM_CODED));
                goto _59;
_59:;
                j = j - 1;
                pTerm += 56;
            }
            iLoop = iNext;
        }
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
        j = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnBase;
        for (;;) {
            if (!(j > 0)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_CODED))) != 0) {
                goto _61;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS))) == 0) {
                goto _61;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_EQUIV) == 0) {
                goto _61;
            }
            if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FleftCursor != iCur) {
                goto _61;
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0) {
                goto _61;
            }
            pE1 = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
            pAlt = _sqlite3WhereFindTerm(tls, pWC, iCur, (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn, notReady, libc.Uint32FromInt32(libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IN) | libc.Int32FromInt32(WO_IS)), (uintptr)0);
            if (pAlt == (uintptr)0) {
                goto _61;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAlt)).Value.FwtFlags) & (int32)TERM_CODED) != 0) {
                goto _61;
            }
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAlt)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Collate))) != (uint32)0) {
                goto _61;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAlt)).Value.FeOperator) & (int32)WO_IN) != 0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAlt)).Value.FpExpr)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0 && (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAlt)).Value.FpExpr + 32))))).Value.FpEList)).Value.FnExpr > (int32)1) {
                goto _61;
            }
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<TExpr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAlt)).Value.FpExpr))).Value;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp))).Value.FpLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE1)).Value.FpLeft;
            _sqlite3ExprIfFalse(tls, pParse, bp, addrCont, (int32)SQLITE_JUMPIFNULL);
            v4 = pAlt + 18;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)) | libc.Int32FromInt32(TERM_CODED));
            goto _61;
_61:;
            j = j - 1;
            pTerm += 56;
        }
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ != 0) {
            jmp11 = 0;
            pRJ = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ;
            pTab1 = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80))).Value.FpSTab;
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab1)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                r1 = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, (int32)2);
                _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab1, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, -(int32)1, r1 + (int32)1);
                nPk1 = (int32)1;
            } else {
                pPk3 = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab1);
                nPk1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk3)).Value.FnKeyCol);
                r1 = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nPk1 + (int32)1);
                iPk1 = 0;
                for (;;) {
                    if (!(iPk1 < nPk1)) {
                        break;
                    }
                    iCol1 = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk3)).Value.FaiColumn + (uintptr)iPk1 * 2)));
                    _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab1, iCur, iCol1, r1 + (int32)1 + iPk1);
                    goto _63;
_63:;
                    iPk1 = iPk1 + 1;
                }
            }
            jmp11 = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Found, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch, 0, r1 + (int32)1, nPk1);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, r1 + (int32)1, nPk1, r1);
            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch, r1, r1 + (int32)1, nPk1);
            if ((@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom != 0) {
                _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_FilterAdd, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom, 0, r1 + (int32)1, nPk1);
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_USESEEKRESULT);
            }
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, jmp11);
            _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, r1, nPk1 + (int32)1);
        }
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin != 0) {
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrFirst = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin);
            if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ == (uintptr)0) {
                goto code_outer_join_constraints;
            }
        }
        if (!((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ != 0)) {
            goto _64;
        }
        pRJ1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ;
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_BeginSubrtn, 0, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ1)).Value.FregReturn);
        (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ1)).Value.FaddrSubrtn = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn = (Tu8)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn + 1);
        goto code_outer_join_constraints;
code_outer_join_constraints:;
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
        j = libc.Int32FromInt32(0);
        for (;;) {
            if (!(j < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnBase)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_CODED))) != 0) {
                goto _65;
            }
            if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll & (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FnotReady) != (uint64)0) {
                goto _65;
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
                goto _65;
            }
            _sqlite3ExprIfFalse(tls, pParse, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr, addrCont, (int32)SQLITE_JUMPIFNULL);
            v4 = pTerm + 18;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)) | libc.Int32FromInt32(TERM_CODED));
            goto _65;
_65:;
            j = j + 1;
            pTerm += 56;
        }
_64:;
        r2 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FnotReady;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r2;
}

internal static void _sqlite3WhereEnd(ж<libc.TLS> tls, uintptr pWInfo) {
    int32 addr = default;
    int32 addrIfNull = default;
    int32 addrSeek = default;
    int32 bEarlyOut = default;
    int32 i = default;
    int32 iDb = default;
    int32 iEnd = default;
    int32 j = default;
    int32 j1 = default;
    int32 k = default;
    int32 last = default;
    int32 m = default;
    int32 m1 = default;
    int32 n = default;
    int32 n1 = default;
    int32 nRJ = default;
    int32 op = default;
    int32 r1 = default;
    int32 ws = default;
    int32 x = default;
    int32 v4 = default;
    uintptr db = default;
    uintptr p = default;
    uintptr pIdx = default;
    uintptr pIdx1 = default;
    uintptr pIn = default;
    uintptr pIn1 = default;
    uintptr pIx = default;
    uintptr pLastOp = default;
    uintptr pLevel = default;
    uintptr pLoop = default;
    uintptr pOp = default;
    uintptr pParse = default;
    uintptr pPk = default;
    uintptr pRJ = default;
    uintptr pSrc = default;
    uintptr pTab = default;
    uintptr pTabItem = default;
    uintptr pTabList = default;
    uintptr v = default;
    uintptr v2 = default;
    bool v3 = default;
    bool v5 = default;
    _ = addr;
    _ = addrIfNull;
    _ = addrSeek;
    _ = bEarlyOut;
    _ = db;
    _ = i;
    _ = iDb;
    _ = iEnd;
    _ = j;
    _ = j1;
    _ = k;
    _ = last;
    _ = m;
    _ = m1;
    _ = n;
    _ = n1;
    _ = nRJ;
    _ = op;
    _ = p;
    _ = pIdx;
    _ = pIdx1;
    _ = pIn;
    _ = pIn1;
    _ = pIx;
    _ = pLastOp;
    _ = pLevel;
    _ = pLoop;
    _ = pOp;
    _ = pParse;
    _ = pPk;
    _ = pRJ;
    _ = pSrc;
    _ = pTab;
    _ = pTabItem;
    _ = pTabList;
    _ = r1;
    _ = v;
    _ = ws;
    _ = x;
    _ = v2;
    _ = v3;
    _ = v4;
    _ = v5;
    pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    pTabList = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    iEnd = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
    nRJ = 0;
    addrSeek = 0;
    i = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - (int32)1;
    for (;;) {
        if (!(i >= 0)) {
            break;
        }
        pLevel = pWInfo + 856 + (uintptr)i * 112;
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ != 0) {
            pRJ = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ;
            _sqlite3VdbeResolveLabel(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrCont);
            (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrCont = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FendSubrtn = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Return, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregReturn, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FaddrSubrtn, (int32)1);
            nRJ = nRJ + 1;
        }
        pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop) != (int32)OP_Noop) {
            v3 = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct) == (int32)WHERE_DISTINCT_ORDERED && i == libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - (int32)1 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_INDEXED) != (uint32)0; if (v3) {
                v2 = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
                pIdx = v2;
            }
            v5 = v3 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(v2 + 100)) & 0x80) >> 7) != 0; if (v5) {
                v4 = libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnDistinctCol);
                n = v4;
            }
            if (v5 && v4 > 0 && (int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiRowLogEst + (uintptr)n * 2))) >= (int32)36) {
                r1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                addrIfNull = 0;
                if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin != 0) {
                    addrIfNull = _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfNullRow, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, r1);
                }
                j = 0;
                for (;;) {
                    if (!(j < n)) {
                        break;
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, j, r1 + j);
                    goto _6;
_6:;
                    j = j + 1;
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += n + (int32)1;
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop) == (int32)OP_Prev) {
                    v4 = (int32)OP_SeekLT;
                } else {
                    v4 = (int32)OP_SeekGT;
                }
                op = v4;
                addrSeek = _sqlite3VdbeAddOp4Int(tls, v, op, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, 0, r1, n);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, (int32)1, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2);
                if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin != 0) {
                    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrIfNull);
                }
            }
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80 + 24 + 4)) & 0x40000) >> 18) != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk);
        }
        _sqlite3VdbeResolveLabel(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrCont);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop) != (int32)OP_Noop) {
            _sqlite3VdbeAddOp3(tls, v, libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop), (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2, libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp3));
            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp5);
            if (addrSeek != 0) {
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrSeek);
                addrSeek = 0;
            }
            if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregBignull != 0) {
                _sqlite3VdbeResolveLabel(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBignull);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_DecrJumpZero, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FregBignull, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp2 - (int32)1);
            }
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IN_ABLE) != (uint32)0 && (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FnIn > 0) {
            _sqlite3VdbeResolveLabel(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrNxt);
            j1 = (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FnIn;
            pIn = (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FaInLoop + (uintptr)(j1 - (int32)1) * 20;
            for (;;) {
                if (!(j1 > 0)) {
                    break;
                }
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FaddrInTop + (int32)1);
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FeEndLoopOp) != (int32)OP_Noop) {
                    if ((@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FnPrefix != 0) {
                        bEarlyOut = libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) == (uint32)0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IN_EARLYOUT) != (uint32)0);
                        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin != 0) {
                            _sqlite3VdbeAddOp3(tls, v, (int32)OP_IfNullRow, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FiCur, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2 + bEarlyOut, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FiBase);
                        }
                        if (bEarlyOut != 0) {
                            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IfNoHope, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FiBase, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FnPrefix);
                            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FaddrInTop + (int32)1);
                        }
                    }
                    _sqlite3VdbeAddOp2(tls, v, libc.Int32FromUint8((@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FeEndLoopOp), (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FiCur, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FaddrInTop);
                }
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn)).Value.FaddrInTop - (int32)1);
                goto _8;
_8:;
                j1 = j1 - 1;
                pIn -= 20;
            }
        }
        _sqlite3VdbeResolveLabel(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk);
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ != 0) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Return, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ)).Value.FregReturn, 0, (int32)1);
        }
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrSkip != 0) {
            _sqlite3VdbeGoto(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrSkip);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrSkip);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrSkip - (int32)2);
        }
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin != 0) {
            ws = libc.Int32FromUint32((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags);
            addr = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IfPos, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiLeftJoin);
            if ((ws & (int32)WHERE_IDX_ONLY) == 0) {
                pSrc = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
                if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40) >> 6) != 0) {
                    n1 = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 72))))).Value.FregResult;
                    m = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab)).Value.FnCol;
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, n1, n1 + m - (int32)1);
                }
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur);
            }
            if ((ws & (int32)WHERE_INDEXED) != 0 || (ws & (int32)WHERE_MULTI_OR) != 0 && ~@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel).of(TWhereLevel.ᏑFu).Reinterpret<TWhereLevelᴛ1_Fu, uintptr>() != 0) {
                if ((ws & (int32)WHERE_MULTI_OR) != 0) {
                    pIx = ~@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel).of(TWhereLevel.ᏑFu).Reinterpret<TWhereLevelᴛ1_Fu, uintptr>();
                    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIx)).Value.FpSchema);
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_ReopenIdx, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIx)).Value.Ftnum), iDb);
                    _sqlite3VdbeSetP4KeyInfo(tls, pParse, pIx);
                }
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur);
            }
            if ((ws & (int32)WHERE_IN_ABLE) != 0) {
                m1 = 0;
                for (;;) {
                    if (!(m1 < (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FnIn)) {
                        break;
                    }
                    pIn1 = (@unsafe.Pointer.To<_sqlite3WhereCodeOneLoopStart_type>((@unsafe.Pointer)(pLevel + 80))).Value.FaInLoop + (uintptr)m1 * 20;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn1)).Value.FeEndLoopOp) != (int32)OP_Noop) {
                        _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, (@unsafe.Pointer.To<TInLoop>((@unsafe.Pointer)pIn1)).Value.FiCur);
                    }
                    goto _9;
_9:;
                    m1 = m1 + 1;
                }
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fop) == (int32)OP_Return) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.Fp1, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrFirst);
            } else {
                _sqlite3VdbeGoto(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrFirst);
            }
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr);
        }
        goto _1;
_1:;
        i = i - 1;
    }
    i = 0;
    pLevel = pWInfo + 856;
    for (;;) {
        if (!(i < libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel))) {
            break;
        }
        pIdx1 = (uintptr)0;
        pTabItem = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
        pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FpSTab;
        pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ != 0) {
            _sqlite3WhereRightJoinLoop(tls, pWInfo, i, pLevel);
            goto _10;
        }
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pTabItem + 24 + 4)) & 0x40) >> 6) != 0) {
            _translateColumnToCopy(ref (tls).DerefOrNull(), pParse, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBody, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTabItem + 72))))).Value.FregResult, 0);
            goto _10;
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_INDEXED) | libc.Int32FromInt32(WHERE_IDX_ONLY))) != 0) {
            pIdx1 = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
        } else {
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_MULTI_OR) != 0) {
                pIdx1 = ~@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel).of(TWhereLevel.ᏑFu).Reinterpret<TWhereLevelᴛ1_Fu, uintptr>();
            }
        }
        if (pIdx1 != 0 && !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass) == ONEPASS_OFF || !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx1)).Value.FpTable)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
                last = iEnd;
            } else {
                last = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiEndWhere;
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx1 + 100)) & 0x800) >> 11) != 0) {
                p = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr;
                while (p != 0) {
                    if ((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur == (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur) {
                        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiDataCur = -(int32)1;
                        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur = -(int32)1;
                    }
                    p = (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext;
                }
            }
            k = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBody + (int32)1;
            pOp = _sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, k);
            pLastOp = pOp + (uintptr)(last - k) * 24;
            for (;;) {
                if ((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1 != (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur) {
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Column || libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Offset) {
                        x = (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2;
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Offset) {
                        } else {
                            if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
                                pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
                                x = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)x * 2)));
                            } else {
                                x = (int32)_sqlite3StorageColumnToTable(ref (tls).DerefOrNull(), pTab, (int16)x);
                            }
                        }
                        x = _sqlite3TableColumnToIndex(ref (tls).DerefOrNull(), pIdx1, x);
                        if (x >= 0) {
                            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2 = x;
                            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur;
                        } else {
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_IDX_ONLY) | libc.Int32FromInt32(WHERE_EXPRIDX))) != 0) {
                                if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IDX_ONLY) != 0) {
                                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24316, 0);
                                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_INTERNAL;
                                } else {
                                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLoop + 48))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(WHERE_EXPRIDX));
                                    _sqlite3WhereAddExplainText(tls, pParse, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBody - (int32)1, pTabList, pLevel, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags);
                                }
                            }
                        }
                    } else {
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Rowid) {
                            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur;
                            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode = (uint8)OP_IdxRowid;
                        } else {
                            if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_IfNullRow) {
                                (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur;
                            }
                        }
                    }
                }
                goto _12;
_12:;
                pOp += 24;
                v2 = pOp;
                if (!(v2 < pLastOp)) {
                    break;
                }
            }
        }
        goto _10;
_10:;
        i = i + 1;
        pLevel += 112;
    }
    _sqlite3VdbeResolveLabel(tls, v, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiBreak);
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnQueryLoop = (int16)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsavedNQueryLoop;
    _whereInfoFree(tls, db, pWInfo);
    v2 = pParse + 35;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)) - nRJ);
    return;
}

internal partial struct _sqlite3WhereRightJoinLoop__ᴛ1 /*dyn*/ {
    public /*[0][88]*/ array<array<Tu8>> FfromSpace = new(0, () => new(88));
    public TSrcList FsSrc;
    public array<byte> F__ccgo_pad2 = new(80);
}

internal static void _sqlite3WhereRightJoinLoop(ж<libc.TLS> tls, uintptr pWInfo, int32 iLevel, uintptr pLevel) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(112);
        defer(tls.Free, (nint)112, ref ᒐ);
        int32 addrCont = default;
        int32 iCol = default;
        int32 iCur = default;
        int32 iIdxCur = default;
        int32 iPk = default;
        int32 k = default;
        int32 nPk = default;
        int32 r = default;
        int32 r2 = default;
        int32 v3 = default;
        TBitmask mAll = default;
        uintptr pFrom = default;
        uintptr pLoop = default;
        uintptr pParse = default;
        uintptr pPk = default;
        uintptr pRJ = default;
        uintptr pRight = default;
        uintptr pSubWInfo = default;
        uintptr pSubWhere = default;
        uintptr pSubq = default;
        uintptr pTab = default;
        uintptr pTabItem = default;
        uintptr pTerm = default;
        uintptr pWC = default;
        uintptr v = default;
        uintptr v4 = default;
        _sqlite3WhereRightJoinLoop__ᴛ1 _ᴛ1 = new();
        _ = addrCont;
        _ = iCol;
        _ = iCur;
        _ = iIdxCur;
        _ = iPk;
        _ = k;
        _ = mAll;
        _ = nPk;
        _ = pFrom;
        _ = pLoop;
        _ = pParse;
        _ = pPk;
        _ = pRJ;
        _ = pRight;
        _ = pSubWInfo;
        _ = pSubWhere;
        _ = pSubq;
        _ = pTab;
        _ = pTabItem;
        _ = pTerm;
        _ = pWC;
        _ = r;
        _ = r2;
        _ = v;
        _ = v3;
        _ = v4;
        pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        pRJ = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpRJ;
        pSubWhere = (uintptr)0;
        pWC = pWInfo + 104;
        pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
        pTabItem = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80;
        mAll = (uint64)0;
        _sqlite3VdbeExplain(tls, pParse, (uint8)1, __ccgo_ts + 24069, libc.VaList(bp + 96, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FpSTab)).Value.FzName));
        k = 0;
        for (;;) {
            if (!(k < iLevel)) {
                break;
            }
            pRight = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)k * 112))).Value.FiFrom * 80;
            mAll = (TBitmask)(mAll | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)k * 112))).Value.FpWLoop)).Value.FmaskSelf);
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pRight + 24 + 4)) & 0x40) >> 6) != 0) {
                pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRight + 72));
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregResult, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregResult + (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FpSelect)).Value.FpEList)).Value.FnExpr - (int32)1);
            }
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)k * 112))).Value.FiTabCur);
            iIdxCur = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)k * 112))).Value.FiIdxCur;
            if (iIdxCur != 0) {
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, iIdxCur);
            }
            goto _1;
_1:;
            k = k + 1;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (int32)JT_LTORJ) == 0) {
            mAll = (TBitmask)(mAll | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf);
            k = 0;
            for (;;) {
                if (!(k < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm)) {
                    break;
                }
                pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)k * 56;
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_SLICE))) != 0 && libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) != (int32)WO_ROWVAL) {
                    break;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll & ~mAll) != 0) {
                    goto _2;
                }
                if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_InnerON))) != (uint32)0) {
                    goto _2;
                }
                pSubWhere = _sqlite3ExprAnd(tls, pParse, pSubWhere, _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr, 0));
                goto _2;
_2:;
                k = k + 1;
            }
        }
        if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur != 0) {
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_NullRow, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur);
        }
        pFrom = bp;
        (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pFrom)).Value.FnSrc = (int32)1;
        (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pFrom)).Value.FnAlloc = (uint32)1;
        libc.Xmemcpy(tls, pFrom + 8, pTabItem, (uint64)80);
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pFrom + 8))).Value.Ffg.Fjointype = (uint8)0;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn) >= (int32)100) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24083, 0);
            return;
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn = (Tu8)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn + 1);
        pSubWInfo = _sqlite3WhereBegin(tls, pParse, pFrom, pSubWhere, (uintptr)0, (uintptr)0, (uintptr)0, (uint16)WHERE_RIGHT_JOIN, 0);
        if (pSubWInfo != 0) {
            iCur = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur;
            v4 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
            v3 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            r = v3;
            addrCont = _sqlite3WhereContinueLabel(ref (tls).DerefOrNull(), pSubWInfo);
            pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FpSTab;
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iCur, -(int32)1, r);
                nPk = (int32)1;
            } else {
                pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
                nPk = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nPk - (int32)1;
                iPk = 0;
                for (;;) {
                    if (!(iPk < nPk)) {
                        break;
                    }
                    iCol = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)iPk * 2)));
                    _sqlite3ExprCodeGetColumnOfTable(tls, v, pTab, iCur, iCol, r + iPk);
                    goto _5;
_5:;
                    iPk = iPk + 1;
                }
            }
            if ((@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom != 0) {
                _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Filter, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2, r, nPk);
            }
            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Found, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch, addrCont, r, nPk);
            r2 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, r, nPk, r2);
            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FiMatch, r2, r, nPk);
            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, r2);
            if ((@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom != 0) {
                _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_FilterAdd, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregBloom, 0, r, nPk);
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_USESEEKRESULT);
            }
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FregReturn, (@unsafe.Pointer.To<TWhereRightJoin>((@unsafe.Pointer)pRJ)).Value.FaddrSubrtn);
            _sqlite3WhereEnd(tls, pSubWInfo);
        }
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pSubWhere);
        _sqlite3VdbeExplainPop(ref (tls).DerefOrNull(), pParse);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn = (Tu8)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwithinRJSubrtn - 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3WhereTabFuncArgs(ж<libc.TLS> tls, uintptr pParse, uintptr pItem, uintptr pWC) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 j = default;
        int32 k = default;
        int32 v2 = default;
        Tu32 joinType = default;
        uintptr pArgs = default;
        uintptr pColRef = default;
        uintptr pRhs = default;
        uintptr pTab = default;
        uintptr pTerm = default;
        _ = j;
        _ = joinType;
        _ = k;
        _ = pArgs;
        _ = pColRef;
        _ = pRhs;
        _ = pTab;
        _ = pTerm;
        _ = v2;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x8) >> 3) == 0) {
            return;
        }
        pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab;
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB))) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21169, libc.VaList(bp + 8, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName));
            return;
        }
        pArgs = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 48));
        if (pArgs == (uintptr)0) {
            return;
        }
        v2 = libc.Int32FromInt32(0);
        k = v2;
        j = v2;
        for (;;) {
            if (!(j < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pArgs)).Value.FnExpr)) {
                break;
            }
            while (k < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)k * 16))).Value.FcolFlags) & (int32)COLFLAG_HIDDEN) == 0) {
                k = k + 1;
            }
            if (k >= (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24118, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, j));
                return;
            }
            pColRef = _sqlite3ExprAlloc(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (int32)TK_COLUMN, nil, 0);
            if (pColRef == (uintptr)0) {
                return;
            }
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColRef)).Value.FiTable = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor;
            v2 = k;
            k = k + 1;
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColRef)).Value.FiColumn = (int16)v2;
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pColRef + 64))).Value = pTab;
            (@unsafe.Pointer.To<TBitmask>(__ccgo_ptr(pItem + 40))).Value |= _sqlite3ExprColUsed(ref (tls).DerefOrNull(), pColRef);
            pRhs = _sqlite3PExpr(tls, pParse, (int32)TK_UPLUS, _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pArgs + 8 + (uintptr)j * 32))).Value.FpExpr, 0), (uintptr)0);
            pTerm = _sqlite3PExpr(tls, pParse, (int32)TK_EQ, pColRef, pRhs);
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_RIGHT))) != 0) {
                joinType = (uint32)EP_OuterON;
            } else {
                joinType = (uint32)EP_InnerON;
            }
            _sqlite3SetJoinExpr(ref (tls).DerefOrNull(), pTerm, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor, joinType);
            _whereClauseInsert(tls, pWC, pTerm, (uint16)TERM_DYNAMIC);
            goto _1;
_1:;
            j = j + 1;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3_table_column_metadata__ccgo_gp(ж<libc.TLS> tls, uintptr db, uintptr zDbName, uintptr zTableName, uintptr zColumnName, uintptr pzDataType, @unsafe.Pointer pzCollSeq, uintptr pNotNull, uintptr pPrimaryKey, uintptr pAutoinc) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 autoinc = default;
        int32 iCol = default;
        int32 notnull = default;
        int32 primarykey = default;
        int32 rc = default;
        uintptr pCol = default;
        uintptr pTab = default;
        uintptr zCollSeq = default;
        uintptr zDataType = default;
        uintptr v1 = default;
        uintptr _ᴛ1 = default;
        _ = autoinc;
        _ = iCol;
        _ = notnull;
        _ = pCol;
        _ = pTab;
        _ = primarykey;
        _ = rc;
        _ = zCollSeq;
        _ = zDataType;
        _ = v1;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
        pTab = (uintptr)0;
        pCol = (uintptr)0;
        iCol = 0;
        zDataType = (uintptr)0;
        zCollSeq = (uintptr)0;
        notnull = 0;
        primarykey = 0;
        autoinc = 0;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        _sqlite3BtreeEnterAll(tls, db);
        rc = _sqlite3Init(tls, db, bp);
        if (SQLITE_OK != rc) {
            goto error_out;
        }
        pTab = _sqlite3FindTable(tls, db, zTableName, zDbName);
        if (!(pTab != 0) || libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW) {
            pTab = (uintptr)0;
            goto error_out;
        }
        if (zColumnName == (uintptr)0) {
        } else {
            iCol = _sqlite3ColumnIndex(ref (tls).DerefOrNull(), pTab, zColumnName);
            if (iCol >= 0) {
                pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16;
            } else {
                if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 && _sqlite3IsRowid(ref (tls).DerefOrNull(), zColumnName) != 0) {
                    iCol = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey;
                    if (iCol >= 0) {
                        v1 = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16;
                    } else {
                        v1 = (uintptr)0;
                    }
                    pCol = v1;
                } else {
                    pTab = (uintptr)0;
                    goto error_out;
                }
            }
        }
        if (pCol != 0) {
            zDataType = _sqlite3ColumnType(tls, pCol, (uintptr)0);
            zCollSeq = _sqlite3ColumnColl(ref (tls).DerefOrNull(), pCol);
            notnull = libc.BoolInt32((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pCol + 8)) & 0xf) >> 0) != 0);
            primarykey = libc.BoolInt32((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_PRIMKEY) != 0);
            autoinc = libc.BoolInt32((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey == iCol && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Autoincrement) != (uint32)0);
        } else {
            zDataType = __ccgo_ts + 1198;
            primarykey = (int32)1;
        }
        if (!(zCollSeq != 0)) {
            zCollSeq = (uintptr)Ꮡ_sqlite3StrBINARY;
        }
        goto error_out;
error_out:;
        _sqlite3BtreeLeaveAll(tls, db);
        if (pzDataType != 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzDataType))).Value = zDataType;
        }
        if (pzCollSeq != nil) {
            (@unsafe.Pointer.To<uintptr>(pzCollSeq)).Value = zCollSeq;
        }
        if (pNotNull != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNotNull))).Value = notnull;
        }
        if (pPrimaryKey != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pPrimaryKey))).Value = primarykey;
        }
        if (pAutoinc != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pAutoinc))).Value = autoinc;
        }
        if (SQLITE_OK == rc && !(pTab != 0)) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3MPrintf(tls, db, __ccgo_ts + 26506, libc.VaList(bp + 16, zTableName, zColumnName));
            rc = (int32)SQLITE_ERROR;
        }
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) != 0) {
            v1 = __ccgo_ts + 3982;
        } else {
            v1 = (uintptr)0;
        }
        _sqlite3ErrorWithMsg(tls, db, rc, v1, libc.VaList(bp + 16, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))));
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        rc = _sqlite3ApiExit(tls, db, rc);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _vdbeUnbind(ж<libc.TLS> tls, uintptr p, uint32 i) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pVar = default;
        uint32 v1 = default;
        uint32 v3 = default;
        bool v2 = default;
        bool v4 = default;
        _ = pVar;
        _ = v1;
        _ = v2;
        _ = v3;
        _ = v4;
        if (_vdbeSafetyNotNull(tls, p) != 0) {
            r = _sqlite3MisuseError(tls, (int32)95387); goto ᒐdone;
        }
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) != (int32)VDBE_READY_STATE) {
            _sqlite3Error(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, _sqlite3MisuseError(tls, (int32)95391));
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
            Xsqlite3_log(tls, (int32)SQLITE_MISUSE, __ccgo_ts + 5759, libc.VaList(bp + 8, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql));
            r = _sqlite3MisuseError(tls, (int32)95395); goto ᒐdone;
        }
        if (i >= libc.Uint32FromInt16((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnVar)) {
            _sqlite3Error(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, (int32)SQLITE_RANGE);
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
            r = (int32)SQLITE_RANGE; goto ᒐdone;
        }
        pVar = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar + (uintptr)i * 56;
        v2 = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fexpmask != (uint32)0; if (v2) {
            if (i >= (uint32)31) {
                v1 = (uint32)0x80000000U;
            } else {
                v1 = libc.Uint32FromInt32(1).Lsh((uint64)(i));
            }
        }
        if (v2 && (Tu32)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fexpmask & v1) != (uint32)0) {
            v4 = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fsmimask == (uint32)0; if (!v4) {
                if (i >= (uint32)31) {
                    v3 = (uint32)0x80000000U;
                } else {
                    v3 = libc.Uint32FromInt32(1).Lsh((uint64)(i));
                }
            }
            if (v4 || (Tu32)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fsmimask & v3) == (uint32)0 || (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVar)).Value.Fflags) & (int32)MEM_Int) != 0 && ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVar) >= 0 && ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVar) <= (int64)1) {
                libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 0, 0x3);
            }
        }
        _sqlite3VdbeMemRelease(tls, pVar);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVar)).Value.Fflags = (uint16)MEM_Null;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.FerrCode = SQLITE_OK;
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _walIndexRecover(ж<libc.TLS> tls, uintptr pWal) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        uintptr aData = default;
        uintptr aFrame = default;
        uintptr aPrivate = default;
        uintptr pInfo = default;
        array<Tu32> aFrameCksum = new(2);
        int32 i = default;
        int32 iLock = default;
        int32 isValid = default;
        int32 rc = default;
        int32 szFrame = default;
        int32 szPage = default;
        Tu32 iFirst = default;
        Tu32 iFrame = default;
        Tu32 iLast = default;
        Tu32 iLastFrame = default;
        Tu32 iPg = default;
        Tu32 magic = default;
        Tu32 nHdr = default;
        Tu32 nHdr32 = default;
        ref var nTruncate = ref heap(new Tu32(), out var ᏑnTruncate);
        ref var pgno = ref heap(new Tu32(), out var Ꮡpgno);
        Tu32 version = default;
        Ti64 iOffset = default;
        uint64 v2 = default;
        uint64 v3 = default;
        array<Tu8> _ᴛ1 = new(32);
        uintptr _ᴛ2 = default;
        Ti64 _ᴛ3 = default;
        _ = aData;
        _ = aFrame;
        _ = aFrameCksum;
        _ = aPrivate;
        _ = i;
        _ = iFirst;
        _ = iFrame;
        _ = iLast;
        _ = iLastFrame;
        _ = iLock;
        _ = iOffset;
        _ = iPg;
        _ = isValid;
        _ = magic;
        _ = nHdr;
        _ = nHdr32;
        _ = nTruncate;
        _ = pInfo;
        _ = pgno;
        _ = rc;
        _ = szFrame;
        _ = szPage;
        _ = version;
        _ = v2;
        _ = v3;
        aFrameCksum.Assign(new Tu32[]{}.array(2));
        iLock = (int32)WAL_ALL_BUT_WRITE + libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock);
        rc = _walLockExclusive(tls, pWal, iLock, libc.Int32FromInt32(3) + libc.Int32FromInt32(0) - iLock);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        libc.Xmemset(tls, pWal + 72, 0, (uint64)48);
        rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp);
        if (rc != SQLITE_OK) {
            goto recovery_error;
        }
        if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) > (int64)WAL_HDRSIZE) {
            aPrivate = (uintptr)0;
            aFrame = (uintptr)0;
            rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp + 8, (int32)WAL_HDRSIZE, 0);
            if (rc != SQLITE_OK) {
                goto recovery_error;
            }
            magic = _sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 8));
            szPage = libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 16)));
            if ((Tu32)(magic & (uint32)0xFFFFFFFEU) != (uint32)WAL_MAGIC || (szPage & (szPage - (int32)1)) != 0 || szPage > (int32)SQLITE_MAX_PAGE_SIZE || szPage < (int32)512) {
                goto finished;
            }
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FbigEndCksum = (uint8)((Tu32)(magic & libc.Uint32FromInt32(0x00000001)));
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage = libc.Uint32FromInt32(szPage);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnCkpt = _sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 20));
            libc.Xmemcpy(tls, pWal + 72 + 32, bp + 8 + 16, (uint64)8);
            _walChecksumBytes(ref (tls).DerefOrNull(), libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FbigEndCksum) == SQLITE_BIGENDIAN), bp + 8, libc.Int32FromInt32(WAL_HDRSIZE) - libc.Int32FromInt32(2) * libc.Int32FromInt32(4), (uintptr)0, (@unsafe.Pointer)(pWal + 72 + 24));
            if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24)) != _sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 32)) || ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4)) != _sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 36))) {
                goto finished;
            }
            version = _sqlite3Get4byte(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 12));
            if (version != (uint32)WAL_MAX_VERSION) {
                rc = _sqlite3CantopenError(tls, (int32)68581);
                goto finished;
            }
            szFrame = szPage + (int32)WAL_FRAME_HDRSIZE;
            aFrame = Xsqlite3_malloc64(tls, (uint64)(libc.Uint64FromInt32(szFrame) + (libc.Uint64FromInt64(2) * libc.Uint64FromInt32(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2)) + libc.Uint64FromInt32(HASHTABLE_NPAGE) * libc.Uint64FromInt64(4))));
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpFree = aFrame;
            if (!(aFrame != 0)) {
                rc = (int32)SQLITE_NOMEM;
                goto recovery_error;
            }
            aData = aFrame + 24;
            aPrivate = aData + (uintptr)szPage;
            iLastFrame = libc.Uint32FromInt64(quo((~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) - (int64)WAL_HDRSIZE), (int64)szFrame));
            iPg = (uint32)0;
            for (;;) {
                if (!(iPg <= libc.Uint32FromInt32(_walFramePage(ref (tls).DerefOrNull(), iLastFrame)))) {
                    break;
                }
                if ((uint64)iLastFrame < libc.Uint64FromInt32(HASHTABLE_NPAGE) - (libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40)) / libc.Uint64FromInt64(4) + (uint64)(iPg * (uint32)HASHTABLE_NPAGE)) {
                    v2 = (uint64)iLastFrame;
                } else {
                    v2 = libc.Uint64FromInt32(HASHTABLE_NPAGE) - (libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40)) / libc.Uint64FromInt64(4) + (uint64)(iPg * (uint32)HASHTABLE_NPAGE);
                }
                iLast = (uint32)v2;
                if (iPg == (uint32)0) {
                    v3 = (uint64)0;
                } else {
                    v3 = libc.Uint64FromInt32(HASHTABLE_NPAGE) - (libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40)) / libc.Uint64FromInt64(4) + (uint64)((iPg - (uint32)1) * (uint32)HASHTABLE_NPAGE);
                }
                iFirst = (uint32)((uint64)1 + v3);
                rc = _walIndexPage(tls, pWal, libc.Int32FromUint32(iPg), bp + 40);
                if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 40)) == (uintptr)0) {
                    break;
                }
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiWiPg = libc.Int32FromUint32(iPg);
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWiValue = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 40));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPg * 8))).Value = aPrivate;
                iFrame = iFirst;
                for (;;) {
                    if (!(iFrame <= iLast)) {
                        break;
                    }
                    iOffset = libc.Int64FromInt32(WAL_HDRSIZE) + libc.Int64FromUint32(iFrame - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE));
                    rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, aFrame, szFrame, iOffset);
                    if (rc != SQLITE_OK) {
                        break;
                    }
                    isValid = _walDecodeFrame(tls, pWal, @unsafe.Pointer.FromPinnedBox(Ꮡpgno), @unsafe.Pointer.FromPinnedBox(ᏑnTruncate), aData, aFrame);
                    if (!(isValid != 0)) {
                        break;
                    }
                    rc = _walIndexAppend(tls, pWal, iFrame, pgno);
                    if (rc != SQLITE_OK) {
                        break;
                    }
                    if (nTruncate != 0) {
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame = iFrame;
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FnPage = nTruncate;
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage = libc.Uint16FromInt32(szPage & libc.Int32FromInt32(0xff00) | szPage.Rsh((int64)(libc.Int32FromInt32(16))));
                        aFrameCksum[0] = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24));
                        aFrameCksum[(int32)1] = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4));
                    }
                    goto _4;
_4:;
                    iFrame = iFrame + 1;
                }
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPg * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 40))).Value;
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiWiPg = 0;
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWiValue = (uintptr)0;
                if (iPg == (uint32)0) {
                    v2 = libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40);
                } else {
                    v2 = (uint64)0;
                }
                nHdr = (uint32)v2;
                nHdr32 = (uint32)((uint64)nHdr / (uint64)4);
                libc.Xmemcpy(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 40)) + (uintptr)nHdr32 * 4, aPrivate + (uintptr)nHdr32 * 4, libc.Uint64FromInt64(2) * libc.Uint64FromInt32(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2)) + libc.Uint64FromInt32(HASHTABLE_NPAGE) * libc.Uint64FromInt64(4) - (uint64)nHdr);
                _modernc_seh_inject(ref (tls).DerefOrNull(), pWal);
                if (iFrame <= iLast) {
                    break;
                }
                goto _1;
_1:;
                iPg = iPg + 1;
            }
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpFree = (uintptr)0;
            Xsqlite3_free(tls, aFrame);
        }
        goto finished;
finished:;
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24))).Value = aFrameCksum[0];
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4))).Value = aFrameCksum[(int32)1];
            _walIndexWriteHdr(tls, pWal);
            pInfo = _walCkptInfo(ref (tls).DerefOrNull(), pWal);
            (@unsafe.Pointer.To<TWalCkptInfo>((@unsafe.Pointer)pInfo)).Value.FnBackfill = (uint32)0;
            (@unsafe.Pointer.To<TWalCkptInfo>((@unsafe.Pointer)pInfo)).Value.FnBackfillAttempted = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pInfo + 4))).Value = (uint32)0;
            i = (int32)1;
            for (;;) {
                if (!(i < libc.Int32FromInt32(SQLITE_SHM_NLOCK) - libc.Int32FromInt32(3))) {
                    break;
                }
                rc = _walLockExclusive(tls, pWal, (int32)3 + i, (int32)1);
                if (rc == SQLITE_OK) {
                    if (i == (int32)1 && (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame != 0) {
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pInfo + 4 + (uintptr)i * 4))).Value = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
                    } else {
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pInfo + 4 + (uintptr)i * 4))).Value = (uint32)READMARK_NOT_USED;
                    }
                    _modernc_seh_inject(ref (tls).DerefOrNull(), pWal);
                    _walUnlockExclusive(tls, pWal, (int32)3 + i, (int32)1);
                } else {
                    if (rc != (int32)SQLITE_BUSY) {
                        goto recovery_error;
                    }
                }
                goto _6;
_6:;
                i = i + 1;
            }
            if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FnPage != 0) {
                Xsqlite3_log(tls, libc.Int32FromInt32(SQLITE_NOTICE) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 4318, libc.VaList(bp + 56, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FzWalName));
            }
        }
        goto recovery_error;
recovery_error:;
        _walUnlockExclusive(tls, pWal, iLock, libc.Int32FromInt32(3) + libc.Int32FromInt32(0) - iLock);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _whereLoopAddAll(ж<libc.TLS> tls, @unsafe.Pointer pBuilder) {
    int32 bFirstPastRJ = default;
    int32 hasRightCrossJoin = default;
    int32 i = default;
    int32 iTab = default;
    int32 rc = default;
    uintptr db = default;
    uintptr p = default;
    uintptr pEnd = default;
    uintptr pItem = default;
    uintptr pNew = default;
    uintptr pTabList = default;
    uintptr pTerm = default;
    uintptr pWC = default;
    uintptr pWInfo = default;
    TBitmask mPrereq = default;
    TBitmask mPrior = default;
    TBitmask mUnusable = default;
    _ = bFirstPastRJ;
    _ = db;
    _ = hasRightCrossJoin;
    _ = i;
    _ = iTab;
    _ = mPrereq;
    _ = mPrior;
    _ = mUnusable;
    _ = p;
    _ = pEnd;
    _ = pItem;
    _ = pNew;
    _ = pTabList;
    _ = pTerm;
    _ = pWC;
    _ = pWInfo;
    _ = rc;
    pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
    mPrereq = (uint64)0;
    mPrior = (uint64)0;
    pTabList = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList;
    pEnd = pTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel * 80;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb;
    rc = SQLITE_OK;
    bFirstPastRJ = 0;
    hasRightCrossJoin = 0;
    pNew = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
    (@unsafe.Pointer.To<uint32>(@unsafe.Add(pBuilder, 48))).Value = (uint32)SQLITE_QUERY_PLANNER_LIMIT;
    iTab = 0;
    pItem = pTabList + 8;
    for (;;) {
        if (!(pItem < pEnd)) {
            break;
        }
        mUnusable = (uint64)0;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiTab = libc.Uint8FromInt32(iTab);
        (@unsafe.Pointer.To<uint32>(@unsafe.Add(pBuilder, 48))).Value += (uint32)SQLITE_QUERY_PLANNER_LIMIT_INCR;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf = _sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor);
        if (bFirstPastRJ != 0 || (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_CROSS) | libc.Int32FromInt32(JT_LTORJ))) != 0) {
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_CROSS))) != 0) {
                hasRightCrossJoin = (int32)1;
            }
            mPrereq = (TBitmask)(mPrereq | mPrior);
            bFirstPastRJ = libc.BoolInt32((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0);
        } else {
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x40000) >> 18) != 0) {
                pWC = pWInfo + 104;
                i = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnBase;
                pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
                for (;;) {
                    if (!(i > 0)) {
                        break;
                    }
                    if ((TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf & (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll) != (uint64)0) {
                        mPrereq = (TBitmask)(mPrereq | (TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll & ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf - (uint64)1)));
                    }
                    goto _2;
_2:;
                    i = i - 1;
                    pTerm += 56;
                }
            } else {
                if (!(hasRightCrossJoin != 0)) {
                    mPrereq = (uint64)0;
                }
            }
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
            p = pItem + 1 * 80;
            for (;;) {
                if (!(p < pEnd)) {
                    break;
                }
                if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_CROSS))) != 0) {
                    mUnusable = (TBitmask)(mUnusable | _sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p)).Value.FiCursor));
                }
                goto _3;
_3:;
                p += 80;
            }
            rc = _whereLoopAddVirtual(tls, pBuilder, mPrereq, mUnusable);
        } else {
            rc = _whereLoopAddBtree(tls, pBuilder, mPrereq);
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8))))).Value.FhasOr != 0) {
            rc = _whereLoopAddOr(tls, pBuilder, mPrereq, mUnusable);
        }
        mPrior = (TBitmask)(mPrior | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf);
        if (rc != 0 || (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            if (rc == (int32)SQLITE_DONE) {
                Xsqlite3_log(tls, (int32)SQLITE_WARNING, __ccgo_ts + 24217, 0);
                rc = SQLITE_OK;
            } else {
                break;
            }
        }
        goto _1;
_1:;
        iTab = iTab + 1;
        pItem += 80;
    }
    _whereLoopClear(tls, db, pNew);
    return rc;
}

internal static int32 /*r*/ _whereLoopAddVirtualOne(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, TBitmask mPrereq, TBitmask mUsable, Tu16 mExclude, uintptr pIdxInfo, Tu16 mNoOmit, @unsafe.Pointer pbIn, @unsafe.Pointer pbRetryLimit) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        int32 iTerm = default;
        int32 j = default;
        int32 mxTerm = default;
        int32 nConstraint = default;
        int32 rc = default;
        int32 v3 = default;
        uintptr pHidden = default;
        uintptr pIdxCons = default;
        uintptr pNew = default;
        uintptr pParse = default;
        uintptr pSrc = default;
        uintptr pTerm = default;
        uintptr pTerm1 = default;
        uintptr pUsage = default;
        uintptr pWC = default;
        uintptr v4 = default;
        bool v5 = default;
        uint32 v7 = default;
        _ = i;
        _ = iTerm;
        _ = j;
        _ = mxTerm;
        _ = nConstraint;
        _ = pHidden;
        _ = pIdxCons;
        _ = pNew;
        _ = pParse;
        _ = pSrc;
        _ = pTerm;
        _ = pTerm1;
        _ = pUsage;
        _ = pWC;
        _ = rc;
        _ = v3;
        _ = v4;
        _ = v5;
        _ = v7;
        pWC = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8));
        pHidden = pIdxInfo + 1 * 96;
        pUsage = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage;
        rc = SQLITE_OK;
        pNew = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
        pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pBuilder)))).Value.FpParse;
        pSrc = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pBuilder)))).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiTab * 80;
        nConstraint = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint;
        (@unsafe.Pointer.To<int32>(pbIn)).Value = 0;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq = mPrereq;
        pIdxCons = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pIdxInfo + 8));
        i = 0;
        for (;;) {
            if (!(i < nConstraint)) {
                break;
            }
            pTerm = _termFromWhereClause(ref (tls).DerefOrNull(), pWC, (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pIdxCons)).Value.FiTermOffset);
            (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pIdxCons)).Value.Fusable = (uint8)0;
            if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight & mUsable) == (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & libc.Int32FromUint16(mExclude)) == 0 && (pbRetryLimit != nil || !(_isLimitTerm(ref (tls).DerefOrNull(), pTerm) != 0))) {
                (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pIdxCons)).Value.Fusable = (uint8)1;
            }
            goto _1;
_1:;
            i = i + 1;
            pIdxCons += 12;
        }
        libc.Xmemset(tls, pUsage, 0, (uint64)8 * libc.Uint64FromInt32(nConstraint));
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr = (uintptr)0;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = 0;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.ForderByConsumed = 0;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = libc.Float64FromFloat64(1e+99D) / libc.Float64FromInt32(2);
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)25;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxFlags = 0;
        (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FmHandleIn = (uint32)0;
        rc = _vtabBestIndex(tls, pParse, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab, pIdxInfo);
        if (rc != 0) {
            if (rc == (int32)SQLITE_CONSTRAINT) {
                _freeIdxStr(tls, pIdxInfo);
                r = SQLITE_OK; goto ᒐdone;
            }
            r = rc; goto ᒐdone;
        }
        mxTerm = -(int32)1;
        libc.Xmemset(tls, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm, 0, (uint64)8 * libc.Uint64FromInt32(nConstraint));
        libc.Xmemset(tls, pNew + 24, 0, (uint64)24);
        pIdxCons = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pIdxInfo + 8));
        i = 0;
        for (;;) {
            if (!(i < nConstraint)) {
                break;
            }
            v3 = (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr(pUsage + (uintptr)i * 8))).Value.FargvIndex - libc.Int32FromInt32(1);
            iTerm = v3;
            if (v3 >= 0) {
                j = (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pIdxCons)).Value.FiTermOffset;
                v5 = iTerm >= nConstraint || j < 0; if (!v5) {
                    v4 = _termFromWhereClause(ref (tls).DerefOrNull(), pWC, j);
                    pTerm1 = v4;
                }
                if (v5 || v4 == (uintptr)0 || ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)iTerm * 8)) != (uintptr)0 || libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pIdxCons)).Value.Fusable) == 0) {
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24191, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab)).Value.FzName));
                    _freeIdxStr(tls, pIdxInfo);
                    r = (int32)SQLITE_ERROR; goto ᒐdone;
                }
                (@unsafe.Pointer.To<TBitmask>(__ccgo_ptr(pNew))).Value |= (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FprereqRight;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)iTerm * 8))).Value = pTerm1;
                if (iTerm > mxTerm) {
                    mxTerm = iTerm;
                }
                if ((@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr(pUsage + (uintptr)i * 8))).Value.Fomit != 0) {
                    if (i < (int32)16 && (((int32)1).Lsh((int64)(i)) & libc.Int32FromUint16(mNoOmit)) == 0) {
                        v4 = pNew + 24 + 6;
                        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)) | libc.Int32FromInt32(1).Lsh((int64)(iTerm)));
                    } else {
                    }
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FeMatchOp) == (int32)SQLITE_INDEX_CONSTRAINT_OFFSET) {
                        libc.SetBitFieldPtr8Uint32(pNew + 24 + 4, libc.Uint32FromInt32(1), 1, 0x2);
                    }
                }
                if (i <= (int32)31) {
                    v7 = libc.Uint32FromInt32(1).Lsh((int64)(i));
                } else {
                    v7 = (uint32)0;
                }
                if ((v7 & (@unsafe.Pointer.To<THiddenIndexInfo>((@unsafe.Pointer)pHidden)).Value.FmHandleIn) != 0) {
                    (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pNew + 24))).Value.FmHandleIn |= (uint32)(libc.Uint32FromInt32(1).Lsh((int64)(iTerm)));
                } else {
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm1)).Value.FeOperator) & (int32)WO_IN) != 0) {
                        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.ForderByConsumed = 0;
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIdxInfo + 80))).Value &= ~libc.Int32FromInt32(SQLITE_INDEX_SCAN_UNIQUE);
                        (@unsafe.Pointer.To<int32>(pbIn)).Value = (int32)1;
                    }
                }
                if (_isLimitTerm(ref (tls).DerefOrNull(), pTerm1) != 0 && (~@unsafe.Pointer.To<int32>(pbIn) != 0 || !(_allConstraintsUsed(ref (tls).DerefOrNull(), pUsage, i) != 0))) {
                    _freeIdxStr(tls, pIdxInfo);
                    (@unsafe.Pointer.To<int32>(pbRetryLimit)).Value = (int32)1;
                    r = SQLITE_OK; goto ᒐdone;
                }
            }
            goto _2;
_2:;
            i = i + 1;
            pIdxCons += 12;
        }
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = libc.Uint16FromInt32(mxTerm + (int32)1);
        i = 0;
        for (;;) {
            if (!(i <= mxTerm)) {
                break;
            }
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)i * 8)) == (uintptr)0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24191, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab)).Value.FzName));
                _freeIdxStr(tls, pIdxInfo);
                r = (int32)SQLITE_ERROR; goto ᒐdone;
            }
            goto _8;
_8:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pNew + 24))).Value.FidxNum = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum;
        libc.SetBitFieldPtr8Uint32(pNew + 24 + 4, libc.Uint32FromInt32((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FneedToFreeIdxStr), 0, 0x1);
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FneedToFreeIdxStr = 0;
        (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pNew + 24))).Value.FidxStr = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr;
        if ((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.ForderByConsumed != 0) {
            v3 = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnOrderBy;
        } else {
            v3 = 0;
        }
        (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pNew + 24))).Value.FisOrdered = (int8)v3;
        libc.SetBitFieldPtr8Uint32(pNew + 24 + 4, libc.BoolUint32(((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxFlags & libc.Int32FromInt32(SQLITE_INDEX_SCAN_HEX)) != libc.Int32FromInt32(0)), 2, 0x4);
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = 0;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = _sqlite3LogEstFromDouble(tls, (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost);
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = _sqlite3LogEst(ref (tls).DerefOrNull(), libc.Uint64FromInt64((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows));
        if (((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxFlags & (int32)SQLITE_INDEX_SCAN_UNIQUE) != 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_ONEROW;
        } else {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(WHERE_ONEROW));
        }
        rc = _whereLoopInsert(tls, pBuilder, pNew);
        if ((int32)((Tu32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pNew + 24 + 4)) & 0x1) >> 0)) != 0) {
            Xsqlite3_free(tls, (@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pNew + 24))).Value.FidxStr);
            libc.SetBitFieldPtr8Uint32(pNew + 24 + 4, libc.Uint32FromInt32(0), 0, 0x1);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _wherePathSolver(ж<libc.TLS> tls, uintptr pWInfo, TLogEst nRowEst) {
    uintptr aFrom = default;
    uintptr aSortCost = default;
    uintptr aTo = default;
    uintptr pFrom = default;
    uintptr pLevel = default;
    uintptr pParse = default;
    uintptr pSpace = default;
    uintptr pTo = default;
    uintptr pWLoop = default;
    uintptr pX = default;
    uintptr v11 = default;
    int32 iLoop = default;
    int32 ii = default;
    int32 jj = default;
    int32 mxChoice = default;
    int32 mxI = default;
    int32 nFrom = default;
    int32 nLoop = default;
    int32 nOrder = default;
    int32 nOrderBy = default;
    int32 nSpace = default;
    int32 nTo = default;
    int32 rc = default;
    int32 rc1 = default;
    int32 v2 = default;
    Ti8 isOrdered = default;
    ref var m = ref heap(new TBitmask(), out var Ꮡm);
    TBitmask maskNew = default;
    ref var notUsed = ref heap(new TBitmask(), out var ᏑnotUsed);
    ref var revMask = ref heap(new TBitmask(), out var ᏑrevMask);
    ref var revMask1 = ref heap(new TBitmask(), out var ᏑrevMask1);
    TLogEst mxCost = default;
    TLogEst mxUnsort = default;
    TLogEst nOut = default;
    TLogEst rCost = default;
    TLogEst rUnsort = default;
    Tu32 wsFlags = default;
    _ = aFrom;
    _ = aSortCost;
    _ = aTo;
    _ = iLoop;
    _ = ii;
    _ = isOrdered;
    _ = jj;
    _ = m;
    _ = maskNew;
    _ = mxChoice;
    _ = mxCost;
    _ = mxI;
    _ = mxUnsort;
    _ = nFrom;
    _ = nLoop;
    _ = nOrder;
    _ = nOrderBy;
    _ = nOut;
    _ = nSpace;
    _ = nTo;
    _ = notUsed;
    _ = pFrom;
    _ = pLevel;
    _ = pParse;
    _ = pSpace;
    _ = pTo;
    _ = pWLoop;
    _ = pX;
    _ = rCost;
    _ = rUnsort;
    _ = rc;
    _ = rc1;
    _ = revMask;
    _ = revMask1;
    _ = wsFlags;
    _ = v11;
    _ = v2;
    mxI = 0;
    mxCost = 0;
    mxUnsort = 0;
    aSortCost = (uintptr)0;
    pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
    nLoop = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel);
    if (nLoop <= (int32)1) {
        mxChoice = (int32)1;
    } else {
        if (nLoop == (int32)2) {
            mxChoice = (int32)5;
        } else {
            if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                mxChoice = (int32)1;
            } else {
                mxChoice = _computeMxChoice(ref (tls).DerefOrNull(), pWInfo);
            }
        }
    }
    if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy == (uintptr)0 || (int32)nRowEst == 0) {
        nOrderBy = 0;
    } else {
        nOrderBy = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy)).Value.FnExpr;
    }
    nSpace = libc.Int32FromUint64(((uint64)32 + (uint64)8 * libc.Uint64FromInt32(nLoop)) * libc.Uint64FromInt32(mxChoice) * (uint64)2);
    nSpace = libc.Int32FromUint64((uint64)nSpace + libc.Uint64FromInt64(2) * libc.Uint64FromInt32(nOrderBy));
    pSpace = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, libc.Uint64FromInt32(nSpace));
    if (pSpace == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    aTo = pSpace;
    aFrom = aTo + (uintptr)mxChoice * 32;
    libc.Xmemset(tls, aFrom, 0, (uint64)32);
    pX = aFrom + (uintptr)mxChoice * 32;
    ii = mxChoice * (int32)2;
    pFrom = aTo;
    for (;;) {
        if (!(ii > 0)) {
            break;
        }
        (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop = pX;
        goto _1;
_1:;
        ii = ii - 1;
        pFrom += 32;
        pX = pX + (uintptr)nLoop * 8;
    }
    if (nOrderBy != 0) {
        aSortCost = pX;
        libc.Xmemset(tls, aSortCost, 0, (uint64)2 * libc.Uint64FromInt32(nOrderBy));
    }
    if ((int32)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnQueryLoop < (int32)48) {
        v2 = (int32)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnQueryLoop;
    } else {
        v2 = (int32)48;
    }
    (@unsafe.Pointer.To<TWherePath>(__ccgo_ptr(aFrom))).Value.FnRow = (int16)v2;
    nFrom = (int32)1;
    if (nOrderBy != 0) {
        if (nLoop > 0) {
            v2 = -(int32)1;
        } else {
            v2 = nOrderBy;
        }
        (@unsafe.Pointer.To<TWherePath>(__ccgo_ptr(aFrom))).Value.FisOrdered = (int8)v2;
    }
    iLoop = 0;
    for (;;) {
        if (!(iLoop < nLoop)) {
            break;
        }
        nTo = 0;
        ii = 0;
        pFrom = aFrom;
        for (;;) {
            if (!(ii < nFrom)) {
                break;
            }
            pWLoop = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops;
            for (;;) {
                if (!(pWLoop != 0)) {
                    break;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.Fprereq & ~(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FmaskLoop) != (uint64)0) {
                    goto _6;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf & (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FmaskLoop) != (uint64)0) {
                    goto _6;
                }
                if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FwsFlags & (uint32)WHERE_AUTO_INDEX) != (uint32)0 && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FnRow < (int32)3) {
                    goto _6;
                }
                rUnsort = (int16)((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrRun + (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FnRow);
                if ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrSetup != 0) {
                    rUnsort = _sqlite3LogEstAdd(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrSetup, rUnsort);
                }
                rUnsort = _sqlite3LogEstAdd(ref (tls).DerefOrNull(), rUnsort, (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FrUnsort);
                nOut = (int16)((int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FnRow + (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FnOut);
                maskNew = (TBitmask)((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FmaskLoop | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf);
                isOrdered = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FisOrdered;
                if ((int32)isOrdered < 0) {
                    revMask = (uint64)0;
                    isOrdered = _wherePathSatisfiesOrderBy(tls, pWInfo, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy, pFrom, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags, libc.Uint16FromInt32(iLoop), pWLoop, @unsafe.Pointer.FromPinnedBox(ᏑrevMask));
                } else {
                    revMask = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FrevLoop;
                }
                if ((int32)isOrdered >= 0 && (int32)isOrdered < nOrderBy) {
                    if ((int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr(aSortCost + (uintptr)isOrdered * 2))) == 0) {
                        (@unsafe.Pointer.To<TLogEst>(__ccgo_ptr(aSortCost + (uintptr)isOrdered * 2))).Value = _whereSortingCost(ref (tls).DerefOrNull(), pWInfo, nRowEst, nOrderBy, (int32)isOrdered);
                    }
                    rCost = (int16)((int32)_sqlite3LogEstAdd(ref (tls).DerefOrNull(), rUnsort, ~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr(aSortCost + (uintptr)isOrdered * 2))) + (int32)3);
                } else {
                    rCost = rUnsort;
                    rUnsort = (int16)((int32)rUnsort - libc.Int32FromInt32(2));
                }
                jj = 0;
                pTo = aTo;
                for (;;) {
                    if (!(jj < nTo)) {
                        break;
                    }
                    if ((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FmaskLoop == maskNew && ((((int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FisOrdered ^ (int32)isOrdered) & (int32)0x80) == 0 || iLoop == nLoop - (int32)1)) {
                        break;
                    }
                    goto _7;
_7:;
                    jj = jj + 1;
                    pTo += 32;
                }
                if (jj >= nTo) {
                    if (nTo >= mxChoice && ((int32)rCost > (int32)mxCost || (int32)rCost == (int32)mxCost && (int32)rUnsort >= (int32)mxUnsort)) {
                        goto _6;
                    }
                    if (nTo < mxChoice) {
                        v2 = nTo;
                        nTo = nTo + 1;
                        jj = v2;
                    } else {
                        jj = mxI;
                    }
                    pTo = aTo + (uintptr)jj * 32;
                } else {
                    if ((int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost < (int32)rCost || (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost == (int32)rCost && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FnRow < (int32)nOut || (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost == (int32)rCost && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FnRow == (int32)nOut && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrUnsort < (int32)rUnsort || (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost == (int32)rCost && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FnRow == (int32)nOut && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrUnsort == (int32)rUnsort && _whereLoopIsNoBetter(ref (tls).DerefOrNull(), pWLoop, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FaLoop + (uintptr)iLoop * 8))) != 0) {
                        goto _6;
                    }
                }
                (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FmaskLoop = (TBitmask)((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FmaskLoop | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf);
                (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrevLoop = revMask;
                (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FnRow = nOut;
                (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost = rCost;
                (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrUnsort = rUnsort;
                (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FisOrdered = isOrdered;
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FaLoop, (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop, (uint64)8 * libc.Uint64FromInt32(iLoop));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FaLoop + (uintptr)iLoop * 8))).Value = pWLoop;
                if (nTo >= mxChoice) {
                    mxI = 0;
                    mxCost = (@unsafe.Pointer.To<TWherePath>(__ccgo_ptr(aTo))).Value.FrCost;
                    mxUnsort = (@unsafe.Pointer.To<TWherePath>(__ccgo_ptr(aTo))).Value.FnRow;
                    jj = (int32)1;
                    pTo = aTo + 1 * 32;
                    for (;;) {
                        if (!(jj < mxChoice)) {
                            break;
                        }
                        if ((int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost > (int32)mxCost || (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost == (int32)mxCost && (int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrUnsort > (int32)mxUnsort) {
                            mxCost = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrCost;
                            mxUnsort = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pTo)).Value.FrUnsort;
                            mxI = jj;
                        }
                        goto _9;
_9:;
                        jj = jj + 1;
                        pTo += 32;
                    }
                }
                goto _6;
_6:;
                pWLoop = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FpNextLoop;
            }
            goto _5;
_5:;
            ii = ii + 1;
            pFrom += 32;
        }
        pFrom = aTo;
        aTo = aFrom;
        aFrom = pFrom;
        nFrom = nTo;
        goto _4;
_4:;
        iLoop = iLoop + 1;
    }
    if (nFrom == 0) {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24252, 0);
        _sqlite3DbFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pSpace);
        return (int32)SQLITE_ERROR;
    }
    pFrom = aFrom;
    iLoop = 0;
    for (;;) {
        if (!(iLoop < nLoop)) {
            break;
        }
        pLevel = pWInfo + 856 + (uintptr)iLoop * 112;
        v11 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop + (uintptr)iLoop * 8));
        pWLoop = v11;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop = v11;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FiTab;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiTabCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiFrom * 80))).Value.FiCursor;
        goto _10;
_10:;
        iLoop = iLoop + 1;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_DISTINCTBY) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct) == WHERE_DISTINCT_NOOP && nRowEst != 0) {
        rc = (int32)_wherePathSatisfiesOrderBy(tls, pWInfo, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpResultSet, pFrom, (uint16)WHERE_DISTINCTBY, libc.Uint16FromInt32(nLoop - (int32)1), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop + (uintptr)(nLoop - (int32)1) * 8)), @unsafe.Pointer.FromPinnedBox(ᏑnotUsed));
        if (rc == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpResultSet)).Value.FnExpr) {
            (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct = (uint8)WHERE_DISTINCT_ORDERED;
        }
    }
    libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(0), 2, 0x4);
    if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy != 0) {
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FisOrdered;
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_DISTINCTBY) != 0) {
            if ((int32)(@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FisOrdered == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy)).Value.FnExpr) {
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct = (uint8)WHERE_DISTINCT_ORDERED;
            }
        } else {
            (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FrevLoop;
            if ((int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat <= 0) {
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat = 0;
                if (nLoop > 0) {
                    wsFlags = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop + (uintptr)(nLoop - (int32)1) * 8))))).Value.FwsFlags;
                    if ((Tu32)(wsFlags & (uint32)WHERE_ONEROW) == (uint32)0 && (Tu32)(wsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_IPK) | libc.Int32FromInt32(WHERE_COLUMN_IN))) != libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_IPK) | libc.Int32FromInt32(WHERE_COLUMN_IN))) {
                        m = (uint64)0;
                        rc1 = (int32)_wherePathSatisfiesOrderBy(tls, pWInfo, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy, pFrom, (uint16)WHERE_ORDERBY_LIMIT, libc.Uint16FromInt32(nLoop - (int32)1), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop + (uintptr)(nLoop - (int32)1) * 8)), @unsafe.Pointer.FromPinnedBox(Ꮡm));
                        if (rc1 == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy)).Value.FnExpr) {
                            libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 2, 0x4);
                            (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask = m;
                        }
                    }
                }
            } else {
                if (nLoop != 0 && (int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat == (int32)1 && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (libc.Int32FromInt32(WHERE_ORDERBY_MIN) | libc.Int32FromInt32(WHERE_ORDERBY_MAX))) != 0) {
                    libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 2, 0x4);
                }
            }
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_SORTBYGROUP) != 0 && (int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy)).Value.FnExpr && nLoop > 0) {
            revMask1 = (uint64)0;
            nOrder = (int32)_wherePathSatisfiesOrderBy(tls, pWInfo, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy, pFrom, (uint16)0, libc.Uint16FromInt32(nLoop - (int32)1), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FaLoop + (uintptr)(nLoop - (int32)1) * 8)), @unsafe.Pointer.FromPinnedBox(ᏑrevMask1));
            if (nOrder == (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy)).Value.FnExpr) {
                libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 3, 0x8);
                (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask = revMask1;
            }
        }
    }
    (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnRowOut = (@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pFrom)).Value.FnRow;
    _sqlite3DbFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pSpace);
    return SQLITE_OK;
}

} // end sqlite3_package
