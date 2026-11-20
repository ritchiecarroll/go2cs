namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt SQLITE_MAX_MMAP_SIZE => 2147418112;

internal static int32 /*r*/ _pagerBeginReadTransaction(ж<libc.TLS> tls, uintptr pPager) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        int32 _ᴛ1 = default;
        _ = rc;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
        _sqlite3WalEndReadTransaction(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal);
        rc = _sqlite3WalBeginReadTransaction(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, bp);
        if (rc != SQLITE_OK || ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0) {
            _pager_reset(tls, pPager);
            if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FbUseFetch != 0) {
                _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, 0, (uintptr)0);
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _pagerFixMaplimit(ж<libc.TLS> tls, uintptr pPager) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr fd = default;
        Tsqlite3_int64 _ᴛ1 = default;
        _ = fd;
        fd = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd;
        if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)fd)).Value.FpMethods != (uintptr)0 && (@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)fd)).Value.FpMethods)).Value.FiVersion >= (int32)3) {
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp))).Value = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FszMmap;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FbUseFetch = libc.BoolUint8(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp)) > libc.Int64FromInt32(0));
            _setGetterMethod(ref (tls).DerefOrNull(), pPager);
            _sqlite3OsFileControlHint(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (int32)SQLITE_FCNTL_MMAP_SIZE, bp);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _setGetterMethod(ref libc.TLS tls, uintptr pPager) {
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FxGet = __ccgo_fp(_getPageError);
    } else {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FbUseFetch != 0) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FxGet = __ccgo_fp(_getPageMMap);
        } else {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FxGet = __ccgo_fp(_getPageNormal);
        }
    }
}

internal static int32 /*r*/ _sqlite3BtreeSetMmapLimit(ж<libc.TLS> tls, uintptr p, Tsqlite3_int64 szMmap) {
    uintptr pBt = default;
    _ = pBt;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    _sqlite3PagerSetMmapLimit(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, szMmap);
    _sqlite3BtreeLeave(tls, p);
    return SQLITE_OK;
}

internal static readonly ж<TSqlite3Config> Ꮡ_sqlite3Config = new StaticBox<TSqlite3Config>(new TSqlite3Config(
    FbCoreMutex: (uint8)1,
    FbFullMutex: libc.BoolUint8(true),
    FbUseCis: (uint8)SQLITE_ALLOW_COVERING_INDEX_SCAN,
    FbExtraSchemaChecks: (uint8)1,
    FmxStrlen: (int32)0x7ffffffe,
    FszLookaside: (int32)1200,
    FnLookaside: (int32)40,
    FnStmtSpill: libc.Int32FromInt32(64) * libc.Int32FromInt32(1024),
    FmxMmap: (int64)SQLITE_MAX_MMAP_SIZE,
    FnPage: (int32)SQLITE_DEFAULT_PCACHE_INITSZ,
    FszPma: (uint32)SQLITE_SORTER_PMASZ,
    FmxMemdbSize: (int64)SQLITE_MEMDB_DEFAULT_MAXSIZE,
    FiOnceResetThreshold: (int32)0x7ffffffe,
    FszSorterRef: (uint32)SQLITE_DEFAULT_SORTERREF_SIZE
));
internal static ref TSqlite3Config _sqlite3Config => ref ((StaticBox<TSqlite3Config>)Ꮡ_sqlite3Config).ValueSlot;

internal static int32 /*r*/ _sqlite3OsFetch(ж<libc.TLS> tls, uintptr id, Ti64 iOff, int32 iAmt, uintptr pp) {
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, Tsqlite3_int64, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)id)).Value.FpMethods)).Value.FxFetch)(tls, id, iOff, iAmt, pp);
}

internal static int32 /*r*/ _sqlite3OsUnfetch(ж<libc.TLS> tls, uintptr id, Ti64 iOff, uintptr p) {
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, Tsqlite3_int64, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)id)).Value.FpMethods)).Value.FxUnfetch)(tls, id, iOff, p);
}

internal static int32 /*r*/ _vdbeSorterOpenTempFile(ж<libc.TLS> tls, uintptr db, Ti64 nExtend, uintptr ppFd) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Ti64 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        if (_sqlite3FaultSim(tls, (int32)202) != 0) {
            r = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(13).Lsh((int64)(libc.Int32FromInt32(8))); goto ᒐdone;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = _sqlite3OsOpenMalloc(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVfs, (uintptr)0, (@unsafe.Pointer)ppFd, libc.Int32FromInt32(SQLITE_OPEN_TEMP_JOURNAL) | libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE) | libc.Int32FromInt32(SQLITE_OPEN_EXCLUSIVE) | libc.Int32FromInt32(SQLITE_OPEN_DELETEONCLOSE), bp);
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) == SQLITE_OK) {
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8))).Value = (int64)SQLITE_MAX_MMAP_SIZE;
            _sqlite3OsFileControlHint(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppFd)), (int32)SQLITE_FCNTL_MMAP_SIZE, bp + 8);
            if (nExtend > 0) {
                _vdbeSorterExtendFile(tls, db, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppFd)), nExtend);
            }
        }
        r = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end sqlite3_package
