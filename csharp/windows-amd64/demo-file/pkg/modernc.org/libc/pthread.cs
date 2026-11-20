// Copyright 2021 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using runtime = runtime_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @unsafe = unsafe_package;
using cpu = golang.org.x.sys.cpu_package;
using errno = go.modernc.org.libc.errno_package;
using pthread = go.modernc.org.libc.pthread_package;
using types = go.modernc.org.libc.sys.types_package;
using ctime = go.modernc.org.libc.time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;
using go.sync;
using golang.org.x.sys;
using static libc_package;

partial class libc_package {

internal static map<int32, ж<TLS>> threads = new map<int32, ж<TLS>>{};
internal static readonly ж<sync.Mutex> ᏑthreadsMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex threadsMu => ref ((StaticBox<sync.Mutex>)ᏑthreadsMu).ValueSlot;
internal static pthreadꓸPthread_key_t threadKey;
internal static map<pthreadꓸPthread_key_t, slice<uintptr>> threadKeyDestructors = new map<pthreadꓸPthread_key_t, slice<uintptr>>{};
internal static readonly ж<sync.Mutex> ᏑthreadsKeysMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex threadsKeysMu => ref ((StaticBox<sync.Mutex>)ᏑthreadsKeysMu).ValueSlot;
internal static map<uintptr, ж<cond>> conds = new map<uintptr, ж<cond>>{};
internal static readonly ж<sync.Mutex> ᏑcondsMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex condsMu => ref ((StaticBox<sync.Mutex>)ᏑcondsMu).ValueSlot;

partial struct TLS {
    internal uintptr errnop;
    internal slice<slice<uintptr>> allocaStack;
    internal slice<uintptr> allocas;
    internal slice<uintptr> jumpBuffers;
    internal uint32 lastError;
    /*embed*/ internal pthreadData pthreadData;
    internal nint sp;
    internal stackHeader stack;
    public int32 ID;
    internal int32 reentryGuard;
    internal int32 stackHeaderBalance;
}

internal static readonly ж<int32> Ꮡerrno0 = new StaticBox<int32>(default(int32));
internal static ref int32 errno0 => ref ((StaticBox<int32>)Ꮡerrno0).ValueSlot;

public static partial ж<TLS> NewTLS() {
    return newTLS(false);
}

internal static ж<TLS> newTLS(bool detached) {
    ref var id = ref heap<int32>(atomic.AddInt32(Ꮡtid, 1), out var Ꮡid);
    var t = Ꮡ(new TLS(ID: id, errnop: (uintptr)Ꮡerrno0));
    t.ᏑpthreadData.init(t, detached);
    if (memgrind) {
        atomic.AddInt32(ᏑtlsBalance, 1);
    }
    t.errnop = t.Alloc((nint)/* unsafe.Sizeof(int32(0)) */ (uintptr)4);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)t.errnop)).Value = 0;
    return t;
}

} partial class libcᴛTLS {

public static nint StackSlots(this ref TLS tls) {
    return tls.sp;
}

internal static partial uintptr /*r*/ alloca(this ж<TLS> Ꮡt, size_t n) {
    uintptr r = default;
    ref var t = ref Ꮡt.DerefOrNull();

    r = Xmalloc(Ꮡt, n);
    t.allocas = append(t.allocas, r);
    return r;
}

public static Action FreeAlloca(this ж<TLS> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    t.allocaStack = append(t.allocaStack, t.allocas);
    t.allocas = default;
    return () => {
        foreach (var (_, v) in range(Ꮡt.Value.allocas)) {
            Xfree(Ꮡt, v);
        }
        nint n = len(Ꮡt.Value.allocaStack);
        Ꮡt.Value.allocas = Ꮡt.Value.allocaStack[n - 1];
        Ꮡt.Value.allocaStack = Ꮡt.Value.allocaStack.slice(0, n - 1);
    };
}

public static void PushJumpBuffer(this ref TLS tls, uintptr jb) {
    tls.jumpBuffers = append(tls.jumpBuffers, jb);
}

} partial class libc_package {

partial struct LongjmpRetval {
    public uintptr JumpBuffer;
    public int32 Val;
}

} partial class libcᴛTLS {

public static void PopJumpBuffer(this ref TLS tls, uintptr jb) {
    nint n = len(tls.jumpBuffers);
    if (n == 0 || tls.jumpBuffers[n - 1] != jb) {
        throw panic(todo("unsupported setjmp/longjmp usage"));
    }

    tls.jumpBuffers = tls.jumpBuffers.slice(0, n - 1);
}

public static void Longjmp(this ref TLS tls, uintptr jb, int32 val) {
    nint i = len(tls.jumpBuffers) - 1;
    for (; i >= 0 && tls.jumpBuffers[i] != jb; i--) {
    }
    if (i < 0) {
        throw panic(todo("unsupported setjmp/longjmp usage"));
    }

    tls.jumpBuffers = appendꓸꓸꓸ(tls.jumpBuffers.slice(0, i), tls.jumpBuffers.slice(i + 1));
    if (val == 0) {
        val = 1;
    }
    throw panic(new LongjmpRetval(JumpBuffer: jb, Val: val));
}

} partial class libc_package {

public static partial uintptr Xalloca(ж<TLS> tls, size_t size) {
    if (__ccgo_strace) {
        trc("tls=%v size=%v, (%v:)", tls.OrTypedNil(), size, origin(2));
    }
    return tls.alloca(size);
}

public static partial uintptr X__builtin_alloca(ж<TLS> tls, size_t size) {
    if (__ccgo_strace) {
        trc("tls=%v size=%v, (%v:)", tls.OrTypedNil(), size, origin(2));
    }
    return Xalloca(tls, size);
}

partial struct pthreadData {
    internal channel<EmptyStruct> done;
    internal map<pthreadꓸPthread_key_t, uintptr> kv;
    internal uintptr retVal;
    internal channel<EmptyStruct> wait;
    internal bool detached;
}

} partial class libcᴛpthreadData {

internal static void init(this ж<pthreadData> Ꮡd, ж<TLS> Ꮡt, bool detached) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();
        ref var t = ref Ꮡt.DerefOrNull();

        d.detached = detached;
        d.wait = new channel<EmptyStruct>(1);
        if (detached) {
            return;
        }

        d.done = new channel<EmptyStruct>(0);

        ᏑthreadsMu.Lock();

        defer(ᏑthreadsMu.Unlock, ref ᒐ);

        threads[t.ID] = Ꮡt;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void close(this ref pthreadData d, ж<TLS> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ᏑthreadsMu.Lock();

        defer(ᏑthreadsMu.Unlock, ref ᒐ);

        delete(threads, t.ID);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} partial class libc_package {

public static partial int32 Xpthread_attr_destroy(ж<TLS> t, uintptr pAttr) {
    if (__ccgo_strace) {
        trc("t=%v pAttr=%v, (%v:)", t.OrTypedNil(), pAttr, origin(2));
    }
    return 0;
}

public static partial int32 Xpthread_attr_setscope(ж<TLS> t, uintptr pAttr, int32 contentionScope) {
    if (__ccgo_strace) {
        trc("t=%v pAttr=%v contentionScope=%v, (%v:)", t.OrTypedNil(), pAttr, contentionScope, origin(2));
    }
    switch (contentionScope) {
    case var ᴛ when ᴛ == pthread.PTHREAD_SCOPE_SYSTEM: {
        return 0;
    }
    default: {
        throw panic(todo("", contentionScope));
        break;
    }}

}

public static partial int32 Xpthread_attr_setstacksize(ж<TLS> t, uintptr attr, typesꓸSize_t stackSize) {
    if (__ccgo_strace) {
        trc("t=%v attr=%v stackSize=%v, (%v:)", t.OrTypedNil(), attr, stackSize, origin(2));
    }
    return 0;
}

partial struct cond {
    /*embed*/ public sync_package.Mutex Mutex;
    internal map<ж<TLS>, EmptyStruct> waiters;
}

internal static ж<cond> newCond() {
    return Ꮡ(new cond(
        waiters: new map<ж<TLS>, EmptyStruct>{}
    ));
}

} partial class libcᴛcond {

internal static int32 signal(this ж<cond> Ꮡc, bool all) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (Ꮡc == nil) {
            return errno.EINVAL;
        }

        Ꮡc.of(cond.ᏑMutex).Lock();

        ᒐd1 = true;

        foreach (var (tls, _) in c.waiters) {
            tls.wait.ᐸꟷ(new EmptyStruct());
            delete(c.waiters, tls);
            if (!all) {
                break;
            }
        }
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.of(cond.ᏑMutex).Unlock(); ᒐ.Run(); }
}

} partial class libc_package {

public static partial int32 Xpthread_cond_init(ж<TLS> t, uintptr pCond, uintptr pAttr) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v pAttr=%v, (%v:)", t.OrTypedNil(), pAttr, origin(2));
        }
        if (pCond == 0) {
            return errno.EINVAL;
        }

        if (pAttr != 0) {
            throw panic(todo("%#x %#x", pCond, pAttr));
        }

        ᏑcondsMu.Lock();

        defer(ᏑcondsMu.Unlock, ref ᒐ);

        conds[pCond] = newCond();
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial int32 Xpthread_cond_destroy(ж<TLS> t, uintptr pCond) {
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v pCond=%v, (%v:)", t.OrTypedNil(), pCond, origin(2));
        }
        if (pCond == 0) {
            return errno.EINVAL;
        }

        ᏑcondsMu.Lock();

        defer(ᏑcondsMu.Unlock, ref ᒐ);

        var cond = conds[pCond];
        if (cond == nil) {
            return errno.EINVAL;
        }

        cond.of(libc_package.cond.ᏑMutex).Lock();

        var condʗ1 = cond;
        defer(condʗ1.of(libc_package.cond.ᏑMutex).Unlock, ref ᒐ);

        if (len(cond.waiters) != 0) {
            return errno.EBUSY;
        }

        delete(conds, pCond);
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial int32 Xpthread_cond_signal(ж<TLS> t, uintptr pCond) {
    if (__ccgo_strace) {
        trc("t=%v pCond=%v, (%v:)", t.OrTypedNil(), pCond, origin(2));
    }
    return condSignal(pCond, false);
}

