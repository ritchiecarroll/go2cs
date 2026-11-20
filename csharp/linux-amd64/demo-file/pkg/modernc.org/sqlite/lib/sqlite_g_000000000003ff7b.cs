namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _fts5DoclistIterNext(ref libc.TLS tls, uintptr pIter) {
    ref var iDelta = ref heap(new Tu64(), out var ᏑiDelta);
    ref var nPos = ref heap(new int32(), out var ᏑnPos);
    uintptr p = default;
    uintptr v1 = default;
    _ = iDelta;
    _ = nPos;
    _ = p;
    _ = v1;
    p = (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaPoslist + (uintptr)(@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnSize + (uintptr)(@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnPoslist;
    if (p >= (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaEof) {
        (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaPoslist = (uintptr)0;
    } else {
        p = p + (uintptr)_sqlite3Fts5GetVarint(ref tls, (@unsafe.Pointer)p, @unsafe.Pointer.FromPinnedBox(ᏑiDelta));
        v1 = pIter + 8;
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v1)).Value = (Ti64)((uint64)(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)v1)) + iDelta);
        if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))) & (int32)0x80) != 0) {
            (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnSize = _sqlite3Fts5GetVarint32(ref tls, p, @unsafe.Pointer.FromPinnedBox(ᏑnPos));
            (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnPoslist = nPos >> (int)((int32)1);
        } else {
            (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnPoslist = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))) >> (int)((int32)1);
            (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnSize = (int32)1;
        }
        (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaPoslist = p;
        if ((@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaPoslist + (uintptr)(@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FnPoslist > (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaEof) {
            (@unsafe.Pointer.To<TFts5DoclistIter>((@unsafe.Pointer)pIter)).Value.FaPoslist = (uintptr)0;
        }
    }
}

internal static void _fts5IndexAutomerge(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer ppStruct, int32 nLeaf) {
    int32 nRem = default;
    int32 nWork = default;
    Tu64 nWrite = default;
    uintptr pStruct = default;
    _ = nRem;
    _ = nWork;
    _ = nWrite;
    _ = pStruct;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FnAutomerge > 0 && ~@unsafe.Pointer.To<uintptr>(ppStruct) != (uintptr)0) {
        pStruct = ~@unsafe.Pointer.To<uintptr>(ppStruct);
        nWrite = (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnWriteCounter;
        nWork = libc.Int32FromUint64((nWrite + libc.Uint64FromInt32(nLeaf)) / libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnWorkUnit) - nWrite / libc.Uint64FromInt32((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnWorkUnit));
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pStruct + 8))).Value += libc.Uint64FromInt32(nLeaf);
        nRem = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FnWorkUnit * nWork * (@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnLevel;
        _fts5IndexMerge(tls, p, ppStruct, nRem, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FnAutomerge);
    }
}

internal static void _sumStep(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    uintptr p = default;
    int32 type1 = default;
    ref var x = ref heap(new Ti64(), out var Ꮡx);
    _ = p;
    _ = type1;
    _ = x;
    _ = argc;
    p = Xsqlite3_aggregate_context(tls, context, (int32)40);
    type1 = Xsqlite3_value_numeric_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (p != 0 && type1 != (int32)SQLITE_NULL) {
        (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fcnt = (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fcnt + 1;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fapprox) == 0) {
            if (type1 != (int32)SQLITE_INTEGER) {
                _kahanBabuskaNeumaierInit(ref (tls).DerefOrNull(), p, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum);
                (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fapprox = (uint8)1;
                _kahanBabuskaNeumaierStep(tls, p, Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
            } else {
                x = (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum;
                if (_sqlite3AddInt64(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡx), Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)))) == 0) {
                    (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum = x;
                } else {
                    (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fovrfl = (uint8)1;
                    _kahanBabuskaNeumaierInit(ref (tls).DerefOrNull(), p, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum);
                    (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fapprox = (uint8)1;
                    _kahanBabuskaNeumaierStepInt64(tls, p, Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
                }
            }
        } else {
            if (type1 == (int32)SQLITE_INTEGER) {
                _kahanBabuskaNeumaierStepInt64(tls, p, Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
            } else {
                (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fovrfl = (uint8)0;
                _kahanBabuskaNeumaierStep(tls, p, Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
            }
        }
    }
}

internal static int32 /*r*/ _whereOrInsert(ref libc.TLS tls, @unsafe.Pointer pSet, TBitmask prereq, TLogEst rRun, TLogEst nOut) {
    Tu16 i = default;
    Tu16 v2 = default;
    @unsafe.Pointer p = default;
    @unsafe.Pointer v3 = default;
    _ = i;
    _ = p;
    _ = v2;
    _ = v3;
    i = ~@unsafe.Pointer.To<Tu16>(pSet);
    p = @unsafe.Add(pSet, 8);
    for (;;) {
        if (!(libc.Int32FromUint16(i) > 0)) {
            break;
        }
        if ((int32)rRun <= (int32)(~@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 8))) && (TBitmask)(prereq & ~@unsafe.Pointer.To<TBitmask>(p)) == prereq) {
            goto whereOrInsert_done;
        }
        if ((int32)(~@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 8))) <= (int32)rRun && (TBitmask)(~@unsafe.Pointer.To<TBitmask>(p) & prereq) == ~@unsafe.Pointer.To<TBitmask>(p)) {
            return 0;
        }
        goto _1;
_1:;
        i = (Tu16)(i - 1);
        p = @unsafe.Add(p, 16);
    }
    if (libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(pSet)) < (int32)N_OR_COST) {
        v3 = pSet;
        v2 = ~@unsafe.Pointer.To<Tu16>(v3);
        (@unsafe.Pointer.To<Tu16>(v3)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>(v3) + 1);
        p = @unsafe.Add(@unsafe.Add(pSet, 8), (uintptr)v2 * 16);
        (@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 10))).Value = nOut;
    } else {
        p = @unsafe.Add(pSet, 8);
        i = (uint16)1;
        for (;;) {
            if (!(libc.Int32FromUint16(i) < libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(pSet)))) {
                break;
            }
            if ((int32)(~@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 8))) > (int32)(~@unsafe.Pointer.To<TLogEst>(@unsafe.Add(@unsafe.Add(@unsafe.Add(pSet, 8), (uintptr)i * 16), 8)))) {
                p = @unsafe.Add(@unsafe.Add(pSet, 8), (uintptr)i * 16);
            }
            goto _4;
_4:;
            i = (Tu16)(i + 1);
        }
        if ((int32)(~@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 8))) <= (int32)rRun) {
            return 0;
        }
    }
    goto whereOrInsert_done;
whereOrInsert_done:;
    (@unsafe.Pointer.To<TBitmask>(p)).Value = prereq;
    (@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 8))).Value = rRun;
    if ((int32)(~@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 10))) > (int32)nOut) {
        (@unsafe.Pointer.To<TLogEst>(@unsafe.Add(p, 10))).Value = nOut;
    }
    return (int32)1;
}

} // end sqlite3_package
