// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using poll = @internal.poll_package;
using Δio = io_package;
using fs = go.io.fs_package;
using socktest = net.@internal.socktest_package;
using Δos = os_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using go.io;
using net.@internal;
using static go.net_package;

partial class net_internal_test_package {

internal static error isValid(this ж<global::go.net_package.OpError> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (e.Op == ""u8) {
        return fmt.Errorf("OpError.Op is empty: %v"u8, Ꮡe.OrTypedNil());
    }
    if (e.Net == ""u8) {
        return fmt.Errorf("OpError.Net is empty: %v"u8, Ꮡe.OrTypedNil());
    }
    foreach (var (_, addr) in slice<global::go.net_package.ΔAddr>([e.Source, e.Addr])) {
        switch (addr.type()) {
        case null: {
            break;
        }
        case ж<global::go.net_package.TCPAddr> addrΔ1: {
            if (addrΔ1 == nil) {
                return fmt.Errorf("OpError.Source or Addr is non-nil interface: %#v, %v"u8, addrΔ1.OrTypedNil(), Ꮡe.OrTypedNil());
            }
            break;
        }
        case ж<global::go.net_package.UDPAddr> addrΔ1: {
            if (addrΔ1 == nil) {
                return fmt.Errorf("OpError.Source or Addr is non-nil interface: %#v, %v"u8, addrΔ1.OrTypedNil(), Ꮡe.OrTypedNil());
            }
            break;
        }
        case ж<global::go.net_package.IPAddr> addrΔ1: {
            if (addrΔ1 == nil) {
                return fmt.Errorf("OpError.Source or Addr is non-nil interface: %#v, %v"u8, addrΔ1.OrTypedNil(), Ꮡe.OrTypedNil());
            }
            break;
        }
        case ж<global::go.net_package.IPNet> addrΔ1: {
            if (addrΔ1 == nil) {
                return fmt.Errorf("OpError.Source or Addr is non-nil interface: %#v, %v"u8, addrΔ1.OrTypedNil(), Ꮡe.OrTypedNil());
            }
            break;
        }
        case ж<global::go.net_package.UnixAddr> addrΔ1: {
            if (addrΔ1 == nil) {
                return fmt.Errorf("OpError.Source or Addr is non-nil interface: %#v, %v"u8, addrΔ1.OrTypedNil(), Ꮡe.OrTypedNil());
            }
            break;
        }
        case ж<global::go.net_package.pipeAddr> addrΔ1: {
            if (addrΔ1 == nil) {
                return fmt.Errorf("OpError.Source or Addr is non-nil interface: %#v, %v"u8, addrΔ1.OrTypedNil(), Ꮡe.OrTypedNil());
            }
            break;
        }
        case fileAddr addrΔ1: {
            if (addrΔ1 == ""u8) {
                return fmt.Errorf("OpError.Source or Addr is empty: %#v, %v"u8, addrΔ1, Ꮡe.OrTypedNil());
            }
            break;
        }
        default: {
            var addrΔ1 = addr;
            return fmt.Errorf("OpError.Source or Addr is unknown type: %T, %v"u8, addrΔ1, Ꮡe.OrTypedNil());
        }}
    }
    if (e.Err == nil) {
        return fmt.Errorf("OpError.Err is empty: %v"u8, Ꮡe.OrTypedNil());
    }
    return default;
}

internal partial interface parseDialError_type /*dyn*/ {
    void isAddrinfoErrno();
}

// parseDialError parses nestedErr and reports whether it is a valid
// error value from Dial, Listen functions.
// It returns nil when nestedErr is valid.
internal static error parseDialError(error nestedErr) {
    if (nestedErr == nil) {
        return default;
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        nestedErr = err.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);

second:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.AddrError> _:
    case ж<global::go.net_package.timeoutError> _:
    case ж<global::go.net_package.DNSError> _:
    case InvalidAddrError _:
    case ж<global::go.net_package.ParseError> _:
    case ж<poll.DeadlineExceededError> _:
    case UnknownNetworkError _: {
        var err = nestedErr;
        return default;
    }
    case {} Δerr when Δerr._<parseDialError_type>(out var err): {
        return default;
    }
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: { // for Plan 9
        nestedErr = err.Err;
        goto third;
        break;
    }}
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, errCanceled) || AreEqual(ᴛ, ErrClosed) || AreEqual(ᴛ, errMissingAddress) || AreEqual(ᴛ, errNoSuitableAddress) || AreEqual(ᴛ, context.DeadlineExceeded) || AreEqual(ᴛ, context.Canceled): {
        return default;
    }}
    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);

third:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

internal static slice<prohibitionaryDialArgTestsᴛ1> dialErrorTests = slice<prohibitionaryDialArgTestsᴛ1>([
    new("foo", ""),
    new("bar", "baz"),
    new("datakit", "mh/astro/r70"),
    new("tcp", ""),
    new("tcp", "127.0.0.1:☺"),
    new("tcp", "no-such-name:80"),
    new("tcp", "mh/astro/r70:http"),

    new("tcp", JoinHostPort("127.0.0.1", "-1")),
    new("tcp", JoinHostPort("127.0.0.1", "123456789")),
    new("udp", JoinHostPort("127.0.0.1", "-1")),
    new("udp", JoinHostPort("127.0.0.1", "123456789")),
    new("ip:icmp", "127.0.0.1"),

    new("unix", "/path/to/somewhere"),
    new("unixgram", "/path/to/somewhere"),
    new("unixpacket", "/path/to/somewhere")
]);

public static void TestDialError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("%s does not have full support of socktest", Δruntime.GOOS);
            break;
        }}

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context ctx, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => (default, new global::go.net_package.DNSErrorжerror(Ꮡ(new DNSError(Err: "dial error test", Name: "name", Server: "server", IsTimeout: true))));
        Ꮡsw.Set(socktest.FilterConnect, (ж<socktest.Status> so) => (default, errOpNotSupported));
        defer(Ꮡsw.Set, socktest.FilterConnect, (socktest.Filter)(default), ref ᒐ);

        ref var d = ref heap<global::go.net_package.Dialer>(new Dialer(Timeout: someTimeout), out var Ꮡd);
        foreach (var (i, vᴛ1) in dialErrorTests) {
            ref var tt = ref heap(new prohibitionaryDialArgTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
                var (c, err) = Ꮡd.Dial(ttʗ1.network, ttʗ1.address);
                if (err == nil) {
                    tΔ1.Errorf("should fail; %s:%s->%s", c.LocalAddr().Network(), c.LocalAddr(), c.RemoteAddr());
                    c.Close();
                    return;
                }
                if (ttʗ1.network == "tcp"u8 || ttʗ1.network == "udp"u8) {
                    var nerr = err;
                    {
                        var (op, ok) = nerr._<ж<global::go.net_package.OpError>>(ᐧ); if (ok) {
                            nerr = op.Err;
                        }
                    }
                    {
                        var (sys, ok) = nerr._<ж<Δos.SyscallError>>(ᐧ); if (ok) {
                            nerr = sys.Err;
                        }
                    }
                    if (AreEqual(nerr, errOpNotSupported)) {
                        tΔ1.Fatalf("should fail without %v; %s:%s->", nerr, ttʗ1.network, ttʗ1.address);
                    }
                }
                if (c != nil) {
                    tΔ1.Errorf("Dial returned non-nil interface %T(%v) with err != nil", c, c);
                }
                {
                    err = parseDialError(err); if (err != nil) {
                        tΔ1.Error(err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestProtocolDialError(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}

    foreach (var (_, network) in slice<@string>(["tcp", "udp", "ip:4294967296", "unix", "unixpacket", "unixgram"])) {
        error err = default;
        var exprᴛ2 = network;
        switch (ᐧ) {
        case {} when exprᴛ2 == "tcp"u8: {
            (_, err) = DialTCP(network, nil, Ꮡ(new TCPAddr(Port: (1 << 16))));
            break;
        }
        case {} when exprᴛ2 == "udp"u8: {
            (_, err) = DialUDP(network, nil, Ꮡ(new UDPAddr(Port: (1 << 16))));
            break;
        }
        case {} when exprᴛ2 == "ip:4294967296"u8: {
            (_, err) = DialIP(network, nil, nil);
            break;
        }
        case {} when exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8 || exprᴛ2 == "unixgram"u8: {
            (_, err) = DialUnix(network, nil, Ꮡ(new UnixAddr(Name: "//")));
            break;
        }}
        if (err == nil) {
            t.Errorf("%s: should fail", network);
            continue;
        }
        {
            var errΔ1 = parseDialError(err); if (errΔ1 != nil) {
                t.Errorf("%s: %v", network, errΔ1);
                continue;
            }
        }
        t.Logf("%s: error as expected: %v", network, err);
    }
}

internal partial struct TestDialAddrError_type /*dyn*/ {
    internal @string network;
    internal @string lit;
    internal ж<global::go.net_package.TCPAddr> addr;
}

public static void TestDialAddrError(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}

    if (!supportsIPv4() || !supportsIPv6()) {
        t.Skip(bothIPv4AndIPv6Areˢ);
    }

    foreach (var (_, vᴛ1) in slice<TestDialAddrError_type>([
        new("tcp4", "::1", nil),
        new("tcp4", "", Ꮡ(new TCPAddr(IP: IPv6loopback))),
        // We don't test the {"tcp6", "byte sequence", nil}
        // case for now because there is no easy way to
        // control name resolution.
        new("tcp6", "", Ꮡ(new TCPAddr(IP: new IP(new byte[]{0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef}.slice()))))
    ])) {
        ref var tt = ref heap(new TestDialAddrError_type(), out var Ꮡtt);
        tt = vᴛ1;

        @string desc = tt.lit;
        if (desc == ""u8) {
            desc = tt.addr.String();
        }
        var ttʗ1 = tt;
        t.Run(fmt.Sprintf("%s/%s"u8, tt.network, desc), (ж<testing.T> tΔ1) => {
            error err = default;
            global::go.net_package.Conn c = default;
            @string op = default;
            if (ttʗ1.lit != ""u8) {
                (c, err) = Dial(ttʗ1.network, JoinHostPort(ttʗ1.lit, "0"));
                op = fmt.Sprintf("Dial(%q, %q)"u8, ttʗ1.network, JoinHostPort(ttʗ1.lit, "0"));
            } else {
                var (ᴛ1, ᴛ2) = DialTCP(ttʗ1.network, nil, ttʗ1.addr);
                (c, err) = (new global::go.net_package.TCPConnжConn(ᴛ1), ᴛ2);
                op = fmt.Sprintf("DialTCP(%q, %q)"u8, ttʗ1.network, ttʗ1.addr.OrTypedNil());
            }
            tΔ1.Logf("%s: %v", op, err);
            if (err == nil) {
                c.Close();
                tΔ1.Fatalf("%s succeeded, want error", op);
            }
            {
                var perr = parseDialError(err); if (perr != nil) {
                    tΔ1.Fatal(perr);
                }
            }
            var operr = err._<ж<global::go.net_package.OpError>>().Err;
            var (aerr, ok) = operr._<ж<global::go.net_package.AddrError>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("OpError.Err is %T, want *AddrError", operr);
            }
            @string want = ttʗ1.lit;
            if (ttʗ1.lit == ""u8) {
                want = ttʗ1.addr.IP.String();
            }
            if (aerr.Addr != want) {
                tΔ1.Errorf("error Addr=%q, want %q", aerr.Addr, want);
            }
        });
    }
}

internal static slice<prohibitionaryDialArgTestsᴛ1> listenErrorTests = slice<prohibitionaryDialArgTestsᴛ1>([
    new("foo", ""),
    new("bar", "baz"),
    new("datakit", "mh/astro/r70"),
    new("tcp", "127.0.0.1:☺"),
    new("tcp", "no-such-name:80"),
    new("tcp", "mh/astro/r70:http"),

    new("tcp", JoinHostPort("127.0.0.1", "-1")),
    new("tcp", JoinHostPort("127.0.0.1", "123456789")),

    new("unix", "/path/to/somewhere"),
    new("unixpacket", "/path/to/somewhere")
]);

public static void TestListenError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("%s does not have full support of socktest", Δruntime.GOOS);
            break;
        }}

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context _, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => (default, new global::go.net_package.DNSErrorжerror(Ꮡ(new DNSError(Err: "listen error test", Name: "name", Server: "server", IsTimeout: true))));
        Ꮡsw.Set(socktest.FilterListen, (ж<socktest.Status> so) => (default, errOpNotSupported));
        defer(Ꮡsw.Set, socktest.FilterListen, (socktest.Filter)(default), ref ᒐ);

        foreach (var (i, vᴛ1) in listenErrorTests) {
            ref var tt = ref heap(new prohibitionaryDialArgTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            t.Run(fmt.Sprintf("%s_%s"u8, tt.network, tt.address), (ж<testing.T> tΔ1) => {
                var (ln, err) = Listen(ttʗ1.network, ttʗ1.address);
                if (err == nil) {
                    tΔ1.Errorf("#%d: should fail; %s:%s->", i, ln.Addr().Network(), ln.Addr());
                    ln.Close();
                    return;
                }
                if (ttʗ1.network == "tcp"u8) {
                    var nerr = err;
                    {
                        var (op, ok) = nerr._<ж<global::go.net_package.OpError>>(ᐧ); if (ok) {
                            nerr = op.Err;
                        }
                    }
                    {
                        var (sys, ok) = nerr._<ж<Δos.SyscallError>>(ᐧ); if (ok) {
                            nerr = sys.Err;
                        }
                    }
                    if (AreEqual(nerr, errOpNotSupported)) {
                        tΔ1.Fatalf("#%d: should fail without %v; %s:%s->", i, nerr, ttʗ1.network, ttʗ1.address);
                    }
                }
                if (ln != nil) {
                    tΔ1.Errorf("Listen returned non-nil interface %T(%v) with err != nil", ln, ln);
                }
                {
                    err = parseDialError(err); if (err != nil) {
                        tΔ1.Errorf("#%d: %v", i, err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<prohibitionaryDialArgTestsᴛ1> listenPacketErrorTests = slice<prohibitionaryDialArgTestsᴛ1>([
    new("foo", ""),
    new("bar", "baz"),
    new("datakit", "mh/astro/r70"),
    new("udp", "127.0.0.1:☺"),
    new("udp", "no-such-name:80"),
    new("udp", "mh/astro/r70:http"),

    new("udp", JoinHostPort("127.0.0.1", "-1")),
    new("udp", JoinHostPort("127.0.0.1", "123456789"))
]);

public static void TestListenPacketError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skipf("%s does not have full support of socktest", Δruntime.GOOS);
            break;
        }}

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context _, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => (default, new global::go.net_package.DNSErrorжerror(Ꮡ(new DNSError(Err: "listen error test", Name: "name", Server: "server", IsTimeout: true))));

        foreach (var (i, vᴛ1) in listenPacketErrorTests) {
            ref var tt = ref heap(new prohibitionaryDialArgTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            t.Run(fmt.Sprintf("%s_%s"u8, tt.network, tt.address), (ж<testing.T> tΔ1) => {
                var (c, err) = ListenPacket(ttʗ1.network, ttʗ1.address);
                if (err == nil) {
                    tΔ1.Errorf("#%d: should fail; %s:%s->", i, c.LocalAddr().Network(), c.LocalAddr());
                    c.Close();
                    return;
                }
                if (c != nil) {
                    tΔ1.Errorf("ListenPacket returned non-nil interface %T(%v) with err != nil", c, c);
                }
                {
                    err = parseDialError(err); if (err != nil) {
                        tΔ1.Errorf("#%d: %v", i, err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestProtocolListenError(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", Δruntime.GOOS);
        break;
    }}

    foreach (var (_, network) in slice<@string>(["tcp", "udp", "ip:4294967296", "unix", "unixpacket", "unixgram"])) {
        error err = default;
        var exprᴛ2 = network;
        switch (ᐧ) {
        case {} when exprᴛ2 == "tcp"u8: {
            (_, err) = ListenTCP(network, Ꮡ(new TCPAddr(Port: (1 << 16))));
            break;
        }
        case {} when exprᴛ2 == "udp"u8: {
            (_, err) = ListenUDP(network, Ꮡ(new UDPAddr(Port: (1 << 16))));
            break;
        }
        case {} when exprᴛ2 == "ip:4294967296"u8: {
            (_, err) = ListenIP(network, nil);
            break;
        }
        case {} when exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8: {
            (_, err) = ListenUnix(network, Ꮡ(new UnixAddr(Name: "//")));
            break;
        }
        case {} when exprᴛ2 == "unixgram"u8: {
            (_, err) = ListenUnixgram(network, Ꮡ(new UnixAddr(Name: "//")));
            break;
        }}
        if (err == nil) {
            t.Errorf("%s: should fail", network);
            continue;
        }
        {
            err = parseDialError(err); if (err != nil) {
                t.Errorf("%s: %v", network, err);
                continue;
            }
        }
    }
}

// parseReadError parses nestedErr and reports whether it is a valid
// error value from Read functions.
// It returns nil when nestedErr is valid.
internal static error parseReadError(error nestedErr) {
    if (nestedErr == nil) {
        return default;
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        nestedErr = err.Err;
        goto second;
        break;
    }}
    if (AreEqual(nestedErr, Δio.EOF)) {
        return default;
    }
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);

second:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }}
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, ErrClosed) || AreEqual(ᴛ, errTimeout) || AreEqual(ᴛ, poll.ErrNotPollable) || AreEqual(ᴛ, Δos.ErrDeadlineExceeded): {
        return default;
    }}
    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);

third:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

// parseWriteError parses nestedErr and reports whether it is a valid
// error value from Write functions.
// It returns nil when nestedErr is valid.
internal static error parseWriteError(error nestedErr) {
    if (nestedErr == nil) {
        return default;
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        nestedErr = err.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);

second:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.AddrError> _:
    case ж<global::go.net_package.timeoutError> _:
    case ж<global::go.net_package.DNSError> _:
    case InvalidAddrError _:
    case ж<global::go.net_package.ParseError> _:
    case ж<poll.DeadlineExceededError> _:
    case UnknownNetworkError _: {
        var err = nestedErr;
        return default;
    }
    case {} Δerr when Δerr._<parseDialError_type>(out var err): {
        return default;
    }
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }}
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, errCanceled) || AreEqual(ᴛ, ErrClosed) || AreEqual(ᴛ, errMissingAddress) || AreEqual(ᴛ, errTimeout) || AreEqual(ᴛ, Δos.ErrDeadlineExceeded) || AreEqual(ᴛ, ErrWriteToConnected) || AreEqual(ᴛ, Δio.ErrUnexpectedEOF): {
        return default;
    }}
    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);

third:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string useOfClosedNetworkˢ => ref useOfClosedNetworkˢᵇ;
private static readonly @string useOfClosedNetworkˢᵇ = "use of closed network connection"u8;

// parseCloseError parses nestedErr and reports whether it is a valid
// error value from Close functions.
// It returns nil when nestedErr is valid.
internal static error parseCloseError(error nestedErr, bool isShutdown) {
    if (nestedErr == nil) {
        return default;
    }

    // Because historically we have not exported the error that we
    // return for an operation on a closed network connection,
    // there are programs that test for the exact error string.
    // Verify that string here so that we don't break those
    // programs unexpectedly. See issues #4373 and #19252.
    @string want = useOfClosedNetworkˢ;
    if (!isShutdown && !strings.Contains(nestedErr.Error(), want)) {
        return fmt.Errorf("error string %q does not contain expected string %q"u8, nestedErr, want);
    }

    if (!isShutdown && !errors.Is(nestedErr, ErrClosed)) {
        return fmt.Errorf("errors.Is(%v, errClosed) returns false, want true"u8, nestedErr);
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        nestedErr = err.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);

second:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: { // for Plan 9
        nestedErr = err.Err;
        goto third;
        break;
    }}
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, ErrClosed): {
        return default;
    }}
    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);

third:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, fs.ErrClosed): { // for Plan 9
        return default;
    }}
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

