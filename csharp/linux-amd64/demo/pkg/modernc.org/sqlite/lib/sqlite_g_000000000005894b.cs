global using TColumn = go.modernc.org.sqlite.lib.sqlite3_package.TColumnᴛ1;
global using TExpr = go.modernc.org.sqlite.lib.sqlite3_package.TExprᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using System.Runtime.InteropServices;
using go.golib;
using go.modernc.org;

partial class sqlite3_package {

partial struct TColumnᴛ1 /*dyn*/ {
    public uintptr FzCnName;
    public uint8 F__ccgo8;
    public int8 Faffinity;
    public Tu8 FszEst;
    public Tu8 FhName;
    public Tu16 FiDflt;
    public Tu16 FcolFlags;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TExprᴛ1_Fu /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int FiValue;
    [FieldOffset(0)] public uintptr FzToken;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TExprᴛ1_Fx /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpSelect;
    [FieldOffset(0)] public uintptr FpList;
}

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] partial struct TExprᴛ1_Fw /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int FiOfst;
    [FieldOffset(0)] public int32 FiJoin;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 4), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TExprᴛ1_Fy_Fsub /*dyn*/ {
    [FieldOffset(0)] public int32 FiAddr;
    [FieldOffset(4)] public int32 FregReturn;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct TExprᴛ1_Fy /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uintptr>))] public readonly long FpWin;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<int32>))] public readonly int FnReg;
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<TExprᴛ1_Fy_Fsub>))] public readonly int Fsub;
    [FieldOffset(0)] public uintptr FpTab;
}

[global::go.golib.GoInlineLayout(Size = 72, Align = 8), StructLayout(LayoutKind.Explicit, Size = 72)] partial struct TExprᴛ1 /*dyn*/ {
    [FieldOffset(0)] public Tu8 Fop;
    [FieldOffset(1)] public int8 FaffExpr;
    [FieldOffset(2)] public Tu8 Fop2;
    [FieldOffset(4)] public Tu32 Fflags;
    [FieldOffset(8)] public TExprᴛ1_Fu Fu;
    [FieldOffset(16)] public uintptr FpLeft;
    [FieldOffset(24)] public uintptr FpRight;
    [FieldOffset(32)] public TExprᴛ1_Fx Fx;
    [FieldOffset(40)] public int32 FnHeight;
    [FieldOffset(44)] public int32 FiTable;
    [FieldOffset(48)] public TynVar FiColumn;
    [FieldOffset(50)] public Ti16 FiAgg;
    [FieldOffset(52)] public TExprᴛ1_Fw Fw;
    [FieldOffset(56)] public uintptr FpAggInfo;
    [FieldOffset(64)] public TExprᴛ1_Fy Fy;
}

public static uintptr /*r*/ Xsqlite3_database_file_object(ж<libc.TLS> tls, uintptr zName) {
    uintptr p = default;
    uintptr pPager = default;
    _ = p;
    _ = pPager;
    while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(1))))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(2))))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(3))))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)(-libc.Int32FromInt32(4))))) != 0) {
        zName = zName - 1;
    }
    p = zName - (uintptr)4 - (uintptr)8;
    pPager = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p));
    return (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd;
}

