// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δos = os_package;
using Δruntime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using net.@internal;
using socktest = net.@internal.socktest_package;
using static go.net_package;

partial class net_internal_test_package {


[GoType("dyn")] partial struct prohibitionaryDialArgTestsᴛ1 {
    internal @string network;
    internal @string address;
}
internal static slice<prohibitionaryDialArgTestsᴛ1> prohibitionaryDialArgTests = new prohibitionaryDialArgTestsᴛ1[]{
    new("tcp6"u8, "127.0.0.1"u8),
    new("tcp6"u8, "::ffff:127.0.0.1"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mappingIpv4AddressInsideˢ = (@string)"mapping ipv4 address inside ipv6 address not supported"u8;

public static void TestProhibitionaryDialArg(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        if (!supportsIPv4map()) {
            Ꮡt.Skip(mappingIpv4AddressInsideˢ);
        }
        var (ln, err) = Listen(tcpˢ, "[::]:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        (_, var port, err) = SplitHostPort(ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        foreach (var (i, tt) in prohibitionaryDialArgTests) {
            var (c, errΔ1) = Dial(tt.network, JoinHostPort(tt.address, port));
            if (errΔ1 == default!) {
                c.Close();
                Ꮡt.Errorf("#%d: %v"u8, i, errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDialLocal(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (_, port, err) = SplitHostPort(ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var c, err) = Dial(tcpˢ, JoinHostPort(""u8, port));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        c.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bothIPv4AndIPv6Areˢ = (@string)"both IPv4 and IPv6 are required"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDialerDualStackFDLeak(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("%s does not have full support of socktest"u8, Δruntime.GOOS);
        }
        else if (exprᴛ1 == "windows"u8) {
            Ꮡt.Skipf("not implemented a way to cancel dial racers in TCP SYN-SENT state on %s"u8, Δruntime.GOOS);
        }
        else if (exprᴛ1 == "openbsd"u8) {
            testenv.SkipFlaky(new net_test_package.testing_TжTB(Ꮡt), 15157);
        }

        if (!supportsIPv4() || !supportsIPv6()) {
            Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
        }
        var before = Ꮡsw.Sockets();
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        var handler = (ж<dualStackServer> dssΔ1, global::go.net_package.Listener ln) => {
            while (ᐧ) {
                var (c, errΔ1) = ln.Accept();
                if (errΔ1 != default!) {
                    return;
                }
                c.Close();
            }
        };
        var (dss, err) = newDualStackServer();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ2 = dss.buildup(handler); if (errΔ2 != default!) {
                dss.teardown();
                Ꮡt.Fatal(errΔ2);
            }
        }
        const nint N = 10;
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(N);
        var d = Ꮡ(new Dialer(DualStack: true, Timeout: (time.Duration)(5000000000L)));
        for (nint i = 0; i < N; i++) {
            var dʗ1 = d;
            var dssʗ1 = dss;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (c, errΔ3) = dʗ1.Dial(tcpˢ, JoinHostPort(localhostˢ, (~dssʗ1).port));
                    if (errΔ3 != default!) {
                        Ꮡt.Error(errΔ3);
                        return;
                    }
                    c.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
        dss.teardown();
        var after = Ꮡsw.Sockets();
        if (len(after) != len(before)) {
            Ꮡt.Errorf("got %d; want %d"u8, len(after), len(before));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Define a pair of blackholed (IPv4, IPv6) addresses, for which dialTCP is
// expected to hang until the timeout elapses. These addresses are reserved
// for benchmarking by RFC 6890.
internal static readonly @string slowDst4 = "198.18.0.254"u8;

internal static readonly @string slowDst6 = "2001:2::254"u8;

// In some environments, the slow IPs may be explicitly unreachable, and fail
// more quickly than expected. This test hook prevents dialTCP from returning
// before the deadline.
internal static (ж<global::go.net_package.TCPConn>, error) slowDialTCP(context.Context ctx, @string network, ж<global::go.net_package.TCPAddr> Ꮡladdr, ж<global::go.net_package.TCPAddr> Ꮡraddr) {
    ref var raddr = ref Ꮡraddr.DerefOrNull();

    var sd = Ꮡ(new sysDialer(network: network, address: Ꮡraddr.String()));
    var (c, err) = sd.doDialTCP(ctx, Ꮡladdr, Ꮡraddr);
    if (ParseIP(slowDst4).Equal(raddr.IP) || ParseIP(slowDst6).Equal(raddr.IP)) {
        // Wait for the deadline, or indefinitely if none exists.
        ᐸꟷ(ctx.Done());
    }
    return (c, err);
}

internal static time.Duration /*dialLatency*/ dialClosedPort(ж<testing.T> Ꮡt) {
    // On most platforms, dialing a closed port should be nearly instantaneous —
    // less than a few hundred milliseconds. However, on some platforms it may be
    // much slower: on Windows and OpenBSD, it has been observed to take up to a
    // few seconds.
    var (l, err) = Listen(tcpˢ, "127.0.0.1:0"u8);
    if (err != default!) {
        Ꮡt.Fatalf("dialClosedPort: Listen failed: %v"u8, err);
    }
    @string addr = l.Addr().String();
    l.Close();
    var startTime = time.Now();
    (var c, err) = Dial(tcpˢ, addr);
    if (err == default!) {
        c.Close();
    }
    var elapsed = time.Since(startTime);
    Ꮡt.Logf("dialClosedPort: measured delay %v"u8, elapsed);
    return elapsed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcp6ˢ = "tcp6"u8;
internal static readonly @string tcp4ˢ = "tcp4"u8;
internal static readonly @string unreachableˢ = "unreachable"u8;

[GoType("dyn")] internal partial struct TestDialParallel_type {
    internal slice<@string> primaries;
    internal slice<@string> fallbacks;
    internal @string teardownNetwork;
    internal bool expectOk;
    internal time.Duration expectElapsed;
}

public static void TestDialParallel(ж<testing.T> Ꮡt) {
    time.Duration instant = 0;
    time.Duration fallbackDelay = /* 200 * time.Millisecond */ 200000000;
    slice<@string> nCopies(@string s, nint n) {
        var @out = new slice<@string>(n);
        for (nint i = 0; i < n; i++) {
            @out[i] = s;
        }
        return @out;
    }
// These should just work on the first try.
// Primary is slow; fallback should kick in.
// Skip a "connection refused" in the primary thread.
// Skip a "connection refused" in the fallback thread.
// Primary refused, fallback without delay.
// Everything is refused.
// Nothing to do; fail instantly.
// Connecting to tons of addresses should not trip the deadline.
    slice<TestDialParallel_type> testCases = new TestDialParallel_type[]{
        new(new @string[]{"127.0.0.1"u8}.slice(), new @string[]{}.slice(), ""u8, true, instant),
        new(new @string[]{"::1"u8}.slice(), new @string[]{}.slice(), ""u8, true, instant),
        new(new @string[]{"127.0.0.1"u8, "::1"u8}.slice(), new @string[]{slowDst6}.slice(), "tcp6"u8, true, instant),
        new(new @string[]{"::1"u8, "127.0.0.1"u8}.slice(), new @string[]{slowDst4}.slice(), "tcp4"u8, true, instant),
        new(new @string[]{slowDst4}.slice(), new @string[]{"::1"u8}.slice(), ""u8, true, fallbackDelay),
        new(new @string[]{"127.0.0.1"u8, "::1"u8}.slice(), new @string[]{}.slice(), "tcp4"u8, true, instant),
        new(new @string[]{"::1"u8, "127.0.0.1"u8}.slice(), new @string[]{}.slice(), "tcp6"u8, true, instant),
        new(new @string[]{slowDst4, slowDst6}.slice(), new @string[]{"::1"u8, "127.0.0.1"u8}.slice(), "tcp6"u8, true, fallbackDelay),
        new(new @string[]{"127.0.0.1"u8}.slice(), new @string[]{"::1"u8}.slice(), "tcp4"u8, true, instant),
        new(new @string[]{"::1"u8}.slice(), new @string[]{"127.0.0.1"u8}.slice(), "tcp6"u8, true, instant),
        new(new @string[]{"127.0.0.1"u8}.slice(), new @string[]{}.slice(), "tcp4"u8, false, instant),
        new(new @string[]{}.slice(), new @string[]{}.slice(), ""u8, false, instant),
        new(nCopies("::1"u8, 1000), new @string[]{}.slice(), ""u8, true, instant)
    }.slice();
    // Convert a list of IP strings into TCPAddrs.
    global::go.net_package.addrList makeAddrs(slice<@string> ips, @string port) {
        global::go.net_package.addrList @out = default!;
        foreach (var (_, ip) in ips) {
            var (addr, err) = ResolveTCPAddr(tcpˢ, JoinHostPort(ip, port));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            @out = append(@out, (global::go.net_package.ΔAddr)(new global::go.net_package.TCPAddrжΔAddr(addr)));
        }
        return @out;
    }
    foreach (var (i, tt) in testCases) {
        nint iΔ1 = i;
        ref var ttΔ1 = ref heap<TestDialParallel_type>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var makeAddrsʗ1 = makeAddrs;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(fmt.Sprint(iΔ1), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            var ttʗ2 = ttʗ1;
            var dialTCP = (ж<global::go.net_package.TCPConn>, error) (context.Context ctxΔ1, @string network, ж<global::go.net_package.TCPAddr> laddr, ж<global::go.net_package.TCPAddr> raddr) => {
                @string n = tcp6ˢ;
                if ((~raddr).IP.To4() != default!) {
                    n = tcp4ˢ;
                }
                if (n == ttʗ2.teardownNetwork) {
                    return (default!, errors.New(unreachableˢ));
                }
                {
                    @string r = (~raddr).IP.String(); if (r == slowDst4 || r == slowDst6) {
                        ᐸꟷ(ctxΔ1.Done());
                        return (default!, ctxΔ1.Err());
                    }
                }
                return (Ꮡ(new TCPConn(nil)), default!);
            };
            var primaries = makeAddrsʗ1(ttʗ1.primaries, "80"u8);
            var fallbacks = makeAddrsʗ1(ttʗ1.fallbacks, "80"u8);
            ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
            d = new Dialer(
                FallbackDelay: fallbackDelay
            );
            time.Duration forever = /* 60 * time.Minute */ 3600000000000;
            if (ttʗ1.expectElapsed == instant) {
                d.FallbackDelay = forever;
            }
            var startTime = time.Now();
            var sd = Ꮡ(new sysDialer(
                Dialer: d,
                network: "tcp"u8,
                address: "?"u8,
                testHookDialTCP: dialTCP
            ));
            var (c, err) = sd.dialParallel(context.Background(), primaries, fallbacks);
            var elapsed = time.Since(startTime);
            if (c != default!) {
                c.Close();
            }
            if (ttʗ1.expectOk && err != default!){
                tΔ1.Errorf("#%d: got %v; want nil"u8, iΔ1, err);
            } else 
            if (!ttʗ1.expectOk && err == default!) {
                tΔ1.Errorf("#%d: got nil; want non-nil"u8, iΔ1);
            }
            if (elapsed < ttʗ1.expectElapsed || elapsed >= forever) {
                tΔ1.Errorf("#%d: got %v; want >= %v, < forever"u8, iΔ1, elapsed, ttʗ1.expectElapsed);
            }
            // Repeat each case, ensuring that it can be canceled.
            var (ctx, cancel) = context.WithCancel(context.Background());
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(1);
            var cancelʗ1 = cancel;
            goǃ(() => {
                time.Sleep(5 * time.Millisecond);
                cancelʗ1();
                Ꮡwg.Done();
            });
            // Ignore errors, since all we care about is that the
            // call can be canceled.
            (c, _) = sd.dialParallel(ctx, primaries, fallbacks);
            if (c != default!) {
                c.Close();
            }
            Ꮡwg.Wait();
        });
    }
}

internal static (slice<global::go.net_package.IPAddr>, error) lookupSlowFast(context.Context ctx, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) {
    var exprᴛ1 = host;
    if (exprᴛ1 == "slow6loopback4"u8) {
        return (new global::go.net_package.IPAddr[]{ // Returns a slow IPv6 address, and a local IPv4 address.

            new(IP: ParseIP(slowDst6)),
            new(IP: ParseIP("127.0.0.1"u8))
        }.slice(), default!);
    }
    { /* default: */
        return fn(ctx, network, host);
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string slow6loopback4ˢ = "slow6loopback4"u8;

[GoType("dyn")] internal partial struct TestDialerFallbackDelay_type {
    internal bool dualstack;
    internal time.Duration delay;
    internal time.Duration expectElapsed;
}

public static void TestDialerFallbackDelay(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv4() || !supportsIPv6()) {
            Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
        }
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupSlowFast;
        var origTestHookDialTCP = testHookDialTCP;
        var origTestHookDialTCPʗ1 = origTestHookDialTCP;
        defer(() => {
            testHookDialTCP = origTestHookDialTCPʗ1;
        }, ref ᒐ);
        testHookDialTCP = slowDialTCP;
// Use a very brief delay, which should fallback immediately.
// Use a 200ms explicit timeout.
// The default is 300ms.
        slice<TestDialerFallbackDelay_type> testCases = new TestDialerFallbackDelay_type[]{
            new(true, 1 * time.ΔNanosecond, 0),
            new(true, 200 * time.Millisecond, 200 * time.Millisecond),
            new(true, 0, 300 * time.Millisecond)
        }.slice();
        var handler = (ж<dualStackServer> dssΔ1, global::go.net_package.Listener ln) => {
            while (ᐧ) {
                var (c, errΔ1) = ln.Accept();
                if (errΔ1 != default!) {
                    return;
                }
                c.Close();
            }
        };
        var (dss, err) = newDualStackServer();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dssʗ1 = dss;
        defer(() => dssʗ1.teardown(), ref ᒐ);
        {
            var errΔ2 = dss.buildup(handler); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        foreach (var (i, tt) in testCases) {
            var d = Ꮡ(new Dialer(DualStack: tt.dualstack, FallbackDelay: tt.delay));
            var startTime = time.Now();
            var (c, errΔ3) = d.Dial(tcpˢ, JoinHostPort(slow6loopback4ˢ, (~dss).port));
            var elapsed = time.Since(startTime);
            if (errΔ3 == default!){
                c.Close();
            } else 
            if (tt.dualstack) {
                Ꮡt.Error(errΔ3);
            }
            var expectMin = tt.expectElapsed - 1 * time.Millisecond;
            var expectMax = tt.expectElapsed + 95 * time.Millisecond;
            if (elapsed < expectMin) {
                Ꮡt.Errorf("#%d: got %v; want >= %v"u8, i, elapsed, expectMin);
            }
            if (elapsed > expectMax) {
                Ꮡt.Errorf("#%d: got %v; want <= %v"u8, i, elapsed, expectMax);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDialParallelSpuriousConnection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (!supportsIPv4() || !supportsIPv6()) {
            Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
        }
        ref var readDeadline = ref heap(new time.Time(), out var ᏑreadDeadline);
        {
            var (td, ok) = t.Deadline(); if (ok){
                time.Duration arbitraryCleanupMargin = /* 1 * time.Second */ 1000000000;
                readDeadline = td.Add(-arbitraryCleanupMargin);
            } else {
                readDeadline = time.Now().Add((time.Duration)(5000000000L));
            }
        }
        ref var closed = ref heap(new Δsync.WaitGroup(), out var Ꮡclosed);
        Ꮡclosed.Add(2);
        var readDeadlineʗ1 = readDeadline;
        var handler = (ж<dualStackServer> dssΔ1, global::go.net_package.Listener ln) => {
            // Accept one connection per address.
            var (cΔ1, errΔ1) = ln.Accept();
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            // Workaround for https://go.dev/issue/37795.
            // On arm64 macOS (current as of macOS 12.4),
            // reading from a socket at the same time as the client
            // is closing it occasionally hangs for 60 seconds before
            // returning ECONNRESET. Sleep for a bit to give the
            // socket time to close before trying to read from it.
            if (Δruntime.GOOS == "darwin"u8 && Δruntime.GOARCH == "arm64"u8) {
                time.Sleep(10 * time.Millisecond);
            }
            // The client should close itself, without sending data.
            cΔ1.SetReadDeadline(readDeadlineʗ1);
            array<byte> b = new(1);
            {
                var (_, errΔ2) = cΔ1.Read(b[..]); if (!AreEqual(errΔ2, Δio.EOF)) {
                    Ꮡt.Errorf("got %v; want %v"u8, errΔ2, Δio.EOF);
                }
            }
            cΔ1.Close();
            Ꮡclosed.Done();
        };
        var (dss, err) = newDualStackServer();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dssʗ1 = dss;
        defer(() => dssʗ1.teardown(), ref ᒐ);
        {
            var errΔ3 = dss.buildup(handler); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        time.Duration fallbackDelay = /* 100 * time.Millisecond */ 100000000;
        ref var dialing = ref heap(new Δsync.WaitGroup(), out var Ꮡdialing);
        Ꮡdialing.Add(2);
        var origTestHookDialTCP = testHookDialTCP;
        var origTestHookDialTCPʗ1 = origTestHookDialTCP;
        defer(() => {
            testHookDialTCP = origTestHookDialTCPʗ1;
        }, ref ᒐ);
        testHookDialTCP = (context.Context ctx, @string net, ж<global::go.net_package.TCPAddr> laddr, ж<global::go.net_package.TCPAddr> raddr) => {
            // Wait until Happy Eyeballs kicks in and both connections are dialing,
            // and inhibit cancellation.
            // This forces dialParallel to juggle two successful connections.
            Ꮡdialing.Done();
            Ꮡdialing.Wait();
            // Now ignore the provided context (which will be canceled) and use a
            // different one to make sure this completes with a valid connection,
            // which we hope to be closed below:
            var sdΔ1 = Ꮡ(new sysDialer(network: net, address: raddr.String()));
            return sdΔ1.doDialTCP(context.Background(), laddr, raddr);
        };
        ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
        d = new Dialer(
            FallbackDelay: fallbackDelay
        );
        var sd = Ꮡ(new sysDialer(
            Dialer: d,
            network: "tcp"u8,
            address: "?"u8
        ));
        var dssʗ2 = dss;
        global::go.net_package.addrList makeAddr(@string ip) {
            var (addr, errΔ4) = ResolveTCPAddr(tcpˢ, JoinHostPort(ip, (~dssʗ2).port));
            if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
            return new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(addr)}.slice());
        }
        // dialParallel returns one connection (and closes the other.)
        (var c, err) = sd.dialParallel(context.Background(), makeAddr("127.0.0.1"u8), makeAddr("::1"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        c.Close();
        // The server should've seen both connections.
        Ꮡclosed.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestDialerPartialDeadline_type {
    internal time.Time now;
    internal time.Time deadline;
    internal nint addrs;
    internal time.Time expectDeadline;
    internal error expectErr;
}

public static void TestDialerPartialDeadline(ж<testing.T> Ꮡt) {
    var now = time.Date(2000, time.January, 1, 0, 0, 0, 0, time.ΔUTC);
// Regular division.
// Bump against the 2-second sane minimum.
// Total available is now below the sane minimum.
// Null deadline.
// Step the clock forward and cross the deadline.
    slice<TestDialerPartialDeadline_type> testCases = new TestDialerPartialDeadline_type[]{
        new(now, now.Add((time.Duration)(12000000000L)), 1, now.Add((time.Duration)(12000000000L)), default!),
        new(now, now.Add((time.Duration)(12000000000L)), 2, now.Add((time.Duration)(6000000000L)), default!),
        new(now, now.Add((time.Duration)(12000000000L)), 3, now.Add((time.Duration)(4000000000L)), default!),
        new(now, now.Add((time.Duration)(12000000000L)), 999, now.Add(2 * time.ΔSecond), default!),
        new(now, now.Add(1900 * time.Millisecond), 999, now.Add(1900 * time.Millisecond), default!),
        new(now, noDeadline, 1, noDeadline, default!),
        new(now.Add(-1 * time.Millisecond), now, 1, now, default!),
        new(now.Add(0 * time.Millisecond), now, 1, noDeadline, errTimeout),
        new(now.Add(1 * time.Millisecond), now, 1, noDeadline, errTimeout)
    }.slice();
    foreach (var (i, tt) in testCases) {
        var (deadline, err) = partialDeadline(tt.now, tt.deadline, tt.addrs);
        if (!AreEqual(err, tt.expectErr)) {
            Ꮡt.Errorf("#%d: got %v; want %v"u8, i, err, tt.expectErr);
        }
        if (!deadline.Equal(tt.expectDeadline)) {
            Ꮡt.Errorf("#%d: got %v; want %v"u8, i, deadline, tt.expectDeadline);
        }
    }
}

// isEADDRINUSE reports whether err is syscall.EADDRINUSE.
internal static Func<error, bool> isEADDRINUSE = (error err) => false;

[GoType("dyn")] [GoLocalName("test")] internal partial struct TestDialerLocalAddr_test {
    internal @string network, raddr;
    internal global::go.net_package.ΔAddr laddr;
    [GoEmbedded] internal error error;
}

public static void TestDialerLocalAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!supportsIPv4() || !supportsIPv6()) {
            Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
        }
        slice<TestDialerLocalAddr_test> tests = new TestDialerLocalAddr_test[]{
            new("tcp4"u8, "127.0.0.1"u8, default!, default!),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(nil))), default!),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8)))), default!),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8).To4()))), default!),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::"u8)))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To4()))), default!),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To16()))), default!),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), errNoSuitableAddress),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp4"u8, "127.0.0.1"u8, new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp6"u8, "::1"u8, default!, default!),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(nil))), default!),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8)))), default!),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8).To4()))), default!),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::"u8)))), default!),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To4()))), errNoSuitableAddress),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To16()))), errNoSuitableAddress),
            new("tcp6"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), default!),
            new("tcp6"u8, "::1"u8, new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp6"u8, "::1"u8, new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp"u8, "127.0.0.1"u8, default!, default!),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(nil))), default!),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8)))), default!),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8).To4()))), default!),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To4()))), default!),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To16()))), default!),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), errNoSuitableAddress),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp"u8, "127.0.0.1"u8, new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp"u8, "::1"u8, default!, default!),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(nil))), default!),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8)))), default!),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0"u8).To4()))), default!),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::"u8)))), default!),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To4()))), errNoSuitableAddress),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8).To16()))), errNoSuitableAddress),
            new("tcp"u8, "::1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), default!),
            new("tcp"u8, "::1"u8, new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))),
            new("tcp"u8, "::1"u8, new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr(nil))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8))))
        }.slice();
        nint issue34264Index = -1;
        if (supportsIPv4map()){
            issue34264Index = len(tests);
            tests = append(tests, new TestDialerLocalAddr_test(
                "tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::"u8)))), default!
            ));
        } else {
            tests = append(tests, new TestDialerLocalAddr_test(
                "tcp"u8, "127.0.0.1"u8, new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::"u8)))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"u8)))
            ));
        }
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        var handler = (ж<localServer> ls, global::go.net_package.Listener ln) => {
            while (ᐧ) {
                var (c, err) = ln.Accept();
                if (err != default!) {
                    return;
                }
                c.Close();
            }
        };
        ref var lss = ref heap(new array<ж<localServer>>(2), out var Ꮡlss);
        foreach (var (i, network) in new @string[]{"tcp4"u8, "tcp6"u8}.slice()) {
            lss[i] = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), network);
            var lssʗ1 = lss;
            defer(() => lssʗ1[i].teardown(), ref ᒐ);
            {
                var err = lss[i].buildup(handler); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        }
        foreach (var (i, tt) in tests) {
            var d = Ꮡ(new Dialer(LocalAddr: tt.laddr));
            @string addr = default!;
            var ip = ParseIP(tt.raddr);
            if (ip.To4() != default!) {
                addr = (~lss[0]).Listener.Addr().String();
            }
            if (ip.To16() != default! && ip.To4() == default!) {
                addr = (~lss[1]).Listener.Addr().String();
            }
            var (c, err) = d.Dial(tt.network, addr);
            if (err == default! && tt.error != default! || err != default! && tt.error == default!) {
                if (i == issue34264Index && Δruntime.GOOS == "freebsd"u8 && isEADDRINUSE(err)){
                    // https://golang.org/issue/34264: FreeBSD through at least version 12.2
                    // has been observed to fail with EADDRINUSE when dialing from an IPv6
                    // local address to an IPv4 remote address.
                    Ꮡt.Logf("%s %v->%s: got %v; want %v"u8, tt.network, tt.laddr, tt.raddr, err, tt.error);
                    Ꮡt.Logf("(spurious EADDRINUSE ignored on freebsd: see https://golang.org/issue/34264)"u8);
                } else {
                    Ꮡt.Errorf("%s %v->%s: got %v; want %v"u8, tt.network, tt.laddr, tt.raddr, err, tt.error);
                }
            }
            if (err != default!) {
                {
                    var perr = parseDialError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                continue;
            }
            c.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDialerDualStack(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.SkipFlaky(new net_test_package.testing_TжTB(Ꮡt), 13324);
        if (!supportsIPv4() || !supportsIPv6()) {
            Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
        }
        var closedPortDelay = dialClosedPort(Ꮡt);
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        var handler = (ж<dualStackServer> dss, global::go.net_package.Listener ln) => {
            while (ᐧ) {
                var (c, err) = ln.Accept();
                if (err != default!) {
                    return;
                }
                c.Close();
            }
        };
        ref var timeout = ref heap(new time.Duration(), out var Ꮡtimeout);

        timeout = 150 * time.Millisecond + closedPortDelay;
        foreach (var (_, vᴛ1) in new bool[]{false, true}.slice()) {
            ref var dualstack = ref heap(new bool(), out var Ꮡdualstack);
            dualstack = vᴛ1;

            var (dss, err) = newDualStackServer();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var dssʗ1 = dss;
            defer(() => dssʗ1.teardown(), ref ᒐ);
            {
                var errΔ1 = dss.buildup(handler); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            var d = Ꮡ(new Dialer(DualStack: dualstack, Timeout: timeout));
            foreach ((_, _) in (~dss).lns) {
                var (c, errΔ2) = d.Dial(tcpˢ, JoinHostPort(localhostˢ, (~dss).port));
                if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                    continue;
                }
                {
                    var addr = c.LocalAddr()._<ж<global::go.net_package.TCPAddr>>();
                    switch (ᐧ) {
                    case {} when (~addr).IP.To4() != default!: {
                        dss.teardownNetwork(tcp4ˢ);
                        break;
                    }
                    case {} when (~addr).IP.To16() != default! && (~addr).IP.To4() == default!: {
                        dss.teardownNetwork(tcp6ˢ);
                        break;
                    }}
                }

                c.Close();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestDialerKeepAlive_tests {
    internal time.Duration ka;
    internal time.Duration expected;
}

public static void TestDialerKeepAlive(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Cleanup(() => {
            testHookSetKeepAlive = (global::go.net_package.KeepAliveConfig _) => {
            };
        });
        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener lnΔ1) => {
            while (ᐧ) {
                var (c, err) = lnΔ1.Accept();
                if (err != default!) {
                    return;
                }
                c.Close();
            }
        };
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ, Ꮡ(new ListenConfig(
            KeepAlive: -1
        )));
        // prevent calling hook from accepting
        var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var err = ls.buildup(handler); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        var tests = new TestDialerKeepAlive_tests[]{
            new(-1, -1),
            new(0, 0),
            new((time.Duration)(5000000000L), (time.Duration)(5000000000L)),
            new((time.Duration)(30000000000L), (time.Duration)(30000000000L))
        }.slice();
        time.Duration got = -1;
        testHookSetKeepAlive = (global::go.net_package.KeepAliveConfig cfg) => {
            got = cfg.Idle;
        };
        foreach (var (_, test) in tests) {
            got = -1;
            ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
            d = new Dialer(KeepAlive: test.ka);
            var (c, err) = Ꮡd.Dial(tcpˢ, (~ls).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            c.Close();
            if (got != test.expected) {
                Ꮡt.Errorf("Dialer.KeepAlive = %v: SetKeepAlive set to %v, want %v"u8, d.KeepAlive, got, test.expected);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timeoutWaitingForDialToˢ = (@string)"timeout waiting for dial to fail"u8;
internal static readonly object unexpectedSuccessfulˢ = (@string)"unexpected successful connection"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDialCancel(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        mustHaveExternalNetwork(Ꮡt);
        @string blackholeIPPort = JoinHostPort(slowDst4, "1234"u8);
        if (!supportsIPv4()) {
            blackholeIPPort = JoinHostPort(slowDst6, "1234"u8);
        }
        var ticker = time.NewTicker(10 * time.Millisecond);
        var tickerʗ1 = ticker;
        defer(tickerʗ1.Stop, ref ᒐ);
        const nint cancelTick = 5; // the timer tick we cancel the dial at
        const nint timeoutTick = 100;
        ref var d = ref heap(new global::go.net_package.Dialer(), out var Ꮡd);
        var cancel = new channel<EmptyStruct>(0);
        d.Cancel = cancel.WithDirection(GoChanDir.Recv);
        var errc = new channel<error>(1);
        var connc = new channel<global::go.net_package.Conn>(1);
        var conncʗ1 = connc;
        var errcʗ1 = errc;
        goǃ(() => {
            {
                var (c, err) = Ꮡd.Dial(tcpˢ, blackholeIPPort); if (err != default!){
                    errcʗ1.ᐸꟷ(err);
                } else {
                    conncʗ1.ᐸꟷ(c);
                }
            }
        });
        nint ticks = 0;
        while (ᐧ) {
            var selᴛ1 = (~ticker).C;
            var selᴛ2 = connc;
            var selᴛ3 = errc;
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                ticks++;
                if (ticks == cancelTick) {
                    builtin.close(cancel);
                }
                if (ticks == timeoutTick) {
                    Ꮡt.Fatal(timeoutWaitingForDialToˢ);
                }
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out var c): {
                c.Close();
                Ꮡt.Fatal(unexpectedSuccessfulˢ);
                break;
            }
            case 2 when selᴛ3.ꟷᐳ(out var err): {
                {
                    var perr = parseDialError(err); if (perr != default!) {
                        Ꮡt.Error(perr);
                    }
                }
                if (ticks < cancelTick) {
                    // Using strings.Contains is ugly but
                    // may work on plan9 and windows.
                    var ignorable = new @string[]{
                        "connection refused"u8,
                        "unreachable"u8,
                        "no route to host"u8,
                        "invalid argument"u8
                    }.slice();
                    @string e = err.Error();
                    foreach (var (_, ignore) in ignorable) {
                        if (strings.Contains(e, ignore)) {
                            Ꮡt.Skipf("connection to %v failed fast with %v"u8, blackholeIPPort, err);
                        }
                    }
                    Ꮡt.Fatalf("dial error after %d ticks (%d before cancel sent): %v"u8,
                        ticks, cancelTick - ticks, err);
                }
                {
                    var (oe, ok) = err._<ж<global::go.net_package.OpError>>(ᐧ); if (!ok || !AreEqual((~oe).Err, errCanceled)) {
                        Ꮡt.Fatalf("dial error = %v (%T); want OpError with Err == errCanceled"u8, err, err);
                    }
                }
                return; // success.
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object avoidingTimeSleepˢ = (@string)"avoiding time.Sleep"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string messageᶜ = "echo!\n"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCancelAfterDial(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(avoidingTimeSleepˢ);
        }
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var lnʗ1 = ln;
        defer(() => {
            lnʗ1.Close();
            Ꮡwg.Wait();
        }, ref ᒐ);
        // Echo back the first line of each incoming connection.
        var lnʗ2 = ln;
        goǃ(() => {
            while (ᐧ) {
                var (c, err) = lnʗ2.Accept();
                if (err != default!) {
                    break;
                }
                var rb = bufio.NewReader(new net_test_package.net_ConnᴠReader(c));
                (var line, err) = rb.ReadString((rune)'\n');
                if (err != default!) {
                    Ꮡt.Error(err);
                    c.Close();
                    continue;
                }
                {
                    var (_, errΔ1) = c.Write(slice<byte>(line)); if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                    }
                }
                c.Close();
            }
            Ꮡwg.Done();
        });
        var lnʗ3 = ln;
        void @try() {
            GoFrame ᒐ = default;
            try {
                var cancel = new channel<EmptyStruct>(0);
                var d = Ꮡ(new Dialer(Cancel: cancel));
                var (c, err) = d.Dial(tcpˢ, lnʗ3.Addr().String());
                // Immediately after dialing, request cancellation and sleep.
                // Before Issue 15078 was fixed, this would cause subsequent operations
                // to fail with an i/o timeout roughly 50% of the time.
                builtin.close(cancel);
                time.Sleep(10 * time.Millisecond);
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                // Send some data to confirm that the connection is still alive.
                @string message = messageᶜ;
                {
                    var (_, errΔ1) = c.Write(slice<byte>(message)); if (errΔ1 != default!) {
                        Ꮡt.Fatal(errΔ1);
                    }
                }
                // The server should echo the line, and close the connection.
                var rb = bufio.NewReader(new net_test_package.net_ConnᴠReader(c));
                (var line, err) = rb.ReadString((rune)'\n');
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                if (line != message) {
                    Ꮡt.Errorf("got %q; want %q"u8, line, message);
                }
                {
                    var (_, errΔ2) = rb.ReadByte(); if (!AreEqual(errΔ2, Δio.EOF)) {
                        Ꮡt.Errorf("got %v; want %v"u8, errΔ2, Δio.EOF);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        // This bug manifested about 50% of the time, so try it a few times.
        for (nint i = 0; i < 10; i++) {
            @try();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWindowsOnlyTestˢ = (@string)"skipping windows only test"u8;
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;

public static void TestDialClosedPortFailFast(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δruntime.GOOS != "windows"u8) {
        // Reported by go.dev/issues/23366.
        Ꮡt.Skip(skippingWindowsOnlyTestˢ);
    }
    foreach (var (_, network) in new @string[]{"tcp"u8, "tcp4"u8, "tcp6"u8}.slice()) {
        Ꮡt.Run(network, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(network)) {
                    tΔ1.Skipf("skipping: can't listen on %s"u8, network);
                }
                // Reserve a local port till the end of the
                // test by opening a listener and connecting to
                // it using Dial.
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network);
                @string addr = ln.Addr().String();
                var (conn1, err) = Dial(network, addr);
                if (err != default!) {
                    ln.Close();
                    tΔ1.Fatal(err);
                }
                var conn1ʗ1 = conn1;
                defer(() => conn1ʗ1.Close(), ref ᒐ);
                // Now close the listener so the next Dial fails
                // keeping conn1 alive so the port is not made
                // available.
                ln.Close();
                var maxElapsed = time.ΔSecond;
                // The host can be heavy-loaded and take
                // longer than configured. Retry until
                // Dial takes less than maxElapsed or
                // the test times out.
                while (ᐧ) {
                    var startTime = time.Now();
                    var (conn2, errΔ1) = Dial(network, addr);
                    if (errΔ1 == default!) {
                        conn2.Close();
                        tΔ1.Fatal(errorExpectedˢ);
                    }
                    var elapsed = time.Since(startTime);
                    if (elapsed < maxElapsed) {
                        break;
                    }
                    tΔ1.Logf("got %v; want < %v"u8, elapsed, maxElapsed);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localhost0ˢ = "localhost:0"u8;

// Issue 18806: it should always be possible to net.Dial a
// net.Listener().Addr().String when the listen address was ":n", even
// if the machine has halfway configured IPv6 such that it can bind on
// "::" not connect back to that same address.
public static void TestDialListenerAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (!testableNetwork(tcp4ˢ)) {
            Ꮡt.Skipf("skipping: can't listen on tcp4"u8);
        }
        // The original issue report was for listening on just ":0" on a system that
        // supports both tcp4 and tcp6 for external traffic but only tcp4 for loopback
        // traffic. However, the port opened by ":0" is externally-accessible, and may
        // trigger firewall alerts or otherwise be mistaken for malicious activity
        // (see https://go.dev/issue/59497). Moreover, it often does not reproduce
        // the scenario in the issue, in which the port *cannot* be dialed as tcp6.
        //
        // To address both of those problems, we open a tcp4-only localhost port, but
        // then dial the address string that the listener would have reported for a
        // dual-stack port.
        var (ln, err) = Listen(tcp4ˢ, localhost0ˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        Ꮡt.Logf("listening on %q"u8, ln.Addr());
        (_, var port, err) = SplitHostPort(ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // If we had opened a dual-stack port without an explicit "localhost" address,
        // the Listener would arbitrarily report an empty tcp6 address in its Addr
        // string.
        //
        // The documentation for Dial says ‘if the host is empty or a literal
        // unspecified IP address, as in ":80", "0.0.0.0:80" or "[::]:80" for TCP and
        // UDP, "", "0.0.0.0" or "::" for IP, the local system is assumed.’
        // In #18806, it was decided that that should include the local tcp4 host
        // even if the string is in the tcp6 format.
        @string dialAddr = "[::]:"u8 + port;
        (var c, err) = Dial(tcp4ˢ, dialAddr);
        if (err != default!) {
            Ꮡt.Fatalf(@"Dial(""tcp4"", %q): %v"u8, dialAddr, err);
        }
        c.Close();
        Ꮡt.Logf(@"Dial(""tcp4"", %q) succeeded"u8, dialAddr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string streamDialˢ = "StreamDial"u8;
internal static readonly @string packetDialˢ = "PacketDial"u8;

public static void TestDialerControl(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }
    else if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skipf("skipping: fake net does not support Dialer.Control"u8);
    }

    Ꮡt.Run(streamDialˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            foreach (var (_, network) in new @string[]{"tcp"u8, "tcp4"u8, "tcp6"u8, "unix"u8, "unixpacket"u8}.slice()) {
                if (!testableNetwork(network)) {
                    continue;
                }
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network);
                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
                d = new Dialer(Control: controlOnConnSetup);
                var (c, err) = Ꮡd.Dial(network, ln.Addr().String());
                if (err != default!) {
                    tΔ1.Error(err);
                    continue;
                }
                c.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡt.Run(packetDialˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            foreach (var (_, network) in new @string[]{"udp"u8, "udp4"u8, "udp6"u8, "unixgram"u8}.slice()) {
                if (!testableNetwork(network)) {
                    continue;
                }
                var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(tΔ2), network);
                if (network == "unixgram"u8) {
                    defer(Δos.Remove, c1.LocalAddr().String(), ref ᒐ);
                }
                var c1ʗ1 = c1;
                defer(() => c1ʗ1.Close(), ref ᒐ);
                ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
                d = new Dialer(Control: controlOnConnSetup);
                var (c2, err) = Ꮡd.Dial(network, c1.LocalAddr().String());
                if (err != default!) {
                    tΔ2.Error(err);
                    continue;
                }
                c2.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

public static void TestDialerControlContext(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("%s does not have full support of socktest"u8, Δruntime.GOOS);
    }
    else if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skipf("skipping: fake net does not support Dialer.ControlContext"u8);
    }

    Ꮡt.Run(streamDialˢ, (ж<testing.T> tΔ1) => {
        foreach (var (i, network) in new @string[]{"tcp"u8, "tcp4"u8, "tcp6"u8, "unix"u8, "unixpacket"u8}.slice()) {
            tΔ1.Run(network, (ж<testing.T> tΔ2) => {
                GoFrame ᒐ = default;
                try {
                    if (!testableNetwork(network)) {
                        tΔ2.Skipf("skipping: %s not available"u8, network);
                    }
                    var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ2), network);
                    var lnʗ1 = ln;
                    defer(() => lnʗ1.Close(), ref ᒐ);
                    nint id = default!;
                    ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
                    d = new Dialer(ControlContext: (context.Context ctx, @string networkΔ1, @string address, syscall.RawConn cΔ1) => {
                        id = ctx.Value((@string)"id"u8)._<nint>();
                        return controlOnConnSetup(networkΔ1, address, cΔ1);
                    }
                    );
                    var (c, err) = Ꮡd.DialContext(context.WithValue(context.Background(), (@string)"id"u8, i + 1), network, ln.Addr().String());
                    if (err != default!) {
                        tΔ2.Fatal(err);
                    }
                    if (id != i + 1) {
                        tΔ2.Errorf("got id %d, want %d"u8, id, i + 1);
                    }
                    c.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    });
}

// mustHaveExternalNetwork is like testenv.MustHaveExternalNetwork
// except on non-Linux, non-mobile builders it permits the test to
// run in -short mode.
internal static void mustHaveExternalNetwork(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Helper();
    var definitelyHasLongtestBuilder = Δruntime.GOOS == "linux"u8;
    var mobile = Δruntime.GOOS == "android"u8 || Δruntime.GOOS == "ios"u8;
    var fake = Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8;
    if (testenv.Builder() != ""u8 && !definitelyHasLongtestBuilder && !mobile && !fake) {
        // On a non-Linux, non-mobile builder (e.g., freebsd-amd64-13_0).
        //
        // Don't skip testing because otherwise the test may never run on
        // any builder if this port doesn't also have a -longtest builder.
        return;
    }
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
}

[GoType] internal partial struct contextWithNonZeroDeadline {
    [GoEmbedded] public context_package.Context Context;
}

internal static (time.Time, bool) Deadline(this contextWithNonZeroDeadline _) {
    // Return non-zero time.Time value with false indicating that no deadline is set.
    return (time.Unix(0, 0), false);
}

public static void TestDialWithNonZeroDeadline(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (_, port, err) = SplitHostPort(ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ctx = new contextWithNonZeroDeadline(Context: context.Background());
        ref var dialer = ref heap(new global::go.net_package.Dialer(), out var Ꮡdialer);
        (var c, err) = Ꮡdialer.DialContext(ctx, tcpˢ, JoinHostPort(""u8, port));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        c.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
