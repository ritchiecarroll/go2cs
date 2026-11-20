global using Ptrdiff_t = long;
global using Size_t = ulong;
global using Wchar_t = ushort;
global using X__int128_t = go.modernc.org.libc.time_package.X__int128_tᴛ1;
global using X__uint128_t = go.modernc.org.libc.time_package.X__uint128_tᴛ1;
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
global using Threadlocaleinfostruct = go.modernc.org.libc.time_package.Threadlocaleinfostructᴛ1;
global using Pthreadlocinfo = go.uintptr;
global using Pthreadmbcinfo = go.uintptr;
global using Localeinfo_struct = go.modernc.org.libc.time_package.Localeinfo_structᴛ1;
global using X_locale_tstruct = go.modernc.org.libc.time_package.Localeinfo_structᴛ1;
global using X_locale_t = go.uintptr;
global using TagLC_ID = go.modernc.org.libc.time_package.TagLC_IDᴛ1;
global using LC_ID = go.modernc.org.libc.time_package.TagLC_IDᴛ1;
global using LPLC_ID = go.uintptr;
global using Threadlocinfo = go.modernc.org.libc.time_package.Threadlocaleinfostructᴛ1;
global using Clock_t = int;
global using Tm = go.modernc.org.libc.time_package.Tmᴛ1;
global using Timeval = go.modernc.org.libc.time_package.Timevalᴛ1;
global using Timezone = go.modernc.org.libc.time_package.Timezoneᴛ1;
global using X__timeb32 = go.modernc.org.libc.time_package.X__timeb32ᴛ1;
global using Timeb = go.modernc.org.libc.time_package.Timebᴛ1;
global using X__timeb64 = go.modernc.org.libc.time_package.X__timeb64ᴛ1;
global using Timespec = go.modernc.org.libc.time_package.Timespecᴛ1;
global using Itimerspec = go.modernc.org.libc.time_package.Itimerspecᴛ1;
global using Clockid_t = int;

namespace go.modernc.org.libc;

