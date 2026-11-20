namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _getFileMode(ж<libc.TLS> tls, uintptr zFile, @unsafe.Pointer pMode, @unsafe.Pointer pUid, @unsafe.Pointer pGid) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        int32 rc = default;
        Tstat _ᴛ1 = new();
        _ = rc;
        rc = SQLITE_OK;
        if (0 == GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)4].FpCurrent)(tls, zFile, bp)) {
            (@unsafe.Pointer.To<Tmode_t>(pMode)).Value = (Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & (uint32)511);
            (@unsafe.Pointer.To<Tuid_t>(pUid)).Value = (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_uid;
            (@unsafe.Pointer.To<Tgid_t>(pGid)).Value = (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_gid;
        } else {
            rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(7).Lsh((int64)(libc.Int32FromInt32(8)));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _robust_open(ж<libc.TLS> tls, uintptr z, int32 f, Tmode_t m) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(176);
        defer(tls.Free, (nint)176, ref ᒐ);
        int32 fd = default;
        Tmode_t m2 = default;
        uint32 v1 = default;
        Tstat _ᴛ1 = new();
        _ = fd;
        _ = m2;
        _ = v1;
        if (m != 0) {
            v1 = m;
        } else {
            v1 = (uint32)SQLITE_DEFAULT_FILE_PERMISSIONS;
        }
        m2 = v1;
        while ((int32)1 != 0) {
            fd = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32, int32>>(_aSyscall[0].FpCurrent)(tls, z, f | (int32)O_CLOEXEC, libc.Int32FromUint32(m2));
            if (fd < 0) {
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))) == (int32)EINTR) {
                    continue;
                }
                break;
            }
            if (fd >= (int32)SQLITE_MINIMUM_FILE_DESCRIPTOR) {
                break;
            }
            if ((f & (libc.Int32FromInt32(O_EXCL) | libc.Int32FromInt32(O_CREAT))) == (libc.Int32FromInt32(O_EXCL) | libc.Int32FromInt32(O_CREAT))) {
                GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(_aSyscall[(int32)16].FpCurrent)(tls, z);
            }
            GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32>>(_aSyscall[(int32)1].FpCurrent)(tls, fd);
            Xsqlite3_log(tls, (int32)SQLITE_WARNING, __ccgo_ts + 3766, libc.VaList(bp + 152, z, fd));
            fd = -(int32)1;
            if (GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32, int32>>(_aSyscall[0].FpCurrent)(tls, __ccgo_ts + 3809, O_RDONLY, libc.Int32FromUint32(m)) < 0) {
                break;
            }
        }
        if (fd >= 0) {
            if (m != (uint32)0) {
                if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, fd, bp) == 0 && (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size == 0 && (Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & (uint32)511) != m) {
                    GoFuncCookie.Func<Func<ж<libc.TLS>, int32, Tmode_t, int32>>(_aSyscall[(int32)14].FpCurrent)(tls, fd, m);
                }
            }
        }
        r = fd;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _unixAccess(ж<libc.TLS> tls, uintptr NotUsed, uintptr zPath, int32 flags, uintptr pResOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        Tstat _ᴛ1 = new();
        _ = NotUsed;
        if (flags == SQLITE_ACCESS_EXISTS) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pResOut))).Value = libc.BoolInt32(0 == GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)4].FpCurrent)(tls, zPath, bp) && (!((Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & libc.Uint32FromInt32(S_IFMT)) == libc.Uint32FromInt32(S_IFREG)) || (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size > 0));
        } else {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(pResOut))).Value = libc.BoolInt32(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>(_aSyscall[(int32)2].FpCurrent)(tls, zPath, libc.Int32FromInt32(W_OK) | libc.Int32FromInt32(R_OK)) == 0);
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _unixFileSize(ж<libc.TLS> tls, uintptr id, uintptr pSize) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        int32 rc = default;
        Tstat _ᴛ1 = new();
        _ = rc;
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)id)).Value.Fh, bp);
        if (rc != 0) {
            _storeLastErrno(ref (tls).DerefOrNull(), id, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))));
            r = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(7).Lsh((int64)(libc.Int32FromInt32(8))); goto ᒐdone;
        }
        (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pSize))).Value = (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size;
        if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pSize)) == (int64)1) {
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(pSize))).Value = 0;
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _unixTempFileDir(ж<libc.TLS> tls) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        uint32 i = default;
        uintptr zDir = default;
        Tstat _ᴛ1 = new();
        _ = i;
        _ = zDir;
        i = (uint32)0;
        for (;;) {
            if (!(i < (uint32)7)) {
                break;
            }
            switch (i) {
            case var ᴛ when ᴛ == (uint32)0: {
                zDir = Xsqlite3_temp_directory;
                break;
            }
            case var ᴛ when ᴛ == (uint32)1: {
                zDir = libc.Xgetenv(tls, __ccgo_ts + 4039);
                break;
            }
            case var ᴛ when ᴛ == (uint32)2: {
                zDir = libc.Xgetenv(tls, __ccgo_ts + 4053);
                break;
            }
            case var ᴛ when ᴛ == (uint32)3: {
                zDir = __ccgo_ts + 4060;
                break;
            }
            case var ᴛ when ᴛ == (uint32)4: {
                zDir = __ccgo_ts + 4069;
                break;
            }
            case var ᴛ when ᴛ == (uint32)5: {
                zDir = __ccgo_ts + 4078;
                break;
            }
            default: {
                zDir = __ccgo_ts + 1767;
                break;
                break;
            }}

            if (zDir != (uintptr)0 && GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)4].FpCurrent)(tls, zDir, bp) == 0 && (Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & (uint32)S_IFMT) == (uint32)S_IFDIR && GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>(_aSyscall[(int32)2].FpCurrent)(tls, zDir, (int32)3) == 0) {
                r = zDir; goto ᒐdone;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        r = (uintptr)0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end sqlite3_package
