namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static Tsqlite3_int64 /*r*/ Xsqlite3_soft_heap_limit64(ж<libc.TLS> tls, Tsqlite3_int64 n) {
    Tsqlite3_int64 excess = default;
    Tsqlite3_int64 nUsed = default;
    Tsqlite3_int64 priorLimit = default;
    int32 rc = default;
    _ = excess;
    _ = nUsed;
    _ = priorLimit;
    _ = rc;
    rc = Xsqlite3_initialize(tls);
    if (rc != 0) {
        return (int64)(-(int32)1);
    }
    Xsqlite3_mutex_enter(tls, _mem0.Fmutex);
    priorLimit = _mem0.FalarmThreshold;
    if (n < 0) {
        Xsqlite3_mutex_leave(tls, _mem0.Fmutex);
        return priorLimit;
    }
    if (_mem0.FhardLimit > 0 && (n > _mem0.FhardLimit || n == 0)) {
        n = _mem0.FhardLimit;
    }
    _mem0.FalarmThreshold = n;
    nUsed = _sqlite3StatusValue(ref (tls).DerefOrNull(), SQLITE_STATUS_MEMORY_USED);
    libc.AtomicStoreNInt32((uintptr)Ꮡ_mem0 + 24, libc.BoolInt32(n > 0 && n <= nUsed), libc.Int32FromInt32(__ATOMIC_RELAXED));
    Xsqlite3_mutex_leave(tls, _mem0.Fmutex);
    excess = Xsqlite3_memory_used(tls) - n;
    if (excess > 0) {
        Xsqlite3_release_memory(tls, (int32)((Tsqlite3_int64)(excess & libc.Int64FromInt32(0x7fffffff))));
    }
    return priorLimit;
}

internal static void _first_valueFinalizeFunc(ж<libc.TLS> tls, uintptr pCtx) {
    uintptr p = default;
    _ = p;
    p = Xsqlite3_aggregate_context(tls, pCtx, (int32)16);
    if (p != 0 && (@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue != 0) {
        Xsqlite3_result_value(tls, pCtx, (@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue);
        Xsqlite3_value_free(tls, (@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue);
        (@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue = (uintptr)0;
    }
}

internal static void _first_valueStepFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apArg) {
    uintptr p = default;
    _ = p;
    p = Xsqlite3_aggregate_context(tls, pCtx, (int32)16);
    if (p != 0 && (@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue == (uintptr)0) {
        (@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue = Xsqlite3_value_dup(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
        if (!((@unsafe.Pointer.To<TNthValueCtx>((@unsafe.Pointer)p)).Value.FpValue != 0)) {
            Xsqlite3_result_error_nomem(tls, pCtx);
        }
    }
    _ = nArg;
    _ = apArg;
}

internal static int32 /*r*/ _fts5DlidxIterFirst(ref libc.TLS tls, uintptr pIter) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl)) {
            break;
        }
        _fts5DlidxLvlNext(ref tls, pIter + 8 + (uintptr)i * 32);
        goto _1;
_1:;
        i = i + 1;
    }
    return (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)(pIter + 8))).Value.FbEof;
}

internal static void _fts5DlidxIterFree(ж<libc.TLS> tls, uintptr pIter) {
    int32 i = default;
    _ = i;
    if (pIter != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFts5DlidxIter>((@unsafe.Pointer)pIter)).Value.FnLvl)) {
                break;
            }
            _fts5DataRelease(tls, (@unsafe.Pointer.To<TFts5DlidxLvl>((@unsafe.Pointer)(pIter + 8 + (uintptr)i * 32))).Value.FpData);
            goto _1;
_1:;
            i = i + 1;
        }
        Xsqlite3_free(tls, pIter);
    }
}

