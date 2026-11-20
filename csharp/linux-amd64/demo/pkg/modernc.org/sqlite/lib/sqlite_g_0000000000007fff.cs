global using fd_mask = ulong;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _pager_unlock(ж<libc.TLS> tls, uintptr pPager) {
    int32 iDc = default;
    int32 rc = default;
    int32 v1 = default;
    _ = iDc;
    _ = rc;
    _ = v1;
    _sqlite3BitvecDestroy(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal = (uintptr)0;
    _releaseAllSavepoints(tls, pPager);
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_ERROR) {
            _sqlite3WalEndWriteTransaction(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal);
        }
        _sqlite3WalEndReadTransaction(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal);
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_OPEN;
    } else {
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0)) {
            if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0) {
                v1 = _sqlite3OsDeviceCharacteristics(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd);
            } else {
                v1 = 0;
            }
            iDc = v1;
            if (0 == (iDc & (int32)SQLITE_IOCAP_UNDELETABLE_WHEN_OPEN) || (int32)1 != (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) & (int32)5)) {
                _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
            }
            rc = _pagerUnlockDb(tls, pPager, NO_LOCK);
            if (rc != SQLITE_OK && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_ERROR) {
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock = libc.Uint8FromInt32(libc.Int32FromInt32(EXCLUSIVE_LOCK) + libc.Int32FromInt32(1));
            }
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_OPEN;
        }
    }
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile) == 0) {
            _pager_reset(tls, pPager);
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FchangeCountDone = (uint8)0;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_OPEN;
        } else {
            if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0) {
                v1 = PAGER_OPEN;
            } else {
                v1 = (int32)PAGER_READER;
            }
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = libc.Uint8FromInt32(v1);
        }
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FbUseFetch != 0) {
            _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, 0, (uintptr)0);
        }
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode = SQLITE_OK;
        _setGetterMethod(ref (tls).DerefOrNull(), pPager);
    }
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = 0;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr = 0;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsetSuper = (uint8)0;
}

internal static int32 /*r*/ _unixFetch(ж<libc.TLS> tls, uintptr fd, Ti64 iOff, int32 nAmt, uintptr pp) {
    int32 nEofBuffer = default;
    int32 rc = default;
    uintptr pFd = default;
    _ = nEofBuffer;
    _ = pFd;
    _ = rc;
    pFd = fd;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (uintptr)0;
    if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSizeMax > 0) {
        nEofBuffer = (int32)256;
        if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FpMapRegion == (uintptr)0) {
            rc = _unixMapfile(tls, pFd, (int64)(-(int32)1));
            if (rc != SQLITE_OK) {
                return rc;
            }
        }
        if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSize >= iOff + (int64)nAmt + (int64)nEofBuffer) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FpMapRegion + (uintptr)iOff;
            (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FnFetchOut = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FnFetchOut + 1;
        }
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _unixUnfetch(ж<libc.TLS> tls, uintptr fd, Ti64 iOff, uintptr p) {
    uintptr pFd = default;
    _ = pFd;
    pFd = fd;
    _ = iOff;
    if (p != 0) {
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FnFetchOut = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FnFetchOut - 1;
    } else {
        _unixUnmapfile(tls, pFd);
    }
    return SQLITE_OK;
}

} // end sqlite3_package
