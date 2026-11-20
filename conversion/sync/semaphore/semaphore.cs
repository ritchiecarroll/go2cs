// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sync;

using list = container.list_package;
using context = context_package;
using sync = sync_package;
using container;
using static semaphore_package;

partial class semaphore_package {

partial struct waiter {
    internal int64 n;
    internal channel/*<-*/<EmptyStruct> ready = channel/*<-*/<EmptyStruct>.SendOnly;
}

public static ж<Weighted> NewWeighted(int64 n) {
    var w = Ꮡ(new Weighted(size: n));
    return w;
}

partial struct Weighted {
    internal int64 size;
    internal int64 cur;
    internal sync.Mutex mu;
    internal list.List waiters;
}

} partial class semaphoreᴛWeighted {

public static error Acquire(this ж<Weighted> Ꮡs, context.Context ctx, int64 n) {
    ref var s = ref Ꮡs.DerefOrNull();

    var done = ctx.Done();

    s.mu.Lock();
    var doneᴛ1 = done;
    switch (trySelect(ᐸꟷ(doneᴛ1, ꓸꓸꓸ))) {
    case 0 when doneᴛ1.ꟷᐳ(out _): {
        s.mu.Unlock();
        return ctx.Err();
    }
    default: {
        break;
    }}
    if (s.size - s.cur >= n && s.waiters.Len() == 0) {
        s.cur += n;
        s.mu.Unlock();
        return default;
    }

    if (n > s.size) {
        s.mu.Unlock();
        ᐸꟷ(done);
        return ctx.Err();
    }

    var ready = new channel<EmptyStruct>(0);
    var w = new waiter(n: n, ready: ready);
    var elem = Ꮡs.Ꮡwaiters.PushBack(w);
    s.mu.Unlock();

    var doneᴛ2 = done;
    var readyᴛ1 = ready;
    switch (select(ᐸꟷ(doneᴛ2, ꓸꓸꓸ), ᐸꟷ(readyᴛ1, ꓸꓸꓸ))) {
    case 0 when doneᴛ2.ꟷᐳ(out _): {
        s.mu.Lock();
        var readyᴛ2 = ready;
        switch (trySelect(ᐸꟷ(readyᴛ2, ꓸꓸꓸ))) {
        case 0 when readyᴛ2.ꟷᐳ(out _): {
            s.cur -= n;
            Ꮡs.notifyWaiters();
            break;
        }
        default: {
            var isFront = s.waiters.Front() == elem;
            Ꮡs.Ꮡwaiters.Remove(elem);
            if (isFront && s.size > s.cur) {
                Ꮡs.notifyWaiters();
            }
            break;
        }}
        s.mu.Unlock();
        return ctx.Err();
    }
    case 1 when readyᴛ1.ꟷᐳ(out _): {
        var doneᴛ3 = done;
        switch (trySelect(ᐸꟷ(doneᴛ3, ꓸꓸꓸ))) {
        case 0 when doneᴛ3.ꟷᐳ(out _): {
            Ꮡs.Release(n);
            return ctx.Err();
        }
        default: {
            break;
        }}
        return default;
    }}
    return default;
}

public static bool TryAcquire(this ж<Weighted> Ꮡs, int64 n) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.mu.Lock();
    var success = s.size - s.cur >= n && s.waiters.Len() == 0;
    if (success) {
        s.cur += n;
    }
    s.mu.Unlock();
    return success;
}

public static void Release(this ж<Weighted> Ꮡs, int64 n) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.mu.Lock();
    s.cur -= n;
    if (s.cur < 0) {
        s.mu.Unlock();
        throw panic("semaphore: released more than held");
    }
    Ꮡs.notifyWaiters();
    s.mu.Unlock();
}

internal static void notifyWaiters(this ж<Weighted> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    for (;;) {
        var next = s.waiters.Front();
        if (next == nil) {
            break;
        }

        var w = (~next).Value._<waiter>();
        if (s.size - s.cur < w.n) {
            break;
        }

        s.cur += w.n;
        Ꮡs.Ꮡwaiters.Remove(next);
        close(w.ready);
    }
}

} // end semaphoreᴛWeighted
