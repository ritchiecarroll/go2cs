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

internal static void Main() {
    var a = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    var aPending = a.Stop();
    fmt.Println("A stop-after-fire pending:", aPending, "stale:", tryRecv((~a).C));

    var b = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    var bPending = b.Reset(time.ΔHour);
    fmt.Println("B reset-after-fire pending:", bPending, "stale:", tryRecv((~b).C));

    var c = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    fmt.Println("C len:", len((~c).C), "cap:", cap((~c).C));
    fmt.Println("C receive:", !(ᐸꟷ((~c).C)).IsZero());

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
    fmt.Println("D ticker stop stale:", dStale);

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
    fmt.Println("E ticker reset stale:", eStale);

    nint fDelivered = 0;
    for (nint iΔ3 = 0; iΔ3 < 200; iΔ3++) {
        var t = time.NewTimer(time.ΔHour);
        t.Reset(time.Millisecond);
        ᐸꟷ((~t).C);
        fDelivered++;
    }
    fmt.Println("F reset-to-imminent delivered:", fDelivered);

    var done = new channel<bool>(1);
    var doneʗ1 = done;
    var g = time.AfterFunc(1 * time.Millisecond, () => {
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    fmt.Println("G afterfunc stop-after-fire pending:", g.Stop());

    var h = time.After(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    fmt.Println("H after len:", len(h), "cap:", cap(h));
    fmt.Println("H after receive:", !(ᐸꟷ(h)).IsZero());

    var i = time.NewTimer(1 * time.Millisecond);
    time.Sleep(25 * time.Millisecond);
    i.Stop();
    fmt.Println("I second stop pending:", i.Stop(), "stale:", tryRecv((~i).C));

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
    fmt.Println("J batch stop pending:", jPending, "of", (nint)(batch), "stale:", jStale);

    var (kPending, kStale) = stopRound(true);
    fmt.Println("K batch reset pending:", kPending, "of", (nint)(batch), "stale:", kStale);
}

} // end main_package
