global using Dl_info = go.modernc.org.sqlite.lib.sqlite3_package.TDl_infoᴛ1;
global using TDl_info = go.modernc.org.sqlite.lib.sqlite3_package.TDl_infoᴛ1;
global using Tquad_t = long;
global using Tu_int16_t = ushort;
global using Tu_int32_t = uint;
global using Tu_int64_t = ulong;
global using Tu_int8_t = byte;
global using Tu_quad_t = ulong;
global using Twinsize = go.modernc.org.libc_package.Twinsizeᴛ1;
global using u_int16_t = ushort;
global using u_int32_t = uint;
global using u_int64_t = ulong;
global using u_int8_t = byte;
global using u_quad_t = ulong;
global using winsize = go.modernc.org.libc_package.Twinsizeᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt BIG_ENDIAN => 4321;

public static UntypedInt F_UNLCK => 2;

public static UntypedFloat HUGE => 3.4028234663852886e+38;

public static UntypedInt LITTLE_ENDIAN => 1234;

public static UntypedInt L_INCR => 1;

public static UntypedInt L_SET => 0;

public static UntypedInt L_XTND => 2;

public static UntypedInt MATH_ERREXCEPT => 2;

public static UntypedInt MATH_ERRNO => 1;

public static UntypedInt PDP_ENDIAN => 3412;

public static UntypedInt RAND_MAX => 2147483647;

partial struct TDl_infoᴛ1 /*dyn*/ {
    public uintptr Fdli_fname;
    public uintptr Fdli_fbase;
    public uintptr Fdli_sname;
    public uintptr Fdli_saddr;
}

public static UntypedInt TIOCM_CAR => 64;

public static UntypedInt TIOCM_CD => 64;

public static UntypedInt TIOCM_CTS => 32;

public static UntypedInt TIOCM_DSR => 256;

public static UntypedInt TIOCM_DTR => 2;

public static UntypedInt TIOCM_LE => 1;

public static UntypedInt TIOCM_RI => 128;

public static UntypedInt TIOCM_RNG => 128;

public static UntypedInt TIOCM_RTS => 4;

public static UntypedInt TIOCM_SR => 16;

public static UntypedInt TIOCM_ST => 8;

public static UntypedInt TIOCPKT_DATA => 0;

public static UntypedInt TIOCPKT_DOSTOP => 32;

public static UntypedInt TIOCPKT_FLUSHREAD => 1;

public static UntypedInt TIOCPKT_FLUSHWRITE => 2;

public static UntypedInt TIOCPKT_IOCTL => 64;

public static UntypedInt TIOCPKT_NOSTOP => 16;

public static UntypedInt TIOCPKT_START => 8;

public static UntypedInt TIOCPKT_STOP => 4;

internal static UntypedInt _IOLBF => 1;

internal static UntypedInt _IONBF => 2;

internal static int32 /*r*/ _walAssertLockmask(ref libc.TLS tls, uintptr pWal) {
    Tu32 mExpect = default;
    int32 v1 = default;
    int32 v2 = default;
    int32 v3 = default;
    uint32 v4 = default;
    _ = mExpect;
    _ = v1;
    _ = v2;
    _ = v3;
    _ = v4;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == 0) {
        if ((int32)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock < 0) {
            v1 = 0;
        } else {
            v1 = _S1.Lsh((int64)(libc.Int32FromInt32(3) + (int32)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock));
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock != 0) {
            v2 = _E1.Lsh((int64)(libc.Int32FromInt32(WAL_WRITE_LOCK)));
        } else {
            v2 = 0;
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock != 0) {
            v3 = _E1.Lsh((int64)(libc.Int32FromInt32(WAL_CKPT_LOCK)));
        } else {
            v3 = 0;
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpSnapshot != 0) {
            v4 = (Tu32)((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FlockMask & libc.Uint32FromInt32(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(WAL_CKPT_LOCK)))));
        } else {
            v4 = (uint32)0;
        }
        mExpect = libc.Uint32FromInt32(v1 | v2 | v3) | v4;
    }
    return (int32)1;
}

} // end sqlite3_package
