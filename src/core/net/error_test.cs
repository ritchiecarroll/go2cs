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
    foreach (var (_, addr) in new global::go.net_package.ΔAddr[]{e.Source, e.Addr}.slice()) {
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
    if (e.Err == default!) {
        return fmt.Errorf("OpError.Err is empty: %v"u8, Ꮡe.OrTypedNil());
    }
    return default!;
}

internal partial interface parseDialError_type /*dyn*/ {
    void isAddrinfoErrno();
}

// parseDialError parses nestedErr and reports whether it is a valid
// error value from Dial, Listen functions.
// It returns nil when nestedErr is valid.
internal static error parseDialError(error nestedErr) {
    if (nestedErr == default!) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        nestedErr = err.Value.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
second:
    if (isPlatformError(nestedErr)) {
        return default!;
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
        return default!;
    }
    case {} Δerr when Δerr._<parseDialError_type>(out var err): {
        return default!;
    }
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }}
    // for Plan 9
    var exprᴛ1 = nestedErr;
    if (AreEqual(exprᴛ1, errCanceled) || AreEqual(exprᴛ1, ErrClosed) || AreEqual(exprᴛ1, errMissingAddress) || AreEqual(exprᴛ1, errNoSuitableAddress) || AreEqual(exprᴛ1, context.DeadlineExceeded) || AreEqual(exprᴛ1, context.Canceled)) {
        return default!;
    }

    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);
third:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

internal static slice<prohibitionaryDialArgTestsᴛ1> dialErrorTests = new prohibitionaryDialArgTestsᴛ1[]{
    new("foo"u8, ""u8),
    new("bar"u8, "baz"u8),
    new("datakit"u8, "mh/astro/r70"u8),
    new("tcp"u8, ""u8),
    new("tcp"u8, "127.0.0.1:☺"u8),
    new("tcp"u8, "no-such-name:80"u8),
    new("tcp"u8, "mh/astro/r70:http"u8),
    new("tcp"u8, JoinHostPort("127.0.0.1"u8, "-1"u8)),
    new("tcp"u8, JoinHostPort("127.0.0.1"u8, "123456789"u8)),
    new("udp"u8, JoinHostPort("127.0.0.1"u8, "-1"u8)),
    new("udp"u8, JoinHostPort("127.0.0.1"u8, "123456789"u8)),
    new("ip:icmp"u8, "127.0.0.1"u8),
    new("unix"u8, "/path/to/somewhere"u8),
    new("unixgram"u8, "/path/to/somewhere"u8),
    new("unixpacket"u8, "/path/to/somewhere"u8)
}.slice();

