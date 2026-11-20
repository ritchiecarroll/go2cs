namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_stmt_explain(ж<libc.TLS> tls, uintptr pStmt, int32 eMode) {
    int32 rc = default;
    uintptr v = default;
    _ = rc;
    _ = v;
    v = pStmt;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fdb)).Value.Fmutex);
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(v + 200)) & 0xc) >> 2)) == eMode) {
        rc = SQLITE_OK;
    } else {
        if (eMode < 0 || eMode > (int32)2) {
            rc = (int32)SQLITE_ERROR;
        } else {
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FprepFlags) & (int32)SQLITE_PREPARE_SAVESQL) == 0) {
                rc = (int32)SQLITE_ERROR;
            } else {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FeVdbeState) != (int32)VDBE_READY_STATE) {
                    rc = (int32)SQLITE_BUSY;
                } else {
                    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnMem >= (int32)10 && (eMode != (int32)2 || (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(v + 200)) & 0x100) >> 8)) != 0)) {
                        libc.SetBitFieldPtr16Uint32(v + 200, libc.Uint32FromInt32(eMode), 2, 0xc);
                        rc = SQLITE_OK;
                    } else {
                        libc.SetBitFieldPtr16Uint32(v + 200, libc.Uint32FromInt32(eMode), 2, 0xc);
                        rc = _sqlite3Reprepare(tls, v);
                        libc.SetBitFieldPtr16Uint32(v + 200, libc.BoolUint32(eMode == libc.Int32FromInt32(2)), 8, 0x100);
                    }
                }
            }
        }
    }
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(v + 200)) & 0xc) >> 2)) != 0) {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnResColumn = libc.Uint16FromInt32((int32)12 - (int32)4 * (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(v + 200)) & 0xc) >> 2)));
    } else {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnResColumn = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnResAlloc;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Fdb)).Value.Fmutex);
    return rc;
}

public static int32 /*r*/ Xsqlite3_test_control(ж<libc.TLS> tls, int32 op, uintptr va) {
    uintptr aProg = default;
    uintptr db = default;
    uintptr db1 = default;
    uintptr db2 = default;
    uintptr db3 = default;
    uintptr db4 = default;
    uintptr db5 = default;
    uintptr db6 = default;
    uintptr db7 = default;
    uintptr pCtx = default;
    uintptr pI1 = default;
    uintptr pI2 = default;
    uintptr pN = default;
    uintptr pR = default;
    uintptr pU64 = default;
    uintptr pn = default;
    uintptr ptr = default;
    uintptr xBenignBegin = default;
    uintptr xBenignEnd = default;
    uintptr z = default;
    ref var ap = ref heap(new Tva_list(), out var Ꮡap);
    int32 b = default;
    int32 iDb = default;
    int32 opTrace = default;
    int32 rc = default;
    int32 sz = default;
    int32 x = default;
    int32 x1 = default;
    int32 x2 = default;
    int32 y = default;
    int32 v1 = default;
    uint32 newVal = default;
    float64 rIn = default;
    TLogEst rLogEst = default;
    bool v2 = default;
    _ = aProg;
    _ = ap;
    _ = b;
    _ = db;
    _ = db1;
    _ = db2;
    _ = db3;
    _ = db4;
    _ = db5;
    _ = db6;
    _ = db7;
    _ = iDb;
    _ = newVal;
    _ = opTrace;
    _ = pCtx;
    _ = pI1;
    _ = pI2;
    _ = pN;
    _ = pR;
    _ = pU64;
    _ = pn;
    _ = ptr;
    _ = rIn;
    _ = rLogEst;
    _ = rc;
    _ = sz;
    _ = x;
    _ = x1;
    _ = x2;
    _ = xBenignBegin;
    _ = xBenignEnd;
    _ = y;
    _ = z;
    _ = v1;
    _ = v2;
    rc = 0;
    ap = va;
    var matchᴛ1 = false;
    if (op == (int32)SQLITE_TESTCTRL_PRNG_SAVE) { matchᴛ1 = true;
        do {
            _sqlite3PrngSaveState(tls);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_PRNG_RESTORE) { matchᴛ1 = true;
        do {
            _sqlite3PrngRestoreState(tls);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_PRNG_SEED) { matchᴛ1 = true;
        do {
            x = libc.VaInt32(Ꮡap);
            db = libc.VaUintptr(Ꮡap);
            v2 = db != 0; if (v2) {
                v1 = (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb))).Value.FpSchema)).Value.Fschema_cookie;
                y = v1;
            }
            if (v2 && v1 != 0) {
                x = y;
            }
            _sqlite3Config.FiPrngSeed = libc.Uint32FromInt32(x);
            Xsqlite3_randomness(tls, 0, (uintptr)0);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_FK_NO_ACTION) { matchᴛ1 = true;
        do {
            db1 = libc.VaUintptr(Ꮡap);
            b = libc.VaInt32(Ꮡap);
            if (b != 0) {
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db1 + 48))).Value |= libc.Uint64FromInt32(libc.Int32FromInt32(0x00008)).Lsh((int64)(libc.Int32FromInt32(32)));
            } else {
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db1 + 48))).Value &= ~(libc.Uint64FromInt32(libc.Int32FromInt32(0x00008)).Lsh((int64)(libc.Int32FromInt32(32))));
            }
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_BITVEC_TEST) { matchᴛ1 = true;
        do {
            sz = libc.VaInt32(Ꮡap);
            aProg = libc.VaUintptr(Ꮡap);
            rc = _sqlite3BitvecBuiltinTest(tls, sz, aProg);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_FAULT_INSTALL) { matchᴛ1 = true;
        do {
            _sqlite3Config.FxTestCallback = libc.VaUintptr(Ꮡap);
            rc = _sqlite3FaultSim(tls, 0);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_BENIGN_MALLOC_HOOKS) { matchᴛ1 = true;
        do {
            xBenignBegin = libc.VaUintptr(Ꮡap);
            xBenignEnd = libc.VaUintptr(Ꮡap);
            _sqlite3BenignMallocHooks(ref (tls).DerefOrNull(), xBenignBegin, xBenignEnd);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_PENDING_BYTE) { matchᴛ1 = true;
        do {
            rc = _sqlite3PendingByte;
            newVal = libc.VaUint32(Ꮡap);
            if (newVal != 0) {
                _sqlite3PendingByte = libc.Int32FromUint32(newVal);
            }
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_ASSERT) { matchᴛ1 = true;
        do {
            x1 = 0;
            rc = x1;
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_ALWAYS) { matchᴛ1 = true;
        do {
            x2 = libc.VaInt32(Ꮡap);
            if (x2 != 0) {
                v1 = x2;
            } else {
                v1 = 0;
            }
            rc = v1;
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_BYTEORDER) { matchᴛ1 = true;
        do {
            rc = libc.Int32FromInt32(SQLITE_BYTEORDER) * libc.Int32FromInt32(100) + libc.Int32FromInt32(SQLITE_LITTLEENDIAN) * libc.Int32FromInt32(10) + libc.Int32FromInt32(SQLITE_BIGENDIAN);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_OPTIMIZATIONS) { matchᴛ1 = true;
        do {
            db2 = libc.VaUintptr(Ꮡap);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db2)).Value.FdbOptFlags = libc.VaUint32(Ꮡap);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_GETOPT) { matchᴛ1 = true;
        do {
            db3 = libc.VaUintptr(Ꮡap);
            pN = libc.VaUintptr(Ꮡap);
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pN))).Value = libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db3)).Value.FdbOptFlags);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_LOCALTIME_FAULT) { matchᴛ1 = true;
        do {
            _sqlite3Config.FbLocaltimeFault = libc.VaInt32(Ꮡap);
            if (_sqlite3Config.FbLocaltimeFault == (int32)2) {
                _sqlite3Config.FxAltLocaltime = libc.VaUintptr(Ꮡap);
            } else {
                _sqlite3Config.FxAltLocaltime = (uintptr)0;
            }
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_INTERNAL_FUNCTIONS) { matchᴛ1 = true;
        do {
            db4 = libc.VaUintptr(Ꮡap);
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(db4 + 44))).Value ^= (uint32)DBFLAG_InternalFunc;
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_NEVER_CORRUPT) { matchᴛ1 = true;
        do {
            _sqlite3Config.FneverCorrupt = libc.VaInt32(Ꮡap);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_EXTRA_SCHEMA_CHECKS) { matchᴛ1 = true;
        do {
            _sqlite3Config.FbExtraSchemaChecks = libc.Uint8FromInt32(libc.VaInt32(Ꮡap));
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_ONCE_RESET_THRESHOLD) { matchᴛ1 = true;
        do {
            _sqlite3Config.FiOnceResetThreshold = libc.VaInt32(Ꮡap);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_VDBE_COVERAGE) { matchᴛ1 = true;
        do {
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_SORTER_MMAP) { matchᴛ1 = true;
        do {
            db5 = libc.VaUintptr(Ꮡap);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db5)).Value.FnMaxSorterMmap = libc.VaInt32(Ꮡap);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_ISINIT) { matchᴛ1 = true;
        do {
            if (libc.AtomicLoadPInt32((uintptr)Ꮡ_sqlite3Config + 340) == 0) {
                rc = (int32)SQLITE_ERROR;
            }
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_IMPOSTER) { matchᴛ1 = true;
        do {
            db6 = libc.VaUintptr(Ꮡap);
            Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Fmutex);
            iDb = _sqlite3FindDbName(tls, db6, libc.VaUintptr(Ꮡap));
            if (iDb >= 0) {
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Finit1.FiDb = libc.Uint8FromInt32(iDb);
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Finit1.Fbusy = (uint8)libc.AssignBitFieldPtr8Uint32(db6 + 200 + 8, libc.Uint32FromInt32(libc.VaInt32(Ꮡap)), 2, 1, 0x6);
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Finit1.FnewTnum = libc.Uint32FromInt32(libc.VaInt32(Ꮡap));
                if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Finit1.Fbusy) == 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Finit1.FnewTnum > (uint32)0) {
                    _sqlite3ResetAllSchemasOfConnection(tls, db6);
                }
            }
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db6)).Value.Fmutex);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_RESULT_INTREAL) { matchᴛ1 = true;
        do {
            pCtx = libc.VaUintptr(Ꮡap);
            _sqlite3ResultIntReal(ref (tls).DerefOrNull(), pCtx);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_SEEK_COUNT) { matchᴛ1 = true;
        do {
            db7 = libc.VaUintptr(Ꮡap);
            pn = libc.VaUintptr(Ꮡap);
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pn))).Value = (uint64)0;
            _ = db7;
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_TRACEFLAGS) { matchᴛ1 = true;
        do {
            opTrace = libc.VaInt32(Ꮡap);
            ptr = libc.VaUintptr(Ꮡap);
            switch (opTrace) {
            case 0: {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(ptr))).Value = _sqlite3TreeTrace;
                break;
            }
            case var ᴛ when ᴛ == (int32)1: {
                _sqlite3TreeTrace = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(ptr));
                break;
            }
            case var ᴛ when ᴛ == (int32)2: {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(ptr))).Value = _sqlite3WhereTrace;
                break;
            }
            case var ᴛ when ᴛ == (int32)3: {
                _sqlite3WhereTrace = ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(ptr));
                break;
                break;
            }}

            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_LOGEST) { matchᴛ1 = true;
        do {
            rIn = libc.VaFloat64(Ꮡap);
            rLogEst = _sqlite3LogEstFromDouble(tls, rIn);
            pI1 = libc.VaUintptr(Ꮡap);
            pU64 = libc.VaUintptr(Ꮡap);
            pI2 = libc.VaUintptr(Ꮡap);
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pI1))).Value = (int32)rLogEst;
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pU64))).Value = _sqlite3LogEstToInt(ref (tls).DerefOrNull(), rLogEst);
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pI2))).Value = (int32)_sqlite3LogEst(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(pU64)));
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_ATOF) {
        do {
            z = libc.VaUintptr(Ꮡap);
            pR = libc.VaUintptr(Ꮡap);
            rc = _sqlite3AtoF(tls, z, (@unsafe.Pointer)pR);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_TESTCTRL_JSON_SELFCHECK) { matchᴛ1 = true;
        do {
            break;
        } while (false);
    }

    _ = ap;
    return rc;
}

internal static int32 /*r*/ _analysisLoader(ж<libc.TLS> tls, uintptr pData, int32 argc, uintptr argv, uintptr NotUsed) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(160);
        defer(tls.Free, (nint)160, ref ᒐ);
        uintptr aiRowEst = default;
        uintptr pIndex = default;
        uintptr pInfo = default;
        uintptr pTable = default;
        uintptr z = default;
        int32 nCol = default;
        TIndex _ᴛ1 = default;
        _ = aiRowEst;
        _ = nCol;
        _ = pIndex;
        _ = pInfo;
        _ = pTable;
        _ = z;
        pInfo = pData;
        _ = NotUsed;
        _ = argc;
        if (argv == (uintptr)0 || ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)) == (uintptr)0 || ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8)) == (uintptr)0) {
            r = 0; goto ᒐdone;
        }
        pTable = _sqlite3FindTable(tls, (@unsafe.Pointer.To<TanalysisInfo>((@unsafe.Pointer)pInfo)).Value.Fdb, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), (@unsafe.Pointer.To<TanalysisInfo>((@unsafe.Pointer)pInfo)).Value.FzDatabase);
        if (pTable == (uintptr)0) {
            r = 0; goto ᒐdone;
        }
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)) == (uintptr)0) {
            pIndex = (uintptr)0;
        } else {
            if (Xsqlite3_stricmp(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8))) == 0) {
                pIndex = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTable);
            } else {
                pIndex = _sqlite3FindIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TanalysisInfo>((@unsafe.Pointer)pInfo)).Value.Fdb, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 1 * 8)), (@unsafe.Pointer.To<TanalysisInfo>((@unsafe.Pointer)pInfo)).Value.FzDatabase);
            }
        }
        z = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv + 2 * 8));
        if (pIndex != 0) {
            aiRowEst = (uintptr)0;
            nCol = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnKeyCol) + (int32)1;
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiRowEst == (uintptr)0) {
                (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiRowEst = _sqlite3MallocZero(tls, (uint64)((uint64)8 * libc.Uint64FromInt32(nCol)));
                if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiRowEst == (uintptr)0) {
                    _sqlite3OomFault(tls, (@unsafe.Pointer.To<TanalysisInfo>((@unsafe.Pointer)pInfo)).Value.Fdb);
                }
            }
            aiRowEst = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiRowEst;
            libc.SetBitFieldPtr16Uint32(pIndex + 100, libc.Uint32FromInt32(0), 2, 0x4);
            _decodeIntArray(tls, z, nCol, aiRowEst, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiRowLogEst, pIndex);
            libc.SetBitFieldPtr16Uint32(pIndex + 100, libc.Uint32FromInt32(1), 7, 0x80);
            if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpPartIdxWhere == (uintptr)0) {
                (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnRowLogEst = ~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiRowLogEst));
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTable + 48))).Value |= (uint32)TF_HasStat1;
            }
        } else {
            (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FszIdxRow = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FszTabRow;
            _decodeIntArray(tls, z, (int32)1, (uintptr)0, pTable + 58, bp);
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FszTabRow = (@unsafe.Pointer.To<TIndex>(__ccgo_ptr(bp))).Value.FszIdxRow;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTable + 48))).Value |= (uint32)TF_HasStat1;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _autoIncBegin(ж<libc.TLS> tls, uintptr pParse, int32 iDb, uintptr pTab) {
    int32 memId = default;
    int32 v2 = default;
    uintptr pInfo = default;
    uintptr pSeqTab = default;
    uintptr pToplevel = default;
    uintptr v1 = default;
    _ = memId;
    _ = pInfo;
    _ = pSeqTab;
    _ = pToplevel;
    _ = v1;
    _ = v2;
    memId = 0;
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Autoincrement) != (uint32)0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmDbFlags & (uint32)DBFLAG_Vacuum) == (uint32)0) {
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) {
            v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel;
        } else {
            v1 = pParse;
        }
        pToplevel = v1;
        pSeqTab = (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)(@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema)).Value.FpSeqTab;
        if (pSeqTab == (uintptr)0 || !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSeqTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) || libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSeqTab)).Value.FeTabType) == (int32)TABTYP_VTAB || (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pSeqTab)).Value.FnCol != (int32)2) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr + 1;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)));
            return 0;
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pToplevel + 40)) & 0x200) >> 9)) == 0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FpAinc = (uintptr)0;
        }
        pInfo = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FpAinc;
        while (pInfo != 0 && (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FpTab != pTab) {
            pInfo = (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FpNext;
        }
        if (pInfo == (uintptr)0) {
            pInfo = _sqlite3DbMallocRawNN(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)24);
            _sqlite3ParserAddCleanup(tls, pToplevel, __ccgo_fp(_sqlite3DbFree), pInfo);
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0) {
                return 0;
            }
            (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FpNext = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FpAinc;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FpAinc = pInfo;
            libc.SetBitFieldPtr16Uint32(pToplevel + 40, libc.Uint32FromInt32(1), 9, 0x200);
            (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FpTab = pTab;
            (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FiDb = iDb;
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FnMem = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pToplevel)).Value.FnMem + 1;
            v1 = pToplevel + 60;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1) + 1;
            v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v1);
            (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FregCtr = v2;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pToplevel + 60))).Value += (int32)2;
        }
        memId = (@unsafe.Pointer.To<TAutoincInfo>((@unsafe.Pointer)pInfo)).Value.FregCtr;
    }
    return memId;
}

internal static int32 /*r*/ _autoVacuumCommit(ж<libc.TLS> tls, uintptr p) {
    uintptr db = default;
    uintptr pBt = default;
    uintptr pPager = default;
    int32 iDb = default;
    int32 rc = default;
    TPgno iFree = default;
    TPgno nFin = default;
    TPgno nFree = default;
    TPgno nOrig = default;
    TPgno nVac = default;
    _ = db;
    _ = iDb;
    _ = iFree;
    _ = nFin;
    _ = nFree;
    _ = nOrig;
    _ = nVac;
    _ = pBt;
    _ = pPager;
    _ = rc;
    rc = SQLITE_OK;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    pPager = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager;
    _invalidateAllOverflowCache(ref (tls).DerefOrNull(), pBt);
    if (!((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FincrVacuum != 0)) {
        nOrig = _btreePagecount(ref (tls).DerefOrNull(), pBt);
        if (_ptrmapPageno(ref (tls).DerefOrNull(), pBt, nOrig) == nOrig || nOrig == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
            return _sqlite3CorruptError(tls, (int32)77406);
        }
        nFree = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 36));
        db = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAutovacPages != 0) {
            iDb = 0;
            for (;;) {
                if (!(iDb < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpBt == p) {
                    break;
                }
                goto _1;
_1:;
                iDb = iDb + 1;
            }
            nVac = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu32, Tu32, Tu32, uint32>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAutovacPages)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpAutovacPagesArg, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName, nOrig, nFree, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize);
            if (nVac > nFree) {
                nVac = nFree;
            }
            if (nVac == (uint32)0) {
                return SQLITE_OK;
            }
        } else {
            nVac = nFree;
        }
        nFin = _finalDbSize(ref (tls).DerefOrNull(), pBt, nOrig, nVac);
        if (nFin > nOrig) {
            return _sqlite3CorruptError(tls, (int32)77433);
        }
        if (nFin < nOrig) {
            rc = _saveAllCursors(tls, pBt, (uint32)0, (uintptr)0);
        }
        iFree = nOrig;
        for (;;) {
            if (!(iFree > nFin && rc == SQLITE_OK)) {
                break;
            }
            rc = _incrVacuumStep(tls, pBt, nFin, iFree, libc.BoolInt32(nVac == nFree));
            goto _2;
_2:;
            iFree = iFree - 1;
        }
        if ((rc == (int32)SQLITE_DONE || rc == SQLITE_OK) && nFree > (uint32)0) {
            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FpDbPage);
            if (nVac == nFree) {
                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 32), (uint32)0);
                _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 36), (uint32)0);
            }
            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 28), nFin);
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbDoTruncate = (uint8)1;
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = nFin;
        }
        if (rc != SQLITE_OK) {
            _sqlite3PagerRollback(tls, pPager);
        }
    }
    return rc;
}

internal static int32 /*r*/ _balance_quick(ж<libc.TLS> tls, uintptr pParent, uintptr pPage, uintptr pSpace) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(128);
        defer(tls.Free, (nint)128, ref ᒐ);
        uintptr pBt = default;
        ref var pNew = ref heap(new uintptr(), out var ᏑpNew);
        uintptr pOut = default;
        uintptr pStop = default;
        uintptr v1 = default;
        uintptr v3 = default;
        ref var pgnoNew = ref heap(new TPgno(), out var ᏑpgnoNew);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        Tu8 v2 = default;
        TCellArray _ᴛ1 = new();
        uintptr _ᴛ2 = default;
        Tu16 _ᴛ3 = default;
        _ = pBt;
        _ = pNew;
        _ = pOut;
        _ = pStop;
        _ = pgnoNew;
        _ = rc;
        _ = v1;
        _ = v2;
        _ = v3;
        pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) == 0) {
            r = _sqlite3CorruptError(tls, (int32)81195); goto ᒐdone;
        }
        rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpNew), @unsafe.Pointer.FromPinnedBox(ᏑpgnoNew), (uint32)0, (uint8)0);
        if (rc == SQLITE_OK) {
            pOut = pSpace + 4;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pPage + 40))).Value;
            (@unsafe.Pointer.To<Tu16>(__ccgo_ptr(bp + 8))).Value = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxCellSize)(tls, pPage, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
            _zeroPage(tls, pNew, libc.Int32FromInt32(PTF_INTKEY) | libc.Int32FromInt32(PTF_LEAFDATA) | libc.Int32FromInt32(PTF_LEAF));
            (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 16))).Value.FnCell = (int32)1;
            (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 16))).Value.FpRef = pPage;
            (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 16))).Value.FapCell = bp;
            (@unsafe.Pointer.To<TCellArray>(__ccgo_ptr(bp + 16))).Value.FszCell = bp + 8;
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 16 + 32))).Value = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 16 + 80))).Value = (int32)2;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 16 + 80 + (uintptr)(libc.Int32FromInt32(NB) * libc.Int32FromInt32(2) - libc.Int32FromInt32(1)) * 4))).Value = (int32)0x7fffffff;
            rc = _rebuildPage(tls, __ccgo_ptr(bp + 16), 0, (int32)1, pNew);
            if (rc != 0) {
                _releasePage(tls, pNew);
                r = rc; goto ᒐdone;
            }
            (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew)).Value.FnFree = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew)).Value.FcellOffset - (uint32)2 - (uint32)(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(bp + 8))));
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
                _ptrmapPut(tls, pBt, pgnoNew, (uint8)PTRMAP_BTREE, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.Fpgno, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                if (libc.Int32FromUint16(~@unsafe.Pointer.To<Tu16>(__ccgo_ptr(bp + 8))) > libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pNew)).Value.FminLocal)) {
                    _ptrmapPutOvflPtr(tls, pNew, pNew, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
                }
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - (int32)1))))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - (int32)1)) + 1)))));
            pStop = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) + 9;
            for (;;) {
                v1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) + 1;
                if (!((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))) & (int32)0x80) != 0 && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) < pStop)) {
                    break;
                }
            }
            pStop = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) + 9;
            for (;;) {
                v1 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp));
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) + 1;
                v2 = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1));
                v3 = pOut;
                pOut = pOut + 1;
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v3))).Value = v2;
                if (!((libc.Int32FromUint8(v2) & (int32)0x80) != 0 && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) < pStop)) {
                    break;
                }
            }
            if (rc == SQLITE_OK) {
                rc = _insertCell(tls, pParent, libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FnCell), pSpace, (int32)((int64)pOut - (int64)pSpace), (uintptr)0, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fpgno);
            }
            _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pParent)).Value.FhdrOffset) + (int32)8)), pgnoNew);
            _releasePage(tls, pNew);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _btreeBeginTrans(ж<libc.TLS> tls, uintptr p, int32 wrflag, @unsafe.Pointer pSchemaVersion) {
    uintptr pBlock = default;
    uintptr pBt = default;
    uintptr pIter = default;
    uintptr pPage1 = default;
    uintptr pPager = default;
    uintptr v1 = default;
    int32 rc = default;
    int32 v5 = default;
    bool v6 = default;
    _ = pBlock;
    _ = pBt;
    _ = pIter;
    _ = pPage1;
    _ = pPager;
    _ = rc;
    _ = v1;
    _ = v5;
    _ = v6;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    pPager = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager;
    rc = SQLITE_OK;
    _sqlite3BtreeEnter(tls, p);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_WRITE || libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_READ && !(wrflag != 0)) {
        goto trans_begun;
    }
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_ResetDatabase) != 0 && libc.Int32FromUint8(_sqlite3PagerIsreadonly(ref (tls).DerefOrNull(), pPager)) == 0) {
        v1 = pBt + 40;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(BTS_READ_ONLY));
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_READ_ONLY) != 0 && wrflag != 0) {
        rc = (int32)SQLITE_READONLY;
        goto trans_begun;
    }
    pBlock = (uintptr)0;
    if (wrflag != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction) == (int32)TRANS_WRITE || (libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_PENDING) != 0) {
        pBlock = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpWriter)).Value.Fdb;
    } else {
        if (wrflag > (int32)1) {
            pIter = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpLock;
            for (;;) {
                if (!(pIter != 0)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pIter)).Value.FpBtree != p) {
                    pBlock = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pIter)).Value.FpBtree)).Value.Fdb;
                    break;
                }
                goto _2;
