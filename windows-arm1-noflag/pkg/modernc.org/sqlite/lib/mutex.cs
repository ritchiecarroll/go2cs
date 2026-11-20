// Copyright 2021 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org.sqlite.lib;

using fmt = fmt_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using libc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using go.modernc.org;
using go.modernc.org.libc.sys;
using go.sync;

partial class sqlite3_package {

[GoInit] internal static void init() {
    var tls = libc.NewTLS();
    if (Xsqlite3_threadsafe(tls) == 0) {
        throw panic(fmt.Errorf("sqlite: thread safety configuration error"u8));
    }

    var varArgs = libc.Xmalloc(tls, (typesꓸSize_t)/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
    if (varArgs == 0) {
        throw panic(fmt.Errorf("cannot allocate memory"u8));
    }

    var rc = Xsqlite3_config(tls, SQLITE_CONFIG_MUTEX, libc.VaList(varArgs, (uintptr)ᏑmutexMethods)); if (rc != SQLITE_OK) {
        var p = Xsqlite3_errstr(tls, rc);
        @string str = libc.GoString(p);
        throw panic(fmt.Errorf("sqlite: failed to configure mutex methods: %v"u8, str));
    }

    libc.Xfree(tls, varArgs);
    tls.Close();
}

internal static readonly ж<Sqlite3_mutex_methods> ᏑmutexMethods = new StaticBox<Sqlite3_mutex_methods>(default(Sqlite3_mutex_methods));
internal static ref Sqlite3_mutex_methods mutexMethods => ref ((StaticBox<Sqlite3_mutex_methods>)ᏑmutexMethods).ValueSlot;
internal static void initᴛmutexMethods() { mutexMethods = new Sqlite3_mutex_methods(
    FxMutexInit: (uintptr)GoFuncCookie.Of((Func<ж<libc.TLS>, int32>)mutexInit),
    FxMutexEnd: (uintptr)GoFuncCookie.Of((Func<ж<libc.TLS>, int32>)mutexEnd),
    FxMutexAlloc: (uintptr)GoFuncCookie.Of((Func<ж<libc.TLS>, int32, uintptr>)mutexAlloc),
    FxMutexFree: (uintptr)GoFuncCookie.Of((Action<ж<libc.TLS>, uintptr>)mutexFree),
    FxMutexEnter: (uintptr)GoFuncCookie.Of((Action<ж<libc.TLS>, uintptr>)mutexEnter),
    FxMutexTry: (uintptr)GoFuncCookie.Of((Func<ж<libc.TLS>, uintptr, int32>)mutexTry),
    FxMutexLeave: (uintptr)GoFuncCookie.Of((Action<ж<libc.TLS>, uintptr>)mutexLeave),
    FxMutexHeld: (uintptr)GoFuncCookie.Of((Func<ж<libc.TLS>, uintptr, int32>)mutexHeld),
    FxMutexNotheld: (uintptr)GoFuncCookie.Of((Func<ж<libc.TLS>, uintptr, int32>)mutexNotheld)
); }
internal static readonly ж<mutex> ᏑmutexApp10 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexApp10 => ref ((StaticBox<mutex>)ᏑmutexApp10).ValueSlot;
internal static readonly ж<mutex> ᏑmutexApp20 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexApp20 => ref ((StaticBox<mutex>)ᏑmutexApp20).ValueSlot;
internal static readonly ж<mutex> ᏑmutexApp30 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexApp30 => ref ((StaticBox<mutex>)ᏑmutexApp30).ValueSlot;
internal static readonly ж<mutex> ᏑmutexLRU0 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexLRU0 => ref ((StaticBox<mutex>)ᏑmutexLRU0).ValueSlot;
internal static readonly ж<mutex> ᏑmutexMaster0 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexMaster0 => ref ((StaticBox<mutex>)ᏑmutexMaster0).ValueSlot;
internal static readonly ж<mutex> ᏑmutexMem0 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexMem0 => ref ((StaticBox<mutex>)ᏑmutexMem0).ValueSlot;
internal static readonly ж<mutex> ᏑmutexOpen0 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexOpen0 => ref ((StaticBox<mutex>)ᏑmutexOpen0).ValueSlot;
internal static readonly ж<mutex> ᏑmutexPMem0 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexPMem0 => ref ((StaticBox<mutex>)ᏑmutexPMem0).ValueSlot;
internal static readonly ж<mutex> ᏑmutexPRNG0 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexPRNG0 => ref ((StaticBox<mutex>)ᏑmutexPRNG0).ValueSlot;
internal static readonly ж<mutex> ᏑmutexVFS10 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexVFS10 => ref ((StaticBox<mutex>)ᏑmutexVFS10).ValueSlot;
internal static readonly ж<mutex> ᏑmutexVFS20 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexVFS20 => ref ((StaticBox<mutex>)ᏑmutexVFS20).ValueSlot;
internal static readonly ж<mutex> ᏑmutexVFS30 = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex mutexVFS30 => ref ((StaticBox<mutex>)ᏑmutexVFS30).ValueSlot;
internal static uintptr mutexApp1 = (uintptr)ᏑmutexApp10;
internal static uintptr mutexApp2 = (uintptr)ᏑmutexApp20;
internal static uintptr mutexApp3 = (uintptr)ᏑmutexApp30;
internal static uintptr mutexLRU = (uintptr)ᏑmutexLRU0;
internal static uintptr mutexMaster = (uintptr)ᏑmutexMaster0;
internal static uintptr mutexMem = (uintptr)ᏑmutexMem0;
internal static uintptr mutexOpen = (uintptr)ᏑmutexOpen0;
internal static uintptr mutexPMem = (uintptr)ᏑmutexPMem0;
internal static uintptr mutexPRNG = (uintptr)ᏑmutexPRNG0;
internal static uintptr mutexVFS1 = (uintptr)ᏑmutexVFS10;
internal static uintptr mutexVFS2 = (uintptr)ᏑmutexVFS20;
internal static uintptr mutexVFS3 = (uintptr)ᏑmutexVFS30;

partial struct mutex {
    /*embed*/ public sync_package.Mutex Mutex;
    internal int32 cnt;
    internal int32 id;
    internal bool recursive;
}

internal static int32 mutexInit(ж<libc.TLS> tls) { return SQLITE_OK; }

internal static int32 mutexEnd(ж<libc.TLS> tls) { return SQLITE_OK; }

internal static uintptr /*r*/ mutexAlloc(ж<libc.TLS> tls, int32 typ) {
    uintptr r = default;

    switch (typ) {
    case var ᴛ when ᴛ == SQLITE_MUTEX_FAST: {
        r = libc.Xcalloc(tls, 1, (typesꓸSize_t)/* unsafe.Sizeof(mutex{}) */ (uintptr)20);
        return r;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_RECURSIVE: {
        r = libc.Xcalloc(tls, 1, (typesꓸSize_t)/* unsafe.Sizeof(mutex{}) */ (uintptr)20);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)r)).Value.recursive = true;
        return r;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_MASTER: {
        return mutexMaster;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_MEM: {
        return mutexMem;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_OPEN: {
        return mutexOpen;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_PRNG: {
        return mutexPRNG;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_LRU: {
        return mutexLRU;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_PMEM: {
        return mutexPMem;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_APP1: {
        return mutexApp1;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_APP2: {
        return mutexApp2;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_APP3: {
        return mutexApp3;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_VFS1: {
        return mutexVFS1;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_VFS2: {
        return mutexVFS2;
    }
    case var ᴛ when ᴛ == SQLITE_MUTEX_STATIC_VFS3: {
        return mutexVFS3;
    }
    default: {
        return 0;
    }}

}

internal static void mutexFree(ж<libc.TLS> tls, uintptr m) {
    libc.Xfree(tls, m);
}

internal static void mutexEnter(ж<libc.TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (m == 0) {
        return;
    }

    if (!(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.recursive) {
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Lock();
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, tls.ID);
        return;
    }

    if (atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid) == tls.ID) {
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt++;
        return;
    }

    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Lock();

    atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, tls.ID);
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt = 1;
}

internal static int32 mutexTry(ж<libc.TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (m == 0) {
        return SQLITE_OK;
    }

    if (!(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.recursive) {
        if ((@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).TryLock()) {
            atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, tls.ID);
            return SQLITE_OK;
        }

        return SQLITE_BUSY;
    }

    if (atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid) == tls.ID) {
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt++;
        return SQLITE_OK;
    }

    if ((@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).TryLock()) {
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, tls.ID);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt = 1;
        return SQLITE_OK;
    }

    return SQLITE_BUSY;
}

internal static void mutexLeave(ж<libc.TLS> tls, uintptr m) {
    if (m == 0) {
        return;
    }

    if (!(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.recursive) {
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, 0);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Unlock();
        return;
    }

    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt--;

    if ((@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt == 0) {
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, 0);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Unlock();
    }
}

internal static int32 mutexHeld(ж<libc.TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (m == 0) {
        return 1;
    }

    return libc.Bool32(atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid) == tls.ID);
}

internal static int32 mutexNotheld(ж<libc.TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (m == 0) {
        return 1;
    }

    return libc.Bool32(atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid) != tls.ID);
}

} // end sqlite3_package
