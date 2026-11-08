// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests for package cgi
namespace go.net.http;

using bufio = bufio_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using net = net_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using os = os_package;
using filepath = go.path.filepath_package;
using regexp = regexp_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using fs = go.io.fs_package;
using go.@internal;
using go.net;
using go.net.http;
using go.path;
using static go.net.http.cgi_package;

partial class cgi_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string serverSoftwareˢ => ref serverSoftwareˢᵇ;
private static readonly @string serverSoftwareˢᵇ = "SERVER_SOFTWARE"u8;

// TestMain executes the test binary as the cgi server if
// SERVER_SOFTWARE is set, and runs the tests otherwise.
public static void TestMain(ж<testing.M> m) {
    // SERVER_SOFTWARE swap variable is set when starting the cgi server.
    if (os.Getenv(serverSoftwareˢ) != ""u8) {
        cgiMain();
        os.Exit(0);
    }

    os.Exit(m.Run());
}

internal static ж<http.Request> newRequest(@string httpreq) {
    var buf = bufio.NewReader(new cgi_internal_test_package.strings_ReaderжReader(strings.NewReader(httpreq)));
    var (req, err) = http.ReadRequest(buf);
    if (err != nil) {
        throw panic("cgi: bogus http request in test: " + httpreq);
    }
    req.RemoteAddr = "1.2.3.4:1234";
    return req;
}

