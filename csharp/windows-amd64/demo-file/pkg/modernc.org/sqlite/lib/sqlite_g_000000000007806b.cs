namespace go.modernc.org.sqlite.lib;

using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

public static int32 /*r*/ Xsqlite3_initialize(ж<libc.TLS> tls) {
    int32 r = default;

    if (libc.AtomicLoadPInt32((uintptr)Ꮡ_sqlite3Config + 340) != 0) {
        return SQLITE_OK;
    } else {
        return _sqlite3Initialize(tls);
    }
    return r;
}

internal static int32 /*r*/ _sqlite3MutexInit(ж<libc.TLS> tls) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡmu.Lock();
        defer(Ꮡmu.Unlock, ref ᒐ);
        uintptr pFrom = default;
        uintptr pTo = default;
        int32 rc = default;
        _ = pFrom;
        _ = pTo;
        _ = rc;
        rc = SQLITE_OK;
        if (!(_sqlite3Config.Fmutex.FxMutexAlloc != 0)) {
            pTo = (uintptr)Ꮡ_sqlite3Config + 96;
            if (_sqlite3Config.FbCoreMutex != 0) {
                pFrom = _sqlite3DefaultMutex(ref (tls).DerefOrNull());
            } else {
                pFrom = _sqlite3NoopMutex(ref (tls).DerefOrNull());
            }
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexInit = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexInit;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexEnd = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexEnd;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexFree = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexFree;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexEnter = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexEnter;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexTry = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexTry;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexLeave = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexLeave;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexHeld = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexHeld;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexNotheld = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexNotheld;
            (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pTo)).Value.FxMutexAlloc = (@unsafe.Pointer.To<Tsqlite3_mutex_methods>((@unsafe.Pointer)pFrom)).Value.FxMutexAlloc;
        }
        rc = GoFuncCookie.Func<Func<ж<libc.TLS>, int32>>(_sqlite3Config.Fmutex.FxMutexInit)(tls);
        r = rc;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static int32 /*r*/ _vdbeSorterJoinThread(ж<libc.TLS> tls, uintptr pTask) {
    ref var pRet = ref heap(new uintptr(), out var ᏑpRet);
    int32 rc = default;
    _ = pRet;
    _ = rc;
    rc = SQLITE_OK;
    if ((@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpThread != 0) {
        pRet = (uintptr)(int64)libc.Int32FromInt32(SQLITE_ERROR);
        _sqlite3ThreadJoin(tls, (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpThread, @unsafe.Pointer.FromBox(ᏑpRet));
        rc = (int32)(int64)pRet;
        (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FbDone = 0;
        (@unsafe.Pointer.To<TSortSubtask>((@unsafe.Pointer)pTask)).Value.FpThread = (uintptr)0;
    }
    return rc;
}

} // end sqlite3_package
