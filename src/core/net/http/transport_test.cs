// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests for transport.go.
//
// More tests are in clientserver_test.go (for things testing both client & server for both
// HTTP/1 and HTTP/2). This
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using gzip = compress.gzip_package;
using context = context_package;
using rand = crypto.rand_package;
using tls = crypto.tls_package;
using x509 = crypto.x509_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using token = global::go.go.token_package;
using nettrace = global::go.@internal.nettrace_package;
using synctest = global::go.@internal.synctest_package;
using io = io_package;
using log = log_package;
using mrand = global::go.math.rand_package;
using net = net_package;
using static global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using httptrace = global::go.net.http.httptrace_package;
using httputil = global::go.net.http.httputil_package;
using testcert = global::go.net.http.@internal.testcert_package;
using textproto = global::go.net.textproto_package;
using url = global::go.net.url_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using iotest = global::go.testing.iotest_package;
using time = time_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using compress;
using crypto;
using encoding;
using global::go.@internal;
using global::go.go;
using global::go.net;
using global::go.net.http;
using global::go.net.http.@internal;
using global::go.sync;
using global::go.testing;
using static global::go.net.http_internal_test_package;
using vendor.golang.org.x.net.http;
using Δhttp = global::go.net.http_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸstring = Span<@string>;

partial class http_test_package {

// TODO: test 5 pipelined requests with responses: 1) OK, 2) OK, Connection: Close
// and then verify that the final 2 responses get errors back.
// Include the address of the net.Conn in addition to the RemoteAddr,
// in case kernels reuse source ports quickly (see Issue 52450)
// hostPortHandler writes back the client's "host:port".
internal static Δhttp.HandlerFunc hostPortHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    if (r.FormValue("close"u8) == "true"u8) {
        w.Header().Set("Connection"u8, "close"u8);
    }
    w.Header().Set("X-Saw-Close"u8, fmt.Sprint((~r).Close));
    w.Write(slice<byte>((~r).RemoteAddr));
    {
        var (c, ok) = http_internal_test_package.ResponseWriterConnForTesting(w); if (ok) {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), ", %T %p"u8, c, c);
        }
    }
});

// testCloseConn is a net.Conn tracked by a testConnSet.
[GoType] partial struct testCloseConn {
    [GoEmbedded] public net_package.Conn Conn;
    internal ж<testConnSet> set;
}

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this testCloseConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this testCloseConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this testCloseConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this testCloseConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this testCloseConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this testCloseConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

// Go method set entry for the promoted 'Conn.Write()' - provided ONLY by the embedded
// interface field in *testCloseConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Write(this testCloseConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Write(b);

internal static error Close(this ж<testCloseConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.set.remove(new http_test_package.testCloseConnжConn(Ꮡc));
    return c.Conn.Close();
}

// testConnSet tracks a set of TCP connections and whether they've
// been closed.
[GoType] partial struct testConnSet {
    internal ж<testing.T> t;
    internal sync.Mutex mu; // guards closed and list
    internal map<net.Conn, bool> closed;
    internal slice<net.Conn> list; // in order created
}

internal static void insert(this ж<testConnSet> Ꮡtcs, net.Conn c) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var tcs = ref Ꮡtcs.DerefOrNull();

        tcs.mu.Lock();
        ᒐd1 = true;
        tcs.closed[c] = false;
        tcs.list = append(tcs.list, c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡtcs.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void remove(this ж<testConnSet> Ꮡtcs, net.Conn c) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var tcs = ref Ꮡtcs.DerefOrNull();

        tcs.mu.Lock();
        ᒐd1 = true;
        tcs.closed[c] = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡtcs.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// some tests use this to manage raw tcp connections for later inspection
internal static (ж<testConnSet>, Func<@string, @string, (net.Conn, error)>) makeTestDial(ж<testing.T> Ꮡt) {
    var connSet = Ꮡ(new testConnSet(
        t: Ꮡt,
        closed: new map<net.Conn, bool>()
    ));
    var connSetʗ1 = connSet;
    var dial = (net.Conn, error) (@string n, @string addr) => {
        var (c, err) = net.Dial(n, addr);
        if (err != default!) {
            return (default!, err);
        }
        var tc = Ꮡ(new testCloseConn(c, connSetʗ1));
        connSetʗ1.insert(new http_test_package.testCloseConnжConn(tc));
        return (new http_test_package.testCloseConnжConn(tc), default!);
    };
    return (connSet, dial);
}

internal static void check(this ж<testConnSet> Ꮡtcs, ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var tcs = ref Ꮡtcs.DerefOrNull();

        tcs.mu.Lock();
        ᒐd1 = true;
        for (nint i = 4; i >= 0; i--) {
            foreach (var (iΔ1, c) in tcs.list) {
                if (tcs.closed[c]) {
                    continue;
                }
                if (iΔ1 != 0) {
                    // TODO(bcmills): What is the Sleep here doing, and why is this
                    // Unlock/Sleep/Lock cycle needed at all?
                    tcs.mu.Unlock();
                    time.Sleep(50 * time.Millisecond);
                    tcs.mu.Lock();
                    continue;
                }
                Ꮡt.Errorf("TCP connection #%d, %p (of %d total) was not closed"u8, iΔ1 + 1, c, len(tcs.list));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡtcs.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

public static void TestReuseRequest(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testReuseRequest(Δp0, Δp1));
}

internal static void testReuseRequest(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("{}"u8));
    }))).Value.ts;
    var c = ts.Client();
    var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
    var (res, err) = c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = (~res).Body.Close();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (res, err) = c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = (~res).Body.Close();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

// Two subsequent requests and verify their response is the same.
// The response from the server is our own IP:port
public static void TestTransportKeepAlives(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportKeepAlives(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportKeepAlives(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
    var c = ts.Client();
    foreach (var (_, disableKeepAlive) in new bool[]{false, true}.slice()) {
        (~c).Transport._<ж<Δhttp.Transport>>().Value.DisableKeepAlives = disableKeepAlive;
        var cʗ1 = c;
        var tsʗ1 = ts;
        @string fetch(nint n) {
            var (res, err) = cʗ1.Get((~tsʗ1).URL);
            if (err != default!) {
                Ꮡt.Fatalf("error in disableKeepAlive=%v, req #%d, GET: %v"u8, disableKeepAlive, n, err);
            }
            (var body, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Fatalf("error in disableKeepAlive=%v, req #%d, ReadAll: %v"u8, disableKeepAlive, n, err);
            }
            return ((@string)body);
        }
        @string body1 = fetch(1);
        @string body2 = fetch(2);
        var bodiesDiffer = body1 != body2;
        if (bodiesDiffer != disableKeepAlive) {
            Ꮡt.Errorf("error in disableKeepAlive=%v. unexpected bodiesDiffer=%v; body1=%q; body2=%q"u8,
                disableKeepAlive, bodiesDiffer, body1, body2);
        }
    }
}

public static void TestTransportConnectionCloseOnResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportConnectionCloseOnResponse(Δp0, Δp1));
}

