global using Tstat = go.modernc.org.libc_package.Tstatᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _appendOnePathElement(ж<libc.TLS> tls, uintptr pPath, uintptr zName, int32 nName) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(4256);
        defer(tls.Free, (nint)4256, ref ᒐ);
        Tssize_t got = default;
        uintptr zIn = default;
        uintptr v2 = default;
        int32 v1 = default;
        Tstat _ᴛ1 = new();
        array<int8> _ᴛ2 = new(4098);
        _ = got;
        _ = zIn;
        _ = v1;
        _ = v2;
        if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName))) == (int32)'.') {
            if (nName == (int32)1) {
                return;
            }
            if ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zName + 1))) == (int32)'.' && nName == (int32)2) {
                if ((@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FnUsed > (int32)1) {
                    for (;;) {
                        v2 = pPath + 20;
                        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) - 1;
                        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                        if (!((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FzOut + (uintptr)v1))) != (int32)'/')) {
                            break;
                        }
                    }
                }
                return;
            }
        }
        if ((@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FnUsed + nName + (int32)2 >= (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FnOut) {
            (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.Frc = (int32)SQLITE_ERROR;
            return;
        }
        v2 = pPath + 20;
        v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
        (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FzOut + (uintptr)v1))).Value = (int8)'/';
        libc.Xmemcpy(tls, (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FzOut + (uintptr)(@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FnUsed, zName, libc.Uint64FromInt32(nName));
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pPath + 20))).Value += nName;
        if ((@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.Frc == SQLITE_OK) {
            (@unsafe.Pointer.To<int8>(__ccgo_ptr((@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FzOut + (uintptr)(@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FnUsed))).Value = 0;
            zIn = (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FzOut;
            if (GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)27].FpCurrent)(tls, zIn, bp) != 0) {
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))) != (int32)ENOENT) {
                    (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.Frc = _unixLogErrorAtLine(tls, _sqlite3CantopenError(tls, (int32)47761), __ccgo_ts + 3754, zIn, (int32)47761);
                }
            } else {
                if ((Tmode_t)((@unsafe.Pointer.To<Tstat>(__ccgo_ptr(bp))).Value.Fst_mode & (uint32)S_IFMT) == (uint32)S_IFLNK) {
                    v2 = pPath + 4;
                    v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2);
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v2) + 1;
                    if (v1 > (int32)SQLITE_MAX_SYMLINK) {
                        (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.Frc = _sqlite3CantopenError(tls, (int32)47767);
                        return;
                    }
                    got = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, Tsize_t, Tssize_t>>(_aSyscall[(int32)26].FpCurrent)(tls, zIn, bp + 144, libc.Uint64FromInt64(4098) - libc.Uint64FromInt32(2));
                    if (got <= 0 || got >= libc.Int64FromInt64(4098) - libc.Int64FromInt32(2)) {
                        (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.Frc = _unixLogErrorAtLine(tls, _sqlite3CantopenError(tls, (int32)47772), __ccgo_ts + 3745, zIn, (int32)47772);
                        return;
                    }
                    @unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 144), (nint)(got), 4098) = 0;
                    if ((int32)@unsafe.Pointer.ElementRef<int8>(__ccgo_ptr(bp + 144), 0, 4098) == (int32)'/') {
                        (@unsafe.Pointer.To<TDbPath>((@unsafe.Pointer)pPath)).Value.FnUsed = 0;
                    } else {
                        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pPath + 20))).Value -= nName + (int32)1;
                    }
                    _appendAllPathElements(tls, pPath, bp + 144);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
