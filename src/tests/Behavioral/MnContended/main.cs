namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using Δsync = sync_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

internal static UntypedInt lockIters => 2000;
internal static UntypedInt chanItems => 2000;
internal static UntypedInt picks => 10000;

internal static bool reading = os.Getenv("MN_CONT_READING") == "1"u8;

internal static nint spin(nint n) {
    nint x = 0;
    for (nint i = 0; i < n; i++) {
        x += (i ^ (x >> 3));
    }
    return x;
}

internal static void report(@string phase, time.Time began) {
    if (reading) {
        var evalᴛ1 = time.Since(began);
        var evalᴛ2 = evalᴛ1.Microseconds();
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: %s elapsed_ms=%.1f\n"u8, phase, (float64)evalᴛ2 / 1000D);
    }
}

internal static partial void contendedMutex(nint workers) {
    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var start = new channel<EmptyStruct>(0);
    nint counter = 0;
    var sink = new slice<nint>(workers);

    for (nint w = 0; w < workers; w++) {
        Ꮡwg.Add(1);
        var sinkʗ1 = sink;
        var startʗ1 = start;
        goǃ([GoFunc("contendedMutex.func1")] (nint wΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ᐸꟷ(startʗ1);
                for (nint i = 0; i < lockIters; i++) {
                    Ꮡmu.Lock();
                    counter++;
                    var evalᴛ1 = spin(20);
                    sinkʗ1[wΔ1] += evalᴛ1;
                    Ꮡmu.Unlock();
                    var evalᴛ2 = spin(50);
                    sinkʗ1[wΔ1] += evalᴛ2;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, w);
    }

    var began = time.Now();
    close(start);
    Ꮡwg.Wait();
    report("mutex", began);

    fmt.Println("mutex: every increment counted:", counter == workers * (nint)lockIters);
}

internal static partial void contendedChannel(nint pairs) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var start = new channel<EmptyStruct>(0);
    var ch = new channel<nint>(0);
    var sums = new slice<nint>(pairs);

    for (nint s = 0; s < pairs; s++) {
        Ꮡwg.Add(1);
        var chʗ1 = ch;
        var startʗ1 = start;
        goǃ([GoFunc("contendedChannel.func1")] (nint sΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ᐸꟷ(startʗ1);
                for (nint i = 1; i <= chanItems; i++) {
                    chʗ1.ᐸꟷ(i);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, s);
    }

    ref var rg = ref heap(new Δsync.WaitGroup(), out var Ꮡrg);
    for (nint r = 0; r < pairs; r++) {
        Ꮡrg.Add(1);
        var chʗ2 = ch;
        var startʗ2 = start;
        var sumsʗ1 = sums;
        goǃ([GoFunc("contendedChannel.func2")] (nint rΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡrg.Done, ref ᒐ);
                ᐸꟷ(startʗ2);
                for (nint i = 0; i < chanItems; i++) {
                    var evalᴛ1 = ᐸꟷ(chʗ2);
                    sumsʗ1[rΔ1] += evalᴛ1;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, r);
    }

    var began = time.Now();
    close(start);
    Ꮡwg.Wait();
    Ꮡrg.Wait();
    report("channel", began);

    nint total = 0;
    foreach (var (_, v) in range(sums)) {
        total += v;
    }
    fmt.Println("channel: every item received once:", total == pairs * (nint)chanItems * (nint)(chanItems + 1) / 2);
}

internal static partial void readySelect(nint selectors) {
    var start = new channel<EmptyStruct>(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var got = GoReflect.WithElemDims(new slice<array<nint>>(selectors, () => new(4)), 4);

    for (nint g = 0; g < selectors; g++) {
        Ꮡwg.Add(1);
        var gotʗ1 = got;
        var startʗ1 = start;
        goǃ([GoFunc("readySelect.func1")] (nint gΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var chs = ref heap(new array<channel<nint>>(4), out var Ꮡchs);
                foreach (var c in range(len(chs))) {
                    chs[c] = new channel<nint>(1);
                    chs[c].ᐸꟷ(c);
                }
                ᐸꟷ(startʗ1);
                for (nint i = 0; i < picks; i++) {
                    nint v = default;
                    var selᴛ1 = chs[0];
                    var selᴛ2 = chs[1];
                    var selᴛ3 = chs[2];
                    var selᴛ4 = chs[3];
                    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
                    case 0 when selᴛ1.ꟷᐳ(out v): {
                        break;
                    }
                    case 1 when selᴛ2.ꟷᐳ(out v): {
                        break;
                    }
                    case 2 when selᴛ3.ꟷᐳ(out v): {
                        break;
                    }
                    case 3 when selᴛ4.ꟷᐳ(out v): {
                        break;
                    }}
                    gotʗ1[gΔ1][v]++;
                    chs[v].ᐸꟷ(v);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, g);
    }

    var began = time.Now();
    close(start);
    Ꮡwg.Wait();
    report("select", began);

    var (every, fair) = (true, true);
    foreach (var g in range(len(got))) {
        foreach (var (_, n) in range(got[g].ΔRangeSnapshot())) {
            every = every && n > 0;
            fair = fair && n >= (nint)(picks / 10);
        }
    }
    if (reading) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: select picks[0]=%v\n"u8, got[0]);
    }
    fmt.Println("select: every case picked:", every);
    fmt.Println("select: no case starved:", fair);
}

internal static void main() {
    nint procs = runtime.GOMAXPROCS(0);
    if (reading) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: procs=%d\n"u8, procs);
    }

    contendedMutex(2 * procs);
    contendedChannel(2 * procs);
    readySelect(2 * procs);
}

} // end main_package
