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
}

internal static void add(this ж<counter> Ꮡc, nint v) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.mu.Lock();
    c.sum += v;
    c.n++;
    time.Sleep(time.Microsecond);
    c.mu.Unlock();
}

internal static void worker(ж<counter> c, nint id, ж<Δsync.WaitGroup> wg) {
    GoFrame ᒐ = default;
    try {
        defer(wg.Done, ref ᒐ);
        time.Sleep(time.Millisecond);
        c.add(id);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    nint n = 2000;
    var (v, err) = strconv.Atoi(os.Getenv("MN_LS_N")); if (err == nil && v > 0) {
        n = v;
    }

    var c = Ꮡ(new counter());
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(n);

    for (nint i = 0; i < n; i++) {
        goǃ(worker, c, i, Ꮡwg);
    }

    Ꮡwg.Wait();
    fmt.Println("n:", c.n, "sum:", c.sum, "match:", c.sum == n * (n - 1) / 2);
}

} // end main_package
