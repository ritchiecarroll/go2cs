global using Tsuseconds_t = long;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _unixCurrentTimeInt64__ccgo_gp(ж<libc.TLS> tls, uintptr NotUsed, @unsafe.Pointer piNow) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        Ttimeval _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        libc.Xgettimeofday(tls, bp, (uintptr)0);
        (@unsafe.Pointer.To<Tsqlite3_int64>(piNow)).Value = _unixEpoch + (int64)1000 * (@unsafe.Pointer.To<Ttimeval>(__ccgo_ptr(bp))).Value.Ftv_sec + (@unsafe.Pointer.To<Ttimeval>(__ccgo_ptr(bp))).Value.Ftv_usec / (int64)1000;
        _ = NotUsed;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end sqlite3_package
