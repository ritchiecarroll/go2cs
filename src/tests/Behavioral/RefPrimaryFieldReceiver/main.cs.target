namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct counter {
    internal Δsync.Mutex mu;
    internal nint n;
}

internal static void bump(this ж<counter> Ꮡc, nint times) {
    ref var c = ref Ꮡc.DerefOrNull();

    for (nint i = 0; i < times; i++) {
        c.mu.Lock();
        c.n++;
        c.mu.Unlock();
    }
}

internal static nint get(this ж<counter> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        return c.n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static nint nBeforeTouch(this ж<counter> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        if (Ꮡc == nil) {
            return -1;
        }
        c.mu.Lock();
        ᒐd1 = true;
        return c.n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static partial void bumpAsync(this ж<counter> Ꮡc, nint times, ж<Δsync.WaitGroup> wg) {
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(wg.Done, ref ᒐ);
            for (nint i = 0; i < times; i++) {
                Ꮡc.Value.mu.Lock();
                Ꮡc.Value.n++;
                Ꮡc.Value.mu.Unlock();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }, GoStack.Bounded);
}

internal static bool tryWhileHeld(this ж<counter> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.mu.Lock();
    var got = c.mu.TryLock();
    if (got) {
        c.mu.Unlock();
    }
    c.mu.Unlock();
    return got;
}

internal static partial void Main() {
    UntypedInt workers = 8;
    UntypedInt each = 2000;

    var c = Ꮡ(new counter());
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < workers; i++) {
        Ꮡwg.Add(1);
        var cʗ1 = c;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                cʗ1.bump(each);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, GoStack.Bounded);
    }
    for (nint i = 0; i < workers; i++) {
        Ꮡwg.Add(1);
        c.bumpAsync(each, Ꮡwg);
    }
    Ꮡwg.Wait();
    fmt.Println("total:", c.get(), "want:", (nint)(2 * workers * each));

    fmt.Println("trylock while held:", c.tryWhileHeld());

    c.of(counter.Ꮡmu).Lock();
    var released = new channel<bool>(0);
    var cʗ2 = c;
    var releasedʗ1 = released;
    goǃ(() => {
        cʗ2.of(counter.Ꮡmu).Unlock();
        releasedʗ1.ᐸꟷ(true);
    }, GoStack.Bounded);
    ᐸꟷ(released);
    c.of(counter.Ꮡmu).Lock();
    c.of(counter.Ꮡmu).Unlock();
    fmt.Println("cross-goroutine unlock: ok");

    var p = Ꮡ(new counter());
    var q = p;
    q.of(counter.Ꮡmu).Lock();
    q.n = 42;
    q.of(counter.Ꮡmu).Unlock();
    fmt.Println("via local pointer:", p.n);

    ж<counter> nilc = default;
    fmt.Println("nil receiver, early return:", nilc.nBeforeTouch());
}

} // end main_package
