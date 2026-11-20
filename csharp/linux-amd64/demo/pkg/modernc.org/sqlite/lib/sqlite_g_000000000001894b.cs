namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_preupdate_new(ж<libc.TLS> tls, uintptr db, int32 iIdx, uintptr ppValue) {
    int32 iStore = default;
    int32 rc = default;
    int32 v1 = default;
    uintptr p = default;
    uintptr pData = default;
    uintptr pMem = default;
    uintptr pUnpack = default;
    _ = iStore;
    _ = p;
    _ = pData;
    _ = pMem;
    _ = pUnpack;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    iStore = 0;
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpPreUpdate;
    if (!(p != 0) || (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_DELETE) {
        rc = _sqlite3MisuseError(tls, (int32)96154);
        goto preupdate_new_out;
    }
    if ((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpPk != 0 && (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.Fop != (int32)SQLITE_UPDATE) {
        iStore = _sqlite3TableColumnToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpPk, iIdx);
    } else {
        if (iIdx >= (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FnCol) {
            return _sqlite3MisuseError(tls, (int32)96160);
        } else {
            iStore = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab, (int16)iIdx);
        }
    }
    if (iStore >= (int32)(@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpCsr)).Value.FnField || iStore < 0) {
        rc = (int32)SQLITE_RANGE;
        goto preupdate_new_out;
    }
    if ((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_INSERT) {
        pUnpack = (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpNewUnpacked;
        if (!(pUnpack != 0)) {
            pData = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.Fv)).Value.FaMem + (uintptr)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FiNewReg * 56;
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pData)).Value.Fflags) & (int32)MEM_Zero) != 0) {
                v1 = _sqlite3VdbeMemExpandBlob(tls, pData);
            } else {
                v1 = 0;
            }
            rc = v1;
            if (rc != SQLITE_OK) {
                goto preupdate_new_out;
            }
            pUnpack = _vdbeUnpackRecord(tls, (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpKeyinfo, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pData)).Value.Fn, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pData)).Value.Fz);
            if (!(pUnpack != 0)) {
                rc = (int32)SQLITE_NOMEM;
                goto preupdate_new_out;
            }
            (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpNewUnpacked = pUnpack;
        }
        pMem = (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pUnpack)).Value.FaMem + (uintptr)iStore * 56;
        if (iIdx == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FiPKey) {
            _sqlite3VdbeMemSetInt64(tls, pMem, (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FiKey2);
        } else {
            if (iStore >= libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pUnpack)).Value.FnField)) {
                pMem = _columnNullValue(ref (tls).DerefOrNull());
            } else {
                if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FaCol != 0 && (int32)(@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FaCol + (uintptr)iIdx * 16))).Value.Faffinity == (int32)SQLITE_AFF_REAL && (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
                    _sqlite3VdbeMemRealify(tls, pMem);
                }
            }
        }
    } else {
        if (!((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FaNew != 0)) {
            (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FaNew = _sqlite3DbMallocZero(tls, db, (uint64)((uint64)56 * libc.Uint64FromInt16((@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpCsr)).Value.FnField)));
            if (!((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FaNew != 0)) {
                rc = (int32)SQLITE_NOMEM;
                goto preupdate_new_out;
            }
        }
        pMem = (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FaNew + (uintptr)iStore * 56;
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) == 0) {
            if (iIdx == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FiPKey) {
                _sqlite3VdbeMemSetInt64(tls, pMem, (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FiKey2);
            } else {
                rc = _sqlite3VdbeMemCopy(tls, pMem, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.Fv)).Value.FaMem + (uintptr)((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FiNewReg + (int32)1 + iStore) * 56);
                if (rc != SQLITE_OK) {
                    goto preupdate_new_out;
                }
            }
        }
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppValue))).Value = pMem;
    goto preupdate_new_out;
preupdate_new_out:;
    _sqlite3Error(tls, db, rc);
    return _sqlite3ApiExit(tls, db, rc);
}

public static int32 /*r*/ Xsqlite3_preupdate_old(ж<libc.TLS> tls, uintptr db, int32 iIdx, uintptr ppValue) {
    uintptr aRec = default;
    uintptr p = default;
    uintptr pCol = default;
    uintptr pDflt = default;
    uintptr pMem = default;
    ref var pVal = ref heap(new uintptr(), out var ᏑpVal);
    uintptr v1 = default;
    int32 iStore = default;
    int32 nByte = default;
    int32 rc = default;
    Tu32 nRec = default;
    _ = aRec;
    _ = iStore;
    _ = nByte;
    _ = nRec;
    _ = p;
    _ = pCol;
    _ = pDflt;
    _ = pMem;
    _ = pVal;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    iStore = 0;
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpPreUpdate;
    if (!(p != 0) || (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_INSERT) {
        rc = _sqlite3MisuseError(tls, (int32)95996);
        goto preupdate_old_out;
    }
    if ((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpPk != 0) {
        iStore = _sqlite3TableColumnToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpPk, iIdx);
    } else {
        if (iIdx >= (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FnCol) {
            rc = _sqlite3MisuseError(tls, (int32)96002);
            goto preupdate_old_out;
        } else {
            iStore = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab, (int16)iIdx);
        }
    }
    if (iStore >= (int32)(@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpCsr)).Value.FnField || iStore < 0) {
        rc = (int32)SQLITE_RANGE;
        goto preupdate_old_out;
    }
    if (iIdx == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FiPKey) {
        v1 = p + 80;
        pMem = v1;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppValue))).Value = v1;
        _sqlite3VdbeMemSetInt64(tls, pMem, (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FiKey1);
    } else {
        if ((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpUnpacked == (uintptr)0) {
            nRec = _sqlite3BtreePayloadSize(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpCsr + 48)));
            aRec = _sqlite3DbMallocRaw(tls, db, (uint64)nRec);
            if (!(aRec != 0)) {
                goto preupdate_old_out;
            }
            rc = _sqlite3BtreePayload(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpCsr + 48)), (uint32)0, nRec, aRec);
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpUnpacked = _vdbeUnpackRecord(tls, (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpKeyinfo, libc.Int32FromUint32(nRec), aRec);
                if (!((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpUnpacked != 0)) {
                    rc = (int32)SQLITE_NOMEM;
                }
            }
            if (rc != SQLITE_OK) {
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aRec);
                goto preupdate_old_out;
            }
            (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FaRecord = aRec;
        }
        v1 = (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpUnpacked)).Value.FaMem + (uintptr)iStore * 56;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppValue))).Value = v1;
        pMem = v1;
        if (iStore >= libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpUnpacked)).Value.FnField)) {
            pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FaCol + (uintptr)iIdx * 16;
            if (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FiDflt) > 0) {
                if ((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FapDflt == (uintptr)0) {
                    nByte = libc.Int32FromUint64((uint64)8 * libc.Uint64FromInt16((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FnCol));
                    (@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FapDflt = _sqlite3DbMallocZero(tls, db, libc.Uint64FromInt32(nByte));
                    if ((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FapDflt == (uintptr)0) {
                        goto preupdate_old_out;
                    }
                }
                if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FapDflt + (uintptr)iIdx * 8)) == (uintptr)0) {
                    pVal = (uintptr)0;
                    pDflt = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TTableᴛ1_Fu_Ftab>((@unsafe.Pointer)((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab + 64))).Value.FpDfltList + 8 + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FiDflt) - (int32)1) * 32))).Value.FpExpr;
                    rc = _sqlite3ValueFromExpr(tls, db, pDflt, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc, libc.Uint8FromInt8((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.Faffinity), @unsafe.Pointer.FromBox(ᏑpVal));
                    if (rc == SQLITE_OK && pVal == (uintptr)0) {
                        rc = _sqlite3CorruptError(tls, (int32)96058);
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FapDflt + (uintptr)iIdx * 8))).Value = pVal;
                }
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppValue))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FapDflt + (uintptr)iIdx * 8))).Value;
            } else {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppValue))).Value = _columnNullValue(ref (tls).DerefOrNull());
            }
        } else {
            if ((int32)(@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpTab)).Value.FaCol + (uintptr)iIdx * 16))).Value.Faffinity == (int32)SQLITE_AFF_REAL) {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
                    _sqlite3VdbeMemRealify(tls, pMem);
                }
            }
        }
    }
    goto preupdate_old_out;
preupdate_old_out:;
    _sqlite3Error(tls, db, rc);
    return _sqlite3ApiExit(tls, db, rc);
}

public static void Xsqlite3_str_appendchar(ж<libc.TLS> tls, uintptr p, int32 N, int8 c) {
    int32 v1 = default;
    bool v2 = default;
    Tu32 v4 = default;
    uintptr v5 = default;
    _ = v1;
    _ = v2;
    _ = v4;
    _ = v5;
    v2 = libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar) + (int64)N >= libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnAlloc); if (v2) {
        v1 = _sqlite3StrAccumEnlarge(tls, p, (int64)N);
        N = v1;
    }
    if (v2 && v1 <= 0) {
        return;
    }
    for (;;) {
        v1 = N;
        N = N - 1;
        if (!(v1 > 0)) {
            break;
        }
        v5 = p + 24;
        v4 = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v5);
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v5) + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText + (uintptr)v4))).Value = c;
    }
}

internal static void _applyAffinity(ж<libc.TLS> tls, uintptr pRec, int8 affinity, Tu8 enc) {
    uintptr v1 = default;
    _ = v1;
    if ((int32)affinity >= (int32)SQLITE_AFF_NUMERIC) {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pRec)).Value.Fflags) & (int32)MEM_Int) == 0) {
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pRec)).Value.Fflags) & (libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_IntReal))) == 0) {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pRec)).Value.Fflags) & (int32)MEM_Str) != 0) {
                    _applyNumericAffinity(tls, (@unsafe.Pointer)pRec, (int32)1);
                }
            } else {
                if ((int32)affinity <= (int32)SQLITE_AFF_REAL) {
                    _sqlite3VdbeIntegerAffinity(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRec);
                }
            }
        }
    } else {
        if ((int32)affinity == (int32)SQLITE_AFF_TEXT) {
            if (0 == (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pRec)).Value.Fflags) & (int32)MEM_Str)) {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pRec)).Value.Fflags) & (libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
                    _sqlite3VdbeMemStringify(tls, pRec, enc, (uint8)1);
                }
            }
            v1 = pRec + 20;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal)));
        }
    }
}

internal static int32 /*r*/ _codeAllEqualityTerms(ж<libc.TLS> tls, uintptr pParse, uintptr pLevel, int32 bRev, int32 nExtraReg, @unsafe.Pointer pzAff) {
    int32 iIdxCur = default;
    int32 j = default;
    int32 nReg = default;
    int32 r1 = default;
    int32 regBase = default;
    int32 v1 = default;
    Tu16 nEq = default;
    Tu16 nSkip = default;
    uintptr pIdx = default;
    uintptr pLoop = default;
    uintptr pRight = default;
    uintptr pTerm = default;
    uintptr v = default;
    uintptr zAff = default;
    _ = iIdxCur;
    _ = j;
    _ = nEq;
    _ = nReg;
    _ = nSkip;
    _ = pIdx;
    _ = pLoop;
    _ = pRight;
    _ = pTerm;
    _ = r1;
    _ = regBase;
    _ = v;
    _ = zAff;
    _ = v1;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop;
    nEq = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq;
    nSkip = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnSkip;
    pIdx = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
    regBase = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
    nReg = libc.Int32FromUint16(nEq) + nExtraReg;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nReg;
    zAff = _sqlite3DbStrDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, _sqlite3IndexAffinityStr(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pIdx));
    if (nSkip != 0) {
        iIdxCur = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FiIdxCur;
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, regBase, regBase + libc.Int32FromUint16(nSkip) - (int32)1);
        if (bRev != 0) {
            v1 = (int32)OP_Last;
        } else {
            v1 = (int32)OP_Rewind;
        }
        _sqlite3VdbeAddOp1(tls, v, v1, iIdxCur);
        j = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
        if (bRev != 0) {
            v1 = (int32)OP_SeekLT;
        } else {
            v1 = (int32)OP_SeekGT;
        }
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrSkip = _sqlite3VdbeAddOp4Int(tls, v, v1, iIdxCur, 0, regBase, libc.Int32FromUint16(nSkip));
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, j);
        j = 0;
        for (;;) {
            if (!(j < libc.Int32FromUint16(nSkip))) {
                break;
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iIdxCur, j, regBase + j);
            goto _3;
_3:;
            j = j + 1;
        }
    }
    j = libc.Int32FromUint16(nSkip);
    for (;;) {
        if (!(j < libc.Int32FromUint16(nEq))) {
            break;
        }
        pTerm = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8));
        r1 = _codeEqualityTerm(tls, pParse, pTerm, pLevel, j, bRev, regBase + j);
        if (r1 != regBase + j) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Copy, r1, regBase + j);
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_IN) != 0) {
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & (uint32)EP_xIsSelect) != 0) {
                if (zAff != 0) {
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)j))).Value = (int8)SQLITE_AFF_BLOB;
                }
            }
        } else {
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_ISNULL) == 0) {
                pRight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpRight;
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_IS) == 0 && _sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), pRight) != 0) {
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, regBase + j, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FaddrBrk);
                }
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
                    if ((int32)_sqlite3CompareAffinity(tls, pRight, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)j))) == (int32)SQLITE_AFF_BLOB) {
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)j))).Value = (int8)SQLITE_AFF_BLOB;
                    }
                    if (_sqlite3ExprNeedsNoAffinityChange(ref (tls).DerefOrNull(), pRight, ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)j))) != 0) {
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff + (uintptr)j))).Value = (int8)SQLITE_AFF_BLOB;
                    }
                }
            }
        }
        goto _4;
_4:;
        j = j + 1;
    }
    (@unsafe.Pointer.To<uintptr>(pzAff)).Value = zAff;
    return regBase;
}

internal static uintptr /*r*/ _computeIndexAffStr(ж<libc.TLS> tls, uintptr db, uintptr pIdx) {
    int8 aff = default;
    int32 n = default;
    uintptr pTab = default;
    Ti16 x = default;
    _ = aff;
    _ = n;
    _ = pTab;
    _ = x;
    pTab = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable;
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff = _sqlite3DbMallocRaw(tls, (uintptr)0, libc.Uint64FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn) + (int32)1));
    if (!((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff != 0)) {
        _sqlite3OomFault(tls, db);
        return (uintptr)0;
    }
    n = 0;
    for (;;) {
        if (!(n < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn))) {
            break;
        }
        x = ~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)n * 2));
        if ((int32)x >= 0) {
            aff = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)x * 16))).Value.Faffinity;
        } else {
            if ((int32)x == -(int32)1) {
                aff = (int8)SQLITE_AFF_INTEGER;
            } else {
                aff = _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr + 8 + (uintptr)n * 32))).Value.FpExpr);
            }
        }
        if ((int32)aff < (int32)SQLITE_AFF_BLOB) {
            aff = (int8)SQLITE_AFF_BLOB;
        }
        if ((int32)aff > (int32)SQLITE_AFF_NUMERIC) {
            aff = (int8)SQLITE_AFF_NUMERIC;
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff + (uintptr)n))).Value = aff;
        goto _1;
_1:;
        n = n + 1;
    }
    (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff + (uintptr)n))).Value = 0;
    return (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff;
}

internal static void _constInsert(ж<libc.TLS> tls, @unsafe.Pointer pConst, uintptr pColumn, uintptr pValue, uintptr pExpr) {
    int32 i = default;
    uintptr pE2 = default;
    _ = i;
    _ = pE2;
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColumn)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_FixedCol))) != (uint32)0) {
        return;
    }
    if ((int32)_sqlite3ExprAffinity(tls, pValue) != 0) {
        return;
    }
    if (!(_sqlite3IsBinary(ref (tls).DerefOrNull(), _sqlite3ExprCompareCollSeq(tls, ~@unsafe.Pointer.To<uintptr>(pConst), (@unsafe.Pointer)pExpr)) != 0)) {
        return;
    }
    if (_sqlite3ExprCanReturnSubtype(tls, ~@unsafe.Pointer.To<uintptr>(pConst), pValue) != 0) {
        return;
    }
    i = 0;
    for (;;) {
        if (!(i < ~@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16)))) {
            break;
        }
        pE2 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pConst, 32)) + (uintptr)(i * (int32)2) * 8));
        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FiTable == (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColumn)).Value.FiTable && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FiColumn == (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColumn)).Value.FiColumn) {
            return;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if ((int32)_sqlite3ExprAffinity(tls, pColumn) <= (int32)SQLITE_AFF_BLOB) {
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 24))).Value = (int32)1;
    }
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16)) + 1;
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pConst, 32))).Value = _sqlite3DbReallocOrFree(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pConst)))).Value.Fdb, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pConst, 32)), (uint64)(libc.Uint64FromInt32(~@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16)) * (int32)2) * (uint64)8));
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pConst, 32)) == (uintptr)0) {
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16))).Value = 0;
    } else {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pConst, 32)) + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16)) * (int32)2 - (int32)2) * 8))).Value = pColumn;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pConst, 32)) + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(pConst, 16)) * (int32)2 - (int32)1) * 8))).Value = pValue;
    }
}

internal static uintptr /*r*/ _exprINAffinity(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    int8 a = default;
    int32 i = default;
    int32 nVal = default;
    uintptr pA = default;
    uintptr pLeft = default;
    uintptr pSelect = default;
    uintptr zRet = default;
    uintptr v1 = default;
    _ = a;
    _ = i;
    _ = nVal;
    _ = pA;
    _ = pLeft;
    _ = pSelect;
    _ = zRet;
    _ = v1;
    pLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
    nVal = _sqlite3ExprVectorSize(ref (tls).DerefOrNull(), pLeft);
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
        v1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32));
    } else {
        v1 = (uintptr)0;
    }
    pSelect = v1;
    zRet = _sqlite3DbMallocRaw(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, libc.Uint64FromInt64((int64)1 + (int64)nVal));
    if (zRet != 0) {
        i = 0;
        for (;;) {
            if (!(i < nVal)) {
                break;
            }
            pA = _sqlite3VectorFieldSubexpr(ref (tls).DerefOrNull(), pLeft, i);
            a = _sqlite3ExprAffinity(tls, pA);
            if (pSelect != 0) {
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zRet + (uintptr)i))).Value = _sqlite3CompareAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList + 8 + (uintptr)i * 32))).Value.FpExpr, a);
            } else {
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zRet + (uintptr)i))).Value = a;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zRet + (uintptr)nVal))).Value = (int8)'\u0000';
    }
    return zRet;
}

