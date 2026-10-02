// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// End-to-end serving tests
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using gzip = compress.gzip_package;
using zlib = compress.zlib_package;
using context = context_package;
using tls = crypto.tls_package;
using x509 = crypto.x509_package;
using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using synctest = global::go.@internal.synctest_package;
using testenv = global::go.@internal.testenv_package;
using io = io_package;
using log = log_package;
using rand = global::go.math.rand_package;
using multipart = global::go.mime.multipart_package;
using net = net_package;
using static global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using httptrace = global::go.net.http.httptrace_package;
using httputil = global::go.net.http.httputil_package;
using @internal = global::go.net.http.internal_package;
using testcert = global::go.net.http.@internal.testcert_package;
using url = global::go.net.url_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using reflect = reflect_package;
using regexp = regexp_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = global::go.sync.atomic_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using System.Runtime.CompilerServices;
using compress;
using crypto;
using encoding;
using exec = global::go.os.exec_package;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.math;
using global::go.mime;
using global::go.net;
using global::go.net.http;
using global::go.net.http.@internal;
using global::go.os;
using global::go.path;
using global::go.sync;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;
using ꓸꓸꓸstring = Span<@string>;

partial class http_test_package {

[GoType("@string")] partial struct dummyAddr;

[GoType] partial struct oneConnListener {
    internal net.Conn conn;
}

[GoRecv] internal static (net.Conn c, error err) Accept(this ref oneConnListener l) {
    net.Conn c = default!;
    error err = default!;

    c = l.conn;
    if (c == default!) {
        err = io.EOF;
        return (c, err);
    }
    err = default!;
    l.conn = default!;
    return (c, err);
}

[GoRecv] internal static error Close(this ref oneConnListener l) {
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testAddressˢ = "test-address"u8;

[GoRecv] internal static netꓸAddr Addr(this ref oneConnListener l) {
    return ((dummyAddr)(@string)testAddressˢ);
}

internal static @string Network(this dummyAddr a) {
    return ((@string)a);
}

internal static @string String(this dummyAddr a) {
    return ((@string)a);
}

[GoType] partial struct noopConn {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localAddrˢ = "local-addr"u8;

internal static netꓸAddr LocalAddr(this noopConn _) {
    return ((dummyAddr)(@string)localAddrˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string remoteAddrˢ = "remote-addr"u8;

internal static netꓸAddr RemoteAddr(this noopConn _) {
    return ((dummyAddr)(@string)remoteAddrˢ);
}

internal static error SetDeadline(this noopConn _, time.Time t) {
    return default!;
}

internal static error SetReadDeadline(this noopConn _, time.Time t) {
    return default!;
}

internal static error SetWriteDeadline(this noopConn _, time.Time t) {
    return default!;
}

[GoType] partial struct rwTestConn {
    [GoEmbedded] public io_package.Reader Reader;
    [GoEmbedded] public io_package.Writer Writer;
    internal partial ref noopConn noopConn { get; }
    internal Func<error> closeFunc;  // called if non-nil
    internal channel<bool> closec; // else, if non-nil, send value to it on close
}

[GoRecv] internal static error Close(this ref rwTestConn c) {
    if (c.closeFunc != default!) {
        return c.closeFunc();
    }
    var selᴛ35 = c.closec.ᐸꟷ(true, ꓸꓸꓸ);
    switch (trySelect(selᴛ35)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    return default!;
}

[GoType] partial struct testConn {
    internal sync.Mutex readMu; // for TestHandlerBodyClose
    internal bytes.Buffer readBuf;
    internal bytes.Buffer writeBuf;
    internal channel<bool> closec; // 1-buffered; receives true when Close is called
    internal partial ref noopConn noopConn { get; }
}

internal static ж<testConn> newTestConn() {
    return Ꮡ(new testConn(closec: new channel<bool>(1)));
}

internal static (nint, error) Read(this ж<testConn> Ꮡc, slice<byte> b) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.readMu.Lock();
        ᒐd1 = true;
        return c.readBuf.Read(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().readMu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static (nint, error) Write(this ref testConn c, slice<byte> b) {
    return c.writeBuf.Write(b);
}

[GoRecv] internal static error Close(this ref testConn c) {
    var selᴛ36 = c.closec.ᐸꟷ(true, ꓸꓸꓸ);
    switch (trySelect(selᴛ36)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    return default!;
}

// reqBytes treats req as a request (with \n delimiters) and returns it with \r\n delimiters,
// ending in \r\n\r\n
internal static slice<byte> reqBytes(@string req) {
    return slice<byte>(strings.ReplaceAll(strings.TrimSpace(req), "\n"u8, "\r\n"u8) + "\r\n\r\n");
}

[GoType] partial struct handlerTest {
    internal bytes.Buffer logbuf;
    internal httpꓸHandler handler;
}

internal static handlerTest newHandlerTest(httpꓸHandler h) {
    return new handlerTest(handler: h);
}

internal static @string rawResponse(this ж<handlerTest> Ꮡht, @string req) {
    ref var ht = ref Ꮡht.DerefOrNull();

    var reqb = reqBytes(req);
    ref var output = ref heap(new strings.Builder(), out var Ꮡoutput);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(reqb)),
        Writer: new http_test_package.strings_BuilderжWriter(Ꮡoutput),
        closec: new channel<bool>(1)
    ));
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    var srv = Ꮡ(new Server(
        ErrorLog: log.New(new http_test_package.bytes_BufferжWriter(Ꮡht.of(handlerTest.Ꮡlogbuf)), ""u8, 0),
        Handler: ht.handler
    ));
    var srvʗ1 = srv;
    goǃ(ᴛ1 => srvʗ1.Serve(ᴛ1), new http_test_package.oneConnListenerжListener(ln));
    ᐸꟷ((~conn).closec);
    return output.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilFirstRequestˢ = (@string)"Got nil first request."u8;

public static void TestConsumingBodyOnNextConn(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var conn = @new<testConn>();
        for (nint i = 0; i < 2; i++) {
            conn.of(testConn.ᏑreadBuf).Write(slice<byte>((@string)("POST / HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "Content-Length: 11\r\n"u8 + "\r\n"u8 + "foo=1&bar=1"u8)));
        }
        nint reqNum = 0;
        var ch = new channel<ж<Δhttp.Request>>(0);
        var servech = new channel<error>(0);
        var listener = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
        var chʗ1 = ch;
        var handler = (Δhttp.ResponseWriter res, ж<Δhttp.Request> reqΔ1) => {
            reqNum++;
            chʗ1.ᐸꟷ(reqΔ1);
        };
        var handlerʗ1 = handler;
        var listenerʗ1 = listener;
        var servechʗ1 = servech;
        goǃ(() => {
            servechʗ1.ᐸꟷ(Serve(new http_test_package.oneConnListenerжListener(listenerʗ1), new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(handlerʗ1))));
        });
        ж<Δhttp.Request> req = default!;
        req = ᐸꟷ(ch);
        if (req == nil) {
            Ꮡt.Fatal(gotNilFirstRequestˢ);
        }
        if ((~req).Method != "POST"u8) {
            Ꮡt.Errorf("For request #1's method, got %q; expected %q"u8,
                (~req).Method, postˢ);
        }
        req = ᐸꟷ(ch);
        if (req == nil) {
            Ꮡt.Fatal(gotNilFirstRequestˢ);
        }
        if ((~req).Method != "POST"u8) {
            Ꮡt.Errorf("For request #2's method, got %q; expected %q"u8,
                (~req).Method, postˢ);
        }
        {
            var serveerr = ᐸꟷ(servech); if (!AreEqual(serveerr, io.EOF)) {
                Ꮡt.Errorf("Serve returned %q; expected EOF"u8, serveerr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("@string")] partial struct stringHandler;

internal static void ServeHTTP(this stringHandler s, Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡr) {
    w.Header().Set(resultˢ, ((@string)s));
}


[GoType("dyn")] partial struct handlersᴛ1 {
    internal @string pattern;
    internal @string msg;
}
internal static slice<handlersᴛ1> handlers = new handlersᴛ1[]{
    new("/"u8, "Default"u8),
    new("/someDir/"u8, "someDir"u8),
    new("/#/"u8, "hash"u8),
    new("someHost.com/someDir/"u8, "someHost.com/someDir"u8)
}.slice();

// redirections for trees

[GoType("dyn")] partial struct vtestsᴛ1 {
    internal @string url;
    internal @string expected;
}
internal static slice<vtestsᴛ1> vtests = new vtestsᴛ1[]{
    new("http://localhost/someDir/apage"u8, "someDir"u8),
    new("http://localhost/%23/apage"u8, "hash"u8),
    new("http://localhost/otherDir/apage"u8, "Default"u8),
    new("http://someHost.com/someDir/apage"u8, "someHost.com/someDir"u8),
    new("http://otherHost.com/someDir/apage"u8, "someDir"u8),
    new("http://otherHost.com/aDir/apage"u8, "Default"u8),
    new("http://localhost/someDir"u8, "/someDir/"u8),
    new("http://localhost/%23"u8, "/%23/"u8),
    new("http://someHost.com/someDir"u8, "/someDir/"u8)
}.slice();

public static void TestHostHandlers(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHostHandlers(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testHostHandlers(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var mux = NewServeMux();
        foreach (var (_, h) in handlers) {
            mux.Handle(h.pattern, ((stringHandler)h.msg));
        }
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var cc = httputil.NewClientConn(conn, nil);
        foreach (var (_, vt) in vtests) {
            ж<Δhttp.Response> r = default!;
            ref var req = ref heap(new Δhttp.Request(), out var Ꮡreq);
            {
                (req.URL, err) = url.Parse(vt.url); if (err != default!) {
                    Ꮡt.Errorf("cannot parse url: %v"u8, err);
                    continue;
                }
            }
            {
                var errΔ1 = cc.Write(Ꮡreq); if (errΔ1 != default!) {
                    Ꮡt.Errorf("writing request: %v"u8, errΔ1);
                    continue;
                }
            }
            (r, var errΔ2) = cc.Read(Ꮡreq);
            if (errΔ2 != default!) {
                Ꮡt.Errorf("reading response: %v"u8, errΔ2);
                continue;
            }
            var exprᴛ1 = (~r).StatusCode;
            if (exprᴛ1 == StatusOK) {
                @string s = (~r).Header.Get(resultˢ);
                if (s != vt.expected) {
                    Ꮡt.Errorf("Get(%q) = %q, want %q"u8, vt.url, s, vt.expected);
                }
            }
            else if (exprᴛ1 == StatusMovedPermanently) {
                @string s = (~r).Header.Get(locationˢ);
                if (s != vt.expected) {
                    Ꮡt.Errorf("Get(%q) = %q, want %q"u8, vt.url, s, vt.expected);
                }
            }
            else { /* default: */
                Ꮡt.Errorf("Get(%q) unhandled status code %d"u8, vt.url, (~r).StatusCode);
            }

        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


[GoType("dyn")] partial struct serveMuxRegisterᴛ1 {
    internal @string pattern;
    internal httpꓸHandler h;
}
internal static slice<serveMuxRegisterᴛ1> serveMuxRegister = new serveMuxRegisterᴛ1[]{
    new("/dir/"u8, new http_test_package.http_HandlerFuncᴠΔHandler(serve(200))),
    new("/search"u8, new http_test_package.http_HandlerFuncᴠΔHandler(serve(201))),
    new("codesearch.google.com/search"u8, new http_test_package.http_HandlerFuncᴠΔHandler(serve(202))),
    new("codesearch.google.com/"u8, new http_test_package.http_HandlerFuncᴠΔHandler(serve(203))),
    new("example.com/"u8, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc(checkQueryStringHandler)))
}.slice();

// serve returns a handler that sends a response with the given code.
internal static Δhttp.HandlerFunc serve(nint code) {
    return (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(code);
    };
}

// checkQueryStringHandler checks if r.URL.RawQuery has the same value
// as the URL excluding the scheme and the query string and sends 200
// response code if it is, 500 otherwise.
internal static void checkQueryStringHandler(Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var u = r.URL.Value;
    u.Scheme = "http"u8;
    u.Host = r.Host;
    u.RawQuery = ""u8;
    if ("http://"u8 + (~r.URL).RawQuery == u.String()){
        w.WriteHeader(200);
    } else {
        w.WriteHeader(500);
    }
}

// The /foo -> /foo/ redirect applies to CONNECT requests
// but the path canonicalization does not.

[GoType("dyn")] partial struct serveMuxTestsᴛ1 {
    internal @string method;
    internal @string host;
    internal @string path;
    internal nint code;
    internal @string pattern;
}
internal static slice<serveMuxTestsᴛ1> serveMuxTests = new serveMuxTestsᴛ1[]{
    new("GET"u8, "google.com"u8, "/"u8, 404, ""u8),
    new("GET"u8, "google.com"u8, "/dir"u8, 301, "/dir/"u8),
    new("GET"u8, "google.com"u8, "/dir/"u8, 200, "/dir/"u8),
    new("GET"u8, "google.com"u8, "/dir/file"u8, 200, "/dir/"u8),
    new("GET"u8, "google.com"u8, "/search"u8, 201, "/search"u8),
    new("GET"u8, "google.com"u8, "/search/"u8, 404, ""u8),
    new("GET"u8, "google.com"u8, "/search/foo"u8, 404, ""u8),
    new("GET"u8, "codesearch.google.com"u8, "/search"u8, 202, "codesearch.google.com/search"u8),
    new("GET"u8, "codesearch.google.com"u8, "/search/"u8, 203, "codesearch.google.com/"u8),
    new("GET"u8, "codesearch.google.com"u8, "/search/foo"u8, 203, "codesearch.google.com/"u8),
    new("GET"u8, "codesearch.google.com"u8, "/"u8, 203, "codesearch.google.com/"u8),
    new("GET"u8, "codesearch.google.com:443"u8, "/"u8, 203, "codesearch.google.com/"u8),
    new("GET"u8, "images.google.com"u8, "/search"u8, 201, "/search"u8),
    new("GET"u8, "images.google.com"u8, "/search/"u8, 404, ""u8),
    new("GET"u8, "images.google.com"u8, "/search/foo"u8, 404, ""u8),
    new("GET"u8, "google.com"u8, "/../search"u8, 301, "/search"u8),
    new("GET"u8, "google.com"u8, "/dir/.."u8, 301, ""u8),
    new("GET"u8, "google.com"u8, "/dir/.."u8, 301, ""u8),
    new("GET"u8, "google.com"u8, "/dir/./file"u8, 301, "/dir/"u8),
    new("CONNECT"u8, "google.com"u8, "/dir"u8, 301, "/dir/"u8),
    new("CONNECT"u8, "google.com"u8, "/../search"u8, 404, ""u8),
    new("CONNECT"u8, "google.com"u8, "/dir/.."u8, 200, "/dir/"u8),
    new("CONNECT"u8, "google.com"u8, "/dir/.."u8, 200, "/dir/"u8),
    new("CONNECT"u8, "google.com"u8, "/dir/./file"u8, 200, "/dir/"u8)
}.slice();

public static void TestServeMuxHandler(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var mux = NewServeMux();
    foreach (var (_, e) in serveMuxRegister) {
        mux.Handle(e.pattern, e.h);
    }
    foreach (var (_, tt) in serveMuxTests) {
        var r = Ꮡ(new Request(
            Method: tt.method,
            Host: tt.host,
            URL: Ꮡ(new url.URL(
                Path: tt.path
            ))
        ));
        var (h, pattern) = mux.Handler(r);
        var rr = httptest.NewRecorder();
        h.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rr), r);
        if (pattern != tt.pattern || (~rr).Code != tt.code) {
            Ꮡt.Errorf("%s %s %s = %d, %q, want %d, %q"u8, tt.method, tt.host, tt.path, (~rr).Code, pattern, tt.code, tt.pattern);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedCallToMuxˢ = (@string)"expected call to mux.HandleFunc to panic"u8;

// Issue 24297
public static void TestServeMuxHandleFuncWithNilHandler(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(() => {
            {
                var err = recover(); if (err == default!) {
                    Ꮡt.Error(expectedCallToMuxˢ);
                }
            }
        }, ref ᒐ);
        var mux = NewServeMux();
        mux.HandleFunc("/"u8, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


[GoType("dyn")] partial struct serveMuxTests2ᴛ1 {
    internal @string method;
    internal @string host;
    internal @string url;
    internal nint code;
    internal bool redirOk;
}
internal static slice<serveMuxTests2ᴛ1> serveMuxTests2 = new serveMuxTests2ᴛ1[]{
    new("GET"u8, "google.com"u8, "/"u8, 404, false),
    new("GET"u8, "example.com"u8, "/test/?example.com/test/"u8, 200, false),
    new("GET"u8, "example.com"u8, "test/?example.com/test/"u8, 200, true)
}.slice();

// TestServeMuxHandlerRedirects tests that automatic redirects generated by
// mux.Handler() shouldn't clear the request's query string.
public static void TestServeMuxHandlerRedirects(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var mux = NewServeMux();
    foreach (var (_, e) in serveMuxRegister) {
        mux.Handle(e.pattern, e.h);
    }
    foreach (var (_, tt) in serveMuxTests2) {
        nint tries = 1; // expect at most 1 redirection if redirOk is true.
        @string turl = tt.url;
        while (ᐧ) {
            var (u, e) = url.Parse(turl);
            if (e != default!) {
                Ꮡt.Fatal(e);
            }
            var r = Ꮡ(new Request(
                Method: tt.method,
                Host: tt.host,
                URL: u
            ));
            var (h, _) = mux.Handler(r);
            var rr = httptest.NewRecorder();
            h.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rr), r);
            if ((~rr).Code != 301) {
                if ((~rr).Code != tt.code) {
                    Ꮡt.Errorf("%s %s %s = %d, want %d"u8, tt.method, tt.host, tt.url, (~rr).Code, tt.code);
                }
                break;
            }
            if (!tt.redirOk) {
                Ꮡt.Errorf("%s %s %s, unexpected redirect"u8, tt.method, tt.host, tt.url);
                break;
            }
            turl = (~rr).HeaderMap.Get(locationˢ);
            tries--;
        }
        if (tries < 0) {
            Ꮡt.Errorf("%s %s %s, too many redirects"u8, tt.method, tt.host, tt.url);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooTxtˢ3 = "/foo.txt"u8;

// Tests for https://golang.org/issue/900
public static void TestMuxRedirectLeadingSlashes(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var paths = new @string[]{"//foo.txt"u8, "///foo.txt"u8, "/../../foo.txt"u8}.slice();
    foreach (var (_, path) in paths) {
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader("GET "u8 + path + " HTTP/1.1\r\nHost: test\r\n\r\n"u8))));
        if (err != default!) {
            Ꮡt.Errorf("%s"u8, err);
        }
        var mux = NewServeMux();
        var resp = httptest.NewRecorder();
        mux.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(resp), req);
        {
            @string loc = resp.Header().Get(locationˢ);
            @string expected = fooTxtˢ3; if (loc != expected) {
                Ꮡt.Errorf("Expected Location header set to %q; got %q"u8, expected, loc);
                return;
            }
        }
        {
            nint code = resp.Value.Code;
            nint expected = StatusMovedPermanently; if (code != expected) {
                Ꮡt.Errorf("Expected response code of StatusMovedPermanently; got %d"u8, code);
                return;
            }
        }
    }
}

// Test that the special cased "/route" redirect
// implicitly created by a registered "/route/"
// properly sets the query string in the redirect URL.
// See Issue 17841.
public static void TestServeWithSlashRedirectKeepsQueryString(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServeWithSlashRedirectKeepsQueryString(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testOneˢ = "/testOne"u8;
internal static readonly @string testTwoˢ = "/testTwo/"u8;
internal static readonly @string testThreeˢ = "/testThree"u8;
internal static readonly @string testThreeˢ2 = "/testThree/"u8;

[GoType("dyn")] internal partial struct testServeWithSlashRedirectKeepsQueryString_tests {
    internal @string path;
    internal @string method;
    internal @string want;
    internal bool statusOk;
}

internal static void testServeWithSlashRedirectKeepsQueryString(ж<testing.T> Ꮡt, testMode mode) {
    var writeBackQuery = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%s"u8, (~(~r).URL).RawQuery);
    };
    var mux = NewServeMux();
    mux.HandleFunc(testOneˢ, writeBackQuery);
    mux.HandleFunc(testTwoˢ, writeBackQuery);
    mux.HandleFunc(testThreeˢ, writeBackQuery);
    mux.HandleFunc(testThreeˢ2, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%s:bar"u8, (~(~r).URL).RawQuery);
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
    var tests = new array<testServeWithSlashRedirectKeepsQueryString_tests>(10){
        [0] = new("/testOne?this=that"u8, "GET"u8, "this=that"u8, true),
        [1] = new("/testTwo?foo=bar"u8, "GET"u8, "foo=bar"u8, true),
        [2] = new("/testTwo?a=1&b=2&a=3"u8, "GET"u8, "a=1&b=2&a=3"u8, true),
        [3] = new("/testTwo?"u8, "GET"u8, ""u8, true),
        [4] = new("/testThree?foo"u8, "GET"u8, "foo"u8, true),
        [5] = new("/testThree/?foo"u8, "GET"u8, "foo:bar"u8, true),
        [6] = new("/testThree?foo"u8, "CONNECT"u8, "foo"u8, true),
        [7] = new("/testThree/?foo"u8, "CONNECT"u8, "foo:bar"u8, true), // canonicalization or not

        [8] = new("/testOne/foo/..?foo"u8, "GET"u8, "foo"u8, true),
        [9] = new("/testOne/foo/..?foo"u8, "CONNECT"u8, "404 page not found\n"u8, false)
    };
    foreach (var (i, tt) in tests.ΔRangeSnapshot()) {
        var (req, _) = NewRequest(tt.method, (~ts).URL + tt.path, default!);
        var (res, err) = ts.Client().Do(req);
        if (err != default!) {
            continue;
        }
        var (slurp, _) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (!tt.statusOk) {
            {
                nint got = res.Value.StatusCode;
                nint want = 404; if (got != want) {
                    Ꮡt.Errorf("#%d: Status = %d; want = %d"u8, i, got, want);
                }
            }
        }
        {
            @string got = ((@string)slurp);
            @string want = tt.want; if (got != want) {
                Ꮡt.Errorf("#%d: Body = %q; want = %q"u8, i, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleComPkgFooˢ = "example.com/pkg/foo/"u8;
internal static readonly @string exampleComPkgBarˢ = "example.com/pkg/bar"u8;
internal static readonly @string exampleComPkgBarˢ2 = "example.com/pkg/bar/"u8;
internal static readonly @string exampleCom3000PkgConnectˢ = "example.com:3000/pkg/connect/"u8;
internal static readonly @string exampleCom9000ˢ = "example.com:9000/"u8;
internal static readonly @string pkgBazˢ = "/pkg/baz/"u8;

[GoType("dyn")] internal partial struct TestServeWithSlashRedirectForHostPatterns_tests {
    internal @string method;
    internal @string url;
    internal nint code;
    internal @string loc;
    internal @string want;
}

public static void TestServeWithSlashRedirectForHostPatterns(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var mux = NewServeMux();
    mux.Handle(exampleComPkgFooˢ, ((stringHandler)(@string)exampleComPkgFooˢ));
    mux.Handle(exampleComPkgBarˢ, ((stringHandler)(@string)exampleComPkgBarˢ));
    mux.Handle(exampleComPkgBarˢ2, ((stringHandler)(@string)exampleComPkgBarˢ2));
    mux.Handle(exampleCom3000PkgConnectˢ, ((stringHandler)(@string)exampleCom3000PkgConnectˢ));
    mux.Handle(exampleCom9000ˢ, ((stringHandler)(@string)exampleCom9000ˢ));
    mux.Handle(pkgBazˢ, ((stringHandler)(@string)pkgBazˢ));
    var tests = new TestServeWithSlashRedirectForHostPatterns_tests[]{
        new("GET"u8, "http://example.com/"u8, 404, ""u8, ""u8),
        new("GET"u8, "http://example.com/pkg/foo"u8, 301, "/pkg/foo/"u8, ""u8),
        new("GET"u8, "http://example.com/pkg/bar"u8, 200, ""u8, "example.com/pkg/bar"u8),
        new("GET"u8, "http://example.com/pkg/bar/"u8, 200, ""u8, "example.com/pkg/bar/"u8),
        new("GET"u8, "http://example.com/pkg/baz"u8, 301, "/pkg/baz/"u8, ""u8),
        new("GET"u8, "http://example.com:3000/pkg/foo"u8, 301, "/pkg/foo/"u8, ""u8),
        new("CONNECT"u8, "http://example.com/"u8, 404, ""u8, ""u8),
        new("CONNECT"u8, "http://example.com:3000/"u8, 404, ""u8, ""u8),
        new("CONNECT"u8, "http://example.com:9000/"u8, 200, ""u8, "example.com:9000/"u8),
        new("CONNECT"u8, "http://example.com/pkg/foo"u8, 301, "/pkg/foo/"u8, ""u8),
        new("CONNECT"u8, "http://example.com:3000/pkg/foo"u8, 404, ""u8, ""u8),
        new("CONNECT"u8, "http://example.com:3000/pkg/baz"u8, 301, "/pkg/baz/"u8, ""u8),
        new("CONNECT"u8, "http://example.com:3000/pkg/connect"u8, 301, "/pkg/connect/"u8, ""u8)
    }.slice();
    foreach (var (i, tt) in tests) {
        var (req, _) = NewRequest(tt.method, tt.url, default!);
        var w = httptest.NewRecorder();
        mux.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);
        {
            nint got = w.Value.Code;
            nint want = tt.code; if (got != want) {
                Ꮡt.Errorf("#%d: Status = %d; want = %d"u8, i, got, want);
            }
        }
        if (tt.code == 301){
            {
                @string got = (~w).HeaderMap.Get(locationˢ);
                @string want = tt.loc; if (got != want) {
                    Ꮡt.Errorf("#%d: Location = %q; want = %q"u8, i, got, want);
                }
            }
        } else {
            {
                @string got = (~w).HeaderMap.Get(resultˢ);
                @string want = tt.want; if (got != want) {
                    Ꮡt.Errorf("#%d: Result = %q; want = %q"u8, i, got, want);
                }
            }
        }
    }
}

// Test that we don't attempt trailing-slash redirect on a path that already has
// a trailing slash.
// See issue #65624.
public static void TestMuxNoSlashRedirectWithTrailingSlash(ж<testing.T> Ꮡt) {
    var mux = NewServeMux();
    mux.HandleFunc("/{x}/"u8, (Δhttp.ResponseWriter wΔ1, ж<Δhttp.Request> r) => {
        fmt.Fprintln(new http_test_package.http_ResponseWriterᴠWriter(wΔ1), (@string)"ok"u8);
    });
    var w = httptest.NewRecorder();
    var (req, _) = NewRequest(getˢ2, "/"u8, default!);
    mux.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);
    {
        nint g = w.Value.Code;
        nint wΔ2 = 404; if (g != wΔ2) {
            Ꮡt.Errorf("got %d, want %d"u8, g, wΔ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getXˢ = "GET /{x}/"u8;

// Test that we don't attempt trailing-slash response 405 on a path that already has
// a trailing slash.
// See issue #67657.
public static void TestMuxNoSlash405WithTrailingSlash(ж<testing.T> Ꮡt) {
    var mux = NewServeMux();
    mux.HandleFunc(getXˢ, (Δhttp.ResponseWriter wΔ1, ж<Δhttp.Request> r) => {
        fmt.Fprintln(new http_test_package.http_ResponseWriterᴠWriter(wΔ1), (@string)"ok"u8);
    });
    var w = httptest.NewRecorder();
    var (req, _) = NewRequest(getˢ2, "/"u8, default!);
    mux.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);
    {
        nint g = w.Value.Code;
        nint wΔ2 = 404; if (g != wΔ2) {
            Ꮡt.Errorf("got %d, want %d"u8, g, wΔ2);
        }
    }
}

public static void TestShouldRedirectConcurrency(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testShouldRedirectConcurrency(Δp0, Δp1));
}

internal static void testShouldRedirectConcurrency(ж<testing.T> Ꮡt, testMode mode) {
    var mux = NewServeMux();
    newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux));
    mux.HandleFunc("/"u8, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    });
}

public static void BenchmarkServeMux(ж<testing.B> Ꮡb) {
    benchmarkServeMux(Ꮡb, true);
}

public static void BenchmarkServeMux_SkipServe(ж<testing.B> Ꮡb) {
    benchmarkServeMux(Ꮡb, false);
}

[GoType("dyn")] internal partial struct benchmarkServeMux_test {
    internal @string path;
    internal nint code;
    internal ж<Δhttp.Request> req;
}

internal static void benchmarkServeMux(ж<testing.B> Ꮡb, bool runHandler) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Build example handlers and requests
    slice<benchmarkServeMux_test> tests = default!;
    var endpoints = new @string[]{"search"u8, "dir"u8, "file"u8, "change"u8, "count"u8, "s"u8}.slice();
    foreach (var (_, e) in endpoints) {
        for (nint i = 200; i < 230; i++) {
            ref var p = ref heap<@string>(out var Ꮡp);
            p = fmt.Sprintf("/%s/%d/"u8, e, i);
            tests = append(tests, new benchmarkServeMux_test(
                path: p,
                code: i,
                req: Ꮡ(new Request(Method: "GET"u8, Host: "localhost"u8, URL: Ꮡ(new url.URL(Path: p))))
            ));
        }
    }
    var mux = NewServeMux();
    foreach (var (_, tt) in tests) {
        mux.Handle(tt.path, new http_test_package.http_HandlerFuncᴠΔHandler(serve(tt.code)));
    }
    var rw = httptest.NewRecorder();
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, tt) in tests) {
            rw.Value = new httptest.ResponseRecorder(nil);
            var (h, pattern) = mux.Handler(tt.req);
            if (runHandler) {
                h.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rw), tt.req);
                if (pattern != tt.path || (~rw).Code != tt.code) {
                    Ꮡb.Fatalf("got %d, %q, want %d, %q"u8, (~rw).Code, pattern, tt.code, tt.path);
                }
            }
        }
    }
}

public static void TestServerTimeouts(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerTimeouts(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerTimeouts(ж<testing.T> Ꮡt, testMode mode) {
    runTimeSensitiveTest(Ꮡt, new time.Duration[]{
        10 * time.Millisecond,
        50 * time.Millisecond,
        100 * time.Millisecond,
        500 * time.Millisecond,
        1 * time.ΔSecond
    }.slice(), (ж<testing.T> tΔ1, time.Duration timeout) => testServerTimeoutsWithTimeout(tΔ1, timeout, mode));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string req1ˢ = "req=1"u8;
internal static readonly @string req2ˢ = "req=2"u8;

internal static error testServerTimeoutsWithTimeout(ж<testing.T> Ꮡt, time.Duration timeout, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ref var reqNum = ref heap(new atomic.Int32(), out var ᏑreqNum);
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter res, ж<Δhttp.Request> req) => {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(res), "req=%d"u8, ᏑreqNum.Add(1));
        })), (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.ReadTimeout = timeout;
            tsΔ1.Value.Config.Value.WriteTimeout = timeout;
        });
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var ts = cst.Value.ts;
        // Hit the HTTP server successfully.
        var c = ts.Client();
        var (r, err) = c.Get((~ts).URL);
        if (err != default!) {
            return fmt.Errorf("http Get #1: %v"u8, err);
        }
        (var got, err) = io.ReadAll((~r).Body);
        @string expected = req1ˢ;
        if (((sstring)got) != expected || err != default!) {
            return fmt.Errorf("Unexpected response for request #1; got %q ,%v; expected %q, nil"u8,
                ((@string)got), err, expected);
        }
        // Slow client that should timeout.
        var t1 = time.Now();
        (var conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            return fmt.Errorf("Dial: %v"u8, err);
        }
        var buf = new slice<byte>(1);
        (var n, err) = conn.Read(buf);
        conn.Close();
        var latency = time.Since(t1);
        if (n != 0 || !AreEqual(err, io.EOF)) {
            return fmt.Errorf("Read = %v, %v, wanted %v, %v"u8, n, err, (nint)(0), io.EOF);
        }
        var minLatency = timeout / 5 * 4;
        if (latency < minLatency) {
            return fmt.Errorf("got EOF after %s, want >= %s"u8, latency, minLatency);
        }
        // Hit the HTTP server successfully again, verifying that the
        // previous slow connection didn't run our handler.  (that we
        // get "req=2", not "req=3")
        (r, err) = c.Get((~ts).URL);
        if (err != default!) {
            return fmt.Errorf("http Get #2: %v"u8, err);
        }
        (got, err) = io.ReadAll((~r).Body);
        (~r).Body.Close();
        expected = req2ˢ;
        if (((sstring)got) != expected || err != default!) {
            return fmt.Errorf("Get #2 got %q, %v, want %q, nil"u8, ((@string)got), err, expected);
        }
        if (!testing.Short()) {
            var (connΔ1, errΔ1) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
            if (errΔ1 != default!) {
                return fmt.Errorf("long Dial: %v"u8, errΔ1);
            }
            var connʗ1 = connΔ1;
            defer(() => connʗ1.Close(), ref ᒐ);
            goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), io.Discard, new http_test_package.net_ConnᴠReader(connΔ1));
            for (nint i = 0; i < 5; i++) {
                var (_, errΔ2) = connΔ1.Write(slice<byte>("GET / HTTP/1.1\r\nHost: foo\r\n\r\n"u8));
                if (errΔ2 != default!) {
                    return fmt.Errorf("on write %d: %v"u8, i, errΔ2);
                }
                time.Sleep(timeout / 2);
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static void TestServerReadTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerReadTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tooManyRetriesˢ = "too many retries"u8;

internal static void testServerReadTimeout(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string respBody = responseBodyˢ;
        for (var timeoutᴛ1 = 5 * time.Millisecond; ᐧ ; timeoutᴛ1 *= 2) {
            var timeout = timeoutᴛ1;
            var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter resΔ1, ж<Δhttp.Request> req) => {
                var (_, errΔ1) = io.Copy(io.Discard, (~req).Body);
                if (!errors.Is(errΔ1, os.ErrDeadlineExceeded)) {
                    Ꮡt.Errorf("server timed out reading request body: got err %v; want os.ErrDeadlineExceeded"u8, errΔ1);
                }
                resΔ1.Write(slice<byte>(respBody));
            })), (ж<httptest.Server> ts) => {
                ts.Value.Config.Value.ReadHeaderTimeout = -1; // don't time out while reading headers
                ts.Value.Config.Value.ReadTimeout = timeout;
                Ꮡt.Logf("Server.Config.ReadTimeout = %v"u8, timeout);
            });
            ref var retries = ref heap(new atomic.Int32(), out var Ꮡretries);
            (~(~cst).c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = (ж<url.URL>, error) (ж<Δhttp.Request> _Δp0) => {
                if (Ꮡretries.Add(1) != 1) {
                    return (default!, errors.New(tooManyRetriesˢ));
                }
                return (default!, default!);
            };
            var (pr, pw) = io.Pipe();
            var (res, err) = (~cst).c.Post((~(~cst).ts).URL, textApocryphalˢ, new io.PipeReaderжReader(pr));
            if (err != default!) {
                Ꮡt.Logf("Get error, retrying: %v"u8, err);
                cst.close();
                continue;
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            (var got, err) = io.ReadAll((~res).Body);
            if (((sstring)got) != respBody || err != default!) {
                Ꮡt.Errorf("client read response body: %q, %v; want %q, nil"u8, ((@string)got), err, respBody);
            }
            pw.Close();
            break;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerNoReadTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerNoReadTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloGophersˢ = "Hello, Gophers!"u8;
internal static readonly @string hiGophersˢ = "Hi, Gophers!"u8;

internal static void testServerNoReadTimeout(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string reqBody = helloGophersˢ;
        @string resBody = hiGophersˢ;
        foreach (var (_, timeout) in new time.Duration[]{0, -1}.slice()) {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter resΔ1, ж<Δhttp.Request> req) => {
                var ctl = NewResponseController(resΔ1);
                ctl.EnableFullDuplex();
                resΔ1.WriteHeader(StatusOK);
                // Flush the headers before processing the request body
                // to unblock the client from the RoundTrip.
                {
                    var errΔ1 = ctl.Flush(); if (errΔ1 != default!) {
                        Ꮡt.Errorf("server flush response: %v"u8, errΔ1);
                        return;
                    }
                }
                var (gotΔ1, errΔ2) = io.ReadAll((~req).Body);
                if (((sstring)gotΔ1) != reqBody || errΔ2 != default!) {
                    Ꮡt.Errorf("server read request body: %v; got %q, want %q"u8, errΔ2, gotΔ1, reqBody);
                }
                resΔ1.Write(slice<byte>(resBody));
            })), (ж<httptest.Server> ts) => {
                ts.Value.Config.Value.ReadTimeout = timeout;
                Ꮡt.Logf("Server.Config.ReadTimeout = %d"u8, timeout);
            });
            var (pr, pw) = io.Pipe();
            var (res, err) = (~cst).c.Post((~(~cst).ts).URL, textPlainˢ, new io.PipeReaderжReader(pr));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            // TODO(panjf2000): sleep is not so robust, maybe find a better way to test this?
            time.Sleep(10 * time.Millisecond); // stall sending body to server to test server doesn't time out
            pw.Write(slice<byte>(reqBody));
            pw.Close();
            (var got, err) = io.ReadAll((~res).Body);
            if (((sstring)got) != resBody || err != default!) {
                Ꮡt.Errorf("client read response body: %v; got %v, want %q"u8, err, got, resBody);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerWriteTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerWriteTimeout(Δp0, Δp1));
}

internal static void testServerWriteTimeout(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        for (var timeoutᴛ1 = 5 * time.Millisecond; ᐧ ; timeoutᴛ1 *= 2) {
            var timeout = timeoutᴛ1;
            var errc = new channel<error>(2);
            var errcʗ1 = errc;

            var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter resΔ1, ж<Δhttp.Request> req) => {
                errcʗ1.ᐸꟷ(default!);
                var (_, errΔ1) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(resΔ1), ((neverEnding)(rune)'a'));
                errcʗ1.ᐸꟷ(errΔ1);
            })), (ж<httptest.Server> ts) => {
                ts.Value.Config.Value.WriteTimeout = timeout;
                Ꮡt.Logf("Server.Config.WriteTimeout = %v"u8, timeout);
            });
            // The server's WriteTimeout parameter also applies to reads during the TLS
            // handshake. The client makes the last write during the handshake, and if
            // the server happens to time out during the read of that write, the client
            // may think that the connection was accepted even though the server thinks
            // it timed out.
            //
            // The client only notices that the server connection is gone when it goes
            // to actually write the request — and when that fails, it retries
            // internally (the same as if the server had closed the connection due to a
            // racing idle-timeout).
            //
            // With unlucky and very stable scheduling (as may be the case with the fake wasm
            // net stack), this can result in an infinite retry loop that doesn't
            // propagate the error up far enough for us to adjust the WriteTimeout.
            //
            // To avoid that problem, we explicitly forbid internal retries by rejecting
            // them in a Proxy hook in the transport.
            ref var retries = ref heap(new atomic.Int32(), out var Ꮡretries);
            (~(~cst).c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = (ж<url.URL>, error) (ж<Δhttp.Request> _Δp0) => {
                if (Ꮡretries.Add(1) != 1) {
                    return (default!, errors.New(tooManyRetriesˢ));
                }
                return (default!, default!);
            };
            var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
            if (err != default!) {
                // Probably caused by the write timeout expiring before the handler runs.
                Ꮡt.Logf("Get error, retrying: %v"u8, err);
                cst.close();
                continue;
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            (_, err) = io.Copy(io.Discard, (~res).Body);
            if (err == default!) {
                Ꮡt.Errorf("client reading from truncated request body: got nil error, want non-nil"u8);
            }
            var selᴛ37 = errc;
            switch (trySelect(ᐸꟷ(selᴛ37, ꓸꓸꓸ))) {
            case 0 when selᴛ37.ꟷᐳ(out _): {
                err = ᐸꟷ(errc); // io.Copy error
                if (!errors.Is(err, os.ErrDeadlineExceeded)) {
                    Ꮡt.Errorf("server timed out writing request body: got err %v; want os.ErrDeadlineExceeded"u8, err);
                }
                return;
            }
            default: {
                Ꮡt.Logf("handler didn't run, retrying"u8);
                cst.close();
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// The write timeout expired before the handler started.
public static void TestServerNoWriteTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerNoWriteTimeout(Δp0, Δp1));
}

internal static void testServerNoWriteTimeout(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        foreach (var (_, timeout) in new time.Duration[]{0, -1}.slice()) {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter resΔ1, ж<Δhttp.Request> req) => {
                var (_, errΔ1) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(resΔ1), ((neverEnding)(rune)'a'));
                Ꮡt.Logf("server write response: %v"u8, errΔ1);
            })), (ж<httptest.Server> ts) => {
                ts.Value.Config.Value.WriteTimeout = timeout;
                Ꮡt.Logf("Server.Config.WriteTimeout = %d"u8, timeout);
            });
            var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            (var n, err) = io.CopyN(io.Discard, (~res).Body, ((int64)1 << (int)(20))); // 1MB should be sufficient to prove the point
            if (n != ((int64)1 << (int)(20)) || err != default!) {
                Ꮡt.Errorf("client read response body: %d, %v"u8, n, err);
            }
            // This shutdown really should be automatic, but it isn't right now.
            // Shutdown (rather than Close) ensures the handler is done before we return.
            (~res).Body.Close();
            (~(~cst).ts).Config.Shutdown(context_package.Background());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that the HTTP/2 server handles Server.WriteTimeout (Issue 18437)
public static void TestWriteDeadlineExtendedOnNewRequest(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testWriteDeadlineExtendedOnNewRequest(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

internal static void testWriteDeadlineExtendedOnNewRequest(ж<testing.T> Ꮡt, testMode mode) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter res, ж<Δhttp.Request> req) => {
    })),
        (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.WriteTimeout = 250 * time.Millisecond;
        }).Value.ts;
    var c = ts.Client();
    for (nint i = 1; i <= 3; i++) {
        var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var r, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("http2 Get #%d: %v"u8, i, err);
        }
        (~r).Body.Close();
        time.Sleep((~(~ts).Config).WriteTimeout / 2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object allAttemptsFailedˢ = (@string)"all attempts failed"u8;

// tryTimeouts runs testFunc with increasing timeouts. Test passes on first success,
// and fails if all timeouts fail.
internal static void tryTimeouts(ж<testing.T> Ꮡt, Func<time.Duration, error> testFunc) {
    var tries = new time.Duration[]{250 * time.Millisecond, 500 * time.Millisecond, 1 * time.ΔSecond}.slice();
    foreach (var (i, timeout) in tries) {
        var err = testFunc(timeout);
        if (err == default!) {
            return;
        }
        Ꮡt.Logf("failed at %v: %v"u8, timeout, err);
        if (i != len(tries) - 1) {
            Ꮡt.Logf("retrying at %v ..."u8, tries[i + 1]);
        }
    }
    Ꮡt.Fatal(allAttemptsFailedˢ);
}

// Test that the HTTP/2 server RSTs stream on slow write.
public static void TestWriteDeadlineEnforcedPerStream(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    setParallel(Ꮡt);
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tryTimeouts(tΔ1, (time.Duration timeout) => testWriteDeadlineEnforcedPerStream(tΔ1, mode, timeout));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string streamId3InternalErrorˢ = "stream ID 3; INTERNAL_ERROR"u8;

internal static error testWriteDeadlineEnforcedPerStream(ж<testing.T> Ꮡt, testMode mode, time.Duration timeout) {
    GoFrame ᒐ = default;
    try {
        var firstRequest = new channel<bool>(1);
        var firstRequestʗ1 = firstRequest;

        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter res, ж<Δhttp.Request> reqΔ1) => {
            var selᴛ38 = firstRequestʗ1.ᐸꟷ(true, ꓸꓸꓸ);
            switch (trySelect(selᴛ38)) {
            case 0: {
                break;
            }
            default: {
                time.Sleep(timeout);
                break;
            }}
        })), // first request succeeds
 // second request times out
 (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.WriteTimeout = timeout / 2;
        });
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var ts = cst.Value.ts;
        var c = ts.Client();
        var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
        if (err != default!) {
            return fmt.Errorf("NewRequest: %v"u8, err);
        }
        (var r, err) = c.Do(req);
        if (err != default!) {
            return fmt.Errorf("Get #1: %v"u8, err);
        }
        (~r).Body.Close();
        (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
        if (err != default!) {
            return fmt.Errorf("NewRequest: %v"u8, err);
        }
        (r, err) = c.Do(req);
        if (err == default!) {
            (~r).Body.Close();
            return fmt.Errorf("Get #2 expected error, got nil"u8);
        }
        if (mode == http2Mode) {
            @string expected = streamId3InternalErrorˢ; // client IDs are odd, second stream should be 3
            if (!strings.Contains(err.Error(), expected)) {
                return fmt.Errorf("http2 Get #2: expected error to contain %q, got %q"u8, expected, err);
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Test that the HTTP/2 server does not send RST when WriteDeadline not set.
public static void TestNoWriteDeadline(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
            var tΔ1 = (ж<testing.T>)tΔ1Δp;
            tryTimeouts(tΔ1, (time.Duration timeout) => testNoWriteDeadline(tΔ1, mode, timeout));
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error testNoWriteDeadline(ж<testing.T> Ꮡt, testMode mode, time.Duration timeout) {
    GoFrame ᒐ = default;
    try {
        var firstRequest = new channel<bool>(1);
        var firstRequestʗ1 = firstRequest;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter res, ж<Δhttp.Request> req) => {
            var selᴛ39 = firstRequestʗ1.ᐸꟷ(true, ꓸꓸꓸ);
            switch (trySelect(selᴛ39)) {
            case 0: {
                break;
            }
            default: {
                time.Sleep(timeout);
                break;
            }}
        })));
        // first request succeeds
        // second request times out
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var ts = cst.Value.ts;
        var c = ts.Client();
        for (nint i = 0; i < 2; i++) {
            var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
            if (err != default!) {
                return fmt.Errorf("NewRequest: %v"u8, err);
            }
            (var r, err) = c.Do(req);
            if (err != default!) {
                return fmt.Errorf("Get #%d: %v"u8, i, err);
            }
            (~r).Body.Close();
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// golang.org/issue/4741 -- setting only a write timeout that triggers
// shouldn't cause a handler to block forever on reads (next HTTP
// request) that will never happen.
public static void TestOnlyWriteTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testOnlyWriteTimeout(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noEstablishedConnectionˢ = (@string)"no established connection found"u8;
internal static readonly object expectedWriteErrorAfterˢ = (@string)"expected write error after timeout"u8;

internal static void testOnlyWriteTimeout(ж<testing.T> Ꮡt, testMode mode) {
    ref var mu = ref heap(new sync.RWMutex(), out var Ꮡmu);
    ref var conn = ref heap<net.Conn>(out var Ꮡconn);
    channel<error> afterTimeoutErrc = new channel<error>(1);
    var afterTimeoutErrcʗ1 = afterTimeoutErrc;

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        GoFrame ᒐ = default;
        try {
            var buf = new slice<byte>((512 << (int)(10)));
            var (_, errΔ1) = w.Write(buf);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("handler Write error: %v"u8, errΔ1);
                return;
            }
            Ꮡmu.RLock();
            defer(Ꮡmu.RUnlock, ref ᒐ);
            if (Ꮡconn.ValueSlot == default!) {
                Ꮡt.Error(noEstablishedConnectionˢ);
                return;
            }
            Ꮡconn.ValueSlot.SetWriteDeadline(time.Now().Add((time.Duration)(-30000000000L)));
            (_, errΔ1) = w.Write(buf);
            afterTimeoutErrcʗ1.ᐸꟷ(errΔ1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })), (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Listener = new trackLastConnListener((~tsΔ1).Listener, Ꮡmu, Ꮡconn);
    }).Value.ts;
    var c = ts.Client();
    var cʗ1 = c;
    var tsʗ1 = ts;
    var err = ((Func<error>)(() => {
        var (res, errΔ2) = cʗ1.Get((~tsʗ1).URL);
        if (errΔ2 != default!) {
            return errΔ2;
        }
        (_, errΔ2) = io.Copy(io.Discard, (~res).Body);
        (~res).Body.Close();
        return errΔ2;
    }))();
    if (err == default!) {
        Ꮡt.Errorf("expected an error copying body from Get request"u8);
    }
    {
        var errΔ3 = ᐸꟷ(afterTimeoutErrc); if (errΔ3 == default!) {
            Ꮡt.Error(expectedWriteErrorAfterˢ);
        }
    }
}

// trackLastConnListener tracks the last net.Conn that was accepted.
[GoType] partial struct trackLastConnListener {
    [GoEmbedded] public net_package.Listener Listener;
    internal ж<sync.RWMutex> mu;
    internal ж<net.Conn> last; // destination
}

internal static (net.Conn c, error err) Accept(this trackLastConnListener l) {
    net.Conn c = default!;
    error err = default!;

    (c, err) = l.Listener.Accept();
    if (err == default!) {
        l.mu.Lock();
        l.last.ValueSlot = c;
        l.mu.Unlock();
    }
    return (c, err);
}

// TestIdentityResponse verifies that a handler can unset
public static void TestIdentityResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testIdentityResponse(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue56019ˢ = (@string)"https://go.dev/issue/56019"u8;
internal static readonly @string overwriteˢ = "overwrite"u8;
internal static readonly @string underwriteˢ = "underwrite"u8;
internal static readonly @string tooShortˢ = "\r\n\r\ntoo short"u8;

internal static void testIdentityResponse(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (mode == http2Mode) {
        Ꮡt.Skip(httpsGoDevIssue56019ˢ);
    }
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        rw.Header().Set(contentLengthˢ, "3"u8);
        rw.Header().Set(transferEncodingˢ, req.FormValue("te"u8));
        switch (ᐧ) {
        case {} when req.FormValue(overwriteˢ) == "1"u8: {
            var (_, errΔ2) = rw.Write(slice<byte>("foo TOO LONG"u8));
            if (!AreEqual(errΔ2, ErrContentLength)) {
                Ꮡt.Errorf("expected ErrContentLength; got %v"u8, errΔ2);
            }
            break;
        }
        case {} when req.FormValue(underwriteˢ) == "1"u8: {
            rw.Header().Set(contentLengthˢ, "500"u8);
            rw.Write(slice<byte>("too short"u8));
            break;
        }
        default: {
            rw.Write(slice<byte>("foo"u8));
            break;
        }}

    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(handler)).Value.ts;
    var c = ts.Client();
    // Note: this relies on the assumption (which is true) that
    // Get sends HTTP/1.1 or greater requests. Otherwise the
    // server wouldn't have the choice to send back chunked
    // responses.
    foreach (var (_, te) in new @string[]{""u8, "identity"u8}.slice()) {
        @string urlΔ1 = (~ts).URL + "/?te="u8 + te;
        var (resΔ1, errΔ3) = c.Get(urlΔ1);
        if (errΔ3 != default!) {
            Ꮡt.Fatalf("error with Get of %s: %v"u8, urlΔ1, errΔ3);
        }
        {
            var (cl, expected) = (resΔ1.Value.ContentLength, (int64)3); if (cl != expected) {
                Ꮡt.Errorf("for %s expected res.ContentLength of %d; got %d"u8, urlΔ1, expected, cl);
            }
        }
        {
            @string cl = (~resΔ1).Header.Get(contentLengthˢ);
            @string expected = "3"u8; if (cl != expected) {
                Ꮡt.Errorf("for %s expected Content-Length header of %q; got %q"u8, urlΔ1, expected, cl);
            }
        }
        {
            nint tl = len((~resΔ1).TransferEncoding);
            nint expected = 0; if (tl != expected) {
                Ꮡt.Errorf("for %s expected len(res.TransferEncoding) of %d; got %d (%v)"u8,
                    urlΔ1, expected, tl, (~resΔ1).TransferEncoding);
            }
        }
        (~resΔ1).Body.Close();
    }
    // Verify that ErrContentLength is returned
    @string url = (~ts).URL + "/?overwrite=1"u8;
    var (res, err) = c.Get(url);
    if (err != default!) {
        Ꮡt.Fatalf("error with Get of %s: %v"u8, url, err);
    }
    (~res).Body.Close();
    if (mode != http1Mode) {
        return;
    }
    // Verify that the connection is closed when the declared Content-Length
    // is larger than what the handler wrote.
    (var conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
    if (err != default!) {
        Ꮡt.Fatalf("error dialing: %v"u8, err);
    }
    (_, err) = conn.Write(slice<byte>("GET /?underwrite=1 HTTP/1.1\r\nHost: foo\r\n\r\n"u8));
    if (err != default!) {
        Ꮡt.Fatalf("error writing: %v"u8, err);
    }
    // The ReadAll will hang for a failing test.
    var (got, _) = io.ReadAll(new http_test_package.net_ConnᴠReader(conn));
    @string expectedSuffix = tooShortˢ;
    if (!strings.HasSuffix(((@string)got), expectedSuffix)) {
        Ꮡt.Errorf("Expected output to end with %q; got response body %q"u8,
            expectedSuffix, ((@string)got));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object dialErrorˢ = (@string)"dial error:"u8;
internal static readonly object printErrorˢ = (@string)"print error:"u8;
internal static readonly object readResponseErrorˢ = (@string)"ReadResponse error:"u8;
internal static readonly object readErrorˢ = (@string)"read error:"u8;

internal static void testTCPConnectionCloses(ж<testing.T> Ꮡt, @string req, httpꓸHandler h) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        var s = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http1Mode, h).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~s).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(dialErrorˢ, err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        (_, err) = fmt.Fprint(new http_test_package.net_ConnᴠWriter(conn), req);
        if (err != default!) {
            Ꮡt.Fatal(printErrorˢ, err);
        }
        var r = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
        (var res, err) = ReadResponse(r, Ꮡ(new Request(Method: "GET"u8)));
        if (err != default!) {
            Ꮡt.Fatal(readResponseErrorˢ, err);
        }
        (_, err) = io.ReadAll(new http_test_package.bufio_ReaderжReader(r));
        if (err != default!) {
            Ꮡt.Fatal(readErrorˢ, err);
        }
        if (!(~res).Close) {
            Ꮡt.Errorf("Response.Close = false; want true"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testTCPConnectionStaysOpen(ж<testing.T> Ꮡt, @string req, httpꓸHandler handler) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), http1Mode, handler).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var br = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
        for (nint i = 0; i < 2; i++) {
            {
                var (_, errΔ1) = io.WriteString(new http_test_package.net_ConnᴠWriter(conn), req); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            var (res, errΔ2) = ReadResponse(br, nil);
            if (errΔ2 != default!) {
                Ꮡt.Fatalf("res %d: %v"u8, i + 1, errΔ2);
            }
            {
                var (_, errΔ3) = io.Copy(io.Discard, (~res).Body); if (errΔ3 != default!) {
                    Ꮡt.Fatalf("res %d body copy: %v"u8, i + 1, errΔ3);
                }
            }
            (~res).Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10ˢ = "GET / HTTP/1.0\r\n\r\n"u8;

// TestServeHTTP10Close verifies that HTTP/1.0 requests won't be kept alive.
public static void TestServeHTTP10Close(ж<testing.T> Ꮡt) {
    testTCPConnectionCloses(Ꮡt, getHttp10ˢ, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ServeFile(w, r, testdataFileˢ);
    })));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostFooˢ = "GET / HTTP/1.1\r\nHost: foo\r\nConnection: close\r\n\r\n"u8;

// TestClientCanClose verifies that clients can also force a connection to close.
public static void TestClientCanClose(ж<testing.T> Ꮡt) {
    testTCPConnectionCloses(Ꮡt, getHttp11HostFooˢ, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostFooˢ2 = "GET / HTTP/1.1\r\nHost: foo\r\n\r\n\r\n"u8;
internal static readonly @string closeˢ = "close"u8;

// Nothing.

// TestHandlersCanSetConnectionClose verifies that handlers can force a connection to close,
// even for HTTP/1.1 requests.
public static void TestHandlersCanSetConnectionClose11(ж<testing.T> Ꮡt) {
    testTCPConnectionCloses(Ꮡt, getHttp11HostFooˢ2, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(connectionˢ, closeˢ);
    })));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10ConnectionKeepˢ = "GET / HTTP/1.0\r\nConnection: keep-alive\r\n\r\n"u8;

public static void TestHandlersCanSetConnectionClose10(ж<testing.T> Ꮡt) {
    testTCPConnectionCloses(Ꮡt, getHttp10ConnectionKeepˢ, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(connectionˢ, closeˢ);
    })));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string priHttp20Smˢ = "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8;

public static void TestHTTP2UpgradeClosesConnection(ж<testing.T> Ꮡt) {
    testTCPConnectionCloses(Ꮡt, priHttp20Smˢ, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })));
}

