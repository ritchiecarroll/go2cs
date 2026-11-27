namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using Δsync = sync_package;
using time = time_package;
using System.Runtime.CompilerServices;
using Δio = io_package;

partial class main_package {

internal static Action<Action> foreign;

internal static bool reading = os.Getenv("MN_FOREIGN_READING") == "1"u8;

internal static partial void runForeign(Action f) {
    if (foreign != nil) {
        foreign(f);
        return;
    }
    goǃ([GoFunc("runForeign.func1")] () => {
        GoFrame ᒐ = default;
        try {
            runtime.LockOSThread();
            defer(runtime.UnlockOSThread, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

internal static void report(@string row, time.Time began) {
    if (reading) {
        var evalᴛ1 = time.Since(began);
        var evalᴛ2 = evalᴛ1.Microseconds();
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: %s elapsed_ms=%.1f\n"u8, row, (float64)evalᴛ2 / 1000D);
    }
}

internal static partial void f1() {
    var ch = new channel<nint>(0);
    var done = new channel<nint>(0);
    var began = time.Now();
    var chʗ1 = ch;
    var doneʗ1 = done;
    runForeign([GoFunc("f1.func1")] () => {
        var evalᴛ1 = ᐸꟷ(chʗ1);
        doneʗ1.ᐸꟷ(evalᴛ1);
    });
    var chʗ2 = ch;
    goǃ([GoFunc("f1.func2")] () => {
        time.Sleep(5 * time.Millisecond);
        chʗ2.ᐸꟷ(7);
    });
    nint v = ᐸꟷ(done);
    report("F1", began);
    fmt.Println("F1 the foreign receiver got:", v);
}

internal static partial void f2() {
    var ch = new channel<nint>(0);
    var got = new channel<nint>(0);
    var began = time.Now();
    var chʗ1 = ch;
    var gotʗ1 = got;
    goǃ([GoFunc("f2.func1")] () => {
        var evalᴛ1 = ᐸꟷ(chʗ1);
        gotʗ1.ᐸꟷ(evalᴛ1);
    });
    var chʗ2 = ch;
    runForeign([GoFunc("f2.func2")] () => {
        time.Sleep(5 * time.Millisecond);
        chʗ2.ᐸꟷ(9);
    });
    nint v = ᐸꟷ(got);
    report("F2", began);
    fmt.Println("F2 the parked goroutine got:", v);
}

internal static void worker(channel<EmptyStruct> gate, ж<Δsync.WaitGroup> wg, ref nint n, ж<Δsync.Mutex> mu) {
    GoFrame ᒐ = default;
    try {
        defer(wg.Done, ref ᒐ);
        ᐸꟷ(gate);
        mu.Lock();
        n++;
        mu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void f3() {
    const nint workers = 100;
    var done = new channel<nint>(0);
    var began = time.Now();
    var doneʗ1 = done;
    runForeign([MethodImpl(MethodImplOptions.NoInlining), GoFunc("f3.func1")] () => {
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
        ref var nΔ1 = ref heap<nint>(0, out var ᏑnΔ1);
        var gate = new channel<EmptyStruct>(0);
        for (nint i = 0; i < workers; i++) {
            Ꮡwg.Add(1);
            goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => worker(ᴛ1, ᴛ2, ref ᴛ3.DerefOrNull(), ᴛ4), gate, Ꮡwg, ᏑnΔ1, Ꮡmu);
        }
        close(gate);
        Ꮡwg.Wait();
        doneʗ1.ᐸꟷ(nΔ1);
    });
    nint n = ᐸꟷ(done);
    report("F3", began);
    fmt.Println("F3 goroutines started on a foreign thread finished:", n);
}

internal static partial void f4() {
    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
    var held = new channel<EmptyStruct>(0);
    var release = new channel<EmptyStruct>(0);
    var done = new channel<bool>(0);
    var began = time.Now();
    var heldʗ1 = held;
    var releaseʗ1 = release;
    goǃ([GoFunc("f4.func1")] () => {
        Ꮡmu.Lock();
        close(heldʗ1);
        ᐸꟷ(releaseʗ1);
        Ꮡmu.Unlock();
    });
    var doneʗ1 = done;
    var heldʗ2 = held;
    runForeign([GoFunc("f4.func2")] () => {
        ᐸꟷ(heldʗ2);
        Ꮡmu.Lock();
        doneʗ1.ᐸꟷ(true);
        Ꮡmu.Unlock();
    });
    var heldʗ3 = held;
    var releaseʗ2 = release;
    goǃ([GoFunc("f4.func3")] () => {
        ᐸꟷ(heldʗ3);
        time.Sleep(5 * time.Millisecond);
        close(releaseʗ2);
    });
    var ok = ᐸꟷ(done);
    report("F4", began);
    fmt.Println("F4 the foreign locker got the mutex:", ok);
}

internal static void echo(channel<nint> req, channel<nint> resp) {
    foreach (var v in req) {
        resp.ᐸꟷ(v + 1);
    }
    close(resp);
}

internal static nint spin(nint n) {
    nint x = 0;
    for (nint i = 0; i < n; i++) {
        x += (i ^ (x >> 3));
    }
    return x;
}

internal static partial void f5() {
    const nint trips = 1000;
    nint procs = runtime.GOMAXPROCS(0);
    nint threads = 4 * procs;
    var results = new channel<bool>(threads);
    var startBusy = new channel<EmptyStruct>(0);
    var sinks = new channel<nint>(procs);
    ref var busy = ref heap(new Δsync.WaitGroup(), out var Ꮡbusy);
    var began = time.Now();

    for (nint p = 0; p < procs; p++) {
        Ꮡbusy.Add(1);
        var sinksʗ1 = sinks;
        var startBusyʗ1 = startBusy;
        goǃ([GoFunc("f5.func1")] () => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡbusy.Done, ref ᒐ);
                ᐸꟷ(startBusyʗ1);
                nint sink = 0;
                for (nint i = 0; i < 2000; i++) {
                    sink += spin(20000);
                }
                sinksʗ1.ᐸꟷ(sink);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    close(startBusy);

    for (nint t = 0; t < threads; t++) {
        var (req, resp) = (new channel<nint>(0), new channel<nint>(0));
        goǃ(echo, req, resp);
        var reqʗ1 = req;
        var respʗ1 = resp;
        var resultsʗ1 = results;
        runForeign([GoFunc("f5.func2")] () => {
            var ok = true;
            for (nint i = 0; i < trips; i++) {
                reqʗ1.ᐸꟷ(i);
                ok = ok && ᐸꟷ(respʗ1) == i + 1;
            }
            close(reqʗ1);
            var (_, open) = ᐸꟷ(respʗ1, ꟷ);
            resultsʗ1.ᐸꟷ(ok && !open);
        });
    }

    var all = true;
    for (nint t = 0; t < threads; t++) {
        all = all && ᐸꟷ(results);
    }
    Ꮡbusy.Wait();
    report("F5", began);
    fmt.Println("F5 every round trip answered:", all);
}

internal static void f6() {
    var res = new channel<@string>(0);
    var began = time.Now();
    var resʗ1 = res;
    runForeign([GoFunc("f6.func1")] () => {
        GoFrame ᒐ = default;
        try {
            var resʗ2 = resʗ1;
            defer([GoFunc("f6.func1.1")] () => {
                var evalᴛ1 = fmt.Sprint(recover());
                resʗ2.ᐸꟷ(evalᴛ1);
            }, ref ᒐ);
            throw panic("foreign boom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    @string r = ᐸꟷ(res);
    report("F6", began);
    fmt.Println("F6 recovered on the foreign thread:", r);
}

internal static bool runs(@string row) {
    @string only = os.Getenv("MN_FOREIGN_ONLY");
    return only == ""u8 || only == row;
}

internal static void main() {
    if (runs("F1")) {
        f1();
    }
    if (runs("F2")) {
        f2();
    }
    if (runs("F3")) {
        f3();
    }
    if (runs("F4")) {
        f4();
    }
    if (runs("F5")) {
        f5();
    }
    if (runs("F6")) {
        f6();
    }
}

} // end main_package
