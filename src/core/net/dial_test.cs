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
using netip = net.netip_package;
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
using net;
using net.@internal;
using socktest = net.@internal.socktest_package;
using static go.net_package;

partial class net_internal_test_package {

partial struct prohibitionaryDialArgTestsᴛ1 /*dyn*/ {
    internal @string network;
    internal @string address;
}
internal static slice<prohibitionaryDialArgTestsᴛ1> prohibitionaryDialArgTests = slice<prohibitionaryDialArgTestsᴛ1>([
    new("tcp6", "127.0.0.1"),
    new("tcp6", "::ffff:127.0.0.1")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mappingIpv4AddressInsideˢ = (@string)"mapping ipv4 address inside ipv6 address not supported"u8;

public static void TestProhibitionaryDialArg(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("not supported on %s", Δruntime.GOOS);
            break;
        }}
        if (!supportsIPv4map()) {
            t.Skip(mappingIpv4AddressInsideˢ);
        }

        var (ln, err) = Listen(tcpˢ, "[::]:0");
        if (err != nil) {
            t.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        (_, var port, err) = SplitHostPort(ln.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }

        foreach (var (i, tt) in prohibitionaryDialArgTests) {
            var (c, errΔ1) = Dial(tt.network, JoinHostPort(tt.address, port));
            if (errΔ1 == nil) {
                c.Close();
                t.Errorf("#%d: %v", i, errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDialLocal(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(t), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (_, port, err) = SplitHostPort(ln.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        (var c, err) = Dial(tcpˢ, JoinHostPort(""u8, port));
        if (err != nil) {
            t.Fatal(err);
        }
        c.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bothIPv4AndIPv6Areˢ = (@string)"both IPv4 and IPv6 are required"u8;

public static partial void TestDialerDualStackFDLeak(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("%s does not have full support of socktest", Δruntime.GOOS);
            break;
        }
        case {} when exprᴛ1 == "windows"u8: {
            t.Skipf("not implemented a way to cancel dial racers in TCP SYN-SENT state on %s", Δruntime.GOOS);
            break;
        }
        case {} when exprᴛ1 == "openbsd"u8: {
            testenv.SkipFlaky(new net_test_package.testing_TжTB(t), 15157);
            break;
        }}
        if (!supportsIPv4() || !supportsIPv6()) {
            t.Skip(bothIPv4AndIPv6Areˢ);
        }

        var before = Ꮡsw.Sockets();
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        var handler = (ж<dualStackServer> dssΔ1, global::go.net_package.Listener ln) => {
            for (;;) {
                var (c, errΔ1) = ln.Accept();
                if (errΔ1 != nil) {
                    return;
                }
                c.Close();
            }
        };
        var (dss, err) = newDualStackServer();
        if (err != nil) {
            t.Fatal(err);
        }
        {
            var errΔ2 = dss.buildup(handler); if (errΔ2 != nil) {
                dss.teardown();
                t.Fatal(errΔ2);
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
                    var (c, errΔ3) = dʗ1.Dial(tcpˢ, JoinHostPort(localhostˢ, dssʗ1.port));
                    if (errΔ3 != nil) {
                        t.Error(errΔ3);
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
            t.Errorf("got %d; want %d", len(after), len(before));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Define a pair of blackholed (IPv4, IPv6) addresses, for which dialTCP is
// expected to hang until the timeout elapses. These addresses are reserved
// for benchmarking by RFC 6890.
internal static ref readonly @string slowDst4 => ref slowDst4ᵇ;
private static readonly @string slowDst4ᵇ = "198.18.0.254"u8;
internal static ref readonly @string slowDst6 => ref slowDst6ᵇ;
private static readonly @string slowDst6ᵇ = "2001:2::254"u8;

// In some environments, the slow IPs may be explicitly unreachable, and fail
// more quickly than expected. This test hook prevents dialTCP from returning
// before the deadline.
internal static (ж<global::go.net_package.TCPConn>, error) slowDialTCP(context.Context ctx, @string network, ж<global::go.net_package.TCPAddr> laddr, ж<global::go.net_package.TCPAddr> Ꮡraddr) {
    ref var raddr = ref Ꮡraddr.DerefOrNull();

    var sd = Ꮡ(new sysDialer(network: network, address: Ꮡraddr.String()));
    var (c, err) = sd.doDialTCP(ctx, laddr, Ꮡraddr);
    if (ParseIP(slowDst4).Equal(raddr.IP) || ParseIP(slowDst6).Equal(raddr.IP)) {
        // Wait for the deadline, or indefinitely if none exists.
        ᐸꟷ(ctx.Done());
    }
    return (c, err);
}

internal static time.Duration /*dialLatency*/ dialClosedPort(ж<testing.T> t) {
    // On most platforms, dialing a closed port should be nearly instantaneous —
    // less than a few hundred milliseconds. However, on some platforms it may be
    // much slower: on Windows and OpenBSD, it has been observed to take up to a
    // few seconds.

    var (l, err) = Listen(tcpˢ, "127.0.0.1:0");
    if (err != nil) {
        t.Fatalf("dialClosedPort: Listen failed: %v", err);
    }
    @string addr = l.Addr().String();
    l.Close();

    var startTime = time.Now();
    (var c, err) = Dial(tcpˢ, addr);
    if (err == nil) {
        c.Close();
    }
    var elapsed = time.Since(startTime);
    t.Logf("dialClosedPort: measured delay %v", elapsed);
    return elapsed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tcp6ˢ => ref tcp6ˢᵇ;
private static readonly @string tcp6ˢᵇ = "tcp6"u8;
internal static ref readonly @string tcp4ˢ => ref tcp4ˢᵇ;
private static readonly @string tcp4ˢᵇ = "tcp4"u8;
internal static ref readonly @string unreachableˢ => ref unreachableˢᵇ;
private static readonly @string unreachableˢᵇ = "unreachable"u8;

internal partial struct TestDialParallel_type /*dyn*/ {
    internal slice<@string> primaries;
    internal slice<@string> fallbacks;
    internal @string teardownNetwork;
    internal bool expectOk;
    internal time.Duration expectElapsed;
}

public static void TestDialParallel(ж<testing.T> t) {
    time.Duration instant = 0;
    time.Duration fallbackDelay = /* 200 * time.Millisecond */ 200000000;

    slice<@string> nCopies(@string s, nint n) {
        var @out = new slice<@string>(n);
        for (nint i = 0; i < n; i++) {
            @out[i] = s;
        }
        return @out;
    }

    slice<TestDialParallel_type> testCases = slice<TestDialParallel_type>([
        // These should just work on the first try.
        new(slice<@string>(["127.0.0.1"]), slice<@string>([]), "", true, instant),
        new(slice<@string>(["::1"]), slice<@string>([]), "", true, instant),
        new(slice<@string>(["127.0.0.1", "::1"]), slice<@string>([slowDst6]), "tcp6", true, instant),
        new(slice<@string>(["::1", "127.0.0.1"]), slice<@string>([slowDst4]), "tcp4", true, instant),
        // Primary is slow; fallback should kick in.
        new(slice<@string>([slowDst4]), slice<@string>(["::1"]), "", true, fallbackDelay),
        // Skip a "connection refused" in the primary thread.
        new(slice<@string>(["127.0.0.1", "::1"]), slice<@string>([]), "tcp4", true, instant),
        new(slice<@string>(["::1", "127.0.0.1"]), slice<@string>([]), "tcp6", true, instant),
        // Skip a "connection refused" in the fallback thread.
        new(slice<@string>([slowDst4, slowDst6]), slice<@string>(["::1", "127.0.0.1"]), "tcp6", true, fallbackDelay),
        // Primary refused, fallback without delay.
        new(slice<@string>(["127.0.0.1"]), slice<@string>(["::1"]), "tcp4", true, instant),
        new(slice<@string>(["::1"]), slice<@string>(["127.0.0.1"]), "tcp6", true, instant),
        // Everything is refused.
        new(slice<@string>(["127.0.0.1"]), slice<@string>([]), "tcp4", false, instant),
        // Nothing to do; fail instantly.
        new(slice<@string>([]), slice<@string>([]), "", false, instant),
        // Connecting to tons of addresses should not trip the deadline.
        new(nCopies("::1", 1000), slice<@string>([]), "", true, instant)
    ]);

    // Convert a list of IP strings into TCPAddrs.
    global::go.net_package.addrList makeAddrs(slice<@string> ips, @string port) {
        var outᴷ = new stackAppend<global::go.net_package.ΔAddr>(moves: true);
        global::go.net_package.addrList @out = default;
        foreach (var (_, ip) in ips) {
            var (addr, err) = ResolveTCPAddr(tcpˢ, JoinHostPort(ip, port));
            if (err != nil) {
                t.Fatal(err);
            }
            @out = append(outᴷ, @out, (global::go.net_package.ΔAddr)(new global::go.net_package.TCPAddrжΔAddr(addr)));
        }
        @out = move2heap(outᴷ, @out);
        return @out;
    }

    foreach (var (i, vᴛ1) in testCases) {
        ref var tt = ref heap(new TestDialParallel_type(), out var Ꮡtt);
        tt = vᴛ1;

        var makeAddrsʗ1 = makeAddrs;
        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            var ttʗ2 = ttʗ1;
            var dialTCP = (ж<global::go.net_package.TCPConn>, error) (context.Context ctxΔ1, @string network, ж<global::go.net_package.TCPAddr> laddr, ж<global::go.net_package.TCPAddr> raddr) => {
                @string n = tcp6ˢ;
                if (raddr.IP.To4() != nil) {
                    n = tcp4ˢ;
                }
                if (n == ttʗ2.teardownNetwork) {
                    return (default, errors.New(unreachableˢ));
                }
                {
                    @string r = raddr.IP.String(); if (r == slowDst4 || r == slowDst6) {
                        ᐸꟷ(ctxΔ1.Done());
                        return (default, ctxΔ1.Err());
                    }
                }
                return (Ꮡ(new TCPConn()), default);
            };

            var primaries = makeAddrsʗ1(ttʗ1.primaries, "80");
            var fallbacks = makeAddrsʗ1(ttʗ1.fallbacks, "80");
            ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(
                FallbackDelay: fallbackDelay
            ), out var Ꮡd);
            time.Duration forever = /* 60 * time.Minute */ 3600000000000;
            if (ttʗ1.expectElapsed == instant) {
                d.FallbackDelay = forever;
            }
            var startTime = time.Now();
            var sd = Ꮡ(new sysDialer(
                Dialer: d,
                network: "tcp",
                address: "?",
                testHookDialTCP: dialTCP
            ));
            var (c, err) = sd.dialParallel(context.Background(), primaries, fallbacks);
            var elapsed = time.Since(startTime);

            if (c != nil) {
                c.Close();
            }

            if (ttʗ1.expectOk && err != nil) {
                tΔ1.Errorf("#%d: got %v; want nil", i, err);
            } else if (!ttʗ1.expectOk && err == nil) {
                tΔ1.Errorf("#%d: got nil; want non-nil", i);
            }

            if (elapsed < ttʗ1.expectElapsed || elapsed >= forever) {
                tΔ1.Errorf("#%d: got %v; want >= %v, < forever", i, elapsed, ttʗ1.expectElapsed);
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
            if (c != nil) {
                c.Close();
            }
            Ꮡwg.Wait();
        });
    }
}

internal static (slice<global::go.net_package.IPAddr>, error) lookupSlowFast(context.Context ctx, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) {
    var exprᴛ1 = host;
    switch (ᐧ) {
    case {} when exprᴛ1 == "slow6loopback4"u8: {
        // Returns a slow IPv6 address, and a local IPv4 address.
        return (slice<global::go.net_package.IPAddr>([
            new(IP: ParseIP(slowDst6)),
            new(IP: ParseIP("127.0.0.1"))
        ]), default);
    }
    default: {
        return fn(ctx, network, host);
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string slow6loopback4ˢ => ref slow6loopback4ˢᵇ;
private static readonly @string slow6loopback4ˢᵇ = "slow6loopback4"u8;

internal partial struct TestDialerFallbackDelay_type /*dyn*/ {
    internal bool dualstack;
    internal time.Duration delay;
    internal time.Duration expectElapsed;
}

public static void TestDialerFallbackDelay(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));

        if (!supportsIPv4() || !supportsIPv6()) {
            t.Skip(bothIPv4AndIPv6Areˢ);
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

        slice<TestDialerFallbackDelay_type> testCases = slice<TestDialerFallbackDelay_type>([
            // Use a very brief delay, which should fallback immediately.
            new(true, 1 * time.ΔNanosecond, 0),
            // Use a 200ms explicit timeout.
            new(true, 200 * time.Millisecond, 200 * time.Millisecond),
            // The default is 300ms.
            new(true, 0, 300 * time.Millisecond)
        ]);

        var handler = (ж<dualStackServer> dssΔ1, global::go.net_package.Listener ln) => {
            for (;;) {
                var (c, errΔ1) = ln.Accept();
                if (errΔ1 != nil) {
                    return;
                }
                c.Close();
            }
        };
        var (dss, err) = newDualStackServer();
        if (err != nil) {
            t.Fatal(err);
        }
        var dssʗ1 = dss;
        defer(() => dssʗ1.teardown(), ref ᒐ);
        {
            var errΔ2 = dss.buildup(handler); if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
        }

        foreach (var (i, tt) in testCases) {
            var d = Ꮡ(new Dialer(DualStack: tt.dualstack, FallbackDelay: tt.delay));

            var startTime = time.Now();
            var (c, errΔ3) = d.Dial(tcpˢ, JoinHostPort(slow6loopback4ˢ, dss.port));
            var elapsed = time.Since(startTime);
            if (errΔ3 == nil) {
                c.Close();
            } else if (tt.dualstack) {
                t.Error(errΔ3);
            }
            var expectMin = tt.expectElapsed - 1 * time.Millisecond;
            var expectMax = tt.expectElapsed + 95 * time.Millisecond;
            if (elapsed < expectMin) {
                t.Errorf("#%d: got %v; want >= %v", i, elapsed, expectMin);
            }
            if (elapsed > expectMax) {
                t.Errorf("#%d: got %v; want <= %v", i, elapsed, expectMax);
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
            var (td, ok) = t.Deadline(); if (ok) {
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
            if (errΔ1 != nil) {
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
                    Ꮡt.Errorf("got %v; want %v", errΔ2, Δio.EOF);
                }
            }
            cΔ1.Close();
            Ꮡclosed.Done();
        };
        var (dss, err) = newDualStackServer();
        if (err != nil) {
            Ꮡt.Fatal(err);
        }
        var dssʗ1 = dss;
        defer(() => dssʗ1.teardown(), ref ᒐ);
        {
            var errΔ3 = dss.buildup(handler); if (errΔ3 != nil) {
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

        ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(
            FallbackDelay: fallbackDelay
        ), out var Ꮡd);
        var sd = Ꮡ(new sysDialer(
            Dialer: d,
            network: "tcp",
            address: "?"
        ));

        var dssʗ2 = dss;
        global::go.net_package.addrList makeAddr(@string ip) {
            var (addr, errΔ4) = ResolveTCPAddr(tcpˢ, JoinHostPort(ip, dssʗ2.port));
            if (errΔ4 != nil) {
                Ꮡt.Fatal(errΔ4);
            }
            return new addrList(new global::go.net_package.ΔAddr[]{new global::go.net_package.TCPAddrжΔAddr(addr)}.slice());
        }

        // dialParallel returns one connection (and closes the other.)
        (var c, err) = sd.dialParallel(context.Background(), makeAddr("127.0.0.1"), makeAddr("::1"));
        if (err != nil) {
            Ꮡt.Fatal(err);
        }
        c.Close();

        // The server should've seen both connections.
        Ꮡclosed.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestDialerPartialDeadline_type /*dyn*/ {
    internal time.Time now;
    internal time.Time deadline;
    internal nint addrs;
    internal time.Time expectDeadline;
    internal error expectErr;
}

public static void TestDialerPartialDeadline(ж<testing.T> t) {
    var now = time.Date(2000, time.January, 1, 0, 0, 0, 0, time.ΔUTC);
    slice<TestDialerPartialDeadline_type> testCases = slice<TestDialerPartialDeadline_type>([
        // Regular division.
        new(now, now.Add((time.Duration)(12000000000L)), 1, now.Add((time.Duration)(12000000000L)), default),
        new(now, now.Add((time.Duration)(12000000000L)), 2, now.Add((time.Duration)(6000000000L)), default),
        new(now, now.Add((time.Duration)(12000000000L)), 3, now.Add((time.Duration)(4000000000L)), default),
        // Bump against the 2-second sane minimum.
        new(now, now.Add((time.Duration)(12000000000L)), 999, now.Add(2 * time.ΔSecond), default),
        // Total available is now below the sane minimum.
        new(now, now.Add(1900 * time.Millisecond), 999, now.Add(1900 * time.Millisecond), default),
        // Null deadline.
        new(now, noDeadline, 1, noDeadline, default),
        // Step the clock forward and cross the deadline.
        new(now.Add(-1 * time.Millisecond), now, 1, now, default),
        new(now.Add(0 * time.Millisecond), now, 1, noDeadline, errTimeout),
        new(now.Add(1 * time.Millisecond), now, 1, noDeadline, errTimeout)
    ]);
    foreach (var (i, tt) in testCases) {
        var (deadline, err) = partialDeadline(tt.now, tt.deadline, tt.addrs);
        if (!AreEqual(err, tt.expectErr)) {
            t.Errorf("#%d: got %v; want %v", i, err, tt.expectErr);
        }
        if (!deadline.Equal(tt.expectDeadline)) {
            t.Errorf("#%d: got %v; want %v", i, deadline, tt.expectDeadline);
        }
    }
}

// isEADDRINUSE reports whether err is syscall.EADDRINUSE.
internal static Func<error, bool> isEADDRINUSE = (error err) => false;

internal partial struct TestDialerLocalAddr_test /*dyn*/ {
    internal @string network, raddr;
    internal global::go.net_package.ΔAddr laddr;
    /*embed*/ internal error error;
}

public static void TestDialerLocalAddr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!supportsIPv4() || !supportsIPv6()) {
            t.Skip(bothIPv4AndIPv6Areˢ);
        }
        slice<TestDialerLocalAddr_test> tests = slice<TestDialerLocalAddr_test>([
            new("tcp4", "127.0.0.1", default, default),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr())), default),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0")))), default),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0").To4()))), default),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::")))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To4()))), default),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To16()))), default),
            new("tcp4", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), errNoSuitableAddress),
            new("tcp4", "127.0.0.1", new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),
            new("tcp4", "127.0.0.1", new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),

            new("tcp6", "::1", default, default),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr())), default),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0")))), default),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0").To4()))), default),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::")))), default),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To4()))), errNoSuitableAddress),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To16()))), errNoSuitableAddress),
            new("tcp6", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), default),
            new("tcp6", "::1", new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),
            new("tcp6", "::1", new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),

            new("tcp", "127.0.0.1", default, default),
            new("tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr())), default),
            new("tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0")))), default),
            new("tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0").To4()))), default),
            new("tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To4()))), default),
            new("tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To16()))), default),
            new("tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), errNoSuitableAddress),
            new("tcp", "127.0.0.1", new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),
            new("tcp", "127.0.0.1", new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),

            new("tcp", "::1", default, default),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr())), default),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0")))), default),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("0.0.0.0").To4()))), default),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::")))), default),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To4()))), errNoSuitableAddress),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1").To16()))), errNoSuitableAddress),
            new("tcp", "::1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: IPv6loopback))), default),
            new("tcp", "::1", new global::go.net_package.UDPAddrжΔAddr(Ꮡ(new UDPAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))),
            new("tcp", "::1", new global::go.net_package.UnixAddrжΔAddr(Ꮡ(new UnixAddr())), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error"))))
        ]);

        nint issue34264Index = -1;
        if (supportsIPv4map()) {
            issue34264Index = len(tests);
            tests = append(tests, new TestDialerLocalAddr_test(
                "tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::")))), default
            ));
        } else {
            tests = append(tests, new TestDialerLocalAddr_test(
                "tcp", "127.0.0.1", new global::go.net_package.TCPAddrжΔAddr(Ꮡ(new TCPAddr(IP: ParseIP("::")))), new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: "some error")))
            ));
        }

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        var handler = (ж<localServer> ls, global::go.net_package.Listener ln) => {
            for (;;) {
                var (c, err) = ln.Accept();
                if (err != nil) {
                    return;
                }
                c.Close();
            }
        };
        ref var lss = ref heap(new array<ж<localServer>>(2), out var Ꮡlss);
        foreach (var (i, network) in slice<@string>(["tcp4", "tcp6"])) {
            lss[i] = newLocalServer(new net_test_package.testing_TжTB(t), network);
            var lssʗ1 = lss;
            defer(ᴛ0 => ᴛ0.teardown(), lssʗ1[i], ref ᒐ);
            {
                var err = lss[i].buildup(handler); if (err != nil) {
                    t.Fatal(err);
                }
            }
        }

        foreach (var (i, tt) in tests) {
            var d = Ꮡ(new Dialer(LocalAddr: tt.laddr));
            @string addr = default;
            var ip = ParseIP(tt.raddr);
            if (ip.To4() != nil) {
                addr = lss[0].Listener.Addr().String();
            }
            if (ip.To16() != nil && ip.To4() == nil) {
                addr = lss[1].Listener.Addr().String();
            }
            var (c, err) = d.Dial(tt.network, addr);
            if (err == nil && tt.error != nil || err != nil && tt.error == nil) {
                if (i == issue34264Index && Δruntime.GOOS == "freebsd"u8 && isEADDRINUSE(err)) {
                    // https://golang.org/issue/34264: FreeBSD through at least version 12.2
                    // has been observed to fail with EADDRINUSE when dialing from an IPv6
                    // local address to an IPv4 remote address.
                    t.Logf("%s %v->%s: got %v; want %v", tt.network, tt.laddr, tt.raddr, err, tt.error);
                    t.Logf("(spurious EADDRINUSE ignored on freebsd: see https://golang.org/issue/34264)");
                } else {
                    t.Errorf("%s %v->%s: got %v; want %v", tt.network, tt.laddr, tt.raddr, err, tt.error);
                }
            }
            if (err != nil) {
                {
                    var perr = parseDialError(err); if (perr != nil) {
                        t.Error(perr);
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

public static void TestDialerDualStack(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.SkipFlaky(new net_test_package.testing_TжTB(t), 13324);

        if (!supportsIPv4() || !supportsIPv6()) {
            t.Skip(bothIPv4AndIPv6Areˢ);
        }

        var closedPortDelay = dialClosedPort(t);

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        var handler = (ж<dualStackServer> dss, global::go.net_package.Listener ln) => {
            for (;;) {
                var (c, err) = ln.Accept();
                if (err != nil) {
                    return;
                }
                c.Close();
            }
        };

        ref var timeout = ref heap(new time.Duration(), out var Ꮡtimeout);

        timeout = 150 * time.Millisecond + closedPortDelay;
        foreach (var (_, vᴛ1) in slice<bool>([false, true])) {
            ref var dualstack = ref heap(new bool(), out var Ꮡdualstack);
            dualstack = vᴛ1;

            var (dss, err) = newDualStackServer();
            if (err != nil) {
                t.Fatal(err);
            }
            var dssʗ1 = dss;
            defer(() => dssʗ1.teardown(), ref ᒐ);
            {
                var errΔ1 = dss.buildup(handler); if (errΔ1 != nil) {
                    t.Fatal(errΔ1);
                }
            }

            var d = Ꮡ(new Dialer(DualStack: dualstack, Timeout: timeout));
            foreach (var _ᴛ1 in range(dss.lns)) {
                var (c, errΔ2) = d.Dial(tcpˢ, JoinHostPort(localhostˢ, dss.port));
                if (errΔ2 != nil) {
                    t.Error(errΔ2);
                    continue;
                }
                {
                    var addr = c.LocalAddr()._<ж<global::go.net_package.TCPAddr>>();
                    switch (ᐧ) {
                    case {} when addr.IP.To4() != nil: {
                        dss.teardownNetwork(tcp4ˢ);
                        break;
                    }
                    case {} when addr.IP.To16() != nil && addr.IP.To4() == nil: {
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

internal partial struct TestDialerKeepAlive_tests /*dyn*/ {
    internal time.Duration ka;
    internal time.Duration expected;
}

public static void TestDialerKeepAlive(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Cleanup(() => {
            testHookSetKeepAlive = (global::go.net_package.KeepAliveConfig _) => {
            };
        });

        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener lnΔ1) => {
            for (;;) {
                var (c, err) = lnΔ1.Accept();
                if (err != nil) {
                    return;
                }
                c.Close();
            }
        };
        var ln = newLocalListener(new net_test_package.testing_TжTB(t), tcpˢ, Ꮡ(new ListenConfig(
            KeepAlive: -1 // prevent calling hook from accepting
        )));
        var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var err = ls.buildup(handler); if (err != nil) {
                t.Fatal(err);
            }
        }

        var tests = slice<TestDialerKeepAlive_tests>([
            new(-1, -1),
            new(0, 0),
            new((time.Duration)(5000000000L), (time.Duration)(5000000000L)),
            new((time.Duration)(30000000000L), (time.Duration)(30000000000L))
        ]);

        time.Duration got = -1;
        testHookSetKeepAlive = (global::go.net_package.KeepAliveConfig cfg) => {
            got = cfg.Idle;
        };

        foreach (var (_, test) in tests) {
            got = -1;
            ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(KeepAlive: test.ka), out var Ꮡd);
            var (c, err) = Ꮡd.Dial(tcpˢ, ls.Listener.Addr().String());
            if (err != nil) {
                t.Fatal(err);
            }
            c.Close();
            if (got != test.expected) {
                t.Errorf("Dialer.KeepAlive = %v: SetKeepAlive set to %v, want %v", d.KeepAlive, got, test.expected);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timeoutWaitingForDialToˢ = (@string)"timeout waiting for dial to fail"u8;
internal static readonly object unexpectedSuccessfulˢ = (@string)"unexpected successful connection"u8;

public static partial void TestDialCancel(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        mustHaveExternalNetwork(t);

        @string blackholeIPPort = JoinHostPort(slowDst4, "1234");
        if (!supportsIPv4()) {
            blackholeIPPort = JoinHostPort(slowDst6, "1234");
        }

        var ticker = time.NewTicker(10 * time.Millisecond);
        var tickerʗ1 = ticker;
        defer(tickerʗ1.Stop, ref ᒐ);

        const nint cancelTick = 5; // the timer tick we cancel the dial at
        const nint timeoutTick = 100;

        ref var d = ref heap(new global::go.net_package.Dialer(), out var Ꮡd);
        var cancel = new channel<EmptyStruct>(0);
        d.Cancel = cancel.AsRecvOnly();
        var errc = new channel<error>(1);
        var connc = new channel<global::go.net_package.Conn>(1);
        var conncʗ1 = connc;
        var errcʗ1 = errc;
        goǃ(() => {
            {
                var (c, err) = Ꮡd.Dial(tcpˢ, blackholeIPPort); if (err != nil) {
                    errcʗ1.ᐸꟷ(err);
                } else {
                    conncʗ1.ᐸꟷ(c);
                }
            }
        });
        nint ticks = 0;
        for (;;) {
            var selᴛ1 = ticker.C;
            var selᴛ2 = connc;
            var selᴛ3 = errc;
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                ticks++;
                if (ticks == cancelTick) {
                    builtin.close(cancel);
                }
                if (ticks == timeoutTick) {
                    t.Fatal(timeoutWaitingForDialToˢ);
                }
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out var c): {
                c.Close();
                t.Fatal(unexpectedSuccessfulˢ);
                break;
            }
            case 2 when selᴛ3.ꟷᐳ(out var err): {
                {
                    var perr = parseDialError(err); if (perr != nil) {
                        t.Error(perr);
                    }
                }
                if (ticks < cancelTick) {
                    // Using strings.Contains is ugly but
                    // may work on plan9 and windows.
                    var ignorable = slice<@string>([
                        "connection refused",
                        "unreachable",
                        "no route to host",
                        "invalid argument"
                    ]);
                    @string e = err.Error();
                    foreach (var (_, ignore) in ignorable) {
                        if (strings.Contains(e, ignore)) {
                            t.Skipf("connection to %v failed fast with %v", blackholeIPPort, err);
                        }
                    }

                    t.Fatalf("dial error after %d ticks (%d before cancel sent): %v",
                        ticks, cancelTick - ticks, err);
                }
                {
                    var (oe, ok) = err._<ж<global::go.net_package.OpError>>(ᐧ); if (!ok || !AreEqual(oe.Err, errCanceled)) {
                        t.Fatalf("dial error = %v (%T); want OpError with Err == errCanceled", err, err);
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
internal static ref readonly @string messageᶜ => ref messageᶜᵇ;
private static readonly @string messageᶜᵇ = "echo!\n"u8;

public static partial void TestCancelAfterDial(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(avoidingTimeSleepˢ);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(t), tcpˢ);

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
            for (;;) {
                var (c, err) = lnʗ2.Accept();
                if (err != nil) {
                    break;
                }
                var rb = bufio.NewReader(new net_test_package.net_ConnᴠReader(c));
                (var line, err) = rb.ReadString((byte)'\n');
                if (err != nil) {
                    t.Error(err);
                    c.Close();
                    continue;
                }
                {
                    var (_, errΔ1) = c.Write(slice<byte>(line)); if (errΔ1 != nil) {
                        t.Error(errΔ1);
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

                if (err != nil) {
                    t.Fatal(err);
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);

                // Send some data to confirm that the connection is still alive.
                @string message = messageᶜ;
                {
                    var (_, errΔ1) = c.Write(slice<byte>(message)); if (errΔ1 != nil) {
                        t.Fatal(errΔ1);
                    }
                }

                // The server should echo the line, and close the connection.
                var rb = bufio.NewReader(new net_test_package.net_ConnᴠReader(c));
                (var line, err) = rb.ReadString((byte)'\n');
                if (err != nil) {
                    t.Fatal(err);
                }
                if (line != message) {
                    t.Errorf("got %q; want %q", line, message);
                }
                {
                    var (_, errΔ2) = rb.ReadByte(); if (!AreEqual(errΔ2, Δio.EOF)) {
                        t.Errorf("got %v; want %v", errΔ2, Δio.EOF);
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

public static void TestDialClosedPortFailFast(ж<testing.T> t) {
    if (Δruntime.GOOS != "windows"u8) {
        // Reported by go.dev/issues/23366.
        t.Skip(skippingWindowsOnlyTestˢ);
    }
    foreach (var (_, network) in slice<@string>(["tcp", "tcp4", "tcp6"])) {
        t.Run(network, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(network)) {
                    tΔ1.Skipf("skipping: can't listen on %s", network);
                }
                // Reserve a local port till the end of the
                // test by opening a listener and connecting to
                // it using Dial.
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network);
                @string addr = ln.Addr().String();
                var (conn1, err) = Dial(network, addr);
                if (err != nil) {
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
                for (;;) {
                    var startTime = time.Now();
                    var (conn2, errΔ1) = Dial(network, addr);
                    if (errΔ1 == nil) {
                        conn2.Close();
                        tΔ1.Fatal(errorExpectedˢ);
                    }
                    var elapsed = time.Since(startTime);
                    if (elapsed < maxElapsed) {
                        break;
                    }
                    tΔ1.Logf("got %v; want < %v", elapsed, maxElapsed);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string localhost0ˢ => ref localhost0ˢᵇ;
private static readonly @string localhost0ˢᵇ = "localhost:0"u8;

// Issue 18806: it should always be possible to net.Dial a
// net.Listener().Addr().String when the listen address was ":n", even
// if the machine has halfway configured IPv6 such that it can bind on
// "::" not connect back to that same address.
public static void TestDialListenerAddr(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!testableNetwork(tcp4ˢ)) {
            t.Skipf("skipping: can't listen on tcp4");
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
        if (err != nil) {
            t.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);

        t.Logf("listening on %q", ln.Addr());
        (_, var port, err) = SplitHostPort(ln.Addr().String());
        if (err != nil) {
            t.Fatal(err);
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
        if (err != nil) {
            t.Fatalf(@"Dial(""tcp4"", %q): %v", dialAddr, err);
        }
        c.Close();
        t.Logf(@"Dial(""tcp4"", %q) succeeded", dialAddr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string streamDialˢ => ref streamDialˢᵇ;
private static readonly @string streamDialˢᵇ = "StreamDial"u8;
internal static ref readonly @string packetDialˢ => ref packetDialˢᵇ;
private static readonly @string packetDialˢᵇ = "PacketDial"u8;

public static void TestDialerControl(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skipf("skipping: fake net does not support Dialer.Control");
        break;
    }}

    t.Run(streamDialˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            foreach (var (_, network) in slice<@string>(["tcp", "tcp4", "tcp6", "unix", "unixpacket"])) {
                if (!testableNetwork(network)) {
                    continue;
                }
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network);
                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Control: controlOnConnSetup), out var Ꮡd);
                var (c, err) = Ꮡd.Dial(network, ln.Addr().String());
                if (err != nil) {
                    tΔ1.Error(err);
                    continue;
                }
                c.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    t.Run(packetDialˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            foreach (var (_, network) in slice<@string>(["udp", "udp4", "udp6", "unixgram"])) {
                if (!testableNetwork(network)) {
                    continue;
                }
                var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(tΔ2), network);
                if (network == "unixgram"u8) {
                    defer(Δos.Remove, c1.LocalAddr().String(), ref ᒐ);
                }
                var c1ʗ1 = c1;
                defer(() => c1ʗ1.Close(), ref ᒐ);
                ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Control: controlOnConnSetup), out var Ꮡd);
                var (c2, err) = Ꮡd.Dial(network, c1.LocalAddr().String());
                if (err != nil) {
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

public static void TestDialerControlContext(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("%s does not have full support of socktest", Δruntime.GOOS);
        break;
    }
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skipf("skipping: fake net does not support Dialer.ControlContext");
        break;
    }}
    t.Run(streamDialˢ, (ж<testing.T> tΔ1) => {
        foreach (var (i, network) in slice<@string>(["tcp", "tcp4", "tcp6", "unix", "unixpacket"])) {
            tΔ1.Run(network, (ж<testing.T> tΔ2) => {
                GoFrame ᒐ = default;
                try {
                    if (!testableNetwork(network)) {
                        tΔ2.Skipf("skipping: %s not available", network);
                    }

                    var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ2), network);
                    var lnʗ1 = ln;
                    defer(() => lnʗ1.Close(), ref ᒐ);
                    nint id = default;
                    ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(ControlContext: (context.Context ctx, @string networkΔ1, @string address, syscall.RawConn cΔ1) => {
                        id = ctx.Value("id")._<nint>();
                        return controlOnConnSetup(networkΔ1, address, cΔ1);
                    }
                    ), out var Ꮡd);
                    var (c, err) = Ꮡd.DialContext(context.WithValue(context.Background(), "id", i + 1), network, ln.Addr().String());
                    if (err != nil) {
                        tΔ2.Fatal(err);
                    }
                    if (id != i + 1) {
                        tΔ2.Errorf("got id %d, want %d", id, i + 1);
                    }
                    c.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    });
}

public static void TestDialContext(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skipf("skipping: fake net does not support Dialer.ControlContext");
        break;
    }}

    t.Run(streamDialˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            error err = default;
            foreach (var (i, network) in slice<@string>(["tcp", "tcp4", "tcp6", "unix", "unixpacket"])) {
                if (!testableNetwork(network)) {
                    continue;
                }
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), network);
                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                nint id = default;
                ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(ControlContext: (context.Context ctx, @string networkΔ1, @string address, syscall.RawConn cΔ1) => {
                    id = ctx.Value("id")._<nint>();
                    return controlOnConnSetup(networkΔ1, address, cΔ1);
                }
                ), out var Ꮡd);
                global::go.net_package.Conn c = default;
                var exprᴛ2 = network;
                switch (ᐧ) {
                case {} when exprᴛ2 == "tcp"u8 || exprᴛ2 == "tcp4"u8 || exprᴛ2 == "tcp6"u8: {
                    var (raddr, errΔ2) = netip.ParseAddrPort(ln.Addr().String());
                    if (errΔ2 != nil) {
                        tΔ1.Error(errΔ2);
                        continue;
                    }
                    var (ᴛ1, ᴛ2) = Ꮡd.DialTCP(context.WithValue(context.Background(), "id", i + 1), network, (((ж<global::go.net_package.TCPAddr>)nil)).AddrPort(), raddr);
                    (c, errΔ2) = (new global::go.net_package.TCPConnжConn(ᴛ1), ᴛ2);
                    break;
                }
                case {} when exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8: {
                    var (raddr, errΔ3) = ResolveUnixAddr(network, ln.Addr().String());
                    if (errΔ3 != nil) {
                        tΔ1.Error(errΔ3);
                        continue;
                    }
                    var (ᴛ1, ᴛ2) = Ꮡd.DialUnix(context.WithValue(context.Background(), "id", i + 1), network, nil, raddr);
                    (c, errΔ3) = (new global::go.net_package.UnixConnжConn(ᴛ1), ᴛ2);
                    break;
                }}
                if (err != nil) {
                    tΔ1.Error(err);
                    continue;
                }
                if (id != i + 1) {
                    tΔ1.Errorf("%s: got id %d, want %d", network, id, i + 1);
                }
                c.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    t.Run(packetDialˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            error err = default;
            foreach (var (i, network) in slice<@string>(["udp", "udp4", "udp6", "unixgram"])) {
                if (!testableNetwork(network)) {
                    continue;
                }
                var c1 = newLocalPacketListener(new net_test_package.testing_TжTB(tΔ2), network);
                if (network == "unixgram"u8) {
                    defer(Δos.Remove, c1.LocalAddr().String(), ref ᒐ);
                }
                var c1ʗ1 = c1;
                defer(() => c1ʗ1.Close(), ref ᒐ);
                nint id = default;
                ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(ControlContext: (context.Context ctx, @string networkΔ1, @string address, syscall.RawConn c) => {
                    id = ctx.Value("id")._<nint>();
                    return controlOnConnSetup(networkΔ1, address, c);
                }
                ), out var Ꮡd);
                global::go.net_package.Conn c2 = default;
                var exprᴛ3 = network;
                switch (ᐧ) {
                case {} when exprᴛ3 == "udp"u8 || exprᴛ3 == "udp4"u8 || exprᴛ3 == "udp6"u8: {
                    var (raddr, errΔ2) = netip.ParseAddrPort(c1.LocalAddr().String());
                    if (errΔ2 != nil) {
                        tΔ2.Error(errΔ2);
                        continue;
                    }
                    var (ᴛ1, ᴛ2) = Ꮡd.DialUDP(context.WithValue(context.Background(), "id", i + 1), network, (((ж<global::go.net_package.UDPAddr>)nil)).AddrPort(), raddr);
                    (c2, errΔ2) = (new global::go.net_package.UDPConnжConn(ᴛ1), ᴛ2);
                    break;
                }
                case {} when exprᴛ3 == "unixgram"u8: {
                    var (raddr, errΔ3) = ResolveUnixAddr(network, c1.LocalAddr().String());
                    if (errΔ3 != nil) {
                        tΔ2.Error(errΔ3);
                        continue;
                    }
                    var (ᴛ1, ᴛ2) = Ꮡd.DialUnix(context.WithValue(context.Background(), "id", i + 1), network, nil, raddr);
                    (c2, errΔ3) = (new global::go.net_package.UnixConnжConn(ᴛ1), ᴛ2);
                    break;
                }}
                if (err != nil) {
                    tΔ2.Error(err);
                    continue;
                }
                if (id != i + 1) {
                    tΔ2.Errorf("%s: got id %d, want %d", network, id, i + 1);
                }
                c2.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// mustHaveExternalNetwork is like testenv.MustHaveExternalNetwork
// except on non-Linux, non-mobile builders it permits the test to
// run in -short mode.
internal static void mustHaveExternalNetwork(ж<testing.T> t) {
    t.Helper();
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
    testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(t));
}

internal partial struct contextWithNonZeroDeadline {
    /*embed*/ public context_package.Context Context;
}

internal static (time.Time, bool) Deadline(this contextWithNonZeroDeadline _) {
    // Return non-zero time.Time value with false indicating that no deadline is set.
    return (time.Unix(0, 0), false);
}

public static void TestDialWithNonZeroDeadline(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(t), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (_, port, err) = SplitHostPort(ln.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }

        var ctx = new contextWithNonZeroDeadline(Context: context.Background());
        ref var dialer = ref heap(new global::go.net_package.Dialer(), out var Ꮡdialer);
        (var c, err) = Ꮡdialer.DialContext(ctx, tcpˢ, JoinHostPort(""u8, port));
        if (err != nil) {
            t.Fatal(err);
        }
        c.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
