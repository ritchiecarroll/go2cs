namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using sort = sort_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

partial struct pair {
    internal Δsync.Mutex a, b;
    internal int64 stampA;
    internal int64 stampB;
    internal slice<int64> latency;
}

internal static void side(Δsync.Locker me, Δsync.Locker other, ref int64 myStamp, ref int64 otherStamp, slice<int64> lat, nint cycles, bool first, ж<Δsync.WaitGroup> done) {
    GoFrame ᒐ = default;
    try {
        defer(done.Done, ref ᒐ);
        runtime.Gosched();
        for (nint i = 0; i < cycles; i++) {
            me.Lock();
            if (!first || i > 0) {
                lat[i] = time.Now().UnixNano() - myStamp;
            }
            otherStamp = time.Now().UnixNano();
            other.Unlock();
        }
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
    nint pairs = envInt("MN_HB_PAIRS", 100);
    nint cycles = envInt("MN_HB_CYCLES", 1000);

    var ps = new slice<ж<pair>>(pairs);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    Ꮡdone.Add(2 * pairs);
    var start = time.Now();

    foreach (var p in range(len(ps))) {
        var x = Ꮡ(new pair(latency: new slice<int64>(2 * cycles)));
        ps[p] = x;
        x.Ꮡa.Lock();
        x.Ꮡb.Lock();
        Δsync.Locker la = new Δsync.MutexжLocker(x.Ꮡa);
        Δsync.Locker lb = new Δsync.MutexжLocker(x.Ꮡb);
        goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4, ᴛ5, ᴛ6, ᴛ7) => side(ᴛ1, ᴛ2, ref ᴛ3.DerefOrNull(), ref ᴛ4.DerefOrNull(), ᴛ5, ᴛ6, true, ᴛ7), la, lb, x.ᏑstampA, x.ᏑstampB, x.latency.slice(0, cycles), cycles, Ꮡdone);
        goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4, ᴛ5, ᴛ6, ᴛ7) => side(ᴛ1, ᴛ2, ref ᴛ3.DerefOrNull(), ref ᴛ4.DerefOrNull(), ᴛ5, ᴛ6, false, ᴛ7), lb, la, x.ᏑstampB, x.ᏑstampA, x.latency.slice(cycles), cycles, Ꮡdone);
        x.stampA = time.Now().UnixNano();
        x.Ꮡa.Unlock();
    }

    Ꮡdone.Wait();
    var wall = time.Since(start);

    slice<int64> all = default;
    foreach (var (_, x) in range(ps)) {
        foreach (var (i, v) in range(x.latency)) {
            if (i != 0 && v > 0) {
                all = append(all, v);
            }
        }
    }
    var allʗ1 = all;
    sort.Slice(all, (nint i, nint j) => allʗ1[i] < allʗ1[j]);
    @string us(int64 v) => strconv.FormatFloat((float64)v / 1000D, (byte)'f', 1, 64);
    nint n = len(all);
    fmt.Fprintln(new os.FileжWriter(os.Stderr), "HANDBACK samples=" + strconv.Itoa(n) + " p50=" + us(all[n / 2]) + " p90=" + us(all[n * 9 / 10]) + " p99=" + us(all[n * 99 / 100]) + " max=" + us(all[n - 1]) + " us wall-ms=" + strconv.FormatInt(wall.Milliseconds(), 10));
    fmt.Println("pairs:", pairs, "cycles:", cycles);
}

} // end main_package
