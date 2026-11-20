// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using runtime = runtime_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @unsafe = unsafe_package;
using unix = golang.org.x.sys.unix_package;
using go.sync;
using golang.org.x.sys;

partial class libc_package {

partial struct pthreadAttr {
    internal int32 detachState;
}

partial struct pthreadCleanupItem {
    internal uintptr routine, arg;
}

partial struct pthreadMutex {
    /*embed*/ public sync_package.Mutex Mutex;
    internal int32 count;
    internal uint32 typ;
    internal int32 owner;
}

partial struct pthreadConds {
    /*embed*/ public sync_package.Mutex Mutex;
    internal map<uintptr, slice<channel<EmptyStruct>>> conds;
}

internal static array<byte> _ᴛ6ʗ = new(20);
internal static array<byte> _ᴛ7ʗ = new(52);
internal static readonly ж<sync.Mutex> ᏑpthreadKeysMutex = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex pthreadKeysMutex => ref ((StaticBox<sync.Mutex>)ᏑpthreadKeysMutex).ValueSlot;
internal static slice<uintptr> pthreadKeyDestructors;
internal static slice<Tpthread_key_t> pthreadKeysFree;
internal static readonly ж<pthreadConds> Ꮡconds = new StaticBox<pthreadConds>(new pthreadConds(conds: new map<uintptr, slice<channel<EmptyStruct>>>{}));
internal static ref pthreadConds conds => ref ((StaticBox<pthreadConds>)Ꮡconds).ValueSlot;

internal static int32 _pthread_setcancelstate(ref TLS tls, int32 @new, uintptr old) {
    if ((uint32)@new > 2) {
        return EINVAL;
    }

    var p = tls.pthread + /* unsafe.Offsetof(t__pthread{}.Fcanceldisable) */ (uintptr)64;
    if (old != 0) {
        var r = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p);
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)old)).Value = (int32)(byte)r;
    }
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)p)).Value = @new;
    return 0;
}

public static uintptr Xpthread_getspecific(ж<TLS> Ꮡtls, Tpthread_key_t k) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    return tls.pthreadKeyValues[k];
}

public static int32 Xpthread_setspecific(ж<TLS> Ꮡtls, Tpthread_key_t k, uintptr x) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    if (tls.pthreadKeyValues == nil) {
        tls.pthreadKeyValues = new map<Tpthread_key_t, uintptr>{};
    }
    tls.pthreadKeyValues[k] = x;
    return 0;
}

