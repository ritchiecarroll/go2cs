global using Ptrdiff_t = long;
global using Size_t = ulong;
global using Wchar_t = ushort;
global using X__int128_t = go.modernc.org.libc.fcntl_package.X__int128_tᴛ1;
global using X__uint128_t = go.modernc.org.libc.fcntl_package.X__uint128_tᴛ1;
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
global using Threadlocaleinfostruct = go.modernc.org.libc.fcntl_package.Threadlocaleinfostructᴛ1;
global using Pthreadlocinfo = go.uintptr;
global using Pthreadmbcinfo = go.uintptr;
global using Localeinfo_struct = go.modernc.org.libc.fcntl_package.Localeinfo_structᴛ1;
global using X_locale_tstruct = go.modernc.org.libc.fcntl_package.Localeinfo_structᴛ1;
global using X_locale_t = go.uintptr;
global using TagLC_ID = go.modernc.org.libc.fcntl_package.TagLC_IDᴛ1;
global using LC_ID = go.modernc.org.libc.fcntl_package.TagLC_IDᴛ1;
global using LPLC_ID = go.uintptr;
global using Threadlocinfo = go.modernc.org.libc.fcntl_package.Threadlocaleinfostructᴛ1;
global using X_fsize_t = uint;
global using X_finddata32_t = go.modernc.org.libc.fcntl_package.X_finddata32_tᴛ1;
global using X_finddata32i64_t = go.modernc.org.libc.fcntl_package.X_finddata32i64_tᴛ1;
global using X_finddata64i32_t = go.modernc.org.libc.fcntl_package.X_finddata64i32_tᴛ1;
global using X__finddata64_t = go.modernc.org.libc.fcntl_package.X__finddata64_tᴛ1;
global using X_wfinddata32_t = go.modernc.org.libc.fcntl_package.X_wfinddata32_tᴛ1;
global using X_wfinddata32i64_t = go.modernc.org.libc.fcntl_package.X_wfinddata32i64_tᴛ1;
global using X_wfinddata64i32_t = go.modernc.org.libc.fcntl_package.X_wfinddata64i32_tᴛ1;
global using X_wfinddata64_t = go.modernc.org.libc.fcntl_package.X_wfinddata64_tᴛ1;
global using X_off_t = int;
global using Off32_t = int;
global using X_off64_t = long;
global using Off64_t = long;
global using Off_t = long;

namespace go.modernc.org.libc;

