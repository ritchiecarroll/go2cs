// Copyright 2019 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using go.modernc.org;
using go.modernc.org.libc.sys;
using go.sync;
using static sqlite_package;

partial class sqlite_package {

partial struct mutex {
    /*embed*/ public sync_package.Mutex Mutex;
    internal int32 woken;
}

} partial class sqliteᴛmutex {

internal static bool wake(this ж<mutex> Ꮡm) {
    if (!atomic.CompareAndSwapInt32(Ꮡm.Ꮡwoken, 0, 1)) {
        return false;
    }

    Ꮡm.of(mutex.ᏑMutex).Unlock();
    return true;
}

} partial class sqlite_package {

internal static uintptr mutexAlloc(ж<Δlibc.TLS> tls) {
    return Δlibc.Xcalloc(tls, 1, (typesꓸSize_t)/* unsafe.Sizeof(mutex{}) */ (uintptr)12);
}

internal static void mutexFree(ж<Δlibc.TLS> tls, uintptr m) { Δlibc.Xfree(tls, m); }

} // end sqlite_package