public static uintptr /*r*/ Xsqlite3_vmprintf(ж<libc.TLS> tls, uintptr zFormat, Tva_list ap) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(112);
        defer(tls.Free, (nint)112, ref ᒐ);
        uintptr z = default;
        TStrAccum _ᴛ1 = default;
        array<int8> _ᴛ2 = new(70);
        _ = z;
        if (Xsqlite3_initialize(tls) != 0) {
            r = (uintptr)0; goto ᒐdone;
        }
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 72), (uintptr)0, bp, (int32)70, (int32)SQLITE_MAX_LENGTH);
        Xsqlite3_str_vappendf(tls, bp + 72, zFormat, ap);
        z = _sqlite3StrAccumFinish(tls, __ccgo_ptr(bp + 72));
        r = z;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int8 /*r*/ _comparisonAffinity(ж<libc.TLS> tls, @unsafe.Pointer pExpr) {
    int8 aff = default;
    _ = aff;
    aff = _sqlite3ExprAffinity(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pExpr, 16)));
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pExpr, 24)) != 0) {
        aff = _sqlite3CompareAffinity(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pExpr, 24)), aff);
    } else {
        if ((Tu32)(~@unsafe.Pointer.To<Tu32>(@unsafe.Add(pExpr, 4)) & (uint32)EP_xIsSelect) != (uint32)0) {
            aff = _sqlite3CompareAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pExpr, 32))))).Value.FpEList + 8))).Value.FpExpr, aff);
        } else {
            if ((int32)aff == 0) {
                aff = (int8)SQLITE_AFF_BLOB;
            }
        }
    }
    return aff;
}

internal static int32 /*r*/ _compoundHasDifferentAffinities(ж<libc.TLS> tls, uintptr p) {
    int8 aff = default;
    int32 ii = default;
    uintptr pList = default;
    uintptr pSub1 = default;
    _ = aff;
    _ = ii;
    _ = pList;
    _ = pSub1;
    pList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
            break;
        }
        aff = _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ii * 32))).Value.FpExpr);
        pSub1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior;
        for (;;) {
            if (!(pSub1 != 0)) {
                break;
            }
            if ((int32)_sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpEList + 8 + (uintptr)ii * 32))).Value.FpExpr) != (int32)aff) {
                return (int32)1;
            }
            goto _2;
_2:;
            pSub1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub1)).Value.FpPrior;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    return 0;
}

internal static int32 /*r*/ _exprListIsNoAffinity(ж<libc.TLS> tls, uintptr pParse, uintptr pRow) {
    int32 ii = default;
    uintptr pExpr = default;
    _ = ii;
    _ = pExpr;
    if (_sqlite3ExprListIsConstant(tls, pParse, pRow, 0) == 0) {
        return 0;
    }
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pRow)).Value.FnExpr)) {
            break;
        }
        pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pRow + 8 + (uintptr)ii * 32))).Value.FpExpr;
        if (0 != (int32)_sqlite3ExprAffinity(tls, pExpr)) {
            return 0;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    return (int32)1;
}

internal static uintptr /*r*/ _exprTableRegister(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 regBase, Ti16 iCol) {
    uintptr db = default;
    uintptr pCol = default;
    uintptr pExpr = default;
    uintptr zColl = default;
    _ = db;
    _ = pCol;
    _ = pExpr;
    _ = zColl;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pExpr = _sqlite3Expr(tls, db, (int32)TK_REGISTER, (uintptr)0);
    if (pExpr != 0) {
        if ((int32)iCol >= 0 && (int32)iCol != (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
            pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16;
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable = regBase + (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), pTab, iCol) + (int32)1;
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FaffExpr = (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.Faffinity;
            zColl = _sqlite3ColumnColl(ref (tls).DerefOrNull(), pCol);
            if (zColl == (uintptr)0) {
                zColl = (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDfltColl)).Value.FzName;
            }
            pExpr = _sqlite3ExprAddCollateString(tls, pParse, pExpr, zColl);
        } else {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable = regBase;
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FaffExpr = (int8)SQLITE_AFF_INTEGER;
        }
    }
    return pExpr;
}

internal static void _jsonAppendChar(ж<libc.TLS> tls, uintptr p, int8 c) {
    Tu64 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed >= (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc) {
        _jsonAppendCharExpand(tls, p, c);
    } else {
        v2 = p + 24;
        v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = c;
    }
}

internal static void _jsonAppendCharExpand(ж<libc.TLS> tls, uintptr p, int8 c) {
    Tu64 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if (_jsonStringGrow(tls, p, (uint32)1) != 0) {
        return;
    }
    v2 = p + 24;
    v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = c;
}

