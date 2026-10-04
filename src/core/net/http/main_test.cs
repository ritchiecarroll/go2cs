// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using fmt = fmt_package;
using io = io_package;
using log = log_package;
using Δhttp = global::go.net.http_package;
using os = os_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.net;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

internal static ж<log.Logger> quietLog = log.New(io.Discard, ""u8, 0);

public static void TestMain(ж<testing.M> Ꮡm) {
    http_internal_test_package.MaxWriteWaitBeforeConnReuse.Value = (time.Duration)(3600000000000L);
    nint v = Ꮡm.Run();
    if (v == 0 && goroutineLeaked()) {
        os.Exit(1);
    }
    os.Exit(v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testingMBeforeFunc1ˢ = "testing.(*M).before.func1"u8;
internal static readonly @string osSignalSignalRecvˢ = "os/signal.signal_recv"u8;
internal static readonly @string createdByNetStartServerˢ = "created by net.startServer"u8;
internal static readonly @string createdByTestingRunTestsˢ = "created by testing.RunTests"u8;
internal static readonly @string closeWriteAndWaitˢ = "closeWriteAndWait"u8;
internal static readonly @string testingMainˢ = "testing.Main("u8;
internal static readonly @string runtimeGoexitˢ = "runtime.goexit"u8;
internal static readonly @string createdByRuntimeGcˢ = "created by runtime.gc"u8;
internal static readonly @string interestingGoroutinesˢ = "interestingGoroutines"u8;
internal static readonly @string runtimeMHeapScavengerˢ = "runtime.MHeap_Scavenger"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<@string> /*gs*/ interestingGoroutines() {
    slice<@string> gs = default!;

    var buf = new slice<byte>((2 << (int)(20)));
    buf = buf.slice(0, runtime.Stack(buf, true));
    foreach (var (_, g) in strings.Split(((@string)buf), "\n\n"u8)) {
        var (_, stack, _) = strings.Cut(g, "\n"u8);
        stack = strings.TrimSpace(stack);
        if (stack == ""u8 || strings.Contains(stack, testingMBeforeFunc1ˢ) || strings.Contains(stack, osSignalSignalRecvˢ) || strings.Contains(stack, createdByNetStartServerˢ) || strings.Contains(stack, createdByTestingRunTestsˢ) || strings.Contains(stack, closeWriteAndWaitˢ) || strings.Contains(stack, testingMainˢ) || strings.Contains(stack, // These only show up with GOTRACEBACK=2; Issue 5005 (comment 28)
 runtimeGoexitˢ) || strings.Contains(stack, createdByRuntimeGcˢ) || strings.Contains(stack, interestingGoroutinesˢ) || strings.Contains(stack, runtimeMHeapScavengerˢ)) {
            continue;
        }
        gs = append(gs, stack);
    }
    slices.Sort<slice<@string>, @string>(gs);
    return gs;
}

// Verify the other tests didn't leave any goroutines running.
internal static bool goroutineLeaked() {
    if (testing.Short() || runningBenchmarks()) {
        // Don't worry about goroutine leaks in -short mode or in
        // benchmark mode. Too distracting when there are false positives.
        return false;
    }
    map<@string, nint> stackCount = default!;
    for (nint i = 0; i < 5; i++) {
        nint n = 0;
        stackCount = new map<@string, nint>();
        var gs = interestingGoroutines();
        foreach (var (_, g) in gs) {
            stackCount[g]++;
            n++;
        }
        if (n == 0) {
            return false;
        }
        // Wait for goroutines to schedule and die off:
        time.Sleep(100 * time.Millisecond);
    }
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "Too many goroutines running after net/http test(s).\n"u8);
    foreach (var (stack, count) in stackCount) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%d instances of:\n%s\n"u8, count, stack);
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http2ˢ = "HTTP2"u8;

// setParallel marks t as a parallel test if we're in short mode
// (all.bash), but as a serial test otherwise. Using t.Parallel isn't
// compatible with the afterTest func in non-short mode.
internal static void setParallel(ж<testing.T> Ꮡt) {
    if (strings.Contains(Ꮡt.Name(), http2ˢ)) {
        http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
    }
    if (testing.Short()) {
        Ꮡt.Parallel();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testBenchˢ = "-test.bench="u8;

internal static bool runningBenchmarks() {
    foreach (var (i, arg) in os.Args) {
        if (strings.HasPrefix(arg, testBenchˢ) && !strings.HasSuffix(arg, "="u8)) {
            return true;
        }
        if (arg == "-test.bench"u8 && i < len(os.Args) - 1 && os.Args[i + 1] != "") {
            return true;
        }
    }
    return false;
}

internal static bool leakReported;

internal static void afterTest(testing.TB t) {
    Δhttp.DefaultTransport._<ж<Δhttp.Transport>>().CloseIdleConnections();
    if (testing.Short()) {
        return;
    }
    if (leakReported) {
        // To avoid confusion, only report the first leak of each test run.
        // After the first leak has been reported, we can't tell whether the leaked
        // goroutines are a new leak from a subsequent test or just the same
        // goroutines from the first leak still hanging around, and we may add a lot
        // of latency waiting for them to exit at the end of each test.
        return;
    }
    // We shouldn't be running the leak check for parallel tests, because we might
    // report the goroutines from a test that is still running as a leak from a
    // completely separate test that has just finished. So we use non-atomic loads
    // and stores for the leakReported variable, and store every time we start a
    // leak check so that the race detector will flag concurrent leak checks as a
    // race even if we don't detect any leaks.
    leakReported = true;
    @string bad = default!;
    var badSubstring = new map<@string, @string>{
        [").readLoop("u8] = "a Transport"u8,
        [").writeLoop("u8] = "a Transport"u8,
        ["created by net/http/httptest.(*Server).Start"u8] = "an httptest.Server"u8,
        ["timeoutHandler"u8] = "a TimeoutHandler"u8,
        ["net.(*netFD).connect("u8] = "a timing out dial"u8,
        [").noteClientGone("u8] = "a closenotifier sender"u8
    };
    @string stacks = default!;
    for (nint i = 0; i < 2500; i++) {
        bad = ""u8;
        stacks = strings.Join(interestingGoroutines(), "\n\n"u8);
        foreach (var (substr, what) in badSubstring) {
            if (strings.Contains(stacks, substr)) {
                bad = what;
            }
        }
        if (bad == ""u8) {
            leakReported = false;
            return;
        }
        // Bad stuff found, but goroutines might just still be
        // shutting down, so give it some time.
        time.Sleep(1 * time.Millisecond);
    }
    t.Errorf("Test appears to have leaked %s:\n%s"u8, bad, stacks);
}

// waitCondition waits for fn to return true,
// checking immediately and then at exponentially increasing intervals.
internal static void waitCondition(testing.TB t, time.Duration delay, Func<time.Duration, bool> fn) {
    t.Helper();
    var start = time.Now();
    time.Duration since = default!;
    while (!fn(since)) {
        time.Sleep(delay);
        delay = 2 * delay - (delay / 2); // 1.5x, rounded up
        since = time.Since(start);
    }
}

} // end http_test_package
