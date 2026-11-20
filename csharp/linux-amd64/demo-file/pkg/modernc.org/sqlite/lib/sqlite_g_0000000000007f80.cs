global using TSQLiteThread = go.modernc.org.sqlite.lib.sqlite3_package.TSQLiteThreadᴛ1;
global using T_G_fpos64_t = go.modernc.org.libc_package.Tfpos_tᴛ1;
global using T_IO_cookie_io_functions_t = go.modernc.org.libc_package.Tcookie_io_functions_tᴛ1;
global using Tf_owner_ex = go.modernc.org.libc_package.Tf_owner_exᴛ1;
global using Tfile_handle = go.modernc.org.libc_package.Tfile_handleᴛ1;
global using Tpthread_barrierattr_t = go.modernc.org.libc_package.Tpthread_mutexattr_tᴛ1;
global using Tpthread_condattr_t = go.modernc.org.libc_package.Tpthread_mutexattr_tᴛ1;
global using Tpthread_mutexattr_t = go.modernc.org.libc_package.Tpthread_mutexattr_tᴛ1;
global using Tpthread_once_t = int;
global using Tpthread_rwlockattr_t = go.modernc.org.libc_package.Tpthread_rwlockattr_tᴛ1;
global using Tpthread_spinlock_t = int;
global using Tsqlite3_mutex = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_mutexᴛ1;
global using _G_fpos64_t = go.modernc.org.libc_package.Tfpos_tᴛ1;
global using _IO_cookie_io_functions_t = go.modernc.org.libc_package.Tcookie_io_functions_tᴛ1;
global using cpu_set_t = go.modernc.org.sqlite.lib.sqlite3_package.Tcpu_set_tᴛ1;
global using double_t = double;
global using f_owner_ex = go.modernc.org.libc_package.Tf_owner_exᴛ1;
global using file_handle = go.modernc.org.libc_package.Tfile_handleᴛ1;
global using itimerspec = go.modernc.org.libc_package.Titimerspecᴛ1;
global using ptrdiff_t = long;
global using sched_param = go.modernc.org.libc_package.Tsched_paramᴛ1;
global using sqlite3_io_methods = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_io_methodsᴛ1;
global using sqlite3_mutex = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_mutexᴛ1;
global using statx = go.modernc.org.sqlite.lib.sqlite3_package.Tstatxᴛ1;
global using statx_timestamp = go.modernc.org.libc_package.Tstatx_timestampᴛ1;
global using t__isoc_va_list = go.uintptr;
global using t__ptcb = go.modernc.org.sqlite.lib.sqlite3_package.t__ptcbᴛ1;
global using t__sigset_t = go.modernc.org.sqlite.lib.sqlite3_package.Tcpu_set_tᴛ1;
global using timezone = go.modernc.org.sqlite.lib.sqlite3_package.Ttimezoneᴛ1;
global using uint_fast32_t = uint;

namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using System.Runtime.InteropServices;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt AT_EACCESS => 512;

public static UntypedInt AT_EMPTY_PATH => 4096;

public static UntypedInt AT_NO_AUTOMOUNT => 2048;

public static UntypedInt AT_RECURSIVE => 32768;

public static UntypedInt AT_REMOVEDIR => 512;

public static UntypedInt AT_STATX_DONT_SYNC => 16384;

public static UntypedInt AT_STATX_FORCE_SYNC => 8192;

public static UntypedInt AT_STATX_SYNC_AS_STAT => 0;

public static UntypedInt AT_STATX_SYNC_TYPE => 24576;

public static UntypedInt AT_SYMLINK_NOFOLLOW => 256;

public static UntypedInt CLOCK_BOOTTIME => 7;

public static UntypedInt CLOCK_BOOTTIME_ALARM => 9;

public static UntypedInt CLOCK_MONOTONIC_COARSE => 6;

public static UntypedInt CLOCK_REALTIME_ALARM => 8;

public static UntypedInt CLOCK_REALTIME_COARSE => 5;

public static UntypedInt CLOCK_SGI_CYCLE => 10;

public static UntypedInt CLOCK_TAI => 11;

public static UntypedInt CLONE_CHILD_CLEARTID => 2097152;

public static UntypedInt CLONE_CHILD_SETTID => 16777216;

public static UntypedInt CLONE_DETACHED => 4194304;

public static UntypedInt CLONE_FILES => 1024;

public static UntypedInt CLONE_FS => 512;

public static UntypedInt CLONE_IO => 2147483648;

public static UntypedInt CLONE_NEWCGROUP => 33554432;

public static UntypedInt CLONE_NEWIPC => 134217728;

public static UntypedInt CLONE_NEWNET => 1073741824;

public static UntypedInt CLONE_NEWNS => 131072;

public static UntypedInt CLONE_NEWPID => 536870912;

public static UntypedInt CLONE_NEWTIME => 128;

public static UntypedInt CLONE_NEWUSER => 268435456;

public static UntypedInt CLONE_NEWUTS => 67108864;

public static UntypedInt CLONE_PARENT => 32768;

public static UntypedInt CLONE_PARENT_SETTID => 1048576;

public static UntypedInt CLONE_PIDFD => 4096;

public static UntypedInt CLONE_PTRACE => 8192;

public static UntypedInt CLONE_SETTLS => 524288;

public static UntypedInt CLONE_SIGHAND => 2048;

public static UntypedInt CLONE_SYSVSEM => 262144;

public static UntypedInt CLONE_THREAD => 65536;

public static UntypedInt CLONE_UNTRACED => 8388608;

public static UntypedInt CLONE_VFORK => 16384;

public static UntypedInt CLONE_VM => 256;

public static UntypedInt CPU_SETSIZE => 1024;

public static UntypedInt CSIGNAL => 255;

public static UntypedInt DN_ACCESS => 1;

public static UntypedInt DN_ATTRIB => 32;

public static UntypedInt DN_CREATE => 4;

public static UntypedInt DN_DELETE => 8;

public static UntypedInt DN_MODIFY => 2;

public static UntypedInt DN_MULTISHOT => 2147483648;

public static UntypedInt DN_RENAME => 16;

public static UntypedInt EADDRINUSE => 98;

public static UntypedInt EADDRNOTAVAIL => 99;

public static UntypedInt EAFNOSUPPORT => 97;

public static UntypedInt EALREADY => 114;

public static UntypedInt EBADE => 52;

public static UntypedInt EBADFD => 77;

public static UntypedInt EBADMSG => 74;

public static UntypedInt EBADR => 53;

public static UntypedInt EBADRQC => 56;

public static UntypedInt EBADSLT => 57;

public static UntypedInt EBFONT => 59;

public static UntypedInt ECANCELED => 125;

public static UntypedInt ECHRNG => 44;

public static UntypedInt ECONNABORTED => 103;

public static UntypedInt ECONNREFUSED => 111;

public static UntypedInt ECONNRESET => 104;

public static UntypedInt EDEADLK => 35;

public static UntypedInt EDESTADDRREQ => 89;

public static UntypedInt EDOTDOT => 73;

public static UntypedInt EDQUOT => 122;

public static UntypedInt EHOSTDOWN => 112;

public static UntypedInt EHOSTUNREACH => 113;

public static UntypedInt EHWPOISON => 133;

public static UntypedInt EIDRM => 43;

public static UntypedInt EINPROGRESS => 115;

public static UntypedInt EISCONN => 106;

public static UntypedInt EISNAM => 120;

public static UntypedInt EKEYEXPIRED => 127;

public static UntypedInt EKEYREJECTED => 129;

public static UntypedInt EKEYREVOKED => 128;

public static UntypedInt EL2HLT => 51;

public static UntypedInt EL2NSYNC => 45;

public static UntypedInt EL3HLT => 46;

public static UntypedInt EL3RST => 47;

public static UntypedInt ELIBACC => 79;

public static UntypedInt ELIBBAD => 80;

public static UntypedInt ELIBEXEC => 83;

public static UntypedInt ELIBMAX => 82;

public static UntypedInt ELIBSCN => 81;

public static UntypedInt ELNRNG => 48;

public static UntypedInt ELOOP => 40;

public static UntypedInt EMEDIUMTYPE => 124;

public static UntypedInt EMSGSIZE => 90;

public static UntypedInt EMULTIHOP => 72;

public static UntypedInt ENAMETOOLONG => 36;

public static UntypedInt ENAVAIL => 119;

public static UntypedInt ENETDOWN => 100;

public static UntypedInt ENETRESET => 102;

public static UntypedInt ENETUNREACH => 101;

public static UntypedInt ENOANO => 55;

public static UntypedInt ENOBUFS => 105;

public static UntypedInt ENOCSI => 50;

public static UntypedInt ENOKEY => 126;

public static UntypedInt ENOLCK => 37;

public static UntypedInt ENOMEDIUM => 123;

public static UntypedInt ENOMSG => 42;

public static UntypedInt ENOPROTOOPT => 92;

public static UntypedInt ENOSYS => 38;

public static UntypedInt ENOTCONN => 107;

public static UntypedInt ENOTEMPTY => 39;

public static UntypedInt ENOTNAM => 118;

public static UntypedInt ENOTRECOVERABLE => 131;

public static UntypedInt ENOTSOCK => 88;

public static UntypedInt ENOTSUP => 95;

public static UntypedInt ENOTUNIQ => 76;

public static UntypedInt EOPNOTSUPP => 95;

public static UntypedInt EOVERFLOW => 75;

public static UntypedInt EOWNERDEAD => 130;

public static UntypedInt EPFNOSUPPORT => 96;

public static UntypedInt EPROTONOSUPPORT => 93;

public static UntypedInt EPROTOTYPE => 91;

public static UntypedInt EREMCHG => 78;

public static UntypedInt EREMOTEIO => 121;

public static UntypedInt ERESTART => 85;

public static UntypedInt ERFKILL => 132;

public static UntypedInt ESHUTDOWN => 108;

public static UntypedInt ESOCKTNOSUPPORT => 94;

public static UntypedInt ESTALE => 116;

public static UntypedInt ESTRPIPE => 86;

public static UntypedInt ETIMEDOUT => 110;

public static UntypedInt ETOOMANYREFS => 109;

public static UntypedInt EUCLEAN => 117;

public static UntypedInt EUNATCH => 49;

public static UntypedInt EUSERS => 87;

public static UntypedInt EXFULL => 54;

public static UntypedInt F2FS_FEATURE_ATOMIC_WRITE => 4;

public static UntypedInt F2FS_IOCTL_MAGIC => 245;

public static UntypedInt FALLOC_FL_KEEP_SIZE => 1;

public static UntypedInt FALLOC_FL_PUNCH_HOLE => 2;

public static UntypedInt FAPPEND => 1024;

public static UntypedInt FASYNC => 8192;

public static UntypedInt FFSYNC => 1052672;

public static UntypedInt FILENAME_MAX => 4096;

public static UntypedInt FIOGETOWN => 35075;

public static UntypedInt FIOSETOWN => 35073;

public static UntypedInt FNDELAY => 2048;

public static UntypedInt FNONBLOCK => 2048;

public static UntypedInt FOPEN_MAX => 1000;

public static UntypedInt FP_NAN => 0;

public static UntypedInt FP_ZERO => 2;

public static UntypedInt F_ADD_SEALS => 1033;

public static UntypedInt F_CANCELLK => 1029;

public static UntypedInt F_DUPFD_CLOEXEC => 1030;

public static UntypedInt F_GETLEASE => 1025;

public static UntypedInt F_GETOWN => 9;

public static UntypedInt F_GETOWNER_UIDS => 17;

public static UntypedInt F_GETOWN_EX => 16;

public static UntypedInt F_GETPIPE_SZ => 1032;

public static UntypedInt F_GETSIG => 11;

public static UntypedInt F_GET_FILE_RW_HINT => 1037;

public static UntypedInt F_GET_RW_HINT => 1035;

public static UntypedInt F_GET_SEALS => 1034;

public static UntypedInt F_NOTIFY => 1026;

public static UntypedInt F_OFD_GETLK => 36;

public static UntypedInt F_OFD_SETLK => 37;

public static UntypedInt F_OFD_SETLKW => 38;

public static UntypedInt F_OWNER_GID => 2;

public static UntypedInt F_OWNER_PGRP => 2;

public static UntypedInt F_OWNER_PID => 1;

public static UntypedInt F_OWNER_TID => 0;

public static UntypedInt F_RDLCK => 0;

public static UntypedInt F_SEAL_FUTURE_WRITE => 16;

public static UntypedInt F_SETLEASE => 1024;

public static UntypedInt F_SETOWN => 8;

public static UntypedInt F_SETOWN_EX => 15;

public static UntypedInt F_SETPIPE_SZ => 1031;

public static UntypedInt F_SETSIG => 10;

public static UntypedInt F_SET_FILE_RW_HINT => 1038;

public static UntypedInt F_SET_RW_HINT => 1036;

public static UntypedInt F_WRLCK => 1;

public static UntypedInt HAVE_MREMAP => 1;

public static readonly @string HUGE_VALF = "INFINITY"u8;

public static UntypedInt L_ctermid => 20;

public static UntypedInt L_cuserid => 20;

public static UntypedInt L_tmpnam => 20;

public static UntypedInt MADV_COLD => 20;

public static UntypedInt MADV_DODUMP => 17;

public static UntypedInt MADV_DOFORK => 11;

public static UntypedInt MADV_DONTDUMP => 16;

public static UntypedInt MADV_DONTFORK => 10;

public static UntypedInt MADV_FREE => 8;

public static UntypedInt MADV_HUGEPAGE => 14;

public static UntypedInt MADV_HWPOISON => 100;

public static UntypedInt MADV_KEEPONFORK => 19;

public static UntypedInt MADV_MERGEABLE => 12;

public static UntypedInt MADV_NOHUGEPAGE => 15;

public static UntypedInt MADV_PAGEOUT => 21;

public static UntypedInt MADV_REMOVE => 9;

public static UntypedInt MADV_SOFT_OFFLINE => 101;

public static UntypedInt MADV_UNMERGEABLE => 13;

public static UntypedInt MADV_WIPEONFORK => 18;

public static UntypedInt MAP_ANON => 32;

public static UntypedInt MAP_ANONYMOUS => 32;

public static UntypedInt MAP_DENYWRITE => 2048;

public static UntypedInt MAP_EXECUTABLE => 4096;

public static UntypedInt MAP_FIXED_NOREPLACE => 1048576;

public static UntypedInt MAP_GROWSDOWN => 256;

public static UntypedInt MAP_HUGETLB => 262144;

public static UntypedInt MAP_HUGE_16GB => 2281701376;

public static UntypedInt MAP_HUGE_16KB => 939524096;

public static UntypedInt MAP_HUGE_16MB => 1610612736;

public static UntypedInt MAP_HUGE_1GB => 2013265920;

public static UntypedInt MAP_HUGE_1MB => 1342177280;

public static UntypedInt MAP_HUGE_256MB => 1879048192;

public static UntypedInt MAP_HUGE_2GB => 2080374784;

public static UntypedInt MAP_HUGE_2MB => 1409286144;

public static UntypedInt MAP_HUGE_32MB => 1677721600;

public static UntypedInt MAP_HUGE_512KB => 1275068416;

public static UntypedInt MAP_HUGE_512MB => 1946157056;

public static UntypedInt MAP_HUGE_64KB => 1073741824;

public static UntypedInt MAP_HUGE_8MB => 1543503872;

public static UntypedInt MAP_HUGE_MASK => 63;

public static UntypedInt MAP_HUGE_SHIFT => 26;

public static UntypedInt MAP_NONBLOCK => 65536;

public static UntypedInt MAP_POPULATE => 32768;

public static UntypedInt MAP_SHARED_VALIDATE => 3;

public static UntypedInt MAP_STACK => 131072;

public static UntypedInt MAP_SYNC => 524288;

public static UntypedInt MAX_HANDLE_SZ => 128;

public static UntypedInt MLOCK_ONFAULT => 1;

public static UntypedInt MREMAP_DONTUNMAP => 4;

public static UntypedInt MREMAP_FIXED => 2;

public static UntypedInt MREMAP_MAYMOVE => 1;

public static UntypedInt N_6PACK => 7;

public static UntypedInt N_AX25 => 5;

public static UntypedInt N_CAIF => 20;

public static UntypedInt N_GIGASET_M101 => 16;

public static UntypedInt N_GSM0710 => 21;

public static UntypedInt N_HCI => 15;

public static UntypedInt N_HDLC => 13;

public static UntypedInt N_IRDA => 11;

public static UntypedInt N_MASC => 8;

public static UntypedInt N_MOUSE => 2;

public static UntypedInt N_NCI => 25;

public static UntypedInt N_NULL => 27;

public static UntypedInt N_PPP => 3;

public static UntypedInt N_PPS => 18;

public static UntypedInt N_PROFIBUS_FDL => 10;

public static UntypedInt N_R3964 => 9;

public static UntypedInt N_SLCAN => 17;

public static UntypedInt N_SLIP => 1;

public static UntypedInt N_SMSBLOCK => 12;

public static UntypedInt N_SPEAKUP => 26;

public static UntypedInt N_STRIP => 4;

public static UntypedInt N_SYNC_PPP => 14;

public static UntypedInt N_TI_WL => 22;

public static UntypedInt N_TRACEROUTER => 24;

public static UntypedInt N_TRACESINK => 23;

public static UntypedInt N_TTY => 0;

public static UntypedInt N_V253 => 19;

public static UntypedInt N_X25 => 6;

public static UntypedInt O_ACCMODE => 2097155;

public static UntypedInt O_APPEND => 1024;

public static UntypedInt O_ASYNC => 8192;

public static UntypedInt O_CLOEXEC => 524288;

public static UntypedInt O_CREAT => 64;

public static UntypedInt O_DSYNC => 4096;

public static UntypedInt O_EXCL => 128;

public static UntypedInt O_EXEC => 2097152;

public static UntypedInt O_NDELAY => 2048;

public static UntypedInt O_NOATIME => 262144;

public static UntypedInt O_NOCTTY => 256;

public static UntypedInt O_NONBLOCK => 2048;

public static UntypedInt O_PATH => 2097152;

public static UntypedInt O_RSYNC => 1052672;

public static UntypedInt O_SYNC => 1052672;

public static UntypedInt O_TTY_INIT => 0;

public static UntypedInt POSIX_CLOSE_RESTART => 0;

public static UntypedInt PROT_GROWSDOWN => 16777216;

public static UntypedInt PROT_GROWSUP => 33554432;

public static UntypedInt PTHREAD_BARRIER_SERIAL_THREAD => -1;

public static UntypedInt PTHREAD_CANCELED => 18446744073709551615;

public static UntypedInt PTHREAD_CANCEL_ASYNCHRONOUS => 1;

public static UntypedInt PTHREAD_CANCEL_DEFERRED => 0;

public static UntypedInt PTHREAD_CANCEL_DISABLE => 1;

public static UntypedInt PTHREAD_CANCEL_ENABLE => 0;

public static UntypedInt PTHREAD_CANCEL_MASKED => 2;

public static UntypedInt PTHREAD_CREATE_DETACHED => 1;

public static UntypedInt PTHREAD_CREATE_JOINABLE => 0;

public static UntypedInt PTHREAD_EXPLICIT_SCHED => 1;

public static UntypedInt PTHREAD_INHERIT_SCHED => 0;

public static UntypedInt PTHREAD_MUTEX_DEFAULT => 0;

public static UntypedInt PTHREAD_MUTEX_ERRORCHECK => 2;

public static UntypedInt PTHREAD_MUTEX_NORMAL => 0;

public static UntypedInt PTHREAD_MUTEX_RECURSIVE => 1;

public static UntypedInt PTHREAD_MUTEX_ROBUST => 1;

public static UntypedInt PTHREAD_MUTEX_STALLED => 0;

public static UntypedInt PTHREAD_NULL => 0;

public static UntypedInt PTHREAD_ONCE_INIT => 0;

public static UntypedInt PTHREAD_PRIO_INHERIT => 1;

public static UntypedInt PTHREAD_PRIO_NONE => 0;

public static UntypedInt PTHREAD_PRIO_PROTECT => 2;

public static UntypedInt PTHREAD_PROCESS_PRIVATE => 0;

public static UntypedInt PTHREAD_PROCESS_SHARED => 1;

public static UntypedInt PTHREAD_SCOPE_PROCESS => 1;

public static UntypedInt PTHREAD_SCOPE_SYSTEM => 0;

public static readonly @string P_tmpdir = "/tmp"u8;

public static UntypedInt RTLD_DEFAULT => 0;

public static UntypedInt RWF_WRITE_LIFE_NOT_SET => 0;

public static UntypedInt RWH_WRITE_LIFE_EXTREME => 5;

public static UntypedInt RWH_WRITE_LIFE_LONG => 4;

public static UntypedInt RWH_WRITE_LIFE_MEDIUM => 3;

public static UntypedInt RWH_WRITE_LIFE_NONE => 1;

public static UntypedInt RWH_WRITE_LIFE_SHORT => 2;

public static UntypedInt SCHED_BATCH => 3;

public static UntypedInt SCHED_DEADLINE => 6;

public static UntypedInt SCHED_FIFO => 1;

public static UntypedInt SCHED_IDLE => 5;

public static UntypedInt SCHED_OTHER => 0;

public static UntypedInt SCHED_RESET_ON_FORK => 1073741824;

public static UntypedInt SCHED_RR => 2;

public static UntypedInt SIOCADDDLCI => 35200;

public static UntypedInt SIOCADDMULTI => 35121;

public static UntypedInt SIOCADDRT => 35083;

public static UntypedInt SIOCATMARK => 35077;

public static UntypedInt SIOCDARP => 35155;

public static UntypedInt SIOCDELDLCI => 35201;

public static UntypedInt SIOCDELMULTI => 35122;

public static UntypedInt SIOCDELRT => 35084;

public static UntypedInt SIOCDEVPRIVATE => 35312;

public static UntypedInt SIOCDIFADDR => 35126;

public static UntypedInt SIOCDRARP => 35168;

public static UntypedInt SIOCGARP => 35156;

public static UntypedInt SIOCGIFADDR => 35093;

public static UntypedInt SIOCGIFBR => 35136;

public static UntypedInt SIOCGIFBRDADDR => 35097;

public static UntypedInt SIOCGIFCONF => 35090;

public static UntypedInt SIOCGIFCOUNT => 35128;

public static UntypedInt SIOCGIFDSTADDR => 35095;

public static UntypedInt SIOCGIFENCAP => 35109;

public static UntypedInt SIOCGIFFLAGS => 35091;

public static UntypedInt SIOCGIFHWADDR => 35111;

public static UntypedInt SIOCGIFINDEX => 35123;

public static UntypedInt SIOCGIFMAP => 35184;

public static UntypedInt SIOCGIFMEM => 35103;

public static UntypedInt SIOCGIFMETRIC => 35101;

public static UntypedInt SIOCGIFMTU => 35105;

public static UntypedInt SIOCGIFNAME => 35088;

public static UntypedInt SIOCGIFNETMASK => 35099;

public static UntypedInt SIOCGIFPFLAGS => 35125;

public static UntypedInt SIOCGIFSLAVE => 35113;

public static UntypedInt SIOCGIFTXQLEN => 35138;

public static UntypedInt SIOCGPGRP => 35076;

public static UntypedInt SIOCGRARP => 35169;

public static UntypedInt SIOCPROTOPRIVATE => 35296;

public static UntypedInt SIOCRTMSG => 35085;

public static UntypedInt SIOCSARP => 35157;

public static UntypedInt SIOCSIFADDR => 35094;

public static UntypedInt SIOCSIFBR => 35137;

public static UntypedInt SIOCSIFBRDADDR => 35098;

public static UntypedInt SIOCSIFDSTADDR => 35096;

public static UntypedInt SIOCSIFENCAP => 35110;

public static UntypedInt SIOCSIFFLAGS => 35092;

public static UntypedInt SIOCSIFHWADDR => 35108;

public static UntypedInt SIOCSIFHWBROADCAST => 35127;

public static UntypedInt SIOCSIFLINK => 35089;

public static UntypedInt SIOCSIFMAP => 35185;

public static UntypedInt SIOCSIFMEM => 35104;

public static UntypedInt SIOCSIFMETRIC => 35102;

public static UntypedInt SIOCSIFMTU => 35106;

public static UntypedInt SIOCSIFNAME => 35107;

public static UntypedInt SIOCSIFNETMASK => 35100;

public static UntypedInt SIOCSIFPFLAGS => 35124;

public static UntypedInt SIOCSIFSLAVE => 35120;

public static UntypedInt SIOCSIFTXQLEN => 35139;

public static UntypedInt SIOCSPGRP => 35074;

public static UntypedInt SIOCSRARP => 35170;

public static UntypedInt SIOGIFINDEX => 35123;

public static UntypedInt SPLICE_F_GIFT => 8;

public static UntypedInt SPLICE_F_MORE => 4;

public static UntypedInt SPLICE_F_MOVE => 1;

public static UntypedInt SPLICE_F_NONBLOCK => 2;

public static UntypedInt SQLITE_MAX_PATHLEN => 4096;

public static UntypedInt SQLITE_MUTEX_NREF => 0;

public static UntypedInt STATX_ALL => 4095;

public static UntypedInt STATX_ATIME => 32;

public static UntypedInt STATX_BASIC_STATS => 2047;

public static UntypedInt STATX_BLOCKS => 1024;

public static UntypedInt STATX_BTIME => 2048;

public static UntypedInt STATX_CTIME => 128;

public static UntypedInt STATX_GID => 16;

public static UntypedInt STATX_INO => 256;

public static UntypedInt STATX_MODE => 2;

public static UntypedInt STATX_MTIME => 64;

public static UntypedInt STATX_NLINK => 4;

public static UntypedInt STATX_SIZE => 512;

public static UntypedInt STATX_TYPE => 1;

public static UntypedInt STATX_UID => 8;

public static UntypedInt SYNC_FILE_RANGE_WAIT_AFTER => 4;

public static UntypedInt SYNC_FILE_RANGE_WAIT_BEFORE => 1;

public static UntypedInt SYNC_FILE_RANGE_WRITE => 2;

public static UntypedInt TCSBRKP => 21541;

public static UntypedInt TIOCCBRK => 21544;

public static UntypedInt TIOCCONS => 21533;

public static UntypedInt TIOCEXCL => 21516;

public static UntypedInt TIOCGETD => 21540;

public static UntypedInt TIOCGICOUNT => 21597;

public static UntypedInt TIOCGLCKTRMIOS => 21590;

public static UntypedInt TIOCGRS485 => 21550;

public static UntypedInt TIOCGSERIAL => 21534;

public static UntypedInt TIOCGSID => 21545;

public static UntypedInt TIOCGSOFTCAR => 21529;

public static UntypedInt TIOCLINUX => 21532;

public static UntypedInt TIOCMBIC => 21527;

public static UntypedInt TIOCMBIS => 21526;

public static UntypedInt TIOCMGET => 21525;

public static UntypedInt TIOCMIWAIT => 21596;

public static UntypedInt TIOCMSET => 21528;

public static UntypedInt TIOCM_LOOP => 32768;

public static UntypedInt TIOCM_OUT1 => 8192;

public static UntypedInt TIOCM_OUT2 => 16384;

public static UntypedInt TIOCNOTTY => 21538;

public static UntypedInt TIOCNXCL => 21517;

public static UntypedInt TIOCPKT => 21536;

public static UntypedInt TIOCSBRK => 21543;

public static UntypedInt TIOCSCTTY => 21518;

public static UntypedInt TIOCSERCONFIG => 21587;

public static UntypedInt TIOCSERGETLSR => 21593;

public static UntypedInt TIOCSERGETMULTI => 21594;

public static UntypedInt TIOCSERGSTRUCT => 21592;

public static UntypedInt TIOCSERGWILD => 21588;

public static UntypedInt TIOCSERSETMULTI => 21595;

public static UntypedInt TIOCSERSWILD => 21589;

public static UntypedInt TIOCSER_TEMT => 1;

public static UntypedInt TIOCSETD => 21539;

public static UntypedInt TIOCSLCKTRMIOS => 21591;

public static UntypedInt TIOCSRS485 => 21551;

public static UntypedInt TIOCSSERIAL => 21535;

public static UntypedInt TIOCSSOFTCAR => 21530;

public static UntypedInt TIOCSTI => 21522;

public static UntypedInt TIOCVHANGUP => 21559;

public static UntypedInt TMP_MAX => 10000;

partial struct TSQLiteThreadᴛ1 /*dyn*/ {
    public Tpthread_t Ftid;
    public int32 Fdone;
    public uintptr FpOut;
    public uintptr FxTask;
    public uintptr FpIn;
}

[global::go.golib.GoInlineLayout(Size = 40, Align = 8), StructLayout(LayoutKind.Explicit, Size = 40)] partial struct Tsqlite3_mutexᴛ1 /*dyn*/ {
    [FieldOffset(0)] public Tpthread_mutex_t Fmutex;
}

public static UntypedInt WINT_MAX => 4294967295;

public static int32 /*r*/ Xmodernc_ofd_locking(ж<libc.TLS> tls, int32 onoff) {
    int32 prev = default;
    _ = prev;
    _ofdInitOnce(tls);
    if (!(libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdSupported) != 0)) {
        return -(int32)1;
    }
    prev = libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdEnabled);
    if (onoff < 0 || libc.BoolInt32(onoff != 0) == prev) {
        return prev;
    }
    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdFrozen) != 0) {
        return -(int32)2;
    }
    libc.AtomicStorePInt32((uintptr)Ꮡ_ofdEnabled, libc.BoolInt32(onoff != libc.Int32FromInt32(0)));
    return prev;
}

public static int32 /*r*/ Xsqlite3_blob_reopen(ж<libc.TLS> tls, uintptr pBlob, Tsqlite3_int64 iRow) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        ref var zErr = ref heap(new uintptr(), out var ᏑzErr);
        uintptr v1 = default;
        int32 rc = default;
        _ = db;
        _ = p;
        _ = rc;
        _ = zErr;
        _ = v1;
        p = pBlob;
        if (p == (uintptr)0) {
            r = _sqlite3MisuseError(tls, (int32)106780); goto ᒐdone;
        }
        db = (@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.Fdb;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        if ((@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpStmt == (uintptr)0) {
            rc = (int32)SQLITE_ABORT;
        } else {
            (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)(@unsafe.Pointer.To<TIncrblob>((@unsafe.Pointer)p)).Value.FpStmt)).Value.Frc = SQLITE_OK;
            rc = _blobSeekToRow(tls, p, iRow, @unsafe.Pointer.FromBox(ᏑzErr));
            if (rc != SQLITE_OK) {
                if (zErr != 0) {
                    v1 = __ccgo_ts + 3982;
                } else {
                    v1 = libc.UintptrFromInt32(0);
                }
                _sqlite3ErrorWithMsg(tls, db, rc, v1, libc.VaList(bp + 8, zErr));
                _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zErr);
            }
        }
        rc = _sqlite3ApiExit(tls, db, rc);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static int32 /*r*/ Xsqlite3_os_end(ж<libc.TLS> tls) {
    _unixBigLock = (uintptr)0;
    libc.AtomicStorePInt32((uintptr)Ꮡ_ofdFrozen, 0);
    return SQLITE_OK;
}