public static int32 Xpthread_key_create(ж<TLS> tls, uintptr k, uintptr dtor) {
    GoFrame ᒐ = default;
    try {
        ᏑpthreadKeysMutex.Lock();

        defer(ᏑpthreadKeysMutex.Unlock, ref ᒐ);

        Tpthread_key_t key = default;
        var l = (Tpthread_key_t)len(pthreadKeysFree);
        switch (ᐧ) {
        case {} when l is 0: {
            key = (Tpthread_key_t)len(pthreadKeyDestructors);
            pthreadKeyDestructors = append(pthreadKeyDestructors, dtor);
            break;
        }
        default: {
            key = pthreadKeysFree[l - 1];
            pthreadKeysFree = pthreadKeysFree.slice(0, (nint)(l - 1));
            pthreadKeyDestructors[key] = dtor;
            break;
        }}

        (@unsafe.Pointer.To<Tpthread_key_t>((@unsafe.Pointer)k)).Value = key;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 Xpthread_key_delete(ж<TLS> tls, Tpthread_key_t k) {
    GoFrame ᒐ = default;
    try {
        ᏑpthreadKeysMutex.Lock();

        defer(ᏑpthreadKeysMutex.Unlock, ref ᒐ);

        pthreadKeysFree = append(pthreadKeysFree, k);
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial int32 Xpthread_create(ж<TLS> tls, uintptr res, uintptr attrp, uintptr entry, uintptr arg) {
    pthreadAttr attr = default;
    if (attrp != 0) {
        attr = ~@unsafe.Pointer.To<pthreadAttr>((@unsafe.Pointer)attrp);
    }

    var detachState = (int32)_DT_JOINABLE;
    if (attr.detachState != 0) {
        detachState = _DT_DETACHED;
    }
    var tls2 = NewTLS();
    tls2.ownsPthread = false;
    (@unsafe.Pointer.To<Tpthread_t>((@unsafe.Pointer)res)).Value = tls2.pthread;
    (@unsafe.Pointer.To<t__pthread>((@unsafe.Pointer)tls2.pthread)).Value.Fdetach_state = detachState;
    if (detachState == _DT_JOINABLE) {
        (@unsafe.Pointer.To<sync.Mutex>((@unsafe.Pointer)(tls2.pthread + /* unsafe.Offsetof(t__pthread{}.F__ccgo_join_mutex) */ (uintptr)200))).Lock();
    }

    var tls2ʗ1 = tls2;
    goǃ(() => {
        Xpthread_exit(tls2ʗ1, GoFuncCookie.Func<Func<ж<TLS>, uintptr, uintptr>>(entry)(tls2ʗ1, arg));
    });

    return 0;
}

public static void Xpthread_exit(ж<TLS> Ꮡtls, uintptr result) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    var state = atomic.LoadInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)(tls.pthread + /* unsafe.Offsetof(t__pthread{}.Fdetach_state) */ (uintptr)56)));
    (@unsafe.Pointer.To<t__pthread>((@unsafe.Pointer)tls.pthread)).Value.Fresult = result;
    switch (state) {
    case var ᴛ when ᴛ == _DT_JOINABLE || ᴛ == _DT_DETACHED: {
        break;
    }
    default: {
        throw panic(todo("", state));
        break;
    }}

    while (len(tls.pthreadCleanupItems) != 0) {
        Xpthread_cleanup_pop(Ꮡtls, 1);
    }
    for (;;) {
        var done = true;
        foreach (var (k, v) in tls.pthreadKeyValues) {
            if (v != 0) {
                delete(tls.pthreadKeyValues, k);
                ᏑpthreadKeysMutex.Lock();
                ref var d = ref heap<uintptr>(pthreadKeyDestructors[k], out var Ꮡd);
                ᏑpthreadKeysMutex.Unlock();
                if (d != 0) {
                    done = false;
                    GoFuncCookie.Func<Action<ж<TLS>, uintptr>>(d)(Ꮡtls, v);
                }
            }
        }
        if (done) {
            break;
        }
    }
    var mu = @unsafe.Pointer.To<sync.Mutex>((@unsafe.Pointer)(tls.pthread + /* unsafe.Offsetof(t__pthread{}.F__ccgo_join_mutex) */ (uintptr)200));
    mu.TryLock();
    mu.Unlock();
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)(tls.pthread + /* unsafe.Offsetof(t__pthread{}.Fdetach_state) */ (uintptr)56)), _DT_EXITED);
    Ꮡtls.Close();
    runtime.Goexit();
}

public static int32 /*r*/ Xpthread_join(ж<TLS> tls, Tpthread_t t, uintptr res) {
    if ((@unsafe.Pointer.To<t__pthread>((@unsafe.Pointer)t)).Value.Fdetach_state > _DT_JOINABLE) {
        return EINVAL;
    }

    (@unsafe.Pointer.To<sync.Mutex>((@unsafe.Pointer)(t + /* unsafe.Offsetof(t__pthread{}.F__ccgo_join_mutex) */ (uintptr)200))).Lock();
    if (res != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)res)).Value = (@unsafe.Pointer.To<t__pthread>((@unsafe.Pointer)t)).Value.Fresult;
    }
    return 0;
}

public static void Xpthread_cleanup_push(ж<TLS> tls, uintptr f, uintptr x) {
    X_pthread_cleanup_push(tls, 0, f, x);
}

internal static void __pthread_cleanup_push(ref TLS tls, uintptr _, uintptr f, uintptr x) {
    tls.pthreadCleanupItems = append(tls.pthreadCleanupItems, new pthreadCleanupItem(f, x));
}

public static void X_pthread_cleanup_push(ж<TLS> Ꮡtls, uintptr _, uintptr f, uintptr x) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    tls.pthreadCleanupItems = append(tls.pthreadCleanupItems, new pthreadCleanupItem(f, x));
}

public static void Xpthread_cleanup_pop(ж<TLS> tls, int32 run) {
    X_pthread_cleanup_pop(tls, 0, run);
}

internal static void __pthread_cleanup_pop(ж<TLS> tls, uintptr _, int32 run) {
    X_pthread_cleanup_pop(tls, 0, run);
}