_2:;
                pIter = (@unsafe.Pointer.To<TBtLock>((@unsafe.Pointer)pIter)).Value.FpNext;
            }
        }
    }
    if (pBlock != 0) {
        _sqlite3ConnectionBlocked(tls, (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb, pBlock);
        rc = libc.Int32FromInt32(SQLITE_LOCKED) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        goto trans_begun;
    }
    rc = _querySharedCacheTableLock(tls, p, (uint32)SCHEMA_ROOT, (uint8)READ_LOCK);
    if (SQLITE_OK != rc) {
        goto trans_begun;
    }
    v1 = pBt + 40;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(BTS_INITIALLY_EMPTY));
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage == (uint32)0) {
        v1 = pBt + 40;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTS_INITIALLY_EMPTY));
    }
    for (var cond = true; cond; cond = (rc & (int32)0xFF) == (int32)SQLITE_BUSY && libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction) == TRANS_NONE && _btreeInvokeBusyHandler(tls, pBt) != 0) {
        for (;;) {
            v6 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1 == (uintptr)0; if (v6) {
                v5 = _lockBtree(tls, pBt);
                rc = v5;
            }
            if (!(v6 && SQLITE_OK == v5)) {
                break;
            }
        }
        if (rc == SQLITE_OK && wrflag != 0) {
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_READ_ONLY) != 0) {
                rc = (int32)SQLITE_READONLY;
            } else {
                rc = _sqlite3PagerBegin(tls, pPager, libc.BoolInt32(wrflag > (int32)1), _sqlite3TempInMemory(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb));
                if (rc == SQLITE_OK) {
                    rc = _newDatabase(tls, pBt);
                } else {
                    if (rc == (libc.Int32FromInt32(SQLITE_BUSY) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)))) && libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction) == TRANS_NONE) {
                        rc = (int32)SQLITE_BUSY;
                    }
                }
            }
        }
        if (rc != SQLITE_OK) {
            _ = SQLITE_OK;
            _unlockBtreeIfUnused(tls, pBt);
        }
    }
    if (rc == SQLITE_OK) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == TRANS_NONE) {
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnTransaction = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnTransaction + 1;
            if ((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fsharable != 0) {
                (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Flock.FeLock = (uint8)READ_LOCK;
                (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Flock.FpNext = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpLock;
                (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpLock = p + 48;
            }
        }
        if (wrflag != 0) {
            v5 = (int32)TRANS_WRITE;
        } else {
            v5 = (int32)TRANS_READ;
        }
        (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans = libc.Uint8FromInt32(v5);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) > libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction)) {
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans;
        }
        if (wrflag != 0) {
            pPage1 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1;
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpWriter = p;
            v1 = pBt + 40;
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) & ~libc.Int32FromInt32(BTS_EXCLUSIVE));
            if (wrflag > (int32)1) {
                v1 = pBt + 40;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(BTS_EXCLUSIVE));
            }
            if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage != _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 28))) {
                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FpDbPage);
                if (rc == SQLITE_OK) {
                    _sqlite3Put4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage1)).Value.FaData + 28), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage);
                }
            }
        }
    }
    goto trans_begun;
trans_begun:;
    if (rc == SQLITE_OK) {
        if (pSchemaVersion != nil) {
            (@unsafe.Pointer.To<int32>(pSchemaVersion)).Value = libc.Int32FromUint32(_sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1)).Value.FaData + 40)));
        }
        if (wrflag != 0) {
            rc = _sqlite3PagerOpenSavepoint(tls, pPager, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb)).Value.FnSavepoint);
        }
    }
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static int32 /*r*/ _btreeNext(ж<libc.TLS> tls, uintptr pCur) {
    int32 r = default;

    int32 idx = default;
    int32 rc = default;
    int32 v1 = default;
    uintptr pPage = default;
    uintptr v3 = default;
    Tu16 v2 = default;
    _ = idx;
    _ = pPage;
    _ = rc;
    _ = v1;
    _ = v2;
    _ = v3;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) != CURSOR_VALID) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
            v1 = _btreeRestoreCursorPosition(tls, pCur);
        } else {
            v1 = SQLITE_OK;
        }
        rc = v1;
        if (rc != SQLITE_OK) {
            return rc;
        }
        if ((int32)CURSOR_INVALID == libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState)) {
            return (int32)SQLITE_DONE;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == (int32)CURSOR_SKIPNEXT) {
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_VALID;
            if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext > 0) {
                return SQLITE_OK;
            }
        }
    }
    pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
    v3 = pCur + 86;
    (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3) + 1);
    v2 = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v3);
    idx = libc.Int32FromUint16(v2);
    if (_sqlite3FaultSim(tls, (int32)412) != 0) {
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = (uint8)0;
    }
    if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit != 0)) {
        return _sqlite3CorruptError(tls, (int32)79533);
    }
    if (idx >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            rc = _moveToChild(tls, pCur, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8))));
            if (rc != 0) {
                return rc;
            }
            return _moveToLeftmost(tls, pCur);
        }
        for (var cond = true; cond; cond = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix) >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
            if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage == 0) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
                return (int32)SQLITE_DONE;
            }
            _moveToParent(tls, pCur);
            pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        }
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey != 0) {
            return _sqlite3BtreeNext(tls, pCur, 0);
        } else {
            return SQLITE_OK;
        }
    }
    if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0) {
        return SQLITE_OK;
    } else {
        return _moveToLeftmost(tls, pCur);
    }
    return r;
}

internal static int32 /*r*/ _btreeOverwriteOverflowCell(ж<libc.TLS> tls, uintptr pCur, @unsafe.Pointer pX) {
    int32 iOffset = default;
    int32 nTotal = default;
    int32 rc = default;
    Tu32 ovflPageSize = default;
    TPgno ovflPgno = default;
    uintptr pBt = default;
    ref var pPage = ref heap(new uintptr(), out var ᏑpPage);
    _ = iOffset;
    _ = nTotal;
    _ = ovflPageSize;
    _ = ovflPgno;
    _ = pBt;
    _ = pPage;
    _ = rc;
    nTotal = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 36)) + ~@unsafe.Pointer.To<int32>(@unsafe.Add(pX, 40));
    pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
    rc = _btreeOverwriteContent(tls, pPage, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FpPayload, pX, 0, libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal));
    if (rc != 0) {
        return rc;
    }
    iOffset = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnLocal);
    ovflPgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FpPayload + (uintptr)iOffset));
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
    ovflPageSize = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)4;
    for (var cond = true; cond; cond = iOffset < nTotal) {
        rc = _btreeGetPage(tls, pBt, ovflPgno, @unsafe.Pointer.FromBox(ᏑpPage), 0);
        if (rc != 0) {
            return rc;
        }
        if (_sqlite3PagerPageRefcount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage) != (int32)1 || (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit != 0) {
            rc = _sqlite3CorruptError(tls, (int32)82516);
        } else {
            if (libc.Uint32FromInt32(iOffset) + ovflPageSize < libc.Uint32FromInt32(nTotal)) {
                ovflPgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData);
            } else {
                ovflPageSize = libc.Uint32FromInt32(nTotal - iOffset);
            }
            rc = _btreeOverwriteContent(tls, pPage, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)4, pX, iOffset, libc.Int32FromUint32(ovflPageSize));
        }
        _sqlite3PagerUnref(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
        if (rc != 0) {
            return rc;
        }
        iOffset = libc.Int32FromUint32((uint32)iOffset + ovflPageSize);
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _clearCellOverflow(ж<libc.TLS> tls, uintptr pPage, uintptr pCell, @unsafe.Pointer pInfo) {
    ref var iNext = ref heap(new TPgno(), out var ᏑiNext);
    TPgno ovflPgno = default;
    int32 nOvfl = default;
    int32 rc = default;
    int32 v1 = default;
    Tu32 ovflPageSize = default;
    uintptr pBt = default;
    ref var pOvfl = ref heap(new uintptr(), out var ᏑpOvfl);
    uintptr v2 = default;
    bool v3 = default;
    _ = iNext;
    _ = nOvfl;
    _ = ovflPageSize;
    _ = ovflPgno;
    _ = pBt;
    _ = pOvfl;
    _ = rc;
    _ = v1;
    _ = v2;
    _ = v3;
    if (pCell + (uintptr)(~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pInfo, 22))) > (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd) {
        return _sqlite3CorruptError(tls, (int32)80174);
    }
    ovflPgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pCell + (uintptr)(~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pInfo, 22))) - libc.UintptrFromInt32(4)));
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
    ovflPageSize = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize - (uint32)4;
    nOvfl = libc.Int32FromUint32((~@unsafe.Pointer.To<Tu32>(@unsafe.Add(pInfo, 16)) - (uint32)(~@unsafe.Pointer.To<Tu16>(@unsafe.Add(pInfo, 20))) + ovflPageSize - (uint32)1) / ovflPageSize);
    for (;;) {
        v1 = nOvfl;
        nOvfl = nOvfl - 1;
        if (!(v1 != 0)) {
            break;
        }
        iNext = (uint32)0;
        pOvfl = (uintptr)0;
        if (ovflPgno < (uint32)2 || ovflPgno > _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
            return _sqlite3CorruptError(tls, (int32)80191);
        }
        if (nOvfl != 0) {
            rc = _getOverflowPage(tls, pBt, ovflPgno, @unsafe.Pointer.FromBox(ᏑpOvfl), @unsafe.Pointer.FromPinnedBox(ᏑiNext));
            if (rc != 0) {
                return rc;
            }
        }
        v3 = pOvfl != 0; if (!v3) {
            v2 = _btreePageLookup(tls, pBt, ovflPgno);
            pOvfl = v2;
        }
        if ((v3 || v2 != (uintptr)0) && _sqlite3PagerPageRefcount(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOvfl)).Value.FpDbPage) != (int32)1) {
            rc = _sqlite3CorruptError(tls, (int32)80211);
        } else {
            rc = _freePage2(tls, pBt, pOvfl, ovflPgno);
        }
        if (pOvfl != 0) {
            _sqlite3PagerUnref(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pOvfl)).Value.FpDbPage);
        }
        if (rc != 0) {
            return rc;
        }
        ovflPgno = iNext;
    }
    return SQLITE_OK;
}

internal static void _codeDeferredSeek(ж<libc.TLS> tls, uintptr pWInfo, uintptr pIdx, int32 iCur, int32 iIdxCur) {
    uintptr ai = default;
    uintptr pParse = default;
    uintptr pTab = default;
    uintptr v = default;
    uintptr v1 = default;
    int32 i = default;
    int32 x1 = default;
    int32 x2 = default;
    bool v2 = default;
    _ = ai;
    _ = i;
    _ = pParse;
    _ = pTab;
    _ = v;
    _ = x1;
    _ = x2;
    _ = v1;
    _ = v2;
    pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 0, 0x1);
    _sqlite3VdbeAddOp3(tls, v, (int32)OP_DeferredSeek, iIdxCur, 0, iCur);
    v2 = (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (libc.Int32FromInt32(WHERE_OR_SUBCLAUSE) | libc.Int32FromInt32(WHERE_RIGHT_JOIN))) != 0; if (v2) {
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) {
            v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel;
        } else {
            v1 = pParse;
        }
    }
    if (v2 && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)v1)).Value.FwriteMask == (uint32)0) {
        pTab = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable;
        ai = _sqlite3DbMallocZero(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (uint64)((uint64)4 * libc.Uint64FromInt32((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol + libc.Int32FromInt32(1))));
        if (ai != 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(ai))).Value = libc.Uint32FromInt16((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol);
            i = 0;
            for (;;) {
                if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn) - (int32)1)) {
                    break;
                }
                x1 = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)));
                x2 = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), pTab, (int16)x1);
                if (x1 >= 0) {
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(ai + (uintptr)(x2 + (int32)1) * 4))).Value = libc.Uint32FromInt32(i + (int32)1);
                }
                goto _3;
_3:;
                i = i + 1;
            }
            _sqlite3VdbeChangeP4(tls, v, -(int32)1, ai, -(int32)13);
        }
    }
}

internal static int32 /*r*/ _columnIsGoodIndexCandidate(ref libc.TLS tls, uintptr pTab, int32 iCol) {
    int32 j = default;
    uintptr pIdx = default;
    _ = j;
    _ = pIdx;
    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(pIdx != (uintptr)0)) {
            break;
        }
        j = 0;
        for (;;) {
            if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol))) {
                break;
            }
            if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)j * 2))) == iCol) {
                if (j == 0) {
                    return 0;
                }
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x80) >> 7) != 0 && (int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiRowLogEst + (uintptr)(j + (int32)1) * 2))) > (int32)20) {
                    return 0;
                }
                break;
            }
            goto _2;
_2:;
            j = j + 1;
        }
        goto _1;
_1:;
        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
    }
    return (int32)1;
}

internal static uintptr /*r*/ _columnName(ж<libc.TLS> tls, uintptr pStmt, int32 N, int32 useUtf16, int32 useType) {
    uintptr db = default;
    uintptr p = default;
    uintptr ret = default;
    int32 i = default;
    int32 n = default;
    int32 v1 = default;
    Tu8 prior_mallocFailed = default;
    _ = db;
    _ = i;
    _ = n;
    _ = p;
    _ = prior_mallocFailed;
    _ = ret;
    _ = v1;
    if (N < 0) {
        return (uintptr)0;
    }
    ret = (uintptr)0;
    p = pStmt;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) != 0) {
        if (useType > 0) {
            goto columnName_end;
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) == (int32)1) {
            v1 = (int32)8;
        } else {
            v1 = (int32)4;
        }
        n = v1;
        if (N >= n) {
            goto columnName_end;
        }
        if (useUtf16 != 0) {
            i = libc.Int32FromUint8(_iExplainColNames16[N + (int32)8 * (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) - (int32)8]);
            ret = (uintptr)Ꮡ_azExplainColNames16data + (uintptr)i * 2;
        } else {
            ret = _azExplainColNames8[N + (int32)8 * (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) - (int32)8];
        }
        goto columnName_end;
    }
    n = libc.Int32FromUint16((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnResColumn);
    if (N < n) {
        prior_mallocFailed = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed;
        N = N + useType * n;
        if (useUtf16 != 0) {
            ret = Xsqlite3_value_text16(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaColName + (uintptr)N * 56);
        } else {
            ret = Xsqlite3_value_text(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaColName + (uintptr)N * 56);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed) > libc.Int32FromUint8(prior_mallocFailed)) {
            _sqlite3OomClear(ref (tls).DerefOrNull(), db);
            ret = (uintptr)0;
        }
    }
    goto columnName_end;
columnName_end:;
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return ret;
}

internal static int32 /*r*/ _computeMxChoice(ref libc.TLS tls, uintptr pWInfo) {
    uintptr aFromTabs = default;
    uintptr pFactTab = default;
    uintptr pStart = default;
    uintptr pWLoop = default;
    int32 iFromIdx = default;
    int32 nDep = default;
    int32 nLoop = default;
    int32 v5 = default;
    TBitmask m = default;
    TBitmask mSeen = default;
    TBitmask mSelfJoin = default;
    TLogEst mxRun = default;
    _ = aFromTabs;
    _ = iFromIdx;
    _ = m;
    _ = mSeen;
    _ = mSelfJoin;
    _ = mxRun;
    _ = nDep;
    _ = nLoop;
    _ = pFactTab;
    _ = pStart;
    _ = pWLoop;
    _ = v5;
    nLoop = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel);
    if (nLoop >= (int32)4 && !((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pWInfo + 68)) & 0x10) >> 4) != 0) && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse)).Value.Fdb)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_StarQuery))) == (uint32)0) {
        mSelfJoin = (uint64)0;
        libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 4, 0x10);
        aFromTabs = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8;
        pStart = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpLoops;
        iFromIdx = 0;
        m = libc.Uint64FromInt32(1);
        for (;;) {
            if (!(iFromIdx < nLoop)) {
                break;
            }
            nDep = 0;
            mSeen = (uint64)0;
            pFactTab = aFromTabs + (uintptr)iFromIdx * 80;
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFactTab)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_CROSS))) != 0) {
                if (iFromIdx + (int32)3 > nLoop) {
                    break;
                }
                while (pStart != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pStart)).Value.FiTab) <= iFromIdx) {
                    pStart = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pStart)).Value.FpNextLoop;
                }
            }
            pWLoop = pStart;
            for (;;) {
                if (!(pWLoop != 0)) {
                    break;
                }
                if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(aFromTabs + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FiTab * 80))).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_CROSS))) != 0) {
                    break;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.Fprereq & m) != (uint64)0 && (TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf & mSeen) == (uint64)0 && (TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf & mSelfJoin) == (uint64)0) {
                    if ((@unsafe.Pointer.To<TSrcItem>(__ccgo_ptr(aFromTabs + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FiTab * 80))).Value.FpSTab == (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pFactTab)).Value.FpSTab) {
                        mSelfJoin = (TBitmask)(mSelfJoin | m);
                    } else {
                        nDep = nDep + 1;
                        mSeen = (TBitmask)(mSeen | (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf);
                    }
                }
                goto _2;
_2:;
                pWLoop = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FpNextLoop;
            }
            if (nDep <= (int32)2) {
                goto _1;
            }
            libc.SetBitFieldPtr8Uint32(pWInfo + 68, libc.Uint32FromInt32(1), 5, 0x20);
            mxRun = (int16)(-libc.Int32FromInt32(32768));
            pWLoop = pStart;
            for (;;) {
                if (!(pWLoop != 0)) {
                    break;
                }
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FiTab) < iFromIdx) {
                    goto _3;
                }
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FiTab) > iFromIdx) {
                    break;
                }
                if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrRun > (int32)mxRun) {
                    mxRun = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrRun;
                }
                goto _3;
_3:;
                pWLoop = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FpNextLoop;
            }
            if ((int32)mxRun < (int32)LOGEST_MAX) {
                mxRun = (TLogEst)(mxRun + 1);
            }
            pWLoop = pStart;
            for (;;) {
                if (!(pWLoop != 0)) {
                    break;
                }
                if ((TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FmaskSelf & mSeen) == (uint64)0) {
                    goto _4;
                }
                if ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FnLTerm != 0) {
                    goto _4;
                }
                if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrRun < (int32)mxRun) {
                    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FrRun = mxRun;
                }
                goto _4;
_4:;
                pWLoop = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pWLoop)).Value.FpNextLoop;
            }
            goto _1;
_1:;
            iFromIdx = iFromIdx + 1;
            m = m << (int)((uint64)1);
        }
    }
    if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pWInfo + 68)) & 0x20) >> 5) != 0) {
        v5 = (int32)18;
    } else {
        v5 = (int32)12;
    }
    return v5;
}

internal static void _convertToWithoutRowidTable(ж<libc.TLS> tls, uintptr pParse, uintptr pTab) {
    uintptr db = default;
    uintptr pIdx = default;
    uintptr pList = default;
    uintptr pPk = default;
    uintptr v = default;
    uintptr zColl = default;
    uintptr v12 = default;
    int32 i = default;
    int32 j = default;
    int32 n = default;
    int32 nExtra = default;
    int32 nPk = default;
    int32 v3 = default;
    ref var ipkToken = ref heap(new TToken(), out var ᏑipkToken);
    Tu16 v5 = default;
    _ = db;
    _ = i;
    _ = ipkToken;
    _ = j;
    _ = n;
    _ = nExtra;
    _ = nPk;
    _ = pIdx;
    _ = pList;
    _ = pPk;
    _ = v;
    _ = zColl;
    _ = v12;
    _ = v3;
    _ = v5;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x6) >> 1) != 0)) {
        i = 0;
        for (;;) {
            if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                break;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_PRIMKEY) != 0 && (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16 + 8)) & 0xf) >> 0) == OE_None) {
                libc.SetBitFieldPtr8Uint32((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16 + 8, libc.Uint32FromInt32(OE_Abort), 0, 0xf);
            }
            goto _1;
_1:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value |= (uint32)TF_HasNotNull;
    }
    if ((@unsafe.Pointer.To<_sqlite3AddColumn_type>((@unsafe.Pointer)(pParse + 240))).Value.FaddrCrTab != 0) {
        _sqlite3VdbeChangeP3(ref (tls).DerefOrNull(), v, (@unsafe.Pointer.To<_sqlite3AddColumn_type>((@unsafe.Pointer)(pParse + 240))).Value.FaddrCrTab, (int32)BTREE_BLOBKEY);
    }
    if ((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey >= 0) {
        _sqlite3TokenInit(tls, @unsafe.Pointer.FromPinnedBox(ᏑipkToken), (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey * 16))).Value.FzCnName);
        pList = _sqlite3ExprListAppend(tls, pParse, (uintptr)0, _sqlite3ExprAlloc(tls, db, (int32)TK_ID, @unsafe.Pointer.FromPinnedBox(ᏑipkToken), 0));
        if (pList == (uintptr)0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(TF_WithoutRowid));
            return;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8))).Value.FpExpr, pTab + 52);
        }
        (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8))).Value.Ffg.FsortFlags = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiPkSortOrder;
        (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey = (int16)(-(int32)1);
        _sqlite3CreateIndex(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0, pList, libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FkeyConf), (uintptr)0, (uintptr)0, 0, 0, (uint8)SQLITE_IDXTYPE_PRIMARYKEY);
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pTab + 48))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(TF_WithoutRowid));
            return;
        }
        pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
    } else {
        pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
        v3 = libc.Int32FromInt32(1);
        j = v3;
        i = v3;
        for (;;) {
            if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol))) {
                break;
            }
            if (_isDupColumn(ref (tls).DerefOrNull(), pPk, j, pPk, i) != 0) {
                (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnColumn = (Tu16)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnColumn - 1);
            } else {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FazColl + (uintptr)j * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FazColl + (uintptr)i * 8))).Value;
                (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaSortOrder + (uintptr)j))).Value = (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaSortOrder + (uintptr)i))).Value;
                v3 = j;
                j = j + 1;
                (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)v3 * 2))).Value = (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2))).Value;
            }
            goto _2;
