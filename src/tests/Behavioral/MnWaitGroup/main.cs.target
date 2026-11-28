namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using Δsync = sync_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

internal static bool reading = os.Getenv("MN_WG_READING") == "1"u8;

internal static void waitAll(ж<Δsync.WaitGroup> wg) {
    wg.Wait();
}

internal static partial bool released(nint k) {
    ref var work = ref heap(new Δsync.WaitGroup(), out var Ꮡwork);
    ref var ready = ref heap(new Δsync.WaitGroup(), out var Ꮡready);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    var results = new slice<nint>(k);

    Ꮡwork.Add(1);
    Ꮡready.Add(k);
    Ꮡdone.Add(k);

    for (nint i = 0; i < k; i++) {
        var resultsʗ1 = results;
        goǃ([GoFunc("released.func1")] (nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡdone.Done, ref ᒐ);
                Ꮡready.Done();
                waitAll(Ꮡwork);
                resultsʗ1[iΔ1] = iΔ1 * iΔ1;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }

    Ꮡready.Wait();
    time.Sleep(20 * time.Millisecond);
    Ꮡwork.Done();
    Ꮡdone.Wait();

    foreach (var (i, r) in range(results)) {
        if (r != i * i) {
            return false;
        }
    }
    return true;
}

internal static partial bool generations(nint k, nint n) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    nint total = 0;
    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);

    for (nint gen = 0; gen < n; gen++) {
        Ꮡwg.Add(k);
        for (nint i = 0; i < k; i++) {
            goǃ([GoFunc("generations.func1")] () => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    Ꮡmu.Lock();
                    total++;
                    Ꮡmu.Unlock();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    return total == k * n;
}

internal static void main() {
    nint procs = runtime.GOMAXPROCS(0);
    nint k = 4 * procs;
    if (k < 8) {
        k = 8;
    }
    if (reading) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: procs=%d waiters=%d\n"u8, procs, k);
    }

    fmt.Println("every waiter released:", released(k));
    fmt.Println("three generations counted:", generations(k, 3));

    ref var zero = ref heap(new Δsync.WaitGroup(), out var Ꮡzero);
    Ꮡzero.Wait();
    fmt.Println("zero-count wait returned");
}

} // end main_package
