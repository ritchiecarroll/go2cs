// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sync;

using list = container.list_package;
using context = context_package;
using sync = sync_package;
using container;

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

public static error Acquire(this ж<Weighted> Ꮡs, context.Context ctx, int64 n) {
    ref var s = ref Ꮡs.DerefOrNull();

    var done = ctx.Done();

    s.mu.Lock();
    var selᴛ1 = done;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        s.mu.Unlock();
        return ctx.Err();
    }
    default: {
        break;
    }}
    if (s.size - s.cur >= n && s.waiters.Len() == 0) {
        s.cur += n;
        s.mu.Unlock();
        return default!;
    }

    if (n > s.size) {
        s.mu.Unlock();
        ᐸꟷ(done);
        return ctx.Err();
    }

    var ready = new channel<EmptyStruct>(0);
    var w = new waiter(n: n, ready: ready);
    var elem = Ꮡs.of(Weighted.Ꮡwaiters).PushBack(w);
    s.mu.Unlock();

    var selᴛ2 = done;
    var selᴛ3 = ready;
    switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0 when selᴛ2.ꟷᐳ(out _): {
        s.mu.Lock();
        var selᴛ4 = ready;
        switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out _): {
            s.cur -= n;
            Ꮡs.notifyWaiters();
            break;
        }
        default: {
            var isFront = s.waiters.Front() == elem;
            Ꮡs.of(Weighted.Ꮡwaiters).Remove(elem);
            if (isFront && s.size > s.cur) {
                Ꮡs.notifyWaiters();
            }
            break;
        }}
        s.mu.Unlock();
        return ctx.Err();
    }
    case 1 when selᴛ3.ꟷᐳ(out _): {
        var selᴛ5 = done;
        switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
        case 0 when selᴛ5.ꟷᐳ(out _): {
            Ꮡs.Release(n);
            return ctx.Err();
        }
        default: {
            break;
        }}
        return default!;
    }}
    return default!;
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

    while (ᐧ) {
        var next = s.waiters.Front();
        if (next == nil) {
            break;
        }

        var w = (~next).Value._<waiter>();
        if (s.size - s.cur < w.n) {
            break;
        }

        s.cur += w.n;
        Ꮡs.of(Weighted.Ꮡwaiters).Remove(next);
        close(w.ready);
    }
}

} // end semaphore_package
