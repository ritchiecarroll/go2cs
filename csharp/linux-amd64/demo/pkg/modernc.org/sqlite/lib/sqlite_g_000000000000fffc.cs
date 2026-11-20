global using Tdev_t = ulong;
global using pthread_barrier_t = go.modernc.org.libc_package.Tpthread_barrier_tᴛ1;
global using pthread_barrierattr_t = go.modernc.org.libc_package.Tpthread_mutexattr_tᴛ1;
global using pthread_spinlock_t = int;

namespace go.modernc.org.sqlite.lib;

partial class sqlite3_package {

public static UntypedInt POSIX_FADV_NORMAL => 0;

public static UntypedInt POSIX_FADV_RANDOM => 1;

public static UntypedInt POSIX_FADV_SEQUENTIAL => 2;

public static UntypedInt POSIX_FADV_WILLNEED => 3;

public static UntypedInt RTLD_NODELETE => 4096;

} // end sqlite3_package
