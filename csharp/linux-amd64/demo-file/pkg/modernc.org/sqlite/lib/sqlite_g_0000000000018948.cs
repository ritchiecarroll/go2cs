namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static uintptr /*r*/ Xsqlite3_create_filename(ж<libc.TLS> tls, uintptr zDatabase, uintptr zJournal, uintptr zWal, int32 nParam, uintptr azParam) {
    int32 i = default;
    Tsqlite3_int64 nByte = default;
    uintptr p = default;
    uintptr pResult = default;
    uintptr v2 = default;
    _ = i;
    _ = nByte;
    _ = p;
    _ = pResult;
    _ = v2;
    nByte = libc.Int64FromUint64(libc.Xstrlen(tls, zDatabase) + libc.Xstrlen(tls, zJournal) + libc.Xstrlen(tls, zWal) + (uint64)10);
    i = 0;
    for (;;) {
        if (!(i < nParam * (int32)2)) {
            break;
        }
        nByte = libc.Int64FromUint64((uint64)nByte + (uint64)(libc.Xstrlen(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azParam + (uintptr)i * 8))) + libc.Uint64FromInt32(1)));
        goto _1;
_1:;
        i = i + 1;
    }
    v2 = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
    p = v2;
    pResult = v2;
    if (p == (uintptr)0) {
        return (uintptr)0;
    }
    libc.Xmemset(tls, p, 0, (uint64)4);
    p = p + (uintptr)4;
    p = _appendText(tls, p, zDatabase);
    i = 0;
    for (;;) {
        if (!(i < nParam * (int32)2)) {
            break;
        }
        p = _appendText(tls, p, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(azParam + (uintptr)i * 8)));
        goto _3;
_3:;
        i = i + 1;
    }
    v2 = p;
    p = p + 1;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v2))).Value = 0;
    p = _appendText(tls, p, zJournal);
    p = _appendText(tls, p, zWal);
    v2 = p;
    p = p + 1;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v2))).Value = 0;
    v2 = p;
    p = p + 1;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v2))).Value = 0;
    return pResult + (uintptr)4;
}

internal static void _concatFuncCore(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv, int32 nSep, uintptr zSep) {
    int32 bNotNull = default;
    int32 i = default;
    int32 k = default;
    Ti64 j = default;
    Ti64 n = default;
    uintptr v = default;
    uintptr z = default;
    _ = bNotNull;
    _ = i;
    _ = j;
    _ = k;
    _ = n;
    _ = v;
    _ = z;
    n = 0;
    bNotNull = 0;
    i = 0;
    for (;;) {
        if (!(i < argc)) {
            break;
        }
        n = n + (int64)Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
        goto _1;
_1:;
        i = i + 1;
    }
    n = n + (int64)(argc - libc.Int32FromInt32(1)) * (int64)nSep;
    z = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(n + (int64)1));
    if (z == (uintptr)0) {
        Xsqlite3_result_error_nomem(tls, context);
        return;
    }
    j = 0;
    i = 0;
    for (;;) {
        if (!(i < argc)) {
            break;
        }
        if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8))) != (int32)SQLITE_NULL) {
            k = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
            v = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
            if (v != (uintptr)0) {
                if (bNotNull != 0 && nSep > 0) {
                    libc.Xmemcpy(tls, z + (uintptr)j, zSep, libc.Uint64FromInt32(nSep));
                    j = j + (int64)nSep;
                }
                libc.Xmemcpy(tls, z + (uintptr)j, v, libc.Uint64FromInt32(k));
                j = j + (int64)k;
                bNotNull = (int32)1;
            }
        }
        goto _2;
_2:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)j))).Value = 0;
    Xsqlite3_result_text64(tls, context, z, libc.Uint64FromInt64(j), __ccgo_fp(Xsqlite3_free), (uint8)SQLITE_UTF8_ZT);
}

internal static void _exprAnalyzeOrTerm(ж<libc.TLS> tls, uintptr pSrc, uintptr pWC, int32 idxTerm) {
    int32 affLeft = default;
    int32 affRight = default;
    int32 i = default;
    int32 iColumn = default;
    int32 iCursor = default;
    int32 iOne = default;
    int32 iTwo = default;
    int32 idxNew = default;
    int32 j = default;
    int32 j1 = default;
    int32 okToChngToIN = default;
    int32 v7 = default;
    int32 v9 = default;
    TBitmask b = default;
    TBitmask b1 = default;
    TBitmask chngToIN = default;
    TBitmask indexable = default;
    uintptr db = default;
    uintptr pAndInfo = default;
    uintptr pAndTerm = default;
    uintptr pAndWC = default;
    uintptr pCollSeq = default;
    uintptr pDup = default;
    uintptr pExpr = default;
    uintptr pLeft = default;
    uintptr pLeft1 = default;
    uintptr pList = default;
    uintptr pNew = default;
    uintptr pOne = default;
    uintptr pOrInfo = default;
    uintptr pOrTerm = default;
    uintptr pOrWc = default;
    uintptr pOther = default;
    uintptr pParse = default;
    uintptr pTerm = default;
    uintptr pThis = default;
    uintptr pTwo = default;
    uintptr pWInfo = default;
    uintptr v1 = default;
    uintptr v2 = default;
    _ = affLeft;
    _ = affRight;
    _ = b;
    _ = b1;
    _ = chngToIN;
    _ = db;
    _ = i;
    _ = iColumn;
    _ = iCursor;
    _ = iOne;
    _ = iTwo;
    _ = idxNew;
    _ = indexable;
    _ = j;
    _ = j1;
    _ = okToChngToIN;
    _ = pAndInfo;
    _ = pAndTerm;
    _ = pAndWC;
    _ = pCollSeq;
    _ = pDup;
    _ = pExpr;
    _ = pLeft;
    _ = pLeft1;
    _ = pList;
    _ = pNew;
    _ = pOne;
    _ = pOrInfo;
    _ = pOrTerm;
    _ = pOrWc;
    _ = pOther;
    _ = pParse;
    _ = pTerm;
    _ = pThis;
    _ = pTwo;
    _ = pWInfo;
    _ = v1;
    _ = v2;
    _ = v7;
    _ = v9;
    pWInfo = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo;
    pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)idxTerm * 56;
    pExpr = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
    v1 = _sqlite3DbMallocZero(tls, db, (uint64)496);
    pOrInfo = v1;
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTerm + 32))).Value = v1;
    if (pOrInfo == (uintptr)0) {
        return;
    }
    v1 = pTerm + 18;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(TERM_ORINFO));
    pOrWc = pOrInfo;
    libc.Xmemset(tls, pOrWc + 40, 0, (uint64)448);
    _sqlite3WhereClauseInit(ref (tls).DerefOrNull(), pOrWc, pWInfo);
    _sqlite3WhereSplit(tls, pOrWc, pExpr, (uint8)TK_OR);
    _sqlite3WhereExprAnalyze(tls, pSrc, pOrWc);
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
        return;
    }
    indexable = ~libc.Uint64FromInt32(0);
    chngToIN = ~libc.Uint64FromInt32(0);
    i = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.FnTerm - (int32)1;
    pOrTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa;
    for (;;) {
        if (!(i >= 0 && indexable != 0)) {
            break;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FeOperator) & (int32)WO_SINGLE) == 0) {
            chngToIN = (uint64)0;
            pAndInfo = _sqlite3DbMallocRawNN(tls, db, (uint64)488);
            if (pAndInfo != 0) {
                b = (uint64)0;
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOrTerm + 32))).Value = pAndInfo;
                v1 = pOrTerm + 18;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(TERM_ANDINFO));
                (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FeOperator = (uint16)WO_AND;
                (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor = -(int32)1;
                pAndWC = pAndInfo;
                libc.Xmemset(tls, pAndWC + 40, 0, (uint64)448);
                _sqlite3WhereClauseInit(ref (tls).DerefOrNull(), pAndWC, (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo);
                _sqlite3WhereSplit(tls, pAndWC, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr, (uint8)TK_AND);
                _sqlite3WhereExprAnalyze(tls, pSrc, pAndWC);
                (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pAndWC)).Value.FpOuter = pWC;
                if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                    j = 0;
                    pAndTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pAndWC)).Value.Fa;
                    for (;;) {
                        if (!(j < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pAndWC)).Value.FnTerm)) {
                            break;
                        }
                        if (_allowedOp(ref (tls).DerefOrNull(), libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAndTerm)).Value.FpExpr)).Value.Fop)) != 0 || libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAndTerm)).Value.FeOperator) == (int32)WO_AUX) {
                            b = (TBitmask)(b | _sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pAndTerm)).Value.FleftCursor));
                        }
                        goto _5;
_5:;
                        j = j + 1;
                        pAndTerm += 56;
                    }
                }
                indexable = (TBitmask)(indexable & b);
            }
        } else {
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FwtFlags) & (int32)TERM_COPIED) != 0) {
            } else {
                b1 = _sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor);
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FwtFlags) & (int32)TERM_VIRTUAL) != 0) {
                    pOther = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FiParent * 56;
                    b1 = (TBitmask)(b1 | _sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOther)).Value.FleftCursor));
                }
                indexable = (TBitmask)(indexable & b1);
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FeOperator) & (int32)WO_EQ) == 0) {
                    chngToIN = (uint64)0;
                } else {
                    chngToIN = (TBitmask)(chngToIN & b1);
                }
            }
        }
        goto _3;
_3:;
        i = i - 1;
        pOrTerm += 56;
    }
    (@unsafe.Pointer.To<TWhereOrInfo>((@unsafe.Pointer)pOrInfo)).Value.Findexable = indexable;
    (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator = (uint16)WO_OR;
    (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FleftCursor = -(int32)1;
    if (indexable != 0) {
        (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FhasOr = (uint8)1;
    }
    if (indexable != 0 && (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.FnTerm == (int32)2) {
        iOne = 0;
        for (;;) {
            v7 = iOne;
            iOne = iOne + 1;
            v1 = _whereNthSubterm(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa, v7);
            pOne = v1;
            if (!(v1 != (uintptr)0)) {
                break;
            }
            iTwo = 0;
            for (;;) {
                v9 = iTwo;
                iTwo = iTwo + 1;
                v2 = _whereNthSubterm(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa + 1 * 56, v9);
                pTwo = v2;
                if (!(v2 != (uintptr)0)) {
                    break;
                }
                _whereCombineDisjuncts(tls, pSrc, pWC, pOne, pTwo);
            }
        }
    }
    if (chngToIN != 0) {
        okToChngToIN = 0;
        iColumn = -(int32)1;
        iCursor = -(int32)1;
        j1 = 0;
        j1 = 0;
        for (;;) {
            if (!(j1 < (int32)2 && !(okToChngToIN != 0))) {
                break;
            }
            pLeft = (uintptr)0;
            pOrTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa;
            i = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.FnTerm - (int32)1;
            for (;;) {
                if (!(i >= 0)) {
                    break;
                }
                v1 = pOrTerm + 18;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(TERM_OK));
                if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor == iCursor) {
                    goto _11;
                }
                if ((TBitmask)(chngToIN & _sqlite3WhereGetMask(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor)) == (uint64)0) {
                    goto _11;
                }
                iColumn = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pOrTerm + 32))).Value.FleftColumn;
                iCursor = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor;
                pLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr)).Value.FpLeft;
                break;
                goto _11;
_11:;
                i = i - 1;
                pOrTerm += 56;
            }
            if (i < 0) {
                break;
            }
            okToChngToIN = (int32)1;
            for (;;) {
                if (!(i >= 0 && okToChngToIN != 0)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor != iCursor) {
                    v1 = pOrTerm + 18;
                    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(TERM_OK));
                } else {
                    if ((@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pOrTerm + 32))).Value.FleftColumn != iColumn || iColumn == -(int32)2 && _sqlite3ExprCompare(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr)).Value.FpLeft, pLeft, -(int32)1) != 0) {
                        okToChngToIN = 0;
                    } else {
                        affRight = (int32)_sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr)).Value.FpRight);
                        affLeft = (int32)_sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr)).Value.FpLeft);
                        if (affRight != 0 && affRight != affLeft) {
                            okToChngToIN = 0;
                        } else {
                            v1 = pOrTerm + 18;
                            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(TERM_OK));
                        }
                    }
                }
                goto _13;
_13:;
                i = i - 1;
                pOrTerm += 56;
            }
            goto _10;
_10:;
            j1 = j1 + 1;
        }
        if (okToChngToIN != 0) {
            pList = (uintptr)0;
            pLeft1 = (uintptr)0;
            pCollSeq = (uintptr)0;
            i = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.FnTerm - (int32)1;
            pOrTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWc)).Value.Fa;
            for (;;) {
                if (!(i >= 0)) {
                    break;
                }
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FwtFlags) & (int32)TERM_OK) == 0) {
                    goto _16;
                }
                pThis = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FpExpr;
                pDup = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pThis)).Value.FpRight, 0);
                pList = _sqlite3ExprListAppend(tls, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse, pList, pDup);
                if (pLeft1 == (uintptr)0) {
                    pLeft1 = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pThis)).Value.FpLeft;
                    pCollSeq = _sqlite3ExprCompareCollSeq(tls, pParse, (@unsafe.Pointer)pThis);
                } else {
                    if (pCollSeq != _sqlite3ExprCompareCollSeq(tls, pParse, (@unsafe.Pointer)pThis)) {
                        pLeft1 = (uintptr)0;
                        break;
                    }
                }
                goto _16;
