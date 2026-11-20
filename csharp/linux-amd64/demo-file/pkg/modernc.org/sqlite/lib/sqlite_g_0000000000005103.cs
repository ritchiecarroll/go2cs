namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static uintptr /*r*/ _findReusableFd(ж<libc.TLS> tls, uintptr zPath, int32 flags) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(144);
        defer(tls.Free, (nint)144, ref ᒐ);
        uintptr pInode = default;
        uintptr pUnused = default;
        uintptr pp = default;
        Tstat _ᴛ1 = new();
        _ = pInode;
        _ = pUnused;
        _ = pp;
        pUnused = (uintptr)0;
        _unixEnterMutex(tls);
        if (_inodeList != (uintptr)0 && 0 == GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)4].FpCurrent)(tls, zPath, bp)) {
            pInode = _inodeList;
            while (pInode != 0 && ((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FfileId.Fdev != (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_dev || (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FfileId.Fino != (@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_ino)) {
                pInode = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext;
            }
            if (pInode != 0) {
                Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
                flags = flags & (libc.Int32FromInt32(SQLITE_OPEN_READONLY) | libc.Int32FromInt32(SQLITE_OPEN_READWRITE));
                pp = pInode + 40;
                for (;;) {
                    if (!(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) != 0 && (@unsafe.Pointer.To<TUnixUnusedFd>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))))).Value.Fflags != flags)) {
                        break;
                    }
                    goto _1;
_1:;
                    pp = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp)) + 8;
                }
                pUnused = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp));
                if (pUnused != 0) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pp))).Value = (@unsafe.Pointer.To<TUnixUnusedFd>((@unsafe.Pointer)pUnused)).Value.FpNext;
                }
                Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
            }
        }
        _unixLeaveMutex(tls);
        r = pUnused;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end sqlite3_package