public static int32 /*r*/ Xsqlite3_overload_function(ж<libc.TLS> tls, uintptr db, uintptr zName, int32 nArg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        uintptr zCopy = default;
        _ = rc;
        _ = zCopy;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        rc = libc.BoolInt32(_sqlite3FindFunction(tls, db, zName, nArg, (uint8)SQLITE_UTF8, (uint8)0) != (uintptr)0);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        if (rc != 0) {
            r = SQLITE_OK; goto ᒐdone;
        }
        zCopy = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, zName));
        if (zCopy == (uintptr)0) {
            r = (int32)SQLITE_NOMEM; goto ᒐdone;
        }
        r = Xsqlite3_create_function_v2(tls, db, zName, nArg, (int32)SQLITE_UTF8, zCopy, __ccgo_fp(_sqlite3InvalidFunction), (uintptr)0, (uintptr)0, __ccgo_fp(Xsqlite3_free));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static void Xsqlite3_result_error_toobig(ж<libc.TLS> tls, uintptr pCtx) {
    (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FisError = (int32)SQLITE_TOOBIG;
    _sqlite3VdbeMemSetStr(tls, (@unsafe.Pointer.To<Tsqlite3_context>((@unsafe.Pointer)pCtx)).Value.FpOut, __ccgo_ts + 5676, (int64)(-(int32)1), (uint8)SQLITE_UTF8, libc.UintptrFromInt32(0));
}

public static void Xsqlite3_result_str(ж<libc.TLS> tls, uintptr pCtx, uintptr pStr, int32 eOwn) {
    uintptr zText = default;
    _ = zText;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.FaccError) == 0) {
        if ((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.FnChar == (uint32)0) {
            _setResultStrOrError(tls, pCtx, __ccgo_ts + 1732, 0, (uint8)SQLITE_UTF8_ZT, libc.UintptrFromInt32(0));
            _sqlite3_str_reset__ccgo_gp(tls, (@unsafe.Pointer)pStr);
        } else {
            zText = Xsqlite3_str_value(tls, pStr);
            if (eOwn == SQLITE_COPY) {
                _setResultStrOrError(tls, pCtx, zText, libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.FnChar), (uint8)SQLITE_UTF8, (uintptr)(-libc.Int32FromInt32(1)));
            } else {
                _setResultStrOrError(tls, pCtx, zText, libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.FnChar), (uint8)SQLITE_UTF8_ZT, __ccgo_fp(_sqlite3RowSetClear));
                _sqlite3StrAccumInit(ref (tls).DerefOrNull(), (@unsafe.Pointer)pStr, (@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.Fdb, (uintptr)0, 0, libc.Int32FromUint32((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.FmxAlloc));
            }
        }
    } else {
        if (libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_str>((@unsafe.Pointer)pStr)).Value.FaccError) == (int32)SQLITE_NOMEM) {
            Xsqlite3_result_error_nomem(tls, pCtx);
        } else {
            Xsqlite3_result_error_toobig(tls, pCtx);
        }
    }
    if (eOwn == (int32)SQLITE_FINISH) {
        Xsqlite3_str_free(tls, pStr);
    }
}

public static int32 /*r*/ Xsqlite3_set_errmsg(ж<libc.TLS> tls, uintptr db, int32 errcode, uintptr zMsg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        _ = rc;
        rc = SQLITE_OK;
        if (!(_sqlite3SafetyCheckOk(tls, db) != 0)) {
            r = _sqlite3MisuseError(tls, (int32)191549); goto ᒐdone;
        }
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        if (zMsg != 0) {
            _sqlite3ErrorWithMsg(tls, db, errcode, __ccgo_ts + 3982, libc.VaList(bp + 8, zMsg));
        } else {
            _sqlite3Error(tls, db, errcode);
        }
        rc = _sqlite3ApiExit(tls, db, rc);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static uintptr /*r*/ Xsqlite3_sourceid(ж<libc.TLS> tls) {
    return __ccgo_ts + 42837;
}

public static int32 /*r*/ Xsqlite3_unlock_notify(ж<libc.TLS> tls, uintptr db, uintptr __ccgo_fp_xNotify, uintptr _pArg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)bp)).Value = _pArg;
        uintptr p = default;
        uintptr v2 = default;
        int32 rc = default;
        _ = p;
        _ = rc;
        _ = v2;
        rc = SQLITE_OK;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        _enterMutex(tls);
        if (__ccgo_fp_xNotify == (uintptr)0) {
            _removeFromBlockedList(ref (tls).DerefOrNull(), db);
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpBlockingConnection = (uintptr)0;
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpUnlockConnection = (uintptr)0;
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxUnlockNotify = (uintptr)0;
            (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpUnlockArg = (uintptr)0;
        } else {
            if ((uintptr)0 == (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpBlockingConnection) {
                GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr, int32>>(__ccgo_fp_xNotify)(tls, bp, (int32)1);
            } else {
                p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpBlockingConnection;
                for (;;) {
                    if (!(p != 0 && p != db)) {
                        break;
                    }
                    goto _1;
_1:;
                    p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)p)).Value.FpUnlockConnection;
                }
                if (p != 0) {
                    rc = (int32)SQLITE_LOCKED;
                } else {
                    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpUnlockConnection = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpBlockingConnection;
                    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxUnlockNotify = __ccgo_fp_xNotify;
                    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpUnlockArg = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp));
                    _removeFromBlockedList(ref (tls).DerefOrNull(), db);
                    _addToBlockedList(ref (tls).DerefOrNull(), db);
                }
            }
        }
        _leaveMutex(tls);
        if (rc != 0) {
            v2 = __ccgo_ts + 26542;
        } else {
            v2 = (uintptr)0;
        }
        _sqlite3ErrorWithMsg(tls, db, rc, v2, 0);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ Xsqlite3changegroup_schema(ж<libc.TLS> tls, uintptr pGrp, uintptr db, uintptr zDb) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        _ = rc;
        rc = SQLITE_OK;
        if ((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FpList != 0 || (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fdb != 0) {
            rc = (int32)SQLITE_MISUSE;
        } else {
            (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FzDb = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, zDb));
            if ((@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.FzDb == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                (@unsafe.Pointer.To<Tsqlite3_changegroup>((@unsafe.Pointer)pGrp)).Value.Fdb = db;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ Xsqlite3rbu_savestate(ж<libc.TLS> tls, uintptr p) {
    uintptr pDb = default;
    uintptr zBegin = default;
    uintptr v1 = default;
    int32 rc = default;
    _ = pDb;
    _ = rc;
    _ = zBegin;
    _ = v1;
    rc = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc;
    if (rc == (int32)SQLITE_DONE) {
        return SQLITE_OK;
    }
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage == (int32)RBU_STAGE_OAL) {
        if (rc == SQLITE_OK) {
            rc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbMain, __ccgo_ts + 16231, (uintptr)0, (uintptr)0, nil);
        }
    }
    if (rc == SQLITE_OK && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage == (int32)RBU_STAGE_CKPT) {
        pDb = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FpTargetFd)).Value.FpReal;
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pDb)).Value.FpMethods)).Value.FxSync)(tls, pDb, (int32)SQLITE_SYNC_NORMAL);
    }
    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = rc;
    _rbuSaveState(tls, p, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage);
    rc = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc;
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage == (int32)RBU_STAGE_OAL) {
        if (rc == SQLITE_OK) {
            rc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbRbu, __ccgo_ts + 16231, (uintptr)0, (uintptr)0, nil);
        }
        if (rc == SQLITE_OK) {
            if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzTarget == (uintptr)0) {
                v1 = __ccgo_ts + 16216;
            } else {
                v1 = __ccgo_ts + 34857;
            }
            zBegin = v1;
            rc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbRbu, zBegin, (uintptr)0, (uintptr)0, nil);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbMain, __ccgo_ts + 34857, (uintptr)0, (uintptr)0, nil);
        }
    }
    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = rc;
    return rc;
}

internal static UntypedInt _CS_GNU_LIBC_VERSION => 2;

internal static UntypedInt _CS_GNU_LIBPTHREAD_VERSION => 3;

internal static UntypedInt _CS_PATH => 0;

internal static UntypedInt _CS_POSIX_V5_WIDTH_RESTRICTED_ENVS => 4;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFF32_CFLAGS => 1116;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFF32_LDFLAGS => 1117;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFF32_LIBS => 1118;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFF32_LINTFLAGS => 1119;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFFBIG_CFLAGS => 1120;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFFBIG_LDFLAGS => 1121;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFFBIG_LIBS => 1122;

internal static UntypedInt _CS_POSIX_V6_ILP32_OFFBIG_LINTFLAGS => 1123;

internal static UntypedInt _CS_POSIX_V6_LP64_OFF64_CFLAGS => 1124;

internal static UntypedInt _CS_POSIX_V6_LP64_OFF64_LDFLAGS => 1125;

internal static UntypedInt _CS_POSIX_V6_LP64_OFF64_LIBS => 1126;

internal static UntypedInt _CS_POSIX_V6_LP64_OFF64_LINTFLAGS => 1127;

internal static UntypedInt _CS_POSIX_V6_LPBIG_OFFBIG_CFLAGS => 1128;

internal static UntypedInt _CS_POSIX_V6_LPBIG_OFFBIG_LDFLAGS => 1129;

internal static UntypedInt _CS_POSIX_V6_LPBIG_OFFBIG_LIBS => 1130;

internal static UntypedInt _CS_POSIX_V6_LPBIG_OFFBIG_LINTFLAGS => 1131;

internal static UntypedInt _CS_POSIX_V6_WIDTH_RESTRICTED_ENVS => 1;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFF32_CFLAGS => 1132;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFF32_LDFLAGS => 1133;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFF32_LIBS => 1134;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFF32_LINTFLAGS => 1135;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFFBIG_CFLAGS => 1136;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFFBIG_LDFLAGS => 1137;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFFBIG_LIBS => 1138;

internal static UntypedInt _CS_POSIX_V7_ILP32_OFFBIG_LINTFLAGS => 1139;

internal static UntypedInt _CS_POSIX_V7_LP64_OFF64_CFLAGS => 1140;

internal static UntypedInt _CS_POSIX_V7_LP64_OFF64_LDFLAGS => 1141;

internal static UntypedInt _CS_POSIX_V7_LP64_OFF64_LIBS => 1142;

internal static UntypedInt _CS_POSIX_V7_LP64_OFF64_LINTFLAGS => 1143;

internal static UntypedInt _CS_POSIX_V7_LPBIG_OFFBIG_CFLAGS => 1144;

internal static UntypedInt _CS_POSIX_V7_LPBIG_OFFBIG_LDFLAGS => 1145;

internal static UntypedInt _CS_POSIX_V7_LPBIG_OFFBIG_LIBS => 1146;

internal static UntypedInt _CS_POSIX_V7_LPBIG_OFFBIG_LINTFLAGS => 1147;

internal static UntypedInt _CS_POSIX_V7_THREADS_CFLAGS => 1150;

internal static UntypedInt _CS_POSIX_V7_THREADS_LDFLAGS => 1151;

internal static UntypedInt _CS_POSIX_V7_WIDTH_RESTRICTED_ENVS => 5;

internal static UntypedInt _CS_V6_ENV => 1148;

internal static UntypedInt _CS_V7_ENV => 1149;

internal static UntypedInt _GNU_SOURCE => 1;

internal static UntypedInt _IOC_READ => 2;

internal static UntypedInt _PC_2_SYMLINKS => 20;

internal static UntypedInt _PC_ALLOC_SIZE_MIN => 18;

internal static UntypedInt _PC_CHOWN_RESTRICTED => 6;

internal static UntypedInt _PC_LINK_MAX => 0;

internal static UntypedInt _PC_MAX_CANON => 1;

internal static UntypedInt _PC_MAX_INPUT => 2;

internal static UntypedInt _PC_NAME_MAX => 3;

internal static UntypedInt _PC_PATH_MAX => 4;

internal static UntypedInt _PC_PIPE_BUF => 5;

internal static UntypedInt _PC_SOCK_MAXBUF => 12;

internal static UntypedInt _PC_SYNC_IO => 9;

internal static UntypedInt _POSIX2_C_BIND => 200809;

internal static UntypedInt _POSIX_ADVISORY_INFO => 200809;

internal static UntypedInt _POSIX_ASYNCHRONOUS_IO => 200809;

internal static UntypedInt _POSIX_BARRIERS => 200809;

internal static UntypedInt _POSIX_CLOCK_SELECTION => 200809;

internal static UntypedInt _POSIX_FSYNC => 200809;

internal static UntypedInt _POSIX_IPV6 => 200809;

internal static UntypedInt _POSIX_MAPPED_FILES => 200809;

internal static UntypedInt _POSIX_MEMLOCK => 200809;

internal static UntypedInt _POSIX_MEMLOCK_RANGE => 200809;

internal static UntypedInt _POSIX_MEMORY_PROTECTION => 200809;

internal static UntypedInt _POSIX_MESSAGE_PASSING => 200809;

internal static UntypedInt _POSIX_RAW_SOCKETS => 200809;

internal static UntypedInt _POSIX_READER_WRITER_LOCKS => 200809;

internal static UntypedInt _POSIX_REALTIME_SIGNALS => 200809;

internal static UntypedInt _POSIX_SEMAPHORES => 200809;

internal static UntypedInt _POSIX_SPIN_LOCKS => 200809;

internal static UntypedInt _POSIX_THREADS => 200809;

internal static UntypedInt _POSIX_THREAD_ATTR_STACKADDR => 200809;

internal static UntypedInt _POSIX_THREAD_ATTR_STACKSIZE => 200809;

internal static UntypedInt _POSIX_THREAD_PRIORITY_SCHEDULING => 200809;

internal static UntypedInt _POSIX_THREAD_PROCESS_SHARED => 200809;

internal static UntypedInt _POSIX_THREAD_SAFE_FUNCTIONS => 200809;

internal static UntypedInt _POSIX_TIMEOUTS => 200809;

internal static UntypedInt _SC_2_CHAR_TERM => 95;

internal static UntypedInt _SC_2_C_BIND => 47;

internal static UntypedInt _SC_2_C_DEV => 48;

internal static UntypedInt _SC_2_FORT_DEV => 49;

internal static UntypedInt _SC_2_FORT_RUN => 50;

internal static UntypedInt _SC_2_LOCALEDEF => 52;

internal static UntypedInt _SC_2_PBS => 168;

internal static UntypedInt _SC_2_PBS_ACCOUNTING => 169;

internal static UntypedInt _SC_2_PBS_CHECKPOINT => 175;

internal static UntypedInt _SC_2_PBS_LOCATE => 170;

internal static UntypedInt _SC_2_PBS_MESSAGE => 171;

internal static UntypedInt _SC_2_PBS_TRACK => 172;

internal static UntypedInt _SC_2_UPE => 97;

internal static UntypedInt _SC_2_VERSION => 46;

internal static UntypedInt _SC_ADVISORY_INFO => 132;

internal static UntypedInt _SC_AIO_LISTIO_MAX => 23;

internal static UntypedInt _SC_AIO_MAX => 24;

internal static UntypedInt _SC_AIO_PRIO_DELTA_MAX => 25;

internal static UntypedInt _SC_ARG_MAX => 0;

internal static UntypedInt _SC_ASYNCHRONOUS_IO => 12;

internal static UntypedInt _SC_ATEXIT_MAX => 87;

internal static UntypedInt _SC_AVPHYS_PAGES => 86;

internal static UntypedInt _SC_BARRIERS => 133;

internal static UntypedInt _SC_BC_BASE_MAX => 36;

internal static UntypedInt _SC_BC_DIM_MAX => 37;

internal static UntypedInt _SC_BC_SCALE_MAX => 38;

internal static UntypedInt _SC_BC_STRING_MAX => 39;

internal static UntypedInt _SC_CHILD_MAX => 1;

internal static UntypedInt _SC_CLK_TCK => 2;

internal static UntypedInt _SC_CLOCK_SELECTION => 137;

internal static UntypedInt _SC_COLL_WEIGHTS_MAX => 40;

internal static UntypedInt _SC_CPUTIME => 138;

internal static UntypedInt _SC_DELAYTIMER_MAX => 26;

internal static UntypedInt _SC_EXPR_NEST_MAX => 42;

internal static UntypedInt _SC_FSYNC => 15;

internal static UntypedInt _SC_GETGR_R_SIZE_MAX => 69;

internal static UntypedInt _SC_GETPW_R_SIZE_MAX => 70;

internal static UntypedInt _SC_HOST_NAME_MAX => 180;

internal static UntypedInt _SC_IOV_MAX => 60;

internal static UntypedInt _SC_IPV6 => 235;

internal static UntypedInt _SC_JOB_CONTROL => 7;

internal static UntypedInt _SC_LINE_MAX => 43;

internal static UntypedInt _SC_LOGIN_NAME_MAX => 71;

internal static UntypedInt _SC_MAPPED_FILES => 16;

internal static UntypedInt _SC_MEMLOCK => 17;

internal static UntypedInt _SC_MEMLOCK_RANGE => 18;

internal static UntypedInt _SC_MEMORY_PROTECTION => 19;

internal static UntypedInt _SC_MESSAGE_PASSING => 20;

internal static UntypedInt _SC_MINSIGSTKSZ => 249;

internal static UntypedInt _SC_MONOTONIC_CLOCK => 149;

internal static UntypedInt _SC_MQ_OPEN_MAX => 27;

internal static UntypedInt _SC_MQ_PRIO_MAX => 28;

internal static UntypedInt _SC_NGROUPS_MAX => 3;

internal static UntypedInt _SC_NPROCESSORS_CONF => 83;

internal static UntypedInt _SC_NPROCESSORS_ONLN => 84;

internal static UntypedInt _SC_NZERO => 109;

internal static UntypedInt _SC_OPEN_MAX => 4;

internal static UntypedInt _SC_PAGESIZE => 30;

internal static UntypedInt _SC_PAGE_SIZE => 30;

internal static UntypedInt _SC_PASS_MAX => 88;

internal static UntypedInt _SC_PHYS_PAGES => 85;

internal static UntypedInt _SC_PRIORITIZED_IO => 13;

internal static UntypedInt _SC_PRIORITY_SCHEDULING => 10;

internal static UntypedInt _SC_RAW_SOCKETS => 236;

internal static UntypedInt _SC_READER_WRITER_LOCKS => 153;

internal static UntypedInt _SC_REALTIME_SIGNALS => 9;

internal static UntypedInt _SC_REGEXP => 155;

internal static UntypedInt _SC_RE_DUP_MAX => 44;

internal static UntypedInt _SC_RTSIG_MAX => 31;

internal static UntypedInt _SC_SAVED_IDS => 8;

internal static UntypedInt _SC_SEMAPHORES => 21;

internal static UntypedInt _SC_SEM_NSEMS_MAX => 32;

internal static UntypedInt _SC_SEM_VALUE_MAX => 33;

internal static UntypedInt _SC_SHARED_MEMORY_OBJECTS => 22;

internal static UntypedInt _SC_SHELL => 157;

internal static UntypedInt _SC_SIGQUEUE_MAX => 34;

internal static UntypedInt _SC_SIGSTKSZ => 250;

internal static UntypedInt _SC_SPAWN => 159;

internal static UntypedInt _SC_SPIN_LOCKS => 154;

internal static UntypedInt _SC_SPORADIC_SERVER => 160;

internal static UntypedInt _SC_SS_REPL_MAX => 241;

internal static UntypedInt _SC_STREAMS => 174;

internal static UntypedInt _SC_STREAM_MAX => 5;

internal static UntypedInt _SC_SYMLOOP_MAX => 173;

internal static UntypedInt _SC_SYNCHRONIZED_IO => 14;

internal static UntypedInt _SC_THREADS => 67;

internal static UntypedInt _SC_THREAD_CPUTIME => 139;

internal static UntypedInt _SC_THREAD_DESTRUCTOR_ITERATIONS => 73;

internal static UntypedInt _SC_THREAD_KEYS_MAX => 74;

internal static UntypedInt _SC_THREAD_PRIORITY_SCHEDULING => 79;

internal static UntypedInt _SC_THREAD_PRIO_INHERIT => 80;

internal static UntypedInt _SC_THREAD_PRIO_PROTECT => 81;

internal static UntypedInt _SC_THREAD_PROCESS_SHARED => 82;

internal static UntypedInt _SC_THREAD_ROBUST_PRIO_INHERIT => 247;

internal static UntypedInt _SC_THREAD_ROBUST_PRIO_PROTECT => 248;

internal static UntypedInt _SC_THREAD_SAFE_FUNCTIONS => 68;

internal static UntypedInt _SC_THREAD_SPORADIC_SERVER => 161;

internal static UntypedInt _SC_THREAD_STACK_MIN => 75;

internal static UntypedInt _SC_THREAD_THREADS_MAX => 76;

internal static UntypedInt _SC_TIMEOUTS => 164;

internal static UntypedInt _SC_TIMERS => 11;

internal static UntypedInt _SC_TIMER_MAX => 35;

internal static UntypedInt _SC_TRACE => 181;

internal static UntypedInt _SC_TRACE_EVENT_FILTER => 182;

internal static UntypedInt _SC_TRACE_EVENT_NAME_MAX => 242;

internal static UntypedInt _SC_TRACE_INHERIT => 183;

internal static UntypedInt _SC_TRACE_LOG => 184;

internal static UntypedInt _SC_TRACE_NAME_MAX => 243;

internal static UntypedInt _SC_TRACE_SYS_MAX => 244;

internal static UntypedInt _SC_TRACE_USER_EVENT_MAX => 245;

internal static UntypedInt _SC_TTY_NAME_MAX => 72;

internal static UntypedInt _SC_TYPED_MEMORY_OBJECTS => 165;

internal static UntypedInt _SC_TZNAME_MAX => 6;

internal static UntypedInt _SC_UIO_MAXIOV => 60;

internal static UntypedInt _SC_V6_ILP32_OFF32 => 176;

internal static UntypedInt _SC_V6_ILP32_OFFBIG => 177;

internal static UntypedInt _SC_V6_LP64_OFF64 => 178;

internal static UntypedInt _SC_V6_LPBIG_OFFBIG => 179;

internal static UntypedInt _SC_V7_ILP32_OFF32 => 237;

internal static UntypedInt _SC_V7_ILP32_OFFBIG => 238;

internal static UntypedInt _SC_V7_LP64_OFF64 => 239;

internal static UntypedInt _SC_V7_LPBIG_OFFBIG => 240;

internal static UntypedInt _SC_VERSION => 29;

internal static UntypedInt _SC_XBS5_ILP32_OFF32 => 125;

internal static UntypedInt _SC_XBS5_ILP32_OFFBIG => 126;

internal static UntypedInt _SC_XBS5_LP64_OFF64 => 127;

internal static UntypedInt _SC_XBS5_LPBIG_OFFBIG => 128;

internal static UntypedInt _SC_XOPEN_CRYPT => 92;

internal static UntypedInt _SC_XOPEN_ENH_I18N => 93;

internal static UntypedInt _SC_XOPEN_LEGACY => 129;

internal static UntypedInt _SC_XOPEN_REALTIME => 130;

internal static UntypedInt _SC_XOPEN_REALTIME_THREADS => 131;

internal static UntypedInt _SC_XOPEN_SHM => 94;

internal static UntypedInt _SC_XOPEN_STREAMS => 246;

internal static UntypedInt _SC_XOPEN_UNIX => 91;

internal static UntypedInt _SC_XOPEN_VERSION => 89;

internal static UntypedInt _SC_XOPEN_XCU_VERSION => 90;

internal static UntypedInt _SC_XOPEN_XPG2 => 98;

internal static UntypedInt _SC_XOPEN_XPG3 => 99;

internal static UntypedInt _SC_XOPEN_XPG4 => 100;

internal static UntypedInt _STDC_PREDEF_H => 1;

internal static UntypedInt _XOPEN_VERSION => 700;

internal static UntypedInt __BIG_ENDIAN => 4321;

internal static UntypedInt __LITTLE_ENDIAN => 1234;

internal static UntypedInt __PDP_ENDIAN => 3412;

internal static UntypedInt __STDC_IEC_559_COMPLEX__ => 1;

internal static UntypedInt __STDC_IEC_559__ => 1;

internal static UntypedInt __STDC_IEC_60559_BFP__ => 201404;

internal static UntypedInt __STDC_IEC_60559_COMPLEX__ => 201404;

internal static UntypedInt __STDC_ISO_10646__ => 201706;

internal static UntypedInt __USE_TIME_BITS64 => 1;

internal static UntypedInt __WINT_MAX__ => 4294967295;

internal static UntypedInt __gnu_linux__ => 1;

internal static readonly @string __inline = "inline"u8;

internal static UntypedInt __linux => 1;

internal static UntypedInt __linux__ => 1;

internal static readonly @string __tm_gmtoff = "tm_gmtoff"u8;

internal static readonly @string __tm_zone = "tm_zone"u8;


partial struct _aAggᴛ1 /*dyn*/ {
    public uintptr FxStep;
    public uintptr FxFinal;
    public uintptr FzName;
}
internal static readonly ж<array<_aAggᴛ1>> Ꮡ_aAgg = new StaticBox<array<_aAggᴛ1>>(default(array<_aAggᴛ1>));
internal static ref array<_aAggᴛ1> _aAgg => ref ((StaticBox<array<_aAggᴛ1>>)Ꮡ_aAgg).ValueSlot;
internal static void initᴛ_aAgg() { _aAgg = new array<_aAggᴛ1>(1){
    [0] = new(
        FzName: __ccgo_ts + 30383
    )
}; }

internal static readonly ж<array<TFuncDef>> Ꮡ_aAlterTableFuncs = new StaticBox<array<TFuncDef>>(default(array<TFuncDef>));
internal static ref array<TFuncDef> _aAlterTableFuncs => ref ((StaticBox<array<TFuncDef>>)Ꮡ_aAlterTableFuncs).ValueSlot;
internal static void initᴛ_aAlterTableFuncs() { _aAlterTableFuncs = new array<TFuncDef>(9, () => new()){
    [0] = new(
        FnArg: (int16)9,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12766
    ),
    [1] = new(
        FnArg: (int16)7,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12787
    ),
    [2] = new(
        FnArg: (int16)7,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12807
    ),
    [3] = new(
        FnArg: (int16)3,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12826
    ),
    [4] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12845
    ),
    [5] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12868
    ),
    [6] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12891
    ),
    [7] = new(
        FnArg: (int16)3,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12903
    ),
    [8] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FzName: __ccgo_ts + 12925
    )
}; }

internal static readonly ж<array<_sqlite3ParseUri_type>> Ꮡ_aCacheMode = new StaticBox<array<_sqlite3ParseUri_type>>(default(array<_sqlite3ParseUri_type>));
internal static ref array<_sqlite3ParseUri_type> _aCacheMode => ref ((StaticBox<array<_sqlite3ParseUri_type>>)Ꮡ_aCacheMode).ValueSlot;
internal static void initᴛ_aCacheMode() { _aCacheMode = new array<_sqlite3ParseUri_type>(3){
    [0] = new(
        Fz: __ccgo_ts + 26338,
        Fmode: (int32)SQLITE_OPEN_SHAREDCACHE
    ),
    [1] = new(
        Fz: __ccgo_ts + 26345,
        Fmode: (int32)SQLITE_OPEN_PRIVATECACHE
    ),
    [2] = new()
}; }

internal static readonly ж<array<TFuncDef>> Ꮡ_aDateTimeFuncs = new StaticBox<array<TFuncDef>>(default(array<TFuncDef>));
internal static ref array<TFuncDef> _aDateTimeFuncs => ref ((StaticBox<array<TFuncDef>>)Ꮡ_aDateTimeFuncs).ValueSlot;
internal static void initᴛ_aDateTimeFuncs() { _aDateTimeFuncs = new array<TFuncDef>(10, () => new()){
    [0] = new(
        FnArg: (int16)(-(int32)1),
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1334
    ),
    [1] = new(
        FnArg: (int16)(-(int32)1),
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1354
    ),
    [2] = new(
        FnArg: (int16)(-(int32)1),
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1547
    ),
    [3] = new(
        FnArg: (int16)(-(int32)1),
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1552
    ),
    [4] = new(
        FnArg: (int16)(-(int32)1),
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1557
    ),
    [5] = new(
        FnArg: (int16)(-(int32)1),
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1566
    ),
    [6] = new(
        FnArg: (int16)2,
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8) | libc.Int32FromInt32(SQLITE_FUNC_CONSTANT)),
        FpUserData: (uintptr)Ꮡ_sqlite3Config,
        FzName: __ccgo_ts + 1575
    ),
    [7] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8)),
        FzName: __ccgo_ts + 1584
    ),
    [8] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8)),
        FzName: __ccgo_ts + 1597
    ),
    [9] = new(
        FfuncFlags: libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_FUNC_BUILTIN) | libc.Int32FromInt32(SQLITE_FUNC_SLOCHNG) | libc.Int32FromInt32(SQLITE_UTF8)),
        FzName: __ccgo_ts + 1615
    )
}; }


partial struct _aFuncᴛ1 /*dyn*/ {
    public uintptr FxFunc;
    public int8 FnArg;
    public uint8 FbPure;
    public uintptr FzName;
}
internal static readonly ж<array<_aFuncᴛ1>> Ꮡ_aFunc = new StaticBox<array<_aFuncᴛ1>>(default(array<_aFuncᴛ1>));
internal static ref array<_aFuncᴛ1> _aFunc => ref ((StaticBox<array<_aFuncᴛ1>>)Ꮡ_aFunc).ValueSlot;
internal static void initᴛ_aFunc() { _aFunc = new array<_aFuncᴛ1>(12){
    [0] = new(
        FnArg: (int8)1,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30240
    ),
    [1] = new(
        FnArg: (int8)1,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30253
    ),
    [2] = new(
        FnArg: (int8)1,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30266
    ),
    [3] = new(
        FnArg: (int8)(-(int32)1),
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30279
    ),
    [4] = new(
        FnArg: (int8)2,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30225
    ),
    [5] = new(
        FnArg: (int8)3,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30291
    ),
    [6] = new(
        FnArg: (int8)2,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30209
    ),
    [7] = new(
        FnArg: (int8)1,
        FzName: __ccgo_ts + 30314
    ),
    [8] = new(
        FnArg: (int8)1,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30328
    ),
    [9] = new(
        FnArg: (int8)7,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30341
    ),
    [10] = new(
        FnArg: (int8)4,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30355
    ),
    [11] = new(
        FnArg: (int8)1,
        FbPure: (uint8)1,
        FzName: __ccgo_ts + 30371
    )
}; }

internal static array<uintptr> _aMsg;
internal static void initᴛ_aMsg() { _aMsg = new array<uintptr>(29){
    [0] = __ccgo_ts + 25525,
    [1] = __ccgo_ts + 25538,
    [3] = __ccgo_ts + 25554,
    [4] = __ccgo_ts + 25579,
    [5] = __ccgo_ts + 25593,
    [6] = __ccgo_ts + 25612,
    [7] = __ccgo_ts + 1702,
    [8] = __ccgo_ts + 25637,
    [9] = __ccgo_ts + 25674,
    [10] = __ccgo_ts + 25686,
    [11] = __ccgo_ts + 25701,
    [12] = __ccgo_ts + 25734,
    [13] = __ccgo_ts + 25752,
    [14] = __ccgo_ts + 25777,
    [15] = __ccgo_ts + 25806,
    [17] = __ccgo_ts + 6325,
    [18] = __ccgo_ts + 5676,
    [19] = __ccgo_ts + 25823,
    [20] = __ccgo_ts + 25841,
    [21] = __ccgo_ts + 25859,
    [23] = __ccgo_ts + 25893,
    [25] = __ccgo_ts + 25914,
    [26] = __ccgo_ts + 25940,
    [27] = __ccgo_ts + 25963,
    [28] = __ccgo_ts + 25984
}; }


partial struct _aOpᴛ1 /*dyn*/ {
    public uintptr FzOp;
    public uint8 FeOp;
}
internal static array<_aOpᴛ1> _aOp;
internal static void initᴛ_aOp() { _aOp = new array<_aOpᴛ1>(4){
    [0] = new(
        FzOp: __ccgo_ts + 17925,
        FeOp: (uint8)SQLITE_INDEX_CONSTRAINT_MATCH
    ),
    [1] = new(
        FzOp: __ccgo_ts + 17259,
        FeOp: (uint8)SQLITE_INDEX_CONSTRAINT_GLOB
    ),
    [2] = new(
        FzOp: __ccgo_ts + 16718,
        FeOp: (uint8)SQLITE_INDEX_CONSTRAINT_LIKE
    ),
    [3] = new(
        FzOp: __ccgo_ts + 24104,
        FeOp: (uint8)SQLITE_INDEX_CONSTRAINT_REGEXP
    )
}; }

internal static readonly ж<array<_sqlite3ParseUri_type>> Ꮡ_aOpenMode = new StaticBox<array<_sqlite3ParseUri_type>>(default(array<_sqlite3ParseUri_type>));
internal static ref array<_sqlite3ParseUri_type> _aOpenMode => ref ((StaticBox<array<_sqlite3ParseUri_type>>)Ꮡ_aOpenMode).ValueSlot;
internal static void initᴛ_aOpenMode() { _aOpenMode = new array<_sqlite3ParseUri_type>(5){
    [0] = new(
        Fz: __ccgo_ts + 26358,
        Fmode: (int32)SQLITE_OPEN_READONLY
    ),
    [1] = new(
        Fz: __ccgo_ts + 26361,
        Fmode: (int32)SQLITE_OPEN_READWRITE
    ),
    [2] = new(
        Fz: __ccgo_ts + 26364,
        Fmode: (libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE))
    ),
    [3] = new(
        Fz: __ccgo_ts + 19146,
        Fmode: (int32)SQLITE_OPEN_MEMORY
    ),
    [4] = new()
}; }