_16:;
                i = i - 1;
                pOrTerm += 56;
            }
            if (pLeft1 == (uintptr)0) {
                pNew = (uintptr)0;
            } else {
                pDup = _sqlite3ExprDup(tls, db, pLeft1, 0);
                if (_sqlite3ExprCollSeq(tls, pParse, pDup) != pCollSeq && pCollSeq != (uintptr)0) {
                    pDup = _sqlite3ExprAddCollateString(tls, pParse, pDup, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pCollSeq)).Value.FzName);
                }
                pNew = _sqlite3PExpr(tls, pParse, (int32)TK_IN, pDup, (uintptr)0);
            }
            if (pNew != 0) {
                _transferJoinMarkings(ref (tls).DerefOrNull(), pNew, pExpr);
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 32))).Value = pList;
                idxNew = _whereClauseInsert(tls, pWC, pNew, libc.Uint16FromInt32(libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_DYNAMIC)));
                _exprAnalyze(tls, pSrc, pWC, idxNew);
                _markTermAsChild(ref (tls).DerefOrNull(), pWC, idxNew, idxTerm);
            } else {
                _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pList);
            }
        }
    }
}

internal static uintptr /*r*/ _fts5ConfigGobbleWord(ж<libc.TLS> tls, @unsafe.Pointer pRc, uintptr zIn, @unsafe.Pointer pzOut, @unsafe.Pointer pbQuoted) {
    int32 ii = default;
    Tsqlite3_int64 nIn = default;
    uintptr zOut = default;
    uintptr zRet = default;
    _ = ii;
    _ = nIn;
    _ = zOut;
    _ = zRet;
    zRet = (uintptr)0;
    nIn = libc.Int64FromUint64(libc.Xstrlen(tls, zIn));
    zOut = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nIn + (int64)1));
    (@unsafe.Pointer.To<int32>(pbQuoted)).Value = 0;
    (@unsafe.Pointer.To<uintptr>(pzOut)).Value = (uintptr)0;
    if (zOut == (uintptr)0) {
        (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemcpy(tls, zOut, zIn, libc.Uint64FromInt64(nIn + libc.Int64FromInt32(1)));
        if (_fts5_isopenquote(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zOut))) != 0) {
            ii = _fts5Dequote(ref (tls).DerefOrNull(), zOut);
            zRet = zIn + (uintptr)ii;
            (@unsafe.Pointer.To<int32>(pbQuoted)).Value = (int32)1;
        } else {
            zRet = _fts5ConfigSkipBareword(ref (tls).DerefOrNull(), zIn);
            if (zRet != 0) {
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zOut + (uintptr)((int64)zRet - (int64)zIn)))).Value = (int8)'\u0000';
            }
        }
    }
    if (zRet == (uintptr)0) {
        Xsqlite3_free(tls, zOut);
    } else {
        (@unsafe.Pointer.To<uintptr>(pzOut)).Value = zOut;
    }
    return zRet;
}

internal static void _fts5LocaleFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apArg) {
    Ti64 nBlob = default;
    Ti64 nLocale = default;
    Ti64 nText = default;
    uintptr p = default;
    uintptr pBlob = default;
    uintptr pCsr = default;
    uintptr zLocale = default;
    uintptr zText = default;
    uintptr v1 = default;
    _ = nBlob;
    _ = nLocale;
    _ = nText;
    _ = p;
    _ = pBlob;
    _ = pCsr;
    _ = zLocale;
    _ = zText;
    _ = v1;
    zLocale = (uintptr)0;
    nLocale = 0;
    zText = (uintptr)0;
    nText = 0;
    _ = nArg;
    zLocale = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
    nLocale = (int64)Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
    zText = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg + 1 * 8)));
    nText = (int64)Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg + 1 * 8)));
    if (zLocale == (uintptr)0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zLocale))) == (int32)'\u0000') {
        Xsqlite3_result_text(tls, pCtx, zText, (int32)nText, (uintptr)(-libc.Int32FromInt32(1)));
    } else {
        p = Xsqlite3_user_data(tls, pCtx);
        pBlob = (uintptr)0;
        pCsr = (uintptr)0;
        nBlob = 0;
        nBlob = (int64)libc.Int32FromInt64(16) + nLocale + (int64)1 + nText;
        pBlob = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nBlob));
        if (pBlob == (uintptr)0) {
            Xsqlite3_result_error_nomem(tls, pCtx);
            return;
        }
        pCsr = pBlob;
        libc.Xmemcpy(tls, pCsr, p + 96, libc.Uint64FromInt32(libc.Int32FromInt64(16)));
        pCsr = pCsr + (uintptr)libc.Int32FromInt64(16);
        libc.Xmemcpy(tls, pCsr, zLocale, libc.Uint64FromInt64(nLocale));
        pCsr = pCsr + (uintptr)nLocale;
        v1 = pCsr;
        pCsr = pCsr + 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))).Value = (uint8)0x00;
        if (zText != 0) {
            libc.Xmemcpy(tls, pCsr, zText, libc.Uint64FromInt64(nText));
        }
        Xsqlite3_result_blob(tls, pCtx, pBlob, (int32)nBlob, __ccgo_fp(Xsqlite3_free));
    }
}

internal static int32 /*r*/ _fts5PorterCb(ж<libc.TLS> tls, uintptr pCtx, int32 tflags, uintptr pToken, int32 nToken, int32 iStart, int32 iEnd) {
    int32 r = default;

    uintptr aBuf = default;
    uintptr p = default;
    int8 c = default;
    ref var nBuf = ref heap(new int32(), out var ᏑnBuf);
    int32 v1 = default;
    _ = aBuf;
    _ = c;
    _ = nBuf;
    _ = p;
    _ = v1;
    p = pCtx;
    if (nToken > (int32)FTS5_PORTER_MAX_TOKEN || nToken < (int32)3) {
        goto pass_through;
    }
    aBuf = (@unsafe.Pointer.To<TPorterContext>((@unsafe.Pointer)p)).Value.FaBuf;
    nBuf = nToken;
    libc.Xmemcpy(tls, aBuf, pToken, libc.Uint64FromInt32(nBuf));
    _fts5PorterStep1A(ref (tls).DerefOrNull(), aBuf, @unsafe.Pointer.FromPinnedBox(ᏑnBuf));
    if (_fts5PorterStep1B(tls, aBuf, @unsafe.Pointer.FromPinnedBox(ᏑnBuf)) != 0) {
        if (_fts5PorterStep1B2(tls, aBuf, @unsafe.Pointer.FromPinnedBox(ᏑnBuf)) == 0) {
            c = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)1)));
            if (_fts5PorterIsVowel(ref (tls).DerefOrNull(), c, 0) == 0 && (int32)c != (int32)'l' && (int32)c != (int32)'s' && (int32)c != (int32)'z' && (int32)c == (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)2))))) {
                nBuf = nBuf - 1;
            } else {
                if (_fts5Porter_MEq1(ref (tls).DerefOrNull(), aBuf, nBuf) != 0 && _fts5Porter_Ostar(ref (tls).DerefOrNull(), aBuf, nBuf) != 0) {
                    v1 = nBuf;
                    nBuf = nBuf + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)v1))).Value = (int8)'e';
                }
            }
        }
    }
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)1)))) == (int32)'y' && _fts5Porter_Vowel(ref (tls).DerefOrNull(), aBuf, nBuf - (int32)1) != 0) {
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)1)))).Value = (int8)'i';
    }
    _fts5PorterStep2(tls, aBuf, @unsafe.Pointer.FromPinnedBox(ᏑnBuf));
    _fts5PorterStep3(tls, aBuf, @unsafe.Pointer.FromPinnedBox(ᏑnBuf));
    _fts5PorterStep4(tls, aBuf, @unsafe.Pointer.FromPinnedBox(ᏑnBuf));
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)1)))) == (int32)'e') {
        if (_fts5Porter_MGt1(ref (tls).DerefOrNull(), aBuf, nBuf - (int32)1) != 0 || _fts5Porter_MEq1(ref (tls).DerefOrNull(), aBuf, nBuf - (int32)1) != 0 && !(_fts5Porter_Ostar(ref (tls).DerefOrNull(), aBuf, nBuf - (int32)1) != 0)) {
            nBuf = nBuf - 1;
        }
    }
    if (nBuf > (int32)1 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)1)))) == (int32)'l' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aBuf + (uintptr)(nBuf - (int32)2)))) == (int32)'l' && _fts5Porter_MGt1(ref (tls).DerefOrNull(), aBuf, nBuf - (int32)1) != 0) {
        nBuf = nBuf - 1;
    }
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, int32, int32, int32>>((@unsafe.Pointer.To<TPorterContext>((@unsafe.Pointer)p)).Value.FxToken)(tls, (@unsafe.Pointer.To<TPorterContext>((@unsafe.Pointer)p)).Value.FpCtx, tflags, aBuf, nBuf, iStart, iEnd);
    goto pass_through;
pass_through:;
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, int32, int32, int32>>((@unsafe.Pointer.To<TPorterContext>((@unsafe.Pointer)p)).Value.FxToken)(tls, (@unsafe.Pointer.To<TPorterContext>((@unsafe.Pointer)p)).Value.FpCtx, tflags, pToken, nToken, iStart, iEnd);
    return r;
}

internal static int32 /*r*/ _fts5TriTokenize(ж<libc.TLS> tls, uintptr pTok, uintptr pCtx, int32 unusedFlags, uintptr pText, int32 nText, uintptr __ccgo_fp_xToken) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        array<int32> aStart = new(3);
        Tu32 iCode = default;
        int32 iNext = default;
        int32 ii = default;
        int32 rc = default;
        uintptr p = default;
        uintptr z1 = default;
        uintptr zEof = default;
        uintptr zIn = default;
        uintptr zOut = default;
        uintptr v1 = default;
        array<int8> _ᴛ1 = new(32);
        _ = aStart;
        _ = iCode;
        _ = iNext;
        _ = ii;
        _ = p;
        _ = rc;
        _ = z1;
        _ = zEof;
        _ = zIn;
        _ = zOut;
        _ = v1;
        p = pTok;
        rc = SQLITE_OK;
        zOut = bp;
        zIn = pText;
        if (zIn != 0) {
            v1 = zIn + (uintptr)nText;
        } else {
            v1 = (uintptr)0;
        }
        zEof = v1;
        iCode = (uint32)0;
        _ = unusedFlags;
        ii = 0;
        for (;;) {
            if (!(ii < (int32)3)) {
                break;
            }
            for (var cond = true; cond; cond = iCode == (uint32)0) {
                aStart[ii] = (int32)((int64)zIn - (int64)pText);
                if (zIn >= zEof) {
                    r = SQLITE_OK; goto ᒐdone;
                }
                v1 = zIn;
                zIn = zIn + 1;
                iCode = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1)));
                if (iCode >= (uint32)0xc0) {
                    iCode = (uint32)_fts5Utf8Trans1[iCode - (uint32)0xc0];
                    while (zIn < zEof && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIn))) & (int32)0xc0) == (int32)0x80) {
                        v1 = zIn;
                        zIn = zIn + 1;
                        iCode = (iCode << (int)(libc.Int32FromInt32(6))) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1))));
                    }
                    if (iCode < (uint32)0x80 || (Tu32)(iCode & (uint32)0xFFFFF800U) == (uint32)0xD800 || (Tu32)(iCode & (uint32)0xFFFFFFFEU) == (uint32)0xFFFE) {
                        iCode = (uint32)0xFFFD;
                    }
                }
                if ((@unsafe.Pointer.To<TTrigramTokenizer>((@unsafe.Pointer)p)).Value.FbFold != 0) {
                    iCode = libc.Uint32FromInt32(_sqlite3Fts5UnicodeFold(ref (tls).DerefOrNull(), libc.Int32FromUint32(iCode), (@unsafe.Pointer.To<TTrigramTokenizer>((@unsafe.Pointer)p)).Value.FiFoldParam));
                }
            }
            if (iCode < (uint32)0x00080) {
                v1 = zOut;
                zOut = zOut + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = libc.Int8FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0xFF))));
            } else {
                if (iCode < (uint32)0x00800) {
                    v1 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0xC0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x1F)))));
                    v1 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                } else {
                    if (iCode < (uint32)0x10000) {
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0xE0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x0F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                    } else {
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0xF0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(18)) & libc.Uint32FromInt32(0x07)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x3F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                    }
                }
            }
            goto _2;
