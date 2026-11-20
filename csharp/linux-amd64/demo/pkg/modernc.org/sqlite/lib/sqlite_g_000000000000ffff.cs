global using pthread_attr_t = go.modernc.org.sqlite.lib.sqlite3_package.Tpthread_attr_tᴛ1;
global using pthread_cond_t = go.modernc.org.sqlite.lib.sqlite3_package.Tpthread_cond_tᴛ1;
global using pthread_condattr_t = go.modernc.org.libc_package.Tpthread_mutexattr_tᴛ1;
global using pthread_key_t = uint;
global using pthread_mutex_t = go.modernc.org.libc_package.Tpthread_mutex_tᴛ1;
global using pthread_mutexattr_t = go.modernc.org.libc_package.Tpthread_mutexattr_tᴛ1;
global using pthread_once_t = int;
global using pthread_rwlock_t = go.modernc.org.libc_package.Tpthread_rwlock_tᴛ1;
global using pthread_rwlockattr_t = go.modernc.org.libc_package.Tpthread_rwlockattr_tᴛ1;
global using pthread_t = go.uintptr;

namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static UntypedInt MS_INVALIDATE => 2;

internal static int32 /*r*/ _unixGetpagesize(ж<libc.TLS> tls) {
    return (int32)libc.Xsysconf(tls, (int32)_SC_PAGESIZE);
}

} // end sqlite3_package
