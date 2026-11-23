namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

partial struct counter {
    internal Δsync.Mutex mu;
    internal nint sum;
    internal nint n;
}

internal static void worker(ж<counter> Ꮡc, nint id, ж<Δsync.WaitGroup> arrived, ж<Δsync.WaitGroup> release, ж<Δsync.WaitGroup> done) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        defer(done.Done, ref ᒐ);
        arrived.Done();
        release.Wait();
        c.mu.Lock();
        c.sum += id;
        c.n++;
        c.mu.Unlock();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    nint n = 20000;
    var (v, err) = strconv.Atoi(os.Getenv("MN_BR_N")); if (err == nil && v > 0) {
        n = v;
    }

    var c = Ꮡ(new counter());
    ref var arrived = ref heap(new Δsync.WaitGroup(), out var Ꮡarrived);
    ref var release = ref heap(new Δsync.WaitGroup(), out var Ꮡrelease);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    Ꮡarrived.Add(n);
    Ꮡrelease.Add(1);
    Ꮡdone.Add(n);

    var t0 = time.Now();

    for (nint i = 0; i < n; i++) {
        goǃ(worker, c, i, Ꮡarrived, Ꮡrelease, Ꮡdone);
    }

    Ꮡarrived.Wait();
    var t1 = time.Now();
    Ꮡrelease.Done();
    Ꮡdone.Wait();
    var t2 = time.Now();
    fmt.Fprintln(new os.FileжWriter(os.Stderr), "PHASES spawn-ms=" + strconv.FormatInt(t1.Sub(t0).Milliseconds(), 10) + " return-ms=" + strconv.FormatInt(t2.Sub(t1).Milliseconds(), 10));
    fmt.Println("n:", c.n, "sum:", c.sum, "match:", c.sum == n * (n - 1) / 2);
}

} // end main_package