public static partial int32 Xpthread_cond_broadcast(ж<TLS> t, uintptr pCond) {
    if (__ccgo_strace) {
        trc("t=%v pCond=%v, (%v:)", t.OrTypedNil(), pCond, origin(2));
    }
    return condSignal(pCond, true);
}

internal static int32 condSignal(uintptr pCond, bool all) {
    if (pCond == 0) {
        return errno.EINVAL;
    }

    ᏑcondsMu.Lock();
    var cond = conds[pCond];
    ᏑcondsMu.Unlock();

    return cond.signal(all);
}

public static partial int32 Xpthread_cond_wait(ж<TLS> Ꮡt, uintptr pCond, uintptr pMutex) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v pMutex=%v, (%v:)", Ꮡt.OrTypedNil(), pMutex, origin(2));
    }
    if (pCond == 0) {
        return errno.EINVAL;
    }

    ᏑcondsMu.Lock();
    var cond = conds[pCond];
    if (cond == nil) {
        cond = newCond();
        conds[pCond] = cond;
    }

    cond.of(libc_package.cond.ᏑMutex).Lock();
    cond.waiters[Ꮡt] = new EmptyStruct();
    cond.of(libc_package.cond.ᏑMutex).Unlock();

    ᏑcondsMu.Unlock();

    var mu = lookupMutex(pMutex);

    mu.unlock();
    ᐸꟷ(t.wait);
    mu.@lock(t.ID);
    return 0;
}