_2:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol = libc.Uint16FromInt32(j);
    }
    libc.SetBitFieldPtr16Uint32(pPk + 100, libc.Uint32FromInt32(1), 5, 0x20);
    if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(db + 200 + 8)) & 0x6) >> 1) != 0)) {
        libc.SetBitFieldPtr16Uint32(pPk + 100, libc.Uint32FromInt32(1), 3, 0x8);
    }
    v5 = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnKeyCol;
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FnColumn = v5;
    nPk = libc.Int32FromUint16(v5);
    if (v != 0 && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.Ftnum > (uint32)0) {
        _sqlite3VdbeChangeOpcode(ref (tls).DerefOrNull(), v, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.Ftnum), (uint8)OP_Goto);
    }
    (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.Ftnum = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum;
    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(pIdx != 0)) {
            break;
        }
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
            goto _6;
        }
        v3 = libc.Int32FromInt32(0);
        n = v3;
        i = v3;
        for (;;) {
            if (!(i < nPk)) {
                break;
            }
            if (!(_isDupColumn(ref (tls).DerefOrNull(), pIdx, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol), pPk, i) != 0)) {
                n = n + 1;
            }
            goto _7;
_7:;
            i = i + 1;
        }
        if (n == 0) {
            (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol;
            goto _6;
        }
        if (_resizeIndexObject(tls, pParse, pIdx, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol) + n) != 0) {
            return;
        }
        i = 0;
        j = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
        for (;;) {
            if (!(i < nPk)) {
                break;
            }
            if (!(_isDupColumn(ref (tls).DerefOrNull(), pIdx, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol), pPk, i) != 0)) {
                (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)j * 2))).Value = (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)i * 2))).Value;
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)j * 8))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FazColl + (uintptr)i * 8))).Value;
                if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaSortOrder + (uintptr)i)) != 0) {
                    libc.SetBitFieldPtr16Uint32(pIdx + 100, libc.Uint32FromInt32(1), 9, 0x200);
                }
                j = j + 1;
            }
            goto _9;
_9:;
            i = i + 1;
        }
        goto _6;
_6:;
        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
    }
    nExtra = 0;
    i = 0;
    for (;;) {
        if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
            break;
        }
        if (!(_hasColumn(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn, nPk, i) != 0) && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) == 0) {
            nExtra = nExtra + 1;
        }
        goto _10;
_10:;
        i = i + 1;
    }
    if (_resizeIndexObject(tls, pParse, pPk, nPk + nExtra) != 0) {
        return;
    }
    i = 0;
    j = nPk;
    for (;;) {
        if (!(i < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
            break;
        }
        if (!(_hasColumn(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn, j, i) != 0) && (libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16))).Value.FcolFlags) & (int32)COLFLAG_VIRTUAL) == 0) {
            zColl = _sqlite3ColumnColl(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)i * 16);
            (@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FaiColumn + (uintptr)j * 2))).Value = (int16)i;
            if (zColl != 0) {
                v12 = zColl;
            } else {
                v12 = (uintptr)Ꮡ_sqlite3StrBINARY;
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.FazColl + (uintptr)j * 8))).Value = v12;
            j = j + 1;
        }
        goto _11;
_11:;
        i = i + 1;
    }
    _recomputeColumnsNotIndexed(ref (tls).DerefOrNull(), pPk);
}

internal static int32 /*r*/ _disableUnusedSubqueryResultColumns(ref libc.TLS tls, uintptr pItem) {
    TBitmask colUsed = default;
    TBitmask m = default;
    Tu16 iCol = default;
    int32 j = default;
    int32 nChng = default;
    int32 nCol = default;
    int32 v3 = default;
    uintptr pList = default;
    uintptr pSub = default;
    uintptr pTab = default;
    uintptr pX = default;
    uintptr pY = default;
    uint64 v5 = default;
    _ = colUsed;
    _ = iCol;
    _ = j;
    _ = m;
    _ = nChng;
    _ = nCol;
    _ = pList;
    _ = pSub;
    _ = pTab;
    _ = pX;
    _ = pY;
    _ = v3;
    _ = v5;
    nChng = 0;
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x10) >> 4) != 0 || (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x200) >> 9) != 0) {
        return 0;
    }
    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab;
    pSub = (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect;
    pX = pSub;
    for (;;) {
        if (!(pX != 0)) {
            break;
        }
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_Distinct) | libc.Int32FromInt32(SF_Aggregate))) != (uint32)0) {
            return 0;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FpPrior != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.Fop) != (int32)TK_ALL) {
            return 0;
        }
        if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FpWin != 0) {
            return 0;
        }
        goto _1;
_1:;
        pX = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FpPrior;
    }
    colUsed = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FcolUsed;
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy != 0) {
        pList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSub)).Value.FpOrderBy;
        j = 0;
        for (;;) {
            if (!(j < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                break;
            }
            iCol = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)(pList + 8 + (uintptr)j * 32 + 24));
            if (libc.Int32FromUint16(iCol) > 0) {
                iCol = (Tu16)(iCol - 1);
                if (libc.Int32FromUint16(iCol) >= libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8))) {
                    v3 = libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1);
                } else {
                    v3 = libc.Int32FromUint16(iCol);
                }
                colUsed = (TBitmask)(colUsed | libc.Uint64FromInt32(1).Lsh((int64)(v3)));
            }
            goto _2;
_2:;
            j = j + 1;
        }
    }
    nCol = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol;
    j = 0;
    for (;;) {
        if (!(j < nCol)) {
            break;
        }
        if (j < libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)) {
            v5 = libc.Uint64FromInt32(1).Lsh((int64)(j));
        } else {
            v5 = libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromUint64(libc.Uint64FromInt64(8) * libc.Uint64FromInt32(8)) - libc.Int32FromInt32(1)));
        }
        m = v5;
        if ((TBitmask)(m & colUsed) != (uint64)0) {
            goto _4;
        }
        pX = pSub;
        for (;;) {
            if (!(pX != 0)) {
                break;
            }
            pY = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FpEList + 8 + (uintptr)j * 32))).Value.FpExpr;
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pY)).Value.Fop) == (int32)TK_NULL) {
                goto _6;
            }
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pY)).Value.Fop = (uint8)TK_NULL;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pY + 4))).Value &= ~libc.Uint32FromInt32(libc.Int32FromInt32(EP_Skip) | libc.Int32FromInt32(EP_Unlikely));
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pX + 4))).Value |= (uint32)SF_PushDown;
            nChng = nChng + 1;
            goto _6;
_6:;
            pX = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pX)).Value.FpPrior;
        }
        goto _4;
_4:;
        j = j + 1;
    }
    return nChng;
}

internal static int32 /*r*/ _fromClauseTermCanBeCoroutine(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, int32 i, int32 selFlags) {
    uintptr pCteUse = default;
    uintptr pItem = default;
    _ = pCteUse;
    _ = pItem;
    pItem = pTabList + 8 + (uintptr)i * 80;
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x200) >> 9) != 0) {
        pCteUse = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 56));
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FeM10d) == M10d_Yes) {
            return 0;
        }
        if ((@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FnUse >= (int32)2 && libc.Int32FromUint8((@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)pCteUse)).Value.FeM10d) != (int32)M10d_No) {
            return 0;
        }
    }
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8))).Value.Ffg.Fjointype) & (int32)JT_LTORJ) != 0) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Coroutines))) != (uint32)0) {
        return 0;
    }
    if (_isSelfJoinView(tls, pTabList, pItem, i + (int32)1, (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc) != (uintptr)0) {
        return 0;
    }
    if (i == 0) {
        if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pTabList)).Value.FnSrc == (int32)1) {
            return (int32)1;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pTabList + 8 + 1 * 80))).Value.Ffg.Fjointype) & (int32)JT_CROSS) != 0) {
            return (int32)1;
        }
        if ((selFlags & (int32)SF_UpdateFrom) != 0) {
            return 0;
        }
        return (int32)1;
    }
    if ((selFlags & (int32)SF_UpdateFrom) != 0) {
        return 0;
    }
    while ((int32)1 != 0) {
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_OUTER) | libc.Int32FromInt32(JT_CROSS))) != 0) {
            return 0;
        }
        if (i == 0) {
            break;
        }
        i = i - 1;
        pItem -= 80;
        if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) != 0) {
            return 0;
        }
    }
    return (int32)1;
}

internal static int32 /*r*/ _geopolyColumn(ж<libc.TLS> tls, uintptr cur, uintptr ctx, int32 i) {
    uintptr p = default;
    uintptr pCsr = default;
    uintptr pNode = default;
    uintptr pRtree = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = p;
    _ = pCsr;
    _ = pNode;
    _ = pRtree;
    _ = rc;
    pRtree = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)cur)).Value.FpVtab;
    pCsr = cur;
    p = _rtreeSearchPointFirst(ref (tls).DerefOrNull(), pCsr);
    rc = SQLITE_OK;
    pNode = _rtreeNodeOfFirstSearchPoint(tls, pCsr, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
    if (rc != 0) {
        return rc;
    }
    if (p == (uintptr)0) {
        return SQLITE_OK;
    }
    if (i == 0 && Xsqlite3_vtab_nochange(tls, ctx) != 0) {
        return SQLITE_OK;
    }
    if (i <= libc.Int32FromUint16((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnAux)) {
        if (!((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FbAuxValid != 0)) {
            if ((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux == (uintptr)0) {
                rc = _sqlite3_prepare_v3__ccgo_gp(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzReadAuxSql, -(int32)1, (uint32)0, (@unsafe.Pointer)(pCsr + 56), (uintptr)0);
                if (rc != 0) {
                    return rc;
                }
            }
            Xsqlite3_bind_int64(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux, (int32)1, _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell)));
            rc = Xsqlite3_step(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux);
            if (rc == (int32)SQLITE_ROW) {
                (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FbAuxValid = (uint8)1;
            } else {
                Xsqlite3_reset(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux);
                if (rc == (int32)SQLITE_DONE) {
                    rc = SQLITE_OK;
                }
                return rc;
            }
        }
        Xsqlite3_result_value(tls, ctx, Xsqlite3_column_value(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux, i + (int32)2));
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _indexMightHelpWithOrderBy(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, uintptr pIndex, int32 iCursor) {
    uintptr aColExpr = default;
    uintptr pExpr = default;
    uintptr pOB = default;
    uintptr v1 = default;
    int32 ii = default;
    int32 jj = default;
    _ = aColExpr;
    _ = ii;
    _ = jj;
    _ = pExpr;
    _ = pOB;
    _ = v1;
    if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIndex + 100)) & 0x4) >> 2) != 0) {
        return 0;
    }
    v1 = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pBuilder)))).Value.FpOrderBy;
    pOB = v1;
    if (v1 == (uintptr)0) {
        return 0;
    }
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pOB)).Value.FnExpr)) {
            break;
        }
        pExpr = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pOB + 8 + (uintptr)ii * 32))).Value.FpExpr);
        if (pExpr == (uintptr)0) {
            goto _2;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_COLUMN || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AGG_COLUMN) && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable == iCursor) {
            if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn < 0) {
                return (int32)1;
            }
            jj = 0;
            for (;;) {
                if (!(jj < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnKeyCol))) {
                    break;
                }
                if ((int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn == (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiColumn + (uintptr)jj * 2)))) {
                    return (int32)1;
                }
                goto _3;
_3:;
                jj = jj + 1;
            }
        } else {
            v1 = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaColExpr;
            aColExpr = v1;
            if (v1 != (uintptr)0) {
                jj = 0;
                for (;;) {
                    if (!(jj < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnKeyCol))) {
                        break;
                    }
                    if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FaiColumn + (uintptr)jj * 2))) != -(int32)2) {
                        goto _5;
                    }
                    if (_sqlite3ExprCompareSkip(tls, pExpr, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(aColExpr + 8 + (uintptr)jj * 32))).Value.FpExpr, iCursor) == 0) {
                        return (int32)1;
                    }
                    goto _5;
_5:;
                    jj = jj + 1;
                }
            }
        }
        goto _2;
_2:;
        ii = ii + 1;
    }
    return 0;
}

internal static uintptr /*r*/ _isCandidateForInOpt(ref libc.TLS tls, uintptr pX) {
    int32 i = default;
    uintptr p = default;
    uintptr pEList = default;
    uintptr pRes = default;
    uintptr pSrc = default;
    uintptr pTab = default;
    _ = i;
    _ = p;
    _ = pEList;
    _ = pRes;
    _ = pSrc;
    _ = pTab;
    if (!((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fflags & libc.Uint32FromInt32(EP_xIsSelect)) != libc.Uint32FromInt32(0))) {
        return (uintptr)0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_VarSelect))) != (uint32)0) {
        return (uintptr)0;
    }
    p = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pX + 32));
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior != 0) {
        return (uintptr)0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Aggregate) != 0) {
        return (uintptr)0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit != 0) {
        return (uintptr)0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere != 0) {
        return (uintptr)0;
    }
    pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
    if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc != (int32)1) {
        return (uintptr)0;
    }
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + 24 + 4)) & 0x4) >> 2) != 0) {
        return (uintptr)0;
    }
    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8))).Value.FpSTab;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
        return (uintptr)0;
    }
    pEList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
            break;
        }
        pRes = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FpExpr;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRes)).Value.Fop) != (int32)TK_COLUMN) {
            return (uintptr)0;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return p;
}

internal static uintptr /*r*/ _isSimpleCount(ref libc.TLS tls, uintptr p, uintptr pAggInfo) {
    uintptr pExpr = default;
    uintptr pTab = default;
    _ = pExpr;
    _ = pTab;
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere != 0 || (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList)).Value.FnExpr != (int32)1 || (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc)).Value.FnSrc != (int32)1 || (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8 + 24 + 4)) & 0x4) >> 2) != 0 || (@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FnFunc != (int32)1 || (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving != 0) {
        return (uintptr)0;
    }
    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc + 8))).Value.FpSTab;
    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_NORM))) {
        return (uintptr)0;
    }
    pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList + 8))).Value.FpExpr;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_AGG_FUNCTION) {
        return (uintptr)0;
    }
    if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpAggInfo != pAggInfo) {
        return (uintptr)0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)(@unsafe.Pointer.To<TAggInfo_func>(__ccgo_ptr((@unsafe.Pointer.To<TAggInfo>((@unsafe.Pointer)pAggInfo)).Value.FaFunc))).Value.FpFunc)).Value.FfuncFlags & (uint32)SQLITE_FUNC_COUNT) == (uint32)0) {
        return (uintptr)0;
    }
    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Distinct) | libc.Int32FromInt32(EP_WinFunc))) != (uint32)0) {
        return (uintptr)0;
    }
    return pTab;
}

internal static int32 /*r*/ _moveToRoot(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pRoot = default;
    uintptr v2 = default;
    int32 rc = default;
    TPgno subpage = default;
    Ti8 v1 = default;
    _ = pRoot;
    _ = rc;
    _ = subpage;
    _ = v1;
    _ = v2;
    rc = SQLITE_OK;
    if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage >= 0) {
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage != 0) {
            _releasePageNotNull(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage);
            for (;;) {
                v2 = pCur + 84;
                (@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v2)).Value = (Ti8)(~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v2) - 1);
                v1 = ~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v2);
                if (!(v1 != 0)) {
                    break;
                }
                _releasePageNotNull(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage * 8)));
            }
            v2 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144));
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage = v2;
            pRoot = v2;
            goto skip_init;
        }
    } else {
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot == (uint32)0) {
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
            return (int32)SQLITE_EMPTY;
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == (int32)CURSOR_FAULT) {
                    return (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext;
                }
                _sqlite3BtreeClearCursor(tls, pCur);
            }
            rc = _getAndInitPage(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot, (@unsafe.Pointer)(pCur + 136), libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurPagerFlags));
            if (rc != SQLITE_OK) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
                return rc;
            }
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage = 0;
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurIntKey = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FintKey;
        }
    }
    pRoot = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FisInit) == 0 || libc.BoolInt32((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo == (uintptr)0) != libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FintKey)) {
        return _sqlite3CorruptError(tls, (int32)78788);
    }
    goto skip_init;
skip_init:;
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = (uint16)0;
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
    v2 = pCur + 1;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v2)) & ~(libc.Int32FromInt32(BTCF_AtLast) | libc.Int32FromInt32(BTCF_ValidNKey) | libc.Int32FromInt32(BTCF_ValidOvfl)));
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FnCell) > 0) {
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_VALID;
    } else {
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.Fleaf != 0)) {
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.Fpgno != (uint32)1) {
                return _sqlite3CorruptError(tls, (int32)78800);
            }
            subpage = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FhdrOffset) + (int32)8)));
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_VALID;
            rc = _moveToChild(tls, pCur, subpage);
        } else {
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
            rc = (int32)SQLITE_EMPTY;
        }
    }
    return rc;
}

internal static void _nodeGetCell(ref libc.TLS tls, @unsafe.Pointer pRtree, @unsafe.Pointer pNode, int32 iCell, @unsafe.Pointer pCell) {
    int32 ii = default;
    @unsafe.Pointer pCoord = default;
    uintptr pData = default;
    _ = ii;
    _ = pCoord;
    _ = pData;
    ii = 0;
    (@unsafe.Pointer.To<Ti64>(pCell)).Value = _nodeGetRowid(ref tls, pRtree, pNode, iCell);
    pData = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pNode, 24)) + (uintptr)(libc.Int32FromInt32(12) + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pRtree, 39))) * iCell);
    pCoord = @unsafe.Add(pCell, 8);
    for (var cond = true; cond; cond = ii < libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pRtree, 37)))) {
        _readCoord(ref tls, pData, @unsafe.Add(pCoord, (uintptr)ii * 4));
        _readCoord(ref tls, pData + (uintptr)4, @unsafe.Add(pCoord, (uintptr)(ii + (int32)1) * 4));
        pData = pData + (uintptr)8;
        ii = ii + (int32)2;
    }
}

internal static Ti64 /*r*/ _nodeGetRowid(ref libc.TLS tls, @unsafe.Pointer pRtree, @unsafe.Pointer pNode, int32 iCell) {
    return _readInt64(ref tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pNode, 24)) + (uintptr)((int32)4 + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(@unsafe.Add(pRtree, 39))) * iCell)));
}

internal static void _ptrmapPutOvflPtr(ж<libc.TLS> tls, uintptr pPage, uintptr pSrc, uintptr pCell, @unsafe.Pointer pRC) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        TPgno ovfl = default;
        TCellInfo _ᴛ1 = default;
        _ = ovfl;
        if (~@unsafe.Pointer.To<int32>(pRC) != 0) {
            return;
        }
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, pCell, bp);
        if ((uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal < (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload) {
            if ((uint64)pCell < (uint64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pSrc)).Value.FaDataEnd && (uint64)(pCell + (uintptr)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal) > (uint64)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pSrc)).Value.FaDataEnd) {
                (@unsafe.Pointer.To<int32>(pRC)).Value = _sqlite3CorruptError(tls, (int32)74764);
                return;
            }
            ovfl = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(pCell + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize) - (int32)4)));
            _ptrmapPut(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt, ovfl, (uint8)PTRMAP_OVERFLOW1, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fpgno, pRC);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _renameSetENames(ref libc.TLS tls, uintptr pEList, int32 val) {
    int32 i = default;
    _ = i;
    if (pEList != 0) {
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            libc.SetBitFieldPtr16Uint32(pEList + 8 + (uintptr)i * 32 + 16 + 4, libc.Uint32FromInt32(val & libc.Int32FromInt32(0x3)), 0, 0x3);
            goto _1;
_1:;
            i = i + 1;
        }
    }
}

internal static int32 /*r*/ _renameUnmapSelectCb(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 i = default;
    uintptr pList = default;
    uintptr pParse = default;
    uintptr pSrc = default;
    _ = i;
    _ = pList;
    _ = pParse;
    _ = pSrc;
    pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
        return (int32)WRC_Abort;
    }
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_View) | libc.Int32FromInt32(SF_CopyCte))) != 0) {
        return (int32)WRC_Prune;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList != 0) {
        pList = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
                break;
            }
            if ((@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FzEName != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x3) >> 0) == ENAME_NAME) {
                _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, (uintptr)0, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FzEName);
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc != 0) {
        pSrc = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpSrc;
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)pSrc)).Value.FnSrc)) {
                break;
            }
            _sqlite3RenameTokenRemap(ref (tls).DerefOrNull(), pParse, (uintptr)0, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80))).Value.FzName);
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 24 + 4)) & 0x800) >> 11) == 0) {
                _sqlite3WalkExpr(tls, pWalker, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 64)));
            } else {
                _unmapColumnIdlistNames(ref (tls).DerefOrNull(), pParse, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pSrc + 8 + (uintptr)i * 80 + 64)));
            }
            goto _2;
_2:;
            i = i + 1;
        }
    }
    _renameWalkWith(tls, pWalker, p);
    return WRC_Continue;
}

internal static int32 /*r*/ _resolveAsName(ж<libc.TLS> tls, uintptr pParse, uintptr pEList, uintptr pE) {
    int32 i = default;
    uintptr zCol = default;
    _ = i;
    _ = zCol;
    _ = pParse;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE)).Value.Fop) == (int32)TK_ID) {
        zCol = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pE + 8));
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEList)).Value.FnExpr)) {
                break;
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x3) >> 0) == ENAME_NAME && Xsqlite3_stricmp(tls, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEList + 8 + (uintptr)i * 32))).Value.FzEName, zCol) == 0) {
                return i + (int32)1;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    }
    return 0;
}