internal static ж<httptest.ResponseRecorder> runCgiTest(ж<testing.T> t, ж<global::go.net.http.cgi_package.Handler> h, @string httpreq, map<@string, @string> expectedMap, params Span<Action<map<@string, @string>>> checksʗp) {
    var checks = checksʗp.sslice();

    var rw = httptest.NewRecorder();
    var req = newRequest(httpreq);
    h.ServeHTTP(new cgi_internal_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    runResponseChecks(t, rw, expectedMap, checks.ꓸꓸꓸ);
    return rw;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bodyˢ => ref bodyˢᵇ;
private static readonly @string bodyˢᵇ = "_body"u8;

internal static void runResponseChecks(ж<testing.T> t, ж<httptest.ResponseRecorder> Ꮡrw, map<@string, @string> expectedMap, params Span<Action<map<@string, @string>>> checksʗp) {
    var checks = checksʗp.sslice();
    ref var rw = ref Ꮡrw.DerefOrNull();

    // Make a map to hold the test map that the CGI returns.
    var m = new map<@string, @string>();
    m[bodyˢ] = rw.Body.String();
    nint linesRead = 0;
readlines:
    for (;;) {
        var (line, err) = rw.Body.ReadString((byte)'\n');
        switch (ᐧ) {
        case {} when AreEqual(err, io.EOF): {
            goto break_readlines;
            break;
        }
        case {} when err != nil: {
            t.Fatalf("unexpected error reading from CGI: %v", err);
            break;
        }}
        linesRead++;
        @string trimmedLine = strings.TrimRight(line, "\r\n");
        var (k, v, ok) = strings.Cut(trimmedLine, "=");
        if (!ok) {
            t.Fatalf("Unexpected response from invalid line number %v: %q; existing map=%v",
                linesRead, line, m);
        }
        m[k] = v;
continue_readlines:;
    }
break_readlines:;

    foreach (var (key, expected) in expectedMap) {
        @string got = m[key];
        if (key == "cwd"u8) {
            // For Windows. golang.org/issue/4645.
            var (fi1, _) = os.Stat(got);
            var (fi2, _) = os.Stat(expected);
            if (os.SameFile(fi1, fi2)) {
                got = expected;
            }
        }
        if (got != expected) {
            t.Errorf("for key %q got %q; expected %q", key, got, expected);
        }
    }
    foreach (var (_, check) in range(checks)) {
        check(m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiFooBarABHttp10ˢ => ref getTestCgiFooBarABHttp10ˢᵇ;
private static readonly @string getTestCgiFooBarABHttp10ˢᵇ = "GET /test.cgi?foo=bar&a=b HTTP/1.0\nHost: example.com:80\n\n"u8;
internal static ref readonly @string textHtmlˢ => ref textHtmlˢᵇ;
private static readonly @string textHtmlˢᵇ = "text/html"u8;

public static void TestCGIBasicGet(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    var expectedMap = new map<@string, @string>{
        ["test"] = "Hello CGI",
        ["param-a"] = "b",
        ["param-foo"] = "bar",
        ["env-GATEWAY_INTERFACE"] = "CGI/1.1",
        ["env-HTTP_HOST"] = "example.com:80",
        ["env-PATH_INFO"] = "",
        ["env-QUERY_STRING"] = "foo=bar&a=b",
        ["env-REMOTE_ADDR"] = "1.2.3.4",
        ["env-REMOTE_HOST"] = "1.2.3.4",
        ["env-REMOTE_PORT"] = "1234",
        ["env-REQUEST_METHOD"] = "GET",
        ["env-REQUEST_URI"] = "/test.cgi?foo=bar&a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-SCRIPT_NAME"] = "/test.cgi",
        ["env-SERVER_NAME"] = "example.com",
        ["env-SERVER_PORT"] = "80",
        ["env-SERVER_SOFTWARE"] = "go"
    };
    var replay = runCgiTest(t, h, getTestCgiFooBarABHttp10ˢ, expectedMap);

    {
        @string expected = textHtmlˢ;
        @string got = replay.Header().Get(contentTypeˢ2); if (got != expected) {
            t.Errorf("got a Content-Type of %q; expected %q", got, expected);
        }
    }
    {
        @string expected = xTestValueˢ;
        @string got = replay.Header().Get(xTestHeaderˢ); if (got != expected) {
            t.Errorf("got a X-Test-Header of %q; expected %q", got, expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiFooBarABHttp10ˢ2 => ref getTestCgiFooBarABHttp10ˢ2ᵇ;
private static readonly @string getTestCgiFooBarABHttp10ˢ2ᵇ = "GET /test.cgi?foo=bar&a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestCGIEnvIPv6(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    var expectedMap = new map<@string, @string>{
        ["test"] = "Hello CGI",
        ["param-a"] = "b",
        ["param-foo"] = "bar",
        ["env-GATEWAY_INTERFACE"] = "CGI/1.1",
        ["env-HTTP_HOST"] = "example.com",
        ["env-PATH_INFO"] = "",
        ["env-QUERY_STRING"] = "foo=bar&a=b",
        ["env-REMOTE_ADDR"] = "2000::3000",
        ["env-REMOTE_HOST"] = "2000::3000",
        ["env-REMOTE_PORT"] = "12345",
        ["env-REQUEST_METHOD"] = "GET",
        ["env-REQUEST_URI"] = "/test.cgi?foo=bar&a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-SCRIPT_NAME"] = "/test.cgi",
        ["env-SERVER_NAME"] = "example.com",
        ["env-SERVER_PORT"] = "80",
        ["env-SERVER_SOFTWARE"] = "go"
    };

    var rw = httptest.NewRecorder();
    var req = newRequest(getTestCgiFooBarABHttp10ˢ2);
    req.RemoteAddr = "[2000::3000]:12345";
    h.ServeHTTP(new cgi_internal_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    runResponseChecks(t, rw, expectedMap);
}

public static void TestCGIBasicGetAbsPath(ж<testing.T> t) {
    ref var absPath = ref heap<@string>(out var ᏑabsPath);
    (absPath, var err) = filepath.Abs(os.Args[0]);
    if (err != nil) {
        t.Fatal(err);
    }
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: absPath,
        Root: "/test.cgi"
    ));
    var expectedMap = new map<@string, @string>{
        ["env-REQUEST_URI"] = "/test.cgi?foo=bar&a=b",
        ["env-SCRIPT_FILENAME"] = absPath,
        ["env-SCRIPT_NAME"] = "/test.cgi"
    };
    runCgiTest(t, h, getTestCgiFooBarABHttp10ˢ2, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiExtrapathABˢ => ref getTestCgiExtrapathABˢᵇ;
private static readonly @string getTestCgiExtrapathABˢᵇ = "GET /test.cgi/extrapath?a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestPathInfo(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    var expectedMap = new map<@string, @string>{
        ["param-a"] = "b",
        ["env-PATH_INFO"] = "/extrapath",
        ["env-QUERY_STRING"] = "a=b",
        ["env-REQUEST_URI"] = "/test.cgi/extrapath?a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-SCRIPT_NAME"] = "/test.cgi"
    };
    runCgiTest(t, h, getTestCgiExtrapathABˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getMyscriptBarABHttp10ˢ => ref getMyscriptBarABHttp10ˢᵇ;
private static readonly @string getMyscriptBarABHttp10ˢᵇ = "GET /myscript/bar?a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestPathInfoDirRoot(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/myscript//"
    ));
    var expectedMap = new map<@string, @string>{
        ["env-PATH_INFO"] = "/bar",
        ["env-QUERY_STRING"] = "a=b",
        ["env-REQUEST_URI"] = "/myscript/bar?a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-SCRIPT_NAME"] = "/myscript"
    };
    runCgiTest(t, h, getMyscriptBarABHttp10ˢ, expectedMap);
}

public static void TestDupHeaders(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0]
    ));
    var expectedMap = new map<@string, @string>{
        ["env-REQUEST_URI"] = "/myscript/bar?a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-HTTP_COOKIE"] = "nom=NOM; yum=YUM",
        ["env-HTTP_X_FOO"] = "val1, val2"
    };
    runCgiTest(t, h, "GET /myscript/bar?a=b HTTP/1.0\n"u8 + "Cookie: nom=NOM\n"u8 + "Cookie: yum=YUM\n"u8 + "X-Foo: val1\n"u8 + "X-Foo: val2\n"u8 + "Host: example.com\n\n"u8,
        expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string envHttpProxyˢ => ref envHttpProxyˢᵇ;
private static readonly @string envHttpProxyˢᵇ = "env-HTTP_PROXY"u8;

// Issue 16405: CGI+http.Transport differing uses of HTTP_PROXY.
// Verify we don't set the HTTP_PROXY environment variable.
// Hope nobody was depending on it. It's not a known header, though.
public static void TestDropProxyHeader(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0]
    ));
    var expectedMap = new map<@string, @string>{
        ["env-REQUEST_URI"] = "/myscript/bar?a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-HTTP_X_FOO"] = "a"
    };
    runCgiTest(t, h, "GET /myscript/bar?a=b HTTP/1.0\n"u8 + "X-Foo: a\n"u8 + "Proxy: should_be_stripped\n"u8 + "Host: example.com\n\n"u8,
        expectedMap,
        (map<@string, @string> reqInfo) => {
            var (v, ok) = reqInfo[envHttpProxyˢ, ꟷ]; if (ok) {
                t.Errorf("HTTP_PROXY = %q; should be absent", v);
            }
        });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getBarABHttp10Hostˢ => ref getBarABHttp10Hostˢᵇ;