// Nothing. (if not hijacked, the server should close the connection
// afterwards)
internal static void send204(Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡr) {
    w.WriteHeader(204);
}

internal static void send304(Δhttp.ResponseWriter w, ж<Δhttp.Request> Ꮡr) {
    w.WriteHeader(304);
}

// Issue 15647: 204 responses can't have bodies, so HTTP/1.0 keep-alive conns should stay open.
public static void TestHTTP10KeepAlive204Response(ж<testing.T> Ꮡt) {
    testTCPConnectionStaysOpen(Ꮡt, getHttp10ConnectionKeepˢ, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc(send204)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostFooˢ3 = "GET / HTTP/1.1\r\nHost: foo\r\n\r\n"u8;

public static void TestHTTP11KeepAlive204Response(ж<testing.T> Ꮡt) {
    testTCPConnectionStaysOpen(Ꮡt, getHttp11HostFooˢ3, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc(send204)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10ConnectionKeepˢ2 = "GET / HTTP/1.0\r\nConnection: keep-alive\r\nIf-Modified-Since: Mon, 02 Jan 2006 15:04:05 GMT\r\n\r\n"u8;

public static void TestHTTP10KeepAlive304Response(ж<testing.T> Ꮡt) {
    testTCPConnectionStaysOpen(Ꮡt,
        getHttp10ConnectionKeepˢ2,
        new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc(send304)));
}

// Issue 15703
public static void TestKeepAliveFinalChunkWithEOF(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testKeepAliveFinalChunkWithEOF(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noAddressˢ = (@string)"no address"u8;

[GoType("dyn")] internal partial struct testKeepAliveFinalChunkWithEOF_data {
    public @string Addr;
}

internal static void testKeepAliveFinalChunkWithEOF(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w._<Flusher>().Flush(); // force chunked encoding
        w.Write(slice<byte>("{\"Addr\": \"" + (~r).RemoteAddr + "\"}"));
    })));
    ref var addrs = ref heap(new array<testKeepAliveFinalChunkWithEOF_data>(2), out var Ꮡaddrs);
    foreach (var (i, _) in addrs) {
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var errΔ1 = json.NewDecoder((~res).Body).Decode(Ꮡaddrs.at<testKeepAliveFinalChunkWithEOF_data>(i)); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        if (addrs[i].Addr == ""u8) {
            Ꮡt.Fatal(noAddressˢ);
        }
        (~res).Body.Close();
    }
    if (addrs[0] != addrs[1]) {
        Ꮡt.Fatalf("connection not reused"u8);
    }
}

public static void TestSetsRemoteAddr(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testSetsRemoteAddr(Δp0, Δp1));
}

internal static void testSetsRemoteAddr(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%s"u8, (~r).RemoteAddr);
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatalf("Get error: %v"u8, err);
    }
    (var body, err) = io.ReadAll((~res).Body);
    if (err != default!) {
        Ꮡt.Fatalf("ReadAll error: %v"u8, err);
    }
    @string ip = ((@string)body);
    if (!strings.HasPrefix(ip, "127.0.0.1:"u8) && !strings.HasPrefix(ip, "[::1]:"u8)) {
        Ꮡt.Fatalf("Expected local addr; got %q"u8, ip);
    }
}

[GoType] partial struct blockingRemoteAddrListener {
    [GoEmbedded] public net_package.Listener Listener;
    internal channel/*<-*/<net.Conn> conns = channel/*<-*/<net.Conn>.SendOnly;
}

// Go method set entry for the promoted 'Listener.Addr()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrListener's method set; see the pointer-only satisfaction record.
internal static netꓸAddr Addr(this blockingRemoteAddrListener recvᴛ) => recvᴛ.Listener.Addr();

// Go method set entry for the promoted 'Listener.Close()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrListener's method set; see the pointer-only satisfaction record.
internal static error Close(this blockingRemoteAddrListener recvᴛ) => recvᴛ.Listener.Close();

[GoRecv] internal static (net.Conn, error) Accept(this ref blockingRemoteAddrListener l) {
    var (c, err) = l.Listener.Accept();
    if (err != default!) {
        return (default!, err);
    }
    var brac = Ꮡ(new blockingRemoteAddrConn(
        Conn: c,
        addrs: new channel<netꓸAddr>(1)
    ));
    l.conns.ᐸꟷ(new http_test_package.blockingRemoteAddrConnжConn(brac));
    return (new http_test_package.blockingRemoteAddrConnжConn(brac), default!);
}

[GoType] partial struct blockingRemoteAddrConn {
    [GoEmbedded] public net_package.Conn Conn;
    internal channel<netꓸAddr> addrs;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static error Close(this blockingRemoteAddrConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this blockingRemoteAddrConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this blockingRemoteAddrConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this blockingRemoteAddrConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this blockingRemoteAddrConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this blockingRemoteAddrConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

// Go method set entry for the promoted 'Conn.Write()' - provided ONLY by the embedded
// interface field in *blockingRemoteAddrConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Write(this blockingRemoteAddrConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Write(b);

[GoRecv] internal static netꓸAddr RemoteAddr(this ref blockingRemoteAddrConn c) {
    return ᐸꟷ(c.addrs);
}

// Issue 12943
public static void TestServerAllowsBlockingRemoteAddr(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerAllowsBlockingRemoteAddr(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ra1212121212ˢ = "RA:12.12.12.12:12"u8;
internal static readonly @string ra2121212121ˢ = "RA:21.21.21.21:21"u8;

internal static void testServerAllowsBlockingRemoteAddr(ж<testing.T> Ꮡt, testMode mode) {
    var conns = new channel<net.Conn>(0);
    var connsʗ1 = conns;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "RA:%s"u8, (~r).RemoteAddr);
    })), (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Listener = new http_test_package.blockingRemoteAddrListenerжListener(Ꮡ(new blockingRemoteAddrListener(
            Listener: (~tsΔ1).Listener,
            conns: connsʗ1
        )));
    }).Value.ts;
    var c = ts.Client();
    // Force separate connection for each:
    (~c).Transport._<ж<Δhttp.Transport>>().Value.DisableKeepAlives = true;
    var cʗ1 = c;
    var tsʗ1 = ts;
    void fetch(nint num, channel/*<-*/<@string> response) {
        GoFrame ᒐ = default;
        try {
            var (resp, err) = cʗ1.Get((~tsʗ1).URL);
            if (err != default!) {
                Ꮡt.Errorf("Request %d: %v"u8, num, err);
                response.ᐸꟷ(""u8);
                return;
            }
            var respʗ1 = resp;
            defer(() => (~respʗ1).Body.Close(), ref ᒐ);
            (var body, err) = io.ReadAll((~resp).Body);
            if (err != default!) {
                Ꮡt.Errorf("Request %d: %v"u8, num, err);
                response.ᐸꟷ(""u8);
                return;
            }
            response.ᐸꟷ(((@string)body));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    // Start a request. The server will block on getting conn.RemoteAddr.
    var response1c = new channel<@string>(1);
    var fetchʗ1 = fetch;
    goǃ(fetchʗ1, (nint)(1), response1c.WithDirection(GoChanDir.Send));
    // Wait for the server to accept it; grab the connection.
    var conn1 = ᐸꟷ(conns);
    // Start another request and grab its connection
    var response2c = new channel<@string>(1);
    var fetchʗ2 = fetch;
    goǃ(fetchʗ2, (nint)(2), response2c.WithDirection(GoChanDir.Send));
    var conn2 = ᐸꟷ(conns);
    // Send a response on connection 2.
    (~conn2._<ж<blockingRemoteAddrConn>>()).addrs.ᐸꟷ(new net.TCPAddrжΔAddr(Ꮡ(new net.TCPAddr(
        IP: net.ParseIP("12.12.12.12"u8), Port: 12))));
    // ... and see it
    @string response2 = ᐸꟷ(response2c);
    {
        @string g = response2;
        @string e = ra1212121212ˢ; if (g != e) {
            Ꮡt.Fatalf("response 2 addr = %q; want %q"u8, g, e);
        }
    }
    // Finish the first response.
    (~conn1._<ж<blockingRemoteAddrConn>>()).addrs.ᐸꟷ(new net.TCPAddrжΔAddr(Ꮡ(new net.TCPAddr(
        IP: net.ParseIP("21.21.21.21"u8), Port: 21))));
    // ... and see it
    @string response1 = ᐸꟷ(response1c);
    {
        @string g = response1;
        @string e = ra2121212121ˢ; if (g != e) {
            Ꮡt.Fatalf("response 1 addr = %q; want %q"u8, g, e);
        }
    }
}

// TestHeadResponses verifies that all MIME type sniffing and Content-Length
// counting of GET requests also happens on HEAD requests.
public static void TestHeadResponses(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHeadResponses(Δp0, Δp1));
}

[GoType("dyn")] internal partial struct testHeadResponses_src {
    [GoEmbedded] public io_package.Reader Reader;
}

internal static void testHeadResponses(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (_, errΔ1) = w.Write(slice<byte>("<html>"u8));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("ResponseWriter.Write: %v"u8, errΔ1);
        }
        // Also exercise the ReaderFrom path
        (_, errΔ1) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), new testHeadResponses_src(new http_test_package.strings_ReaderжReader(strings.NewReader("789a"u8))));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("Copy(ResponseWriter, ...): %v"u8, errΔ1);
        }
    })));
    var (res, err) = (~cst).c.Head((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (len((~res).TransferEncoding) > 0) {
        Ꮡt.Errorf("expected no TransferEncoding; got %v"u8, (~res).TransferEncoding);
    }
    {
        @string ct = (~res).Header.Get(contentTypeˢ); if (ct != "text/html; charset=utf-8"u8) {
            Ꮡt.Errorf("Content-Type: %q; want text/html; charset=utf-8"u8, ct);
        }
    }
    {
        var v = res.Value.ContentLength; if (v != 10) {
            Ꮡt.Errorf("Content-Length: %d; want 10"u8, v);
        }
    }
    (var body, err) = io.ReadAll((~res).Body);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (len(body) > 0) {
        Ꮡt.Errorf("got unexpected body %q"u8, ((@string)body));
    }
}

// Ensure ResponseWriter.ReadFrom doesn't write a body in response to a HEAD request.
// https://go.dev/issue/68609
public static void TestHeadReaderFrom(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHeadReaderFrom(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testHeadReaderFrom(ж<testing.T> Ꮡt, testMode mode) {
    // Body is large enough to exceed the content-sniffing length.
    @string wantBody = strings.Repeat("a"u8, 4096);
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w._<io.ReaderFrom>().ReadFrom(new http_test_package.strings_ReaderжReader(strings.NewReader(wantBody)));
    })));
    var (res, err) = (~cst).c.Head((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var gotBody, err) = io.ReadAll((~res).Body);
    (~res).Body.Close();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((sstring)gotBody) != wantBody) {
        Ꮡt.Errorf("got unexpected body len=%v, want %v"u8, len(gotBody), len(wantBody));
    }
}

public static void TestTLSHandshakeTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTLSHandshakeTimeout(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsHandshakeˢ = "TLS handshake"u8;

internal static void testTLSHandshakeTimeout(ж<testing.T> Ꮡt, testMode mode) {
    var errLog = @new<strings.Builder>();
        var errLogʗ1 = errLog;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })),
        (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.ReadTimeout = 250 * time.Millisecond;
            tsΔ1.Value.Config.Value.ErrorLog = log.New(new http_test_package.strings_BuilderжWriter(errLogʗ1), ""u8, 0);
        });
    var ts = cst.Value.ts;
    var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
    if (err != default!) {
        Ꮡt.Fatalf("Dial: %v"u8, err);
    }
    array<byte> buf = new(1);
    (var n, err) = conn.Read(buf[..]);
    if (err == default! || n != 0) {
        Ꮡt.Errorf("Read = %d, %v; want an error and no bytes"u8, n, err);
    }
    conn.Close();
    cst.close();
    {
        @string v = errLog.String(); if (!strings.Contains(v, timeoutˢ) && !strings.Contains(v, tlsHandshakeˢ)) {
            Ꮡt.Errorf("expected a TLS handshake timeout error; got %q"u8, v);
        }
    }
}

