// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using context = context_package;
using tls = crypto.tls_package;
using errors = errors_package;
using io = io_package;
using net = net_package;
using Δhttp = global::go.net.http_package;
using httptrace = global::go.net.http.httptrace_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using synctest = global::go.testing.synctest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using global::go.net;
using global::go.net.http;
using global::go.testing;
using static global::go.net.http_internal_test_package;
using time = time_package;
using ꓸꓸꓸany = Span<any>;

partial class http_test_package {

// Successive requests use the same HTTP/1 connection.
public static void TestTransportPoolConnReusePriorConnection(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode);

        // First request creates a new connection.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http11ˢ);
        rt1.finish();

        // Second request reuses the first connection.
        var rt2 = dt.roundTrip();
        rt2.wantDone(c1, http11ˢ);
        rt2.finish();
    });
}

// Two HTTP/1 requests made at the same time use different connections.
public static void TestTransportPoolConnCannotReuseConnectionInUse(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode);

        // First request creates a new connection.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http11ˢ);

        // Second request is made while the first request is still using its connection,
        // so it goes on a new connection.
        var rt2 = dt.roundTrip();
        var c2 = dt.wantDial();
        c2.finish(default!);
        rt2.wantDone(c2, http11ˢ);
    });
}

// When an HTTP/2 connection is at its stream limit
// a new request is made on a new connection.
internal static void testTransportPoolConnHTTP2NoStrictMaxConcurrentRequests(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http2Mode, (ж<Δhttp.Server> srv) => {
            srv.Value.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
                MaxConcurrentStreams: 2
            ));
        });

        // First request dials an HTTP/2 connection.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http20ˢ);

        // Second request uses the existing connection.
        var rt2 = dt.roundTrip();
        rt2.wantDone(c1, http20ˢ);

        // Third request creates a new connection
        var rt3 = dt.roundTrip();
        var c2 = dt.wantDial();
        c2.finish(default!);
        rt3.wantDone(c2, http20ˢ);

        rt1.finish();
        rt2.finish();
        rt3.finish();

        // With slots available on both connections, we prefer the oldest.
        var rt4 = dt.roundTrip();
        rt4.wantDone(c1, http20ˢ);
        var rt5 = dt.roundTrip();
        rt5.wantDone(c1, http20ˢ);
        var rt6 = dt.roundTrip();
        rt6.wantDone(c2, http20ˢ);
        rt4.finish();
        rt5.finish();
        rt6.finish();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippedUntilH2BundleGoˢ = (@string)"skipped until h2_bundle.go includes support for StrictMaxConcurrentRequests"u8;

// When an HTTP/2 connection is at its stream limit
// and StrictMaxConcurrentRequests = true,
// a new request waits for a slot on the existing connection.
public static void TestTransportPoolConnHTTP2StrictMaxConcurrentRequests(ж<testing.T> Ꮡt) {
    Ꮡt.Skip(skippedUntilH2BundleGoˢ);

    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http2Mode, (ж<Δhttp.Server> srv) => {
            srv.Value.HTTP2.Value.MaxConcurrentStreams = 2;
        }, (ж<Δhttp.Transport> tr) => {
            tr.Value.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
                StrictMaxConcurrentRequests: true
            ));
        });

        // First request dials an HTTP/2 connection.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http20ˢ);

        // Second request uses the existing connection.
        var rt2 = dt.roundTrip();
        rt2.wantDone(c1, http20ˢ);

        // Third request blocks waiting for a slot on the existing connection.
        var rt3 = dt.roundTrip();

        // First request finishing unblocks the thirrd.
        rt1.finish();
        rt3.wantDone(c1, http20ˢ);

        rt2.finish();
        rt3.finish();
    });
}