internal static Tu8 /*r*/ _getSafetyLevel(ж<libc.TLS> tls, uintptr z, int32 omitFull, Tu8 dflt) {
    int32 i = default;
    int32 n = default;
    _ = i;
    _ = n;
    if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z)))]) & (int32)0x04) != 0) {
        return libc.BoolUint8(_sqlite3Atoi(tls, z) != 0);
    }
    n = _sqlite3Strlen30(tls, z);
    i = 0;
    for (;;) {
        if (!(i < libc.Int32FromUint64(libc.Uint64FromInt64(8) / libc.Uint64FromInt64(1)))) {
            break;
        }
        if (libc.Int32FromUint8(_iLength[i]) == n && Xsqlite3_strnicmp(tls, (uintptr)Ꮡ_zText + (uintptr)_iOffset[i], z, n) == 0 && (!(omitFull != 0) || libc.Int32FromUint8(_iValue[i]) <= (int32)1)) {
            return _iValue[i];
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return dflt;
}

internal static void _groupConcatStep(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    uintptr db = default;
    uintptr pGCC = default;
    uintptr pnsl = default;
    uintptr zSep = default;
    uintptr zVal = default;
    int32 firstTerm = default;
    int32 i = default;
    int32 nA = default;
    int32 nSep = default;
    int32 nVal = default;
    int32 v1 = default;
    _ = db;
    _ = firstTerm;
    _ = i;
    _ = nA;
    _ = nSep;
    _ = nVal;
    _ = pGCC;
    _ = pnsl;
    _ = zSep;
    _ = zVal;
    _ = v1;
    if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))) == (int32)SQLITE_NULL) {
        return;
    }
    pGCC = Xsqlite3_aggregate_context(tls, context, (int32)48);
    if (pGCC != 0) {
        db = Xsqlite3_context_db_handle(tls, context);
        firstTerm = libc.BoolInt32((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FmxAlloc == (uint32)0);
        (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FmxAlloc = libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136)));
        if (argc == (int32)1) {
            if (!(firstTerm != 0)) {
                Xsqlite3_str_appendchar(tls, pGCC, (int32)1, (int8)',');
            } else {
                (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnFirstSepLength = (int32)1;
            }
        } else {
            if (!(firstTerm != 0)) {
                zSep = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
                nSep = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
                if (zSep != 0) {
                    Xsqlite3_str_append(tls, pGCC, zSep, nSep);
                } else {
                    nSep = 0;
                }
                if (nSep != (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnFirstSepLength || (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths != (uintptr)0) {
                    pnsl = (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths;
                    if (pnsl == (uintptr)0) {
                        pnsl = Xsqlite3_malloc64(tls, (uint64)(libc.Uint64FromInt32((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum + libc.Int32FromInt32(1)) * (uint64)4));
                        if (pnsl != (uintptr)0) {
                            i = 0;
                            nA = (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum - (int32)1;
                            while (i < nA) {
                                v1 = i;
                                i = i + 1;
                                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnsl + (uintptr)v1 * 4))).Value = (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnFirstSepLength;
                            }
                        }
                    } else {
                        pnsl = Xsqlite3_realloc64(tls, pnsl, (uint64)(libc.Uint64FromInt32((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum) * (uint64)4));
                    }
                    if (pnsl != (uintptr)0) {
                        if ((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum > 0) {
                            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnsl + (uintptr)((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum - (int32)1) * 4))).Value = nSep;
                        }
                        (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths = pnsl;
                    } else {
                        _sqlite3StrAccumSetError(tls, (@unsafe.Pointer)pGCC, (uint8)SQLITE_NOMEM);
                    }
                }
            } else {
                (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnFirstSepLength = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
            }
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pGCC + 32))).Value += (int32)1;
        zVal = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        nVal = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (zVal != 0) {
            Xsqlite3_str_append(tls, pGCC, zVal, nVal);
        }
    }
}

internal static void _hexFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    uint8 c = default;
    int32 i = default;
    int32 n = default;
    uintptr pBlob = default;
    uintptr z = default;
    uintptr zHex = default;
    uintptr v1 = default;
    _ = c;
    _ = i;
    _ = n;
    _ = pBlob;
    _ = z;
    _ = zHex;
    _ = v1;
    _ = argc;
    pBlob = Xsqlite3_value_blob(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    n = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    v1 = _contextMalloc(tls, context, (int64)n * (int64)2 + (int64)1);
    zHex = v1;
    z = v1;
    if (zHex != 0) {
        i = 0;
        for (;;) {
            if (!(i < n)) {
                break;
            }
            c = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(pBlob));
            v1 = z;
            z = z + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = _hexdigits[(libc.Int32FromUint8(c) >> (int)((int32)4) & (int32)0xf)];
            v1 = z;
            z = z + 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(v1))).Value = _hexdigits[(libc.Int32FromUint8(c) & (int32)0xf)];
            goto _2;
_2:;
            i = i + 1;
            pBlob = pBlob + 1;
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(z))).Value = 0;
        Xsqlite3_result_text64(tls, context, zHex, libc.Uint64FromInt64((int64)z - (int64)zHex), __ccgo_fp(Xsqlite3_free), (uint8)SQLITE_UTF8_ZT);
    }
}

internal static uintptr /*r*/ _indexInAffinityOk(ж<libc.TLS> tls, uintptr pParse, uintptr pTerm, Tu8 idxaff) {
    int32 iField = default;
    ref var inexpr = ref heap(new TExpr(), out var Ꮡinexpr);
    uintptr pRet = default;
    uintptr v1 = default;
    @unsafe.Pointer pX = default;
    _ = iField;
    _ = inexpr;
    _ = pRet;
    _ = pX;
    _ = v1;
    pX = (@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
    if (_sqlite3ExprIsVector(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 16))) != 0) {
        iField = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FiField - (int32)1;
        inexpr.Fflags = (uint32)0;
        inexpr.Fop = (uint8)TK_EQ;
        inexpr.FpLeft = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 16)) + 32)) + 8 + (uintptr)iField * 32))).Value.FpExpr;
        inexpr.FpRight = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 32))))).Value.FpEList + 8 + (uintptr)iField * 32))).Value.FpExpr;
        pX = @unsafe.Pointer.FromPinnedBox(Ꮡinexpr);
    }
    if (_sqlite3IndexAffinityOk(tls, pX, libc.Int8FromUint8(idxaff)) != 0) {
        pRet = _sqlite3ExprCompareCollSeq(tls, pParse, pX);
        if (pRet != 0) {
            v1 = (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pRet)).Value.FzName;
        } else {
            v1 = (uintptr)Ꮡ_sqlite3StrBINARY;
        }
        return v1;
    }
    return (uintptr)0;
}

internal static int32 /*r*/ _isLikeOrGlob(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, @unsafe.Pointer ppPrefix, @unsafe.Pointer pisComplete, @unsafe.Pointer pnoCase) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu8 c = default;
        Tu8 v1 = default;
        int32 cnt = default;
        int32 iCol = default;
        int32 iFrom = default;
        int32 iTo = default;
        int32 isNum = default;
        int32 op = default;
        int32 r1 = default;
        int32 rc = default;
        int32 v3 = default;
        uintptr db = default;
        uintptr pLeft = default;
        uintptr pList = default;
        uintptr pPrefix = default;
        uintptr pReprepare = default;
        uintptr pRight = default;
        uintptr pVal = default;
        uintptr v = default;
        uintptr z = default;
        ref var z2 = ref heap(new uintptr(), out var Ꮡz2);
        uintptr zNew = default;
        ref var rDummy = ref heap(new float64(), out var ᏑrDummy);
        array<Tu8> _ᴛ1 = new(4);
        _ = c;
        _ = cnt;
        _ = db;
        _ = iCol;
        _ = iFrom;
        _ = iTo;
        _ = isNum;
        _ = op;
        _ = pLeft;
        _ = pList;
        _ = pPrefix;
        _ = pReprepare;
        _ = pRight;
        _ = pVal;
        _ = r1;
        _ = rDummy;
        _ = rc;
        _ = v;
        _ = z;
        _ = z2;
        _ = zNew;
        _ = v1;
        _ = v3;
        z = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pVal = (uintptr)0;
        if (!(_sqlite3IsLikeFunction(tls, db, pExpr, pnoCase, bp) != 0)) {
            r = 0; goto ᒐdone;
        }
        pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32));
        pLeft = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + 1 * 32))).Value.FpExpr;
        pRight = _sqlite3ExprSkipCollate(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8))).Value.FpExpr);
        op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fop);
        if (op == (int32)TK_VARIABLE && (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_EnableQPSG) == (uint64)0) {
            pReprepare = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpReprepare;
            iCol = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.FiColumn;
            pVal = _sqlite3VdbeGetBoundValue(tls, pReprepare, iCol, (uint8)SQLITE_AFF_BLOB);
            if (pVal != 0 && Xsqlite3_value_type(tls, pVal) == (int32)SQLITE_TEXT) {
                z = Xsqlite3_value_text(tls, pVal);
            }
            _sqlite3VdbeReprepareOnBind(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, iCol, 0);
        } else {
            if (op == (int32)TK_STRING) {
                z = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRight + 8));
            }
        }
        if (z != 0) {
            cnt = 0;
            for (;;) {
                v1 = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)cnt));
                c = v1;
                if (!(libc.Int32FromUint8(v1) != 0 && libc.Int32FromUint8(c) != libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), 0, 4)) && libc.Int32FromUint8(c) != libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), (int32)1, 4)) && libc.Int32FromUint8(c) != libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), (int32)2, 4)))) {
                    break;
                }
                cnt = cnt + 1;
                if (libc.Int32FromUint8(c) == libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), (int32)3, 4)) && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)cnt))) > 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)cnt))) < (int32)0x80) {
                    cnt = cnt + 1;
                } else {
                    if (libc.Int32FromUint8(c) >= (int32)0x80) {
                        z2 = z + (uintptr)cnt - (uintptr)1;
                        if (libc.Int32FromUint8(c) == (int32)0xff || _sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(Ꮡz2)) == (uint32)0xfffd || libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc) == (int32)SQLITE_UTF16LE) {
                            cnt = cnt - 1;
                            break;
                        } else {
                            cnt = (int32)((int64)z2 - (int64)z);
                        }
                    }
                }
            }
            if ((cnt > (int32)1 || cnt > 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z))) != libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), (int32)3, 4))) && (int32)255 != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(cnt - (int32)1))))) {
                (@unsafe.Pointer.To<int32>(pisComplete)).Value = libc.BoolInt32(libc.Int32FromUint8(c) == libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), 0, 4)) && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(cnt + (int32)1)))) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc) != (int32)SQLITE_UTF16LE);
                pPrefix = _sqlite3Expr(tls, db, (int32)TK_STRING, z);
                if (pPrefix != 0) {
                    zNew = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pPrefix + 8));
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)cnt))).Value = 0;
                    v3 = libc.Int32FromInt32(0);
                    iTo = v3;
                    iFrom = v3;
                    for (;;) {
                        if (!(iFrom < cnt)) {
                            break;
                        }
                        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)iFrom))) == libc.Int32FromUint8(@unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), (int32)3, 4))) {
                            iFrom = iFrom + 1;
                        }
                        v3 = iTo;
                        iTo = iTo + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)v3))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)iFrom))).Value;
                        goto _2;
_2:;
                        iFrom = iFrom + 1;
                    }
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)iTo))).Value = 0;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fop) != (int32)TK_COLUMN || (int32)_sqlite3ExprAffinity(tls, pLeft) != (int32)SQLITE_AFF_TEXT || (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc) | libc.Int32FromInt32(EP_Subrtn))) == (uint32)0 && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pLeft + 64)) != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pLeft + 64))))).Value.FeTabType) == (int32)TABTYP_VTAB) {
                        isNum = _sqlite3AtoF(tls, zNew, @unsafe.Pointer.FromPinnedBox(ᏑrDummy));
                        if (isNum <= 0) {
                            if (iTo == (int32)1 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew))) == (int32)'-') {
                                isNum = +libc.Int32FromInt32(1);
                            } else {
                                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)(iTo - (int32)1)))).Value = (int8)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)(iTo - (int32)1))) + 1);
                                isNum = _sqlite3AtoF(tls, zNew, @unsafe.Pointer.FromPinnedBox(ᏑrDummy));
                                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)(iTo - (int32)1)))).Value = (int8)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNew + (uintptr)(iTo - (int32)1))) - 1);
                            }
                        }
                        if (isNum > 0) {
                            _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pPrefix);
                            _sqlite3ValueFree(tls, pVal);
                            r = 0; goto ᒐdone;
                        }
                    }
                }
                (@unsafe.Pointer.To<uintptr>(ppPrefix)).Value = pPrefix;
                if (op == (int32)TK_VARIABLE) {
                    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
                    _sqlite3VdbeReprepareOnBind(ref (tls).DerefOrNull(), v, (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.FiColumn, 0);
                    if (~@unsafe.Pointer.To<int32>(pisComplete) != 0 && ~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRight + 8)) + 1)) != 0) {
                        r1 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                        _sqlite3ExprCodeTarget(tls, pParse, pRight, r1);
                        _sqlite3VdbeChangeP3(ref (tls).DerefOrNull(), v, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) - (int32)1, 0);
                        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, r1);
                    }
                }
            } else {
                z = (uintptr)0;
            }
        }
        rc = libc.BoolInt32(z != (uintptr)0);
        _sqlite3ValueFree(tls, pVal);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _jsonEachColumn(ж<libc.TLS> tls, uintptr cur, uintptr ctx, int32 iColumn) {
    Tu8 eType = default;
    Tu32 i = default;
    Tu32 i1 = default;
    Tu32 i2 = default;
    Tu32 j = default;
    Tu32 n = default;
    Tu32 n1 = default;
    Tu64 nBase = default;
    uintptr p = default;
    ref var x = ref heap(new Ti64(), out var Ꮡx);
    _ = eType;
    _ = i;
    _ = i1;
    _ = i2;
    _ = j;
    _ = n;
    _ = n1;
    _ = nBase;
    _ = p;
    _ = x;
    p = cur;
    switch (iColumn) {
    case var ᴛ when ᴛ == JEACH_KEY: {
        if ((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParent == (uint32)0) {
            if ((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnRoot == (uint32)1) {
                break;
            }
            j = libc.Uint32FromInt32(_jsonEachPathLength(tls, p));
            n = (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnRoot - j;
            if (n == (uint32)0) {
                break;
            } else {
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf + (uintptr)j))) == (int32)'[') {
                    _sqlite3Atoi64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf + (uintptr)(j + (uint32)1), @unsafe.Pointer.FromPinnedBox(Ꮡx), libc.Int32FromUint32(n - (uint32)1), (uint8)SQLITE_UTF8);
                    Xsqlite3_result_int64(tls, ctx, x);
                } else {
                    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf + (uintptr)(j + (uint32)1)))) == (int32)'"') {
                        Xsqlite3_result_text(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf + (uintptr)(j + (uint32)2), libc.Int32FromUint32(n - (uint32)3), (uintptr)(-libc.Int32FromInt32(1)));
                    } else {
                        Xsqlite3_result_text(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf + (uintptr)(j + (uint32)1), libc.Int32FromUint32(n - (uint32)1), (uintptr)(-libc.Int32FromInt32(1)));
                    }
                }
            }
            break;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FeType) == (int32)JSONB_OBJECT) {
            _jsonReturnFromBlob(tls, p + 192, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fi, ctx, (int32)1);
        } else {
            Xsqlite3_result_int64(tls, ctx, (@unsafe.Pointer.To<TJsonParent>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FaParent + (uintptr)((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParent - (uint32)1) * 24))).Value.FiKey);
        }
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_VALUE: {
        i = libc.Uint32FromInt32(_jsonSkipLabel(ref (tls).DerefOrNull(), p));
        _jsonReturnFromBlob(tls, p + 192, i, ctx, libc.Int32FromUint8((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FeMode));
        if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FaBlob + (uintptr)i))) & (int32)0x0f) >= (int32)JSONB_ARRAY) {
            Xsqlite3_result_subtype(tls, ctx, (uint32)JSON_SUBTYPE);
        }
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_TYPE: {
        i1 = libc.Uint32FromInt32(_jsonSkipLabel(ref (tls).DerefOrNull(), p));
        eType = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FaBlob + (uintptr)i1))) & (int32)0x0f);
        Xsqlite3_result_text(tls, ctx, _jsonbType[eType], -(int32)1, libc.UintptrFromInt32(0));
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_ATOM: {
        i2 = libc.Uint32FromInt32(_jsonSkipLabel(ref (tls).DerefOrNull(), p));
        if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FaBlob + (uintptr)i2))) & (int32)0x0f) < (int32)JSONB_ARRAY) {
            _jsonReturnFromBlob(tls, p + 192, i2, ctx, (int32)1);
        }
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_ID: {
        Xsqlite3_result_int64(tls, ctx, libc.Int64FromUint32((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fi));
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_PARENT: {
        if ((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParent > (uint32)0 && (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FbRecursive != 0) {
            Xsqlite3_result_int64(tls, ctx, libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParent>(__ccgo_ptr((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FaParent + (uintptr)((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParent - (uint32)1) * 24))).Value.FiHead));
        }
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_FULLKEY: {
        nBase = (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FnUsed;
        if ((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnParent != 0) {
            _jsonAppendPathName(tls, p);
        }
        Xsqlite3_result_text64(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FnUsed, (uintptr)(-libc.Int32FromInt32(1)), (uint8)SQLITE_UTF8);
        (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FnUsed = nBase;
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_PATH: {
        n1 = libc.Uint32FromInt32(_jsonEachPathLength(tls, p));
        Xsqlite3_result_text64(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf, (uint64)n1, (uintptr)(-libc.Int32FromInt32(1)), (uint8)SQLITE_UTF8);
        break;
    }
    default: {
        Xsqlite3_result_text(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf, libc.Int32FromUint32((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FnRoot), (uintptr)(-libc.Int32FromInt32(1)));
        break;
    }
    case var ᴛ when ᴛ == (int32)JEACH_JSON: {
        if ((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FzJson == (uintptr)0) {
            Xsqlite3_result_blob(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FaBlob, libc.Int32FromUint32((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FnBlob), (uintptr)(-libc.Int32FromInt32(1)));
        } else {
            Xsqlite3_result_text(tls, ctx, (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FsParse.FzJson, -(int32)1, (uintptr)(-libc.Int32FromInt32(1)));
        }
        break;
        break;
    }}

    return SQLITE_OK;
}

internal static int32 /*r*/ _jsonEachPathLength(ж<libc.TLS> tls, uintptr p) {
    int8 cSaved = default;
    Tu32 n = default;
    ref var sz = ref heap(new Tu32(), out var Ꮡsz);
    Tu32 x = default;
    uintptr z = default;
    _ = cSaved;
    _ = n;
    _ = sz;
    _ = x;
    _ = z;
    n = (uint32)(@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FnUsed;
    z = (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fpath.FzBuf;
    if ((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FiRowid == (uint32)0 && (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FbRecursive != 0 && n >= (uint32)2) {
        while (n > (uint32)1) {
            n = n - 1;
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n))) == (int32)'[' || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n))) == (int32)'.') {
                sz = (uint32)0;
                cSaved = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n));
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n))).Value = 0;
                x = _jsonLookupStep(tls, p + 192, (uint32)0, z + (uintptr)1, (uint32)0);
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)n))).Value = cSaved;
                if (x >= (uint32)JSON_LOOKUP_PATHERROR) {
                    continue;
                }
                if (x + _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)(p + 192), x, @unsafe.Pointer.FromPinnedBox(Ꮡsz)) == (@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.Fi) {
                    break;
                }
            }
        }
    }
    return libc.Int32FromUint32(n);
}

