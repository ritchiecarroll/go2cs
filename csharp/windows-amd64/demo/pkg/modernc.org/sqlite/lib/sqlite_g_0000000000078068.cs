namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static int32 /*r*/ _sqlite3Initialize(ж<libc.TLS> tls) {
    uintptr pMainMtx = default;
    int32 rc = default;
    _ = pMainMtx;
    _ = rc;
    rc = _sqlite3MutexInit(tls);
    if (rc != 0) {
        return rc;
    }
    pMainMtx = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_STATIC_MAIN);
    Xsqlite3_mutex_enter(tls, pMainMtx);
    _sqlite3Config.FisMutexInit = (int32)1;
    if (!(_sqlite3Config.FisMallocInit != 0)) {
        rc = _sqlite3MallocInit(tls);
    }
    if (rc == SQLITE_OK) {
        _sqlite3Config.FisMallocInit = (int32)1;
        if (!(_sqlite3Config.FpInitMutex != 0)) {
            _sqlite3Config.FpInitMutex = _sqlite3MutexAlloc(tls, (int32)SQLITE_MUTEX_RECURSIVE);
            if (_sqlite3Config.FbCoreMutex != 0 && !(_sqlite3Config.FpInitMutex != 0)) {
                rc = (int32)SQLITE_NOMEM;
            }
        }
    }
    if (rc == SQLITE_OK) {
        _sqlite3Config.FnRefInitMutex = _sqlite3Config.FnRefInitMutex + 1;
    }
    Xsqlite3_mutex_leave(tls, pMainMtx);
    if (rc != SQLITE_OK) {
        return rc;
    }
    Xsqlite3_mutex_enter(tls, _sqlite3Config.FpInitMutex);
    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_sqlite3Config + 340) == 0 && _sqlite3Config.FinProgress == 0) {
        _sqlite3Config.FinProgress = (int32)1;
        libc.Xmemset(tls, (uintptr)Ꮡ_sqlite3BuiltinFunctions, 0, (uint64)184);
        _sqlite3RegisterBuiltinFunctions(tls);
        if (_sqlite3Config.FisPCacheInit == 0) {
            rc = _sqlite3PcacheInitialize(tls);
        }
        if (rc == SQLITE_OK) {
            _sqlite3Config.FisPCacheInit = (int32)1;
            rc = _sqlite3OsInit(tls);
        }
        if (rc == SQLITE_OK) {
            rc = _sqlite3MemdbInit(tls);
        }
        if (rc == SQLITE_OK) {
            _sqlite3PCacheBufferSetup(ref (tls).DerefOrNull(), _sqlite3Config.FpPage, _sqlite3Config.FszPage, _sqlite3Config.FnPage);
        }
        if (rc == SQLITE_OK) {
            libc.AtomicStorePInt32((uintptr)Ꮡ_sqlite3Config + 340, (int32)1);
        }
        _sqlite3Config.FinProgress = 0;
    }
    Xsqlite3_mutex_leave(tls, _sqlite3Config.FpInitMutex);
    Xsqlite3_mutex_enter(tls, pMainMtx);
    _sqlite3Config.FnRefInitMutex = _sqlite3Config.FnRefInitMutex - 1;
    if (_sqlite3Config.FnRefInitMutex <= 0) {
        Xsqlite3_mutex_free(tls, _sqlite3Config.FpInitMutex);
        _sqlite3Config.FpInitMutex = (uintptr)0;
    }
    Xsqlite3_mutex_leave(tls, pMainMtx);
    return rc;
}

} // end sqlite3_package