internal static void testTransportConnectionCloseOnResponse(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
    var (connSet, testDial) = makeTestDial(Ꮡt);
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    tr.Value.Dial = testDial;
    foreach (var (_, connectionClose) in new bool[]{false, true}.slice()) {
        var cʗ1 = c;
        var tsʗ1 = ts;
        @string fetch(nint n) {
            GoFrame ᒐ = default;
            try {
                var req = @new<Δhttp.Request>();
                error err = default!;
                (req.Value.URL, err) = url.Parse((~tsʗ1).URL + fmt.Sprintf("/?close=%v"u8, connectionClose));
                if (err != default!) {
                    Ꮡt.Fatalf("URL parse error: %v"u8, err);
                }
                req.Value.Method = getˢ2;
                req.Value.Proto = http11ˢ;
                req.Value.ProtoMajor = 1;
                req.Value.ProtoMinor = 1;
                (var res, err) = cʗ1.Do(req);
                if (err != default!) {
                    Ꮡt.Fatalf("error in connectionClose=%v, req #%d, Do: %v"u8, connectionClose, n, err);
                }
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                (var body, err) = io.ReadAll((~res).Body);
                if (err != default!) {
                    Ꮡt.Fatalf("error in connectionClose=%v, req #%d, ReadAll: %v"u8, connectionClose, n, err);
                }
                return ((@string)body);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        @string body1 = fetch(1);
        @string body2 = fetch(2);
        var bodiesDiffer = body1 != body2;
        if (bodiesDiffer != connectionClose) {
            Ꮡt.Errorf("error in connectionClose=%v. unexpected bodiesDiffer=%v; body1=%q; body2=%q"u8,
                connectionClose, bodiesDiffer, body1, body2);
        }
        tr.CloseIdleConnections();
    }
    connSet.check(Ꮡt);
}

// TestTransportConnectionCloseOnRequest tests that the Transport's doesn't reuse
// an underlying TCP connection after making an http.Request with Request.Close set.
//
// It tests the behavior by making an HTTP request to a server which
// describes the source connection it got (remote port number +
// address of its net.Conn).
public static void TestTransportConnectionCloseOnRequest(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportConnectionCloseOnRequest(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xSawCloseˢ = "X-Saw-Close"u8;

internal static void testTransportConnectionCloseOnRequest(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
    var (connSet, testDial) = makeTestDial(Ꮡt);
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    tr.Value.Dial = testDial;
    foreach (var (_, reqClose) in new bool[]{false, true}.slice()) {
        var cʗ1 = c;
        var tsʗ1 = ts;
        @string fetch(nint n) {
            var req = @new<Δhttp.Request>();
            error err = default!;
            (req.Value.URL, err) = url.Parse((~tsʗ1).URL);
            if (err != default!) {
                Ꮡt.Fatalf("URL parse error: %v"u8, err);
            }
            req.Value.Method = getˢ2;
            req.Value.Proto = http11ˢ;
            req.Value.ProtoMajor = 1;
            req.Value.ProtoMinor = 1;
            req.Value.Close = reqClose;
            (var res, err) = cʗ1.Do(req);
            if (err != default!) {
                Ꮡt.Fatalf("error in Request.Close=%v, req #%d, Do: %v"u8, reqClose, n, err);
            }
            {
                @string gotΔ1 = (~res).Header.Get(xSawCloseˢ);
                @string wantΔ1 = fmt.Sprint(reqClose); if (gotΔ1 != wantΔ1) {
                    Ꮡt.Errorf("for Request.Close = %v; handler's X-Saw-Close was %v; want %v"u8,
                        reqClose, gotΔ1, !reqClose);
                }
            }
            (var body, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Fatalf("for Request.Close=%v, on request %v/2: ReadAll: %v"u8, reqClose, n, err);
            }
            return ((@string)body);
        }
        @string body1 = fetch(1);
        @string body2 = fetch(2);
        nint got = 1;
        if (body1 != body2) {
            got++;
        }
        nint want = 1;
        if (reqClose) {
            want = 2;
        }
        if (got != want) {
            Ꮡt.Errorf("for Request.Close=%v: server saw %v unique connections, wanted %v\n\nbodies were: %q and %q"u8,
                reqClose, got, want, body1, body2);
        }
        tr.CloseIdleConnections();
    }
    connSet.check(Ꮡt);
}

// if the Transport's DisableKeepAlives is set, all requests should
// send Connection: close.
// HTTP/1-only (Connection: close doesn't exist in h2)
public static void TestTransportConnectionCloseOnRequestDisableKeepAlive(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportConnectionCloseOnRequestDisableKeepAlive(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportConnectionCloseOnRequestDisableKeepAlive(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.DisableKeepAlives = true;
    var (res, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    if ((~res).Header.Get(xSawCloseˢ) != "true"u8) {
        Ꮡt.Errorf("handler didn't see Connection: close "u8);
    }
}

// Test that Transport only sends one "Connection: close", regardless of
// how "close" was indicated.
public static void TestTransportRespectRequestWantsClose(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportRespectRequestWantsClose(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

[GoType("dyn")] internal partial struct testTransportRespectRequestWantsClose_tests {
    internal bool disableKeepAlives;
    internal bool close;
}

internal static void testTransportRespectRequestWantsClose(ж<testing.T> Ꮡt, testMode mode) {
    var tests = new testTransportRespectRequestWantsClose_tests[]{
        new(disableKeepAlives: false, close: false),
        new(disableKeepAlives: false, close: true),
        new(disableKeepAlives: true, close: false),
        new(disableKeepAlives: true, close: true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new testTransportRespectRequestWantsClose_tests(), out var Ꮡtc);
        tc = vᴛ1;

            var tcʗ1 = tc;
        Ꮡt.Run(fmt.Sprintf("DisableKeepAlive=%v,RequestClose=%v"u8, tc.disableKeepAlives, tc.close),
            (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
                    var c = ts.Client();
                    (~c).Transport._<ж<Δhttp.Transport>>().Value.DisableKeepAlives = tcʗ1.disableKeepAlives;
                    var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
                    if (err != default!) {
                        tΔ1.Fatal(err);
                    }
                    nint count = 0;
                    var trace = Ꮡ(new httptrace.ClientTrace(
                        WroteHeaderField: (@string key, slice<@string> field) => {
                            if (key != "Connection"u8) {
                                return;
                            }
                            if (httpguts.HeaderValuesContainsToken(field, closeˢ)) {
                                count += 1;
                            }
                        }
                    ));
                    req = req.WithContext(httptrace.WithClientTrace(req.Context(), trace));
                    req.Value.Close = tcʗ1.close;
                    (var res, err) = c.Do(req);
                    if (err != default!) {
                        tΔ1.Fatal(err);
                    }
                    var resʗ1 = res;
                    defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                    {
                        var want = tcʗ1.disableKeepAlives || tcʗ1.close; if (count > 1 || (count == 1) != want) {
                            tΔ1.Errorf("expecting want:%v, got 'Connection: close':%d"u8, want, count);
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
    }
}

public static void TestTransportIdleCacheKeys(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIdleCacheKeys(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportIdleCacheKeys(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    {
        nint e = 0;
        nint g = len(tr.IdleConnKeysForTesting()); if (e != g) {
            Ꮡt.Errorf("After CloseIdleConnections expected %d idle conn cache keys; got %d"u8, e, g);
        }
    }
    var (resp, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    io.ReadAll((~resp).Body);
    var keys = tr.IdleConnKeysForTesting();
    {
        nint e = 1;
        nint g = len(keys); if (e != g) {
            Ꮡt.Fatalf("After Get expected %d idle conn cache keys; got %d"u8, e, g);
        }
    }
    {
        @string e = "|http|"u8 + (~ts).Listener.Addr().String(); if (keys[0] != e) {
            Ꮡt.Errorf("Expected idle cache key %q; got %q"u8, e, keys[0]);
        }
    }
    tr.CloseIdleConnections();
    {
        nint e = 0;
        nint g = len(tr.IdleConnKeysForTesting()); if (e != g) {
            Ꮡt.Errorf("After CloseIdleConnections expected %d idle conn cache keys; got %d"u8, e, g);
        }
    }
}

// Tests that the HTTP transport re-uses connections when a client
// reads to the end of a response Body without closing it.
public static void TestTransportReadToEndReusesConn(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportReadToEndReusesConn(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string msgᶜ1 = "foobar"u8;

internal static void testTransportReadToEndReusesConn(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string msg = msgᶜ1;
        ref var addrSeen = ref heap<map<@string, nint>>(out var ᏑaddrSeen);
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ᏑaddrSeen.ValueSlot[(~r).RemoteAddr]++;
            if ((~(~r).URL).Path == "/chunked/"u8){
                w.WriteHeader(200);
                w._<Flusher>().Flush();
            } else {
                w.Header().Set(contentLengthˢ, strconv.Itoa(len(msg)));
                w.WriteHeader(200);
            }
            w.Write(slice<byte>(msg));
        }))).Value.ts;
        foreach (var (pi, path) in new @string[]{"/content-length/"u8, "/chunked/"u8}.slice()) {
            nint wantLen = new nint[]{len(msg), -1}.slice()[pi];
            addrSeen = new map<@string, nint>();
            for (nint i = 0; i < 3; i++) {
                var (res, err) = ts.Client().Get((~ts).URL + path);
                if (err != default!) {
                    Ꮡt.Errorf("Get %s: %v"u8, path, err);
                    continue;
                }
                // We want to close this body eventually (before the
                // defer afterTest at top runs), but not before the
                // len(addrSeen) check at the bottom of this test,
                // since Closing this early in the loop would risk
                // making connections be re-used for the wrong reason.
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                if ((~res).ContentLength != (int64)wantLen) {
                    Ꮡt.Errorf("%s res.ContentLength = %d; want %d"u8, path, (~res).ContentLength, wantLen);
                }
                (var got, err) = io.ReadAll((~res).Body);
                if (((sstring)got) != msg || err != default!) {
                    Ꮡt.Errorf("%s ReadAll(Body) = %q, %v; want %q, nil"u8, path, ((@string)got), err, msg);
                }
            }
            if (len(addrSeen) != 1) {
                Ꮡt.Errorf("for %s, server saw %d distinct client addresses; want 1"u8, path, len(addrSeen));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportMaxPerHostIdleConns(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportMaxPerHostIdleConns(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string res1ˢ = "res1"u8;
internal static readonly @string res2ˢ = "res2"u8;
internal static readonly @string res3ˢ = "res3"u8;

internal static partial void testTransportMaxPerHostIdleConns(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var stop = new channel<EmptyStruct>(0); // stop marks the exit of main Test goroutine
        defer(ᴛ1 => builtin.close(ᴛ1), stop, ref ᒐ);
        var resch = new channel<@string>(0);
        var gotReq = new channel<bool>(0);
        var gotReqʗ1 = gotReq;
        var reschʗ1 = resch;
        var stopʗ1 = stop;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            gotReqʗ1.ᐸꟷ(true);
            @string msg = default!;
            var selᴛ62 = stopʗ1;
            var selᴛ63 = reschʗ1;
            switch (select(ᐸꟷ(selᴛ62, ꓸꓸꓸ), ᐸꟷ(selᴛ63, ꓸꓸꓸ))) {
            case 0 when selᴛ62.ꟷᐳ(out _): {
                return;
            }
            case 1 when selᴛ63.ꟷᐳ(out msg): {
                break;
            }}
            var (_, err) = w.Write(slice<byte>(msg));
            if (err != default!) {
                Ꮡt.Errorf("Write: %v"u8, err);
                return;
            }
        }))).Value.ts;
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        nint maxIdleConnsPerHost = 2;
        tr.Value.MaxIdleConnsPerHost = maxIdleConnsPerHost;
        // Start 3 outstanding requests and wait for the server to get them.
        // Their responses will hang until we write to resch, though.
        var donech = new channel<bool>(0);
        var cʗ1 = c;
        var donechʗ1 = donech;
        var stopʗ2 = stop;
        var tsʗ1 = ts;
        void doReq() {
            GoFrame ᒐ = default;
            try {
                var donechʗ2 = donechʗ1;
                var stopʗ3 = stopʗ2;
                defer(() => {
                    var selᴛ64 = stopʗ3;
                    var selᴛ65 = donechʗ2.ᐸꟷ(Ꮡt.Failed(), ꓸꓸꓸ);
                    switch (select(ᐸꟷ(selᴛ64, ꓸꓸꓸ), selᴛ65)) {
                    case 0 when selᴛ64.ꟷᐳ(out _): {
                        return;
                    }
                    case 1: {
                        break;
                    }}
                }, ref ᒐ);
                var (resp, err) = cʗ1.Get((~tsʗ1).URL);
                if (err != default!) {
                    Ꮡt.Error(err);
                    return;
                }
                {
                    var (_, errΔ1) = io.ReadAll((~resp).Body); if (errΔ1 != default!) {
                        Ꮡt.Errorf("ReadAll: %v"u8, errΔ1);
                        return;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        var doReqʗ1 = doReq;
        goǃ(doReqʗ1);
        ᐸꟷ(gotReq);
        var doReqʗ2 = doReq;
        goǃ(doReqʗ2);
        ᐸꟷ(gotReq);
        var doReqʗ3 = doReq;
        goǃ(doReqʗ3);
        ᐸꟷ(gotReq);
        {
            nint e = 0;
            nint g = len(tr.IdleConnKeysForTesting()); if (e != g) {
                Ꮡt.Fatalf("Before writes, expected %d idle conn cache keys; got %d"u8, e, g);
            }
        }
        resch.ᐸꟷ(res1ˢ);
        ᐸꟷ(donech);
        var keys = tr.IdleConnKeysForTesting();
        {
            nint e = 1;
            nint g = len(keys); if (e != g) {
                Ꮡt.Fatalf("after first response, expected %d idle conn cache keys; got %d"u8, e, g);
            }
        }
        @string addr = (~ts).Listener.Addr().String();
        @string cacheKey = "|http|"u8 + addr;
        if (keys[0] != cacheKey) {
            Ꮡt.Fatalf("Expected idle cache key %q; got %q"u8, cacheKey, keys[0]);
        }
        {
            nint e = 1;
            nint g = tr.IdleConnCountForTesting(httpˢ, addr); if (e != g) {
                Ꮡt.Errorf("after first response, expected %d idle conns; got %d"u8, e, g);
            }
        }
        resch.ᐸꟷ(res2ˢ);
        ᐸꟷ(donech);
        {
            nint g = tr.IdleConnCountForTesting(httpˢ, addr);
            nint w = 2; if (g != w) {
                Ꮡt.Errorf("after second response, idle conns = %d; want %d"u8, g, w);
            }
        }
        resch.ᐸꟷ(res3ˢ);
        ᐸꟷ(donech);
        {
            nint g = tr.IdleConnCountForTesting(httpˢ, addr);
            nint w = maxIdleConnsPerHost; if (g != w) {
                Ꮡt.Errorf("after third response, idle conns = %d; want %d"u8, g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportMaxConnsPerHostIncludeDialInProgress(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportMaxConnsPerHostIncludeDialInProgress(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string req1ˢ2 = "req1"u8;
internal static readonly @string req2ˢ2 = "req2"u8;
internal static readonly object req2DialStartedWhileReq1ˢ = (@string)"req2 dial started while req1 dial in progress"u8;

internal static partial void testTransportMaxConnsPerHostIncludeDialInProgress(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (_, err) = w.Write(slice<byte>("foo"u8));
        if (err != default!) {
            Ꮡt.Fatalf("Write: %v"u8, err);
        }
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    var dialStarted = new channel<EmptyStruct>(0);
    var stallDial = new channel<EmptyStruct>(0);
    var dialStartedʗ1 = dialStarted;
    var stallDialʗ1 = stallDial;
    tr.Value.Dial = (@string network, @string addr) => {
        dialStartedʗ1.ᐸꟷ(new EmptyStruct());
        ᐸꟷ(stallDialʗ1);
        return net.Dial(network, addr);
    };
    tr.Value.DisableKeepAlives = true;
    tr.Value.MaxConnsPerHost = 1;
    var preDial = new channel<EmptyStruct>(0);
    var reqComplete = new channel<EmptyStruct>(0);
    var preDialʗ1 = preDial;
    var reqCompleteʗ1 = reqComplete;
    var trʗ1 = tr;
    var tsʗ1 = ts;
    void doReq(@string reqId) {
        var (req, _) = NewRequest(getˢ2, (~tsʗ1).URL, default!);
            var preDialʗ2 = preDialʗ1;
        var trace = Ꮡ(new httptrace.ClientTrace(
            GetConn: (@string hostPort) => {
                preDialʗ2.ᐸꟷ(new EmptyStruct());
            }
        ));
        req = req.WithContext(httptrace.WithClientTrace(req.Context(), trace));
        var (resp, err) = trʗ1.RoundTrip(req);
        if (err != default!) {
            Ꮡt.Errorf("unexpected error for request %s: %v"u8, reqId, err);
        }
        (_, err) = io.ReadAll((~resp).Body);
        if (err != default!) {
            Ꮡt.Errorf("unexpected error for request %s: %v"u8, reqId, err);
        }
        reqCompleteʗ1.ᐸꟷ(new EmptyStruct());
    }
    // get req1 to dial-in-progress
    var doReqʗ1 = doReq;
    goǃ(doReqʗ1, req1ˢ2);
    ᐸꟷ(preDial);
    ᐸꟷ(dialStarted);
    // get req2 to waiting on conns per host to go down below max
    var doReqʗ2 = doReq;
    goǃ(doReqʗ2, req2ˢ2);
    ᐸꟷ(preDial);
    var selᴛ66 = dialStarted;
    switch (trySelect(ᐸꟷ(selᴛ66, ꓸꓸꓸ))) {
    case 0 when selᴛ66.ꟷᐳ(out _): {
        Ꮡt.Error(req2DialStartedWhileReq1ˢ);
        return;
    }
    default: {
        break;
    }}
    // let req1 complete
    stallDial.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(reqComplete);
    // let req2 complete
    ᐸꟷ(dialStarted);
    stallDial.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(reqComplete);
}

public static void TestTransportMaxConnsPerHost(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportMaxConnsPerHost(Δp0, Δp1), new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
}

internal static partial void testTransportMaxConnsPerHost(ж<testing.T> Ꮡt, testMode mode) {
    http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
    var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (_, err) = w.Write(slice<byte>("foo"u8));
        if (err != default!) {
            Ꮡt.Fatalf("Write: %v"u8, err);
        }
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(h)).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    tr.Value.MaxConnsPerHost = 1;
    ref var mu = ref heap<sync.Mutex>(out var Ꮡmu);
    mu = new sync.Mutex(nil);
    ref var conns = ref heap<slice<net.Conn>>(out var Ꮡconns);
    ref var dialCnt = ref heap(new int32(), out var ᏑdialCnt);
    ref var gotConnCnt = ref heap(new int32(), out var ᏑgotConnCnt);
    ref var tlsHandshakeCnt = ref heap(new int32(), out var ᏑtlsHandshakeCnt);
    tr.Value.Dial = (@string network, @string addr) => {
        GoFrame ᒐ = default;
        try {
            atomic.AddInt32(ᏑdialCnt, 1);
            var (cΔ1, err) = net.Dial(network, addr);
            Ꮡmu.Lock();
            defer(Ꮡmu.Unlock, ref ᒐ);
            Ꮡconns.ValueSlot = append(Ꮡconns.ValueSlot, cΔ1);
            return (cΔ1, err);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    };
    var cʗ1 = c;
    var tsʗ1 = ts;
    void doReq() {
        GoFrame ᒐ = default;
        try {

            var trace = Ꮡ(new httptrace.ClientTrace(
                GotConn: (httptrace.GotConnInfo connInfo) => {
                    if (!connInfo.Reused) {
                        atomic.AddInt32(ᏑgotConnCnt, 1);
                    }
                },
                TLSHandshakeStart: () => {
                    atomic.AddInt32(ᏑtlsHandshakeCnt, 1);
                }
            ));
            var (req, _) = NewRequest(getˢ2, (~tsʗ1).URL, default!);
            req = req.WithContext(httptrace.WithClientTrace(req.Context(), trace));
            var (resp, err) = cʗ1.Do(req);
            if (err != default!) {
                Ꮡt.Fatalf("request failed: %v"u8, err);
            }
            var respʗ1 = resp;
            defer(() => (~respʗ1).Body.Close(), ref ᒐ);
            (_, err) = io.ReadAll((~resp).Body);
            if (err != default!) {
                Ꮡt.Fatalf("read body failed: %v"u8, err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    ref var wg = ref heap<sync.WaitGroup>(out var Ꮡwg);
    wg = new sync.WaitGroup(nil);
    for (nint i = 0; i < 10; i++) {
        Ꮡwg.Add(1);
        var doReqʗ1 = doReq;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                doReqʗ1();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    var expected = (int32)(~tr).MaxConnsPerHost;
    if (dialCnt != expected) {
        Ꮡt.Errorf("round 1: too many dials: %d != %d"u8, dialCnt, expected);
    }
    if (gotConnCnt != expected) {
        Ꮡt.Errorf("round 1: too many get connections: %d != %d"u8, gotConnCnt, expected);
    }
    if ((~ts).TLS != nil && tlsHandshakeCnt != expected) {
        Ꮡt.Errorf("round 1: too many tls handshakes: %d != %d"u8, tlsHandshakeCnt, expected);
    }
    if (Ꮡt.Failed()) {
        Ꮡt.FailNow();
    }
    Ꮡmu.Lock();
    foreach (var (_, cΔ2) in conns) {
        cΔ2.Close();
    }
    conns = default!;
    Ꮡmu.Unlock();
    tr.CloseIdleConnections();
    doReq();
    expected++;
    if (dialCnt != expected) {
        Ꮡt.Errorf("round 2: too many dials: %d"u8, dialCnt);
    }
    if (gotConnCnt != expected) {
        Ꮡt.Errorf("round 2: too many get connections: %d != %d"u8, gotConnCnt, expected);
    }
    if ((~ts).TLS != nil && tlsHandshakeCnt != expected) {
        Ꮡt.Errorf("round 2: too many tls handshakes: %d != %d"u8, tlsHandshakeCnt, expected);
    }
}

public static void TestTransportMaxConnsPerHostDialCancellation(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportMaxConnsPerHostDialCancellation(Δp0, Δp1),
        testNotParallel, // because test uses SetPendingDialHooks

        new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
}

internal static void testTransportMaxConnsPerHostDialCancellation(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
        var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (_, errΔ1) = w.Write(slice<byte>("foo"u8));
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("Write: %v"u8, errΔ1);
            }
        });
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(h));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var ts = cst.Value.ts;
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        tr.Value.MaxConnsPerHost = 1;
        // This request is canceled when dial is queued, which preempts dialing.
        var (ctx, cancel) = context_package.WithCancel(context_package.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        http_internal_test_package.SetPendingDialHooks(cancel, default!);
        defer(http_internal_test_package.SetPendingDialHooks, (Action)(default!), (Action)(default!), ref ᒐ);
        var (req, _) = NewRequestWithContext(ctx, getˢ2, (~ts).URL, default!);
        var (_, err) = c.Do(req);
        if (!errors.Is(err, context_package.Canceled)) {
            Ꮡt.Errorf("expected error %v, got %v"u8, context_package.Canceled, err);
        }
        // This request should succeed.
        http_internal_test_package.SetPendingDialHooks(default!, default!);
        (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
        (var resp, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("request failed: %v"u8, err);
        }
        var respʗ1 = resp;
        defer(() => (~respʗ1).Body.Close(), ref ᒐ);
        (_, err) = io.ReadAll((~resp).Body);
        if (err != default!) {
            Ꮡt.Fatalf("read body failed: %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportRemovesDeadIdleConnections(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportRemovesDeadIdleConnections(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string firstˢ = "first"u8;
internal static readonly @string secondˢ = "second"u8;

internal static void testTransportRemovesDeadIdleConnections(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), (~r).RemoteAddr);
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    var cʗ1 = c;
    var tsʗ1 = ts;
    void doReq(@string name) {
        GoFrame ᒐ = default;
        try {
            // Do a POST instead of a GET to prevent the Transport's
            // idempotent request retry logic from kicking in...
            var (res, err) = cʗ1.Post((~tsʗ1).URL, ""u8, default!);
            if (err != default!) {
                Ꮡt.Fatalf("%s: %v"u8, name, err);
            }
            if ((~res).StatusCode != 200) {
                Ꮡt.Fatalf("%s: %v"u8, name, (~res).Status);
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            (var slurp, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Fatalf("%s: %v"u8, name, err);
            }
            Ꮡt.Logf("%s: ok (%q)"u8, name, slurp);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    doReq(firstˢ);
    var keys1 = tr.IdleConnKeysForTesting();
    ts.CloseClientConnections();
    ref var keys2 = ref heap<slice<@string>>(out var Ꮡkeys2);
    var keys1ʗ1 = keys1;
    var trʗ1 = tr;
    waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
        Ꮡkeys2.ValueSlot = trʗ1.IdleConnKeysForTesting();
        if (len(Ꮡkeys2.ValueSlot) != 0) {
            if (d > 0) {
                Ꮡt.Logf("Transport hasn't noticed idle connection's death in %v.\nbefore: %q\n after: %q\n"u8, d, keys1ʗ1, Ꮡkeys2.ValueSlot);
            }
            return false;
        }
        return true;
    });
    doReq(secondˢ);
}

// Test that the Transport notices when a server hangs up on its
// unexpectedly (a keep-alive connection is closed).
public static void TestTransportServerClosingUnexpectedly(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportServerClosingUnexpectedly(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportServerClosingUnexpectedly(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hostPortHandler)).Value.ts;
    var c = ts.Client();
    var cʗ1 = c;
    var tsʗ1 = ts;
    @string fetch(nint n, nint retries) {
        void condFatalf(@string format, params ꓸꓸꓸany argʗp) {
            var arg = argʗp.sslice();
            if (retries <= 0) {
                Ꮡt.Fatalf(format, arg.ꓸꓸꓸ);
            }
            Ꮡt.Logf("retrying shortly after expected error: "u8 + format, arg.ꓸꓸꓸ);
            time.Sleep(time.ΔSecond / ((time.Duration)(int64)retries));
        }
        while (retries >= 0) {
            retries--;
            var (res, err) = cʗ1.Get((~tsʗ1).URL);
            if (err != default!) {
                condFatalf("error in req #%d, GET: %v"u8, n, err);
                continue;
            }
            (var body, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                condFatalf("error in req #%d, ReadAll: %v"u8, n, err);
                continue;
            }
            (~res).Body.Close();
            return ((@string)body);
        }
        throw panic("unreachable");
    }
    @string body1 = fetch(1, 0);
    @string body2 = fetch(2, 0);
    // Close all the idle connections in a way that's similar to
    // the server hanging up on us. We don't use
    // httptest.Server.CloseClientConnections because it's
    // best-effort and stops blocking after 5 seconds. On a loaded
    // machine running many tests concurrently it's possible for
    // that method to be async and cause the body3 fetch below to
    // run on an old connection. This function is synchronous.
    http_internal_test_package.ExportCloseTransportConnsAbruptly((~c).Transport._<ж<Δhttp.Transport>>());
    @string body3 = fetch(3, 5);
    if (body1 != body2) {
        Ꮡt.Errorf("expected body1 and body2 to be equal"u8);
    }
    if (body2 == body3) {
        Ꮡt.Errorf("expected body2 and body3 to be different"u8);
    }
}

// Test for https://golang.org/issue/2616 (appropriate issue number)
// This fails pretty reliably with GOMAXPROCS=100 or something high.
public static void TestStressSurpriseServerCloses(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testStressSurpriseServerCloses(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ = (@string)"skipping test in short mode"u8;

internal static partial void testStressSurpriseServerCloses(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingTestInShortModeˢ);
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentLengthˢ, "5"u8);
        w.Header().Set(contentTypeˢ, textPlainˢ);
        w.Write(slice<byte>("Hello"u8));
        w._<Flusher>().Flush();
        var (conn, buf, _) = w._<Hijacker>().Hijack();
        buf.Value.Writer.Value.Flush();
        conn.Close();
    }))).Value.ts;
    var c = ts.Client();
    // Do a bunch of traffic from different goroutines. Send to activityc
    // after each request completes, regardless of whether it failed.
    // If these are too high, OS X exhausts its ephemeral ports
    // and hangs waiting for them to transition TCP states. That's
    // not what we want to test. TODO(bradfitz): use an io.Pipe
    // dialer for this test instead?
    UntypedInt numClients = 20;
    
    UntypedInt reqsPerClient = 25;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(numClients * reqsPerClient);
    for (nint i = 0; i < numClients; i++) {
        var cʗ1 = c;
        var tsʗ1 = ts;
        goǃ(() => {
            for (nint iΔ1 = 0; iΔ1 < reqsPerClient; iΔ1++) {
                var (res, err) = cʗ1.Get((~tsʗ1).URL);
                if (err == default!) {
                    // We expect errors since the server is
                    // hanging up on us after telling us to
                    // send more requests, so we don't
                    // actually care what the error is.
                    // But we want to close the body in cases
                    // where we won the race.
                    (~res).Body.Close();
                }
                Ꮡwg.Done();
            }
        });
    }
    // Make sure all the request come back, one way or another.
    Ꮡwg.Wait();
}

// TestTransportHeadResponses verifies that we deal with Content-Lengths
// with no bodies properly
public static void TestTransportHeadResponses(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportHeadResponses(Δp0, Δp1));
}

internal static void testTransportHeadResponses(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if ((~r).Method != "HEAD"u8) {
            throw panic("expected HEAD; got " + (~r).Method);
        }
        w.Header().Set(contentLengthˢ, "123"u8);
        w.WriteHeader(200);
    }))).Value.ts;
    var c = ts.Client();
    for (nint i = 0; i < 2; i++) {
        var (res, err) = c.Head((~ts).URL);
        if (err != default!) {
            Ꮡt.Errorf("error on loop %d: %v"u8, i, err);
            continue;
        }
        {
            @string e = "123"u8;
            @string g = (~res).Header.Get(contentLengthˢ); if (e != g) {
                Ꮡt.Errorf("loop %d: expected Content-Length header of %q, got %q"u8, i, e, g);
            }
        }
        {
            var (e, g) = ((int64)123, res.Value.ContentLength); if (e != g) {
                Ꮡt.Errorf("loop %d: expected res.ContentLength of %v, got %v"u8, i, e, g);
            }
        }
        {
            var (all, errΔ1) = io.ReadAll((~res).Body); if (errΔ1 != default!){
                Ꮡt.Errorf("loop %d: Body ReadAll: %v"u8, i, errΔ1);
            } else 
            if (len(all) != 0) {
                Ꮡt.Errorf("Bogus body %q"u8, all);
            }
        }
    }
}

// TestTransportHeadChunkedResponse verifies that we ignore chunked transfer-encoding
// on responses to HEAD requests.
public static void TestTransportHeadChunkedResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportHeadChunkedResponse(Δp0, Δp1), new testMode[]{http1Mode}.slice(), testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xClientIpportˢ = "x-client-ipport"u8;

internal static void testTransportHeadChunkedResponse(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~r).Method != "HEAD"u8) {
                throw panic("expected HEAD; got " + (~r).Method);
            }
            w.Header().Set(transferEncodingˢ, chunkedˢ); // client should ignore
            w.Header().Set(xClientIpportˢ, (~r).RemoteAddr);
            w.WriteHeader(200);
        }))).Value.ts;
        var c = ts.Client();
        // Ensure that we wait for the readLoop to complete before
        // calling Head again
        var didRead = new channel<bool>(0);
        var didReadʗ1 = didRead;
        http_internal_test_package.SetReadLoopBeforeNextReadHook(() => {
            didReadʗ1.ᐸꟷ(true);
        });
        defer(http_internal_test_package.SetReadLoopBeforeNextReadHook, (Action)(default!), ref ᒐ);
        var (res1, err) = c.Head((~ts).URL);
        ᐸꟷ(didRead);
        if (err != default!) {
            Ꮡt.Fatalf("request 1 error: %v"u8, err);
        }
        (var res2, err) = c.Head((~ts).URL);
        ᐸꟷ(didRead);
        if (err != default!) {
            Ꮡt.Fatalf("request 2 error: %v"u8, err);
        }
        {
            @string v1 = (~res1).Header.Get(xClientIpportˢ);
            @string v2 = (~res2).Header.Get(xClientIpportˢ); if (v1 != v2) {
                Ꮡt.Errorf("ip/ports differed between head requests: %q vs %q"u8, v1, v2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Requests with no accept-encoding header use transparent compression
// Requests with other accept-encoding should pass through unmodified
// Requests with accept-encoding == gzip should be passed through

[GoType("dyn")] partial struct roundTripTestsᴛ1 {
    internal @string accept;
    internal @string expectAccept;
    internal bool compressed;
}
internal static slice<roundTripTestsᴛ1> roundTripTests = new roundTripTestsᴛ1[]{
    new(""u8, "gzip"u8, false),
    new("foo"u8, "foo"u8, false),
    new("gzip"u8, "gzip"u8, true)
}.slice();

// Test that the modification made to the Request by the RoundTripper is cleaned up
public static void TestRoundTripGzip(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRoundTripGzip(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectAcceptˢ = "expect_accept"u8;
internal static readonly @string testnumˢ = "testnum"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string responseBodyᶜ = "test response body"u8;

internal static void testRoundTripGzip(ж<testing.T> Ꮡt, testMode mode) {
    @string responseBody = responseBodyᶜ;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        @string accept = (~req).Header.Get(acceptEncodingˢ);
        {
            @string expect = req.FormValue(expectAcceptˢ); if (accept != expect) {
                Ꮡt.Errorf("in handler, test %v: Accept-Encoding = %q, want %q"u8,
                    req.FormValue(testnumˢ), accept, expect);
            }
        }
        if (accept == "gzip"u8){
            rw.Header().Set(contentEncodingˢ, gzipˢ);
            var gz = gzip.NewWriter(new http_test_package.http_ResponseWriterᴠWriter(rw));
            gz.Write(slice<byte>(responseBody));
            gz.Close();
        } else {
            rw.Header().Set(contentEncodingˢ, accept);
            rw.Write(slice<byte>(responseBody));
        }
    }))).Value.ts;
    var tr = (~ts.Client()).Transport._<ж<Δhttp.Transport>>();
    foreach (var (i, test) in roundTripTests) {
        // Test basic request (no accept-encoding)
        var (req, _) = NewRequest(getˢ2, fmt.Sprintf("%s/?testnum=%d&expect_accept=%s"u8, (~ts).URL, i, test.expectAccept), default!);
        if (test.accept != ""u8) {
            (~req).Header.Set(acceptEncodingˢ, test.accept);
        }
        var (res, err) = tr.RoundTrip(req);
        if (err != default!) {
            Ꮡt.Errorf("%d. RoundTrip: %v"u8, i, err);
            continue;
        }
        slice<byte> body = default!;
        if (test.compressed){
            ж<gzip.Reader> r = default!;
            (r, err) = gzip.NewReader((~res).Body);
            if (err != default!) {
                Ꮡt.Errorf("%d. gzip NewReader: %v"u8, i, err);
                continue;
            }
            (body, err) = io.ReadAll(new http_test_package.gzip_ReaderжReader(r));
            (~res).Body.Close();
        } else {
            (body, err) = io.ReadAll((~res).Body);
        }
        if (err != default!) {
            Ꮡt.Errorf("%d. Error: %q"u8, i, err);
            continue;
        }
        {
            @string g = ((@string)body);
            @string e = responseBody; if (g != e) {
                Ꮡt.Errorf("%d. body = %q; want %q"u8, i, g, e);
            }
        }
        {
            @string g = (~req).Header.Get(acceptEncodingˢ);
            @string e = test.accept; if (g != e) {
                Ꮡt.Errorf("%d. Accept-Encoding = %q; want %q (it was mutated, in violation of RoundTrip contract)"u8, i, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(contentEncodingˢ);
            @string e = test.accept; if (g != e) {
                Ꮡt.Errorf("%d. Content-Encoding = %q; want %q"u8, i, g, e);
            }
        }
    }
}

public static void TestTransportGzip(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportGzip(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue56020ˢ = (@string)"https://go.dev/issue/56020"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testStringᶜ = "The test string aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"u8;

internal static void testTransportGzip(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        Ꮡt.Skip(httpsGoDevIssue56020ˢ);
    }
    @string testString = testStringᶜ;
    const int64 nRandBytes = /* 1024 * 1024 */ 1048576;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        GoFrame ᒐ = default;
        try {
            if ((~req).Method == "HEAD"u8) {
                {
                    @string g = (~req).Header.Get(acceptEncodingˢ); if (g != ""u8) {
                        Ꮡt.Errorf("HEAD request sent with Accept-Encoding of %q; want none"u8, g);
                    }
                }
                return;
            }
            {
                @string g = (~req).Header.Get(acceptEncodingˢ);
                @string e = gzipˢ; if (g != e) {
                    Ꮡt.Errorf("Accept-Encoding = %q, want %q"u8, g, e);
                }
            }
            rw.Header().Set(contentEncodingˢ, gzipˢ);
            io.Writer w = new http_test_package.http_ResponseWriterᴠWriter(rw);
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            if (req.FormValue(chunkedˢ) == "0"u8) {
                w = new http_test_package.bytes_BufferжWriter(Ꮡbuf);
                defer(io.Copy, new http_test_package.http_ResponseWriterᴠWriter(rw), new http_test_package.bytes_BufferжReader(Ꮡbuf), ref ᒐ);
                defer(() => {
                    rw.Header().Set(contentLengthˢ, strconv.Itoa(Ꮡbuf.Value.Len()));
                }, ref ᒐ);
            }
            var gz = gzip.NewWriter(w);
            gz.Write(slice<byte>(testString));
            if (req.FormValue(bodyˢ) == "large"u8) {
                io.CopyN(new http_test_package.gzip_WriterжWriter(gz), rand.Reader, nRandBytes);
            }
            gz.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))).Value.ts;
    var c = ts.Client();
    foreach (var (_, chunked) in new @string[]{"1"u8, "0"u8}.slice()) {
        // First fetch something large, but only read some of it.
        var (resΔ1, errΔ1) = c.Get((~ts).URL + "/?body=large&chunked="u8 + chunked);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("large get: %v"u8, errΔ1);
        }
        var buf = new slice<byte>(len(testString));
        (var n, errΔ1) = io.ReadFull((~resΔ1).Body, buf);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("partial read of large response: size=%d, %v"u8, n, errΔ1);
        }
        {
            @string e = testString;
            @string g = ((@string)buf); if (e != g) {
                Ꮡt.Errorf("partial read got %q, expected %q"u8, g, e);
            }
        }
        (~resΔ1).Body.Close();
        // Read on the body, even though it's closed
        (n, errΔ1) = (~resΔ1).Body.Read(buf);
        if (n != 0 || errΔ1 == default!) {
            Ꮡt.Errorf("expected error post-closed large Read; got = %d, %v"u8, n, errΔ1);
        }
        // Then something small.
        (resΔ1, errΔ1) = c.Get((~ts).URL + "/?chunked="u8 + chunked);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        (var body, errΔ1) = io.ReadAll((~resΔ1).Body);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        {
            @string g = ((@string)body);
            @string e = testString; if (g != e) {
                Ꮡt.Fatalf("body = %q; want %q"u8, g, e);
            }
        }
        {
            @string g = (~resΔ1).Header.Get(contentEncodingˢ);
            @string e = ""u8; if (g != e) {
                Ꮡt.Fatalf("Content-Encoding = %q; want %q"u8, g, e);
            }
        }
        // Read on the body after it's been fully read:
        (n, errΔ1) = (~resΔ1).Body.Read(buf);
        if (n != 0 || errΔ1 == default!) {
            Ꮡt.Errorf("expected Read error after exhausted reads; got %d, %v"u8, n, errΔ1);
        }
        (~resΔ1).Body.Close();
        (n, errΔ1) = (~resΔ1).Body.Read(buf);
        if (n != 0 || errΔ1 == default!) {
            Ꮡt.Errorf("expected Read error after Close; got %d, %v"u8, n, errΔ1);
        }
    }
    // And a HEAD request too, because they're always weird.
    var (res, err) = c.Head((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatalf("Head: %v"u8, err);
    }
    if ((~res).StatusCode != 200) {
        Ꮡt.Errorf("Head status=%d; want=200"u8, (~res).StatusCode);
    }
}

// A transport100Continue test exercises Transport behaviors when sending a
// request with an Expect: 100-continue header.
[GoType] partial struct transport100ContinueTest {
    internal ж<testing.T> t;
    internal channel<EmptyStruct> reqdone;
    internal ж<Δhttp.Response> resp;
    internal error respErr;
    internal net.Conn conn;
    internal ж<bufio.Reader> reader;
}

internal static readonly @string transport100ContinueTestBody = "request body"u8;

// newTransport100ContinueTest creates a Transport and sends an Expect: 100-continue
// request on it.
internal static partial ж<transport100ContinueTest> newTransport100ContinueTest(ж<testing.T> Ꮡt, time.Duration timeout) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var test = Ꮡ(new transport100ContinueTest(
            t: Ꮡt,
            reqdone: new channel<EmptyStruct>(0)
        ));
        var tr = Ꮡ(new Transport(
            ExpectContinueTimeout: timeout
        ));
        var lnʗ2 = ln;
        var testʗ1 = test;
        var trʗ1 = tr;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), (~testʗ1).reqdone, ref ᒐ);
                var body = strings.NewReader(transport100ContinueTestBody);
                var (req, _) = NewRequest(putˢ, "http://"u8 + lnʗ2.Addr().String(), new http_test_package.strings_ReaderжReader(body));
                (~req).Header.Set(expectˢ, continueˢ);
                req.Value.ContentLength = (int64)len(transport100ContinueTestBody);
                (testʗ1.Value.resp, testʗ1.Value.respErr) = trʗ1.RoundTrip(req);
                (~(~testʗ1).resp).Body.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var (c, err) = ln.Accept();
        if (err != default!) {
            Ꮡt.Fatalf("Accept: %v"u8, err);
        }
        var cʗ1 = c;
        Ꮡt.Cleanup(() => {
            cʗ1.Close();
        });
        var br = bufio.NewReader(new http_test_package.net_ConnᴠReader(c));
        (_, err) = ReadRequest(br);
        if (err != default!) {
            Ꮡt.Fatalf("ReadRequest: %v"u8, err);
        }
        test.Value.conn = c;
        test.Value.reader = br;
        var testʗ2 = test;
        var trʗ2 = tr;
        Ꮡt.Cleanup(() => {
            ᐸꟷ((~testʗ2).reqdone);
            trʗ2.CloseIdleConnections();
            var (got, _) = io.ReadAll(new http_test_package.bufio_ReaderжReader((~testʗ2).reader));
            if (len(got) > 0) {
                Ꮡt.Fatalf("Transport sent unexpected bytes: %q"u8, got);
            }
        });
        return test;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// respond sends response lines from the server to the transport.
[GoRecv] internal static void respond(this ref transport100ContinueTest test, params ꓸꓸꓸstring linesʗp) {
    var lines = linesʗp.sslice();

    foreach (var (_, line) in lines) {
        {
            var (_, err) = test.conn.Write(slice<byte>(line + "\r\n")); if (err != default!) {
                test.t.Fatalf("Write: %v"u8, err);
            }
        }
    }
    {
        var (_, err) = test.conn.Write(slice<byte>("\r\n"u8)); if (err != default!) {
            test.t.Fatalf("Write: %v"u8, err);
        }
    }
}

// wantBodySent ensures the transport has sent the request body to the server.
[GoRecv] internal static void wantBodySent(this ref transport100ContinueTest test) {
    var (got, err) = io.ReadAll(io.LimitReader(new http_test_package.bufio_ReaderжReader(test.reader), (int64)len(transport100ContinueTestBody)));
    if (err != default!) {
        test.t.Fatalf("unexpected error reading body: %v"u8, err);
    }
    {
        @string gotΔ1 = ((@string)got);
        @string want = transport100ContinueTestBody; if (gotΔ1 != want) {
            test.t.Fatalf("unexpected body: got %q, want %q"u8, gotΔ1, want);
        }
    }
}

// wantRequestDone ensures the Transport.RoundTrip has completed with the expected status.
[GoRecv] internal static void wantRequestDone(this ref transport100ContinueTest test, nint want) {
    ᐸꟷ(test.reqdone);
    if (test.respErr != default!) {
        test.t.Fatalf("unexpected RoundTrip error: %v"u8, test.respErr);
    }
    {
        nint got = test.resp.Value.StatusCode; if (got != want) {
            test.t.Fatalf("unexpected response code: got %v, want %v"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11100Continueˢ = "HTTP/1.1 100 Continue"u8;
internal static readonly @string http11200ˢ = "HTTP/1.1 200"u8;

public static void TestTransportExpect100ContinueSent(ж<testing.T> Ꮡt) {
    var test = newTransport100ContinueTest(Ꮡt, (time.Duration)(3600000000000L));
    // Server sends a 100 Continue response, and the client sends the request body.
    test.respond(http11100Continueˢ);
    test.wantBodySent();
    test.respond(http11200ˢ, contentLength0ˢ);
    test.wantRequestDone(200);
}

public static void TestTransportExpect100Continue200ResponseNoConnClose(ж<testing.T> Ꮡt) {
    var test = newTransport100ContinueTest(Ꮡt, (time.Duration)(3600000000000L));
    // No 100 Continue response, no Connection: close header.
    test.respond(http11200ˢ, contentLength0ˢ);
    test.wantBodySent();
    test.wantRequestDone(200);
}

public static void TestTransportExpect100Continue200ResponseWithConnClose(ж<testing.T> Ꮡt) {
    var test = newTransport100ContinueTest(Ꮡt, (time.Duration)(3600000000000L));
    // No 100 Continue response, Connection: close header set.
    test.respond(http11200ˢ, connectionCloseˢ, contentLength0ˢ);
    test.wantRequestDone(200);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11500ˢ = "HTTP/1.1 500"u8;

public static void TestTransportExpect100Continue500ResponseNoConnClose(ж<testing.T> Ꮡt) {
    var test = newTransport100ContinueTest(Ꮡt, (time.Duration)(3600000000000L));
    // No 100 Continue response, no Connection: close header.
    test.respond(http11500ˢ, contentLength0ˢ);
    test.wantBodySent();
    test.wantRequestDone(500);
}

public static void TestTransportExpect100Continue500ResponseTimeout(ж<testing.T> Ꮡt) {
    var test = newTransport100ContinueTest(Ꮡt, 5 * time.Millisecond); // short timeout
    test.wantBodySent(); // after timeout
    test.respond(http11200ˢ, contentLength0ˢ);
    test.wantRequestDone(200);
}

public static void TestSOCKS5Proxy(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testSOCKS5Proxy(Δp0, Δp1), new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xSentinelˢ = "X-Sentinel"u8;

internal static void testSOCKS5Proxy(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ch = new channel<@string>(1);
        var l = newLocalListener(Ꮡt);
        var lʗ1 = l;
        defer(() => lʗ1.Close(), ref ᒐ);
        defer(ᴛ1 => builtin.close(ᴛ1), ch, ref ᒐ);
        var chʗ1 = ch;
        var lʗ2 = l;
        [MethodImpl(MethodImplOptions.NoInlining)] void proxy(ж<testing.T> tΔ1) {
            GoFrame ᒐ = default;
            try {
                var (s, errΔ1) = lʗ2.Accept();
                if (errΔ1 != default!) {
                    tΔ1.Errorf("socks5 proxy Accept(): %v"u8, errΔ1);
                    return;
                }
                var sʗ1 = s;
                defer(() => sʗ1.Close(), ref ᒐ);
                array<byte> buf = new(22);
                {
                    var (_, errΔ2) = io.ReadFull(new http_test_package.net_ConnᴠReader(s), buf[..3]); if (errΔ2 != default!) {
                        tΔ1.Errorf("socks5 proxy initial read: %v"u8, errΔ2);
                        return;
                    }
                }
                {
                    var want = new byte[]{5, 1, 0}.slice(); if (!bytes.Equal(buf[..3], want)) {
                        tΔ1.Errorf("socks5 proxy initial read: got %v, want %v"u8, buf[..3], want);
                        return;
                    }
                }
                {
                    var (_, errΔ3) = s.Write(new byte[]{5, 0}.slice()); if (errΔ3 != default!) {
                        tΔ1.Errorf("socks5 proxy initial write: %v"u8, errΔ3);
                        return;
                    }
                }
                {
                    var (_, errΔ4) = io.ReadFull(new http_test_package.net_ConnᴠReader(s), buf[..4]); if (errΔ4 != default!) {
                        tΔ1.Errorf("socks5 proxy second read: %v"u8, errΔ4);
                        return;
                    }
                }
                {
                    var want = new byte[]{5, 1, 0}.slice(); if (!bytes.Equal(buf[..3], want)) {
                        tΔ1.Errorf("socks5 proxy second read: got %v, want %v"u8, buf[..3], want);
                        return;
                    }
                }
                nint ipLen = default!;
                switch (buf[3]) {
                case 1: {
                    ipLen = net.IPv4len;
                    break;
                }
                case 4: {
                    ipLen = net.IPv6len;
                    break;
                }
                default: {
                    tΔ1.Errorf("socks5 proxy second read: unexpected address type %v"u8, buf[4]);
                    return;
                }}

                {
                    var (_, errΔ5) = io.ReadFull(new http_test_package.net_ConnᴠReader(s), buf.slice(4, ipLen + 6)); if (errΔ5 != default!) {
                        tΔ1.Errorf("socks5 proxy address read: %v"u8, errΔ5);
                        return;
                    }
                }
                var ip = ((net.IP)(buf.slice(4, ipLen + 4)));
                var port = binary.BigEndian.Uint16(buf.slice(ipLen + 4, ipLen + 6));
                copy(buf[..3], new byte[]{5, 0, 0}.slice());
                {
                    var (_, errΔ6) = s.Write(buf.slice(0, ipLen + 6)); if (errΔ6 != default!) {
                        tΔ1.Errorf("socks5 proxy connect write: %v"u8, errΔ6);
                        return;
                    }
                }
                chʗ1.ᐸꟷ(fmt.Sprintf("proxy for %s:%d"u8, ip, port));
                // Implement proxying.
                @string targetHost = net.JoinHostPort(ip.String(), strconv.Itoa((nint)port));
                (var targetConn, errΔ1) = net.Dial(tcpˢ, targetHost);
                if (errΔ1 != default!) {
                    tΔ1.Errorf("net.Dial failed"u8);
                    return;
                }
                goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), new http_test_package.net_ConnᴠWriter(targetConn), new http_test_package.net_ConnᴠReader(s));
                io.Copy(new http_test_package.net_ConnᴠWriter(s), new http_test_package.net_ConnᴠReader(targetConn)); // Wait for the client to close the socket.
                targetConn.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        var (pu, err) = url.Parse("socks5://"u8 + l.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string sentinelHeader = xSentinelˢ;
        @string sentinelValue = "12345"u8;
        var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(sentinelHeader, sentinelValue);
        });
        foreach (var (_, useTLS) in new bool[]{false, true}.slice()) {
            var chʗ2 = ch;
            var hʗ1 = h;
            var proxyʗ1 = proxy;
            var puʗ1 = pu;
            Ꮡt.Run(fmt.Sprintf("useTLS=%v"u8, useTLS), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
                var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ2), mode, new http_test_package.http_HandlerFuncᴠΔHandler(hʗ1)).Value.ts;
                var proxyʗ2 = proxyʗ1;
                goǃ(proxyʗ2, tΔ2);
                var c = ts.Client();
                (~c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = ProxyURL(puʗ1);
                var (r, errΔ7) = c.Head((~ts).URL);
                if (errΔ7 != default!) {
                    tΔ2.Fatal(errΔ7);
                }
                if ((~r).Header.Get(sentinelHeader) != sentinelValue) {
                    tΔ2.Errorf("Failed to retrieve sentinel value"u8);
                }
                @string got = ᐸꟷ(chʗ2);
                ts.Close();
                (var tsu, errΔ7) = url.Parse((~ts).URL);
                if (errΔ7 != default!) {
                    tΔ2.Fatal(errΔ7);
                }
                @string want = "proxy for "u8 + (~tsu).Host;
                if (got != want) {
                    tΔ2.Errorf("got %q, want %q"u8, got, want);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object invalidSiteUrlˢ = (@string)"Invalid site URL"u8;

[GoType("dyn")] internal partial struct TestTransportProxy_testCases {
    internal testMode siteMode, proxyMode;
}

public static void TestTransportProxy(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var testCases = new TestTransportProxy_testCases[]{
            new(http1Mode, http1Mode),
            new(http1Mode, https1Mode),
            new(https1Mode, http1Mode),
            new(https1Mode, https1Mode)
        }.slice();
        foreach (var (_, testCase) in testCases) {
            testMode siteMode = testCase.siteMode;
            testMode proxyMode = testCase.proxyMode;
            Ꮡt.Run(fmt.Sprintf("site=%v/proxy=%v"u8, siteMode, proxyMode), (ж<testing.T> tΔ1) => {
                var siteCh = new channel<ж<Δhttp.Request>>(1);
                var siteChʗ1 = siteCh;
                var h1 = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                    siteChʗ1.ᐸꟷ(r);
                });
                var proxyCh = new channel<ж<Δhttp.Request>>(1);
                var proxyChʗ1 = proxyCh;
                var h2 = new Δhttp.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                    proxyChʗ1.ᐸꟷ(r);
                    // Implement an entire CONNECT proxy
                    if ((~r).Method == "CONNECT"u8) {
                        var (hijacker, ok) = w._<Hijacker>(ᐧ);
                        if (!ok) {
                            tΔ1.Errorf("hijack not allowed"u8);
                            return;
                        }
                        var (clientConn, _, errΔ1) = hijacker.Hijack();
                        if (errΔ1 != default!) {
                            tΔ1.Errorf("hijacking failed"u8);
                            return;
                        }
                        var res = Ꮡ(new Response(
                            StatusCode: StatusOK,
                            Proto: "HTTP/1.1"u8,
                            ProtoMajor: 1,
                            ProtoMinor: 1,
                            Header: new httpꓸHeader(0)
                        ));
                        (var targetConn, errΔ1) = net.Dial(tcpˢ, (~(~r).URL).Host);
                        if (errΔ1 != default!) {
                            tΔ1.Errorf("net.Dial(%q) failed: %v"u8, (~(~r).URL).Host, errΔ1);
                            return;
                        }
                        {
                            var errΔ2 = res.Write(new http_test_package.net_ConnᴠWriter(clientConn)); if (errΔ2 != default!) {
                                tΔ1.Errorf("Writing 200 OK failed: %v"u8, errΔ2);
                                return;
                            }
                        }
                        goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), new http_test_package.net_ConnᴠWriter(targetConn), new http_test_package.net_ConnᴠReader(clientConn));
                        var clientConnʗ1 = clientConn;
                        var targetConnʗ1 = targetConn;
                        goǃ(() => {
                            io.Copy(new http_test_package.net_ConnᴠWriter(clientConnʗ1), new http_test_package.net_ConnᴠReader(targetConnʗ1));
                            targetConnʗ1.Close();
                        });
                    }
                });
                var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), siteMode, new http_test_package.http_HandlerFuncᴠΔHandler(h1)).Value.ts;
                var proxy = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), proxyMode, new http_test_package.http_HandlerFuncᴠΔHandler(h2)).Value.ts;
                var (pu, err) = url.Parse((~proxy).URL);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                // If neither server is HTTPS or both are, then c may be derived from either.
                // If only one server is HTTPS, c must be derived from that server in order
                // to ensure that it is configured to use the fake root CA from testcert.go.
                var c = proxy.Client();
                if (siteMode == https1Mode) {
                    c = ts.Client();
                }
                (~c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = ProxyURL(pu);
                {
                    var (_, errΔ1) = c.Head((~ts).URL); if (errΔ1 != default!) {
                        tΔ1.Error(errΔ1);
                    }
                }
                var got = ᐸꟷ(proxyCh);
                (~c).Transport._<ж<Δhttp.Transport>>().CloseIdleConnections();
                ts.Close();
                proxy.Close();
                if (siteMode == https1Mode){
                    // First message should be a CONNECT, asking for a socket to the real server,
                    if ((~got).Method != "CONNECT"u8) {
                        tΔ1.Errorf("Wrong method for secure proxying: %q"u8, (~got).Method);
                    }
                    @string gotHost = got.Value.URL.Value.Host;
                    var (puΔ1, errΔ2) = url.Parse((~ts).URL);
                    if (errΔ2 != default!) {
                        tΔ1.Fatal(invalidSiteUrlˢ);
                    }
                    {
                        @string wantHost = puΔ1.Value.Host; if (gotHost != wantHost) {
                            tΔ1.Errorf("Got CONNECT host %q, want %q"u8, gotHost, wantHost);
                        }
                    }
                    // The next message on the channel should be from the site's server.
                    var next = ᐸꟷ(siteCh);
                    if ((~next).Method != "HEAD"u8) {
                        tΔ1.Errorf("Wrong method at destination: %s"u8, (~next).Method);
                    }
                    {
                        @string nextURL = (~next).URL.String(); if (nextURL != "/"u8) {
                            tΔ1.Errorf("Wrong URL at destination: %s"u8, nextURL);
                        }
                    }
                } else {
                    if ((~got).Method != "HEAD"u8) {
                        tΔ1.Errorf("Wrong method for destination: %q"u8, (~got).Method);
                    }
                    @string gotURL = (~got).URL.String();
                    @string wantURL = (~ts).URL + "/"u8;
                    if (gotURL != wantURL) {
                        tΔ1.Errorf("Got URL %q, want %q"u8, gotURL, wantURL);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestOnProxyConnectResponse_type {
    internal nint proxyStatusCode;
    internal error err;
}

public static void TestOnProxyConnectResponse(ж<testing.T> Ꮡt) {
    slice<TestOnProxyConnectResponse_type> tcases = new TestOnProxyConnectResponse_type[]{
        new(
            StatusOK,
            default!
        ),
        new(
            StatusForbidden,
            errors.New("403"u8)
        )
    }.slice();
    foreach (var (_, vᴛ1) in tcases) {
        ref var tcase = ref heap(new TestOnProxyConnectResponse_type(), out var Ꮡtcase);
        tcase = vᴛ1;

        var h1 = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        });
        var tcaseʗ1 = tcase;
        var h2 = new Δhttp.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            // Implement an entire CONNECT proxy
            if ((~r).Method == "CONNECT"u8) {
                if (tcaseʗ1.proxyStatusCode != StatusOK) {
                    w.WriteHeader(tcaseʗ1.proxyStatusCode);
                    return;
                }
                var (hijacker, ok) = w._<Hijacker>(ᐧ);
                if (!ok) {
                    Ꮡt.Errorf("hijack not allowed"u8);
                    return;
                }
                var (clientConn, _, errΔ1) = hijacker.Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("hijacking failed"u8);
                    return;
                }
                var res = Ꮡ(new Response(
                    StatusCode: StatusOK,
                    Proto: "HTTP/1.1"u8,
                    ProtoMajor: 1,
                    ProtoMinor: 1,
                    Header: new httpꓸHeader(0)
                ));
                (var targetConn, errΔ1) = net.Dial(tcpˢ, (~(~r).URL).Host);
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("net.Dial(%q) failed: %v"u8, (~(~r).URL).Host, errΔ1);
                    return;
                }
                {
                    var errΔ2 = res.Write(new http_test_package.net_ConnᴠWriter(clientConn)); if (errΔ2 != default!) {
                        Ꮡt.Errorf("Writing 200 OK failed: %v"u8, errΔ2);
                        return;
                    }
                }
                goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), new http_test_package.net_ConnᴠWriter(targetConn), new http_test_package.net_ConnᴠReader(clientConn));
                var clientConnʗ1 = clientConn;
                var targetConnʗ1 = targetConn;
                goǃ(() => {
                    io.Copy(new http_test_package.net_ConnᴠWriter(clientConnʗ1), new http_test_package.net_ConnᴠReader(targetConnʗ1));
                    targetConnʗ1.Close();
                });
            }
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), https1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(h1)).Value.ts;
        var proxy = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), https1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(h2)).Value.ts;
        var (pu, err) = url.Parse((~proxy).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var c = proxy.Client();
        ref var dials = ref heap(new atomic.Int32(), out var Ꮡdials);
        ref var closes = ref heap(new atomic.Int32(), out var Ꮡcloses);
        (~c).Transport._<ж<Δhttp.Transport>>().Value.DialContext = (net.Conn, error) (context.Context ctx, @string network, @string addr) => {
            var (conn, errΔ1) = net.Dial(network, addr);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            Ꮡdials.Add(1);
            return (new noteCloseConn(
                Conn: conn,
                closeFunc: () => {
                    Ꮡcloses.Add(1);
                }
            ), default!);
        };
        (~c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = ProxyURL(pu);
        var puʗ1 = pu;
        var tcaseʗ2 = tcase;
        var tsʗ1 = ts;
        (~c).Transport._<ж<Δhttp.Transport>>().Value.OnProxyConnectResponse = (context.Context ctx, ж<url.URL> proxyURL, ж<Δhttp.Request> connectReq, ж<Δhttp.Response> connectRes) => {
            if (proxyURL.String() != puʗ1.String()) {
                Ꮡt.Errorf("proxy url got %s, want %s"u8, proxyURL.OrTypedNil(), puʗ1.OrTypedNil());
            }
            if ("https://"u8 + (~connectReq).URL.String() != (~tsʗ1).URL) {
                Ꮡt.Errorf("connect url got %s, want %s"u8, (~connectReq).URL.OrTypedNil(), (~tsʗ1).URL);
            }
            return tcaseʗ2.err;
        };
        var wantCloses = (int32)0;
        {
            var (_, errΔ2) = c.Head((~ts).URL); if (errΔ2 != default!){
                wantCloses = 1;
                if (tcase.err != default! && !strings.Contains(errΔ2.Error(), tcase.err.Error())) {
                    Ꮡt.Errorf("got %v, want %v"u8, errΔ2, tcase.err);
                }
            } else {
                if (tcase.err != default!) {
                    Ꮡt.Errorf("got %v, want nil"u8, errΔ2);
                }
            }
        }
        {
            var (got, want) = (Ꮡdials.Load(), (int32)1); if (got != want) {
                Ꮡt.Errorf("got %v dials, want %v"u8, got, want);
            }
        }
        // #64804: If OnProxyConnectResponse returns an error, we should close the conn.
        {
            var (got, want) = (Ꮡcloses.Load(), wantCloses); if (got != want) {
                Ꮡt.Errorf("got %v closes, want %v"u8, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsGolangFakeTldˢ = "https://golang.fake.tld/"u8;

// Issue 28012: verify that the Transport closes its TCP connection to http proxies
// when they're slow to reply to HTTPS CONNECT responses.
public static partial void TestTransportProxyHTTPSConnectLeak(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var cancelc = new channel<EmptyStruct>(0);
        var cancelcʗ1 = cancelc;
        http_internal_test_package.SetTestHookProxyConnectTimeout(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (context.Context ctxʗp, time.Duration timeout) => {
            ref var ctx = ref heap(ctxʗp, out var Ꮡctx);
            (ctx, var cancel) = context_package.WithCancel(ctx);
            var cancelʗ1 = cancel;
            var cancelcʗ2 = cancelcʗ1;
            goǃ(() => {
                var selᴛ67 = cancelcʗ2;
                var selᴛ68 = Ꮡctx.ValueSlot.Done();
                switch (select(ᐸꟷ(selᴛ67, ꓸꓸꓸ), ᐸꟷ(selᴛ68, ꓸꓸꓸ))) {
                case 0 when selᴛ67.ꟷᐳ(out _): {
                    break;
                }
                case 1 when selᴛ68.ꟷᐳ(out _): {
                    break;
                }}
                cancelʗ1();
            });
            return (ctx, cancel);
        });
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var listenerDone = new channel<EmptyStruct>(0);
        var cancelcʗ3 = cancelc;
        var listenerDoneʗ1 = listenerDone;
        var lnʗ2 = ln;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), listenerDoneʗ1, ref ᒐ);
                var (cΔ1, errΔ1) = lnʗ2.Accept();
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("Accept: %v"u8, errΔ1);
                    return;
                }
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
                // Read the CONNECT request
                var br = bufio.NewReader(new http_test_package.net_ConnᴠReader(cΔ1));
                (var cr, errΔ1) = ReadRequest(br);
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("proxy server failed to read CONNECT request"u8);
                    return;
                }
                if ((~cr).Method != "CONNECT"u8) {
                    Ꮡt.Errorf("unexpected method %q"u8, (~cr).Method);
                    return;
                }
                // Now hang and never write a response; instead, cancel the request and wait
                // for the client to close.
                // (Prior to Issue 28012 being fixed, we never closed.)
                builtin.close(cancelcʗ3);
                ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                (_, errΔ1) = br.Read(buf[..]);
                if (!AreEqual(errΔ1, io.EOF)) {
                    Ꮡt.Errorf("proxy server Read err = %v; want EOF"u8, errΔ1);
                }
                return;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
                var lnʗ3 = ln;
        var c = Ꮡ(new Client(
            Transport: new Δhttp.TransportжRoundTripper(Ꮡ(new Transport(
                Proxy: (ж<Δhttp.Request> _Δp0) => url.Parse("http://"u8 + lnʗ3.Addr().String())
            )))
        ));
        var (req, err) = NewRequest(getˢ2, httpsGolangFakeTldˢ, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, err) = c.Do(req);
        if (err == default!) {
            Ꮡt.Errorf("unexpected Get success"u8);
        }
        // Wait unconditionally for the listener goroutine to exit: this should never
        // hang, so if it does we want a full goroutine dump — and that's exactly what
        // the testing package will give us when the test run times out.
        ᐸꟷ(listenerDone);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someDialErrorˢ = "some dial error"u8;
internal static readonly @string httpProxyFakeTldˢ = "http://proxy.fake.tld/"u8;
internal static readonly @string httpFakeTldˢ = "http://fake.tld"u8;
internal static readonly object wantedANonNilErrorˢ = (@string)"wanted a non-nil error"u8;

// Issue 16997: test transport dial preserves typed errors
public static void TestTransportDialPreservesNetOpProxyError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        error errDial = errors.New(someDialErrorˢ);
            var errDialʗ1 = errDial;
        var tr = Ꮡ(new Transport(
            Proxy: (ж<Δhttp.Request> _Δp0) => url.Parse(httpProxyFakeTldˢ),
            Dial: (@string _Δp0, @string _Δp1) => (default!, errDialʗ1)
        ));
        var trʗ1 = tr;
        defer(trʗ1.CloseIdleConnections, ref ᒐ);
        var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        var (req, _) = NewRequest(getˢ2, httpFakeTldˢ, default!);
        var (res, err) = c.Do(req);
        if (err == default!) {
            (~res).Body.Close();
            Ꮡt.Fatal(wantedANonNilErrorˢ);
        }
        var (uerr, ok) = err._<ж<urlꓸError>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("got %T, want *url.Error"u8, err);
        }
        (var oe, ok) = (~uerr).Err._<ж<net.OpError>>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("url.Error.Err =  %T; want *net.OpError"u8, (~uerr).Err);
        }
        var want = Ꮡ(new net.OpError(
            Op: "proxyconnect"u8,
            Net: "tcp"u8,
            Err: errDial
        ));
        // original error, unwrapped.
        if (!reflect.DeepEqual(oe.OrTypedNil(), want.OrTypedNil())) {
            Ꮡt.Errorf("Got error %#v; want %#v"u8, oe.OrTypedNil(), want.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 36431: calls to RoundTrip should not mutate t.ProxyConnectHeader.
//
// (A bug caused dialConn to instead write the per-request Proxy-Authorization
// header through to the shared Header instance, introducing a data race.)
public static void TestTransportProxyDialDoesNotMutateProxyConnectHeader(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportProxyDialDoesNotMutateProxyConnectHeader(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aladdinˢ2 = "aladdin"u8;
internal static readonly @string opensesameˢ = "opensesame"u8;

internal static void testTransportProxyDialDoesNotMutateProxyConnectHeader(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var proxy = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, NotFoundHandler()).Value.ts;
        var proxyʗ1 = proxy;
        defer(proxyʗ1.Close, ref ᒐ);
        var c = proxy.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        var proxyʗ2 = proxy;
        tr.Value.Proxy = (ж<url.URL>, error) (ж<Δhttp.Request> _Δp0) => {
            var (u, _) = url.Parse((~proxyʗ2).URL);
            u.Value.User = url.UserPassword(aladdinˢ2, opensesameˢ);
            return (u, default!);
        };
        var h = tr.Value.ProxyConnectHeader;
        if (h == default!) {
            h = new httpꓸHeader(0);
        }
        tr.Value.ProxyConnectHeader = h.Clone();
        var (req, err) = NewRequest(getˢ2, httpsGolangFakeTldˢ, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, err) = c.Do(req);
        if (err == default!) {
            Ꮡt.Errorf("unexpected Get success"u8);
        }
        if (!reflect.DeepEqual((~tr).ProxyConnectHeader, h)) {
            Ꮡt.Errorf("tr.ProxyConnectHeader = %v; want %v"u8, (~tr).ProxyConnectHeader, h);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestTransportGzipRecursive sends a gzip quine and checks that the
// client gets the same value back. This is more cute than anything,
// but checks that we don't recurse forever, and checks that
// Content-Encoding is removed.
public static void TestTransportGzipRecursive(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportGzipRecursive(Δp0, Δp1));
}

internal static void testTransportGzipRecursive(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentEncodingˢ, gzipˢ);
        w.Write(rgz);
    }))).Value.ts;
    var c = ts.Client();
    var (res, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var body, err) = io.ReadAll((~res).Body);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(body, rgz)) {
        Ꮡt.Fatalf("Incorrect result from recursive gz:\nhave=%x\nwant=%x"u8,
            body, rgz);
    }
    {
        @string g = (~res).Header.Get(contentEncodingˢ);
        @string e = ""u8; if (g != e) {
            Ꮡt.Fatalf("Content-Encoding = %q; want %q"u8, g, e);
        }
    }
}

// golang.org/issue/7750: request fails when server replies with
// a short gzip body
public static void TestTransportGzipShort(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportGzipShort(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectAnErrorFromReadingˢ = (@string)"Expect an error from reading a body."u8;

internal static void testTransportGzipShort(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentEncodingˢ, gzipˢ);
            w.Write(new byte[]{0x1f, 0x8b}.slice());
        }))).Value.ts;
        var c = ts.Client();
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (_, err) = io.ReadAll((~res).Body);
        if (err == default!) {
            Ꮡt.Fatal(expectAnErrorFromReadingˢ);
        }
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            Ꮡt.Errorf("ReadAll error = %v; want io.ErrUnexpectedEOF"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Wait until number of goroutines is no greater than nmax, or time out.
internal static nint waitNumGoroutine(nint nmax) {
    nint nfinal = runtime.NumGoroutine();
    for (nint ntries = 10; ntries > 0 && nfinal > nmax; ntries--) {
        time.Sleep(50 * time.Millisecond);
        runtime.GC();
        nfinal = runtime.NumGoroutine();
    }
    return nfinal;
}

// tests that persistent goroutine connections shut down when no longer desired.
public static void TestTransportPersistConnLeak(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportPersistConnLeak(Δp0, Δp1), testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object flakyInHttp2ˢ = (@string)"flaky in HTTP/2"u8;
internal static readonly object tooManyNewGoroutinesˢ = (@string)"too many new goroutines"u8;

internal static partial void testTransportPersistConnLeak(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        Ꮡt.Skip(flakyInHttp2ˢ);
    }
    // Not parallel: counts goroutines
    const nint numReq = 25;
    var gotReqCh = new channel<bool>(numReq);
    var unblockCh = new channel<bool>(numReq);
    var gotReqChʗ1 = gotReqCh;
    var unblockChʗ1 = unblockCh;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        gotReqChʗ1.ᐸꟷ(true);
        ᐸꟷ(unblockChʗ1);
        w.Header().Set(contentLengthˢ, "0"u8);
        w.WriteHeader(204);
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    nint n0 = runtime.NumGoroutine();
    var didReqCh = new channel<bool>(numReq);
    var failed = new channel<bool>(numReq);
    for (nint i = 0; i < numReq; i++) {
        var cʗ1 = c;
        var didReqChʗ1 = didReqCh;
        var failedʗ1 = failed;
        var tsʗ1 = ts;
        goǃ(() => {
            var (res, err) = cʗ1.Get((~tsʗ1).URL);
            didReqChʗ1.ᐸꟷ(true);
            if (err != default!) {
                Ꮡt.Logf("client fetch error: %v"u8, err);
                failedʗ1.ᐸꟷ(true);
                return;
            }
            (~res).Body.Close();
        });
    }
    // Wait for all goroutines to be stuck in the Handler.
    for (nint i = 0; i < numReq; i++) {
        var selᴛ69 = gotReqCh;
        var selᴛ70 = failed;
        switch (select(ᐸꟷ(selᴛ69, ꓸꓸꓸ), ᐸꟷ(selᴛ70, ꓸꓸꓸ))) {
        case 0 when selᴛ69.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ70.ꟷᐳ(out _): {
            break;
        }}
    }
    // ok
    // Not great but not what we are testing:
    // sometimes an overloaded system will fail to make all the connections.
    nint nhigh = runtime.NumGoroutine();
    // Tell all handlers to unblock and reply.
    builtin.close(unblockCh);
    // Wait for all HTTP clients to be done.
    for (nint i = 0; i < numReq; i++) {
        ᐸꟷ(didReqCh);
    }
    tr.CloseIdleConnections();
    nint nfinal = waitNumGoroutine(n0 + 5);
    nint growth = nfinal - n0;
    // We expect 0 or 1 extra goroutine, empirically. Allow up to 5.
    // Previously we were leaking one per numReq.
    if ((nint)growth > 5) {
        Ꮡt.Logf("goroutine growth: %d -> %d -> %d (delta: %d)"u8, n0, nhigh, nfinal, growth);
        Ꮡt.Error(tooManyNewGoroutinesˢ);
    }
}

// golang.org/issue/4531: Transport leaks goroutines when
// request.ContentLength is explicitly short
public static void TestTransportPersistConnLeakShortBody(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportPersistConnLeakShortBody(Δp0, Δp1), testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectAnErrorFromWritingˢ = (@string)"Expect an error from writing too long of a body."u8;

internal static void testTransportPersistConnLeakShortBody(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        Ꮡt.Skip(flakyInHttp2ˢ);
    }
    // Not parallel: measures goroutines.
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    nint n0 = runtime.NumGoroutine();
    var body = slice<byte>("Hello"u8);
    for (nint i = 0; i < 20; i++) {
        var (req, err) = NewRequest(postˢ, (~ts).URL, new http_test_package.bytes_ReaderжReader(bytes.NewReader(body)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        req.Value.ContentLength = (int64)(len(body) - 2); // explicitly short
        (_, err) = c.Do(req);
        if (err == default!) {
            Ꮡt.Fatal(expectAnErrorFromWritingˢ);
        }
    }
    nint nhigh = runtime.NumGoroutine();
    tr.CloseIdleConnections();
    nint nfinal = waitNumGoroutine(n0 + 5);
    nint growth = nfinal - n0;
    // We expect 0 or 1 extra goroutine, empirically. Allow up to 5.
    // Previously we were leaking one per numReq.
    Ꮡt.Logf("goroutine growth: %d -> %d -> %d (delta: %d)"u8, n0, nhigh, nfinal, growth);
    if ((nint)growth > 5) {
        Ꮡt.Error(tooManyNewGoroutinesˢ);
    }
}

// A countedConn is a net.Conn that decrements an atomic counter when finalized.
[GoType] partial struct countedConn {
    [GoEmbedded] public net_package.Conn Conn;
}

// A countingDialer dials connections and counts the number that remain reachable.
[GoType] partial struct countingDialer {
    internal net.Dialer dialer;
    internal sync.Mutex mu;
    internal int64 total, live;
}

internal static (net.Conn, error) DialContext(this ж<countingDialer> Ꮡd, context.Context ctx, @string network, @string address) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        var (conn, err) = Ꮡd.of(countingDialer.Ꮡdialer).DialContext(ctx, network, address);
        if (err != default!) {
            return (default!, err);
        }
        var counted = @new<countedConn>();
        counted.Value.Conn = conn;
        d.mu.Lock();
        ᒐd1 = true;
        d.total++;
        d.live++;
        runtime.SetFinalizer(counted.OrTypedNil(), Ꮡd.decrement);
        return (new http_test_package.countedConnжConn(counted), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void decrement(this ж<countingDialer> Ꮡd, ж<countedConn> _) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        ᒐd1 = true;
        d.live--;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static (int64 total, int64 live) Read(this ж<countingDialer> Ꮡd) {
    int64 total = default!;
    int64 live = default!;
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        d.mu.Lock();
        defer(Ꮡd.of(countingDialer.Ꮡmu).Unlock, ref ᒐ);
        (total, live) = (d.total, d.live);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (total, live);
}

public static void TestTransportPersistConnLeakNeverIdle(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportPersistConnLeakNeverIdle(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedBrokenConnectionˢ = (@string)"expected broken connection"u8;

internal static void testTransportPersistConnLeakNeverIdle(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        // Close every connection so that it cannot be kept alive.
        var (conn, _, err) = w._<Hijacker>().Hijack();
        if (err != default!) {
            Ꮡt.Errorf("Hijack failed unexpectedly: %v"u8, err);
            return;
        }
        conn.Close();
    }))).Value.ts;
    ref var d = ref heap(new countingDialer(), out var Ꮡd);
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.DialContext = Ꮡd.DialContext;
    var body = slice<byte>("Hello"u8);
    for (nint i = 0; ᐧ ; i++) {
        var (total, live) = Ꮡd.Read();
        if (live < total) {
            break;
        }
        if (i >= (1 << (int)(12))) {
            Ꮡt.Fatalf("Count of live client net.Conns (%d) not lower than total (%d) after %d Do / GC iterations."u8, live, total, i);
        }
        var (req, err) = NewRequest(postˢ, (~ts).URL, new http_test_package.bytes_ReaderжReader(bytes.NewReader(body)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, err) = c.Do(req);
        if (err == default!) {
            Ꮡt.Fatal(expectedBrokenConnectionˢ);
        }
        runtime.GC();
    }
}

[GoType] partial struct countedContext {
    [GoEmbedded] public context_package.Context Context;
}

[GoType] partial struct contextCounter {
    internal sync.Mutex mu;
    internal int64 live;
}

internal static context.Context Track(this ж<contextCounter> Ꮡcc, context.Context ctx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        var counted = @new<countedContext>();
        counted.Value.Context = ctx;
        cc.mu.Lock();
        ᒐd1 = true;
        cc.live++;
        runtime.SetFinalizer(counted.OrTypedNil(), Ꮡcc.decrement);
        return new http_test_package.countedContextжContext(counted);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡcc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void decrement(this ж<contextCounter> Ꮡcc, ж<countedContext> _) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        cc.mu.Lock();
        ᒐd1 = true;
        cc.live--;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡcc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static int64 /*live*/ Read(this ж<contextCounter> Ꮡcc) {
    int64 live = default!;
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        cc.mu.Lock();
        defer(Ꮡcc.of(contextCounter.Ꮡmu).Unlock, ref ᒐ);
        live = cc.live;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return live;
}

public static void TestTransportPersistConnContextLeakMaxConnsPerHost(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportPersistConnContextLeakMaxConnsPerHost(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue56021ˢ = (@string)"https://go.dev/issue/56021"u8;

internal static void testTransportPersistConnContextLeakMaxConnsPerHost(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        Ꮡt.Skip(httpsGoDevIssue56021ˢ);
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        runtime.Gosched();
        w.WriteHeader(StatusOK);
    }))).Value.ts;
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.MaxConnsPerHost = 1;
    var ctx = context_package.Background();
    var body = slice<byte>("Hello"u8);
    var bodyʗ1 = body;
    var cʗ1 = c;
    var ctxʗ1 = ctx;
    var tsʗ1 = ts;
    [MethodImpl(MethodImplOptions.NoInlining)] void doPosts(ж<contextCounter> cc) {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint n = 64; n > 0; n--) {
            Ꮡwg.Add(1);
            var bodyʗ2 = bodyʗ1;
            var cʗ2 = cʗ1;
            var ctxʗ2 = ctxʗ1;
            var tsʗ2 = tsʗ1;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var ctxΔ1 = cc.Track(ctxʗ2);
                    var (req, err) = NewRequest(postˢ, (~tsʗ2).URL, new http_test_package.bytes_ReaderжReader(bytes.NewReader(bodyʗ2)));
                    if (err != default!) {
                        Ꮡt.Error(err);
                    }
                    (_, err) = cʗ2.Do(req.WithContext(ctxΔ1));
                    if (err != default!) {
                        Ꮡt.Errorf("Do failed with error: %v"u8, err);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    ref var initialCC = ref heap(new contextCounter(), out var ᏑinitialCC);
    doPosts(ᏑinitialCC);
    // flushCC exists only to put pressure on the GC to finalize the initialCC
    // contexts: the flushCC allocations should eventually displace the initialCC
    // allocations.
    ref var flushCC = ref heap(new contextCounter(), out var ᏑflushCC);
    for (nint i = 0; ᐧ ; i++) {
        var live = ᏑinitialCC.Read();
        if (live == 0) {
            break;
        }
        if (i >= 100) {
            Ꮡt.Fatalf("%d Contexts still not finalized after %d GC cycles."u8, live, i);
        }
        doPosts(ᏑflushCC);
        runtime.GC();
    }
}

// This used to crash; https://golang.org/issue/3266
public static void TestTransportIdleConnCrash(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIdleConnCrash(Δp0, Δp1));
}

internal static partial void testTransportIdleConnCrash(ж<testing.T> Ꮡt, testMode mode) {
    ref var tr = ref heap<ж<Δhttp.Transport>>(out var Ꮡtr);
    var unblockCh = new channel<bool>(1);
    var unblockChʗ1 = unblockCh;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ᐸꟷ(unblockChʗ1);
        Ꮡtr.ValueSlot.CloseIdleConnections();
    }))).Value.ts;
    var c = ts.Client();
    tr = (~c).Transport._<ж<Δhttp.Transport>>();
    var didreq = new channel<bool>(0);
    var cʗ1 = c;
    var didreqʗ1 = didreq;
    var tsʗ1 = ts;
    goǃ(() => {
        var (res, err) = cʗ1.Get((~tsʗ1).URL);
        if (err != default!){
            Ꮡt.Error(err);
        } else {
            (~res).Body.Close(); // returns idle conn
        }
        didreqʗ1.ᐸꟷ(true);
    });
    unblockCh.ᐸꟷ(true);
    ᐸꟷ(didreq);
}

// Test that the transport doesn't close the TCP connection early,
// before the response body has been read. This was a regression
// which sadly lacked a triggering test. The large response body made
// the old race easier to trigger.
public static void TestIssue3644(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIssue3644(Δp0, Δp1));
}

internal static void testIssue3644(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        UntypedInt numFoos = 5000;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(connectionˢ, closeˢ);
            for (nint i = 0; i < numFoos; i++) {
                w.Write(slice<byte>("foo "u8));
            }
        }))).Value.ts;
        var c = ts.Client();
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var bs, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(bs) != (nint)((nint)numFoos * len("foo "))) {
            Ꮡt.Errorf("unexpected response length"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that a client receives a server's reply, even if the server doesn't read
// the entire request body.
public static void TestIssue3595(ж<testing.T> Ꮡt) {
    // Not parallel: modifies the global rstAvoidanceDelay.
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIssue3595(Δp0, Δp1), testNotParallel);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string deniedMsgᶜ = "sorry, denied."u8;

internal static void testIssue3595(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    runTimeSensitiveTest(Ꮡt, new time.Duration[]{
        1 * time.Millisecond,
        5 * time.Millisecond,
        10 * time.Millisecond,
        50 * time.Millisecond,
        100 * time.Millisecond,
        500 * time.Millisecond,
        time.ΔSecond,
        (time.Duration)(5000000000L)
    }.slice(), error (ж<testing.T> tΔ1, time.Duration timeout) => {
        GoFrame ᒐ = default;
        try {
            http_internal_test_package.SetRSTAvoidanceDelay(tΔ1, timeout);
            tΔ1.Logf("set RST avoidance delay to %v"u8, timeout);
            @string deniedMsg = deniedMsgᶜ;
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                Error(w, deniedMsg, StatusUnauthorized);
            })));
            // We need to close cst explicitly here so that in-flight server
            // requests don't race with the call to SetRSTAvoidanceDelay for a retry.
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            var c = ts.Client();
            var (res, err) = c.Post((~ts).URL, applicationOctetStreamˢ, ((neverEnding)(rune)'a'));
            if (err != default!) {
                return fmt.Errorf("Post: %v"u8, err);
            }
            (var got, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                return fmt.Errorf("Body ReadAll: %v"u8, err);
            }
            tΔ1.Logf("server response:\n%s"u8, got);
            if (!strings.Contains(((@string)got), deniedMsg)) {
                // If we got an RST packet too early, we should have seen an error
                // from io.ReadAll, not a silently-truncated body.
                tΔ1.Errorf("Known bug: response %q does not contain %q"u8, got, deniedMsg);
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// From https://golang.org/issue/4454 ,
// "client fails to handle requests with no body and chunked encoding"
public static void TestChunkedNoContent(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testChunkedNoContent(Δp0, Δp1));
}

internal static void testChunkedNoContent(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(StatusNoContent);
    }))).Value.ts;
    var c = ts.Client();
    foreach (var (_, closeBody) in new bool[]{true, false}.slice()) {
        const nint n = 4;
        for (nint i = 1; i <= n; i++) {
            var (res, err) = c.Get((~ts).URL);
            if (err != default!){
                Ꮡt.Errorf("closingBody=%v, req %d/%d: %v"u8, closeBody, i, (nint)(n), err);
            } else {
                if (closeBody) {
                    (~res).Body.Close();
                }
            }
        }
    }
}