internal static void _resolveP2Values(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer pMaxVtabArgs) {
    uintptr aLabel = default;
    uintptr pOp = default;
    uintptr pParse = default;
    int32 n = default;
    int32 nMaxVtabArgs = default;
    _ = aLabel;
    _ = n;
    _ = nMaxVtabArgs;
    _ = pOp;
    _ = pParse;
    nMaxVtabArgs = ~@unsafe.Pointer.To<int32>(pMaxVtabArgs);
    pParse = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpParse;
    aLabel = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FaLabel;
    libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 6, 0x40);
    libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(0), 7, 0x80);
    pOp = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp + (uintptr)((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp - (int32)1) * 24;
    while ((int32)1 != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fopcode) <= (int32)SQLITE_MX_JUMP_OPCODE) {
            var exprᴛ1 = libc.Int32FromUint8((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fopcode);
            var matchᴛ1 = false;
            if (exprᴛ1 == (int32)OP_Transaction) { matchᴛ1 = true;
                if ((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2 != 0) {
                    libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(0), 6, 0x40);
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)OP_AutoCommit) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == OP_Savepoint) { matchᴛ1 = true;
                libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 7, 0x80);
            }
            else if (exprᴛ1 == (int32)OP_Checkpoint) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)OP_Vacuum) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)OP_JournalMode) { matchᴛ1 = true;
                libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(0), 6, 0x40);
                libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(1), 7, 0x80);
            }
            else if (exprᴛ1 == (int32)OP_Init) { matchᴛ1 = true;
                goto resolve_p2_values_loop_exit;
            }
            else if (exprᴛ1 == (int32)OP_VUpdate) { matchᴛ1 = true;
                if ((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2 > nMaxVtabArgs) {
                    nMaxVtabArgs = (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2;
                }
            }
            else if (exprᴛ1 == (int32)OP_VFilter) { matchᴛ1 = true;
                n = (@unsafe.Pointer.To<TOp>(__ccgo_ptr(pOp + (uintptr)(-libc.Int32FromInt32(1)) * 24))).Value.Fp1;
                if (n > nMaxVtabArgs) {
                    nMaxVtabArgs = n;
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                do {
                    if ((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2 < 0) {
                        (@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aLabel + (uintptr)(~(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2) * 4));
                    }
                    break;
                } while (false);
            }

        }
        pOp -= 24;
    }
    goto resolve_p2_values_loop_exit;
resolve_p2_values_loop_exit:;
    if (aLabel != 0) {
        _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FaLabel);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FaLabel = (uintptr)0;
    }
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnLabel = 0;
    (@unsafe.Pointer.To<int32>(pMaxVtabArgs)).Value = nMaxVtabArgs;
}

internal static int32 /*r*/ _rtreeColumn(ж<libc.TLS> tls, uintptr cur, uintptr ctx, int32 i) {
    ref var c = ref heap(new TRtreeCoord(), out var Ꮡc);
    uintptr p = default;
    uintptr pCsr = default;
    uintptr pNode = default;
    uintptr pRtree = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = c;
    _ = p;
    _ = pCsr;
    _ = pNode;
    _ = pRtree;
    _ = rc;
    pRtree = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)cur)).Value.FpVtab;
    pCsr = cur;
    p = _rtreeSearchPointFirst(ref (tls).DerefOrNull(), pCsr);
    rc = SQLITE_OK;
    pNode = _rtreeNodeOfFirstSearchPoint(tls, pCsr, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
    if (rc != 0) {
        return rc;
    }
    if (p == (uintptr)0) {
        return SQLITE_OK;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell) >= _readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2)) {
        return (int32)SQLITE_ABORT;
    }
    if (i == 0) {
        Xsqlite3_result_int64(tls, ctx, _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell)));
    } else {
        if (i <= libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2)) {
            _nodeGetCoord(ref (tls).DerefOrNull(), pRtree, pNode, libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell), i - (int32)1, @unsafe.Pointer.FromPinnedBox(Ꮡc));
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
                Xsqlite3_result_double(tls, ctx, (float64)(~Ꮡc.Reinterpret<TRtreeCoord, TRtreeValue>()));
            } else {
                Xsqlite3_result_int(tls, ctx, ~Ꮡc.Reinterpret<TRtreeCoord, int32>());
            }
        } else {
            if (!((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FbAuxValid != 0)) {
                if ((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux == (uintptr)0) {
                    rc = _sqlite3_prepare_v3__ccgo_gp(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzReadAuxSql, -(int32)1, (uint32)0, (@unsafe.Pointer)(pCsr + 56), (uintptr)0);
                    if (rc != 0) {
                        return rc;
                    }
                }
                Xsqlite3_bind_int64(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux, (int32)1, _nodeGetRowid(ref (tls).DerefOrNull(), (@unsafe.Pointer)pRtree, (@unsafe.Pointer)pNode, libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)p)).Value.FiCell)));
                rc = Xsqlite3_step(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux);
                if (rc == (int32)SQLITE_ROW) {
                    (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FbAuxValid = (uint8)1;
                } else {
                    Xsqlite3_reset(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux);
                    if (rc == (int32)SQLITE_DONE) {
                        rc = SQLITE_OK;
                    }
                    return rc;
                }
            }
            Xsqlite3_result_value(tls, ctx, Xsqlite3_column_value(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux, i - libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2) + (int32)1));
        }
    }
    return SQLITE_OK;
}

internal static void _sqlite3AddNotNull(ref libc.TLS tls, uintptr pParse, int32 onError) {
    uintptr p = default;
    uintptr pCol = default;
    uintptr pIdx = default;
    _ = p;
    _ = pCol;
    _ = pIdx;
    p = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTable;
    if (p == (uintptr)0 || (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)p)).Value.FnCol < (int32)1) {
        return;
    }
    pCol = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)p)).Value.FaCol + (uintptr)((int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)p)).Value.FnCol - (int32)1) * 16;
    libc.SetBitFieldPtr8Uint32(pCol + 8, (uint32)libc.Uint8FromInt32(onError), 0, 0xf);
    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 48))).Value |= (uint32)TF_HasNotNull;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_UNIQUE) != 0) {
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)p)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn))) == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)p)).Value.FnCol - (int32)1) {
                libc.SetBitFieldPtr16Uint32(pIdx + 100, libc.Uint32FromInt32(1), 3, 0x8);
            }
            goto _1;
_1:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        }
    }
}

internal static int32 /*r*/ _sqlite3BtreeCount(ж<libc.TLS> tls, uintptr db, uintptr pCur, @unsafe.Pointer pnEntry) {
    int32 iIdx = default;
    int32 rc = default;
    Ti64 nEntry = default;
    uintptr pPage = default;
    _ = iIdx;
    _ = nEntry;
    _ = pPage;
    _ = rc;
    nEntry = 0;
    rc = _moveToRoot(tls, pCur);
    if (rc == (int32)SQLITE_EMPTY) {
        (@unsafe.Pointer.To<Ti64>(pnEntry)).Value = 0;
        return SQLITE_OK;
    }
    while (rc == SQLITE_OK && !(libc.AtomicLoadNInt32(db + 440, libc.Int32FromInt32(__ATOMIC_RELAXED)) != 0)) {
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0 || !((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FintKey != 0)) {
            nEntry = nEntry + libc.Int64FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
        }
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0) {
            for (var cond = true; cond; cond = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix) >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FnCell)) {
                if ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage == 0) {
                    (@unsafe.Pointer.To<Ti64>(pnEntry)).Value = nEntry;
                    return _moveToRoot(tls, pCur);
                }
                _moveToParent(tls, pCur);
            }
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = (Tu16)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix + 1);
            pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        }
        iIdx = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix);
        if (iIdx == libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
            rc = _moveToChild(tls, pCur, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + (int32)8))));
        } else {
            rc = _moveToChild(tls, pCur, _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * iIdx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * iIdx) + 1))))))));
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeDelete(ж<libc.TLS> tls, uintptr pCur, Tu8 flags) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        Tu8 bPreserve = default;
        int32 iCellDepth = default;
        int32 iCellIdx = default;
        int32 nCell = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        TPgno n = default;
        uintptr p = default;
        uintptr pBt = default;
        uintptr pCell = default;
        uintptr pLeaf = default;
        uintptr pPage = default;
        uintptr pTmp = default;
        uintptr v2 = default;
        Ti8 v1 = default;
        TCellInfo _ᴛ1 = default;
        _ = bPreserve;
        _ = iCellDepth;
        _ = iCellIdx;
        _ = n;
        _ = nCell;
        _ = p;
        _ = pBt;
        _ = pCell;
        _ = pLeaf;
        _ = pPage;
        _ = pTmp;
        _ = rc;
        _ = v1;
        _ = v2;
        p = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBtree;
        pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) != CURSOR_VALID) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
                rc = _btreeRestoreCursorPosition(tls, pCur);
                if (rc != 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) != CURSOR_VALID) {
                    r = rc; goto ᒐdone;
                }
            } else {
                r = _sqlite3CorruptError(tls, (int32)83043); goto ᒐdone;
            }
        }
        iCellDepth = (int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage;
        iCellIdx = libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix);
        pPage = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) <= iCellIdx) {
            r = _sqlite3CorruptError(tls, (int32)83052); goto ᒐdone;
        }
        pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * iCellIdx)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)((int32)2 * iCellIdx) + 1)))));
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree < 0 && _btreeComputeFreeSpace(tls, pPage) != 0) {
            r = _sqlite3CorruptError(tls, (int32)83056); goto ᒐdone;
        }
        if (pCell < (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) {
            r = _sqlite3CorruptError(tls, (int32)83059); goto ᒐdone;
        }
        bPreserve = libc.BoolUint8((libc.Int32FromUint8(flags) & (int32)BTREE_SAVEPOSITION) != 0);
        if (bPreserve != 0) {
            if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0) || (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree + libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxCellSize)(tls, pPage, pCell)) + (int32)2 > libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize * libc.Uint32FromInt32(2) / libc.Uint32FromInt32(3)) || libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) == (int32)1) {
                rc = _saveCursorKey(tls, pCur);
                if (rc != 0) {
                    r = rc; goto ᒐdone;
                }
            } else {
                bPreserve = (uint8)2;
            }
        }
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            rc = _sqlite3BtreePrevious(tls, pCur, 0);
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_Multiple) != 0) {
            rc = _saveAllCursors(tls, pBt, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot, pCur);
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
        }
        if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo == (uintptr)0 && (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FhasIncrblobCur != 0) {
            _invalidateIncrblobCursors(ref (tls).DerefOrNull(), p, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnKey, 0);
        }
        rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage);
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uintptr>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FxParseCell)(tls, pPage, pCell, bp);
        if ((uint32)(@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnLocal != (@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnPayload) {
            rc = _clearCellOverflow(tls, pPage, pCell, __ccgo_ptr(bp));
        } else {
            rc = SQLITE_OK;
        }
        _dropCell(tls, pPage, iCellIdx, libc.Int32FromUint16((@unsafe.Pointer.To<TCellInfo>(__ccgo_ptr(bp))).Value.FnSize), @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (rc != 0) {
            r = rc; goto ᒐdone;
        }
        if (!((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fleaf != 0)) {
            pLeaf = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage;
            if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FnFree < 0) {
                rc = _btreeComputeFreeSpace(tls, pLeaf);
                if (rc != 0) {
                    r = rc; goto ᒐdone;
                }
            }
            if (iCellDepth < (int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage - (int32)1) {
                n = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)(iCellDepth + (int32)1) * 8))))).Value.Fpgno;
            } else {
                n = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.Fpgno;
            }
            pCell = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FaData + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FmaskPage) & (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FaCellIdx + (uintptr)((int32)2 * (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FnCell) - (int32)1))))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FaCellIdx + (uintptr)((int32)2 * (libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FnCell) - (int32)1)) + 1)))));
            if (pCell < (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FaData + 4) {
                r = _sqlite3CorruptError(tls, (int32)83150); goto ᒐdone;
            }
            nCell = libc.Int32FromUint16(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tu16>>((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FxCellSize)(tls, pLeaf, pCell));
            pTmp = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpTmpSpace;
            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FpDbPage);
            if (rc == SQLITE_OK) {
                rc = _insertCell(tls, pPage, iCellIdx, pCell - (uintptr)4, nCell + (int32)4 - libc.BoolInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(pCell))) == (int32)2), pTmp, n);
            }
            _dropCell(tls, pLeaf, libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pLeaf)).Value.FnCell) - (int32)1, nCell, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            if (rc != 0) {
                r = rc; goto ᒐdone;
            }
        }
        if ((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.FnFree * (int32)3 <= libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt)).Value.FusableSize) * (int32)2) {
            rc = SQLITE_OK;
        } else {
            rc = _balance(tls, pCur);
        }
        if (rc == SQLITE_OK && (int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage > iCellDepth) {
            _releasePageNotNull(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage);
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage = (Ti8)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage - 1);
            while ((int32)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage > iCellDepth) {
                v2 = pCur + 84;
                v1 = ~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v2);
                (@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v2)).Value = (Ti8)(~@unsafe.Pointer.To<Ti8>((@unsafe.Pointer)v2) - 1);
                _releasePage(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)v1 * 8)));
            }
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pCur + 144 + (uintptr)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FiPage * 8));
            rc = _balance(tls, pCur);
        }
        if (rc == SQLITE_OK) {
            if (libc.Int32FromUint8(bPreserve) > (int32)1) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_SKIPNEXT;
                if (iCellIdx >= libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell)) {
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext = -(int32)1;
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell) - (int32)1);
                } else {
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext = (int32)1;
                }
            } else {
                rc = _moveToRoot(tls, pCur);
                if (bPreserve != 0) {
                    _btreeReleaseAllCursorPages(tls, pCur);
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_REQUIRESEEK;
                }
                if (rc == (int32)SQLITE_EMPTY) {
                    rc = SQLITE_OK;
                }
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _sqlite3ColumnType(ж<libc.TLS> tls, uintptr pCol, uintptr zDflt) {
    uintptr r = default;

    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FcolFlags) & (int32)COLFLAG_HASTYPE) != 0) {
        return (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName + (uintptr)libc.Xstrlen(tls, (@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)pCol)).Value.FzCnName) + (uintptr)1;
    } else {
        if ((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pCol + 8)) & 0xf0) >> 4) != 0) {
            return _sqlite3StdType[(int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pCol + 8)) & 0xf0) >> 4) - (int32)1];
        } else {
            return zDflt;
        }
    }
    return r;
}

internal static void _sqlite3CompleteInsertion(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 iDataCur, int32 iIdxCur, int32 regNewData, uintptr aRegIdx, int32 update_flags, int32 appendBias, int32 useSeekResult) {
    int32 i = default;
    int32 v2 = default;
    uintptr pIdx = default;
    uintptr v = default;
    Tu8 pik_flags = default;
    _ = i;
    _ = pIdx;
    _ = pik_flags;
    _ = v;
    _ = v2;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    i = 0;
    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(pIdx != 0)) {
            break;
        }
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)) == 0) {
            goto _1;
        }
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != 0 || update_flags != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x800) >> 11) != 0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)), _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2);
        }
        if (useSeekResult != 0) {
            v2 = (int32)OPFLAG_USESEEKRESULT;
        } else {
            v2 = 0;
        }
        pik_flags = libc.Uint8FromInt32(v2);
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY && !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
            pik_flags = libc.Uint8FromInt32((int32)pik_flags | libc.Int32FromInt32(OPFLAG_NCHANGE));
            pik_flags = libc.Uint8FromInt32((int32)pik_flags | update_flags & libc.Int32FromInt32(OPFLAG_SAVEPOSITION));
            if (update_flags == 0) {
                _codeWithoutRowidPreupdate(tls, pParse, pTab, iIdxCur + i, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)));
            }
        }
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x8) >> 3) != 0) {
            v2 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
        } else {
            v2 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
        }
        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_IdxInsert, iIdxCur + i, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)), ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)) + (int32)1, v2);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)pik_flags);
        goto _1;
_1:;
        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        i = i + 1;
    }
    if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
        return;
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0) {
        pik_flags = (uint8)0;
    } else {
        pik_flags = (uint8)OPFLAG_NCHANGE;
        if (update_flags != 0) {
            v2 = update_flags;
        } else {
            v2 = (int32)OPFLAG_LASTROWID;
        }
        pik_flags = libc.Uint8FromInt32((int32)pik_flags | v2);
    }
    if (appendBias != 0) {
        pik_flags = libc.Uint8FromInt32((int32)pik_flags | libc.Int32FromInt32(OPFLAG_APPEND));
    }
    if (useSeekResult != 0) {
        pik_flags = libc.Uint8FromInt32((int32)pik_flags | libc.Int32FromInt32(OPFLAG_USESEEKRESULT));
    }
    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Insert, iDataCur, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)), regNewData);
    if (!((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0)) {
        _sqlite3VdbeAppendP4(tls, v, pTab, -(int32)5);
    }
    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)pik_flags);
}

internal static uintptr /*r*/ _sqlite3ExpandReturning(ж<libc.TLS> tls, uintptr pParse, uintptr pList, uintptr pTab) {
    uintptr db = default;
    uintptr pItem = default;
    uintptr pItem1 = default;
    uintptr pNew = default;
    uintptr pNewExpr = default;
    uintptr pNewExpr1 = default;
    uintptr pOldExpr = default;
    int32 i = default;
    int32 jj = default;
    _ = db;
    _ = i;
    _ = jj;
    _ = pItem;
    _ = pItem1;
    _ = pNew;
    _ = pNewExpr;
    _ = pNewExpr1;
    _ = pOldExpr;
    pNew = (uintptr)0;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr)) {
            break;
        }
        pOldExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FpExpr;
        if (pOldExpr == (uintptr)0) {
            goto _1;
        }
        if (_isAsteriskTerm(tls, pParse, pOldExpr) != 0) {
            jj = 0;
            for (;;) {
                if (!(jj < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
                    break;
                }
                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TColumn>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)jj * 16))).Value.FcolFlags) & (int32)COLFLAG_HIDDEN) != 0) {
                    goto _2;
                }
                pNewExpr = _sqlite3Expr(tls, db, (int32)TK_ID, (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)jj * 16))).Value.FzCnName);
                pNew = _sqlite3ExprListAppend(tls, pParse, pNew, pNewExpr);
                if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0)) {
                    pItem = pNew + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr - (int32)1) * 32;
                    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FzEName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FaCol + (uintptr)jj * 16))).Value.FzCnName);
                    libc.SetBitFieldPtr16Uint32(pItem + 16 + 4, libc.Uint32FromInt32(ENAME_NAME), 0, 0x3);
                }
                goto _2;
_2:;
                jj = jj + 1;
            }
        } else {
            pNewExpr1 = _sqlite3ExprDup(tls, db, pOldExpr, 0);
            pNew = _sqlite3ExprListAppend(tls, pParse, pNew, pNewExpr1);
            if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) && (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FzEName != (uintptr)0) {
                pItem1 = pNew + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pNew)).Value.FnExpr - (int32)1) * 32;
                (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem1)).Value.FzEName = _sqlite3DbStrDup(tls, db, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32))).Value.FzEName);
                libc.SetBitFieldPtr16Uint32(pItem1 + 16 + 4, libc.Uint32FromInt32((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pList + 8 + (uintptr)i * 32 + 16 + 4)) & 0x3) >> 0)), 0, 0x3);
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return pNew;
}

internal static void _sqlite3ExpirePreparedStatements(ref libc.TLS tls, uintptr db, int32 iCode) {
    uintptr p = default;
    _ = p;
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVdbe;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        libc.SetBitFieldPtr16Uint32(p + 200, libc.Uint32FromInt32(iCode + libc.Int32FromInt32(1)), 0, 0x3);
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpVNext;
    }
}

internal static int32 /*r*/ _sqlite3ExprCanBeNull(ref libc.TLS tls, uintptr p) {
    int32 r = default;

    Tu8 op = default;
    _ = op;
    while (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UPLUS || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UMINUS) {
        p = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
    }
    op = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop;
    if (libc.Int32FromUint8(op) == (int32)TK_REGISTER) {
        op = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop2;
    }
    var exprᴛ1 = libc.Int32FromUint8(op);
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)TK_INTEGER) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_STRING) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_FLOAT) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_BLOB) { matchᴛ1 = true;
        return 0;
    }
    if (exprᴛ1 == (int32)TK_COLUMN) {
        return libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_CanBeNull))) != (uint32)0 || ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64)) == (uintptr)0 || (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn >= 0 && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64))))).Value.FaCol != (uintptr)0 && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn < (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64))))).Value.FnCol && (int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 64))))).Value.FaCol + (uintptr)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiColumn * 16 + 8)) & 0xf) >> 0) == 0);
    }
    if (!matchᴛ1) { /* default: */
        return (int32)1;
    }
    return default;

    return r;
}

internal static int32 /*r*/ _sqlite3ExprCodeExprList(ж<libc.TLS> tls, uintptr pParse, uintptr pList, int32 target, int32 srcReg, Tu8 flags) {
    Tu8 copyOp = default;
    int32 i = default;
    int32 inReg = default;
    int32 j = default;
    int32 n = default;
    int32 v1 = default;
    uintptr pExpr = default;
    uintptr pItem = default;
    uintptr pOp = default;
    uintptr v = default;
    uintptr v5 = default;
    bool v4 = default;
    _ = copyOp;
    _ = i;
    _ = inReg;
    _ = j;
    _ = n;
    _ = pExpr;
    _ = pItem;
    _ = pOp;
    _ = v;
    _ = v1;
    _ = v4;
    _ = v5;
    if ((libc.Int32FromUint8(flags) & (int32)SQLITE_ECEL_DUP) != 0) {
        v1 = (int32)OP_Copy;
    } else {
        v1 = (int32)OP_SCopy;
    }
    copyOp = libc.Uint8FromInt32(v1);
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    n = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pList)).Value.FnExpr;
    if (!((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x80) >> 7)) != 0)) {
        flags = libc.Uint8FromInt32((int32)flags & ~libc.Int32FromInt32(SQLITE_ECEL_FACTOR));
    }
    pItem = pList + 8;
    i = libc.Int32FromInt32(0);
    for (;;) {
        if (!(i < n)) {
            break;
        }
        pExpr = (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr;
        v4 = (libc.Int32FromUint8(flags) & (int32)SQLITE_ECEL_REF) != 0; if (v4) {
            v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TExprList_itemᴛ1_Fu_Fx>((@unsafe.Pointer)(pItem + 24))).Value.FiOrderByCol);
            j = v1;
        }
        if (v4 && v1 > 0) {
            if ((libc.Int32FromUint8(flags) & (int32)SQLITE_ECEL_OMITREF) != 0) {
                i = i - 1;
                n = n - 1;
            } else {
                _sqlite3VdbeAddOp2(tls, v, libc.Int32FromUint8(copyOp), j + srcReg - (int32)1, target + i);
            }
        } else {
            if ((libc.Int32FromUint8(flags) & (int32)SQLITE_ECEL_FACTOR) != 0 && _sqlite3ExprIsConstantNotJoin(tls, pParse, pExpr) != 0) {
                _sqlite3ExprCodeRunJustOnce(tls, pParse, pExpr, target + i);
            } else {
                inReg = _sqlite3ExprCodeTarget(tls, pParse, pExpr, target + i);
                if (inReg != target + i) {
                    v4 = libc.Int32FromUint8(copyOp) == (int32)OP_Copy; if (v4) {
                        v5 = _sqlite3VdbeGetLastOp(ref (tls).DerefOrNull(), v);
                        pOp = v5;
                    }
                    if (v4 && libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)v5)).Value.Fopcode) == (int32)OP_Copy && (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1 + (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 + (int32)1 == inReg && (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2 + (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 + (int32)1 == target + i && libc.Int32FromUint16((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp5) == 0) {
                        (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 = (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 + 1;
                    } else {
                        _sqlite3VdbeAddOp2(tls, v, libc.Int32FromUint8(copyOp), inReg, target + i);
                    }
                }
            }
        }
        goto _2;
_2:;
        i = i + 1;
        pItem += 32;
    }
    return n;
}

internal static int32 /*r*/ _sqlite3ExprCodeRunJustOnce(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 regDest) {
    int32 addr = default;
    int32 i = default;
    int32 v2 = default;
    uintptr p = default;
    uintptr pItem = default;
    uintptr pItem1 = default;
    uintptr v = default;
    uintptr v3 = default;
    _ = addr;
    _ = i;
    _ = p;
    _ = pItem;
    _ = pItem1;
    _ = v;
    _ = v2;
    _ = v3;
    p = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpConstExpr;
    if (regDest < 0 && p != 0) {
        pItem = p + 8;
        i = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)p)).Value.FnExpr;
        for (;;) {
            if (!(i > 0)) {
                break;
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pItem + 16 + 4)) & 0x8) >> 3) != 0 && _sqlite3ExprCompare(tls, (uintptr)0, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.FpExpr, pExpr, -(int32)1) == 0) {
                return ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pItem + 24));
            }
            goto _1;