internal static void _jsonArrayFunc(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        int32 i = default;
        TJsonString _ᴛ1 = new();
        _ = i;
        _jsonStringInit(ref (tls).DerefOrNull(), bp, ctx);
        _jsonAppendChar(tls, bp, (int8)'[');
        i = 0;
        for (;;) {
            if (!(i < argc)) {
                break;
            }
            _jsonAppendSeparator(tls, bp);
            _jsonAppendSqlValue(tls, bp, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        _jsonAppendChar(tls, bp, (int8)']');
        _jsonReturnString(tls, bp, (uintptr)0, (uintptr)0);
        Xsqlite3_result_subtype(tls, ctx, (uint32)JSON_SUBTYPE);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _jsonArrayStep(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    uintptr pStr = default;
    _ = pStr;
    _ = argc;
    pStr = Xsqlite3_aggregate_context(tls, ctx, (int32)136);
    if (pStr != 0) {
        if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FzBuf == (uintptr)0) {
            _jsonStringInit(ref (tls).DerefOrNull(), pStr, ctx);
            _jsonAppendChar(tls, pStr, (int8)'[');
        } else {
            if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed > (uint64)1) {
                _jsonAppendChar(tls, pStr, (int8)',');
            }
        }
        (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FpCtx = ctx;
        _jsonAppendSqlValue(tls, pStr, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    }
}

internal static int32 /*r*/ _rbuStep(ж<libc.TLS> tls, uintptr p) {
    int8 c = default;
    int32 eType = default;
    int32 i = default;
    uintptr pIter = default;
    ref var pUpdate = ref heap(new uintptr(), out var ᏑpUpdate);
    uintptr pVal = default;
    ref var zMask = ref heap(new uintptr(), out var ᏑzMask);
    _ = c;
    _ = eType;
    _ = i;
    _ = pIter;
    _ = pUpdate;
    _ = pVal;
    _ = zMask;
    pIter = p + 88;
    zMask = (uintptr)0;
    eType = _rbuStepType(tls, p, @unsafe.Pointer.FromBox(ᏑzMask));
    if (eType != 0) {
        if ((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FzIdx == (uintptr)0 && (eType == (int32)RBU_IDX_DELETE || eType == (int32)RBU_IDX_INSERT)) {
            _rbuBadControlError(tls, p);
        } else {
            if (eType == (int32)RBU_REPLACE) {
                if ((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FzIdx == (uintptr)0) {
                    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p + 312))).Value += (int64)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fobjiter.FnIndex;
                    _rbuStepOneOp(tls, p, (int32)RBU_DELETE);
                }
                if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                    _rbuStepOneOp(tls, p, (int32)RBU_INSERT);
                }
            } else {
                if (eType != (int32)RBU_UPDATE) {
                    _rbuStepOneOp(tls, p, eType);
                } else {
                    pUpdate = (uintptr)0;
                    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p + 312))).Value -= (int64)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fobjiter.FnIndex;
                    _rbuGetUpdateStmt(tls, p, pIter, zMask, @unsafe.Pointer.FromBox(ᏑpUpdate));
                    if (pUpdate != 0) {
                        i = 0;
                        for (;;) {
                            if (!((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && i < (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FnCol)) {
                                break;
                            }
                            c = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zMask + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FaiSrcOrder + (uintptr)i * 4)))));
                            pVal = Xsqlite3_column_value(tls, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FpSelect, i);
                            if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FabTblPk + (uintptr)i)) != 0 || (int32)c != (int32)'.') {
                                (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3_bind_value(tls, pUpdate, i + (int32)1, pVal);
                            }
                            goto _1;
_1:;
                            i = i + 1;
                        }
                        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && ((@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FeType == (int32)RBU_PK_VTAB || (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FeType == (int32)RBU_PK_NONE)) {
                            pVal = Xsqlite3_column_value(tls, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FpSelect, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FnCol + (int32)1);
                            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3_bind_value(tls, pUpdate, (@unsafe.Pointer.To<TRbuObjIter>((@unsafe.Pointer)pIter)).Value.FnCol + (int32)1, pVal);
                        }
                        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                            Xsqlite3_step(tls, pUpdate);
                            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = _resetAndCollectError(tls, pUpdate, p + 64);
                        }
                    }
                }
            }
        }
    }
    return (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc;
}

