namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using Δsync = sync_package;

partial class main_package {

internal static void main() {
    nint start = runtime.GOMAXPROCS(0);
    fmt.Println("start positive:", start > 0);

    fmt.Println("set 2 returns the start:", runtime.GOMAXPROCS(2) == start);
    fmt.Println("read:", runtime.GOMAXPROCS(0));
    fmt.Println("read with -1:", runtime.GOMAXPROCS(-1));

    fmt.Println("set 1 returns:", runtime.GOMAXPROCS(1));
    fmt.Println("sum under 1:", work(64));

    fmt.Println("set 3 returns:", runtime.GOMAXPROCS(3));
    fmt.Println("sum under 3:", work(64));

    fmt.Println("restore returns:", runtime.GOMAXPROCS(start));
    fmt.Println("restored:", runtime.GOMAXPROCS(0) == start);
}

internal static partial nint work(nint n) {
    var results = new channel<nint>(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);

    for (nint i = 0; i < n; i++) {
        Ꮡwg.Add(1);
        var resultsʗ1 = results;
        goǃ([GoFunc("work.func1")] (nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                resultsʗ1.ᐸꟷ(iΔ1 * iΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }

    var resultsʗ2 = results;
    goǃ([GoFunc("work.func2")] () => {
        Ꮡwg.Wait();
        close(resultsʗ2);
    });

    nint sum = 0;
    foreach (var r in results) {
        sum += r;
    }

    return sum;
}

} // end main_package