public static partial int32 Xpthread_cond_timedwait(ж<TLS> Ꮡt, uintptr pCond, uintptr pMutex, uintptr pAbsTime) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (__ccgo_strace) {
            trc("t=%v pAbsTime=%v, (%v:)", Ꮡt.OrTypedNil(), pAbsTime, origin(2));
        }
        if (pCond == 0) {
            return errno.EINVAL;
        }

        var deadlineSecs = (@unsafe.Pointer.To<timeꓸTimespec>((@unsafe.Pointer)pAbsTime)).Value.Ftv_sec;
        var deadlineNsecs = (@unsafe.Pointer.To<timeꓸTimespec>((@unsafe.Pointer)pAbsTime)).Value.Ftv_nsec;
        var deadline = time.Unix((int64)deadlineSecs, (int64)deadlineNsecs);
        var d = deadline.Sub(time.Now());
        if (d <= 0) {
            return errno.ETIMEDOUT;
        }

        ᏑcondsMu.Lock();
        var cond = conds[pCond];
        if (cond == nil) {
            cond = newCond();
            conds[pCond] = cond;
        }

        cond.of(libc_package.cond.ᏑMutex).Lock();
        cond.waiters[Ꮡt] = new EmptyStruct();
        cond.of(libc_package.cond.ᏑMutex).Unlock();

        ᏑcondsMu.Unlock();

        var mu = lookupMutex(pMutex);

        var to = time.After(d);
        mu.unlock();

        var muʗ1 = mu;
        defer(muʗ1.@lock, t.ID, ref ᒐ);

        var selᴛ1 = t.wait;
        var toᴛ1 = to;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(toᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            return 0;
        }
        case 1 when toᴛ1.ꟷᐳ(out _): {
            cond.of(libc_package.cond.ᏑMutex).Lock();

            var condʗ1 = cond;
            defer(condʗ1.of(libc_package.cond.ᏑMutex).Unlock, ref ᒐ);

            delete(cond.waiters, Ꮡt);
            var selᴛ3 = t.wait;
            switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                break;
            }
            default: {
                break;
            }}
            return errno.ETIMEDOUT;
        }}
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static readonly ж<array<mutexShard>> ᏑmutexShards = new StaticBox<array<mutexShard>>(new array<mutexShard>(64, () => new(nil)));
internal static ref array<mutexShard> mutexShards => ref ((StaticBox<array<mutexShard>>)ᏑmutexShards).ValueSlot;

