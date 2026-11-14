// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using errors = errors_package;
using synctest = global::go.@internal.synctest_package;
using Δhttp = global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using strings = strings_package;
using testing = testing_package;
using context = context_package;
using global::go.@internal;
using global::go.net;
using global::go.net.http;
using io = io_package;
using net = net_package;
using static global::go.net.http_internal_test_package;
using ꓸꓸꓸstring = Span<@string>;

partial class http_test_package {

// An http1ServerTest tests an HTTP/1 server using a fake network.
// It must be used in a synctest bubble.
partial struct http1ServerTest {
    internal ж<testing.T> t;
    internal ж<clientServerTest> cst;
    internal ж<httptest.Server> ts;
}

internal static ж<http1ServerTest> newHTTP1ServerTest(ж<testing.T> t, Δhttp.HandlerFunc h) {
    if (h == nil) {
        h = (Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        };
    }
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(h), optFakeNet.OrTypedNil());
    var st = Ꮡ(new http1ServerTest(
        t: t,
        cst: cst,
        ts: cst.ts
    ));
    return st;
}

// client returns a Client that sends requests to the server.
internal static ж<Δhttp.Client> client(this ref http1ServerTest st) {
    return st.ts.Client();
}

// transport returns a Transport that sends requests to the server.
internal static ж<Δhttp.Transport> transport(this ref http1ServerTest st) {
    return st.ts.Client().Transport._<ж<Δhttp.Transport>>();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleTldˢ => ref exampleTldˢᵇ;
private static readonly @string exampleTldˢᵇ = "example.tld"u8;

// dial returns a connection to the server.
internal static ж<http1TestConn> dial(this ref http1ServerTest st) {
    var t = st.t;
    t.Helper();
    var (nc, err) = st.transport().DialContext(st.t.Context(), tcpˢ, exampleTldˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    var ncʗ1 = nc;
    t.Cleanup(() => {
        ncʗ1.Close();
    });
    var conn = nc._<ж<fakeNetConn>>();
    conn.SetReadError(errWouldBlock); // effectively make reads non-blocking
    return Ꮡ(new http1TestConn(
        t: st.t,
        conn: conn,
        bufr: bufio.NewReader(new http_test_package.fakeNetConnжReader(conn))
    ));
}

internal static error errWouldBlock = errors.New("would block");

partial struct http1TestConn {
    internal ж<testing.T> t;
    internal ж<fakeNetConn> conn;
    internal ж<bufio.Reader> bufr;
}

// writeMessage writes a number of CRLF-terminated lines to the connection.
internal static void writeMessage(this ref http1TestConn tc, params ꓸꓸꓸstring linesʗp) {
    var lines = linesʗp.slice();

    var t = tc.t;
    t.Helper();
    var (_, err) = tc.conn.Write(slice<byte>(strings.Join(lines, "\r\n") + "\r\n")); if (err != nil) {
        t.Fatalf("conn write: %v", err);
    }
}

// readResponse reads a response from the connection (not including the response body).
internal static ж<Δhttp.Response> readResponse(this ref http1TestConn tc) {
    var t = tc.t;
    t.Helper();
    synctest.Wait();
    var (resp, err) = Δhttp.ReadResponse(tc.bufr, nil);
    if (err != nil) {
        t.Fatalf("ReadResponse: %v", err);
    }
    return resp;
}

// wantIdle asserts that the connection is not closed and has no pending data to read.
internal static void wantIdle(this ref http1TestConn tc) {
    var t = tc.t;
    t.Helper();
    synctest.Wait();
    var (got, err) = tc.bufr.Peek(32); if (len(got) != 0 || !errors.Is(err, errWouldBlock)) {
        t.Fatalf("read from conn: %q, %v; expect conn to be idle", got, err);
    }
}

// wantClosed asserts that the connection is read-closed and has no pending data to read.
internal static void wantClosed(this ref http1TestConn tc) {
    var t = tc.t;
    t.Helper();
    synctest.Wait();
    var (got, err) = tc.bufr.Peek(32); if (len(got) != 0 || !AreEqual(err, errWouldBlock)) {
        t.Fatalf("read from conn: %q, %v; expect conn to be closed", got, err);
    }
}

} // end http_test_package
