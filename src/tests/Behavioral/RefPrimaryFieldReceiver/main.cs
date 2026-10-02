namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct counter {
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void bumpAsync(this ж<counter> Ꮡc, nint times, ж<Δsync.WaitGroup> Ꮡwg) {
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            for (nint i = 0; i < times; i++) {
                Ꮡc.Value.mu.Lock();
                Ꮡc.Value.n++;
                Ꮡc.Value.mu.Unlock();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object totalˢ = (@string)"total:"u8;
private static readonly object wantˢ = (@string)"want:"u8;
private static readonly object trylockWhileHeldˢ = (@string)"trylock while held:"u8;
private static readonly object crossGoroutineUnlockOkˢ = (@string)"cross-goroutine unlock: ok"u8;
private static readonly object viaLocalPointerˢ = (@string)"via local pointer:"u8;
private static readonly object nilReceiverEarlyReturnˢ = (@string)"nil receiver, early return:"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    UntypedInt workers = 8;
    UntypedInt each = 2000;
    var c = Ꮡ(new counter(nil));
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
        });
    }
    for (nint i = 0; i < workers; i++) {
        Ꮡwg.Add(1);
        c.bumpAsync(each, Ꮡwg);
    }
    Ꮡwg.Wait();
    fmt.Println(totalˢ, c.get(), wantˢ, (nint)(2 * workers * each));
    fmt.Println(trylockWhileHeldˢ, c.tryWhileHeld());
    c.of(counter.Ꮡmu).Lock();
    var released = new channel<bool>(0);
    var cʗ2 = c;
    var releasedʗ1 = released;
    goǃ(() => {
        cʗ2.of(counter.Ꮡmu).Unlock();
        releasedʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(released);
    c.of(counter.Ꮡmu).Lock();
    c.of(counter.Ꮡmu).Unlock();
    fmt.Println(crossGoroutineUnlockOkˢ);
    var p = Ꮡ(new counter(nil));
    var q = p;
    q.of(counter.Ꮡmu).Lock();
    q.Value.n = 42;
    q.of(counter.Ꮡmu).Unlock();
    fmt.Println(viaLocalPointerˢ, (~p).n);
    ж<counter> nilc = default!;
    fmt.Println(nilReceiverEarlyReturnˢ, nilc.nBeforeTouch());
}

} // end main_package