public static void TestDialError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("%s does not have full support of socktest"u8, Δruntime.GOOS);
        }

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context ctx, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => (default!, new global::go.net_package.DNSErrorжerror(Ꮡ(new DNSError(Err: "dial error test"u8, Name: "name"u8, Server: "server"u8, IsTimeout: true))));
        Ꮡsw.Set(socktest.FilterConnect, (ж<socktest.Status> so) => (default!, errOpNotSupported));
        defer(Ꮡsw.Set, socktest.FilterConnect, (socktest.Filter)(default!), ref ᒐ);
        ref var d = ref heap<global::go.net_package.Dialer>(out var Ꮡd);
        d = new Dialer(Timeout: someTimeout);
        foreach (var (i, tt) in dialErrorTests) {
            nint iΔ1 = i;
            ref var ttΔ1 = ref heap<prohibitionaryDialArgTestsᴛ1>(out var ᏑttΔ1);
            ttΔ1 = tt;
            var ttʗ1 = ttΔ1;
            Ꮡt.Run(fmt.Sprint(iΔ1), (ж<testing.T> tΔ1) => {
                var (c, err) = Ꮡd.Dial(ttʗ1.network, ttʗ1.address);
                if (err == default!) {
                    tΔ1.Errorf("should fail; %s:%s->%s"u8, c.LocalAddr().Network(), c.LocalAddr(), c.RemoteAddr());
                    c.Close();
                    return;
                }
                if (ttʗ1.network == "tcp"u8 || ttʗ1.network == "udp"u8) {
                    var nerr = err;
                    {
                        var (op, ok) = nerr._<ж<global::go.net_package.OpError>>(ᐧ); if (ok) {
                            nerr = op.Value.Err;
                        }
                    }
                    {
                        var (sys, ok) = nerr._<ж<Δos.SyscallError>>(ᐧ); if (ok) {
                            nerr = sys.Value.Err;
                        }
                    }
                    if (AreEqual(nerr, errOpNotSupported)) {
                        tΔ1.Fatalf("should fail without %v; %s:%s->"u8, nerr, ttʗ1.network, ttʗ1.address);
                    }
                }
                if (c != default!) {
                    tΔ1.Errorf("Dial returned non-nil interface %T(%v) with err != nil"u8, c, c);
                }
                {
                    err = parseDialError(err); if (err != default!) {
                        tΔ1.Error(err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestProtocolDialError(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "solaris"u8 || exprᴛ1 == "illumos"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    foreach (var (_, network) in new @string[]{"tcp"u8, "udp"u8, "ip:4294967296"u8, "unix"u8, "unixpacket"u8, "unixgram"u8}.slice()) {
        error err = default!;
        var exprᴛ2 = network;
        if (exprᴛ2 == "tcp"u8) {
            (_, err) = DialTCP(network, nil, Ꮡ(new TCPAddr(Port: (1 << (int)(16)))));
        }
        else if (exprᴛ2 == "udp"u8) {
            (_, err) = DialUDP(network, nil, Ꮡ(new UDPAddr(Port: (1 << (int)(16)))));
        }
        else if (exprᴛ2 == "ip:4294967296"u8) {
            (_, err) = DialIP(network, nil, nil);
        }
        else if (exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8 || exprᴛ2 == "unixgram"u8) {
            (_, err) = DialUnix(network, nil, Ꮡ(new UnixAddr(Name: "//"u8)));
        }

        if (err == default!) {
            Ꮡt.Errorf("%s: should fail"u8, network);
            continue;
        }
        {
            var errΔ1 = parseDialError(err); if (errΔ1 != default!) {
                Ꮡt.Errorf("%s: %v"u8, network, errΔ1);
                continue;
            }
        }
        Ꮡt.Logf("%s: error as expected: %v"u8, network, err);
    }
}

internal partial struct TestDialAddrError_type /*dyn*/ {
    internal @string network;
    internal @string lit;
    internal ж<global::go.net_package.TCPAddr> addr;
}

public static void TestDialAddrError(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    if (!supportsIPv4() || !supportsIPv6()) {
        Ꮡt.Skip(bothIPv4AndIPv6Areˢ);
    }
    foreach (var (_, vᴛ1) in new TestDialAddrError_type[]{
        new("tcp4"u8, "::1"u8, nil),
        new("tcp4"u8, ""u8, Ꮡ(new TCPAddr(IP: IPv6loopback))), // We don't test the {"tcp6", "byte sequence", nil}
 // case for now because there is no easy way to
 // control name resolution.

        new("tcp6"u8, ""u8, Ꮡ(new TCPAddr(IP: new IP(new byte[]{0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef}.slice()))))
    }.slice()) {
        ref var tt = ref heap(new TestDialAddrError_type(), out var Ꮡtt);
        tt = vᴛ1;

        @string desc = tt.lit;
        if (desc == ""u8) {
            desc = tt.addr.String();
        }
        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprintf("%s/%s"u8, tt.network, desc), (ж<testing.T> tΔ1) => {
            error err = default!;
            global::go.net_package.Conn c = default!;
            @string op = default!;
            if (ttʗ1.lit != ""u8){
                (c, err) = Dial(ttʗ1.network, JoinHostPort(ttʗ1.lit, "0"u8));
                op = fmt.Sprintf("Dial(%q, %q)"u8, ttʗ1.network, JoinHostPort(ttʗ1.lit, "0"u8));
            } else {
                var (ᴛ1, ᴛ2) = DialTCP(ttʗ1.network, nil, ttʗ1.addr);
                (c, err) = (new global::go.net_package.TCPConnжConn(ᴛ1), ᴛ2);
                op = fmt.Sprintf("DialTCP(%q, %q)"u8, ttʗ1.network, ttʗ1.addr.OrTypedNil());
            }
            tΔ1.Logf("%s: %v"u8, op, err);
            if (err == default!) {
                c.Close();
                tΔ1.Fatalf("%s succeeded, want error"u8, op);
            }
            {
                var perr = parseDialError(err); if (perr != default!) {
                    tΔ1.Fatal(perr);
                }
            }
            var operr = err._<ж<global::go.net_package.OpError>>().Value.Err;
            var (aerr, ok) = operr._<ж<global::go.net_package.AddrError>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("OpError.Err is %T, want *AddrError"u8, operr);
            }
            @string want = ttʗ1.lit;
            if (ttʗ1.lit == ""u8) {
                want = (~ttʗ1.addr).IP.String();
            }
            if ((~aerr).Addr != want) {
                tΔ1.Errorf("error Addr=%q, want %q"u8, (~aerr).Addr, want);
            }
        });
    }
}

internal static slice<prohibitionaryDialArgTestsᴛ1> listenErrorTests = new prohibitionaryDialArgTestsᴛ1[]{
    new("foo"u8, ""u8),
    new("bar"u8, "baz"u8),
    new("datakit"u8, "mh/astro/r70"u8),
    new("tcp"u8, "127.0.0.1:☺"u8),
    new("tcp"u8, "no-such-name:80"u8),
    new("tcp"u8, "mh/astro/r70:http"u8),
    new("tcp"u8, JoinHostPort("127.0.0.1"u8, "-1"u8)),
    new("tcp"u8, JoinHostPort("127.0.0.1"u8, "123456789"u8)),
    new("unix"u8, "/path/to/somewhere"u8),
    new("unixpacket"u8, "/path/to/somewhere"u8)
}.slice();

public static void TestListenError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("%s does not have full support of socktest"u8, Δruntime.GOOS);
        }

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context _, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => (default!, new global::go.net_package.DNSErrorжerror(Ꮡ(new DNSError(Err: "listen error test"u8, Name: "name"u8, Server: "server"u8, IsTimeout: true))));
        Ꮡsw.Set(socktest.FilterListen, (ж<socktest.Status> so) => (default!, errOpNotSupported));
        defer(Ꮡsw.Set, socktest.FilterListen, (socktest.Filter)(default!), ref ᒐ);
        foreach (var (i, vᴛ1) in listenErrorTests) {
            ref var tt = ref heap(new prohibitionaryDialArgTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            Ꮡt.Run(fmt.Sprintf("%s_%s"u8, tt.network, tt.address), (ж<testing.T> tΔ1) => {
                var (ln, err) = Listen(ttʗ1.network, ttʗ1.address);
                if (err == default!) {
                    tΔ1.Errorf("#%d: should fail; %s:%s->"u8, i, ln.Addr().Network(), ln.Addr());
                    ln.Close();
                    return;
                }
                if (ttʗ1.network == "tcp"u8) {
                    var nerr = err;
                    {
                        var (op, ok) = nerr._<ж<global::go.net_package.OpError>>(ᐧ); if (ok) {
                            nerr = op.Value.Err;
                        }
                    }
                    {
                        var (sys, ok) = nerr._<ж<Δos.SyscallError>>(ᐧ); if (ok) {
                            nerr = sys.Value.Err;
                        }
                    }
                    if (AreEqual(nerr, errOpNotSupported)) {
                        tΔ1.Fatalf("#%d: should fail without %v; %s:%s->"u8, i, nerr, ttʗ1.network, ttʗ1.address);
                    }
                }
                if (ln != default!) {
                    tΔ1.Errorf("Listen returned non-nil interface %T(%v) with err != nil"u8, ln, ln);
                }
                {
                    err = parseDialError(err); if (err != default!) {
                        tΔ1.Errorf("#%d: %v"u8, i, err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<prohibitionaryDialArgTestsᴛ1> listenPacketErrorTests = new prohibitionaryDialArgTestsᴛ1[]{
    new("foo"u8, ""u8),
    new("bar"u8, "baz"u8),
    new("datakit"u8, "mh/astro/r70"u8),
    new("udp"u8, "127.0.0.1:☺"u8),
    new("udp"u8, "no-such-name:80"u8),
    new("udp"u8, "mh/astro/r70:http"u8),
    new("udp"u8, JoinHostPort("127.0.0.1"u8, "-1"u8)),
    new("udp"u8, JoinHostPort("127.0.0.1"u8, "123456789"u8))
}.slice();

public static void TestListenPacketError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("%s does not have full support of socktest"u8, Δruntime.GOOS);
        }

        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = (slice<global::go.net_package.IPAddr>, error) (context.Context _, Func<context.Context, @string, @string, (slice<global::go.net_package.IPAddr>, error)> fn, @string network, @string host) => (default!, new global::go.net_package.DNSErrorжerror(Ꮡ(new DNSError(Err: "listen error test"u8, Name: "name"u8, Server: "server"u8, IsTimeout: true))));
        foreach (var (i, vᴛ1) in listenPacketErrorTests) {
            ref var tt = ref heap(new prohibitionaryDialArgTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            Ꮡt.Run(fmt.Sprintf("%s_%s"u8, tt.network, tt.address), (ж<testing.T> tΔ1) => {
                var (c, err) = ListenPacket(ttʗ1.network, ttʗ1.address);
                if (err == default!) {
                    tΔ1.Errorf("#%d: should fail; %s:%s->"u8, i, c.LocalAddr().Network(), c.LocalAddr());
                    c.Close();
                    return;
                }
                if (c != default!) {
                    tΔ1.Errorf("ListenPacket returned non-nil interface %T(%v) with err != nil"u8, c, c);
                }
                {
                    err = parseDialError(err); if (err != default!) {
                        tΔ1.Errorf("#%d: %v"u8, i, err);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestProtocolListenError(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    foreach (var (_, network) in new @string[]{"tcp"u8, "udp"u8, "ip:4294967296"u8, "unix"u8, "unixpacket"u8, "unixgram"u8}.slice()) {
        error err = default!;
        var exprᴛ2 = network;
        if (exprᴛ2 == "tcp"u8) {
            (_, err) = ListenTCP(network, Ꮡ(new TCPAddr(Port: (1 << (int)(16)))));
        }
        else if (exprᴛ2 == "udp"u8) {
            (_, err) = ListenUDP(network, Ꮡ(new UDPAddr(Port: (1 << (int)(16)))));
        }
        else if (exprᴛ2 == "ip:4294967296"u8) {
            (_, err) = ListenIP(network, nil);
        }
        else if (exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8) {
            (_, err) = ListenUnix(network, Ꮡ(new UnixAddr(Name: "//"u8)));
        }
        else if (exprᴛ2 == "unixgram"u8) {
            (_, err) = ListenUnixgram(network, Ꮡ(new UnixAddr(Name: "//"u8)));
        }

        if (err == default!) {
            Ꮡt.Errorf("%s: should fail"u8, network);
            continue;
        }
        {
            err = parseDialError(err); if (err != default!) {
                Ꮡt.Errorf("%s: %v"u8, network, err);
                continue;
            }
        }
    }
}

// parseReadError parses nestedErr and reports whether it is a valid
// error value from Read functions.
// It returns nil when nestedErr is valid.
internal static error parseReadError(error nestedErr) {
    if (nestedErr == default!) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        nestedErr = err.Value.Err;
        goto second;
        break;
    }}
    if (AreEqual(nestedErr, Δio.EOF)) {
        return default!;
    }
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
second:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }}
    var exprᴛ1 = nestedErr;
    if (AreEqual(exprᴛ1, ErrClosed) || AreEqual(exprᴛ1, errTimeout) || AreEqual(exprᴛ1, poll.ErrNotPollable) || AreEqual(exprᴛ1, Δos.ErrDeadlineExceeded)) {
        return default!;
    }

    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);
third:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

// parseWriteError parses nestedErr and reports whether it is a valid
// error value from Write functions.
// It returns nil when nestedErr is valid.
internal static error parseWriteError(error nestedErr) {
    if (nestedErr == default!) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        nestedErr = err.Value.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
second:
    if (isPlatformError(nestedErr)) {
        return default!;
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
        return default!;
    }
    case {} Δerr when Δerr._<parseDialError_type>(out var err): {
        return default!;
    }
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }}
    var exprᴛ1 = nestedErr;
    if (AreEqual(exprᴛ1, errCanceled) || AreEqual(exprᴛ1, ErrClosed) || AreEqual(exprᴛ1, errMissingAddress) || AreEqual(exprᴛ1, errTimeout) || AreEqual(exprᴛ1, Δos.ErrDeadlineExceeded) || AreEqual(exprᴛ1, ErrWriteToConnected) || AreEqual(exprᴛ1, Δio.ErrUnexpectedEOF)) {
        return default!;
    }

    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);
third:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string useOfClosedNetworkˢ = "use of closed network connection"u8;

// parseCloseError parses nestedErr and reports whether it is a valid
// error value from Close functions.
// It returns nil when nestedErr is valid.
internal static error parseCloseError(error nestedErr, bool isShutdown) {
    if (nestedErr == default!) {
        return default!;
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
            var errΔ1 = err.isValid(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        nestedErr = err.Value.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
second:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }}
    // for Plan 9
    var exprᴛ1 = nestedErr;
    if (AreEqual(exprᴛ1, ErrClosed)) {
        return default!;
    }

    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);
third:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    var exprᴛ2 = nestedErr;
    if (AreEqual(exprᴛ2, fs.ErrClosed)) {
        return default!;
    }

    // for Plan 9
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

public static void TestCloseError(ж<testing.T> Ꮡt) {
    Ꮡt.Run(tcpˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), tcpˢ);
            var lnʗ1 = ln;
            defer(() => lnʗ1.Close(), ref ᒐ);
            var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            for (nint i = 0; i < 3; i++) {
                err = c._<ж<global::go.net_package.TCPConn>>().CloseRead();
                {
                    var perr = parseCloseError(err, true); if (perr != default!) {
                        tΔ1.Errorf("#%d: %v"u8, i, perr);
                    }
                }
            }
            for (nint i = 0; i < 3; i++) {
                err = c._<ж<global::go.net_package.TCPConn>>().CloseWrite();
                {
                    var perr = parseCloseError(err, true); if (perr != default!) {
                        tΔ1.Errorf("#%d: %v"u8, i, perr);
                    }
                }
            }
            for (nint i = 0; i < 3; i++) {
                err = c.Close();
                {
                    var perr = parseCloseError(err, false); if (perr != default!) {
                        tΔ1.Errorf("#%d: %v"u8, i, perr);
                    }
                }
                err = ln.Close();
                {
                    var perr = parseCloseError(err, false); if (perr != default!) {
                        tΔ1.Errorf("#%d: %v"u8, i, perr);
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡt.Run(udpˢ, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            if (!testableNetwork(udpˢ)) {
                tΔ2.Skipf("skipping: udp not available"u8);
            }
            var (pc, err) = ListenPacket(udpˢ, "127.0.0.1:0"u8);
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            var pcʗ1 = pc;
            defer(() => pcʗ1.Close(), ref ᒐ);
            for (nint i = 0; i < 3; i++) {
                err = pc.Close();
                {
                    var perr = parseCloseError(err, false); if (perr != default!) {
                        tΔ2.Errorf("#%d: %v"u8, i, perr);
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
    if (nestedErr == default!) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        nestedErr = err.Value.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
second:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }}
    // for Plan 9
    var exprᴛ1 = nestedErr;
    if (AreEqual(exprᴛ1, ErrClosed) || AreEqual(exprᴛ1, errTimeout) || AreEqual(exprᴛ1, poll.ErrNotPollable) || AreEqual(exprᴛ1, Δos.ErrDeadlineExceeded)) {
        return default!;
    }

    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);
third:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

public static void TestAcceptError(ж<testing.T> Ꮡt) {
    var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
        while (ᐧ) {
            ln._<ж<global::go.net_package.TCPListener>>().SetDeadline(time.Now().Add(5 * time.Millisecond));
            var (c, err) = ln.Accept();
            {
                var perr = parseAcceptError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
            if (err != default!) {
                if (c != default!) {
                    Ꮡt.Errorf("Accept returned non-nil interface %T(%v) with err != nil"u8, c, c);
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
    var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
    {
        var err = ls.buildup(handler); if (err != default!) {
            ls.teardown();
            Ꮡt.Fatal(err);
        }
    }
    time.Sleep(100 * time.Millisecond);
    ls.teardown();
}

// parseCommonError parses nestedErr and reports whether it is a valid
// error value from miscellaneous functions.
// It returns nil when nestedErr is valid.
internal static error parseCommonError(error nestedErr) {
    if (nestedErr == default!) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.OpError> err: {
        {
            var errΔ1 = err.isValid(); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        nestedErr = err.Value.Err;
        goto second;
        break;
    }}
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
second:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<Δos.SyscallError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }
    case ж<Δos.LinkError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }
    case ж<fs.PathError> err: {
        nestedErr = err.Value.Err;
        goto third;
        break;
    }}
    var exprᴛ1 = nestedErr;
    if (AreEqual(exprᴛ1, ErrClosed)) {
        return default!;
    }

    return fmt.Errorf("unexpected type on 2nd nested level: %T"u8, nestedErr);
third:
    if (isPlatformError(nestedErr)) {
        return default!;
    }
    return fmt.Errorf("unexpected type on 3rd nested level: %T"u8, nestedErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goNettestˢ = "go-nettest"u8;
internal static readonly object shouldFailˢ = (@string)"should fail"u8;

public static void TestFileError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "windows"u8) {
            Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
        }

        var (f, err) = Δos.CreateTemp(""u8, goNettestˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(Δos.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var c, err) = FileConn(f);
        if (err != default!){
            if (c != default!) {
                Ꮡt.Errorf("FileConn returned non-nil interface %T(%v) with err != nil"u8, c, c);
            }
            {
                var perr = parseCommonError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
        } else {
            c.Close();
            Ꮡt.Error(shouldFailˢ);
        }
        (var ln, err) = FileListener(f);
        if (err != default!){
            if (ln != default!) {
                Ꮡt.Errorf("FileListener returned non-nil interface %T(%v) with err != nil"u8, ln, ln);
            }
            {
                var perr = parseCommonError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
        } else {
            ln.Close();
            Ꮡt.Error(shouldFailˢ);
        }
        (var pc, err) = FilePacketConn(f);
        if (err != default!){
            if (pc != default!) {
                Ꮡt.Errorf("FilePacketConn returned non-nil interface %T(%v) with err != nil"u8, pc, pc);
            }
            {
                var perr = parseCommonError(err); if (perr != default!) {
                    Ꮡt.Error(perr);
                }
            }
        } else {
            pc.Close();
            Ꮡt.Error(shouldFailˢ);
        }
        ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        for (nint i = 0; i < 3; i++) {
            var (fΔ1, errΔ1) = ln._<ж<global::go.net_package.TCPListener>>().File();
            if (errΔ1 != default!){
                {
                    var perr = parseCommonError(errΔ1); if (perr != default!) {
                        Ꮡt.Error(perr);
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
    if (nestedErr == default!) {
        return default!;
    }
    switch (nestedErr.type()) {
    case ж<global::go.net_package.AddrError> _:
    case ж<global::go.net_package.DNSError> _: {
        return default!;
    }
    case ж<fs.PathError>: {
        return default!;
    }}

    // for Plan 9
    return fmt.Errorf("unexpected type on 1st nested level: %T"u8, nestedErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errCanceledIsNotContextˢ = (@string)"errCanceled is not context.Canceled"u8;
internal static readonly object errTimeoutIsNotContextˢ = (@string)"errTimeout is not context.DeadlineExceeded"u8;

public static void TestContextError(ж<testing.T> Ꮡt) {
    if (!errors.Is(errCanceled, context.Canceled)) {
        Ꮡt.Error(errCanceledIsNotContextˢ);
    }
    if (!errors.Is(errTimeout, context.DeadlineExceeded)) {
        Ꮡt.Error(errTimeoutIsNotContextˢ);
    }
}

} // end net_internal_test_package
