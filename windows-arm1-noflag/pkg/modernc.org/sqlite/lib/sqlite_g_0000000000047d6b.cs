namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _vdbeSorterExtendFile(ж<libc.TLS> tls, uintptr db, uintptr pFd, Ti64 _nByte) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)bp)).Value = _nByte;
        int32 _ᴛ1 = default;
        uintptr _ᴛ2 = default;
        if (~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) <= (int64)(@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnMaxSorterMmap && (@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pFd)).Value.FpMethods)).Value.FiVersion >= (int32)3) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8))).Value = (uintptr)0;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 16))).Value = libc.Int32FromInt32(4) * libc.Int32FromInt32(1024);
            _sqlite3OsFileControlHint(tls, pFd, (int32)SQLITE_FCNTL_CHUNK_SIZE, bp + 16);
            _sqlite3OsFileControlHint(tls, pFd, (int32)SQLITE_FCNTL_SIZE_HINT, bp);
            _sqlite3OsFetch(tls, pFd, 0, (int32)(~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp))), bp + 8);
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)) != 0) {
                _sqlite3OsUnfetch(tls, pFd, 0, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp + 8)));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sqlite3_package