public static void TestTLSServer(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTLSServer(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xTlsSetˢ = "X-TLS-Set"u8;
internal static readonly @string xTlsHandshakeCompleteˢ = "X-TLS-HandshakeComplete"u8;

internal static void testTLSServer(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~r).TLS != nil) {
                w.Header().Set(xTlsSetˢ, trueˢ);
                if ((~(~r).TLS).HandshakeComplete) {
                    w.Header().Set(xTlsHandshakeCompleteˢ, trueˢ);
                }
            }
        })), (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        }).Value.ts;
        // Connect an idle TCP connection to this server before we run
        // our real tests. This idle connection used to block forever
        // in the TLS handshake, preventing future connections from
        // being accepted. It may prevent future accidental blocking
        // in newConn.
        var (idleConn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("Dial: %v"u8, err);
        }
        var idleConnʗ1 = idleConn;
        defer(() => idleConnʗ1.Close(), ref ᒐ);
        if (!strings.HasPrefix((~ts).URL, httpsˢ2)) {
            Ꮡt.Errorf("expected test TLS server to start with https://, got %q"u8, (~ts).URL);
            return;
        }
        var client = ts.Client();
        (var res, err) = client.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        if (res == nil) {
            Ꮡt.Errorf("got nil Response"u8);
            return;
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).Header.Get(xTlsSetˢ) != "true"u8) {
            Ꮡt.Errorf("expected X-TLS-Set response header"u8);
            return;
        }
        if ((~res).Header.Get(xTlsHandshakeCompleteˢ) != "true"u8) {
            Ꮡt.Errorf("expected X-TLS-HandshakeComplete header"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServeTLS(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
        // Not parallel: uses global test hooks.
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        defer(http_internal_test_package.SetTestHookServerServe, (Action<ж<Δhttp.Server>, net.Listener>)(default!), ref ᒐ);
        var (cert, err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var tlsConf = Ꮡ(new tls.Config(
            Certificates: new tls.Certificate[]{cert}.slice()
        ));
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        ref var addr = ref heap<@string>(out var Ꮡaddr);
        addr = ln.Addr().String();
        var serving = new channel<bool>(1);
        var servingʗ1 = serving;
        http_internal_test_package.SetTestHookServerServe((ж<Δhttp.Server> sΔ1, net.Listener lnΔ1) => {
            servingʗ1.ᐸꟷ(true);
        });
        var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        });
        var s = Ꮡ(new Server(
            Addr: addr,
            TLSConfig: tlsConf,
            Handler: new http_test_package.http_HandlerFuncᴠΔHandler(handler)
        ));
        var errc = new channel<error>(1);
        var errcʗ1 = errc;
        var lnʗ2 = ln;
        var sʗ1 = s;
        goǃ(() => {
            errcʗ1.ᐸꟷ(sʗ1.ServeTLS(lnʗ2, ""u8, ""u8));
        });
        var selᴛ40 = errc;
        var selᴛ41 = serving;
        switch (select(ᐸꟷ(selᴛ40, ꓸꓸꓸ), ᐸꟷ(selᴛ41, ꓸꓸꓸ))) {
        case 0 when selᴛ40.ꟷᐳ(out var errΔ1): {
            Ꮡt.Fatalf("ServeTLS: %v"u8, errΔ1);
            break;
        }
        case 1 when selᴛ41.ꟷᐳ(out _): {
            break;
        }}
        (var c, err) = tls.Dial(tcpˢ, ln.Addr().String(), Ꮡ(new tls.Config(
            InsecureSkipVerify: true,
            NextProtos: new @string[]{"h2"u8, "http/1.1"u8}.slice()
        )));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            @string got = c.ConnectionState().NegotiatedProtocol;
            @string want = "h2"u8; if (got != want) {
                Ꮡt.Errorf("NegotiatedProtocol = %q; want %q"u8, got, want);
            }
        }
        {
            var (got, want) = (c.ConnectionState().NegotiatedProtocolIsMutual, true); if (got != want) {
                Ꮡt.Errorf("NegotiatedProtocolIsMutual = %v; want %v"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that the HTTPS server nicely rejects plaintext HTTP/1.x requests.
public static void TestTLSServerRejectHTTPRequests(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTLSServerRejectHTTPRequests(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedHttpsRequestˢ = (@string)"unexpected HTTPS request"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantPrefixᶜ = "HTTP/1.0 400 Bad Request\r\n"u8;

internal static void testTLSServerRejectHTTPRequests(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            Ꮡt.Error(unexpectedHttpsRequestˢ);
        })), (ж<httptest.Server> tsΔ1) => {
            ref var errBuf = ref heap(new bytes.Buffer(), out var ᏑerrBuf);
            tsΔ1.Value.Config.Value.ErrorLog = log.New(new http_test_package.bytes_BufferжWriter(ᏑerrBuf), ""u8, 0);
        }).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        io.WriteString(new http_test_package.net_ConnᴠWriter(conn), getHttp11HostFooˢ3);
        (var slurp, err) = io.ReadAll(new http_test_package.net_ConnᴠReader(conn));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string wantPrefix = wantPrefixᶜ;
        if (!strings.HasPrefix(((@string)slurp), wantPrefix)) {
            Ꮡt.Errorf("response = %q; wanted prefix %q"u8, slurp, wantPrefix);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 15908
public static void TestAutomaticHTTP2_Serve_NoTLSConfig(ж<testing.T> Ꮡt) {
    testAutomaticHTTP2_Serve(Ꮡt, nil, true);
}

public static void TestAutomaticHTTP2_Serve_NonH2TLSConfig(ж<testing.T> Ꮡt) {
    testAutomaticHTTP2_Serve(Ꮡt, Ꮡ(new tls.Config(nil)), false);
}

public static void TestAutomaticHTTP2_Serve_H2TLSConfig(ж<testing.T> Ꮡt) {
    testAutomaticHTTP2_Serve(Ꮡt, Ꮡ(new tls.Config(NextProtos: new @string[]{"h2"u8}.slice())), true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAnErrorˢ = (@string)"expected an error"u8;

internal static void testAutomaticHTTP2_Serve(ж<testing.T> Ꮡt, ж<tls.Config> ᏑtlsConf, bool wantH2) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var ln = newLocalListener(Ꮡt);
        ln.Close(); // immediately (not a defer!)
        ref var s = ref heap(new Δhttp.Server(), out var Ꮡs);
        s.TLSConfig = ᏑtlsConf;
        {
            var err = Ꮡs.Serve(ln); if (err == default!) {
                Ꮡt.Fatal(expectedAnErrorˢ);
            }
        }
        var gotH2 = s.TLSNextProto["h2"u8] != default!;
        if (gotH2 != wantH2) {
            Ꮡt.Errorf("http2 configured = %v; want %v"u8, gotH2, wantH2);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAutomaticHTTP2_Serve_WithTLSConfig(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var ln = newLocalListener(Ꮡt);
        ln.Close(); // immediately (not a defer!)
        ref var s = ref heap(new Δhttp.Server(), out var Ꮡs);
        // Set the TLSConfig. In reality, this would be the
        // *tls.Config given to tls.NewListener.
        s.TLSConfig = Ꮡ(new tls.Config(
            NextProtos: new @string[]{"h2"u8}.slice()
        ));
        {
            var err = Ꮡs.Serve(ln); if (err == default!) {
                Ꮡt.Fatal(expectedAnErrorˢ);
            }
        }
        var on = s.TLSNextProto["h2"u8] != default!;
        if (!on) {
            Ꮡt.Errorf("http2 wasn't automatically enabled"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAutomaticHTTP2_ListenAndServe(ж<testing.T> Ꮡt) {
    var (cert, err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    testAutomaticHTTP2_ListenAndServe(Ꮡt, Ꮡ(new tls.Config(
        Certificates: new tls.Certificate[]{cert}.slice()
    )));
}

public static void TestAutomaticHTTP2_ListenAndServe_GetCertificate(ж<testing.T> Ꮡt) {
    ref var cert = ref heap<tls.Certificate>(out var Ꮡcert);
    (cert, var err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    testAutomaticHTTP2_ListenAndServe(Ꮡt, Ꮡ(new tls.Config(
        GetCertificate: (ж<tls.ClientHelloInfo> clientHello) => (Ꮡcert, default!)
    )));
}

public static void TestAutomaticHTTP2_ListenAndServe_GetConfigForClient(ж<testing.T> Ꮡt) {
    var (cert, err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var conf = Ꮡ(new tls.Config( // GetConfigForClient requires specifying a full tls.Config so we must set
 // NextProtos ourselves.

        NextProtos: new @string[]{"h2"u8}.slice(),
        Certificates: new tls.Certificate[]{cert}.slice()
    ));
        var confʗ1 = conf;
    testAutomaticHTTP2_ListenAndServe(Ꮡt, Ꮡ(new tls.Config(
        GetConfigForClient: (ж<tls.ClientHelloInfo> clientHello) => (confʗ1, default!)
    )));
}

internal static void testAutomaticHTTP2_ListenAndServe(ж<testing.T> Ꮡt, ж<tls.Config> ᏑtlsConf) {
    GoFrame ᒐ = default;
    try {
        http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
        // Not parallel: uses global test hooks.
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        defer(http_internal_test_package.SetTestHookServerServe, (Action<ж<Δhttp.Server>, net.Listener>)(default!), ref ᒐ);
        bool ok = default!;
        ref var s = ref heap<ж<Δhttp.Server>>(out var Ꮡs);
        const nint maxTries = 5;
        net.Listener ln = default!;
Try:
        for (nint @try = 0; @try < maxTries; @try++) {
            ln = newLocalListener(Ꮡt);
            ref var addr = ref heap<@string>(out var Ꮡaddr);
            addr = ln.Addr().String();
            ln.Close();
            Ꮡt.Logf("Got %v"u8, addr);
            var lnc = new channel<net.Listener>(1);
            var lncʗ1 = lnc;
            http_internal_test_package.SetTestHookServerServe((ж<Δhttp.Server> sΔ1, net.Listener lnΔ1) => {
                lncʗ1.ᐸꟷ(lnΔ1);
            });
            s = Ꮡ(new Server(
                Addr: addr,
                TLSConfig: ᏑtlsConf
            ));
            var errc = new channel<error>(1);
            var errcʗ1 = errc;
            goǃ(() => {
                errcʗ1.ᐸꟷ(Ꮡs.ValueSlot.ListenAndServeTLS(""u8, ""u8));
            });
            var selᴛ42 = errc;
            var selᴛ43 = lnc;
            switch (select(ᐸꟷ(selᴛ42, ꓸꓸꓸ), ᐸꟷ(selᴛ43, ꓸꓸꓸ))) {
            case 0 when selᴛ42.ꟷᐳ(out var errΔ1): {
                Ꮡt.Logf("On try #%v: %v"u8, @try + 1, errΔ1);
                continue;
                break;
            }
            case 1 when selᴛ43.ꟷᐳ(out ln): {
                ok = true;
                Ꮡt.Logf("Listening on %v"u8, ln.Addr().String());
                goto break_Try;
                break;
            }}
continue_Try:;
        }
break_Try:;
        if (!ok) {
            Ꮡt.Fatalf("Failed to start up after %d tries"u8, (nint)(maxTries));
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var (c, err) = tls.Dial(tcpˢ, ln.Addr().String(), Ꮡ(new tls.Config(
            InsecureSkipVerify: true,
            NextProtos: new @string[]{"h2"u8, "http/1.1"u8}.slice()
        )));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            @string got = c.ConnectionState().NegotiatedProtocol;
            @string want = "h2"u8; if (got != want) {
                Ꮡt.Errorf("NegotiatedProtocol = %q; want %q"u8, got, want);
            }
        }
        {
            var (got, want) = (c.ConnectionState().NegotiatedProtocolIsMutual, true); if (got != want) {
                Ꮡt.Errorf("NegotiatedProtocolIsMutual = %v; want %v"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct serverExpectTest {
    internal nint contentLength; // of request body
    internal bool chunked;
    internal @string expectation; // e.g. "100-continue"
    internal bool readBody;   // whether handler should read the body (if false, sends StatusUnauthorized)
    internal @string expectedResponse; // expected substring in first line of http response
}

internal static serverExpectTest expectTest(nint contentLength, @string expectation, bool readBody, @string expectedResponse) {
    return new serverExpectTest(
        contentLength: contentLength,
        expectation: expectation,
        readBody: readBody,
        expectedResponse: expectedResponse
    );
}

// Normal 100-continues, case-insensitive.
// No 100-continue.
// 100-continue but requesting client to deny us,
// so it never reads the body.
// Likewise without 100-continue:
// Non-standard expectations are failures
// Expect-100 requested but no body (is apparently okay: Issue 7625)
// Expect-100 requested but handler doesn't read the body
// Expect-100 continue with no body, but a chunked body.
internal static slice<serverExpectTest> serverExpectTests = new serverExpectTest[]{
    expectTest(100, "100-continue"u8, true, "100 Continue"u8),
    expectTest(100, "100-cOntInUE"u8, true, "100 Continue"u8),
    expectTest(100, ""u8, true, "200 OK"u8),
    expectTest(100, "100-continue"u8, false, "401 Unauthorized"u8),
    expectTest(100, ""u8, false, "401 Unauthorized"u8),
    expectTest(0, "a-pony"u8, false, "417 Expectation Failed"u8),
    expectTest(0, "100-continue"u8, true, "200 OK"u8),
    expectTest(0, "100-continue"u8, false, "401 Unauthorized"u8),
    new(
        expectation: "100-continue"u8,
        readBody: true,
        chunked: true,
        expectedResponse: "100 Continue"u8
    )
}.slice();

// Tests that the server responds to the "Expect" request header
// correctly.
public static void TestServerExpect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerExpect(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readbodyTrueˢ = "readbody=true"u8;
internal static readonly @string transferEncodingChunkedˢ = "Transfer-Encoding: chunked"u8;

[GoType("dyn")] internal partial struct testServerExpect_type {
    [GoEmbedded] public io_package.Writer Writer;
    [GoEmbedded] public io_package.Closer Closer;
}

internal static void testServerExpect(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        // Note using r.FormValue("readbody") because for POST
        // requests that would read from r.Body, which we only
        // conditionally want to do.
        if (strings.Contains((~(~r).URL).RawQuery, readbodyTrueˢ)){
            io.ReadAll((~r).Body);
            w.Write(slice<byte>("Hi"u8));
        } else {
            w.WriteHeader(StatusUnauthorized);
        }
    }))).Value.ts;
    var tsʗ1 = ts;
    void runTest(serverExpectTest test) {
        GoFrame ᒐ = default;
        try {
            var (conn, err) = net.Dial(tcpˢ, (~tsʗ1).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatalf("Dial: %v"u8, err);
            }
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            // Only send the body immediately if we're acting like an HTTP client
            // that doesn't send 100-continue expectations.
            var writeBody = test.contentLength != 0 && strings.ToLower(test.expectation) != "100-continue"u8;
            ref var wg = ref heap<sync.WaitGroup>(out var Ꮡwg);
            Ꮡwg.Value = new sync.WaitGroup(nil);
            Ꮡwg.Add(1);
            defer(Ꮡwg.Wait, ref ᒐ);
            var connʗ2 = conn;
            var testʗ1 = test;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    @string contentLen = fmt.Sprintf("Content-Length: %d"u8, testʗ1.contentLength);
                    if (testʗ1.chunked) {
                        contentLen = transferEncodingChunkedˢ;
                    }
                    var (_, errΔ1) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(connʗ2), "POST /?readbody=%v HTTP/1.1\r\n"u8 + "Connection: close\r\n"u8 + "%s\r\n"u8 + "Expect: %s\r\nHost: foo\r\n\r\n"u8,
                        testʗ1.readBody, contentLen, testʗ1.expectation);
                    if (errΔ1 != default!) {
                        Ꮡt.Errorf("On test %#v, error writing request headers: %v"u8, testʗ1, errΔ1);
                        return;
                    }
                    if (writeBody) {
                        io.WriteCloser targ = new testServerExpect_type(
                            new http_test_package.net_ConnᴠWriter(connʗ2),
                            io.NopCloser(default!)
                        );
                        if (testʗ1.chunked) {
                            targ = httputil.NewChunkedWriter(new http_test_package.net_ConnᴠWriter(connʗ2));
                        }
                        @string body = strings.Repeat("A"u8, testʗ1.contentLength);
                        (_, errΔ1) = fmt.Fprint(targ, body);
                        if (errΔ1 == default!) {
                            errΔ1 = targ.Close();
                        }
                        if (errΔ1 != default!) {
                            if (!testʗ1.readBody) {
                                // Server likely already hung up on us.
                                // See larger comment below.
                                Ꮡt.Logf("On test %#v, acceptable error writing request body: %v"u8, testʗ1, errΔ1);
                                return;
                            }
                            Ꮡt.Errorf("On test %#v, error writing request body: %v"u8, testʗ1, errΔ1);
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            var bufr = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
            (var line, err) = bufr.ReadString((rune)'\n');
            if (err != default!) {
                if (writeBody && !test.readBody) {
                    // This is an acceptable failure due to a possible TCP race:
                    // We were still writing data and the server hung up on us. A TCP
                    // implementation may send a RST if our request body data was known
                    // to be lost, which may trigger our reads to fail.
                    // See RFC 1122 page 88.
                    Ꮡt.Logf("On test %#v, acceptable error from ReadString: %v"u8, test, err);
                    return;
                }
                Ꮡt.Fatalf("On test %#v, ReadString: %v"u8, test, err);
            }
            if (!strings.Contains(line, test.expectedResponse)) {
                Ꮡt.Errorf("On test %#v, got first line = %q; want %q"u8, test, line, test.expectedResponse);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    foreach (var (_, test) in serverExpectTests) {
        runTest(test);
    }
}

// Under a ~256KB (maxPostHandlerReadBytes) threshold, the server
// should consume client request bodies that a handler didn't read.
public static void TestServerUnreadRequestBodyLittle(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var conn = @new<testConn>();
        @string body = strings.Repeat("x"u8, (100 << (int)(10)));
        conn.of(testConn.ᏑreadBuf).Write(slice<byte>(fmt.Sprintf(
            "POST / HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "Content-Length: %d\r\n"u8 + "\r\n"u8, len(body))));
        conn.of(testConn.ᏑreadBuf).Write(slice<byte>(body));
        var done = new channel<bool>(0);
        var connʗ1 = conn;
        nint readBufLen() {
            GoFrame ᒐ = default;
            try {
                connʗ1.of(testConn.ᏑreadMu).Lock();
                var connʗ2 = connʗ1;
                defer(connʗ2.of(testConn.ᏑreadMu).Unlock, ref ᒐ);
                return connʗ1.of(testConn.ᏑreadBuf).Len();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        var ls = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
        var doneʗ1 = done;
        var readBufLenʗ1 = readBufLen;
        goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ls), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
                {
                    nint bufLen = readBufLenʗ1(); if (bufLen < len(body) / 2) {
                        Ꮡt.Errorf("on request, read buffer length is %d; expected about 100 KB"u8, bufLen);
                    }
                }
                rw.WriteHeader(200);
                rw._<Flusher>().Flush();
                {
                    nint g = readBufLenʗ1();
                    nint e = 0; if (g != e) {
                        Ꮡt.Errorf("after WriteHeader, read buffer length is %d; want %d"u8, g, e);
                    }
                }
                {
                    @string c = rw.Header().Get(connectionˢ); if (c != ""u8) {
                        Ꮡt.Errorf(@"Connection header = %q; want """""u8, c);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Over a ~256KB (maxPostHandlerReadBytes) threshold, the server
// should ignore client request bodies that a handler didn't read
// and close the connection.
public static void TestServerUnreadRequestBodyLarge(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Log(skippingInShortModeˢ);
    }
    var conn = @new<testConn>();
    @string body = strings.Repeat("x"u8, (1 << (int)(20)));
    conn.of(testConn.ᏑreadBuf).Write(slice<byte>(fmt.Sprintf(
        "POST / HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "Content-Length: %d\r\n"u8 + "\r\n"u8, len(body))));
    conn.of(testConn.ᏑreadBuf).Write(slice<byte>(body));
    conn.Value.closec = new channel<bool>(1);
    var ls = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
    var connʗ1 = conn;
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ls), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        if (connʗ1.of(testConn.ᏑreadBuf).Len() < len(body) / 2) {
            Ꮡt.Errorf("on request, read buffer length is %d; expected about 1MB"u8, connʗ1.of(testConn.ᏑreadBuf).Len());
        }
        rw.WriteHeader(200);
        rw._<Flusher>().Flush();
        if (connʗ1.of(testConn.ᏑreadBuf).Len() < len(body) / 2) {
            Ꮡt.Errorf("post-WriteHeader, read buffer length is %d; expected about 1MB"u8, connʗ1.of(testConn.ᏑreadBuf).Len());
        }
    })));
    ᐸꟷ((~conn).closec);
    {
        @string res = conn.of(testConn.ᏑwriteBuf).String(); if (!strings.Contains(res, connectionCloseˢ)) {
            Ꮡt.Errorf("Expected a Connection: close header; got response: %s"u8, res);
        }
    }
}

[GoType] partial struct handlerBodyCloseTest {
    internal nint bodySize;
    internal bool bodyChunked;
    internal bool reqConnClose;
    internal bool wantEOFSearch; // should Handler's Body.Close do Reads, looking for EOF?
    internal bool wantNextReq; // should it find the next request on the same conn?
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connectionCloseˢ2 = "Connection: close\r\n"u8;

internal static @string connectionHeader(this handlerBodyCloseTest t) {
    if (t.reqConnClose) {
        return connectionCloseˢ2;
    }
    return ""u8;
}

// Small enough to slurp past to the next request +
// has Content-Length.
// Small enough to slurp past to the next request +
// is chunked.
// Small enough to slurp past to the next request +
// has Content-Length +
// declares Connection: close (so pointless to read more).
// Small enough to slurp past to the next request +
// declares Connection: close,
// but chunked, so it might have trailers.
// TODO: maybe skip this search if no trailers were declared
// in the headers.
// Big with Content-Length, so give up immediately if we know it's too big.
// has a Content-Length
// Big chunked, so read a bit before giving up.
// Big with Connection: close, but chunked, so search for trailers.
// TODO: maybe skip this search if no trailers were declared
// in the headers.
// Big with Connection: close, so don't do any reads on Close.
// With Content-Length.
internal static array<handlerBodyCloseTest> handlerBodyCloseTests = new array<handlerBodyCloseTest>(8){
    [0] = new(
        bodySize: (20 << (int)(10)),
        bodyChunked: false,
        reqConnClose: false,
        wantEOFSearch: true,
        wantNextReq: true
    ),
    [1] = new(
        bodySize: (20 << (int)(10)),
        bodyChunked: true,
        reqConnClose: false,
        wantEOFSearch: true,
        wantNextReq: true
    ),
    [2] = new(
        bodySize: (20 << (int)(10)),
        bodyChunked: false,
        reqConnClose: true,
        wantEOFSearch: false,
        wantNextReq: false
    ),
    [3] = new(
        bodySize: (20 << (int)(10)),
        bodyChunked: true,
        reqConnClose: true,
        wantEOFSearch: true,
        wantNextReq: false
    ),
    [4] = new(
        bodySize: (1 << (int)(20)),
        bodyChunked: false,
        reqConnClose: false,
        wantEOFSearch: false,
        wantNextReq: false
    ),
    [5] = new(
        bodySize: (1 << (int)(20)),
        bodyChunked: true,
        reqConnClose: false,
        wantEOFSearch: true,
        wantNextReq: false
    ),
    [6] = new(
        bodySize: (1 << (int)(20)),
        bodyChunked: true,
        reqConnClose: true,
        wantEOFSearch: true,
        wantNextReq: false
    ),
    [7] = new(
        bodySize: (1 << (int)(20)),
        bodyChunked: false,
        reqConnClose: true,
        wantEOFSearch: false,
        wantNextReq: false
    )
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ2 = (@string)"skipping in -short mode"u8;

public static void TestHandlerBodyClose(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingInShortModeˢ2);
    }
    foreach (var (i, tt) in handlerBodyCloseTests.ΔRangeSnapshot()) {
        testHandlerBodyClose(Ꮡt, i, tt);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostTestˢ = "GET / HTTP/1.1\r\nHost: test\r\n\r\n"u8;

internal static void testHandlerBodyClose(ж<testing.T> Ꮡt, nint i, handlerBodyCloseTest tt) {
    var conn = @new<testConn>();
    @string body = strings.Repeat("x"u8, tt.bodySize);
    if (tt.bodyChunked){
        conn.of(testConn.ᏑreadBuf).WriteString("POST / HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + tt.connectionHeader() + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8);
        var cw = @internal.NewChunkedWriter(new http_test_package.bytes_BufferжWriter(conn.of(testConn.ᏑreadBuf)));
        io.WriteString(cw, body);
        cw.Close();
        conn.of(testConn.ᏑreadBuf).WriteString("\r\n"u8);
    } else {
        conn.of(testConn.ᏑreadBuf).Write(slice<byte>(fmt.Sprintf(
            "POST / HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + tt.connectionHeader() + "Content-Length: %d\r\n"u8 + "\r\n"u8, len(body))));
        conn.of(testConn.ᏑreadBuf).Write(slice<byte>(body));
    }
    if (!tt.reqConnClose) {
        conn.of(testConn.ᏑreadBuf).WriteString(getHttp11HostTestˢ);
    }
    conn.Value.closec = new channel<bool>(1);
    var connʗ1 = conn;
    nint readBufLen() {
        GoFrame ᒐ = default;
        try {
            connʗ1.of(testConn.ᏑreadMu).Lock();
            var connʗ2 = connʗ1;
            defer(connʗ2.of(testConn.ᏑreadMu).Unlock, ref ᒐ);
            return connʗ1.of(testConn.ᏑreadBuf).Len();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    var ls = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
    nint numReqs = default!;
    nint size0 = default!;
    nint size1 = default!;
    var readBufLenʗ1 = readBufLen;
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ls), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        numReqs++;
        if (numReqs == 1) {
            size0 = readBufLenʗ1();
            (~req).Body.Close();
            size1 = readBufLenʗ1();
        }
    })));
    ᐸꟷ((~conn).closec);
    if (numReqs < 1 || numReqs > 2) {
        Ꮡt.Fatalf("%d. bug in test. unexpected number of requests = %d"u8, i, numReqs);
    }
    var didSearch = size0 != size1;
    if (didSearch != tt.wantEOFSearch) {
        Ꮡt.Errorf("%d. did EOF search = %v; want %v (size went from %d to %d)"u8, i, didSearch, !didSearch, size0, size1);
    }
    if (tt.wantNextReq && numReqs != 2) {
        Ꮡt.Errorf("%d. numReq = %d; want 2"u8, i, numReqs);
    }
}

// testHandlerBodyConsumer represents a function injected into a test handler to
// vary work done on a request Body.
[GoType] partial struct testHandlerBodyConsumer {
    internal @string name;
    internal Action<io.ReadCloser> f;
}

internal static slice<testHandlerBodyConsumer> testHandlerBodyConsumers = new testHandlerBodyConsumer[]{
    new("nil"u8, (io.ReadCloser _) => {
    }),
    new("close"u8, (io.ReadCloser r) => {
        r.Close();
    }),
    new("discard"u8, (io.ReadCloser r) => {
        io.Copy(io.Discard, r);
    })
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string secretˢ = "secret"u8;
internal static readonly object requestForSecretˢ = (@string)"Request for /secret encountered, should not have happened."u8;

public static void TestRequestBodyReadErrorClosesConnection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        foreach (var (_, vᴛ1) in testHandlerBodyConsumers) {
            ref var handler = ref heap(new testHandlerBodyConsumer(), out var Ꮡhandler);
            handler = vᴛ1;

            var conn = @new<testConn>();
            conn.of(testConn.ᏑreadBuf).WriteString("POST /public HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "hax\r\n"u8 + "GET /secret HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "\r\n"u8);
            // Invalid chunked encoding
            conn.Value.closec = new channel<bool>(1);
            var ls = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
            nint numReqs = default!;
            var handlerʗ1 = handler;
            goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ls), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _, ж<Δhttp.Request> req) => {
                numReqs++;
                if (strings.Contains((~(~req).URL).Path, secretˢ)) {
                    Ꮡt.Error(requestForSecretˢ);
                }
                handlerʗ1.f((~req).Body);
            })));
            ᐸꟷ((~conn).closec);
            if (numReqs != 1) {
                Ꮡt.Errorf("Handler %v: got %d reqs; want 1"u8, handler.name, numReqs);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestInvalidTrailerClosesConnection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        foreach (var (_, vᴛ1) in testHandlerBodyConsumers) {
            ref var handler = ref heap(new testHandlerBodyConsumer(), out var Ꮡhandler);
            handler = vᴛ1;

            var conn = @new<testConn>();
            conn.of(testConn.ᏑreadBuf).WriteString("POST /public HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "Trailer: hack\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "3\r\n"u8 + "hax\r\n"u8 + "0\r\n"u8 + "I'm not a valid trailer\r\n"u8 + "GET /secret HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "\r\n"u8);
            conn.Value.closec = new channel<bool>(1);
            var ln = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
            nint numReqs = default!;
            var handlerʗ1 = handler;
            goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _, ж<Δhttp.Request> req) => {
                numReqs++;
                if (strings.Contains((~(~req).URL).Path, secretˢ)) {
                    Ꮡt.Errorf("Handler %s, Request for /secret encountered, should not have happened."u8, handlerʗ1.name);
                }
                handlerʗ1.f((~req).Body);
            })));
            ᐸꟷ((~conn).closec);
            if (numReqs != 1) {
                Ꮡt.Errorf("Handler %s: got %d reqs; want 1"u8, handler.name, numReqs);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// slowTestConn is a net.Conn that provides a means to simulate parts of a
// request being received piecemeal. Deadlines can be set and enforced in both
// Read and Write.
[GoType] partial struct slowTestConn {
    // over multiple calls to Read, time.Durations are slept, strings are read.
    internal slice<any> script;
    internal channel<bool> closec;
    internal sync.Mutex mu; // guards rd/wd
    internal time.Time rd, wd;  // read, write deadline
    internal partial ref noopConn noopConn { get; }
}

internal static error SetDeadline(this ж<slowTestConn> Ꮡc, time.Time t) {
    Ꮡc.SetReadDeadline(t);
    Ꮡc.SetWriteDeadline(t);
    return default!;
}

internal static error SetReadDeadline(this ж<slowTestConn> Ꮡc, time.Time t) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        c.rd = t;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error SetWriteDeadline(this ж<slowTestConn> Ꮡc, time.Time t) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        c.wd = t;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static (nint n, error err) Read(this ж<slowTestConn> Ꮡc, slice<byte> b) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        defer(Ꮡc.of(slowTestConn.Ꮡmu).Unlock, ref ᒐ);
restart:
        if (!c.rd.IsZero() && time.Now().After(c.rd)) {
            (n, err) = (0, syscall.ETIMEDOUT); goto ᒐdone;
        }
        if (len(c.script) == 0) {
            (n, err) = (0, io.EOF); goto ᒐdone;
        }
        switch (c.script[0].type()) {
        case time.Duration cue: {
            if (!c.rd.IsZero()) {
                // If the deadline falls in the middle of our sleep window, deduct
                // part of the sleep, then return a timeout.
                {
                    var remaining = time.Until(c.rd); if (remaining < cue) {
                        c.script[0] = cue - remaining;
                        time.Sleep(remaining);
                        (n, err) = (0, syscall.ETIMEDOUT); goto ᒐdone;
                    }
                }
            }
            c.script = c.script[1..];
            time.Sleep(cue);
            goto restart;
            break;
        }
        case @string cue: {
            n = copy(b, cue);
            if (len(cue) > n){
                // If cue is too big for the buffer, leave the end for the next Read.
                c.script[0] = cue[(int)(n)..];
            } else {
                c.script = c.script[1..];
            }
            break;
        }
        default: {
            var cue = c.script[0];
            throw panic("unknown cue in slowTestConn script");
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

[GoRecv] internal static error Close(this ref slowTestConn c) {
    var selᴛ44 = c.closec.ᐸꟷ(true, ꓸꓸꓸ);
    switch (trySelect(selᴛ44)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
    return default!;
}

[GoRecv] internal static (nint, error) Write(this ref slowTestConn c, slice<byte> b) {
    if (!c.wd.IsZero() && time.Now().After(c.wd)) {
        return (0, syscall.ETIMEDOUT);
    }
    return (len(b), default!);
}

public static void TestRequestBodyTimeoutClosesConnection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ2);
        }
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        foreach (var (_, vᴛ1) in testHandlerBodyConsumers) {
            ref var handler = ref heap(new testHandlerBodyConsumer(), out var Ꮡhandler);
            handler = vᴛ1;

            var conn = Ꮡ(new slowTestConn(
                script: new any[]{
                    (@string)("POST /public HTTP/1.1\r\n" + "Host: test\r\n" + "Content-Length: 10000\r\n" + "\r\n"),
                    (@string)"foo bar baz"u8,
                    600 * time.Millisecond, // Request deadline should hit here

                    (@string)("GET /secret HTTP/1.1\r\n" + "Host: test\r\n" + "\r\n")
                }.slice(),
                closec: new channel<bool>(1)
            ));
            var ls = Ꮡ(new oneConnListener(new http_test_package.slowTestConnжConn(conn)));
            nint numReqs = default!;
                var handlerʗ1 = handler;
            ref var s = ref heap<Δhttp.Server>(out var Ꮡs);
            s = new Server(
                Handler: new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _, ж<Δhttp.Request> req) => {
                    numReqs++;
                    if (strings.Contains((~(~req).URL).Path, secretˢ)) {
                        Ꮡt.Error(requestForSecretˢ);
                    }
                    handlerʗ1.f((~req).Body);
                })),
                ReadTimeout: 400 * time.Millisecond
            );
            goǃ(ᴛ1 => Ꮡs.Serve(ᴛ1), new http_test_package.oneConnListenerжListener(ls));
            ᐸꟷ((~conn).closec);
            if (numReqs != 1) {
                Ꮡt.Errorf("Handler %v: got %d reqs; want 1"u8, handler.name, numReqs);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// cancelableTimeoutContext overwrites the error message to DeadlineExceeded
[GoType] partial struct cancelableTimeoutContext {
    [GoEmbedded] public context_package.Context Context;
}

internal static error Err(this cancelableTimeoutContext c) {
    if (c.Context.Err() != default!) {
        return context_package.DeadlineExceeded;
    }
    return default!;
}

public static void TestTimeoutHandler(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandler(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string titleTimeoutTitleˢ = "<title>Timeout</title>"u8;
internal static readonly @string textHtmlCharsetUtf8ˢ = "text/html; charset=utf-8"u8;

internal static void testTimeoutHandler(ж<testing.T> Ꮡt, testMode mode) {
    var sendHi = new channel<bool>(1);
    var writeErrors = new channel<error>(1);
    var sendHiʗ1 = sendHi;
    var writeErrorsʗ1 = writeErrors;
    var sayHi = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        ᐸꟷ(sendHiʗ1);
        var (_, werr) = w.Write(slice<byte>("hi"u8));
        writeErrorsʗ1.ᐸꟷ(werr);
    });
    var (ctx, cancel) = context_package.WithCancel(context_package.Background());
    var h = http_internal_test_package.NewTestTimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(sayHi), new cancelableTimeoutContext(ctx));
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, h);
    // Succeed without timing out:
    sendHi.ᐸꟷ(true);
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    {
        nint g = res.Value.StatusCode;
        nint e = StatusOK; if (g != e) {
            Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
        }
    }
    var (body, _) = io.ReadAll((~res).Body);
    {
        @string g = ((@string)body);
        @string e = "hi"u8; if (g != e) {
            Ꮡt.Errorf("got body %q; expected %q"u8, g, e);
        }
    }
    {
        var g = ᐸꟷ(writeErrors); if (g != default!) {
            Ꮡt.Errorf("got unexpected Write error on first request: %v"u8, g);
        }
    }
    // Times out:
    cancel();
    (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    {
        nint g = res.Value.StatusCode;
        nint e = StatusServiceUnavailable; if (g != e) {
            Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
        }
    }
    (body, _) = io.ReadAll((~res).Body);
    if (!strings.Contains(((@string)body), titleTimeoutTitleˢ)) {
        Ꮡt.Errorf("expected timeout body; got %q"u8, ((@string)body));
    }
    {
        @string g = (~res).Header.Get(contentTypeˢ);
        @string w = textHtmlCharsetUtf8ˢ; if (g != w) {
            Ꮡt.Errorf("response content-type = %q; want %q"u8, g, w);
        }
    }
    // Now make the previously-timed out handler speak again,
    // which verifies the panic is handled:
    sendHi.ᐸꟷ(true);
    {
        var (g, e) = (ᐸꟷ(writeErrors), ErrHandlerTimeout); if (!AreEqual(g, e)) {
            Ꮡt.Errorf("expected Write error of %v; got %v"u8, e, g);
        }
    }
}

// See issues 8209 and 8414.
public static void TestTimeoutHandlerRace(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerRace(Δp0, Δp1));
}

internal static void testTimeoutHandlerRace(ж<testing.T> Ꮡt, testMode mode) {
    var delayHi = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (ms, _) = strconv.Atoi((~(~r).URL).Path[1..]);
        if (ms == 0) {
            ms = 1;
        }
        for (nint i = 0; i < ms; i++) {
            w.Write(slice<byte>("hi"u8));
            time.Sleep(time.Millisecond);
        }
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, TimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(delayHi), 20 * time.Millisecond, ""u8)).Value.ts;
    var c = ts.Client();
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    var gate = new channel<bool>(10);
    nint n = 50;
    if (testing.Short()) {
        n = 10;
        gate = new channel<bool>(3);
    }
    for (nint i = 0; i < n; i++) {
        gate.ᐸꟷ(true);
        Ꮡwg.Add(1);
        var cʗ1 = c;
        var gateʗ1 = gate;
        var tsʗ1 = ts;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var gateʗ2 = gateʗ1;
                defer(() => {
                    ᐸꟷ(gateʗ2);
                }, ref ᒐ);
                var (res, err) = cʗ1.Get(fmt.Sprintf("%s/%d"u8, (~tsʗ1).URL, rand.Intn(50)));
                if (err == default!) {
                    io.Copy(io.Discard, (~res).Body);
                    (~res).Body.Close();
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// See issues 8209 and 8414.
// Both issues involved panics in the implementation of TimeoutHandler.
public static void TestTimeoutHandlerRaceHeader(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerRaceHeader(Δp0, Δp1));
}

internal static void testTimeoutHandlerRaceHeader(ж<testing.T> Ꮡt, testMode mode) {
    var delay204 = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(204);
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, TimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(delay204), time.ΔNanosecond, ""u8)).Value.ts;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    var gate = new channel<bool>(50);
    nint n = 500;
    if (testing.Short()) {
        n = 10;
    }
    var c = ts.Client();
    for (nint i = 0; i < n; i++) {
        gate.ᐸꟷ(true);
        Ꮡwg.Add(1);
        var cʗ1 = c;
        var gateʗ1 = gate;
        var tsʗ1 = ts;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var gateʗ2 = gateʗ1;
                defer(() => {
                    ᐸꟷ(gateʗ2);
                }, ref ᒐ);
                var (res, err) = cʗ1.Get((~tsʗ1).URL);
                if (err != default!) {
                    // We see ECONNRESET from the connection occasionally,
                    // and that's OK: this test is checking that the server does not panic.
                    Ꮡt.Log(err);
                    return;
                }
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                io.Copy(io.Discard, (~res).Body);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Issue 9162
public static void TestTimeoutHandlerRaceHeaderTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerRaceHeaderTimeout(Δp0, Δp1));
}

internal static void testTimeoutHandlerRaceHeaderTimeout(ж<testing.T> Ꮡt, testMode mode) {
    var sendHi = new channel<bool>(1);
    var writeErrors = new channel<error>(1);
    var sendHiʗ1 = sendHi;
    var writeErrorsʗ1 = writeErrors;
    var sayHi = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentTypeˢ, textPlainˢ);
        ᐸꟷ(sendHiʗ1);
        var (_, werr) = w.Write(slice<byte>("hi"u8));
        writeErrorsʗ1.ᐸꟷ(werr);
    });
    var (ctx, cancel) = context_package.WithCancel(context_package.Background());
    var h = http_internal_test_package.NewTestTimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(sayHi), new cancelableTimeoutContext(ctx));
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, h);
    // Succeed without timing out:
    sendHi.ᐸꟷ(true);
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    {
        nint g = res.Value.StatusCode;
        nint e = StatusOK; if (g != e) {
            Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
        }
    }
    var (body, _) = io.ReadAll((~res).Body);
    {
        @string g = ((@string)body);
        @string e = "hi"u8; if (g != e) {
            Ꮡt.Errorf("got body %q; expected %q"u8, g, e);
        }
    }
    {
        var g = ᐸꟷ(writeErrors); if (g != default!) {
            Ꮡt.Errorf("got unexpected Write error on first request: %v"u8, g);
        }
    }
    // Times out:
    cancel();
    (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    {
        nint g = res.Value.StatusCode;
        nint e = StatusServiceUnavailable; if (g != e) {
            Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
        }
    }
    (body, _) = io.ReadAll((~res).Body);
    if (!strings.Contains(((@string)body), titleTimeoutTitleˢ)) {
        Ꮡt.Errorf("expected timeout body; got %q"u8, ((@string)body));
    }
    // Now make the previously-timed out handler speak again,
    // which verifies the panic is handled:
    sendHi.ᐸꟷ(true);
    {
        var (g, e) = (ᐸꟷ(writeErrors), ErrHandlerTimeout); if (!AreEqual(g, e)) {
            Ꮡt.Errorf("expected Write error of %v; got %v"u8, e, g);
        }
    }
}

// Issue 14568.
public static void TestTimeoutHandlerStartTimerWhenServing(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerStartTimerWhenServing(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingSleepingTestInˢ = (@string)"skipping sleeping test in -short mode"u8;

internal static void testTimeoutHandlerStartTimerWhenServing(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingSleepingTestInˢ);
        }
        Δhttp.HandlerFunc handler = (Δhttp.ResponseWriter w, ж<Δhttp.Request> _) => {
            w.WriteHeader(StatusNoContent);
        };
        var timeout = 300 * time.Millisecond;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, TimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(handler), timeout, ""u8)).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var c = ts.Client();
        // Issue was caused by the timeout handler starting the timer when
        // was created, not when the request. So wait for more than the timeout
        // to ensure that's not the case.
        time.Sleep(2 * timeout);
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != StatusNoContent) {
            Ꮡt.Errorf("got res.StatusCode %d, want %v"u8, (~res).StatusCode, (nint)(StatusNoContent));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestTimeoutHandlerContextCanceled(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerContextCanceled(Δp0, Δp1));
}

internal static void testTimeoutHandlerContextCanceled(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var writeErrors = new channel<error>(1);
        var writeErrorsʗ1 = writeErrors;
        var sayHi = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentTypeˢ, textPlainˢ);
            error errΔ1 = default!;
            // The request context has already been canceled, but
            // retry the write for a while to give the timeout handler
            // a chance to notice.
            for (nint i = 0; i < 100; i++) {
                (_, errΔ1) = w.Write(slice<byte>("a"u8));
                if (errΔ1 != default!) {
                    break;
                }
                time.Sleep(1 * time.Millisecond);
            }
            writeErrorsʗ1.ᐸꟷ(errΔ1);
        });
        var (ctx, cancel) = context_package.WithCancel(context_package.Background());
        cancel();
        var h = http_internal_test_package.NewTestTimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(sayHi), ctx);
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, h);
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        {
            nint g = res.Value.StatusCode;
            nint e = StatusServiceUnavailable; if (g != e) {
                Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
            }
        }
        var (body, _) = io.ReadAll((~res).Body);
        {
            @string g = ((@string)body);
            @string e = ""u8; if (g != e) {
                Ꮡt.Errorf("got body %q; expected %q"u8, g, e);
            }
        }
        {
            var (g, e) = (ᐸꟷ(writeErrors), context_package.Canceled); if (!AreEqual(g, e)) {
                Ꮡt.Errorf("got unexpected Write in handler: %v, want %g"u8, g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// https://golang.org/issue/15948
public static void TestTimeoutHandlerEmptyResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerEmptyResponse(Δp0, Δp1));
}

internal static void testTimeoutHandlerEmptyResponse(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
// No response.
        Δhttp.HandlerFunc handler = (Δhttp.ResponseWriter w, ж<Δhttp.Request> _) => {
        };
        var timeout = 300 * time.Millisecond;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, TimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(handler), timeout, ""u8)).Value.ts;
        var c = ts.Client();
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != StatusOK) {
            Ꮡt.Errorf("got res.StatusCode %d, want %v"u8, (~res).StatusCode, (nint)(StatusOK));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object intentionalDeathForˢ = (@string)"intentional death for testing"u8;

// https://golang.org/issues/22084
public static void TestTimeoutHandlerPanicRecovery(ж<testing.T> Ꮡt) {
    var wrapper = (httpꓸHandler h) => TimeoutHandler(h, time.ΔSecond, ""u8);
    var wrapperʗ1 = wrapper;
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testHandlerPanic(tΔ1, false, mode, wrapperʗ1, intentionalDeathForˢ);
    }, testNotParallel);
}

public static void TestRedirectBadPath(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This used to crash. It's not valid input (bad path), but it
    // shouldn't crash.
    var rr = httptest.NewRecorder();
    var req = Ꮡ(new Request(
        Method: "GET"u8,
        URL: Ꮡ(new url.URL(
            Scheme: "http"u8,
            Path: "not-empty-but-no-leading-slash"u8
        ))
    ));
    // bogus
    Redirect(new http_test_package.httptest_ResponseRecorderжResponseWriter(rr), req, ""u8, 304);
    if ((~rr).Code != 304) {
        Ꮡt.Errorf("Code = %d; want 304"u8, (~rr).Code);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComQuxˢ = "http://example.com/qux/"u8;

[GoType("dyn")] internal partial struct TestRedirect_type {
    internal @string @in;
    internal @string want;
}

// Test different URL formats and schemes
public static void TestRedirect(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (req, _) = NewRequest(getˢ2, httpExampleComQuxˢ, default!);
// normal http
// normal https
// custom scheme
// schemeless
// relative to the root
// relative to the current path
// relative to the current path (+ going upwards)
// incorrect number of slashes
// Verifies we don't path.Clean() on the wrong parts in redirects:
    slice<TestRedirect_type> tests = new TestRedirect_type[]{
        new("http://foobar.com/baz"u8, "http://foobar.com/baz"u8),
        new("https://foobar.com/baz"u8, "https://foobar.com/baz"u8),
        new("test://foobar.com/baz"u8, "test://foobar.com/baz"u8),
        new("//foobar.com/baz"u8, "//foobar.com/baz"u8),
        new("/foobar.com/baz"u8, "/foobar.com/baz"u8),
        new("foobar.com/baz"u8, "/qux/foobar.com/baz"u8),
        new("../quux/foobar.com/baz"u8, "/quux/foobar.com/baz"u8),
        new("///foobar.com/baz"u8, "/foobar.com/baz"u8),
        new("/foo?next=http://bar.com/"u8, "/foo?next=http://bar.com/"u8),
        new("http://localhost:8080/_ah/login?continue=http://localhost:8080/"u8,
            "http://localhost:8080/_ah/login?continue=http://localhost:8080/"u8),
        new("/фубар"u8, "/%d1%84%d1%83%d0%b1%d0%b0%d1%80"u8),
        new("http://foo.com/фубар"u8, "http://foo.com/%d1%84%d1%83%d0%b1%d0%b0%d1%80"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var rec = httptest.NewRecorder();
        Redirect(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, tt.@in, 302);
        {
            nint got = rec.Value.Code;
            nint want = 302; if (got != want) {
                Ꮡt.Errorf("Redirect(%q) generated status code %v; want %v"u8, tt.@in, got, want);
            }
        }
        {
            @string got = rec.Header().Get(locationˢ); if (got != tt.want) {
                Ꮡt.Errorf("Redirect(%q) generated Location header %q; want %q"u8, tt.@in, got, tt.want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestRedirectContentTypeAndBody_ctHeader {
    public slice<@string> Values;
}

[GoType("dyn")] internal partial struct TestRedirectContentTypeAndBody_type {
    internal @string method;
    internal ж<TestRedirectContentTypeAndBody_ctHeader> ct; // Optional Content-Type header to set.
    internal @string wantCT;
    internal @string wantBody;
}

// Test that Redirect sets Content-Type header for GET and HEAD requests
// and writes a short HTML body, unless the request already has a Content-Type header.
public static void TestRedirectContentTypeAndBody(ж<testing.T> Ꮡt) {
    slice<TestRedirectContentTypeAndBody_type> tests = new TestRedirectContentTypeAndBody_type[]{
        new(MethodGet, nil, "text/html; charset=utf-8"u8, "<a href=\"/foo\">Found</a>.\n\n"u8),
        new(MethodHead, nil, "text/html; charset=utf-8"u8, ""u8),
        new(MethodPost, nil, ""u8, ""u8),
        new(MethodDelete, nil, ""u8, ""u8),
        new("foo"u8, nil, ""u8, ""u8),
        new(MethodGet, Ꮡ(new TestRedirectContentTypeAndBody_ctHeader(new @string[]{"application/test"u8}.slice())), "application/test"u8, ""u8),
        new(MethodGet, Ꮡ(new TestRedirectContentTypeAndBody_ctHeader(new @string[]{}.slice())), ""u8, ""u8),
        new(MethodGet, Ꮡ(new TestRedirectContentTypeAndBody_ctHeader((slice<@string>)(default!))), ""u8, ""u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var req = httptest.NewRequest(tt.method, httpExampleComQuxˢ, default!);
        var rec = httptest.NewRecorder();
        if (tt.ct != nil) {
            rec.Header()[contentTypeˢ] = tt.ct.Value.Values;
        }
        Redirect(new http_test_package.httptest_ResponseRecorderжResponseWriter(rec), req, fooˢ3, 302);
        {
            nint got = rec.Value.Code;
            nint want = 302; if (got != want) {
                Ꮡt.Errorf("Redirect(%q, %#v) generated status code %v; want %v"u8, tt.method, tt.ct.OrTypedNil(), got, want);
            }
        }
        {
            @string got = rec.Header().Get(contentTypeˢ);
            @string want = tt.wantCT; if (got != want) {
                Ꮡt.Errorf("Redirect(%q, %#v) generated Content-Type header %q; want %q"u8, tt.method, tt.ct.OrTypedNil(), got, want);
            }
        }
        var resp = rec.Result();
        var (body, err) = io.ReadAll((~resp).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            @string got = ((@string)body);
            @string want = tt.wantBody; if (got != want) {
                Ꮡt.Errorf("Redirect(%q, %#v) generated Body %q; want %q"u8, tt.method, tt.ct.OrTypedNil(), got, want);
            }
        }
    }
}

// TestZeroLengthPostAndResponse exercises an optimization done by the Transport:
// when there is no body (either because the method doesn't permit a body, or an
// explicit Content-Length of zero is present), then the transport can re-use the
// connection immediately. But when it re-uses the connection, it typically closes
// the previous request's body, which is not optimal for zero-lengthed bodies,
// as the client would then see http.ErrBodyReadAfterClose and not 0, io.EOF.
public static void TestZeroLengthPostAndResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testZeroLengthPostAndResponse(Δp0, Δp1));
}

internal static void testZeroLengthPostAndResponse(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        var (all, errΔ1) = io.ReadAll((~r).Body);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("handler ReadAll: %v"u8, errΔ1);
        }
        if (len(all) != 0) {
            Ꮡt.Errorf("handler got %d bytes; expected 0"u8, len(all));
        }
        rw.Header().Set(contentLengthˢ, "0"u8);
    })));
    var (req, err) = NewRequest(postˢ, (~(~cst).ts).URL, new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    req.Value.ContentLength = 0;
    array<ж<Δhttp.Response>> resp = new(5);
    foreach (var (i, _) in resp) {
        (resp[i], err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("client post #%d: %v"u8, i, err);
        }
    }
    foreach (var (i, _) in resp) {
        var (all, errΔ2) = io.ReadAll((~resp[i]).Body);
        if (errΔ2 != default!) {
            Ꮡt.Fatalf("req #%d: client ReadAll: %v"u8, i, errΔ2);
        }
        if (len(all) != 0) {
            Ꮡt.Errorf("req #%d: client got %d bytes; expected 0"u8, i, len(all));
        }
    }
}

public static void TestHandlerPanicNil(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testHandlerPanic(tΔ1, false, mode, default!, default!);
    }, testNotParallel);
}

public static void TestHandlerPanic(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testHandlerPanic(tΔ1, false, mode, default!, intentionalDeathForˢ);
    }, testNotParallel);
}

public static void TestHandlerPanicWithHijack(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Only testing HTTP/1, and our http2 server doesn't support hijacking.
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testHandlerPanic(tΔ1, true, mode, default!, intentionalDeathForˢ);
    }, new testMode[]{http1Mode}.slice());
}

internal static void testHandlerPanic(ж<testing.T> Ꮡt, bool withHijack, testMode mode, Func<httpꓸHandler, httpꓸHandler> wrapper, any panicValue) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Direct log output to a pipe.
        //
        // We read from the pipe to verify that the handler actually caught the panic
        // and logged something.
        //
        // We use a pipe rather than a buffer, because when testing connection hijacking
        // server shutdown doesn't wait for the hijacking handler to return, so the
        // log may occur after the server has shut down.
        var (pr, pw) = io.Pipe();
        var pwʗ1 = pw;
        defer(() => pwʗ1.Close(), ref ᒐ);

        httpꓸHandler handler = new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                if (withHijack) {
                    var (rwc, _, errΔ1) = w._<Hijacker>().Hijack();
                    if (errΔ1 != default!) {
                        Ꮡt.Logf("unexpected error: %v"u8, errΔ1);
                    }
                    var rwcʗ1 = rwc;
                    defer(() => rwcʗ1.Close(), ref ᒐ);
                }
                throw panic(panicValue);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }));
        if (wrapper != default!) {
            handler = wrapper(handler);
        }
        var pwʗ2 = pw;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, handler, (ж<httptest.Server> ts) => {
            ts.Value.Config.Value.ErrorLog = log.New(new io.PipeWriterжWriter(pwʗ2), ""u8, 0);
        });
        // Do a blocking read on the log output pipe.
        var done = new channel<bool>(1);
        var doneʗ1 = done;
        var prʗ1 = pr;
        goǃ(() => {
            var buf = new slice<byte>((4 << (int)(10)));
            var (_, errΔ2) = prʗ1.Read(buf);
            prʗ1.Close();
            if (errΔ2 != default! && !AreEqual(errΔ2, io.EOF)) {
                Ꮡt.Error(errΔ2);
            }
            doneʗ1.ᐸꟷ(true);
        });
        var (_, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err == default!) {
            Ꮡt.Logf("expected an error"u8);
        }
        if (panicValue == default!) {
            return;
        }
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct terrorWriter {
    internal ж<testing.T> t;
}

internal static (nint, error) Write(this terrorWriter w, slice<byte> p) {
    w.t.Errorf("%s"u8, p);
    return (len(p), default!);
}

// Issue 16456: allow writing 0 bytes on hijacked conn to test hijack
// without any log spam.
public static void TestServerWriteHijackZeroBytes(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerWriteHijackZeroBytes(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedWriteˢ = "Unexpected write: "u8;

internal static void testServerWriteHijackZeroBytes(ж<testing.T> Ꮡt, testMode mode) {
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
            w._<Flusher>().Flush();
            var (conn, _, errΔ1) = w._<Hijacker>().Hijack();
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Hijack: %v"u8, errΔ1);
                return;
            }
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            (_, errΔ1) = w.Write(default!);
            if (!AreEqual(errΔ1, ErrHijacked)) {
                Ꮡt.Errorf("Write error = %v; want ErrHijacked"u8, errΔ1);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })), (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Config.Value.ErrorLog = log.New(new terrorWriter(Ꮡt), unexpectedWriteˢ, 0);
    }).Value.ts;
    var c = ts.Client();
    var (res, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    ᐸꟷ(done);
}

public static void TestServerNoDate(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testServerNoHeader(tΔ1, mode, dateˢ);
    });
}

public static void TestServerContentType(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testServerNoHeader(tΔ1, mode, contentTypeˢ);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string htmlFooHtmlˢ = "<html>foo</html>"u8;

internal static void testServerNoHeader(ж<testing.T> Ꮡt, testMode mode, @string header) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header()[header] = default!;
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), htmlFooHtmlˢ); // non-empty
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    {
        var (got, ok) = (~res).Header[header, ꟷ]; if (ok) {
            Ꮡt.Fatalf("Expected no %s header; got %q"u8, header, got);
        }
    }
}

public static void TestStripPrefix(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testStripPrefix(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xPathˢ = "X-Path"u8;
internal static readonly @string xRawPathˢ = "X-RawPath"u8;
internal static readonly @string fooBarˢ2 = "/foo/bar"u8;

[GoType("dyn")] internal partial struct testStripPrefix_cases {
    internal @string reqPath;
    internal @string path; // If empty we want a 404.
    internal @string rawPath;
}

internal static void testStripPrefix(ж<testing.T> Ꮡt, testMode mode) {
    var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(xPathˢ, (~(~r).URL).Path);
        w.Header().Set(xRawPathˢ, (~(~r).URL).RawPath);
    });
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, StripPrefix(fooBarˢ2, new http_test_package.http_HandlerFuncᴠΔHandler(h))).Value.ts;
    var c = ts.Client();
    var cases = new testStripPrefix_cases[]{
        new("/foo/bar/qux"u8, "/qux"u8, ""u8),
        new("/foo/bar%2Fqux"u8, "/qux"u8, "%2Fqux"u8),
        new("/foo%2Fbar/qux"u8, ""u8, ""u8), // Escaped prefix does not match.

        new("/bar"u8, ""u8, ""u8)
    }.slice();
    // No prefix match.
    foreach (var (_, vᴛ1) in cases) {
        ref var tc = ref heap(new testStripPrefix_cases(), out var Ꮡtc);
        tc = vᴛ1;

        var cʗ1 = c;
        var tcʗ1 = tc;
        var tsʗ1 = ts;
        Ꮡt.Run(tc.reqPath, (ж<testing.T> tΔ1) => {
            var (res, err) = cʗ1.Get((~tsʗ1).URL + tcʗ1.reqPath);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (~res).Body.Close();
            if (tcʗ1.path == ""u8) {
                if ((~res).StatusCode != StatusNotFound) {
                    tΔ1.Errorf("got %q, want 404 Not Found"u8, (~res).Status);
                }
                return;
            }
            if ((~res).StatusCode != StatusOK) {
                tΔ1.Fatalf("got %q, want 200 OK"u8, (~res).Status);
            }
            {
                @string g = (~res).Header.Get(xPathˢ);
                @string w = tcʗ1.path; if (g != w) {
                    tΔ1.Errorf("got Path %q, want %q"u8, g, w);
                }
            }
            {
                @string g = (~res).Header.Get(xRawPathˢ);
                @string w = tcʗ1.rawPath; if (g != w) {
                    tΔ1.Errorf("got RawPath %q, want %q"u8, g, w);
                }
            }
        });
    }
}

// https://golang.org/issue/18952.
public static void TestStripPrefixNotModifyRequest(ж<testing.T> Ꮡt) {
    var h = StripPrefix(fooˢ3, NotFoundHandler());
    var req = httptest.NewRequest(getˢ2, fooBarˢ2, default!);
    h.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), req);
    if ((~(~req).URL).Path != "/foo/bar"u8) {
        Ꮡt.Errorf("StripPrefix should not modify the provided Request, but it did"u8);
    }
}

public static void TestRequestLimit(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRequestLimit(Δp0, Δp1));
}

internal static void testRequestLimit(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            Ꮡt.Fatalf("didn't expect to get request in Handler"u8);
        })), (optQuietLog).OrTypedNilFunc());
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        nint bytesPerHeader = len("header12345: val12345\r\n");
        for (nint i = 0; i < ((nint)(DefaultMaxHeaderBytes + 4096) / bytesPerHeader) + 1; i++) {
            (~req).Header.Set(fmt.Sprintf("header%05d"u8, i), fmt.Sprintf("val%05d"u8, i));
        }
        var (res, err) = (~cst).c.Do(req);
        if (res != nil) {
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        }
        if (mode == http2Mode){
            // In HTTP/2, the result depends on a race. If the client has received the
            // server's SETTINGS before RoundTrip starts sending the request, then RoundTrip
            // will fail with an error. Otherwise, the client should receive a 431 from the
            // server.
            if (err == default! && (~res).StatusCode != 431) {
                Ꮡt.Fatalf("expected 431 response status; got: %d %s"u8, (~res).StatusCode, (~res).Status);
            }
        } else {
            // In HTTP/1, we expect a 431 from the server.
            // Some HTTP clients may fail on this undefined behavior (server replying and
            // closing the connection while the request is still being written), but
            // we do support it (at least currently), so we expect a response below.
            if (err != default!) {
                Ꮡt.Fatalf("Do: %v"u8, err);
            }
            if ((~res).StatusCode != 431) {
                Ꮡt.Fatalf("expected 431 response status; got: %d %s"u8, (~res).StatusCode, (~res).Status);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("num:byte")] partial struct neverEnding;

internal static (nint n, error err) Read(this neverEnding b, slice<byte> p) {
    foreach (var (i, _) in p) {
        p[i] = (byte)b;
    }
    return (len(p), default!);
}

[GoType] partial struct bodyLimitReader {
    internal sync.Mutex mu;
    internal nint count;
    internal nint limit;
    internal channel<EmptyStruct> closed;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string closedˢ = "closed"u8;
internal static readonly @string atLimitˢ = "at limit"u8;

internal static (nint, error) Read(this ж<bodyLimitReader> Ꮡr, slice<byte> p) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        ᒐd1 = true;
        var selᴛ45 = r.closed;
        switch (trySelect(ᐸꟷ(selᴛ45, ꓸꓸꓸ))) {
        case 0 when selᴛ45.ꟷᐳ(out _): {
            return (0, errors.New(closedˢ));
        }
        default: {
            break;
        }}
        if (r.count > r.limit) {
            return (0, errors.New(atLimitˢ));
        }
        r.count += len(p);
        foreach (var (i, _) in p) {
            p[i] = (rune)'a';
        }
        return (len(p), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error Close(this ж<bodyLimitReader> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        r.mu.Lock();
        ᒐd1 = true;
        builtin.close(r.closed);
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡr.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

public static void TestRequestBodyLimit(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRequestBodyLimit(Δp0, Δp1));
}

internal static void testRequestBodyLimit(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    UntypedInt limit = /* 1 << 20 */ 1048576;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        r.Value.Body = MaxBytesReader(w, (~r).Body, limit);
        var (n, errΔ1) = io.Copy(io.Discard, (~r).Body);
        if (errΔ1 == default!) {
            Ꮡt.Errorf("expected error from io.Copy"u8);
        }
        if (n != limit) {
            Ꮡt.Errorf("io.Copy = %d, want %d"u8, n, (nint)(limit));
        }
        var (mbErr, ok) = errΔ1._<ж<Δhttp.MaxBytesError>>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("expected MaxBytesError, got %T"u8, errΔ1);
        }
        if ((~mbErr).Limit != limit) {
            Ꮡt.Errorf("MaxBytesError.Limit = %d, want %d"u8, (~mbErr).Limit, (nint)(limit));
        }
    })));
    var body = Ꮡ(new bodyLimitReader(
        closed: new channel<EmptyStruct>(0),
        limit: limit * 200
    ));
    var (req, _) = NewRequest(postˢ, (~(~cst).ts).URL, new http_test_package.bodyLimitReaderжReader(body));
    // Send the POST, but don't care it succeeds or not. The
    // remote side is going to reply and then close the TCP
    // connection, and HTTP doesn't really define if that's
    // allowed or not. Some HTTP clients will get the response
    // and some (like ours, currently) will complain that the
    // request write failed, without reading the response.
    //
    // But that's okay, since what we're really testing is that
    // the remote side hung up on us before we wrote too much.
    var (resp, err) = (~cst).c.Do(req);
    if (err == default!) {
        (~resp).Body.Close();
    }
    // Wait for the Transport to finish writing the request body.
    // It will close the body when done.
    ᐸꟷ((~body).closed);
    if ((~body).count > (nint)(limit * 100)) {
        Ꮡt.Errorf("handler restricted the request body to %d bytes, but client managed to write %d"u8,
            (nint)(limit), (~body).count);
    }
}

// TestClientWriteShutdown tests that if the client shuts down the write
// side of their TCP connection, the server doesn't send a 400 Bad Request.
public static void TestClientWriteShutdown(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientWriteShutdown(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestSeeHttpsˢ = (@string)"skipping test; see https://golang.org/issue/17906"u8;

internal static void testClientWriteShutdown(ж<testing.T> Ꮡt, testMode mode) {
    if (runtime.GOOS == "plan9"u8) {
        Ꮡt.Skip(skippingTestSeeHttpsˢ);
    }
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).Value.ts;
    var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
    if (err != default!) {
        Ꮡt.Fatalf("Dial: %v"u8, err);
    }
    err = conn._<ж<net.TCPConn>>().CloseWrite();
    if (err != default!) {
        Ꮡt.Fatalf("CloseWrite: %v"u8, err);
    }
    (var bs, err) = io.ReadAll(new http_test_package.net_ConnᴠReader(conn));
    if (err != default!) {
        Ꮡt.Errorf("ReadAll: %v"u8, err);
    }
    @string got = ((@string)bs);
    if (got != ""u8) {
        Ꮡt.Errorf("read %q from server; want nothing"u8, got);
    }
}

// Tests that chunked server responses that write 1 byte at a time are
// buffered before chunk headers are added, not after chunk headers.
public static void TestServerBufferedChunking(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var conn = @new<testConn>();
    conn.of(testConn.ᏑreadBuf).Write(slice<byte>("GET / HTTP/1.1\r\nHost: foo\r\n\r\n"u8));
    conn.Value.closec = new channel<bool>(1);
    var ls = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ls), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        rw._<Flusher>().Flush(); // force the Header to be sent, in chunking mode, not counting the length
        rw.Write(new byte[]{(rune)'x'}.slice());
        rw.Write(new byte[]{(rune)'y'}.slice());
        rw.Write(new byte[]{(rune)'z'}.slice());
    })));
    ᐸꟷ((~conn).closec);
    if (!bytes.HasSuffix(conn.of(testConn.ᏑwriteBuf).Bytes(), slice<byte>("\r\n\r\n3\r\nxyz\r\n0\r\n\r\n"u8))) {
        Ꮡt.Errorf("response didn't end with a single 3 byte 'xyz' chunk; got:\n%q"u8,
            conn.of(testConn.ᏑwriteBuf).Bytes());
    }
}

// Tests that the server flushes its response headers out when it's
// ignoring the response body and waits a bit before forcefully
// closing the TCP connection, causing the client to get a RST.
// See https://golang.org/issue/3595
public static void TestServerGracefulClose(ж<testing.T> Ꮡt) {
    // Not parallel: modifies the global rstAvoidanceDelay.
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerGracefulClose(Δp0, Δp1), new testMode[]{http1Mode}.slice(), testNotParallel);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string byeˢ = "bye"u8;
internal static readonly @string unauthorizedˢ = "401 Unauthorized"u8;

internal static void testServerGracefulClose(ж<testing.T> Ꮡt, testMode mode) {
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
            const nint bodySize = /* 5 << 20 */ 5242880;
            ref var req = ref heap<slice<byte>>(out var Ꮡreq);
            Ꮡreq.ValueSlot = slice<byte>(fmt.Sprintf("POST / HTTP/1.1\r\nHost: foo.com\r\nContent-Length: %d\r\n\r\n"u8, (nint)(bodySize)));
            for (nint i = 0; i < bodySize; i++) {
                Ꮡreq.ValueSlot = append(Ꮡreq.ValueSlot, (byte)((rune)'x'));
            }
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                Error(w, byeˢ, StatusUnauthorized);
            })));
            // We need to close cst explicitly here so that in-flight server
            // requests don't race with the call to SetRSTAvoidanceDelay for a retry.
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
            if (err != default!) {
                return err;
            }
            var writeErr = new channel<error>(0);
            var connʗ1 = conn;
            var writeErrʗ1 = writeErr;
            goǃ(() => {
                var (_, errΔ1) = connʗ1.Write(Ꮡreq.ValueSlot);
                writeErrʗ1.ᐸꟷ(errΔ1);
            });
            var connʗ2 = conn;
            var writeErrʗ2 = writeErr;
            defer(() => {
                connʗ2.Close();
                // Wait for write to finish. This is a broken pipe on both
                // Darwin and Linux, but checking this isn't the point of
                // the test.
                ᐸꟷ(writeErrʗ2);
            }, ref ᒐ);
            var br = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
            nint lineNum = 0;
            while (ᐧ) {
                var (line, errΔ2) = br.ReadString((rune)'\n');
                if (AreEqual(errΔ2, io.EOF)) {
                    break;
                }
                if (errΔ2 != default!) {
                    return fmt.Errorf("ReadLine: %v"u8, errΔ2);
                }
                lineNum++;
                if (lineNum == 1 && !strings.Contains(line, unauthorizedˢ)) {
                    tΔ1.Errorf("Response line = %q; want a 401"u8, line);
                }
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

public static void TestCaseSensitiveMethod(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testCaseSensitiveMethod(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ3 = "get"u8;

internal static void testCaseSensitiveMethod(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~r).Method != "get"u8) {
                Ꮡt.Errorf(@"Got method %q; want ""get"""u8, (~r).Method);
            }
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var (req, _) = NewRequest(getˢ3, (~(~cst).ts).URL, default!);
        var (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestContentLengthZero tests that for both an HTTP/1.0 and HTTP/1.1
// request (both keep-alive), when a Handler never writes any
// response, the net/http package adds a "Content-Length: 0" response
// header.
public static void TestContentLengthZero(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testContentLengthZero(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testContentLengthZero(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
    }))).Value.ts;
    foreach (var (_, version) in new @string[]{"HTTP/1.0"u8, "HTTP/1.1"u8}.slice()) {
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("error dialing: %v"u8, err);
        }
        (_, err) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(conn), "GET / %v\r\nConnection: keep-alive\r\nHost: foo\r\n\r\n"u8, version);
        if (err != default!) {
            Ꮡt.Fatalf("error writing: %v"u8, err);
        }
        var (req, _) = NewRequest(getˢ2, "/"u8, default!);
        (var res, err) = ReadResponse(bufio.NewReader(new http_test_package.net_ConnᴠReader(conn)), req);
        if (err != default!) {
            Ꮡt.Fatalf("error reading response: %v"u8, err);
        }
        {
            var te = res.Value.TransferEncoding; if (len(te) > 0) {
                Ꮡt.Errorf("For version %q, Transfer-Encoding = %q; want none"u8, version, te);
            }
        }
        {
            var cl = res.Value.ContentLength; if (cl != 0) {
                Ꮡt.Errorf("For version %q, Content-Length = %v; want 0"u8, version, cl);
            }
        }
        conn.Close();
    }
}

public static void TestCloseNotifier(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testCloseNotifier(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testCloseNotifier(ж<testing.T> Ꮡt, testMode mode) {
    var gotReq = new channel<bool>(1);
    var sawClose = new channel<bool>(1);
    var gotReqʗ1 = gotReq;
    var sawCloseʗ1 = sawClose;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        gotReqʗ1.ᐸꟷ(true);
        var cc = rw._<CloseNotifier>().CloseNotify();
        ᐸꟷ(cc);
        sawCloseʗ1.ᐸꟷ(true);
    }))).Value.ts;
    ref var err = ref heap<error>(out var Ꮡerr);
    (var conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
    if (err != default!) {
        Ꮡt.Fatalf("error dialing: %v"u8, err);
    }
    var diec = new channel<bool>(0);
    var connʗ1 = conn;
    var diecʗ1 = diec;
    goǃ(() => {
        (_, Ꮡerr.ValueSlot) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(connʗ1), "GET / HTTP/1.1\r\nConnection: keep-alive\r\nHost: foo\r\n\r\n"u8);
        if (Ꮡerr.ValueSlot != default!) {
            Ꮡt.Error(Ꮡerr.ValueSlot);
            return;
        }
        ᐸꟷ(diecʗ1);
        connʗ1.Close();
    });
For:
    while (ᐧ) {
        var selᴛ46 = gotReq;
        var selᴛ47 = sawClose;
        switch (select(ᐸꟷ(selᴛ46, ꓸꓸꓸ), ᐸꟷ(selᴛ47, ꓸꓸꓸ))) {
        case 0 when selᴛ46.ꟷᐳ(out _): {
            diec.ᐸꟷ(true);
            break;
        }
        case 1 when selᴛ47.ꟷᐳ(out _): {
            goto break_For;
            break;
        }}
continue_For:;
    }
break_For:;
    ts.Close();
}

// Tests that a pipelined request does not cause the first request's
// Handler's CloseNotify channel to fire.
//
// Issue 13165 (where it used to deadlock), but behavior changed in Issue 23921.
public static void TestCloseNotifierPipelined(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testCloseNotifierPipelined(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedCloseNotifyˢ = (@string)"unexpected CloseNotify"u8;
internal static readonly object tooManyRequestsˢ = (@string)"too many requests"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string reqᶜ = "GET / HTTP/1.1\r\nConnection: keep-alive\r\nHost: foo\r\n\r\n"u8;

internal static void testCloseNotifierPipelined(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var gotReq = new channel<bool>(2);
        var sawClose = new channel<bool>(2);
        var gotReqʗ1 = gotReq;
        var sawCloseʗ1 = sawClose;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
            gotReqʗ1.ᐸꟷ(true);
            var cc = rw._<CloseNotifier>().CloseNotify();
            var selᴛ48 = cc;
            var selᴛ49 = time.After(100 * time.Millisecond);
            switch (select(ᐸꟷ(selᴛ48, ꓸꓸꓸ), ᐸꟷ(selᴛ49, ꓸꓸꓸ))) {
            case 0 when selᴛ48.ꟷᐳ(out _): {
                Ꮡt.Error(unexpectedCloseNotifyˢ);
                break;
            }
            case 1 when selᴛ49.ꟷᐳ(out _): {
                break;
            }}
            sawCloseʗ1.ᐸꟷ(true);
        }))).Value.ts;
        ref var err = ref heap<error>(out var Ꮡerr);
        (var conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("error dialing: %v"u8, err);
        }
        var diec = new channel<bool>(1);
        defer(ᴛ1 => builtin.close(ᴛ1), diec, ref ᒐ);
        var connʗ1 = conn;
        var diecʗ1 = diec;
        goǃ(() => {
            @string req = reqᶜ;
            (_, Ꮡerr.ValueSlot) = io.WriteString(new http_test_package.net_ConnᴠWriter(connʗ1), req + req); // two requests
            if (Ꮡerr.ValueSlot != default!) {
                Ꮡt.Error(Ꮡerr.ValueSlot);
                return;
            }
            ᐸꟷ(diecʗ1);
            connʗ1.Close();
        });
        nint reqs = 0;
        nint closes = 0;
        while (ᐧ) {
            var selᴛ50 = gotReq;
            var selᴛ51 = sawClose;
            switch (select(ᐸꟷ(selᴛ50, ꓸꓸꓸ), ᐸꟷ(selᴛ51, ꓸꓸꓸ))) {
            case 0 when selᴛ50.ꟷᐳ(out _): {
                reqs++;
                if (reqs > 2) {
                    Ꮡt.Fatal(tooManyRequestsˢ);
                }
                break;
            }
            case 1 when selᴛ51.ꟷᐳ(out _): {
                closes++;
                if (closes > 1) {
                    return;
                }
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10HostGolangOrgˢ = "GET / HTTP/1.0\nHost: golang.org"u8;

public static void TestCloseNotifierChanLeak(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var req = reqBytes(getHttp10HostGolangOrgˢ);
        for (nint i = 0; i < 20; i++) {
            ref var output = ref heap(new bytes.Buffer(), out var Ꮡoutput);
            var conn = Ꮡ(new rwTestConn(
                Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(req)),
                Writer: new http_test_package.bytes_BufferжWriter(Ꮡoutput),
                closec: new channel<bool>(1)
            ));
            var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
            var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                // Ignore the return value and never read from
                // it, testing that we don't leak goroutines
                // on the sending side:
                _ = rw._<CloseNotifier>().CloseNotify();
            });
            goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
            ᐸꟷ((~conn).closec);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that we can use CloseNotifier in one request, and later call Hijack
// on a second request on the same connection.
//
// It also tests that the connReader stitches together its background
// 1-byte read for CloseNotifier when CloseNotifier doesn't fire with
// the rest of the second HTTP later.
//
// Issue 9763.
// HTTP/1-only test. (http2 doesn't have Hijack)
public static void TestHijackAfterCloseNotifier(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHijackAfterCloseNotifier(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string closenotifyˢ = "closenotify"u8;
internal static readonly @string hijackˢ2 = "hijack"u8;

internal static void testHijackAfterCloseNotifier(ж<testing.T> Ꮡt, testMode mode) {
    var script = new channel<@string>(2);
    script.ᐸꟷ(closenotifyˢ);
    script.ᐸꟷ(hijackˢ2);
    builtin.close(script);
    var scriptʗ1 = script;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        @string plan = ᐸꟷ(scriptʗ1);
        var exprᴛ1 = plan;
        if (exprᴛ1 == "closenotify"u8) {
            w._<CloseNotifier>().CloseNotify(); // discard result
            w.Header().Set(xAddrˢ, (~r).RemoteAddr);
        }
        else if (exprᴛ1 == "hijack"u8) {
            var (c, _, errΔ2) = w._<Hijacker>().Hijack();
            if (errΔ2 != default!) {
                Ꮡt.Errorf("Hijack in Handler: %v"u8, errΔ2);
                return;
            }
            {
                var (_, ok) = c._<ж<net.TCPConn>>(ᐧ); if (!ok) {
                    // Verify it's not wrapped in some type.
                    // Not strictly a go1 compat issue, but in practice it probably is.
                    Ꮡt.Errorf("type of hijacked conn is %T; want *net.TCPConn"u8, c);
                }
            }
            fmt.Fprintf(new http_test_package.net_ConnᴠWriter(c), "HTTP/1.0 200 OK\r\nX-Addr: %v\r\nContent-Length: 0\r\n\r\n"u8, (~r).RemoteAddr);
            c.Close();
            return;
        }
        else { /* default: */
            throw panic("bogus plan; too many requests");
        }

    }))).Value.ts;
    var (res1, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        log.Fatal(err);
    }
    (var res2, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        log.Fatal(err);
    }
    @string addr1 = (~res1).Header.Get(xAddrˢ);
    @string addr2 = (~res2).Header.Get(xAddrˢ);
    if (addr1 == ""u8 || addr1 != addr2) {
        Ꮡt.Errorf("addr1, addr2 = %q, %q; want same"u8, addr1, addr2);
    }
}

public static void TestHijackBeforeRequestBodyRead(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHijackBeforeRequestBodyRead(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object backendReadWrongRequestˢ = (@string)"Backend read wrong request body."u8;

internal static void testHijackBeforeRequestBodyRead(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        slice<byte> requestBody = bytes.Repeat(slice<byte>("a"u8), (1 << (int)(20)));
        var bodyOkay = new channel<bool>(1);
        var gotCloseNotify = new channel<bool>(1);
        var bodyOkayʗ1 = bodyOkay;
        var gotCloseNotifyʗ1 = gotCloseNotify;
        var requestBodyʗ1 = requestBody;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), bodyOkayʗ1, ref ᒐ); // caller will read false if nothing else
                var reqBody = r.Value.Body;
                r.Value.Body = default!; // to test that server.go doesn't use this value.
                var gone = w._<CloseNotifier>().CloseNotify();
                var (slurp, errΔ1) = io.ReadAll(reqBody);
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("Body read: %v"u8, errΔ1);
                    return;
                }
                if (len(slurp) != len(requestBodyʗ1)) {
                    Ꮡt.Errorf("Backend read %d request body bytes; want %d"u8, len(slurp), len(requestBodyʗ1));
                    return;
                }
                if (!bytes.Equal(slurp, requestBodyʗ1)) {
                    Ꮡt.Error(backendReadWrongRequestˢ); // 1MB; omitting details
                    return;
                }
                bodyOkayʗ1.ᐸꟷ(true);
                ᐸꟷ(gone);
                gotCloseNotifyʗ1.ᐸꟷ(true);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        fmt.Fprintf(new http_test_package.net_ConnᴠWriter(conn), "POST / HTTP/1.1\r\nHost: foo\r\nContent-Length: %d\r\n\r\n%s"u8,
            len(requestBody), requestBody);
        if (!ᐸꟷ(bodyOkay)) {
            // already failed.
            return;
        }
        conn.Close();
        ᐸꟷ(gotCloseNotify);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestOptions(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testOptions(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testOptions(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var uric = new channel<@string>(2); // only expect 1, but leave space for 2
        var mux = NewServeMux();
        var uricʗ1 = uric;
        mux.HandleFunc("/"u8, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            uricʗ1.ᐸꟷ((~r).RequestURI);
        });
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        // An OPTIONS * request should succeed.
        (_, err) = conn.Write(slice<byte>("OPTIONS * HTTP/1.1\r\nHost: foo.com\r\n\r\n"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var br = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
        (var res, err) = ReadResponse(br, Ꮡ(new Request(Method: "OPTIONS"u8)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != 200) {
            Ꮡt.Errorf("Got non-200 response to OPTIONS *: %#v"u8, res.OrTypedNil());
        }
        // A GET * request on a ServeMux should fail.
        (_, err) = conn.Write(slice<byte>("GET * HTTP/1.1\r\nHost: foo.com\r\n\r\n"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (res, err) = ReadResponse(br, Ꮡ(new Request(Method: "GET"u8)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != 400) {
            Ꮡt.Errorf("Got non-400 response to GET *: %#v"u8, res.OrTypedNil());
        }
        (res, err) = Get((~ts).URL + "/second"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~res).Body.Close();
        {
            @string got = ᐸꟷ(uric); if (got != "/second"u8) {
                Ꮡt.Errorf("Handler saw request for %q; want /second"u8, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestOptionsHandler(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testOptionsHandler(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testOptionsHandler(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var rc = new channel<ж<Δhttp.Request>>(1);
        var rcʗ1 = rc;

        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            rcʗ1.ᐸꟷ(r);
        })), (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.DisableGeneralOptionsHandler = true;
        }).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        (_, err) = conn.Write(slice<byte>("OPTIONS * HTTP/1.1\r\nHost: foo.com\r\n\r\n"u8));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var got = ᐸꟷ(rc); if ((~got).Method != "OPTIONS"u8 || (~got).RequestURI != "*"u8) {
                Ꮡt.Errorf("Expected OPTIONS * request, got %v"u8, got.OrTypedNil());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentLengthˢ2 = "Content-Length:"u8;
internal static readonly @string noContentLengthˢ = "no content-length"u8;
internal static readonly @string contentTypeTextPlainˢ = "Content-Type: text/plain"u8;
internal static readonly @string noContentTypeˢ = "no content-type"u8;
internal static readonly @string someTypeˢ = "some/type"u8;
internal static readonly @string tooLateˢ = "Too-Late"u8;
internal static readonly @string bogusˢ = "bogus"u8;
internal static readonly @string contentTypeSomeTypeˢ = "Content-Type: some/type"u8;
internal static readonly @string wrongContentTypeˢ = "wrong content-type"u8;
internal static readonly @string donTWantTooLateHeaderˢ = "don't want too-late header"u8;
internal static readonly @string writeAlreadyWroteHeadersˢ = "Write already wrote headers"u8;
internal static readonly @string headerAppearedFromAfterˢ = "header appeared from after WriteHeader"u8;
internal static readonly @string notChunkedˢ = "not chunked"u8;
internal static readonly @string xWrongˢ = "x/wrong"u8;
internal static readonly @string contentTypeTextHtmlˢ = "Content-Type: text/html"u8;
internal static readonly @string wrongContentTypeWantHtmlˢ = "wrong content-type; want html"u8;
internal static readonly @string contentLength0ˢ = "Content-Length: 0"u8;
internal static readonly @string want0ContentLengthˢ = "want 0 content-length"u8;
internal static readonly @string someHeaderˢ = "Some-Header"u8;
internal static readonly @string someValueˢ = "some-value"u8;
internal static readonly @string didnTGetHeaderˢ = "didn't get header"u8;
internal static readonly @string wrongStatusˢ = "wrong status"u8;
internal static readonly @string shouldnTHaveSeenTooLateˢ = "shouldn't have seen Too-Late"u8;
internal static readonly @string getHttp11HostGolangOrgˢ = "GET / HTTP/1.1\nHost: golang.org"u8;

[GoType("dyn")] internal partial struct TestHeaderToWire_tests {
    internal @string name;
    internal Action<Δhttp.ResponseWriter, ж<Δhttp.Request>> handler;
    internal Func<@string, @string, error> check;
}

// Tests regarding the ordering of Write, WriteHeader, Header, and
// Flush calls. In Go 1.0, rw.WriteHeader immediately flushed the
// (*response).header to the wire. In Go 1.1, the actual wire flush is
// delayed, so we could maybe tack on a Content-Length and better
// Content-Type after we see more (or all) of the output. To preserve
// compatibility with Go 1, we need to be careful to track which
// headers were live at the time of WriteHeader, so we write the same
// ones, even if the handler modifies them (~erroneously) after the
// first Write.
public static void TestHeaderToWire(ж<testing.T> Ꮡt) {
    var tests = new TestHeaderToWire_tests[]{
        new(
            name: "write without Header"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.Write(slice<byte>("hello world"u8));
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, contentLengthˢ2)) {
                    return errors.New(noContentLengthˢ);
                }
                if (!strings.Contains(got, contentTypeTextPlainˢ)) {
                    return errors.New(noContentTypeˢ);
                }
                return default!;
            }
        ),
        new(
            name: "Header mutation before write"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                var h = rw.Header();
                h.Set(contentTypeˢ, someTypeˢ);
                rw.Write(slice<byte>("hello world"u8));
                h.Set(tooLateˢ, bogusˢ);
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, contentLengthˢ2)) {
                    return errors.New(noContentLengthˢ);
                }
                if (!strings.Contains(got, contentTypeSomeTypeˢ)) {
                    return errors.New(wrongContentTypeˢ);
                }
                if (strings.Contains(got, tooLateˢ)) {
                    return errors.New(donTWantTooLateHeaderˢ);
                }
                return default!;
            }
        ),
        new(
            name: "write then useless Header mutation"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.Write(slice<byte>("hello world"u8));
                rw.Header().Set(tooLateˢ, writeAlreadyWroteHeadersˢ);
            },
            check: error (@string got, @string logs) => {
                if (strings.Contains(got, tooLateˢ)) {
                    return errors.New(headerAppearedFromAfterˢ);
                }
                return default!;
            }
        ),
        new(
            name: "flush then write"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw._<Flusher>().Flush();
                rw.Write(slice<byte>("post-flush"u8));
                rw.Header().Set(tooLateˢ, writeAlreadyWroteHeadersˢ);
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, transferEncodingChunkedˢ)) {
                    return errors.New(notChunkedˢ);
                }
                if (strings.Contains(got, tooLateˢ)) {
                    return errors.New(headerAppearedFromAfterˢ);
                }
                return default!;
            }
        ),
        new(
            name: "header then flush"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.Header().Set(contentTypeˢ, someTypeˢ);
                rw._<Flusher>().Flush();
                rw.Write(slice<byte>("post-flush"u8));
                rw.Header().Set(tooLateˢ, writeAlreadyWroteHeadersˢ);
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, transferEncodingChunkedˢ)) {
                    return errors.New(notChunkedˢ);
                }
                if (strings.Contains(got, tooLateˢ)) {
                    return errors.New(headerAppearedFromAfterˢ);
                }
                if (!strings.Contains(got, contentTypeSomeTypeˢ)) {
                    return errors.New(wrongContentTypeˢ);
                }
                return default!;
            }
        ),
        new(
            name: "sniff-on-first-write content-type"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.Write(slice<byte>("<html><head></head><body>some html</body></html>"u8));
                rw.Header().Set(contentTypeˢ, xWrongˢ);
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, contentTypeTextHtmlˢ)) {
                    return errors.New(wrongContentTypeWantHtmlˢ);
                }
                return default!;
            }
        ),
        new(
            name: "explicit content-type wins"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.Header().Set(contentTypeˢ, someTypeˢ);
                rw.Write(slice<byte>("<html><head></head><body>some html</body></html>"u8));
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, contentTypeSomeTypeˢ)) {
                    return errors.New(wrongContentTypeWantHtmlˢ);
                }
                return default!;
            }
        ),
        new(
            name: "empty handler"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, contentLength0ˢ)) {
                    return errors.New(want0ContentLengthˢ);
                }
                return default!;
            }
        ),
        new(
            name: "only Header, no write"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.Header().Set(someHeaderˢ, someValueˢ);
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, someHeaderˢ)) {
                    return errors.New(didnTGetHeaderˢ);
                }
                return default!;
            }
        ),
        new(
            name: "WriteHeader call"u8,
            handler: (Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                rw.WriteHeader(404);
                rw.Header().Set(tooLateˢ, someValueˢ);
            },
            check: error (@string got, @string logs) => {
                if (!strings.Contains(got, "404"u8)) {
                    return errors.New(wrongStatusˢ);
                }
                if (strings.Contains(got, tooLateˢ)) {
                    return errors.New(shouldnTHaveSeenTooLateˢ);
                }
                return default!;
            }
        )
    }.slice();
    foreach (var (_, tc) in tests) {
        ref var ht = ref heap<handlerTest>(out var Ꮡht);
        ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(tc.handler)));
        @string got = Ꮡht.rawResponse(getHttp11HostGolangOrgˢ);
        @string logs = Ꮡht.of(handlerTest.Ꮡlogbuf).String();
        {
            var err = tc.check(got, logs); if (err != default!) {
                Ꮡt.Errorf("%s: %v\nGot response:\n%s\n\n%s"u8, tc.name, err, got, logs);
            }
        }
    }
}

[GoType] partial struct errorListener {
    internal slice<error> errs;
}

[GoRecv] internal static (net.Conn c, error err) Accept(this ref errorListener l) {
    net.Conn c = default!;
    error err = default!;

    if (len(l.errs) == 0) {
        return (default!, io.EOF);
    }
    err = l.errs[0];
    l.errs = l.errs[1..];
    return (c, err);
}

[GoRecv] internal static error Close(this ref errorListener l) {
    return default!;
}

[GoRecv] internal static netꓸAddr Addr(this ref errorListener l) {
    return ((dummyAddr)(@string)testAddressˢ);
}

public static void TestAcceptMaxFds(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var ln = Ꮡ(new errorListener(new error[]{new net.OpErrorжerror(Ꮡ(new net.OpError(
        Op: "accept"u8,
        Err: syscall.EMFILE
    )))
    }.slice()
    ));
    var server = Ꮡ(new Server(
        Handler: new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Δhttp.HandlerFunc>(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _Δp0, ж<Δhttp.Request> _Δp1) => {
        }))),
        ErrorLog: log.New(io.Discard, ""u8, 0)
    ));
    // noisy otherwise
    var err = server.Serve(new http_test_package.errorListenerжListener(ln));
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Errorf("got error %v, want EOF"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hijackToBufwHijackToConnˢ = "[hijack-to-bufw][hijack-to-conn]"u8;

public static void TestWriteAfterHijack(ж<testing.T> Ꮡt) {
    var req = reqBytes(getHttp11HostGolangOrgˢ);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var wrotec = new channel<bool>(1);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(req)),
        Writer: new http_test_package.strings_BuilderжWriter(Ꮡbuf),
        closec: new channel<bool>(1)
    ));
    var wrotecʗ1 = wrotec;
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        var (connΔ1, bufrw, err) = rw._<Hijacker>().Hijack();
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        var bufrwʗ1 = bufrw;
        var connʗ1 = connΔ1;
        var wrotecʗ2 = wrotecʗ1;
        goǃ(() => {
            bufrwʗ1.Value.Writer.Value.Write(slice<byte>("[hijack-to-bufw]"u8));
            bufrwʗ1.Value.Writer.Value.Flush();
            connʗ1.Write(slice<byte>("[hijack-to-conn]"u8));
            connʗ1.Close();
            wrotecʗ2.ᐸꟷ(true);
        });
    });
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
    ᐸꟷ((~conn).closec);
    ᐸꟷ(wrotec);
    {
        @string g = buf.String();
        @string w = hijackToBufwHijackToConnˢ; if (g != w) {
            Ꮡt.Errorf("wrote %q; want %q"u8, g, w);
        }
    }
}