public static void TestTransportConcurrency(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportConcurrency(Δp0, Δp1), testNotParallel, new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string echoˢ = "echo"u8;
internal static readonly @string connectionResetByPeerˢ = ": connection reset by peer"u8;

internal static partial void testTransportConcurrency(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        // Not parallel: uses global test hooks.
        nint maxProcs = 16;
        nint numReqs = 500;
        if (testing.Short()) {
            (maxProcs, numReqs) = (4, 50);
        }
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(maxProcs), ref ᒐ);
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%v"u8, r.FormValue(echoˢ));
        }))).Value.ts;
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(numReqs);
        // Due to the Transport's "socket late binding" (see
        // idleConnCh in transport.go), the numReqs HTTP requests
        // below can finish with a dial still outstanding. To keep
        // the leak checker happy, keep track of pending dials and
        // wait for them to finish (and be closed or returned to the
        // idle pool) before we close idle connections.
        http_internal_test_package.SetPendingDialHooks(() => {
            Ꮡwg.Add(1);
        }, Ꮡwg.Done);
        defer(http_internal_test_package.SetPendingDialHooks, (Action)(default!), (Action)(default!), ref ᒐ);
        var c = ts.Client();
        var reqs = new channel<@string>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), reqs, ref ᒐ);
        for (nint i = 0; i < maxProcs * 2; i++) {
            var cʗ1 = c;
            var reqsʗ1 = reqs;
            var tsʗ1 = ts;
            goǃ(() => {
                foreach (var req in reqsʗ1) {
                    var (res, err) = cʗ1.Get((~tsʗ1).URL + "/?echo="u8 + req);
                    if (err != default!) {
                        if (runtime.GOOS == "netbsd"u8 && strings.HasSuffix(err.Error(), connectionResetByPeerˢ)){
                            // https://go.dev/issue/52168: this test was observed to fail with
                            // ECONNRESET errors in Dial on various netbsd builders.
                            Ꮡt.Logf("error on req %s: %v"u8, req, err);
                            Ꮡt.Logf("(see https://go.dev/issue/52168)"u8);
                        } else {
                            Ꮡt.Errorf("error on req %s: %v"u8, req, err);
                        }
                        Ꮡwg.Done();
                        continue;
                    }
                    (var all, err) = io.ReadAll((~res).Body);
                    if (err != default!){
                        Ꮡt.Errorf("read error on req %s: %v"u8, req, err);
                    } else 
                    if (((sstring)all) != req) {
                        Ꮡt.Errorf("body of req %s = %q; want %q"u8, req, all, req);
                    }
                    (~res).Body.Close();
                    Ꮡwg.Done();
                }
            });
        }
        for (nint i = 0; i < numReqs; i++) {
            reqs.ᐸꟷ(fmt.Sprintf("request-%d"u8, i));
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue4191_InfiniteGetTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIssue4191_InfiniteGetTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ6 = "/get"u8;

internal static void testIssue4191_InfiniteGetTimeout(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var mux = NewServeMux();
        mux.HandleFunc(getˢ6, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), ((neverEnding)(rune)'a'));
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
        var connc = new channel<net.Conn>(1);
        var c = ts.Client();
        var conncʗ1 = connc;
        (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = (net.Conn, error) (@string n, @string addr) => {
            var (connΔ1, errΔ1) = net.Dial(n, addr);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            var selᴛ71 = conncʗ1.ᐸꟷ(connΔ1, ꓸꓸꓸ);
            switch (trySelect(selᴛ71)) {
            case 0: {
                break;
            }
            default: {
                break;
            }}
            return (connΔ1, default!);
        };
        var (res, err) = c.Get((~ts).URL + "/get"u8);
        if (err != default!) {
            Ꮡt.Fatalf("Error issuing GET: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        var conn = ᐸꟷ(connc);
        conn.SetDeadline(time.Now().Add(1 * time.Millisecond));
        (_, err) = io.Copy(io.Discard, (~res).Body);
        if (err == default!) {
            Ꮡt.Errorf("Unexpected successful copy"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue4191_InfiniteGetToPutTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIssue4191_InfiniteGetToPutTimeout(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string putˢ2 = "/put"u8;
internal static readonly @string clientˢ = "client"u8;

internal static void testIssue4191_InfiniteGetToPutTimeout(ж<testing.T> Ꮡt, testMode mode) {
    const bool debug = false;
    var mux = NewServeMux();
    mux.HandleFunc(getˢ6, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), ((neverEnding)(rune)'a'));
    });
    mux.HandleFunc(putˢ2, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            defer(() => (~r).Body.Close(), ref ᒐ);
            io.Copy(io.Discard, (~r).Body);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
    var timeout = 100 * time.Millisecond;
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = (net.Conn, error) (@string n, @string addr) => {
        var (conn, err) = net.Dial(n, addr);
        if (err != default!) {
            return (default!, err);
        }
        conn.SetDeadline(time.Now().Add(timeout));
        if (debug) {
            conn = http_internal_test_package.NewLoggingConn(clientˢ, conn);
        }
        return (conn, default!);
    };
    var getFailed = false;
    nint nRuns = 5;
    if (testing.Short()) {
        nRuns = 1;
    }
    for (nint i = 0; i < nRuns; i++) {
        if (debug) {
            println((@string)"run"u8, i + 1, (@string)"of"u8, nRuns);
        }
        var (sres, err) = c.Get((~ts).URL + "/get"u8);
        if (err != default!) {
            if (!getFailed) {
                // Make the timeout longer, once.
                getFailed = true;
                Ꮡt.Logf("increasing timeout"u8);
                i--;
                timeout *= 10;
                continue;
            }
            Ꮡt.Errorf("Error issuing GET: %v"u8, err);
            break;
        }
        var (req, _) = NewRequest(putˢ, (~ts).URL + "/put"u8, (~sres).Body);
        (_, err) = c.Do(req);
        if (err == default!) {
            (~sres).Body.Close();
            Ꮡt.Errorf("Unexpected successful PUT"u8);
            break;
        }
        (~sres).Body.Close();
    }
    if (debug) {
        println((@string)"tests complete; waiting for handlers to finish"u8);
    }
    ts.Close();
}

public static void TestTransportResponseHeaderTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportResponseHeaderTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTimeoutTestInˢ = (@string)"skipping timeout test in -short mode"u8;
internal static readonly @string fastˢ = "/fast"u8;
internal static readonly @string slowˢ = "/slow"u8;
internal static readonly @string timeoutAwaitingResponseˢ = "timeout awaiting response headers"u8;

[GoType("dyn")] internal partial struct testTransportResponseHeaderTimeout_tests {
    internal @string path;
    internal bool wantTimeout;
}

internal static void testTransportResponseHeaderTimeout(ж<testing.T> Ꮡt, testMode mode) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingTimeoutTestInˢ);
    }
    var timeout = 2 * time.Millisecond;
    var retry = true;
    while (retry && !Ꮡt.Failed()) {
        ref var srvWG = ref heap(new sync.WaitGroup(), out var ᏑsrvWG);
        var inHandler = new channel<bool>(1);
        var mux = NewServeMux();
        var inHandlerʗ1 = inHandler;
        mux.HandleFunc(fastˢ, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            inHandlerʗ1.ᐸꟷ(true);
            ᏑsrvWG.Done();
        });
        var inHandlerʗ2 = inHandler;
        mux.HandleFunc(slowˢ, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            inHandlerʗ2.ᐸꟷ(true);
            ᐸꟷ(r.Context().Done());
            ᏑsrvWG.Done();
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
        var c = ts.Client();
        (~c).Transport._<ж<Δhttp.Transport>>().Value.ResponseHeaderTimeout = timeout;
        retry = false;
        ᏑsrvWG.Add(3);
        var tests = new testTransportResponseHeaderTimeout_tests[]{
            new(path: "/fast"u8),
            new(path: "/slow"u8, wantTimeout: true),
            new(path: "/fast"u8)
        }.slice();
        foreach (var (i, tt) in tests) {
            var (req, _) = NewRequest(getˢ2, (~ts).URL + tt.path, default!);
            req = req.WithT(Ꮡt);
            var (res, err) = c.Do(req);
            ᐸꟷ(inHandler);
            if (err != default!) {
                var (uerr, ok) = err._<ж<urlꓸError>>(ᐧ);
                if (!ok) {
                    Ꮡt.Errorf("error is not a url.Error; got: %#v"u8, err);
                    continue;
                }
                (var nerr, ok) = (~uerr).Err._<netꓸError>(ᐧ);
                if (!ok) {
                    Ꮡt.Errorf("error does not satisfy net.Error interface; got: %#v"u8, err);
                    continue;
                }
                if (!nerr.Timeout()) {
                    Ꮡt.Errorf("want timeout error; got: %q"u8, nerr);
                    continue;
                }
                if (!tt.wantTimeout) {
                    if (!retry) {
                        // The timeout may be set too short. Retry with a longer one.
                        Ꮡt.Logf("unexpected timeout for path %q after %v; retrying with longer timeout"u8, tt.path, timeout);
                        timeout *= 2;
                        retry = true;
                    }
                }
                if (!strings.Contains(err.Error(), timeoutAwaitingResponseˢ)) {
                    Ꮡt.Errorf("%d. unexpected error: %v"u8, i, err);
                }
                continue;
            }
            if (tt.wantTimeout) {
                Ꮡt.Errorf(@"no error for path %q; expected ""timeout awaiting response headers"""u8, tt.path);
                continue;
            }
            if ((~res).StatusCode != 200) {
                Ꮡt.Errorf("%d for path %q status = %d; want 200"u8, i, tt.path, (~res).StatusCode);
            }
        }
        ᏑsrvWG.Wait();
        ts.Close();
    }
}

// A cancelTest is a test of request cancellation.
[GoType] partial struct cancelTest {
    internal testMode mode;
    internal Func<ж<Δhttp.Request>, ж<Δhttp.Request>> newReq;    // prepare the request to cancel
    internal Action<ж<Δhttp.Transport>, ж<Δhttp.Request>> cancel; // cancel the request
    internal Action<@string, error> checkErr;            // verify the expected error
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string transportCancelˢ = "TransportCancel"u8;

// runCancelTestTransport uses Transport.CancelRequest.
internal static void runCancelTestTransport(ж<testing.T> Ꮡt, testMode mode, Action<ж<testing.T>, cancelTest> f) {
    Ꮡt.Run(transportCancelˢ, (ж<testing.T> tΔ1) => {
        f(tΔ1, new cancelTest(
            mode: mode,
            newReq: (ж<Δhttp.Request> req) => req,
            cancel: (ж<Δhttp.Transport> tr, ж<Δhttp.Request> req) => {
                tr.CancelRequest(req);
            },
            checkErr: (@string when, error err) => {
                if (!errors.Is(err, http_internal_test_package.ExportErrRequestCanceled) && !errors.Is(err, http_internal_test_package.ExportErrRequestCanceledConn)) {
                    tΔ1.Errorf("%v error = %v, want errRequestCanceled or errRequestCanceledConn"u8, when, err);
                }
            }
        ));
    });
}

// runCancelTestChannel uses Request.Cancel.
internal static void runCancelTestChannel(ж<testing.T> Ꮡt, testMode mode, Action<ж<testing.T>, cancelTest> f) {
    var cancelc = new channel<EmptyStruct>(0);
    var cancelcʗ1 = cancelc;
    var cancelOnce = sync.OnceFunc(() => {
        builtin.close(cancelcʗ1);
    });
        var cancelcʗ2 = cancelc;

        var cancelOnceʗ1 = cancelOnce;

    f(Ꮡt, new cancelTest(
        mode: mode,
        newReq: (ж<Δhttp.Request> req) => {
            req.Value.Cancel = cancelcʗ2.WithDirection(GoChanDir.Recv);
            return req;
        },
        cancel: (ж<Δhttp.Transport> tr, ж<Δhttp.Request> req) => {
            cancelOnceʗ1();
        },
        checkErr: (@string when, error err) => {
            if (!errors.Is(err, http_internal_test_package.ExportErrRequestCanceled) && !errors.Is(err, http_internal_test_package.ExportErrRequestCanceledConn)) {
                Ꮡt.Errorf("%v error = %v, want errRequestCanceled or errRequestCanceledConn"u8, when, err);
            }
        }
    ));
}

// runCancelTestContext uses a request context.
internal static void runCancelTestContext(ж<testing.T> Ꮡt, testMode mode, Action<ж<testing.T>, cancelTest> f) {
    var (ctx, cancel) = context_package.WithCancel(context_package.Background());
        var ctxʗ1 = ctx;

        var cancelʗ1 = cancel;

    f(Ꮡt, new cancelTest(
        mode: mode,
        newReq: (ж<Δhttp.Request> req) => req.WithContext(ctxʗ1),
        cancel: (ж<Δhttp.Transport> tr, ж<Δhttp.Request> req) => {
            cancelʗ1();
        },
        checkErr: (@string when, error err) => {
            if (!errors.Is(err, context_package.Canceled)) {
                Ꮡt.Errorf("%v error = %v, want context.Canceled"u8, when, err);
            }
        }
    ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string requestCancelˢ = "RequestCancel"u8;
internal static readonly @string contextCancelˢ = "ContextCancel"u8;

internal static void runCancelTest(ж<testing.T> Ꮡt, Action<ж<testing.T>, cancelTest> f, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.sslice();

    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        if (mode == http1Mode) {
            tΔ1.Run(transportCancelˢ, (ж<testing.T> tΔ2) => {
                runCancelTestTransport(tΔ2, mode, f);
            });
        }
        tΔ1.Run(requestCancelˢ, (ж<testing.T> tΔ3) => {
            runCancelTestChannel(tΔ3, mode, f);
        });
        tΔ1.Run(contextCancelˢ, (ж<testing.T> tΔ4) => {
            runCancelTestContext(tΔ4, mode, f);
        });
    }, opts.ꓸꓸꓸ);
}

public static void TestTransportCancelRequest(ж<testing.T> Ꮡt) {
    runCancelTest(Ꮡt, testTransportCancelRequest);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ2 = (@string)"skipping test in -short mode"u8;
internal static readonly @string bodyReadˢ = "Body.Read"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string msgᶜ2 = "Hello"u8;

internal static void testTransportCancelRequest(ж<testing.T> Ꮡt, cancelTest test) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingTestInShortModeˢ2);
        }
        @string msg = msgᶜ2;
        var unblockc = new channel<bool>(0);
        var unblockcʗ1 = unblockc;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), test.mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), msg);
            w._<Flusher>().Flush(); // send headers and some body
            ᐸꟷ(unblockcʗ1);
        }))).Value.ts;
        defer(ᴛ1 => builtin.close(ᴛ1), unblockc, ref ᒐ);
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
        req = test.newReq(req);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var body = new slice<byte>(len(msg));
        var (n, _) = io.ReadFull((~res).Body, body);
        if (n != len(body) || !bytes.Equal(body, slice<byte>(msg))) {
            Ꮡt.Errorf("Body = %q; want %q"u8, body.slice(0, n), msg);
        }
        test.cancel(tr, req);
        (var tail, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        test.checkErr(bodyReadˢ, err);
        if (len(tail) > 0) {
            Ꮡt.Errorf("Spurious bytes from Body.Read: %q"u8, tail);
        }
        // Verify no outstanding requests after readLoop/writeLoop
        // goroutines shut down.
        var trʗ1 = tr;
        waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
            nint nΔ1 = trʗ1.NumPendingRequestsForTesting();
            if (nΔ1 > 0) {
                if (d > 0) {
                    Ꮡt.Logf("pending requests = %d after %v (want 0)"u8, nΔ1, d);
                }
                return false;
            }
            return true;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void testTransportCancelRequestInDo(ж<testing.T> Ꮡt, cancelTest test, io.Reader body) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingTestInShortModeˢ2);
        }
        var unblockc = new channel<bool>(0);
        var unblockcʗ1 = unblockc;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), test.mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ᐸꟷ(unblockcʗ1);
        }))).Value.ts;
        defer(ᴛ1 => builtin.close(ᴛ1), unblockc, ref ᒐ);
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        var donec = new channel<bool>(0);
        var (req, _) = NewRequest(getˢ2, (~ts).URL, body);
        req = test.newReq(req);
        var cʗ1 = c;
        var donecʗ1 = donec;
        var reqʗ1 = req;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), donecʗ1, ref ᒐ);
                cʗ1.Do(reqʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        unblockc.ᐸꟷ(true);
        var donecʗ2 = donec;
        var reqʗ2 = req;
        var testʗ1 = test;
        var trʗ1 = tr;
        waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
            testʗ1.cancel(trʗ1, reqʗ2);
            var selᴛ72 = donecʗ2;
            switch (trySelect(ᐸꟷ(selᴛ72, ꓸꓸꓸ))) {
            case 0 when selᴛ72.ꟷᐳ(out _): {
                return true;
            }
            default: {
                if (d > 0) {
                    Ꮡt.Logf("Do of canceled request has not returned after %v"u8, d);
                }
                return false;
            }}
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportCancelRequestInDo(ж<testing.T> Ꮡt) {
    runCancelTest(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1, cancelTest test) => {
        testTransportCancelRequestInDo(tΔ1, test, default!);
    });
}

public static void TestTransportCancelRequestWithBodyInDo(ж<testing.T> Ꮡt) {
    runCancelTest(Ꮡt, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1, cancelTest test) => {
        testTransportCancelRequestInDo(tΔ1, test, new http_test_package.bytes_BufferжReader(bytes.NewBuffer(new byte[]{0}.slice())));
    });
}

public static void TestTransportCancelRequestInDial(ж<testing.T> Ꮡt) {
    runCancelTest(Ꮡt, testTransportCancelRequestInDial);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object dialBlockingˢ = (@string)"dial: blocking"u8;
internal static readonly @string mainTestGoroutineExitedˢ = "main Test goroutine exited"u8;
internal static readonly @string nopeˢ = "nope"u8;
internal static readonly @string httpSomethingNoNetworkˢ = "http://something.no-network.tld/"u8;
internal static readonly @string getˢ7 = "Get"u8;
internal static readonly @string dialBlockingCancelingGetˢ = """
dial: blocking
canceling
Get error = true

"""u8;

internal static partial void testTransportCancelRequestInDial(ж<testing.T> Ꮡt, cancelTest test) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        if (testing.Short()) {
            Ꮡt.Skip(skippingTestInShortModeˢ2);
        }
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        var eventLog = log.New(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf), ""u8, 0);
        var unblockDial = new channel<bool>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), unblockDial, ref ᒐ);
        var inDial = new channel<bool>(0);
            var eventLogʗ1 = eventLog;
            var inDialʗ1 = inDial;
            var unblockDialʗ1 = unblockDial;
        var tr = Ꮡ(new Transport(
            Dial: (@string network, @string addr) => {
                eventLogʗ1.Println(dialBlockingˢ);
                if (!ᐸꟷ(inDialʗ1)) {
                    return (default!, errors.New(mainTestGoroutineExitedˢ));
                }
                ᐸꟷ(unblockDialʗ1);
                return (default!, errors.New(nopeˢ));
            }
        ));
        var cl = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        var gotres = new channel<bool>(0);
        var (req, _) = NewRequest(getˢ2, httpSomethingNoNetworkˢ, default!);
        req = test.newReq(req);
        var clʗ1 = cl;
        var eventLogʗ2 = eventLog;
        var gotresʗ1 = gotres;
        var reqʗ1 = req;
        var testʗ1 = test;
        goǃ(() => {
            var (_, err) = clʗ1.Do(reqʗ1);
            eventLogʗ2.Printf("Get error = %v"u8, err != default!);
            testʗ1.checkErr(getˢ7, err);
            gotresʗ1.ᐸꟷ(true);
        });
        inDial.ᐸꟷ(true);
        eventLog.Printf("canceling"u8);
        test.cancel(tr, req);
        test.cancel(tr, req); // used to panic on second call to Transport.Cancel
        {
            var (d, ok) = t.Deadline(); if (ok) {
                // When the test's deadline is about to expire, log the pending events for
                // better debugging.
                var timeout = time.Until(d) * 19 / 20; // Allow 5% for cleanup.
                var timer = time.AfterFunc(timeout, () => {
                    throw panic(fmt.Sprintf("hang in %s. events are: %s"u8, Ꮡt.Name(), Ꮡlogbuf.Value.String()));
                });
                var timerʗ1 = timer;
                defer(() => timerʗ1.Stop(), ref ᒐ);
            }
        }
        ᐸꟷ(gotres);
        @string got = logbuf.String();
        @string want = dialBlockingCancelingGetˢ;
        if (got != want) {
            Ꮡt.Errorf("Got events:\n%s\nWant:\n%s"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 51354
public static void TestTransportCancelRequestWithBody(ж<testing.T> Ꮡt) {
    runCancelTest(Ꮡt, testTransportCancelRequestWithBody);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string withbodyˢ = "withbody"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string msgᶜ3 = "Hello"u8;

internal static void testTransportCancelRequestWithBody(ж<testing.T> Ꮡt, cancelTest test) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingTestInShortModeˢ2);
        }
        @string msg = msgᶜ3;
        var unblockc = new channel<EmptyStruct>(0);
        var unblockcʗ1 = unblockc;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), test.mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), msg);
            w._<Flusher>().Flush(); // send headers and some body
            ᐸꟷ(unblockcʗ1);
        }))).Value.ts;
        defer(ᴛ1 => builtin.close(ᴛ1), unblockc, ref ᒐ);
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        var (req, _) = NewRequest(postˢ, (~ts).URL, new http_test_package.strings_ReaderжReader(strings.NewReader(withbodyˢ)));
        req = test.newReq(req);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var body = new slice<byte>(len(msg));
        var (n, _) = io.ReadFull((~res).Body, body);
        if (n != len(body) || !bytes.Equal(body, slice<byte>(msg))) {
            Ꮡt.Errorf("Body = %q; want %q"u8, body.slice(0, n), msg);
        }
        test.cancel(tr, req);
        (var tail, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        test.checkErr(bodyReadˢ, err);
        if (len(tail) > 0) {
            Ꮡt.Errorf("Spurious bytes from Body.Read: %q"u8, tail);
        }
        // Verify no outstanding requests after readLoop/writeLoop
        // goroutines shut down.
        var trʗ1 = tr;
        waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
            nint nΔ1 = trʗ1.NumPendingRequestsForTesting();
            if (nΔ1 > 0) {
                if (d > 0) {
                    Ꮡt.Logf("pending requests = %d after %v (want 0)"u8, nΔ1, d);
                }
                return false;
            }
            return true;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportCancelRequestBeforeDo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We can't cancel a request that hasn't started using Transport.CancelRequest.
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(requestCancelˢ, (ж<testing.T> tΔ2) => {
            runCancelTestChannel(tΔ2, mode, testTransportCancelRequestBeforeDo);
        });
        tΔ1.Run(contextCancelˢ, (ж<testing.T> tΔ3) => {
            runCancelTestContext(tΔ3, mode, testTransportCancelRequestBeforeDo);
        });
    });
}

