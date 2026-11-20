namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _fileHasMoved(ж<libc.TLS> tls, uintptr pFile) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        Tstat _ᴛ1 = new();
        r = libc.BoolInt32((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode != (uintptr)0 && (GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)4].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath, bp) != 0 || (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_ino != (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FfileId.Fino));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _unixMapfile(ж<libc.TLS> tls, uintptr pFd, Ti64 nMap) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        Tstat _ᴛ1 = new();
        if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FnFetchOut > 0) {
            r = SQLITE_OK; goto ᒐdone;
        }
        if (nMap < 0) {
            if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr, int32>>(_aSyscall[(int32)5].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.Fh, bp) != 0) {
                r = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(7).Lsh((int64)(libc.Int32FromInt32(8))); goto ᒐdone;
            }
            nMap = (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_size;
        }
        if (nMap > (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSizeMax) {
            nMap = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSizeMax;
        }
        if (nMap != (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FmmapSize) {
            _unixRemapfile(tls, pFd, nMap);
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} // end sqlite3_package
