global using Ptrdiff_t = long;
global using Size_t = ulong;
global using Wchar_t = ushort;
global using X__int128_t = go.modernc.org.libc.errno_package.X__int128_tᴛ1;
global using X__uint128_t = go.modernc.org.libc.errno_package.X__uint128_tᴛ1;
global using X__builtin_va_list = go.uintptr;
global using X__float128 = double;
global using Va_list = go.uintptr;
global using X__gnuc_va_list = go.uintptr;
global using Ssize_t = long;
global using Rsize_t = ulong;
global using Intptr_t = long;
global using Uintptr_t = ulong;
global using Wint_t = ushort;
global using Wctype_t = ushort;
global using Errno_t = int;
global using X__time32_t = int;
global using X__time64_t = long;
global using Time_t = long;
global using Threadlocaleinfostruct = go.modernc.org.libc.errno_package.Threadlocaleinfostructᴛ1;
global using Pthreadlocinfo = go.uintptr;
global using Pthreadmbcinfo = go.uintptr;
global using Localeinfo_struct = go.modernc.org.libc.errno_package.Localeinfo_structᴛ1;
global using X_locale_tstruct = go.modernc.org.libc.errno_package.Localeinfo_structᴛ1;
global using X_locale_t = go.uintptr;
global using TagLC_ID = go.modernc.org.libc.errno_package.TagLC_IDᴛ1;
global using LC_ID = go.modernc.org.libc.errno_package.TagLC_IDᴛ1;
global using LPLC_ID = go.uintptr;
global using Threadlocinfo = go.modernc.org.libc.errno_package.Threadlocaleinfostructᴛ1;

namespace go.modernc.org.libc;

