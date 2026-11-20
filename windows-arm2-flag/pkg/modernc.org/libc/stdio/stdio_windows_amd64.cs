global using Ptrdiff_t = long;
global using Size_t = ulong;
global using Wchar_t = ushort;
global using X__int128_t = go.modernc.org.libc.stdio_package.X__int128_tᴛ1;
global using X__uint128_t = go.modernc.org.libc.stdio_package.X__uint128_tᴛ1;
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
global using Threadlocaleinfostruct = go.modernc.org.libc.stdio_package.Threadlocaleinfostructᴛ1;
global using Pthreadlocinfo = go.uintptr;
global using Pthreadmbcinfo = go.uintptr;
global using Localeinfo_struct = go.modernc.org.libc.stdio_package.Localeinfo_structᴛ1;
global using X_locale_tstruct = go.modernc.org.libc.stdio_package.Localeinfo_structᴛ1;
global using X_locale_t = go.uintptr;
global using TagLC_ID = go.modernc.org.libc.stdio_package.TagLC_IDᴛ1;
global using LC_ID = go.modernc.org.libc.stdio_package.TagLC_IDᴛ1;
global using LPLC_ID = go.uintptr;
global using Threadlocinfo = go.modernc.org.libc.stdio_package.Threadlocaleinfostructᴛ1;
global using X_iobuf = go.modernc.org.libc.stdio_package.X_iobufᴛ1;
global using FILE = go.modernc.org.libc.stdio_package.X_iobufᴛ1;
global using X_off_t = int;
global using Off32_t = int;
global using X_off64_t = long;
global using Off64_t = long;
global using Off_t = long;
global using Fpos_t = long;

namespace go.modernc.org.libc;