internal static readonly ж<array<TPragmaName>> Ꮡ_aPragmaName = new StaticBox<array<TPragmaName>>(default(array<TPragmaName>));
internal static ref array<TPragmaName> _aPragmaName => ref ((StaticBox<array<TPragmaName>>)Ꮡ_aPragmaName).ValueSlot;
internal static void initᴛ_aPragmaName() { _aPragmaName = new array<TPragmaName>(66){
    [0] = new(
        FzName: __ccgo_ts + 18145,
        FePragTyp: (uint8)PragTyp_ANALYSIS_LIMIT,
        FmPragFlg: (uint8)PragFlg_Result0
    ),
    [1] = new(
        FzName: __ccgo_ts + 18160,
        FePragTyp: (uint8)PragTyp_HEADER_VALUE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NoColumns1) | libc.Int32FromInt32(PragFlg_Result0)),
        FiArg: (uint64)BTREE_APPLICATION_ID
    ),
    [2] = new(
        FzName: __ccgo_ts + 18175,
        FePragTyp: (uint8)PragTyp_AUTO_VACUUM,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [3] = new(
        FzName: __ccgo_ts + 18187,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_AutoIndex
    ),
    [4] = new(
        FzName: __ccgo_ts + 18203,
        FePragTyp: (uint8)PragTyp_BUSY_TIMEOUT,
        FmPragFlg: (uint8)PragFlg_Result0,
        FiPragCName: (uint8)56,
        FnPragCName: (uint8)1
    ),
    [5] = new(
        FzName: __ccgo_ts + 18126,
        FePragTyp: (uint8)PragTyp_CACHE_SIZE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [6] = new(
        FzName: __ccgo_ts + 18216,
        FePragTyp: (uint8)PragTyp_CACHE_SPILL,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [7] = new(
        FzName: __ccgo_ts + 18228,
        FePragTyp: (uint8)PragTyp_CASE_SENSITIVE_LIKE,
        FmPragFlg: (uint8)PragFlg_NoColumns
    ),
    [8] = new(
        FzName: __ccgo_ts + 18248,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_CellSizeCk
    ),
    [9] = new(
        FzName: __ccgo_ts + 18264,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_CkptFullFSync
    ),
    [10] = new(
        FzName: __ccgo_ts + 18285,
        FePragTyp: (uint8)PragTyp_COLLATION_LIST,
        FmPragFlg: (uint8)PragFlg_Result0,
        FiPragCName: (uint8)33,
        FnPragCName: (uint8)2
    ),
    [11] = new(
        FzName: __ccgo_ts + 18300,
        FePragTyp: (uint8)PragTyp_COMPILE_OPTIONS,
        FmPragFlg: (uint8)PragFlg_Result0
    ),
    [12] = new(
        FzName: __ccgo_ts + 18316,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: libc.Uint64FromInt32(libc.Int32FromInt32(0x00001)).Lsh((int64)(libc.Int32FromInt32(32)))
    ),
    [13] = new(
        FzName: __ccgo_ts + 18330,
        FePragTyp: (uint8)PragTyp_HEADER_VALUE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_ReadOnly) | libc.Int32FromInt32(PragFlg_Result0)),
        FiArg: (uint64)BTREE_DATA_VERSION
    ),
    [14] = new(
        FzName: __ccgo_ts + 18343,
        FePragTyp: (uint8)PragTyp_DATABASE_LIST,
        FmPragFlg: (uint8)PragFlg_Result0,
        FiPragCName: (uint8)50,
        FnPragCName: (uint8)3
    ),
    [15] = new(
        FzName: __ccgo_ts + 18357,
        FePragTyp: (uint8)PragTyp_DEFAULT_CACHE_SIZE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiPragCName: (uint8)55,
        FnPragCName: (uint8)1
    ),
    [16] = new(
        FzName: __ccgo_ts + 18376,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_DeferFKs
    ),
    [17] = new(
        FzName: __ccgo_ts + 18395,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_NullCallback
    ),
    [18] = new(
        FzName: __ccgo_ts + 18418,
        FePragTyp: (uint8)PragTyp_ENCODING,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [19] = new(
        FzName: __ccgo_ts + 18427,
        FePragTyp: (uint8)PragTyp_FOREIGN_KEY_CHECK,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FiPragCName: (uint8)43,
        FnPragCName: (uint8)4
    ),
    [20] = new(
        FzName: __ccgo_ts + 18445,
        FePragTyp: (uint8)PragTyp_FOREIGN_KEY_LIST,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FnPragCName: (uint8)8
    ),
    [21] = new(
        FzName: __ccgo_ts + 18462,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_ForeignKeys
    ),
    [22] = new(
        FzName: __ccgo_ts + 18475,
        FePragTyp: (uint8)PragTyp_HEADER_VALUE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_ReadOnly) | libc.Int32FromInt32(PragFlg_Result0))
    ),
    [23] = new(
        FzName: __ccgo_ts + 18490,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_FullColNames
    ),
    [24] = new(
        FzName: __ccgo_ts + 18508,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_FullFSync
    ),
    [25] = new(
        FzName: __ccgo_ts + 18518,
        FePragTyp: (uint8)PragTyp_FUNCTION_LIST,
        FmPragFlg: (uint8)PragFlg_Result0,
        FiPragCName: (uint8)15,
        FnPragCName: (uint8)6
    ),
    [26] = new(
        FzName: __ccgo_ts + 18532,
        FePragTyp: (uint8)PragTyp_HARD_HEAP_LIMIT,
        FmPragFlg: (uint8)PragFlg_Result0
    ),
    [27] = new(
        FzName: __ccgo_ts + 18548,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_IgnoreChecks
    ),
    [28] = new(
        FzName: __ccgo_ts + 18573,
        FePragTyp: (uint8)PragTyp_INCREMENTAL_VACUUM,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_NoColumns))
    ),
    [29] = new(
        FzName: __ccgo_ts + 18592,
        FePragTyp: (uint8)PragTyp_INDEX_INFO,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FiPragCName: (uint8)27,
        FnPragCName: (uint8)3
    ),
    [30] = new(
        FzName: __ccgo_ts + 18603,
        FePragTyp: (uint8)PragTyp_INDEX_LIST,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FiPragCName: (uint8)33,
        FnPragCName: (uint8)5
    ),
    [31] = new(
        FzName: __ccgo_ts + 18614,
        FePragTyp: (uint8)PragTyp_INDEX_INFO,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FiPragCName: (uint8)27,
        FnPragCName: (uint8)6,
        FiArg: (uint64)1
    ),
    [32] = new(
        FzName: __ccgo_ts + 18626,
        FePragTyp: (uint8)PragTyp_INTEGRITY_CHECK,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt))
    ),
    [33] = new(
        FzName: __ccgo_ts + 18642,
        FePragTyp: (uint8)PragTyp_JOURNAL_MODE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq))
    ),
    [34] = new(
        FzName: __ccgo_ts + 18655,
        FePragTyp: (uint8)PragTyp_JOURNAL_SIZE_LIMIT,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq))
    ),
    [35] = new(
        FzName: __ccgo_ts + 18674,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_LegacyAlter
    ),
    [36] = new(
        FzName: __ccgo_ts + 18693,
        FePragTyp: (uint8)PragTyp_LOCKING_MODE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq))
    ),
    [37] = new(
        FzName: __ccgo_ts + 18706,
        FePragTyp: (uint8)PragTyp_PAGE_COUNT,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq))
    ),
    [38] = new(
        FzName: __ccgo_ts + 18721,
        FePragTyp: (uint8)PragTyp_MMAP_SIZE
    ),
    [39] = new(
        FzName: __ccgo_ts + 18731,
        FePragTyp: (uint8)PragTyp_MODULE_LIST,
        FmPragFlg: (uint8)PragFlg_Result0,
        FiPragCName: (uint8)9,
        FnPragCName: (uint8)1
    ),
    [40] = new(
        FzName: __ccgo_ts + 18743,
        FePragTyp: (uint8)PragTyp_OPTIMIZE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_NeedSchema))
    ),
    [41] = new(
        FzName: __ccgo_ts + 18752,
        FePragTyp: (uint8)PragTyp_PAGE_COUNT,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq))
    ),
    [42] = new(
        FzName: __ccgo_ts + 18763,
        FePragTyp: (uint8)PragTyp_PAGE_SIZE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [43] = new(
        FzName: __ccgo_ts + 18773,
        FePragTyp: (uint8)PragTyp_PRAGMA_LIST,
        FmPragFlg: (uint8)PragFlg_Result0,
        FiPragCName: (uint8)9,
        FnPragCName: (uint8)1
    ),
    [44] = new(
        FzName: __ccgo_ts + 18785,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_QueryOnly
    ),
    [45] = new(
        FzName: __ccgo_ts + 18796,
        FePragTyp: (uint8)PragTyp_INTEGRITY_CHECK,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt))
    ),
    [46] = new(
        FzName: __ccgo_ts + 18808,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: libc.Uint64FromInt32(libc.Int32FromInt32(0x00004)).Lsh((int64)(libc.Int32FromInt32(32)))
    ),
    [47] = new(
        FzName: __ccgo_ts + 18825,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_RecTriggers
    ),
    [48] = new(
        FzName: __ccgo_ts + 18844,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_ReverseOrder
    ),
    [49] = new(
        FzName: __ccgo_ts + 18870,
        FePragTyp: (uint8)PragTyp_HEADER_VALUE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NoColumns1) | libc.Int32FromInt32(PragFlg_Result0)),
        FiArg: (uint64)BTREE_SCHEMA_VERSION
    ),
    [50] = new(
        FzName: __ccgo_ts + 18885,
        FePragTyp: (uint8)PragTyp_SECURE_DELETE,
        FmPragFlg: (uint8)PragFlg_Result0
    ),
    [51] = new(
        FzName: __ccgo_ts + 18899,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_ShortColNames
    ),
    [52] = new(
        FzName: __ccgo_ts + 18918,
        FePragTyp: (uint8)PragTyp_SHRINK_MEMORY,
        FmPragFlg: (uint8)PragFlg_NoColumns
    ),
    [53] = new(
        FzName: __ccgo_ts + 18932,
        FePragTyp: (uint8)PragTyp_SOFT_HEAP_LIMIT,
        FmPragFlg: (uint8)PragFlg_Result0
    ),
    [54] = new(
        FzName: __ccgo_ts + 18948,
        FePragTyp: (uint8)PragTyp_SYNCHRONOUS,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_SchemaReq) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [55] = new(
        FzName: __ccgo_ts + 18960,
        FePragTyp: (uint8)PragTyp_TABLE_INFO,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FiPragCName: (uint8)8,
        FnPragCName: (uint8)6
    ),
    [56] = new(
        FzName: __ccgo_ts + 18971,
        FePragTyp: (uint8)PragTyp_TABLE_LIST,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1)),
        FiPragCName: (uint8)21,
        FnPragCName: (uint8)6
    ),
    [57] = new(
        FzName: __ccgo_ts + 18982,
        FePragTyp: (uint8)PragTyp_TABLE_INFO,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NeedSchema) | libc.Int32FromInt32(PragFlg_Result1) | libc.Int32FromInt32(PragFlg_SchemaOpt)),
        FiPragCName: (uint8)8,
        FnPragCName: (uint8)7,
        FiArg: (uint64)1
    ),
    [58] = new(
        FzName: __ccgo_ts + 18994,
        FePragTyp: (uint8)PragTyp_TEMP_STORE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1))
    ),
    [59] = new(
        FzName: __ccgo_ts + 19005,
        FePragTyp: (uint8)PragTyp_TEMP_STORE_DIRECTORY,
        FmPragFlg: (uint8)PragFlg_NoColumns1
    ),
    [60] = new(
        FzName: __ccgo_ts + 19026,
        FePragTyp: (uint8)PragTyp_THREADS,
        FmPragFlg: (uint8)PragFlg_Result0
    ),
    [61] = new(
        FzName: __ccgo_ts + 19034,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: (uint64)SQLITE_TrustedSchema
    ),
    [62] = new(
        FzName: __ccgo_ts + 19049,
        FePragTyp: (uint8)PragTyp_HEADER_VALUE,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_NoColumns1) | libc.Int32FromInt32(PragFlg_Result0)),
        FiArg: (uint64)BTREE_USER_VERSION
    ),
    [63] = new(
        FzName: __ccgo_ts + 19062,
        FePragTyp: (uint8)PragTyp_WAL_AUTOCHECKPOINT
    ),
    [64] = new(
        FzName: __ccgo_ts + 19081,
        FePragTyp: (uint8)PragTyp_WAL_CHECKPOINT,
        FmPragFlg: (uint8)PragFlg_NeedSchema,
        FiPragCName: (uint8)47,
        FnPragCName: (uint8)3
    ),
    [65] = new(
        FzName: __ccgo_ts + 19096,
        FePragTyp: (uint8)PragTyp_FLAG,
        FmPragFlg: libc.Uint8FromInt32(libc.Int32FromInt32(PragFlg_Result0) | libc.Int32FromInt32(PragFlg_NoColumns1)),
        FiArg: libc.Uint64FromInt32(libc.Int32FromInt32(SQLITE_WriteSchema) | libc.Int32FromInt32(SQLITE_NoSchemaError))
    )
}; }

internal static readonly ж<array<Tunix_syscall>> Ꮡ_aSyscall = new StaticBox<array<Tunix_syscall>>(default(array<Tunix_syscall>));
internal static ref array<Tunix_syscall> _aSyscall => ref ((StaticBox<array<Tunix_syscall>>)Ꮡ_aSyscall).ValueSlot;
internal static void initᴛ_aSyscall() { _aSyscall = new array<Tunix_syscall>(29){
    [0] = new(
        FzName: __ccgo_ts + 3556
    ),
    [1] = new(
        FzName: __ccgo_ts + 3561
    ),
    [2] = new(
        FzName: __ccgo_ts + 3567
    ),
    [3] = new(
        FzName: __ccgo_ts + 3574
    ),
    [4] = new(
        FzName: __ccgo_ts + 3581
    ),
    [5] = new(
        FzName: __ccgo_ts + 3586
    ),
    [6] = new(
        FzName: __ccgo_ts + 3592
    ),
    [7] = new(
        FzName: __ccgo_ts + 3602
    ),
    [8] = new(
        FzName: __ccgo_ts + 3608
    ),
    [9] = new(
        FzName: __ccgo_ts + 3613
    ),
    [10] = new(
        FzName: __ccgo_ts + 3619
    ),
    [11] = new(
        FzName: __ccgo_ts + 3627
    ),
    [12] = new(
        FzName: __ccgo_ts + 3633
    ),
    [13] = new(
        FzName: __ccgo_ts + 3640
    ),
    [14] = new(
        FzName: __ccgo_ts + 3649
    ),
    [15] = new(
        FzName: __ccgo_ts + 3656
    ),
    [16] = new(
        FzName: __ccgo_ts + 3666
    ),
    [17] = new(
        FzName: __ccgo_ts + 3673
    ),
    [18] = new(
        FzName: __ccgo_ts + 3687
    ),
    [19] = new(
        FzName: __ccgo_ts + 3693
    ),
    [20] = new(
        FzName: __ccgo_ts + 3699
    ),
    [21] = new(
        FzName: __ccgo_ts + 3706
    ),
    [22] = new(
        FzName: __ccgo_ts + 3714
    ),
    [23] = new(
        FzName: __ccgo_ts + 3719
    ),
    [24] = new(
        FzName: __ccgo_ts + 3726
    ),
    [25] = new(
        FzName: __ccgo_ts + 3733
    ),
    [26] = new(
        FzName: __ccgo_ts + 3745
    ),
    [27] = new(
        FzName: __ccgo_ts + 3754
    ),
    [28] = new(
        FzName: __ccgo_ts + 3760
    )
}; }


partial struct _aTableᴛ1 /*dyn*/ {
    public uintptr FzName;
    public uintptr FzCols;
}
internal static array<_aTableᴛ1> _aTable;
internal static void initᴛ_aTable() { _aTable = new array<_aTableᴛ1>(3){
    [0] = new(
        FzName: __ccgo_ts + 12948,
        FzCols: __ccgo_ts + 12961
    ),
    [1] = new(
        FzName: __ccgo_ts + 12974,
        FzCols: __ccgo_ts + 12987
    ),
    [2] = new(
        FzName: __ccgo_ts + 13015
    )
}; }

internal static void _absFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    Ti64 iVal = default;
    float64 rVal = default;
    _ = iVal;
    _ = rVal;
    _ = argc;
    switch (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)))) {
    case var ᴛ when ᴛ == (int32)SQLITE_INTEGER: {
        iVal = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (iVal < 0) {
            if (iVal == (int64)(-libc.Int32FromInt32(1)) - (libc.Int64FromUint32(0xffffffffU) | libc.Int64FromInt32(0x7fffffff).Lsh((int64)(libc.Int32FromInt32(32))))) {
                Xsqlite3_result_error(tls, context, __ccgo_ts + 16571, -(int32)1);
                return;
            }
            iVal = -iVal;
        }
        Xsqlite3_result_int64(tls, context, iVal);
        break;
    }
    case var ᴛ when ᴛ == (int32)SQLITE_NULL: {
        Xsqlite3_result_null(tls, context);
        break;
    }
    default: {
        rVal = Xsqlite3_value_double(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)));
        if (rVal < libc.Float64FromInt32(0)) {
            rVal = -rVal;
        }
        Xsqlite3_result_double(tls, context, rVal);
        break;
        break;
    }}

}

internal static uintptr /*r*/ _actionName(ref libc.TLS tls, Tu8 action) {
    uintptr zName = default;
    _ = zName;
    switch (libc.Int32FromUint8(action)) {
    case var ᴛ when ᴛ == (int32)OE_SetNull: {
        zName = __ccgo_ts + 19215;
        break;
    }
    case var ᴛ when ᴛ == (int32)OE_SetDflt: {
        zName = __ccgo_ts + 19224;
        break;
    }
    case var ᴛ when ᴛ == (int32)OE_Cascade: {
        zName = __ccgo_ts + 19236;
        break;
    }
    case var ᴛ when ᴛ == (int32)OE_Restrict: {
        zName = __ccgo_ts + 19244;
        break;
    }
    default: {
        zName = __ccgo_ts + 19253;
        break;
        break;
    }}

    return zName;
}

internal static int32 /*r*/ _alterRtrimConstraint(ж<libc.TLS> tls, uintptr db, uintptr pCons, int32 nCons) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 iEnd = default;
        int32 iOff = default;
        int32 nToken = default;
        ref var t = ref heap(new int32(), out var Ꮡt);
        uintptr zTmp = default;
        _ = iEnd;
        _ = iOff;
        _ = nToken;
        _ = t;
        _ = zTmp;
        zTmp = _sqlite3MPrintf(tls, db, __ccgo_ts + 12164, libc.VaList(bp + 8, nCons, pCons));
        iOff = 0;
        iEnd = 0;
        if (zTmp == (uintptr)0) {
            r = 0; goto ᒐdone;
        }
        while ((int32)1 != 0) {
            t = 0;
            nToken = (int32)_sqlite3GetToken(ref (tls).DerefOrNull(), zTmp + (uintptr)iOff, @unsafe.Pointer.FromPinnedBox(Ꮡt));
            if (t == (int32)TK_ILLEGAL) {
                break;
            }
            if (t != (int32)TK_SPACE && (t != (int32)TK_COMMENT || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(zTmp + (uintptr)iOff))) != (int32)'-')) {
                iEnd = iOff + nToken;
            }
            iOff = iOff + nToken;
        }
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zTmp);
        r = iEnd;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static readonly ж<TFuncDef> Ꮡ_attach_func = new StaticBox<TFuncDef>(default(TFuncDef));
internal static ref TFuncDef _attach_func => ref ((StaticBox<TFuncDef>)Ꮡ_attach_func).ValueSlot;
internal static void initᴛ_attach_func() { _attach_func = new TFuncDef(
    FnArg: (int16)3,
    FfuncFlags: (uint32)SQLITE_UTF8,
    FzName: __ccgo_ts + 13640
); }

internal static int32 /*r*/ _authFunctions(ж<libc.TLS> tls, uintptr pWalker, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr pParse = default;
        int32 rc = default;
        _ = pParse;
        _ = rc;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_FUNCTION) {
            pParse = (@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse;
            rc = _sqlite3AuthCheck(tls, pParse, (int32)SQLITE_FUNCTION, (uintptr)0, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + 8)), (uintptr)0);
            if (rc != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7355, libc.VaList(bp + 8, p));
                r = (int32)WRC_Abort; goto ᒐdone;
            }
        }
        r = WRC_Continue;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static array<uintptr> _az;
internal static void initᴛ_az() { _az = new array<uintptr>(3){
    [0] = __ccgo_ts + 16216,
    [1] = __ccgo_ts + 16238,
    [2] = __ccgo_ts + 16222
}; }

internal static array<uintptr> _azAlterType;
internal static void initᴛ_azAlterType() { _azAlterType = new array<uintptr>(4){
    [0] = __ccgo_ts + 20090,
    [1] = __ccgo_ts + 20097,
    [2] = __ccgo_ts + 20109,
    [3] = __ccgo_ts + 20120
}; }

internal static array<uintptr> _azEnc;
internal static void initᴛ_azEnc() { _azEnc = new array<uintptr>(4){
    [1] = __ccgo_ts + 19295,
    [2] = __ccgo_ts + 19300,
    [3] = __ccgo_ts + 19308
}; }

internal static array<uintptr> _azEndings;
internal static void initᴛ_azEndings() { _azEndings = new array<uintptr>(1){
    [0] = __ccgo_ts + 17697
}; }

internal static array<uintptr> _azErr;
internal static void initᴛ_azErr() { _azErr = new array<uintptr>(5){
    [0] = __ccgo_ts + 24790,
    [1] = __ccgo_ts + 24843,
    [2] = __ccgo_ts + 24345,
    [3] = __ccgo_ts + 24894,
    [4] = __ccgo_ts + 24946
}; }

internal static array<uintptr> _azExplainColNames8;
internal static void initᴛ_azExplainColNames8() { _azExplainColNames8 = new array<uintptr>(12){
    [0] = __ccgo_ts + 5699,
    [1] = __ccgo_ts + 5704,
    [2] = __ccgo_ts + 5711,
    [3] = __ccgo_ts + 5714,
    [4] = __ccgo_ts + 5717,
    [5] = __ccgo_ts + 5720,
    [6] = __ccgo_ts + 5723,
    [7] = __ccgo_ts + 5726,
    [8] = __ccgo_ts + 5734,
    [9] = __ccgo_ts + 5737,
    [10] = __ccgo_ts + 5744,
    [11] = __ccgo_ts + 5752
}; }

internal static array<uintptr> _azFormat;
internal static void initᴛ_azFormat() { _azFormat = new array<uintptr>(2){
    [0] = __ccgo_ts + 29169,
    [1] = __ccgo_ts + 29180
}; }

internal static array<uintptr> _azInsType;
internal static void initᴛ_azInsType() { _azInsType = new array<uintptr>(3){
    [0] = __ccgo_ts + 26949,
    [1] = __ccgo_ts + 26956,
    [2] = __ccgo_ts + 26960
}; }

internal static array<uintptr> _azModeName;
internal static void initᴛ_azModeName() { _azModeName = new array<uintptr>(6){
    [0] = __ccgo_ts + 19263,
    [1] = __ccgo_ts + 19270,
    [2] = __ccgo_ts + 19278,
    [3] = __ccgo_ts + 19282,
    [4] = __ccgo_ts + 19146,
    [5] = __ccgo_ts + 19291
}; }

internal static array<uintptr> _azModule;
internal static void initᴛ_azModule() { _azModule = new array<uintptr>(4){
    [0] = __ccgo_ts + 27547,
    [1] = __ccgo_ts + 27557,
    [2] = __ccgo_ts + 27567,
    [3] = __ccgo_ts + 27578
}; }

internal static array<uintptr> _azName;
internal static void initᴛ_azName() { _azName = new array<uintptr>(191){
    [0] = __ccgo_ts + 1938,
    [1] = __ccgo_ts + 1948,
    [2] = __ccgo_ts + 1959,
    [3] = __ccgo_ts + 1971,
    [4] = __ccgo_ts + 1982,
    [5] = __ccgo_ts + 1994,
    [6] = __ccgo_ts + 2001,
    [7] = __ccgo_ts + 2009,
    [8] = __ccgo_ts + 2017,
    [9] = __ccgo_ts + 2022,
    [10] = __ccgo_ts + 2027,
    [11] = __ccgo_ts + 2033,
    [12] = __ccgo_ts + 2047,
    [13] = __ccgo_ts + 2053,
    [14] = __ccgo_ts + 2063,
    [15] = __ccgo_ts + 2068,
    [16] = __ccgo_ts + 2073,
    [17] = __ccgo_ts + 2076,
    [18] = __ccgo_ts + 2082,
    [19] = __ccgo_ts + 2089,
    [20] = __ccgo_ts + 2093,
    [21] = __ccgo_ts + 2103,
    [22] = __ccgo_ts + 2110,
    [23] = __ccgo_ts + 2117,
    [24] = __ccgo_ts + 2124,
    [25] = __ccgo_ts + 2131,
    [26] = __ccgo_ts + 2140,
    [27] = __ccgo_ts + 2151,
    [28] = __ccgo_ts + 2160,
    [29] = __ccgo_ts + 2166,
    [30] = __ccgo_ts + 2176,
    [31] = __ccgo_ts + 2186,
    [32] = __ccgo_ts + 2191,
    [33] = __ccgo_ts + 2205,
    [34] = __ccgo_ts + 2216,
    [35] = __ccgo_ts + 2221,
    [36] = __ccgo_ts + 2228,
    [37] = __ccgo_ts + 2236,
    [38] = __ccgo_ts + 2247,
    [39] = __ccgo_ts + 2252,
    [40] = __ccgo_ts + 2257,
    [41] = __ccgo_ts + 2263,
    [42] = __ccgo_ts + 2269,
    [43] = __ccgo_ts + 2275,
    [44] = __ccgo_ts + 2278,
    [45] = __ccgo_ts + 2282,
    [46] = __ccgo_ts + 2288,
    [47] = __ccgo_ts + 2297,
    [48] = __ccgo_ts + 2308,
    [49] = __ccgo_ts + 2319,
    [50] = __ccgo_ts + 2327,
    [51] = __ccgo_ts + 2336,
    [52] = __ccgo_ts + 2343,
    [53] = __ccgo_ts + 2351,
    [54] = __ccgo_ts + 2354,
    [55] = __ccgo_ts + 2357,
    [56] = __ccgo_ts + 2360,
    [57] = __ccgo_ts + 2363,
    [58] = __ccgo_ts + 2366,
    [59] = __ccgo_ts + 2369,
    [60] = __ccgo_ts + 2376,
    [61] = __ccgo_ts + 2382,
    [62] = __ccgo_ts + 2392,
    [63] = __ccgo_ts + 2405,
    [64] = __ccgo_ts + 2416,
    [65] = __ccgo_ts + 2422,
    [66] = __ccgo_ts + 2429,
    [67] = __ccgo_ts + 2438,
    [68] = __ccgo_ts + 2447,
    [69] = __ccgo_ts + 2454,
    [70] = __ccgo_ts + 2467,
    [71] = __ccgo_ts + 2478,
    [72] = __ccgo_ts + 2483,
    [73] = __ccgo_ts + 2491,
    [74] = __ccgo_ts + 2497,
    [75] = __ccgo_ts + 2504,
    [76] = __ccgo_ts + 2516,
    [77] = __ccgo_ts + 2521,
    [78] = __ccgo_ts + 2530,
    [79] = __ccgo_ts + 2535,
    [80] = __ccgo_ts + 2544,
    [81] = __ccgo_ts + 2549,
    [82] = __ccgo_ts + 2554,
    [83] = __ccgo_ts + 2560,
    [84] = __ccgo_ts + 2568,
    [85] = __ccgo_ts + 2576,
    [86] = __ccgo_ts + 2586,
    [87] = __ccgo_ts + 2594,
    [88] = __ccgo_ts + 2601,
    [89] = __ccgo_ts + 2614,
    [90] = __ccgo_ts + 2619,
    [91] = __ccgo_ts + 2631,
    [92] = __ccgo_ts + 2639,
    [93] = __ccgo_ts + 2646,
    [94] = __ccgo_ts + 2657,
    [95] = __ccgo_ts + 2664,
    [96] = __ccgo_ts + 2671,
    [97] = __ccgo_ts + 2681,
    [98] = __ccgo_ts + 2690,
    [99] = __ccgo_ts + 2701,
    [100] = __ccgo_ts + 2707,
    [101] = __ccgo_ts + 2718,
    [102] = __ccgo_ts + 2728,
    [103] = __ccgo_ts + 2738,
    [104] = __ccgo_ts + 2745,
    [105] = __ccgo_ts + 2751,
    [106] = __ccgo_ts + 2761,
    [107] = __ccgo_ts + 2772,
    [108] = __ccgo_ts + 2776,
    [109] = __ccgo_ts + 2785,
    [110] = __ccgo_ts + 2794,
    [111] = __ccgo_ts + 2801,
    [112] = __ccgo_ts + 2811,
    [113] = __ccgo_ts + 2818,
    [114] = __ccgo_ts + 2827,
    [115] = __ccgo_ts + 2837,
    [116] = __ccgo_ts + 2844,
    [117] = __ccgo_ts + 2852,
    [118] = __ccgo_ts + 2866,
    [119] = __ccgo_ts + 2874,
    [120] = __ccgo_ts + 2888,
    [121] = __ccgo_ts + 2899,
    [122] = __ccgo_ts + 2912,
    [123] = __ccgo_ts + 2923,
    [124] = __ccgo_ts + 2929,
    [125] = __ccgo_ts + 2941,
    [126] = __ccgo_ts + 2950,
    [127] = __ccgo_ts + 2958,
    [128] = __ccgo_ts + 2967,
    [129] = __ccgo_ts + 2976,
    [130] = __ccgo_ts + 2983,
    [131] = __ccgo_ts + 2991,
    [132] = __ccgo_ts + 2998,
    [133] = __ccgo_ts + 3009,
    [134] = __ccgo_ts + 3023,
    [135] = __ccgo_ts + 3034,
    [136] = __ccgo_ts + 3042,
    [137] = __ccgo_ts + 3048,
    [138] = __ccgo_ts + 3056,
    [139] = __ccgo_ts + 3064,
    [140] = __ccgo_ts + 3074,
    [141] = __ccgo_ts + 3087,
    [142] = __ccgo_ts + 3097,
    [143] = __ccgo_ts + 3110,
    [144] = __ccgo_ts + 3119,
    [145] = __ccgo_ts + 3130,
    [146] = __ccgo_ts + 3138,
    [147] = __ccgo_ts + 3144,
    [148] = __ccgo_ts + 3156,
    [149] = __ccgo_ts + 3168,
    [150] = __ccgo_ts + 3176,
    [151] = __ccgo_ts + 3188,
    [152] = __ccgo_ts + 3201,
    [153] = __ccgo_ts + 3211,
    [154] = __ccgo_ts + 3221,
    [155] = __ccgo_ts + 3226,
    [156] = __ccgo_ts + 3238,
    [157] = __ccgo_ts + 3250,
    [158] = __ccgo_ts + 3260,
    [159] = __ccgo_ts + 3266,
    [160] = __ccgo_ts + 3276,
    [161] = __ccgo_ts + 3283,
    [162] = __ccgo_ts + 3295,
    [163] = __ccgo_ts + 3306,
    [164] = __ccgo_ts + 3314,
    [165] = __ccgo_ts + 3323,
    [166] = __ccgo_ts + 3332,
    [167] = __ccgo_ts + 3341,
    [168] = __ccgo_ts + 3348,
    [169] = __ccgo_ts + 3359,
    [170] = __ccgo_ts + 3372,
    [171] = __ccgo_ts + 3382,
    [172] = __ccgo_ts + 3389,
    [173] = __ccgo_ts + 3397,
    [174] = __ccgo_ts + 3406,
    [175] = __ccgo_ts + 3412,
    [176] = __ccgo_ts + 3419,
    [177] = __ccgo_ts + 3427,
    [178] = __ccgo_ts + 3435,
    [179] = __ccgo_ts + 3443,
    [180] = __ccgo_ts + 3453,
    [181] = __ccgo_ts + 3462,
    [182] = __ccgo_ts + 3473,
    [183] = __ccgo_ts + 3484,
    [184] = __ccgo_ts + 3495,
    [185] = __ccgo_ts + 3505,
    [186] = __ccgo_ts + 3511,
    [187] = __ccgo_ts + 3522,
    [188] = __ccgo_ts + 3533,
    [189] = __ccgo_ts + 3538,
    [190] = __ccgo_ts + 3546
}; }