using math = math_package;
using reflect = reflect_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class errno_package {

internal static float64 _ᴛ1ʗ = math.Pi;
internal static reflect.Kind _ᴛ2ʗ;
internal static atomic.Value _ᴛ3ʗ;
internal static @unsafe.Pointer _ᴛ4ʗ;

public static UntypedInt DUMMYSTRUCTNAME => 0;
public static UntypedInt DUMMYSTRUCTNAME1 => 0;
public static UntypedInt DUMMYSTRUCTNAME2 => 0;
public static UntypedInt DUMMYSTRUCTNAME3 => 0;
public static UntypedInt DUMMYSTRUCTNAME4 => 0;
public static UntypedInt DUMMYSTRUCTNAME5 => 0;
public static UntypedInt DUMMYUNIONNAME => 0;
public static UntypedInt DUMMYUNIONNAME1 => 0;
public static UntypedInt DUMMYUNIONNAME2 => 0;
public static UntypedInt DUMMYUNIONNAME3 => 0;
public static UntypedInt DUMMYUNIONNAME4 => 0;
public static UntypedInt DUMMYUNIONNAME5 => 0;
public static UntypedInt DUMMYUNIONNAME6 => 0;
public static UntypedInt DUMMYUNIONNAME7 => 0;
public static UntypedInt DUMMYUNIONNAME8 => 0;
public static UntypedInt DUMMYUNIONNAME9 => 0;
public static UntypedInt E2BIG => 7;
public static UntypedInt EACCES => 13;
public static UntypedInt EADDRINUSE => 100;
public static UntypedInt EADDRNOTAVAIL => 101;
public static UntypedInt EAFNOSUPPORT => 102;
public static UntypedInt EAGAIN => 11;
public static UntypedInt EALREADY => 103;
public static UntypedInt EBADF => 9;
public static UntypedInt EBADMSG => 104;
public static UntypedInt EBUSY => 16;
public static UntypedInt ECANCELED => 105;
public static UntypedInt ECHILD => 10;
public static UntypedInt ECONNABORTED => 106;
public static UntypedInt ECONNREFUSED => 107;
public static UntypedInt ECONNRESET => 108;
public static UntypedInt EDEADLK => 36;
public static UntypedInt EDEADLOCK => 36;
public static UntypedInt EDESTADDRREQ => 109;
public static UntypedInt EDOM => 33;
public static UntypedInt EEXIST => 17;
public static UntypedInt EFAULT => 14;
public static UntypedInt EFBIG => 27;
public static UntypedInt EHOSTUNREACH => 110;
public static UntypedInt EIDRM => 111;
public static UntypedInt EILSEQ => 42;
public static UntypedInt EINPROGRESS => 112;
public static UntypedInt EINTR => 4;
public static UntypedInt EINVAL => 22;
public static UntypedInt EIO => 5;
public static UntypedInt EISCONN => 113;
public static UntypedInt EISDIR => 21;
public static UntypedInt ELOOP => 114;
public static UntypedInt EMFILE => 24;
public static UntypedInt EMLINK => 31;
public static UntypedInt EMSGSIZE => 115;
public static UntypedInt ENAMETOOLONG => 38;
public static UntypedInt ENETDOWN => 116;
public static UntypedInt ENETRESET => 117;
public static UntypedInt ENETUNREACH => 118;
public static UntypedInt ENFILE => 23;
public static UntypedInt ENOBUFS => 119;
public static UntypedInt ENODATA => 120;
public static UntypedInt ENODEV => 19;
public static UntypedInt ENOENT => 2;
public static UntypedInt ENOEXEC => 8;
public static UntypedInt ENOFILE => 2;
public static UntypedInt ENOLCK => 39;
public static UntypedInt ENOLINK => 121;
public static UntypedInt ENOMEM => 12;
public static UntypedInt ENOMSG => 122;
public static UntypedInt ENOPROTOOPT => 123;
public static UntypedInt ENOSPC => 28;
public static UntypedInt ENOSR => 124;
public static UntypedInt ENOSTR => 125;
public static UntypedInt ENOSYS => 40;
public static UntypedInt ENOTCONN => 126;
public static UntypedInt ENOTDIR => 20;
public static UntypedInt ENOTEMPTY => 41;
public static UntypedInt ENOTRECOVERABLE => 127;
public static UntypedInt ENOTSOCK => 128;
public static UntypedInt ENOTSUP => 129;
public static UntypedInt ENOTTY => 25;
public static UntypedInt ENXIO => 6;
public static UntypedInt EOPNOTSUPP => 130;
public static UntypedInt EOVERFLOW => 132;
public static UntypedInt EOWNERDEAD => 133;
public static UntypedInt EPERM => 1;
public static UntypedInt EPIPE => 32;
public static UntypedInt EPROTO => 134;
public static UntypedInt EPROTONOSUPPORT => 135;
public static UntypedInt EPROTOTYPE => 136;
public static UntypedInt ERANGE => 34;
public static UntypedInt EROFS => 30;
public static UntypedInt ESPIPE => 29;
public static UntypedInt ESRCH => 3;
public static UntypedInt ETIME => 137;
public static UntypedInt ETIMEDOUT => 138;
public static UntypedInt ETXTBSY => 139;
public static UntypedInt EWOULDBLOCK => 140;
public static UntypedInt EXDEV => 18;
public static UntypedInt MINGW_DDK_H => 0;
public static UntypedInt MINGW_DDRAW_VERSION => 7;
public static UntypedInt MINGW_HAS_DDK_H => 1;
public static UntypedInt MINGW_HAS_DDRAW_H => 1;
public static UntypedInt MINGW_HAS_SECURE_API => 1;
public static UntypedInt MINGW_SDK_INIT => 0;
public static UntypedInt STRUNCATE => 80;
public static UntypedInt UNALIGNED => 0;
public static UntypedInt USE___UUIDOF => 0;
public static UntypedInt WIN32 => 1;
public static UntypedInt WIN64 => 1;
public static UntypedInt WINNT => 1;
public static UntypedInt X_AGLOBAL => 0;
public static UntypedInt X_ANONYMOUS_STRUCT => 0;
public static UntypedInt X_ANONYMOUS_UNION => 0;
public static UntypedInt X_ARGMAX => 100;
public static UntypedInt X_CONST_RETURN => 0;
public static UntypedInt X_CRTNOALIAS => 0;
public static UntypedInt X_CRTRESTRICT => 0;
public static UntypedInt X_CRT_ALTERNATIVE_IMPORTED => 0;
public static UntypedInt X_CRT_ERRNO_DEFINED => 0;
public static UntypedInt X_CRT_MANAGED_HEAP_DEPRECATE => 0;
public static UntypedInt X_CRT_PACKING => 8;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES_MEMORY => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_COUNT => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_MEMORY => 0;
public static UntypedInt X_DLL => 0;
public static UntypedInt X_ERRCODE_DEFINED => 0;
public static UntypedInt X_FILE_OFFSET_BITS => 64;
public static UntypedInt X_INC_CRTDEFS => 0;
public static UntypedInt X_INC_CRTDEFS_MACRO => 0;
public static UntypedInt X_INC_ERRNO => 0;
public static UntypedInt X_INC_MINGW_SECAPI => 0;
public static UntypedInt X_INC_VADEFS => 0;
public static UntypedInt X_INC__MINGW_H => 0;
public static UntypedInt X_INT128_DEFINED => 0;
public static UntypedInt X_INTEGRAL_MAX_BITS => 64;
public static UntypedInt X_INTPTR_T_DEFINED => 0;
public static UntypedInt X_MT => 0;
public static UntypedInt X_M_AMD64 => 100;
public static UntypedInt X_M_X64 => 100;
public static UntypedInt X_PGLOBAL => 0;
public static UntypedInt X_PTRDIFF_T_ => 0;
public static UntypedInt X_PTRDIFF_T_DEFINED => 0;
public static UntypedInt X_REENTRANT => 1;
public static UntypedInt X_RSIZE_T_DEFINED => 0;
public static UntypedInt X_SECURECRT_ERRCODE_VALUES_DEFINED => 0;
public static UntypedInt X_SECURECRT_FILL_BUFFER_PATTERN => 0xFD;
public static UntypedInt X_SIZE_T_DEFINED => 0;
public static UntypedInt X_SSIZE_T_DEFINED => 0;
public static UntypedInt X_TAGLC_ID_DEFINED => 0;
public static UntypedInt X_THREADLOCALEINFO => 0;
public static UntypedInt X_TIME32_T_DEFINED => 0;
public static UntypedInt X_TIME64_T_DEFINED => 0;
public static UntypedInt X_TIME_T_DEFINED => 0;
public static UntypedInt X_UINTPTR_T_DEFINED => 0;
public static UntypedInt X_VA_LIST_DEFINED => 0;
public static UntypedInt X_W64 => 0;
public static UntypedInt X_WCHAR_T_DEFINED => 0;
public static UntypedInt X_WCTYPE_T_DEFINED => 0;
public static UntypedInt X_WIN32 => 1;
public static UntypedInt X_WIN32_WINNT => 0x502;
public static UntypedInt X_WIN64 => 1;
public static UntypedInt X_WINT_T => 0;

partial struct X__int128_tᴛ1 /*dyn*/ {
    public int64 Flo;
    public int64 Fhi;
}
partial struct X__uint128_tᴛ1 /*dyn*/ {
    public uint64 Flo;
    public uint64 Fhi;
}

partial struct Threadlocaleinfostructᴛ1_Flc_category /*dyn*/ {
    public uintptr Flocale;
    public uintptr Fwlocale;
    public uintptr Frefcount;
    public uintptr Fwrefcount;
}

partial struct Threadlocaleinfostructᴛ1 /*dyn*/ {
    public int32 Frefcount;
    public uint32 Flc_codepage;
    public uint32 Flc_collate_cp;
    public array<uint32> Flc_handle = new(6);
    public array<LC_ID> Flc_id = new(6);
    public array<Threadlocaleinfostructᴛ1_Flc_category> Flc_category = new(6);
    public int32 Flc_clike;
    public int32 Fmb_cur_max;
    public uintptr Flconv_intl_refcount;
    public uintptr Flconv_num_refcount;
    public uintptr Flconv_mon_refcount;
    public uintptr Flconv;
    public uintptr Fctype1_refcount;
    public uintptr Fctype1;
    public uintptr Fpctype;
    public uintptr Fpclmap;
    public uintptr Fpcumap;
    public uintptr Flc_time_curr;
}

partial struct Localeinfo_structᴛ1 /*dyn*/ {
    public Pthreadlocinfo Flocinfo;
    public Pthreadmbcinfo Fmbcinfo;
}

partial struct TagLC_IDᴛ1 /*dyn*/ {
    public uint16 FwLanguage;
    public uint16 FwCountry;
    public uint16 FwCodePage;
}

internal static int8 _ᴛ5ʗ;

} // end errno_package