public static void TestDoubleHijack(ж<testing.T> Ꮡt) {
    var req = reqBytes(getHttp11HostGolangOrgˢ);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(req)),
        Writer: new http_test_package.bytes_BufferжWriter(Ꮡbuf),
        closec: new channel<bool>(1)
    ));
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        var (connΔ1, _, err) = rw._<Hijacker>().Hijack();
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        (_, _, err) = rw._<Hijacker>().Hijack();
        if (err == default!) {
            Ꮡt.Errorf("got err = nil;  want err != nil"u8);
        }
        connΔ1.Close();
    });
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
    ᐸꟷ((~conn).closec);
}

// https://golang.org/issue/5955
// Note that this does not test the "request too large"
// exit path from the http server. This is intentional;
// not sending Connection: close is just a minor wire
// optimization and is pointless if dealing with a
// badly behaved client.
public static void TestHTTP10ConnectionHeader(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHTTP10ConnectionHeader(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object dialErrˢ = (@string)"dial err:"u8;
internal static readonly object connWriteErrˢ = (@string)"conn write err:"u8;
internal static readonly object readResponseErrˢ = (@string)"ReadResponse err:"u8;

[GoType("dyn")] internal partial struct testHTTP10ConnectionHeader_tests {
    internal @string req;  // raw http request
    internal slice<@string> expect; // expected Connection header(s)
}

internal static void testHTTP10ConnectionHeader(ж<testing.T> Ꮡt, testMode mode) {
    var mux = NewServeMux();
    mux.Handle("/"u8, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _Δp0, ж<Δhttp.Request> _Δp1) => {
    })));
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new Δhttp.ServeMuxжΔHandler(mux)).Value.ts;
    // net/http uses HTTP/1.1 for requests, so write requests manually
    var tests = new testHTTP10ConnectionHeader_tests[]{
        new(
            req: "GET / HTTP/1.0\r\n\r\n"u8,
            expect: default!
        ),
        new(
            req: "OPTIONS * HTTP/1.0\r\n\r\n"u8,
            expect: default!
        ),
        new(
            req: "GET / HTTP/1.0\r\nConnection: keep-alive\r\n\r\n"u8,
            expect: new @string[]{"keep-alive"u8}.slice()
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(dialErrˢ, err);
        }
        (_, err) = fmt.Fprint(new http_test_package.net_ConnᴠWriter(conn), tt.req);
        if (err != default!) {
            Ꮡt.Fatal(connWriteErrˢ, err);
        }
        (var resp, err) = ReadResponse(bufio.NewReader(new http_test_package.net_ConnᴠReader(conn)), Ꮡ(new Request(Method: "GET"u8)));
        if (err != default!) {
            Ꮡt.Fatal(readResponseErrˢ, err);
        }
        conn.Close();
        (~resp).Body.Close();
        var got = (~resp).Header[connectionˢ];
        if (!slices.Equal<slice<@string>, @string>(got, tt.expect)) {
            Ꮡt.Errorf("wrong Connection headers for request %q. Got %q expect %q"u8, tt.req, got, tt.expect);
        }
    }
}