internal static array<uintptr> _azName1;
internal static void initᴛ_azName1() { _azName1 = new array<uintptr>(3){
    [0] = __ccgo_ts + 27980,
    [1] = __ccgo_ts + 5737,
    [2] = __ccgo_ts + 18076
}; }

internal static array<uintptr> _azName2;
internal static void initᴛ_azName2() { _azName2 = new array<uintptr>(5){
    [0] = __ccgo_ts + 40891,
    [1] = __ccgo_ts + 37882,
    [2] = __ccgo_ts + 27589,
    [3] = __ccgo_ts + 38577,
    [4] = __ccgo_ts + 13159
}; }

internal static readonly ж<array<uintptr>> Ꮡ_azOne = new StaticBox<array<uintptr>>(default(array<uintptr>));
internal static ref array<uintptr> _azOne => ref ((StaticBox<array<uintptr>>)Ꮡ_azOne).ValueSlot;
internal static void initᴛ_azOne() { _azOne = new array<uintptr>(1){
    [0] = __ccgo_ts + 11667
}; }

internal static array<uintptr> _azSql;
internal static void initᴛ_azSql() { _azSql = new array<uintptr>(8){
    [0] = __ccgo_ts + 27985,
    [1] = __ccgo_ts + 28038,
    [2] = __ccgo_ts + 28083,
    [3] = __ccgo_ts + 28135,
    [4] = __ccgo_ts + 28189,
    [5] = __ccgo_ts + 28234,
    [6] = __ccgo_ts + 28292,
    [7] = __ccgo_ts + 28347
}; }

internal static array<uintptr> _azType;
internal static void initᴛ_azType() { _azType = new array<uintptr>(4){
    [0] = __ccgo_ts + 5908,
    [1] = __ccgo_ts + 5917,
    [2] = __ccgo_ts + 5924,
    [3] = __ccgo_ts + 5930
}; }

internal static array<uintptr> _azType1;
internal static void initᴛ_azType1() { _azType1 = new array<uintptr>(6){
    [0] = __ccgo_ts + 1732,
    [1] = __ccgo_ts + 14487,
    [2] = __ccgo_ts + 14493,
    [3] = __ccgo_ts + 14498,
    [4] = __ccgo_ts + 14503,
    [5] = __ccgo_ts + 14493
}; }

internal static array<uintptr> _azType2;
internal static void initᴛ_azType2() { _azType2 = new array<uintptr>(5){
    [0] = __ccgo_ts + 6566,
    [1] = __ccgo_ts + 6561,
    [2] = __ccgo_ts + 8561,
    [3] = __ccgo_ts + 8556,
    [4] = __ccgo_ts + 1718
}; }

internal static array<uintptr> _azTypes;
internal static void initᴛ_azTypes() { _azTypes = new array<uintptr>(5){
    [0] = __ccgo_ts + 1194,
    [1] = __ccgo_ts + 1206,
    [2] = __ccgo_ts + 1211,
    [3] = __ccgo_ts + 1189,
    [4] = __ccgo_ts + 1746
}; }

internal static void _checkPtrmap(ж<libc.TLS> tls, uintptr pCheck, TPgno iChild, Tu8 eType, TPgno iParent) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        ref var ePtrmapType = ref heap(new Tu8(), out var ᏑePtrmapType);
        ref var iPtrmapParent = ref heap(new TPgno(), out var ᏑiPtrmapParent);
        int32 rc = default;
        _ = ePtrmapType;
        _ = iPtrmapParent;
        _ = rc;
        rc = _ptrmapGet(tls, (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FpBt, iChild, @unsafe.Pointer.FromPinnedBox(ᏑePtrmapType), @unsafe.Pointer.FromPinnedBox(ᏑiPtrmapParent));
        if (rc != SQLITE_OK) {
            if (rc == (int32)SQLITE_NOMEM || rc == (libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(12).Lsh((int64)(libc.Int32FromInt32(8))))) {
                _checkOom(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCheck);
            }
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4444, libc.VaList(bp + 8, iChild));
            return;
        }
        if (libc.Int32FromUint8(ePtrmapType) != libc.Int32FromUint8(eType) || iPtrmapParent != iParent) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4473, libc.VaList(bp + 8, iChild, libc.Int32FromUint8(eType), iParent, libc.Int32FromUint8(ePtrmapType), iPtrmapParent));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _checkReadTransaction(ж<libc.TLS> tls, uintptr db, uintptr p) {
    if (_sqlite3BtreeTxnState(ref (tls).DerefOrNull(), p) != SQLITE_TXN_NONE) {
        _sqlite3ErrorWithMsg(tls, db, (int32)SQLITE_ERROR, __ccgo_ts + 5231, 0);
        return (int32)SQLITE_ERROR;
    }
    return SQLITE_OK;
}

internal static int32 /*r*/ _checkRef(ж<libc.TLS> tls, uintptr pCheck, TPgno iPage) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if (iPage > (@unsafe.Pointer.To<TIntegrityCk>((@unsafe.Pointer)pCheck)).Value.FnCkPage || iPage == (uint32)0) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4396, libc.VaList(bp + 8, iPage));
            r = (int32)1; goto ᒐdone;
        }
        if (_getPageReferenced(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCheck, iPage) != 0) {
            _checkAppendMsg(tls, pCheck, __ccgo_ts + 4419, libc.VaList(bp + 8, iPage));
            r = (int32)1; goto ᒐdone;
        }
        _setPageReferenced(ref (tls).DerefOrNull(), (@unsafe.Pointer)pCheck, iPage);
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _codeTriggerProgram(ж<libc.TLS> tls, uintptr pParse, uintptr pStepList, int32 orconf) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr pSelect = default;
        uintptr pStep = default;
        uintptr v = default;
        ref var sDest = ref heap(new TSelectDest(), out var ᏑsDest);
        int32 v2 = default;
        _ = db;
        _ = pSelect;
        _ = pStep;
        _ = sDest;
        _ = v;
        _ = v2;
        v = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpVdbe;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pStep = pStepList;
        for (;;) {
            if (!(pStep != 0)) {
                break;
            }
            if (orconf == (int32)OE_Default) {
                v2 = libc.Int32FromUint8((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.Forconf);
            } else {
                v2 = libc.Int32FromUint8(libc.Uint8FromInt32(orconf));
            }
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeOrconf = libc.Uint8FromInt32(v2);
            if ((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FzSpan != 0) {
                _sqlite3VdbeAddOp4(tls, v, (int32)OP_Trace, (int32)0x7fffffff, (int32)1, 0, _sqlite3MPrintf(tls, db, __ccgo_ts + 6555, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FzSpan)), -(int32)7);
            }
            switch (libc.Int32FromUint8((@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.Fop)) {
            case var ᴛ when ᴛ == (int32)TK_UPDATE: {
                _sqlite3Update(tls, pParse, _sqlite3SrcListDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc, 0), _sqlite3ExprListDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpExprList, 0), _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpWhere, 0), libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeOrconf), (uintptr)0, (uintptr)0, (uintptr)0);
                _sqlite3VdbeAddOp0(tls, v, (int32)OP_ResetCount);
                break;
            }
            case var ᴛ when ᴛ == (int32)TK_INSERT: {
                _sqlite3Insert(tls, pParse, _sqlite3SrcListDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc, 0), _sqlite3SelectDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSelect, 0), _sqlite3IdListDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpIdList), libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeOrconf), _sqlite3UpsertDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpUpsert));
                _sqlite3VdbeAddOp0(tls, v, (int32)OP_ResetCount);
                break;
            }
            case var ᴛ when ᴛ == (int32)TK_DELETE: {
                _sqlite3DeleteFrom(tls, pParse, _sqlite3SrcListDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSrc, 0), _sqlite3ExprDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpWhere, 0), (uintptr)0, (uintptr)0);
                _sqlite3VdbeAddOp0(tls, v, (int32)OP_ResetCount);
                break;
            }
            default: {
                pSelect = _sqlite3SelectDup(tls, db, (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpSelect, 0);
                _sqlite3SelectDestInit(ref (tls).DerefOrNull(), @unsafe.Pointer.FromPinnedBox(ᏑsDest), (int32)SRT_Discard, 0);
                _sqlite3Select(tls, pParse, pSelect, @unsafe.Pointer.FromPinnedBox(ᏑsDest));
                _sqlite3SelectDelete(tls, (@unsafe.Pointer)db, pSelect);
                break;
                break;
            }}

            goto _1;
_1:;
            pStep = (@unsafe.Pointer.To<TTriggerStep>((@unsafe.Pointer)pStep)).Value.FpNext;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _concatFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    _concatFuncCore(tls, context, argc, argv, 0, __ccgo_ts + 1732);
}

internal static readonly ж<TFuncDef> Ꮡ_detach_func = new StaticBox<TFuncDef>(default(TFuncDef));
internal static ref TFuncDef _detach_func => ref ((StaticBox<TFuncDef>)Ꮡ_detach_func).ValueSlot;
internal static void initᴛ_detach_func() { _detach_func = new TFuncDef(
    FnArg: (int16)1,
    FfuncFlags: (uint32)SQLITE_UTF8,
    FzName: __ccgo_ts + 13626
); }

internal static int32 /*r*/ _disallowAggregatesInOrderByCb(ж<libc.TLS> tls, uintptr pWalker, uintptr pExpr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop) == (int32)TK_AGG_FUNCTION && (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.FpAggInfo == (uintptr)0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)(@unsafe.Pointer.To<TWalker>((@unsafe.Pointer)pWalker)).Value.FpParse, __ccgo_ts + 24599, libc.VaList(bp + 8, ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(pExpr + 8))));
        }
        r = WRC_Continue;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _dotlockLock(ж<libc.TLS> tls, uintptr id, int32 eFileLock) {
    uintptr pFile = default;
    uintptr zLockFile = default;
    int32 rc = default;
    int32 tErrno = default;
    _ = pFile;
    _ = rc;
    _ = tErrno;
    _ = zLockFile;
    pFile = id;
    zLockFile = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlockingContext;
    rc = SQLITE_OK;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock) > NO_LOCK) {
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock = libc.Uint8FromInt32(eFileLock);
        libc.Xutimes(tls, zLockFile, libc.UintptrFromInt32(0));
        return SQLITE_OK;
    }
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, Tmode_t, int32>>(_aSyscall[(int32)18].FpCurrent)(tls, zLockFile, (uint32)511);
    if (rc < 0) {
        tErrno = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls)));
        if ((int32)EEXIST == tErrno) {
            rc = (int32)SQLITE_BUSY;
        } else {
            rc = _sqliteErrorFromPosixError(ref (tls).DerefOrNull(), tErrno, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(15).Lsh((int64)(libc.Int32FromInt32(8))));
            if (rc != (int32)SQLITE_BUSY) {
                _storeLastErrno(ref (tls).DerefOrNull(), pFile, tErrno);
            }
        }
        return rc;
    }
    (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock = libc.Uint8FromInt32(eFileLock);
    return rc;
}

internal static int32 /*r*/ _dotlockUnlock(ж<libc.TLS> tls, uintptr id, int32 eFileLock) {
    uintptr pFile = default;
    uintptr zLockFile = default;
    int32 rc = default;
    int32 tErrno = default;
    _ = pFile;
    _ = rc;
    _ = tErrno;
    _ = zLockFile;
    pFile = id;
    zLockFile = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FlockingContext;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock) == eFileLock) {
        return SQLITE_OK;
    }
    if (eFileLock == (int32)SHARED_LOCK) {
        (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock = (uint8)SHARED_LOCK;
        return SQLITE_OK;
    }
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(_aSyscall[(int32)19].FpCurrent)(tls, zLockFile);
    if (rc < 0) {
        tErrno = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls)));
        if (tErrno == (int32)ENOENT) {
            rc = SQLITE_OK;
        } else {
            rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(8).Lsh((int64)(libc.Int32FromInt32(8)));
            _storeLastErrno(ref (tls).DerefOrNull(), pFile, tErrno);
        }
        return rc;
    }
    (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FeFileLock = (uint8)NO_LOCK;
    return SQLITE_OK;
}

internal static array<uintptr> _encnames;
internal static void initᴛ_encnames() { _encnames = new array<uintptr>(4){
    [0] = __ccgo_ts + 5345,
    [1] = __ccgo_ts + 5347,
    [2] = __ccgo_ts + 5349,
    [3] = __ccgo_ts + 5354
}; }

internal static readonly ж<array<_sqlite3Pragma_type>> Ꮡ_encnames1 = new StaticBox<array<_sqlite3Pragma_type>>(default(array<_sqlite3Pragma_type>));
internal static ref array<_sqlite3Pragma_type> _encnames1 => ref ((StaticBox<array<_sqlite3Pragma_type>>)Ꮡ_encnames1).ValueSlot;
internal static void initᴛ_encnames1() { _encnames1 = new array<_sqlite3Pragma_type>(9){
    [0] = new(
        FzName: __ccgo_ts + 19905,
        Fenc: (uint8)SQLITE_UTF8
    ),
    [1] = new(
        FzName: __ccgo_ts + 19910,
        Fenc: (uint8)SQLITE_UTF8
    ),
    [2] = new(
        FzName: __ccgo_ts + 19916,
        Fenc: (uint8)SQLITE_UTF16LE
    ),
    [3] = new(
        FzName: __ccgo_ts + 19925,
        Fenc: (uint8)SQLITE_UTF16BE
    ),
    [4] = new(
        FzName: __ccgo_ts + 19934,
        Fenc: (uint8)SQLITE_UTF16LE
    ),
    [5] = new(
        FzName: __ccgo_ts + 19942,
        Fenc: (uint8)SQLITE_UTF16BE
    ),
    [6] = new(
        FzName: __ccgo_ts + 19950
    ),
    [7] = new(
        FzName: __ccgo_ts + 19957
    ),
    [8] = new()
}; }

internal static void _explainAppendTerm(ж<libc.TLS> tls, uintptr pStr, uintptr pIdx, int32 nTerm, int32 iTerm, int32 bAnd, uintptr zOp) {
    int32 i = default;
    _ = i;
    if (bAnd != 0) {
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 23774, (int32)5);
    }
    if (nTerm > (int32)1) {
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 23780, (int32)1);
    }
    i = 0;
    for (;;) {
        if (!(i < nTerm)) {
            break;
        }
        if (i != 0) {
            Xsqlite3_str_append(tls, pStr, __ccgo_ts + 12016, (int32)1);
        }
        Xsqlite3_str_appendall(tls, pStr, _explainIndexColumnName(ref (tls).DerefOrNull(), pIdx, iTerm + i));
        goto _1;
_1:;
        i = i + 1;
    }
    if (nTerm > (int32)1) {
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 5343, (int32)1);
    }
    Xsqlite3_str_append(tls, pStr, zOp, (int32)1);
    if (nTerm > (int32)1) {
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 23780, (int32)1);
    }
    i = 0;
    for (;;) {
        if (!(i < nTerm)) {
            break;
        }
        if (i != 0) {
            Xsqlite3_str_append(tls, pStr, __ccgo_ts + 12016, (int32)1);
        }
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 5345, (int32)1);
        goto _2;
_2:;
        i = i + 1;
    }
    if (nTerm > (int32)1) {
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 5343, (int32)1);
    }
}

internal static void _explainIndexRange(ж<libc.TLS> tls, uintptr pStr, uintptr pLoop) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 i = default;
        int32 j = default;
        Tu16 nEq = default;
        Tu16 nSkip = default;
        uintptr pIndex = default;
        uintptr z = default;
        uintptr v2 = default;
        _ = i;
        _ = j;
        _ = nEq;
        _ = nSkip;
        _ = pIndex;
        _ = z;
        _ = v2;
        pIndex = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FpIndex;
        nEq = (@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnEq;
        nSkip = (@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FnSkip;
        if (libc.Int32FromUint16(nEq) == 0 && (Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & libc.Uint32FromInt32(libc.Int32FromInt32(WHERE_BTM_LIMIT) | libc.Int32FromInt32(WHERE_TOP_LIMIT))) == (uint32)0) {
            return;
        }
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 23782, (int32)2);
        i = 0;
        for (;;) {
            if (!(i < libc.Int32FromUint16(nEq))) {
                break;
            }
            z = _explainIndexColumnName(ref (tls).DerefOrNull(), pIndex, i);
            if (i != 0) {
                Xsqlite3_str_append(tls, pStr, __ccgo_ts + 23774, (int32)5);
            }
            if (i >= libc.Int32FromUint16(nSkip)) {
                v2 = __ccgo_ts + 23785;
            } else {
                v2 = __ccgo_ts + 23790;
            }
            Xsqlite3_str_appendf(tls, pStr, v2, libc.VaList(bp + 8, z));
            goto _1;
_1:;
            i = i + 1;
        }
        j = i;
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_BTM_LIMIT) != 0) {
            _explainAppendTerm(tls, pStr, pIndex, libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnBtm), j, i, __ccgo_ts + 23798);
            i = (int32)1;
        }
        if ((Tu32)((@unsafe.Pointer.To<TWhereLoop>((@unsafe.Pointer)pLoop)).Value.FwsFlags & (uint32)WHERE_TOP_LIMIT) != 0) {
            _explainAppendTerm(tls, pStr, pIndex, libc.Int32FromUint16((@unsafe.Pointer.To<_whereLoopIsNoBetter_type>((@unsafe.Pointer)(pLoop + 24))).Value.FnTop), j, i, __ccgo_ts + 23800);
        }
        Xsqlite3_str_append(tls, pStr, __ccgo_ts + 5343, (int32)1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _explainTempTable(ж<libc.TLS> tls, uintptr pParse, uintptr zUsage) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        _sqlite3VdbeExplain(tls, pParse, (uint8)0, __ccgo_ts + 20655, libc.VaList(bp + 8, zUsage));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _fts5ExtractExprText(ж<libc.TLS> tls, uintptr pConfig, uintptr pVal, @unsafe.Pointer pzText, @unsafe.Pointer pbFreeAndReset) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        ref var nLoc = ref heap(new int32(), out var ᏑnLoc);
        ref var nText = ref heap(new int32(), out var ᏑnText);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        ref var pLoc = ref heap(new uintptr(), out var ᏑpLoc);
        ref var pText = ref heap(new uintptr(), out var ᏑpText);
        _ = nLoc;
        _ = nText;
        _ = pLoc;
        _ = pText;
        _ = rc;
        rc = SQLITE_OK;
        if (_sqlite3Fts5IsLocaleValue(tls, pConfig, pVal) != 0) {
            pText = (uintptr)0;
            nText = 0;
            pLoc = (uintptr)0;
            nLoc = 0;
            rc = _sqlite3Fts5DecodeLocaleValue(tls, pVal, @unsafe.Pointer.FromBox(ᏑpText), @unsafe.Pointer.FromPinnedBox(ᏑnText), @unsafe.Pointer.FromBox(ᏑpLoc), @unsafe.Pointer.FromPinnedBox(ᏑnLoc));
            (@unsafe.Pointer.To<uintptr>(pzText)).Value = _sqlite3Fts5Mprintf(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ts + 12164, libc.VaList(bp + 8, nText, pText));
            if (rc == SQLITE_OK) {
                _sqlite3Fts5SetLocale(ref (tls).DerefOrNull(), pConfig, pLoc, nLoc);
            }
            (@unsafe.Pointer.To<int32>(pbFreeAndReset)).Value = (int32)1;
        } else {
            (@unsafe.Pointer.To<uintptr>(pzText)).Value = Xsqlite3_value_text(tls, pVal);
            (@unsafe.Pointer.To<int32>(pbFreeAndReset)).Value = 0;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5Fts5Func(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apArg) {
    uintptr pGlobal = default;
    uintptr ppApi = default;
    _ = pGlobal;
    _ = ppApi;
    pGlobal = Xsqlite3_user_data(tls, pCtx);
    _ = nArg;
    ppApi = Xsqlite3_value_pointer(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)), __ccgo_ts + 40787);
    if (ppApi != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppApi))).Value = pGlobal;
    }
}

internal static int32 /*r*/ _fts5IndexCorruptIdx(ж<libc.TLS> tls, uintptr pIdx) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.Frc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        _sqlite3Fts5ConfigErrmsg(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.FpConfig, __ccgo_ts + 39202, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.FpConfig)).Value.FzName));
        r = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5IndexCorruptIter(ж<libc.TLS> tls, uintptr pIdx, uintptr pIter) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.Frc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        _sqlite3Fts5ConfigErrmsg(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.FpConfig, __ccgo_ts + 39150, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FiLeafPgno, (@unsafe.Pointer.To<TFts5StructureSegment>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5SegIter>((@unsafe.Pointer)pIter)).Value.FpSeg)).Value.FiSegid, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.FpConfig)).Value.FzName));
        r = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5IndexCorruptRowid(ж<libc.TLS> tls, uintptr pIdx, Ti64 iRowid) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.Frc = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        _sqlite3Fts5ConfigErrmsg(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.FpConfig, __ccgo_ts + 39093, libc.VaList(bp + 8, iRowid, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)pIdx)).Value.FpConfig)).Value.FzName));
        r = libc.Int32FromInt32(SQLITE_CORRUPT) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5IntegrityMethod(ж<libc.TLS> tls, uintptr pVtab, uintptr zSchema, uintptr zTabname, int32 isQuick, uintptr pzErr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr pTab = default;
        int32 rc = default;
        int32 v1 = default;
        _ = pTab;
        _ = rc;
        _ = v1;
        pTab = pVtab;
        _ = isQuick;
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FpzErrmsg = pzErr;
        rc = _sqlite3Fts5StorageIntegrity(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage, 0);
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr)) == (uintptr)0 && rc != SQLITE_OK) {
            if ((rc & (int32)0xff) == (int32)SQLITE_CORRUPT) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 40898, libc.VaList(bp + 8, zSchema, zTabname));
                if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr)) != 0) {
                    v1 = SQLITE_OK;
                } else {
                    v1 = (int32)SQLITE_NOMEM;
                }
                rc = v1;
            } else {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 40944, libc.VaList(bp + 8, zSchema, zTabname, Xsqlite3_errstr(tls, rc)));
            }
        } else {
            if ((rc & (int32)0xff) == (int32)SQLITE_CORRUPT) {
                rc = SQLITE_OK;
            }
        }
        _sqlite3Fts5IndexCloseReader(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex);
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FpzErrmsg = (uintptr)0;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _fts5PrepareStatement(ж<libc.TLS> tls, uintptr ppStmt, uintptr pConfig, uintptr zFmt, uintptr va) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tva_list ap = default;
        ref var pRet = ref heap(new uintptr(), out var ᏑpRet);
        uintptr zSql = default;
        int32 rc = default;
        _ = ap;
        _ = pRet;
        _ = rc;
        _ = zSql;
        pRet = (uintptr)0;
        ap = va;
        zSql = Xsqlite3_vmprintf(tls, zFmt, ap);
        if (zSql == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            rc = _sqlite3_prepare_v3__ccgo_gp(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, zSql, -(int32)1, (uint32)SQLITE_PREPARE_PERSISTENT, @unsafe.Pointer.FromBox(ᏑpRet), (uintptr)0);
            if (rc != SQLITE_OK) {
                _sqlite3Fts5ConfigErrmsg(tls, pConfig, __ccgo_ts + 3982, libc.VaList(bp + 8, Xsqlite3_errmsg(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb)));
            }
            Xsqlite3_free(tls, zSql);
        }
        _ = ap;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppStmt))).Value = pRet;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5SourceIdFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apUnused) {
    _ = nArg;
    _ = apUnused;
    Xsqlite3_result_text(tls, pCtx, __ccgo_ts + 40800, -(int32)1, (uintptr)(-libc.Int32FromInt32(1)));
}

internal static int32 /*r*/ _fts5SpecialInsert(ж<libc.TLS> tls, uintptr pTab, uintptr zCmd, uintptr pVal) {
    ref var bError = ref heap(new int32(), out var ᏑbError);
    int32 bLoadConfig = default;
    int32 iArg = default;
    int32 nMerge = default;
    int32 rc = default;
    uintptr pConfig = default;
    _ = bError;
    _ = bLoadConfig;
    _ = iArg;
    _ = nMerge;
    _ = pConfig;
    _ = rc;
    pConfig = (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig;
    rc = SQLITE_OK;
    bError = 0;
    bLoadConfig = 0;
    if (0 == Xsqlite3_stricmp(tls, __ccgo_ts + 40289, zCmd)) {
        if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == FTS5_CONTENT_NORMAL) {
            _fts5SetVtabError(tls, pTab, __ccgo_ts + 40300, 0);
            rc = (int32)SQLITE_ERROR;
        } else {
            rc = _sqlite3Fts5StorageDeleteAll(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage);
        }
        bLoadConfig = (int32)1;
    } else {
        if (0 == Xsqlite3_stricmp(tls, __ccgo_ts + 40380, zCmd)) {
            if (_fts5IsContentless(ref (tls).DerefOrNull(), pTab, (int32)1) != 0) {
                _fts5SetVtabError(tls, pTab, __ccgo_ts + 40388, 0);
                rc = (int32)SQLITE_ERROR;
            } else {
                rc = _sqlite3Fts5StorageRebuild(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage);
            }
            bLoadConfig = (int32)1;
        } else {
            if (0 == Xsqlite3_stricmp(tls, __ccgo_ts + 18743, zCmd)) {
                rc = _sqlite3Fts5StorageOptimize(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage);
            } else {
                if (0 == Xsqlite3_stricmp(tls, __ccgo_ts + 40444, zCmd)) {
                    nMerge = Xsqlite3_value_int(tls, pVal);
                    rc = _sqlite3Fts5StorageMerge(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage, nMerge);
                } else {
                    if (0 == Xsqlite3_stricmp(tls, __ccgo_ts + 40450, zCmd)) {
                        iArg = Xsqlite3_value_int(tls, pVal);
                        rc = _sqlite3Fts5StorageIntegrity(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage, iArg);
                    } else {
                        if (0 == Xsqlite3_stricmp(tls, __ccgo_ts + 40466, zCmd)) {
                            rc = _sqlite3Fts5FlushToDisk(tls, pTab);
                        } else {
                            rc = _sqlite3Fts5FlushToDisk(tls, pTab);
                            if (rc == SQLITE_OK) {
                                rc = _sqlite3Fts5IndexLoadConfig(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex);
                            }
                            if (rc == SQLITE_OK) {
                                rc = _sqlite3Fts5ConfigSetValue(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig, zCmd, pVal, @unsafe.Pointer.FromPinnedBox(ᏑbError));
                            }
                            if (rc == SQLITE_OK) {
                                if (bError != 0) {
                                    rc = (int32)SQLITE_ERROR;
                                } else {
                                    rc = _sqlite3Fts5StorageConfigValue(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.FpStorage, zCmd, pVal, 0);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    if (rc == SQLITE_OK && bLoadConfig != 0) {
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FiCookie = (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpConfig)).Value.FiCookie - 1;
        rc = _sqlite3Fts5IndexLoadConfig(tls, (@unsafe.Pointer.To<TFts5FullTable>((@unsafe.Pointer)pTab)).Value.Fp.FpIndex);
    }
    return rc;
}

internal static int32 /*r*/ _fts5StorageCount(ж<libc.TLS> tls, uintptr p, uintptr zSuffix, @unsafe.Pointer pnRow) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        ref var pCnt = ref heap(new uintptr(), out var ᏑpCnt);
        uintptr pConfig = default;
        uintptr zSql = default;
        int32 rc = default;
        _ = pCnt;
        _ = pConfig;
        _ = rc;
        _ = zSql;
        pConfig = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig;
        zSql = Xsqlite3_mprintf(tls, __ccgo_ts + 42093, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, zSuffix));
        if (zSql == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            pCnt = (uintptr)0;
            rc = _sqlite3_prepare_v2__ccgo_gp(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, zSql, -(int32)1, @unsafe.Pointer.FromBox(ᏑpCnt), nil);
            if (rc == SQLITE_OK) {
                if ((int32)SQLITE_ROW == Xsqlite3_step(tls, pCnt)) {
                    (@unsafe.Pointer.To<Ti64>(pnRow)).Value = Xsqlite3_column_int64(tls, pCnt, 0);
                }
                rc = Xsqlite3_finalize(tls, pCnt);
            }
        }
        Xsqlite3_free(tls, zSql);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5StorageRenameOne(ж<libc.TLS> tls, uintptr pConfig, @unsafe.Pointer pRc, uintptr zTail, uintptr zName) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        if (~@unsafe.Pointer.To<int32>(pRc) == SQLITE_OK) {
            (@unsafe.Pointer.To<int32>(pRc)).Value = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 41737, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, zTail, zName, zTail));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uintptr /*r*/ _fts5ValueToText(ж<libc.TLS> tls, uintptr pVal) {
    uintptr zRet = default;
    uintptr v1 = default;
    _ = zRet;
    _ = v1;
    zRet = Xsqlite3_value_text(tls, pVal);
    if (zRet != 0) {
        v1 = zRet;
    } else {
        v1 = __ccgo_ts + 1732;
    }
    return v1;
}

internal static int32 /*r*/ _fts5VocabIsBinary(ж<libc.TLS> tls, uintptr pInfo, int32 iCons) {
    return libc.BoolInt32(0 == Xsqlite3_stricmp(tls, __ccgo_ts + 42712, Xsqlite3_vtab_collation(tls, pInfo, iCons)));
}

internal static int32 /*r*/ _fts5VocabTableType(ж<libc.TLS> tls, uintptr zType, uintptr pzErr, @unsafe.Pointer peType) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        uintptr zCopy = default;
        _ = rc;
        _ = zCopy;
        rc = SQLITE_OK;
        zCopy = _sqlite3Fts5Strndup(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), zType, -(int32)1);
        if (rc == SQLITE_OK) {
            _sqlite3Fts5Dequote(ref (tls).DerefOrNull(), zCopy);
            if (Xsqlite3_stricmp(tls, zCopy, __ccgo_ts + 42510) == 0) {
                (@unsafe.Pointer.To<int32>(peType)).Value = FTS5_VOCAB_COL;
            } else {
                if (Xsqlite3_stricmp(tls, zCopy, __ccgo_ts + 42514) == 0) {
                    (@unsafe.Pointer.To<int32>(peType)).Value = (int32)FTS5_VOCAB_ROW;
                } else {
                    if (Xsqlite3_stricmp(tls, zCopy, __ccgo_ts + 42518) == 0) {
                        (@unsafe.Pointer.To<int32>(peType)).Value = (int32)FTS5_VOCAB_INSTANCE;
                    } else {
                        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 42527, libc.VaList(bp + 8, zCopy));
                        rc = (int32)SQLITE_ERROR;
                    }
                }
            }
            Xsqlite3_free(tls, zCopy);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _fts5WriteFlushBtree(ж<libc.TLS> tls, uintptr p, uintptr pWriter) {
    int32 bFlag = default;
    uintptr z = default;
    uintptr v1 = default;
    _ = bFlag;
    _ = z;
    _ = v1;
    if ((@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage == 0) {
        return;
    }
    bFlag = _fts5WriteFlushDlidx(tls, p, pWriter);
    if ((@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fbtterm.Fn > 0) {
            v1 = (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fbtterm.Fp;
        } else {
            v1 = __ccgo_ts + 1732;
        }
        z = v1;
        Xsqlite3_bind_blob(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter, (int32)2, z, (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.Fbtterm.Fn, libc.UintptrFromInt32(0));
        Xsqlite3_bind_int64(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter, (int32)3, (int64)bFlag + ((int64)(@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage).Lsh((int64)(libc.Int32FromInt32(1))));
        Xsqlite3_step(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter);
        (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.Frc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter);
        Xsqlite3_bind_null(tls, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpIdxWriter, (int32)2);
    }
    (@unsafe.Pointer.To<TFts5SegWriter>((@unsafe.Pointer)pWriter)).Value.FiBtPage = 0;
}

internal static void _fts5yyStackOverflow(ж<libc.TLS> tls, uintptr fts5yypParser) {
    uintptr pParse = default;
    _ = pParse;
    pParse = (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.FpParse;
    while ((@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yytos > (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.Ffts5yystack) {
        _fts5yy_pop_parser_stack(tls, fts5yypParser);
    }
    _sqlite3Fts5ParseError(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 37381, 0);
    (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.FpParse = pParse;
}

internal static void _fts5yy_syntax_error(ж<libc.TLS> tls, uintptr fts5yypParser, int32 fts5yymajor, TFts5Token fts5yyminor) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr pParse = default;
        _ = pParse;
        pParse = (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.FpParse;
        _ = fts5yymajor;
        _sqlite3Fts5ParseError(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 37409, libc.VaList(bp + 8, fts5yyminor.Fn, fts5yyminor.Fp));
        (@unsafe.Pointer.To<Tfts5yyParser>((@unsafe.Pointer)fts5yypParser)).Value.FpParse = pParse;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _geopolyBestIndex(ж<libc.TLS> tls, uintptr tab, uintptr pIdxInfo) {
    int32 iFuncTerm = default;
    int32 iRowidTerm = default;
    int32 idxNum = default;
    int32 ii = default;
    uintptr p = default;
    _ = iFuncTerm;
    _ = iRowidTerm;
    _ = idxNum;
    _ = ii;
    _ = p;
    iRowidTerm = -(int32)1;
    iFuncTerm = -(int32)1;
    idxNum = 0;
    _ = tab;
    ii = 0;
    for (;;) {
        if (!(ii < (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FnConstraint)) {
            break;
        }
        p = (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraint + (uintptr)ii * 12;
        if (!((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fusable != 0)) {
            goto _1;
        }
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn < 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) == (int32)SQLITE_INDEX_CONSTRAINT_EQ) {
            iRowidTerm = ii;
            break;
        }
        if ((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.FiColumn == 0 && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) >= (int32)SQLITE_INDEX_CONSTRAINT_FUNCTION) {
            iFuncTerm = ii;
            idxNum = libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3_index_constraint>((@unsafe.Pointer)p)).Value.Fop) - (int32)SQLITE_INDEX_CONSTRAINT_FUNCTION + (int32)2;
        }
        goto _1;
_1:;
        ii = ii + 1;
    }
    if (iRowidTerm >= 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = (int32)1;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr = __ccgo_ts + 18076;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)iRowidTerm * 8))).Value.FargvIndex = (int32)1;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)iRowidTerm * 8))).Value.Fomit = (uint8)1;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)30D;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)1;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxFlags = (int32)SQLITE_INDEX_SCAN_UNIQUE;
        return SQLITE_OK;
    }
    if (iFuncTerm >= 0) {
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = idxNum;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr = __ccgo_ts + 30154;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)iFuncTerm * 8))).Value.FargvIndex = (int32)1;
        (@unsafe.Pointer.To<Tsqlite3_index_constraint_usage>(__ccgo_ptr((@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FaConstraintUsage + (uintptr)iFuncTerm * 8))).Value.Fomit = (uint8)0;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)300D;
        (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)10;
        return SQLITE_OK;
    }
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxNum = (int32)4;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FidxStr = __ccgo_ts + 30160;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedCost = (float64)3e+06D;
    (@unsafe.Pointer.To<Tsqlite3_index_info>((@unsafe.Pointer)pIdxInfo)).Value.FestimatedRows = (int64)100000;
    return SQLITE_OK;
}