internal static void _jsonInsertIntoBlob(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv, int32 eEdit) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        int32 flgs = default;
        int32 i = default;
        int32 v1 = default;
        uintptr p = default;
        uintptr zPath = default;
        Tu32 rc = default;
        TJsonParse _ᴛ1 = default;
        _ = flgs;
        _ = i;
        _ = p;
        _ = rc;
        _ = zPath;
        _ = v1;
        rc = (uint32)0;
        zPath = (uintptr)0;
        if (argc == (int32)1) {
            v1 = 0;
        } else {
            v1 = (int32)JSON_EDITABLE;
        }
        flgs = v1;
        p = _jsonParseFuncArg(tls, ctx, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), libc.Uint32FromInt32(flgs));
        if (p == (uintptr)0) {
            return;
        }
        i = (int32)1;
        for (;;) {
            if (!(i < argc - (int32)1)) {
                break;
            }
            if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8))) == (int32)SQLITE_NULL) {
                goto _2;
            }
            zPath = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
            if (zPath == (uintptr)0) {
                Xsqlite3_result_error_nomem(tls, ctx);
                _jsonParseFree(tls, p);
                return;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath))) != (int32)'$') {
                goto jsonInsertIntoBlob_patherror;
            }
            if (_jsonFunctionArgToBlob(tls, ctx, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)(i + (int32)1) * 8)), bp) != 0) {
                _jsonParseReset(tls, __ccgo_ptr(bp));
                _jsonParseFree(tls, p);
                return;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath + 1))) == 0) {
                if (eEdit == (int32)JEDIT_REPL || eEdit == (int32)JEDIT_SET) {
                    _jsonBlobEdit(tls, p, (uint32)0, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FnBlob, (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FaBlob, (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnBlob);
                }
                rc = (uint32)0;
            } else {
                (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FeEdit = libc.Uint8FromInt32(eEdit);
                (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FnIns = (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnBlob;
                (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FaIns = (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FaBlob;
                (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.Fdelta = 0;
                (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FiDepth = (uint16)0;
                rc = _jsonLookupStep(tls, p, (uint32)0, zPath + (uintptr)1, (uint32)0);
            }
            _jsonParseReset(tls, __ccgo_ptr(bp));
            if (rc == (uint32)JSON_LOOKUP_NOTFOUND) {
                goto _2;
            }
            if (rc >= (uint32)JSON_LOOKUP_PATHERROR) {
                goto jsonInsertIntoBlob_patherror;
            }
            goto _2;
_2:;
            i = i + (int32)2;
        }
        _jsonReturnParse(tls, ctx, p);
        _jsonParseFree(tls, p);
        return;
        goto jsonInsertIntoBlob_patherror;
jsonInsertIntoBlob_patherror:;
        _jsonParseFree(tls, p);
        _jsonBadPathError(tls, ctx, zPath, libc.Int32FromUint32(rc));
        return;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _jsonRemoveFunc(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    int32 i = default;
    int32 v1 = default;
    uintptr p = default;
    uintptr zPath = default;
    Tu32 rc = default;
    _ = i;
    _ = p;
    _ = rc;
    _ = zPath;
    _ = v1;
    zPath = (uintptr)0;
    if (argc < (int32)1) {
        return;
    }
    if (argc > (int32)1) {
        v1 = (int32)JSON_EDITABLE;
    } else {
        v1 = 0;
    }
    p = _jsonParseFuncArg(tls, ctx, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), libc.Uint32FromInt32(v1));
    if (p == (uintptr)0) {
        return;
    }
    i = (int32)1;
    for (;;) {
        if (!(i < argc)) {
            break;
        }
        zPath = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + (uintptr)i * 8)));
        if (zPath == (uintptr)0) {
            goto json_remove_done;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath))) != (int32)'$') {
            goto json_remove_patherror;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath + 1))) == 0) {
            goto json_remove_done;
        }
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FeEdit = (uint8)JEDIT_DEL;
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.Fdelta = 0;
        rc = _jsonLookupStep(tls, p, (uint32)0, zPath + (uintptr)1, (uint32)0);
        if (rc >= (uint32)JSON_LOOKUP_PATHERROR) {
            if (rc == (uint32)JSON_LOOKUP_NOTFOUND) {
                goto _2;
            } else {
                _jsonBadPathError(tls, ctx, zPath, libc.Int32FromUint32(rc));
            }
            goto json_remove_done;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    _jsonReturnParse(tls, ctx, p);
    _jsonParseFree(tls, p);
    return;
    goto json_remove_patherror;
json_remove_patherror:;
    _jsonBadPathError(tls, ctx, zPath, 0);
    goto json_remove_done;
json_remove_done:;
    _jsonParseFree(tls, p);
    return;
}

internal static void _jsonTypeFunc(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    Tu32 i = default;
    uintptr p = default;
    uintptr zPath = default;
    _ = i;
    _ = p;
    _ = zPath;
    zPath = (uintptr)0;
    p = _jsonParseFuncArg(tls, ctx, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), (uint32)0);
    if (p == (uintptr)0) {
        return;
    }
    if (argc == (int32)2) {
        zPath = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
        if (zPath == (uintptr)0) {
            goto json_type_done;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zPath))) != (int32)'$') {
            _jsonBadPathError(tls, ctx, zPath, 0);
            goto json_type_done;
        }
        i = _jsonLookupStep(tls, p, (uint32)0, zPath + (uintptr)1, (uint32)0);
        if (i >= (uint32)JSON_LOOKUP_PATHERROR) {
            if (i == (uint32)JSON_LOOKUP_NOTFOUND) {
            } else {
                _jsonBadPathError(tls, ctx, zPath, libc.Int32FromUint32(i));
            }
            goto json_type_done;
        }
    } else {
        i = (uint32)0;
    }
    Xsqlite3_result_text(tls, ctx, _jsonbType[(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)p)).Value.FaBlob + (uintptr)i))) & (int32)0x0f)], -(int32)1, libc.UintptrFromInt32(0));
    goto json_type_done;
json_type_done:;
    _jsonParseFree(tls, p);
}

internal static int32 /*r*/ _pager_delsuper(ж<libc.TLS> tls, uintptr pPager, uintptr zSuper) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 bSeen = default;
        int32 c = default;
        int32 flags = default;
        int32 flags1 = default;
        int32 rc = default;
        uintptr pJournal = default;
        uintptr pSuper = default;
        uintptr pVfs = default;
        uintptr zFree = default;
        uintptr zJournal = default;
        uintptr zSuperJournal = default;
        ref var zSuperPtr = ref heap(new uintptr(), out var ᏑzSuperPtr);
        int8 v1 = default;
        int8 v2 = default;
        int8 v3 = default;
        int32 _ᴛ1 = default;
        Ti64 _ᴛ2 = default;
        _ = bSeen;
        _ = c;
        _ = flags;
        _ = flags1;
        _ = pJournal;
        _ = pSuper;
        _ = pVfs;
        _ = rc;
        _ = zFree;
        _ = zJournal;
        _ = zSuperJournal;
        _ = zSuperPtr;
        _ = v1;
        _ = v2;
        _ = v3;
        pVfs = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs;
        zSuperJournal = (uintptr)0;
        zFree = (uintptr)0;
        bSeen = 0;
        if (_pagerIsSuperJrnlName(tls, zSuper) == 0) {
            r = SQLITE_OK; goto ᒐdone;
        }
        pSuper = _sqlite3MallocZero(tls, libc.Uint64FromInt64((int64)2 * (int64)(@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FszOsFile));
        if (!(pSuper != 0)) {
            rc = (int32)SQLITE_NOMEM;
            pJournal = (uintptr)0;
        } else {
            flags = libc.Int32FromInt32(SQLITE_OPEN_READONLY) | libc.Int32FromInt32(SQLITE_OPEN_SUPER_JOURNAL);
            rc = _sqlite3OsOpen(tls, pVfs, zSuper, pSuper, flags, (uintptr)0);
            pJournal = pSuper + (uintptr)(@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FszOsFile;
        }
        if (rc != SQLITE_OK) {
            goto delsuper_out;
        }
        rc = _sqlite3OsFileSize(tls, pSuper, bp);
        if (rc != SQLITE_OK) {
            goto delsuper_out;
        }
        zFree = _sqlite3Malloc(tls, libc.Uint64FromInt64((int64)4 + ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) + (int64)2));
        if (!(zFree != 0)) {
            rc = (int32)SQLITE_NOMEM;
            goto delsuper_out;
        } else {
        }
        v3 = libc.Int8FromInt32(0);
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zFree + 3))).Value = v3;
        v2 = v3;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zFree + 2))).Value = v2;
        v1 = v2;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zFree + 1))).Value = v1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zFree))).Value = v1;
        zSuperJournal = zFree + 4;
        rc = _sqlite3OsRead(tls, pSuper, zSuperJournal, (int32)(~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp))), 0);
        if (rc != SQLITE_OK) {
            goto delsuper_out;
        }
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zSuperJournal + (uintptr)(~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)))))).Value = 0;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zSuperJournal + (uintptr)(~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) + (int64)1)))).Value = 0;
        zJournal = zSuperJournal;
        while ((int64)zJournal - (int64)zSuperJournal < ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp))) {
            if (libc.Xstrcmp(tls, zJournal, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal) == 0) {
                bSeen = (int32)1;
            } else {
                rc = _sqlite3OsAccess(tls, pVfs, zJournal, SQLITE_ACCESS_EXISTS, bp + 8);
                if (rc != SQLITE_OK) {
                    goto delsuper_out;
                }
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)) != 0) {
                    zSuperPtr = (uintptr)0;
                    flags1 = libc.Int32FromInt32(SQLITE_OPEN_READONLY) | libc.Int32FromInt32(SQLITE_OPEN_SUPER_JOURNAL);
                    rc = _sqlite3OsOpen(tls, pVfs, zJournal, pJournal, flags1, (uintptr)0);
                    if (rc != SQLITE_OK) {
                        goto delsuper_out;
                    }
                    rc = _readSuperJournal(tls, pJournal, (uint64)1 + libc.Uint64FromInt32((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FmxPathname), @unsafe.Pointer.FromBox(ᏑzSuperPtr));
                    _sqlite3OsClose(tls, pJournal);
                    if (rc != SQLITE_OK) {
                        goto delsuper_out;
                    }
                    c = libc.BoolInt32(zSuperPtr != (uintptr)0 && libc.Xstrcmp(tls, zSuperPtr, zSuper) == 0);
                    _freeSuperJournal(tls, zSuperPtr);
                    if (c != 0) {
                        goto delsuper_out;
                    }
                }
            }
            zJournal = zJournal + (uintptr)(_sqlite3Strlen30(tls, zJournal) + libc.Int32FromInt32(1));
        }
        _sqlite3OsClose(tls, pSuper);
        if (bSeen != 0) {
            rc = _sqlite3OsDelete(tls, pVfs, zSuper, 0);
        }
        goto delsuper_out;
delsuper_out:;
        Xsqlite3_free(tls, zFree);
        if (pSuper != 0) {
            _sqlite3OsClose(tls, pSuper);
            Xsqlite3_free(tls, pSuper);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _propagateConstantExprRewrite(ж<libc.TLS> tls, uintptr pWalker, uintptr pExpr) {
    uintptr pConst = default;
    _ = pConst;
    pConst = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
    if ((@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FbHasAffBlob != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) >= (int32)TK_EQ && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) <= (int32)TK_GE || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_IS) {
            _propagateConstantExprRewriteOne(tls, pConst, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, 0);
            if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FpOomFault)) != 0) {
                return (int32)WRC_Prune;
            }
            if ((int32)_sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft) != (int32)SQLITE_AFF_TEXT) {
                _propagateConstantExprRewriteOne(tls, pConst, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, 0);
            }
        }
    }
    return _propagateConstantExprRewriteOne(tls, pConst, pExpr, (@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FbHasAffBlob);
}

internal static int32 /*r*/ _propagateConstantExprRewriteOne(ж<libc.TLS> tls, uintptr pConst, uintptr pExpr, int32 bIgnoreAffBlob) {
    int32 i = default;
    uintptr pColumn = default;
    _ = i;
    _ = pColumn;
    if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FpOomFault)) != 0) {
        return (int32)WRC_Prune;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_COLUMN) {
        return WRC_Continue;
    }
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & (libc.Uint32FromInt32(EP_FixedCol) | (@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FmExcludeOn)) != (uint32)0) {
        return WRC_Continue;
    }
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FnConst)) {
            break;
        }
        pColumn = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FapExpr + (uintptr)(i * (int32)2) * 8));
        if (pColumn == pExpr) {
            goto _1;
        }
        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColumn)).Value.FiTable != (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable) {
            goto _1;
        }
        if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pColumn)).Value.FiColumn != (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn) {
            goto _1;
        }
        if (bIgnoreAffBlob != 0 && (int32)_sqlite3ExprAffinity(tls, pColumn) <= (int32)SQLITE_AFF_BLOB) {
            break;
        }
        (@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FnChng = (@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FnChng + 1;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pExpr + 4))).Value &= ~libc.Uint32FromInt32(libc.Int32FromInt32(EP_Leaf));
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pExpr + 4))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(EP_FixedCol));
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft = _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FpParse)).Value.Fdb, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FapExpr + (uintptr)(i * (int32)2 + (int32)1) * 8)), 0);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereConst>((@unsafe.Pointer)pConst)).Value.FpParse)).Value.Fdb)).Value.FmallocFailed != 0) {
            return (int32)WRC_Prune;
        }
        break;
        goto _1;
_1:;
        i = i + 1;
    }
    return (int32)WRC_Prune;
}

internal static int32 /*r*/ _readSuperJournal(ж<libc.TLS> tls, uintptr pJrnl, Tu64 nSuper, @unsafe.Pointer pzSuper) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var cksum = ref heap(new Tu32(), out var Ꮡcksum);
        ref var len1 = ref heap(new Tu32(), out var Ꮡlen1);
        Tu32 u = default;
        int32 rc = default;
        int32 v1 = default;
        int32 v2 = default;
        int32 v4 = default;
        int32 v6 = default;
        uintptr zOut = default;
        bool v3 = default;
        bool v5 = default;
        bool v7 = default;
        array<uint8> _ᴛ1 = new(8);
        Ti64 _ᴛ2 = default;
        _ = cksum;
        _ = len1;
        _ = rc;
        _ = u;
        _ = zOut;
        _ = v1;
        _ = v2;
        _ = v3;
        _ = v4;
        _ = v5;
        _ = v6;
        _ = v7;
        zOut = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(pzSuper)).Value = (uintptr)0;
        v1 = _sqlite3OsFileSize(tls, pJrnl, bp);
        rc = v1;
        v3 = SQLITE_OK != v1 || ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) < (int64)16; if (!v3) {
            v2 = _read32bits(tls, pJrnl, ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) - (int64)16, @unsafe.Pointer.FromPinnedBox(Ꮡlen1));
            rc = v2;
        }
        v5 = v3 || SQLITE_OK != v2 || (uint64)len1 >= nSuper || libc.Int64FromUint32(len1) > ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) - (int64)16 || len1 == (uint32)0; if (!v5) {
            v4 = _read32bits(tls, pJrnl, ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) - (int64)12, @unsafe.Pointer.FromPinnedBox(Ꮡcksum));
            rc = v4;
        }
        v7 = v5 || SQLITE_OK != v4; if (!v7) {
            v6 = _sqlite3OsRead(tls, pJrnl, bp + 8, (int32)8, ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) - (int64)8);
            rc = v6;
        }
        if (v7 || SQLITE_OK != v6) {
            r = rc; goto ᒐdone;
        }
        zOut = _sqlite3MallocZero(tls, (uint64)((uint32)4 + len1 + (uint32)2));
        if (!(zOut != 0)) {
            if (libc.Xmemcmp(tls, bp + 8, (uintptr)Ꮡ_aJournalMagic, (uint64)8) != 0) {
                v1 = SQLITE_OK;
            } else {
                v1 = (int32)SQLITE_NOMEM;
            }
            rc = v1;
        } else {
            zOut = zOut + 4;
            v1 = _sqlite3OsRead(tls, pJrnl, zOut, libc.Int32FromUint32(len1), ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) - (int64)16 - libc.Int64FromUint32(len1));
            rc = v1;
            if (SQLITE_OK == v1) {
                u = (uint32)0;
                for (;;) {
                    if (!(u < len1)) {
                        break;
                    }
                    cksum = cksum - libc.Uint32FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zOut + (uintptr)u)));
                    goto _10;