// See golang.org/issue/5660
public static void TestServerReaderFromOrder(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerReaderFromOrder(Δp0, Δp1));
}

internal static void testServerReaderFromOrder(ж<testing.T> Ꮡt, testMode mode) {
    var (pr, pw) = io.Pipe();
    UntypedInt size = /* 3 << 20 */ 3145728;
    var prʗ1 = pr;
    var pwʗ1 = pw;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ1) => {
        rw.Header().Set(contentTypeˢ, textPlainˢ); // prevent sniffing path
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        var prʗ2 = prʗ1;
        goǃ(() => {
            io.Copy(new http_test_package.http_ResponseWriterᴠWriter(rw), new io.PipeReaderжReader(prʗ2));
            builtin.close(doneʗ1);
        });
        time.Sleep(25 * time.Millisecond); // give Copy a chance to break things
        var (n, errΔ1) = io.Copy(io.Discard, (~reqΔ1).Body);
        if (errΔ1 != default!) {
            Ꮡt.Errorf("handler Copy: %v"u8, errΔ1);
            return;
        }
        if (n != size) {
            Ꮡt.Errorf("handler Copy = %d; want %d"u8, n, (nint)(size));
        }
        pwʗ1.Write(slice<byte>("hi"u8));
        pwʗ1.Close();
        ᐸꟷ(done);
    })));
    var (req, err) = NewRequest(postˢ, (~(~cst).ts).URL, io.LimitReader(((neverEnding)(rune)'a'), size));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var res, err) = (~cst).c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var all, err) = io.ReadAll((~res).Body);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    if (((sstring)all) != "hi"u8) {
        Ꮡt.Errorf("Body = %q; want hi"u8, all);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stuffˢ2 = "stuff"u8;

// Issue 6157, Issue 6685
public static void TestCodesPreventingContentTypeAndBody(ж<testing.T> Ꮡt) {
    foreach (var (_, code) in new nint[]{StatusNotModified, StatusNoContent}.slice()) {
        ref var ht = ref heap<handlerTest>(out var Ꮡht);
        ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~(~r).URL).Path == "/header"u8) {
                w.Header().Set(contentLengthˢ, "123"u8);
            }
            w.WriteHeader(code);
            if ((~(~r).URL).Path == "/more"u8) {
                w.Write(slice<byte>("stuff"u8));
            }
        })));
        foreach (var (_, req) in new @string[]{
            "GET / HTTP/1.0"u8,
            "GET /header HTTP/1.0"u8,
            "GET /more HTTP/1.0"u8,
            "GET / HTTP/1.1\nHost: foo"u8,
            "GET /header HTTP/1.1\nHost: foo"u8,
            "GET /more HTTP/1.1\nHost: foo"u8
        }.slice()) {
            @string got = Ꮡht.rawResponse(req);
            @string wantStatus = fmt.Sprintf("%d %s"u8, code, StatusText(code));
            if (!strings.Contains(got, wantStatus)){
                Ꮡt.Errorf("Code %d: Wanted %q Modified for %q: %s"u8, code, wantStatus, req, got);
            } else 
            if (strings.Contains(got, contentLengthˢ)){
                Ꮡt.Errorf("Code %d: Got a Content-Length from %q: %s"u8, code, req, got);
            } else 
            if (strings.Contains(got, stuffˢ2)) {
                Ꮡt.Errorf("Code %d: Response contains a body from %q: %s"u8, code, req, got);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBarˢ3 = "foo/bar"u8;
internal static readonly @string getHttp11HostFooˢ4 = "GET / HTTP/1.1\nHost: foo"u8;
internal static readonly @string contentTypeFooBarˢ = "Content-Type: foo/bar"u8;
internal static readonly @string contentLength123ˢ = "Content-Length: 123"u8;

public static void TestContentTypeOkayOn204(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var ht = ref heap<handlerTest>(out var Ꮡht);
    ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentLengthˢ, "123"u8); // suppressed
        w.Header().Set(contentTypeˢ, fooBarˢ3);
        w.WriteHeader(204);
    })));
    @string got = Ꮡht.rawResponse(getHttp11HostFooˢ4);
    if (!strings.Contains(got, contentTypeFooBarˢ)) {
        Ꮡt.Errorf("Response = %q; want Content-Type: foo/bar"u8, got);
    }
    if (strings.Contains(got, contentLength123ˢ)) {
        Ꮡt.Errorf("Response = %q; don't want a Content-Length"u8, got);
    }
}

// Issue 6995
// A server Handler can receive a Request, and then turn around and
// give a copy of that Request.Body out to the Transport (e.g. any
// proxy).  So then two people own that Request.Body (both the server
// and the http client), and both think they can close it on failure.
// Therefore, all incoming server requests Bodies need to be thread-safe.
public static void TestTransportAndServerSharedBodyRace(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportAndServerSharedBodyRace(Δp0, Δp1), testNotParallel);
}

internal static void testTransportAndServerSharedBodyRace(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    // The proxy server in the middle of the stack for this test potentially
    // from its handler after only reading half of the body.
    // That can trigger https://go.dev/issue/3595, which is otherwise
    // irrelevant to this test.
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
            UntypedInt bodySize = /* 1 << 20 */ 1048576;
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            var backend = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ1) => {
                GoFrame ᒐ = default;
                try {
                    // Work around https://go.dev/issue/38370: clientServerTest uses
                    // an httptest.Server under the hood, and in HTTP/2 mode it does not always
                    // “[block] until all outstanding requests on this server have completed”,
                    // causing the call to Logf below to race with the end of the test.
                    //
                    // Since the client doesn't cancel the request until we have copied half
                    // the body, this call to add happens before the test is cleaned up,
                    // preventing the race.
                    Ꮡwg.Add(1);
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (n, errΔ1) = io.CopyN(new http_test_package.http_ResponseWriterᴠWriter(rw), (~reqΔ1).Body, bodySize);
                    tΔ1.Logf("backend CopyN: %v, %v"u8, n, errΔ1);
                    ᐸꟷ(reqΔ1.Context().Done());
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            })));
            // We need to close explicitly here so that in-flight server
            // requests don't race with the call to SetRSTAvoidanceDelay for a retry.
            var backendʗ1 = backend;
            defer(() => {
                Ꮡwg.Wait();
                backendʗ1.close();
            }, ref ᒐ);
            ref var proxy = ref heap<ж<clientServerTest>>(out var Ꮡproxy);
            var backendʗ2 = backend;
            Ꮡproxy.ValueSlot = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ2) => {
                var (req2, _) = NewRequest(postˢ, (~(~backendʗ2).ts).URL, (~reqΔ2).Body);
                req2.Value.ContentLength = bodySize;
                var cancel = new channel<EmptyStruct>(0);
                req2.Value.Cancel = cancel.WithDirection(GoChanDir.Recv);
                var (bresp, errΔ2) = (~Ꮡproxy.ValueSlot).c.Do(req2);
                if (errΔ2 != default!) {
                    tΔ1.Errorf("Proxy outbound request: %v"u8, errΔ2);
                    return;
                }
                (_, errΔ2) = io.CopyN(io.Discard, (~bresp).Body, bodySize / 2);
                if (errΔ2 != default!) {
                    tΔ1.Errorf("Proxy copy error: %v"u8, errΔ2);
                    return;
                }
                var brespʗ1 = bresp;
                tΔ1.Cleanup(() => {
                    (~brespʗ1).Body.Close();
                });
                // Try to cause a race. Canceling the client request will cause the client
                // transport to close req2.Body. Returning from the server handler will
                // cause the server to close req.Body. Since they are the same underlying
                // ReadCloser, that will result in concurrent calls to Close (and possibly a
                // Read concurrent with a Close).
                if (mode == http2Mode){
                    builtin.close(cancel);
                } else {
                    (~(~Ꮡproxy.ValueSlot).c).Transport._<ж<Δhttp.Transport>>().CancelRequest(req2);
                }
                rw.Write(slice<byte>("OK"u8));
            })));
            defer(Ꮡproxy.ValueSlot.close, ref ᒐ);
            var (req, _) = NewRequest(postˢ, (~(~Ꮡproxy.ValueSlot).ts).URL, io.LimitReader(((neverEnding)(rune)'a'), bodySize));
            var (res, err) = (~Ꮡproxy.ValueSlot).c.Do(req);
            if (err != default!) {
                return fmt.Errorf("original request: %v"u8, err);
            }
            (~res).Body.Close();
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// Test that a hanging Request.Body.Read from another goroutine can't
// cause the Handler goroutine's Request.Body.Close to block.
// See issue 7121.
public static void TestRequestBodyCloseDoesntBlock(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRequestBodyCloseDoesntBlock(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readWasNilExpectedErrorˢ = (@string)"Read was nil. Expected error."u8;

internal static void testRequestBodyCloseDoesntBlock(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ2);
        }
        var readErrCh = new channel<error>(1);
        var errCh = new channel<error>(2);
        var readErrChʗ1 = readErrCh;
        var server = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
            var readErrChʗ2 = readErrChʗ1;
            goǃ((io.Reader body) => {
                var (_, err) = body.Read(new slice<byte>(100));
                readErrChʗ2.ᐸꟷ(err);
            }, (~req).Body);
            time.Sleep(500 * time.Millisecond);
        }))).Value.ts;
        var closeConn = new channel<bool>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), closeConn, ref ᒐ);
        var closeConnʗ1 = closeConn;
        var errChʗ1 = errCh;
        var serverʗ1 = server;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (conn, err) = net.Dial(tcpˢ, (~serverʗ1).Listener.Addr().String());
                if (err != default!) {
                    errChʗ1.ᐸꟷ(err);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                (_, err) = conn.Write(slice<byte>("POST / HTTP/1.1\r\nConnection: close\r\nHost: foo\r\nContent-Length: 100000\r\n\r\n"u8));
                if (err != default!) {
                    errChʗ1.ᐸꟷ(err);
                    return;
                }
                // And now just block, making the server block on our
                // 100000 bytes of body that will never arrive.
                ᐸꟷ(closeConnʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var selᴛ52 = readErrCh;
        var selᴛ53 = errCh;
        switch (select(ᐸꟷ(selᴛ52, ꓸꓸꓸ), ᐸꟷ(selᴛ53, ꓸꓸꓸ))) {
        case 0 when selᴛ52.ꟷᐳ(out var err): {
            if (err == default!) {
                Ꮡt.Error(readWasNilExpectedErrorˢ);
            }
            break;
        }
        case 1 when selᴛ53.ꟷᐳ(out var err): {
            Ꮡt.Error(err);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10ˢ2 = "GET / HTTP/1.0"u8;
internal static readonly object responseWriterDidNotˢ = (@string)"ResponseWriter did not implement io.StringWriter"u8;
internal static readonly object handlerWasNeverCalledˢ = (@string)"handler was never called"u8;

// test that ResponseWriter implements io.StringWriter.
public static void TestResponseWriterWriteString(ж<testing.T> Ꮡt) {
    var okc = new channel<bool>(1);
    var okcʗ1 = okc;
    ref var ht = ref heap<handlerTest>(out var Ꮡht);
    ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (_, ok) = w._<io.StringWriter>(ᐧ);
        okcʗ1.ᐸꟷ(ok);
    })));
    Ꮡht.rawResponse(getHttp10ˢ2);
    var selᴛ54 = okc;
    switch (trySelect(ᐸꟷ(selᴛ54, ꓸꓸꓸ))) {
    case 0 when selᴛ54.ꟷᐳ(out var ok): {
        if (!ok) {
            Ꮡt.Error(responseWriterDidNotˢ);
        }
        break;
    }
    default: {
        Ꮡt.Error(handlerWasNeverCalledˢ);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cestˢ = "CEST"u8;

public static void TestAppendTime(ж<testing.T> Ꮡt) {
    array<byte> b = new(29); /* len(TimeFormat) */
    var t1 = time.Date(2013, 9, 21, 15, 41, 0, 0, time.FixedZone(cestˢ, 2 * 60 * 60));
    var res = http_internal_test_package.ExportAppendTime(b[..0], t1);
    var (t2, err) = ParseTime(((@string)res));
    if (err != default!) {
        Ꮡt.Fatalf("Error parsing time: %s"u8, err);
    }
    if (!t1.Equal(t2)) {
        Ꮡt.Fatalf("Times differ; expected: %v, got %v (%s)"u8, t1, t2, ((@string)res));
    }
}

public static void TestServerConnState(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerConnState(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bogusRequestˢ = "BOGUS REQUEST\r\n\r\n"u8;

// A stateLog is a log of states over the lifetime of a connection.
[GoType("dyn")] internal partial struct testServerConnState_stateLog {
    internal net.Conn active; // The connection for which the log is recorded; set to the first connection seen in StateNew.
    internal slice<Δhttp.ConnState> got;
    internal slice<Δhttp.ConnState> want;
    internal channel/*<-*/<EmptyStruct> complete = channel/*<-*/<EmptyStruct>.SendOnly; // If non-nil, closed when either 'got' is equal to 'want', or 'got' is no longer a prefix of 'want'.
}

internal static void testServerConnState(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var handler = new map<@string, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>{
            ["/"u8] = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Hello."u8);
            },
            ["/close"u8] = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                w.Header().Set(connectionˢ, closeˢ);
                fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Hello."u8);
            },
            ["/hijack"u8] = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                var (cΔ1, _, _) = w._<Hijacker>().Hijack();
                cΔ1.Write(slice<byte>("HTTP/1.0 200 OK\r\nConnection: close\r\n\r\nHello."u8));
                cΔ1.Close();
            },
            ["/hijack-panic"u8] = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                var (cΔ2, _, _) = w._<Hijacker>().Hijack();
                cΔ2.Write(slice<byte>("HTTP/1.0 200 OK\r\nConnection: close\r\n\r\nHello."u8));
                cΔ2.Close();
                throw panic("intentional panic");
            }
        };
        var activeLog = new channel<ж<testServerConnState_stateLog>>(1);
        // wantLog invokes doRequests, then waits for the resulting connection to
        // either pass through the sequence of states in want or enter a state outside
        // of that sequence.
        var activeLogʗ1 = activeLog;
        void wantLog(Action doRequests, params Span<Δhttp.ConnState> wantʗp) {
            var want = wantʗp.slice();
            Ꮡt.Helper();
            var complete = new channel<EmptyStruct>(0);
            activeLogʗ1.ᐸꟷ(Ꮡ(new testServerConnState_stateLog(want: want, complete: complete)));
            doRequests();
            ᐸꟷ(complete);
            var sl = ᐸꟷ(activeLogʗ1);
            if (!slices.Equal<slice<Δhttp.ConnState>, Δhttp.ConnState>((~sl).got, (~sl).want)) {
                Ꮡt.Errorf("Request(s) produced unexpected state sequence.\nGot:  %v\nWant: %v"u8, (~sl).got, (~sl).want);
            }
        }
        // Don't return sl to activeLog: we don't expect any further states after
        // this point, and want to keep the ConnState callback blocked until the
        // next call to wantLog.
        var handlerʗ1 = handler;

        var activeLogʗ2 = activeLog;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            handlerʗ1[(~(~r).URL).Path](w, r);
        })), (ж<httptest.Server> tsΔ1) => {
            tsΔ1.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
            var activeLogʗ3 = activeLogʗ2;
            tsΔ1.Value.Config.Value.ConnState = (net.Conn cΔ3, Δhttp.ConnState state) => {
                if (cΔ3 == default!) {
                    Ꮡt.Errorf("nil conn seen in state %s"u8, state);
                    return;
                }
                var sl = ᐸꟷ(activeLogʗ3);
                if ((~sl).active == default! && state == StateNew){
                    sl.Value.active = cΔ3;
                } else 
                if (!AreEqual((~sl).active, cΔ3)) {
                    Ꮡt.Errorf("unexpected conn in state %s"u8, state);
                    activeLogʗ3.ᐸꟷ(sl);
                    return;
                }
                sl.Value.got = append((~sl).got, state);
                if ((~sl).complete != default! && (len((~sl).got) >= len((~sl).want) || !slices.Equal<slice<Δhttp.ConnState>, Δhttp.ConnState>((~sl).got, (~sl).want[..(int)(len((~sl).got))]))) {
                    builtin.close((~sl).complete);
                    sl.Value.complete = default!;
                }
                activeLogʗ3.ᐸꟷ(sl);
            };
        }).Value.ts;
        var activeLogʗ4 = activeLog;
        var tsʗ1 = ts;
        defer(() => {
            activeLogʗ4.ᐸꟷ(Ꮡ(new testServerConnState_stateLog(nil))); // If the test failed, allow any remaining ConnState callbacks to complete.
            tsʗ1.Close();
        }, ref ᒐ);
        var c = ts.Client();
        var cʗ1 = c;
        void mustGet(@string url, params ꓸꓸꓸstring headersʗp) {
            GoFrame ᒐ = default;
            try {
                var headers = headersʗp.slice();
                Ꮡt.Helper();
                var (req, err) = NewRequest(getˢ2, url, default!);
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                while (len(headers) > 0) {
                    (~req).Header.Add(headers[0], headers[1]);
                    headers = headers[2..];
                }
                (var res, err) = cʗ1.Do(req);
                if (err != default!) {
                    Ꮡt.Errorf("Error fetching %s: %v"u8, url, err);
                    return;
                }
                (_, err) = io.ReadAll((~res).Body);
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                if (err != default!) {
                    Ꮡt.Errorf("Error reading %s: %v"u8, url, err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        var mustGetʗ1 = mustGet;
        var tsʗ2 = ts;
        wantLog(() => {
            mustGetʗ1((~tsʗ2).URL + "/"u8);
            mustGetʗ1((~tsʗ2).URL + "/close"u8);
        }, StateNew, StateActive, StateIdle, StateActive, StateClosed);
        var mustGetʗ2 = mustGet;
        var tsʗ3 = ts;
        wantLog(() => {
            mustGetʗ2((~tsʗ3).URL + "/"u8);
            mustGetʗ2((~tsʗ3).URL + "/"u8, connectionˢ, closeˢ);
        }, StateNew, StateActive, StateIdle, StateActive, StateClosed);
        var mustGetʗ3 = mustGet;
        var tsʗ4 = ts;
        wantLog(() => {
            mustGetʗ3((~tsʗ4).URL + "/hijack"u8);
        }, StateNew, StateActive, StateHijacked);
        var mustGetʗ4 = mustGet;
        var tsʗ5 = ts;
        wantLog(() => {
            mustGetʗ4((~tsʗ5).URL + "/hijack-panic"u8);
        }, StateNew, StateActive, StateHijacked);
        var tsʗ6 = ts;
        wantLog(() => {
            var (cΔ4, err) = net.Dial(tcpˢ, (~tsʗ6).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            cΔ4.Close();
        }, StateNew, StateClosed);
        var tsʗ7 = ts;
        wantLog(() => {
            var (cΔ5, err) = net.Dial(tcpˢ, (~tsʗ7).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                var (_, errΔ1) = io.WriteString(new http_test_package.net_ConnᴠWriter(cΔ5), bogusRequestˢ); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            cΔ5.Read(new slice<byte>(1)); // block until server hangs up on us
            cΔ5.Close();
        }, StateNew, StateActive, StateClosed);
        var tsʗ8 = ts;
        wantLog(() => {
            var (cΔ6, err) = net.Dial(tcpˢ, (~tsʗ8).Listener.Addr().String());
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                var (_, errΔ1) = io.WriteString(new http_test_package.net_ConnᴠWriter(cΔ6), getHttp11HostFooˢ3); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            (var res, err) = ReadResponse(bufio.NewReader(new http_test_package.net_ConnᴠReader(cΔ6)), nil);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            {
                var (_, errΔ2) = io.Copy(io.Discard, (~res).Body); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            cΔ6.Close();
        }, StateNew, StateActive, StateIdle, StateClosed);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerKeepAlivesEnabledResultClose(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerKeepAlivesEnabledResultClose(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerKeepAlivesEnabledResultClose(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        })), (ж<httptest.Server> tsΔ1) => {
            (~tsΔ1).Config.SetKeepAlivesEnabled(false);
        }).Value.ts;
        var (res, err) = ts.Client().Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if (!(~res).Close) {
            Ꮡt.Errorf("Body.Close == false; want true"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/7856
public static void TestServerEmptyBodyRace(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerEmptyBodyRace(Δp0, Δp1));
}

internal static void testServerEmptyBodyRace(ж<testing.T> Ꮡt, testMode mode) {
    ref var n = ref heap(new int32(), out var Ꮡn);
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        atomic.AddInt32(Ꮡn, 1);
    })), (optQuietLog).OrTypedNilFunc());
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    UntypedInt reqs = 20;
    for (nint i = 0; i < reqs; i++) {
        Ꮡwg.Add(1);
        var cstʗ1 = cst;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (res, err) = (~cstʗ1).c.Get((~(~cstʗ1).ts).URL);
                if (err != default!) {
                    // Try to deflake spurious "connection reset by peer" under load.
                    // See golang.org/issue/22540.
                    time.Sleep(10 * time.Millisecond);
                    (res, err) = (~cstʗ1).c.Get((~(~cstʗ1).ts).URL);
                    if (err != default!) {
                        Ꮡt.Error(err);
                        return;
                    }
                }
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                (_, err) = io.Copy(io.Discard, (~res).Body);
                if (err != default!) {
                    Ꮡt.Error(err);
                    return;
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    {
        var got = atomic.LoadInt32(Ꮡn); if (got != reqs) {
            Ꮡt.Errorf("handler ran %d times; want %d"u8, got, (nint)(reqs));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stateNewNotSeenˢ = (@string)"StateNew not seen"u8;

public static void TestServerConnStateNew(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var sawNew = false; // if the test is buggy, we'll race on this variable.
    var srv = Ꮡ(new Server(
        ConnState: (net.Conn c, Δhttp.ConnState state) => {
            if (state == StateNew) {
                sawNew = true; // testing that this write isn't racy
            }
        },
        Handler: new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        }))
    ));
    // irrelevant
    srv.Serve(new http_test_package.oneConnListenerжListener(Ꮡ(new oneConnListener(
        conn: new http_test_package.rwTestConnжConn(Ꮡ(new rwTestConn(
            Reader: new http_test_package.strings_ReaderжReader(strings.NewReader(getHttp11HostFooˢ3)),
            Writer: io.Discard
        )))
    ))));
    if (!sawNew) {
        // testing that this read isn't racy
        Ꮡt.Error(stateNewNotSeenˢ);
    }
}

[GoType] partial struct closeWriteTestConn {
    internal partial ref rwTestConn rwTestConn { get; }
    internal bool didCloseWrite;
}

[GoRecv] internal static error CloseWrite(this ref closeWriteTestConn c) {
    c.didCloseWrite = true;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didnTSeeCloseWriteCallˢ = (@string)"didn't see CloseWrite call"u8;

public static void TestCloseWrite(ж<testing.T> Ꮡt) {
    http_internal_test_package.SetRSTAvoidanceDelay(Ꮡt, 1 * time.Millisecond);
    ref var srv = ref heap(new Δhttp.Server(), out var Ꮡsrv);
    ref var testConn = ref heap(new closeWriteTestConn(), out var ᏑtestConn);
    var c = http_internal_test_package.ExportServerNewConn(Ꮡsrv, new http_test_package.closeWriteTestConnжConn(ᏑtestConn));
    http_internal_test_package.ExportCloseWriteAndWait(c);
    if (!testConn.didCloseWrite) {
        Ꮡt.Error(didnTSeeCloseWriteCallˢ);
    }
}

// This verifies that a handler can Flush and then Hijack.
//
// A similar test crashed once during development, but it was only
// testing this tangentially and temporarily until another TODO was
// fixed.
//
// So add an explicit test for this.
public static void TestServerFlushAndHijack(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerFlushAndHijack(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ5 = "Hello, "u8;
internal static readonly @string world0ˢ = "6\r\nworld!\r\n0\r\n\r\n"u8;
internal static readonly @string helloWorldˢ3 = "Hello, world!"u8;

internal static void testServerFlushAndHijack(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloˢ5);
            w._<Flusher>().Flush();
            var (conn, buf, _) = w._<Hijacker>().Hijack();
            buf.Value.Writer.Value.WriteString(world0ˢ);
            {
                var errΔ1 = buf.Value.Writer.Value.Flush(); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
            {
                var errΔ2 = conn.Close(); if (errΔ2 != default!) {
                    Ꮡt.Error(errΔ2);
                }
            }
        }))).Value.ts;
        var (res, err) = Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var all, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            @string want = helloWorldˢ3; if (((sstring)all) != want) {
                Ꮡt.Errorf("Got %q; want %q"u8, all, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/8534 -- the Server shouldn't reuse a connection
// for keep-alive after it's seen any Write error (e.g. a timeout) on
// that net.Conn.
//
// To test, verify we don't timeout or see fewer unique client
// addresses (== unique connections) than requests.
public static void TestServerKeepAliveAfterWriteError(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerKeepAliveAfterWriteError(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerKeepAliveAfterWriteError(ж<testing.T> Ꮡt, testMode mode) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ2);
    }
    const nint numReq = 3;
    var addrc = new channel<@string>(numReq);
    var addrcʗ1 = addrc;

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        addrcʗ1.ᐸꟷ((~r).RemoteAddr);
        time.Sleep(500 * time.Millisecond);
        w._<Flusher>().Flush();
    })), (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Config.Value.WriteTimeout = 250 * time.Millisecond;
    }).Value.ts;
    var errc = new channel<error>(numReq);
    var errcʗ1 = errc;
    var tsʗ1 = ts;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), errcʗ1, ref ᒐ);
            for (nint i = 0; i < numReq; i++) {
                var (res, err) = Get((~tsʗ1).URL);
                if (res != nil) {
                    (~res).Body.Close();
                }
                errcʗ1.ᐸꟷ(err);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var addrSeen = new map<@string, bool>{};
    nint numOkay = 0;
    while (ᐧ) {
        var selᴛ55 = addrc;
        var selᴛ56 = errc;
        switch (select(ᐸꟷ(selᴛ55, ꓸꓸꓸ), ᐸꟷ(selᴛ56, ꓸꓸꓸ))) {
        case 0 when selᴛ55.ꟷᐳ(out var v): {
            addrSeen[v] = true;
            break;
        }
        case 1 when selᴛ56.ꟷᐳ(out var err, out var ok): {
            if (!ok) {
                if (len(addrSeen) != numReq) {
                    Ꮡt.Errorf("saw %d unique client addresses; want %d"u8, len(addrSeen), (nint)(numReq));
                }
                if (numOkay != 0) {
                    Ꮡt.Errorf("got %d successful client requests; want 0"u8, numOkay);
                }
                return;
            }
            if (err == default!) {
                numOkay++;
            }
            break;
        }}
    }
}

// Issue 9987: shouldn't add automatic Content-Length (or
// Content-Type) if a Transfer-Encoding was set by the handler.
public static void TestNoContentLengthIfTransferEncoding(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testNoContentLengthIfTransferEncoding(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string htmlˢ = "<html>"u8;

internal static void testNoContentLengthIfTransferEncoding(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(transferEncodingˢ, fooˢ);
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), htmlˢ);
        }))).Value.ts;
        var (c, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("Dial: %v"u8, err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = io.WriteString(new http_test_package.net_ConnᴠWriter(c), getHttp11HostFooˢ3); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var bs = bufio.NewScanner(new http_test_package.net_ConnᴠReader(c));
        ref var got = ref heap(new strings.Builder(), out var Ꮡgot);
        while (bs.Scan()) {
            if (strings.TrimSpace(bs.Text()) == ""u8) {
                break;
            }
            Ꮡgot.WriteString(bs.Text());
            Ꮡgot.WriteByte((rune)'\n');
        }
        {
            var errΔ2 = bs.Err(); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        if (strings.Contains(got.String(), contentLengthˢ)) {
            Ꮡt.Errorf("Unexpected Content-Length in response headers: %s"u8, got.String());
        }
        if (strings.Contains(got.String(), contentTypeˢ)) {
            Ꮡt.Errorf("Unexpected Content-Type in response headers: %s"u8, got.String());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// tolerate extra CRLF(s) before Request-Line on subsequent requests on a conn
// Issue 10876.
public static void TestTolerateCRLFBeforeRequestLine(ж<testing.T> Ꮡt) {
    var req = slice<byte>((@string)("POST / HTTP/1.1\r\nHost: golang.org\r\nContent-Length: 3\r\n\r\nABC"u8 + "\r\n\r\n"u8 + "GET / HTTP/1.1\r\nHost: golang.org\r\n\r\n"u8));
    // <-- this stuff is bogus, but we'll ignore it
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(req)),
        Writer: new http_test_package.bytes_BufferжWriter(Ꮡbuf),
        closec: new channel<bool>(1)
    ));
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    nint numReq = 0;
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        numReq++;
    })));
    ᐸꟷ((~conn).closec);
    if (numReq != 2) {
        Ꮡt.Errorf("num requests = %d; want 2"u8, numReq);
        Ꮡt.Logf("Res: %s"u8, buf.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string putReadbodyHttp11Userˢ = """
PUT /readbody HTTP/1.1
User-Agent: PycURL/7.22.0
Host: 127.0.0.1:9000
Accept: */*
Expect: 100-continue
Content-Length: 10

HelloWorld


"""u8;
internal static readonly object expectHeaderShouldNotBeˢ = (@string)"Expect header should not be filtered out"u8;

public static void TestIssue13893_Expect100(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // test that the Server doesn't filter out Expect headers.
    var req = reqBytes(putReadbodyHttp11Userˢ);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(req)),
        Writer: new http_test_package.bytes_BufferжWriter(Ꮡbuf),
        closec: new channel<bool>(1)
    ));
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        {
            var (_, ok) = (~r).Header[expectˢ, ꟷ]; if (!ok) {
                Ꮡt.Error(expectHeaderShouldNotBeˢ);
            }
        }
    })));
    ᐸꟷ((~conn).closec);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string putReadbodyHttp11Userˢ2 = """
PUT /readbody HTTP/1.1
User-Agent: PycURL/7.22.0
Host: 127.0.0.1:9000
Accept: */*
Expect: 100-continue
Content-Length: 10

HelloWorldPUT /noreadbody HTTP/1.1
User-Agent: PycURL/7.22.0
Host: 127.0.0.1:9000
Accept: */*
Expect: 100-continue
Content-Length: 10

GET /should-be-ignored HTTP/1.1
Host: foo


"""u8;
internal static readonly @string helloWorldˢ4 = "Hello world!"u8;

