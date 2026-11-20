namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal partial struct _codeReturningTrigger__ᴛ3 /*dyn*/ {
    public /*[0][88]*/ array<array<Tu8>> FfromSpace = new(0, () => new(88));
    public TSrcList FsSrc;
    public array<byte> F__ccgo_pad2 = new(80);
}

internal static void _codeReturningTrigger(ж<libc.TLS> tls, uintptr pParse, uintptr pTrigger, uintptr pTab, int32 regIn) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(272);
        defer(tls.Free, (nint)272, ref ᒐ);
        uintptr db = default;
        uintptr pCol = default;
        uintptr pFrom = default;
        uintptr pNew = default;
        uintptr pReturning = default;
        uintptr v = default;
        uintptr v2 = default;
        int32 i = default;
        int32 nCol = default;
        int32 reg = default;
        int32 v1 = default;
        TNameContext _ᴛ1 = new();
        TSelect _ᴛ2 = default;
        _codeReturningTrigger__ᴛ3 _ᴛ3 = new();
        _ = db;
        _ = i;
        _ = nCol;
        _ = pCol;
        _ = pFrom;
        _ = pNew;
        _ = pReturning;
        _ = reg;
        _ = v;
        _ = v1;
        _ = v2;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if (!((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x8) >> 3)) != 0)) {
            return;
        }
        pReturning = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse).of(TParse.ᏑFu1).Reinterpret<TParseᴛ1_Fu1, _sqlite3AddReturning_type>()).Value.FpReturning;
        if (pTrigger != pReturning + 16) {
            return;
        }
        libc.Xmemset(tls, bp, 0, (uint64)120);
        libc.Xmemset(tls, bp + 120, 0, (uint64)88);
        pFrom = bp + 120;
        (@unsafe.Pointer.To<TSelect>(__ccgo_ptr(bp))).Value.FpEList = _sqlite3ExprListDup(tls, db, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FpReturnEL, 0);
        (@unsafe.Pointer.To<TSelect>(__ccgo_ptr(bp))).Value.FpSrc = pFrom;
        (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pFrom)).Value.FnSrc = (int32)1;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pFrom + 8))).Value.FpSTab = pTab;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pFrom + 8))).Value.FzName = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName;
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pFrom + 8))).Value.FiCursor = -(int32)1;
        _sqlite3SelectPrep(tls, pParse, bp, (uintptr)0);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
            _sqlite3GenerateColumnNames(tls, pParse, __ccgo_ptr(bp));
        }
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TSelect>(__ccgo_ptr(bp))).Value.FpEList);
        pNew = _sqlite3ExpandReturning(tls, pParse, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FpReturnEL, pTab);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
            libc.Xmemset(tls, bp + 208, 0, (uint64)56);
            if ((@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FnRetCol == 0) {
                (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FnRetCol = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr;
                v2 = pParse + 56;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetCur = v1;
            }
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 208))).Value.FpParse = pParse;
            (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 208)).of(TNameContext.ᏑFuNC)), 0))).Value = regIn;
            (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 208))).Value.FncFlags = (int32)NC_UBaseReg;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeTriggerOp = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.Fop;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab = pTab;
            if (_sqlite3ResolveExprListNames(tls, bp + 208, pNew) == SQLITE_OK && !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                nCol = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr;
                reg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                _sqlite3ProcessReturningSubqueries(tls, pNew, pTab);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nCol + (int32)2;
                (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetReg = reg;
                i = 0;
                for (;;) {
                    if (!(i < nCol)) {
                        break;
                    }
                    pCol = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pNew + 8 + (uintptr)i * 32))).Value.FpExpr;
                    _sqlite3ExprCodeFactorable(tls, pParse, pCol, reg + i);
                    if ((int32)_sqlite3ExprAffinity(tls, pCol) == (int32)SQLITE_AFF_REAL) {
                        _sqlite3VdbeAddOp1(tls, v, (int32)OP_RealAffinity, reg + i);
                    }
                    goto _3;