_10:;
                    u = u + 1;
                }
            }
            if (rc != SQLITE_OK || !(_pagerIsSuperJrnlName(tls, zOut) != 0) || cksum != 0 || libc.Xmemcmp(tls, bp + 8, (uintptr)Ꮡ_aJournalMagic, (uint64)8) != 0) {
                _freeSuperJournal(tls, zOut);
                zOut = (uintptr)0;
            }
        }
        (@unsafe.Pointer.To<uintptr>(pzSuper)).Value = zOut;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _resolveOrderGroupBy(ж<libc.TLS> tls, uintptr pNC, uintptr pSelect, uintptr pOrderBy, uintptr zType) {
    int32 i = default;
    ref var iCol = ref heap(new int32(), out var ᏑiCol);
    int32 j = default;
    int32 nResult = default;
    uintptr pE = default;
    uintptr pE2 = default;
    uintptr pItem = default;
    uintptr pParse = default;
    _ = i;
    _ = iCol;
    _ = j;
    _ = nResult;
    _ = pE;
    _ = pE2;
    _ = pItem;
    _ = pParse;
    nResult = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList)).Value.FnExpr;
    pParse = (@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FpParse;
    i = 0;
    pItem = pOrderBy + 8;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr)) {
            break;
        }
        pE = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr;
        pE2 = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), pE);
        if (pE2 == (uintptr)0) {
            goto _1;
        }
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zType))) != (int32)'G') {
            iCol = _resolveAsName(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList, pE2);
            if (iCol > 0) {
                (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem + 24))).Value.FiOrderByCol = libc.Uint16FromInt32(iCol);
                goto _1;
            }
        }
        if (_sqlite3ExprIsInteger(tls, pE2, @unsafe.Pointer.FromPinnedBox(ᏑiCol), (uintptr)0, 0) != 0) {
            if (iCol < (int32)1 || iCol > (int32)0xffff) {
                _resolveOutOfRangeError(tls, pParse, zType, i + (int32)1, nResult, pE2);
                return (int32)1;
            }
            (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem + 24))).Value.FiOrderByCol = libc.Uint16FromInt32(iCol);
            goto _1;
        }
        (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem + 24))).Value.FiOrderByCol = (uint16)0;
        if (_sqlite3ResolveExprNames(tls, pNC, pE) != 0) {
            return (int32)1;
        }
        j = 0;
        for (;;) {
            if (!(j < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList)).Value.FnExpr)) {
                break;
            }
            if (_sqlite3ExprCompare(tls, (uintptr)0, pE, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList + 8 + (uintptr)j * 32))).Value.FpExpr, -(int32)1) == 0) {
                _windowRemoveExprFromSelect(tls, pSelect, pE);
                (@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem + 24))).Value.FiOrderByCol = libc.Uint16FromInt32(j + (int32)1);
            }
            goto _2;
_2:;
            j = j + 1;
        }
        goto _1;
_1:;
        i = i + 1;
        pItem += 32;
    }
    return _sqlite3ResolveOrderGroupBy(tls, pParse, pSelect, pOrderBy, zType);
}

internal static void _sqlite3AddCheckConstraint(ж<libc.TLS> tls, uintptr pParse, uintptr pCheckExpr, uintptr zStart, uintptr zEnd) {
    uintptr db = default;
    uintptr pTab = default;
    ref var t = ref heap(new TToken(), out var Ꮡt);
    _ = db;
    _ = pTab;
    _ = t;
    pTab = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTable;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (pTab != 0 && !(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) == libc.Int32FromInt32(PARSE_MODE_DECLARE_VTAB)) && !(_sqlite3BtreeIsReadonly(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb * 32))).Value.FpBt) != 0)) {
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpCheck = _sqlite3ExprListAppend(tls, pParse, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpCheck, pCheckExpr);
        if ((@unsafe.Pointer.To<_sqlite3AddColumn_type>((@unsafe.Pointer)(pParse + 240))).Value.FconstraintName.Fn != 0) {
            _sqlite3ExprListSetName(tls, pParse, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpCheck, (@unsafe.Pointer)(pParse + 240 + 16), (int32)1);
        } else {
            zStart = zStart + 1;
            for (;;) {
                if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStart)))]) & (int32)0x01) != 0)) {
                    break;
                }
                goto _1;
_1:;
                zStart = zStart + 1;
            }
            while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zEnd + (uintptr)(-libc.Int32FromInt32(1)))))]) & (int32)0x01) != 0) {
                zEnd = zEnd - 1;
            }
            t.Fz = zStart;
            t.Fn = libc.Uint32FromInt32((int32)((int64)zEnd - (int64)t.Fz));
            _sqlite3ExprListSetName(tls, pParse, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpCheck, @unsafe.Pointer.FromPinnedBox(Ꮡt), (int32)1);
        }
    } else {
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pCheckExpr);
    }
}

internal static uintptr /*r*/ _sqlite3DbSpanDup(ж<libc.TLS> tls, uintptr db, uintptr zStart, uintptr zEnd) {
    int32 n = default;
    _ = n;
    while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStart)))]) & (int32)0x01) != 0) {
        zStart = zStart + 1;
    }
    n = (int32)((int64)zEnd - (int64)zStart);
    while ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zStart + (uintptr)(n - (int32)1))))]) & (int32)0x01) != 0) {
        n = n - 1;
    }
    return _sqlite3DbStrNDup(tls, db, zStart, libc.Uint64FromInt32(n));
}

internal static void _sqlite3DequoteToken(ref libc.TLS tls, @unsafe.Pointer p) {
    uint32 i = default;
    _ = i;
    if (~@unsafe.Pointer.To<uint32>(@unsafe.Add(p, 8)) < (uint32)2) {
        return;
    }
    if (!((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(p))))]) & libc.Int32FromInt32(0x80)) != 0)) {
        return;
    }
    i = (uint32)1;
    for (;;) {
        if (!(i < ~@unsafe.Pointer.To<uint32>(@unsafe.Add(p, 8)) - (uint32)1)) {
            break;
        }
        if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(p) + (uintptr)i)))]) & (int32)0x80) != 0) {
            return;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<uint32>(@unsafe.Add(p, 8))).Value -= (uint32)2;
    (@unsafe.Pointer.To<uintptr>(p)).Value = ~@unsafe.Pointer.To<uintptr>(p) + 1;
}

internal static int8 /*r*/ _sqlite3ExprAffinity(ж<libc.TLS> tls, uintptr pExpr) {
    int32 op = default;
    _ = op;
    op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop);
    while ((int32)1 != 0) {
        if (op == (int32)TK_COLUMN || op == (int32)TK_AGG_COLUMN && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64)) != (uintptr)0) {
            return _sqlite3TableColumnAffinity(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64)), (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn);
        }
        if (op == (int32)TK_SELECT) {
            return _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32))))).Value.FpEList + 8))).Value.FpExpr);
        }
        if (op == (int32)TK_CAST) {
            return _sqlite3AffinityType(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)), (uintptr)0);
        }
        if (op == (int32)TK_SELECT_COLUMN) {
            return _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft + 32))))).Value.FpEList + 8 + (uintptr)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn * 32))).Value.FpExpr);
        }
        if (op == (int32)TK_VECTOR || op == (int32)TK_FUNCTION && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FaffExpr == (int32)SQLITE_AFF_DEFER) {
            return _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) + 8))).Value.FpExpr);
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Skip) | libc.Int32FromInt32(EP_IfNullRow))) != (uint32)0) {
            pExpr = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
            op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop);
            continue;
        }
        if (op != (int32)TK_REGISTER) {
            break;
        }
        op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop2);
        if (op == (int32)TK_REGISTER) {
            break;
        }
    }
    return (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FaffExpr;
}

internal static void _sqlite3ExprCodeGeneratedColumn(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, uintptr pCol, int32 regOut) {
    int32 iAddr = default;
    int32 nErr = default;
    int32 p3 = default;
    uintptr v = default;
    _ = iAddr;
    _ = nErr;
    _ = p3;
    _ = v;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    nErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab > 0) {
        iAddr = _sqlite3VdbeAddOp3(tls, v, (int32)OP_IfNullRow, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab - (int32)1, 0, regOut);
    } else {
        iAddr = 0;
    }
    _sqlite3ExprCodeCopy(tls, pParse, _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pTab, pCol), regOut);
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) != 0 && (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Strict) != (uint32)0) {
        p3 = (int32)2 + (int32)(((int64)pCol - (int64)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol) / 16);
        _sqlite3VdbeAddOp4(tls, v, (int32)OP_TypeCheck, regOut, (int32)1, p3, pTab, -(int32)5);
    } else {
        if ((int32)(@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.Faffinity >= (int32)SQLITE_AFF_TEXT) {
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_Affinity, regOut, (int32)1, 0, pCol + 9, (int32)1);
        }
    }
    if (iAddr != 0) {
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, iAddr);
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr > nErr) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FerrByteOffset = -(int32)1;
    }
}

internal static void _sqlite3ExprCodeIN(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 destIfFalse, int32 destIfNull) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 addrTop = default;
        int32 addrTruthOp = default;
        int32 destNotNull = default;
        int32 destStep2 = default;
        int32 destStep6 = default;
        int32 eType = default;
        int32 i = default;
        ref var iDummy = ref heap(new int32(), out var ᏑiDummy);
        ref var iTab = ref heap(new int32(), out var ᏑiTab);
        int32 ii = default;
        int32 labelOk = default;
        int32 nVector = default;
        int32 op = default;
        int32 op1 = default;
        int32 r2 = default;
        int32 r3 = default;
        int32 rLhs = default;
        int32 rLhsOrig = default;
        int32 rTmp = default;
        int32 regCkNull = default;
        ref var regToFree = ref heap(new int32(), out var ᏑregToFree);
        int32 v3 = default;
        int8 aff = default;
        uintptr aiMap = default;
        uintptr p = default;
        uintptr p1 = default;
        uintptr pColl = default;
        uintptr pColl1 = default;
        uintptr pLeft = default;
        uintptr pList = default;
        uintptr pOp = default;
        uintptr pRhs = default;
        uintptr v = default;
        uintptr zAff = default;
        uintptr v1 = default;
        Tu8 okConstFactor = default;
        int32 _ᴛ1 = default;
        _ = addrTop;
        _ = addrTruthOp;
        _ = aff;
        _ = aiMap;
        _ = destNotNull;
        _ = destStep2;
        _ = destStep6;
        _ = eType;
        _ = i;
        _ = iDummy;
        _ = iTab;
        _ = ii;
        _ = labelOk;
        _ = nVector;
        _ = okConstFactor;
        _ = op;
        _ = op1;
        _ = p;
        _ = p1;
        _ = pColl;
        _ = pColl1;
        _ = pLeft;
        _ = pList;
        _ = pOp;
        _ = pRhs;
        _ = r2;
        _ = r3;
        _ = rLhs;
        _ = rLhsOrig;
        _ = rTmp;
        _ = regCkNull;
        _ = regToFree;
        _ = v;
        _ = zAff;
        _ = v1;
        _ = v3;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
        aiMap = (uintptr)0;
        zAff = (uintptr)0;
        destStep6 = 0;
        iTab = 0;
        okConstFactor = libc.Uint8FromInt32((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x80) >> 7)));
        pLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
        if (_sqlite3ExprCheckIN(tls, pParse, pExpr) != 0) {
            return;
        }
        zAff = _exprINAffinity(tls, pParse, pExpr);
        nVector = _sqlite3ExprVectorSize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft);
        aiMap = _sqlite3DbMallocZero(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)(libc.Uint64FromInt32(nVector) * (uint64)4));
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0) {
            goto sqlite3ExprCodeIN_oom_error;
        }
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        if (destIfFalse == destIfNull) {
            v1 = (uintptr)0;
        } else {
            v1 = bp;
        }
        eType = _sqlite3FindInIndex(tls, pParse, pExpr, libc.Uint32FromInt32(libc.Int32FromInt32(IN_INDEX_MEMBERSHIP) | libc.Int32FromInt32(IN_INDEX_NOOP_OK)), v1, aiMap, @unsafe.Pointer.FromPinnedBox(ᏑiTab));
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(0), 7, 0x80);
        rLhs = _exprCodeVector(tls, pParse, pLeft, @unsafe.Pointer.FromPinnedBox(ᏑiDummy));
        libc.SetBitFieldPtr16Uint32(pParse + 40, (uint32)okConstFactor, 7, 0x80);
        if (eType == (int32)IN_INDEX_NOOP) {
            labelOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            regCkNull = 0;
            pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32));
            pColl = _sqlite3ExprCollSeq(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft);
            if (destIfNull != destIfFalse) {
                regCkNull = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_BitAnd, rLhs, rLhs, regCkNull);
            }
            ii = 0;
            for (;;) {
                if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                    break;
                }
                r2 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ii * 32))).Value.FpExpr, @unsafe.Pointer.FromPinnedBox(ᏑregToFree));
                if (regCkNull != 0 && _sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ii * 32))).Value.FpExpr) != 0) {
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_BitAnd, regCkNull, r2, regCkNull);
                }
                _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regToFree);
                if (ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr - (int32)1 || destIfNull != destIfFalse) {
                    if (rLhs != r2) {
                        v3 = (int32)OP_Eq;
                    } else {
                        v3 = (int32)OP_NotNull;
                    }
                    op = v3;
                    _sqlite3VdbeAddOp4(tls, v, op, rLhs, labelOk, r2, pColl, -(int32)2);
                    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff))));
                } else {
                    if (rLhs != r2) {
                        v3 = (int32)OP_Ne;
                    } else {
                        v3 = (int32)OP_IsNull;
                    }
                    op1 = v3;
                    _sqlite3VdbeAddOp4(tls, v, op1, rLhs, destIfFalse, r2, pColl, -(int32)2);
                    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff))) | (int32)SQLITE_JUMPIFNULL));
                }
                goto _2;
_2:;
                ii = ii + 1;
            }
            if (regCkNull != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, regCkNull, destIfNull);
                _sqlite3VdbeGoto(tls, v, destIfFalse);
            }
            _sqlite3VdbeResolveLabel(tls, v, labelOk);
            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regCkNull);
            goto sqlite3ExprCodeIN_finished;
        }
        if (eType != (int32)IN_INDEX_ROWID) {
            if (nVector == (int32)1) {
                aff = ~@unsafe.Pointer.To<int8>(__ccgo_ptr(zAff));
                if ((int32)aff >= (int32)SQLITE_AFF_TEXT && (int32)aff != (int32)_sqlite3ExprAffinity(tls, pLeft)) {
                    rTmp = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, rLhs, rTmp, 0);
                    rLhs = rTmp;
                    _sqlite3VdbeAddOp4(tls, v, (int32)OP_Affinity, rLhs, (int32)1, 0, zAff, (int32)1);
                }
            } else {
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_Affinity, rLhs, nVector, 0, zAff, nVector);
            }
            i = 0;
            for (;;) {
                if (!(i < nVector && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiMap + (uintptr)i * 4)) == i)) {
                    break;
                }
                goto _5;
_5:;
                i = i + 1;
            }
            if (i != nVector) {
                rLhsOrig = rLhs;
                rLhs = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nVector);
                i = 0;
                for (;;) {
                    if (!(i < nVector)) {
                        break;
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, rLhsOrig + i, rLhs + ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiMap + (uintptr)i * 4)), 0);
                    goto _6;
_6:;
                    i = i + 1;
                }
            }
        }
        if (destIfNull == destIfFalse) {
            destStep2 = destIfFalse;
        } else {
            v3 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            destStep6 = v3;
            destStep2 = v3;
        }
        i = 0;
        for (;;) {
            if (!(i < nVector)) {
                break;
            }
            p = _sqlite3VectorFieldSubexpr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, i);
            if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
                goto sqlite3ExprCodeIN_oom_error;
            }
            if (_sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), p) != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, rLhs + ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiMap + (uintptr)i * 4)), destStep2);
            }
            goto _8;
_8:;
            i = i + 1;
        }
        if (eType == (int32)IN_INDEX_ROWID) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_SeekRowid, iTab, destIfFalse, rLhs);
            addrTruthOp = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
        } else {
            if (destIfFalse == destIfNull) {
                if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subrtn))) != (uint32)0) {
                    pOp = _sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<_sqlite3CodeRhsOfIN_type>((@unsafe.Pointer)(pExpr + 64))).Value.FiAddr);
                    if ((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 > 0) {
                        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Filter, (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3, destIfFalse, rLhs, nVector);
                    }
                }
                _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_NotFound, iTab, destIfFalse, rLhs, nVector);
                goto sqlite3ExprCodeIN_finished;
            }
            addrTruthOp = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Found, iTab, 0, rLhs, nVector);
        }
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0 && nVector == (int32)1) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_NotNull, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)), destIfFalse);
        }
        if (destIfFalse == destIfNull) {
            _sqlite3VdbeGoto(tls, v, destIfFalse);
        }
        if (destStep6 != 0) {
            _sqlite3VdbeResolveLabel(tls, v, destStep6);
        }
        if (eType == (int32)IN_INDEX_INDEX_DESC) {
            addrTop = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Last, iTab, destIfFalse);
        } else {
            addrTop = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iTab, destIfFalse);
        }
        if (nVector > (int32)1) {
            destNotNull = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        } else {
            destNotNull = destIfFalse;
        }
        i = 0;
        for (;;) {
            if (!(i < nVector)) {
                break;
            }
            r3 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
            p1 = _sqlite3VectorFieldSubexpr(ref (tls).DerefOrNull(), pLeft, i);
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
                pRhs = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32))))).Value.FpEList + 8 + (uintptr)i * 32))).Value.FpExpr;
                pColl1 = _sqlite3BinaryCompareCollSeq(tls, pParse, p1, pRhs);
            } else {
                pColl1 = _sqlite3ExprCollSeq(tls, pParse, p1);
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, iTab, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiMap + (uintptr)i * 4)), r3);
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_Ne, rLhs + ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiMap + (uintptr)i * 4)), destNotNull, r3, pColl1, -(int32)2);
            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, r3);
            goto _9;
_9:;
            i = i + 1;
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, destIfNull);
        if (nVector > (int32)1) {
            _sqlite3VdbeResolveLabel(tls, v, destNotNull);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iTab, addrTop + (int32)1);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, destIfFalse);
        }
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrTruthOp);
        goto sqlite3ExprCodeIN_finished;
sqlite3ExprCodeIN_finished:;
        goto sqlite3ExprCodeIN_oom_error;
sqlite3ExprCodeIN_oom_error:;
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, aiMap);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, zAff);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _sqlite3ExprCollSeq(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    uintptr db = default;
    uintptr p = default;
    uintptr pColl = default;
    uintptr pNext = default;
    uintptr zColl = default;
    int32 i = default;
    int32 j = default;
    int32 op = default;
    int32 v1 = default;
    _ = db;
    _ = i;
    _ = j;
    _ = op;
    _ = p;
    _ = pColl;
    _ = pNext;
    _ = zColl;
    _ = v1;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pColl = (uintptr)0;
    p = pExpr;
    while (p != 0) {
        op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop);
        if (op == (int32)TK_REGISTER) {
            op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop2);
        }
        if (op == (int32)TK_AGG_COLUMN && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64)) != (uintptr)0 || op == (int32)TK_COLUMN || op == (int32)TK_TRIGGER) {
            v1 = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn;
            j = v1;
            if (v1 >= 0) {
                zColl = _sqlite3ColumnColl(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64))))).Value.FaCol + (uintptr)j * 16);
                pColl = _sqlite3FindCollSeq(tls, db, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc, zColl, 0);
            }
            break;
        }
        if (op == (int32)TK_CAST || op == (int32)TK_UPLUS) {
            p = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
            continue;
        }
        if (op == (int32)TK_VECTOR || op == (int32)TK_FUNCTION && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FaffExpr == (int32)SQLITE_AFF_DEFER) {
            p = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)) + 8))).Value.FpExpr;
            continue;
        }
        if (op == (int32)TK_COLLATE) {
            pColl = _sqlite3GetCollSeq(tls, pParse, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc, (uintptr)0, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)));
            break;
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & (uint32)EP_Collate) != 0) {
            if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft != 0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft)).Value.Fflags & (uint32)EP_Collate) != (uint32)0) {
                p = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
            } else {
                pNext = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight;
                if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & (uint32)EP_xIsSelect) == (uint32)0 && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)) != (uintptr)0 && !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                    i = 0;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32))))).Value.FnExpr)) {
                            break;
                        }
                        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)) + 8 + (uintptr)i * 32))).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Collate))) != (uint32)0) {
                            pNext = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)) + 8 + (uintptr)i * 32))).Value.FpExpr;
                            break;
                        }
                        goto _2;