internal static void _renderLogMsg(ж<libc.TLS> tls, int32 iErrCode, uintptr zFormat, Tva_list ap) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(736);
        defer(tls.Free, (nint)736, ref ᒐ);
        TStrAccum _ᴛ1 = default;
        array<int8> _ᴛ2 = new(700);
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp), (uintptr)0, bp + 32, (int32)700, 0);
        Xsqlite3_str_vappendf(tls, bp, zFormat, ap);
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, int32, uintptr>>(_sqlite3Config.FxLog)(tls, _sqlite3Config.FpLogArg, iErrCode, _sqlite3StrAccumFinish(tls, __ccgo_ptr(bp)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _selectAddSubqueryTypeInfo(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 i = default;
    uintptr pFrom = default;
    uintptr pParse = default;
    uintptr pSel = default;
    uintptr pTab = default;
    uintptr pTabList = default;
    _ = i;
    _ = pFrom;
    _ = pParse;
    _ = pSel;
    _ = pTab;
    _ = pTabList;
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_HasTypeInfo) != 0) {
        return;
    }
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_HasTypeInfo;
    pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
    pTabList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
    i = 0;
    pFrom = pTabList + 8;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc)) {
            break;
        }
        pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFrom)).Value.FpSTab;
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Ephemeral) != (uint32)0 && (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pFrom + 24 + 4)) & 0x4) >> 2) != 0) {
            pSel = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 72))))).Value.FpSelect;
            _sqlite3SubqueryColumnTypes(tls, pParse, pTab, pSel, (int8)SQLITE_AFF_NONE);
        }
        goto _1;
_1:;
        i = i + 1;
        pFrom += 80;
    }
}

internal static void _sessionAppendIdent(ж<libc.TLS> tls, uintptr p, uintptr zStr, @unsafe.Pointer pRc) {
    int32 nStr = default;
    uintptr zIn = default;
    uintptr zOut = default;
    uintptr v1 = default;
    uintptr v2 = default;
    _ = nStr;
    _ = zIn;
    _ = zOut;
    _ = v1;
    _ = v2;
    nStr = _sqlite3Strlen30(tls, zStr) * (int32)2 + (int32)2 + (int32)2;
    if (0 == _sessionBufferGrow(tls, (@unsafe.Pointer)p, (int64)nStr, pRc)) {
        zOut = (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf;
        zIn = zStr;
        v1 = zOut;
        zOut = zOut + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)'"';
        if (zIn != (uintptr)0) {
            while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(zIn)) != 0) {
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zIn))) == (int32)'"') {
                    v1 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)'"';
                }
                v1 = zOut;
                zOut = zOut + 1;
                v2 = zIn;
                zIn = zIn + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(v2))).Value;
            }
        }
        v1 = zOut;
        zOut = zOut + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)'"';
        (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf = (int32)((int64)zOut - (int64)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf);
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf))).Value = (uint8)0x00;
    }
}