// A new request made while an HTTP/2 dial is in progress will start a second dial.
public static void TestTransportPoolConnHTTP2Startup(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http2Mode, (ж<Δhttp.Server> srv) => {
        });

        // Two requests start.
        // Since the second request starts before the first dial finishes, it starts a second dial.
        var rt1 = dt.roundTrip();
        var rt2 = dt.roundTrip();
        var c1 = dt.wantDial();
        var c2 = dt.wantDial();

        // Both requests use the conn of the first dial to complete.
        c1.finish(default!);
        rt1.wantDone(c1, http20ˢ);
        rt2.wantDone(c1, http20ˢ);

        rt1.finish();
        rt2.finish();
        c2.finish(default!);
    });
}

// When a request finishes using an HTTP/1 connection,
// a pending request attempting to dial a new connection will use the newly-available one.
public static void TestTransportPoolConnConnectionBecomesAvailableDuringDial(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode);

        // First request creates a new connection.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http11ˢ);

        // Second request is made while the first request is still using its connection.
        // The first connection completes while the second Dial is in progress, so the
        // second request uses the first connection.
        var rt2 = dt.roundTrip();
        var c2 = dt.wantDial();
        rt1.finish();
        rt2.wantDone(c1, http11ˢ);

        // This section is a bit overfitted to the current Transport implementation:
        // A third request starts. We have an in-progress dial that was started by rt2,
        // but this new request (rt3) is going to ignore it and make a dial of its own.
        // rt3 will use the first of these dials that completes.
        var rt3 = dt.roundTrip();
        var c3 = dt.wantDial();
        c2.finish(default!);
        rt3.wantDone(c2, http11ˢ);

        c3.finish(default!);
    });
}

// Connections are not reused when DisableKeepAlives = true.
public static void TestTransportPoolDisableKeepAlives(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode, (ж<Δhttp.Transport> tr) => {
            tr.Value.DisableKeepAlives = true;
        });

        // Two requests, each uses a separate connection.
        foreach (var _ᴛ1 in range(2)) {
            var rt = dt.roundTrip();
            var c = dt.wantDial();
            c.finish(default!);
            rt.wantDone(c, http11ˢ);
            rt.finish();
        }
    });
}

// Canceling a request before its connection is created returns the conn to the pool.
public static void TestTransportPoolCancelRequestReusesConn(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode);

        // First request is canceled before its connection is created.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        (~rt1).cancel();
        rt1.wantError();

        // Second request uses the first connection.
        var rt2 = dt.roundTrip();
        var c2 = dt.wantDial();
        c1.finish(default!); // first dial finishes
        rt2.wantDone(c1, http11ˢ);
        rt2.finish();

        c2.finish(default!); // second dial finishes
    });
}

// Connections are not reused when DisableKeepAlives = true.
public static void TestTransportPoolCancelRequestWithDisableKeepAlives(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode, (ж<Δhttp.Transport> tr) => {
            tr.Value.DisableKeepAlives = true;
        });

        // First request is canceled before its connection is created.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        (~rt1).cancel();
        rt1.wantError();

        // Dial finishes. DisableKeepAlives = true, so we discard the connection.
        c1.finish(default!);

        // Second request is made on a new connection.
        var rt2 = dt.roundTrip();
        var c2 = dt.wantDial();
        c2.finish(default!);
        rt2.wantDone(c2, http11ˢ);
        rt2.finish();
    });
}

// Connections are not reused after an error.
public static void TestTransportPoolConnectionBroken(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode);

        // First request creates a new connection.
        // The connection breaks while sending the response.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http11ˢ);
        (~c1).fakeNetConn.Close(); // break the connection
        rt1.finish();

        // Second request is made on a new connection, since the first is broken.
        var rt2 = dt.roundTrip();
        var c2 = dt.wantDial();
        c2.finish(default!);
        rt2.wantDone(c2, http11ˢ);
        (~c2).fakeNetConn.Close();
        rt2.finish();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object host1FakeTldˢ = (@string)"host1.fake.tld"u8;