internal static void testTransportCancelRequestBeforeDo(ж<testing.T> Ꮡt, cancelTest test) {
    GoFrame ᒐ = default;
    try {
        var unblockc = new channel<bool>(0);
        var unblockcʗ1 = unblockc;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), test.mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ᐸꟷ(unblockcʗ1);
        })));
        defer(ᴛ1 => builtin.close(ᴛ1), unblockc, ref ᒐ);
        var c = (~cst).ts.Client();
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        req = test.newReq(req);
        test.cancel((~cst).tr, req);
        var (_, err) = c.Do(req);
        test.checkErr("Do"u8, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 11020. The returned error message should be errRequestCanceled
public static void TestTransportCancelRequestBeforeResponseHeaders(ж<testing.T> Ꮡt) {
    runCancelTest(Ꮡt, testTransportCancelRequestBeforeResponseHeaders, new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string roundTripˢ = "RoundTrip"u8;

internal static partial void testTransportCancelRequestBeforeResponseHeaders(ж<testing.T> Ꮡt, cancelTest test) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var serverConnCh = new channel<net.Conn>(1);
            var serverConnChʗ1 = serverConnCh;
        var tr = Ꮡ(new Transport(
            Dial: (@string network, @string addr) => {
                var (cc, scΔ1) = net.Pipe();
                serverConnChʗ1.ᐸꟷ(scΔ1);
                return (cc, default!);
            }
        ));
        var trʗ1 = tr;
        defer(trʗ1.CloseIdleConnections, ref ᒐ);
        var errc = new channel<error>(1);
        var (req, _) = NewRequest(getˢ2, httpExampleComˢ2, default!);
        req = test.newReq(req);
        var errcʗ1 = errc;
        var reqʗ1 = req;
        var trʗ2 = tr;
        goǃ(() => {
            var (_, errΔ1) = trʗ2.RoundTrip(reqʗ1);
            errcʗ1.ᐸꟷ(errΔ1);
        });
        var sc = ᐸꟷ(serverConnCh);
        var verb = new slice<byte>(3);
        {
            var (_, errΔ2) = io.ReadFull(new http_test_package.net_ConnᴠReader(sc), verb); if (errΔ2 != default!) {
                Ꮡt.Errorf("Error reading HTTP verb from server: %v"u8, errΔ2);
            }
        }
        if (((sstring)verb) != "GET"u8) {
            Ꮡt.Errorf("server received %q; want GET"u8, verb);
        }
        var scʗ1 = sc;
        defer(() => scʗ1.Close(), ref ᒐ);
        test.cancel(tr, req);
        var err = ᐸꟷ(errc);
        if (err == default!) {
            Ꮡt.Fatalf("unexpected success from RoundTrip"u8);
        }
        test.checkErr(roundTripˢ, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/3672 -- Client can't close HTTP stream
// Calling Close on a Response.Body used to just read until EOF.
// Now it actually closes the TCP connection.
public static void TestTransportCloseResponseBody(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportCloseResponseBody(Δp0, Δp1));
}

internal static void testTransportCloseResponseBody(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var writeErr = new channel<error>(1);
        var msg = slice<byte>("young\n"u8);
        var msgʗ1 = msg;
        var writeErrʗ1 = writeErr;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            while (ᐧ) {
                var (_, errΔ1) = w.Write(msgʗ1);
                if (errΔ1 != default!) {
                    writeErrʗ1.ᐸꟷ(errΔ1);
                    return;
                }
                w._<Flusher>().Flush();
            }
        }))).Value.ts;
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
        var trʗ1 = tr;
        defer(trʗ1.CancelRequest, req, ref ᒐ);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        const nint repeats = 3;
        var buf = new slice<byte>(len(msg) * repeats);
        var want = bytes.Repeat(msg, repeats);
        (_, err) = io.ReadFull((~res).Body, buf);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!bytes.Equal(buf, want)) {
            Ꮡt.Fatalf("read %q; want %q"u8, buf, want);
        }
        {
            var errΔ2 = (~res).Body.Close(); if (errΔ2 != default!) {
                Ꮡt.Errorf("Close = %v"u8, errΔ2);
            }
        }
        {
            var errΔ3 = ᐸꟷ(writeErr); if (errΔ3 == default!) {
                Ꮡt.Errorf("expected non-nil write error"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct fooProto {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this fooProto _, ж<Δhttp.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    var res = Ꮡ(new Response(
        Status: "200 OK"u8,
        StatusCode: 200,
        Header: new httpꓸHeader(0),
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader("You wanted "u8 + req.URL.String())))
    ));
    return (res, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBarComPathˢ = "foo://bar.com/path"u8;
internal static readonly @string youWantedFooBarComPathˢ = "You wanted foo://bar.com/path"u8;

public static void TestTransportAltProto(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new Transport(nil));
        var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        tr.RegisterProtocol(fooˢ, new fooProto(nil));
        var (res, err) = c.Get(fooBarComPathˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var bodyb, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string body = ((@string)bodyb);
        {
            @string e = youWantedFooBarComPathˢ; if (body != e) {
                Ꮡt.Errorf("got response %q, want %q"u8, body, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpNoHostInRequestUrlˢ = "http: no Host in request URL"u8;

public static void TestTransportNoHost(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new Transport(nil));
        var (_, err) = tr.RoundTrip(Ꮡ(new Request(
            Header: new httpꓸHeader(0),
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8
            ))
        )));
        @string want = httpNoHostInRequestUrlˢ;
        {
            @string got = fmt.Sprint(err); if (got != want) {
                Ꮡt.Errorf("error = %v; want %q"u8, err, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 13311
public static void TestTransportEmptyMethod(ж<testing.T> Ꮡt) {
    var (req, _) = NewRequest(getˢ2, httpFooComˢ, default!);
    req.Value.Method = ""u8; // docs say "For client requests an empty string means GET"
    var (got, err) = httputil.DumpRequestOut(req, false); // DumpRequestOut uses Transport
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!strings.Contains(((@string)got), getˢ)) {
        Ꮡt.Fatalf("expected substring 'GET '; got: %s"u8, got);
    }
}

public static void TestTransportSocketLateBinding(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportSocketLateBinding(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooIpportˢ = "foo-ipport"u8;
internal static readonly @string barˢ4 = "/bar"u8;
internal static readonly @string barIpportˢ = "bar-ipport"u8;
internal static readonly @string manuallyClosedˢ = "manually closed"u8;
internal static readonly object noAddrOnFooRequestˢ = (@string)"No addr on /foo request"u8;

internal static partial void testTransportSocketLateBinding(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var mux = NewServeMux();
        var fooGate = new channel<bool>(1);
        var fooGateʗ1 = fooGate;
        mux.HandleFunc(fooˢ3, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(fooIpportˢ, (~r).RemoteAddr);
            w._<Flusher>().Flush();
            ᐸꟷ(fooGateʗ1);
        });
        mux.HandleFunc(barˢ4, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(barIpportˢ, (~r).RemoteAddr);
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
        var dialGate = new channel<bool>(1);
        var dialing = new channel<bool>(0);
        var c = ts.Client();
        var dialGateʗ1 = dialGate;
        var dialingʗ1 = dialing;
        (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = (net.Conn, error) (@string n, @string addr) => {
            while (ᐧ) {
                var selᴛ73 = dialGateʗ1;
                var selᴛ74 = dialingʗ1.ᐸꟷ(true, ꓸꓸꓸ);
                switch (select(ᐸꟷ(selᴛ73, ꓸꓸꓸ), selᴛ74)) {
                case 0 when selᴛ73.ꟷᐳ(out var ok): {
                    if (!ok) {
                        return (default!, errors.New(manuallyClosedˢ));
                    }
                    return net.Dial(n, addr);
                }
                case 1: {
                    break;
                }}
            }
        };
        defer(ᴛ1 => builtin.close(ᴛ1), dialGate, ref ᒐ);
        dialGate.ᐸꟷ(true); // only allow one dial
        var (fooRes, err) = c.Get((~ts).URL + "/foo"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string fooAddr = (~fooRes).Header.Get(fooIpportˢ);
        if (fooAddr == ""u8) {
            Ꮡt.Fatal(noAddrOnFooRequestˢ);
        }
        var fooDone = new channel<EmptyStruct>(0);
        var dialingʗ2 = dialing;
        var fooDoneʗ1 = fooDone;
        var fooGateʗ2 = fooGate;
        var fooResʗ1 = fooRes;
        goǃ(() => {
            // We know that the foo Dial completed and reached the handler because we
            // read its header. Wait for the bar request to block in Dial, then
            // let the foo response finish so we can use its connection for /bar.
            if (mode == http2Mode){
                // In HTTP/2 mode, the second Dial won't happen because the protocol
                // multiplexes the streams by default. Just sleep for an arbitrary time;
                // the test should pass regardless of how far the bar request gets by this
                // point.
                var selᴛ75 = dialingʗ2;
                var selᴛ76 = time.After(10 * time.Millisecond);
                switch (select(ᐸꟷ(selᴛ75, ꓸꓸꓸ), ᐸꟷ(selᴛ76, ꓸꓸꓸ))) {
                case 0 when selᴛ75.ꟷᐳ(out _): {
                    Ꮡt.Errorf("unexpected second Dial in HTTP/2 mode"u8);
                    break;
                }
                case 1 when selᴛ76.ꟷᐳ(out _): {
                    break;
                }}
            } else {
                ᐸꟷ(dialingʗ2);
            }
            fooGateʗ2.ᐸꟷ(true);
            io.Copy(io.Discard, (~fooResʗ1).Body);
            (~fooResʗ1).Body.Close();
            builtin.close(fooDoneʗ1);
        });
        var fooDoneʗ2 = fooDone;
        defer(() => {
            ᐸꟷ(fooDoneʗ2);
        }, ref ᒐ);
        (var barRes, err) = c.Get((~ts).URL + "/bar"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string barAddr = (~barRes).Header.Get(barIpportˢ);
        if (barAddr != fooAddr) {
            Ꮡt.Fatalf("/foo came from conn %q; /bar came from %q instead"u8, fooAddr, barAddr);
        }
        (~barRes).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string requestIdˢ = "Request-Id"u8;
internal static readonly @string xWantResponseCodeˢ = "X-Want-Response-Code"u8;
internal static readonly @string continueˢ2 = "100 Continue"u8;
internal static readonly @string echoRequestIdˢ = "Echo-Request-Id"u8;
internal static readonly @string httpDummyTldˢ2 = "http://dummy.tld/"u8;

// Issue 2184
public static void TestTransportReading100Continue(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        const nint numReqs = 5;
        @string reqBody(nint n) => fmt.Sprintf("request body %d"u8, n);
        @string reqID(nint n) => fmt.Sprintf("REQ-ID-%d"u8, n);
        var reqBodyʗ1 = reqBody;
        var reqIDʗ1 = reqID;
        void send100Response(ж<io.PipeWriter> w, ж<io.PipeReader> r) {
            GoFrame ᒐ = default;
            try {
                defer(() => w.Close(), ref ᒐ);
                defer(() => r.Close(), ref ᒐ);
                var br = bufio.NewReader(new io.PipeReaderжReader(r));
                nint n = 0;
                while (ᐧ) {
                    n++;
                    var (req, err) = ReadRequest(br);
                    if (AreEqual(err, io.EOF)) {
                        return;
                    }
                    if (err != default!) {
                        Ꮡt.Error(err);
                        return;
                    }
                    (var slurp, err) = io.ReadAll((~req).Body);
                    if (err != default!) {
                        Ꮡt.Errorf("Server request body slurp: %v"u8, err);
                        return;
                    }
                    @string id = (~req).Header.Get(requestIdˢ);
                    @string resCode = (~req).Header.Get(xWantResponseCodeˢ);
                    if (resCode == ""u8) {
                        resCode = continueˢ2;
                        if (((@string)slurp) != reqBodyʗ1(n)) {
                            Ꮡt.Errorf("Server got %q, %v; want %q"u8, slurp, err, reqBodyʗ1(n));
                        }
                    }
                    @string body = fmt.Sprintf("Response number %d"u8, n);
                    var v = slice<byte>(strings.Replace(fmt.Sprintf("""
HTTP/1.1 %s
Date: Thu, 28 Feb 2013 17:55:41 GMT

HTTP/1.1 200 OK
Content-Type: text/html
Echo-Request-Id: %s
Content-Length: %d

%s
"""u8, resCode, id, len(body), body), "\n"u8, "\r\n"u8, -1));
                    w.Write(v);
                    if (id == reqIDʗ1(numReqs)) {
                        return;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
            var send100Responseʗ1 = send100Response;
        var tr = Ꮡ(new Transport(
            Dial: [MethodImpl(MethodImplOptions.NoInlining)] (@string n, @string addr) => {
                var (sr, sw) = io.Pipe(); // server read/write
                var (cr, cw) = io.Pipe(); // client read/write
                    var cwʗ1 = cw;
                    var swʗ1 = sw;
                var conn = Ꮡ(new rwTestConn(
                    Reader: new io.PipeReaderжReader(cr),
                    Writer: new io.PipeWriterжWriter(sw),
                    closeFunc: () => {
                        swʗ1.Close();
                        cwʗ1.Close();
                        return default!;
                    }
                ));
                var send100Responseʗ2 = send100Responseʗ1;
                goǃ(send100Responseʗ2, cw, sr);
                return (new http_test_package.rwTestConnжConn(conn), default!);
            },
            DisableKeepAlives: false
        ));
        var trʗ1 = tr;
        defer(trʗ1.CloseIdleConnections, ref ᒐ);
        var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        var cʗ1 = c;
        void testResponse(ж<Δhttp.Request> req, @string name, nint wantCode) {
            Ꮡt.Helper();
            var (res, err) = cʗ1.Do(req);
            if (err != default!) {
                Ꮡt.Fatalf("%s: Do: %v"u8, name, err);
            }
            if ((~res).StatusCode != wantCode) {
                Ꮡt.Fatalf("%s: Response Statuscode=%d; want %d"u8, name, (~res).StatusCode, wantCode);
            }
            {
                @string id = (~req).Header.Get(requestIdˢ);
                @string idBack = (~res).Header.Get(echoRequestIdˢ); if (id != ""u8 && id != idBack) {
                    Ꮡt.Errorf("%s: response id %q != request id %q"u8, name, idBack, id);
                }
            }
            (_, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Fatalf("%s: Slurp error: %v"u8, name, err);
            }
        }
        // Few 100 responses, making sure we're not off-by-one.
        for (nint i = 1; i <= numReqs; i++) {
            var (req, _) = NewRequest(postˢ, httpDummyTldˢ2, new http_test_package.strings_ReaderжReader(strings.NewReader(reqBody(i))));
            (~req).Header.Set(requestIdˢ, reqID(i));
            testResponse(req, fmt.Sprintf("100, %d/%d"u8, i, (nint)(numReqs)), 200);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 17739: the HTTP client must ignore any unknown 1xx
// informational responses before the actual response.
public static void TestTransportIgnore1xxResponses(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIgnore1xxResponses(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string code123HeaderMapFooBarˢ = "1xx: code=123, header=map[Foo:[bar]]\nHTTP/1.1 200 OK\r\nContent-Length: 5\r\nBar: baz\r\n\r\nHello"u8;

internal static void testTransportIgnore1xxResponses(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (conn, buf, _) = w._<Hijacker>().Hijack();
            buf.Value.Writer.Value.Write(slice<byte>("HTTP/1.1 123 OneTwoThree\r\nFoo: bar\r\n\r\nHTTP/1.1 200 OK\r\nBar: baz\r\nContent-Length: 5\r\n\r\nHello"u8));
            buf.Value.Writer.Value.Flush();
            conn.Close();
        })));
        cst.Value.tr.Value.DisableKeepAlives = true; // prevent log spam; our test server is hanging up anyway
        ref var got = ref heap(new strings.Builder(), out var Ꮡgot);
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        req = req.WithContext(httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
            Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
                fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡgot), "1xx: code=%v, header=%v\n"u8, code, header);
                return default!;
            }
        ))));
        var (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        res.Write(new http_test_package.strings_BuilderжWriter(Ꮡgot));
        @string want = code123HeaderMapFooBarˢ;
        if (got.String() != want) {
            Ꮡt.Errorf(" got: %q\nwant: %q\n"u8, got.String(), want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportLimits1xxResponses(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportLimits1xxResponses(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xHeaderˢ = "X-Header"u8;

internal static void testTransportLimits1xxResponses(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Add(xHeaderˢ, strings.Repeat("a"u8, 100));
        for (nint i = 0; i < 10; i++) {
            w.WriteHeader(123);
        }
        w.WriteHeader(204);
    })));
    cst.Value.tr.Value.DisableKeepAlives = true; // prevent log spam; our test server is hanging up anyway
    cst.Value.tr.Value.MaxResponseHeaderBytes = 1000;
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err == default!) {
        (~res).Body.Close();
        Ꮡt.Fatalf("RoundTrip succeeded; want error"u8);
    }
    foreach (var (_, want) in new @string[]{
        "response headers exceeded"u8,
        "too many 1xx"u8,
        "header list too large"u8
    }.slice()) {
        if (strings.Contains(err.Error(), want)) {
            return;
        }
    }
    Ꮡt.Errorf(@"got error %q; want ""response headers exceeded"" or ""too many 1xx"""u8, err);
}

public static void TestTransportDoesNotLimitDelivered1xxResponses(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportDoesNotLimitDelivered1xxResponses(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skipUntilXNetHttp2ˢ = (@string)"skip until x/net/http2 updated"u8;

internal static void testTransportDoesNotLimitDelivered1xxResponses(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        Ꮡt.Skip(skipUntilXNetHttp2ˢ);
    }
    const nint num1xx = 10;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Add(xHeaderˢ, strings.Repeat("a"u8, 100));
        for (nint i = 0; i < 10; i++) {
            w.WriteHeader(123);
        }
        w.WriteHeader(204);
    })));
    cst.Value.tr.Value.DisableKeepAlives = true; // prevent log spam; our test server is hanging up anyway
    cst.Value.tr.Value.MaxResponseHeaderBytes = 1000;
    nint got1xx = 0;
    var ctx = httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
        Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
            got1xx++;
            return default!;
        }
    )));
    var (req, _) = NewRequestWithContext(ctx, getˢ2, (~(~cst).ts).URL, default!);
    var (res, err) = (~cst).c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    if (got1xx != num1xx) {
        Ꮡt.Errorf("Got %v 1xx responses, want %x"u8, got1xx, (nint)(num1xx));
    }
}

// Issue 26161: the HTTP client must treat 101 responses
// as the final response.
public static void TestTransportTreat101Terminal(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportTreat101Terminal(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportTreat101Terminal(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (conn, buf, _) = w._<Hijacker>().Hijack();
            buf.Value.Writer.Value.Write(slice<byte>("HTTP/1.1 101 Switching Protocols\r\n\r\n"u8));
            buf.Value.Writer.Value.Write(slice<byte>("HTTP/1.1 204 No Content\r\n\r\n"u8));
            buf.Value.Writer.Value.Flush();
            conn.Close();
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != StatusSwitchingProtocols) {
            Ꮡt.Errorf("StatusCode = %v; want 101 Switching Protocols"u8, (~res).StatusCode);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct proxyFromEnvTest {
    internal @string req; // URL to fetch; blank means "http://example.com"
    internal @string env; // HTTP_PROXY
    internal @string httpsenv; // HTTPS_PROXY
    internal @string noenv; // NO_PROXY
    internal @string reqmeth; // REQUEST_METHOD
    internal @string want;
    internal error wanterr;
}

internal static @string String(this proxyFromEnvTest t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    void space() {
        if (Ꮡbuf.Value.Len() > 0) {
            Ꮡbuf.WriteByte((rune)' ');
        }
    }
    if (t.env != ""u8) {
        fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), "http_proxy=%q"u8, t.env);
    }
    if (t.httpsenv != ""u8) {
        space();
        fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), "https_proxy=%q"u8, t.httpsenv);
    }
    if (t.noenv != ""u8) {
        space();
        fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), "no_proxy=%q"u8, t.noenv);
    }
    if (t.reqmeth != ""u8) {
        space();
        fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), "request_method=%q"u8, t.reqmeth);
    }
    @string req = httpExampleComˢ;
    if (t.req != ""u8) {
        req = t.req;
    }
    space();
    fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), "req=%q"u8, req);
    return strings.TrimSpace(buf.String());
}

// Don't use secure for http
// Use secure for https.
// Issue 16405: don't use HTTP_PROXY in a CGI environment,
// where HTTP_PROXY can be attacker-controlled.
internal static slice<proxyFromEnvTest> proxyFromEnvTests = new proxyFromEnvTest[]{
    new(env: "127.0.0.1:8080"u8, want: "http://127.0.0.1:8080"u8),
    new(env: "cache.corp.example.com:1234"u8, want: "http://cache.corp.example.com:1234"u8),
    new(env: "cache.corp.example.com"u8, want: "http://cache.corp.example.com"u8),
    new(env: "https://cache.corp.example.com"u8, want: "https://cache.corp.example.com"u8),
    new(env: "http://127.0.0.1:8080"u8, want: "http://127.0.0.1:8080"u8),
    new(env: "https://127.0.0.1:8080"u8, want: "https://127.0.0.1:8080"u8),
    new(env: "socks5://127.0.0.1"u8, want: "socks5://127.0.0.1"u8),
    new(env: "socks5h://127.0.0.1"u8, want: "socks5h://127.0.0.1"u8),
    new(req: "http://insecure.tld/"u8, env: "http.proxy.tld"u8, httpsenv: "secure.proxy.tld"u8, want: "http://http.proxy.tld"u8),
    new(req: "https://secure.tld/"u8, env: "http.proxy.tld"u8, httpsenv: "secure.proxy.tld"u8, want: "http://secure.proxy.tld"u8),
    new(req: "https://secure.tld/"u8, env: "http.proxy.tld"u8, httpsenv: "https://secure.proxy.tld"u8, want: "https://secure.proxy.tld"u8),
    new(env: "http://10.1.2.3:8080"u8, reqmeth: "POST"u8,
        want: "<nil>"u8,
        wanterr: errors.New("refusing to use HTTP_PROXY value in CGI environment; see golang.org/s/cgihttpproxy"u8)),
    new(want: "<nil>"u8),
    new(noenv: "example.com"u8, req: "http://example.com/"u8, env: "proxy"u8, want: "<nil>"u8),
    new(noenv: ".example.com"u8, req: "http://example.com/"u8, env: "proxy"u8, want: "http://proxy"u8),
    new(noenv: "ample.com"u8, req: "http://example.com/"u8, env: "proxy"u8, want: "http://proxy"u8),
    new(noenv: "example.com"u8, req: "http://foo.example.com/"u8, env: "proxy"u8, want: "<nil>"u8),
    new(noenv: ".foo.com"u8, req: "http://example.com/"u8, env: "proxy"u8, want: "http://proxy"u8)
}.slice();

