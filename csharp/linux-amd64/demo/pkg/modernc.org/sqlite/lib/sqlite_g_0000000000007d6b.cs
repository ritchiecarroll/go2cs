namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_config(ж<libc.TLS> tls, int32 op, uintptr va) {
    ref var ap = ref heap(new Tva_list(), out var Ꮡap);
    int32 bOpenUri = default;
    int32 rc = default;
    Tsqlite3_int64 mxMmap = default;
    Tsqlite3_int64 szMmap = default;
    uintptr pLogArg = default;
    uintptr pVal = default;
    uintptr xLog = default;
    _ = ap;
    _ = bOpenUri;
    _ = mxMmap;
    _ = pLogArg;
    _ = pVal;
    _ = rc;
    _ = szMmap;
    _ = xLog;
    rc = SQLITE_OK;
    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_sqlite3Config + 340) != 0) {
        if (op < 0 || op > (int32)63 || (libc.Uint64FromInt32(1).Lsh((int64)(op)) & _mAnytimeConfigOption) == (uint64)0) {
            return _sqlite3MisuseError(tls, (int32)189227);
        }
    }
    ap = va;
    var matchᴛ1 = false;
    if (op == (int32)SQLITE_CONFIG_SINGLETHREAD) { matchᴛ1 = true;
        _sqlite3Config.FbCoreMutex = (uint8)0;
        _sqlite3Config.FbFullMutex = (uint8)0;
    }
    else if (op == (int32)SQLITE_CONFIG_MULTITHREAD) { matchᴛ1 = true;
        _sqlite3Config.FbCoreMutex = (uint8)1;
        _sqlite3Config.FbFullMutex = (uint8)0;
    }
    else if (op == (int32)SQLITE_CONFIG_SERIALIZED) { matchᴛ1 = true;
        _sqlite3Config.FbCoreMutex = (uint8)1;
        _sqlite3Config.FbFullMutex = (uint8)1;
    }
    else if (op == (int32)SQLITE_CONFIG_MUTEX) { matchᴛ1 = true;
        _sqlite3Config.Fmutex = ~@unsafe.Pointer.To<Tsqlite3_mutex_methods>(__ccgo_ptr(libc.VaUintptr(Ꮡap)));
    }
    else if (op == (int32)SQLITE_CONFIG_GETMUTEX) { matchᴛ1 = true;
        (@unsafe.Pointer.To<Tsqlite3_mutex_methods>(__ccgo_ptr(libc.VaUintptr(Ꮡap)))).Value = _sqlite3Config.Fmutex;
    }
    else if (op == (int32)SQLITE_CONFIG_MALLOC) { matchᴛ1 = true;
        _sqlite3Config.Fm = ~@unsafe.Pointer.To<Tsqlite3_mem_methods>(__ccgo_ptr(libc.VaUintptr(Ꮡap)));
    }
    else if (op == (int32)SQLITE_CONFIG_GETMALLOC) { matchᴛ1 = true;
        if (_sqlite3Config.Fm.FxMalloc == (uintptr)0) {
            _sqlite3MemSetDefault(tls);
        }
        (@unsafe.Pointer.To<Tsqlite3_mem_methods>(__ccgo_ptr(libc.VaUintptr(Ꮡap)))).Value = _sqlite3Config.Fm;
    }
    else if (op == (int32)SQLITE_CONFIG_MEMSTATUS) { matchᴛ1 = true;
        _sqlite3Config.FbMemstat = libc.VaInt32(Ꮡap);
    }
    else if (op == (int32)SQLITE_CONFIG_SMALL_MALLOC) { matchᴛ1 = true;
        _sqlite3Config.FbSmallMalloc = libc.BoolUint8(libc.VaInt32(Ꮡap) != 0);
    }
    else if (op == (int32)SQLITE_CONFIG_PAGECACHE) { matchᴛ1 = true;
        _sqlite3Config.FpPage = libc.VaUintptr(Ꮡap);
        _sqlite3Config.FszPage = libc.VaInt32(Ꮡap);
        _sqlite3Config.FnPage = libc.VaInt32(Ꮡap);
    }
    else if (op == (int32)SQLITE_CONFIG_PCACHE_HDRSZ) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.VaUintptr(Ꮡap)))).Value = _sqlite3HeaderSizeBtree(ref (tls).DerefOrNull()) + _sqlite3HeaderSizePcache(ref (tls).DerefOrNull()) + _sqlite3HeaderSizePcache1(ref (tls).DerefOrNull());
    }
    else if (op == (int32)SQLITE_CONFIG_PCACHE) { matchᴛ1 = true;
    }
    else if (op == (int32)SQLITE_CONFIG_GETPCACHE) { matchᴛ1 = true;
        rc = (int32)SQLITE_ERROR;
    }
    else if (op == (int32)SQLITE_CONFIG_PCACHE2) { matchᴛ1 = true;
        _sqlite3Config.Fpcache2 = ~@unsafe.Pointer.To<Tsqlite3_pcache_methods2>(__ccgo_ptr(libc.VaUintptr(Ꮡap)));
    }
    else if (op == (int32)SQLITE_CONFIG_GETPCACHE2) { matchᴛ1 = true;
        do {
            if (_sqlite3Config.Fpcache2.FxInit == (uintptr)0) {
                _sqlite3PCacheSetDefault(tls);
            }
            (@unsafe.Pointer.To<Tsqlite3_pcache_methods2>(__ccgo_ptr(libc.VaUintptr(Ꮡap)))).Value = _sqlite3Config.Fpcache2;
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_CONFIG_LOOKASIDE) { matchᴛ1 = true;
        do {
            _sqlite3Config.FszLookaside = libc.VaInt32(Ꮡap);
            _sqlite3Config.FnLookaside = libc.VaInt32(Ꮡap);
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_CONFIG_LOG) { matchᴛ1 = true;
        do {
            xLog = libc.VaUintptr(Ꮡap);
            pLogArg = libc.VaUintptr(Ꮡap);
            libc.AtomicStoreNUintptr((uintptr)Ꮡ_sqlite3Config + 376, xLog, libc.Int32FromInt32(__ATOMIC_RELAXED));
            libc.AtomicStoreNUintptr((uintptr)Ꮡ_sqlite3Config + 384, pLogArg, libc.Int32FromInt32(__ATOMIC_RELAXED));
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1 && op == (int32)SQLITE_CONFIG_URI) { matchᴛ1 = true;
        bOpenUri = libc.VaInt32(Ꮡap);
        libc.AtomicStoreNUint8((uintptr)Ꮡ_sqlite3Config + 6, libc.Uint8FromInt32(bOpenUri), libc.Int32FromInt32(__ATOMIC_RELAXED));
    }
    else if (op == (int32)SQLITE_CONFIG_COVERING_INDEX_SCAN) { matchᴛ1 = true;
        _sqlite3Config.FbUseCis = libc.Uint8FromInt32(libc.VaInt32(Ꮡap));
    }
    else if (op == (int32)SQLITE_CONFIG_MMAP_SIZE) { matchᴛ1 = true;
        szMmap = libc.VaInt64(Ꮡap);
        mxMmap = libc.VaInt64(Ꮡap);
        if (mxMmap < 0 || mxMmap > (int64)SQLITE_MAX_MMAP_SIZE) {
            mxMmap = (int64)SQLITE_MAX_MMAP_SIZE;
        }
        if (szMmap < 0) {
            szMmap = SQLITE_DEFAULT_MMAP_SIZE;
        }
        if (szMmap > mxMmap) {
            szMmap = mxMmap;
        }
        _sqlite3Config.FmxMmap = mxMmap;
        _sqlite3Config.FszMmap = szMmap;
    }
    else if (op == (int32)SQLITE_CONFIG_PMASZ) { matchᴛ1 = true;
        _sqlite3Config.FszPma = libc.VaUint32(Ꮡap);
    }
    else if (op == (int32)SQLITE_CONFIG_STMTJRNL_SPILL) { matchᴛ1 = true;
        _sqlite3Config.FnStmtSpill = libc.VaInt32(Ꮡap);
    }
    else if (op == (int32)SQLITE_CONFIG_MEMDB_MAXSIZE) { matchᴛ1 = true;
        _sqlite3Config.FmxMemdbSize = libc.VaInt64(Ꮡap);
    }
    else if (op == (int32)SQLITE_CONFIG_ROWID_IN_VIEW) {
        pVal = libc.VaUintptr(Ꮡap);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pVal))).Value = 0;
    }
    else if (!matchᴛ1) { /* default: */
        do {
            rc = (int32)SQLITE_ERROR;
            break;
        } while (false);
    }

    _ = ap;
    return rc;
}