internal static readonly object host2FakeTldˢ = (@string)"host2.fake.tld"u8;

// MaxIdleConnsPerHost limits the number of idle connections.
public static void TestTransportPoolClosesConnsPastMaxIdleConnsPerHost(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        var dt = newTransportDialTester(tΔ1, http1Mode, (ж<Δhttp.Transport> tr) => {
            tr.Value.MaxIdleConnsPerHost = 1;
        });

        // First request creates a new connection.
        var rt1 = dt.roundTrip(host1FakeTldˢ);
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http11ˢ);

        // Second request also creates a new connection.
        var rt2 = dt.roundTrip(host1FakeTldˢ);
        var c2 = dt.wantDial();
        c2.finish(default!);
        rt2.wantDone(c2, http11ˢ);

        // Third request is to a different host.
        var rt3 = dt.roundTrip(host2FakeTldˢ);
        var c3 = dt.wantDial();
        c3.finish(default!);
        rt3.wantDone(c3, http11ˢ);

        // All requests finish. One conn is in excess of MaxIdleConnsPerHost, and is closed.
        rt3.finish();
        rt2.finish();
        rt1.finish();
        c1.wantClosed();

        // Additional requests reuse the remaining connections.
        var rt4 = dt.roundTrip(host1FakeTldˢ);
        rt4.wantDone(c2, http11ˢ);
        rt4.finish();
        var rt5 = dt.roundTrip(host2FakeTldˢ);
        rt5.wantDone(c3, http11ˢ);
        rt5.finish();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippedUntilH2BundleGoˢ2 = (@string)"skipped until h2_bundle.go includes support for MaxConcurrentStreams"u8;

// Current (but probably wrong) behavior:
// MaxIdleConnsPerHost doesn't apply to HTTP/2 connections.
public static void TestTransportPoolMaxIdleConnsPerHostHTTP2(ж<testing.T> Ꮡt) {
    synctest.Test(Ꮡt, (ж<testing.T> tΔ1) => {
        tΔ1.Skip(skippedUntilH2BundleGoˢ2);

        var dt = newTransportDialTester(tΔ1, http2Mode, (ж<Δhttp.Server> srv) => {
            srv.Value.HTTP2 = Ꮡ(new Δhttp.HTTP2Config(
                MaxConcurrentStreams: 1
            ));
        }, (ж<Δhttp.Transport> tr) => {
            tr.Value.MaxIdleConnsPerHost = 1;
        });

        // First request creates a new connection.
        var rt1 = dt.roundTrip();
        var c1 = dt.wantDial();
        c1.finish(default!);
        rt1.wantDone(c1, http20ˢ);

        // Second request also creates a new connection.
        var rt2 = dt.roundTrip();
        var c2 = dt.wantDial();
        c2.finish(default!);
        rt2.wantDone(c2, http20ˢ);

        // Both requests finish.
        // We have two idle conns for this host, but we keep them both.
        rt1.finish();
        rt2.finish();

        // Two new requests use the existing connections.
        var rt3 = dt.roundTrip();
        rt3.wantDone(c1, http20ˢ);
        var rt4 = dt.roundTrip();
        rt4.wantDone(c2, http20ˢ);
    });
}

// A transportDialTester manages a test of a connection's Dials.
partial struct transportDialTester {
    internal ж<testing.T> t;
    internal ж<clientServerTest> cst;
    internal sync.Mutex dialsMu;
    internal slice<ж<transportDialTesterConn>> dials;
    internal nint roundTripCount;
    internal nint dialCount;
}

// A transportDialTesterRoundTrip is a RoundTrip made as part of a dial test.
partial struct transportDialTesterRoundTrip {
    internal ж<testing.T> t;
    internal nint roundTripID;               // distinguishes RoundTrips in logs
    internal Action cancel; // cancels the Request context
    internal io.WriteCloser reqBody;     // write half of the Request.Body
    internal bool respBodyClosed;               // set when the user calls Response.Body.Close
    internal bool returned;               // set when RoundTrip returns
    internal ж<Δhttp.Response> res;
    internal error err;
    internal ж<transportDialTesterConn> conn;
}