public static void X_pthread_cleanup_pop(ж<TLS> Ꮡtls, uintptr _, int32 run) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    nint l = len(tls.pthreadCleanupItems);
    var item = tls.pthreadCleanupItems[l - 1];
    tls.pthreadCleanupItems = tls.pthreadCleanupItems.slice(0, l - 1);
    if (run != 0) {
        GoFuncCookie.Func<Action<ж<TLS>, uintptr>>(item.routine)(Ꮡtls, item.arg);
    }
}

public static int32 Xpthread_attr_init(ж<TLS> tls, uintptr a) {
    (@unsafe.Pointer.To<Tpthread_attr_t>((@unsafe.Pointer)a)).Value = new Tpthread_attr_t();
    return 0;
}

public static int32 Xpthread_attr_setscope(ж<TLS> tls, uintptr a, int32 scope) {
    switch (scope) {
    case var ᴛ when ᴛ == PTHREAD_SCOPE_SYSTEM: {
        return 0;
    }
    case var ᴛ when ᴛ == PTHREAD_SCOPE_PROCESS: {
        return ENOTSUP;
    }
    default: {
        return EINVAL;
    }}

}

public static int32 Xpthread_attr_setstacksize(ж<TLS> tls, uintptr a, Tsize_t stacksite) {
    return 0;
}

public static int32 /*r*/ Xpthread_attr_setdetachstate(ж<TLS> tls, uintptr a, int32 state) {
    if ((uint32)state > 1) {
        return EINVAL;
    }

    (@unsafe.Pointer.To<pthreadAttr>((@unsafe.Pointer)a)).Value.detachState = state;
    return 0;
}

public static int32 Xpthread_attr_getdetachstate(ж<TLS> tls, uintptr a, uintptr state) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)state)).Value = (@unsafe.Pointer.To<pthreadAttr>((@unsafe.Pointer)a)).Value.detachState;
    return 0;
}

public static int32 Xpthread_attr_destroy(ж<TLS> tls, uintptr a) {
    return 0;
}

public static uintptr Xpthread_self(ж<TLS> Ꮡtls) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    return tls.pthread;
}

public static int32 Xpthread_mutex_init(ж<TLS> tls, uintptr m, uintptr a) {
    (@unsafe.Pointer.To<Tpthread_mutex_t>((@unsafe.Pointer)m)).Value = new Tpthread_mutex_t();
    if (a != 0) {
        (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Value.typ = (@unsafe.Pointer.To<Tpthread_mutexattr_t>((@unsafe.Pointer)a)).Value.F__attr;
    }
    return 0;
}

public static int32 Xpthread_mutex_destroy(ж<TLS> tls, uintptr m) {
    (@unsafe.Pointer.To<Tpthread_mutex_t>((@unsafe.Pointer)m)).Value = new Tpthread_mutex_t();
    return 0;
}

public static int32 Xpthread_mutex_lock(ж<TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    var typ = (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Value.typ;
    switch (typ) {
    case var ᴛ when ᴛ == PTHREAD_MUTEX_NORMAL: {
        (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
        return 0;
    }
    case var ᴛ when ᴛ == PTHREAD_MUTEX_RECURSIVE: {
        if (atomic.CompareAndSwapInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0, tls.ID)) {
            atomic.StoreInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡcount, 1);
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
            return 0;
        }

        if (atomic.LoadInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner) == tls.ID) {
            atomic.AddInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡcount, 1);
            return 0;
        }

        for (;;) {
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
            if (atomic.CompareAndSwapInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0, tls.ID)) {
                atomic.StoreInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡcount, 1);
                return 0;
            }

            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Unlock();
        }
        break;
    }
    case var ᴛ when ᴛ == PTHREAD_MUTEX_ERRORCHECK: {
        if (atomic.LoadInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner) == tls.ID) {
            return EDEADLK;
        }

        if (atomic.CompareAndSwapInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0, tls.ID)) {
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
            return 0;
        }

        for (;;) {
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
            if (atomic.CompareAndSwapInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0, tls.ID)) {
                return 0;
            }

            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Unlock();
        }
        break;
    }
    default: {
        throw panic(todo("", typ));
        break;
    }}

}