using math = math_package;
using reflect = reflect_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class stdio_package {

internal static float64 _ᴛ1ʗ = math.Pi;
internal static reflect.Kind _ᴛ2ʗ;
internal static atomic.Value _ᴛ3ʗ;
internal static @unsafe.Pointer _ᴛ4ʗ;

public static UntypedInt BUFSIZ => 512;
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
public static UntypedInt EOF => -1;
public static UntypedInt FILENAME_MAX => 260;
public static UntypedInt FOPEN_MAX => 20;
public static UntypedInt MINGW_DDK_H => 0;
public static UntypedInt MINGW_DDRAW_VERSION => 7;
public static UntypedInt MINGW_HAS_DDK_H => 1;
public static UntypedInt MINGW_HAS_DDRAW_H => 1;
public static UntypedInt MINGW_HAS_SECURE_API => 1;
public static UntypedInt MINGW_SDK_INIT => 0;
public static UntypedInt SEEK_CUR => 1;
public static UntypedInt SEEK_END => 2;
public static UntypedInt SEEK_SET => 0;
public static UntypedInt STDERR_FILENO => 2;
public static UntypedInt STDIN_FILENO => 0;
public static UntypedInt STDOUT_FILENO => 1;
public static UntypedInt SYS_OPEN => 20;
public static UntypedInt TMP_MAX => 32767;
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
public static UntypedInt X_CRT_DIRECTORY_DEFINED => 0;
public static UntypedInt X_CRT_MANAGED_HEAP_DEPRECATE => 0;
public static UntypedInt X_CRT_PACKING => 8;
public static UntypedInt X_CRT_PERROR_DEFINED => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES_MEMORY => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_COUNT => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_MEMORY => 0;
public static UntypedInt X_CRT_WPERROR_DEFINED => 0;
public static UntypedInt X_DLL => 0;
public static UntypedInt X_ERRCODE_DEFINED => 0;
public static UntypedInt X_FILE_DEFINED => 0;
public static UntypedInt X_FILE_OFFSET_BITS => 64;
public static UntypedInt X_FILE_OFFSET_BITS_SET_FSEEKO => 0;
public static UntypedInt X_FILE_OFFSET_BITS_SET_FTELLO => 0;
public static UntypedInt X_FILE_OFFSET_BITS_SET_OFFT => 0;
public static UntypedInt X_FPOS_T_DEFINED => 0;
public static UntypedInt X_INC_CRTDEFS => 0;
public static UntypedInt X_INC_CRTDEFS_MACRO => 0;
public static UntypedInt X_INC_MINGW_SECAPI => 0;
public static UntypedInt X_INC_STDIO => 0;
public static UntypedInt X_INC_STDIO_S => 0;
public static UntypedInt X_INC_SWPRINTF_INL => 0;
public static UntypedInt X_INC_VADEFS => 0;
public static UntypedInt X_INC__MINGW_H => 0;
public static UntypedInt X_INT128_DEFINED => 0;
public static UntypedInt X_INTEGRAL_MAX_BITS => 64;
public static UntypedInt X_INTPTR_T_DEFINED => 0;
public static UntypedInt X_IOB_ENTRIES => 20;
public static UntypedInt X_IOEOF => 0x0010;
public static UntypedInt X_IOERR => 0x0020;
public static UntypedInt X_IOFBF => 0x0000;
public static UntypedInt X_IOLBF => 0x0040;
public static UntypedInt X_IOMYBUF => 0x0008;
public static UntypedInt X_IONBF => 0x0004;
public static UntypedInt X_IOREAD => 0x0001;
public static UntypedInt X_IORW => 0x0080;
public static UntypedInt X_IOSTRG => 0x0040;
public static UntypedInt X_IOWRT => 0x0002;
public static UntypedInt X_MT => 0;
public static UntypedInt X_M_AMD64 => 100;
public static UntypedInt X_M_X64 => 100;
public static UntypedInt X_NFILE => 512;
public static UntypedInt X_NSTREAM_ => 512;
public static UntypedInt X_OFF64_T_DEFINED => 0;
public static UntypedInt X_OFF_T_ => 0;
public static UntypedInt X_OFF_T_DEFINED => 0;
public static UntypedInt X_OLD_P_OVERLAY => 2;
public static UntypedInt X_PGLOBAL => 0;
public static UntypedInt X_PTRDIFF_T_ => 0;
public static UntypedInt X_PTRDIFF_T_DEFINED => 0;
public static UntypedInt X_P_DETACH => 4;
public static UntypedInt X_P_NOWAIT => 1;
public static UntypedInt X_P_NOWAITO => 3;
public static UntypedInt X_P_OVERLAY => 2;
public static UntypedInt X_P_WAIT => 0;
public static readonly @string X_P_tmpdir = "\\"u8;
public static UntypedInt X_REENTRANT => 1;
public static UntypedInt X_RSIZE_T_DEFINED => 0;
public static UntypedInt X_SECURECRT_FILL_BUFFER_PATTERN => 0xFD;
public static UntypedInt X_SIZE_T_DEFINED => 0;
public static UntypedInt X_SPAWNV_DEFINED => 0;
public static UntypedInt X_SSIZE_T_DEFINED => 0;
public static UntypedInt X_STDIO_DEFINED => 0;
public static UntypedInt X_STDIO_S_DEFINED => 0;
public static UntypedInt X_STDSTREAM_DEFINED => 0;
public static UntypedInt X_SYS_OPEN => 20;
public static UntypedInt X_TAGLC_ID_DEFINED => 0;
public static UntypedInt X_THREADLOCALEINFO => 0;
public static UntypedInt X_TIME32_T_DEFINED => 0;
public static UntypedInt X_TIME64_T_DEFINED => 0;
public static UntypedInt X_TIME_T_DEFINED => 0;
public static UntypedInt X_TWO_DIGIT_EXPONENT => 0x1;
public static UntypedInt X_UINTPTR_T_DEFINED => 0;
public static UntypedInt X_VA_LIST_DEFINED => 0;
public static UntypedInt X_W64 => 0;
public static UntypedInt X_WAIT_CHILD => 0;
public static UntypedInt X_WAIT_GRANDCHILD => 1;
public static UntypedInt X_WCHAR_T_DEFINED => 0;
public static UntypedInt X_WCTYPE_T_DEFINED => 0;
public static UntypedInt X_WIN32 => 1;
public static UntypedInt X_WIN32_WINNT => 0x502;
public static UntypedInt X_WIN64 => 1;
public static UntypedInt X_WINT_T => 0;
public static UntypedInt X_WSPAWN_DEFINED => 0;
public static UntypedInt X_WSTDIO_DEFINED => 0;
public static UntypedInt X_WSTDIO_S_DEFINED => 0;

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

partial struct X_iobufᴛ1 /*dyn*/ {
    public uintptr F_ptr;
    public int32 F_cnt;
    public array<byte> F__ccgo_pad1 = new(4);
    public uintptr F_base;
    public int32 F_flag;
    public int32 F_file;
    public int32 F_charbuf;
    public int32 F_bufsiz;
    public uintptr F_tmpfname;
}

internal static int8 _ᴛ5ʗ;

} // end stdio_package