_1:;
            pItem += 32;
            i = i - 1;
        }
    }
    pExpr = _sqlite3ExprDup(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr, 0);
    if (pExpr != (uintptr)0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_HasFunc))) != (uint32)0) {
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        addr = _sqlite3VdbeAddOp0(tls, v, (int32)OP_Once);
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(0), 7, 0x80);
        if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmallocFailed != 0)) {
            if (regDest < 0) {
                v3 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                regDest = v2;
            }
            _sqlite3ExprCode(tls, pParse, pExpr, regDest);
        }
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 7, 0x80);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr);
    } else {
        p = _sqlite3ExprListAppend(tls, pParse, p, pExpr);
        if (p != 0) {
            pItem1 = p + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)p)).Value.FnExpr - (int32)1) * 32;
            libc.SetBitFieldPtr16Uint32(pItem1 + 16 + 4, libc.BoolUint32(regDest < libc.Int32FromInt32(0)), 3, 0x8);
            if (regDest < 0) {
                v3 = pParse + 60;
                (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3) + 1;
                v2 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v3);
                regDest = v2;
            }
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pItem1 + 24))).Value = regDest;
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpConstExpr = p;
    }
    return regDest;
}

internal static int32 /*r*/ _sqlite3ExprCodeTemp(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, @unsafe.Pointer pReg) {
    int32 r1 = default;
    int32 r2 = default;
    _ = r1;
    _ = r2;
    pExpr = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), pExpr);
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x80) >> 7)) != 0 && pExpr != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_REGISTER && _sqlite3ExprIsConstantNotJoin(tls, pParse, pExpr) != 0) {
        (@unsafe.Pointer.To<int32>(pReg)).Value = 0;
        r2 = _sqlite3ExprCodeRunJustOnce(tls, pParse, pExpr, -(int32)1);
    } else {
        r1 = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
        r2 = _sqlite3ExprCodeTarget(tls, pParse, pExpr, r1);
        if (r2 == r1) {
            (@unsafe.Pointer.To<int32>(pReg)).Value = r1;
        } else {
            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, r1);
            (@unsafe.Pointer.To<int32>(pReg)).Value = 0;
        }
    }
    return r2;
}

internal static void _sqlite3ExprListSetSortOrder(ref libc.TLS tls, uintptr p, int32 iSortOrder, int32 eNulls) {
    uintptr pItem = default;
    uintptr v1 = default;
    _ = pItem;
    _ = v1;
    if (p == (uintptr)0) {
        return;
    }
    pItem = p + 8 + (uintptr)((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)p)).Value.FnExpr - (int32)1) * 32;
    if (iSortOrder == -(int32)1) {
        iSortOrder = SQLITE_SO_ASC;
    }
    (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)pItem)).Value.Ffg.FsortFlags = libc.Uint8FromInt32(iSortOrder);
    if (eNulls != -(int32)1) {
        libc.SetBitFieldPtr16Uint32(pItem + 16 + 4, libc.Uint32FromInt32(1), 5, 0x20);
        if (iSortOrder != eNulls) {
            v1 = pItem + 16;
            (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(KEYINFO_ORDER_BIGNULL));
        }
    }
}

internal static void _sqlite3FinishCoding(ж<libc.TLS> tls, uintptr pParse) {
    int32 addrRewind = default;
    int32 i = default;
    int32 iDb = default;
    int32 reg = default;
    int32 v2 = default;
    uintptr db = default;
    uintptr pEL = default;
    uintptr pRet = default;
    uintptr pReturning = default;
    uintptr pSchema = default;
    uintptr v = default;
    uintptr vtab = default;
    _ = addrRewind;
    _ = db;
    _ = i;
    _ = iDb;
    _ = pEL;
    _ = pRet;
    _ = pReturning;
    _ = pSchema;
    _ = reg;
    _ = v;
    _ = vtab;
    _ = v2;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested != 0) {
        return;
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_NOMEM;
        }
        return;
    }
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (v == (uintptr)0) {
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_DONE;
            return;
        }
        v = _sqlite3GetVdbe(tls, pParse);
        if (v == (uintptr)0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_ERROR;
        }
    }
    if (v != 0) {
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x8) >> 3)) != 0) {
            pReturning = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse).of(TParse.ᏑFu1).Reinterpret<TParseᴛ1_Fu1, _sqlite3AddReturning_type>()).Value.FpReturning;
            if ((@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FnRetCol != 0) {
                _sqlite3VdbeAddOp0(tls, v, (int32)OP_FkCheck);
                addrRewind = _sqlite3VdbeAddOp1(tls, v, (int32)OP_Rewind, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetCur);
                reg = (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetReg;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FnRetCol)) {
                        break;
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Column, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetCur, i, reg + i);
                    goto _1;
_1:;
                    i = i + 1;
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_ResultRow, reg, i);
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pReturning)).Value.FiRetCur, addrRewind + (int32)1);
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrRewind);
            }
        }
        _sqlite3VdbeAddOp0(tls, v, (int32)OP_Halt);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, 0);
        iDb = 0;
        for (;;) {
            if (libc.BoolInt32((TyDbMask)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FcookieMask & (libc.Uint32FromInt32(1).Lsh((int64)(iDb)))) != (uint32)0) == 0) {
                goto _3;
            }
            _sqlite3VdbeUsesBtree(ref (tls).DerefOrNull(), v, iDb);
            pSchema = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FpSchema;
            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Transaction, iDb, libc.BoolInt32((TyDbMask)((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FwriteMask & (libc.Uint32FromInt32(1).Lsh((int64)(iDb)))) != (uint32)0), (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.Fschema_cookie, (@unsafe.Pointer.To<TSchema>((@unsafe.Pointer)pSchema)).Value.FiGeneration);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy) == 0) {
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)1);
            }
            goto _3;
_3:;
            iDb = iDb + 1;
            v2 = iDb;
            if (!(v2 < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDb)) {
                break;
            }
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnVtabLock)) {
                break;
            }
            vtab = _sqlite3GetVTable(ref (tls).DerefOrNull(), db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FapVtabLock + (uintptr)i * 8)));
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_VBegin, 0, 0, 0, vtab, -(int32)12);
            goto _4;
_4:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnVtabLock = 0;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTableLock != 0) {
            _codeTableLocks(tls, pParse);
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x200) >> 9)) != 0) {
            _sqlite3AutoincrementBegin(tls, pParse);
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpConstExpr != 0) {
            pEL = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpConstExpr;
            libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(0), 7, 0x80);
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pEL)).Value.FnExpr)) {
                    break;
                }
                _sqlite3ExprCode(tls, pParse, (@unsafe.Pointer.To<TExprList_item>((@unsafe.Pointer)(pEL + 8 + (uintptr)i * 32))).Value.FpExpr, ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)(pEL + 8 + (uintptr)i * 32 + 24)));
                goto _5;
_5:;
                i = i + 1;
            }
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x8) >> 3)) != 0) {
            pRet = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse).of(TParse.ᏑFu1).Reinterpret<TParseᴛ1_Fu1, _sqlite3AddReturning_type>()).Value.FpReturning;
            if ((@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FnRetCol != 0) {
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_OpenEphemeral, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FiRetCur, (@unsafe.Pointer.To<TReturning>((@unsafe.Pointer)pRet)).Value.FnRetCol);
            }
        }
        _sqlite3VdbeGoto(tls, v, (int32)1);
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
        _sqlite3VdbeMakeReady(tls, v, pParse);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_DONE;
    } else {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_ERROR;
    }
}

internal static void _sqlite3FkCheck(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 regOld, int32 regNew, uintptr aChange, int32 bChngRowid) {
    @unsafe.Pointer aiCol = default;
    ref var aiCol1 = ref heap(new uintptr(), out var ᏑaiCol1);
    ref var aiFree = ref heap(new uintptr(), out var ᏑaiFree);
    uintptr db = default;
    uintptr pFKey = default;
    ref var pIdx = ref heap(new uintptr(), out var ᏑpIdx);
    ref var pIdx1 = ref heap(new uintptr(), out var ᏑpIdx1);
    uintptr pItem = default;
    uintptr pSrc = default;
    uintptr pTo = default;
    uintptr v = default;
    uintptr zCol = default;
    uintptr zDb = default;
    uintptr v7 = default;
    int32 bIgnore = default;
    int32 eAction = default;
    int32 i = default;
    ref var iCol = ref heap(new int32(), out var ᏑiCol);
    int32 iDb = default;
    int32 iFromCol = default;
    int32 iJump = default;
    int32 iReg = default;
    int32 isIgnoreErrors = default;
    int32 rcauth = default;
    int32 v4 = default;
    _ = aiCol;
    _ = aiCol1;
    _ = aiFree;
    _ = bIgnore;
    _ = db;
    _ = eAction;
    _ = i;
    _ = iCol;
    _ = iDb;
    _ = iFromCol;
    _ = iJump;
    _ = iReg;
    _ = isIgnoreErrors;
    _ = pFKey;
    _ = pIdx;
    _ = pIdx1;
    _ = pItem;
    _ = pSrc;
    _ = pTo;
    _ = rcauth;
    _ = v;
    _ = zCol;
    _ = zDb;
    _ = v4;
    _ = v7;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    isIgnoreErrors = (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x1) >> 0));
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_ForeignKeys) == (uint64)0) {
        return;
    }
    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_NORM))) {
        return;
    }
    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
    zDb = (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName;
    pFKey = (@unsafe.Pointer.To<TTableᴛ1_Fu_Ftab>((@unsafe.Pointer)(pTab + 64))).Value.FpFKey;
    for (;;) {
        if (!(pFKey != 0)) {
            break;
        }
        pIdx = (uintptr)0;
        aiFree = (uintptr)0;
        bIgnore = 0;
        if (aChange != 0 && Xsqlite3_stricmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FzTo) != 0 && _fkChildIsModified(ref (tls).DerefOrNull(), pTab, pFKey, aChange, bChngRowid) == 0) {
            goto _1;
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x1) >> 0)) != 0) {
            pTo = _sqlite3FindTable(tls, db, (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FzTo, zDb);
        } else {
            pTo = _sqlite3LocateTable(tls, pParse, (uint32)0, (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FzTo, zDb);
        }
        if (!(pTo != 0) || _sqlite3FkLocateIndex(tls, pParse, pTo, pFKey, @unsafe.Pointer.FromBox(ᏑpIdx), @unsafe.Pointer.FromBox(ᏑaiFree)) != 0) {
            if (!(isIgnoreErrors != 0) || (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                return;
            }
            if (pTo == (uintptr)0) {
                v = _sqlite3GetVdbe(tls, pParse);
                iJump = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol + (int32)1;
                i = 0;
                for (;;) {
                    if (!(i < (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol)) {
                        break;
                    }
                    iFromCol = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64 + (uintptr)i * 16))).Value.FiFrom;
                    iReg = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom, (int16)iFromCol) + regOld + (int32)1;
                    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, iReg, iJump);
                    goto _2;
_2:;
                    i = i + 1;
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkCounter, libc.Int32FromUint8((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred), -(int32)1);
            }
            goto _1;
        }
        if (aiFree != 0) {
            aiCol = (@unsafe.Pointer)aiFree;
        } else {
            iCol = (@unsafe.Pointer.To<TsColMap>((@unsafe.Pointer)(pFKey + 64))).Value.FiFrom;
            aiCol = @unsafe.Pointer.FromPinnedBox(ᏑiCol);
        }
        i = 0;
        for (;;) {
            if (!(i < (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol)) {
                break;
            }
            if (~@unsafe.Pointer.To<int32>(@unsafe.Add(aiCol, (uintptr)i * 4)) == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                (@unsafe.Pointer.To<int32>(@unsafe.Add(aiCol, (uintptr)i * 4))).Value = -(int32)1;
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth != 0) {
                if (pIdx != 0) {
                    v4 = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)));
                } else {
                    v4 = (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTo)).Value.FiPKey;
                }
                zCol = (@unsafe.Pointer.To<TColumn>(__ccgo_ptr((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTo)).Value.FaCol + (uintptr)v4 * 16))).Value.FzCnName;
                rcauth = _sqlite3AuthReadCol(tls, pParse, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTo)).Value.FzName, zCol, iDb);
                bIgnore = libc.BoolInt32(rcauth == (int32)SQLITE_IGNORE);
            }
            goto _3;
_3:;
            i = i + 1;
        }
        _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTo)).Value.Ftnum, (uint8)0, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTo)).Value.FzName);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab + 1;
        if (regOld != 0) {
            _fkLookupParent(tls, pParse, iDb, pTo, pIdx, pFKey, aiCol, regOld, -(int32)1, bIgnore);
        }
        if (regNew != 0 && !(_isSetNullAction(ref (tls).DerefOrNull(), pParse, pFKey) != 0)) {
            _fkLookupParent(tls, pParse, iDb, pTo, pIdx, pFKey, aiCol, regNew, +libc.Int32FromInt32(1), bIgnore);
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aiFree);
        goto _1;
_1:;
        pFKey = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextFrom;
    }
    pFKey = _sqlite3FkReferences(ref (tls).DerefOrNull(), pTab);
    for (;;) {
        if (!(pFKey != 0)) {
            break;
        }
        pIdx1 = (uintptr)0;
        aiCol1 = (uintptr)0;
        if (aChange != 0 && _fkParentIsModified(ref (tls).DerefOrNull(), pTab, pFKey, aChange, bChngRowid) == 0) {
            goto _5;
        }
        if (!((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred != 0) && !((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & libc.Uint64FromInt32(SQLITE_DeferFKs)) != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FisMultiWrite != 0)) {
            goto _5;
        }
        if (_sqlite3FkLocateIndex(tls, pParse, pTab, pFKey, @unsafe.Pointer.FromBox(ᏑpIdx1), @unsafe.Pointer.FromBox(ᏑaiCol1)) != 0) {
            if (!(isIgnoreErrors != 0) || (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                return;
            }
            goto _5;
        }
        pSrc = _sqlite3SrcListAppend(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0);
        if (pSrc != 0) {
            pItem = pSrc + 8;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom)).Value.FzName;
            (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab)).Value.FnTabRef = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab)).Value.FnTabRef + 1;
            v7 = pParse + 56;
            v4 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7);
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v7) + 1;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor = v4;
            if (regNew != 0) {
                _fkScanChildren(tls, pParse, pSrc, pTab, pIdx1, pFKey, aiCol1, regNew, -(int32)1);
            }
            if (regOld != 0) {
                eAction = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pFKey + 45 + libc.BoolUintptr(aChange != (uintptr)0))));
                if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00008)).Lsh((int64)(libc.Int32FromInt32(32))))) != 0) {
                    eAction = OE_None;
                }
                _fkScanChildren(tls, pParse, pSrc, pTab, pIdx1, pFKey, aiCol1, regOld, (int32)1);
                if (!((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred != 0) && eAction != (int32)OE_Cascade && eAction != (int32)OE_SetNull) {
                    _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
                }
            }
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FzName = (uintptr)0;
            _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pSrc);
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, aiCol1);
        goto _5;
_5:;
        pFKey = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpNextTo;
    }
}

internal static void _sqlite3FkDropTable(ж<libc.TLS> tls, uintptr pParse, uintptr pName, uintptr pTab) {
    uintptr db = default;
    uintptr p = default;
    uintptr v = default;
    int32 iSkip = default;
    _ = db;
    _ = iSkip;
    _ = p;
    _ = v;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_ForeignKeys) != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == TABTYP_NORM) {
        iSkip = 0;
        v = _sqlite3GetVdbe(tls, pParse);
        if (_sqlite3FkReferences(ref (tls).DerefOrNull(), pTab) == (uintptr)0) {
            p = (@unsafe.Pointer.To<TTableᴛ1_Fu_Ftab>((@unsafe.Pointer)(pTab + 64))).Value.FpFKey;
            for (;;) {
                if (!(p != 0)) {
                    break;
                }
                if ((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)p)).Value.FisDeferred != 0 || (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_DeferFKs) != 0) {
                    break;
                }
                goto _1;
_1:;
                p = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)p)).Value.FpNextFrom;
            }
            if (!(p != 0)) {
                return;
            }
            iSkip = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkIfZero, (int32)1, iSkip);
        }
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(1), 0, 0x1);
        _sqlite3DeleteFrom(tls, pParse, _sqlite3SrcListDup(tls, db, pName, 0), (uintptr)0, (uintptr)0, (uintptr)0);
        libc.SetBitFieldPtr16Uint32(pParse + 40, libc.Uint32FromInt32(0), 0, 0x1);
        if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_DeferFKs) == (uint64)0) {
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkIfZero, 0, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2);
            _sqlite3HaltConstraint(tls, pParse, libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8))), (int32)OE_Abort, (uintptr)0, (int8)(-libc.Int32FromInt32(1)), (uint8)P5_ConstraintFK);
        }
        if (iSkip != 0) {
            _sqlite3VdbeResolveLabel(tls, v, iSkip);
        }
    }
}

internal static int32 /*r*/ _sqlite3GenerateIndexKey(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, int32 iDataCur, int32 regOut, int32 prefixOnly, @unsafe.Pointer piPartIdxLabel, uintptr pPrior, int32 regPrior) {
    int32 j = default;
    int32 nCol = default;
    int32 regBase = default;
    int32 v1 = default;
    uintptr v = default;
    _ = j;
    _ = nCol;
    _ = regBase;
    _ = v;
    _ = v1;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (piPartIdxLabel != nil) {
        if ((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != 0) {
            (@unsafe.Pointer.To<int32>(piPartIdxLabel)).Value = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = iDataCur + (int32)1;
            _sqlite3ExprIfFalseDup(tls, pParse, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere, ~@unsafe.Pointer.To<int32>(piPartIdxLabel), (int32)SQLITE_JUMPIFNULL);
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FiSelfTab = 0;
            pPrior = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<int32>(piPartIdxLabel)).Value = 0;
        }
    }
    if (prefixOnly != 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x8) >> 3) != 0) {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
    } else {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
    }
    nCol = v1;
    regBase = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nCol);
    if (pPrior != 0 && (regBase != regPrior || (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPrior)).Value.FpPartIdxWhere != 0)) {
        pPrior = (uintptr)0;
    }
    j = 0;
    for (;;) {
        if (!(j < nCol)) {
            break;
        }
        if (pPrior != 0 && (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPrior)).Value.FaiColumn + (uintptr)j * 2))) == (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)j * 2))) && (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPrior)).Value.FaiColumn + (uintptr)j * 2))) != -(int32)2) {
            goto _2;
        }
        _sqlite3ExprCodeLoadIndexColumn(tls, pParse, pIdx, iDataCur, j, regBase + j);
        if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)j * 2))) >= 0) {
            _sqlite3VdbeDeletePriorOpcode(tls, v, (uint8)OP_RealAffinity);
        }
        goto _2;
_2:;
        j = j + 1;
    }
    if (regOut != 0) {
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_MakeRecord, regBase, nCol, regOut);
    }
    _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, regBase, nCol);
    return regBase;
}

internal static void _sqlite3GenerateRowIndexDelete(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 iDataCur, int32 iIdxCur, uintptr aRegIdx, int32 iIdxNoSeek) {
    int32 i = default;
    ref var iPartIdxLabel = ref heap(new int32(), out var ᏑiPartIdxLabel);
    int32 p3 = default;
    int32 r1 = default;
    int32 v3 = default;
    uintptr pIdx = default;
    uintptr pPk = default;
    uintptr pPrior = default;
    uintptr v = default;
    uintptr v1 = default;
    _ = i;
    _ = iPartIdxLabel;
    _ = p3;
    _ = pIdx;
    _ = pPk;
    _ = pPrior;
    _ = r1;
    _ = v;
    _ = v1;
    _ = v3;
    r1 = -(int32)1;
    pPrior = (uintptr)0;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
        v1 = (uintptr)0;
    } else {
        v1 = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
    }
    pPk = v1;
    i = 0;
    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(pIdx != 0)) {
            break;
        }
        p3 = 0;
        if (aRegIdx != (uintptr)0 && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4)) == 0) {
            goto _2;
        }
        if (pIdx == pPk) {
            goto _2;
        }
        if (iIdxCur + i == iIdxNoSeek) {
            goto _2;
        }
        r1 = _sqlite3GenerateIndexKey(tls, pParse, pIdx, iDataCur, 0, (int32)1, @unsafe.Pointer.FromPinnedBox(ᏑiPartIdxLabel), pPrior, r1);
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x800) >> 11) != 0 && aRegIdx != 0) {
            p3 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aRegIdx + (uintptr)i * 4));
        }
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_IdxDelete, iIdxCur + i, r1, p3);
        _sqlite3VdbeChangeP4(tls, v, -(int32)1, pIdx, -(int32)6);
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x8) >> 3) != 0) {
            v3 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
        } else {
            v3 = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
        }
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32(v3));
        _sqlite3ResolvePartIdxLabel(tls, pParse, iPartIdxLabel);
        pPrior = pIdx;
        goto _2;
_2:;
        i = i + 1;
        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
    }
}

