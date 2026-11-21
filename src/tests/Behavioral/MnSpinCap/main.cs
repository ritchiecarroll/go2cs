namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using go.sync;
using Δio = io_package;

partial class main_package {

internal static bool pure = os.Getenv("MN_SPIN_PURE") == "1"u8;

partial struct arm {
    internal channel<EmptyStruct> start;
    internal channel<nint> wake;
    internal atomic.Bool stop;
    internal atomic.Int64 spins;
    internal atomic.Int64 got;
    internal Δsync.WaitGroup done;
}

internal static void spinner(this ж<arm> Ꮡa) {
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        defer(Ꮡa.Ꮡdone.Done, ref ᒐ);
        ᐸꟷ(a.start);

        int64 n = default;
        while (!Ꮡa.Ꮡstop.Load()) {
            n++;
            if (!pure) {
                runtime.Gosched();
            }
        }
        Ꮡa.Ꮡspins.Add(n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void waker(this ж<arm> Ꮡa) {
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        defer(Ꮡa.Ꮡdone.Done, ref ᒐ);
        ᐸꟷ(a.start);
        time.Sleep(20 * time.Millisecond);
        a.wake.ᐸꟷ(42);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void sleeper(this ж<arm> Ꮡa) {
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.DerefOrNull();

        defer(Ꮡa.Ꮡdone.Done, ref ᒐ);
        nint v = ᐸꟷ(a.wake);
        Ꮡa.Ꮡgot.Store((int64)v);
        Ꮡa.Ꮡstop.Store(true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    nint extra = 4;
    @string s = os.Getenv("MN_SPIN_EXTRA"); if (s != ""u8) {
        var (v, err) = strconv.Atoi(s); if (err == nil && v > 0) {
            extra = v;
        }
    }

    nint procs = runtime.GOMAXPROCS(0);
    nint spinners = procs + extra;

    var a = Ꮡ(new arm(start: new channel<EmptyStruct>(0), wake: new channel<nint>(0)));
    a.Ꮡdone.Add(spinners + 2);

    for (nint i = 0; i < spinners; i++) {
        var aʗ1 = a;
        goǃ(aʗ1.spinner);
    }
    var aʗ2 = a;
    goǃ(aʗ2.sleeper);
    var aʗ3 = a;
    goǃ(aʗ3.waker);

    var began = time.Now();
    close(a.start);
    a.Ꮡdone.Wait();
    var elapsed = time.Since(began);

    fmt.Println("spinners beyond the P count:", spinners > procs);
    fmt.Println("the sleeper received:", a.Ꮡgot.Load());
    fmt.Println("every spinner stopped:", a.Ꮡstop.Load());
    fmt.Println("the spinners ran:", a.Ꮡspins.Load() > 0);

    if (os.Getenv("MN_SPIN_READING") == "1"u8) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "reading: procs=%d spinners=%d elapsed_ms=%.1f spins=%d\n"u8,
            procs, spinners, (float64)elapsed.Microseconds() / 1000D, a.Ꮡspins.Load());
    }
}

} // end main_package