_2:;
            ii = ii + 1;
        }
        while ((int32)1 != 0) {
            for (var cond = true; cond; cond = iCode == (uint32)0) {
                iNext = (int32)((int64)zIn - (int64)pText);
                if (zIn >= zEof) {
                    iCode = (uint32)0;
                    break;
                }
                v1 = zIn;
                zIn = zIn + 1;
                iCode = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1)));
                if (iCode >= (uint32)0xc0) {
                    iCode = (uint32)_fts5Utf8Trans1[iCode - (uint32)0xc0];
                    while (zIn < zEof && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zIn))) & (int32)0xc0) == (int32)0x80) {
                        v1 = zIn;
                        zIn = zIn + 1;
                        iCode = (iCode << (int)(libc.Int32FromInt32(6))) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1))));
                    }
                    if (iCode < (uint32)0x80 || (Tu32)(iCode & (uint32)0xFFFFF800U) == (uint32)0xD800 || (Tu32)(iCode & (uint32)0xFFFFFFFEU) == (uint32)0xFFFE) {
                        iCode = (uint32)0xFFFD;
                    }
                }
                if ((@unsafe.Pointer.To<TTrigramTokenizer>((@unsafe.Pointer)p)).Value.FbFold != 0) {
                    iCode = libc.Uint32FromInt32(_sqlite3Fts5UnicodeFold(ref (tls).DerefOrNull(), libc.Int32FromUint32(iCode), (@unsafe.Pointer.To<TTrigramTokenizer>((@unsafe.Pointer)p)).Value.FiFoldParam));
                }
            }
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, int32, int32, int32>>(__ccgo_fp_xToken)(tls, pCtx, 0, bp, (int32)((int64)zOut - ((t__predefined_ptrdiff_t)bp)), aStart[0], iNext);
            if (iCode == (uint32)0 || rc != SQLITE_OK) {
                break;
            }
            z1 = bp;
            v1 = z1;
            z1 = z1 + 1;
            if (libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(v1)))) >= (int32)0xc0) {
                while ((libc.Int32FromUint8(libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z1)))) & (int32)0xc0) == (int32)0x80) {
                    z1 = z1 + 1;
                }
            }
            libc.Xmemmove(tls, bp, z1, libc.Uint64FromInt64((int64)zOut - (int64)z1));
            zOut = zOut - (uintptr)((int64)z1 - ((t__predefined_ptrdiff_t)bp));
            if (iCode < (uint32)0x00080) {
                v1 = zOut;
                zOut = zOut + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = libc.Int8FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0xFF))));
            } else {
                if (iCode < (uint32)0x00800) {
                    v1 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0xC0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x1F)))));
                    v1 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                } else {
                    if (iCode < (uint32)0x10000) {
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0xE0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x0F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                    } else {
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0xF0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(18)) & libc.Uint32FromInt32(0x07)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x3F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3F)))));
                        v1 = zOut;
                        zOut = zOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                    }
                }
            }
            aStart[0] = aStart[(int32)1];
            aStart[(int32)1] = aStart[(int32)2];
            aStart[(int32)2] = iNext;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _fts5UnicodeTokenize(ж<libc.TLS> tls, uintptr pTokenizer, uintptr pCtx, int32 iUnused, uintptr pText, int32 nText, uintptr __ccgo_fp_xToken) {
    uintptr a = default;
    uintptr aFold = default;
    uintptr p = default;
    uintptr pEnd = default;
    uintptr zCsr = default;
    uintptr zOut = default;
    uintptr zTerm = default;
    uintptr v3 = default;
    Tu32 iCode = default;
    int32 ie = default;
    int32 @is = default;
    int32 nFold = default;
    int32 rc = default;
    int32 v7 = default;
    _ = a;
    _ = aFold;
    _ = iCode;
    _ = ie;
    _ = @is;
    _ = nFold;
    _ = p;
    _ = pEnd;
    _ = rc;
    _ = zCsr;
    _ = zOut;
    _ = zTerm;
    _ = v3;
    _ = v7;
    p = pTokenizer;
    rc = SQLITE_OK;
    a = p;
    zTerm = pText + (uintptr)nText;
    zCsr = pText;
    aFold = (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaFold;
    nFold = (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FnFold;
    pEnd = aFold + (uintptr)(nFold - (int32)6);
    _ = iUnused;
_2:;
    if (!(rc == SQLITE_OK)) {
        goto _1;
    }
    zOut = aFold;
    while ((int32)1 != 0) {
        if (zCsr >= zTerm) {
            goto tokenize_done;
        }
        if ((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) & (int32)0x80) != 0) {
            @is = (int32)((int64)zCsr - (int64)pText);
            v3 = zCsr;
            zCsr = zCsr + 1;
            iCode = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v3)));
            if (iCode >= (uint32)0xc0) {
                iCode = (uint32)_fts5Utf8Trans1[iCode - (uint32)0xc0];
                while (zCsr < zTerm && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) & (int32)0xc0) == (int32)0x80) {
                    v3 = zCsr;
                    zCsr = zCsr + 1;
                    iCode = (iCode << (int)(libc.Int32FromInt32(6))) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v3))));
                }
                if (iCode < (uint32)0x80 || (Tu32)(iCode & (uint32)0xFFFFF800U) == (uint32)0xD800 || (Tu32)(iCode & (uint32)0xFFFFFFFEU) == (uint32)0xFFFE) {
                    iCode = (uint32)0xFFFD;
                }
            }
            if (_fts5UnicodeIsAlnum(ref (tls).DerefOrNull(), p, libc.Int32FromUint32(iCode)) != 0) {
                goto non_ascii_tokenchar;
            }
        } else {
            if (~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + (uintptr)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))))) != 0) {
                @is = (int32)((int64)zCsr - (int64)pText);
                goto ascii_tokenchar;
            }
            zCsr = zCsr + 1;
        }
    }
_6:;
    if (!(zCsr < zTerm)) {
        goto _5;
    }
    if (zOut > pEnd) {
        aFold = Xsqlite3_malloc64(tls, libc.Uint64FromInt64((int64)nFold * (int64)2));
        if (aFold == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
            goto tokenize_done;
        }
        zOut = aFold + (uintptr)((int64)zOut - (int64)(@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaFold);
        libc.Xmemcpy(tls, aFold, (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaFold, libc.Uint64FromInt32(nFold));
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaFold);
        (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaFold = aFold;
        v7 = nFold * libc.Int32FromInt32(2);
        nFold = v7;
        (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FnFold = v7;
        pEnd = aFold + (uintptr)(nFold - (int32)6);
    }
    if (!((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) & (int32)0x80) != 0)) {
        goto _8;
    }
    v3 = zCsr;
    zCsr = zCsr + 1;
    iCode = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v3)));
    if (iCode >= (uint32)0xc0) {
        iCode = (uint32)_fts5Utf8Trans1[iCode - (uint32)0xc0];
        while (zCsr < zTerm && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) & (int32)0xc0) == (int32)0x80) {
            v3 = zCsr;
            zCsr = zCsr + 1;
            iCode = (iCode << (int)(libc.Int32FromInt32(6))) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v3))));
        }
        if (iCode < (uint32)0x80 || (Tu32)(iCode & (uint32)0xFFFFF800U) == (uint32)0xD800 || (Tu32)(iCode & (uint32)0xFFFFFFFEU) == (uint32)0xFFFE) {
            iCode = (uint32)0xFFFD;
        }
    }
    if (!(_fts5UnicodeIsAlnum(ref (tls).DerefOrNull(), p, libc.Int32FromUint32(iCode)) != 0 || _sqlite3Fts5UnicodeIsdiacritic(ref (tls).DerefOrNull(), libc.Int32FromUint32(iCode)) != 0)) {
        goto _12;
    }
    goto non_ascii_tokenchar;
non_ascii_tokenchar:;
    iCode = libc.Uint32FromInt32(_sqlite3Fts5UnicodeFold(ref (tls).DerefOrNull(), libc.Int32FromUint32(iCode), (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FeRemoveDiacritic));
    if (iCode != 0) {
        if (iCode < (uint32)0x00080) {
            v3 = zOut;
            zOut = zOut + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = libc.Int8FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0xFF))));
        } else {
            if (iCode < (uint32)0x00800) {
                v3 = zOut;
                zOut = zOut + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0xC0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x1F)))));
                v3 = zOut;
                zOut = zOut + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
            } else {
                if (iCode < (uint32)0x10000) {
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0xE0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x0F)))));
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3F)))));
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                } else {
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0xF0 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(18)) & libc.Uint32FromInt32(0x07)))));
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(12)) & libc.Uint32FromInt32(0x3F)))));
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode >> (int)(libc.Int32FromInt32(6)) & libc.Uint32FromInt32(0x3F)))));
                    v3 = zOut;
                    zOut = zOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)((int32)0x80 + libc.Int32FromUint8((uint8)((Tu32)(iCode & libc.Uint32FromInt32(0x3F)))));
                }
            }
        }
    }
    goto _13;
_12:;
    goto _5;
_13:;
    goto _9;
_8:;
    if (!(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + (uintptr)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr)))))) == 0)) {
        goto _24;
    }
    goto _5;
    goto _25;
_24:;
    goto ascii_tokenchar;
ascii_tokenchar:;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) >= (int32)'A' && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) <= (int32)'Z') {
        v3 = zOut;
        zOut = zOut + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = (int8)(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) + (int32)32);
    } else {
        v3 = zOut;
        zOut = zOut + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(v3))).Value = libc.Int8FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr)));
    }
    zCsr = zCsr + 1;
_25:;
_9:;
    ie = (int32)((int64)zCsr - (int64)pText);
    goto _6;
_5:;
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, int32, int32, int32>>(__ccgo_fp_xToken)(tls, pCtx, 0, aFold, (int32)((int64)zOut - (int64)aFold), @is, ie);
    goto _2;
_1:;
    goto tokenize_done;
tokenize_done:;
    if (rc == (int32)SQLITE_DONE) {
        rc = SQLITE_OK;
    }
    return rc;
}