internal static void testProxyForRequest(ж<testing.T> Ꮡt, proxyFromEnvTest tt, Func<ж<Δhttp.Request>, (ж<url.URL>, error)> proxyForRequest) {
    Ꮡt.Helper();
    @string reqURL = tt.req;
    if (reqURL == ""u8) {
        reqURL = httpExampleComˢ;
    }
    var (req, _) = NewRequest(getˢ2, reqURL, default!);
    var (urlΔ1, err) = proxyForRequest(req);
    {
        @string g = fmt.Sprintf("%v"u8, err);
        @string e = fmt.Sprintf("%v"u8, tt.wanterr); if (g != e) {
            Ꮡt.Errorf("%v: got error = %q, want %q"u8, tt, g, e);
            return;
        }
    }
    {
        @string got = fmt.Sprintf("%s"u8, urlΔ1.OrTypedNil()); if (got != tt.want) {
            Ꮡt.Errorf("%v: got URL = %q, want %q"u8, tt, urlΔ1.OrTypedNil(), tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpProxyˢ = "HTTP_PROXY"u8;
internal static readonly @string httpsProxyˢ = "HTTPS_PROXY"u8;
internal static readonly @string noProxyˢ = "NO_PROXY"u8;
internal static readonly @string requestMethodˢ = "REQUEST_METHOD"u8;

public static void TestProxyFromEnvironment(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        http_internal_test_package.ResetProxyEnv();
        defer(http_internal_test_package.ResetProxyEnv, ref ᒐ);
        foreach (var (_, vᴛ1) in proxyFromEnvTests) {
            ref var tt = ref heap(new proxyFromEnvTest(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            testProxyForRequest(Ꮡt, tt, (ж<Δhttp.Request> req) => {
                os.Setenv(httpProxyˢ, ttʗ1.env);
                os.Setenv(httpsProxyˢ, ttʗ1.httpsenv);
                os.Setenv(noProxyˢ, ttʗ1.noenv);
                os.Setenv(requestMethodˢ, ttʗ1.reqmeth);
                http_internal_test_package.ResetCachedEnvironment();
                return ProxyFromEnvironment(req);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpProxyˢ2 = "http_proxy"u8;
internal static readonly @string httpsProxyˢ2 = "https_proxy"u8;
internal static readonly @string noProxyˢ2 = "no_proxy"u8;

public static void TestProxyFromEnvironmentLowerCase(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        http_internal_test_package.ResetProxyEnv();
        defer(http_internal_test_package.ResetProxyEnv, ref ᒐ);
        foreach (var (_, vᴛ1) in proxyFromEnvTests) {
            ref var tt = ref heap(new proxyFromEnvTest(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            testProxyForRequest(Ꮡt, tt, (ж<Δhttp.Request> req) => {
                os.Setenv(httpProxyˢ2, ttʗ1.env);
                os.Setenv(httpsProxyˢ2, ttʗ1.httpsenv);
                os.Setenv(noProxyˢ2, ttʗ1.noenv);
                os.Setenv(requestMethodˢ, ttʗ1.reqmeth);
                http_internal_test_package.ResetCachedEnvironment();
                return ProxyFromEnvironment(req);
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIdleConnChannelLeak(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIdleConnChannelLeak(Δp0, Δp1), new testMode[]{http1Mode}.slice(), testNotParallel);
}

internal static void testIdleConnChannelLeak(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        // Not parallel: uses global test hooks.
        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
        nint n = default!;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            Ꮡmu.Lock();
            n++;
            Ꮡmu.Unlock();
        }))).Value.ts;
        const nint nReqs = 5;
        var didRead = new channel<bool>(nReqs);
        var didReadʗ1 = didRead;
        http_internal_test_package.SetReadLoopBeforeNextReadHook(() => {
            didReadʗ1.ᐸꟷ(true);
        });
        defer(http_internal_test_package.SetReadLoopBeforeNextReadHook, (Action)(default!), ref ᒐ);
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        var tsʗ1 = ts;
        tr.Value.Dial = (@string netw, @string addr) => net.Dial(netw, (~tsʗ1).Listener.Addr().String());
        // First, without keep-alives.
        foreach (var (_, disableKeep) in new bool[]{true, false}.slice()) {
            tr.Value.DisableKeepAlives = disableKeep;
            for (nint i = 0; i < nReqs; i++) {
                var (_, err) = c.Get(fmt.Sprintf("http://foo-host-%d.tld/"u8, i));
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
            // Note: no res.Body.Close is needed here, since the
            // response Content-Length is zero. Perhaps the test
            // should be more explicit and use a HEAD, but tests
            // elsewhere guarantee that zero byte responses generate
            // a "Content-Length: 0" instead of chunking.
            // At this point, each of the 5 Transport.readLoop goroutines
            // are scheduling noting that there are no response bodies (see
            // earlier comment), and are then calling putIdleConn, which
            // decrements this count. Usually that happens quickly, which is
            // why this test has seemed to work for ages. But it's still
            // racey: we have wait for them to finish first. See Issue 10427
            for (nint i = 0; i < nReqs; i++) {
                ᐸꟷ(didRead);
            }
            {
                nint got = tr.IdleConnWaitMapSizeForTesting(); if (got != 0) {
                    Ꮡt.Fatalf("for DisableKeepAlives = %v, map size = %d; want 0"u8, disableKeep, got);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Verify the status quo: that the Client.Post function coerces its
// body into a ReadCloser if it's a Closer, and that the Transport
// then closes it.
public static void TestTransportClosesRequestBody(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportClosesRequestBody(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportClosesRequestBody(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.Copy(io.Discard, (~r).Body);
    }))).Value.ts;
    var c = ts.Client();
    ref var closes = ref heap<nint>(out var Ꮡcloses);
    closes = 0;
    var (res, err) = c.Post((~ts).URL, textPlainˢ, new countCloseReader(Ꮡcloses, new http_test_package.strings_ReaderжReader(strings.NewReader(helloˢ3))));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    if (closes != 1) {
        Ꮡt.Errorf("closes = %d; want 1"u8, closes);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsDummyTldˢ = "https://dummy.tld/"u8;
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;
internal static readonly @string handshakeTimeoutˢ = "handshake timeout"u8;

public static partial void TestTransportTLSHandshakeTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var testdonec = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), testdonec, ref ᒐ);
        var lnʗ2 = ln;
        var testdonecʗ1 = testdonec;
        goǃ(() => {
            var (c, errΔ1) = lnʗ2.Accept();
            if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
                return;
            }
            ᐸꟷ(testdonecʗ1);
            c.Close();
        });
            var lnʗ3 = ln;
        var tr = Ꮡ(new Transport(
            Dial: (@string _Δp0, @string _Δp1) => net.Dial(tcpˢ, lnʗ3.Addr().String()),
            TLSHandshakeTimeout: 250 * time.Millisecond
        ));
        var cl = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        var (_, err) = cl.Get(httpsDummyTldˢ);
        if (err == default!) {
            Ꮡt.Error(expectedErrorˢ);
            return;
        }
        var (ue, ok) = err._<ж<urlꓸError>>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("expected url.Error; got %#v"u8, err);
            return;
        }
        (var ne, ok) = (~ue).Err._<netꓸError>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("expected net.Error; got %#v"u8, err);
            return;
        }
        if (!ne.Timeout()) {
            Ꮡt.Errorf("expected timeout error; got %v"u8, err);
        }
        if (!strings.Contains(err.Error(), handshakeTimeoutˢ)) {
            Ꮡt.Errorf("expected 'handshake timeout' in error; got %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Trying to repro golang.org/issue/3514
public static void TestTLSServerClosesConnection(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTLSServerClosesConnection(Δp0, Δp1), new testMode[]{https1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keepAliveThenDieˢ = "/keep-alive-then-die"u8;

internal static void testTLSServerClosesConnection(ж<testing.T> Ꮡt, testMode mode) {
    var closedc = new channel<bool>(1);
    var closedcʗ1 = closedc;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if (strings.Contains((~(~r).URL).Path, keepAliveThenDieˢ)) {
            var (conn, _, _) = w._<Hijacker>().Hijack();
            conn.Write(slice<byte>("HTTP/1.1 200 OK\r\nContent-Length: 3\r\n\r\nfoo"u8));
            conn.Close();
            closedcʗ1.ᐸꟷ(true);
            return;
        }
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "hello"u8);
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    nint nSuccess = 0;
    slice<error> errs = default!;
    const nint trials = 20;
    for (nint i = 0; i < trials; i++) {
        tr.CloseIdleConnections();
        var (res, err) = c.Get((~ts).URL + "/keep-alive-then-die"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ᐸꟷ(closedc);
        (var slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)slurp) != "foo"u8) {
            Ꮡt.Errorf("Got %q, want foo"u8, slurp);
        }
        // Now try again and see if we successfully
        // pick a new connection.
        (res, err) = c.Get((~ts).URL + "/"u8);
        if (err != default!) {
            errs = append(errs, err);
            continue;
        }
        (slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            errs = append(errs, err);
            continue;
        }
        nSuccess++;
    }
    if (nSuccess > 0){
        Ꮡt.Logf("successes = %d of %d"u8, nSuccess, (nint)(trials));
    } else {
        Ꮡt.Errorf("All runs failed:"u8);
    }
    foreach (var (_, err) in errs) {
        Ꮡt.Logf("  err: %v"u8, err);
    }
}

[GoType("chan byte")] partial struct byteFromChanReader;

internal static (nint n, error err) Read(this byteFromChanReader c, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (len(p) == 0) {
        return (n, err);
    }
    var (b, ok) = ᐸꟷ<byte>(c, ꟷ);
    if (!ok) {
        return (0, io.EOF);
    }
    p[0] = b;
    return (1, default!);
}

// Verifies that the Transport doesn't reuse a connection in the case
// where the server replies before the request has been fully
// written. We still honor that reply (see TestIssue3595), but don't
// send future requests on the connection because it's then in a
// questionable state.
// golang.org/issue/7569
public static void TestTransportNoReuseAfterEarlyResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportNoReuseAfterEarlyResponse(Δp0, Δp1), new testMode[]{http1Mode}.slice(), testNotParallel);
}

[GoType("dyn")] internal partial struct testTransportNoReuseAfterEarlyResponse_sconn {
    public partial ref global::go.sync_package.Mutex Mutex { get; }
    internal net.Conn c;
}

internal static void testTransportNoReuseAfterEarlyResponse(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        defer((time.Duration d) => {
            http_internal_test_package.MaxWriteWaitBeforeConnReuse.Value = d;
        }, http_internal_test_package.MaxWriteWaitBeforeConnReuse.Value, ref ᒐ);
        http_internal_test_package.MaxWriteWaitBeforeConnReuse.Value = 10 * time.Millisecond;
        ref var sconn = ref heap(new testTransportNoReuseAfterEarlyResponse_sconn(), out var Ꮡsconn);
        bool getOkay = default!;
        ref var copying = ref heap(new sync.WaitGroup(), out var Ꮡcopying);
        void closeConn() {
            GoFrame ᒐ = default;
            try {
                Ꮡsconn.of(testTransportNoReuseAfterEarlyResponse_sconn.ᏑMutex).Lock();
                defer(Ꮡsconn.of(testTransportNoReuseAfterEarlyResponse_sconn.ᏑMutex).Unlock, ref ᒐ);
                if (Ꮡsconn.Value.c != default!) {
                    Ꮡsconn.Value.c.Close();
                    Ꮡsconn.Value.c = default!;
                    if (!getOkay) {
                        Ꮡt.Logf("Closed server connection"u8);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        var closeConnʗ1 = closeConn;
        defer(() => {
            closeConnʗ1();
            Ꮡcopying.Wait();
        }, ref ᒐ);
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~r).Method == "GET"u8) {
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), barˢ);
                return;
            }
            var (conn, _, _) = w._<Hijacker>().Hijack();
            Ꮡsconn.of(testTransportNoReuseAfterEarlyResponse_sconn.ᏑMutex).Lock();
            Ꮡsconn.Value.c = conn;
            Ꮡsconn.of(testTransportNoReuseAfterEarlyResponse_sconn.ᏑMutex).Unlock();
            conn.Write(slice<byte>("HTTP/1.1 200 OK\r\nContent-Length: 3\r\n\r\nfoo"u8)); // keep-alive
            Ꮡcopying.Add(1);
            var connʗ1 = conn;
            goǃ(() => {
                io.Copy(io.Discard, new http_test_package.net_ConnᴠReader(connʗ1));
                Ꮡcopying.Done();
            });
        }))).Value.ts;
        var c = ts.Client();
        UntypedInt bodySize = /* 256 << 10 */ 262144;
        var finalBit = new byteFromChanReader(1);
        var (req, _) = NewRequest(postˢ, (~ts).URL, io.MultiReader(io.LimitReader(((neverEnding)(rune)'x'), bodySize - 1), finalBit));
        req.Value.ContentLength = bodySize;
        var (res, err) = c.Do(req);
        {
            var errΔ1 = wantBody(res, err, fooˢ); if (errΔ1 != default!) {
                Ꮡt.Errorf("POST response: %v"u8, errΔ1);
            }
        }
        (res, err) = c.Get((~ts).URL);
        {
            var errΔ2 = wantBody(res, err, barˢ); if (errΔ2 != default!) {
                Ꮡt.Errorf("GET response: %v"u8, errΔ2);
                return;
            }
        }
        getOkay = true; // suppress test noise
        finalBit.ᐸꟷ((rune)'x'); // unblock the writeloop of the first Post
        builtin.close<byte>(finalBit);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that we don't leak Transport persistConn.readLoop goroutines
// when a server hangs up immediately after saying it would keep-alive.
public static void TestTransportIssue10457(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIssue10457(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportIssue10457(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            // Send a response with no body, keep-alive
            // (implicit), and then lie and immediately close the
            // connection. This forces the Transport's readLoop to
            // immediately Peek an io.EOF and get to the point
            // that used to hang.
            var (conn, _, _) = w._<Hijacker>().Hijack();
            conn.Write(slice<byte>("HTTP/1.1 200 OK\r\nFoo: Bar\r\nContent-Length: 0\r\n\r\n"u8)); // keep-alive
            conn.Close();
        }))).Value.ts;
        var c = ts.Client();
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        // Just a sanity check that we at least get the response. The real
        // test here is that the "defer afterTest" above doesn't find any
        // leaked goroutines.
        {
            @string got = (~res).Header.Get(fooˢ2);
            @string want = barˢ2; if (got != want) {
                Ꮡt.Errorf("Foo header = %q; want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal delegate error closerFunc();

internal static partial error Close(this closerFunc f) {
    return f();
}

[GoType] partial struct writerFuncConn {
    [GoEmbedded] public net_package.Conn Conn;
    internal Func<slice<byte>, (nint n, error err)> write;
}

internal static (nint n, error err) Write(this writerFuncConn c, slice<byte> p) {
    return c.write(p);
}

// Issues 4677, 18241, and 17844. If we try to reuse a connection that the
// server is in the process of closing, we may end up successfully writing out
// our request (or a portion of our request) only to find a connection error
// when we try to read from (or finish writing to) the socket.
//
// NOTE: we resend a request only if:
//   - we reused a keep-alive connection
//   - we haven't yet received any header data
//   - either we wrote no bytes to the server, or the request is idempotent
//
// This automatically prevents an infinite resend loop because we'll run out of
// the cached keep-alive connections eventually.
public static void TestRetryRequestsOnError(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRetryRequestsOnError(Δp0, Δp1), testNotParallel, new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFakeGolangˢ = "http://fake.golang"u8;
internal static readonly @string fooˢ7 = "foo\n"u8;
internal static readonly @string secondWriteFailsˢ = "second write fails"u8;
internal static readonly @string xStatusˢ = "X-Status"u8;

[GoType("dyn")] internal partial struct testRetryRequestsOnError_testCases {
    internal @string name;
    internal nint failureN;
    internal error failureErr;
    // Note that we can't just re-use the Request object across calls to c.Do
    // because we need to rewind Body between calls.  (GetBody is only used to
    // rewind Body on failure and redirects, not just because it's done.)
    internal Func<ж<Δhttp.Request>> req;
    internal @string reqString;
}

internal static void testRetryRequestsOnError(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<Δhttp.Request> newRequest(@string method, @string urlStr, io.Reader body) {
        var (req, err) = NewRequest(method, urlStr, body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        return req;
    }
            var newRequestʗ1 = newRequest;

            var newRequestʗ2 = newRequest;

            var newRequestʗ3 = newRequest;

            var newRequestʗ4 = newRequest;
    var testCases = new testRetryRequestsOnError_testCases[]{
        new(
            name: "IdempotentNoBodySomeWritten"u8, // Believe that we've written some bytes to the server, so we know we're
 // not just in the "retry when no bytes sent" case".

            failureN: 1, // Use the specific error that shouldRetryRequest looks for with idempotent requests.

            failureErr: http_internal_test_package.ExportErrServerClosedIdle,
            req: () => newRequestʗ1(getˢ2, httpFakeGolangˢ, default!),
            reqString: @"GET / HTTP/1.1\r\nHost: fake.golang\r\nUser-Agent: Go-http-client/1.1\r\nAccept-Encoding: gzip\r\n\r\n"u8
        ),
        new(
            name: "IdempotentGetBodySomeWritten"u8, // Believe that we've written some bytes to the server, so we know we're
 // not just in the "retry when no bytes sent" case".

            failureN: 1, // Use the specific error that shouldRetryRequest looks for with idempotent requests.

            failureErr: http_internal_test_package.ExportErrServerClosedIdle,
            req: () => newRequestʗ2(getˢ2, httpFakeGolangˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(fooˢ7))),
            reqString: @"GET / HTTP/1.1\r\nHost: fake.golang\r\nUser-Agent: Go-http-client/1.1\r\nContent-Length: 4\r\nAccept-Encoding: gzip\r\n\r\nfoo\n"u8
        ),
        new(
            name: "NothingWrittenNoBody"u8, // It's key that we return 0 here -- that's what enables Transport to know
 // that nothing was written, even though this is a non-idempotent request.

            failureN: 0,
            failureErr: errors.New(secondWriteFailsˢ),
            req: () => newRequestʗ3(deleteˢ, httpFakeGolangˢ, default!),
            reqString: @"DELETE / HTTP/1.1\r\nHost: fake.golang\r\nUser-Agent: Go-http-client/1.1\r\nAccept-Encoding: gzip\r\n\r\n"u8
        ),
        new(
            name: "NothingWrittenGetBody"u8, // It's key that we return 0 here -- that's what enables Transport to know
 // that nothing was written, even though this is a non-idempotent request.

            failureN: 0,
            failureErr: errors.New(secondWriteFailsˢ), // Note that NewRequest will set up GetBody for strings.Reader, which is
 // required for the retry to occur

            req: () => newRequestʗ4(postˢ, httpFakeGolangˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(fooˢ7))),
            reqString: @"POST / HTTP/1.1\r\nHost: fake.golang\r\nUser-Agent: Go-http-client/1.1\r\nContent-Length: 4\r\nAccept-Encoding: gzip\r\n\r\nfoo\n"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new testRetryRequestsOnError_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
                ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
                void logf(@string format, params ꓸꓸꓸany argsʗp) {
                    GoFrame ᒐ = default;
                    try {
                        var args = argsʗp.sslice();
                        Ꮡmu.Lock();
                        defer(Ꮡmu.Unlock, ref ᒐ);
                        fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf), format, args.ꓸꓸꓸ);
                        Ꮡlogbuf.WriteByte((rune)'\n');
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }
                var logfʗ1 = logf;
                var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                    logfʗ1("Handler"u8);
                    w.Header().Set(xStatusˢ, "ok"u8);
                }))).Value.ts;
                ref var writeNumAtomic = ref heap(new int32(), out var ᏑwriteNumAtomic);
                var c = ts.Client();
                var logfʗ2 = logf;
                var tcʗ2 = tcʗ1;
                var tsʗ1 = ts;
                (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = (net.Conn, error) (@string network, @string addr) => {
                    logfʗ2("Dial"u8);
                    var (cΔ1, err) = net.Dial(network, (~tsʗ1).Listener.Addr().String());
                    if (err != default!) {
                        logfʗ2("Dial error: %v"u8, err);
                        return (default!, err);
                    }
                        var cʗ1 = cΔ1;
                        var logfʗ3 = logfʗ2;
                        var tcʗ3 = tcʗ2;
                    return (new http_test_package.writerFuncConnжConn(Ꮡ(new writerFuncConn(
                        Conn: cΔ1,
                        write: (slice<byte> p) => {
                            if (atomic.AddInt32(ᏑwriteNumAtomic, 1) == 2) {
                                logfʗ3("intentional write failure"u8);
                                return (tcʗ3.failureN, tcʗ3.failureErr);
                            }
                            logfʗ3("Write(%q)"u8, p);
                            return cʗ1.Write(p);
                        }
                    ))), default!);
                };
                var logfʗ4 = logf;
                http_internal_test_package.SetRoundTripRetried(() => {
                    logfʗ4("Retried."u8);
                });
                defer(http_internal_test_package.SetRoundTripRetried, (Action)(default!), ref ᒐ);
                for (nint i = 0; i < 3; i++) {
                    var t0 = time.Now();
                    var req = tcʗ1.req();
                    var (res, err) = c.Do(req);
                    if (err != default!) {
                        if (time.Since(t0) < http_internal_test_package.MaxWriteWaitBeforeConnReuse.Value / 2) {
                            Ꮡmu.Lock();
                            @string gotΔ1 = Ꮡlogbuf.Value.String();
                            Ꮡmu.Unlock();
                            tΔ1.Fatalf("i=%d: Do = %v; log:\n%s"u8, i, err, gotΔ1);
                        }
                        tΔ1.Skipf("connection likely wasn't recycled within %d, interfering with actual test; skipping"u8, http_internal_test_package.MaxWriteWaitBeforeConnReuse.Value);
                    }
                    (~res).Body.Close();
                    if ((~res).Request != req) {
                        tΔ1.Errorf("Response.Request != original request; want identical Request"u8);
                    }
                }
                Ꮡmu.Lock();
                @string got = Ꮡlogbuf.Value.String();
                Ꮡmu.Unlock();
                @string want = fmt.Sprintf("""
Dial
Write("%s")
Handler
intentional write failure
Retried.
Dial
Write("%s")
Handler
Write("%s")
Handler

"""u8, tcʗ1.reqString, tcʗ1.reqString, tcʗ1.reqString);
                if (got != want) {
                    tΔ1.Errorf("Log of events differs. Got:\n%s\nWant:\n%s"u8, got, want);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Issue 6981
public static void TestTransportClosesBodyOnError(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportClosesBodyOnError(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakeErrorˢ = "fake error"u8;

[GoType("dyn")] internal partial struct testTransportClosesBodyOnError_body {
    [GoEmbedded] public io_package.Reader Reader;
    [GoEmbedded] public io_package.Closer Closer;
}

internal static void testTransportClosesBodyOnError(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var readBody = new channel<error>(1);
        var readBodyʗ1 = readBody;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (_, errΔ1) = io.ReadAll((~r).Body);
            readBodyʗ1.ᐸꟷ(errΔ1);
        }))).Value.ts;
        var c = ts.Client();
        var fakeErr = errors.New(fakeErrorˢ);
        var didClose = new channel<bool>(1);
            var didCloseʗ1 = didClose;
        var (req, _) = NewRequest(postˢ, (~ts).URL, new testTransportClosesBodyOnError_body(
            io.MultiReader(io.LimitReader(((neverEnding)(rune)'x'), ((int64)1 << (int)(20))), iotest.ErrReader(fakeErr)),
            new http_test_package.closerFuncᴠCloser(new closerFunc(() => {
                var selᴛ77 = didCloseʗ1.ᐸꟷ(true, ꓸꓸꓸ);
                switch (trySelect(selᴛ77)) {
                case 0: {
                    break;
                }
                default: {
                    break;
                }}
                return default!;
            }))
        ));
        var (res, err) = c.Do(req);
        if (res != nil) {
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        }
        if (err == default! || !strings.Contains(err.Error(), fakeErr.Error())) {
            Ꮡt.Fatalf("Do error = %v; want something containing %q"u8, err, fakeErr.Error());
        }
        {
            var errΔ2 = ᐸꟷ(readBody); if (errΔ2 == default!) {
                Ꮡt.Errorf("Unexpected success reading request body from handler; want 'unexpected EOF reading trailer'"u8);
            }
        }
        var selᴛ78 = didClose;
        switch (trySelect(ᐸꟷ(selᴛ78, ꓸꓸꓸ))) {
        case 0 when selᴛ78.ꟷᐳ(out _): {
            break;
        }
        default: {
            Ꮡt.Errorf("didn't see Body.Close"u8);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportDialTLS(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportDialTLS(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didnTGetRequestˢ = (@string)"didn't get request"u8;
internal static readonly object didnTUseDialHookˢ = (@string)"didn't use dial hook"u8;

internal static void testTransportDialTLS(ж<testing.T> Ꮡt, testMode mode) {
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);                    // guards following
    bool gotReq = default!;
    bool didDial = default!;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡmu.Lock();
        gotReq = true;
        Ꮡmu.Unlock();
    }))).Value.ts;
    var c = ts.Client();
    var cʗ1 = c;
    (~c).Transport._<ж<Δhttp.Transport>>().Value.DialTLS = (@string netw, @string addr) => {
        Ꮡmu.Lock();
        didDial = true;
        Ꮡmu.Unlock();
        var (cΔ1, errΔ1) = tls.Dial(netw, addr, (~(~cʗ1).Transport._<ж<Δhttp.Transport>>()).TLSClientConfig);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        return (new tls.ConnжConn(cΔ1), cΔ1.Handshake());
    };
    var (res, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    Ꮡmu.Lock();
    if (!gotReq) {
        Ꮡt.Error(didnTGetRequestˢ);
    }
    if (!didDial) {
        Ꮡt.Error(didnTUseDialHookˢ);
    }
}

public static void TestTransportDialContext(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportDialContext(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someKeyˢ = "some-key"u8;

internal static void testTransportDialContext(ж<testing.T> Ꮡt, testMode mode) {
    @string ctxKey = someKeyˢ;
    @string ctxValue = someValueˢ;
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);                             // guards following
    bool gotReq = default!;
    ref var gotCtxValue = ref heap<any>(out var ᏑgotCtxValue);
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡmu.Lock();
        gotReq = true;
        Ꮡmu.Unlock();
    }))).Value.ts;
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.DialContext = (context.Context ctxΔ1, @string netw, @string addr) => {
        Ꮡmu.Lock();
        ᏑgotCtxValue.ValueSlot = ctxΔ1.Value(ctxKey);
        Ꮡmu.Unlock();
        return net.Dial(netw, addr);
    };
    var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ctx = context_package.WithValue(context_package.Background(), ctxKey, ctxValue);
    (var res, err) = c.Do(req.WithContext(ctx));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    Ꮡmu.Lock();
    if (!gotReq) {
        Ꮡt.Error(didnTGetRequestˢ);
    }
    {
        var got = gotCtxValue;
        @string want = ctxValue; if (!AreEqual(got, want)) {
            Ꮡt.Errorf("got context with value %v, want %v"u8, got, want);
        }
    }
}

public static void TestTransportDialTLSContext(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportDialTLSContext(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

internal static void testTransportDialTLSContext(ж<testing.T> Ꮡt, testMode mode) {
    @string ctxKey = someKeyˢ;
    @string ctxValue = someValueˢ;
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);                             // guards following
    bool gotReq = default!;
    ref var gotCtxValue = ref heap<any>(out var ᏑgotCtxValue);
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡmu.Lock();
        gotReq = true;
        Ꮡmu.Unlock();
    }))).Value.ts;
    var c = ts.Client();
    var cʗ1 = c;
    (~c).Transport._<ж<Δhttp.Transport>>().Value.DialTLSContext = (context.Context ctxΔ1, @string netw, @string addr) => {
        Ꮡmu.Lock();
        ᏑgotCtxValue.ValueSlot = ctxΔ1.Value(ctxKey);
        Ꮡmu.Unlock();
        var (cΔ1, errΔ1) = tls.Dial(netw, addr, (~(~cʗ1).Transport._<ж<Δhttp.Transport>>()).TLSClientConfig);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        return (new tls.ConnжConn(cΔ1), cΔ1.HandshakeContext(ctxΔ1));
    };
    var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ctx = context_package.WithValue(context_package.Background(), ctxKey, ctxValue);
    (var res, err) = c.Do(req.WithContext(ctx));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    Ꮡmu.Lock();
    if (!gotReq) {
        Ꮡt.Error(didnTGetRequestˢ);
    }
    {
        var got = gotCtxValue;
        @string want = ctxValue; if (!AreEqual(got, want)) {
            Ꮡt.Errorf("got context with value %v, want %v"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorMessageˢ = "errorMessage"u8;
internal static readonly object expectedProxyErrorToBeˢ = (@string)"Expected proxy error to be returned by RoundTrip"u8;

// Test for issue 8755
// Ensure that if a proxy returns an error, it is exposed by RoundTrip
public static void TestRoundTripReturnsProxyError(ж<testing.T> Ꮡt) {
    var badProxy = (ж<url.URL>, error) (ж<Δhttp.Request> _Δp0) => (default!, errors.New(errorMessageˢ));
    var tr = Ꮡ(new Transport(Proxy: badProxy));
    var (req, _) = NewRequest(getˢ2, httpExampleComˢ, default!);
    var (_, err) = tr.RoundTrip(req);
    if (err == default!) {
        Ꮡt.Error(expectedProxyErrorToBeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string startˢ = "start"u8;
internal static readonly object putFailedˢ = (@string)"put failed"u8;
internal static readonly object secondPutFailedˢ = (@string)"second put failed"u8;
internal static readonly @string afterPutˢ = "after put"u8;
internal static readonly object shouldBeIdleAfterˢ = (@string)"should be idle after CloseIdleConnections"u8;
internal static readonly @string afterCloseIdleˢ = "after close idle"u8;
internal static readonly object putDidnTFailˢ = (@string)"put didn't fail"u8;
internal static readonly @string afterSecondPutˢ = "after second put"u8;
internal static readonly object shouldnTBeIdleAfterˢ = (@string)"shouldn't be idle after QueueForIdleConnForTesting"u8;
internal static readonly object afterReActivationˢ = (@string)"after re-activation"u8;
internal static readonly @string afterFinalPutˢ = "after final put"u8;

// tests that putting an idle conn after a call to CloseIdleConns does return it
public static void TestTransportCloseIdleConnsThenReturn(ж<testing.T> Ꮡt) {
    var tr = Ꮡ(new Transport(nil));
    var trʗ1 = tr;
    bool wantIdle(@string when, nint n) {
        nint got = trʗ1.IdleConnCountForTesting(httpˢ, exampleComˢ); // key used by PutIdleTestConn
        if (got == n) {
            return true;
        }
        Ꮡt.Errorf("%s: idle conns = %d; want %d"u8, when, got, n);
        return false;
    }
    wantIdle(startˢ, 0);
    if (!tr.PutIdleTestConn(httpˢ, exampleComˢ)) {
        Ꮡt.Fatal(putFailedˢ);
    }
    if (!tr.PutIdleTestConn(httpˢ, exampleComˢ)) {
        Ꮡt.Fatal(secondPutFailedˢ);
    }
    wantIdle(afterPutˢ, 2);
    tr.CloseIdleConnections();
    if (!tr.IsIdleForTesting()) {
        Ꮡt.Error(shouldBeIdleAfterˢ);
    }
    wantIdle(afterCloseIdleˢ, 0);
    if (tr.PutIdleTestConn(httpˢ, exampleComˢ)) {
        Ꮡt.Fatal(putDidnTFailˢ);
    }
    wantIdle(afterSecondPutˢ, 0);
    tr.QueueForIdleConnForTesting(); // should toggle the transport out of idle mode
    if (tr.IsIdleForTesting()) {
        Ꮡt.Error(shouldnTBeIdleAfterˢ);
    }
    if (!tr.PutIdleTestConn(httpˢ, exampleComˢ)) {
        Ꮡt.Fatal(afterReActivationˢ);
    }
    wantIdle(afterFinalPutˢ, 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleCom443ˢ = "example.com:443"u8;
internal static readonly object gotConnCalledˢ = (@string)"GotConn called"u8;
internal static readonly @string httpsExampleComˢ2 = "https://example.com"u8;
internal static readonly @string afterRoundTripˢ = "after round trip"u8;

// Test for issue 34282
// Ensure that getConn doesn't call the GotConn trace hook on an HTTP/2 idle conn
public static void TestTransportTraceGotConnH2IdleConns(ж<testing.T> Ꮡt) {
    var tr = Ꮡ(new Transport(nil));
    var trʗ1 = tr;
    bool wantIdle(@string when, nint n) {
        nint got = trʗ1.IdleConnCountForTesting(httpsˢ, exampleCom443ˢ); // key used by PutIdleTestConnH2
        if (got == n) {
            return true;
        }
        Ꮡt.Errorf("%s: idle conns = %d; want %d"u8, when, got, n);
        return false;
    }
    wantIdle(startˢ, 0);
    var alt = new funcRoundTripper(() => {
    });
    if (!tr.PutIdleTestConnH2(httpsˢ, exampleCom443ˢ, new http_test_package.funcRoundTripperᴠRoundTripper(alt))) {
        Ꮡt.Fatal(putFailedˢ);
    }
    wantIdle(afterPutˢ, 1);
    var ctx = httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
        GotConn: (httptrace.GotConnInfo _Δp0) => {
            // tr.getConn should leave it for the HTTP/2 alt to call GotConn.
            Ꮡt.Error(gotConnCalledˢ);
        }
    )));
    var (req, _) = NewRequestWithContext(ctx, MethodGet, httpsExampleComˢ2, default!);
    var (_, err) = tr.RoundTrip(req);
    if (!AreEqual(err, errFakeRoundTrip)) {
        Ꮡt.Errorf("got error: %v; want %q"u8, err, errFakeRoundTrip);
    }
    wantIdle(afterRoundTripˢ, 1);
}

// https://go.dev/issue/70515
//
// When the first request on a new connection fails, we do not retry the request.
// If the first request on a connection races with IdleConnTimeout,
// we should not fail the request.
public static void TestTransportIdleConnRacesRequest(ж<testing.T> Ꮡt) {
    // Use unencrypted HTTP/2, since the *tls.Conn interfers with our ability to
    // block the connection closing.
    runSynctest(Ꮡt, testTransportIdleConnRacesRequest, new testMode[]{http1Mode, http2UnencryptedMode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object removeSkipWhen70515Isˢ = (@string)"remove skip when #70515 is fixed"u8;
internal static readonly object expectedRequestToFailButˢ = (@string)"expected request to fail, but it succeeded"u8;

internal static partial void testTransportIdleConnRacesRequest(testing.TB t, testMode mode) {
    if (mode == http2UnencryptedMode) {
        t.Skip(removeSkipWhen70515Isˢ);
    }
    var timeout = 1 * time.Millisecond;
    var trFunc = (ж<Δhttp.Transport> tr) => {
        tr.Value.IdleConnTimeout = timeout;
    };
    var cst = newClientServerTest(t, mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })), (trFunc).OrTypedNilFunc(), optFakeNet.OrTypedNil());
    cst.Value.li.Value.trackConns = true;
    // We want to put a connection into the pool which has never had a request made on it.
    //
    // Make a request and cancel it before the dial completes.
    // Then complete the dial.
    var dialc = new channel<EmptyStruct>(0);
    var dialcʗ1 = dialc;
    cst.Value.li.Value.onDial = () => {
        ᐸꟷ(dialcʗ1);
    };
    var (ctx, cancel) = context_package.WithCancel(context_package.Background());
    var req1c = new channel<error>(0);
    var cstʗ1 = cst;
    var ctxʗ1 = ctx;
    var req1cʗ1 = req1c;
    goǃ(() => {
        var (req, _) = NewRequestWithContext(ctxʗ1, getˢ2, (~(~cstʗ1).ts).URL, default!);
        var (resp, err) = (~cstʗ1).c.Do(req);
        if (err == default!) {
            (~resp).Body.Close();
        }
        req1cʗ1.ᐸꟷ(err);
    });
    // Wait for the connection attempt to start.
    synctest.Wait();
    // Cancel the request.
    cancel();
    synctest.Wait();
    {
        var err = ᐸꟷ(req1c); if (err == default!) {
            t.Fatal(expectedRequestToFailButˢ);
        }
    }
    // Unblock the dial, placing a new, unused connection into the Transport's pool.
    builtin.close(dialc);
    // We want IdleConnTimeout to race with a new request.
    //
    // There's no perfect way to do this, but the following exercises the bug in #70515:
    // Block net.Conn.Close, wait until IdleConnTimeout occurs, and make a request while
    // the connection close is still blocked.
    //
    // First: Wait for IdleConnTimeout. The net.Conn.Close blocks.
    synctest.Wait();
    var closec = new channel<EmptyStruct>(0);
    var closecʗ1 = closec;
    (~(~cst).li).conns[0].Value.peer.Value.onClose = () => {
        ᐸꟷ(closecʗ1);
    };
    time.Sleep(timeout);
    synctest.Wait();
    // Make a request, which will use a new connection (since the existing one is closing).
    var req2c = new channel<error>(0);
    var cstʗ2 = cst;
    var req2cʗ1 = req2c;
    goǃ(() => {
        var (resp, err) = (~cstʗ2).c.Get((~(~cstʗ2).ts).URL);
        if (err == default!) {
            (~resp).Body.Close();
        }
        req2cʗ1.ᐸꟷ(err);
    });
    // Don't synctest.Wait here: The HTTP/1 transport closes the idle conn
    // with a mutex held, and we'll end up in a deadlock.
    builtin.close(closec);
    {
        var err = ᐸꟷ(req2c); if (err != default!) {
            t.Fatalf("Get: %v"u8, err);
        }
    }
}

public static void TestTransportRemovesConnsAfterIdle(ж<testing.T> Ꮡt) {
    runSynctest(Ꮡt, testTransportRemovesConnsAfterIdle);
}

internal static void testTransportRemovesConnsAfterIdle(testing.TB t, testMode mode) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    var timeout = 1 * time.ΔSecond;
    var trFunc = (ж<Δhttp.Transport> tr) => {
        tr.Value.MaxConnsPerHost = 1;
        tr.Value.MaxIdleConnsPerHost = 1;
        tr.Value.IdleConnTimeout = timeout;
    };
    var cst = newClientServerTest(t, mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(xAddrˢ, (~r).RemoteAddr);
    })), (trFunc).OrTypedNilFunc(), optFakeNet.OrTypedNil());
    // makeRequest returns the local address a request was made from
    // (unique for each connection).
    var cstʗ1 = cst;
    @string makeRequest() {
        var (resp, err) = (~cstʗ1).c.Get((~(~cstʗ1).ts).URL);
        if (err != default!) {
            t.Fatalf("got error: %s"u8, err);
        }
        (~resp).Body.Close();
        return (~resp).Header.Get(xAddrˢ);
    }
    @string addr1 = makeRequest();
    time.Sleep(timeout / 2);
    synctest.Wait();
    @string addr2 = makeRequest();
    if (addr1 != addr2) {
        t.Fatalf("two requests made within IdleConnTimeout should have used the same conn, but used %v, %v"u8, addr1, addr2);
    }
    time.Sleep(timeout);
    synctest.Wait();
    @string addr3 = makeRequest();
    if (addr1 == addr3) {
        t.Fatalf("two requests made more than IdleConnTimeout apart should have used different conns, but used %v, %v"u8, addr1, addr3);
    }
}

public static void TestTransportRemovesConnsAfterBroken(ж<testing.T> Ꮡt) {
    runSynctest(Ꮡt, testTransportRemovesConnsAfterBroken);
}

internal static void testTransportRemovesConnsAfterBroken(testing.TB t, testMode mode) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    var trFunc = (ж<Δhttp.Transport> tr) => {
        tr.Value.MaxConnsPerHost = 1;
        tr.Value.MaxIdleConnsPerHost = 1;
    };
    var cst = newClientServerTest(t, mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(xAddrˢ, (~r).RemoteAddr);
    })), (trFunc).OrTypedNilFunc(), optFakeNet.OrTypedNil());
    cst.Value.li.Value.trackConns = true;
    // makeRequest returns the local address a request was made from
    // (unique for each connection).
    var cstʗ1 = cst;
    @string makeRequest() {
        var (resp, err) = (~cstʗ1).c.Get((~(~cstʗ1).ts).URL);
        if (err != default!) {
            t.Fatalf("got error: %s"u8, err);
        }
        (~resp).Body.Close();
        return (~resp).Header.Get(xAddrˢ);
    }
    @string addr1 = makeRequest();
    @string addr2 = makeRequest();
    if (addr1 != addr2) {
        t.Fatalf("successive requests should have used the same conn, but used %v, %v"u8, addr1, addr2);
    }
    // The connection breaks.
    synctest.Wait();
    (~(~(~cst).li).conns[0]).peer.Close();
    synctest.Wait();
    @string addr3 = makeRequest();
    if (addr1 == addr3) {
        t.Fatalf("successive requests made with conn broken between should have used different conns, but used %v, %v"u8, addr1, addr3);
    }
}

// This tests that a client requesting a content range won't also
// implicitly ask for gzip support. If they want that, they need to do it
// on their own.
// golang.org/issue/8923
public static void TestTransportRangeAndGzip(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportRangeAndGzip(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object transportAdvertisedGzipˢ = (@string)"Transport advertised gzip support in the Accept header"u8;
internal static readonly object noRangeInRequestˢ = (@string)"no Range in request"u8;
internal static readonly @string bytes711ˢ = "bytes=7-11"u8;

internal static void testTransportRangeAndGzip(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if (strings.Contains((~r).Header.Get(acceptEncodingˢ), gzipˢ)) {
            Ꮡt.Error(transportAdvertisedGzipˢ);
        }
        if ((~r).Header.Get(rangeˢ) == ""u8) {
            Ꮡt.Error(noRangeInRequestˢ);
        }
    }))).Value.ts;
    var c = ts.Client();
    var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
    (~req).Header.Set(rangeˢ, bytes711ˢ);
    var (res, err) = c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
}

// Test for issue 10474
public static void TestTransportResponseCancelRace(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportResponseCancelRace(Δp0, Δp1));
}

internal static void testTransportResponseCancelRace(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        // important that this response has a body.
        array<byte> b = new(1024);
        w.Write(b[..]);
    }))).Value.ts;
    var tr = (~ts.Client()).Transport._<ж<Δhttp.Transport>>();
    var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var res, err) = tr.RoundTrip(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // If we do an early close, Transport just throws the connection away and
    // doesn't reuse it. In order to trigger the bug, it has to reuse the connection
    // so read the body
    {
        var (_, errΔ1) = io.Copy(io.Discard, (~res).Body); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    (var req2, err) = NewRequest(getˢ2, (~ts).URL, default!);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    tr.CancelRequest(req);
    (res, err) = tr.RoundTrip(req2);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
}

// Test for issue 19248: Content-Encoding's value is case insensitive.
public static void TestTransportContentEncodingCaseInsensitive(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportContentEncodingCaseInsensitive(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string encodedStringᶜ = "Hello Gopher"u8;

internal static void testTransportContentEncodingCaseInsensitive(ж<testing.T> Ꮡt, testMode mode) {
    foreach (var (_, ce) in new @string[]{"gzip"u8, "GZIP"u8}.slice()) {
        @string ceΔ1 = ce;
        Ꮡt.Run(ceΔ1, (ж<testing.T> tΔ1) => {
            @string encodedString = encodedStringᶜ;
            var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                w.Header().Set(contentEncodingˢ, ceΔ1);
                var gz = gzip.NewWriter(new http_test_package.http_ResponseWriterᴠWriter(w));
                gz.Write(slice<byte>(encodedString));
                gz.Close();
            }))).Value.ts;
            var (res, err) = ts.Client().Get((~ts).URL);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (var body, err) = io.ReadAll((~res).Body);
            (~res).Body.Close();
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (((sstring)body) != encodedString) {
                tΔ1.Fatalf("Expected body %q, got: %q\n"u8, encodedString, ((@string)body));
            }
        });
    }
}

// https://go.dev/issue/49621
public static void TestConnClosedBeforeRequestIsWritten(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testConnClosedBeforeRequestIsWritten(Δp0, Δp1), testNotParallel, new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ = "error"u8;

internal static void testConnClosedBeforeRequestIsWritten(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        })),
            (ж<Δhttp.Transport> tr) => {
                tr.Value.DialContext = (net.Conn, error) (context.Context _Δp0, @string network, @string addr) => {
                    // Connection immediately returns errors.
                    return (new http_test_package.funcConnжConn(Ꮡ(new funcConn(
                        read: (slice<byte> _Δp0) => (0, errors.New(errorˢ)),
                        write: (slice<byte> _Δp0) => (0, errors.New(errorˢ))
                    ))), default!);
                };
            }).Value.ts;
        // Set a short delay in RoundTrip to give the persistConn time to notice
        // the connection is broken. We want to exercise the path where writeLoop exits
        // before it reads the request to send. If this delay is too short, we may instead
        // exercise the path where writeLoop accepts the request and then fails to write it.
        // That's fine, so long as we get the desired path often enough.
        http_internal_test_package.SetEnterRoundTripHook(() => {
            time.Sleep(1 * time.Millisecond);
        });
        defer(http_internal_test_package.SetEnterRoundTripHook, (Action)(default!), ref ᒐ);
        ref var closes = ref heap(new nint(), out var Ꮡcloses);
        var (_, err) = ts.Client().Post((~ts).URL, textPlainˢ, new countCloseReader(Ꮡcloses, new http_test_package.strings_ReaderжReader(strings.NewReader(helloˢ3))));
        if (err == default!) {
            Ꮡt.Fatalf("expected request to fail, but it did not"u8);
        }
        if (closes != 1) {
            Ꮡt.Errorf("after RoundTrip, request body was closed %v times; want 1"u8, closes);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// logWritesConn is a net.Conn that logs each Write call to writes
// and then proxies to w.
// It proxies Read calls to a reader it receives from rch.
[GoType] partial struct logWritesConn {
    [GoEmbedded] public net_package.Conn Conn; // nil. crash on use.
    internal io.Writer w;
    internal /*<-*/channel<io.Reader> rch = /*<-*/channel<io.Reader>.RecvOnly;
    internal io.Reader r; // nil until received by rch
    internal sync.Mutex mu;
    internal slice<@string> writes;
}

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *logWritesConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this logWritesConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *logWritesConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this logWritesConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *logWritesConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this logWritesConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *logWritesConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this logWritesConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *logWritesConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this logWritesConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint n, error err) Write(this ж<logWritesConn> Ꮡc, slice<byte> p) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        defer(Ꮡc.of(logWritesConn.Ꮡmu).Unlock, ref ᒐ);
        c.writes = append(c.writes, ((@string)p));
        (n, err) = c.w.Write(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (n, err);
}

[GoRecv] internal static (nint n, error err) Read(this ref logWritesConn c, slice<byte> p) {
    if (c.r == default!) {
        c.r = ᐸꟷ(c.rch);
    }
    return c.r.Read(p);
}

[GoRecv] internal static error Close(this ref logWritesConn c) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpLocalhost8080ˢ = "http://localhost:8080"u8;
internal static readonly @string http11204NoContentˢ = "HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"u8;

// Issue 6574
public static partial void TestTransportFlushesBodyChunks(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var resBody = new channel<io.Reader>(1);
        var (connr, connw) = io.Pipe(); // connection pipe pair
        var lw = Ꮡ(new logWritesConn(
            rch: resBody,
            w: new io.PipeWriterжWriter(connw)
        ));
            var lwʗ1 = lw;
        var tr = Ꮡ(new Transport(
            Dial: (@string network, @string addr) => (new http_test_package.logWritesConnжConn(lwʗ1), default!)
        ));
        var (bodyr, bodyw) = io.Pipe(); // body pipe pair
        var bodywʗ1 = bodyw;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var bodywʗ2 = bodywʗ1;
                defer(() => bodywʗ2.Close(), ref ᒐ);
                for (nint i = 0; i < 3; i++) {
                    fmt.Fprintf(new io.PipeWriterжWriter(bodywʗ1), "num%d\n"u8, i);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var resc = new channel<ж<Δhttp.Response>>(0);
        var bodyrʗ1 = bodyr;
        var rescʗ1 = resc;
        var trʗ1 = tr;
        goǃ(() => {
            var (reqΔ1, _) = NewRequest(postˢ, httpLocalhost8080ˢ, new io.PipeReaderжReader(bodyrʗ1));
            (~reqΔ1).Header.Set(userAgentˢ2, "x"u8); // known value for test
            var (resΔ1, errΔ1) = trʗ1.RoundTrip(reqΔ1);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("RoundTrip: %v"u8, errΔ1);
                builtin.close(rescʗ1);
                return;
            }
            rescʗ1.ᐸꟷ(resΔ1);
        });
        // Fully consume the request before checking the Write log vs. want.
        var (req, err) = ReadRequest(bufio.NewReader(new io.PipeReaderжReader(connr)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        io.Copy(io.Discard, (~req).Body);
        // Unblock the transport's roundTrip goroutine.
        resBody.ᐸꟷ(new http_test_package.strings_ReaderжReader(strings.NewReader(http11204NoContentˢ)));
        var (res, ok) = ᐸꟷ(resc, ꟷ);
        if (!ok) {
            return;
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        var want = new @string[]{
            "POST / HTTP/1.1\r\nHost: localhost:8080\r\nUser-Agent: x\r\nTransfer-Encoding: chunked\r\nAccept-Encoding: gzip\r\n\r\n"u8,
            "5\r\nnum0\n\r\n"u8,
            "5\r\nnum1\n\r\n"u8,
            "5\r\nnum2\n\r\n"u8,
            "0\r\n\r\n"u8
        }.slice();
        if (!slices.Equal<slice<@string>, @string>((~lw).writes, want)) {
            Ꮡt.Errorf("Writes differed.\n Got: %q\nWant: %q\n"u8, (~lw).writes, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 22088: flush Transport request headers if we're not sure the body won't block on read.
public static void TestTransportFlushesRequestHeader(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportFlushesRequestHeader(Δp0, Δp1));
}

internal static partial void testTransportFlushesRequestHeader(ж<testing.T> Ꮡt, testMode mode) {
    var gotReq = new channel<EmptyStruct>(0);
    var gotReqʗ1 = gotReq;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        builtin.close(gotReqʗ1);
    })));
    var (pr, pw) = io.Pipe();
    var (req, err) = NewRequest(postˢ, (~(~cst).ts).URL, new io.PipeReaderжReader(pr));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var gotRes = new channel<EmptyStruct>(0);
    var cstʗ1 = cst;
    var gotResʗ1 = gotRes;
    var reqʗ1 = req;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), gotResʗ1, ref ᒐ);
            var (res, errΔ1) = (~cstʗ1).tr.RoundTrip(reqʗ1);
            if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
                return;
            }
            (~res).Body.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    ᐸꟷ(gotReq);
    pw.Close();
    ᐸꟷ(gotRes);
}

[GoType] partial struct wgReadCloser {
    [GoEmbedded] public io_package.Reader Reader;
    internal ж<sync.WaitGroup> wg;
    internal bool closed;
}

[GoRecv] internal static error Close(this ref wgReadCloser c) {
    if (c.closed) {
        return net.ErrClosed;
    }
    c.closed = true;
    c.wg.Done();
    return default!;
}

// Issue 11745.
public static void TestTransportPrefersResponseOverWriteError(ж<testing.T> Ꮡt) {
    // Not parallel: modifies the global rstAvoidanceDelay.
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportPrefersResponseOverWriteError(Δp0, Δp1), testNotParallel);
}

internal static void testTransportPrefersResponseOverWriteError(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    runTimeSensitiveTest(Ꮡt, new time.Duration[]{
        1 * time.Millisecond,
        5 * time.Millisecond,
        10 * time.Millisecond,
        50 * time.Millisecond,
        100 * time.Millisecond,
        500 * time.Millisecond,
        time.ΔSecond,
        (time.Duration)(5000000000L)
    }.slice(), error (ж<testing.T> tΔ1, time.Duration timeout) => {
        GoFrame ᒐ = default;
        try {
            http_internal_test_package.SetRSTAvoidanceDelay(tΔ1, timeout);
            tΔ1.Logf("set RST avoidance delay to %v"u8, timeout);
            UntypedInt contentLengthLimit = /* 1024 * 1024 */ 1048576; // 1MB
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                if ((~r).ContentLength >= contentLengthLimit) {
                    w.WriteHeader(StatusBadRequest);
                    (~r).Body.Close();
                    return;
                }
                w.WriteHeader(StatusOK);
            })));
            // We need to close cst explicitly here so that in-flight server
            // requests don't race with the call to SetRSTAvoidanceDelay for a retry.
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            var c = ts.Client();
            nint count = 100;
            @string bigBody = strings.Repeat("a"u8, contentLengthLimit * 2);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            defer(Ꮡwg.Wait, ref ᒐ);
            var getBody = (io.ReadCloser, error) () => {
                Ꮡwg.Add(1);
                var body = Ꮡ(new wgReadCloser(
                    Reader: new http_test_package.strings_ReaderжReader(strings.NewReader(bigBody)),
                    wg: Ꮡwg
                ));
                return (new http_test_package.wgReadCloserжReadCloser(body), default!);
            };
            for (nint i = 0; i < count; i++) {
                var (reqBody, _) = getBody();
                var (req, err) = NewRequest(putˢ, (~ts).URL, reqBody);
                if (err != default!) {
                    reqBody.Close();
                    tΔ1.Fatal(err);
                }
                req.Value.ContentLength = (int64)len(bigBody);
                req.Value.GetBody = getBody;
                (var resp, err) = c.Do(req);
                if (err != default!){
                    return fmt.Errorf("Do %d: %v"u8, i, err);
                } else {
                    (~resp).Body.Close();
                    if ((~resp).StatusCode != 400) {
                        tΔ1.Errorf("Expected status code 400, got %v"u8, (~resp).Status);
                    }
                }
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

public static void TestTransportAutomaticHTTP2(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(nil)), true);
}

public static void TestTransportAutomaticHTTP2_DialerAndTLSConfigSupportsHTTP2AndTLSConfig(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        ForceAttemptHTTP2: true,
        TLSClientConfig: @new<tls.Config>()
    )), true);
}

// golang.org/issue/14391: also check DefaultTransport
public static void TestTransportAutomaticHTTP2_DefaultTransport(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, DefaultTransport._<ж<Δhttp.Transport>>(), true);
}

public static void TestTransportAutomaticHTTP2_TLSNextProto(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        TLSNextProto: new map<@string, Func<@string, ж<tls.Conn>, Δhttp.RoundTripper>>()
    )), false);
}

public static void TestTransportAutomaticHTTP2_TLSConfig(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        TLSClientConfig: @new<tls.Config>()
    )), false);
}

public static void TestTransportAutomaticHTTP2_ExpectContinueTimeout(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        ExpectContinueTimeout: 1 * time.ΔSecond
    )), true);
}

public static void TestTransportAutomaticHTTP2_Dial(ж<testing.T> Ꮡt) {
    ref var d = ref heap(new net.Dialer(), out var Ꮡd);
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        Dial: Ꮡd.Dial
    )), false);
}

public static void TestTransportAutomaticHTTP2_DialContext(ж<testing.T> Ꮡt) {
    ref var d = ref heap(new net.Dialer(), out var Ꮡd);
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        DialContext: Ꮡd.DialContext
    )), false);
}

public static void TestTransportAutomaticHTTP2_DialTLS(ж<testing.T> Ꮡt) {
    testTransportAutoHTTP(Ꮡt, Ꮡ(new Transport(
        DialTLS: (@string network, @string addr) => {
            throw panic("unused");
        }
    )), false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorFromˢ2 = (@string)"expected error from RoundTrip"u8;

internal static void testTransportAutoHTTP(ж<testing.T> Ꮡt, ж<Δhttp.Transport> Ꮡtr, bool wantH2) {
    ref var tr = ref Ꮡtr.DerefOrNull();

    http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
    var (_, err) = Ꮡtr.RoundTrip(@new<Δhttp.Request>());
    if (err == default!) {
        Ꮡt.Error(expectedErrorFromˢ2);
    }
    {
        var reg = tr.TLSNextProto["h2"u8] != default!; if (reg != wantH2) {
            Ꮡt.Errorf("HTTP/2 registered = %v; want %v"u8, reg, wantH2);
        }
    }
}

// Issue 13633: there was a race where we returned bodyless responses
// to callers before recycling the persistent connection, which meant
// a client doing two subsequent requests could end up on different
// connections. It's somewhat harmless but enough tests assume it's
// not true in order to test other things that it's worth fixing.
// Plus it's nice to be consistent and not have timing-dependent
// behavior.
public static void TestTransportReuseConnEmptyResponseBody(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportReuseConnEmptyResponseBody(Δp0, Δp1));
}

internal static void testTransportReuseConnEmptyResponseBody(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(xAddrˢ, (~r).RemoteAddr);
    })));
    // Empty response body.
    nint n = 100;
    if (testing.Short()) {
        n = 10;
    }
    @string firstAddr = default!;
    for (nint i = 0; i < n; i++) {
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            log.Fatal(err);
        }
        @string addr = (~res).Header.Get(xAddrˢ);
        if (i == 0){
            firstAddr = addr;
        } else 
        if (addr != firstAddr) {
            Ꮡt.Fatalf("On request %d, addr %q != original addr %q"u8, i + 1, addr, firstAddr);
        }
        (~res).Body.Close();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsFakeTldˢ = "https://fake.tld/"u8;
