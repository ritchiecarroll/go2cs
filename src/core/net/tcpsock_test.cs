// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.net_package;

partial class net_internal_test_package {

public static void BenchmarkTCP4OneShot(ж<testing.B> Ꮡb) {
    benchmarkTCP(Ꮡb, false, false, "127.0.0.1:0"u8);
}

public static void BenchmarkTCP4OneShotTimeout(ж<testing.B> Ꮡb) {
    benchmarkTCP(Ꮡb, false, true, "127.0.0.1:0"u8);
}

public static void BenchmarkTCP4Persistent(ж<testing.B> Ꮡb) {
    benchmarkTCP(Ꮡb, true, false, "127.0.0.1:0"u8);
}

public static void BenchmarkTCP4PersistentTimeout(ж<testing.B> Ꮡb) {
    benchmarkTCP(Ꮡb, true, true, "127.0.0.1:0"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object ipv6IsNotSupportedˢ = (@string)"ipv6 is not supported"u8;

public static void BenchmarkTCP6OneShot(ж<testing.B> Ꮡb) {
    if (!supportsIPv6()) {
        Ꮡb.Skip(ipv6IsNotSupportedˢ);
    }
    benchmarkTCP(Ꮡb, false, false, "[::1]:0"u8);
}

public static void BenchmarkTCP6OneShotTimeout(ж<testing.B> Ꮡb) {
    if (!supportsIPv6()) {
        Ꮡb.Skip(ipv6IsNotSupportedˢ);
    }
    benchmarkTCP(Ꮡb, false, true, "[::1]:0"u8);
}

public static void BenchmarkTCP6Persistent(ж<testing.B> Ꮡb) {
    if (!supportsIPv6()) {
        Ꮡb.Skip(ipv6IsNotSupportedˢ);
    }
    benchmarkTCP(Ꮡb, true, false, "[::1]:0"u8);
}

public static void BenchmarkTCP6PersistentTimeout(ж<testing.B> Ꮡb) {
    if (!supportsIPv6()) {
        Ꮡb.Skip(ipv6IsNotSupportedˢ);
    }
    benchmarkTCP(Ꮡb, true, true, "[::1]:0"u8);
}

internal static partial void benchmarkTCP(ж<testing.B> Ꮡb, bool persistent, bool timeout, @string laddr) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ᏑtestHookUninstaller.Do(uninstallTestHooks);
        UntypedInt msgLen = 512;
        nint conns = b.N;
        nint numConcurrent = Δruntime.GOMAXPROCS(-1) * 2;
        nint msgs = 1;
        if (persistent) {
            conns = numConcurrent;
            msgs = quo(b.N, conns);
            if (msgs == 0) {
                msgs = 1;
            }
            if (conns > b.N) {
                conns = b.N;
            }
        }
        bool sendMsg(global::go.net_package.Conn c, slice<byte> buf) {
            var (n, errΔ1) = c.Write(buf);
            if (n != len(buf) || errΔ1 != default!) {
                Ꮡb.Log(errΔ1);
                return false;
            }
            return true;
        }
        bool recvMsg(global::go.net_package.Conn c, slice<byte> buf) {
            for (nint read = 0; read != len(buf); ) {
                var (n, errΔ2) = c.Read(buf);
                read += n;
                if (errΔ2 != default!) {
                    Ꮡb.Log(errΔ2);
                    return false;
                }
            }
            return true;
        }
        var (ln, err) = Listen(tcpˢ, laddr);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var serverSem = new channel<bool>(numConcurrent);
        // Acceptor.
        var lnʗ2 = ln;
        var recvMsgʗ1 = recvMsg;
        var sendMsgʗ1 = sendMsg;
        var serverSemʗ1 = serverSem;
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
            while (ᐧ) {
                var (c, errΔ3) = lnʗ2.Accept();
                if (errΔ3 != default!) {
                    break;
                }
                serverSemʗ1.ᐸꟷ(true);
                // Server connection.
                var recvMsgʗ2 = recvMsgʗ1;
                var sendMsgʗ2 = sendMsgʗ1;
                var serverSemʗ2 = serverSemʗ1;
                goǃ((global::go.net_package.Conn cΔ1) => {
                    GoFrame ᒐ = default;
                    try {
                        var serverSemʗ3 = serverSemʗ2;
                        defer(() => {
                            cΔ1.Close();
                            ᐸꟷ(serverSemʗ3);
                        }, ref ᒐ);
                        if (timeout) {
                            cΔ1.SetDeadline(time.Now().Add(time.ΔHour)); // Not intended to fire.
                        }
                        ref var buf = ref heap(new array<byte>(512), out var Ꮡbuf);
                        for (nint m = 0; m < msgs; m++) {
                            if (!recvMsgʗ2(cΔ1, buf[..]) || !sendMsgʗ2(cΔ1, buf[..])) {
                                break;
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, c);
            }
        });
        var clientSem = new channel<bool>(numConcurrent);
        for (nint i = 0; i < conns; i++) {
            clientSem.ᐸꟷ(true);
            // Client connection.
            var clientSemʗ1 = clientSem;
            var lnʗ3 = ln;
            var recvMsgʗ3 = recvMsg;
            var sendMsgʗ3 = sendMsg;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    var clientSemʗ2 = clientSemʗ1;
                    defer(() => {
                        ᐸꟷ(clientSemʗ2);
                    }, ref ᒐ);
                    var (c, errΔ4) = Dial(tcpˢ, lnʗ3.Addr().String());
                    if (errΔ4 != default!) {
                        Ꮡb.Log(errΔ4);
                        return;
                    }
                    var cʗ1 = c;
                    defer(() => cʗ1.Close(), ref ᒐ);
                    if (timeout) {
                        c.SetDeadline(time.Now().Add(time.ΔHour)); // Not intended to fire.
                    }
                    ref var buf = ref heap(new array<byte>(512), out var Ꮡbuf);
                    for (nint m = 0; m < msgs; m++) {
                        if (!sendMsgʗ3(c, buf[..]) || !recvMsgʗ3(c, buf[..])) {
                            break;
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        for (nint i = 0; i < numConcurrent; i++) {
            clientSem.ᐸꟷ(true);
            serverSem.ᐸꟷ(true);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkTCP4ConcurrentReadWrite(ж<testing.B> Ꮡb) {
    benchmarkTCPConcurrentReadWrite(Ꮡb, "127.0.0.1:0"u8);
}

public static void BenchmarkTCP6ConcurrentReadWrite(ж<testing.B> Ꮡb) {
    if (!supportsIPv6()) {
        Ꮡb.Skip(ipv6IsNotSupportedˢ);
    }
    benchmarkTCPConcurrentReadWrite(Ꮡb, "[::1]:0"u8);
}

internal static partial void benchmarkTCPConcurrentReadWrite(ж<testing.B> Ꮡb, @string laddr) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ᏑtestHookUninstaller.Do(uninstallTestHooks);
        // The benchmark creates GOMAXPROCS client/server pairs.
        // Each pair creates 4 goroutines: client reader/writer and server reader/writer.
        // The benchmark stresses concurrent reading and writing to the same connection.
        // Such pattern is used in net/http and net/rpc.
        b.StopTimer();
        nint P = Δruntime.GOMAXPROCS(0);
        nint N = quo(b.N, P);
        nint W = 1000;
        // Setup P client/server connections.
        var clients = new slice<global::go.net_package.Conn>(P);
        var servers = new slice<global::go.net_package.Conn>(P);
        var (ln, err) = Listen(tcpˢ, laddr);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        var lnʗ2 = ln;
        var serversʗ1 = servers;
        goǃ(() => {
            for (nint p = 0; p < P; p++) {
                var (s, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    Ꮡb.Error(errΔ1);
                    return;
                }
                serversʗ1[p] = s;
            }
            doneʗ1.ᐸꟷ(true);
        });
        for (nint p = 0; p < P; p++) {
            var (c, errΔ2) = Dial(tcpˢ, ln.Addr().String());
            if (errΔ2 != default!) {
                Ꮡb.Fatal(errΔ2);
            }
            clients[p] = c;
        }
        ᐸꟷ(done);
        b.StartTimer();
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(4 * P);
        for (nint p = 0; p < P; p++) {
            // Client writer.
            goǃ((global::go.net_package.Conn c) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var v = (byte)i;
                        for (nint w = 0; w < W; w++) {
                            v *= v;
                        }
                        buf[0] = v;
                        var (_, errΔ3) = c.Write(buf[..]);
                        if (errΔ3 != default!) {
                            Ꮡb.Error(errΔ3);
                            return;
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, clients[p]);
            // Pipe between server reader and server writer.
            var pipe = new channel<byte>(128);
            // Server reader.
            var pipeʗ1 = pipe;
            goǃ((global::go.net_package.Conn s) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var (_, errΔ4) = s.Read(buf[..]);
                        if (errΔ4 != default!) {
                            Ꮡb.Error(errΔ4);
                            return;
                        }
                        pipeʗ1.ᐸꟷ(buf[0]);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, servers[p]);
            // Server writer.
            var pipeʗ2 = pipe;
            goǃ((global::go.net_package.Conn s) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var v = (byte)(ᐸꟷ(pipeʗ2));
                        for (nint w = 0; w < W; w++) {
                            v *= v;
                        }
                        buf[0] = v;
                        var (_, errΔ5) = s.Write(buf[..]);
                        if (errΔ5 != default!) {
                            Ꮡb.Error(errΔ5);
                            return;
                        }
                    }
                    s.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, servers[p]);
            // Client reader.
            goǃ((global::go.net_package.Conn c) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                    for (nint i = 0; i < N; i++) {
                        var (_, errΔ6) = c.Read(buf[..]);
                        if (errΔ6 != default!) {
                            Ꮡb.Error(errΔ6);
                            return;
                        }
                    }
                    c.Close();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, clients[p]);
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct resolveTCPAddrTest {
    internal @string network;
    internal @string litAddrOrName;
    internal ж<global::go.net_package.TCPAddr> addr;
    internal error err;
}

// Go 1.0 behavior
// Go 1.0 behavior
internal static slice<resolveTCPAddrTest> resolveTCPAddrTests;
internal static void initᴛresolveTCPAddrTests() { resolveTCPAddrTests = new resolveTCPAddrTest[]{
    new("tcp"u8, "127.0.0.1:0"u8, Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 0)), default!),
    new("tcp4"u8, "127.0.0.1:65535"u8, Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 65535)), default!),
    new("tcp"u8, "[::1]:0"u8, Ꮡ(new TCPAddr(IP: ParseIP("::1"u8), Port: 0)), default!),
    new("tcp6"u8, "[::1]:65535"u8, Ꮡ(new TCPAddr(IP: ParseIP("::1"u8), Port: 65535)), default!),
    new("tcp"u8, "[::1%en0]:1"u8, Ꮡ(new TCPAddr(IP: ParseIP("::1"u8), Port: 1, Zone: "en0"u8)), default!),
    new("tcp6"u8, "[::1%911]:2"u8, Ꮡ(new TCPAddr(IP: ParseIP("::1"u8), Port: 2, Zone: "911"u8)), default!),
    new(""u8, "127.0.0.1:0"u8, Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1), Port: 0)), default!),
    new(""u8, "[::1]:0"u8, Ꮡ(new TCPAddr(IP: ParseIP("::1"u8), Port: 0)), default!),
    new("tcp"u8, ":12345"u8, Ꮡ(new TCPAddr(Port: 12345)), default!),
    new("http"u8, "127.0.0.1:0"u8, nil, new net_test_package.net_UnknownNetworkErrorᴠerror(((global::go.net_package.UnknownNetworkError)(@string)"http"u8))),
    new("tcp"u8, "127.0.0.1:http"u8, Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8), Port: 80)), default!),
    new("tcp"u8, "[::ffff:127.0.0.1]:http"u8, Ꮡ(new TCPAddr(IP: ParseIP("::ffff:127.0.0.1"u8), Port: 80)), default!),
    new("tcp"u8, "[2001:db8::1]:http"u8, Ꮡ(new TCPAddr(IP: ParseIP("2001:db8::1"u8), Port: 80)), default!),
    new("tcp4"u8, "127.0.0.1:http"u8, Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8), Port: 80)), default!),
    new("tcp4"u8, "[::ffff:127.0.0.1]:http"u8, Ꮡ(new TCPAddr(IP: ParseIP("127.0.0.1"u8), Port: 80)), default!),
    new("tcp6"u8, "[2001:db8::1]:http"u8, Ꮡ(new TCPAddr(IP: ParseIP("2001:db8::1"u8), Port: 80)), default!),
    new("tcp4"u8, "[2001:db8::1]:http"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "2001:db8::1"u8)))),
    new("tcp6"u8, "127.0.0.1:http"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "127.0.0.1"u8)))),
    new("tcp6"u8, "[::ffff:127.0.0.1]:http"u8, nil, new global::go.net_package.AddrErrorжerror(Ꮡ(new AddrError(Err: errNoSuitableAddress.Error(), Addr: "::ffff:127.0.0.1"u8))))
}.slice(); }