// A transportDialTesterConn is a client connection created by the Transport as
// part of a dial test.
partial struct transportDialTesterConn {
    internal ж<testing.T> t;
    internal nint connID;       // distinguished Dials in logs
    internal channel<error> ready; // sent on to complete the Dial
    internal slice<@string> protos;
    internal channel<EmptyStruct> closed;
    /*embed*/ internal ж<fakeNetConn> fakeNetConn;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testFinishedˢ = "test finished"u8;

internal static ж<transportDialTester> newTransportDialTester(ж<testing.T> Ꮡt, testMode mode, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.sslice();

    Ꮡt.Helper();
    var dt = Ꮡ(new transportDialTester(
        t: Ꮡt
    ));
    var dtʗ1 = dt;
    (ж<transportDialTesterConn>, error) dialContext(context.Context _Δp0, @string network, @string address) {
        var c = Ꮡ(new transportDialTesterConn(
            t: Ꮡt,
            ready: new channel<error>(0),
            closed: new channel<EmptyStruct>(0)
        ));
        // Notify the test that a Dial has started,
        // and wait for the test to notify us that it should complete.
        dtʗ1.of(transportDialTester.ᏑdialsMu).Lock();
        dtʗ1.Value.dials = append((~dtʗ1).dials, c);
        dtʗ1.of(transportDialTester.ᏑdialsMu).Unlock();

        var selᴛ62 = (~c).ready;
        var selᴛ63 = Ꮡt.Context().Done();
        switch (select(ᐸꟷ(selᴛ62, ꓸꓸꓸ), ᐸꟷ(selᴛ63, ꓸꓸꓸ))) {
        case 0 when selᴛ62.ꟷᐳ(out var err): {
            if (err != default!) {
                return (default!, err);
            }
            break;
        }
        case 1 when selᴛ63.ꟷᐳ(out _): {
            Ꮡt.Errorf("test finished with dial in progress"u8);
            return (default!, errors.New(testFinishedˢ));
        }}

        c.Value.fakeNetConn = (~(~dtʗ1).cst).li.connect();
        var cʗ1 = c;
        Ꮡt.Cleanup(() => {
            (~cʗ1).fakeNetConn.Close();
        });
        // Use the *transportDialTesterConn as the net.Conn,
        // to let tests associate requests with connections.
        return (c, default!);
    }

    var dialContextʗ1 = dialContext;
    var dtʗ2 = dt;
    dt.Value.cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        // Write response headers when we receive a request.
        Δhttp.NewResponseController(w).EnableFullDuplex();
        w.WriteHeader(200);
        Δhttp.NewResponseController(w).Flush();
        // Wait for the client to send the request body,
        // to synchronize with the rest of the test.
        io.ReadAll((~r).Body);
    })), appendꓸꓸꓸ(new any[]{optFakeNet.OrTypedNil(), (ж<Δhttp.Transport> tr) => {
        var dialContextʗ2 = dialContextʗ1;
        var dtʗ3 = dtʗ2;
        tr.Value.DialContext = (net.Conn, error) (context.Context ctx, @string network, @string addr) => {
            var (ᴛ1, ᴛ2) = dialContextʗ2(ctx, network, (~(~(~dtʗ3).cst).ts).Listener.Addr().String());
            return (new http_test_package.transportDialTesterConnжConn(ᴛ1), ᴛ2);
        };
        var dialContextʗ3 = dialContextʗ1;
        var dtʗ4 = dtʗ2;
        tr.Value.DialTLSContext = (net.Conn, error) (context.Context ctx, @string network, @string addr) => {
            var (conn, err) = dialContextʗ3(ctx, network, (~(~(~dtʗ4).cst).ts).Listener.Addr().String());
            if (err != default!) {
                return (default!, err);
            }
            var config = Ꮡ(new tls.Config(
                InsecureSkipVerify: true,
                NextProtos: new @string[]{"h2"u8, "http/1.1"u8}.slice()
            ));
            if ((~conn).protos != default!) {
                config.Value.NextProtos = conn.Value.protos;
            }
            var tc = tls.Client(new http_test_package.transportDialTesterConnжConn(conn), config);
            {
                var errΔ1 = tc.Handshake(); if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }
            }
            return (new tls.ConnжConn(tc), default!);
        };
    }
    }.slice(), opts).ꓸꓸꓸ);
    return dt;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakeTldˢ = "fake.tld"u8;