internal static uintptr /*r*/ _geopolyParseJson(ж<libc.TLS> tls, uintptr z, @unsafe.Pointer pRc) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr aNew = default;
        uintptr pOut = default;
        uintptr v1 = default;
        int8 c = default;
        int32 ii = default;
        int32 rc = default;
        bool v2 = default;
        TGeoParse _ᴛ1 = default;
        int32 _ᴛ2 = default;
        _ = aNew;
        _ = c;
        _ = ii;
        _ = pOut;
        _ = rc;
        _ = v1;
        _ = v2;
        rc = SQLITE_OK;
        libc.Xmemset(tls, bp, 0, (uint64)32);
        (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz = z;
        if ((int32)_geopolySkipSpace(ref (tls).DerefOrNull(), __ccgo_ptr(bp)) == (int32)'[') {
            (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz + 1;
            while ((int32)_geopolySkipSpace(ref (tls).DerefOrNull(), __ccgo_ptr(bp)) == (int32)'[') {
                ii = 0;
                (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz + 1;
                if ((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex >= (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnAlloc) {
                    (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnAlloc = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnAlloc * (int32)2 + (int32)16;
                    aNew = Xsqlite3_realloc64(tls, (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa, (uint64)(libc.Uint64FromInt32((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnAlloc) * (uint64)4 * (uint64)2));
                    if (aNew == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                        (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnErr = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnErr + 1;
                        break;
                    }
                    (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa = aNew;
                }
                for (;;) {
                    if (ii <= (int32)1) {
                        v1 = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa + (uintptr)((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex * (int32)2 + ii) * 4;
                    } else {
                        v1 = (uintptr)0;
                    }
                    if (!(_geopolyParseNumber(tls, __ccgo_ptr(bp), v1) != 0)) {
                        break;
                    }
                    ii = ii + 1;
                    if (ii == (int32)2) {
                        (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex + 1;
                    }
                    c = _geopolySkipSpace(ref (tls).DerefOrNull(), __ccgo_ptr(bp));
                    (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz + 1;
                    if ((int32)c == (int32)',') {
                        continue;
                    }
                    if ((int32)c == (int32)']' && ii >= (int32)2) {
                        break;
                    }
                    (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnErr = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnErr + 1;
                    rc = (int32)SQLITE_ERROR;
                    goto parse_json_err;
                }
                if ((int32)_geopolySkipSpace(ref (tls).DerefOrNull(), __ccgo_ptr(bp)) == (int32)',') {
                    (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz + 1;
                    continue;
                }
                break;
            }
            v2 = (int32)_geopolySkipSpace(ref (tls).DerefOrNull(), __ccgo_ptr(bp)) == (int32)']' && (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex >= (int32)4 && (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex < (int32)16777216 && ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa)) == ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa + (uintptr)((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex * (int32)2 - (int32)2) * 4)) && ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa + 1 * 4)) == ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa + (uintptr)((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex * (int32)2 - (int32)1) * 4)); if (v2) {
                (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fz + 1;
            }
            if (v2 && (int32)_geopolySkipSpace(ref (tls).DerefOrNull(), __ccgo_ptr(bp)) == libc.Int32FromInt32(0)) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 32))).Value = (int32)1;
                (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex - 1;
                pOut = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(40) + (uint64)(libc.Uint64FromInt64(4) * libc.Uint64FromInt32(2)) * libc.Uint64FromInt64((int64)(@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex - libc.Int64FromInt32(4)));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 32))).Value = (int32)1;
                if (pOut == (uintptr)0) {
                    goto parse_json_err;
                }
                (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)pOut)).Value.FnVertex = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex;
                libc.Xmemcpy(tls, pOut + 8, (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa, libc.Uint64FromInt32((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex * (int32)2) * (uint64)4);
                (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4))).Value = (@unsafe.Pointer.To<uint8>(__ccgo_ptr(bp + 32))).Value;
                (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4 + 1))).Value = libc.Uint8FromInt32((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex >> (int)((int32)16) & (int32)0xff);
                (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4 + 2))).Value = libc.Uint8FromInt32((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex >> (int)((int32)8) & (int32)0xff);
                (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4 + 3))).Value = libc.Uint8FromInt32((@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnVertex & (int32)0xff);
                Xsqlite3_free(tls, (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa);
                if (pRc != nil) {
                    (@unsafe.Pointer.To<int32>(pRc)).Value = SQLITE_OK;
                }
                r = pOut; goto ᒐdone;
            } else {
                (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnErr = (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.FnErr + 1;
                rc = (int32)SQLITE_ERROR;
            }
        }
        goto parse_json_err;
parse_json_err:;
        if (pRc != nil) {
            (@unsafe.Pointer.To<int32>(pRc)).Value = rc;
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TGeoParse>(__ccgo_ptr(bp))).Value.Fa);
        r = (uintptr)0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _jsonAppendString(ж<libc.TLS> tls, uintptr p, uintptr zIn, Tu32 N) {
    Tu8 c = default;
    Tu32 k = default;
    uintptr z = default;
    uintptr v2 = default;
    Tu64 v1 = default;
    _ = c;
    _ = k;
    _ = z;
    _ = v1;
    _ = v2;
    z = zIn;
    if (z == (uintptr)0) {
        return;
    }
    if ((uint64)N + (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed + (uint64)2 >= (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc && _jsonStringGrow(tls, p, N + (uint32)2) != 0) {
        return;
    }
    v2 = p + 24;
    v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = (int8)'"';
    while ((int32)1 != 0) {
        k = (uint32)0;
        while ((int32)1 != 0) {
            if (k + (uint32)3 >= N) {
                while (k < N && _jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)k))] != 0) {
                    k = k + 1;
                }
                break;
            }
            if (!(_jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)k))] != 0)) {
                break;
            }
            if (!(_jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(k + (uint32)1)))] != 0)) {
                k = k + (uint32)1;
                break;
            }
            if (!(_jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(k + (uint32)2)))] != 0)) {
                k = k + (uint32)2;
                break;
            }
            if (!(_jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(k + (uint32)3)))] != 0)) {
                k = k + (uint32)3;
                break;
            } else {
                k = k + (uint32)4;
            }
        }
        if (k >= N) {
            if (k > (uint32)0) {
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)(@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed, z, (uint64)k);
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 24))).Value += (uint64)k;
            }
            break;
        }
        if (k > (uint32)0) {
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)(@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed, z, (uint64)k);
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 24))).Value += (uint64)k;
            z = z + (uintptr)k;
            N = N - k;
        }
        c = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z));
        if (libc.Int32FromUint8(c) == (int32)'"' || libc.Int32FromUint8(c) == (int32)'\\') {
            if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed + (uint64)N + (uint64)3 > (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc && _jsonStringGrow(tls, p, N + (uint32)3) != 0) {
                return;
            }
            v2 = p + 24;
            v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = (int8)'\\';
            v2 = p + 24;
            v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = libc.Int8FromUint8(c);
        } else {
            if (libc.Int32FromUint8(c) == (int32)'\'') {
                v2 = p + 24;
                v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
                (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = libc.Int8FromUint8(c);
            } else {
                if ((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed + (uint64)N + (uint64)7 > (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnAlloc && _jsonStringGrow(tls, p, N + (uint32)7) != 0) {
                    return;
                }
                _jsonAppendControlChar(ref (tls).DerefOrNull(), p, c);
            }
        }
        z = z + 1;
        N = N - 1;
    }
    v2 = p + 24;
    v1 = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Tu64>((@unsafe.Pointer)v2) + 1;
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FzBuf + (uintptr)v1))).Value = (int8)'"';
}

internal static Tu32 /*r*/ _jsonCreateEditSubstructure(ж<libc.TLS> tls, uintptr pParse, uintptr pIns, uintptr zTail) {
    int32 rc = default;
    uintptr v1 = default;
    _ = rc;
    _ = v1;
    libc.Xmemset(tls, pIns, 0, (uint64)72);
    (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.Fdb = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zTail))) == 0) {
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FaBlob = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaIns;
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FnBlob = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnIns;
        rc = 0;
    } else {
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FnBlob = (uint32)1;
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FaBlob = (uintptr)Ꮡ_emptyObject + libc.BoolUintptr((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zTail))) == (int32)'.');
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FeEdit = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FeEdit;
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FnIns = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnIns;
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FaIns = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaIns;
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FiDepth = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FiDepth) + (int32)1);
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.FiDepth) >= (int32)JSON_MAX_DEPTH) {
            return (uint32)JSON_LOOKUP_TOODEEP;
        }
        rc = libc.Int32FromUint32(_jsonLookupStep(tls, pIns, (uint32)0, zTail, (uint32)0));
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FiDepth = (Tu16)((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FiDepth - 1);
        v1 = pParse + 47;
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromUint8((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pIns)).Value.Foom));
    }
    return libc.Uint32FromInt32(rc);
}

internal static void _jsonErrorFunc(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        Ti64 iErrPos = default;
        Tu32 k = default;
        TJsonParse _ᴛ1 = default;
        _ = iErrPos;
        _ = k;
        iErrPos = 0;
        _ = argc;
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.Fdb = Xsqlite3_context_db_handle(tls, ctx);
        if (_jsonArgIsJsonb(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), __ccgo_ptr(bp)) != 0) {
            iErrPos = libc.Int64FromUint32(_jsonbValidityCheck(tls, __ccgo_ptr(bp), (uint32)0, (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnBlob, (uint32)1));
        } else {
            (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FzJson = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
            if ((@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FzJson == (uintptr)0) {
                return;
            }
            (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnJson = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
            if (_jsonConvertTextToBlob(tls, bp, (uintptr)0) != 0) {
                if ((@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.Foom != 0) {
                    iErrPos = (int64)(-(int32)1);
                } else {
                    k = (uint32)0;
                    for (;;) {
                        if (!(k < (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FiErr && ~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FzJson + (uintptr)k)) != 0)) {
                            break;
                        }
                        if (((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FzJson + (uintptr)k))) & (int32)0xc0) != (int32)0x80) {
                            iErrPos = iErrPos + 1;
                        }
                        goto _1;
_1:;
                        k = k + 1;
                    }
                    iErrPos = iErrPos + 1;
                }
            }
        }
        _jsonParseReset(tls, __ccgo_ptr(bp));
        if (iErrPos < 0) {
            Xsqlite3_result_error_nomem(tls, ctx);
        } else {
            Xsqlite3_result_int64(tls, ctx, iErrPos);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _memdbOpen(ж<libc.TLS> tls, uintptr pVfs, uintptr zName, uintptr pFd, int32 flags, uintptr pOutFlags) {
    uintptr apNew = default;
    uintptr p = default;
    uintptr pFile = default;
    uintptr pVfsMutex = default;
    uintptr v3 = default;
    int32 i = default;
    int32 szName = default;
    int32 v2 = default;
    _ = apNew;
    _ = i;
    _ = p;
    _ = pFile;
    _ = pVfsMutex;
    _ = szName;
    _ = v2;
    _ = v3;
    pFile = pFd;
    p = (uintptr)0;
    _ = pVfs;
    libc.Xmemset(tls, pFile, 0, (uint64)24);
    szName = _sqlite3Strlen30(tls, zName);
    if (szName > (int32)1 && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName))) == (int32)'/' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName))) == (int32)'\\')) {
        pVfsMutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_VFS1);
        Xsqlite3_mutex_enter(tls, pVfsMutex);
        i = 0;
        for (;;) {
            if (!(i < _memdb_g.FnMemStore)) {
                break;
            }
            if (libc.Xstrcmp(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_memdb_g.FapMemStore + (uintptr)i * 8))))).Value.FzFName, zName) == 0) {
                p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(_memdb_g.FapMemStore + (uintptr)i * 8));
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (p == (uintptr)0) {
            p = _sqlite3Malloc(tls, (uint64)72 + libc.Uint64FromInt64((int64)szName) + (uint64)3);
            if (p == (uintptr)0) {
                Xsqlite3_mutex_leave(tls, pVfsMutex);
                return (int32)SQLITE_NOMEM;
            }
            apNew = _sqlite3Realloc(tls, _memdb_g.FapMemStore, (uint64)8 * libc.Uint64FromInt64(libc.Int64FromInt32(1) + (int64)_memdb_g.FnMemStore));
            if (apNew == (uintptr)0) {
                Xsqlite3_free(tls, p);
                Xsqlite3_mutex_leave(tls, pVfsMutex);
                return (int32)SQLITE_NOMEM;
            }
            v3 = (uintptr)Ꮡ_memdb_g;
            v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)v2 * 8))).Value = p;
            _memdb_g.FapMemStore = apNew;
            libc.Xmemset(tls, p, 0, (uint64)72);
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FmFlags = libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_DESERIALIZE_RESIZEABLE) | libc.Int32FromInt32(SQLITE_DESERIALIZE_FREEONCLOSE));
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax = _sqlite3Config.FmxMemdbSize;
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FzFName = p + 1 * 72;
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FzFName, zName, libc.Uint64FromInt32(szName + (int32)1));
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FpMutex = Xsqlite3_mutex_alloc(tls, SQLITE_MUTEX_FAST);
            if ((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FpMutex == (uintptr)0) {
                _memdb_g.FnMemStore = _memdb_g.FnMemStore - 1;
                Xsqlite3_free(tls, p);
                Xsqlite3_mutex_leave(tls, pVfsMutex);
                return (int32)SQLITE_NOMEM;
            }
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef = (int32)1;
            _memdbEnter(tls, p);
        } else {
            _memdbEnter(tls, p);
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef = (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnRef + 1;
        }
        Xsqlite3_mutex_leave(tls, pVfsMutex);
    } else {
        p = _sqlite3Malloc(tls, (uint64)72);
        if (p == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemset(tls, p, 0, (uint64)72);
        (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FmFlags = libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_DESERIALIZE_RESIZEABLE) | libc.Int32FromInt32(SQLITE_DESERIALIZE_FREEONCLOSE));
        (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax = _sqlite3Config.FmxMemdbSize;
    }
    (@unsafe.Pointer.To<TMemFile>((@unsafe.Pointer)pFile)).Value.FpStore = p;
    if (pOutFlags != (uintptr)0) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pOutFlags))).Value = flags | (int32)SQLITE_OPEN_MEMORY;
    }
    (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pFd)).Value.FpMethods = (uintptr)Ꮡ_memdb_io_methods;
    _memdbLeave(tls, p);
    return SQLITE_OK;
}

internal static int32 /*r*/ _rbuDeltaApply(ж<libc.TLS> tls, uintptr zSrc, int32 lenSrc, uintptr zDeltaʗp, int32 lenDeltaʗp, uintptr zOut) {
    ref var zDelta = ref heap(zDeltaʗp, out var ᏑzDelta);
    ref var lenDelta = ref heap(lenDeltaʗp, out var ᏑlenDelta);

    uint32 cnt = default;
    uint32 ofst = default;
    Tsqlite3_uint64 limit = default;
    Tsqlite3_uint64 total = default;
    _ = cnt;
    _ = limit;
    _ = ofst;
    _ = total;
    total = (uint64)0;
    limit = (uint64)_rbuDeltaGetInt(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzDelta), @unsafe.Pointer.FromPinnedBox(ᏑlenDelta));
    if (lenDelta <= 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDelta))) != (int32)'\n') {
        return -(int32)1;
    }
    zDelta = zDelta + 1;
    lenDelta = lenDelta - 1;
    while (lenDelta > 0 && ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDelta)) != 0) {
        cnt = _rbuDeltaGetInt(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzDelta), @unsafe.Pointer.FromPinnedBox(ᏑlenDelta));
        if (lenDelta <= 0) {
            return -(int32)1;
        }
        switch ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDelta)))) {
        case var ᴛ when ᴛ == (int32)'@': {
            zDelta = zDelta + 1;
            lenDelta = lenDelta - 1;
            ofst = _rbuDeltaGetInt(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑzDelta), @unsafe.Pointer.FromPinnedBox(ᏑlenDelta));
            if (lenDelta > 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zDelta))) != (int32)',') {
                return -(int32)1;
            }
            zDelta = zDelta + 1;
            lenDelta = lenDelta - 1;
            total = total + (uint64)cnt;
            if (total > limit) {
                return -(int32)1;
            }
            if ((uint64)ofst + (uint64)cnt > libc.Uint64FromInt32(lenSrc)) {
                return -(int32)1;
            }
            libc.Xmemcpy(tls, zOut, zSrc + (uintptr)ofst, (uint64)cnt);
            zOut = zOut + (uintptr)cnt;
            break;
        }
        case var ᴛ when ᴛ == (int32)':': {
            zDelta = zDelta + 1;
            lenDelta = lenDelta - 1;
            total = total + (uint64)cnt;
            if (total > limit) {
                return -(int32)1;
            }
            if (cnt > libc.Uint32FromInt32(lenDelta)) {
                return -(int32)1;
            }
            libc.Xmemcpy(tls, zOut, zDelta, (uint64)cnt);
            zOut = zOut + (uintptr)cnt;
            zDelta = zDelta + (uintptr)cnt;
            lenDelta = libc.Int32FromUint32((uint32)lenDelta - cnt);
            break;
        }
        case var ᴛ when ᴛ == (int32)';': {
            zDelta = zDelta + 1;
            lenDelta = lenDelta - 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zOut))).Value = 0;
            if (total != limit) {
                return -(int32)1;
            }
            return libc.Int32FromUint64(total);
        }
        default: {
            return -(int32)1;
        }}

    }
    return -(int32)1;
}

