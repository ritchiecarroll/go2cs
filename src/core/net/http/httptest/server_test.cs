// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using sync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.net;
using static go.net.http.httptest_package;
using time = time_package;

partial class httptest_internal_test_package {

// type newServerFunc is a methodless func type — rendered inline as its base delegate

// The manual variants of newServer create a Server manually by only filling
// in the exported fields of Server.
internal static map<@string, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>>> newServers;
internal static void initᴛnewServers() { newServers = new map<@string, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>>>{
    ["NewServer"u8] = NewServer,
    ["NewTLSServer"u8] = NewTLSServer,
    ["NewServerManual"u8] = (httpꓸHandler h) => {
        var ts = Ꮡ(new Server(Listener: newLocalListener(), Config: Ꮡ(new http.Server(Handler: h))));
        ts.Start();
        return ts;
    },
    ["NewTLSServerManual"u8] = (httpꓸHandler h) => {
        var ts = Ꮡ(new Server(Listener: newLocalListener(), Config: Ꮡ(new http.Server(Handler: h))));
        ts.StartTLS();
        return ts;
    }
}; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string serverˢ = "Server"u8;
internal static readonly @string getAfterCloseˢ = "GetAfterClose"u8;
internal static readonly @string serverCloseBlockingˢ = "ServerCloseBlocking"u8;
internal static readonly @string serverClientˢ = "ServerClient"u8;

public static void TestServer(ж<testing.T> Ꮡt) {
    foreach (var (_, name) in new @string[]{"NewServer"u8, "NewServerManual"u8}.slice()) {
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var newServer = newServers[name];
            var newServerʗ1 = newServer;
            tΔ1.Run(serverˢ, (ж<testing.T> tΔ2) => {
                testServer(tΔ2, newServerʗ1);
            });
            var newServerʗ2 = newServer;
            tΔ1.Run(getAfterCloseˢ, (ж<testing.T> tΔ3) => {
                testGetAfterClose(tΔ3, newServerʗ2);
            });
            var newServerʗ3 = newServer;
            tΔ1.Run(serverCloseBlockingˢ, (ж<testing.T> tΔ4) => {
                testServerCloseBlocking(tΔ4, newServerʗ3);
            });
            var newServerʗ4 = newServer;
            tΔ1.Run("ServerCloseClientConnections"u8, (ж<testing.T> tΔ5) => {
                testServerCloseClientConnections(tΔ5, newServerʗ4);
            });
            var newServerʗ5 = newServer;
            tΔ1.Run("ServerClientTransportType"u8, (ж<testing.T> tΔ6) => {
                testServerClientTransportType(tΔ6, newServerʗ5);
            });
        });
    }
    foreach (var (_, name) in new @string[]{"NewTLSServer"u8, "NewTLSServerManual"u8}.slice()) {
        Ꮡt.Run(name, (ж<testing.T> tΔ7) => {
            var newServer = newServers[name];
            var newServerʗ6 = newServer;
            tΔ7.Run(serverClientˢ, (ж<testing.T> tΔ8) => {
                testServerClient(tΔ8, newServerʗ6);
            });
            var newServerʗ7 = newServer;
            tΔ7.Run("TLSServerClientTransportType"u8, (ж<testing.T> tΔ9) => {
                testTLSServerClientTransportType(tΔ9, newServerʗ7);
            });
        });
    }
}

internal static void testServer(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newServer) {
    GoFrame ᒐ = default;
    try {
        var ts = newServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("hello"u8));
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (res, err) = http.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var got, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)got) != "hello"u8) {
            Ꮡt.Errorf("got %q, want hello"u8, ((@string)got));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 12781
internal static void testGetAfterClose(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newServer) {
    var ts = newServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
        w.Write(slice<byte>("hello"u8));
    })));
    var (res, err) = http.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var got, err) = io.ReadAll((~res).Body);
    (~res).Body.Close();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((sstring)got) != "hello"u8) {
        Ꮡt.Fatalf("got %q, want hello"u8, ((@string)got));
    }
    ts.Close();
    (res, err) = http.Get((~ts).URL);
    if (err == default!) {
        var (body, _) = io.ReadAll((~res).Body);
        Ꮡt.Fatalf("Unexpected response after close: %v, %v, %s"u8, (~res).Status, (~res).Header, body);
    }
}