internal static readonly object fooRoundTripperShouldNotˢ = (@string)"foo RoundTripper should not be called"u8;

// Issue 13839
public static partial void TestNoCrashReturningTransportAltConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ref var cert = ref heap<tls.Certificate>(out var Ꮡcert);
        (cert, var err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        http_internal_test_package.SetPendingDialHooks(() => {
            Ꮡwg.Add(1);
        }, Ꮡwg.Done);
        defer(http_internal_test_package.SetPendingDialHooks, (Action)(default!), (Action)(default!), ref ᒐ);
        var testDone = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), testDone, ref ᒐ);
        var certʗ1 = cert;
        var lnʗ2 = ln;
        var testDoneʗ1 = testDone;
        goǃ(() => {
            var tln = tls.NewListener(lnʗ2, Ꮡ(new tls.Config(
                NextProtos: new @string[]{"foo"u8}.slice(),
                Certificates: new tls.Certificate[]{certʗ1}.slice()
            )));
            var (sc, errΔ1) = tln.Accept();
            if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
                return;
            }
            {
                var errΔ2 = sc._<ж<tls.Conn>>().Handshake(); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                    return;
                }
            }
            ᐸꟷ(testDoneʗ1);
            sc.Close();
        });
        @string addr = ln.Addr().String();
        var (req, _) = NewRequest(getˢ2, httpsFakeTldˢ, default!);
        var cancel = new channel<EmptyStruct>(0);
        req.Value.Cancel = cancel.WithDirection(GoChanDir.Recv);
        var doReturned = new channel<bool>(1);
        var madeRoundTripper = new channel<bool>(1);
                var madeRoundTripperʗ1 = madeRoundTripper;


            var cancelʗ1 = cancel;
            var doReturnedʗ1 = doReturned;
        var tr = Ꮡ(new Transport(
            DisableKeepAlives: true,
            TLSNextProto: new map<@string, Func<@string, ж<tls.Conn>, Δhttp.RoundTripper>>{
                ["foo"u8] = (@string authority, ж<tls.Conn> cΔ1) => {
                    madeRoundTripperʗ1.ᐸꟷ(true);
                    return new http_test_package.funcRoundTripperᴠRoundTripper(new funcRoundTripper(() => {
                        Ꮡt.Error(fooRoundTripperShouldNotˢ);
                    }));
                }
            },
            Dial: (@string _Δp0, @string _Δp1) => {
                throw panic("shouldn't be called");
            },
            DialTLS: (@string _Δp0, @string _Δp1) => {
                var (tc, errΔ3) = tls.Dial(tcpˢ, addr, Ꮡ(new tls.Config(
                    InsecureSkipVerify: true,
                    NextProtos: new @string[]{"foo"u8}.slice()
                )));
                if (errΔ3 != default!) {
                    return (default!, errΔ3);
                }
                {
                    var errΔ4 = tc.Handshake(); if (errΔ4 != default!) {
                        return (default!, errΔ4);
                    }
                }
                builtin.close(cancelʗ1);
                ᐸꟷ(doReturnedʗ1);
                return (new tls.ConnжConn(tc), default!);
            }
        ));
        var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        (_, err) = c.Do(req);
        {
            var (ue, ok) = err._<ж<urlꓸError>>(ᐧ); if (!ok || !AreEqual((~ue).Err, http_internal_test_package.ExportErrRequestCanceledConn)) {
                Ꮡt.Fatalf("Do error = %v; want url.Error with errRequestCanceledConn"u8, err);
            }
        }
        doReturned.ᐸꟷ(true);
        ᐸꟷ(madeRoundTripper);
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportReuseConnection_Gzip_Chunked(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testTransportReuseConnection_Gzip(tΔ1, mode, true);
    });
}

public static void TestTransportReuseConnection_Gzip_ContentLength(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testTransportReuseConnection_Gzip(tΔ1, mode, false);
    });
}

// Make sure we re-use underlying TCP connection for gzipped responses too.
internal static void testTransportReuseConnection_Gzip(ж<testing.T> Ꮡt, testMode mode, bool chunked) {
    ref var t = ref Ꮡt.DerefOrNull();

    var addr = new channel<@string>(2);
    var addrʗ1 = addr;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        addrʗ1.ᐸꟷ((~r).RemoteAddr);
        w.Header().Set(contentEncodingˢ, gzipˢ);
        if (chunked) {
            w._<Flusher>().Flush();
        }
        w.Write(rgz); // arbitrary gzip response
    }))).Value.ts;
    var c = ts.Client();




    var trace = Ꮡ(new httptrace.ClientTrace(
        GetConn: (@string hostPort) => {
            Ꮡt.Logf("GetConn(%q)"u8, hostPort);
        },
        GotConn: (httptrace.GotConnInfo ci) => {
            Ꮡt.Logf("GotConn(%+v)"u8, ci);
        },
        PutIdleConn: (error err) => {
            Ꮡt.Logf("PutIdleConn(%v)"u8, err);
        },
        ConnectStart: (@string network, @string addrΔ1) => {
            Ꮡt.Logf("ConnectStart(%q, %q)"u8, network, addrΔ1);
        },
        ConnectDone: (@string network, @string addrΔ2, error err) => {
            Ꮡt.Logf("ConnectDone(%q, %q, %v)"u8, network, addrΔ2, err);
        }
    ));
    var ctx = httptrace.WithClientTrace(context_package.Background(), trace);
    for (nint i = 0; i < 2; i++) {
        var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
        req = req.WithContext(ctx);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var buf = new slice<byte>(len(rgz));
        {
            var (n, errΔ1) = io.ReadFull((~res).Body, buf); if (errΔ1 != default!) {
                Ꮡt.Errorf("%d. ReadFull = %v, %v"u8, i, n, errΔ1);
            }
        }
    }
    // Note: no res.Body.Close call. It should work without it,
    // since the flate.Reader's internal buffering will hit EOF
    // and that should be sufficient.
    @string a1 = ᐸꟷ(addr);
    @string a2 = ᐸꟷ(addr);
    if (a1 != a2) {
        Ꮡt.Fatalf("didn't reuse connection"u8);
    }
}

public static void TestTransportResponseHeaderLength(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportResponseHeaderLength(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object http2TransportDoesnTˢ = (@string)"HTTP/2 Transport doesn't support MaxResponseHeaderBytes"u8;
internal static readonly @string longˢ = "Long"u8;
internal static readonly @string serverResponseHeadersˢ = "server response headers exceeded 524288 bytes"u8;

internal static void testTransportResponseHeaderLength(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (mode == http2Mode) {
            Ꮡt.Skip(http2TransportDoesnTˢ);
        }
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~(~r).URL).Path == "/long"u8) {
                w.Header().Set(longˢ, strings.Repeat("a"u8, (1 << (int)(20))));
            }
        }))).Value.ts;
        var c = ts.Client();
        (~c).Transport._<ж<Δhttp.Transport>>().Value.MaxResponseHeaderBytes = ((int64)512 << (int)(10));
        {
            var (resΔ1, errΔ1) = c.Get((~ts).URL); if (errΔ1 != default!){
                Ꮡt.Fatal(errΔ1);
            } else {
                (~resΔ1).Body.Close();
            }
        }
        var (res, err) = c.Get((~ts).URL + "/long"u8);
        if (err == default!) {
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            int64 n = default!;
            foreach (var (k, vv) in (~res).Header) {
                foreach (var (_, v) in vv) {
                    n += (int64)len(k) + (int64)len(v);
                }
            }
            Ꮡt.Fatalf("Unexpected success. Got %v and %d bytes of response headers"u8, (~res).Status, n);
        }
        {
            @string want = serverResponseHeadersˢ; if (!strings.Contains(err.Error(), want)) {
                Ꮡt.Errorf("got error: %v; want %q"u8, err, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportEventTrace(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testTransportEventTrace(tΔ1, mode, false);
    }, testNotParallel);
}

// test a non-nil httptrace.ClientTrace but with all hooks set to zero.
public static void TestTransportEventTrace_NoHooks(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testTransportEventTrace(tΔ1, mode, true);
    }, testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xFooMultipleValsˢ = "X-Foo-Multiple-Vals"u8;
internal static readonly @string dnsStartHostDnsIsFakedˢ = "DNS start: {Host:dns-is-faked.golang}"u8;
internal static readonly @string gotConnˢ = "got conn: {"u8;
internal static readonly @string reusedFalseWasIdleFalseˢ = "Reused:false WasIdle:false IdleTime:0s"u8;
internal static readonly @string firstResponseByteˢ = "first response byte"u8;
internal static readonly @string tlsHandshakeStartˢ = "tls handshake start"u8;
internal static readonly @string tlsHandshakeDoneˢ = "tls handshake done"u8;
internal static readonly @string putIdleConnNilˢ = "PutIdleConn = <nil>"u8;
internal static readonly @string wroteHeaderFieldUserˢ = "WroteHeaderField: User-Agent: [Go-http-client/1.1]"u8;
internal static readonly @string wroteHeaderFieldXFooˢ = "WroteHeaderField: X-Foo-Multiple-Vals: [bar baz]"u8;
internal static readonly @string wroteHeaderFieldAcceptˢ = "WroteHeaderField: Accept-Encoding: [gzip]"u8;
internal static readonly @string wroteHeadersˢ = "WroteHeaders"u8;
internal static readonly @string wait100Continueˢ = "Wait100Continue"u8;
internal static readonly @string got100Continueˢ = "Got100Continue"u8;
internal static readonly @string wroteRequestErrNilˢ = "WroteRequest: {Err:<nil>}"u8;
internal static readonly @string toUdpˢ = " to udp "u8;
internal static readonly @string gettingConnForDnsIsFakedˢ = "Getting conn for dns-is-faked.golang:"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string resBodyᶜ1 = "some body"u8;

internal static void testTransportEventTrace(ж<testing.T> Ꮡt, testMode mode, bool noHooks) {
    GoFrame ᒐ = default;
    try {
        @string resBody = resBodyᶜ1;
        var gotWroteReqEvent = new channel<EmptyStruct>(500);
        var gotWroteReqEventʗ1 = gotWroteReqEvent;

        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~r).Method == "GET"u8) {
                // Do nothing for the second request.
                return;
            }
            {
                var (_, errΔ1) = io.ReadAll((~r).Body); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
            if (!noHooks) {
                ᐸꟷ(gotWroteReqEventʗ1);
            }
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), resBody);
        })), (ж<Δhttp.Transport> tr) => {
            if ((~tr).TLSClientConfig != nil) {
                tr.Value.TLSClientConfig.Value.InsecureSkipVerify = true;
            }
        });
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        cst.Value.tr.Value.ExpectContinueTimeout = 1 * time.ΔSecond;
        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);                    // guards buf
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        void logf(@string format, params ꓸꓸꓸany argsʗp) {
            GoFrame ᒐ = default;
            try {
                var args = argsʗp.sslice();
                Ꮡmu.Lock();
                defer(Ꮡmu.Unlock, ref ᒐ);
                fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), format, args.ꓸꓸꓸ);
                Ꮡbuf.WriteByte((rune)'\n');
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        @string addrStr = (~(~cst).ts).Listener.Addr().String();
        var (ip, port, err) = net.SplitHostPort(addrStr);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Install a fake DNS server.
        var ctx = context_package.WithValue(context_package.Background(), new nettrace.LookupIPAltResolverKey(nil), (slice<net.IPAddr>, error) (context.Context ctxΔ1, @string network, @string host) => {
            if (host != "dns-is-faked.golang"u8) {
                Ꮡt.Errorf("unexpected DNS host lookup for %q/%q"u8, network, host);
                return (default!, default!);
            }
            return (new net.IPAddr[]{new(IP: net.ParseIP(ip))}.slice(), default!);
        });
        @string body = someBodyˢ;
        var (req, _) = NewRequest(postˢ, cst.scheme() + "://dns-is-faked.golang:"u8 + port, new http_test_package.strings_ReaderжReader(strings.NewReader(body)));
        req.Value.Header[xFooMultipleValsˢ] = new @string[]{"bar"u8, "baz"u8}.slice();
            var logfʗ1 = logf;

            var logfʗ2 = logf;

            var logfʗ3 = logf;

            var logfʗ4 = logf;

            var logfʗ5 = logf;

            var logfʗ6 = logf;

            var logfʗ7 = logf;

            var logfʗ8 = logf;

            var logfʗ9 = logf;

            var logfʗ10 = logf;

            var logfʗ11 = logf;

            var logfʗ12 = logf;

            var gotWroteReqEventʗ2 = gotWroteReqEvent;
            var logfʗ13 = logf;
        var trace = Ꮡ(new httptrace.ClientTrace(
            GetConn: (@string hostPort) => {
                logfʗ1("Getting conn for %v ..."u8, hostPort);
            },
            GotConn: (httptrace.GotConnInfo ci) => {
                logfʗ2("got conn: %+v"u8, ci);
            },
            GotFirstResponseByte: () => {
                logfʗ3("first response byte"u8);
            },
            PutIdleConn: (error errΔ2) => {
                logfʗ4("PutIdleConn = %v"u8, errΔ2);
            },
            DNSStart: (httptrace.DNSStartInfo e) => {
                logfʗ5("DNS start: %+v"u8, e);
            },
            DNSDone: (httptrace.DNSDoneInfo e) => {
                logfʗ6("DNS done: %+v"u8, e);
            },
            ConnectStart: (@string network, @string addr) => {
                logfʗ7("ConnectStart: Connecting to %s %s ..."u8, network, addr);
            },
            ConnectDone: (@string network, @string addr, error errΔ3) => {
                if (errΔ3 != default!) {
                    Ꮡt.Errorf("ConnectDone: %v"u8, errΔ3);
                }
                logfʗ8("ConnectDone: connected to %s %s = %v"u8, network, addr, errΔ3);
            },
            WroteHeaderField: (@string key, slice<@string> value) => {
                logfʗ9("WroteHeaderField: %s: %v"u8, key, value);
            },
            WroteHeaders: () => {
                logfʗ10("WroteHeaders"u8);
            },
            Wait100Continue: () => {
                logfʗ11("Wait100Continue"u8);
            },
            Got100Continue: () => {
                logfʗ12("Got100Continue"u8);
            },
            WroteRequest: (httptrace.WroteRequestInfo e) => {
                logfʗ13("WroteRequest: %+v"u8, e);
                gotWroteReqEventʗ2.ᐸꟷ(new EmptyStruct());
            }
        ));
        if (mode == http2Mode) {
            var logfʗ14 = logf;
            trace.Value.TLSHandshakeStart = () => {
                logfʗ14("tls handshake start"u8);
            };
            var logfʗ15 = logf;
            trace.Value.TLSHandshakeDone = (tlsꓸConnectionState s, error errΔ4) => {
                logfʗ15("tls handshake done. ConnectionState = %v \n err = %v"u8, s, errΔ4);
            };
        }
        if (noHooks) {
            // zero out all func pointers, trying to get some path to crash
            trace.Value = new httptrace.ClientTrace(nil);
        }
        req = req.WithContext(httptrace.WithClientTrace(ctx, trace));
        (~req).Header.Set(expectˢ, continueˢ);
        (var res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        logf("got roundtrip.response"u8);
        (var slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        logf("consumed body"u8);
        if (((sstring)slurp) != resBody || (~res).StatusCode != 200) {
            Ꮡt.Fatalf("Got %q, %v; want %q, 200 OK"u8, slurp, (~res).Status, resBody);
        }
        (~res).Body.Close();
        if (noHooks) {
            // Done at this point. Just testing a full HTTP
            // requests can happen with a trace pointing to a zero
            // ClientTrace, full of nil func pointers.
            return;
        }
        Ꮡmu.Lock();
        @string got = buf.String();
        Ꮡmu.Unlock();
        void wantOnce(@string subΔ1) {
            if (strings.Count(got, subΔ1) != 1) {
                Ꮡt.Errorf("expected substring %q exactly once in output."u8, subΔ1);
            }
        }
        void wantOnceOrMore(@string subΔ2) {
            if (strings.Count(got, subΔ2) == 0) {
                Ꮡt.Errorf("expected substring %q at least once in output."u8, subΔ2);
            }
        }
        wantOnce("Getting conn for dns-is-faked.golang:"u8 + port);
        wantOnce(dnsStartHostDnsIsFakedˢ);
        wantOnce("DNS done: {Addrs:[{IP:"u8 + ip + " Zone:}] Err:<nil> Coalesced:false}"u8);
        wantOnce(gotConnˢ);
        wantOnceOrMore("Connecting to tcp "u8 + addrStr);
        wantOnceOrMore("connected to tcp "u8 + addrStr + " = <nil>"u8);
        wantOnce(reusedFalseWasIdleFalseˢ);
        wantOnce(firstResponseByteˢ);
        if (mode == http2Mode){
            wantOnce(tlsHandshakeStartˢ);
            wantOnce(tlsHandshakeDoneˢ);
        } else {
            wantOnce(putIdleConnNilˢ);
            wantOnce(wroteHeaderFieldUserˢ);
            // TODO(meirf): issue 19761. Make these agnostic to h1/h2. (These are not h1 specific, but the
            // WroteHeaderField hook is not yet implemented in h2.)
            wantOnce(fmt.Sprintf("WroteHeaderField: Host: [dns-is-faked.golang:%s]"u8, port));
            wantOnce(fmt.Sprintf("WroteHeaderField: Content-Length: [%d]"u8, len(body)));
            wantOnce(wroteHeaderFieldXFooˢ);
            wantOnce(wroteHeaderFieldAcceptˢ);
        }
        wantOnce(wroteHeadersˢ);
        wantOnce(wait100Continueˢ);
        wantOnce(got100Continueˢ);
        wantOnce(wroteRequestErrNilˢ);
        if (strings.Contains(got, toUdpˢ)) {
            Ꮡt.Errorf("should not see UDP (DNS) connections"u8);
        }
        if (Ꮡt.Failed()) {
            Ꮡt.Errorf("Output:\n%s"u8, got);
        }
        // And do a second request:
        (req, _) = NewRequest(getˢ2, cst.scheme() + "://dns-is-faked.golang:"u8 + port, default!);
        req = req.WithContext(httptrace.WithClientTrace(ctx, trace));
        (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != 200) {
            Ꮡt.Fatal((~res).Status);
        }
        (~res).Body.Close();
        Ꮡmu.Lock();
        got = buf.String();
        Ꮡmu.Unlock();
        @string sub = gettingConnForDnsIsFakedˢ;
        {
            nint gotn = strings.Count(got, sub);
            nint want = 2; if (gotn != want) {
                Ꮡt.Errorf("substring %q appeared %d times; want %d. Log:\n%s"u8, sub, gotn, want, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportEventTraceTLSVerify(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportEventTraceTLSVerify(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedRequestˢ = (@string)"Unexpected request"u8;
internal static readonly object expectedRequestToFailTlsˢ = (@string)"Expected request to fail TLS verification"u8;
internal static readonly @string tlsHandshakeStartˢ2 = "TLSHandshakeStart"u8;
internal static readonly @string tlsHandshakeDoneˢ2 = "TLSHandshakeDone"u8;
internal static readonly @string errTlsFailedToVerifyˢ = "err = tls: failed to verify certificate: x509: certificate is valid for example.com"u8;

internal static void testTransportEventTraceTLSVerify(ж<testing.T> Ꮡt, testMode mode) {
    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    void logf(@string format, params ꓸꓸꓸany argsʗp) {
        GoFrame ᒐ = default;
        try {
            var args = argsʗp.sslice();
            Ꮡmu.Lock();
            defer(Ꮡmu.Unlock, ref ᒐ);
            fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), format, args.ꓸꓸꓸ);
            Ꮡbuf.WriteByte((rune)'\n');
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var logfʗ1 = logf;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡt.Error(unexpectedRequestˢ);
    })), (ж<httptest.Server> tsΔ1) => {
        var logfʗ2 = logfʗ1;
        tsΔ1.Value.Config.Value.ErrorLog = log.New(new http_test_package.funcWriterᴠWriter(new funcWriter((slice<byte> p) => {
            logfʗ2("%s"u8, p);
            return (len(p), default!);
        })), ""u8, 0);
    }).Value.ts;
    var certpool = x509.NewCertPool();
    certpool.AddCert(ts.Certificate());
    var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(Ꮡ(new Transport(
        TLSClientConfig: Ꮡ(new tls.Config(
            ServerName: "dns-is-faked.golang"u8,
            RootCAs: certpool
        ))
    )))
    ));
        var logfʗ3 = logf;

        var logfʗ4 = logf;
    var trace = Ꮡ(new httptrace.ClientTrace(
        TLSHandshakeStart: () => {
            logfʗ3("TLSHandshakeStart"u8);
        },
        TLSHandshakeDone: (tlsꓸConnectionState s, error errΔ1) => {
            logfʗ4("TLSHandshakeDone: ConnectionState = %v \n err = %v"u8, s, errΔ1);
        }
    ));
    var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
    req = req.WithContext(httptrace.WithClientTrace(context_package.Background(), trace));
    var (_, err) = c.Do(req);
    if (err == default!) {
        Ꮡt.Error(expectedRequestToFailTlsˢ);
    }
    Ꮡmu.Lock();
    @string got = buf.String();
    Ꮡmu.Unlock();
    void wantOnce(@string sub) {
        if (strings.Count(got, sub) != 1) {
            Ꮡt.Errorf("expected substring %q exactly once in output."u8, sub);
        }
    }
    wantOnce(tlsHandshakeStartˢ2);
    wantOnce(tlsHandshakeDoneˢ2);
    wantOnce(errTlsFailedToVerifyˢ);
    if (Ꮡt.Failed()) {
        Ꮡt.Errorf("Output:\n%s"u8, got);
    }
}

internal static Func<bool> isDNSHijacked = sync.OnceValue(bool () => {
    var (addrs, _) = net.LookupHost("dns-should-not-resolve.golang"u8);
    return len(addrs) != 0;
});

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestRequiresNonˢ = (@string)"skipping; test requires non-hijacking DNS server"u8;

internal static void skipIfDNSHijacked(ж<testing.T> Ꮡt) {
    // Skip this test if the user is using a shady/ISP
    // DNS server hijacking queries.
    // See issues 16732, 16716.
    if (isDNSHijacked()) {
        Ꮡt.Skip(skippingTestRequiresNonˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpDnsShouldNotResolveˢ = "http://dns-should-not-resolve.golang:80"u8;
internal static readonly object expectedErrorDuringDnsˢ = (@string)"expected error during DNS lookup"u8;
internal static readonly @string dnsStartHostDnsShouldNotˢ = "DNSStart: {Host:dns-should-not-resolve.golang}"u8;
internal static readonly @string dnsDoneAddrsErrˢ = "DNSDone: {Addrs:[] Err:"u8;
internal static readonly @string connectStartˢ = "ConnectStart"u8;
internal static readonly @string connectDoneˢ = "ConnectDone"u8;

public static void TestTransportEventTraceRealDNS(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        skipIfDNSHijacked(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new Transport(nil));
        var trʗ1 = tr;
        defer(trʗ1.CloseIdleConnections, ref ᒐ);
        var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);                    // guards buf
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        void logf(@string format, params ꓸꓸꓸany argsʗp) {
            GoFrame ᒐ = default;
            try {
                var args = argsʗp.sslice();
                Ꮡmu.Lock();
                defer(Ꮡmu.Unlock, ref ᒐ);
                fmt.Fprintf(new http_test_package.strings_BuilderжWriter(Ꮡbuf), format, args.ꓸꓸꓸ);
                Ꮡbuf.WriteByte((rune)'\n');
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        var (req, _) = NewRequest(getˢ2, httpDnsShouldNotResolveˢ, default!);
            var logfʗ1 = logf;

            var logfʗ2 = logf;

            var logfʗ3 = logf;

            var logfʗ4 = logf;
        var trace = Ꮡ(new httptrace.ClientTrace(
            DNSStart: (httptrace.DNSStartInfo e) => {
                logfʗ1("DNSStart: %+v"u8, e);
            },
            DNSDone: (httptrace.DNSDoneInfo e) => {
                logfʗ2("DNSDone: %+v"u8, e);
            },
            ConnectStart: (@string network, @string addr) => {
                logfʗ3("ConnectStart: %s %s"u8, network, addr);
            },
            ConnectDone: (@string network, @string addr, error errΔ1) => {
                logfʗ4("ConnectDone: %s %s %v"u8, network, addr, errΔ1);
            }
        ));
        req = req.WithContext(httptrace.WithClientTrace(context_package.Background(), trace));
        var (resp, err) = c.Do(req);
        if (err == default!) {
            (~resp).Body.Close();
            Ꮡt.Fatal(expectedErrorDuringDnsˢ);
        }
        Ꮡmu.Lock();
        @string got = buf.String();
        Ꮡmu.Unlock();
        void wantSub(@string sub) {
            if (!strings.Contains(got, sub)) {
                Ꮡt.Errorf("expected substring %q in output."u8, sub);
            }
        }
        wantSub(dnsStartHostDnsShouldNotˢ);
        wantSub(dnsDoneAddrsErrˢ);
        if (strings.Contains(got, connectStartˢ) || strings.Contains(got, connectDoneˢ)) {
            Ꮡt.Errorf("should not see Connect events"u8);
        }
        if (Ꮡt.Failed()) {
            Ꮡt.Errorf("Output:\n%s"u8, got);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpDummyTld123fooBarˢ = "http://dummy.tld:123foo/bar"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;
internal static readonly @string invalidPort123fooAfterˢ = @"invalid port "":123foo"" after host"u8;

// Issue 14353: port can only contain digits.
public static void TestTransportRejectsAlphaPort(ж<testing.T> Ꮡt) {
    var (res, err) = Get(httpDummyTld123fooBarˢ);
    if (err == default!) {
        (~res).Body.Close();
        Ꮡt.Fatal(unexpectedSuccessˢ);
    }
    var (ue, ok) = err._<ж<urlꓸError>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("got %#v; want *url.Error"u8, err);
    }
    @string got = (~ue).Err.Error();
    @string want = invalidPort123fooAfterˢ;
    if (got != want) {
        Ꮡt.Errorf("got error %q; want %q"u8, got, want);
    }
}

// Test the httptrace.TLSHandshake{Start,Done} hooks with an https http1
// connections. The http2 test is done in TestTransportEventTrace_h2
public static void TestTLSHandshakeTrace(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTLSHandshakeTrace(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorToBeNilButˢ = (@string)"Expected error to be nil but was:"u8;
internal static readonly object unableToConstructTestˢ = (@string)"Unable to construct test request:"u8;
internal static readonly object unexpectedErrorMakingˢ = (@string)"Unexpected error making request:"u8;
internal static readonly object expectedˢ = (@string)"Expected TLSHandshakeStart to be called, but wasn't"u8;
internal static readonly object expectedTLSHandshakeDoneˢ = (@string)"Expected TLSHandshakeDone to be called, but wasn't"u8;

internal static void testTLSHandshakeTrace(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        }))).Value.ts;
        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
        bool start = default!;
        bool done = default!;
        var trace = Ꮡ(new httptrace.ClientTrace(
            TLSHandshakeStart: () => {
                GoFrame ᒐ = default;
                try {
                    Ꮡmu.Lock();
                    defer(Ꮡmu.Unlock, ref ᒐ);
                    start = true;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            },
            TLSHandshakeDone: (tlsꓸConnectionState s, error errΔ1) => {
                GoFrame ᒐ = default;
                try {
                    Ꮡmu.Lock();
                    defer(Ꮡmu.Unlock, ref ᒐ);
                    done = true;
                    if (errΔ1 != default!) {
                        Ꮡt.Fatal(expectedErrorToBeNilButˢ, errΔ1);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }
        ));
        var c = ts.Client();
        var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
        if (err != default!) {
            Ꮡt.Fatal(unableToConstructTestˢ, err);
        }
        req = req.WithContext(httptrace.WithClientTrace(req.Context(), trace));
        (var r, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(unexpectedErrorMakingˢ, err);
        }
        (~r).Body.Close();
        Ꮡmu.Lock();
        defer(Ꮡmu.Unlock, ref ᒐ);
        if (!start) {
            Ꮡt.Fatal(expectedˢ);
        }
        if (!done) {
            Ꮡt.Fatal(expectedTLSHandshakeDoneˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportMaxIdleConns(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportMaxIdleConns(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportMaxIdleConns(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).Value.ts;
    // No body for convenience.
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    tr.Value.MaxIdleConns = 4;
    var (ip, port, err) = net.SplitHostPort((~ts).Listener.Addr().String());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ctx = context_package.WithValue(context_package.Background(), new nettrace.LookupIPAltResolverKey(nil), (slice<net.IPAddr>, error) (context.Context ctxΔ1, @string _Δp1, @string host) => (new net.IPAddr[]{new(IP: net.ParseIP(ip))}.slice(), default!));
    var cʗ1 = c;
    var ctxʗ1 = ctx;
    void hitHost(nint n) {
        var (req, _) = NewRequest(getˢ2, fmt.Sprintf("http://host-%d.dns-is-faked.golang:"u8 + port, n), default!);
        req = req.WithContext(ctxʗ1);
        var (res, errΔ1) = cʗ1.Do(req);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        (~res).Body.Close();
    }
    for (nint i = 0; i < 4; i++) {
        hitHost(i);
    }
    var want = new @string[]{
        "|http|host-0.dns-is-faked.golang:"u8 + port,
        "|http|host-1.dns-is-faked.golang:"u8 + port,
        "|http|host-2.dns-is-faked.golang:"u8 + port,
        "|http|host-3.dns-is-faked.golang:"u8 + port
    }.slice();
    {
        var got = tr.IdleConnKeysForTesting(); if (!slices.Equal<slice<@string>, @string>(got, want)) {
            Ꮡt.Fatalf("idle conn keys mismatch.\n got: %q\nwant: %q\n"u8, got, want);
        }
    }
    // Now hitting the 5th host should kick out the first host:
    hitHost(4);
    want = new @string[]{
        "|http|host-1.dns-is-faked.golang:"u8 + port,
        "|http|host-2.dns-is-faked.golang:"u8 + port,
        "|http|host-3.dns-is-faked.golang:"u8 + port,
        "|http|host-4.dns-is-faked.golang:"u8 + port
    }.slice();
    {
        var got = tr.IdleConnKeysForTesting(); if (!slices.Equal<slice<@string>, @string>(got, want)) {
            Ꮡt.Fatalf("idle conn keys mismatch after 5th host.\n got: %q\nwant: %q\n"u8, got, want);
        }
    }
}

public static void TestTransportIdleConnTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIdleConnTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string useOfClosedNetworkˢ = "use of closed network connection"u8;

internal static void testTransportIdleConnTimeout(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        var timeout = 1 * time.Millisecond;
timeoutLoop:
        while (ᐧ) {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            })));
            // No body for convenience.
            var tr = cst.Value.tr;
            tr.Value.IdleConnTimeout = timeout;
            var trʗ1 = tr;
            defer(trʗ1.CloseIdleConnections, ref ᒐ);
            var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
            var trʗ2 = tr;
            slice<@string> idleConns() {
                if (mode == http2Mode){
                    return trʗ2.IdleConnStrsForTesting_h2();
                } else {
                    return trʗ2.IdleConnStrsForTesting();
                }
            }
            @string conn = default!;
            var cʗ1 = c;
            var cstʗ1 = cst;
            var idleConnsʗ1 = idleConns;
            bool /*timeoutOk*/ doReq(nint n) {
                var (req, _) = NewRequest(getˢ2, (~(~cstʗ1).ts).URL, default!);
                req = req.WithContext(httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
                    PutIdleConn: (error errΔ1) => {
                        if (errΔ1 != default!) {
                            Ꮡt.Errorf("failed to keep idle conn: %v"u8, errΔ1);
                        }
                    }
                ))));
                var (res, err) = cʗ1.Do(req);
                if (err != default!) {
                    if (strings.Contains(err.Error(), useOfClosedNetworkˢ)) {
                        Ꮡt.Logf("req %v: connection closed prematurely"u8, n);
                        return false;
                    }
                }
                if (err == default!) {
                    (~res).Body.Close();
                }
                var conns = idleConnsʗ1();
                if (len(conns) != 1) {
                    if (len(conns) == 0) {
                        Ꮡt.Logf("req %v: no idle conns"u8, n);
                        return false;
                    }
                    Ꮡt.Fatalf("req %v: unexpected number of idle conns: %q"u8, n, conns);
                }
                if (conn == ""u8) {
                    conn = conns[0];
                }
                if (conn != conns[0]) {
                    Ꮡt.Logf("req %v: cached connection changed; expected the same one throughout the test"u8, n);
                    return false;
                }
                return true;
            }
            for (nint i = 0; i < 3; i++) {
                if (!doReq(i)) {
                    Ꮡt.Logf("idle conn timeout %v appears to be too short; retrying with longer"u8, timeout);
                    timeout *= 2;
                    cst.close();
                    goto continue_timeoutLoop;
                }
                time.Sleep(timeout / 2);
            }
            var idleConnsʗ2 = idleConns;
            waitCondition(new http_test_package.testing_TжTB(Ꮡt), timeout / 2, (time.Duration d) => {
                {
                    var got = idleConnsʗ2(); if (len(got) != 0) {
                        if (d >= timeout * 3 / 2) {
                            Ꮡt.Logf("after %v, idle conns = %q"u8, d, got);
                        }
                        return false;
                    }
                }
                return true;
            });
            break;
continue_timeoutLoop:;
        }
break_timeoutLoop:;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 16208: Go 1.7 crashed after Transport.IdleConnTimeout if an
// HTTP/2 connection was established but its caller no longer
// wanted it. (Assuming the connection cache was enabled, which it is
// by default)
//
// This test reproduced the crash by setting the IdleConnTimeout low
// (to make the test reasonable) and then making a request which is
// canceled by the DialTLS hook, which then also waits to return the
// real connection until after the RoundTrip saw the error.  Then we
// know the successful tls.Dial from DialTLS will need to go into the
// idle pool. Then we give it a of time to explode.
public static void TestIdleConnH2Crash(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIdleConnH2Crash(Δp0, Δp1), new testMode[]{http2Mode}.slice());
}

internal static void testIdleConnH2Crash(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        })));
        // nothing
        var (ctx, cancel) = context_package.WithCancel(context_package.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var sawDoErr = new channel<bool>(1);
        var testDone = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), testDone, ref ᒐ);
        cst.Value.tr.Value.IdleConnTimeout = 5 * time.Millisecond;
        var cancelʗ2 = cancel;
        var sawDoErrʗ1 = sawDoErr;
        var testDoneʗ1 = testDone;
        cst.Value.tr.Value.DialTLS = (net.Conn, error) (@string network, @string addr) => {
            var (c, errΔ1) = tls.Dial(network, addr, Ꮡ(new tls.Config(
                InsecureSkipVerify: true,
                NextProtos: new @string[]{"h2"u8}.slice()
            )));
            if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
                return (default!, errΔ1);
            }
            {
                var cs = c.ConnectionState(); if (cs.NegotiatedProtocol != "h2"u8) {
                    Ꮡt.Errorf("protocol = %q; want %q"u8, cs.NegotiatedProtocol, (@string)"h2"u8);
                    c.Close();
                    return (default!, errors.New(bogusˢ));
                }
            }
            cancelʗ2();
            var selᴛ79 = sawDoErrʗ1;
            var selᴛ80 = testDoneʗ1;
            switch (select(ᐸꟷ(selᴛ79, ꓸꓸꓸ), ᐸꟷ(selᴛ80, ꓸꓸꓸ))) {
            case 0 when selᴛ79.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ80.ꟷᐳ(out _): {
                break;
            }}
            return (new tls.ConnжConn(c), default!);
        };
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        req = req.WithContext(ctx);
        var (res, err) = (~cst).c.Do(req);
        if (err == default!) {
            (~res).Body.Close();
            Ꮡt.Fatal(unexpectedSuccessˢ);
        }
        sawDoErr.ᐸꟷ(true);
        // Wait for the explosion.
        time.Sleep((~(~cst).tr).IdleConnTimeout * 10);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct funcConn {
    [GoEmbedded] public net_package.Conn Conn;
    internal Func<slice<byte>, (nint, error)> read;
    internal Func<slice<byte>, (nint, error)> write;
}

internal static (nint, error) Read(this funcConn c, slice<byte> p) {
    return c.read(p);
}

internal static (nint, error) Write(this funcConn c, slice<byte> p) {
    return c.write(p);
}

internal static error Close(this funcConn c) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string specificErrorValueˢ = "specific error value"u8;
internal static readonly @string httpFakeTldˢ2 = "http://fake.tld/"u8;

// Issue 16465: Transport.RoundTrip should return the raw net.Conn.Read error from Peek
// back to the caller.
public static void TestTransportReturnsPeekError(ж<testing.T> Ꮡt) {
    var errValue = errors.New(specificErrorValueˢ);
    var wrote = new channel<EmptyStruct>(0);
    var wroteʗ1 = wrote;
    var wroteOnce = sync.OnceFunc(() => {
        builtin.close(wroteʗ1);
    });
        var errValueʗ1 = errValue;
        var wroteʗ2 = wrote;
        var wroteOnceʗ1 = wroteOnce;
    var tr = Ꮡ(new Transport(
        Dial: (@string network, @string addr) => {
                var errValueʗ2 = errValueʗ1;
                var wroteʗ3 = wroteʗ2;

                var wroteOnceʗ2 = wroteOnceʗ1;
            var c = new funcConn(
                read: (slice<byte> _Δp0) => {
                    ᐸꟷ(wroteʗ3);
                    return (0, errValueʗ2);
                },
                write: (slice<byte> p) => {
                    wroteOnceʗ2();
                    return (len(p), default!);
                }
            );
            return (c, default!);
        }
    ));
    var (_, err) = tr.RoundTrip(httptest.NewRequest(getˢ2, httpFakeTldˢ2, default!));
    if (!AreEqual(err, errValue)) {
        Ꮡt.Errorf("error = %#v; want %v"u8, err, errValue);
    }
}

