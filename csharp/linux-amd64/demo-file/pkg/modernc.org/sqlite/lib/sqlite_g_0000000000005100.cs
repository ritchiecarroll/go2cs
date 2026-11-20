global using Tblksize_t = long;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _fcntlSizeHint(ж<libc.TLS> tls, uintptr pFile, Ti64 nByte) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        Ti64 iWrite = default;
        Ti64 nSize = default;
        int32 nBlk = default;
        int32 nWrite = default;
        int32 rc = default;
        Tstat _ᴛ1 = new();
        _ = iWrite;
        _ = nBlk;
        _ = nSize;
        _ = nWrite;
        _ = rc;
        if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk > 0) {
            if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh, bp) != 0) {
                r = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(7).Lsh((int64)(libc.Int32FromInt32(8))); goto ᒐdone;
            }
            nSize = quo((nByte + (int64)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk - (int64)1), (int64)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk) * (int64)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk;
            if (nSize > (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size) {
                nBlk = (int32)(@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_blksize;
                nWrite = 0;
                iWrite = quo((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size, (int64)nBlk) * (int64)nBlk + (int64)nBlk - (int64)1;
                for (;;) {
                    if (!(iWrite < nSize + (int64)nBlk - (int64)1)) {
                        break;
                    }
                    if (iWrite >= nSize) {
                        iWrite = nSize - (int64)1;
                    }
                    nWrite = _seekAndWrite(tls, pFile, iWrite, __ccgo_ts + 1732, (int32)1);
                    if (nWrite != (int32)1) {
                        r = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8))); goto ᒐdone;
                    }
                    goto _1;
_1:;
                    iWrite = iWrite + (int64)nBlk;
                }
            }
        }
        if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSizeMax > 0 && nByte > (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSize) {
            if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk <= 0) {
                if (_robust_ftruncate(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh, nByte) != 0) {
                    _storeLastErrno(ref (tls).DerefOrNull(), pFile, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))));
                    r = _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(6).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3592, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath, (int32)44903); goto ᒐdone;
                }
            }
            rc = _unixMapfile(tls, pFile, nByte);
            r = rc; goto ᒐdone;
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _findInodeInfo(ж<libc.TLS> tls, uintptr pFile, uintptr ppInode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(160);
        defer(tls.Free, (nint)160, ref ᒐ);
        int32 fd = default;
        int32 rc = default;
        uintptr pInode = default;
        TunixFileId _ᴛ1 = default;
        Tstat _ᴛ2 = new();
        _ = fd;
        _ = pInode;
        _ = rc;
        pInode = (uintptr)0;
        fd = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh;
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, fd, bp + 16);
        if (rc != 0) {
            _storeLastErrno(ref (tls).DerefOrNull(), pFile, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))));
            r = (int32)SQLITE_IOERR; goto ᒐdone;
        }
        libc.Xmemset(tls, bp, 0, (uint64)16);
        (@unsafe.Pointer.To<TunixFileId>(__ccgo_ptr(bp))).Value.Fdev = (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp + 16))).Value.Fst_dev;
        (@unsafe.Pointer.To<TunixFileId>(__ccgo_ptr(bp))).Value.Fino = (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp + 16))).Value.Fst_ino;
        pInode = _inodeList;
        while (pInode != 0 && libc.Xmemcmp(tls, bp, pInode, (uint64)16) != 0) {
            pInode = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext;
        }
        if (pInode == (uintptr)0) {
            pInode = Xsqlite3_malloc64(tls, (uint64)80);
            if (pInode == (uintptr)0) {
                r = (int32)SQLITE_NOMEM; goto ᒐdone;
            }
            libc.Xmemset(tls, pInode, 0, (uint64)80);
            libc.Xmemcpy(tls, pInode, bp, (uint64)16);
            if (_sqlite3Config.FbCoreMutex != 0) {
                (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex = Xsqlite3_mutex_alloc(tls, SQLITE_MUTEX_FAST);
                if ((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex == (uintptr)0) {
                    Xsqlite3_free(tls, pInode);
                    r = (int32)SQLITE_NOMEM; goto ᒐdone;
                }
            }
            (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnRef = (int32)1;
            (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext = _inodeList;
            (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpPrev = (uintptr)0;
            if (_inodeList != 0) {
                (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)_inodeList)).Value.FpPrev = pInode;
            }
            _inodeList = pInode;
        } else {
            (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnRef = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnRef + 1;
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppInode))).Value = pInode;
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _unixOpenSharedMemory(ж<libc.TLS> tls, uintptr pDbFd) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(160);
        defer(tls.Free, (nint)160, ref ᒐ);
        int32 nShmFilename = default;
        int32 rc = default;
        uintptr p = default;
        uintptr pInode = default;
        uintptr pShmNode = default;
        uintptr zBasePath = default;
        uintptr zShm = default;
        uintptr v1 = default;
        Tstat _ᴛ1 = new();
        _ = nShmFilename;
        _ = p;
        _ = pInode;
        _ = pShmNode;
        _ = rc;
        _ = zBasePath;
        _ = zShm;
        _ = v1;
        p = (uintptr)0;
        rc = SQLITE_OK;
        p = Xsqlite3_malloc64(tls, (uint64)24);
        if (p == (uintptr)0) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        libc.Xmemset(tls, p, 0, (uint64)24);
        _unixEnterMutex(tls);
        pInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FpInode;
        pShmNode = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpShmNode;
        if (pShmNode == (uintptr)0) {
            zBasePath = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FzPath;
            if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.Fh, bp) != 0) {
                rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(7).Lsh((int64)(libc.Int32FromInt32(8)));
                goto shm_open_err;
            }
            nShmFilename = (int32)6 + libc.Int32FromUint64(libc.Xstrlen(tls, zBasePath));
            pShmNode = Xsqlite3_malloc64(tls, (uint64)((uint64)96 + libc.Uint64FromInt32(nShmFilename)));
            if (pShmNode == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
                goto shm_open_err;
            }
            libc.Xmemset(tls, pShmNode, 0, (uint64)96 + libc.Uint64FromInt32(nShmFilename));
            v1 = pShmNode + 1 * 96;
            (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FzFilename = v1;
            zShm = v1;
            Xsqlite3_snprintf(tls, nShmFilename, zShm, __ccgo_ts + 3996, libc.VaList(bp + 152, zBasePath));
            (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm = -(int32)1;
            (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FpInode)).Value.FpShmNode = pShmNode;
            (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FpInode;
            if (_sqlite3Config.FbCoreMutex != 0) {
                (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpShmMutex = Xsqlite3_mutex_alloc(tls, SQLITE_MUTEX_FAST);
                if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpShmMutex == (uintptr)0) {
                    rc = (int32)SQLITE_NOMEM;
                    goto shm_open_err;
                }
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FbProcessLock) == 0) {
                if (0 == Xsqlite3_uri_boolean(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FzPath, __ccgo_ts + 4003, 0)) {
                    (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm = _robust_open(tls, zShm, libc.Int32FromInt32(O_RDWR) | libc.Int32FromInt32(O_CREAT) | libc.Int32FromInt32(O_NOFOLLOW), (Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & libc.Uint32FromInt32(511)));
                }
                if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm < 0) {
                    (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm = _robust_open(tls, zShm, libc.Int32FromInt32(O_RDONLY) | libc.Int32FromInt32(O_NOFOLLOW), (Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & libc.Uint32FromInt32(511)));
                    if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm < 0) {
                        rc = _unixLogErrorAtLine(tls, _sqlite3CantopenError(tls, (int32)45843), __ccgo_ts + 3556, zShm, (int32)45843);
                        goto shm_open_err;
                    }
                    (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisReadonly = (uint8)1;
                }
                _robustFchown(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_uid, (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_gid);
                rc = _unixLockSharedMemory(tls, pDbFd, pShmNode);
                if (rc != SQLITE_OK && rc != (libc.Int32FromInt32(SQLITE_READONLY) | libc.Int32FromInt32(5).Lsh((int64)(libc.Int32FromInt32(8))))) {
                    goto shm_open_err;
                }
            }
        }
        (@unsafe.Pointer.To<TunixShm>((@unsafe.Pointer)p)).Value.FpShmNode = pShmNode;
        (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRef = (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRef + 1;
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FpShm = p;
        _unixLeaveMutex(tls);
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpShmMutex);
        (@unsafe.Pointer.To<TunixShm>((@unsafe.Pointer)p)).Value.FpNext = (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpFirst;
        (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpFirst = p;
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpShmMutex);
        r = rc; goto ᒐdone;
        goto shm_open_err;
shm_open_err:;
        _unixShmPurge(tls, pDbFd);
        Xsqlite3_free(tls, p);
        _unixLeaveMutex(tls);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _unixShmMap(ж<libc.TLS> tls, uintptr fd, int32 iRegion, int32 szRegion, int32 bExtend, uintptr pp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        uintptr apNew = default;
        uintptr p = default;
        uintptr pDbFd = default;
        uintptr pMem = default;
        uintptr pShmNode = default;
        uintptr zFile = default;
        uintptr v4 = default;
        Ti64 i = default;
        Ti64 iPg = default;
        Ti64 nByte = default;
        Ti64 nMap = default;
        int32 nReqRegion = default;
        int32 nShmPerMap = default;
        int32 rc = default;
        ref var x = ref heap(new int32(), out var Ꮡx);
        int32 v2 = default;
        Tstat _ᴛ1 = new();
        _ = apNew;
        _ = i;
        _ = iPg;
        _ = nByte;
        _ = nMap;
        _ = nReqRegion;
        _ = nShmPerMap;
        _ = p;
        _ = pDbFd;
        _ = pMem;
        _ = pShmNode;
        _ = rc;
        _ = x;
        _ = zFile;
        _ = v2;
        _ = v4;
        pDbFd = fd;
        rc = SQLITE_OK;
        nShmPerMap = _unixShmRegionPerMap(tls);
        if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FpShm == (uintptr)0) {
            rc = _unixOpenSharedMemory(tls, pDbFd);
            if (rc != SQLITE_OK) {
                r = rc; goto ᒐdone;
            }
        }
        p = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pDbFd)).Value.FpShm;
        pShmNode = (@unsafe.Pointer.To<TunixShm>((@unsafe.Pointer)p)).Value.FpShmNode;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpShmMutex);
        if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisUnlocked != 0) {
            rc = _unixLockSharedMemory(tls, pDbFd, pShmNode);
            if (rc != SQLITE_OK) {
                goto shmpage_out;
            }
            (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisUnlocked = (uint8)0;
        }
        nReqRegion = quo((iRegion + nShmPerMap), nShmPerMap) * nShmPerMap;
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRegion) < nReqRegion) {
            nByte = (int64)nReqRegion * (int64)szRegion;
            (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FszRegion = szRegion;
            if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm >= 0) {
                if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, bp) != 0) {
                    rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(19).Lsh((int64)(libc.Int32FromInt32(8)));
                    goto shmpage_out;
                }
                if ((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size < nByte) {
                    if (!(bExtend != 0)) {
                        goto shmpage_out;
                    } else {
                        iPg = quo((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size, (int64)_pgsz);
                        for (;;) {
                            if (!(iPg < quo(nByte, (int64)_pgsz))) {
                                break;
                            }
                            x = 0;
                            if (_seekAndWriteFd(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, iPg * (int64)_pgsz + (int64)_pgsz - (int64)1, __ccgo_ts + 1732, (int32)1, @unsafe.Pointer.FromPinnedBox(Ꮡx)) != (int32)1) {
                                zFile = (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FzFilename;
                                rc = _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(19).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3627, zFile, (int32)45987);
                                goto shmpage_out;
                            }
                            goto _1;
_1:;
                            iPg = iPg + 1;
                        }
                    }
                }
            }
            apNew = Xsqlite3_realloc64(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FapRegion, (uint64)(libc.Uint64FromInt32(nReqRegion) * (uint64)8));
            if (!(apNew != 0)) {
                rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8)));
                goto shmpage_out;
            }
            (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FapRegion = apNew;
            while (libc.Int32FromUint16((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRegion) < nReqRegion) {
                nMap = (int64)szRegion * (int64)nShmPerMap;
                if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm >= 0) {
                    if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisReadonly != 0) {
                        v2 = (int32)PROT_READ;
                    } else {
                        v2 = libc.Int32FromInt32(PROT_READ) | libc.Int32FromInt32(PROT_WRITE);
                    }
                    pMem = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, Tsize_t, int32, int32, int32, Toff_t, uintptr>>(_aSyscall[(int32)22].FpCurrent)(tls, (uintptr)0, libc.Uint64FromInt64(nMap), v2, (int32)MAP_SHARED, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, (int64)((int64)szRegion * libc.Int64FromUint16((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRegion)));
                    if (pMem == (uintptr)(-libc.Int32FromInt32(1))) {
                        rc = _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(21).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3714, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FzFilename, (int32)46014);
                        goto shmpage_out;
                    }
                } else {
                    pMem = Xsqlite3_malloc64(tls, libc.Uint64FromInt64(nMap));
                    if (pMem == (uintptr)0) {
                        rc = (int32)SQLITE_NOMEM;
                        goto shmpage_out;
                    }
                    libc.Xmemset(tls, pMem, 0, libc.Uint64FromInt64(nMap));
                }
                i = 0;
                for (;;) {
                    if (!(i < (int64)nShmPerMap)) {
                        break;
                    }
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FapRegion + (uintptr)(libc.Int64FromUint16((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRegion) + i) * 8))).Value = pMem + (uintptr)((int64)szRegion * i);
                    goto _3;