internal static void testServerCloseBlocking(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newServer) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var ts = newServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("hello"u8));
        })));
        var tsʗ1 = ts;
        net.Conn dial() {
            var (c, errΔ1) = net.Dial(tcpˢ, (~tsʗ1).Listener.Addr().String());
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            return c;
        }
        // Keep one connection in StateNew (connected, but not sending anything)
        var cnew = dial();
        var cnewʗ1 = cnew;
        defer(() => cnewʗ1.Close(), ref ᒐ);
        // Keep one connection in StateIdle (idle after a request)
        var cidle = dial();
        var cidleʗ1 = cidle;
        defer(() => cidleʗ1.Close(), ref ᒐ);
        cidle.Write(slice<byte>("HEAD / HTTP/1.1\r\nHost: foo\r\n\r\n"u8));
        var (_, err) = http.ReadResponse(bufio.NewReader(new httptest_test_package.net_ConnᴠReader(cidle)), nil);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ts.Close(); // test we don't hang here forever.
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 14290
internal static void testServerCloseClientConnections(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newServer) {
    GoFrame ᒐ = default;
    try {
        ref var s = ref heap<ж<global::go.net.http.httptest_package.Server>>(out var Ꮡs);
        s = newServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (http.ResponseWriter w, ж<http.Request> r) => {
            Ꮡs.ValueSlot.CloseClientConnections();
        })));
        defer(Ꮡs.ValueSlot.Close, ref ᒐ);
        var (res, err) = http.Get((~s).URL);
        if (err == default!) {
            (~res).Body.Close();
            Ꮡt.Fatalf("Unexpected response: %#v"u8, res.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that the Server.Client method works and returns an http.Client that can hit
// NewTLSServer without cert warnings.
internal static void testServerClient(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newTLSServer) {
    GoFrame ᒐ = default;
    try {
        var ts = newTLSServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("hello"u8));
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var client = ts.Client();
        var (res, err) = client.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var got, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)got) != "hello"u8) {
            Ꮡt.Errorf("got %q, want hello"u8, ((@string)got));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that the Server.Client.Transport interface is implemented
// by a *http.Transport.
internal static void testServerClientTransportType(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newServer) {
    GoFrame ᒐ = default;
    try {
        var ts = newServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var client = ts.Client();
        {
            var (_, ok) = (~client).Transport._<ж<http.Transport>>(ᐧ); if (!ok) {
                Ꮡt.Errorf("got %T, want *http.Transport"u8, (~client).Transport);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that the TLS Server.Client.Transport interface is implemented
// by a *http.Transport.
internal static void testTLSServerClientTransportType(ж<testing.T> Ꮡt, Func<httpꓸHandler, ж<global::go.net.http.httptest_package.Server>> newTLSServer) {
    GoFrame ᒐ = default;
    try {
        var ts = newTLSServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var client = ts.Client();
        {
            var (_, ok) = (~client).Transport._<ж<http.Transport>>(ᐧ); if (!ok) {
                Ꮡt.Errorf("got %T, want *http.Transport"u8, (~client).Transport);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct onlyCloseListener {
    /*embed*/ public net_package.Listener Listener;
}

internal static error Close(this onlyCloseListener _) {
    return default!;
}

// Issue 19729: panic in Server.Close for values created directly
// without a constructor (so the unexported client field is nil).
public static void TestServerZeroValueClose(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = Ꮡ(new Server(
        Listener: new onlyCloseListener(nil),
        Config: Ꮡ(new http.Server(nil))
    ));
    ts.Close(); // tests that it doesn't panic
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToHijackˢ = (@string)"failed to hijack"u8;

// Issue 51799: test hijacking a connection and then closing it
// concurrently with closing the server.
public static partial void TestCloseHijackedConnection(ж<testing.T> Ꮡt) {
    var hijacked = new channel<net.Conn>(0);
    var hijackedʗ1 = hijacked;
    var ts = NewServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), hijackedʗ1, ref ᒐ);
            var (hj, ok) = w._<http.Hijacker>(ᐧ);
            if (!ok) {
                Ꮡt.Fatal(failedToHijackˢ);
            }
            var (c, _, err) = hj.Hijack();
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            hijackedʗ1.ᐸꟷ(c);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })));
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var tsʗ1 = ts;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var (req, err) = http.NewRequest(getˢ, (~tsʗ1).URL, default!);
            if (err != default!) {
                Ꮡt.Log(err);
            }
            // Use a client not associated with the Server.
            ref var c = ref heap(new http.Client(), out var Ꮡc);
            (var resp, err) = Ꮡc.Do(req);
            if (err != default!) {
                Ꮡt.Log(err);
                return;
            }
            (~resp).Body.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Add(1);
    var conn = ᐸꟷ(hijacked);
    var tsʗ2 = ts;
    goǃ((net.Conn connΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            // Close the connection and then inform the Server that
            // we closed it.
            connΔ1.Close();
            (~(~tsʗ2).Config).ConnState(connΔ1, http.StateClosed);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }, conn);
    Ꮡwg.Add(1);
    var tsʗ3 = ts;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            tsʗ3.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xProtoˢ = "X-Proto"u8;

internal partial struct TestTLSServerWithHTTP2_modes /*dyn*/ {
    internal @string name;
    internal @string wantProto;
}

public static void TestTLSServerWithHTTP2(ж<testing.T> Ꮡt) {
    var modes = new TestTLSServerWithHTTP2_modes[]{
        new("http1"u8, "HTTP/1.1"u8),
        new("http2"u8, "HTTP/2.0"u8)
    }.slice();
    foreach (var (_, vᴛ1) in modes) {
        ref var tt = ref heap(new TestTLSServerWithHTTP2_modes(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var cst = NewUnstartedServer(new httptest_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
                    w.Header().Set(xProtoˢ, (~r).Proto);
                })));
                var exprᴛ1 = ttʗ1.name;
                if (exprᴛ1 == "http2"u8) {
                    cst.Value.EnableHTTP2 = true;
                    cst.StartTLS();
                }
                else { /* default: */
                    cst.Start();
                }

                var cstʗ1 = cst;
                defer(cstʗ1.Close, ref ᒐ);
                var (res, err) = cst.Client().Get((~cst).URL);
                if (err != default!) {
                    tΔ1.Fatalf("Failed to make request: %v"u8, err);
                }
                {
                    @string g = (~res).Header.Get(xProtoˢ);
                    @string w = ttʗ1.wantProto; if (g != w) {
                        tΔ1.Fatalf("X-Proto header mismatch:\n\tgot:  %q\n\twant: %q"u8, g, w);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end httptest_internal_test_package
