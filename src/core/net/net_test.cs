// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using socktest = net.@internal.socktest_package;
using Δos = os_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using net.@internal;
using static go.net_package;

partial class net_internal_test_package {

public static void TestCloseRead(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    Ꮡt.Parallel();
    foreach (var (_, network) in new @string[]{"tcp"u8, "unix"u8, "unixpacket"u8}.slice()) {
        @string networkΔ1 = network;
        Ꮡt.Run(networkΔ1, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("network %s is not testable on the current platform"u8, networkΔ1);
                }
                tΔ1.Parallel();
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var exprᴛ2 = networkΔ1;
                if (exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8) {
                    defer(Δos.Remove, ln.Addr().String(), ref ᒐ);
                }

                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var exprᴛ3 = networkΔ1;
                if (exprᴛ3 == "unix"u8 || exprᴛ3 == "unixpacket"u8) {
                    defer(Δos.Remove, c.LocalAddr().String(), ref ᒐ);
                }

                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                switch (c.type()) {
                case ж<global::go.net_package.TCPConn> cΔ1: {
                    err = cΔ1.CloseRead();
                    break;
                }
                case ж<global::go.net_package.UnixConn> cΔ1: {
                    err = cΔ1.CloseRead();
                    break;
                }}
                if (err != default!) {
                    {
                        var perr = parseCloseError(err, true); if (perr != default!) {
                            tΔ1.Error(perr);
                        }
                    }
                    tΔ1.Fatal(err);
                }
                array<byte> b = new(1);
                (var n, err) = c.Read(b[..]);
                if (n != 0 || err == default!) {
                    tΔ1.Fatalf("got (%d, %v); want (0, error)"u8, n, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestCloseWrite(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("not supported on %s"u8, Δruntime.GOOS);
    }

    Ꮡt.Parallel();
    ref var deadline = ref heap<time.Time>(out var Ꮡdeadline);
    (deadline, _) = t.Deadline();
    if (!deadline.IsZero()) {
        // Leave 10% headroom on the deadline to report errors and clean up.
        deadline = deadline.Add(-time.Until(deadline) / 10);
    }
    foreach (var (_, network) in new @string[]{"tcp"u8, "unix"u8, "unixpacket"u8}.slice()) {
        @string networkΔ1 = network;
        var deadlineʗ1 = deadline;
        Ꮡt.Run(networkΔ1, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("network %s is not testable on the current platform"u8, networkΔ1);
                }
                tΔ1.Parallel();
                var deadlineʗ2 = deadlineʗ1;
                var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
                    GoFrame ᒐ = default;
                    try {
                        var (cΔ1, errΔ1) = ln.Accept();
                        if (errΔ1 != default!) {
                            tΔ1.Error(errΔ1);
                            return;
                        }
                        // Workaround for https://go.dev/issue/49352.
                        // On arm64 macOS (current as of macOS 12.4),
                        // reading from a socket at the same time as the client
                        // is closing it occasionally hangs for 60 seconds before
                        // returning ECONNRESET. Sleep for a bit to give the
                        // socket time to close before trying to read from it.
                        if (Δruntime.GOOS == "darwin"u8 && Δruntime.GOARCH == "arm64"u8) {
                            time.Sleep(10 * time.Millisecond);
                        }
                        if (!deadlineʗ2.IsZero()) {
                            cΔ1.SetDeadline(deadlineʗ2);
                        }
                        var cʗ1 = cΔ1;
                        defer(() => cʗ1.Close(), ref ᒐ);
                        array<byte> bΔ1 = new(1);
                        (var nΔ1, errΔ1) = cΔ1.Read(bΔ1[..]);
                        if (nΔ1 != 0 || !AreEqual(errΔ1, Δio.EOF)) {
                            tΔ1.Errorf("got (%d, %v); want (0, io.EOF)"u8, nΔ1, errΔ1);
                            return;
                        }
                        switch (cΔ1.type()) {
                        case ж<global::go.net_package.TCPConn> cΔ2: {
                            errΔ1 = cΔ2.CloseWrite();
                            break;
                        }
                        case ж<global::go.net_package.UnixConn> cΔ2: {
                            errΔ1 = cΔ2.CloseWrite();
                            break;
                        }}
                        if (errΔ1 != default!) {
                            {
                                var perr = parseCloseError(errΔ1, true); if (perr != default!) {
                                    tΔ1.Error(perr);
                                }
                            }
                            tΔ1.Error(errΔ1);
                            return;
                        }
                        (nΔ1, errΔ1) = cΔ1.Write(bΔ1[..]);
                        if (errΔ1 == default!) {
                            tΔ1.Errorf("got (%d, %v); want (any, error)"u8, nΔ1, errΔ1);
                            return;
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                };
                var ls = newLocalServer(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var lsʗ1 = ls;
                defer(() => lsʗ1.teardown(), ref ᒐ);
                {
                    var errΔ2 = ls.buildup(handler); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
                var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                if (!deadlineʗ1.IsZero()) {
                    c.SetDeadline(deadlineʗ1);
                }
                var exprᴛ2 = networkΔ1;
                if (exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8) {
                    defer(Δos.Remove, c.LocalAddr().String(), ref ᒐ);
                }

                var cʗ2 = c;
                defer(() => cʗ2.Close(), ref ᒐ);
                switch (c.type()) {
                case ж<global::go.net_package.TCPConn> cΔ1: {
                    err = cΔ1.CloseWrite();
                    break;
                }
                case ж<global::go.net_package.UnixConn> cΔ1: {
                    err = cΔ1.CloseWrite();
                    break;
                }}
                if (err != default!) {
                    {
                        var perr = parseCloseError(err, true); if (perr != default!) {
                            tΔ1.Error(perr);
                        }
                    }
                    tΔ1.Fatal(err);
                }
                array<byte> b = new(1);
                (var n, err) = c.Read(b[..]);
                if (n != 0 || !AreEqual(err, Δio.EOF)) {
                    tΔ1.Fatalf("got (%d, %v); want (0, io.EOF)"u8, n, err);
                }
                (n, err) = c.Write(b[..]);
                if (err == default!) {
                    tΔ1.Fatalf("got (%d, %v); want (any, error)"u8, n, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestConnClose(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, network) in new @string[]{"tcp"u8, "unix"u8, "unixpacket"u8}.slice()) {
        @string networkΔ1 = network;
        Ꮡt.Run(networkΔ1, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("network %s is not testable on the current platform"u8, networkΔ1);
                }
                tΔ1.Parallel();
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var exprᴛ1 = networkΔ1;
                if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
                    defer(Δos.Remove, ln.Addr().String(), ref ᒐ);
                }

                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                var (c, err) = Dial(ln.Addr().Network(), ln.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var exprᴛ2 = networkΔ1;
                if (exprᴛ2 == "unix"u8 || exprᴛ2 == "unixpacket"u8) {
                    defer(Δos.Remove, c.LocalAddr().String(), ref ᒐ);
                }

                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                {
                    var errΔ1 = c.Close(); if (errΔ1 != default!) {
                        {
                            var perr = parseCloseError(errΔ1, false); if (perr != default!) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ1);
                    }
                }
                array<byte> b = new(1);
                (var n, err) = c.Read(b[..]);
                if (n != 0 || err == default!) {
                    tΔ1.Fatalf("got (%d, %v); want (0, error)"u8, n, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestListenerClose(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, network) in new @string[]{"tcp"u8, "unix"u8, "unixpacket"u8}.slice()) {
        @string networkΔ1 = network;
        Ꮡt.Run(networkΔ1, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("network %s is not testable on the current platform"u8, networkΔ1);
                }
                tΔ1.Parallel();
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var exprᴛ1 = networkΔ1;
                if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
                    defer(Δos.Remove, ln.Addr().String(), ref ᒐ);
                }

                {
                    var errΔ1 = ln.Close(); if (errΔ1 != default!) {
                        {
                            var perr = parseCloseError(errΔ1, false); if (perr != default!) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ1);
                    }
                }
                var (c, err) = ln.Accept();
                if (err == default!) {
                    c.Close();
                    tΔ1.Fatal(shouldFailˢ);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Note: we cannot ensure that a subsequent Dial does not succeed, because
// we do not in general have any guarantee that ln.Addr is not immediately
// reused. (TCP sockets enter a TIME_WAIT state when closed, but that only
// applies to existing connections for the port — it does not prevent the
// port itself from being used for entirely new connections in the
// meantime.)
public static void TestPacketConnClose(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, network) in new @string[]{"udp"u8, "unixgram"u8}.slice()) {
        @string networkΔ1 = network;
        Ꮡt.Run(networkΔ1, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("network %s is not testable on the current platform"u8, networkΔ1);
                }
                tΔ1.Parallel();
                var c = newLocalPacketListener(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var exprᴛ1 = networkΔ1;
                if (exprᴛ1 == "unixgram"u8) {
                    defer(Δos.Remove, c.LocalAddr().String(), ref ᒐ);
                }

                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                {
                    var errΔ1 = c.Close(); if (errΔ1 != default!) {
                        {
                            var perr = parseCloseError(errΔ1, false); if (perr != default!) {
                                tΔ1.Error(perr);
                            }
                        }
                        tΔ1.Fatal(errΔ1);
                    }
                }
                array<byte> b = new(1);
                var (n, _, err) = c.ReadFrom(b[..]);
                if (n != 0 || err == default!) {
                    tΔ1.Fatalf("got (%d, %v); want (0, error)"u8, n, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// See golang.org/issue/6163, golang.org/issue/6987.
public static partial void TestAcceptIgnoreAbortedConnRequest(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("%s does not have full support of socktest"u8, Δruntime.GOOS);
        }

        var syserr = new channel<error>(0);
        var syserrʗ1 = syserr;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), syserrʗ1, ref ᒐ);
                foreach (var (_, errΔ1) in abortedConnRequestErrors) {
                    syserrʗ1.ᐸꟷ(errΔ1);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var syserrʗ2 = syserr;
        Ꮡsw.Set(socktest.FilterAccept, (ж<socktest.Status> so) => {
            {
                var (errΔ2, ok) = ᐸꟷ(syserrʗ2, ꟷ); if (ok) {
                    return (default!, errΔ2);
                }
            }
            return (default!, default!);
        });
        defer(Ꮡsw.Set, socktest.FilterAccept, (socktest.Filter)(default!), ref ᒐ);
        var operr = new channel<error>(1);
        var operrʗ1 = operr;
        var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener ln) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), operrʗ1, ref ᒐ);
                var (cΔ1, errΔ3) = ln.Accept();
                if (errΔ3 != default!) {
                    {
                        var perr = parseAcceptError(errΔ3); if (perr != default!) {
                            operrʗ1.ᐸꟷ(perr);
                        }
                    }
                    operrʗ1.ᐸꟷ(errΔ3);
                    return;
                }
                cΔ1.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        };
        var ls = newLocalServer(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lsʗ1 = ls;
        defer(() => lsʗ1.teardown(), ref ᒐ);
        {
            var errΔ4 = ls.buildup(handler); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
        var (c, err) = Dial((~ls).Listener.Addr().Network(), (~ls).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        c.Close();
        foreach (var errΔ5 in operr) {
            Ꮡt.Error(errΔ5);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestZeroByteRead(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, network) in new @string[]{"tcp"u8, "unix"u8, "unixpacket"u8}.slice()) {
        @string networkΔ1 = network;
        Ꮡt.Run(networkΔ1, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (!testableNetwork(networkΔ1)) {
                    tΔ1.Skipf("network %s is not testable on the current platform"u8, networkΔ1);
                }
                tΔ1.Parallel();
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), networkΔ1);
                var connc = new channel<global::go.net_package.Conn>(1);
                var conncʗ1 = connc;
                var lnʗ1 = ln;
                defer(() => {
                    lnʗ1.Close();
                    foreach (var cΔ1 in conncʗ1) {
                        if (cΔ1 != default!) {
                            cΔ1.Close();
                        }
                    }
                }, ref ᒐ);
                var conncʗ2 = connc;
                var lnʗ2 = ln;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(ᴛ1 => builtin.close(ᴛ1), conncʗ2, ref ᒐ);
                        var (cΔ2, errΔ1) = lnʗ2.Accept();
                        if (errΔ1 != default!) {
                            tΔ1.Error(errΔ1);
                        }
                        conncʗ2.ᐸꟷ(cΔ2); // might be nil
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var (c, err) = Dial(networkΔ1, ln.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                var sc = ᐸꟷ(connc);
                if (sc == default!) {
                    return;
                }
                var scʗ1 = sc;
                defer(() => scʗ1.Close(), ref ᒐ);
                if (Δruntime.GOOS == "windows"u8) {
                    // A zero byte read on Windows caused a wait for readability first.
                    // Rather than change that behavior, satisfy it in this test.
                    // See Issue 15735.
                    goǃ((ᴛ1, ᴛ2) => Δio.WriteString(ᴛ1, ᴛ2), new net_test_package.net_ConnᴠWriter(sc), (@string)"a");
                }
                (var n, err) = c.Read(default!);
                if (n != 0 || err != default!) {
                    tΔ1.Errorf("%s: zero byte client read = %v, %v; want 0, nil"u8, networkΔ1, n, err);
                }
                if (Δruntime.GOOS == "windows"u8) {
                    // Same as comment above.
                    goǃ((ᴛ1, ᴛ2) => Δio.WriteString(ᴛ1, ᴛ2), new net_test_package.net_ConnᴠWriter(c), (@string)"a");
                }
                (n, err) = sc.Read(default!);
                if (n != 0 || err != default!) {
                    tΔ1.Errorf("%s: zero byte server read = %v, %v; want 0, nil"u8, networkΔ1, n, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// withTCPConnPair sets up a TCP connection between two peers, then
// runs peer1 and peer2 concurrently. withTCPConnPair returns when
// both have completed.
internal static partial void withTCPConnPair(ж<testing.T> Ꮡt, Func<ж<global::go.net_package.TCPConn>, error> peer1, Func<ж<global::go.net_package.TCPConn>, error> peer2) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(2);
        var errcʗ1 = errc;
        var lnʗ2 = ln;
        goǃ(() => {
            var (c1, err) = lnʗ2.Accept();
            if (err != default!) {
                errcʗ1.ᐸꟷ(err);
                return;
            }
            err = peer1(c1._<ж<global::go.net_package.TCPConn>>());
            c1.Close();
            errcʗ1.ᐸꟷ(err);
        });
        var errcʗ2 = errc;
        var lnʗ3 = ln;
        goǃ(() => {
            var (c2, err) = Dial(tcpˢ, lnʗ3.Addr().String());
            if (err != default!) {
                errcʗ2.ᐸꟷ(err);
                return;
            }
            err = peer2(c2._<ж<global::go.net_package.TCPConn>>());
            c2.Close();
            errcʗ2.ᐸꟷ(err);
        });
        for (nint i = 0; i < 2; i++) {
            {
                var err = ᐸꟷ(errc); if (err != default!) {
                    Ꮡt.Error(err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeoutWaitingForReadToˢ = "timeout waiting for Read to finish"u8;

// Tests that a blocked Read is interrupted by a concurrent SetReadDeadline
// modifying that Conn's read deadline to the past.
// See golang.org/cl/30164 which documented this. The net/http package
// depends on this.
public static void TestReadTimeoutUnblocksRead(ж<testing.T> Ꮡt) {
    var serverDone = new channel<EmptyStruct>(0);
    var serverDoneʗ1 = serverDone;
    var server = [MethodImpl(MethodImplOptions.NoInlining)] (ж<global::go.net_package.TCPConn> cs) => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), serverDoneʗ1, ref ᒐ);
            var errc = new channel<error>(1);
            var errcʗ1 = errc;
            goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => builtin.close(ᴛ1), errcʗ1, ref ᒐ);
                    goǃ(() => {
                        // TODO: find a better way to wait
                        // until we're blocked in the cs.Read
                        // call below. Sleep is lame.
                        time.Sleep(100 * time.Millisecond);
                        // Interrupt the upcoming Read, unblocking it:
                        cs.of(global::go.net_package.TCPConn.Ꮡconn).SetReadDeadline(time.Unix(123, 0)); // time in the past
                    });
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    var (n, err) = cs.of(global::go.net_package.TCPConn.Ꮡconn).Read(buf[..1]);
                    if (n != 0 || err == default!) {
                        errcʗ1.ᐸꟷ(fmt.Errorf("Read = %v, %v; want 0, non-nil"u8, n, err));
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            var selᴛ9 = errc;
            var selᴛ10 = time.After((time.Duration)(5000000000L));
            switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
            case 0 when selᴛ9.ꟷᐳ(out var err): {
                return err;
            }
            case 1 when selᴛ10.ꟷᐳ(out _): {
                var buf = new slice<byte>((2 << (int)(20)));
                buf = buf.slice(0, Δruntime.Stack(buf, true));
                println((@string)"Stacks at timeout:\n"u8, ((@string)buf));
                return errors.New(timeoutWaitingForReadToˢ);
            }}
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    };
    // Do nothing in the client. Never write. Just wait for the
    // server's half to be done.
    var serverDoneʗ2 = serverDone;
    var client = error (ж<global::go.net_package.TCPConn> _) => {
        ᐸꟷ(serverDoneʗ2);
        return default!;
    };
    withTCPConnPair(Ꮡt, client, server);
}

// Issue 17695: verify that a blocked Read is woken up by a Close.
public static void TestCloseUnblocksRead(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var server = error (ж<global::go.net_package.TCPConn> cs) => {
        // Give the client time to get stuck in a Read:
        time.Sleep(20 * time.Millisecond);
        cs.of(global::go.net_package.TCPConn.Ꮡconn).Close();
        return default!;
    };
    var client = error (ж<global::go.net_package.TCPConn> ss) => {
        var (n, err) = ss.of(global::go.net_package.TCPConn.Ꮡconn).Read(new byte[]{0}.slice());
        if (n != 0 || !AreEqual(err, Δio.EOF)) {
            return fmt.Errorf("Read = %v, %v; want 0, EOF"u8, n, err);
        }
        return default!;
    };
    withTCPConnPair(Ꮡt, client, server);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readSucceededˢ = (@string)"Read succeeded unexpectedly"u8;
internal static readonly object readUnexpectedlyReturnedˢ = (@string)"Read unexpectedly returned io.EOF after socket was abruptly closed"u8;

// Issue 24808: verify that ECONNRESET is not temporary for read.
public static partial void TestNotTemporaryRead(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var serverDone = new channel<EmptyStruct>(0);
        var dialed = new channel<EmptyStruct>(0);
        var dialedʗ1 = dialed;
        var lnʗ1 = ln;
        var serverDoneʗ1 = serverDone;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), serverDoneʗ1, ref ᒐ);
                var (cs, errΔ1) = lnʗ1.Accept();
                if (errΔ1 != default!) {
                    return;
                }
                ᐸꟷ(dialedʗ1);
                cs._<ж<global::go.net_package.TCPConn>>().SetLinger(0);
                cs.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var lnʗ2 = ln;
        var serverDoneʗ2 = serverDone;
        defer(() => {
            lnʗ2.Close();
            ᐸꟷ(serverDoneʗ2);
        }, ref ᒐ);
        var (ss, err) = Dial(tcpˢ, ln.Addr().String());
        builtin.close(dialed);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ssʗ1 = ss;
        defer(() => ssʗ1.Close(), ref ᒐ);
        (_, err) = ss.Read(new byte[]{0}.slice());
        if (err == default!){
            Ꮡt.Fatal(readSucceededˢ);
        } else 
        if (AreEqual(err, Δio.EOF)) {
            // This happens on Plan 9, but for some reason (prior to CL 385314) it was
            // accepted everywhere else too.
            if (Δruntime.GOOS == "plan9"u8) {
                return;
            }
            Ꮡt.Fatal(readUnexpectedlyReturnedˢ);
        }
        {
            var (ne, ok) = err._<ΔError>(ᐧ); if (!ok){
                Ꮡt.Errorf("Read error does not implement net.Error: %v"u8, err);
            } else 
            if (ne.Temporary()) {
                Ꮡt.Errorf("Read error is unexpectedly temporary: %v"u8, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errClosedDoesNotˢ = (@string)"ErrClosed does not implement Error"u8;

// The various errors should implement the Error interface.
public static void TestErrors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    global::go.net_package.ΔError _ᴛ1 = new global::go.net_package.OpErrorжΔError(Ꮡ(new OpError(nil)));
    global::go.net_package.ΔError _ᴛ2 = new global::go.net_package.ParseErrorжΔError(Ꮡ(new ParseError(nil)));
    global::go.net_package.ΔError _ᴛ3 = new global::go.net_package.AddrErrorжΔError(Ꮡ(new AddrError(nil)));
    global::go.net_package.ΔError _ᴛ4 = ((global::go.net_package.UnknownNetworkError)(@string)""u8);
    global::go.net_package.ΔError _ᴛ5 = ((global::go.net_package.InvalidAddrError)(@string)""u8);
    global::go.net_package.ΔError _ᴛ6 = new net_test_package.net_timeoutErrorжΔError(Ꮡ(new timeoutError(nil)));
    global::go.net_package.ΔError _ᴛ7 = new global::go.net_package.DNSConfigErrorжΔError(Ꮡ(new DNSConfigError(nil)));
    global::go.net_package.ΔError _ᴛ8 = new global::go.net_package.DNSErrorжΔError(Ꮡ(new DNSError(nil)));
    // ErrClosed was introduced as type error, so we can't check
    // it using a declaration.
    {
        var (_, ok) = ErrClosed._<ΔError>(ᐧ); if (!ok) {
            Ꮡt.Fatal(errClosedDoesNotˢ);
        }
    }
}

} // end net_internal_test_package