internal static uintptr /*r*/ _sqlite3KeyInfoOfIndex(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx) {
    int32 i = default;
    int32 nCol = default;
    int32 nKey = default;
    uintptr pKey = default;
    uintptr zColl = default;
    uintptr v2 = default;
    _ = i;
    _ = nCol;
    _ = nKey;
    _ = pKey;
    _ = zColl;
    _ = v2;
    nCol = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
    nKey = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
        return (uintptr)0;
    }
    if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x8) >> 3) != 0) {
        pKey = _sqlite3KeyInfoAlloc(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, nKey, nCol - nKey);
    } else {
        pKey = _sqlite3KeyInfoAlloc(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, nCol, 0);
    }
    if (pKey != 0) {
        i = 0;
        for (;;) {
            if (!(i < nCol)) {
                break;
            }
            zColl = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FazColl + (uintptr)i * 8));
            if (zColl == (uintptr)Ꮡ_sqlite3StrBINARY) {
                v2 = (uintptr)0;
            } else {
                v2 = _sqlite3LocateCollSeq(tls, pParse, zColl);
            }
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pKey + 32 + (uintptr)i * 8))).Value = v2;
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKey)).Value.FaSortFlags + (uintptr)i))).Value = (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSortOrder + (uintptr)i))).Value;
            goto _1;
_1:;
            i = i + 1;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x100) >> 8) == 0 && _sqlite3HashFind(ref (tls).DerefOrNull(), (@unsafe.Pointer)((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpSchema + 32), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FzName) != 0) {
                libc.SetBitFieldPtr16Uint32(pIdx + 100, libc.Uint32FromInt32(1), 8, 0x100);
                (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = libc.Int32FromInt32(SQLITE_ERROR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)));
            }
            _sqlite3KeyInfoUnref(tls, pKey);
            pKey = (uintptr)0;
        }
    }
    return pKey;
}

internal static uintptr /*r*/ _sqlite3MultiValues(ж<libc.TLS> tls, uintptr pParse, uintptr pLeft, uintptr pRow) {
    ref var dest = ref heap(new TSelectDest(), out var Ꮡdest);
    int32 f = default;
    int32 v1 = default;
    uintptr p = default;
    uintptr pRet = default;
    uintptr pSelect = default;
    uintptr pSubq = default;
    uintptr pSubq1 = default;
    uintptr v = default;
    uintptr v2 = default;
    _ = dest;
    _ = f;
    _ = p;
    _ = pRet;
    _ = pSelect;
    _ = pSubq;
    _ = pSubq1;
    _ = v;
    _ = v1;
    _ = v2;
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pParse + 40)) & 0x40) >> 6)) != 0 || (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Finit1.Fbusy != 0 || _sqlite3ExprListIsConstant(tls, pParse, pRow, (int32)1) == 0 || (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpSrc)).Value.FnSrc == 0 && _exprListIsNoAffinity(tls, pParse, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpEList) == 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) != PARSE_MODE_NORMAL) {
        pSelect = (uintptr)0;
        f = libc.Int32FromInt32(SF_Values) | libc.Int32FromInt32(SF_MultiValue);
        if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpSrc)).Value.FnSrc != 0) {
            _sqlite3MultiValuesEnd(tls, pParse, pLeft);
            f = (int32)SF_Values;
        } else {
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpPrior != 0) {
                f = libc.Int32FromUint32(libc.Uint32FromInt32(f) & (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FselFlags);
            }
        }
        pSelect = _sqlite3SelectNew(tls, pParse, pRow, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, libc.Uint32FromInt32(f), (uintptr)0);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLeft + 4))).Value &= ~libc.Uint32FromInt32(SF_MultiValue);
        if (pSelect != 0) {
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.Fop = (uint8)TK_ALL;
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pSelect)).Value.FpPrior = pLeft;
            pLeft = pSelect;
        }
    } else {
        p = (uintptr)0;
        if ((@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpSrc)).Value.FnSrc == 0) {
            v = _sqlite3GetVdbe(tls, pParse);
            pRet = _sqlite3SelectNew(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uint32)0, (uintptr)0);
            if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FmDbFlags & (uint32)DBFLAG_SchemaKnownOk) == (uint32)0) {
                _sqlite3ReadSchema(tls, pParse);
            }
            if (pRet != 0) {
                (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRet)).Value.FpSrc)).Value.FnSrc = (int32)1;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRet)).Value.FpPrior = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpPrior;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRet)).Value.Fop = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.Fop;
                if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRet)).Value.FpPrior != 0) {
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pRet + 4))).Value |= (uint32)SF_Values;
                }
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpPrior = (uintptr)0;
                (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.Fop = (uint8)TK_SELECT;
                p = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pRet)).Value.FpSrc + 8;
                libc.SetBitFieldPtr32Uint32(p + 24 + 4, libc.Uint32FromInt32(1), 6, 0x40);
                (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p)).Value.FiCursor = -(int32)1;
                (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p).of(TSrcItem.ᏑFu1).Reinterpret<TSrcItemᴛ1_Fu1, Tu32>()).Value = (uint32)2;
                if (_sqlite3SrcItemAttachSubquery(tls, pParse, p, pLeft, 0) != 0) {
                    pSubq = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 72));
                    (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)1;
                    v2 = pParse + 60;
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                    (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn = v1;
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_InitCoroutine, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn, 0, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FaddrFillSub);
                    _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(Ꮡdest), (int32)SRT_Coroutine, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregReturn);
                    dest.FiSdst = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnMem + (int32)3;
                    dest.FnSdst = (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpEList)).Value.FnExpr;
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pParse + 60))).Value += (int32)2 + dest.FnSdst;
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLeft + 4))).Value |= (uint32)SF_MultiValue;
                    _sqlite3Select(tls, pParse, pLeft, @unsafe.Pointer.FromPinnedBox(Ꮡdest));
                    (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq)).Value.FregResult = dest.FiSdst;
                }
                pLeft = pRet;
            }
        } else {
            p = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)pLeft)).Value.FpSrc + 8;
            (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p).of(TSrcItem.ᏑFu1).Reinterpret<TSrcItemᴛ1_Fu1, Tu32>()).Value = ~@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)p).of(TSrcItem.ᏑFu1).Reinterpret<TSrcItemᴛ1_Fu1, Tu32>() + 1;
        }
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
            pSubq1 = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 72));
            if ((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq1)).Value.FpSelect)).Value.FpEList)).Value.FnExpr != (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)pRow)).Value.FnExpr) {
                _sqlite3SelectWrongNumTermsError(tls, pParse, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq1)).Value.FpSelect);
            } else {
                _sqlite3ExprCodeExprList(tls, pParse, pRow, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq1)).Value.FregResult, 0, (uint8)0);
                _sqlite3VdbeAddOp1(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (int32)OP_Yield, (@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)pSubq1)).Value.FregReturn);
            }
        }
        _sqlite3ExprListDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pRow);
    }
    return pLeft;
}

internal static int32 /*r*/ _sqlite3OpenTableAndIndices(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 op, Tu8 p5, int32 iBase, uintptr aToOpen, @unsafe.Pointer piDataCur, @unsafe.Pointer piIdxCur) {
    int32 i = default;
    int32 iDataCur = default;
    int32 iDb = default;
    int32 iIdxCur = default;
    int32 v1 = default;
    uintptr pIdx = default;
    uintptr v = default;
    _ = i;
    _ = iDataCur;
    _ = iDb;
    _ = iIdxCur;
    _ = pIdx;
    _ = v;
    _ = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
        v1 = -libc.Int32FromInt32(999);
        (@unsafe.Pointer.To<int32>(piIdxCur)).Value = v1;
        (@unsafe.Pointer.To<int32>(piDataCur)).Value = v1;
        return 0;
    }
    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (iBase < 0) {
        iBase = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab;
    }
    v1 = iBase;
    iBase = iBase + 1;
    iDataCur = v1;
    (@unsafe.Pointer.To<int32>(piDataCur)).Value = iDataCur;
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 && (aToOpen == (uintptr)0 || ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen)) != 0)) {
        _sqlite3OpenTable(tls, pParse, iDataCur, iDb, pTab, op);
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FnoSharedCache) == 0) {
            _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum, libc.BoolUint8(op == (int32)OP_OpenWrite), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
        }
    }
    (@unsafe.Pointer.To<int32>(piIdxCur)).Value = iBase;
    i = 0;
    pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
    for (;;) {
        if (!(pIdx != 0)) {
            break;
        }
        v1 = iBase;
        iBase = iBase + 1;
        iIdxCur = v1;
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY && !((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0))) {
            (@unsafe.Pointer.To<int32>(piDataCur)).Value = iIdxCur;
            p5 = (uint8)0;
        }
        if (aToOpen == (uintptr)0 || ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aToOpen + (uintptr)(i + (int32)1))) != 0) {
            _sqlite3VdbeAddOp3(tls, v, op, iIdxCur, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum), iDb);
            _sqlite3VdbeSetP4KeyInfo(tls, pParse, pIdx);
            _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)p5);
        }
        goto _3;
_3:;
        pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        i = i + 1;
    }
    if (iBase > (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab) {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab = iBase;
    }
    return i;
}

internal static void _sqlite3RefillIndex(ж<libc.TLS> tls, uintptr pParse, uintptr pIndex, int32 memRootPage) {
    int32 addr1 = default;
    int32 addr2 = default;
    int32 iDb = default;
    int32 iIdx = default;
    ref var iPartIdxLabel = ref heap(new int32(), out var ᏑiPartIdxLabel);
    int32 iSorter = default;
    int32 iTab = default;
    int32 j2 = default;
    int32 regRecord = default;
    int32 v1 = default;
    int32 v3 = default;
    uintptr db = default;
    uintptr pKey = default;
    uintptr pTab = default;
    uintptr v = default;
    uintptr v2 = default;
    uintptr v4 = default;
    TPgno tnum = default;
    _ = addr1;
    _ = addr2;
    _ = db;
    _ = iDb;
    _ = iIdx;
    _ = iPartIdxLabel;
    _ = iSorter;
    _ = iTab;
    _ = j2;
    _ = pKey;
    _ = pTab;
    _ = regRecord;
    _ = tnum;
    _ = v;
    _ = v1;
    _ = v2;
    _ = v3;
    _ = v4;
    pTab = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpTable;
    v2 = pParse + 56;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    iTab = v1;
    v4 = pParse + 56;
    v3 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) + 1;
    iIdx = v3;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpSchema);
    if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_REINDEX, (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FzName, (uintptr)0, (@unsafe.Pointer.To<TDb>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaDb + (uintptr)iDb * 32))).Value.FzDbSName) != 0) {
        return;
    }
    _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum, (uint8)1, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
    v = _sqlite3GetVdbe(tls, pParse);
    if (v == (uintptr)0) {
        return;
    }
    if (memRootPage >= 0) {
        tnum = libc.Uint32FromInt32(memRootPage);
    } else {
        tnum = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.Ftnum;
    }
    pKey = _sqlite3KeyInfoOfIndex(tls, pParse, pIndex);
    v2 = pParse + 56;
    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
    iSorter = v1;
    _sqlite3VdbeAddOp4(tls, v, (int32)OP_SorterOpen, iSorter, 0, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnKeyCol), _sqlite3KeyInfoRef(ref (tls).DerefOrNull(), pKey), -(int32)9);
    _sqlite3OpenTable(tls, pParse, iTab, iDb, pTab, (int32)OP_OpenRead);
    addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_Rewind, iTab, 0);
    regRecord = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
    _sqlite3MultiWrite(ref (tls).DerefOrNull(), pParse);
    _sqlite3GenerateIndexKey(tls, pParse, pIndex, iTab, regRecord, 0, @unsafe.Pointer.FromPinnedBox(ᏑiPartIdxLabel), (uintptr)0, 0);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_SorterInsert, iSorter, regRecord);
    _sqlite3ResolvePartIdxLabel(tls, pParse, iPartIdxLabel);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Next, iTab, addr1 + (int32)1);
    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
    if (memRootPage < 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Clear, libc.Int32FromUint32(tnum), iDb);
    }
    _sqlite3VdbeAddOp4(tls, v, (int32)OP_OpenWrite, iIdx, libc.Int32FromUint32(tnum), iDb, pKey, -(int32)9);
    if (memRootPage >= 0) {
        v1 = (int32)OPFLAG_P2ISREG;
    } else {
        v1 = 0;
    }
    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32((int32)OPFLAG_BULKCSR | v1));
    addr1 = _sqlite3VdbeAddOp2(tls, v, (int32)OP_SorterSort, iSorter, 0);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FonError) != OE_None) {
        j2 = _sqlite3VdbeGoto(tls, v, (int32)1);
        addr2 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
        _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_SorterCompare, iSorter, j2, regRecord, libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FnKeyCol));
        _sqlite3UniqueConstraint(tls, pParse, (int32)OE_Abort, pIndex);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, j2);
    } else {
        _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
        addr2 = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
    }
    _sqlite3VdbeAddOp3(tls, v, (int32)OP_SorterData, iSorter, regRecord, iIdx);
    if (!((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIndex + 100)) & 0x200) >> 9) != 0)) {
        _sqlite3VdbeAddOp1(tls, v, (int32)OP_SeekEnd, iIdx);
    }
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_IdxInsert, iIdx, regRecord);
    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)OPFLAG_USESEEKRESULT);
    _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regRecord);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_SorterNext, iSorter, addr2);
    _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addr1);
    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, iTab);
    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, iIdx);
    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, iSorter);
}

internal static int32 /*r*/ _sqlite3Step(ж<libc.TLS> tls, uintptr p) {
    uintptr db = default;
    int32 rc = default;
    _ = db;
    _ = rc;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) != (int32)VDBE_RUN_STATE) {
        goto restart_step;
restart_step:;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) == (int32)VDBE_READY_STATE) {
            if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x3) >> 0)) != 0) {
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_SCHEMA;
                rc = (int32)SQLITE_ERROR;
                if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FprepFlags) & (int32)SQLITE_PREPARE_SAVESQL) != 0) {
                    rc = _sqlite3VdbeTransferError(tls, p);
                }
                goto end_of_step;
            }
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive == 0) {
                libc.AtomicStoreNInt32(db + 440, libc.Int32FromInt32(0), libc.Int32FromInt32(__ATOMIC_RELAXED));
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmTrace) & (libc.Int32FromInt32(SQLITE_TRACE_PROFILE) | libc.Int32FromInt32(SQLITE_TRACE_XPROFILE))) != 0 && !((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0) && (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql != 0) {
                _sqlite3OsCurrentTimeInt64(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVfs, p + 184);
            } else {
            }
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive + 1;
            if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x40) >> 6)) == 0) {
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeWrite = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeWrite + 1;
            }
            if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x80) >> 7)) != 0) {
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeRead = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeRead + 1;
            }
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fpc = 0;
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState = (uint8)VDBE_RUN_STATE;
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) == (int32)VDBE_HALT_STATE) {
                Xsqlite3_reset(tls, p);
                goto restart_step;
            }
        }
    }
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) != 0) {
        rc = _sqlite3VdbeList(tls, p);
    } else {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeExec = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeExec + 1;
        rc = _sqlite3VdbeExec(tls, p);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeExec = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeExec - 1;
    }
    if (rc == (int32)SQLITE_ROW) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrCode = (int32)SQLITE_ROW;
        return (int32)SQLITE_ROW;
    } else {
        if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FstartTime > 0) {
            _invokeProfileCallback(tls, db, p);
        }
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpResultRow = (uintptr)0;
        if (rc == (int32)SQLITE_DONE && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit != 0) {
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = _doWalCallbacks(tls, db);
            if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc != SQLITE_OK) {
                rc = (int32)SQLITE_ERROR;
            }
        } else {
            if (rc != (int32)SQLITE_DONE && (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FprepFlags) & (int32)SQLITE_PREPARE_SAVESQL) != 0) {
                rc = _sqlite3VdbeTransferError(tls, p);
            }
        }
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrCode = rc;
    if ((int32)SQLITE_NOMEM == _sqlite3ApiExit(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc)) {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FprepFlags) & (int32)SQLITE_PREPARE_SAVESQL) != 0) {
            rc = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc;
        }
    }
    goto end_of_step;
end_of_step:;
    return rc & (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrMask;
}

internal static int32 /*r*/ _sqlite3Utf16ByteLen(ref libc.TLS tls, uintptr zIn, int32 nByte, int32 nChar) {
    int32 c = default;
    int32 n = default;
    uintptr z = default;
    uintptr zEnd = default;
    _ = c;
    _ = n;
    _ = z;
    _ = zEnd;
    z = zIn;
    zEnd = z + (uintptr)(nByte - (int32)1);
    n = 0;
    if (true) {
        z = z + 1;
    }
    while (n < nChar && z <= zEnd) {
        c = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z)));
        z = z + (uintptr)2;
        if (c >= (int32)0xd8 && c < (int32)0xdc && z <= zEnd && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) >= (int32)0xdc && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) < (int32)0xe0) {
            z = z + (uintptr)2;
        }
        n = n + 1;
    }
    return (int32)((int64)z - (int64)zIn) - libc.BoolInt32(true);
}

internal static void _sqlite3VdbeFreeCursorNN(ж<libc.TLS> tls, uintptr p, uintptr pCx) {
    uintptr pModule = default;
    uintptr pVCur = default;
    _ = pModule;
    _ = pVCur;
    if ((int32)((TBool)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pCx + 8)) & 0x10) >> 4)) != 0) {
        _freeCursorWithCache(tls, p, pCx);
        return;
    }
    switch (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeCursor>((@unsafe.Pointer)pCx)).Value.FeCurType)) {
    case var ᴛ when ᴛ == (int32)CURTYPE_SORTER: {
        _sqlite3VdbeSorterClose(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, pCx);
        break;
    }
    case var ᴛ when ᴛ == CURTYPE_BTREE: {
        _sqlite3BtreeCloseCursor(tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCx + 48)));
        break;
    }
    case var ᴛ when ᴛ == (int32)CURTYPE_VTAB: {
        pVCur = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pCx + 48));
        pModule = (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pVCur)).Value.FpVtab)).Value.FpModule;
        (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pVCur)).Value.FpVtab)).Value.FnRef = (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)pVCur)).Value.FpVtab)).Value.FnRef - 1;
        GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_module>((@unsafe.Pointer)pModule)).Value.FxClose)(tls, pVCur);
        break;
        break;
    }}

}

internal static int32 /*r*/ _sqlite3VdbeHalt(ж<libc.TLS> tls, uintptr p) {
    uintptr db = default;
    int32 eStatementOp = default;
    int32 isSpecialError = default;
    int32 mrc = default;
    int32 rc = default;
    int32 v1 = default;
    _ = db;
    _ = eStatementOp;
    _ = isSpecialError;
    _ = mrc;
    _ = rc;
    _ = v1;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
    }
    _closeAllCursors(tls, p);
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x80) >> 7)) != 0) {
        eStatementOp = 0;
        _sqlite3VdbeEnter(tls, p);
        if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc != 0) {
            mrc = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc & (int32)0xff;
            isSpecialError = libc.BoolInt32(mrc == (int32)SQLITE_NOMEM || mrc == (int32)SQLITE_IOERR || mrc == (int32)SQLITE_INTERRUPT || mrc == (int32)SQLITE_FULL);
        } else {
            v1 = libc.Int32FromInt32(0);
            isSpecialError = v1;
            mrc = v1;
        }
        if (isSpecialError != 0) {
            if (!((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x40) >> 6)) != 0) || mrc != (int32)SQLITE_INTERRUPT) {
                if ((mrc == (int32)SQLITE_NOMEM || mrc == (int32)SQLITE_FULL) && (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x20) >> 5)) != 0) {
                    eStatementOp = (int32)SAVEPOINT_ROLLBACK;
                } else {
                    _sqlite3RollbackAll(tls, db, libc.Int32FromInt32(SQLITE_ABORT) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))));
                    _sqlite3CloseSavepoints(tls, db);
                    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit = (uint8)1;
                    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
                }
            }
        }
        if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK || libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FerrorAction) == (int32)OE_Fail && !(isSpecialError != 0)) {
            _sqlite3VdbeCheckFkImmediate(tls, p);
        }
        if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVTrans > 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FaVTrans == (uintptr)0) && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit != 0 && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeWrite == libc.BoolInt32((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x40) >> 6)) == 0)) {
            if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK || libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FerrorAction) == (int32)OE_Fail && !(isSpecialError != 0)) {
                rc = _sqlite3VdbeCheckFkDeferred(tls, p);
                if (rc != SQLITE_OK) {
                    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x40) >> 6)) != 0) {
                        _sqlite3VdbeLeave(tls, p);
                        return (int32)SQLITE_ERROR;
                    }
                    rc = libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
                } else {
                    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (libc.Uint64FromInt32(libc.Int32FromInt32(0x00002)).Lsh((int64)(libc.Int32FromInt32(32))))) != 0) {
                        rc = (int32)SQLITE_CORRUPT;
                        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value &= ~(libc.Uint64FromInt32(libc.Int32FromInt32(0x00002)).Lsh((int64)(libc.Int32FromInt32(32))));
                    } else {
                        rc = _vdbeCommit(tls, db, p);
                    }
                }
                if (rc == (int32)SQLITE_BUSY && (int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x40) >> 6)) != 0) {
                    _sqlite3VdbeLeave(tls, p);
                    return (int32)SQLITE_BUSY;
                } else {
                    if (rc != SQLITE_OK) {
                        _sqlite3SystemError(tls, db, rc);
                        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = rc;
                        _sqlite3RollbackAll(tls, db, SQLITE_OK);
                        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
                    } else {
                        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDeferredCons = 0;
                        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnDeferredImmCons = 0;
                        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(db + 48))).Value &= ~libc.Uint64FromInt32(SQLITE_DeferFKs);
                        _sqlite3CommitInternalChanges(ref (tls).DerefOrNull(), db);
                    }
                }
            } else {
                if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == (int32)SQLITE_SCHEMA && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive > (int32)1) {
                    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
                } else {
                    _sqlite3RollbackAll(tls, db, SQLITE_OK);
                    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
                }
            }
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnStatement = 0;
        } else {
            if (eStatementOp == 0) {
                if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK || libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FerrorAction) == (int32)OE_Fail) {
                    eStatementOp = (int32)SAVEPOINT_RELEASE;
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FerrorAction) == (int32)OE_Abort) {
                        eStatementOp = (int32)SAVEPOINT_ROLLBACK;
                    } else {
                        _sqlite3RollbackAll(tls, db, libc.Int32FromInt32(SQLITE_ABORT) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))));
                        _sqlite3CloseSavepoints(tls, db);
                        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit = (uint8)1;
                        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
                    }
                }
            }
        }
        if (eStatementOp != 0) {
            rc = _sqlite3VdbeCloseStatement(tls, p, eStatementOp);
            if (rc != 0) {
                if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK || ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc & (int32)0xff) == (int32)SQLITE_CONSTRAINT) {
                    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = rc;
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzErrMsg);
                    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzErrMsg = (uintptr)0;
                }
                _sqlite3RollbackAll(tls, db, libc.Int32FromInt32(SQLITE_ABORT) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))));
                _sqlite3CloseSavepoints(tls, db);
                (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit = (uint8)1;
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
            }
        }
        if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x10) >> 4)) != 0) {
            if (eStatementOp != (int32)SAVEPOINT_ROLLBACK) {
                _sqlite3VdbeSetChanges(ref (tls).DerefOrNull(), db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange);
            } else {
                _sqlite3VdbeSetChanges(ref (tls).DerefOrNull(), db, 0);
            }
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnChange = 0;
        }
        _sqlite3VdbeLeave(tls, p);
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeActive - 1;
    if (!((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x40) >> 6)) != 0)) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeWrite = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeWrite - 1;
    }
    if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0x80) >> 7)) != 0) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeRead = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeRead - 1;
    }
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState = (uint8)VDBE_HALT_STATE;
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
    }
    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit != 0) {
        _sqlite3ConnectionUnlocked(tls, db);
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == (int32)SQLITE_BUSY) {
        v1 = (int32)SQLITE_BUSY;
    } else {
        v1 = SQLITE_OK;
    }
    return v1;
}

