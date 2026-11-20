namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using @unsafe = unsafe_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _compare2pow63(ref libc.TLS tls, uintptr zNum, int32 incr) {
    int32 c = default;
    int32 i = default;
    uintptr pow63 = default;
    _ = c;
    _ = i;
    _ = pow63;
    c = 0;
    pow63 = __ccgo_ts + 1837;
    i = 0;
    for (;;) {
        if (!(c == 0 && i < (int32)18)) {
            break;
        }
        c = ((int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)(i * incr)))) - (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(pow63 + (uintptr)i)))) * (int32)10;
        goto _1;
_1:;
        i = i + 1;
    }
    if (c == 0) {
        c = (int32)(~@unsafe.Pointer.To<int8>(__ccgo_ptr(zNum + (uintptr)((int32)18 * incr)))) - (int32)'8';
    }
    return c;
}

} // end sqlite3_package
