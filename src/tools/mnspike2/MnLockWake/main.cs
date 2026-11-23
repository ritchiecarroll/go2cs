namespace go;

using fmt = fmt_package;
using os = os_package;
using sort = sort_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;
using Δio = io_package;

partial class main_package {

partial struct state {
    internal Δsync.Mutex mu;
    internal time.Time t0;
}

internal static void waiter(ж<state> Ꮡs, slice<time.Duration> lat, nint i, channel/*<-*/<bool> done) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.mu.Lock();
    lat[i] = time.Since(s.t0);
    s.mu.Unlock();
    done.ᐸꟷ(true);
}

internal static partial void main() {
    nint n = 2000;
    var (v, err) = strconv.Atoi(os.Getenv("MN_LW_N")); if (err == nil && v > 0) {
        n = v;
    }

    var s = Ꮡ(new state());
    var lat = new slice<time.Duration>(n);
    var done = new channel<bool>(0);

    for (nint i = 0; i < n; i++) {
        s.Ꮡmu.Lock();
        goǃ(waiter, s, lat, i, done.AsSendOnly());
        time.Sleep(200 * time.Microsecond);
        s.t0 = time.Now();
        s.Ꮡmu.Unlock();
        ᐸꟷ(done);
    }

    var latʗ1 = lat;
    sort.Slice(lat, (nint a, nint b) => latʗ1[a] < latʗ1[b]);
    @string us(time.Duration d) => strconv.FormatFloat((float64)d.Nanoseconds() / 1000D, (byte)'f', 1, 64);
    fmt.Fprintln(new os.FileжWriter(os.Stderr), "LATENCY p50=" + us(lat[n / 2]) + " p90=" + us(lat[n * 9 / 10]) + " p99=" + us(lat[n * 99 / 100]) + " max=" + us(lat[n - 1]) + " us");
    fmt.Println("iterations:", n);
}

} // end main_package