partial struct mutexShard {
    /*embed*/ public sync_package.Mutex Mutex;
    internal map<uintptr, ж<mutex>> m;
    internal cpu.CacheLinePad _;
}

[GoInit] internal static void initΔ3() {
    foreach (var i in range(len(mutexShards))) {
        mutexShards[i].m = new map<uintptr, ж<mutex>>{};
    }
}

internal static ж<mutexShard> mutexShardOf(uintptr pMutex) {
    return ᏑmutexShards.at<mutexShard>((ulong)(((uint64)pMutex * 0x9e3779b97f4a7c15UL >> 58)));
}

internal static ж<mutex> lookupMutex(uintptr pMutex) {
    var s = mutexShardOf(pMutex);
    s.of(mutexShard.ᏑMutex).Lock();
    var mu = s.m[pMutex];
    s.of(mutexShard.ᏑMutex).Unlock();
    return mu;
}

internal static void setMutex(uintptr pMutex, ж<mutex> mu) {
    var s = mutexShardOf(pMutex);
    s.of(mutexShard.ᏑMutex).Lock();
    if (mu == nil) {
        delete(s.m, pMutex);
    } else {
        s.m[pMutex] = mu;
    }
    s.of(mutexShard.ᏑMutex).Unlock();
}

partial struct mutex {
    /*embed*/ public sync_package.Mutex Mutex;
    internal nint typ;
    internal sync.Mutex wait;
    internal int32 id;
    internal int32 cnt;
    internal bool robust;
}

internal static ж<mutex> newMutex(nint typ) {
    return Ꮡ(new mutex(
        typ: typ
    ));
}

} partial class libcᴛmutex {

internal static int32 @lock(this ж<mutex> Ꮡm, int32 id) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.robust) {
        throw panic(todo(""));
    }

    var exprᴛ1 = m.typ;
    var matchᴛ1 = false;
    var matchᴛ2 = exprᴛ1 == pthread.PTHREAD_MUTEX_NORMAL || exprᴛ1 == pthread.PTHREAD_MUTEX_RECURSIVE;
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == pthread.PTHREAD_MUTEX_NORMAL) {
        Ꮡm.of(mutex.ᏑMutex).Lock();
        m.id = id;
        return 0;
    }
    if (exprᴛ1 == pthread.PTHREAD_MUTEX_RECURSIVE) { matchᴛ1 = true;
        for (;;) {
            Ꮡm.of(mutex.ᏑMutex).Lock();
            switch (m.id) {
            case 0: {
                m.cnt = 1;
                m.id = id;
                m.wait.Lock();
                Ꮡm.of(mutex.ᏑMutex).Unlock();
                return 0;
            }
            case var ᴛ when ᴛ == id: {
                m.cnt++;
                Ꮡm.of(mutex.ᏑMutex).Unlock();
                return 0;
            }}

            Ꮡm.of(mutex.ᏑMutex).Unlock();
            m.wait.Lock();
            m.wait.Unlock();
        }
    }
    return default;

}

internal static int32 tryLock(this ж<mutex> Ꮡm, int32 id) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.robust) {
        throw panic(todo(""));
    }

    var exprᴛ1 = m.typ;
    var matchᴛ1 = false;
    var matchᴛ2 = exprᴛ1 == pthread.PTHREAD_MUTEX_NORMAL || exprᴛ1 == pthread.PTHREAD_MUTEX_RECURSIVE;
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == pthread.PTHREAD_MUTEX_NORMAL) {
        if (Ꮡm.of(mutex.ᏑMutex).TryLock()) {
            m.id = id;
            return 0;
        }

        return errno.EBUSY;
    }
    if (exprᴛ1 == pthread.PTHREAD_MUTEX_RECURSIVE) { matchᴛ1 = true;
        Ꮡm.of(mutex.ᏑMutex).Lock();
        switch (m.id) {
        case 0: {
            m.cnt = 1;
            m.id = id;
            m.wait.Lock();
            Ꮡm.of(mutex.ᏑMutex).Unlock();
            return 0;
        }
        case var ᴛ when ᴛ == id: {
            m.cnt++;
            Ꮡm.of(mutex.ᏑMutex).Unlock();
            return 0;
        }}

        Ꮡm.of(mutex.ᏑMutex).Unlock();
        return errno.EBUSY;
    }
    return default;

}