_2:;
                        i = i + 1;
                    }
                }
                p = pNext;
            }
        } else {
            break;
        }
    }
    if (_sqlite3CheckCollSeq(tls, pParse, pColl) != 0) {
        pColl = (uintptr)0;
    }
    return pColl;
}

internal static int32 /*r*/ _sqlite3ExprDataType(ж<libc.TLS> tls, uintptr pExpr) {
    int32 aff = default;
    int32 ii = default;
    int32 res = default;
    uintptr pList = default;
    _ = aff;
    _ = ii;
    _ = pList;
    _ = res;
    while (pExpr != 0) {
        var exprᴛ1 = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop);
        var matchᴛ1 = false;
        if (exprᴛ1 == (int32)TK_COLLATE) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_IF_NULL_ROW) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_UPLUS) { matchᴛ1 = true;
            pExpr = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
        }
        else if (exprᴛ1 == (int32)TK_NULL) { matchᴛ1 = true;
            pExpr = (uintptr)0;
        }
        else if (exprᴛ1 == (int32)TK_STRING) { matchᴛ1 = true;
            return (int32)0x02;
        }
        else if (exprᴛ1 == (int32)TK_BLOB) { matchᴛ1 = true;
            return (int32)0x04;
        }
        else if (exprᴛ1 == (int32)TK_CONCAT) { matchᴛ1 = true;
            return (int32)0x06;
        }
        else if (exprᴛ1 == (int32)TK_VARIABLE) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_AGG_FUNCTION) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_FUNCTION) { matchᴛ1 = true;
            return (int32)0x07;
        }
        else if (exprᴛ1 == (int32)TK_COLUMN) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_AGG_COLUMN) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_SELECT) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_CAST) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_SELECT_COLUMN) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_VECTOR) { matchᴛ1 = true;
            aff = (int32)_sqlite3ExprAffinity(tls, pExpr);
            if (aff >= (int32)SQLITE_AFF_NUMERIC) {
                return (int32)0x05;
            }
            if (aff == (int32)SQLITE_AFF_TEXT) {
                return (int32)0x06;
            }
            return (int32)0x07;
        }
        else if (exprᴛ1 == (int32)TK_CASE) {
            res = 0;
            pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32));
            ii = (int32)1;
            for (;;) {
                if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                    break;
                }
                res = res | _sqlite3ExprDataType(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)ii * 32))).Value.FpExpr);
                goto _1;
_1:;
                ii = ii + (int32)2;
            }
            if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr % (int32)2 != 0) {
                res = res | _sqlite3ExprDataType(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr - (int32)1) * 32))).Value.FpExpr);
            }
            return res;
        }
        else if (!matchᴛ1) { /* default: */
            return (int32)0x01;
        }

    }
    return 0x00;
}

internal static int32 /*r*/ _sqlite3ExprIsIIF(ж<libc.TLS> tls, uintptr db, uintptr pExpr) {
    uintptr pDef = default;
    uintptr pList = default;
    uintptr z = default;
    _ = pDef;
    _ = pList;
    _ = z;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_FUNCTION) {
        z = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8));
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)'i' && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z))) != (int32)'I') {
            return 0;
        }
        if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) == (uintptr)0) {
            return 0;
        }
        pDef = _sqlite3FindFunction(tls, db, z, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32))))).Value.FnExpr, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc, (uint8)0);
        if (pDef == (uintptr)0) {
            return 0;
        }
        if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FfuncFlags & (uint32)SQLITE_FUNC_INLINE) == (uint32)0) {
            return 0;
        }
        if ((int32)(int64)(@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FpUserData != (int32)INLINEFUNC_iif) {
            return 0;
        }
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_CASE) {
            if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft != (uintptr)0) {
                return 0;
            }
        } else {
            return 0;
        }
    }
    pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32));
    if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr == (int32)2) {
        return (int32)1;
    }
    if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr == (int32)3 && _sqlite3ExprIsNotTrue(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + 2 * 32))).Value.FpExpr) != 0) {
        return (int32)1;
    }
    return 0;
}

internal static int32 /*r*/ _sqlite3Fts5ExprPattern(ж<libc.TLS> tls, uintptr pConfig, int32 bGlob, int32 iCol, uintptr zText, @unsafe.Pointer pp) {
    array<int8> aSpec = new(3);
    int32 bAnd = default;
    int32 i = default;
    int32 iFirst = default;
    int32 iOut = default;
    int32 jj = default;
    int32 rc = default;
    int32 v1 = default;
    Ti64 nText = default;
    uintptr zExpr = default;
    _ = aSpec;
    _ = bAnd;
    _ = i;
    _ = iFirst;
    _ = iOut;
    _ = jj;
    _ = nText;
    _ = rc;
    _ = zExpr;
    _ = v1;
    nText = libc.Int64FromUint64(libc.Xstrlen(tls, zText));
    zExpr = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nText * (int64)4 + (int64)1));
    rc = SQLITE_OK;
    if (zExpr == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        iOut = 0;
        i = 0;
        iFirst = 0;
        if (bGlob == 0) {
            aSpec[0] = (int8)'_';
            aSpec[(int32)1] = (int8)'%';
            aSpec[(int32)2] = 0;
        } else {
            aSpec[0] = (int8)'*';
            aSpec[(int32)1] = (int8)'?';
            aSpec[(int32)2] = (int8)'[';
        }
        while ((int64)i <= nText) {
            if ((int64)i == nText || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)i))) == (int32)aSpec[0] || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)i))) == (int32)aSpec[(int32)1] || (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)i))) == (int32)aSpec[(int32)2]) {
                if (_fts5ExprCountChar(ref (tls).DerefOrNull(), zText + (uintptr)iFirst, i - iFirst) >= (int32)3) {
                    v1 = iOut;
                    iOut = iOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zExpr + (uintptr)v1))).Value = (int8)'"';
                    jj = iFirst;
                    for (;;) {
                        if (!(jj < i)) {
                            break;
                        }
                        v1 = iOut;
                        iOut = iOut + 1;
                        (@unsafe.Pointer.To<int8>(__ccgo_ptr(zExpr + (uintptr)v1))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)jj))).Value;
                        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)jj))) == (int32)'"') {
                            v1 = iOut;
                            iOut = iOut + 1;
                            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zExpr + (uintptr)v1))).Value = (int8)'"';
                        }
                        goto _2;
_2:;
                        jj = jj + 1;
                    }
                    v1 = iOut;
                    iOut = iOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zExpr + (uintptr)v1))).Value = (int8)'"';
                    v1 = iOut;
                    iOut = iOut + 1;
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zExpr + (uintptr)v1))).Value = (int8)' ';
                }
                if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)i))) == (int32)aSpec[(int32)2]) {
                    i = i + (int32)2;
                    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)(i - (int32)1)))) == (int32)'^') {
                        i = i + 1;
                    }
                    while ((int64)i < nText && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zText + (uintptr)i))) != (int32)']') {
                        i = i + 1;
                    }
                }
                iFirst = i + (int32)1;
            }
            i = i + 1;
        }
        if (iOut > 0) {
            bAnd = 0;
            if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail != FTS5_DETAIL_FULL) {
                bAnd = (int32)1;
                if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail == (int32)FTS5_DETAIL_NONE) {
                    iCol = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol;
                }
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zExpr + (uintptr)iOut))).Value = (int8)'\u0000';
            rc = _sqlite3Fts5ExprNew(tls, pConfig, bAnd, iCol, zExpr, pp, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FpzErrmsg);
        } else {
            (@unsafe.Pointer.To<uintptr>(pp)).Value = (uintptr)0;
        }
        Xsqlite3_free(tls, zExpr);
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3IsShadowTableOf(ж<libc.TLS> tls, uintptr db, uintptr pTab, uintptr zName) {
    int32 nName = default;
    uintptr pMod = default;
    _ = nName;
    _ = pMod;
    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB))) {
        return 0;
    }
    nName = _sqlite3Strlen30(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
    if (Xsqlite3_strnicmp(tls, zName, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, nName) != 0) {
        return 0;
    }
    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + (uintptr)nName))) != (int32)'_') {
        return 0;
    }
    pMod = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)(db + 584), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(pTab + 64))).Value.FazArg)));
    if (pMod == (uintptr)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule)).Value.FiVersion < (int32)3) {
        return 0;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule)).Value.FxShadowName == (uintptr)0) {
        return 0;
    }
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule)).Value.FxShadowName)(tls, zName + (uintptr)nName + (uintptr)1);
}

internal static void _sqlite3MarkAllShadowTablesOf(ж<libc.TLS> tls, uintptr db, uintptr pTab) {
    uintptr k = default;
    uintptr pMod = default;
    uintptr pOther = default;
    int32 nName = default;
    _ = k;
    _ = nName;
    _ = pMod;
    _ = pOther;
    pMod = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)(db + 584), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TTableᴛ1_Fu_Fvtab>((@unsafe.Pointer)(pTab + 64))).Value.FazArg)));
    if (pMod == (uintptr)0) {
        return;
    }
    if ((@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule == (uintptr)0) {
        return;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule)).Value.FiVersion < (int32)3) {
        return;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule)).Value.FxShadowName == (uintptr)0) {
        return;
    }
    nName = _sqlite3Strlen30(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
    k = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema + 8))).Value.Ffirst;
    for (;;) {
        if (!(k != 0)) {
            break;
        }
        pOther = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)k)).Value.Fdata;
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pOther)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_NORM))) {
            goto _1;
        }
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pOther)).Value.FtabFlags & (uint32)TF_Shadow) != 0) {
            goto _1;
        }
        if (Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pOther)).Value.FzName, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, nName) == 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pOther)).Value.FzName + (uintptr)nName))) == (int32)'_' && GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)(@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule)).Value.FxShadowName)(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pOther)).Value.FzName + (uintptr)nName + (uintptr)1) != 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pOther + 48))).Value |= (uint32)TF_Shadow;
        }
        goto _1;
_1:;
        k = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)k)).Value.Fnext;
    }
}

internal static int32 /*r*/ _sqlite3MemRealValueRCSlowPath(ж<libc.TLS> tls, @unsafe.Pointer pMem, @unsafe.Pointer pValue) {
    int32 r = default;

    int32 i = default;
    int32 j = default;
    int32 n = default;
    int32 rc = default;
    int32 v2 = default;
    uintptr z = default;
    uintptr zCopy = default;
    uintptr zCopy1 = default;
    _ = i;
    _ = j;
    _ = n;
    _ = rc;
    _ = z;
    _ = zCopy;
    _ = zCopy1;
    _ = v2;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<float64>(pValue)).Value = (float64)0D;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pMem, 22))) == (int32)SQLITE_UTF8) {
        zCopy = _sqlite3DbStrNDup(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 24)), ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 8)), libc.Uint64FromInt32(~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 16))));
        if (zCopy != 0) {
            rc = _sqlite3AtoF(tls, zCopy, pValue);
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 24))), zCopy);
        }
        return rc;
    } else {
        n = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 16)) & ~libc.Int32FromInt32(1);
        zCopy1 = _sqlite3DbMallocRaw(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 24)), libc.Uint64FromInt32(n / (int32)2 + (int32)2));
        if (zCopy1 != 0) {
            z = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 8));
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pMem, 22))) == (int32)SQLITE_UTF16LE) {
                v2 = libc.Int32FromInt32(0);
                j = v2;
                i = v2;
                for (;;) {
                    if (!(i < n - (int32)1)) {
                        break;
                    }
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zCopy1 + (uintptr)j))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))).Value;
                    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (int32)1)))) != 0) {
                        break;
                    }
                    goto _1;
_1:;
                    i = i + (int32)2;
                    j = j + 1;
                }
            } else {
                v2 = libc.Int32FromInt32(0);
                j = v2;
                i = v2;
                for (;;) {
                    if (!(i < n - (int32)1)) {
                        break;
                    }
                    if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)i))) != 0) {
                        break;
                    }
                    (@unsafe.Pointer.To<int8>(__ccgo_ptr(zCopy1 + (uintptr)j))).Value = (@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)(i + (int32)1)))).Value;
                    goto _3;
_3:;
                    i = i + (int32)2;
                    j = j + 1;
                }
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zCopy1 + (uintptr)j))).Value = 0;
            rc = _sqlite3AtoF(tls, zCopy1, pValue);
            if (i < n) {
                rc = -(int32)100;
            }
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 24))), zCopy1);
        }
        return rc;
    }
    return r;
}

internal static uintptr /*r*/ _sqlite3StrAccumFinish(ж<libc.TLS> tls, @unsafe.Pointer p) {
    if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)) != 0) {
        (@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8)) + (uintptr)(~@unsafe.Pointer.To<Tu32>(@unsafe.Add(p, 24)))))).Value = 0;
        if (~@unsafe.Pointer.To<Tu32>(@unsafe.Add(p, 20)) > (uint32)0 && !((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(p, 29))) & libc.Int32FromInt32(SQLITE_PRINTF_MALLOCED)) != libc.Int32FromInt32(0))) {
            return _strAccumFinishRealloc(tls, p);
        }
    }
    return ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 8));
}

internal static void _sqlite3StrAppendchar64(ж<libc.TLS> tls, uintptr p, Ti64 N, int8 c) {
    Ti64 v1 = default;
    bool v2 = default;
    Tu32 v4 = default;
    uintptr v5 = default;
    _ = v1;
    _ = v2;
    _ = v4;
    _ = v5;
    v2 = libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar) + N >= libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnAlloc); if (v2) {
        v1 = (int64)_sqlite3StrAccumEnlarge(tls, p, N);
        N = v1;
    }
    if (v2 && v1 <= 0) {
        return;
    }
    for (;;) {
        v1 = N;
        N = N - 1;
        if (!(v1 > 0)) {
            break;
        }
        v5 = p + 24;
        v4 = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v5);
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v5)).Value = ~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v5) + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText + (uintptr)v4))).Value = c;
    }
}

internal static uintptr /*r*/ _sqlite3TableAffinityStr(ж<libc.TLS> tls, uintptr db, uintptr pTab) {
    int32 i = default;
    int32 j = default;
    int32 v2 = default;
    int32 v3 = default;
    uintptr zColAff = default;
    _ = i;
    _ = j;
    _ = zColAff;
    _ = v2;
    _ = v3;
    zColAff = _sqlite3DbMallocRaw(tls, db, libc.Uint64FromInt32((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + (int32)1));
    if (zColAff != 0) {
        v2 = libc.Int32FromInt32(0);
        j = v2;
        i = v2;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) == 0) {
                v3 = j;
                j = j + 1;
                (@unsafe.Pointer.To<int8>(__ccgo_ptr(zColAff + (uintptr)v3))).Value = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.Faffinity;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        for (var cond = true; cond; cond = j >= 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zColAff + (uintptr)j))) <= (int32)SQLITE_AFF_BLOB) {
            v2 = j;
            j = j - 1;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(zColAff + (uintptr)v2))).Value = 0;
        }
    }
    return zColAff;
}

internal static int32 /*r*/ _sqlite3VListNameToNum(ж<libc.TLS> tls, uintptr pIn, uintptr zName, int32 nName) {
    int32 i = default;
    int32 mx = default;
    uintptr z = default;
    _ = i;
    _ = mx;
    _ = z;
    if (pIn == (uintptr)0) {
        return 0;
    }
    mx = ~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + 1 * 4));
    i = (int32)2;
    for (var cond = true; cond; cond = i < mx) {
        z = pIn + (uintptr)(i + (int32)2) * 4;
        if (libc.Xstrncmp(tls, z, zName, libc.Uint64FromInt32(nName)) == 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(z + (uintptr)nName))) == 0) {
            return ~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + (uintptr)i * 4));
        }
        i = i + ~@unsafe.Pointer.To<TVList>(__ccgo_ptr(pIn + (uintptr)(i + (int32)1) * 4));
    }
    return 0;
}

internal static uintptr /*r*/ _sqlite3VMPrintf(ж<libc.TLS> tls, uintptr db, uintptr zFormat, Tva_list ap) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(112);
        defer(tls.Free, (nint)112, ref ᒐ);
        uintptr z = default;
        TStrAccum _ᴛ1 = default;
        array<int8> _ᴛ2 = new(70);
        _ = z;
        _sqlite3StrAccumInit(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 72), db, bp, (int32)70, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136)));
        (@unsafe.Pointer.To<TStrAccum>(__ccgo_ptr(bp + 72))).Value.FprintfFlags = (uint8)SQLITE_PRINTF_INTERNAL;
        Xsqlite3_str_vappendf(tls, bp + 72, zFormat, ap);
        z = _sqlite3StrAccumFinish(tls, __ccgo_ptr(bp + 72));
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TStrAccum>(__ccgo_ptr(bp + 72))).Value.FaccError) == (int32)SQLITE_NOMEM) {
            _sqlite3OomFault(tls, db);
        }
        r = z;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeMemZeroTerminateIfAble(ж<libc.TLS> tls, @unsafe.Pointer pMem) {
    @unsafe.Pointer v1 = default;
    _ = v1;
    if ((libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pMem, 20))) & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Term) | libc.Int32FromInt32(MEM_Ephem) | libc.Int32FromInt32(MEM_Static))) != (int32)MEM_Str) {
        return 0;
    }
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pMem, 22))) != (int32)SQLITE_UTF8) {
        return 0;
    }
    if ((libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pMem, 20))) & (int32)MEM_Dyn) != 0) {
        if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 48)) == __ccgo_fp(Xsqlite3_free) && Xsqlite3_msize(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 8))) >= libc.Uint64FromInt32(~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 16)) + libc.Int32FromInt32(1))) {
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 8)) + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 16)))))).Value = 0;
            v1 = @unsafe.Add(pMem, 20);
            (@unsafe.Pointer.To<Tu16>(v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>(v1)) | libc.Int32FromInt32(MEM_Term));
            return (int32)1;
        }
        if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 48)) == __ccgo_fp(_sqlite3RCStrUnref)) {
            v1 = @unsafe.Add(pMem, 20);
            (@unsafe.Pointer.To<Tu16>(v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>(v1)) | libc.Int32FromInt32(MEM_Term));
            return (int32)1;
        }
    } else {
        if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 32)) >= ~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 16)) + (int32)1) {
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pMem, 8)) + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(pMem, 16)))))).Value = 0;
            v1 = @unsafe.Add(pMem, 20);
            (@unsafe.Pointer.To<Tu16>(v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>(v1)) | libc.Int32FromInt32(MEM_Term));
            return (int32)1;
        }
    }
    return 0;
}