using math = math_package;
using reflect = reflect_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class fcntl_package {

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
public static UntypedInt F_OK => 0;
public static UntypedInt MINGW_DDK_H => 0;
public static UntypedInt MINGW_DDRAW_VERSION => 7;
public static UntypedInt MINGW_HAS_DDK_H => 1;
public static UntypedInt MINGW_HAS_DDRAW_H => 1;
public static UntypedInt MINGW_HAS_SECURE_API => 1;
public static UntypedInt MINGW_SDK_INIT => 0;
public static UntypedInt O_ACCMODE => 3;
public static UntypedInt O_APPEND => 8;
public static UntypedInt O_BINARY => 32768;
public static UntypedInt O_CREAT => 256;
public static UntypedInt O_EXCL => 1024;
public static UntypedInt O_NOINHERIT => 128;
public static UntypedInt O_RANDOM => 16;
public static UntypedInt O_RAW => 32768;
public static UntypedInt O_RDONLY => 0;
public static UntypedInt O_RDWR => 2;
public static UntypedInt O_SEQUENTIAL => 32;
public static UntypedInt O_TEMPORARY => 64;
public static UntypedInt O_TEXT => 16384;
public static UntypedInt O_TRUNC => 512;
public static UntypedInt O_WRONLY => 1;
public static UntypedInt R_OK => 4;
public static UntypedInt UNALIGNED => 0;
public static UntypedInt USE___UUIDOF => 0;
public static UntypedInt WIN32 => 1;
public static UntypedInt WIN64 => 1;
public static UntypedInt WINNT => 1;
public static UntypedInt W_OK => 2;
public static UntypedInt X_OK => 1;
public static UntypedInt X_AGLOBAL => 0;
public static UntypedInt X_ANONYMOUS_STRUCT => 0;
public static UntypedInt X_ANONYMOUS_UNION => 0;
public static UntypedInt X_ARGMAX => 100;
public static UntypedInt X_A_ARCH => 0x20;
public static UntypedInt X_A_HIDDEN => 0x02;
public static UntypedInt X_A_NORMAL => 0x00;
public static UntypedInt X_A_RDONLY => 0x01;
public static UntypedInt X_A_SUBDIR => 0x10;
public static UntypedInt X_A_SYSTEM => 0x04;
public static UntypedInt X_CONST_RETURN => 0;
public static UntypedInt X_CRTNOALIAS => 0;
public static UntypedInt X_CRTRESTRICT => 0;
public static UntypedInt X_CRT_ALTERNATIVE_IMPORTED => 0;
public static UntypedInt X_CRT_DIRECTORY_DEFINED => 0;
public static UntypedInt X_CRT_MANAGED_HEAP_DEPRECATE => 0;
public static UntypedInt X_CRT_MEMORY_DEFINED => 0;
public static UntypedInt X_CRT_PACKING => 8;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES_MEMORY => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_COUNT => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_MEMORY => 0;
public static UntypedInt X_DLL => 0;
public static UntypedInt X_ERRCODE_DEFINED => 0;
public static UntypedInt X_FILE_OFFSET_BITS => 64;
public static UntypedInt X_FILE_OFFSET_BITS_SET_LSEEK => 0;
public static UntypedInt X_FILE_OFFSET_BITS_SET_OFFT => 0;
public static UntypedInt X_FINDDATA_T_DEFINED => 0;
public static UntypedInt X_FSIZE_T_DEFINED => 0;
public static UntypedInt X_INC_CRTDEFS => 0;
public static UntypedInt X_INC_CRTDEFS_MACRO => 0;
public static UntypedInt X_INC_FCNTL => 0;
public static UntypedInt X_INC_MINGW_SECAPI => 0;
public static UntypedInt X_INC_STRING => 0;
public static UntypedInt X_INC_STRING_S => 0;
public static UntypedInt X_INC_VADEFS => 0;
public static UntypedInt X_INC__MINGW_H => 0;
public static UntypedInt X_INT128_DEFINED => 0;
public static UntypedInt X_INTEGRAL_MAX_BITS => 64;
public static UntypedInt X_INTPTR_T_DEFINED => 0;
public static UntypedInt X_IO_H_ => 0;
public static UntypedInt X_MT => 0;
public static UntypedInt X_M_AMD64 => 100;
public static UntypedInt X_M_X64 => 100;
public static UntypedInt X_NLSCMPERROR => 2147483647;
public static UntypedInt X_NLSCMP_DEFINED => 0;
public static UntypedInt X_OFF64_T_DEFINED => 0;
public static UntypedInt X_OFF_T_ => 0;
public static UntypedInt X_OFF_T_DEFINED => 0;
public static UntypedInt X_O_ACCMODE => 3;
public static UntypedInt X_O_APPEND => 0x0008;
public static UntypedInt X_O_BINARY => 0x8000;
public static UntypedInt X_O_CREAT => 0x0100;
public static UntypedInt X_O_EXCL => 0x0400;
public static UntypedInt X_O_NOINHERIT => 0x0080;
public static UntypedInt X_O_RANDOM => 0x0010;
public static UntypedInt X_O_RAW => 32768;
public static UntypedInt X_O_RDONLY => 0x0000;
public static UntypedInt X_O_RDWR => 0x0002;
public static UntypedInt X_O_SEQUENTIAL => 0x0020;
public static UntypedInt X_O_SHORT_LIVED => 0x1000;
public static UntypedInt X_O_TEMPORARY => 0x0040;
public static UntypedInt X_O_TEXT => 0x4000;
public static UntypedInt X_O_TRUNC => 0x0200;
public static UntypedInt X_O_U16TEXT => 0x20000;
public static UntypedInt X_O_U8TEXT => 0x40000;
public static UntypedInt X_O_WRONLY => 0x0001;
public static UntypedInt X_O_WTEXT => 0x10000;
public static UntypedInt X_PGLOBAL => 0;
public static UntypedInt X_PTRDIFF_T_ => 0;
public static UntypedInt X_PTRDIFF_T_DEFINED => 0;
public static UntypedInt X_REENTRANT => 1;
public static UntypedInt X_RSIZE_T_DEFINED => 0;
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
public static UntypedInt X_WConst_return => 0;
public static UntypedInt X_WFINDDATA_T_DEFINED => 0;
public static UntypedInt X_WIN32 => 1;
public static UntypedInt X_WIN32_WINNT => 0x502;
public static UntypedInt X_WIN64 => 1;
public static UntypedInt X_WINT_T => 0;
public static UntypedInt X_WIO_DEFINED => 0;
public static UntypedInt X_WSTRING_DEFINED => 0;
public static UntypedInt X_WSTRING_S_DEFINED => 0;

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

partial struct X_finddata32_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public X__time32_t Ftime_create;
    public X__time32_t Ftime_access;
    public X__time32_t Ftime_write;
    public X_fsize_t Fsize;
    public array<int8> Fname = new(260);
}

partial struct X_finddata32i64_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public X__time32_t Ftime_create;
    public X__time32_t Ftime_access;
    public X__time32_t Ftime_write;
    public int64 Fsize;
    public array<int8> Fname = new(260);
    public array<byte> F__ccgo_pad1 = new(4);
}

partial struct X_finddata64i32_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public array<byte> F__ccgo_pad1 = new(4);
    public X__time64_t Ftime_create;
    public X__time64_t Ftime_access;
    public X__time64_t Ftime_write;
    public X_fsize_t Fsize;
    public array<int8> Fname = new(260);
}

partial struct X__finddata64_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public array<byte> F__ccgo_pad1 = new(4);
    public X__time64_t Ftime_create;
    public X__time64_t Ftime_access;
    public X__time64_t Ftime_write;
    public int64 Fsize;
    public array<int8> Fname = new(260);
    public array<byte> F__ccgo_pad2 = new(4);
}

partial struct X_wfinddata32_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public X__time32_t Ftime_create;
    public X__time32_t Ftime_access;
    public X__time32_t Ftime_write;
    public X_fsize_t Fsize;
    public array<Wchar_t> Fname = new(260);
}

partial struct X_wfinddata32i64_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public X__time32_t Ftime_create;
    public X__time32_t Ftime_access;
    public X__time32_t Ftime_write;
    public int64 Fsize;
    public array<Wchar_t> Fname = new(260);
}

partial struct X_wfinddata64i32_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public array<byte> F__ccgo_pad1 = new(4);
    public X__time64_t Ftime_create;
    public X__time64_t Ftime_access;
    public X__time64_t Ftime_write;
    public X_fsize_t Fsize;
    public array<Wchar_t> Fname = new(260);
    public array<byte> F__ccgo_pad2 = new(4);
}

partial struct X_wfinddata64_tᴛ1 /*dyn*/ {
    public uint32 Fattrib;
    public array<byte> F__ccgo_pad1 = new(4);
    public X__time64_t Ftime_create;
    public X__time64_t Ftime_access;
    public X__time64_t Ftime_write;
    public int64 Fsize;
    public array<Wchar_t> Fname = new(260);
}

internal static int8 _ᴛ5ʗ;

} // end fcntl_package