public static void TestCloseError(ж<testing.T> t) {
    t.Run(tcpˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), tcpˢ);
            var lnʗ1 = ln;
            defer(() => lnʗ1.Close(), ref ᒐ);
            var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);

            for (nint i = 0; i < 3; i++) {
                err = c._<ж<global::go.net_package.TCPConn>>().CloseRead();
                {
                    var perr = parseCloseError(err, true); if (perr != nil) {
                        tΔ1.Errorf("#%d: %v", i, perr);
                    }
                }
            }
            for (nint i = 0; i < 3; i++) {
                err = c._<ж<global::go.net_package.TCPConn>>().CloseWrite();
                {
                    var perr = parseCloseError(err, true); if (perr != nil) {
                        tΔ1.Errorf("#%d: %v", i, perr);
                    }
                }
            }
            for (nint i = 0; i < 3; i++) {
                err = c.Close();
                {
                    var perr = parseCloseError(err, false); if (perr != nil) {
                        tΔ1.Errorf("#%d: %v", i, perr);
                    }
                }
                err = ln.Close();
                {
                    var perr = parseCloseError(err, false); if (perr != nil) {
                        tΔ1.Errorf("#%d: %v", i, perr);
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    t.Run(udpˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            if (!testableNetwork(udpˢ)) {
                tΔ2.Skipf("skipping: udp not available");
            }

            var (pc, err) = ListenPacket(udpˢ, "127.0.0.1:0");
            if (err != nil) {
                tΔ2.Fatal(err);
            }
            var pcʗ1 = pc;
            defer(() => pcʗ1.Close(), ref ᒐ);

            for (nint i = 0; i < 3; i++) {
                err = pc.Close();
                {
                    var perr = parseCloseError(err, false); if (perr != nil) {
                        tΔ2.Errorf("#%d: %v", i, perr);
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// parseAcceptError parses nestedErr and reports whether it is a valid
// error value from Accept functions.
// It returns nil when nestedErr is valid.
internal static error parseAcceptError(error nestedErr) {
    if (nestedErr == nil) {
        return default;
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        nestedErr = err.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);

second:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: { // for Plan 9
        nestedErr = err.Err;
        goto third;
        break;
    }}
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, ErrClosed) || AreEqual(ᴛ, errTimeout) || AreEqual(ᴛ, poll.ErrNotPollable) || AreEqual(ᴛ, Δos.ErrDeadlineExceeded): {
        return default;
    }}
    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);

third:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

public static void TestAcceptError(ж<testing.T> t) {
    var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
        for (;;) {
            ln._<ж<global::go.net_package.TCPListener>>().SetDeadline(time.Now().Add(5 * time.Millisecond));
            var (c, err) = ln.Accept();
            {
                var perr = parseAcceptError(err); if (perr != nil) {
                    t.Error(perr);
                }
            }
            if (err != nil) {
                if (c != nil) {
                    t.Errorf("Accept returned non-nil interface %T(%v) with err != nil", c, c);
                }
                {
                    var (nerr, ok) = err._<ΔError>(ᐧ); if (!ok || (!nerr.Timeout() && !nerr.Temporary())) {
                        return;
                    }
                }
                continue;
            }
            c.Close();
        }
    };
    var ls = newLocalServer(new net_test_package.testing_TжTB(t), tcpˢ);
    {
        var err = ls.buildup(handler); if (err != nil) {
            ls.teardown();
            t.Fatal(err);
        }
    }

    time.Sleep(100 * time.Millisecond);
    ls.teardown();
}

// parseCommonError parses nestedErr and reports whether it is a valid
// error value from miscellaneous functions.
// It returns nil when nestedErr is valid.
internal static error parseCommonError(error nestedErr) {
    if (nestedErr == nil) {
        return default;
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != nil) {
                return errΔ1;
            }
        }
        nestedErr = err.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);

second:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }
    case ж<Δos.LinkError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: {
        nestedErr = err.Err;
        goto third;
        break;
    }}
    switch (nestedErr) {
    case var ᴛ when AreEqual(ᴛ, ErrClosed): {
        return default;
    }}
    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);

