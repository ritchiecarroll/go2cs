namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static bool tryRecv(/*<-*/channel<time.Time> c) {
    var selᴛ1 = c;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        return true;
    }
    default: {
        return false;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aStopAfterFirePendingˢ = (@string)"A stop-after-fire pending:"u8;
private static readonly object staleˢ = (@string)"stale:"u8;
private static readonly object bResetAfterFirePendingˢ = (@string)"B reset-after-fire pending:"u8;
private static readonly object cLenˢ = (@string)"C len:"u8;
private static readonly object capˢ = (@string)"cap:"u8;
private static readonly object cReceiveˢ = (@string)"C receive:"u8;
private static readonly object dTickerStopStaleˢ = (@string)"D ticker stop stale:"u8;
private static readonly object eTickerResetStaleˢ = (@string)"E ticker reset stale:"u8;
private static readonly object fResetToImminentˢ = (@string)"F reset-to-imminent delivered:"u8;
private static readonly object gAfterfuncStopAfterFireˢ = (@string)"G afterfunc stop-after-fire pending:"u8;
private static readonly object hAfterLenˢ = (@string)"H after len:"u8;
private static readonly object hAfterReceiveˢ = (@string)"H after receive:"u8;
private static readonly object iSecondStopPendingˢ = (@string)"I second stop pending:"u8;
private static readonly object jBatchStopPendingˢ = (@string)"J batch stop pending:"u8;
private static readonly object kBatchResetPendingˢ = (@string)"K batch reset pending:"u8;

internal static void Main() {
    var a = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    var aPending = a.Stop();
    fmt.Println(aStopAfterFirePendingˢ, aPending, staleˢ, tryRecv((~a).C));

    var b = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    var bPending = b.Reset(time.ΔHour);
    fmt.Println(bResetAfterFirePendingˢ, bPending, staleˢ, tryRecv((~b).C));

    var c = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    fmt.Println(cLenˢ, len((~c).C), capˢ, cap((~c).C));
    fmt.Println(cReceiveˢ, !(ᐸꟷ((~c).C)).IsZero());

    time.Duration period = /* 200 * time.Microsecond */ 200000;
    nint dStale = 0;
    for (nint iΔ1 = 0; iΔ1 < 300; iΔ1++) {
        var tkΔ1 = time.NewTicker(period);
        time.Sleep(((time.Duration)(int64)(iΔ1 % 9)) * period / 4);
        tkΔ1.Stop();
        if (tryRecv((~tkΔ1).C)) {
            dStale++;
        }
    }
    fmt.Println(dTickerStopStaleˢ, dStale);

    nint eStale = 0;
    var tk = time.NewTicker(period);
    for (nint iΔ2 = 0; iΔ2 < 300; iΔ2++) {
        time.Sleep(((time.Duration)(int64)(iΔ2 % 9)) * period / 4);
        tk.Reset(time.ΔHour);
        if (tryRecv((~tk).C)) {
            eStale++;
        }
        tk.Reset(period);
    }
    tk.Stop();
    fmt.Println(eTickerResetStaleˢ, eStale);

    nint fDelivered = 0;
    for (nint iΔ3 = 0; iΔ3 < 200; iΔ3++) {
        var t = time.NewTimer(time.ΔHour);
        t.Reset(time.Millisecond);
        ᐸꟷ((~t).C);
        fDelivered++;
    }
    fmt.Println(fResetToImminentˢ, fDelivered);

    var done = new channel<bool>(1);
    var doneʗ1 = done;
    var g = time.AfterFunc(1 * time.Millisecond, () => {
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    fmt.Println(gAfterfuncStopAfterFireˢ, g.Stop());

    var h = time.After(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    fmt.Println(hAfterLenˢ, len(h), capˢ, cap(h));
    fmt.Println(hAfterReceiveˢ, !(ᐸꟷ(h)).IsZero());

    var i = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    i.Stop();
    fmt.Println(iSecondStopPendingˢ, i.Stop(), staleˢ, tryRecv((~i).C));

    const nint batch = 600;
    time.Duration armAt = /* 40 * time.Millisecond */ 40000000;

    (nint, nint) stopRound(bool reset) {
        var start = time.Now();
        var timers = new slice<ж<time.Timer>>(batch);
        foreach (var (n, _) in timers) {
            timers[n] = time.NewTimer(armAt - time.Since(start));
        }
        time.Sleep(armAt - time.Since(start));

        nint pending = 0;
        nint stale = 0;
        foreach (var (_, t) in timers) {
            bool was = default!;
            if (reset){
                was = t.Reset(time.ΔHour);
            } else {
                was = t.Stop();
            }
            if (was) {
                pending++;
            }
        }
        foreach (var (_, t) in timers) {
            if (tryRecv((~t).C)) {
                stale++;
            }
        }
        return (pending, stale);
    }

    var (jPending, jStale) = stopRound(false);
    fmt.Println(jBatchStopPendingˢ, jPending, (@string)"of"u8, (nint)(batch), staleˢ, jStale);

    var (kPending, kStale) = stopRound(true);
    fmt.Println(kBatchResetPendingˢ, kPending, (@string)"of"u8, (nint)(batch), staleˢ, kStale);
}

} // end main_package