public static void TestIssue11549_Expect100(ж<testing.T> Ꮡt) {
    var req = reqBytes(putReadbodyHttp11Userˢ2);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.bytes_ReaderжReader(bytes.NewReader(req)),
        Writer: new http_test_package.strings_BuilderжWriter(Ꮡbuf),
        closec: new channel<bool>(1)
    ));
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    nint numReq = 0;
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        numReq++;
        if ((~(~r).URL).Path == "/readbody"u8) {
            io.ReadAll((~r).Body);
        }
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloWorldˢ4);
    })));
    ᐸꟷ((~conn).closec);
    if (numReq != 2) {
        Ꮡt.Errorf("num requests = %d; want 2"u8, numReq);
    }
    if (!strings.Contains(buf.String(), connectionCloseˢ2)) {
        Ꮡt.Errorf("expected 'Connection: close' in response; got: %s"u8, buf.String());
    }
}

// If a Handler finishes and there's an unread request body,
// verify the server implicitly tries to do a read on it before replying.
public static void TestHandlerFinishSkipBigContentLengthRead(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var conn = newTestConn();
    conn.of(testConn.ᏑreadBuf).WriteString(
        "POST / HTTP/1.1\r\n"u8 + "Host: test\r\n"u8 + "Content-Length: 9999999999\r\n"u8 + "\r\n"u8 + strings.Repeat("a"u8, (1 << (int)(20))));
    var ls = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
    nint inHandlerLen = default!;
    var connʗ1 = conn;
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ls), new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        inHandlerLen = connʗ1.of(testConn.ᏑreadBuf).Len();
        rw.WriteHeader(404);
    })));
    ᐸꟷ((~conn).closec);
    nint afterHandlerLen = conn.of(testConn.ᏑreadBuf).Len();
    if (afterHandlerLen != inHandlerLen) {
        Ꮡt.Errorf("unexpected implicit read. Read buffer went from %d -> %d"u8, inHandlerLen, afterHandlerLen);
    }
}

public static void TestHandlerSetsBodyNil(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHandlerSetsBodyNil(Δp0, Δp1));
}

internal static void testHandlerSetsBodyNil(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        r.Value.Body = default!;
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%v"u8, (~r).RemoteAddr);
    })));
    var cstʗ1 = cst;
    @string get() {
        GoFrame ᒐ = default;
        try {
            var (res, err) = (~cstʗ1).c.Get((~(~cstʗ1).ts).URL);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            (var slurp, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            return ((@string)slurp);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    @string a = get();
    @string b = get();
    if (a != b) {
        Ꮡt.Errorf("Failed to reuse connections between requests: %v vs %v"u8, a, b);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ4 = "GET / "u8;
internal static readonly @string httpˢ3 = "HTTP/"u8;

[GoType("dyn")] internal partial struct TestServerValidatesHostHeader_tests {
    internal @string proto;
    internal @string host;
    internal nint want;
}

// Test that we validate the Host header.
// Issue 11206 (invalid bytes in Host) and 13624 (Host present in HTTP/1.1)
public static void TestServerValidatesHostHeader(ж<testing.T> Ꮡt) {
    var tests = new TestServerValidatesHostHeader_tests[]{
        new("HTTP/0.9"u8, ""u8, 505),
        new("HTTP/1.1"u8, ""u8, 400),
        new("HTTP/1.1"u8, "Host: \r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: 1.2.3.4\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: foo.com\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: foo-bar_baz.com\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: foo.com:80\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: ::1\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: [::1]\r\n"u8, 200), // questionable without port, but accept it

        new("HTTP/1.1"u8, "Host: [::1]:80\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: [::1%25en0]:80\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: 1.2.3.4\r\n"u8, 200),
        new("HTTP/1.1"u8, "Host: \x06\r\n"u8, 400),
        new("HTTP/1.1"u8, ((@string)(new byte[]{0x48, 0x6f, 0x73, 0x74, 0x3a, 0x20, 0xff, 0x0d, 0x0a})), 400),
        new("HTTP/1.1"u8, "Host: {\r\n"u8, 400),
        new("HTTP/1.1"u8, "Host: }\r\n"u8, 400),
        new("HTTP/1.1"u8, "Host: first\r\nHost: second\r\n"u8, 400), // HTTP/1.0 can lack a host header, but if present
 // must play by the rules too:

        new("HTTP/1.0"u8, ""u8, 200),
        new("HTTP/1.0"u8, "Host: first\r\nHost: second\r\n"u8, 400),
        new("HTTP/1.0"u8, ((@string)(new byte[]{0x48, 0x6f, 0x73, 0x74, 0x3a, 0x20, 0xff, 0x0d, 0x0a})), 400), // Make an exception for HTTP upgrade requests:

        new("PRI * HTTP/2.0"u8, ""u8, 200), // Also an exception for CONNECT requests: (Issue 18215)

        new("CONNECT golang.org:443 HTTP/1.1"u8, ""u8, 200), // But not other HTTP/2 stuff:

        new("PRI / HTTP/2.0"u8, ""u8, 505),
        new("GET / HTTP/2.0"u8, ""u8, 505),
        new("GET / HTTP/3.0"u8, ""u8, 505)
    }.slice();
    foreach (var (_, tt) in tests) {
        var conn = newTestConn();
        @string methodTarget = getˢ4;
        if (!strings.HasPrefix(tt.proto, httpˢ3)) {
            methodTarget = ""u8;
        }
        io.WriteString(new http_test_package.bytes_BufferжWriter(conn.of(testConn.ᏑreadBuf)), methodTarget + tt.proto + "\r\n"u8 + tt.host + "\r\n"u8);
        var ln = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
        ref var srv = ref heap<Δhttp.Server>(out var Ꮡsrv);
        srv = new Server(
            ErrorLog: quietLog,
            Handler: new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _Δp0, ж<Δhttp.Request> _Δp1) => {
            }))
        );
        goǃ(ᴛ1 => Ꮡsrv.Serve(ᴛ1), new http_test_package.oneConnListenerжListener(ln));
        ᐸꟷ((~conn).closec);
        var (res, err) = ReadResponse(bufio.NewReader(new http_test_package.bytes_BufferжReader(conn.of(testConn.ᏑwriteBuf))), nil);
        if (err != default!) {
            Ꮡt.Errorf("For %s %q, ReadResponse: %v"u8, tt.proto, tt.host, res.OrTypedNil());
            continue;
        }
        if ((~res).StatusCode != tt.want) {
            Ꮡt.Errorf("For %s %q, Status = %d; want %d"u8, tt.proto, tt.host, (~res).StatusCode, tt.want);
        }
    }
}

public static void TestServerHandlersCanHandleH2PRI(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerHandlersCanHandleH2PRI(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string upgradeResponseᶜ = "upgrade here"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ2 = "SM\r\n\r\n"u8;

internal static void testServerHandlersCanHandleH2PRI(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string upgradeResponse = upgradeResponseᶜ;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                var (conn, br, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                if ((~r).Method != "PRI"u8 || (~r).RequestURI != "*"u8) {
                    Ꮡt.Errorf("Got method/target %q %q; want PRI *"u8, (~r).Method, (~r).RequestURI);
                    return;
                }
                if (!(~r).Close) {
                    Ꮡt.Errorf("Request.Close = true; want false"u8);
                }
                @string want = wantᶜ2;
                var buf = new slice<byte>(len(want));
                (var n, errΔ1) = io.ReadFull(new http_test_package.bufio_ReadWriterжReader(br), buf);
                if (errΔ1 != default! || ((sstring)(buf[..(int)(n)])) != want) {
                    Ꮡt.Errorf("Read = %v, %v (%q), want %q"u8, n, errΔ1, buf[..(int)(n)], want);
                    return;
                }
                io.WriteString(new http_test_package.net_ConnᴠWriter(conn), upgradeResponse);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))).Value.ts;
        var (c, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("Dial: %v"u8, err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        io.WriteString(new http_test_package.net_ConnᴠWriter(c), priHttp20Smˢ);
        (var slurp, err) = io.ReadAll(new http_test_package.net_ConnᴠReader(c));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)slurp) != upgradeResponse) {
            Ꮡt.Errorf("Handler response = %q; want %q"u8, slurp, upgradeResponse);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestServerValidatesHeaders_tests {
    internal @string header;
    internal nint want;
}

// Test that we validate the valid bytes in HTTP/1 headers.
// Issue 11207.
public static void TestServerValidatesHeaders(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    setParallel(Ꮡt);
    var tests = new TestServerValidatesHeaders_tests[]{
        new(""u8, 200),
        new("Foo: bar\r\n"u8, 200),
        new("X-Foo: bar\r\n"u8, 200),
        new("Foo: a space\r\n"u8, 200),
        new("A space: foo\r\n"u8, 400), // space in header

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xff, 0x62, 0x61, 0x72, 0x3a, 0x20, 0x66, 0x6f, 0x6f, 0x0d, 0x0a})), 400), // binary in header

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72, 0x3a, 0x20, 0x66, 0x6f, 0x6f, 0x0d, 0x0a})), 400), // binary in header

        new("Foo: "u8 + strings.Repeat("x"u8, (1 << (int)(21))) + "\r\n"u8, 431), // header too large
 // Spaces between the header key and colon are not allowed.
 // See RFC 7230, Section 3.2.4.

        new("Foo : bar\r\n"u8, 400),
        new("Foo\t: bar\r\n"u8, 400), // Empty header keys are invalid.
 // See RFC 7230, Section 3.2.

        new(": empty key\r\n"u8, 400), // Requests with invalid Content-Length headers should be rejected
 // regardless of the presence of a Transfer-Encoding header.
 // Check out RFC 9110, Section 8.6 and RFC 9112, Section 6.3.3.

        new("Content-Length: notdigits\r\n"u8, 400),
        new("Content-Length: notdigits\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n\r\n"u8, 400),
        new("foo: foo foo\r\n"u8, 200), // LWS space is okay

        new("foo: foo\tfoo\r\n"u8, 200), // LWS tab is okay

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x3a, 0x20, 0x66, 0x6f, 0x6f, 0x00, 0x66, 0x6f, 0x6f, 0x0d, 0x0a})), 400), // CTL 0x00 in value is bad

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x3a, 0x20, 0x66, 0x6f, 0x6f, 0x7f, 0x66, 0x6f, 0x6f, 0x0d, 0x0a})), 400), // CTL 0x7f in value is bad

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x3a, 0x20, 0x66, 0x6f, 0x6f, 0xff, 0x66, 0x6f, 0x6f, 0x0d, 0x0a})), 200)
    }.slice();
    // non-ASCII high octets in value are fine
    foreach (var (_, tt) in tests) {
        var conn = newTestConn();
        io.WriteString(new http_test_package.bytes_BufferжWriter(conn.of(testConn.ᏑreadBuf)), "GET / HTTP/1.1\r\nHost: foo\r\n"u8 + tt.header + "\r\n"u8);
        var ln = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
        ref var srv = ref heap<Δhttp.Server>(out var Ꮡsrv);
        srv = new Server(
            ErrorLog: quietLog,
            Handler: new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter _Δp0, ж<Δhttp.Request> _Δp1) => {
            }))
        );
        goǃ(ᴛ1 => Ꮡsrv.Serve(ᴛ1), new http_test_package.oneConnListenerжListener(ln));
        ᐸꟷ((~conn).closec);
        var (res, err) = ReadResponse(bufio.NewReader(new http_test_package.bytes_BufferжReader(conn.of(testConn.ᏑwriteBuf))), nil);
        if (err != default!) {
            Ꮡt.Errorf("For %q, ReadResponse: %v"u8, tt.header, res.OrTypedNil());
            continue;
        }
        if ((~res).StatusCode != tt.want) {
            Ꮡt.Errorf("For %q, Status = %d; want %d"u8, tt.header, (~res).StatusCode, tt.want);
        }
    }
}

public static void TestServerRequestContextCancel_ServeHTTPDone(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerRequestContextCancel_ServeHTTPDone(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldNotBeDoneInˢ = (@string)"should not be Done in ServeHTTP"u8;
internal static readonly object contextShouldBeDoneAfterˢ = (@string)"context should be done after ServeHTTP completes"u8;

internal static void testServerRequestContextCancel_ServeHTTPDone(ж<testing.T> Ꮡt, testMode mode) {
    var ctxc = new channel<context.Context>(1);
    var ctxcʗ1 = ctxc;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var ctxΔ1 = r.Context();
        var selᴛ57 = ctxΔ1.Done();
        switch (trySelect(ᐸꟷ(selᴛ57, ꓸꓸꓸ))) {
        case 0 when selᴛ57.ꟷᐳ(out _): {
            Ꮡt.Error(shouldNotBeDoneInˢ);
            break;
        }
        default: {
            break;
        }}
        ctxcʗ1.ᐸꟷ(ctxΔ1);
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    var ctx = ᐸꟷ(ctxc);
    var selᴛ58 = ctx.Done();
    switch (trySelect(ᐸꟷ(selᴛ58, ꓸꓸꓸ))) {
    case 0 when selᴛ58.ꟷᐳ(out _): {
        break;
    }
    default: {
        Ꮡt.Error(contextShouldBeDoneAfterˢ);
        break;
    }}
}

// Tests that the Request.Context available to the Handler is canceled
// if the peer closes their TCP connection. This requires that the server
// is always blocked in a Read call so it notices the EOF from the client.
// See issues 15927 and 15224.
public static void TestServerRequestContextCancel_ConnClose(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerRequestContextCancel_ConnClose(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerRequestContextCancel_ConnClose(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var inHandler = new channel<EmptyStruct>(0);
        var handlerDone = new channel<EmptyStruct>(0);
        var handlerDoneʗ1 = handlerDone;
        var inHandlerʗ1 = inHandler;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            builtin.close(inHandlerʗ1);
            ᐸꟷ(r.Context().Done());
            builtin.close(handlerDoneʗ1);
        }))).Value.ts;
        var (c, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        io.WriteString(new http_test_package.net_ConnᴠWriter(c), getHttp11HostFooˢ3);
        ᐸꟷ(inHandler);
        c.Close(); // this should trigger the context being done
        ᐸꟷ(handlerDone);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerContext_ServerContextKey(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerContext_ServerContextKey(Δp0, Δp1));
}

internal static void testServerContext_ServerContextKey(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var ctx = r.Context();
        var got = ctx.Value(ServerContextKey.OrTypedNil());
        {
            var (_, ok) = got._<ж<Δhttp.Server>>(ᐧ); if (!ok) {
                Ꮡt.Errorf("context value = %T; want *http.Server"u8, got);
            }
        }
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
}

public static void TestServerContext_LocalAddrContextKey(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerContext_LocalAddrContextKey(Δp0, Δp1));
}

internal static void testServerContext_LocalAddrContextKey(ж<testing.T> Ꮡt, testMode mode) {
    var ch = new channel<any>(1);
    var chʗ1 = ch;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        chʗ1.ᐸꟷ(r.Context().Value(LocalAddrContextKey.OrTypedNil()));
    })));
    {
        var (_, err) = (~cst).c.Head((~(~cst).ts).URL); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string host = (~(~cst).ts).Listener.Addr().String();
    var got = ᐸꟷ(ch);
    {
        var (addr, ok) = got._<netꓸAddr>(ᐧ); if (!ok){
            Ꮡt.Errorf("local addr value = %T; want net.Addr"u8, got);
        } else 
        if (fmt.Sprint(addr) != host) {
            Ꮡt.Errorf("local addr = %v; want %v"u8, addr, host);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string hdrᶜ = "Transfer-Encoding: chunked"u8;

// https://golang.org/issue/15960
public static void TestHandlerSetTransferEncodingChunked(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        ref var ht = ref heap<handlerTest>(out var Ꮡht);
        ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(transferEncodingˢ, chunkedˢ);
            w.Write(slice<byte>("hello"u8));
        })));
        @string resp = Ꮡht.rawResponse(getHttp11HostFooˢ4);
        @string hdr = hdrᶜ;
        {
            nint n = strings.Count(resp, hdr); if (n != 1) {
                Ꮡt.Errorf("want 1 occurrence of %q in response, got %v\nresponse: %v"u8, hdr, n, resp);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// https://golang.org/issue/16063
public static void TestHandlerSetTransferEncodingGzip(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        ref var ht = ref heap<handlerTest>(out var Ꮡht);
        ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(transferEncodingˢ, gzipˢ);
            var gz = gzip.NewWriter(new http_test_package.http_ResponseWriterᴠWriter(w));
            gz.Write(slice<byte>("hello"u8));
            gz.Close();
        })));
        @string resp = Ꮡht.rawResponse(getHttp11HostFooˢ4);
        foreach (var (_, v) in new @string[]{"gzip"u8, "chunked"u8}.slice()) {
            @string hdr = "Transfer-Encoding: "u8 + v;
            {
                nint n = strings.Count(resp, hdr); if (n != 1) {
                    Ꮡt.Errorf("want 1 occurrence of %q in response, got %v\nresponse: %v"u8, hdr, n, resp);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkClientServer(ж<testing.B> Ꮡb) {
    run<BжTBRun>(Ꮡb, (Δp0, Δp1) => benchmarkClientServer(Δp0, Δp1), new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object getˢ5 = (@string)"Get:"u8;
internal static readonly object readAllˢ = (@string)"ReadAll:"u8;
internal static readonly object gotBodyˢ = (@string)"Got body:"u8;

internal static void benchmarkClientServer(ж<testing.B> Ꮡb, testMode mode) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.StopTimer();
    var ts = newClientServerTest(new http_test_package.testing_BжTB(Ꮡb), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(rw), "Hello world.\n"u8);
    }))).Value.ts;
    b.StartTimer();
    var c = ts.Client();
    for (nint i = 0; i < b.N; i++) {
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡb.Fatal(getˢ5, err);
        }
        (var all, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (err != default!) {
            Ꮡb.Fatal(readAllˢ, err);
        }
        @string body = ((@string)all);
        if (body != "Hello world.\n"u8) {
            Ꮡb.Fatal(gotBodyˢ, body);
        }
    }
    b.StopTimer();
}

public static void BenchmarkClientServerParallel(ж<testing.B> Ꮡb) {
    foreach (var (_, parallelism) in new nint[]{4, 64}.slice()) {
        Ꮡb.Run(fmt.Sprint(parallelism), (ж<testing.B> bΔ1) => {
            run<BжTBRun>(bΔ1, (BжTBRun bΔ2Δp, testMode mode) => {
                var bΔ2 = (ж<testing.B>)bΔ2Δp;
                benchmarkClientServerParallel(bΔ2, parallelism, mode);
            }, new testMode[]{http1Mode, https1Mode, http2Mode}.slice());
        });
    }
}

internal static void benchmarkClientServerParallel(ж<testing.B> Ꮡb, nint parallelism, testMode mode) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var ts = newClientServerTest(new http_test_package.testing_BжTB(Ꮡb), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(rw), "Hello world.\n"u8);
    }))).Value.ts;
    b.ResetTimer();
    b.SetParallelism(parallelism);
    var tsʗ1 = ts;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var c = tsʗ1.Client();
        while (pb.Next()) {
            var (res, err) = c.Get((~tsʗ1).URL);
            if (err != default!) {
                Ꮡb.Logf("Get: %v"u8, err);
                continue;
            }
            (var all, err) = io.ReadAll((~res).Body);
            (~res).Body.Close();
            if (err != default!) {
                Ꮡb.Logf("ReadAll: %v"u8, err);
                continue;
            }
            sstring body = ((sstring)all);
            if (body != "Hello world.\n"u8) {
                throw panic("Got body: " + body);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testBenchServerUrlˢ = "TEST_BENCH_SERVER_URL"u8;
internal static readonly @string testBenchClientNˢ = "TEST_BENCH_CLIENT_N"u8;
internal static readonly @string testBenchBenchmarkServerˢ = "-test.bench=^BenchmarkServer$"u8;

// A benchmark for profiling the server without the HTTP client code.
// The client code runs in a subprocess.
//
// For use like:
//
//	$ go test -c
//	$ ./http.test -test.run='^$' -test.bench='^BenchmarkServer$' -test.benchtime=15s -test.cpuprofile=http.prof
//	$ go tool pprof http.test http.prof
//	(pprof) web
public static void BenchmarkServer(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.ReportAllocs();
        // Child process mode;
        {
            @string url = os.Getenv(testBenchServerUrlˢ); if (url != ""u8) {
                var (n, errΔ1) = strconv.Atoi(os.Getenv(testBenchClientNˢ));
                if (errΔ1 != default!) {
                    throw panic(errΔ1);
                }
                for (nint i = 0; i < n; i++) {
                    var (resΔ1, errΔ2) = Get(url);
                    if (errΔ2 != default!) {
                        log.Panicf("Get: %v"u8, errΔ2);
                    }
                    (var all, errΔ2) = io.ReadAll((~resΔ1).Body);
                    (~resΔ1).Body.Close();
                    if (errΔ2 != default!) {
                        log.Panicf("ReadAll: %v"u8, errΔ2);
                    }
                    @string body = ((@string)all);
                    if (body != "Hello world.\n"u8) {
                        log.Panicf("Got body: %q"u8, body);
                    }
                }
                os.Exit(0);
                return;
            }
        }
        slice<byte> res = slice<byte>("Hello world.\n"u8);
        b.StopTimer();
        var resʗ1 = res;
        var ts = httptest.NewServer(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
            rw.Header().Set(contentTypeˢ, textHtmlCharsetUtf8ˢ);
            rw.Write(resʗ1);
        })));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        b.StartTimer();
        var cmd = testenv.Command(new http_test_package.testing_BжTB(Ꮡb), os.Args[0], testRunˢ, testBenchBenchmarkServerˢ);
        cmd.Value.Env = appendꓸꓸꓸ(new @string[]{
            fmt.Sprintf("TEST_BENCH_CLIENT_N=%d"u8, b.N),
            fmt.Sprintf("TEST_BENCH_SERVER_URL=%s"u8, (~ts).URL)
        }.slice(), os.Environ());
        var (@out, err) = cmd.CombinedOutput();
        if (err != default!) {
            Ꮡb.Errorf("Test failure: %v, with output: %s"u8, err, @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// getNoBody wraps Get but closes any Response.Body before returning the response.
internal static (ж<Δhttp.Response>, error) getNoBody(@string urlStr) {
    var (res, err) = Get(urlStr);
    if (err != default!) {
        return (default!, err);
    }
    (~res).Body.Close();
    return (res, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testBenchServerˢ = "TEST_BENCH_SERVER"u8;
internal static readonly @string testBenchServerPortˢ = "TEST_BENCH_SERVER_PORT"u8;
internal static readonly @string stopˢ = "stop"u8;
internal static readonly @string testBenchBenchmarkClientˢ = "-test.bench=^BenchmarkClient$"u8;

// A benchmark for profiling the client without the HTTP server code.
// The server code runs in a subprocess.
public static void BenchmarkClient(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        b.ReportAllocs();
        b.StopTimer();
        defer(afterTest, new http_test_package.testing_BжTB(Ꮡb), ref ᒐ);
        slice<byte> data = slice<byte>("Hello world.\n"u8);
        {
            @string server = os.Getenv(testBenchServerˢ); if (server != ""u8) {
                // Server process mode.
                @string port = os.Getenv(testBenchServerPortˢ); // can be set by user
                if (port == ""u8) {
                    port = "0"u8;
                }
                var (ln, errΔ1) = net.Listen(tcpˢ, "localhost:"u8 + port);
                if (errΔ1 != default!) {
                    fmt.Fprintln(new os.FileжWriter(os.Stderr), errΔ1.Error());
                    os.Exit(1);
                }
                fmt.Println(ln.Addr().String());
                var dataʗ1 = data;
                HandleFunc("/"u8, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                    r.ParseForm();
                    if ((~r).Form.Get(stopˢ) != ""u8) {
                        os.Exit(0);
                    }
                    w.Header().Set(contentTypeˢ, textHtmlCharsetUtf8ˢ);
                    w.Write(dataʗ1);
                });
                ref var srv = ref heap(new Δhttp.Server(), out var Ꮡsrv);
                log.Fatal(Ꮡsrv.Serve(ln));
            }
        }
        // Start server process.
        var (ctx, cancel) = context_package.WithCancel(context_package.Background());
        var cmd = testenv.CommandContext(new http_test_package.testing_BжTB(Ꮡb), ctx, os.Args[0], testRunˢ, testBenchBenchmarkClientˢ);
        cmd.Value.Env = append(cmd.Environ(), "TEST_BENCH_SERVER=yes"u8);
        cmd.Value.Stderr = new os.FileжWriter(os.Stderr);
        var (stdout, err) = cmd.StdoutPipe();
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        {
            var errΔ2 = cmd.Start(); if (errΔ2 != default!) {
                Ꮡb.Fatalf("subprocess failed to start: %v"u8, errΔ2);
            }
        }
        var done = new channel<error>(1);
        var cmdʗ1 = cmd;
        var doneʗ1 = done;
        goǃ(() => {
            doneʗ1.ᐸꟷ(cmdʗ1.Wait());
            builtin.close(doneʗ1);
        });
        var cancelʗ1 = cancel;
        var doneʗ2 = done;
        defer(() => {
            cancelʗ1();
            ᐸꟷ(doneʗ2);
        }, ref ᒐ);
        // Wait for the server in the child process to respond and tell us
        // its listening address, once it's started listening:
        var bs = bufio.NewScanner(stdout);
        if (!bs.Scan()) {
            Ꮡb.Fatalf("failed to read listening URL from child: %v"u8, bs.Err());
        }
        @string url = "http://"u8 + strings.TrimSpace(bs.Text()) + "/"u8;
        {
            var (_, errΔ3) = getNoBody(url); if (errΔ3 != default!) {
                Ꮡb.Fatalf("initial probe of child process failed: %v"u8, errΔ3);
            }
        }
        // Do b.N requests to the server.
        b.StartTimer();
        for (nint i = 0; i < b.N; i++) {
            var (res, errΔ4) = Get(url);
            if (errΔ4 != default!) {
                Ꮡb.Fatalf("Get: %v"u8, errΔ4);
            }
            (var body, errΔ4) = io.ReadAll((~res).Body);
            (~res).Body.Close();
            if (errΔ4 != default!) {
                Ꮡb.Fatalf("ReadAll: %v"u8, errΔ4);
            }
            if (!bytes.Equal(body, data)) {
                Ꮡb.Fatalf("Got body: %q"u8, body);
            }
        }
        b.StopTimer();
        // Instruct server process to stop.
        getNoBody(url + "?stop=yes"u8);
        {
            var errΔ5 = ᐸꟷ(done); if (errΔ5 != default!) {
                Ꮡb.Fatalf("subprocess failed: %v"u8, errΔ5);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10HostGolangOrgˢ2 = """
GET / HTTP/1.0
Host: golang.org
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-US,en;q=0.8
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3

"""u8;

public static void BenchmarkServerFakeConnNoKeepAlive(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var req = reqBytes(getHttp10HostGolangOrgˢ2);
    var res = slice<byte>("Hello world!\n"u8);
    var conn = newTestConn();
    var resʗ1 = res;
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        rw.Header().Set(contentTypeˢ, textHtmlCharsetUtf8ˢ);
        rw.Write(resʗ1);
    });
    var ln = @new<oneConnListener>();
    for (nint i = 0; i < b.N; i++) {
        conn.of(testConn.ᏑreadBuf).Reset();
        conn.of(testConn.ᏑwriteBuf).Reset();
        conn.of(testConn.ᏑreadBuf).Write(req);
        ln.Value.conn = new http_test_package.testConnжConn(conn);
        Serve(new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
        ᐸꟷ((~conn).closec);
    }
}

// repeatReader reads content count times, then EOFs.
[GoType] partial struct repeatReader {
    internal slice<byte> content;
    internal nint count;
    internal nint off;
}

[GoRecv] internal static (nint n, error err) Read(this ref repeatReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (r.count <= 0) {
        return (0, io.EOF);
    }
    n = copy(p, r.content[(int)(r.off)..]);
    r.off += n;
    if (r.off == len(r.content)) {
        r.count--;
        r.off = 0;
    }
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostGolangOrgˢ2 = """
GET / HTTP/1.1
Host: golang.org
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-US,en;q=0.8
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3

"""u8;

public static void BenchmarkServerFakeConnWithKeepAlive(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var req = reqBytes(getHttp11HostGolangOrgˢ2);
    var res = slice<byte>("Hello world!\n"u8);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.repeatReaderжReader(Ꮡ(new repeatReader(content: req, count: b.N))),
        Writer: io.Discard,
        closec: new channel<bool>(1)
    ));
    nint handled = 0;
    var resʗ1 = res;
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        handled++;
        rw.Header().Set(contentTypeˢ, textHtmlCharsetUtf8ˢ);
        rw.Write(resʗ1);
    });
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
    ᐸꟷ((~conn).closec);
    if (b.N != handled) {
        Ꮡb.Errorf("b.N=%d but handled %d"u8, b.N, handled);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp11HostGolangOrgˢ3 = """
GET / HTTP/1.1
Host: golang.org

"""u8;

// same as above, but representing the most simple possible request
// and handler. Notably: the handler does not call rw.Header().
public static void BenchmarkServerFakeConnWithKeepAliveLite(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var req = reqBytes(getHttp11HostGolangOrgˢ3);
    var res = slice<byte>("Hello world!\n"u8);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.repeatReaderжReader(Ꮡ(new repeatReader(content: req, count: b.N))),
        Writer: io.Discard,
        closec: new channel<bool>(1)
    ));
    nint handled = 0;
    var resʗ1 = res;
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        handled++;
        rw.Write(resʗ1);
    });
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
    ᐸꟷ((~conn).closec);
    if (b.N != handled) {
        Ꮡb.Errorf("b.N=%d but handled %d"u8, b.N, handled);
    }
}

internal static readonly @string someResponse = "<html>some response</html>"u8;

// A Response that's just no bigger than 2KB, the buffer-before-chunking threshold.
internal static slice<byte> response = bytes.Repeat(slice<byte>(someResponse), (2 << (int)(10)) / len(someResponse));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textHtmlˢ = "text/html"u8;

// Both Content-Type and Content-Length set. Should be no buffering.
public static void BenchmarkServerHandlerTypeLen(ж<testing.B> Ꮡb) {
    benchmarkHandler(Ꮡb, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentTypeˢ, textHtmlˢ);
        w.Header().Set(contentLengthˢ, strconv.Itoa(len(response)));
        w.Write(response);
    })));
}

// A Content-Type is set, but no length. No sniffing, but will count the Content-Length.
public static void BenchmarkServerHandlerNoLen(ж<testing.B> Ꮡb) {
    benchmarkHandler(Ꮡb, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentTypeˢ, textHtmlˢ);
        w.Write(response);
    })));
}

// A Content-Length is set, but the Content-Type will be sniffed.
public static void BenchmarkServerHandlerNoType(ж<testing.B> Ꮡb) {
    benchmarkHandler(Ꮡb, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentLengthˢ, strconv.Itoa(len(response)));
        w.Write(response);
    })));
}

// Neither a Content-Type or Content-Length, so sniffed and counted.
public static void BenchmarkServerHandlerNoHeader(ж<testing.B> Ꮡb) {
    benchmarkHandler(Ꮡb, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(response);
    })));
}

internal static void benchmarkHandler(ж<testing.B> Ꮡb, httpꓸHandler h) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var req = reqBytes(getHttp11HostGolangOrgˢ3);
    var conn = Ꮡ(new rwTestConn(
        Reader: new http_test_package.repeatReaderжReader(Ꮡ(new repeatReader(content: req, count: b.N))),
        Writer: io.Discard,
        closec: new channel<bool>(1)
    ));
    nint handled = 0;
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        handled++;
        h.ServeHTTP(rw, r);
    });
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(handler));
    ᐸꟷ((~conn).closec);
    if (b.N != handled) {
        Ꮡb.Errorf("b.N=%d but handled %d"u8, b.N, handled);
    }
}

public static void BenchmarkServerHijack(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var req = reqBytes(getHttp11HostGolangOrgˢ3);
    var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (connΔ1, _, err) = w._<Hijacker>().Hijack();
        if (err != default!) {
            throw panic(err);
        }
        connΔ1.Close();
    });
    var conn = Ꮡ(new rwTestConn(
        Writer: io.Discard,
        closec: new channel<bool>(1)
    ));
    var ln = Ꮡ(new oneConnListener(conn: new http_test_package.rwTestConnжConn(conn)));
    for (nint i = 0; i < b.N; i++) {
        conn.Value.Reader = new http_test_package.bytes_ReaderжReader(bytes.NewReader(req));
        ln.Value.conn = new http_test_package.rwTestConnжConn(conn);
        Serve(new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(h));
        ᐸꟷ((~conn).closec);
    }
}

public static void BenchmarkCloseNotifier(ж<testing.B> Ꮡb) {
    run<BжTBRun>(Ꮡb, (Δp0, Δp1) => benchmarkCloseNotifier(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void benchmarkCloseNotifier(ж<testing.B> Ꮡb, testMode mode) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.StopTimer();
    var sawClose = new channel<bool>(0);
    var sawCloseʗ1 = sawClose;
    var ts = newClientServerTest(new http_test_package.testing_BжTB(Ꮡb), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        ᐸꟷ(rw._<CloseNotifier>().CloseNotify());
        sawCloseʗ1.ᐸꟷ(true);
    }))).Value.ts;
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡb.Fatalf("error dialing: %v"u8, err);
        }
        (_, err) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(conn), "GET / HTTP/1.1\r\nConnection: keep-alive\r\nHost: foo\r\n\r\n"u8);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        conn.Close();
        ᐸꟷ(sawClose);
    }
    b.StopTimer();
}