internal static int32 /*r*/ _rtreeBestIndex(ж<libc.TLS> tls, uintptr tab, uintptr pIdxInfo) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 bMatch = default;
        int32 iIdx = default;
        int32 ii = default;
        int32 jj = default;
        int32 rc = default;
        int32 v4 = default;
        Tu8 doOmit = default;
        Tu8 op = default;
        Ti64 nRow = default;
        uintptr p = default;
        uintptr pRtree = default;
        array<int8> _ᴛ1 = new(41);
        _ = bMatch;
        _ = doOmit;
        _ = iIdx;
        _ = ii;
        _ = jj;
        _ = nRow;
        _ = op;
        _ = p;
        _ = pRtree;
        _ = rc;
        _ = v4;
        pRtree = tab;
        rc = SQLITE_OK;
        bMatch = 0;
        iIdx = 0;
        libc.Xmemset(tls, bp, 0, (uint64)41);
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
                break;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)ii * 12))).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_MATCH) {
                bMatch = (int32)1;
            }
            goto _1;
_1:;
            ii = ii + 1;
        }
        ii = 0;
        for (;;) {
            if (!(ii < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint && iIdx < libc.Int32FromUint64(libc.Uint64FromInt64(41) - libc.Uint64FromInt32(1)))) {
                break;
            }
            p = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)ii * 12;
            if (bMatch == 0 && (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fusable != 0 && (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn <= 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
                jj = 0;
                for (;;) {
                    if (!(jj < ii)) {
                        break;
                    }
                    (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)jj * 8))).Value.FargvIndex = 0;
                    (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)jj * 8))).Value.Fomit = (uint8)0;
                    goto _3;
_3:;
                    jj = jj + 1;
                }
                (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = (int32)1;
                (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)ii * 8))).Value.FargvIndex = (int32)1;
                (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)jj * 8))).Value.Fomit = (uint8)1;
                (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)30D;
                (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)1;
                (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxFlags = (int32)SQLITE_INDEX_SCAN_UNIQUE;
                r = SQLITE_OK; goto ᒐdone;
            }
            if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fusable != 0 && ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn > 0 && (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn <= libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2) || libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_MATCH)) {
                doOmit = (uint8)1;
                switch (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop)) {
                case var ᴛ when ᴛ == (int32)SQLITE_INDEX_CONSTRAINT_EQ: {
                    op = (uint8)RTREE_EQ;
                    doOmit = (uint8)0;
                    break;
                }
                case var ᴛ when ᴛ == (int32)SQLITE_INDEX_CONSTRAINT_GT: {
                    op = (uint8)RTREE_GT;
                    doOmit = (uint8)0;
                    break;
                }
                case var ᴛ when ᴛ == (int32)SQLITE_INDEX_CONSTRAINT_LE: {
                    op = (uint8)RTREE_LE;
                    break;
                }
                case var ᴛ when ᴛ == (int32)SQLITE_INDEX_CONSTRAINT_LT: {
                    op = (uint8)RTREE_LT;
                    doOmit = (uint8)0;
                    break;
                }
                case var ᴛ when ᴛ == (int32)SQLITE_INDEX_CONSTRAINT_GE: {
                    op = (uint8)RTREE_GE;
                    break;
                }
                case var ᴛ when ᴛ == (int32)SQLITE_INDEX_CONSTRAINT_MATCH: {
                    op = (uint8)RTREE_MATCH;
                    break;
                }
                default: {
                    op = (uint8)0;
                    break;
                    break;
                }}

                if (op != 0) {
                    v4 = iIdx;
                    iIdx = iIdx + 1;
                    @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), v4, 41) = libc.Int8FromUint8(op);
                    v4 = iIdx;
                    iIdx = iIdx + 1;
                    @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp), v4, 41) = (int8)((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn - libc.Int32FromInt32(1) + libc.Int32FromUint8((uint8)'0'));
                    (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)ii * 8))).Value.FargvIndex = iIdx / (int32)2;
                    (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)ii * 8))).Value.Fomit = doOmit;
                }
            }
            goto _2;
_2:;
            ii = ii + 1;
        }
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = (int32)2;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FneedToFreeIdxStr = (int32)1;
        if (iIdx > 0) {
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr = Xsqlite3_malloc(tls, iIdx + (int32)1);
            if ((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr == (uintptr)0) {
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr, bp, libc.Uint64FromInt32(iIdx + (int32)1));
        }
        nRow = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnRowEst >> (int)(iIdx / (int32)2);
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)(libc.Float64FromFloat64(6D) * (float64)nRow);
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = nRow;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _rtreeFilter(ж<libc.TLS> tls, uintptr pVtabCursor, int32 idxNum, uintptr idxStr, int32 argc, uintptr argv) {
    int32 eType = default;
    int32 eType1 = default;
    ref var iCell = ref heap(new int32(), out var ᏑiCell);
    int32 ii = default;
    int32 rc = default;
    ref var iNode = ref heap(new Ti64(), out var ᏑiNode);
    Ti64 iRowid = default;
    Tsqlite3_int64 iVal = default;
    uintptr p = default;
    uintptr p1 = default;
    uintptr pCsr = default;
    ref var pLeaf = ref heap(new uintptr(), out var ᏑpLeaf);
    uintptr pNew = default;
    ref var pRoot = ref heap(new uintptr(), out var ᏑpRoot);
    uintptr pRtree = default;
    _ = eType;
    _ = eType1;
    _ = iCell;
    _ = iNode;
    _ = iRowid;
    _ = iVal;
    _ = ii;
    _ = p;
    _ = p1;
    _ = pCsr;
    _ = pLeaf;
    _ = pNew;
    _ = pRoot;
    _ = pRtree;
    _ = rc;
    pRtree = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pVtabCursor)).Value.FpVtab;
    pCsr = pVtabCursor;
    pRoot = (uintptr)0;
    rc = SQLITE_OK;
    iCell = 0;
    _rtreeReference(ref (tls).DerefOrNull(), pRtree);
    _resetCursor(tls, pCsr);
    (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FiStrategy = idxNum;
    if (idxNum == (int32)1) {
        iRowid = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        iNode = 0;
        eType = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT && 0 == _sqlite3IntFloatCompare(tls, iRowid, Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))))) {
            rc = _findLeafNode(tls, pRtree, iRowid, @unsafe.Pointer.FromBox(ᏑpLeaf), @unsafe.Pointer.FromPinnedBox(ᏑiNode));
        } else {
            rc = SQLITE_OK;
            pLeaf = (uintptr)0;
        }
        if (rc == SQLITE_OK && pLeaf != (uintptr)0) {
            p = _rtreeSearchPointNew(tls, pCsr, (float64)0D, (uint8)0);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCsr + 88))).Value = pLeaf;
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.Fid = iNode;
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FeWithin = (uint8)PARTLY_WITHIN;
            rc = _nodeRowidIndex(ref (tls).DerefOrNull(), pRtree, pLeaf, iRowid, @unsafe.Pointer.FromPinnedBox(ᏑiCell));
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell = libc.Uint8FromInt32(iCell);
        } else {
            (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FatEOF = (uint8)1;
        }
    } else {
        rc = _nodeAcquire(tls, pRtree, (int64)1, (uintptr)0, @unsafe.Pointer.FromBox(ᏑpRoot));
        if (rc == SQLITE_OK && argc > 0) {
            (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint = Xsqlite3_malloc64(tls, (uint64)((uint64)24 * libc.Uint64FromInt32(argc)));
            (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FnConstraint = argc;
            if (!((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint != 0)) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                libc.Xmemset(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint, 0, (uint64)24 * libc.Uint64FromInt32(argc));
                libc.Xmemset(tls, pCsr + 128, 0, (uint64)4 * libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth + libc.Int32FromInt32(1)));
                ii = 0;
                for (;;) {
                    if (!(ii < argc)) {
                        break;
                    }
                    p1 = (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint + (uintptr)ii * 24;
                    eType1 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)ii * 8)));
                    (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(idxStr + (uintptr)(ii * (int32)2))));
                    (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(idxStr + (uintptr)(ii * (int32)2 + (int32)1)))) - (int32)'0';
                    if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop >= (int32)RTREE_MATCH) {
                        rc = _deserializeGeometry(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)ii * 8)), p1);
                        if (rc != SQLITE_OK) {
                            break;
                        }
                        (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)(@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FpInfo)).Value.FnCoord = libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2);
                        (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)(@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FpInfo)).Value.FanQueue = pCsr + 128;
                        (@unsafe.Pointer.To<Tsqlite3_rtree_query_info>((@unsafe.Pointer)(@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FpInfo)).Value.FmxLevel = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth + (int32)1;
                    } else {
                        if (eType1 == (int32)SQLITE_INTEGER) {
                            iVal = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)ii * 8)));
                            (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)iVal;
                            if (iVal >= libc.Int64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(48))) || iVal <= -(libc.Int64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(48))))) {
                                if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop == (int32)RTREE_LT) {
                                    (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)RTREE_LE;
                                }
                                if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop == (int32)RTREE_GT) {
                                    (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)RTREE_GE;
                                }
                            }
                        } else {
                            if (eType1 == (int32)SQLITE_FLOAT) {
                                (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)ii * 8)));
                            } else {
                                (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)0D;
                                if (eType1 == (int32)SQLITE_NULL) {
                                    (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)RTREE_FALSE;
                                } else {
                                    if ((@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop == (int32)RTREE_LT || (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop == (int32)RTREE_LE) {
                                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)RTREE_TRUE;
                                    } else {
                                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)RTREE_FALSE;
                                    }
                                }
                            }
                        }
                    }
                    goto _1;
_1:;
                    ii = ii + 1;
                }
            }
        }
        if (rc == SQLITE_OK) {
            pNew = _rtreeSearchPointNew(tls, pCsr, (float64)0D, libc.Uint8FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth + libc.Int32FromInt32(1)));
            if (pNew == (uintptr)0) {
                return (int32)SQLITE_NOMEM;
            }
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pNew)).Value.Fid = (int64)1;
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pNew)).Value.FiCell = (uint8)0;
            (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pNew)).Value.FeWithin = (uint8)PARTLY_WITHIN;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCsr + 88))).Value = pRoot;
            pRoot = (uintptr)0;
            rc = _rtreeStepToLeaf(tls, pCsr);
        }
    }
    _nodeRelease(tls, pRtree, pRoot);
    _rtreeRelease(tls, pRtree);
    return rc;
}

internal static uintptr /*r*/ _sqlite3ExprAlloc(ж<libc.TLS> tls, uintptr db, int32 op, @unsafe.Pointer pToken, int32 dequote) {
    int32 nExtra = default;
    uintptr pNew = default;
    uint32 v1 = default;
    _ = nExtra;
    _ = pNew;
    _ = v1;
    if (pToken != nil) {
        v1 = ~@unsafe.Pointer.To<uint32>(@unsafe.Add(pToken, 8)) + (uint32)1;
    } else {
        v1 = (uint32)0;
    }
    nExtra = libc.Int32FromUint32(v1);
    pNew = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)72 + libc.Uint64FromInt32(nExtra)));
    if (pNew != 0) {
        libc.Xmemset(tls, pNew, 0, (uint64)72);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.Fop = libc.Uint8FromInt32(op);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FiAgg = (int16)(-(int32)1);
        if (nExtra != 0) {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 8))).Value = pNew + 1 * 72;
            if (~@unsafe.Pointer.To<uint32>(@unsafe.Add(pToken, 8)) != 0) {
                libc.Xmemcpy(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 8)), ~@unsafe.Pointer.To<uintptr>(pToken), (uint64)(~@unsafe.Pointer.To<uint32>(@unsafe.Add(pToken, 8))));
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 8)) + (uintptr)(~@unsafe.Pointer.To<uint32>(@unsafe.Add(pToken, 8)))))).Value = 0;
            if (dequote != 0 && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 8)))))]) & (int32)0x80) != 0) {
                _sqlite3DequoteExpr(ref (tls).DerefOrNull(), pNew);
            }
        }
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FnHeight = (int32)1;
    }
    return pNew;
}