internal static int32 /*r*/ _geopolyFindFunction(ж<libc.TLS> tls, uintptr pVtab, int32 nArg, uintptr zName, uintptr __ccgo_fp_pxFunc, uintptr ppArg) {
    _ = pVtab;
    _ = nArg;
    if (Xsqlite3_stricmp(tls, zName, __ccgo_ts + 30209) == 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(__ccgo_fp_pxFunc))).Value = __ccgo_fp(_geopolyOverlapFunc);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppArg))).Value = (uintptr)0;
        return (int32)SQLITE_INDEX_CONSTRAINT_FUNCTION;
    }
    if (Xsqlite3_stricmp(tls, zName, __ccgo_ts + 30225) == 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(__ccgo_fp_pxFunc))).Value = __ccgo_fp(_geopolyWithinFunc);
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppArg))).Value = (uintptr)0;
        return libc.Int32FromInt32(SQLITE_INDEX_CONSTRAINT_FUNCTION) + libc.Int32FromInt32(1);
    }
    return 0;
}

internal static void _geopolyJsonFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        uintptr x = default;
        int32 i = default;
        _ = db;
        _ = i;
        _ = p;
        _ = x;
        p = _geopolyFuncParam(tls, context, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(argv)), nil);
        _ = argc;
        if (p != 0) {
            db = Xsqlite3_context_db_handle(tls, context);
            x = Xsqlite3_str_new(tls, db);
            Xsqlite3_str_append(tls, x, __ccgo_ts + 26857, (int32)1);
            i = 0;
            for (;;) {
                if (!(i < (@unsafe.Pointer.To<TGeoPoly>((@unsafe.Pointer)p)).Value.FnVertex)) {
                    break;
                }
                Xsqlite3_str_appendf(tls, x, __ccgo_ts + 30018, libc.VaList(bp + 8, (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(i * (int32)2) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(i * (int32)2 + (int32)1) * 4)))));
                goto _1;
_1:;
                i = i + 1;
            }
            Xsqlite3_str_appendf(tls, x, __ccgo_ts + 30029, libc.VaList(bp + 8, (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2)) * 4))), (float64)(~@unsafe.Pointer.To<TGeoCoord>(__ccgo_ptr(p + 8 + (uintptr)(libc.Int32FromInt32(0) * libc.Int32FromInt32(2) + libc.Int32FromInt32(1)) * 4)))));
            Xsqlite3_result_text(tls, context, Xsqlite3_str_finish(tls, x), -(int32)1, __ccgo_fp(Xsqlite3_free));
            Xsqlite3_free(tls, p);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _getAutoVacuum(ж<libc.TLS> tls, uintptr z) {
    int32 i = default;
    int32 v1 = default;
    _ = i;
    _ = v1;
    if (0 == _sqlite3StrICmp(ref (tls).DerefOrNull(), z, __ccgo_ts + 8582)) {
        return BTREE_AUTOVACUUM_NONE;
    }
    if (0 == _sqlite3StrICmp(ref (tls).DerefOrNull(), z, __ccgo_ts + 19129)) {
        return (int32)BTREE_AUTOVACUUM_FULL;
    }
    if (0 == _sqlite3StrICmp(ref (tls).DerefOrNull(), z, __ccgo_ts + 19134)) {
        return (int32)BTREE_AUTOVACUUM_INCR;
    }
    i = _sqlite3Atoi(tls, z);
    if (i >= 0 && i <= (int32)2) {
        v1 = i;
    } else {
        v1 = 0;
    }
    return libc.Int32FromUint8(libc.Uint8FromInt32(v1));
}

internal static int32 /*r*/ _getLockingMode(ref libc.TLS tls, uintptr z) {
    if (z != 0) {
        if (0 == _sqlite3StrICmp(ref tls, z, __ccgo_ts + 19112)) {
            return (int32)PAGER_LOCKINGMODE_EXCLUSIVE;
        }
        if (0 == _sqlite3StrICmp(ref tls, z, __ccgo_ts + 19122)) {
            return PAGER_LOCKINGMODE_NORMAL;
        }
    }
    return -(int32)1;
}

internal static int32 /*r*/ _isAlterableTable(ж<libc.TLS> tls, uintptr pParse, uintptr pTab) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if (0 == Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, __ccgo_ts + 6830, (int32)7) || (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Eponymous) != (uint32)0 || (Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Shadow) != (uint32)0 && _sqlite3ReadOnlyShadowTables(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 8717, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
            r = (int32)1; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _isAsteriskTerm(ж<libc.TLS> tls, uintptr pParse, uintptr pTerm) {
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTerm)).Value.Fop) == (int32)TK_ASTERISK) {
        return (int32)1;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTerm)).Value.Fop) != (int32)TK_DOT) {
        return 0;
    }
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)(@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pTerm)).Value.FpRight)).Value.Fop) != (int32)TK_ASTERISK) {
        return 0;
    }
    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22696, 0);
    return (int32)1;
}

internal static int32 /*r*/ _isRealTable(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 iOp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        array<uintptr> azMsg = new(3);
        uintptr zType = default;
        _ = azMsg;
        _ = zType;
        zType = (uintptr)0;
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW) {
            zType = __ccgo_ts + 11219;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
            zType = __ccgo_ts + 11224;
        }
        if (zType != 0) {
            azMsg.Assign(new array<uintptr>(3){
                [0] = __ccgo_ts + 11238,
                [1] = __ccgo_ts + 11256,
                [2] = __ccgo_ts + 11273
            });
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 11293, libc.VaList(bp + 8, azMsg[iOp], zType, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
            r = (int32)1; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _isRowidAlias(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, uintptr zCol, uintptr zOp) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_WithoutRowid) == (uint32)0 && _sqlite3IsRowid(ref (tls).DerefOrNull(), zCol) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 10363, libc.VaList(bp + 8, zOp, zCol));
            r = (int32)SQLITE_ERROR; goto ᒐdone;
        }
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _isValidSchemaTableName(ж<libc.TLS> tls, uintptr zTab, uintptr pTab, uintptr zDb) {
    uintptr zLegacy = default;
    _ = zLegacy;
    if (Xsqlite3_strnicmp(tls, zTab, __ccgo_ts + 6830, (int32)7) != 0) {
        return 0;
    }
    zLegacy = (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName;
    if (libc.Xstrcmp(tls, zLegacy + (uintptr)7, __ccgo_ts + 6838 + 7) == 0) {
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zTab + (uintptr)7, __ccgo_ts + 6857 + 7) == 0) {
            return (int32)1;
        }
        if (zDb == (uintptr)0) {
            return 0;
        }
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zTab + (uintptr)7, __ccgo_ts + 6370 + 7) == 0) {
            return (int32)1;
        }
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zTab + (uintptr)7, __ccgo_ts + 6876 + 7) == 0) {
            return (int32)1;
        }
    } else {
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zTab + (uintptr)7, __ccgo_ts + 6876 + 7) == 0) {
            return (int32)1;
        }
    }
    return 0;
}

internal static uintptr /*r*/ _jsonBadPathError(ж<libc.TLS> tls, uintptr ctx, uintptr zPath, int32 rc) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr zMsg = default;
        _ = zMsg;
        if (rc == libc.Int32FromUint32(JSON_LOOKUP_NOTARRAY)) {
            zMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 26793, libc.VaList(bp + 8, zPath));
        } else {
            if (rc == libc.Int32FromUint32(JSON_LOOKUP_ERROR)) {
                zMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 26645, 0);
            } else {
                if (rc == libc.Int32FromUint32(JSON_LOOKUP_TOODEEP)) {
                    zMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 26818, 0);
                } else {
                    zMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 26837, libc.VaList(bp + 8, zPath));
                }
            }
        }
        if (ctx == (uintptr)0) {
            r = zMsg; goto ᒐdone;
        }
        if (zMsg != 0) {
            Xsqlite3_result_error(tls, ctx, zMsg, -(int32)1);
            Xsqlite3_free(tls, zMsg);
        } else {
            Xsqlite3_result_error_nomem(tls, ctx);
        }
        r = (uintptr)0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _jsonEachMalformedInput(ж<libc.TLS> tls, uintptr cur) {
    int32 v1 = default;
    _ = v1;
    Xsqlite3_free(tls, (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)cur)).Value.FpVtab)).Value.FzErrMsg);
    (@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)cur)).Value.FpVtab)).Value.FzErrMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 26645, 0);
    _jsonEachCursorReset(tls, cur);
    if ((@unsafe.Pointer.To<Tsqlite3_vtab>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_vtab_cursor>((@unsafe.Pointer)cur)).Value.FpVtab)).Value.FzErrMsg != 0) {
        v1 = (int32)SQLITE_ERROR;
    } else {
        v1 = (int32)SQLITE_NOMEM;
    }
    return v1;
}

internal static void _jsonReplaceFunc(ж<libc.TLS> tls, uintptr ctx, int32 argc, uintptr argv) {
    if (argc < (int32)1) {
        return;
    }
    if ((argc & (int32)1) == 0) {
        _jsonWrongNumArgs(tls, ctx, __ccgo_ts + 17181);
        return;
    }
    _jsonInsertIntoBlob(tls, ctx, argc, argv, (int32)JEDIT_REPL);
}

internal static void _jsonWrongNumArgs(ж<libc.TLS> tls, uintptr pCtx, uintptr zFuncName) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr zMsg = default;
        _ = zMsg;
        zMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 26691, libc.VaList(bp + 8, zFuncName));
        Xsqlite3_result_error(tls, pCtx, zMsg, -(int32)1);
        Xsqlite3_free(tls, zMsg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static array<uintptr> _jsonbType;
internal static void initᴛ_jsonbType() { _jsonbType = new array<uintptr>(17){
    [0] = __ccgo_ts + 1718,
    [1] = __ccgo_ts + 8252,
    [2] = __ccgo_ts + 8257,
    [3] = __ccgo_ts + 6566,
    [4] = __ccgo_ts + 6566,
    [5] = __ccgo_ts + 6561,
    [6] = __ccgo_ts + 6561,
    [7] = __ccgo_ts + 8561,
    [8] = __ccgo_ts + 8561,
    [9] = __ccgo_ts + 8561,
    [10] = __ccgo_ts + 8561,
    [11] = __ccgo_ts + 26565,
    [12] = __ccgo_ts + 26571,
    [13] = __ccgo_ts + 1732,
    [14] = __ccgo_ts + 1732,
    [15] = __ccgo_ts + 1732,
    [16] = __ccgo_ts + 1732
}; }

internal static Tu32 /*r*/ _jsonbValidityCheck(ж<libc.TLS> tls, @unsafe.Pointer pParse, Tu32 i, Tu32 iEnd, Tu32 iDepth) {
    Tu32 r = default;
    GoFrame ᒐ = default;
    try {
        tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var c = ref heap(new Tu32(), out var Ꮡc);
        Tu32 cnt = default;
        Tu32 j = default;
        Tu32 k = default;
        Tu32 n = default;
        Tu32 sub = default;
        Tu32 sub1 = default;
        ref var sz = ref heap(new Tu32(), out var Ꮡsz);
        Tu32 szC = default;
        Tu8 seen = default;
        Tu8 x = default;
        uintptr z = default;
        uint32 v1 = default;
        _ = c;
        _ = cnt;
        _ = j;
        _ = k;
        _ = n;
        _ = seen;
        _ = sub;
        _ = sub1;
        _ = sz;
        _ = szC;
        _ = x;
        _ = z;
        _ = v1;
        if (iDepth > (uint32)JSON_MAX_DEPTH) {
            r = i + (uint32)1; goto ᒐdone;
        }
        sz = (uint32)0;
        n = _jsonbPayloadSize(ref (tls).DerefOrNull(), pParse, i, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
        if (n == (uint32)0) {
            r = i + (uint32)1; goto ᒐdone;
        }
        if (i + n + sz != iEnd) {
            r = i + (uint32)1; goto ᒐdone;
        }
        z = ~@unsafe.Pointer.To<uintptr>(pParse);
        x = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)i))) & (int32)0x0f);
        var exprᴛ1 = libc.Int32FromUint8(x);
        var matchᴛ1 = false;
        if (exprᴛ1 == JSONB_NULL) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)JSONB_TRUE) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)JSONB_FALSE) { matchᴛ1 = true;
            if (n + sz == (uint32)1) {
                v1 = (uint32)0;
            } else {
                v1 = i + (uint32)1;
            }
            r = v1; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_INT) { matchᴛ1 = true;
            if (sz < (uint32)1) {
                r = i + (uint32)1; goto ᒐdone;
            }
            j = i + n;
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'-') {
                j = j + 1;
                if (sz < (uint32)2) {
                    r = i + (uint32)1; goto ᒐdone;
                }
            }
            k = i + n + sz;
            while (j < k) {
                if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))]) & (int32)0x04) != 0) {
                    j = j + 1;
                } else {
                    r = j + (uint32)1; goto ᒐdone;
                }
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_INT5) { matchᴛ1 = true;
            if (sz < (uint32)3) {
                r = i + (uint32)1; goto ᒐdone;
            }
            j = i + n;
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'-') {
                if (sz < (uint32)4) {
                    r = i + (uint32)1; goto ᒐdone;
                }
                j = j + 1;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) != (int32)'0') {
                r = i + (uint32)1; goto ᒐdone;
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) != (int32)'x' && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) != (int32)'X') {
                r = j + (uint32)2; goto ᒐdone;
            }
            j = j + (uint32)2;
            k = i + n + sz;
            while (j < k) {
                if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))]) & (int32)0x08) != 0) {
                    j = j + 1;
                } else {
                    r = j + (uint32)1; goto ᒐdone;
                }
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_FLOAT) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)JSONB_FLOAT5) { matchᴛ1 = true;
            seen = (uint8)0;
            if (sz < (uint32)2) {
                r = i + (uint32)1; goto ᒐdone;
            }
            j = i + n;
            k = j + sz;
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'-') {
                j = j + 1;
                if (sz < (uint32)3) {
                    r = i + (uint32)1; goto ᒐdone;
                }
            }
            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'.') {
                if (libc.Int32FromUint8(x) == (int32)JSONB_FLOAT) {
                    r = j + (uint32)1; goto ᒐdone;
                }
                if (!((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))]) & libc.Int32FromInt32(0x04)) != 0)) {
                    r = j + (uint32)1; goto ᒐdone;
                }
                j = j + (uint32)2;
                seen = (uint8)1;
            } else {
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'0' && libc.Int32FromUint8(x) == (int32)JSONB_FLOAT) {
                    if (j + (uint32)3 > k) {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) != (int32)'.' && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) != (int32)'e' && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) != (int32)'E') {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                    j = j + 1;
                }
            }
            for (;;) {
                if (!(j < k)) {
                    break;
                }
                if ((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))]) & (int32)0x04) != 0) {
                    goto _2;
                }
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'.') {
                    if (libc.Int32FromUint8(seen) > 0) {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                    if (libc.Int32FromUint8(x) == (int32)JSONB_FLOAT && (j == k - (uint32)1 || !((libc.Int32FromUint8(_sqlite3CtypeMap[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))]) & libc.Int32FromInt32(0x04)) != 0))) {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                    seen = (uint8)1;
                    goto _2;
                }
                if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'e' || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'E') {
                    if (libc.Int32FromUint8(seen) == (int32)2) {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                    if (j == k - (uint32)1) {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) == (int32)'+' || libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) == (int32)'-') {
                        j = j + 1;
                        if (j == k - (uint32)1) {
                            r = j + (uint32)1; goto ᒐdone;
                        }
                    }
                    seen = (uint8)2;
                    goto _2;
                }
                r = j + (uint32)1; goto ᒐdone;
                goto _2;
_2:;
                j = j + 1;
            }
            if (libc.Int32FromUint8(seen) == 0) {
                r = i + (uint32)1; goto ᒐdone;
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_TEXT) { matchᴛ1 = true;
            j = i + n;
            k = j + sz;
            while (j < k) {
                if (!(_jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))] != 0) && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) != (int32)'\'') {
                    r = j + (uint32)1; goto ᒐdone;
                }
                j = j + 1;
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_TEXTJ) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == (int32)JSONB_TEXT5) { matchᴛ1 = true;
            j = i + n;
            k = j + sz;
            while (j < k) {
                if (!(_jsonIsOk[~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))] != 0) && libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) != (int32)'\'') {
                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) == (int32)'"') {
                        if (libc.Int32FromUint8(x) == (int32)JSONB_TEXTJ) {
                            r = j + (uint32)1; goto ᒐdone;
                        }
                    } else {
                        if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) <= (int32)0x1f) {
                            if (libc.Int32FromUint8(x) == (int32)JSONB_TEXTJ) {
                                r = j + (uint32)1; goto ᒐdone;
                            }
                        } else {
                            if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) != (int32)'\\' || j + (uint32)1 >= k) {
                                r = j + (uint32)1; goto ᒐdone;
                            } else {
                                if (libc.Xstrchr(tls, __ccgo_ts + 26734, libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1))))) != (uintptr)0) {
                                    j = j + 1;
                                } else {
                                    if (libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)(j + (uint32)1)))) == (int32)'u') {
                                        if (j + (uint32)5 >= k) {
                                            r = j + (uint32)1; goto ᒐdone;
                                        }
                                        if (!(_jsonIs4Hex(ref (tls).DerefOrNull(), z + (uintptr)(j + (uint32)2)) != 0)) {
                                            r = j + (uint32)1; goto ᒐdone;
                                        }
                                        j = j + 1;
                                    } else {
                                        if (libc.Int32FromUint8(x) != (int32)JSONB_TEXT5) {
                                            r = j + (uint32)1; goto ᒐdone;
                                        } else {
                                            c = (uint32)0;
                                            szC = _jsonUnescapeOneChar(ref (tls).DerefOrNull(), z + (uintptr)j, k - j, @unsafe.Pointer.FromPinnedBox(Ꮡc));
                                            if (c == (uint32)JSON_INVALID_CHAR) {
                                                r = j + (uint32)1; goto ᒐdone;
                                            }
                                            j = j + (szC - (uint32)1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                j = j + 1;
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_TEXTRAW) { matchᴛ1 = true;
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_ARRAY) { matchᴛ1 = true;
            j = i + n;
            k = j + sz;
            while (j < k) {
                sz = (uint32)0;
                n = _jsonbPayloadSize(ref (tls).DerefOrNull(), pParse, j, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
                if (n == (uint32)0) {
                    r = j + (uint32)1; goto ᒐdone;
                }
                if (j + n + sz > k) {
                    r = j + (uint32)1; goto ᒐdone;
                }
                sub = _jsonbValidityCheck(tls, pParse, j, j + n + sz, iDepth + (uint32)1);
                if (sub != 0) {
                    r = sub; goto ᒐdone;
                }
                j = j + (n + sz);
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (exprᴛ1 == (int32)JSONB_OBJECT) {
            cnt = (uint32)0;
            j = i + n;
            k = j + sz;
            while (j < k) {
                sz = (uint32)0;
                n = _jsonbPayloadSize(ref (tls).DerefOrNull(), pParse, j, @unsafe.Pointer.FromPinnedBox(Ꮡsz));
                if (n == (uint32)0) {
                    r = j + (uint32)1; goto ᒐdone;
                }
                if (j + n + sz > k) {
                    r = j + (uint32)1; goto ᒐdone;
                }
                if ((Tu32)(cnt & (uint32)1) == (uint32)0) {
                    x = libc.Uint8FromInt32(libc.Int32FromUint8(~@unsafe.Pointer.To<Tu8>(__ccgo_ptr(z + (uintptr)j))) & (int32)0x0f);
                    if (libc.Int32FromUint8(x) < (int32)JSONB_TEXT || libc.Int32FromUint8(x) > (int32)JSONB_TEXTRAW) {
                        r = j + (uint32)1; goto ᒐdone;
                    }
                }
                sub1 = _jsonbValidityCheck(tls, pParse, j, j + n + sz, iDepth + (uint32)1);
                if (sub1 != 0) {
                    r = sub1; goto ᒐdone;
                }
                cnt = cnt + 1;
                j = j + (n + sz);
            }
            if ((Tu32)(cnt & (uint32)1) != (uint32)0) {
                r = j + (uint32)1; goto ᒐdone;
            }
            r = (uint32)0; goto ᒐdone;
        }
        if (!matchᴛ1) { /* default: */
            r = i + (uint32)1; goto ᒐdone;
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _loadStat4(ж<libc.TLS> tls, uintptr db, uintptr zDb) {
    uintptr pStat4 = default;
    uintptr v1 = default;
    int32 rc = default;
    bool v2 = default;
    _ = pStat4;
    _ = rc;
    _ = v1;
    _ = v2;
    rc = SQLITE_OK;
    v2 = (Tu32)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FdbOptFlags & libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_Stat4))) == (uint32)0; if (v2) {
        v1 = _sqlite3FindTable(tls, db, __ccgo_ts + 12974, zDb);
        pStat4 = v1;
    }
    if (v2 && v1 != (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pStat4)).Value.FeTabType) == TABTYP_NORM) {
        rc = _loadStatTbl(tls, db, __ccgo_ts + 13200, __ccgo_ts + 13269, zDb);
    }
    return rc;
}

internal static void _logBadConnection(ж<libc.TLS> tls, uintptr zType) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Xsqlite3_log(tls, (int32)SQLITE_MISUSE, __ccgo_ts + 1876, libc.VaList(bp + 8, zType));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _memdbFileControl(ж<libc.TLS> tls, uintptr pFile, int32 op, uintptr pArg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        Tsqlite3_int64 iLimit = default;
        uintptr p = default;
        int32 rc = default;
        _ = iLimit;
        _ = p;
        _ = rc;
        p = (@unsafe.Pointer.To<TMemFile>((@unsafe.Pointer)pFile)).Value.FpStore;
        rc = (int32)SQLITE_NOTFOUND;
        _memdbEnter(tls, p);
        if (op == (int32)SQLITE_FCNTL_VFSNAME) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pArg))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 4160, libc.VaList(bp + 8, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FaData, (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz));
            rc = SQLITE_OK;
        }
        if (op == (int32)SQLITE_FCNTL_SIZE_LIMIT) {
            iLimit = ~@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(pArg));
            if (iLimit < (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz) {
                if (iLimit < 0) {
                    iLimit = (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax;
                } else {
                    iLimit = (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.Fsz;
                }
            }
            (@unsafe.Pointer.To<TMemStore>((@unsafe.Pointer)p)).Value.FszMax = iLimit;
            (@unsafe.Pointer.To<Tsqlite3_int64>(__ccgo_ptr(pArg))).Value = iLimit;
            rc = SQLITE_OK;
        }
        _memdbLeave(tls, p);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _memdbFullPathname(ж<libc.TLS> tls, uintptr pVfs, uintptr zPath, int32 nOut, uintptr zOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        _ = pVfs;
        Xsqlite3_snprintf(tls, nOut, zOut, __ccgo_ts + 3982, libc.VaList(bp + 8, zPath));
        r = SQLITE_OK;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static readonly ж<Tsqlite3_vfs> Ꮡ_memdb_vfs = new StaticBox<Tsqlite3_vfs>(default(Tsqlite3_vfs));
internal static ref Tsqlite3_vfs _memdb_vfs => ref ((StaticBox<Tsqlite3_vfs>)Ꮡ_memdb_vfs).ValueSlot;
internal static void initᴛ_memdb_vfs() { _memdb_vfs = new Tsqlite3_vfs(
    FiVersion: (int32)2,
    FmxPathname: (int32)1024,
    FzName: __ccgo_ts + 4154
); }

internal static int32 /*r*/ _multiSelectValues(ж<libc.TLS> tls, uintptr pParse, uintptr p, @unsafe.Pointer pDest) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 bShowAll = default;
        int32 nRow = default;
        int32 rc = default;
        uintptr v1 = default;
        _ = bShowAll;
        _ = nRow;
        _ = rc;
        _ = v1;
        nRow = (int32)1;
        rc = 0;
        bShowAll = libc.BoolInt32((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpLimit == (uintptr)0);
        for (var cond = true; cond; cond = (int32)1 != 0) {
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpWin != 0) {
                r = -(int32)1; goto ᒐdone;
            }
            if ((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior == (uintptr)0) {
                break;
            }
            p = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpPrior;
            nRow = nRow + bShowAll;
        }
        if (nRow == (int32)1) {
            v1 = __ccgo_ts + 1732;
        } else {
            v1 = __ccgo_ts + 20942;
        }
        _sqlite3VdbeExplain(tls, pParse, (uint8)0, __ccgo_ts + 20944, libc.VaList(bp + 8, nRow, v1));
        while (p != 0) {
            _selectInnerLoop(tls, pParse, p, -(int32)1, nil, nil, pDest, (int32)1, (int32)1);
            if (!(bShowAll != 0)) {
                break;
            }
            (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FnSelectRow = (int16)nRow;
            p = (@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FpNext;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _notValidImpl(ж<libc.TLS> tls, uintptr pParse, uintptr pNC, uintptr zMsg, uintptr pExpr, uintptr pError) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr zIn = default;
        _ = zIn;
        zIn = __ccgo_ts + 7172;
        if (((@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FncFlags & (int32)NC_IdxExpr) != 0) {
            zIn = __ccgo_ts + 7200;
        } else {
            if (((@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FncFlags & (int32)NC_IsCheck) != 0) {
                zIn = __ccgo_ts + 7218;
            } else {
                if (((@unsafe.Pointer.To<TNameContext>((@unsafe.Pointer)pNC)).Value.FncFlags & (int32)NC_GenCol) != 0) {
                    zIn = __ccgo_ts + 7236;
                }
            }
        }
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7254, libc.VaList(bp + 8, zMsg, zIn));
        if (pExpr != 0) {
            (@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fop = (uint8)TK_NULL;
        }
        _sqlite3RecordErrorOffsetOfExpr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pError);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _ntileStepFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nArg, uintptr apArg) {
    uintptr p = default;
    _ = p;
    _ = nArg;
    p = Xsqlite3_aggregate_context(tls, pCtx, (int32)24);
    if (p != 0) {
        if ((@unsafe.Pointer.To<TNtileCtx>((@unsafe.Pointer)p)).Value.FnTotal == 0) {
            (@unsafe.Pointer.To<TNtileCtx>((@unsafe.Pointer)p)).Value.FnParam = Xsqlite3_value_int64(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
            if ((@unsafe.Pointer.To<TNtileCtx>((@unsafe.Pointer)p)).Value.FnParam <= 0) {
                Xsqlite3_result_error(tls, pCtx, __ccgo_ts + 24401, -(int32)1);
            }
        }
        (@unsafe.Pointer.To<TNtileCtx>((@unsafe.Pointer)p)).Value.FnTotal = (@unsafe.Pointer.To<TNtileCtx>((@unsafe.Pointer)p)).Value.FnTotal + 1;
    }
}

internal static readonly ж<int32> Ꮡ_ofdConfirmed = new StaticBox<int32>((int32)0);
internal static ref int32 _ofdConfirmed => ref ((StaticBox<int32>)Ꮡ_ofdConfirmed).ValueSlot;

internal static readonly ж<int32> Ꮡ_ofdDecided = new StaticBox<int32>((int32)0);
internal static ref int32 _ofdDecided => ref ((StaticBox<int32>)Ꮡ_ofdDecided).ValueSlot;

internal static readonly ж<int32> Ꮡ_ofdEnabled = new StaticBox<int32>((int32)0);
internal static ref int32 _ofdEnabled => ref ((StaticBox<int32>)Ꮡ_ofdEnabled).ValueSlot;

internal static readonly ж<int32> Ꮡ_ofdFrozen = new StaticBox<int32>((int32)0);
internal static ref int32 _ofdFrozen => ref ((StaticBox<int32>)Ꮡ_ofdFrozen).ValueSlot;

internal static readonly ж<int32> Ꮡ_ofdSupported = new StaticBox<int32>((int32)1);
internal static ref int32 _ofdSupported => ref ((StaticBox<int32>)Ꮡ_ofdSupported).ValueSlot;

internal static int32 /*r*/ _osFcntlOfd(ж<libc.TLS> tls, int32 fd, int32 cmd, uintptr arg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 ofdCmd = default;
        int32 rc = default;
        uintptr pLock = default;
        _ = ofdCmd;
        _ = pLock;
        _ = rc;
        if (!(libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdFrozen) != 0)) {
            libc.AtomicStorePInt32((uintptr)Ꮡ_ofdFrozen, (int32)1);
        }
        if (libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdEnabled) != 0 && libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdSupported) != 0) {
            ofdCmd = cmd;
            if (cmd == (int32)F_SETLK) {
                ofdCmd = (int32)F_OFD_SETLK;
            } else {
                if (cmd == (int32)F_SETLKW) {
                    ofdCmd = (int32)F_OFD_SETLKW;
                } else {
                    if (cmd == (int32)F_GETLK) {
                        ofdCmd = (int32)F_OFD_GETLK;
                    }
                }
            }
            if (ofdCmd != cmd) {
                pLock = arg;
                (@unsafe.Pointer.To<Tflock>((@unsafe.Pointer)pLock)).Value.Fl_pid = 0;
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32, uintptr, int32>>(_aSyscall[(int32)7].FpCurrent)(tls, fd, ofdCmd, libc.VaList(bp + 8, arg));
                if (rc != -(int32)1) {
                    libc.AtomicStorePInt32((uintptr)Ꮡ_ofdConfirmed, (int32)1);
                    r = rc; goto ᒐdone;
                }
                if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))) != (int32)EINVAL || libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdConfirmed) != 0) {
                    r = rc; goto ᒐdone;
                }
                libc.AtomicStorePInt32((uintptr)Ꮡ_ofdSupported, 0);
            }
        }
        r = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32, uintptr, int32>>(_aSyscall[(int32)7].FpCurrent)(tls, fd, cmd, libc.VaList(bp + 8, arg));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static uintptr /*r*/ _parserAddExprIdListTerm(ж<libc.TLS> tls, uintptr pParse, uintptr pPrior, uintptr pIdToken, int32 hasCollate, int32 sortOrder) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr p = default;
        _ = p;
        p = _sqlite3ExprListAppend(tls, pParse, pPrior, (uintptr)0);
        if ((hasCollate != 0 || sortOrder != -(int32)1) && libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Finit1.Fbusy) == 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 25111, libc.VaList(bp + 8, (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pIdToken)).Value.Fn, (@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pIdToken)).Value.Fz));
        }
        _sqlite3ExprListSetName(tls, pParse, p, (@unsafe.Pointer)pIdToken, (int32)1);
        r = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _parserSyntaxError(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24996, libc.VaList(bp + 8, p));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static array<uintptr> _pragCName;
