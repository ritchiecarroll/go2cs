global using Ptrdiff_t = long;
global using Size_t = ulong;
global using Wchar_t = ushort;
global using X__int128_t = go.modernc.org.libc.pthread_package.X__int128_tᴛ1;
global using X__uint128_t = go.modernc.org.libc.pthread_package.X__uint128_tᴛ1;
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
global using Threadlocaleinfostruct = go.modernc.org.libc.pthread_package.Threadlocaleinfostructᴛ1;
global using Pthreadlocinfo = go.uintptr;
global using Pthreadmbcinfo = go.uintptr;
global using Localeinfo_struct = go.modernc.org.libc.pthread_package.Localeinfo_structᴛ1;
global using X_locale_tstruct = go.modernc.org.libc.pthread_package.Localeinfo_structᴛ1;
global using X_locale_t = go.uintptr;
global using TagLC_ID = go.modernc.org.libc.pthread_package.TagLC_IDᴛ1;
global using LC_ID = go.modernc.org.libc.pthread_package.TagLC_IDᴛ1;
global using LPLC_ID = go.uintptr;
global using Threadlocinfo = go.modernc.org.libc.pthread_package.Threadlocaleinfostructᴛ1;
global using Max_align_t = go.modernc.org.libc.pthread_package.Max_align_tᴛ1;
global using X_ino_t = ushort;
global using Ino_t = ushort;
global using X_dev_t = uint;
global using Dev_t = uint;
global using X_pid_t = long;
global using Pid_t = long;
global using X_mode_t = ushort;
global using Mode_t = ushort;
global using X_off_t = int;
global using Off32_t = int;
global using X_off64_t = long;
global using Off64_t = long;
global using Off_t = long;
global using Useconds_t = uint;
global using Timespec = go.modernc.org.libc.pthread_package.Timespecᴛ1;
global using Itimerspec = go.modernc.org.libc.pthread_package.Itimerspecᴛ1;
global using X_sigset_t = ulong;
global using X_PVFV = go.uintptr;
global using X_PIFV = go.uintptr;
global using X_PVFI = go.uintptr;
global using X_onexit_table_t1 = go.modernc.org.libc.pthread_package.X_onexit_table_t1ᴛ1;
global using X_onexit_table_t = go.modernc.org.libc.pthread_package.X_onexit_table_t1ᴛ1;
global using X_onexit_t = go.uintptr;
global using Sig_atomic_t = int;
global using X__p_sig_fn_t = go.uintptr;
global using X__timeb32 = go.modernc.org.libc.pthread_package.X__timeb32ᴛ1;
global using Timeb = go.modernc.org.libc.pthread_package.Timebᴛ1;
global using X__timeb64 = go.modernc.org.libc.pthread_package.X__timeb64ᴛ1;
global using Pthread_once_t = int;
global using Pthread_mutexattr_t = uint;
global using Pthread_key_t = uint;
global using Pthread_barrierattr_t = go.uintptr;
global using Pthread_condattr_t = int;
global using Pthread_rwlockattr_t = int;
global using Pthread_t = ulong;
global using X_pthread_cleanup1 = go.modernc.org.libc.pthread_package.X_pthread_cleanup1ᴛ1;
global using X_pthread_cleanup = go.modernc.org.libc.pthread_package.X_pthread_cleanup1ᴛ1;
global using Sched_param = go.modernc.org.libc.pthread_package.Sched_paramᴛ1;
global using Pthread_attr_t1 = go.modernc.org.libc.pthread_package.Pthread_attr_t1ᴛ1;
global using Pthread_attr_t = go.modernc.org.libc.pthread_package.Pthread_attr_t1ᴛ1;
global using Pthread_spinlock_t = go.uintptr;
global using Pthread_mutex_t = go.uintptr;
global using Pthread_cond_t = go.uintptr;
global using Pthread_rwlock_t = go.uintptr;
global using Pthread_barrier_t = go.uintptr;
global using Clockid_t = int;

namespace go.modernc.org.libc;