internal static uintptr /*r*/ _sqlite3FindFunction(ж<libc.TLS> tls, uintptr db, uintptr zName, int32 nArg, Tu8 enc, Tu8 createFlag) {
    int32 bestScore = default;
    int32 h = default;
    int32 nName = default;
    int32 score = default;
    int32 score1 = default;
    uintptr p = default;
    uintptr pBest = default;
    uintptr pOther = default;
    uintptr z = default;
    uintptr v1 = default;
    bool v2 = default;
    _ = bestScore;
    _ = h;
    _ = nName;
    _ = p;
    _ = pBest;
    _ = pOther;
    _ = score;
    _ = score1;
    _ = z;
    _ = v1;
    _ = v2;
    pBest = (uintptr)0;
    bestScore = 0;
    nName = _sqlite3Strlen30(tls, zName);
    p = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)(db + 632), zName);
    while (p != 0) {
        score = _matchQuality(ref (tls).DerefOrNull(), p, nArg, enc);
        if (score > bestScore) {
            pBest = p;
            bestScore = score;
        }
        p = (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FpNext;
    }
    if (!(createFlag != 0) && (pBest == (uintptr)0 || (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_PreferBuiltin) != (uint32)0)) {
        bestScore = 0;
        h = (libc.Int32FromUint8(_sqlite3UpperToLower[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName)))]) + nName) % (int32)SQLITE_FUNC_HASH_SZ;
        p = _sqlite3FunctionSearch(ref (tls).DerefOrNull(), h, zName);
        while (p != 0) {
            score1 = _matchQuality(ref (tls).DerefOrNull(), p, nArg, enc);
            if (score1 > bestScore) {
                pBest = p;
                bestScore = score1;
            }
            p = (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FpNext;
        }
    }
    v2 = createFlag != 0 && bestScore < (int32)FUNC_PERFECT_MATCH; if (v2) {
        v1 = _sqlite3DbMallocZero(tls, db, (uint64)((uint64)72 + libc.Uint64FromInt32(nName) + (uint64)1));
        pBest = v1;
    }
    if (v2 && v1 != (uintptr)0) {
        (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FzName = pBest + 1 * 72;
        (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FnArg = libc.Int16FromUint16(libc.Uint16FromInt32(nArg));
        (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FfuncFlags = (uint32)enc;
        libc.Xmemcpy(tls, pBest + 1 * 72, zName, libc.Uint64FromInt32(nName + (int32)1));
        z = (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FzName;
        for (;;) {
            if (!(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z)) != 0)) {
                break;
            }
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z))).Value = _sqlite3UpperToLower[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z))];
            goto _3;
_3:;
            z = z + 1;
        }
        pOther = _sqlite3HashInsert(tls, db + 632, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FzName, pBest);
        if (pOther == pBest) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pBest);
            _sqlite3OomFault(tls, db);
            return (uintptr)0;
        } else {
            (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FpNext = pOther;
        }
    }
    if (pBest != 0 && ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pBest)).Value.FxSFunc != 0 || createFlag != 0)) {
        return pBest;
    }
    return (uintptr)0;
}

internal static int32 /*r*/ _sqlite3Fts5ConfigParseRank(ж<libc.TLS> tls, uintptr zIn, @unsafe.Pointer pzRank, @unsafe.Pointer pzRankArgs) {
    uintptr p = default;
    uintptr pArgs = default;
    uintptr pRank = default;
    uintptr zRank = default;
    uintptr zRankArgs = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = p;
    _ = pArgs;
    _ = pRank;
    _ = rc;
    _ = zRank;
    _ = zRankArgs;
    p = zIn;
    zRank = (uintptr)0;
    zRankArgs = (uintptr)0;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<uintptr>(pzRank)).Value = (uintptr)0;
    (@unsafe.Pointer.To<uintptr>(pzRankArgs)).Value = (uintptr)0;
    if (p == (uintptr)0) {
        rc = (int32)SQLITE_ERROR;
    } else {
        p = _fts5ConfigSkipWhitespace(ref (tls).DerefOrNull(), p);
        pRank = p;
        p = _fts5ConfigSkipBareword(ref (tls).DerefOrNull(), p);
        if (p != 0) {
            zRank = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)((uintptr)1 + p) - (int64)pRank);
            if (zRank != 0) {
                libc.Xmemcpy(tls, zRank, pRank, libc.Uint64FromInt64((int64)p - (int64)pRank));
            }
        } else {
            rc = (int32)SQLITE_ERROR;
        }
        if (rc == SQLITE_OK) {
            p = _fts5ConfigSkipWhitespace(ref (tls).DerefOrNull(), p);
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != (int32)'(') {
                rc = (int32)SQLITE_ERROR;
            }
            p = p + 1;
        }
        if (rc == SQLITE_OK) {
            p = _fts5ConfigSkipWhitespace(ref (tls).DerefOrNull(), p);
            pArgs = p;
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(p))) != (int32)')') {
                p = _fts5ConfigSkipArgs(tls, p);
                if (p == (uintptr)0) {
                    rc = (int32)SQLITE_ERROR;
                } else {
                    zRankArgs = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)((uintptr)1 + p) - (int64)pArgs);
                    if (zRankArgs != 0) {
                        libc.Xmemcpy(tls, zRankArgs, pArgs, libc.Uint64FromInt64((int64)p - (int64)pArgs));
                    }
                }
            }
        }
    }
    if (rc != SQLITE_OK) {
        Xsqlite3_free(tls, zRank);
    } else {
        (@unsafe.Pointer.To<uintptr>(pzRank)).Value = zRank;
        (@unsafe.Pointer.To<uintptr>(pzRankArgs)).Value = zRankArgs;
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5HashWrite(ж<libc.TLS> tls, uintptr pHash, Ti64 iRowid, int32 iCol, int32 iPos, int8 bByte, uintptr pToken, int32 nToken) {
    int32 bNew = default;
    int32 nIncr = default;
    int32 rc = default;
    int32 v2 = default;
    Tu64 iDiff = default;
    uint32 iHash = default;
    Tsqlite3_int64 nByte = default;
    Tsqlite3_int64 nNew = default;
    uintptr p = default;
    uintptr pNew = default;
    uintptr pPtr = default;
    uintptr pp = default;
    uintptr zKey = default;
    uintptr zKey1 = default;
    uintptr v6 = default;
    _ = bNew;
    _ = iDiff;
    _ = iHash;
    _ = nByte;
    _ = nIncr;
    _ = nNew;
    _ = p;
    _ = pNew;
    _ = pPtr;
    _ = pp;
    _ = rc;
    _ = zKey;
    _ = zKey1;
    _ = v2;
    _ = v6;
    nIncr = 0;
    bNew = libc.BoolInt32((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail == FTS5_DETAIL_FULL);
    iHash = _fts5HashKey2(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot, libc.Uint8FromInt8(bByte), pToken, nToken);
    p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)iHash * 8));
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        zKey = p + 1 * 48;
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zKey))) == (int32)bByte && (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnKey == nToken + (int32)1 && libc.Xmemcmp(tls, zKey + 1, pToken, libc.Uint64FromInt32(nToken)) == 0) {
            break;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FpHashNext;
    }
    if (p == (uintptr)0) {
        nByte = libc.Int64FromUint64((uint64)48 + libc.Uint64FromInt32(nToken + libc.Int32FromInt32(1)) + (uint64)1 + (uint64)64);
        if (nByte < (int64)128) {
            nByte = (int64)128;
        }
        if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnEntry * (int32)2 >= (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot) {
            rc = _fts5HashResize(tls, pHash);
            if (rc != SQLITE_OK) {
                return rc;
            }
            iHash = _fts5HashKey2(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot, libc.Uint8FromInt8(bByte), pToken, nToken);
        }
        p = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (!(p != 0)) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemset(tls, p, 0, (uint64)48);
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnAlloc = (int32)nByte;
        zKey1 = p + 1 * 48;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zKey1))).Value = bByte;
        libc.Xmemcpy(tls, zKey1 + 1, pToken, libc.Uint64FromInt32(nToken));
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnKey = nToken + (int32)1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zKey1 + (uintptr)(nToken + (int32)1)))).Value = (int8)'\u0000';
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData = libc.Int32FromUint64(libc.Uint64FromInt32(nToken + (int32)1) + (uint64)48);
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FpHashNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)iHash * 8));
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)iHash * 8))).Value = p;
        (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnEntry = (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnEntry + 1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(p + (uintptr)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData), libc.Uint64FromInt64(iRowid));
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiRowid = iRowid;
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData;
        if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail != (int32)FTS5_DETAIL_NONE) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += (int32)1;
            if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail == FTS5_DETAIL_FULL) {
                v2 = 0;
            } else {
                v2 = -(int32)1;
            }
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiCol = (int16)v2;
        }
    } else {
        if ((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnAlloc - (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData < libc.Int32FromInt32(9) + libc.Int32FromInt32(4) + libc.Int32FromInt32(1) + libc.Int32FromInt32(3) + libc.Int32FromInt32(5)) {
            nNew = (int64)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnAlloc * (int64)2;
            pNew = Xsqlite3_realloc64(tls, p, libc.Uint64FromInt64(nNew));
            if (pNew == (uintptr)0) {
                return (int32)SQLITE_NOMEM;
            }
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)pNew)).Value.FnAlloc = (int32)nNew;
            pp = (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)iHash * 8;
            for (;;) {
                if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != p)) {
                    break;
                }
                goto _3;
_3:;
                pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp));
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = pNew;
            p = pNew;
        }
        nIncr = nIncr - (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData;
    }
    pPtr = p;
    if (iRowid != (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiRowid) {
        iDiff = libc.Uint64FromInt64(iRowid) - libc.Uint64FromInt64((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiRowid);
        _fts5HashAddPoslistSize(tls, pHash, p, (uintptr)0);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(pPtr + (uintptr)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData), iDiff);
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiRowid = iRowid;
        bNew = (int32)1;
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData;
        if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail != (int32)FTS5_DETAIL_NONE) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += (int32)1;
            if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail == FTS5_DETAIL_FULL) {
                v2 = 0;
            } else {
                v2 = -(int32)1;
            }
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiCol = (int16)v2;
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiPos = 0;
        }
    }
    if (iCol >= 0) {
        if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail == (int32)FTS5_DETAIL_NONE) {
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbContent = (uint8)1;
        } else {
            if (iCol != (int32)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiCol) {
                if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail == FTS5_DETAIL_FULL) {
                    v6 = p + 24;
                    v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v6);
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v6)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v6) + 1;
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtr + (uintptr)v2))).Value = (uint8)0x01;
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(pPtr + (uintptr)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData), libc.Uint64FromInt32(iCol));
                    (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiCol = (int16)iCol;
                    (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiPos = 0;
                } else {
                    bNew = (int32)1;
                    v2 = iCol;
                    iPos = v2;
                    (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiCol = (int16)v2;
                }
            }
            if (bNew != 0) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(pPtr + (uintptr)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData), libc.Uint64FromInt32(iPos - (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiPos + (int32)2));
                (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiPos = iPos;
            }
        }
    } else {
        (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbDel = (uint8)1;
    }
    nIncr = nIncr + (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FpnByte))).Value += nIncr;
    return SQLITE_OK;
}

internal static uintptr /*r*/ _sqlite3Fts5Strndup(ж<libc.TLS> tls, @unsafe.Pointer pRc, uintptr pIn, int32 nIn) {
    uintptr zRet = default;
    _ = zRet;
    zRet = (uintptr)0;
    if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
        if (nIn < 0) {
            nIn = libc.Int32FromUint64(libc.Xstrlen(tls, pIn));
        }
        zRet = Xsqlite3_malloc64(tls, libc.Uint64FromInt64((int64)nIn + (int64)1));
        if (zRet != 0) {
            libc.Xmemcpy(tls, zRet, pIn, libc.Uint64FromInt32(nIn));
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zRet + (uintptr)nIn))).Value = (int8)'\u0000';
        } else {
            (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
        }
    }
    return zRet;
}

internal static int32 /*r*/ _sqlite3Fts5TermsetAdd(ж<libc.TLS> tls, uintptr p, int32 iIdx, uintptr pTerm, int32 nTerm, @unsafe.Pointer pbPresent) {
    Tu32 hash = default;
    int32 i = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    uintptr pEntry = default;
    _ = hash;
    _ = i;
    _ = pEntry;
    _ = rc;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<int32>(pbPresent)).Value = 0;
    if (p != 0) {
        hash = (uint32)13;
        i = nTerm - (int32)1;
        for (;;) {
            if (!(i >= 0)) {
                break;
            }
            hash = (Tu32)((Tu32)(hash << (int)(libc.Int32FromInt32(3)) ^ hash) ^ libc.Uint32FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pTerm + (uintptr)i))));
            goto _1;