// Issue 13835: international domain names should work
public static void TestTransportIDNA(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIDNA(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hitHandlerˢ = "Hit-Handler"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string uniDomainᶜ = "гофер.го"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string punyDomainᶜ = "xn--c1ae0ajs.xn--c1aw"u8;

internal static void testTransportIDNA(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string uniDomain = uniDomainᶜ;
        @string punyDomain = punyDomainᶜ;
        @string port = default!;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            @string want = punyDomain + ":" + port;
            if ((~r).Host != want) {
                Ꮡt.Errorf("Host header = %q; want %q"u8, (~r).Host, want);
            }
            if (mode == http2Mode) {
                if ((~r).TLS == nil){
                    Ꮡt.Errorf("r.TLS == nil"u8);
                } else 
                if ((~(~r).TLS).ServerName != punyDomain) {
                    Ꮡt.Errorf("TLS.ServerName = %q; want %q"u8, (~(~r).TLS).ServerName, punyDomain);
                }
            }
            w.Header().Set(hitHandlerˢ, "1"u8);
        })), (ж<Δhttp.Transport> tr) => {
            if ((~tr).TLSClientConfig != nil) {
                tr.Value.TLSClientConfig.Value.InsecureSkipVerify = true;
            }
        });
        (var ip, port, var err) = net.SplitHostPort((~(~cst).ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // Install a fake DNS server.
        var ctx = context_package.WithValue(context_package.Background(), new nettrace.LookupIPAltResolverKey(nil), (slice<net.IPAddr>, error) (context.Context ctxΔ1, @string network, @string host) => {
            if (host != punyDomain) {
                Ꮡt.Errorf("got DNS host lookup for %q/%q; want %q"u8, network, host, punyDomain);
                return (default!, default!);
            }
            return (new net.IPAddr[]{new(IP: net.ParseIP(ip))}.slice(), default!);
        });
        var (req, _) = NewRequest(getˢ2, cst.scheme() + "://"u8 + uniDomain + ":"u8 + port, default!);
        var trace = Ꮡ(new httptrace.ClientTrace(
            GetConn: (@string hostPort) => {
                @string want = net.JoinHostPort(punyDomain, port);
                if (hostPort != want) {
                    Ꮡt.Errorf("getting conn for %q; want %q"u8, hostPort, want);
                }
            },
            DNSStart: (httptrace.DNSStartInfo e) => {
                if (e.Host != punyDomain) {
                    Ꮡt.Errorf("DNSStart Host = %q; want %q"u8, e.Host, punyDomain);
                }
            }
        ));
        req = req.WithContext(httptrace.WithClientTrace(ctx, trace));
        (var res, err) = (~cst).tr.RoundTrip(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).Header.Get(hitHandlerˢ) != "1"u8) {
            var (@out, errΔ1) = httputil.DumpResponse(res, true);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            Ꮡt.Errorf("Response body wasn't from Handler. Got:\n%s\n"u8, @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 13290: send User-Agent in proxy CONNECT
public static void TestTransportProxyConnectHeader(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportProxyConnectHeader(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testTransportProxyConnectHeader(ж<testing.T> Ꮡt, testMode mode) {
    var reqc = new channel<ж<Δhttp.Request>>(1);
    var reqcʗ1 = reqc;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        if ((~rΔ1).Method != "CONNECT"u8) {
            Ꮡt.Errorf("method = %q; want CONNECT"u8, (~rΔ1).Method);
        }
        reqcʗ1.ᐸꟷ(rΔ1);
        var (cΔ1, _, errΔ1) = w._<Hijacker>().Hijack();
        if (errΔ1 != default!) {
            Ꮡt.Errorf("Hijack: %v"u8, errΔ1);
            return;
        }
        cΔ1.Close();
    }))).Value.ts;
    var c = ts.Client();
    var tsʗ1 = ts;
    (~c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = (ж<Δhttp.Request> rΔ2) => url.Parse((~tsʗ1).URL);
    (~c).Transport._<ж<Δhttp.Transport>>().Value.ProxyConnectHeader = new httpꓸHeader(new map<@string, slice<@string>>{
        ["User-Agent"u8] = new @string[]{"foo"u8}.slice(),
        ["Other"u8] = new @string[]{"bar"u8}.slice()
    });
    var (res, err) = c.Get(httpsDummyTldˢ); // https to force a CONNECT
    if (err == default!) {
        (~res).Body.Close();
        Ꮡt.Errorf("unexpected success"u8);
    }
    var r = ᐸꟷ(reqc);
    {
        @string got = (~r).Header.Get(userAgentˢ2);
        @string want = fooˢ; if (got != want) {
            Ꮡt.Errorf("CONNECT request User-Agent = %q; want %q"u8, got, want);
        }
    }
    {
        @string got = (~r).Header.Get(otherˢ2);
        @string want = barˢ; if (got != want) {
            Ꮡt.Errorf("CONNECT request Other = %q; want %q"u8, got, want);
        }
    }
}

public static void TestTransportProxyGetConnectHeader(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportProxyGetConnectHeader(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string foo2ˢ = "foo2"u8;

internal static void testTransportProxyGetConnectHeader(ж<testing.T> Ꮡt, testMode mode) {
    var reqc = new channel<ж<Δhttp.Request>>(1);
    var reqcʗ1 = reqc;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
        if ((~rΔ1).Method != "CONNECT"u8) {
            Ꮡt.Errorf("method = %q; want CONNECT"u8, (~rΔ1).Method);
        }
        reqcʗ1.ᐸꟷ(rΔ1);
        var (cΔ1, _, errΔ1) = w._<Hijacker>().Hijack();
        if (errΔ1 != default!) {
            Ꮡt.Errorf("Hijack: %v"u8, errΔ1);
            return;
        }
        cΔ1.Close();
    }))).Value.ts;
    var c = ts.Client();
    var tsʗ1 = ts;
    (~c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = (ж<Δhttp.Request> rΔ2) => url.Parse((~tsʗ1).URL);
    // These should be ignored:
    (~c).Transport._<ж<Δhttp.Transport>>().Value.ProxyConnectHeader = new httpꓸHeader(new map<@string, slice<@string>>{
        ["User-Agent"u8] = new @string[]{"foo"u8}.slice(),
        ["Other"u8] = new @string[]{"bar"u8}.slice()
    });
    (~c).Transport._<ж<Δhttp.Transport>>().Value.GetProxyConnectHeader = (httpꓸHeader, error) (context.Context ctx, ж<url.URL> proxyURL, @string target) => (new httpꓸHeader(new map<@string, slice<@string>>{
            ["User-Agent"u8] = new @string[]{"foo2"u8}.slice(),
            ["Other"u8] = new @string[]{"bar2"u8}.slice()
        }), default!);
    var (res, err) = c.Get(httpsDummyTldˢ); // https to force a CONNECT
    if (err == default!) {
        (~res).Body.Close();
        Ꮡt.Errorf("unexpected success"u8);
    }
    var r = ᐸꟷ(reqc);
    {
        @string got = (~r).Header.Get(userAgentˢ2);
        @string want = foo2ˢ; if (got != want) {
            Ꮡt.Errorf("CONNECT request User-Agent = %q; want %q"u8, got, want);
        }
    }
    {
        @string got = (~r).Header.Get(otherˢ2);
        @string want = bar2ˢ; if (got != want) {
            Ꮡt.Errorf("CONNECT request Other = %q; want %q"u8, got, want);
        }
    }
}

internal static error errFakeRoundTrip = errors.New("fake roundtrip"u8);

internal delegate void funcRoundTripper();

internal static (ж<Δhttp.Response>, error) RoundTrip(this funcRoundTripper fn, ж<Δhttp.Request> _) {
    fn();
    return (default!, errFakeRoundTrip);
}

internal static error wantBody(ж<Δhttp.Response> Ꮡres, error err, @string want) {
    ref var res = ref Ꮡres.DerefOrNull();

    if (err != default!) {
        return err;
    }
    (var slurp, err) = io.ReadAll(res.Body);
    if (err != default!) {
        return fmt.Errorf("error reading body: %v"u8, err);
    }
    if (((sstring)slurp) != want) {
        return fmt.Errorf("body = %q; want %q"u8, slurp, want);
    }
    {
        var errΔ1 = res.Body.Close(); if (errΔ1 != default!) {
            return fmt.Errorf("body Close = %v"u8, errΔ1);
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcp6ˢ = "tcp6"u8;

internal static net.Listener newLocalListener(ж<testing.T> Ꮡt) {
    var (ln, err) = net.Listen(tcpˢ, "127.0.0.1:0"u8);
    if (err != default!) {
        (ln, err) = net.Listen(tcp6ˢ, "[::1]:0"u8);
    }
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return ln;
}

[GoType] partial struct countCloseReader {
    internal ж<nint> n;
    [GoEmbedded] public io_package.Reader Reader;
}

internal static error Close(this countCloseReader cr) {
    (cr.n.Value)++;
    return default!;
}

// rgz is a gzip quine that uncompresses to itself.
internal static slice<byte> rgz = new byte[]{
    0x1f, 0x8b, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x72, 0x65, 0x63, 0x75, 0x72, 0x73,
    0x69, 0x76, 0x65, 0x00, 0x92, 0xef, 0xe6, 0xe0,
    0x60, 0x00, 0x83, 0xa2, 0xd4, 0xe4, 0xd2, 0xa2,
    0xe2, 0xcc, 0xb2, 0x54, 0x06, 0x00, 0x00, 0x17,
    0x00, 0xe8, 0xff, 0x92, 0xef, 0xe6, 0xe0, 0x60,
    0x00, 0x83, 0xa2, 0xd4, 0xe4, 0xd2, 0xa2, 0xe2,
    0xcc, 0xb2, 0x54, 0x06, 0x00, 0x00, 0x17, 0x00,
    0xe8, 0xff, 0x42, 0x12, 0x46, 0x16, 0x06, 0x00,
    0x05, 0x00, 0xfa, 0xff, 0x42, 0x12, 0x46, 0x16,
    0x06, 0x00, 0x05, 0x00, 0xfa, 0xff, 0x00, 0x05,
    0x00, 0xfa, 0xff, 0x00, 0x14, 0x00, 0xeb, 0xff,
    0x42, 0x12, 0x46, 0x16, 0x06, 0x00, 0x05, 0x00,
    0xfa, 0xff, 0x00, 0x05, 0x00, 0xfa, 0xff, 0x00,
    0x14, 0x00, 0xeb, 0xff, 0x42, 0x88, 0x21, 0xc4,
    0x00, 0x00, 0x14, 0x00, 0xeb, 0xff, 0x42, 0x88,
    0x21, 0xc4, 0x00, 0x00, 0x14, 0x00, 0xeb, 0xff,
    0x42, 0x88, 0x21, 0xc4, 0x00, 0x00, 0x14, 0x00,
    0xeb, 0xff, 0x42, 0x88, 0x21, 0xc4, 0x00, 0x00,
    0x14, 0x00, 0xeb, 0xff, 0x42, 0x88, 0x21, 0xc4,
    0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00,
    0x00, 0xff, 0xff, 0x00, 0x17, 0x00, 0xe8, 0xff,
    0x42, 0x88, 0x21, 0xc4, 0x00, 0x00, 0x00, 0x00,
    0xff, 0xff, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00,
    0x17, 0x00, 0xe8, 0xff, 0x42, 0x12, 0x46, 0x16,
    0x06, 0x00, 0x00, 0x00, 0xff, 0xff, 0x01, 0x08,
    0x00, 0xf7, 0xff, 0x3d, 0xb1, 0x20, 0x85, 0xfa,
    0x00, 0x00, 0x00, 0x42, 0x12, 0x46, 0x16, 0x06,
    0x00, 0x00, 0x00, 0xff, 0xff, 0x01, 0x08, 0x00,
    0xf7, 0xff, 0x3d, 0xb1, 0x20, 0x85, 0xfa, 0x00,
    0x00, 0x00, 0x3d, 0xb1, 0x20, 0x85, 0xfa, 0x00,
    0x00, 0x00
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object panickedExpectingAnErrorˢ = (@string)"panicked, expecting an error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ3 = "unknown status code"u8;

// Ensure that a missing status doesn't make the server panic
// See Issue https://golang.org/issues/21701
public static partial void TestMissingStatusNoPanic(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string want = wantᶜ3;
    var ln = newLocalListener(Ꮡt);
    @string addr = ln.Addr().String();
    var done = new channel<bool>(0);
    @string fullAddrURL = fmt.Sprintf("http://%s"u8, addr);
    @string raw = "HTTP/1.1 400\r\n"u8 + "Date: Wed, 30 Aug 2017 19:09:27 GMT\r\n"u8 + "Content-Type: text/html; charset=utf-8\r\n"u8 + "Content-Length: 10\r\n"u8 + "Last-Modified: Wed, 30 Aug 2017 19:02:02 GMT\r\n"u8 + "Vary: Accept-Encoding\r\n\r\n"u8 + "Aloha Olaa"u8;
    var doneʗ1 = done;
    var lnʗ1 = ln;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
            var (conn, _) = lnʗ1.Accept();
            if (conn != default!) {
                io.WriteString(new http_test_package.net_ConnᴠWriter(conn), raw);
                io.ReadAll(new http_test_package.net_ConnᴠReader(conn));
                conn.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var (proxyURL, err) = url.Parse(fullAddrURL);
    if (err != default!) {
        Ꮡt.Fatalf("proxyURL: %v"u8, err);
    }
    var tr = Ꮡ(new Transport(Proxy: ProxyURL(proxyURL)));
    var (req, _) = NewRequest(getˢ2, httpsGolangOrgˢ, default!);
    (var res, err, var panicked) = doFetchCheckPanic(tr, req);
    if (panicked) {
        Ꮡt.Error(panickedExpectingAnErrorˢ);
    }
    if (res != nil && (~res).Body != default!) {
        io.Copy(io.Discard, (~res).Body);
        (~res).Body.Close();
    }
    if (err == default! || !strings.Contains(err.Error(), want)) {
        Ꮡt.Errorf("got=%v want=%q"u8, err, want);
    }
    ln.Close();
    ᐸꟷ(done);
}

internal static (ж<Δhttp.Response> res, error err, bool panicked) doFetchCheckPanic(ж<Δhttp.Transport> Ꮡtr, ж<Δhttp.Request> Ꮡreq) {
    ж<Δhttp.Response> res = default!;
    error err = default!;
    bool panicked = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    panicked = true;
                }
            }
        }, ref ᒐ);
        (res, err) = Ꮡtr.RoundTrip(Ꮡreq);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (res, err, panicked);
}

// Issue 22330: do not allow the response body to be read when the status code
// forbids a response body.
public static void TestNoBodyOnChunked304Response(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testNoBodyOnChunked304Response(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testNoBodyOnChunked304Response(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (conn, buf, _) = w._<Hijacker>().Hijack();
        buf.Value.Writer.Value.Write(slice<byte>("HTTP/1.1 304 NOT MODIFIED\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n\r\n"u8));
        buf.Value.Writer.Value.Flush();
        conn.Close();
    })));
    // Our test server above is sending back bogus data after the
    // response (the "0\r\n\r\n" part), which causes the Transport
    // code to log spam. Disable keep-alives so we never even try
    // to reuse the connection.
    cst.Value.tr.Value.DisableKeepAlives = true;
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!AreEqual((~res).Body, NoBody)) {
        Ꮡt.Errorf("Unexpected body on 304 response"u8);
    }
}

internal delegate (nint, error) funcWriter(slice<byte> _Δp0);

internal static partial (nint, error) Write(this funcWriter f, slice<byte> p) {
    return f(p);
}

[GoType] partial struct doneContext {
    [GoEmbedded] public context_package.Context Context;
    internal error err;
}

internal static /*<-*/channel<EmptyStruct> Done(this doneContext _) {
    var c = new channel<EmptyStruct>(0);
    builtin.close(c);
    return c.WithDirection(GoChanDir.Recv);
}