internal static int32 unlock(this ж<mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (m.robust) {
        throw panic(todo(""));
    }

    var exprᴛ1 = m.typ;
    var matchᴛ1 = false;
    var matchᴛ2 = exprᴛ1 == pthread.PTHREAD_MUTEX_NORMAL || exprᴛ1 == pthread.PTHREAD_MUTEX_RECURSIVE;
    if (!matchᴛ2) { /* default: */
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == pthread.PTHREAD_MUTEX_NORMAL) {
        m.id = 0;
        Ꮡm.of(mutex.ᏑMutex).Unlock();
        return 0;
    }
    if (exprᴛ1 == pthread.PTHREAD_MUTEX_RECURSIVE) { matchᴛ1 = true;
        Ꮡm.of(mutex.ᏑMutex).Lock();
        m.cnt--;
        if (m.cnt == 0) {
            m.id = 0;
            m.wait.Unlock();
        }
        Ꮡm.of(mutex.ᏑMutex).Unlock();
        return 0;
    }
    return default;

}

} partial class libc_package {

public static partial int32 Xpthread_mutex_destroy(ж<TLS> t, uintptr pMutex) {
    if (__ccgo_strace) {
        trc("t=%v pMutex=%v, (%v:)", t.OrTypedNil(), pMutex, origin(2));
    }
    setMutex(pMutex, nil);
    return 0;
}

public static partial int32 Xpthread_mutex_lock(ж<TLS> Ꮡt, uintptr pMutex) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v pMutex=%v, (%v:)", Ꮡt.OrTypedNil(), pMutex, origin(2));
    }
    var s = mutexShardOf(pMutex);
    s.of(mutexShard.ᏑMutex).Lock();
    var mu = s.m[pMutex];
    if (mu == nil) {
        mu = newMutex((nint)X__ccgo_getMutexType(Ꮡt, pMutex));
        s.m[pMutex] = mu;
    }
    s.of(mutexShard.ᏑMutex).Unlock();
    return mu.@lock(t.ID);
}

public static partial int32 Xpthread_mutex_trylock(ж<TLS> Ꮡt, uintptr pMutex) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v pMutex=%v, (%v:)", Ꮡt.OrTypedNil(), pMutex, origin(2));
    }
    var s = mutexShardOf(pMutex);
    s.of(mutexShard.ᏑMutex).Lock();
    var mu = s.m[pMutex];
    if (mu == nil) {
        mu = newMutex((nint)X__ccgo_getMutexType(Ꮡt, pMutex));
        s.m[pMutex] = mu;
    }
    s.of(mutexShard.ᏑMutex).Unlock();
    return mu.tryLock(t.ID);
}

public static partial int32 Xpthread_mutex_unlock(ж<TLS> t, uintptr pMutex) {
    if (__ccgo_strace) {
        trc("t=%v pMutex=%v, (%v:)", t.OrTypedNil(), pMutex, origin(2));
    }
    return lookupMutex(pMutex).unlock();
}

public static int32 Xpthread_key_create(ж<TLS> t, uintptr pKey, uintptr destructor) {
    GoFrame ᒐ = default;
    try {
        ᏑthreadsKeysMu.Lock();

        defer(ᏑthreadsKeysMu.Unlock, ref ᒐ);

        threadKey++;
        var r = threadKey;
        if (destructor != 0) {
            threadKeyDestructors[r] = append(threadKeyDestructors[r], destructor);
        }
        (@unsafe.Pointer.To<pthreadꓸPthread_key_t>((@unsafe.Pointer)pKey)).Value = (pthreadꓸPthread_key_t)r;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static partial int32 Xpthread_key_delete(ж<TLS> Ꮡt, pthreadꓸPthread_key_t key) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v key=%v, (%v:)", Ꮡt.OrTypedNil(), key, origin(2));
    }
    var (_, ok) = t.kv[key, ꟷ]; if (ok) {
        delete(t.kv, key);
        return 0;
    }

    throw panic(todo(""));
}