third:
    if (isPlatformError(nestedErr)) {
        return default;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goNettestˢ => ref goNettestˢᵇ;
private static readonly @string goNettestˢᵇ = "go-nettest"u8;
internal static readonly object shouldFailˢ = (@string)"should fail"u8;

public static void TestFileError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Δos.CreateTemp(""u8, goNettestˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Δos.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var c, err) = FileConn(f);
        if (err != nil) {
            if (c != nil) {
                t.Errorf("FileConn returned non-nil interface %T(%v) with err != nil", c, c);
            }
            {
                var perr = parseCommonError(err); if (perr != nil) {
                    t.Error(perr);
                }
            }
        } else {
            c.Close();
            t.Error(shouldFailˢ);
        }
        (var ln, err) = FileListener(f);
        if (err != nil) {
            if (ln != nil) {
                t.Errorf("FileListener returned non-nil interface %T(%v) with err != nil", ln, ln);
            }
            {
                var perr = parseCommonError(err); if (perr != nil) {
                    t.Error(perr);
                }
            }
        } else {
            ln.Close();
            t.Error(shouldFailˢ);
        }
        (var pc, err) = FilePacketConn(f);
        if (err != nil) {
            if (pc != nil) {
                t.Errorf("FilePacketConn returned non-nil interface %T(%v) with err != nil", pc, pc);
            }
            {
                var perr = parseCommonError(err); if (perr != nil) {
                    t.Error(perr);
                }
            }
        } else {
            pc.Close();
            t.Error(shouldFailˢ);
        }

        ln = newLocalListener(new net_test_package.testing_TжTB(t), tcpˢ);

        for (nint i = 0; i < 3; i++) {
            var (fΔ1, errΔ1) = ln._<ж<global::go.net_package.TCPListener>>().File();
            if (errΔ1 != nil) {
                {
                    var perr = parseCommonError(errΔ1); if (perr != nil) {
                        t.Error(perr);
                    }
                }
            } else {
                fΔ1.Close();
            }
            ln.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error parseLookupPortError(error nestedErr) {
    if (nestedErr == nil) {
        return default;
    }

    switch (nestedErr.type()) {
    case ж<global::go.net_package.AddrError> _:
    case ж<global::go.net_package.DNSError> _: {
        return default;
    }
    case ж<fs.PathError>: { // for Plan 9
        return default;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errCanceledIsNotContextˢ = (@string)"errCanceled is not context.Canceled"u8;
internal static readonly object errTimeoutIsNotContextˢ = (@string)"errTimeout is not context.DeadlineExceeded"u8;

public static void TestContextError(ж<testing.T> t) {
    if (!errors.Is(errCanceled, context.Canceled)) {
        t.Error(errCanceledIsNotContextˢ);
    }
    if (!errors.Is(errTimeout, context.DeadlineExceeded)) {
        t.Error(errTimeoutIsNotContextˢ);
    }
}

} // end net_internal_test_package
