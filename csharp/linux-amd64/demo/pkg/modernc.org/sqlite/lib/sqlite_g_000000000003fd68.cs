namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static void Xsqlite3_randomness(ж<libc.TLS> tls, int32 N, uintptr pBuf) {
    uintptr mutex = default;
    uintptr pVfs = default;
    uintptr zBuf = default;
    uintptr v1 = default;
    _ = mutex;
    _ = pVfs;
    _ = zBuf;
    _ = v1;
    zBuf = pBuf;
    if (Xsqlite3_initialize(tls) != 0) {
        return;
    }
    mutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_PRNG);
    Xsqlite3_mutex_enter(tls, mutex);
    if (N <= 0 || pBuf == (uintptr)0) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng))).Value = (uint32)0;
        Xsqlite3_mutex_leave(tls, mutex);
        return;
    }
    if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng)) == (uint32)0) {
        pVfs = Xsqlite3_vfs_find(tls, (uintptr)0);
        libc.Xmemcpy(tls, (uintptr)Ꮡ_sqlite3Prng, (uintptr)Ꮡ_chacha20_init, (uint64)16);
        if (pVfs == (uintptr)0) {
            libc.Xmemset(tls, (uintptr)Ꮡ_sqlite3Prng + 4 * 4, 0, (uint64)44);
        } else {
            _sqlite3OsRandomness(tls, pVfs, (int32)44, (uintptr)Ꮡ_sqlite3Prng + 4 * 4);
        }
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng + 15 * 4))).Value = (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng + 12 * 4))).Value;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng + 12 * 4))).Value = (uint32)0;
        _sqlite3Prng.Fn = (uint8)0;
    }
    while ((int32)1 != 0) {
        if (N <= libc.Int32FromUint8(_sqlite3Prng.Fn)) {
            libc.Xmemcpy(tls, zBuf, (uintptr)Ꮡ_sqlite3Prng + 64 + (uintptr)(libc.Int32FromUint8(_sqlite3Prng.Fn) - N), libc.Uint64FromInt32(N));
            v1 = (uintptr)Ꮡ_sqlite3Prng + 128;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) - N);
            break;
        }
        if (libc.Int32FromUint8(_sqlite3Prng.Fn) > 0) {
            libc.Xmemcpy(tls, zBuf, (uintptr)Ꮡ_sqlite3Prng + 64, (uint64)_sqlite3Prng.Fn);
            N = N - libc.Int32FromUint8(_sqlite3Prng.Fn);
            zBuf = zBuf + (uintptr)_sqlite3Prng.Fn;
        }
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng + 12 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr((uintptr)Ꮡ_sqlite3Prng + 12 * 4)) + 1;
        _chacha_block(tls, (uintptr)Ꮡ_sqlite3Prng + 64, (uintptr)Ꮡ_sqlite3Prng);
        _sqlite3Prng.Fn = (uint8)64;
    }
    Xsqlite3_mutex_leave(tls, mutex);
}

public static int32 /*r*/ Xsqlite3_set_clientdata(ж<libc.TLS> tls, uintptr db, uintptr zName, uintptr pData, uintptr __ccgo_fp_xDestructor) {
    Tsize_t n = default;
    uintptr p = default;
    uintptr pp = default;
    _ = n;
    _ = p;
    _ = pp;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    pp = db + 816;
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData;
    for (;;) {
        if (!(p != 0 && libc.Xstrcmp(tls, p + 24, zName) != 0)) {
            break;
        }
        pp = p;
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpNext;
    }
    if (p != 0) {
        if ((@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FxDestructor != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FxDestructor)(tls, (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpData);
        }
        if (pData == (uintptr)0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpNext;
            Xsqlite3_free(tls, p);
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
            return SQLITE_OK;
        }
    } else {
        if (pData == (uintptr)0) {
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
            return SQLITE_OK;
        } else {
            n = libc.Xstrlen(tls, zName);
            p = Xsqlite3_malloc64(tls, (uint64)((uint64)(libc.UintptrFromInt32(0) + 24) + (n + libc.Uint64FromInt32(1))));
            if (p == (uintptr)0) {
                if (__ccgo_fp_xDestructor != 0) {
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>(__ccgo_fp_xDestructor)(tls, pData);
                }
                Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
                return (int32)SQLITE_NOMEM;
            }
            libc.Xmemcpy(tls, p + 24, zName, n + (uint64)1);
            (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpNext = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData;
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData = p;
        }
    }
    (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpData = pData;
    (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FxDestructor = __ccgo_fp_xDestructor;
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return SQLITE_OK;
}

public static void Xsqlite3_str_append(ж<libc.TLS> tls, uintptr p, uintptr z, int32 N) {
    if ((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar + libc.Uint32FromInt32(N) >= (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnAlloc) {
        _enlargeAndAppend(tls, p, z, N);
    } else {
        if (N != 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 24))).Value += libc.Uint32FromInt32(N);
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText + (uintptr)((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar - libc.Uint32FromInt32(N)), z, libc.Uint64FromInt32(N));
        }
    }
}

public static uintptr /*r*/ Xsqlite3_value_dup(ж<libc.TLS> tls, uintptr pOrig) {
    uintptr pNew = default;
    uintptr v1 = default;
    _ = pNew;
    _ = v1;
    if (pOrig == (uintptr)0) {
        return (uintptr)0;
    }
    pNew = Xsqlite3_malloc(tls, (int32)56);
    if (pNew == (uintptr)0) {
        return (uintptr)0;
    }
    libc.Xmemset(tls, pNew, 0, (uint64)56);
    libc.Xmemcpy(tls, pNew, pOrig, (uint64)(libc.UintptrFromInt32(0) + 24));
    v1 = pNew + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(MEM_Dyn));
    (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pNew)).Value.Fdb = (uintptr)0;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pNew)).Value.Fflags) & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Blob))) != 0) {
        v1 = pNew + 20;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(MEM_Static) | libc.Int32FromInt32(MEM_Dyn)));
        v1 = pNew + 20;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Ephem));
        if (_sqlite3VdbeMemMakeWriteable(tls, pNew) != SQLITE_OK) {
            _sqlite3ValueFree(tls, pNew);
            pNew = (uintptr)0;
        }
    } else {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pNew)).Value.Fflags) & (int32)MEM_Null) != 0) {
            v1 = pNew + 20;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(MEM_Term) | libc.Int32FromInt32(MEM_Subtype)));
        }
    }
    return pNew;
}

public static int32 /*r*/ Xsqlite3changegroup_change_blob(ж<libc.TLS> tls, uintptr pGrp, int32 bNew, int32 iCol, uintptr pVal, int32 nVal) {
    Tsqlite3_int64 nByte = default;
    ref var pBuf = ref heap(new uintptr(), out var ᏑpBuf);
    int32 rc = default;
    int32 v1 = default;
    _ = nByte;
    _ = pBuf;
    _ = rc;
    _ = v1;
    nByte = (int64)((int32)1 + _sessionVarintLen(ref (tls).DerefOrNull(), nVal)) + (int64)nVal;
    rc = SQLITE_OK;
    pBuf = (uintptr)0;
    v1 = _checkChangeParams(tls, pGrp, bNew, iCol, nByte, @unsafe.Pointer.FromBox(ᏑpBuf));
    rc = v1;
    if (SQLITE_OK != v1) {
        return rc;
    }
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf))).Value = (uint8)SQLITE_BLOB;
    (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = (int32)1 + _sessionVarintPut(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf + 1), nVal);
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf, pVal, libc.Uint64FromInt32(nVal));
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += nVal;
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3changegroup_change_text(ж<libc.TLS> tls, uintptr pGrp, int32 bNew, int32 iCol, uintptr pVal, int32 nVal) {
    Tsqlite3_int64 nByte = default;
    Tsqlite3_int64 nText = default;
    ref var pBuf = ref heap(new uintptr(), out var ᏑpBuf);
    uintptr v3 = default;
    int32 rc = default;
    int32 v2 = default;
    uint64 v1 = default;
    _ = nByte;
    _ = nText;
    _ = pBuf;
    _ = rc;
    _ = v1;
    _ = v2;
    _ = v3;
    if (nVal >= 0) {
        v1 = libc.Uint64FromInt32(nVal);
    } else {
        v1 = libc.Xstrlen(tls, pVal);
    }
    nText = libc.Int64FromUint64(v1);
    nByte = (int64)((int32)1 + _sessionVarintLen(ref (tls).DerefOrNull(), (int32)nText)) + nText;
    rc = SQLITE_OK;
    pBuf = (uintptr)0;
    v2 = _checkChangeParams(tls, pGrp, bNew, iCol, nByte, @unsafe.Pointer.FromBox(ᏑpBuf));
    rc = v2;
    if (SQLITE_OK != v2) {
        return rc;
    }
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf))).Value = (uint8)SQLITE_TEXT;
    (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = (int32)1 + _sessionVarintPut(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf + 1), (int32)nText);
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf, pVal, libc.Uint64FromInt64(nText));
    v3 = pBuf + 8;
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = (int32)((int64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)) + nText);
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3rbu_create_vfs(ж<libc.TLS> tls, uintptr zName, uintptr zParent) {
    Tsize_t nByte = default;
    Tsize_t nName = default;
    uintptr pNew = default;
    uintptr pParent = default;
    uintptr zSpace = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = nByte;
    _ = nName;
    _ = pNew;
    _ = pParent;
    _ = rc;
    _ = zSpace;
    _ = v1;
    pNew = (uintptr)0;
    rc = SQLITE_OK;
    nName = libc.Xstrlen(tls, zName);
    nByte = (uint64)208 + nName + (uint64)1;
    pNew = Xsqlite3_malloc64(tls, nByte);
    if (pNew == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemset(tls, pNew, 0, nByte);
        pParent = Xsqlite3_vfs_find(tls, zParent);
        if (pParent == (uintptr)0) {
            rc = (int32)SQLITE_NOTFOUND;
        } else {
            libc.Xmemcpy(tls, pNew, (uintptr)Ꮡ_vfs_template, (uint64)168);
            (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.Fbase.FmxPathname = (@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pParent)).Value.FmxPathname;
            (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.Fbase.FszOsFile = libc.Int32FromUint64((uint64)104 + libc.Uint64FromInt32((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pParent)).Value.FszOsFile));
            (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.FpRealVfs = pParent;
            v1 = pNew + 1 * 208;
            zSpace = v1;
            (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.Fbase.FzName = v1;
            libc.Xmemcpy(tls, zSpace, zName, nName);
            (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.Fmutex = Xsqlite3_mutex_alloc(tls, (int32)SQLITE_MUTEX_RECURSIVE);
            if ((@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.Fmutex == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                rc = Xsqlite3_vfs_register(tls, pNew, 0);
            }
        }
        if (rc != SQLITE_OK) {
            Xsqlite3_mutex_free(tls, (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pNew)).Value.Fmutex);
            Xsqlite3_free(tls, pNew);
        }
    }
    return rc;
}

public static int32 /*r*/ Xsqlite3session_attach(ж<libc.TLS> tls, uintptr pSession, uintptr zName) {
    int32 nByte = default;
    int32 nName = default;
    int32 rc = default;
    uintptr pTab = default;
    uintptr ppTab = default;
    _ = nByte;
    _ = nName;
    _ = pTab;
    _ = ppTab;
    _ = rc;
    rc = SQLITE_OK;
    Xsqlite3_mutex_enter(tls, Xsqlite3_db_mutex(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fdb));
    if (!(zName != 0)) {
        (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FbAutoAttach = (int32)1;
    } else {
        nName = _sqlite3Strlen30(tls, zName);
        pTab = (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FpTable;
        for (;;) {
            if (!(pTab != 0)) {
                break;
            }
            if (0 == Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zName, nName + (int32)1)) {
                break;
            }
            goto _1;
_1:;
            pTab = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FpNext;
        }
        if (!(pTab != 0)) {
            nByte = libc.Int32FromUint64((uint64)88 + libc.Uint64FromInt32(nName) + (uint64)1);
            pTab = _sessionMalloc64(tls, pSession, (int64)nByte);
            if (!(pTab != 0)) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                libc.Xmemset(tls, pTab, 0, (uint64)88);
                (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName = pTab + 1 * 88;
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zName, libc.Uint64FromInt32(nName + (int32)1));
                ppTab = pSession + 88;
                for (;;) {
                    if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTab)) != 0)) {
                        break;
                    }
                    goto _2;
_2:;
                    ppTab = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTab));
                }
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppTab))).Value = pTab;
            }
        }
    }
    Xsqlite3_mutex_leave(tls, Xsqlite3_db_mutex(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fdb));
    return rc;
}

public static int32 /*r*/ Xsqlite3session_create(ж<libc.TLS> tls, uintptr db, uintptr zDb, uintptr ppSession) {
    int32 nDb = default;
    uintptr pNew = default;
    uintptr pOld = default;
    _ = nDb;
    _ = pNew;
    _ = pOld;
    nDb = _sqlite3Strlen30(tls, zDb);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppSession))).Value = (uintptr)0;
    pNew = Xsqlite3_malloc64(tls, (uint64)((uint64)136 + libc.Uint64FromInt32(nDb) + (uint64)1));
    if (!(pNew != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, pNew, 0, (uint64)136);
    (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pNew)).Value.Fdb = db;
    (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pNew)).Value.FzDb = pNew + 1 * 136;
    (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pNew)).Value.FbEnable = (int32)1;
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pNew)).Value.FzDb, zDb, libc.Uint64FromInt32(nDb + (int32)1));
    _sessionPreupdateHooks(ref (tls).DerefOrNull(), pNew);
    Xsqlite3_mutex_enter(tls, Xsqlite3_db_mutex(tls, db));
    pOld = Xsqlite3_preupdate_hook(tls, db, __ccgo_fp(_xPreUpdate), pNew);
    (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pNew)).Value.FpNext = pOld;
    Xsqlite3_mutex_leave(tls, Xsqlite3_db_mutex(tls, db));
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppSession))).Value = pNew;
    return SQLITE_OK;
}

internal static void _SortByDimension(ж<libc.TLS> tls, uintptr pRtree, uintptr aIdx, int32 nIdx, int32 iDim, uintptr aCell, uintptr aSpare) {
    uintptr aLeft = default;
    uintptr aRight = default;
    int32 iLeft = default;
    int32 iRight = default;
    int32 nLeft = default;
    int32 nRight = default;
    TRtreeDValue xleft1 = default;
    TRtreeDValue xleft2 = default;
    TRtreeDValue xright1 = default;
    TRtreeDValue xright2 = default;
    float64 v1 = default;
    float64 v2 = default;
    float64 v3 = default;
    float64 v4 = default;
    _ = aLeft;
    _ = aRight;
    _ = iLeft;
    _ = iRight;
    _ = nLeft;
    _ = nRight;
    _ = xleft1;
    _ = xleft2;
    _ = xright1;
    _ = xright2;
    _ = v1;
    _ = v2;
    _ = v3;
    _ = v4;
    if (nIdx > (int32)1) {
        iLeft = 0;
        iRight = 0;
        nLeft = nIdx / (int32)2;
        nRight = nIdx - nLeft;
        aLeft = aIdx;
        aRight = aIdx + (uintptr)nLeft * 4;
        _SortByDimension(tls, pRtree, aLeft, nLeft, iDim, aCell, aSpare);
        _SortByDimension(tls, pRtree, aRight, nRight, iDim, aCell, aSpare);
        libc.Xmemcpy(tls, aSpare, aLeft, (uint64)4 * libc.Uint64FromInt32(nLeft));
        aLeft = aSpare;
        while (iLeft < nLeft || iRight < nRight) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v1 = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aLeft + (uintptr)iLeft * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2) * 4)));
            } else {
                v1 = (float64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aLeft + (uintptr)iLeft * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2) * 4)));
            }
            xleft1 = v1;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v2 = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aLeft + (uintptr)iLeft * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2 + (int32)1) * 4)));
            } else {
                v2 = (float64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aLeft + (uintptr)iLeft * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2 + (int32)1) * 4)));
            }
            xleft2 = v2;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v3 = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRight + (uintptr)iRight * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2) * 4)));
            } else {
                v3 = (float64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRight + (uintptr)iRight * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2) * 4)));
            }
            xright1 = v3;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                v4 = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRight + (uintptr)iRight * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2 + (int32)1) * 4)));
            } else {
                v4 = (float64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRight + (uintptr)iRight * 4))) * 48 + 8 + (uintptr)(iDim * (int32)2 + (int32)1) * 4)));
            }
            xright2 = v4;
            if (iLeft != nLeft && (iRight == nRight || xleft1 < xright1 || xleft1 == xright1 && xleft2 < xright2)) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aIdx + (uintptr)(iLeft + iRight) * 4))).Value = (@unsafe.Pointer.To<int32>(__ccgo_ptr(aLeft + (uintptr)iLeft * 4))).Value;
                iLeft = iLeft + 1;
            } else {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aIdx + (uintptr)(iLeft + iRight) * 4))).Value = (@unsafe.Pointer.To<int32>(__ccgo_ptr(aRight + (uintptr)iRight * 4))).Value;
                iRight = iRight + 1;
            }
        }
    }
}

internal static uintptr /*r*/ _allocateCursor(ж<libc.TLS> tls, uintptr p, int32 iCur, int32 nField, Tu8 eCurType) {
    Ti64 nByte = default;
    uintptr pCx = default;
    uintptr pMem = default;
    uintptr v1 = default;
    _ = nByte;
    _ = pCx;
    _ = pMem;
    _ = v1;
    if (iCur > 0) {
        v1 = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem + (uintptr)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnMem - iCur) * 56;
    } else {
        v1 = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem;
    }
    pMem = v1;
    pCx = (uintptr)0;
    nByte = libc.Int64FromUint64((((uint64)(libc.UintptrFromInt32(0) + 120) + libc.Uint64FromInt32(7)) & libc.Uint64FromInt32(~libc.Int32FromInt32(7))) + libc.Uint64FromInt32(nField + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(8));
    if (libc.Int32FromUint8(eCurType) == CURTYPE_BTREE) {
        nByte = nByte + (int64)_sqlite3BtreeCursorSize(ref (tls).DerefOrNull());
    }
    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr + (uintptr)iCur * 8)) != 0) {
        _sqlite3VdbeFreeCursorNN(tls, p, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr + (uintptr)iCur * 8)));
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr + (uintptr)iCur * 8))).Value = (uintptr)0;
    }
    if ((int64)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc < nByte) {
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc > 0) {
            _sqlite3DbFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc);
        }
        v1 = _sqlite3DbMallocRaw(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, libc.Uint64FromInt64(nByte));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc = v1;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = v1;
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc == (uintptr)0) {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc = 0;
            return (uintptr)0;
        }
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc = (int32)nByte;
    }
    v1 = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc;
    pCx = v1;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FapCsr + (uintptr)iCur * 8))).Value = v1;
    libc.Xmemset(tls, pCx, 0, (uint64)(libc.UintptrFromInt32(0) + 40));
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCx)).Value.FeCurType = eCurType;
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCx)).Value.FnField = (int16)nField;
    (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCx)).Value.FaOffset = pCx + 120 + (uintptr)nField * 4;
    if (libc.Int32FromUint8(eCurType) == CURTYPE_BTREE) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCx + 48))).Value = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)((((uint64)(libc.UintptrFromInt32(0) + 120) + libc.Uint64FromInt32(7)) & libc.Uint64FromInt32(~libc.Int32FromInt32(7))) + libc.Uint64FromInt32(nField + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(8));
        _sqlite3BtreeCursorZero(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCx + 48)));
    }
    return pCx;
}

internal static int32 /*r*/ _allocateTempSpace(ж<libc.TLS> tls, uintptr pBt) {
    uintptr pCur = default;
    _ = pCur;
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace = _sqlite3PageMalloc(tls, libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize));
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace == (uintptr)0) {
        pCur = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpCursor;
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpCursor = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpNext;
        libc.Xmemset(tls, pCur, 0, (uint64)296);
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace, 0, (uint64)8);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pBt + 136))).Value += (uintptr)4;
    return SQLITE_OK;
}

internal static int32 /*r*/ _btreeOverwriteContent(ж<libc.TLS> tls, uintptr pPage, uintptr pDest, @unsafe.Pointer pX, int32 iOffset, int32 iAmt) {
    int32 i = default;
    int32 nData = default;
    int32 rc = default;
    int32 rc1 = default;
    int32 rc2 = default;
    _ = i;
    _ = nData;
    _ = rc;
    _ = rc1;
    _ = rc2;
    nData = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 36)) - iOffset;
    if (nData <= 0) {
        i = 0;
        for (;;) {
            if (!(i < iAmt && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDest + (uintptr)i))) == 0)) {
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (i < iAmt) {
            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
            if (rc != 0) {
                return rc;
            }
            libc.Xmemset(tls, pDest + (uintptr)i, 0, libc.Uint64FromInt32(iAmt - i));
        }
    } else {
        if (nData < iAmt) {
            rc1 = _btreeOverwriteContent(tls, pPage, pDest + (uintptr)nData, pX, iOffset + nData, iAmt - nData);
            if (rc1 != 0) {
                return rc1;
            }
            iAmt = nData;
        }
        if (libc.Xmemcmp(tls, pDest, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 16)) + (uintptr)iOffset, libc.Uint64FromInt32(iAmt)) != 0) {
            rc2 = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
            if (rc2 != 0) {
                return rc2;
            }
            libc.Xmemmove(tls, pDest, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pX, 16)) + (uintptr)iOffset, libc.Uint64FromInt32(iAmt));
        }
    }
    return SQLITE_OK;
}

internal static void _codeAttach(ж<libc.TLS> tls, uintptr pParse, int32 type1, uintptr pFunc, uintptr pAuthArg, uintptr pFilename, uintptr pDbname, uintptr pKey) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr v = default;
        uintptr zAuthArg = default;
        int32 rc = default;
        int32 regArgs = default;
        TNameContext _ᴛ1 = new();
        _ = db;
        _ = rc;
        _ = regArgs;
        _ = v;
        _ = zAuthArg;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if (SQLITE_OK != _sqlite3ReadSchema(tls, pParse)) {
            goto attach_end;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            goto attach_end;
        }
        libc.Xmemset(tls, bp, 0, (uint64)56);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        if (SQLITE_OK != _resolveAttachExpr(tls, bp, pFilename) || SQLITE_OK != _resolveAttachExpr(tls, bp, pDbname) || SQLITE_OK != _resolveAttachExpr(tls, bp, pKey)) {
            goto attach_end;
        }
        if (pAuthArg != 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pAuthArg)).Value.Fop) == (int32)TK_STRING) {
                zAuthArg = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pAuthArg + 8));
            } else {
                zAuthArg = (uintptr)0;
            }
            rc = _sqlite3AuthCheck(tls, pParse, type1, zAuthArg, (uintptr)0, (uintptr)0);
            if (rc != SQLITE_OK) {
                goto attach_end;
            }
        }
        v = _sqlite3GetVdbe(tls, pParse);
        regArgs = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, (int32)4);
        _sqlite3ExprCode(tls, pParse, pFilename, regArgs);
        _sqlite3ExprCode(tls, pParse, pDbname, regArgs + (int32)1);
        _sqlite3ExprCode(tls, pParse, pKey, regArgs + (int32)2);
        if (v != 0) {
            _sqlite3VdbeAddFunctionCall(tls, pParse, 0, regArgs + (int32)3 - (int32)(@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FnArg, regArgs + (int32)3, (int32)(@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pFunc)).Value.FnArg, pFunc, 0);
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_Expire, libc.BoolInt32(type1 == (int32)SQLITE_ATTACH));
        }
        goto attach_end;
attach_end:;
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pFilename);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pDbname);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pKey);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _convertCompoundSelectToSubquery(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr a = default;
        uintptr db = default;
        uintptr pNew = default;
        uintptr pNewSrc = default;
        uintptr pParse = default;
        uintptr pX = default;
        int32 i = default;
        TToken _ᴛ1 = default;
        _ = a;
        _ = db;
        _ = i;
        _ = pNew;
        _ = pNewSrc;
        _ = pParse;
        _ = pX;
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior == (uintptr)0) {
            r = WRC_Continue; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy == (uintptr)0) {
            r = WRC_Continue; goto ᒐdone;
        }
        pX = p;
        for (;;) {
            if (!(pX != 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_ALL || libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_SELECT))) {
                break;
            }
            goto _1;
_1:;
            pX = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FpPrior;
        }
        if (pX == (uintptr)0) {
            r = WRC_Continue; goto ᒐdone;
        }
        a = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy + 8;
        if (~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)(a + 24)) != 0) {
            r = WRC_Continue; goto ᒐdone;
        }
        i = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy)).Value.FnExpr - (int32)1;
        for (;;) {
            if (!(i >= 0)) {
                break;
            }
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExprList_item>(__ccgo_ptr(a + (uintptr)i * 32))).Value.FpExpr)).Value.Fflags & (uint32)EP_Collate) != 0) {
                break;
            }
            goto _2;
_2:;
            i = i - 1;
        }
        if (i < 0) {
            r = WRC_Continue; goto ᒐdone;
        }
        pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pNew = _sqlite3DbMallocZero(tls, db, (uint64)120);
        if (pNew == (uintptr)0) {
            r = (int32)WRC_Abort; goto ᒐdone;
        }
        libc.Xmemset(tls, bp, 0, (uint64)16);
        pNewSrc = _sqlite3SrcListAppendFromTerm(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0, __ccgo_ptr(bp), pNew, (uintptr)0);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pNewSrc);
            r = (int32)WRC_Abort; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TSelect>(__ccgo_ptr(pNew))).Value = (@unsafe.Pointer.To<TSelect>(__ccgo_ptr(p))).Value;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc = pNewSrc;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList = _sqlite3ExprListAppend(tls, pParse, (uintptr)0, _sqlite3Expr(tls, db, (int32)TK_ASTERISK, (uintptr)0));
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.Fop = (uint8)TK_SELECT;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpGroupBy = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpHaving = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpOrderBy = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpNext = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWith = (uintptr)0;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWinDefn = (uintptr)0;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(SF_Compound);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value |= (uint32)SF_Converted;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpPrior)).Value.FpNext = pNew;
        (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pNew)).Value.FpLimit = (uintptr)0;
        r = WRC_Continue;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _copyNodeContent(ж<libc.TLS> tls, uintptr pFrom, uintptr pTo, @unsafe.Pointer pRC) {
    uintptr aFrom = default;
    uintptr aTo = default;
    uintptr pBt = default;
    int32 iData = default;
    int32 iFromHdr = default;
    int32 iToHdr = default;
    int32 rc = default;
    int32 v1 = default;
    _ = aFrom;
    _ = aTo;
    _ = iData;
    _ = iFromHdr;
    _ = iToHdr;
    _ = pBt;
    _ = rc;
    _ = v1;
    if (~@unsafe.Pointer.To<int32>(pRC) == SQLITE_OK) {
        pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pFrom)).Value.FpBt;
        aFrom = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pFrom)).Value.FaData;
        aTo = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTo)).Value.FaData;
        iFromHdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pFrom)).Value.FhdrOffset);
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTo)).Value.Fpgno == (uint32)1) {
            v1 = (int32)100;
        } else {
            v1 = 0;
        }
        iToHdr = v1;
        iData = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aFrom + (uintptr)(iFromHdr + (int32)5)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aFrom + (uintptr)(iFromHdr + (int32)5) + 1)));
        libc.Xmemcpy(tls, aTo + (uintptr)iData, aFrom + (uintptr)iData, (uint64)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - libc.Uint32FromInt32(iData)));
        libc.Xmemcpy(tls, aTo + (uintptr)iToHdr, aFrom + (uintptr)iFromHdr, libc.Uint64FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pFrom)).Value.FcellOffset) + (int32)2 * libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pFrom)).Value.FnCell)));
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pTo)).Value.FisInit = (uint8)0;
        rc = _btreeInitPage(tls, pTo);
        if (rc == SQLITE_OK) {
            rc = _btreeComputeFreeSpace(tls, pTo);
        }
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<int32>(pRC)).Value = rc;
            return;
        }
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
            (@unsafe.Pointer.To<int32>(pRC)).Value = _setChildPtrmaps(tls, pTo);
        }
    }
}

internal static int32 /*r*/ _copyPayload(ж<libc.TLS> tls, uintptr pPayload, uintptr pBuf, int32 nByte, int32 eOp, uintptr pDbPage) {
    int32 rc = default;
    _ = rc;
    if (eOp != 0) {
        rc = _sqlite3PagerWrite(tls, pDbPage);
        if (rc != SQLITE_OK) {
            return rc;
        }
        libc.Xmemcpy(tls, pPayload, pBuf, libc.Uint64FromInt32(nByte));
    } else {
        libc.Xmemcpy(tls, pBuf, pPayload, libc.Uint64FromInt32(nByte));
    }
    return SQLITE_OK;
}

internal static uintptr /*r*/ _createAggContext(ж<libc.TLS> tls, uintptr p, int32 nByte) {
    uintptr pMem = default;
    _ = pMem;
    pMem = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpMem;
    if (nByte <= 0) {
        _sqlite3VdbeMemSetNull(tls, pMem);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = (uintptr)0;
    } else {
        _sqlite3VdbeMemClearAndResize(tls, pMem, nByte);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Agg;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)pMem)).Value = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpFunc;
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz != 0) {
            libc.Xmemset(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, 0, libc.Uint64FromInt32(nByte));
        }
    }
    return (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz;
}

internal static uintptr /*r*/ _dbReallocFinish(ж<libc.TLS> tls, uintptr db, uintptr p, Tu64 n) {
    uintptr pNew = default;
    _ = pNew;
    pNew = (uintptr)0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed) == 0) {
        if (_isLookaside(ref (tls).DerefOrNull(), db, p) != 0) {
            pNew = _sqlite3DbMallocRawNN(tls, db, n);
            if (pNew != 0) {
                libc.Xmemcpy(tls, pNew, p, libc.Uint64FromInt32(_lookasideMallocSize(ref (tls).DerefOrNull(), db, p)));
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, p);
            }
        } else {
            pNew = _sqlite3Realloc(tls, p, n);
            if (!(pNew != 0)) {
                _sqlite3OomFault(tls, db);
            }
        }
    }
    return pNew;
}

internal static int32 /*r*/ _defragmentPage(ж<libc.TLS> tls, uintptr pPage, int32 nMaxFrag) {
    int32 cbrk = default;
    int32 cellOffset = default;
    int32 hdr = default;
    int32 i = default;
    int32 iCellFirst = default;
    int32 iCellLast = default;
    int32 iCellStart = default;
    int32 iFree = default;
    int32 iFree2 = default;
    int32 nCell = default;
    int32 pc = default;
    int32 size = default;
    int32 sz = default;
    int32 sz2 = default;
    int32 top = default;
    int32 usableSize = default;
    uintptr data = default;
    uintptr pAddr = default;
    uintptr pAddr1 = default;
    uintptr pEnd = default;
    uintptr src = default;
    uintptr temp = default;
    _ = cbrk;
    _ = cellOffset;
    _ = data;
    _ = hdr;
    _ = i;
    _ = iCellFirst;
    _ = iCellLast;
    _ = iCellStart;
    _ = iFree;
    _ = iFree2;
    _ = nCell;
    _ = pAddr;
    _ = pAddr1;
    _ = pEnd;
    _ = pc;
    _ = size;
    _ = src;
    _ = sz;
    _ = sz2;
    _ = temp;
    _ = top;
    _ = usableSize;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
    hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset);
    cellOffset = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FcellOffset);
    nCell = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
    iCellFirst = cellOffset + (int32)2 * nCell;
    usableSize = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize);
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7)))) <= nMaxFrag) {
        iFree = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)1)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)1) + 1)));
        if (iFree > usableSize - (int32)4) {
            return _sqlite3CorruptError(tls, (int32)74822);
        }
        if (iFree != 0) {
            iFree2 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iFree))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iFree + 1)));
            if (iFree2 > usableSize - (int32)4) {
                return _sqlite3CorruptError(tls, (int32)74825);
            }
            if (0 == iFree2 || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iFree2))) == 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFree2 + (int32)1)))) == 0) {
                pEnd = data + (uintptr)(cellOffset + nCell * (int32)2);
                sz2 = 0;
                sz = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFree + (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFree + (int32)2) + 1)));
                top = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5) + 1)));
                if (top >= iFree) {
                    return _sqlite3CorruptError(tls, (int32)74833);
                }
                if (iFree2 != 0) {
                    if (iFree + sz > iFree2) {
                        return _sqlite3CorruptError(tls, (int32)74836);
                    }
                    sz2 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFree2 + (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFree2 + (int32)2) + 1)));
                    if (iFree2 + sz2 > usableSize) {
                        return _sqlite3CorruptError(tls, (int32)74838);
                    }
                    libc.Xmemmove(tls, data + (uintptr)(iFree + sz + sz2), data + (uintptr)(iFree + sz), libc.Uint64FromInt32(iFree2 - (iFree + sz)));
                    sz = sz + sz2;
                } else {
                    if (iFree + sz > usableSize) {
                        return _sqlite3CorruptError(tls, (int32)74842);
                    }
                }
                cbrk = top + sz;
                libc.Xmemmove(tls, data + (uintptr)cbrk, data + (uintptr)top, libc.Uint64FromInt32(iFree - top));
                pAddr = data + (uintptr)cellOffset;
                for (;;) {
                    if (!(pAddr < pEnd)) {
                        break;
                    }
                    pc = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr + 1)));
                    if (pc < iFree) {
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr))).Value = libc.Uint8FromInt32((pc + sz).Rsh((int64)(libc.Int32FromInt32(8))));
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr + 1))).Value = libc.Uint8FromInt32(pc + sz);
                    } else {
                        if (pc < iFree2) {
                            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr))).Value = libc.Uint8FromInt32((pc + sz2).Rsh((int64)(libc.Int32FromInt32(8))));
                            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr + 1))).Value = libc.Uint8FromInt32(pc + sz2);
                        }
                    }
                    goto _1;
_1:;
                    pAddr = pAddr + (uintptr)2;
                }
                goto defragment_out;
            }
        }
    }
    cbrk = usableSize;
    iCellLast = usableSize - (int32)4;
    iCellStart = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5) + 1)));
    if (nCell > 0) {
        temp = _sqlite3PagerTempSpace(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FpPager);
        libc.Xmemcpy(tls, temp, data, libc.Uint64FromInt32(usableSize));
        src = temp;
        i = 0;
        for (;;) {
            if (!(i < nCell)) {
                break;
            }
            pAddr1 = data + (uintptr)(cellOffset + i * (int32)2);
            pc = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr1))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr1 + 1)));
            if (pc > iCellLast) {
                return _sqlite3CorruptError(tls, (int32)74875);
            }
            size = libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxCellSize)(tls, pPage, src + (uintptr)pc));
            cbrk = cbrk - size;
            if (cbrk < iCellStart || pc + size > usableSize) {
                return _sqlite3CorruptError(tls, (int32)74881);
            }
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr1))).Value = libc.Uint8FromInt32(cbrk.Rsh((int64)(libc.Int32FromInt32(8))));
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pAddr1 + 1))).Value = libc.Uint8FromInt32(cbrk);
            libc.Xmemcpy(tls, data + (uintptr)cbrk, src + (uintptr)pc, libc.Uint64FromInt32(size));
            goto _2;
_2:;
            i = i + 1;
        }
    }
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7)))).Value = (uint8)0;
    goto defragment_out;
defragment_out:;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7)))) + cbrk - iCellFirst != (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree) {
        return _sqlite3CorruptError(tls, (int32)74895);
    }
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5)))).Value = libc.Uint8FromInt32(cbrk.Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5) + 1))).Value = libc.Uint8FromInt32(cbrk);
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)1)))).Value = (uint8)0;
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)2)))).Value = (uint8)0;
    libc.Xmemset(tls, data + (uintptr)iCellFirst, 0, libc.Uint64FromInt32(cbrk - iCellFirst));
    return SQLITE_OK;
}

internal static void _dropCell(ж<libc.TLS> tls, uintptr pPage, int32 idx, int32 sz, @unsafe.Pointer pRC) {
    uintptr data = default;
    uintptr ptr = default;
    int32 hdr = default;
    int32 rc = default;
    Tu32 pc = default;
    _ = data;
    _ = hdr;
    _ = pc;
    _ = ptr;
    _ = rc;
    if (~@unsafe.Pointer.To<int32>(pRC) != 0) {
        return;
    }
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
    ptr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * idx);
    pc = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(ptr))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(ptr + 1))));
    hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset);
    if (pc + libc.Uint32FromInt32(sz) > (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize) {
        (@unsafe.Pointer.To<int32>(pRC)).Value = _sqlite3CorruptError(tls, (int32)80467);
        return;
    }
    rc = _freeSpace(tls, pPage, libc.Int32FromUint32(pc), sz);
    if (rc != 0) {
        (@unsafe.Pointer.To<int32>(pRC)).Value = rc;
        return;
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell = (Tu16)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell - 1);
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) == 0) {
        libc.Xmemset(tls, data + (uintptr)(hdr + (int32)1), 0, (uint64)4);
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)7)))).Value = (uint8)0;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5)))).Value = (uint8)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize >> (int)(libc.Int32FromInt32(8)));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)5) + 1))).Value = (uint8)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize;
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize - (uint32)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset - (uint32)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize - (uint32)8);
    } else {
        libc.Xmemmove(tls, ptr, ptr + (uintptr)2, libc.Uint64FromInt32((int32)2 * (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - idx)));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)3)))).Value = libc.Uint8FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell).Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(hdr + (int32)3) + 1))).Value = (uint8)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pPage + 20))).Value += (int32)2;
    }
}

internal static int32 /*r*/ _editPage(ж<libc.TLS> tls, uintptr pPg, int32 iOld, int32 iNew, int32 nNew, @unsafe.Pointer pCArray) {
    uintptr aData = default;
    uintptr pBegin = default;
    uintptr pCellptr = default;
    ref var pData = ref heap(new uintptr(), out var ᏑpData);
    int32 hdr = default;
    int32 i = default;
    int32 iCell = default;
    int32 iNewEnd = default;
    int32 iOldEnd = default;
    int32 nAdd = default;
    int32 nCell = default;
    int32 nShift = default;
    int32 nTail = default;
    int32 v1 = default;
    _ = aData;
    _ = hdr;
    _ = i;
    _ = iCell;
    _ = iNewEnd;
    _ = iOldEnd;
    _ = nAdd;
    _ = nCell;
    _ = nShift;
    _ = nTail;
    _ = pBegin;
    _ = pCellptr;
    _ = pData;
    _ = v1;
    aData = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaData;
    hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FhdrOffset);
    pBegin = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx + (uintptr)(nNew * (int32)2);
    nCell = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell);
    iOldEnd = iOld + libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell) + libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnOverflow);
    iNewEnd = iNew + nNew;
    if (iOld < iNew) {
        nShift = _pageFreeArray(tls, pPg, iOld, iNew - iOld, pCArray);
        if (nShift > nCell) {
            return _sqlite3CorruptError(tls, (int32)81077);
        }
        libc.Xmemmove(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx + (uintptr)(nShift * (int32)2), libc.Uint64FromInt32(nCell * (int32)2));
        nCell = nCell - nShift;
    }
    if (iNewEnd < iOldEnd) {
        nTail = _pageFreeArray(tls, pPg, iNewEnd, iOldEnd - iNewEnd, pCArray);
        nCell = nCell - nTail;
    }
    pData = aData + (uintptr)(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5) + 1))));
    if (pData < pBegin) {
        goto editpage_fail;
    }
    if (pData > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaDataEnd) {
        goto editpage_fail;
    }
    if (iNew < iOld) {
        if (nNew < iOld - iNew) {
            v1 = nNew;
        } else {
            v1 = iOld - iNew;
        }
        nAdd = v1;
        pCellptr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx;
        libc.Xmemmove(tls, pCellptr + (uintptr)(nAdd * (int32)2), pCellptr, libc.Uint64FromInt32(nCell * (int32)2));
        if (_pageInsertArray(tls, pPg, pBegin, @unsafe.Pointer.FromBox(ᏑpData), pCellptr, iNew, nAdd, pCArray) != 0) {
            goto editpage_fail;
        }
        nCell = nCell + nAdd;
    }
    i = 0;
    for (;;) {
        if (!(i < libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnOverflow))) {
            break;
        }
        iCell = iOld + libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(pPg + 28 + (uintptr)i * 2))) - iNew;
        if (iCell >= 0 && iCell < nNew) {
            pCellptr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx + (uintptr)(iCell * (int32)2);
            if (nCell > iCell) {
                libc.Xmemmove(tls, pCellptr + 2, pCellptr, libc.Uint64FromInt32((nCell - iCell) * (int32)2));
            }
            nCell = nCell + 1;
            _cachedCellSize(tls, pCArray, iCell + iNew);
            if (_pageInsertArray(tls, pPg, pBegin, @unsafe.Pointer.FromBox(ᏑpData), pCellptr, iCell + iNew, (int32)1, pCArray) != 0) {
                goto editpage_fail;
            }
        }
        goto _2;
_2:;
        i = i + 1;
    }
    pCellptr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx + (uintptr)(nCell * (int32)2);
    if (_pageInsertArray(tls, pPg, pBegin, @unsafe.Pointer.FromBox(ᏑpData), pCellptr, iNew + nCell, nNew - nCell, pCArray) != 0) {
        goto editpage_fail;
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell = libc.Uint16FromInt32(nNew);
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnOverflow = (uint8)0;
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)3)))).Value = libc.Uint8FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell).Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)3) + 1))).Value = (uint8)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell;
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5)))).Value = libc.Uint8FromInt64(((int64)pData - (int64)aData).Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5) + 1))).Value = libc.Uint8FromInt64((int64)pData - (int64)aData);
    return SQLITE_OK;
    goto editpage_fail;
editpage_fail:;
    if (nNew < (int32)1) {
        return _sqlite3CorruptError(tls, (int32)81155);
    }
    _populateCellCache(tls, pCArray, iNew, nNew);
    return _rebuildPage(tls, pCArray, iNew, nNew, pPg);
}

internal static void _enlargeAndAppend(ж<libc.TLS> tls, uintptr p, uintptr z, int32 N) {
    N = _sqlite3StrAccumEnlarge(tls, p, (int64)N);
    if (N > 0) {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FzText + (uintptr)(@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar, z, libc.Uint64FromInt32(N));
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 24))).Value += libc.Uint32FromInt32(N);
    }
}

internal static void _exprCodeBetween(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 dest, uintptr __ccgo_fp_xJump, int32 jumpIfNull) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(224);
        defer(tls.Free, (nint)224, ref ᒐ);
        uintptr db = default;
        uintptr pDel = default;
        ref var regFree1 = ref heap(new int32(), out var ᏑregFree1);
        TExpr _ᴛ1 = new();
        TExpr _ᴛ2 = new();
        TExpr _ᴛ3 = new();
        _ = db;
        _ = pDel;
        _ = regFree1;
        regFree1 = 0;
        pDel = (uintptr)0;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        libc.Xmemset(tls, bp + 72, 0, (uint64)72);
        libc.Xmemset(tls, bp + 144, 0, (uint64)72);
        libc.Xmemset(tls, bp, 0, (uint64)72);
        pDel = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, 0);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed) == 0) {
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp))).Value.Fop = (uint8)TK_AND;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp))).Value.FpLeft = bp + 72;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp))).Value.FpRight = bp + 144;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp + 72))).Value.Fop = (uint8)TK_GE;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp + 72))).Value.FpLeft = pDel;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp + 72))).Value.FpRight = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) + 8))).Value.FpExpr;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp + 144))).Value.Fop = (uint8)TK_LE;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp + 144))).Value.FpLeft = pDel;
            (@unsafe.Pointer.To<TExpr>(__ccgo_ptr(bp + 144))).Value.FpRight = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) + 8 + 1 * 32))).Value.FpExpr;
            _sqlite3ExprToRegister(ref (tls).DerefOrNull(), pDel, _exprCodeVector(tls, pParse, pDel, @unsafe.Pointer.FromPinnedBox(ᏑregFree1)));
            if (__ccgo_fp_xJump != 0) {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xJump)(tls, pParse, bp, dest, jumpIfNull);
            } else {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pDel + 4))).Value |= (uint32)EP_OuterON;
                _sqlite3ExprCodeTarget(tls, pParse, bp, dest);
            }
            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regFree1);
        }
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pDel);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _exprDup(ж<libc.TLS> tls, uintptr db, uintptr p, int32 dupFlags, uintptr pEdupBuf) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 nAlloc = default;
        int32 nNewSize = default;
        int32 nToken = default;
        int32 v2 = default;
        Tu32 nSize = default;
        Tu32 staticFlag = default;
        uint32 nStructSize = default;
        uintptr pNew = default;
        uintptr zToken = default;
        uintptr v1 = default;
        TEdupBuf _ᴛ1 = default;
        _ = nAlloc;
        _ = nNewSize;
        _ = nSize;
        _ = nStructSize;
        _ = nToken;
        _ = pNew;
        _ = staticFlag;
        _ = zToken;
        _ = v1;
        _ = v2;
        nToken = -(int32)1;
        if (pEdupBuf != 0) {
            (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc = (@unsafe.Pointer.To<TEdupBuf>((@unsafe.Pointer)pEdupBuf)).Value.FzAlloc;
            staticFlag = (uint32)EP_Static;
        } else {
            if (dupFlags != 0) {
                nAlloc = _dupedExprSize(tls, p);
            } else {
                if (!((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_IntValue))) != libc.Uint32FromInt32(0)) && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)) != 0) {
                    nToken = libc.Int32FromUint64((libcꓸTsize_t)(libc.Xstrlen(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8))) & (uint64)0x3fffffff) + (uint64)1);
                    nAlloc = libc.Int32FromUint64((libc.Uint64FromInt64(72) + libc.Uint64FromInt32(nToken) + libc.Uint64FromInt32(7)) & libc.Uint64FromInt32(~libc.Int32FromInt32(7)));
                } else {
                    nToken = 0;
                    nAlloc = libc.Int32FromUint64((libc.Uint64FromInt64(72) + libc.Uint64FromInt32(7)) & libc.Uint64FromInt32(~libc.Int32FromInt32(7)));
                }
            }
            (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc = _sqlite3DbMallocRawNN(tls, db, libc.Uint64FromInt32(nAlloc));
            staticFlag = (uint32)0;
        }
        pNew = (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc;
        if (pNew != 0) {
            nStructSize = libc.Uint32FromInt32(_dupedExprStructSize(ref (tls).DerefOrNull(), p, dupFlags));
            nNewSize = libc.Int32FromUint32(nStructSize & (uint32)0xfff);
            if (nToken < 0) {
                if (!((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_IntValue))) != libc.Uint32FromInt32(0)) && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)) != 0) {
                    nToken = _sqlite3Strlen30(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8))) + (int32)1;
                } else {
                    nToken = 0;
                }
            }
            if (dupFlags != 0) {
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc, p, libc.Uint64FromInt32(nNewSize));
            } else {
                nSize = libc.Uint32FromInt32(_exprStructSize(ref (tls).DerefOrNull(), p));
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc, p, (uint64)nSize);
                if ((uint64)nSize < (uint64)72) {
                    libc.Xmemset(tls, (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc + (uintptr)nSize, 0, (uint64)72 - (uint64)nSize);
                }
                nNewSize = (int32)72;
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 4))).Value &= libc.Uint32FromInt32(~(libc.Int32FromInt32(EP_Reduced) | libc.Int32FromInt32(EP_TokenOnly) | libc.Int32FromInt32(EP_Static)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 4))).Value |= (nStructSize & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Reduced) | libc.Int32FromInt32(EP_TokenOnly)));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 4))).Value |= staticFlag;
            if (dupFlags != 0) {
            }
            if (nToken > 0) {
                v1 = (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc + (uintptr)nNewSize;
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 8))).Value = v1;
                zToken = v1;
                libc.Xmemcpy(tls, zToken, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)), libc.Uint64FromInt32(nToken));
                nNewSize = nNewSize + nToken;
            }
            (@unsafe.Pointer.To<TEdupBuf>(__ccgo_ptr(bp))).Value.FzAlloc += (uintptr)((nNewSize + libc.Int32FromInt32(7)) & ~libc.Int32FromInt32(7));
            if ((Tu32)(((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags | (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.Fflags)) & libc.Uint32FromInt32(libc.Int32FromInt32(EP_TokenOnly) | libc.Int32FromInt32(EP_Leaf))) == (uint32)0) {
                if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
                    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 32))).Value = _sqlite3SelectDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)), dupFlags);
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) != (int32)TK_ORDER) {
                        v2 = dupFlags;
                    } else {
                        v2 = 0;
                    }
                    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 32))).Value = _sqlite3ExprListDup(tls, db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 32)), v2);
                }
                if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc))) != (uint32)0) {
                    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 64))).Value = _sqlite3WindowDup(tls, db, pNew, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64)));
                }
                if (dupFlags != 0) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_SELECT_COLUMN) {
                        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FpLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
                    } else {
                        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft != 0) {
                            v1 = _exprDup(tls, db, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft, (int32)EXPRDUP_REDUCE, bp);
                        } else {
                            v1 = (uintptr)0;
                        }
                        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FpLeft = v1;
                    }
                    if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight != 0) {
                        v1 = _exprDup(tls, db, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight, (int32)EXPRDUP_REDUCE, bp);
                    } else {
                        v1 = (uintptr)0;
                    }
                    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FpRight = v1;
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_SELECT_COLUMN) {
                        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FpLeft = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
                    } else {
                        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FpLeft = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft, 0);
                    }
                    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FpRight = _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight, 0);
                }
            }
        }
        if (pEdupBuf != 0) {
            libc.Xmemcpy(tls, pEdupBuf, bp, (uint64)8);
        }
        r = pNew;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _exprIsDeterministic(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode = (uint16)1;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_exprNodeIsDeterministic);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkFail);
        _sqlite3WalkExpr(tls, bp, p);
        r = libc.Int32FromUint16((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _exprVectorRegister(ж<libc.TLS> tls, uintptr pParse, uintptr pVector, int32 iField, int32 regSelect, uintptr pTmp, @unsafe.Pointer ppExpr, @unsafe.Pointer pRegFree) {
    Tu8 op = default;
    _ = op;
    op = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pVector)).Value.Fop;
    if (libc.Int32FromUint8(op) == (int32)TK_REGISTER) {
        (@unsafe.Pointer.To<uintptr>(ppExpr)).Value = _sqlite3VectorFieldSubexpr(ref (tls).DerefOrNull(), pVector, iField);
        return (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pVector)).Value.FiTable + iField;
    }
    if (libc.Int32FromUint8(op) == (int32)TK_SELECT) {
        libc.Xmemset(tls, pTmp, 0, (uint64)72);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTmp)).Value.Fop = (uint8)TK_SELECT_COLUMN;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTmp)).Value.FpLeft = pVector;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTmp)).Value.FiColumn = (int16)iField;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTmp)).Value.FiTable = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pVector + 32))))).Value.FpEList)).Value.FnExpr;
        (@unsafe.Pointer.To<uintptr>(ppExpr)).Value = pTmp;
        return regSelect + iField;
    }
    if (libc.Int32FromUint8(op) == (int32)TK_VECTOR) {
        (@unsafe.Pointer.To<uintptr>(ppExpr)).Value = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pVector + 32)) + 8 + (uintptr)iField * 32))).Value.FpExpr;
        return _sqlite3ExprCodeTemp(tls, pParse, ~@unsafe.Pointer.To<uintptr>(ppExpr), pRegFree);
    }
    return 0;
}

internal static Tu64 /*r*/ _filterHash(ж<libc.TLS> tls, uintptr aMem, uintptr pOp) {
    Tu64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu64 h = default;
        int32 i = default;
        int32 mx = default;
        int32 n = default;
        int32 v2 = default;
        uintptr p = default;
        Tu64 _ᴛ1 = default;
        Tu64 _ᴛ2 = default;
        _ = h;
        _ = i;
        _ = mx;
        _ = n;
        _ = p;
        _ = v2;
        h = (uint64)0;
        i = (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp3;
        mx = i + (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp4.Fi;
        for (;;) {
            if (!(i < mx)) {
                break;
            }
            p = aMem + (uintptr)i * 56;
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
                h = h + libc.Uint64FromInt64(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)p));
            } else {
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Real) != 0) {
                    h = h + libc.Uint64FromInt64(_sqlite3VdbeIntValue(ref (tls).DerefOrNull(), p));
                } else {
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Str) != 0) {
                        h = h + libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn);
                        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn >= libc.Int32FromInt64(8)) {
                            libc.Xmemcpy(tls, bp, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fz, (uint64)8);
                            h = h + ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp));
                            libc.Xmemcpy(tls, bp, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fz + (uintptr)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn - (uintptr)8, (uint64)8);
                            h = h + ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp));
                        } else {
                            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (uint64)0;
                            libc.Xmemcpy(tls, bp, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fz, libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn));
                            h = h + ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp));
                        }
                    } else {
                        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Blob) != 0) {
                            n = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn;
                            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8))).Value = (uint64)0;
                            if (n != 0) {
                                if (n < libc.Int32FromInt64(8)) {
                                    v2 = n;
                                } else {
                                    v2 = libc.Int32FromInt64(8);
                                }
                                libc.Xmemcpy(tls, bp + 8, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fz, libc.Uint64FromInt32(v2));
                                h = h + ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8));
                            }
                            h = h + libc.Uint64FromInt32(n);
                            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Zero) != 0) {
                                h = h + libc.Uint64FromInt32(~@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>());
                            }
                        }
                    }
                }
            }
            goto _1;
_1:;
            i = i + 1;
        }
        r = h;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _findCollSeqEntry(ж<libc.TLS> tls, uintptr db, uintptr zName, int32 create) {
    int32 nName = default;
    uintptr pColl = default;
    uintptr pDel = default;
    _ = nName;
    _ = pColl;
    _ = pDel;
    pColl = _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)(db + 656), zName);
    if ((uintptr)0 == pColl && create != 0) {
        nName = _sqlite3Strlen30(tls, zName) + (int32)1;
        pColl = _sqlite3DbMallocZero(tls, db, (uint64)(libc.Uint64FromInt32(3) * libc.Uint64FromInt64(40) + libc.Uint64FromInt32(nName)));
        if (pColl != 0) {
            pDel = (uintptr)0;
            (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl))).Value.FzName = pColl + 3 * 40;
            (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl))).Value.Fenc = (uint8)SQLITE_UTF8;
            (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl + 1 * 40))).Value.FzName = pColl + 3 * 40;
            (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl + 1 * 40))).Value.Fenc = (uint8)SQLITE_UTF16LE;
            (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl + 2 * 40))).Value.FzName = pColl + 3 * 40;
            (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl + 2 * 40))).Value.Fenc = (uint8)SQLITE_UTF16BE;
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl))).Value.FzName, zName, libc.Uint64FromInt32(nName));
            pDel = _sqlite3HashInsert(tls, db + 656, (@unsafe.Pointer.To<TCollSeq>(__ccgo_ptr(pColl))).Value.FzName, pColl);
            if (pDel != (uintptr)0) {
                _sqlite3OomFault(tls, db);
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pDel);
                pColl = (uintptr)0;
            }
        }
    }
    return pColl;
}

internal static void _fkScanChildren(ж<libc.TLS> tls, uintptr pParse, uintptr pSrc, uintptr pTab, uintptr pIdx, uintptr pFKey, uintptr aiCol, int32 regData, int32 nIncr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr pAll = default;
        uintptr pEq = default;
        uintptr pEq1 = default;
        uintptr pLeft = default;
        uintptr pLeft1 = default;
        uintptr pNe = default;
        uintptr pRight = default;
        uintptr pRight1 = default;
        uintptr pWInfo = default;
        uintptr pWhere = default;
        uintptr v = default;
        uintptr zCol = default;
        int32 i = default;
        int32 iFkIfZero = default;
        int32 v2 = default;
        Ti16 iCol = default;
        Ti16 iCol1 = default;
        TNameContext _ᴛ1 = new();
        _ = db;
        _ = i;
        _ = iCol;
        _ = iCol1;
        _ = iFkIfZero;
        _ = pAll;
        _ = pEq;
        _ = pEq1;
        _ = pLeft;
        _ = pLeft1;
        _ = pNe;
        _ = pRight;
        _ = pRight1;
        _ = pWInfo;
        _ = pWhere;
        _ = v;
        _ = zCol;
        _ = v2;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pWhere = (uintptr)0;
        iFkIfZero = 0;
        v = _sqlite3GetVdbe(tls, pParse);
        if (nIncr < 0) {
            iFkIfZero = _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkIfZero, libc.Int32FromUint8((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred), 0);
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol)) {
                break;
            }
            if (pIdx != 0) {
                v2 = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)));
            } else {
                v2 = -(int32)1;
            }
            iCol = (int16)v2;
            pLeft = _exprTableRegister(tls, pParse, pTab, regData, iCol);
            if (aiCol != 0) {
                v2 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aiCol + (uintptr)i * 4));
            } else {
                v2 = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64))).Value.FiFrom;
            }
            iCol = (int16)v2;
            zCol = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom)).Value.FaCol + (uintptr)iCol * 16))).Value.FzCnName;
            pRight = _sqlite3Expr(tls, db, (int32)TK_ID, zCol);
            pEq = _sqlite3PExpr(tls, pParse, (int32)TK_EQ, pLeft, pRight);
            pWhere = _sqlite3ExprAnd(tls, pParse, pWhere, pEq);
            goto _1;
_1:;
            i = i + 1;
        }
        if (pTab == (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom && nIncr > 0) {
            if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
                pLeft1 = _exprTableRegister(tls, pParse, pTab, regData, (int16)(-(int32)1));
                pRight1 = _exprTableColumn(tls, db, pTab, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8))).Value.FiCursor, (int16)(-(int32)1));
                pNe = _sqlite3PExpr(tls, pParse, (int32)TK_NE, pLeft1, pRight1);
            } else {
                pAll = (uintptr)0;
                i = 0;
                for (;;) {
                    if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol))) {
                        break;
                    }
                    iCol1 = ~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2));
                    pLeft1 = _exprTableRegister(tls, pParse, pTab, regData, iCol1);
                    pRight1 = _sqlite3Expr(tls, db, (int32)TK_ID, (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)iCol1 * 16))).Value.FzCnName);
                    pEq1 = _sqlite3PExpr(tls, pParse, (int32)TK_IS, pLeft1, pRight1);
                    pAll = _sqlite3ExprAnd(tls, pParse, pAll, pEq1);
                    goto _4;
_4:;
                    i = i + 1;
                }
                pNe = _sqlite3PExpr(tls, pParse, (int32)TK_NOT, pAll, (uintptr)0);
            }
            pWhere = _sqlite3ExprAnd(tls, pParse, pWhere, pNe);
        }
        libc.Xmemset(tls, bp, 0, (uint64)56);
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpSrcList = pSrc;
        (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        _sqlite3ResolveExprNames(tls, bp, pWhere);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
            pWInfo = _sqlite3WhereBegin(tls, pParse, pSrc, pWhere, (uintptr)0, (uintptr)0, (uintptr)0, (uint16)0, 0);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkCounter, libc.Int32FromUint8((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred), nIncr);
            if (pWInfo != 0) {
                _sqlite3WhereEnd(tls, pWInfo);
            }
        }
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhere);
        if (iFkIfZero != 0) {
            _sqlite3VdbeJumpHereOrPopInst(ref (tls).DerefOrNull(), v, iFkIfZero);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _freeSpace(ж<libc.TLS> tls, uintptr pPage, int32 iStart, int32 iSize) {
    uintptr data = default;
    uintptr pTmp = default;
    uintptr v2 = default;
    Tu8 hdr = default;
    int32 iEnd = default;
    int32 iFreeBlk = default;
    int32 iOrigSize = default;
    int32 iPtr = default;
    int32 iPtrEnd = default;
    int32 nFrag = default;
    int32 x = default;
    int32 v1 = default;
    _ = data;
    _ = hdr;
    _ = iEnd;
    _ = iFreeBlk;
    _ = iOrigSize;
    _ = iPtr;
    _ = iPtrEnd;
    _ = nFrag;
    _ = pTmp;
    _ = x;
    _ = v1;
    _ = v2;
    nFrag = 0;
    iOrigSize = iSize;
    iEnd = iStart + iSize;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
    hdr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset;
    iPtr = libc.Int32FromUint8(hdr) + (int32)1;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iPtr + (int32)1)))) == 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iPtr))) == 0) {
        iFreeBlk = 0;
    } else {
        for (;;) {
            v1 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iPtr))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iPtr + 1)));
            iFreeBlk = v1;
            if (!(v1 < iStart)) {
                break;
            }
            if (iFreeBlk <= iPtr) {
                if (iFreeBlk == 0) {
                    break;
                }
                return _sqlite3CorruptError(tls, (int32)75121);
            }
            iPtr = iFreeBlk;
        }
        if (iFreeBlk > libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize) - (int32)4) {
            return _sqlite3CorruptError(tls, (int32)75126);
        }
        if (iFreeBlk != 0 && iEnd + (int32)3 >= iFreeBlk) {
            nFrag = iFreeBlk - iEnd;
            if (iEnd > iFreeBlk) {
                return _sqlite3CorruptError(tls, (int32)75138);
            }
            iEnd = iFreeBlk + (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFreeBlk + (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iFreeBlk + (int32)2) + 1))));
            if (iEnd > libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize)) {
                return _sqlite3CorruptError(tls, (int32)75141);
            }
            iSize = iEnd - iStart;
            iFreeBlk = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iFreeBlk))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iFreeBlk + 1)));
        }
        if (iPtr > libc.Int32FromUint8(hdr) + (int32)1) {
            iPtrEnd = iPtr + (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iPtr + (int32)2)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iPtr + (int32)2) + 1))));
            if (iPtrEnd + (int32)3 >= iStart) {
                if (iPtrEnd > iStart) {
                    return _sqlite3CorruptError(tls, (int32)75154);
                }
                nFrag = nFrag + (iStart - iPtrEnd);
                iSize = iEnd - iPtr;
                iStart = iPtr;
            }
        }
        if (nFrag > libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)7))))) {
            return _sqlite3CorruptError(tls, (int32)75160);
        }
        v2 = data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)7);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v2)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v2)) - libc.Int32FromUint8(libc.Uint8FromInt32(nFrag)));
    }
    pTmp = data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)5);
    x = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp + 1)));
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FbtsFlags) & (int32)BTS_FAST_SECURE) != 0) {
        libc.Xmemset(tls, data + (uintptr)iStart, 0, libc.Uint64FromInt32(iSize));
    }
    if (iStart <= x) {
        if (iStart < x) {
            return _sqlite3CorruptError(tls, (int32)75174);
        }
        if (iPtr != libc.Int32FromUint8(hdr) + (int32)1) {
            return _sqlite3CorruptError(tls, (int32)75175);
        }
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)1)))).Value = libc.Uint8FromInt32(iFreeBlk.Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)1) + 1))).Value = libc.Uint8FromInt32(iFreeBlk);
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)5)))).Value = libc.Uint8FromInt32(iEnd.Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)5) + 1))).Value = libc.Uint8FromInt32(iEnd);
    } else {
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iPtr))).Value = libc.Uint8FromInt32(iStart.Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iPtr + 1))).Value = libc.Uint8FromInt32(iStart);
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iStart))).Value = libc.Uint8FromInt32(iFreeBlk.Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)iStart + 1))).Value = libc.Uint8FromInt32(iFreeBlk);
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iStart + (int32)2)))).Value = libc.Uint8FromInt32(libc.Int32FromUint16(libc.Uint16FromInt32(iSize)).Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(data + (uintptr)(iStart + (int32)2) + 1))).Value = (uint8)libc.Uint16FromInt32(iSize);
    }
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pPage + 20))).Value += iOrigSize;
    return SQLITE_OK;
}

internal static void _fts5AppendPoslist(ж<libc.TLS> tls, uintptr p, Tu64 iDelta, uintptr pMulti, uintptr pBuf) {
    int32 nByte = default;
    int32 nData = default;
    int32 v1 = default;
    bool v2 = default;
    _ = nByte;
    _ = nData;
    _ = v1;
    _ = v2;
    nData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pMulti)).Value.Fbase.FnData;
    nByte = nData + (int32)9 + (int32)9 + (int32)FTS5_DATA_ZERO_PADDING;
    v2 = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK; if (v2) {
        if (libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn) + libc.Uint32FromInt32(nByte) <= libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.FnSpace)) {
            v1 = 0;
        } else {
            v1 = _sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), (@unsafe.Pointer)pBuf, libc.Uint32FromInt32(nByte + (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn));
        }
    }
    if (v2 && 0 == v1) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn), iDelta);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn), libc.Uint64FromInt32(nData * libc.Int32FromInt32(2)));
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn, (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pMulti)).Value.Fbase.FpData, libc.Uint64FromInt32(nData));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += nData;
        libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn, 0, (uint64)FTS5_DATA_ZERO_PADDING);
    }
}

internal static uintptr /*r*/ _fts5AppendTokendataIter(ж<libc.TLS> tls, uintptr p, uintptr pIn, uintptr pAppend) {
    Ti64 nAlloc = default;
    Ti64 nByte = default;
    Ti64 v2 = default;
    uintptr pNew = default;
    uintptr pRet = default;
    uintptr v3 = default;
    int64 v1 = default;
    _ = nAlloc;
    _ = nByte;
    _ = pNew;
    _ = pRet;
    _ = v1;
    _ = v2;
    _ = v3;
    pRet = pIn;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if (pIn == (uintptr)0 || (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pIn)).Value.FnIter == (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pIn)).Value.FnIterAlloc) {
            if (pIn != 0) {
                v1 = (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pIn)).Value.FnIterAlloc * (int64)2;
            } else {
                v1 = (int64)16;
            }
            nAlloc = v1;
            nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 72) + libc.Uint64FromInt64(nAlloc + libc.Int64FromInt32(1)) * libc.Uint64FromInt64(104));
            pNew = Xsqlite3_realloc64(tls, pIn, libc.Uint64FromInt64(nByte));
            if (pNew == (uintptr)0) {
                (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
            } else {
                if (pIn == (uintptr)0) {
                    libc.Xmemset(tls, pNew, 0, libc.Uint64FromInt64(nByte));
                }
                pRet = pNew;
                (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pNew)).Value.FnIterAlloc = nAlloc;
            }
        }
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
        _fts5IterClose(tls, pAppend);
    } else {
        v3 = pRet + 40;
        v2 = ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v3);
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v3) + 1;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRet + 72 + (uintptr)v2 * 8))).Value = pAppend;
    }
    return pRet;
}

internal static int32 /*r*/ _fts5Bm25GetData(ж<libc.TLS> tls, uintptr pApi, uintptr pFts, @unsafe.Pointer ppData) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 i = default;
        int32 nPhrase = default;
        int32 rc = default;
        float64 idf = default;
        Tsqlite3_int64 nByte = default;
        uintptr p = default;
        Tsqlite3_int64 _ᴛ1 = default;
        Tsqlite3_int64 _ᴛ2 = default;
        Tsqlite3_int64 _ᴛ3 = default;
        _ = i;
        _ = idf;
        _ = nByte;
        _ = nPhrase;
        _ = p;
        _ = rc;
        rc = SQLITE_OK;
        p = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxGetAuxdata)(tls, pFts, 0);
        if (p == (uintptr)0) {
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp))).Value = 0;
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 8))).Value = 0;
            nPhrase = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxPhraseCount)(tls, pFts);
            nByte = libc.Int64FromUint64((uint64)32 + libc.Uint64FromInt32(nPhrase * (int32)2) * (uint64)8);
            p = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
            if (p == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                libc.Xmemset(tls, p, 0, libc.Uint64FromInt64(nByte));
                (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)p)).Value.FnPhrase = nPhrase;
                (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)p)).Value.FaIDF = p + 1 * 32;
                (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)p)).Value.FaFreq = (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)p)).Value.FaIDF + (uintptr)nPhrase * 8;
            }
            if (rc == SQLITE_OK) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxRowCount)(tls, pFts, bp);
            }
            if (rc == SQLITE_OK) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxColumnTotalSize)(tls, pFts, -(int32)1, bp + 8);
            }
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)p)).Value.Favgdl = (float64)(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 8))) / (float64)(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp)));
            }
            i = 0;
            for (;;) {
                if (!(rc == SQLITE_OK && i < nPhrase)) {
                    break;
                }
                (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16))).Value = 0;
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxQueryPhrase)(tls, pFts, i, bp + 16, __ccgo_fp(_fts5CountCb));
                if (rc == SQLITE_OK) {
                    idf = libc.Xlog(tls, ((float64)(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp)) - ~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16))) + (float64)0.5D) / ((float64)(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16))) + (float64)0.5D));
                    if (idf <= (float64)0D) {
                        idf = (float64)1e-06D;
                    }
                    (@unsafe.Pointer.To<float64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Bm25Data>((@unsafe.Pointer)p)).Value.FaIDF + (uintptr)i * 8))).Value = idf;
                }
                goto _1;
_1:;
                i = i + 1;
            }
            if (rc != SQLITE_OK) {
                Xsqlite3_free(tls, p);
            } else {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TFts5ExtensionApi>((@unsafe.Pointer)pApi)).Value.FxSetAuxdata)(tls, pFts, p, __ccgo_fp(Xsqlite3_free));
            }
            if (rc != SQLITE_OK) {
                p = (uintptr)0;
            }
        }
        (@unsafe.Pointer.To<uintptr>(ppData)).Value = p;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _fts5CloneColset(ж<libc.TLS> tls, @unsafe.Pointer pRc, uintptr pOrig) {
    Tsqlite3_int64 nByte = default;
    uintptr pRet = default;
    _ = nByte;
    _ = pRet;
    if (pOrig != 0) {
        nByte = libc.Int64FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(quo(((@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)pOrig)).Value.FnCol + libc.Int32FromInt32(2)), libc.Int32FromInt32(2))));
        pRet = _sqlite3Fts5MallocZero(tls, pRc, nByte);
        if (pRet != 0) {
            libc.Xmemcpy(tls, pRet, pOrig, libc.Uint64FromInt64(nByte));
        }
    } else {
        pRet = (uintptr)0;
    }
    return pRet;
}

internal static int32 /*r*/ _fts5CreateAux(ж<libc.TLS> tls, uintptr pApi, uintptr zName, uintptr pUserData, Tfts5_extension_function __ccgo_fp_xFunc, uintptr __ccgo_fp_xDestroy) {
    Tsqlite3_int64 nByte = default;
    Tsqlite3_int64 nName = default;
    uintptr pAux = default;
    uintptr pGlobal = default;
    int32 rc = default;
    _ = nByte;
    _ = nName;
    _ = pAux;
    _ = pGlobal;
    _ = rc;
    pGlobal = pApi;
    rc = Xsqlite3_overload_function(tls, (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.Fdb, zName, -(int32)1);
    if (rc == SQLITE_OK) {
        nName = libc.Int64FromUint64(libc.Xstrlen(tls, zName) + (uint64)1);
        nByte = libc.Int64FromUint64((uint64)48 + libc.Uint64FromInt64(nName));
        pAux = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (pAux != 0) {
            libc.Xmemset(tls, pAux, 0, libc.Uint64FromInt64(nByte));
            (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FzFunc = pAux + 1 * 48;
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FzFunc, zName, libc.Uint64FromInt64(nName));
            (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FpGlobal = pGlobal;
            (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FpUserData = pUserData;
            (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FxFunc = __ccgo_fp_xFunc;
            (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FxDestroy = __ccgo_fp_xDestroy;
            (@unsafe.Pointer.To<TFts5Auxiliary>((@unsafe.Pointer)pAux)).Value.FpNext = (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpAux;
            (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpAux = pAux;
        } else {
            rc = (int32)SQLITE_NOMEM;
        }
    }
    return rc;
}

internal static uintptr /*r*/ _fts5DlidxIterInit(ж<libc.TLS> tls, uintptr p, int32 bRev, int32 iSegid, int32 iLeafPg) {
    int32 bDone = default;
    int32 i = default;
    Ti64 iRowid = default;
    Tsqlite3_int64 nByte = default;
    uintptr pIter = default;
    uintptr pLvl = default;
    uintptr pNew = default;
    _ = bDone;
    _ = i;
    _ = iRowid;
    _ = nByte;
    _ = pIter;
    _ = pLvl;
    _ = pNew;
    pIter = (uintptr)0;
    bDone = 0;
    i = 0;
    for (;;) {
        if (!((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && bDone == 0)) {
            break;
        }
        nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 8) + libc.Uint64FromInt32(i + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(32));
        pNew = Xsqlite3_realloc64(tls, pIter, libc.Uint64FromInt64(nByte));
        if (pNew == (uintptr)0) {
            (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
        } else {
            iRowid = ((int64)iSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(1)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)i).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)iLeafPg;
            pLvl = pNew + 8 + (uintptr)i * 32;
            pIter = pNew;
            libc.Xmemset(tls, pLvl, 0, (uint64)32);
            (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData = _fts5DataRead(tls, p, iRowid);
            if ((@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData != 0 && (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)pLvl)).Value.FpData)).Value.Fp))) & (int32)0x0001) == 0) {
                bDone = (int32)1;
            }
            (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl = i + (int32)1;
            if ((@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl > libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_HEIGHT_B)))) {
                (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FiSegid = iSegid;
        if (bRev == 0) {
            _fts5DlidxIterFirst(ref (tls).DerefOrNull(), pIter);
        } else {
            _fts5DlidxIterLast(tls, p, pIter);
        }
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
        _fts5DlidxIterFree(tls, pIter);
        pIter = (uintptr)0;
    }
    return pIter;
}

internal static void _fts5DoSecureDelete(ж<libc.TLS> tls, uintptr p, uintptr pSeg) {
    uintptr aIdx = default;
    uintptr aPg = default;
    uintptr aTermIdx = default;
    uintptr pPg = default;
    uintptr pTerm = default;
    int32 bDetailNone = default;
    int32 bEmpty = default;
    ref var bLastInDoclist = ref heap(new int32(), out var ᏑbLastInDoclist);
    int32 iDelKeyOff = default;
    int32 iIdx = default;
    int32 iKey = default;
    int32 iKeyOff = default;
    int32 iKeyOff1 = default;
    int32 iKeyOut = default;
    int32 iNextOff = default;
    int32 iOff = default;
    int32 iPgIdx = default;
    int32 iPgno = default;
    int32 iPrevKeyOut = default;
    int32 iSOP = default;
    int32 iSegid = default;
    int32 iStart = default;
    int32 iTermIdx = default;
    int32 nByte = default;
    int32 nIdx = default;
    int32 nMove = default;
    int32 nNewPg = default;
    int32 nPg = default;
    ref var nPos = ref heap(new int32(), out var ᏑnPos);
    int32 nShift = default;
    int32 nTermIdx = default;
    int32 pgno = default;
    int32 v2 = default;
    ref var iDelta = ref heap(new Tu64(), out var ᏑiDelta);
    ref var iNextDelta = ref heap(new Tu64(), out var ᏑiNextDelta);
    ref var nPrefix = ref heap(new Tu64(), out var ᏑnPrefix);
    ref var nPrefix2 = ref heap(new Tu64(), out var ᏑnPrefix2);
    ref var nSuffix = ref heap(new Tu64(), out var ᏑnSuffix);
    ref var nSuffix2 = ref heap(new Tu64(), out var ᏑnSuffix2);
    Ti64 iId = default;
    Ti64 iKeyIn = default;
    Ti64 iTermOff = default;
    ref var iVal = ref heap(new Tu32(), out var ᏑiVal);
    ref var iVal1 = ref heap(new Tu32(), out var ᏑiVal1);
    ref var iVal2 = ref heap(new Tu32(), out var ᏑiVal2);
    ref var iVal3 = ref heap(new Tu32(), out var ᏑiVal3);
    uint64 v5 = default;
    _ = aIdx;
    _ = aPg;
    _ = aTermIdx;
    _ = bDetailNone;
    _ = bEmpty;
    _ = bLastInDoclist;
    _ = iDelKeyOff;
    _ = iDelta;
    _ = iId;
    _ = iIdx;
    _ = iKey;
    _ = iKeyIn;
    _ = iKeyOff;
    _ = iKeyOff1;
    _ = iKeyOut;
    _ = iNextDelta;
    _ = iNextOff;
    _ = iOff;
    _ = iPgIdx;
    _ = iPgno;
    _ = iPrevKeyOut;
    _ = iSOP;
    _ = iSegid;
    _ = iStart;
    _ = iTermIdx;
    _ = iTermOff;
    _ = iVal;
    _ = iVal1;
    _ = iVal2;
    _ = iVal3;
    _ = nByte;
    _ = nIdx;
    _ = nMove;
    _ = nNewPg;
    _ = nPg;
    _ = nPos;
    _ = nPrefix;
    _ = nPrefix2;
    _ = nShift;
    _ = nSuffix;
    _ = nSuffix2;
    _ = nTermIdx;
    _ = pPg;
    _ = pTerm;
    _ = pgno;
    _ = v2;
    _ = v5;
    bDetailNone = libc.BoolInt32((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail == libc.Int32FromInt32(FTS5_DETAIL_NONE));
    iSegid = (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg)).Value.FiSegid;
    aPg = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.Fp;
    nPg = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.Fnn;
    iPgIdx = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf)).Value.FszLeaf;
    iDelta = (uint64)0;
    iNextOff = 0;
    iOff = 0;
    nIdx = 0;
    aIdx = (uintptr)0;
    bLastInDoclist = 0;
    iIdx = 0;
    iStart = 0;
    iDelKeyOff = 0;
    nIdx = nPg - iPgIdx;
    aIdx = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)(p + 60), (int64)nIdx + (int64)16);
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
        return;
    }
    libc.Xmemcpy(tls, aIdx, aPg + (uintptr)iPgIdx, libc.Uint64FromInt32(nIdx));
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno == (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno) {
        iStart = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafOffset;
    } else {
        iStart = libc.Int32FromUint16(_fts5GetU16(ref (tls).DerefOrNull(), aPg));
    }
    if (iStart > nPg) {
        _fts5IndexCorruptIdx(tls, p);
        Xsqlite3_free(tls, aIdx);
        return;
    }
    iSOP = iStart + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iStart), @unsafe.Pointer.FromPinnedBox(ᏑiDelta)));
    if (bDetailNone != 0) {
        while ((int64)iSOP < (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafOffset) {
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aPg + (uintptr)iSOP))) == 0x00) {
                iSOP = iSOP + 1;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aPg + (uintptr)iSOP))) == 0x00) {
                iSOP = iSOP + 1;
            }
            iStart = iSOP;
            iSOP = iStart + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iStart), @unsafe.Pointer.FromPinnedBox(ᏑiDelta)));
        }
        iNextOff = iSOP;
        if (iNextOff < (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiEndofDoclist && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aPg + (uintptr)iNextOff))) == 0x00) {
            iNextOff = iNextOff + 1;
        }
        if (iNextOff < (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiEndofDoclist && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aPg + (uintptr)iNextOff))) == 0x00) {
            iNextOff = iNextOff + 1;
        }
    } else {
        nPos = 0;
        iSOP = iSOP + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aPg + (uintptr)iSOP, @unsafe.Pointer.FromPinnedBox(ᏑnPos));
        while ((int64)iSOP < (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafOffset) {
            iStart = iSOP + nPos / (int32)2;
            iSOP = iStart + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iStart), @unsafe.Pointer.FromPinnedBox(ᏑiDelta)));
            iSOP = iSOP + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aPg + (uintptr)iSOP, @unsafe.Pointer.FromPinnedBox(ᏑnPos));
        }
        iNextOff = iSOP + (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos;
    }
    iOff = iStart;
    if (iNextOff >= iPgIdx) {
        pgno = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno + (int32)1;
        _fts5SecureDeleteOverflow(tls, p, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg, pgno, @unsafe.Pointer.FromPinnedBox(ᏑbLastInDoclist));
        iNextOff = iPgIdx;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FbDel) == 0) {
        if (iNextOff != iPgIdx) {
            iKeyOff = 0;
            iIdx = 0;
            for (;;) {
                if (!(iIdx < nIdx)) {
                    break;
                }
                iVal = (uint32)0;
                iIdx = iIdx + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aIdx + (uintptr)iIdx, @unsafe.Pointer.FromPinnedBox(ᏑiVal));
                iKeyOff = libc.Int32FromUint32((uint32)iKeyOff + iVal);
                if (iKeyOff == iNextOff) {
                    bLastInDoclist = (int32)1;
                }
                goto _1;
_1:;
            }
        }
        if (libc.Int32FromUint16(_fts5GetU16(ref (tls).DerefOrNull(), aPg)) == iStart && (bLastInDoclist != 0 || iNextOff == iPgIdx)) {
            _fts5PutU16(ref (tls).DerefOrNull(), aPg, (uint16)0);
        }
    }
    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FbDel != 0) {
        iOff = iOff + _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(aPg + (uintptr)iOff), iDelta);
        v2 = iOff;
        iOff = iOff + 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aPg + (uintptr)v2))).Value = (uint8)0x01;
    } else {
        if (bLastInDoclist == 0) {
            if (iNextOff != iPgIdx) {
                iNextDelta = (uint64)0;
                iNextOff = iNextOff + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iNextOff), @unsafe.Pointer.FromPinnedBox(ᏑiNextDelta)));
                iOff = iOff + _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(aPg + (uintptr)iOff), iDelta + iNextDelta);
            }
        } else {
            if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno == (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno && iStart == (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafOffset) {
                iKey = 0;
                iKeyOff1 = 0;
                iIdx = 0;
                for (;;) {
                    if (!(iIdx < nIdx)) {
                        break;
                    }
                    iVal1 = (uint32)0;
                    iIdx = iIdx + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aIdx + (uintptr)iIdx, @unsafe.Pointer.FromPinnedBox(ᏑiVal1));
                    if (libc.Uint32FromInt32(iKeyOff1) + iVal1 > libc.Uint32FromInt32(iStart)) {
                        break;
                    }
                    iKeyOff1 = libc.Int32FromUint32((uint32)iKeyOff1 + iVal1);
                    goto _3;
_3:;
                    iKey = iKey + 1;
                }
                v2 = iKeyOff1;
                iOff = v2;
                iDelKeyOff = v2;
                if (iNextOff != iPgIdx) {
                    nPrefix = (uint64)0;
                    nSuffix = (uint64)0;
                    nPrefix2 = (uint64)0;
                    nSuffix2 = (uint64)0;
                    iDelKeyOff = iNextOff;
                    iNextOff = iNextOff + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iNextOff), @unsafe.Pointer.FromPinnedBox(ᏑnPrefix2)));
                    iNextOff = iNextOff + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iNextOff), @unsafe.Pointer.FromPinnedBox(ᏑnSuffix2)));
                    if (iKey != (int32)1) {
                        iKeyOff1 = iKeyOff1 + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iKeyOff1), @unsafe.Pointer.FromPinnedBox(ᏑnPrefix)));
                    }
                    iKeyOff1 = iKeyOff1 + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aPg + (uintptr)iKeyOff1), @unsafe.Pointer.FromPinnedBox(ᏑnSuffix)));
                    if (nPrefix < nPrefix2) {
                        v5 = nPrefix;
                    } else {
                        v5 = nPrefix2;
                    }
                    nPrefix = v5;
                    nSuffix = nPrefix2 + nSuffix2 - nPrefix;
                    if (libc.Uint64FromInt32(iKeyOff1) + nSuffix > libc.Uint64FromInt32(iPgIdx) || libc.Uint64FromInt32(iNextOff) + nSuffix2 > libc.Uint64FromInt32(iPgIdx)) {
                        _fts5IndexCorruptIdx(tls, p);
                    } else {
                        if (iKey != (int32)1) {
                            iOff = iOff + _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(aPg + (uintptr)iOff), nPrefix);
                        }
                        iOff = iOff + _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(aPg + (uintptr)iOff), nSuffix);
                        if (nPrefix2 > libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.Fterm.Fn)) {
                            _fts5IndexCorruptIdx(tls, p);
                        } else {
                            if (nPrefix2 > nPrefix) {
                                libc.Xmemcpy(tls, aPg + (uintptr)iOff, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.Fterm.Fp + (uintptr)nPrefix, nPrefix2 - nPrefix);
                                iOff = libc.Int32FromUint64((uint64)iOff + (nPrefix2 - nPrefix));
                            }
                        }
                        libc.Xmemmove(tls, aPg + (uintptr)iOff, aPg + (uintptr)iNextOff, nSuffix2);
                        iOff = libc.Int32FromUint64((uint64)iOff + nSuffix2);
                        iNextOff = libc.Int32FromUint64((uint64)iNextOff + nSuffix2);
                    }
                }
            } else {
                if (iStart == (int32)4) {
                    iPgno = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno - (int32)1;
                    for (;;) {
                        if (!(iPgno > (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno)) {
                            break;
                        }
                        pPg = _fts5LeafRead(tls, p, ((int64)iSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)iPgno);
                        bEmpty = libc.BoolInt32(pPg != 0 && (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fnn == (int32)4);
                        _fts5DataRelease(tls, pPg);
                        if (bEmpty == 0) {
                            break;
                        }
                        goto _6;
_6:;
                        iPgno = iPgno - 1;
                    }
                    if (iPgno == (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno) {
                        iId = ((int64)iSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno;
                        pTerm = _fts5LeafRead(tls, p, iId);
                        if (pTerm != 0 && (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.FszLeaf == (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafOffset) {
                            aTermIdx = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.FszLeaf;
                            nTermIdx = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.Fnn - (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.FszLeaf;
                            iTermIdx = 0;
                            iTermOff = 0;
                            while ((int32)1 != 0) {
                                iVal2 = (uint32)0;
                                nByte = _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aTermIdx + (uintptr)iTermIdx, @unsafe.Pointer.FromPinnedBox(ᏑiVal2));
                                iTermOff = iTermOff + libc.Int64FromUint32(iVal2);
                                if (iTermIdx + nByte >= nTermIdx) {
                                    break;
                                }
                                iTermIdx = iTermIdx + nByte;
                            }
                            nTermIdx = iTermIdx;
                            if (iTermOff > (int64)(@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.FszLeaf) {
                                _fts5IndexCorruptIdx(tls, p);
                            } else {
                                libc.Xmemmove(tls, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.Fp + (uintptr)iTermOff, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.FszLeaf, libc.Uint64FromInt32(nTermIdx));
                                _fts5PutU16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.Fp + 2, libc.Uint16FromInt64(iTermOff));
                                _fts5DataWrite(tls, p, iId, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pTerm)).Value.Fp, (int32)(iTermOff + (int64)nTermIdx));
                                if (nTermIdx == 0) {
                                    _fts5SecureDeleteIdxEntry(tls, p, iSegid, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno);
                                }
                            }
                        }
                        _fts5DataRelease(tls, pTerm);
                    }
                }
            }
        }
    }
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        nMove = nPg - iNextOff;
        nShift = iNextOff - iOff;
        nNewPg = 0;
        iPrevKeyOut = 0;
        iKeyIn = 0;
        if (nMove > 0) {
            libc.Xmemmove(tls, aPg + (uintptr)iOff, aPg + (uintptr)iNextOff, libc.Uint64FromInt32(nMove));
        }
        iPgIdx = iPgIdx - nShift;
        nNewPg = iPgIdx;
        _fts5PutU16(ref (tls).DerefOrNull(), aPg + 2, libc.Uint16FromInt32(iPgIdx));
        iIdx = 0;
        for (;;) {
            if (!(iIdx < nIdx)) {
                break;
            }
            iVal3 = (uint32)0;
            iIdx = iIdx + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aIdx + (uintptr)iIdx, @unsafe.Pointer.FromPinnedBox(ᏑiVal3));
            iKeyIn = iKeyIn + libc.Int64FromUint32(iVal3);
            if (iKeyIn > (int64)nPg) {
                _fts5IndexCorruptIdx(tls, p);
                break;
            }
            if (iKeyIn != (int64)iDelKeyOff) {
                if (iKeyIn > (int64)iOff) {
                    v2 = nShift;
                } else {
                    v2 = 0;
                }
                iKeyOut = (int32)(iKeyIn - (int64)v2);
                nNewPg = nNewPg + _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(aPg + (uintptr)nNewPg), libc.Uint64FromInt32(iKeyOut - iPrevKeyOut));
                iPrevKeyOut = iKeyOut;
            }
            goto _7;
_7:;
        }
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && iPgIdx == nNewPg && nIdx > 0 && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno != (int32)1) {
            _fts5SecureDeleteIdxEntry(tls, p, iSegid, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno);
        }
        _fts5DataWrite(tls, p, ((int64)iSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno, aPg, nNewPg);
    }
    Xsqlite3_free(tls, aIdx);
}

internal static void _fts5ExprAddChildren(ж<libc.TLS> tls, uintptr p, uintptr pSub) {
    int32 ii = default;
    int32 nByte = default;
    int32 v1 = default;
    uintptr v2 = default;
    _ = ii;
    _ = nByte;
    _ = v1;
    _ = v2;
    ii = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FnChild;
    if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FeType != (int32)FTS5_NOT && (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pSub)).Value.FeType == (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FeType) {
        nByte = libc.Int32FromUint64((uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pSub)).Value.FnChild));
        libc.Xmemcpy(tls, p + 48 + (uintptr)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FnChild * 8, pSub + 48, libc.Uint64FromInt32(nByte));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 40))).Value += (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pSub)).Value.FnChild;
        Xsqlite3_free(tls, pSub);
    } else {
        v2 = p + 40;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48 + (uintptr)v1 * 8))).Value = pSub;
    }
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FnChild)) {
            break;
        }
        if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FiHeight > (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48 + (uintptr)ii * 8))))).Value.FiHeight + (int32)1) {
            v1 = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FiHeight;
        } else {
            v1 = (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 48 + (uintptr)ii * 8))))).Value.FiHeight + (int32)1;
        }
        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)p)).Value.FiHeight = v1;
        goto _3;
_3:;
        ii = ii + 1;
    }
}

internal static int32 /*r*/ _fts5ExprNearIsMatch(ж<libc.TLS> tls, @unsafe.Pointer pRc, uintptr pNear) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(192);
        defer(tls.Free, (nint)192, ref ᒐ);
        uintptr a = default;
        uintptr apPhrase = default;
        uintptr pPos = default;
        uintptr pPoslist = default;
        uintptr pWriter = default;
        int32 bMatch = default;
        int32 bRet = default;
        int32 i = default;
        int32 iAdv = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        Ti64 iMax = default;
        Ti64 iMin = default;
        Ti64 iPos = default;
        Tsqlite3_int64 nByte = default;
        array<TFts5NearTrimmer> _ᴛ1 = new(4);
        _ = a;
        _ = apPhrase;
        _ = bMatch;
        _ = bRet;
        _ = i;
        _ = iAdv;
        _ = iMax;
        _ = iMin;
        _ = iPos;
        _ = nByte;
        _ = pPos;
        _ = pPoslist;
        _ = pWriter;
        _ = rc;
        a = bp;
        apPhrase = pNear + 24;
        rc = ~@unsafe.Pointer.To<int32>(pRc);
        if ((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase > libc.Int32FromUint64(libc.Uint64FromInt64(192) / libc.Uint64FromInt64(48))) {
            nByte = libc.Int64FromUint64((uint64)48 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase));
            a = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), nByte);
        } else {
            libc.Xmemset(tls, bp, 0, (uint64)192);
        }
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<int32>(pRc)).Value = rc;
            r = 0; goto ᒐdone;
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                break;
            }
            pPoslist = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apPhrase + (uintptr)i * 8)) + 8;
            _fts5LookaheadReaderInit(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pPoslist)).Value.Fp, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pPoslist)).Value.Fn, a + (uintptr)i * 48);
            (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pPoslist)).Value.Fn = 0;
            (@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a + (uintptr)i * 48))).Value.FpOut = pPoslist;
            goto _1;
_1:;
            i = i + 1;
        }
        while ((int32)1 != 0) {
            iMax = (@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a))).Value.Freader.FiPos;
            for (var cond = true; cond; cond = bMatch == 0) {
                bMatch = (int32)1;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                        break;
                    }
                    pPos = a + (uintptr)i * 48;
                    iMin = iMax - (int64)(@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNear + 24 + (uintptr)i * 8))))).Value.FnTerm - (int64)(@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnNear;
                    if ((@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)pPos)).Value.FiPos < iMin || (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)pPos)).Value.FiPos > iMax) {
                        bMatch = 0;
                        while ((@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)pPos)).Value.FiPos < iMin) {
                            if (_fts5LookaheadReaderNext(ref (tls).DerefOrNull(), pPos) != 0) {
                                goto ismatch_out;
                            }
                        }
                        if ((@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)pPos)).Value.FiPos > iMax) {
                            iMax = (@unsafe.Pointer.To<TFts5LookaheadReader>((@unsafe.Pointer)pPos)).Value.FiPos;
                        }
                    }
                    goto _2;
_2:;
                    i = i + 1;
                }
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                    break;
                }
                iPos = (@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a + (uintptr)i * 48))).Value.Freader.FiPos;
                pWriter = a + (uintptr)i * 48 + 32;
                if ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a + (uintptr)i * 48))).Value.FpOut)).Value.Fn == 0 || iPos != (@unsafe.Pointer.To<TFts5PoslistWriter>((@unsafe.Pointer)pWriter)).Value.FiPrev) {
                    _sqlite3Fts5PoslistSafeAppend(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a + (uintptr)i * 48))).Value.FpOut, (@unsafe.Pointer)pWriter, iPos);
                }
                goto _3;
_3:;
                i = i + 1;
            }
            iAdv = 0;
            iMin = (@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a))).Value.Freader.FiLookahead;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a + (uintptr)i * 48))).Value.Freader.FiLookahead < iMin) {
                    iMin = (@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a + (uintptr)i * 48))).Value.Freader.FiLookahead;
                    iAdv = i;
                }
                goto _4;
_4:;
                i = i + 1;
            }
            if (_fts5LookaheadReaderNext(ref (tls).DerefOrNull(), a + (uintptr)iAdv * 48) != 0) {
                goto ismatch_out;
            }
        }
        goto ismatch_out;
ismatch_out:;
        bRet = libc.BoolInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5NearTrimmer>(__ccgo_ptr(a))).Value.FpOut)).Value.Fn > 0);
        (@unsafe.Pointer.To<int32>(pRc)).Value = rc;
        if (a != bp) {
            Xsqlite3_free(tls, a);
        }
        r = bRet; goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _fts5ExprPhraseIsMatch(ж<libc.TLS> tls, uintptr pNode, uintptr pPhrase, @unsafe.Pointer pbMatch) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        ref var a = ref heap(new uintptr(), out var Ꮡa);
        uintptr aIter = default;
        uintptr pPos = default;
        uintptr pTerm = default;
        int32 bFirst = default;
        int32 bFlag = default;
        int32 bMatch = default;
        int32 i = default;
        ref var n = ref heap(new int32(), out var Ꮡn);
        int32 rc = default;
        ref var buf = ref heap(new TFts5Buffer(), out var Ꮡbuf);
        Ti64 iAdj = default;
        Ti64 iPos = default;
        Tsqlite3_int64 nByte = default;
        ref var writer = ref heap(new TFts5PoslistWriter(), out var Ꮡwriter);
        array<TFts5PoslistReader> _ᴛ1 = new(4);
        _ = a;
        _ = aIter;
        _ = bFirst;
        _ = bFlag;
        _ = bMatch;
        _ = buf;
        _ = i;
        _ = iAdj;
        _ = iPos;
        _ = n;
        _ = nByte;
        _ = pPos;
        _ = pTerm;
        _ = rc;
        _ = writer;
        writer = new TFts5PoslistWriter();
        aIter = bp;
        rc = SQLITE_OK;
        bFirst = libc.Int32FromUint8((@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32))).Value.FbFirst);
        _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pPhrase + 8));
        if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm > libc.Int32FromUint64(libc.Uint64FromInt64(128) / libc.Uint64FromInt64(32))) {
            nByte = libc.Int64FromUint64((uint64)32 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm));
            aIter = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
            if (!(aIter != 0)) {
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
        }
        libc.Xmemset(tls, aIter, 0, (uint64)32 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm));
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm)) {
                break;
            }
            pTerm = pPhrase + 32 + (uintptr)i * 40;
            n = 0;
            bFlag = 0;
            a = (uintptr)0;
            if ((@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpSynonym != 0) {
                buf = new TFts5Buffer();
                rc = _fts5ExprSynonymList(tls, pTerm, (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid, @unsafe.Pointer.FromPinnedBox(Ꮡbuf), @unsafe.Pointer.FromBox(Ꮡa), @unsafe.Pointer.FromPinnedBox(Ꮡn));
                if (rc != 0) {
                    Xsqlite3_free(tls, a);
                    goto ismatch_out;
                }
                if (a == buf.Fp) {
                    bFlag = (int32)1;
                }
            } else {
                a = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpIter)).Value.FpData;
                n = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpIter)).Value.FnData;
            }
            _sqlite3Fts5PoslistReaderInit(tls, a, n, aIter + (uintptr)i * 32);
            (@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FbFlag = libc.Uint8FromInt32(bFlag);
            if ((@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FbEof != 0) {
                goto ismatch_out;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        while ((int32)1 != 0) {
            iPos = (@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter))).Value.FiPos;
            for (var cond = true; cond; cond = bMatch == 0) {
                bMatch = (int32)1;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm)) {
                        break;
                    }
                    pPos = aIter + (uintptr)i * 32;
                    iAdj = iPos + (int64)i;
                    if ((@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pPos)).Value.FiPos != iAdj) {
                        bMatch = 0;
                        while ((@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pPos)).Value.FiPos < iAdj) {
                            if (_sqlite3Fts5PoslistReaderNext(ref (tls).DerefOrNull(), pPos) != 0) {
                                goto ismatch_out;
                            }
                        }
                        if ((@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pPos)).Value.FiPos > iAdj) {
                            iPos = (@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pPos)).Value.FiPos - (int64)i;
                        }
                    }
                    goto _2;
_2:;
                    i = i + 1;
                }
            }
            if (bFirst == 0 || (int32)((Ti64)(iPos & libc.Int64FromInt32(0x7FFFFFFF))) == 0) {
                rc = _sqlite3Fts5PoslistWriterAppend(tls, (@unsafe.Pointer)(pPhrase + 8), @unsafe.Pointer.FromPinnedBox(Ꮡwriter), iPos);
                if (rc != SQLITE_OK) {
                    goto ismatch_out;
                }
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm)) {
                    break;
                }
                if (_sqlite3Fts5PoslistReaderNext(ref (tls).DerefOrNull(), aIter + (uintptr)i * 32) != 0) {
                    goto ismatch_out;
                }
                goto _3;
_3:;
                i = i + 1;
            }
        }
        goto ismatch_out;
ismatch_out:;
        (@unsafe.Pointer.To<int32>(pbMatch)).Value = libc.BoolInt32((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.Fposlist.Fn > 0);
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm)) {
                break;
            }
            if ((@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FbFlag != 0) {
                Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.Fa);
            }
            goto _4;
_4:;
            i = i + 1;
        }
        if (aIter != bp) {
            Xsqlite3_free(tls, aIter);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _fts5ExprSynonymList(ж<libc.TLS> tls, uintptr pTerm, Ti64 iRowid, @unsafe.Pointer pBuf, @unsafe.Pointer pa, @unsafe.Pointer pn) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        uintptr aIter = default;
        uintptr aNew = default;
        uintptr p = default;
        uintptr pIter = default;
        int32 i = default;
        int32 nAlloc = default;
        int32 nIter = default;
        int32 rc = default;
        Ti64 iMin = default;
        Ti64 iPrev = default;
        Tsqlite3_int64 nByte = default;
        ref var writer = ref heap(new TFts5PoslistWriter(), out var Ꮡwriter);
        array<TFts5PoslistReader> _ᴛ1 = new(4);
        _ = aIter;
        _ = aNew;
        _ = i;
        _ = iMin;
        _ = iPrev;
        _ = nAlloc;
        _ = nByte;
        _ = nIter;
        _ = p;
        _ = pIter;
        _ = rc;
        _ = writer;
        aIter = bp;
        nIter = 0;
        nAlloc = (int32)4;
        rc = SQLITE_OK;
        p = pTerm;
        for (;;) {
            if (!(p != 0)) {
                break;
            }
            pIter = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpIter;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FbEof) == 0 && (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FiRowid == iRowid) {
                if ((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FnData == 0) {
                    goto _1;
                }
                if (nIter == nAlloc) {
                    nByte = libc.Int64FromUint64((uint64)32 * libc.Uint64FromInt32(nAlloc) * (uint64)2);
                    aNew = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
                    if (aNew == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                        goto synonym_poslist_out;
                    }
                    libc.Xmemcpy(tls, aNew, aIter, (uint64)32 * libc.Uint64FromInt32(nIter));
                    nAlloc = nAlloc * (int32)2;
                    if (aIter != bp) {
                        Xsqlite3_free(tls, aIter);
                    }
                    aIter = aNew;
                }
                _sqlite3Fts5PoslistReaderInit(tls, (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FpData, (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)pIter)).Value.FnData, aIter + (uintptr)nIter * 32);
                nIter = nIter + 1;
            }
            goto _1;
_1:;
            p = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpSynonym;
        }
        if (nIter == (int32)1) {
            (@unsafe.Pointer.To<uintptr>(pa)).Value = (@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter))).Value.Fa;
            (@unsafe.Pointer.To<int32>(pn)).Value = (@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter))).Value.Fn;
        } else {
            writer = new TFts5PoslistWriter();
            iPrev = (int64)(-(int32)1);
            _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), pBuf);
            while ((int32)1 != 0) {
                iMin = libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32)));
                i = 0;
                for (;;) {
                    if (!(i < nIter)) {
                        break;
                    }
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FbEof) == 0) {
                        if ((@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FiPos == iPrev) {
                            if (_sqlite3Fts5PoslistReaderNext(ref (tls).DerefOrNull(), aIter + (uintptr)i * 32) != 0) {
                                goto _2;
                            }
                        }
                        if ((@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FiPos < iMin) {
                            iMin = (@unsafe.Pointer.To<TFts5PoslistReader>(__ccgo_ptr(aIter + (uintptr)i * 32))).Value.FiPos;
                        }
                    }
                    goto _2;
_2:;
                    i = i + 1;
                }
                if (iMin == (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32)))) || rc != SQLITE_OK) {
                    break;
                }
                rc = _sqlite3Fts5PoslistWriterAppend(tls, pBuf, @unsafe.Pointer.FromPinnedBox(Ꮡwriter), iMin);
                iPrev = iMin;
            }
            if (rc == SQLITE_OK) {
                (@unsafe.Pointer.To<uintptr>(pa)).Value = (@unsafe.Pointer.To<uintptr>(pBuf)).Value;
                (@unsafe.Pointer.To<int32>(pn)).Value = (@unsafe.Pointer.To<int32>(@unsafe.Add(pBuf, 8))).Value;
            }
        }
        goto synonym_poslist_out;
synonym_poslist_out:;
        if (aIter != bp) {
            Xsqlite3_free(tls, aIter);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5FlushOneHash(ж<libc.TLS> tls, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        ref var bDel = ref heap(new int32(), out var ᏑbDel);
        int32 bSecureDelete = default;
        int32 bTermWritten = default;
        int32 eDetail = default;
        int32 iOff = default;
        int32 iPos = default;
        int32 iSegid = default;
        int32 n = default;
        int32 nCopy = default;
        ref var nDoclist = ref heap(new int32(), out var ᏑnDoclist);
        ref var nPos = ref heap(new int32(), out var ᏑnPos);
        int32 nSpace = default;
        ref var nTerm = ref heap(new int32(), out var ᏑnTerm);
        ref var pgnoLast = ref heap(new int32(), out var ᏑpgnoLast);
        int32 pgsz = default;
        int32 v1 = default;
        ref var iDelta = ref heap(new Tu64(), out var ᏑiDelta);
        Tu64 iRowidDelta = default;
        Ti64 iPrev = default;
        Ti64 iRowid = default;
        uintptr pBuf = default;
        ref var pDoclist = ref heap(new uintptr(), out var ᏑpDoclist);
        uintptr pHash = default;
        uintptr pPoslist = default;
        uintptr pSeg = default;
        ref var pStruct = ref heap(new uintptr(), out var ᏑpStruct);
        ref var zTerm = ref heap(new uintptr(), out var ᏑzTerm);
        uintptr v2 = default;
        @unsafe.Pointer pPgidx = default;
        TFts5SegWriter _ᴛ1 = default;
        _ = bDel;
        _ = bSecureDelete;
        _ = bTermWritten;
        _ = eDetail;
        _ = iDelta;
        _ = iOff;
        _ = iPos;
        _ = iPrev;
        _ = iRowid;
        _ = iRowidDelta;
        _ = iSegid;
        _ = n;
        _ = nCopy;
        _ = nDoclist;
        _ = nPos;
        _ = nSpace;
        _ = nTerm;
        _ = pBuf;
        _ = pDoclist;
        _ = pHash;
        _ = pPgidx;
        _ = pPoslist;
        _ = pSeg;
        _ = pStruct;
        _ = pgnoLast;
        _ = pgsz;
        _ = zTerm;
        _ = v1;
        _ = v2;
        pHash = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpHash;
        pgnoLast = 0;
        pStruct = _fts5StructureRead(tls, p);
        _fts5StructureInvalidate(tls, p);
        if (_sqlite3Fts5HashIsEmpty(ref (tls).DerefOrNull(), pHash) == 0) {
            iSegid = _fts5AllocateSegid(tls, p, pStruct);
            if (iSegid != 0) {
                pgsz = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.Fpgsz;
                eDetail = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail;
                bSecureDelete = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FbSecureDelete;
                _fts5WriteInit(tls, p, bp, iSegid);
                pBuf = bp + 8 + 8;
                pPgidx = __ccgo_ptr(bp + 32);
                if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                    (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = _sqlite3Fts5HashScanInit(tls, pHash, (uintptr)0, 0);
                }
                while ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && 0 == _sqlite3Fts5HashScanEof(ref (tls).DerefOrNull(), pHash)) {
                    _sqlite3Fts5HashScanEntry(tls, pHash, @unsafe.Pointer.FromBox(ᏑzTerm), @unsafe.Pointer.FromPinnedBox(ᏑnTerm), @unsafe.Pointer.FromBox(ᏑpDoclist), @unsafe.Pointer.FromPinnedBox(ᏑnDoclist));
                    if (bSecureDelete == 0) {
                        _fts5WriteAppendTerm(tls, p, bp, nTerm, zTerm);
                        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
                            break;
                        }
                    }
                    if (!(bSecureDelete != 0) && pgsz >= (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pPgidx, 8)) + nDoclist + (int32)1) {
                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn, pDoclist, libc.Uint64FromInt32(nDoclist));
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += nDoclist;
                    } else {
                        bTermWritten = libc.BoolInt32(!(bSecureDelete != 0));
                        iRowid = 0;
                        iPrev = 0;
                        iOff = 0;
                        while ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && iOff < nDoclist) {
                            iDelta = (uint64)0;
                            iOff = iOff + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pDoclist + (uintptr)iOff), @unsafe.Pointer.FromPinnedBox(ᏑiDelta)));
                            iRowid = libc.Int64FromUint64((uint64)iRowid + iDelta);
                            if (bSecureDelete != 0) {
                                if (eDetail == (int32)FTS5_DETAIL_NONE) {
                                    if (iOff < nDoclist && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDoclist + (uintptr)iOff))) == 0x00 && !(_fts5FlushSecureDelete(tls, p, pStruct, zTerm, nTerm, iRowid) != 0)) {
                                        iOff = iOff + 1;
                                        if (iOff < nDoclist && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDoclist + (uintptr)iOff))) == 0x00) {
                                            iOff = iOff + 1;
                                        } else {
                                            continue;
                                        }
                                    }
                                } else {
                                    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDoclist + (uintptr)iOff))) & (int32)0x01) != 0 && !(_fts5FlushSecureDelete(tls, p, pStruct, zTerm, nTerm, iRowid) != 0)) {
                                        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDoclist + (uintptr)iOff))) == (int32)0x01) {
                                            iOff = iOff + 1;
                                            continue;
                                        }
                                    }
                                }
                            }
                            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && bTermWritten == 0) {
                                _fts5WriteAppendTerm(tls, p, bp, nTerm, zTerm);
                                bTermWritten = (int32)1;
                            }
                            if ((@unsafe.Pointer.To<TFts5SegWriter>(__ccgo_ptr(bp))).Value.FbFirstRowidInPage != 0) {
                                _fts5PutU16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp, libc.Uint16FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn));
                                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn), libc.Uint64FromInt64(iRowid));
                                (@unsafe.Pointer.To<TFts5SegWriter>(__ccgo_ptr(bp))).Value.FbFirstRowidInPage = (uint8)0;
                                _fts5WriteDlidxAppend(tls, p, __ccgo_ptr(bp), iRowid);
                            } else {
                                iRowidDelta = libc.Uint64FromInt64(iRowid) - libc.Uint64FromInt64(iPrev);
                                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn), iRowidDelta);
                            }
                            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
                                break;
                            }
                            iPrev = iRowid;
                            if (eDetail == (int32)FTS5_DETAIL_NONE) {
                                if (iOff < nDoclist && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDoclist + (uintptr)iOff))) == 0) {
                                    v2 = pBuf + 8;
                                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)v1))).Value = (uint8)0;
                                    iOff = iOff + 1;
                                    if (iOff < nDoclist && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pDoclist + (uintptr)iOff))) == 0) {
                                        v2 = pBuf + 8;
                                        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)v1))).Value = (uint8)0;
                                        iOff = iOff + 1;
                                    }
                                }
                                if ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pPgidx, 8)) >= pgsz) {
                                    _fts5WriteFlushLeaf(tls, p, bp);
                                }
                            } else {
                                bDel = 0;
                                nPos = 0;
                                nCopy = _fts5GetPoslistSize(ref (tls).DerefOrNull(), pDoclist + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(ᏑnPos), @unsafe.Pointer.FromPinnedBox(ᏑbDel));
                                if (bDel != 0 && bSecureDelete != 0) {
                                    _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), (@unsafe.Pointer)pBuf, (int64)nPos * (int64)2);
                                    iOff = iOff + nCopy;
                                    nCopy = nPos;
                                } else {
                                    nCopy = nCopy + nPos;
                                }
                                if ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pPgidx, 8)) + nCopy <= pgsz) {
                                    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn, pDoclist + (uintptr)iOff, libc.Uint64FromInt32(nCopy));
                                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += nCopy;
                                } else {
                                    pPoslist = pDoclist + (uintptr)iOff;
                                    iPos = 0;
                                    while ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                                        nSpace = pgsz - (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn - ~@unsafe.Pointer.To<int32>(@unsafe.Add(pPgidx, 8));
                                        n = 0;
                                        if (nCopy - iPos <= nSpace) {
                                            n = nCopy - iPos;
                                        } else {
                                            n = _fts5PoslistPrefix(ref (tls).DerefOrNull(), pPoslist + (uintptr)iPos, nSpace);
                                        }
                                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn, pPoslist + (uintptr)iPos, libc.Uint64FromInt32(n));
                                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pBuf + 8))).Value += n;
                                        iPos = iPos + n;
                                        if ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pPgidx, 8)) >= pgsz) {
                                            _fts5WriteFlushLeaf(tls, p, bp);
                                        }
                                        if (iPos >= nCopy) {
                                            break;
                                        }
                                    }
                                }
                                iOff = iOff + nCopy;
                            }
                        }
                    }
                    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                        _sqlite3Fts5HashScanNext(ref (tls).DerefOrNull(), pHash);
                    }
                }
                _fts5WriteFinish(tls, p, bp, @unsafe.Pointer.FromPinnedBox(ᏑpgnoLast));
                if (pgnoLast > 0) {
                    if ((@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel == 0) {
                        _fts5StructureAddLevel(tls, p + 60, @unsafe.Pointer.FromBox(ᏑpStruct));
                    }
                    _fts5StructureExtendLevel(tls, p + 60, pStruct, 0, (int32)1, 0);
                    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                        v2 = pStruct + 32 + 4;
                        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                        pSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pStruct + 32))).Value.FaSeg + (uintptr)v1 * 56;
                        (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid = iSegid;
                        (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst = (int32)1;
                        (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoLast = pgnoLast;
                        if ((@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr > (uint64)0) {
                            (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiOrigin1 = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr;
                            (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiOrigin2 = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr;
                            (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FnEntry = libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnPendingRow);
                            (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr + 1;
                        }
                        (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment + 1;
                    }
                    _fts5StructurePromote(tls, p, 0, pStruct);
                }
            }
        }
        _fts5IndexAutomerge(tls, p, @unsafe.Pointer.FromBox(ᏑpStruct), pgnoLast + (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnContentlessDelete);
        _fts5IndexCrisismerge(tls, p, @unsafe.Pointer.FromBox(ᏑpStruct));
        _fts5StructureWrite(tls, p, (@unsafe.Pointer)pStruct);
        _fts5StructureRelease(tls, pStruct);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _fts5FreeCursorComponents(ж<libc.TLS> tls, uintptr pCsr) {
    int32 eStmt = default;
    uintptr pData = default;
    uintptr pNext = default;
    uintptr pSorter = default;
    uintptr pTab = default;
    _ = eStmt;
    _ = pData;
    _ = pNext;
    _ = pSorter;
    _ = pTab;
    pTab = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab;
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FaInstIter);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FaInst);
    if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpStmt != 0) {
        eStmt = _fts5StmtType(ref (tls).DerefOrNull(), pCsr);
        _sqlite3Fts5StorageStmtRelease(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage, eStmt, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpStmt);
    }
    if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter != 0) {
        pSorter = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpSorter;
        Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TFts5Sorter>((@unsafe.Pointer)pSorter)).Value.FpStmt);
        Xsqlite3_free(tls, pSorter);
    }
    if ((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FePlan != (int32)FTS5_PLAN_SOURCE) {
        _sqlite3Fts5ExprFree(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr);
    }
    pData = (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpAuxdata;
    for (;;) {
        if (!(pData != 0)) {
            break;
        }
        pNext = (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpNext;
        if ((@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FxDelete != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FxDelete)(tls, (@unsafe.Pointer.To<TFts5Auxdata>((@unsafe.Pointer)pData)).Value.FpPtr);
        }
        Xsqlite3_free(tls, pData);
        goto _1;
_1:;
        pData = pNext;
    }
    Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpRankArgStmt);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FapRankArg);
    if (((@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fcsrflags & (int32)FTS5CSR_FREE_ZRANK) != 0) {
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FzRank);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FzRankArgs);
    }
    _sqlite3Fts5IndexCloseReader(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex);
    libc.Xmemset(tls, pCsr + 32, 0, (uint64)184 - libc.Uint64FromInt64((int64)(pCsr + 32) - (int64)pCsr));
}

internal static int32 /*r*/ _fts5HashAddPoslistSize(ж<libc.TLS> tls, uintptr pHash, uintptr p, uintptr p2) {
    int32 nByte = default;
    int32 nData = default;
    int32 nPos = default;
    int32 nRet = default;
    int32 nSz = default;
    int32 v2 = default;
    uintptr pPtr = default;
    uintptr v1 = default;
    _ = nByte;
    _ = nData;
    _ = nPos;
    _ = nRet;
    _ = nSz;
    _ = pPtr;
    _ = v1;
    _ = v2;
    nRet = 0;
    if ((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist != 0) {
        if (p2 != 0) {
            v1 = p2;
        } else {
            v1 = p;
        }
        pPtr = v1;
        nData = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData;
        if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FeDetail == (int32)FTS5_DETAIL_NONE) {
            if ((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbDel != 0) {
                v2 = nData;
                nData = nData + 1;
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtr + (uintptr)v2))).Value = (uint8)0x00;
                if ((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbContent != 0) {
                    v2 = nData;
                    nData = nData + 1;
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtr + (uintptr)v2))).Value = (uint8)0x00;
                }
            }
        } else {
            nSz = nData - (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist - (int32)1;
            nPos = nSz * (int32)2 + libc.Int32FromUint8((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbDel);
            if (nPos <= (int32)127) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPtr + (uintptr)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist))).Value = libc.Uint8FromInt32(nPos);
            } else {
                nByte = _sqlite3Fts5GetVarintLen(ref (tls).DerefOrNull(), libc.Uint32FromInt32(nPos));
                libc.Xmemmove(tls, pPtr + (uintptr)((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist + nByte), pPtr + (uintptr)((@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist + (int32)1), libc.Uint64FromInt32(nSz));
                _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)(pPtr + (uintptr)(@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist), libc.Uint64FromInt32(nPos));
                nData = nData + (nByte - (int32)1);
            }
        }
        nRet = nData - (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData;
        if (p2 == (uintptr)0) {
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FiSzPoslist = 0;
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbDel = (uint8)0;
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FbContent = (uint8)0;
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData = nData;
        }
    }
    return nRet;
}

internal static int32 /*r*/ _fts5HashEntrySort(ж<libc.TLS> tls, uintptr pHash, uintptr pTerm, int32 nTerm, uintptr ppSorted) {
    uintptr ap = default;
    uintptr pEntry = default;
    uintptr pIter = default;
    uintptr pList = default;
    int32 i = default;
    int32 iSlot = default;
    int32 nMergeSlot = default;
    _ = ap;
    _ = i;
    _ = iSlot;
    _ = nMergeSlot;
    _ = pEntry;
    _ = pIter;
    _ = pList;
    nMergeSlot = (int32)32;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppSorted))).Value = (uintptr)0;
    ap = Xsqlite3_malloc64(tls, (uint64)((uint64)8 * libc.Uint64FromInt32(nMergeSlot)));
    if (!(ap != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, ap, 0, (uint64)8 * libc.Uint64FromInt32(nMergeSlot));
    iSlot = 0;
    for (;;) {
        if (!(iSlot < (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot)) {
            break;
        }
        pIter = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)iSlot * 8));
        for (;;) {
            if (!(pIter != 0)) {
                break;
            }
            if (pTerm == (uintptr)0 || (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)pIter)).Value.FnKey >= nTerm && 0 == libc.Xmemcmp(tls, pIter + 1 * 48, pTerm, libc.Uint64FromInt32(nTerm))) {
                pEntry = pIter;
                (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)pEntry)).Value.FpScanNext = (uintptr)0;
                i = 0;
                for (;;) {
                    if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)i * 8)) != 0)) {
                        break;
                    }
                    pEntry = _fts5HashEntryMerge(tls, pEntry, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)i * 8)));
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)i * 8))).Value = (uintptr)0;
                    goto _3;
_3:;
                    i = i + 1;
                }
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)i * 8))).Value = pEntry;
            }
            goto _2;
_2:;
            pIter = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)pIter)).Value.FpHashNext;
        }
        goto _1;
_1:;
        iSlot = iSlot + 1;
    }
    pList = (uintptr)0;
    i = 0;
    for (;;) {
        if (!(i < nMergeSlot)) {
            break;
        }
        pList = _fts5HashEntryMerge(tls, pList, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)i * 8)));
        goto _4;
_4:;
        i = i + 1;
    }
    Xsqlite3_free(tls, ap);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppSorted))).Value = pList;
    return SQLITE_OK;
}

internal static int32 /*r*/ _fts5HashResize(ж<libc.TLS> tls, uintptr pHash) {
    uintptr apNew = default;
    uintptr apOld = default;
    uintptr p = default;
    int32 i = default;
    int32 nNew = default;
    uint32 iHash = default;
    _ = apNew;
    _ = apOld;
    _ = i;
    _ = iHash;
    _ = nNew;
    _ = p;
    nNew = (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot * (int32)2;
    apOld = (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot;
    apNew = Xsqlite3_malloc64(tls, (uint64)(libc.Uint64FromInt32(nNew) * (uint64)8));
    if (!(apNew != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, apNew, 0, libc.Uint64FromInt32(nNew) * (uint64)8);
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot)) {
            break;
        }
        while (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOld + (uintptr)i * 8)) != 0) {
            p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOld + (uintptr)i * 8));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOld + (uintptr)i * 8))).Value = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FpHashNext;
            iHash = _fts5HashKey(ref (tls).DerefOrNull(), nNew, p + 1 * 48, (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnKey);
            (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FpHashNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)iHash * 8));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)iHash * 8))).Value = p;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    Xsqlite3_free(tls, apOld);
    (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot = nNew;
    (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot = apNew;
    return SQLITE_OK;
}

internal static void _fts5IndexExtractColset(ж<libc.TLS> tls, uintptr pRc, uintptr pColset, uintptr pPos, int32 nPos, uintptr pIter) {
    uintptr aCopy = default;
    uintptr p = default;
    uintptr pEnd = default;
    uintptr v1 = default;
    int32 i = default;
    ref var iCurrent = ref heap(new int32(), out var ᏑiCurrent);
    bool v2 = default;
    _ = aCopy;
    _ = i;
    _ = iCurrent;
    _ = p;
    _ = pEnd;
    _ = v1;
    _ = v2;
    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(pRc)) == SQLITE_OK) {
        p = pPos;
        aCopy = p;
        pEnd = p + (uintptr)nPos;
        i = 0;
        iCurrent = 0;
        if ((@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)pColset)).Value.FnCol > (int32)1 && _sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)pRc, (@unsafe.Pointer)(pIter + 40), libc.Uint32FromInt32(nPos)) != 0) {
            return;
        }
        while ((int32)1 != 0) {
            while (~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pColset + 4 + (uintptr)i * 4)) < iCurrent) {
                i = i + 1;
                if (i == (@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)pColset)).Value.FnCol) {
                    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fp;
                    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FnData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fn;
                    return;
                }
            }
            while (p < pEnd && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))) != (int32)0x01) {
                for (;;) {
                    v2 = p < pEnd; if (v2) {
                        v1 = p;
                        p = p + 1;
                    }
                    if (!(v2 && (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))) & (int32)0x80) != 0)) {
                        break;
                    }
                }
            }
            if (~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pColset + 4 + (uintptr)i * 4)) == iCurrent) {
                if ((@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)pColset)).Value.FnCol == (int32)1) {
                    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = aCopy;
                    (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FnData = (int32)((int64)p - (int64)aCopy);
                    return;
                }
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(pIter + 40))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(pIter + 40))).Value.Fn, aCopy, libc.Uint64FromInt64((int64)p - (int64)aCopy));
                v1 = pIter + 40 + 8;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = (int32)((int64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)) + ((int64)p - (int64)aCopy));
            }
            if (p >= pEnd) {
                (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FpData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fp;
                (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fbase.FnData = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.Fposlist.Fn;
                return;
            }
            v1 = p;
            p = p + 1;
            aCopy = v1;
            v1 = p;
            p = p + 1;
            iCurrent = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1)));
            if ((iCurrent & (int32)0x80) != 0) {
                p = p - 1;
                p = p + (uintptr)_sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), p, @unsafe.Pointer.FromPinnedBox(ᏑiCurrent));
            }
        }
    }
}

internal static void _fts5IndexMergeLevel(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer ppStruct, int32 iLvl, @unsafe.Pointer pnRem) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        int32 bOldest = default;
        int32 bTermWritten = default;
        int32 eDetail = default;
        int32 flags = default;
        int32 i = default;
        int32 iSegid = default;
        int32 nInput = default;
        int32 nMove = default;
        int32 nPos = default;
        int32 nRem = default;
        ref var nTerm = ref heap(new int32(), out var ᏑnTerm);
        int32 v1 = default;
        ref var pIter = ref heap(new uintptr(), out var ᏑpIter);
        uintptr pLvl = default;
        uintptr pLvlOut = default;
        uintptr pOld = default;
        uintptr pSeg = default;
        uintptr pSegIter = default;
        uintptr pStruct = default;
        uintptr pTerm = default;
        bool v4 = default;
        TFts5Buffer _ᴛ1 = default;
        TFts5SegWriter _ᴛ2 = default;
        _ = bOldest;
        _ = bTermWritten;
        _ = eDetail;
        _ = flags;
        _ = i;
        _ = iSegid;
        _ = nInput;
        _ = nMove;
        _ = nPos;
        _ = nRem;
        _ = nTerm;
        _ = pIter;
        _ = pLvl;
        _ = pLvlOut;
        _ = pOld;
        _ = pSeg;
        _ = pSegIter;
        _ = pStruct;
        _ = pTerm;
        _ = v1;
        _ = v4;
        pStruct = ~@unsafe.Pointer.To<uintptr>(ppStruct);
        pLvl = pStruct + 32 + (uintptr)iLvl * 16;
        pIter = (uintptr)0;
        if (pnRem != nil) {
            v1 = ~@unsafe.Pointer.To<int32>(pnRem);
        } else {
            v1 = 0;
        }
        nRem = v1;
        eDetail = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail;
        flags = (int32)FTS5INDEX_QUERY_NOOUTPUT;
        bTermWritten = 0;
        libc.Xmemset(tls, bp, 0, (uint64)120);
        libc.Xmemset(tls, bp + 120, 0, (uint64)16);
        if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge != 0) {
            pLvlOut = pStruct + 32 + (uintptr)(iLvl + (int32)1) * 16;
            nInput = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge;
            pSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FaSeg + (uintptr)((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg - (int32)1) * 56;
            _fts5WriteInit(tls, p, bp, (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid);
            (@unsafe.Pointer.To<TFts5SegWriter>(__ccgo_ptr(bp))).Value.Fwriter.Fpgno = (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoLast + (int32)1;
            (@unsafe.Pointer.To<TFts5SegWriter>(__ccgo_ptr(bp))).Value.FiBtPage = 0;
        } else {
            iSegid = _fts5AllocateSegid(tls, p, pStruct);
            if (iLvl == (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel - (int32)1) {
                _fts5StructureAddLevel(tls, p + 60, ppStruct);
                pStruct = ~@unsafe.Pointer.To<uintptr>(ppStruct);
            }
            _fts5StructureExtendLevel(tls, p + 60, pStruct, iLvl + (int32)1, (int32)1, 0);
            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
                return;
            }
            pLvl = pStruct + 32 + (uintptr)iLvl * 16;
            pLvlOut = pStruct + 32 + (uintptr)(iLvl + (int32)1) * 16;
            _fts5WriteInit(tls, p, bp, iSegid);
            pSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FaSeg + (uintptr)(@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg * 56;
            (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg + 1;
            (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoFirst = (int32)1;
            (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid = iSegid;
            (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment + 1;
            nInput = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg;
            if ((@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr > (uint64)0) {
                (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiOrigin1 = (@unsafe.Pointer.To<TFts5StructureSegment>(__ccgo_ptr((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg))).Value.FiOrigin1;
                (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiOrigin2 = (@unsafe.Pointer.To<TFts5StructureSegment>(__ccgo_ptr((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg - (int32)1) * 56))).Value.FiOrigin2;
            }
        }
        bOldest = libc.BoolInt32((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg == (int32)1 && (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel == iLvl + (int32)2);
        _fts5MultiIterNew(tls, p, pStruct, flags, (uintptr)0, (uintptr)0, 0, iLvl, nInput, @unsafe.Pointer.FromBox(ᏑpIter));
        for (;;) {
            if (!(_fts5MultiIterEof(ref (tls).DerefOrNull(), p, pIter) == 0)) {
                break;
            }
            pSegIter = pIter + 104 + (uintptr)(@unsafe.Pointer.To<TFts5CResult>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FaFirst + 1 * 4))).Value.FiFirst * 128;
            pTerm = _fts5MultiIterTerm(ref (tls).DerefOrNull(), pIter, @unsafe.Pointer.FromPinnedBox(ᏑnTerm));
            v4 = nTerm != (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 120))).Value.Fn; if (!v4) {
                if (nTerm <= 0) {
                    v1 = 0;
                } else {
                    v1 = libc.Xmemcmp(tls, pTerm, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 120))).Value.Fp, libc.Uint64FromInt32(nTerm));
                }
            }
            if (v4 || v1 != 0) {
                if (pnRem != nil && (@unsafe.Pointer.To<TFts5SegWriter>(__ccgo_ptr(bp))).Value.FnLeafWritten > nRem) {
                    break;
                }
                _sqlite3Fts5BufferSet(tls, (@unsafe.Pointer)(p + 60), bp + 120, nTerm, pTerm);
                bTermWritten = 0;
            }
            if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSegIter)).Value.FnPos == 0 && (bOldest != 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSegIter)).Value.FbDel) == 0)) {
                goto _2;
            }
            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && bTermWritten == 0) {
                _fts5WriteAppendTerm(tls, p, bp, nTerm, pTerm);
                bTermWritten = (int32)1;
            }
            _fts5WriteAppendRowid(tls, p, bp, _fts5MultiIterRowid(ref (tls).DerefOrNull(), pIter));
            if (eDetail == (int32)FTS5_DETAIL_NONE) {
                if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSegIter)).Value.FbDel != 0) {
                    _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp + 16), libc.Int64FromInt32(0));
                    if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSegIter)).Value.FnPos > 0) {
                        _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp + 16), libc.Int64FromInt32(0));
                    }
                }
            } else {
                nPos = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSegIter)).Value.FnPos * (int32)2 + libc.Int32FromUint8((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSegIter)).Value.FbDel);
                _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp + 16), (int64)nPos);
                _fts5ChunkIterate(tls, p, pSegIter, bp, __ccgo_fp(_fts5MergeChunkCallback));
            }
            goto _2;
_2:;
            _fts5MultiIterNext(tls, p, pIter, 0, 0);
        }
        _fts5WriteFinish(tls, p, bp, (@unsafe.Pointer)(pSeg + 8));
        if (_fts5MultiIterEof(ref (tls).DerefOrNull(), p, pIter) != 0) {
            i = 0;
            for (;;) {
                if (!(i < nInput)) {
                    break;
                }
                pOld = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)i * 56;
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pSeg + 48))).Value += (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pOld)).Value.FnEntry - (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pOld)).Value.FnEntryTombstone;
                _fts5DataRemoveSegment(tls, p, pOld);
                goto _5;
_5:;
                i = i + 1;
            }
            if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg != nInput) {
                nMove = libc.Int32FromUint64(libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg - nInput) * (uint64)56);
                libc.Xmemmove(tls, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg + (uintptr)nInput * 56, libc.Uint64FromInt32(nMove));
            }
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pStruct + 24))).Value -= nInput;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pLvl + 4))).Value -= nInput;
            (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge = 0;
            if ((@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoLast == 0) {
                (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg = (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvlOut)).Value.FnSeg - 1;
                (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnSegment - 1;
            }
        } else {
            _fts5TrimSegments(tls, p, pIter);
            (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnMerge = nInput;
        }
        _fts5MultiIterFree(tls, pIter);
        _sqlite3Fts5BufferFree(tls, bp + 120);
        if (pnRem != nil) {
            (@unsafe.Pointer.To<int32>(pnRem)).Value -= (@unsafe.Pointer.To<TFts5SegWriter>(__ccgo_ptr(bp))).Value.FnLeafWritten;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _fts5MergePrefixLists(ж<libc.TLS> tls, uintptr p, uintptr p1, int32 nBuf, uintptr aBuf) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(1056);
        defer(tls.Free, (nint)1056, ref ᒐ);
        int32 i = default;
        int32 nMerge = default;
        int32 nOut = default;
        int32 nTail = default;
        int32 nTmp = default;
        Ti64 iLastRowid = default;
        ref var iPrev = ref heap(new Ti64(), out var ᏑiPrev);
        ref var pHead = ref heap(new uintptr(), out var ᏑpHead);
        uintptr pI = default;
        uintptr pNext = default;
        uintptr pSave = default;
        uintptr pThis = default;
        uintptr pThis1 = default;
        uintptr pX = default;
        array<TPrefixMerger> _ᴛ1 = new(16);
        TFts5Buffer _ᴛ2 = default;
        TFts5Buffer _ᴛ3 = default;
        _ = i;
        _ = iLastRowid;
        _ = iPrev;
        _ = nMerge;
        _ = nOut;
        _ = nTail;
        _ = nTmp;
        _ = pHead;
        _ = pI;
        _ = pNext;
        _ = pSave;
        _ = pThis;
        _ = pThis1;
        _ = pX;
        pHead = (uintptr)0;
        nOut = 0;
        (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1024))).Value = new TFts5Buffer();
        (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1040))).Value = new TFts5Buffer();
        iLastRowid = 0;
        libc.Xmemset(tls, bp, 0, (uint64)64 * libc.Uint64FromInt32(nBuf + libc.Int32FromInt32(1)));
        pHead = bp + (uintptr)nBuf * 64;
        _fts5DoclistIterInit(tls, p1, pHead);
        i = 0;
        for (;;) {
            if (!(i < nBuf)) {
                break;
            }
            _fts5DoclistIterInit(tls, aBuf + (uintptr)i * 16, bp + (uintptr)i * 64);
            _fts5PrefixMergerInsertByRowid(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpHead), bp + (uintptr)i * 64);
            nOut = nOut + (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(aBuf + (uintptr)i * 16))).Value.Fn;
            goto _1;
_1:;
            i = i + 1;
        }
        if (nOut == 0) {
            return;
        }
        nOut = nOut + ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)p1)).Value.Fn + (int32)9 + (int32)10 * nBuf);
        if (_sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp + 1024), libc.Uint32FromInt32(nOut)) != 0) {
            return;
        }
        while (pHead != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 1024 + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fn), libc.Uint64FromInt64((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.Fiter.FiRowid) - libc.Uint64FromInt64(iLastRowid));
            iLastRowid = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.Fiter.FiRowid;
            if ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FpNext != 0 && iLastRowid == (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)(@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FpNext)).Value.Fiter.FiRowid) {
                iPrev = 0;
                nTmp = (int32)FTS5_DATA_ZERO_PADDING;
                nMerge = 0;
                pSave = pHead;
                pThis = (uintptr)0;
                nTail = 0;
                pHead = (uintptr)0;
                while (pSave != 0 && (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.Fiter.FiRowid == iLastRowid) {
                    pNext = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.FpNext;
                    (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.FiOff = 0;
                    (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.FiPos = 0;
                    (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.FaPos = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.Fiter.FaPoslist + (uintptr)(@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.Fiter.FnSize;
                    _sqlite3Fts5PoslistNext64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.FaPos, (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.Fiter.FnPoslist, (@unsafe.Pointer)(pSave + 40), (@unsafe.Pointer)(pSave + 32));
                    nTmp = nTmp + ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pSave)).Value.Fiter.FnPoslist + (int32)10);
                    nMerge = nMerge + 1;
                    _fts5PrefixMergerInsertByPosition(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpHead), pSave);
                    pSave = pNext;
                }
                if (pHead == (uintptr)0 || (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FpNext == (uintptr)0) {
                    _fts5IndexCorruptIdx(tls, p);
                    break;
                }
                if (_sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp + 1040), libc.Uint32FromInt32(nTmp + nMerge * (int32)10)) != 0) {
                    break;
                }
                _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 1040));
                pThis = pHead;
                pHead = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FpNext;
                _sqlite3Fts5PoslistSafeAppend(tls, __ccgo_ptr(bp + 1040), @unsafe.Pointer.FromPinnedBox(ᏑiPrev), (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FiPos);
                _sqlite3Fts5PoslistNext64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FaPos, (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.Fiter.FnPoslist, (@unsafe.Pointer)(pThis + 40), (@unsafe.Pointer)(pThis + 32));
                _fts5PrefixMergerInsertByPosition(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpHead), pThis);
                while ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FpNext != 0) {
                    pThis = pHead;
                    if ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FiPos != iPrev) {
                        _sqlite3Fts5PoslistSafeAppend(tls, __ccgo_ptr(bp + 1040), @unsafe.Pointer.FromPinnedBox(ᏑiPrev), (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FiPos);
                    }
                    _sqlite3Fts5PoslistNext64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FaPos, (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.Fiter.FnPoslist, (@unsafe.Pointer)(pThis + 40), (@unsafe.Pointer)(pThis + 32));
                    pHead = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis)).Value.FpNext;
                    _fts5PrefixMergerInsertByPosition(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpHead), pThis);
                }
                if ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FiPos != iPrev) {
                    _sqlite3Fts5PoslistSafeAppend(tls, __ccgo_ptr(bp + 1040), @unsafe.Pointer.FromPinnedBox(ᏑiPrev), (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FiPos);
                }
                nTail = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.Fiter.FnPoslist - (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FiOff;
                if ((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1040))).Value.Fn + nTail > nTmp - (int32)FTS5_DATA_ZERO_PADDING) {
                    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                        _fts5IndexCorruptIdx(tls, p);
                    }
                    break;
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 1024 + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fn), libc.Uint64FromInt32(((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1040))).Value.Fn + nTail) * libc.Int32FromInt32(2)));
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fn, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1040))).Value.Fp, libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1040))).Value.Fn));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 1024 + 8))).Value += (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1040))).Value.Fn;
                if (nTail > 0) {
                    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fn, (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FaPos + (uintptr)(@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pHead)).Value.FiOff, libc.Uint64FromInt32(nTail));
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 1024 + 8))).Value += nTail;
                }
                pHead = pSave;
                i = 0;
                for (;;) {
                    if (!(i < nBuf + (int32)1)) {
                        break;
                    }
                    pX = bp + (uintptr)i * 64;
                    if ((@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pX)).Value.Fiter.FaPoslist != 0 && (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pX)).Value.Fiter.FiRowid == iLastRowid) {
                        _fts5DoclistIterNext(ref (tls).DerefOrNull(), pX);
                        _fts5PrefixMergerInsertByRowid(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpHead), pX);
                    }
                    goto _2;
_2:;
                    i = i + 1;
                }
            } else {
                pThis1 = pHead;
                pI = pThis1;
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(bp + 1024))).Value.Fn, (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pI)).Value.FaPoslist, libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pI)).Value.FnPoslist + (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pI)).Value.FnSize));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 1024 + 8))).Value += (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pI)).Value.FnPoslist + (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pI)).Value.FnSize;
                _fts5DoclistIterNext(ref (tls).DerefOrNull(), pI);
                pHead = (@unsafe.Pointer.To<TPrefixMerger>((@unsafe.Pointer)pThis1)).Value.FpNext;
                _fts5PrefixMergerInsertByRowid(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpHead), pThis1);
            }
        }
        _sqlite3Fts5BufferFree(tls, p1);
        _sqlite3Fts5BufferFree(tls, bp + 1040);
        libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1024))).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1024))).Value.Fn, 0, (uint64)FTS5_DATA_ZERO_PADDING);
        (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(p1))).Value = (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 1024))).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _fts5MergeRowidLists(ж<libc.TLS> tls, uintptr p, uintptr p1, int32 nBuf, uintptr aBuf) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var i1 = ref heap(new int32(), out var Ꮡi1);
        ref var i2 = ref heap(new int32(), out var Ꮡi2);
        Ti64 iOut = default;
        ref var iRowid1 = ref heap(new Ti64(), out var ᏑiRowid1);
        ref var iRowid2 = ref heap(new Ti64(), out var ᏑiRowid2);
        uintptr p2 = default;
        TFts5Buffer _ᴛ1 = default;
        _ = i1;
        _ = i2;
        _ = iOut;
        _ = iRowid1;
        _ = iRowid2;
        _ = p2;
        i1 = 0;
        i2 = 0;
        iRowid1 = 0;
        iRowid2 = 0;
        iOut = 0;
        p2 = aBuf;
        _ = nBuf;
        libc.Xmemset(tls, bp, 0, (uint64)16);
        _sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)p1)).Value.Fn + (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)p2)).Value.Fn + (int32)9));
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
            return;
        }
        _fts5NextRowid(ref (tls).DerefOrNull(), p1, @unsafe.Pointer.FromPinnedBox(Ꮡi1), @unsafe.Pointer.FromPinnedBox(ᏑiRowid1));
        _fts5NextRowid(ref (tls).DerefOrNull(), p2, @unsafe.Pointer.FromPinnedBox(Ꮡi2), @unsafe.Pointer.FromPinnedBox(ᏑiRowid2));
        while (i1 >= 0 || i2 >= 0) {
            if (i1 >= 0 && (i2 < 0 || iRowid1 < iRowid2)) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fn), libc.Uint64FromInt64(iRowid1) - libc.Uint64FromInt64(iOut));
                iOut = iRowid1;
                _fts5NextRowid(ref (tls).DerefOrNull(), p1, @unsafe.Pointer.FromPinnedBox(Ꮡi1), @unsafe.Pointer.FromPinnedBox(ᏑiRowid1));
            } else {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fn), libc.Uint64FromInt64(iRowid2) - libc.Uint64FromInt64(iOut));
                iOut = iRowid2;
                if (i1 >= 0 && iRowid1 == iRowid2) {
                    _fts5NextRowid(ref (tls).DerefOrNull(), p1, @unsafe.Pointer.FromPinnedBox(Ꮡi1), @unsafe.Pointer.FromPinnedBox(ᏑiRowid1));
                }
                _fts5NextRowid(ref (tls).DerefOrNull(), p2, @unsafe.Pointer.FromPinnedBox(Ꮡi2), @unsafe.Pointer.FromPinnedBox(ᏑiRowid2));
            }
        }
        _fts5BufferSwap(ref (tls).DerefOrNull(), __ccgo_ptr(bp), p1);
        _sqlite3Fts5BufferFree(tls, bp);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _fts5NewTokenizerModule(ж<libc.TLS> tls, uintptr pGlobal, uintptr zName, uintptr pUserData, uintptr __ccgo_fp_xDestroy, @unsafe.Pointer ppNew) {
    Tsqlite3_int64 nByte = default;
    Tsqlite3_int64 nName = default;
    uintptr pNew = default;
    uintptr v1 = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = nByte;
    _ = nName;
    _ = pNew;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    nName = libc.Int64FromUint64(libc.Xstrlen(tls, zName) + (uint64)1);
    nByte = libc.Int64FromUint64((uint64)96 + libc.Uint64FromInt64(nName));
    v1 = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), nByte);
    pNew = v1;
    (@unsafe.Pointer.To<uintptr>(ppNew)).Value = v1;
    if (pNew != 0) {
        (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pNew)).Value.FzName = pNew + 1 * 96;
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pNew)).Value.FzName, zName, libc.Uint64FromInt64(nName));
        (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pNew)).Value.FpUserData = pUserData;
        (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pNew)).Value.FxDestroy = __ccgo_fp_xDestroy;
        (@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pNew)).Value.FpNext = (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpTok;
        (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpTok = pNew;
        if ((@unsafe.Pointer.To<TFts5TokenizerModule>((@unsafe.Pointer)pNew)).Value.FpNext == (uintptr)0) {
            (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpDfltTok = pNew;
        }
    }
    return rc;
}

internal static int32 /*r*/ _fts5OpenMethod__ccgo_gp(ж<libc.TLS> tls, uintptr pVTab, @unsafe.Pointer ppCsr) {
    Tsqlite3_int64 nByte = default;
    uintptr pConfig = default;
    uintptr pCsr = default;
    uintptr pGlobal = default;
    uintptr pTab = default;
    uintptr v2 = default;
    int32 rc = default;
    Ti64 v1 = default;
    _ = nByte;
    _ = pConfig;
    _ = pCsr;
    _ = pGlobal;
    _ = pTab;
    _ = rc;
    _ = v1;
    _ = v2;
    pTab = pVTab;
    pConfig = (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig;
    pCsr = (uintptr)0;
    rc = _fts5NewTransaction(tls, pTab);
    if (rc == SQLITE_OK) {
        nByte = libc.Int64FromUint64((uint64)184 + libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol) * (uint64)4);
        pCsr = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (pCsr != 0) {
            pGlobal = (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpGlobal;
            libc.Xmemset(tls, pCsr, 0, libc.Uint64FromInt64(nByte));
            (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FaColumnSize = pCsr + 1 * 184;
            (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpNext = (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpCsr;
            (@unsafe.Pointer.To<TFts5Global>((@unsafe.Pointer)pGlobal)).Value.FpCsr = pCsr;
            v2 = pGlobal + 56;
            (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FiCsrId = v1;
        } else {
            rc = (int32)SQLITE_NOMEM;
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppCsr)).Value = pCsr;
    return rc;
}

internal static int32 /*r*/ _fts5ParseTokenize__ccgo_gp(ж<libc.TLS> tls, @unsafe.Pointer pContext, int32 tflags, uintptr pToken, int32 nToken, int32 iUnused1, int32 iUnused2) {
    int32 SZALLOC = default;
    int32 nNew = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 v1 = default;
    Tsqlite3_int64 nByte = default;
    @unsafe.Pointer pCtx = default;
    uintptr pNew = default;
    uintptr pPhrase = default;
    uintptr pSyn = default;
    uintptr pTerm = default;
    uintptr v3 = default;
    _ = SZALLOC;
    _ = nByte;
    _ = nNew;
    _ = pCtx;
    _ = pNew;
    _ = pPhrase;
    _ = pSyn;
    _ = pTerm;
    _ = rc;
    _ = v1;
    _ = v3;
    rc = SQLITE_OK;
    SZALLOC = (int32)8;
    pCtx = pContext;
    pPhrase = ~@unsafe.Pointer.To<uintptr>(pCtx);
    _ = iUnused1;
    _ = iUnused2;
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pCtx, 16)) != SQLITE_OK) {
        return ~@unsafe.Pointer.To<int32>(@unsafe.Add(pCtx, 16));
    }
    if (nToken > (int32)FTS5_MAX_TOKEN_SIZE) {
        nToken = (int32)FTS5_MAX_TOKEN_SIZE;
    }
    if (pPhrase != 0 && (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm > 0 && (tflags & (int32)FTS5_TOKEN_COLOCATED) != 0) {
        nByte = libc.Int64FromUint64(libc.Uint64FromInt64(40) + libc.Uint64FromInt64(16) + libc.Uint64FromInt32(nToken) + (uint64)1);
        pSyn = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (pSyn == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            libc.Xmemset(tls, pSyn, 0, libc.Uint64FromInt64(nByte));
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FpTerm = pSyn + (uintptr)40 + (uintptr)16;
            v1 = nToken;
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FnQueryTerm = v1;
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FnFullTerm = v1;
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FpTerm, pToken, libc.Uint64FromInt32(nToken));
            if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCtx, 8))))).Value.FbTokendata != 0) {
                (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FnQueryTerm = libc.Int32FromUint64(libc.Xstrlen(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FpTerm));
            }
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pSyn)).Value.FpSynonym = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32 + (uintptr)((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm - (int32)1) * 40))).Value.FpSynonym;
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pPhrase + 32 + (uintptr)((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm - (int32)1) * 40))).Value.FpSynonym = pSyn;
        }
    } else {
        if (pPhrase == (uintptr)0 || rem((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm, SZALLOC) == 0) {
            if (pPhrase != 0) {
                v1 = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm;
            } else {
                v1 = 0;
            }
            nNew = SZALLOC + v1;
            pNew = Xsqlite3_realloc64(tls, pPhrase, (uint64)((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(nNew + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(40)));
            if (pNew == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                if (pPhrase == (uintptr)0) {
                    libc.Xmemset(tls, pNew, 0, (uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(40));
                }
                v3 = pNew;
                pPhrase = v3;
                (@unsafe.Pointer.To<uintptr>(pCtx)).Value = v3;
                (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pNew)).Value.FnTerm = nNew - SZALLOC;
            }
        }
        if (rc == SQLITE_OK) {
            v3 = pPhrase + 24;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
            pTerm = pPhrase + 32 + (uintptr)v1 * 40;
            libc.Xmemset(tls, pTerm, 0, (uint64)40);
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpTerm = _sqlite3Fts5Strndup(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), pToken, nToken);
            v1 = nToken;
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FnQueryTerm = v1;
            (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FnFullTerm = v1;
            if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCtx, 8))))).Value.FbTokendata != 0 && rc == SQLITE_OK) {
                (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FnQueryTerm = libc.Int32FromUint64(libc.Xstrlen(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)pTerm)).Value.FpTerm));
            }
        }
    }
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pCtx, 16))).Value = rc;
    return rc;
}

internal static int32 /*r*/ _fts5PoslistBlob(ж<libc.TLS> tls, uintptr pCtx, uintptr pCsr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var dummy = ref heap(new uintptr(), out var Ꮡdummy);
        ref var dummy1 = ref heap(new uintptr(), out var Ꮡdummy1);
        ref var pPoslist = ref heap(new uintptr(), out var ᏑpPoslist);
        ref var pPoslist1 = ref heap(new uintptr(), out var ᏑpPoslist1);
        int32 i = default;
        int32 nByte = default;
        ref var nByte1 = ref heap(new int32(), out var ᏑnByte1);
        int32 nPhrase = default;
        int32 nPoslist = default;
        ref var nPoslist1 = ref heap(new int32(), out var ᏑnPoslist1);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        TFts5Buffer _ᴛ1 = default;
        _ = dummy;
        _ = dummy1;
        _ = i;
        _ = nByte;
        _ = nByte1;
        _ = nPhrase;
        _ = nPoslist;
        _ = nPoslist1;
        _ = pPoslist;
        _ = pPoslist1;
        _ = rc;
        rc = SQLITE_OK;
        nPhrase = _sqlite3Fts5ExprPhraseCount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr);
        libc.Xmemset(tls, bp, 0, (uint64)16);
        switch ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab)).Value.FpConfig)).Value.FeDetail) {
        case var ᴛ when ᴛ == FTS5_DETAIL_FULL: {
            goto _1;
            break;
        }
        case var ᴛ when ᴛ == (int32)FTS5_DETAIL_COLUMNS: {
            goto _2;
            break;
        }
        default: {
            goto _3;
            break;
        }}

        goto _4;
_1:;
        i = 0;
_7:;
        if (!(i < nPhrase - (int32)1)) {
            goto _5;
        }
        nByte = _sqlite3Fts5ExprPoslist(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, i, @unsafe.Pointer.FromBox(Ꮡdummy));
        _sqlite3Fts5BufferAppendVarint(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ptr(bp), (int64)nByte);
        goto _6;
_6:;
        i = i + 1;
        goto _7;
        goto _5;
_5:;
        i = 0;
        for (;;) {
            if (!(i < nPhrase)) {
                break;
            }
            nPoslist = _sqlite3Fts5ExprPoslist(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, i, @unsafe.Pointer.FromBox(ᏑpPoslist));
            _sqlite3Fts5BufferAppendBlob(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), bp, libc.Uint32FromInt32(nPoslist), pPoslist);
            goto _8;
_8:;
            i = i + 1;
        }
        goto _4;
_2:;
        i = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && i < nPhrase - (int32)1)) {
                break;
            }
            rc = _sqlite3Fts5ExprPhraseCollist(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, i, @unsafe.Pointer.FromBox(Ꮡdummy1), @unsafe.Pointer.FromPinnedBox(ᏑnByte1));
            _sqlite3Fts5BufferAppendVarint(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ptr(bp), (int64)nByte1);
            goto _9;
_9:;
            i = i + 1;
        }
        i = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && i < nPhrase)) {
                break;
            }
            rc = _sqlite3Fts5ExprPhraseCollist(tls, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpExpr, i, @unsafe.Pointer.FromBox(ᏑpPoslist1), @unsafe.Pointer.FromPinnedBox(ᏑnPoslist1));
            _sqlite3Fts5BufferAppendBlob(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), bp, libc.Uint32FromInt32(nPoslist1), pPoslist1);
            goto _10;
_10:;
            i = i + 1;
        }
        goto _4;
_3:;
        goto _4;
_4:;
        Xsqlite3_result_blob(tls, pCtx, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn, __ccgo_fp(Xsqlite3_free));
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5PoslistCallback(ж<libc.TLS> tls, uintptr pUnused, uintptr pContext, uintptr pChunk, int32 nChunk) {
    _ = pUnused;
    if (nChunk > 0) {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pContext)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pContext)).Value.Fn, pChunk, libc.Uint64FromInt32(nChunk));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pContext + 8))).Value += nChunk;
    }
}

internal static void _fts5PoslistFilterCallback(ж<libc.TLS> tls, uintptr pUnused, uintptr pContext, uintptr pChunk, int32 nChunk) {
    int32 i = default;
    ref var iCol = ref heap(new int32(), out var ᏑiCol);
    ref var iCol1 = ref heap(new int32(), out var ᏑiCol1);
    int32 iEnd = default;
    int32 iStart = default;
    int32 v1 = default;
    uintptr pCtx = default;
    _ = i;
    _ = iCol;
    _ = iCol1;
    _ = iEnd;
    _ = iStart;
    _ = pCtx;
    _ = v1;
    pCtx = pContext;
    _ = pUnused;
    if (nChunk > 0) {
        i = 0;
        iStart = 0;
        if ((@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState == (int32)2) {
            v1 = i;
            i = i + 1;
            iCol = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pChunk + (uintptr)v1)));
            if ((iCol & (int32)0x80) != 0) {
                i = i - 1;
                i = i + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), pChunk + (uintptr)i, @unsafe.Pointer.FromPinnedBox(ᏑiCol));
            }
            if (_fts5IndexColsetTest(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpColset, iCol) != 0) {
                (@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState = (int32)1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf + 8))).Value += _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf)).Value.Fn), libc.Uint64FromInt32(libc.Int32FromInt32(1)));
            } else {
                (@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState = 0;
            }
        }
        for (var cond = true; cond; cond = i < nChunk) {
            while (i < nChunk && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pChunk + (uintptr)i))) != (int32)0x01) {
                iEnd = i + (int32)9;
                for (;;) {
                    v1 = i;
                    i = i + 1;
                    if (!((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pChunk + (uintptr)v1))) & (int32)0x80) != 0 && i < iEnd)) {
                        break;
                    }
                }
            }
            if ((@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState != 0) {
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf)).Value.Fn, pChunk + (uintptr)iStart, libc.Uint64FromInt32(i - iStart));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf + 8))).Value += i - iStart;
            }
            if (i < nChunk) {
                iStart = i;
                i = i + 1;
                if (i >= nChunk) {
                    (@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState = (int32)2;
                } else {
                    v1 = i;
                    i = i + 1;
                    iCol1 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pChunk + (uintptr)v1)));
                    if ((iCol1 & (int32)0x80) != 0) {
                        i = i - 1;
                        i = i + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), pChunk + (uintptr)i, @unsafe.Pointer.FromPinnedBox(ᏑiCol1));
                    }
                    (@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState = _fts5IndexColsetTest(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpColset, iCol1);
                    if ((@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FeState != 0) {
                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)(@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf)).Value.Fn, pChunk + (uintptr)iStart, libc.Uint64FromInt32(i - iStart));
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TPoslistCallbackCtx>((@unsafe.Pointer)pCtx)).Value.FpBuf + 8))).Value += i - iStart;
                        iStart = i;
                    }
                }
            }
        }
    }
}

internal static void _fts5SecureDeleteOverflow(ж<libc.TLS> tls, uintptr p, uintptr pSeg, int32 iPgno, @unsafe.Pointer pbLastInDoclist) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aIdx = default;
        uintptr aPg = default;
        uintptr pLeaf = default;
        int32 bDetailNone = default;
        int32 i1 = default;
        int32 i2 = default;
        ref var iFirst = ref heap(new int32(), out var ᏑiFirst);
        ref var iNext = ref heap(new int32(), out var ᏑiNext);
        int32 nIdx = default;
        int32 nPg = default;
        int32 nShift = default;
        int32 pgno = default;
        Ti64 iRowid = default;
        array<Tu8> _ᴛ1 = new(4);
        _ = aIdx;
        _ = aPg;
        _ = bDetailNone;
        _ = i1;
        _ = i2;
        _ = iFirst;
        _ = iNext;
        _ = iRowid;
        _ = nIdx;
        _ = nPg;
        _ = nShift;
        _ = pLeaf;
        _ = pgno;
        bDetailNone = libc.BoolInt32((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail == libc.Int32FromInt32(FTS5_DETAIL_NONE));
        pLeaf = (uintptr)0;
        (@unsafe.Pointer.To<int32>(pbLastInDoclist)).Value = (int32)1;
        pgno = iPgno;
        for (;;) {
            if (!((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && pgno <= (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FpgnoLast)) {
                break;
            }
            iRowid = ((int64)(@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)pgno;
            iNext = 0;
            aPg = (uintptr)0;
            pLeaf = _fts5LeafRead(tls, p, iRowid);
            if (pLeaf == (uintptr)0) {
                break;
            }
            aPg = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fp;
            iNext = libc.Int32FromUint16(_fts5GetU16(ref (tls).DerefOrNull(), aPg));
            if (iNext != 0) {
                (@unsafe.Pointer.To<int32>(pbLastInDoclist)).Value = 0;
            }
            if (iNext == 0 && (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf < (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn) {
                _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aPg + (uintptr)(@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf, @unsafe.Pointer.FromPinnedBox(ᏑiNext));
            }
            if (iNext == 0) {
                @unsafe.Pointer.StoreArray<Tu8>(__ccgo_ptr(bp), 4, new array<Tu8>(4){
                    [3] = (uint8)0x04
                });
                if (bDetailNone == 0) {
                    _fts5DataWrite(tls, p, iRowid, bp, (int32)4);
                }
                _fts5DataRelease(tls, pLeaf);
                pLeaf = (uintptr)0;
            } else {
                if (bDetailNone != 0) {
                    break;
                } else {
                    if (iNext >= (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf || (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn < (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf || iNext < (int32)4) {
                        _fts5IndexCorruptRowid(tls, p, iRowid);
                        break;
                    } else {
                        nShift = iNext - (int32)4;
                        nIdx = 0;
                        aIdx = (uintptr)0;
                        if ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn > (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf) {
                            iFirst = 0;
                            i1 = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf;
                            i2 = 0;
                            i1 = i1 + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), aPg + (uintptr)i1, @unsafe.Pointer.FromPinnedBox(ᏑiFirst));
                            if (iFirst < iNext) {
                                _fts5IndexCorruptRowid(tls, p, iRowid);
                                break;
                            }
                            aIdx = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)(p + 60), (int64)((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn - (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf + (int32)2));
                            if (aIdx == (uintptr)0) {
                                break;
                            }
                            i2 = _sqlite3Fts5PutVarint(tls, (@unsafe.Pointer)aIdx, libc.Uint64FromInt32(iFirst - nShift));
                            if (i1 < (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn) {
                                libc.Xmemcpy(tls, aIdx + (uintptr)i2, aPg + (uintptr)i1, libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn - i1));
                                i2 = i2 + ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn - i1);
                            }
                            nIdx = i2;
                        }
                        nPg = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf - nShift;
                        libc.Xmemmove(tls, aPg + 4, aPg + (uintptr)((int32)4 + nShift), libc.Uint64FromInt32(nPg - (int32)4));
                        _fts5PutU16(ref (tls).DerefOrNull(), aPg + 2, libc.Uint16FromInt32(nPg));
                        if (_fts5GetU16(ref (tls).DerefOrNull(), aPg) != 0) {
                            _fts5PutU16(ref (tls).DerefOrNull(), aPg, (uint16)4);
                        }
                        if (nIdx > 0) {
                            libc.Xmemcpy(tls, aPg + (uintptr)nPg, aIdx, libc.Uint64FromInt32(nIdx));
                            nPg = nPg + nIdx;
                        }
                        Xsqlite3_free(tls, aIdx);
                        _fts5DataWrite(tls, p, iRowid, aPg, nPg);
                        break;
                    }
                }
            }
            goto _1;
_1:;
            pgno = pgno + 1;
        }
        _fts5DataRelease(tls, pLeaf);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _fts5SegiterPoslist(ж<libc.TLS> tls, uintptr p, uintptr pSeg, uintptr pColset, uintptr pBuf) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 v1 = default;
        TPoslistCallbackCtx _ᴛ1 = default;
        TPoslistOffsetsCtx _ᴛ2 = default;
        _ = v1;
        if (libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn) + libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos + libc.Int32FromInt32(FTS5_DATA_ZERO_PADDING)) <= libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.FnSpace)) {
            v1 = 0;
        } else {
            v1 = _sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), (@unsafe.Pointer)pBuf, libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos + (int32)FTS5_DATA_ZERO_PADDING + (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn));
        }
        if (0 == v1) {
            libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn + (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FnPos), 0, (uint64)FTS5_DATA_ZERO_PADDING);
            if (pColset == (uintptr)0) {
                _fts5ChunkIterate(tls, p, pSeg, pBuf, __ccgo_fp(_fts5PoslistCallback));
            } else {
                if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail == FTS5_DETAIL_FULL) {
                    (@unsafe.Pointer.To<TPoslistCallbackCtx>(__ccgo_ptr(bp))).Value.FpBuf = pBuf;
                    (@unsafe.Pointer.To<TPoslistCallbackCtx>(__ccgo_ptr(bp))).Value.FpColset = pColset;
                    (@unsafe.Pointer.To<TPoslistCallbackCtx>(__ccgo_ptr(bp))).Value.FeState = _fts5IndexColsetTest(ref (tls).DerefOrNull(), pColset, 0);
                    _fts5ChunkIterate(tls, p, pSeg, bp, __ccgo_fp(_fts5PoslistFilterCallback));
                } else {
                    libc.Xmemset(tls, bp + 24, 0, (uint64)24);
                    (@unsafe.Pointer.To<TPoslistOffsetsCtx>(__ccgo_ptr(bp + 24))).Value.FpBuf = pBuf;
                    (@unsafe.Pointer.To<TPoslistOffsetsCtx>(__ccgo_ptr(bp + 24))).Value.FpColset = pColset;
                    _fts5ChunkIterate(tls, p, pSeg, bp + 24, __ccgo_fp(_fts5PoslistOffsetsCallback));
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _fts5SetupPrefixIter(ж<libc.TLS> tls, uintptr p, int32 bDesc, int32 iIdx, uintptr pToken, int32 nToken, uintptr pColset, @unsafe.Pointer ppIter) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        int32 i = default;
        int32 iFree = default;
        int32 v3 = default;
        uintptr pCtx = default;
        uintptr pData = default;
        uintptr pStruct = default;
        TPrefixSetupCtx _ᴛ1 = default;
        TTokendataSetupCtx _ᴛ2 = default;
        _ = i;
        _ = iFree;
        _ = pCtx;
        _ = pData;
        _ = pStruct;
        _ = v3;
        libc.Xmemset(tls, bp, 0, (uint64)72);
        libc.Xmemset(tls, bp + 72, 0, (uint64)16);
        (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnMerge = (int32)1;
        (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FiLastRowid = 0;
        (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnBuf = (int32)32;
        if (iIdx == 0 && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail == FTS5_DETAIL_FULL && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FbPrefixInsttoken != 0) {
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FpTokendata = bp + 72;
            (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 72))).Value.FpT = _fts5IdxMalloc(tls, p, libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 72) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(104)));
        }
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail == (int32)FTS5_DETAIL_NONE) {
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FxMerge = __ccgo_fp(_fts5MergeRowidLists);
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FxAppend = __ccgo_fp(_fts5AppendRowid);
        } else {
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnMerge = libc.Int32FromInt32(FTS5_MERGE_NLIST) - libc.Int32FromInt32(1);
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnBuf = (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnMerge * (int32)8;
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FxMerge = __ccgo_fp(_fts5MergePrefixLists);
            (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FxAppend = __ccgo_fp(_fts5AppendPoslist);
        }
        (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FaBuf = _fts5IdxMalloc(tls, p, libc.Int64FromUint64((uint64)16 * libc.Uint64FromInt32((@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnBuf)));
        pStruct = _fts5StructureRead(tls, p);
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            pCtx = bp;
            if (iIdx != 0) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pToken))).Value = (uint8)'0';
                _fts5VisitEntries(tls, p, pColset, pToken, nToken, 0, __ccgo_fp(_prefixIterSetupCb), pCtx);
            }
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pToken))).Value = libc.Uint8FromInt32((int32)'0' + iIdx);
            _fts5VisitEntries(tls, p, pColset, pToken, nToken, (int32)1, __ccgo_fp(_prefixIterSetupCb), pCtx);
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnBuf)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, int32, uintptr>>((@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FxMerge)(tls, p, bp + 48, (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnMerge, (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FaBuf + (uintptr)i * 16);
                }
                iFree = i;
                for (;;) {
                    if (!(iFree < i + (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnMerge)) {
                        break;
                    }
                    _sqlite3Fts5BufferFree(tls, (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FaBuf + (uintptr)iFree * 16);
                    goto _2;
_2:;
                    iFree = iFree + 1;
                }
                goto _1;
_1:;
                i = i + (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FnMerge;
            }
            pData = _fts5IdxMalloc(tls, p, libc.Int64FromUint64((uint64)16 + libc.Uint64FromInt64((int64)(@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.Fdoclist.Fn) + (uint64)FTS5_DATA_ZERO_PADDING));
            if (pData != 0) {
                (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fp = pData + 1 * 16;
                v3 = (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.Fdoclist.Fn;
                (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.FszLeaf = v3;
                (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn = v3;
                if ((@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.Fdoclist.Fn != 0) {
                    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fp, (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.Fdoclist.Fp, libc.Uint64FromInt32((@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.Fdoclist.Fn));
                }
                _fts5MultiIterNew2(tls, p, pData, bDesc, ppIter);
            }
            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FpTokendata != 0) {
                _fts5TokendataIterSortMap(tls, p, (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 72))).Value.FpT);
                (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(ppIter)))).Value.FpTokenDataIter = (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 72))).Value.FpT;
                (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 72))).Value.FpT = (uintptr)0;
            }
        }
        _fts5TokendataIterDelete(tls, (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 72))).Value.FpT);
        _sqlite3Fts5BufferFree(tls, bp + 48);
        _fts5StructureRelease(tls, pStruct);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TPrefixSetupCtx>(__ccgo_ptr(bp))).Value.FaBuf);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _fts5SetupPrefixIterTokendata(ж<libc.TLS> tls, uintptr pIter, uintptr pToken, int32 nToken) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr p = default;
        TTokendataSetupCtx _ᴛ1 = default;
        TFts5Buffer _ᴛ2 = default;
        _ = p;
        p = (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpIndex;
        (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value = new TFts5Buffer();
        libc.Xmemset(tls, bp + 16, 0, (uint64)16);
        if (!(libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fn) + libc.Uint32FromInt32(nToken + libc.Int32FromInt32(1)) <= libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.FnSpace))) {
            _sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), libc.Uint32FromInt32(nToken + (int32)1 + (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fn));
        }
        (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 16))).Value.FpT = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)(p + 60), libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 72) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(104)));
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp))).Value = (uint8)'0';
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp + 1, pToken, libc.Uint64FromInt32(nToken));
            (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn = nToken + (int32)1;
            _fts5VisitEntries(tls, p, (uintptr)0, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn, (int32)1, __ccgo_fp(_prefixIterSetupTokendataCb), bp + 16);
            _fts5TokendataIterSortMap(tls, p, (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 16))).Value.FpT);
        }
        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FpTokenDataIter = (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 16))).Value.FpT;
        } else {
            _fts5TokendataIterDelete(tls, (@unsafe.Pointer.To<TTokendataSetupCtx>(__ccgo_ptr(bp + 16))).Value.FpT);
        }
        _sqlite3Fts5BufferFree(tls, bp);
        r = _fts5IndexReturn(ref (tls).DerefOrNull(), p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5StructureAddLevel(ж<libc.TLS> tls, uintptr pRc, @unsafe.Pointer ppStruct) {
    Tsqlite3_int64 nByte = default;
    int32 nLevel = default;
    uintptr pStruct = default;
    _ = nByte;
    _ = nLevel;
    _ = pStruct;
    _fts5StructureMakeWritable(tls, pRc, ppStruct);
    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(pRc)) == SQLITE_OK) {
        pStruct = ~@unsafe.Pointer.To<uintptr>(ppStruct);
        nLevel = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel;
        nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(nLevel + libc.Int32FromInt32(2)) * libc.Uint64FromInt64(16));
        pStruct = Xsqlite3_realloc64(tls, pStruct, libc.Uint64FromInt64(nByte));
        if (pStruct != 0) {
            libc.Xmemset(tls, pStruct + 32 + (uintptr)nLevel * 16, 0, (uint64)16);
            (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel + 1;
            (@unsafe.Pointer.To<uintptr>(ppStruct)).Value = pStruct;
        } else {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pRc))).Value = (int32)SQLITE_NOMEM;
        }
    }
}

internal static void _fts5StructureExtendLevel(ж<libc.TLS> tls, uintptr pRc, uintptr pStruct, int32 iLvl, int32 nExtra, int32 bInsert) {
    uintptr aNew = default;
    uintptr pLvl = default;
    Tsqlite3_int64 nByte = default;
    int32 nMove = default;
    _ = aNew;
    _ = nByte;
    _ = nMove;
    _ = pLvl;
    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(pRc)) == SQLITE_OK) {
        pLvl = pStruct + 32 + (uintptr)iLvl * 16;
        nByte = libc.Int64FromUint64(libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg + nExtra) * (uint64)56);
        aNew = Xsqlite3_realloc64(tls, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg, libc.Uint64FromInt64(nByte));
        if (aNew != 0) {
            if (bInsert == 0) {
                libc.Xmemset(tls, aNew + (uintptr)(@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg * 56, 0, (uint64)56 * libc.Uint64FromInt32(nExtra));
            } else {
                nMove = libc.Int32FromUint64(libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FnSeg) * (uint64)56);
                libc.Xmemmove(tls, aNew + (uintptr)nExtra * 56, aNew, libc.Uint64FromInt32(nMove));
                libc.Xmemset(tls, aNew, 0, (uint64)56 * libc.Uint64FromInt32(nExtra));
            }
            (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg = aNew;
        } else {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pRc))).Value = (int32)SQLITE_NOMEM;
        }
    }
}

internal static void _fts5StructureMakeWritable(ж<libc.TLS> tls, uintptr pRc, @unsafe.Pointer pp) {
    int32 i = default;
    Ti64 nByte = default;
    uintptr p = default;
    uintptr pLvl = default;
    uintptr pNew = default;
    _ = i;
    _ = nByte;
    _ = p;
    _ = pLvl;
    _ = pNew;
    p = ~@unsafe.Pointer.To<uintptr>(pp);
    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(pRc)) == SQLITE_OK && (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnRef > (int32)1) {
        nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnLevel) * libc.Uint64FromInt64(16));
        pNew = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)pRc, nByte);
        if (pNew != 0) {
            libc.Xmemcpy(tls, pNew, p, libc.Uint64FromInt64(nByte));
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnLevel)) {
                    break;
                }
                (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pNew + 32 + (uintptr)i * 16))).Value.FaSeg = (uintptr)0;
                goto _1;
_1:;
                i = i + 1;
            }
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnLevel)) {
                    break;
                }
                pLvl = pNew + 32 + (uintptr)i * 16;
                nByte = libc.Int64FromUint64((uint64)56 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pNew + 32 + (uintptr)i * 16))).Value.FnSeg));
                (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)pRc, nByte);
                if ((@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg == (uintptr)0) {
                    i = 0;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnLevel)) {
                            break;
                        }
                        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(pNew + 32 + (uintptr)i * 16))).Value.FaSeg);
                        goto _3;
_3:;
                        i = i + 1;
                    }
                    Xsqlite3_free(tls, pNew);
                    return;
                }
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)pLvl)).Value.FaSeg, (@unsafe.Pointer.To<TFts5StructureLevel>((@unsafe.Pointer)(p + 32 + (uintptr)i * 16))).Value.FaSeg, libc.Uint64FromInt64(nByte));
                goto _2;
_2:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnRef = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)p)).Value.FnRef - 1;
            (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pNew)).Value.FnRef = (int32)1;
        }
        (@unsafe.Pointer.To<uintptr>(pp)).Value = pNew;
    }
}

internal static void _fts5TokendataIterSortMap(ж<libc.TLS> tls, uintptr p, uintptr pT) {
    uintptr a1 = default;
    uintptr a2 = default;
    uintptr aTmp = default;
    uintptr tmp = default;
    int32 i1 = default;
    int32 n1 = default;
    int32 n2 = default;
    Ti64 nByte = default;
    Ti64 nHalf = default;
    int64 v3 = default;
    int64 v4 = default;
    _ = a1;
    _ = a2;
    _ = aTmp;
    _ = i1;
    _ = n1;
    _ = n2;
    _ = nByte;
    _ = nHalf;
    _ = tmp;
    _ = v3;
    _ = v4;
    aTmp = (uintptr)0;
    nByte = libc.Int64FromUint64(libc.Uint64FromInt64((@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap) * (uint64)24);
    aTmp = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)(p + 60), nByte);
    if (aTmp != 0) {
        a1 = (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FaMap;
        a2 = aTmp;
        nHalf = (int64)1;
        for (;;) {
            if (!(nHalf < (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap)) {
                break;
            }
            i1 = 0;
            for (;;) {
                if (!((int64)i1 < (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap)) {
                    break;
                }
                if (nHalf < (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap - (int64)i1) {
                    v3 = nHalf;
                } else {
                    v3 = (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap - (int64)i1;
                }
                n1 = (int32)v3;
                if (nHalf < (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap - (int64)i1 - (int64)n1) {
                    v4 = nHalf;
                } else {
                    v4 = (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap - (int64)i1 - (int64)n1;
                }
                n2 = (int32)v4;
                _fts5TokendataMerge(tls, a1 + (uintptr)i1 * 24, n1, a1 + (uintptr)(i1 + n1) * 24, n2, a2 + (uintptr)i1 * 24);
                goto _2;
_2:;
                i1 = (int32)((int64)i1 + nHalf * libc.Int64FromInt32(2));
            }
            tmp = a1;
            a1 = a2;
            a2 = tmp;
            goto _1;
_1:;
            nHalf = nHalf * (int64)2;
        }
        if (a1 != (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FaMap) {
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FaMap, a1, (uint64)(libc.Uint64FromInt64((@unsafe.Pointer.To<TFts5TokenDataIter>((@unsafe.Pointer)pT)).Value.FnMap) * (uint64)24));
        }
        Xsqlite3_free(tls, aTmp);
    }
}

internal static void _fts5TrimSegments(ж<libc.TLS> tls, uintptr p, uintptr pIter) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 i = default;
        int32 iId = default;
        int32 iOff = default;
        int32 nDiff = default;
        Ti64 iLeafRowid = default;
        uintptr pData = default;
        uintptr pSeg = default;
        array<Tu8> _ᴛ1 = new(4);
        TFts5Buffer _ᴛ2 = default;
        _ = i;
        _ = iId;
        _ = iLeafRowid;
        _ = iOff;
        _ = nDiff;
        _ = pData;
        _ = pSeg;
        libc.Xmemset(tls, bp, 0, (uint64)16);
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pIter)).Value.FnSeg && (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK)) {
                break;
            }
            pSeg = pIter + 104 + (uintptr)i * 128;
            if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg == (uintptr)0) {
            } else {
                if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf == (uintptr)0) {
                    (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg)).Value.FpgnoLast = 0;
                    (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg)).Value.FpgnoFirst = 0;
                } else {
                    iOff = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafOffset;
                    iId = (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg)).Value.FiSegid;
                    @unsafe.Pointer.StoreArray<Tu8>(__ccgo_ptr(bp + 16), 4, new Tu8[]{}.array(4));
                    iLeafRowid = ((int64)iId).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno;
                    pData = _fts5LeafRead(tls, p, iLeafRowid);
                    if (pData != 0) {
                        if (iOff > (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.FszLeaf) {
                            _fts5IndexCorruptRowid(tls, p, iLeafRowid);
                        } else {
                            _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), __ccgo_ptr(bp));
                            if (!(libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fn) + libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn) <= libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.FnSpace))) {
                                _sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn + (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)bp)).Value.Fn));
                            }
                            _sqlite3Fts5BufferAppendBlob(tls, (@unsafe.Pointer)(p + 60), bp, (uint32)4, bp + 16);
                            _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), (int64)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.Fterm.Fn);
                            _sqlite3Fts5BufferAppendBlob(tls, (@unsafe.Pointer)(p + 60), bp, libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.Fterm.Fn), (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.Fterm.Fp);
                            _sqlite3Fts5BufferAppendBlob(tls, (@unsafe.Pointer)(p + 60), bp, libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.FszLeaf - iOff), (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fp + (uintptr)iOff);
                            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                                _fts5PutU16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp + 2, libc.Uint16FromInt32((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn));
                            }
                            _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), libc.Int64FromInt32(4));
                            if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiLeafPgno == (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiEndofDoclist < (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.FszLeaf && (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiPgidxOff <= (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn) {
                                nDiff = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.FszLeaf - (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiEndofDoclist;
                                _sqlite3Fts5BufferAppendVarint(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), (int64)(@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn - (int64)1 - (int64)nDiff - (int64)4);
                                _sqlite3Fts5BufferAppendBlob(tls, (@unsafe.Pointer)(p + 60), bp, libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn - (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiPgidxOff), (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiPgidxOff);
                            }
                            (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpSeg)).Value.FpgnoFirst = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FiTermLeafPgno;
                            _fts5DataDelete(tls, p, ((int64)iId).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)libc.Int32FromInt32(1), iLeafRowid);
                            _fts5DataWrite(tls, p, iLeafRowid, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fn);
                        }
                        _fts5DataRelease(tls, pData);
                    }
                }
            }
            goto _1;
_1:;
            i = i + 1;
        }
        _sqlite3Fts5BufferFree(tls, bp);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _fts5UnicodeAddExceptions(ж<libc.TLS> tls, uintptr p, uintptr z, int32 bTokenChars) {
    uintptr aNew = default;
    uintptr zCsr = default;
    uintptr zTerm = default;
    uintptr v1 = default;
    int32 bToken = default;
    int32 i = default;
    int32 n = default;
    int32 nNew = default;
    int32 rc = default;
    Tu32 iCode = default;
    _ = aNew;
    _ = bToken;
    _ = i;
    _ = iCode;
    _ = n;
    _ = nNew;
    _ = rc;
    _ = zCsr;
    _ = zTerm;
    _ = v1;
    rc = SQLITE_OK;
    n = libc.Int32FromUint64(libc.Xstrlen(tls, z));
    if (n > 0) {
        aNew = Xsqlite3_realloc64(tls, (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaiException, (uint64)(libc.Uint64FromInt32(n + (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FnException) * (uint64)4));
        if (aNew != 0) {
            nNew = (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FnException;
            zCsr = z;
            zTerm = z + (uintptr)n;
            while (zCsr < zTerm) {
                v1 = zCsr;
                zCsr = zCsr + 1;
                iCode = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1)));
                if (iCode >= (uint32)0xc0) {
                    iCode = (uint32)_fts5Utf8Trans1[iCode - (uint32)0xc0];
                    while (zCsr < zTerm && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zCsr))) & (int32)0xc0) == (int32)0x80) {
                        v1 = zCsr;
                        zCsr = zCsr + 1;
                        iCode = (iCode << (int)(libc.Int32FromInt32(6))) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1))));
                    }
                    if (iCode < (uint32)0x80 || (Tu32)(iCode & (uint32)0xFFFFF800U) == (uint32)0xD800 || (Tu32)(iCode & (uint32)0xFFFFFFFEU) == (uint32)0xFFFE) {
                        iCode = (uint32)0xFFFD;
                    }
                }
                if (iCode < (uint32)128) {
                    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + (uintptr)iCode))).Value = libc.Uint8FromInt32(bTokenChars);
                } else {
                    bToken = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 160 + (uintptr)_sqlite3Fts5UnicodeCategory(ref (tls).DerefOrNull(), iCode))));
                    if (bToken != bTokenChars && _sqlite3Fts5UnicodeIsdiacritic(ref (tls).DerefOrNull(), libc.Int32FromUint32(iCode)) == 0) {
                        i = 0;
                        for (;;) {
                            if (!(i < nNew)) {
                                break;
                            }
                            if (libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(aNew + (uintptr)i * 4))) > iCode) {
                                break;
                            }
                            goto _3;
_3:;
                            i = i + 1;
                        }
                        libc.Xmemmove(tls, aNew + (uintptr)(i + (int32)1) * 4, aNew + (uintptr)i * 4, libc.Uint64FromInt32(nNew - i) * (uint64)4);
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(aNew + (uintptr)i * 4))).Value = libc.Int32FromUint32(iCode);
                        nNew = nNew + 1;
                    }
                }
            }
            (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FaiException = aNew;
            (@unsafe.Pointer.To<TUnicode61Tokenizer>((@unsafe.Pointer)p)).Value.FnException = nNew;
        } else {
            rc = (int32)SQLITE_NOMEM;
        }
    }
    return rc;
}

internal static int32 /*r*/ _fts5VocabNextMethod(ж<libc.TLS> tls, uintptr pCursor) {
    int32 bCmp = default;
    int32 eDetail = default;
    int32 iCol = default;
    ref var iOff = ref heap(new int32(), out var ᏑiOff);
    int32 ii1 = default;
    int32 nCmp = default;
    int32 nCol = default;
    int32 nPos = default;
    ref var nTerm = ref heap(new int32(), out var ᏑnTerm);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 v2 = default;
    ref var iPos = ref heap(new Ti64(), out var ᏑiPos);
    Ti64 v3 = default;
    ref var ii = ref heap(new Tu32(), out var Ꮡii);
    uintptr pCsr = default;
    uintptr pPos = default;
    uintptr pTab = default;
    uintptr zTerm = default;
    _ = bCmp;
    _ = eDetail;
    _ = iCol;
    _ = iOff;
    _ = iPos;
    _ = ii;
    _ = ii1;
    _ = nCmp;
    _ = nCol;
    _ = nPos;
    _ = nTerm;
    _ = pCsr;
    _ = pPos;
    _ = pTab;
    _ = rc;
    _ = zTerm;
    _ = v2;
    _ = v3;
    pCsr = pCursor;
    pTab = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pCursor)).Value.FpVtab;
    nCol = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FnCol;
    rc = _sqlite3Fts5StructureTest(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpIndex, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpStruct);
    if (rc != SQLITE_OK) {
        return rc;
    }
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Frowid = (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Frowid + 1;
    if ((@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)pTab)).Value.FeType == (int32)FTS5_VOCAB_INSTANCE) {
        return _fts5VocabInstanceNext(tls, pCsr);
    }
    if ((@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)pTab)).Value.FeType == FTS5_VOCAB_COL) {
        (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol = (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol + 1;
        for (;;) {
            if (!((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol < nCol)) {
                break;
            }
            if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol * 8)) != 0) {
                break;
            }
            goto _1;
_1:;
            (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol = (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol + 1;
        }
    }
    if ((@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)pTab)).Value.FeType != FTS5_VOCAB_COL || (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol >= nCol) {
        if ((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter)).Value.FbEof != 0) {
            (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FbEof = (int32)1;
        } else {
            zTerm = _sqlite3Fts5IterTerm(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter, @unsafe.Pointer.FromPinnedBox(ᏑnTerm));
            if ((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FnLeTerm >= 0) {
                if (nTerm < (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FnLeTerm) {
                    v2 = nTerm;
                } else {
                    v2 = (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FnLeTerm;
                }
                nCmp = v2;
                bCmp = libc.Xmemcmp(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FzLeTerm, zTerm, libc.Uint64FromInt32(nCmp));
                if (bCmp < 0 || bCmp == 0 && (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FnLeTerm < nTerm) {
                    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FbEof = (int32)1;
                    return SQLITE_OK;
                }
            }
            _sqlite3Fts5BufferSet(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), pCsr + 96, nTerm, zTerm);
            libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt, 0, libc.Uint64FromInt32(nCol) * (uint64)8);
            libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc, 0, libc.Uint64FromInt32(nCol) * (uint64)8);
            (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol = 0;
            while (rc == SQLITE_OK) {
                eDetail = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FeDetail;
                iPos = 0;
                iOff = 0;
                pPos = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter)).Value.FpData;
                nPos = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter)).Value.FnData;
                switch ((@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)pTab)).Value.FeType) {
                case var ᴛ when ᴛ == (int32)FTS5_VOCAB_ROW: {
                    if (eDetail == FTS5_DETAIL_FULL && ((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FcolUsed & (int32)0x04) != 0) {
                        while (iPos < (int64)nPos) {
                            v3 = iPos;
                            iPos = iPos + 1;
                            ii = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPos + (uintptr)v3)));
                            if ((Tu32)(ii & (uint32)0x80) != 0) {
                                iPos = iPos - 1;
                                iPos = iPos + (int64)_sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), pPos + (uintptr)iPos, @unsafe.Pointer.FromPinnedBox(Ꮡii));
                            }
                            if (ii == (uint32)1) {
                                v3 = iPos;
                                iPos = iPos + 1;
                                ii = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pPos + (uintptr)v3)));
                                if ((Tu32)(ii & (uint32)0x80) != 0) {
                                    iPos = iPos - 1;
                                    iPos = iPos + (int64)_sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), pPos + (uintptr)iPos, @unsafe.Pointer.FromPinnedBox(Ꮡii));
                                }
                            } else {
                                (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt))).Value = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt)) + 1;
                            }
                        }
                    }
                    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc))).Value = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc)) + 1;
                    break;
                }
                case var ᴛ when ᴛ == FTS5_VOCAB_COL: {
                    if (eDetail == FTS5_DETAIL_FULL) {
                        iCol = -(int32)1;
                        while (0 == _sqlite3Fts5PoslistNext64(ref (tls).DerefOrNull(), pPos, nPos, @unsafe.Pointer.FromPinnedBox(ᏑiOff), @unsafe.Pointer.FromPinnedBox(ᏑiPos))) {
                            ii1 = (int32)((Ti64)(iPos >> (int)(libc.Int32FromInt32(32)) & libc.Int64FromInt32(0x7FFFFFFF)));
                            if (iCol != ii1) {
                                if (ii1 >= nCol) {
                                    rc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
                                    break;
                                }
                                (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)ii1 * 8))).Value = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)ii1 * 8)) + 1;
                                iCol = ii1;
                            }
                            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt + (uintptr)ii1 * 8))).Value = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt + (uintptr)ii1 * 8)) + 1;
                        }
                    } else {
                        if (eDetail == (int32)FTS5_DETAIL_COLUMNS) {
                            while (0 == _sqlite3Fts5PoslistNext64(ref (tls).DerefOrNull(), pPos, nPos, @unsafe.Pointer.FromPinnedBox(ᏑiOff), @unsafe.Pointer.FromPinnedBox(ᏑiPos))) {
                                if (iPos >= (int64)nCol) {
                                    rc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
                                    break;
                                }
                                (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)iPos * 8))).Value = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)iPos * 8)) + 1;
                            }
                        } else {
                            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc))).Value = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc)) + 1;
                        }
                    }
                    break;
                }
                default: {
                    break;
                    break;
                }}

                if (rc == SQLITE_OK) {
                    rc = _sqlite3Fts5IterNextScan(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter);
                }
                if ((@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)pTab)).Value.FeType == (int32)FTS5_VOCAB_INSTANCE) {
                    break;
                }
                if (rc == SQLITE_OK) {
                    zTerm = _sqlite3Fts5IterTerm(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter, @unsafe.Pointer.FromPinnedBox(ᏑnTerm));
                    if (nTerm != (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Fterm.Fn || nTerm > 0 && libc.Xmemcmp(tls, zTerm, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Fterm.Fp, libc.Uint64FromInt32(nTerm)) != 0) {
                        break;
                    }
                    if ((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter)).Value.FbEof != 0) {
                        break;
                    }
                }
            }
        }
    }
    if (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FbEof == 0 && (@unsafe.Pointer.To<TFts5VocabTable>((@unsafe.Pointer)pTab)).Value.FeType == FTS5_VOCAB_COL) {
        for (;;) {
            if (!((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol < nCol && ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc + (uintptr)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol * 8)) == 0)) {
                break;
            }
            goto _5;
_5:;
            (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol = (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol + 1;
        }
        if ((@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol == nCol) {
            rc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        }
    }
    return rc;
}

internal static void _fts5VocabResetCursor(ж<libc.TLS> tls, uintptr pCsr) {
    int32 nCol = default;
    _ = nCol;
    nCol = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpFts5)).Value.FpConfig)).Value.FnCol;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.Frowid = 0;
    _sqlite3Fts5IterClose(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter);
    _sqlite3Fts5StructureRelease(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpStruct);
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpStruct = (uintptr)0;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FpIter = (uintptr)0;
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FzLeTerm);
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FnLeTerm = -(int32)1;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FzLeTerm = (uintptr)0;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FbEof = 0;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiCol = 0;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstPos = 0;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FiInstOff = 0;
    (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FcolUsed = 0;
    libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaCnt, 0, (uint64)8 * libc.Uint64FromInt32(nCol));
    libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5VocabCursor>((@unsafe.Pointer)pCsr)).Value.FaDoc, 0, (uint64)8 * libc.Uint64FromInt32(nCol));
}

internal static int32 /*r*/ _fts5WriteDlidxGrow(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer pWriter, int32 nLvl) {
    uintptr aDlidx = default;
    Tsize_t nByte = default;
    _ = aDlidx;
    _ = nByte;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && nLvl >= ~@unsafe.Pointer.To<int32>(@unsafe.Add(pWriter, 84))) {
        aDlidx = Xsqlite3_realloc64(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pWriter, 88)), (uint64)((uint64)32 * libc.Uint64FromInt32(nLvl)));
        if (aDlidx == (uintptr)0) {
            (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
        } else {
            nByte = (uint64)32 * libc.Uint64FromInt32(nLvl - ~@unsafe.Pointer.To<int32>(@unsafe.Add(pWriter, 84)));
            libc.Xmemset(tls, aDlidx + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(pWriter, 84))) * 32, 0, nByte);
            (@unsafe.Pointer.To<uintptr>(@unsafe.Add(pWriter, 88))).Value = aDlidx;
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pWriter, 84))).Value = nLvl;
        }
    }
    return (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc;
}

internal static uintptr /*r1*/ _geopolyBBox(ж<libc.TLS> tls, uintptr context, uintptr pPoly, uintptr aCoord, @unsafe.Pointer pRc) {
    uintptr r1 = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        float32 mnX = default;
        float32 mnY = default;
        float32 mxX = default;
        float32 mxY = default;
        float32 v3 = default;
        uintptr p = default;
        uintptr pOut = default;
        float64 r = default;
        int32 _ᴛ1 = default;
        _ = mnX;
        _ = mnY;
        _ = mxX;
        _ = mxY;
        _ = p;
        _ = pOut;
        _ = r;
        _ = v3;
        pOut = (uintptr)0;
        if (pPoly == (uintptr)0 && aCoord != (uintptr)0) {
            p = (uintptr)0;
            mnX = ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)aCoord);
            mxX = ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCoord + 1 * 4));
            mnY = ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCoord + 2 * 4));
            mxY = ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCoord + 3 * 4));
            goto geopolyBboxFill;
        } else {
            p = _geopolyFuncParam(tls, context, pPoly, pRc);
        }
        if (!(p != 0)) {
            goto _1;
        }
        v3 = ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2)) * 4));
        mxX = v3;
        mnX = v3;
        v3 = ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4));
        mxY = v3;
        mnY = v3;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)1;
        for (;;) {
            if (!(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) < (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p)).Value.FnVertex)) {
                break;
            }
            r = (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) * (int32)2) * 4)));
            if (r < (float64)mnX) {
                mnX = (float32)r;
            } else {
                if (r > (float64)mxX) {
                    mxX = (float32)r;
                }
            }
            r = (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) * (int32)2 + (int32)1) * 4)));
            if (r < (float64)mnY) {
                mnY = (float32)r;
            } else {
                if (r > (float64)mxY) {
                    mxY = (float32)r;
                }
            }
            goto _5;
_5:;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + 1;
        }
        if (pRc != nil) {
            (@unsafe.Pointer.To<int32>(pRc)).Value = SQLITE_OK;
        }
        if (!(aCoord == (uintptr)0)) {
            goto _6;
        }
        goto geopolyBboxFill;
geopolyBboxFill:;
        pOut = Xsqlite3_realloc64(tls, p, (uint64)(libc.Uint64FromInt64(40) + libc.Uint64FromInt64(4) * libc.Uint64FromInt32(2) * libc.Uint64FromInt32(libc.Int32FromInt32(4) - libc.Int32FromInt32(4))));
        if (pOut == (uintptr)0) {
            Xsqlite3_free(tls, p);
            if (context != 0) {
                Xsqlite3_result_error_nomem(tls, context);
            }
            if (pRc != nil) {
                (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
            }
            r1 = (uintptr)0; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)pOut)).Value.FnVertex = (int32)4;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)1;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4))).Value = (@unsafe.Pointer.To<uint8>(__ccgo_ptr(bp))).Value;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4 + 1))).Value = (uint8)0;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4 + 2))).Value = (uint8)0;
        (@unsafe.Pointer.To<uint8>(__ccgo_ptr(pOut + 4 + 3))).Value = (uint8)4;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2)) * 4))).Value = mnX;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4))).Value = mnY;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(1) * libc.Int32FromInt32(2)) * 4))).Value = mxX;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(1) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4))).Value = mnY;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(2) * libc.Int32FromInt32(2)) * 4))).Value = mxX;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(2) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4))).Value = mxY;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(3) * libc.Int32FromInt32(2)) * 4))).Value = mnX;
        (@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pOut + 8 + (uintptr)(libc.Int32FromInt32(3) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4))).Value = mxY;
        goto _7;
_6:;
        Xsqlite3_free(tls, p);
        (@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)aCoord)).Value = mnX;
        (@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCoord + 1 * 4))).Value = mxX;
        (@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCoord + 2 * 4))).Value = mnY;
        (@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(aCoord + 3 * 4))).Value = mxY;
_7:;
        goto _2;
_1:;
        if (aCoord != 0) {
            libc.Xmemset(tls, aCoord, 0, libc.Uint64FromInt64(4) * libc.Uint64FromInt32(4));
        }
_2:;
        r1 = pOut;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r1;
}

internal static int32 /*r*/ _geopolyFilter(ж<libc.TLS> tls, uintptr pVtabCursor, int32 idxNum, uintptr idxStr, int32 argc, uintptr argv) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var iCell = ref heap(new int32(), out var ᏑiCell);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        ref var iNode = ref heap(new Ti64(), out var ᏑiNode);
        Ti64 iRowid = default;
        uintptr p = default;
        uintptr p1 = default;
        uintptr pCsr = default;
        ref var pLeaf = ref heap(new uintptr(), out var ᏑpLeaf);
        uintptr pNew = default;
        ref var pRoot = ref heap(new uintptr(), out var ᏑpRoot);
        uintptr pRtree = default;
        uintptr v1 = default;
        array<TRtreeCoord> _ᴛ1 = new(4, () => new());
        _ = iCell;
        _ = iNode;
        _ = iRowid;
        _ = p;
        _ = p1;
        _ = pCsr;
        _ = pLeaf;
        _ = pNew;
        _ = pRoot;
        _ = pRtree;
        _ = rc;
        _ = v1;
        pRtree = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pVtabCursor)).Value.FpVtab;
        pCsr = pVtabCursor;
        pRoot = (uintptr)0;
        rc = SQLITE_OK;
        iCell = 0;
        _ = idxStr;
        _rtreeReference(ref (tls).DerefOrNull(), pRtree);
        _resetCursor(tls, pCsr);
        (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FiStrategy = idxNum;
        if (idxNum == (int32)1) {
            iRowid = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
            iNode = 0;
            rc = _findLeafNode(tls, pRtree, iRowid, @unsafe.Pointer.FromBox(ᏑpLeaf), @unsafe.Pointer.FromPinnedBox(ᏑiNode));
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
            if (rc == SQLITE_OK && idxNum <= (int32)3) {
                _geopolyBBox(tls, (uintptr)0, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), bp, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                if (rc != 0) {
                    goto geopoly_filter_end;
                }
                v1 = Xsqlite3_malloc(tls, libc.Int32FromUint64(libc.Uint64FromInt64(24) * libc.Uint64FromInt32(4)));
                p1 = v1;
                (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint = v1;
                (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FnConstraint = (int32)4;
                if (p1 == (uintptr)0) {
                    rc = (int32)SQLITE_NOMEM;
                } else {
                    libc.Xmemset(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FaConstraint, 0, libc.Uint64FromInt64(24) * libc.Uint64FromInt32(4));
                    libc.Xmemset(tls, pCsr + 128, 0, (uint64)4 * libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth + libc.Int32FromInt32(1)));
                    if (idxNum == (int32)2) {
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'B';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = 0;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 1 * 4)));
                        p1 += 24;
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'D';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)1;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)bp));
                        p1 += 24;
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'B';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)2;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 3 * 4)));
                        p1 += 24;
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'D';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)3;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 2 * 4)));
                    } else {
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'D';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = 0;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)bp));
                        p1 += 24;
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'B';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)1;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 1 * 4)));
                        p1 += 24;
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'D';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)2;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 2 * 4)));
                        p1 += 24;
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.Fop = (int32)'B';
                        (@unsafe.Pointer.To<TRtreeConstraint>((@unsafe.Pointer)p1)).Value.FiCoord = (int32)3;
                        (@unsafe.Pointer.To<TRtreeDValue>((@unsafe.Pointer)(p1 + 8))).Value = (float64)(~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 3 * 4)));
                    }
                }
            }
            if (rc == SQLITE_OK) {
                pNew = _rtreeSearchPointNew(tls, pCsr, (float64)0D, libc.Uint8FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiDepth + libc.Int32FromInt32(1)));
                if (pNew == (uintptr)0) {
                    rc = (int32)SQLITE_NOMEM;
                    goto geopoly_filter_end;
                }
                (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pNew)).Value.Fid = (int64)1;
                (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pNew)).Value.FiCell = (uint8)0;
                (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pNew)).Value.FeWithin = (uint8)PARTLY_WITHIN;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCsr + 88))).Value = pRoot;
                pRoot = (uintptr)0;
                rc = _rtreeStepToLeaf(tls, pCsr);
            }
        }
        goto geopoly_filter_end;
geopoly_filter_end:;
        _nodeRelease(tls, pRtree, pRoot);
        _rtreeRelease(tls, pRtree);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _geopolyFuncParam(ж<libc.TLS> tls, uintptr pCtx, uintptr pVal, @unsafe.Pointer pRc) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr a = default;
        uintptr p = default;
        uintptr zJson = default;
        uintptr v4 = default;
        int32 ii = default;
        int32 nByte = default;
        int32 nVertex = default;
        int32 v1 = default;
        bool v2 = default;
        int32 _ᴛ1 = default;
        _ = a;
        _ = ii;
        _ = nByte;
        _ = nVertex;
        _ = p;
        _ = zJson;
        _ = v1;
        _ = v2;
        _ = v4;
        p = (uintptr)0;
        v2 = Xsqlite3_value_type(tls, pVal) == (int32)SQLITE_BLOB; if (v2) {
            v1 = Xsqlite3_value_bytes(tls, pVal);
            nByte = v1;
        }
        if (v2 && v1 >= libc.Int32FromUint64(libc.Uint64FromInt32(4) + libc.Uint64FromInt32(6) * libc.Uint64FromInt64(4))) {
            a = Xsqlite3_value_blob(tls, pVal);
            if (a == (uintptr)0) {
                if (pCtx != 0) {
                    Xsqlite3_result_error_nomem(tls, pCtx);
                }
                r = (uintptr)0; goto ᒐdone;
            }
            nVertex = (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + 1))) << (int)((int32)16)) + (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + 2))) << (int)((int32)8)) + libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a + 3)));
            if ((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a))) == 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a))) == (int32)1) && libc.Uint64FromInt32(nVertex * (int32)2) * (uint64)4 + (uint64)4 == (uint64)libc.Uint32FromInt32(nByte)) {
                p = Xsqlite3_malloc64(tls, (uint64)((uint64)40 + libc.Uint64FromInt32((nVertex - (int32)1) * (int32)2) * (uint64)4));
                if (p == (uintptr)0) {
                    if (pRc != nil) {
                        (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
                    }
                    if (pCtx != 0) {
                        Xsqlite3_result_error_nomem(tls, pCtx);
                    }
                } else {
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)1;
                    (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p)).Value.FnVertex = nVertex;
                    libc.Xmemcpy(tls, p + 4, a, libc.Uint64FromInt32(nByte));
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a))) != libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(bp)))) {
                        ii = 0;
                        for (;;) {
                            if (!(ii < nVertex)) {
                                break;
                            }
                            _geopolySwab32(ref (tls).DerefOrNull(), p + 8 + (uintptr)(ii * (int32)2) * 4);
                            _geopolySwab32(ref (tls).DerefOrNull(), p + 8 + (uintptr)(ii * (int32)2 + (int32)1) * 4);
                            goto _3;
_3:;
                            ii = ii + 1;
                        }
                        v4 = p + 4;
                        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v4)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v4)) ^ libc.Int32FromInt32(1));
                    }
                }
            }
            if (pRc != nil) {
                (@unsafe.Pointer.To<int32>(pRc)).Value = SQLITE_OK;
            }
            r = p; goto ᒐdone;
        } else {
            if (Xsqlite3_value_type(tls, pVal) == (int32)SQLITE_TEXT) {
                zJson = Xsqlite3_value_text(tls, pVal);
                if (zJson == (uintptr)0) {
                    if (pRc != nil) {
                        (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
                    }
                    r = (uintptr)0; goto ᒐdone;
                }
                r = _geopolyParseJson(tls, zJson, pRc); goto ᒐdone;
            } else {
                if (pRc != nil) {
                    (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_ERROR;
                }
                r = (uintptr)0; goto ᒐdone;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _getPageNormal(ж<libc.TLS> tls, uintptr pPager, TPgno pgno, uintptr ppPage, int32 flags) {
    Tu8 noContent = default;
    ref var pBase = ref heap(new uintptr(), out var ᏑpBase);
    uintptr pPg = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = noContent;
    _ = pBase;
    _ = pPg;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    if (pgno == (uint32)0) {
        return _sqlite3CorruptError(tls, (int32)64726);
    }
    pBase = _sqlite3PcacheFetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache, pgno, (int32)3);
    if (pBase == (uintptr)0) {
        pPg = (uintptr)0;
        rc = _sqlite3PcacheFetchStress(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache, pgno, @unsafe.Pointer.FromBox(ᏑpBase));
        if (rc != SQLITE_OK) {
            goto pager_acquire_err;
        }
        if (pBase == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
            goto pager_acquire_err;
        }
    }
    v1 = _sqlite3PcacheFetchFinish(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache, pgno, pBase);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = v1;
    pPg = v1;
    noContent = libc.BoolUint8((flags & (int32)PAGER_GET_NOCONTENT) != 0);
    if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager != 0 && !(noContent != 0)) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248)) + 1;
        return SQLITE_OK;
    } else {
        if (pgno == (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FlckPgno) {
            rc = _sqlite3CorruptError(tls, (int32)64758);
            goto pager_acquire_err;
        }
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager = pPager;
        if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != libc.UintptrFromInt32(0)) || (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize < pgno || noContent != 0) {
            if (pgno > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmxPgno) {
                rc = (int32)SQLITE_FULL;
                if (pgno <= (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize) {
                    _sqlite3PcacheRelease(tls, pPg);
                    pPg = (uintptr)0;
                }
                goto pager_acquire_err;
            }
            if (noContent != 0) {
                _sqlite3BeginBenignMalloc(tls);
                if (pgno <= (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbOrigSize) {
                    _sqlite3BitvecSet(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal, pgno);
                }
                _addToSavepointBitvecs(tls, pPager, pgno);
                _sqlite3EndBenignMalloc(tls);
            }
            libc.Xmemset(tls, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData, 0, libc.Uint64FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize));
        } else {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + 1 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + 1 * 4)) + 1;
            rc = _readDbPage(tls, pPg);
            if (rc != SQLITE_OK) {
                goto pager_acquire_err;
            }
        }
    }
    return SQLITE_OK;
    goto pager_acquire_err;
pager_acquire_err:;
    if (pPg != 0) {
        _sqlite3PcacheDrop(tls, pPg);
    }
    _pagerUnlockIfUnused(tls, pPager);
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = (uintptr)0;
    return rc;
}

internal static void _groupConcatInverse(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    int32 nVS = default;
    uintptr pGCC = default;
    _ = nVS;
    _ = pGCC;
    _ = argc;
    if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))) == (int32)SQLITE_NULL) {
        return;
    }
    pGCC = Xsqlite3_aggregate_context(tls, context, (int32)48);
    if (pGCC != 0) {
        Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        nVS = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pGCC + 32))).Value -= (int32)1;
        if ((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths != (uintptr)0) {
            if ((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum > 0) {
                nVS = nVS + ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths));
                libc.Xmemmove(tls, (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths, (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths + (uintptr)1 * 4, libc.Uint64FromInt32((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnAccum - libc.Int32FromInt32(1)) * (uint64)4);
            }
        } else {
            nVS = nVS + (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FnFirstSepLength;
        }
        if (nVS >= libc.Int32FromUint32((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FnChar)) {
            (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FnChar = (uint32)0;
        } else {
            (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FnChar -= libc.Uint32FromInt32(nVS);
            libc.Xmemmove(tls, (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FzText, (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FzText + (uintptr)nVS, (uint64)(@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FnChar);
        }
        if ((@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FnChar == (uint32)0) {
            (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.Fstr.FmxAlloc = (uint32)0;
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths);
            (@unsafe.Pointer.To<TGroupConcatCtx>((@unsafe.Pointer)pGCC)).Value.FpnSepLengths = (uintptr)0;
        }
    }
}

internal static int32 /*r*/ _growVTrans(ж<libc.TLS> tls, uintptr db) {
    int32 ARRAY_INCR = default;
    uintptr aVTrans = default;
    Tsqlite3_int64 nBytes = default;
    _ = ARRAY_INCR;
    _ = aVTrans;
    _ = nBytes;
    ARRAY_INCR = (int32)5;
    if (rem((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans, ARRAY_INCR) == 0) {
        nBytes = libc.Int64FromUint64((uint64)8 * libc.Uint64FromInt64((int64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans + (int64)ARRAY_INCR));
        aVTrans = _sqlite3DbRealloc(tls, db, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans, libc.Uint64FromInt64(nBytes));
        if (!(aVTrans != 0)) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemset(tls, aVTrans + (uintptr)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans * 8, 0, (uint64)8 * libc.Uint64FromInt32(ARRAY_INCR));
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans = aVTrans;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _insertCellFast(ж<libc.TLS> tls, uintptr pPage, int32 i, uintptr pCell, int32 sz) {
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
    if (sz + (int32)2 > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree) {
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
        libc.Xmemcpy(tls, data + (uintptr)idx, pCell, libc.Uint64FromInt32(sz));
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

internal static void _jsonBlobAppendNode(ж<libc.TLS> tls, uintptr pParse, Tu8 eType, Tu64 szPayload, uintptr aPayload) {
    uintptr a = default;
    uintptr v1 = default;
    _ = a;
    _ = v1;
    if ((uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob + szPayload + (uint64)9 > (uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlobAlloc) {
        _jsonBlobExpandAndAppendNode(tls, pParse, eType, szPayload, aPayload);
        return;
    }
    a = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob;
    if (szPayload <= (uint64)11) {
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = (uint8)((uint64)eType | szPayload << (int)(libc.Int32FromInt32(4)));
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pParse + 8))).Value += (uint32)1;
    } else {
        if (szPayload <= (uint64)0xff) {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(eType) | (int32)0xc0);
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 1))).Value = (uint8)((Tu64)(szPayload & (uint64)0xff));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pParse + 8))).Value += (uint32)2;
        } else {
            if (szPayload <= (uint64)0xffff) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(eType) | (int32)0xd0);
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 1))).Value = (uint8)((Tu64)(szPayload >> (int)(libc.Int32FromInt32(8)) & (uint64)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 2))).Value = (uint8)((Tu64)(szPayload & (uint64)0xff));
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pParse + 8))).Value += (uint32)3;
            } else {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(eType) | (int32)0xe0);
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 1))).Value = (uint8)((Tu64)(szPayload >> (int)(libc.Int32FromInt32(24)) & (uint64)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 2))).Value = (uint8)((Tu64)(szPayload >> (int)(libc.Int32FromInt32(16)) & (uint64)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 3))).Value = (uint8)((Tu64)(szPayload >> (int)(libc.Int32FromInt32(8)) & (uint64)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 4))).Value = (uint8)((Tu64)(szPayload & (uint64)0xff));
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pParse + 8))).Value += (uint32)5;
            }
        }
    }
    if (aPayload != 0) {
        v1 = pParse + 8;
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v1)).Value = (Tu32)((uint64)(~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v1)) + szPayload);
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)((uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob - szPayload), aPayload, szPayload);
    }
}

internal static int32 /*r*/ _jsonBlobChangePayloadSize(ж<libc.TLS> tls, uintptr pParse, Tu32 i, Tu32 szPayload) {
    uintptr a = default;
    int32 delta = default;
    Tu8 nExtra = default;
    Tu8 nNeeded = default;
    Tu8 szType = default;
    Tu64 newSize = default;
    _ = a;
    _ = delta;
    _ = nExtra;
    _ = nNeeded;
    _ = newSize;
    _ = szType;
    if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Foom != 0) {
        return 0;
    }
    a = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)i;
    szType = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) >> (int)((int32)4));
    if (libc.Int32FromUint8(szType) <= (int32)11) {
        nExtra = (uint8)0;
    } else {
        if (libc.Int32FromUint8(szType) == (int32)12) {
            nExtra = (uint8)1;
        } else {
            if (libc.Int32FromUint8(szType) == (int32)13) {
                nExtra = (uint8)2;
            } else {
                if (libc.Int32FromUint8(szType) == (int32)14) {
                    nExtra = (uint8)4;
                } else {
                    nExtra = (uint8)8;
                }
            }
        }
    }
    if (szPayload <= (uint32)11) {
        nNeeded = (uint8)0;
    } else {
        if (szPayload <= (uint32)0xff) {
            nNeeded = (uint8)1;
        } else {
            if (szPayload <= (uint32)0xffff) {
                nNeeded = (uint8)2;
            } else {
                nNeeded = (uint8)4;
            }
        }
    }
    delta = libc.Int32FromUint8(nNeeded) - libc.Int32FromUint8(nExtra);
    if (delta != 0) {
        newSize = (uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob + libc.Uint64FromInt32(delta);
        if (delta > 0) {
            if (newSize > (uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlobAlloc && _jsonBlobExpand(tls, pParse, newSize) != 0) {
                return 0;
            }
            a = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)i;
            libc.Xmemmove(tls, a + (uintptr)((int32)1 + delta), a + 1, (uint64)((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob - (i + (uint32)1)));
        } else {
            libc.Xmemmove(tls, a + 1, a + (uintptr)((int32)1 - delta), (uint64)((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob - (i + (uint32)1 - libc.Uint32FromInt32(delta))));
        }
        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob = (uint32)newSize;
    }
    if (libc.Int32FromUint8(nNeeded) == 0) {
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = (uint8)(libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) & libc.Int32FromInt32(0x0f)) | szPayload << (int)(libc.Int32FromInt32(4)));
    } else {
        if (libc.Int32FromUint8(nNeeded) == (int32)1) {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) & (int32)0x0f | (int32)0xc0);
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 1))).Value = (uint8)((Tu32)(szPayload & (uint32)0xff));
        } else {
            if (libc.Int32FromUint8(nNeeded) == (int32)2) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) & (int32)0x0f | (int32)0xd0);
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 1))).Value = (uint8)((Tu32)(szPayload >> (int)(libc.Int32FromInt32(8)) & (uint32)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 2))).Value = (uint8)((Tu32)(szPayload & (uint32)0xff));
            } else {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) & (int32)0x0f | (int32)0xe0);
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 1))).Value = (uint8)((Tu32)(szPayload >> (int)(libc.Int32FromInt32(24)) & (uint32)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 2))).Value = (uint8)((Tu32)(szPayload >> (int)(libc.Int32FromInt32(16)) & (uint32)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 3))).Value = (uint8)((Tu32)(szPayload >> (int)(libc.Int32FromInt32(8)) & (uint32)0xff));
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + 4))).Value = (uint8)((Tu32)(szPayload & (uint32)0xff));
            }
        }
    }
    return delta;
}

internal static void _jsonBlobEdit(ж<libc.TLS> tls, uintptr pParse, Tu32 iDel, Tu32 nDel, uintptr aIns, Tu32 nIns) {
    Ti64 d = default;
    uintptr v1 = default;
    _ = d;
    _ = v1;
    d = libc.Int64FromUint32(nIns) - libc.Int64FromUint32(nDel);
    if (d < 0 && d >= (int64)(-libc.Int32FromInt32(8)) && aIns != (uintptr)0 && _jsonBlobOverwrite(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)iDel, aIns, nIns, libc.Uint32FromInt32((int32)(-d))) != 0) {
        return;
    }
    if (d != 0) {
        if (libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) + d > libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlobAlloc)) {
            _jsonBlobExpand(tls, pParse, (uint64)(@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob + libc.Uint64FromInt64(d));
            if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Foom != 0) {
                return;
            }
        }
        libc.Xmemmove(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(iDel + nIns), (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(iDel + nDel), (uint64)((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob - (iDel + nDel)));
        v1 = pParse + 8;
        (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v1)).Value = (Tu32)((int64)(~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v1)) + d);
        v1 = pParse + 52;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = (int32)((int64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)) + d);
    }
    if (nIns != 0 && aIns != 0) {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)iDel, aIns, (uint64)nIns);
    }
}

internal static int32 /*r*/ _jsonBlobOverwrite(ж<libc.TLS> tls, uintptr aOut, uintptr aIns, Tu32 nIns, Tu32 d) {
    Tu32 i = default;
    Tu32 szPayload = default;
    Tu8 szHdr = default;
    _ = i;
    _ = szHdr;
    _ = szPayload;
    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aIns))) & (int32)0x0f) <= (int32)2) {
        return 0;
    }
    switch (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aIns))).Rsh((int64)(libc.Int32FromInt32(4)))) {
    default: {
        if ((((int32)1).Lsh((uint64)(d)) & (int32)0x116) == 0) {
            return 0;
        }
        i = d + (uint32)1;
        szHdr = (uint8)1;
        break;
    }
    case var ᴛ when ᴛ == (int32)12: {
        if ((((int32)1).Lsh((uint64)(d)) & (int32)0x8a) == 0) {
            return 0;
        }
        i = d + (uint32)2;
        szHdr = (uint8)2;
        break;
    }
    case var ᴛ when ᴛ == (int32)13: {
        if (d != (uint32)2 && d != (uint32)6) {
            return 0;
        }
        i = d + (uint32)3;
        szHdr = (uint8)3;
        break;
    }
    case var ᴛ when ᴛ == (int32)14: {
        if (d != (uint32)4) {
            return 0;
        }
        i = (uint32)9;
        szHdr = (uint8)5;
        break;
    }
    case var ᴛ when ᴛ == (int32)15: {
        return 0;
    }}

    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aOut))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aIns))) & (int32)0x0f | libc.Int32FromUint8(_aType1[i - (uint32)2]));
    libc.Xmemcpy(tls, aOut + (uintptr)i, aIns + (uintptr)szHdr, (uint64)(nIns - (uint32)szHdr));
    szPayload = nIns - (uint32)szHdr;
    while ((int32)1 != 0) {
        i = i - 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aOut + (uintptr)i))).Value = (uint8)((Tu32)(szPayload & (uint32)0xff));
        if (i == (uint32)1) {
            break;
        }
        szPayload = szPayload >> (int)((uint32)8);
    }
    return (int32)1;
}

internal static int32 /*r*/ _jsonCacheInsert(ж<libc.TLS> tls, uintptr ctx, uintptr pParse) {
    uintptr db = default;
    uintptr p = default;
    _ = db;
    _ = p;
    p = Xsqlite3_get_auxdata(tls, ctx, -(int32)429938);
    if (p == (uintptr)0) {
        db = Xsqlite3_context_db_handle(tls, ctx);
        p = _sqlite3DbMallocZero(tls, db, (uint64)48);
        if (p == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
        (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.Fdb = db;
        Xsqlite3_set_auxdata(tls, ctx, -(int32)429938, p, __ccgo_fp(_jsonCacheDeleteGeneric));
        p = Xsqlite3_get_auxdata(tls, ctx, -(int32)429938);
        if (p == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
    }
    if ((@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed >= (int32)JSON_CACHE_SIZE) {
        _jsonParseFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16)));
        libc.Xmemmove(tls, p + 16, p + 16 + 1 * 8, libc.Uint64FromInt32(libc.Int32FromInt32(JSON_CACHE_SIZE) - libc.Int32FromInt32(1)) * libc.Uint64FromInt64(8));
        (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed = libc.Int32FromInt32(JSON_CACHE_SIZE) - libc.Int32FromInt32(1);
    }
    (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FeEdit = (uint8)0;
    (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnJPRef = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnJPRef + 1;
    (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FbReadOnly = (uint8)1;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)(@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed * 8))).Value = pParse;
    (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed = (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed + 1;
    return SQLITE_OK;
}

internal static uintptr /*r*/ _jsonCacheSearch(ж<libc.TLS> tls, uintptr ctx, uintptr pArg) {
    uintptr r = default;

    int32 i = default;
    int32 nJson = default;
    uintptr p = default;
    uintptr tmp = default;
    uintptr zJson = default;
    _ = i;
    _ = nJson;
    _ = p;
    _ = tmp;
    _ = zJson;
    if (Xsqlite3_value_type(tls, pArg) != (int32)SQLITE_TEXT) {
        return (uintptr)0;
    }
    zJson = Xsqlite3_value_text(tls, pArg);
    if (zJson == (uintptr)0) {
        return (uintptr)0;
    }
    nJson = Xsqlite3_value_bytes(tls, pArg);
    p = Xsqlite3_get_auxdata(tls, ctx, -(int32)429938);
    if (p == (uintptr)0) {
        return (uintptr)0;
    }
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed)) {
            break;
        }
        if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8))))).Value.FzJson == zJson) {
            break;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if (i >= (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed)) {
                break;
            }
            if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8))))).Value.FnJson != nJson) {
                goto _2;
            }
            if (libc.Xmemcmp(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8))))).Value.FzJson, zJson, libc.Uint64FromInt32(nJson)) == 0) {
                break;
            }
            goto _2;
_2:;
            i = i + 1;
        }
    }
    if (i < (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed) {
        if (i < (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed - (int32)1) {
            tmp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8));
            libc.Xmemmove(tls, p + 16 + (uintptr)i * 8, p + 16 + (uintptr)(i + (int32)1) * 8, libc.Uint64FromInt32((@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed - i - libc.Int32FromInt32(1)) * (uint64)8);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)((@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed - (int32)1) * 8))).Value = tmp;
            i = (@unsafe.Pointer.To<TJsonCache>((@unsafe.Pointer)p)).Value.FnUsed - (int32)1;
        }
        return ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)i * 8));
    } else {
        return (uintptr)0;
    }
    return r;
}

internal static int32 /*r*/ _jsonMergePatch(ж<libc.TLS> tls, uintptr pTarget, Tu32 iTarget, uintptr pPatch, Tu32 iPatch, Tu32 iDepth) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu8 ePLabel = default;
        Tu8 eTLabel = default;
        Tu8 x = default;
        Tu32 iPCursor = default;
        Tu32 iPEnd = default;
        Tu32 iPLabel = default;
        Tu32 iPValue = default;
        Tu32 iTCursor = default;
        Tu32 iTEnd = default;
        Tu32 iTEndBE = default;
        Tu32 iTLabel = default;
        Tu32 iTStart = default;
        Tu32 iTValue = default;
        Tu32 n = default;
        Tu32 nPLabel = default;
        Tu32 nPValue = default;
        Tu32 nTLabel = default;
        Tu32 nTValue = default;
        ref var sz = ref heap(new Tu32(), out var Ꮡsz);
        Tu32 szNew = default;
        ref var szPLabel = ref heap(new Tu32(), out var ᏑszPLabel);
        ref var szPValue = ref heap(new Tu32(), out var ᏑszPValue);
        Tu32 szPatch = default;
        ref var szTLabel = ref heap(new Tu32(), out var ᏑszTLabel);
        ref var szTValue = ref heap(new Tu32(), out var ᏑszTValue);
        Tu32 szTarget = default;
        int32 isEqual = default;
        int32 rc = default;
        int32 rc1 = default;
        int32 savedDelta = default;
        int32 savedDelta1 = default;
        int32 v1 = default;
        _ = ePLabel;
        _ = eTLabel;
        _ = iPCursor;
        _ = iPEnd;
        _ = iPLabel;
        _ = iPValue;
        _ = iTCursor;
        _ = iTEnd;
        _ = iTEndBE;
        _ = iTLabel;
        _ = iTStart;
        _ = iTValue;
        _ = isEqual;
        _ = n;
        _ = nPLabel;
        _ = nPValue;
        _ = nTLabel;
        _ = nTValue;
        _ = rc;
        _ = rc1;
        _ = savedDelta;
        _ = savedDelta1;
        _ = sz;
        _ = szNew;
        _ = szPLabel;
        _ = szPValue;
        _ = szPatch;
        _ = szTLabel;
        _ = szTValue;
        _ = szTarget;
        _ = x;
        _ = v1;
        sz = (uint32)0;
        iTLabel = (uint32)0;
        nTLabel = (uint32)0;
        szTLabel = (uint32)0;
        iTValue = (uint32)0;
        nTValue = (uint32)0;
        szTValue = (uint32)0;
        x = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPatch))) & (int32)0x0f);
        if (libc.Int32FromUint8(x) != (int32)JSONB_OBJECT) {
            n = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPatch, iPatch, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
            szPatch = n + sz;
            sz = (uint32)0;
            n = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pTarget, iTarget, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
            szTarget = n + sz;
            _jsonBlobEdit(tls, pTarget, iTarget, szTarget, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPatch, szPatch);
            if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Foom != 0) {
                v1 = (int32)JSON_MERGE_OOM;
            } else {
                v1 = JSON_MERGE_OK;
            }
            r = v1; goto ᒐdone;
        }
        x = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)iTarget))) & (int32)0x0f);
        if (libc.Int32FromUint8(x) != (int32)JSONB_OBJECT) {
            n = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pTarget, iTarget, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
            _jsonBlobEdit(tls, pTarget, iTarget + n, sz, (uintptr)0, (uint32)0);
            x = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)iTarget));
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)iTarget))).Value = libc.Uint8FromInt32(libc.Int32FromUint8(x) & (int32)0xf0 | (int32)JSONB_OBJECT);
        }
        n = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPatch, iPatch, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
        if (n == (uint32)0) {
            r = (int32)JSON_MERGE_BADPATCH; goto ᒐdone;
        }
        iPCursor = iPatch + n;
        iPEnd = iPCursor + sz;
        n = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pTarget, iTarget, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
        if (n == (uint32)0) {
            r = (int32)JSON_MERGE_BADTARGET; goto ᒐdone;
        }
        iTStart = iTarget + n;
        iTEndBE = iTStart + sz;
        while (iPCursor < iPEnd) {
            iPLabel = iPCursor;
            ePLabel = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPCursor))) & (int32)0x0f);
            if (libc.Int32FromUint8(ePLabel) < (int32)JSONB_TEXT || libc.Int32FromUint8(ePLabel) > (int32)JSONB_TEXTRAW) {
                r = (int32)JSON_MERGE_BADPATCH; goto ᒐdone;
            }
            nPLabel = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPatch, iPCursor, @unsafe.Pointer.FromPinnedBox(ᏑszPLabel));
            if (nPLabel == (uint32)0) {
                r = (int32)JSON_MERGE_BADPATCH; goto ᒐdone;
            }
            iPValue = iPCursor + nPLabel + szPLabel;
            if (iPValue >= iPEnd) {
                r = (int32)JSON_MERGE_BADPATCH; goto ᒐdone;
            }
            nPValue = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pPatch, iPValue, @unsafe.Pointer.FromPinnedBox(ᏑszPValue));
            if (nPValue == (uint32)0) {
                r = (int32)JSON_MERGE_BADPATCH; goto ᒐdone;
            }
            iPCursor = iPValue + nPValue + szPValue;
            if (iPCursor > iPEnd) {
                r = (int32)JSON_MERGE_BADPATCH; goto ᒐdone;
            }
            iTCursor = iTStart;
            iTEnd = iTEndBE + libc.Uint32FromInt32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Fdelta);
            while (iTCursor < iTEnd) {
                iTLabel = iTCursor;
                eTLabel = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)iTCursor))) & (int32)0x0f);
                if (libc.Int32FromUint8(eTLabel) < (int32)JSONB_TEXT || libc.Int32FromUint8(eTLabel) > (int32)JSONB_TEXTRAW) {
                    r = (int32)JSON_MERGE_BADTARGET; goto ᒐdone;
                }
                nTLabel = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pTarget, iTCursor, @unsafe.Pointer.FromPinnedBox(ᏑszTLabel));
                if (nTLabel == (uint32)0) {
                    r = (int32)JSON_MERGE_BADTARGET; goto ᒐdone;
                }
                iTValue = iTLabel + nTLabel + szTLabel;
                if (iTValue >= iTEnd) {
                    r = (int32)JSON_MERGE_BADTARGET; goto ᒐdone;
                }
                nTValue = _jsonbPayloadSize(ref (tls).DerefOrNull(), (@unsafe.Pointer)pTarget, iTValue, @unsafe.Pointer.FromPinnedBox(ᏑszTValue));
                if (nTValue == (uint32)0) {
                    r = (int32)JSON_MERGE_BADTARGET; goto ᒐdone;
                }
                if (iTValue + nTValue + szTValue > iTEnd) {
                    r = (int32)JSON_MERGE_BADTARGET; goto ᒐdone;
                }
                isEqual = _jsonLabelCompare(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)(iPLabel + nPLabel), szPLabel, libc.BoolInt32(libc.Int32FromUint8(ePLabel) == (int32)JSONB_TEXT || libc.Int32FromUint8(ePLabel) == (int32)JSONB_TEXTRAW), (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)(iTLabel + nTLabel), szTLabel, libc.BoolInt32(libc.Int32FromUint8(eTLabel) == (int32)JSONB_TEXT || libc.Int32FromUint8(eTLabel) == (int32)JSONB_TEXTRAW));
                if (isEqual != 0) {
                    break;
                }
                iTCursor = iTValue + nTValue + szTValue;
            }
            x = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPValue))) & (int32)0x0f);
            if (iTCursor < iTEnd) {
                if (libc.Int32FromUint8(x) == 0) {
                    _jsonBlobEdit(tls, pTarget, iTLabel, nTLabel + szTLabel + nTValue + szTValue, (uintptr)0, (uint32)0);
                    if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Foom != 0) {
                        r = (int32)JSON_MERGE_OOM; goto ᒐdone;
                    }
                } else {
                    savedDelta = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Fdelta;
                    (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Fdelta = 0;
                    if (iDepth >= (uint32)JSON_MAX_DEPTH) {
                        r = (int32)JSON_MERGE_TOODEEP; goto ᒐdone;
                    }
                    rc = _jsonMergePatch(tls, pTarget, iTValue, pPatch, iPValue, iDepth + (uint32)1);
                    if (rc != 0) {
                        r = rc; goto ᒐdone;
                    }
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pTarget + 52))).Value += savedDelta;
                }
            } else {
                if (libc.Int32FromUint8(x) > 0) {
                    szNew = szPLabel + nPLabel;
                    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPValue))) & (int32)0x0f) != (int32)JSONB_OBJECT) {
                        _jsonBlobEdit(tls, pTarget, iTEnd, (uint32)0, (uintptr)0, szPValue + nPValue + szNew);
                        if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Foom != 0) {
                            r = (int32)JSON_MERGE_OOM; goto ᒐdone;
                        }
                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)iTEnd, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPLabel, (uint64)szNew);
                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)(iTEnd + szNew), (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPValue, (uint64)(szPValue + nPValue));
                    } else {
                        _jsonBlobEdit(tls, pTarget, iTEnd, (uint32)0, (uintptr)0, szNew + (uint32)1);
                        if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Foom != 0) {
                            r = (int32)JSON_MERGE_OOM; goto ᒐdone;
                        }
                        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)iTEnd, (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pPatch)).Value.FaBlob + (uintptr)iPLabel, (uint64)szNew);
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.FaBlob + (uintptr)(iTEnd + szNew)))).Value = (uint8)0x00;
                        savedDelta1 = (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Fdelta;
                        (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Fdelta = 0;
                        if (iDepth >= (uint32)JSON_MAX_DEPTH) {
                            r = (int32)JSON_MERGE_TOODEEP; goto ᒐdone;
                        }
                        rc1 = _jsonMergePatch(tls, pTarget, iTEnd + szNew, pPatch, iPValue, iDepth + (uint32)1);
                        if (rc1 != 0) {
                            r = rc1; goto ᒐdone;
                        }
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pTarget + 52))).Value += savedDelta1;
                    }
                }
            }
        }
        if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Fdelta != 0) {
            _jsonAfterEditSizeAdjust(tls, pTarget, iTarget);
        }
        if ((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pTarget)).Value.Foom != 0) {
            v1 = (int32)JSON_MERGE_OOM;
        } else {
            v1 = JSON_MERGE_OK;
        }
        r = v1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _jsonReturnStringAsBlob(ж<libc.TLS> tls, uintptr pStr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        TJsonParse _ᴛ1 = default;
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FzJson = (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FzBuf;
        (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnJson = libc.Int32FromUint64((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FnUsed);
        (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.Fdb = Xsqlite3_context_db_handle(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FpCtx);
        _jsonTranslateTextToBlob(tls, bp, (uint32)0);
        if ((@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.Foom != 0) {
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.Fdb, (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FaBlob);
            Xsqlite3_result_error_nomem(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FpCtx);
        } else {
            Xsqlite3_result_blob(tls, (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)pStr)).Value.FpCtx, (@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FaBlob, libc.Int32FromUint32((@unsafe.Pointer.To<TJsonParse>(__ccgo_ptr(bp))).Value.FnBlob), __ccgo_fp(_sqlite3RowSetClear));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _memdbRead(ж<libc.TLS> tls, uintptr pFile, uintptr zBuf, int32 iAmt, Tsqlite_int64 iOfst) {
    uintptr p = default;
    _ = p;
    p = (@unsafe.Pointer.To<TMemFile>((@unsafe.Pointer)pFile)).Value.FpStore;
    _memdbEnter(tls, p);
    if (iOfst + (int64)iAmt > (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz) {
        libc.Xmemset(tls, zBuf, 0, libc.Uint64FromInt32(iAmt));
        if (iOfst < (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz) {
            libc.Xmemcpy(tls, zBuf, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData + (uintptr)iOfst, libc.Uint64FromInt64((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz - iOfst));
        }
        _memdbLeave(tls, p);
        return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    libc.Xmemcpy(tls, zBuf, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData + (uintptr)iOfst, libc.Uint64FromInt32(iAmt));
    _memdbLeave(tls, p);
    return SQLITE_OK;
}

internal static int32 /*r*/ _memdbWrite(ж<libc.TLS> tls, uintptr pFile, uintptr z, int32 iAmt, Tsqlite_int64 iOfst) {
    uintptr p = default;
    int32 rc = default;
    int32 v1 = default;
    bool v2 = default;
    _ = p;
    _ = rc;
    _ = v1;
    _ = v2;
    p = (@unsafe.Pointer.To<TMemFile>((@unsafe.Pointer)pFile)).Value.FpStore;
    _memdbEnter(tls, p);
    if (((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FmFlags & (uint32)SQLITE_DESERIALIZE_READONLY) != 0) {
        _memdbLeave(tls, p);
        return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    if (iOfst + (int64)iAmt > (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz) {
        v2 = iOfst + (int64)iAmt > (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszAlloc; if (v2) {
            v1 = _memdbEnlarge(tls, p, iOfst + (int64)iAmt);
            rc = v1;
        }
        if (v2 && v1 != SQLITE_OK) {
            _memdbLeave(tls, p);
            return rc;
        }
        if (iOfst > (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz) {
            libc.Xmemset(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz, 0, libc.Uint64FromInt64(iOfst - (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz));
        }
        (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz = iOfst + (int64)iAmt;
    }
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData + (uintptr)iOfst, z, libc.Uint64FromInt32(iAmt));
    _memdbLeave(tls, p);
    return SQLITE_OK;
}

internal static int32 /*r*/ _memjrnlRead(ж<libc.TLS> tls, uintptr pJfd, uintptr zBuf, int32 iAmt, Tsqlite_int64 iOfst) {
    int32 iChunkOffset = default;
    int32 iSpace = default;
    int32 nCopy = default;
    int32 nRead = default;
    int32 v5 = default;
    Tsqlite3_int64 iOff = default;
    uintptr p = default;
    uintptr pChunk = default;
    uintptr zOut = default;
    uintptr v2 = default;
    bool v3 = default;
    int64 v6 = default;
    _ = iChunkOffset;
    _ = iOff;
    _ = iSpace;
    _ = nCopy;
    _ = nRead;
    _ = p;
    _ = pChunk;
    _ = zOut;
    _ = v2;
    _ = v3;
    _ = v5;
    _ = v6;
    p = pJfd;
    zOut = zBuf;
    nRead = iAmt;
    if ((int64)iAmt + iOfst > (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fendpoint.FiOffset) {
        return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    if ((@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Freadpoint.FiOffset != iOfst || iOfst == 0) {
        iOff = 0;
        pChunk = (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FpFirst;
        for (;;) {
            if (!(pChunk != 0 && iOff + (int64)(@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize <= iOfst)) {
                break;
            }
            iOff = iOff + (int64)(@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize;
            goto _1;
_1:;
            pChunk = (@unsafe.Pointer.To<TFileChunk>((@unsafe.Pointer)pChunk)).Value.FpNext;
        }
    } else {
        pChunk = (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Freadpoint.FpChunk;
    }
    iChunkOffset = (int32)(rem(iOfst, (int64)(@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize));
    for (;;) {
        iSpace = (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize - iChunkOffset;
        if (nRead < (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize - iChunkOffset) {
            v5 = nRead;
        } else {
            v5 = (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize - iChunkOffset;
        }
        nCopy = v5;
        libc.Xmemcpy(tls, zOut, pChunk + 8 + (uintptr)iChunkOffset, libc.Uint64FromInt32(nCopy));
        zOut = zOut + (uintptr)nCopy;
        nRead = nRead - iSpace;
        iChunkOffset = 0;
        goto _4;
_4:;
        v3 = nRead >= 0; if (v3) {
            v2 = (@unsafe.Pointer.To<TFileChunk>((@unsafe.Pointer)pChunk)).Value.FpNext;
            pChunk = v2;
        }
        if (!(v3 && v2 != (uintptr)0 && nRead > 0)) {
            break;
        }
    }
    if (pChunk != 0) {
        v6 = iOfst + (int64)iAmt;
    } else {
        v6 = 0;
    }
    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Freadpoint.FiOffset = v6;
    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Freadpoint.FpChunk = pChunk;
    return SQLITE_OK;
}

internal static int32 /*r*/ _memjrnlWrite(ж<libc.TLS> tls, uintptr pJfd, uintptr zBuf, int32 iAmt, Tsqlite_int64 iOfst) {
    int32 iChunkOffset = default;
    int32 iSpace = default;
    int32 nWrite = default;
    int32 rc = default;
    int32 v1 = default;
    uintptr p = default;
    uintptr pChunk = default;
    uintptr pNew = default;
    uintptr zWrite = default;
    uintptr v2 = default;
    _ = iChunkOffset;
    _ = iSpace;
    _ = nWrite;
    _ = p;
    _ = pChunk;
    _ = pNew;
    _ = rc;
    _ = zWrite;
    _ = v1;
    _ = v2;
    p = pJfd;
    nWrite = iAmt;
    zWrite = zBuf;
    if ((@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnSpill > 0 && (int64)iAmt + iOfst > (int64)(@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnSpill) {
        rc = _memjrnlCreateFile(tls, p);
        if (rc == SQLITE_OK) {
            rc = _sqlite3OsWrite(tls, pJfd, zBuf, iAmt, iOfst);
        }
        return rc;
    } else {
        if (iOfst > 0 && iOfst != (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fendpoint.FiOffset) {
            _memjrnlTruncate(tls, pJfd, iOfst);
        }
        if (iOfst == 0 && (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FpFirst != 0) {
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FpFirst + 8, zBuf, libc.Uint64FromInt32(iAmt));
        } else {
            while (nWrite > 0) {
                pChunk = (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fendpoint.FpChunk;
                iChunkOffset = (int32)(rem((@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fendpoint.FiOffset, (int64)(@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize));
                if (nWrite < (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize - iChunkOffset) {
                    v1 = nWrite;
                } else {
                    v1 = (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize - iChunkOffset;
                }
                iSpace = v1;
                if (iChunkOffset == 0) {
                    pNew = Xsqlite3_malloc(tls, libc.Int32FromUint64(libc.Uint64FromInt64(16) + libc.Uint64FromInt32((@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize - libc.Int32FromInt32(8))));
                    if (!(pNew != 0)) {
                        return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8)));
                    }
                    (@unsafe.Pointer.To<TFileChunk>((@unsafe.Pointer)pNew)).Value.FpNext = (uintptr)0;
                    if (pChunk != 0) {
                        (@unsafe.Pointer.To<TFileChunk>((@unsafe.Pointer)pChunk)).Value.FpNext = pNew;
                    } else {
                        (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FpFirst = pNew;
                    }
                    v2 = pNew;
                    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fendpoint.FpChunk = v2;
                    pChunk = v2;
                }
                libc.Xmemcpy(tls, pChunk + 8 + (uintptr)iChunkOffset, zWrite, libc.Uint64FromInt32(iSpace));
                zWrite = zWrite + (uintptr)iSpace;
                nWrite = nWrite - iSpace;
                (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fendpoint.FiOffset += (int64)iSpace;
            }
        }
    }
    return SQLITE_OK;
}

internal static void _nodeDeleteCell(ж<libc.TLS> tls, uintptr pRtree, uintptr pNode, int32 iCell) {
    int32 nByte = default;
    uintptr pDst = default;
    uintptr pSrc = default;
    _ = nByte;
    _ = pDst;
    _ = pSrc;
    pDst = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + (uintptr)((int32)4 + libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell) * iCell);
    pSrc = pDst + (uintptr)(@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell;
    nByte = (_readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2) - iCell - (int32)1) * libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell);
    libc.Xmemmove(tls, pDst, pSrc, libc.Uint64FromInt32(nByte));
    _writeInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2, _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2) - (int32)1);
    (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FisDirty = (int32)1;
}

internal static uintptr /*r*/ _nodeNew(ж<libc.TLS> tls, uintptr pRtree, uintptr pParent) {
    uintptr pNode = default;
    _ = pNode;
    pNode = Xsqlite3_malloc64(tls, (uint64)((uint64)40 + libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize)));
    if (pNode != 0) {
        libc.Xmemset(tls, pNode, 0, (uint64)40 + libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize));
        (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData = pNode + 1 * 40;
        (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FnRef = (int32)1;
        (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnNodeRef = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnNodeRef + 1;
        (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FpParent = pParent;
        (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FisDirty = (int32)1;
        _nodeReference(ref (tls).DerefOrNull(), pParent);
    }
    return pNode;
}

internal static void _nodeZero(ж<libc.TLS> tls, uintptr pRtree, uintptr p) {
    libc.Xmemset(tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)p)).Value.FzData + 2, 0, libc.Uint64FromInt32((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize - (int32)2));
    (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)p)).Value.FisDirty = (int32)1;
}

internal static uintptr /*r*/ _pageFindSlot(ж<libc.TLS> tls, uintptr pPg, int32 nByte, @unsafe.Pointer pRc) {
    uintptr aData = default;
    uintptr pTmp = default;
    uintptr v2 = default;
    int32 hdr = default;
    int32 iAddr = default;
    int32 maxPC = default;
    int32 pc = default;
    int32 size = default;
    int32 x = default;
    int32 v1 = default;
    _ = aData;
    _ = hdr;
    _ = iAddr;
    _ = maxPC;
    _ = pTmp;
    _ = pc;
    _ = size;
    _ = x;
    _ = v1;
    _ = v2;
    hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FhdrOffset);
    aData = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaData;
    iAddr = hdr + (int32)1;
    pTmp = aData + (uintptr)iAddr;
    pc = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp + 1)));
    maxPC = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FpBt)).Value.FusableSize - libc.Uint32FromInt32(nByte));
    while (pc <= maxPC) {
        pTmp = aData + (uintptr)(pc + (int32)2);
        size = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp + 1)));
        v1 = size - nByte;
        x = v1;
        if (v1 >= 0) {
            if (x < (int32)4) {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)7)))) > (int32)57) {
                    return (uintptr)0;
                }
                libc.Xmemcpy(tls, aData + (uintptr)iAddr, aData + (uintptr)pc, (uint64)2);
                v2 = aData + (uintptr)(hdr + (int32)7);
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)) + libc.Int32FromUint8(libc.Uint8FromInt32(x)));
                return aData + (uintptr)pc;
            } else {
                if (x + pc > maxPC) {
                    (@unsafe.Pointer.To<int32>(pRc)).Value = _sqlite3CorruptError(tls, (int32)74952);
                    return (uintptr)0;
                } else {
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(pc + (int32)2)))).Value = libc.Uint8FromInt32(x.Rsh((int64)(libc.Int32FromInt32(8))));
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(pc + (int32)2) + 1))).Value = libc.Uint8FromInt32(x);
                }
            }
            return aData + (uintptr)(pc + x);
        }
        iAddr = pc;
        pTmp = aData + (uintptr)pc;
        pc = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pTmp + 1)));
        if (pc <= iAddr) {
            if (pc != 0) {
                (@unsafe.Pointer.To<int32>(pRc)).Value = _sqlite3CorruptError(tls, (int32)74967);
            }
            return (uintptr)0;
        }
    }
    if (pc > maxPC + nByte - (int32)4) {
        (@unsafe.Pointer.To<int32>(pRc)).Value = _sqlite3CorruptError(tls, (int32)74974);
    }
    return (uintptr)0;
}

internal static int32 /*r*/ _pageInsertArray(ж<libc.TLS> tls, uintptr pPg, uintptr pBegin, @unsafe.Pointer ppData, uintptr pCellptr, int32 iFirst, int32 nCell, @unsafe.Pointer pCArray) {
    uintptr aData = default;
    uintptr pData = default;
    uintptr pEnd = default;
    uintptr pSlot = default;
    uintptr v2 = default;
    int32 i = default;
    int32 iEnd = default;
    int32 k = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 sz = default;
    bool v3 = default;
    _ = aData;
    _ = i;
    _ = iEnd;
    _ = k;
    _ = pData;
    _ = pEnd;
    _ = pSlot;
    _ = rc;
    _ = sz;
    _ = v2;
    _ = v3;
    i = iFirst;
    aData = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaData;
    pData = ~@unsafe.Pointer.To<uintptr>(ppData);
    iEnd = iFirst + nCell;
    if (iEnd <= iFirst) {
        return 0;
    }
    k = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pCArray, 80), (uintptr)k * 4)) <= i)) {
            break;
        }
        goto _1;
_1:;
        k = k + 1;
    }
    pEnd = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Add(pCArray, 32), (uintptr)k * 8));
    while ((int32)1 != 0) {
        sz = libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCArray, 24)) + (uintptr)i * 2)));
        v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + 1))) == 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + 2))) == 0; if (!v3) {
            v2 = _pageFindSlot(tls, pPg, sz, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            pSlot = v2;
        }
        if (v3 || v2 == (uintptr)0) {
            if ((int64)pData - (int64)pBegin < (int64)sz) {
                return (int32)1;
            }
            pData = pData - (uintptr)sz;
            pSlot = pData;
        }
        if ((uint64)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCArray, 16)) + (uintptr)i * 8)) + (uintptr)sz) > (uint64)pEnd && (uint64)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCArray, 16)) + (uintptr)i * 8))) < (uint64)pEnd) {
            _sqlite3CorruptError(tls, (int32)80955);
            return (int32)1;
        }
        libc.Xmemmove(tls, pSlot, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCArray, 16)) + (uintptr)i * 8)), libc.Uint64FromInt32(sz));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCellptr))).Value = libc.Uint8FromInt64(((int64)pSlot - (int64)aData).Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCellptr + 1))).Value = libc.Uint8FromInt64((int64)pSlot - (int64)aData);
        pCellptr = pCellptr + (uintptr)2;
        i = i + 1;
        if (i >= iEnd) {
            break;
        }
        if (~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pCArray, 80), (uintptr)k * 4)) <= i) {
            k = k + 1;
            pEnd = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Add(pCArray, 32), (uintptr)k * 8));
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppData)).Value = pData;
    return 0;
}

internal static int32 /*r*/ _pagerOpenSavepoint(ж<libc.TLS> tls, uintptr pPager, int32 nSavepoint) {
    uintptr aNew = default;
    int32 ii = default;
    int32 nCurrent = default;
    int32 rc = default;
    _ = aNew;
    _ = ii;
    _ = nCurrent;
    _ = rc;
    rc = SQLITE_OK;
    nCurrent = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint;
    aNew = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint, (uint64)((uint64)56 * libc.Uint64FromInt32(nSavepoint)));
    if (!(aNew != 0)) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, aNew + (uintptr)nCurrent * 56, 0, libc.Uint64FromInt32(nSavepoint - nCurrent) * (uint64)56);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FaSavepoint = aNew;
    ii = nCurrent;
    for (;;) {
        if (!(ii < nSavepoint)) {
            break;
        }
        (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FnOrig = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize;
        if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0 && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff > 0) {
            (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FiOffset = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
        } else {
            (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FiOffset = libc.Int64FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize);
        }
        (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FiSubRec = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSubRec;
        (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FpInSavepoint = _sqlite3BitvecCreate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize);
        (@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FbTruncateOnRelease = (int32)1;
        if (!((@unsafe.Pointer.To<TPagerSavepoint>(__ccgo_ptr(aNew + (uintptr)ii * 56))).Value.FpInSavepoint != 0)) {
            return (int32)SQLITE_NOMEM;
        }
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
            _sqlite3WalSavepoint(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, aNew + (uintptr)ii * 56 + 36);
        }
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint = ii + (int32)1;
        goto _1;
_1:;
        ii = ii + 1;
    }
    return rc;
}

internal static int32 /*r*/ _pager_playback_one_page(ж<libc.TLS> tls, uintptr pPager, @unsafe.Pointer pOffset, uintptr pDone, int32 isMainJrnl, int32 isSavepnt) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aData = default;
        uintptr jfd = default;
        uintptr pData = default;
        uintptr v1 = default;
        ref var cksum = ref heap(new Tu32(), out var Ꮡcksum);
        int32 isSynced = default;
        int32 rc = default;
        int32 v2 = default;
        Ti64 ofst = default;
        ref var pgno = ref heap(new TPgno(), out var Ꮡpgno);
        bool v3 = default;
        uintptr _ᴛ1 = default;
        _ = aData;
        _ = cksum;
        _ = isSynced;
        _ = jfd;
        _ = ofst;
        _ = pData;
        _ = pgno;
        _ = rc;
        _ = v1;
        _ = v2;
        _ = v3;
        aData = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace;
        if (isMainJrnl != 0) {
            v1 = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd;
        } else {
            v1 = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd;
        }
        jfd = v1;
        rc = _read32bits(tls, jfd, ~@unsafe.Pointer.To<Ti64>(pOffset), @unsafe.Pointer.FromPinnedBox(Ꮡpgno));
        if (rc != SQLITE_OK) {
            r = rc; goto ᒐdone;
        }
        rc = _sqlite3OsRead(tls, jfd, aData, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, ~@unsafe.Pointer.To<Ti64>(pOffset) + (int64)4);
        if (rc != SQLITE_OK) {
            r = rc; goto ᒐdone;
        }
        (@unsafe.Pointer.To<Ti64>(pOffset)).Value += (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize + (int64)4 + (int64)(isMainJrnl * (int32)4);
        if (pgno == (uint32)0 || pgno == (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FlckPgno) {
            r = (int32)SQLITE_DONE; goto ᒐdone;
        }
        if (pgno > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize || _sqlite3BitvecTest(ref (tls).DerefOrNull(), pDone, pgno) != 0) {
            r = SQLITE_OK; goto ᒐdone;
        }
        if (isMainJrnl != 0) {
            rc = _read32bits(tls, jfd, ~@unsafe.Pointer.To<Ti64>(pOffset) - (int64)4, @unsafe.Pointer.FromPinnedBox(Ꮡcksum));
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
            if (!(isSavepnt != 0) && _pager_cksum(ref (tls).DerefOrNull(), pPager, aData) != cksum) {
                r = (int32)SQLITE_DONE; goto ᒐdone;
            }
        }
        v3 = pDone != 0; if (v3) {
            v2 = _sqlite3BitvecSet(tls, pDone, pgno);
            rc = v2;
        }
        if (v3 && v2 != SQLITE_OK) {
            r = rc; goto ᒐdone;
        }
        if (pgno == (uint32)1 && (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnReserve != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + 20)))) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnReserve = libc.Int16FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + 20)));
        }
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = _sqlite3PagerLookup(tls, pPager, pgno);
        }
        if (isMainJrnl != 0) {
            isSynced = libc.BoolInt32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoSync != 0 || ~@unsafe.Pointer.To<Ti64>(pOffset) <= (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr);
        } else {
            isSynced = libc.BoolInt32(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) == (uintptr)0 || 0 == (libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.Fflags) & (int32)PGHDR_NEED_SYNC));
        }
        if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) >= (int32)PAGER_WRITER_DBMOD || libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == PAGER_OPEN) && isSynced != 0) {
            ofst = libc.Int64FromUint32(pgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize;
            rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, aData, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, ofst);
            if (pgno > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize) {
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize = pgno;
            }
            if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpBackup != 0) {
                _sqlite3BackupUpdate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpBackup, pgno, aData);
            }
        } else {
            if (!(isMainJrnl != 0) && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) == (uintptr)0) {
                v1 = pPager + 25;
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(SPILLFLAG_ROLLBACK));
                rc = _sqlite3PagerGet(tls, pPager, pgno, bp, (int32)1);
                v1 = pPager + 25;
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(SPILLFLAG_ROLLBACK));
                if (rc != SQLITE_OK) {
                    r = rc; goto ᒐdone;
                }
                _sqlite3PcacheMakeDirty(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            }
        }
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) != 0) {
            pData = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpData;
            libc.Xmemcpy(tls, pData, aData, libc.Uint64FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize));
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FxReiniter)(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            if (pgno == (uint32)1) {
                libc.Xmemcpy(tls, pPager + 136, pData + 24, (uint64)16);
            }
            _sqlite3PcacheRelease(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _pager_truncate(ж<libc.TLS> tls, uintptr pPager, TPgno nPage) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pTmp = default;
        int32 rc = default;
        int32 szPage = default;
        Ti64 _ᴛ1 = default;
        Ti64 _ᴛ2 = default;
        _ = pTmp;
        _ = rc;
        _ = szPage;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) >= (int32)PAGER_WRITER_DBMOD || libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == PAGER_OPEN)) {
            szPage = (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize;
            rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, bp);
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))).Value = (int64)szPage * libc.Int64FromUint32(nPage);
            if (rc == SQLITE_OK && ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) != ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))) {
                if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) > ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))) {
                    rc = _sqlite3OsTruncate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8)));
                } else {
                    if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) + (int64)szPage <= ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))) {
                        pTmp = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace;
                        libc.Xmemset(tls, pTmp, 0, libc.Uint64FromInt32(szPage));
                        _sqlite3OsFileControlHint(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (int32)SQLITE_FCNTL_SIZE_HINT, bp + 8);
                        rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, pTmp, szPage, ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8)) - (int64)szPage);
                    }
                }
                if (rc == SQLITE_OK) {
                    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize = nPage;
                }
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _pager_write_pagelist(ж<libc.TLS> tls, uintptr pPager, uintptr pList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Ti64 offset = default;
        uintptr pData = default;
        TPgno pgno = default;
        int32 rc = default;
        Tsqlite3_int64 _ᴛ1 = default;
        _ = offset;
        _ = pData;
        _ = pgno;
        _ = rc;
        rc = SQLITE_OK;
        if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != libc.UintptrFromInt32(0))) {
            rc = _pagerOpentemp(tls, pPager, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, libc.Int32FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FvfsFlags));
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbHintSize < (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize && ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.FpDirty != 0 || (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.Fpgno > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbHintSize)) {
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize * libc.Int64FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize);
            _sqlite3OsFileControlHint(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (int32)SQLITE_FCNTL_SIZE_HINT, bp);
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbHintSize = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize;
        }
        while (rc == SQLITE_OK && pList != 0) {
            pgno = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.Fpgno;
            if (pgno <= (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize && 0 == (libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.Fflags) & (int32)PGHDR_DONT_WRITE)) {
                offset = libc.Int64FromUint32(pgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize;
                if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.Fpgno == (uint32)1) {
                    _pager_write_changecounter(ref (tls).DerefOrNull(), pList);
                }
                pData = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.FpData;
                rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, pData, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, offset);
                if (pgno == (uint32)1) {
                    libc.Xmemcpy(tls, pPager + 136, pData + 24, (uint64)16);
                }
                if (pgno > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize) {
                    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize = pgno;
                }
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + 2 * 4))).Value = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pPager + 248 + 2 * 4)) + 1;
                _sqlite3BackupUpdate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpBackup, pgno, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.FpData);
            } else {
            }
            pList = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.FpDirty;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _pragmaLocate(ж<libc.TLS> tls, uintptr zName) {
    int32 lwr = default;
    int32 mid = default;
    int32 rc = default;
    int32 upr = default;
    uintptr v1 = default;
    _ = lwr;
    _ = mid;
    _ = rc;
    _ = upr;
    _ = v1;
    mid = 0;
    lwr = 0;
    upr = libc.Int32FromUint64(libc.Uint64FromInt64(1584) / libc.Uint64FromInt64(24)) - libc.Int32FromInt32(1);
    while (lwr <= upr) {
        mid = (lwr + upr) / (int32)2;
        rc = Xsqlite3_stricmp(tls, zName, _aPragmaName[mid].FzName);
        if (rc == 0) {
            break;
        }
        if (rc < 0) {
            upr = mid - (int32)1;
        } else {
            lwr = mid + (int32)1;
        }
    }
    if (lwr > upr) {
        v1 = (uintptr)0;
    } else {
        v1 = (uintptr)Ꮡ_aPragmaName + (uintptr)mid * 24;
    }
    return v1;
}

internal static int32 /*r*/ _propagateConstants(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        int32 nChng = default;
        TWalker _ᴛ1 = new();
        TWhereConst _ᴛ2 = default;
        _ = nChng;
        nChng = 0;
        (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FpOomFault = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb + 103;
        for (var cond = true; cond; cond = (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FnChng != 0) {
            (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FnConst = 0;
            (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FnChng = 0;
            (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FapExpr = (uintptr)0;
            (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FbHasAffBlob = 0;
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc != (uintptr)0 && (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc > 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
                (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FmExcludeOn = libc.Uint32FromInt32(libc.Int32FromInt32(EP_InnerON) | libc.Int32FromInt32(EP_OuterON));
            } else {
                (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FmExcludeOn = (uint32)EP_OuterON;
            }
            _findConstInWhere(tls, __ccgo_ptr(bp), (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere);
            if ((@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FnConst != 0) {
                libc.Xmemset(tls, bp + 40, 0, (uint64)48);
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 40))).Value.FpParse = pParse;
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 40))).Value.FxExprCallback = __ccgo_fp(_propagateConstantExprRewrite);
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 40))).Value.FxSelectCallback = __ccgo_fp(_sqlite3SelectWalkNoop);
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 40))).Value.FxSelectCallback2 = (uintptr)0;
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 40))).Value.FwalkerDepth = 0;
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 40)).of(TWalker.ᏑFu)), 0))).Value = bp;
                _sqlite3WalkExpr(tls, bp + 40, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere);
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FpParse)).Value.Fdb, (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FapExpr);
                nChng = nChng + (@unsafe.Pointer.To<TWhereConst>(__ccgo_ptr(bp))).Value.FnChng;
            }
        }
        r = nChng;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _pushOntoSorter(ж<libc.TLS> tls, uintptr pParse, @unsafe.Pointer pSort, uintptr pSelect, int32 regData, int32 regOrigData, int32 nData, int32 nPrefixReg) {
    int32 addrFirst = default;
    int32 addrJmp = default;
    int32 bSeq = default;
    int32 iCsr = default;
    int32 iLimit = default;
    int32 iSkip = default;
    int32 nBase = default;
    int32 nExpr = default;
    int32 nKey = default;
    int32 nOBSat = default;
    int32 op = default;
    int32 regBase = default;
    int32 regPrevKey = default;
    int32 regRecord = default;
    int32 v1 = default;
    uintptr pKI = default;
    uintptr pOp = default;
    uintptr v = default;
    uintptr v4 = default;
    _ = addrFirst;
    _ = addrJmp;
    _ = bSeq;
    _ = iCsr;
    _ = iLimit;
    _ = iSkip;
    _ = nBase;
    _ = nExpr;
    _ = nKey;
    _ = nOBSat;
    _ = op;
    _ = pKI;
    _ = pOp;
    _ = regBase;
    _ = regPrevKey;
    _ = regRecord;
    _ = v;
    _ = v1;
    _ = v4;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    bSeq = libc.BoolInt32((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pSort, 36))) & (int32)SORTFLAG_UseSorter) == 0);
    nExpr = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pSort)))).Value.FnExpr;
    nBase = nExpr + bSeq + nData;
    regRecord = 0;
    nOBSat = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 8));
    iSkip = 0;
    if (nPrefixReg != 0) {
        regBase = regData - nPrefixReg;
    } else {
        regBase = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nBase;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FiOffset != 0) {
        v1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FiOffset + (int32)1;
    } else {
        v1 = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FiLimit;
    }
    iLimit = v1;
    (@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 28))).Value = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
    if (regOrigData != 0) {
        v1 = (int32)SQLITE_ECEL_REF;
    } else {
        v1 = 0;
    }
    _sqlite3ExprCodeExprList(tls, pParse, ~@unsafe.Pointer.To<uintptr>(pSort), regBase, regOrigData, libc.Uint8FromInt32((int32)SQLITE_ECEL_DUP | v1));
    if (bSeq != 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Sequence, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 12)), regBase + nExpr);
    }
    if (nPrefixReg == 0 && nData > 0) {
        _sqlite3ExprCodeMove(tls, pParse, regData, regBase + nExpr + bSeq, nData);
    }
    if (nOBSat > 0) {
        regRecord = _makeSorterRecord(tls, pParse, pSort, pSelect, regBase, nBase);
        regPrevKey = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 8))).Value;
        nKey = nExpr - ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 8)) + bSeq;
        if (bSeq != 0) {
            addrFirst = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IfNot, regBase + nExpr);
        } else {
            addrFirst = _sqlite3VdbeAddOp1(tls, v, (int32)OP_SequenceTest, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 12)));
        }
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Compare, regPrevKey, regBase, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 8)));
        pOp = _sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 24)));
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0) {
            return;
        }
        (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2 = nKey + nData;
        pKI = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16));
        libc.Xmemset(tls, (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKI)).Value.FaSortFlags, 0, (uint64)(@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKI)).Value.FnKeyField);
        _sqlite3VdbeChangeP4(tls, v, -(int32)1, pKI, -(int32)9);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16))).Value = _sqlite3KeyInfoFromExprList(tls, pParse, ~@unsafe.Pointer.To<uintptr>(pSort), nOBSat, libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKI)).Value.FnAllField) - libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKI)).Value.FnKeyField) - (int32)1);
        pOp = (uintptr)0;
        addrJmp = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Jump, addrJmp + (int32)1, 0, addrJmp + (int32)1);
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 20))).Value = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        v4 = pParse + 60;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
        (@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 16))).Value = v1;
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Gosub, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 16)), ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 20)));
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_ResetSorter, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 12)));
        if (iLimit != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfNot, iLimit, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 28)));
        }
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrFirst);
        _sqlite3ExprCodeMove(tls, pParse, regBase, regPrevKey, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 8)));
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrJmp);
    }
    if (iLimit != 0) {
        iCsr = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 12));
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_IfNotZero, iLimit, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)4);
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Last, iCsr, 0);
        iSkip = _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxLE, iCsr, 0, regBase + nOBSat, nExpr - nOBSat);
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_Delete, iCsr);
    }
    if (regRecord == 0) {
        regRecord = _makeSorterRecord(tls, pParse, pSort, pSelect, regBase, nBase);
    }
    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pSort, 36))) & (int32)SORTFLAG_UseSorter) != 0) {
        op = (int32)OP_SorterInsert;
    } else {
        op = (int32)OP_IdxInsert;
    }
    _sqlite3VdbeAddOp4Int(tls, v, op, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 12)), regRecord, regBase + nOBSat, nBase - nOBSat);
    if (iSkip != 0) {
        if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 32)) != 0) {
            v1 = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pSort, 32));
        } else {
            v1 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        }
        _sqlite3VdbeChangeP2(ref (tls).DerefOrNull(), v, iSkip, v1);
    }
}

internal static int32 /*r*/ _quotedCompare(ж<libc.TLS> tls, uintptr ctx, int32 t, uintptr zQuote, int32 nQuote, uintptr zCmp, @unsafe.Pointer pRes) {
    uintptr zCopy = default;
    _ = zCopy;
    zCopy = (uintptr)0;
    if (t == (int32)TK_ILLEGAL) {
        (@unsafe.Pointer.To<int32>(pRes)).Value = (int32)1;
        return SQLITE_OK;
    }
    zCopy = _sqlite3MallocZero(tls, libc.Uint64FromInt32(nQuote + (int32)1));
    if (zCopy == (uintptr)0) {
        Xsqlite3_result_error_nomem(tls, ctx);
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemcpy(tls, zCopy, zQuote, libc.Uint64FromInt32(nQuote));
    _sqlite3Dequote(ref (tls).DerefOrNull(), zCopy);
    (@unsafe.Pointer.To<int32>(pRes)).Value = Xsqlite3_stricmp(tls, zCopy, zCmp);
    Xsqlite3_free(tls, zCopy);
    return SQLITE_OK;
}

internal static uintptr /*r*/ _rbuMalloc(ж<libc.TLS> tls, uintptr p, Tsqlite3_int64 nByte) {
    uintptr pRet = default;
    _ = pRet;
    pRet = (uintptr)0;
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        pRet = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (pRet == (uintptr)0) {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
        } else {
            libc.Xmemset(tls, pRet, 0, libc.Uint64FromInt64(nByte));
        }
    }
    return pRet;
}

internal static int32 /*r*/ _rbuVfsRead(ж<libc.TLS> tls, uintptr pFile, uintptr zBuf, int32 iAmt, Tsqlite_int64 iOfst) {
    uintptr aBuf = default;
    uintptr p = default;
    uintptr pBuf = default;
    uintptr pFd = default;
    uintptr pRbu = default;
    Tu32 iRoot = default;
    int32 rc = default;
    int32 v1 = default;
    _ = aBuf;
    _ = iRoot;
    _ = p;
    _ = pBuf;
    _ = pFd;
    _ = pRbu;
    _ = rc;
    _ = v1;
    p = pFile;
    pRbu = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu;
    if (pRbu != 0 && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FeStage == (int32)RBU_STAGE_CAPTURE) {
        rc = _rbuCaptureWalRead(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu, iOfst, iAmt);
    } else {
        if (pRbu != 0 && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FeStage == (int32)RBU_STAGE_OAL && ((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FopenFlags & (int32)SQLITE_OPEN_WAL) != 0 && iOfst >= (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FiOalSz) {
            rc = SQLITE_OK;
            libc.Xmemset(tls, zBuf, 0, libc.Uint64FromInt32(iAmt));
        } else {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Tsqlite3_int64, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal)).Value.FpMethods)).Value.FxRead)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, zBuf, iAmt, iOfst);
            if (pRbu != 0 && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FzTarget == (uintptr)0 && rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)))) && iOfst == 0 && ((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FopenFlags & (int32)SQLITE_OPEN_MAIN_DB) != 0 && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.Frc == SQLITE_OK) {
                pFd = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FpRbuFd;
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Tsqlite3_int64, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pFd)).Value.FpMethods)).Value.FxRead)(tls, pFd, zBuf, iAmt, iOfst);
                if (rc == SQLITE_OK) {
                    aBuf = zBuf;
                    if (_rbuGetU32(ref (tls).DerefOrNull(), aBuf + 52) != 0) {
                        v1 = (int32)1;
                    } else {
                        v1 = 0;
                    }
                    iRoot = libc.Uint32FromInt32(v1);
                    _rbuPutU32(ref (tls).DerefOrNull(), aBuf + 52, iRoot);
                    _rbuPutU32(ref (tls).DerefOrNull(), aBuf + 36, (uint32)0);
                    _rbuPutU32(ref (tls).DerefOrNull(), aBuf + 32, (uint32)0);
                    _rbuPutU32(ref (tls).DerefOrNull(), aBuf + 28, (uint32)1);
                    _rbuPutU32(ref (tls).DerefOrNull(), aBuf + 24, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FpRbuFd)).Value.FiCookie + (uint32)1);
                    if (iAmt > (int32)100) {
                        libc.Xmemset(tls, aBuf + 100, 0, libc.Uint64FromInt32(iAmt - (int32)100));
                        _rbuPutU16(ref (tls).DerefOrNull(), aBuf + 105, libc.Uint16FromInt32(iAmt & (int32)0xFFFF));
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf + 100))).Value = (uint8)0x0D;
                    }
                }
            }
        }
        if (rc == SQLITE_OK && iOfst == 0 && ((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FopenFlags & (int32)SQLITE_OPEN_MAIN_DB) != 0) {
            pBuf = zBuf;
            (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FiCookie = _rbuGetU32(ref (tls).DerefOrNull(), pBuf + 24);
            (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FiWriteVer = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pBuf + 19));
        }
    }
    return rc;
}

internal static int32 /*r*/ _rbuVfsShmMap(ж<libc.TLS> tls, uintptr pFile, int32 iRegion, int32 szRegion, int32 isWrite, uintptr pp) {
    uintptr apNew = default;
    uintptr p = default;
    uintptr pNew = default;
    int32 eStage = default;
    int32 rc = default;
    int32 v1 = default;
    Tsqlite3_int64 nByte = default;
    _ = apNew;
    _ = eStage;
    _ = nByte;
    _ = p;
    _ = pNew;
    _ = rc;
    _ = v1;
    p = pFile;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu != 0) {
        v1 = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu)).Value.FeStage;
    } else {
        v1 = 0;
    }
    eStage = v1;
    if (eStage == (int32)RBU_STAGE_OAL) {
        nByte = libc.Int64FromUint64(libc.Uint64FromInt32(iRegion + libc.Int32FromInt32(1)) * (uint64)8);
        apNew = Xsqlite3_realloc64(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm, libc.Uint64FromInt64(nByte));
        if (apNew == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            libc.Xmemset(tls, apNew + (uintptr)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FnShm * 8, 0, (uint64)8 * libc.Uint64FromInt32(libc.Int32FromInt32(1) + iRegion - (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FnShm));
            (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm = apNew;
            (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FnShm = iRegion + (int32)1;
        }
        if (rc == SQLITE_OK) {
            pNew = Xsqlite3_malloc64(tls, libc.Uint64FromInt32(szRegion));
            if (pNew == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                libc.Xmemset(tls, pNew, 0, libc.Uint64FromInt32(szRegion));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm + (uintptr)iRegion * 8))).Value = pNew;
            }
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FapShm + (uintptr)iRegion * 8))).Value;
        } else {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (uintptr)0;
        }
    } else {
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal)).Value.FpMethods)).Value.FxShmMap)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, iRegion, szRegion, isWrite, pp);
    }
    return rc;
}

internal static int32 /*r*/ _readDbPage(ж<libc.TLS> tls, uintptr pPg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr dbFileVers = default;
        uintptr pPager = default;
        Ti64 iOffset = default;
        int32 rc = default;
        Tu32 _ᴛ1 = default;
        _ = dbFileVers;
        _ = iOffset;
        _ = pPager;
        _ = rc;
        pPager = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value = (uint32)0;
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
            rc = _sqlite3WalFindFrame(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno, bp);
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
        }
        if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp)) != 0) {
            rc = _sqlite3WalReadFrame(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp)), (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData);
        } else {
            iOffset = libc.Int64FromUint32((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize;
            rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, iOffset);
            if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))))) {
                rc = SQLITE_OK;
            }
        }
        if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno == (uint32)1) {
            if (rc != 0) {
                libc.Xmemset(tls, pPager + 136, (int32)0xff, (uint64)16);
            } else {
                dbFileVers = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData + 24;
                libc.Xmemcpy(tls, pPager + 136, dbFileVers, (uint64)16);
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _rebuildPage(ж<libc.TLS> tls, @unsafe.Pointer pCArray, int32 iFirst, int32 nCell, uintptr pPg) {
    uintptr aData = default;
    uintptr pCell = default;
    uintptr pCellptr = default;
    uintptr pData = default;
    uintptr pEnd = default;
    uintptr pSrcEnd = default;
    uintptr pTmp = default;
    int32 hdr = default;
    int32 i = default;
    int32 iEnd = default;
    int32 k = default;
    int32 usableSize = default;
    Tu32 j = default;
    Tu16 sz = default;
    _ = aData;
    _ = hdr;
    _ = i;
    _ = iEnd;
    _ = j;
    _ = k;
    _ = pCell;
    _ = pCellptr;
    _ = pData;
    _ = pEnd;
    _ = pSrcEnd;
    _ = pTmp;
    _ = sz;
    _ = usableSize;
    hdr = libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FhdrOffset);
    aData = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaData;
    usableSize = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FpBt)).Value.FusableSize);
    pEnd = aData + (uintptr)usableSize;
    i = iFirst;
    iEnd = i + nCell;
    pCellptr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FaCellIdx;
    pTmp = _sqlite3PagerTempSpace(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FpBt)).Value.FpPager);
    j = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5) + 1))));
    if (j > libc.Uint32FromInt32(usableSize)) {
        j = (uint32)0;
    }
    libc.Xmemcpy(tls, pTmp + (uintptr)j, aData + (uintptr)j, (uint64)(libc.Uint32FromInt32(usableSize) - j));
    k = 0;
    for (;;) {
        if (!(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pCArray, 80), (uintptr)k * 4)) <= i)) {
            break;
        }
        goto _1;
_1:;
        k = k + 1;
    }
    pSrcEnd = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Add(pCArray, 32), (uintptr)k * 8));
    pData = pEnd;
    while ((int32)1 != 0) {
        pCell = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCArray, 16)) + (uintptr)i * 8));
        sz = ~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pCArray, 24)) + (uintptr)i * 2));
        if ((uint64)pCell >= (uint64)(aData + (uintptr)j) && (uint64)pCell < (uint64)pEnd) {
            if ((uint64)(pCell + (uintptr)sz) > (uint64)pEnd) {
                return _sqlite3CorruptError(tls, (int32)80857);
            }
            pCell = pTmp + (uintptr)((int64)pCell - (int64)aData);
        } else {
            if ((uint64)(pCell + (uintptr)sz) > (uint64)pSrcEnd && (uint64)pCell < (uint64)pSrcEnd) {
                return _sqlite3CorruptError(tls, (int32)80862);
            }
        }
        pData = pData - (uintptr)sz;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCellptr))).Value = libc.Uint8FromInt64(((int64)pData - (int64)aData).Rsh((int64)(libc.Int32FromInt32(8))));
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCellptr + 1))).Value = libc.Uint8FromInt64((int64)pData - (int64)aData);
        pCellptr = pCellptr + (uintptr)2;
        if (pData < pCellptr) {
            return _sqlite3CorruptError(tls, (int32)80868);
        }
        libc.Xmemmove(tls, pData, pCell, (uint64)sz);
        i = i + 1;
        if (i >= iEnd) {
            break;
        }
        if (~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(pCArray, 80), (uintptr)k * 4)) <= i) {
            k = k + 1;
            pSrcEnd = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Add(pCArray, 32), (uintptr)k * 8));
        }
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell = libc.Uint16FromInt32(nCell);
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnOverflow = (uint8)0;
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)1)))).Value = libc.Uint8FromInt32(libc.Int32FromInt32(0).Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)1) + 1))).Value = libc.Uint8FromInt32(libc.Int32FromInt32(0));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)3)))).Value = libc.Uint8FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell).Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)3) + 1))).Value = (uint8)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPg)).Value.FnCell;
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5)))).Value = libc.Uint8FromInt64(((int64)pData - (int64)aData).Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)5) + 1))).Value = libc.Uint8FromInt64((int64)pData - (int64)aData);
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aData + (uintptr)(hdr + (int32)7)))).Value = (uint8)0x00;
    return SQLITE_OK;
}

internal static int32 /*r*/ _rehash(ж<libc.TLS> tls, uintptr pH, uint32 new_size) {
    uintptr elem = default;
    uintptr new_ht = default;
    uintptr next_elem = default;
    uint32 v1 = default;
    _ = elem;
    _ = new_ht;
    _ = next_elem;
    _ = v1;
    if ((uint64)new_size * (uint64)16 > (uint64)SQLITE_MALLOC_SOFT_LIMIT) {
        new_size = (uint32)(libc.Uint64FromInt32(SQLITE_MALLOC_SOFT_LIMIT) / libc.Uint64FromInt64(16));
    }
    if (new_size == (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fhtsize) {
        return 0;
    }
    _sqlite3BeginBenignMalloc(tls);
    new_ht = _sqlite3Malloc(tls, (uint64)new_size * (uint64)16);
    _sqlite3EndBenignMalloc(tls);
    if (new_ht == (uintptr)0) {
        return 0;
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fht);
    (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fht = new_ht;
    v1 = (uint32)(libc.Uint64FromInt32(_sqlite3MallocSize(tls, new_ht)) / libc.Uint64FromInt64(16));
    new_size = v1;
    (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Fhtsize = v1;
    libc.Xmemset(tls, new_ht, 0, (uint64)new_size * (uint64)16);
    elem = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Ffirst;
    (@unsafe.Pointer.To<THash>((@unsafe.Pointer)pH)).Value.Ffirst = libc.UintptrFromInt32(0);
    for (;;) {
        if (!(elem != 0)) {
            break;
        }
        next_elem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fnext;
        _insertElement(ref (tls).DerefOrNull(), pH, new_ht + (uintptr)((@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)elem)).Value.Fh % new_size) * 16, elem);
        goto _2;
_2:;
        elem = next_elem;
    }
    return (int32)1;
}

internal static void _renameQuotefixFunc(ж<libc.TLS> tls, uintptr context, int32 NotUsed, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(528);
        defer(tls.Free, (nint)528, ref ᒐ);
        uintptr db = default;
        uintptr pSelect = default;
        uintptr zDb = default;
        uintptr zInput = default;
        int32 i = default;
        int32 rc = default;
        int32 v1 = default;
        Tsqlite3_xauth xAuth = default;
        TRenameCtx _ᴛ1 = default;
        TParse _ᴛ2 = new();
        TWalker _ᴛ3 = new();
        _ = db;
        _ = i;
        _ = pSelect;
        _ = rc;
        _ = xAuth;
        _ = zDb;
        _ = zInput;
        _ = v1;
        db = Xsqlite3_context_db_handle(tls, context);
        zDb = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        zInput = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
        xAuth = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth = (uintptr)0;
        _sqlite3BtreeEnterAll(tls, db);
        _ = NotUsed;
        if (zDb != 0 && zInput != 0) {
            rc = _renameParseSql(tls, bp, zDb, db, zInput, 0);
            if (rc == SQLITE_OK) {
                libc.Xmemset(tls, bp + 440, 0, (uint64)32);
                libc.Xmemset(tls, bp + 472, 0, (uint64)48);
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 472))).Value.FpParse = bp;
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 472))).Value.FxExprCallback = __ccgo_fp(_renameQuotefixExprCb);
                (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 472))).Value.FxSelectCallback = __ccgo_fp(_renameColumnSelectCb);
                (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp + 472)).of(TWalker.ᏑFu)), 0))).Value = bp + 440;
                if ((@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable != 0) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable)).Value.FeTabType) == (int32)TABTYP_VIEW) {
                        pSelect = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable).of(TTable.ᏑFu).Reinterpret<TTableᴛ1_Fu, TTableᴛ1_Fu_Fview>()).Value.FpSelect;
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSelect + 4))).Value &= ~libc.Uint32FromInt32(SF_View);
                        (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.Frc = SQLITE_OK;
                        _sqlite3SelectPrep(tls, bp, pSelect, (uintptr)0);
                        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                            v1 = (int32)SQLITE_NOMEM;
                        } else {
                            v1 = (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.Frc;
                        }
                        rc = v1;
                        if (rc == SQLITE_OK) {
                            _sqlite3WalkSelect(tls, bp + 472, pSelect);
                        }
                    } else {
                        _sqlite3WalkExprList(tls, bp + 472, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable)).Value.FpCheck);
                        i = 0;
                        for (;;) {
                            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable)).Value.FnCol)) {
                                break;
                            }
                            _sqlite3WalkExpr(tls, bp + 472, _sqlite3ColumnExpr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable)).Value.FaCol + (uintptr)i * 16));
                            goto _2;
_2:;
                            i = i + 1;
                        }
                    }
                } else {
                    if ((@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewIndex != 0) {
                        _sqlite3WalkExprList(tls, bp + 472, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewIndex)).Value.FaColExpr);
                        _sqlite3WalkExpr(tls, bp + 472, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewIndex)).Value.FpPartIdxWhere);
                    } else {
                        rc = _renameResolveTrigger(tls, bp);
                        if (rc == SQLITE_OK) {
                            _renameWalkTrigger(tls, bp + 472, (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTrigger);
                        }
                    }
                }
                if (rc == SQLITE_OK) {
                    rc = _renameEditSql(tls, context, __ccgo_ptr(bp + 440), zInput, (uintptr)0, 0);
                }
                _renameTokenFree(tls, db, (@unsafe.Pointer.To<TRenameCtx>(__ccgo_ptr(bp + 440))).Value.FpList);
            }
            if (rc != SQLITE_OK) {
                if (_sqlite3WritableSchema(ref (tls).DerefOrNull(), db) != 0 && rc == (int32)SQLITE_ERROR) {
                    Xsqlite3_result_value(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
                } else {
                    Xsqlite3_result_error_code(tls, context, rc);
                }
            }
            _renameParseCleanup(tls, __ccgo_ptr(bp));
        }
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth = xAuth;
        _sqlite3BtreeLeaveAll(tls, db);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _renameTableTest(ж<libc.TLS> tls, uintptr context, int32 NotUsed, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(496);
        defer(tls.Free, (nint)496, ref ᒐ);
        int32 bNoDQS = default;
        int32 bTemp = default;
        int32 i1 = default;
        int32 i2 = default;
        int32 isLegacy = default;
        int32 rc = default;
        uintptr db = default;
        uintptr zDb = default;
        uintptr zInput = default;
        uintptr zWhen = default;
        Tu64 flags = default;
        Tsqlite3_xauth xAuth = default;
        TNameContext _ᴛ1 = new();
        TParse _ᴛ2 = new();
        _ = bNoDQS;
        _ = bTemp;
        _ = db;
        _ = flags;
        _ = i1;
        _ = i2;
        _ = isLegacy;
        _ = rc;
        _ = xAuth;
        _ = zDb;
        _ = zInput;
        _ = zWhen;
        db = Xsqlite3_context_db_handle(tls, context);
        zDb = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        zInput = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
        bTemp = Xsqlite3_value_int(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 4 * 8)));
        isLegacy = libc.Int32FromUint64((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & libc.Uint64FromInt32(SQLITE_LegacyAlter)));
        zWhen = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 5 * 8)));
        bNoDQS = Xsqlite3_value_int(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 6 * 8)));
        xAuth = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth = (uintptr)0;
        _ = NotUsed;
        if (zDb != 0 && zInput != 0) {
            flags = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags;
            if (bNoDQS != 0) {
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value &= libc.Uint64FromInt32(~(libc.Int32FromInt32(SQLITE_DqsDML) | libc.Int32FromInt32(SQLITE_DqsDDL)));
            }
            rc = _renameParseSql(tls, bp, zDb, db, zInput, bTemp);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags = flags;
            if (rc == SQLITE_OK) {
                if (isLegacy == 0 && (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable)).Value.FeTabType) == (int32)TABTYP_VIEW) {
                    libc.Xmemset(tls, bp + 440, 0, (uint64)56);
                    (@unsafe.Pointer.To<TNameContext>(__ccgo_ptr(bp + 440))).Value.FpParse = bp;
                    _sqlite3SelectPrep(tls, bp, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTable).of(TTable.ᏑFu).Reinterpret<TTableᴛ1_Fu, TTableᴛ1_Fu_Fview>()).Value.FpSelect, bp + 440);
                    if ((@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FnErr != 0) {
                        rc = (@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.Frc;
                    }
                } else {
                    if ((@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTrigger != 0) {
                        if (isLegacy == 0) {
                            rc = _renameResolveTrigger(tls, bp);
                        }
                        if (rc == SQLITE_OK) {
                            i1 = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>(__ccgo_ptr(bp))).Value.FpNewTrigger)).Value.FpTabSchema);
                            i2 = _sqlite3FindDbName(tls, db, zDb);
                            if (i1 == i2) {
                                Xsqlite3_result_int(tls, context, (int32)1);
                            }
                        }
                    }
                }
            }
            if (rc != SQLITE_OK && zWhen != 0 && !(_sqlite3WritableSchema(ref (tls).DerefOrNull(), db) != 0)) {
                _renameColumnParseError(tls, context, zWhen, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 3 * 8)), __ccgo_ptr(bp));
            }
            _renameParseCleanup(tls, __ccgo_ptr(bp));
        }
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth = xAuth;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _replaceFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    uint32 cntExpand = default;
    uintptr db = default;
    uintptr zOld = default;
    uintptr zOut = default;
    uintptr zPattern = default;
    uintptr zRep = default;
    uintptr zStr = default;
    Ti64 i = default;
    Ti64 j = default;
    Ti64 nOut = default;
    Ti64 v2 = default;
    Ti64 v3 = default;
    int32 loopLimit = default;
    int32 nPattern = default;
    int32 nRep = default;
    int32 nStr = default;
    _ = cntExpand;
    _ = db;
    _ = i;
    _ = j;
    _ = loopLimit;
    _ = nOut;
    _ = nPattern;
    _ = nRep;
    _ = nStr;
    _ = zOld;
    _ = zOut;
    _ = zPattern;
    _ = zRep;
    _ = zStr;
    _ = v2;
    _ = v3;
    db = Xsqlite3_context_db_handle(tls, context);
    _ = argc;
    zStr = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (zStr == (uintptr)0) {
        return;
    }
    nStr = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    zPattern = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    if (zPattern == (uintptr)0) {
        return;
    }
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zPattern))) == 0) {
        Xsqlite3_result_text(tls, context, zStr, nStr, (uintptr)(-libc.Int32FromInt32(1)));
        return;
    }
    nPattern = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)));
    zRep = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)));
    if (zRep == (uintptr)0) {
        return;
    }
    nRep = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)));
    nOut = (int64)(nStr + (int32)1);
    zOut = _contextMalloc(tls, context, nOut);
    if (zOut == (uintptr)0) {
        return;
    }
    loopLimit = nStr - nPattern;
    cntExpand = (uint32)0;
    v2 = libc.Int64FromInt32(0);
    j = v2;
    i = v2;
    for (;;) {
        if (!(i <= (int64)loopLimit)) {
            break;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zStr + (uintptr)i))) != libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(zPattern))) || libc.Xmemcmp(tls, zStr + (uintptr)i, zPattern, libc.Uint64FromInt32(nPattern)) != 0) {
            v3 = j;
            j = j + 1;
            (@unsafe.Pointer.To<uint8>(__ccgo_ptr(zOut + (uintptr)v3))).Value = (@unsafe.Pointer.To<uint8>(__ccgo_ptr(zStr + (uintptr)i))).Value;
        } else {
            if (nRep > nPattern) {
                nOut = nOut + (int64)(nRep - nPattern);
                if (nOut - (int64)1 > (int64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136)))) {
                    Xsqlite3_result_error_toobig(tls, context);
                    Xsqlite3_free(tls, zOut);
                    return;
                }
                cntExpand = cntExpand + 1;
                if ((cntExpand & (cntExpand - (uint32)1)) == (uint32)0) {
                    zOld = zOut;
                    zOut = _sqlite3Realloc(tls, zOut, libc.Uint64FromInt64((int64)(int32)nOut + (nOut - (int64)nStr - (int64)1)));
                    if (zOut == (uintptr)0) {
                        Xsqlite3_result_error_nomem(tls, context);
                        Xsqlite3_free(tls, zOld);
                        return;
                    }
                }
            }
            libc.Xmemcpy(tls, zOut + (uintptr)j, zRep, libc.Uint64FromInt32(nRep));
            j = j + (int64)nRep;
            i = i + (int64)(nPattern - (int32)1);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    libc.Xmemcpy(tls, zOut + (uintptr)j, zStr + (uintptr)i, libc.Uint64FromInt64((int64)nStr - i));
    j = j + ((int64)nStr - i);
    (@unsafe.Pointer.To<uint8>(__ccgo_ptr(zOut + (uintptr)j))).Value = (uint8)0;
    Xsqlite3_result_text(tls, context, zOut, (int32)j, __ccgo_fp(Xsqlite3_free));
}

internal static void _resolveAlias(ж<libc.TLS> tls, uintptr pParse, uintptr pEList, int32 iCol, uintptr pExpr, int32 nSubquery) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        uintptr db = default;
        uintptr pDup = default;
        uintptr pOrig = default;
        TExpr _ᴛ1 = new();
        _ = db;
        _ = pDup;
        _ = pOrig;
        pOrig = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)iCol * 32))).Value.FpExpr;
        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpAggInfo != 0) {
            return;
        }
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pDup = _sqlite3ExprDup(tls, db, pOrig, 0);
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pDup);
            pDup = (uintptr)0;
        } else {
            _incrAggFunctionDepth(tls, pDup, nSubquery);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_COLLATE) {
                pDup = _sqlite3ExprAddCollateString(tls, pParse, pDup, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)));
            }
            libc.Xmemcpy(tls, bp, pDup, (uint64)72);
            libc.Xmemcpy(tls, pDup, pExpr, (uint64)72);
            libc.Xmemcpy(tls, pExpr, bp, (uint64)72);
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc))) != (uint32)0) {
                if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64)) != (uintptr)0) {
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64))))).Value.FpOwner = pExpr;
                }
            }
            _sqlite3ExprDeferredDelete(tls, pParse, pDup);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _rtreeUpdate(ж<libc.TLS> tls, uintptr pVtab, int32 nData, uintptr aData, uintptr pRowid) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 bHaveRowid = default;
        int32 ii = default;
        int32 jj = default;
        int32 nn = default;
        int32 rc = default;
        int32 rc2 = default;
        int32 steprc = default;
        ref var pLeaf = ref heap(new uintptr(), out var ᏑpLeaf);
        uintptr pRtree = default;
        uintptr pUp = default;
        TRtreeCell _ᴛ1 = new();
        _ = bHaveRowid;
        _ = ii;
        _ = jj;
        _ = nn;
        _ = pLeaf;
        _ = pRtree;
        _ = pUp;
        _ = rc;
        _ = rc2;
        _ = steprc;
        pRtree = pVtab;
        rc = SQLITE_OK;
        bHaveRowid = 0;
        if ((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnNodeRef != 0) {
            r = libc.Int32FromInt32(SQLITE_LOCKED) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))); goto ᒐdone;
        }
        _rtreeReference(ref (tls).DerefOrNull(), pRtree);
        libc.Xmemset(tls, bp, 0, (uint64)48);
        if (nData > (int32)1) {
            nn = nData - (int32)4;
            if (nn > libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2)) {
                nn = libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2);
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                ii = 0;
                for (;;) {
                    if (!(ii < nn)) {
                        break;
                    }
                    (@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 8 + (uintptr)ii * 4))).Value = _rtreeValueDown(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + (uintptr)(ii + (int32)3) * 8)));
                    (@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 8 + (uintptr)(ii + (int32)1) * 4))).Value = _rtreeValueUp(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + (uintptr)(ii + (int32)4) * 8)));
                    if (~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 8 + (uintptr)ii * 4)) > ~@unsafe.Pointer.To<TRtreeValue>((@unsafe.Pointer)(bp + 8 + (uintptr)(ii + (int32)1) * 4))) {
                        rc = _rtreeConstraintError(tls, pRtree, ii + (int32)1);
                        goto constraint;
                    }
                    goto _1;
_1:;
                    ii = ii + (int32)2;
                }
            } else {
                ii = 0;
                for (;;) {
                    if (!(ii < nn)) {
                        break;
                    }
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(bp + 8 + (uintptr)ii * 4))).Value = _rtreeValueInt32(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + (uintptr)(ii + (int32)3) * 8)));
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(bp + 8 + (uintptr)(ii + (int32)1) * 4))).Value = _rtreeValueInt32(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + (uintptr)(ii + (int32)4) * 8)));
                    if (~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(bp + 8 + (uintptr)ii * 4)) > ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(bp + 8 + (uintptr)(ii + (int32)1) * 4))) {
                        rc = _rtreeConstraintError(tls, pRtree, ii + (int32)1);
                        goto constraint;
                    }
                    goto _2;
_2:;
                    ii = ii + (int32)2;
                }
            }
            if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + 2 * 8))) != (int32)SQLITE_NULL) {
                (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp))).Value.FiRowid = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + 2 * 8)));
                if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData))) == (int32)SQLITE_NULL || Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData))) != (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp))).Value.FiRowid) {
                    Xsqlite3_bind_int64(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpReadRowid, (int32)1, (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp))).Value.FiRowid);
                    steprc = Xsqlite3_step(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpReadRowid);
                    rc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpReadRowid);
                    if ((int32)SQLITE_ROW == steprc) {
                        if (Xsqlite3_vtab_on_conflict(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb) == (int32)SQLITE_REPLACE) {
                            rc = _rtreeDeleteRowid(tls, pRtree, (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp))).Value.FiRowid);
                        } else {
                            rc = _rtreeConstraintError(tls, pRtree, 0);
                            goto constraint;
                        }
                    }
                }
                bHaveRowid = (int32)1;
            }
        }
        if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData))) != (int32)SQLITE_NULL) {
            rc = _rtreeDeleteRowid(tls, pRtree, Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData))));
        }
        if (rc == SQLITE_OK && nData > (int32)1) {
            pLeaf = (uintptr)0;
            if (bHaveRowid == 0) {
                rc = _rtreeNewRowid(tls, pRtree, __ccgo_ptr(bp));
            }
            (@unsafe.Pointer.To<Tsqlite_int64>(__ccgo_ptr(pRowid))).Value = (@unsafe.Pointer.To<TRtreeCell>(__ccgo_ptr(bp))).Value.FiRowid;
            if (rc == SQLITE_OK) {
                rc = _ChooseLeaf(tls, pRtree, __ccgo_ptr(bp), 0, @unsafe.Pointer.FromBox(ᏑpLeaf));
            }
            if (rc == SQLITE_OK) {
                rc = _rtreeInsertCell(tls, pRtree, pLeaf, bp, 0);
                rc2 = _nodeRelease(tls, pRtree, pLeaf);
                if (rc == SQLITE_OK) {
                    rc = rc2;
                }
            }
            if (rc == SQLITE_OK && (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnAux != 0) {
                pUp = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FpWriteAux;
                Xsqlite3_bind_int64(tls, pUp, (int32)1, ~@unsafe.Pointer.To<Tsqlite_int64>(__ccgo_ptr(pRowid)));
                jj = 0;
                for (;;) {
                    if (!(jj < libc.Int32FromUint16((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnAux))) {
                        break;
                    }
                    Xsqlite3_bind_value(tls, pUp, jj + (int32)2, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2) + (int32)3 + jj) * 8)));
                    goto _3;
_3:;
                    jj = jj + 1;
                }
                Xsqlite3_step(tls, pUp);
                rc = Xsqlite3_reset(tls, pUp);
            }
        }
        goto constraint;
constraint:;
        _rtreeRelease(tls, pRtree);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sampleCopy(ж<libc.TLS> tls, uintptr p, uintptr pTo, uintptr pFrom) {
    (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pTo)).Value.FisPSample = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FisPSample;
    (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pTo)).Value.FiCol = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FiCol;
    (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pTo)).Value.FiHash = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FiHash;
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pTo)).Value.FanEq, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FanEq, (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol));
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pTo)).Value.FanLt, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FanLt, (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol));
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pTo)).Value.FanDLt, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FanDLt, (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnCol));
    if ((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FnRowid != 0) {
        _sampleSetRowid(tls, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, pTo, libc.Int32FromUint32((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pFrom)).Value.FnRowid), ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pFrom + 24)));
    } else {
        _sampleSetRowidInt64(tls, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, pTo, ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)(pFrom + 24)));
    }
}

internal static void _sampleInsert(ж<libc.TLS> tls, uintptr p, uintptr pNew, int32 nEqZero) {
    uintptr anDLt = default;
    uintptr anEq = default;
    uintptr anLt = default;
    uintptr pMin = default;
    uintptr pOld = default;
    uintptr pSample = default;
    uintptr pUpgrade = default;
    int32 i = default;
    int32 iMin = default;
    _ = anDLt;
    _ = anEq;
    _ = anLt;
    _ = i;
    _ = iMin;
    _ = pMin;
    _ = pOld;
    _ = pSample;
    _ = pUpgrade;
    pSample = (uintptr)0;
    if (nEqZero > (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnMaxEqZero) {
        (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnMaxEqZero = nEqZero;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pNew)).Value.FisPSample) == 0) {
        pUpgrade = (uintptr)0;
        i = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample - (int32)1;
        for (;;) {
            if (!(i >= 0)) {
                break;
            }
            pOld = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)i * 48;
            if (~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pOld)).Value.FanEq + (uintptr)(@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pNew)).Value.FiCol * 8)) == (uint64)0) {
                if ((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pOld)).Value.FisPSample != 0) {
                    return;
                }
                if (pUpgrade == (uintptr)0 || _sampleIsBetter(ref (tls).DerefOrNull(), p, pOld, pUpgrade) != 0) {
                    pUpgrade = pOld;
                }
            }
            goto _1;
_1:;
            i = i - 1;
        }
        if (pUpgrade != 0) {
            (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pUpgrade)).Value.FiCol = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pNew)).Value.FiCol;
            (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pUpgrade)).Value.FanEq + (uintptr)(@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pUpgrade)).Value.FiCol * 8))).Value = (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pNew)).Value.FanEq + (uintptr)(@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pUpgrade)).Value.FiCol * 8))).Value;
            goto find_new_min;
        }
    }
    if ((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample >= (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample) {
        pMin = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FiMin * 48;
        anEq = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pMin)).Value.FanEq;
        anLt = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pMin)).Value.FanLt;
        anDLt = (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pMin)).Value.FanDLt;
        _sampleClear(tls, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fdb, pMin);
        libc.Xmemmove(tls, pMin, pMin + 1 * 48, (uint64)48 * libc.Uint64FromInt32((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample - (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FiMin - libc.Int32FromInt32(1)));
        pSample = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample - (int32)1) * 48;
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pSample)).Value.FnRowid = (uint32)0;
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pSample)).Value.FanEq = anEq;
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pSample)).Value.FanDLt = anDLt;
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pSample)).Value.FanLt = anLt;
        (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample - (int32)1;
    }
    pSample = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample * 48;
    _sampleCopy(tls, p, pSample, pNew);
    (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample = (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample + 1;
    libc.Xmemset(tls, (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)pSample)).Value.FanEq, 0, (uint64)8 * libc.Uint64FromInt32(nEqZero));
    goto find_new_min;
find_new_min:;
    if ((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FnSample >= (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample) {
        iMin = -(int32)1;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FmxSample)) {
                break;
            }
            if ((@unsafe.Pointer.To<TStatSample>(__ccgo_ptr((@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)i * 48))).Value.FisPSample != 0) {
                goto _2;
            }
            if (iMin < 0 || _sampleIsBetter(ref (tls).DerefOrNull(), p, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)iMin * 48, (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.Fa + (uintptr)i * 48) != 0) {
                iMin = i;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TStatAccum>((@unsafe.Pointer)p)).Value.FiMin = iMin;
    }
}

internal static void _sampleSetRowid(ж<libc.TLS> tls, uintptr db, uintptr p, int32 n, uintptr pData) {
    if ((@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid != 0) {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 24)));
    }
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 24))).Value = _sqlite3DbMallocRawNN(tls, db, libc.Uint64FromInt32(n));
    if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 24)) != 0) {
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid = libc.Uint32FromInt32(n);
        libc.Xmemcpy(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 24)), pData, libc.Uint64FromInt32(n));
    } else {
        (@unsafe.Pointer.To<TStatSample>((@unsafe.Pointer)p)).Value.FnRowid = (uint32)0;
    }
}

internal static int32 /*r*/ _saveCursorKey(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pKey = default;
    int32 rc = default;
    _ = pKey;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurIntKey != 0) {
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey = _sqlite3BtreeIntegerKey(tls, pCur);
    } else {
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey = libc.Int64FromUint32(_sqlite3BtreePayloadSize(tls, pCur));
        pKey = _sqlite3Malloc(tls, libc.Uint64FromInt64((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey + (int64)9 + (int64)8));
        if (pKey != 0) {
            rc = _sqlite3BtreePayload(tls, pCur, (uint32)0, libc.Uint32FromInt32((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey), pKey);
            if (rc == SQLITE_OK) {
                libc.Xmemset(tls, pKey + (uintptr)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey, 0, libc.Uint64FromInt32(libc.Int32FromInt32(9) + libc.Int32FromInt32(8)));
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey = pKey;
            } else {
                Xsqlite3_free(tls, pKey);
            }
        } else {
            rc = (int32)SQLITE_NOMEM;
        }
    }
    return rc;
}

internal static int32 /*r*/ _selectCheckOnClausesSelect(ж<libc.TLS> tls, uintptr pWalker, uintptr pSelect) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr pCtx = default;
        TCheckOnCtx _ᴛ1 = default;
        _ = pCtx;
        pCtx = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc == (@unsafe.Pointer.To<TCheckOnCtx>((@unsafe.Pointer)pCtx)).Value.FpSrc || (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc)).Value.FnSrc == 0) {
            r = WRC_Continue; goto ᒐdone;
        } else {
            libc.Xmemset(tls, bp, 0, (uint64)24);
            (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp))).Value.FpSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpSrc;
            (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp))).Value.FpParent = pCtx;
            (@unsafe.Pointer.To<TCheckOnCtx>(__ccgo_ptr(bp))).Value.FiJoin = -(int32)1;
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40))).Value = bp;
            _sqlite3WalkSelect(tls, pWalker, pSelect);
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40))).Value = pCtx;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pSelect + 4))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(SF_OnToWhere));
            r = (int32)WRC_Prune; goto ᒐdone;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _selectWindowRewriteExprCb(ж<libc.TLS> tls, uintptr pWalker, uintptr pExpr) {
    int32 f = default;
    int32 i = default;
    int32 i1 = default;
    int32 iCol = default;
    int32 iJoin = default;
    int32 nSrc = default;
    int32 v4 = default;
    uintptr p = default;
    uintptr pDup = default;
    uintptr pParse = default;
    uintptr pWin = default;
    _ = f;
    _ = i;
    _ = i1;
    _ = iCol;
    _ = iJoin;
    _ = nSrc;
    _ = p;
    _ = pDup;
    _ = pParse;
    _ = pWin;
    _ = v4;
    p = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalker + 40));
    pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
    if ((@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSubSelect != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_COLUMN) {
            return WRC_Continue;
        } else {
            nSrc = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc;
            i = 0;
            for (;;) {
                if (!(i < nSrc)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable == (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSrc + 8 + (uintptr)i * 80))).Value.FiCursor) {
                    break;
                }
                goto _1;
_1:;
                i = i + 1;
            }
            if (i == nSrc) {
                return WRC_Continue;
            }
        }
    }
    var exprᴛ1 = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop);
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)TK_FUNCTION) { matchᴛ1 = true;
        do {
            if (!((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc))) != libc.Uint32FromInt32(0))) {
                break;
            } else {
                pWin = (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpWin;
                for (;;) {
                    if (!(pWin != 0)) {
                        break;
                    }
                    if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64)) == pWin) {
                        return (int32)WRC_Prune;
                    }
                    goto _2;
_2:;
                    pWin = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpNextWin;
                }
            }
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_IF_NULL_ROW) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_AGG_FUNCTION) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_COLUMN) {
        iCol = -(int32)1;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0) {
            return (int32)WRC_Abort;
        }
        if ((@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub != 0) {
            i1 = 0;
            for (;;) {
                if (!(i1 < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub)).Value.FnExpr)) {
                    break;
                }
                if (0 == _sqlite3ExprCompare(tls, (uintptr)0, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub + 8 + (uintptr)i1 * 32))).Value.FpExpr, pExpr, -(int32)1)) {
                    iCol = i1;
                    break;
                }
                goto _3;
_3:;
                i1 = i1 + 1;
            }
        }
        if (iCol < 0) {
            pDup = _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr, 0);
            if (pDup != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pDup)).Value.Fop) == (int32)TK_AGG_FUNCTION) {
                (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pDup)).Value.Fop = (uint8)TK_FUNCTION;
            }
            (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub = _sqlite3ExprListAppend(tls, pParse, (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub, pDup);
        }
        if ((@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub != 0) {
            f = libc.Int32FromUint32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Collate) | libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_InnerON))));
            iJoin = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pExpr + 52));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pExpr + 4))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(EP_Static));
            _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pExpr + 4))).Value &= ~libc.Uint32FromInt32(libc.Int32FromInt32(EP_Static));
            libc.Xmemset(tls, pExpr, 0, (uint64)72);
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop = (uint8)TK_COLUMN;
            if (iCol < 0) {
                v4 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpSub)).Value.FnExpr - (int32)1;
            } else {
                v4 = iCol;
            }
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn = (int16)v4;
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpWin)).Value.FiEphCsr;
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64))).Value = (@unsafe.Pointer.To<TWindowRewrite>((@unsafe.Pointer)p)).Value.FpTab;
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags = libc.Uint32FromInt32(f);
            if ((f & (libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_InnerON))) != 0) {
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pExpr + 52))).Value = iJoin;
            }
        }
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0) {
            return (int32)WRC_Abort;
        }
    }
    else if (!matchᴛ1) { /* default: */
        do {
            break;
        } while (false);
    }

    return WRC_Continue;
}

internal static void _sessionAppendBlob(ж<libc.TLS> tls, uintptr p, uintptr aBlob, int32 nBlob, @unsafe.Pointer pRc) {
    if (nBlob > 0 && 0 == _sessionBufferGrow(tls, (@unsafe.Pointer)p, (int64)nBlob, pRc)) {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf, aBlob, libc.Uint64FromInt32(nBlob));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 8))).Value += nBlob;
    }
}

internal static void _sessionAppendPartialUpdate(ж<libc.TLS> tls, uintptr pBuf, uintptr pIter, uintptr aRec, int32 nRec, uintptr aChange, int32 nChange, @unsafe.Pointer pRc) {
    uintptr a1 = default;
    uintptr a2 = default;
    uintptr a2Eof = default;
    uintptr pOut = default;
    uintptr v1 = default;
    int32 bData = default;
    int32 i = default;
    int32 n1 = default;
    int32 n11 = default;
    int32 n2 = default;
    int32 n21 = default;
    int32 v4 = default;
    _ = a1;
    _ = a2;
    _ = a2Eof;
    _ = bData;
    _ = i;
    _ = n1;
    _ = n11;
    _ = n2;
    _ = n21;
    _ = pOut;
    _ = v1;
    _ = v4;
    _sessionBufferGrow(tls, (@unsafe.Pointer)pBuf, libc.Int64FromInt32(2) + (int64)nRec + (int64)nChange, pRc);
    if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
        bData = 0;
        pOut = (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf;
        a1 = aRec;
        a2 = aChange;
        a2Eof = a2 + (uintptr)nChange;
        v1 = pOut;
        pOut = pOut + 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))).Value = (uint8)SQLITE_UPDATE;
        v1 = pOut;
        pOut = pOut + 1;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))).Value = libc.Uint8FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbIndirect);
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol)) {
                break;
            }
            n1 = _sessionSerialLen(ref (tls).DerefOrNull(), a1);
            if (a2 >= a2Eof) {
                v4 = 0;
            } else {
                v4 = _sessionSerialLen(ref (tls).DerefOrNull(), a2);
            }
            n2 = v4;
            if (n2 <= 0 || ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FabPK + (uintptr)i)) != 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a2))) == 0) {
                if (!(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FabPK + (uintptr)i)) != 0) && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a1)) != 0) {
                    bData = (int32)1;
                }
                libc.Xmemcpy(tls, pOut, a1, libc.Uint64FromInt32(n1));
                pOut = pOut + (uintptr)n1;
            } else {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a2))) != (int32)0xFF && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a1)) != 0) {
                    bData = (int32)1;
                    libc.Xmemcpy(tls, pOut, a2, libc.Uint64FromInt32(n2));
                    pOut = pOut + (uintptr)n2;
                } else {
                    v1 = pOut;
                    pOut = pOut + 1;
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))).Value = (uint8)'\u0000';
                }
            }
            a1 = a1 + (uintptr)n1;
            a2 = a2 + (uintptr)n2;
            goto _3;
_3:;
            i = i + 1;
        }
        if (bData != 0) {
            a2 = aChange;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol)) {
                    break;
                }
                n11 = _sessionSerialLen(ref (tls).DerefOrNull(), a1);
                n21 = _sessionSerialLen(ref (tls).DerefOrNull(), a2);
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FabPK + (uintptr)i)) != 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a2))) != (int32)0xFF) {
                    libc.Xmemcpy(tls, pOut, a1, libc.Uint64FromInt32(n11));
                    pOut = pOut + (uintptr)n11;
                } else {
                    v1 = pOut;
                    pOut = pOut + 1;
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))).Value = (uint8)'\u0000';
                }
                a1 = a1 + (uintptr)n11;
                a2 = a2 + (uintptr)n21;
                goto _6;
_6:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = (int32)((int64)pOut - (int64)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf);
        }
    }
}

internal static void _sessionAppendRecordMerge(ж<libc.TLS> tls, uintptr pBuf, int32 nCol, uintptr a1, int32 n1, uintptr a2, int32 n2, @unsafe.Pointer pRc) {
    uintptr a1Eof = default;
    uintptr a2Eof = default;
    uintptr pOut = default;
    int32 i = default;
    int32 nn1 = default;
    int32 nn2 = default;
    int32 v2 = default;
    int32 v3 = default;
    _ = a1Eof;
    _ = a2Eof;
    _ = i;
    _ = nn1;
    _ = nn2;
    _ = pOut;
    _ = v2;
    _ = v3;
    a1Eof = a1 + (uintptr)n1;
    a2Eof = a2 + (uintptr)n2;
    _sessionBufferGrow(tls, (@unsafe.Pointer)pBuf, (int64)n1 + (int64)n2, pRc);
    if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
        pOut = (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf;
        i = 0;
        for (;;) {
            if (!(i < nCol)) {
                break;
            }
            if (a1 < a1Eof) {
                v2 = _sessionSerialLen(ref (tls).DerefOrNull(), a1);
            } else {
                v2 = 0;
            }
            nn1 = v2;
            if (a2 < a2Eof) {
                v3 = _sessionSerialLen(ref (tls).DerefOrNull(), a2);
            } else {
                v3 = 0;
            }
            nn2 = v3;
            if (nn1 == 0 || nn2 > 0 && (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a1))) == 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a1))) == (int32)0xFF)) {
                libc.Xmemcpy(tls, pOut, a2, libc.Uint64FromInt32(nn2));
                pOut = pOut + (uintptr)nn2;
            } else {
                libc.Xmemcpy(tls, pOut, a1, libc.Uint64FromInt32(nn1));
                pOut = pOut + (uintptr)nn1;
            }
            a1 = a1 + (uintptr)nn1;
            a2 = a2 + (uintptr)nn2;
            goto _1;
_1:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = (int32)((int64)pOut - (int64)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FaBuf);
    }
}

internal static void _sessionAppendStr(ж<libc.TLS> tls, uintptr p, uintptr zStr, @unsafe.Pointer pRc) {
    int32 nStr = default;
    _ = nStr;
    nStr = _sqlite3Strlen30(tls, zStr);
    if (0 == _sessionBufferGrow(tls, (@unsafe.Pointer)p, (int64)nStr + (int64)1, pRc)) {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf, zStr, libc.Uint64FromInt32(nStr));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 8))).Value += nStr;
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)p)).Value.FnBuf))).Value = (uint8)0x00;
    }
}

internal static int32 /*r*/ _sessionAppendUpdate(ж<libc.TLS> tls, uintptr pBuf, int32 bPatchset, uintptr pStmt, uintptr p, uintptr abPK) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 bChanged = default;
        int32 bNoop = default;
        int32 eType = default;
        int32 i = default;
        ref var n = ref heap(new int32(), out var Ꮡn);
        int32 nAdvance = default;
        int32 nHdr = default;
        int32 nRewind = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        uintptr pCsr = default;
        TSessionBuffer _ᴛ1 = default;
        float64 _ᴛ2 = default;
        Tsqlite3_int64 _ᴛ3 = default;
        _ = bChanged;
        _ = bNoop;
        _ = eType;
        _ = i;
        _ = n;
        _ = nAdvance;
        _ = nHdr;
        _ = nRewind;
        _ = pCsr;
        _ = rc;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value = new TSessionBuffer();
        bNoop = (int32)1;
        nRewind = (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf;
        pCsr = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FaRecord;
        _sessionAppendByte(tls, (@unsafe.Pointer)pBuf, (uint8)SQLITE_UPDATE, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        _sessionAppendByte(tls, (@unsafe.Pointer)pBuf, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FbIndirect, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        i = 0;
        for (;;) {
            if (!(i < Xsqlite3_column_count(tls, pStmt))) {
                break;
            }
            bChanged = 0;
            eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCsr)));
            var matchᴛ1 = false;
            if (eType == (int32)SQLITE_NULL) { matchᴛ1 = true;
                nAdvance = (int32)1;
                if (Xsqlite3_column_type(tls, pStmt, i) != (int32)SQLITE_NULL) {
                    bChanged = (int32)1;
                }
            }
            else if (eType == (int32)SQLITE_FLOAT) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && eType == (int32)SQLITE_INTEGER) {
                do {
                    nAdvance = (int32)9;
                    if (eType == Xsqlite3_column_type(tls, pStmt, i)) {
                        (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16))).Value = _sessionGetI64(ref (tls).DerefOrNull(), pCsr + 1);
                        if (eType == (int32)SQLITE_INTEGER) {
                            if (~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16)) == Xsqlite3_column_int64(tls, pStmt, i)) {
                                break;
                            }
                        } else {
                            libc.Xmemcpy(tls, bp + 24, bp + 16, (uint64)8);
                            if (~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 24)) == Xsqlite3_column_double(tls, pStmt, i)) {
                                break;
                            }
                        }
                    }
                    bChanged = (int32)1;
                } while (false);
            }
            else if (!matchᴛ1) { /* default: */
                do {
                    nHdr = (int32)1 + _sessionVarintGet(ref (tls).DerefOrNull(), pCsr + 1, @unsafe.Pointer.FromPinnedBox(Ꮡn));
                    nAdvance = nHdr + n;
                    if (eType == Xsqlite3_column_type(tls, pStmt, i) && n == Xsqlite3_column_bytes(tls, pStmt, i) && (n == 0 || 0 == libc.Xmemcmp(tls, pCsr + (uintptr)nHdr, Xsqlite3_column_blob(tls, pStmt, i), libc.Uint64FromInt32(n)))) {
                        break;
                    }
                    bChanged = (int32)1;
                } while (false);
            }

            if (bChanged != 0) {
                bNoop = 0;
            }
            if (bPatchset == 0) {
                if (bChanged != 0 || ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i)) != 0) {
                    _sessionAppendBlob(tls, pBuf, pCsr, nAdvance, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                } else {
                    _sessionAppendByte(tls, (@unsafe.Pointer)pBuf, (uint8)0, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                }
            }
            if (bChanged != 0 || bPatchset != 0 && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i)) != 0) {
                _sessionAppendCol(tls, bp, pStmt, i, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            } else {
                _sessionAppendByte(tls, __ccgo_ptr(bp), (uint8)0, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            }
            pCsr = pCsr + (uintptr)nAdvance;
            goto _1;
_1:;
            i = i + 1;
        }
        if (bNoop != 0) {
            (@unsafe.Pointer.To<TSessionBuffer>((@unsafe.Pointer)pBuf)).Value.FnBuf = nRewind;
        } else {
            _sessionAppendBlob(tls, pBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sessionChangeMerge(ж<libc.TLS> tls, uintptr pTab, int32 bRebase, int32 bPatchset, uintptr pExist, int32 op2, int32 bIndirect, uintptr aRec, int32 nRec, @unsafe.Pointer ppNew) {
    uintptr a1 = default;
    ref var a11 = ref heap(new uintptr(), out var Ꮡa11);
    ref var a12 = ref heap(new uintptr(), out var Ꮡa12);
    uintptr a2 = default;
    ref var a21 = ref heap(new uintptr(), out var Ꮡa21);
    ref var aCsr = ref heap(new uintptr(), out var ᏑaCsr);
    uintptr aExist = default;
    uintptr pIn = default;
    uintptr pNew = default;
    uintptr pOut = default;
    uintptr pOut1 = default;
    uintptr v2 = default;
    int32 i = default;
    int32 i1 = default;
    int32 n1 = default;
    int32 n2 = default;
    int32 nIn = default;
    int32 op1 = default;
    int32 rc = default;
    Tsqlite3_int64 nByte = default;
    Tsqlite3_int64 nByte1 = default;
    _ = a1;
    _ = a11;
    _ = a12;
    _ = a2;
    _ = a21;
    _ = aCsr;
    _ = aExist;
    _ = i;
    _ = i1;
    _ = n1;
    _ = n2;
    _ = nByte;
    _ = nByte1;
    _ = nIn;
    _ = op1;
    _ = pIn;
    _ = pNew;
    _ = pOut;
    _ = pOut1;
    _ = rc;
    _ = v2;
    pNew = (uintptr)0;
    rc = SQLITE_OK;
    if (!(pExist != 0)) {
        pNew = Xsqlite3_malloc64(tls, (uint64)((uint64)32 + libc.Uint64FromInt32(nRec)));
        if (!(pNew != 0)) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemset(tls, pNew, 0, (uint64)32);
        (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.Fop = libc.Uint8FromInt32(op2);
        (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FbIndirect = libc.Uint8FromInt32(bIndirect);
        (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord = pNew + 1 * 32;
        if (bIndirect == 0 || bRebase == 0) {
            (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord = nRec;
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord, aRec, libc.Uint64FromInt32(nRec));
        } else {
            pIn = aRec;
            pOut = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord;
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                    break;
                }
                nIn = _sessionSerialLen(ref (tls).DerefOrNull(), pIn);
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pIn))) == 0) {
                    v2 = pOut;
                    pOut = pOut + 1;
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2))).Value = (uint8)0;
                } else {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i))) == 0) {
                        v2 = pOut;
                        pOut = pOut + 1;
                        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2))).Value = (uint8)0xFF;
                    } else {
                        libc.Xmemcpy(tls, pOut, pIn, libc.Uint64FromInt32(nIn));
                        pOut = pOut + (uintptr)nIn;
                    }
                }
                pIn = pIn + (uintptr)nIn;
                goto _1;
_1:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord = (int32)((int64)pOut - (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord);
        }
    } else {
        if (bRebase != 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.Fop) == (int32)SQLITE_DELETE && (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FbIndirect != 0) {
                (@unsafe.Pointer.To<uintptr>(ppNew)).Value = pExist;
            } else {
                nByte = libc.Int64FromUint64(libc.Uint64FromInt32(nRec + (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FnRecord) + (uint64)32);
                pNew = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
                if (pNew == (uintptr)0) {
                    rc = (int32)SQLITE_NOMEM;
                } else {
                    a1 = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FaRecord;
                    a2 = aRec;
                    libc.Xmemset(tls, pNew, 0, libc.Uint64FromInt64(nByte));
                    (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FbIndirect = libc.BoolUint8(bIndirect != 0 || (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FbIndirect != 0);
                    (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.Fop = libc.Uint8FromInt32(op2);
                    v2 = pNew + 1 * 32;
                    (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord = v2;
                    pOut1 = v2;
                    i1 = 0;
                    for (;;) {
                        if (!(i1 < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                            break;
                        }
                        n1 = _sessionSerialLen(ref (tls).DerefOrNull(), a1);
                        n2 = _sessionSerialLen(ref (tls).DerefOrNull(), a2);
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a1))) == (int32)0xFF || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i1))) == 0 && bIndirect != 0) {
                            v2 = pOut1;
                            pOut1 = pOut1 + 1;
                            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2))).Value = (uint8)0xFF;
                        } else {
                            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a2))) == 0) {
                                libc.Xmemcpy(tls, pOut1, a1, libc.Uint64FromInt32(n1));
                                pOut1 = pOut1 + (uintptr)n1;
                            } else {
                                libc.Xmemcpy(tls, pOut1, a2, libc.Uint64FromInt32(n2));
                                pOut1 = pOut1 + (uintptr)n2;
                            }
                        }
                        a1 = a1 + (uintptr)n1;
                        a2 = a2 + (uintptr)n2;
                        goto _5;
_5:;
                        i1 = i1 + 1;
                    }
                    (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord = (int32)((int64)pOut1 - (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord);
                }
                Xsqlite3_free(tls, pExist);
            }
        } else {
            op1 = libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.Fop);
            if (op1 == (int32)SQLITE_INSERT && op2 == (int32)SQLITE_INSERT || op1 == (int32)SQLITE_UPDATE && op2 == (int32)SQLITE_INSERT || op1 == (int32)SQLITE_DELETE && op2 == (int32)SQLITE_UPDATE || op1 == (int32)SQLITE_DELETE && op2 == (int32)SQLITE_DELETE) {
                pNew = pExist;
            } else {
                if (op1 == (int32)SQLITE_INSERT && op2 == (int32)SQLITE_DELETE) {
                    Xsqlite3_free(tls, pExist);
                } else {
                    aExist = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FaRecord;
                    nByte1 = libc.Int64FromUint64((uint64)32 + libc.Uint64FromInt32((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FnRecord) + libc.Uint64FromInt32(nRec));
                    pNew = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte1));
                    if (!(pNew != 0)) {
                        Xsqlite3_free(tls, pExist);
                        return (int32)SQLITE_NOMEM;
                    }
                    libc.Xmemset(tls, pNew, 0, (uint64)32);
                    (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FbIndirect = libc.BoolUint8(bIndirect != 0 && (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pExist)).Value.FbIndirect != 0);
                    v2 = pNew + 1 * 32;
                    (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord = v2;
                    aCsr = v2;
                    if (op1 == (int32)SQLITE_INSERT) {
                        a11 = aRec;
                        (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.Fop = (uint8)SQLITE_INSERT;
                        if (bPatchset == 0) {
                            _sessionSkipRecord(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(Ꮡa11), (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol);
                        }
                        _sessionMergeRecord(tls, @unsafe.Pointer.FromBox(ᏑaCsr), (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol, aExist, a11);
                    } else {
                        if (op1 == (int32)SQLITE_DELETE) {
                            (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.Fop = (uint8)SQLITE_UPDATE;
                            if (bPatchset != 0) {
                                libc.Xmemcpy(tls, aCsr, aRec, libc.Uint64FromInt32(nRec));
                                aCsr = aCsr + (uintptr)nRec;
                            } else {
                                if (0 == _sessionMergeUpdate(tls, @unsafe.Pointer.FromBox(ᏑaCsr), pTab, bPatchset, aExist, (uintptr)0, aRec, (uintptr)0)) {
                                    Xsqlite3_free(tls, pNew);
                                    pNew = (uintptr)0;
                                }
                            }
                        } else {
                            if (op2 == (int32)SQLITE_UPDATE) {
                                a12 = aExist;
                                a21 = aRec;
                                if (bPatchset == 0) {
                                    _sessionSkipRecord(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(Ꮡa12), (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol);
                                    _sessionSkipRecord(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(Ꮡa21), (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol);
                                }
                                (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.Fop = (uint8)SQLITE_UPDATE;
                                if (0 == _sessionMergeUpdate(tls, @unsafe.Pointer.FromBox(ᏑaCsr), pTab, bPatchset, aRec, aExist, a12, a21)) {
                                    Xsqlite3_free(tls, pNew);
                                    pNew = (uintptr)0;
                                }
                            } else {
                                (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.Fop = (uint8)SQLITE_DELETE;
                                if (bPatchset != 0) {
                                    libc.Xmemcpy(tls, aCsr, aRec, libc.Uint64FromInt32(nRec));
                                    aCsr = aCsr + (uintptr)nRec;
                                } else {
                                    _sessionMergeRecord(tls, @unsafe.Pointer.FromBox(ᏑaCsr), (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol, aRec, aExist);
                                }
                            }
                        }
                    }
                    if (pNew != 0) {
                        (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord = (int32)((int64)aCsr - (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord);
                    }
                    Xsqlite3_free(tls, pExist);
                }
            }
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppNew)).Value = pNew;
    return rc;
}

internal static int32 /*r*/ _sessionChangesetFindTable(ж<libc.TLS> tls, uintptr pGrp, uintptr zTab, uintptr pIter, @unsafe.Pointer ppTab) {
    ref var abPK = ref heap(new uintptr(), out var ᏑabPK);
    uintptr pTab = default;
    uintptr ppNew = default;
    ref var nCol = ref heap(new int32(), out var ᏑnCol);
    int32 nTab = default;
    int32 rc = default;
    int32 v2 = default;
    _ = abPK;
    _ = nCol;
    _ = nTab;
    _ = pTab;
    _ = ppNew;
    _ = rc;
    _ = v2;
    rc = SQLITE_OK;
    pTab = (uintptr)0;
    nTab = libc.Int32FromUint64(libc.Xstrlen(tls, zTab));
    abPK = (uintptr)0;
    nCol = 0;
    (@unsafe.Pointer.To<uintptr>(ppTab)).Value = (uintptr)0;
    pTab = (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FpList;
    for (;;) {
        if (!(pTab != 0)) {
            break;
        }
        if (0 == Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zTab, nTab + (int32)1)) {
            break;
        }
        goto _1;
_1:;
        pTab = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FpNext;
    }
    if (pIter != 0) {
        _sqlite3changeset_pk__ccgo_gp(ref (tls).DerefOrNull(), pIter, @unsafe.Pointer.FromBox(ᏑabPK), @unsafe.Pointer.FromPinnedBox(ᏑnCol));
    } else {
        if (!(pTab != 0) && !((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fdb != 0)) {
            return (int32)SQLITE_ERROR;
        }
    }
    if (!(pTab != 0)) {
        pTab = Xsqlite3_malloc64(tls, (uint64)((uint64)88 + libc.Uint64FromInt32(nCol) + libc.Uint64FromInt32(nTab) + (uint64)1));
        if (!(pTab != 0)) {
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemset(tls, pTab, 0, (uint64)88);
        (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol = nCol;
        (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK = pTab + 1 * 88;
        if (nCol > 0) {
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK, abPK, libc.Uint64FromInt32(nCol));
        }
        (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)nCol;
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zTab, libc.Uint64FromInt32(nTab + (int32)1));
        if ((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fdb != 0) {
            (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol = 0;
            rc = _sessionInitTable(tls, (uintptr)0, pTab, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fdb, (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FzDb);
            if (rc != 0 || (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol == 0) {
                Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FazCol);
                Xsqlite3_free(tls, pTab);
                if (rc != 0) {
                    v2 = rc;
                } else {
                    v2 = (int32)SQLITE_ERROR;
                }
                return v2;
            }
        }
        ppNew = pGrp + 8;
        for (;;) {
            if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew)) != 0)) {
                break;
            }
            goto _3;
_3:;
            ppNew = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew));
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew))).Value = pTab;
    }
    if (pIter != 0 && !(_sessionChangesetCheckCompat(tls, pTab, nCol, abPK) != 0)) {
        rc = (int32)SQLITE_SCHEMA;
    }
    (@unsafe.Pointer.To<uintptr>(ppTab)).Value = pTab;
    return rc;
}

internal static int32 /*r*/ _sessionChangesetInvert(ж<libc.TLS> tls, uintptr pInput, uintptr __ccgo_fp_xOutput, uintptr pOut, uintptr pnInverted, uintptr ppInverted) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr abPK = default;
        uintptr apVal = default;
        uintptr pVal = default;
        uintptr pVal1 = default;
        uintptr v7 = default;
        int32 bIndirect = default;
        int32 eType2 = default;
        int32 iCol = default;
        ref var nByte = ref heap(new int32(), out var ᏑnByte);
        ref var nByte1 = ref heap(new int32(), out var ᏑnByte1);
        ref var nCol = ref heap(new int32(), out var ᏑnCol);
        int32 nVar = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        int32 v1 = default;
        Tu8 eType = default;
        TSessionBuffer _ᴛ1 = default;
        TSessionBuffer _ᴛ2 = default;
        _ = abPK;
        _ = apVal;
        _ = bIndirect;
        _ = eType;
        _ = eType2;
        _ = iCol;
        _ = nByte;
        _ = nByte1;
        _ = nCol;
        _ = nVar;
        _ = pVal;
        _ = pVal1;
        _ = rc;
        _ = v1;
        _ = v7;
        rc = SQLITE_OK;
        nCol = 0;
        abPK = (uintptr)0;
        apVal = (uintptr)0;
        (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp + 16))).Value = new TSessionBuffer();
        libc.Xmemset(tls, bp, 0, (uint64)16);
        if (ppInverted != 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppInverted))).Value = (uintptr)0;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnInverted))).Value = 0;
        }
        while ((int32)1 != 0) {
            v1 = _sessionInputBuffer(tls, pInput, (int32)2);
            rc = v1;
            if (v1 != 0) {
                goto finished_invert;
            }
            if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext + (int32)1 >= (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FnData) {
                if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext != (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FnData) {
                    rc = _sqlite3CorruptError(tls, (int32)239673);
                    goto finished_invert;
                }
                break;
            }
            eType = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext));
            var exprᴛ1 = libc.Int32FromUint8(eType);
            var matchᴛ1 = false;
            if (exprᴛ1 == (int32)'T') { matchᴛ1 = true;
                (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext + 1;
                v1 = _sessionChangesetBufferTblhdr(tls, pInput, @unsafe.Pointer.FromPinnedBox(ᏑnByte));
                rc = v1;
                if (v1 != 0) {
                    goto finished_invert;
                }
                nVar = _sessionVarintGet(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext, @unsafe.Pointer.FromPinnedBox(ᏑnCol));
                (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp + 16))).Value.FnBuf = 0;
                _sessionAppendBlob(tls, bp + 16, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext + nVar), nCol, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendByte(tls, __ccgo_ptr(bp), eType, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendBlob(tls, bp, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext, nByte, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                if (rc != 0) {
                    goto finished_invert;
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pInput + 8))).Value += nByte;
                Xsqlite3_free(tls, apVal);
                apVal = (uintptr)0;
                abPK = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp + 16))).Value.FaBuf;
            }
            else if (exprᴛ1 == (int32)SQLITE_INSERT) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)SQLITE_DELETE) { matchᴛ1 = true;
                bIndirect = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext + (int32)1))));
                if (libc.Int32FromUint8(eType) == (int32)SQLITE_DELETE) {
                    v1 = (int32)SQLITE_INSERT;
                } else {
                    v1 = (int32)SQLITE_DELETE;
                }
                eType2 = v1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pInput + 8))).Value += (int32)2;
                rc = _sessionChangesetBufferRecord(tls, pInput, nCol, @unsafe.Pointer.FromPinnedBox(ᏑnByte1));
                _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32(eType2), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32(bIndirect), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendBlob(tls, bp, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext, nByte1, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pInput + 8))).Value += nByte1;
                if (rc != 0) {
                    goto finished_invert;
                }
            }
            else if (exprᴛ1 == (int32)SQLITE_UPDATE) {
                if ((uintptr)0 == apVal) {
                    apVal = Xsqlite3_malloc64(tls, (uint64)((uint64)8 * libc.Uint64FromInt32(nCol) * (uint64)2));
                    if ((uintptr)0 == apVal) {
                        rc = (int32)SQLITE_NOMEM;
                        goto finished_invert;
                    }
                    libc.Xmemset(tls, apVal, 0, (uint64)8 * libc.Uint64FromInt32(nCol) * (uint64)2);
                }
                _sessionAppendByte(tls, __ccgo_ptr(bp), eType, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendByte(tls, __ccgo_ptr(bp), ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FaData + (uintptr)((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pInput)).Value.FiNext + (int32)1))), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pInput + 8))).Value += (int32)2;
                rc = _sessionReadRecord(tls, pInput, nCol, (uintptr)0, apVal, nil);
                if (rc == SQLITE_OK) {
                    rc = _sessionReadRecord(tls, pInput, nCol, (uintptr)0, apVal + (uintptr)nCol * 8, nil);
                }
                iCol = 0;
                for (;;) {
                    if (!(iCol < nCol)) {
                        break;
                    }
                    if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)iCol)) != 0) {
                        v1 = 0;
                    } else {
                        v1 = nCol;
                    }
                    pVal = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)(iCol + v1) * 8));
                    _sessionAppendValue(tls, bp, pVal, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    goto _4;
_4:;
                    iCol = iCol + 1;
                }
                iCol = 0;
                for (;;) {
                    if (!(iCol < nCol)) {
                        break;
                    }
                    if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)iCol)) != 0) {
                        v7 = (uintptr)0;
                    } else {
                        v7 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)iCol * 8));
                    }
                    pVal1 = v7;
                    _sessionAppendValue(tls, bp, pVal1, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    goto _6;
_6:;
                    iCol = iCol + 1;
                }
                iCol = 0;
                for (;;) {
                    if (!(iCol < nCol * (int32)2)) {
                        break;
                    }
                    _sqlite3ValueFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)iCol * 8)));
                    goto _8;
_8:;
                    iCol = iCol + 1;
                }
                libc.Xmemset(tls, apVal, 0, (uint64)8 * libc.Uint64FromInt32(nCol) * (uint64)2);
                if (rc != SQLITE_OK) {
                    goto finished_invert;
                }
            }
            else if (!matchᴛ1) { /* default: */
                rc = _sqlite3CorruptError(tls, (int32)239776);
                goto finished_invert;
            }

            if (__ccgo_fp_xOutput != 0 && (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf >= _sessions_strm_chunk_size) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xOutput)(tls, pOut, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf);
                (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf = 0;
                if (rc != SQLITE_OK) {
                    goto finished_invert;
                }
            }
        }
        if (pnInverted != 0 && ppInverted != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnInverted))).Value = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppInverted))).Value = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf;
            (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf = (uintptr)0;
        } else {
            if ((@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf > 0 && __ccgo_fp_xOutput != (uintptr)0) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xOutput)(tls, pOut, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf);
            }
        }
        goto finished_invert;
finished_invert:;
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf);
        Xsqlite3_free(tls, apVal);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp + 16))).Value.FaBuf);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sessionChangesetNextOne(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer paRec, @unsafe.Pointer pnRec, @unsafe.Pointer pbNew, @unsafe.Pointer pbEmpty) {
    uintptr abPK = default;
    uintptr apNew = default;
    uintptr apOld = default;
    uintptr v10 = default;
    uintptr v3 = default;
    uintptr v6 = default;
    int32 i = default;
    int32 nVal = default;
    int32 v2 = default;
    Tu8 op = default;
    _ = abPK;
    _ = apNew;
    _ = apOld;
    _ = i;
    _ = nVal;
    _ = op;
    _ = v10;
    _ = v2;
    _ = v3;
    _ = v6;
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
        return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol * (int32)2)) {
                break;
            }
            _sqlite3ValueFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)i * 8)));
            goto _1;
_1:;
            i = i + 1;
        }
        libc.Xmemset(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue, 0, (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol) * (uint64)2);
    }
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = _sessionInputBuffer(tls, p, (int32)2);
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
        return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
    }
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiCurrent = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext;
    _sessionDiscardData(tls, p);
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext >= (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FnData) {
        return (int32)SQLITE_DONE;
    }
    v3 = p + 8;
    v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
    op = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FaData + (uintptr)v2));
    while (libc.Int32FromUint8(op) == (int32)'T' || libc.Int32FromUint8(op) == (int32)'P') {
        if (pbNew != nil) {
            (@unsafe.Pointer.To<int32>(pbNew)).Value = (int32)1;
        }
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset = libc.BoolInt32(libc.Int32FromUint8(op) == (int32)'P');
        if (_sessionChangesetReadTblhdr(tls, p) != 0) {
            return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
        }
        v2 = _sessionInputBuffer(tls, p, (int32)2);
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = v2;
        if (v2 != 0) {
            return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
        }
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiCurrent = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext;
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext >= (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FnData) {
            return (int32)SQLITE_DONE;
        }
        v3 = p + 8;
        v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
        op = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FaData + (uintptr)v2));
    }
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FzTab == (uintptr)0 || (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset != 0 && (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbInvert != 0) {
        v2 = _sqlite3CorruptError(tls, (int32)239353);
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = v2;
        return v2;
    }
    if (libc.Int32FromUint8(op) != (int32)SQLITE_UPDATE && libc.Int32FromUint8(op) != (int32)SQLITE_DELETE && libc.Int32FromUint8(op) != (int32)SQLITE_INSERT || (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext >= (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FnData) {
        v2 = _sqlite3CorruptError(tls, (int32)239359);
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = v2;
        return v2;
    }
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop = libc.Int32FromUint8(op);
    v3 = p + 8;
    v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbIndirect = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FaData + (uintptr)v2)));
    if (paRec != nil) {
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset == 0 && libc.Int32FromUint8(op) == (int32)SQLITE_UPDATE) {
            nVal = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol * (int32)2;
        } else {
            if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset != 0 && libc.Int32FromUint8(op) == (int32)SQLITE_DELETE) {
                nVal = 0;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol)) {
                        break;
                    }
                    if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK + (uintptr)i)) != 0) {
                        nVal = nVal + 1;
                    }
                    goto _11;
_11:;
                    i = i + 1;
                }
            } else {
                nVal = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol;
            }
        }
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = _sessionChangesetBufferRecord(tls, p, nVal, pnRec);
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
            return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
        }
        (@unsafe.Pointer.To<uintptr>(paRec)).Value = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FaData + (uintptr)(@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext;
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext += ~@unsafe.Pointer.To<int32>(pnRec);
    } else {
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbInvert != 0) {
            v3 = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)(@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol * 8;
        } else {
            v3 = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue;
        }
        apOld = v3;
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbInvert != 0) {
            v6 = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue;
        } else {
            v6 = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)(@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol * 8;
        }
        apNew = v6;
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop != (int32)SQLITE_INSERT && ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset == 0 || (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_DELETE)) {
            if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset != 0) {
                v10 = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK;
            } else {
                v10 = (uintptr)0;
            }
            abPK = v10;
            (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = _sessionReadRecord(tls, p, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol, abPK, apOld, nil);
            if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
                return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
            }
        }
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop != (int32)SQLITE_DELETE) {
            (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = _sessionReadRecord(tls, p, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol, (uintptr)0, apNew, pbEmpty);
            if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
                return (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc;
            }
        }
        if (((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset != 0 || (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbInvert != 0) && (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_UPDATE) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol)) {
                    break;
                }
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK + (uintptr)i)) != 0) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)i * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)(i + (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol) * 8))).Value;
                    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)i * 8)) == (uintptr)0) {
                        v2 = _sqlite3CorruptError(tls, (int32)239405);
                        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = v2;
                        return v2;
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)(i + (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol) * 8))).Value = (uintptr)0;
                }
                goto _15;
_15:;
                i = i + 1;
            }
        } else {
            if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbInvert != 0) {
                if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_INSERT) {
                    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop = (int32)SQLITE_DELETE;
                } else {
                    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_DELETE) {
                        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop = (int32)SQLITE_INSERT;
                    }
                }
            }
        }
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FbPatchset == 0 && (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fop == (int32)SQLITE_UPDATE) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol)) {
                    break;
                }
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK + (uintptr)i))) == 0 && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)(i + (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol) * 8)) == (uintptr)0) {
                    _sqlite3ValueFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)i * 8)));
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)i * 8))).Value = (uintptr)0;
                }
                goto _17;
_17:;
                i = i + 1;
            }
        }
    }
    return (int32)SQLITE_ROW;
}

internal static int32 /*r*/ _sessionChangesetReadTblhdr(ж<libc.TLS> tls, uintptr p) {
    Tsize_t iPK = default;
    int32 nByte = default;
    ref var nCopy = ref heap(new int32(), out var ᏑnCopy);
    int32 nVarint = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 v2 = default;
    uintptr v1 = default;
    _ = iPK;
    _ = nByte;
    _ = nCopy;
    _ = nVarint;
    _ = rc;
    _ = v1;
    _ = v2;
    rc = _sessionChangesetBufferTblhdr(tls, p, @unsafe.Pointer.FromPinnedBox(ᏑnCopy));
    if (rc == SQLITE_OK) {
        nVarint = _sessionVarintGet(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FaData + (uintptr)(@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext, (@unsafe.Pointer)(p + 120));
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol > 0) {
            nCopy = nCopy - nVarint;
            (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext += nVarint;
            nByte = libc.Int32FromUint64(libc.Uint64FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol) * (uint64)8 * (uint64)2 + libc.Uint64FromInt32(nCopy));
            (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Ftblhdr.FnBuf = 0;
            _sessionBufferGrow(tls, (@unsafe.Pointer)(p + 72), (int64)nByte, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        } else {
            rc = _sqlite3CorruptError(tls, (int32)239267);
        }
    }
    if (rc == SQLITE_OK) {
        iPK = (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol) * (uint64)2;
        libc.Xmemset(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Ftblhdr.FaBuf, 0, iPK);
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Ftblhdr.FaBuf + (uintptr)iPK, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FaData + (uintptr)(@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext, libc.Uint64FromInt32(nCopy));
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Fin.FiNext += nCopy;
    }
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Ftblhdr.FaBuf;
    if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue == (uintptr)0) {
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK = (uintptr)0;
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FzTab = (uintptr)0;
    } else {
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FapValue + (uintptr)((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol * (int32)2) * 8;
        if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK != 0) {
            v1 = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FabPK + (uintptr)(@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FnCol;
        } else {
            v1 = (uintptr)0;
        }
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.FzTab = v1;
    }
    v2 = rc;
    (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)p)).Value.Frc = v2;
    return v2;
}

internal static void _sessionDiscardData(ж<libc.TLS> tls, uintptr pIn) {
    int32 nMove = default;
    _ = nMove;
    if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FxInput != 0 && (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiCurrent >= _sessions_strm_chunk_size) {
        nMove = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FnBuf - (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiCurrent;
        if (nMove > 0) {
            libc.Xmemmove(tls, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FaBuf, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FaBuf + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiCurrent, libc.Uint64FromInt32(nMove));
        }
        (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FnBuf -= (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiCurrent;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIn + 8))).Value -= (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiCurrent;
        (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiCurrent = 0;
        (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.Fbuf.FnBuf;
    }
}

internal static int32 /*r*/ _sessionGrowHash(ж<libc.TLS> tls, uintptr pSession, int32 bPatchset, uintptr pTab) {
    uintptr apNew = default;
    uintptr p = default;
    uintptr pNext = default;
    int32 bPkOnly = default;
    int32 i = default;
    int32 iHash = default;
    int32 v1 = default;
    Tsqlite3_int64 nNew = default;
    _ = apNew;
    _ = bPkOnly;
    _ = i;
    _ = iHash;
    _ = nNew;
    _ = p;
    _ = pNext;
    _ = v1;
    if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange == 0 || (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnEntry >= (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange / (int32)2) {
        if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange != 0) {
            v1 = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange;
        } else {
            v1 = (int32)128;
        }
        nNew = (int64)2 * (int64)v1;
        apNew = _sessionMalloc64(tls, pSession, libc.Int64FromUint64((uint64)8 * libc.Uint64FromInt64(nNew)));
        if (apNew == (uintptr)0) {
            if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange == 0) {
                return (int32)SQLITE_ERROR;
            }
            return SQLITE_OK;
        }
        libc.Xmemset(tls, apNew, 0, (uint64)((uint64)8 * libc.Uint64FromInt64(nNew)));
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange)) {
                break;
            }
            p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange + (uintptr)i * 8));
            for (;;) {
                if (!(p != 0)) {
                    break;
                }
                bPkOnly = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_DELETE && bPatchset != 0);
                iHash = libc.Int32FromUint32(_sessionChangeHash(ref (tls).DerefOrNull(), pTab, bPkOnly, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FaRecord, (int32)nNew));
                pNext = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FpNext;
                (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)p)).Value.FpNext = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)iHash * 8));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apNew + (uintptr)iHash * 8))).Value = p;
                goto _3;
_3:;
                p = pNext;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        _sessionFree(tls, pSession, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange);
        (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange = (int32)nNew;
        (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange = apNew;
    }
    return SQLITE_OK;
}

internal static void _sessionMergeRecord(ж<libc.TLS> tls, @unsafe.Pointer paOut, int32 nCol, uintptr aLeft, uintptr aRight) {
    uintptr a1 = default;
    uintptr a2 = default;
    uintptr aOut = default;
    int32 iCol = default;
    int32 n1 = default;
    int32 n2 = default;
    _ = a1;
    _ = a2;
    _ = aOut;
    _ = iCol;
    _ = n1;
    _ = n2;
    a1 = aLeft;
    a2 = aRight;
    aOut = ~@unsafe.Pointer.To<uintptr>(paOut);
    iCol = 0;
    for (;;) {
        if (!(iCol < nCol)) {
            break;
        }
        n1 = _sessionSerialLen(ref (tls).DerefOrNull(), a1);
        n2 = _sessionSerialLen(ref (tls).DerefOrNull(), a2);
        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a2)) != 0) {
            libc.Xmemcpy(tls, aOut, a2, libc.Uint64FromInt32(n2));
            aOut = aOut + (uintptr)n2;
        } else {
            libc.Xmemcpy(tls, aOut, a1, libc.Uint64FromInt32(n1));
            aOut = aOut + (uintptr)n1;
        }
        a1 = a1 + (uintptr)n1;
        a2 = a2 + (uintptr)n2;
        goto _1;
_1:;
        iCol = iCol + 1;
    }
    (@unsafe.Pointer.To<uintptr>(paOut)).Value = aOut;
}

internal static int32 /*r*/ _sessionMergeUpdate(ж<libc.TLS> tls, @unsafe.Pointer paOut, uintptr pTab, int32 bPatchset, uintptr aOldRecord1, uintptr aOldRecord2, uintptr aNewRecord1, uintptr aNewRecord2) {
    uintptr aNew = default;
    uintptr aNew1 = default;
    ref var aNew11 = ref heap(new uintptr(), out var ᏑaNew11);
    ref var aNew2 = ref heap(new uintptr(), out var ᏑaNew2);
    uintptr aOld = default;
    uintptr aOld1 = default;
    ref var aOld11 = ref heap(new uintptr(), out var ᏑaOld11);
    ref var aOld2 = ref heap(new uintptr(), out var ᏑaOld2);
    uintptr aOut = default;
    uintptr v2 = default;
    int32 bRequired = default;
    int32 i = default;
    ref var nNew = ref heap(new int32(), out var ᏑnNew);
    ref var nNew1 = ref heap(new int32(), out var ᏑnNew1);
    ref var nOld = ref heap(new int32(), out var ᏑnOld);
    ref var nOld1 = ref heap(new int32(), out var ᏑnOld1);
    _ = aNew;
    _ = aNew1;
    _ = aNew11;
    _ = aNew2;
    _ = aOld;
    _ = aOld1;
    _ = aOld11;
    _ = aOld2;
    _ = aOut;
    _ = bRequired;
    _ = i;
    _ = nNew;
    _ = nNew1;
    _ = nOld;
    _ = nOld1;
    _ = v2;
    aOld11 = aOldRecord1;
    aOld2 = aOldRecord2;
    aNew11 = aNewRecord1;
    aNew2 = aNewRecord2;
    aOut = ~@unsafe.Pointer.To<uintptr>(paOut);
    if (bPatchset == 0) {
        bRequired = 0;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            aOld = _sessionMergeValue(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑaOld11), @unsafe.Pointer.FromBox(ᏑaOld2), @unsafe.Pointer.FromPinnedBox(ᏑnOld));
            aNew = _sessionMergeValue(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑaNew11), @unsafe.Pointer.FromBox(ᏑaNew2), @unsafe.Pointer.FromPinnedBox(ᏑnNew));
            if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i)) != 0 || nOld != nNew || libc.Xmemcmp(tls, aOld, aNew, libc.Uint64FromInt32(nNew)) != 0) {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i))) == 0) {
                    bRequired = (int32)1;
                }
                libc.Xmemcpy(tls, aOut, aOld, libc.Uint64FromInt32(nOld));
                aOut = aOut + (uintptr)nOld;
            } else {
                v2 = aOut;
                aOut = aOut + 1;
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2))).Value = (uint8)'\u0000';
            }
            goto _1;
_1:;
            i = i + 1;
        }
        if (!(bRequired != 0)) {
            return 0;
        }
    }
    aOld11 = aOldRecord1;
    aOld2 = aOldRecord2;
    aNew11 = aNewRecord1;
    aNew2 = aNewRecord2;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
            break;
        }
        aOld1 = _sessionMergeValue(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑaOld11), @unsafe.Pointer.FromBox(ᏑaOld2), @unsafe.Pointer.FromPinnedBox(ᏑnOld1));
        aNew1 = _sessionMergeValue(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑaNew11), @unsafe.Pointer.FromBox(ᏑaNew2), @unsafe.Pointer.FromPinnedBox(ᏑnNew1));
        if (bPatchset == 0 && (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i)) != 0 || nOld1 == nNew1 && 0 == libc.Xmemcmp(tls, aOld1, aNew1, libc.Uint64FromInt32(nNew1)))) {
            v2 = aOut;
            aOut = aOut + 1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2))).Value = (uint8)'\u0000';
        } else {
            libc.Xmemcpy(tls, aOut, aNew1, libc.Uint64FromInt32(nNew1));
            aOut = aOut + (uintptr)nNew1;
        }
        goto _3;
_3:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<uintptr>(paOut)).Value = aOut;
    return (int32)1;
}

internal static int32 /*r*/ _sessionPreupdateEqual(ж<libc.TLS> tls, uintptr pSession, Ti64 iRowid, uintptr pTab, uintptr pChange, int32 op) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr a = default;
        uintptr z = default;
        uintptr v2 = default;
        int32 eType = default;
        int32 iCol = default;
        int32 iIdx = default;
        ref var n = ref heap(new int32(), out var Ꮡn);
        int32 rc = default;
        Ti64 _ᴛ1 = default;
        uintptr _ᴛ2 = default;
        float64 _ᴛ3 = default;
        _ = a;
        _ = eType;
        _ = iCol;
        _ = iIdx;
        _ = n;
        _ = rc;
        _ = z;
        _ = v2;
        a = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord;
        if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FbRowid != 0) {
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) != (int32)SQLITE_INTEGER) {
                r = 0; goto ᒐdone;
            }
            r = libc.BoolInt32(_sessionGetI64(ref (tls).DerefOrNull(), a + 1) == iRowid); goto ᒐdone;
        }
        iCol = 0;
        for (;;) {
            if (!(iCol < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            if (!(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)iCol)) != 0)) {
                a = a + (uintptr)_sessionSerialLen(ref (tls).DerefOrNull(), a);
            } else {
                v2 = a;
                a = a + 1;
                eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2)));
                iIdx = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FaiIdx + (uintptr)iCol * 4));
                if (op == (int32)SQLITE_INSERT) {
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FxNew)(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx, iIdx, bp);
                } else {
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FxOld)(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx, iIdx, bp);
                }
                _ = rc;
                if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))) != eType) {
                    r = 0; goto ᒐdone;
                }
                if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))).Value = _sessionGetI64(ref (tls).DerefOrNull(), a);
                    a = a + (uintptr)8;
                    if (eType == (int32)SQLITE_INTEGER) {
                        if (Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))) != ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))) {
                            r = 0; goto ᒐdone;
                        }
                    } else {
                        libc.Xmemcpy(tls, bp + 16, bp + 8, (uint64)8);
                        if (Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))) != ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 16))) {
                            r = 0; goto ᒐdone;
                        }
                    }
                } else {
                    a = a + (uintptr)_sessionVarintGet(ref (tls).DerefOrNull(), a, @unsafe.Pointer.FromPinnedBox(Ꮡn));
                    if (Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))) != n) {
                        r = 0; goto ᒐdone;
                    }
                    if (eType == (int32)SQLITE_TEXT) {
                        z = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                    } else {
                        z = Xsqlite3_value_blob(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                    }
                    if (n > 0 && libc.Xmemcmp(tls, a, z, libc.Uint64FromInt32(n)) != 0) {
                        r = 0; goto ᒐdone;
                    }
                    a = a + (uintptr)n;
                }
            }
            goto _1;
_1:;
            iCol = iCol + 1;
        }
        r = (int32)1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sessionPreupdateHash(ж<libc.TLS> tls, uintptr pSession, Ti64 iRowid, uintptr pTab, int32 bNew, @unsafe.Pointer piHash, @unsafe.Pointer pbNullPK) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 eType = default;
        int32 i = default;
        int32 iIdx = default;
        int32 n = default;
        int32 rc = default;
        uint32 h = default;
        uintptr z = default;
        Ti64 _ᴛ1 = default;
        uintptr _ᴛ2 = default;
        float64 _ᴛ3 = default;
        _ = eType;
        _ = h;
        _ = i;
        _ = iIdx;
        _ = n;
        _ = rc;
        _ = z;
        h = (uint32)0;
        if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FbRowid != 0) {
            h = _sessionHashAppendI64(ref (tls).DerefOrNull(), h, iRowid);
        } else {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                    break;
                }
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i)) != 0) {
                    iIdx = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FaiIdx + (uintptr)i * 4));
                    if (bNew != 0) {
                        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FxNew)(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx, iIdx, bp);
                    } else {
                        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FxOld)(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx, iIdx, bp);
                    }
                    if (rc != SQLITE_OK) {
                        r = rc; goto ᒐdone;
                    }
                    eType = Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                    h = _sessionHashAppendType(ref (tls).DerefOrNull(), h, eType);
                    if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                        if (eType == (int32)SQLITE_INTEGER) {
                            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))).Value = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                        } else {
                            (@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 16))).Value = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                            libc.Xmemcpy(tls, bp + 8, bp + 16, (uint64)8);
                        }
                        h = _sessionHashAppendI64(ref (tls).DerefOrNull(), h, ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8)));
                    } else {
                        if (eType == (int32)SQLITE_TEXT || eType == (int32)SQLITE_BLOB) {
                            if (eType == (int32)SQLITE_TEXT) {
                                z = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                            } else {
                                z = Xsqlite3_value_blob(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                            }
                            n = Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                            if (!(z != 0) && (eType != (int32)SQLITE_BLOB || n > 0)) {
                                r = (int32)SQLITE_NOMEM; goto ᒐdone;
                            }
                            h = _sessionHashAppendBlob(ref (tls).DerefOrNull(), h, n, z);
                        } else {
                            (@unsafe.Pointer.To<int32>(pbNullPK)).Value = (int32)1;
                        }
                    }
                }
                goto _1;
_1:;
                i = i + 1;
            }
        }
        (@unsafe.Pointer.To<int32>(piHash)).Value = libc.Int32FromUint32(h % libc.Uint32FromInt32((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange));
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sessionPutDouble(ж<libc.TLS> tls, @unsafe.Pointer aBuf, float64 _r) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = _r;
        Tu64 _ᴛ1 = default;
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        _sessionPutI64(ref (tls).DerefOrNull(), aBuf, libc.Int64FromUint64(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8))));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sessionReadRecord(ж<libc.TLS> tls, uintptr pIn, int32 nCol, uintptr abPK, uintptr apOut, @unsafe.Pointer pbEmpty) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aVal = default;
        uintptr v3 = default;
        int32 eType = default;
        int32 i = default;
        ref var nByte = ref heap(new int32(), out var ᏑnByte);
        int32 nRem = default;
        int32 rc = default;
        int32 v2 = default;
        Tu8 enc = default;
        float64 _ᴛ1 = default;
        Tsqlite3_int64 _ᴛ2 = default;
        _ = aVal;
        _ = eType;
        _ = enc;
        _ = i;
        _ = nByte;
        _ = nRem;
        _ = rc;
        _ = v2;
        _ = v3;
        rc = SQLITE_OK;
        if (pbEmpty != nil) {
            (@unsafe.Pointer.To<int32>(pbEmpty)).Value = (int32)1;
        }
        i = 0;
        for (;;) {
            if (!(i < nCol && rc == SQLITE_OK)) {
                break;
            }
            eType = 0;
            if (abPK != 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i))) == 0) {
                goto _1;
            }
            rc = _sessionInputBuffer(tls, pIn, (int32)9);
            if (rc == SQLITE_OK) {
                if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext >= (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData) {
                    rc = _sqlite3CorruptError(tls, (int32)239082);
                } else {
                    v3 = pIn + 8;
                    v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                    eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FaData + (uintptr)v2)));
                    if (eType != 0) {
                        if (pbEmpty != nil) {
                            (@unsafe.Pointer.To<int32>(pbEmpty)).Value = 0;
                        }
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOut + (uintptr)i * 8))).Value = _sqlite3ValueNew(tls, (uintptr)0);
                        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOut + (uintptr)i * 8)) != 0)) {
                            rc = (int32)SQLITE_NOMEM;
                        }
                    }
                }
            }
            if (rc == SQLITE_OK) {
                aVal = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext;
                if (eType == (int32)SQLITE_TEXT || eType == (int32)SQLITE_BLOB) {
                    nRem = (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData - (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext;
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIn + 8))).Value += _sessionVarintGetSafe(tls, aVal, nRem, @unsafe.Pointer.FromPinnedBox(ᏑnByte));
                    rc = _sessionInputBuffer(tls, pIn, nByte);
                    if (rc == SQLITE_OK) {
                        if (nByte < 0 || nByte > (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData - (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext) {
                            rc = _sqlite3CorruptError(tls, (int32)239103);
                        } else {
                            if (eType == (int32)SQLITE_TEXT) {
                                v2 = (int32)SQLITE_UTF8;
                            } else {
                                v2 = 0;
                            }
                            enc = libc.Uint8FromInt32(v2);
                            rc = _sessionValueSetStr(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOut + (uintptr)i * 8)), (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext, nByte, enc);
                            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIn + 8))).Value += nByte;
                        }
                    }
                }
                if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                    if ((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData - (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext < (int32)8) {
                        rc = _sqlite3CorruptError(tls, (int32)239113);
                    } else {
                        (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp))).Value = _sessionGetI64(ref (tls).DerefOrNull(), aVal);
                        if (eType == (int32)SQLITE_INTEGER) {
                            _sqlite3VdbeMemSetInt64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOut + (uintptr)i * 8)), ~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp)));
                        } else {
                            libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
                            _sqlite3VdbeMemSetDouble(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apOut + (uintptr)i * 8)), ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 8)));
                        }
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pIn + 8))).Value += (int32)8;
                    }
                }
            }
            goto _1;
_1:;
            i = i + 1;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sessionRebase(ж<libc.TLS> tls, uintptr p, uintptr pIter, uintptr __ccgo_fp_xOutput, uintptr pOut, uintptr pnOut, uintptr ppOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var aRec = ref heap(new uintptr(), out var ᏑaRec);
        uintptr pChange = default;
        ref var pCsr = ref heap(new uintptr(), out var ᏑpCsr);
        uintptr pTab = default;
        uintptr zTab = default;
        int32 bDone = default;
        ref var bNew = ref heap(new int32(), out var ᏑbNew);
        int32 iHash = default;
        ref var nRec = ref heap(new int32(), out var ᏑnRec);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        int32 v2 = default;
        TSessionBuffer _ᴛ1 = default;
        _ = aRec;
        _ = bDone;
        _ = bNew;
        _ = iHash;
        _ = nRec;
        _ = pChange;
        _ = pCsr;
        _ = pTab;
        _ = rc;
        _ = zTab;
        _ = v2;
        rc = SQLITE_OK;
        aRec = (uintptr)0;
        nRec = 0;
        bNew = 0;
        pTab = (uintptr)0;
        (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value = new TSessionBuffer();
        while ((int32)SQLITE_ROW == _sessionChangesetNext(tls, pIter, @unsafe.Pointer.FromBox(ᏑaRec), @unsafe.Pointer.FromPinnedBox(ᏑnRec), @unsafe.Pointer.FromPinnedBox(ᏑbNew))) {
            pChange = (uintptr)0;
            bDone = 0;
            if (bNew != 0) {
                zTab = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FzTab;
                pTab = (@unsafe.Pointer.To<Tsqlite3_rebaser>((@unsafe.Pointer)p)).Value.Fgrp.FpList;
                for (;;) {
                    if (!(pTab != 0)) {
                        break;
                    }
                    if (0 == Xsqlite3_stricmp(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zTab)) {
                        break;
                    }
                    goto _1;
_1:;
                    pTab = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FpNext;
                }
                bNew = 0;
                if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbPatchset != 0) {
                    rc = (int32)SQLITE_ERROR;
                }
                if ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbPatchset != 0) {
                    v2 = (int32)'P';
                } else {
                    v2 = (int32)'T';
                }
                _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32(v2), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendVarint(tls, __ccgo_ptr(bp), (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendBlob(tls, bp, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FabPK, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendBlob(tls, bp, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FzTab, libc.Int32FromUint64(libc.Xstrlen(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FzTab)) + (int32)1, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            }
            if (pTab != 0 && rc == SQLITE_OK) {
                iHash = libc.Int32FromUint32(_sessionChangeHash(ref (tls).DerefOrNull(), pTab, 0, aRec, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnChange));
                pChange = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FapChange + (uintptr)iHash * 8));
                for (;;) {
                    if (!(pChange != 0)) {
                        break;
                    }
                    if (_sessionChangeEqual(tls, pTab, 0, aRec, 0, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord) != 0) {
                        break;
                    }
                    goto _3;
_3:;
                    pChange = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FpNext;
                }
            }
            if (pChange != 0) {
                switch ((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.Fop) {
                case var ᴛ when ᴛ == (int32)SQLITE_INSERT: {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.Fop) == (int32)SQLITE_INSERT) {
                        bDone = (int32)1;
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FbIndirect) == 0) {
                            _sessionAppendByte(tls, __ccgo_ptr(bp), (uint8)SQLITE_UPDATE, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                            _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbIndirect), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                            _sessionAppendBlob(tls, bp, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FnRecord, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                            _sessionAppendBlob(tls, bp, aRec, nRec, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                        }
                    }
                    break;
                }
                case var ᴛ when ᴛ == (int32)SQLITE_UPDATE: {
                    bDone = (int32)1;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.Fop) == (int32)SQLITE_DELETE) {
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FbIndirect) == 0) {
                            pCsr = aRec;
                            _sessionSkipRecord(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(ᏑpCsr), (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol);
                            _sessionAppendByte(tls, __ccgo_ptr(bp), (uint8)SQLITE_INSERT, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                            _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbIndirect), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                            _sessionAppendRecordMerge(tls, bp, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol, pCsr, (int32)((int64)nRec - ((int64)pCsr - (int64)aRec)), (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FnRecord, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                        }
                    } else {
                        _sessionAppendPartialUpdate(tls, bp, pIter, aRec, nRec, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FnRecord, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    }
                    break;
                }
                default: {
                    bDone = (int32)1;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.Fop) == (int32)SQLITE_INSERT) {
                        _sessionAppendByte(tls, __ccgo_ptr(bp), (uint8)SQLITE_DELETE, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                        _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbIndirect), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                        _sessionAppendRecordMerge(tls, bp, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FnCol, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FnRecord, aRec, nRec, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                    }
                    break;
                    break;
                }}

            }
            if (bDone == 0) {
                _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.Fop), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendByte(tls, __ccgo_ptr(bp), libc.Uint8FromInt32((@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pIter)).Value.FbIndirect), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                _sessionAppendBlob(tls, bp, aRec, nRec, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            }
            if (rc == SQLITE_OK && __ccgo_fp_xOutput != 0 && (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf > _sessions_strm_chunk_size) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xOutput)(tls, pOut, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf);
                (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf = 0;
            }
            if (rc != 0) {
                break;
            }
        }
        if (rc != SQLITE_OK) {
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf);
            libc.Xmemset(tls, bp, 0, (uint64)16);
        }
        if (rc == SQLITE_OK) {
            if (__ccgo_fp_xOutput != 0) {
                if ((@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf > 0) {
                    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>(__ccgo_fp_xOutput)(tls, pOut, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf);
                }
            } else {
                if (ppOut != 0) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf;
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnOut))).Value = (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FnBuf;
                    (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf = (uintptr)0;
                }
            }
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TSessionBuffer>(__ccgo_ptr(bp))).Value.FaBuf);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sessionRetryIterInit(ж<libc.TLS> tls, @unsafe.Pointer pRetry, int32 bPatchset, uintptr zTab, uintptr pApply, @unsafe.Pointer ppIter) {
    Tsize_t nByte = default;
    ref var pRet = ref heap(new uintptr(), out var ᏑpRet);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = nByte;
    _ = pRet;
    _ = rc;
    pRet = (uintptr)0;
    rc = SQLITE_OK;
    rc = _sessionChangesetStart(tls, @unsafe.Pointer.FromBox(ᏑpRet), (uintptr)0, (uintptr)0, ~@unsafe.Pointer.To<int32>(@unsafe.Add(pRetry, 8)), ~@unsafe.Pointer.To<uintptr>(pRetry), (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FbInvertConstraints, (int32)1);
    if (rc == SQLITE_OK) {
        nByte = libc.Uint64FromInt32((int32)2 * (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FnCol) * (uint64)8;
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FbPatchset = bPatchset;
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FzTab = zTab;
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FnCol = (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FnCol;
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FabPK = (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FabPK;
        _sessionBufferGrow(tls, (@unsafe.Pointer)(pRet + 72), libc.Int64FromUint64(nByte), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FapValue = (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.Ftblhdr.FaBuf;
        if (rc == SQLITE_OK) {
            libc.Xmemset(tls, (@unsafe.Pointer.To<Tsqlite3_changeset_iter>((@unsafe.Pointer)pRet)).Value.FapValue, 0, nByte);
        } else {
            Xsqlite3changeset_finalize(tls, pRet);
            pRet = (uintptr)0;
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppIter)).Value = pRet;
    return rc;
}

internal static int32 /*r*/ _sessionSelectBind(ж<libc.TLS> tls, uintptr pSelect, int32 nCol, uintptr abPK, uintptr pChange) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr a = default;
        uintptr v2 = default;
        int32 eType = default;
        int32 i = default;
        ref var n = ref heap(new int32(), out var Ꮡn);
        ref var n1 = ref heap(new int32(), out var Ꮡn1);
        int32 rc = default;
        Ti64 iVal = default;
        Ti64 _ᴛ1 = default;
        float64 _ᴛ2 = default;
        _ = a;
        _ = eType;
        _ = i;
        _ = iVal;
        _ = n;
        _ = n1;
        _ = rc;
        _ = v2;
        rc = SQLITE_OK;
        a = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pChange)).Value.FaRecord;
        i = 0;
        for (;;) {
            if (!(i < nCol && rc == SQLITE_OK)) {
                break;
            }
            v2 = a;
            a = a + 1;
            eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2)));
            var matchᴛ1 = false;
            if (eType is 0) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && eType == (int32)SQLITE_NULL) { matchᴛ1 = true;
            }
            else if (eType == (int32)SQLITE_INTEGER) { matchᴛ1 = true;
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i)) != 0) {
                    iVal = _sessionGetI64(ref (tls).DerefOrNull(), a);
                    rc = Xsqlite3_bind_int64(tls, pSelect, i + (int32)1, iVal);
                }
                a = a + (uintptr)8;
            }
            else if (eType == (int32)SQLITE_FLOAT) { matchᴛ1 = true;
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i)) != 0) {
                    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))).Value = _sessionGetI64(ref (tls).DerefOrNull(), a);
                    libc.Xmemcpy(tls, bp, bp + 8, (uint64)8);
                    rc = Xsqlite3_bind_double(tls, pSelect, i + (int32)1, ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp)));
                }
                a = a + (uintptr)8;
            }
            else if (eType == (int32)SQLITE_TEXT) {
                a = a + (uintptr)_sessionVarintGet(ref (tls).DerefOrNull(), a, @unsafe.Pointer.FromPinnedBox(Ꮡn));
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i)) != 0) {
                    rc = Xsqlite3_bind_text(tls, pSelect, i + (int32)1, a, n, (uintptr)(-libc.Int32FromInt32(1)));
                }
                a = a + (uintptr)n;
            }
            else if (!matchᴛ1) { /* default: */
                do {
                    a = a + (uintptr)_sessionVarintGet(ref (tls).DerefOrNull(), a, @unsafe.Pointer.FromPinnedBox(Ꮡn1));
                    if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(abPK + (uintptr)i)) != 0) {
                        rc = Xsqlite3_bind_blob(tls, pSelect, i + (int32)1, a, n1, (uintptr)(-libc.Int32FromInt32(1)));
                    }
                    a = a + (uintptr)n1;
                    break;
                } while (false);
            }

            goto _1;
_1:;
            i = i + 1;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r1*/ _sessionSerializeValue(ж<libc.TLS> tls, uintptr aBuf, uintptr pValue, @unsafe.Pointer pnWrite) {
    int32 eType = default;
    int32 n = default;
    int32 nByte = default;
    int32 nVarint = default;
    Tu64 i = default;
    float64 r = default;
    uintptr z = default;
    _ = eType;
    _ = i;
    _ = n;
    _ = nByte;
    _ = nVarint;
    _ = r;
    _ = z;
    if (pValue != 0) {
        eType = Xsqlite3_value_type(tls, pValue);
        if (aBuf != 0) {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf))).Value = libc.Uint8FromInt32(eType);
        }
        var matchᴛ1 = false;
        if (eType == (int32)SQLITE_NULL) { matchᴛ1 = true;
            nByte = (int32)1;
        }
        else if (eType == (int32)SQLITE_INTEGER) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && eType == (int32)SQLITE_FLOAT) {
            if (aBuf != 0) {
                if (eType == (int32)SQLITE_INTEGER) {
                    i = libc.Uint64FromInt64(Xsqlite3_value_int64(tls, pValue));
                    _sessionPutI64(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aBuf + 1), libc.Int64FromUint64(i));
                } else {
                    r = Xsqlite3_value_double(tls, pValue);
                    _sessionPutDouble(tls, (@unsafe.Pointer)(aBuf + 1), r);
                }
            }
            nByte = (int32)9;
        }
        else if (!matchᴛ1) { /* default: */
            do {
                if (eType == (int32)SQLITE_TEXT) {
                    z = Xsqlite3_value_text(tls, pValue);
                } else {
                    z = Xsqlite3_value_blob(tls, pValue);
                }
                n = Xsqlite3_value_bytes(tls, pValue);
                if (z == (uintptr)0 && (eType != (int32)SQLITE_BLOB || n > 0)) {
                    return (int32)SQLITE_NOMEM;
                }
                nVarint = _sessionVarintLen(ref (tls).DerefOrNull(), n);
                if (aBuf != 0) {
                    _sessionVarintPut(tls, (@unsafe.Pointer)(aBuf + 1), n);
                    if (n > 0) {
                        libc.Xmemcpy(tls, aBuf + (uintptr)(nVarint + (int32)1), z, libc.Uint64FromInt32(n));
                    }
                }
                nByte = (int32)1 + nVarint + n;
                break;
            } while (false);
        }

    } else {
        nByte = (int32)1;
        if (aBuf != 0) {
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf))).Value = (uint8)'\u0000';
        }
    }
    if (pnWrite != nil) {
        (@unsafe.Pointer.To<Tsqlite3_int64>(pnWrite)).Value += (int64)nByte;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _sessionUpdateMaxSize(ж<libc.TLS> tls, int32 op, uintptr pSession, uintptr pTab, uintptr pC) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 bChanged = default;
        int32 eType = default;
        int32 iIdx = default;
        int32 ii = default;
        int32 ii1 = default;
        ref var nByte = ref heap(new int32(), out var ᏑnByte);
        int32 nIncr = default;
        int32 nOld = default;
        ref var nNew = ref heap(new Ti64(), out var ᏑnNew);
        uintptr pCsr = default;
        uintptr v3 = default;
        float64 _ᴛ1 = default;
        Tsqlite3_int64 _ᴛ2 = default;
        uintptr _ᴛ3 = default;
        uintptr _ᴛ4 = default;
        _ = bChanged;
        _ = eType;
        _ = iIdx;
        _ = ii;
        _ = ii1;
        _ = nByte;
        _ = nIncr;
        _ = nNew;
        _ = nOld;
        _ = pCsr;
        _ = v3;
        nNew = (int64)2;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.Fop) == (int32)SQLITE_INSERT) {
            if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FbRowid != 0) {
                nNew = nNew + (int64)9;
            }
            if (op != (int32)SQLITE_DELETE) {
                ii = 0;
                for (;;) {
                    if (!(ii < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                        break;
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
                    GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FxNew)(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx, ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FaiIdx + (uintptr)ii * 4)), bp);
                    _sessionSerializeValue(tls, (uintptr)0, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), @unsafe.Pointer.FromPinnedBox(ᏑnNew));
                    goto _1;
_1:;
                    ii = ii + 1;
                }
            }
        } else {
            if (op == (int32)SQLITE_DELETE) {
                nNew = nNew + (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FnRecord;
                if (Xsqlite3_preupdate_blobwrite(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fdb) >= 0) {
                    nNew = nNew + (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FnRecord;
                }
            } else {
                pCsr = (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FaRecord;
                if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FbRowid != 0) {
                    nNew = nNew + (int64)(libc.Int32FromInt32(9) + libc.Int32FromInt32(1));
                    pCsr = pCsr + (uintptr)9;
                }
                ii1 = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FbRowid;
                for (;;) {
                    if (!(ii1 < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                        break;
                    }
                    bChanged = (int32)1;
                    nOld = 0;
                    iIdx = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FaiIdx + (uintptr)ii1 * 4));
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))).Value = (uintptr)0;
                    GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FxNew)(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx, iIdx, bp + 8);
                    if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)) == (uintptr)0) {
                        r = (int32)SQLITE_NOMEM; goto ᒐdone;
                    }
                    v3 = pCsr;
                    pCsr = pCsr + 1;
                    eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3)));
                    var matchᴛ1 = false;
                    if (eType == (int32)SQLITE_NULL) { matchᴛ1 = true;
                        bChanged = libc.BoolInt32(Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))) != (int32)SQLITE_NULL);
                    }
                    else if (eType == (int32)SQLITE_FLOAT) { matchᴛ1 = true;
                        fallthrough = true;
                    }
                    if (fallthrough || !matchᴛ1 && eType == (int32)SQLITE_INTEGER) {
                        if (eType == Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)))) {
                            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16))).Value = _sessionGetI64(ref (tls).DerefOrNull(), pCsr);
                            if (eType == (int32)SQLITE_INTEGER) {
                                bChanged = libc.BoolInt32(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 16)) != Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))));
                            } else {
                                libc.Xmemcpy(tls, bp + 24, bp + 16, (uint64)8);
                                bChanged = libc.BoolInt32(~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 24)) != Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))));
                            }
                        }
                        nOld = (int32)8;
                        pCsr = pCsr + (uintptr)8;
                    }
                    else if (!matchᴛ1) { /* default: */
                        do {
                            nOld = _sessionVarintGet(ref (tls).DerefOrNull(), pCsr, @unsafe.Pointer.FromPinnedBox(ᏑnByte));
                            pCsr = pCsr + (uintptr)nOld;
                            nOld = nOld + nByte;
                            if (eType == Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))) && nByte == Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))) && (nByte == 0 || 0 == libc.Xmemcmp(tls, pCsr, Xsqlite3_value_blob(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))), libc.Uint64FromInt32(nByte)))) {
                                bChanged = 0;
                            }
                            pCsr = pCsr + (uintptr)nByte;
                            break;
                        } while (false);
                    }

                    if (bChanged != 0 && ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)ii1)) != 0) {
                        nNew = (int64)((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FnRecord + (int32)2);
                        break;
                    }
                    if (bChanged != 0) {
                        nNew = nNew + (int64)((int32)1 + nOld);
                        _sessionSerializeValue(tls, (uintptr)0, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)), @unsafe.Pointer.FromPinnedBox(ᏑnNew));
                    } else {
                        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)ii1)) != 0) {
                            nNew = nNew + (int64)((int32)2 + nOld);
                        } else {
                            nNew = nNew + (int64)2;
                        }
                    }
                    goto _2;
_2:;
                    ii1 = ii1 + 1;
                }
            }
        }
        if (nNew > (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FnMaxSize) {
            nIncr = (int32)(nNew - (int64)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FnMaxSize);
            (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pC)).Value.FnMaxSize = (int32)nNew;
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pSession + 64))).Value += (int64)nIncr;
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sessionUpdateOneChange(ж<libc.TLS> tls, uintptr pSession, @unsafe.Pointer pRc, uintptr pp, int32 nCol, uintptr pDflt) {
    int32 eType = default;
    int32 iField = default;
    int32 n = default;
    int32 n1 = default;
    int32 n2 = default;
    int32 nByte = default;
    int32 nIncr = default;
    int32 v1 = default;
    Ti64 iVal = default;
    uintptr pNew = default;
    uintptr pOld = default;
    uintptr z = default;
    uintptr z1 = default;
    uintptr v2 = default;
    float64 rVal = default;
    _ = eType;
    _ = iField;
    _ = iVal;
    _ = n;
    _ = n1;
    _ = n2;
    _ = nByte;
    _ = nIncr;
    _ = pNew;
    _ = pOld;
    _ = rVal;
    _ = z;
    _ = z1;
    _ = v1;
    _ = v2;
    pOld = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp));
    while (libc.Int32FromUint16((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pOld)).Value.FnRecordField) < nCol) {
        pNew = (uintptr)0;
        nByte = 0;
        nIncr = 0;
        iField = libc.Int32FromUint16((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pOld)).Value.FnRecordField);
        eType = Xsqlite3_column_type(tls, pDflt, iField);
        var matchᴛ1 = false;
        if (eType == (int32)SQLITE_NULL) { matchᴛ1 = true;
            nIncr = (int32)1;
        }
        else if (eType == (int32)SQLITE_INTEGER) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && eType == (int32)SQLITE_FLOAT) {
            nIncr = (int32)9;
        }
        else if (!matchᴛ1) { /* default: */
            do {
                n = Xsqlite3_column_bytes(tls, pDflt, iField);
                nIncr = (int32)1 + _sessionVarintLen(ref (tls).DerefOrNull(), n) + n;
                break;
            } while (false);
        }

        nByte = libc.Int32FromUint64(libc.Uint64FromInt32(nIncr) + ((uint64)32 + libc.Uint64FromInt32((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pOld)).Value.FnRecord)));
        pNew = _sessionMalloc64(tls, pSession, (int64)nByte);
        if (pNew == (uintptr)0) {
            (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
            return;
        } else {
            libc.Xmemcpy(tls, pNew, pOld, (uint64)32);
            (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord = pNew + 1 * 32;
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pOld)).Value.FaRecord, libc.Uint64FromInt32((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pOld)).Value.FnRecord));
            v2 = pNew + 8;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)v1))).Value = libc.Uint8FromInt32(eType);
            switch (eType) {
            case var ᴛ when ᴛ == (int32)SQLITE_INTEGER: {
                iVal = Xsqlite3_column_int64(tls, pDflt, iField);
                _sessionPutI64(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord), iVal);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 8))).Value += (int32)8;
                break;
            }
            case var ᴛ when ᴛ == (int32)SQLITE_FLOAT: {
                rVal = Xsqlite3_column_double(tls, pDflt, iField);
                _sessionPutDouble(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord), rVal);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 8))).Value += (int32)8;
                break;
            }
            case var ᴛ when ᴛ == (int32)SQLITE_TEXT: {
                n1 = Xsqlite3_column_bytes(tls, pDflt, iField);
                z = Xsqlite3_column_text(tls, pDflt, iField);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 8))).Value += _sessionVarintPut(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord), n1);
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord, z, libc.Uint64FromInt32(n1));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 8))).Value += n1;
                break;
            }
            case var ᴛ when ᴛ == (int32)SQLITE_BLOB: {
                n2 = Xsqlite3_column_bytes(tls, pDflt, iField);
                z1 = Xsqlite3_column_blob(tls, pDflt, iField);
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 8))).Value += _sessionVarintPut(tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord), n2);
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FaRecord + (uintptr)(@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecord, z1, libc.Uint64FromInt32(n2));
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 8))).Value += n2;
                break;
            }
            default: {
                break;
                break;
            }}

            _sessionFree(tls, pSession, pOld);
            v2 = pNew;
            pOld = v2;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = v2;
            (@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecordField = (Tu16)((@unsafe.Pointer.To<TSessionChange>((@unsafe.Pointer)pNew)).Value.FnRecordField + 1);
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pNew + 4))).Value += nIncr;
            if (pSession != 0) {
                (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pSession + 64))).Value += (int64)nIncr;
            }
        }
    }
}

internal static int32 /*r*/ _sessionValueSetStr(ж<libc.TLS> tls, uintptr pVal, uintptr aData, int32 nData, Tu8 enc) {
    uintptr aCopy = default;
    _ = aCopy;
    aCopy = Xsqlite3_malloc64(tls, libc.Uint64FromInt64((int64)nData + (int64)1));
    if (aCopy == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemcpy(tls, aCopy, aData, libc.Uint64FromInt32(nData));
    _sqlite3ValueSetStr(tls, pVal, nData, aCopy, enc, __ccgo_fp(Xsqlite3_free));
    return SQLITE_OK;
}

internal static int32 /*r*/ _sessionVarintGetSafe(ж<libc.TLS> tls, uintptr aBuf, int32 nBuf, @unsafe.Pointer piVal) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aRead = default;
        array<Tu8> _ᴛ1 = new(9);
        _ = aRead;
        aRead = aBuf;
        libc.Xmemset(tls, bp, 0, (uint64)9);
        if (libc.Uint64FromInt32(nBuf) < (uint64)9) {
            libc.Xmemcpy(tls, bp, aBuf, libc.Uint64FromInt32(nBuf));
            aRead = bp;
        }
        r = _sessionVarintGet(ref (tls).DerefOrNull(), aRead, piVal);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _splitNodeStartree(ж<libc.TLS> tls, uintptr pRtree, uintptr aCell, int32 nCell, uintptr pLeft, uintptr pRight, uintptr pBboxLeft, uintptr pBboxRight) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        uintptr aSpare = default;
        uintptr aaSorted = default;
        uintptr pBbox = default;
        uintptr pCell = default;
        uintptr pTarget = default;
        uintptr v7 = default;
        uintptr v8 = default;
        TRtreeDValue area = default;
        TRtreeDValue fBestArea = default;
        TRtreeDValue fBestMargin = default;
        TRtreeDValue fBestOverlap = default;
        TRtreeDValue margin = default;
        TRtreeDValue overlap = default;
        int32 iBestDim = default;
        int32 iBestLeft = default;
        int32 iBestSplit = default;
        int32 ii = default;
        int32 jj = default;
        int32 kk = default;
        int32 nLeft = default;
        Tsqlite3_int64 nByte = default;
        TRtreeCell _ᴛ1 = new();
        TRtreeCell _ᴛ2 = new();
        _ = aSpare;
        _ = aaSorted;
        _ = area;
        _ = fBestArea;
        _ = fBestMargin;
        _ = fBestOverlap;
        _ = iBestDim;
        _ = iBestLeft;
        _ = iBestSplit;
        _ = ii;
        _ = jj;
        _ = kk;
        _ = margin;
        _ = nByte;
        _ = nLeft;
        _ = overlap;
        _ = pBbox;
        _ = pCell;
        _ = pTarget;
        _ = v7;
        _ = v8;
        iBestDim = 0;
        iBestSplit = 0;
        fBestMargin = (float64)0D;
        nByte = libc.Int64FromUint64(libc.Uint64FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim) + libc.Int32FromInt32(1)) * ((uint64)8 + libc.Uint64FromInt32(nCell) * (uint64)4));
        aaSorted = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (!(aaSorted != 0)) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        aSpare = aaSorted + (uintptr)(@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim * 8 + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim) * nCell) * 4;
        libc.Xmemset(tls, aaSorted, 0, libc.Uint64FromInt64(nByte));
        ii = 0;
        for (;;) {
            if (!(ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim))) {
                break;
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8))).Value = aaSorted + (uintptr)(@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim * 8 + (uintptr)(ii * nCell) * 4;
            jj = 0;
            for (;;) {
                if (!(jj < nCell)) {
                    break;
                }
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8)) + (uintptr)jj * 4))).Value = jj;
                goto _2;
_2:;
                jj = jj + 1;
            }
            _SortByDimension(tls, pRtree, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8)), nCell, ii, aCell, aSpare);
            goto _1;
_1:;
            ii = ii + 1;
        }
        ii = 0;
        for (;;) {
            if (!(ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim))) {
                break;
            }
            margin = (float64)0D;
            fBestOverlap = (float64)0D;
            fBestArea = (float64)0D;
            iBestLeft = 0;
            nLeft = quo(((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize - (int32)4), libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell)) / (int32)3;
            for (;;) {
                if (!(nLeft <= nCell - quo(((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize - (int32)4), libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell)) / (int32)3)) {
                    break;
                }
                libc.Xmemcpy(tls, bp, aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8))))) * 48, (uint64)48);
                libc.Xmemcpy(tls, bp + 48, aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8)) + (uintptr)(nCell - (int32)1) * 4))) * 48, (uint64)48);
                kk = (int32)1;
                for (;;) {
                    if (!(kk < nCell - (int32)1)) {
                        break;
                    }
                    if (kk < nLeft) {
                        _cellUnion(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp), (@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8)) + (uintptr)kk * 4))) * 48));
                    } else {
                        _cellUnion(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp + 48), (@unsafe.Pointer)(aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)ii * 8)) + (uintptr)kk * 4))) * 48));
                    }
                    goto _5;
_5:;
                    kk = kk + 1;
                }
                margin = margin + _cellMargin(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp));
                margin = margin + _cellMargin(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp + 48));
                overlap = _cellOverlap(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp), __ccgo_ptr(bp + 48), (int32)1);
                area = _cellArea(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp)) + _cellArea(ref (tls).DerefOrNull(), pRtree, __ccgo_ptr(bp + 48));
                if (nLeft == quo(((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize - (int32)4), libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell)) / (int32)3 || overlap < fBestOverlap || overlap == fBestOverlap && area < fBestArea) {
                    iBestLeft = nLeft;
                    fBestOverlap = overlap;
                    fBestArea = area;
                }
                goto _4;
_4:;
                nLeft = nLeft + 1;
            }
            if (ii == 0 || margin < fBestMargin) {
                iBestDim = ii;
                fBestMargin = margin;
                iBestSplit = iBestLeft;
            }
            goto _3;
_3:;
            ii = ii + 1;
        }
        libc.Xmemcpy(tls, pBboxLeft, aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)iBestDim * 8))))) * 48, (uint64)48);
        libc.Xmemcpy(tls, pBboxRight, aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)iBestDim * 8)) + (uintptr)iBestSplit * 4))) * 48, (uint64)48);
        ii = 0;
        for (;;) {
            if (!(ii < nCell)) {
                break;
            }
            if (ii < iBestSplit) {
                v7 = pLeft;
            } else {
                v7 = pRight;
            }
            pTarget = v7;
            if (ii < iBestSplit) {
                v8 = pBboxLeft;
            } else {
                v8 = pBboxRight;
            }
            pBbox = v8;
            pCell = aCell + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aaSorted + (uintptr)iBestDim * 8)) + (uintptr)ii * 4))) * 48;
            _nodeInsertCell(ref (tls).DerefOrNull(), pRtree, pTarget, pCell);
            _cellUnion(ref (tls).DerefOrNull(), pRtree, (@unsafe.Pointer)pBbox, (@unsafe.Pointer)pCell);
            goto _6;
_6:;
            ii = ii + 1;
        }
        Xsqlite3_free(tls, aaSorted);
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _sqlite3ArrayAllocate(ж<libc.TLS> tls, uintptr db, uintptr pArray, int32 szEntry, uintptr pnEntry, @unsafe.Pointer pIdx) {
    Tsqlite3_int64 n = default;
    Tsqlite3_int64 sz = default;
    uintptr pNew = default;
    uintptr z = default;
    int32 v1 = default;
    int64 v2 = default;
    _ = n;
    _ = pNew;
    _ = sz;
    _ = z;
    _ = v1;
    _ = v2;
    v1 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(pnEntry));
    (@unsafe.Pointer.To<int32>(pIdx)).Value = v1;
    n = (int64)v1;
    if ((Tsqlite3_int64)(n & (n - (int64)1)) == 0) {
        if (n == 0) {
            v2 = (int64)1;
        } else {
            v2 = (int64)2 * n;
        }
        sz = v2;
        pNew = _sqlite3DbRealloc(tls, db, pArray, libc.Uint64FromInt64(sz * (int64)szEntry));
        if (pNew == (uintptr)0) {
            (@unsafe.Pointer.To<int32>(pIdx)).Value = -(int32)1;
            return pArray;
        }
        pArray = pNew;
    }
    z = pArray;
    libc.Xmemset(tls, z + (uintptr)(n * (int64)szEntry), 0, libc.Uint64FromInt32(szEntry));
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnEntry))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(pnEntry)) + 1;
    return pArray;
}

internal static void _sqlite3BitvecClear(ж<libc.TLS> tls, uintptr p, Tu32 i, uintptr pBuf) {
    uintptr aiValues = default;
    uintptr v1 = default;
    Tu32 bin = default;
    Tu32 h = default;
    uint32 j = default;
    _ = aiValues;
    _ = bin;
    _ = h;
    _ = j;
    _ = v1;
    if (p == (uintptr)0) {
        return;
    }
    i = i - 1;
    while ((@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor != 0) {
        bin = i / (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor;
        i = i % (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor;
        p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)bin * 8));
        if (!(p != 0)) {
            return;
        }
    }
    if ((uint64)(@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiSize <= (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(1) * libc.Uint64FromInt32(BITVEC_SZELEM)) {
        v1 = p + 16 + (uintptr)(i / (uint32)BITVEC_SZELEM);
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~libc.Int32FromUint8(libc.Uint8FromInt32(libc.Int32FromInt32(1).Lsh((uint64)((Tu32)(i & libc.Uint32FromInt32(libc.Int32FromInt32(BITVEC_SZELEM) - libc.Int32FromInt32(1))))))));
    } else {
        aiValues = pBuf;
        libc.Xmemcpy(tls, aiValues, p + 16, (uint64)496);
        libc.Xmemset(tls, p + 16, 0, (uint64)496);
        (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet = (uint32)0;
        j = (uint32)0;
        for (;;) {
            if (!((uint64)j < (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4))) {
                break;
            }
            if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aiValues + (uintptr)j * 4)) != 0 && ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aiValues + (uintptr)j * 4)) != i + (uint32)1) {
                h = (uint32)((uint64)((~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aiValues + (uintptr)j * 4)) - libc.Uint32FromInt32(1)) * libc.Uint32FromInt32(1)) % ((libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4)));
                (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet = (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet + 1;
                while (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 16 + (uintptr)h * 4)) != 0) {
                    h = h + 1;
                    if ((uint64)h >= (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4)) {
                        h = (uint32)0;
                    }
                }
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 16 + (uintptr)h * 4))).Value = (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aiValues + (uintptr)j * 4))).Value;
            }
            goto _2;
_2:;
            j = j + 1;
        }
    }
}

internal static int32 /*r*/ _sqlite3BitvecSet(ж<libc.TLS> tls, uintptr p, Tu32 i) {
    uintptr aiValues = default;
    uintptr v1 = default;
    Tu32 bin = default;
    Tu32 h = default;
    Tu32 v2 = default;
    uint32 j = default;
    int32 rc = default;
    _ = aiValues;
    _ = bin;
    _ = h;
    _ = j;
    _ = rc;
    _ = v1;
    _ = v2;
    if (p == (uintptr)0) {
        return SQLITE_OK;
    }
    i = i - 1;
    while ((uint64)(@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiSize > (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(1) * libc.Uint64FromInt32(BITVEC_SZELEM) && (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor != 0) {
        bin = i / (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor;
        i = i % (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor;
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)bin * 8)) == (uintptr)0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)bin * 8))).Value = _sqlite3BitvecCreate(tls, (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor);
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)bin * 8)) == (uintptr)0) {
                return (int32)SQLITE_NOMEM;
            }
        }
        p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(p + 16 + (uintptr)bin * 8));
    }
    if ((uint64)(@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiSize <= (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(1) * libc.Uint64FromInt32(BITVEC_SZELEM)) {
        v1 = p + 16 + (uintptr)(i / (uint32)BITVEC_SZELEM);
        (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(1).Lsh((uint64)((Tu32)(i & libc.Uint32FromInt32(libc.Int32FromInt32(BITVEC_SZELEM) - libc.Int32FromInt32(1))))));
        return SQLITE_OK;
    }
    v2 = i;
    i = i + 1;
    h = (uint32)((uint64)(v2 * libc.Uint32FromInt32(1)) % ((libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4)));
    if (!(~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 16 + (uintptr)h * 4)) != 0)) {
        if ((uint64)(@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet < (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4) - libc.Uint64FromInt32(1)) {
            goto bitvec_set_end;
        } else {
            goto bitvec_set_rehash;
        }
    }
    for (var cond = true; cond; cond = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 16 + (uintptr)h * 4)) != 0) {
        if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 16 + (uintptr)h * 4)) == i) {
            return SQLITE_OK;
        }
        h = h + 1;
        if ((uint64)h >= (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4)) {
            h = (uint32)0;
        }
    }
    goto bitvec_set_rehash;
bitvec_set_rehash:;
    if ((uint64)(@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet >= (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4) / libc.Uint64FromInt32(2)) {
        aiValues = _sqlite3DbMallocRaw(tls, (uintptr)0, (uint64)496);
        if (aiValues == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        } else {
            libc.Xmemcpy(tls, aiValues, p + 16, (uint64)496);
            libc.Xmemset(tls, p + 16, 0, (uint64)496);
            (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor = (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiSize / (uint32)((libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(8));
            if ((@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiSize % (uint32)((libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(8)) != (uint32)0) {
                (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor = (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor + 1;
            }
            if ((uint64)(@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor < (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(1) * libc.Uint64FromInt32(BITVEC_SZELEM)) {
                (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FiDivisor = (uint32)((libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(1) * libc.Uint64FromInt32(BITVEC_SZELEM));
            }
            rc = _sqlite3BitvecSet(tls, p, i);
            j = (uint32)0;
            for (;;) {
                if (!((uint64)j < (libc.Uint64FromInt32(BITVEC_SZ) - libc.Uint64FromInt32(3) * libc.Uint64FromInt64(4)) / libc.Uint64FromInt64(8) * libc.Uint64FromInt64(8) / libc.Uint64FromInt64(4))) {
                    break;
                }
                if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aiValues + (uintptr)j * 4)) != 0) {
                    rc = rc | _sqlite3BitvecSet(tls, p, ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aiValues + (uintptr)j * 4)));
                }
                goto _3;
_3:;
                j = j + 1;
            }
            _sqlite3DbFree__ccgo_gp(tls, nil, aiValues);
            return rc;
        }
    }
    goto bitvec_set_end;
bitvec_set_end:;
    (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet = (@unsafe.Pointer.To<TBitvec>((@unsafe.Pointer)p)).Value.FnSet + 1;
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 16 + (uintptr)h * 4))).Value = i;
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3BtreeCopyFile(ж<libc.TLS> tls, uintptr pTo, uintptr pFrom) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        uintptr pFd = default;
        uintptr v1 = default;
        int32 rc = default;
        Tsqlite3_backup _ᴛ1 = default;
        Ti64 _ᴛ2 = default;
        _ = pFd;
        _ = rc;
        _ = v1;
        _sqlite3BtreeEnter(tls, pTo);
        _sqlite3BtreeEnter(tls, pFrom);
        pFd = _sqlite3PagerFile(ref (tls).DerefOrNull(), _sqlite3BtreePager(ref (tls).DerefOrNull(), pTo));
        if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pFd)).Value.FpMethods != 0) {
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 80))).Value = (int64)_sqlite3BtreeGetPageSize(ref (tls).DerefOrNull(), pFrom) * libc.Int64FromUint32(_sqlite3BtreeLastPage(ref (tls).DerefOrNull(), pFrom));
            rc = _sqlite3OsFileControl(tls, pFd, (int32)SQLITE_FCNTL_OVERWRITE, bp + 80);
            if (rc == (int32)SQLITE_NOTFOUND) {
                rc = SQLITE_OK;
            }
            if (rc != 0) {
                goto copy_finished;
            }
        }
        libc.Xmemset(tls, bp, 0, (uint64)80);
        (@unsafe.Pointer.To<Tsqlite3_backup>(__ccgo_ptr(bp))).Value.FpSrcDb = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)pFrom)).Value.Fdb;
        (@unsafe.Pointer.To<Tsqlite3_backup>(__ccgo_ptr(bp))).Value.FpSrc = pFrom;
        (@unsafe.Pointer.To<Tsqlite3_backup>(__ccgo_ptr(bp))).Value.FpDest = pTo;
        (@unsafe.Pointer.To<Tsqlite3_backup>(__ccgo_ptr(bp))).Value.FiNext = (uint32)1;
        Xsqlite3_backup_step(tls, bp, (int32)0x7FFFFFFF);
        rc = Xsqlite3_backup_finish(tls, bp);
        if (rc == SQLITE_OK) {
            v1 = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)pTo)).Value.FpBt + 40;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(BTS_PAGESIZE_FIXED));
        } else {
            _sqlite3PagerClearCache(tls, _sqlite3BtreePager(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<Tsqlite3_backup>(__ccgo_ptr(bp))).Value.FpDest));
        }
        goto copy_finished;
copy_finished:;
        _sqlite3BtreeLeave(tls, pFrom);
        _sqlite3BtreeLeave(tls, pTo);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3ColumnSetColl(ж<libc.TLS> tls, uintptr db, uintptr pCol, uintptr zColl) {
    Ti64 n = default;
    Ti64 nColl = default;
    uintptr zNew = default;
    uintptr v1 = default;
    _ = n;
    _ = nColl;
    _ = zNew;
    _ = v1;
    n = (int64)(_sqlite3Strlen30(tls, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName) + (int32)1);
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_HASTYPE) != 0) {
        n = n + (int64)(_sqlite3Strlen30(tls, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName + (uintptr)n) + (int32)1);
    }
    nColl = (int64)(_sqlite3Strlen30(tls, zColl) + (int32)1);
    zNew = _sqlite3DbRealloc(tls, db, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName, libc.Uint64FromInt64(nColl + n));
    if (zNew != 0) {
        (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName = zNew;
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName + (uintptr)n, zColl, libc.Uint64FromInt64(nColl));
        v1 = pCol + 14;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(COLFLAG_HASCOLL));
    }
}

internal static void _sqlite3ConnectionUnlocked(ж<libc.TLS> tls, uintptr db) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        uintptr aArg = default;
        uintptr aDyn = default;
        uintptr p = default;
        uintptr pNew = default;
        uintptr pp = default;
        ref var xUnlockNotify = ref heap(new uintptr(), out var ᏑxUnlockNotify);
        uintptr v2 = default;
        int32 nArg = default;
        int32 v3 = default;
        array<uintptr> _ᴛ1 = new(16);
        _ = aArg;
        _ = aDyn;
        _ = nArg;
        _ = p;
        _ = pNew;
        _ = pp;
        _ = xUnlockNotify;
        _ = v2;
        _ = v3;
        xUnlockNotify = (uintptr)0;
        nArg = 0;
        aDyn = (uintptr)0;
        aArg = bp;
        _enterMutex(tls);
        pp = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_sqlite3BlockedList);
        for (;;) {
            if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0)) {
                break;
            }
            p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp));
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpBlockingConnection == db) {
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpBlockingConnection = (uintptr)0;
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpUnlockConnection == db) {
                if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FxUnlockNotify != xUnlockNotify && nArg != 0) {
                    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, int32>>(xUnlockNotify)(tls, aArg, nArg);
                    nArg = 0;
                }
                _sqlite3BeginBenignMalloc(tls);
                if (!(aDyn != 0) && nArg == libc.Int32FromUint64(libc.Uint64FromInt64(128) / libc.Uint64FromInt64(8)) || aDyn != 0 && nArg == libc.Int32FromUint64(libc.Uint64FromInt32(_sqlite3MallocSize(tls, aDyn)) / libc.Uint64FromInt64(8))) {
                    pNew = _sqlite3Malloc(tls, (uint64)(libc.Uint64FromInt32(nArg) * (uint64)8 * (uint64)2));
                    if (pNew != 0) {
                        libc.Xmemcpy(tls, pNew, aArg, libc.Uint64FromInt32(nArg) * (uint64)8);
                        Xsqlite3_free(tls, aDyn);
                        v2 = pNew;
                        aArg = v2;
                        aDyn = v2;
                    } else {
                        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, int32>>(xUnlockNotify)(tls, aArg, nArg);
                        nArg = 0;
                    }
                }
                _sqlite3EndBenignMalloc(tls);
                v3 = nArg;
                nArg = nArg + 1;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(aArg + (uintptr)v3 * 8))).Value = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpUnlockArg;
                xUnlockNotify = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FxUnlockNotify;
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpUnlockConnection = (uintptr)0;
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FxUnlockNotify = (uintptr)0;
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpUnlockArg = (uintptr)0;
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpBlockingConnection == (uintptr)0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpUnlockConnection == (uintptr)0) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpNextBlocked;
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpNextBlocked = (uintptr)0;
            } else {
                pp = p + 864;
            }
            goto _1;
_1:;
        }
        if (nArg != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, int32>>(xUnlockNotify)(tls, aArg, nArg);
        }
        Xsqlite3_free(tls, aDyn);
        _leaveMutex(tls);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3DefaultRowEst(ж<libc.TLS> tls, uintptr pIdx) {
    uintptr a = default;
    int32 i = default;
    int32 nCopy = default;
    int32 v1 = default;
    TLogEst x = default;
    TLogEst v2 = default;
    _ = a;
    _ = i;
    _ = nCopy;
    _ = x;
    _ = v1;
    _ = v2;
    a = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiRowLogEst;
    if (libc.Int32FromUint64(libc.Uint64FromInt64(10) / libc.Uint64FromInt64(2)) < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol)) {
        v1 = libc.Int32FromUint64(libc.Uint64FromInt64(10) / libc.Uint64FromInt64(2));
    } else {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
    }
    nCopy = v1;
    x = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FnRowLogEst;
    if ((int32)x < (int32)99) {
        v2 = libc.Int16FromInt32(99);
        x = v2;
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FnRowLogEst = v2;
    }
    if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != (uintptr)0) {
        x = (int16)((int32)x - libc.Int32FromInt32(10));
    }
    (@unsafe.Pointer.To<TLogEst>(__ccgo_ptr(a))).Value = x;
    libc.Xmemcpy(tls, a + 1 * 2, (uintptr)Ꮡ_aVal, libc.Uint64FromInt32(nCopy) * (uint64)2);
    i = nCopy + (int32)1;
    for (;;) {
        if (!(i <= libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol))) {
            break;
        }
        (@unsafe.Pointer.To<TLogEst>(__ccgo_ptr(a + (uintptr)i * 2))).Value = (int16)23;
        goto _3;
_3:;
        i = i + 1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError) != OE_None) {
        (@unsafe.Pointer.To<TLogEst>(__ccgo_ptr(a + (uintptr)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol * 2))).Value = 0;
    }
}

internal static int32 /*r*/ _sqlite3ExprCanReturnSubtype(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        TWalker _ᴛ1 = new();
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_exprNodeCanReturnSubtype);
        _sqlite3WalkExpr(tls, bp, pExpr);
        r = libc.Int32FromUint16((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _sqlite3ExprInt32(ж<libc.TLS> tls, uintptr db, int32 iVal) {
    uintptr pNew = default;
    int32 v1 = default;
    _ = pNew;
    _ = v1;
    pNew = _sqlite3DbMallocRawNN(tls, db, (uint64)72);
    if (pNew != 0) {
        libc.Xmemset(tls, pNew, 0, (uint64)72);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.Fop = (uint8)TK_INTEGER;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FiAgg = (int16)(-(int32)1);
        if (iVal != 0) {
            v1 = (int32)EP_IsTrue;
        } else {
            v1 = (int32)EP_IsFalse;
        }
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.Fflags = libc.Uint32FromInt32(libc.Int32FromInt32(EP_IntValue) | libc.Int32FromInt32(EP_Leaf) | v1);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew).of(TExpr.ᏑFu).Reinterpret<TExprᴛ1_Fu, int32>()).Value = iVal;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.FnHeight = (int32)1;
    }
    return pNew;
}

internal static int32 /*r*/ _sqlite3ExprReferencesUpdatedColumn(ж<libc.TLS> tls, uintptr pExpr, uintptr aiChng, int32 chngRowid) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr v1 = default;
        TWalker _ᴛ1 = new();
        _ = v1;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode = (uint16)0;
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_checkConstraintExprNode);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = aiChng;
        _sqlite3WalkExpr(tls, bp, pExpr);
        if (!(chngRowid != 0)) {
            v1 = bp + 36;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(CKCNSTRNT_ROWID));
        }
        r = libc.BoolInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode) != 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static float64 /*r*/ _sqlite3Fp10Convert2(ж<libc.TLS> tls, Tu64 d, int32 p) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 adj = default;
        int32 b = default;
        int32 e = default;
        int32 lp = default;
        int32 s = default;
        Tu64 hi = default;
        ref var lo = ref heap(new Tu64(), out var Ꮡlo);
        Tu64 pwr10h = default;
        Tu64 sticky = default;
        Tu64 u = default;
        Tu64 x = default;
        Tu32 mid1 = default;
        Tu32 mid2 = default;
        ref var pwr10l = ref heap(new Tu32(), out var Ꮡpwr10l);
        Tu64 _ᴛ1 = default;
        float64 _ᴛ2 = default;
        _ = adj;
        _ = b;
        _ = e;
        _ = hi;
        _ = lo;
        _ = lp;
        _ = mid1;
        _ = mid2;
        _ = pwr10h;
        _ = pwr10l;
        _ = s;
        _ = sticky;
        _ = u;
        _ = x;
        if (p < -(int32)348) {
            r = (float64)0D; goto ᒐdone;
        }
        if (p > +libc.Int32FromInt32(347)) {
            r = (float64)libc.X__builtin_inff(tls); goto ᒐdone;
        }
        b = (int32)64 - _countLeadingZeros(ref (tls).DerefOrNull(), d);
        lp = _pwr10to2(ref (tls).DerefOrNull(), p);
        e = (int32)53 - b - lp;
        if (e > (int32)1074) {
            if (e >= (int32)1130) {
                r = (float64)0D; goto ᒐdone;
            }
            e = (int32)1074;
        }
        s = -(e - ((int32)64 - b) + lp + (int32)3);
        pwr10h = _powerOfTen(ref (tls).DerefOrNull(), p, @unsafe.Pointer.FromPinnedBox(Ꮡpwr10l));
        if (pwr10l != (uint32)0) {
            pwr10h = pwr10h + 1;
            pwr10l = ~pwr10l;
        }
        x = d << (int)((int32)64 - b);
        hi = _sqlite3Multiply128(ref (tls).DerefOrNull(), x, pwr10h, @unsafe.Pointer.FromPinnedBox(Ꮡlo));
        mid1 = (uint32)(lo >> (int)((int32)32));
        sticky = (uint64)1;
        if ((Tu64)(hi & (libc.Uint64FromInt32(1).Lsh((int64)(s)) - (uint64)1)) == (uint64)0) {
            mid2 = (uint32)(_sqlite3Multiply128(ref (tls).DerefOrNull(), x, (uint64)pwr10l << (int)((int32)32), @unsafe.Pointer.FromPinnedBox(Ꮡlo)) >> (int)((int32)32));
            sticky = libc.BoolUint64(mid1 - mid2 > libc.Uint32FromInt32(1));
            hi = hi - libc.BoolUint64(mid1 < mid2);
        }
        u = (Tu64)(hi >> (int)(s) | sticky);
        adj = libc.BoolInt32(u >= libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(55))) - libc.Uint64FromInt32(2));
        if (adj != 0) {
            u = (Tu64)(u >> (int)(adj) | (Tu64)(u & (uint64)1));
            e = e - adj;
        }
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (u + (uint64)1 + (Tu64)(u >> (int)(libc.Int32FromInt32(2)) & (uint64)1)) >> (int)((int32)2);
        if (e <= -(int32)972) {
            r = (float64)libc.X__builtin_inff(tls); goto ᒐdone;
        }
        if ((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) & (libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(52))))) != (uint64)0) {
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) & ~(libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(52))))) | libc.Uint64FromInt32(libc.Int32FromInt32(1075) - e).Lsh((int64)(libc.Int32FromInt32(52))));
        }
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        r = ~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp + 8));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3Fts5BufferAppendBlob(ж<libc.TLS> tls, @unsafe.Pointer pRc, uintptr pBuf, Tu32 nData, uintptr pData) {
    int32 v1 = default;
    uintptr v2 = default;
    _ = v1;
    _ = v2;
    if (nData != 0) {
        if (libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn) + nData <= libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.FnSpace)) {
            v1 = 0;
        } else {
            v1 = _sqlite3Fts5BufferSize(tls, pRc, (@unsafe.Pointer)pBuf, nData + libc.Uint32FromInt32((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn));
        }
        if (v1 != 0) {
            return;
        }
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)(@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn, pData, (uint64)nData);
        v2 = pBuf + 8;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = (int32)((uint32)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)) + nData);
    }
}

internal static int32 /*r*/ _sqlite3Fts5ExprAnd(ж<libc.TLS> tls, uintptr pp1, uintptr p2) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr ap = default;
        uintptr p1 = default;
        int32 i = default;
        int32 nPhrase = default;
        TFts5Parse _ᴛ1 = default;
        _ = ap;
        _ = i;
        _ = nPhrase;
        _ = p1;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp1)) != 0 && p2 != 0) {
            p1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp1));
            nPhrase = (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FnPhrase + (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FnPhrase;
            (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FpRoot = _sqlite3Fts5ParseNode(tls, __ccgo_ptr(bp), (int32)FTS5_AND, (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FpRoot, (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FpRoot, (uintptr)0);
            (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FpRoot = (uintptr)0;
            if ((@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc == SQLITE_OK) {
                ap = Xsqlite3_realloc64(tls, (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FapExprPhrase, (uint64)(libc.Uint64FromInt32(nPhrase) * (uint64)8));
                if (ap == (uintptr)0) {
                    (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc = (int32)SQLITE_NOMEM;
                } else {
                    libc.Xmemmove(tls, ap + (uintptr)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FnPhrase * 8, ap, libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FnPhrase) * (uint64)8);
                    i = 0;
                    for (;;) {
                        if (!(i < (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FnPhrase)) {
                            break;
                        }
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ap + (uintptr)i * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FapExprPhrase + (uintptr)i * 8))).Value;
                        goto _1;
_1:;
                        i = i + 1;
                    }
                    (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FnPhrase = nPhrase;
                    (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p1)).Value.FapExprPhrase = ap;
                }
            }
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)p2)).Value.FapExprPhrase);
            Xsqlite3_free(tls, p2);
        } else {
            if (p2 != 0) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp1))).Value = p2;
            }
        }
        r = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _sqlite3Fts5ExprClearPoslists(ж<libc.TLS> tls, uintptr pExpr, int32 bLive) {
    int32 i = default;
    uintptr pBuf = default;
    uintptr pNode = default;
    uintptr pRet = default;
    _ = i;
    _ = pBuf;
    _ = pNode;
    _ = pRet;
    pRet = Xsqlite3_malloc64(tls, (uint64)((uint64)16 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase)));
    if (pRet != 0) {
        libc.Xmemset(tls, pRet, 0, (uint64)16 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase));
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase)) {
                break;
            }
            pBuf = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)i * 8)) + 8;
            pNode = (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)i * 8))))).Value.FpNode;
            if (bLive != 0 && ((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn == 0 || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FiRowid != (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpRoot)).Value.FiRowid || (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pNode)).Value.FbEof != 0)) {
                (@unsafe.Pointer.To<TFts5PoslistPopulator>(__ccgo_ptr(pRet + (uintptr)i * 16))).Value.FbMiss = (int32)1;
            } else {
                (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn = 0;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return pRet;
}

internal static int32 /*r*/ _sqlite3Fts5ExprClonePhrase(ж<libc.TLS> tls, uintptr pExpr, int32 iPhrase, uintptr ppNew) {
    int32 i = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    int32 tflags = default;
    Tsqlite3_int64 nByte = default;
    uintptr p = default;
    uintptr pColset = default;
    uintptr pColsetOrig = default;
    uintptr pNew = default;
    uintptr pOrig = default;
    ref var sCtx = ref heap(new TTokenCtx(), out var ᏑsCtx);
    _ = i;
    _ = nByte;
    _ = p;
    _ = pColset;
    _ = pColsetOrig;
    _ = pNew;
    _ = pOrig;
    _ = rc;
    _ = sCtx;
    _ = tflags;
    rc = SQLITE_OK;
    pOrig = (uintptr)0;
    pNew = (uintptr)0;
    sCtx = new TTokenCtx();
    if (!(pExpr != 0) || iPhrase < 0 || iPhrase >= (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FnPhrase) {
        rc = (int32)SQLITE_RANGE;
    } else {
        pOrig = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FapExprPhrase + (uintptr)iPhrase * 8));
        pNew = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)40);
    }
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FapExprPhrase = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), (int64)8);
    }
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 48) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(8)));
    }
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FpNear = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 24) + libc.Uint64FromInt32(libc.Int32FromInt32(2)) * libc.Uint64FromInt64(8)));
    }
    if (rc == SQLITE_OK && pOrig != (uintptr)0) {
        pColsetOrig = (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pOrig)).Value.FpNode)).Value.FpNear)).Value.FpColset;
        if (pColsetOrig != 0) {
            nByte = libc.Int64FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(quo(((@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)pColsetOrig)).Value.FnCol + libc.Int32FromInt32(2)), libc.Int32FromInt32(2))));
            pColset = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), nByte);
            if (pColset != 0) {
                libc.Xmemcpy(tls, pColset, pColsetOrig, libc.Uint64FromInt64(nByte));
            }
            (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FpNear)).Value.FpColset = pColset;
        }
    }
    if (rc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pOrig)).Value.FnTerm != 0) {
            sCtx.FpConfig = (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpConfig;
            i = 0;
            for (;;) {
                if (!(rc == SQLITE_OK && i < (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pOrig)).Value.FnTerm)) {
                    break;
                }
                tflags = 0;
                p = pOrig + 32 + (uintptr)i * 40;
                for (;;) {
                    if (!(p != 0 && rc == SQLITE_OK)) {
                        break;
                    }
                    rc = _fts5ParseTokenize__ccgo_gp(tls, @unsafe.Pointer.FromPinnedBox(ᏑsCtx), tflags, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpTerm, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FnFullTerm, 0, 0);
                    tflags = (int32)FTS5_TOKEN_COLOCATED;
                    goto _2;
_2:;
                    p = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpSynonym;
                }
                if (rc == SQLITE_OK) {
                    (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(sCtx.FpPhrase + 32 + (uintptr)i * 40))).Value.FbPrefix = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pOrig + 32 + (uintptr)i * 40))).Value.FbPrefix;
                    (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(sCtx.FpPhrase + 32 + (uintptr)i * 40))).Value.FbFirst = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pOrig + 32 + (uintptr)i * 40))).Value.FbFirst;
                }
                goto _1;
_1:;
                i = i + 1;
            }
        } else {
            sCtx.FpPhrase = _sqlite3Fts5MallocZero(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(40)));
        }
    }
    if (rc == SQLITE_OK && sCtx.FpPhrase != 0) {
        (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpIndex = (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpIndex;
        (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpConfig = (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pExpr)).Value.FpConfig;
        (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FnPhrase = (int32)1;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FapExprPhrase))).Value = sCtx.FpPhrase;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FpNear + 24))).Value = sCtx.FpPhrase;
        (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FpNear)).Value.FnPhrase = (int32)1;
        (@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)sCtx.FpPhrase)).Value.FpNode = (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot;
        if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pOrig)).Value.FnTerm == (int32)1 && (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pOrig + 32))).Value.FpSynonym == (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)(pOrig + 32))).Value.FbFirst) == 0) {
            (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FeType = (int32)FTS5_TERM;
            (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FxNext = __ccgo_fp(_fts5ExprNodeNext_TERM);
        } else {
            (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FeType = (int32)FTS5_STRING;
            (@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot)).Value.FxNext = __ccgo_fp(_fts5ExprNodeNext_STRING);
        }
    } else {
        _sqlite3Fts5ExprFree(tls, pNew);
        _fts5ExprPhraseFree(tls, sCtx.FpPhrase);
        pNew = (uintptr)0;
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew))).Value = pNew;
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5ExprNew(ж<libc.TLS> tls, uintptr pConfig, int32 bPhraseToAnd, int32 iCol, uintptr zExpr, @unsafe.Pointer ppNew, uintptr pzErr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 n = default;
        int32 t = default;
        uintptr pColset = default;
        uintptr pEngine = default;
        uintptr pNew = default;
        ref var z = ref heap(new uintptr(), out var Ꮡz);
        uintptr v1 = default;
        TFts5Parse _ᴛ1 = default;
        TFts5Token _ᴛ2 = default;
        _ = n;
        _ = pColset;
        _ = pEngine;
        _ = pNew;
        _ = t;
        _ = z;
        _ = v1;
        z = zExpr;
        (@unsafe.Pointer.To<uintptr>(ppNew)).Value = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = (uintptr)0;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FbPhraseToAnd = bPhraseToAnd;
        pEngine = _sqlite3Fts5ParserAlloc(tls, __ccgo_fp(_fts5ParseAlloc));
        if (pEngine == (uintptr)0) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FpConfig = pConfig;
        for (var cond = true; cond; cond = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc == SQLITE_OK && t != FTS5_EOF) {
            t = _fts5ExprGetToken(tls, __ccgo_ptr(bp), @unsafe.Pointer.FromBox(Ꮡz), bp + 48);
            _sqlite3Fts5Parser(tls, pEngine, t, ~@unsafe.Pointer.To<TFts5Token>(__ccgo_ptr(bp + 48)), bp);
        }
        _sqlite3Fts5ParserFree(tls, pEngine, __ccgo_fp(_fts5ParseFree));
        if ((@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc == SQLITE_OK && iCol < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol) {
            n = libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(quo((libc.Int32FromInt32(1) + libc.Int32FromInt32(2)), libc.Int32FromInt32(2))));
            pColset = _sqlite3Fts5MallocZero(tls, __ccgo_ptr(bp + 16), (int64)n);
            if (pColset != 0) {
                (@unsafe.Pointer.To<TFts5Colset>((@unsafe.Pointer)pColset)).Value.FnCol = (int32)1;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pColset + 4))).Value = iCol;
                _sqlite3Fts5ParseSetColset(tls, __ccgo_ptr(bp), (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FpExpr, pColset);
            }
        }
        if ((@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc == SQLITE_OK) {
            v1 = Xsqlite3_malloc64(tls, (uint64)40);
            pNew = v1;
            (@unsafe.Pointer.To<uintptr>(ppNew)).Value = v1;
            if (pNew == (uintptr)0) {
                (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc = (int32)SQLITE_NOMEM;
                _sqlite3Fts5ParseNodeFree(tls, (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FpExpr);
            } else {
                (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpRoot = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FpExpr;
                (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpIndex = (uintptr)0;
                (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FpConfig = pConfig;
                (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FapExprPhrase = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FapPhrase;
                (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FnPhrase = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FnPhrase;
                (@unsafe.Pointer.To<TFts5Expr>((@unsafe.Pointer)pNew)).Value.FbDesc = 0;
                (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FapPhrase = (uintptr)0;
            }
        } else {
            _sqlite3Fts5ParseNodeFree(tls, (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FpExpr);
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FapPhrase);
        if ((uintptr)0 == ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FzErr;
        } else {
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.FzErr);
        }
        r = (@unsafe.Pointer.To<TFts5Parse>(__ccgo_ptr(bp))).Value.Frc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3Fts5HashClear(ж<libc.TLS> tls, uintptr pHash) {
    int32 i = default;
    uintptr pNext = default;
    uintptr pSlot = default;
    _ = i;
    _ = pNext;
    _ = pSlot;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot)) {
            break;
        }
        pSlot = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)i * 8));
        for (;;) {
            if (!(pSlot != 0)) {
                break;
            }
            pNext = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)pSlot)).Value.FpHashNext;
            Xsqlite3_free(tls, pSlot);
            goto _2;
_2:;
            pSlot = pNext;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot, 0, libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot) * (uint64)8);
    (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnEntry = 0;
}

internal static int32 /*r*/ _sqlite3Fts5HashNew(ж<libc.TLS> tls, uintptr pConfig, uintptr ppNew, uintptr pnByte) {
    Tsqlite3_int64 nByte = default;
    uintptr pNew = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = nByte;
    _ = pNew;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    v1 = Xsqlite3_malloc64(tls, (uint64)40);
    pNew = v1;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew))).Value = v1;
    if (pNew == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        libc.Xmemset(tls, pNew, 0, (uint64)40);
        (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FpnByte = pnByte;
        (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FeDetail = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail;
        (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FnSlot = (int32)1024;
        nByte = libc.Int64FromUint64((uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FnSlot));
        (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FaSlot = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if ((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FaSlot == (uintptr)0) {
            Xsqlite3_free(tls, pNew);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppNew))).Value = (uintptr)0;
            rc = (int32)SQLITE_NOMEM;
        } else {
            libc.Xmemset(tls, (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pNew)).Value.FaSlot, 0, libc.Uint64FromInt64(nByte));
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5HashQuery(ж<libc.TLS> tls, uintptr pHash, int32 nPre, uintptr pTerm, int32 nTerm, @unsafe.Pointer ppOut, @unsafe.Pointer pnDoclist) {
    uint32 iHash = default;
    int32 nHashPre = default;
    int32 nList = default;
    uintptr p = default;
    uintptr pFaux = default;
    uintptr pRet = default;
    uintptr zKey = default;
    uintptr v2 = default;
    _ = iHash;
    _ = nHashPre;
    _ = nList;
    _ = p;
    _ = pFaux;
    _ = pRet;
    _ = zKey;
    _ = v2;
    iHash = _fts5HashKey(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FnSlot, pTerm, nTerm);
    zKey = (uintptr)0;
    p = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Hash>((@unsafe.Pointer)pHash)).Value.FaSlot + (uintptr)iHash * 8));
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        zKey = p + 1 * 48;
        if (nTerm == (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnKey && libc.Xmemcmp(tls, zKey, pTerm, libc.Uint64FromInt32(nTerm)) == 0) {
            break;
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FpHashNext;
    }
    if (p != 0) {
        nHashPre = libc.Int32FromUint64((uint64)48 + libc.Uint64FromInt32(nTerm));
        nList = (@unsafe.Pointer.To<TFts5HashEntry>((@unsafe.Pointer)p)).Value.FnData - nHashPre;
        v2 = Xsqlite3_malloc64(tls, libc.Uint64FromInt32(nPre + nList + (int32)10));
        (@unsafe.Pointer.To<uintptr>(ppOut)).Value = v2;
        pRet = v2;
        if (pRet != 0) {
            pFaux = pRet + (uintptr)(nPre - nHashPre);
            libc.Xmemcpy(tls, pRet + (uintptr)nPre, p + (uintptr)nHashPre, libc.Uint64FromInt32(nList));
            nList = nList + _fts5HashAddPoslistSize(tls, pHash, p, pFaux);
            (@unsafe.Pointer.To<int32>(pnDoclist)).Value = nList;
        } else {
            (@unsafe.Pointer.To<int32>(pnDoclist)).Value = 0;
            return (int32)SQLITE_NOMEM;
        }
    } else {
        (@unsafe.Pointer.To<uintptr>(ppOut)).Value = (uintptr)0;
        (@unsafe.Pointer.To<int32>(pnDoclist)).Value = 0;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3Fts5IndexGetAverages(ж<libc.TLS> tls, uintptr p, uintptr pnRow, uintptr anSize) {
    int32 i = default;
    int32 iCol = default;
    int32 nCol = default;
    uintptr pData = default;
    _ = i;
    _ = iCol;
    _ = nCol;
    _ = pData;
    nCol = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FnCol;
    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pnRow))).Value = 0;
    libc.Xmemset(tls, anSize, 0, (uint64)8 * libc.Uint64FromInt32(nCol));
    pData = _fts5DataRead(tls, p, (int64)FTS5_AVERAGES_ROWID);
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn != 0) {
        i = 0;
        i = i + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fp + (uintptr)i), (@unsafe.Pointer)pnRow));
        iCol = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fnn && iCol < nCol)) {
                break;
            }
            i = i + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pData)).Value.Fp + (uintptr)i), (@unsafe.Pointer)(anSize + (uintptr)iCol * 8)));
            goto _1;
_1:;
            iCol = iCol + 1;
        }
    }
    _fts5DataRelease(tls, pData);
    return _fts5IndexReturn(ref (tls).DerefOrNull(), p);
}

internal static int32 /*r*/ _sqlite3Fts5IndexQuery(ж<libc.TLS> tls, uintptr p, uintptr pToken, int32 nToken, int32 flags, uintptr pColset, uintptr ppIter) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 bDesc = default;
        int32 bTokendata = default;
        int32 iIdx = default;
        int32 iPrefixIdx = default;
        int32 nChar = default;
        int32 nIdxChar = default;
        uintptr pConfig = default;
        ref var pRet = ref heap(new uintptr(), out var ᏑpRet);
        uintptr pSeg = default;
        uintptr pStruct = default;
        TFts5Buffer _ᴛ1 = default;
        _ = bDesc;
        _ = bTokendata;
        _ = iIdx;
        _ = iPrefixIdx;
        _ = nChar;
        _ = nIdxChar;
        _ = pConfig;
        _ = pRet;
        _ = pSeg;
        _ = pStruct;
        pConfig = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig;
        pRet = (uintptr)0;
        (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value = new TFts5Buffer();
        if (_sqlite3Fts5BufferSize(tls, (@unsafe.Pointer)(p + 60), __ccgo_ptr(bp), libc.Uint32FromInt32(nToken + (int32)1)) == 0) {
            iIdx = 0;
            iPrefixIdx = 0;
            bTokendata = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbTokendata;
            if (nToken > 0) {
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp + 1, pToken, libc.Uint64FromInt32(nToken));
            }
            if ((flags & (libc.Int32FromInt32(FTS5INDEX_QUERY_NOTOKENDATA) | libc.Int32FromInt32(FTS5INDEX_QUERY_SCAN))) != 0) {
                bTokendata = 0;
            }
            if ((flags & (int32)FTS5INDEX_QUERY_PREFIX) != 0) {
                nChar = _fts5IndexCharlen(ref (tls).DerefOrNull(), pToken, nToken);
                iIdx = (int32)1;
                for (;;) {
                    if (!(iIdx <= (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnPrefix)) {
                        break;
                    }
                    nIdxChar = ~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FaPrefix + (uintptr)(iIdx - (int32)1) * 4));
                    if (nIdxChar == nChar) {
                        break;
                    }
                    if (nIdxChar == nChar + (int32)1) {
                        iPrefixIdx = iIdx;
                    }
                    goto _1;
_1:;
                    iIdx = iIdx + 1;
                }
            }
            if (bTokendata != 0 && iIdx == 0) {
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp))).Value = (uint8)'0';
                pRet = _fts5SetupTokendataIter(tls, p, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, nToken + (int32)1, pColset);
            } else {
                if (iIdx <= (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnPrefix) {
                    pStruct = _fts5StructureRead(tls, p);
                    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp))).Value = libc.Uint8FromInt32(libc.Int32FromUint8((uint8)'0') + iIdx);
                    if (pStruct != 0) {
                        _fts5MultiIterNew(tls, p, pStruct, flags | (int32)FTS5INDEX_QUERY_SKIPEMPTY, pColset, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, nToken + (int32)1, -(int32)1, 0, @unsafe.Pointer.FromBox(ᏑpRet));
                        _fts5StructureRelease(tls, pStruct);
                    }
                } else {
                    bDesc = libc.BoolInt32((flags & (int32)FTS5INDEX_QUERY_DESC) != 0);
                    _fts5SetupPrefixIter(tls, p, bDesc, iPrefixIdx, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp))).Value.Fp, nToken + (int32)1, pColset, @unsafe.Pointer.FromBox(ᏑpRet));
                    if (pRet == (uintptr)0) {
                    } else {
                        _fts5IterSetOutputCb(tls, p + 60, pRet);
                        if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                            pSeg = pRet + 104 + (uintptr)(@unsafe.Pointer.To<TFts5CResult>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pRet)).Value.FaFirst + 1 * 4))).Value.FiFirst * 128;
                            if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pSeg)).Value.FpLeaf != 0) {
                                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr>>((@unsafe.Pointer.To<TFts5Iter>((@unsafe.Pointer)pRet)).Value.FxSetOutputs)(tls, pRet, pSeg);
                            }
                        }
                    }
                }
            }
            if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc != 0) {
                _fts5IterClose(tls, pRet);
                pRet = (uintptr)0;
                _fts5IndexCloseReader(tls, p);
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppIter))).Value = pRet;
            _sqlite3Fts5BufferFree(tls, bp);
        }
        r = _fts5IndexReturn(ref (tls).DerefOrNull(), p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _sqlite3Fts5MallocZero(ж<libc.TLS> tls, @unsafe.Pointer pRc, Tsqlite3_int64 nByte) {
    uintptr pRet = default;
    _ = pRet;
    pRet = (uintptr)0;
    if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
        pRet = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
        if (pRet == (uintptr)0) {
            if (nByte > 0) {
                (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
            }
        } else {
            libc.Xmemset(tls, pRet, 0, libc.Uint64FromInt64(nByte));
        }
    }
    return pRet;
}

internal static uintptr /*r*/ _sqlite3Fts5ParseImplicitAnd(ж<libc.TLS> tls, uintptr pParse, uintptr pLeft, uintptr pRight) {
    uintptr ap = default;
    uintptr pPrev = default;
    uintptr pRet = default;
    _ = ap;
    _ = pPrev;
    _ = pRet;
    pRet = (uintptr)0;
    if ((@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.Frc != 0) {
        _sqlite3Fts5ParseNodeFree(tls, pLeft);
        _sqlite3Fts5ParseNodeFree(tls, pRight);
    } else {
        if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pLeft)).Value.FeType == (int32)FTS5_AND) {
            pPrev = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pLeft + 48 + (uintptr)((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pLeft)).Value.FnChild - (int32)1) * 8));
        } else {
            pPrev = pLeft;
        }
        if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pRight)).Value.FeType == FTS5_EOF) {
            _sqlite3Fts5ParseNodeFree(tls, pRight);
            pRet = pLeft;
            (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase = (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase - 1;
        } else {
            if ((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pPrev)).Value.FeType == FTS5_EOF) {
                if (pPrev == pLeft) {
                    pRet = pRight;
                } else {
                    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pLeft + 48 + (uintptr)((@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pLeft)).Value.FnChild - (int32)1) * 8))).Value = pRight;
                    pRet = pLeft;
                }
                ap = (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FapPhrase + (uintptr)((@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase - (int32)1 - (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pRight)).Value.FpNear)).Value.FnPhrase) * 8;
                libc.Xmemmove(tls, ap, ap + 1 * 8, (uint64)8 * libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprNode>((@unsafe.Pointer)pRight)).Value.FpNear)).Value.FnPhrase));
                (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase = (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase - 1;
                _sqlite3Fts5ParseNodeFree(tls, pPrev);
            } else {
                pRet = _sqlite3Fts5ParseNode(tls, (@unsafe.Pointer)pParse, (int32)FTS5_AND, pLeft, pRight, (uintptr)0);
            }
        }
    }
    return pRet;
}

internal static uintptr /*r*/ _sqlite3Fts5ParseNearset(ж<libc.TLS> tls, @unsafe.Pointer pParse, uintptr pNear, uintptr pPhrase) {
    int32 SZALLOC = default;
    int32 nNew = default;
    int32 v1 = default;
    Tsqlite3_int64 nByte = default;
    Tsqlite3_int64 nByte1 = default;
    uintptr pLast = default;
    uintptr pRet = default;
    uintptr v2 = default;
    _ = SZALLOC;
    _ = nByte;
    _ = nByte1;
    _ = nNew;
    _ = pLast;
    _ = pRet;
    _ = v1;
    _ = v2;
    SZALLOC = (int32)8;
    pRet = (uintptr)0;
    if (~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 16)) == SQLITE_OK) {
        if (pNear == (uintptr)0) {
            nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 24) + libc.Uint64FromInt32(SZALLOC + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(8));
            pRet = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte));
            if (pRet == (uintptr)0) {
                (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 16))).Value = (int32)SQLITE_NOMEM;
            } else {
                libc.Xmemset(tls, pRet, 0, libc.Uint64FromInt64(nByte));
            }
        } else {
            if (rem((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase, SZALLOC) == 0) {
                nNew = (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pNear)).Value.FnPhrase + SZALLOC;
                nByte1 = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 24) + libc.Uint64FromInt32(nNew + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(8));
                pRet = Xsqlite3_realloc64(tls, pNear, libc.Uint64FromInt64(nByte1));
                if (pRet == (uintptr)0) {
                    (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 16))).Value = (int32)SQLITE_NOMEM;
                }
            } else {
                pRet = pNear;
            }
        }
    }
    if (pRet == (uintptr)0) {
        _sqlite3Fts5ParseNearsetFree(tls, pNear);
        _sqlite3Fts5ParsePhraseFree(tls, pPhrase);
    } else {
        if ((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pRet)).Value.FnPhrase > 0) {
            pLast = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRet + 24 + (uintptr)((@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pRet)).Value.FnPhrase - (int32)1) * 8));
            if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pPhrase)).Value.FnTerm == 0) {
                _fts5ExprPhraseFree(tls, pPhrase);
                (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pRet)).Value.FnPhrase = (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pRet)).Value.FnPhrase - 1;
                (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 20))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 20)) - 1;
                pPhrase = pLast;
            } else {
                if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)pLast)).Value.FnTerm == 0) {
                    _fts5ExprPhraseFree(tls, pLast);
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pParse, 24)) + (uintptr)(~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 20)) - (int32)2) * 8))).Value = pPhrase;
                    (@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 20))).Value = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pParse, 20)) - 1;
                    (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pRet)).Value.FnPhrase = (@unsafe.Pointer.To<TFts5ExprNearset>((@unsafe.Pointer)pRet)).Value.FnPhrase - 1;
                }
            }
        }
        v2 = pRet + 16;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pRet + 24 + (uintptr)v1 * 8))).Value = pPhrase;
    }
    return pRet;
}

internal static uintptr /*r*/ _sqlite3Fts5ParseTerm(ж<libc.TLS> tls, uintptr pParse, uintptr pAppend, uintptr pToken, int32 bPrefix) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 flags = default;
        int32 n = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr pConfig = default;
        ref var z = ref heap(new uintptr(), out var Ꮡz);
        bool v3 = default;
        TTokenCtx _ᴛ1 = default;
        _ = flags;
        _ = n;
        _ = pConfig;
        _ = rc;
        _ = z;
        _ = v1;
        _ = v3;
        pConfig = (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FpConfig;
        z = (uintptr)0;
        libc.Xmemset(tls, bp, 0, (uint64)24);
        (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase = pAppend;
        (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpConfig = pConfig;
        rc = _fts5ParseStringFromToken(tls, pToken, @unsafe.Pointer.FromBox(Ꮡz));
        if (rc == SQLITE_OK) {
            if (bPrefix != 0) {
                v1 = (int32)FTS5_TOKENIZE_PREFIX;
            } else {
                v1 = 0;
            }
            flags = (int32)FTS5_TOKENIZE_QUERY | v1;
            _sqlite3Fts5Dequote(ref (tls).DerefOrNull(), z);
            n = libc.Int32FromUint64(libc.Xstrlen(tls, z));
            rc = _sqlite3Fts5Tokenize(tls, pConfig, flags, z, n, bp, __ccgo_fp(_fts5ParseTokenize));
        }
        Xsqlite3_free(tls, z);
        v3 = rc != 0; if (!v3) {
            v1 = (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.Frc;
            rc = v1;
        }
        if (v3 || v1 != 0) {
            (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.Frc = rc;
            _fts5ExprPhraseFree(tls, (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase);
            (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase = (uintptr)0;
        } else {
            if (pAppend == (uintptr)0) {
                if (_parseGrowPhraseArray(tls, (@unsafe.Pointer)pParse) != 0) {
                    _fts5ExprPhraseFree(tls, (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase);
                    r = (uintptr)0; goto ᒐdone;
                }
                (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase = (@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase + 1;
            }
            if ((@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase == (uintptr)0) {
                (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase = _sqlite3Fts5MallocZero(tls, (@unsafe.Pointer)(pParse + 16), libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(40)));
            } else {
                if ((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase)).Value.FnTerm != 0) {
                    (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)((@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase + 32 + (uintptr)((@unsafe.Pointer.To<TFts5ExprPhrase>((@unsafe.Pointer)(@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase)).Value.FnTerm - (int32)1) * 40))).Value.FbPrefix = libc.Uint8FromInt32(bPrefix);
                }
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FapPhrase + (uintptr)((@unsafe.Pointer.To<TFts5Parse>((@unsafe.Pointer)pParse)).Value.FnPhrase - (int32)1) * 8))).Value = (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase;
        }
        r = (@unsafe.Pointer.To<TTokenCtx>(__ccgo_ptr(bp))).Value.FpPhrase;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3Fts5PoslistReaderInit(ж<libc.TLS> tls, uintptr a, int32 n, uintptr pIter) {
    libc.Xmemset(tls, pIter, 0, (uint64)32);
    (@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pIter)).Value.Fa = a;
    (@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pIter)).Value.Fn = n;
    _sqlite3Fts5PoslistReaderNext(ref (tls).DerefOrNull(), pIter);
    return libc.Int32FromUint8((@unsafe.Pointer.To<TFts5PoslistReader>((@unsafe.Pointer)pIter)).Value.FbEof);
}

internal static int32 /*r*/ _sqlite3Fts5StorageIndexInsert(ж<libc.TLS> tls, uintptr p, uintptr apVal, Ti64 iRowid) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 iCol = default;
        ref var nLoc = ref heap(new int32(), out var ᏑnLoc);
        ref var nText = ref heap(new int32(), out var ᏑnText);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        uintptr pConfig = default;
        ref var pLoc = ref heap(new uintptr(), out var ᏑpLoc);
        ref var pText = ref heap(new uintptr(), out var ᏑpText);
        uintptr pVal = default;
        TFts5Buffer _ᴛ1 = default;
        TFts5InsertCtx _ᴛ2 = default;
        _ = iCol;
        _ = nLoc;
        _ = nText;
        _ = pConfig;
        _ = pLoc;
        _ = pText;
        _ = pVal;
        _ = rc;
        pConfig = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig;
        rc = SQLITE_OK;
        libc.Xmemset(tls, bp + 16, 0, (uint64)16);
        (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FpStorage = p;
        rc = _fts5StorageLoadTotals(tls, p, (int32)1);
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5IndexBeginWrite(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpIndex, 0, iRowid);
        }
        (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol)) {
                break;
            }
            (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FszCol = 0;
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FabUnindexed + (uintptr)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol))) == 0) {
                nText = 0;
                pText = (uintptr)0;
                nLoc = 0;
                pLoc = (uintptr)0;
                pVal = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)((@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + (int32)2) * 8));
                if ((@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpSavedRow != 0 && Xsqlite3_value_nochange(tls, pVal) != 0) {
                    pVal = Xsqlite3_column_value(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpSavedRow, (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + (int32)1);
                    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == FTS5_CONTENT_NORMAL && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbLocale != 0) {
                        iCol = (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + (int32)1 + (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol;
                        pLoc = Xsqlite3_column_text(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpSavedRow, iCol);
                        nLoc = Xsqlite3_column_bytes(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpSavedRow, iCol);
                    }
                } else {
                    pVal = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal + (uintptr)((@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + (int32)2) * 8));
                }
                if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbLocale != 0 && _sqlite3Fts5IsLocaleValue(tls, pConfig, pVal) != 0) {
                    rc = _sqlite3Fts5DecodeLocaleValue(tls, pVal, @unsafe.Pointer.FromBox(ᏑpText), @unsafe.Pointer.FromPinnedBox(ᏑnText), @unsafe.Pointer.FromBox(ᏑpLoc), @unsafe.Pointer.FromPinnedBox(ᏑnLoc));
                } else {
                    pText = Xsqlite3_value_text(tls, pVal);
                    nText = Xsqlite3_value_bytes(tls, pVal);
                }
                if (rc == SQLITE_OK) {
                    _sqlite3Fts5SetLocale(ref (tls).DerefOrNull(), pConfig, pLoc, nLoc);
                    rc = _sqlite3Fts5Tokenize(tls, pConfig, (int32)FTS5_TOKENIZE_DOCUMENT, pText, nText, bp, __ccgo_fp(_fts5StorageInsertCallback));
                    _sqlite3Fts5ClearLocale(ref (tls).DerefOrNull(), pConfig);
                }
            }
            _sqlite3Fts5BufferAppendVarint(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ptr(bp + 16), (int64)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FszCol);
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FaTotalSize + (uintptr)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol * 8))).Value += (int64)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FszCol;
            goto _1;
_1:;
            (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol = (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + 1;
        }
        (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FnTotalRow = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FnTotalRow + 1;
        if (rc == SQLITE_OK) {
            rc = _fts5StorageInsertDocsize(tls, p, iRowid, __ccgo_ptr(bp + 16));
        }
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Buffer>(__ccgo_ptr(bp + 16))).Value.Fp);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5StorageRebuild(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var buf = ref heap(new TFts5Buffer(), out var Ꮡbuf);
        int32 iCol = default;
        ref var nLoc = ref heap(new int32(), out var ᏑnLoc);
        ref var nText = ref heap(new int32(), out var ᏑnText);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        int32 rc2 = default;
        Ti64 iRowid = default;
        uintptr pConfig = default;
        ref var pLoc = ref heap(new uintptr(), out var ᏑpLoc);
        ref var pScan = ref heap(new uintptr(), out var ᏑpScan);
        ref var pText = ref heap(new uintptr(), out var ᏑpText);
        uintptr pVal = default;
        TFts5InsertCtx _ᴛ1 = default;
        _ = buf;
        _ = iCol;
        _ = iRowid;
        _ = nLoc;
        _ = nText;
        _ = pConfig;
        _ = pLoc;
        _ = pScan;
        _ = pText;
        _ = pVal;
        _ = rc;
        _ = rc2;
        buf = new TFts5Buffer();
        pConfig = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig;
        pScan = (uintptr)0;
        libc.Xmemset(tls, bp, 0, (uint64)16);
        (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FpStorage = p;
        rc = _sqlite3Fts5StorageDeleteAll(tls, p);
        if (rc == SQLITE_OK) {
            rc = _fts5StorageLoadTotals(tls, p, (int32)1);
        }
        if (rc == SQLITE_OK) {
            rc = _fts5StorageGetStmt(tls, p, (int32)FTS5_STMT_SCAN, @unsafe.Pointer.FromBox(ᏑpScan), (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FpzErrmsg);
        }
        while (rc == SQLITE_OK && (int32)SQLITE_ROW == Xsqlite3_step(tls, pScan)) {
            iRowid = Xsqlite3_column_int64(tls, pScan, 0);
            _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡbuf));
            rc = _sqlite3Fts5IndexBeginWrite(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpIndex, 0, iRowid);
            (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol = 0;
            for (;;) {
                if (!(rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol < (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol)) {
                    break;
                }
                (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FszCol = 0;
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FabUnindexed + (uintptr)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol))) == 0) {
                    nText = 0;
                    pText = (uintptr)0;
                    nLoc = 0;
                    pLoc = (uintptr)0;
                    pVal = Xsqlite3_column_value(tls, pScan, (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + (int32)1);
                    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == (int32)FTS5_CONTENT_EXTERNAL && _sqlite3Fts5IsLocaleValue(tls, pConfig, pVal) != 0) {
                        rc = _sqlite3Fts5DecodeLocaleValue(tls, pVal, @unsafe.Pointer.FromBox(ᏑpText), @unsafe.Pointer.FromPinnedBox(ᏑnText), @unsafe.Pointer.FromBox(ᏑpLoc), @unsafe.Pointer.FromPinnedBox(ᏑnLoc));
                    } else {
                        pText = Xsqlite3_value_text(tls, pVal);
                        nText = Xsqlite3_value_bytes(tls, pVal);
                        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbLocale != 0) {
                            iCol = (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + (int32)1 + (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol;
                            pLoc = Xsqlite3_column_text(tls, pScan, iCol);
                            nLoc = Xsqlite3_column_bytes(tls, pScan, iCol);
                        }
                    }
                    if (rc == SQLITE_OK) {
                        _sqlite3Fts5SetLocale(ref (tls).DerefOrNull(), pConfig, pLoc, nLoc);
                        rc = _sqlite3Fts5Tokenize(tls, pConfig, (int32)FTS5_TOKENIZE_DOCUMENT, pText, nText, bp, __ccgo_fp(_fts5StorageInsertCallback));
                        _sqlite3Fts5ClearLocale(ref (tls).DerefOrNull(), pConfig);
                    }
                }
                _sqlite3Fts5BufferAppendVarint(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), @unsafe.Pointer.FromPinnedBox(Ꮡbuf), (int64)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FszCol);
                (@unsafe.Pointer.To<Ti64>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FaTotalSize + (uintptr)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol * 8))).Value += (int64)(@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FszCol;
                goto _1;
_1:;
                (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol = (@unsafe.Pointer.To<TFts5InsertCtx>(__ccgo_ptr(bp))).Value.FiCol + 1;
            }
            (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FnTotalRow = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FnTotalRow + 1;
            if (rc == SQLITE_OK) {
                rc = _fts5StorageInsertDocsize(tls, p, iRowid, @unsafe.Pointer.FromPinnedBox(Ꮡbuf));
            }
        }
        Xsqlite3_free(tls, buf.Fp);
        rc2 = Xsqlite3_reset(tls, pScan);
        if (rc == SQLITE_OK) {
            rc = rc2;
        }
        if (rc == SQLITE_OK) {
            rc = _fts5StorageSaveTotals(tls, p);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3JournalOpen(ж<libc.TLS> tls, uintptr pVfs, uintptr zName, uintptr pJfd, int32 flags, int32 nSpill) {
    uintptr p = default;
    _ = p;
    p = pJfd;
    libc.Xmemset(tls, p, 0, (uint64)80);
    if (nSpill == 0) {
        return _sqlite3OsOpen(tls, pVfs, zName, pJfd, flags, (uintptr)0);
    }
    if (nSpill > 0) {
        (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize = nSpill;
    } else {
        (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnChunkSize = libc.Int32FromUint64(libc.Uint64FromInt32(libc.Int32FromInt32(8) + libc.Int32FromInt32(MEMJOURNAL_DFLT_FILECHUNKSIZE)) - libc.Uint64FromInt64(16));
    }
    (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pJfd)).Value.FpMethods = (uintptr)Ꮡ_MemJournalMethods;
    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FnSpill = nSpill;
    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.Fflags = flags;
    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FzJournal = zName;
    (@unsafe.Pointer.To<TMemJournal>((@unsafe.Pointer)p)).Value.FpVfs = pVfs;
    return SQLITE_OK;
}

internal static uintptr /*r*/ _sqlite3KeyInfoAlloc(ж<libc.TLS> tls, uintptr db, int32 N, int32 X) {
    int32 nExtra = default;
    uintptr p = default;
    _ = nExtra;
    _ = p;
    nExtra = libc.Int32FromUint64(libc.Uint64FromInt32(N + X) * (libc.Uint64FromInt64(8) + libc.Uint64FromInt32(1)));
    if (N + X > (int32)0xffff) {
        return _sqlite3OomFault(tls, db);
    }
    p = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)(libc.UintptrFromInt32(0) + 32) + libc.Uint64FromInt32(libc.Int32FromInt32(0)) * libc.Uint64FromInt64(8) + libc.Uint64FromInt32(nExtra)));
    if (p != 0) {
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)p)).Value.FaSortFlags = p + 32 + (uintptr)(N + X) * 8;
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)p)).Value.FnKeyField = libc.Uint16FromInt32(N);
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)p)).Value.FnAllField = libc.Uint16FromInt32(N + X);
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)p)).Value.Fenc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc;
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)p)).Value.Fdb = db;
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)p)).Value.FnRef = (uint32)1;
        libc.Xmemset(tls, p + 32, 0, libc.Uint64FromInt32(nExtra));
    } else {
        return _sqlite3OomFault(tls, db);
    }
    return p;
}

internal static TLogEst /*r*/ _sqlite3LogEstFromDouble(ж<libc.TLS> tls, float64 _x) {
    TLogEst r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = _x;
        TLogEst e = default;
        Tu64 _ᴛ1 = default;
        _ = e;
        if (~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp)) <= libc.Float64FromInt32(1)) {
            r = 0; goto ᒐdone;
        }
        if (~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp)) <= libc.Float64FromInt32(2000000000)) {
            r = _sqlite3LogEst(ref (tls).DerefOrNull(), (uint64)(~@unsafe.Pointer.To<float64>(__ccgo_ptr(bp)))); goto ᒐdone;
        }
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        e = libc.Int16FromUint64((~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8)) >> (int)(libc.Int32FromInt32(52))) - (uint64)1022);
        r = (int16)((int32)e * (int32)10);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3OsRandomness(ж<libc.TLS> tls, uintptr pVfs, int32 nByte, uintptr zBufOut) {
    int32 r = default;

    if (_sqlite3Config.FiPrngSeed != 0) {
        libc.Xmemset(tls, zBufOut, 0, libc.Uint64FromInt32(nByte));
        if (nByte > libc.Int32FromInt64(4)) {
            nByte = (int32)4;
        }
        libc.Xmemcpy(tls, zBufOut, (uintptr)Ꮡ_sqlite3Config + 432, libc.Uint64FromInt32(nByte));
        return SQLITE_OK;
    } else {
        return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FxRandomness)(tls, pVfs, nByte, zBufOut);
    }
    return r;
}

internal static uintptr /*r*/ _sqlite3PExpr(ж<libc.TLS> tls, uintptr pParse, int32 op, uintptr pLeft, uintptr pRight) {
    uintptr p = default;
    _ = p;
    p = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)72);
    if (p != 0) {
        libc.Xmemset(tls, p, 0, (uint64)72);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop = libc.Uint8FromInt32(op & (int32)0xff);
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiAgg = (int16)(-(int32)1);
        _sqlite3ExprAttachSubtrees(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, p, pLeft, pRight);
        _sqlite3ExprCheckHeight(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FnHeight);
    } else {
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pLeft);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pRight);
    }
    return p;
}

internal static int32 /*r*/ _sqlite3PagerReadFileheader(ж<libc.TLS> tls, uintptr pPager, int32 N, uintptr pDest) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    libc.Xmemset(tls, pDest, 0, libc.Uint64FromInt32(N));
    if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0) {
        rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, pDest, N, 0);
        if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))))) {
            rc = SQLITE_OK;
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3PagerSetPagesize(ж<libc.TLS> tls, uintptr pPager, @unsafe.Pointer pPageSize, int32 nReserve) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pNew = default;
        Tu32 pageSize = default;
        int32 rc = default;
        Ti64 _ᴛ1 = default;
        _ = pNew;
        _ = pageSize;
        _ = rc;
        rc = SQLITE_OK;
        pageSize = ~@unsafe.Pointer.To<Tu32>(pPageSize);
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmemDb) == 0 || (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize == (uint32)0) && _sqlite3PcacheRefCount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache) == 0 && pageSize != 0 && pageSize != libc.Uint32FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize)) {
            pNew = libc.UintptrFromInt32(0);
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp))).Value = 0;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) > PAGER_OPEN && (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0) {
                rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, bp);
            }
            if (rc == SQLITE_OK) {
                pNew = _sqlite3PageMalloc(tls, libc.Int32FromUint32(pageSize + (uint32)8));
                if (!(pNew != 0)) {
                    rc = (int32)SQLITE_NOMEM;
                } else {
                    libc.Xmemset(tls, pNew + (uintptr)pageSize, 0, (uint64)8);
                }
            }
            if (rc == SQLITE_OK) {
                _pager_reset(tls, pPager);
                rc = _sqlite3PcacheSetPageSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache, libc.Int32FromUint32(pageSize));
            }
            if (rc == SQLITE_OK) {
                _sqlite3PageFree(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace);
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace = pNew;
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize = libc.Uint32FromInt64(quo((~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) + libc.Int64FromUint32(pageSize) - libc.Int64FromInt32(1)), libc.Int64FromUint32(pageSize)));
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize = libc.Int64FromUint32(pageSize);
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FlckPgno = libc.Uint32FromInt32(_sqlite3PendingByte) / pageSize + (uint32)1;
            } else {
                _sqlite3PageFree(tls, pNew);
            }
        }
        (@unsafe.Pointer.To<Tu32>(pPageSize)).Value = libc.Uint32FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize);
        if (rc == SQLITE_OK) {
            if (nReserve < 0) {
                nReserve = (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnReserve;
            }
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnReserve = (int16)nReserve;
            _pagerFixMaplimit(tls, pPager);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3PcacheOpen(ж<libc.TLS> tls, int32 szPage, int32 szExtra, int32 bPurgeable, uintptr __ccgo_fp_xStress, uintptr pStress, uintptr p) {
    libc.Xmemset(tls, p, 0, (uint64)80);
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FszPage = (int32)1;
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FszExtra = szExtra;
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FbPurgeable = libc.Uint8FromInt32(bPurgeable);
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FeCreate = (uint8)2;
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FxStress = __ccgo_fp_xStress;
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpStress = pStress;
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FszCache = (int32)100;
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FszSpill = (int32)1;
    return _sqlite3PcacheSetPageSize(tls, p, szPage);
}

internal static void _sqlite3PcacheTruncate(ж<libc.TLS> tls, uintptr pCache, TPgno pgno) {
    uintptr p = default;
    uintptr pNext = default;
    uintptr pPage1 = default;
    _ = p;
    _ = pNext;
    _ = pPage1;
    if ((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache != 0) {
        p = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpDirty;
        for (;;) {
            if (!(p != 0)) {
                break;
            }
            pNext = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirtyNext;
            if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fpgno > pgno) {
                _sqlite3PcacheMakeClean(tls, p);
            }
            goto _1;
_1:;
            p = pNext;
        }
        if (pgno == (uint32)0 && (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FnRefSum != 0) {
            pPage1 = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uint32, int32, uintptr>>(_sqlite3Config.Fpcache2.FxFetch)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache, (uint32)1, 0);
            if (pPage1 != 0) {
                libc.Xmemset(tls, (@unsafe.Pointer.To<Tsqlite3_pcache_page>((@unsafe.Pointer)pPage1)).Value.FpBuf, 0, libc.Uint64FromInt32((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FszPage));
                pgno = (uint32)1;
            }
        }
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uint32>>(_sqlite3Config.Fpcache2.FxTruncate)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache, pgno + (uint32)1);
    }
}

internal static int32 /*r*/ _sqlite3ReferencesSrcList(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, uintptr pSrcList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        TWalker _ᴛ1 = new();
        TRefSrcList _ᴛ2 = default;
        libc.Xmemset(tls, bp, 0, (uint64)48);
        libc.Xmemset(tls, bp + 48, 0, (uint64)32);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxExprCallback = __ccgo_fp(_exprRefToSrcList);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback = __ccgo_fp(_selectRefEnter);
        (@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FxSelectCallback2 = __ccgo_fp(_selectRefLeave);
        (@unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp)).of(TWalker.ᏑFu)), 0))).Value = bp + 48;
        (@unsafe.Pointer.To<TRefSrcList>(__ccgo_ptr(bp + 48))).Value.Fdb = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        (@unsafe.Pointer.To<TRefSrcList>(__ccgo_ptr(bp + 48))).Value.FpRef = pSrcList;
        _sqlite3WalkExprList(tls, bp, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)));
        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft != 0) {
            _sqlite3WalkExprList(tls, bp, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft + 32)));
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_WinFunc))) != (uint32)0) {
            _sqlite3WalkExpr(tls, bp, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64))))).Value.FpFilter);
        }
        if ((@unsafe.Pointer.To<TRefSrcList>(__ccgo_ptr(bp + 48))).Value.FaiExclude != 0) {
            _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TRefSrcList>(__ccgo_ptr(bp + 48))).Value.FaiExclude);
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode) & (int32)0x01) != 0) {
            r = (int32)1; goto ᒐdone;
        } else {
            if ((@unsafe.Pointer.To<TWalker>(__ccgo_ptr(bp))).Value.FeCode != 0) {
                r = 0; goto ᒐdone;
            } else {
                r = -(int32)1; goto ᒐdone;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3SchemaClear(ж<libc.TLS> tls, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(928);
        defer(tls.Free, (nint)928, ref ᒐ);
        uintptr pElem = default;
        uintptr pSchema = default;
        uintptr pTab = default;
        uintptr v3 = default;
        THash _ᴛ1 = default;
        THash _ᴛ2 = default;
        Tsqlite3 _ᴛ3 = new();
        _ = pElem;
        _ = pSchema;
        _ = pTab;
        _ = v3;
        pSchema = p;
        libc.Xmemset(tls, bp + 48, 0, (uint64)872);
        (@unsafe.Pointer.To<THash>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FtblHash;
        (@unsafe.Pointer.To<THash>(__ccgo_ptr(bp + 24))).Value = (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FtrigHash;
        _sqlite3HashInit(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pSchema + 56));
        _sqlite3HashClear(tls, (@unsafe.Pointer)(pSchema + 32));
        pElem = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)(bp + 24))).Value.Ffirst;
        for (;;) {
            if (!(pElem != 0)) {
                break;
            }
            _sqlite3DeleteTrigger(tls, __ccgo_ptr(bp + 48), (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fdata);
            goto _1;
_1:;
            pElem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fnext;
        }
        _sqlite3HashClear(tls, __ccgo_ptr(bp + 24));
        _sqlite3HashInit(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pSchema + 8));
        pElem = (@unsafe.Pointer.To<THash>((@unsafe.Pointer)bp)).Value.Ffirst;
        for (;;) {
            if (!(pElem != 0)) {
                break;
            }
            pTab = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fdata;
            _sqlite3DeleteTable(tls, __ccgo_ptr(bp + 48), pTab);
            goto _2;
_2:;
            pElem = (@unsafe.Pointer.To<THashElem>((@unsafe.Pointer)pElem)).Value.Fnext;
        }
        _sqlite3HashClear(tls, __ccgo_ptr(bp));
        _sqlite3HashClear(tls, (@unsafe.Pointer)(pSchema + 80));
        (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FpSeqTab = (uintptr)0;
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FschemaFlags) & (int32)DB_SchemaLoaded) != 0) {
            (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FiGeneration = (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FiGeneration + 1;
        }
        v3 = pSchema + 114;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)) & ~(libc.Int32FromInt32(DB_SchemaLoaded) | libc.Int32FromInt32(DB_ResetWanted)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _sqlite3SrcListAppend(ж<libc.TLS> tls, uintptr pParse, uintptr pList, uintptr pTable, uintptr pDatabase) {
    uintptr db = default;
    uintptr pItem = default;
    uintptr pNew = default;
    _ = db;
    _ = pItem;
    _ = pNew;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (pList == (uintptr)0) {
        pList = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)((uint64)(libc.UintptrFromInt32(0) + 8) + libc.Uint64FromInt32(libc.Int32FromInt32(1)) * libc.Uint64FromInt64(80)));
        if (pList == (uintptr)0) {
            return (uintptr)0;
        }
        (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnAlloc = (uint32)1;
        (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnSrc = (int32)1;
        libc.Xmemset(tls, pList + 8, 0, (uint64)80);
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pList + 8))).Value.FiCursor = -(int32)1;
    } else {
        pNew = _sqlite3SrcListEnlarge(tls, pParse, pList, (int32)1, (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnSrc);
        if (pNew == (uintptr)0) {
            _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pList);
            return (uintptr)0;
        } else {
            pList = pNew;
        }
    }
    pItem = pList + 8 + (uintptr)((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pList)).Value.FnSrc - (int32)1) * 80;
    if (pDatabase != 0 && (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pDatabase)).Value.Fz == (uintptr)0) {
        pDatabase = (uintptr)0;
    }
    if (pDatabase != 0) {
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pDatabase);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pTable);
    } else {
        (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pTable);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))).Value = (uintptr)0;
    }
    return pList;
}

internal static uintptr /*r*/ _sqlite3SrcListAppendList(ж<libc.TLS> tls, uintptr pParse, uintptr p1, uintptr p2) {
    int32 nOld = default;
    uintptr pNew = default;
    uintptr v1 = default;
    _ = nOld;
    _ = pNew;
    _ = v1;
    if (p2 != 0) {
        nOld = (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p1)).Value.FnSrc;
        pNew = _sqlite3SrcListEnlarge(tls, pParse, p1, (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p2)).Value.FnSrc, nOld);
        if (pNew == (uintptr)0) {
            _sqlite3SrcListDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, p2);
        } else {
            p1 = pNew;
            libc.Xmemcpy(tls, p1 + 8 + (uintptr)nOld * 80, p2 + 8, libc.Uint64FromInt32((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)p2)).Value.FnSrc) * (uint64)80);
            v1 = p1 + 8 + 24;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(JT_LTORJ) & libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(p2 + 8))).Value.Ffg.Fjointype));
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, p2);
        }
    }
    return p1;
}

internal static int32 /*r*/ _sqlite3StrAccumEnlarge(ж<libc.TLS> tls, uintptr p, Ti64 N) {
    Ti64 szNew = default;
    uintptr zNew = default;
    uintptr zOld = default;
    uintptr v1 = default;
    _ = szNew;
    _ = zNew;
    _ = zOld;
    _ = v1;
    if ((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FaccError != 0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FmxAlloc == (uint32)0) {
        _sqlite3StrAccumSetError(tls, (@unsafe.Pointer)p, (uint8)SQLITE_TOOBIG);
        return libc.Int32FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnAlloc - (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar - (uint32)1);
    } else {
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FprintfFlags) & (int32)SQLITE_PRINTF_MALLOCED) != 0) {
            v1 = (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FzText;
        } else {
            v1 = (uintptr)0;
        }
        zOld = v1;
        szNew = libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar) + N + (int64)1;
        if (szNew + libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar) <= libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FmxAlloc)) {
            szNew = szNew + libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar);
        }
        if (szNew > libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FmxAlloc)) {
            _sqlite3_str_reset__ccgo_gp(tls, (@unsafe.Pointer)p);
            _sqlite3StrAccumSetError(tls, (@unsafe.Pointer)p, (uint8)SQLITE_TOOBIG);
            return 0;
        } else {
            (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnAlloc = libc.Uint32FromInt32((int32)szNew);
        }
        if ((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.Fdb != 0) {
            zNew = _sqlite3DbRealloc(tls, (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.Fdb, zOld, (uint64)(@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnAlloc);
        } else {
            zNew = _sqlite3Realloc(tls, zOld, (uint64)(@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnAlloc);
        }
        if (zNew != 0) {
            if (!((libc.Int32FromUint8((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FprintfFlags) & libc.Int32FromInt32(SQLITE_PRINTF_MALLOCED)) != libc.Int32FromInt32(0)) && (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar > (uint32)0) {
                libc.Xmemcpy(tls, zNew, (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FzText, (uint64)(@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar);
            }
            (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FzText = zNew;
            (@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnAlloc = libc.Uint32FromInt32(_sqlite3DbMallocSize(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.Fdb, zNew));
            v1 = p + 29;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(SQLITE_PRINTF_MALLOCED));
        } else {
            _sqlite3_str_reset__ccgo_gp(tls, (@unsafe.Pointer)p);
            _sqlite3StrAccumSetError(tls, (@unsafe.Pointer)p, (uint8)SQLITE_NOMEM);
            return 0;
        }
    }
    return (int32)N;
}

internal static void _sqlite3StrAppend64(ж<libc.TLS> tls, uintptr p, uintptr z, Ti64 N) {
    uintptr v1 = default;
    _ = v1;
    if (libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar) + N >= libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnAlloc)) {
        _enlargeAndAppend(tls, p, z, (int32)N);
    } else {
        if (N != 0) {
            v1 = p + 24;
            (@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v1)).Value = (Tu32)((int64)(~@unsafe.Pointer.To<Tu32>((@unsafe.Pointer)v1)) + N);
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FzText + (uintptr)(libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar) - N), z, libc.Uint64FromInt64(N));
        }
    }
}

internal static int32 /*r*/ _sqlite3VdbeAddDouble(ж<libc.TLS> tls, uintptr p, int32 iDest, float64 _rVal) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = _rVal;
        Ti64 _ᴛ1 = default;
        libc.Xmemcpy(tls, bp + 8, bp, (uint64)8);
        r = _sqlite3VdbeAddOp3(tls, p, (int32)OP_Real, libc.Int32FromUint64(libc.Uint64FromInt64(~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))) & libc.Uint64FromUint32(0xffffffffU)), iDest, libc.Int32FromUint64(libc.Uint64FromInt64(~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))).Rsh((int64)(libc.Int32FromInt32(32)))));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeMemCopy(ж<libc.TLS> tls, uintptr pTo, uintptr pFrom) {
    int32 rc = default;
    uintptr v1 = default;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pTo)).Value.Fflags) & (libc.Int32FromInt32(MEM_Agg) | libc.Int32FromInt32(MEM_Dyn))) != 0) {
        _vdbeMemClearExternAndSetNull(tls, pTo);
    }
    libc.Xmemcpy(tls, pTo, pFrom, (uint64)(libc.UintptrFromInt32(0) + 24));
    v1 = pTo + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(MEM_Dyn));
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pTo)).Value.Fflags) & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Blob))) != 0) {
        if (0 == (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pFrom)).Value.Fflags) & (int32)MEM_Static)) {
            v1 = pTo + 20;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(MEM_Ephem));
            rc = _sqlite3VdbeMemMakeWriteable(tls, pTo);
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3VdbeMemExpandBlob(ж<libc.TLS> tls, uintptr pMem) {
    int32 nByte = default;
    uintptr v1 = default;
    _ = nByte;
    _ = v1;
    nByte = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn + ~@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>();
    if (nByte <= 0) {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Blob) == 0) {
            return SQLITE_OK;
        }
        nByte = (int32)1;
    }
    if (_sqlite3VdbeMemGrow(tls, pMem, nByte, (int32)1) != 0) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemset(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz + (uintptr)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn, 0, libc.Uint64FromInt32(~@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>()));
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pMem + 16))).Value += (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>()).Value;
    v1 = pMem + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(MEM_Zero) | libc.Int32FromInt32(MEM_Term)));
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3VdbeMemGrow(ж<libc.TLS> tls, uintptr pMem, int32 n, int32 bPreserve) {
    uintptr v1 = default;
    _ = v1;
    if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc > 0 && bPreserve != 0 && (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz == (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc) {
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb != 0) {
            v1 = _sqlite3DbReallocOrFree(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, libc.Uint64FromInt32(n));
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc = v1;
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = v1;
        } else {
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, libc.Uint64FromInt32(n));
            if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc == (uintptr)0) {
                Xsqlite3_free(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz);
            }
            (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc;
        }
        bPreserve = 0;
    } else {
        if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc > 0) {
            _sqlite3DbFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc);
        }
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc = _sqlite3DbMallocRaw(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, libc.Uint64FromInt32(n));
    }
    if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc == (uintptr)0) {
        _sqlite3VdbeMemSetNull(tls, pMem);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = (uintptr)0;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc = 0;
        return (int32)SQLITE_NOMEM;
    } else {
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FszMalloc = _sqlite3DbMallocSize(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc);
    }
    if (bPreserve != 0 && (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz != 0) {
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn));
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Dyn) != 0) {
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FxDel)(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz);
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FzMalloc;
    v1 = pMem + 20;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(MEM_Dyn) | libc.Int32FromInt32(MEM_Ephem) | libc.Int32FromInt32(MEM_Static)));
    return SQLITE_OK;
}

internal static void _sqlite3VdbeMemShallowCopy(ж<libc.TLS> tls, uintptr pTo, uintptr pFrom, int32 srcType) {
    uintptr v1 = default;
    _ = v1;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pTo)).Value.Fflags) & (libc.Int32FromInt32(MEM_Agg) | libc.Int32FromInt32(MEM_Dyn))) != 0) {
        _vdbeClrCopy(tls, pTo, pFrom, srcType);
        return;
    }
    libc.Xmemcpy(tls, pTo, pFrom, (uint64)(libc.UintptrFromInt32(0) + 24));
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pFrom)).Value.Fflags) & (int32)MEM_Static) == 0) {
        v1 = pTo + 20;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(MEM_Dyn) | libc.Int32FromInt32(MEM_Static) | libc.Int32FromInt32(MEM_Ephem)));
        v1 = pTo + 20;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | srcType);
    }
}

internal static void _sqlite3VdbePreUpdateHook(ж<libc.TLS> tls, uintptr v, uintptr pCsr, int32 op, uintptr zDb, uintptr pTab, Ti64 iKey1, int32 iReg, int32 iBlobWrite) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(208);
        defer(tls.Free, (nint)208, ref ᒐ);
        uintptr db = default;
        uintptr zTbl = default;
        int32 i = default;
        int32 i1 = default;
        Ti64 iKey2 = default;
        Ti64 v1 = default;
        TPreUpdate _ᴛ1 = new();
        _ = db;
        _ = i;
        _ = i1;
        _ = iKey2;
        _ = zTbl;
        _ = v1;
        db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fdb;
        zTbl = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName;
        libc.Xmemset(tls, bp, 0, (uint64)200);
        if (libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) == 0) {
            v1 = libc.Int64FromInt32(0);
            iKey2 = v1;
            iKey1 = v1;
            (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
        } else {
            if (op == (int32)SQLITE_UPDATE) {
                iKey2 = ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FaMem + (uintptr)iReg * 56));
            } else {
                iKey2 = iKey1;
            }
        }
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.Fv = v;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpCsr = pCsr;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.Fop = op;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FiNewReg = iReg;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo = bp + 168;
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo)).Value.Fdb = db;
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo)).Value.Fenc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc;
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo)).Value.FnKeyField = libc.Uint16FromInt16((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol);
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo)).Value.FaSortFlags = (uintptr)0;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FiKey1 = iKey1;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FiKey2 = iKey2;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpTab = pTab;
        (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FiBlobWrite = iBlobWrite;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpPreUpdate = bp;
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, int32, uintptr, uintptr, Tsqlite3_int64, Tsqlite3_int64>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxPreUpdateCallback)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpPreUpdateArg, db, op, zDb, zTbl, iKey1, iKey2);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpPreUpdate = (uintptr)0;
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FaRecord);
        _vdbeFreeUnpacked(tls, db, libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo)).Value.FnKeyField) + (int32)1, (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpUnpacked);
        _vdbeFreeUnpacked(tls, db, libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpKeyinfo)).Value.FnKeyField) + (int32)1, (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FpNewUnpacked);
        _sqlite3VdbeMemRelease(tls, bp + 80);
        if ((@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FaNew != 0) {
            i = 0;
            for (;;) {
                if (!(i < (int32)(@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCsr)).Value.FnField)) {
                    break;
                }
                _sqlite3VdbeMemRelease(tls, (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FaNew + (uintptr)i * 56);
                goto _2;
_2:;
                i = i + 1;
            }
            _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FaNew);
        }
        if ((@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FapDflt != 0) {
            i1 = 0;
            for (;;) {
                if (!(i1 < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                    break;
                }
                _sqlite3ValueFree(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FapDflt + (uintptr)i1 * 8)));
                goto _3;
_3:;
                i1 = i1 + 1;
            }
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TPreUpdate>(__ccgo_ptr(bp))).Value.FapDflt);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3VdbeSorterInit(ж<libc.TLS> tls, uintptr db, int32 nField, uintptr pCsr) {
    int32 i = default;
    int32 nWorker = default;
    int32 pgsz = default;
    int32 rc = default;
    int32 szKeyInfo = default;
    int32 v2 = default;
    Ti64 mxCache = default;
    Ti64 sz = default;
    uintptr pBt = default;
    uintptr pKeyInfo = default;
    uintptr pSorter = default;
    uintptr pTask = default;
    uintptr v1 = default;
    Tu32 szPma = default;
    int64 v4 = default;
    _ = i;
    _ = mxCache;
    _ = nWorker;
    _ = pBt;
    _ = pKeyInfo;
    _ = pSorter;
    _ = pTask;
    _ = pgsz;
    _ = rc;
    _ = sz;
    _ = szKeyInfo;
    _ = szPma;
    _ = v1;
    _ = v2;
    _ = v4;
    rc = SQLITE_OK;
    if (_sqlite3TempInMemory(ref (tls).DerefOrNull(), db) != 0 || libc.Int32FromUint8(_sqlite3Config.FbCoreMutex) == 0) {
        nWorker = 0;
    } else {
        nWorker = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(db + 136 + 11 * 4));
    }
    szKeyInfo = libc.Int32FromUint64((uint64)(libc.UintptrFromInt32(0) + 32) + (uint64)(@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCsr)).Value.FpKeyInfo)).Value.FnAllField * libc.Uint64FromInt64(8));
    sz = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 96) + libc.Uint64FromInt32(nWorker + libc.Int32FromInt32(1)) * libc.Uint64FromInt64(104));
    pSorter = _sqlite3DbMallocZero(tls, db, libc.Uint64FromInt64(sz + (int64)szKeyInfo));
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCsr + 48))).Value = pSorter;
    if (pSorter == (uintptr)0) {
        rc = (int32)SQLITE_NOMEM;
    } else {
        pBt = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb))).Value.FpBt;
        v1 = pSorter + (uintptr)sz;
        pKeyInfo = v1;
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FpKeyInfo = v1;
        libc.Xmemcpy(tls, pKeyInfo, (@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCsr)).Value.FpKeyInfo, libc.Uint64FromInt32(szKeyInfo));
        (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.Fdb = (uintptr)0;
        if (nField != 0 && nWorker == 0) {
            (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FnKeyField = libc.Uint16FromInt32(nField);
        }
        _sqlite3BtreeEnter(tls, pBt);
        v2 = _sqlite3BtreeGetPageSize(ref (tls).DerefOrNull(), pBt);
        pgsz = v2;
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Fpgsz = v2;
        _sqlite3BtreeLeave(tls, pBt);
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnTask = libc.Uint8FromInt32(nWorker + (int32)1);
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FiPrev = libc.Uint8FromInt32(nWorker - libc.Int32FromInt32(1));
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FbUseThreads = libc.BoolUint8(libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnTask) > libc.Int32FromInt32(1));
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Fdb = db;
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnTask))) {
                break;
            }
            pTask = pSorter + 96 + (uintptr)i * 104;
            (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter = pSorter;
            goto _3;
_3:;
            i = i + 1;
        }
        if (!(_sqlite3TempInMemory(ref (tls).DerefOrNull(), db) != 0)) {
            szPma = _sqlite3Config.FszPma;
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmnPmaSize = libc.Int32FromUint32(szPma * libc.Uint32FromInt32(pgsz));
            mxCache = (int64)(@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb))).Value.FpSchema)).Value.Fcache_size;
            if (mxCache < 0) {
                mxCache = mxCache * (int64)(-(int32)1024);
            } else {
                mxCache = mxCache * (int64)pgsz;
            }
            if (mxCache < (int64)(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(29))))) {
                v4 = mxCache;
            } else {
                v4 = (int64)(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(29))));
            }
            mxCache = v4;
            if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmnPmaSize > (int32)mxCache) {
                v2 = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmnPmaSize;
            } else {
                v2 = (int32)mxCache;
            }
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxPmaSize = v2;
            if (libc.Int32FromUint8(_sqlite3Config.FbSmallMalloc) == 0) {
                (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnMemory = pgsz;
                (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory = _sqlite3Malloc(tls, libc.Uint64FromInt32(pgsz));
                if (!((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory != 0)) {
                    rc = (int32)SQLITE_NOMEM;
                }
            }
        }
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FnAllField) < (int32)13 && (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pKeyInfo + 32)) == (uintptr)0 || ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pKeyInfo + 32)) == (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDfltColl) && (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FaSortFlags))) & (int32)KEYINFO_ORDER_BIGNULL) == 0) {
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FtypeMask = libc.Uint8FromInt32(libc.Int32FromInt32(SORTER_TYPE_INTEGER) | libc.Int32FromInt32(SORTER_TYPE_TEXT) | libc.Int32FromInt32(SORTER_TYPE_REAL));
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3VdbeSorterRowkey(ж<libc.TLS> tls, uintptr pCsr, uintptr pOut) {
    ref var nKey = ref heap(new int32(), out var ᏑnKey);
    uintptr pKey = default;
    uintptr pSorter = default;
    _ = nKey;
    _ = pKey;
    _ = pSorter;
    pSorter = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCsr + 48));
    pKey = _vdbeSorterRowkey(ref (tls).DerefOrNull(), pSorter, @unsafe.Pointer.FromPinnedBox(ᏑnKey));
    if (_sqlite3VdbeMemClearAndResize(tls, pOut, nKey) != 0) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pOut)).Value.Fn = nKey;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pOut)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pOut)).Value.Fflags) & ~(libc.Int32FromInt32(MEM_TypeMask) | libc.Int32FromInt32(MEM_Zero)) | (int32)MEM_Blob);
    libc.Xmemcpy(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pOut)).Value.Fz, pKey, libc.Uint64FromInt32(nKey));
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3VdbeSorterWrite(ж<libc.TLS> tls, uintptr pCsr, uintptr pVal) {
    uintptr aNew = default;
    uintptr pNew = default;
    uintptr pSorter = default;
    uintptr v1 = default;
    int32 bFlush = default;
    int32 iListOff = default;
    int32 nMin = default;
    int32 rc = default;
    ref var t = ref heap(new int32(), out var Ꮡt);
    Tsqlite3_int64 nNew = default;
    Ti64 nPMA = default;
    Ti64 nReq = default;
    _ = aNew;
    _ = bFlush;
    _ = iListOff;
    _ = nMin;
    _ = nNew;
    _ = nPMA;
    _ = nReq;
    _ = pNew;
    _ = pSorter;
    _ = rc;
    _ = t;
    _ = v1;
    rc = SQLITE_OK;
    pSorter = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCsr + 48));
    t = libc.Int32FromUint32((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fz + 1))));
    if (t >= (int32)0x80) {
        _sqlite3GetVarint32(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fz + 1, @unsafe.Pointer.FromPinnedBox(Ꮡt));
    }
    if (t > 0 && t < (int32)10) {
        if (t == (int32)7) {
            v1 = pSorter + 92;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & libc.Int32FromInt32(SORTER_TYPE_REAL));
        } else {
            v1 = pSorter + 92;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & (libc.Int32FromInt32(SORTER_TYPE_INTEGER) | libc.Int32FromInt32(SORTER_TYPE_REAL)));
        }
    } else {
        if (t > (int32)10 && (t & (int32)0x01) != 0) {
            v1 = pSorter + 92;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & libc.Int32FromInt32(SORTER_TYPE_TEXT));
        } else {
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FtypeMask = (uint8)0;
        }
    }
    nReq = libc.Int64FromUint64(libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fn) + (uint64)16);
    nPMA = (int64)((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fn + _sqlite3VarintLen(ref (tls).DerefOrNull(), libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fn)));
    if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxPmaSize != 0) {
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory != 0) {
            bFlush = libc.BoolInt32((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FiMemory != 0 && (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FiMemory + nReq > (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxPmaSize);
        } else {
            bFlush = libc.BoolInt32((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FszPMA > (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxPmaSize || (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FszPMA > (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmnPmaSize && _sqlite3HeapNearlyFull(ref (tls).DerefOrNull()) != 0);
        }
        if (bFlush != 0) {
            rc = _vdbeSorterFlushPMA(tls, pSorter);
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FszPMA = 0;
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FiMemory = 0;
        }
    }
    (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FszPMA += nPMA;
    if (nPMA > (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxKeysize) {
        (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxKeysize = (int32)nPMA;
    }
    if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory != 0) {
        nMin = (int32)((int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FiMemory + nReq);
        if (nMin > (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnMemory) {
            nNew = (int64)2 * (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnMemory;
            iListOff = -(int32)1;
            if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList != 0) {
                iListOff = (int32)((int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList - (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory);
            }
            while (nNew < (int64)nMin) {
                nNew = nNew * (int64)2;
            }
            if (nNew > (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxPmaSize) {
                nNew = (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FmxPmaSize;
            }
            if (nNew < (int64)nMin) {
                nNew = (int64)nMin;
            }
            aNew = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory, libc.Uint64FromInt64(nNew));
            if (!(aNew != 0)) {
                return (int32)SQLITE_NOMEM;
            }
            if (iListOff >= 0) {
                (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList = aNew + (uintptr)iListOff;
            }
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory = aNew;
            (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FnMemory = (int32)nNew;
        }
        pNew = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory + (uintptr)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.FiMemory;
        v1 = pSorter + 80;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = (int32)((int64)(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)) + (Ti64)((nReq + libc.Int64FromInt32(7)) & (int64)(~libc.Int32FromInt32(7))));
        if ((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList != 0) {
            (@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)pNew).of(TSorterRecord.ᏑFu).Reinterpret<TSorterRecordᴛ1_Fu, int32>()).Value = (int32)((int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList - (int64)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FaMemory);
        }
    } else {
        pNew = _sqlite3Malloc(tls, libc.Uint64FromInt64(nReq));
        if (pNew == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pNew + 8))).Value = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList;
    }
    libc.Xmemcpy(tls, pNew + libc.UintptrFromInt32(1) * 16, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fz, libc.Uint64FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fn));
    (@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)pNew)).Value.FnVal = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pVal)).Value.Fn;
    (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)pSorter)).Value.Flist.FpList = pNew;
    return rc;
}

internal static uintptr /*r*/ _sqlite3VtabCreateModule(ж<libc.TLS> tls, uintptr db, uintptr zName, uintptr pModule, uintptr pAux, uintptr __ccgo_fp_xDestroy) {
    int32 nName = default;
    uintptr pDel = default;
    uintptr pMod = default;
    uintptr zCopy = default;
    _ = nName;
    _ = pDel;
    _ = pMod;
    _ = zCopy;
    if (pModule == (uintptr)0) {
        zCopy = zName;
        pMod = (uintptr)0;
    } else {
        nName = _sqlite3Strlen30(tls, zName);
        pMod = _sqlite3Malloc(tls, (uint64)((uint64)48 + libc.Uint64FromInt32(nName) + (uint64)1));
        if (pMod == (uintptr)0) {
            _sqlite3OomFault(tls, db);
            return (uintptr)0;
        }
        zCopy = pMod + 1 * 48;
        libc.Xmemcpy(tls, zCopy, zName, libc.Uint64FromInt32(nName + (int32)1));
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FzName = zCopy;
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpModule = pModule;
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpAux = pAux;
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FxDestroy = __ccgo_fp_xDestroy;
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FpEpoTab = (uintptr)0;
        (@unsafe.Pointer.To<TModule>((@unsafe.Pointer)pMod)).Value.FnRefModule = (int32)1;
    }
    pDel = _sqlite3HashInsert(tls, db + 584, zCopy, pMod);
    if (pDel != 0) {
        if (pDel == pMod) {
            _sqlite3OomFault(tls, db);
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pDel);
            pMod = (uintptr)0;
        } else {
            _sqlite3VtabEponymousTableClear(tls, db, pDel);
            _sqlite3VtabModuleUnref(tls, db, pDel);
        }
    }
    return pMod;
}

internal static uintptr /*r*/ _sqlite3VtabOverloadFunction(ж<libc.TLS> tls, uintptr db, uintptr pDef, int32 nArg, uintptr pExpr) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pMod = default;
        uintptr pNew = default;
        uintptr pTab = default;
        uintptr pVtab = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        uintptr _ᴛ2 = default;
        _ = pMod;
        _ = pNew;
        _ = pTab;
        _ = pVtab;
        _ = rc;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))).Value = (uintptr)0;
        rc = 0;
        if (pExpr == (uintptr)0) {
            r = pDef; goto ᒐdone;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_COLUMN) {
            r = pDef; goto ᒐdone;
        }
        pTab = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 64));
        if (pTab == (uintptr)0) {
            r = pDef; goto ᒐdone;
        }
        if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB))) {
            r = pDef; goto ᒐdone;
        }
        pVtab = (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)_sqlite3GetVTable(ref (tls).DerefOrNull(), db, pTab))).Value.FpVtab;
        pMod = (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)pVtab)).Value.FpModule;
        if ((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pMod)).Value.FxFindFunction == (uintptr)0) {
            r = pDef; goto ᒐdone;
        }
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, uintptr, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pMod)).Value.FxFindFunction)(tls, pVtab, nArg, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FzName, bp, bp + 8);
        if (rc == 0) {
            r = pDef; goto ᒐdone;
        }
        pNew = _sqlite3DbMallocZero(tls, db, (uint64)((uint64)72 + libc.Uint64FromInt32(_sqlite3Strlen30(tls, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FzName)) + (uint64)1));
        if (pNew == (uintptr)0) {
            r = pDef; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TFuncDef>(__ccgo_ptr(pNew))).Value = (@unsafe.Pointer.To<TFuncDef>(__ccgo_ptr(pDef))).Value;
        (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pNew)).Value.FzName = pNew + 1 * 72;
        libc.Xmemcpy(tls, pNew + 1 * 72, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FzName, libc.Uint64FromInt32(_sqlite3Strlen30(tls, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FzName) + (int32)1));
        (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pNew)).Value.FxSFunc = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp));
        (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pNew)).Value.FpUserData = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8));
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 4))).Value |= (uint32)SQLITE_FUNC_EPHEM;
        r = pNew;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3WalReadFrame(ж<libc.TLS> tls, uintptr pWal, Tu32 iRead, int32 nOut, uintptr pOut) {
    Ti64 iOffset = default;
    int32 sz = default;
    _ = iOffset;
    _ = sz;
    sz = libc.Int32FromUint16((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage);
    sz = (sz & (int32)0xfe00) + ((sz & (int32)0x0001) << (int)((int32)16));
    if (nOut > sz) {
        libc.Xmemset(tls, pOut + (uintptr)sz, 0, libc.Uint64FromInt32(nOut - sz));
        nOut = sz;
    }
    iOffset = (int64)WAL_HDRSIZE + libc.Int64FromUint32(iRead - libc.Uint32FromInt32(1)) * (int64)(sz + libc.Int32FromInt32(WAL_FRAME_HDRSIZE)) + (int64)WAL_FRAME_HDRSIZE;
    return _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, pOut, nOut, iOffset);
}

internal static int32 /*r*/ _sqlite3WhereOkOnePass(ж<libc.TLS> tls, uintptr pWInfo, uintptr aiCur) {
    libc.Xmemcpy(tls, aiCur, pWInfo + 40, libc.Uint64FromInt64(4) * libc.Uint64FromInt32(2));
    return libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeOnePass);
}

internal static void _sqlite3WindowCodeStep(ж<libc.TLS> tls, uintptr pParse, uintptr p, uintptr pWInfo, int32 regGosub, int32 addrGosub) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        int32 addr = default;
        int32 addr1 = default;
        int32 addrBreak = default;
        int32 addrBreak1 = default;
        int32 addrBreak2 = default;
        int32 addrBreak3 = default;
        int32 addrEmpty = default;
        int32 addrGe = default;
        int32 addrGosubFlush = default;
        int32 addrInteger = default;
        int32 addrNe = default;
        int32 addrNext = default;
        int32 addrStart = default;
        int32 addrStart1 = default;
        int32 bRPS = default;
        int32 bRPS1 = default;
        int32 csrInput = default;
        int32 csrWrite = default;
        int32 iInput = default;
        int32 lbl = default;
        int32 lbl1 = default;
        int32 lblWhereEnd = default;
        int32 nInput = default;
        int32 nPart = default;
        int32 nPeer = default;
        int32 op = default;
        int32 regEnd = default;
        int32 regFlushPart = default;
        int32 regNew = default;
        int32 regNewPart = default;
        int32 regNewPeer = default;
        int32 regPeer = default;
        int32 regRecord = default;
        int32 regStart = default;
        int32 v1 = default;
        uintptr pKeyInfo = default;
        uintptr pMWin = default;
        uintptr pOrderBy = default;
        uintptr pPart = default;
        uintptr v = default;
        uintptr v2 = default;
        TWindowCodeArg _ᴛ1 = default;
        _ = addr;
        _ = addr1;
        _ = addrBreak;
        _ = addrBreak1;
        _ = addrBreak2;
        _ = addrBreak3;
        _ = addrEmpty;
        _ = addrGe;
        _ = addrGosubFlush;
        _ = addrInteger;
        _ = addrNe;
        _ = addrNext;
        _ = addrStart;
        _ = addrStart1;
        _ = bRPS;
        _ = bRPS1;
        _ = csrInput;
        _ = csrWrite;
        _ = iInput;
        _ = lbl;
        _ = lbl1;
        _ = lblWhereEnd;
        _ = nInput;
        _ = nPart;
        _ = nPeer;
        _ = op;
        _ = pKeyInfo;
        _ = pMWin;
        _ = pOrderBy;
        _ = pPart;
        _ = regEnd;
        _ = regFlushPart;
        _ = regNew;
        _ = regNewPart;
        _ = regNewPeer;
        _ = regPeer;
        _ = regRecord;
        _ = regStart;
        _ = v;
        _ = v1;
        _ = v2;
        pMWin = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin;
        pOrderBy = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpOrderBy;
        v = _sqlite3GetVdbe(tls, pParse);
        csrInput = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8))).Value.FiCursor;
        nInput = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8))).Value.FpSTab)).Value.FnCol;
        addrGosubFlush = 0;
        addrInteger = 0;
        regNewPeer = 0;
        regPeer = 0;
        regFlushPart = 0;
        regStart = 0;
        regEnd = 0;
        lblWhereEnd = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        libc.Xmemset(tls, bp, 0, (uint64)72);
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FpParse = pParse;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FpMWin = pMWin;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FpVdbe = v;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregGosub = regGosub;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FaddrGosub = addrGosub;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FiEphCsr;
        csrWrite = (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr + (int32)1;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fstart.Fcsr = (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr + (int32)2;
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fend.Fcsr = (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr + (int32)3;
        switch (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart)) {
        case var ᴛ when ᴛ == (int32)TK_FOLLOWING: {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) != (int32)TK_RANGE && _windowExprGtZero(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpStart) != 0) {
                (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FeDelete = (int32)WINDOW_RETURN_ROW;
            }
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_UNBOUNDED: {
            if (_windowCacheFrame(ref (tls).DerefOrNull(), pMWin) == 0) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) == (int32)TK_PRECEDING) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) != (int32)TK_RANGE && _windowExprGtZero(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpEnd) != 0) {
                        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FeDelete = (int32)WINDOW_AGGSTEP;
                    }
                } else {
                    (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FeDelete = (int32)WINDOW_RETURN_ROW;
                }
            }
            break;
        }
        default: {
            (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FeDelete = (int32)WINDOW_AGGINVERSE;
            break;
            break;
        }}

        regNew = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nInput;
        v2 = pParse + 60;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        regRecord = v1;
        v2 = pParse + 60;
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregRowid = v1;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_PRECEDING || libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_FOLLOWING) {
            v2 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            regStart = v1;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) == (int32)TK_PRECEDING || libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) == (int32)TK_FOLLOWING) {
            v2 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            regEnd = v1;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) != (int32)TK_ROWS) {
            if (pOrderBy != 0) {
                v1 = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr;
            } else {
                v1 = 0;
            }
            nPeer = v1;
            regNewPeer = regNew + (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FnBufferCol;
            if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition != 0) {
                regNewPeer = regNewPeer + (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition)).Value.FnExpr;
            }
            regPeer = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nPeer;
            (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fstart.Freg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nPeer;
            (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Freg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nPeer;
            (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fend.Freg = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)1;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += nPeer;
        }
        iInput = 0;
        for (;;) {
            if (!(iInput < nInput)) {
                break;
            }
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, csrInput, iInput, regNew + iInput);
            goto _10;
_10:;
            iInput = iInput + 1;
        }
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regNew, nInput, regRecord);
        if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition != 0) {
            pPart = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition;
            nPart = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pPart)).Value.FnExpr;
            regNewPart = regNew + (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FnBufferCol;
            pKeyInfo = _sqlite3KeyInfoFromExprList(tls, pParse, pPart, 0, 0);
            v2 = pParse + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
            regFlushPart = v1;
            addr = _sqlite3VdbeAddOp3(tls, v, (int32)OP_Compare, regNewPart, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregPart, nPart);
            _sqlite3VdbeAppendP4(tls, v, pKeyInfo, -(int32)9);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Jump, addr + (int32)2, addr + (int32)4, addr + (int32)2);
            addrGosubFlush = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Gosub, regFlushPart);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regNewPart, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregPart, nPart - (int32)1);
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_NewRowid, csrWrite, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregRowid);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, csrWrite, regRecord, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregRowid);
        addrNe = _sqlite3VdbeAddOp3(tls, v, (int32)OP_Ne, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregOne, 0, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregRowid);
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregArg = _windowInitAccum(tls, pParse, pMWin);
        if (regStart != 0) {
            _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpStart, regStart);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE) {
                v1 = (int32)3;
            } else {
                v1 = 0;
            }
            _windowCheckValue(tls, pParse, regStart, 0 + v1);
        }
        if (regEnd != 0) {
            _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpEnd, regEnd);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE) {
                v1 = (int32)3;
            } else {
                v1 = 0;
            }
            _windowCheckValue(tls, pParse, regEnd, (int32)1 + v1);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) != (int32)TK_RANGE && libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) && regStart != 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_FOLLOWING) {
                v1 = (int32)OP_Ge;
            } else {
                v1 = (int32)OP_Le;
            }
            op = v1;
            addrGe = _sqlite3VdbeAddOp3(tls, v, op, regStart, 0, regEnd);
            _windowAggFinal(tls, __ccgo_ptr(bp), 0);
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr);
            _windowReturnOneRow(tls, __ccgo_ptr(bp));
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_ResetSorter, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, lblWhereEnd);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrGe);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_FOLLOWING && libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) != (int32)TK_RANGE && regEnd != 0) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Subtract, regStart, regEnd, regStart);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) != (int32)TK_UNBOUNDED) {
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fstart.Fcsr);
        }
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr);
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fend.Fcsr);
        if (regPeer != 0 && pOrderBy != 0) {
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regNewPeer, regPeer, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr - (int32)1);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regPeer, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fstart.Freg, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr - (int32)1);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regPeer, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Freg, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr - (int32)1);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_Copy, regPeer, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fend.Freg, (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOrderBy)).Value.FnExpr - (int32)1);
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, lblWhereEnd);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrNe);
        if (regPeer != 0) {
            _windowIfNewPeer(tls, pParse, pOrderBy, regNewPeer, regPeer, lblWhereEnd);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_FOLLOWING) {
            _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGSTEP, 0, 0);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) != (int32)TK_UNBOUNDED) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE) {
                    lbl = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                    addrNext = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                    _windowCodeRangeTest(tls, __ccgo_ptr(bp), (int32)OP_Ge, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr, regEnd, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fend.Fcsr, lbl);
                    _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                    _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, 0);
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, addrNext);
                    _sqlite3VdbeResolveLabel(tls, v, lbl);
                } else {
                    _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, regEnd, 0);
                    _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                }
            }
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) == (int32)TK_PRECEDING) {
                bRPS = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_PRECEDING && libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE);
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGSTEP, regEnd, 0);
                if (bRPS != 0) {
                    _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                }
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, 0);
                if (!(bRPS != 0)) {
                    _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                }
            } else {
                addr1 = 0;
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGSTEP, 0, 0);
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) != (int32)TK_UNBOUNDED) {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE) {
                        lbl1 = 0;
                        addr1 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                        if (regEnd != 0) {
                            lbl1 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                            _windowCodeRangeTest(tls, __ccgo_ptr(bp), (int32)OP_Ge, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr, regEnd, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fend.Fcsr, lbl1);
                        }
                        _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, 0);
                        _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                        if (regEnd != 0) {
                            _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, addr1);
                            _sqlite3VdbeResolveLabel(tls, v, lbl1);
                        }
                    } else {
                        if (regEnd != 0) {
                            addr1 = _sqlite3VdbeAddOp3(tls, v, (int32)OP_IfPos, regEnd, 0, (int32)1);
                        }
                        _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, 0);
                        _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                        if (regEnd != 0) {
                            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
                        }
                    }
                }
            }
        }
        _sqlite3VdbeResolveLabel(tls, v, lblWhereEnd);
        _sqlite3WhereEnd(tls, pWInfo);
        if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition != 0) {
            addrInteger = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regFlushPart);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrGosubFlush);
        }
        (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.FregRowid = 0;
        addrEmpty = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, csrWrite);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) == (int32)TK_PRECEDING) {
            bRPS1 = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_PRECEDING && libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE);
            _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGSTEP, regEnd, 0);
            if (bRPS1 != 0) {
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
            }
            _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, 0);
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeStart) == (int32)TK_FOLLOWING) {
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGSTEP, 0, 0);
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeFrmType) == (int32)TK_RANGE) {
                    addrStart = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                    addrBreak2 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, (int32)1);
                    addrBreak1 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, (int32)1);
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FeEnd) == (int32)TK_UNBOUNDED) {
                        addrStart = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                        addrBreak1 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, regStart, (int32)1);
                        addrBreak2 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, 0, (int32)1);
                    } else {
                        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Subtract, regStart, regEnd, regEnd);
                        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regStart);
                        addrStart = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                        addrBreak1 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, regEnd, (int32)1);
                        addrBreak2 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, (int32)1);
                    }
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, addrStart);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrBreak2);
                addrStart = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                addrBreak3 = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, (int32)1);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, addrStart);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrBreak1);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrBreak3);
            } else {
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGSTEP, 0, 0);
                addrStart1 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
                addrBreak = _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_RETURN_ROW, 0, (int32)1);
                _windowCodeOp(tls, __ccgo_ptr(bp), (int32)WINDOW_AGGINVERSE, regStart, 0);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, addrStart1);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrBreak);
            }
        }
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrEmpty);
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_ResetSorter, (@unsafe.Pointer.To<TWindowCodeArg>(__ccgo_ptr(bp))).Value.Fcurrent.Fcsr);
        if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FpPartition != 0) {
            if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregStartRowid != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, (int32)1, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregStartRowid);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pMWin)).Value.FregEndRowid);
            }
            _sqlite3VdbeChangeP1(ref (tls).DerefOrNull(), v, addrInteger, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v));
            _sqlite3VdbeAddOp1(tls, v, (int32)OP_Return, regFlushPart);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _statGetPage(ж<libc.TLS> tls, uintptr pBt, Tu32 iPg, uintptr pPg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr a = default;
        int32 pgsz = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = a;
        _ = pgsz;
        _ = rc;
        pgsz = _sqlite3BtreeGetPageSize(ref (tls).DerefOrNull(), pBt);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
        if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)pPg)).Value.FaPg == (uintptr)0) {
            (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)pPg)).Value.FaPg = Xsqlite3_malloc(tls, pgsz + (int32)DBSTAT_PAGE_PADDING_BYTES);
            if ((@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)pPg)).Value.FaPg == (uintptr)0) {
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
            libc.Xmemset(tls, (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)pPg)).Value.FaPg + (uintptr)pgsz, 0, (uint64)DBSTAT_PAGE_PADDING_BYTES);
        }
        rc = _sqlite3PagerGet(tls, _sqlite3BtreePager(ref (tls).DerefOrNull(), pBt), iPg, bp, 0);
        if (rc == SQLITE_OK) {
            a = _sqlite3PagerGetData(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TStatPage>((@unsafe.Pointer)pPg)).Value.FaPg, a, libc.Uint64FromInt32(pgsz));
            _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _valueFromValueList(ж<libc.TLS> tls, uintptr pVal, uintptr ppOut, int32 bNext) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        ref var dummy = ref heap(new int32(), out var Ꮡdummy);
        int32 iOff = default;
        int32 rc = default;
        int32 v1 = default;
        ref var iSerial = ref heap(new Tu32(), out var ᏑiSerial);
        Tu32 sz = default;
        uintptr pOut = default;
        uintptr pRhs = default;
        uintptr zBuf = default;
        TMem _ᴛ1 = new();
        _ = dummy;
        _ = iOff;
        _ = iSerial;
        _ = pOut;
        _ = pRhs;
        _ = rc;
        _ = sz;
        _ = zBuf;
        _ = v1;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (uintptr)0;
        if (pVal == (uintptr)0) {
            r = _sqlite3MisuseError(tls, (int32)94771); goto ᒐdone;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (int32)MEM_Dyn) == 0 || (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.FxDel != __ccgo_fp(_sqlite3VdbeValueListFree)) {
            r = (int32)SQLITE_ERROR; goto ᒐdone;
        } else {
            pRhs = (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fz;
        }
        if (bNext != 0) {
            rc = _sqlite3BtreeNext(tls, (@unsafe.Pointer.To<TValueList>((@unsafe.Pointer)pRhs)).Value.FpCsr, 0);
        } else {
            dummy = 0;
            rc = _sqlite3BtreeFirst(tls, (@unsafe.Pointer.To<TValueList>((@unsafe.Pointer)pRhs)).Value.FpCsr, @unsafe.Pointer.FromPinnedBox(Ꮡdummy));
            if (_sqlite3BtreeEof(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TValueList>((@unsafe.Pointer)pRhs)).Value.FpCsr) != 0) {
                rc = (int32)SQLITE_DONE;
            }
        }
        if (rc == SQLITE_OK) {
            libc.Xmemset(tls, bp, 0, (uint64)56);
            sz = _sqlite3BtreePayloadSize(tls, (@unsafe.Pointer.To<TValueList>((@unsafe.Pointer)pRhs)).Value.FpCsr);
            rc = _sqlite3VdbeMemFromBtreeZeroOffset(tls, (@unsafe.Pointer.To<TValueList>((@unsafe.Pointer)pRhs)).Value.FpCsr, sz, bp);
            if (rc == SQLITE_OK) {
                zBuf = (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fz;
                pOut = (@unsafe.Pointer.To<TValueList>((@unsafe.Pointer)pRhs)).Value.FpOut;
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zBuf + 1))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
                    iSerial = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zBuf + 1)));
                    v1 = libc.Int32FromInt32(1);
                } else {
                    v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref (tls).DerefOrNull(), zBuf + 1, @unsafe.Pointer.FromPinnedBox(ᏑiSerial)));
                }
                iOff = (int32)1 + libc.Int32FromUint8(libc.Uint8FromInt32(v1));
                _sqlite3VdbeSerialGet(tls, zBuf + (uintptr)iOff, iSerial, pOut);
                (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pOut)).Value.Fenc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pOut)).Value.Fdb)).Value.Fenc;
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pOut)).Value.Fflags) & (int32)MEM_Ephem) != 0 && _sqlite3VdbeMemMakeWriteable(tls, pOut) != 0) {
                    rc = (int32)SQLITE_NOMEM;
                } else {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = pOut;
                }
            }
            _sqlite3VdbeMemRelease(tls, bp);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _vdbeIndexKeyCompare(ж<libc.TLS> tls, uintptr pCsr, uintptr pMem, @unsafe.Pointer pRc) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        Tu32 nKey = default;
        int32 ret = default;
        TMem _ᴛ1 = new();
        _ = nKey;
        _ = ret;
        ret = 0;
        nKey = (uint32)0;
        nKey = _sqlite3BtreePayloadSize(tls, pCsr);
        if (nKey == libc.Uint32FromInt32((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn) && (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Blob) != 0) {
            libc.Xmemset(tls, bp, 0, (uint64)56);
            (@unsafe.Pointer.To<int32>(pRc)).Value = _sqlite3VdbeMemFromBtreeZeroOffset(tls, pCsr, nKey, bp);
            ret = libc.BoolInt32(~@unsafe.Pointer.To<int32>(pRc) != SQLITE_OK || 0 == libc.Xmemcmp(tls, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fz, (uint64)nKey));
            _sqlite3VdbeMemReleaseMalloc(tls, bp);
        }
        r = ret;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _vdbeIsMatchingIndexKey(ж<libc.TLS> tls, uintptr pCur, int32 bInt, TBitmask mask, uintptr p, @unsafe.Pointer piRes) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr aRec = default;
        ref var iSerial = ref heap(new Tu32(), out var ᏑiSerial);
        Tu32 idxHdr = default;
        Tu32 idxRec = default;
        Tu32 nRec = default;
        ref var szHdr = ref heap(new Tu32(), out var ᏑszHdr);
        int32 ii = default;
        int32 nCol = default;
        int32 nSerial = default;
        int32 rc = default;
        int32 res = default;
        int32 v1 = default;
        TMem _ᴛ1 = new();
        _ = aRec;
        _ = iSerial;
        _ = idxHdr;
        _ = idxRec;
        _ = ii;
        _ = nCol;
        _ = nRec;
        _ = nSerial;
        _ = rc;
        _ = res;
        _ = szHdr;
        _ = v1;
        aRec = (uintptr)0;
        nRec = (uint32)0;
        rc = SQLITE_OK;
        libc.Xmemset(tls, bp, 0, (uint64)56);
        (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fenc = (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FpKeyInfo)).Value.Fenc;
        (@unsafe.Pointer.To<TMem>(__ccgo_ptr(bp))).Value.Fdb = (@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FpKeyInfo)).Value.Fdb;
        nRec = _sqlite3BtreePayloadSize(tls, pCur);
        if (nRec > (uint32)0x7fffffff) {
            r = _sqlite3CorruptError(tls, (int32)93316); goto ᒐdone;
        }
        aRec = _sqlite3MallocZero(tls, (uint64)(nRec + (uint32)5));
        if (aRec == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            rc = _sqlite3BtreePayload(tls, pCur, (uint32)0, nRec, aRec);
        }
        if (rc == SQLITE_OK) {
            szHdr = (uint32)0;
            idxHdr = (uint32)0;
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
                szHdr = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec)));
                v1 = libc.Int32FromInt32(1);
            } else {
                v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref (tls).DerefOrNull(), aRec, @unsafe.Pointer.FromPinnedBox(ᏑszHdr)));
            }
            idxHdr = (uint32)libc.Uint8FromInt32(v1);
            if (szHdr > (uint32)98307) {
                rc = (int32)SQLITE_CORRUPT;
            } else {
                res = 0;
                idxRec = szHdr;
                ii = 0;
                nCol = libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FpKeyInfo)).Value.FnAllField);
                ii = 0;
                for (;;) {
                    if (!(ii < nCol && rc == SQLITE_OK)) {
                        break;
                    }
                    iSerial = (uint32)0;
                    nSerial = 0;
                    if (idxHdr >= szHdr) {
                        rc = _sqlite3CorruptError(tls, (int32)93347);
                        break;
                    }
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + (uintptr)idxHdr))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
                        iSerial = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + (uintptr)idxHdr)));
                        v1 = libc.Int32FromInt32(1);
                    } else {
                        v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref (tls).DerefOrNull(), aRec + (uintptr)idxHdr, @unsafe.Pointer.FromPinnedBox(ᏑiSerial)));
                    }
                    idxHdr = idxHdr + (uint32)libc.Uint8FromInt32(v1);
                    nSerial = libc.Int32FromUint32(_sqlite3VdbeSerialTypeLen(ref (tls).DerefOrNull(), iSerial));
                    if (idxRec + libc.Uint32FromInt32(nSerial) > nRec) {
                        rc = _sqlite3CorruptError(tls, (int32)93353);
                    } else {
                        _sqlite3VdbeSerialGet(tls, aRec + (uintptr)idxRec, iSerial, bp);
                        if (_vdbeSkipField(tls, mask, ii, (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FaMem + (uintptr)ii * 56, bp, bInt) == 0) {
                            res = _sqlite3MemCompare(tls, bp, (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FaMem + (uintptr)ii * 56, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)p)).Value.FpKeyInfo + 32 + (uintptr)ii * 8)));
                            if (res != 0) {
                                break;
                            }
                        }
                    }
                    idxRec = idxRec + _sqlite3VdbeSerialTypeLen(ref (tls).DerefOrNull(), iSerial);
                    goto _2;
_2:;
                    ii = ii + 1;
                }
                (@unsafe.Pointer.To<int32>(piRes)).Value = res;
            }
        }
        Xsqlite3_free(tls, aRec);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _vdbePmaReadBlob(ж<libc.TLS> tls, uintptr p, int32 nByte, @unsafe.Pointer ppOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr aNew = default;
        ref var aNext = ref heap(new uintptr(), out var ᏑaNext);
        int32 iBuf = default;
        int32 nAvail = default;
        int32 nCopy = default;
        int32 nRead = default;
        int32 nRem = default;
        int32 rc = default;
        int32 rc1 = default;
        Tsqlite3_int64 nNew = default;
        int64 v1 = default;
        _ = aNew;
        _ = aNext;
        _ = iBuf;
        _ = nAvail;
        _ = nCopy;
        _ = nNew;
        _ = nRead;
        _ = nRem;
        _ = rc;
        _ = rc1;
        _ = v1;
        if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaMap != 0) {
            (@unsafe.Pointer.To<uintptr>(ppOut)).Value = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaMap + (uintptr)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff;
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p))).Value += (int64)nByte;
            r = SQLITE_OK; goto ᒐdone;
        }
        iBuf = (int32)(rem((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff, (int64)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer));
        if (iBuf == 0) {
            if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiEof - (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff > (int64)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer) {
                nRead = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer;
            } else {
                nRead = (int32)((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiEof - (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff);
            }
            rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FpFd, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaBuffer, nRead, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff);
            if (rc != SQLITE_OK) {
                r = rc; goto ᒐdone;
            }
        }
        nAvail = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer - iBuf;
        if (nByte <= nAvail) {
            (@unsafe.Pointer.To<uintptr>(ppOut)).Value = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaBuffer + (uintptr)iBuf;
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p))).Value += (int64)nByte;
        } else {
            if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnAlloc < nByte) {
                if ((int64)libc.Int32FromInt32(128) > (int64)2 * (int64)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnAlloc) {
                    v1 = (int64)libc.Int32FromInt32(128);
                } else {
                    v1 = (int64)2 * (int64)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnAlloc;
                }
                nNew = v1;
                while ((int64)nByte > nNew) {
                    nNew = nNew * (int64)2;
                }
                aNew = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaAlloc, libc.Uint64FromInt64(nNew));
                if (!(aNew != 0)) {
                    r = (int32)SQLITE_NOMEM; goto ᒐdone;
                }
                (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnAlloc = (int32)nNew;
                (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaAlloc = aNew;
            }
            libc.Xmemcpy(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaAlloc, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaBuffer + (uintptr)iBuf, libc.Uint64FromInt32(nAvail));
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p))).Value += (int64)nAvail;
            nRem = nByte - nAvail;
            while (nRem > 0) {
                aNext = (uintptr)0;
                nCopy = nRem;
                if (nRem > (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer) {
                    nCopy = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer;
                }
                rc1 = _vdbePmaReadBlob(tls, p, nCopy, @unsafe.Pointer.FromBox(ᏑaNext));
                if (rc1 != SQLITE_OK) {
                    r = rc1; goto ᒐdone;
                }
                libc.Xmemcpy(tls, (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaAlloc + (uintptr)(nByte - nRem), aNext, libc.Uint64FromInt32(nCopy));
                nRem = nRem - nCopy;
            }
            (@unsafe.Pointer.To<uintptr>(ppOut)).Value = (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaAlloc;
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _vdbePmaWriteBlob(ж<libc.TLS> tls, uintptr p, uintptr pData, int32 nData) {
    int32 nCopy = default;
    int32 nRem = default;
    int32 v1 = default;
    _ = nCopy;
    _ = nRem;
    _ = v1;
    nRem = nData;
    while (nRem > 0 && (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FeFWErr == 0) {
        nCopy = nRem;
        if (nCopy > (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FnBuffer - (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd) {
            nCopy = (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FnBuffer - (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd;
        }
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer + (uintptr)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd, pData + (uintptr)(nData - nRem), libc.Uint64FromInt32(nCopy));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(p + 24))).Value += nCopy;
        if ((@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd == (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FnBuffer) {
            (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FeFWErr = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FpFd, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer + (uintptr)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd - (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiWriteOff + (int64)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart);
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 48))).Value += libc.Uint64FromInt32((@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd - (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart);
            v1 = libc.Int32FromInt32(0);
            (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd = v1;
            (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart = v1;
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p + 32))).Value += (int64)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FnBuffer;
        }
        nRem = nRem - nCopy;
    }
}

internal static int32 /*r*/ _vdbePmaWriterFinish(ж<libc.TLS> tls, uintptr p, uintptr piEof, uintptr pnSpill) {
    int32 rc = default;
    _ = rc;
    if ((@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FeFWErr == 0 && (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer != 0 && (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd > (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart) {
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FeFWErr = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FpFd, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer + (uintptr)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd - (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiWriteOff + (int64)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart);
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(p + 48))).Value += libc.Uint64FromInt32((@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd - (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart);
    }
    (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(piEof))).Value = (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiWriteOff + (int64)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd;
    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pnSpill))).Value += (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FnPmaSpill;
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer);
    rc = (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FeFWErr;
    libc.Xmemset(tls, p, 0, (uint64)56);
    return rc;
}

internal static void _vdbePmaWriterInit(ж<libc.TLS> tls, uintptr pFd, uintptr p, int32 nBuf, Ti64 iStart) {
    int32 v1 = default;
    _ = v1;
    libc.Xmemset(tls, p, 0, (uint64)56);
    (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer = _sqlite3Malloc(tls, libc.Uint64FromInt32(nBuf));
    if (!((@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FaBuffer != 0)) {
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FeFWErr = (int32)SQLITE_NOMEM;
    } else {
        v1 = (int32)(rem(iStart, (int64)nBuf));
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart = v1;
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufEnd = v1;
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiWriteOff = iStart - (int64)(@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FiBufStart;
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FnBuffer = nBuf;
        (@unsafe.Pointer.To<TPmaWriter>((@unsafe.Pointer)p)).Value.FpFd = pFd;
    }
}

internal static int32 /*r*/ _vdbeSkipField(ж<libc.TLS> tls, TBitmask mask, int32 iCol, uintptr pMem1, uintptr pMem2, int32 bIntegrity) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uint64 v1 = default;
        Tu64 _ᴛ1 = default;
        Tu64 _ᴛ2 = default;
        _ = v1;
        if (iCol >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) || (TBitmask)(mask & (libc.Uint64FromInt32(1).Lsh((int64)(iCol)))) == (uint64)0) {
            r = 0; goto ᒐdone;
        }
        if (bIntegrity == 0) {
            r = (int32)1; goto ᒐdone;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fflags) & (int32)MEM_Real) != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem2)).Value.Fflags) & (int32)MEM_Real) != 0) {
            libc.Xmemcpy(tls, bp, pMem1, (uint64)8);
            libc.Xmemcpy(tls, bp + 8, pMem2, (uint64)8);
            if (~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) < ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8))) {
                v1 = ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8)) - ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp));
            } else {
                v1 = ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) - ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8));
            }
            if (v1 <= (uint64)BTREE_ULPDISTORTION) {
                r = (int32)1; goto ᒐdone;
            }
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _vdbeSorterSort(ж<libc.TLS> tls, uintptr pTask, uintptr pList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(512);
        defer(tls.Free, (nint)512, ref ᒐ);
        int32 i = default;
        int32 rc = default;
        uintptr p = default;
        uintptr pNext = default;
        uintptr v3 = default;
        array<uintptr> _ᴛ1 = new(64);
        _ = i;
        _ = p;
        _ = pNext;
        _ = rc;
        _ = v3;
        rc = _vdbeSortAllocUnpacked(tls, pTask);
        if (rc != SQLITE_OK) {
            r = rc; goto ᒐdone;
        }
        p = (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FpList;
        (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FxCompare = _vdbeSorterGetCompare(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter);
        libc.Xmemset(tls, bp, 0, (uint64)512);
        while (p != 0) {
            if ((@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FaMemory != 0) {
                if (p == (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FaMemory) {
                    pNext = (uintptr)0;
                } else {
                    pNext = (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FaMemory + (uintptr)(~@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)p).of(TSorterRecord.ᏑFu).Reinterpret<TSorterRecordᴛ1_Fu, int32>());
                }
            } else {
                pNext = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8));
            }
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8))).Value = (uintptr)0;
            i = 0;
            for (;;) {
                if (!(@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64) != 0)) {
                    break;
                }
                p = _vdbeSorterMerge(tls, pTask, p, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64));
                @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64) = (uintptr)0;
                goto _1;
_1:;
                i = i + 1;
            }
            @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64) = p;
            p = pNext;
        }
        p = (uintptr)0;
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint64(libc.Uint64FromInt64(512) / libc.Uint64FromInt64(8)))) {
                break;
            }
            if (@unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64) == (uintptr)0) {
                goto _2;
            }
            if (p != 0) {
                v3 = _vdbeSorterMerge(tls, pTask, p, @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64));
            } else {
                v3 = @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), i, 64);
            }
            p = v3;
            goto _2;
_2:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FpList = p;
        r = libc.Int32FromUint8((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpUnpacked)).Value.FerrCode);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _vdbeUnpackRecord(ж<libc.TLS> tls, uintptr pKeyInfo, int32 nKey, uintptr pKey) {
    uintptr pRet = default;
    _ = pRet;
    pRet = _sqlite3VdbeAllocUnpackedRecord(tls, pKeyInfo);
    if (pRet != 0) {
        libc.Xmemset(tls, (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pRet)).Value.FaMem, 0, (uint64)56 * libc.Uint64FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FnKeyField) + libc.Int32FromInt32(1)));
        _sqlite3VdbeRecordUnpack(tls, nKey, pKey, pRet);
    }
    return pRet;
}

internal static int32 /*r*/ _walBeginShmUnreliable(ж<libc.TLS> tls, uintptr pWal, @unsafe.Pointer pChanged) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr aData = default;
        uintptr aFrame = default;
        array<Tu32> aSaveCksum = new(2);
        int32 i = default;
        int32 rc = default;
        int32 szFrame = default;
        int32 v1 = default;
        Ti64 iOffset = default;
        ref var nTruncate = ref heap(new Tu32(), out var ᏑnTruncate);
        ref var pgno = ref heap(new Tu32(), out var Ꮡpgno);
        array<Tu8> _ᴛ1 = new(32);
        uintptr _ᴛ2 = default;
        Ti64 _ᴛ3 = default;
        _ = aData;
        _ = aFrame;
        _ = aSaveCksum;
        _ = i;
        _ = iOffset;
        _ = nTruncate;
        _ = pgno;
        _ = rc;
        _ = szFrame;
        _ = v1;
        aFrame = (uintptr)0;
        rc = _walLockShared(tls, pWal, libc.Int32FromInt32(3) + libc.Int32FromInt32(0));
        if (rc != SQLITE_OK) {
            if (rc == (int32)SQLITE_BUSY) {
                rc = -(int32)1;
            }
            goto begin_unreliable_shm_out;
        }
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock = 0;
        rc = _sqlite3OsShmMap(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd, 0, libc.Int32FromUint64(libc.Uint64FromInt64(2) * libc.Uint64FromInt32(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2)) + libc.Uint64FromInt32(HASHTABLE_NPAGE) * libc.Uint64FromInt64(4)), 0, bp + 40);
        if (rc != (libc.Int32FromInt32(SQLITE_READONLY) | libc.Int32FromInt32(5).Lsh((int64)(libc.Int32FromInt32(8))))) {
            if (rc == (int32)SQLITE_READONLY) {
                v1 = -(int32)1;
            } else {
                v1 = rc;
            }
            rc = v1;
            goto begin_unreliable_shm_out;
        }
        libc.Xmemcpy(tls, pWal + 72, _walIndexHdr(ref (tls).DerefOrNull(), pWal), (uint64)48);
        rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp);
        if (rc != SQLITE_OK) {
            goto begin_unreliable_shm_out;
        }
        if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) < (int64)WAL_HDRSIZE) {
            (@unsafe.Pointer.To<int32>(pChanged)).Value = (int32)1;
            if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame == (uint32)0) {
                v1 = SQLITE_OK;
            } else {
                v1 = -(int32)1;
            }
            rc = v1;
            goto begin_unreliable_shm_out;
        }
        rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp + 8, (int32)WAL_HDRSIZE, 0);
        if (rc != SQLITE_OK) {
            goto begin_unreliable_shm_out;
        }
        if (libc.Xmemcmp(tls, pWal + 72 + 32, bp + 8 + 16, (uint64)8) != 0) {
            rc = -(int32)1;
            goto begin_unreliable_shm_out;
        }
        szFrame = libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage + (uint32)WAL_FRAME_HDRSIZE);
        aFrame = Xsqlite3_malloc64(tls, libc.Uint64FromInt32(szFrame));
        if (aFrame == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
            goto begin_unreliable_shm_out;
        }
        aData = aFrame + 24;
        aSaveCksum[0] = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24));
        aSaveCksum[(int32)1] = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4));
        iOffset = libc.Int64FromInt32(WAL_HDRSIZE) + libc.Int64FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame + libc.Uint32FromInt32(1) - libc.Uint32FromInt32(1)) * libc.Int64FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage + libc.Uint32FromInt32(WAL_FRAME_HDRSIZE));
        for (;;) {
            if (!(iOffset + (int64)szFrame <= ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)))) {
                break;
            }
            rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, aFrame, szFrame, iOffset);
            if (rc != SQLITE_OK) {
                break;
            }
            if (!(_walDecodeFrame(tls, pWal, @unsafe.Pointer.FromPinnedBox(Ꮡpgno), @unsafe.Pointer.FromPinnedBox(ᏑnTruncate), aData, aFrame) != 0)) {
                break;
            }
            if (nTruncate != 0) {
                rc = -(int32)1;
                break;
            }
            goto _3;
_3:;
            iOffset = iOffset + (int64)szFrame;
        }
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24))).Value = aSaveCksum[0];
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4))).Value = aSaveCksum[(int32)1];
        goto begin_unreliable_shm_out;
begin_unreliable_shm_out:;
        Xsqlite3_free(tls, aFrame);
        if (rc != SQLITE_OK) {
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnWiData)) {
                    break;
                }
                Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)i * 8)));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)i * 8))).Value = (uintptr)0;
                goto _4;
_4:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FbShmUnreliable = (uint8)0;
            _sqlite3WalEndReadTransaction(tls, pWal);
            (@unsafe.Pointer.To<int32>(pChanged)).Value = (int32)1;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _walCleanupHash(ж<libc.TLS> tls, uintptr pWal) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 i = default;
        int32 iLimit = default;
        int32 nByte = default;
        TWalHashLoc _ᴛ1 = default;
        _ = i;
        _ = iLimit;
        _ = nByte;
        iLimit = 0;
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame == (uint32)0) {
            return;
        }
        i = _walHashGet(tls, pWal, _walFramePage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame), bp);
        if (i != 0) {
            return;
        }
        iLimit = libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame - (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero);
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2))) {
                break;
            }
            if (libc.Int32FromUint16(~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash + (uintptr)i * 2))) > iLimit) {
                (@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash + (uintptr)i * 2))).Value = (uint16)0;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        nByte = (int32)((int64)(@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash - (int64)((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno + (uintptr)iLimit * 4));
        libc.Xmemset(tls, (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno + (uintptr)iLimit * 4, 0, libc.Uint64FromInt32(nByte));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _walIndexAppend(ж<libc.TLS> tls, uintptr pWal, Tu32 iFrame, Tu32 iPage) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 iKey = default;
        int32 idx = default;
        int32 nByte = default;
        int32 nCollide = default;
        int32 rc = default;
        int32 v2 = default;
        TWalHashLoc _ᴛ1 = default;
        _ = iKey;
        _ = idx;
        _ = nByte;
        _ = nCollide;
        _ = rc;
        _ = v2;
        rc = _walHashGet(tls, pWal, _walFramePage(ref (tls).DerefOrNull(), iFrame), bp);
        if (rc == SQLITE_OK) {
            idx = libc.Int32FromUint32(iFrame - (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero);
            if (idx == (int32)1) {
                nByte = (int32)((int64)((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash + (uintptr)(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2)) * 2) - (int64)(@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno);
                libc.Xmemset(tls, (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno, 0, libc.Uint64FromInt32(nByte));
            }
            if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno + (uintptr)(idx - (int32)1) * 4)) != 0) {
                _walCleanupHash(tls, pWal);
            }
            nCollide = idx;
            iKey = _walHash(ref (tls).DerefOrNull(), iPage);
            for (;;) {
                if (!(~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash + (uintptr)iKey * 2)) != 0)) {
                    break;
                }
                v2 = nCollide;
                nCollide = nCollide - 1;
                if (v2 == 0) {
                    r = _sqlite3CorruptError(tls, (int32)68449); goto ᒐdone;
                }
                goto _1;
_1:;
                iKey = _walNextHash(ref (tls).DerefOrNull(), iKey);
            }
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno + (uintptr)((idx - (int32)1) & (libc.Int32FromInt32(HASHTABLE_NPAGE) - libc.Int32FromInt32(1))) * 4))).Value = iPage;
            libc.AtomicStoreNUint16((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash + (uintptr)iKey * 2, libc.Uint16FromInt32(idx), libc.Int32FromInt32(__ATOMIC_RELAXED));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _walIndexPageRealloc(ж<libc.TLS> tls, uintptr pWal, int32 iPage, uintptr ppPage) {
    uintptr apNew = default;
    uintptr v1 = default;
    Tsqlite3_int64 nByte = default;
    int32 rc = default;
    _ = apNew;
    _ = nByte;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnWiData <= iPage) {
        nByte = libc.Int64FromUint64((uint64)8 * libc.Uint64FromInt64(libc.Int64FromInt32(1) + (int64)iPage));
        apNew = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData, libc.Uint64FromInt64(nByte));
        if (!(apNew != 0)) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = (uintptr)0;
            return (int32)SQLITE_NOMEM;
        }
        libc.Xmemset(tls, apNew + (uintptr)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnWiData * 8, 0, (uint64)8 * libc.Uint64FromInt32(iPage + libc.Int32FromInt32(1) - (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnWiData));
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData = apNew;
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FnWiData = iPage + (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == (int32)WAL_HEAPMEMORY_MODE) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPage * 8))).Value = _sqlite3MallocZero(tls, (uint64)(libc.Uint64FromInt64(2) * libc.Uint64FromInt32(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2)) + libc.Uint64FromInt32(HASHTABLE_NPAGE) * libc.Uint64FromInt64(4)));
        if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPage * 8)) != 0)) {
            rc = (int32)SQLITE_NOMEM;
        }
    } else {
        rc = _sqlite3OsShmMap(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd, iPage, libc.Int32FromUint64(libc.Uint64FromInt64(2) * libc.Uint64FromInt32(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2)) + libc.Uint64FromInt32(HASHTABLE_NPAGE) * libc.Uint64FromInt64(4)), libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock), (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPage * 8);
        if (rc == SQLITE_OK) {
            if (iPage > 0 && _sqlite3FaultSim(tls, (int32)600) != 0) {
                rc = (int32)SQLITE_NOMEM;
            }
        } else {
            if ((rc & (int32)0xff) == (int32)SQLITE_READONLY) {
                v1 = pWal + 66;
                (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(WAL_SHM_RDONLY));
                if (rc == (int32)SQLITE_READONLY) {
                    rc = SQLITE_OK;
                }
            }
        }
    }
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)iPage * 8))).Value;
    return rc;
}

internal static int32 /*r*/ _walIndexTryHdr(ж<libc.TLS> tls, uintptr pWal, @unsafe.Pointer pChanged) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(96);
        defer(tls.Free, (nint)96, ref ᒐ);
        ref var aCksum = ref heap(new array<Tu32>(2), out var ᏑaCksum);
        uintptr aHdr = default;
        TWalIndexHdr _ᴛ1 = new();
        TWalIndexHdr _ᴛ2 = new();
        _ = aCksum;
        _ = aHdr;
        aHdr = _walIndexHdr(ref (tls).DerefOrNull(), pWal);
        libc.Xmemcpy(tls, bp, aHdr, (uint64)48);
        _walShmBarrier(tls, pWal);
        libc.Xmemcpy(tls, bp + 48, aHdr + 1 * 48, (uint64)48);
        if (libc.Xmemcmp(tls, bp, bp + 48, (uint64)48) != 0) {
            r = (int32)1; goto ᒐdone;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWalIndexHdr>(__ccgo_ptr(bp))).Value.FisInit) == 0) {
            r = (int32)1; goto ᒐdone;
        }
        _walChecksumBytes(ref (tls).DerefOrNull(), (int32)1, bp, libc.Int32FromUint64(libc.Uint64FromInt64(48) - libc.Uint64FromInt64(8)), (uintptr)0, @unsafe.Pointer.FromPinnedBox(ᏑaCksum));
        if (aCksum[0] != ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 40)) || aCksum[(int32)1] != ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp + 40 + 1 * 4))) {
            r = (int32)1; goto ᒐdone;
        }
        if (libc.Xmemcmp(tls, pWal + 72, bp, (uint64)48) != 0) {
            (@unsafe.Pointer.To<int32>(pChanged)).Value = (int32)1;
            libc.Xmemcpy(tls, pWal + 72, bp, (uint64)48);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage = libc.Uint32FromInt32((libc.Int32FromUint16((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage) & (int32)0xfe00) + ((libc.Int32FromUint16((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage) & (int32)0x0001) << (int)((int32)16)));
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _walIndexWriteHdr(ж<libc.TLS> tls, uintptr pWal) {
    uintptr aHdr = default;
    int32 nCksum = default;
    _ = aHdr;
    _ = nCksum;
    aHdr = _walIndexHdr(ref (tls).DerefOrNull(), pWal);
    nCksum = libc.Int32FromUint64((uint64)(libc.UintptrFromInt32(0) + 40));
    (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FisInit = (uint8)1;
    (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FiVersion = (uint32)WALINDEX_MAX_VERSION;
    _walChecksumBytes(ref (tls).DerefOrNull(), (int32)1, pWal + 72, nCksum, (uintptr)0, (@unsafe.Pointer)(pWal + 72 + 40));
    libc.Xmemcpy(tls, aHdr + 1 * 48, pWal + 72, (uint64)48);
    _walShmBarrier(tls, pWal);
    libc.Xmemcpy(tls, aHdr, pWal + 72, (uint64)48);
}

internal static int32 /*r*/ _walIteratorInit(ж<libc.TLS> tls, uintptr pWal, Tu32 nBackfill, @unsafe.Pointer pp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr aIndex = default;
        uintptr aPagemap = default;
        uintptr aTmp = default;
        uintptr p = default;
        int32 eAlg = default;
        int32 i = default;
        int32 iFirstPage = default;
        int32 iLastPage = default;
        int32 j = default;
        ref var nEntry = ref heap(new int32(), out var ᏑnEntry);
        int32 nSegment = default;
        int32 rc = default;
        Tu32 iLast = default;
        Tu32 iZero = default;
        Tu32 pgno = default;
        Tsqlite3_int64 nByte = default;
        Tsqlite3_int64 nExtra = default;
        uint64 v1 = default;
        uint32 v2 = default;
        TWalHashLoc _ᴛ1 = default;
        _ = aIndex;
        _ = aPagemap;
        _ = aTmp;
        _ = eAlg;
        _ = i;
        _ = iFirstPage;
        _ = iLast;
        _ = iLastPage;
        _ = iZero;
        _ = j;
        _ = nByte;
        _ = nEntry;
        _ = nExtra;
        _ = nSegment;
        _ = p;
        _ = pgno;
        _ = rc;
        _ = v1;
        _ = v2;
        aTmp = (uintptr)0;
        rc = SQLITE_OK;
        iLast = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame;
        iFirstPage = _walFramePage(ref (tls).DerefOrNull(), nBackfill + (uint32)1);
        iLastPage = _walFramePage(ref (tls).DerefOrNull(), iLast);
        if (iFirstPage != 0) {
            v1 = libc.Uint64FromInt32(HASHTABLE_NPAGE) - (libc.Uint64FromInt64(48) * libc.Uint64FromInt32(2) + libc.Uint64FromInt64(40)) / libc.Uint64FromInt64(4) + libc.Uint64FromInt32((iFirstPage - (int32)1) * (int32)HASHTABLE_NPAGE);
        } else {
            v1 = (uint64)0;
        }
        iZero = (uint32)v1;
        if ((uint64)(iLast - iZero) * (uint64)2 > (uint64)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FnPage * (uint64)4) {
            nSegment = (int32)1;
            eAlg = (int32)2;
            nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 16) + libc.Uint64FromInt32(nSegment) * (uint64)32 + (uint64)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FnPage * (uint64)4);
            nExtra = 0;
        } else {
            nSegment = iLastPage - iFirstPage + (int32)1;
            eAlg = (int32)1;
            nByte = libc.Int64FromUint64((uint64)(libc.UintptrFromInt32(0) + 16) + libc.Uint64FromInt32(nSegment) * (uint64)32 + (uint64)(iLast - iZero) * (uint64)2);
            if (iLast > (uint32)HASHTABLE_NPAGE) {
                v2 = (uint32)HASHTABLE_NPAGE;
            } else {
                v2 = iLast;
            }
            nExtra = libc.Int64FromUint64((uint64)2 * (uint64)v2);
        }
        p = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nByte + nExtra));
        if (!(p != 0)) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        libc.Xmemset(tls, p, 0, libc.Uint64FromInt64(nByte));
        (@unsafe.Pointer.To<TWalIterator>((@unsafe.Pointer)p)).Value.FnSegment = nSegment;
        if (eAlg == (int32)2) {
            (@unsafe.Pointer.To<TWalIterator>((@unsafe.Pointer)p)).Value.FnPagemap = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FnPage;
            (@unsafe.Pointer.To<TWalSegment>((@unsafe.Pointer)(p + 16))).Value.FaPgno = p + 16 + 1 * 32 - (uintptr)1 * 4;
        } else {
            aTmp = p + (uintptr)nByte;
        }
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpFree = p;
        i = iFirstPage;
        for (;;) {
            if (!(rc == SQLITE_OK && i <= iLastPage)) {
                break;
            }
            rc = _walHashGet(tls, pWal, i, bp);
            if (rc == SQLITE_OK) {
                if (i == iLastPage) {
                    nEntry = libc.Int32FromUint32(iLast - (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero);
                } else {
                    nEntry = (int32)(((int64)(@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaHash - (int64)(@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno) / 4);
                }
                if (eAlg == (int32)2) {
                    aPagemap = (@unsafe.Pointer.To<TWalSegment>((@unsafe.Pointer)(p + 16))).Value.FaPgno;
                    j = 0;
                    for (;;) {
                        if (!(j < nEntry)) {
                            break;
                        }
                        pgno = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno + (uintptr)j * 4));
                        if (pgno <= (@unsafe.Pointer.To<TWalIterator>((@unsafe.Pointer)p)).Value.FnPagemap) {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aPagemap + (uintptr)pgno * 4))).Value = (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero + libc.Uint32FromInt32(j) + (uint32)1;
                        }
                        goto _4;
_4:;
                        j = j + 1;
                    }
                } else {
                    aIndex = p + 16 + (uintptr)(@unsafe.Pointer.To<TWalIterator>((@unsafe.Pointer)p)).Value.FnSegment * 32 + (uintptr)((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero - iZero) * 2;
                    (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero = (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero + 1;
                    j = 0;
                    for (;;) {
                        if (!(j < nEntry)) {
                            break;
                        }
                        (@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aIndex + (uintptr)j * 2))).Value = libc.Uint16FromInt32(j);
                        goto _5;
_5:;
                        j = j + 1;
                    }
                    _walMergesort(tls, (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno, aTmp, aIndex, @unsafe.Pointer.FromPinnedBox(ᏑnEntry));
                    (@unsafe.Pointer.To<TWalSegment>((@unsafe.Pointer)(p + 16 + (uintptr)(i - iFirstPage) * 32))).Value.FiZero = libc.Int32FromUint32((@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FiZero);
                    (@unsafe.Pointer.To<TWalSegment>((@unsafe.Pointer)(p + 16 + (uintptr)(i - iFirstPage) * 32))).Value.FnEntry = nEntry;
                    (@unsafe.Pointer.To<TWalSegment>((@unsafe.Pointer)(p + 16 + (uintptr)(i - iFirstPage) * 32))).Value.FaIndex = aIndex;
                    (@unsafe.Pointer.To<TWalSegment>((@unsafe.Pointer)(p + 16 + (uintptr)(i - iFirstPage) * 32))).Value.FaPgno = (@unsafe.Pointer.To<TWalHashLoc>(__ccgo_ptr(bp))).Value.FaPgno;
                }
            }
            goto _3;
_3:;
            i = i + 1;
        }
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpFree = (uintptr)0;
            _walIteratorFree(tls, p);
            p = (uintptr)0;
        }
        (@unsafe.Pointer.To<uintptr>(pp)).Value = p;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _walMerge(ж<libc.TLS> tls, uintptr aContent, uintptr aLeft, int32 nLeft, @unsafe.Pointer paRight, @unsafe.Pointer pnRight, uintptr aTmp) {
    uintptr aRight = default;
    TPgno dbpage = default;
    int32 iLeft = default;
    int32 iOut = default;
    int32 iRight = default;
    int32 nRight = default;
    int32 v1 = default;
    Tht_slot logpage = default;
    _ = aRight;
    _ = dbpage;
    _ = iLeft;
    _ = iOut;
    _ = iRight;
    _ = logpage;
    _ = nRight;
    _ = v1;
    iLeft = 0;
    iRight = 0;
    iOut = 0;
    nRight = ~@unsafe.Pointer.To<int32>(pnRight);
    aRight = ~@unsafe.Pointer.To<uintptr>(paRight);
    while (iRight < nRight || iLeft < nLeft) {
        if (iLeft < nLeft && (iRight >= nRight || ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aContent + (uintptr)(~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aLeft + (uintptr)iLeft * 2))) * 4)) < ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aContent + (uintptr)(~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aRight + (uintptr)iRight * 2))) * 4)))) {
            v1 = iLeft;
            iLeft = iLeft + 1;
            logpage = ~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aLeft + (uintptr)v1 * 2));
        } else {
            v1 = iRight;
            iRight = iRight + 1;
            logpage = ~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aRight + (uintptr)v1 * 2));
        }
        dbpage = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aContent + (uintptr)logpage * 4));
        v1 = iOut;
        iOut = iOut + 1;
        (@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aTmp + (uintptr)v1 * 2))).Value = logpage;
        if (iLeft < nLeft && ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aContent + (uintptr)(~@unsafe.Pointer.To<Tht_slot>(__ccgo_ptr(aLeft + (uintptr)iLeft * 2))) * 4)) == dbpage) {
            iLeft = iLeft + 1;
        }
    }
    (@unsafe.Pointer.To<uintptr>(paRight)).Value = aLeft;
    (@unsafe.Pointer.To<int32>(pnRight)).Value = iOut;
    libc.Xmemcpy(tls, aLeft, aTmp, (uint64)2 * libc.Uint64FromInt32(iOut));
}

internal partial struct _walMergesort__ᴛ1 /*dyn*/ {
    public int32 FnList;
    public uintptr FaList;
}

internal static void _walMergesort(ж<libc.TLS> tls, uintptr aContent, uintptr aBuffer, uintptr aList, @unsafe.Pointer pnList) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(208);
        defer(tls.Free, (nint)208, ref ᒐ);
        ref var aMerge = ref heap(new uintptr(), out var ᏑaMerge);
        uintptr p = default;
        uintptr p1 = default;
        int32 iList = default;
        int32 nList = default;
        ref var nMerge = ref heap(new int32(), out var ᏑnMerge);
        Tu32 iSub = default;
        array<_walMergesort__ᴛ1> _ᴛ1 = new(13);
        _ = aMerge;
        _ = iList;
        _ = iSub;
        _ = nList;
        _ = nMerge;
        _ = p;
        _ = p1;
        nList = ~@unsafe.Pointer.To<int32>(pnList);
        nMerge = 0;
        aMerge = (uintptr)0;
        iSub = (uint32)0;
        libc.Xmemset(tls, bp, 0, (uint64)208);
        iList = 0;
        for (;;) {
            if (!(iList < nList)) {
                break;
            }
            nMerge = (int32)1;
            aMerge = aList + (uintptr)iList * 2;
            iSub = (uint32)0;
            for (;;) {
                if (!((iList & (((int32)1).Lsh((uint64)(iSub)))) != 0)) {
                    break;
                }
                p = bp + (uintptr)iSub * 16;
                _walMerge(tls, aContent, (@unsafe.Pointer.To<_walMergesort__ᴛ1>((@unsafe.Pointer)p)).Value.FaList, (@unsafe.Pointer.To<_walMergesort__ᴛ1>((@unsafe.Pointer)p)).Value.FnList, @unsafe.Pointer.FromBox(ᏑaMerge), @unsafe.Pointer.FromPinnedBox(ᏑnMerge), aBuffer);
                goto _2;
_2:;
                iSub = iSub + 1;
            }
            @unsafe.Pointer.ElementRef<_walMergesort__ᴛ1>(__ccgo_ptr(bp), (nint)(iSub), 13).FaList = aMerge;
            @unsafe.Pointer.ElementRef<_walMergesort__ᴛ1>(__ccgo_ptr(bp), (nint)(iSub), 13).FnList = nMerge;
            goto _1;
_1:;
            iList = iList + 1;
        }
        iSub = iSub + 1;
        for (;;) {
            if (!(iSub < libc.Uint32FromInt32(libc.Int32FromUint64(libc.Uint64FromInt64(208) / libc.Uint64FromInt64(16))))) {
                break;
            }
            if ((nList & (((int32)1).Lsh((uint64)(iSub)))) != 0) {
                p1 = bp + (uintptr)iSub * 16;
                _walMerge(tls, aContent, (@unsafe.Pointer.To<_walMergesort__ᴛ1>((@unsafe.Pointer)p1)).Value.FaList, (@unsafe.Pointer.To<_walMergesort__ᴛ1>((@unsafe.Pointer)p1)).Value.FnList, @unsafe.Pointer.FromBox(ᏑaMerge), @unsafe.Pointer.FromPinnedBox(ᏑnMerge), aBuffer);
            }
            goto _3;
_3:;
            iSub = iSub + 1;
        }
        (@unsafe.Pointer.To<int32>(pnList)).Value = nMerge;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _whereClauseInsert(ж<libc.TLS> tls, uintptr pWC, uintptr p, Tu16 wtFlags) {
    uintptr db = default;
    uintptr pOld = default;
    uintptr pTerm = default;
    uintptr v3 = default;
    int32 idx = default;
    int32 v1 = default;
    int32 v2 = default;
    _ = db;
    _ = idx;
    _ = pOld;
    _ = pTerm;
    _ = v1;
    _ = v2;
    _ = v3;
    if ((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm >= (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnSlot) {
        pOld = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo)).Value.FpParse)).Value.Fdb;
        (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa = _sqlite3WhereMalloc(tls, (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo, (uint64)((uint64)56 * libc.Uint64FromInt32((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnSlot) * (uint64)2));
        if ((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa == (uintptr)0) {
            if ((libc.Int32FromUint16(wtFlags) & (int32)TERM_DYNAMIC) != 0) {
                _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, p);
            }
            (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa = pOld;
            return 0;
        }
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa, pOld, (uint64)56 * libc.Uint64FromInt32((@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm));
        (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnSlot = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnSlot * (int32)2;
    }
    v3 = pWC + 20;
    v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
    v1 = v2;
    idx = v1;
    pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)v1 * 56;
    if ((libc.Int32FromUint16(wtFlags) & (int32)TERM_VIRTUAL) == 0) {
        (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnBase = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm;
    }
    if (p != 0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Unlikely))) != (uint32)0) {
        (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FtruthProb = (int16)((int32)_sqlite3LogEst(ref (tls).DerefOrNull(), libc.Uint64FromInt32((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiTable)) - (int32)270);
    } else {
        (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FtruthProb = (int16)1;
    }
    (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), p);
    (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags = wtFlags;
    (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpWC = pWC;
    (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FiParent = -(int32)1;
    libc.Xmemset(tls, pTerm + 20, 0, libc.Uint64FromInt64(56) - (uint64)(libc.UintptrFromInt32(0) + 20));
    return idx;
}

internal static int32 /*r*/ _whereLoopAddOr(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, TBitmask mPrereq, TBitmask mUnusable) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(656);
        defer(tls.Free, (nint)656, ref ᒐ);
        int32 i = default;
        int32 iCur = default;
        int32 j = default;
        int32 once = default;
        int32 rc = default;
        uintptr pItem = default;
        uintptr pNew = default;
        uintptr pOrTerm = default;
        uintptr pOrWC = default;
        uintptr pOrWCEnd = default;
        uintptr pTerm = default;
        uintptr pWC = default;
        uintptr pWCEnd = default;
        uintptr pWInfo = default;
        ref var sSubBuild = ref heap(new TWhereLoopBuilder(), out var ᏑsSubBuild);
        TWhereOrSet _ᴛ1 = new();
        TWhereOrSet _ᴛ2 = new();
        TWhereOrSet _ᴛ3 = new();
        TWhereClause _ᴛ4 = new();
        _ = i;
        _ = iCur;
        _ = j;
        _ = once;
        _ = pItem;
        _ = pNew;
        _ = pOrTerm;
        _ = pOrWC;
        _ = pOrWCEnd;
        _ = pTerm;
        _ = pWC;
        _ = pWCEnd;
        _ = pWInfo;
        _ = rc;
        _ = sSubBuild;
        pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
        rc = SQLITE_OK;
        pWC = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8));
        pWCEnd = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FnTerm * 56;
        pNew = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
        libc.Xmemset(tls, bp + 488, 0, (uint64)56);
        pItem = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiTab * 80;
        iCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor;
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (int32)JT_RIGHT) != 0) {
            r = SQLITE_OK; goto ᒐdone;
        }
        pTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fa;
        for (;;) {
            if (!(pTerm < pWCEnd && rc == SQLITE_OK)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_OR) != 0 && (TBitmask)((@unsafe.Pointer.To<TWhereOrInfo>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTerm + 32))))).Value.Findexable & (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf) != (uint64)0) {
                pOrWC = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pTerm + 32));
                pOrWCEnd = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWC)).Value.Fa + (uintptr)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWC)).Value.FnTerm * 56;
                once = (int32)1;
                sSubBuild = ~@unsafe.Pointer.To<TWhereLoopBuilder>(pBuilder);
                sSubBuild.FpOrSet = bp + 544;
                pOrTerm = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pOrWC)).Value.Fa;
                for (;;) {
                    if (!(pOrTerm < pOrWCEnd)) {
                        break;
                    }
                    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FeOperator) & (int32)WO_AND) != 0) {
                        sSubBuild.FpWC = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOrTerm + 32));
                    } else {
                        if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOrTerm)).Value.FleftCursor == iCur) {
                            (@unsafe.Pointer.To<TWhereClause>(__ccgo_ptr(bp))).Value.FpWInfo = (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo;
                            (@unsafe.Pointer.To<TWhereClause>(__ccgo_ptr(bp))).Value.FpOuter = pWC;
                            (@unsafe.Pointer.To<TWhereClause>(__ccgo_ptr(bp))).Value.Fop = (uint8)TK_AND;
                            (@unsafe.Pointer.To<TWhereClause>(__ccgo_ptr(bp))).Value.FnTerm = (int32)1;
                            (@unsafe.Pointer.To<TWhereClause>(__ccgo_ptr(bp))).Value.FnBase = (int32)1;
                            (@unsafe.Pointer.To<TWhereClause>(__ccgo_ptr(bp))).Value.Fa = pOrTerm;
                            sSubBuild.FpWC = bp;
                        } else {
                            goto _2;
                        }
                    }
                    (@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 544))).Value.Fn = (uint16)0;
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                        rc = _whereLoopAddVirtual(tls, @unsafe.Pointer.FromPinnedBox(ᏑsSubBuild), mPrereq, mUnusable);
                    } else {
                        rc = _whereLoopAddBtree(tls, @unsafe.Pointer.FromPinnedBox(ᏑsSubBuild), mPrereq);
                    }
                    if (rc == SQLITE_OK) {
                        rc = _whereLoopAddOr(tls, @unsafe.Pointer.FromPinnedBox(ᏑsSubBuild), mPrereq, mUnusable);
                    }
                    if (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 544))).Value.Fn) == 0) {
                        (@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 488))).Value.Fn = (uint16)0;
                        break;
                    } else {
                        if (once != 0) {
                            _whereOrMove(tls, bp + 488, bp + 544);
                            once = 0;
                        } else {
                            _whereOrMove(tls, bp + 600, bp + 488);
                            (@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 488))).Value.Fn = (uint16)0;
                            i = 0;
                            for (;;) {
                                if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 600))).Value.Fn))) {
                                    break;
                                }
                                j = 0;
                                for (;;) {
                                    if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 544))).Value.Fn))) {
                                        break;
                                    }
                                    _whereOrInsert(ref (tls).DerefOrNull(), __ccgo_ptr(bp + 488), (TBitmask)((@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 600 + 8 + (uintptr)i * 16))).Value.Fprereq | (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 544 + 8 + (uintptr)j * 16))).Value.Fprereq), _sqlite3LogEstAdd(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 600 + 8 + (uintptr)i * 16))).Value.FrRun, (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 544 + 8 + (uintptr)j * 16))).Value.FrRun), _sqlite3LogEstAdd(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 600 + 8 + (uintptr)i * 16))).Value.FnOut, (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 544 + 8 + (uintptr)j * 16))).Value.FnOut));
                                    goto _4;
_4:;
                                    j = j + 1;
                                }
                                goto _3;
_3:;
                                i = i + 1;
                            }
                        }
                    }
                    goto _2;
_2:;
                    pOrTerm += 56;
                }
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = (uint16)1;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm))).Value = pTerm;
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = (uint32)WHERE_MULTI_OR;
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = 0;
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiSortIdx = (uint8)0;
                libc.Xmemset(tls, pNew + 24, 0, (uint64)24);
                i = 0;
                for (;;) {
                    if (!(rc == SQLITE_OK && i < libc.Int32FromUint16((@unsafe.Pointer.To<TWhereOrSet>(__ccgo_ptr(bp + 488))).Value.Fn))) {
                        break;
                    }
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = (int16)((int32)(@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 488 + 8 + (uintptr)i * 16))).Value.FrRun + (int32)1);
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 488 + 8 + (uintptr)i * 16))).Value.FnOut;
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq = (@unsafe.Pointer.To<TWhereOrCost>(__ccgo_ptr(bp + 488 + 8 + (uintptr)i * 16))).Value.Fprereq;
                    rc = _whereLoopInsert(tls, pBuilder, pNew);
                    goto _5;
_5:;
                    i = i + 1;
                }
            }
            goto _1;
_1:;
            pTerm += 56;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _whereLoopResize(ж<libc.TLS> tls, uintptr db, uintptr p, int32 n) {
    uintptr paNew = default;
    _ = paNew;
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnLSlot) >= n) {
        return SQLITE_OK;
    }
    n = (n + (int32)7) & ~libc.Int32FromInt32(7);
    paNew = _sqlite3DbMallocRawNN(tls, db, (uint64)((uint64)8 * libc.Uint64FromInt32(n)));
    if (paNew == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    libc.Xmemcpy(tls, paNew, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FaLTerm, (uint64)8 * (uint64)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnLSlot);
    if ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FaLTerm != p + 80) {
        _sqlite3DbFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FaLTerm);
    }
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FaLTerm = paNew;
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnLSlot = libc.Uint16FromInt32(n);
    return SQLITE_OK;
}

internal static TBitmask /*r*/ _whereOmitNoopJoin(ж<libc.TLS> tls, uintptr pWInfo, TBitmask notReady) {
    int32 hasRightJoin = default;
    int32 i = default;
    int32 nByte = default;
    TBitmask m1 = default;
    TBitmask tabUsed = default;
    uintptr pEnd = default;
    uintptr pItem = default;
    uintptr pLoop = default;
    uintptr pTerm = default;
    uintptr v4 = default;
    _ = hasRightJoin;
    _ = i;
    _ = m1;
    _ = nByte;
    _ = pEnd;
    _ = pItem;
    _ = pLoop;
    _ = pTerm;
    _ = tabUsed;
    _ = v4;
    tabUsed = _sqlite3WhereExprListUsage(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpResultSet);
    if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy != 0) {
        tabUsed = (TBitmask)(tabUsed | _sqlite3WhereExprListUsage(ref (tls).DerefOrNull(), pWInfo + 592, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy));
    }
    hasRightJoin = libc.BoolInt32((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0);
    i = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - (int32)1;
    for (;;) {
        if (!(i >= (int32)1)) {
            break;
        }
        pLoop = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856 + (uintptr)i * 112))).Value.FpWLoop;
        pItem = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FiTab * 80;
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_RIGHT))) != (int32)JT_LEFT) {
            goto _1;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_WANT_DISTINCT) == 0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_ONEROW) == (uint32)0) {
            goto _1;
        }
        if ((TBitmask)(tabUsed & (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf) != (uint64)0) {
            goto _1;
        }
        pEnd = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsWC.Fa + (uintptr)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsWC.FnTerm * 56;
        pTerm = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsWC.Fa;
        for (;;) {
            if (!(pTerm < pEnd)) {
                break;
            }
            if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll & (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf) != (uint64)0) {
                if (!((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON))) != libc.Uint32FromInt32(0)) || ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr + 52)) != (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor) {
                    break;
                }
            }
            if (hasRightJoin != 0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_InnerON))) != (uint32)0 && ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr + 52)) == (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor) {
                break;
            }
            goto _2;
_2:;
            pTerm += 56;
        }
        if (pTerm < pEnd) {
            goto _1;
        }
        m1 = libc.Uint64FromInt32(1).Lsh((int64)(i)) - (uint64)1;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask = (TBitmask)((TBitmask)(m1 & (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask) | (TBitmask)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FrevMask >> (int)(libc.Int32FromInt32(1)) & ~m1));
        notReady = (TBitmask)(notReady & ~(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf);
        pTerm = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FsWC.Fa;
        for (;;) {
            if (!(pTerm < pEnd)) {
                break;
            }
            if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll & (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf) != (uint64)0) {
                v4 = pTerm + 18;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)) | libc.Int32FromInt32(TERM_CODED));
                (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqAll = (uint64)0;
            }
            goto _3;
_3:;
            pTerm += 56;
        }
        if (i != libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - (int32)1) {
            nByte = libc.Int32FromUint64(libc.Uint64FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - libc.Int32FromInt32(1) - i) * (uint64)112);
            libc.Xmemmove(tls, pWInfo + 856 + (uintptr)i * 112, pWInfo + 856 + (uintptr)(i + (int32)1) * 112, libc.Uint64FromInt32(nByte));
        }
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel = (Tu8)((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel - 1);
        goto _1;
_1:;
        i = i - 1;
    }
    return notReady;
}

internal static int32 /*r*/ _yyGrowStack(ж<libc.TLS> tls, uintptr p) {
    int32 idx = default;
    int32 nLimit = default;
    int32 newSize = default;
    int32 oldSize = default;
    uintptr pNew = default;
    _ = idx;
    _ = nLimit;
    _ = newSize;
    _ = oldSize;
    _ = pNew;
    oldSize = (int32)1 + (int32)(((int64)(@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.FyystackEnd - (int64)(@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack) / 24);
    nLimit = _parserStackSizeLimit(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.FpParse);
    newSize = oldSize * (int32)2 + (int32)100;
    if (newSize > nLimit) {
        newSize = nLimit;
        if (newSize <= oldSize) {
            return (int32)1;
        }
    }
    idx = (int32)(((int64)(@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyytos - (int64)(@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack) / 24);
    if ((@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack == p + 32) {
        pNew = _parserStackRealloc(tls, (uintptr)0, (uint64)(libc.Uint64FromInt32(newSize) * (uint64)24), (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.FpParse);
        if (pNew == (uintptr)0) {
            return (int32)1;
        }
        libc.Xmemcpy(tls, pNew, (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack, libc.Uint64FromInt32(oldSize) * (uint64)24);
    } else {
        pNew = _parserStackRealloc(tls, (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack, (uint64)(libc.Uint64FromInt32(newSize) * (uint64)24), (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.FpParse);
        if (pNew == (uintptr)0) {
            return (int32)1;
        }
    }
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack = pNew;
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyytos = (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack + (uintptr)idx * 24;
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.FyystackEnd = (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)p)).Value.Fyystack + (uintptr)(newSize - (int32)1) * 24;
    return 0;
}

[GoInit] internal static void initΔ2() {
    @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ_aVfs);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 32))).Value = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_posixIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 40))).Value = __ccgo_fp(_unixOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 48))).Value = __ccgo_fp(_unixDelete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 56))).Value = __ccgo_fp(_unixAccess);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 64))).Value = __ccgo_fp(_unixFullPathname);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 72))).Value = __ccgo_fp(_unixDlOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 80))).Value = __ccgo_fp(_unixDlError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 88))).Value = __ccgo_fp(_unixDlSym);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 96))).Value = __ccgo_fp(_unixDlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 104))).Value = __ccgo_fp(_unixRandomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 112))).Value = __ccgo_fp(_unixSleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 120))).Value = __ccgo_fp(_unixCurrentTime);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 128))).Value = __ccgo_fp(_unixGetLastError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 136))).Value = __ccgo_fp(_unixCurrentTimeInt64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 144))).Value = __ccgo_fp(_unixSetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 152))).Value = __ccgo_fp(_unixGetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 160))).Value = __ccgo_fp(_unixNextSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 200))).Value = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_nolockIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 208))).Value = __ccgo_fp(_unixOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 216))).Value = __ccgo_fp(_unixDelete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 224))).Value = __ccgo_fp(_unixAccess);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 232))).Value = __ccgo_fp(_unixFullPathname);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 240))).Value = __ccgo_fp(_unixDlOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 248))).Value = __ccgo_fp(_unixDlError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 256))).Value = __ccgo_fp(_unixDlSym);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 264))).Value = __ccgo_fp(_unixDlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 272))).Value = __ccgo_fp(_unixRandomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 280))).Value = __ccgo_fp(_unixSleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 288))).Value = __ccgo_fp(_unixCurrentTime);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 296))).Value = __ccgo_fp(_unixGetLastError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 304))).Value = __ccgo_fp(_unixCurrentTimeInt64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 312))).Value = __ccgo_fp(_unixSetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 320))).Value = __ccgo_fp(_unixGetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 328))).Value = __ccgo_fp(_unixNextSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 368))).Value = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_dotlockIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 376))).Value = __ccgo_fp(_unixOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 384))).Value = __ccgo_fp(_unixDelete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 392))).Value = __ccgo_fp(_unixAccess);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 400))).Value = __ccgo_fp(_unixFullPathname);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 408))).Value = __ccgo_fp(_unixDlOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 416))).Value = __ccgo_fp(_unixDlError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 424))).Value = __ccgo_fp(_unixDlSym);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 432))).Value = __ccgo_fp(_unixDlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 440))).Value = __ccgo_fp(_unixRandomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 448))).Value = __ccgo_fp(_unixSleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 456))).Value = __ccgo_fp(_unixCurrentTime);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 464))).Value = __ccgo_fp(_unixGetLastError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 472))).Value = __ccgo_fp(_unixCurrentTimeInt64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 480))).Value = __ccgo_fp(_unixSetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 488))).Value = __ccgo_fp(_unixGetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 496))).Value = __ccgo_fp(_unixNextSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 536))).Value = (uintptr)@unsafe.Pointer.FromBox(Ꮡ_posixIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 544))).Value = __ccgo_fp(_unixOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 552))).Value = __ccgo_fp(_unixDelete);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 560))).Value = __ccgo_fp(_unixAccess);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 568))).Value = __ccgo_fp(_unixFullPathname);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 576))).Value = __ccgo_fp(_unixDlOpen);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 584))).Value = __ccgo_fp(_unixDlError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 592))).Value = __ccgo_fp(_unixDlSym);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 600))).Value = __ccgo_fp(_unixDlClose);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 608))).Value = __ccgo_fp(_unixRandomness);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 616))).Value = __ccgo_fp(_unixSleep);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 624))).Value = __ccgo_fp(_unixCurrentTime);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 632))).Value = __ccgo_fp(_unixGetLastError);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 640))).Value = __ccgo_fp(_unixCurrentTimeInt64);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 648))).Value = __ccgo_fp(_unixSetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 656))).Value = __ccgo_fp(_unixGetSystemCall);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 664))).Value = __ccgo_fp(_unixNextSystemCall);
}

} // end sqlite3_package