internal static int32 /*r*/ _sqlite3VdbeList(ж<libc.TLS> tls, uintptr p) {
    ref var aOp = ref heap(new uintptr(), out var ᏑaOp);
    uintptr db = default;
    uintptr pMem = default;
    uintptr pOp = default;
    uintptr pSub = default;
    uintptr zP4 = default;
    int32 bListSubprogs = default;
    ref var i = ref heap(new int32(), out var Ꮡi);
    int32 rc = default;
    _ = aOp;
    _ = bListSubprogs;
    _ = db;
    _ = i;
    _ = pMem;
    _ = pOp;
    _ = pSub;
    _ = rc;
    _ = zP4;
    pSub = (uintptr)0;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    rc = SQLITE_OK;
    pMem = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem + 1 * 56;
    bListSubprogs = libc.BoolInt32((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) == (int32)1 || (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_TriggerEQP) != (uint64)0);
    _releaseMemArray(tls, pMem, (int32)8);
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc == (int32)SQLITE_NOMEM) {
        _sqlite3OomFault(tls, db);
        return (int32)SQLITE_ERROR;
    }
    if (bListSubprogs != 0) {
        pSub = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaMem + 9 * 56;
    } else {
        pSub = (uintptr)0;
    }
    rc = _sqlite3VdbeNextOpcode(tls, p, pSub, libc.BoolInt32((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) == (int32)2), p + 48, @unsafe.Pointer.FromPinnedBox(Ꮡi), @unsafe.Pointer.FromBox(ᏑaOp));
    if (rc == SQLITE_OK) {
        pOp = aOp + (uintptr)i * 24;
        if (libc.AtomicLoadNInt32(db + 440, libc.Int32FromInt32(__ATOMIC_RELAXED)) != 0) {
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_INTERRUPT;
            rc = (int32)SQLITE_ERROR;
            _sqlite3VdbeError(tls, p, _sqlite3ErrStr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc), 0);
        } else {
            zP4 = _sqlite3VdbeDisplayP4(tls, db, pOp);
            if ((int32)((Tbft)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(p + 200)) & 0xc) >> 2)) == (int32)2) {
                _sqlite3VdbeMemSetInt64(tls, pMem, (int64)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp1);
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)1 * 56, (int64)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2);
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)2 * 56, (int64)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp3);
                _sqlite3VdbeMemSetStr(tls, pMem + (uintptr)3 * 56, zP4, (int64)(-(int32)1), (uint8)SQLITE_UTF8, __ccgo_fp(Xsqlite3_free));
            } else {
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)0 * 56, (int64)i);
                _sqlite3VdbeMemSetStr(tls, pMem + (uintptr)1 * 56, _sqlite3OpcodeName(ref (tls).DerefOrNull(), libc.Int32FromUint8((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fopcode)), (int64)(-(int32)1), (uint8)SQLITE_UTF8, libc.UintptrFromInt32(0));
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)2 * 56, (int64)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp1);
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)3 * 56, (int64)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp2);
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)4 * 56, (int64)(@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp3);
                _sqlite3VdbeMemSetInt64(tls, pMem + (uintptr)6 * 56, libc.Int64FromUint16((@unsafe.Pointer.To<TOp>((@unsafe.Pointer)pOp)).Value.Fp5));
                _sqlite3VdbeMemSetNull(tls, pMem + (uintptr)7 * 56);
                _sqlite3VdbeMemSetStr(tls, pMem + (uintptr)5 * 56, zP4, (int64)(-(int32)1), (uint8)SQLITE_UTF8, __ccgo_fp(Xsqlite3_free));
            }
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpResultRow = pMem;
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed != 0) {
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
                rc = (int32)SQLITE_ERROR;
            } else {
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = SQLITE_OK;
                rc = (int32)SQLITE_ROW;
            }
        }
    }
    return rc;
}

internal static void _sqlite3WhereMinMaxOptEarlyOut(ж<libc.TLS> tls, uintptr v, uintptr pWInfo) {
    int32 i = default;
    uintptr pInner = default;
    _ = i;
    _ = pInner;
    if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pWInfo + 68)) & 0x4) >> 2) != 0)) {
        return;
    }
    if ((int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat == 0) {
        return;
    }
    i = libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - (int32)1;
    for (;;) {
        if (!(i >= 0)) {
            break;
        }
        pInner = pWInfo + 856 + (uintptr)i * 112;
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pInner)).Value.FpWLoop)).Value.FwsFlags & (uint32)WHERE_COLUMN_IN) != (uint32)0) {
            _sqlite3VdbeGoto(tls, v, (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pInner)).Value.FaddrNxt);
            return;
        }
        goto _1;
_1:;
        i = i - 1;
    }
    _sqlite3VdbeGoto(tls, v, (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiBreak);
}

internal static int32 /*r*/ _sqlite3WhereOrderByLimitOptLabel(ref libc.TLS tls, uintptr pWInfo) {
    uintptr pInner = default;
    int32 v1 = default;
    _ = pInner;
    _ = v1;
    if (!((int32)(uint32)((uint8)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(pWInfo + 68)) & 0x4) >> 2) != 0)) {
        return (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiContinue;
    }
    pInner = pWInfo + 856 + (uintptr)(libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnLevel) - (int32)1) * 112;
    if ((@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pInner)).Value.FpRJ != 0) {
        v1 = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiContinue;
    } else {
        v1 = (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pInner)).Value.FaddrNxt;
    }
    return v1;
}

internal static int32 /*r*/ _vdbeRecordCompareInt(ж<libc.TLS> tls, int32 nKey1, uintptr pKey1, uintptr pPKey2) {
    uintptr aKey = default;
    Ti64 lhs = default;
    Ti64 v = default;
    int32 res = default;
    int32 serial_type = default;
    _ = aKey;
    _ = lhs;
    _ = res;
    _ = serial_type;
    _ = v;
    aKey = pKey1 + (uintptr)(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pKey1))) & (int32)0x3F);
    serial_type = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pKey1 + 1)));
    if (libc.Uint32FromInt32(serial_type - libc.Int32FromInt32(1)) <= (uint32)5) {
        lhs = libc.Int64FromUint64(_sqlite3Get8byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aKey)).Rsh((uint64)(_aShift[serial_type]));
    } else {
        if (serial_type == (int32)8 || serial_type == (int32)9) {
            lhs = (int64)(serial_type - (int32)8);
        } else {
            return _sqlite3VdbeRecordCompare(tls, nKey1, pKey1, pPKey2);
        }
    }
    v = ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)(pPKey2 + 16));
    if (v > lhs) {
        res = (int32)(@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pPKey2)).Value.Fr1;
    } else {
        if (v < lhs) {
            res = (int32)(@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pPKey2)).Value.Fr2;
        } else {
            if (libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pPKey2)).Value.FnField) > (int32)1) {
                res = _sqlite3VdbeRecordCompareWithSkip(tls, nKey1, pKey1, (@unsafe.Pointer)pPKey2, (int32)1);
            } else {
                res = (int32)(@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pPKey2)).Value.Fdefault_rc;
                (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pPKey2)).Value.FeqSeen = (uint8)1;
            }
        }
    }
    return res;
}

internal static int32 /*r*/ _walDecodeFrame(ж<libc.TLS> tls, uintptr pWal, @unsafe.Pointer piPage, @unsafe.Pointer pnTruncate, uintptr aData, uintptr aFrame) {
    uintptr aCksum = default;
    int32 nativeCksum = default;
    Tu32 pgno = default;
    _ = aCksum;
    _ = nativeCksum;
    _ = pgno;
    aCksum = pWal + 72 + 24;
    if (libc.Xmemcmp(tls, pWal + 72 + 32, aFrame + 8, (uint64)8) != 0) {
        return 0;
    }
    pgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aFrame);
    if (pgno == (uint32)0) {
        return 0;
    }
    if (!((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage != 0)) {
        return 0;
    }
    nativeCksum = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FbigEndCksum) == SQLITE_BIGENDIAN);
    _walChecksumBytes(ref (tls).DerefOrNull(), nativeCksum, aFrame, (int32)8, aCksum, (@unsafe.Pointer)aCksum);
    _walChecksumBytes(ref (tls).DerefOrNull(), nativeCksum, aData, libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage), aCksum, (@unsafe.Pointer)aCksum);
    if (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aCksum)) != _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aFrame + 16)) || ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aCksum + 1 * 4)) != _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aFrame + 20))) {
        return 0;
    }
    (@unsafe.Pointer.To<Tu32>(piPage)).Value = pgno;
    (@unsafe.Pointer.To<Tu32>(pnTruncate)).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aFrame + 4));
    return (int32)1;
}

internal static int32 /*r*/ _walRewriteChecksums(ж<libc.TLS> tls, uintptr pWal, Tu32 iLast) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr aBuf = default;
        Ti64 iCksumOff = default;
        Ti64 iOff = default;
        Tu32 iPgno = default;
        Tu32 iRead = default;
        Tu32 nDbSize = default;
        int32 rc = default;
        int32 szPage = default;
        array<Tu8> _ᴛ1 = new(24);
        _ = aBuf;
        _ = iCksumOff;
        _ = iOff;
        _ = iPgno;
        _ = iRead;
        _ = nDbSize;
        _ = rc;
        _ = szPage;
        szPage = libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage);
        rc = SQLITE_OK;
        aBuf = Xsqlite3_malloc(tls, szPage + (int32)WAL_FRAME_HDRSIZE);
        if (aBuf == (uintptr)0) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum == (uint32)1) {
            iCksumOff = (int64)24;
        } else {
            iCksumOff = (int64)WAL_HDRSIZE + libc.Int64FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum - libc.Uint32FromInt32(1) - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE)) + (int64)16;
        }
        rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, aBuf, libc.Int32FromUint64(libc.Uint64FromInt64(4) * libc.Uint64FromInt32(2)), iCksumOff);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24))).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aBuf);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pWal + 72 + 24 + 1 * 4))).Value = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aBuf + (uintptr)4));
        iRead = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum;
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiReCksum = (uint32)0;
        for (;;) {
            if (!(rc == SQLITE_OK && iRead <= iLast)) {
                break;
            }
            iOff = libc.Int64FromInt32(WAL_HDRSIZE) + libc.Int64FromUint32(iRead - libc.Uint32FromInt32(1)) * (int64)(szPage + libc.Int32FromInt32(WAL_FRAME_HDRSIZE));
            rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, aBuf, szPage + (int32)WAL_FRAME_HDRSIZE, iOff);
            if (rc == SQLITE_OK) {
                iPgno = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)aBuf);
                nDbSize = _sqlite3Get4byte(ref (tls).DerefOrNull(), (@unsafe.Pointer)(aBuf + 4));
                _walEncodeFrame(tls, pWal, iPgno, nDbSize, aBuf + 24, bp);
                rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp, (int32)24, iOff);
            }
            goto _1;
_1:;
            iRead = iRead + 1;
        }
        Xsqlite3_free(tls, aBuf);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _whereIsCoveringIndexWalkCallback(ж<libc.TLS> tls, uintptr pWalk, uintptr pExpr) {
    uintptr aiColumn = default;
    uintptr pCk = default;
    uintptr pIdx = default;
    int32 i = default;
    Tu16 nColumn = default;
    _ = aiColumn;
    _ = i;
    _ = nColumn;
    _ = pCk;
    _ = pIdx;
    pCk = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalk + 40));
    pIdx = (@unsafe.Pointer.To<TCoveringIndexCheck>((@unsafe.Pointer)pCk)).Value.FpIdx;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_COLUMN || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AGG_COLUMN) {
        if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiTable != (@unsafe.Pointer.To<TCoveringIndexCheck>((@unsafe.Pointer)pCk)).Value.FiTabCur) {
            return WRC_Continue;
        }
        pIdx = (@unsafe.Pointer.To<TCoveringIndexCheck>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalk + 40))))).Value.FpIdx;
        aiColumn = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn;
        nColumn = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn;
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint16(nColumn))) {
                break;
            }
            if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr(aiColumn + (uintptr)i * 2))) == (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn) {
                return WRC_Continue;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<TCoveringIndexCheck>((@unsafe.Pointer)pCk)).Value.FbUnidx = (uint8)1;
        return (int32)WRC_Abort;
    } else {
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x800) >> 11) != 0 && _exprIsCoveredByIndex(tls, pExpr, pIdx, (@unsafe.Pointer.To<TCoveringIndexCheck>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pWalk + 40))))).Value.FiTabCur) != 0) {
            (@unsafe.Pointer.To<TCoveringIndexCheck>((@unsafe.Pointer)pCk)).Value.FbExpr = (uint8)1;
            return (int32)WRC_Prune;
        }
    }
    return WRC_Continue;
}

internal static int32 /*r*/ _whereKeyStats(ж<libc.TLS> tls, uintptr pParse, uintptr pIdx, uintptr pRec, int32 roundUp, @unsafe.Pointer aStat) {
    uintptr aSample = default;
    int32 i = default;
    int32 iCol = default;
    int32 iMin = default;
    int32 iSamp = default;
    int32 iSample = default;
    int32 iTest = default;
    int32 n = default;
    int32 nField = default;
    int32 res = default;
    int32 v1 = default;
    TtRowcnt iGap = default;
    TtRowcnt iLower = default;
    TtRowcnt iUpper = default;
    _ = aSample;
    _ = i;
    _ = iCol;
    _ = iGap;
    _ = iLower;
    _ = iMin;
    _ = iSamp;
    _ = iSample;
    _ = iTest;
    _ = iUpper;
    _ = n;
    _ = nField;
    _ = res;
    _ = v1;
    aSample = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaSample;
    iMin = 0;
    iLower = (uint64)0;
    _ = pParse;
    if (!((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable)).Value.FtabFlags & libc.Uint32FromInt32(TF_WithoutRowid)) == libc.Uint32FromInt32(0)) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_PRIMARYKEY) {
        nField = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol);
    } else {
        nField = libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn);
    }
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pRec)).Value.FnField) < nField) {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pRec)).Value.FnField);
    } else {
        v1 = nField;
    }
    nField = v1;
    iCol = 0;
    iSample = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnSample * nField;
    for (var cond = true; cond; cond = res != 0 && iMin < iSample) {
        iTest = (iMin + iSample) / (int32)2;
        iSamp = quo(iTest, nField);
        if (iSamp > 0) {
            n = rem(iTest, nField) + (int32)1;
            for (;;) {
                if (!(n < nField)) {
                    break;
                }
                if (~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)(iSamp - (int32)1) * 40))).Value.FanLt + (uintptr)(n - (int32)1) * 8)) != ~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)iSamp * 40))).Value.FanLt + (uintptr)(n - (int32)1) * 8))) {
                    break;
                }
                goto _2;
_2:;
                n = n + 1;
            }
        } else {
            n = iTest + (int32)1;
        }
        (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pRec)).Value.FnField = libc.Uint16FromInt32(n);
        res = _sqlite3VdbeRecordCompare(tls, (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)iSamp * 40))).Value.Fn, (@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)iSamp * 40))).Value.Fp, pRec);
        if (res < 0) {
            iLower = ~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)iSamp * 40))).Value.FanLt + (uintptr)(n - (int32)1) * 8)) + ~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)iSamp * 40))).Value.FanEq + (uintptr)(n - (int32)1) * 8));
            iMin = iTest + (int32)1;
        } else {
            if (res == 0 && n < nField) {
                iLower = ~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)iSamp * 40))).Value.FanLt + (uintptr)(n - (int32)1) * 8));
                iMin = iTest + (int32)1;
                res = -(int32)1;
            } else {
                iSample = iTest;
                iCol = n - (int32)1;
            }
        }
    }
    i = quo(iSample, nField);
    if (res == 0) {
        (@unsafe.Pointer.To<TtRowcnt>(aStat)).Value = (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)i * 40))).Value.FanLt + (uintptr)iCol * 8))).Value;
        (@unsafe.Pointer.To<TtRowcnt>(@unsafe.Add(aStat, 8))).Value = (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)i * 40))).Value.FanEq + (uintptr)iCol * 8))).Value;
    } else {
        if (i >= (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnSample) {
            iUpper = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnRowEst0;
        } else {
            iUpper = ~@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndexSample>(__ccgo_ptr(aSample + (uintptr)i * 40))).Value.FanLt + (uintptr)iCol * 8));
        }
        if (iLower >= iUpper) {
            iGap = (uint64)0;
        } else {
            iGap = iUpper - iLower;
        }
        if (roundUp != 0) {
            iGap = iGap * (uint64)2 / (uint64)3;
        } else {
            iGap = iGap / (uint64)3;
        }
        (@unsafe.Pointer.To<TtRowcnt>(aStat)).Value = iLower + iGap;
        (@unsafe.Pointer.To<TtRowcnt>(@unsafe.Add(aStat, 8))).Value = (@unsafe.Pointer.To<TtRowcnt>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaAvgEq + (uintptr)(nField - (int32)1) * 8))).Value;
    }
    (@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pRec)).Value.FnField = libc.Uint16FromInt32(nField);
    return i;
}

internal static int32 /*r*/ _whereLoopAddBtreeIndex(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, uintptr pSrc, uintptr pProbe, TLogEst nInMul) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        TLogEst M = default;
        TLogEst logK = default;
        TLogEst nIter = default;
        TLogEst nOutUnadjusted = default;
        TLogEst rCostIdx = default;
        TLogEst rLogSize = default;
        TLogEst rSize = default;
        TLogEst saved_nOut = default;
        TLogEst x = default;
        int32 bRedundant = default;
        int32 i = default;
        int32 iCol = default;
        int32 nEq = default;
        int32 nIn = default;
        int32 nRecValid = default;
        int32 nVecLen = default;
        int32 opMask = default;
        int32 rc = default;
        int32 v21 = default;
        uintptr db = default;
        uintptr pBtm = default;
        uintptr pExpr = default;
        uintptr pExpr1 = default;
        uintptr pNew = default;
        uintptr pParse = default;
        uintptr pTerm = default;
        uintptr pTop = default;
        uintptr pWInfo = default;
        uintptr v5 = default;
        Tu16 eOp = default;
        Tu16 saved_nBtm = default;
        Tu16 saved_nEq = default;
        Tu16 saved_nLTerm = default;
        Tu16 saved_nSkip = default;
        Tu16 saved_nTop = default;
        Tu16 v4 = default;
        ref var nOut = ref heap(new TtRowcnt(), out var ᏑnOut);
        TBitmask saved_prereq = default;
        Tu32 saved_wsFlags = default;
        ref var scan = ref heap(new TWhereScan(), out var Ꮡscan);
        @unsafe.Pointer v2 = default;
        bool v22 = default;
        _ = M;
        _ = bRedundant;
        _ = db;
        _ = eOp;
        _ = i;
        _ = iCol;
        _ = logK;
        _ = nEq;
        _ = nIn;
        _ = nIter;
        _ = nOut;
        _ = nOutUnadjusted;
        _ = nRecValid;
        _ = nVecLen;
        _ = opMask;
        _ = pBtm;
        _ = pExpr;
        _ = pExpr1;
        _ = pNew;
        _ = pParse;
        _ = pTerm;
        _ = pTop;
        _ = pWInfo;
        _ = rCostIdx;
        _ = rLogSize;
        _ = rSize;
        _ = rc;
        _ = saved_nBtm;
        _ = saved_nEq;
        _ = saved_nLTerm;
        _ = saved_nOut;
        _ = saved_nSkip;
        _ = saved_nTop;
        _ = saved_prereq;
        _ = saved_wsFlags;
        _ = scan;
        _ = x;
        _ = v2;
        _ = v21;
        _ = v22;
        _ = v4;
        _ = v5;
        pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
        pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        rc = SQLITE_OK;
        pTop = (uintptr)0;
        pBtm = (uintptr)0;
        pNew = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
            r = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc; goto ᒐdone;
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags & (uint32)WHERE_BTM_LIMIT) != 0) {
            opMask = libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ)));
        } else {
            opMask = libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IN) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_ISNULL) | libc.Int32FromInt32(WO_IS);
        }
        if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x4) >> 2) != 0) {
            opMask = opMask & ~(libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))));
        }
        saved_nEq = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq;
        saved_nBtm = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnBtm;
        saved_nTop = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnTop;
        saved_nSkip = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip;
        saved_nLTerm = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm;
        saved_wsFlags = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags;
        saved_prereq = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq;
        saved_nOut = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut;
        pTerm = _whereScanInit(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan), ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8)), (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor, libc.Int32FromUint16(saved_nEq), libc.Uint32FromInt32(opMask), pProbe);
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = 0;
        rSize = ~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst));
        rLogSize = _estLog(ref (tls).DerefOrNull(), rSize);
        for (;;) {
            if (!(rc == SQLITE_OK && pTerm != (uintptr)0)) {
                break;
            }
            eOp = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator;
            nIn = 0;
            nRecValid = ~@unsafe.Pointer.To<int32>(@unsafe.Add(pBuilder, 40));
            if ((libc.Int32FromUint16(eOp) == (int32)WO_ISNULL || (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_VNULL) != 0) && _indexColumnNotNull(ref (tls).DerefOrNull(), pProbe, libc.Int32FromUint16(saved_nEq)) != 0) {
                goto _1;
            }
            if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight & (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf) != 0) {
                goto _1;
            }
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_LIKEOPT) != 0 && libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) == libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ)))) {
                goto _1;
            }
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.Ffg.Fjointype) & (libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_LTORJ) | libc.Int32FromInt32(JT_RIGHT))) != 0 && !(_constraintCompatibleWithOuterJoin(ref (tls).DerefOrNull(), pTerm, pSrc) != 0)) {
                goto _1;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FonError) != OE_None && libc.Int32FromUint16(saved_nEq) == libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnKeyCol) - (int32)1) {
                v2 = @unsafe.Add(pBuilder, 44);
                (@unsafe.Pointer.To<uint8>(v2)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>(v2)) | libc.Int32FromInt32(SQLITE_BLDF1_UNIQUE));
            } else {
                v2 = @unsafe.Add(pBuilder, 44);
                (@unsafe.Pointer.To<uint8>(v2)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>(v2)) | libc.Int32FromInt32(SQLITE_BLDF1_INDEXED));
            }
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = saved_wsFlags;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq = saved_nEq;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnBtm = saved_nBtm;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnTop = saved_nTop;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = saved_nLTerm;
            if (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) >= libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLSlot) && _whereLoopResize(tls, db, pNew, libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) + (int32)1) != 0) {
                break;
            }
            v5 = pNew + 52;
            v4 = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5);
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5) + 1);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)v4 * 8))).Value = pTerm;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq = (TBitmask)(((TBitmask)(saved_prereq | (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight)) & ~(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FmaskSelf);
            if ((libc.Int32FromUint16(eOp) & (int32)WO_IN) != 0) {
                pExpr = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
                if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & (uint32)EP_xIsSelect) != (uint32)0) {
                    bRedundant = 0;
                    nIn = (int32)46;
                    i = 0;
                    for (;;) {
                        if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) - (int32)1)) {
                            break;
                        }
                        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)i * 8)) != 0 && (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)i * 8))))).Value.FpExpr == pExpr) {
                            nIn = 0;
                            if ((@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)i * 8)) + 32))).Value.FiField == (@unsafe.Pointer.To<_exprAnalyze_type>((@unsafe.Pointer)(pTerm + 32))).Value.FiField) {
                                bRedundant = (int32)1;
                            }
                        }
                        goto _6;
