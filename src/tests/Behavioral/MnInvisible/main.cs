namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using Δos = os_package;
using exec = go.os.exec_package;
using Δruntime = runtime_package;
using debug = go.runtime.debug_package;
using sort = sort_package;
using strconv = strconv_package;
using time = time_package;
using go.os;
using go.runtime;
using Δio = io_package;

partial class main_package {

internal static Func<(Action, Action)> newManagedEvent;

internal static Func<(Action, Action)> newNativeFlag;

internal static Func<bool, (Action, Action)> newBracketedFlag;

internal static Func<(int64, int64, int64, int64, bool)> counters;

internal static bool reading = Δos.Getenv("MN_INV_READING") == "1"u8;

internal static (Action wait, Action set) channelEvent() {
    var ch = new channel<EmptyStruct>(0);
    var chʗ1 = ch;

    var chʗ2 = ch;
    return ([GoFunc("channelEvent.func1")] () => {
        ᐸꟷ(chʗ1);
    }, [GoFunc("channelEvent.func2")] () => {
        close(chʗ2);
    });
}

internal static void blocker(channel<EmptyStruct> gate, Action wait, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    wait();
    done.ᐸꟷ(1);
}

internal static partial nint blockAll(nint k, Action wait, Action set) {
    var gate = new channel<EmptyStruct>(0);
    var done = new channel<nint>(k);
    for (nint i = 0; i < k; i++) {
        goǃ(blocker, gate, wait, done.AsSendOnly());
    }
    close(gate);
    time.Sleep(50 * time.Millisecond);
    set();
    nint n = 0;
    for (nint i = 0; i < k; i++) {
        n += ᐸꟷ(done);
    }
    return n;
}

internal static void v1(nint k) {
    var mk = newManagedEvent;
    if (mk == nil) {
        mk = channelEvent;
    }
    var (ww, ws) = channelEvent();
    blockAll(k, ww, ws);
    int64 i0 = default;
    int64 s0 = default;
    if (counters != nil) {
        (_, _, i0, s0, _) = counters();
    }
    var (wait, set) = mk();
    var ok = blockAll(k, wait, set) == k;
    if (counters != nil) {
        var (_, _, i1, s1, on) = counters(); if (on) {
            ok = ok && s1 > s0 && i1 == i0;
        }
    }
    fmt.Println("V1 goroutines released from a managed wait, which the scheduler sensed:", ok);
}

internal static void v2(nint k) {
    var mk = newNativeFlag;
    if (mk == nil) {
        mk = channelEvent;
    }
    var (wait, set) = mk();
    fmt.Println("V2 goroutines released from a native wait:", blockAll(k, wait, set) == k);
}

internal static bool bracketed(nint k, bool blocks) {
    var mk = newBracketedFlag;
    if (mk == nil) {
        mk = [GoFunc("bracketed.func1")] (bool _Δp0) => channelEvent();
    }
    var (ww, ws) = channelEvent();
    blockAll(k, ww, ws);
    int64 h0 = default;
    int64 r0 = default;
    int64 i0 = default;
    if (counters != nil) {
        (h0, r0, i0, _, _) = counters();
    }
    var (wait, set) = mk(blocks);
    var released = blockAll(k, wait, set) == k;
    if (counters == nil) {
        return released;
    }
    var (h1, r1, i1, _, on) = counters();
    switch (ᐧ) {
    case {} when !on: {
        return released;
    }
    case {} when blocks: {
        return released && h1 - h0 >= (int64)k && r1 == r0;
    }
    default: {
        return released && r1 > r0 && i1 == i0;
    }}

}

internal static nint busy(time.Duration d) {
    nint x = 0;
    for (var began = time.Now(); time.Since(began) < d; ) {
        for (nint i = 0; i < 1000; i++) {
            x += (i ^ (x >> 3));
        }
    }
    return x;
}

internal static void cpu(channel<EmptyStruct> gate, channel<EmptyStruct> stop, time.Duration step, channel/*<-*/<nint> sinks) {
    ᐸꟷ(gate);
    nint sink = 0;
    for (;;) {
        var stopᴛ1 = stop;
        switch (trySelect(ᐸꟷ(stopᴛ1, ꓸꓸꓸ))) {
        case 0 when stopᴛ1.ꟷᐳ(out _): {
            sinks.ᐸꟷ(sink);
            return;
        }
        default: {
            break;
        }}
        sink += busy(step);
        Δruntime.Gosched();
    }
}

internal static partial void v4(nint procs) {
    var step = 200 * time.Millisecond;
    var (s, err) = strconv.Atoi(Δos.Getenv("MN_INV_STEP_MS")); if (err == nil && s > 0) {
        step = ((time.Duration)(int64)s) * time.Millisecond;
    }
    var (gate, stop) = (new channel<EmptyStruct>(0), new channel<EmptyStruct>(0));
    var sinks = new channel<nint>(procs);
    for (nint p = 0; p < procs; p++) {
        goǃ(cpu, gate, stop, step, sinks.AsSendOnly());
    }
    close(gate);

    const nint wakes = 200;
    var lat = new slice<time.Duration>(0, wakes);
    for (nint i = 0; i < wakes; i++) {
        var began = time.Now();
        time.Sleep(25 * time.Millisecond);
        lat = append(lat, time.Since(began) - 25 * time.Millisecond);
    }
    close(stop);
    for (nint p = 0; p < procs; p++) {
        ᐸꟷ(sinks);
    }
    if (reading) {
        var latʗ1 = lat;
        sort.Slice(lat, [GoFunc("v4.func1")] (nint i, nint j) => latʗ1[i] < latʗ1[j]);
        fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "reading: V4 step_ms=%d wakes=%d p50_ms=%.1f p99_ms=%.1f max_ms=%.1f\n"u8, step.Milliseconds(), len(lat),
            ms(lat[len(lat) / 2]), ms(lat[len(lat) * 99 / 100]), ms(lat[len(lat) - 1]));
    }
    fmt.Println("V4 every wake ran:", len(lat) == wakes);
}