internal static void _sqlite3ColumnDefault(ж<libc.TLS> tls, uintptr v, uintptr pTab, int32 i, int32 iReg) {
    uintptr pCol = default;
    _ = pCol;
    pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16;
    if ((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FiDflt != 0) {
        _columnDefaultUncommonCase(tls, v, pTab, pCol);
    }
    if ((int32)(@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.Faffinity == (int32)SQLITE_AFF_REAL) {
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_RealAffinity, iReg);
    }
}

internal static void _sqlite3InsertBuiltinFuncs(ж<libc.TLS> tls, uintptr aDef, int32 nDef) {
    int32 h = default;
    int32 i = default;
    int32 nName = default;
    uintptr pOther = default;
    uintptr zName = default;
    _ = h;
    _ = i;
    _ = nName;
    _ = pOther;
    _ = zName;
    i = 0;
    for (;;) {
        if (!(i < nDef)) {
            break;
        }
        zName = (@unsafe.Pointer.To<TFuncDef>(__ccgo_ptr(aDef + (uintptr)i * 72))).Value.FzName;
        nName = _sqlite3Strlen30(tls, zName);
        h = ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName))) + nName) % (int32)SQLITE_FUNC_HASH_SZ;
        pOther = _sqlite3FunctionSearch(ref (tls).DerefOrNull(), h, zName);
        if (pOther != 0) {
            (@unsafe.Pointer.To<TFuncDef>(__ccgo_ptr(aDef + (uintptr)i * 72))).Value.FpNext = (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pOther)).Value.FpNext;
            (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pOther)).Value.FpNext = aDef + (uintptr)i * 72;
        } else {
            (@unsafe.Pointer.To<TFuncDef>(__ccgo_ptr(aDef + (uintptr)i * 72))).Value.FpNext = (uintptr)0;
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(aDef + (uintptr)i * 72 + 64))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((uintptr)Ꮡ_sqlite3BuiltinFunctions + (uintptr)h * 8))).Value;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((uintptr)Ꮡ_sqlite3BuiltinFunctions + (uintptr)h * 8))).Value = aDef + (uintptr)i * 72;
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _sqlite3MatchEName(ж<libc.TLS> tls, uintptr pItem, uintptr zCol, uintptr zTab, uintptr zDb, @unsafe.Pointer pbRowid) {
    int32 eEName = default;
    int32 n = default;
    uintptr zSpan = default;
    _ = eEName;
    _ = n;
    _ = zSpan;
    eEName = (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pItem + 16 + 4)) & 0x3) >> 0);
    if (eEName != (int32)ENAME_TAB && (eEName != (int32)ENAME_ROWID || pbRowid == nil)) {
        return 0;
    }
    zSpan = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FzEName;
    n = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSpan + (uintptr)n)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSpan + (uintptr)n))) != (int32)'.')) {
            break;
        }
        goto _1;
_1:;
        n = n + 1;
    }
    if (zDb != 0 && (Xsqlite3_strnicmp(tls, zSpan, zDb, n) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDb + (uintptr)n))) != 0)) {
        return 0;
    }
    zSpan = zSpan + (uintptr)(n + (int32)1);
    n = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSpan + (uintptr)n)) != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSpan + (uintptr)n))) != (int32)'.')) {
            break;
        }
        goto _2;
_2:;
        n = n + 1;
    }
    if (zTab != 0 && (Xsqlite3_strnicmp(tls, zSpan, zTab, n) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zTab + (uintptr)n))) != 0)) {
        return 0;
    }
    zSpan = zSpan + (uintptr)(n + (int32)1);
    if (zCol != 0) {
        if (eEName == (int32)ENAME_TAB && _sqlite3StrICmp(ref (tls).DerefOrNull(), zSpan, zCol) != 0) {
            return 0;
        }
        if (eEName == (int32)ENAME_ROWID && _sqlite3IsRowid(ref (tls).DerefOrNull(), zCol) == 0) {
            return 0;
        }
    }
    if (eEName == (int32)ENAME_ROWID) {
        (@unsafe.Pointer.To<int32>(pbRowid)).Value = (int32)1;
    }
    return (int32)1;
}