internal static void _sqlite3WindowCodeInit(ж<libc.TLS> tls, uintptr pParse, uintptr pSelect) {
    int32 nEphExpr = default;
    int32 nExpr = default;
    int32 v1 = default;
    uintptr p = default;
    uintptr pKeyInfo = default;
    uintptr pList = default;
    uintptr pMWin = default;
    uintptr pWin = default;
    uintptr v = default;
    uintptr v2 = default;
    _ = nEphExpr;
    _ = nExpr;
    _ = p;
    _ = pKeyInfo;
    _ = pList;
    _ = pMWin;
    _ = pWin;
    _ = v;
    _ = v1;
    _ = v2;
    nEphExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8 + 72))))).Value.FpSelect)).Value.FpEList)).Value.FnExpr;
    pMWin = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWin;
    v = _sqlite3GetVdbe(tls, pParse);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr, nEphExpr);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr + (int32)1, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr + (int32)2, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr + (int32)3, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr);
    if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition != 0) {
        nExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition)).Value.FnExpr;
        (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregPart = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nExpr;
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Null, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregPart, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregPart + nExpr - (int32)1);
    }
    v2 = pParse + 60;
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregOne = v1;
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregOne);
    if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeExclude != 0) {
        v2 = pParse + 60;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregStartRowid = v1;
        v2 = pParse + 60;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregEndRowid = v1;
        v2 = pParse + 56;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FcsrApp = v1;
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregStartRowid);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregEndRowid);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FcsrApp, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr);
        return;
    }
    pWin = pMWin;
    for (;;) {
        if (!(pWin != 0)) {
            break;
        }
        p = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpWFunc;
        if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags & (uint32)SQLITE_FUNC_MINMAX) != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FeStart) != (int32)TK_UNBOUNDED) {
            pList = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOwner + 32));
            pKeyInfo = _sqlite3KeyInfoFromExprList(tls, pParse, pList, 0, 0);
            v2 = pParse + 56;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp = v1;
            (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)3;
            if (pKeyInfo != 0 && (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpWFunc)).Value.FzName + 1))) == (int32)'i') {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FaSortFlags))).Value = (uint8)KEYINFO_ORDER_DESC;
            }
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp, (int32)2);
            _sqlite3VdbeAppendP4(tls, v, pKeyInfo, -(int32)9);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp + (int32)1);
        } else {
            if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FzName == (uintptr)Ꮡ_nth_valueName || (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FzName == (uintptr)Ꮡ_first_valueName) {
                (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FregApp = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
                v2 = pParse + 56;
                v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp = v1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)2;
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr);
            } else {
                if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FzName == (uintptr)Ꮡ_leadName || (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FzName == (uintptr)Ꮡ_lagName) {
                    v2 = pParse + 56;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp = v1;
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenDup, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FcsrApp, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr);
                }
            }
        }
        goto _9;
_9:;
        pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
    }
}

internal static int32 /*r*/ _termCanDriveIndex(ж<libc.TLS> tls, uintptr pTerm, uintptr pSrc, TBitmask notReady) {
    int8 aff = default;
    int32 leftCol = default;
    _ = aff;
    _ = leftCol;
    if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FleftCursor != (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor) {
        return 0;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS))) == 0) {
        return 0;
    }
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0 && !(_constraintCompatibleWithOuterJoin(ref (tls).DerefOrNull(), pTerm, pSrc) != 0)) {
        return 0;
    }
    if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight & notReady) != (uint64)0) {
        return 0;
    }
    leftCol = (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn;
    if (leftCol < 0) {
        return 0;
    }
    aff = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab)).Value.FaCol + (uintptr)leftCol * 16))).Value.Faffinity;
    if (!(_sqlite3IndexAffinityOk(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr, aff) != 0)) {
        return 0;
    }
    return _columnIsGoodIndexCandidate(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab, leftCol);
}

internal static int32 /*r*/ _vdbeColumnFromOverflow(ж<libc.TLS> tls, uintptr pC, int32 iCol, Tu32 t, Ti64 iOffset, Tu32 cacheStatus, Tu32 colCacheCtr, uintptr pDest) {
    uintptr db = default;
    uintptr pBuf = default;
    uintptr pCache = default;
    uintptr v1 = default;
    int32 encoding = default;
    int32 len1 = default;
    int32 rc = default;
    _ = db;
    _ = encoding;
    _ = len1;
    _ = pBuf;
    _ = pCache;
    _ = rc;
    _ = v1;
    db = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pDest)).Value.Fdb;
    encoding = libc.Int32FromUint8((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pDest)).Value.Fenc);
    len1 = libc.Int32FromUint32(_sqlite3VdbeSerialTypeLen(ref (tls).DerefOrNull(), t));
    if (len1 > ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136))) {
        return (int32)SQLITE_TOOBIG;
    }
    if (len1 > (int32)4000 && (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pC)).Value.FpKeyInfo == (uintptr)0) {
        if ((int32)((TBool)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pC + 8)) & 0x10) >> 4)) == 0) {
            (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pC)).Value.FpCache = _sqlite3DbMallocZero(tls, db, (uint64)32);
            if ((@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pC)).Value.FpCache == (uintptr)0) {
                return (int32)SQLITE_NOMEM;
            }
            libc.SetBitFieldPtr8Uint32(pC + 8, libc.Uint32FromInt32(1), 4, 0x10);
        }
        pCache = (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pC)).Value.FpCache;
        if ((@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue == (uintptr)0 || (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FiCol != iCol || (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FcacheStatus != cacheStatus || (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FcolCacheCtr != colCacheCtr || (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FiOffset != _sqlite3BtreeOffset(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pC + 48)))) {
            if ((@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue != 0) {
                _sqlite3RCStrUnref(tls, (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue);
            }
            v1 = _sqlite3RCStrNew(tls, libc.Uint64FromInt32(len1 + (int32)3));
            (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue = v1;
            pBuf = v1;
            if (pBuf == (uintptr)0) {
                return (int32)SQLITE_NOMEM;
            }
            rc = _sqlite3BtreePayload(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pC + 48)), libc.Uint32FromInt64(iOffset), libc.Uint32FromInt32(len1), pBuf);
            if (rc != 0) {
                return rc;
            }
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(pBuf + (uintptr)len1))).Value = 0;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(pBuf + (uintptr)(len1 + (int32)1)))).Value = 0;
            (@unsafe.Pointer.To<int8>(__ccgo_ptr(pBuf + (uintptr)(len1 + (int32)2)))).Value = 0;
            (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FiCol = iCol;
            (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FcacheStatus = cacheStatus;
            (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FcolCacheCtr = colCacheCtr;
            (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FiOffset = _sqlite3BtreeOffset(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pC + 48)));
        } else {
            pBuf = (@unsafe.Pointer.To<TVdbeTxtBlbCache>((@unsafe.Pointer)pCache)).Value.FpCValue;
        }
        _sqlite3RCStrRef(ref (tls).DerefOrNull(), pBuf);
        if ((Tu32)(t & (uint32)1) != 0) {
            rc = _sqlite3VdbeMemSetStr(tls, pDest, pBuf, (int64)len1, libc.Uint8FromInt32(encoding), __ccgo_fp(_sqlite3RCStrUnref));
            v1 = pDest + 20;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Term));
        } else {
            rc = _sqlite3VdbeMemSetStr(tls, pDest, pBuf, (int64)len1, (uint8)0, __ccgo_fp(_sqlite3RCStrUnref));
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pDest)).Value.Fenc = libc.Uint8FromInt32(encoding);
        }
    } else {
        rc = _sqlite3VdbeMemFromBtree(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pC + 48)), libc.Uint32FromInt64(iOffset), libc.Uint32FromInt32(len1), pDest);
        if (rc != 0) {
            return rc;
        }
        _sqlite3VdbeSerialGet(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pDest)).Value.Fz, t, pDest);
        if ((Tu32)(t & (uint32)1) != (uint32)0 && encoding == (int32)SQLITE_UTF8) {
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pDest)).Value.Fz + (uintptr)len1))).Value = 0;
            v1 = pDest + 20;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Term));
        }
    }
    v1 = pDest + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(MEM_Ephem));
    return rc;
}

internal static void _whereAddIndexedExpr(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, int32 iIdxCur, uintptr pTabItem) {
    int32 i = default;
    int32 j = default;
    uintptr p = default;
    uintptr pArg = default;
    uintptr pExpr = default;
    uintptr pTab = default;
    _ = i;
    _ = j;
    _ = p;
    _ = pArg;
    _ = pExpr;
    _ = pTab;
    pTab = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable;
    i = 0;
    for (;;) {
        if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn))) {
            break;
        }
        j = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)));
        if (j == -(int32)2) {
            pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaColExpr + 8 + (uintptr)i * 32))).Value.FpExpr;
        } else {
            if (j >= 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16))).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) != 0) {
                pExpr = _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pTab, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)j * 16);
            } else {
                goto _1;
            }
        }
        if (_sqlite3ExprIsConstant(tls, (uintptr)0, pExpr) != 0) {
            goto _1;
        }
        p = _sqlite3DbMallocRaw(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)32);
        if (p == (uintptr)0) {
            break;
        }
        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr;
        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpExpr = _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr, 0);
        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiDataCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.FiCursor;
        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur = iIdxCur;
        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCol = i;
        (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FbMaybeNullRow = libc.BoolUint8((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pTabItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0);
        if (_sqlite3IndexAffinityStr(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pIdx) != 0) {
            (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.Faff = libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzColAff + (uintptr)i)));
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxEpr = p;
        if ((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext == (uintptr)0) {
            pArg = pParse + 104;
            _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_whereIndexedExprCleanup), pArg);
        }
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static void _wherePartIdxExpr(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, uintptr pPart, @unsafe.Pointer pMask, int32 iIdxCur, uintptr pItem) {
    Tu8 aff = default;
    int32 bNullRow = default;
    uintptr db = default;
    uintptr p = default;
    uintptr pArg = default;
    uintptr pLeft = default;
    uintptr pRight = default;
    _ = aff;
    _ = bNullRow;
    _ = db;
    _ = p;
    _ = pArg;
    _ = pLeft;
    _ = pRight;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.Fop) == (int32)TK_AND) {
        _wherePartIdxExpr(tls, pParse, pIdx, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.FpRight, pMask, iIdxCur, pItem);
        pPart = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.FpLeft;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.Fop) == (int32)TK_EQ || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.Fop) == (int32)TK_IS) {
        pLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.FpLeft;
        pRight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pPart)).Value.FpRight;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fop) != (int32)TK_COLUMN) {
            return;
        }
        if (!(_sqlite3ExprIsConstant(tls, (uintptr)0, pRight) != 0)) {
            return;
        }
        if (!(_sqlite3IsBinary(ref (tls).DerefOrNull(), _sqlite3ExprCompareCollSeq(tls, pParse, (@unsafe.Pointer)pPart)) != 0)) {
            return;
        }
        if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FiColumn < 0) {
            return;
        }
        aff = libc.Uint8FromInt8((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FaCol + (uintptr)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FiColumn * 16))).Value.Faffinity);
        if (libc.Int32FromUint8(aff) >= (int32)SQLITE_AFF_TEXT) {
            if (pItem != 0) {
                db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
                p = _sqlite3DbMallocRaw(tls, db, (uint64)32);
                if (p != 0) {
                    bNullRow = libc.BoolInt32((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ))) != 0);
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpExpr = _sqlite3ExprDup(tls, db, pRight, 0);
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiDataCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor;
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCur = iIdxCur;
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FiIdxCol = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FiColumn;
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FbMaybeNullRow = libc.Uint8FromInt32(bNullRow);
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxPartExpr;
                    (@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.Faff = aff;
                    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpIdxPartExpr = p;
                    if ((@unsafe.Pointer.To<TIndexedExpr>((@unsafe.Pointer)p)).Value.FpIENext == (uintptr)0) {
                        pArg = pParse + 112;
                        _sqlite3ParserAddCleanup(tls, pParse, __ccgo_fp(_whereIndexedExprCleanup), pArg);
                    }
                }
            } else {
                if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FiColumn < libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)) {
                    (@unsafe.Pointer.To<TBitmask>(pMask)).Value &= ~(libc.Uint64FromInt32(1).Lsh((int64)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FiColumn)));
                }
            }
        }
    }
}

internal static Ti8 /*r*/ _wherePathSatisfiesOrderBy(ж<libc.TLS> tls, uintptr pWInfo, uintptr pOrderBy, uintptr pPath, Tu16 wctrlFlags, Tu16 nLoop, uintptr pLast, @unsafe.Pointer pRevMask) {
    int8 affRight = default;
    Tu8 bOnce = default;
    Tu8 distinctColumns = default;
    Tu8 isMatch = default;
    Tu8 isOrderDistinct = default;
    Tu8 rev = default;
    Tu8 revIdx = default;
    Tu8 revSet = default;
    Tu8 v5 = default;
    uintptr db = default;
    uintptr p = default;
    uintptr pCExpr = default;
    uintptr pColl = default;
    uintptr pColl1 = default;
    uintptr pColl2 = default;
    uintptr pIndex = default;
    uintptr pIxExpr = default;
    uintptr pLoop = default;
    uintptr pOBExpr = default;
    uintptr pParse = default;
    uintptr pTerm = default;
    uintptr pX = default;
    uintptr v4 = default;
    Tu16 eOp = default;
    Tu16 eqOpMask = default;
    Tu16 nColumn = default;
    Tu16 nKeyCol = default;
    Tu16 nOrderBy = default;
    int32 i = default;
    int32 iColumn = default;
    int32 iCur = default;
    int32 iLoop = default;
    int32 j = default;
    TBitmask m = default;
    TBitmask mTerm = default;
    TBitmask obDone = default;
    ref var obSat = ref heap(new TBitmask(), out var ᏑobSat);
    TBitmask orderDistinctMask = default;
    TBitmask ready = default;
    uint64 v11 = default;
    _ = affRight;
    _ = bOnce;
    _ = db;
    _ = distinctColumns;
    _ = eOp;
    _ = eqOpMask;
    _ = i;
    _ = iColumn;
    _ = iCur;
    _ = iLoop;
    _ = isMatch;
    _ = isOrderDistinct;
    _ = j;
    _ = m;
    _ = mTerm;
    _ = nColumn;
    _ = nKeyCol;
    _ = nOrderBy;
    _ = obDone;
    _ = obSat;
    _ = orderDistinctMask;
    _ = p;
    _ = pCExpr;
    _ = pColl;
    _ = pColl1;
    _ = pColl2;
    _ = pIndex;
    _ = pIxExpr;
    _ = pLoop;
    _ = pOBExpr;
    _ = pParse;
    _ = pTerm;
    _ = pX;
    _ = ready;
    _ = rev;
    _ = revIdx;
    _ = revSet;
    _ = v11;
    _ = v4;
    _ = v5;
    pLoop = (uintptr)0;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb;
    obSat = (uint64)0;
    if (nLoop != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OrderByIdxJoin))) != (uint32)0) {
        return 0;
    }
    nOrderBy = libc.Uint16FromInt32((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr);
    if (libc.Int32FromUint16(nOrderBy) > libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)) {
        return 0;
    }
    isOrderDistinct = (uint8)1;
    obDone = libc.Uint64FromInt32(1).Lsh((uint64)(nOrderBy)) - (uint64)1;
    orderDistinctMask = (uint64)0;
    ready = (uint64)0;
    eqOpMask = libc.Uint16FromInt32(libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS) | libc.Int32FromInt32(WO_ISNULL));
    if ((libc.Int32FromUint16(wctrlFlags) & (libc.Int32FromInt32(WHERE_ORDERBY_LIMIT) | libc.Int32FromInt32(WHERE_ORDERBY_MAX) | libc.Int32FromInt32(WHERE_ORDERBY_MIN))) != 0) {
        eqOpMask = libc.Uint16FromInt32((int32)eqOpMask | libc.Int32FromInt32(WO_IN));
    }
    iLoop = 0;
    for (;;) {
        if (!(isOrderDistinct != 0 && obSat < obDone && iLoop <= libc.Int32FromUint16(nLoop))) {
            break;
        }
        if (iLoop > 0) {
            ready = (TBitmask)(ready | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf);
        }
        if (iLoop < libc.Int32FromUint16(nLoop)) {
            pLoop = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWherePath>((@unsafe.Pointer)pPath)).Value.FaLoop + (uintptr)iLoop * 8));
            if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_ORDERBY_LIMIT) != 0) {
                goto _1;
            }
        } else {
            pLoop = pLast;
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_VIRTUALTABLE) != 0) {
            if ((@unsafe.Pointer.To<_sqlite3WhereAddExplainText_type>((@unsafe.Pointer)(pLoop + 24))).Value.FisOrdered != 0 && (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy == pOrderBy) {
                obSat = obDone;
            } else {
                isOrderDistinct = (uint8)0;
            }
            break;
        }
        iCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FiTab * 80))).Value.FiCursor;
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint16(nOrderBy))) {
                break;
            }
            if ((libc.Uint64FromInt32(1).Lsh((int64)(i)) & obSat) != 0) {
                goto _2;
            }
            pOBExpr = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr);
            if (pOBExpr == (uintptr)0) {
                goto _2;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.Fop) != (int32)TK_COLUMN && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.Fop) != (int32)TK_AGG_COLUMN) {
                goto _2;
            }
            if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.FiTable != iCur) {
                goto _2;
            }
            pTerm = _sqlite3WhereFindTerm(tls, pWInfo + 104, iCur, (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.FiColumn, ~ready, (uint32)eqOpMask, (uintptr)0);
            if (pTerm == (uintptr)0) {
                goto _2;
            }
            if (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) == (int32)WO_IN) {
                j = 0;
                for (;;) {
                    if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnLTerm) && pTerm != ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8)))) {
                        break;
                    }
                    goto _3;
