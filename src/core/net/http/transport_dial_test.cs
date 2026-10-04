// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using context = context_package;
using io = io_package;
using net = net_package;
using Δhttp = global::go.net.http_package;
using httptrace = global::go.net.http.httptrace_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.net;
using global::go.net.http;
using static global::go.net.http_internal_test_package;
using time = time_package;

partial class http_test_package {

public static void TestTransportPoolConnReusePriorConnection(ж<testing.T> Ꮡt) {
    var dt = newTransportDialTester(Ꮡt, http1Mode);
    // First request creates a new connection.
    var rt1 = dt.roundTrip();
    var c1 = dt.wantDial();
    c1.finish(default!);
    rt1.wantDone(c1);
    rt1.finish();
    // Second request reuses the first connection.
    var rt2 = dt.roundTrip();
    rt2.wantDone(c1);
    rt2.finish();
}

public static void TestTransportPoolConnCannotReuseConnectionInUse(ж<testing.T> Ꮡt) {
    var dt = newTransportDialTester(Ꮡt, http1Mode);
    // First request creates a new connection.
    var rt1 = dt.roundTrip();
    var c1 = dt.wantDial();
    c1.finish(default!);
    rt1.wantDone(c1);
    // Second request is made while the first request is still using its connection,
    // so it goes on a new connection.
    var rt2 = dt.roundTrip();
    var c2 = dt.wantDial();
    c2.finish(default!);
    rt2.wantDone(c2);
}

public static void TestTransportPoolConnConnectionBecomesAvailableDuringDial(ж<testing.T> Ꮡt) {
    var dt = newTransportDialTester(Ꮡt, http1Mode);
    // First request creates a new connection.
    var rt1 = dt.roundTrip();
    var c1 = dt.wantDial();
    c1.finish(default!);
    rt1.wantDone(c1);
    // Second request is made while the first request is still using its connection.
    // The first connection completes while the second Dial is in progress, so the
    // second request uses the first connection.
    var rt2 = dt.roundTrip();
    var c2 = dt.wantDial();
    rt1.finish();
    rt2.wantDone(c1);
    // This section is a bit overfitted to the current Transport implementation:
    // A third request starts. We have an in-progress dial that was started by rt2,
    // but this new request (rt3) is going to ignore it and make a dial of its own.
    // rt3 will use the first of these dials that completes.
    var rt3 = dt.roundTrip();
    var c3 = dt.wantDial();
    c2.finish(default!);
    rt3.wantDone(c2);
    c3.finish(default!);
}

// A transportDialTester manages a test of a connection's Dials.
[GoType] partial struct transportDialTester {
    internal ж<testing.T> t;
    internal ж<clientServerTest> cst;
    internal channel<ж<transportDialTesterConn>> dials; // each new conn is sent to this channel
    internal nint roundTripCount;
    internal nint dialCount;
}

// A transportDialTesterRoundTrip is a RoundTrip made as part of a dial test.
[GoType] partial struct transportDialTesterRoundTrip {
    internal ж<testing.T> t;
    internal nint roundTripID;               // distinguishes RoundTrips in logs
    internal Action cancel; // cancels the Request context
    internal io.WriteCloser reqBody;     // write half of the Request.Body
    internal bool finished;
    internal channel<EmptyStruct> done; // closed when RoundTrip returns:w
    internal ж<Δhttp.Response> res;
    internal error err;
    internal ж<transportDialTesterConn> conn;
}

// A transportDialTesterConn is a client connection created by the Transport as
// part of a dial test.
[GoType] partial struct transportDialTesterConn {
    internal ж<testing.T> t;
    internal nint connID;       // distinguished Dials in logs
    internal channel<error> ready; // sent on to complete the Dial
    [GoEmbedded] public net_package.Conn Conn;
}

internal static ж<transportDialTester> newTransportDialTester(ж<testing.T> Ꮡt, testMode mode) {
    Ꮡt.Helper();
    var dt = Ꮡ(new transportDialTester(
        t: Ꮡt,
        dials: new channel<ж<transportDialTesterConn>>(0)
    ));
    var dtʗ1 = dt;
    dt.Value.cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        // Write response headers when we receive a request.
        Δhttp.NewResponseController(w).EnableFullDuplex();
        w.WriteHeader(200);
        Δhttp.NewResponseController(w).Flush();
        // Wait for the client to send the request body,
        // to synchronize with the rest of the test.
        io.ReadAll((~r).Body);
    })), (ж<Δhttp.Transport> tr) => {
        var dtʗ2 = dtʗ1;
        tr.Value.DialContext = (context.Context ctx, @string network, @string address) => {
            var c = Ꮡ(new transportDialTesterConn(
                t: Ꮡt,
                ready: new channel<error>(0)
            ));
            // Notify the test that a Dial has started,
            // and wait for the test to notify us that it should complete.
            (~dtʗ2).dials.ᐸꟷ(c);
            {
                var errΔ1 = ᐸꟷ((~c).ready); if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }
            }
            var (nc, err) = net.Dial(network, address);
            if (err != default!) {
                return (default!, err);
            }
            // Use the *transportDialTesterConn as the net.Conn,
            // to let tests associate requests with connections.
            c.Value.Conn = nc;
            return (new http_test_package.transportDialTesterConnжConn(c), err);
        };
    });
    return dt;
}