internal static int32 /*r*/ _sqlite3Prepare16(ж<libc.TLS> tls, uintptr db, uintptr zSql, int32 nBytes, Tu32 prepFlags, uintptr ppStmt, uintptr pzTail) {
    int32 chars_parsed = default;
    int32 rc = default;
    int32 sz = default;
    int32 sz1 = default;
    uintptr z = default;
    uintptr z1 = default;
    uintptr zSql8 = default;
    ref var zTail8 = ref heap(new uintptr(), out var ᏑzTail8);
    _ = chars_parsed;
    _ = rc;
    _ = sz;
    _ = sz1;
    _ = z;
    _ = z1;
    _ = zSql8;
    _ = zTail8;
    zTail8 = (uintptr)0;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppStmt))).Value = (uintptr)0;
    if (!(_sqlite3SafetyCheckOk(tls, db) != 0) || zSql == (uintptr)0) {
        return _sqlite3MisuseError(tls, (int32)149874);
    }
    if (nBytes >= 0) {
        z = zSql;
        sz = 0;
        for (;;) {
            if (!(sz < nBytes && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)sz))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(sz + (int32)1)))) != 0))) {
                break;
            }
            goto _1;
_1:;
            sz = sz + (int32)2;
        }
        nBytes = sz;
    } else {
        z1 = zSql;
        sz1 = 0;
        for (;;) {
            if (!((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z1 + (uintptr)sz1))) != 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z1 + (uintptr)(sz1 + (int32)1)))) != 0)) {
                break;
            }
            goto _2;
_2:;
            sz1 = sz1 + (int32)2;
        }
        nBytes = sz1;
    }
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    zSql8 = _sqlite3Utf16to8(tls, db, zSql, nBytes, (uint8)SQLITE_UTF16LE);
    if (zSql8 != 0) {
        rc = _sqlite3LockAndPrepare(tls, db, zSql8, -(int32)1, prepFlags, (uintptr)0, (@unsafe.Pointer)ppStmt, @unsafe.Pointer.FromBox(ᏑzTail8));
    }
    if (zTail8 != 0 && pzTail != 0) {
        chars_parsed = _sqlite3Utf8CharLen(ref (tls).DerefOrNull(), zSql8, (int32)((int64)zTail8 - (int64)zSql8));
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzTail))).Value = zSql + (uintptr)_sqlite3Utf16ByteLen(ref (tls).DerefOrNull(), zSql, nBytes, chars_parsed);
    }
    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zSql8);
    rc = _sqlite3ApiExit(tls, db, rc);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

internal static uintptr /*r*/ _sqlite3ResultSetOfSelect(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect, int8 aff) {
    uintptr db = default;
    uintptr pTab = default;
    Tu64 savedFlags = default;
    _ = db;
    _ = pTab;
    _ = savedFlags;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel + 1;
    if (_checkSubqueryNestingDepth(tls, pParse) != 0) {
        return (uintptr)0;
    }
    savedFlags = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags;
    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value &= ~libc.Uint64FromInt32(SQLITE_FullColNames);
    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value |= (uint64)SQLITE_ShortColNames;
    _sqlite3SelectPrep(tls, pParse, pSelect, (uintptr)0);
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags = savedFlags;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
        return (uintptr)0;
    }
    while ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior != 0) {
        pSelect = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior;
    }
    pTab = _sqlite3DbMallocZero(tls, db, (uint64)120);
    if (pTab == (uintptr)0) {
        return (uintptr)0;
    }
    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnTabRef = (uint32)1;
    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName = (uintptr)0;
    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnRowLogEst = (int16)200;
    _sqlite3ColumnsFromExprList(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList, pTab + 54, pTab + 8);
    _sqlite3SubqueryColumnTypes(tls, pParse, pTab, pSelect, aff);
    (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey = (int16)(-(int32)1);
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
        _sqlite3DeleteTable(tls, (@unsafe.Pointer)db, pTab);
        return (uintptr)0;
    }
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnNestSel - 1;
    return pTab;
}