_3:;
                    j = j + 1;
                }
                if (j >= libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnLTerm)) {
                    goto _2;
                }
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS))) != 0 && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.FiColumn >= 0) {
                pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
                pColl1 = _sqlite3ExprNNCollSeq(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr);
                pCExpr = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
                pColl2 = _sqlite3ExprCompareCollSeq(tls, pParse, (@unsafe.Pointer)pCExpr);
                if (pColl2 == (uintptr)0 || _sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl1)).Value.FzName, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl2)).Value.FzName) != 0) {
                    goto _2;
                }
                affRight = _sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCExpr)).Value.FpRight);
                if ((int32)affRight != 0 && (int32)affRight != (int32)_sqlite3ExprAffinity(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pCExpr)).Value.FpLeft)) {
                    goto _2;
                }
            }
            obSat = (TBitmask)(obSat | libc.Uint64FromInt32(1).Lsh((int64)(i)));
            goto _2;
_2:;
            i = i + 1;
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_ONEROW) == (uint32)0) {
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IPK) != 0) {
                if ((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpOrderBy != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OrderBySubq))) == (uint32)0 && _wherePathMatchSubqueryOB(ref (tls).DerefOrNull(), pLoop, iLoop, iCur, pOrderBy, pRevMask, @unsafe.Pointer.FromPinnedBox(ᏑobSat), wctrlFlags) != 0) {
                    nColumn = (uint16)0;
                    isOrderDistinct = (uint8)0;
                } else {
                    nColumn = (uint16)1;
                }
                pIndex = (uintptr)0;
                nKeyCol = (uint16)0;
            } else {
                v4 = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
                pIndex = v4;
                if (v4 == (uintptr)0 || (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIndex + 100)) & 0x4) >> 2) != 0) {
                    return 0;
                } else {
                    nKeyCol = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnKeyCol;
                    nColumn = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnColumn;
                    isOrderDistinct = libc.BoolUint8(libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FonError) != OE_None && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_SKIPSCAN) == (uint32)0);
                }
            }
            v5 = libc.Uint8FromInt32(0);
            revSet = v5;
            rev = v5;
            distinctColumns = (uint8)0;
            j = 0;
            for (;;) {
                if (!(j < libc.Int32FromUint16(nColumn))) {
                    break;
                }
                bOnce = (uint8)1;
                if (j < libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq) && j >= libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnSkip)) {
                    eOp = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8))))).Value.FeOperator;
                    if ((libc.Int32FromUint16(eOp) & libc.Int32FromUint16(eqOpMask)) != 0) {
                        if ((libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_ISNULL) | libc.Int32FromInt32(WO_IS))) != 0) {
                            isOrderDistinct = (uint8)0;
                        }
                        goto _6;
                    } else {
                        if ((libc.Int32FromUint16(eOp) & (int32)WO_IN) != 0) {
                            pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8))))).Value.FpExpr;
                            i = j + (int32)1;
                            for (;;) {
                                if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq))) {
                                    break;
                                }
                                if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)i * 8))))).Value.FpExpr == pX) {
                                    bOnce = (uint8)0;
                                    break;
                                }
                                goto _7;
_7:;
                                i = i + 1;
                            }
                        }
                    }
                }
                if (pIndex != 0) {
                    iColumn = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiColumn + (uintptr)j * 2)));
                    revIdx = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaSortOrder + (uintptr)j))) & (int32)KEYINFO_ORDER_DESC);
                    if (iColumn == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable)).Value.FiPKey) {
                        iColumn = -(int32)1;
                    }
                } else {
                    iColumn = -(int32)1;
                    revIdx = (uint8)0;
                }
                if (isOrderDistinct != 0) {
                    if (iColumn >= 0 && j >= libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq) && (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable)).Value.FaCol + (uintptr)iColumn * 16 + 8)) & 0xf) >> 0) == 0) {
                        isOrderDistinct = (uint8)0;
                    }
                    if (iColumn == -(int32)2) {
                        isOrderDistinct = (uint8)0;
                    }
                }
                isMatch = (uint8)0;
                i = 0;
                for (;;) {
                    if (!(bOnce != 0 && i < libc.Int32FromUint16(nOrderBy))) {
                        break;
                    }
                    if ((libc.Uint64FromInt32(1).Lsh((int64)(i)) & obSat) != 0) {
                        goto _8;
                    }
                    pOBExpr = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr);
                    if (pOBExpr == (uintptr)0) {
                        goto _8;
                    }
                    if ((libc.Int32FromUint16(wctrlFlags) & (libc.Int32FromInt32(WHERE_GROUPBY) | libc.Int32FromInt32(WHERE_DISTINCTBY))) == 0) {
                        bOnce = (uint8)0;
                    }
                    if (iColumn >= -(int32)1) {
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.Fop) != (int32)TK_COLUMN && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.Fop) != (int32)TK_AGG_COLUMN) {
                            goto _8;
                        }
                        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.FiTable != iCur) {
                            goto _8;
                        }
                        if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pOBExpr)).Value.FiColumn != iColumn) {
                            goto _8;
                        }
                    } else {
                        pIxExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaColExpr + 8 + (uintptr)j * 32))).Value.FpExpr;
                        if (_sqlite3ExprCompareSkip(tls, pOBExpr, pIxExpr, iCur) != 0) {
                            goto _8;
                        }
                    }
                    if (iColumn != -(int32)1) {
                        pColl = _sqlite3ExprNNCollSeq(tls, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr);
                        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FzName, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FazColl + (uintptr)j * 8))) != 0) {
                            goto _8;
                        }
                    }
                    if ((libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_DISTINCTBY) != 0) {
                        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnDistinctCol = libc.Uint16FromInt32(j + (int32)1);
                    }
                    isMatch = (uint8)1;
                    break;
                    goto _8;
_8:;
                    i = i + 1;
                }
                if (isMatch != 0 && (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_GROUPBY) == 0) {
                    if (revSet != 0) {
                        if ((libc.Int32FromUint8(rev) ^ libc.Int32FromUint8(revIdx)) != (libc.Int32FromUint8((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.Ffg.FsortFlags) & (int32)KEYINFO_ORDER_DESC)) {
                            isMatch = (uint8)0;
                        }
                    } else {
                        rev = libc.Uint8FromInt32(libc.Int32FromUint8(revIdx) ^ libc.Int32FromUint8((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.Ffg.FsortFlags) & (int32)KEYINFO_ORDER_DESC);
                        if (rev != 0) {
                            (@unsafe.Pointer.To<TBitmask>(pRevMask)).Value |= libc.Uint64FromInt32(1).Lsh((int64)(iLoop));
                        }
                        revSet = (uint8)1;
                    }
                }
                if (isMatch != 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.Ffg.FsortFlags) & (int32)KEYINFO_ORDER_BIGNULL) != 0) {
                    if (j == libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq)) {
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLoop + 48))).Value |= (uint32)WHERE_BIGNULL_SORT;
                    } else {
                        isMatch = (uint8)0;
                    }
                }
                if (isMatch != 0) {
                    if (iColumn == -(int32)1) {
                        distinctColumns = (uint8)1;
                    }
                    obSat = (TBitmask)(obSat | libc.Uint64FromInt32(1).Lsh((int64)(i)));
                } else {
                    if (j == 0 || j < libc.Int32FromUint16(nKeyCol)) {
                        isOrderDistinct = (uint8)0;
                    }
                    break;
                }
                goto _6;
_6:;
                j = j + 1;
            }
            if (distinctColumns != 0) {
                isOrderDistinct = (uint8)1;
            }
        }
        if (isOrderDistinct != 0) {
            orderDistinctMask = (TBitmask)(orderDistinctMask | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf);
            i = 0;
            for (;;) {
                if (!(i < libc.Int32FromUint16(nOrderBy))) {
                    break;
                }
                if ((libc.Uint64FromInt32(1).Lsh((int64)(i)) & obSat) != 0) {
                    goto _9;
                }
                p = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOrderBy + 8 + (uintptr)i * 32))).Value.FpExpr;
                mTerm = _sqlite3WhereExprUsage(ref (tls).DerefOrNull(), pWInfo + 592, p);
                if (mTerm == (uint64)0 && !(_sqlite3ExprIsConstant(tls, (uintptr)0, p) != 0)) {
                    goto _9;
                }
                if ((TBitmask)(mTerm & ~orderDistinctMask) == (uint64)0) {
                    obSat = (TBitmask)(obSat | libc.Uint64FromInt32(1).Lsh((int64)(i)));
                }
                goto _9;
_9:;
                i = i + 1;
            }
        }
        goto _1;
_1:;
        iLoop = iLoop + 1;
    }
    if (obSat == obDone) {
        return libc.Int8FromUint16(nOrderBy);
    }
    if (!(isOrderDistinct != 0)) {
        i = libc.Int32FromUint16(nOrderBy) - (int32)1;
        for (;;) {
            if (!(i > 0)) {
                break;
            }
            if (i < libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))) {
                v11 = libc.Uint64FromInt32(1).Lsh((int64)(i)) - (uint64)1;
            } else {
                v11 = (uint64)0;
            }
            m = v11;
            if ((TBitmask)(obSat & m) == m) {
                return (int8)i;
            }
            goto _10;
_10:;
            i = i - 1;
        }
        return 0;
    }
    return (int8)(-(int32)1);
}

internal static int32 /*r*/ _whereRangeSkipScanEst(ж<libc.TLS> tls, uintptr pParse, uintptr pLower, uintptr pUpper, uintptr pLoop, @unsafe.Pointer pbDone) {
    Tu8 aff = default;
    uintptr db = default;
    uintptr p = default;
    ref var p1 = ref heap(new uintptr(), out var Ꮡp1);
    ref var p2 = ref heap(new uintptr(), out var Ꮡp2);
    uintptr pColl = default;
    ref var pVal = ref heap(new uintptr(), out var ᏑpVal);
    uintptr v3 = default;
    int32 i = default;
    int32 nAdjust = default;
    int32 nDiff = default;
    int32 nEq = default;
    int32 nLower = default;
    int32 nUpper = default;
    int32 rc = default;
    int32 res = default;
    int32 res1 = default;
    int32 v1 = default;
    _ = aff;
    _ = db;
    _ = i;
    _ = nAdjust;
    _ = nDiff;
    _ = nEq;
    _ = nLower;
    _ = nUpper;
    _ = p;
    _ = p1;
    _ = p2;
    _ = pColl;
    _ = pVal;
    _ = rc;
    _ = res;
    _ = res1;
    _ = v1;
    _ = v3;
    p = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
    nEq = libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq);
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    nLower = -(int32)1;
    nUpper = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FnSample + (int32)1;
    rc = SQLITE_OK;
    aff = libc.Uint8FromInt8(_sqlite3IndexColumnAffinity(tls, db, p, nEq));
    p1 = (uintptr)0;
    p2 = (uintptr)0;
    pVal = (uintptr)0;
    pColl = _sqlite3LocateCollSeq(tls, pParse, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FazColl + (uintptr)nEq * 8)));
    if (pLower != 0) {
        rc = _sqlite3Stat4ValueFromExpr(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pLower)).Value.FpExpr)).Value.FpRight, aff, @unsafe.Pointer.FromBox(Ꮡp1));
        nLower = 0;
    }
    if (pUpper != 0 && rc == SQLITE_OK) {
        rc = _sqlite3Stat4ValueFromExpr(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pUpper)).Value.FpExpr)).Value.FpRight, aff, @unsafe.Pointer.FromBox(Ꮡp2));
        if (p2 != 0) {
            v1 = 0;
        } else {
            v1 = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FnSample;
        }
        nUpper = v1;
    }
    if (p1 != 0 || p2 != 0) {
        i = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && i < (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FnSample)) {
                break;
            }
            rc = _sqlite3Stat4Column(tls, db, (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FaSample + (uintptr)i * 40))).Value.Fp, (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FaSample + (uintptr)i * 40))).Value.Fn, nEq, @unsafe.Pointer.FromBox(ᏑpVal));
            if (rc == SQLITE_OK && p1 != 0) {
                res = _sqlite3MemCompare(tls, p1, pVal, pColl);
                if (res >= 0) {
                    nLower = nLower + 1;
                }
            }
            if (rc == SQLITE_OK && p2 != 0) {
                res1 = _sqlite3MemCompare(tls, p2, pVal, pColl);
                if (res1 >= 0) {
                    nUpper = nUpper + 1;
                }
            }
            goto _2;
_2:;
            i = i + 1;
        }
        nDiff = nUpper - nLower;
        if (nDiff <= 0) {
            nDiff = (int32)1;
        }
        if (nDiff != (int32)1 || pUpper == (uintptr)0 || pLower == (uintptr)0) {
            nAdjust = (int32)_sqlite3LogEst(ref (tls).DerefOrNull(), libc.Uint64FromInt32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)p)).Value.FnSample)) - (int32)_sqlite3LogEst(ref (tls).DerefOrNull(), libc.Uint64FromInt32(nDiff));
            v3 = pLoop + 22;
            (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v3)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v3)) - nAdjust);
            (@unsafe.Pointer.To<int32>(pbDone)).Value = (int32)1;
        }
    } else {
    }
    _sqlite3ValueFree(tls, p1);
    _sqlite3ValueFree(tls, p2);
    _sqlite3ValueFree(tls, pVal);
    return rc;
}

internal static int32 /*r*/ _whereRangeVectorLen(ж<libc.TLS> tls, uintptr pParse, int32 iCur, uintptr pIdx, int32 nEq, uintptr pTerm) {
    int8 aff = default;
    int8 idxaff = default;
    int32 i = default;
    int32 nCmp = default;
    int32 v1 = default;
    uintptr pColl = default;
    uintptr pLhs = default;
    uintptr pRhs = default;
    uintptr t = default;
    _ = aff;
    _ = i;
    _ = idxaff;
    _ = nCmp;
    _ = pColl;
    _ = pLhs;
    _ = pRhs;
    _ = t;
    _ = v1;
    nCmp = _sqlite3ExprVectorSize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpLeft);
    if (nCmp < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn) - nEq) {
        v1 = nCmp;
    } else {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn) - nEq;
    }
    nCmp = v1;
    i = (int32)1;
    for (;;) {
        if (!(i < nCmp)) {
            break;
        }
        idxaff = 0;
        pLhs = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpLeft + 32)) + 8 + (uintptr)i * 32))).Value.FpExpr;
        pRhs = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpRight;
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRhs)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
            pRhs = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRhs + 32))))).Value.FpEList + 8 + (uintptr)i * 32))).Value.FpExpr;
        } else {
            pRhs = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRhs + 32)) + 8 + (uintptr)i * 32))).Value.FpExpr;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLhs)).Value.Fop) != (int32)TK_COLUMN || (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLhs)).Value.FiTable != iCur || (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLhs)).Value.FiColumn != (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)(i + nEq) * 2))) || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSortOrder + (uintptr)(i + nEq)))) != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSortOrder + (uintptr)nEq)))) {
            break;
        }
        aff = _sqlite3CompareAffinity(tls, pRhs, _sqlite3ExprAffinity(tls, pLhs));
        idxaff = _sqlite3TableColumnAffinity(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable, (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLhs)).Value.FiColumn);
        if ((int32)aff != (int32)idxaff) {
            break;
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Commuted))) != (uint32)0) {
            t = pRhs;
            pRhs = pLhs;
            pLhs = t;
        }
        pColl = _sqlite3BinaryCompareCollSeq(tls, pParse, pLhs, pRhs);
        if (pColl == (uintptr)0) {
            break;
        }
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FzName, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)(i + nEq) * 8))) != 0) {
            break;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    return i;
}

internal static uintptr /*r*/ _whereScanNext(ж<libc.TLS> tls, @unsafe.Pointer pScan) {
    Ti16 iColumn = default;
    int32 iCur = default;
    int32 j = default;
    int32 k = default;
    uintptr pColl = default;
    uintptr pParse = default;
    uintptr pTerm = default;
    uintptr pWC = default;
    uintptr pX = default;
    uintptr zCollName = default;
    uintptr v2 = default;
    bool v3 = default;
    _ = iColumn;
    _ = iCur;
    _ = j;
    _ = k;
    _ = pColl;
    _ = pParse;
    _ = pTerm;
    _ = pWC;
    _ = pX;
    _ = zCollName;
    _ = v2;
    _ = v3;
    k = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pScan, 32));
    pWC = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 8));
    while ((int32)1 != 0) {
        iColumn = ~@unsafe.Pointer.To<Ti16>(@unsafe.Add(@unsafe.Add(pScan, 88), (uintptr)(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41))) - (int32)1) * 2));
        iCur = ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pScan, 44), (uintptr)(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41))) - (int32)1) * 4));
        for (var cond = true; cond; cond = pWC != (uintptr)0) {
            pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)k * 56;
            for (;;) {
                if (!(k < (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FleftCursor == iCur && (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FleftColumn == (int32)iColumn && ((int32)iColumn != -(int32)2 || _sqlite3ExprCompareSkip(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpLeft, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 24)), iCur) == 0) && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41))) <= (int32)1 || !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON))) != libc.Uint32FromInt32(0)))) {
                    v3 = (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_EQUIV) != 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42))) < libc.Int32FromUint64(libc.Uint64FromInt64(44) / libc.Uint64FromInt64(4)); if (v3) {
                        v2 = _whereRightSubexprIsColumn(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr);
                        pX = v2;
                    }
                    if (v3 && v2 != (uintptr)0) {
                        j = 0;
                        for (;;) {
                            if (!(j < libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42))))) {
                                break;
                            }
                            if (~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pScan, 44), (uintptr)j * 4)) == (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FiTable && (int32)(~@unsafe.Pointer.To<Ti16>(@unsafe.Add(@unsafe.Add(pScan, 88), (uintptr)j * 2))) == (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FiColumn) {
                                break;
                            }
                            goto _4;
_4:;
                            j = j + 1;
                        }
                        if (j == libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42)))) {
                            (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pScan, 44), (uintptr)j * 4))).Value = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FiTable;
                            (@unsafe.Pointer.To<Ti16>(@unsafe.Add(@unsafe.Add(pScan, 88), (uintptr)j * 2))).Value = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FiColumn;
                            (@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42))).Value = (uint8)(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42)) + 1);
                        }
                    }
                    if (((uint32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator & ~@unsafe.Pointer.To<Tu32>(@unsafe.Add(pScan, 36))) != (uint32)0) {
                        if (~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 16)) != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_ISNULL) == 0) {
                            pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo)).Value.FpParse;
                            pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
                            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_IN) != 0) {
                                zCollName = _indexInAffinityOk(tls, pParse, pTerm, libc.Uint8FromInt8(~@unsafe.Pointer.To<int8>(@unsafe.Add(pScan, 40))));
                                if (!(zCollName != 0)) {
                                    goto _1;
                                }
                            } else {
                                if (!(_sqlite3IndexAffinityOk(tls, (@unsafe.Pointer)pX, ~@unsafe.Pointer.To<int8>(@unsafe.Add(pScan, 40))) != 0)) {
                                    goto _1;
                                }
                                pColl = _sqlite3ExprCompareCollSeq(tls, pParse, (@unsafe.Pointer)pX);
                                if (pColl != 0) {
                                    v2 = (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FzName;
                                } else {
                                    v2 = (uintptr)Ꮡ_sqlite3StrBINARY;
                                }
                                zCollName = v2;
                            }
                            if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zCollName, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 16))) != 0) {
                                goto _1;
                            }
                        }
                        v3 = (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS))) != 0; if (v3) {
                            pX = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.FpRight;
                        }
                        if (v3 && pX != libc.UintptrFromInt32(0) && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_COLUMN && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FiTable == ~@unsafe.Pointer.To<int32>(@unsafe.Add(pScan, 44)) && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FiColumn == (int32)(~@unsafe.Pointer.To<Ti16>(@unsafe.Add(pScan, 88)))) {
                            goto _1;
                        }
                        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pScan, 8))).Value = pWC;
                        (@unsafe.Pointer.To<int32>(@unsafe.Add(pScan, 32))).Value = k + (int32)1;
                        return pTerm;
                    }
                }
                goto _1;
