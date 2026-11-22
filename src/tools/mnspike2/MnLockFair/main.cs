namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

partial struct counter {
    internal Δsync.Mutex mu;
    internal nint sum;
    internal nint n;
    internal nint park;
    internal nint plug;
}

internal static void parked(ж<counter> Ꮡc, nint id, ж<Δsync.WaitGroup> wg) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(wg.Done, ref ᒐ);
        time.Sleep(time.Millisecond);
        c.mu.Lock();
        c.sum += id;
        c.n++;
        c.park++;
        c.mu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void blocked(ref counter c, Δsync.Locker l, nint id, ж<Δsync.WaitGroup> wg) {
    GoFrame ᒐ = default;
    try {
        defer(wg.Done, ref ᒐ);
        time.Sleep(time.Millisecond);
        l.Lock();
        c.sum += id;
        c.n++;
        c.plug++;
        l.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    nint n = 4000;
    var (v, err) = strconv.Atoi(os.Getenv("MN_LF_N")); if (err == nil && v > 0) {
        n = v;
    }

    var c = Ꮡ(new counter());
    Δsync.Locker l = new Δsync.MutexжLocker(c.Ꮡmu);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(n);

    for (nint i = 0; i < n; i++) {
        if (i % 2 == 0) {
            goǃ(parked, c, i, Ꮡwg);
        } else {
            goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => blocked(ref ᴛ1.DerefOrNull(), ᴛ2, ᴛ3, ᴛ4), c, l, i, Ꮡwg);
        }
    }

    Ꮡwg.Wait();
    fmt.Println("n:", c.n, "sum:", c.sum, "match:", c.sum == n * (n - 1) / 2);
    fmt.Println("through the field:", c.park, "through a Locker:", c.plug);
}

} // end main_package