public static int32 Xpthread_mutex_trylock(ж<TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    var typ = (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Value.typ;
    switch (typ) {
    case var ᴛ when ᴛ == PTHREAD_MUTEX_NORMAL: {
        if ((@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).TryLock()) {
            return 0;
        }

        return EBUSY;
    }
    case var ᴛ when ᴛ == PTHREAD_MUTEX_RECURSIVE: {
        if (atomic.LoadInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner) == tls.ID) {
            atomic.AddInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡcount, 1);
            return 0;
        }

        if (atomic.CompareAndSwapInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0, tls.ID)) {
            atomic.StoreInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡcount, 1);
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
            return 0;
        }

        return EBUSY;
    }
    case var ᴛ when ᴛ == PTHREAD_MUTEX_ERRORCHECK: {
        if (atomic.CompareAndSwapInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0, tls.ID)) {
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Lock();
            return 0;
        }

        return EBUSY;
    }
    default: {
        throw panic(todo("typ=%v", typ));
        break;
    }}

}

public static int32 Xpthread_mutex_unlock(ж<TLS> Ꮡtls, uintptr m) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    var typ = (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Value.typ;
    switch (typ) {
    case var ᴛ when ᴛ == PTHREAD_MUTEX_NORMAL: {
        (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Unlock();
        return 0;
    }
    case var ᴛ when ᴛ == PTHREAD_MUTEX_RECURSIVE: {
        if (atomic.LoadInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner) != tls.ID) {
            return EPERM;
        }

        if (atomic.AddInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡcount, -1) == 0) {
            atomic.StoreInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0);
            (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Unlock();
        }
        return 0;
    }
    case var ᴛ when ᴛ == PTHREAD_MUTEX_ERRORCHECK: {
        if (atomic.LoadInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner) != tls.ID) {
            return EPERM;
        }

        atomic.StoreInt32(@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m).Ꮡowner, 0);
        (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Unlock();
        return 0;
    }
    default: {
        throw panic(todo("", typ));
        break;
    }}

}

public static int32 Xpthread_cond_init(ж<TLS> tls, uintptr c, uintptr a) {
    (@unsafe.Pointer.To<Tpthread_cond_t>((@unsafe.Pointer)c)).Value = new Tpthread_cond_t();
    if (a != 0) {
        (@unsafe.Pointer.To<Tpthread_cond_t>((@unsafe.Pointer)c)).Value.F__u.F__i[4] = (int32)((@unsafe.Pointer.To<Tpthread_condattr_t>((@unsafe.Pointer)a)).Value.F__attr & 0x7fffffff);
    }

    Ꮡconds.of(pthreadConds.ᏑMutex).Lock();
    delete(conds.conds, c);
    Ꮡconds.of(pthreadConds.ᏑMutex).Unlock();
    return 0;
}