internal static float64 ms(time.Duration d) { return (float64)d.Microseconds() / 1000D; }

internal static void pipeReader(channel<EmptyStruct> gate, ж<Δos.File> r, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    var b = new slice<byte>(1);
    var (n, _) = r.Read(b);
    done.ᐸꟷ(n);
}

internal static partial nint censusPipe(nint k) {
    var (r, w, err) = Δos.Pipe();
    if (err != nil) {
        return -1;
    }
    var (gate, done) = (new channel<EmptyStruct>(0), new channel<nint>(k));
    for (nint i = 0; i < k; i++) {
        goǃ(pipeReader, gate, r, done.AsSendOnly());
    }
    close(gate);
    time.Sleep(300 * time.Millisecond);
    w.Write(new slice<byte>(k));
    nint n = 0;
    for (nint i = 0; i < k; i++) {
        n += ᐸꟷ(done);
    }
    return n;
}

internal static void acceptor(channel<EmptyStruct> gate, Δnet.Listener l, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    var (c, err) = l.Accept();
    if (err == nil) {
        c.Close();
        done.ᐸꟷ(1);
        return;
    }
    done.ᐸꟷ(0);
}

internal static partial nint censusAccept(nint k) {
    GoFrame ᒐ = default;
    try {
        var (l, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != nil) {
            return -1;
        }
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        var (gate, done) = (new channel<EmptyStruct>(0), new channel<nint>(k));
        for (nint i = 0; i < k; i++) {
            goǃ(acceptor, gate, l, done.AsSendOnly());
        }
        close(gate);
        time.Sleep(300 * time.Millisecond);
        for (nint i = 0; i < k; i++) {
            {
                var (c, errΔ1) = Δnet.Dial("tcp", l.Addr().String()); if (errΔ1 == nil) {
                    var cʗ1 = c;
                    defer(() => cʗ1.Close(), ref ᒐ);
                }
            }
        }
        nint n = 0;
        for (nint i = 0; i < k; i++) {
            n += ᐸꟷ(done);
        }
        return n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void connReader(channel<EmptyStruct> gate, Δnet.Conn c, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    var b = new slice<byte>(1);
    var (n, _) = c.Read(b);
    done.ᐸꟷ(n);
}

internal static partial nint censusNetRead(nint k) {
    GoFrame ᒐ = default;
    try {
        var writersᴷ = new stackAppend<Δnet.Conn>();

        var (l, err) = Δnet.Listen("tcp", "127.0.0.1:0");
        if (err != nil) {
            return -1;
        }
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        var accepted = new channel<Δnet.Conn>(k);
        var acceptedʗ1 = accepted;
        var lʗ2 = l;
        goǃ([GoFunc("censusNetRead.func1")] () => {
            for (nint i = 0; i < k; i++) {
                var (c, errΔ1) = lʗ2.Accept();
                if (errΔ1 != nil) {
                    close(acceptedʗ1);
                    return;
                }
                acceptedʗ1.ᐸꟷ(c);
            }
        });
        var writers = new slice<Δnet.Conn>(0, k);
        for (nint i = 0; i < k; i++) {
            var (c, errΔ2) = Δnet.Dial("tcp", l.Addr().String());
            if (errΔ2 != nil) {
                return -1;
            }
            writers = append(writersᴷ, writers, c);
        }
        var (gate, done) = (new channel<EmptyStruct>(0), new channel<nint>(k));
        for (nint i = 0; i < k; i++) {
            goǃ(connReader, gate, ᐸꟷ(accepted), done.AsSendOnly());
        }
        close(gate);
        time.Sleep(300 * time.Millisecond);
        foreach (var (_, c) in range(writers)) {
            c.Write([1]);
        }
        nint n = 0;
        for (nint i = 0; i < k; i++) {
            n += ᐸꟷ(done);
        }
        return n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void waiter(channel<EmptyStruct> gate, ж<exec.Cmd> cmd, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    if (cmd.Wait() == nil) {
        done.ᐸꟷ(1);
        return;
    }
    done.ᐸꟷ(0);
}

internal static partial nint censusExec(nint k) {
    var (self, err) = Δos.Executable();
    if (err != nil) {
        return -1;
    }
    var (gate, done) = (new channel<EmptyStruct>(0), new channel<nint>(k));
    for (nint i = 0; i < k; i++) {
        var cmd = exec.Command(self);
        cmd.Env = append(Δos.Environ(), (@string)"MN_INV_CHILD=1", (@string)"GO2CS_MN=");
        if (cmd.Start() != nil) {
            return -1;
        }
        goǃ(waiter, gate, cmd, done.AsSendOnly());
    }
    close(gate);
    nint n = 0;
    for (nint i = 0; i < k; i++) {
        n += ᐸꟷ(done);
    }
    return n;
}

internal static void sleeper(channel<EmptyStruct> gate, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    time.Sleep(300 * time.Millisecond);
    done.ᐸꟷ(1);
}

internal static partial nint censusSleep(nint k) {
    var (gate, done) = (new channel<EmptyStruct>(0), new channel<nint>(k));
    for (nint i = 0; i < k; i++) {
        goǃ(sleeper, gate, done.AsSendOnly());
    }
    close(gate);
    nint n = 0;
    for (nint i = 0; i < k; i++) {
        n += ᐸꟷ(done);
    }
    return n;
}

internal static void stdinReader(channel<EmptyStruct> gate) {
    ᐸꟷ(gate);
    var b = new slice<byte>(1);
    Δos.Stdin.Read(b);
}

internal static partial void censusStdin(nint k) {
    var gate = new channel<EmptyStruct>(0);
    for (nint i = 0; i < k; i++) {
        goǃ(stdinReader, gate);
    }
    close(gate);
    time.Sleep(300 * time.Millisecond);
    fmt.Println("C-stdin blocked:", k);
    Δos.Exit(0);
}

internal static void resolver(channel<EmptyStruct> gate, channel/*<-*/<nint> done) {
    ᐸꟷ(gate);
    Δnet.LookupHost("go2cs-census.invalid");
    done.ᐸꟷ(1);
}

internal static partial nint censusDNS(nint k) {
    var (gate, done) = (new channel<EmptyStruct>(0), new channel<nint>(k));
    for (nint i = 0; i < k; i++) {
        goǃ(resolver, gate, done.AsSendOnly());
    }
    close(gate);
    nint n = 0;
    for (nint i = 0; i < k; i++) {
        n += ᐸꟷ(done);
    }
    return n;
}

internal static bool runs(@string row) {
    @string only = Δos.Getenv("MN_INV_ONLY");
    return only == ""u8 || only == row;
}

internal static void main() {
    if (Δos.Getenv("MN_INV_CHILD") == "1"u8) {
        time.Sleep(300 * time.Millisecond);
        return;
    }

    nint procs = Δruntime.GOMAXPROCS(0);
    nint k = 4 * procs;

    var exprᴛ1 = Δos.Getenv("MN_INV_ONLY");
    switch (ᐧ) {
    case {} when exprᴛ1 == "C-pipe"u8: {
        fmt.Println("C-pipe released:", censusPipe(k) == k);
        return;
    }
    case {} when exprᴛ1 == "C-accept"u8: {
        fmt.Println("C-accept released:", censusAccept(k) == k);
        return;
    }
    case {} when exprᴛ1 == "C-netread"u8: {
        fmt.Println("C-netread released:", censusNetRead(k) == k);
        return;
    }
    case {} when exprᴛ1 == "C-exec"u8: {
        fmt.Println("C-exec released:", censusExec(k) == k);
        return;
    }
    case {} when exprᴛ1 == "C-sleep"u8: {
        fmt.Println("C-sleep released:", censusSleep(k) == k);
        return;
    }
    case {} when exprᴛ1 == "C-chan"u8: {
        var (wait, set) = channelEvent();
        fmt.Println("C-chan released:", blockAll(k, wait, set) == k);
        return;
    }
    case {} when exprᴛ1 == "C-stdin"u8: {
        censusStdin(k);
        return;
    }
    case {} when exprᴛ1 == "V7"u8: {
        fmt.Println("V7 SetMaxThreads returned the old ceiling:", debug.SetMaxThreads(16));
        v2(k);
        return;
    }
    case {} when exprᴛ1 == "C-dns"u8: {
        fmt.Println("C-dns released:", censusDNS(k) == k);
        return;
    }}

    if (runs("V1")) {
        v1(k);
    }
    if (runs("V2")) {
        v2(k);
    }
    if (runs("V4")) {
        v4(procs);
    }
    if (runs("V5")) {
        fmt.Println("V5 a Blocks native call hands its P off at once:", bracketed(k, true));
    }
    if (runs("V6")) {
        fmt.Println("V6 a MayBlock native call has its P retaken, not injected:", bracketed(k, false));
    }
}

} // end main_package