public static partial uintptr Xpthread_getspecific(ж<TLS> Ꮡt, pthreadꓸPthread_key_t key) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v key=%v, (%v:)", Ꮡt.OrTypedNil(), key, origin(2));
    }
    return t.kv[key];
}

public static partial int32 Xpthread_setspecific(ж<TLS> Ꮡt, pthreadꓸPthread_key_t key, uintptr value) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v key=%v value=%v, (%v:)", Ꮡt.OrTypedNil(), key, value, origin(2));
    }
    if (t.kv == nil) {
        t.kv = new map<pthreadꓸPthread_key_t, uintptr>{};
    }
    t.kv[key] = value;
    return 0;
}

public static partial int32 Xpthread_create(ж<TLS> t, uintptr pThread, uintptr pAttr, uintptr startRoutine, uintptr arg) {
    if (__ccgo_strace) {
        trc("t=%v arg=%v, (%v:)", t.OrTypedNil(), arg, origin(2));
    }
    var fn = GoFuncCookie.Func<Func<ж<TLS>, uintptr, uintptr>>(startRoutine);
    var detached = pAttr != 0 && X__ccgo_pthreadAttrGetDetachState(t, pAttr) == pthread.PTHREAD_CREATE_DETACHED;
    var tls = newTLS(detached);
    (@unsafe.Pointer.To<pthreadꓸPthread_t>((@unsafe.Pointer)pThread)).Value = (pthreadꓸPthread_t)tls.ID;

    var fnʗ1 = fn;
    var tlsʗ1 = tls;
    goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
        Xpthread_exit(tlsʗ1, fnʗ1(tlsʗ1, arg));
    });

    return 0;
}

public static partial int32 Xpthread_detach(ж<TLS> t, pthreadꓸPthread_t thread) {
    if (__ccgo_strace) {
        trc("t=%v thread=%v, (%v:)", t.OrTypedNil(), thread, origin(2));
    }
    ᏑthreadsMu.Lock();
    var tls = threads[(int32)thread];
    if (tls == nil) {
        ᏑthreadsMu.Unlock();
        return errno.ESRCH;
    }
    tls.detached = true;
    ᏑthreadsMu.Unlock();
    return 0;
}

public static partial int32 Xpthread_equal(ж<TLS> t, pthreadꓸPthread_t t1, pthreadꓸPthread_t t2) {
    if (__ccgo_strace) {
        trc("t=%v t2=%v, (%v:)", t.OrTypedNil(), t2, origin(2));
    }
    return Bool32(t1 == t2);
}

public static partial void Xpthread_exit(ж<TLS> Ꮡt, uintptr value) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v value=%v, (%v:)", Ꮡt.OrTypedNil(), value, origin(2));
    }
    t.retVal = value;

    foreach (var (k, v) in t.kv) {
        if (v == 0) {
            continue;
        }

        ᏑthreadsKeysMu.Lock();
        var destructors = threadKeyDestructors[k];
        ᏑthreadsKeysMu.Unlock();

        foreach (var (_, destructor) in range(destructors)) {
            delete(t.kv, k);
            throw panic(todo("%#x", destructor));
        }
    }

    switch (ᐧ) {
    case {} when t.detached: {
        ᏑthreadsMu.Lock();
        delete(threads, t.ID);
        ᏑthreadsMu.Unlock();
        break;
    }
    default: {
        close(t.done);
        break;
    }}

    runtime.Goexit();
}

public static partial int32 Xpthread_join(ж<TLS> t, pthreadꓸPthread_t thread, uintptr pValue) {
    if (__ccgo_strace) {
        trc("t=%v thread=%v pValue=%v, (%v:)", t.OrTypedNil(), thread, pValue, origin(2));
    }
    ᏑthreadsMu.Lock();
    var tls = threads[(int32)thread];
    if (tls == nil) {
        ᏑthreadsMu.Unlock();
        return errno.ESRCH;
    }
    delete(threads, (int32)thread);
    ᏑthreadsMu.Unlock();
    ᐸꟷ(tls.done);
    if (pValue != 0) {
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)pValue)).Value = tls.retVal;
    }
    return 0;
}

public static partial pthreadꓸPthread_t Xpthread_self(ж<TLS> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v, (%v:)", Ꮡt.OrTypedNil(), origin(2));
    }
    return (pthreadꓸPthread_t)t.ID;
}

} // end libc_package
