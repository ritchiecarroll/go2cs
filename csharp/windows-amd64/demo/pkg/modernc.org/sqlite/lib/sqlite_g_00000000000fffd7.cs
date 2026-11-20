namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt SIG_ATOMIC_MAX => 2147483647;

public static UntypedInt SIG_ATOMIC_MIN => -2147483648;

internal static Tu64 /*r*/ _sqlite3Multiply128(ref libc.TLS tls, Tu64 a, Tu64 b, @unsafe.Pointer pLo) {
    Tu64 a0 = default;
    Tu64 a0b0 = default;
    Tu64 a0b1 = default;
    Tu64 a1 = default;
    Tu64 a1b0 = default;
    Tu64 a1b1 = default;
    Tu64 b0 = default;
    Tu64 b1 = default;
    Tu64 t = default;
    _ = a0;
    _ = a0b0;
    _ = a0b1;
    _ = a1;
    _ = a1b0;
    _ = a1b1;
    _ = b0;
    _ = b1;
    _ = t;
    a0 = (uint64)(uint32)a;
    a1 = a >> (int)((int32)32);
    b0 = (uint64)(uint32)b;
    b1 = b >> (int)((int32)32);
    a0b0 = a0 * b0;
    a1b1 = a1 * b1;
    a0b1 = a0 * b1;
    a1b0 = a1 * b0;
    t = (a0b0 >> (int)(libc.Int32FromInt32(32))) + (uint64)(uint32)a0b1 + (uint64)(uint32)a1b0;
    (@unsafe.Pointer.To<Tu64>(pLo)).Value = (Tu64)((Tu64)(a0b0 & (uint64)0xffffffffU) | t << (int)(libc.Int32FromInt32(32)));
    return a1b1 + (a0b1 >> (int)(libc.Int32FromInt32(32))) + (a1b0 >> (int)(libc.Int32FromInt32(32))) + (t >> (int)(libc.Int32FromInt32(32)));
}

} // end sqlite3_package