// Verify this doesn't race (Issue 16505)
public static void TestConcurrentServerServe(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    for (nint i = 0; i < 100; i++) {
        var ln1 = Ꮡ(new oneConnListener(conn: default!));
        var ln2 = Ꮡ(new oneConnListener(conn: default!));
        ref var srv = ref heap<Δhttp.Server>(out var Ꮡsrv);
        srv = new Server(nil);
        var ln1ʗ1 = ln1;
        goǃ(() => {
            Ꮡsrv.Serve(new http_test_package.oneConnListenerжListener(ln1ʗ1));
        });
        var ln2ʗ1 = ln2;
        goǃ(() => {
            Ꮡsrv.Serve(new http_test_package.oneConnListenerжListener(ln2ʗ1));
        });
    }
}

public static void TestServerIdleTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerIdleTimeout(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerIdleTimeout(ж<testing.T> Ꮡt, testMode mode) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    runTimeSensitiveTest(Ꮡt, new time.Duration[]{
        10 * time.Millisecond,
        100 * time.Millisecond,
        1 * time.ΔSecond,
        (time.Duration)(10000000000L)
    }.slice(), error (ж<testing.T> tΔ1, time.Duration readHeaderTimeout) => {
        GoFrame ᒐ = default;
        try {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                io.Copy(io.Discard, (~r).Body);
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), (~r).RemoteAddr);
            })), (ж<httptest.Server> tsΔ1) => {
                tsΔ1.Value.Config.Value.ReadHeaderTimeout = readHeaderTimeout;
                tsΔ1.Value.Config.Value.IdleTimeout = 2 * readHeaderTimeout;
            });
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            tΔ1.Logf("ReadHeaderTimeout = %v"u8, (~(~ts).Config).ReadHeaderTimeout);
            tΔ1.Logf("IdleTimeout = %v"u8, (~(~ts).Config).IdleTimeout);
            var c = ts.Client();
            var cʗ1 = c;
            var tsʗ1 = ts;
            (@string, error) get() {
                GoFrame ᒐ = default;
                try {
                    var (res, errΔ1) = cʗ1.Get((~tsʗ1).URL);
                    if (errΔ1 != default!) {
                        return ("", errΔ1);
                    }
                    var resʗ1 = res;
                    defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                    (var slurp, errΔ1) = io.ReadAll((~res).Body);
                    if (errΔ1 != default!) {
                        // If we're at this point the headers have definitely already been
                        // read and the server is not idle, so neither timeout applies:
                        // this should never fail.
                        tΔ1.Fatal(errΔ1);
                    }
                    return (((@string)slurp), default!);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
                finally { ᒐ.Run(); }
            }
            var (a1, err) = get();
            if (err != default!) {
                return err;
            }
            (var a2, err) = get();
            if (err != default!) {
                return err;
            }
            if (a1 != a2) {
                return fmt.Errorf("did requests on different connections"u8);
            }
            time.Sleep((~(~ts).Config).IdleTimeout * 3 / 2);
            (var a3, err) = get();
            if (err != default!) {
                return err;
            }
            if (a2 == a3) {
                return fmt.Errorf("request three unexpectedly on same connection"u8);
            }
            // And test that ReadHeaderTimeout still works:
            (var conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
            if (err != default!) {
                return err;
            }
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            conn.Write(slice<byte>("GET / HTTP/1.1\r\nHost: foo.com\r\n"u8));
            time.Sleep((~(~ts).Config).ReadHeaderTimeout * 2);
            {
                var (_, errΔ1) = io.CopyN(io.Discard, new http_test_package.net_ConnᴠReader(conn), 1); if (errΔ1 == default!) {
                    return fmt.Errorf("copy byte succeeded; want err"u8);
                }
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

internal static @string get(ж<testing.T> Ꮡt, ж<Δhttp.Client> Ꮡc, @string url) {
    GoFrame ᒐ = default;
    try {
        var (res, err) = Ꮡc.Get(url);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        return ((@string)slurp);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Tests that calls to Server.SetKeepAlivesEnabled(false) closes any
// currently-open connections.
public static void TestServerSetKeepAlivesEnabledClosesConns(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerSetKeepAlivesEnabledClosesConns(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerSetKeepAlivesEnabledClosesConns(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), (~r).RemoteAddr);
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    var cʗ1 = c;
    var tsʗ1 = ts;
    @string getΔ1() => get(Ꮡt, cʗ1, (~tsʗ1).URL);
    @string a1 = getΔ1();
    @string a2 = getΔ1();
    if (a1 == a2){
        Ꮡt.Logf("made two requests from a single conn %q (as expected)"u8, a1);
    } else {
        Ꮡt.Errorf("server reported requests from %q and %q; expected same connection"u8, a1, a2);
    }
    // The two requests should have used the same connection,
    // and there should not have been a second connection that
    // was created by racing dial against reuse.
    // (The first get was completed when the second get started.)
    {
        var conns = tr.IdleConnStrsForTesting(); if (len(conns) != 1) {
            Ꮡt.Errorf("found %d idle conns (%q); want 1"u8, len(conns), conns);
        }
    }
    // SetKeepAlivesEnabled should discard idle conns.
    (~ts).Config.SetKeepAlivesEnabled(false);
    var trʗ1 = tr;
    waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
        {
            var conns = trʗ1.IdleConnStrsForTesting(); if (len(conns) > 0) {
                if (d > 0) {
                    Ꮡt.Logf("idle conns %v after SetKeepAlivesEnabled called = %q; waiting for empty"u8, d, conns);
                }
                return false;
            }
        }
        return true;
    });
}

// If we make a third request it should use a new connection, but in general
// we have no way to verify that: the new connection could happen to reuse the
// exact same ports from the previous connection.
public static void TestServerShutdown(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerShutdown(Δp0, Δp1));
}

internal static void testServerShutdown(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var cst = ref heap<ж<clientServerTest>>(out var Ꮡcst);
    ref var once = ref heap(new sync.Once(), out var Ꮡonce);
    var statesRes = new channel<map<Δhttp.ConnState, nint>>(1);
    var shutdownRes = new channel<error>(1);
    var gotOnShutdown = new channel<EmptyStruct>(0);
    var gotOnShutdownʗ1 = gotOnShutdown;
    var shutdownResʗ1 = shutdownRes;
    var statesResʗ1 = statesRes;
    var handler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var first = false;
        var shutdownResʗ2 = shutdownResʗ1;
        var statesResʗ2 = statesResʗ1;
        Ꮡonce.Do(() => {
            statesResʗ2.ᐸꟷ((~(~Ꮡcst.ValueSlot).ts).Config.ExportAllConnsByState());
            var shutdownResʗ3 = shutdownResʗ2;
            goǃ(() => {
                shutdownResʗ3.ᐸꟷ((~(~Ꮡcst.ValueSlot).ts).Config.Shutdown(context_package.Background()));
            });
            first = true;
        });
        if (first) {
            // Shutdown is graceful, so it should not interrupt this in-flight response
            // but should reject new requests. (Since this request is still in flight,
            // the server's port should not be reused for another server yet.)
            ᐸꟷ(gotOnShutdownʗ1);
            // TODO(#59038): The HTTP/2 server empirically does not always reject new
            // requests. As a workaround, loop until we see a failure.
            while (!Ꮡt.Failed()) {
                var (res, err) = (~Ꮡcst.ValueSlot).c.Get((~(~Ꮡcst.ValueSlot).ts).URL);
                if (err != default!) {
                    break;
                }
                var (outΔ1, _) = io.ReadAll((~res).Body);
                (~res).Body.Close();
                if (mode == http2Mode) {
                    Ꮡt.Logf("%v: unexpected success (%q). Listener should be closed before OnShutdown is called."u8, (~(~Ꮡcst.ValueSlot).ts).URL, outΔ1);
                    Ꮡt.Logf("Retrying to work around https://go.dev/issue/59038."u8);
                    continue;
                }
                Ꮡt.Errorf("%v: unexpected success (%q). Listener should be closed before OnShutdown is called."u8, (~(~Ꮡcst.ValueSlot).ts).URL, outΔ1);
            }
        }
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), (~r).RemoteAddr);
    });
    var gotOnShutdownʗ2 = gotOnShutdown;
    cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(handler), (ж<httptest.Server> srv) => {
        var gotOnShutdownʗ3 = gotOnShutdownʗ2;
        (~srv).Config.RegisterOnShutdown(() => {
            builtin.close(gotOnShutdownʗ3);
        });
    });
    @string @out = get(Ꮡt, (~cst).c, (~(~cst).ts).URL); // calls t.Fail on failure
    Ꮡt.Logf("%v: %q"u8, (~(~cst).ts).URL, @out);
    {
        var err = ᐸꟷ(shutdownRes); if (err != default!) {
            Ꮡt.Fatalf("Shutdown: %v"u8, err);
        }
    }
    ᐸꟷ(gotOnShutdown); // Will hang if RegisterOnShutdown is broken.
    {
        var states = ᐸꟷ(statesRes); if (states[StateActive] != 1) {
            Ꮡt.Errorf("connection in wrong state, %v"u8, states);
        }
    }
}

