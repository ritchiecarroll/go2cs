namespace go;

using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

partial struct storm {
    internal Δsync.Mutex mu;
    internal nint counter;
    internal channel<EmptyStruct> start;
    internal channel<nint> tokens;
    internal channel<nint> results;
    internal Δsync.WaitGroup arrived;
    internal Δsync.WaitGroup done;
}

internal static void worker(this ж<storm> Ꮡs, nint id) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(Ꮡs.Ꮡdone.Done, ref ᒐ);
        Ꮡs.Ꮡarrived.Done();

        nint got = 0;

        var selᴛ1 = s.start;
        var selᴛ2 = s.tokens;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out var t): {
            got = t;
            ᐸꟷ(s.start);
            break;
        }}

        time.Sleep(time.Millisecond);

        s.mu.Lock();
        s.counter += id;
        s.mu.Unlock();

        s.results.ᐸꟷ(id + got);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// M:N SPIKE (R5b, B1): the hidden async twin of worker, written by hand as the twin generator (B2) would
// write it. Same statements in the same order; every call that can park is the callee's twin, awaited; the
// receiver stays the box, and the ref local is derived again after each await.
internal static async global::go.golib.GoTask workerꞏtwin(this ж<storm> Ꮡs, nint id) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(Ꮡs.Ꮡdone.Done, ref ᒐ);
        Ꮡs.Ꮡarrived.Done();

        nint got = 0;

        var selᴛ1 = s.start;
        var selᴛ2 = s.tokens;
        switch (await selectꞏtwin(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out var t): {
            got = t;
            s = ref Ꮡs.DerefOrNull();
            await s.start.Receiveꞏtwin();
            break;
        }}

        // MN_STORM_SYNCSLEEP=1: the one callee with no twin. The goroutine blocks its carrier's thread there.
        if (s_syncSleep) {
            time.Sleep(time.Millisecond);
        } else {
            await time.Sleepꞏtwin(time.Millisecond);
        }

        s = ref Ꮡs.DerefOrNull();
        await s.mu.Lockꞏtwin();
        s = ref Ꮡs.DerefOrNull();
        s.counter += id;
        s.mu.Unlock();

        await s.results.Sendꞏtwin(id + got);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// M:N SPIKE: what goǃ becomes for a callee that has a twin. With GO2CS_MN unset this is goǃ exactly.
internal static void goǃtwin(Action body, Func<global::go.golib.GoTask> twin, Delegate entry, [System.Runtime.CompilerServices.CallerFilePath] string ᴛcallerFile = "", [System.Runtime.CompilerServices.CallerLineNumber] int ᴛcallerLine = 0) {
    // MN_STORM_NOTWIN=1: a goroutine whose callee has no twin at all runs its synchronous body on a carrier.
    if (s_noTwin) {
        twin = () => { body(); return default; };
    }

    global::go.golib.Goroutine.StartTwin(body, twin, entry, ᴛcallerFile, ᴛcallerLine);
}

private static readonly bool s_noTwin = System.Environment.GetEnvironmentVariable("MN_STORM_NOTWIN") == "1";
private static readonly bool s_syncSleep = System.Environment.GetEnvironmentVariable("MN_STORM_SYNCSLEEP") == "1";

// M:N SPIKE micro-readings (MN_MODE=ping, MN_MODE=leaf): two goroutines trading a value over unbuffered
// channels, and one goroutine whose channel operations never park. Each has its body and its hand-written twin.
internal static void pinger(channel<nint> ping, channel<nint> pong, channel<nint> done, nint k) {
    for (nint i = 0; i < k; i++) {
        ping.ᐸꟷ(i);
        ᐸꟷ(pong);
    }

    done.ᐸꟷ(k);
}

internal static async global::go.golib.GoTask pingerꞏtwin(channel<nint> ping, channel<nint> pong, channel<nint> done, nint k) {
    for (nint i = 0; i < k; i++) {
        await ping.Sendꞏtwin(i);
        await pong.Receiveꞏtwin();
    }

    await done.Sendꞏtwin(k);
}

internal static void ponger(channel<nint> ping, channel<nint> pong, nint k) {
    for (nint i = 0; i < k; i++) {
        pong.ᐸꟷ(ᐸꟷ(ping) + 1);
    }
}

internal static async global::go.golib.GoTask pongerꞏtwin(channel<nint> ping, channel<nint> pong, nint k) {
    for (nint i = 0; i < k; i++) {
        await pong.Sendꞏtwin(await ping.Receiveꞏtwin() + 1);
    }
}

internal static void leaf(channel<nint> done, nint k) {
    var ch = new channel<nint>(1);
    long before = System.GC.GetAllocatedBytesForCurrentThread();
    var watch = System.Diagnostics.Stopwatch.StartNew();
    nint sum = 0;

    for (nint i = 0; i < k; i++) {
        ch.ᐸꟷ(i);
        sum += ᐸꟷ(ch);
    }

    watch.Stop();
    long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
    System.Console.Error.WriteLine($"READING leaf path=body k={k} ns-per-iteration={watch.Elapsed.TotalNanoseconds / k:F1} allocated-bytes={allocated} sum={sum}");
    done.ᐸꟷ(1);
}

