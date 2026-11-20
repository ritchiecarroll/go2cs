// Copyright 2021 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using pthread = go.modernc.org.libc.pthread_package;
using go.modernc.org.libc;

partial class libc_package {

partial struct pthreadAttr {
    internal int32 detachState;
}

public static partial int32 Xpthread_attr_init(ж<TLS> t, uintptr pAttr) {
    if (__ccgo_strace) {
        trc("t=%v pAttr=%v, (%v:)", t.OrTypedNil(), pAttr, origin(2));
    }
    (@unsafe.Pointer.To<pthreadAttr>((@unsafe.Pointer)pAttr)).Value = new pthreadAttr();
    return 0;
}

public static partial int32 Xpthread_mutex_init(ж<TLS> t, uintptr pMutex, uintptr pAttr) {
    if (__ccgo_strace) {
        trc("t=%v pAttr=%v, (%v:)", t.OrTypedNil(), pAttr, origin(2));
    }
    nint typ = pthread.PTHREAD_MUTEX_DEFAULT;
    if (pAttr != 0) {
        typ = (nint)X__ccgo_pthreadMutexattrGettype(t, pAttr);
    }
    setMutex(pMutex, newMutex(typ));
    return 0;
}

public static int32 Xpthread_atfork(ж<TLS> tls, uintptr prepare, uintptr parent, uintptr child) {
    return 0;
}

public static int32 Xpthread_sigmask(ж<TLS> tls, int32 now, uintptr set, uintptr old) {
    return 0;
}

} // end libc_package