public static int32 /*r*/ Xpthread_cond_timedwait(ж<TLS> tls, uintptr c, uintptr m, uintptr ts) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        /*<-*/channel<time.Time> to = /*<-*/channel<time.Time>.RecvOnly;
        if (ts != 0) {
            var nsec = (int64)(@unsafe.Pointer.To<Ttimespec>((@unsafe.Pointer)ts)).Value.Ftv_nsec;
            if (nsec < 0 || nsec >= 1000000000) {
                r = EINVAL; goto ᒐdone;
            }

            ref var now = ref heap(new unix.Timespec(), out var Ꮡnow);
            var err = unix.ClockGettime((@unsafe.Pointer.To<Tpthread_cond_t>((@unsafe.Pointer)c)).Value.F__u.F__i[4], Ꮡnow); if (err != nil) {
                r = EINVAL; goto ᒐdone;
            }

            var d = ((time.Duration)((int64)(@unsafe.Pointer.To<Ttimespec>((@unsafe.Pointer)ts)).Value.Ftv_sec - (int64)now.Sec)) * time.Second + ((time.Duration)(nsec - (int64)now.Nsec));
            if (d <= 0) {
                r = ETIMEDOUT; goto ᒐdone;
            }

            to = time.After(d);
        }

        Ꮡconds.of(pthreadConds.ᏑMutex).Lock();
        ref var waiters = ref heap<slice<channel<EmptyStruct>>>(conds.conds[c], out var Ꮡwaiters);
        var ch = new channel<EmptyStruct>(1);
        waiters = append(waiters, ch);
        conds.conds[c] = waiters;
        Ꮡconds.of(pthreadConds.ᏑMutex).Unlock();

        var chʗ1 = ch;
        defer(() => {
            GoFrame ᒐ = default;
            try {
                Ꮡconds.of(pthreadConds.ᏑMutex).Lock();

                defer(Ꮡconds.of(pthreadConds.ᏑMutex).Unlock, ref ᒐ);

                Ꮡwaiters.ValueSlot = conds.conds[c];
                foreach (var (i, v) in range(Ꮡwaiters.ValueSlot)) {
                    if (v == chʗ1) {
                        conds.conds[c] = appendꓸꓸꓸ(Ꮡwaiters.ValueSlot.slice(0, i), Ꮡwaiters.ValueSlot.slice(i + 1));
                        return;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);

        {
            var typ = (@unsafe.Pointer.To<pthreadMutex>((@unsafe.Pointer)m)).Value.typ;
            switch (typ) {
            case var ᴛ when ᴛ == PTHREAD_MUTEX_NORMAL || ᴛ == PTHREAD_MUTEX_RECURSIVE || ᴛ == PTHREAD_MUTEX_ERRORCHECK: {
                r = Xpthread_mutex_unlock(tls, m); if (r != 0) {
                    goto ᒐdone;
                }

                var chᴛ1 = ch;
                var toᴛ1 = to;
                switch (select(ᐸꟷ(chᴛ1, ꓸꓸꓸ), ᐸꟷ(toᴛ1, ꓸꓸꓸ))) {
                case 0 when chᴛ1.ꟷᐳ(out _): {
                    break;
                }
                case 1 when toᴛ1.ꟷᐳ(out _): {
                    r = ETIMEDOUT;
                    break;
                }}
                Xpthread_mutex_lock(tls, m);
                goto ᒐdone;
            }
            default: {
                throw panic(todo("", typ));
                break;
            }}
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial int32 Xpthread_cond_wait(ж<TLS> tls, uintptr c, uintptr m) {
    return Xpthread_cond_timedwait(tls, c, m, 0);
}

public static int32 Xpthread_cond_signal(ж<TLS> tls, uintptr c) {
    return pthreadSignalN(ref (tls).DerefOrNull(), c, false);
}

internal static int32 pthreadSignalN(ref TLS tls, uintptr c, bool all) {
    Ꮡconds.of(pthreadConds.ᏑMutex).Lock();
    var waiters = conds.conds[c];
    var handle = waiters;
    if (len(waiters) != 0) {
        switch (ᐧ) {
        case {} when all: {
            delete(conds.conds, c);
            break;
        }
        default: {
            handle = handle[..1];
            conds.conds[c] = waiters[1..];
            break;
        }}

    }
    Ꮡconds.of(pthreadConds.ᏑMutex).Unlock();
    foreach (var (_, v) in range(handle)) {
        close(v);
    }
    return 0;
}

public static int32 Xpthread_cond_broadcast(ж<TLS> tls, uintptr c) {
    return pthreadSignalN(ref (tls).DerefOrNull(), c, true);
}

public static int32 Xpthread_cond_destroy(ж<TLS> tls, uintptr c) {
    return Xpthread_cond_broadcast(tls, c);
}

public static int32 Xpthread_atfork(ж<TLS> tls, uintptr prepare, uintptr parent, uintptr child) {
    return 0;
}

public static int32 Xpthread_mutexattr_init(ж<TLS> tls, uintptr a) {
    (@unsafe.Pointer.To<Tpthread_mutexattr_t>((@unsafe.Pointer)a)).Value = new Tpthread_mutexattr_t();
    return 0;
}

public static int32 Xpthread_mutexattr_destroy(ж<TLS> tls, uintptr a) {
    return 0;
}

public static int32 Xpthread_mutexattr_settype(ж<TLS> tls, uintptr a, int32 typ) {
    if ((uint32)typ > 2) {
        return EINVAL;
    }

    (@unsafe.Pointer.To<Tpthread_mutexattr_t>((@unsafe.Pointer)a)).Value.F__attr = (uint32)typ & 3;
    return 0;
}

public static int32 Xpthread_detach(ж<TLS> Ꮡtls, uintptr t) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    var state = atomic.SwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)(t + /* unsafe.Offsetof(t__pthread{}.Fdetach_state) */ (uintptr)56)), _DT_DETACHED);
    switch (state) {
    case var ᴛ when ᴛ == _DT_JOINABLE || ᴛ == _DT_EXITED || ᴛ == _DT_DETACHED: {
        return 0;
    }
    default: {
        throw panic(todo("", tls.ID, state));
        break;
    }}

}

public static int32 Xpthread_equal(ж<TLS> tls, uintptr t, uintptr u) {
    return Bool32(t == u);
}

internal static int32 _pthread_sigmask(ref TLS tls, int32 now, uintptr set, uintptr old) {
    return 0;
}

partial struct barrierState {
    internal sync.Mutex mu;
    internal ж<sync.Cond> cond;
    internal uint32 count;
    internal uint32 tripCount;
    internal uint32 generation;
}

internal static map<uintptr, ж<barrierState>> barriers = new map<uintptr, ж<barrierState>>{};
internal static readonly ж<sync.Mutex> ᏑbarriersMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex barriersMu => ref ((StaticBox<sync.Mutex>)ᏑbarriersMu).ValueSlot;

public static int32 Xpthread_barrier_init(ж<TLS> tls, uintptr barrier, uintptr attr, uint32 count) {
    GoFrame ᒐ = default;
    try {
        if (count == 0) {
            return EINVAL;
        }
        ᏑbarriersMu.Lock();
        defer(ᏑbarriersMu.Unlock, ref ᒐ);
        var state = Ꮡ(new barrierState(tripCount: count));
        state.cond = sync.NewCond(new sync.MutexжLocker(state.Ꮡmu));
        barriers[barrier] = state;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 Xpthread_barrier_destroy(ж<TLS> tls, uintptr barrier) {
    GoFrame ᒐ = default;
    try {
        ᏑbarriersMu.Lock();
        defer(ᏑbarriersMu.Unlock, ref ᒐ);
        delete(barriers, barrier);
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 Xpthread_barrier_wait(ж<TLS> tls, uintptr barrier) {
    ᏑbarriersMu.Lock();
    var state = barriers[barrier];
    ᏑbarriersMu.Unlock();
    if (state == nil) {
        return EINVAL;
    }
    state.Ꮡmu.Lock();
    var gen = state.generation;
    state.count++;
    if (state.count >= state.tripCount) {
        state.count = 0;
        state.generation++;
        state.cond.Broadcast();
        state.Ꮡmu.Unlock();
        return -1;
    }
    while (gen == state.generation) {
        state.cond.Wait();
    }
    state.Ꮡmu.Unlock();
    return 0;
}

public static int32 Xpthread_condattr_init(ж<TLS> tls, uintptr a) {
    (@unsafe.Pointer.To<Tpthread_condattr_t>((@unsafe.Pointer)a)).Value = new Tpthread_condattr_t();
    return 0;
}

public static int32 Xpthread_condattr_destroy(ж<TLS> tls, uintptr a) {
    return 0;
}

public static int32 Xpthread_condattr_setclock(ж<TLS> tls, uintptr a, Tclockid_t clk) {
    if (clk < 0 || (uint32)clk - 2 < 2) {
        return EINVAL;
    }

    var p = @unsafe.Pointer.To<Tpthread_condattr_t>((@unsafe.Pointer)a);
    p.Value.F__attr &= (uint32)(0x80000000U);
    p.Value.F__attr |= (uint32)((uint32)clk);
    return 0;
}

public static int32 Xpthread_condattr_getclock(ж<TLS> tls, uintptr a, uintptr clk) {
    (@unsafe.Pointer.To<Tclockid_t>((@unsafe.Pointer)clk)).Value = (Tclockid_t)((@unsafe.Pointer.To<Tpthread_condattr_t>((@unsafe.Pointer)a)).Value.F__attr & 0x7fffffff);
    return 0;
}

public static int32 Xpthread_condattr_setpshared(ж<TLS> tls, uintptr a, int32 pshared) {
    if ((uint32)pshared > 1) {
        return EINVAL;
    }

    var p = @unsafe.Pointer.To<Tpthread_condattr_t>((@unsafe.Pointer)a);
    p.Value.F__attr &= (uint32)(0x7fffffff);
    p.Value.F__attr |= (uint32)(((uint32)pshared << 31));
    return 0;
}

public static int32 Xpthread_condattr_getpshared(ж<TLS> tls, uintptr a, uintptr pshared) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)pshared)).Value = (int32)((@unsafe.Pointer.To<Tpthread_condattr_t>((@unsafe.Pointer)a)).Value.F__attr >> 31);
    return 0;
}

} // end libc_package
