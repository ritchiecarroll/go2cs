namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

internal static void pinger(channel<nint> a, channel<nint> b, ж<atomic.Bool> stop, channel/*<-*/<int64> done) {
    int64 trips = default;
    while (!stop.Load()) {
        a.ᐸꟷ(1);
        ᐸꟷ(b);
        trips++;
    }
    close(a);
    done.ᐸꟷ(trips);
}

internal static void ponger(channel<nint> a, channel<nint> b) {
    for (;;) {
        var (v, ok) = ᐸꟷ(a, ꟷ);
        if (!ok) {
            return;
        }
        b.ᐸꟷ(v);
    }
}

internal static void yielder(/*<-*/channel<EmptyStruct> start, nint n, ж<atomic.Bool> stop, channel/*<-*/<int64> done) {
    ᐸꟷ(start);
    for (nint i = 0; i < n; i++) {
        runtime.Gosched();
    }
    stop.Store(true);
    done.ᐸꟷ((int64)n);
}

internal static partial void main() {
    nint n = 1000;
    @string s = os.Getenv("MN_FAIR_YIELDS"); if (s != ""u8) {
        var (v, err) = strconv.Atoi(s); if (err == nil && v > 0) {
            n = v;
        }
    }

    var (a, b) = (new channel<nint>(0), new channel<nint>(0));
    var start = new channel<EmptyStruct>(0);
    ref var stop = ref heap(new atomic.Bool(), out var Ꮡstop);
    var (pings, yields) = (new channel<int64>(1), new channel<int64>(1));

    goǃ(ponger, a, b);
    goǃ(pinger, a, b, Ꮡstop, pings.AsSendOnly());
    goǃ(yielder, start.AsRecvOnly(), n, Ꮡstop, yields.AsSendOnly());
    close(start);

    var y = ᐸꟷ(yields);
    var p = ᐸꟷ(pings);

    fmt.Println("the yielder finished its yields:", y == (int64)n);
    fmt.Println("the ping-pong made progress:", p > 0);
}

} // end main_package