internal static void initᴛ_pragCName() { _pragCName = new array<uintptr>(57){
    [0] = __ccgo_ts + 5734,
    [1] = __ccgo_ts + 17893,
    [2] = __ccgo_ts + 9451,
    [3] = __ccgo_ts + 17897,
    [4] = __ccgo_ts + 17902,
    [5] = __ccgo_ts + 17905,
    [6] = __ccgo_ts + 17915,
    [7] = __ccgo_ts + 17925,
    [8] = __ccgo_ts + 17931,
    [9] = __ccgo_ts + 17935,
    [10] = __ccgo_ts + 17940,
    [11] = __ccgo_ts + 17945,
    [12] = __ccgo_ts + 17953,
    [13] = __ccgo_ts + 17964,
    [14] = __ccgo_ts + 17967,
    [15] = __ccgo_ts + 17935,
    [16] = __ccgo_ts + 17974,
    [17] = __ccgo_ts + 17940,
    [18] = __ccgo_ts + 17982,
    [19] = __ccgo_ts + 17986,
    [20] = __ccgo_ts + 17991,
    [21] = __ccgo_ts + 17997,
    [22] = __ccgo_ts + 17935,
    [23] = __ccgo_ts + 17940,
    [24] = __ccgo_ts + 18004,
    [25] = __ccgo_ts + 18009,
    [26] = __ccgo_ts + 18012,
    [27] = __ccgo_ts + 18019,
    [28] = __ccgo_ts + 17931,
    [29] = __ccgo_ts + 17935,
    [30] = __ccgo_ts + 18025,
    [31] = __ccgo_ts + 18030,
    [32] = __ccgo_ts + 18035,
    [33] = __ccgo_ts + 17893,
    [34] = __ccgo_ts + 17935,
    [35] = __ccgo_ts + 18039,
    [36] = __ccgo_ts + 18046,
    [37] = __ccgo_ts + 18053,
    [38] = __ccgo_ts + 13163,
    [39] = __ccgo_ts + 13159,
    [40] = __ccgo_ts + 18061,
    [41] = __ccgo_ts + 18066,
    [42] = __ccgo_ts + 18071,
    [43] = __ccgo_ts + 9451,
    [44] = __ccgo_ts + 18076,
    [45] = __ccgo_ts + 5737,
    [46] = __ccgo_ts + 18082,
    [47] = __ccgo_ts + 18087,
    [48] = __ccgo_ts + 17278,
    [49] = __ccgo_ts + 18092,
    [50] = __ccgo_ts + 17893,
    [51] = __ccgo_ts + 17935,
    [52] = __ccgo_ts + 18105,
    [53] = __ccgo_ts + 18110,
    [54] = __ccgo_ts + 18119,
    [55] = __ccgo_ts + 18126,
    [56] = __ccgo_ts + 18137
}; }

internal static void _pragmaFunclistLine(ж<libc.TLS> tls, uintptr v, uintptr p, int32 isBuiltin, int32 showInternFuncs) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        Tu32 mask = default;
        uintptr zType = default;
        _ = mask;
        _ = zType;
        mask = libc.Uint32FromInt32(libc.Int32FromInt32(SQLITE_DETERMINISTIC) | libc.Int32FromInt32(SQLITE_DIRECTONLY) | libc.Int32FromInt32(SQLITE_SUBTYPE) | libc.Int32FromInt32(SQLITE_INNOCUOUS) | libc.Int32FromInt32(SQLITE_FUNC_INTERNAL));
        if (showInternFuncs != 0) {
            mask = (uint32)0xffffffffU;
        }
        for (;;) {
            if (!(p != 0)) {
                break;
            }
            if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxSFunc == (uintptr)0) {
                goto _1;
            }
            if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags & (uint32)SQLITE_FUNC_INTERNAL) != (uint32)0 && showInternFuncs == 0) {
                goto _1;
            }
            if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxValue != (uintptr)0) {
                zType = __ccgo_ts + 19316;
            } else {
                if ((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FxFinalize != (uintptr)0) {
                    zType = __ccgo_ts + 19318;
                } else {
                    zType = __ccgo_ts + 7983;
                }
            }
            _sqlite3VdbeMultiLoad(tls, v, (int32)1, __ccgo_ts + 19320, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FzName, isBuiltin, zType, _azEnc[(Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags & (uint32)SQLITE_FUNC_ENCMASK)], (int32)(@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FnArg, (Tu32)((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FfuncFlags & mask) ^ (uint32)SQLITE_INNOCUOUS)));
            goto _1;
_1:;
            p = (@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)p)).Value.FpNext;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _prepareAndCollectError(ж<libc.TLS> tls, uintptr db, @unsafe.Pointer ppStmt, @unsafe.Pointer pzErrmsg, uintptr zSql) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        _ = rc;
        rc = _sqlite3_prepare_v2__ccgo_gp(tls, db, zSql, -(int32)1, ppStmt, nil);
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<uintptr>(pzErrmsg)).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, Xsqlite3_errmsg(tls, db)));
            (@unsafe.Pointer.To<uintptr>(ppStmt)).Value = (uintptr)0;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _pthreadMutexEnd(ж<libc.TLS> tls) {
    return SQLITE_OK;
}

internal static void _pthreadMutexEnter(ж<libc.TLS> tls, uintptr p) {
    libc.Xpthread_mutex_lock(tls, p);
}

internal static void _pthreadMutexFree(ж<libc.TLS> tls, uintptr p) {
    libc.Xpthread_mutex_destroy(tls, p);
    Xsqlite3_free(tls, p);
}

internal static int32 /*r*/ _pthreadMutexInit(ж<libc.TLS> tls) {
    return SQLITE_OK;
}

internal static void _pthreadMutexLeave(ж<libc.TLS> tls, uintptr p) {
    libc.Xpthread_mutex_unlock(tls, p);
}

internal static int32 /*r*/ _pthreadMutexTry(ж<libc.TLS> tls, uintptr p) {
    int32 rc = default;
    _ = rc;
    if (libc.Xpthread_mutex_trylock(tls, p) == 0) {
        rc = SQLITE_OK;
    } else {
        rc = (int32)SQLITE_BUSY;
    }
    return rc;
}

internal static readonly ж<int32> Ꮡ_randomnessPid = new StaticBox<int32>((int32)0);
internal static ref int32 _randomnessPid => ref ((StaticBox<int32>)Ꮡ_randomnessPid).ValueSlot;

internal static void _rbuBadControlError(ж<libc.TLS> tls, uintptr p) {
    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_ERROR;
    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzErrmsg = Xsqlite3_mprintf(tls, __ccgo_ts + 31967, 0);
}

internal static void _rbuDeleteOalFile(ж<libc.TLS> tls, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr zOal = default;
        uintptr _ᴛ1 = default;
        _ = zOal;
        zOal = _rbuMPrintf(tls, p, __ccgo_ts + 33973, libc.VaList(bp + 16, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzTarget));
        if (zOal != 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
            Xsqlite3_file_control(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbMain, __ccgo_ts + 6890, (int32)SQLITE_FCNTL_VFS_POINTER, bp);
            GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_vfs>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FxDelete)(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), zOal, 0);
            Xsqlite3_free(tls, zOal);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _rbuExclusiveCheckpoint(ж<libc.TLS> tls, uintptr db) {
    uintptr zUri = default;
    _ = zUri;
    zUri = Xsqlite3_db_filename(tls, db, (uintptr)0);
    return Xsqlite3_uri_boolean(tls, zUri, __ccgo_ts + 33948, 0);
}

internal static void _rbuFinalize(ж<libc.TLS> tls, uintptr p, uintptr pStmt) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        int32 rc = default;
        _ = db;
        _ = rc;
        db = Xsqlite3_db_handle(tls, pStmt);
        rc = Xsqlite3_finalize(tls, pStmt);
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && rc != SQLITE_OK) {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = rc;
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzErrmsg = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, Xsqlite3_errmsg(tls, db)));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _rbuIndexCntFunc(ж<libc.TLS> tls, uintptr pCtx, int32 nVal, uintptr apVal) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        ref var pStmt = ref heap(new uintptr(), out var ᏑpStmt);
        ref var zErrmsg = ref heap(new uintptr(), out var ᏑzErrmsg);
        uintptr v1 = default;
        int32 nIndex = default;
        int32 rc = default;
        _ = db;
        _ = nIndex;
        _ = p;
        _ = pStmt;
        _ = rc;
        _ = zErrmsg;
        _ = v1;
        p = Xsqlite3_user_data(tls, pCtx);
        pStmt = (uintptr)0;
        zErrmsg = (uintptr)0;
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzTarget == (uintptr)0) {
            v1 = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbRbu;
        } else {
            v1 = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbMain;
        }
        db = v1;
        _ = nVal;
        rc = _prepareFreeAndCollectError(tls, db, @unsafe.Pointer.FromBox(ᏑpStmt), @unsafe.Pointer.FromBox(ᏑzErrmsg), Xsqlite3_mprintf(tls, __ccgo_ts + 34562, libc.VaList(bp + 8, Xsqlite3_value_text(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apVal))))));
        if (rc != SQLITE_OK) {
            Xsqlite3_result_error(tls, pCtx, zErrmsg, -(int32)1);
        } else {
            nIndex = 0;
            if ((int32)SQLITE_ROW == Xsqlite3_step(tls, pStmt)) {
                nIndex = Xsqlite3_column_int(tls, pStmt, 0);
            }
            rc = Xsqlite3_finalize(tls, pStmt);
            if (rc == SQLITE_OK) {
                Xsqlite3_result_int(tls, pCtx, nIndex);
            } else {
                Xsqlite3_result_error(tls, pCtx, Xsqlite3_errmsg(tls, db), -(int32)1);
            }
        }
        Xsqlite3_free(tls, zErrmsg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _rbuLockDatabase(ж<libc.TLS> tls, uintptr db) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
        Xsqlite3_file_control(tls, db, __ccgo_ts + 6890, (int32)RBU_ZIPVFS_CTRL_FILE_POINTER, bp);
        if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)) != 0) {
            Xsqlite3_file_control(tls, db, __ccgo_ts + 6890, (int32)SQLITE_FCNTL_FILE_POINTER, bp);
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpMethods)).Value.FxLock)(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), (int32)SQLITE_LOCK_SHARED);
            if (rc == SQLITE_OK) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpMethods)).Value.FxUnlock)(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), SQLITE_LOCK_NONE);
            }
            Xsqlite3_file_control(tls, db, __ccgo_ts + 6890, (int32)RBU_ZIPVFS_CTRL_FILE_POINTER, bp);
        } else {
            Xsqlite3_file_control(tls, db, __ccgo_ts + 6890, (int32)SQLITE_FCNTL_FILE_POINTER, bp);
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpMethods != 0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpMethods)).Value.FxLock)(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), (int32)SQLITE_LOCK_SHARED);
            if (rc == SQLITE_OK) {
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))))).Value.FpMethods)).Value.FxLock)(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp)), (int32)SQLITE_LOCK_EXCLUSIVE);
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _rbuOpenDbhandle(ж<libc.TLS> tls, uintptr p, uintptr zName, int32 bUseVfs) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var db = ref heap(new uintptr(), out var Ꮡdb);
        uintptr v1 = default;
        int32 flags = default;
        _ = db;
        _ = flags;
        _ = v1;
        db = (uintptr)0;
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            flags = libc.Int32FromInt32(SQLITE_OPEN_READWRITE) | libc.Int32FromInt32(SQLITE_OPEN_CREATE) | libc.Int32FromInt32(SQLITE_OPEN_URI);
            if (bUseVfs != 0) {
                v1 = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzVfsName;
            } else {
                v1 = (uintptr)0;
            }
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = _sqlite3_open_v2__ccgo_gp(tls, zName, @unsafe.Pointer.FromBox(Ꮡdb), flags, v1);
            if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc != 0) {
                (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FzErrmsg = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, Xsqlite3_errmsg(tls, db)));
                Xsqlite3_close(tls, db);
                db = (uintptr)0;
            }
        }
        r = db;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _rbuSetupCheckpoint(ж<libc.TLS> tls, uintptr p, uintptr pState) {
    int32 nSectorSize = default;
    int32 rc2 = default;
    int32 v1 = default;
    uintptr pDb = default;
    uintptr pWal = default;
    _ = nSectorSize;
    _ = pDb;
    _ = pWal;
    _ = rc2;
    _ = v1;
    if (pState == (uintptr)0) {
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage = 0;
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbMain, __ccgo_ts + 33867, (uintptr)0, (uintptr)0, nil);
        }
    }
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage = (int32)RBU_STAGE_CAPTURE;
        rc2 = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FdbMain, __ccgo_ts + 33913, (uintptr)0, (uintptr)0, nil);
        if (rc2 != (int32)SQLITE_NOTICE) {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = rc2;
        }
    }
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK && (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FnFrame > 0) {
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage = (int32)RBU_STAGE_CKPT;
        if (pState != 0) {
            v1 = (@unsafe.Pointer.To<TRbuState>((@unsafe.Pointer)pState)).Value.FnRow;
        } else {
            v1 = 0;
        }
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FnStep = v1;
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FaBuf = _rbuMalloc(tls, p, (int64)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz);
        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FiWalCksum = _rbuShmChecksum(tls, p);
    }
    if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc == SQLITE_OK) {
        if ((@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FnFrame == 0 || pState != 0 && (@unsafe.Pointer.To<TRbuState>((@unsafe.Pointer)pState)).Value.FiWalCksum != (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FiWalCksum) {
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = (int32)SQLITE_DONE;
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FeStage = (int32)RBU_STAGE_DONE;
        } else {
            pDb = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FpTargetFd)).Value.FpReal;
            pWal = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FpTargetFd)).Value.FpWalFd)).Value.FpReal;
            nSectorSize = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pDb)).Value.FpMethods)).Value.FxSectorSize)(tls, pDb);
            if (nSectorSize > (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz) {
                (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FnPagePerSector = quo(nSectorSize, (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Fpgsz);
            } else {
                (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.FnPagePerSector = (int32)1;
            }
            (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)p)).Value.Frc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, int32>>((@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)pWal)).Value.FpMethods)).Value.FxSync)(tls, pWal, (int32)SQLITE_SYNC_NORMAL);
        }
    }
}

internal static int32 /*r*/ _rbuVfsFileControl(ж<libc.TLS> tls, uintptr pFile, int32 op, uintptr pArg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr p = default;
        uintptr pRbu = default;
        uintptr pRbu1 = default;
        uintptr pRbuVfs = default;
        ref var xControl = ref heap(new uintptr(), out var ᏑxControl);
        uintptr zIn = default;
        uintptr zOut = default;
        int32 rc = default;
        uintptr _ᴛ1 = default;
        _ = p;
        _ = pRbu;
        _ = pRbu1;
        _ = pRbuVfs;
        _ = rc;
        _ = xControl;
        _ = zIn;
        _ = zOut;
        p = pFile;
        xControl = (@unsafe.Pointer.To<Tsqlite3_io_methods>((@unsafe.Pointer)(@unsafe.Pointer.To<Tsqlite3_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal)).Value.FpMethods)).Value.FxFileControl;
        if (op == (int32)SQLITE_FCNTL_RBU) {
            pRbu = pArg;
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>(xControl)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, op, pArg);
            if (rc == (int32)SQLITE_NOTFOUND) {
                (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(bp))).Value = (uintptr)0;
                rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>(xControl)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, (int32)SQLITE_FCNTL_ZIPVFS, bp);
                if (rc == SQLITE_OK) {
                    rc = (int32)SQLITE_ERROR;
                    (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FzErrmsg = Xsqlite3_mprintf(tls, __ccgo_ts + 34932, 0);
                } else {
                    if (rc == (int32)SQLITE_NOTFOUND) {
                        (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu)).Value.FpTargetFd = p;
                        (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbu = pRbu;
                        _rbuMainlistAdd(tls, p);
                        if ((@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpWalFd != 0) {
                            (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)(@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpWalFd)).Value.FpRbu = pRbu;
                        }
                        rc = SQLITE_OK;
                    }
                }
            }
            r = rc; goto ᒐdone;
        } else {
            if (op == (int32)SQLITE_FCNTL_RBUCNT) {
                pRbu1 = pArg;
                (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu1)).Value.FnRbu = (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu1)).Value.FnRbu + 1;
                (@unsafe.Pointer.To<Tsqlite3rbu>((@unsafe.Pointer)pRbu1)).Value.FpRbuFd = p;
                (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FbNolock = (uint8)1;
            }
        }
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, int32>>(xControl)(tls, (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpReal, op, pArg);
        if (rc == SQLITE_OK && op == (int32)SQLITE_FCNTL_VFSNAME) {
            pRbuVfs = (@unsafe.Pointer.To<Trbu_file>((@unsafe.Pointer)p)).Value.FpRbuVfs;
            zIn = ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pArg));
            zOut = Xsqlite3_mprintf(tls, __ccgo_ts + 34955, libc.VaList(bp + 16, (@unsafe.Pointer.To<Trbu_vfs>((@unsafe.Pointer)pRbuVfs)).Value.Fbase.FzName, zIn));
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pArg))).Value = zOut;
            if (zOut == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _realAuthCheck(ж<libc.TLS> tls, uintptr pParse, int32 code, uintptr zArg1, uintptr zArg2, uintptr zArg3) {
    uintptr db = default;
    int32 rc = default;
    _ = db;
    _ = rc;
    db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
    if (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FeParseMode) != PARSE_MODE_NORMAL) {
        return SQLITE_OK;
    }
    rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32, uintptr, uintptr, uintptr, uintptr, int32>>((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FxAuth)(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpAuthArg, code, zArg1, zArg2, zArg3, (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FzAuthContext);
    if (rc == (int32)SQLITE_DENY) {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13786, 0);
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_AUTH;
    } else {
        if (rc != SQLITE_OK && rc != (int32)SQLITE_IGNORE) {
            rc = (int32)SQLITE_DENY;
            _sqliteAuthBadReturnCode(tls, pParse);
        }
    }
    return rc;
}

internal static void _renameFixQuotes(ж<libc.TLS> tls, uintptr pParse, uintptr zDb, int32 bTemp) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        _sqlite3NestedParse(tls, pParse, __ccgo_ts + 9094, libc.VaList(bp + 8, zDb, zDb));
        if (bTemp == 0) {
            _sqlite3NestedParse(tls, pParse, __ccgo_ts + 9241, 0);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _resetAndCollectError(ж<libc.TLS> tls, uintptr pStmt, uintptr pzErrmsg) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        _ = rc;
        rc = Xsqlite3_reset(tls, pStmt);
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErrmsg))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, Xsqlite3_errmsg(tls, Xsqlite3_db_handle(tls, pStmt))));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _resolveOutOfRangeError(ж<libc.TLS> tls, uintptr pParse, uintptr zType, int32 i, int32 mx, uintptr pError) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 7643, libc.VaList(bp + 8, i, zType, mx));
        _sqlite3RecordErrorOffsetOfExpr(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, pError);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _robust_close(ж<libc.TLS> tls, uintptr pFile, int32 h, int32 lineno) {
    uintptr v1 = default;
    _ = v1;
    if (GoFuncCookie.Func<Func<ж<libc.TLS>, int32, int32>>(_aSyscall[(int32)1].FpCurrent)(tls, h) != 0) {
        if (pFile != 0) {
            v1 = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath;
        } else {
            v1 = (uintptr)0;
        }
        _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(16).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3561, v1, lineno);
    }
}

internal static int32 /*r*/ _robust_ftruncate(ж<libc.TLS> tls, int32 h, Tsqlite3_int64 sz) {
    int32 rc = default;
    _ = rc;
    for (var cond = true; cond; cond = rc < 0 && ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))) == (int32)EINTR) {
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, int32, Toff_t, int32>>(_aSyscall[(int32)6].FpCurrent)(tls, h, sz);
    }
    return rc;
}

internal static int32 /*r*/ _rtreeConstraintError(ж<libc.TLS> tls, uintptr pRtree, int32 iCol) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        ref var pStmt = ref heap(new uintptr(), out var ᏑpStmt);
        uintptr zCol = default;
        uintptr zCol1 = default;
        uintptr zCol2 = default;
        uintptr zSql = default;
        int32 rc = default;
        int32 v1 = default;
        _ = pStmt;
        _ = rc;
        _ = zCol;
        _ = zCol1;
        _ = zCol2;
        _ = zSql;
        _ = v1;
        pStmt = (uintptr)0;
        zSql = Xsqlite3_mprintf(tls, __ccgo_ts + 27690, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName));
        if (zSql != 0) {
            rc = _sqlite3_prepare_v2__ccgo_gp(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb, zSql, -(int32)1, @unsafe.Pointer.FromBox(ᏑpStmt), nil);
        } else {
            rc = (int32)SQLITE_NOMEM;
        }
        Xsqlite3_free(tls, zSql);
        if (rc == SQLITE_OK) {
            if (iCol == 0) {
                zCol = Xsqlite3_column_name(tls, pStmt, 0);
                (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fbase.FzErrMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 27710, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, zCol));
            } else {
                zCol1 = Xsqlite3_column_name(tls, pStmt, iCol);
                zCol2 = Xsqlite3_column_name(tls, pStmt, iCol + (int32)1);
                (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fbase.FzErrMsg = Xsqlite3_mprintf(tls, __ccgo_ts + 27742, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, zCol1, zCol2));
            }
        }
        Xsqlite3_finalize(tls, pStmt);
        if (rc == SQLITE_OK) {
            v1 = (int32)SQLITE_CONSTRAINT;
        } else {
            v1 = rc;
        }
        r = v1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _rtreeDestroy(ж<libc.TLS> tls, uintptr pVtab) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr pRtree = default;
        uintptr zCreate = default;
        int32 rc = default;
        _ = pRtree;
        _ = rc;
        _ = zCreate;
        pRtree = pVtab;
        zCreate = Xsqlite3_mprintf(tls, __ccgo_ts + 27594, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName));
        if (!(zCreate != 0)) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            _nodeBlobReset(tls, pRtree);
            rc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb, zCreate, (uintptr)0, (uintptr)0, nil);
            Xsqlite3_free(tls, zCreate);
        }
        if (rc == SQLITE_OK) {
            _rtreeRelease(tls, pRtree);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _rtreeIntegrity(ж<libc.TLS> tls, uintptr pVtab, uintptr zSchema, uintptr zName, int32 isQuick, uintptr pzErr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr pRtree = default;
        int32 rc = default;
        _ = pRtree;
        _ = rc;
        pRtree = pVtab;
        _ = zSchema;
        _ = zName;
        _ = isQuick;
        rc = _rtreeCheckTable(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, (@unsafe.Pointer)pzErr);
        if (rc == SQLITE_OK && ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr)) != 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 29948, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))));
            if (~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr)) == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _rtreeQueryStat1(ж<libc.TLS> tls, uintptr db, uintptr pRtree) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        Ti64 nRow = default;
        ref var p = ref heap(new uintptr(), out var Ꮡp);
        uintptr zFmt = default;
        uintptr zSql = default;
        int32 rc = default;
        int32 v1 = default;
        int64 v2 = default;
        _ = nRow;
        _ = p;
        _ = rc;
        _ = zFmt;
        _ = zSql;
        _ = v1;
        _ = v2;
        zFmt = __ccgo_ts + 27924;
        nRow = (int64)RTREE_MIN_ROWEST;
        rc = _sqlite3_table_column_metadata__ccgo_gp(tls, db, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, __ccgo_ts + 12948, (uintptr)0, (uintptr)0, nil, (uintptr)0, (uintptr)0, (uintptr)0);
        if (rc != SQLITE_OK) {
            (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnRowEst = (int64)RTREE_DEFAULT_ROWEST;
            if (rc == (int32)SQLITE_ERROR) {
                v1 = SQLITE_OK;
            } else {
                v1 = rc;
            }
            r = v1; goto ᒐdone;
        }
        zSql = Xsqlite3_mprintf(tls, zFmt, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName));
        if (zSql == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            rc = _sqlite3_prepare_v2__ccgo_gp(tls, db, zSql, -(int32)1, @unsafe.Pointer.FromBox(Ꮡp), nil);
            if (rc == SQLITE_OK) {
                if (Xsqlite3_step(tls, p) == (int32)SQLITE_ROW) {
                    nRow = Xsqlite3_column_int64(tls, p, 0);
                }
                rc = Xsqlite3_finalize(tls, p);
            }
            Xsqlite3_free(tls, zSql);
        }
        if (nRow > (int64)libc.Int32FromInt32(RTREE_MIN_ROWEST)) {
            v2 = nRow;
        } else {
            v2 = (int64)libc.Int32FromInt32(RTREE_MIN_ROWEST);
        }
        (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FnRowEst = v2;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _rtreeRename(ж<libc.TLS> tls, uintptr pVtab, uintptr zNewName) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        uintptr pRtree = default;
        uintptr zSql = default;
        int32 rc = default;
        _ = pRtree;
        _ = rc;
        _ = zSql;
        pRtree = pVtab;
        rc = (int32)SQLITE_NOMEM;
        zSql = Xsqlite3_mprintf(tls, __ccgo_ts + 27779, libc.VaList(bp + 8, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, zNewName, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, zNewName, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzDb, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.FzName, zNewName));
        if (zSql != 0) {
            _nodeBlobReset(tls, pRtree);
            rc = _sqlite3_exec__ccgo_gp(tls, (@unsafe.Pointer.To<TRtree>((@unsafe.Pointer)pRtree)).Value.Fdb, zSql, (uintptr)0, (uintptr)0, nil);
            Xsqlite3_free(tls, zSql);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _rtreedepth(ж<libc.TLS> tls, uintptr ctx, int32 nArg, uintptr apArg) {
    uintptr zBlob = default;
    _ = zBlob;
    _ = nArg;
    if (Xsqlite3_value_type(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg))) != (int32)SQLITE_BLOB || Xsqlite3_value_bytes(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg))) < (int32)2) {
        Xsqlite3_result_error(tls, ctx, __ccgo_ts + 29203, -(int32)1);
    } else {
        zBlob = Xsqlite3_value_blob(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr(apArg)));
        if (zBlob != 0) {
            Xsqlite3_result_int(tls, ctx, _readInt16(ref (tls).DerefOrNull(), zBlob));
        } else {
            Xsqlite3_result_error_nomem(tls, ctx);
        }
    }
}

internal static readonly ж<Tsqlite3_mutex_methods> Ꮡ_sMutex1 = new StaticBox<Tsqlite3_mutex_methods>(new Tsqlite3_mutex_methods());
internal static ref Tsqlite3_mutex_methods _sMutex1 => ref ((StaticBox<Tsqlite3_mutex_methods>)Ꮡ_sMutex1).ValueSlot;