internal static void _fts5WriteDlidxClear(ж<libc.TLS> tls, uintptr p, uintptr pWriter, int32 bFlush) {
    int32 i = default;
    uintptr pDlidx = default;
    _ = i;
    _ = pDlidx;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FnDlidx)) {
            break;
        }
        pDlidx = (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FaDlidx + (uintptr)i * 32;
        if ((@unsafe.Pointer.To<TFts5DlidxWriter>((@unsafe.Pointer)pDlidx)).Value.Fbuf.Fn == 0) {
            break;
        }
        if (bFlush != 0) {
            _fts5DataWrite(tls, p, ((int64)(@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(1)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)i).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)(@unsafe.Pointer.To<TFts5DlidxWriter>((@unsafe.Pointer)pDlidx)).Value.Fpgno, (@unsafe.Pointer.To<TFts5DlidxWriter>((@unsafe.Pointer)pDlidx)).Value.Fbuf.Fp, (@unsafe.Pointer.To<TFts5DlidxWriter>((@unsafe.Pointer)pDlidx)).Value.Fbuf.Fn);
        }
        _sqlite3Fts5BufferZero(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pDlidx + 16));
        (@unsafe.Pointer.To<TFts5DlidxWriter>((@unsafe.Pointer)pDlidx)).Value.FbPrevValid = 0;
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static void _mallocWithAlarm(ж<libc.TLS> tls, int32 n, @unsafe.Pointer pp) {
    int32 nFull = default;
    Tsqlite3_int64 nUsed = default;
    uintptr p = default;
    _ = nFull;
    _ = nUsed;
    _ = p;
    nFull = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32>>(_sqlite3Config.Fm.FxRoundup)(tls, n);
    _sqlite3StatusHighwater(ref (tls).DerefOrNull(), (int32)SQLITE_STATUS_MALLOC_SIZE, n);
    if (_mem0.FalarmThreshold > 0) {
        nUsed = _sqlite3StatusValue(ref (tls).DerefOrNull(), SQLITE_STATUS_MEMORY_USED);
        if (nUsed >= _mem0.FalarmThreshold - (int64)nFull) {
            libc.AtomicStoreNInt32((uintptr)Ꮡ_mem0 + 24, libc.Int32FromInt32(1), libc.Int32FromInt32(__ATOMIC_RELAXED));
            _sqlite3MallocAlarm(tls, nFull);
            if (_mem0.FhardLimit != 0) {
                nUsed = _sqlite3StatusValue(ref (tls).DerefOrNull(), SQLITE_STATUS_MEMORY_USED);
                if (nUsed >= _mem0.FhardLimit - (int64)nFull) {
                    (@unsafe.Pointer.To<uintptr>(pp)).Value = (uintptr)0;
                    return;
                }
            }
        } else {
            libc.AtomicStoreNInt32((uintptr)Ꮡ_mem0 + 24, libc.Int32FromInt32(0), libc.Int32FromInt32(__ATOMIC_RELAXED));
        }
    }
    p = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr>>(_sqlite3Config.Fm.FxMalloc)(tls, nFull);
    if (p == (uintptr)0 && _mem0.FalarmThreshold > 0) {
        _sqlite3MallocAlarm(tls, nFull);
        p = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr>>(_sqlite3Config.Fm.FxMalloc)(tls, nFull);
    }
    if (p != 0) {
        nFull = _sqlite3MallocSize(tls, p);
        _sqlite3StatusUp(ref (tls).DerefOrNull(), SQLITE_STATUS_MEMORY_USED, nFull);
        _sqlite3StatusUp(ref (tls).DerefOrNull(), (int32)SQLITE_STATUS_MALLOC_COUNT, (int32)1);
    }
    (@unsafe.Pointer.To<uintptr>(pp)).Value = p;
}

internal static int32 /*r*/ _sqlite3HeapNearlyFull(ref libc.TLS tls) {
    return libc.AtomicLoadNInt32((uintptr)Ꮡ_mem0 + 24, libc.Int32FromInt32(__ATOMIC_RELAXED));
}

internal static uintptr /*r*/ _sqlite3WhereMalloc(ж<libc.TLS> tls, uintptr pWInfo, Tu64 nByte) {
    uintptr pBlock = default;
    _ = pBlock;
    pBlock = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb, nByte + (uint64)16);
    if (pBlock != 0) {
        (@unsafe.Pointer.To<TWhereMemBlock>((@unsafe.Pointer)pBlock)).Value.FpNext = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpMemToFree;
        (@unsafe.Pointer.To<TWhereMemBlock>((@unsafe.Pointer)pBlock)).Value.Fsz = nByte;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpMemToFree = pBlock;
        pBlock += 16;
    }
    return pBlock;
}

internal static void _sumInverse(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    Ti64 iVal = default;
    ref var x = ref heap(new Ti64(), out var Ꮡx);
    uintptr p = default;
    int32 type1 = default;
    _ = iVal;
    _ = p;
    _ = type1;
    _ = x;
    _ = argc;
    p = Xsqlite3_aggregate_context(tls, context, (int32)40);
    type1 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (p != 0 && type1 != (int32)SQLITE_NULL) {
        (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fcnt = (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fcnt - 1;
        if (!((@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fapprox != 0)) {
            x = (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum;
            if (_sqlite3SubInt64(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡx), Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)))) == 0) {
                (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum = x;
                return;
            }
            (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fovrfl = (uint8)1;
            (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fapprox = (uint8)1;
            _kahanBabuskaNeumaierInit(ref (tls).DerefOrNull(), p, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum);
        }
        if (type1 == (int32)SQLITE_INTEGER) {
            iVal = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
            if (iVal != (int64)(-libc.Int32FromInt32(1)) - (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))))) {
                _kahanBabuskaNeumaierStepInt64(tls, p, -iVal);
            } else {
                _kahanBabuskaNeumaierStepInt64(tls, p, libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))));
                _kahanBabuskaNeumaierStepInt64(tls, p, (int64)1);
            }
        } else {
            _kahanBabuskaNeumaierStep(tls, p, -Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
        }
    }
}

} // end sqlite3_package