internal static error Err(this doneContext d) {
    return d.err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFakeExampleˢ = "http://fake.example/"u8;
internal static readonly @string someErrorˢ = "some error"u8;

// Issue 25852: Transport should check whether Context is done early.
public static void TestTransportCheckContextDoneEarly(ж<testing.T> Ꮡt) {
    var tr = Ꮡ(new Transport(nil));
    var (req, _) = NewRequest(getˢ2, httpFakeExampleˢ, default!);
    var wantErr = errors.New(someErrorˢ);
    req = req.WithContext(new doneContext(context_package.Background(), wantErr));
    var (_, err) = tr.RoundTrip(req);
    if (!AreEqual(err, wantErr)) {
        Ꮡt.Errorf("error = %v; want %v"u8, err, wantErr);
    }
}

// Issue 23399: verify that if a client request times out, the Transport's
// conn is closed so that it's not reused.
//
// This is the test variant that times out before the server replies with
// any response headers.
public static void TestClientTimeoutKillsConn_BeforeHeaders(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientTimeoutKillsConn_BeforeHeaders(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedGetSuccessˢ = (@string)"unexpected Get success"u8;

internal static void testClientTimeoutKillsConn_BeforeHeaders(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var timeout = 1 * time.Millisecond;
    while (ᐧ) {
        var inHandler = new channel<bool>(0);
        var cancelHandler = new channel<EmptyStruct>(0);
        var handlerDone = new channel<bool>(0);
        var cancelHandlerʗ1 = cancelHandler;
        var handlerDoneʗ1 = handlerDone;
        var inHandlerʗ1 = inHandler;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                ᐸꟷ(r.Context().Done());
                var selᴛ81 = cancelHandlerʗ1;
                var selᴛ82 = inHandlerʗ1.ᐸꟷ(true, ꓸꓸꓸ);
                switch (select(ᐸꟷ(selᴛ81, ꓸꓸꓸ), selᴛ82)) {
                case 0 when selᴛ81.ꟷᐳ(out _): {
                    return;
                }
                case 1: {
                    break;
                }}
                var handlerDoneʗ2 = handlerDoneʗ1;
                defer(() => {
                    handlerDoneʗ2.ᐸꟷ(true);
                }, ref ᒐ);
                // Read from the conn until EOF to verify that it was correctly closed.
                var (conn, _, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                (var n, errΔ1) = conn.Read(new byte[]{0}.slice());
                if (n != 0 || !AreEqual(errΔ1, io.EOF)) {
                    Ꮡt.Errorf("unexpected Read result: %v, %v"u8, n, errΔ1);
                }
                conn.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        cst.Value.c.Value.Timeout = timeout;
        var (_, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err == default!) {
            builtin.close(cancelHandler);
            Ꮡt.Fatal(unexpectedGetSuccessˢ);
        }
        var tooSlow = time.NewTimer(timeout * 10);
        var selᴛ83 = (~tooSlow).C;
        var selᴛ84 = inHandler;
        switch (select(ᐸꟷ(selᴛ83, ꓸꓸꓸ), ᐸꟷ(selᴛ84, ꓸꓸꓸ))) {
        case 0 when selᴛ83.ꟷᐳ(out _): {
            Ꮡt.Logf("no handler seen in %v; retrying with longer timeout"u8, // If we didn't get into the Handler, that probably means the builder was
 // just slow and the Get failed in that time but never made it to the
 // server. That's fine; we'll try again with a longer timeout.
 timeout);
            builtin.close(cancelHandler);
            cst.close();
            timeout *= 2;
            continue;
            break;
        }
        case 1 when selᴛ84.ꟷᐳ(out _): {
            tooSlow.Stop();
            ᐸꟷ(handlerDone);
            break;
        }}
        break;
    }
}

// Issue 23399: verify that if a client request times out, the Transport's
// conn is closed so that it's not reused.
//
// This is the test variant that has the server send response headers
// first, and time out during the write of the response body.
public static void TestClientTimeoutKillsConn_AfterHeaders(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientTimeoutKillsConn_AfterHeaders(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testClientTimeoutKillsConn_AfterHeaders(ж<testing.T> Ꮡt, testMode mode) {
    var inHandler = new channel<bool>(0);
    var cancelHandler = new channel<EmptyStruct>(0);
    var handlerDone = new channel<bool>(0);
    var cancelHandlerʗ1 = cancelHandler;
    var handlerDoneʗ1 = handlerDone;
    var inHandlerʗ1 = inHandler;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            w.Header().Set(contentLengthˢ, "100"u8);
            w._<Flusher>().Flush();
            var selᴛ85 = cancelHandlerʗ1;
            var selᴛ86 = inHandlerʗ1.ᐸꟷ(true, ꓸꓸꓸ);
            switch (select(ᐸꟷ(selᴛ85, ꓸꓸꓸ), selᴛ86)) {
            case 0 when selᴛ85.ꟷᐳ(out _): {
                return;
            }
            case 1: {
                break;
            }}
            var handlerDoneʗ2 = handlerDoneʗ1;
            defer(() => {
                handlerDoneʗ2.ᐸꟷ(true);
            }, ref ᒐ);
            var (conn, _, errΔ1) = w._<Hijacker>().Hijack();
            if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
                return;
            }
            conn.Write(slice<byte>("foo"u8));
            (var n, errΔ1) = conn.Read(new byte[]{0}.slice());
            // The error should be io.EOF or "read tcp
            // 127.0.0.1:35827->127.0.0.1:40290: read: connection
            // reset by peer" depending on timing. Really we just
            // care that it returns at all. But if it returns with
            // data, that's weird.
            if (n != 0 || errΔ1 == default!) {
                Ꮡt.Errorf("unexpected Read result: %v, %v"u8, n, errΔ1);
            }
            conn.Close();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })));
    // Set Timeout to something very long but non-zero to exercise
    // the codepaths that check for it. But rather than wait for it to fire
    // (which would make the test slow), we send on the req.Cancel channel instead,
    // which happens to exercise the same code paths.
    cst.Value.c.Value.Timeout = (time.Duration)(86400000000000L); // just to be non-zero, not to hit it.
    var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
    var cancelReq = new channel<EmptyStruct>(0);
    req.Value.Cancel = cancelReq.WithDirection(GoChanDir.Recv);
    var (res, err) = (~cst).c.Do(req);
    if (err != default!) {
        builtin.close(cancelHandler);
        Ꮡt.Fatalf("Get error: %v"u8, err);
    }
    // Cancel the request while the handler is still blocked on sending to the
    // inHandler channel. Then read it until it fails, to verify that the
    // connection is broken before the handler itself closes it.
    builtin.close(cancelReq);
    (var got, err) = io.ReadAll((~res).Body);
    if (err == default!) {
        Ꮡt.Errorf("unexpected success; read %q, nil"u8, got);
    }
    // Now unblock the handler and wait for it to complete.
    ᐸꟷ(inHandler);
    ᐸꟷ(handlerDone);
}

public static void TestTransportResponseBodyWritableOnProtocolSwitch(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportResponseBodyWritableOnProtocolSwitch(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11101Switchingˢ = "HTTP/1.1 101 Switching Protocols Hi\r\nConnection: upgRADe\r\nUpgrade: foo\r\n\r\nSome buffered data\n"u8;
internal static readonly @string upgradeˢ2 = "upgrade"u8;
internal static readonly @string someBufferedDataˢ = "Some buffered data"u8;
internal static readonly @string echoˢ2 = "echo\n"u8;
internal static readonly @string echoˢ3 = "ECHO"u8;

internal static void testTransportResponseBodyWritableOnProtocolSwitch(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var done = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), done, ref ᒐ);
        var doneʗ1 = done;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                var (conn, _, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                io.WriteString(new http_test_package.net_ConnᴠWriter(conn), http11101Switchingˢ);
                var bsΔ1 = bufio.NewScanner(new http_test_package.net_ConnᴠReader(conn));
                bsΔ1.Scan();
                fmt.Fprintf(new http_test_package.net_ConnᴠWriter(conn), "%s\n"u8, strings.ToUpper(bsΔ1.Text()));
                ᐸꟷ(doneʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        (~req).Header.Set(upgradeˢ, fooˢ);
        (~req).Header.Set(connectionˢ, upgradeˢ2);
        var (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != 101) {
            Ꮡt.Fatalf("expected 101 switching protocols; got %v, %v"u8, (~res).Status, (~res).Header);
        }
        var (rwc, ok) = (~res).Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("expected a ReadWriteCloser; got a %T"u8, (~res).Body);
        }
        var rwcʗ1 = rwc;
        defer(() => rwcʗ1.Close(), ref ᒐ);
        var bs = bufio.NewScanner(rwc);
        if (!bs.Scan()) {
            Ꮡt.Fatalf("expected readable input"u8);
        }
        {
            @string got = bs.Text();
            @string want = someBufferedDataˢ; if (got != want) {
                Ꮡt.Errorf("read %q; want %q"u8, got, want);
            }
        }
        io.WriteString(rwc, echoˢ2);
        if (!bs.Scan()) {
            Ꮡt.Fatalf("expected another line"u8);
        }
        {
            @string got = bs.Text();
            @string want = echoˢ3; if (got != want) {
                Ꮡt.Errorf("read %q; want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTransportCONNECTBidi(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportCONNECTBidi(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connectˢ = "CONNECT"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string targetᶜ = "backend:443"u8;

internal static void testTransportCONNECTBidi(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string target = targetᶜ;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                if ((~r).Method != "CONNECT"u8) {
                    Ꮡt.Errorf("unexpected method %q"u8, (~r).Method);
                    w.WriteHeader(500);
                    return;
                }
                if ((~r).RequestURI != target) {
                    Ꮡt.Errorf("unexpected CONNECT target %q"u8, (~r).RequestURI);
                    w.WriteHeader(500);
                    return;
                }
                var (nc, brw, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var ncʗ1 = nc;
                defer(() => ncʗ1.Close(), ref ᒐ);
                nc.Write(slice<byte>("HTTP/1.1 200 OK\r\n\r\n"u8));
                // Switch to a little protocol that capitalize its input lines:
                while (ᐧ) {
                    var (line, errΔ2) = brw.Value.Reader.Value.ReadString((rune)'\n');
                    if (errΔ2 != default!) {
                        if (!AreEqual(errΔ2, io.EOF)) {
                            Ꮡt.Error(errΔ2);
                        }
                        return;
                    }
                    io.WriteString(new http_test_package.bufio_ReadWriterжWriter(brw), strings.ToUpper(line));
                    brw.Value.Writer.Value.Flush();
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var (pr, pw) = io.Pipe();
        var pwʗ1 = pw;
        defer(() => pwʗ1.Close(), ref ᒐ);
        var (req, err) = NewRequest(connectˢ, (~(~cst).ts).URL, new io.PipeReaderжReader(pr));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        req.Value.URL.Value.Opaque = target;
        (var res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != 200) {
            Ꮡt.Fatalf("status code = %d; want 200"u8, (~res).StatusCode);
        }
        var br = bufio.NewReader((~res).Body);
        foreach (var (_, str) in new @string[]{"foo"u8, "bar"u8, "baz"u8}.slice()) {
            fmt.Fprintf(new io.PipeWriterжWriter(pw), "%s\n"u8, str);
            var (got, errΔ3) = br.ReadString((rune)'\n');
            if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
            got = strings.TrimSpace(got);
            @string want = strings.ToUpper(str);
            if (got != want) {
                Ꮡt.Fatalf("got %q; want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestTransportRequestReplayable_tests {
    internal @string name;
    internal ж<Δhttp.Request> req;
    internal bool want;
}

public static void TestTransportRequestReplayable(ж<testing.T> Ꮡt) {
    var someBody = io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)));
    var tests = new TestTransportRequestReplayable_tests[]{
        new(
            name: "GET"u8,
            req: Ꮡ(new Request(Method: "GET"u8)),
            want: true
        ),
        new(
            name: "GET_http.NoBody"u8,
            req: Ꮡ(new Request(Method: "GET"u8, Body: NoBody)),
            want: true
        ),
        new(
            name: "GET_body"u8,
            req: Ꮡ(new Request(Method: "GET"u8, Body: someBody)),
            want: false
        ),
        new(
            name: "POST"u8,
            req: Ꮡ(new Request(Method: "POST"u8)),
            want: false
        ),
        new(
            name: "POST_idempotency-key"u8,
            req: Ꮡ(new Request(Method: "POST"u8, Header: new httpꓸHeader(new map<@string, slice<@string>>{["Idempotency-Key"u8] = new @string[]{"x"u8}.slice()}))),
            want: true
        ),
        new(
            name: "POST_x-idempotency-key"u8,
            req: Ꮡ(new Request(Method: "POST"u8, Header: new httpꓸHeader(new map<@string, slice<@string>>{["X-Idempotency-Key"u8] = new @string[]{"x"u8}.slice()}))),
            want: true
        ),
        new(
            name: "POST_body"u8,
            req: Ꮡ(new Request(Method: "POST"u8, Header: new httpꓸHeader(new map<@string, slice<@string>>{["Idempotency-Key"u8] = new @string[]{"x"u8}.slice()}), Body: someBody)),
            want: false
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestTransportRequestReplayable_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = ttʗ1.req.ExportIsReplayable();
            if (got != ttʗ1.want) {
                tΔ1.Errorf("replyable = %v; want %v"u8, got, ttʗ1.want);
            }
        });
    }
}

// testMockTCPConn is a mock TCP connection used to test that
// ReadFrom is called when sending the request body.
[GoType] partial struct testMockTCPConn {
    public partial ref ж<net_package.TCPConn> TCPConn { get; }
    public bool ReadFromCalled;
}

[GoRecv] internal static (int64, error) ReadFrom(this ref testMockTCPConn c, io.Reader r) {
    c.ReadFromCalled = true;
    return c.TCPConn.ReadFrom(r);
}

public static void TestTransportRequestWriteRoundTrip(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportRequestWriteRoundTrip(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netHttpNewfilefuncˢ = "net-http-newfilefunc"u8;

[GoType("dyn")] internal partial struct testTransportRequestWriteRoundTrip_cases {
    internal @string name;
    internal Func<(io.Reader, Action, error)> readerFunc;
    internal int64 contentLength;
    internal bool expectedReadFrom;
}

internal static void testTransportRequestWriteRoundTrip(ж<testing.T> Ꮡt, testMode mode) {
    var nBytes = (int64)(((int64)1 << (int)(10)));
    var newFileFunc = (io.Reader r, Action done, error err) () => {
        Action done = default!;
        error err = default!;
        (var f, err) = os.CreateTemp(""u8, netHttpNewfilefuncˢ);
        if (err != default!) {
            return (default!, default!, err);
        }
        // Write some bytes to the file to enable reading.
        {
            var (_, errΔ1) = io.CopyN(new os.FileжWriter(f), rand.Reader, nBytes); if (errΔ1 != default!) {
                return (default!, default!, fmt.Errorf("failed to write data to file: %v"u8, errΔ1));
            }
        }
        {
            var (_, errΔ2) = f.Seek(0, 0); if (errΔ2 != default!) {
                return (default!, default!, fmt.Errorf("failed to seek to front: %v"u8, errΔ2));
            }
        }
        var fʗ1 = f;
        done = () => {
            fʗ1.Close();
            os.Remove(fʗ1.Name());
        };
        return (new http_test_package.os_FileжReader(f), done, default!);
    };
    var newBufferFunc = (io.Reader, Action, error) () => (new http_test_package.bytes_BufferжReader(bytes.NewBuffer(new slice<byte>((nint)(nBytes)))), () => {
        }, default!);
    var cases = new testTransportRequestWriteRoundTrip_cases[]{
        new(
            name: "file, length"u8,
            readerFunc: newFileFunc,
            contentLength: nBytes,
            expectedReadFrom: true
        ),
        new(
            name: "file, no length"u8,
            readerFunc: newFileFunc
        ),
        new(
            name: "file, negative length"u8,
            readerFunc: newFileFunc,
            contentLength: -1
        ),
        new(
            name: "buffer"u8,
            contentLength: nBytes,
            readerFunc: newBufferFunc
        ),
        new(
            name: "buffer, no length"u8,
            readerFunc: newBufferFunc
        ),
        new(
            name: "buffer, length -1"u8,
            contentLength: -1,
            readerFunc: newBufferFunc
        )
    }.slice();
    foreach (var (_, vᴛ1) in cases) {
        ref var tc = ref heap(new testTransportRequestWriteRoundTrip_cases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (r, cleanup, err) = tcʗ1.readerFunc();
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var cleanupʗ1 = cleanup;
                defer(cleanupʗ1, ref ᒐ);
                var tConn = Ꮡ(new testMockTCPConn(nil));
                var tConnʗ1 = tConn;
                var trFunc = (ж<Δhttp.Transport> tr) => {
                    var tConnʗ2 = tConnʗ1;
                    tr.Value.DialContext = (net.Conn, error) (context.Context ctx, @string network, @string addr) => {
                        ref var d = ref heap(new net.Dialer(), out var Ꮡd);
                        var (conn, errΔ1) = Ꮡd.DialContext(ctx, network, addr);
                        if (errΔ1 != default!) {
                            return (default!, errΔ1);
                        }
                        var (tcpConn, ok) = conn._<ж<net.TCPConn>>(ᐧ);
                        if (!ok) {
                            return (default!, fmt.Errorf("%s/%s does not provide a *net.TCPConn"u8, network, addr));
                        }
                        tConnʗ2.Value.TCPConn = tcpConn;
                        return (new http_test_package.testMockTCPConnжConn(tConnʗ2), default!);
                    };
                };
                var cst = newClientServerTest(
                    new http_test_package.testing_TжTB(tΔ1),
                    mode,
                    new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
                        io.Copy(io.Discard, (~rΔ1).Body);
                        (~rΔ1).Body.Close();
                        w.WriteHeader(200);
                    })),
                    (trFunc).OrTypedNilFunc());
                (var req, err) = NewRequest(putˢ, (~(~cst).ts).URL, r);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                req.Value.ContentLength = tcʗ1.contentLength;
                (~req).Header.Set(contentTypeˢ, applicationOctetStreamˢ);
                (var resp, err) = (~cst).c.Do(req);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var respʗ1 = resp;
                defer(() => (~respʗ1).Body.Close(), ref ᒐ);
                if ((~resp).StatusCode != 200) {
                    tΔ1.Fatalf("status code = %d; want 200"u8, (~resp).StatusCode);
                }
                var expectedReadFrom = tcʗ1.expectedReadFrom;
                if (mode != http1Mode) {
                    expectedReadFrom = false;
                }
                if (!(~tConn).ReadFromCalled && expectedReadFrom) {
                    tΔ1.Fatalf("did not call ReadFrom"u8);
                }
                if ((~tConn).ReadFromCalled && !expectedReadFrom) {
                    tΔ1.Fatalf("ReadFrom was unexpectedly invoked"u8);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestTransportClone(ж<testing.T> Ꮡt) {
    var tr = Ꮡ(new Transport(
        Proxy: (ж<Δhttp.Request> _Δp0) => {
            throw panic("");
        },
        OnProxyConnectResponse: (context.Context ctx, ж<url.URL> proxyURL, ж<Δhttp.Request> connectReq, ж<Δhttp.Response> connectRes) => default!,
        DialContext: (context.Context ctx, @string network, @string addr) => {
            throw panic("");
        },
        Dial: (@string network, @string addr) => {
            throw panic("");
        },
        DialTLS: (@string network, @string addr) => {
            throw panic("");
        },
        DialTLSContext: (context.Context ctx, @string network, @string addr) => {
            throw panic("");
        },
        TLSClientConfig: @new<tls.Config>(),
        TLSHandshakeTimeout: time.ΔSecond,
        DisableKeepAlives: true,
        DisableCompression: true,
        MaxIdleConns: 1,
        MaxIdleConnsPerHost: 1,
        MaxConnsPerHost: 1,
        IdleConnTimeout: time.ΔSecond,
        ResponseHeaderTimeout: time.ΔSecond,
        ExpectContinueTimeout: time.ΔSecond,
        ProxyConnectHeader: new httpꓸHeader(new map<@string, slice<@string>>{}),
        GetProxyConnectHeader: (context.Context _Δp0, ж<url.URL> _Δp1, @string _Δp2) => (default!, default!),
        MaxResponseHeaderBytes: 1,
        ForceAttemptHTTP2: true,
        HTTP2: Ꮡ(new HTTP2Config(MaxConcurrentStreams: 1)),
        Protocols: Ꮡ(new Protocols(nil)),
        TLSNextProto: new map<@string, Func<@string, ж<tls.Conn>, Δhttp.RoundTripper>>{
            ["foo"u8] = (@string authority, ж<tls.Conn> c) => {
                throw panic("");
            }
        },
        ReadBufferSize: 1,
        WriteBufferSize: 1
    ));
    (~tr).Protocols.SetHTTP1(true);
    (~tr).Protocols.SetHTTP2(true);
    var tr2 = tr.Clone();
    var rv = reflect.ValueOf(tr2.OrTypedNil()).Elem();
    var rt = rv.Type();
    for (nint i = 0; i < rt.NumField(); i++) {
        var sf = rt.Field(i);
        if (!token.IsExported(sf.Name)) {
            continue;
        }
        if (rv.Field(i).IsZero()) {
            Ꮡt.Errorf("cloned field t2.%s is zero"u8, sf.Name);
        }
    }
    {
        var (_, ok) = (~tr2).TLSNextProto[fooˢ, ꟷ]; if (!ok) {
            Ꮡt.Errorf("cloned Transport lacked TLSNextProto 'foo' key"u8);
        }
    }
    // But test that a nil TLSNextProto is kept nil:
    tr = @new<Δhttp.Transport>();
    tr2 = tr.Clone();
    if ((~tr2).TLSNextProto != default!) {
        Ꮡt.Errorf("Transport.TLSNextProto unexpected non-nil"u8);
    }
}

[GoType("dyn")] internal partial struct TestIs408_tests {
    internal @string @in;
    internal bool want;
}

public static void TestIs408(ж<testing.T> Ꮡt) {
    var tests = new TestIs408_tests[]{
        new("HTTP/1.0 408"u8, true),
        new("HTTP/1.1 408"u8, true),
        new("HTTP/1.8 408"u8, true),
        new("HTTP/2.0 408"u8, false), // maybe h2c would do this? but false for now.

        new("HTTP/1.1 408 "u8, true),
        new("HTTP/1.1 40"u8, false),
        new("http/1.0 408"u8, false),
        new("HTTP/1-1 408"u8, false)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            var got = http_internal_test_package.Export_is408Message(slice<byte>(tt.@in)); if (got != tt.want) {
                Ꮡt.Errorf("is408Message(%q) = %v; want %v"u8, tt.@in, got, tt.want);
            }
        }
    }
}

public static void TestTransportIgnores408(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportIgnores408(Δp0, Δp1), new testMode[]{http1Mode}.slice(), testNotParallel);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string targetᶜ1 = "backend:443"u8;

internal static void testTransportIgnores408(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        // Not parallel. Relies on mutating the log package's global Output.
        defer(log.SetOutput, log.Writer(), ref ᒐ);
        ref var logout = ref heap(new strings.Builder(), out var Ꮡlogout);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogout));
        @string target = targetᶜ1;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                var (nc, _, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var ncʗ1 = nc;
                defer(() => ncʗ1.Close(), ref ᒐ);
                nc.Write(slice<byte>("HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nok"u8));
                nc.Write(slice<byte>("HTTP/1.1 408 bye\r\n"u8)); // changing 408 to 409 makes test fail
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var (req, err) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)slurp) != "ok"u8) {
            Ꮡt.Fatalf("got %q; want ok"u8, slurp);
        }
        var cstʗ1 = cst;
        waitCondition(new http_test_package.testing_TжTB(Ꮡt), 1 * time.Millisecond, (time.Duration d) => {
            {
                nint n = (~cstʗ1).tr.IdleConnKeyCountForTesting(); if (n != 0) {
                    if (d > 0) {
                        Ꮡt.Logf("%v idle conns still present after %v"u8, n, d);
                    }
                    return false;
                }
            }
            return true;
        });
        {
            @string got = logout.String(); if (got != ""u8) {
                Ꮡt.Fatalf("expected no log output; got: %s"u8, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestInvalidHeaderResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testInvalidHeaderResponse(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ8 = "Foo "u8;

internal static void testInvalidHeaderResponse(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (conn, buf, _) = w._<Hijacker>().Hijack();
            buf.Value.Writer.Value.Write(slice<byte>((@string)("HTTP/1.1 200 OK\r\n"u8 + "Date: Wed, 30 Aug 2017 19:09:27 GMT\r\n"u8 + "Content-Type: text/html; charset=utf-8\r\n"u8 + "Content-Length: 0\r\n"u8 + "Foo : bar\r\n\r\n"u8)));
            buf.Value.Writer.Value.Flush();
            conn.Close();
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        {
            @string v = (~res).Header.Get(fooˢ2); if (v != ""u8) {
                Ꮡt.Errorf(@"unexpected ""Foo"" header: %q"u8, v);
            }
        }
        {
            @string v = (~res).Header.Get(fooˢ8); if (v != "bar"u8) {
                Ꮡt.Errorf(@"bad ""Foo "" header value: %q, want %q"u8, v, barˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("bool")] partial struct bodyCloser;

[GoRecv] internal static error Close(this ref bodyCloser bc) {
    bc = true;
    return default!;
}

[GoRecv] internal static (nint n, error err) Read(this ref bodyCloser bc, slice<byte> b) {
    return (0, io.EOF);
}

// Issue 35015: ensure that Transport closes the body on any error
// with an invalid request, as promised by Client.Do docs.
public static void TestTransportClosesBodyOnInvalidRequests(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportClosesBodyOnInvalidRequests(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorˢ2 = (@string)"Expected an error"u8;
internal static readonly object expectedBodyToHaveBeenˢ = (@string)"Expected body to have been closed"u8;

[GoType("dyn")] internal partial struct testTransportClosesBodyOnInvalidRequests_tests {
    internal @string name;
    internal ж<Δhttp.Request> req;
    internal @string wantErr;
}

internal static void testTransportClosesBodyOnInvalidRequests(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡt.Errorf("Should not have been invoked"u8);
    }))).Value.ts;
    var (u, _) = url.Parse((~cst).URL);
    var tests = new testTransportClosesBodyOnInvalidRequests_tests[]{
        new(
            name: "invalid method"u8,
            req: Ꮡ(new Request(
                Method: " "u8,
                URL: u
            )),
            wantErr: @"invalid method "" """u8
        ),
        new(
            name: "nil URL"u8,
            req: Ꮡ(new Request(
                Method: "GET"u8
            )),
            wantErr: @"nil Request.URL"u8
        ),
        new(
            name: "invalid header key"u8,
            req: Ꮡ(new Request(
                Method: "GET"u8,
                Header: new httpꓸHeader(new map<@string, slice<@string>>{["💡"u8] = new @string[]{"emoji"u8}.slice()}),
                URL: u
            )),
            wantErr: @"invalid header field name ""💡"""u8
        ),
        new(
            name: "invalid header value"u8,
            req: Ꮡ(new Request(
                Method: "POST"u8,
                Header: new httpꓸHeader(new map<@string, slice<@string>>{["key"u8] = new @string[]{"\x19"u8}.slice()}),
                URL: u
            )),
            wantErr: @"invalid header field value for ""key"""u8
        ),
        new(
            name: "non HTTP(s) scheme"u8,
            req: Ꮡ(new Request(
                Method: "POST"u8,
                URL: Ꮡ(new url.URL(Scheme: "faux"u8))
            )),
            wantErr: @"unsupported protocol scheme ""faux"""u8
        ),
        new(
            name: "no Host in URL"u8,
            req: Ꮡ(new Request(
                Method: "POST"u8,
                URL: Ꮡ(new url.URL(Scheme: "http"u8))
            )),
            wantErr: @"no Host in request URL"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new testTransportClosesBodyOnInvalidRequests_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var cstʗ1 = cst;
        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var bc = ref heap(new bodyCloser(), out var Ꮡbc);
            var req = ttʗ1.req;
            req.Value.Body = new http_test_package.bodyCloserжReadCloser(Ꮡbc);
            var (_, err) = cstʗ1.Client().Do(ttʗ1.req);
            if (err == default!) {
                tΔ1.Fatal(expectedAnErrorˢ2);
            }
            if (((bodyCloser)(!(bool)bc))) {
                tΔ1.Fatal(expectedBodyToHaveBeenˢ);
            }
            {
                @string g = err.Error();
                @string w = ttʗ1.wantErr; if (!strings.HasSuffix(g, w)) {
                    tΔ1.Fatalf("Error mismatch: %q does not end with %q"u8, g, w);
                }
            }
        });
    }
}

// breakableConn is a net.Conn wrapper with a Write method
// that will fail when its brokenState is true.
[GoType] partial struct breakableConn {
    [GoEmbedded] public net_package.Conn Conn;
    internal partial ref ж<brokenState> brokenState { get; }
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static error Close(this breakableConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this breakableConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this breakableConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this breakableConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this breakableConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this breakableConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *breakableConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this breakableConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

[GoType] partial struct brokenState {
    public partial ref global::go.sync_package.Mutex Mutex { get; }
    internal bool broken;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someWriteErrorˢ = "some write error"u8;

internal static (nint n, error err) Write(this ж<breakableConn> Ꮡw, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.brokenState.of(brokenState.ᏑMutex).Lock();
        defer(Ꮡw.Value.brokenState.of(brokenState.ᏑMutex).Unlock, ref ᒐ);
        if (w.broken) {
            (n, err) = (0, errors.New(someWriteErrorˢ)); goto ᒐdone;
        }
        (n, err) = w.Conn.Write(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

// Issue 34978: don't cache a broken HTTP/2 connection
public static void TestDontCacheBrokenHTTP2Conn(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testDontCacheBrokenHTTP2Conn(Δp0, Δp1), new testMode[]{http2Mode}.slice());
}

internal static void testDontCacheBrokenHTTP2Conn(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })), (optQuietLog).OrTypedNilFunc());
    ref var brokenState = ref heap(new brokenState(), out var ᏑbrokenState);
    const nint numReqs = 5;
    ref var numDials = ref heap(new uint32(), out var ᏑnumDials);                // atomic
    ref var gotConns = ref heap(new uint32(), out var ᏑgotConns);
    cst.Value.tr.Value.Dial = (@string netw, @string addr) => {
        atomic.AddUint32(ᏑnumDials, 1);
        var (c, err) = net.Dial(netw, addr);
        if (err != default!) {
            Ꮡt.Errorf("unexpected Dial error: %v"u8, err);
            return (default!, err);
        }
        return (new http_test_package.breakableConnжConn(Ꮡ(new breakableConn(c, ᏑbrokenState))), err);
    };
    for (nint i = 1; i <= numReqs; i++) {
        ᏑbrokenState.of(http_test_package.brokenState.ᏑMutex).Lock();
        brokenState.broken = false;
        ᏑbrokenState.of(http_test_package.brokenState.ᏑMutex).Unlock();
        // doBreak controls whether we break the TCP connection after the TLS
        // handshake (before the HTTP/2 handshake). We test a few failures
        // in a row followed by a final success.
        var doBreak = i != numReqs;
        var ctx = httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
            GotConn: (httptrace.GotConnInfo info) => {
                Ꮡt.Logf("got conn: %v, reused=%v, wasIdle=%v, idleTime=%v"u8, info.Conn.LocalAddr(), info.Reused, info.WasIdle, info.IdleTime);
                atomic.AddUint32(ᏑgotConns, 1);
            },
            TLSHandshakeDone: (tlsꓸConnectionState cfg, error errΔ1) => {
                GoFrame ᒐ = default;
                try {
                    ᏑbrokenState.of(http_test_package.brokenState.ᏑMutex).Lock();
                    defer(ᏑbrokenState.of(http_test_package.brokenState.ᏑMutex).Unlock, ref ᒐ);
                    if (doBreak) {
                        ᏑbrokenState.Value.broken = true;
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }
        )));
        var (req, err) = NewRequestWithContext(ctx, getˢ2, (~(~cst).ts).URL, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (_, err) = (~cst).c.Do(req);
        if (doBreak != (err != default!)) {
            Ꮡt.Errorf("for iteration %d, doBreak=%v; unexpected error %v"u8, i, doBreak, err);
        }
    }
    {
        var got = atomic.LoadUint32(ᏑgotConns);
        nint want = 1; if ((nint)got != want) {
            Ꮡt.Errorf("GotConn calls = %v; want %v"u8, got, want);
        }
    }
    {
        var got = atomic.LoadUint32(ᏑnumDials);
        nint want = numReqs; if ((nint)got != want) {
            Ꮡt.Errorf("Dials = %v; want %v"u8, got, want);
        }
    }
}

// Issue 34941
// When the client has too many concurrent requests on a single connection,
// http.http2noCachedConnError is reported on multiple requests. There should
// only be one decrement regardless of the number of failures.
public static void TestTransportDecrementConnWhenIdleConnRemoved(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportDecrementConnWhenIdleConnRemoved(Δp0, Δp1), new testMode[]{http2Mode}.slice());
}

internal static partial void testTransportDecrementConnWhenIdleConnRemoved(ж<testing.T> Ꮡt, testMode mode) {
    http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
    var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (_, err) = w.Write(slice<byte>("foo"u8));
        if (err != default!) {
            Ꮡt.Fatalf("Write: %v"u8, err);
        }
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(h)).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    tr.Value.MaxConnsPerHost = 1;
    var errCh = new channel<error>(300);
    var cʗ1 = c;
    var errChʗ1 = errCh;
    var tsʗ1 = ts;
    void doReq() {
        GoFrame ᒐ = default;
        try {
            var (resp, err) = cʗ1.Get((~tsʗ1).URL);
            if (err != default!) {
                errChʗ1.ᐸꟷ(fmt.Errorf("request failed: %v"u8, err));
                return;
            }
            var respʗ1 = resp;
            defer(() => (~respʗ1).Body.Close(), ref ᒐ);
            (_, err) = io.ReadAll((~resp).Body);
            if (err != default!) {
                errChʗ1.ᐸꟷ(fmt.Errorf("read body failed: %v"u8, err));
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 300; i++) {
        Ꮡwg.Add(1);
        var doReqʗ1 = doReq;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                doReqʗ1();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    builtin.close(errCh);
    foreach (var err in errCh) {
        Ꮡt.Errorf("error occurred: %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cancelˢ = "cancel"u8;
internal static readonly @string cancelBarComPathˢ = "cancel://bar.com/path"u8;
internal static readonly object requestUnexpectedlyˢ = (@string)"request unexpectedly succeeded"u8;

// Issue 36820
// Test that we use the older backward compatible cancellation protocol
// when a RoundTripper is registered via RegisterProtocol.
public static void TestAltProtoCancellation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new Transport(nil));
        var c = Ꮡ(new Client(
            Transport: new Δhttp.TransportжRoundTripper(tr),
            Timeout: time.Millisecond
        ));
        tr.RegisterProtocol(cancelˢ, new cancelProto(nil));
        var (_, err) = c.Get(cancelBarComPathˢ);
        if (err == default!){
            Ꮡt.Error(requestUnexpectedlyˢ);
        } else 
        if (!strings.Contains(err.Error(), errCancelProto.Error())) {
            Ꮡt.Errorf("got error %q, does not contain expected string %q"u8, err, errCancelProto);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error errCancelProto = errors.New("canceled as expected"u8);

[GoType] partial struct cancelProto {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this cancelProto _, ж<Δhttp.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    ᐸꟷ(req.Cancel);
    return (default!, errCancelProto);
}

internal delegate (ж<Δhttp.Response>, error) roundTripFunc(ж<Δhttp.Request> r);

internal static partial (ж<Δhttp.Response>, error) RoundTrip(this roundTripFunc f, ж<Δhttp.Request> Ꮡr) {
    return f(Ꮡr);
}

// Issue 32441: body is not reset after ErrSkipAltProtocol
public static void TestIssue32441(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIssue32441(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bodyLengthIsZeroˢ = (@string)"body length is zero"u8;
internal static readonly object bodyLengthIsZeroDuringˢ = (@string)"body length is zero during round trip"u8;
internal static readonly @string dataˢ = "data"u8;

internal static void testIssue32441(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        {
            var (n, _) = io.Copy(io.Discard, (~r).Body); if (n == 0) {
                Ꮡt.Error(bodyLengthIsZeroˢ);
            }
        }
    }))).Value.ts;
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().RegisterProtocol(httpˢ, new http_test_package.roundTripFuncᴠRoundTripper(new roundTripFunc((ж<Δhttp.Request> r) => {
        // Draining body to trigger failure condition on actual request to server.
        {
            var (n, _) = io.Copy(io.Discard, (~r).Body); if (n == 0) {
                Ꮡt.Error(bodyLengthIsZeroDuringˢ);
            }
        }
        return (default!, ErrSkipAltProtocol);
    })));
    {
        var (_, err) = c.Post((~ts).URL, applicationOctetStreamˢ, new http_test_package.bytes_BufferжReader(bytes.NewBufferString(dataˢ))); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// Issue 39017. Ensure that HTTP/1 transports reject Content-Length headers
// that contain a sign (eg. "+3"), per RFC 2616, Section 14.13.
public static void TestTransportRejectsSignInContentLength(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportRejectsSignInContentLength(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedANonNilErrorAndAˢ = (@string)"Expected a non-nil error and a nil http.Response"u8;
internal static readonly @string badContentLength3ˢ = @"bad Content-Length ""+3"""u8;

internal static void testTransportRejectsSignInContentLength(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentLengthˢ, "+3"u8);
        w.Write(slice<byte>("abc"u8));
    }))).Value.ts;
    var c = cst.Client();
    var (res, err) = c.Get((~cst).URL);
    if (err == default! || res != nil) {
        Ꮡt.Fatal(expectedANonNilErrorAndAˢ);
    }
    {
        @string got = err.Error();
        @string want = badContentLength3ˢ; if (!strings.Contains(got, want)) {
            Ꮡt.Fatalf("Error mismatch\nGot: %q\nWanted substring: %q"u8, got, want);
        }
    }
}

// dumpConn is a net.Conn which writes to Writer and reads from Reader
[GoType] partial struct dumpConn {
    [GoEmbedded] public io_package.Writer Writer;
    [GoEmbedded] public io_package.Reader Reader;
}

[GoRecv] internal static error Close(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static netꓸAddr LocalAddr(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static netꓸAddr RemoteAddr(this ref dumpConn c) {
    return default!;
}

[GoRecv] internal static error SetDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetReadDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

// delegateReader is a reader that delegates to another reader,
// once it arrives on a channel.
[GoType] partial struct delegateReader {
    internal channel<io.Reader> c;
    internal io.Reader r; // nil until received from c
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string delegateClosedˢ = "delegate closed"u8;

[GoRecv] internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) {
    if (r.r == default!) {
        bool ok = default!;
        {
            (r.r, ok) = ᐸꟷ(r.c, ꟷ); if (!ok) {
                return (0, errors.New(delegateClosedˢ));
            }
        }
    }
    return r.r.Read(p);
}

internal static partial void testTransportRace(ж<Δhttp.Request> Ꮡreq) {
    GoFrame ᒐ = default;
    try {
        ref var req = ref Ꮡreq.DerefOrNull();

        var save = req.Body;
        var (pr, pw) = io.Pipe();
        var prʗ1 = pr;
        defer(() => prʗ1.Close(), ref ᒐ);
        var pwʗ1 = pw;
        defer(() => pwʗ1.Close(), ref ᒐ);
        var dr = Ꮡ(new delegateReader(c: new channel<io.Reader>(0)));
            var drʗ1 = dr;
            var pwʗ2 = pw;
        var t = Ꮡ(new Transport(
            Dial: (@string netΔ1, @string addr) => (new http_test_package.dumpConnжConn(Ꮡ(new dumpConn(new io.PipeWriterжWriter(pwʗ2), new http_test_package.delegateReaderжReader(drʗ1)))), default!)
        ));
        var tʗ1 = t;
        defer(tʗ1.CloseIdleConnections, ref ᒐ);
        var quitReadCh = new channel<EmptyStruct>(0);
        // Wait for the request before replying with a dummy response:
        var drʗ2 = dr;
        var prʗ2 = pr;
        var quitReadChʗ1 = quitReadCh;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), quitReadChʗ1, ref ᒐ);
                var (reqΔ1, err) = ReadRequest(bufio.NewReader(new io.PipeReaderжReader(prʗ2)));
                if (err == default!) {
                    // Ensure all the body is read; otherwise
                    // we'll get a partial dump.
                    io.Copy(io.Discard, (~reqΔ1).Body);
                    (~reqΔ1).Body.Close();
                }
                var selᴛ87 = (~drʗ2).c.ᐸꟷ(new http_test_package.strings_ReaderжReader(strings.NewReader(http11204NoContentˢ)), ꓸꓸꓸ);
                var selᴛ88 = quitReadChʗ1.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
                switch (select(selᴛ87, selᴛ88)) {
                case 0: {
                    break;
                }
                case 1: {
                    builtin.close((~drʗ2).c);
                    break;
                }}
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        // Ensure delegate is closed so Read doesn't block forever.
        t.RoundTrip(Ꮡreq);
        // Ensure the reader returns before we reset req.Body to prevent
        // a data race on req.Body.
        pw.Close();
        ᐸꟷ(quitReadCh);
        req.Body = save;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 37669
// Test that a cancellation doesn't result in a data race due to the writeLoop
// goroutine being left running, if the caller mutates the processed Request
// upon completion.
public static void TestErrorWriteLoopRace(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            return;
        }
        Ꮡt.Parallel();
        for (nint i = 0; i < 1000; i++) {
            var delay = ((time.Duration)(int64)mrand.Intn(5)) * time.Millisecond;
            var (ctx, cancel) = context_package.WithTimeout(context_package.Background(), delay);
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);
            var r = bytes.NewBuffer(new slice<byte>(10000));
            var (req, err) = NewRequestWithContext(ctx, MethodPost, httpExampleComˢ, new http_test_package.bytes_BufferжReader(r));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            testTransportRace(req);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 41600
// Test that a new request which uses the connection of an active request
// cannot cause it to be canceled as well.
public static void TestCancelRequestWhenSharingConnection(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testCancelRequestWhenSharingConnection(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static partial void testCancelRequestWhenSharingConnection(ж<testing.T> Ꮡt, testMode mode) {
    var reqc = new channel<channel<EmptyStruct>>(2);
    var reqcʗ1 = reqc;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        var ch = new channel<EmptyStruct>(1);
        reqcʗ1.ᐸꟷ(ch);
        ᐸꟷ(ch);
        w.Header().Add(contentLengthˢ, "0"u8);
    }))).Value.ts;
    var client = ts.Client();
    var transport = (~client).Transport._<ж<Δhttp.Transport>>();
    transport.Value.MaxIdleConns = 1;
    transport.Value.MaxConnsPerHost = 1;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var putidlec = new channel<channel<EmptyStruct>>(1);
    var reqerrc = new channel<error>(1);
    var clientʗ1 = client;
    var putidlecʗ1 = putidlec;
    var reqerrcʗ1 = reqerrc;
    var tsʗ1 = ts;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
                var putidlecʗ2 = putidlecʗ1;
            var ctx = httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
                PutIdleConn: (error _Δp0) => {
                    // Signal that the idle conn has been returned to the pool,
                    // and wait for the order to proceed.
                    var ch = new channel<EmptyStruct>(0);
                    putidlecʗ2.ᐸꟷ(ch);
                    builtin.close(putidlecʗ2); // panic if PutIdleConn runs twice for some reason
                    ᐸꟷ(ch);
                }
            )));
            var (req, _) = NewRequestWithContext(ctx, getˢ2, (~tsʗ1).URL, default!);
            var (res, err) = clientʗ1.Do(req);
            if (err != default!){
                reqerrcʗ1.ᐸꟷ(err);
            } else {
                (~res).Body.Close();
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // Wait for the first request to receive a response and return the
    // connection to the idle pool.
    var selᴛ89 = reqerrc;
    var selᴛ90 = reqc;
    switch (select(ᐸꟷ(selᴛ89, ꓸꓸꓸ), ᐸꟷ(selᴛ90, ꓸꓸꓸ))) {
    case 0 when selᴛ89.ꟷᐳ(out var err): {
        Ꮡt.Fatalf("request 1: got err %v, want nil"u8, err);
        break;
    }
    case 1 when selᴛ90.ꟷᐳ(out var r1c): {
        builtin.close(r1c);
        break;
    }}
    channel<EmptyStruct> idlec = default!;
    var selᴛ91 = reqerrc;
    var selᴛ92 = putidlec;
    switch (select(ᐸꟷ(selᴛ91, ꓸꓸꓸ), ᐸꟷ(selᴛ92, ꓸꓸꓸ))) {
    case 0 when selᴛ91.ꟷᐳ(out var err): {
        Ꮡt.Fatalf("request 1: got err %v, want nil"u8, err);
        break;
    }
    case 1 when selᴛ92.ꟷᐳ(out idlec): {
        break;
    }}
    Ꮡwg.Add(1);
    var (cancelctx, cancel) = context_package.WithCancel(context_package.Background());
    var cancelctxʗ1 = cancelctx;
    var clientʗ2 = client;
    var idlecʗ1 = idlec;
    var tsʗ2 = ts;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var (req, _) = NewRequestWithContext(cancelctxʗ1, getˢ2, (~tsʗ2).URL, default!);
            var (res, err) = clientʗ2.Do(req);
            if (err == default!) {
                (~res).Body.Close();
            }
            if (!errors.Is(err, context_package.Canceled)) {
                Ꮡt.Errorf("request 2: got err %v, want Canceled"u8, err);
            }
            // Unblock the first request.
            builtin.close(idlecʗ1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // Wait for the second request to arrive at the server, and then cancel
    // the request context.
    var r2c = ᐸꟷ(reqc);
    cancel();
    ᐸꟷ(idlec);
    builtin.close(r2c);
    Ꮡwg.Wait();
}

public static void TestHandlerAbortRacesBodyRead(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHandlerAbortRacesBodyRead(Δp0, Δp1));
}

internal static partial void testHandlerAbortRacesBodyRead(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), io.Discard, (~req).Body);
        throw panic(ErrAbortHandler);
    }))).Value.ts;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 2; i++) {
        Ꮡwg.Add(1);
        var tsʗ1 = ts;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint j = 0; j < 10; j++) {
                    const int64 reqLen = /* 6 * 1024 * 1024 */ 6291456;
                    var (req, _) = NewRequest(postˢ, (~tsʗ1).URL, new io.LimitedReaderжReader(Ꮡ(new io.LimitedReader(R: ((neverEnding)(rune)'x'), N: reqLen))));
                    req.Value.ContentLength = reqLen;
                    var (resp, _) = (~tsʗ1.Client()).Transport.RoundTrip(req);
                    if (resp != nil) {
                        (~resp).Body.Close();
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

public static void TestRequestSanitization(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRequestSanitization(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue60374Testˢ = (@string)"https://go.dev/issue/60374 test fails when run with HTTP/2"u8;
internal static readonly @string xEvilˢ = "X-Evil"u8;
internal static readonly @string goDevXEvilEvilˢ = "go.dev\r\nX-Evil:evil"u8;

internal static void testRequestSanitization(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        // Remove this after updating x/net.
        Ꮡt.Skip(httpsGoDevIssue60374Testˢ);
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ1) => {
        {
            var (h, ok) = (~reqΔ1).Header[xEvilˢ, ꟷ]; if (ok) {
                Ꮡt.Errorf("request has X-Evil header: %q"u8, h);
            }
        }
    }))).Value.ts;
    var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
    req.Value.Host = goDevXEvilEvilˢ;
    var (resp, _) = ts.Client().Do(req);
    if (resp != nil) {
        (~resp).Body.Close();
    }
}

public static void TestProxyAuthHeader(ж<testing.T> Ꮡt) {
    // Not parallel: Sets an environment variable.
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testProxyAuthHeader(Δp0, Δp1), new testMode[]{http1Mode}.slice(), testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpˢ4 = "http://_/"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string usernameᶜ = "u"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string passwordᶜ = "@/?!"u8;

internal static void testProxyAuthHeader(ж<testing.T> Ꮡt, testMode mode) {
    @string username = usernameᶜ;
    @string password = passwordᶜ;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        // Copy the Proxy-Authorization header to a new Request,
        // since Request.BasicAuth only parses the Authorization header.
        Δhttp.Request r2 = default!;
        r2.Header = new httpꓸHeader(new map<@string, slice<@string>>{
            ["Authorization"u8] = (~req).Header[proxyAuthorizationˢ]
        });
        var (gotuser, gotpass, ok) = r2.BasicAuth();
        if (!ok || gotuser != username || gotpass != password) {
            Ꮡt.Errorf("req.BasicAuth() = %q, %q, %v; want %q, %q, true"u8, gotuser, gotpass, ok, username, password);
        }
    })));
    var (u, err) = url.Parse((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    u.Value.User = url.UserPassword(username, password);
    Ꮡt.Setenv(httpProxyˢ, u.String());
    cst.Value.tr.Value.Proxy = ProxyURL(u);
    (var resp, err) = (~cst).c.Get(httpˢ4);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~resp).Body.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11200ContentLength3ˢ = "HTTP/1.1 200\r\nContent-Length: 3\r\n\r\nfoo"u8;

// Issue 61708
public static partial void TestTransportReqCancelerCleanupOnRequestBodyWriteError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ln = newLocalListener(Ꮡt);
        @string addr = ln.Addr().String();
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        var lnʗ1 = ln;
        goǃ(() => {
            var (conn, errΔ1) = lnʗ1.Accept();
            if (errΔ1 != default!) {
                Ꮡt.Errorf("ln.Accept: %v"u8, errΔ1);
                return;
            }
            // Start reading request before sending response to avoid
            // "Unsolicited response received on idle HTTP channel" RoundTrip error.
            {
                var (_, errΔ2) = io.ReadFull(new http_test_package.net_ConnᴠReader(conn), new slice<byte>(1)); if (errΔ2 != default!) {
                    Ꮡt.Errorf("conn.Read: %v"u8, errΔ2);
                    return;
                }
            }
            io.WriteString(new http_test_package.net_ConnᴠWriter(conn), http11200ContentLength3ˢ);
            ᐸꟷ(doneʗ1);
            conn.Close();
        });
        var didRead = new channel<bool>(0);
        var didReadʗ1 = didRead;
        http_internal_test_package.SetReadLoopBeforeNextReadHook(() => {
            didReadʗ1.ᐸꟷ(true);
        });
        defer(http_internal_test_package.SetReadLoopBeforeNextReadHook, (Action)(default!), ref ᒐ);
        var tr = Ꮡ(new Transport(nil));
        // Send a request with a body guaranteed to fail on write.
        var (req, err) = NewRequest(postˢ, "http://"u8 + addr, io.LimitReader(((neverEnding)(rune)'x'), ((int64)1 << (int)(30))));
        if (err != default!) {
            Ꮡt.Fatalf("NewRequest: %v"u8, err);
        }
        (var resp, err) = tr.RoundTrip(req);
        if (err != default!) {
            Ꮡt.Fatalf("tr.RoundTrip: %v"u8, err);
        }
        builtin.close(done);
        // Before closing response body wait for readLoopDone goroutine
        // to complete due to closed connection by writeLoop.
        ᐸꟷ(didRead);
        (~resp).Body.Close();
        // Verify no outstanding requests after readLoop/writeLoop
        // goroutines shut down.
        var trʗ1 = tr;
        waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
            nint n = trʗ1.NumPendingRequestsForTesting();
            if (n > 0) {
                if (d > 0) {
                    Ꮡt.Logf("pending requests = %d after %v (want 0)"u8, n, d);
                }
                return false;
            }
            return true;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestValidateClientRequestTrailers(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testValidateClientRequestTrailers(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedNonNilResponseˢ = (@string)"Unexpected non-nil response"u8;

[GoType("dyn")] internal partial struct testValidateClientRequestTrailers_cases {
    internal httpꓸHeader trailer;
    internal @string wantErr;
}

internal static void testValidateClientRequestTrailers(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        rw.Write(slice<byte>("Hello"u8));
    }))).Value.ts;
    var cases = new testValidateClientRequestTrailers_cases[]{
        new(new httpꓸHeader(new map<@string, slice<@string>>{["Trx"u8] = new @string[]{"x\r\nX-Another-One"u8}.slice()}), @"invalid trailer field value for ""Trx"""u8),
        new(new httpꓸHeader(new map<@string, slice<@string>>{["\r\nTrx"u8] = new @string[]{"X-Another-One"u8}.slice()}), @"invalid trailer field name ""\r\nTrx"""u8)
    }.slice();
    foreach (var (i, vᴛ1) in cases) {
        ref var tt = ref heap(new testValidateClientRequestTrailers_cases(), out var Ꮡtt);
        tt = vᴛ1;

        @string testName = fmt.Sprintf("%s%d"u8, mode, i);
        var cstʗ1 = cst;
        var ttʗ1 = tt;
        Ꮡt.Run(testName, (ж<testing.T> tΔ1) => {
            var (req, err) = NewRequest(getˢ2, (~cstʗ1).URL, default!);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            req.Value.Trailer = ttʗ1.trailer;
            (var res, err) = cstʗ1.Client().Do(req);
            if (err == default!) {
                tΔ1.Fatal(expectedAnErrorˢ2);
            }
            {
                @string g = err.Error();
                @string w = ttʗ1.wantErr; if (!strings.Contains(g, w)) {
                    tΔ1.Fatalf("Mismatched error\n\t%q\ndoes not contain\n\t%q"u8, g, w);
                }
            }
            if (res != nil) {
                tΔ1.Fatal(unexpectedNonNilResponseˢ);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http2client0ˢ = "http2client=0"u8;
internal static readonly @string http2server0ˢ = "http2server=0"u8;
internal static readonly @string xProtoˢ = "X-Proto"u8;

[GoType("dyn")] internal partial struct TestTransportServerProtocols_type {
    internal @string name;
    internal @string scheme;
    internal Action<ж<testing.T>> setup;
    internal Action<ж<Δhttp.Transport>> transport;
    internal Action<ж<Δhttp.Server>> server;
    internal @string want;
}

public static void TestTransportServerProtocols(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
    DefaultTransport._<ж<Δhttp.Transport>>().CloseIdleConnections();
    ref var cert = ref heap<tls.Certificate>(out var Ꮡcert);
    (cert, var err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var leafCert, err) = x509.ParseCertificate(cert.ΔCertificate[0]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var certpool = x509.NewCertPool();
    certpool.AddCert(leafCert);
    foreach (var (_, vᴛ1) in new TestTransportServerProtocols_type[]{new(
        name: "http default"u8,
        scheme: "http"u8,
        want: "HTTP/1.1"u8
    ), new(
        name: "https default"u8,
        scheme: "https"u8,
        transport: (ж<Δhttp.Transport> tr) => {
        }, // Transport default is HTTP/1.

        want: "HTTP/1.1"u8
    ), new(
        name: "https transport protocols include HTTP2"u8,
        scheme: "https"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            // Server default is to support HTTP/2, so if the Transport enables
            // HTTP/2 we get it.
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
            (~tr).Protocols.SetHTTP2(true);
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "https transport protocols only include HTTP1"u8,
        scheme: "https"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            // Explicitly enable only HTTP/1.
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
        },
        want: "HTTP/1.1"u8
    ), new(
        name: "https transport ForceAttemptHTTP2"u8,
        scheme: "https"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            // Pre-Protocols-field way of enabling HTTP/2.
            tr.Value.ForceAttemptHTTP2 = true;
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "https transport protocols override TLSNextProto"u8,
        scheme: "https"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            // Setting TLSNextProto to an empty map is the historical way
            // of disabling HTTP/2. Explicitly enabling HTTP2 in the Protocols
            // field takes precedence.
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
            (~tr).Protocols.SetHTTP2(true);
            tr.Value.TLSNextProto = new map<@string, Func<@string, ж<tls.Conn>, Δhttp.RoundTripper>>{};
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "https server disables HTTP2 with TLSNextProto"u8,
        scheme: "https"u8,
        server: (ж<Δhttp.Server> srv) => {
            // Disable HTTP/2 on the server with TLSNextProto,
            // use default Protocols value.
            srv.Value.TLSNextProto = new map<@string, Action<ж<Δhttp.Server>, ж<tls.Conn>, httpꓸHandler>>{};
        },
        want: "HTTP/1.1"u8
    ), new(
        name: "https server Protocols overrides empty TLSNextProto"u8,
        scheme: "https"u8,
        server: (ж<Δhttp.Server> srv) => {
            // Explicitly enabling HTTP2 in the Protocols field takes precedence
            // over setting an empty TLSNextProto.
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetHTTP1(true);
            (~srv).Protocols.SetHTTP2(true);
            srv.Value.TLSNextProto = new map<@string, Action<ж<Δhttp.Server>, ж<tls.Conn>, httpꓸHandler>>{};
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "https server protocols only include HTTP1"u8,
        scheme: "https"u8,
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetHTTP1(true);
        },
        want: "HTTP/1.1"u8
    ), new(
        name: "https server protocols include HTTP2"u8,
        scheme: "https"u8,
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetHTTP1(true);
            (~srv).Protocols.SetHTTP2(true);
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "GODEBUG disables HTTP2 client"u8,
        scheme: "https"u8,
        setup: (ж<testing.T> tΔ1) => {
            tΔ1.Setenv(godebugˢ, http2client0ˢ);
        },
        transport: (ж<Δhttp.Transport> tr) => {
            // Server default is to support HTTP/2, so if the Transport enables
            // HTTP/2 we get it.
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
            (~tr).Protocols.SetHTTP2(true);
        },
        want: "HTTP/1.1"u8
    ), new(
        name: "GODEBUG disables HTTP2 server"u8,
        scheme: "https"u8,
        setup: (ж<testing.T> tΔ2) => {
            tΔ2.Setenv(godebugˢ, http2server0ˢ);
        },
        transport: (ж<Δhttp.Transport> tr) => {
            // Server default is to support HTTP/2, so if the Transport enables
            // HTTP/2 we get it.
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
            (~tr).Protocols.SetHTTP2(true);
        },
        want: "HTTP/1.1"u8
    ), new(
        name: "unencrypted HTTP2 with prior knowledge"u8,
        scheme: "http"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetUnencryptedHTTP2(true);
        },
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetHTTP1(true);
            (~srv).Protocols.SetUnencryptedHTTP2(true);
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "unencrypted HTTP2 only on server"u8,
        scheme: "http"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetUnencryptedHTTP2(true);
        },
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetUnencryptedHTTP2(true);
        },
        want: "HTTP/2.0"u8
    ), new(
        name: "unencrypted HTTP2 with no server support"u8,
        scheme: "http"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetUnencryptedHTTP2(true);
        },
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetHTTP1(true);
        },
        want: "error"u8
    ), new(
        name: "HTTP1 with no server support"u8,
        scheme: "http"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
        },
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetUnencryptedHTTP2(true);
        },
        want: "error"u8
    ), new(
        name: "HTTPS1 with no server support"u8,
        scheme: "https"u8,
        transport: (ж<Δhttp.Transport> tr) => {
            tr.Value.Protocols = Ꮡ(new Protocols(nil));
            (~tr).Protocols.SetHTTP1(true);
        },
        server: (ж<Δhttp.Server> srv) => {
            srv.Value.Protocols = Ꮡ(new Protocols(nil));
            (~srv).Protocols.SetHTTP2(true);
        },
        want: "error"u8
    )
    }.slice()) {
        ref var test = ref heap(new TestTransportServerProtocols_type(), out var Ꮡtest);
        test = vᴛ1;

        var certʗ1 = cert;
        var certpoolʗ1 = certpool;
        var testʗ1 = test;
        Ꮡt.Run(test.name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
            // We don't use httptest here because it makes its own decisions
            // about how to enable/disable HTTP/2.
            var srv = Ꮡ(new Server(
                TLSConfig: Ꮡ(new tls.Config(
                    Certificates: new tls.Certificate[]{certʗ1}.slice()
                )),
                Handler: new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
                    w.Header().Set(xProtoˢ, (~req).Proto);
                }))
            ));
            var tr = Ꮡ(new Transport(
                TLSClientConfig: Ꮡ(new tls.Config(
                    RootCAs: certpoolʗ1
                ))
            ));
            if (testʗ1.setup != default!) {
                testʗ1.setup(tΔ3);
            }
            if (testʗ1.server != default!) {
                testʗ1.server(srv);
            }
            if (testʗ1.transport != default!){
                testʗ1.transport(tr);
            } else {
                tr.Value.Protocols = Ꮡ(new Protocols(nil));
                (~tr).Protocols.SetHTTP1(true);
                (~tr).Protocols.SetHTTP2(true);
            }
            var listener = newLocalListener(tΔ3);
            var srvc = new channel<error>(1);
            var listenerʗ1 = listener;
            var srvʗ1 = srv;
            var srvcʗ1 = srvc;
            var testʗ2 = testʗ1;
            goǃ(() => {
                var exprᴛ1 = testʗ2.scheme;
                if (exprᴛ1 == "http"u8) {
                    srvcʗ1.ᐸꟷ(srvʗ1.Serve(listenerʗ1));
                }
                else if (exprᴛ1 == "https"u8) {
                    srvcʗ1.ᐸꟷ(srvʗ1.ServeTLS(listenerʗ1, ""u8, ""u8));
                }

            });
            var srvʗ2 = srv;
            var srvcʗ2 = srvc;
            tΔ3.Cleanup(() => {
                srvʗ2.Close();
                ᐸꟷ(srvcʗ2);
            });
            var client = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
            var (resp, errΔ1) = client.Get(testʗ1.scheme + "://"u8 + listener.Addr().String());
            if (errΔ1 != default!) {
                if (testʗ1.want == "error"u8) {
                    return;
                }
                tΔ3.Fatal(errΔ1);
            }
            {
                @string got = (~resp).Header.Get(xProtoˢ); if (got != testʗ1.want) {
                    tΔ3.Fatalf("request proto %q, want %q"u8, got, testʗ1.want);
                }
            }
        });
    }
}

public static void TestIssue61474(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIssue61474(Δp0, Δp1), new testMode[]{http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testIsTooLargeˢ = (@string)"test is too large"u8;

internal static partial void testIssue61474(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (testing.Short()) {
            return;
        }
        // This test reliably exercises the condition causing #61474,
        // but requires many iterations to do so.
        // Keep the test around for now, but don't run it by default.
        Ꮡt.Skip(testIsTooLargeˢ);
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        })), (ж<Δhttp.Transport> tr) => {
            tr.Value.MaxConnsPerHost = 1;
        });
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        foreach (var _ᴛ1 in range(100000)) {
            Ꮡwg.Add(1);
            var cstʗ1 = cst;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    var (ctx, cancel) = context_package.WithTimeout(Ꮡt.Context(), 1 * time.Millisecond);
                    var cancelʗ1 = cancel;
                    defer(() => cancelʗ1(), ref ᒐ);
                    var (req, _) = NewRequestWithContext(ctx, getˢ2, (~(~cstʗ1).ts).URL, default!);
                    var (resp, err) = (~cstʗ1).c.Do(req);
                    if (err == default!) {
                        (~resp).Body.Close();
                    }
                    Ꮡwg.Done();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