_6:;
                        i = i + 1;
                    }
                    if (bRedundant != 0) {
                        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = (Tu16)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm - 1);
                        goto _1;
                    }
                } else {
                    if (~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32)) != 0 && (@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32))))).Value.FnExpr != 0) {
                        nIn = (int32)_sqlite3LogEst(ref (tls).DerefOrNull(), libc.Uint64FromInt32((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 32))))).Value.FnExpr));
                    }
                }
                if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x80) >> 7) != 0 && (int32)rLogSize >= (int32)10) {
                    M = ~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst + (uintptr)saved_nEq * 2));
                    logK = _estLog(ref (tls).DerefOrNull(), (int16)nIn);
                    x = (int16)((int32)M + (int32)logK + (int32)10 - (nIn + (int32)rLogSize));
                    if ((int32)x >= 0) {
                    } else {
                        if ((int32)nInMul < (int32)2 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_SeekScan))) == (uint32)0) {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_IN_SEEKSCAN;
                        } else {
                            goto _1;
                        }
                    }
                }
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_COLUMN_IN;
            } else {
                if ((libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS))) != 0) {
                    iCol = (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiColumn + (uintptr)saved_nEq * 2)));
                    (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_COLUMN_EQ;
                    if (iCol == -(int32)1 || iCol >= 0 && (int32)nInMul == 0 && libc.Int32FromUint16(saved_nEq) == libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnKeyCol) - (int32)1) {
                        if (iCol == -(int32)1 || (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x8) >> 3) != 0 || libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnKeyCol) == (int32)1 && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FonError != 0 && (libc.Int32FromUint16(eOp) & (int32)WO_EQ) != 0) {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_ONEROW;
                        } else {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_UNQ_WANTED;
                        }
                    }
                    if (libc.Int32FromUint8(scan.FiEquiv) > (int32)1) {
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_TRANSCONS;
                    }
                } else {
                    if ((libc.Int32FromUint16(eOp) & (int32)WO_ISNULL) != 0) {
                        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_COLUMN_NULL;
                    } else {
                        nVecLen = _whereRangeVectorLen(tls, pParse, (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FiCursor, pProbe, libc.Int32FromUint16(saved_nEq), pTerm);
                        if ((libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) != 0) {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_RANGE) | libc.Int32FromInt32(WHERE_BTM_LIMIT));
                            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnBtm = libc.Uint16FromInt32(nVecLen);
                            pBtm = pTerm;
                            pTop = (uintptr)0;
                            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_LIKEOPT) != 0) {
                                pTop = pTerm + 1 * 56;
                                if (_whereLoopResize(tls, db, pNew, libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) + (int32)1) != 0) {
                                    break;
                                }
                                v5 = pNew + 52;
                                v4 = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5);
                                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5) + 1);
                                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)v4 * 8))).Value = pTop;
                                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_TOP_LIMIT;
                                (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnTop = (uint16)1;
                            }
                        } else {
                            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_RANGE) | libc.Int32FromInt32(WHERE_TOP_LIMIT));
                            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnTop = libc.Uint16FromInt32(nVecLen);
                            pTop = pTerm;
                            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags & (uint32)WHERE_BTM_LIMIT) != (uint32)0) {
                                v5 = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)(libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) - (int32)2) * 8));
                            } else {
                                v5 = (uintptr)0;
                            }
                            pBtm = v5;
                        }
                    }
                }
            }
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags & (uint32)WHERE_COLUMN_RANGE) != 0) {
                _whereRangeScanEst(tls, pParse, pBuilder, pBtm, pTop, pNew);
            } else {
                v5 = pNew + 24;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5) + 1);
                v4 = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5);
                nEq = libc.Int32FromUint16(v4);
                if ((int32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FtruthProb <= 0 && (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiColumn + (uintptr)saved_nEq * 2))) >= 0) {
                    v5 = pNew + 22;
                    (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) + (int32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FtruthProb);
                    v5 = pNew + 22;
                    (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) - nIn);
                } else {
                    nOut = (uint64)0;
                    if ((int32)nInMul == 0 && (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnSample != 0 && libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq) <= (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnSampleCol && ((libc.Int32FromUint16(eOp) & (int32)WO_IN) == 0 || (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr)).Value.Fflags & (uint32)EP_xIsSelect) == (uint32)0) && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Stat4))) == (uint32)0) {
                        pExpr1 = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
                        if ((libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_ISNULL) | libc.Int32FromInt32(WO_IS))) != 0) {
                            rc = _whereEqualScanEst(tls, pParse, pBuilder, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr1)).Value.FpRight, @unsafe.Pointer.FromPinnedBox(ᏑnOut));
                        } else {
                            rc = _whereInScanEst(tls, pParse, pBuilder, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr1 + 32)), @unsafe.Pointer.FromPinnedBox(ᏑnOut));
                        }
                        if (rc == (int32)SQLITE_NOTFOUND) {
                            rc = SQLITE_OK;
                        }
                        if (rc != SQLITE_OK) {
                            break;
                        }
                        if (nOut != 0) {
                            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = _sqlite3LogEst(ref (tls).DerefOrNull(), nOut);
                            if (nEq == (int32)1 && (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut + (int32)10 > (int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst)))) {
                                v5 = pTerm + 18;
                                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5)) | libc.Int32FromInt32(TERM_HIGHTRUTH));
                                if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_HEURTRUTH) != 0) {
                                    v2 = @unsafe.Add(pBuilder, 45);
                                    (@unsafe.Pointer.To<uint8>(v2)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>(v2)) | libc.Int32FromInt32(SQLITE_BLDF2_2NDPASS));
                                }
                            }
                            if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut > (int32)saved_nOut) {
                                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = saved_nOut;
                            }
                            v5 = pNew + 22;
                            (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) - nIn);
                        }
                    }
                    if (nOut == (uint64)0) {
                        v5 = pNew + 22;
                        (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) + ((int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst + (uintptr)nEq * 2))) - (int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst + (uintptr)(nEq - (int32)1) * 2)))));
                        if ((libc.Int32FromUint16(eOp) & (int32)WO_ISNULL) != 0) {
                            v5 = pNew + 22;
                            (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) + libc.Int32FromInt32(10));
                        }
                    }
                }
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x3) >> 0) == (int32)SQLITE_IDXTYPE_IPK) {
                rCostIdx = (int16)((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut + (int32)16);
            } else {
                rCostIdx = (int16)((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut + (int32)1 + quo((int32)15 * (int32)(@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FszIdxRow, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pSrc)).Value.FpSTab)).Value.FszTabRow));
            }
            rCostIdx = _sqlite3LogEstAdd(ref (tls).DerefOrNull(), rLogSize, rCostIdx);
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = rCostIdx;
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_IDX_ONLY) | libc.Int32FromInt32(WHERE_IPK) | libc.Int32FromInt32(WHERE_EXPRIDX))) == (uint32)0) {
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun = _sqlite3LogEstAdd(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrRun, (int16)((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut + (int32)16));
            }
            nOutUnadjusted = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut;
            v5 = pNew + 20;
            (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) + ((int32)nInMul + nIn));
            v5 = pNew + 22;
            (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) + ((int32)nInMul + nIn));
            _whereLoopOutputAdjust(tls, ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8)), pNew, rSize);
            if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40000) >> 18) != 0) {
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = 0;
            }
            rc = _whereLoopInsert(tls, pBuilder, pNew);
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags & (uint32)WHERE_COLUMN_RANGE) != 0) {
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = saved_nOut;
            } else {
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = nOutUnadjusted;
            }
            if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags & (uint32)WHERE_TOP_LIMIT) == (uint32)0 && libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq) < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnColumn) && (libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq) < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnKeyCol) || (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x3) >> 0) != (int32)SQLITE_IDXTYPE_PRIMARYKEY)) {
                if (libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq) > (int32)3) {
                    _sqlite3ProgressCheck(tls, pParse);
                }
                _whereLoopAddBtreeIndex(tls, pBuilder, pSrc, pProbe, (int16)((int32)nInMul + nIn));
            }
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = saved_nOut;
            (@unsafe.Pointer.To<int32>(@unsafe.Add(pBuilder, 40))).Value = nRecValid;
            goto _1;
_1:;
            pTerm = _whereScanNext(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan));
        }
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq = saved_prereq;
        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq = saved_nEq;
        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnBtm = saved_nBtm;
        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnTop = saved_nTop;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip = saved_nSkip;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = saved_wsFlags;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = saved_nOut;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = saved_nLTerm;
        v22 = libc.Int32FromUint16(saved_nEq) == libc.Int32FromUint16(saved_nSkip) && libc.Int32FromUint16(saved_nEq) + (int32)1 < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FnKeyCol) && libc.Int32FromUint16(saved_nEq) == libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x40) >> 6) == 0 && (int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pProbe + 100)) & 0x80) >> 7) != 0 && (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_SkipScan))) == (uint32)0 && (int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst + (uintptr)(libc.Int32FromUint16(saved_nEq) + (int32)1) * 2))) >= (int32)42 && (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pSrc + 24 + 4)) & 0x40000) >> 18) == 0; if (v22) {
            v21 = _whereLoopResize(tls, db, pNew, libc.Int32FromUint16((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm) + (int32)1);
            rc = v21;
        }
        if (v22 && v21 == SQLITE_OK) {
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq = (Tu16)((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq + 1);
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip = (Tu16)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip + 1);
            v5 = pNew + 52;
            v4 = ~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5);
            (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5)).Value = (Tu16)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v5) + 1);
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FaLTerm + (uintptr)v4 * 8))).Value = (uintptr)0;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pNew + 48))).Value |= (uint32)WHERE_SKIPSCAN;
            nIter = (int16)((int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst + (uintptr)saved_nEq * 2))) - (int32)(~@unsafe.Pointer.To<TLogEst>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pProbe)).Value.FaiRowLogEst + (uintptr)(libc.Int32FromUint16(saved_nEq) + (int32)1) * 2))));
            v5 = pNew + 22;
            (@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)).Value = (TLogEst)((int32)(~@unsafe.Pointer.To<TLogEst>((@unsafe.Pointer)v5)) - (int32)nIter);
            nIter = (int16)((int32)nIter + libc.Int32FromInt32(5));
            _whereLoopAddBtreeIndex(tls, pBuilder, pSrc, pProbe, (int16)((int32)nIter + (int32)nInMul));
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnOut = saved_nOut;
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pNew + 24))).Value.FnEq = saved_nEq;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnSkip = saved_nSkip;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = saved_wsFlags;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _whereLoopAddVirtual(ж<libc.TLS> tls, @unsafe.Pointer pBuilder, TBitmask mPrereq, TBitmask mUnusable) {
    ref var bIn = ref heap(new int32(), out var ᏑbIn);
    ref var bRetry = ref heap(new int32(), out var ᏑbRetry);
    int32 i = default;
    int32 iTerm = default;
    int32 nConstraint = default;
    int32 rc = default;
    int32 seenZero = default;
    int32 seenZeroNoIN = default;
    TBitmask mBest = default;
    TBitmask mBestNoIn = default;
    TBitmask mNext = default;
    TBitmask mPrev = default;
    TBitmask mThis = default;
    TBitmask v1 = default;
    ref var mNoOmit = ref heap(new Tu16(), out var ᏑmNoOmit);
    uintptr p = default;
    uintptr pNew = default;
    uintptr pParse = default;
    uintptr pSrc = default;
    uintptr pWC = default;
    uintptr pWInfo = default;
    bool v2 = default;
    _ = bIn;
    _ = bRetry;
    _ = i;
    _ = iTerm;
    _ = mBest;
    _ = mBestNoIn;
    _ = mNext;
    _ = mNoOmit;
    _ = mPrev;
    _ = mThis;
    _ = nConstraint;
    _ = p;
    _ = pNew;
    _ = pParse;
    _ = pSrc;
    _ = pWC;
    _ = pWInfo;
    _ = rc;
    _ = seenZero;
    _ = seenZeroNoIN;
    _ = v1;
    _ = v2;
    rc = SQLITE_OK;
    bRetry = 0;
    pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
    pParse = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpParse;
    pWC = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 8));
    pNew = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
    pSrc = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FiTab * 80;
    p = _allocateIndexInfo(tls, pWInfo, pWC, mUnusable, pSrc, @unsafe.Pointer.FromPinnedBox(ᏑmNoOmit));
    if (p == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FrSetup = 0;
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FwsFlags = (uint32)WHERE_VIRTUALTABLE;
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.FnLTerm = (uint16)0;
    libc.SetBitFieldPtr8Uint32(pNew + 24 + 4, libc.Uint32FromInt32(0), 0, 0x1);
    nConstraint = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)p)).Value.FnConstraint;
    if (_whereLoopResize(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pNew, nConstraint) != 0) {
        _freeIndexInfo(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, p);
        return (int32)SQLITE_NOMEM;
    }
    rc = _whereLoopAddVirtualOne(tls, pBuilder, mPrereq, libc.Uint64FromInt32(-libc.Int32FromInt32(1)), (uint16)0, p, mNoOmit, @unsafe.Pointer.FromPinnedBox(ᏑbIn), @unsafe.Pointer.FromPinnedBox(ᏑbRetry));
    if (bRetry != 0) {
        rc = _whereLoopAddVirtualOne(tls, pBuilder, mPrereq, libc.Uint64FromInt32(-libc.Int32FromInt32(1)), (uint16)0, p, mNoOmit, @unsafe.Pointer.FromPinnedBox(ᏑbIn), nil);
    }
    v2 = rc == SQLITE_OK; if (v2) {
        v1 = (TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq & ~mPrereq);
        mBest = v1;
    }
    if (v2 && (v1 != (uint64)0 || bIn != 0)) {
        seenZero = 0;
        seenZeroNoIN = 0;
        mPrev = (uint64)0;
        mBestNoIn = (uint64)0;
        if (bIn != 0) {
            rc = _whereLoopAddVirtualOne(tls, pBuilder, mPrereq, libc.Uint64FromInt32(-libc.Int32FromInt32(1)), (uint16)WO_IN, p, mNoOmit, @unsafe.Pointer.FromPinnedBox(ᏑbIn), nil);
            mBestNoIn = (TBitmask)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq & ~mPrereq);
            if (mBestNoIn == (uint64)0) {
                seenZero = (int32)1;
                seenZeroNoIN = (int32)1;
            }
        }
        while (rc == SQLITE_OK) {
            mNext = libc.Uint64FromInt32(-libc.Int32FromInt32(1));
            i = 0;
            for (;;) {
                if (!(i < nConstraint)) {
                    break;
                }
                iTerm = (@unsafe.Pointer.To<Tsqlite3_index_constraint>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)p)).Value.FaConstraint + (uintptr)i * 12))).Value.FiTermOffset;
                mThis = (TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)_termFromWhereClause(ref (tls).DerefOrNull(), pWC, iTerm))).Value.FprereqRight & ~mPrereq);
                if (mThis > mPrev && mThis < mNext) {
                    mNext = mThis;
                }
                goto _3;
_3:;
                i = i + 1;
            }
            mPrev = mNext;
            if (mNext == libc.Uint64FromInt32(-libc.Int32FromInt32(1))) {
                break;
            }
            if (mNext == mBest || mNext == mBestNoIn) {
                continue;
            }
            rc = _whereLoopAddVirtualOne(tls, pBuilder, mPrereq, (TBitmask)(mNext | mPrereq), (uint16)0, p, mNoOmit, @unsafe.Pointer.FromPinnedBox(ᏑbIn), nil);
            if ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pNew)).Value.Fprereq == mPrereq) {
                seenZero = (int32)1;
                if (bIn == 0) {
                    seenZeroNoIN = (int32)1;
                }
            }
        }
        if (rc == SQLITE_OK && seenZero == 0) {
            rc = _whereLoopAddVirtualOne(tls, pBuilder, mPrereq, mPrereq, (uint16)0, p, mNoOmit, @unsafe.Pointer.FromPinnedBox(ᏑbIn), nil);
            if (bIn == 0) {
                seenZeroNoIN = (int32)1;
            }
        }
        if (rc == SQLITE_OK && seenZeroNoIN == 0) {
            rc = _whereLoopAddVirtualOne(tls, pBuilder, mPrereq, mPrereq, (uint16)WO_IN, p, mNoOmit, @unsafe.Pointer.FromPinnedBox(ᏑbIn), nil);
        }
    }
    _freeIndexInfo(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, p);
    return rc;
}

internal static void _whereReverseScanOrder(ref libc.TLS tls, uintptr pWInfo) {
    int32 ii = default;
    uintptr pItem = default;
    _ = ii;
    _ = pItem;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<TSrcList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList)).Value.FnSrc)) {
            break;
        }
        pItem = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8 + (uintptr)ii * 80;
        if (!((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x200) >> 9) != 0) || libc.Int32FromUint8((@unsafe.Pointer.To<TCteUse>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 56))))).Value.FeM10d) != M10d_Yes || (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x4) >> 2) == 0 || (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TSubquery>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pItem + 72))))).Value.FpSelect)).Value.FpOrderBy == (uintptr)0) {
            (@unsafe.Pointer.To<TBitmask>(__ccgo_ptr(pWInfo + 96))).Value |= libc.Uint64FromInt32(1).Lsh((int64)(ii));
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
}

internal static int32 /*r*/ _whereShortCut(ж<libc.TLS> tls, @unsafe.Pointer pBuilder) {
    int32 iCur = default;
    int32 j = default;
    int32 opMask = default;
    int32 v2 = default;
    uintptr pIdx = default;
    uintptr pItem = default;
    uintptr pLoop = default;
    uintptr pTab = default;
    uintptr pTerm = default;
    uintptr pWC = default;
    uintptr pWInfo = default;
    ref var scan = ref heap(new TWhereScan(), out var Ꮡscan);
    _ = iCur;
    _ = j;
    _ = opMask;
    _ = pIdx;
    _ = pItem;
    _ = pLoop;
    _ = pTab;
    _ = pTerm;
    _ = pWC;
    _ = pWInfo;
    _ = scan;
    _ = v2;
    pWInfo = ~@unsafe.Pointer.To<uintptr>(pBuilder);
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) != 0) {
        return 0;
    }
    pItem = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpTabList + 8;
    pTab = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FpSTab;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
        return 0;
    }
    if ((int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x2) >> 1) != 0 || (int32)((~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pItem + 24 + 4)) & 0x1) >> 0) != 0) {
        return 0;
    }
    iCur = (@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FiCursor;
    pWC = pWInfo + 104;
    pLoop = ~@unsafe.Pointer.To<uintptr>(@unsafe.Add(pBuilder, 16));
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags = (uint32)0;
    (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnSkip = (uint16)0;
    pTerm = _whereScanInit(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan), pWC, iCur, -(int32)1, libc.Uint32FromInt32(libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS)), (uintptr)0);
    while (pTerm != 0 && (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight != 0) {
        pTerm = _whereScanNext(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan));
    }
    if (pTerm != 0) {
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags = libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_EQ) | libc.Int32FromInt32(WHERE_IPK) | libc.Int32FromInt32(WHERE_ONEROW));
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm))).Value = pTerm;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnLTerm = (uint16)1;
        (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq = (uint16)1;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FrRun = (int16)33;
    } else {
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FonError) != libc.Int32FromInt32(OE_None)) || (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpPartIdxWhere != (uintptr)0 || libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol) > libc.Int32FromUint64(libc.Uint64FromInt64(24) / libc.Uint64FromInt64(8))) {
                goto _1;
            }
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x8) >> 3) != 0) {
                v2 = libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS);
            } else {
                v2 = (int32)WO_EQ;
            }
            opMask = v2;
            j = 0;
            for (;;) {
                if (!(j < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol))) {
                    break;
                }
                pTerm = _whereScanInit(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan), pWC, iCur, j, libc.Uint32FromInt32(opMask), pIdx);
                while (pTerm != 0 && (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FprereqRight != 0) {
                    pTerm = _whereScanNext(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan));
                }
                if (pTerm == (uintptr)0) {
                    break;
                }
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FaLTerm + (uintptr)j * 8))).Value = pTerm;
                goto _3;
_3:;
                j = j + 1;
            }
            if (j != libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnKeyCol)) {
                goto _1;
            }
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags = libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_COLUMN_EQ) | libc.Int32FromInt32(WHERE_ONEROW) | libc.Int32FromInt32(WHERE_INDEXED));
            if ((int32)(uint32)((uint16)(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)(pIdx + 100)) & 0x20) >> 5) != 0 || (TBitmask)((@unsafe.Pointer.To<TSrcItem>((@unsafe.Pointer)pItem)).Value.FcolUsed & (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FcolNotIdxed) == (uint64)0) {
                (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLoop + 48))).Value |= (uint32)WHERE_IDX_ONLY;
            }
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnLTerm = libc.Uint16FromInt32(j);
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq = libc.Uint16FromInt32(j);
            (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex = pIdx;
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FrRun = (int16)39;
            break;
            goto _1;
_1:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        }
    }
    if ((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags != 0) {
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnOut = libc.Int16FromInt32(1);
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856))).Value.FpWLoop = pLoop;
        (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FmaskSelf = (uint64)1;
        (@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)(pWInfo + 856))).Value.FiTabCur = iCur;
        (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnRowOut = (int16)1;
        if ((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy != 0) {
            (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FnOBSat = (int8)(@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpOrderBy)).Value.FnExpr;
        }
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0) {
            (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct = (uint8)WHERE_DISTINCT_UNIQUE;
        }
        if (libc.Int32FromUint8(scan.FiEquiv) > (int32)1) {
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pLoop + 48))).Value |= (uint32)WHERE_TRANSCONS;
        }
        return (int32)1;
    }
    return 0;
}

} // end sqlite3_package