internal static int32 /*r*/ _getPageMMap(ж<libc.TLS> tls, uintptr pPager, TPgno pgno, uintptr ppPage, int32 flags) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 bMmapOk = default;
        int32 rc = default;
        ref var pPg = ref heap(new uintptr(), out var ᏑpPg);
        Tu32 _ᴛ1 = default;
        uintptr _ᴛ2 = default;
        _ = bMmapOk;
        _ = pPg;
        _ = rc;
        rc = SQLITE_OK;
        pPg = (uintptr)0;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value = (uint32)0;
        bMmapOk = libc.BoolInt32(pgno > (uint32)1 && (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_READER || (flags & (int32)PAGER_GET_READONLY) != 0));
        if (pgno <= (uint32)1 && pgno == (uint32)0) {
            r = _sqlite3CorruptError(tls, (int32)64841); goto ᒐdone;
        }
        if (bMmapOk != 0 && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
            rc = _sqlite3WalFindFrame(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, pgno, bp);
            if (rc != SQLITE_OK) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = (uintptr)0;
                r = rc; goto ᒐdone;
            }
        }
        if (bMmapOk != 0 && ~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp)) == (uint32)0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))).Value = (uintptr)0;
            rc = _sqlite3OsFetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, libc.Int64FromUint32(pgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, bp + 8);
            if (rc == SQLITE_OK && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)) != 0) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) > (int32)PAGER_READER || (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0) {
                    pPg = _sqlite3PagerLookup(tls, pPager, pgno);
                }
                if (pPg == (uintptr)0) {
                    rc = _pagerAcquireMapPage(tls, pPager, pgno, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)), @unsafe.Pointer.FromBox(ᏑpPg));
                } else {
                    _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, libc.Int64FromUint32(pgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)));
                }
                if (pPg != 0) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = pPg;
                    r = SQLITE_OK; goto ᒐdone;
                }
            }
            if (rc != SQLITE_OK) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppPage))).Value = (uintptr)0;
                r = rc; goto ᒐdone;
            }
        }
        r = _getPageNormal(tls, pPager, pgno, ppPage, flags);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _setDeviceCharacteristics(ref libc.TLS tls, uintptr pFd) {
    if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FsectorSize == 0) {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FctrlFlags) & (int32)UNIXFILE_PSOW) != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pFd + 116))).Value |= (int32)SQLITE_IOCAP_POWERSAFE_OVERWRITE;
        }
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pFd + 116))).Value |= (int32)SQLITE_IOCAP_SUBPAGE_READ;
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FsectorSize = (int32)SQLITE_DEFAULT_SECTOR_SIZE;
    }
}

internal static void _unixUnmapfile(ж<libc.TLS> tls, uintptr pFd) {
    if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FpMapRegion != 0) {
        GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, Tsize_t, int32>>(_aSyscall[(int32)23].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FpMapRegion, libc.Uint64FromInt64((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSizeActual));
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FpMapRegion = (uintptr)0;
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSize = 0;
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSizeActual = 0;
    }
}

} // end sqlite3_package