_1:;
            i = i - 1;
        }
        hash = (Tu32)((Tu32)(hash << (int)(libc.Int32FromInt32(3)) ^ hash) ^ libc.Uint32FromInt32(iIdx));
        hash = hash % libc.Uint32FromInt32(libc.Int32FromUint64(libc.Uint64FromInt64(4096) / libc.Uint64FromInt64(8)));
        pEntry = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + (uintptr)hash * 8));
        for (;;) {
            if (!(pEntry != 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FiIdx == iIdx && (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FnTerm == nTerm && libc.Xmemcmp(tls, (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FpTerm, pTerm, libc.Uint64FromInt32(nTerm)) == 0) {
                (@unsafe.Pointer.To<int32>(pbPresent)).Value = (int32)1;
                break;
            }
            goto _2;
_2:;
            pEntry = (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FpNext;
        }
        if (pEntry == (uintptr)0) {
            pEntry = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), libc.Int64FromUint64((uint64)24 + libc.Uint64FromInt32(nTerm)));
            if (pEntry != 0) {
                (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FpTerm = pEntry + 1 * 24;
                (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FnTerm = nTerm;
                (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FiIdx = iIdx;
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FpTerm, pTerm, libc.Uint64FromInt32(nTerm));
                (@unsafe.Pointer.To<TFts5TermsetEntry>((@unsafe.Pointer)pEntry)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + (uintptr)hash * 8));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + (uintptr)hash * 8))).Value = pEntry;
            }
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3GetInt32(ж<libc.TLS> tls, uintptr zNum, uintptr pValue) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 c = default;
        int32 i = default;
        int32 neg = default;
        int32 v3 = default;
        Tsqlite_int64 v = default;
        bool v4 = default;
        Tu32 _ᴛ1 = default;
        _ = c;
        _ = i;
        _ = neg;
        _ = v;
        _ = v3;
        _ = v4;
        v = 0;
        neg = 0;
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'-') {
            neg = (int32)1;
            zNum = zNum + 1;
        } else {
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'+') {
                zNum = zNum + 1;
            } else {
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'0' && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + 1))) == (int32)'x' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + 1))) == (int32)'X') && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + 2)))]) & (int32)0x08) != 0) {
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value = (uint32)0;
                    zNum = zNum + (uintptr)2;
                    while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'0') {
                        zNum = zNum + 1;
                    }
                    i = 0;
                    for (;;) {
                        if (!(i < (int32)8 && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)i)))]) & (int32)0x08) != 0)) {
                            break;
                        }
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp)) * (uint32)16 + (uint32)_sqlite3HexToInt(ref (tls).DerefOrNull(), (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)i))));
                        goto _1;
_1:;
                        i = i + 1;
                    }
                    if ((Tu32)(~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp)) & (uint32)0x80000000U) == (uint32)0 && (libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)i)))]) & (int32)0x08) == 0) {
                        libc.Xmemcpy(tls, pValue, bp, (uint64)4);
                        r = (int32)1; goto ᒐdone;
                    } else {
                        r = 0; goto ᒐdone;
                    }
                }
            }
        }
        if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum)))]) & libc.Int32FromInt32(0x04)) != 0)) {
            r = 0; goto ᒐdone;
        }
        while ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum))) == (int32)'0') {
            zNum = zNum + 1;
        }
        i = 0;
        for (;;) {
            v4 = i < (int32)11; if (v4) {
                v3 = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)i))) - libc.Int32FromUint8((uint8)'0');
                c = v3;
            }
            if (!(v4 && v3 >= 0 && c <= (int32)9)) {
                break;
            }
            v = v * (int64)10 + (int64)c;
            goto _2;
_2:;
            i = i + 1;
        }
        if (i > (int32)10) {
            r = 0; goto ᒐdone;
        }
        if (v - (int64)neg > (int64)2147483647) {
            r = 0; goto ᒐdone;
        }
        if (neg != 0) {
            v = -v;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pValue))).Value = (int32)v;
        r = (int32)1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal partial struct _sqlite3Int64ToText__ᴛ1 /*dyn*/ {
    public array<Tu16> FforceAlignment = new(0);
    public array<int8> Fa = new(21);
    public array<byte> F__ccgo_pad2 = new(1);
}

internal static int32 /*r*/ _sqlite3Int64ToText(ж<libc.TLS> tls, Ti64 v, uintptr zOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 i = default;
        int32 kk = default;
        int32 v2 = default;
        Tu64 x = default;
        uint64 v1 = default;
        _sqlite3Int64ToText__ᴛ1 _ᴛ1 = new();
        _ = i;
        _ = kk;
        _ = x;
        _ = v1;
        _ = v2;
        if (v > 0) {
            x = libc.Uint64FromInt64(v);
        } else {
            if (v == 0) {
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zOut))).Value = (int8)'0';
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zOut + 1))).Value = 0;
                r = (int32)1; goto ᒐdone;
            } else {
                if (v == (int64)(-libc.Int32FromInt32(1)) - (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))))) {
                    v1 = libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(63)));
                } else {
                    v1 = libc.Uint64FromInt64(-v);
                }
                x = v1;
            }
        }
        i = libc.Int32FromUint64(libc.Uint64FromInt64(21) - libc.Uint64FromInt32(1));
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(bp + (uintptr)i))).Value = 0;
        while (x >= (uint64)10) {
            kk = libc.Int32FromUint64(x % (uint64)100 * (uint64)2);
            (@unsafe.Pointer.To<Tu16>(__ccgo_ptr(bp + (uintptr)(i - (int32)2)))).Value = (@unsafe.Pointer.To<Tu16>(__ccgo_ptr((uintptr)Ꮡ_sqlite3DigitPairs + (uintptr)kk))).Value;
            i = i - (int32)2;
            x = x / (uint64)100;
        }
        if (x != 0) {
            i = i - 1;
            v2 = i;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(bp + (uintptr)v2))).Value = libc.Int8FromUint64(x + (uint64)'0');
        }
        if (v < 0) {
            i = i - 1;
            v2 = i;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(bp + (uintptr)v2))).Value = (int8)'-';
        }
        libc.Xmemcpy(tls, zOut, bp + (uintptr)i, (uint64)21 - libc.Uint64FromInt32(i));
        r = libc.Int32FromUint64(libc.Uint64FromInt64(21) - libc.Uint64FromInt32(1) - libc.Uint64FromInt32(i));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3IsLikeFunction(ж<libc.TLS> tls, uintptr db, uintptr pExpr, @unsafe.Pointer pIsNocase, uintptr aWc) {
    int32 nExpr = default;
    uintptr pDef = default;
    uintptr pEscape = default;
    uintptr zEscape = default;
    _ = nExpr;
    _ = pDef;
    _ = pEscape;
    _ = zEscape;
    if (!(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) != 0)) {
        return 0;
    }
    nExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32))))).Value.FnExpr;
    pDef = _sqlite3FindFunction(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)), nExpr, (uint8)SQLITE_UTF8, (uint8)0);
    if (pDef == (uintptr)0 || (Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FfuncFlags & (uint32)SQLITE_FUNC_LIKE) == (uint32)0) {
        return 0;
    }
    libc.Xmemcpy(tls, aWc, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FpUserData, (uint64)3);
    if (nExpr < (int32)3) {
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(aWc + 3))).Value = 0;
    } else {
        pEscape = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) + 8 + 2 * 32))).Value.FpExpr;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pEscape)).Value.Fop) != (int32)TK_STRING) {
            return 0;
        }
        zEscape = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pEscape + 8));
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zEscape))) == 0 || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zEscape + 1))) != 0) {
            return 0;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zEscape))) == (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aWc)))) {
            return 0;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zEscape))) == (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(aWc + 1)))) {
            return 0;
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(aWc + 3))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(zEscape))).Value;
    }
    (@unsafe.Pointer.To<int32>(pIsNocase)).Value = libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FfuncFlags & (uint32)SQLITE_FUNC_CASE) == (uint32)0);
    return (int32)1;
}

internal static uintptr /*r*/ _sqlite3VListAdd(ж<libc.TLS> tls, uintptr db, uintptr pIn, uintptr zName, int32 nName, int32 iVal) {
    int32 i = default;
    int32 nInt = default;
    Tsqlite3_int64 nAlloc = default;
    uintptr pOut = default;
    uintptr z = default;
    int64 v1 = default;
    _ = i;
    _ = nAlloc;
    _ = nInt;
    _ = pOut;
    _ = z;
    _ = v1;
    nInt = nName / (int32)4 + (int32)3;
    if (pIn == (uintptr)0 || ~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + 1 * 4)) + nInt > ~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn))) {
        if (pIn != 0) {
            v1 = (int64)2 * (int64)(~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn)));
        } else {
            v1 = (int64)10;
        }
        nAlloc = v1 + (int64)nInt;
        pOut = _sqlite3DbRealloc(tls, db, pIn, libc.Uint64FromInt64(nAlloc) * (uint64)4);
        if (pOut == (uintptr)0) {
            return pIn;
        }
        if (pIn == (uintptr)0) {
            (@unsafe.Pointer.To<TVList>(__ccgo_ptr(pOut + 1 * 4))).Value = (int32)2;
        }
        pIn = pOut;
        (@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn))).Value = (int32)nAlloc;
    }
    i = ~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + 1 * 4));
    (@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + (uintptr)i * 4))).Value = iVal;
    (@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + (uintptr)(i + (int32)1) * 4))).Value = nInt;
    z = pIn + (uintptr)(i + (int32)2) * 4;
    (@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + 1 * 4))).Value = i + nInt;
    libc.Xmemcpy(tls, z, zName, libc.Uint64FromInt32(nName));
    (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)nName))).Value = 0;
    return pIn;
}

internal static int32 /*r*/ _sqlite3VdbeMemHandleBom(ж<libc.TLS> tls, uintptr pMem) {
    Tu8 b1 = default;
    Tu8 b2 = default;
    Tu8 bom = default;
    int32 rc = default;
    uintptr v1 = default;
    _ = b1;
    _ = b2;
    _ = bom;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    bom = (uint8)0;
    if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn > (int32)1) {
        b1 = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz));
        b2 = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + libc.UintptrFromInt32(1)));
        if (libc.Int32FromUint8(b1) == (int32)0xFE && libc.Int32FromUint8(b2) == (int32)0xFF) {
            bom = (uint8)SQLITE_UTF16BE;
        }
        if (libc.Int32FromUint8(b1) == (int32)0xFF && libc.Int32FromUint8(b2) == (int32)0xFE) {
            bom = (uint8)SQLITE_UTF16LE;
        }
    }
    if (bom != 0) {
        rc = _sqlite3VdbeMemMakeWriteable(tls, pMem);
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pMem + 16))).Value -= (int32)2;
            libc.Xmemmove(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + 2, libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn));
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn))).Value = (int8)'\u0000';
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn + (int32)1)))).Value = (int8)'\u0000';
            v1 = pMem + 20;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Term));
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc = bom;
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3VdbeMemSetStr(ж<libc.TLS> tls, uintptr pMem, uintptr z, Ti64 n, Tu8 enc, uintptr __ccgo_fp_xDel) {
    Tu16 flags = default;
    int32 iLimit = default;
    int32 v2 = default;
    Ti64 nAlloc = default;
    Ti64 nByte = default;
    int64 v3 = default;
    _ = flags;
    _ = iLimit;
    _ = nAlloc;
    _ = nByte;
    _ = v2;
    _ = v3;
    nByte = n;
    if (!(z != 0)) {
        _sqlite3VdbeMemSetNull(tls, pMem);
        return SQLITE_OK;
    }
    if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb != 0) {
        iLimit = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb + 136));
    } else {
        iLimit = (int32)SQLITE_MAX_LENGTH;
    }
    if (nByte < 0) {
        if (libc.Int32FromUint8(enc) == (int32)SQLITE_UTF8) {
            nByte = libc.Int64FromUint64(libc.Xstrlen(tls, z));
        } else {
            nByte = 0;
            for (;;) {
                if (!(nByte <= (int64)iLimit && ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)nByte))) | (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(nByte + (int64)1))))) != 0)) {
                    break;
                }
                goto _1;