private static readonly @string getBarABHttp10Hostˢᵇ = "GET /bar?a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestPathInfoNoRoot(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: ""
    ));
    var expectedMap = new map<@string, @string>{
        ["env-PATH_INFO"] = "/bar",
        ["env-QUERY_STRING"] = "a=b",
        ["env-REQUEST_URI"] = "/bar?a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-SCRIPT_NAME"] = ""
    };
    runCgiTest(t, h, getBarABHttp10Hostˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string postTestCgiABHttp10Hostˢ => ref postTestCgiABHttp10Hostˢᵇ;
private static readonly @string postTestCgiABHttp10Hostˢᵇ = """
POST /test.cgi?a=b HTTP/1.0
Host: example.com
Content-Type: application/x-www-form-urlencoded
Content-Length: 15

postfoo=postbar
"""u8.Raw();

public static void TestCGIBasicPost(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    @string postReq = postTestCgiABHttp10Hostˢ;
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    var expectedMap = new map<@string, @string>{
        ["test"] = "Hello CGI",
        ["param-postfoo"] = "postbar",
        ["env-REQUEST_METHOD"] = "POST",
        ["env-CONTENT_LENGTH"] = "15",
        ["env-REQUEST_URI"] = "/test.cgi?a=b"
    };
    runCgiTest(t, h, postReq, expectedMap);
}

internal static @string chunk(@string s) {
    return fmt.Sprintf("%x\r\n%s\r\n"u8, len(s), s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string postfooˢ => ref postfooˢᵇ;
private static readonly @string postfooˢᵇ = "postfoo"u8;
internal static ref readonly @string postbarˢ => ref postbarˢᵇ;
private static readonly @string postbarˢᵇ = "postbar"u8;

// The CGI spec doesn't allow chunked requests.
public static void TestCGIPostChunked(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    @string postReq = """
POST /test.cgi?a=b HTTP/1.1
Host: example.com
Content-Type: application/x-www-form-urlencoded
Transfer-Encoding: chunked


"""u8.Raw() + chunk(postfooˢ) + chunk("=") + chunk(postbarˢ) + chunk(""u8);

    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    var expectedMap = new map<@string, @string>{};
    var resp = runCgiTest(t, h, postReq, expectedMap);
    nint got = resp.Code;
    nint expected = http.StatusBadRequest; if (got != expected) {
        t.Fatalf("Expected %v response code from chunked request body; got %d",
            expected, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiLocHttpFooComˢ => ref getTestCgiLocHttpFooComˢᵇ;
private static readonly @string getTestCgiLocHttpFooComˢᵇ = "GET /test.cgi?loc=http://foo.com/ HTTP/1.0\nHost: example.com\n\n"u8;
internal static ref readonly @string httpFooComˢ => ref httpFooComˢᵇ;
private static readonly @string httpFooComˢᵇ = "http://foo.com/"u8;

public static void TestRedirect(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    var rec = runCgiTest(t, h, getTestCgiLocHttpFooComˢ, default);
    {
        nint e = 302;
        nint g = rec.Code; if (e != g) {
            t.Errorf("expected status code %d; got %d", e, g);
        }
    }
    {
        @string e = httpFooComˢ;
        @string g = rec.Header().Get(locationˢ); if (e != g) {
            t.Errorf("expected Location header of %q; got %q", e, g);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiLocFooHttp10ˢ => ref getTestCgiLocFooHttp10ˢᵇ;
private static readonly @string getTestCgiLocFooHttp10ˢᵇ = "GET /test.cgi?loc=/foo HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestInternalRedirect(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var baseHandler = new http.HandlerFunc((http.ResponseWriter rw, ж<http.Request> req) => {
        fmt.Fprintf(new cgi_internal_test_package.http_ResponseWriterᴠWriter(rw), "basepath=%s\n"u8, req.URL.Path);
        fmt.Fprintf(new cgi_internal_test_package.http_ResponseWriterᴠWriter(rw), "remoteaddr=%s\n"u8, req.RemoteAddr);
    });
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi",
        PathLocationHandler: new cgi_internal_test_package.http_HandlerFuncᴠΔHandler(baseHandler)
    ));
    var expectedMap = new map<@string, @string>{
        ["basepath"] = "/foo",
        ["remoteaddr"] = "1.2.3.4:1234"
    };
    runCgiTest(t, h, getTestCgiLocFooHttp10ˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tcpˢ => ref tcpˢᵇ;
private static readonly @string tcpˢᵇ = "tcp"u8;
internal static ref readonly @string httpExampleComTestCgiˢ => ref httpExampleComTestCgiˢᵇ;
private static readonly @string httpExampleComTestCgiˢᵇ = "http://example.com/test.cgi?bigresponse=1"u8;

// TestCopyError tests that we kill the process if there's an error copying
// its output. (for example, from the client having gone away)
//
// If we fail to do so, the test will time out (and dump its goroutines) with a
// call to [Handler.ServeHTTP] blocked on a deferred call to [exec.Cmd.Wait].
public static void TestCopyError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));

        var h = Ꮡ(new Handler(
            Path: os.Args[0],
            Root: "/test.cgi"
        ));
        var ts = httptest.NewServer(new cgi_internal_test_package.cgi_HandlerжΔHandler(h));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var (conn, err) = net.Dial(tcpˢ, ts.Listener.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        var (req, _) = http.NewRequest(getˢ, httpExampleComTestCgiˢ, default);
        err = req.Write(new cgi_internal_test_package.net_ConnᴠWriter(conn));
        if (err != nil) {
            t.Fatalf("Write: %v", err);
        }
        (var res, err) = http.ReadResponse(bufio.NewReader(new cgi_internal_test_package.net_ConnᴠReader(conn)), req);
        if (err != nil) {
            t.Fatalf("ReadResponse: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        array<byte> buf = new(5000);
        (var n, err) = io.ReadFull(new cgi_internal_test_package.io_ReadCloserᴠReader(res.Body), buf[..]);
        if (err != nil) {
            t.Fatalf("ReadFull: %d bytes, %v", n, err);
        }

        if (!handlerRunning()) {
            t.Fatalf("pre-conn.Close, expected handler to still be running");
        }
        conn.Close();
        var closed = time.Now();

        var nextSleep = 1 * time.Millisecond;
        for (;;) {
            time.Sleep(nextSleep);
            nextSleep *= 2;
            if (!handlerRunning()) {
                break;
            }
            t.Logf("handler still running %v after conn.Close", time.Since(closed));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string netHttpCgiHandlerˢ => ref netHttpCgiHandlerˢᵇ;
private static readonly @string netHttpCgiHandlerˢᵇ = @"net/http/cgi\.\(\*Handler\)\.ServeHTTP"u8;

// handlerRunning reports whether any goroutine is currently running
// [Handler.ServeHTTP].
internal static partial bool handlerRunning() {
    var r = regexp.MustCompile(netHttpCgiHandlerˢ);
    var buf = new slice<byte>((64 << 10));
    for (;;) {
        nint n = runtime.Stack(buf, true);
        if (n < len(buf)) {
            return r.Match(buf.slice(0, n));
        }
        // Buffer wasn't large enough for a full goroutine dump.
        // Resize it and try again.
        buf = new slice<byte>(2 * len(buf));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiHttp10Hostˢ => ref getTestCgiHttp10Hostˢᵇ;
private static readonly @string getTestCgiHttp10Hostˢᵇ = "GET /test.cgi HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestDir(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    ref var cwd = ref heap<@string>(out var Ꮡcwd);
    (cwd, _) = os.Getwd();
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi",
        Dir: cwd
    ));
    var expectedMap = new map<@string, @string>{
        ["cwd"] = cwd
    };
    runCgiTest(t, h, getTestCgiHttp10Hostˢ, expectedMap);

    (cwd, _) = os.Getwd();
    (cwd, _) = filepath.Split(os.Args[0]);
    h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"
    ));
    expectedMap = new map<@string, @string>{
        ["cwd"] = cwd
    };
    runCgiTest(t, h, getTestCgiHttp10Hostˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTestCgiˢ => ref testdataTestCgiˢᵇ;
private static readonly @string testdataTestCgiˢᵇ = "testdata/test.cgi"u8;

public static void TestEnvOverride(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    var (cgifile, _) = filepath.Abs(testdataTestCgiˢ);

    ref var cwd = ref heap<@string>(out var Ꮡcwd);
    (cwd, _) = os.Getwd();
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi",
        Dir: cwd,
        Env: [
            "SCRIPT_FILENAME="u8 + cgifile,
            "REQUEST_URI=/foo/bar",
            "PATH=/wibble"]
    ));
    var expectedMap = new map<@string, @string>{
        ["cwd"] = cwd,
        ["env-SCRIPT_FILENAME"] = cgifile,
        ["env-REQUEST_URI"] = "/foo/bar",
        ["env-PATH"] = "/wibble"
    };
    runCgiTest(t, h, getTestCgiHttp10Hostˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestCgiWritestderr1ˢ => ref getTestCgiWritestderr1ˢᵇ;
private static readonly @string getTestCgiWritestderr1ˢᵇ = "GET /test.cgi?writestderr=1 HTTP/1.0\nHost: example.com\n\n"u8;
internal static ref readonly @string helloStderrˢ => ref helloStderrˢᵇ;
private static readonly @string helloStderrˢᵇ = "Hello, stderr!\n"u8;

public static void TestHandlerStderr(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));
    ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi",
        Stderr: new cgi_internal_test_package.strings_BuilderжWriter(Ꮡstderr)
    ));

    var rw = httptest.NewRecorder();
    var req = newRequest(getTestCgiWritestderr1ˢ);
    h.ServeHTTP(new cgi_internal_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    @string got = stderr.String();
    @string want = helloStderrˢ; if (got != want) {
        t.Errorf("Stderr = %q; want %q", got, want);
    }
}

internal partial struct TestRemoveLeadingDuplicates_tests /*dyn*/ {
    internal slice<@string> env;
    internal slice<@string> want;
}

public static void TestRemoveLeadingDuplicates(ж<testing.T> t) {
    var tests = slice<TestRemoveLeadingDuplicates_tests>([
        new(
            env: ["a=b", "b=c", "a=b2"],
            want: ["b=c", "a=b2"]
        ),
        new(
            env: ["a=b", "b=c", "d", "e=f"],
            want: ["a=b", "b=c", "d", "e=f"]
        )
    ]);
    foreach (var (_, tt) in range(tests)) {
        var got = removeLeadingDuplicates(tt.env);
        if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
            t.Errorf("removeLeadingDuplicates(%q) = %q; want %q", tt.env, got, tt.want);
        }
    }
}

} // end cgi_internal_test_package
