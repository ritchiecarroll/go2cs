global using Ttm = go.modernc.org.libc_package.Ttm1ᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _walHandleException(ж<libc.TLS> tls, uintptr pWal) {
    int32 ii = default;
    int32 v1 = default;
    int32 v2 = default;
    int32 v3 = default;
    Tu32 mUnlock = default;
    _ = ii;
    _ = mUnlock;
    _ = v1;
    _ = v2;
    _ = v3;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock) == (int32)2) {
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock = (uint8)0;
        }
        if ((int32)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock < 0) {
            v1 = 0;
        } else {
            v1 = _S.Lsh((int64)(libc.Int32FromInt32(3) + (int32)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock));
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock != 0) {
            v2 = _E.Lsh((int64)(libc.Int32FromInt32(WAL_WRITE_LOCK)));
        } else {
            v2 = 0;
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock != 0) {
            v3 = _E.Lsh((int64)(libc.Int32FromInt32(WAL_CKPT_LOCK)));
        } else {
            v3 = 0;
        }
        mUnlock = (Tu32)((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FlockMask & libc.Uint32FromInt32(~(v1 | v2 | v3)));
        ii = 0;
        for (;;) {
            if (!(ii < (int32)SQLITE_SHM_NLOCK)) {
                break;
            }
            if ((libc.Uint32FromInt32(_S.Lsh((int64)(ii))) & mUnlock) != 0) {
                _walUnlockShared(tls, pWal, ii);
            }
            if ((libc.Uint32FromInt32(_E.Lsh((int64)(ii))) & mUnlock) != 0) {
                _walUnlockExclusive(tls, pWal, ii, (int32)1);
            }
            goto _4;
_4:;
            ii = ii + 1;
        }
    }
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpFree);
    (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpFree = (uintptr)0;
    if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWiValue != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData + (uintptr)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiWiPg * 8))).Value = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWiValue;
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWiValue = (uintptr)0;
    }
    return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(34).Lsh((int64)(libc.Int32FromInt32(8)));
}

} // end sqlite3_package