// roundTrip starts a RoundTrip.
// It returns immediately, without waiting for the RoundTrip call to complete.
internal static partial ж<transportDialTesterRoundTrip> roundTrip(this ж<transportDialTester> Ꮡdt, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.sslice();
    ref var dt = ref Ꮡdt.DerefOrNull();

    dt.t.Helper();
    @string host = fakeTldˢ;
    foreach (var (_, o) in opts) {
        switch (o.type()) {
        case @string oΔ1: {
            host = oΔ1;
            break;
        }
        default: {
            var oΔ1 = o;
            dt.t.Fatalf("unknown option type %T"u8, oΔ1);
            break;
        }}
    }
    ref var ctx = ref heap<context.Context>(out var Ꮡctx);
    (ctx, var cancel) = context_package.WithCancel(context_package.Background());
    var (pr, pw) = io.Pipe();
    dt.roundTripCount++;
    var rt = Ꮡ(new transportDialTesterRoundTrip(
        t: dt.t,
        roundTripID: dt.roundTripCount,
        reqBody: new io.PipeWriterжWriteCloser(pw),
        cancel: cancel
    ));
    dt.t.Logf("RoundTrip %v: started"u8, (~rt).roundTripID);
    var rtʗ1 = rt;
    dt.t.Cleanup(() => {
        (~rtʗ1).cancel();
        rtʗ1.finish();
    });
    var prʗ1 = pr;
    var rtʗ2 = rt;
    goǃ(() => {
            var rtʗ3 = rtʗ2;
        Ꮡctx.ValueSlot = httptrace.WithClientTrace(Ꮡctx.ValueSlot, Ꮡ(new httptrace.ClientTrace(
            GotConn: (httptrace.GotConnInfo info) => {
                var c = info.Conn;
                {
                    var (tlsConn, ok) = c._<ж<tls.Conn>>(ᐧ); if (ok) {
                        c = tlsConn.NetConn();
                    }
                }
                rtʗ3.Value.conn = c._<ж<transportDialTesterConn>>();
            }
        )));
        var (proto, _, _) = strings.Cut((~(~Ꮡdt.Value.cst).ts).URL, ":"u8);
        var (req, _) = Δhttp.NewRequestWithContext(Ꮡctx.ValueSlot, postˢ, proto + "://"u8 + host, new io.PipeReaderжReader(prʗ1));
        (~req).Header.Set(contentTypeˢ, textPlainˢ);
        (rtʗ2.Value.res, rtʗ2.Value.err) = (~Ꮡdt.Value.cst).tr.RoundTrip(req);
        Ꮡdt.Value.t.Logf("RoundTrip %v: done (err:%v)"u8, (~rtʗ2).roundTripID, (~rtʗ2).err);
        rtʗ2.Value.returned = true;
    });
    return rt;
}

