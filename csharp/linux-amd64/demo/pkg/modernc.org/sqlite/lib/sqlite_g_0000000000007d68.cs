namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt NFDBITS => 64;

internal static int32 /*r*/ _closeUnixFile(ж<libc.TLS> tls, uintptr id) {
    uintptr pFile = default;
    _ = pFile;
    pFile = id;
    _unixUnmapfile(tls, pFile);
    if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh >= 0) {
        _robust_close(tls, pFile, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh, (int32)43115);
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh = -(int32)1;
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpPreallocatedUnused);
    libc.Xmemset(tls, pFile, 0, (uint64)120);
    return SQLITE_OK;
}

internal static int32 /*r*/ _pagerAcquireMapPage(ж<libc.TLS> tls, uintptr pPager, TPgno pgno, uintptr pData, @unsafe.Pointer ppPage) {
    uintptr p = default;
    uintptr v1 = default;
    _ = p;
    _ = v1;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpMmapFreelist != 0) {
        v1 = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpMmapFreelist;
        p = v1;
        (@unsafe.Pointer.To<uintptr>(ppPage)).Value = v1;
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpMmapFreelist = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty;
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpDirty = (uintptr)0;
        libc.Xmemset(tls, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpExtra, 0, (uint64)8);
    } else {
        v1 = _sqlite3MallocZero(tls, (uint64)80 + (uint64)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnExtra);
        p = v1;
        (@unsafe.Pointer.To<uintptr>(ppPage)).Value = v1;
        if (p == (uintptr)0) {
            _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, libc.Int64FromUint32(pgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, pData);
            return (int32)SQLITE_NOMEM;
        }
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpExtra = p + 1 * 80;
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fflags = (uint16)PGHDR_MMAP;
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FnRef = (int64)1;
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpPager = pPager;
    }
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fpgno = pgno;
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpData = pData;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnMmapOut = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnMmapOut + 1;
    return SQLITE_OK;
}

internal static int32 /*r*/ _unixRead(ж<libc.TLS> tls, uintptr id, uintptr pBuf, int32 amt, Tsqlite3_int64 offset) {
    int32 r = default;

    int32 got = default;
    int32 nCopy = default;
    uintptr pFile = default;
    _ = got;
    _ = nCopy;
    _ = pFile;
    pFile = id;
    if (offset < (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSize) {
        if (offset + (int64)amt <= (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSize) {
            libc.Xmemcpy(tls, pBuf, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpMapRegion + (uintptr)offset, libc.Uint64FromInt32(amt));
            return SQLITE_OK;
        } else {
            nCopy = (int32)((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSize - offset);
            libc.Xmemcpy(tls, pBuf, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpMapRegion + (uintptr)offset, libc.Uint64FromInt32(nCopy));
            pBuf = pBuf + (uintptr)nCopy;
            amt = amt - nCopy;
            offset = offset + (int64)nCopy;
        }
    }
    got = _seekAndRead(tls, pFile, offset, pBuf, amt);
    if (got == amt) {
        return SQLITE_OK;
    } else {
        if (got < 0) {
            var exprᴛ1 = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlastErrno;
            var matchᴛ1 = false;
            if (exprᴛ1 == (int32)ERANGE) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)EIO) {
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)ENXIO) { matchᴛ1 = true;
                return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(33).Lsh((int64)(libc.Int32FromInt32(8)));
            }

            return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        } else {
            _storeLastErrno(ref (tls).DerefOrNull(), pFile, 0);
            libc.Xmemset(tls, pBuf + (uintptr)got, 0, libc.Uint64FromInt32(amt - got));
            return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8)));
        }
    }
    return r;
}

internal static int32 /*r*/ _vdbeSorterListToPMA(ж<libc.TLS> tls, uintptr pTask, uintptr pList) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        uintptr pNext = default;
        int32 rc = default;
        TPmaWriter _ᴛ1 = default;
        _ = db;
        _ = p;
        _ = pNext;
        _ = rc;
        db = (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.Fdb;
        rc = SQLITE_OK;
        libc.Xmemset(tls, bp, 0, (uint64)56);
        if ((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FpFd == (uintptr)0) {
            rc = _vdbeSorterOpenTempFile(tls, db, 0, pTask + 64);
        }
        if (rc == SQLITE_OK) {
            _vdbeSorterExtendFile(tls, db, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FpFd, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FiEof + (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FszPMA + (int64)9);
        }
        if (rc == SQLITE_OK) {
            rc = _vdbeSorterSort(tls, pTask, pList);
        }
        if (rc == SQLITE_OK) {
            pNext = (uintptr)0;
            _vdbePmaWriterInit(tls, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FpFd, bp, (@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.Fpgsz, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.Ffile.FiEof);
            (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FnPMA = (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FnPMA + 1;
            _vdbePmaWriteVarint(tls, bp, libc.Uint64FromInt64((@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FszPMA));
            p = (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FpList;
            for (;;) {
                if (!(p != 0)) {
                    break;
                }
                pNext = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8));
                _vdbePmaWriteVarint(tls, bp, libc.Uint64FromInt32((@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)p)).Value.FnVal));
                _vdbePmaWriteBlob(tls, bp, p + libc.UintptrFromInt32(1) * 16, (@unsafe.Pointer.To<TSorterRecord>((@unsafe.Pointer)p)).Value.FnVal);
                if ((@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FaMemory == (uintptr)0) {
                    Xsqlite3_free(tls, p);
                }
                goto _1;
_1:;
                p = pNext;
            }
            (@unsafe.Pointer.To<TSorterList>((@unsafe.Pointer)pList)).Value.FpList = p;
            rc = _vdbePmaWriterFinish(tls, bp, pTask + 64 + 8, pTask + 96);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end sqlite3_package