_3:;
                    i = i + 1;
                }
                v4 = pShmNode + 32;
                (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v4)) + nShmPerMap);
            }
        }
        goto shmpage_out;
shmpage_out:;
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FnRegion) > iRegion) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FapRegion + (uintptr)iRegion * 8))).Value;
        } else {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (uintptr)0;
        }
        if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisReadonly != 0 && rc == SQLITE_OK) {
            rc = (int32)SQLITE_READONLY;
        }
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FpShmMutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _verifyDbFile(ж<libc.TLS> tls, uintptr pFile) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(160);
        defer(tls.Free, (nint)160, ref ᒐ);
        int32 rc = default;
        Tstat _ᴛ1 = new();
        _ = rc;
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FctrlFlags) & (int32)UNIXFILE_NOLOCK) != 0) {
            return;
        }
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh, bp);
        if (rc != 0) {
            Xsqlite3_log(tls, (int32)SQLITE_WARNING, __ccgo_ts + 3874, libc.VaList(bp + 152, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath));
            return;
        }
        if ((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_nlink == (uint64)0) {
            Xsqlite3_log(tls, (int32)SQLITE_WARNING, __ccgo_ts + 3898, libc.VaList(bp + 152, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath));
            return;
        }
        if ((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_nlink > (uint64)1) {
            Xsqlite3_log(tls, (int32)SQLITE_WARNING, __ccgo_ts + 3927, libc.VaList(bp + 152, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath));
            return;
        }
        if (_fileHasMoved(tls, pFile) != 0) {
            Xsqlite3_log(tls, (int32)SQLITE_WARNING, __ccgo_ts + 3954, libc.VaList(bp + 152, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath));
            return;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
