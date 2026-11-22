namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

partial struct storm {
    internal Δsync.Mutex mu;
    internal nint counter;
    internal channel<EmptyStruct> start;
    internal channel<nint> tokens;
    internal channel<nint> results;
    internal Δsync.WaitGroup arrived;
    internal Δsync.WaitGroup done;
}

internal static void worker(this ж<storm> Ꮡs, nint id) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(Ꮡs.Ꮡdone.Done, ref ᒐ);
        Ꮡs.Ꮡarrived.Done();

        nint got = 0;

        var selᴛ1 = s.start;
        var selᴛ2 = s.tokens;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out var t): {
            got = t;
            ᐸꟷ(s.start);
            break;
        }}

        time.Sleep(time.Millisecond);

        s.mu.Lock();
        s.counter += id;
        s.mu.Unlock();

        s.results.ᐸꟷ(id + got);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint envInt(@string name, nint fallback) {
    var (v, err) = strconv.Atoi(os.Getenv(name)); if (err == nil && v > 0) {
        return v;
    }

    return fallback;
}

internal static partial void main() {
    nint n = envInt("MN_STORM_N", 100000);
    nint hold = envInt("MN_STORM_HOLD_MS", 0);

    var s = Ꮡ(new storm(
        start: new channel<EmptyStruct>(0),
        tokens: new channel<nint>(0),
        results: new channel<nint>(0)
    ));

    s.Ꮡarrived.Add(n);
    s.Ꮡdone.Add(n);

    for (nint iᴛ1 = 0; iᴛ1 < n; iᴛ1++) {
        var i = iᴛ1;
        if (i % 2 == 0) {
            var sʗ1 = s;
            goǃ(sʗ1.worker, i);
        } else {
            var sʗ2 = s;
            goǃ(() => {
                sʗ2.worker(i);
            });
        }
    }

    s.Ꮡarrived.Wait();
    fmt.Println("parked:", n);

    if (hold > 0) {
        time.Sleep(((time.Duration)(int64)hold) * time.Millisecond);
    }

    nint tokens = n / 10;

    for (nint i = 0; i < tokens; i++) {
        s.tokens.ᐸꟷ(1);
    }

    close(s.start);

    nint total = 0;

    for (nint i = 0; i < n; i++) {
        total += ᐸꟷ(s.results);
    }

    s.Ꮡdone.Wait();

    nint want = n * (n - 1) / 2;

    fmt.Println("tokens:", tokens);
    fmt.Println("counter:", s.counter, "match:", s.counter == want);
    fmt.Println("total:", total, "match:", total == want + tokens);
}

} // end main_package