using math = math_package;
using reflect = reflect_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using sync;

partial class pthread_package {

internal static float64 _ᴛ1ʗ = math.Pi;
internal static reflect.Kind _ᴛ2ʗ;
internal static atomic.Value _ᴛ3ʗ;
internal static @unsafe.Pointer _ᴛ4ʗ;

public static UntypedInt CHAR_BIT => 8;
public static UntypedInt CHAR_MAX => 127;
public static UntypedInt CHAR_MIN => -128;
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
public static UntypedInt INT_MAX => 2147483647;
public static UntypedInt INT_MIN => -2147483648;
public static UntypedInt LLONG_MAX => 9223372036854775807;
public static UntypedInt LLONG_MIN => -9223372036854775808;
public static UntypedInt LONG_LONG_MAX => 9223372036854775807;
public static UntypedInt LONG_LONG_MIN => -9223372036854775808;
public static UntypedInt LONG_MAX => 2147483647;
public static UntypedInt LONG_MIN => -2147483648;
public static UntypedInt MAX_READ_LOCKS => 2147483646;
public static UntypedInt MB_LEN_MAX => 5;
public static UntypedInt MINGW_DDK_H => 0;
public static UntypedInt MINGW_DDRAW_VERSION => 7;
public static UntypedInt MINGW_HAS_DDK_H => 1;
public static UntypedInt MINGW_HAS_DDRAW_H => 1;
public static UntypedInt MINGW_HAS_SECURE_API => 1;
public static UntypedInt MINGW_SDK_INIT => 0;
public static UntypedInt NSIG => 23;
public static UntypedInt OLD_P_OVERLAY => 2;
public static UntypedInt PATH_MAX => 260;
public static UntypedInt PTHREAD_BARRIER_SERIAL_THREAD => 1;
public static UntypedInt PTHREAD_CANCEL_ASYNCHRONOUS => 0x02;
public static UntypedInt PTHREAD_CANCEL_DEFERRED => 0;
public static UntypedInt PTHREAD_CANCEL_DISABLE => 0;
public static UntypedInt PTHREAD_CANCEL_ENABLE => 0x01;
public static UntypedInt PTHREAD_CREATE_DETACHED => 0x04;
public static UntypedInt PTHREAD_CREATE_JOINABLE => 0;
public static UntypedInt PTHREAD_DEFAULT_ATTR => 1;
public static UntypedInt PTHREAD_DESTRUCTOR_ITERATIONS => 256;
public static UntypedInt PTHREAD_EXPLICIT_SCHED => 0;
public static UntypedInt PTHREAD_INHERIT_SCHED => 0x08;
public static UntypedInt PTHREAD_KEYS_MAX => 1048576;
public static UntypedInt PTHREAD_MUTEX_ADAPTIVE_NP => 0;
public static UntypedInt PTHREAD_MUTEX_DEFAULT => 0;
public static UntypedInt PTHREAD_MUTEX_ERRORCHECK => 1;
public static UntypedInt PTHREAD_MUTEX_ERRORCHECK_NP => 1;
public static UntypedInt PTHREAD_MUTEX_FAST_NP => 0;
public static UntypedInt PTHREAD_MUTEX_NORMAL => 0;
public static UntypedInt PTHREAD_MUTEX_PRIVATE => 0;
public static UntypedInt PTHREAD_MUTEX_RECURSIVE => 2;
public static UntypedInt PTHREAD_MUTEX_RECURSIVE_NP => 2;
public static UntypedInt PTHREAD_MUTEX_SHARED => 1;
public static UntypedInt PTHREAD_MUTEX_TIMED_NP => 0;
public static UntypedInt PTHREAD_ONCE_INIT => 0;
public static UntypedInt PTHREAD_PRIO_INHERIT => 8;
public static UntypedInt PTHREAD_PRIO_MULT => 32;
public static UntypedInt PTHREAD_PRIO_NONE => 0;
public static UntypedInt PTHREAD_PRIO_PROTECT => 16;
public static UntypedInt PTHREAD_PROCESS_PRIVATE => 0;
public static UntypedInt PTHREAD_PROCESS_SHARED => 1;
public static UntypedInt PTHREAD_SCOPE_PROCESS => 0;
public static UntypedInt PTHREAD_SCOPE_SYSTEM => 0x10;
public static UntypedInt PTHREAD_THREADS_MAX => 2019;
public static UntypedInt P_DETACH => 4;
public static UntypedInt P_NOWAIT => 1;
public static UntypedInt P_NOWAITO => 3;
public static UntypedInt P_OVERLAY => 2;
public static UntypedInt P_WAIT => 0;
public static UntypedInt RWLS_PER_THREAD => 8;
public static UntypedInt SCHAR_MAX => 127;
public static UntypedInt SCHAR_MIN => -128;
public static UntypedInt SCHED_FIFO => 1;
public static UntypedInt SCHED_MAX => 2;
public static UntypedInt SCHED_MIN => 0;
public static UntypedInt SCHED_OTHER => 0;
public static UntypedInt SCHED_RR => 2;
public static UntypedInt SEM_NSEMS_MAX => 1024;
public static UntypedInt SHRT_MAX => 32767;
public static UntypedInt SHRT_MIN => -32768;
public static UntypedInt SIGABRT => 22;
public static UntypedInt SIGABRT2 => 22;
public static UntypedInt SIGABRT_COMPAT => 6;
public static UntypedInt SIGBREAK => 21;
public static UntypedInt SIGFPE => 8;
public static UntypedInt SIGILL => 4;
public static UntypedInt SIGINT => 2;
public static UntypedInt SIGSEGV => 11;
public static UntypedInt SIGTERM => 15;
public static UntypedInt SIG_BLOCK => 0;
public static UntypedInt SIG_SETMASK => 2;
public static UntypedInt SIG_UNBLOCK => 1;
public static UntypedInt SIZE_MAX => 18446744073709551615;
public static UntypedInt SSIZE_MAX => 9223372036854775807;
public static UntypedInt STRUNCATE => 80;
public static UntypedInt UCHAR_MAX => 255;
public static UntypedInt UINT_MAX => 4294967295;
public static UntypedInt ULLONG_MAX => 18446744073709551615;
public static UntypedInt ULONG_LONG_MAX => 18446744073709551615;
public static UntypedInt ULONG_MAX => 4294967295;
public static UntypedInt UNALIGNED => 0;
public static UntypedInt USE___UUIDOF => 0;
public static UntypedInt USHRT_MAX => 65535;
public static UntypedInt WAIT_CHILD => 0;
public static UntypedInt WAIT_GRANDCHILD => 1;
public static UntypedInt WIN32 => 1;
public static UntypedInt WIN64 => 1;
public static UntypedInt WINNT => 1;
public static UntypedInt WINPTHREAD_API => 0;
public static UntypedInt WIN_PTHREADS_H => 0;
public static UntypedInt WIN_PTHREADS_PTHREAD_COMPAT_H => 0;
public static UntypedInt WIN_PTHREADS_SIGNAL_H => 0;
public static UntypedInt WIN_PTHREADS_UNISTD_H => 0;
public static UntypedInt X_AGLOBAL => 0;
public static UntypedInt X_ANONYMOUS_STRUCT => 0;
public static UntypedInt X_ANONYMOUS_UNION => 0;
public static UntypedInt X_ANSI_STDDEF_H => 0;
public static UntypedInt X_ARGMAX => 100;
public static UntypedInt X_CONST_RETURN => 0;
public static UntypedInt X_CRTNOALIAS => 0;
public static UntypedInt X_CRTRESTRICT => 0;
public static UntypedInt X_CRT_ALTERNATIVE_IMPORTED => 0;
public static UntypedInt X_CRT_ERRNO_DEFINED => 0;
public static UntypedInt X_CRT_GETPID_DEFINED => 0;
public static UntypedInt X_CRT_MANAGED_HEAP_DEPRECATE => 0;
public static UntypedInt X_CRT_PACKING => 8;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_SECURE_NAMES_MEMORY => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_COUNT => 0;
public static UntypedInt X_CRT_SECURE_CPP_OVERLOAD_STANDARD_NAMES_MEMORY => 0;
public static UntypedInt X_CRT_SYSTEM_DEFINED => 0;
public static UntypedInt X_CRT_TERMINATE_DEFINED => 0;
public static UntypedInt X_CRT_WSYSTEM_DEFINED => 0;
public static UntypedInt X_DEV_T_DEFINED => 0;
public static UntypedInt X_DLL => 0;
public static UntypedInt X_ERRCODE_DEFINED => 0;
public static UntypedInt X_FILE_OFFSET_BITS => 64;
public static UntypedInt X_FILE_OFFSET_BITS_SET_OFFT => 0;
public static UntypedInt X_GCC_LIMITS_H_ => 0;
public static UntypedInt X_GCC_MAX_ALIGN_T => 0;
public static UntypedInt X_GTHREAD_USE_MUTEX_INIT_FUNC => 1;
public static UntypedInt X_I16_MAX => 32767;
public static UntypedInt X_I16_MIN => -32768;
public static UntypedInt X_I32_MAX => 2147483647;
public static UntypedInt X_I32_MIN => -2147483648;
public static UntypedInt X_I64_MAX => 9223372036854775807;
public static UntypedInt X_I64_MIN => -9223372036854775808;
public static UntypedInt X_I8_MAX => 127;
public static UntypedInt X_I8_MIN => -128;
public static UntypedInt X_INC_CORECRT_STARTUP => 0;
public static UntypedInt X_INC_CRTDEFS => 0;
public static UntypedInt X_INC_CRTDEFS_MACRO => 0;
public static UntypedInt X_INC_ERRNO => 0;
public static UntypedInt X_INC_LIMITS => 0;
public static UntypedInt X_INC_MINGW_SECAPI => 0;
public static UntypedInt X_INC_PROCESS => 0;
public static UntypedInt X_INC_SIGNAL => 0;
public static UntypedInt X_INC_STDDEF => 0;
public static UntypedInt X_INC_TYPES => 0;
public static UntypedInt X_INC_VADEFS => 0;
public static UntypedInt X_INC__MINGW_H => 0;
public static UntypedInt X_INO_T_DEFINED => 0;
public static UntypedInt X_INT128_DEFINED => 0;
public static UntypedInt X_INTEGRAL_MAX_BITS => 64;
public static UntypedInt X_INTPTR_T_DEFINED => 0;
public static UntypedInt X_LIMITS_H___ => 0;
public static UntypedInt X_MODE_T_ => 0;
public static UntypedInt X_MT => 0;
public static UntypedInt X_M_AMD64 => 100;
public static UntypedInt X_M_X64 => 100;
public static UntypedInt X_OFF64_T_DEFINED => 0;
public static UntypedInt X_OFF_T_ => 0;
public static UntypedInt X_OFF_T_DEFINED => 0;
public static UntypedInt X_OLD_P_OVERLAY => 2;
public static UntypedInt X_PGLOBAL => 0;
public static UntypedInt X_PID_T_ => 0;
public static UntypedInt X_POSIX_BARRIERS => 200112;
public static UntypedInt X_POSIX_CLOCK_SELECTION => 200112;
public static UntypedInt X_POSIX_READER_WRITER_LOCKS => 200112;
public static UntypedInt X_POSIX_SEMAPHORES => 200112;
public static UntypedInt X_POSIX_SEM_NSEMS_MAX => 256;
public static UntypedInt X_POSIX_SPIN_LOCKS => 200112;
public static UntypedInt X_POSIX_THREADS => 200112;
public static UntypedInt X_POSIX_THREAD_DESTRUCTOR_ITERATIONS => 256;
public static UntypedInt X_POSIX_THREAD_KEYS_MAX => 1048576;
public static UntypedInt X_POSIX_THREAD_SAFE_FUNCTIONS => 200112;
public static UntypedInt X_POSIX_TIMEOUTS => 200112;
public static UntypedInt X_PTRDIFF_T_ => 0;
public static UntypedInt X_PTRDIFF_T_DEFINED => 0;
public static UntypedInt X_P_DETACH => 4;
public static UntypedInt X_P_NOWAIT => 1;
public static UntypedInt X_P_NOWAITO => 3;
public static UntypedInt X_P_OVERLAY => 2;
public static UntypedInt X_P_WAIT => 0;
public static UntypedInt X_REENTRANT => 1;
public static UntypedInt X_RSIZE_T_DEFINED => 0;
public static UntypedInt X_SECURECRT_ERRCODE_VALUES_DEFINED => 0;
public static UntypedInt X_SECURECRT_FILL_BUFFER_PATTERN => 0xFD;
public static UntypedInt X_SIGSET_T_ => 0;
public static UntypedInt X_SIG_ATOMIC_T_DEFINED => 0;
public static UntypedInt X_SIZE_T_DEFINED => 0;
public static UntypedInt X_SPAWNV_DEFINED => 0;
public static UntypedInt X_SSIZE_T_DEFINED => 0;
public static UntypedInt X_STDDEF_H => 0;
public static UntypedInt X_STDDEF_H_ => 0;
public static UntypedInt X_TAGLC_ID_DEFINED => 0;
public static UntypedInt X_THREADLOCALEINFO => 0;
public static UntypedInt X_TIME32_T_DEFINED => 0;
public static UntypedInt X_TIME64_T_DEFINED => 0;
public static UntypedInt X_TIMEB_DEFINED => 0;
public static UntypedInt X_TIMEB_H_ => 0;
public static UntypedInt X_TIMEB_H_S => 0;
public static UntypedInt X_TIMESPEC_DEFINED => 0;
public static UntypedInt X_TIME_T_DEFINED => 0;
public static UntypedInt X_UI16_MAX => 0xffff;
public static UntypedInt X_UI32_MAX => 0xffffffff;
public static UntypedInt X_UI64_MAX => 0xffffffffffffffff;
public static UntypedInt X_UI8_MAX => 0xff;
public static UntypedInt X_UINTPTR_T_DEFINED => 0;
public static UntypedInt X_VA_LIST_DEFINED => 0;
public static UntypedInt X_W64 => 0;
public static UntypedInt X_WAIT_CHILD => 0;
public static UntypedInt X_WAIT_GRANDCHILD => 1;
public static UntypedInt X_WCHAR_T_DEFINED => 0;
public static UntypedInt X_WCTYPE_T_DEFINED => 0;
public static UntypedInt X_WEXEC_DEFINED => 0;
public static UntypedInt X_WIN32 => 1;
public static UntypedInt X_WIN32_WINNT => 0x502;
public static UntypedInt X_WIN64 => 1;
public static UntypedInt X_WINT_T => 0;
public static UntypedInt X_WSPAWN_DEFINED => 0;

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

partial struct Max_align_tᴛ1 /*dyn*/ {
    public int64 F__max_align_ll;
    public float64 F__max_align_ld;
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

partial struct X_onexit_table_t1ᴛ1 /*dyn*/ {
    public uintptr F_first;
    public uintptr F_last;
    public uintptr F_end;
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

partial struct X_pthread_cleanup1ᴛ1 /*dyn*/ {
    public uintptr Ffunc;
    public uintptr Farg;
    public uintptr Fnext;
}

partial struct Sched_paramᴛ1 /*dyn*/ {
    public int32 Fsched_priority;
}


partial struct Pthread_attr_t1ᴛ1 /*dyn*/ {
    public uint32 Fp_state;
    public array<byte> F__ccgo_pad1 = new(4);
    public uintptr Fstack;
    public Size_t Fs_size;
    public Sched_paramᴛ1 Fparam;
    public array<byte> F__ccgo_pad2 = new(4);
}

internal static int8 _ᴛ5ʗ;

} // end pthread_package