// roundTrip starts a RoundTrip.
// It returns immediately, without waiting for the RoundTrip call to complete.
[MethodImpl(MethodImplOptions.NoInlining)] internal static ж<transportDialTesterRoundTrip> roundTrip(this ж<transportDialTester> Ꮡdt) {
    ref var dt = ref Ꮡdt.DerefOrNull();

    dt.t.Helper();
    ref var ctx = ref heap<context.Context>(out var Ꮡctx);
    (ctx, var cancel) = context_package.WithCancel(context_package.Background());
    var (pr, pw) = io.Pipe();
    var rt = Ꮡ(new transportDialTesterRoundTrip(
        t: dt.t,
        roundTripID: dt.roundTripCount,
        done: new channel<EmptyStruct>(0),
        reqBody: new io.PipeWriterжWriteCloser(pw),
        cancel: cancel
    ));
    dt.roundTripCount++;
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
                rtʗ3.Value.conn = info.Conn._<ж<transportDialTesterConn>>();
            }
        )));
        var (req, _) = Δhttp.NewRequestWithContext(Ꮡctx.ValueSlot, postˢ, (~(~Ꮡdt.Value.cst).ts).URL, new io.PipeReaderжReader(prʗ1));
        (~req).Header.Set(contentTypeˢ, textPlainˢ);
        (rtʗ2.Value.res, rtʗ2.Value.err) = (~Ꮡdt.Value.cst).tr.RoundTrip(req);
        Ꮡdt.Value.t.Logf("RoundTrip %v: done (err:%v)"u8, (~rtʗ2).roundTripID, (~rtʗ2).err);
        builtin.close((~rtʗ2).done);
    });
    return rt;
}

// wantDone indicates that a RoundTrip should have returned.
[GoRecv] internal static void wantDone(this ref transportDialTesterRoundTrip rt, ж<transportDialTesterConn> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    rt.t.Helper();
    ᐸꟷ(rt.done);
    if (rt.err != default!) {
        rt.t.Fatalf("RoundTrip %v: want success, got err %v"u8, rt.roundTripID, rt.err);
    }
    if (rt.conn != Ꮡc) {
        rt.t.Fatalf("RoundTrip %v: want on conn %v, got conn %v"u8, rt.roundTripID, c.connID, (~rt.conn).connID);
    }
}

// finish completes a RoundTrip by sending the request body, consuming the response body,
// and closing the response body.
[GoRecv] internal static void finish(this ref transportDialTesterRoundTrip rt) {
    rt.t.Helper();
    if (rt.finished) {
        return;
    }
    rt.finished = true;
    ᐸꟷ(rt.done);
    if (rt.err != default!) {
        return;
    }
    rt.reqBody.Close();
    io.ReadAll((~rt.res).Body);
    (~rt.res).Body.Close();
    rt.t.Logf("RoundTrip %v: closed request body"u8, rt.roundTripID);
}

// wantDial waits for the Transport to start a Dial.
[GoRecv] internal static ж<transportDialTesterConn> wantDial(this ref transportDialTester dt) {
    var c = ᐸꟷ(dt.dials);
    c.Value.connID = dt.dialCount;
    dt.dialCount++;
    dt.t.Logf("Dial %v: started"u8, (~c).connID);
    return c;
}

// finish completes a Dial.
[GoRecv] internal static void finish(this ref transportDialTesterConn c, error err) {
    c.t.Logf("Dial %v: finished (err:%v)"u8, c.connID, err);
    c.ready.ᐸꟷ(err);
    builtin.close(c.ready);
}

} // end http_test_package
