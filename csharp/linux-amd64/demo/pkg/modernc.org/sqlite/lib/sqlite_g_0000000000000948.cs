namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _sqlite3PagerSharedLock(ж<libc.TLS> tls, uintptr pPager) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        ref var bHotJournal = ref heap(new int32(), out var ᏑbHotJournal);
        int32 f = default;
        int32 rc = default;
        uintptr pVfs = default;
        int32 _ᴛ1 = default;
        array<int8> _ᴛ2 = new(16);
        int32 _ᴛ3 = default;
        _ = bHotJournal;
        _ = f;
        _ = pVfs;
        _ = rc;
        rc = SQLITE_OK;
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != libc.UintptrFromInt32(0)) && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == PAGER_OPEN) {
            bHotJournal = (int32)1;
            rc = _pager_wait_on_lock(tls, pPager, (int32)SHARED_LOCK);
            if (rc != SQLITE_OK) {
                goto failed;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) <= (int32)SHARED_LOCK) {
                rc = _hasHotJournal(tls, pPager, @unsafe.Pointer.FromPinnedBox(ᏑbHotJournal));
            }
            if (rc != SQLITE_OK) {
                goto failed;
            }
            if (bHotJournal != 0) {
                if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FreadOnly != 0) {
                    rc = libc.Int32FromInt32(SQLITE_READONLY) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
                    goto failed;
                }
                rc = _pagerLockDb(tls, pPager, (int32)EXCLUSIVE_LOCK);
                if (rc != SQLITE_OK) {
                    goto failed;
                }
                if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != libc.UintptrFromInt32(0)) && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) != (int32)PAGER_JOURNALMODE_OFF) {
                    pVfs = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs;
                    rc = _sqlite3OsAccess(tls, pVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, SQLITE_ACCESS_EXISTS, bp);
                    if (rc == SQLITE_OK && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0) {
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4))).Value = 0;
                        f = libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_MAIN_JOURNAL);
                        rc = _sqlite3OsOpen(tls, pVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, f, bp + 4);
                        if (rc == SQLITE_OK && (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) & (int32)SQLITE_OPEN_READONLY) != 0) {
                            rc = _sqlite3CantopenError(tls, (int32)64513);
                            _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
                        }
                    }
                }
                if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0) {
                    rc = _pagerSyncHotJournal(tls, pPager);
                    if (rc == SQLITE_OK) {
                        rc = _pager_playback(tls, pPager, libc.BoolInt32(!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0)));
                        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_OPEN;
                    }
                } else {
                    if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0)) {
                        _pagerUnlockDb(tls, pPager, (int32)SHARED_LOCK);
                    }
                }
                if (rc != SQLITE_OK) {
                    _pager_error(ref (tls).DerefOrNull(), pPager, rc);
                    goto failed;
                }
            }
            if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0) && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FhasHeldSharedLock != 0) {
                rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, bp + 8, (int32)16, (int64)24);
                if (rc != SQLITE_OK) {
                    if (rc != (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))))) {
                        goto failed;
                    }
                    libc.Xmemset(tls, bp + 8, 0, (uint64)16);
                }
                if (libc.Xmemcmp(tls, pPager + 136, bp + 8, (uint64)16) != 0) {
                    _pager_reset(tls, pPager);
                    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FbUseFetch != 0) {
                        _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, 0, (uintptr)0);
                    }
                }
            }
            rc = _pagerOpenWalIfPresent(tls, pPager);
        }
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
            rc = _pagerBeginReadTransaction(tls, pPager);
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == PAGER_OPEN && rc == SQLITE_OK) {
            rc = _pagerPagecount(tls, pPager, (@unsafe.Pointer)(pPager + 32));
        }
        goto failed;
failed:;
        if (rc != SQLITE_OK) {
            _pager_unlock(tls, pPager);
        } else {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_READER;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FhasHeldSharedLock = (uint8)1;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end sqlite3_package