internal static async global::go.golib.GoTask leafꞏtwin(channel<nint> done, nint k) {
    var ch = new channel<nint>(1);
    long before = System.GC.GetAllocatedBytesForCurrentThread();
    var watch = System.Diagnostics.Stopwatch.StartNew();
    nint sum = 0;

    for (nint i = 0; i < k; i++) {
        await ch.Sendꞏtwin(i);
        sum += await ch.Receiveꞏtwin();
    }

    watch.Stop();
    long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
    System.Console.Error.WriteLine($"READING leaf path=twin k={k} ns-per-iteration={watch.Elapsed.TotalNanoseconds / k:F1} allocated-bytes={allocated} sum={sum}");
    await done.Sendꞏtwin(1);
}

internal static bool microReadings() {
    string mode = System.Environment.GetEnvironmentVariable("MN_MODE") ?? "";
    nint k = envInt("MN_K", 200000);
    var done = new channel<nint>(0);

    if (mode == "ping") {
        var ping = new channel<nint>(0);
        var pong = new channel<nint>(0);
        long before = System.GC.GetTotalAllocatedBytes(false);
        var watch = System.Diagnostics.Stopwatch.StartNew();
        goǃtwin(() => ponger(ping, pong, k), () => pongerꞏtwin(ping, pong, k), (Action<channel<nint>, channel<nint>, nint>)ponger);
        goǃtwin(() => pinger(ping, pong, done, k), () => pingerꞏtwin(ping, pong, done, k), (Action<channel<nint>, channel<nint>, channel<nint>, nint>)pinger);
        ᐸꟷ(done);
        watch.Stop();
        long allocated = System.GC.GetTotalAllocatedBytes(false) - before;
        System.Console.Error.WriteLine($"READING ping k={k} ns-per-round-trip={watch.Elapsed.TotalNanoseconds / k:F0} bytes-per-round-trip={(double)allocated / k:F1}");
        return true;
    }

    // M:N PROTOTYPE control (MN_MODE=foreign): a twin that awaits an awaiter golib does not own is resumed off its
    // carrier. The scheduler's off-carrier counter has to read it, or a zero in that counter proves nothing.
    if (mode == "foreign") {
        var signal = new channel<nint>(1);
        Action body = () => signal.ᐸꟷ(1);
        goǃtwin(body, async () => {
            await System.Threading.Tasks.Task.Yield();
            await signal.Sendꞏtwin(1);
        }, body);
        ᐸꟷ(signal);
        System.Console.Error.WriteLine($"READING foreign done k={k}");
        return true;
    }

    if (mode == "leaf") {
        goǃtwin(() => leaf(done, k), () => leafꞏtwin(done, k), (Action<channel<nint>, nint>)leaf);
        ᐸꟷ(done);
        return true;
    }

    return false;
}

internal static nint envInt(@string name, nint fallback) {
    var (v, err) = strconv.Atoi(os.Getenv(name)); if (err == nil && v > 0) {
        return v;
    }

    return fallback;
}

internal static partial void Main() {
    if (microReadings()) {
        return;
    }

    nint n = envInt("MN_STORM_N", 100000);
    nint hold = envInt("MN_STORM_HOLD_MS", 0);

    var s = Ꮡ(new storm(
        start: new channel<EmptyStruct>(0),
        tokens: new channel<nint>(0),
        results: new channel<nint>(0)
    ));

    s.Ꮡarrived.Add(n);
    s.Ꮡdone.Add(n);

    for (nint iᴛ1 = 0; iᴛ1 < n; iᴛ1++) {
        var i = iᴛ1;
        if (i % 2 == 0) {
            var sʗ1 = s;
            goǃtwin(() => sʗ1.worker(i), () => sʗ1.workerꞏtwin(i), (Action<nint>)sʗ1.worker);
        } else {
            var sʗ2 = s;
            Action literal = () => {
                sʗ2.worker(i);
            };
            goǃtwin(literal, async () => {
                await sʗ2.workerꞏtwin(i);
            }, literal);
        }
    }

    s.Ꮡarrived.Wait();
    fmt.Println("parked:", n);

    if (hold > 0) {
        time.Sleep(((time.Duration)(int64)hold) * time.Millisecond);
    }

    nint tokens = n / 10;

    for (nint i = 0; i < tokens; i++) {
        s.tokens.ᐸꟷ(1);
    }

    close(s.start);

    nint total = 0;

    for (nint i = 0; i < n; i++) {
        total += ᐸꟷ(s.results);
    }

    s.Ꮡdone.Wait();

    nint want = n * (n - 1) / 2;

    fmt.Println("tokens:", tokens);
    fmt.Println("counter:", s.counter, "match:", s.counter == want);
    fmt.Println("total:", total, "match:", total == want + tokens);
}

} // end main_package
