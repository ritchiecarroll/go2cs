global using DbPath = go.modernc.org.sqlite.lib.sqlite3_package.TDbPathᴛ1;
global using TDbPath = go.modernc.org.sqlite.lib.sqlite3_package.TDbPathᴛ1;
global using TUnixUnusedFd = go.modernc.org.sqlite.lib.sqlite3_package.TUnixUnusedFdᴛ1;
global using Tblkcnt_t = long;
global using Tcaddr_t = go.uintptr;
global using Tfinder_type = go.uintptr;
global using Tgid_t = uint;
global using Tino_t = ulong;
global using Tuid_t = uint;
global using Tuint = uint;
global using TunixShm = go.modernc.org.sqlite.lib.sqlite3_package.TunixShmᴛ1;
global using TunixShmNode = go.modernc.org.sqlite.lib.sqlite3_package.TunixShmNodeᴛ1;
global using Tunix_syscall = go.modernc.org.sqlite.lib.sqlite3_package.Tunix_syscallᴛ1;
global using Tushort = ushort;
global using TvxworksFileId = go.modernc.org.sqlite.lib.sqlite3_package.TvxworksFileIdᴛ1;
global using UnixUnusedFd = go.modernc.org.sqlite.lib.sqlite3_package.TUnixUnusedFdᴛ1;
global using blkcnt_t = long;
global using blksize_t = long;
global using clockid_t = int;
global using finder_type = go.uintptr;
global using flock = go.modernc.org.libc_package.Tflockᴛ1;
global using id_t = uint;
global using itimerval = go.modernc.org.libc_package.Titimervalᴛ1;
global using key_t = int;
global using nlink_t = ulong;
global using quad_t = long;
global using sigset_t = go.modernc.org.sqlite.lib.sqlite3_package.Tcpu_set_tᴛ1;
global using stat = go.modernc.org.libc_package.Tstatᴛ1;
global using suseconds_t = long;
global using @uint = uint;
global using unixFile = go.modernc.org.sqlite.lib.sqlite3_package.TunixFileᴛ1;
global using unixFileId = go.modernc.org.sqlite.lib.sqlite3_package.TunixFileIdᴛ1;
global using unixInodeInfo = go.modernc.org.sqlite.lib.sqlite3_package.TunixInodeInfoᴛ1;
global using unixShm = go.modernc.org.sqlite.lib.sqlite3_package.TunixShmᴛ1;
global using unixShmNode = go.modernc.org.sqlite.lib.sqlite3_package.TunixShmNodeᴛ1;
global using unix_syscall = go.modernc.org.sqlite.lib.sqlite3_package.Tunix_syscallᴛ1;
global using @ushort = ushort;
global using vxworksFileId = go.modernc.org.sqlite.lib.sqlite3_package.TvxworksFileIdᴛ1;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using System.Runtime.InteropServices;
using go.arrays;
using go.golib;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt BUFSIZ => 1024;

public static readonly @string DOTLOCK_SUFFIX = ".lock"u8;

public static UntypedInt ENOTBLK => 15;

public static UntypedInt ETXTBSY => 26;

public static UntypedInt FD_CLOEXEC => 1;

public static UntypedInt F_DUPFD => 0;

public static UntypedInt F_GETFD => 1;

public static UntypedInt F_GETFL => 3;

public static UntypedInt F_LOCK => 1;

public static UntypedInt F_OK => 0;

public static UntypedInt F_SETFD => 2;

public static UntypedInt F_SETFL => 4;

public static UntypedInt F_TEST => 3;

public static UntypedInt F_TLOCK => 2;

public static UntypedInt F_ULOCK => 0;

public static UntypedInt HAVE_FCHMOD => 1;

public static UntypedInt HAVE_GETHOSTUUID => 0;

public static UntypedInt INTERFACE => 1;

public static UntypedInt ITIMER_PROF => 2;

public static UntypedInt ITIMER_REAL => 0;

public static UntypedInt ITIMER_VIRTUAL => 1;

public static UntypedInt MADV_DONTNEED => 4;

public static UntypedInt MADV_NORMAL => 0;

public static UntypedInt MADV_RANDOM => 1;

public static UntypedInt MADV_SEQUENTIAL => 2;

public static UntypedInt MADV_WILLNEED => 3;

public static UntypedInt MAP_FAILED => 18446744073709551615;

public static UntypedInt MAP_FILE => 0;

public static UntypedInt MAP_FIXED => 16;

public static UntypedInt MAP_PRIVATE => 2;

public static UntypedInt MAP_SHARED => 1;

public static UntypedInt MAX_PATHNAME => 512;

public static UntypedInt MS_ASYNC => 1;

public static UntypedInt O_BINARY => 0;

public static UntypedInt O_RDONLY => 0;

public static UntypedInt O_RDWR => 2;

public static UntypedInt O_WRONLY => 1;

public static UntypedInt POSIX_MADV_DONTNEED => 4;

public static UntypedInt POSIX_MADV_NORMAL => 0;

public static UntypedInt POSIX_MADV_RANDOM => 1;

public static UntypedInt POSIX_MADV_SEQUENTIAL => 2;

public static UntypedInt POSIX_MADV_WILLNEED => 3;

public static UntypedInt PROT_EXEC => 4;

public static UntypedInt PROT_NONE => 0;

public static UntypedInt PROT_READ => 1;

public static UntypedInt PROT_WRITE => 2;

public static UntypedInt RTLD_LAZY => 1;

public static UntypedInt RTLD_NEXT => 18446744073709551615;

public static UntypedInt RTLD_NOW => 2;

public static UntypedInt R_OK => 4;

public static UntypedInt SQLITE_DEFAULT_FILE_PERMISSIONS => 420;

public static UntypedInt SQLITE_DEFAULT_PROXYDIR_PERMISSIONS => 493;

public static UntypedInt SQLITE_FSFLAGS_IS_MSDOS => 1;

public static UntypedInt SQLITE_MAX_SYMLINKS => 100;

public static UntypedInt SQLITE_MINIMUM_FILE_DESCRIPTOR => 3;

public static UntypedInt SQLITE_OS_UNIX => 1;

public static UntypedInt S_IEXEC => 64;

public static UntypedInt S_IFBLK => 24576;

public static UntypedInt S_IFCHR => 8192;

public static UntypedInt S_IFDIR => 16384;

public static UntypedInt S_IFIFO => 4096;

public static UntypedInt S_IFLNK => 40960;

public static UntypedInt S_IFMT => 61440;

public static UntypedInt S_IFREG => 32768;

public static UntypedInt S_IFSOCK => 49152;

public static UntypedInt S_IREAD => 256;

public static UntypedInt S_IRGRP => 32;

public static UntypedInt S_IROTH => 4;

public static UntypedInt S_IRUSR => 256;

public static UntypedInt S_IRWXG => 56;

public static UntypedInt S_IRWXO => 7;

public static UntypedInt S_IRWXU => 448;

public static UntypedInt S_ISGID => 1024;

public static UntypedInt S_ISUID => 2048;

public static UntypedInt S_ISVTX => 512;

public static UntypedInt S_IWGRP => 16;

public static UntypedInt S_IWOTH => 2;

public static UntypedInt S_IWRITE => 128;

public static UntypedInt S_IWUSR => 128;

public static UntypedInt S_IXGRP => 8;

public static UntypedInt S_IXOTH => 1;

public static UntypedInt S_IXUSR => 64;

partial struct TDbPathᴛ1 /*dyn*/ {
    public int32 Frc;
    public int32 FnSymlink;
    public uintptr FzOut;
    public int32 FnOut;
    public int32 FnUsed;
}

public static UntypedInt TIME_UTC => 1;

partial struct TUnixUnusedFdᴛ1 /*dyn*/ {
    public int32 Ffd;
    public int32 Fflags;
    public uintptr FpNext;
}

partial struct TunixShmᴛ1 /*dyn*/ {
    public uintptr FpShmNode;
    public uintptr FpNext;
    public Tu8 FhasMutex;
    public Tu8 Fid;
    public Tu16 FsharedMask;
    public Tu16 FexclMask;
}

[global::go.golib.GoInlineLayout(Size = 96, Align = 8), StructLayout(LayoutKind.Explicit, Size = 96)] partial struct TunixShmNodeᴛ1 /*dyn*/ {
    [FieldOffset(0)] public uintptr FpInode;
    [FieldOffset(8)] public uintptr FpShmMutex;
    [FieldOffset(16)] public uintptr FzFilename;
    [FieldOffset(24)] public int32 FhShm;
    [FieldOffset(28)] public int32 FszRegion;
    [FieldOffset(32)] public Tu16 FnRegion;
    [FieldOffset(34)] public Tu8 FisReadonly;
    [FieldOffset(35)] public Tu8 FisUnlocked;
    [FieldOffset(40)] public uintptr FapRegion;
    [FieldOffset(48)] public int32 FnRef;
    [FieldOffset(56)] public uintptr FpFirst;
    [FieldOffset(64)] public array8<int32> FaLock;
}

partial struct Tunix_syscallᴛ1 /*dyn*/ {
    public uintptr FzName;
    public Tsqlite3_syscall_ptr FpCurrent;
    public Tsqlite3_syscall_ptr FpDefault;
}

partial struct TvxworksFileIdᴛ1 /*dyn*/ {
    public uintptr FpNext;
    public int32 FnRef;
    public int32 FnName;
    public uintptr FzCanonicalName;
}

public static UntypedInt UNIXFILE_DELETE => 32;

public static UntypedInt UNIXFILE_DIRSYNC => 8;

public static UntypedInt UNIXFILE_EXCL => 1;

public static UntypedInt UNIXFILE_NOLOCK => 128;

public static UntypedInt UNIXFILE_PERSIST_WAL => 4;

public static UntypedInt UNIXFILE_PSOW => 16;

public static UntypedInt UNIXFILE_RDONLY => 2;

public static UntypedInt UNIXFILE_URI => 64;

public static UntypedInt UNIX_SHM_BASE => 120;

public static UntypedInt UNIX_SHM_DMS => 128;

public static UntypedInt W_OK => 2;

public static UntypedInt X_OK => 1;

public static uintptr /*r*/ Xsqlite3_aggregate_context(ж<libc.TLS> tls, uintptr p, int32 nByte) {
    uintptr r = default;

    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpMem)).Value.Fflags) & (int32)MEM_Agg) == 0) {
        return _createAggContext(tls, p, nByte);
    } else {
        return (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)p)).Value.FpMem)).Value.Fz;
    }
    return r;
}

public static int32 /*r*/ Xsqlite3_backup_pagecount(ж<libc.TLS> tls, uintptr p) {
    return libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FnPagecount);
}

public static int32 /*r*/ Xsqlite3_backup_remaining(ж<libc.TLS> tls, uintptr p) {
    return libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3_backup>((@unsafe.Pointer)p)).Value.FnRemaining);
}

public static int32 /*r*/ Xsqlite3_bind_blob64(ж<libc.TLS> tls, uintptr pStmt, int32 i, uintptr zData, Tsqlite3_uint64 nData, uintptr __ccgo_fp_xDel) {
    return _bindText(tls, pStmt, i, zData, libc.Int64FromUint64(nData), __ccgo_fp_xDel, (uint8)0);
}

public static int32 /*r*/ Xsqlite3_bind_null(ж<libc.TLS> tls, uintptr pStmt, int32 i) {
    uintptr p = default;
    int32 rc = default;
    _ = p;
    _ = rc;
    p = pStmt;
    rc = _vdbeUnbind(tls, p, libc.Uint32FromInt32(i - libc.Int32FromInt32(1)));
    if (rc == SQLITE_OK) {
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb)).Value.Fmutex);
    }
    return rc;
}

public static int32 /*r*/ Xsqlite3_bind_value(ж<libc.TLS> tls, uintptr pStmt, int32 i, uintptr pValue) {
    int32 rc = default;
    float64 v1 = default;
    _ = rc;
    _ = v1;
    switch (Xsqlite3_value_type(tls, pValue)) {
    case var ᴛ when ᴛ == (int32)SQLITE_INTEGER: {
        rc = Xsqlite3_bind_int64(tls, pStmt, i, ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pValue));
        break;
    }
    case var ᴛ when ᴛ == (int32)SQLITE_FLOAT: {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fflags) & (int32)MEM_Real) != 0) {
            v1 = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pValue);
        } else {
            v1 = (float64)(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pValue));
        }
        rc = Xsqlite3_bind_double(tls, pStmt, i, v1);
        break;
    }
    case var ᴛ when ᴛ == (int32)SQLITE_BLOB: {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fflags) & (int32)MEM_Zero) != 0) {
            rc = Xsqlite3_bind_zeroblob(tls, pStmt, i, ~@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue).of(Tsqlite3_value.ᏑFu).Reinterpret<TMemValue, int32>());
        } else {
            rc = Xsqlite3_bind_blob(tls, pStmt, i, (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fz, (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fn, (uintptr)(-libc.Int32FromInt32(1)));
        }
        break;
    }
    case var ᴛ when ᴛ == (int32)SQLITE_TEXT: {
        rc = _bindText(tls, pStmt, i, (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fz, (int64)(@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fn, (uintptr)(-libc.Int32FromInt32(1)), (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pValue)).Value.Fenc);
        break;
    }
    default: {
        rc = Xsqlite3_bind_null(tls, pStmt, i);
        break;
        break;
    }}

    return rc;
}

public static int32 /*r*/ Xsqlite3_column_count(ж<libc.TLS> tls, uintptr pStmt) {
    uintptr pVm = default;
    _ = pVm;
    pVm = pStmt;
    if (pVm == (uintptr)0) {
        return 0;
    }
    return libc.Int32FromUint16((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVm)).Value.FnResColumn);
}

public static int32 /*r*/ Xsqlite3_create_collation_v2(ж<libc.TLS> tls, uintptr db, uintptr zName, int32 enc, uintptr pCtx, uintptr __ccgo_fp_xCompare, uintptr __ccgo_fp_xDel) {
    int32 rc = default;
    _ = rc;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    rc = _createCollation(tls, db, zName, libc.Uint8FromInt32(enc), pCtx, __ccgo_fp_xCompare, __ccgo_fp_xDel);
    rc = _sqlite3ApiExit(tls, db, rc);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

public static int32 /*r*/ Xsqlite3_data_count(ж<libc.TLS> tls, uintptr pStmt) {
    uintptr pVm = default;
    _ = pVm;
    pVm = pStmt;
    if (pVm == (uintptr)0 || (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVm)).Value.FpResultRow == (uintptr)0) {
        return 0;
    }
    return libc.Int32FromUint16((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)pVm)).Value.FnResColumn);
}

public static int32 /*r*/ Xsqlite3_extended_result_codes(ж<libc.TLS> tls, uintptr db, int32 onoff) {
    uint32 v1 = default;
    _ = v1;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (onoff != 0) {
        v1 = (uint32)0xffffffffU;
    } else {
        v1 = (uint32)0xff;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrMask = libc.Int32FromUint32(v1);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3_get_autocommit(ж<libc.TLS> tls, uintptr db) {
    int32 iRet = default;
    _ = iRet;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    iRet = libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FautoCommit);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return iRet;
}

public static int32 /*r*/ Xsqlite3_keyword_name(ж<libc.TLS> tls, int32 i, uintptr pzName, uintptr pnName) {
    if (i < 0 || i >= (int32)SQLITE_N_KEYWORD) {
        return (int32)SQLITE_ERROR;
    }
    i = i + 1;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzName))).Value = (uintptr)Ꮡ_zKWText + (uintptr)_aKWOffset[i];
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnName))).Value = libc.Int32FromUint8(_aKWLen[i]);
    return SQLITE_OK;
}

public static uintptr /*r*/ Xsqlite3_malloc(ж<libc.TLS> tls, int32 n) {
    uintptr v1 = default;
    _ = v1;
    if (Xsqlite3_initialize(tls) != 0) {
        return (uintptr)0;
    }
    if (n <= 0) {
        v1 = (uintptr)0;
    } else {
        v1 = _sqlite3Malloc(tls, libc.Uint64FromInt32(n));
    }
    return v1;
}

public static Tsqlite3_uint64 /*r*/ Xsqlite3_msize(ж<libc.TLS> tls, uintptr p) {
    int32 v1 = default;
    _ = v1;
    if (p != 0) {
        v1 = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(_sqlite3Config.Fm.FxSize)(tls, p);
    } else {
        v1 = 0;
    }
    return libc.Uint64FromInt32(v1);
}

public static int32 /*r*/ Xsqlite3_open(ж<libc.TLS> tls, uintptr zFilename, uintptr ppDb) {
    return _openDatabase(tls, zFilename, (@unsafe.Pointer)ppDb, libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE)), (uintptr)0);
}

public static int32 /*r*/ Xsqlite3_preupdate_count(ж<libc.TLS> tls, uintptr db) {
    uintptr p = default;
    int32 v1 = default;
    _ = p;
    _ = v1;
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpPreUpdate;
    if (p != 0) {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TPreUpdate>((@unsafe.Pointer)p)).Value.FpKeyinfo)).Value.FnKeyField);
    } else {
        v1 = 0;
    }
    return v1;
}

public static void Xsqlite3_progress_handler(ж<libc.TLS> tls, uintptr db, int32 nOps, uintptr __ccgo_fp_xProgress, uintptr pArg) {
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if (nOps > 0) {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProgress = __ccgo_fp_xProgress;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnProgressOps = libc.Uint32FromInt32(nOps);
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProgressArg = pArg;
    } else {
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProgress = (uintptr)0;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnProgressOps = (uint32)0;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProgressArg = (uintptr)0;
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
}

public static uintptr /*r*/ Xsqlite3_realloc(ж<libc.TLS> tls, uintptr pOld, int32 n) {
    if (Xsqlite3_initialize(tls) != 0) {
        return (uintptr)0;
    }
    if (n < 0) {
        n = 0;
    }
    return _sqlite3Realloc(tls, pOld, libc.Uint64FromInt32(n));
}

public static void Xsqlite3_result_blob64(ж<libc.TLS> tls, uintptr pCtx, uintptr z, Tsqlite3_uint64 n, uintptr __ccgo_fp_xDel) {
    if (n > (uint64)0x7fffffff) {
        _invokeValueDestructor(tls, z, __ccgo_fp_xDel, pCtx);
    } else {
        _setResultStrOrError(tls, pCtx, z, libc.Int32FromUint64(n), (uint8)0, __ccgo_fp_xDel);
    }
}

public static void Xsqlite3_result_error_code(ж<libc.TLS> tls, uintptr pCtx, int32 errCode) {
    int32 v1 = default;
    _ = v1;
    if (errCode != 0) {
        v1 = errCode;
    } else {
        v1 = -(int32)1;
    }
    (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FisError = v1;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpOut)).Value.Fflags) & (int32)MEM_Null) != 0) {
        _setResultStrOrError(tls, pCtx, _sqlite3ErrStr(ref (tls).DerefOrNull(), errCode), -(int32)1, (uint8)SQLITE_UTF8, libc.UintptrFromInt32(0));
    }
}

public static void Xsqlite3_result_text16be(ж<libc.TLS> tls, uintptr pCtx, uintptr z, int32 n, uintptr __ccgo_fp_xDel) {
    _setResultStrOrError(tls, pCtx, z, libc.Int32FromUint64(libc.Uint64FromInt32(n) & ~libc.Uint64FromInt32(1)), (uint8)SQLITE_UTF16BE, __ccgo_fp_xDel);
}

public static void Xsqlite3_result_text16le(ж<libc.TLS> tls, uintptr pCtx, uintptr z, int32 n, uintptr __ccgo_fp_xDel) {
    _setResultStrOrError(tls, pCtx, z, libc.Int32FromUint64(libc.Uint64FromInt32(n) & ~libc.Uint64FromInt32(1)), (uint8)SQLITE_UTF16LE, __ccgo_fp_xDel);
}

public static void Xsqlite3_result_value(ж<libc.TLS> tls, uintptr pCtx, uintptr pValue) {
    uintptr pOut = default;
    _ = pOut;
    pOut = (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpOut;
    _sqlite3VdbeMemCopy(tls, pOut, pValue);
    _sqlite3VdbeChangeEncoding(tls, pOut, libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.Fenc));
    if (_sqlite3VdbeMemTooBig(ref (tls).DerefOrNull(), pOut) != 0) {
        Xsqlite3_result_error_toobig(tls, pCtx);
    }
}

public static void Xsqlite3_result_zeroblob(ж<libc.TLS> tls, uintptr pCtx, int32 n) {
    int32 v1 = default;
    _ = v1;
    if (n > 0) {
        v1 = n;
    } else {
        v1 = 0;
    }
    Xsqlite3_result_zeroblob64(tls, pCtx, libc.Uint64FromInt32(v1));
}

public static int32 /*r*/ Xsqlite3_stmt_busy(ж<libc.TLS> tls, uintptr pStmt) {
    uintptr v = default;
    _ = v;
    v = pStmt;
    return libc.BoolInt32(v != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FeVdbeState) == (int32)VDBE_RUN_STATE);
}

public static int32 /*r*/ Xsqlite3_str_errcode(ж<libc.TLS> tls, uintptr p) {
    int32 v1 = default;
    _ = v1;
    if (p != 0) {
        v1 = libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FaccError);
    } else {
        v1 = (int32)SQLITE_NOMEM;
    }
    return v1;
}

public static int32 /*r*/ Xsqlite3_str_length(ж<libc.TLS> tls, uintptr p) {
    uint32 v1 = default;
    _ = v1;
    if (p != 0) {
        v1 = (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)p)).Value.FnChar;
    } else {
        v1 = (uint32)0;
    }
    return libc.Int32FromUint32(v1);
}

public static int32 /*r*/ Xsqlite3_strnicmp(ж<libc.TLS> tls, uintptr zLeft, uintptr zRight, int32 N) {
    uintptr a = default;
    uintptr b = default;
    int32 v1 = default;
    _ = a;
    _ = b;
    _ = v1;
    if (zLeft == (uintptr)0) {
        if (zRight != 0) {
            v1 = -(int32)1;
        } else {
            v1 = 0;
        }
        return v1;
    } else {
        if (zRight == (uintptr)0) {
            return (int32)1;
        }
    }
    a = zLeft;
    b = zRight;
    for (;;) {
        v1 = N;
        N = N - 1;
        if (!(v1 > 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a))) != 0 && libc.Int32FromUint8(_sqlite3UpperToLower[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a))]) == libc.Int32FromUint8(_sqlite3UpperToLower[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(b))]))) {
            break;
        }
        a = a + 1;
        b = b + 1;
    }
    if (N < 0) {
        v1 = 0;
    } else {
        v1 = libc.Int32FromUint8(_sqlite3UpperToLower[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a))]) - libc.Int32FromUint8(_sqlite3UpperToLower[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(b))]);
    }
    return v1;
}

public static int32 /*r*/ Xsqlite3_uri_boolean(ж<libc.TLS> tls, uintptr zFilename, uintptr zParam, int32 bDflt) {
    uintptr z = default;
    int32 v1 = default;
    _ = z;
    _ = v1;
    z = Xsqlite3_uri_parameter(tls, zFilename, zParam);
    bDflt = libc.BoolInt32(bDflt != 0);
    if (z != 0) {
        v1 = libc.Int32FromUint8(_sqlite3GetBoolean(tls, z, libc.Uint8FromInt32(bDflt)));
    } else {
        v1 = bDflt;
    }
    return v1;
}

public static int32 /*r*/ Xsqlite3_value_encoding(ж<libc.TLS> tls, uintptr pVal) {
    return libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fenc);
}

public static int32 /*r*/ Xsqlite3_value_frombind(ж<libc.TLS> tls, uintptr pVal) {
    return libc.BoolInt32((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (int32)MEM_FromBind) != 0);
}

public static int32 /*r*/ Xsqlite3_value_nochange(ж<libc.TLS> tls, uintptr pVal) {
    return libc.BoolInt32((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (libc.Int32FromInt32(MEM_Null) | libc.Int32FromInt32(MEM_Zero))) == (libc.Int32FromInt32(MEM_Null) | libc.Int32FromInt32(MEM_Zero)));
}

public static uint32 /*r*/ Xsqlite3_value_subtype(ж<libc.TLS> tls, uintptr pVal) {
    uintptr pMem = default;
    int32 v1 = default;
    _ = pMem;
    _ = v1;
    pMem = pVal;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Subtype) != 0) {
        v1 = libc.Int32FromUint8((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FeSubtype);
    } else {
        v1 = 0;
    }
    return libc.Uint32FromInt32(v1);
}

public static int32 /*r*/ Xsqlite3_value_type(ж<libc.TLS> tls, uintptr pVal) {
    return libc.Int32FromUint8(_aType[(libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (int32)MEM_AffMask)]);
}

public static int32 /*r*/ Xsqlite3_vtab_config(ж<libc.TLS> tls, uintptr db, int32 op, uintptr va) {
    ref var ap = ref heap(new Tva_list(), out var Ꮡap);
    uintptr p = default;
    int32 rc = default;
    _ = ap;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVtabCtx;
    if (!(p != 0)) {
        rc = _sqlite3MisuseError(tls, (int32)164384);
    } else {
        ap = va;
        switch (op) {
        case var ᴛ when ᴛ == (int32)SQLITE_VTAB_CONSTRAINT_SUPPORT: {
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)p)).Value.FpVTable)).Value.FbConstraint = libc.Uint8FromInt32(libc.VaInt32(Ꮡap));
            break;
        }
        case var ᴛ when ᴛ == (int32)SQLITE_VTAB_INNOCUOUS: {
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)p)).Value.FpVTable)).Value.FeVtabRisk = (uint8)SQLITE_VTABRISK_Low;
            break;
        }
        case var ᴛ when ᴛ == (int32)SQLITE_VTAB_DIRECTONLY: {
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)p)).Value.FpVTable)).Value.FeVtabRisk = (uint8)SQLITE_VTABRISK_High;
            break;
        }
        case var ᴛ when ᴛ == (int32)SQLITE_VTAB_USES_ALL_SCHEMAS: {
            (@unsafe.Pointer.To<TVTable>((@unsafe.Pointer)(@unsafe.Pointer.To<TVtabCtx>((@unsafe.Pointer)p)).Value.FpVTable)).Value.FbAllSchemas = (uint8)1;
            break;
        }
        default: {
            rc = _sqlite3MisuseError(tls, (int32)164406);
            break;
            break;
        }}

        _ = ap;
    }
    if (rc != SQLITE_OK) {
        _sqlite3Error(tls, db, rc);
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

internal static UntypedInt __SIZEOF_WCHAR_T__ => 4;

internal static UntypedInt __SIZEOF_WINT_T__ => 4;

internal static UntypedInt __WCHAR_WIDTH__ => 32;

internal static UntypedInt __WINT_WIDTH__ => 32;

internal static readonly @string __restrict_arr = "restrict"u8;

internal static array<Tu16> _aFlag = new array<Tu16>(2){
    [0] = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Blob) | libc.Int32FromInt32(MEM_Ephem)),
    [1] = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Ephem))
};

internal static array<Tu16> _aFlag1 = new array<Tu16>(2){
    [0] = (uint16)MEM_Blob,
    [1] = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Term))
};


partial struct _aKeywordᴛ1 /*dyn*/ {
    public Tu8 Fi;
    public Tu8 FnChar;
    public Tu8 Fcode;
}
internal static array<_aKeywordᴛ1> _aKeyword = new array<_aKeywordᴛ1>(7){
    [0] = new(
        FnChar: (uint8)7,
        Fcode: (uint8)JT_NATURAL
    ),
    [1] = new(
        Fi: (uint8)6,
        FnChar: (uint8)4,
        Fcode: libc.Uint8FromInt32(libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_OUTER))
    ),
    [2] = new(
        Fi: (uint8)10,
        FnChar: (uint8)5,
        Fcode: (uint8)JT_OUTER
    ),
    [3] = new(
        Fi: (uint8)14,
        FnChar: (uint8)5,
        Fcode: libc.Uint8FromInt32(libc.Int32FromInt32(JT_RIGHT) | libc.Int32FromInt32(JT_OUTER))
    ),
    [4] = new(
        Fi: (uint8)19,
        FnChar: (uint8)4,
        Fcode: libc.Uint8FromInt32(libc.Int32FromInt32(JT_LEFT) | libc.Int32FromInt32(JT_RIGHT) | libc.Int32FromInt32(JT_OUTER))
    ),
    [5] = new(
        Fi: (uint8)23,
        FnChar: (uint8)5,
        Fcode: (uint8)JT_INNER
    ),
    [6] = new(
        Fi: (uint8)28,
        FnChar: (uint8)5,
        Fcode: libc.Uint8FromInt32(libc.Int32FromInt32(JT_INNER) | libc.Int32FromInt32(JT_CROSS))
    )
};

internal static readonly ж<array<TFuncDef>> Ꮡ_aWindowFuncs = new StaticBox<array<TFuncDef>>(default(array<TFuncDef>));
internal static ref array<TFuncDef> _aWindowFuncs => ref ((StaticBox<array<TFuncDef>>)Ꮡ_aWindowFuncs).ValueSlot;
internal static void initᴛ_aWindowFuncs() { _aWindowFuncs = new array<TFuncDef>(15, () => new()){
    [0] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_row_numberName
    ),
    [1] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_dense_rankName
    ),
    [2] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_rankName
    ),
    [3] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_percent_rankName
    ),
    [4] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_cume_distName
    ),
    [5] = new(
        FnArg: (int16)1,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_ntileName
    ),
    [6] = new(
        FnArg: (int16)1,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_last_valueName
    ),
    [7] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_nth_valueName
    ),
    [8] = new(
        FnArg: (int16)1,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_first_valueName
    ),
    [9] = new(
        FnArg: (int16)1,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_leadName
    ),
    [10] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_leadName
    ),
    [11] = new(
        FnArg: (int16)3,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_leadName
    ),
    [12] = new(
        FnArg: (int16)1,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_lagName
    ),
    [13] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_lagName
    ),
    [14] = new(
        FnArg: (int16)3,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_WINDOW) | libc.Int32FromInt32(0)),
        FzName: (uintptr)Ꮡ_lagName
    )
}; }

internal static int32 /*r*/ _accessPayloadChecked(ж<libc.TLS> tls, uintptr pCur, Tu32 offset, Tu32 amt, uintptr pBuf) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == (int32)CURSOR_INVALID) {
        return (int32)SQLITE_ABORT;
    }
    rc = _btreeRestoreCursorPosition(tls, pCur);
    if (rc != 0) {
        v1 = rc;
    } else {
        v1 = _accessPayload(tls, pCur, offset, amt, pBuf, 0);
    }
    return v1;
}

internal static void _addArgumentToVtab(ж<libc.TLS> tls, uintptr pParse) {
    uintptr db = default;
    uintptr z = default;
    int32 n = default;
    _ = db;
    _ = n;
    _ = z;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FsArg.Fz != 0 && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTable != 0) {
        z = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FsArg.Fz;
        n = libc.Int32FromUint32((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FsArg.Fn);
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        _addModuleArgument(tls, pParse, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpNewTable, _sqlite3DbStrNDup(tls, db, z, libc.Uint64FromInt32(n)));
    }
}

internal static int32 /*r*/ _anotherValidCursor(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pOther = default;
    _ = pOther;
    pOther = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt)).Value.FpCursor;
    for (;;) {
        if (!(pOther != 0)) {
            break;
        }
        if (pOther != pCur && libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pOther)).Value.FeState) == CURSOR_VALID && (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pOther)).Value.FpPage == (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage) {
            return _sqlite3CorruptError(tls, (int32)82292);
        }
        goto _1;
_1:;
        pOther = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pOther)).Value.FpNext;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _blobReadWrite(ж<libc.TLS> tls, uintptr pBlob, uintptr z, int32 n, int32 iOffset, uintptr __ccgo_fp_xCall) {
    ref var bDiff = ref heap(new int32(), out var ᏑbDiff);
    int32 rc = default;
    uintptr db = default;
    uintptr p = default;
    uintptr v = default;
    Tsqlite3_int64 iKey = default;
    _ = bDiff;
    _ = db;
    _ = iKey;
    _ = p;
    _ = rc;
    _ = v;
    rc = SQLITE_OK;
    p = pBlob;
    if (p == (uintptr)0) {
        return _sqlite3MisuseError(tls, (int32)106665);
    }
    db = (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.Fdb;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    v = (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpStmt;
    if (n < 0 || iOffset < 0 || (int64)iOffset + (int64)n > (int64)(@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FnByte) {
        rc = (int32)SQLITE_ERROR;
    } else {
        if (v == (uintptr)0) {
            rc = (int32)SQLITE_ABORT;
        } else {
            _sqlite3BtreeEnterCursor(tls, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr);
            if (__ccgo_fp_xCall == __ccgo_fp(_sqlite3BtreePutData) && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxPreUpdateCallback != 0) {
                if (_sqlite3BtreeCursorIsValidNN(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr) == 0) {
                    bDiff = 0;
                    rc = _sqlite3BtreeCursorRestore(tls, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr, @unsafe.Pointer.FromPinnedBox(ᏑbDiff));
                }
                if (_sqlite3BtreeCursorIsValidNN(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr) != 0) {
                    iKey = _sqlite3BtreeIntegerKey(tls, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr);
                    _sqlite3VdbePreUpdateHook(tls, v, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FapCsr)), (int32)SQLITE_DELETE, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FzDb, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpTab, iKey, -(int32)1, libc.Int32FromUint16((@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FiCol));
                }
            }
            if (rc == SQLITE_OK) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, Tu32, Tu32, uintptr, int32>>(__ccgo_fp_xCall)(tls, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr, libc.Uint32FromInt32(iOffset + (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FiOffset), libc.Uint32FromInt32(n), z);
            }
            _sqlite3BtreeLeaveCursor(tls, (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpCsr);
            if (rc == (int32)SQLITE_ABORT) {
                _sqlite3VdbeFinalize(tls, v);
                (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpStmt = (uintptr)0;
            } else {
                (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.Frc = rc;
            }
        }
    }
    _sqlite3Error(tls, db, rc);
    rc = _sqlite3ApiExit(tls, db, rc);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    return rc;
}

internal static void _bothImplyNotNullRow(ж<libc.TLS> tls, uintptr pWalker, uintptr pE1, uintptr pE2) {
    if (libc.Int32FromUint16((@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode) == 0) {
        _sqlite3WalkExpr(tls, pWalker, pE1);
        if ((@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode != 0) {
            (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode = (uint16)0;
            _sqlite3WalkExpr(tls, pWalker, pE2);
        }
    }
}

internal static int32 /*r*/ _btreeComputeFreeSpace(ж<libc.TLS> tls, uintptr pPage) {
    uintptr data = default;
    Tu8 hdr = default;
    int32 iCellFirst = default;
    int32 iCellLast = default;
    int32 nFree = default;
    int32 pc = default;
    int32 top = default;
    int32 usableSize = default;
    Tu32 next = default;
    Tu32 size = default;
    _ = data;
    _ = hdr;
    _ = iCellFirst;
    _ = iCellLast;
    _ = nFree;
    _ = next;
    _ = pc;
    _ = size;
    _ = top;
    _ = usableSize;
    usableSize = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize);
    hdr = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData;
    top = (((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)5)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)5) + 1)))) - (int32)1) & (int32)0xffff) + (int32)1;
    iCellFirst = libc.Int32FromUint8(hdr) + (int32)8 + libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize) + (int32)2 * libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell);
    iCellLast = usableSize - (int32)4;
    pc = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)1)))) << (int)((int32)8) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)1) + 1)));
    nFree = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(libc.Int32FromUint8(hdr) + (int32)7)))) + top;
    if (pc > 0) {
        if (pc < top) {
            return _sqlite3CorruptError(tls, (int32)75305);
        }
        while ((int32)1 != 0) {
            if (pc > iCellLast) {
                return _sqlite3CorruptError(tls, (int32)75310);
            }
            next = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)pc))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)pc + 1))));
            size = libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(pc + (int32)2)))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + (uintptr)(pc + (int32)2) + 1))));
            if (size < (uint32)4 && _sqlite3FaultSim(tls, (int32)422) == SQLITE_OK) {
                return _sqlite3CorruptError(tls, (int32)75319);
            }
            nFree = libc.Int32FromUint32(libc.Uint32FromInt32(nFree) + size);
            if (next < libc.Uint32FromInt32(pc) + size + (uint32)4) {
                break;
            }
            pc = libc.Int32FromUint32(next);
        }
        if (next > (uint32)0) {
            return _sqlite3CorruptError(tls, (int32)75327);
        }
        if (libc.Uint32FromInt32(pc) + size > libc.Uint32FromInt32(usableSize)) {
            return _sqlite3CorruptError(tls, (int32)75331);
        }
    }
    if (nFree > usableSize || nFree < iCellFirst) {
        return _sqlite3CorruptError(tls, (int32)75343);
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree = libc.Int32FromUint16(libc.Uint16FromInt32(nFree - iCellFirst));
    return SQLITE_OK;
}

internal static int32 /*r*/ _btreeCreateTable(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer piTable, int32 createTabFlags) {
    ref var eType = ref heap(new Tu8(), out var ᏑeType);
    ref var iPtrPage = ref heap(new TPgno(), out var ᏑiPtrPage);
    ref var pgnoMove = ref heap(new TPgno(), out var ᏑpgnoMove);
    ref var pgnoRoot = ref heap(new TPgno(), out var ᏑpgnoRoot);
    uintptr pBt = default;
    ref var pPageMove = ref heap(new uintptr(), out var ᏑpPageMove);
    ref var pRoot = ref heap(new uintptr(), out var ᏑpRoot);
    int32 ptfFlags = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = eType;
    _ = iPtrPage;
    _ = pBt;
    _ = pPageMove;
    _ = pRoot;
    _ = pgnoMove;
    _ = pgnoRoot;
    _ = ptfFlags;
    _ = rc;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
        _invalidateAllOverflowCache(ref (tls).DerefOrNull(), pBt);
        _sqlite3BtreeGetMeta(tls, p, (int32)BTREE_LARGEST_ROOT_PAGE, @unsafe.Pointer.FromPinnedBox(ᏑpgnoRoot));
        if (pgnoRoot > _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
            return _sqlite3CorruptError(tls, (int32)83266);
        }
        pgnoRoot = pgnoRoot + 1;
        while (pgnoRoot == _ptrmapPageno(ref (tls).DerefOrNull(), pBt, pgnoRoot) || pgnoRoot == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
            pgnoRoot = pgnoRoot + 1;
        }
        rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpPageMove), @unsafe.Pointer.FromPinnedBox(ᏑpgnoMove), pgnoRoot, (uint8)BTALLOC_EXACT);
        if (rc != SQLITE_OK) {
            return rc;
        }
        if (pgnoMove != pgnoRoot) {
            eType = (uint8)0;
            iPtrPage = (uint32)0;
            rc = _saveAllCursors(tls, pBt, (uint32)0, (uintptr)0);
            _releasePage(tls, pPageMove);
            if (rc != SQLITE_OK) {
                return rc;
            }
            rc = _btreeGetPage(tls, pBt, pgnoRoot, @unsafe.Pointer.FromBox(ᏑpRoot), 0);
            if (rc != SQLITE_OK) {
                return rc;
            }
            rc = _ptrmapGet(tls, pBt, pgnoRoot, @unsafe.Pointer.FromPinnedBox(ᏑeType), @unsafe.Pointer.FromPinnedBox(ᏑiPtrPage));
            if (libc.Int32FromUint8(eType) == (int32)PTRMAP_ROOTPAGE || libc.Int32FromUint8(eType) == (int32)PTRMAP_FREEPAGE) {
                rc = _sqlite3CorruptError(tls, (int32)83314);
            }
            if (rc != SQLITE_OK) {
                _releasePage(tls, pRoot);
                return rc;
            }
            rc = _relocatePage(tls, pBt, pRoot, eType, iPtrPage, pgnoMove, 0);
            _releasePage(tls, pRoot);
            if (rc != SQLITE_OK) {
                return rc;
            }
            rc = _btreeGetPage(tls, pBt, pgnoRoot, @unsafe.Pointer.FromBox(ᏑpRoot), 0);
            if (rc != SQLITE_OK) {
                return rc;
            }
            rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FpDbPage);
            if (rc != SQLITE_OK) {
                _releasePage(tls, pRoot);
                return rc;
            }
        } else {
            pRoot = pPageMove;
        }
        _ptrmapPut(tls, pBt, pgnoRoot, (uint8)PTRMAP_ROOTPAGE, (uint32)0, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        if (rc != 0) {
            _releasePage(tls, pRoot);
            return rc;
        }
        rc = _sqlite3BtreeUpdateMeta(tls, p, (int32)4, pgnoRoot);
        if (rc != 0) {
            _releasePage(tls, pRoot);
            return rc;
        }
    } else {
        rc = _allocateBtreePage(tls, pBt, @unsafe.Pointer.FromBox(ᏑpRoot), @unsafe.Pointer.FromPinnedBox(ᏑpgnoRoot), (uint32)1, (uint8)0);
        if (rc != 0) {
            return rc;
        }
    }
    if ((createTabFlags & (int32)BTREE_INTKEY) != 0) {
        ptfFlags = libc.Int32FromInt32(PTF_INTKEY) | libc.Int32FromInt32(PTF_LEAFDATA) | libc.Int32FromInt32(PTF_LEAF);
    } else {
        ptfFlags = libc.Int32FromInt32(PTF_ZERODATA) | libc.Int32FromInt32(PTF_LEAF);
    }
    _zeroPage(tls, pRoot, ptfFlags);
    _sqlite3PagerUnref(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pRoot)).Value.FpDbPage);
    (@unsafe.Pointer.To<TPgno>(piTable)).Value = pgnoRoot;
    return SQLITE_OK;
}

internal static int32 /*r*/ _btreeDropTable(ж<libc.TLS> tls, uintptr p, TPgno iTable, @unsafe.Pointer piMoved) {
    ref var maxRootPgno = ref heap(new TPgno(), out var ᏑmaxRootPgno);
    uintptr pBt = default;
    ref var pMove = ref heap(new uintptr(), out var ᏑpMove);
    ref var pPage = ref heap(new uintptr(), out var ᏑpPage);
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = maxRootPgno;
    _ = pBt;
    _ = pMove;
    _ = pPage;
    _ = rc;
    pPage = (uintptr)0;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    if (iTable > _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
        return _sqlite3CorruptError(tls, (int32)83529);
    }
    rc = _sqlite3BtreeClearTable(tls, p, libc.Int32FromUint32(iTable), nil);
    if (rc != 0) {
        return rc;
    }
    rc = _btreeGetPage(tls, pBt, iTable, @unsafe.Pointer.FromBox(ᏑpPage), 0);
    if (rc != 0) {
        _releasePage(tls, pPage);
        return rc;
    }
    (@unsafe.Pointer.To<int32>(piMoved)).Value = 0;
    if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
        _sqlite3BtreeGetMeta(tls, p, (int32)BTREE_LARGEST_ROOT_PAGE, @unsafe.Pointer.FromPinnedBox(ᏑmaxRootPgno));
        if (iTable == maxRootPgno) {
            _freePage(tls, pPage, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            _releasePage(tls, pPage);
            if (rc != SQLITE_OK) {
                return rc;
            }
        } else {
            _releasePage(tls, pPage);
            rc = _btreeGetPage(tls, pBt, maxRootPgno, @unsafe.Pointer.FromBox(ᏑpMove), 0);
            if (rc != SQLITE_OK) {
                return rc;
            }
            rc = _relocatePage(tls, pBt, pMove, (uint8)PTRMAP_ROOTPAGE, (uint32)0, iTable, 0);
            _releasePage(tls, pMove);
            if (rc != SQLITE_OK) {
                return rc;
            }
            pMove = (uintptr)0;
            rc = _btreeGetPage(tls, pBt, maxRootPgno, @unsafe.Pointer.FromBox(ᏑpMove), 0);
            if (rc == SQLITE_OK) {
                rc = _sqlite3PagerWrite(tls, (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pMove)).Value.FpDbPage);
            }
            _freePage(tls, pMove, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
            _releasePage(tls, pMove);
            if (rc != SQLITE_OK) {
                return rc;
            }
            (@unsafe.Pointer.To<int32>(piMoved)).Value = libc.Int32FromUint32(maxRootPgno);
        }
        maxRootPgno = maxRootPgno - 1;
        while (maxRootPgno == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1) || _ptrmapPageno(ref (tls).DerefOrNull(), pBt, maxRootPgno) == maxRootPgno) {
            maxRootPgno = maxRootPgno - 1;
        }
        rc = _sqlite3BtreeUpdateMeta(tls, p, (int32)4, maxRootPgno);
    } else {
        _freePage(tls, pPage, @unsafe.Pointer.FromPinnedBox(Ꮡrc));
        _releasePage(tls, pPage);
    }
    return rc;
}

internal static void _btreeEndTransaction(ж<libc.TLS> tls, uintptr p) {
    uintptr db = default;
    uintptr pBt = default;
    _ = db;
    _ = pBt;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    db = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb;
    (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbDoTruncate = (uint8)0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) > TRANS_NONE && (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnVdbeRead > (int32)1) {
        _downgradeAllSharedCacheTableLocks(ref (tls).DerefOrNull(), p);
        (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans = (uint8)TRANS_READ;
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) != TRANS_NONE) {
            _clearAllSharedCacheTableLocks(tls, p);
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnTransaction = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnTransaction - 1;
            if (0 == (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnTransaction) {
                (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction = (uint8)TRANS_NONE;
            }
        }
        (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans = (uint8)TRANS_NONE;
        _unlockBtreeIfUnused(tls, pBt);
    }
}

internal static int32 /*r*/ _btreeInitPage(ж<libc.TLS> tls, uintptr pPage) {
    uintptr data = default;
    uintptr pBt = default;
    _ = data;
    _ = pBt;
    pBt = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt;
    data = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset;
    if (_decodeFlags(tls, pPage, libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data)))) != 0) {
        return _sqlite3CorruptError(tls, (int32)75411);
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaskPage = (uint16)((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize - libc.Uint32FromInt32(1));
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnOverflow = (uint8)0;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FcellOffset = libc.Uint16FromInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset) + libc.Int32FromInt32(8) + libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize));
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaCellIdx = data + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize + (uintptr)8;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataEnd = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaDataOfst = (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData + (uintptr)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FchildPtrSize;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell = libc.Uint16FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + 3))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(data + 3 + 1))));
    if ((uint32)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnCell > ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize - (uint32)8) / (uint32)6) {
        return _sqlite3CorruptError(tls, (int32)75425);
    }
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FnFree = -(int32)1;
    (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit = (uint8)1;
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_CellSizeCk) != 0) {
        return _btreeCellSizeCheck(tls, pPage);
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _btreeMoveto(ж<libc.TLS> tls, uintptr pCur, uintptr pKey, Ti64 nKey, int32 bias, @unsafe.Pointer pRes) {
    uintptr pIdxKey = default;
    uintptr pKeyInfo = default;
    int32 rc = default;
    _ = pIdxKey;
    _ = pKeyInfo;
    _ = rc;
    if (pKey != 0) {
        pKeyInfo = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo;
        pIdxKey = _sqlite3VdbeAllocUnpackedRecord(tls, pKeyInfo);
        if (pIdxKey == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
        _sqlite3VdbeRecordUnpack(tls, (int32)nKey, pKey, pIdxKey);
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FnField) == 0 || libc.Int32FromUint16((@unsafe.Pointer.To<TUnpackedRecord>((@unsafe.Pointer)pIdxKey)).Value.FnField) > libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)pKeyInfo)).Value.FnAllField)) {
            rc = _sqlite3CorruptError(tls, (int32)74033);
        } else {
            rc = _sqlite3BtreeIndexMoveto(tls, pCur, pIdxKey, pRes);
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKeyInfo)).Value.Fdb, pIdxKey);
    } else {
        pIdxKey = (uintptr)0;
        rc = _sqlite3BtreeTableMoveto(tls, pCur, nKey, bias, pRes);
    }
    return rc;
}

internal static uintptr /*r*/ _btreePageFromDbPage(ref libc.TLS tls, uintptr pDbPage, TPgno pgno, uintptr pBt) {
    uintptr pPage = default;
    int32 v1 = default;
    _ = pPage;
    _ = v1;
    pPage = _sqlite3PagerGetExtra(ref tls, pDbPage);
    if (pgno != (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fpgno) {
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FaData = _sqlite3PagerGetData(ref tls, pDbPage);
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpDbPage = pDbPage;
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt = pBt;
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.Fpgno = pgno;
        if (pgno == (uint32)1) {
            v1 = (int32)100;
        } else {
            v1 = 0;
        }
        (@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FhdrOffset = libc.Uint8FromInt32(v1);
    }
    return pPage;
}

internal static void _btreeParseCellPtrNoPayload(ж<libc.TLS> tls, uintptr pPage, uintptr pCell, uintptr pInfo) {
    _ = pPage;
    (@unsafe.Pointer.To<TCellInfo>((@unsafe.Pointer)pInfo)).Value.FnSize = libc.Uint16FromInt32((int32)4 + libc.Int32FromUint8(_sqlite3GetVarint(ref (tls).DerefOrNull(), pCell + 4, (@unsafe.Pointer)pInfo)));
    (@unsafe.Pointer.To<TCellInfo>((@unsafe.Pointer)pInfo)).Value.FnPayload = (uint32)0;
    (@unsafe.Pointer.To<TCellInfo>((@unsafe.Pointer)pInfo)).Value.FnLocal = (uint16)0;
    (@unsafe.Pointer.To<TCellInfo>((@unsafe.Pointer)pInfo)).Value.FpPayload = (uintptr)0;
    return;
}

internal static int32 /*r*/ _btreePayloadToLocal(ref libc.TLS tls, uintptr pPage, Ti64 nPayload) {
    int32 r = default;

    int32 maxLocal = default;
    int32 minLocal = default;
    int32 surplus = default;
    int32 v1 = default;
    _ = maxLocal;
    _ = minLocal;
    _ = surplus;
    _ = v1;
    maxLocal = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FmaxLocal);
    if (nPayload <= (int64)maxLocal) {
        return (int32)nPayload;
    } else {
        minLocal = libc.Int32FromUint16((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FminLocal);
        surplus = (int32)((int64)minLocal + rem((nPayload - (int64)minLocal), libc.Int64FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FpBt)).Value.FusableSize - libc.Uint32FromInt32(4))));
        if (surplus <= maxLocal) {
            v1 = surplus;
        } else {
            v1 = minLocal;
        }
        return v1;
    }
    return r;
}

internal static int32 /*r*/ _btreeRestoreCursorPosition(ж<libc.TLS> tls, uintptr pCur) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        ref var skipNext = ref heap(new int32(), out var ᏑskipNext);
        _ = rc;
        _ = skipNext;
        skipNext = 0;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == (int32)CURSOR_FAULT) {
            r = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext; goto ᒐdone;
        }
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_INVALID;
        if (_sqlite3FaultSim(tls, (int32)410) != 0) {
            rc = (int32)SQLITE_IOERR;
        } else {
            rc = _btreeMoveto(tls, pCur, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FnKey, 0, @unsafe.Pointer.FromPinnedBox(ᏑskipNext));
        }
        if (rc == SQLITE_OK) {
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey);
            (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey = (uintptr)0;
            if (skipNext != 0) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext = skipNext;
            }
            if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == CURSOR_VALID) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_SKIPNEXT;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _bytelengthFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    Ti64 m = default;
    int32 v1 = default;
    _ = m;
    _ = v1;
    _ = argc;
    var exprᴛ1 = Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)SQLITE_BLOB) { matchᴛ1 = true;
        Xsqlite3_result_int(tls, context, Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
    }
    else if (exprᴛ1 == (int32)SQLITE_INTEGER) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)SQLITE_FLOAT) { matchᴛ1 = true;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)Xsqlite3_context_db_handle(tls, context))).Value.Fenc) <= (int32)SQLITE_UTF8) {
            v1 = (int32)1;
        } else {
            v1 = (int32)2;
        }
        m = (int64)v1;
        Xsqlite3_result_int64(tls, context, (int64)Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))) * m);
    }
    else if (exprᴛ1 == (int32)SQLITE_TEXT) {
        if (Xsqlite3_value_encoding(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))) <= (int32)SQLITE_UTF8) {
            Xsqlite3_result_int(tls, context, Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
        } else {
            Xsqlite3_result_int(tls, context, Xsqlite3_value_bytes16(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv))));
        }
    }
    else if (!matchᴛ1) { /* default: */
        do {
            Xsqlite3_result_null(tls, context);
            break;
        } while (false);
    }

}

internal static int32 /*r*/ _cellContains(ref libc.TLS tls, uintptr pRtree, @unsafe.Pointer p1, @unsafe.Pointer p2) {
    @unsafe.Pointer a1 = default;
    @unsafe.Pointer a11 = default;
    @unsafe.Pointer a2 = default;
    @unsafe.Pointer a21 = default;
    int32 ii = default;
    _ = a1;
    _ = a11;
    _ = a2;
    _ = a21;
    _ = ii;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == (int32)RTREE_COORD_INT32) {
        ii = 0;
        for (;;) {
            if (!(ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2))) {
                break;
            }
            a1 = @unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4);
            a2 = @unsafe.Add(@unsafe.Add(p2, 8), (uintptr)ii * 4);
            if (~@unsafe.Pointer.To<int32>(a2) < ~@unsafe.Pointer.To<int32>(a1) || ~@unsafe.Pointer.To<int32>(@unsafe.Add(a2, 4)) > ~@unsafe.Pointer.To<int32>(@unsafe.Add(a1, 4))) {
                return 0;
            }
            goto _1;
_1:;
            ii = ii + (int32)2;
        }
    } else {
        ii = 0;
        for (;;) {
            if (!(ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2))) {
                break;
            }
            a11 = @unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4);
            a21 = @unsafe.Add(@unsafe.Add(p2, 8), (uintptr)ii * 4);
            if (~@unsafe.Pointer.To<TRtreeValue>(a21) < ~@unsafe.Pointer.To<TRtreeValue>(a11) || ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(a21, 4)) > ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(a11, 4))) {
                return 0;
            }
            goto _2;
_2:;
            ii = ii + (int32)2;
        }
    }
    return (int32)1;
}

internal static TRtreeDValue /*r*/ _cellMargin(ref libc.TLS tls, uintptr pRtree, @unsafe.Pointer p) {
    int32 ii = default;
    TRtreeDValue margin = default;
    float64 v1 = default;
    float64 v2 = default;
    _ = ii;
    _ = margin;
    _ = v1;
    _ = v2;
    margin = libc.Float64FromInt32(0);
    ii = libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2) - (int32)2;
    for (var cond = true; cond; cond = ii >= 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
            v1 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)(ii + (int32)1) * 4)));
        } else {
            v1 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)(ii + (int32)1) * 4)));
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
            v2 = (float64)(~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)ii * 4)));
        } else {
            v2 = (float64)(~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p, 8), (uintptr)ii * 4)));
        }
        margin = margin + (v1 - v2);
        ii = ii - (int32)2;
    }
    return margin;
}

internal static void _cellUnion(ref libc.TLS tls, uintptr pRtree, @unsafe.Pointer p1, @unsafe.Pointer p2) {
    int32 ii = default;
    int32 v3 = default;
    TRtreeValue v1 = default;
    _ = ii;
    _ = v1;
    _ = v3;
    ii = 0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FeCoordType) == RTREE_COORD_REAL32) {
        for (var cond = true; cond; cond = ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2)) {
            if (~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4)) < ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)ii * 4))) {
                v1 = ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4));
            } else {
                v1 = ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)ii * 4));
            }
            (@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4))).Value = v1;
            if (~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)(ii + (int32)1) * 4)) > ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)(ii + (int32)1) * 4))) {
                v1 = ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)(ii + (int32)1) * 4));
            } else {
                v1 = ~@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)(ii + (int32)1) * 4));
            }
            (@unsafe.Pointer.To<TRtreeValue>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)(ii + (int32)1) * 4))).Value = v1;
            ii = ii + (int32)2;
        }
    } else {
        for (var cond = true; cond; cond = ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2)) {
            if (~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4)) < ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)ii * 4))) {
                v3 = ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4));
            } else {
                v3 = ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)ii * 4));
            }
            (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)ii * 4))).Value = v3;
            if (~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)(ii + (int32)1) * 4)) > ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)(ii + (int32)1) * 4))) {
                v3 = ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)(ii + (int32)1) * 4));
            } else {
                v3 = ~@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p2, 8), (uintptr)(ii + (int32)1) * 4));
            }
            (@unsafe.Pointer.To<int32>(@unsafe.Add(@unsafe.Add(p1, 8), (uintptr)(ii + (int32)1) * 4))).Value = v3;
            ii = ii + (int32)2;
        }
    }
}

internal static int32 /*r*/ _changeTempStorage(ж<libc.TLS> tls, uintptr pParse, uintptr zStorageType) {
    uintptr db = default;
    int32 ts = default;
    _ = db;
    _ = ts;
    ts = _getTempStore(ref (tls).DerefOrNull(), zStorageType);
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Ftemp_store) == ts) {
        return SQLITE_OK;
    }
    if (_invalidateTempStorage(tls, pParse) != SQLITE_OK) {
        return (int32)SQLITE_ERROR;
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Ftemp_store = libc.Uint8FromInt32(ts);
    return SQLITE_OK;
}

internal static void _closePendingFds(ж<libc.TLS> tls, uintptr pFile) {
    uintptr p = default;
    uintptr pInode = default;
    uintptr pNext = default;
    _ = p;
    _ = pInode;
    _ = pNext;
    pInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode;
    p = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpUnused;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        pNext = (@unsafe.Pointer.To<TUnixUnusedFd>((@unsafe.Pointer)p)).Value.FpNext;
        _robust_close(tls, pFile, (@unsafe.Pointer.To<TUnixUnusedFd>((@unsafe.Pointer)p)).Value.Ffd, (int32)42251);
        Xsqlite3_free(tls, p);
        goto _1;
_1:;
        p = pNext;
    }
    (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpUnused = (uintptr)0;
}

internal static int32 /*r*/ _codeCompare(ж<libc.TLS> tls, uintptr pParse, uintptr pLeft, uintptr pRight, int32 opcode, int32 in1, int32 in2, int32 dest, int32 jumpIfNull, int32 isCommuted) {
    int32 addr = default;
    int32 p5 = default;
    uintptr p4 = default;
    _ = addr;
    _ = p4;
    _ = p5;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr != 0) {
        return 0;
    }
    if (isCommuted != 0) {
        p4 = _sqlite3BinaryCompareCollSeq(tls, pParse, pRight, pLeft);
    } else {
        p4 = _sqlite3BinaryCompareCollSeq(tls, pParse, pLeft, pRight);
    }
    p5 = libc.Int32FromUint8(_binaryCompareP5(tls, pLeft, pRight, jumpIfNull));
    addr = _sqlite3VdbeAddOp4(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, opcode, in2, dest, in1, p4, -(int32)2);
    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, libc.Uint16FromInt32(p5));
    return addr;
}

internal static int32 /*r*/ _codeEqualityTerm(ж<libc.TLS> tls, uintptr pParse, uintptr pTerm, uintptr pLevel, int32 iEq, int32 bRev, int32 iTarget) {
    int32 iReg = default;
    uintptr pX = default;
    _ = iReg;
    _ = pX;
    pX = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FpExpr;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_EQ || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_IS) {
        iReg = _sqlite3ExprCodeTarget(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.FpRight, iTarget);
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_ISNULL) {
            iReg = iTarget;
            _sqlite3VdbeAddOp2(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, (int32)OP_Null, 0, iReg);
        } else {
            iReg = iTarget;
            _codeINTerm(tls, pParse, pTerm, pLevel, iEq, bRev, iTarget);
        }
    }
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop)).Value.FwsFlags & (uint32)WHERE_TRANSCONS) == (uint32)0 || (libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeOperator) & (int32)WO_EQUIV) == 0) {
        _disableTerm(ref (tls).DerefOrNull(), pLevel, pTerm);
    }
    return iReg;
}

internal static Tu16 /*r*/ _computeNumericType(ж<libc.TLS> tls, uintptr pMem) {
    ref var ix = ref heap(new Tsqlite3_int64(), out var Ꮡix);
    int32 rc = default;
    int32 v1 = default;
    _ = ix;
    _ = rc;
    _ = v1;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Zero) != 0) {
        v1 = _sqlite3VdbeMemExpandBlob(tls, pMem);
    } else {
        v1 = 0;
    }
    if (v1 != 0) {
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = 0;
        return (uint16)MEM_Int;
    }
    rc = _sqlite3MemRealValueRC(tls, (@unsafe.Pointer)pMem, (@unsafe.Pointer)pMem);
    if (rc <= 0) {
        if ((rc & (int32)2) == 0 && _sqlite3Atoi64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, @unsafe.Pointer.FromPinnedBox(Ꮡix), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc) <= (int32)1) {
            (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = ix;
            return (uint16)MEM_Int;
        } else {
            return (uint16)MEM_Real;
        }
    } else {
        if ((rc & (int32)2) == 0 && _sqlite3Atoi64(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz, @unsafe.Pointer.FromPinnedBox(Ꮡix), (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc) == 0) {
            (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = ix;
            return (uint16)MEM_Int;
        }
    }
    return (uint16)MEM_Real;
}

internal static int32 /*r*/ _dbpageBestIndex(ж<libc.TLS> tls, uintptr tab, uintptr pIdxInfo) {
    int32 i = default;
    int32 iPlan = default;
    int32 v3 = default;
    uintptr p = default;
    uintptr p1 = default;
    _ = i;
    _ = iPlan;
    _ = p;
    _ = p1;
    _ = v3;
    iPlan = 0;
    _ = tab;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
            break;
        }
        p = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)i * 12;
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn != (int32)DBPAGE_COLUMN_SCHEMA) {
            goto _1;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) != (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
            goto _1;
        }
        if (!((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fusable != 0)) {
            return (int32)SQLITE_CONSTRAINT;
        }
        iPlan = (int32)2;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.FargvIndex = (int32)1;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.Fomit = (uint8)1;
        break;
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)1e+06D;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
            break;
        }
        p1 = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)i * 12;
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p1)).Value.Fusable != 0 && (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p1)).Value.FiColumn <= 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p1)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)1;
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxFlags = (int32)SQLITE_INDEX_SCAN_UNIQUE;
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)1D;
            if (iPlan != 0) {
                v3 = (int32)2;
            } else {
                v3 = (int32)1;
            }
            (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.FargvIndex = v3;
            (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.Fomit = (uint8)1;
            iPlan = iPlan | (int32)1;
            break;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = iPlan;
    if ((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnOrderBy >= (int32)1 && (@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaOrderBy))).Value.FiColumn <= 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaOrderBy))).Value.Fdesc) == 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.ForderByConsumed = (int32)1;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _dbpageRowid(ж<libc.TLS> tls, uintptr pCursor, uintptr pRowid) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = pCursor;
    (@unsafe.Pointer.To<Tsqlite_int64>(__ccgo_ptr(pRowid))).Value = libc.Int64FromUint32((@unsafe.Pointer.To<TDbpageCursor>((@unsafe.Pointer)pCsr)).Value.Fpgno);
    return SQLITE_OK;
}

internal static int32 /*r*/ _deleteCell(ж<libc.TLS> tls, uintptr pRtree, uintptr pNode, int32 iCell, int32 iHeight) {
    uintptr pParent = default;
    int32 rc = default;
    int32 v1 = default;
    _ = pParent;
    _ = rc;
    _ = v1;
    v1 = _fixLeafParent(tls, pRtree, pNode);
    rc = v1;
    if (SQLITE_OK != v1) {
        return rc;
    }
    _nodeDeleteCell(tls, pRtree, pNode, iCell);
    pParent = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FpParent;
    if (pParent != 0) {
        if (_readInt16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2) < quo(((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize - (int32)4), libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell)) / (int32)3) {
            rc = _removeNode(tls, pRtree, pNode, iHeight);
        } else {
            rc = _fixBoundingBox(tls, pRtree, pNode);
        }
    }
    return rc;
}

internal static void _destroyTable(ж<libc.TLS> tls, uintptr pParse, uintptr pTab) {
    int32 iDb = default;
    TPgno iDestroyed = default;
    TPgno iIdx = default;
    TPgno iLargest = default;
    TPgno iTab = default;
    uintptr pIdx = default;
    _ = iDb;
    _ = iDestroyed;
    _ = iIdx;
    _ = iLargest;
    _ = iTab;
    _ = pIdx;
    iTab = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum;
    iDestroyed = (uint32)0;
    while ((int32)1 != 0) {
        iLargest = (uint32)0;
        if (iDestroyed == (uint32)0 || iTab < iDestroyed) {
            iLargest = iTab;
        }
        pIdx = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
        for (;;) {
            if (!(pIdx != 0)) {
                break;
            }
            iIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum;
            if ((iDestroyed == (uint32)0 || iIdx < iDestroyed) && iIdx > iLargest) {
                iLargest = iIdx;
            }
            goto _1;
_1:;
            pIdx = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpNext;
        }
        if (iLargest == (uint32)0) {
            return;
        } else {
            iDb = _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpSchema);
            _destroyRootPage(tls, pParse, libc.Int32FromUint32(iLargest), iDb);
            iDestroyed = iLargest;
        }
    }
}

internal static int32 /*r*/ _dotlockCheckReservedLock(ж<libc.TLS> tls, uintptr id, uintptr pResOut) {
    uintptr pFile = default;
    _ = pFile;
    pFile = id;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock) >= (int32)SHARED_LOCK) {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pResOut))).Value = 0;
    } else {
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pResOut))).Value = libc.BoolInt32(GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>(_aSyscall[(int32)2].FpCurrent)(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlockingContext, 0) == 0);
    }
    return SQLITE_OK;
}

internal static readonly ж<uintptr> Ꮡ_dotlockIoFinder = new StaticBox<uintptr>((uintptr)0);
internal static ref uintptr _dotlockIoFinder => ref ((StaticBox<uintptr>)Ꮡ_dotlockIoFinder).ValueSlot;

internal static uintptr /*r*/ _dotlockIoFinderImpl(ж<libc.TLS> tls, uintptr z, uintptr p) {
    _ = z;
    _ = p;
    return (uintptr)Ꮡ_dotlockIoMethods;
}

internal static readonly ж<Tsqlite3_io_methods> Ꮡ_dotlockIoMethods = new StaticBox<Tsqlite3_io_methods>(new Tsqlite3_io_methods(
    FiVersion: (int32)1
));
internal static ref Tsqlite3_io_methods _dotlockIoMethods => ref ((StaticBox<Tsqlite3_io_methods>)Ꮡ_dotlockIoMethods).ValueSlot;

internal static TLogEst /*r*/ _estLog(ref libc.TLS tls, TLogEst N) {
    int32 v1 = default;
    _ = v1;
    if ((int32)N <= (int32)10) {
        v1 = 0;
    } else {
        v1 = (int32)_sqlite3LogEst(ref tls, libc.Uint64FromInt16(N)) - (int32)33;
    }
    return (int16)v1;
}

internal static int32 /*r*/ _exprCodeSubselect(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    int32 reg = default;
    _ = reg;
    reg = 0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_SELECT) {
        reg = _sqlite3CodeSubselect(tls, pParse, pExpr);
    }
    return reg;
}

internal static int32 /*r*/ _exprCodeTargetAndOr(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 target, @unsafe.Pointer pTmpReg) {
    int32 addrSkip = default;
    int32 op = default;
    int32 r1 = default;
    int32 r2 = default;
    int32 regSS = default;
    int32 skipOp = default;
    int32 v1 = default;
    uintptr pAlt = default;
    uintptr v = default;
    _ = addrSkip;
    _ = op;
    _ = pAlt;
    _ = r1;
    _ = r2;
    _ = regSS;
    _ = skipOp;
    _ = v;
    _ = v1;
    regSS = 0;
    op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop);
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    pAlt = _sqlite3ExprSimplifiedAndOr(ref (tls).DerefOrNull(), pExpr);
    if (pAlt != pExpr) {
        r1 = _sqlite3ExprCodeTarget(tls, pParse, pAlt, target);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_And, r1, r1, target);
        return target;
    }
    if (op == (int32)TK_AND) {
        v1 = (int32)OP_IfNot;
    } else {
        v1 = (int32)OP_If;
    }
    skipOp = v1;
    if (_exprEvalRhsFirst(ref (tls).DerefOrNull(), pExpr) != 0) {
        v1 = _sqlite3ExprCodeTarget(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, target);
        regSS = v1;
        r2 = v1;
        addrSkip = _sqlite3VdbeAddOp1(tls, v, skipOp, r2);
        r1 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, pTmpReg);
    } else {
        r1 = _sqlite3ExprCodeTarget(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, target);
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0) {
            regSS = r1;
            addrSkip = _sqlite3VdbeAddOp1(tls, v, skipOp, r1);
        } else {
            v1 = libc.Int32FromInt32(0);
            regSS = v1;
            addrSkip = v1;
        }
        r2 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, pTmpReg);
    }
    _sqlite3VdbeAddOp3(tls, v, op, r2, r1, target);
    if (addrSkip != 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_Goto, 0, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2);
        _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrSkip);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Or, regSS, regSS, target);
    }
    return target;
}

internal static Tu16 /*r*/ _exprCommute(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    uintptr t = default;
    _ = t;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft)).Value.Fop) == (int32)TK_VECTOR || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight)).Value.Fop) == (int32)TK_VECTOR || _sqlite3BinaryCompareCollSeq(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight) != _sqlite3BinaryCompareCollSeq(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft)) {
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pExpr + 4))).Value ^= (uint32)EP_Commuted;
    }
    t = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight;
    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft = t;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) >= (int32)TK_GT) {
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop = libc.Uint8FromInt32((libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) - (int32)TK_GT ^ (int32)2) + (int32)TK_GT);
    }
    return (uint16)0;
}

internal static int32 /*r*/ _exprCompareVariable(ж<libc.TLS> tls, uintptr pParse, uintptr pVar, uintptr pExpr) {
    int32 iVar = default;
    int32 res = default;
    int32 v1 = default;
    uintptr pL = default;
    ref var pR = ref heap(new uintptr(), out var ᏑpR);
    _ = iVar;
    _ = pL;
    _ = pR;
    _ = res;
    _ = v1;
    res = (int32)2;
    pR = (uintptr)0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_VARIABLE && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pVar)).Value.FiColumn == (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn) {
        return 0;
    }
    if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_EnableQPSG) != (uint64)0) {
        return (int32)2;
    }
    _sqlite3ValueFromExpr(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr, (uint8)SQLITE_UTF8, (uint8)SQLITE_AFF_BLOB, @unsafe.Pointer.FromBox(ᏑpR));
    if (pR != 0) {
        iVar = (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pVar)).Value.FiColumn;
        _sqlite3VdbeReprepareOnBind(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, iVar, 0);
        pL = _sqlite3VdbeGetBoundValue(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpReprepare, iVar, (uint8)SQLITE_AFF_BLOB);
        if (pL != 0) {
            if (Xsqlite3_value_type(tls, pL) == (int32)SQLITE_TEXT) {
                Xsqlite3_value_text(tls, pL);
            }
            if (_sqlite3MemCompare(tls, pL, pR, (uintptr)0) != 0) {
                v1 = (int32)2;
            } else {
                v1 = 0;
            }
            res = v1;
        }
        _sqlite3ValueFree(tls, pR);
        _sqlite3ValueFree(tls, pL);
    }
    return res;
}

internal static int32 /*r*/ _exprComputeOperands(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, @unsafe.Pointer pR1, @unsafe.Pointer pR2, @unsafe.Pointer pFree1, @unsafe.Pointer pFree2) {
    int32 addrIsNull = default;
    int32 r1 = default;
    int32 r2 = default;
    uintptr v = default;
    _ = addrIsNull;
    _ = r1;
    _ = r2;
    _ = v;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (_exprEvalRhsFirst(ref (tls).DerefOrNull(), pExpr) != 0 && _sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight) != 0) {
        r2 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, pFree2);
        addrIsNull = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IsNull, r2);
    } else {
        r2 = 0;
        addrIsNull = 0;
    }
    r1 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, pFree1);
    if (addrIsNull == 0) {
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0 && _sqlite3ExprCanBeNull(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft) != 0) {
            addrIsNull = _sqlite3VdbeAddOp1(tls, v, (int32)OP_IsNull, r1);
        }
        r2 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, pFree2);
    }
    (@unsafe.Pointer.To<int32>(pR1)).Value = r1;
    (@unsafe.Pointer.To<int32>(pR2)).Value = r2;
    return addrIsNull;
}

internal static int32 /*r*/ _exprEvalRhsFirst(ref libc.TLS tls, uintptr pExpr) {
    int32 r = default;

    if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0 && !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != libc.Uint32FromInt32(0))) {
        return (int32)1;
    } else {
        return 0;
    }
    return r;
}

internal static int32 /*r*/ _exprNodeIsDeterministic(ж<libc.TLS> tls, uintptr pWalker, uintptr pExpr) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_FUNCTION && libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_ConstFunc))) != (uint32)0) == 0) {
        (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FeCode = (uint16)0;
        return (int32)WRC_Abort;
    }
    return WRC_Continue;
}

internal static TPgno /*r*/ _finalDbSize(ref libc.TLS tls, uintptr pBt, TPgno nOrig, TPgno nFree) {
    int32 nEntry = default;
    TPgno nFin = default;
    TPgno nPtrmap = default;
    _ = nEntry;
    _ = nFin;
    _ = nPtrmap;
    nEntry = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize / (uint32)5);
    nPtrmap = (nFree - nOrig + _ptrmapPageno(ref tls, pBt, nOrig) + libc.Uint32FromInt32(nEntry)) / libc.Uint32FromInt32(nEntry);
    nFin = nOrig - nFree - nPtrmap;
    if (nOrig > libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1) && nFin < libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
        nFin = nFin - 1;
    }
    while (_ptrmapPageno(ref tls, pBt, nFin) == nFin || nFin == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
        nFin = nFin - 1;
    }
    return nFin;
}

internal static void _fixDistinctOpenEph(ж<libc.TLS> tls, uintptr pParse, int32 eTnctType, int32 iVal, int32 iOpenEphAddr) {
    uintptr pOp = default;
    uintptr v = default;
    _ = pOp;
    _ = v;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0 && (eTnctType == (int32)WHERE_DISTINCT_UNIQUE || eTnctType == (int32)WHERE_DISTINCT_ORDERED)) {
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        _sqlite3VdbeChangeToNoop(tls, v, iOpenEphAddr);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)_sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, iOpenEphAddr + (int32)1))).Value.Fopcode) == (int32)OP_Explain) {
            _sqlite3VdbeChangeToNoop(tls, v, iOpenEphAddr + (int32)1);
        }
        if (eTnctType == (int32)WHERE_DISTINCT_ORDERED) {
            pOp = _sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, iOpenEphAddr);
            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode = (uint8)OP_Null;
            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp1 = (int32)1;
            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2 = iVal;
        }
    }
}

internal static void _fkLookupParent(ж<libc.TLS> tls, uintptr pParse, int32 iDb, uintptr pTab, uintptr pIdx, uintptr pFKey, @unsafe.Pointer aiCol, int32 regData, int32 nIncr, int32 isIgnore) {
    int32 i = default;
    int32 iChild = default;
    int32 iCur = default;
    int32 iJump = default;
    int32 iMustBeInt = default;
    int32 iOk = default;
    int32 iParent = default;
    int32 iReg = default;
    int32 nCol = default;
    int32 regTemp = default;
    int32 regTemp1 = default;
    uintptr v = default;
    _ = i;
    _ = iChild;
    _ = iCur;
    _ = iJump;
    _ = iMustBeInt;
    _ = iOk;
    _ = iParent;
    _ = iReg;
    _ = nCol;
    _ = regTemp;
    _ = regTemp1;
    _ = v;
    v = _sqlite3GetVdbe(tls, pParse);
    iCur = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnTab - (int32)1;
    iOk = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
    if (nIncr < 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkIfZero, libc.Int32FromUint8((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred), iOk);
    }
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol)) {
            break;
        }
        iReg = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom, (int16)(~@unsafe.Pointer.To<int32>(@unsafe.Add(aiCol, (uintptr)i * 4)))) + regData + (int32)1;
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_IsNull, iReg, iOk);
        goto _1;
_1:;
        i = i + 1;
    }
    if (isIgnore == 0) {
        if (pIdx == (uintptr)0) {
            regTemp = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
            _sqlite3VdbeAddOp2(tls, v, (int32)OP_SCopy, (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom, (int16)(~@unsafe.Pointer.To<int32>(aiCol))) + (int32)1 + regData, regTemp);
            iMustBeInt = _sqlite3VdbeAddOp2(tls, v, (int32)OP_MustBeInt, regTemp, 0);
            if (pTab == (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom && nIncr == (int32)1) {
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_Eq, regData, iOk, regTemp);
                _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_NOTNULL);
            }
            _sqlite3OpenTable(tls, pParse, iCur, iDb, pTab, (int32)OP_OpenRead);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_NotExists, iCur, 0, regTemp);
            _sqlite3VdbeGoto(tls, v, iOk);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) - (int32)2);
            _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, iMustBeInt);
            _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regTemp);
        } else {
            nCol = (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FnCol;
            regTemp1 = _sqlite3GetTempRange(ref (tls).DerefOrNull(), pParse, nCol);
            _sqlite3VdbeAddOp3(tls, v, (int32)OP_OpenRead, iCur, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.Ftnum), iDb);
            _sqlite3VdbeSetP4KeyInfo(tls, pParse, pIdx);
            i = 0;
            for (;;) {
                if (!(i < nCol)) {
                    break;
                }
                _sqlite3VdbeAddOp2(tls, v, (int32)OP_Copy, (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom, (int16)(~@unsafe.Pointer.To<int32>(@unsafe.Add(aiCol, (uintptr)i * 4)))) + (int32)1 + regData, regTemp1 + i);
                goto _2;
_2:;
                i = i + 1;
            }
            if (pTab == (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom && nIncr == (int32)1) {
                iJump = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + nCol + (int32)1;
                i = 0;
                for (;;) {
                    if (!(i < nCol)) {
                        break;
                    }
                    iChild = (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FpFrom, (int16)(~@unsafe.Pointer.To<int32>(@unsafe.Add(aiCol, (uintptr)i * 4)))) + (int32)1 + regData;
                    iParent = (int32)1 + regData;
                    iParent = iParent + (int32)_sqlite3TableColumnToStorage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FpTable, ~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)));
                    if ((int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2))) == (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FiPKey) {
                        iParent = regData;
                    }
                    _sqlite3VdbeAddOp3(tls, v, (int32)OP_Ne, iChild, iJump, iParent);
                    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_JUMPIFNULL);
                    goto _3;
_3:;
                    i = i + 1;
                }
                _sqlite3VdbeGoto(tls, v, iOk);
            }
            _sqlite3VdbeAddOp4(tls, v, (int32)OP_Affinity, regTemp1, nCol, 0, _sqlite3IndexAffinityStr(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pIdx), nCol);
            _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_Found, iCur, iOk, regTemp1, nCol);
            _sqlite3ReleaseTempRange(ref (tls).DerefOrNull(), pParse, regTemp1, nCol);
        }
    }
    if (!((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred != 0) && !((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Fflags & libc.Uint64FromInt32(SQLITE_DeferFKs)) != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) && !((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FisMultiWrite != 0)) {
        _sqlite3HaltConstraint(tls, pParse, libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8))), (int32)OE_Abort, (uintptr)0, (int8)(-libc.Int32FromInt32(1)), (uint8)P5_ConstraintFK);
    } else {
        if (nIncr > 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred) == 0) {
            _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
        }
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_FkCounter, libc.Int32FromUint8((@unsafe.Pointer.To<TFKey>((@unsafe.Pointer)pFKey)).Value.FisDeferred), nIncr);
    }
    _sqlite3VdbeResolveLabel(tls, v, iOk);
    _sqlite3VdbeAddOp1(tls, v, (int32)OP_Close, iCur);
}

internal static int32 /*r*/ _fts5ApiColumnLocale(ж<libc.TLS> tls, uintptr pCtx, int32 iCol, uintptr pzLocale, uintptr pnLocale) {
    ref var nDummy = ref heap(new int32(), out var ᏑnDummy);
    int32 rc = default;
    uintptr pConfig = default;
    uintptr pCsr = default;
    ref var zDummy = ref heap(new uintptr(), out var ᏑzDummy);
    _ = nDummy;
    _ = pConfig;
    _ = pCsr;
    _ = rc;
    _ = zDummy;
    rc = SQLITE_OK;
    pCsr = pCtx;
    pConfig = (@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab)).Value.FpConfig;
    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzLocale))).Value = (uintptr)0;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnLocale))).Value = 0;
    if (iCol < 0 || iCol >= (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCol) {
        rc = (int32)SQLITE_RANGE;
    } else {
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FabUnindexed + (uintptr)iCol))) == 0 && 0 == _fts5IsContentless(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab, (int32)1) && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbLocale != 0) {
            rc = _fts5SeekCursor(tls, pCsr, 0);
            if (rc == SQLITE_OK) {
                zDummy = (uintptr)0;
                nDummy = 0;
                rc = _fts5TextFromStmt(tls, pConfig, (@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.FpStmt, iCol, @unsafe.Pointer.FromBox(ᏑzDummy), @unsafe.Pointer.FromPinnedBox(ᏑnDummy));
                if (rc == SQLITE_OK) {
                    (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzLocale))).Value = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FpLocale;
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pnLocale))).Value = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FnLocale;
                }
                _sqlite3Fts5ClearLocale(ref (tls).DerefOrNull(), pConfig);
            }
        }
    }
    return rc;
}

internal static void _fts5ApiPhraseNextColumn(ж<libc.TLS> tls, uintptr pCtx, uintptr pIter, uintptr piCol) {
    ref var dummy = ref heap(new int32(), out var Ꮡdummy);
    ref var iIncr = ref heap(new int32(), out var ᏑiIncr);
    uintptr pConfig = default;
    uintptr pCsr = default;
    _ = dummy;
    _ = iIncr;
    _ = pConfig;
    _ = pCsr;
    pCsr = pCtx;
    pConfig = (@unsafe.Pointer.To<TFts5Table>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Cursor>((@unsafe.Pointer)pCsr)).Value.Fbase.FpVtab)).Value.FpConfig;
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeDetail == (int32)FTS5_DETAIL_COLUMNS) {
        if ((@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fa >= (@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fb) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(piCol))).Value = -(int32)1;
        } else {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pIter))).Value += (uintptr)_sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fa, @unsafe.Pointer.FromPinnedBox(ᏑiIncr));
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(piCol))).Value += iIncr - (int32)2;
        }
    } else {
        while ((int32)1 != 0) {
            if ((@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fa >= (@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fb) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(piCol))).Value = -(int32)1;
                return;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fa))) == (int32)0x01) {
                break;
            }
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pIter))).Value += (uintptr)_sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fa, @unsafe.Pointer.FromPinnedBox(Ꮡdummy));
        }
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pIter))).Value += (uintptr)((int32)1 + _sqlite3Fts5GetVarint32(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5PhraseIter>((@unsafe.Pointer)pIter)).Value.Fa + 1, (@unsafe.Pointer)piCol));
    }
}

internal static Ti64 /*r*/ _fts5DlidxExtractFirstRowid(ref libc.TLS tls, @unsafe.Pointer pBuf) {
    int32 iOff = default;
    ref var iRowid = ref heap(new Ti64(), out var ᏑiRowid);
    _ = iOff;
    _ = iRowid;
    iOff = (int32)1 + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pBuf) + 1), @unsafe.Pointer.FromPinnedBox(ᏑiRowid)));
    _sqlite3Fts5GetVarint(ref tls, (@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pBuf) + (uintptr)iOff), @unsafe.Pointer.FromPinnedBox(ᏑiRowid));
    return iRowid;
}

internal static int32 /*r*/ _fts5ExprSynonymAdvanceto(ж<libc.TLS> tls, uintptr pTerm, int32 bDesc, @unsafe.Pointer piLast, @unsafe.Pointer pRc) {
    ref var bEof = ref heap(new int32(), out var ᏑbEof);
    int32 rc = default;
    Ti64 iLast = default;
    Ti64 iRowid = default;
    uintptr p = default;
    _ = bEof;
    _ = iLast;
    _ = iRowid;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    iLast = ~@unsafe.Pointer.To<Ti64>(piLast);
    bEof = 0;
    p = pTerm;
    for (;;) {
        if (!(rc == SQLITE_OK && p != 0)) {
            break;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpIter)).Value.FbEof) == 0) {
            iRowid = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpIter)).Value.FiRowid;
            if (bDesc == 0 && iLast > iRowid || bDesc != 0 && iLast < iRowid) {
                rc = _sqlite3Fts5IterNextFrom(tls, (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpIter, iLast);
            }
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpSynonym;
    }
    if (rc != SQLITE_OK) {
        (@unsafe.Pointer.To<int32>(pRc)).Value = rc;
        bEof = (int32)1;
    } else {
        (@unsafe.Pointer.To<Ti64>(piLast)).Value = _fts5ExprSynonymRowid(ref (tls).DerefOrNull(), pTerm, bDesc, @unsafe.Pointer.FromPinnedBox(ᏑbEof));
    }
    return bEof;
}

internal static Ti64 /*r*/ _fts5ExprSynonymRowid(ref libc.TLS tls, uintptr pTerm, int32 bDesc, @unsafe.Pointer pbEof) {
    int32 bRetValid = default;
    Ti64 iRet = default;
    Ti64 iRowid = default;
    uintptr p = default;
    _ = bRetValid;
    _ = iRet;
    _ = iRowid;
    _ = p;
    iRet = 0;
    bRetValid = 0;
    p = pTerm;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if (0 == libc.Int32FromUint8((@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpIter)).Value.FbEof)) {
            iRowid = (@unsafe.Pointer.To<TFts5IndexIter>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpIter)).Value.FiRowid;
            if (bRetValid == 0 || bDesc != libc.BoolInt32(iRowid < iRet)) {
                iRet = iRowid;
                bRetValid = (int32)1;
            }
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TFts5ExprTerm>((@unsafe.Pointer)p)).Value.FpSynonym;
    }
    if (pbEof != nil && bRetValid == 0) {
        (@unsafe.Pointer.To<int32>(pbEof)).Value = (int32)1;
    }
    return iRet;
}

internal static int32 /*r*/ _fts5GetPoslistSize(ref libc.TLS tls, uintptr p, @unsafe.Pointer pnSz, @unsafe.Pointer pbDel) {
    int32 n = default;
    ref var nSz = ref heap(new int32(), out var ᏑnSz);
    int32 v1 = default;
    _ = n;
    _ = nSz;
    _ = v1;
    n = 0;
    v1 = n;
    n = n + 1;
    nSz = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + (uintptr)v1)));
    if ((nSz & (int32)0x80) != 0) {
        n = n - 1;
        n = n + _sqlite3Fts5GetVarint32(ref tls, p + (uintptr)n, @unsafe.Pointer.FromPinnedBox(ᏑnSz));
    }
    (@unsafe.Pointer.To<int32>(pnSz)).Value = nSz / (int32)2;
    (@unsafe.Pointer.To<int32>(pbDel)).Value = nSz & (int32)0x0001;
    return n;
}

internal static Tu16 /*r*/ _fts5GetU16(ref libc.TLS tls, uintptr aIn) {
    return libc.Uint16FromInt32((libc.Int32FromUint16((uint16)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aIn)))) << (int)((int32)8)) + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aIn + 1))));
}

internal static uint32 /*r*/ _fts5HashKey(ref libc.TLS tls, int32 nSlot, uintptr p, int32 n) {
    uint32 h = default;
    int32 i = default;
    _ = h;
    _ = i;
    h = (uint32)13;
    i = n - (int32)1;
    for (;;) {
        if (!(i >= 0)) {
            break;
        }
        h = h << (int)((int32)3) ^ h ^ (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + (uintptr)i)));
        goto _1;
_1:;
        i = i - 1;
    }
    return h % libc.Uint32FromInt32(nSlot);
}

internal static uint32 /*r*/ _fts5HashKey2(ref libc.TLS tls, int32 nSlot, Tu8 b, uintptr p, int32 n) {
    uint32 h = default;
    int32 i = default;
    _ = h;
    _ = i;
    h = (uint32)13;
    i = n - (int32)1;
    for (;;) {
        if (!(i >= 0)) {
            break;
        }
        h = h << (int)((int32)3) ^ h ^ (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + (uintptr)i)));
        goto _1;
_1:;
        i = i - 1;
    }
    h = h << (int)((int32)3) ^ h ^ (uint32)b;
    return h % libc.Uint32FromInt32(nSlot);
}

internal static void _fts5IndexIntegrityCheckEmpty(ж<libc.TLS> tls, uintptr p, uintptr pSeg, int32 iFirst, int32 iNoRowid, int32 iLast) {
    int32 i = default;
    uintptr pLeaf = default;
    _ = i;
    _ = pLeaf;
    i = iFirst;
    for (;;) {
        if (!((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && i <= iLast)) {
            break;
        }
        pLeaf = _fts5LeafRead(tls, p, ((int64)(@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)i);
        if (pLeaf != 0) {
            if (!((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.FszLeaf >= (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fnn) || i >= iNoRowid && 0 != libc.Int32FromUint16(_fts5GetU16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pLeaf)).Value.Fp))) {
                _fts5IndexCorruptRowid(tls, p, ((int64)(@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)pSeg)).Value.FiSegid).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B) + libc.Int32FromInt32(FTS5_DATA_DLI_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B) + libc.Int32FromInt32(FTS5_DATA_HEIGHT_B))) + ((int64)libc.Int32FromInt32(0)).Lsh((int64)(libc.Int32FromInt32(FTS5_DATA_PAGE_B))) + (int64)i);
            }
        }
        _fts5DataRelease(tls, pLeaf);
        goto _1;
_1:;
        i = i + 1;
    }
}

internal static int32 /*r*/ _fts5IndexPrepareStmt(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer ppStmt, uintptr zSql) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if (zSql != 0) {
            rc = _sqlite3_prepare_v3__ccgo_gp(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.Fdb, zSql, -(int32)1, libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_PREPARE_PERSISTENT) | libc.Int32FromInt32(SQLITE_PREPARE_NO_VTAB)), ppStmt, (uintptr)0);
            if (rc == (int32)SQLITE_ERROR) {
                v1 = (int32)SQLITE_CORRUPT;
            } else {
                v1 = rc;
            }
            (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = v1;
        } else {
            (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_NOMEM;
        }
    }
    Xsqlite3_free(tls, zSql);
    return (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc;
}

internal static int32 /*r*/ _fts5IndexTombstoneAddToPage(ref libc.TLS tls, uintptr pPg, int32 bForce, int32 nPg, Tu64 iRowid) {
    uintptr aSlot = default;
    uintptr aSlot1 = default;
    int32 iSlot = default;
    int32 nCollide = default;
    int32 nElem = default;
    int32 nSlot = default;
    int32 szKey = default;
    int32 v1 = default;
    int32 v2 = default;
    int32 v3 = default;
    _ = aSlot;
    _ = aSlot1;
    _ = iSlot;
    _ = nCollide;
    _ = nElem;
    _ = nSlot;
    _ = szKey;
    _ = v1;
    _ = v2;
    _ = v3;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp))) == (int32)4) {
        v1 = (int32)4;
    } else {
        v1 = (int32)8;
    }
    szKey = v1;
    if ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fnn > (int32)16) {
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp))) == (int32)4) {
            v3 = (int32)4;
        } else {
            v3 = (int32)8;
        }
        v2 = quo(((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fnn - (int32)8), v3);
    } else {
        v2 = (int32)1;
    }
    nSlot = v2;
    nElem = libc.Int32FromUint32(_fts5GetU32(ref tls, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp + 4));
    iSlot = libc.Int32FromUint64(iRowid / libc.Uint64FromInt32(nPg) % libc.Uint64FromInt32(nSlot));
    nCollide = nSlot;
    if (szKey == (int32)4 && iRowid > (uint64)0xFFFFFFFFU) {
        return (int32)2;
    }
    if (iRowid == (uint64)0) {
        (@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp + 1))).Value = (uint8)0x01;
        return 0;
    }
    if (bForce == 0 && nElem >= nSlot / (int32)2) {
        return (int32)1;
    }
    _fts5PutU32(ref tls, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp + 4, libc.Uint32FromInt32(nElem + (int32)1));
    if (szKey == (int32)4) {
        aSlot = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp + 8;
        while (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aSlot + (uintptr)iSlot * 4)) != 0) {
            iSlot = rem((iSlot + (int32)1), nSlot);
            v1 = nCollide;
            nCollide = nCollide - 1;
            if (v1 == 0) {
                return 0;
            }
        }
        _fts5PutU32(ref tls, aSlot + (uintptr)iSlot * 4, (uint32)iRowid);
    } else {
        aSlot1 = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pPg)).Value.Fp + 8;
        while (~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(aSlot1 + (uintptr)iSlot * 8)) != 0) {
            iSlot = rem((iSlot + (int32)1), nSlot);
            v1 = nCollide;
            nCollide = nCollide - 1;
            if (v1 == 0) {
                return 0;
            }
        }
        _fts5PutU64(ref tls, aSlot1 + (uintptr)iSlot * 8, iRowid);
    }
    return 0;
}

internal static int32 /*r*/ _fts5IndexTombstoneQuery(ref libc.TLS tls, uintptr pHash, int32 nHashTable, Tu64 iRowid) {
    uintptr aSlot = default;
    uintptr aSlot1 = default;
    int32 iSlot = default;
    int32 nCollide = default;
    int32 nSlot = default;
    int32 szKey = default;
    int32 v1 = default;
    int32 v2 = default;
    int32 v3 = default;
    _ = aSlot;
    _ = aSlot1;
    _ = iSlot;
    _ = nCollide;
    _ = nSlot;
    _ = szKey;
    _ = v1;
    _ = v2;
    _ = v3;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fp))) == (int32)4) {
        v1 = (int32)4;
    } else {
        v1 = (int32)8;
    }
    szKey = v1;
    if ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fnn > (int32)16) {
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fp))) == (int32)4) {
            v3 = (int32)4;
        } else {
            v3 = (int32)8;
        }
        v2 = quo(((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fnn - (int32)8), v3);
    } else {
        v2 = (int32)1;
    }
    nSlot = v2;
    iSlot = libc.Int32FromUint64(iRowid / libc.Uint64FromInt32(nHashTable) % libc.Uint64FromInt32(nSlot));
    nCollide = nSlot;
    if (iRowid == (uint64)0) {
        return libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fp + 1)));
    } else {
        if (szKey == (int32)4) {
            aSlot = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fp + 8;
            while (~@unsafe.Pointer.To<Tu32>(__ccgo_ptr(aSlot + (uintptr)iSlot * 4)) != 0) {
                if ((uint64)_fts5GetU32(ref tls, aSlot + (uintptr)iSlot * 4) == iRowid) {
                    return (int32)1;
                }
                v1 = nCollide;
                nCollide = nCollide - 1;
                if (v1 == 0) {
                    break;
                }
                iSlot = rem((iSlot + (int32)1), nSlot);
            }
        } else {
            aSlot1 = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pHash)).Value.Fp + 8;
            while (~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(aSlot1 + (uintptr)iSlot * 8)) != 0) {
                if (_fts5GetU64(ref tls, aSlot1 + (uintptr)iSlot * 8) == iRowid) {
                    return (int32)1;
                }
                v1 = nCollide;
                nCollide = nCollide - 1;
                if (v1 == 0) {
                    break;
                }
                iSlot = rem((iSlot + (int32)1), nSlot);
            }
        }
    }
    return 0;
}

internal static uintptr /*r*/ _fts5LeafRead(ж<libc.TLS> tls, uintptr p, Ti64 iRowid) {
    uintptr pRet = default;
    _ = pRet;
    pRet = _fts5DataRead(tls, p, iRowid);
    if (pRet != 0) {
        if ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pRet)).Value.Fnn >= (int32)4) {
            (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pRet)).Value.FszLeaf = libc.Int32FromUint16(_fts5GetU16(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pRet)).Value.Fp + 2));
        }
        if ((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pRet)).Value.FszLeaf < (int32)4 || (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pRet)).Value.FszLeaf > (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)pRet)).Value.Fnn) {
            _fts5IndexCorruptRowid(tls, p, iRowid);
            _fts5DataRelease(tls, pRet);
            pRet = (uintptr)0;
        }
    }
    return pRet;
}

internal static void _fts5NextRowid(ref libc.TLS tls, uintptr pBuf, @unsafe.Pointer piOff, @unsafe.Pointer piRowid) {
    int32 i = default;
    ref var iVal = ref heap(new Tu64(), out var ᏑiVal);
    @unsafe.Pointer v1 = default;
    _ = i;
    _ = iVal;
    _ = v1;
    i = ~@unsafe.Pointer.To<int32>(piOff);
    if (i >= (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn) {
        (@unsafe.Pointer.To<int32>(piOff)).Value = -(int32)1;
    } else {
        (@unsafe.Pointer.To<int32>(piOff)).Value = i + libc.Int32FromUint8(_sqlite3Fts5GetVarint(ref tls, (@unsafe.Pointer)((@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fp + (uintptr)i), @unsafe.Pointer.FromPinnedBox(ᏑiVal)));
        v1 = piRowid;
        (@unsafe.Pointer.To<Ti64>(v1)).Value = (Ti64)((uint64)(~@unsafe.Pointer.To<Ti64>(v1)) + iVal);
    }
}

internal static uintptr /*r*/ _fts5ParseAlloc(ж<libc.TLS> tls, Tu64 t) {
    return Xsqlite3_malloc64(tls, libc.Uint64FromInt64(libc.Int64FromUint64(t)));
}

internal static int32 /*r*/ _fts5PrefixCompress(ref libc.TLS tls, int32 nOld, uintptr pOld, uintptr pNew) {
    int32 i = default;
    _ = i;
    i = 0;
    for (;;) {
        if (!(i < nOld)) {
            break;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pOld + (uintptr)i))) != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pNew + (uintptr)i)))) {
            break;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return i;
}

internal static void _fts5PutU16(ref libc.TLS tls, uintptr aOut, Tu16 iVal) {
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aOut))).Value = libc.Uint8FromInt32(libc.Int32FromUint16(iVal).Rsh((int64)(libc.Int32FromInt32(8))));
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aOut + 1))).Value = libc.Uint8FromInt32(libc.Int32FromUint16(iVal) & libc.Int32FromInt32(0xFF));
}

internal static void _fts5SegIterLoadNPos(ref libc.TLS tls, uintptr p, uintptr pIter) {
    int32 iEod = default;
    int32 iOff = default;
    ref var nSz = ref heap(new int32(), out var ᏑnSz);
    int32 v1 = default;
    _ = iEod;
    _ = iOff;
    _ = nSz;
    _ = v1;
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        iOff = (int32)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafOffset;
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig)).Value.FeDetail == (int32)FTS5_DETAIL_NONE) {
            if ((@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiEndofDoclist < (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.FszLeaf) {
                v1 = (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiEndofDoclist;
            } else {
                v1 = (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.FszLeaf;
            }
            iEod = v1;
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FbDel = (uint8)0;
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FnPos = (int32)1;
            if (iOff < iEod && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fp + (uintptr)iOff))) == 0) {
                (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FbDel = (uint8)1;
                iOff = iOff + 1;
                if (iOff < iEod && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fp + (uintptr)iOff))) == 0) {
                    (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FnPos = (int32)1;
                    iOff = iOff + 1;
                } else {
                    (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FnPos = 0;
                }
            }
        } else {
            v1 = iOff;
            iOff = iOff + 1;
            nSz = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fp + (uintptr)v1)));
            if ((nSz & (int32)0x80) != 0) {
                iOff = iOff - 1;
                iOff = iOff + _sqlite3Fts5GetVarint32(ref tls, (@unsafe.Pointer.To<TFts5Data>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpLeaf)).Value.Fp + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(ᏑnSz));
            }
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FbDel = libc.Uint8FromInt32(nSz & libc.Int32FromInt32(0x0001));
            (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FnPos = nSz >> (int)((int32)1);
        }
        (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafOffset = (int64)iOff;
    }
}

internal static int32 /*r*/ _fts5UsePatternMatch(ref libc.TLS tls, uintptr pConfig, uintptr p) {
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FePattern == (int32)FTS5_PATTERN_GLOB && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)FTS5_PATTERN_GLOB) {
        return (int32)1;
    }
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Ft.FePattern == (int32)FTS5_PATTERN_LIKE && (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)FTS5_PATTERN_LIKE || libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)FTS5_PATTERN_GLOB)) {
        return (int32)1;
    }
    return 0;
}

internal static int32 /*r*/ _fts5VocabBestIndexMethod(ж<libc.TLS> tls, uintptr pUnused, uintptr pInfo) {
    int32 i = default;
    int32 iTermEq = default;
    int32 iTermGe = default;
    int32 iTermLe = default;
    int32 idxNum = default;
    int32 nArg = default;
    int32 v2 = default;
    uintptr p = default;
    _ = i;
    _ = iTermEq;
    _ = iTermGe;
    _ = iTermLe;
    _ = idxNum;
    _ = nArg;
    _ = p;
    _ = v2;
    iTermEq = -(int32)1;
    iTermGe = -(int32)1;
    iTermLe = -(int32)1;
    idxNum = libc.Int32FromUint64((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FcolUsed);
    nArg = 0;
    _ = pUnused;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FnConstraint)) {
            break;
        }
        p = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FaConstraint + (uintptr)i * 12;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fusable) == 0) {
            goto _1;
        }
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn == 0 && _fts5VocabIsBinary(tls, pInfo, i) != 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
                iTermEq = i;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_LE) {
                iTermLe = i;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_LT) {
                iTermLe = i;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_GE) {
                iTermGe = i;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_GT) {
                iTermGe = i;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    if (iTermEq >= 0) {
        idxNum = idxNum | (int32)FTS5_VOCAB_TERM_EQ;
        nArg = nArg + 1;
        v2 = nArg;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FaConstraintUsage + (uintptr)iTermEq * 8))).Value.FargvIndex = v2;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FestimatedCost = libc.Float64FromInt32(100);
    } else {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FestimatedCost = libc.Float64FromInt32(1000000);
        if (iTermGe >= 0) {
            idxNum = idxNum | (int32)FTS5_VOCAB_TERM_GE;
            nArg = nArg + 1;
            v2 = nArg;
            (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FaConstraintUsage + (uintptr)iTermGe * 8))).Value.FargvIndex = v2;
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FestimatedCost = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FestimatedCost / libc.Float64FromInt32(2);
        }
        if (iTermLe >= 0) {
            idxNum = idxNum | (int32)FTS5_VOCAB_TERM_LE;
            nArg = nArg + 1;
            v2 = nArg;
            (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FaConstraintUsage + (uintptr)iTermLe * 8))).Value.FargvIndex = v2;
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FestimatedCost = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FestimatedCost / libc.Float64FromInt32(2);
        }
    }
    if ((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FnOrderBy == (int32)1 && (@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FaOrderBy))).Value.FiColumn == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FaOrderBy))).Value.Fdesc) == 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.ForderByConsumed = (int32)1;
    }
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pInfo)).Value.FidxNum = idxNum;
    return SQLITE_OK;
}

internal static void _fts5yy_destructor(ж<libc.TLS> tls, uintptr fts5yypParser, uint8 fts5yymajor, @unsafe.Pointer fts5yypminor) {
    uintptr pParse = default;
    _ = pParse;
    pParse = (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.FpParse;
    var exprᴛ1 = libc.Int32FromUint8(fts5yymajor);
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)16) { matchᴛ1 = true;
        _ = pParse;
    }
    else if (exprᴛ1 == (int32)17) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)18) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)19) { matchᴛ1 = true;
        _sqlite3Fts5ParseNodeFree(tls, ~@unsafe.Pointer.To<uintptr>(fts5yypminor));
    }
    else if (exprᴛ1 == (int32)20) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)21) { matchᴛ1 = true;
        Xsqlite3_free(tls, ~@unsafe.Pointer.To<uintptr>(fts5yypminor));
    }
    else if (exprᴛ1 == (int32)22) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)23) { matchᴛ1 = true;
        _sqlite3Fts5ParseNearsetFree(tls, ~@unsafe.Pointer.To<uintptr>(fts5yypminor));
    }
    else if (exprᴛ1 == (int32)24) { matchᴛ1 = true;
        do {
            _sqlite3Fts5ParsePhraseFree(tls, ~@unsafe.Pointer.To<uintptr>(fts5yypminor));
            break;
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        do {
            break;
        } while (false);
    }

}

internal static uint8 /*r*/ _fts5yy_find_reduce_action(ref libc.TLS tls, uint8 stateno, uint8 iLookAhead) {
    int32 i = default;
    _ = i;
    i = (int32)_fts5yy_reduce_ofst[stateno];
    i = i + libc.Int32FromUint8(iLookAhead);
    return _fts5yy_action[i];
}

internal static uint8 /*r*/ _fts5yy_find_shift_action(ref libc.TLS tls, uint8 iLookAhead, uint8 stateno) {
    uint8 r = default;

    int32 i = default;
    _ = i;
    if (libc.Int32FromUint8(stateno) > (int32)fts5YY_MAX_SHIFT) {
        return stateno;
    }
    for (var cond = true; cond; cond = (int32)1 != 0) {
        i = libc.Int32FromUint8(_fts5yy_shift_ofst[stateno]);
        i = i + libc.Int32FromUint8(iLookAhead);
        if (libc.Int32FromUint8(_fts5yy_lookahead[i]) != libc.Int32FromUint8(iLookAhead)) {
            return _fts5yy_default[stateno];
        } else {
            return _fts5yy_action[i];
        }
    }
    return r;
}

internal static void _geopolyAddSegments(ref libc.TLS tls, uintptr p, uintptr pPoly, uint8 side) {
    uint32 i = default;
    uintptr x = default;
    _ = i;
    _ = x;
    i = (uint32)0;
    for (;;) {
        if (!(i < libc.Uint32FromInt32((@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)pPoly)).Value.FnVertex) - (uint32)1)) {
            break;
        }
        x = pPoly + 8 + (uintptr)(i * (uint32)2) * 4;
        _geopolyAddOneSegment(ref tls, p, ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(x)), ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(x + 1 * 4)), ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(x + 2 * 4)), ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(x + 3 * 4)), side, i);
        goto _1;
_1:;
        i = i + 1;
    }
    x = pPoly + 8 + (uintptr)(i * (uint32)2) * 4;
    _geopolyAddOneSegment(ref tls, p, ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(x)), ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(x + 1 * 4)), ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pPoly + 8)), ~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(pPoly + 8 + 1 * 4)), side, i);
}

internal static int32 /*r*/ _getAndInitPage(ж<libc.TLS> tls, uintptr pBt, TPgno pgno, @unsafe.Pointer ppPage, int32 bReadOnly) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pPage = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = pPage;
        _ = rc;
        if (pgno > _btreePagecount(ref (tls).DerefOrNull(), pBt)) {
            (@unsafe.Pointer.To<uintptr>(ppPage)).Value = (uintptr)0;
            r = _sqlite3CorruptError(tls, (int32)75568); goto ᒐdone;
        }
        rc = _sqlite3PagerGet(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, pgno, bp, bReadOnly);
        if (rc != 0) {
            (@unsafe.Pointer.To<uintptr>(ppPage)).Value = (uintptr)0;
            r = rc; goto ᒐdone;
        }
        pPage = _sqlite3PagerGetExtra(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)pPage)).Value.FisInit) == 0) {
            _btreePageFromDbPage(ref (tls).DerefOrNull(), ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), pgno, pBt);
            rc = _btreeInitPage(tls, pPage);
            if (rc != SQLITE_OK) {
                _releasePage(tls, pPage);
                (@unsafe.Pointer.To<uintptr>(ppPage)).Value = (uintptr)0;
                r = rc; goto ᒐdone;
            }
        }
        (@unsafe.Pointer.To<uintptr>(ppPage)).Value = pPage;
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _getLocalPayload(ref libc.TLS tls, int32 nUsable, Tu8 flags, int32 nTotal) {
    int32 nLocal = default;
    int32 nMaxLocal = default;
    int32 nMinLocal = default;
    _ = nLocal;
    _ = nMaxLocal;
    _ = nMinLocal;
    if (libc.Int32FromUint8(flags) == (int32)0x0D) {
        nMinLocal = (nUsable - (int32)12) * (int32)32 / (int32)255 - (int32)23;
        nMaxLocal = nUsable - (int32)35;
    } else {
        nMinLocal = (nUsable - (int32)12) * (int32)32 / (int32)255 - (int32)23;
        nMaxLocal = (nUsable - (int32)12) * (int32)64 / (int32)255 - (int32)23;
    }
    nLocal = nMinLocal + rem((nTotal - nMinLocal), (nUsable - (int32)4));
    if (nLocal > nMaxLocal) {
        nLocal = nMinLocal;
    }
    return nLocal;
}

internal static int32 /*r*/ _hasHotJournal(ж<libc.TLS> tls, uintptr pPager, @unsafe.Pointer pExists) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 jrnlOpen = default;
        int32 rc = default;
        ref var nPage = ref heap(new TPgno(), out var ᏑnPage);
        uintptr pVfs = default;
        int32 _ᴛ1 = default;
        int32 _ᴛ2 = default;
        Tu8 _ᴛ3 = default;
        int32 _ᴛ4 = default;
        _ = jrnlOpen;
        _ = nPage;
        _ = pVfs;
        _ = rc;
        pVfs = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)1;
        jrnlOpen = libc.BoolInt32(!!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != libc.UintptrFromInt32(0)));
        (@unsafe.Pointer.To<int32>(pExists)).Value = 0;
        if (!(jrnlOpen != 0)) {
            rc = _sqlite3OsAccess(tls, pVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, SQLITE_ACCESS_EXISTS, bp);
        }
        if (rc == SQLITE_OK && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4))).Value = 0;
            rc = _sqlite3OsCheckReservedLock(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, bp + 4);
            if (rc == SQLITE_OK && !(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 4)) != 0)) {
                rc = _pagerPagecount(tls, pPager, @unsafe.Pointer.FromPinnedBox(ᏑnPage));
                if (rc == SQLITE_OK) {
                    if (nPage == (uint32)0 && !(jrnlOpen != 0)) {
                        _sqlite3BeginBenignMalloc(tls);
                        if (_pagerLockDb(tls, pPager, (int32)RESERVED_LOCK) == SQLITE_OK) {
                            _sqlite3OsDelete(tls, pVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, 0);
                            if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0)) {
                                _pagerUnlockDb(tls, pPager, (int32)SHARED_LOCK);
                            }
                        }
                        _sqlite3EndBenignMalloc(tls);
                    } else {
                        if (!(jrnlOpen != 0)) {
                            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8))).Value = libc.Int32FromInt32(SQLITE_OPEN_READONLY) | libc.Int32FromInt32(SQLITE_OPEN_MAIN_JOURNAL);
                            rc = _sqlite3OsOpen(tls, pVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp + 8)), bp + 8);
                        }
                        if (rc == SQLITE_OK) {
                            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(bp + 12))).Value = (uint8)0;
                            rc = _sqlite3OsRead(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, bp + 12, (int32)1, 0);
                            if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))))) {
                                rc = SQLITE_OK;
                            }
                            if (!(jrnlOpen != 0)) {
                                _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
                            }
                            (@unsafe.Pointer.To<int32>(pExists)).Value = libc.BoolInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(bp + 12))) != 0);
                        } else {
                            if (rc == (int32)SQLITE_CANTOPEN) {
                                (@unsafe.Pointer.To<int32>(pExists)).Value = (int32)1;
                                rc = SQLITE_OK;
                            }
                        }
                    }
                }
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr _inodeList = (uintptr)0;

internal static void _invalidateIncrblobCursors(ref libc.TLS tls, uintptr pBtree, TPgno pgnoRoot, Ti64 iRow, int32 isClearTable) {
    uintptr p = default;
    _ = p;
    (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)pBtree)).Value.FhasIncrblobCur = (uint8)0;
    p = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)pBtree)).Value.FpBt)).Value.FpCursor;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FcurFlags) & (int32)BTCF_Incrblob) != 0) {
            (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)pBtree)).Value.FhasIncrblobCur = (uint8)1;
            if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FpgnoRoot == pgnoRoot && (isClearTable != 0 || (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.Finfo.FnKey == iRow)) {
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FeState = (uint8)CURSOR_INVALID;
            }
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FpNext;
    }
}

internal static void _invokeProfileCallback(ж<libc.TLS> tls, uintptr db, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tsqlite3_int64 _ᴛ1 = default;
        Tsqlite3_int64 _ᴛ2 = default;
        _sqlite3OsCurrentTimeInt64(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpVfs, bp);
        (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 8))).Value = (~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp)) - (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FstartTime) * (int64)1000000;
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProfile != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, Tu64>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxProfile)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpProfileArg, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql, libc.Uint64FromInt64(~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(bp + 8))));
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmTrace) & (int32)SQLITE_TRACE_PROFILE) != 0) {
            GoFuncCookie.Func<Func<ж<libc.TLS>, Tu32, uintptr, uintptr, uintptr, int32>>(~@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db).of(Tsqlite3.ᏑFtrace).Reinterpret<Tsqlite3ᴛ1_Ftrace, uintptr>())(tls, (uint32)SQLITE_TRACE_PROFILE, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpTraceArg, p, bp + 8);
        }
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FstartTime = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _isLimitTerm(ref libc.TLS tls, uintptr pTerm) {
    return libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeMatchOp) >= (int32)SQLITE_INDEX_CONSTRAINT_LIMIT && libc.Int32FromUint8((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FeMatchOp) <= (int32)SQLITE_INDEX_CONSTRAINT_OFFSET);
}

internal static Ti64 /*r*/ _journalHdrOffset(ref libc.TLS tls, uintptr pPager) {
    Ti64 c = default;
    Ti64 offset = default;
    _ = c;
    _ = offset;
    offset = 0;
    c = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff;
    if (c != 0) {
        offset = (quo((c - (int64)1), libc.Int64FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize)) + (int64)1) * libc.Int64FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize);
    }
    return offset;
}

internal static int32 /*r*/ _jsonEachBestIndex(ж<libc.TLS> tls, uintptr tab, uintptr pIdxInfo) {
    array<int32> aIdx = new(2);
    int32 i = default;
    int32 iCol = default;
    int32 iMask = default;
    int32 idxMask = default;
    int32 unusableMask = default;
    int32 v1 = default;
    uintptr pConstraint = default;
    _ = aIdx;
    _ = i;
    _ = iCol;
    _ = iMask;
    _ = idxMask;
    _ = pConstraint;
    _ = unusableMask;
    _ = v1;
    unusableMask = 0;
    idxMask = 0;
    _ = tab;
    v1 = -libc.Int32FromInt32(1);
    aIdx[(int32)1] = v1;
    aIdx[0] = v1;
    pConstraint = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
            break;
        }
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.FiColumn < (int32)JEACH_JSON) {
            goto _2;
        }
        iCol = (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.FiColumn - (int32)JEACH_JSON;
        iMask = ((int32)1).Lsh((int64)(iCol));
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.Fusable) == 0) {
            unusableMask = unusableMask | iMask;
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
                aIdx[iCol] = i;
                idxMask = idxMask | iMask;
            }
        }
        goto _2;
_2:;
        i = i + 1;
        pConstraint += 12;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnOrderBy > 0 && (@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaOrderBy))).Value.FiColumn < 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_orderby>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaOrderBy))).Value.Fdesc) == 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.ForderByConsumed = (int32)1;
    }
    if ((unusableMask & ~idxMask) != 0) {
        return (int32)SQLITE_CONSTRAINT;
    }
    if (aIdx[0] < 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = 0;
    } else {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)1D;
        i = aIdx[0];
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.FargvIndex = (int32)1;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.Fomit = (uint8)1;
        if (aIdx[(int32)1] < 0) {
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = (int32)1;
        } else {
            i = aIdx[(int32)1];
            (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.FargvIndex = (int32)2;
            (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)i * 8))).Value.Fomit = (uint8)1;
            (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = (int32)3;
        }
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _jsonEachRowid(ж<libc.TLS> tls, uintptr cur, uintptr pRowid) {
    uintptr p = default;
    _ = p;
    p = cur;
    (@unsafe.Pointer.To<Tsqlite_int64>(__ccgo_ptr(pRowid))).Value = libc.Int64FromUint32((@unsafe.Pointer.To<TJsonEachCursor>((@unsafe.Pointer)p)).Value.FiRowid);
    return SQLITE_OK;
}

internal static Tu8 /*r*/ _jsonHexToInt(ref libc.TLS tls, int32 h) {
    h = h + (int32)9 * ((int32)1 & (h >> (int)((int32)6)));
    return libc.Uint8FromInt32(h & libc.Int32FromInt32(0xf));
}

internal static void _jsonStringTrimOneChar(ref libc.TLS tls, uintptr p) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FeErr) == 0) {
        (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed = (@unsafe.Pointer.To<TJsonString>((@unsafe.Pointer)p)).Value.FnUsed - 1;
    }
}

internal static Tu32 /*r*/ _jsonbPayloadSizeInline(ref libc.TLS tls, uintptr pParse, Tu32 i, @unsafe.Pointer pSz) {
    Tu32 n = default;
    Tu32 sz = default;
    Tu8 x = default;
    Tu8 v1 = default;
    _ = n;
    _ = sz;
    _ = x;
    _ = v1;
    if (i >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) {
        (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
        return (uint32)0;
    } else {
        v1 = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)i))).Rsh((int64)(libc.Int32FromInt32(4))));
        x = v1;
        if (libc.Int32FromUint8(v1) <= (int32)11) {
            sz = (uint32)x;
            n = (uint32)1;
        } else {
            if (libc.Int32FromUint8(x) == (int32)12) {
                if (i + (uint32)1 >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) {
                    (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
                    return (uint32)0;
                }
                sz = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)1))));
                n = (uint32)2;
            } else {
                if (libc.Int32FromUint8(x) == (int32)13) {
                    if (i + (uint32)2 >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) {
                        (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
                        return (uint32)0;
                    }
                    sz = libc.Uint32FromInt32((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)1)))) << (int)((int32)8)) + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)2)))));
                    n = (uint32)3;
                } else {
                    if (libc.Int32FromUint8(x) == (int32)14) {
                        if (i + (uint32)4 >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) {
                            (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
                            return (uint32)0;
                        }
                        sz = ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)1))))).Lsh((int64)(libc.Int32FromInt32(24))) + libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)2)))).Lsh((int64)(libc.Int32FromInt32(16)))) + libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)3)))).Lsh((int64)(libc.Int32FromInt32(8)))) + (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)4))));
                        n = (uint32)5;
                    } else {
                        if (i + (uint32)8 >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)1)))) != 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)2)))) != 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)3)))) != 0 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)4)))) != 0) {
                            (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
                            return (uint32)0;
                        }
                        sz = ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)5))))).Lsh((int64)(libc.Int32FromInt32(24))) + libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)6)))).Lsh((int64)(libc.Int32FromInt32(16)))) + libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)7)))).Lsh((int64)(libc.Int32FromInt32(8)))) + (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)8))));
                        n = (uint32)9;
                    }
                }
            }
        }
    }
    if (libc.Int64FromUint32(i) + libc.Int64FromUint32(sz) + libc.Int64FromUint32(n) > libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) && libc.Int64FromUint32(i) + libc.Int64FromUint32(sz) + libc.Int64FromUint32(n) > libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob - libc.Uint32FromInt32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Fdelta))) {
        (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
        return (uint32)0;
    }
    (@unsafe.Pointer.To<Tu32>(pSz)).Value = sz;
    return n;
}

internal static Tu32 /*r*/ _jsonbPayloadSizeSemiInline(ref libc.TLS tls, uintptr pParse, Tu32 i, @unsafe.Pointer pSz) {
    Tu32 n = default;
    Tu32 sz = default;
    Tu8 x = default;
    Tu8 v1 = default;
    _ = n;
    _ = sz;
    _ = x;
    _ = v1;
    v1 = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)i))).Rsh((int64)(libc.Int32FromInt32(4))));
    x = v1;
    if (libc.Int32FromUint8(v1) <= (int32)11) {
        sz = (uint32)x;
        n = (uint32)1;
    } else {
        if (libc.Int32FromUint8(x) == (int32)12) {
            if (i + (uint32)1 >= (@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) {
                (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
                return (uint32)0;
            }
            sz = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FaBlob + (uintptr)(i + (uint32)1))));
            n = (uint32)2;
        } else {
            return _jsonbPayloadSizeWide(ref tls, (@unsafe.Pointer)pParse, i, pSz, x);
        }
    }
    if (libc.Int64FromUint32(i) + libc.Int64FromUint32(sz) + libc.Int64FromUint32(n) > libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob) && libc.Int64FromUint32(i) + libc.Int64FromUint32(sz) + libc.Int64FromUint32(n) > libc.Int64FromUint32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.FnBlob - libc.Uint32FromInt32((@unsafe.Pointer.To<TJsonParse>((@unsafe.Pointer)pParse)).Value.Fdelta))) {
        (@unsafe.Pointer.To<Tu32>(pSz)).Value = (uint32)0;
        return (uint32)0;
    }
    (@unsafe.Pointer.To<Tu32>(pSz)).Value = sz;
    return n;
}

internal static int32 /*r*/ _lookasideMallocSize(ref libc.TLS tls, uintptr db, uintptr p) {
    int32 v1 = default;
    _ = v1;
    if (p < (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpMiddle) {
        v1 = libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FszTrue);
    } else {
        v1 = (int32)LOOKASIDE_SMALL;
    }
    return v1;
}

internal static int32 /*r*/ _memdbEnlarge(ж<libc.TLS> tls, uintptr p, Tsqlite3_int64 newSz) {
    uintptr pNew = default;
    _ = pNew;
    if (((@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FmFlags & (uint32)SQLITE_DESERIALIZE_RESIZEABLE) == (uint32)0 || (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FnMmap > 0) {
        return (int32)SQLITE_FULL;
    }
    if (newSz > (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax) {
        return (int32)SQLITE_FULL;
    }
    newSz = newSz * (int64)2;
    if (newSz > (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax) {
        newSz = (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax;
    }
    pNew = _sqlite3Realloc(tls, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData, libc.Uint64FromInt64(newSz));
    if (pNew == (uintptr)0) {
        return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData = pNew;
    (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszAlloc = newSz;
    return SQLITE_OK;
}

internal static readonly ж<int32> Ꮡ_moderncFdatasync = new StaticBox<int32>((int32)0);
internal static ref int32 _moderncFdatasync => ref ((StaticBox<int32>)Ꮡ_moderncFdatasync).ValueSlot;

internal static void _nodeGetCoord(ref libc.TLS tls, uintptr pRtree, uintptr pNode, int32 iCell, int32 iCoord, @unsafe.Pointer pCoord) {
    _readCoord(ref tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + (uintptr)((int32)12 + libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell) * iCell + (int32)4 * iCoord), pCoord);
}

internal static uint32 /*r*/ _nodeHash(ref libc.TLS tls, Ti64 iNode) {
    return libc.Uint32FromInt64(iNode) % (uint32)HASHSIZE;
}

internal static int32 /*r*/ _nodeInsertCell(ref libc.TLS tls, uintptr pRtree, uintptr pNode, uintptr pCell) {
    int32 nCell = default;
    int32 nMaxCell = default;
    _ = nCell;
    _ = nMaxCell;
    nMaxCell = quo(((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FiNodeSize - (int32)4), libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell));
    nCell = _readInt16(ref tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2);
    if (nCell < nMaxCell) {
        _nodeOverwriteCell(ref tls, pRtree, pNode, (@unsafe.Pointer)pCell, nCell);
        _writeInt16(ref tls, (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + 2, nCell + (int32)1);
        (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FisDirty = (int32)1;
    }
    return libc.BoolInt32(nCell == nMaxCell);
}

internal static void _nodeOverwriteCell(ref libc.TLS tls, uintptr pRtree, uintptr pNode, @unsafe.Pointer pCell, int32 iCell) {
    int32 ii = default;
    uintptr p = default;
    _ = ii;
    _ = p;
    p = (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FzData + (uintptr)((int32)4 + libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnBytesPerCell) * iCell);
    p = p + (uintptr)_writeInt64(ref tls, p, ~@unsafe.Pointer.To<Ti64>(pCell));
    ii = 0;
    for (;;) {
        if (!(ii < libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnDim2))) {
            break;
        }
        p = p + (uintptr)_writeCoord(ref tls, p, @unsafe.Add(@unsafe.Add(pCell, 8), (uintptr)ii * 4));
        goto _1;
_1:;
        ii = ii + 1;
    }
    (@unsafe.Pointer.To<TRtreeNode>((@unsafe.Pointer)pNode)).Value.FisDirty = (int32)1;
}

internal static int32 /*r*/ _nolockCheckReservedLock(ж<libc.TLS> tls, uintptr NotUsed, uintptr pResOut) {
    _ = NotUsed;
    (@unsafe.Pointer.To<int32>(__ccgo_ptr(pResOut))).Value = 0;
    return SQLITE_OK;
}

internal static int32 /*r*/ _nolockClose(ж<libc.TLS> tls, uintptr id) {
    return _closeUnixFile(tls, id);
}

internal static readonly ж<uintptr> Ꮡ_nolockIoFinder = new StaticBox<uintptr>((uintptr)0);
internal static ref uintptr _nolockIoFinder => ref ((StaticBox<uintptr>)Ꮡ_nolockIoFinder).ValueSlot;

internal static uintptr /*r*/ _nolockIoFinderImpl(ж<libc.TLS> tls, uintptr z, uintptr p) {
    _ = z;
    _ = p;
    return (uintptr)Ꮡ_nolockIoMethods;
}

internal static readonly ж<Tsqlite3_io_methods> Ꮡ_nolockIoMethods = new StaticBox<Tsqlite3_io_methods>(new Tsqlite3_io_methods(
    FiVersion: (int32)3
));
internal static ref Tsqlite3_io_methods _nolockIoMethods => ref ((StaticBox<Tsqlite3_io_methods>)Ꮡ_nolockIoMethods).ValueSlot;

internal static int32 /*r*/ _nolockLock(ж<libc.TLS> tls, uintptr NotUsed, int32 NotUsed2) {
    _ = NotUsed;
    _ = NotUsed2;
    return SQLITE_OK;
}

internal static int32 /*r*/ _nolockUnlock(ж<libc.TLS> tls, uintptr NotUsed, int32 NotUsed2) {
    _ = NotUsed;
    _ = NotUsed2;
    return SQLITE_OK;
}

internal static Tu16 /*r*/ _numericType(ж<libc.TLS> tls, uintptr pMem) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_IntReal) | libc.Int32FromInt32(MEM_Null))) != 0) {
        return libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_IntReal) | libc.Int32FromInt32(MEM_Null)));
    }
    return _computeNumericType(tls, pMem);
    return (uint16)0;
}

internal static int32 /*r*/ _openSubJournal(ж<libc.TLS> tls, uintptr pPager) {
    int32 flags = default;
    int32 nStmtSpill = default;
    int32 rc = default;
    _ = flags;
    _ = nStmtSpill;
    _ = rc;
    rc = SQLITE_OK;
    if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd)).Value.FpMethods != libc.UintptrFromInt32(0))) {
        flags = libc.Int32FromInt32(SQLITE_OPEN_SUBJOURNAL) | libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE) | libc.Int32FromInt32(SQLITE_OPEN_EXCLUSIVE) | libc.Int32FromInt32(SQLITE_OPEN_DELETEONCLOSE);
        nStmtSpill = _sqlite3Config.FnStmtSpill;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_MEMORY || (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsubjInMemory != 0) {
            nStmtSpill = -(int32)1;
        }
        rc = _sqlite3JournalOpen(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (uintptr)0, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd, flags, nStmtSpill);
    }
    return rc;
}

internal static Tu16 /*r*/ _operatorMask(ref libc.TLS tls, int32 op) {
    Tu16 c = default;
    _ = c;
    if (op >= (int32)TK_EQ) {
        c = libc.Uint16FromInt32(libc.Int32FromInt32(WO_EQ).Lsh((int64)(op - libc.Int32FromInt32(TK_EQ))));
    } else {
        if (op == (int32)TK_IN) {
            c = (uint16)WO_IN;
        } else {
            if (op == (int32)TK_ISNULL) {
                c = (uint16)WO_ISNULL;
            } else {
                c = (uint16)WO_IS;
            }
        }
    }
    return c;
}

internal static int32 /*r*/ _osLocaltime(ж<libc.TLS> tls, uintptr t, uintptr pTm) {
    uintptr mutex = default;
    uintptr pX = default;
    int32 rc = default;
    _ = mutex;
    _ = pX;
    _ = rc;
    mutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_MAIN);
    Xsqlite3_mutex_enter(tls, mutex);
    pX = libc.Xlocaltime(tls, t);
    if (_sqlite3Config.FbLocaltimeFault != 0) {
        if (_sqlite3Config.FxAltLocaltime != (uintptr)0 && 0 == GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_sqlite3Config.FxAltLocaltime)(tls, t, pTm)) {
            pX = pTm;
        } else {
            pX = (uintptr)0;
        }
    }
    if (pX != 0) {
        (@unsafe.Pointer.To<Ttm>(__ccgo_ptr(pTm))).Value = (@unsafe.Pointer.To<Ttm>(__ccgo_ptr(pX))).Value;
    }
    Xsqlite3_mutex_leave(tls, mutex);
    rc = libc.BoolInt32(pX == (uintptr)0);
    return rc;
}

internal static int32 /*r*/ _pagerExclusiveLock(ж<libc.TLS> tls, uintptr pPager) {
    Tu8 eOrigLock = default;
    int32 rc = default;
    _ = eOrigLock;
    _ = rc;
    eOrigLock = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock;
    rc = _pagerLockDb(tls, pPager, (int32)EXCLUSIVE_LOCK);
    if (rc != SQLITE_OK) {
        _pagerUnlockDb(tls, pPager, libc.Int32FromUint8(eOrigLock));
    }
    return rc;
}

internal static int32 /*r*/ _pagerFlushOnCommit(ref libc.TLS tls, uintptr pPager, int32 bCommit) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile) == 0) {
        return (int32)1;
    }
    if (!(bCommit != 0)) {
        return 0;
    }
    if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != libc.UintptrFromInt32(0))) {
        return 0;
    }
    return libc.BoolInt32(_sqlite3PCachePercentDirty(ref tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache) >= (int32)25);
}

internal static int32 /*r*/ _pagerLockDb(ж<libc.TLS> tls, uintptr pPager, int32 eLock) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) < eLock || libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) == libc.Int32FromInt32(EXCLUSIVE_LOCK) + libc.Int32FromInt32(1)) {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoLock != 0) {
            v1 = SQLITE_OK;
        } else {
            v1 = _sqlite3OsLock(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, eLock);
        }
        rc = v1;
        if (rc == SQLITE_OK && (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) != libc.Int32FromInt32(EXCLUSIVE_LOCK) + libc.Int32FromInt32(1) || eLock == (int32)EXCLUSIVE_LOCK)) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock = libc.Uint8FromInt32(eLock);
        }
    }
    return rc;
}

internal static int32 /*r*/ _pagerOpenWalIfPresent(ж<libc.TLS> tls, uintptr pPager) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var nPage = ref heap(new TPgno(), out var ᏑnPage);
        int32 rc = default;
        int32 _ᴛ1 = default;
        _ = nPage;
        _ = rc;
        rc = SQLITE_OK;
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0)) {
            rc = _sqlite3OsAccess(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzWal, SQLITE_ACCESS_EXISTS, bp);
            if (rc == SQLITE_OK) {
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0) {
                    rc = _pagerPagecount(tls, pPager, @unsafe.Pointer.FromPinnedBox(ᏑnPage));
                    if (rc != 0) {
                        r = rc; goto ᒐdone;
                    }
                    if (nPage == (uint32)0) {
                        rc = _sqlite3OsDelete(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzWal, 0);
                    } else {
                        rc = _sqlite3PagerOpenWal(tls, pPager, nil);
                    }
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_WAL) {
                        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode = (uint8)PAGER_JOURNALMODE_DELETE;
                    }
                }
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _pagerPagecount(ж<libc.TLS> tls, uintptr pPager, @unsafe.Pointer pnPage) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        TPgno nPage = default;
        int32 rc = default;
        Ti64 _ᴛ1 = default;
        _ = nPage;
        _ = rc;
        nPage = _sqlite3WalDbsize(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal);
        if (nPage == (uint32)0 && (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0) {
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp))).Value = 0;
            rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, bp);
            if (rc != SQLITE_OK) {
                r = rc; goto ᒐdone;
            }
            nPage = libc.Uint32FromInt64(quo((~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) + (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize - libc.Int64FromInt32(1)), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize));
        }
        if (nPage > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmxPgno) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmxPgno = nPage;
        }
        (@unsafe.Pointer.To<TPgno>(pnPage)).Value = nPage;
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _pagerReleaseMapPage(ж<libc.TLS> tls, uintptr pPg) {
    uintptr pPager = default;
    _ = pPager;
    pPager = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnMmapOut = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnMmapOut - 1;
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpDirty = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpMmapFreelist;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpMmapFreelist = pPg;
    _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, libc.Int64FromUint32((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno - libc.Uint32FromInt32(1)) * (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData);
}

internal static void _pagerUnlockAndRollback(ж<libc.TLS> tls, uintptr pPager) {
    Tu8 eLock = default;
    int32 errCode = default;
    _ = eLock;
    _ = errCode;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) != (int32)PAGER_ERROR && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) != PAGER_OPEN) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) >= (int32)PAGER_WRITER_LOCKED) {
            _sqlite3BeginBenignMalloc(tls);
            _sqlite3PagerRollback(tls, pPager);
            _sqlite3EndBenignMalloc(tls);
        } else {
            if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0)) {
                _pager_end_transaction(tls, pPager, 0, 0);
            }
        }
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_ERROR && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_MEMORY && (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0) {
            errCode = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode;
            eLock = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_OPEN;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode = SQLITE_OK;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock = (uint8)EXCLUSIVE_LOCK;
            _pager_playback(tls, pPager, (int32)1);
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode = errCode;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock = eLock;
        }
    }
    _pager_unlock(tls, pPager);
}

internal static int32 /*r*/ _pagerUnlockDb(ж<libc.TLS> tls, uintptr pPager, int32 eLock) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd)).Value.FpMethods != (uintptr)0) {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoLock != 0) {
            v1 = SQLITE_OK;
        } else {
            v1 = _sqlite3OsUnlock(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, eLock);
        }
        rc = v1;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) != libc.Int32FromInt32(EXCLUSIVE_LOCK) + libc.Int32FromInt32(1)) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock = libc.Uint8FromInt32(eLock);
        }
    }
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FchangeCountDone = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile;
    return rc;
}

internal static int32 /*r*/ _pager_end_transaction(ж<libc.TLS> tls, uintptr pPager, int32 hasSuper, int32 bCommit) {
    int32 bDelete = default;
    int32 rc = default;
    int32 rc2 = default;
    int32 v1 = default;
    _ = bDelete;
    _ = rc;
    _ = rc2;
    _ = v1;
    rc = SQLITE_OK;
    rc2 = SQLITE_OK;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) < (int32)PAGER_WRITER_LOCKED && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) < (int32)RESERVED_LOCK) {
        return SQLITE_OK;
    }
    _releaseAllSavepoints(tls, pPager);
    if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0) {
        if (_sqlite3JournalIsInMemory(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd) != 0) {
            _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
        } else {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_TRUNCATE) {
                if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff == 0) {
                    rc = SQLITE_OK;
                } else {
                    rc = _sqlite3OsTruncate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, 0);
                    if (rc == SQLITE_OK && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FfullSync != 0) {
                        rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsyncFlags));
                    }
                }
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = 0;
            } else {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_PERSIST || (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) < (int32)PAGER_JOURNALMODE_WAL) {
                    rc = _zeroJournalHdr(tls, pPager, libc.BoolInt32(hasSuper != 0 || (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0));
                    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = 0;
                } else {
                    bDelete = libc.BoolInt32(!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0));
                    _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
                    if (bDelete != 0) {
                        rc = _sqlite3OsDelete(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FextraSync));
                    }
                }
            }
        }
    }
    _sqlite3BitvecDestroy(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal = (uintptr)0;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnRec = 0;
    if (rc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmemDb != 0 || _pagerFlushOnCommit(ref (tls).DerefOrNull(), pPager, bCommit) != 0) {
            _sqlite3PcacheCleanAll(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
        } else {
            _sqlite3PcacheClearWritable(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
        }
        _sqlite3PcacheTruncate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize);
    }
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
        rc2 = _sqlite3WalEndWriteTransaction(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal);
    } else {
        if (rc == SQLITE_OK && bCommit != 0 && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize) {
            rc = _pager_truncate(tls, pPager, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize);
        }
    }
    if (rc == SQLITE_OK && bCommit != 0) {
        rc = _sqlite3OsFileControl(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (int32)SQLITE_FCNTL_COMMIT_PHASETWO, (uintptr)0);
        if (rc == (int32)SQLITE_NOTFOUND) {
            rc = SQLITE_OK;
        }
    }
    if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0) && (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != libc.UintptrFromInt32(0)) || _sqlite3WalExclusiveMode(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, 0) != 0)) {
        rc2 = _pagerUnlockDb(tls, pPager, (int32)SHARED_LOCK);
    }
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_READER;
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsetSuper = (uint8)0;
    if (rc == SQLITE_OK) {
        v1 = rc2;
    } else {
        v1 = rc;
    }
    return v1;
}

internal static int32 /*r*/ _pager_open_journal(ж<libc.TLS> tls, uintptr pPager) {
    int32 flags = default;
    int32 nSpill = default;
    int32 rc = default;
    uintptr pVfs = default;
    _ = flags;
    _ = nSpill;
    _ = pVfs;
    _ = rc;
    rc = SQLITE_OK;
    pVfs = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
        return (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode;
    }
    if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != libc.UintptrFromInt32(0)) && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) != (int32)PAGER_JOURNALMODE_OFF) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal = _sqlite3BitvecCreate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize);
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
        if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != libc.UintptrFromInt32(0))) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_MEMORY) {
                _sqlite3MemJournalOpen(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
            } else {
                flags = libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE);
                if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0) {
                    flags = flags | (libc.Int32FromInt32(SQLITE_OPEN_DELETEONCLOSE) | libc.Int32FromInt32(SQLITE_OPEN_TEMP_JOURNAL));
                    flags = flags | (int32)SQLITE_OPEN_EXCLUSIVE;
                    nSpill = _sqlite3Config.FnStmtSpill;
                } else {
                    flags = flags | (int32)SQLITE_OPEN_MAIN_JOURNAL;
                    nSpill = _jrnlBufferSize(ref (tls).DerefOrNull(), pPager);
                }
                rc = _databaseIsUnmoved(tls, pPager);
                if (rc == SQLITE_OK) {
                    rc = _sqlite3JournalOpen(tls, pVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, flags, nSpill);
                }
            }
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnRec = 0;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = 0;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsetSuper = (uint8)0;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalHdr = 0;
            rc = _writeJournalHdr(tls, pPager);
        }
    }
    if (rc != SQLITE_OK) {
        _sqlite3BitvecDestroy(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal);
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpInJournal = (uintptr)0;
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = 0;
    } else {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_WRITER_CACHEMOD;
    }
    return rc;
}

internal static uintptr /*r*/ _parserStackRealloc(ж<libc.TLS> tls, uintptr pOld, Tsqlite3_uint64 newSize, uintptr pParse) {
    uintptr p = default;
    uintptr v1 = default;
    _ = p;
    _ = v1;
    if (_sqlite3FaultSim(tls, (int32)700) != 0) {
        v1 = (uintptr)0;
    } else {
        v1 = Xsqlite3_realloc(tls, pOld, libc.Int32FromUint64(newSize));
    }
    p = v1;
    if (p == (uintptr)0) {
        _sqlite3OomFault(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb);
    }
    return p;
}

internal static void _pcache1EnforceMaxPage(ж<libc.TLS> tls, uintptr pCache) {
    uintptr p = default;
    uintptr pGroup = default;
    uintptr v1 = default;
    bool v2 = default;
    _ = p;
    _ = pGroup;
    _ = v1;
    _ = v2;
    pGroup = (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup;
    for (;;) {
        v2 = (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FnPurgeable > (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.FnMaxPage; if (v2) {
            v1 = (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)pGroup)).Value.Flru.FpLruPrev;
            p = v1;
        }
        if (!(v2 && libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)v1)).Value.FisAnchor) == 0)) {
            break;
        }
        _pcache1PinPage(ref (tls).DerefOrNull(), p);
        _pcache1RemoveFromHash(tls, p, (int32)1);
    }
    if ((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage == (uint32)0 && (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpBulk != 0) {
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpBulk);
        v1 = libc.UintptrFromInt32(0);
        (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpFree = v1;
        (@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpBulk = v1;
    }
}

internal static int32 /*r*/ _pcache1Pagecount(ж<libc.TLS> tls, uintptr p) {
    int32 n = default;
    uintptr pCache = default;
    _ = n;
    _ = pCache;
    pCache = p;
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup)).Value.Fmutex);
    n = libc.Int32FromUint32((@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FnPage);
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(@unsafe.Pointer.To<TPCache1>((@unsafe.Pointer)pCache)).Value.FpGroup)).Value.Fmutex);
    return n;
}

internal static void _pcacheManageDirtyList(ref libc.TLS tls, uintptr pPage, Tu8 addRemove) {
    uintptr p = default;
    _ = p;
    p = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpCache;
    if ((libc.Int32FromUint8(addRemove) & (int32)PCACHE_DIRTYLIST_REMOVE) != 0) {
        if ((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpSynced == pPage) {
            (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpSynced = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyPrev;
        }
        if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext != 0) {
            (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)(@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext)).Value.FpDirtyPrev = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyPrev;
        } else {
            (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpDirtyTail = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyPrev;
        }
        if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyPrev != 0) {
            (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)(@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyPrev)).Value.FpDirtyNext = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext;
        } else {
            (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpDirty = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext;
            if ((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpDirty == (uintptr)0) {
                (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FeCreate = (uint8)2;
            }
        }
    }
    if ((libc.Int32FromUint8(addRemove) & (int32)PCACHE_DIRTYLIST_ADD) != 0) {
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyPrev = (uintptr)0;
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpDirty;
        if ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext != 0) {
            (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)(@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.FpDirtyNext)).Value.FpDirtyPrev = pPage;
        } else {
            (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpDirtyTail = pPage;
            if ((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FbPurgeable != 0) {
                (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FeCreate = (uint8)1;
            }
        }
        (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpDirty = pPage;
        if (!((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpSynced != 0) && 0 == (libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPage)).Value.Fflags) & (int32)PGHDR_NEED_SYNC)) {
            (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)p)).Value.FpSynced = pPage;
        }
    }
}

internal static int32 _pgsz = (int32)4096;

internal static readonly ж<uintptr> Ꮡ_posixIoFinder = new StaticBox<uintptr>((uintptr)0);
internal static ref uintptr _posixIoFinder => ref ((StaticBox<uintptr>)Ꮡ_posixIoFinder).ValueSlot;

internal static uintptr /*r*/ _posixIoFinderImpl(ж<libc.TLS> tls, uintptr z, uintptr p) {
    _ = z;
    _ = p;
    return (uintptr)Ꮡ_posixIoMethods;
}

internal static readonly ж<Tsqlite3_io_methods> Ꮡ_posixIoMethods = new StaticBox<Tsqlite3_io_methods>(new Tsqlite3_io_methods(
    FiVersion: (int32)3
));
internal static ref Tsqlite3_io_methods _posixIoMethods => ref ((StaticBox<Tsqlite3_io_methods>)Ꮡ_posixIoMethods).ValueSlot;

internal static int32 /*r*/ _posixOpen(ж<libc.TLS> tls, uintptr zFile, int32 flags, int32 mode) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        r = libc.Xopen(tls, zFile, flags, libc.VaList(bp + 8, mode));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _pragmaVtabBestIndex(ж<libc.TLS> tls, uintptr tab, uintptr pIdxInfo) {
    int32 i = default;
    int32 j = default;
    uintptr pConstraint = default;
    uintptr pTab = default;
    array<int32> seen = new(2);
    _ = i;
    _ = j;
    _ = pConstraint;
    _ = pTab;
    _ = seen;
    pTab = tab;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = libc.Float64FromInt32(1);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPragmaVtab>((@unsafe.Pointer)pTab)).Value.FnHidden) == 0) {
        return SQLITE_OK;
    }
    pConstraint = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint;
    seen[0] = 0;
    seen[(int32)1] = 0;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
            break;
        }
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.FiColumn < libc.Int32FromUint8((@unsafe.Pointer.To<TPragmaVtab>((@unsafe.Pointer)pTab)).Value.FiHidden)) {
            goto _1;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.Fop) != (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
            goto _1;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.Fusable) == 0) {
            return (int32)SQLITE_CONSTRAINT;
        }
        j = (@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)pConstraint)).Value.FiColumn - libc.Int32FromUint8((@unsafe.Pointer.To<TPragmaVtab>((@unsafe.Pointer)pTab)).Value.FiHidden);
        seen[j] = i + (int32)1;
        goto _1;
_1:;
        i = i + 1;
        pConstraint += 12;
    }
    if (seen[0] == 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = libc.Float64FromInt32(2147483647);
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)2147483647;
        return SQLITE_OK;
    }
    j = seen[0] - (int32)1;
    (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)j * 8))).Value.FargvIndex = (int32)1;
    (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)j * 8))).Value.Fomit = (uint8)1;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = libc.Float64FromInt32(20);
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)20;
    if (seen[(int32)1] != 0) {
        j = seen[(int32)1] - (int32)1;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)j * 8))).Value.FargvIndex = (int32)2;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)j * 8))).Value.Fomit = (uint8)1;
    }
    return SQLITE_OK;
}

internal static uintptr /*r*/ _printfTempBuf(ж<libc.TLS> tls, uintptr pAccum, Tsqlite3_int64 n) {
    uintptr z = default;
    _ = z;
    if ((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pAccum)).Value.FaccError != 0) {
        return (uintptr)0;
    }
    if (n > libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pAccum)).Value.FnAlloc) && n > libc.Int64FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pAccum)).Value.FmxAlloc)) {
        _sqlite3StrAccumSetError(tls, (@unsafe.Pointer)pAccum, (uint8)SQLITE_TOOBIG);
        return (uintptr)0;
    }
    z = Xsqlite3_malloc(tls, (int32)n);
    if (z == (uintptr)0) {
        _sqlite3StrAccumSetError(tls, (@unsafe.Pointer)pAccum, (uint8)SQLITE_NOMEM);
    }
    return z;
}

internal static TPgno /*r*/ _ptrmapPageno(ref libc.TLS tls, uintptr pBt, TPgno pgno) {
    TPgno iPtrMap = default;
    TPgno ret = default;
    int32 nPagesPerMapPage = default;
    _ = iPtrMap;
    _ = nPagesPerMapPage;
    _ = ret;
    if (pgno < (uint32)2) {
        return (uint32)0;
    }
    nPagesPerMapPage = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FusableSize / (uint32)5 + (uint32)1);
    iPtrMap = (pgno - (uint32)2) / libc.Uint32FromInt32(nPagesPerMapPage);
    ret = iPtrMap * libc.Uint32FromInt32(nPagesPerMapPage) + (uint32)2;
    if (ret == libc.Uint32FromInt32(_sqlite3PendingByte) / (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpageSize + libc.Uint32FromInt32(1)) {
        ret = ret + 1;
    }
    return ret;
}

internal static int32 /*r*/ _rbuCaptureDbWrite(ref libc.TLS tls, uintptr pRbu, Ti64 iOff) {
    (@unsafe.Pointer.To<TRbuFrame>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FaFrame + (uintptr)((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FnFrame - (int32)1) * 8))).Value.FiDbPage = libc.Uint32FromInt64(quo(iOff, (int64)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.Fpgsz)) + (uint32)1;
    return SQLITE_OK;
}

internal static void _rbuCheckpointFrame(ж<libc.TLS> tls, uintptr p, uintptr pFrame) {
    Ti64 iOff = default;
    uintptr pDb = default;
    uintptr pWal = default;
    _ = iOff;
    _ = pDb;
    _ = pWal;
    pWal = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FpTargetFd)).Value.FpWalFd)).Value.FpReal;
    pDb = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FpTargetFd)).Value.FpReal;
    iOff = libc.Int64FromUint32((@unsafe.Pointer.To<TRbuFrame>((@unsafe.Pointer)pFrame)).Value.FiWalFrame - libc.Uint32FromInt32(1)) * (int64)((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz + libc.Int32FromInt32(24)) + (int64)32 + (int64)24;
    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Tsqlite3_int64, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pWal)).Value.FpMethods)).Value.FxRead)(tls, pWal, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FaBuf, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz, iOff);
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc != 0) {
        return;
    }
    iOff = libc.Int64FromUint32((@unsafe.Pointer.To<TRbuFrame>((@unsafe.Pointer)pFrame)).Value.FiDbPage - libc.Uint32FromInt32(1)) * (int64)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz;
    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, Tsqlite3_int64, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pDb)).Value.FpMethods)).Value.FxWrite)(tls, pDb, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FaBuf, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz, iOff);
}

internal static void _rbuPutU16(ref libc.TLS tls, uintptr aBuf, Tu16 iVal) {
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf))).Value = libc.Uint8FromInt32(libc.Int32FromUint16(iVal) >> (int)((int32)8) & (int32)0xFF);
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf + 1))).Value = libc.Uint8FromInt32(libc.Int32FromUint16(iVal) >> 0 & (int32)0xFF);
}

internal static Ti64 /*r*/ _rbuShmChecksum(ж<libc.TLS> tls, uintptr p) {
    Ti64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Ti64 iRet = default;
        uintptr pDb = default;
        uintptr _ᴛ1 = default;
        _ = iRet;
        _ = pDb;
        iRet = 0;
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            pDb = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FpTargetFd)).Value.FpReal;
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32, int32, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pDb)).Value.FpMethods)).Value.FxShmMap)(tls, pDb, 0, libc.Int32FromInt32(32) * libc.Int32FromInt32(1024), 0, bp);
            if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
                iRet = libc.Int64FromUint64(((uint64)libc.AtomicLoadPUint32(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) + libc.UintptrFromInt32(10) * 4)).Lsh((int64)(libc.Int32FromInt32(32))) + (uint64)libc.AtomicLoadPUint32(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) + 11 * 4));
            }
        }
        r = iRet;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _rbuUnlockShm(ж<libc.TLS> tls, uintptr p) {
    int32 i = default;
    ref var xShmLock = ref heap(new uintptr(), out var ᏑxShmLock);
    _ = i;
    _ = xShmLock;
    if ((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu != 0) {
        xShmLock = (@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal)).Value.FpMethods)).Value.FxShmLock;
        i = 0;
        for (;;) {
            if (!(i < (int32)SQLITE_SHM_NLOCK)) {
                break;
            }
            if ((libc.Uint32FromInt32(libc.Int32FromInt32(1).Lsh((int64)(i))) & (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu)).Value.FmLock) != 0) {
                GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32, int32, int32>>(xShmLock)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, i, (int32)1, libc.Int32FromInt32(SQLITE_SHM_UNLOCK) | libc.Int32FromInt32(SQLITE_SHM_EXCLUSIVE));
            }
            goto _1;
_1:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu)).Value.FmLock = (uint32)0;
    }
}

internal static int32 /*r*/ _readInt16(ref libc.TLS tls, uintptr p) {
    return (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))) << (int)((int32)8)) + libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1)));
}

internal static int32 /*r*/ _readsTable(ж<libc.TLS> tls, uintptr p, int32 iDb, uintptr pTab) {
    int32 i = default;
    int32 iEnd = default;
    uintptr pIndex = default;
    uintptr pOp = default;
    uintptr pVTab = default;
    uintptr v = default;
    uintptr v1 = default;
    TPgno tnum = default;
    _ = i;
    _ = iEnd;
    _ = pIndex;
    _ = pOp;
    _ = pVTab;
    _ = tnum;
    _ = v;
    _ = v1;
    v = _sqlite3GetVdbe(tls, p);
    iEnd = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
        v1 = _sqlite3GetVTable(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.Fdb, pTab);
    } else {
        v1 = (uintptr)0;
    }
    pVTab = v1;
    i = (int32)1;
    for (;;) {
        if (!(i < iEnd)) {
            break;
        }
        pOp = _sqlite3VdbeGetOp(ref (tls).DerefOrNull(), v, i);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_OpenRead && (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 == iDb) {
            tnum = libc.Uint32FromInt32((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp2);
            if (tnum == (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum) {
                return (int32)1;
            }
            pIndex = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpIndex;
            for (;;) {
                if (!(pIndex != 0)) {
                    break;
                }
                if (tnum == (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.Ftnum) {
                    return (int32)1;
                }
                goto _3;
_3:;
                pIndex = (@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIndex)).Value.FpNext;
            }
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_VOpen && ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pOp + 16)) == pVTab) {
            return (int32)1;
        }
        goto _2;
_2:;
        i = i + 1;
    }
    return 0;
}

internal static void _releaseInodeInfo(ж<libc.TLS> tls, uintptr pFile) {
    uintptr pInode = default;
    _ = pInode;
    pInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode;
    if (pInode != 0) {
        (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnRef = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnRef - 1;
        if ((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnRef == 0) {
            Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
            _closePendingFds(tls, pFile);
            Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
            if ((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpPrev != 0) {
                (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpPrev)).Value.FpNext = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext;
            } else {
                _inodeList = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext;
            }
            if ((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext != 0) {
                (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpNext)).Value.FpPrev = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpPrev;
            }
            Xsqlite3_mutex_free(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
            Xsqlite3_free(tls, pInode);
        }
    }
}

internal static int32 /*r*/ _renameColumnSelectCb(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SF_View) | libc.Int32FromInt32(SF_CopyCte))) != 0) {
        return (int32)WRC_Prune;
    }
    _renameWalkWith(tls, pWalker, p);
    return WRC_Continue;
}

internal static int32 /*r*/ _resolveAttachExpr(ж<libc.TLS> tls, uintptr pName, uintptr pExpr) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if (pExpr != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) != (int32)TK_ID) {
            rc = _sqlite3ResolveExprNames(tls, pName, pExpr);
        } else {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop = (uint8)TK_STRING;
        }
    }
    return rc;
}

internal static int32 /*r*/ _rtreeClose(ж<libc.TLS> tls, uintptr cur) {
    uintptr pCsr = default;
    uintptr pRtree = default;
    _ = pCsr;
    _ = pRtree;
    pRtree = (@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)cur)).Value.FpVtab;
    pCsr = cur;
    _resetCursor(tls, pCsr);
    Xsqlite3_finalize(tls, (@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FpReadAux);
    Xsqlite3_free(tls, pCsr);
    (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnCursor = (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnCursor - 1;
    if ((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnCursor == (uint32)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FinWrTrans) == 0) {
        _nodeBlobReset(tls, pRtree);
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _rtreeEof(ж<libc.TLS> tls, uintptr cur) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = cur;
    return libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeCursor>((@unsafe.Pointer)pCsr)).Value.FatEOF);
}

internal static int32 /*r*/ _rtreeSearchPointCompare(ref libc.TLS tls, uintptr pA, uintptr pB) {
    if ((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pA)).Value.FrScore < (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pB)).Value.FrScore) {
        return -(int32)1;
    }
    if ((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pA)).Value.FrScore > (@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pB)).Value.FrScore) {
        return +libc.Int32FromInt32(1);
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pA)).Value.FiLevel) < libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pB)).Value.FiLevel)) {
        return -(int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pA)).Value.FiLevel) > libc.Int32FromUint8((@unsafe.Pointer.To<TRtreeSearchPoint>((@unsafe.Pointer)pB)).Value.FiLevel)) {
        return +libc.Int32FromInt32(1);
    }
    return 0;
}

internal static int32 /*r*/ _rtrimCollFunc(ж<libc.TLS> tls, uintptr pUser, int32 nKey1, uintptr pKey1, int32 nKey2, uintptr pKey2) {
    uintptr pK1 = default;
    uintptr pK2 = default;
    _ = pK1;
    _ = pK2;
    pK1 = pKey1;
    pK2 = pKey2;
    while (nKey1 != 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pK1 + (uintptr)(nKey1 - (int32)1)))) == (int32)' ') {
        nKey1 = nKey1 - 1;
    }
    while (nKey2 != 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pK2 + (uintptr)(nKey2 - (int32)1)))) == (int32)' ') {
        nKey2 = nKey2 - 1;
    }
    return _sqlite3BinaryCompare(tls, pUser, nKey1, pKey1, nKey2, pKey2);
}

internal static int32 /*r*/ _saveCursorPosition(ж<libc.TLS> tls, uintptr pCur) {
    int32 rc = default;
    uintptr v1 = default;
    _ = rc;
    _ = v1;
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_Pinned) != 0) {
        return libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(11).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == (int32)CURSOR_SKIPNEXT) {
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_VALID;
    } else {
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FskipNext = 0;
    }
    rc = _saveCursorKey(tls, pCur);
    if (rc == SQLITE_OK) {
        _btreeReleaseAllCursorPages(tls, pCur);
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState = (uint8)CURSOR_REQUIRESEEK;
    }
    v1 = pCur + 1;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTCF_ValidNKey) | libc.Int32FromInt32(BTCF_ValidOvfl) | libc.Int32FromInt32(BTCF_AtLast)));
    return rc;
}

internal static int32 /*r*/ _saveCursorsOnList(ж<libc.TLS> tls, uintptr p, TPgno iRoot, uintptr pExcept) {
    int32 rc = default;
    _ = rc;
    for (var cond = true; cond; cond = p != 0) {
        if (p != pExcept && ((uint32)0 == iRoot || (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FpgnoRoot == iRoot)) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FeState) == CURSOR_VALID || libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FeState) == (int32)CURSOR_SKIPNEXT) {
                rc = _saveCursorPosition(tls, p);
                if (SQLITE_OK != rc) {
                    return rc;
                }
            } else {
                _btreeReleaseAllCursorPages(tls, p);
            }
        }
        p = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FpNext;
    }
    return SQLITE_OK;
}

internal static void _sessionAppendCol(ж<libc.TLS> tls, uintptr p, uintptr pStmt, int32 iCol, @unsafe.Pointer pRc) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 eType = default;
        int32 nByte = default;
        Tsqlite3_int64 i = default;
        float64 r = default;
        uintptr z = default;
        array<Tu8> _ᴛ1 = new(8);
        _ = eType;
        _ = i;
        _ = nByte;
        _ = r;
        _ = z;
        if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
            eType = Xsqlite3_column_type(tls, pStmt, iCol);
            _sessionAppendByte(tls, (@unsafe.Pointer)p, libc.Uint8FromInt32(eType), pRc);
            if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                if (eType == (int32)SQLITE_INTEGER) {
                    i = Xsqlite3_column_int64(tls, pStmt, iCol);
                    _sessionPutI64(ref (tls).DerefOrNull(), __ccgo_ptr(bp), i);
                } else {
                    r = Xsqlite3_column_double(tls, pStmt, iCol);
                    _sessionPutDouble(tls, __ccgo_ptr(bp), r);
                }
                _sessionAppendBlob(tls, p, bp, (int32)8, pRc);
            }
            if (eType == (int32)SQLITE_BLOB || eType == (int32)SQLITE_TEXT) {
                if (eType == (int32)SQLITE_BLOB) {
                    z = Xsqlite3_column_blob(tls, pStmt, iCol);
                } else {
                    z = Xsqlite3_column_text(tls, pStmt, iCol);
                }
                nByte = Xsqlite3_column_bytes(tls, pStmt, iCol);
                if (z != 0 || eType == (int32)SQLITE_BLOB && nByte == 0) {
                    _sessionAppendVarint(tls, (@unsafe.Pointer)p, nByte, pRc);
                    _sessionAppendBlob(tls, p, z, nByte, pRc);
                } else {
                    (@unsafe.Pointer.To<int32>(pRc)).Value = (int32)SQLITE_NOMEM;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uint32 /*r*/ _sessionChangeHash(ref libc.TLS tls, uintptr pTab, int32 bPkOnly, uintptr aRecord, int32 nBucket) {
    uintptr a = default;
    uintptr v2 = default;
    int32 eType = default;
    int32 i = default;
    int32 isPK = default;
    ref var n = ref heap(new int32(), out var Ꮡn);
    uint32 h = default;
    _ = a;
    _ = eType;
    _ = h;
    _ = i;
    _ = isPK;
    _ = n;
    _ = v2;
    h = (uint32)0;
    a = aRecord;
    i = 0;
    for (;;) {
        if (!(i < (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol)) {
            break;
        }
        isPK = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK + (uintptr)i)));
        if (bPkOnly != 0 && isPK == 0) {
            goto _1;
        }
        if (isPK != 0) {
            v2 = a;
            a = a + 1;
            eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2)));
            h = _sessionHashAppendType(ref tls, h, eType);
            if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                h = _sessionHashAppendI64(ref tls, h, _sessionGetI64(ref tls, a));
                a = a + (uintptr)8;
            } else {
                if (eType == (int32)SQLITE_TEXT || eType == (int32)SQLITE_BLOB) {
                    a = a + (uintptr)_sessionVarintGet(ref tls, a, @unsafe.Pointer.FromPinnedBox(Ꮡn));
                    h = _sessionHashAppendBlob(ref tls, h, n, a);
                    a = a + (uintptr)n;
                }
            }
        } else {
            a = a + (uintptr)_sessionSerialLen(ref tls, a);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return h % libc.Uint32FromInt32(nBucket);
}

internal static int32 /*r*/ _sessionChangesetBufferRecord(ж<libc.TLS> tls, uintptr pIn, int32 nCol, @unsafe.Pointer pnByte) {
    int32 eType = default;
    int32 i = default;
    ref var n = ref heap(new int32(), out var Ꮡn);
    int32 nRem = default;
    int32 rc = default;
    Ti64 nByte = default;
    Ti64 v2 = default;
    _ = eType;
    _ = i;
    _ = n;
    _ = nByte;
    _ = nRem;
    _ = rc;
    _ = v2;
    rc = SQLITE_OK;
    nByte = 0;
    i = 0;
    for (;;) {
        if (!(rc == SQLITE_OK && i < nCol)) {
            break;
        }
        rc = _sessionInputBuffer(tls, pIn, (int32)(nByte + (int64)10));
        if (rc == SQLITE_OK) {
            if ((int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext + nByte >= (int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData) {
                rc = _sqlite3CorruptError(tls, (int32)239209);
            } else {
                v2 = nByte;
                nByte = nByte + 1;
                eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FaData + (uintptr)((int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext + v2))));
                if (eType == (int32)SQLITE_TEXT || eType == (int32)SQLITE_BLOB) {
                    nRem = (int32)((int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData - ((int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext + nByte));
                    nByte = nByte + (int64)_sessionVarintGetSafe(tls, (@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FaData + (uintptr)((int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext + nByte), nRem, @unsafe.Pointer.FromPinnedBox(Ꮡn));
                    nByte = nByte + (int64)n;
                    rc = _sessionInputBuffer(tls, pIn, (int32)nByte);
                } else {
                    if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                        nByte = nByte + (int64)8;
                    } else {
                        if (eType != 0 && eType != (int32)SQLITE_NULL) {
                            rc = _sqlite3CorruptError(tls, (int32)239221);
                        }
                    }
                }
            }
        }
        if (rc == SQLITE_OK && (int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FiNext + nByte > (int64)(@unsafe.Pointer.To<TSessionInput>((@unsafe.Pointer)pIn)).Value.FnData) {
            rc = _sqlite3CorruptError(tls, (int32)239226);
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<int32>(pnByte)).Value = (int32)nByte;
    return rc;
}

internal static Tsqlite3_int64 /*r*/ _sessionGetI64(ref libc.TLS tls, uintptr aRec) {
    Tu64 x = default;
    Tu32 y = default;
    _ = x;
    _ = y;
    x = (uint64)(((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec)))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + 3))));
    y = ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + libc.UintptrFromInt32(4))))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + libc.UintptrFromInt32(4) + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + libc.UintptrFromInt32(4) + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aRec + libc.UintptrFromInt32(4) + 3)));
    x = (x << (int)(libc.Int32FromInt32(32))) + (uint64)y;
    return libc.Int64FromUint64(x);
}

internal static uint32 /*r*/ _sessionHashAppendI64(ref libc.TLS tls, uint32 h, Ti64 i) {
    h = h << (int)((int32)3) ^ h ^ libc.Uint32FromInt64((Ti64)(i & libc.Int64FromUint32(0xFFFFFFFFU)));
    return h << (int)((int32)3) ^ h ^ libc.Uint32FromInt64((Ti64)(i >> (int)(libc.Int32FromInt32(32)) & libc.Int64FromUint32(0xFFFFFFFFU)));
}

internal static uint32 /*r*/ _sessionHashAppendType(ref libc.TLS tls, uint32 h, int32 eType) {
    return h << (int)((int32)3) ^ h ^ libc.Uint32FromInt32(eType);
}

internal static void _sessionPutI64(ref libc.TLS tls, @unsafe.Pointer aBuf, Tsqlite3_int64 i) {
    (@unsafe.Pointer.To<Tu8>(aBuf)).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(56)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 1))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(48)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 2))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(40)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 3))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(32)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 4))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(24)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 5))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(16)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 6))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(8)) & (int64)0xFF));
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 7))).Value = libc.Uint8FromInt64((Tsqlite3_int64)(i >> (int)(libc.Int32FromInt32(0)) & (int64)0xFF));
}

internal static int32 /*r*/ _sessionSeekToRow(ж<libc.TLS> tls, uintptr pIter, uintptr p) {
    int32 ii = default;
    ref var nCol = ref heap(new int32(), out var ᏑnCol);
    ref var op = ref heap(new int32(), out var Ꮡop);
    int32 rc = default;
    uintptr pSelect = default;
    ref var pVal = ref heap(new uintptr(), out var ᏑpVal);
    ref var zDummy = ref heap(new uintptr(), out var ᏑzDummy);
    uintptr v1 = default;
    _ = ii;
    _ = nCol;
    _ = op;
    _ = pSelect;
    _ = pVal;
    _ = rc;
    _ = zDummy;
    _ = v1;
    pSelect = (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FpSelect;
    Xsqlite3_clear_bindings(tls, pSelect);
    _sqlite3changeset_op__ccgo_gp(ref (tls).DerefOrNull(), pIter, @unsafe.Pointer.FromBox(ᏑzDummy), @unsafe.Pointer.FromPinnedBox(ᏑnCol), @unsafe.Pointer.FromPinnedBox(Ꮡop), nil);
    if (op == (int32)SQLITE_INSERT) {
        v1 = __ccgo_fp(Xsqlite3changeset_new);
    } else {
        v1 = __ccgo_fp(Xsqlite3changeset_old);
    }
    rc = _sessionBindRow(tls, pIter, v1, nCol, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FabPK, pSelect);
    if (op != (int32)SQLITE_DELETE && (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FbIgnoreNoop != 0) {
        ii = 0;
        for (;;) {
            if (!(rc == SQLITE_OK && ii < nCol)) {
                break;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)p)).Value.FabPK + (uintptr)ii))) == 0) {
                pVal = (uintptr)0;
                _sqlite3changeset_new__ccgo_gp(ref (tls).DerefOrNull(), pIter, ii, @unsafe.Pointer.FromBox(ᏑpVal));
                Xsqlite3_bind_int(tls, pSelect, ii + (int32)1 + nCol, libc.BoolInt32(pVal == (uintptr)0));
                if (pVal != 0) {
                    rc = _sessionBindValue(tls, pSelect, ii + (int32)1, pVal);
                }
            }
            goto _2;
_2:;
            ii = ii + 1;
        }
    }
    if (rc == SQLITE_OK) {
        rc = Xsqlite3_step(tls, pSelect);
        if (rc != (int32)SQLITE_ROW) {
            rc = Xsqlite3_reset(tls, pSelect);
        }
    }
    return rc;
}

internal static int32 /*r*/ _sessionSerialLen(ref libc.TLS tls, uintptr a) {
    int32 e = default;
    ref var n = ref heap(new int32(), out var Ꮡn);
    _ = e;
    _ = n;
    e = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a)));
    if (e == (int32)SQLITE_INTEGER || e == (int32)SQLITE_FLOAT) {
        return (int32)9;
    }
    if (e == (int32)SQLITE_TEXT || e == (int32)SQLITE_BLOB) {
        return _sessionVarintGet(ref tls, a + 1, @unsafe.Pointer.FromPinnedBox(Ꮡn)) + (int32)1 + n;
    }
    return (int32)1;
}

internal static void _sessionSkipRecord(ref libc.TLS tls, @unsafe.Pointer ppRec, int32 nCol) {
    uintptr aRec = default;
    uintptr v2 = default;
    int32 eType = default;
    int32 i = default;
    ref var nByte = ref heap(new int32(), out var ᏑnByte);
    _ = aRec;
    _ = eType;
    _ = i;
    _ = nByte;
    _ = v2;
    aRec = ~@unsafe.Pointer.To<uintptr>(ppRec);
    i = 0;
    for (;;) {
        if (!(i < nCol)) {
            break;
        }
        v2 = aRec;
        aRec = aRec + 1;
        eType = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2)));
        if (eType == (int32)SQLITE_TEXT || eType == (int32)SQLITE_BLOB) {
            aRec = aRec + (uintptr)_sessionVarintGet(ref tls, aRec, @unsafe.Pointer.FromPinnedBox(ᏑnByte));
            aRec = aRec + (uintptr)nByte;
        } else {
            if (eType == (int32)SQLITE_INTEGER || eType == (int32)SQLITE_FLOAT) {
                aRec = aRec + (uintptr)8;
            }
        }
        goto _1;
_1:;
        i = i + 1;
    }
    (@unsafe.Pointer.To<uintptr>(ppRec)).Value = aRec;
}

internal static int32 /*r*/ _sessionVarintGet(ref libc.TLS tls, uintptr aBuf, @unsafe.Pointer piVal) {
    int32 ret = default;
    int32 v1 = default;
    _ = ret;
    _ = v1;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
        (@unsafe.Pointer.To<int32>(piVal)).Value = libc.Int32FromUint32((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf))));
        v1 = libc.Int32FromInt32(1);
    } else {
        v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref tls, aBuf, piVal));
    }
    ret = libc.Int32FromUint8(libc.Uint8FromInt32(v1));
    (@unsafe.Pointer.To<int32>(piVal)).Value = ~@unsafe.Pointer.To<int32>(piVal) & (int32)0x7FFFFFFF;
    return ret;
}

internal static int32 /*r*/ _sessionVarintLen(ref libc.TLS tls, int32 iVal) {
    return _sqlite3VarintLen(ref tls, libc.Uint64FromInt32(iVal));
}

internal static int32 /*r*/ _sessionVarintPut(ж<libc.TLS> tls, @unsafe.Pointer aBuf, int32 iVal) {
    int32 v1 = default;
    _ = v1;
    if (libc.Uint32FromInt32(iVal) < libc.Uint32FromInt32(0x80)) {
        (@unsafe.Pointer.To<Tu8>(aBuf)).Value = libc.Uint8FromInt32(iVal);
        v1 = libc.Int32FromInt32(1);
    } else {
        v1 = _sqlite3PutVarint(tls, aBuf, libc.Uint64FromInt32(iVal));
    }
    return libc.Int32FromUint8(libc.Uint8FromInt32(v1));
}

internal static void _setDoNotMergeFlagOnCopy(ref libc.TLS tls, uintptr v) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)_sqlite3VdbeGetLastOp(ref tls, v))).Value.Fopcode) == (int32)OP_Copy) {
        _sqlite3VdbeChangeP5(ref tls, v, (uint16)1);
    }
}

internal static void _setPendingFd(ref libc.TLS tls, uintptr pFile) {
    uintptr p = default;
    uintptr pInode = default;
    _ = p;
    _ = pInode;
    pInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode;
    p = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpPreallocatedUnused;
    (@unsafe.Pointer.To<TUnixUnusedFd>((@unsafe.Pointer)p)).Value.FpNext = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpUnused;
    (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpUnused = p;
    (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh = -(int32)1;
    (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpPreallocatedUnused = (uintptr)0;
}

internal static void _setPragmaResultColumnNames(ж<libc.TLS> tls, uintptr v, uintptr pPragma) {
    int32 i = default;
    int32 j = default;
    int32 v1 = default;
    Tu8 n = default;
    _ = i;
    _ = j;
    _ = n;
    _ = v1;
    n = (@unsafe.Pointer.To<TPragmaName>((@unsafe.Pointer)pPragma)).Value.FnPragCName;
    if (libc.Int32FromUint8(n) == 0) {
        v1 = (int32)1;
    } else {
        v1 = libc.Int32FromUint8(n);
    }
    _sqlite3VdbeSetNumCols(tls, v, v1);
    if (libc.Int32FromUint8(n) == 0) {
        _sqlite3VdbeSetColName(tls, v, 0, COLNAME_NAME, (@unsafe.Pointer.To<TPragmaName>((@unsafe.Pointer)pPragma)).Value.FzName, libc.UintptrFromInt32(0));
    } else {
        i = 0;
        j = libc.Int32FromUint8((@unsafe.Pointer.To<TPragmaName>((@unsafe.Pointer)pPragma)).Value.FiPragCName);
        for (;;) {
            if (!(i < libc.Int32FromUint8(n))) {
                break;
            }
            _sqlite3VdbeSetColName(tls, v, i, COLNAME_NAME, _pragCName[j], libc.UintptrFromInt32(0));
            goto _2;
_2:;
            i = i + 1;
            j = j + 1;
        }
    }
}

internal static void _setSectorSize(ж<libc.TLS> tls, uintptr pPager) {
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0 || (_sqlite3OsDeviceCharacteristics(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd) & (int32)SQLITE_IOCAP_POWERSAFE_OVERWRITE) != 0) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize = (uint32)512;
    } else {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize = libc.Uint32FromInt32(_sqlite3SectorSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd));
    }
}

internal static int32 /*r*/ _sqlite3AtoF(ж<libc.TLS> tls, uintptr zIn, @unsafe.Pointer pResult) {
    int32 d = default;
    int32 esign = default;
    int32 exp = default;
    int32 mState = default;
    int32 neg = default;
    Tu64 s = default;
    uint32 v = default;
    uint32 v3 = default;
    uint32 v4 = default;
    uintptr z = default;
    uintptr v7 = default;
    _ = d;
    _ = esign;
    _ = exp;
    _ = mState;
    _ = neg;
    _ = s;
    _ = v;
    _ = z;
    _ = v3;
    _ = v4;
    _ = v7;
    z = zIn;
    neg = 0;
    s = (uint64)0;
    d = 0;
    mState = 0;
    goto start_of_text;
start_of_text:;
    v3 = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - libc.Uint32FromUint8((uint8)'0');
    v = v3;
    if (!(v3 < (uint32)10)) {
        goto _1;
    }
    goto parse_integer_part;
parse_integer_part:;
    mState = (int32)1;
    s = (uint64)v;
    z = z + 1;
    for (;;) {
        v3 = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - libc.Uint32FromUint8((uint8)'0');
        v = v3;
        if (!(v3 < (uint32)10)) {
            break;
        }
        s = s * (uint64)10 + (uint64)v;
        z = z + 1;
        if (s >= ((libc.Uint64FromUint32(0xffffffffU) | libc.Uint64FromUint32(0xffffffffU).Lsh((int64)(libc.Int32FromInt32(32)))) - libc.Uint64FromInt32(9)) / libc.Uint64FromInt32(10)) {
            mState = (int32)9;
            while ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]) & (int32)0x04) != 0) {
                z = z + 1;
                d = d + 1;
            }
            break;
        }
    }
    goto _2;
_1:;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'-') {
        neg = (int32)1;
        z = z + 1;
        v3 = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - libc.Uint32FromUint8((uint8)'0');
        v = v3;
        if (v3 < (uint32)10) {
            goto parse_integer_part;
        }
    } else {
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'+') {
            z = z + 1;
            v3 = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - libc.Uint32FromUint8((uint8)'0');
            v = v3;
            if (v3 < (uint32)10) {
                goto parse_integer_part;
            }
        } else {
            if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]) & (int32)0x01) != 0) {
                for (var cond = true; cond; cond = (libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]) & (int32)0x01) != 0) {
                    z = z + 1;
                }
                goto start_of_text;
            } else {
                s = (uint64)0;
            }
        }
    }
_2:;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'.') {
        z = z + 1;
        if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]) & (int32)0x04) != 0) {
            mState = mState | (int32)1;
            for (;;) {
                if (s < ((libc.Uint64FromUint32(0xffffffffU) | libc.Uint64FromUint32(0xffffffffU).Lsh((int64)(libc.Int32FromInt32(32)))) - libc.Uint64FromInt32(9)) / libc.Uint64FromInt32(10)) {
                    s = s * (uint64)10 + (uint64)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - (uint64)'0';
                    d = d - 1;
                } else {
                    mState = (int32)11;
                }
                goto _8;
_8:;
                z = z + 1;
                v7 = z;
                if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v7))]) & (int32)0x04) != 0)) {
                    break;
                }
            }
        } else {
            if (mState == 0) {
                (@unsafe.Pointer.To<float64>(pResult)).Value = (float64)0D;
                return 0;
            }
        }
        mState = mState | (int32)2;
    } else {
        if (mState == 0) {
            (@unsafe.Pointer.To<float64>(pResult)).Value = (float64)0D;
            return 0;
        }
    }
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'e' || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'E') {
        z = z + 1;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'-') {
            esign = -(int32)1;
            z = z + 1;
        } else {
            esign = +libc.Int32FromInt32(1);
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'+') {
                z = z + 1;
            }
        }
        v3 = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - libc.Uint32FromUint8((uint8)'0');
        v = v3;
        if (v3 < (uint32)10) {
            exp = libc.Int32FromUint32(v);
            z = z + 1;
            mState = mState | (int32)2;
            for (;;) {
                v3 = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) - libc.Uint32FromUint8((uint8)'0');
                v = v3;
                if (!(v3 < (uint32)10)) {
                    break;
                }
                if (exp < (int32)10000) {
                    v4 = libc.Uint32FromInt32(exp * (int32)10) + v;
                } else {
                    v4 = (uint32)10000;
                }
                exp = libc.Int32FromUint32(v4);
                z = z + 1;
            }
            d = d + esign * exp;
        } else {
            z = z - 1;
        }
    }
    if (s == (uint64)0) {
        (@unsafe.Pointer.To<float64>(pResult)).Value = (float64)0D;
        mState = mState | (int32)4;
    } else {
        (@unsafe.Pointer.To<float64>(pResult)).Value = _sqlite3Fp10Convert2(tls, s, d);
    }
    if (neg != 0) {
        (@unsafe.Pointer.To<float64>(pResult)).Value = -~@unsafe.Pointer.To<float64>(pResult);
    }
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == 0) {
        return mState;
    }
    if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]) & (int32)0x01) != 0) {
        for (var cond = true; cond; cond = (libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]) & (int32)0x01) != 0) {
            z = z + 1;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == 0) {
            return mState;
        }
    }
    return libc.Int32FromUint32((uint32)0xfffffff0U | libc.Uint32FromInt32(mState));
}

internal static int32 /*r*/ _sqlite3AuthCheck(ж<libc.TLS> tls, uintptr pParse, int32 code, uintptr zArg1, uintptr zArg2, uintptr zArg3) {
    int32 r = default;

    if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FxAuth != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Finit1.Fbusy) == 0) {
        return _realAuthCheck(tls, pParse, code, zArg1, zArg2, zArg3);
    } else {
        return SQLITE_OK;
    }
    return r;
}

internal static int32 /*r*/ _sqlite3BitvecBuiltinTest(ж<libc.TLS> tls, int32 sz, uintptr aOp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 nx = default;
        int32 op = default;
        int32 pc = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr pBitvec = default;
        uintptr pTmpSpace = default;
        uintptr pV = default;
        uintptr v4 = default;
        int32 _ᴛ1 = default;
        _ = nx;
        _ = op;
        _ = pBitvec;
        _ = pTmpSpace;
        _ = pV;
        _ = pc;
        _ = rc;
        _ = v1;
        _ = v4;
        pBitvec = (uintptr)0;
        pV = (uintptr)0;
        rc = -(int32)1;
        if (sz <= 0) {
            pBitvec = _sqlite3BitvecCreate(tls, (uint32)2 * libc.Uint32FromInt32(-sz));
            pV = (uintptr)0;
        } else {
            pBitvec = _sqlite3BitvecCreate(tls, libc.Uint32FromInt32(sz));
            pV = _sqlite3MallocZero(tls, libc.Uint64FromInt64(((int64)7 + (int64)sz) / (int64)8 + (int64)1));
        }
        pTmpSpace = Xsqlite3_malloc64(tls, (uint64)BITVEC_SZ);
        if (pBitvec == (uintptr)0 || pTmpSpace == (uintptr)0 || pV == (uintptr)0 && sz > 0) {
            goto bitvec_end;
        }
        _sqlite3BitvecSet(tls, (uintptr)0, (uint32)1);
        _sqlite3BitvecClear(tls, (uintptr)0, (uint32)1, pTmpSpace);
        v1 = libc.Int32FromInt32(0);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = v1;
        pc = v1;
        for (;;) {
            v1 = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aOp + (uintptr)pc * 4));
            op = v1;
            if (!(v1 != 0)) {
                break;
            }
            if (op >= (int32)6) {
                pc = pc + 1;
                continue;
            }
            var matchᴛ1 = false;
            if (op == (int32)1) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && op == (int32)2) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && op == (int32)5) { matchᴛ1 = true;
                nx = (int32)4;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(aOp + (uintptr)(pc + (int32)2) * 4)) - (int32)1;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(aOp + (uintptr)(pc + (int32)2) * 4))).Value += (@unsafe.Pointer.To<int32>(__ccgo_ptr(aOp + (uintptr)(pc + (int32)3) * 4))).Value;
            }
            else if (op == (int32)3) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && op == (int32)4) { matchᴛ1 = true;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                do {
                    nx = (int32)2;
                    Xsqlite3_randomness(tls, (int32)4, bp);
                    break;
                } while (false);
            }

            v4 = aOp + (uintptr)(pc + (int32)1) * 4;
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4)).Value = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4) - 1;
            v1 = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)v4);
            if (v1 > 0) {
                nx = 0;
            }
            pc = pc + nx;
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = rem((~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) & (int32)0x7fffffff), sz);
            if ((op & (int32)1) != 0) {
                if (pV != 0) {
                    v4 = pV + (uintptr)((~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + (int32)1) >> (int)((int32)3));
                    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v4)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v4)) | libc.Int32FromInt32(1).Lsh((int64)((~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + libc.Int32FromInt32(1)) & libc.Int32FromInt32(7))));
                }
                if (op != (int32)5) {
                    if (_sqlite3BitvecSet(tls, pBitvec, libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + (int32)1)) != 0) {
                        goto bitvec_end;
                    }
                }
            } else {
                if (pV != 0) {
                    v4 = pV + (uintptr)((~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + (int32)1) >> (int)((int32)3));
                    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v4)).Value = (uint8)((int32)(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)v4)) & ~libc.Int32FromUint8(libc.Uint8FromInt32(libc.Int32FromInt32(1).Lsh((int64)((~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + libc.Int32FromInt32(1)) & libc.Int32FromInt32(7))))));
                }
                _sqlite3BitvecClear(tls, pBitvec, libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + (int32)1), pTmpSpace);
            }
        }
        if (pV != 0) {
            rc = libc.Int32FromUint32(libc.Uint32FromInt32(_sqlite3BitvecTest(ref (tls).DerefOrNull(), (uintptr)0, (uint32)0) + _sqlite3BitvecTest(ref (tls).DerefOrNull(), pBitvec, libc.Uint32FromInt32(sz + (int32)1)) + _sqlite3BitvecTest(ref (tls).DerefOrNull(), pBitvec, (uint32)0)) + (_sqlite3BitvecSize(ref (tls).DerefOrNull(), pBitvec) - libc.Uint32FromInt32(sz)));
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = (int32)1;
            for (;;) {
                if (!(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) <= sz)) {
                    break;
                }
                if (libc.BoolInt32((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(pV + (uintptr)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) >> (int)((int32)3))))) & ((int32)1 << (int)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) & (int32)7))) != 0) != _sqlite3BitvecTest(ref (tls).DerefOrNull(), pBitvec, libc.Uint32FromInt32(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))))) {
                    rc = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp));
                    break;
                }
                goto _7;
_7:;
                (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) + 1;
            }
        } else {
            rc = 0;
        }
        goto bitvec_end;
bitvec_end:;
        Xsqlite3_free(tls, pTmpSpace);
        Xsqlite3_free(tls, pV);
        _sqlite3BitvecDestroy(tls, pBitvec);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3BtreeCheckpoint(ж<libc.TLS> tls, uintptr p, int32 eMode, uintptr pnLog, uintptr pnCkpt) {
    uintptr pBt = default;
    int32 rc = default;
    _ = pBt;
    _ = rc;
    rc = SQLITE_OK;
    if (p != 0) {
        pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
        _sqlite3BtreeEnter(tls, p);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction) != TRANS_NONE) {
            rc = (int32)SQLITE_LOCKED;
        } else {
            rc = _sqlite3PagerCheckpoint(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fdb, eMode, pnLog, pnCkpt);
        }
        _sqlite3BtreeLeave(tls, p);
    }
    return rc;
}

internal static void _sqlite3BtreeClearCache(ж<libc.TLS> tls, uintptr p) {
    uintptr pBt = default;
    _ = pBt;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction) == TRANS_NONE) {
        _sqlite3PagerClearCache(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager);
    }
}

internal static int32 /*r*/ _sqlite3BtreeClearTable(ж<libc.TLS> tls, uintptr p, int32 iTable, @unsafe.Pointer pnChange) {
    uintptr pBt = default;
    int32 rc = default;
    _ = pBt;
    _ = rc;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    rc = _saveAllCursors(tls, pBt, libc.Uint32FromInt32(iTable), (uintptr)0);
    if (SQLITE_OK == rc) {
        if ((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FhasIncrblobCur != 0) {
            _invalidateIncrblobCursors(ref (tls).DerefOrNull(), p, libc.Uint32FromInt32(iTable), 0, (int32)1);
        }
        rc = _clearDatabasePage(tls, pBt, libc.Uint32FromInt32(iTable), 0, pnChange);
    }
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeClearTableOfCursor(ж<libc.TLS> tls, uintptr pCur) {
    return _sqlite3BtreeClearTable(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBtree, libc.Int32FromUint32((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpgnoRoot), nil);
}

internal static int32 /*r*/ _sqlite3BtreeCloseCursor(ж<libc.TLS> tls, uintptr pCur) {
    uintptr pBt = default;
    uintptr pBtree = default;
    uintptr pPrev = default;
    _ = pBt;
    _ = pBtree;
    _ = pPrev;
    pBtree = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBtree;
    if (pBtree != 0) {
        pBt = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt;
        _sqlite3BtreeEnter(tls, pBtree);
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpCursor == pCur) {
            (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpCursor = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpNext;
        } else {
            pPrev = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpCursor;
            for (var cond = true; cond; cond = pPrev != 0) {
                if ((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pPrev)).Value.FpNext == pCur) {
                    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pPrev)).Value.FpNext = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpNext;
                    break;
                }
                pPrev = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pPrev)).Value.FpNext;
            }
        }
        _btreeReleaseAllCursorPages(tls, pCur);
        _unlockBtreeIfUnused(tls, pBt);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FaOverflow);
        Xsqlite3_free(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpKey);
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FopenFlags) & (int32)BTREE_SINGLE) != 0 && (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpCursor == (uintptr)0) {
            _sqlite3BtreeClose(tls, pBtree);
        } else {
            _sqlite3BtreeLeave(tls, pBtree);
        }
        (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBtree = (uintptr)0;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3BtreeCommitPhaseOne(ж<libc.TLS> tls, uintptr p, uintptr zSuperJrnl) {
    uintptr pBt = default;
    int32 rc = default;
    _ = pBt;
    _ = rc;
    rc = SQLITE_OK;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_WRITE) {
        pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
        _sqlite3BtreeEnter(tls, p);
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum != 0) {
            rc = _autoVacuumCommit(tls, p);
            if (rc != SQLITE_OK) {
                _sqlite3BtreeLeave(tls, p);
                return rc;
            }
        }
        if ((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbDoTruncate != 0) {
            _sqlite3PagerTruncateImage(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage);
        }
        rc = _sqlite3PagerCommitPhaseOne(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, zSuperJrnl, 0);
        _sqlite3BtreeLeave(tls, p);
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeCommitPhaseTwo(ж<libc.TLS> tls, uintptr p, int32 bCleanup) {
    uintptr pBt = default;
    int32 rc = default;
    _ = pBt;
    _ = rc;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == TRANS_NONE) {
        return SQLITE_OK;
    }
    _sqlite3BtreeEnter(tls, p);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_WRITE) {
        pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
        rc = _sqlite3PagerCommitPhaseTwo(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager);
        if (rc != SQLITE_OK && bCleanup == 0) {
            _sqlite3BtreeLeave(tls, p);
            return rc;
        }
        (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FiBDataVersion = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FiBDataVersion - 1;
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction = (uint8)TRANS_READ;
        _btreeClearHasContent(tls, pBt);
    }
    _btreeEndTransaction(tls, p);
    _sqlite3BtreeLeave(tls, p);
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3BtreeCursorHasMoved(ref libc.TLS tls, uintptr pCur) {
    return libc.BoolInt32(CURSOR_VALID != libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(pCur))));
}

internal static int32 /*r*/ _sqlite3BtreeCursorIsValidNN(ref libc.TLS tls, uintptr pCur) {
    return libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == CURSOR_VALID);
}

internal static int32 /*r*/ _sqlite3BtreeCursorRestore(ж<libc.TLS> tls, uintptr pCur, @unsafe.Pointer pDifferentRow) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
        v1 = _btreeRestoreCursorPosition(tls, pCur);
    } else {
        v1 = SQLITE_OK;
    }
    rc = v1;
    if (rc != 0) {
        (@unsafe.Pointer.To<int32>(pDifferentRow)).Value = (int32)1;
        return rc;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) != CURSOR_VALID) {
        (@unsafe.Pointer.To<int32>(pDifferentRow)).Value = (int32)1;
    } else {
        (@unsafe.Pointer.To<int32>(pDifferentRow)).Value = 0;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3BtreeDropTable(ж<libc.TLS> tls, uintptr p, int32 iTable, @unsafe.Pointer piMoved) {
    int32 rc = default;
    _ = rc;
    _sqlite3BtreeEnter(tls, p);
    rc = _btreeDropTable(tls, p, libc.Uint32FromInt32(iTable), piMoved);
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static void _sqlite3BtreeEnterAll(ж<libc.TLS> tls, uintptr db) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnoSharedCache) == 0) {
        _btreeEnterAll(tls, db);
    }
}

internal static int32 /*r*/ _sqlite3BtreeEof(ref libc.TLS tls, uintptr pCur) {
    return libc.BoolInt32(CURSOR_VALID != libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState));
}

internal static int32 /*r*/ _sqlite3BtreeGetPageSize(ref libc.TLS tls, uintptr p) {
    return libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FpageSize);
}

internal static int32 /*r*/ _sqlite3BtreeGetRequestedReserve(ж<libc.TLS> tls, uintptr p) {
    int32 n1 = default;
    int32 n2 = default;
    int32 v1 = default;
    _ = n1;
    _ = n2;
    _ = v1;
    _sqlite3BtreeEnter(tls, p);
    n1 = libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FnReserveWanted);
    n2 = _sqlite3BtreeGetReserveNoMutex(ref (tls).DerefOrNull(), p);
    _sqlite3BtreeLeave(tls, p);
    if (n1 > n2) {
        v1 = n1;
    } else {
        v1 = n2;
    }
    return v1;
}

internal static int32 /*r*/ _sqlite3BtreeGetReserveNoMutex(ref libc.TLS tls, uintptr p) {
    int32 n = default;
    _ = n;
    n = libc.Int32FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FpageSize - (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FusableSize);
    return n;
}

internal static int32 /*r*/ _sqlite3BtreeIsEmpty(ж<libc.TLS> tls, uintptr pCur, @unsafe.Pointer pRes) {
    int32 rc = default;
    _ = rc;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == CURSOR_VALID) {
        (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
        return SQLITE_OK;
    }
    rc = _moveToRoot(tls, pCur);
    if (rc == (int32)SQLITE_EMPTY) {
        (@unsafe.Pointer.To<int32>(pRes)).Value = (int32)1;
        rc = SQLITE_OK;
    } else {
        (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeIsReadonly(ref libc.TLS tls, uintptr p) {
    return libc.BoolInt32((libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt)).Value.FbtsFlags) & (int32)BTS_READ_ONLY) != 0);
}

internal static int32 /*r*/ _sqlite3BtreeLast(ж<libc.TLS> tls, uintptr pCur, @unsafe.Pointer pRes) {
    if (CURSOR_VALID == libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) && (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FcurFlags) & (int32)BTCF_AtLast) != 0) {
        (@unsafe.Pointer.To<int32>(pRes)).Value = 0;
        return SQLITE_OK;
    }
    return _btreeLast(tls, pCur, pRes);
}

internal static void _sqlite3BtreeLeaveAll(ж<libc.TLS> tls, uintptr db) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FnoSharedCache) == 0) {
        _btreeLeaveAll(tls, db);
    }
}

internal static int32 /*r*/ _sqlite3BtreeLockTable(ж<libc.TLS> tls, uintptr p, int32 iTab, Tu8 isWriteLock) {
    Tu8 lockType = default;
    int32 rc = default;
    _ = lockType;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fsharable != 0) {
        lockType = libc.Uint8FromInt32((int32)READ_LOCK + libc.Int32FromUint8(isWriteLock));
        _sqlite3BtreeEnter(tls, p);
        rc = _querySharedCacheTableLock(tls, p, libc.Uint32FromInt32(iTab), lockType);
        if (rc == SQLITE_OK) {
            rc = _setSharedCacheTableLock(tls, p, libc.Uint32FromInt32(iTab), lockType);
        }
        _sqlite3BtreeLeave(tls, p);
    }
    return rc;
}

internal static Tsqlite3_int64 /*r*/ _sqlite3BtreeMaxRecordSize(ref libc.TLS tls, uintptr pCur) {
    return libc.Int64FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt)).Value.FpageSize) * libc.Int64FromUint32((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpBt)).Value.FnPage);
}

internal static int32 /*r*/ _sqlite3BtreePayloadChecked(ж<libc.TLS> tls, uintptr pCur, Tu32 offset, Tu32 amt, uintptr pBuf) {
    int32 r = default;

    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) == CURSOR_VALID) {
        return _accessPayload(tls, pCur, offset, amt, pBuf, 0);
    } else {
        return _accessPayloadChecked(tls, pCur, offset, amt, pBuf);
    }
    return r;
}

internal static int32 /*r*/ _sqlite3BtreePrevious(ж<libc.TLS> tls, uintptr pCur, int32 flags) {
    uintptr v1 = default;
    _ = v1;
    _ = flags;
    v1 = pCur + 1;
    (@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)).Value = (Tu8)((int32)(~@unsafe.Pointer.To<Tu8>((@unsafe.Pointer)v1)) & ~(libc.Int32FromInt32(BTCF_AtLast) | libc.Int32FromInt32(BTCF_ValidOvfl) | libc.Int32FromInt32(BTCF_ValidNKey)));
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Finfo.FnSize = (uint16)0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FeState) != CURSOR_VALID || libc.Int32FromUint16((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix) == 0 || libc.Int32FromUint8((@unsafe.Pointer.To<TMemPage>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.FpPage)).Value.Fleaf) == 0) {
        return _btreePrevious(tls, pCur);
    }
    (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix = (Tu16)((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCur)).Value.Fix - 1);
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3BtreePutData(ж<libc.TLS> tls, uintptr pCsr, Tu32 offset, Tu32 amt, uintptr z) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCsr)).Value.FeState) >= (int32)CURSOR_REQUIRESEEK) {
        v1 = _btreeRestoreCursorPosition(tls, pCsr);
    } else {
        v1 = SQLITE_OK;
    }
    rc = v1;
    if (rc != SQLITE_OK) {
        return rc;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCsr)).Value.FeState) != CURSOR_VALID) {
        return (int32)SQLITE_ABORT;
    }
    _saveAllCursors(tls, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCsr)).Value.FpBt, (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCsr)).Value.FpgnoRoot, pCsr);
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)pCsr)).Value.FcurFlags) & (int32)BTCF_WriteFlag) == 0) {
        return (int32)SQLITE_READONLY;
    }
    return _accessPayload(tls, pCsr, offset, amt, z, (int32)1);
}

internal static int32 /*r*/ _sqlite3BtreeRollback(ж<libc.TLS> tls, uintptr p, int32 tripCode, int32 writeOnly) {
    uintptr pBt = default;
    ref var pPage1 = ref heap(new uintptr(), out var ᏑpPage1);
    int32 rc = default;
    int32 rc2 = default;
    int32 rc21 = default;
    int32 v1 = default;
    _ = pBt;
    _ = pPage1;
    _ = rc;
    _ = rc2;
    _ = rc21;
    _ = v1;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    if (tripCode == SQLITE_OK) {
        v1 = _saveAllCursors(tls, pBt, (uint32)0, (uintptr)0);
        tripCode = v1;
        rc = v1;
        if (rc != 0) {
            writeOnly = 0;
        }
    } else {
        rc = SQLITE_OK;
    }
    if (tripCode != 0) {
        rc2 = _sqlite3BtreeTripAllCursors(tls, p, tripCode, writeOnly);
        if (rc2 != SQLITE_OK) {
            rc = rc2;
        }
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_WRITE) {
        rc21 = _sqlite3PagerRollback(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager);
        if (rc21 != SQLITE_OK) {
            rc = rc21;
        }
        if (_btreeGetPage(tls, pBt, (uint32)1, @unsafe.Pointer.FromBox(ᏑpPage1), 0) == SQLITE_OK) {
            _btreeSetNPage(ref (tls).DerefOrNull(), pBt, pPage1);
            _releasePageOne(tls, pPage1);
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction = (uint8)TRANS_READ;
        _btreeClearHasContent(tls, pBt);
    }
    _btreeEndTransaction(tls, p);
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeSavepoint(ж<libc.TLS> tls, uintptr p, int32 op, int32 iSavepoint) {
    uintptr pBt = default;
    int32 rc = default;
    _ = pBt;
    _ = rc;
    rc = SQLITE_OK;
    if (p != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans) == (int32)TRANS_WRITE) {
        pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
        _sqlite3BtreeEnter(tls, p);
        if (op == (int32)SAVEPOINT_ROLLBACK) {
            rc = _saveAllCursors(tls, pBt, (uint32)0, (uintptr)0);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3PagerSavepoint(tls, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPager, op, iSavepoint);
        }
        if (rc == SQLITE_OK) {
            if (iSavepoint < 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_INITIALLY_EMPTY) != 0) {
                (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FnPage = (uint32)0;
            }
            rc = _newDatabase(tls, pBt);
            _btreeSetNPage(ref (tls).DerefOrNull(), pBt, (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1);
        }
        _sqlite3BtreeLeave(tls, p);
    }
    return rc;
}

internal static uintptr /*r*/ _sqlite3BtreeSchema(ж<libc.TLS> tls, uintptr p, int32 nBytes, uintptr __ccgo_fp_xFree) {
    uintptr pBt = default;
    _ = pBt;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    _sqlite3BtreeEnter(tls, p);
    if (!((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpSchema != 0) && nBytes != 0) {
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpSchema = _sqlite3DbMallocZero(tls, (uintptr)0, libc.Uint64FromInt32(nBytes));
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FxFreeSchema = __ccgo_fp_xFree;
    }
    _sqlite3BtreeLeave(tls, p);
    return (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpSchema;
}

internal static int32 /*r*/ _sqlite3BtreeSetAutoVacuum(ж<libc.TLS> tls, uintptr p, int32 autoVacuum) {
    Tu8 av = default;
    uintptr pBt = default;
    int32 rc = default;
    int32 v1 = default;
    bool v2 = default;
    _ = av;
    _ = pBt;
    _ = rc;
    _ = v1;
    _ = v2;
    pBt = (@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FpBt;
    rc = SQLITE_OK;
    av = libc.Uint8FromInt32(autoVacuum);
    _sqlite3BtreeEnter(tls, p);
    v2 = (libc.Int32FromUint16((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FbtsFlags) & (int32)BTS_PAGESIZE_FIXED) != 0; if (v2) {
        if (av != 0) {
            v1 = (int32)1;
        } else {
            v1 = 0;
        }
    }
    if (v2 && v1 != libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum)) {
        rc = (int32)SQLITE_READONLY;
    } else {
        if (av != 0) {
            v1 = (int32)1;
        } else {
            v1 = 0;
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FautoVacuum = libc.Uint8FromInt32(v1);
        if (libc.Int32FromUint8(av) == (int32)2) {
            v1 = (int32)1;
        } else {
            v1 = 0;
        }
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FincrVacuum = libc.Uint8FromInt32(v1);
    }
    _sqlite3BtreeLeave(tls, p);
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeSharable(ref libc.TLS tls, uintptr p) {
    return libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.Fsharable);
}

internal static int32 /*r*/ _sqlite3BtreeTripAllCursors(ж<libc.TLS> tls, uintptr pBtree, int32 errCode, int32 writeOnly) {
    uintptr p = default;
    int32 rc = default;
    _ = p;
    _ = rc;
    rc = SQLITE_OK;
    if (pBtree != 0) {
        _sqlite3BtreeEnter(tls, pBtree);
        p = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)(@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)pBtree)).Value.FpBt)).Value.FpCursor;
        for (;;) {
            if (!(p != 0)) {
                break;
            }
            if (writeOnly != 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FcurFlags) & (int32)BTCF_WriteFlag) == 0) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FeState) == CURSOR_VALID || libc.Int32FromUint8((@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FeState) == (int32)CURSOR_SKIPNEXT) {
                    rc = _saveCursorPosition(tls, p);
                    if (rc != SQLITE_OK) {
                        _sqlite3BtreeTripAllCursors(tls, pBtree, rc, 0);
                        break;
                    }
                }
            } else {
                _sqlite3BtreeClearCursor(tls, p);
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FeState = (uint8)CURSOR_FAULT;
                (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FskipNext = errCode;
            }
            _btreeReleaseAllCursorPages(tls, p);
            goto _1;
_1:;
            p = (@unsafe.Pointer.To<TBtCursor>((@unsafe.Pointer)p)).Value.FpNext;
        }
        _sqlite3BtreeLeave(tls, pBtree);
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3BtreeTxnState(ref libc.TLS tls, uintptr p) {
    int32 v1 = default;
    _ = v1;
    if (p != 0) {
        v1 = libc.Int32FromUint8((@unsafe.Pointer.To<TBtree>((@unsafe.Pointer)p)).Value.FinTrans);
    } else {
        v1 = 0;
    }
    return v1;
}

internal static void _sqlite3CodeRowTrigger(ж<libc.TLS> tls, uintptr pParse, uintptr pTrigger, int32 op, uintptr pChanges, int32 tr_tm, uintptr pTab, int32 reg, int32 orconf, int32 ignoreJump) {
    uintptr p = default;
    _ = p;
    p = pTrigger;
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Fop) == op || (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FbReturning != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_INSERT && op == (int32)TK_UPDATE) && libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Ftr_tm) == tr_tm && _checkColumnOverlap(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpColumns, pChanges) != 0) {
            if (!((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FbReturning != 0)) {
                _sqlite3CodeRowTriggerDirect(tls, pParse, p, pTab, reg, orconf, ignoreJump);
            } else {
                if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel == (uintptr)0) {
                    _codeReturningTrigger(tls, pParse, p, pTab, reg);
                }
            }
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpNext;
    }
}

internal static uintptr /*r*/ _sqlite3Expr(ж<libc.TLS> tls, uintptr db, int32 op, uintptr zToken) {
    ref var x = ref heap(new TToken(), out var Ꮡx);
    _ = x;
    x.Fz = zToken;
    x.Fn = libc.Uint32FromInt32(_sqlite3Strlen30(tls, zToken));
    return _sqlite3ExprAlloc(tls, db, op, @unsafe.Pointer.FromPinnedBox(Ꮡx), 0);
}

internal static uintptr /*r*/ _sqlite3ExprAnd(ж<libc.TLS> tls, uintptr pParse, uintptr pLeft, uintptr pRight) {
    uintptr r = default;

    uintptr db = default;
    Tu32 f = default;
    _ = db;
    _ = f;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (pLeft == (uintptr)0) {
        return pRight;
    } else {
        if (pRight == (uintptr)0) {
            return pLeft;
        } else {
            f = (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fflags | (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fflags);
            if ((Tu32)(f & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_InnerON) | libc.Int32FromInt32(EP_IsFalse) | libc.Int32FromInt32(EP_HasFunc))) == (uint32)EP_IsFalse && !(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
                _sqlite3ExprDeferredDelete(tls, pParse, pLeft);
                _sqlite3ExprDeferredDelete(tls, pParse, pRight);
                return _sqlite3ExprInt32(tls, db, 0);
            } else {
                return _sqlite3PExpr(tls, pParse, (int32)TK_AND, pLeft, pRight);
            }
        }
    }
    return r;
}

internal static void _sqlite3ExprAttachSubtrees(ж<libc.TLS> tls, uintptr db, uintptr pRoot, uintptr pLeft, uintptr pRight) {
    if (pRoot == (uintptr)0) {
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pLeft);
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pRight);
    } else {
        if (pRight != 0) {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRoot)).Value.FpRight = pRight;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pRoot + 4))).Value |= (libc.Uint32FromInt32(libc.Int32FromInt32(EP_Collate) | libc.Int32FromInt32(EP_Subquery) | libc.Int32FromInt32(EP_HasFunc)) & (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fflags);
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRoot)).Value.FnHeight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.FnHeight + (int32)1;
        } else {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRoot)).Value.FnHeight = (int32)1;
        }
        if (pLeft != 0) {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRoot)).Value.FpLeft = pLeft;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pRoot + 4))).Value |= (libc.Uint32FromInt32(libc.Int32FromInt32(EP_Collate) | libc.Int32FromInt32(EP_Subquery) | libc.Int32FromInt32(EP_HasFunc)) & (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fflags);
            if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FnHeight >= (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRoot)).Value.FnHeight) {
                (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRoot)).Value.FnHeight = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.FnHeight + (int32)1;
            }
        }
    }
}

internal static void _sqlite3ExprCode(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 target) {
    int32 inReg = default;
    Tu8 op = default;
    uintptr pX = default;
    _ = inReg;
    _ = op;
    _ = pX;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe == (uintptr)0) {
        return;
    }
    inReg = _sqlite3ExprCodeTarget(tls, pParse, pExpr, target);
    if (inReg != target) {
        pX = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), pExpr);
        if (pX != 0 && ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0 || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pX)).Value.Fop) == (int32)TK_REGISTER)) {
            op = (uint8)OP_Copy;
        } else {
            op = (uint8)OP_SCopy;
        }
        _sqlite3VdbeAddOp2(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, libc.Int32FromUint8(op), inReg, target);
    }
}

internal static int32 /*r*/ _sqlite3ExprCodeGetColumn(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 iColumn, int32 iTable, int32 iReg, Tu8 p5) {
    uintptr pOp = default;
    _ = pOp;
    _sqlite3ExprCodeGetColumnOfTable(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe, pTab, iTable, iColumn, iReg);
    if (p5 != 0) {
        pOp = _sqlite3VdbeGetLastOp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Column) {
            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp5 = (uint16)p5;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_VColumn) {
            (@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp5 = libc.Uint16FromInt32(libc.Int32FromUint8(p5) & libc.Int32FromInt32(OPFLAG_NOCHNG));
        }
    }
    return iReg;
}

internal static int32 /*r*/ _sqlite3ExprIdToTrueFalse(ref libc.TLS tls, uintptr pExpr) {
    Tu32 v = default;
    Tu32 v1 = default;
    bool v2 = default;
    _ = v;
    _ = v1;
    _ = v2;
    v2 = !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Quoted) | libc.Int32FromInt32(EP_IntValue))) != libc.Uint32FromInt32(0)); if (v2) {
        v1 = _sqlite3IsTrueOrFalse(ref tls, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8)));
        v = v1;
    }
    if (v2 && v1 != (uint32)0) {
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop = (uint8)TK_TRUEFALSE;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pExpr + 4))).Value |= v;
        return (int32)1;
    }
    return 0;
}

internal static void _sqlite3ExprIfFalse(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 dest, int32 jumpIfNull) {
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 addrIsNull = default;
        int32 d2 = default;
        int32 destIfNull = default;
        int32 isNot = default;
        int32 isTrue = default;
        int32 op = default;
        ref var r1 = ref heap(new int32(), out var Ꮡr1);
        ref var r2 = ref heap(new int32(), out var Ꮡr2);
        ref var regFree1 = ref heap(new int32(), out var ᏑregFree1);
        ref var regFree2 = ref heap(new int32(), out var ᏑregFree2);
        int32 v19 = default;
        uintptr pAlt = default;
        uintptr pFirst = default;
        uintptr pSecond = default;
        uintptr v = default;
        _ = addrIsNull;
        _ = d2;
        _ = destIfNull;
        _ = isNot;
        _ = isTrue;
        _ = op;
        _ = pAlt;
        _ = pFirst;
        _ = pSecond;
        _ = r1;
        _ = r2;
        _ = regFree1;
        _ = regFree2;
        _ = v;
        _ = v19;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        op = 0;
        regFree1 = 0;
        regFree2 = 0;
        if (v == (uintptr)0) {
            return;
        }
        if (pExpr == (uintptr)0) {
            return;
        }
        op = (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) + (libc.Int32FromInt32(TK_ISNULL) & libc.Int32FromInt32(1)) ^ (int32)1) - (libc.Int32FromInt32(TK_ISNULL) & libc.Int32FromInt32(1));
        switch (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop)) {
        case var ᴛ when ᴛ == (int32)TK_OR: {
            goto _1;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_AND: {
            goto _2;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_NOT: {
            goto _3;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_TRUTH: {
            goto _4;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_ISNOT: {
            goto _5;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_IS: {
            goto _6;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_EQ: {
            goto _7;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_NE: {
            goto _8;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_GE: {
            goto _9;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_GT: {
            goto _10;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_LE: {
            goto _11;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_LT: {
            goto _12;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_NOTNULL: {
            goto _13;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_ISNULL: {
            goto _14;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_BETWEEN: {
            goto _15;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_IN: {
            goto _16;
            break;
        }
        default: {
            goto _17;
            break;
        }}

        goto _18;
_2:;
_1:;
        pAlt = _sqlite3ExprSimplifiedAndOr(ref (tls).DerefOrNull(), pExpr);
        if (pAlt != pExpr) {
            _sqlite3ExprIfFalse(tls, pParse, pAlt, dest, jumpIfNull);
        } else {
            if (_exprEvalRhsFirst(ref (tls).DerefOrNull(), pExpr) != 0) {
                pFirst = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight;
                pSecond = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
            } else {
                pFirst = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
                pSecond = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight;
            }
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AND) {
                _sqlite3ExprIfFalse(tls, pParse, pFirst, dest, jumpIfNull);
                _sqlite3ExprIfFalse(tls, pParse, pSecond, dest, jumpIfNull);
            } else {
                d2 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                _sqlite3ExprIfTrue(tls, pParse, pFirst, d2, jumpIfNull ^ (int32)SQLITE_JUMPIFNULL);
                _sqlite3ExprIfFalse(tls, pParse, pSecond, dest, jumpIfNull);
                _sqlite3VdbeResolveLabel(tls, v, d2);
            }
        }
        goto _18;
_3:;
        _sqlite3ExprIfTrue(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, dest, jumpIfNull);
        goto _18;
_4:;
        isNot = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop2) == (int32)TK_ISNOT);
        isTrue = _sqlite3ExprTruthValue(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight);
        if ((isTrue ^ isNot) != 0) {
            if (isNot != 0) {
                v19 = 0;
            } else {
                v19 = (int32)SQLITE_JUMPIFNULL;
            }
            _sqlite3ExprIfFalse(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, dest, v19);
        } else {
            if (isNot != 0) {
                v19 = 0;
            } else {
                v19 = (int32)SQLITE_JUMPIFNULL;
            }
            _sqlite3ExprIfTrue(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, dest, v19);
        }
        goto _18;
_6:;
_5:;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_IS) {
            v19 = (int32)TK_NE;
        } else {
            v19 = (int32)TK_EQ;
        }
        op = v19;
        jumpIfNull = (int32)SQLITE_NULLEQ;
_12:;
_11:;
_10:;
_9:;
_8:;
_7:;
        if (_sqlite3ExprIsVector(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft) != 0) {
            goto default_expr;
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0 && jumpIfNull != (int32)SQLITE_NULLEQ) {
            addrIsNull = _exprComputeOperands(tls, pParse, pExpr, @unsafe.Pointer.FromPinnedBox(Ꮡr1), @unsafe.Pointer.FromPinnedBox(Ꮡr2), @unsafe.Pointer.FromPinnedBox(ᏑregFree1), @unsafe.Pointer.FromPinnedBox(ᏑregFree2));
        } else {
            r1 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, @unsafe.Pointer.FromPinnedBox(ᏑregFree1));
            r2 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, @unsafe.Pointer.FromPinnedBox(ᏑregFree2));
            addrIsNull = 0;
        }
        _codeCompare(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, op, r1, r2, dest, jumpIfNull, libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Commuted))) != (uint32)0));
        if (addrIsNull != 0) {
            if (jumpIfNull != 0) {
                _sqlite3VdbeChangeP2(ref (tls).DerefOrNull(), v, addrIsNull, dest);
            } else {
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrIsNull);
            }
        }
        goto _18;
_14:;
_13:;
        r1 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, @unsafe.Pointer.FromPinnedBox(ᏑregFree1));
        if (regFree1 != 0) {
            _sqlite3VdbeTypeofColumn(ref (tls).DerefOrNull(), v, r1);
        }
        _sqlite3VdbeAddOp2(tls, v, op, r1, dest);
        goto _18;
_15:;
        _exprCodeBetween(tls, pParse, pExpr, dest, __ccgo_fp(_sqlite3ExprIfFalse), jumpIfNull);
        goto _18;
_16:;
        if (jumpIfNull != 0) {
            _sqlite3ExprCodeIN(tls, pParse, pExpr, dest, dest);
        } else {
            destIfNull = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
            _sqlite3ExprCodeIN(tls, pParse, pExpr, dest, destIfNull);
            _sqlite3VdbeResolveLabel(tls, v, destIfNull);
        }
        goto _18;
_17:;
        goto default_expr;
default_expr:;
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsFalse))) == (uint32)EP_IsFalse) {
            _sqlite3VdbeGoto(tls, v, dest);
        } else {
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsTrue))) == (uint32)EP_IsTrue) {
            } else {
                r1 = _sqlite3ExprCodeTemp(tls, pParse, pExpr, @unsafe.Pointer.FromPinnedBox(ᏑregFree1));
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_IfNot, r1, dest, libc.BoolInt32(jumpIfNull != 0));
            }
        }
        goto _18;
_18:;
        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regFree1);
        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regFree2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3ExprIfFalseDup(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 dest, int32 jumpIfNull) {
    uintptr db = default;
    uintptr pCopy = default;
    _ = db;
    _ = pCopy;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pCopy = _sqlite3ExprDup(tls, db, pExpr, 0);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmallocFailed) == 0) {
        _sqlite3ExprIfFalse(tls, pParse, pCopy, dest, jumpIfNull);
    }
    _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pCopy);
}

internal static void _sqlite3ExprIfTrue(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, int32 dest, int32 jumpIfNull) {
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 addrIsNull = default;
        int32 d2 = default;
        int32 destIfFalse = default;
        int32 destIfNull = default;
        int32 isNot = default;
        int32 isTrue = default;
        int32 op = default;
        ref var r1 = ref heap(new int32(), out var Ꮡr1);
        ref var r2 = ref heap(new int32(), out var Ꮡr2);
        ref var regFree1 = ref heap(new int32(), out var ᏑregFree1);
        ref var regFree2 = ref heap(new int32(), out var ᏑregFree2);
        int32 v19 = default;
        uintptr pAlt = default;
        uintptr pFirst = default;
        uintptr pSecond = default;
        uintptr v = default;
        _ = addrIsNull;
        _ = d2;
        _ = destIfFalse;
        _ = destIfNull;
        _ = isNot;
        _ = isTrue;
        _ = op;
        _ = pAlt;
        _ = pFirst;
        _ = pSecond;
        _ = r1;
        _ = r2;
        _ = regFree1;
        _ = regFree2;
        _ = v;
        _ = v19;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        op = 0;
        regFree1 = 0;
        regFree2 = 0;
        if (v == (uintptr)0) {
            return;
        }
        if (pExpr == (uintptr)0) {
            return;
        }
        op = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop);
        switch (op) {
        case var ᴛ when ᴛ == (int32)TK_OR: {
            goto _1;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_AND: {
            goto _2;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_NOT: {
            goto _3;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_TRUTH: {
            goto _4;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_ISNOT: {
            goto _5;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_IS: {
            goto _6;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_EQ: {
            goto _7;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_NE: {
            goto _8;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_GE: {
            goto _9;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_GT: {
            goto _10;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_LE: {
            goto _11;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_LT: {
            goto _12;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_NOTNULL: {
            goto _13;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_ISNULL: {
            goto _14;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_BETWEEN: {
            goto _15;
            break;
        }
        case var ᴛ when ᴛ == (int32)TK_IN: {
            goto _16;
            break;
        }
        default: {
            goto _17;
            break;
        }}

        goto _18;
_2:;
_1:;
        pAlt = _sqlite3ExprSimplifiedAndOr(ref (tls).DerefOrNull(), pExpr);
        if (pAlt != pExpr) {
            _sqlite3ExprIfTrue(tls, pParse, pAlt, dest, jumpIfNull);
        } else {
            if (_exprEvalRhsFirst(ref (tls).DerefOrNull(), pExpr) != 0) {
                pFirst = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight;
                pSecond = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
            } else {
                pFirst = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
                pSecond = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight;
            }
            if (op == (int32)TK_AND) {
                d2 = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
                _sqlite3ExprIfFalse(tls, pParse, pFirst, d2, jumpIfNull ^ (int32)SQLITE_JUMPIFNULL);
                _sqlite3ExprIfTrue(tls, pParse, pSecond, dest, jumpIfNull);
                _sqlite3VdbeResolveLabel(tls, v, d2);
            } else {
                _sqlite3ExprIfTrue(tls, pParse, pFirst, dest, jumpIfNull);
                _sqlite3ExprIfTrue(tls, pParse, pSecond, dest, jumpIfNull);
            }
        }
        goto _18;
_3:;
        _sqlite3ExprIfFalse(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, dest, jumpIfNull);
        goto _18;
_4:;
        isNot = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop2) == (int32)TK_ISNOT);
        isTrue = _sqlite3ExprTruthValue(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight);
        if ((isTrue ^ isNot) != 0) {
            if (isNot != 0) {
                v19 = (int32)SQLITE_JUMPIFNULL;
            } else {
                v19 = 0;
            }
            _sqlite3ExprIfTrue(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, dest, v19);
        } else {
            if (isNot != 0) {
                v19 = (int32)SQLITE_JUMPIFNULL;
            } else {
                v19 = 0;
            }
            _sqlite3ExprIfFalse(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, dest, v19);
        }
        goto _18;
_6:;
_5:;
        if (op == (int32)TK_IS) {
            v19 = (int32)TK_EQ;
        } else {
            v19 = (int32)TK_NE;
        }
        op = v19;
        jumpIfNull = (int32)SQLITE_NULLEQ;
_12:;
_11:;
_10:;
_9:;
_8:;
_7:;
        if (_sqlite3ExprIsVector(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft) != 0) {
            goto default_expr;
        }
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Subquery))) != (uint32)0 && jumpIfNull != (int32)SQLITE_NULLEQ) {
            addrIsNull = _exprComputeOperands(tls, pParse, pExpr, @unsafe.Pointer.FromPinnedBox(Ꮡr1), @unsafe.Pointer.FromPinnedBox(Ꮡr2), @unsafe.Pointer.FromPinnedBox(ᏑregFree1), @unsafe.Pointer.FromPinnedBox(ᏑregFree2));
        } else {
            r1 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, @unsafe.Pointer.FromPinnedBox(ᏑregFree1));
            r2 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, @unsafe.Pointer.FromPinnedBox(ᏑregFree2));
            addrIsNull = 0;
        }
        _codeCompare(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight, op, r1, r2, dest, jumpIfNull, libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Commuted))) != (uint32)0));
        if (addrIsNull != 0) {
            if (jumpIfNull != 0) {
                _sqlite3VdbeChangeP2(ref (tls).DerefOrNull(), v, addrIsNull, dest);
            } else {
                _sqlite3VdbeJumpHere(ref (tls).DerefOrNull(), v, addrIsNull);
            }
        }
        goto _18;
_14:;
_13:;
        r1 = _sqlite3ExprCodeTemp(tls, pParse, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft, @unsafe.Pointer.FromPinnedBox(ᏑregFree1));
        if (regFree1 != 0) {
            _sqlite3VdbeTypeofColumn(ref (tls).DerefOrNull(), v, r1);
        }
        _sqlite3VdbeAddOp2(tls, v, op, r1, dest);
        goto _18;
_15:;
        _exprCodeBetween(tls, pParse, pExpr, dest, __ccgo_fp(_sqlite3ExprIfTrue), jumpIfNull);
        goto _18;
_16:;
        destIfFalse = _sqlite3VdbeMakeLabel(ref (tls).DerefOrNull(), (@unsafe.Pointer)pParse);
        if (jumpIfNull != 0) {
            v19 = dest;
        } else {
            v19 = destIfFalse;
        }
        destIfNull = v19;
        _sqlite3ExprCodeIN(tls, pParse, pExpr, destIfFalse, destIfNull);
        _sqlite3VdbeGoto(tls, v, dest);
        _sqlite3VdbeResolveLabel(tls, v, destIfFalse);
        goto _18;
_17:;
        goto default_expr;
default_expr:;
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsTrue))) == (uint32)EP_IsTrue) {
            _sqlite3VdbeGoto(tls, v, dest);
        } else {
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsFalse))) == (uint32)EP_IsFalse) {
            } else {
                r1 = _sqlite3ExprCodeTemp(tls, pParse, pExpr, @unsafe.Pointer.FromPinnedBox(ᏑregFree1));
                _sqlite3VdbeAddOp3(tls, v, (int32)OP_If, r1, dest, libc.BoolInt32(jumpIfNull != 0));
            }
        }
        goto _18;
_18:;
        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regFree1);
        _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regFree2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3ExprIsConstantOrFunction(ж<libc.TLS> tls, uintptr p, Tu8 isInit) {
    return _exprIsConst(tls, (uintptr)0, p, (int32)4 + libc.Int32FromUint8(isInit));
}

internal static int32 /*r*/ _sqlite3ExprIsNotTrue(ж<libc.TLS> tls, uintptr pExpr) {
    ref var v = ref heap(new int32(), out var Ꮡv);
    _ = v;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_NULL) {
        return (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_TRUEFALSE && _sqlite3ExprTruthValue(ref (tls).DerefOrNull(), pExpr) == 0) {
        return (int32)1;
    }
    v = (int32)1;
    if (_sqlite3ExprIsInteger(tls, pExpr, @unsafe.Pointer.FromPinnedBox(Ꮡv), (uintptr)0, 0) != 0 && v == 0) {
        return (int32)1;
    }
    return 0;
}

internal static uintptr /*r*/ _sqlite3ExprSimplifiedAndOr(ref libc.TLS tls, uintptr pExpr) {
    uintptr pLeft = default;
    uintptr pRight = default;
    uintptr v1 = default;
    _ = pLeft;
    _ = pRight;
    _ = v1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AND || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_OR) {
        pRight = _sqlite3ExprSimplifiedAndOr(ref tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpRight);
        pLeft = _sqlite3ExprSimplifiedAndOr(ref tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft);
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsTrue))) == (uint32)EP_IsTrue || (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsFalse))) == (uint32)EP_IsFalse) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AND) {
                v1 = pRight;
            } else {
                v1 = pLeft;
            }
            pExpr = v1;
        } else {
            if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsTrue))) == (uint32)EP_IsTrue || (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pLeft)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_OuterON) | libc.Int32FromInt32(EP_IsFalse))) == (uint32)EP_IsFalse) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AND) {
                    v1 = pLeft;
                } else {
                    v1 = pRight;
                }
                pExpr = v1;
            }
        }
    }
    return pExpr;
}

internal static uintptr /*r*/ _sqlite3ExprSkipCollate(ref libc.TLS tls, uintptr pExpr) {
    while (pExpr != 0 && (Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Skip))) != (uint32)0) {
        pExpr = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpLeft;
    }
    return pExpr;
}

internal static void _sqlite3ExprToRegister(ref libc.TLS tls, uintptr pExpr, int32 iReg) {
    uintptr p = default;
    _ = p;
    p = _sqlite3ExprSkipCollateAndLikely(ref tls, pExpr);
    if (p == (uintptr)0) {
        return;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_REGISTER) {
    } else {
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop2 = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop = (uint8)TK_REGISTER;
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiTable = iReg;
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(p + 4))).Value &= ~libc.Uint32FromInt32(libc.Int32FromInt32(EP_Skip));
    }
}

internal static void _sqlite3ExprUnmapAndDelete(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    if (p != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            _sqlite3RenameExprUnmap(tls, pParse, p);
        }
        _sqlite3ExprDeleteNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, p);
    }
}

internal static void _sqlite3Fp2Convert10(ref libc.TLS tls, Tu64 m, int32 e, int32 n, @unsafe.Pointer pD, @unsafe.Pointer pP) {
    ref var d1 = ref heap(new Tu64(), out var Ꮡd1);
    Tu64 h = default;
    ref var d2 = ref heap(new Tu32(), out var Ꮡd2);
    int32 p = default;
    _ = d1;
    _ = d2;
    _ = h;
    _ = p;
    p = n - (int32)1 - _pwr2to10(ref tls, e + (int32)63);
    h = _sqlite3Multiply128(ref tls, m, _powerOfTen(ref tls, p, @unsafe.Pointer.FromPinnedBox(Ꮡd2)), @unsafe.Pointer.FromPinnedBox(Ꮡd1));
    if (n == (int32)18) {
        h = h >> (int)(libc.Uint64FromInt32(-(e + _pwr10to2(ref tls, p) + libc.Int32FromInt32(2))));
        (@unsafe.Pointer.To<Tu64>(pD)).Value = (h + (Tu64)(h << (int)(libc.Int32FromInt32(1)) & (uint64)2)) >> (int)((int32)1);
    } else {
        (@unsafe.Pointer.To<Tu64>(pD)).Value = h >> (int)(-(e + _pwr10to2(ref tls, p) + (int32)1));
    }
    (@unsafe.Pointer.To<int32>(pP)).Value = -p;
}

internal static void _sqlite3Fts5BufferSet(ж<libc.TLS> tls, @unsafe.Pointer pRc, uintptr pBuf, int32 nData, uintptr pData) {
    (@unsafe.Pointer.To<TFts5Buffer>((@unsafe.Pointer)pBuf)).Value.Fn = 0;
    _sqlite3Fts5BufferAppendBlob(tls, pRc, pBuf, libc.Uint32FromInt32(nData), pData);
}

internal static int32 /*r*/ _sqlite3Fts5Get32(ref libc.TLS tls, uintptr aBuf) {
    return libc.Int32FromUint32(((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf)))).Lsh((int64)(libc.Int32FromInt32(24))) + libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) + libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) + (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aBuf + 3))));
}

internal static int32 /*r*/ _sqlite3Fts5GetVarint32(ref libc.TLS tls, uintptr p, @unsafe.Pointer v) {
    int32 r = default;

    Tu32 a = default;
    Tu32 b = default;
    Tu8 n = default;
    ref var v64 = ref heap(new Tu64(), out var Ꮡv64);
    _ = a;
    _ = b;
    _ = n;
    _ = v64;
    a = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p)));
    if (!((Tu32)(a & libc.Uint32FromInt32(0x80)) != 0)) {
        (@unsafe.Pointer.To<Tu32>(v)).Value = a;
        return (int32)1;
    }
    p = p + 1;
    b = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p)));
    if (!((Tu32)(b & libc.Uint32FromInt32(0x80)) != 0)) {
        a = (Tu32)(a & (uint32)0x7f);
        a = a << (int)((int32)7);
        (@unsafe.Pointer.To<Tu32>(v)).Value = (Tu32)(a | b);
        return (int32)2;
    }
    p = p + 1;
    a = a << (int)((int32)14);
    a = (Tu32)(a | (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p))));
    if (!((Tu32)(a & libc.Uint32FromInt32(0x80)) != 0)) {
        a = (Tu32)(a & libc.Uint32FromInt32(libc.Int32FromInt32(0x7f).Lsh((int64)(libc.Int32FromInt32(14))) | libc.Int32FromInt32(0x7f)));
        b = (Tu32)(b & (uint32)0x7f);
        b = b << (int)((int32)7);
        (@unsafe.Pointer.To<Tu32>(v)).Value = (Tu32)(a | b);
        return (int32)3;
    }
    p = p - (uintptr)2;
    n = _sqlite3Fts5GetVarint(ref tls, (@unsafe.Pointer)p, @unsafe.Pointer.FromPinnedBox(Ꮡv64));
    (@unsafe.Pointer.To<Tu32>(v)).Value = (uint32)v64 & (uint32)0x7FFFFFFF;
    return libc.Int32FromUint8(n);
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5GetVarintLen(ref libc.TLS tls, Tu32 iVal) {
    if (iVal < libc.Uint32FromInt32(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(14))))) {
        return (int32)2;
    }
    if (iVal < libc.Uint32FromInt32(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(21))))) {
        return (int32)3;
    }
    if (iVal < libc.Uint32FromInt32(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(28))))) {
        return (int32)4;
    }
    return (int32)5;
}

internal static int32 /*r*/ _sqlite3Fts5IndexGetOrigin(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer piOrigin) {
    uintptr pStruct = default;
    _ = pStruct;
    pStruct = _fts5StructureRead(tls, p);
    if (pStruct != 0) {
        (@unsafe.Pointer.To<Ti64>(piOrigin)).Value = libc.Int64FromUint64((@unsafe.Pointer.To<TFts5Structure>((@unsafe.Pointer)pStruct)).Value.FnOriginCntr);
        _fts5StructureRelease(tls, pStruct);
    }
    return _fts5IndexReturn(ref (tls).DerefOrNull(), p);
}

internal static int32 /*r*/ _sqlite3Fts5PoslistNext64(ref libc.TLS tls, uintptr a, int32 n, @unsafe.Pointer pi, @unsafe.Pointer piOff) {
    int32 r = default;

    int32 i = default;
    int32 v1 = default;
    Ti64 iOff = default;
    ref var iVal = ref heap(new Tu32(), out var ᏑiVal);
    _ = i;
    _ = iOff;
    _ = iVal;
    _ = v1;
    i = ~@unsafe.Pointer.To<int32>(pi);
    if (i >= n) {
        (@unsafe.Pointer.To<Ti64>(piOff)).Value = (int64)(-(int32)1);
        return (int32)1;
    } else {
        iOff = ~@unsafe.Pointer.To<Ti64>(piOff);
        v1 = i;
        i = i + 1;
        iVal = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + (uintptr)v1)));
        if ((Tu32)(iVal & (uint32)0x80) != 0) {
            i = i - 1;
            i = i + _sqlite3Fts5GetVarint32(ref tls, a + (uintptr)i, @unsafe.Pointer.FromPinnedBox(ᏑiVal));
        }
        if (iVal <= (uint32)1) {
            if (iVal == (uint32)0) {
                (@unsafe.Pointer.To<int32>(pi)).Value = i;
                return 0;
            }
            v1 = i;
            i = i + 1;
            iVal = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + (uintptr)v1)));
            if ((Tu32)(iVal & (uint32)0x80) != 0) {
                i = i - 1;
                i = i + _sqlite3Fts5GetVarint32(ref tls, a + (uintptr)i, @unsafe.Pointer.FromPinnedBox(ᏑiVal));
            }
            iOff = libc.Int64FromUint32(iVal) << (int)((int32)32);
            v1 = i;
            i = i + 1;
            iVal = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + (uintptr)v1)));
            if ((Tu32)(iVal & (uint32)0x80) != 0) {
                i = i - 1;
                i = i + _sqlite3Fts5GetVarint32(ref tls, a + (uintptr)i, @unsafe.Pointer.FromPinnedBox(ᏑiVal));
            }
            if (iVal < (uint32)2) {
                (@unsafe.Pointer.To<Ti64>(piOff)).Value = (int64)(-(int32)1);
                return (int32)1;
            }
            (@unsafe.Pointer.To<Ti64>(piOff)).Value = iOff + libc.Int64FromUint32((Tu32)((iVal - libc.Uint32FromInt32(2)) & libc.Uint32FromInt32(0x7FFFFFFF)));
        } else {
            (@unsafe.Pointer.To<Ti64>(piOff)).Value = (Ti64)(iOff & (libc.Int64FromInt32(0x7FFFFFFF).Lsh((int64)(libc.Int32FromInt32(32))))) + (Ti64)((iOff + libc.Int64FromUint32(iVal - libc.Uint32FromInt32(2))) & (int64)0x7FFFFFFF);
        }
        (@unsafe.Pointer.To<int32>(pi)).Value = i;
        return 0;
    }
    return r;
}

internal static void _sqlite3Fts5Put32(ref libc.TLS tls, @unsafe.Pointer aBuf, int32 iVal) {
    (@unsafe.Pointer.To<Tu8>(aBuf)).Value = libc.Uint8FromInt32(iVal >> (int)((int32)24) & (int32)0x00FF);
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 1))).Value = libc.Uint8FromInt32(iVal >> (int)((int32)16) & (int32)0x00FF);
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 2))).Value = libc.Uint8FromInt32(iVal >> (int)((int32)8) & (int32)0x00FF);
    (@unsafe.Pointer.To<Tu8>(@unsafe.Add(aBuf, 3))).Value = libc.Uint8FromInt32(iVal >> 0 & (int32)0x00FF);
}

internal static void _sqlite3Fts5UnicodeAscii(ref libc.TLS tls, uintptr aArray, uintptr aAscii) {
    int32 bToken = default;
    int32 i = default;
    int32 iTbl = default;
    int32 n = default;
    _ = bToken;
    _ = i;
    _ = iTbl;
    _ = n;
    i = 0;
    iTbl = 0;
    while (i < (int32)128) {
        bToken = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aArray + (uintptr)(libc.Int32FromUint16(_aFts5UnicodeData[iTbl]) & (int32)0x1F))));
        n = (libc.Int32FromUint16(_aFts5UnicodeData[iTbl]) >> (int)((int32)5)) + i;
        for (;;) {
            if (!(i < (int32)128 && i < n)) {
                break;
            }
            (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aAscii + (uintptr)i))).Value = libc.Uint8FromInt32(bToken);
            goto _1;
_1:;
            i = i + 1;
        }
        iTbl = iTbl + 1;
    }
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aAscii))).Value = (uint8)0;
}

internal static int32 /*r*/ _sqlite3Fts5UnicodeCategory(ref libc.TLS tls, Tu32 iCode) {
    int32 iHi = default;
    int32 iLo = default;
    int32 iRes = default;
    int32 iTest = default;
    int32 ret = default;
    int32 v1 = default;
    Tu16 iKey = default;
    _ = iHi;
    _ = iKey;
    _ = iLo;
    _ = iRes;
    _ = iTest;
    _ = ret;
    _ = v1;
    iRes = -(int32)1;
    if (iCode >= libc.Uint32FromInt32(libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(20))))) {
        return 0;
    }
    iLo = libc.Int32FromUint16(_aFts5UnicodeBlock[(iCode >> (int)((int32)16))]);
    iHi = libc.Int32FromUint16(_aFts5UnicodeBlock[(uint32)1 + (iCode >> (int)(libc.Int32FromInt32(16)))]);
    iKey = (uint16)((Tu32)(iCode & libc.Uint32FromInt32(0xFFFF)));
    while (iHi > iLo) {
        iTest = (iHi + iLo) / (int32)2;
        if (libc.Int32FromUint16(iKey) >= libc.Int32FromUint16(_aFts5UnicodeMap[iTest])) {
            iRes = iTest;
            iLo = iTest + (int32)1;
        } else {
            iHi = iTest;
        }
    }
    if (iRes < 0) {
        return 0;
    }
    if (libc.Int32FromUint16(iKey) >= libc.Int32FromUint16(_aFts5UnicodeMap[iRes]) + (libc.Int32FromUint16(_aFts5UnicodeData[iRes]) >> (int)((int32)5))) {
        return 0;
    }
    ret = libc.Int32FromUint16(_aFts5UnicodeData[iRes]) & (int32)0x1F;
    if (ret != (int32)30) {
        return ret;
    }
    if (((libc.Int32FromUint16(iKey) - libc.Int32FromUint16(_aFts5UnicodeMap[iRes])) & (int32)0x01) != 0) {
        v1 = (int32)5;
    } else {
        v1 = (int32)9;
    }
    return v1;
}

internal static int32 /*r*/ _sqlite3Fts5UnicodeIsdiacritic(ref libc.TLS tls, int32 c) {
    uint32 mask0 = default;
    uint32 mask1 = default;
    uint32 v1 = default;
    _ = mask0;
    _ = mask1;
    _ = v1;
    mask0 = (uint32)0x08029FDF;
    mask1 = (uint32)0x000361F8;
    if (c < (int32)768 || c > (int32)817) {
        return 0;
    }
    if (c < libc.Int32FromInt32(768) + libc.Int32FromInt32(32)) {
        v1 = mask0 & (libc.Uint32FromInt32(1).Lsh((int64)(c - (int32)768)));
    } else {
        v1 = mask1 & (libc.Uint32FromInt32(1).Lsh((int64)(c - (int32)768 - (int32)32)));
    }
    return libc.Int32FromUint32(v1);
}

internal static Tu8 /*r*/ _sqlite3GetBoolean(ж<libc.TLS> tls, uintptr z, Tu8 dflt) {
    return libc.BoolUint8(libc.Int32FromUint8(_getSafetyLevel(tls, z, (int32)1, dflt)) != 0);
}

internal static Ti64 /*r*/ _sqlite3GetToken(ref libc.TLS tls, uintptr z, @unsafe.Pointer tokenType) {
    int32 c = default;
    int32 delim = default;
    int32 v3 = default;
    Ti64 i = default;
    Ti64 n = default;
    bool v6 = default;
    _ = c;
    _ = delim;
    _ = i;
    _ = n;
    _ = v3;
    _ = v6;
    var exprᴛ1 = libc.Int32FromUint8(_aiClass[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))]);
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)CC_SPACE) { matchᴛ1 = true;
        i = (int64)1;
        for (;;) {
            if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x01) != 0)) {
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_SPACE;
        return i;
    }
    if (exprᴛ1 == (int32)CC_MINUS) { matchᴛ1 = true;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)'-') {
            i = (int64)2;
            for (;;) {
                v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)));
                c = v3;
                if (!(v3 != 0 && c != (int32)'\n')) {
                    break;
                }
                goto _2;
_2:;
                i = i + 1;
            }
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_COMMENT;
            return i;
        } else {
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)'>') {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_PTR;
                return (int64)((int32)2 + libc.BoolInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 2))) == (int32)'>'));
            }
        }
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_MINUS;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_LP) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_LP;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_RP) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_RP;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_SEMI) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_SEMI;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_PLUS) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_PLUS;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_STAR) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_STAR;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_SLASH) { matchᴛ1 = true;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) != (int32)'*' || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 2))) == 0) {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_SLASH;
            return (int64)1;
        }
        i = (int64)3;
        c = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 2)));
        for (;;) {
            v6 = c != (int32)'*' || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) != (int32)'/'; if (v6) {
                v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)));
                c = v3;
            }
            if (!(v6 && v3 != 0)) {
                break;
            }
            goto _4;
_4:;
            i = i + 1;
        }
        if (c != 0) {
            i = i + 1;
        }
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_COMMENT;
        return i;
    }
    if (exprᴛ1 == (int32)CC_PERCENT) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_REM;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_EQ) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_EQ;
        return (int64)((int32)1 + libc.BoolInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)'='));
    }
    if (exprᴛ1 == (int32)CC_LT) { matchᴛ1 = true;
        v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1)));
        c = v3;
        if (v3 == (int32)'=') {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_LE;
            return (int64)2;
        } else {
            if (c == (int32)'>') {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_NE;
                return (int64)2;
            } else {
                if (c == (int32)'<') {
                    (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_LSHIFT;
                    return (int64)2;
                } else {
                    (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_LT;
                    return (int64)1;
                }
            }
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_GT) { matchᴛ1 = true;
        v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1)));
        c = v3;
        if (v3 == (int32)'=') {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_GE;
            return (int64)2;
        } else {
            if (c == (int32)'>') {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_RSHIFT;
                return (int64)2;
            } else {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_GT;
                return (int64)1;
            }
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_BANG) { matchᴛ1 = true;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) != (int32)'=') {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
            return (int64)1;
        } else {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_NE;
            return (int64)2;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_PIPE) { matchᴛ1 = true;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) != (int32)'|') {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_BITOR;
            return (int64)1;
        } else {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_CONCAT;
            return (int64)2;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_COMMA) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_COMMA;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_AND) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_BITAND;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_TILDA) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_BITNOT;
        return (int64)1;
    }
    if (exprᴛ1 == (int32)CC_QUOTE) { matchᴛ1 = true;
        delim = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z)));
        i = (int64)1;
        for (;;) {
            v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)));
            c = v3;
            if (!(v3 != 0)) {
                break;
            }
            if (c == delim) {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(i + (int64)1)))) == delim) {
                    i = i + 1;
                } else {
                    break;
                }
            }
            goto _9;
_9:;
            i = i + 1;
        }
        if (c == (int32)'\'') {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_STRING;
            return i + (int64)1;
        } else {
            if (c != 0) {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ID;
                return i + (int64)1;
            } else {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
                return i;
            }
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_DOT) { matchᴛ1 = true;
        if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))]) & libc.Int32FromInt32(0x04)) != 0)) {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_DOT;
            return (int64)1;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_DIGIT) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_INTEGER;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z))) == (int32)'0' && (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)'x' || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)'X') && (libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 2))]) & (int32)0x08) != 0) {
            i = (int64)3;
            for (;;) {
                if (!((int32)1 != 0)) {
                    break;
                }
                if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x08) == 0) {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'_') {
                        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_QNUMBER;
                    } else {
                        break;
                    }
                }
                goto _11;
_11:;
                i = i + 1;
            }
        } else {
            i = 0;
            for (;;) {
                if (!((int32)1 != 0)) {
                    break;
                }
                if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x04) == 0) {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'_') {
                        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_QNUMBER;
                    } else {
                        break;
                    }
                }
                goto _12;
_12:;
                i = i + 1;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'.') {
                if (~@unsafe.Pointer.To<int32>(tokenType) == (int32)TK_INTEGER) {
                    (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_FLOAT;
                }
                i = i + 1;
                for (;;) {
                    if (!((int32)1 != 0)) {
                        break;
                    }
                    if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x04) == 0) {
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'_') {
                            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_QNUMBER;
                        } else {
                            break;
                        }
                    }
                    goto _13;
_13:;
                    i = i + 1;
                }
            }
            if ((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'e' || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'E') && ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(i + (int64)1)))]) & (int32)0x04) != 0 || (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(i + (int64)1)))) == (int32)'+' || libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(i + (int64)1)))) == (int32)'-') && (libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(i + (int64)2)))]) & (int32)0x04) != 0)) {
                if (~@unsafe.Pointer.To<int32>(tokenType) == (int32)TK_INTEGER) {
                    (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_FLOAT;
                }
                i = i + (int64)2;
                for (;;) {
                    if (!((int32)1 != 0)) {
                        break;
                    }
                    if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x04) == 0) {
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) == (int32)'_') {
                            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_QNUMBER;
                        } else {
                            break;
                        }
                    }
                    goto _14;
_14:;
                    i = i + 1;
                }
            }
        }
        while ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x46) != 0) {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
            i = i + 1;
        }
        return i;
    }
    if (exprᴛ1 == (int32)CC_QUOTE2) { matchᴛ1 = true;
        i = (int64)1;
        c = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z)));
        for (;;) {
            v6 = c != (int32)']'; if (v6) {
                v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)));
                c = v3;
            }
            if (!(v6 && v3 != 0)) {
                break;
            }
            goto _15;
_15:;
            i = i + 1;
        }
        if (c == (int32)']') {
            v3 = (int32)TK_ID;
        } else {
            v3 = (int32)TK_ILLEGAL;
        }
        (@unsafe.Pointer.To<int32>(tokenType)).Value = v3;
        return i;
    }
    if (exprᴛ1 == (int32)CC_VARNUM) { matchᴛ1 = true;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_VARIABLE;
        i = (int64)1;
        for (;;) {
            if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x04) != 0)) {
                break;
            }
            goto _19;
_19:;
            i = i + 1;
        }
        return i;
    }
    if (exprᴛ1 == (int32)CC_DOLLAR) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_VARALPHA) { matchᴛ1 = true;
        n = 0;
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_VARIABLE;
        i = (int64)1;
        for (;;) {
            v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)));
            c = v3;
            if (!(v3 != 0)) {
                break;
            }
            if ((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt32(c)]) & (int32)0x46) != 0) {
                n = n + 1;
            } else {
                if (c == (int32)'(' && n > 0) {
                    for (;;) {
                        i = i + 1;
                        goto _23;
_23:;
                        v3 = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)));
                        c = v3;
                        if (!(v3 != 0 && !((libc.Int32FromUint8(_sqlite3CtypeMap[libc.Uint8FromInt32(c)]) & libc.Int32FromInt32(0x01)) != 0) && c != (int32)')')) {
                            break;
                        }
                    }
                    if (c == (int32)')') {
                        i = i + 1;
                    } else {
                        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
                    }
                    break;
                } else {
                    if (c == (int32)':' && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)(i + (int64)1)))) == (int32)':') {
                        i = i + 1;
                    } else {
                        break;
                    }
                }
            }
            goto _20;
_20:;
            i = i + 1;
        }
        if (n == 0) {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
        }
        return i;
    }
    if (exprᴛ1 == (int32)CC_KYWD0) { matchᴛ1 = true;
        do {
            if (libc.Int32FromUint8(_aiClass[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))]) > (int32)CC_KYWD) {
                i = (int64)1;
                break;
            }
            i = (int64)2;
            for (;;) {
                if (!(libc.Int32FromUint8(_aiClass[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) <= (int32)CC_KYWD)) {
                    break;
                }
                goto _24;
_24:;
                i = i + 1;
            }
            if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x46) != 0) {
                i = i + 1;
                break;
            }
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ID;
            return _keywordCode(ref tls, z, i, tokenType);
        } while (false);
    }
    else if (exprᴛ1 == CC_X) { matchᴛ1 = true;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)'\'') {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_BLOB;
            i = (int64)2;
            for (;;) {
                if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x08) != 0)) {
                    break;
                }
                goto _25;
_25:;
                i = i + 1;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) != (int32)'\'' || i % (int64)2 != 0) {
                (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
                while (~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)) != 0 && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))) != (int32)'\'') {
                    i = i + 1;
                }
            }
            if (~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i)) != 0) {
                i = i + 1;
            }
            return i;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_KYWD) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)CC_ID) { matchᴛ1 = true;
        i = (int64)1;
    }
    else if (exprᴛ1 == (int32)CC_BOM) { matchᴛ1 = true;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 1))) == (int32)0xbb && libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + 2))) == (int32)0xbf) {
            (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_SPACE;
            return (int64)3;
        }
        i = (int64)1;
    }
    else if (exprᴛ1 == (int32)CC_NUL) {
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
        return 0;
    }
    else if (!matchᴛ1) { /* default: */
        (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ILLEGAL;
        return (int64)1;
    }

    while ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z + (uintptr)i))]) & (int32)0x46) != 0) {
        i = i + 1;
    }
    (@unsafe.Pointer.To<int32>(tokenType)).Value = (int32)TK_ID;
    return i;
}

internal static Tu8 /*r*/ _sqlite3GetVarint32(ref libc.TLS tls, uintptr p, @unsafe.Pointer v) {
    Tu8 n = default;
    ref var v64 = ref heap(new Tu64(), out var Ꮡv64);
    _ = n;
    _ = v64;
    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 1))) & (int32)0x80) == 0) {
        (@unsafe.Pointer.To<Tu32>(v)).Value = libc.Uint32FromInt32((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p))) & (int32)0x7f) << (int)((int32)7) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 1))));
        return (uint8)2;
    }
    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 2))) & (int32)0x80) == 0) {
        (@unsafe.Pointer.To<Tu32>(v)).Value = libc.Uint32FromInt32((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p))) & (int32)0x7f) << (int)((int32)14) | (libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 1))) & (int32)0x7f) << (int)((int32)7) | libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p + 2))));
        return (uint8)3;
    }
    n = _sqlite3GetVarint(ref tls, p, @unsafe.Pointer.FromPinnedBox(Ꮡv64));
    if ((Tu64)(v64 & (libc.Uint64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(32))) - libc.Uint64FromInt32(1))) != v64) {
        (@unsafe.Pointer.To<Tu32>(v)).Value = (uint32)0xffffffffU;
    } else {
        (@unsafe.Pointer.To<Tu32>(v)).Value = (uint32)v64;
    }
    return n;
}

internal static Tu8 /*r*/ _sqlite3HexToInt(ref libc.TLS tls, int32 h) {
    h = h + (int32)9 * ((int32)1 & (h >> (int)((int32)6)));
    return libc.Uint8FromInt32(h & libc.Int32FromInt32(0xf));
}

internal static int32 /*r*/ _sqlite3IsIdChar(ref libc.TLS tls, Tu8 c) {
    return libc.BoolInt32((libc.Int32FromUint8(_sqlite3CtypeMap[c]) & (int32)0x46) != 0);
}

internal static uintptr /*r*/ _sqlite3LocateCollSeq(ж<libc.TLS> tls, uintptr pParse, uintptr zName) {
    uintptr db = default;
    uintptr pColl = default;
    Tu8 enc = default;
    Tu8 initbusy = default;
    _ = db;
    _ = enc;
    _ = initbusy;
    _ = pColl;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    enc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc;
    initbusy = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy;
    pColl = _sqlite3FindCollSeq(tls, db, enc, zName, libc.Int32FromUint8(initbusy));
    if (!(initbusy != 0) && (!(pColl != 0) || !((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxCmp != 0))) {
        pColl = _sqlite3GetCollSeq(tls, pParse, enc, pColl, zName);
    }
    return pColl;
}

internal static TLogEst /*r*/ _sqlite3LogEstAdd(ref libc.TLS tls, TLogEst a, TLogEst b) {
    TLogEst r = default;

    if ((int32)a >= (int32)b) {
        if ((int32)a > (int32)b + (int32)49) {
            return a;
        }
        if ((int32)a > (int32)b + (int32)31) {
            return (int16)((int32)a + (int32)1);
        }
        return (int16)((int32)a + libc.Int32FromUint8(_x[(int32)a - (int32)b]));
    } else {
        if ((int32)b > (int32)a + (int32)49) {
            return b;
        }
        if ((int32)b > (int32)a + (int32)31) {
            return (int16)((int32)b + (int32)1);
        }
        return (int16)((int32)b + libc.Int32FromUint8(_x[(int32)b - (int32)a]));
    }
    return r;
}

internal static Tu64 /*r*/ _sqlite3LogEstToInt(ref libc.TLS tls, TLogEst x) {
    Tu64 n = default;
    uint64 v1 = default;
    _ = n;
    _ = v1;
    n = libc.Uint64FromInt32((int32)x % (int32)10);
    x = (int16)(quo((int32)x, libc.Int32FromInt32(10)));
    if (n >= (uint64)5) {
        n = n - (uint64)2;
    } else {
        if (n >= (uint64)1) {
            n = n - (uint64)1;
        }
    }
    if ((int32)x > (int32)60) {
        return libc.Uint64FromInt64(libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))));
    }
    if ((int32)x >= (int32)3) {
        v1 = (n + (uint64)8) << (int)((int32)x - (int32)3);
    } else {
        v1 = (n + (uint64)8) >> (int)((int32)3 - (int32)x);
    }
    return v1;
}

internal static int32 /*r*/ _sqlite3LookasideUsed(ref libc.TLS tls, uintptr db, @unsafe.Pointer pHighwater) {
    Tu32 nFree = default;
    Tu32 nInit = default;
    _ = nFree;
    _ = nInit;
    nInit = _countLookasideSlots(ref tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpInit);
    nFree = _countLookasideSlots(ref tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpFree);
    nInit = nInit + _countLookasideSlots(ref tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpSmallInit);
    nFree = nFree + _countLookasideSlots(ref tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FpSmallFree);
    if (pHighwater != nil) {
        (@unsafe.Pointer.To<int32>(pHighwater)).Value = libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FnSlot - nInit);
    }
    return libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Flookaside.FnSlot - (nInit + nFree));
}

internal static uintptr /*r*/ _sqlite3Malloc(ж<libc.TLS> tls, Tu64 n) {
    ref var p = ref heap(new uintptr(), out var Ꮡp);
    _ = p;
    if (n == (uint64)0 || n > (uint64)SQLITE_MAX_ALLOCATION_SIZE) {
        p = (uintptr)0;
    } else {
        if (_sqlite3Config.FbMemstat != 0) {
            Xsqlite3_mutex_enter(tls, _mem0.Fmutex);
            _mallocWithAlarm(tls, libc.Int32FromUint64(n), @unsafe.Pointer.FromBox(Ꮡp));
            Xsqlite3_mutex_leave(tls, _mem0.Fmutex);
        } else {
            p = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, uintptr>>(_sqlite3Config.Fm.FxMalloc)(tls, libc.Int32FromUint64(n));
        }
    }
    return p;
}

internal static int32 /*r*/ _sqlite3MemCompare(ж<libc.TLS> tls, uintptr pMem1, uintptr pMem2, uintptr pColl) {
    int32 combined_flags = default;
    int32 f1 = default;
    int32 f2 = default;
    _ = combined_flags;
    _ = f1;
    _ = f2;
    f1 = libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fflags);
    f2 = libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem2)).Value.Fflags);
    combined_flags = f1 | f2;
    if ((combined_flags & (int32)MEM_Null) != 0) {
        return (f2 & (int32)MEM_Null) - (f1 & (int32)MEM_Null);
    }
    if ((combined_flags & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_Real) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
        if ((f1 & f2 & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
            if (~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem1) < ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem2)) {
                return -(int32)1;
            }
            if (~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem1) > ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem2)) {
                return +libc.Int32FromInt32(1);
            }
            return 0;
        }
        if ((f1 & f2 & (int32)MEM_Real) != 0) {
            if (~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem1) < ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem2)) {
                return -(int32)1;
            }
            if (~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem1) > ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem2)) {
                return +libc.Int32FromInt32(1);
            }
            return 0;
        }
        if ((f1 & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
            if ((f2 & (int32)MEM_Real) != 0) {
                return _sqlite3IntFloatCompare(tls, ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem1), ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem2));
            } else {
                if ((f2 & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
                    if (~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem1) < ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem2)) {
                        return -(int32)1;
                    }
                    if (~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem1) > ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem2)) {
                        return +libc.Int32FromInt32(1);
                    }
                    return 0;
                } else {
                    return -(int32)1;
                }
            }
        }
        if ((f1 & (int32)MEM_Real) != 0) {
            if ((f2 & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
                return -_sqlite3IntFloatCompare(tls, ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem2), ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem1));
            } else {
                return -(int32)1;
            }
        }
        return +libc.Int32FromInt32(1);
    }
    if ((combined_flags & (int32)MEM_Str) != 0) {
        if ((f1 & (int32)MEM_Str) == 0) {
            return (int32)1;
        }
        if ((f2 & (int32)MEM_Str) == 0) {
            return -(int32)1;
        }
        if (pColl != 0) {
            return _vdbeCompareMemString(tls, pMem1, pMem2, pColl, nil);
        }
    }
    return _sqlite3BlobCompare(tls, pMem1, pMem2);
}

internal static void _sqlite3OpenTable(ж<libc.TLS> tls, uintptr pParse, int32 iCur, int32 iDb, uintptr pTab, int32 opcode) {
    uintptr pPk = default;
    uintptr v = default;
    int32 v1 = default;
    _ = pPk;
    _ = v;
    _ = v1;
    v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
    if (!((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.FnoSharedCache != 0)) {
        if (opcode == (int32)OP_OpenWrite) {
            v1 = (int32)1;
        } else {
            v1 = 0;
        }
        _sqlite3TableLock(tls, pParse, iDb, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum, libc.Uint8FromInt32(v1), (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName);
    }
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0) {
        _sqlite3VdbeAddOp4Int(tls, v, opcode, iCur, libc.Int32FromUint32((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.Ftnum), iDb, (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FnNVCol);
    } else {
        pPk = _sqlite3PrimaryKeyIndex(ref (tls).DerefOrNull(), pTab);
        _sqlite3VdbeAddOp3(tls, v, opcode, iCur, libc.Int32FromUint32((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pPk)).Value.Ftnum), iDb);
        _sqlite3VdbeSetP4KeyInfo(tls, pParse, pPk);
    }
}

internal static int32 /*r*/ _sqlite3OsOpenMalloc(ж<libc.TLS> tls, uintptr pVfs, uintptr zFile, @unsafe.Pointer ppFile, int32 flags, uintptr pOutFlags) {
    uintptr pFile = default;
    int32 rc = default;
    _ = pFile;
    _ = rc;
    pFile = _sqlite3MallocZero(tls, libc.Uint64FromInt32((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)pVfs)).Value.FszOsFile));
    if (pFile != 0) {
        rc = _sqlite3OsOpen(tls, pVfs, zFile, pFile, flags, pOutFlags);
        if (rc != SQLITE_OK) {
            Xsqlite3_free(tls, pFile);
            (@unsafe.Pointer.To<uintptr>(ppFile)).Value = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<uintptr>(ppFile)).Value = pFile;
        }
    } else {
        (@unsafe.Pointer.To<uintptr>(ppFile)).Value = (uintptr)0;
        rc = (int32)SQLITE_NOMEM;
    }
    return rc;
}

internal static uintptr /*r*/ _sqlite3PExprIs(ж<libc.TLS> tls, uintptr pParse, int32 op, uintptr pLeft, uintptr pRight) {
    int32 v1 = default;
    _ = v1;
    if (pRight != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pRight)).Value.Fop) == (int32)TK_NULL) {
        _sqlite3ExprDeferredDelete(tls, pParse, pRight);
        if (op == (int32)TK_IS) {
            v1 = (int32)TK_ISNULL;
        } else {
            v1 = (int32)TK_NOTNULL;
        }
        return _sqlite3PExprIsNull(tls, pParse, v1, pLeft);
    }
    return _sqlite3PExpr(tls, pParse, op, pLeft, pRight);
}

internal static uintptr /*r*/ _sqlite3PExprIsNull(ж<libc.TLS> tls, uintptr pParse, int32 op, uintptr pLeft) {
    uintptr p = default;
    _ = p;
    p = pLeft;
    while (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UPLUS || libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_UMINUS) {
        p = (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft;
    }
    var exprᴛ1 = libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop);
    var matchᴛ1 = false;
    if (exprᴛ1 == (int32)TK_INTEGER) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_STRING) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_FLOAT) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)TK_BLOB) {
        _sqlite3ExprDeferredDelete(tls, pParse, pLeft);
        return _sqlite3ExprInt32(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, libc.BoolInt32(op == (int32)TK_NOTNULL));
    }
    if (!matchᴛ1) { /* default: */
        do {
            break;
        } while (false);
    }

    return _sqlite3PExpr(tls, pParse, op, pLeft, (uintptr)0);
}

internal static int32 /*r*/ _sqlite3PagerBegin(ж<libc.TLS> tls, uintptr pPager, int32 exFlag, int32 subjInMemory) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
        return (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode;
    }
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsubjInMemory = libc.Uint8FromInt32(subjInMemory);
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_READER) {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
            if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0 && _sqlite3WalExclusiveMode(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, -(int32)1) != 0) {
                rc = _pagerLockDb(tls, pPager, (int32)EXCLUSIVE_LOCK);
                if (rc != SQLITE_OK) {
                    return rc;
                }
                _sqlite3WalExclusiveMode(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, (int32)1);
            }
            rc = _sqlite3WalBeginWriteTransaction(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal);
        } else {
            rc = _pagerLockDb(tls, pPager, (int32)RESERVED_LOCK);
            if (rc == SQLITE_OK && exFlag != 0) {
                rc = _pager_wait_on_lock(tls, pPager, (int32)EXCLUSIVE_LOCK);
            }
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_WRITER_LOCKED;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbHintSize = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbOrigSize = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize;
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff = 0;
        }
    }
    return rc;
}

internal static void _sqlite3PagerClearCache(ж<libc.TLS> tls, uintptr pPager) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile) == 0) {
        _pager_reset(tls, pPager);
    }
}

internal static int32 /*r*/ _sqlite3PagerClose(ж<libc.TLS> tls, uintptr pPager, uintptr db) {
    uintptr a = default;
    uintptr pTmp = default;
    _ = a;
    _ = pTmp;
    pTmp = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace;
    _sqlite3BeginBenignMalloc(tls);
    _pagerFreeMapHdrs(tls, pPager);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode = (uint8)0;
    a = (uintptr)0;
    if (db != 0 && (uint64)0 == (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & (uint64)SQLITE_NoCkptOnClose) && SQLITE_OK == _databaseIsUnmoved(tls, pPager)) {
        a = pTmp;
    }
    _sqlite3WalClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, db, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FwalSyncFlags), (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, a);
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal = (uintptr)0;
    _pager_reset(tls, pPager);
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmemDb != 0) {
        _pager_unlock(tls, pPager);
    } else {
        if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0) {
            _pager_error(ref (tls).DerefOrNull(), pPager, _pagerSyncHotJournal(tls, pPager));
        }
        _pagerUnlockAndRollback(tls, pPager);
    }
    _sqlite3EndBenignMalloc(tls);
    _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
    _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd);
    _sqlite3PageFree(tls, pTmp);
    _sqlite3PcacheClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
    Xsqlite3_free(tls, pPager);
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3PagerCloseWal(ж<libc.TLS> tls, uintptr pPager, uintptr db) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        int32 _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != 0)) {
            (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = 0;
            rc = _pagerLockDb(tls, pPager, (int32)SHARED_LOCK);
            if (rc == SQLITE_OK) {
                rc = _sqlite3OsAccess(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzWal, SQLITE_ACCESS_EXISTS, bp);
            }
            if (rc == SQLITE_OK && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != 0) {
                rc = _pagerOpenWal(tls, pPager);
            }
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != 0) {
            rc = _pagerExclusiveLock(tls, pPager);
            if (rc == SQLITE_OK) {
                rc = _sqlite3WalClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, db, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FwalSyncFlags), (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace);
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal = (uintptr)0;
                _pagerFixMaplimit(tls, pPager);
                if (rc != 0 && !((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0)) {
                    _pagerUnlockDb(tls, pPager, (int32)SHARED_LOCK);
                }
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3PagerCommitPhaseOne(ж<libc.TLS> tls, uintptr pPager, uintptr zSuper, int32 noSync) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        TPgno nNew = default;
        uintptr pList = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = nNew;
        _ = pList;
        _ = rc;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
            r = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode; goto ᒐdone;
        }
        if (_sqlite3FaultSim(tls, (int32)400) != 0) {
            r = (int32)SQLITE_IOERR; goto ᒐdone;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) < (int32)PAGER_WRITER_CACHEMOD) {
            r = SQLITE_OK; goto ᒐdone;
        }
        if (0 == _pagerFlushOnCommit(ref (tls).DerefOrNull(), pPager, (int32)1)) {
            _sqlite3BackupRestart(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpBackup);
        } else {
            if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
                pList = _sqlite3PcacheDirtyList(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
                if (pList == (uintptr)0) {
                    rc = _sqlite3PagerGet(tls, pPager, (uint32)1, bp, 0);
                    pList = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp));
                    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pList)).Value.FpDirty = (uintptr)0;
                }
                if (pList != 0) {
                    rc = _pagerWalFrames(tls, pPager, pList, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize, (int32)1);
                }
                _sqlite3PagerUnref(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)));
                if (rc == SQLITE_OK) {
                    _sqlite3PcacheCleanAll(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
                }
            } else {
                rc = _pager_incr_changecounter(tls, pPager, 0);
                if (rc != SQLITE_OK) {
                    goto commit_phase_one_exit;
                }
                rc = _writeSuperJournal(tls, pPager, zSuper);
                if (rc != SQLITE_OK) {
                    goto commit_phase_one_exit;
                }
                rc = _syncJournal(tls, pPager, 0);
                if (rc != SQLITE_OK) {
                    goto commit_phase_one_exit;
                }
                pList = _sqlite3PcacheDirtyList(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
                if (true) {
                    rc = _pager_write_pagelist(tls, pPager, pList);
                }
                if (rc != SQLITE_OK) {
                    goto commit_phase_one_exit;
                }
                _sqlite3PcacheCleanAll(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpPCache);
                if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize > (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbFileSize) {
                    nNew = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize - libc.BoolUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize == (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FlckPgno);
                    rc = _pager_truncate(tls, pPager, nNew);
                    if (rc != SQLITE_OK) {
                        goto commit_phase_one_exit;
                    }
                }
                if (!(noSync != 0)) {
                    rc = _sqlite3PagerSync(tls, pPager, zSuper);
                }
            }
        }
        goto commit_phase_one_exit;
commit_phase_one_exit:;
        if (rc == SQLITE_OK && !((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != libc.UintptrFromInt32(0))) {
            (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_WRITER_FINISHED;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3PagerCommitPhaseTwo(ж<libc.TLS> tls, uintptr pPager) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
        return (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode;
    }
    (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FiDataVersion = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FiDataVersion + 1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_WRITER_LOCKED && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_PERSIST) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_READER;
        return SQLITE_OK;
    }
    rc = _pager_end_transaction(tls, pPager, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsetSuper), (int32)1);
    return _pager_error(ref (tls).DerefOrNull(), pPager, rc);
}

internal static int32 /*r*/ _sqlite3PagerGetJournalMode(ref libc.TLS tls, uintptr pPager) {
    return libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode);
}

internal static int32 /*r*/ _sqlite3PagerLockingMode(ref libc.TLS tls, uintptr pPager, int32 eMode) {
    if (eMode >= 0 && !((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FtempFile != 0) && !(_sqlite3WalHeapMemory(ref tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal) != 0)) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode = libc.Uint8FromInt32(eMode);
    }
    return libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode);
}

internal static int32 /*r*/ _sqlite3PagerOkToChangeJournalMode(ref libc.TLS tls, uintptr pPager) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) >= (int32)PAGER_WRITER_CACHEMOD) {
        return 0;
    }
    if ((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != (uintptr)0 && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff > 0) {
        return 0;
    }
    return (int32)1;
}

internal static void _sqlite3PagerPagecount(ref libc.TLS tls, uintptr pPager, @unsafe.Pointer pnPage) {
    (@unsafe.Pointer.To<int32>(pnPage)).Value = libc.Int32FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize);
}

internal static int32 /*r*/ _sqlite3PagerRollback(ж<libc.TLS> tls, uintptr pPager) {
    int32 eState = default;
    int32 rc = default;
    int32 rc2 = default;
    _ = eState;
    _ = rc;
    _ = rc2;
    rc = SQLITE_OK;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_ERROR) {
        return (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) <= (int32)PAGER_READER) {
        return SQLITE_OK;
    }
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != (uintptr)0) {
        rc = _sqlite3PagerSavepoint(tls, pPager, (int32)SAVEPOINT_ROLLBACK, -(int32)1);
        rc2 = _pager_end_transaction(tls, pPager, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsetSuper), 0);
        if (rc == SQLITE_OK) {
            rc = rc2;
        }
    } else {
        if (!((@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd)).Value.FpMethods != libc.UintptrFromInt32(0)) || libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_WRITER_LOCKED) {
            eState = libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState);
            rc = _pager_end_transaction(tls, pPager, 0, 0);
            if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmemDb != 0) && eState > (int32)PAGER_WRITER_LOCKED) {
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode = (int32)SQLITE_ABORT;
                (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState = (uint8)PAGER_ERROR;
                _setGetterMethod(ref (tls).DerefOrNull(), pPager);
                return rc;
            }
        } else {
            rc = _pager_playback(tls, pPager, 0);
        }
    }
    return _pager_error(ref (tls).DerefOrNull(), pPager, rc);
}

internal static int32 /*r*/ _sqlite3PagerSetJournalMode(ж<libc.TLS> tls, uintptr pPager, int32 eMode) {
    Tu8 eOld = default;
    int32 rc = default;
    int32 state = default;
    _ = eOld;
    _ = rc;
    _ = state;
    eOld = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FmemDb != 0) {
        if (eMode != (int32)PAGER_JOURNALMODE_MEMORY && eMode != (int32)PAGER_JOURNALMODE_OFF) {
            eMode = libc.Int32FromUint8(eOld);
        }
    }
    if (eMode != libc.Int32FromUint8(eOld)) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode = libc.Uint8FromInt32(eMode);
        if (!((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FexclusiveMode != 0) && (libc.Int32FromUint8(eOld) & (int32)5) == (int32)1 && (eMode & (int32)1) == 0) {
            _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeLock) >= (int32)RESERVED_LOCK) {
                _sqlite3OsDelete(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, 0);
            } else {
                rc = SQLITE_OK;
                state = libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState);
                if (state == PAGER_OPEN) {
                    rc = _sqlite3PagerSharedLock(tls, pPager);
                }
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FeState) == (int32)PAGER_READER) {
                    rc = _pagerLockDb(tls, pPager, (int32)RESERVED_LOCK);
                }
                if (rc == SQLITE_OK) {
                    _sqlite3OsDelete(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpVfs, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FzJournal, 0);
                }
                if (rc == SQLITE_OK && state == (int32)PAGER_READER) {
                    _pagerUnlockDb(tls, pPager, (int32)SHARED_LOCK);
                } else {
                    if (state == PAGER_OPEN) {
                        _pager_unlock(tls, pPager);
                    }
                }
            }
        } else {
            if (eMode == (int32)PAGER_JOURNALMODE_OFF || eMode == (int32)PAGER_JOURNALMODE_MEMORY) {
                _sqlite3OsClose(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd);
            }
        }
    }
    return libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode);
}

internal static int32 /*r*/ _sqlite3PagerSync(ж<libc.TLS> tls, uintptr pPager, uintptr zSuper) {
    uintptr pArg = default;
    int32 rc = default;
    _ = pArg;
    _ = rc;
    rc = SQLITE_OK;
    pArg = zSuper;
    rc = _sqlite3OsFileControl(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, (int32)SQLITE_FCNTL_SYNC, pArg);
    if (rc == (int32)SQLITE_NOTFOUND) {
        rc = SQLITE_OK;
    }
    if (rc == SQLITE_OK && !((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoSync != 0)) {
        rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Ffd, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsyncFlags));
    }
    return rc;
}

internal static void _sqlite3PagerUnrefNotNull(ж<libc.TLS> tls, uintptr pPg) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TDbPage>((@unsafe.Pointer)pPg)).Value.Fflags) & (int32)PGHDR_MMAP) != 0) {
        _pagerReleaseMapPage(tls, pPg);
    } else {
        _sqlite3PcacheRelease(tls, pPg);
    }
}

internal static int32 /*r*/ _sqlite3PagerWrite(ж<libc.TLS> tls, uintptr pPg) {
    int32 r = default;

    uintptr pPager = default;
    _ = pPager;
    pPager = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fflags) & (int32)PGHDR_WRITEABLE) != 0 && (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FdbSize >= (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno) {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSavepoint != 0) {
            return _subjournalPageIfRequired(tls, pPg);
        }
        return SQLITE_OK;
    } else {
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode != 0) {
            return (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FerrCode;
        } else {
            if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsectorSize > libc.Uint32FromInt64((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize)) {
                return _pagerWriteLargeSector(tls, pPg);
            } else {
                return _pager_write(tls, pPg);
            }
        }
    }
    return r;
}

internal static int32 /*r*/ _sqlite3ParserFallback(ref libc.TLS tls, int32 iToken) {
    return libc.Int32FromUint16(_yyFallback[iToken]);
}

internal static void _sqlite3PcacheDrop(ж<libc.TLS> tls, uintptr p) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fflags) & (int32)PGHDR_DIRTY) != 0) {
        _pcacheManageDirtyList(ref (tls).DerefOrNull(), p, (uint8)PCACHE_DIRTYLIST_REMOVE);
    }
    (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)(@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpCache)).Value.FnRefSum = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)(@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpCache)).Value.FnRefSum - 1;
    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, int32>>(_sqlite3Config.Fpcache2.FxUnpin)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)(@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpCache)).Value.FpCache, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpPage, (int32)1);
}

internal static uintptr /*r*/ _sqlite3PcacheFetch(ж<libc.TLS> tls, uintptr pCache, TPgno pgno, int32 createFlag) {
    int32 eCreate = default;
    uintptr pRes = default;
    _ = eCreate;
    _ = pRes;
    eCreate = createFlag & libc.Int32FromUint8((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FeCreate);
    pRes = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uint32, int32, uintptr>>(_sqlite3Config.Fpcache2.FxFetch)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache, pgno, eCreate);
    return pRes;
}

internal static int32 /*r*/ _sqlite3PcacheFetchStress(ж<libc.TLS> tls, uintptr pCache, TPgno pgno, @unsafe.Pointer ppPage) {
    uintptr pPg = default;
    int32 rc = default;
    int32 v3 = default;
    _ = pPg;
    _ = rc;
    _ = v3;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FeCreate) == (int32)2) {
        return 0;
    }
    if (_sqlite3PcachePagecount(tls, pCache) > (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FszSpill) {
        pPg = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpSynced;
        for (;;) {
            if (!(pPg != 0 && ((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FnRef != 0 || (libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fflags) & (int32)PGHDR_NEED_SYNC) != 0))) {
                break;
            }
            goto _1;
_1:;
            pPg = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpDirtyPrev;
        }
        (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpSynced = pPg;
        if (!(pPg != 0)) {
            pPg = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpDirtyTail;
            for (;;) {
                if (!(pPg != 0 && (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FnRef != 0)) {
                    break;
                }
                goto _2;
_2:;
                pPg = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpDirtyPrev;
            }
        }
        if (pPg != 0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>((@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FxStress)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpStress, pPg);
            if (rc != SQLITE_OK && rc != (int32)SQLITE_BUSY) {
                return rc;
            }
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppPage)).Value = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uint32, int32, uintptr>>(_sqlite3Config.Fpcache2.FxFetch)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache, pgno, (int32)2);
    if (~@unsafe.Pointer.To<uintptr>(ppPage) == (uintptr)0) {
        v3 = (int32)SQLITE_NOMEM;
    } else {
        v3 = SQLITE_OK;
    }
    return v3;
}

internal static void _sqlite3PcacheMove(ж<libc.TLS> tls, uintptr p, TPgno newPgno) {
    uintptr pCache = default;
    uintptr pOther = default;
    uintptr pXPage = default;
    _ = pCache;
    _ = pOther;
    _ = pXPage;
    pCache = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpCache;
    pOther = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uint32, int32, uintptr>>(_sqlite3Config.Fpcache2.FxFetch)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache, newPgno, 0);
    if (pOther != 0) {
        pXPage = (@unsafe.Pointer.To<Tsqlite3_pcache_page>((@unsafe.Pointer)pOther)).Value.FpExtra;
        (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pXPage)).Value.FnRef = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pXPage)).Value.FnRef + 1;
        (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FnRefSum = (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FnRefSum + 1;
        _sqlite3PcacheDrop(tls, pXPage);
    }
    GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, uintptr, uint32, uint32>>(_sqlite3Config.Fpcache2.FxRekey)(tls, (@unsafe.Pointer.To<TPCache>((@unsafe.Pointer)pCache)).Value.FpCache, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.FpPage, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fpgno, newPgno);
    (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fpgno = newPgno;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fflags) & (int32)PGHDR_DIRTY) != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)p)).Value.Fflags) & (int32)PGHDR_NEED_SYNC) != 0) {
        _pcacheManageDirtyList(ref (tls).DerefOrNull(), p, (uint8)PCACHE_DIRTYLIST_FRONT);
    }
}

internal static int32 /*r*/ _sqlite3PcacheReleaseMemory(ж<libc.TLS> tls, int32 nReq) {
    int32 nFree = default;
    uintptr p = default;
    uintptr v1 = default;
    bool v2 = default;
    _ = nFree;
    _ = p;
    _ = v1;
    _ = v2;
    nFree = 0;
    if (_sqlite3Config.FpPage == (uintptr)0) {
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(uintptr)Ꮡ_pcache1_g)).Value.Fmutex);
        for (;;) {
            v2 = nReq < 0 || nFree < nReq; if (v2) {
                v1 = _pcache1_g.Fgrp.Flru.FpLruPrev;
                p = v1;
            }
            if (!(v2 && v1 != (uintptr)0 && libc.Int32FromUint16((@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)p)).Value.FisAnchor) == 0)) {
                break;
            }
            nFree = nFree + _pcache1MemSize(tls, (@unsafe.Pointer.To<TPgHdr1>((@unsafe.Pointer)p)).Value.Fpage.FpBuf);
            _pcache1PinPage(ref (tls).DerefOrNull(), p);
            _pcache1RemoveFromHash(tls, p, (int32)1);
        }
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TPGroup>((@unsafe.Pointer)(uintptr)Ꮡ_pcache1_g)).Value.Fmutex);
    }
    return nFree;
}

internal static uintptr /*r*/ _sqlite3PragmaVtabRegister(ж<libc.TLS> tls, uintptr db, uintptr zName) {
    uintptr pName = default;
    _ = pName;
    pName = _pragmaLocate(tls, zName + (uintptr)7);
    if (pName == (uintptr)0) {
        return (uintptr)0;
    }
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TPragmaName>((@unsafe.Pointer)pName)).Value.FmPragFlg) & (libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_Result1))) == 0) {
        return (uintptr)0;
    }
    return _sqlite3VtabCreateModule(tls, db, zName, (uintptr)Ꮡ_pragmaVtabModule, pName, (uintptr)0);
}

internal static uintptr /*r*/ _sqlite3Realloc(ж<libc.TLS> tls, uintptr pOld, Tu64 nBytes) {
    int32 nDiff = default;
    int32 nNew = default;
    int32 nOld = default;
    Tsqlite3_int64 nUsed = default;
    Tsqlite3_int64 v1 = default;
    uintptr pNew = default;
    bool v2 = default;
    _ = nDiff;
    _ = nNew;
    _ = nOld;
    _ = nUsed;
    _ = pNew;
    _ = v1;
    _ = v2;
    if (pOld == (uintptr)0) {
        return _sqlite3Malloc(tls, nBytes);
    }
    if (nBytes == (uint64)0) {
        Xsqlite3_free(tls, pOld);
        return (uintptr)0;
    }
    if (nBytes > (uint64)SQLITE_MAX_ALLOCATION_SIZE) {
        return (uintptr)0;
    }
    nOld = _sqlite3MallocSize(tls, pOld);
    nNew = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32>>(_sqlite3Config.Fm.FxRoundup)(tls, libc.Int32FromUint64(nBytes));
    if (nOld == nNew) {
        pNew = pOld;
    } else {
        if (_sqlite3Config.FbMemstat != 0) {
            Xsqlite3_mutex_enter(tls, _mem0.Fmutex);
            _sqlite3StatusHighwater(ref (tls).DerefOrNull(), (int32)SQLITE_STATUS_MALLOC_SIZE, libc.Int32FromUint64(nBytes));
            nDiff = nNew - nOld;
            v2 = nDiff > 0; if (v2) {
                v1 = _sqlite3StatusValue(ref (tls).DerefOrNull(), SQLITE_STATUS_MEMORY_USED);
                nUsed = v1;
            }
            if (v2 && v1 >= _mem0.FalarmThreshold - (int64)nDiff) {
                _sqlite3MallocAlarm(tls, nDiff);
                if (_mem0.FhardLimit > 0 && nUsed >= _mem0.FhardLimit - (int64)nDiff) {
                    Xsqlite3_mutex_leave(tls, _mem0.Fmutex);
                    return (uintptr)0;
                }
            }
            pNew = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr>>(_sqlite3Config.Fm.FxRealloc)(tls, pOld, nNew);
            if (pNew == (uintptr)0 && _mem0.FalarmThreshold > 0) {
                _sqlite3MallocAlarm(tls, libc.Int32FromUint64(nBytes));
                pNew = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr>>(_sqlite3Config.Fm.FxRealloc)(tls, pOld, nNew);
            }
            if (pNew != 0) {
                nNew = _sqlite3MallocSize(tls, pNew);
                _sqlite3StatusUp(ref (tls).DerefOrNull(), SQLITE_STATUS_MEMORY_USED, nNew - nOld);
            }
            Xsqlite3_mutex_leave(tls, _mem0.Fmutex);
        } else {
            pNew = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr>>(_sqlite3Config.Fm.FxRealloc)(tls, pOld, nNew);
        }
    }
    return pNew;
}

internal static int32 /*r*/ _sqlite3Stat4Column(ж<libc.TLS> tls, uintptr db, uintptr pRec, int32 nRec, int32 iCol, @unsafe.Pointer ppVal) {
    uintptr a = default;
    uintptr pMem = default;
    uintptr v4 = default;
    int32 i = default;
    int32 v1 = default;
    Ti64 iField = default;
    Tu32 iHdr = default;
    ref var nHdr = ref heap(new Tu32(), out var ᏑnHdr);
    Tu32 szField = default;
    ref var t = ref heap(new Tu32(), out var Ꮡt);
    _ = a;
    _ = i;
    _ = iField;
    _ = iHdr;
    _ = nHdr;
    _ = pMem;
    _ = szField;
    _ = t;
    _ = v1;
    _ = v4;
    t = (uint32)0;
    szField = (uint32)0;
    a = pRec;
    pMem = ~@unsafe.Pointer.To<uintptr>(ppVal);
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
        nHdr = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a)));
        v1 = libc.Int32FromInt32(1);
    } else {
        v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref (tls).DerefOrNull(), a, @unsafe.Pointer.FromPinnedBox(ᏑnHdr)));
    }
    iHdr = (uint32)libc.Uint8FromInt32(v1);
    if (nHdr > libc.Uint32FromInt32(nRec) || iHdr >= nHdr) {
        return _sqlite3CorruptError(tls, (int32)87796);
    }
    iField = libc.Int64FromUint32(nHdr);
    i = 0;
    for (;;) {
        if (!(i <= iCol)) {
            break;
        }
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + (uintptr)iHdr))) < libc.Int32FromUint8(libc.Uint8FromInt32(0x80))) {
            t = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a + (uintptr)iHdr)));
            v1 = libc.Int32FromInt32(1);
        } else {
            v1 = libc.Int32FromUint8(_sqlite3GetVarint32(ref (tls).DerefOrNull(), a + (uintptr)iHdr, @unsafe.Pointer.FromPinnedBox(Ꮡt)));
        }
        iHdr = iHdr + (uint32)libc.Uint8FromInt32(v1);
        if (iHdr > nHdr) {
            return _sqlite3CorruptError(tls, (int32)87802);
        }
        szField = _sqlite3VdbeSerialTypeLen(ref (tls).DerefOrNull(), t);
        iField = iField + libc.Int64FromUint32(szField);
        goto _2;
_2:;
        i = i + 1;
    }
    if (iField > (int64)nRec) {
        return _sqlite3CorruptError(tls, (int32)87808);
    }
    if (pMem == (uintptr)0) {
        v4 = _sqlite3ValueNew(tls, db);
        (@unsafe.Pointer.To<uintptr>(ppVal)).Value = v4;
        pMem = v4;
        if (pMem == (uintptr)0) {
            return (int32)SQLITE_NOMEM;
        }
    }
    _sqlite3VdbeSerialGet(tls, a + (uintptr)(iField - libc.Int64FromUint32(szField)), t, pMem);
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fenc;
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3StrAccumEnlargeIfNeeded(ж<libc.TLS> tls, uintptr p, Ti64 N) {
    if (N + libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnChar) >= libc.Int64FromUint32((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FnAlloc)) {
        _sqlite3StrAccumEnlarge(tls, p, N);
    }
    return libc.Int32FromUint8((@unsafe.Pointer.To<TStrAccum>((@unsafe.Pointer)p)).Value.FaccError);
}

internal static int32 /*r*/ _sqlite3StrICmp(ref libc.TLS tls, uintptr zLeft, uintptr zRight) {
    uintptr a = default;
    uintptr b = default;
    int32 c = default;
    int32 x = default;
    _ = a;
    _ = b;
    _ = c;
    _ = x;
    a = zLeft;
    b = zRight;
    for (;;) {
        c = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(a)));
        x = libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(b)));
        if (c == x) {
            if (c == 0) {
                break;
            }
        } else {
            c = libc.Int32FromUint8(_sqlite3UpperToLower[c]) - libc.Int32FromUint8(_sqlite3UpperToLower[x]);
            if (c != 0) {
                break;
            }
        }
        a = a + 1;
        b = b + 1;
        goto _1;
_1:;
    }
    return c;
}

internal static void _sqlite3StringToId(ref libc.TLS tls, uintptr p) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_STRING) {
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop = (uint8)TK_ID;
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_COLLATE && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft)).Value.Fop) == (int32)TK_STRING) {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpLeft)).Value.Fop = (uint8)TK_ID;
        }
    }
}

internal static int32 /*r*/ _sqlite3TableColumnToIndex(ref libc.TLS tls, uintptr pIdx, int32 iCol) {
    int32 i = default;
    Ti16 iCol16 = default;
    _ = i;
    _ = iCol16;
    iCol16 = (int16)iCol;
    i = 0;
    for (;;) {
        if (!(i < libc.Int32FromUint16((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FnColumn))) {
            break;
        }
        if ((int32)iCol16 == (int32)(~@unsafe.Pointer.To<Ti16>(__ccgo_ptr((@unsafe.Pointer.To<TIndex>((@unsafe.Pointer)pIdx)).Value.FaiColumn + (uintptr)i * 2)))) {
            return i;
        }
        goto _1;
_1:;
        i = i + 1;
    }
    return -(int32)1;
}

internal static int32 /*r*/ _sqlite3TempInMemory(ref libc.TLS tls, uintptr db) {
    return libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Ftemp_store) == (int32)2);
}

internal static uintptr /*r*/ _sqlite3TriggerDeleteStep(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pWhere, uintptr zStart, uintptr zEnd) {
    uintptr db = default;
    uintptr pTriggerStep = default;
    _ = db;
    _ = pTriggerStep;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pTriggerStep = _triggerStepAllocate(tls, pParse, (uint8)TK_DELETE, pTabList, zStart, zEnd);
    if (pTriggerStep != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpWhere = pWhere;
            pWhere = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpWhere = _sqlite3ExprDup(tls, db, pWhere, (int32)EXPRDUP_REDUCE);
        }
        (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.Forconf = (uint8)OE_Default;
    }
    _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhere);
    return pTriggerStep;
}

internal static uintptr /*r*/ _sqlite3TriggerInsertStep(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pColumn, uintptr pSelect, Tu8 orconf, uintptr pUpsert, uintptr zStart, uintptr zEnd) {
    uintptr db = default;
    uintptr pTriggerStep = default;
    _ = db;
    _ = pTriggerStep;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pTriggerStep = _triggerStepAllocate(tls, pParse, (uint8)TK_INSERT, pTabList, zStart, zEnd);
    if (pTriggerStep != 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpSelect = pSelect;
            pSelect = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpSelect = _sqlite3SelectDup(tls, db, pSelect, (int32)EXPRDUP_REDUCE);
        }
        (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpIdList = pColumn;
        (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpUpsert = pUpsert;
        (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.Forconf = orconf;
        if (pUpsert != 0) {
            _sqlite3HasExplicitNulls(tls, pParse, (@unsafe.Pointer.To<TUpsert>((@unsafe.Pointer)pUpsert)).Value.FpUpsertTarget);
        }
    } else {
        _sqlite3IdListDelete(tls, (@unsafe.Pointer)db, pColumn);
        _sqlite3UpsertDelete(tls, (@unsafe.Pointer)db, pUpsert);
    }
    _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSelect);
    return pTriggerStep;
}

internal static uintptr /*r*/ _sqlite3TriggerUpdateStep(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pFrom, uintptr pEList, uintptr pWhere, Tu8 orconf, uintptr zStart, uintptr zEnd) {
    ref var @as = ref heap(new TToken(), out var Ꮡas);
    uintptr db = default;
    uintptr pFromDup = default;
    uintptr pSub = default;
    uintptr pTriggerStep = default;
    _ = @as;
    _ = db;
    _ = pFromDup;
    _ = pSub;
    _ = pTriggerStep;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    pTriggerStep = _triggerStepAllocate(tls, pParse, (uint8)TK_UPDATE, pTabList, zStart, zEnd);
    if (pTriggerStep != 0) {
        pFromDup = (uintptr)0;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpExprList = pEList;
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpWhere = pWhere;
            pFromDup = pFrom;
            pEList = (uintptr)0;
            pWhere = (uintptr)0;
            pFrom = (uintptr)0;
        } else {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpExprList = _sqlite3ExprListDup(tls, db, pEList, (int32)EXPRDUP_REDUCE);
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpWhere = _sqlite3ExprDup(tls, db, pWhere, (int32)EXPRDUP_REDUCE);
            pFromDup = _sqlite3SrcListDup(tls, db, pFrom, (int32)EXPRDUP_REDUCE);
        }
        (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.Forconf = orconf;
        if (pFromDup != 0 && !(libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= libc.Int32FromInt32(PARSE_MODE_RENAME))) {
            @as = new TToken();
            pSub = _sqlite3SelectNew(tls, pParse, (uintptr)0, pFromDup, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uint32)SF_NestedFrom, (uintptr)0);
            pFromDup = _sqlite3SrcListAppendFromTerm(tls, pParse, (uintptr)0, (uintptr)0, (uintptr)0, @unsafe.Pointer.FromPinnedBox(Ꮡas), pSub, (uintptr)0);
        }
        if (pFromDup != 0 && (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpSrc != 0) {
            (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpSrc = _sqlite3SrcListAppendList(tls, pParse, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pTriggerStep)).Value.FpSrc, pFromDup);
        } else {
            _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pFromDup);
        }
    }
    _sqlite3ExprListDelete(tls, (@unsafe.Pointer)db, pEList);
    _sqlite3ExprDelete(tls, (@unsafe.Pointer)db, pWhere);
    _sqlite3SrcListDelete(tls, (@unsafe.Pointer)db, pFrom);
    return pTriggerStep;
}

internal static int32 /*r1*/ _sqlite3Utf8CharLen(ref libc.TLS tls, uintptr zIn, int32 nByte) {
    int32 r = default;
    uintptr z = default;
    uintptr zTerm = default;
    uintptr v1 = default;
    _ = r;
    _ = z;
    _ = zTerm;
    _ = v1;
    r = 0;
    z = zIn;
    if (nByte >= 0) {
        zTerm = z + (uintptr)nByte;
    } else {
        zTerm = (uintptr)(-libc.Int32FromInt32(1));
    }
    while (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z))) != 0 && z < zTerm) {
        v1 = z;
        z = z + 1;
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))) >= (int32)0xc0) {
            while ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z))) & (int32)0xc0) == (int32)0x80) {
                z = z + 1;
            }
        }
        r = r + 1;
    }
    return r;
}

internal static Tu32 /*r*/ _sqlite3Utf8Read(ref libc.TLS tls, @unsafe.Pointer pz) {
    uint32 c = default;
    uintptr v1 = default;
    @unsafe.Pointer v2 = default;
    _ = c;
    _ = v1;
    _ = v2;
    v2 = pz;
    v1 = ~@unsafe.Pointer.To<uintptr>(v2);
    (@unsafe.Pointer.To<uintptr>(v2)).Value = ~@unsafe.Pointer.To<uintptr>(v2) + 1;
    c = (uint32)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1)));
    if (c >= (uint32)0xc0) {
        c = (uint32)_sqlite3Utf8Trans1[c - (uint32)0xc0];
        while ((libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(~@unsafe.Pointer.To<uintptr>(pz)))) & (int32)0xc0) == (int32)0x80) {
            v2 = pz;
            v1 = ~@unsafe.Pointer.To<uintptr>(v2);
            (@unsafe.Pointer.To<uintptr>(v2)).Value = ~@unsafe.Pointer.To<uintptr>(v2) + 1;
            c = (c << (int)((int32)6)) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v1))));
        }
        if (c < (uint32)0x80 || (c & (uint32)0xFFFFF800U) == (uint32)0xD800) {
            c = (uint32)0xFFFD;
        }
    }
    return c;
}

internal static int32 /*r*/ _sqlite3Utf8ReadLimited(ref libc.TLS tls, uintptr z, int32 n, @unsafe.Pointer piOut) {
    Tu32 c = default;
    int32 i = default;
    _ = c;
    _ = i;
    i = (int32)1;
    c = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z)));
    if (c >= (uint32)0xc0) {
        c = (uint32)_sqlite3Utf8Trans1[c - (uint32)0xc0];
        if (n > (int32)4) {
            n = (int32)4;
        }
        while (i < n && (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)i))) & (int32)0xc0) == (int32)0x80) {
            c = (c << (int)(libc.Int32FromInt32(6))) + libc.Uint32FromInt32(libc.Int32FromInt32(0x3f) & libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)i))));
            i = i + 1;
        }
    }
    (@unsafe.Pointer.To<Tu32>(piOut)).Value = c;
    return i;
}

internal static int32 /*r*/ _sqlite3ValueBytes(ж<libc.TLS> tls, uintptr pVal, Tu8 enc) {
    uintptr p = default;
    _ = p;
    p = pVal;
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Str) != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fenc) == libc.Int32FromUint8(enc)) {
        return (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Str) != 0 && libc.Int32FromUint8(enc) != (int32)SQLITE_UTF8 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fenc) != (int32)SQLITE_UTF8) {
        return (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Blob) != 0) {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Zero) != 0) {
            return (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn + ~@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>();
        } else {
            return (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fn;
        }
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (int32)MEM_Null) != 0) {
        return 0;
    }
    return _valueBytes(tls, pVal, enc);
}

internal static int32 /*r*/ _sqlite3ValueFromExpr(ж<libc.TLS> tls, uintptr db, uintptr pExpr, Tu8 enc, Tu8 affinity, @unsafe.Pointer ppVal) {
    ref var pVal = ref heap(new uintptr(), out var ᏑpVal);
    int32 rc = default;
    _ = pVal;
    _ = rc;
    rc = SQLITE_OK;
    pVal = (uintptr)0;
    if (pExpr != 0) {
        rc = _valueFromExpr(tls, db, pExpr, enc, affinity, @unsafe.Pointer.FromBox(ᏑpVal), nil);
        if (rc == SQLITE_OK && pVal != 0 && libc.Int32FromUint8(affinity) == (int32)SQLITE_AFF_REAL && (libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (int32)MEM_Int) != 0 && (~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVal) < -(int64)140737488355328L || ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pVal) > (int64)140737488355327L)) {
            _sqlite3VdbeMemRealify(tls, pVal);
        }
    }
    (@unsafe.Pointer.To<uintptr>(ppVal)).Value = pVal;
    return rc;
}

internal static int32 /*r*/ _sqlite3ValueIsOfClass(ref libc.TLS tls, uintptr pVal, uintptr __ccgo_fp_xFree) {
    int32 r = default;

    if (pVal != (uintptr)0 && (libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Blob))) != 0 && (libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (int32)MEM_Dyn) != 0 && (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.FxDel == __ccgo_fp_xFree) {
        return (int32)1;
    } else {
        return 0;
    }
    return r;
}

internal static uintptr /*r*/ _sqlite3ValueText(ж<libc.TLS> tls, uintptr pVal, Tu8 enc) {
    if (!(pVal != 0)) {
        return (uintptr)0;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Term))) == (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Term)) && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fenc) == libc.Int32FromUint8(enc)) {
        return (@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fz;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<Tsqlite3_value>((@unsafe.Pointer)pVal)).Value.Fflags) & (int32)MEM_Null) != 0) {
        return (uintptr)0;
    }
    return _valueToText(tls, pVal, enc);
}

internal static Ti64 /*r*/ _sqlite3VarintValue(ж<libc.TLS> tls, uintptr p) {
    Ti64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu8 x = default;
        Tu8 v1 = default;
        uintptr v2 = default;
        Tu64 _ᴛ1 = default;
        _ = x;
        _ = v1;
        _ = v2;
        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (uint64)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(p)));
        if (~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) >= (uint64)0x80) {
            p = p + 1;
            v2 = p;
            v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
            x = v1;
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)v1);
            if (libc.Int32FromUint8(x) >= (int32)0x80) {
                p = p + 1;
                v2 = p;
                v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
                x = v1;
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)v1);
                if (libc.Int32FromUint8(x) >= (int32)0x80) {
                    p = p + 1;
                    v2 = p;
                    v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
                    x = v1;
                    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)0x10204000) ^ (uint64)v1);
                    if (libc.Int32FromUint8(x) >= (int32)0x80) {
                        p = p + 1;
                        v2 = p;
                        v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
                        x = v1;
                        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)0x4000) ^ (uint64)v1);
                        if (libc.Int32FromUint8(x) >= (int32)0x80) {
                            p = p + 1;
                            v2 = p;
                            v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
                            x = v1;
                            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)0x4000) ^ (uint64)v1);
                            if (libc.Int32FromUint8(x) >= (int32)0x80) {
                                p = p + 1;
                                v2 = p;
                                v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
                                x = v1;
                                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)0x4000) ^ (uint64)v1);
                                if (libc.Int32FromUint8(x) >= (int32)0x80) {
                                    p = p + 1;
                                    v2 = p;
                                    v1 = ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2));
                                    x = v1;
                                    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(7)) ^ (uint64)0x4000) ^ (uint64)v1);
                                    if (libc.Int32FromUint8(x) >= (int32)0x80) {
                                        p = p + 1;
                                        v2 = p;
                                        (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)((Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) << (int)(libc.Int32FromInt32(8)) ^ (uint64)0x8000) ^ (uint64)(~@unsafe.Pointer.To<uint8>(__ccgo_ptr(v2))));
                                    }
                                }
                            }
                        }
                    }
                } else {
                    (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) ^ (uint64)0x204000);
                }
            } else {
                (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp))).Value = (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp)) ^ (uint64)0x4000);
            }
        }
        r = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeAddInt64(ж<libc.TLS> tls, uintptr p, int32 iDest, Ti64 iVal) {
    return _sqlite3VdbeAddOp3(tls, p, (int32)OP_Int64, libc.Int32FromUint64(libc.Uint64FromInt64(iVal) & libc.Uint64FromUint32(0xffffffffU)), iDest, libc.Int32FromUint64(libc.Uint64FromInt64(iVal).Rsh((int64)(libc.Int32FromInt32(32)))));
}

internal static int32 /*r*/ _sqlite3VdbeBooleanValue(ж<libc.TLS> tls, uintptr pMem, int32 ifNull) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
        return libc.BoolInt32(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem) != 0);
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Null) != 0) {
        return ifNull;
    }
    return libc.BoolInt32(_sqlite3VdbeRealValue(tls, pMem) != (float64)0D);
}

internal static int32 /*r*/ _sqlite3VdbeChangeEncoding(ж<libc.TLS> tls, uintptr pMem, int32 desiredEnc) {
    int32 rc = default;
    _ = rc;
    if (!((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & libc.Int32FromInt32(MEM_Str)) != 0)) {
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc = libc.Uint8FromInt32(desiredEnc);
        return SQLITE_OK;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc) == desiredEnc) {
        return SQLITE_OK;
    }
    rc = _sqlite3VdbeMemTranslate(tls, pMem, libc.Uint8FromInt32(desiredEnc));
    return rc;
}

internal static void _sqlite3VdbeClearObject(ж<libc.TLS> tls, uintptr db, uintptr p) {
    uintptr pNext = default;
    uintptr pSub = default;
    _ = pNext;
    _ = pSub;
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaColName != 0) {
        _releaseMemArray(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaColName, libc.Int32FromUint16((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnResAlloc) * (int32)COLNAME_N);
        _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaColName);
    }
    pSub = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpProgram;
    for (;;) {
        if (!(pSub != 0)) {
            break;
        }
        pNext = (@unsafe.Pointer.To<TSubProgram>((@unsafe.Pointer)pSub)).Value.FpNext;
        _vdbeFreeOpArray(tls, db, (@unsafe.Pointer.To<TSubProgram>((@unsafe.Pointer)pSub)).Value.FaOp, (@unsafe.Pointer.To<TSubProgram>((@unsafe.Pointer)pSub)).Value.FnOp);
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, pSub);
        goto _1;
_1:;
        pSub = pNext;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) != VDBE_INIT_STATE) {
        _releaseMemArray(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaVar, (int32)(@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnVar);
        if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpVList != 0) {
            _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpVList);
        }
        if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpFree != 0) {
            _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpFree);
        }
    }
    _vdbeFreeOpArray(tls, db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FaOp, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FnOp);
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql != 0) {
        _sqlite3DbNNFreeNN(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql);
    }
}

internal static int32 /*r*/ _sqlite3VdbeExplain(ж<libc.TLS> tls, uintptr pParse, Tu8 bPush, uintptr zFmt, uintptr va) {
    int32 addr = default;
    int32 iThis = default;
    Tva_list ap = default;
    uintptr v = default;
    uintptr zMsg = default;
    _ = addr;
    _ = ap;
    _ = iThis;
    _ = v;
    _ = zMsg;
    addr = 0;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fexplain) == (int32)2 || libc.Bool(0 != 0)) {
        ap = va;
        zMsg = _sqlite3VMPrintf(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, zFmt, ap);
        _ = ap;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        iThis = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)v)).Value.FnOp;
        addr = _sqlite3VdbeAddOp4(tls, v, (int32)OP_Explain, iThis, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FaddrExplain, 0, zMsg, -(int32)7);
        if (bPush != 0) {
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FaddrExplain = iThis;
        }
    }
    return addr;
}

internal static int32 /*r*/ _sqlite3VdbeFinalize(ж<libc.TLS> tls, uintptr p) {
    int32 rc = default;
    _ = rc;
    rc = SQLITE_OK;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) >= (int32)VDBE_READY_STATE) {
        rc = _sqlite3VdbeReset(tls, p);
    }
    _sqlite3VdbeDelete(tls, p);
    return rc;
}

internal static Ti64 /*r*/ _sqlite3VdbeIntValue(ref libc.TLS tls, uintptr pMem) {
    Ti64 r = default;

    int32 flags = default;
    _ = flags;
    flags = libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags);
    if ((flags & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
        return ~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem);
    } else {
        if ((flags & (int32)MEM_Real) != 0) {
            return _sqlite3RealToI64(ref tls, ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem));
        } else {
            if ((flags & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Blob))) != 0 && (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz != (uintptr)0) {
                return _memIntValue(ref tls, pMem);
            } else {
                return 0;
            }
        }
    }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeMemFromBtreeZeroOffset(ж<libc.TLS> tls, uintptr pCur, Tu32 amt, uintptr pMem) {
    ref var available = ref heap(new Tu32(), out var Ꮡavailable);
    int32 rc = default;
    _ = available;
    _ = rc;
    available = (uint32)0;
    rc = SQLITE_OK;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = _sqlite3BtreePayloadFetch(ref (tls).DerefOrNull(), pCur, @unsafe.Pointer.FromPinnedBox(Ꮡavailable));
    if (amt <= available) {
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Blob) | libc.Int32FromInt32(MEM_Ephem));
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = libc.Int32FromUint32(amt);
    } else {
        rc = _sqlite3VdbeMemFromBtree(tls, pCur, (uint32)0, amt, pMem);
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3VdbeMemIntegerify(ref libc.TLS tls, uintptr pMem) {
    (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = _sqlite3VdbeIntValue(ref tls, pMem);
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & ~(libc.Int32FromInt32(MEM_TypeMask) | libc.Int32FromInt32(MEM_Zero)) | (int32)MEM_Int);
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3VdbeMemNulTerminate(ж<libc.TLS> tls, uintptr pMem) {
    int32 r = default;

    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Term) | libc.Int32FromInt32(MEM_Str))) != (int32)MEM_Str) {
        return SQLITE_OK;
    } else {
        return _vdbeMemAddTerminator(tls, pMem);
    }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeMemRealify(ж<libc.TLS> tls, uintptr pMem) {
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem)).Value = _sqlite3VdbeRealValue(tls, pMem);
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & ~(libc.Int32FromInt32(MEM_TypeMask) | libc.Int32FromInt32(MEM_Zero)) | (int32)MEM_Real);
    return SQLITE_OK;
}

internal static void _sqlite3VdbeMemRelease(ж<libc.TLS> tls, uintptr p) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (libc.Int32FromInt32(MEM_Agg) | libc.Int32FromInt32(MEM_Dyn))) != 0 || (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.FszMalloc != 0) {
        _vdbeMemClear(tls, p);
    }
}

internal static void _sqlite3VdbeMemSetInt64(ж<libc.TLS> tls, uintptr pMem, Ti64 val) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Agg) | libc.Int32FromInt32(MEM_Dyn))) != 0) {
        _vdbeReleaseAndSetInt64(tls, pMem, val);
    } else {
        (@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem)).Value = val;
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Int;
    }
}

internal static void _sqlite3VdbeMemSetNull(ж<libc.TLS> tls, uintptr pMem) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Agg) | libc.Int32FromInt32(MEM_Dyn))) != 0) {
        _vdbeMemClearExternAndSetNull(tls, pMem);
    } else {
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = (uint16)MEM_Null;
    }
}

internal static int32 /*r*/ _sqlite3VdbeMemSetRowSet(ж<libc.TLS> tls, uintptr pMem) {
    uintptr db = default;
    uintptr p = default;
    _ = db;
    _ = p;
    db = (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fdb;
    _sqlite3VdbeMemRelease(tls, pMem);
    p = _sqlite3RowSetInit(tls, db);
    if (p == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = p;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Blob) | libc.Int32FromInt32(MEM_Dyn));
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.FxDel = __ccgo_fp(_sqlite3RowSetDelete);
    return SQLITE_OK;
}

internal static void _sqlite3VdbeMemSetZeroBlob(ж<libc.TLS> tls, uintptr pMem, int32 n) {
    _sqlite3VdbeMemRelease(tls, pMem);
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags = libc.Uint16FromInt32(libc.Int32FromInt32(MEM_Blob) | libc.Int32FromInt32(MEM_Zero));
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fn = 0;
    if (n < 0) {
        n = 0;
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem).of(TMem.ᏑFu).Reinterpret<TMemValue, int32>()).Value = n;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fenc = (uint8)SQLITE_UTF8;
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fz = (uintptr)0;
}

internal static float64 /*r*/ _sqlite3VdbeRealValue(ж<libc.TLS> tls, uintptr pMem) {
    float64 r = default;

    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (int32)MEM_Real) != 0) {
        return ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)pMem);
    } else {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Int) | libc.Int32FromInt32(MEM_IntReal))) != 0) {
            return (float64)(~@unsafe.Pointer.To<Ti64>((@unsafe.Pointer)pMem));
        } else {
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem)).Value.Fflags) & (libc.Int32FromInt32(MEM_Str) | libc.Int32FromInt32(MEM_Blob))) != 0) {
                return _sqlite3MemRealValueNoRC(tls, pMem);
            } else {
                return libc.Float64FromInt32(0);
            }
        }
    }
    return r;
}

internal static int32 /*r*/ _sqlite3VdbeReset(ж<libc.TLS> tls, uintptr p) {
    uintptr db = default;
    _ = db;
    db = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FeVdbeState) == (int32)VDBE_RUN_STATE) {
        _sqlite3VdbeHalt(tls, p);
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fpc >= 0) {
        if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpErr != 0 || (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzErrMsg != 0) {
            _sqlite3VdbeTransferError(tls, p);
        } else {
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrCode = (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc;
        }
    }
    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzErrMsg != 0) {
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzErrMsg);
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzErrMsg = (uintptr)0;
    }
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FpResultRow = (uintptr)0;
    return (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc & (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FerrMask;
}

internal static void _sqlite3VdbeSetSql(ж<libc.TLS> tls, uintptr p, uintptr z, int32 n, Tu8 prepFlags) {
    if (p == (uintptr)0) {
        return;
    }
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FprepFlags = prepFlags;
    if ((libc.Int32FromUint8(prepFlags) & (int32)SQLITE_PREPARE_SAVESQL) == 0) {
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fexpmask = (uint32)0;
        (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fsmimask = (uint32)0;
    }
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FzSql = _sqlite3DbStrNDup(tls, (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb, z, libc.Uint64FromInt32(n));
}

internal static void _sqlite3VdbeTypeofColumn(ref libc.TLS tls, uintptr p, int32 iDest) {
    uintptr pOp = default;
    uintptr v1 = default;
    _ = pOp;
    _ = v1;
    pOp = _sqlite3VdbeGetLastOp(ref tls, p);
    if ((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fp3 == iDest && libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeOp>((@unsafe.Pointer)pOp)).Value.Fopcode) == (int32)OP_Column) {
        v1 = pOp + 2;
        (@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)).Value = (Tu16)((int32)(~@unsafe.Pointer.To<Tu16>((@unsafe.Pointer)v1)) | libc.Int32FromInt32(OPFLAG_TYPEOFARG));
    }
}

internal static int32 /*r*/ _sqlite3ViewGetColumnNames(ж<libc.TLS> tls, uintptr pParse, uintptr pTable) {
    if (!(libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FeTabType) == libc.Int32FromInt32(TABTYP_VTAB)) && (int32)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTable)).Value.FnCol > 0) {
        return 0;
    }
    return _viewGetColumnNames(tls, pParse, pTable);
}

internal static int32 /*r*/ _sqlite3WalCallback(ref libc.TLS tls, uintptr pWal) {
    Tu32 ret = default;
    _ = ret;
    ret = (uint32)0;
    if (pWal != 0) {
        ret = (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiCallback;
        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FiCallback = (uint32)0;
    }
    return libc.Int32FromUint32(ret);
}

internal static int32 /*r*/ _sqlite3WalClose(ж<libc.TLS> tls, uintptr pWal, uintptr db, int32 sync_flags, int32 nBuf, uintptr zBuf) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 isDelete = default;
        int32 rc = default;
        int32 v1 = default;
        bool v2 = default;
        int32 _ᴛ1 = default;
        _ = isDelete;
        _ = rc;
        _ = v1;
        _ = v2;
        rc = SQLITE_OK;
        if (pWal != 0) {
            isDelete = 0;
            v2 = zBuf != (uintptr)0; if (v2) {
                v1 = _sqlite3OsLock(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd, (int32)SQLITE_LOCK_EXCLUSIVE);
                rc = v1;
            }
            if (v2 && SQLITE_OK == v1) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == WAL_NORMAL_MODE) {
                    (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode = (uint8)WAL_EXCLUSIVE_MODE;
                }
                rc = _sqlite3WalCheckpoint(tls, pWal, db, SQLITE_CHECKPOINT_PASSIVE, (uintptr)0, (uintptr)0, sync_flags, nBuf, zBuf, (uintptr)0, (uintptr)0);
                if (rc == SQLITE_OK) {
                    (@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))).Value = -(int32)1;
                    _sqlite3OsFileControlHint(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd, (int32)SQLITE_FCNTL_PERSIST_WAL, bp);
                    if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)) != (int32)1) {
                        isDelete = (int32)1;
                    } else {
                        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FmxWalSize >= 0) {
                            _walLimitSize(tls, pWal, 0);
                        }
                    }
                }
            }
            _walIndexClose(tls, pWal, isDelete);
            _sqlite3OsClose(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd);
            if (isDelete != 0) {
                _sqlite3BeginBenignMalloc(tls);
                _sqlite3OsDelete(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpVfs, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FzWalName, 0);
                _sqlite3EndBenignMalloc(tls);
            }
            Xsqlite3_free(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FapWiData);
            Xsqlite3_free(tls, pWal);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3WalExclusiveMode(ж<libc.TLS> tls, uintptr pWal, int32 op) {
    int32 rc = default;
    _ = rc;
    if (op == 0) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) != WAL_NORMAL_MODE) {
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode = (uint8)WAL_NORMAL_MODE;
            if (_walLockShared(tls, pWal, (int32)3 + (int32)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock) != SQLITE_OK) {
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode = (uint8)WAL_EXCLUSIVE_MODE;
            }
            rc = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == WAL_NORMAL_MODE);
        } else {
            rc = 0;
        }
    } else {
        if (op > 0) {
            _walUnlockShared(tls, pWal, (int32)3 + (int32)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadLock);
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode = (uint8)WAL_EXCLUSIVE_MODE;
            rc = (int32)1;
        } else {
            rc = libc.BoolInt32(libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == WAL_NORMAL_MODE);
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3WalHeapMemory(ref libc.TLS tls, uintptr pWal) {
    return libc.BoolInt32(pWal != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) == (int32)WAL_HEAPMEMORY_MODE);
}

internal static int32 /*r*/ _sqlite3WalSnapshotRecover(ж<libc.TLS> tls, uintptr pWal) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pBuf1 = default;
        uintptr pBuf2 = default;
        int32 rc = default;
        array<uintptr> _ᴛ1 = new(2);
        _ = pBuf1;
        _ = pBuf2;
        _ = rc;
        rc = _walLockExclusive(tls, pWal, (int32)WAL_CKPT_LOCK, (int32)1);
        if (rc == SQLITE_OK) {
            pBuf1 = Xsqlite3_malloc(tls, libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage));
            pBuf2 = Xsqlite3_malloc(tls, libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FszPage));
            if (pBuf1 == (uintptr)0 || pBuf2 == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock = (uint8)1;
                @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), 0, 2) = pBuf1;
                @unsafe.Pointer.ElementRef<uintptr>(__ccgo_ptr(bp), (int32)1, 2) = pBuf2;
                rc = _walSehTry(tls, pWal, __ccgo_fp(_walSehSnapshotRecover), bp, (uintptr)0);
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FckptLock = (uint8)0;
            }
            Xsqlite3_free(tls, pBuf1);
            Xsqlite3_free(tls, pBuf2);
            _walUnlockExclusive(tls, pWal, (int32)WAL_CKPT_LOCK, (int32)1);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3WalkSelectExpr(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    uintptr pParse = default;
    uintptr v1 = default;
    int32 rc = default;
    bool v2 = default;
    _ = pParse;
    _ = rc;
    _ = v1;
    _ = v2;
    if (_sqlite3WalkExprList(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpEList) != 0) {
        return (int32)WRC_Abort;
    }
    if (_sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWhere) != 0) {
        return (int32)WRC_Abort;
    }
    if (_sqlite3WalkExprList(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpGroupBy) != 0) {
        return (int32)WRC_Abort;
    }
    if (_sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpHaving) != 0) {
        return (int32)WRC_Abort;
    }
    if (_sqlite3WalkExprList(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpOrderBy) != 0) {
        return (int32)WRC_Abort;
    }
    if (_sqlite3WalkExpr(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit) != 0) {
        return (int32)WRC_Abort;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWinDefn != 0) {
        v2 = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FxSelectCallback2 == __ccgo_fp(_sqlite3WalkWinDefnDummyCallback); if (!v2) {
            v1 = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
            pParse = v1;
        }
        if (v2 || v1 != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME || (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FxSelectCallback2 == __ccgo_fp(_sqlite3SelectPopWith)) {
            rc = _walkWindowList(tls, pWalker, (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWinDefn, 0);
            return rc;
        }
    }
    return WRC_Continue;
}

internal static int32 /*r*/ _sqlite3WhereExplainOneScan(ж<libc.TLS> tls, uintptr pParse, uintptr pTabList, uintptr pLevel, Tu16 wctrlFlags) {
    int32 addr = default;
    int32 ret = default;
    uintptr v = default;
    uintptr v1 = default;
    _ = addr;
    _ = ret;
    _ = v;
    _ = v1;
    ret = 0;
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel != 0) {
        v1 = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel;
    } else {
        v1 = pParse;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)v1)).Value.Fexplain) == (int32)2 || libc.Bool(0 != 0)) {
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop)).Value.FwsFlags & (uint32)WHERE_MULTI_OR) == (uint32)0 && (libc.Int32FromUint16(wctrlFlags) & (int32)WHERE_OR_SUBCLAUSE) == 0) {
            v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
            addr = _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v);
            ret = _sqlite3VdbeAddOp3(tls, v, (int32)OP_Explain, addr, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FaddrExplain, (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereLevel>((@unsafe.Pointer)pLevel)).Value.FpWLoop)).Value.FrRun);
            _sqlite3WhereAddExplainText(tls, pParse, addr, pTabList, pLevel, wctrlFlags);
        }
    }
    return ret;
}

internal static TBitmask /*r*/ _sqlite3WhereExprUsageNN(ref libc.TLS tls, uintptr pMaskSet, uintptr p) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_COLUMN && !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_FixedCol))) != libc.Uint32FromInt32(0))) {
        return _sqlite3WhereGetMask(ref tls, pMaskSet, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FiTable);
    } else {
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_TokenOnly) | libc.Int32FromInt32(EP_Leaf))) != (uint32)0) {
            return (uint64)0;
        }
    }
    return _sqlite3WhereExprUsageFull(ref tls, pMaskSet, p);
}

internal static uintptr /*r*/ _sqlite3WhereFindTerm(ж<libc.TLS> tls, uintptr pWC, int32 iCur, int32 iColumn, TBitmask notReady, Tu32 op, uintptr pIdx) {
    uintptr p = default;
    uintptr pResult = default;
    ref var scan = ref heap(new TWhereScan(), out var Ꮡscan);
    _ = p;
    _ = pResult;
    _ = scan;
    pResult = (uintptr)0;
    p = _whereScanInit(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan), pWC, iCur, iColumn, op, pIdx);
    op = (Tu32)(op & libc.Uint32FromInt32(libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_IS)));
    while (p != 0) {
        if ((TBitmask)((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)p)).Value.FprereqRight & notReady) == (uint64)0) {
            if ((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)p)).Value.FprereqRight == (uint64)0 && ((uint32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)p)).Value.FeOperator & op) != (uint32)0) {
                return p;
            }
            if (pResult == (uintptr)0) {
                pResult = p;
            }
        }
        p = _whereScanNext(tls, @unsafe.Pointer.FromPinnedBox(Ꮡscan));
    }
    return pResult;
}

internal static int32 /*r*/ _sqlite3WhereIsDistinct(ref libc.TLS tls, uintptr pWInfo) {
    return libc.Int32FromUint8((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FeDistinct);
}

internal static int32 /*r*/ _sqlite3WhereLoopBloomable(ref libc.TLS tls, uintptr pLoop) {
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_IPK) != 0) {
        return (int32)1;
    }
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_INDEXED) == (uint32)0) {
        return 0;
    }
    return _sqlite3IndexBloomable(ref tls, (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex, libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq));
}

internal static void _sqlite3WhereSplit(ж<libc.TLS> tls, uintptr pWC, uintptr pExpr, Tu8 op) {
    uintptr pE2 = default;
    _ = pE2;
    pE2 = _sqlite3ExprSkipCollateAndLikely(ref (tls).DerefOrNull(), pExpr);
    (@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.Fop = op;
    if (pE2 == (uintptr)0) {
        return;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.Fop) != libc.Int32FromUint8(op)) {
        _whereClauseInsert(tls, pWC, pExpr, (uint16)0);
    } else {
        _sqlite3WhereSplit(tls, pWC, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FpLeft, op);
        _sqlite3WhereSplit(tls, pWC, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pE2)).Value.FpRight, op);
    }
}

internal static int32 /*r*/ _sqlite3WindowCompare(ж<libc.TLS> tls, uintptr pParse, uintptr p1, uintptr p2, int32 bFilter) {
    int32 res = default;
    int32 v1 = default;
    _ = res;
    _ = v1;
    if (p1 == (uintptr)0 || p2 == (uintptr)0) {
        return (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FeFrmType) != libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FeFrmType)) {
        return (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FeStart) != libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FeStart)) {
        return (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FeEnd) != libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FeEnd)) {
        return (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FeExclude) != libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FeExclude)) {
        return (int32)1;
    }
    if (_sqlite3ExprCompare(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FpStart, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FpStart, -(int32)1) != 0) {
        return (int32)1;
    }
    if (_sqlite3ExprCompare(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FpEnd, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FpEnd, -(int32)1) != 0) {
        return (int32)1;
    }
    v1 = _sqlite3ExprListCompare(tls, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FpPartition, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FpPartition, -(int32)1);
    res = v1;
    if (v1 != 0) {
        return res;
    }
    v1 = _sqlite3ExprListCompare(tls, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FpOrderBy, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FpOrderBy, -(int32)1);
    res = v1;
    if (v1 != 0) {
        return res;
    }
    if (bFilter != 0) {
        v1 = _sqlite3ExprCompare(tls, pParse, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p1)).Value.FpFilter, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p2)).Value.FpFilter, -(int32)1);
        res = v1;
        if (v1 != 0) {
            return res;
        }
    }
    return 0;
}

internal static int32 /*r*/ _sqlite3WindowExtraAggFuncDepth(ж<libc.TLS> tls, uintptr pWalker, uintptr pExpr) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AGG_FUNCTION && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop2) >= (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FwalkerDepth) {
        (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop2 = (Tu8)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop2 + 1);
    }
    return WRC_Continue;
}

internal static uintptr /*r*/ _sqlite3WindowOffsetExpr(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr) {
    if (0 == _sqlite3ExprIsConstant(tls, (uintptr)0, pExpr)) {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) >= (int32)PARSE_MODE_RENAME) {
            _sqlite3RenameExprUnmap(tls, pParse, pExpr);
        }
        _sqlite3ExprDelete(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pExpr);
        pExpr = _sqlite3ExprAlloc(tls, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (int32)TK_NULL, nil, 0);
    }
    return pExpr;
}

internal static int32 /*r*/ _sqlite3WritableSchema(ref libc.TLS tls, uintptr db) {
    return libc.BoolInt32((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fflags & libc.Uint64FromInt32(libc.Int32FromInt32(SQLITE_WriteSchema) | libc.Int32FromInt32(SQLITE_Defensive))) == (uint64)SQLITE_WriteSchema);
}

internal static int32 /*r*/ _sqlite3_open_v2__ccgo_gp(ж<libc.TLS> tls, uintptr filename, @unsafe.Pointer ppDb, int32 flags, uintptr zVfs) {
    return _openDatabase(tls, filename, ppDb, libc.Uint32FromInt32(flags), zVfs);
}

internal static int32 /*r*/ _sqliteErrorFromPosixError(ref libc.TLS tls, int32 posixError, int32 sqliteIOErr) {
    int32 r = default;

    var matchᴛ1 = false;
    if (posixError == (int32)EACCES) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && posixError == (int32)EAGAIN) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && posixError == (int32)ETIMEDOUT) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && posixError == (int32)EBUSY) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && posixError == (int32)EINTR) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && posixError == (int32)ENOLCK) { matchᴛ1 = true;
        return (int32)SQLITE_BUSY;
    }
    if (posixError == (int32)EPERM) {
        return (int32)SQLITE_PERM;
    }
    if (!matchᴛ1) { /* default: */
        return sqliteIOErr;
    }
    return default;

    return r;
}

internal static int32 /*r*/ _statEof(ж<libc.TLS> tls, uintptr pCursor) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = pCursor;
    return libc.Int32FromUint8((@unsafe.Pointer.To<TStatCursor>((@unsafe.Pointer)pCsr)).Value.FisEof);
}

internal static int32 /*r*/ _statRowid(ж<libc.TLS> tls, uintptr pCursor, uintptr pRowid) {
    uintptr pCsr = default;
    _ = pCsr;
    pCsr = pCursor;
    (@unsafe.Pointer.To<Tsqlite_int64>(__ccgo_ptr(pRowid))).Value = libc.Int64FromUint32((@unsafe.Pointer.To<TStatCursor>((@unsafe.Pointer)pCsr)).Value.FiPageno);
    return SQLITE_OK;
}

internal static void _storeLastErrno(ref libc.TLS tls, uintptr pFile, int32 error1) {
    (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlastErrno = error1;
}

internal static int32 /*r*/ _strContainsChar(ref libc.TLS tls, uintptr zStr, int32 nStr, Tu32 ch) {
    Tu32 tst = default;
    ref var z = ref heap(new uintptr(), out var Ꮡz);
    uintptr zEnd = default;
    uintptr v2 = default;
    uint32 v1 = default;
    _ = tst;
    _ = z;
    _ = zEnd;
    _ = v1;
    _ = v2;
    zEnd = zStr + (uintptr)nStr;
    z = zStr;
    while (z < zEnd) {
        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z))) < (int32)0x80) {
            v2 = z;
            z = z + 1;
            v1 = (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2)));
        } else {
            v1 = _sqlite3Utf8Read(ref tls, @unsafe.Pointer.FromBox(Ꮡz));
        }
        tst = v1;
        if (tst == ch) {
            return (int32)1;
        }
    }
    return 0;
}

internal static int32 /*r*/ _subjournalPage(ж<libc.TLS> tls, uintptr pPg) {
    Ti64 offset = default;
    uintptr pData = default;
    uintptr pData2 = default;
    uintptr pPager = default;
    int32 rc = default;
    _ = offset;
    _ = pData;
    _ = pData2;
    _ = pPager;
    _ = rc;
    rc = SQLITE_OK;
    pPager = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpPager;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) != (int32)PAGER_JOURNALMODE_OFF) {
        rc = _openSubJournal(tls, pPager);
        if (rc == SQLITE_OK) {
            pData = (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.FpData;
            offset = libc.Int64FromUint32((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSubRec) * ((int64)4 + (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize);
            pData2 = pData;
            rc = _write32bits(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd, offset, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno);
            if (rc == SQLITE_OK) {
                rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fsjfd, pData2, (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, offset + (int64)4);
            }
        }
    }
    if (rc == SQLITE_OK) {
        (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSubRec = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnSubRec + 1;
        rc = _addToSavepointBitvecs(tls, pPager, (@unsafe.Pointer.To<TPgHdr>((@unsafe.Pointer)pPg)).Value.Fpgno);
    }
    return rc;
}

internal static void _subtypeFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    _ = argc;
    Xsqlite3_result_int(tls, context, libc.Int32FromUint32(Xsqlite3_value_subtype(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)))));
}

internal static int32 /*r*/ _tabIsReadOnly(ж<libc.TLS> tls, uintptr pParse, uintptr pTab) {
    uintptr db = default;
    _ = db;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
        return _vtabIsReadOnly(tls, pParse, pTab);
    }
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & libc.Uint32FromInt32(libc.Int32FromInt32(TF_Readonly) | libc.Int32FromInt32(TF_Shadow))) == (uint32)0) {
        return 0;
    }
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Readonly) != (uint32)0) {
        return libc.BoolInt32(_sqlite3WritableSchema(ref (tls).DerefOrNull(), db) == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fnested) == 0);
    }
    return _sqlite3ReadOnlyShadowTables(ref (tls).DerefOrNull(), db);
}

internal static void _unicodeFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    ref var z = ref heap(new uintptr(), out var Ꮡz);
    _ = z;
    z = Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    _ = argc;
    if (z != 0 && ~@unsafe.Pointer.To<uint8>(__ccgo_ptr(z)) != 0) {
        Xsqlite3_result_int(tls, context, libc.Int32FromUint32(_sqlite3Utf8Read(ref (tls).DerefOrNull(), @unsafe.Pointer.FromBox(Ꮡz))));
    }
}

internal static int32 /*r*/ _unionWithLimitOne(ж<libc.TLS> tls, uintptr db, uintptr p) {
    ref var v = ref heap(new int32(), out var Ꮡv);
    _ = v;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.Fop) != (int32)TK_UNION) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit == (uintptr)0) {
        return 0;
    }
    if ((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit)).Value.FpRight != 0) {
        return 0;
    }
    v = 0;
    if (_sqlite3ExprIsInteger(tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit)).Value.FpLeft, @unsafe.Pointer.FromPinnedBox(Ꮡv), (uintptr)0, 0) == 0) {
        return 0;
    }
    if (v != (int32)1) {
        return 0;
    }
    if ((Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & (uint32)SQLITE_UnionLimit) != (uint32)0) {
        return 0;
    }
    return (int32)1;
}

internal static uintptr _unixBigLock = (uintptr)0;

internal static int32 /*r*/ _unixClose(ж<libc.TLS> tls, uintptr id) {
    uintptr pFile = default;
    uintptr pInode = default;
    int32 rc = default;
    _ = pFile;
    _ = pInode;
    _ = rc;
    rc = SQLITE_OK;
    pFile = id;
    pInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode;
    _verifyDbFile(tls, pFile);
    _unixUnlock(tls, id, NO_LOCK);
    _unixEnterMutex(tls);
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
    if ((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnLock != 0) {
        _setPendingFd(ref (tls).DerefOrNull(), pFile);
    }
    Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FpLockMutex);
    _releaseInodeInfo(tls, pFile);
    rc = _closeUnixFile(tls, id);
    _unixLeaveMutex(tls);
    return rc;
}

internal static int32 /*r*/ _unixCurrentTime(ж<libc.TLS> tls, uintptr NotUsed, uintptr prNow) {
    ref var i = ref heap(new Tsqlite3_int64(), out var Ꮡi);
    int32 rc = default;
    _ = i;
    _ = rc;
    i = 0;
    _ = NotUsed;
    rc = _unixCurrentTimeInt64__ccgo_gp(tls, (uintptr)0, @unsafe.Pointer.FromPinnedBox(Ꮡi));
    (@unsafe.Pointer.To<float64>(__ccgo_ptr(prNow))).Value = (float64)i / (float64)8.64e+07D;
    return rc;
}

internal static int32 _unixCurrentTimeInt64(ж<libc.TLS> tls, uintptr NotUsed, uintptr piNow) {
    return _unixCurrentTimeInt64__ccgo_gp(tls, NotUsed, (@unsafe.Pointer)piNow);
}

internal static int32 /*r*/ _unixDeviceCharacteristics(ж<libc.TLS> tls, uintptr id) {
    uintptr pFd = default;
    _ = pFd;
    pFd = id;
    _setDeviceCharacteristics(ref (tls).DerefOrNull(), pFd);
    return (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FdeviceCharacteristics;
}

internal static void _unixDlClose(ж<libc.TLS> tls, uintptr NotUsed, uintptr pHandle) {
    _ = NotUsed;
    libc.Xdlclose(tls, pHandle);
}

internal static uintptr /*r*/ _unixDlOpen(ж<libc.TLS> tls, uintptr NotUsed, uintptr zFilename) {
    _ = NotUsed;
    return libc.Xdlopen(tls, zFilename, libc.Int32FromInt32(RTLD_NOW) | libc.Int32FromInt32(RTLD_GLOBAL));
}

internal static uintptr /*r*/ _unixDlSym(ж<libc.TLS> tls, uintptr NotUsed, uintptr p, uintptr zSym) {
    ref var x = ref heap(new uintptr(), out var Ꮡx);
    _ = x;
    _ = NotUsed;
    x = __ccgo_fp(libc.Xdlsym);
    return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, uintptr>>(x)(tls, p, zSym);
}

internal static void _unixEnterMutex(ж<libc.TLS> tls) {
    Xsqlite3_mutex_enter(tls, _unixBigLock);
}

internal static int64 _unixEpoch = libc.Int64FromInt32(24405875) * libc.Int64FromInt32(8640000);

internal static void _unixLeaveMutex(ж<libc.TLS> tls) {
    Xsqlite3_mutex_leave(tls, _unixBigLock);
}

internal static int32 /*r*/ _unixSectorSize(ж<libc.TLS> tls, uintptr id) {
    uintptr pFd = default;
    _ = pFd;
    pFd = id;
    _setDeviceCharacteristics(ref (tls).DerefOrNull(), pFd);
    return (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFd)).Value.FsectorSize;
}

internal static int32 /*r*/ _unixShmRegionPerMap(ж<libc.TLS> tls) {
    int32 pgsz = default;
    int32 shmsz = default;
    _ = pgsz;
    _ = shmsz;
    shmsz = libc.Int32FromInt32(32) * libc.Int32FromInt32(1024);
    pgsz = GoFuncCookie.Func<Func<ж<libc.TLS>, int32>>(_aSyscall[(int32)25].FpCurrent)(tls);
    if (pgsz < shmsz) {
        return (int32)1;
    }
    return quo(pgsz, shmsz);
}

internal static int32 /*r*/ _unixUnlock(ж<libc.TLS> tls, uintptr id, int32 eFileLock) {
    return _posixUnlock(tls, id, eFileLock, 0);
}

internal static int32 /*r*/ _unixWrite(ж<libc.TLS> tls, uintptr id, uintptr pBuf, int32 amt, Tsqlite3_int64 offset) {
    uintptr pFile = default;
    int32 wrote = default;
    int32 v1 = default;
    _ = pFile;
    _ = wrote;
    _ = v1;
    pFile = id;
    wrote = 0;
    for (;;) {
        v1 = _seekAndWrite(tls, pFile, offset, pBuf, amt);
        wrote = v1;
        if (!(v1 < amt && wrote > 0)) {
            break;
        }
        amt = amt - wrote;
        offset = offset + (int64)wrote;
        pBuf = pBuf + (uintptr)wrote;
    }
    if (amt > wrote) {
        if (wrote < 0 && (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlastErrno != (int32)ENOSPC) {
            return libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
        } else {
            _storeLastErrno(ref (tls).DerefOrNull(), pFile, 0);
            return (int32)SQLITE_FULL;
        }
    }
    return SQLITE_OK;
}

internal static void _unlockBtreeIfUnused(ж<libc.TLS> tls, uintptr pBt) {
    uintptr pPage1 = default;
    _ = pPage1;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FinTransaction) == TRANS_NONE && (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1 != (uintptr)0) {
        pPage1 = (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1;
        (@unsafe.Pointer.To<TBtShared>((@unsafe.Pointer)pBt)).Value.FpPage1 = (uintptr)0;
        _releasePageOne(tls, pPage1);
    }
}

internal static int32 /*r*/ _vdbeCompareMemString(ж<libc.TLS> tls, uintptr pMem1, uintptr pMem2, uintptr pColl, @unsafe.Pointer prcErr) {
    int32 r = default;

    if (libc.Int32FromUint8((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fenc) == libc.Int32FromUint8((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.Fenc)) {
        return GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32, uintptr, int32>>((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FxCmp)(tls, (@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)pColl)).Value.FpUser, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fn, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem1)).Value.Fz, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem2)).Value.Fn, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)pMem2)).Value.Fz);
    } else {
        return _vdbeCompareMemStringWithEncodingChange(tls, pMem1, pMem2, pColl, prcErr);
    }
    return r;
}

internal static void _vdbeMemClear(ж<libc.TLS> tls, uintptr p) {
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fflags) & (libc.Int32FromInt32(MEM_Agg) | libc.Int32FromInt32(MEM_Dyn))) != 0) {
        _vdbeMemClearExternAndSetNull(tls, p);
    }
    if ((@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.FszMalloc != 0) {
        _sqlite3DbFreeNN(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fdb, (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.FzMalloc);
        (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.FszMalloc = 0;
    }
    (@unsafe.Pointer.To<TMem>((@unsafe.Pointer)p)).Value.Fz = (uintptr)0;
}

internal static int32 /*r*/ _vdbePmaReadVarint(ж<libc.TLS> tls, uintptr p, @unsafe.Pointer pnOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var a = ref heap(new uintptr(), out var Ꮡa);
        int32 i = default;
        int32 iBuf = default;
        int32 rc = default;
        int32 v1 = default;
        array<Tu8> _ᴛ1 = new(16);
        _ = a;
        _ = i;
        _ = iBuf;
        _ = rc;
        _ = v1;
        if ((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaMap != 0) {
            (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p))).Value += libc.Int64FromUint8(_sqlite3GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaMap + (uintptr)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff, pnOut));
        } else {
            iBuf = (int32)(rem((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FiReadOff, (int64)(@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer));
            if (iBuf != 0 && (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FnBuffer - iBuf >= (int32)9) {
                (@unsafe.Pointer.To<Ti64>(__ccgo_ptr(p))).Value += libc.Int64FromUint8(_sqlite3GetVarint(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)p)).Value.FaBuffer + (uintptr)iBuf, pnOut));
            } else {
                i = 0;
                for (var cond = true; cond; cond = (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a))) & (int32)0x80) != 0) {
                    rc = _vdbePmaReadBlob(tls, p, (int32)1, @unsafe.Pointer.FromBox(Ꮡa));
                    if (rc != 0) {
                        r = rc; goto ᒐdone;
                    }
                    v1 = i;
                    i = i + 1;
                    @unsafe.Pointer.ElementRef<Tu8>(__ccgo_ptr(bp), (v1 & (int32)0xf), 16) = ~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(a));
                }
                _sqlite3GetVarint(ref (tls).DerefOrNull(), bp, pnOut);
            }
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _vdbePmaReaderInit(ж<libc.TLS> tls, uintptr pTask, uintptr pFile, Ti64 iStart, uintptr pReadr, @unsafe.Pointer pnByte) {
    ref var nByte = ref heap(new Tu64(), out var ᏑnByte);
    int32 rc = default;
    @unsafe.Pointer v1 = default;
    _ = nByte;
    _ = rc;
    _ = v1;
    rc = _vdbePmaReaderSeek(tls, pTask, pReadr, pFile, iStart);
    if (rc == SQLITE_OK) {
        nByte = (uint64)0;
        rc = _vdbePmaReadVarint(tls, pReadr, @unsafe.Pointer.FromPinnedBox(ᏑnByte));
        (@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FiEof = libc.Int64FromUint64(libc.Uint64FromInt64((@unsafe.Pointer.To<TPmaReader>((@unsafe.Pointer)pReadr)).Value.FiReadOff) + nByte);
        v1 = pnByte;
        (@unsafe.Pointer.To<Ti64>(v1)).Value = (Ti64)((uint64)(~@unsafe.Pointer.To<Ti64>(v1)) + nByte);
    }
    if (rc == SQLITE_OK) {
        rc = _vdbePmaReaderNext(tls, pReadr);
    }
    return rc;
}

internal static Ti64 /*r*/ _vdbeRecordDecodeInt(ж<libc.TLS> tls, Tu32 serial_type, uintptr aKey) {
    Ti64 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tu64 _ᴛ1 = default;
        Tu32 _ᴛ2 = default;
        var matchᴛ1 = false;
        if (serial_type == (uint32)0) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && serial_type == (uint32)1) { matchᴛ1 = true;
            r = (int64)libc.Int8FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey))); goto ᒐdone;
        }
        if (serial_type == (uint32)2) { matchᴛ1 = true;
            r = (int64)(libc.Int32FromInt32(256) * (int32)libc.Int8FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 1)))); goto ᒐdone;
        }
        if (serial_type == (uint32)3) { matchᴛ1 = true;
            r = (int64)(libc.Int32FromInt32(65536) * (int32)libc.Int8FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 1))).Lsh((int64)(libc.Int32FromInt32(8))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 2)))); goto ᒐdone;
        }
        if (serial_type == (uint32)4) { matchᴛ1 = true;
            (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(bp))).Value = ((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey)))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 3)));
            r = (int64)(~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp))); goto ᒐdone;
        }
        if (serial_type == (uint32)5) {
            r = libc.Int64FromUint32(((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(2))))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(2) + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(2) + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(2) + 3)))) + libc.Int64FromInt32(1).Lsh((int64)(libc.Int32FromInt32(32))) * (int64)(libc.Int32FromInt32(256) * (int32)libc.Int8FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey))) | libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 1)))); goto ᒐdone;
        }
        if (serial_type == (uint32)6) { matchᴛ1 = true;
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8))).Value = (uint64)(((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey)))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + 3))));
            (@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8))).Value = (Tu64)(~@unsafe.Pointer.To<Tu64>(__ccgo_ptr(bp + 8)) << (int)(libc.Int32FromInt32(32)) | (uint64)(((uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(4))))).Lsh((int64)(libc.Int32FromInt32(24))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(4) + 1))).Lsh((int64)(libc.Int32FromInt32(16)))) | libc.Uint32FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(4) + 2))).Lsh((int64)(libc.Int32FromInt32(8)))) | (uint32)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(aKey + libc.UintptrFromInt32(4) + 3)))));
            r = ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp + 8)); goto ᒐdone;
        }

        r = libc.Int64FromUint32(serial_type - libc.Uint32FromInt32(8));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _vdbeSorterCompareInt(ж<libc.TLS> tls, uintptr pTask, uintptr pbKey2Cached, uintptr pKey1, int32 nKey1, uintptr pKey2, int32 nKey2) {
    int32 i = default;
    int32 res = default;
    int32 s1 = default;
    int32 s2 = default;
    int32 v21 = default;
    int32 v3 = default;
    Tu8 n = default;
    uintptr p1 = default;
    uintptr p2 = default;
    uintptr v1 = default;
    uintptr v2 = default;
    _ = i;
    _ = n;
    _ = p1;
    _ = p2;
    _ = res;
    _ = s1;
    _ = s2;
    _ = v1;
    _ = v2;
    _ = v21;
    _ = v3;
    p1 = pKey1;
    p2 = pKey2;
    s1 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p1 + 1)));
    s2 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p2 + 1)));
    v1 = p1 + (uintptr)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p1)));
    v2 = p2 + (uintptr)(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p2)));
    if (s1 == s2) {
        n = _aLen[s1];
        res = 0;
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint8(n))) {
                break;
            }
            v21 = libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1 + (uintptr)i))) - libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2 + (uintptr)i)));
            res = v21;
            if (v21 != 0) {
                if (((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))) ^ libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2)))) & (int32)0x80) != 0) {
                    if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))) & (int32)0x80) != 0) {
                        v3 = -(int32)1;
                    } else {
                        v3 = +libc.Int32FromInt32(1);
                    }
                    res = v3;
                }
                break;
            }
            goto _1;
_1:;
            i = i + 1;
        }
    } else {
        if (s1 > (int32)7 && s2 > (int32)7) {
            res = s1 - s2;
        } else {
            if (s2 > (int32)7) {
                res = +libc.Int32FromInt32(1);
            } else {
                if (s1 > (int32)7) {
                    res = -(int32)1;
                } else {
                    res = s1 - s2;
                }
            }
            if (res > 0) {
                if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v1))) & (int32)0x80) != 0) {
                    res = -(int32)1;
                }
            } else {
                if ((libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(v2))) & (int32)0x80) != 0) {
                    res = +libc.Int32FromInt32(1);
                }
            }
        }
    }
    if (res == 0) {
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FpKeyInfo)).Value.FnKeyField) > (int32)1) {
            res = _vdbeSorterCompareTail(tls, pTask, pbKey2Cached, pKey1, nKey1, pKey2, nKey2);
        }
    } else {
        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FpKeyInfo)).Value.FaSortFlags)) != 0) {
            res = res * -(int32)1;
        }
    }
    return res;
}

internal static int32 /*r*/ _vdbeSorterCompareRealInt(ж<libc.TLS> tls, uintptr pTask, uintptr pbKey2Cached, uintptr pKey1, int32 nKey1, uintptr pKey2, int32 nKey2) {
    int32 r = default;

    uintptr p1 = default;
    uintptr p2 = default;
    float64 r1 = default;
    float64 r2 = default;
    _ = p1;
    _ = p2;
    _ = r1;
    _ = r2;
    p1 = pKey1;
    p2 = pKey2;
    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p1 + 1))) == (int32)6 || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p2 + 1))) == (int32)6) {
        return _vdbeSorterCompare(tls, pTask, pbKey2Cached, pKey1, nKey1, pKey2, nKey2);
    } else {
        r1 = _vdbeSorterGetReal(tls, p1);
        r2 = _vdbeSorterGetReal(tls, p2);
        return _vdbeSorterFinishRealCompare(tls, pTask, pbKey2Cached, pKey1, nKey1, pKey2, nKey2, r1, r2);
    }
    return r;
}

internal static int32 /*r*/ _vdbeSorterFinishRealCompare(ж<libc.TLS> tls, uintptr pTask, uintptr pbKey2Cached, uintptr pKey1, int32 nKey1, uintptr pKey2, int32 nKey2, float64 r1, float64 r2) {
    int32 res = default;
    _ = res;
    if (r1 < r2) {
        res = -(int32)1;
    } else {
        if (r1 > r2) {
            res = +libc.Int32FromInt32(1);
        } else {
            res = 0;
        }
    }
    if (res == 0) {
        if (libc.Int32FromUint16((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FpKeyInfo)).Value.FnKeyField) > (int32)1) {
            res = _vdbeSorterCompareTail(tls, pTask, pbKey2Cached, pKey1, nKey1, pKey2, nKey2);
        }
    } else {
        if (~@unsafe.Pointer.To<Tu8>(__ccgo_ptr((@unsafe.Pointer.To<TKeyInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)(@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpSorter)).Value.FpKeyInfo)).Value.FaSortFlags)) != 0) {
            res = res * -(int32)1;
        }
    }
    return res;
}

internal static TSorterCompare /*r*/ _vdbeSorterGetCompare(ref libc.TLS tls, uintptr p) {
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)p)).Value.FtypeMask) & (int32)SORTER_TYPE_INTEGER) != 0) {
        return __ccgo_fp(_vdbeSorterCompareInt);
    } else {
        if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)p)).Value.FtypeMask) & (int32)SORTER_TYPE_TEXT) != 0) {
            return __ccgo_fp(_vdbeSorterCompareText);
        } else {
            if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbeSorter>((@unsafe.Pointer)p)).Value.FtypeMask) & (int32)SORTER_TYPE_REAL) != 0) {
                return __ccgo_fp(_vdbeSorterCompareReal);
            }
        }
    }
    return __ccgo_fp(_vdbeSorterCompare);
}

internal static int32 /*r*/ _walHash(ref libc.TLS tls, Tu32 iPage) {
    return libc.Int32FromUint32((Tu32)(iPage * (uint32)HASHTABLE_HASH_1 & libc.Uint32FromInt32(libc.Int32FromInt32(HASHTABLE_NPAGE) * libc.Int32FromInt32(2) - libc.Int32FromInt32(1))));
}

internal static int32 /*r*/ _walIndexReadHdr(ж<libc.TLS> tls, uintptr pWal, @unsafe.Pointer pChanged) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 bWriteLock = default;
        int32 badHdr = default;
        int32 rc = default;
        int32 v1 = default;
        bool v4 = default;
        uintptr _ᴛ1 = default;
        _ = bWriteLock;
        _ = badHdr;
        _ = rc;
        _ = v1;
        _ = v4;
        rc = _walIndexPage(tls, pWal, 0, bp);
        if (rc != SQLITE_OK) {
            if (rc == (libc.Int32FromInt32(SQLITE_READONLY) | libc.Int32FromInt32(5).Lsh((int64)(libc.Int32FromInt32(8))))) {
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FbShmUnreliable = (uint8)1;
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode = (uint8)WAL_HEAPMEMORY_MODE;
                (@unsafe.Pointer.To<int32>(pChanged)).Value = (int32)1;
            } else {
                r = rc; goto ᒐdone;
            }
        } else {
        }
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) != 0) {
            v1 = _walIndexTryHdr(tls, pWal, pChanged);
        } else {
            v1 = (int32)1;
        }
        badHdr = v1;
        if (badHdr != 0) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FbShmUnreliable) == 0 && (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FreadOnly) & (int32)WAL_SHM_RDONLY) != 0) {
                v1 = _walLockShared(tls, pWal, WAL_WRITE_LOCK);
                rc = v1;
                if (SQLITE_OK == v1) {
                    _walUnlockShared(tls, pWal, WAL_WRITE_LOCK);
                    rc = libc.Int32FromInt32(SQLITE_READONLY) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
                }
            } else {
                bWriteLock = libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock);
                v4 = bWriteLock != 0; if (!v4) {
                    v1 = _walLockExclusive(tls, pWal, WAL_WRITE_LOCK, (int32)1);
                    rc = v1;
                }
                if (v4 || SQLITE_OK == v1) {
                    if (!(bWriteLock != 0)) {
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock = (uint8)2;
                    }
                    v1 = _walIndexPage(tls, pWal, 0, bp);
                    rc = v1;
                    if (SQLITE_OK == v1) {
                        badHdr = _walIndexTryHdr(tls, pWal, pChanged);
                        if (badHdr != 0) {
                            rc = _walIndexRecover(tls, pWal);
                            (@unsafe.Pointer.To<int32>(pChanged)).Value = (int32)1;
                        }
                    }
                    if (bWriteLock == 0) {
                        (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FwriteLock = (uint8)0;
                        _walUnlockExclusive(tls, pWal, WAL_WRITE_LOCK, (int32)1);
                    }
                }
            }
        }
        if (badHdr == 0 && (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FiVersion != (uint32)WALINDEX_MAX_VERSION) {
            rc = _sqlite3CantopenError(tls, (int32)69905);
        }
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FbShmUnreliable != 0) {
            if (rc != SQLITE_OK) {
                _walIndexClose(tls, pWal, 0);
                (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FbShmUnreliable = (uint8)0;
                if (rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(2).Lsh((int64)(libc.Int32FromInt32(8))))) {
                    rc = -(int32)1;
                }
            }
            (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode = (uint8)WAL_NORMAL_MODE;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _walPagesize(ref libc.TLS tls, uintptr pWal) {
    return (libc.Int32FromUint16((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage) & (int32)0xfe00) + ((libc.Int32FromUint16((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FszPage) & (int32)0x0001) << (int)((int32)16));
}

internal static int32 /*r*/ _walSehCheckpoint(ж<libc.TLS> tls, uintptr pWal, uintptr p) {
    uintptr a = default;
    int32 rc = default;
    _ = a;
    _ = rc;
    a = p;
    rc = (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.Frc;
    if (rc == SQLITE_OK) {
        rc = _walIndexReadHdr(tls, pWal, (@unsafe.Pointer)(@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpisChanged);
        if ((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FeMode2 > SQLITE_CHECKPOINT_PASSIVE) {
        }
        if (~@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpisChanged)) != 0 && (@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd)).Value.FpMethods)).Value.FiVersion >= (int32)3) {
            _sqlite3OsUnfetch(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd, 0, (uintptr)0);
        }
    }
    if (rc == SQLITE_OK) {
        _sqlite3FaultSim(tls, (int32)660);
        if ((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame != 0 && _walPagesize(ref (tls).DerefOrNull(), pWal) != (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FnBuf) {
            rc = _sqlite3CorruptError(tls, (int32)71625);
        } else {
            if ((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FeMode2 != -(int32)1) {
                rc = _walCheckpoint(tls, pWal, (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.Fdb, (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FeMode2, (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FxBusy2, (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpBusyArg, (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.Fsync_flags, (@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FzBuf);
            }
        }
        if (rc == SQLITE_OK || rc == (int32)SQLITE_BUSY) {
            if ((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpnLog != 0) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpnLog))).Value = libc.Int32FromUint32((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.Fhdr.FmxFrame);
            }
            _modernc_seh_inject(ref (tls).DerefOrNull(), pWal);
            if ((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpnCkpt != 0) {
                (@unsafe.Pointer.To<int32>(__ccgo_ptr((@unsafe.Pointer.To<TWalSehCheckpoint>((@unsafe.Pointer)a)).Value.FpnCkpt))).Value = libc.Int32FromUint32((@unsafe.Pointer.To<TWalCkptInfo>((@unsafe.Pointer)_walCkptInfo(ref (tls).DerefOrNull(), pWal))).Value.FnBackfill);
            }
        }
    }
    return rc;
}

internal static void _walShmBarrier(ж<libc.TLS> tls, uintptr pWal) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FexclusiveMode) != (int32)WAL_HEAPMEMORY_MODE) {
        _sqlite3OsShmBarrier(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpDbFd);
    }
}

internal static void _whereCombineDisjuncts(ж<libc.TLS> tls, uintptr pSrc, uintptr pWC, uintptr pOne, uintptr pTwo) {
    uintptr db = default;
    uintptr pA = default;
    uintptr pB = default;
    uintptr pNew = default;
    Tu16 eOp = default;
    int32 idxNew = default;
    int32 op = default;
    _ = db;
    _ = eOp;
    _ = idxNew;
    _ = op;
    _ = pA;
    _ = pB;
    _ = pNew;
    eOp = libc.Uint16FromInt32(libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOne)).Value.FeOperator) | libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTwo)).Value.FeOperator));
    if (((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOne)).Value.FwtFlags) | libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTwo)).Value.FwtFlags)) & (int32)TERM_VNULL) != 0) {
        return;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOne)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) == 0) {
        return;
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTwo)).Value.FeOperator) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) == 0) {
        return;
    }
    if ((libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))))) != libc.Int32FromUint16(eOp) && (libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_EQ) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))))) != libc.Int32FromUint16(eOp)) {
        return;
    }
    pA = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pOne)).Value.FpExpr;
    pB = (@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTwo)).Value.FpExpr;
    if (_sqlite3ExprCompare(tls, (uintptr)0, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pA)).Value.FpLeft, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pB)).Value.FpLeft, -(int32)1) != 0) {
        return;
    }
    if (_sqlite3ExprCompare(tls, (uintptr)0, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pA)).Value.FpRight, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pB)).Value.FpRight, -(int32)1) != 0) {
        return;
    }
    if (libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pA)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Commuted))) != (uint32)0) != libc.BoolInt32((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pB)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_Commuted))) != (uint32)0)) {
        return;
    }
    if ((libc.Int32FromUint16(eOp) & (libc.Int32FromUint16(eOp) - (int32)1)) != 0) {
        if ((libc.Int32FromUint16(eOp) & (libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LT) - libc.Int32FromInt32(TK_EQ))) | libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))))) != 0) {
            eOp = libc.Uint16FromInt32(libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_LE) - libc.Int32FromInt32(TK_EQ))));
        } else {
            eOp = libc.Uint16FromInt32(libc.Int32FromInt32(WO_EQ).Lsh((int64)(libc.Int32FromInt32(TK_GE) - libc.Int32FromInt32(TK_EQ))));
        }
    }
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereClause>((@unsafe.Pointer)pWC)).Value.FpWInfo)).Value.FpParse)).Value.Fdb;
    pNew = _sqlite3ExprDup(tls, db, pA, 0);
    if (pNew == (uintptr)0) {
        return;
    }
    op = (int32)TK_EQ;
    for (;;) {
        if (!(libc.Int32FromUint16(eOp) != (int32)WO_EQ << (int)(op - (int32)TK_EQ))) {
            break;
        }
        goto _1;
_1:;
        op = op + 1;
    }
    (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pNew)).Value.Fop = libc.Uint8FromInt32(op);
    idxNew = _whereClauseInsert(tls, pWC, pNew, libc.Uint16FromInt32(libc.Int32FromInt32(TERM_VIRTUAL) | libc.Int32FromInt32(TERM_DYNAMIC)));
    _exprAnalyze(tls, pSrc, pWC, idxNew);
}

internal static void _whereLoopAdjustCost(ref libc.TLS tls, uintptr p, uintptr pTemplate) {
    int32 v2 = default;
    _ = v2;
    if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FwsFlags & (uint32)WHERE_INDEXED) == (uint32)0) {
        return;
    }
    for (;;) {
        if (!(p != 0)) {
            break;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FiTab) != libc.Int32FromUint8((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FiTab)) {
            goto _1;
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FwsFlags & (uint32)WHERE_INDEXED) == (uint32)0) {
            goto _1;
        }
        if (_whereLoopCheaperProperSubset(ref tls, p, pTemplate) != 0) {
            if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FrRun < (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun) {
                v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FrRun;
            } else {
                v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun;
            }
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun = (int16)v2;
            if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnOut - (int32)1 < (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut) {
                v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnOut - (int32)1;
            } else {
                v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut;
            }
            (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut = (int16)v2;
        } else {
            if (_whereLoopCheaperProperSubset(ref tls, pTemplate, p) != 0) {
                if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FrRun > (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun) {
                    v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FrRun;
                } else {
                    v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun;
                }
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FrRun = (int16)v2;
                if ((int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnOut + (int32)1 > (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut) {
                    v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FnOut + (int32)1;
                } else {
                    v2 = (int32)(@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut;
                }
                (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pTemplate)).Value.FnOut = (int16)v2;
            }
        }
        goto _1;
_1:;
        p = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)p)).Value.FpNextLoop;
    }
}

internal static TLogEst /*r*/ _whereRangeAdjust(ref libc.TLS tls, uintptr pTerm, TLogEst nNew) {
    TLogEst nRet = default;
    _ = nRet;
    nRet = nNew;
    if (pTerm != 0) {
        if ((int32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FtruthProb <= 0) {
            nRet = (int16)((int32)nRet + (int32)(@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FtruthProb);
        } else {
            if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereTerm>((@unsafe.Pointer)pTerm)).Value.FwtFlags) & (int32)TERM_VNULL) == 0) {
                nRet = (int16)((int32)nRet - libc.Int32FromInt32(20));
            }
        }
    }
    return nRet;
}

internal static uintptr /*r*/ _whereRightSubexprIsColumn(ref libc.TLS tls, uintptr p) {
    p = _sqlite3ExprSkipCollateAndLikely(ref tls, (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.FpRight);
    if (p != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_COLUMN && !((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_FixedCol))) != libc.Uint32FromInt32(0))) {
        return p;
    }
    return (uintptr)0;
}

internal static TLogEst /*r*/ _whereSortingCost(ref libc.TLS tls, uintptr pWInfo, TLogEst nRow, int32 nOrderBy, int32 nSorted) {
    TLogEst nCol = default;
    TLogEst rSortCost = default;
    _ = nCol;
    _ = rSortCost;
    nCol = _sqlite3LogEst(ref tls, libc.Uint64FromInt32(((@unsafe.Pointer.To<TExprList>((@unsafe.Pointer)(@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FpSelect)).Value.FpEList)).Value.FnExpr + (int32)59) / (int32)30));
    rSortCost = (int16)((int32)nRow + (int32)nCol);
    if (nSorted > 0) {
        rSortCost = (int16)((int32)rSortCost + ((int32)_sqlite3LogEst(ref tls, libc.Uint64FromInt32(quo((nOrderBy - nSorted) * (int32)100, nOrderBy))) - libc.Int32FromInt32(66)));
    }
    if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_USE_LIMIT) != 0) {
        rSortCost = (int16)((int32)rSortCost + libc.Int32FromInt32(10));
        if (nSorted != 0) {
            rSortCost = (int16)((int32)rSortCost + libc.Int32FromInt32(6));
        }
        if ((int32)(@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiLimit < (int32)nRow) {
            nRow = (@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FiLimit;
        }
    } else {
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TWhereInfo>((@unsafe.Pointer)pWInfo)).Value.FwctrlFlags) & (int32)WHERE_WANT_DISTINCT) != 0) {
            if ((int32)nRow > (int32)10) {
                nRow = (int16)((int32)nRow - libc.Int32FromInt32(10));
            }
        }
    }
    rSortCost = (int16)((int32)rSortCost + (int32)_estLog(ref tls, nRow));
    return rSortCost;
}

internal static void _writeInt16(ref libc.TLS tls, uintptr p, int32 i) {
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p))).Value = libc.Uint8FromInt32(i >> (int)((int32)8) & (int32)0xFF);
    (@unsafe.Pointer.To<Tu8>(__ccgo_ptr(p + 1))).Value = libc.Uint8FromInt32(i >> 0 & (int32)0xFF);
}

internal static int32 /*r*/ _xferCheckRowid(ж<libc.TLS> tls, uintptr pWalk, uintptr pExpr) {
    int32 r = default;

    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_COLUMN && (int32)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FiColumn < 0) {
        (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalk)).Value.FeCode = (uint16)1;
        return (int32)WRC_Abort;
    } else {
        return WRC_Continue;
    }
    return r;
}

internal static uint16 /*r*/ _yy_find_reduce_action(ref libc.TLS tls, uint16 stateno, uint16 iLookAhead) {
    int32 i = default;
    _ = i;
    i = (int32)_yy_reduce_ofst[stateno];
    i = i + libc.Int32FromUint16(iLookAhead);
    return _yy_action[i];
}

internal static uint16 /*r*/ _yy_find_shift_action(ref libc.TLS tls, uint16 iLookAhead, uint16 stateno) {
    uint16 r = default;

    int32 i = default;
    int32 j = default;
    uint16 iFallback = default;
    _ = i;
    _ = iFallback;
    _ = j;
    if (libc.Int32FromUint16(stateno) > (int32)YY_MAX_SHIFT) {
        return stateno;
    }
    for (var cond = true; cond; cond = (int32)1 != 0) {
        i = libc.Int32FromUint16(_yy_shift_ofst[stateno]);
        i = i + libc.Int32FromUint16(iLookAhead);
        if (libc.Int32FromUint16(_yy_lookahead[i]) != libc.Int32FromUint16(iLookAhead)) {
            iFallback = _yyFallback[iLookAhead];
            if (libc.Int32FromUint16(iFallback) != 0) {
                iLookAhead = iFallback;
                continue;
            }
            j = i - libc.Int32FromUint16(iLookAhead) + (int32)YYWILDCARD;
            if (libc.Int32FromUint16(_yy_lookahead[j]) == (int32)YYWILDCARD && libc.Int32FromUint16(iLookAhead) > 0) {
                return _yy_action[j];
            }
            return _yy_default[stateno];
        } else {
            return _yy_action[i];
        }
    }
    return r;
}

internal static int32 /*r*/ _zeroJournalHdr(ж<libc.TLS> tls, uintptr pPager, int32 doTruncate) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Ti64 iLimit = default;
        int32 rc = default;
        Ti64 _ᴛ1 = default;
        _ = iLimit;
        _ = rc;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalOff != 0) {
            iLimit = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalSizeLimit;
            if (doTruncate != 0 || iLimit == 0) {
                rc = _sqlite3OsTruncate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, 0);
            } else {
                rc = _sqlite3OsWrite(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, (uintptr)Ꮡ_zeroHdr, (int32)28, 0);
            }
            if (rc == SQLITE_OK && !((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FnoSync != 0)) {
                rc = _sqlite3OsSync(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, (int32)SQLITE_SYNC_DATAONLY | libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FsyncFlags));
            }
            if (rc == SQLITE_OK && iLimit > 0) {
                rc = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, bp);
                if (rc == SQLITE_OK && ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) > iLimit) {
                    rc = _sqlite3OsTruncate(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.Fjfd, iLimit);
                }
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _zeroblobFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    Ti64 n = default;
    int32 rc = default;
    _ = n;
    _ = rc;
    _ = argc;
    n = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
    if (n < 0) {
        n = 0;
    }
    rc = Xsqlite3_result_zeroblob64(tls, context, libc.Uint64FromInt64(n));
    if (rc != 0) {
        Xsqlite3_result_error_code(tls, context, rc);
    }
}

[GoInit] internal static void initΔ7() {
    @unsafe.Pointer p = @unsafe.Pointer.FromBox(Ꮡ_nolockIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_nolockIoFinderImpl);
}

[GoInit] internal static void initΔ8() {
    @unsafe.Pointer p = @unsafe.Pointer.FromBox(Ꮡ_posixIoFinder);
    (@unsafe.Pointer.To<uintptr>(@unsafe.Add(p, 0))).Value = __ccgo_fp(_posixIoFinderImpl);
}

internal static readonly @string static_assert = "_Static_assert"u8;

} // end sqlite3_package