_1:;
                k = k + 1;
                pTerm += 56;
            }
            pWC = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpOuter;
            k = 0;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41))) >= libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 42)))) {
            break;
        }
        pWC = ~@unsafe.Pointer.To<uintptr>(pScan);
        k = 0;
        (@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41))).Value = (uint8)(~@unsafe.Pointer.To<uint8>(@unsafe.Add(pScan, 41)) + 1);
    }
    return (uintptr)0;
}

internal static int32 /*r*/ _writeSuperJournal(ж<libc.TLS> tls, uintptr pPager, uintptr zSuper) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu32 cksum = default;
        Ti64 iHdrOff = default;
        int32 nSuper = default;
        int32 rc = default;
        int32 v2 = default;
        int32 v3 = default;
        int32 v5 = default;
        int32 v7 = default;
        int32 v9 = default;
        bool v10 = default;
        bool v4 = default;
        bool v6 = default;
        bool v8 = default;
        Ti64 _ᴛ1 = default;
        _ = cksum;
        _ = iHdrOff;
        _ = nSuper;
        _ = rc;
        _ = v10;
        _ = v2;
        _ = v3;
        _ = v4;
        _ = v5;
        _ = v6;
        _ = v7;
        _ = v8;
        _ = v9;
        cksum = (uint32)0;
        if (!(zSuper != 0) || libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_MEMORY || !((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != libc.UintptrFromInt32(0))) {
            r = SQLITE_OK; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsetSuper = (uint8)1;
        nSuper = 0;
        for (;;) {
            if (!(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSuper + (uintptr)nSuper)) != 0)) {
                break;
            }
            cksum = cksum + libc.Uint32FromInt8(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zSuper + (uintptr)nSuper)));
            goto _1;
_1:;
            nSuper = nSuper + 1;
        }
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FfullSync != 0) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = _journalHdrOffset(ref (tls).DerefOrNull(), pPager);
        }
        iHdrOff = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
        v2 = _write32bits(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, iHdrOff, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FlckPgno);
        rc = v2;
        v4 = 0 != v2; if (!v4) {
            v3 = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, zSuper, nSuper, iHdrOff + (int64)4);
            rc = v3;
        }
        v6 = v4 || 0 != v3; if (!v6) {
            v5 = _write32bits(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, iHdrOff + (int64)4 + (int64)nSuper, libc.Uint32FromInt32(nSuper));
            rc = v5;
        }
        v8 = v6 || 0 != v5; if (!v8) {
            v7 = _write32bits(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, iHdrOff + (int64)4 + (int64)nSuper + (int64)4, cksum);
            rc = v7;
        }
        v10 = v8 || 0 != v7; if (!v10) {
            v9 = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, (uintptr)Ꮡ_aJournalMagic, (int32)8, iHdrOff + (int64)4 + (int64)nSuper + (int64)8);
            rc = v9;
        }
        if (v10 || 0 != v9) {
            r = rc; goto ᒐdone;
        }
        (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pPager + 96))).Value += (int64)(nSuper + libc.Int32FromInt32(20));
        v2 = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, bp);
        rc = v2;
        if (SQLITE_OK == v2 && ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff) {
            rc = _sqlite3OsTruncate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _xferOptimization(ж<libc.TLS> tls, uintptr pParse, uintptr pDest, uintptr pSelect, int32 onError, int32 iDbDest) {
    int32 r = default;

    int32 addr1 = default;
    int32 addr2 = default;
    int32 destHasUniqueIdx = default;
    int32 emptyDestTest = default;
    int32 emptySrcTest = default;
    int32 i = default;
    int32 iDb = default;
    int32 iDbSrc = default;
    int32 iDest = default;
    int32 iSrc = default;
    int32 regAutoinc = default;
    int32 regData = default;
    int32 regRowid = default;
    int32 v6 = default;
    uintptr db = default;
    uintptr pDestCol = default;
    uintptr pDestExpr = default;
    uintptr pDestIdx = default;
    uintptr pEList = default;
    uintptr pItem = default;
    uintptr pSrc = default;
    uintptr pSrcCol = default;
    uintptr pSrcCol1 = default;
    uintptr pSrcExpr = default;
    uintptr pSrcIdx = default;
    uintptr v = default;
    uintptr zColl = default;
    uintptr v7 = default;
    uint32 eDestType = default;
    uint32 eSrcType = default;
    Tu8 idxInsFlags = default;
    Tu8 insFlags = default;
    _ = addr1;
    _ = addr2;
    _ = db;
    _ = destHasUniqueIdx;
    _ = eDestType;
    _ = eSrcType;
    _ = emptyDestTest;
    _ = emptySrcTest;
    _ = i;
    _ = iDb;
    _ = iDbSrc;
    _ = iDest;
    _ = iSrc;
    _ = idxInsFlags;
    _ = insFlags;
    _ = pDestCol;
    _ = pDestExpr;
    _ = pDestIdx;
    _ = pEList;
    _ = pItem;
    _ = pSrc;
    _ = pSrcCol;
    _ = pSrcCol1;
    _ = pSrcExpr;
    _ = pSrcIdx;
    _ = regAutoinc;
    _ = regData;
    _ = regRowid;
    _ = v;
    _ = zColl;
    _ = v6;
    _ = v7;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    emptyDestTest = 0;
    emptySrcTest = 0;
    destHasUniqueIdx = 0;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpWith != 0 || (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWith != 0) {
        return 0;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FeTabType) == (int32)TABTYP_VTAB) {
        return 0;
    }
    if (onError == (int32)OE_Default) {
        if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FiPKey >= 0) {
            onError = libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FkeyConf);
        }
        if (onError == (int32)OE_Default) {
            onError = (int32)OE_Abort;
        }
    }
    if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc)).Value.FnSrc != (int32)1) {
        return 0;
    }
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8 + 24 + 4)) & 0x4) >> 2) != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpWhere != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpOrderBy != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpGroupBy != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpLimit != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior != 0) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FselFlags & (uint32)SF_Distinct) != 0) {
        return 0;
    }
    pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpEList;
    if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr != (int32)1) {
        return 0;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8))).Value.FpExpr)).Value.Fop) != (int32)TK_ASTERISK) {
        return 0;
    }
    pItem = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc + 8;
    pSrc = _sqlite3LocateTableItem(tls, pParse, (uint32)0, pItem);
    if (pSrc == (uintptr)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.Ftnum == (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.Ftnum && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpSchema == (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpSchema) {
        return 0;
    }
    if (libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) != libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0)) {
        return 0;
    }
    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_NORM))) {
        return 0;
    }
    if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FnCol != (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FnCol) {
        return 0;
    }
    if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FiPKey != (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FiPKey) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FtabFlags & (uint32)TF_Strict) != (uint32)0) {
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FtabFlags & (uint32)TF_Strict) == (uint32)0) {
            return 0;
        }
        i = 0;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FnCol)) {
                break;
            }
            eDestType = libc.Uint32FromInt32((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FaCol + (uintptr)i * 16 + 8)) & 0xf0) >> 4));
            eSrcType = libc.Uint32FromInt32((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FaCol + (uintptr)i * 16 + 8)) & 0xf0) >> 4));
            if (eDestType == (uint32)COLTYPE_ANY) {
                goto _1;
            }
            if (eDestType == eSrcType) {
                goto _1;
            }
            if (eDestType == (uint32)COLTYPE_INT && eSrcType == (uint32)COLTYPE_INTEGER) {
                goto _1;
            }
            if (eDestType == (uint32)COLTYPE_INTEGER && eSrcType == (uint32)COLTYPE_INT) {
                goto _1;
            }
            return 0;
            goto _1;
_1:;
            i = i + 1;
        }
    }
    i = 0;
    for (;;) {
        if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FnCol)) {
            break;
        }
        pDestCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FaCol + (uintptr)i * 16;
        pSrcCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FaCol + (uintptr)i * 16;
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pDestCol)).Value.FcolFlags) & (int32)COLFLAG_GENERATED) != (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pSrcCol)).Value.FcolFlags) & (int32)COLFLAG_GENERATED)) {
            return 0;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pDestCol)).Value.FcolFlags) & (int32)COLFLAG_GENERATED) != 0) {
            if (_sqlite3ExprCompare(tls, (uintptr)0, _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pSrc, pSrcCol), _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pDest, pDestCol), -(int32)1) != 0) {
                return 0;
            }
        }
        if ((int32)(@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pDestCol)).Value.Faffinity != (int32)(@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pSrcCol)).Value.Faffinity) {
            return 0;
        }
        if (Xsqlite3_stricmp(tls, _sqlite3ColumnColl(ref (tls).DerefOrNull(), pDestCol), _sqlite3ColumnColl(ref (tls).DerefOrNull(), pSrcCol)) != 0) {
            return 0;
        }
        if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pDestCol + 8)) & 0xf) >> 0) != 0 && !((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pSrcCol + 8)) & 0xf) >> 0) != 0)) {
            return 0;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pDestCol)).Value.FcolFlags) & (int32)COLFLAG_GENERATED) == 0 && i > 0) {
            pDestExpr = _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pDest, pDestCol);
            pSrcExpr = _sqlite3ColumnExpr(ref (tls).DerefOrNull(), pSrc, pSrcCol);
            if (libc.BoolInt32(pDestExpr == (uintptr)0) != libc.BoolInt32(pSrcExpr == (uintptr)0) || pDestExpr != (uintptr)0 && libc.Xstrcmp(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pDestExpr + 8)), ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrcExpr + 8))) != 0) {
                return 0;
            }
        }
        goto _2;
_2:;
        i = i + 1;
    }
    pDestIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpIndex;
    for (;;) {
        if (!(pDestIdx != 0)) {
            break;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pDestIdx)).Value.FonError) != OE_None) {
            destHasUniqueIdx = (int32)1;
        }
        pSrcIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpIndex;
        for (;;) {
            if (!(pSrcIdx != 0)) {
                break;
            }
            if (_xferCompatibleIndex(tls, pDestIdx, pSrcIdx) != 0) {
                break;
            }
            goto _4;
_4:;
            pSrcIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.FpNext;
        }
        if (pSrcIdx == (uintptr)0) {
            return 0;
        }
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.Ftnum == (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pDestIdx)).Value.Ftnum && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpSchema == (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpSchema && _sqlite3FaultSim(tls, (int32)411) == SQLITE_OK) {
            return 0;
        }
        goto _3;
_3:;
        pDestIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pDestIdx)).Value.FpNext;
    }
    if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpCheck != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0 && !(_xferCompatibleCheck(tls, pDest, pSrc) != 0)) {
        return 0;
    }
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_ForeignKeys) != (uint64)0 && (@unsafe.Pointer.To<TTableᴛ1_Fu_Ftab>((@unsafe.Pointer)(pDest + 64))).Value.FpFKey != (uintptr)0) {
        return 0;
    }
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00001)).Lsh((int64)(libc.Int32FromInt32(32))))) != (uint64)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth != 0) {
        iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpSchema);
        if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_SELECT, (uintptr)0, (uintptr)0, (uintptr)0) != 0) {
            return 0;
        }
        i = 0;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FnCol)) {
                break;
            }
            pSrcCol1 = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FaCol + (uintptr)i * 16;
            if (_sqlite3AuthReadCol(tls, pParse, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FzName, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pSrcCol1)).Value.FzCnName, iDb) != 0) {
                return 0;
            }
            goto _5;
_5:;
            i = i + 1;
        }
    }
    iDbSrc = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpSchema);
    v = _sqlite3GetVdbe(tls, pParse);
    _sqlite3CodeVerifySchema(tls, pParse, iDbSrc);
    v7 = pParse + 56;
    v6 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7) + 1;
    iSrc = v6;
    v7 = pParse + 56;
    v6 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7) + 1;
    iDest = v6;
    regAutoinc = _autoIncBegin(tls, pParse, iDbDest, pDest);
    regData = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Null, 0, regData);
    regRowid = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
    _sqlite3OpenTable(tls, pParse, iDest, iDbDest, pDest, (int32)OP_OpenWrite);
    if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0 && ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FiPKey < 0 && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpIndex != (uintptr)0 || destHasUniqueIdx != 0 || onError != (int32)OE_Abort && onError != (int32)OE_Rollback)) {
        addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iDest, 0);
        emptyDestTest = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Goto);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
    }
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
        _sqlite3OpenTable(tls, pParse, iSrc, iDbSrc, pSrc, (int32)OP_OpenRead);
        emptySrcTest = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iSrc, 0);
        if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FiPKey >= 0) {
            addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iSrc, regRowid);
            if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0) {
                addr2 = _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iDest, 0, regRowid);
                _sqlite3RowidConstraint(tls, pParse, onError, pDest);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr2);
            }
            _autoIncStep(tls, pParse, regAutoinc, regRowid);
        } else {
            if ((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpIndex == (uintptr)0 && !((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & libc.Uint32FromInt32(DBFLAG_VacuumInto)) != 0)) {
                addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, iDest, regRowid);
            } else {
                addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rowid, iSrc, regRowid);
            }
        }
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) != 0) {
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_SeekEnd, iDest);
            insFlags = libc.Uint8FromInt32(libc.Int32FromInt32(OPFLAG_APPEND) | libc.Int32FromInt32(OPFLAG_USESEEKRESULT) | libc.Int32FromInt32(OPFLAG_PREFORMAT));
        } else {
            insFlags = libc.Uint8FromInt32(libc.Int32FromInt32(OPFLAG_NCHANGE) | libc.Int32FromInt32(OPFLAG_LASTROWID) | libc.Int32FromInt32(OPFLAG_APPEND) | libc.Int32FromInt32(OPFLAG_PREFORMAT));
        }
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_RowData, iSrc, regData, (int32)1);
            insFlags = libc.Uint8FromInt32((int32)insFlags & ~libc.Int32FromInt32(OPFLAG_PREFORMAT));
        } else {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_RowCell, iDest, iSrc, regRowid);
        }
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, iDest, regData, regRowid);
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0) {
            _sqlite3VdbeChangeP4(tls, v, -(int32)1, pDest, -(int32)5);
        }
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)insFlags);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iSrc, addr1);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Close, iSrc, 0);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Close, iDest, 0);
    } else {
        _sqlite3TableLock(tls, pParse, iDbDest, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.Ftnum, (uint8)1, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FzName);
        _sqlite3TableLock(tls, pParse, iDbSrc, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.Ftnum, (uint8)0, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FzName);
    }
    pDestIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FpIndex;
    for (;;) {
        if (!(pDestIdx != 0)) {
            break;
        }
        idxInsFlags = (uint8)0;
        pSrcIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FpIndex;
        for (;;) {
            if (!(pSrcIdx != 0)) {
                break;
            }
            if (_xferCompatibleIndex(tls, pDestIdx, pSrcIdx) != 0) {
                break;
            }
            goto _11;
_11:;
            pSrcIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.FpNext;
        }
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenRead, iSrc, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.Ftnum), iDbSrc);
        _sqlite3VdbeSetP4KeyInfo(tls, pParse, pSrcIdx);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenWrite, iDest, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pDestIdx)).Value.Ftnum), iDbDest);
        _sqlite3VdbeSetP4KeyInfo(tls, pParse, pDestIdx);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_BULKCSR);
        addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iSrc, 0);
        if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) != 0) {
            i = 0;
            for (;;) {
                if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.FnColumn))) {
                    break;
                }
                zColl = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.FazColl + (uintptr)i * 8));
                if (Xsqlite3_stricmp(tls, (uintptr)Ꮡ_sqlite3StrBINARY, zColl) != 0) {
                    break;
                }
                goto _12;
_12:;
                i = i + 1;
            }
            if (i == libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pSrcIdx)).Value.FnColumn)) {
                idxInsFlags = libc.Uint8FromInt32(libc.Int32FromInt32(OPFLAG_USESEEKRESULT) | libc.Int32FromInt32(OPFLAG_PREFORMAT));
                _sqlite3VdbeAddOp1(tls, v, (int32)OP_SeekEnd, iDest);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_RowCell, iDest, iSrc);
            }
        } else {
            if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSrc)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pDestIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
                idxInsFlags = libc.Uint8FromInt32((int32)idxInsFlags | libc.Int32FromInt32(OPFLAG_NCHANGE));
            }
        }
        if (libc.Int32FromUint8(idxInsFlags) != (libc.Int32FromInt32(OPFLAG_USESEEKRESULT) | libc.Int32FromInt32(OPFLAG_PREFORMAT))) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_RowData, iSrc, regData, (int32)1);
            if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0 && !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pDest)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pDestIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
                _codeWithoutRowidPreupdate(tls, pParse, pDest, iDest, regData);
            }
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_IdxInsert, iDest, regData);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32(libc.Int32FromUint8(idxInsFlags) | (int32)OPFLAG_APPEND));
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iSrc, addr1 + (int32)1);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Close, iSrc, 0);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Close, iDest, 0);
        goto _10;
_10:;
        pDestIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pDestIdx)).Value.FpNext;
    }
    if (emptySrcTest != 0) {
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, emptySrcTest);
    }
    _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regRowid);
    _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regData);
    if (emptyDestTest != 0) {
        _sqlite3AutoincrementEnd(tls, pParse);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Halt, SQLITE_OK, 0);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, emptyDestTest);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Close, iDest, 0);
        return 0;
    } else {
        return (int32)1;
    }
    return r;
}

} // end sqlite3_package
