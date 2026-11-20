namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static Tu64 /*r*/ _sqlite3BSwap64(ref libc.TLS tls, Tu64 x) {
    x = (Tu64)(x << (int)(libc.Int32FromInt32(32)) | x >> (int)(libc.Int32FromInt32(32)));
    x = (Tu64)((Tu64)(x & (uint64)0x0000ffff0000ffffUL) << (int)((int32)16) | (Tu64)(x & (uint64)0xffff0000ffff0000UL) >> (int)((int32)16));
    x = (Tu64)((Tu64)(x & (uint64)0x00ff00ff00ff00ffUL) << (int)((int32)8) | (Tu64)(x & (uint64)0xff00ff00ff00ff00UL) >> (int)((int32)8));
    return x;
}

} // end sqlite3_package