internal static int32 /*r*/ _sessionApplyOneWithRetry(ж<libc.TLS> tls, uintptr db, uintptr pIter, uintptr pApply, uintptr __ccgo_fp_xConflict, uintptr pCtx) {
    ref var bReplace = ref heap(new int32(), out var ᏑbReplace);
    ref var bRetry = ref heap(new int32(), out var ᏑbRetry);
    int32 rc = default;
    _ = bReplace;
    _ = bRetry;
    _ = rc;
    bReplace = 0;
    bRetry = 0;
    rc = _sessionApplyOneOp(tls, pIter, pApply, __ccgo_fp_xConflict, pCtx, @unsafe.Pointer.FromPinnedBox(ᏑbReplace), @unsafe.Pointer.FromPinnedBox(ᏑbRetry));
    if (rc == SQLITE_OK) {
        if (bRetry != 0) {
            rc = _sessionApplyOneOp(tls, pIter, pApply, __ccgo_fp_xConflict, pCtx, nil, nil);
        } else {
            if (bReplace != 0) {
                rc = _sqlite3_exec__ccgo_gp(tls, db, __ccgo_ts + 36548, (uintptr)0, (uintptr)0, nil);
                if (rc == SQLITE_OK) {
                    rc = _sessionBindRow(tls, pIter, __ccgo_fp(Xsqlite3changeset_new), (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FnCol, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FabPK, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FpDelete);
                    Xsqlite3_bind_int(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FpDelete, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FnCol + (int32)1, (int32)1);
                }
                if (rc == SQLITE_OK) {
                    Xsqlite3_step(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FpDelete);
                    rc = Xsqlite3_reset(tls, (@unsafe.Pointer.To<TSessionApplyCtx>((@unsafe.Pointer)pApply)).Value.FpDelete);
                }
                if (rc == SQLITE_OK) {
                    rc = _sessionApplyOneOp(tls, pIter, pApply, __ccgo_fp_xConflict, pCtx, nil, nil);
                }
                if (rc == SQLITE_OK) {
                    rc = _sqlite3_exec__ccgo_gp(tls, db, __ccgo_ts + 36569, (uintptr)0, (uintptr)0, nil);
                }
            }
        }
    }
    return rc;
}

internal static int32 /*r*/ _sessionDiffFindModified(ж<libc.TLS> tls, uintptr pSession, uintptr pTab, uintptr zFrom, uintptr zExpr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(80);
        defer(tls.Free, (nint)80, ref ᒐ);
        Ti64 iRowid = default;
        uintptr pDiffCtx = default;
        ref var pStmt = ref heap(new uintptr(), out var ᏑpStmt);
        uintptr z1 = default;
        uintptr z2 = default;
        uintptr zExpr2 = default;
        uintptr zStmt = default;
        int32 rc = default;
        int64 v1 = default;
        _ = iRowid;
        _ = pDiffCtx;
        _ = pStmt;
        _ = rc;
        _ = z1;
        _ = z2;
        _ = zExpr2;
        _ = zStmt;
        _ = v1;
        rc = SQLITE_OK;
        zExpr2 = _sessionExprCompareOther(tls, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FzDb, zFrom, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FazCol, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FabPK);
        if (zExpr2 == (uintptr)0) {
            rc = (int32)SQLITE_NOMEM;
        } else {
            z1 = _sessionAllCols(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FzDb, pTab);
            z2 = _sessionAllCols(tls, zFrom, pTab);
            zStmt = Xsqlite3_mprintf(tls, __ccgo_ts + 35771, libc.VaList(bp + 8, z1, z2, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.FzDb, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zFrom, (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FzName, zExpr, zExpr2));
            if (zStmt == (uintptr)0 || z1 == (uintptr)0 || z2 == (uintptr)0) {
                rc = (int32)SQLITE_NOMEM;
            } else {
                rc = _sqlite3_prepare_v2__ccgo_gp(tls, (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fdb, zStmt, -(int32)1, @unsafe.Pointer.FromBox(ᏑpStmt), nil);
                if (rc == SQLITE_OK) {
                    pDiffCtx = (@unsafe.Pointer.To<Tsqlite3_session>((@unsafe.Pointer)pSession)).Value.Fhook.FpCtx;
                    (@unsafe.Pointer.To<TSessionDiffCtx>((@unsafe.Pointer)pDiffCtx)).Value.FpStmt = pStmt;
                    (@unsafe.Pointer.To<TSessionDiffCtx>((@unsafe.Pointer)pDiffCtx)).Value.FnOldOff = (@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FnCol;
                    while ((int32)SQLITE_ROW == Xsqlite3_step(tls, pStmt)) {
                        if ((@unsafe.Pointer.To<TSessionTable>((@unsafe.Pointer)pTab)).Value.FbRowid != 0) {
                            v1 = Xsqlite3_column_int64(tls, pStmt, 0);
                        } else {
                            v1 = 0;
                        }
                        iRowid = v1;
                        _sessionPreupdateOneChange(tls, (int32)SQLITE_UPDATE, iRowid, pSession, pTab);
                    }
                    rc = Xsqlite3_finalize(tls, pStmt);
                }
            }
            Xsqlite3_free(tls, zStmt);
            Xsqlite3_free(tls, z1);
            Xsqlite3_free(tls, z2);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sessionPrepare(ж<libc.TLS> tls, uintptr db, @unsafe.Pointer pp, @unsafe.Pointer pzErrmsg, uintptr zSql) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        _ = rc;
        rc = _sqlite3_prepare_v2__ccgo_gp(tls, db, zSql, -(int32)1, pp, nil);
        if (pzErrmsg != nil && rc != SQLITE_OK) {
            (@unsafe.Pointer.To<uintptr>(pzErrmsg)).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 3982, libc.VaList(bp + 8, Xsqlite3_errmsg(tls, db)));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uintptr /*r*/ _sessionSelectFindNew(ж<libc.TLS> tls, uintptr zDb1, uintptr zDb2, int32 bRowid, uintptr zTbl, uintptr zExpr) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        uintptr zRet = default;
        uintptr zSel = default;
        uintptr v1 = default;
        _ = zRet;
        _ = zSel;
        _ = v1;
        if (bRowid != 0) {
            v1 = __ccgo_ts + 35662;
        } else {
            v1 = __ccgo_ts + 6895;
        }
        zSel = v1;
        zRet = Xsqlite3_mprintf(tls, __ccgo_ts + 35673, libc.VaList(bp + 8, zSel, zDb1, zTbl, zDb2, zTbl, zExpr));
        r = zRet;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3AlterDropConstraint(ж<libc.TLS> tls, uintptr pParse, uintptr pSrc, uintptr pCons, uintptr pCol) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr pTab = default;
        uintptr z = default;
        uintptr zArg = default;
        ref var zDb = ref heap(new uintptr(), out var ᏑzDb);
        ref var iCol = ref heap(new int32(), out var ᏑiCol);
        ref var iDb = ref heap(new int32(), out var ᏑiDb);
        _ = db;
        _ = iCol;
        _ = iDb;
        _ = pTab;
        _ = z;
        _ = zArg;
        _ = zDb;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        pTab = (uintptr)0;
        iDb = 0;
        zDb = (uintptr)0;
        zArg = (uintptr)0;
        pTab = _alterFindTable(tls, pParse, pSrc, @unsafe.Pointer.FromPinnedBox(ᏑiDb), @unsafe.Pointer.FromBox(ᏑzDb), libc.BoolInt32(pCons != (uintptr)0));
        if (!(pTab != 0)) {
            return;
        }
        if (pCons != 0) {
            z = _sqlite3NameFromToken(tls, db, (@unsafe.Pointer)pCons);
            zArg = _sqlite3MPrintf(tls, db, __ccgo_ts + 12043, libc.VaList(bp + 8, z));
            _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, z);
        } else {
            if (_alterFindCol(tls, pParse, pTab, pCol, @unsafe.Pointer.FromPinnedBox(ᏑiCol)) != 0) {
                return;
            }
            zArg = _sqlite3MPrintf(tls, db, __ccgo_ts + 5375, libc.VaList(bp + 8, iCol));
        }
        _sqlite3NestedParse(tls, pParse, __ccgo_ts + 12046, libc.VaList(bp + 8, zDb, zArg, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
        _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, zArg);
        _renameReloadSchema(tls, pParse, iDb, (uint16)INITFLAG_AlterDropCons);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3CantopenError(ж<libc.TLS> tls, int32 lineno) {
    return _sqlite3ReportError(tls, (int32)SQLITE_CANTOPEN, lineno, __ccgo_ts + 26489);
}

internal static int32 /*r*/ _sqlite3Close(ж<libc.TLS> tls, uintptr db, int32 forceZombie) {
    uintptr p = default;
    _ = p;
    if (!(db != 0)) {
        return SQLITE_OK;
    }
    if (!(_sqlite3SafetyCheckSickOrOk(tls, db) != 0)) {
        return _sqlite3MisuseError(tls, (int32)190064);
    }
    Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FmTrace) & (int32)SQLITE_TRACE_CLOSE) != 0) {
        GoFuncCookie.Func<Func<ж<libc.TLS>, Tu32, uintptr, uintptr, uintptr, int32>>(~@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db).of(Tsqlite3.ᏑFtrace).Reinterpret<Tsqlite3ᴛ1_Ftrace, uintptr>())(tls, (uint32)SQLITE_TRACE_CLOSE, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpTraceArg, db, (uintptr)0);
    }
    _disconnectAllVtab(tls, db);
    _sqlite3VtabRollback(tls, db);
    if (!(forceZombie != 0) && _connectionIsBusy(ref (tls).DerefOrNull(), db) != 0) {
        _sqlite3ErrorWithMsg(tls, db, (int32)SQLITE_BUSY, __ccgo_ts + 25457, 0);
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Fmutex);
        return (int32)SQLITE_BUSY;
    }
    while ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData != 0) {
        p = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData;
        (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FpDbData = (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpNext;
        if ((@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FxDestructor != 0) {
            GoFuncCookie.Func<Action<ж<libc.TLS>, uintptr>>((@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FxDestructor)(tls, (@unsafe.Pointer.To<TDbClientData>((@unsafe.Pointer)p)).Value.FpData);
        }
        Xsqlite3_free(tls, p);
    }
    (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FeOpenState = (uint8)SQLITE_STATE_ZOMBIE;
    _sqlite3LeaveMutexAndCloseZombie(tls, db);
    return SQLITE_OK;
}

internal static int32 /*r*/ _sqlite3CorruptError(ж<libc.TLS> tls, int32 lineno) {
    return _sqlite3ReportError(tls, (int32)SQLITE_CORRUPT, lineno, __ccgo_ts + 26462);
}

internal static int32 /*r*/ _sqlite3DbpageRegister(ж<libc.TLS> tls, uintptr db) {
    return Xsqlite3_create_module(tls, db, __ccgo_ts + 35401, (uintptr)Ꮡ_dbpage_module, (uintptr)0);
}

internal static int32 /*r*/ _sqlite3DbstatRegister(ж<libc.TLS> tls, uintptr db) {
    return Xsqlite3_create_module(tls, db, __ccgo_ts + 35216, (uintptr)Ꮡ_dbstat_module, (uintptr)0);
}

internal static uintptr /*r*/ _sqlite3DefaultMutex(ref libc.TLS tls) {
    return (uintptr)Ꮡ_sMutex1;
}

internal static void _sqlite3EndTransaction(ж<libc.TLS> tls, uintptr pParse, int32 eType) {
    int32 isRollback = default;
    uintptr v = default;
    uintptr v1 = default;
    _ = isRollback;
    _ = v;
    _ = v1;
    isRollback = libc.BoolInt32(eType == (int32)TK_ROLLBACK);
    if (isRollback != 0) {
        v1 = __ccgo_ts + 16222;
    } else {
        v1 = __ccgo_ts + 16231;
    }
    if (_sqlite3AuthCheck(tls, pParse, (int32)SQLITE_TRANSACTION, v1, (uintptr)0, (uintptr)0) != 0) {
        return;
    }
    v = _sqlite3GetVdbe(tls, pParse);
    if (v != 0) {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_AutoCommit, (int32)1, isRollback);
    }
}

internal static void _sqlite3ErrorIfNotEmpty(ж<libc.TLS> tls, uintptr pParse, uintptr zDb, uintptr zTab, uintptr zErr) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        _sqlite3NestedParse(tls, pParse, __ccgo_ts + 10325, libc.VaList(bp + 8, zErr, zDb, zTab));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3ExprFunctionUsable(ж<libc.TLS> tls, uintptr pParse, uintptr pExpr, uintptr pDef) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if ((Tu32)((@unsafe.Pointer.To<TExpr>((@unsafe.Pointer)pExpr)).Value.Fflags & libc.Uint32FromInt32(libc.Int32FromInt32(EP_FromDDL))) != (uint32)0 || (libc.Int32FromUint8((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FprepFlags) & (int32)SQLITE_PREPARE_FROM_DDL) != 0) {
            if ((Tu32)((@unsafe.Pointer.To<TFuncDef>((@unsafe.Pointer)pDef)).Value.FfuncFlags & (uint32)SQLITE_FUNC_DIRECT) != (uint32)0 || (Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_TrustedSchema) == (uint64)0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 8113, libc.VaList(bp + 8, pExpr));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _sqlite3ExprOrderByAggregateError(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 8063, libc.VaList(bp + 8, p));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3Fts5ConfigLoad(ж<libc.TLS> tls, uintptr pConfig, int32 iCookie) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        ref var bDummy = ref heap(new int32(), out var ᏑbDummy);
        int32 iVersion = default;
        ref var rc = ref heap(new int32(), out var Ꮡrc);
        ref var p = ref heap(new uintptr(), out var Ꮡp);
        uintptr pVal = default;
        uintptr zK = default;
        uintptr zSelect = default;
        uintptr zSql = default;
        _ = bDummy;
        _ = iVersion;
        _ = p;
        _ = pVal;
        _ = rc;
        _ = zK;
        _ = zSelect;
        _ = zSql;
        zSelect = __ccgo_ts + 38727;
        p = (uintptr)0;
        rc = SQLITE_OK;
        iVersion = 0;
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fpgsz = (int32)FTS5_DEFAULT_PAGE_SIZE;
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnAutomerge = (int32)FTS5_DEFAULT_AUTOMERGE;
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnUsermerge = (int32)FTS5_DEFAULT_USERMERGE;
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCrisisMerge = (int32)FTS5_DEFAULT_CRISISMERGE;
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnHashSize = libc.Int32FromInt32(1024) * libc.Int32FromInt32(1024);
        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnDeleteMerge = (int32)FTS5_DEFAULT_DELETE_AUTOMERGE;
        zSql = _sqlite3Fts5Mprintf(tls, @unsafe.Pointer.FromPinnedBox(Ꮡrc), zSelect, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        if (zSql != 0) {
            rc = _sqlite3_prepare_v2__ccgo_gp(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, zSql, -(int32)1, @unsafe.Pointer.FromBox(Ꮡp), nil);
            Xsqlite3_free(tls, zSql);
        }
        if (rc == SQLITE_OK) {
            while ((int32)SQLITE_ROW == Xsqlite3_step(tls, p)) {
                zK = Xsqlite3_column_text(tls, p, 0);
                pVal = Xsqlite3_column_value(tls, p, (int32)1);
                if (0 == Xsqlite3_stricmp(tls, zK, __ccgo_ts + 38759)) {
                    iVersion = Xsqlite3_value_int(tls, pVal);
                } else {
                    bDummy = 0;
                    _sqlite3Fts5ConfigSetValue(tls, pConfig, zK, pVal, @unsafe.Pointer.FromPinnedBox(ᏑbDummy));
                }
            }
            rc = Xsqlite3_finalize(tls, p);
        }
        if (rc == SQLITE_OK && iVersion != (int32)FTS5_CURRENT_VERSION && iVersion != (int32)FTS5_CURRENT_VERSION_SECUREDELETE) {
            rc = (int32)SQLITE_ERROR;
            _sqlite3Fts5ConfigErrmsg(tls, pConfig, __ccgo_ts + 38767, libc.VaList(bp + 8, iVersion, (int32)FTS5_CURRENT_VERSION, (int32)FTS5_CURRENT_VERSION_SECUREDELETE));
        } else {
            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FiVersion = iVersion;
        }
        if (rc == SQLITE_OK) {
            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FiCookie = iCookie;
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5ConfigSetValue(ж<libc.TLS> tls, uintptr pConfig, uintptr zKey, uintptr pVal, @unsafe.Pointer pbBadkey) {
    int32 bVal = default;
    int32 bVal1 = default;
    int32 nAutomerge = default;
    int32 nCrisisMerge = default;
    int32 nHashSize = default;
    int32 nUsermerge = default;
    int32 nVal = default;
    int32 pgsz = default;
    int32 rc = default;
    int32 v1 = default;
    uintptr zIn = default;
    ref var zRank = ref heap(new uintptr(), out var ᏑzRank);
    ref var zRankArgs = ref heap(new uintptr(), out var ᏑzRankArgs);
    _ = bVal;
    _ = bVal1;
    _ = nAutomerge;
    _ = nCrisisMerge;
    _ = nHashSize;
    _ = nUsermerge;
    _ = nVal;
    _ = pgsz;
    _ = rc;
    _ = zIn;
    _ = zRank;
    _ = zRankArgs;
    _ = v1;
    rc = SQLITE_OK;
    if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38645)) {
        pgsz = 0;
        if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
            pgsz = Xsqlite3_value_int(tls, pVal);
        }
        if (pgsz < (int32)32 || pgsz > libc.Int32FromInt32(64) * libc.Int32FromInt32(1024)) {
            (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
        } else {
            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fpgsz = pgsz;
        }
    } else {
        if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38650)) {
            nHashSize = -(int32)1;
            if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                nHashSize = Xsqlite3_value_int(tls, pVal);
            }
            if (nHashSize <= 0) {
                (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
            } else {
                (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnHashSize = nHashSize;
            }
        } else {
            if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38659)) {
                nAutomerge = -(int32)1;
                if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                    nAutomerge = Xsqlite3_value_int(tls, pVal);
                }
                if (nAutomerge < 0 || nAutomerge > (int32)64) {
                    (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                } else {
                    if (nAutomerge == (int32)1) {
                        nAutomerge = (int32)FTS5_DEFAULT_AUTOMERGE;
                    }
                    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnAutomerge = nAutomerge;
                }
            } else {
                if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38669)) {
                    nUsermerge = -(int32)1;
                    if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                        nUsermerge = Xsqlite3_value_int(tls, pVal);
                    }
                    if (nUsermerge < (int32)2 || nUsermerge > (int32)16) {
                        (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                    } else {
                        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnUsermerge = nUsermerge;
                    }
                } else {
                    if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38679)) {
                        nCrisisMerge = -(int32)1;
                        if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                            nCrisisMerge = Xsqlite3_value_int(tls, pVal);
                        }
                        if (nCrisisMerge < 0) {
                            (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                        } else {
                            if (nCrisisMerge <= (int32)1) {
                                nCrisisMerge = (int32)FTS5_DEFAULT_CRISISMERGE;
                            }
                            if (nCrisisMerge >= (int32)FTS5_MAX_SEGMENT) {
                                nCrisisMerge = libc.Int32FromInt32(FTS5_MAX_SEGMENT) - libc.Int32FromInt32(1);
                            }
                            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnCrisisMerge = nCrisisMerge;
                        }
                    } else {
                        if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38691)) {
                            nVal = -(int32)1;
                            if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                                nVal = Xsqlite3_value_int(tls, pVal);
                            } else {
                                (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                            }
                            if (nVal < 0) {
                                nVal = (int32)FTS5_DEFAULT_DELETE_AUTOMERGE;
                            }
                            if (nVal > (int32)100) {
                                nVal = 0;
                            }
                            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FnDeleteMerge = nVal;
                        } else {
                            if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38259)) {
                                zIn = Xsqlite3_value_text(tls, pVal);
                                rc = _sqlite3Fts5ConfigParseRank(tls, zIn, @unsafe.Pointer.FromBox(ᏑzRank), @unsafe.Pointer.FromBox(ᏑzRankArgs));
                                if (rc == SQLITE_OK) {
                                    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzRank);
                                    Xsqlite3_free(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzRankArgs);
                                    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzRank = zRank;
                                    (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzRankArgs = zRankArgs;
                                } else {
                                    if (rc == (int32)SQLITE_ERROR) {
                                        rc = SQLITE_OK;
                                        (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                                    }
                                }
                            } else {
                                if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38703)) {
                                    bVal = -(int32)1;
                                    if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                                        bVal = Xsqlite3_value_int(tls, pVal);
                                    }
                                    if (bVal < 0) {
                                        (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                                    } else {
                                        if (bVal != 0) {
                                            v1 = (int32)1;
                                        } else {
                                            v1 = 0;
                                        }
                                        (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbSecureDelete = v1;
                                    }
                                } else {
                                    if (0 == Xsqlite3_stricmp(tls, zKey, __ccgo_ts + 38717)) {
                                        bVal1 = -(int32)1;
                                        if ((int32)SQLITE_INTEGER == Xsqlite3_value_numeric_type(tls, pVal)) {
                                            bVal1 = Xsqlite3_value_int(tls, pVal);
                                        }
                                        if (bVal1 < 0) {
                                            (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                                        } else {
                                            if (bVal1 != 0) {
                                                v1 = (int32)1;
                                            } else {
                                                v1 = 0;
                                            }
                                            (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbPrefixInsttoken = v1;
                                        }
                                    } else {
                                        (@unsafe.Pointer.To<int32>(pbBadkey)).Value = (int32)1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5CreateTable(ж<libc.TLS> tls, uintptr pConfig, uintptr zPost, uintptr zDefn, int32 bWithout, uintptr pzErr) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 rc = default;
        ref var zErr = ref heap(new uintptr(), out var ᏑzErr);
        uintptr v1 = default;
        _ = rc;
        _ = zErr;
        _ = v1;
        zErr = (uintptr)0;
        if (bWithout != 0) {
            v1 = __ccgo_ts + 32304;
        } else {
            v1 = __ccgo_ts + 1732;
        }
        rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, @unsafe.Pointer.FromBox(ᏑzErr), __ccgo_ts + 41779, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, zPost, zDefn, v1));
        if (zErr != 0) {
            (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(pzErr))).Value = Xsqlite3_mprintf(tls, __ccgo_ts + 41809, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, zPost, zErr));
            Xsqlite3_free(tls, zErr);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5DropAll(ж<libc.TLS> tls, uintptr pConfig) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(64);
        defer(tls.Free, (nint)64, ref ᒐ);
        int32 rc = default;
        _ = rc;
        rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 41557, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbColumnsize != 0) {
            rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 41661, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == FTS5_CONTENT_NORMAL) {
            rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 41699, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5IndexSetCookie(ж<libc.TLS> tls, uintptr p, int32 iNew) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        ref var pBlob = ref heap(new uintptr(), out var ᏑpBlob);
        uintptr pConfig = default;
        int32 rc = default;
        array<Tu8> _ᴛ1 = new(4);
        _ = pBlob;
        _ = pConfig;
        _ = rc;
        pConfig = (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FpConfig;
        pBlob = (uintptr)0;
        _sqlite3Fts5Put32(ref (tls).DerefOrNull(), __ccgo_ptr(bp), iNew);
        rc = _sqlite3_blob_open__ccgo_gp(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Index>((@unsafe.Pointer)p)).Value.FzDataTbl, __ccgo_ts + 39233, (int64)FTS5_STRUCTURE_ROWID, (int32)1, @unsafe.Pointer.FromBox(ᏑpBlob));
        if (rc == SQLITE_OK) {
            Xsqlite3_blob_write(tls, pBlob, bp, (int32)4, 0);
            rc = Xsqlite3_blob_close(tls, pBlob);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3Fts5ParseSetColset(ж<libc.TLS> tls, @unsafe.Pointer pParse, uintptr pExpr, uintptr pColset) {
    ref var pFree = ref heap(new uintptr(), out var ᏑpFree);
    _ = pFree;
    pFree = pColset;
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)(~@unsafe.Pointer.To<uintptr>(pParse)))).Value.FeDetail == (int32)FTS5_DETAIL_NONE) {
        _sqlite3Fts5ParseError(tls, pParse, __ccgo_ts + 38930, 0);
    } else {
        _fts5ParseSetColset(tls, pParse, pExpr, pColset, @unsafe.Pointer.FromBox(ᏑpFree));
    }
    Xsqlite3_free(tls, pFree);
}

internal static int32 /*r*/ _sqlite3Fts5ParserFallback(ж<libc.TLS> tls, int32 iToken) {
    _ = iToken;
    return 0;
}

internal static int32 /*r*/ _sqlite3Fts5StorageDeleteAll(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        uintptr pConfig = default;
        int32 rc = default;
        _ = pConfig;
        _ = rc;
        pConfig = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpConfig;
        (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FbTotalsValid = 0;
        rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 41985, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbColumnsize != 0) {
            rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 42035, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        }
        if (rc == SQLITE_OK && (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == (int32)FTS5_CONTENT_UNINDEXED) {
            rc = _fts5ExecPrintf(tls, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.Fdb, nil, __ccgo_ts + 42064, libc.VaList(bp + 8, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzDb, (@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FzName));
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5IndexReinit(tls, (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)p)).Value.FpIndex);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3Fts5StorageConfigValue(tls, p, __ccgo_ts + 38759, (uintptr)0, (int32)FTS5_CURRENT_VERSION);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3Fts5StorageRename(ж<libc.TLS> tls, uintptr pStorage, uintptr zName) {
    uintptr pConfig = default;
    ref var rc = ref heap(new int32(), out var Ꮡrc);
    _ = pConfig;
    _ = rc;
    pConfig = (@unsafe.Pointer.To<TFts5Storage>((@unsafe.Pointer)pStorage)).Value.FpConfig;
    rc = _sqlite3Fts5StorageSync(tls, pStorage);
    _fts5StorageRenameOne(tls, pConfig, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ts + 27589, zName);
    _fts5StorageRenameOne(tls, pConfig, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ts + 13159, zName);
    _fts5StorageRenameOne(tls, pConfig, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ts + 40891, zName);
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FbColumnsize != 0) {
        _fts5StorageRenameOne(tls, pConfig, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ts + 38577, zName);
    }
    if ((@unsafe.Pointer.To<TFts5Config>((@unsafe.Pointer)pConfig)).Value.FeContent == FTS5_CONTENT_NORMAL) {
        _fts5StorageRenameOne(tls, pConfig, @unsafe.Pointer.FromPinnedBox(Ꮡrc), __ccgo_ts + 37882, zName);
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3Fts5TokenizerPreload(ж<libc.TLS> tls, uintptr p) {
    return libc.BoolInt32((@unsafe.Pointer.To<TFts5TokenizerConfig>((@unsafe.Pointer)p)).Value.FnArg >= (int32)1 && 0 == Xsqlite3_stricmp(tls, ~@unsafe.Pointer.To<uintptr>(__ccgo_ptr((@unsafe.Pointer.To<TFts5TokenizerConfig>((@unsafe.Pointer)p)).Value.FazArg)), __ccgo_ts + 42496));
}

internal static int32 /*r*/ _sqlite3Fts5VocabInit(ж<libc.TLS> tls, uintptr pGlobal, uintptr db) {
    uintptr p = default;
    _ = p;
    p = pGlobal;
    return Xsqlite3_create_module_v2(tls, db, __ccgo_ts + 42827, (uintptr)Ꮡ_fts5Vocab, p, (uintptr)0);
}

internal static uintptr /*r*/ _sqlite3GetCollSeq(ж<libc.TLS> tls, uintptr pParse, Tu8 enc, uintptr pColl, uintptr zName) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        uintptr p = default;
        _ = db;
        _ = p;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        p = pColl;
        if (!(p != 0)) {
            p = _sqlite3FindCollSeq(tls, db, enc, zName, 0);
        }
        if (!(p != 0) || !((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.FxCmp != 0)) {
            _callCollNeeded(tls, db, libc.Int32FromUint8(enc), zName);
            p = _sqlite3FindCollSeq(tls, db, enc, zName, 0);
        }
        if (p != 0 && !((@unsafe.Pointer.To<TCollSeq>((@unsafe.Pointer)p)).Value.FxCmp != 0) && _synthCollSeq(tls, db, p) != 0) {
            p = (uintptr)0;
        }
        if (p == (uintptr)0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16427, libc.VaList(bp + 8, zName));
            (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = libc.Int32FromInt32(SQLITE_ERROR) | libc.Int32FromInt32(1).Lsh((int64)(libc.Int32FromInt32(8)));
        }
        r = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _sqlite3InvalidFunction(ж<libc.TLS> tls, uintptr context, int32 NotUsed, uintptr NotUsed2) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr zErr = default;
        uintptr zName = default;
        _ = zErr;
        _ = zName;
        zName = Xsqlite3_user_data(tls, context);
        _ = NotUsed;
        _ = NotUsed2;
        zErr = Xsqlite3_mprintf(tls, __ccgo_ts + 26144, libc.VaList(bp + 8, zName));
        Xsqlite3_result_error(tls, context, zErr, -(int32)1);
        Xsqlite3_free(tls, zErr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3IsReadOnly(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, uintptr pTrigger) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if (_tabIsReadOnly(tls, pParse, pTab) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16491, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
            r = (int32)1; goto ᒐdone;
        }
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VIEW && (pTrigger == (uintptr)0 || (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FbReturning != 0 && (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)pTrigger)).Value.FpNext == (uintptr)0)) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 16520, libc.VaList(bp + 8, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName));
            r = (int32)1; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _sqlite3IsRowid(ref libc.TLS tls, uintptr z) {
    if (_sqlite3StrICmp(ref tls, z, __ccgo_ts + 8263) == 0) {
        return (int32)1;
    }
    if (_sqlite3StrICmp(ref tls, z, __ccgo_ts + 8271) == 0) {
        return (int32)1;
    }
    if (_sqlite3StrICmp(ref tls, z, __ccgo_ts + 8277) == 0) {
        return (int32)1;
    }
    return 0;
}

internal static Tu32 /*r*/ _sqlite3IsTrueOrFalse(ref libc.TLS tls, uintptr zIn) {
    if (_sqlite3StrICmp(ref tls, zIn, __ccgo_ts + 8252) == 0) {
        return (uint32)EP_IsTrue;
    }
    if (_sqlite3StrICmp(ref tls, zIn, __ccgo_ts + 8257) == 0) {
        return (uint32)EP_IsFalse;
    }
    return (uint32)0;
}

internal static void _sqlite3MemoryBarrier(ref libc.TLS tls) {
}

internal static int32 /*r*/ _sqlite3MisuseError(ж<libc.TLS> tls, int32 lineno) {
    return _sqlite3ReportError(tls, (int32)SQLITE_MISUSE, lineno, __ccgo_ts + 26482);
}

internal static void _sqlite3OpenSchemaTable(ж<libc.TLS> tls, uintptr p, int32 iDb) {
    uintptr v = default;
    _ = v;
    v = _sqlite3GetVdbe(tls, p);
    _sqlite3TableLock(tls, p, iDb, (uint32)SCHEMA_ROOT, (uint8)1, __ccgo_ts + 6370);
    _sqlite3VdbeAddOp4Int(tls, v, (int32)OP_OpenWrite, 0, (int32)SCHEMA_ROOT, iDb, (int32)5);
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnTab == 0) {
        (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)p)).Value.FnTab = (int32)1;
    }
}

internal static int32 /*r*/ _sqlite3PagerCheckpoint(ж<libc.TLS> tls, uintptr pPager, uintptr db, int32 eMode, uintptr pnLog, uintptr pnCkpt) {
    int32 rc = default;
    uintptr v1 = default;
    _ = rc;
    _ = v1;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal == (uintptr)0 && libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FjournalMode) == (int32)PAGER_JOURNALMODE_WAL) {
        _sqlite3_exec__ccgo_gp(tls, db, __ccgo_ts + 4300, (uintptr)0, (uintptr)0, nil);
    }
    if ((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal != 0) {
        if (eMode <= SQLITE_CHECKPOINT_PASSIVE) {
            v1 = (uintptr)0;
        } else {
            v1 = (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FxBusyHandler;
        }
        rc = _sqlite3WalCheckpoint(tls, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpWal, db, eMode, v1, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpBusyHandlerArg, libc.Int32FromUint8((@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FwalSyncFlags), (int32)(@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpageSize, (@unsafe.Pointer.To<TPager>((@unsafe.Pointer)pPager)).Value.FpTmpSpace, pnLog, pnCkpt);
    }
    return rc;
}

internal static uintptr /*r*/ _sqlite3PreferredTableName(ж<libc.TLS> tls, uintptr zName) {
    if (Xsqlite3_strnicmp(tls, zName, __ccgo_ts + 6830, (int32)7) == 0) {
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zName + (uintptr)7, __ccgo_ts + 6370 + 7) == 0) {
            return __ccgo_ts + 6876;
        }
        if (_sqlite3StrICmp(ref (tls).DerefOrNull(), zName + (uintptr)7, __ccgo_ts + 6838 + 7) == 0) {
            return __ccgo_ts + 6857;
        }
    }
    return zName;
}

internal static void _sqlite3RegisterLikeFunctions(ж<libc.TLS> tls, uintptr db, int32 caseSensitive) {
    int32 flags = default;
    int32 nArg = default;
    uintptr pDef = default;
    uintptr pInfo = default;
    _ = flags;
    _ = nArg;
    _ = pDef;
    _ = pInfo;
    if (caseSensitive != 0) {
        pInfo = (uintptr)Ꮡ_likeInfoAlt;
        flags = libc.Int32FromInt32(SQLITE_FUNC_LIKE) | libc.Int32FromInt32(SQLITE_FUNC_CASE);
    } else {
        pInfo = (uintptr)Ꮡ_likeInfoNorm;
        flags = (int32)SQLITE_FUNC_LIKE;
    }
    nArg = (int32)2;
    for (;;) {
        if (!(nArg <= (int32)3)) {
            break;
        }
        _sqlite3CreateFunc(tls, db, __ccgo_ts + 16718, nArg, (int32)SQLITE_UTF8, pInfo, __ccgo_fp(_likeFunc), (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0);
        pDef = _sqlite3FindFunction(tls, db, __ccgo_ts + 16718, nArg, (uint8)SQLITE_UTF8, (uint8)0);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pDef + 4))).Value |= libc.Uint32FromInt32(flags);
        (@unsafe.Pointer.To<Tu32>(__ccgo_ptr(pDef + 4))).Value &= libc.Uint32FromInt32(~libc.Int32FromInt32(SQLITE_FUNC_UNSAFE));
        goto _1;
_1:;
        nArg = nArg + 1;
    }
}

internal static void _sqlite3RegisterPerConnectionBuiltinFunctions(ж<libc.TLS> tls, uintptr db) {
    int32 rc = default;
    _ = rc;
    rc = Xsqlite3_overload_function(tls, db, __ccgo_ts + 16712, (int32)2);
    if (rc == (int32)SQLITE_NOMEM) {
        _sqlite3OomFault(tls, db);
    }
}

internal static int32 /*r*/ _sqlite3ReportError(ж<libc.TLS> tls, int32 iErr, int32 lineno, uintptr zType) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        Xsqlite3_log(tls, iErr, __ccgo_ts + 26437, libc.VaList(bp + 8, zType, lineno, (uintptr)20 + Xsqlite3_sourceid(tls)));
        r = iErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _sqlite3RtreeInit(ж<libc.TLS> tls, uintptr db) {
    uintptr c = default;
    uintptr c1 = default;
    int32 rc = default;
    int32 utf8 = default;
    _ = c;
    _ = c1;
    _ = rc;
    _ = utf8;
    utf8 = (int32)SQLITE_UTF8;
    rc = Xsqlite3_create_function(tls, db, __ccgo_ts + 30410, (int32)2, utf8, (uintptr)0, __ccgo_fp(_rtreenode), (uintptr)0, (uintptr)0);
    if (rc == SQLITE_OK) {
        rc = Xsqlite3_create_function(tls, db, __ccgo_ts + 30420, (int32)1, utf8, (uintptr)0, __ccgo_fp(_rtreedepth), (uintptr)0, (uintptr)0);
    }
    if (rc == SQLITE_OK) {
        rc = Xsqlite3_create_function(tls, db, __ccgo_ts + 30431, -(int32)1, utf8, (uintptr)0, __ccgo_fp(_rtreecheck), (uintptr)0, (uintptr)0);
    }
    if (rc == SQLITE_OK) {
        c = libc.UintptrFromInt32(RTREE_COORD_REAL32);
        rc = Xsqlite3_create_module_v2(tls, db, __ccgo_ts + 30154, (uintptr)Ꮡ_rtreeModule, c, (uintptr)0);
    }
    if (rc == SQLITE_OK) {
        c1 = libc.UintptrFromInt32(RTREE_COORD_INT32);
        rc = Xsqlite3_create_module_v2(tls, db, __ccgo_ts + 30442, (uintptr)Ꮡ_rtreeModule, c1, (uintptr)0);
    }
    if (rc == SQLITE_OK) {
        rc = _sqlite3_geopoly_init(tls, db);
    }
    return rc;
}