internal static int8 /*r*/ _sqlite3TableColumnAffinity(ref libc.TLS tls, uintptr pTab, int32 iCol) {
    if (iCol < 0 || iCol >= (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol) {
        return (int8)SQLITE_AFF_INTEGER;
    }
    return (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol * 16))).Value.Faffinity;
}

internal static int32 /*r*/ _unicodeSetCategories(ref libc.TLS tls, uintptr p, uintptr zCat) {
    uintptr z = default;
    _ = z;
    z = zCat;
    while (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0) {
        while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) == (int32)' ' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) == (int32)'\t') {
            z = z + 1;
        }
        if (~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)) != 0 && _sqlite3Fts5UnicodeCatParse(ref tls, z, p + 160) != 0) {
            return (int32)SQLITE_ERROR;
        }
        while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)' ' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)'\t' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)'\u0000') {
            z = z + 1;
        }
    }
    _sqlite3Fts5UnicodeAscii(ref tls, p + 160, p);
    return SQLITE_OK;
}

internal static int32 /*r*/ _vdbeMemAddTerminator(ж<libc.TLS> tls, uintptr pMem) {
    uintptr v1 = default;
    _ = v1;
    if (_sqlite3VdbeMemGrow(tls, pMem, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn + (int32)3, (int32)1) != 0) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn))).Value = 0;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn + (int32)1)))).Value = 0;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn + (int32)2)))).Value = 0;
    v1 = pMem + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Term));
    return SQLITE_OK;
}

internal static uintptr /*r*/ _whereScanInit(ж<libc.TLS> tls, @unsafe.Pointer pScan, uintptr pWC, int32 iCur, int32 iColumn, Tu32 opMask, uintptr pIdx) {
    int32 j = default;
    _ = j;
    (@unsafe.Pointer.To<uintptr>(pScan)).Value = pWC;
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 8))).Value = pWC;
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 24))).Value = (uintptr)0;
    (@unsafe.Pointer.To<int8>(@unsafe.Add(pScan, 40))).Value = 0;
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 16))).Value = (uintptr)0;
    (@unsafe.Pointer.To<Tu32>(@unsafe.Add(pScan, 36))).Value = opMask;
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pScan, 32))).Value = 0;
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pScan, 44))).Value = iCur;
    (@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42))).Value = (uint8)1;
    (@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41))).Value = (uint8)1;
    if (pIdx != 0) {
        j = iColumn;
        iColumn = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)j * 2)));
        if (iColumn == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FiPKey) {
            iColumn = -(int32)1;
        } else {
            if (iColumn >= 0) {
                (@unsafe.Pointer.To<int8>(@unsafe.Add(pScan, 40))).Value = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FaCol + (uintptr)iColumn * 16))).Value.Faffinity;
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 16))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)j * 8))).Value;
            } else {
                if (iColumn == -(int32)2) {
                    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 24))).Value = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr + 8 + (uintptr)j * 32))).Value.FpExpr;
                    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 16))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)j * 8))).Value;
                    (@unsafe.Pointer.To<Ti16>(@unsafe.Add(pScan, 88))).Value = (int16)(-libc.Int32FromInt32(2));
                    return _whereScanInitIndexExpr(tls, pScan);
                }
            }
        }
    } else {
        if (iColumn == -(int32)2) {
            return (uintptr)0;
        }
    }
    (@unsafe.Pointer.To<Ti16>(@unsafe.Add(pScan, 88))).Value = (int16)iColumn;
    return _whereScanNext(tls, pScan);
}

internal static uintptr /*r*/ _whereScanInitIndexExpr(ж<libc.TLS> tls, @unsafe.Pointer pScan) {
    (@unsafe.Pointer.To<int8>(@unsafe.Add(pScan, 40))).Value = _sqlite3ExprAffinity(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 24)));
    return _whereScanNext(tls, pScan);
}

} // end sqlite3_package