public static void TestResolveTCPAddr(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var origTestHookLookupIP = testHookLookupIP;
        var origTestHookLookupIPʗ1 = origTestHookLookupIP;
        defer(() => {
            testHookLookupIP = origTestHookLookupIPʗ1;
        }, ref ᒐ);
        testHookLookupIP = lookupLocalhost;
        foreach (var (_, tt) in resolveTCPAddrTests) {
            var (addr, err) = ResolveTCPAddr(tt.network, tt.litAddrOrName);
            if (!reflect.DeepEqual(addr.OrTypedNil(), tt.addr.OrTypedNil()) || !reflect.DeepEqual(err, tt.err)) {
                Ꮡt.Errorf("ResolveTCPAddr(%q, %q) = %#v, %v, want %#v, %v"u8, tt.network, tt.litAddrOrName, addr.OrTypedNil(), err, tt.addr.OrTypedNil(), tt.err);
                continue;
            }
            if (err == default!) {
                var (addr2, errΔ1) = ResolveTCPAddr(addr.Network(), addr.String());
                if (!reflect.DeepEqual(addr2.OrTypedNil(), tt.addr.OrTypedNil()) || !AreEqual(errΔ1, tt.err)) {
                    Ꮡt.Errorf("(%q, %q): ResolveTCPAddr(%q, %q) = %#v, %v, want %#v, %v"u8, tt.network, tt.litAddrOrName, addr.Network(), addr.String(), addr2.OrTypedNil(), errΔ1, tt.addr.OrTypedNil(), tt.err);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


partial struct tcpListenerNameTestsᴛ1 /*dyn*/ {
    internal @string net;
    internal ж<global::go.net_package.TCPAddr> laddr;
}
internal static slice<tcpListenerNameTestsᴛ1> tcpListenerNameTests;
internal static void initᴛtcpListenerNameTests() { tcpListenerNameTests = new tcpListenerNameTestsᴛ1[]{
    new("tcp4"u8, Ꮡ(new TCPAddr(IP: IPv4(127, 0, 0, 1)))),
    new("tcp4"u8, Ꮡ(new TCPAddr(nil))),
    new("tcp4"u8, nil)
}.slice(); }

public static void TestTCPListenerName(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        foreach (var (_, tt) in tcpListenerNameTests) {
            var (ln, err) = ListenTCP(tt.net, tt.laddr);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var lnʗ1 = ln;
            defer(() => lnʗ1.Close(), ref ᒐ);
            var la = ln.Addr();
            {
                var (a, ok) = la._<ж<global::go.net_package.TCPAddr>>(ᐧ); if (!ok || (~a).Port == 0) {
                    Ꮡt.Fatalf("got %v; expected a proper address with non-zero port number"u8, la);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIPv6LinkLocalUnicastTCP(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExternalNetwork(new net_test_package.testing_TжTB(Ꮡt));
        if (!supportsIPv6()) {
            Ꮡt.Skip(iPv6IsNotSupportedˢ);
        }
        foreach (var (i, tt) in ipv6LinkLocalUnicastTCPTests) {
            var (ln, err) = Listen(tt.network, tt.address);
            if (err != default!) {
                // It might return "LookupHost returned no
                // suitable address" error on some platforms.
                Ꮡt.Log(err);
                continue;
            }
            var ls = (Ꮡ(new streamListener(Listener: ln))).newLocalServer();
            var lsʗ1 = ls;
            defer(() => lsʗ1.teardown(), ref ᒐ);
            var ch = new channel<error>(1);
            var chʗ1 = ch;
            var handler = (ж<localServer> lsΔ1, global::go.net_package.Listener lnΔ1) => {
                lsΔ1.transponder(lnΔ1, chʗ1.WithDirection(GoChanDir.Send));
            };
            {
                var errΔ1 = ls.buildup(handler); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                var (la, ok) = ln.Addr()._<ж<global::go.net_package.TCPAddr>>(ᐧ); if (!ok || !tt.nameLookup && (~la).Zone == ""u8) {
                    Ꮡt.Fatalf("got %v; expected a proper address with zone identifier"u8, la.OrTypedNil());
                }
            }
            (var c, err) = Dial(tt.network, (~ls).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            {
                var (la, ok) = c.LocalAddr()._<ж<global::go.net_package.TCPAddr>>(ᐧ); if (!ok || !tt.nameLookup && (~la).Zone == ""u8) {
                    Ꮡt.Fatalf("got %v; expected a proper address with zone identifier"u8, la.OrTypedNil());
                }
            }
            {
                var (ra, ok) = c.RemoteAddr()._<ж<global::go.net_package.TCPAddr>>(ᐧ); if (!ok || !tt.nameLookup && (~ra).Zone == ""u8) {
                    Ꮡt.Fatalf("got %v; expected a proper address with zone identifier"u8, ra.OrTypedNil());
                }
            }
            {
                var (_, errΔ2) = c.Write(slice<byte>("TCP OVER IPV6 LINKLOCAL TEST"u8)); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            var b = new slice<byte>(32);
            {
                var (_, errΔ3) = c.Read(b); if (errΔ3 != default!) {
                    Ꮡt.Fatal(errΔ3);
                }
            }
            foreach (var errΔ4 in ch) {
                Ꮡt.Errorf("#%d: %v"u8, i, errΔ4);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTCPConcurrentAccept(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(4), ref ᒐ);
        var (ln, err) = Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        UntypedInt N = 10;
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(N);
        for (nint i = 0; i < N; i++) {
            var lnʗ1 = ln;
            goǃ(() => {
                while (ᐧ) {
                    var (c, errΔ1) = lnʗ1.Accept();
                    if (errΔ1 != default!) {
                        break;
                    }
                    c.Close();
                }
                Ꮡwg.Done();
            });
        }
        nint attempts = 10 * N;
        nint fails = 0;
        var d = Ꮡ(new Dialer(Timeout: 200 * time.Millisecond));
        for (nint i = 0; i < attempts; i++) {
            var (c, errΔ2) = d.Dial(tcpˢ, ln.Addr().String());
            if (errΔ2 != default!){
                fails++;
            } else {
                c.Close();
            }
        }
        ln.Close();
        Ꮡwg.Wait();
        if (fails > attempts / 9) {
            // see issues 7400 and 7541
            Ꮡt.Fatalf("too many Dial failed: %v"u8, fails);
        }
        if (fails > 0) {
            Ꮡt.Logf("# of failed Dials: %v"u8, fails);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTCPReadWriteAllocs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("not supported on %s"u8, // The implementation of asynchronous cancelable
 // I/O on Plan 9 allocates memory.
 // See net/fd_io_plan9.go.
 Δruntime.GOOS);
        }

        ref var err = ref heap<error>(out var Ꮡerr);
        (var ln, err) = Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        ref var server = ref heap<global::go.net_package.Conn>(out var Ꮡserver);
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        var lnʗ2 = ln;
        goǃ(() => {
            error errΔ1 = default!;
            (Ꮡserver.ValueSlot, errΔ1) = lnʗ2.Accept();
            errcʗ1.ᐸꟷ(errΔ1);
        });
        (var client, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var clientʗ1 = client;
        defer(() => clientʗ1.Close(), ref ᒐ);
        {
            var errΔ2 = ᐸꟷ(errc); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        defer(() => Ꮡserver.ValueSlot.Close(), ref ᒐ);
        ref var buf = ref heap(new array<byte>(128), out var Ꮡbuf);
        var bufʗ1 = buf;
        var clientʗ2 = client;
        var allocs = testing.AllocsPerRun(1000, () => {
            var (_, errΔ3) = Ꮡserver.ValueSlot.Write(bufʗ1[..]);
            if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
            (_, errΔ3) = Δio.ReadFull(new net_test_package.net_ConnᴠReader(clientʗ2), bufʗ1[..]);
            if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        });
        if (allocs > 0D) {
            Ꮡt.Fatalf("got %v; want 0"u8, allocs);
        }
        ref var bufwrt = ref heap(new array<byte>(128), out var Ꮡbufwrt);
        var ch = new channel<bool>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        var bufwrtʗ1 = bufwrt;
        var chʗ1 = ch;
        var errcʗ2 = errc;
        goǃ(() => {
            while (ᐸꟷ(chʗ1)) {
                var (_, errΔ4) = Ꮡserver.ValueSlot.Write(bufwrtʗ1[..]);
                errcʗ2.ᐸꟷ(errΔ4);
            }
        });
        var bufʗ2 = buf;
        var chʗ2 = ch;
        var clientʗ3 = client;
        var errcʗ3 = errc;
        allocs = testing.AllocsPerRun(1000, () => {
            chʗ2.ᐸꟷ(true);
            {
                (_, Ꮡerr.ValueSlot) = Δio.ReadFull(new net_test_package.net_ConnᴠReader(clientʗ3), bufʗ2[..]); if (Ꮡerr.ValueSlot != default!) {
                    Ꮡt.Fatal(Ꮡerr.ValueSlot);
                }
            }
            {
                var errΔ5 = ᐸꟷ(errcʗ3); if (errΔ5 != default!) {
                    Ꮡt.Fatal(errΔ5);
                }
            }
        });
        if (allocs > 0D) {
            Ꮡt.Fatalf("got %v; want 0"u8, allocs);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTCPStress(ж<testing.T> Ꮡt) {
    const nint conns = 2;
    UntypedInt msgLen = 512;
    nint msgs = (nint)10000;
    if (testing.Short()) {
        msgs = 100;
    }
    bool sendMsg(global::go.net_package.Conn c, slice<byte> buf) {
        var (n, errΔ1) = c.Write(buf);
        if (n != len(buf) || errΔ1 != default!) {
            Ꮡt.Log(errΔ1);
            return false;
        }
        return true;
    }
    bool recvMsg(global::go.net_package.Conn c, slice<byte> buf) {
        for (nint read = 0; read != len(buf); ) {
            var (n, errΔ2) = c.Read(buf);
            read += n;
            if (errΔ2 != default!) {
                Ꮡt.Log(errΔ2);
                return false;
            }
        }
        return true;
    }
    var (ln, err) = Listen(tcpˢ, "127.0.0.1:0"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var done = new channel<bool>(0);
    // Acceptor.
    var doneʗ1 = done;
    var lnʗ1 = ln;
    var recvMsgʗ1 = recvMsg;
    var sendMsgʗ1 = sendMsg;
    goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
        GoFrame ᒐ = default;
        try {
            var doneʗ2 = doneʗ1;
            defer(() => {
                doneʗ2.ᐸꟷ(true);
            }, ref ᒐ);
            while (ᐧ) {
                var (c, errΔ3) = lnʗ1.Accept();
                if (errΔ3 != default!) {
                    break;
                }
                // Server connection.
                var recvMsgʗ2 = recvMsgʗ1;
                var sendMsgʗ2 = sendMsgʗ1;
                goǃ((global::go.net_package.Conn cΔ1) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => cΔ1.Close(), ref ᒐ);
                        ref var buf = ref heap(new array<byte>(512), out var Ꮡbuf);
                        for (nint m = 0; m < msgs; m++) {
                            if (!recvMsgʗ2(cΔ1, buf[..]) || !sendMsgʗ2(cΔ1, buf[..])) {
                                break;
                            }
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, c);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    for (nint i = 0; i < conns; i++) {
        // Client connection.
        var doneʗ3 = done;
        var lnʗ2 = ln;
        var recvMsgʗ3 = recvMsg;
        var sendMsgʗ3 = sendMsg;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var doneʗ4 = doneʗ3;
                defer(() => {
                    doneʗ4.ᐸꟷ(true);
                }, ref ᒐ);
                var (c, errΔ4) = Dial(tcpˢ, lnʗ2.Addr().String());
                if (errΔ4 != default!) {
                    Ꮡt.Log(errΔ4);
                    return;
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                ref var buf = ref heap(new array<byte>(512), out var Ꮡbuf);
                for (nint m = 0; m < msgs; m++) {
                    if (!sendMsgʗ3(c, buf[..]) || !recvMsgʗ3(c, buf[..])) {
                        break;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    for (nint i = 0; i < conns; i++) {
        ᐸꟷ(done);
    }
    ln.Close();
    ᐸꟷ(done);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testDisabledUseTcpbigToˢ = (@string)"test disabled; use -tcpbig to enable"u8;

// Test that >32-bit reads work on 64-bit systems.
// On 32-bit systems this tests that maxint reads work.
public static void TestTCPBig(ж<testing.T> Ꮡt) {
    if (!testTCPBig.Value) {
        Ꮡt.Skip(testDisabledUseTcpbigToˢ);
    }
    foreach (var (_, writev) in new bool[]{false, true}.slice()) {
        Ꮡt.Run(fmt.Sprintf("writev=%v"u8, writev), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var ln = newLocalListener(new net_test_package.testing_TжTB(tΔ1), tcpˢ);
                var lnʗ1 = ln;
                defer(() => lnʗ1.Close(), ref ᒐ);
                nint x = (nint)((1 << (int)(30)));
                x = x * 5 + (1 << (int)(20)); // just over 5 GB on 64-bit, just over 1GB on 32-bit
                var done = new channel<nint>(0);
                var doneʗ1 = done;
                var lnʗ2 = ln;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
                        var (cΔ1, errΔ1) = lnʗ2.Accept();
                        if (errΔ1 != default!) {
                            tΔ1.Error(errΔ1);
                            return;
                        }
                        var bufΔ1 = new slice<byte>(x);
                        nint nΔ1 = default!;
                        if (writev){
                            int64 n64 = default!;
                            (n64, errΔ1) = (Ꮡ(new Buffers(new slice<byte>[]{bufΔ1}.slice()))).WriteTo(new net_test_package.net_ConnᴠWriter(cΔ1));
                            nΔ1 = (nint)n64;
                        } else {
                            (nΔ1, errΔ1) = cΔ1.Write(bufΔ1);
                        }
                        if (nΔ1 != len(bufΔ1) || errΔ1 != default!) {
                            tΔ1.Errorf("Write(buf) = %d, %v, want %d, nil"u8, nΔ1, errΔ1, x);
                        }
                        cΔ1.Close();
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var (c, err) = Dial(tcpˢ, ln.Addr().String());
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var buf = new slice<byte>(x);
                (var n, err) = Δio.ReadFull(new net_test_package.net_ConnᴠReader(c), buf);
                if (n != len(buf) || err != default!) {
                    tΔ1.Errorf("Read(buf) = %d, %v, want %d, nil"u8, n, err, x);
                }
                c.Close();
                ᐸꟷ(done);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static partial void TestCopyPipeIntoTCP(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skipf("skipping: os.Pipe not supported on %s"u8, Δruntime.GOOS);
        }

        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        defer(() => {
            {
                var errΔ1 = ᐸꟷ(errcʗ1); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
        }, ref ᒐ);
        var errcʗ2 = errc;
        var lnʗ2 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (cΔ1, errΔ2) = lnʗ2.Accept();
                if (errΔ2 != default!) {
                    errcʗ2.ᐸꟷ(errΔ2);
                    return;
                }
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
                var buf = new slice<byte>(100);
                (var n, errΔ2) = Δio.ReadFull(new net_test_package.net_ConnᴠReader(cΔ1), buf);
                if (!AreEqual(errΔ2, Δio.ErrUnexpectedEOF) || n != 2) {
                    errcʗ2.ᐸꟷ(fmt.Errorf("got err=%q n=%v; want err=%q n=2"u8, errΔ2, n, Δio.ErrUnexpectedEOF));
                    return;
                }
                errcʗ2.ᐸꟷ(default!);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var (c, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ2 = c;
        defer(() => cʗ2.Close(), ref ᒐ);
        (var r, var w, err) = Δos.Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var errc2 = new channel<error>(1);
        var errc2ʗ1 = errc2;
        defer(() => {
            {
                var errΔ3 = ᐸꟷ(errc2ʗ1); if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                }
            }
        }, ref ᒐ);
        var wʗ1 = w;
        defer(() => wʗ1.Close(), ref ᒐ);
        var cʗ3 = c;
        var errc2ʗ2 = errc2;
        var rʗ2 = r;
        goǃ(() => {
            var (_, errΔ4) = Δio.Copy(new net_test_package.net_ConnᴠWriter(cʗ3), new net_test_package.os_FileжReader(rʗ2));
            errc2ʗ2.ᐸꟷ(errΔ4);
        });
        // Split write into 2 packets. That makes Windows TransmitFile
        // drop second packet.
        var packet = new slice<byte>(1);
        (_, err) = w.Write(packet);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        time.Sleep(100 * time.Millisecond);
        (_, err) = w.Write(packet);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void BenchmarkSetReadDeadline(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var ln = newLocalListener(new net_test_package.testing_BжTB(Ꮡb), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        ref var serv = ref heap<global::go.net_package.Conn>(out var Ꮡserv);
        var done = new channel<error>(0);
        var doneʗ1 = done;
        var lnʗ2 = ln;
        goǃ(() => {
            error errΔ1 = default!;
            (Ꮡserv.ValueSlot, errΔ1) = lnʗ2.Accept();
            doneʗ1.ᐸꟷ(errΔ1);
        });
        var (c, err) = Dial(tcpˢ, ln.Addr().String());
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            var errΔ2 = ᐸꟷ(done); if (errΔ2 != default!) {
                Ꮡb.Fatal(errΔ2);
            }
        }
        defer(() => Ꮡserv.ValueSlot.Close(), ref ᒐ);
        c.SetWriteDeadline(time.Now().Add((time.Duration)(7200000000000L)));
        var deadline = time.Now().Add(time.ΔHour);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            c.SetReadDeadline(deadline);
            deadline = deadline.Add(1);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDialTCPDefaultKeepAlive(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var got = ((time.Duration)(-1));
        testHookSetKeepAlive = (global::go.net_package.KeepAliveConfig cfg) => {
            got = cfg.Idle;
        };
        defer(() => {
            testHookSetKeepAlive = (global::go.net_package.KeepAliveConfig _) => {
            };
        }, ref ᒐ);
        var (c, err) = DialTCP(tcpˢ, nil, ln.Addr()._<ж<global::go.net_package.TCPAddr>>());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.of(global::go.net_package.TCPConn.Ꮡconn).Close(), ref ᒐ);
        if (got != 0) {
            Ꮡt.Errorf("got keepalive %v; want %v"u8, got, defaultTCPKeepAliveIdle);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestTCPListenAfterClose(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Regression test for https://go.dev/issue/50216:
        // after calling Close on a Listener, the fake net implementation would
        // erroneously Accept a connection dialed before the call to Close.
        var ln = newLocalListener(new net_test_package.testing_TжTB(Ꮡt), tcpˢ);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        var (ctx, cancel) = context.WithCancel(context.Background());
        var d = Ꮡ(new Dialer(nil));
        for (nint n = 2; n > 0; n--) {
            Ꮡwg.Add(1);
            var ctxʗ1 = ctx;
            var dʗ1 = d;
            var lnʗ2 = ln;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (cΔ1, errΔ1) = dʗ1.DialContext(ctxʗ1, lnʗ2.Addr().Network(), lnʗ2.Addr().String());
                    if (errΔ1 == default!) {
                        ᐸꟷ(ctxʗ1.Done());
                        cΔ1.Close();
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        var (c, err) = ln.Accept();
        if (err == default!){
            c.Close();
        } else {
            Ꮡt.Error(err);
        }
        time.Sleep(10 * time.Millisecond);
        cancel();
        Ꮡwg.Wait();
        ln.Close();
        (c, err) = ln.Accept();
        if (!errors.Is(err, ErrClosed)) {
            if (err == default!) {
                c.Close();
            }
            Ꮡt.Errorf("after l.Close(), l.Accept() = _, %v\nwant %v"u8, err, ErrClosed);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end net_internal_test_package
