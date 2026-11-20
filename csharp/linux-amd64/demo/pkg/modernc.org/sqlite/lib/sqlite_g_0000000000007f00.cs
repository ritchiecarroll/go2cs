namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _unixLockSharedMemory(ж<libc.TLS> tls, uintptr pDbFd, uintptr pShmNode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 rc = default;
        Tflock _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_whence = 0;
        (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_start = (int64)((libc.Int32FromInt32(22) + libc.Int32FromInt32(SQLITE_SHM_NLOCK)) * libc.Int32FromInt32(4) + libc.Int32FromInt32(SQLITE_SHM_NLOCK));
        (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_len = (int64)1;
        (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type = (int16)F_WRLCK;
        if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32, uintptr, int32>>(_aSyscall[(int32)7].FpCurrent)(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, (int32)F_GETLK, libc.VaList(bp + 40, bp)) != 0) {
            rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(15).Lsh((int64)(libc.Int32FromInt32(8)));
        } else {
            if ((int32)(@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type == (int32)F_UNLCK) {
                if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisReadonly != 0) {
                    (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FisUnlocked = (uint8)1;
                    rc = libc.Int32FromInt32(SQLITE_READONLY) | libc.Int32FromInt32(5).Lsh((int64)(libc.Int32FromInt32(8)));
                } else {
                    rc = _unixShmSystemLock(tls, pDbFd, (int32)F_WRLCK, (libc.Int32FromInt32(22) + libc.Int32FromInt32(SQLITE_SHM_NLOCK)) * libc.Int32FromInt32(4) + libc.Int32FromInt32(SQLITE_SHM_NLOCK), (int32)1);
                    if (rc == SQLITE_OK && _robust_ftruncate(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, (int64)3) != 0) {
                        rc = _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(18).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3592, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FzFilename, (int32)45706);
                    }
                }
            } else {
                if ((int32)(@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type == (int32)F_WRLCK) {
                    rc = (int32)SQLITE_BUSY;
                }
            }
        }
        if (rc == SQLITE_OK) {
            rc = _unixShmSystemLock(tls, pDbFd, F_RDLCK, (libc.Int32FromInt32(22) + libc.Int32FromInt32(SQLITE_SHM_NLOCK)) * libc.Int32FromInt32(4) + libc.Int32FromInt32(SQLITE_SHM_NLOCK), (int32)1);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _unixShmSystemLock(ж<libc.TLS> tls, uintptr pFile, int32 lockType, int32 ofst, int32 n) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr pShmNode = default;
        int32 rc = default;
        int32 res = default;
        Tflock _ᴛ1 = default;
        _ = pShmNode;
        _ = rc;
        _ = res;
        rc = SQLITE_OK;
        pShmNode = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FpShmNode;
        if (ofst == (libc.Int32FromInt32(22) + libc.Int32FromInt32(SQLITE_SHM_NLOCK)) * libc.Int32FromInt32(4) + libc.Int32FromInt32(SQLITE_SHM_NLOCK)) {
        } else {
        }
        if ((@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm >= 0) {
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type = (int16)lockType;
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_whence = 0;
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_start = (int64)ofst;
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_len = (int64)n;
            res = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32, uintptr, int32>>(_aSyscall[(int32)7].FpCurrent)(tls, (@unsafe.Pointer.To<TunixShmNode>((@unsafe.Pointer)pShmNode)).Value.FhShm, (int32)F_SETLK, libc.VaList(bp + 40, bp));
            if (res == -(int32)1) {
                rc = (int32)SQLITE_BUSY;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end sqlite3_package