internal static int32 /*r*/ _sqlite3SafetyCheckOk(ж<libc.TLS> tls, uintptr db) {
    int32 r = default;

    Tu8 eOpenState = default;
    _ = eOpenState;
    if (db == (uintptr)0) {
        _logBadConnection(tls, __ccgo_ts + 1746);
        return 0;
    }
    eOpenState = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FeOpenState;
    if (libc.Int32FromUint8(eOpenState) != (int32)SQLITE_STATE_OPEN) {
        if (_sqlite3SafetyCheckSickOrOk(tls, db) != 0) {
            _logBadConnection(tls, __ccgo_ts + 1921);
        }
        return 0;
    } else {
        return (int32)1;
    }
    return r;
}

internal static int32 /*r*/ _sqlite3SafetyCheckSickOrOk(ж<libc.TLS> tls, uintptr db) {
    int32 r = default;

    Tu8 eOpenState = default;
    _ = eOpenState;
    eOpenState = (@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.FeOpenState;
    if (libc.Int32FromUint8(eOpenState) != (int32)SQLITE_STATE_SICK && libc.Int32FromUint8(eOpenState) != (int32)SQLITE_STATE_OPEN && libc.Int32FromUint8(eOpenState) != (int32)SQLITE_STATE_BUSY) {
        _logBadConnection(tls, __ccgo_ts + 1930);
        return 0;
    } else {
        return (int32)1;
    }
    return r;
}

internal static uintptr /*r*/ _sqlite3SelectOpName(ref libc.TLS tls, int32 id) {
    uintptr z = default;
    _ = z;
    switch (id) {
    case var ᴛ when ᴛ == (int32)TK_ALL: {
        z = __ccgo_ts + 20622;
        break;
    }
    case var ᴛ when ᴛ == (int32)TK_INTERSECT: {
        z = __ccgo_ts + 20632;
        break;
    }
    case var ᴛ when ᴛ == (int32)TK_EXCEPT: {
        z = __ccgo_ts + 20642;
        break;
    }
    default: {
        z = __ccgo_ts + 20649;
        break;
        break;
    }}

    return z;
}

internal static void _sqlite3SelectWrongNumTermsError(ж<libc.TLS> tls, uintptr pParse, uintptr p) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        if ((Tu32)((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.FselFlags & (uint32)SF_Values) != 0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21001, 0);
        } else {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 21047, libc.VaList(bp + 8, _sqlite3SelectOpName(ref (tls).DerefOrNull(), libc.Int32FromUint8((@unsafe.Pointer.To<TSelect>((@unsafe.Pointer)p)).Value.Fop))));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static readonly ж<array<uintptr>> Ꮡ_sqlite3StdType = new StaticBox<array<uintptr>>(default(array<uintptr>));
internal static ref array<uintptr> _sqlite3StdType => ref ((StaticBox<array<uintptr>>)Ꮡ_sqlite3StdType).ValueSlot;
internal static void initᴛ_sqlite3StdType() { _sqlite3StdType = new array<uintptr>(6){
    [0] = __ccgo_ts + 1185,
    [1] = __ccgo_ts + 1189,
    [2] = __ccgo_ts + 1194,
    [3] = __ccgo_ts + 1198,
    [4] = __ccgo_ts + 1206,
    [5] = __ccgo_ts + 1211
}; }

internal static void _sqlite3SubselectError(ж<libc.TLS> tls, uintptr pParse, int32 nActual, int32 nExpect) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr zFmt = default;
        _ = zFmt;
        if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
            zFmt = __ccgo_ts + 8359;
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, zFmt, libc.VaList(bp + 8, nActual, nExpect));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _sqlite3ThreadJoin(ж<libc.TLS> tls, uintptr p, uintptr ppOut) {
    int32 rc = default;
    int32 v1 = default;
    _ = rc;
    _ = v1;
    if (p == (uintptr)0) {
        return (int32)SQLITE_NOMEM;
    }
    if ((@unsafe.Pointer.To<TSQLiteThread>((@unsafe.Pointer)p)).Value.Fdone != 0) {
        (@unsafe.Pointer.To<uintptr>(__ccgo_ptr(ppOut))).Value = (@unsafe.Pointer.To<TSQLiteThread>((@unsafe.Pointer)p)).Value.FpOut;
        rc = SQLITE_OK;
    } else {
        if (libc.Xpthread_join(tls, (@unsafe.Pointer.To<TSQLiteThread>((@unsafe.Pointer)p)).Value.Ftid, ppOut) != 0) {
            v1 = (int32)SQLITE_ERROR;
        } else {
            v1 = SQLITE_OK;
        }
        rc = v1;
    }
    Xsqlite3_free(tls, p);
    return rc;
}

internal static int32 /*r*/ _sqlite3TwoPartName(ж<libc.TLS> tls, uintptr pParse, uintptr pName1, uintptr pName2, @unsafe.Pointer pUnqual) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr db = default;
        int32 iDb = default;
        _ = db;
        _ = iDb;
        db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
        if ((@unsafe.Pointer.To<TToken>((@unsafe.Pointer)pName2)).Value.Fn > (uint32)0) {
            if ((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.Fbusy != 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13841, 0);
                r = -(int32)1; goto ᒐdone;
            }
            (@unsafe.Pointer.To<uintptr>(pUnqual)).Value = pName2;
            iDb = _sqlite3FindDb(tls, db, (@unsafe.Pointer)pName1);
            if (iDb < 0) {
                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13858, libc.VaList(bp + 8, pName1));
                r = -(int32)1; goto ᒐdone;
            }
        } else {
            iDb = libc.Int32FromUint8((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)db)).Value.Finit1.FiDb);
            (@unsafe.Pointer.To<uintptr>(pUnqual)).Value = pName1;
        }
        r = iDb;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _sqlite3WindowChain(ж<libc.TLS> tls, uintptr pParse, uintptr pWin, uintptr pList) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr db = default;
        uintptr pExist = default;
        uintptr zErr = default;
        _ = db;
        _ = pExist;
        _ = zErr;
        if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FzBase != 0) {
            db = (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb;
            pExist = _windowFind(tls, pParse, pList, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FzBase);
            if (pExist != 0) {
                zErr = (uintptr)0;
                if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpPartition != 0) {
                    zErr = __ccgo_ts + 24657;
                } else {
                    if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pExist)).Value.FpOrderBy != 0 && (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOrderBy != 0) {
                        zErr = __ccgo_ts + 24674;
                    } else {
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pExist)).Value.FbImplicitFrame) == 0) {
                            zErr = __ccgo_ts + 24690;
                        }
                    }
                }
                if (zErr != 0) {
                    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24710, libc.VaList(bp + 8, zErr, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FzBase));
                } else {
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpPartition = _sqlite3ExprListDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pExist)).Value.FpPartition, 0);
                    if ((@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pExist)).Value.FpOrderBy != 0) {
                        (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FpOrderBy = _sqlite3ExprListDup(tls, db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pExist)).Value.FpOrderBy, 0);
                    }
                    _sqlite3DbFree__ccgo_gp(tls, (@unsafe.Pointer)db, (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FzBase);
                    (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)pWin)).Value.FzBase = (uintptr)0;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static readonly ж<array<uintptr>> Ꮡ_sqlite3azCompileOpt = new StaticBox<array<uintptr>>(default(array<uintptr>));
internal static ref array<uintptr> _sqlite3azCompileOpt => ref ((StaticBox<array<uintptr>>)Ꮡ_sqlite3azCompileOpt).ValueSlot;
internal static void initᴛ_sqlite3azCompileOpt() { _sqlite3azCompileOpt = new array<uintptr>(57){
    [0] = __ccgo_ts,
    [1] = __ccgo_ts + 20,
    [2] = __ccgo_ts + 40,
    [3] = __ccgo_ts + 59,
    [4] = __ccgo_ts + 84,
    [5] = __ccgo_ts + 106,
    [6] = __ccgo_ts + 136,
    [7] = __ccgo_ts + 156,
    [8] = __ccgo_ts + 176,
    [9] = __ccgo_ts + 199,
    [10] = __ccgo_ts + 224,
    [11] = __ccgo_ts + 251,
    [12] = __ccgo_ts + 276,
    [13] = __ccgo_ts + 298,
    [14] = __ccgo_ts + 330,
    [15] = __ccgo_ts + 356,
    [16] = __ccgo_ts + 381,
    [17] = __ccgo_ts + 402,
    [18] = __ccgo_ts + 420,
    [19] = __ccgo_ts + 443,
    [20] = __ccgo_ts + 462,
    [21] = __ccgo_ts + 481,
    [22] = __ccgo_ts + 493,
    [23] = __ccgo_ts + 508,
    [24] = __ccgo_ts + 530,
    [25] = __ccgo_ts + 555,
    [26] = __ccgo_ts + 578,
    [27] = __ccgo_ts + 600,
    [28] = __ccgo_ts + 611,
    [29] = __ccgo_ts + 624,
    [30] = __ccgo_ts + 639,
    [31] = __ccgo_ts + 655,
    [32] = __ccgo_ts + 668,
    [33] = __ccgo_ts + 689,
    [34] = __ccgo_ts + 713,
    [35] = __ccgo_ts + 736,
    [36] = __ccgo_ts + 752,
    [37] = __ccgo_ts + 768,
    [38] = __ccgo_ts + 792,
    [39] = __ccgo_ts + 819,
    [40] = __ccgo_ts + 839,
    [41] = __ccgo_ts + 861,
    [42] = __ccgo_ts + 883,
    [43] = __ccgo_ts + 913,
    [44] = __ccgo_ts + 938,
    [45] = __ccgo_ts + 964,
    [46] = __ccgo_ts + 984,
    [47] = __ccgo_ts + 1004,
    [48] = __ccgo_ts + 1030,
    [49] = __ccgo_ts + 1053,
    [50] = __ccgo_ts + 1079,
    [51] = __ccgo_ts + 1101,
    [52] = __ccgo_ts + 1122,
    [53] = __ccgo_ts + 1137,
    [54] = __ccgo_ts + 1145,
    [55] = __ccgo_ts + 1159,
    [56] = __ccgo_ts + 1172
}; }

internal static void _sqliteAuthBadReturnCode(ж<libc.TLS> tls, uintptr pParse) {
    _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 13724, 0);
    (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Frc = (int32)SQLITE_ERROR;
}

internal static readonly ж<TFuncDef> Ꮡ_statGetFuncdef = new StaticBox<TFuncDef>(default(TFuncDef));
internal static ref TFuncDef _statGetFuncdef => ref ((StaticBox<TFuncDef>)Ꮡ_statGetFuncdef).ValueSlot;
internal static void initᴛ_statGetFuncdef() { _statGetFuncdef = new TFuncDef(
    FnArg: (int16)(libc.Int32FromInt32(1) + libc.Int32FromInt32(IsStat4)),
    FfuncFlags: (uint32)SQLITE_UTF8,
    FzName: __ccgo_ts + 13136
); }

internal static readonly ж<TFuncDef> Ꮡ_statInitFuncdef = new StaticBox<TFuncDef>(default(TFuncDef));
internal static ref TFuncDef _statInitFuncdef => ref ((StaticBox<TFuncDef>)Ꮡ_statInitFuncdef).ValueSlot;
internal static void initᴛ_statInitFuncdef() { _statInitFuncdef = new TFuncDef(
    FnArg: (int16)4,
    FfuncFlags: (uint32)SQLITE_UTF8,
    FzName: __ccgo_ts + 13099
); }

internal static readonly ж<TFuncDef> Ꮡ_statPushFuncdef = new StaticBox<TFuncDef>(default(TFuncDef));
internal static ref TFuncDef _statPushFuncdef => ref ((StaticBox<TFuncDef>)Ꮡ_statPushFuncdef).ValueSlot;
internal static void initᴛ_statPushFuncdef() { _statPushFuncdef = new TFuncDef(
    FnArg: (int16)(libc.Int32FromInt32(2) + libc.Int32FromInt32(IsStat4)),
    FfuncFlags: (uint32)SQLITE_UTF8,
    FzName: __ccgo_ts + 13109
); }

internal static void _sumFinalize(ж<libc.TLS> tls, uintptr context) {
    uintptr p = default;
    _ = p;
    p = Xsqlite3_aggregate_context(tls, context, 0);
    if (p != 0 && (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fcnt > 0) {
        if ((@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fapprox != 0) {
            if ((@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.Fovrfl != 0) {
                Xsqlite3_result_error(tls, context, __ccgo_ts + 16571, -(int32)1);
            } else {
                if (!(_sqlite3IsOverflow(tls, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FrErr) != 0)) {
                    Xsqlite3_result_double(tls, context, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FrSum + (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FrErr);
                } else {
                    Xsqlite3_result_double(tls, context, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FrSum);
                }
            }
        } else {
            Xsqlite3_result_int64(tls, context, (@unsafe.Pointer.To<TSumCtx>((@unsafe.Pointer)p)).Value.FiSum);
        }
    }
}

internal static int32 /*r*/ _tableMayNotBeDropped(ж<libc.TLS> tls, uintptr db, uintptr pTab) {
    if (Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName, __ccgo_ts + 6830, (int32)7) == 0) {
        if (Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName + (uintptr)7, __ccgo_ts + 3581, (int32)4) == 0) {
            return 0;
        }
        if (Xsqlite3_strnicmp(tls, (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FzName + (uintptr)7, __ccgo_ts + 7632, (int32)10) == 0) {
            return 0;
        }
        return (int32)1;
    }
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Shadow) != (uint32)0 && _sqlite3ReadOnlyShadowTables(ref (tls).DerefOrNull(), db) != 0) {
        return (int32)1;
    }
    if ((Tu32)((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FtabFlags & (uint32)TF_Eponymous) != 0) {
        return (int32)1;
    }
    return 0;
}

internal static readonly ж<TToken> Ꮡ_tkCoalesce = new StaticBox<TToken>(default(TToken));
internal static ref TToken _tkCoalesce => ref ((StaticBox<TToken>)Ꮡ_tkCoalesce).ValueSlot;
internal static void initᴛ_tkCoalesce() { _tkCoalesce = new TToken(
    Fz: __ccgo_ts + 7035,
    Fn: (uint32)8
); }

internal static uintptr /*r*/ _triggersReallyExist(ж<libc.TLS> tls, uintptr pParse, uintptr pTab, int32 op, uintptr pChanges, @unsafe.Pointer pMask) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 mask = default;
        uintptr p = default;
        uintptr pList = default;
        uintptr v1 = default;
        _ = mask;
        _ = p;
        _ = pList;
        _ = v1;
        mask = 0;
        pList = (uintptr)0;
        pList = _sqlite3TriggerList(ref (tls).DerefOrNull(), pParse, pTab);
        if (pList != (uintptr)0) {
            p = pList;
            if ((Tu64)((@unsafe.Pointer.To<Tsqlite3>((@unsafe.Pointer)(@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb)).Value.Fflags & (uint64)SQLITE_EnableTrigger) == (uint64)0 && (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpTrigger != (uintptr)0 && _sqlite3SchemaToIndex(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.Fdb, (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)(@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpTrigger)).Value.FpSchema) != (int32)1) {
                if (pList == (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpTrigger) {
                    pList = (uintptr)0;
                    goto exit_triggers_exist;
                }
                while ((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpNext != 0 && (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpNext != (@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FpTrigger) {
                    p = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpNext;
                }
                (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpNext = (uintptr)0;
                p = pList;
            }
            for (var cond = true; cond; cond = p != 0) {
                if (libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Fop) == op && _checkColumnOverlap(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpColumns, pChanges) != 0) {
                    mask = mask | libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Ftr_tm);
                } else {
                    if (libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_RETURNING) {
                        (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Fop = libc.Uint8FromInt32(op);
                        if (libc.Int32FromUint8((@unsafe.Pointer.To<TTable>((@unsafe.Pointer)pTab)).Value.FeTabType) == (int32)TABTYP_VTAB) {
                            if (op != (int32)TK_INSERT) {
                                if (op == (int32)TK_DELETE) {
                                    v1 = __ccgo_ts + 22634;
                                } else {
                                    v1 = __ccgo_ts + 22641;
                                }
                                _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 22648, libc.VaList(bp + 8, v1));
                            }
                            (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Ftr_tm = (uint8)TRIGGER_BEFORE;
                        } else {
                            (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Ftr_tm = (uint8)TRIGGER_AFTER;
                        }
                        mask = mask | libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Ftr_tm);
                    } else {
                        if ((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FbReturning != 0 && libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Fop) == (int32)TK_INSERT && op == (int32)TK_UPDATE && (@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FpToplevel == (uintptr)0) {
                            mask = mask | libc.Int32FromUint8((@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.Ftr_tm);
                        }
                    }
                }
                p = (@unsafe.Pointer.To<TTrigger>((@unsafe.Pointer)p)).Value.FpNext;
            }
        }
        goto exit_triggers_exist;
exit_triggers_exist:;
        if (pMask != nil) {
            (@unsafe.Pointer.To<int32>(pMask)).Value = mask;
        }
        if (mask != 0) {
            v1 = pList;
        } else {
            v1 = (uintptr)0;
        }
        r = v1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _unixCheckReservedLock(ж<libc.TLS> tls, uintptr id, uintptr pResOut) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        uintptr pFile = default;
        int32 rc = default;
        int32 reserved = default;
        int32 v1 = default;
        Tflock _ᴛ1 = default;
        _ = pFile;
        _ = rc;
        _ = reserved;
        _ = v1;
        rc = SQLITE_OK;
        reserved = 0;
        pFile = id;
        Xsqlite3_mutex_enter(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FpLockMutex);
        if (libc.Int32FromUint8((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FeFileLock) > (int32)SHARED_LOCK) {
            reserved = (int32)1;
        }
        if (!(reserved != 0) && !((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FbProcessLock != 0)) {
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_whence = 0;
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_start = (int64)(_sqlite3PendingByte + libc.Int32FromInt32(1));
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_len = (int64)1;
            (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type = (int16)F_WRLCK;
            if (libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdEnabled) != 0 && libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdSupported) != 0 && (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FhLock != 0) {
                v1 = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FhLock;
            } else {
                v1 = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh;
            }
            if (_osFcntlOfd(tls, v1, (int32)F_GETLK, bp) != 0) {
                rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(14).Lsh((int64)(libc.Int32FromInt32(8)));
                _storeLastErrno(ref (tls).DerefOrNull(), pFile, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))));
            } else {
                if ((int32)(@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type != (int32)F_UNLCK) {
                    reserved = (int32)1;
                }
            }
        }
        Xsqlite3_mutex_leave(tls, (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FpLockMutex);
        (@unsafe.Pointer.To<int32>(__ccgo_ptr(pResOut))).Value = reserved;
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _unixDelete(ж<libc.TLS> tls, uintptr NotUsed, uintptr zPath, int32 dirSync) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 rc = default;
        int32 _ᴛ1 = default;
        _ = rc;
        rc = SQLITE_OK;
        _ = NotUsed;
        if (GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, int32>>(_aSyscall[(int32)16].FpCurrent)(tls, zPath) == -(int32)1) {
            if (~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))) == (int32)ENOENT) {
                rc = libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(23).Lsh((int64)(libc.Int32FromInt32(8)));
            } else {
                rc = _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(10).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3666, zPath, (int32)47655);
            }
            r = rc; goto ᒐdone;
        }
        if ((dirSync & (int32)1) != 0) {
            rc = GoFuncCookie.Func<Func<ж<libc.TLS>, uintptr, uintptr, int32>>(_aSyscall[(int32)17].FpCurrent)(tls, zPath, bp);
            if (rc == SQLITE_OK) {
                if (_full_fsync(tls, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)), 0, 0) != 0) {
                    rc = _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(5).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 4107, zPath, (int32)47665);
                }
                _robust_close(tls, (uintptr)0, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(bp)), (int32)47667);
            } else {
                rc = SQLITE_OK;
            }
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static void _unixDlError(ж<libc.TLS> tls, uintptr NotUsed, int32 nBuf, uintptr zBufOut) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr zErr = default;
        _ = zErr;
        _ = NotUsed;
        _unixEnterMutex(tls);
        zErr = libc.Xdlerror(tls);
        if (zErr != 0) {
            Xsqlite3_snprintf(tls, nBuf, zBufOut, __ccgo_ts + 3982, libc.VaList(bp + 8, zErr));
        }
        _unixLeaveMutex(tls);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int32 /*r*/ _unixFileLock(ж<libc.TLS> tls, uintptr pFile, uintptr pLock) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 h = default;
        int32 rc = default;
        int32 v1 = default;
        uintptr pInode = default;
        Tflock _ᴛ1 = default;
        _ = h;
        _ = pInode;
        _ = rc;
        _ = v1;
        pInode = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode;
        if (libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdEnabled) != 0 && libc.AtomicLoadPInt32((uintptr)Ꮡ_ofdSupported) != 0 && (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FhLock != 0) {
            v1 = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FpInode)).Value.FhLock;
        } else {
            v1 = (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh;
        }
        h = v1;
        if ((libc.Int32FromUint16((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FctrlFlags) & (libc.Int32FromInt32(UNIXFILE_EXCL) | libc.Int32FromInt32(UNIXFILE_RDONLY))) == (int32)UNIXFILE_EXCL) {
            if (libc.Int32FromUint8((@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FbProcessLock) == 0) {
                (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_whence = 0;
                (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_start = (int64)(_sqlite3PendingByte + libc.Int32FromInt32(2));
                (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_len = (int64)SHARED_SIZE;
                (@unsafe.Pointer.To<Tflock>(__ccgo_ptr(bp))).Value.Fl_type = (int16)F_WRLCK;
                rc = _osFcntlOfd(tls, h, (int32)F_SETLK, bp);
                if (rc < 0) {
                    r = rc; goto ᒐdone;
                }
                (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FbProcessLock = (uint8)1;
                (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnLock = (@unsafe.Pointer.To<TunixInodeInfo>((@unsafe.Pointer)pInode)).Value.FnLock + 1;
            } else {
                rc = 0;
            }
        } else {
            rc = _osFcntlOfd(tls, h, (int32)F_SETLK, pLock);
        }
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static int32 /*r*/ _unixGetLastError(ж<libc.TLS> tls, uintptr NotUsed, int32 NotUsed2, uintptr NotUsed3) {
    _ = NotUsed;
    _ = NotUsed2;
    _ = NotUsed3;
    return ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls)));
}

internal static int32 /*r*/ _unixLogErrorAtLine(ж<libc.TLS> tls, int32 errcode, uintptr zFunc, uintptr zPath, int32 iLine) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(48);
        defer(tls.Free, (nint)48, ref ᒐ);
        int32 iErrno = default;
        uintptr zErr = default;
        _ = iErrno;
        _ = zErr;
        iErrno = ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls)));
        zErr = __ccgo_ts + 1732;
        if (zPath == (uintptr)0) {
            zPath = __ccgo_ts + 1732;
        }
        Xsqlite3_log(tls, errcode, __ccgo_ts + 3843, libc.VaList(bp + 8, iLine, iErrno, zFunc, zPath, zErr));
        r = errcode;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _unixShmBarrier(ж<libc.TLS> tls, uintptr fd) {
    _ = fd;
    _sqlite3MemoryBarrier(ref (tls).DerefOrNull());
    _unixEnterMutex(tls);
    _unixLeaveMutex(tls);
}

internal static int32 /*r*/ _unixTruncate(ж<libc.TLS> tls, uintptr id, Ti64 nByte) {
    int32 r = default;

    uintptr pFile = default;
    int32 rc = default;
    _ = pFile;
    _ = rc;
    pFile = id;
    if ((@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk > 0) {
        nByte = quo((nByte + (int64)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk - (int64)1), (int64)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk) * (int64)(@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FszChunk;
    }
    rc = _robust_ftruncate(tls, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.Fh, nByte);
    if (rc != 0) {
        _storeLastErrno(ref (tls).DerefOrNull(), pFile, ~@unsafe.Pointer.To<int32>(__ccgo_ptr(libc.X__errno_location(tls))));
        return _unixLogErrorAtLine(tls, libc.Int32FromInt32(SQLITE_IOERR) | libc.Int32FromInt32(6).Lsh((int64)(libc.Int32FromInt32(8))), __ccgo_ts + 3592, (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FzPath, (int32)44782);
    } else {
        if (nByte < (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSize) {
            (@unsafe.Pointer.To<TunixFile>((@unsafe.Pointer)pFile)).Value.FmmapSize = nByte;
        }
        return SQLITE_OK;
    }
    return r;
}

internal static int32 /*r*/ _vdbeFkError(ж<libc.TLS> tls, uintptr p) {
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Frc = libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
    (@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FerrorAction = (uint8)OE_Abort;
    _sqlite3VdbeError(tls, p, __ccgo_ts + 5478, 0);
    if ((libc.Int32FromUint8((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.FprepFlags) & (int32)SQLITE_PREPARE_SAVESQL) == 0) {
        return (int32)SQLITE_ERROR;
    }
    return libc.Int32FromInt32(SQLITE_CONSTRAINT) | libc.Int32FromInt32(3).Lsh((int64)(libc.Int32FromInt32(8)));
}

internal static int32 /*r*/ _vdbeSafety(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;

    if ((@unsafe.Pointer.To<TVdbe>((@unsafe.Pointer)p)).Value.Fdb == (uintptr)0) {
        Xsqlite3_log(tls, (int32)SQLITE_MISUSE, __ccgo_ts + 5591, 0);
        return (int32)1;
    } else {
        return 0;
    }
    return r;
}

internal static int32 /*r*/ _vdbeSafetyNotNull(ж<libc.TLS> tls, uintptr p) {
    int32 r = default;

    if (p == (uintptr)0) {
        Xsqlite3_log(tls, (int32)SQLITE_MISUSE, __ccgo_ts + 5636, 0);
        return (int32)1;
    } else {
        return _vdbeSafety(tls, p);
    }
    return r;
}

internal static void _walLimitSize(ж<libc.TLS> tls, uintptr pWal, Ti64 nMax) {
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(32);
        defer(tls.Free, (nint)32, ref ᒐ);
        int32 rx = default;
        Ti64 _ᴛ1 = default;
        _ = rx;
        _sqlite3BeginBenignMalloc(tls);
        rx = _sqlite3OsFileSize(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, bp);
        if (rx == SQLITE_OK && ~@unsafe.Pointer.To<Ti64>(__ccgo_ptr(bp)) > nMax) {
            rx = _sqlite3OsTruncate(tls, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FpWalFd, nMax);
        }
        _sqlite3EndBenignMalloc(tls);
        if (rx != 0) {
            Xsqlite3_log(tls, rx, __ccgo_ts + 4355, libc.VaList(bp + 16, (@unsafe.Pointer.To<TWal>((@unsafe.Pointer)pWal)).Value.FzWalName));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void _windowCheckValue(ж<libc.TLS> tls, uintptr pParse, int32 reg, int32 eCond) {
    int32 regString = default;
    int32 regZero = default;
    uintptr v = default;
    _ = regString;
    _ = regZero;
    _ = v;
    v = _sqlite3GetVdbe(tls, pParse);
    regZero = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Integer, 0, regZero);
    if (eCond >= (int32)WINDOW_STARTING_NUM) {
        regString = _sqlite3GetTempReg(ref (tls).DerefOrNull(), pParse);
        _sqlite3VdbeAddOp4(tls, v, (int32)OP_String8, 0, regString, 0, __ccgo_ts + 1732, -(int32)1);
        _sqlite3VdbeAddOp3(tls, v, (int32)OP_Ge, regString, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2, reg);
        _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, libc.Uint16FromInt32(libc.Int32FromInt32(SQLITE_AFF_NUMERIC) | libc.Int32FromInt32(SQLITE_JUMPIFNULL)));
    } else {
        _sqlite3VdbeAddOp2(tls, v, (int32)OP_MustBeInt, reg, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2);
    }
    _sqlite3VdbeAddOp3(tls, v, _aOp1[eCond], regZero, _sqlite3VdbeCurrentAddr(ref (tls).DerefOrNull(), v) + (int32)2, reg);
    _sqlite3VdbeChangeP5(ref (tls).DerefOrNull(), v, (uint16)SQLITE_AFF_NUMERIC);
    _sqlite3MayAbort(ref (tls).DerefOrNull(), pParse);
    _sqlite3VdbeAddOp2(tls, v, (int32)OP_Halt, (int32)SQLITE_ERROR, (int32)OE_Abort);
    _sqlite3VdbeAppendP4(tls, v, _azErr[eCond], -(int32)1);
    _sqlite3ReleaseTempReg(ref (tls).DerefOrNull(), pParse, regZero);
}

internal static uintptr /*r*/ _windowFind(ж<libc.TLS> tls, uintptr pParse, uintptr pList, uintptr zName) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uintptr p = default;
        _ = p;
        p = pList;
        for (;;) {
            if (!(p != 0)) {
                break;
            }
            if (_sqlite3StrICmp(ref (tls).DerefOrNull(), (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FzName, zName) == 0) {
                break;
            }
            goto _1;
_1:;
            p = (@unsafe.Pointer.To<TWindow>((@unsafe.Pointer)p)).Value.FpNextWin;
        }
        if (p == (uintptr)0) {
            _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 24446, libc.VaList(bp + 8, zName));
        }
        r = p;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void _yyStackOverflow(ж<libc.TLS> tls, uintptr yypParser) {
    uintptr pParse = default;
    _ = pParse;
    pParse = (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.FpParse;
    while ((@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyytos > (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.Fyystack) {
        _yy_pop_parser_stack(tls, yypParser);
    }
    if ((@unsafe.Pointer.To<TParse>((@unsafe.Pointer)pParse)).Value.FnErr == 0) {
        _sqlite3ErrorMsg(tls, (@unsafe.Pointer)pParse, __ccgo_ts + 25149, 0);
    }
    (@unsafe.Pointer.To<TyyParser>((@unsafe.Pointer)yypParser)).Value.FpParse = pParse;
}

internal static readonly @string alloca = "__builtin_alloca"u8;

internal static readonly @string blkcnt64_t = "blkcnt_t"u8;

internal static readonly @string creat64 = "creat"u8;

internal static readonly @string fallocate64 = "fallocate"u8;

internal static readonly @string fgetpos64 = "fgetpos"u8;

internal static readonly @string flock64 = "flock"u8;

internal static readonly @string fopen64 = "fopen"u8;

internal static readonly @string fpos64_t = "fpos_t"u8;

internal static readonly @string freopen64 = "freopen"u8;

internal static readonly @string fsblkcnt64_t = "fsblkcnt_t"u8;

internal static readonly @string fseeko64 = "fseeko"u8;

internal static readonly @string fsetpos64 = "fsetpos"u8;

internal static readonly @string fsfilcnt64_t = "fsfilcnt_t"u8;

internal static readonly @string fstat64 = "fstat"u8;

internal static readonly @string fstatat64 = "fstatat"u8;

internal static readonly @string ftello64 = "ftello"u8;

internal static readonly @string ftruncate64 = "ftruncate"u8;

internal static readonly @string ino64_t = "ino_t"u8;

internal static UntypedInt linux => 1;

internal static readonly @string lockf64 = "lockf"u8;

internal static readonly @string loff_t = "off_t"u8;

internal static readonly @string lseek64 = "lseek"u8;

internal static readonly @string lstat64 = "lstat"u8;

internal static readonly @string mkostemp64 = "mkostemp"u8;

internal static readonly @string mkostemps64 = "mkostemps"u8;

internal static readonly @string mkstemp64 = "mkstemp"u8;

internal static readonly @string mkstemps64 = "mkstemps"u8;

internal static readonly @string mmap64 = "mmap"u8;

internal static readonly @string off64_t = "off_t"u8;

internal static readonly @string open64 = "open"u8;

internal static readonly @string openat64 = "openat"u8;

internal static readonly @string posix_fadvise64 = "posix_fadvise"u8;

internal static readonly @string posix_fallocate64 = "posix_fallocate"u8;

internal static readonly @string pread64 = "pread"u8;

internal static readonly @string pwrite64 = "pwrite"u8;

internal static readonly @string stat64 = "stat"u8;

partial struct t__ptcbᴛ1 /*dyn*/ {
    public uintptr F__f;
    public uintptr F__x;
    public uintptr F__next;
}

internal static readonly @string tmpfile64 = "tmpfile"u8;

internal static readonly @string truncate64 = "truncate"u8;

} // end sqlite3_package