_1:;
                nByte = nByte + (int64)2;
            }
        }
        flags = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Term));
    } else {
        if (libc.Int32FromUint8(enc) == 0) {
            flags = (uint16)MEM_Blob;
            enc = (uint8)SQLITE_UTF8;
        } else {
            flags = (uint16)MEM_Str;
        }
    }
    if (nByte > (int64)iLimit) {
        if (__ccgo_fp_xDel != 0 && __ccgo_fp_xDel != (uintptr)(-libc.Int32FromInt32(1))) {
            if (__ccgo_fp_xDel == __ccgo_fp(_sqlite3RowSetClear)) {
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, z);
            } else {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDel)(tls, z);
            }
        }
        _sqlite3VdbeMemSetNull(tls, pMem);
        return _sqlite3ErrorToParser(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (int32)SQLITE_TOOBIG);
    }
    if (__ccgo_fp_xDel == (uintptr)(-libc.Int32FromInt32(1))) {
        nAlloc = nByte;
        if ((libc.Int32FromUint16(flags) & (int32)MEM_Term) != 0) {
            if (libc.Int32FromUint8(enc) == (int32)SQLITE_UTF8) {
                v2 = (int32)1;
            } else {
                v2 = (int32)2;
            }
            nAlloc = nAlloc + (int64)v2;
        }
        if (nAlloc > (int64)libc.Int32FromInt32(32)) {
            v3 = nAlloc;
        } else {
            v3 = (int64)libc.Int32FromInt32(32);
        }
        if (_sqlite3VdbeMemClearAndResize(tls, pMem, (int32)v3) != 0) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, z, libc.Uint64FromInt64(nAlloc));
    } else {
        _sqlite3VdbeMemRelease(tls, pMem);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = z;
        if (__ccgo_fp_xDel == __ccgo_fp(_sqlite3RowSetClear)) {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc = _sqlite3DbMallocSize(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc);
        } else {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FxDel = __ccgo_fp_xDel;
            if (__ccgo_fp_xDel == libc.UintptrFromInt32(0)) {
                v2 = (int32)MEM_Static;
            } else {
                v2 = (int32)MEM_Dyn;
            }
            flags = libc.Uint16FromInt32((int32)flags | v2);
        }
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = (int32)((Ti64)(nByte & libc.Int64FromInt32(0x7fffffff)));
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = flags;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc = enc;
    if (libc.Int32FromUint8(enc) > (int32)SQLITE_UTF8 && _sqlite3VdbeMemHandleBom(tls, pMem) != 0) {
        return (int32)SQLITE_NOMEM;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3VdbeMemSetText(ж<libc.TLS> tls, uintptr pMem, uintptr z, Ti64 n, uintptr __ccgo_fp_xDel) {
    Tu16 flags = default;
    Ti64 nAlloc = default;
    Ti64 nByte = default;
    int64 v1 = default;
    _ = flags;
    _ = nAlloc;
    _ = nByte;
    _ = v1;
    nByte = n;
    if (!(z != 0)) {
        _sqlite3VdbeMemSetNull(tls, pMem);
        return SQLITE_OK;
    }
    if (nByte < 0) {
        nByte = libc.Int64FromUint64(libc.Xstrlen(tls, z));
        flags = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Term));
    } else {
        flags = (uint16)MEM_Str;
    }
    if (nByte > (int64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb + 136)))) {
        if (__ccgo_fp_xDel != 0 && __ccgo_fp_xDel != (uintptr)(-libc.Int32FromInt32(1))) {
            if (__ccgo_fp_xDel == __ccgo_fp(_sqlite3RowSetClear)) {
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, z);
            } else {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDel)(tls, z);
            }
        }
        _sqlite3VdbeMemSetNull(tls, pMem);
        return _sqlite3ErrorToParser(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (int32)SQLITE_TOOBIG);
    }
    if (__ccgo_fp_xDel == (uintptr)(-libc.Int32FromInt32(1))) {
        nAlloc = nByte + (int64)1;
        if (nAlloc > (int64)libc.Int32FromInt32(32)) {
            v1 = nAlloc;
        } else {
            v1 = (int64)libc.Int32FromInt32(32);
        }
        if (_sqlite3VdbeMemClearAndResize(tls, pMem, (int32)v1) != 0) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, z, libc.Uint64FromInt64(nByte));
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)nByte))).Value = 0;
    } else {
        _sqlite3VdbeMemRelease(tls, pMem);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = z;
        if (__ccgo_fp_xDel == __ccgo_fp(_sqlite3RowSetClear)) {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc = _sqlite3DbMallocSize(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc);
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FxDel = (uintptr)0;
        } else {
            if (__ccgo_fp_xDel == libc.UintptrFromInt32(0)) {
                (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FxDel = __ccgo_fp_xDel;
                flags = libc.Uint16FromInt32((int32)flags | libc.Int32FromInt32(MEM_Static));
            } else {
                (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FxDel = __ccgo_fp_xDel;
                flags = libc.Uint16FromInt32((int32)flags | libc.Int32FromInt32(MEM_Dyn));
            }
        }
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = flags;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = (int32)((Ti64)(nByte & libc.Int64FromInt32(0x7fffffff)));
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc = (uint8)SQLITE_UTF8;
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3WindowRewrite(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr db = default;
        uintptr pArgs = default;
        uintptr pFilter = default;
        uintptr pGroupBy = default;
        uintptr pHaving = default;
        uintptr pMWin = default;
        uintptr pSort = default;
        uintptr pSrc = default;
        uintptr pSub = default;
        ref var pSublist = ref heap(new uintptr(), out var ᏑpSublist);
        uintptr pTab = default;
        uintptr pTab2 = default;
        uintptr pWhere = default;
        uintptr pWin = default;
        uintptr v = default;
        uintptr v2 = default;
        int32 nSave = default;
        int32 rc = default;
        int32 v1 = default;
        Tu32 selFlags = default;
        TWalker _ᴛ1 = new();
        _ = db;
        _ = nSave;
        _ = pArgs;
        _ = pFilter;
        _ = pGroupBy;
        _ = pHaving;
        _ = pMWin;
        _ = pSort;
        _ = pSrc;
        _ = pSub;
        _ = pSublist;
        _ = pTab;
        _ = pTab2;
        _ = pWhere;
        _ = pWin;
        _ = rc;
        _ = selFlags;
        _ = v;
        _ = v1;
        _ = v2;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior == (uintptr)0 && (Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_WinRewrite) == (uint32)0 && !(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
            v = _sqlite3GetVdbe(tls, pParse);
            db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
            pSub = (uintptr)0;
            pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
            pWhere = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere;
            pGroupBy = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy;
            pHaving = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving;
            pSort = (uintptr)0;
            pSublist = (uintptr)0;
            pMWin = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin;
            selFlags = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags;
            pTab = _sqlite3DbMallocZero(tls, db, (uint64)120);
            if (pTab == (uintptr)0) {
                r = _sqlite3ErrorToParser(ref (tls).DerefOrNull(), db, (int32)SQLITE_NOMEM); goto ᒐdone;
            }
            _sqlite3AggInfoPersistWalkerInit(tls, bp, pParse);
            _sqlite3WalkSelect(tls, bp, p);
            if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Aggregate) == (uint32)0) {
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_disallowAggregatesInOrderByCb);
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = (uintptr)0;
                _sqlite3WalkExprList(tls, bp, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy);
            }
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc = (uintptr)0;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere = (uintptr)0;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy = (uintptr)0;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving = (uintptr)0;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(SF_Aggregate);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_WinRewrite;
            pSort = _exprListAppendList(tls, pParse, (uintptr)0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition, (int32)1);
            pSort = _exprListAppendList(tls, pParse, pSort, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpOrderBy, (int32)1);
            if (pSort != 0 && (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy != 0 && (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy)).Value.FnExpr <= (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSort)).Value.FnExpr) {
                nSave = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSort)).Value.FnExpr;
                (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSort)).Value.FnExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy)).Value.FnExpr;
                if (_sqlite3ExprListCompare(tls, pSort, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy, -(int32)1) == 0) {
                    _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy);
                    (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy = (uintptr)0;
                }
                (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSort)).Value.FnExpr = nSave;
            }
            v2 = pParse + 56;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr = v1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 56))).Value += (int32)3;
            _selectWindowRewriteEList(tls, pParse, pMWin, pSrc, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList, pTab, @unsafe.Pointer.FromBox(ᏑpSublist));
            _selectWindowRewriteEList(tls, pParse, pMWin, pSrc, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy, pTab, @unsafe.Pointer.FromBox(ᏑpSublist));
            if (pSublist != 0) {
                v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSublist)).Value.FnExpr;
            } else {
                v1 = 0;
            }
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FnBufferCol = v1;
            pSublist = _exprListAppendList(tls, pParse, pSublist, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition, 0);
            pSublist = _exprListAppendList(tls, pParse, pSublist, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpOrderBy, 0);
            pWin = pMWin;
            for (;;) {
                if (!(pWin != 0)) {
                    break;
                }
                pArgs = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOwner + 32));
                if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpWFunc)).Value.FfuncFlags & (uint32)SQLITE_SUBTYPE) != 0) {
                    _selectWindowRewriteEList(tls, pParse, pMWin, pSrc, pArgs, pTab, @unsafe.Pointer.FromBox(ᏑpSublist));
                    if (pSublist != 0) {
                        v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSublist)).Value.FnExpr;
                    } else {
                        v1 = 0;
                    }
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol = v1;
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FbExprArgs = (uint8)1;
                } else {
                    if (pSublist != 0) {
                        v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pSublist)).Value.FnExpr;
                    } else {
                        v1 = 0;
                    }
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FiArgCol = v1;
                    pSublist = _exprListAppendList(tls, pParse, pSublist, pArgs, 0);
                }
                if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpFilter != 0) {
                    pFilter = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpFilter, 0);
                    pSublist = _sqlite3ExprListAppend(tls, pParse, pSublist, pFilter);
                }
                v2 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregAccum = v1;
                v2 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregResult = v1;
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregAccum);
                goto _4;
_4:;
                pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
            }
            if (pSublist == (uintptr)0) {
                pSublist = _sqlite3ExprListAppend(tls, pParse, (uintptr)0, _sqlite3ExprInt32(tls, db, 0));
            }
            pSub = _sqlite3SelectNew(tls, pParse, pSublist, pSrc, pWhere, pGroupBy, pHaving, pSort, (uint32)0, (uintptr)0);
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc = _sqlite3SrcListAppend(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0);
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc == (uintptr)0) {
                _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSub);
            } else {
                if (_sqlite3SrcItemAttachSubquery(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8, pSub, 0) != 0) {
                    libc.SetBitFieldPtr32Uint32((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + 24 + 4, libc.Uint32FromInt32(1), 4, 0x10);
                    _sqlite3SrcListAssignCursors(ref (tls).DerefOrNull(), pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc);
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSub + 4))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(SF_Expanded) | libc.Int32FromInt32(SF_OrderByReqd));
                    pTab2 = _sqlite3ResultSetOfSelect(tls, pParse, pSub, (int8)SQLITE_AFF_NONE);
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSub + 4))).Value |= (Tu32)(selFlags & (uint32)SF_Aggregate);
                    if (pTab2 == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                    } else {
                        libc.Xmemcpy(tls, pTab, pTab2, (uint64)120);
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= (uint32)TF_Ephemeral;
                        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8))).Value.FpSTab = pTab;
                        pTab = pTab2;
                        libc.Xmemset(tls, bp, 0, (uint64)48);
                        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_sqlite3WindowExtraAggFuncDepth);
                        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3WalkerDepthIncrease);
                        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = __ccgo_fp(_sqlite3WalkerDepthDecrease);
                        _sqlite3WalkSelect(tls, bp, pSub);
                    }
                }
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                rc = (int32)SQLITE_NOMEM;
            }
            _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_sqlite3DbFree), pTab);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _tokenExpr(ж<libc.TLS> tls, uintptr pParse, int32 op, TToken tʗp) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    uintptr p = default;
    uintptr v1 = default;
    _ = p;
    _ = v1;
    p = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)72 + (uint64)t.Fn + (uint64)1);
    if (p != 0) {
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop = libc.Uint8FromInt32(op);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FaffExpr = 0;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags = (uint32)EP_Leaf;
        v1 = libc.UintptrFromInt32(0);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight = v1;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft = v1;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpAggInfo = (uintptr)0;
        libc.Xmemset(tls, p + 32, 0, (uint64)8);
        libc.Xmemset(tls, p + 64, 0, (uint64)8);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop2 = (uint8)0;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiTable = 0;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn = 0;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8))).Value = p + 1 * 72;
        libc.Xmemcpy(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)), t.Fz, (uint64)t.Fn);
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)) + (uintptr)t.Fn))).Value = 0;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(p + 52))).Value = (int32)((int64)t.Fz - (int64)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzTail);
        if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)))))]) & (int32)0x80) != 0) {
            _sqlite3DequoteExpr(ref (tls).DerefOrNull(), p);
        }
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FnHeight = (int32)1;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            return _sqlite3RenameTokenMap(tls, pParse, p, @unsafe.Pointer.FromPinnedBox(Ꮡt));
        }
    }
    return p;
}

internal static void _zeroPage(ж<libc.TLS> tls, uintptr pPage, int32 flags) {
    uintptr data = default;
    uintptr pBt = default;
    int32 first = default;
    int32 hdr = default;
    int32 v1 = default;
    _ = data;
    _ = first;
    _ = hdr;
    _ = pBt;
    _ = v1;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
    hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset);
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_FAST_SECURE) != 0) {
        libc.Xmemset(tls, data + (uintptr)hdr, 0, (uint64)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(hdr)));
    }
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)hdr))).Value = libc.Uint8FromInt8((int8)flags);
    if ((flags & (int32)PTF_LEAF) == 0) {
        v1 = (int32)12;
    } else {
        v1 = (int32)8;
    }
    first = hdr + v1;
    libc.Xmemset(tls, data + (uintptr)(hdr + (int32)1), 0, (uint64)4);
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7)))).Value = (uint8)0;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5)))).Value = (uint8)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize >> (int)(libc.Int32FromInt32(8)));
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5) + 1))).Value = (uint8)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree = libc.Int32FromUint16((uint16)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(first)));
    _decodeFlags(tls, pPage, flags);
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FcellOffset = libc.Uint16FromInt32(first);
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd = data + (uintptr)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx = data + (uintptr)first;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataOfst = data + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnOverflow = (uint8)0;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage = (uint16)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize - libc.Uint32FromInt32(1));
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell = (uint16)0;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = (uint8)1;
}

} // end sqlite3_package