// wantDone indicates that a RoundTrip should have returned.
internal static void wantDone(this ref transportDialTesterRoundTrip rt, ж<transportDialTesterConn> Ꮡc, @string wantProto) {
    ref var c = ref Ꮡc.DerefOrNull();

    rt.t.Helper();
    synctest.Wait();
    if (!rt.returned) {
        rt.t.Fatalf("RoundTrip %v: still running, want to have returned"u8, rt.roundTripID);
    }
    if (rt.err != default!) {
        rt.t.Fatalf("RoundTrip %v: want success, got err %v"u8, rt.roundTripID, rt.err);
    }
    if (rt.conn != Ꮡc) {
        rt.t.Fatalf("RoundTrip %v: want on conn %v, got conn %v"u8, rt.roundTripID, c.connID, (~rt.conn).connID);
    }
    {
        var (got, want) = (rt.conn, Ꮡc); if (got != want) {
            rt.t.Fatalf("RoundTrip %v: sent on conn %v, want conn %v"u8, rt.roundTripID, (~got).connID, (~want).connID);
        }
    }
    {
        @string got = rt.res.Value.Proto;
        @string want = wantProto; if (got != want) {
            rt.t.Fatalf("RoundTrip %v: got protocol %q, want %q"u8, rt.roundTripID, got, want);
        }
    }
}

// wantError indicates that a RoundTrip should have returned with an error.
internal static void wantError(this ref transportDialTesterRoundTrip rt) {
    rt.t.Helper();
    synctest.Wait();
    if (!rt.returned) {
        rt.t.Fatalf("RoundTrip %v: still running, want to have returned"u8, rt.roundTripID);
    }
    if (rt.err == default!) {
        rt.t.Fatalf("RoundTrip %v: success, want error"u8, rt.roundTripID);
    }
}

// finish completes a RoundTrip by sending the request body, consuming the response body,
// and closing the response body.
internal static void finish(this ref transportDialTesterRoundTrip rt) {
    rt.t.Helper();

    synctest.Wait();
    if (!rt.returned) {
        rt.t.Fatalf("RoundTrip %v: still running, want to have returned"u8, rt.roundTripID);
    }
    if (rt.err != default!) {
        return;
    }

    if (rt.respBodyClosed) {
        return;
    }
    rt.respBodyClosed = true;
    rt.reqBody.Close();
    io.ReadAll((~rt.res).Body);
    (~rt.res).Body.Close();
    rt.t.Logf("RoundTrip %v: closed request body"u8, rt.roundTripID);
}

// wantDial waits for the Transport to start a Dial.
internal static ж<transportDialTesterConn> wantDial(this ж<transportDialTester> Ꮡdt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var dt = ref Ꮡdt.DerefOrNull();

        dt.t.Helper();
        synctest.Wait();
        dt.dialsMu.Lock();
        ᒐd1 = true;
        if (len(dt.dials) == 0) {
            dt.t.Fatalf("no dial started, want one"u8);
        }
        var c = dt.dials[0];
        dt.dials = dt.dials[1..];
        dt.dialCount++;
        c.Value.connID = dt.dialCount;
        dt.t.Logf("Dial %v: started"u8, (~c).connID);
        return c;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡdt.DerefOrNull().dialsMu.Unlock(); ᒐ.Run(); }
}

// finish completes a Dial.
internal static void finish(this ref transportDialTesterConn c, error err) {
    c.t.Helper();
    c.t.Logf("Dial %v: finished (err:%v)"u8, c.connID, err);
    c.ready.ᐸꟷ(err);
    builtin.close(c.ready);
}

internal static partial void wantClosed(this ref transportDialTesterConn c) {
    c.t.Helper();
    ᐸꟷ(c.closed);
}

internal static error Close(this ref transportDialTesterConn c) {
    var selᴛ64 = c.closed;
    switch (trySelect(ᐸꟷ(selᴛ64, ꓸꓸꓸ))) {
    case 0 when selᴛ64.ꟷᐳ(out _): {
        break;
    }
    default: {
        c.t.Logf("Conn %v: closed"u8, c.connID);
        builtin.close(c.closed);
        break;
    }}
    return default!;
}

} // end http_test_package