public static void TestServerShutdownStateNew(ж<testing.T> Ꮡt) {
    runSynctest(Ꮡt, testServerShutdownStateNew);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testTakes56Secondsˢ = (@string)"test takes 5-6 seconds; skipping in short mode"u8;
internal static readonly object shutdownTooSoonˢ = (@string)"shutdown too soon"u8;
internal static readonly object connectionWasClosedByˢ = (@string)"connection was closed by server too soon"u8;

internal static void testServerShutdownStateNew(testing.TB t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(testTakes56Secondsˢ);
        }
        var listener = fakeNetListen();
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);

        var listenerʗ2 = listener;
        var ts = newClientServerTest(t, mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        })), // nothing.
 (ж<httptest.Server> tsΔ1) => {
            (~tsΔ1).Listener.Close();
            tsΔ1.Value.Listener = new http_test_package.fakeNetListenerжListener(listenerʗ2);
            // Ignore irrelevant error about TLS handshake failure.
            tsΔ1.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        }).Value.ts;
        // Start a connection but never write to it.
        var c = listener.connect();
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);
        synctest.Wait();
        var tsʗ1 = ts;
        var shutdownRes = runAsync((EmptyStruct, error) () => (new EmptyStruct(), (~tsʗ1).Config.Shutdown(context_package.Background())));
        // TODO(#59037): This timeout is hard-coded in closeIdleConnections.
        // It is undocumented, and some users may find it surprising.
        // Either document it, or switch to a less surprising behavior.
        time.Duration expectTimeout = /* 5 * time.Second */ 5000000000;
        // Wait until just before the expected timeout.
        time.Sleep((time.Duration)(4999999999L));
        synctest.Wait();
        if (shutdownRes.done()) {
            t.Fatal(shutdownTooSoonˢ);
        }
        if (c.IsClosedByPeer()) {
            t.Fatal(connectionWasClosedByˢ);
        }
        // closeIdleConnections isn't precise about its actual shutdown time.
        // Wait long enough for it to definitely have shut down.
        //
        // (It would be good to make closeIdleConnections less sloppy.)
        time.Sleep(2 * time.ΔSecond);
        synctest.Wait();
        {
            var (_, err) = shutdownRes.result(); if (err != default!) {
                t.Fatalf("Shutdown() = %v, want complete"u8, err);
            }
        }
        if (!c.IsClosedByPeer()) {
            t.Fatalf("connection was not closed by server after shutdown"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 17878: tests that we can call Close twice.
public static void TestServerCloseDeadlock(ж<testing.T> Ꮡt) {
    ref var s = ref heap(new Δhttp.Server(), out var Ꮡs);
    Ꮡs.Close();
    Ꮡs.Close();
}

// Issue 17717: tests that Server.SetKeepAlivesEnabled is respected by
// both HTTP/1 and HTTP/2.
public static void TestServerKeepAlivesEnabled(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerKeepAlivesEnabled(Δp0, Δp1), testNotParallel);
}

internal static void testServerKeepAlivesEnabled(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (mode == http2Mode) {
            var restore = http_internal_test_package.ExportSetH2GoawayTimeout(10 * time.Millisecond);
            var restoreʗ1 = restore;
            defer(restoreʗ1, ref ᒐ);
        }
        // Not parallel: messes with global variable. (http2goAwayTimeout)
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var srv = cst.Value.ts.Value.Config;
        srv.SetKeepAlivesEnabled(false);
        for (nint @try = 0; @try < 2; @try++) {
            var srvʗ1 = srv;
            waitCondition(new http_test_package.testing_TжTB(Ꮡt), 10 * time.Millisecond, (time.Duration d) => {
                if (!srvʗ1.ExportAllConnsIdle()) {
                    if (d > 0) {
                        Ꮡt.Logf("test server still has active conns after %v"u8, d);
                    }
                    return false;
                }
                return true;
            });
            nint conns = 0;
            ref var info = ref heap(new httptrace.GotConnInfo(), out var Ꮡinfo);
            var ctx = httptrace.WithClientTrace(context_package.Background(), Ꮡ(new httptrace.ClientTrace(
                GotConn: (httptrace.GotConnInfo v) => {
                    conns++;
                    Ꮡinfo.Value = v;
                }
            )));
            var (req, err) = NewRequestWithContext(ctx, getˢ2, (~(~cst).ts).URL, default!);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var res, err) = (~cst).c.Do(req);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (~res).Body.Close();
            if (conns != 1) {
                Ꮡt.Fatalf("request %v: got %v conns, want 1"u8, @try, conns);
            }
            if (info.Reused || info.WasIdle) {
                Ꮡt.Fatalf("request %v: Reused=%v (want false), WasIdle=%v (want false)"u8, @try, info.Reused, info.WasIdle);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 18447: test that the Server's ReadTimeout is stopped while
// the server's doing its 1-byte background read between requests,
// waiting for the connection to maybe close.
public static void TestServerCancelsReadTimeoutWhenIdle(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerCancelsReadTimeoutWhenIdle(Δp0, Δp1));
}

internal static void testServerCancelsReadTimeoutWhenIdle(ж<testing.T> Ꮡt, testMode mode) {
    runTimeSensitiveTest(Ꮡt, new time.Duration[]{
        10 * time.Millisecond,
        50 * time.Millisecond,
        250 * time.Millisecond,
        time.ΔSecond,
        2 * time.ΔSecond
    }.slice(), error (ж<testing.T> tΔ1, time.Duration timeout) => {
        GoFrame ᒐ = default;
        try {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                var selᴛ59 = time.After(2 * timeout);
                var selᴛ60 = r.Context().Done();
                switch (select(ᐸꟷ(selᴛ59, ꓸꓸꓸ), ᐸꟷ(selᴛ60, ꓸꓸꓸ))) {
                case 0 when selᴛ59.ꟷᐳ(out _): {
                    fmt.Fprint(new http_test_package.http_ResponseWriterᴠWriter(w), (@string)"ok"u8);
                    break;
                }
                case 1 when selᴛ60.ꟷᐳ(out _): {
                    fmt.Fprint(new http_test_package.http_ResponseWriterᴠWriter(w), r.Context().Err());
                    break;
                }}
            })), (ж<httptest.Server> tsΔ1) => {
                tsΔ1.Value.Config.Value.ReadTimeout = timeout;
                tΔ1.Logf("Server.Config.ReadTimeout = %v"u8, timeout);
            });
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            ref var retries = ref heap(new atomic.Int32(), out var Ꮡretries);
            (~(~cst).c).Transport._<ж<Δhttp.Transport>>().Value.Proxy = (ж<url.URL>, error) (ж<Δhttp.Request> _) => {
                if (Ꮡretries.Add(1) != 1) {
                    return (default!, errors.New(tooManyRetriesˢ));
                }
                return (default!, default!);
            };
            var c = ts.Client();
            var (res, err) = c.Get((~ts).URL);
            if (err != default!) {
                return fmt.Errorf("Get: %v"u8, err);
            }
            (var slurp, err) = io.ReadAll((~res).Body);
            (~res).Body.Close();
            if (err != default!) {
                return fmt.Errorf("Body ReadAll: %v"u8, err);
            }
            if (((sstring)slurp) != "ok"u8) {
                return fmt.Errorf("got: %q, want ok"u8, slurp);
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// Issue 54784: test that the Server's ReadHeaderTimeout only starts once the
// beginning of a request has been received, rather than including time the
// connection spent idle.
public static void TestServerCancelsReadHeaderTimeoutWhenIdle(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerCancelsReadHeaderTimeoutWhenIdle(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerCancelsReadHeaderTimeoutWhenIdle(ж<testing.T> Ꮡt, testMode mode) {
    runTimeSensitiveTest(Ꮡt, new time.Duration[]{
        10 * time.Millisecond,
        50 * time.Millisecond,
        250 * time.Millisecond,
        time.ΔSecond,
        2 * time.ΔSecond
    }.slice(), error (ж<testing.T> tΔ1, time.Duration timeout) => {
        GoFrame ᒐ = default;
        try {
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(serve(200)), (ж<httptest.Server> tsΔ1) => {
                tsΔ1.Value.Config.Value.ReadHeaderTimeout = timeout;
                tsΔ1.Value.Config.Value.IdleTimeout = 0; // disable idle timeout
            });
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            // rather than using an http.Client, create a single connection, so that
            // we can ensure this connection is not closed.
            var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
            if (err != default!) {
                tΔ1.Fatalf("dial failed: %v"u8, err);
            }
            var br = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
            var connʗ1 = conn;
            defer(() => connʗ1.Close(), ref ᒐ);
            {
                var (_, errΔ1) = conn.Write(slice<byte>("GET / HTTP/1.1\r\nHost: e.com\r\n\r\n"u8)); if (errΔ1 != default!) {
                    return fmt.Errorf("writing first request failed: %v"u8, errΔ1);
                }
            }
            {
                var (_, errΔ2) = ReadResponse(br, nil); if (errΔ2 != default!) {
                    return fmt.Errorf("first response (before timeout) failed: %v"u8, errΔ2);
                }
            }
            // wait for longer than the server's ReadHeaderTimeout, and then send
            // another request
            time.Sleep(timeout * 3 / 2);
            {
                var (_, errΔ3) = conn.Write(slice<byte>("GET / HTTP/1.1\r\nHost: e.com\r\n\r\n"u8)); if (errΔ3 != default!) {
                    return fmt.Errorf("writing second request failed: %v"u8, errΔ3);
                }
            }
            {
                var (_, errΔ4) = ReadResponse(br, nil); if (errΔ4 != default!) {
                    return fmt.Errorf("second response (after timeout) failed: %v"u8, errΔ4);
                }
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// runTimeSensitiveTest runs test with the provided durations until one passes.
// If they all fail, t.Fatal is called with the last one's duration and error value.
internal static void runTimeSensitiveTest(ж<testing.T> Ꮡt, slice<time.Duration> durations, Func<ж<testing.T>, time.Duration, error> test) {
    foreach (var (i, d) in durations) {
        var err = test(Ꮡt, d);
        if (err == default!) {
            return;
        }
        if (i == len(durations) - 1 || Ꮡt.Failed()) {
            Ꮡt.Fatalf("failed with duration %v: %v"u8, d, err);
        }
        Ꮡt.Logf("retrying after error with duration %v: %v"u8, d, err);
    }
}

// Issue 18535: test that the Server doesn't try to do a background
// read if it's already done one.
public static void TestServerDuplicateBackgroundRead(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerDuplicateBackgroundRead(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerDuplicateBackgroundRead(ж<testing.T> Ꮡt, testMode mode) {
    if (runtime.GOOS == "netbsd"u8 && runtime.GOARCH == "arm"u8) {
        testenv.SkipFlaky(new http_test_package.testing_TжTB(Ꮡt), 24826);
    }
    nint goroutines = 5;
    nint requests = 2000;
    if (testing.Short()) {
        goroutines = 3;
        requests = 100;
    }
    var hts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc(NotFound))).Value.ts;
    var reqBytes = slice<byte>("GET / HTTP/1.1\r\nHost: e.com\r\n\r\n"u8);
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < goroutines; i++) {
        Ꮡwg.Add(1);
        var htsʗ1 = hts;
        var reqBytesʗ1 = reqBytes;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (cn, err) = net.Dial(tcpˢ, (~htsʗ1).Listener.Addr().String());
                if (err != default!) {
                    Ꮡt.Error(err);
                    return;
                }
                var cnʗ1 = cn;
                defer(() => cnʗ1.Close(), ref ᒐ);
                Ꮡwg.Add(1);
                var cnʗ2 = cn;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        io.Copy(io.Discard, new http_test_package.net_ConnᴠReader(cnʗ2));
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                for (nint j = 0; j < requests; j++) {
                    if (Ꮡt.Failed()) {
                        return;
                    }
                    var (_, errΔ1) = cn.Write(reqBytesʗ1);
                    if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                        return;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Test that the bufio.Reader returned by Hijack includes any buffered
// byte (from the Server's backgroundRead) in its buffer. We want the
// Handler code to be able to tell that a byte is available via
// bufio.Reader.Buffered(), without resorting to Reading it
// (potentially blocking) to get at it.
public static void TestServerHijackGetsBackgroundByte(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerHijackGetsBackgroundByte(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestSeeHttpsˢ2 = (@string)"skipping test; see https://golang.org/issue/18657"u8;
internal static readonly object contextUnexpectedlyˢ = (@string)"context unexpectedly canceled"u8;

internal static void testServerHijackGetsBackgroundByte(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "plan9"u8) {
            Ꮡt.Skip(skippingTestSeeHttpsˢ2);
        }
        var done = new channel<EmptyStruct>(0);
        var inHandler = new channel<bool>(1);
        var doneʗ1 = done;
        var inHandlerʗ1 = inHandler;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
                // Tell the client to send more data after the GET request.
                inHandlerʗ1.ᐸꟷ(true);
                var (conn, buf, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                (var peek, errΔ1) = (~buf).Reader.Peek(3);
                if (((sstring)peek) != "foo"u8 || errΔ1 != default!) {
                    Ꮡt.Errorf("Peek = %q, %v; want foo, nil"u8, peek, errΔ1);
                }
                var selᴛ61 = r.Context().Done();
                switch (trySelect(ᐸꟷ(selᴛ61, ꓸꓸꓸ))) {
                case 0 when selᴛ61.ꟷᐳ(out _): {
                    Ꮡt.Error(contextUnexpectedlyˢ);
                    break;
                }
                default: {
                    break;
                }}
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))).Value.ts;
        var (cn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cnʗ1 = cn;
        defer(() => cnʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ2) = cn.Write(slice<byte>("GET / HTTP/1.1\r\nHost: e.com\r\n\r\n"u8)); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        ᐸꟷ(inHandler);
        {
            var (_, errΔ3) = cn.Write(slice<byte>("foo"u8)); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        {
            var errΔ4 = cn._<ж<net.TCPConn>>().CloseWrite(); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Like TestServerHijackGetsBackgroundByte above but sending a
// immediate 1MB of data to the server to fill up the server's 4KB
// buffer.
public static void TestServerHijackGetsBackgroundByte_big(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerHijackGetsBackgroundByte_big(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testServerHijackGetsBackgroundByte_big(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "plan9"u8) {
            Ꮡt.Skip(skippingTestSeeHttpsˢ2);
        }
        var done = new channel<EmptyStruct>(0);
        const nint size = /* 8 << 10 */ 8192;
        var doneʗ1 = done;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
                var (conn, buf, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                (var slurp, errΔ1) = io.ReadAll(new http_test_package.bufio_ReaderжReader((~buf).Reader));
                if (errΔ1 != default!) {
                    Ꮡt.Errorf("Copy: %v"u8, errΔ1);
                }
                var allX = true;
                foreach (var (_, v) in slurp) {
                    if (v != (rune)'x') {
                        allX = false;
                    }
                }
                if (len(slurp) != size){
                    Ꮡt.Errorf("read %d; want %d"u8, len(slurp), (nint)(size));
                } else 
                if (!allX) {
                    Ꮡt.Errorf("read %q; want %d 'x'"u8, slurp, (nint)(size));
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))).Value.ts;
        var (cn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var cnʗ1 = cn;
        defer(() => cnʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ2) = fmt.Fprintf(new http_test_package.net_ConnᴠWriter(cn), "GET / HTTP/1.1\r\nHost: e.com\r\n\r\n%s"u8,
                strings.Repeat("x"u8, size)); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            var errΔ3 = cn._<ж<net.TCPConn>>().CloseWrite(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestServerValidatesMethod_tests {
    internal @string method;
    internal nint want;
}

// Issue 18319: test that the Server validates the request method.
public static void TestServerValidatesMethod(ж<testing.T> Ꮡt) {
    var tests = new TestServerValidatesMethod_tests[]{
        new("GET"u8, 200),
        new("GE(T"u8, 400)
    }.slice();
    foreach (var (_, tt) in tests) {
        var conn = newTestConn();
        io.WriteString(new http_test_package.bytes_BufferжWriter(conn.of(testConn.ᏑreadBuf)), tt.method + " / HTTP/1.1\r\nHost: foo.example\r\n\r\n"u8);
        var ln = Ꮡ(new oneConnListener(new http_test_package.testConnжConn(conn)));
        goǃ((ᴛ1, ᴛ2) => Serve(ᴛ1, ᴛ2), new http_test_package.oneConnListenerжListener(ln), new http_test_package.http_HandlerFuncᴠΔHandler(serve(200)));
        ᐸꟷ((~conn).closec);
        var (res, err) = ReadResponse(bufio.NewReader(new http_test_package.bytes_BufferжReader(conn.of(testConn.ᏑwriteBuf))), nil);
        if (err != default!) {
            Ꮡt.Errorf("For %s, ReadResponse: %v"u8, tt.method, res.OrTypedNil());
            continue;
        }
        if ((~res).StatusCode != tt.want) {
            Ꮡt.Errorf("For %s, Status = %d; want %d"u8, tt.method, (~res).StatusCode, tt.want);
        }
    }
}

[GoType("[]nint")] partial struct eofListenerNotComparable;

internal static (net.Conn, error) Accept(this eofListenerNotComparable _) {
    return (default!, io.EOF);
}

internal static netꓸAddr Addr(this eofListenerNotComparable _) {
    return default!;
}

internal static error Close(this eofListenerNotComparable _) {
    return default!;
}

// Issue 24812: don't crash on non-comparable Listener
public static void TestServerListenNotComparableListener(ж<testing.T> Ꮡt) {
    ref var s = ref heap(new Δhttp.Server(), out var Ꮡs);
    Ꮡs.Serve(new eofListenerNotComparable(1)); // used to panic
}

// countCloseListener is a Listener wrapper that counts the number of Close calls.
[GoType] partial struct countCloseListener {
    [GoEmbedded] public net_package.Listener Listener;
    internal int32 closes; // atomic
}

// Go method set entry for the promoted 'Listener.Accept()' - provided ONLY by the embedded
// interface field in *countCloseListener's method set; see the pointer-only satisfaction record.
internal static (net.Conn, error) Accept(this countCloseListener recvᴛ) => recvᴛ.Listener.Accept();

// Go method set entry for the promoted 'Listener.Addr()' - provided ONLY by the embedded
// interface field in *countCloseListener's method set; see the pointer-only satisfaction record.
internal static netꓸAddr Addr(this countCloseListener recvᴛ) => recvᴛ.Listener.Addr();

internal static error Close(this ж<countCloseListener> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    error err = default!;
    {
        var n = atomic.AddInt32(Ꮡp.of(countCloseListener.Ꮡcloses), 1); if (n == 1 && p.Listener != default!) {
            err = p.Listener.Close();
        }
    }
    return err;
}

// Issue 24803: don't call Listener.Close on Server.Shutdown.
public static void TestServerCloseListenerOnce(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var ln = newLocalListener(Ꮡt);
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        var cl = Ꮡ(new countCloseListener(Listener: ln));
        var server = Ꮡ(new Server(nil));
        var sdone = new channel<bool>(1);
        var clʗ1 = cl;
        var sdoneʗ1 = sdone;
        var serverʗ1 = server;
        goǃ(() => {
            serverʗ1.Serve(new http_test_package.countCloseListenerжListener(clʗ1));
            sdoneʗ1.ᐸꟷ(true);
        });
        time.Sleep(10 * time.Millisecond);
        server.Shutdown(context_package.Background());
        ln.Close();
        ᐸꟷ(sdone);
        var nclose = atomic.LoadInt32(cl.of(countCloseListener.Ꮡcloses));
        if (nclose != 1) {
            Ꮡt.Errorf("Close calls = %v; want 1"u8, nclose);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 20239: don't block in Serve if Shutdown is called first.
public static void TestServerShutdownThenServe(ж<testing.T> Ꮡt) {
    ref var srv = ref heap(new Δhttp.Server(), out var Ꮡsrv);
    var cl = Ꮡ(new countCloseListener(Listener: default!));
    Ꮡsrv.Shutdown(context_package.Background());
    var got = Ꮡsrv.Serve(new http_test_package.countCloseListenerжListener(cl));
    if (!AreEqual(got, ErrServerClosed)) {
        Ꮡt.Errorf("Serve err = %v; want ErrServerClosed"u8, got);
    }
    var nclose = atomic.LoadInt32(cl.of(countCloseListener.Ꮡcloses));
    if (nclose != 1) {
        Ꮡt.Errorf("Close calls = %v; want 1"u8, nclose);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleComˢ2 = "example.com/"u8;
internal static readonly @string httpExampleCom9000ˢ = "http://example.com:9000/"u8;

// Issue 23351: document and test behavior of ServeMux with ports
public static void TestStripPortFromHost(ж<testing.T> Ꮡt) {
    var mux = NewServeMux();
    mux.HandleFunc(exampleComˢ2, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "OK"u8);
    });
    mux.HandleFunc(exampleCom9000ˢ, (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "uh-oh!"u8);
    });
    var req = httptest.NewRequest(getˢ2, httpExampleCom9000ˢ, default!);
    var rw = httptest.NewRecorder();
    mux.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    @string response = (~rw).Body.String();
    if (response != "OK"u8) {
        Ꮡt.Errorf("Response gotten was %q"u8, response);
    }
}

public static void TestServerContexts(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerContexts(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string onceCloseˢ = "onceClose"u8;
internal static readonly @string baseˢ = "base"u8;
internal static readonly @string connˢ = "conn"u8;

[GoType("dyn")] internal partial struct testServerContexts_baseKey {
}

[GoType("dyn")] internal partial struct testServerContexts_connKey {
}

internal static void testServerContexts(ж<testing.T> Ꮡt, testMode mode) {
    var ch = new channel<context.Context>(1);
    var chʗ1 = ch;

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        chʗ1.ᐸꟷ(r.Context());
    })), (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Config.Value.BaseContext = (net.Listener ln) => {
            if (strings.Contains(reflect.TypeOf(ln).String(), onceCloseˢ)) {
                Ꮡt.Errorf("unexpected onceClose listener type %T"u8, ln);
            }
            return context_package.WithValue(context_package.Background(), new testServerContexts_baseKey(nil), baseˢ);
        };
        tsΔ1.Value.Config.Value.ConnContext = (context.Context ctxΔ1, net.Conn c) => {
            {
                var got = ctxΔ1.Value(new testServerContexts_baseKey(nil));
                @string want = baseˢ; if (!AreEqual(got, want)) {
                    Ꮡt.Errorf("in ConnContext, base context key = %#v; want %q"u8, got, want);
                }
            }
            return context_package.WithValue(ctxΔ1, new testServerContexts_connKey(nil), connˢ);
        };
    }).Value.ts;
    var (res, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    var ctx = ᐸꟷ(ch);
    {
        var got = ctx.Value(new testServerContexts_baseKey(nil));
        @string want = baseˢ; if (!AreEqual(got, want)) {
            Ꮡt.Errorf("base context key = %#v; want %q"u8, got, want);
        }
    }
    {
        var got = ctx.Value(new testServerContexts_connKey(nil));
        @string want = connˢ; if (!AreEqual(got, want)) {
            Ꮡt.Errorf("conn context key = %#v; want %q"u8, got, want);
        }
    }
}

// Issue 35750: check ConnContext not modifying context for other connections
public static void TestConnContextNotModifyingAllContexts(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testConnContextNotModifyingAllContexts(Δp0, Δp1));
}

[GoType("dyn")] internal partial struct testConnContextNotModifyingAllContexts_connKey {
}

internal static void testConnContextNotModifyingAllContexts(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
        rw.Header().Set(connectionˢ, closeˢ);
    })), (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Config.Value.ConnContext = (context.Context ctx, net.Conn c) => {
            {
                var got = ctx.Value(new testConnContextNotModifyingAllContexts_connKey(nil)); if (got != default!) {
                    Ꮡt.Errorf("in ConnContext, unexpected context key = %#v"u8, got);
                }
            }
            return context_package.WithValue(ctx, new testConnContextNotModifyingAllContexts_connKey(nil), connˢ);
        };
    }).Value.ts;
    ж<Δhttp.Response> res = default!;
    error err = default!;
    (res, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    (res, err) = ts.Client().Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
}

// Issue 30710: ensure that as per the spec, a server responds
// with 501 Not Implemented for unsupported transfer-encodings.
public static void TestUnsupportedTransferEncodingsReturn501(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testUnsupportedTransferEncodingsReturn501(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testUnsupportedTransferEncodingsReturn501(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello, World!"u8));
    }))).Value.ts;
    var (serverURL, err) = url.Parse((~cst).URL);
    if (err != default!) {
        Ꮡt.Fatalf("Failed to parse server URL: %v"u8, err);
    }
    var unsupportedTEs = new @string[]{
        "fugazi"u8,
        "foo-bar"u8,
        "unknown"u8,
        @""" chunked"""u8
    }.slice();
    foreach (var (_, badTE) in unsupportedTEs) {
        @string http1ReqBody = fmt.Sprintf(""u8 + "POST / HTTP/1.1\r\nConnection: close\r\n"u8 + "Host: localhost\r\nTransfer-Encoding: %s\r\n\r\n"u8, badTE);
        var (gotBody, errΔ1) = fetchWireResponse((~serverURL).Host, slice<byte>(http1ReqBody));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("%q. unexpected error: %v"u8, badTE, errΔ1);
            continue;
        }
        @string wantBody = fmt.Sprintf(""u8 + "HTTP/1.1 501 Not Implemented\r\nContent-Type: text/plain; charset=utf-8\r\n"u8 + "Connection: close\r\n\r\nUnsupported transfer encoding"u8);
        if (((sstring)gotBody) != wantBody) {
            Ꮡt.Errorf("%q. body\ngot\n%q\nwant\n%q"u8, badTE, gotBody, wantBody);
        }
    }
}

// Issue 31753: don't sniff when Content-Encoding is set
public static void TestContentEncodingNoSniffing(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testContentEncodingNoSniffing(Δp0, Δp1));
}

[GoType("dyn")] internal partial struct testContentEncodingNoSniffing_setting {
    internal @string name;
    internal slice<byte> body;
    // setting contentEncoding as an interface instead of a string
    // directly, so as to differentiate between 3 states:
    //    unset, empty string "" and set string "foo/bar".
    internal any contentEncoding;
    internal @string wantContentType;
}

internal static void testContentEncodingNoSniffing(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var settings = new ж<testContentEncodingNoSniffing_setting>[]{
        Ꮡ(new testContentEncodingNoSniffing_setting(
            name: "gzip content-encoding, gzipped"u8, // don't sniff.

            contentEncoding: (@string)"application/gzip"u8,
            wantContentType: ""u8,
            body: ((Func<slice<byte>>)(() => {
                var buf = @new<bytes.Buffer>();
                var gzw = gzip.NewWriter(new http_test_package.bytes_BufferжWriter(buf));
                gzw.Write(slice<byte>("doctype html><p>Hello</p>"u8));
                gzw.Close();
                return buf.Bytes();
            }))())),
        Ꮡ(new testContentEncodingNoSniffing_setting(
            name: "zlib content-encoding, zlibbed"u8, // don't sniff.

            contentEncoding: (@string)"application/zlib"u8,
            wantContentType: ""u8,
            body: ((Func<slice<byte>>)(() => {
                var buf = @new<bytes.Buffer>();
                var zw = zlib.NewWriter(new http_test_package.bytes_BufferжWriter(buf));
                zw.Write(slice<byte>("doctype html><p>Hello</p>"u8));
                zw.Close();
                return buf.Bytes();
            }))())),
        Ꮡ(new testContentEncodingNoSniffing_setting(
            name: "no content-encoding"u8, // must sniff.

            wantContentType: "application/x-gzip"u8,
            body: ((Func<slice<byte>>)(() => {
                var buf = @new<bytes.Buffer>();
                var gzw = gzip.NewWriter(new http_test_package.bytes_BufferжWriter(buf));
                gzw.Write(slice<byte>("doctype html><p>Hello</p>"u8));
                gzw.Close();
                return buf.Bytes();
            }))())),
        Ꮡ(new testContentEncodingNoSniffing_setting(
            name: "phony content-encoding"u8, // don't sniff.

            contentEncoding: (@string)"foo/bar"u8,
            body: slice<byte>("doctype html><p>Hello</p>"u8))),
        Ꮡ(new testContentEncodingNoSniffing_setting(
            name: "empty but set content-encoding"u8,
            contentEncoding: (@string)""u8,
            wantContentType: "audio/mpeg"u8,
            body: slice<byte>("ID3"u8)))
    }.slice();
    foreach (var (_, tt) in settings) {
        var ttʗ1 = tt;
        Ꮡt.Run((~tt).name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var ttʗ2 = ttʗ1;
                var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> r) => {
                    if ((~ttʗ2).contentEncoding != default!) {
                        rw.Header().Set(contentEncodingˢ, (~ttʗ2).contentEncoding._<@string>());
                    }
                    rw.Write((~ttʗ2).body);
                })));
                var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
                if (err != default!) {
                    tΔ1.Fatalf("Failed to fetch URL: %v"u8, err);
                }
                var resʗ1 = res;
                defer(() => (~resʗ1).Body.Close(), ref ᒐ);
                {
                    @string g = (~res).Header.Get(contentEncodingˢ);
                    var w = ttʗ1.Value.contentEncoding; if (!AreEqual(g, w)) {
                        if (w != default!){
                            // The case where contentEncoding was set explicitly.
                            tΔ1.Errorf("Content-Encoding mismatch\n\tgot:  %q\n\twant: %q"u8, g, w);
                        } else 
                        if (g != ""u8) {
                            // "" should be the equivalent when the contentEncoding is unset.
                            tΔ1.Errorf("Unexpected Content-Encoding %q"u8, g);
                        }
                    }
                }
                {
                    @string g = (~res).Header.Get(contentTypeˢ);
                    @string w = ttʗ1.Value.wantContentType; if (g != w) {
                        tΔ1.Errorf("Content-Type mismatch\n\tgot:  %q\n\twant: %q"u8, g, w);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Issue 30803: ensure that TimeoutHandler logs spurious
// WriteHeader calls, for consistency with other Handlers.
public static void TestTimeoutHandlerSuperfluousLogs(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTimeoutHandlerSuperfluousLogs(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timedOutHereˢ = "timed out here!"u8;

[GoType("dyn")] internal partial struct testTimeoutHandlerSuperfluousLogs_tests {
    internal @string name;
    internal bool mustTimeout;
    internal @string wantResp;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void testTimeoutHandlerSuperfluousLogs(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    var (pc, curFile, _, _) = runtime.Caller(0);
    @string curFileBaseName = filepath.Base(curFile);
    @string testFuncName = runtime.FuncForPC(pc).Name();
    @string timeoutMsg = timedOutHereˢ;
    var tests = new testTimeoutHandlerSuperfluousLogs_tests[]{
        new(
            name: "return before timeout"u8,
            wantResp: "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\n\r\n"u8
        ),
        new(
            name: "return after timeout"u8,
            mustTimeout: true,
            wantResp: fmt.Sprintf("HTTP/1.1 503 Service Unavailable\r\nContent-Length: %d\r\n\r\n%s"u8,
                len(timeoutMsg), timeoutMsg)
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        ref var ttΔ1 = ref heap<testTimeoutHandlerSuperfluousLogs_tests>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(ttΔ1.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var exitHandler = new channel<bool>(1);
                defer(ᴛ1 => builtin.close(ᴛ1), exitHandler, ref ᒐ);
                var lastLine = new channel<nint>(1);
                var exitHandlerʗ1 = exitHandler;
                var lastLineʗ1 = lastLine;
                var sh = new Δhttp.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                    w.WriteHeader(404);
                    w.WriteHeader(404);
                    w.WriteHeader(404);
                    w.WriteHeader(404);
                    var (_, _, line, _) = runtime.Caller(0);
                    lastLineʗ1.ᐸꟷ(line);
                    ᐸꟷ(exitHandlerʗ1);
                });
                if (!ttʗ1.mustTimeout) {
                    exitHandler.ᐸꟷ(true);
                }
                var logBuf = @new<strings.Builder>();
                var srvLog = log.New(new http_test_package.strings_BuilderжWriter(logBuf), ""u8, 0);
                // When expecting to timeout, we'll keep the duration short.
                var dur = 20 * time.Millisecond;
                if (!ttʗ1.mustTimeout) {
                    // Otherwise, make it arbitrarily long to reduce the risk of flakes.
                    dur = (time.Duration)(10000000000L);
                }
                var th = TimeoutHandler(new http_test_package.http_HandlerFuncᴠΔHandler(sh), dur, timeoutMsg);
                var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, th, (optWithServerLog(srvLog)).OrTypedNilFunc());
                var cstʗ1 = cst;
                defer(cstʗ1.close, ref ᒐ);
                var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
                if (err != default!) {
                    tΔ1.Fatalf("Unexpected error: %v"u8, err);
                }
                // Deliberately removing the "Date" header since it is highly ephemeral
                // and will cause failure if we try to match it exactly.
                (~res).Header.Del(dateˢ);
                (~res).Header.Del(contentTypeˢ);
                // Match the response.
                var (blob, _) = httputil.DumpResponse(res, true);
                {
                    @string g = ((@string)blob);
                    @string w = ttʗ1.wantResp; if (g != w) {
                        tΔ1.Errorf("Response mismatch\nGot\n%q\n\nWant\n%q"u8, g, w);
                    }
                }
                // Given 4 w.WriteHeader calls, only the first one is valid
                // and the rest should be reported as the 3 spurious logs.
                var logEntries = strings.Split(strings.TrimSpace(logBuf.String()), "\n"u8);
                {
                    nint g = len(logEntries);
                    nint w = 3; if (g != w) {
                        var (blobΔ1, _) = json.MarshalIndent(logEntries, ""u8, "  "u8);
                        tΔ1.Fatalf("Server logs count mismatch\ngot %d, want %d\n\nGot\n%s\n"u8, g, w, blobΔ1);
                    }
                }
                nint lastSpuriousLine = ᐸꟷ(lastLine);
                nint firstSpuriousLine = lastSpuriousLine - 3;
                // Now ensure that the regexes match exactly.
                //      "http: superfluous response.WriteHeader call from <fn>.func\d.\d (<curFile>:lastSpuriousLine-[1, 3]"
                foreach (var (i, logEntry) in logEntries) {
                    nint wantLine = firstSpuriousLine + i;
                    @string pat = fmt.Sprintf("^http: superfluous response.WriteHeader call from %s.func\\d+.\\d+ \\(%s:%d\\)$"u8,
                        testFuncName, curFileBaseName, wantLine);
                    var re = regexp.MustCompile(pat);
                    if (!re.MatchString(logEntry)) {
                        tΔ1.Errorf("Log entry mismatch\n\t%s\ndoes not match\n\t%s"u8, logEntry, pat);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// fetchWireResponse is a helper for dialing to host,
// sending http1ReqBody as the payload and retrieving
// the response as it was sent on the wire.
internal static (slice<byte>, error) fetchWireResponse(@string host, slice<byte> http1ReqBody) {
    GoFrame ᒐ = default;
    try {
        var (conn, err) = net.Dial(tcpˢ, host);
        if (err != default!) {
            return (default!, err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = conn.Write(http1ReqBody); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        return io.ReadAll(new http_test_package.net_ConnᴠReader(conn));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static void BenchmarkResponseStatusLine(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var bw = bufio.NewWriter(io.Discard);
        array<byte> buf3 = new(3);
        while (pb.Next()) {
            http_internal_test_package.Export_writeStatusLine(bw, true, 200, buf3[..]);
        }
    });
}

public static void TestDisableKeepAliveUpgrade(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testDisableKeepAliveUpgrade(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someProtoˢ = "someProto"u8;

internal static void testDisableKeepAliveUpgrade(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        var s = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                w.Header().Set(connectionˢ, upgradeˢ);
                w.Header().Set(upgradeˢ, someProtoˢ);
                w.WriteHeader(StatusSwitchingProtocols);
                var (c, buf, errΔ1) = w._<Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    return;
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                // Copy from the *bufio.ReadWriter, which may contain buffered data.
                // Copy to the net.Conn, to avoid buffering the output.
                io.Copy(new http_test_package.net_ConnᴠWriter(c), new http_test_package.bufio_ReadWriterжReader(buf));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })), (ж<httptest.Server> ts) => {
            (~ts).Config.SetKeepAlivesEnabled(false);
        }).Value.ts;
        var cl = s.Client();
        (~cl).Transport._<ж<Δhttp.Transport>>().Value.DisableKeepAlives = true;
        var (resp, err) = cl.Get((~s).URL);
        if (err != default!) {
            Ꮡt.Fatalf("failed to perform request: %v"u8, err);
        }
        var respʗ1 = resp;
        defer(() => (~respʗ1).Body.Close(), ref ᒐ);
        if ((~resp).StatusCode != StatusSwitchingProtocols) {
            Ꮡt.Fatalf("unexpected status code: %v"u8, (~resp).StatusCode);
        }
        var (rwc, ok) = (~resp).Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("Response.Body is not an io.ReadWriteCloser: %T"u8, (~resp).Body);
        }
        (_, err) = rwc.Write(slice<byte>("hello"u8));
        if (err != default!) {
            Ꮡt.Fatalf("failed to write to body: %v"u8, err);
        }
        var b = new slice<byte>(5);
        (_, err) = io.ReadFull(rwc, b);
        if (err != default!) {
            Ꮡt.Fatalf("failed to read from body: %v"u8, err);
        }
        if (((sstring)b) != "hello"u8) {
            Ꮡt.Fatalf("unexpected value read from body:\ngot: %q\nwant: %q"u8, b, helloˢ3);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct tlogWriter {
    internal ж<testing.T> t;
}

internal static (nint, error) Write(this tlogWriter w, slice<byte> p) {
    w.t.Log(((@string)p));
    return (len(p), default!);
}

public static void TestWriteHeaderSwitchingProtocols(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testWriteHeaderSwitchingProtocols(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string logˢ = "log: "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantBodyᶜ1 = "want"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantUpgradeᶜ = "someProto"u8;

internal static void testWriteHeaderSwitchingProtocols(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string wantBody = wantBodyᶜ1;
        @string wantUpgrade = wantUpgradeᶜ;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> rΔ1) => {
            GoFrame ᒐ = default;
            try {
                w.Header().Set(connectionˢ, upgradeˢ);
                w.Header().Set(upgradeˢ, wantUpgrade);
                w.WriteHeader(StatusSwitchingProtocols);
                NewResponseController(w).Flush();
                // Writing headers or the body after sending a 101 header should fail.
                w.WriteHeader(200);
                {
                    var (_, errΔ1) = w.Write(slice<byte>("x"u8)); if (errΔ1 == default!) {
                        Ꮡt.Errorf("Write to body after 101 Switching Protocols unexpectedly succeeded"u8);
                    }
                }
                var (c, _, errΔ2) = NewResponseController(w).Hijack();
                if (errΔ2 != default!) {
                    Ꮡt.Errorf("Hijack: %v"u8, errΔ2);
                    return;
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                {
                    var (_, errΔ3) = c.Write(slice<byte>(wantBody)); if (errΔ3 != default!) {
                        Ꮡt.Errorf("Write to hijacked body: %v"u8, errΔ3);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })), (ж<httptest.Server> tsΔ1) => {
            // Don't spam log with warning about superfluous WriteHeader call.
            tsΔ1.Value.Config.Value.ErrorLog = log.New(new tlogWriter(Ꮡt), logˢ, 0);
        }).Value.ts;
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatalf("net.Dial: %v"u8, err);
        }
        (_, err) = conn.Write(slice<byte>("GET / HTTP/1.1\r\nHost: foo\r\n\r\n"u8));
        if (err != default!) {
            Ꮡt.Fatalf("conn.Write: %v"u8, err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var r = bufio.NewReader(new http_test_package.net_ConnᴠReader(conn));
        (var res, err) = ReadResponse(r, Ꮡ(new Request(Method: "GET"u8)));
        if (err != default!) {
            Ꮡt.Fatal(readResponseErrorˢ, err);
        }
        if ((~res).StatusCode != StatusSwitchingProtocols) {
            Ꮡt.Errorf("Response StatusCode=%v, want 101"u8, (~res).StatusCode);
        }
        {
            @string got = (~res).Header.Get(upgradeˢ); if (got != wantUpgrade) {
                Ꮡt.Errorf("Response Upgrade header = %q, want %q"u8, got, wantUpgrade);
            }
        }
        (var body, err) = io.ReadAll(new http_test_package.bufio_ReaderжReader(r));
        if (err != default!) {
            Ꮡt.Error(err);
        }
        if (((sstring)body) != wantBody) {
            Ꮡt.Errorf("Response body = %q, want %q"u8, ((@string)body), wantBody);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttpExampleComHttp11ˢ = "GET http://example.com HTTP/1.1\r\nHost: test\r\n\r\n"u8;

public static void TestMuxRedirectRelative(ж<testing.T> Ꮡt) {
    setParallel(Ꮡt);
    var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(getHttpExampleComHttp11ˢ))));
    if (err != default!) {
        Ꮡt.Errorf("%s"u8, err);
    }
    var mux = NewServeMux();
    var resp = httptest.NewRecorder();
    mux.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(resp), req);
    {
        @string got = resp.Header().Get(locationˢ);
        @string want = "/"u8; if (got != want) {
            Ꮡt.Errorf("Location header expected %q; got %q"u8, want, got);
        }
    }
    {
        nint got = resp.Value.Code;
        nint want = StatusMovedPermanently; if (got != want) {
            Ꮡt.Errorf("Expected response code %d; got %d"u8, want, got);
        }
    }
}

[GoType("dyn")] internal partial struct TestQuerySemicolon_tests {
    internal @string query;
    internal @string xNoSemicolons;
    internal @string xWithSemicolons;
    internal bool expectParseFormErr;
}

// TestQuerySemicolon tests the behavior of semicolons in queries. See Issue 25192.
public static void TestQuerySemicolon(ж<testing.T> Ꮡt) {
    Ꮡt.Cleanup(() => {
        afterTest(new http_test_package.testing_TжTB(Ꮡt));
    });
    var tests = new TestQuerySemicolon_tests[]{
        new("?a=1;x=bad&x=good"u8, "good"u8, "bad"u8, true),
        new("?a=1;b=bad&x=good"u8, "good"u8, "good"u8, true),
        new("?a=1%3Bx=bad&x=good%3B"u8, "good;"u8, "good;"u8, false),
        new("?a=1;x=good;x=bad"u8, ""u8, "good"u8, true)
    }.slice();
    var testsʗ1 = tests;
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        foreach (var (_, vᴛ1) in testsʗ1) {
            ref var tt = ref heap(new TestQuerySemicolon_tests(), out var Ꮡtt);
            tt = vᴛ1;

            var ttʗ1 = tt;
            tΔ1.Run(tt.query + "/allow=false"u8, (ж<testing.T> tΔ2) => {
                var allowSemicolons = false;
                testQuerySemicolon(tΔ2, mode, ttʗ1.query, ttʗ1.xNoSemicolons, allowSemicolons, ttʗ1.expectParseFormErr);
            });
            var ttʗ2 = tt;
            tΔ1.Run(tt.query + "/allow=true"u8, (ж<testing.T> tΔ3) => {
                var (allowSemicolons, expectParseFormErr) = (true, false);
                testQuerySemicolon(tΔ3, mode, ttʗ2.query, ttʗ2.xWithSemicolons, allowSemicolons, expectParseFormErr);
            });
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string semicolonˢ = "semicolon"u8;

internal static void testQuerySemicolon(ж<testing.T> Ꮡt, testMode mode, @string query, @string wantX, bool allowSemicolons, bool expectParseFormErr) {
    var writeBackX = (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        @string x = (~r).URL.Query().Get("x"u8);
        if (expectParseFormErr){
            {
                var errΔ1 = r.ParseForm(); if (errΔ1 == default! || !strings.Contains(errΔ1.Error(), semicolonˢ)) {
                    Ꮡt.Errorf("expected error mentioning semicolons from ParseForm, got %v"u8, errΔ1);
                }
            }
        } else {
            {
                var errΔ2 = r.ParseForm(); if (errΔ2 != default!) {
                    Ꮡt.Errorf("expected no error from ParseForm, got %v"u8, errΔ2);
                }
            }
        }
        {
            @string got = r.FormValue("x"u8); if (x != got) {
                Ꮡt.Errorf("got %q from FormValue, want %q"u8, got, x);
            }
        }
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%s"u8, x);
    };
    var h = ((httpꓸHandler)new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(writeBackX)));
    if (allowSemicolons) {
        h = AllowQuerySemicolons(h);
    }
    var logBuf = Ꮡ(new strings.Builder(nil));
    var logBufʗ1 = logBuf;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, h, (ж<httptest.Server> tsΔ1) => {
        tsΔ1.Value.Config.Value.ErrorLog = log.New(new http_test_package.strings_BuilderжWriter(logBufʗ1), ""u8, 0);
    }).Value.ts;
    var (req, _) = NewRequest(getˢ2, (~ts).URL + query, default!);
    var (res, err) = ts.Client().Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (slurp, _) = io.ReadAll((~res).Body);
    (~res).Body.Close();
    {
        nint got = res.Value.StatusCode;
        nint want = 200; if (got != want) {
            Ꮡt.Errorf("Status = %d; want = %d"u8, got, want);
        }
    }
    {
        @string got = ((@string)slurp);
        @string want = wantX; if (got != want) {
            Ꮡt.Errorf("Body = %q; want = %q"u8, got, want);
        }
    }
}

public static void TestMaxBytesHandler(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Not parallel: modifies the global rstAvoidanceDelay.
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        foreach (var (_, maxSize) in new int64[]{100, 1_000, 1_000_000}.slice()) {
            foreach (var (_, requestSize) in new int64[]{100, 1_000, 1_000_000}.slice()) {
                Ꮡt.Run(fmt.Sprintf("max size %d request size %d"u8, maxSize, requestSize),
                    (ж<testing.T> tΔ1) => {
                        run<TжTBRun>(tΔ1, (TжTBRun tΔ2Δp, testMode mode) => {
                            var tΔ2 = (ж<testing.T>)tΔ2Δp;
                            testMaxBytesHandler(tΔ2, mode, maxSize, requestSize);
                        }, testNotParallel);
                    });
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorOnHandlerˢ = (@string)"expected error on handler side; got nil"u8;

internal static void testMaxBytesHandler(ж<testing.T> Ꮡt, testMode mode, int64 maxSize, int64 requestSize) {
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
            int64 handlerN = default!;
            ref var handlerErr = ref heap<error>(out var ᏑhandlerErr);
            var echo = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                ref var bufΔ1 = ref heap(new bytes.Buffer(), out var ᏑbufΔ1);
                (handlerN, ᏑhandlerErr.ValueSlot) = io.Copy(new http_test_package.bytes_BufferжWriter(ᏑbufΔ1), (~r).Body);
                io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), new http_test_package.bytes_BufferжReader(ᏑbufΔ1));
            });
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, MaxBytesHandler(new http_test_package.http_HandlerFuncᴠΔHandler(echo), maxSize));
            // We need to close cst explicitly here so that in-flight server
            // requests don't race with the call to SetRSTAvoidanceDelay for a retry.
            var cstʗ1 = cst;
            defer(cstʗ1.close, ref ᒐ);
            var ts = cst.Value.ts;
            var c = ts.Client();
            @string body = strings.Repeat("a"u8, (nint)requestSize);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            defer(Ꮡwg.Wait, ref ᒐ);
            var getBody = (io.ReadCloser, error) () => {
                Ꮡwg.Add(1);
                var bodyΔ1 = Ꮡ(new wgReadCloser(
                    Reader: new http_test_package.strings_ReaderжReader(strings.NewReader(body)),
                    wg: Ꮡwg
                ));
                return (new http_test_package.wgReadCloserжReadCloser(bodyΔ1), default!);
            };
            var (reqBody, _) = getBody();
            var (req, err) = NewRequest(postˢ, (~ts).URL, reqBody);
            if (err != default!) {
                reqBody.Close();
                tΔ1.Fatal(err);
            }
            req.Value.ContentLength = (int64)len(body);
            req.Value.GetBody = getBody;
            (~req).Header.Set(contentTypeˢ, textPlainˢ);
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            (var res, err) = c.Do(req);
            if (err != default!){
                return fmt.Errorf("unexpected connection error: %v"u8, err);
            } else {
                (_, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbuf), (~res).Body);
                (~res).Body.Close();
                if (err != default!) {
                    return fmt.Errorf("unexpected read error: %v"u8, err);
                }
            }
            // We don't expect any of the errors after this point to occur due
            // to rstAvoidanceDelay being too short, so we use t.Errorf for those
            // instead of returning a (retriable) error.
            if (handlerN > maxSize) {
                tΔ1.Errorf("expected max request body %d; got %d"u8, maxSize, handlerN);
            }
            if (requestSize > maxSize && ᏑhandlerErr.ValueSlot == default!) {
                tΔ1.Error(expectedErrorOnHandlerˢ);
            }
            if (requestSize <= maxSize) {
                if (ᏑhandlerErr.ValueSlot != default!) {
                    tΔ1.Errorf("%d expected nil error on handler side; got %v"u8, requestSize, ᏑhandlerErr.ValueSlot);
                }
                if (handlerN != requestSize) {
                    tΔ1.Errorf("expected request of size %d; got %d"u8, requestSize, handlerN);
                }
            }
            if (buf.Len() != (nint)handlerN) {
                tΔ1.Errorf("expected echo of size %d; got %d"u8, handlerN, buf.Len());
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11103EarlyHintsLinkˢ = "HTTP/1.1 103 Early Hints\r\nLink: </style.css>; rel=preload; as=style\r\nLink: </script.js>; rel=preload; as=script\r\n\r\nHTTP/1.1 103 Early Hints\r\nLink: </style.css>; rel=preload; as=style\r\nLink: </script.js>; rel=preload; as=script\r\nLink: </foo.js>; rel=preload; as=script\r\n\r\nHTTP/1.1 200 OK\r\nLink: </style.css>; rel=preload; as=style\r\nLink: </script.js>; rel=preload; as=script\r\nLink: </foo.js>; rel=preload; as=script\r\nDate: "u8;

public static void TestEarlyHints(ж<testing.T> Ꮡt) {
    ref var ht = ref heap<handlerTest>(out var Ꮡht);
    ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var h = w.Header();
        h.Add(linkˢ, styleCssRelPreloadAsˢ);
        h.Add(linkˢ, scriptJsRelPreloadAsˢ);
        w.WriteHeader(StatusEarlyHints);
        h.Add(linkˢ, fooJsRelPreloadAsScriptˢ);
        w.WriteHeader(StatusEarlyHints);
        w.Write(slice<byte>("stuff"u8));
    })));
    @string got = Ꮡht.rawResponse(getHttp11HostGolangOrgˢ);
    @string expected = http11103EarlyHintsLinkˢ; // dynamic content expected
    if (!strings.Contains(got, expected)) {
        Ꮡt.Errorf("unexpected response; got %q; should start by %q"u8, got, expected);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11102ProcessingHttp1ˢ = "HTTP/1.1 102 Processing\r\n\r\nHTTP/1.1 200 OK\r\nDate: "u8;

public static void TestProcessing(ж<testing.T> Ꮡt) {
    ref var ht = ref heap<handlerTest>(out var Ꮡht);
    ht = newHandlerTest(new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(StatusProcessing);
        w.Write(slice<byte>("stuff"u8));
    })));
    @string got = Ꮡht.rawResponse(getHttp11HostGolangOrgˢ);
    @string expected = http11102ProcessingHttp1ˢ; // dynamic content expected
    if (!strings.Contains(got, expected)) {
        Ꮡt.Errorf("unexpected response; got %q; should start by %q"u8, got, expected);
    }
}

public static void TestParseFormCleanup(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testParseFormCleanup(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue20253ˢ = (@string)"https://go.dev/issue/20253"u8;
internal static readonly object httpsGoDevIssue25965ˢ = (@string)"https://go.dev/issue/25965"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string keyᶜ = "file"u8;

internal static void testParseFormCleanup(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        if (mode == http2Mode) {
            Ꮡt.Skip(httpsGoDevIssue20253ˢ);
        }
        UntypedInt maxMemory = 1024;
        @string key = keyᶜ;
        if (runtime.GOOS == "windows"u8) {
            // Windows sometimes refuses to remove a file that was just closed.
            Ꮡt.Skip(httpsGoDevIssue25965ˢ);
        }
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            r.ParseMultipartForm(maxMemory);
            var (f, _, errΔ1) = r.FormFile(key);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("r.FormFile(%q) = %v"u8, key, errΔ1);
                return;
            }
            var (of, ok) = f._<ж<os.File>>(ᐧ);
            if (!ok) {
                Ꮡt.Errorf("r.FormFile(%q) returned type %T, want *os.File"u8, key, f);
                return;
            }
            w.Write(slice<byte>(of.Name()));
        })));
        var fBuf = @new<bytes.Buffer>();
        var mw = multipart.NewWriter(new http_test_package.bytes_BufferжWriter(fBuf));
        var (mf, err) = mw.CreateFormFile(key, myfileTxtˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            var (_, errΔ2) = mf.Write(bytes.Repeat(slice<byte>("A"u8), maxMemory * 2)); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            var errΔ3 = mw.Close(); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        (var req, err) = NewRequest(postˢ, (~(~cst).ts).URL, new http_test_package.bytes_BufferжReader(fBuf));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~req).Header.Set(contentTypeˢ, mw.FormDataContentType());
        (var res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var fname, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        cst.close();
        {
            var (_, errΔ4) = os.Stat(((@string)fname)); if (!errors.Is(errΔ4, os.ErrNotExist)) {
                Ꮡt.Errorf("file %q exists after HTTP handler returned"u8, ((@string)fname));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestHeadBody(ж<testing.T> Ꮡt) {
    const bool identityMode = false;
    const bool chunkedMode = true;
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(identityˢ, (ж<testing.T> tΔ2) => {
            testHeadBody(tΔ2, mode, identityMode, headˢ);
        });
        tΔ1.Run(chunkedˢ, (ж<testing.T> tΔ3) => {
            testHeadBody(tΔ3, mode, chunkedMode, headˢ);
        });
    });
}

public static void TestGetBody(ж<testing.T> Ꮡt) {
    const bool identityMode = false;
    const bool chunkedMode = true;
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(identityˢ, (ж<testing.T> tΔ2) => {
            testHeadBody(tΔ2, mode, identityMode, getˢ2);
        });
        tΔ1.Run(chunkedˢ, (ж<testing.T> tΔ3) => {
            testHeadBody(tΔ3, mode, chunkedMode, getˢ2);
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xRequestBodyˢ = "X-Request-Body"u8;

internal static void testHeadBody(ж<testing.T> Ꮡt, testMode mode, bool chunked, @string method) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (b, err) = io.ReadAll((~r).Body);
            if (err != default!) {
                Ꮡt.Errorf("server reading body: %v"u8, err);
                return;
            }
            w.Header().Set(xRequestBodyˢ, ((@string)b));
            w.Header().Set(contentLengthˢ, "0"u8);
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        foreach (var (_, reqBody) in new @string[]{
            ""u8,
            ""u8,
            "request_body"u8,
            ""u8
        }.slice()) {
            io.Reader bodyReader = default!;
            if (reqBody != ""u8) {
                bodyReader = new http_test_package.strings_ReaderжReader(strings.NewReader(reqBody));
                if (chunked) {
                    bodyReader = new http_test_package.bufio_ReaderжReader(bufio.NewReader(bodyReader));
                }
            }
            var (req, err) = NewRequest(method, (~(~cst).ts).URL, bodyReader);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (var res, err) = (~cst).c.Do(req);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            (~res).Body.Close();
            {
                nint got = res.Value.StatusCode;
                nint want = 200; if (got != want) {
                    Ꮡt.Errorf("%v request with %d-byte body: StatusCode = %v, want %v"u8, method, len(reqBody), got, want);
                }
            }
            {
                @string got = (~res).Header.Get(xRequestBodyˢ);
                @string want = reqBody; if (got != want) {
                    Ꮡt.Errorf("%v request with %d-byte body: handler read body %q, want %q"u8, method, len(reqBody), got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// TestDisableContentLength verifies that the Content-Length is set by default
// or disabled when the header is set to nil.
public static void TestDisableContentLength(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testDisableContentLength(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingUntilH2BundleGoˢ = (@string)"skipping until h2_bundle.go is updated; see https://go-review.googlesource.com/c/net/+/471535"u8;

internal static void testDisableContentLength(ж<testing.T> Ꮡt, testMode mode) {
    if (mode == http2Mode) {
        Ꮡt.Skip(skippingUntilH2BundleGoˢ);
    }
    var noCL = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header()[contentLengthˢ] = default!; // disable the default Content-Length response
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "OK"u8);
    })));
    var (res, err) = (~noCL).c.Get((~(~noCL).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (got, haveCL) = (~res).Header[contentLengthˢ, ꟷ]; if (haveCL) {
            Ꮡt.Errorf("Unexpected Content-Length: %q"u8, got);
        }
    }
    {
        var errΔ1 = (~res).Body.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var withCL = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "OK"u8);
    })));
    (res, err) = (~withCL).c.Get((~(~withCL).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = (~res).Header.Get(contentLengthˢ); if (got != "2"u8) {
            Ꮡt.Errorf("Content-Length: %q; want 2"u8, got);
        }
    }
    {
        var errΔ2 = (~res).Body.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
}

public static void TestErrorContentLength(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testErrorContentLength(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string errorBodyᶜ = "an error occurred"u8;

internal static void testErrorContentLength(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string errorBody = errorBodyᶜ;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentLengthˢ, "1000"u8);
            Error(w, errorBody, 400);
        })));
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Get(%q) = %v"u8, (~(~cst).ts).URL, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var body, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("io.ReadAll(res.Body) = %v"u8, err);
        }
        if (((@string)body) != errorBody + "\n") {
            Ꮡt.Fatalf("read body: %q, want %q"u8, ((@string)body), errorBody);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xContentTypeOptionsˢ = "X-Content-Type-Options"u8;
internal static readonly @string scratchAndSniffˢ = "scratch and sniff"u8;
internal static readonly @string otherˢ2 = "Other"u8;
internal static readonly @string oopsˢ = "oops"u8;
internal static readonly object nosniffˢ = (@string)"nosniff"u8;

public static void TestError(ж<testing.T> Ꮡt) {
    var w = httptest.NewRecorder();
    w.Header().Set(contentLengthˢ, "1"u8);
    w.Header().Set(xContentTypeOptionsˢ, scratchAndSniffˢ);
    w.Header().Set(otherˢ2, fooˢ);
    Error(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), oopsˢ, 432);
    var h = w.Header();
    foreach (var (_, hdr) in new @string[]{"Content-Length"u8}.slice()) {
        {
            var (v, ok) = h[hdr, ꟷ]; if (ok) {
                Ꮡt.Errorf("%s: %q, want not present"u8, hdr, v);
            }
        }
    }
    {
        @string v = h.Get(contentTypeˢ); if (v != "text/plain; charset=utf-8"u8) {
            Ꮡt.Errorf("Content-Type: %q, want %q"u8, v, textPlainCharsetUtf8ˢ);
        }
    }
    {
        @string v = h.Get(xContentTypeOptionsˢ); if (v != "nosniff"u8) {
            Ꮡt.Errorf("X-Content-Type-Options: %q, want %q"u8, v, nosniffˢ);
        }
    }
}

public static void TestServerReadAfterWriteHeader100Continue(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerReadAfterWriteHeader100Continue(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue67555ˢ = (@string)"https://go.dev/issue/67555"u8;

internal static void testServerReadAfterWriteHeader100Continue(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Skip(httpsGoDevIssue67555ˢ);
        var body = slice<byte>("body"u8);
        var bodyʗ1 = body;

        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.WriteHeader(200);
            NewResponseController(w).Flush();
            io.ReadAll((~r).Body);
            w.Write(bodyʗ1);
        })), (ж<Δhttp.Transport> tr) => {
            tr.Value.ExpectContinueTimeout = (time.Duration)(86400000000000L); // forever
        });
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ)));
        (~req).Header.Set(expectˢ, continueˢ);
        var (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Get(%q) = %v"u8, (~(~cst).ts).URL, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var got, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("io.ReadAll(res.Body) = %v"u8, err);
        }
        if (!bytes.Equal(got, body)) {
            Ꮡt.Fatalf("response body = %q, want %q"u8, got, body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerReadAfterHandlerDone100Continue(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerReadAfterHandlerDone100Continue(Δp0, Δp1));
}

internal static void testServerReadAfterHandlerDone100Continue(ж<testing.T> Ꮡt, testMode mode) {
    Ꮡt.Skip(httpsGoDevIssue67555ˢ);
    var readyc = new channel<EmptyStruct>(0);
    var readycʗ1 = readyc;

    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var readycʗ2 = readycʗ1;
        goǃ(() => {
            ᐸꟷ(readycʗ2);
            io.ReadAll((~r).Body);
            ᐸꟷ(readycʗ2);
        });
    })), (ж<Δhttp.Transport> tr) => {
        tr.Value.ExpectContinueTimeout = (time.Duration)(86400000000000L); // forever
    });
    var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ)));
    (~req).Header.Set(expectˢ, continueˢ);
    var (res, err) = (~cst).c.Do(req);
    if (err != default!) {
        Ꮡt.Fatalf("Get(%q) = %v"u8, (~(~cst).ts).URL, err);
    }
    (~res).Body.Close();
    readyc.ᐸꟷ(new EmptyStruct()); // server starts reading from the request body
    readyc.ᐸꟷ(new EmptyStruct()); // server finishes reading from the request body
}

public static void TestServerReadAfterHandlerAbort100Continue(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerReadAfterHandlerAbort100Continue(Δp0, Δp1));
}

internal static void testServerReadAfterHandlerAbort100Continue(ж<testing.T> Ꮡt, testMode mode) {
    Ꮡt.Skip(httpsGoDevIssue67555ˢ);
    var readyc = new channel<EmptyStruct>(0);
    var readycʗ1 = readyc;

    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var readycʗ2 = readycʗ1;
        goǃ(() => {
            ᐸꟷ(readycʗ2);
            io.ReadAll((~r).Body);
            ᐸꟷ(readycʗ2);
        });
        throw panic(ErrAbortHandler);
    })), (ж<Δhttp.Transport> tr) => {
        tr.Value.ExpectContinueTimeout = (time.Duration)(86400000000000L); // forever
    });
    var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ)));
    (~req).Header.Set(expectˢ, continueˢ);
    var (res, err) = (~cst).c.Do(req);
    if (err == default!) {
        (~res).Body.Close();
    }
    readyc.ᐸꟷ(new EmptyStruct()); // server starts reading from the request body
    readyc.ᐸꟷ(new EmptyStruct()); // server finishes reading from the request body
}

// Issue #72100: Verify that we don't modify the caller's TLS.Config.NextProtos slice.
public static void TestServerTLSNextProtos(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testServerTLSNextProtos(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

internal static void testServerTLSNextProtos(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    http_internal_test_package.CondSkipHTTP2(new http_test_package.testing_TжTB(Ꮡt));
    var (cert, err) = tls.X509KeyPair(testcert.LocalhostCert, testcert.LocalhostKey);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var leafCert, err) = x509.ParseCertificate(cert.ΔCertificate[0]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var certpool = x509.NewCertPool();
    certpool.AddCert(leafCert);
    var protos = @new<Δhttp.Protocols>();
    var exprᴛ1 = mode;
    if (exprᴛ1 == https1Mode) {
        protos.SetHTTP1(true);
    }
    else if (exprᴛ1 == http2Mode) {
        protos.SetHTTP2(true);
    }

    var wantNextProtos = new @string[]{"http/1.1"u8, "h2"u8, "other"u8}.slice();
    var nextProtos = slices.Clone<slice<@string>, @string>(wantNextProtos);
    // We don't use httptest here because it overrides the tls.Config.
    var srv = Ꮡ(new Server(
        TLSConfig: Ꮡ(new tls.Config(
            Certificates: new tls.Certificate[]{cert}.slice(),
            NextProtos: nextProtos
        )),
        Handler: new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> req) => {
        })),
        Protocols: protos
    ));
    var tr = Ꮡ(new Transport(
        TLSClientConfig: Ꮡ(new tls.Config(
            RootCAs: certpool,
            NextProtos: nextProtos
        )),
        Protocols: protos
    ));
    var listener = newLocalListener(Ꮡt);
    var srvc = new channel<error>(1);
    var listenerʗ1 = listener;
    var srvʗ1 = srv;
    var srvcʗ1 = srvc;
    goǃ(() => {
        srvcʗ1.ᐸꟷ(srvʗ1.ServeTLS(listenerʗ1, ""u8, ""u8));
    });
    var srvʗ2 = srv;
    var srvcʗ2 = srvc;
    Ꮡt.Cleanup(() => {
        srvʗ2.Close();
        ᐸꟷ(srvcʗ2);
    });
    var client = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));
    (var resp, err) = client.Get("https://"u8 + listener.Addr().String());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~resp).Body.Close();
    if (!slices.Equal<slice<@string>, @string>(nextProtos, wantNextProtos)) {
        Ꮡt.Fatalf("after running test: original NextProtos slice = %v, want %v"u8, nextProtos, wantNextProtos);
    }
}

[GoType("dyn")] internal partial struct TestInvalidChunkedBodies_type {
    internal @string name;
    internal @string b;
}

public static void TestInvalidChunkedBodies(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestInvalidChunkedBodies_type[]{new(
        name: "bare LF in chunk size"u8,
        b: "1\na\r\n0\r\n\r\n"u8
    ), new(
        name: "bare LF at body end"u8,
        b: "1\r\na\r\n0\r\n\n"u8
    )
    }.slice()) {
        ref var test = ref heap(new TestInvalidChunkedBodies_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var reqc = new channel<error>(0);
            var reqcʗ1 = reqc;
            var ts = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                var (got, errΔ1) = io.ReadAll((~r).Body);
                if (errΔ1 == default!) {
                    tΔ1.Logf("read body: %q"u8, got);
                }
                reqcʗ1.ᐸꟷ(errΔ1);
            }))).Value.ts;
            var (serverURL, err) = url.Parse((~ts).URL);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (var conn, err) = net.Dial(tcpˢ, (~serverURL).Host);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            {
                var (_, errΔ1) = conn.Write(slice<byte>(
                    "POST / HTTP/1.1\r\n" + "Host: localhost\r\n" + "Transfer-Encoding: chunked\r\n" + "Connection: close\r\n" + "\r\n" + testʗ1.b)); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            conn._<ж<net.TCPConn>>().CloseWrite();
            {
                var errΔ2 = ᐸꟷ(reqc); if (errΔ2 == default!) {
                    tΔ1.Errorf("server handler: io.ReadAll(r.Body) succeeded, want error"u8);
                }
            }
        });
    }
}

} // end http_test_package
