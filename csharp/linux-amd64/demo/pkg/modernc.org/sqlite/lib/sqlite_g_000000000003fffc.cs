global using Tfsblkcnt_t = ulong;
global using Tfsfilcnt_t = ulong;
global using Tint_fast16_t = int;
global using Tlocale_t = go.uintptr;
global using Tuint_fast16_t = uint;
global using locale_t = go.uintptr;
global using timer_t = go.uintptr;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt HAVE_FULLFSYNC => 0;

public static UntypedInt RTLD_GLOBAL => 256;

public static int32 /*r*/ Xmodernc_fdatasync(ж<libc.TLS> tls, int32 onoff) {
    int32 prev = default;
    _ = prev;
    prev = libc.AtomicLoadPInt32((uintptr)Ꮡ_moderncFdatasync);
    if (onoff >= 0) {
        libc.AtomicStorePInt32((uintptr)Ꮡ_moderncFdatasync, libc.BoolInt32(onoff != libc.Int32FromInt32(0)));
    }
    return prev;
}

public static readonly ж<uintptr> ᏑXsqlite3_temp_directory = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr Xsqlite3_temp_directory => ref ((StaticBox<uintptr>)ᏑXsqlite3_temp_directory).ValueSlot;

public static int32 /*r*/ Xsqlite3_vtab_on_conflict(ж<libc.TLS> tls, uintptr db) {
    return libc.Int32FromUint8(_aMap[libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FvtabOnConflict) - (int32)1]);
}

internal static UntypedInt _POSIX_JOB_CONTROL => 1;

internal static UntypedInt _POSIX_REGEXP => 1;

internal static UntypedInt _POSIX_SHELL => 1;

internal static UntypedInt __ELF__ => 1;

internal static UntypedInt __unix__ => 1;

internal static int32 /*r*/ _dotlockClose(ж<libc.TLS> tls, uintptr id) {
    uintptr pFile = default;
    _ = pFile;
    pFile = id;
    _dotlockUnlock(tls, id, NO_LOCK);
    Xsqlite3_free(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlockingContext);
    return _closeUnixFile(tls, id);
}

internal static int32 /*r*/ _full_fsync(ж<libc.TLS> tls, int32 fd, int32 fullSync, int32 dataOnly) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    _ = fullSync;
    _ = dataOnly;
    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_moderncFdatasync) != 0) {
        v1 = libc.Xfdatasync(tls, fd);
    } else {
        v1 = libc.Xfsync(tls, fd);
    }
    rc = v1;
    if (libc.Bool(OS_VXWORKS != 0) && rc != -(int32)1) {
        rc = 0;
    }
    return rc;
}

[GoInit] internal static void initΔ6() {
    @unsafe.Pointer p = @unsafe.Pointer.FromBox(Ꮡ_dotlockIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_dotlockIoFinderImpl);
}

} // end sqlite3_package