_3:;
                    i = i + 1;
                }
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, reg, i, reg + i);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetCur, reg + i + (int32)1);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetCur, reg + i, reg + i + (int32)1);
            }
        }
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pNew);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeTriggerOp = (uint8)0;
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpTriggerTab = (uintptr)0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _isDate(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv, uintptr p) {
    int32 eType = default;
    int32 i = default;
    int32 n = default;
    int32 v1 = default;
    uintptr z = default;
    _ = eType;
    _ = i;
    _ = n;
    _ = z;
    _ = v1;
    libc.Xmemset(tls, p, 0, (uint64)48);
    if (argc == 0) {
        if (!(_sqlite3NotPureFunc(tls, context) != 0)) {
            return (int32)1;
        }
        return _setDateTimeToCurrent(tls, context, p);
    }
    v1 = Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    eType = v1;
    if (v1 == (int32)SQLITE_FLOAT || eType == (int32)SQLITE_INTEGER) {
        _setRawDateNumber(ref (tls).DerefOrNull(), p, Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
    } else {
        z = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (!(z != 0) || _parseDateOrTime(tls, context, z, p) != 0) {
            return (int32)1;
        }
    }
    i = (int32)1;
    for (;;) {
        if (!(i < argc)) {
            break;
        }
        z = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
        n = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
        if (z == (uintptr)0 || _parseModifier(tls, context, z, n, p, i) != 0) {
            return (int32)1;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    _computeJD(tls, p);
    if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(p + 44)) & 0x2) >> 1) != 0 || !(_validJulianDay(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FiJD) != 0)) {
        return (int32)1;
    }
    if (argc == (int32)1 && (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD != 0 && (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FD > (int32)28) {
        (@unsafe.Pointer.To<TDateTime>((@unsafe.Pointer)p)).Value.FvalidYMD = 0;
    }
    return 0;
}

internal static void _jsonGroupInverse(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    int8 c = default;
    int8 v2 = default;
    uint32 i = default;
    int32 inStr = default;
    int32 nNest = default;
    uintptr pStr = default;
    uintptr z = default;
    bool v3 = default;
    _ = c;
    _ = i;
    _ = inStr;
    _ = nNest;
    _ = pStr;
    _ = z;
    _ = v2;
    _ = v3;
    inStr = 0;
    nNest = 0;
    _ = argc;
    _ = argv;
    pStr = Xsqlite3_aggregate_context(tls, ctx, 0);
    if (!(pStr != 0)) {
        return;
    }
    z = (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FzBuf;
    i = (uint32)1;
    for (;;) {
        v3 = (uint64)i < (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed; if (v3) {
            v2 = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i));
            c = v2;
        }
        if (!(v3 && ((int32)v2 != (int32)',' || inStr != 0 || nNest != 0))) {
            break;
        }
        if ((int32)c == (int32)'"') {
            inStr = libc.BoolInt32(!(inStr != 0));
        } else {
            if ((int32)c == (int32)'\\') {
                i = i + 1;
            } else {
                if (!(inStr != 0)) {
                    if ((int32)c == (int32)'{' || (int32)c == (int32)'[') {
                        nNest = nNest + 1;
                    }
                    if ((int32)c == (int32)'}' || (int32)c == (int32)']') {
                        nNest = nNest - 1;
                    }
                }
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if ((uint64)i < (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed) {
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pStr + 24))).Value -= (uint64)i;
        libc.Xmemmove(tls, z + 1, z + (uintptr)(i + (uint32)1), (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed - (uint64)1);
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed))).Value = 0;
    } else {
        (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed = (uint64)1;
    }
}

internal static uintptr /*r*/ _sqlite3DbStrNDup(ж<libc.TLS> tls, uintptr db, uintptr z, Tu64 n) {
    uintptr zNew = default;
    uintptr v1 = default;
    _ = zNew;
    _ = v1;
    if (z != 0) {
        v1 = _sqlite3DbMallocRawNN(tls, db, n + (uint64)1);
    } else {
        v1 = (uintptr)0;
    }
    zNew = v1;
    if (zNew != 0) {
        libc.Xmemcpy(tls, zNew, z, n);
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)n))).Value = 0;
    }
    return zNew;
}

internal static void _sqlite3NestedParse(ж<libc.TLS> tls, uintptr pParse, uintptr zFormat, uintptr va) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(160);
        defer(tls.Free, (nint)160, ref ᒐ);
        Tva_list ap = default;
        uintptr db = default;
        uintptr zSql = default;
        Tu32 savedDbFlags = default;
        array<int8> _ᴛ1 = new(152);
        _ = ap;
        _ = db;
        _ = savedDbFlags;
        _ = zSql;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        savedDbFlags = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            return;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode != 0) {
            return;
        }
        ap = va;
        zSql = _sqlite3VMPrintf(tls, db, zFormat, ap);
        _ = ap;
        if (zSql == (uintptr)0) {
            if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_TOOBIG;
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr + 1;
            return;
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested = (Tu8)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested + 1);
        libc.Xmemcpy(tls, bp, pParse + (uintptr)(uint64)(libc.UintptrFromInt32(0) + 288), libc.Uint64FromInt64(440) - (uint64)(libc.UintptrFromInt32(0) + 288));
        libc.Xmemset(tls, pParse + (uintptr)(uint64)(libc.UintptrFromInt32(0) + 288), 0, libc.Uint64FromInt64(440) - (uint64)(libc.UintptrFromInt32(0) + 288));
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db + 44))).Value |= (uint32)DBFLAG_PreferBuiltin;
        _sqlite3RunParser(tls, pParse, zSql);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags = savedDbFlags;
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zSql);
        libc.Xmemcpy(tls, pParse + (uintptr)(uint64)(libc.UintptrFromInt32(0) + 288), bp, libc.Uint64FromInt64(440) - (uint64)(libc.UintptrFromInt32(0) + 288));
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested = (Tu8)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested - 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