using math = math_package;
using reflect = reflect_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class time_package {

internal static float64 _ᴛ1ʗ = math.Pi;
internal static reflect.Kind _ᴛ2ʗ;
internal static atomic.Value _ᴛ3ʗ;
internal static @unsafe.Pointer _ᴛ4ʗ;

public static UntypedInt CLK_TCK => 1000;
public static UntypedInt CLOCKS_PER_SEC => 1000;
public static UntypedInt CLOCK_MONOTONIC => 1;
public static UntypedInt CLOCK_PROCESS_CPUTIME_ID => 2;
public static UntypedInt CLOCK_REALTIME => 0;
public static UntypedInt CLOCK_THREAD_CPUTIME_ID => 3;
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
public static UntypedInt MINGW_DDK_H => 0;
public static UntypedInt MINGW_DDRAW_VERSION => 7;
public static UntypedInt MINGW_HAS_DDK_H => 1;
public static UntypedInt MINGW_HAS_DDRAW_H => 1;
public static UntypedInt MINGW_HAS_SECURE_API => 1;
public static UntypedInt MINGW_SDK_INIT => 0;
public static UntypedInt TIMER_ABSTIME => 1;
public static UntypedInt UNALIGNED => 0;
public static UntypedInt USE___UUIDOF => 0;
public static UntypedInt WIN32 => 1;
public static UntypedInt WIN64 => 1;
public static UntypedInt WINNT => 1;
public static UntypedInt WINPTHREAD_API => 0;
public static UntypedInt WIN_PTHREADS_TIME_H => 0;
public static UntypedInt X_AGLOBAL => 0;
public static UntypedInt X_ANONYMOUS_STRUCT => 0;
public static UntypedInt X_ANONYMOUS_UNION => 0;
public static UntypedInt X_ARGMAX => 100;
public static UntypedInt X_CLOCK_T_DEFINED => 0;
public static UntypedInt X_CONST_RETURN => 0;
public static UntypedInt X_CRTNOALIAS => 0;
public static UntypedInt X_CRTRESTRICT => 0;
public static UntypedInt X_CRT_ALTERNATIVE_IMPORTED => 0;
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
public static UntypedInt X_INC_MINGW_SECAPI => 0;
public static UntypedInt X_INC_VADEFS => 0;
public static UntypedInt X_INC_WTIME_INL => 0;
public static UntypedInt X_INC_WTIME_S_INL => 0;
public static UntypedInt X_INC__MINGW_H => 0;
public static UntypedInt X_INT128_DEFINED => 0;
public static UntypedInt X_INTEGRAL_MAX_BITS => 64;
public static UntypedInt X_INTPTR_T_DEFINED => 0;
public static UntypedInt X_MT => 0;
public static UntypedInt X_M_AMD64 => 100;
public static UntypedInt X_M_X64 => 100;
public static UntypedInt X_PGLOBAL => 0;
public static UntypedInt X_POSIX_CPUTIME => 200809;
public static UntypedInt X_POSIX_MONOTONIC_CLOCK => 200809;
public static UntypedInt X_POSIX_THREAD_CPUTIME => 200809;
public static UntypedInt X_POSIX_TIMERS => 200809;
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
public static UntypedInt X_TIMEB_DEFINED => 0;
public static UntypedInt X_TIMEB_H_ => 0;
public static UntypedInt X_TIMEB_H_S => 0;
public static UntypedInt X_TIMESPEC_DEFINED => 0;
public static UntypedInt X_TIMEVAL_DEFINED => 0;
public static UntypedInt X_TIMEZONE_DEFINED => 0;
public static UntypedInt X_TIME_H_ => 0;
public static UntypedInt X_TIME_T_DEFINED => 0;
public static UntypedInt X_TM_DEFINED => 0;
public static UntypedInt X_UINTPTR_T_DEFINED => 0;
public static UntypedInt X_VA_LIST_DEFINED => 0;
public static UntypedInt X_W64 => 0;
public static UntypedInt X_WCHAR_T_DEFINED => 0;
public static UntypedInt X_WCTYPE_T_DEFINED => 0;
public static UntypedInt X_WIN32 => 1;
public static UntypedInt X_WIN32_WINNT => 0x502;
public static UntypedInt X_WIN64 => 1;
public static UntypedInt X_WINT_T => 0;
public static UntypedInt X_WTIME_DEFINED => 0;

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

partial struct Tmᴛ1 /*dyn*/ {
    public int32 Ftm_sec;
    public int32 Ftm_min;
    public int32 Ftm_hour;
    public int32 Ftm_mday;
    public int32 Ftm_mon;
    public int32 Ftm_year;
    public int32 Ftm_wday;
    public int32 Ftm_yday;
    public int32 Ftm_isdst;
}

public static readonly ж<int32> ᏑXdaylight = new StaticBox<int32>(default(int32));
public static ref int32 Xdaylight => ref ((StaticBox<int32>)ᏑXdaylight).ValueSlot;

public static readonly ж<int32> ᏑXtimezone = new StaticBox<int32>(default(int32));
public static ref int32 Xtimezone => ref ((StaticBox<int32>)ᏑXtimezone).ValueSlot;
public static readonly ж<array<uintptr>> ᏑXtzname = new StaticBox<array<uintptr>>(new array<uintptr>(2));
public static ref array<uintptr> Xtzname => ref ((StaticBox<array<uintptr>>)ᏑXtzname).ValueSlot;

partial struct Timevalᴛ1 /*dyn*/ {
    public int32 Ftv_sec;
    public int32 Ftv_usec;
}

partial struct Timezoneᴛ1 /*dyn*/ {
    public int32 Ftz_minuteswest;
    public int32 Ftz_dsttime;
}

partial struct X__timeb32ᴛ1 /*dyn*/ {
    public X__time32_t Ftime;
    public uint16 Fmillitm;
    public int16 Ftimezone;
    public int16 Fdstflag;
    public array<byte> F__ccgo_pad1 = new(2);
}

partial struct Timebᴛ1 /*dyn*/ {
    public Time_t Ftime;
    public uint16 Fmillitm;
    public int16 Ftimezone;
    public int16 Fdstflag;
    public array<byte> F__ccgo_pad1 = new(2);
}

partial struct X__timeb64ᴛ1 /*dyn*/ {
    public X__time64_t Ftime;
    public uint16 Fmillitm;
    public int16 Ftimezone;
    public int16 Fdstflag;
    public array<byte> F__ccgo_pad1 = new(2);
}

partial struct Timespecᴛ1 /*dyn*/ {
    public Time_t Ftv_sec;
    public int32 Ftv_nsec;
    public array<byte> F__ccgo_pad1 = new(4);
}



partial struct Itimerspecᴛ1 /*dyn*/ {
    public Timespecᴛ1 Fit_interval;
    public Timespecᴛ1 Fit_value;
}

internal static int8 _ᴛ5ʗ;

} // end time_package
