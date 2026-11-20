global using caddr_t = go.uintptr;
global using fsblkcnt_t = ulong;
global using fsfilcnt_t = ulong;
global using gid_t = uint;
global using uid_t = uint;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _robustFchown(ж<libc.TLS> tls, int32 fd, Tuid_t uid, Tgid_t gid) {
    int32 v1 = default;
    _ = v1;
    if (GoFuncCookie.Func<Func<ж<libc.TLS>, Tuid_t>>(_aSyscall[(int32)21].FpCurrent)(tls) != 0) {
        v1 = 0;
    } else {
        v1 = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, Tuid_t, Tgid_t, int32>>(_aSyscall[(int32)20].FpCurrent)(tls, fd, uid, gid);
    }
    return v1;
}

} // end sqlite3_package
