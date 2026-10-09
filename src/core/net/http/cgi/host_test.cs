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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;
using go.@internal;
using go.net;
using go.net.http;
using go.path;
using static go.net.http.cgi_package;

partial class cgi_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string serverSoftwareˢ = "SERVER_SOFTWARE"u8;

// TestMain executes the test binary as the cgi server if
// SERVER_SOFTWARE is set, and runs the tests otherwise.
public static void TestMain(ж<testing.M> Ꮡm) {
    // SERVER_SOFTWARE swap variable is set when starting the cgi server.
    if (os.Getenv(serverSoftwareˢ) != ""u8) {
        cgiMain();
        os.Exit(0);
    }
    os.Exit(Ꮡm.Run());
}

internal static ж<http.Request> newRequest(@string httpreq) {
    var buf = bufio.NewReader(new cgi_internal_test_package.strings_ReaderжReader(strings.NewReader(httpreq)));
    var (req, err) = http.ReadRequest(buf);
    if (err != default!) {
        throw panic("cgi: bogus http request in test: " + httpreq);
    }
    req.Value.RemoteAddr = "1.2.3.4:1234"u8;
    return req;
}

internal static ж<httptest.ResponseRecorder> runCgiTest(ж<testing.T> Ꮡt, ж<global::go.net.http.cgi_package.Handler> Ꮡh, @string httpreq, map<@string, @string> expectedMap, params Span<Action<map<@string, @string>>> checksʗp) {
    var checks = checksʗp.sslice();

    var rw = httptest.NewRecorder();
    var req = newRequest(httpreq);
    Ꮡh.ServeHTTP(new cgi_internal_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    runResponseChecks(Ꮡt, rw, expectedMap, checks.ꓸꓸꓸ);
    return rw;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bodyˢ = "_body"u8;

internal static void runResponseChecks(ж<testing.T> Ꮡt, ж<httptest.ResponseRecorder> Ꮡrw, map<@string, @string> expectedMap, params Span<Action<map<@string, @string>>> checksʗp) {
    var checks = checksʗp.sslice();

    ref var rw = ref Ꮡrw.DerefOrNull();
    // Make a map to hold the test map that the CGI returns.
    var m = new map<@string, @string>();
    m[bodyˢ] = rw.Body.String();
    nint linesRead = 0;
readlines:
    while (ᐧ) {
        var (line, err) = rw.Body.ReadString((rune)'\n');
        switch (ᐧ) {
        case {} when AreEqual(err, io.EOF): {
            goto break_readlines;
            break;
        }
        case {} when err != default!: {
            Ꮡt.Fatalf("unexpected error reading from CGI: %v"u8, err);
            break;
        }}

        linesRead++;
        @string trimmedLine = strings.TrimRight(line, "\r\n"u8);
        var (k, v, ok) = strings.Cut(trimmedLine, "="u8);
        if (!ok) {
            Ꮡt.Fatalf("Unexpected response from invalid line number %v: %q; existing map=%v"u8,
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
            Ꮡt.Errorf("for key %q got %q; expected %q"u8, key, got, expected);
        }
    }
    foreach (var (_, check) in checks) {
        check(m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestCgiFooBarABHttp10ˢ = "GET /test.cgi?foo=bar&a=b HTTP/1.0\nHost: example.com:80\n\n"u8;
internal static readonly @string textHtmlˢ = "text/html"u8;

public static void TestCGIBasicGet(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["test"u8] = "Hello CGI"u8,
        ["param-a"u8] = "b"u8,
        ["param-foo"u8] = "bar"u8,
        ["env-GATEWAY_INTERFACE"u8] = "CGI/1.1"u8,
        ["env-HTTP_HOST"u8] = "example.com:80"u8,
        ["env-PATH_INFO"u8] = ""u8,
        ["env-QUERY_STRING"u8] = "foo=bar&a=b"u8,
        ["env-REMOTE_ADDR"u8] = "1.2.3.4"u8,
        ["env-REMOTE_HOST"u8] = "1.2.3.4"u8,
        ["env-REMOTE_PORT"u8] = "1234"u8,
        ["env-REQUEST_METHOD"u8] = "GET"u8,
        ["env-REQUEST_URI"u8] = "/test.cgi?foo=bar&a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-SCRIPT_NAME"u8] = "/test.cgi"u8,
        ["env-SERVER_NAME"u8] = "example.com"u8,
        ["env-SERVER_PORT"u8] = "80"u8,
        ["env-SERVER_SOFTWARE"u8] = "go"u8
    };
    var replay = runCgiTest(Ꮡt, h, getTestCgiFooBarABHttp10ˢ, expectedMap);
    {
        @string expected = textHtmlˢ;
        @string got = replay.Header().Get(contentTypeˢ2); if (got != expected) {
            Ꮡt.Errorf("got a Content-Type of %q; expected %q"u8, got, expected);
        }
    }
    {
        @string expected = xTestValueˢ;
        @string got = replay.Header().Get(xTestHeaderˢ); if (got != expected) {
            Ꮡt.Errorf("got a X-Test-Header of %q; expected %q"u8, got, expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestCgiFooBarABHttp10ˢ2 = "GET /test.cgi?foo=bar&a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestCGIEnvIPv6(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["test"u8] = "Hello CGI"u8,
        ["param-a"u8] = "b"u8,
        ["param-foo"u8] = "bar"u8,
        ["env-GATEWAY_INTERFACE"u8] = "CGI/1.1"u8,
        ["env-HTTP_HOST"u8] = "example.com"u8,
        ["env-PATH_INFO"u8] = ""u8,
        ["env-QUERY_STRING"u8] = "foo=bar&a=b"u8,
        ["env-REMOTE_ADDR"u8] = "2000::3000"u8,
        ["env-REMOTE_HOST"u8] = "2000::3000"u8,
        ["env-REMOTE_PORT"u8] = "12345"u8,
        ["env-REQUEST_METHOD"u8] = "GET"u8,
        ["env-REQUEST_URI"u8] = "/test.cgi?foo=bar&a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-SCRIPT_NAME"u8] = "/test.cgi"u8,
        ["env-SERVER_NAME"u8] = "example.com"u8,
        ["env-SERVER_PORT"u8] = "80"u8,
        ["env-SERVER_SOFTWARE"u8] = "go"u8
    };
    var rw = httptest.NewRecorder();
    var req = newRequest(getTestCgiFooBarABHttp10ˢ2);
    req.Value.RemoteAddr = "[2000::3000]:12345"u8;
    h.ServeHTTP(new cgi_internal_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    runResponseChecks(Ꮡt, rw, expectedMap);
}

public static void TestCGIBasicGetAbsPath(ж<testing.T> Ꮡt) {
    ref var absPath = ref heap<@string>(out var ᏑabsPath);
    (absPath, var err) = filepath.Abs(os.Args[0]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: absPath,
        Root: "/test.cgi"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["env-REQUEST_URI"u8] = "/test.cgi?foo=bar&a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = absPath,
        ["env-SCRIPT_NAME"u8] = "/test.cgi"u8
    };
    runCgiTest(Ꮡt, h, getTestCgiFooBarABHttp10ˢ2, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestCgiExtrapathABˢ = "GET /test.cgi/extrapath?a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestPathInfo(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["param-a"u8] = "b"u8,
        ["env-PATH_INFO"u8] = "/extrapath"u8,
        ["env-QUERY_STRING"u8] = "a=b"u8,
        ["env-REQUEST_URI"u8] = "/test.cgi/extrapath?a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-SCRIPT_NAME"u8] = "/test.cgi"u8
    };
    runCgiTest(Ꮡt, h, getTestCgiExtrapathABˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getMyscriptBarABHttp10ˢ = "GET /myscript/bar?a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestPathInfoDirRoot(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/myscript//"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["env-PATH_INFO"u8] = "/bar"u8,
        ["env-QUERY_STRING"u8] = "a=b"u8,
        ["env-REQUEST_URI"u8] = "/myscript/bar?a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-SCRIPT_NAME"u8] = "/myscript"u8
    };
    runCgiTest(Ꮡt, h, getMyscriptBarABHttp10ˢ, expectedMap);
}

public static void TestDupHeaders(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0]
    ));
    var expectedMap = new map<@string, @string>{
        ["env-REQUEST_URI"u8] = "/myscript/bar?a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-HTTP_COOKIE"u8] = "nom=NOM; yum=YUM"u8,
        ["env-HTTP_X_FOO"u8] = "val1, val2"u8
    };
    runCgiTest(Ꮡt, h, "GET /myscript/bar?a=b HTTP/1.0\n"u8 + "Cookie: nom=NOM\n"u8 + "Cookie: yum=YUM\n"u8 + "X-Foo: val1\n"u8 + "X-Foo: val2\n"u8 + "Host: example.com\n\n"u8,
        expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string envHttpProxyˢ = "env-HTTP_PROXY"u8;

// Issue 16405: CGI+http.Transport differing uses of HTTP_PROXY.
// Verify we don't set the HTTP_PROXY environment variable.
// Hope nobody was depending on it. It's not a known header, though.
public static void TestDropProxyHeader(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0]
    ));
    var expectedMap = new map<@string, @string>{
        ["env-REQUEST_URI"u8] = "/myscript/bar?a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-HTTP_X_FOO"u8] = "a"u8
    };
    runCgiTest(Ꮡt, h, "GET /myscript/bar?a=b HTTP/1.0\n"u8 + "X-Foo: a\n"u8 + "Proxy: should_be_stripped\n"u8 + "Host: example.com\n\n"u8,
        expectedMap,
        (map<@string, @string> reqInfo) => {
            {
                var (v, ok) = reqInfo[envHttpProxyˢ, ꟷ]; if (ok) {
                    Ꮡt.Errorf("HTTP_PROXY = %q; should be absent"u8, v);
                }
            }
        });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getBarABHttp10Hostˢ = "GET /bar?a=b HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestPathInfoNoRoot(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: ""u8
    ));
    var expectedMap = new map<@string, @string>{
        ["env-PATH_INFO"u8] = "/bar"u8,
        ["env-QUERY_STRING"u8] = "a=b"u8,
        ["env-REQUEST_URI"u8] = "/bar?a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-SCRIPT_NAME"u8] = ""u8
    };
    runCgiTest(Ꮡt, h, getBarABHttp10Hostˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string postTestCgiABHttp10Hostˢ = """
POST /test.cgi?a=b HTTP/1.0
Host: example.com
Content-Type: application/x-www-form-urlencoded
Content-Length: 15

postfoo=postbar
"""u8;

public static void TestCGIBasicPost(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    @string postReq = postTestCgiABHttp10Hostˢ;
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["test"u8] = "Hello CGI"u8,
        ["param-postfoo"u8] = "postbar"u8,
        ["env-REQUEST_METHOD"u8] = "POST"u8,
        ["env-CONTENT_LENGTH"u8] = "15"u8,
        ["env-REQUEST_URI"u8] = "/test.cgi?a=b"u8
    };
    runCgiTest(Ꮡt, h, postReq, expectedMap);
}

internal static @string chunk(@string s) {
    return fmt.Sprintf("%x\r\n%s\r\n"u8, len(s), s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string postfooˢ = "postfoo"u8;
internal static readonly @string postbarˢ = "postbar"u8;

// The CGI spec doesn't allow chunked requests.
public static void TestCGIPostChunked(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    @string postReq = """
POST /test.cgi?a=b HTTP/1.1
Host: example.com
Content-Type: application/x-www-form-urlencoded
Transfer-Encoding: chunked


"""u8 + chunk(postfooˢ) + chunk("="u8) + chunk(postbarˢ) + chunk(""u8);
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    var expectedMap = new map<@string, @string>{};
    var resp = runCgiTest(Ꮡt, h, postReq, expectedMap);
    {
        nint got = resp.Value.Code;
        nint expected = http.StatusBadRequest; if (got != expected) {
            Ꮡt.Fatalf("Expected %v response code from chunked request body; got %d"u8,
                expected, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestCgiLocHttpFooComˢ = "GET /test.cgi?loc=http://foo.com/ HTTP/1.0\nHost: example.com\n\n"u8;
internal static readonly @string httpFooComˢ = "http://foo.com/"u8;

public static void TestRedirect(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    var rec = runCgiTest(Ꮡt, h, getTestCgiLocHttpFooComˢ, default!);
    {
        nint e = 302;
        nint g = rec.Value.Code; if (e != g) {
            Ꮡt.Errorf("expected status code %d; got %d"u8, e, g);
        }
    }
    {
        @string e = httpFooComˢ;
        @string g = rec.Header().Get(locationˢ); if (e != g) {
            Ꮡt.Errorf("expected Location header of %q; got %q"u8, e, g);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestCgiLocFooHttp10ˢ = "GET /test.cgi?loc=/foo HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestInternalRedirect(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var baseHandler = new http.HandlerFunc((http.ResponseWriter rw, ж<http.Request> req) => {
        fmt.Fprintf(new cgi_internal_test_package.http_ResponseWriterᴠWriter(rw), "basepath=%s\n"u8, (~(~req).URL).Path);
        fmt.Fprintf(new cgi_internal_test_package.http_ResponseWriterᴠWriter(rw), "remoteaddr=%s\n"u8, (~req).RemoteAddr);
    });
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8,
        PathLocationHandler: new cgi_internal_test_package.http_HandlerFuncᴠΔHandler(baseHandler)
    ));
    var expectedMap = new map<@string, @string>{
        ["basepath"u8] = "/foo"u8,
        ["remoteaddr"u8] = "1.2.3.4:1234"u8
    };
    runCgiTest(Ꮡt, h, getTestCgiLocFooHttp10ˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string httpExampleComTestCgiˢ = "http://example.com/test.cgi?bigresponse=1"u8;

// TestCopyError tests that we kill the process if there's an error copying
// its output. (for example, from the client having gone away)
//
// If we fail to do so, the test will time out (and dump its goroutines) with a
// call to [Handler.ServeHTTP] blocked on a deferred call to [exec.Cmd.Wait].
public static void TestCopyError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
        var h = Ꮡ(new Handler(
            Path: os.Args[0],
            Root: "/test.cgi"u8
        ));
        var ts = httptest.NewServer(new cgi_internal_test_package.cgi_HandlerжΔHandler(h));
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (conn, err) = net.Dial(tcpˢ, (~ts).Listener.Addr().String());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var (req, _) = http.NewRequest(getˢ, httpExampleComTestCgiˢ, default!);
        err = req.Write(new cgi_internal_test_package.net_ConnᴠWriter(conn));
        if (err != default!) {
            Ꮡt.Fatalf("Write: %v"u8, err);
        }
        (var res, err) = http.ReadResponse(bufio.NewReader(new cgi_internal_test_package.net_ConnᴠReader(conn)), req);
        if (err != default!) {
            Ꮡt.Fatalf("ReadResponse: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        array<byte> buf = new(5000);
        (var n, err) = io.ReadFull(new cgi_internal_test_package.io_ReadCloserᴠReader((~res).Body), buf[..]);
        if (err != default!) {
            Ꮡt.Fatalf("ReadFull: %d bytes, %v"u8, n, err);
        }
        if (!handlerRunning()) {
            Ꮡt.Fatalf("pre-conn.Close, expected handler to still be running"u8);
        }
        conn.Close();
        var closed = time.Now();
        var nextSleep = 1 * time.Millisecond;
        while (ᐧ) {
            time.Sleep(nextSleep);
            nextSleep *= 2;
            if (!handlerRunning()) {
                break;
            }
            Ꮡt.Logf("handler still running %v after conn.Close"u8, time.Since(closed));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netHttpCgiHandlerˢ = @"net/http/cgi\.\(\*Handler\)\.ServeHTTP"u8;

// handlerRunning reports whether any goroutine is currently running
// [Handler.ServeHTTP].
internal static partial bool handlerRunning() {
    var r = regexp.MustCompile(netHttpCgiHandlerˢ);
    var buf = new slice<byte>((64 << (int)(10)));
    while (ᐧ) {
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
internal static readonly @string getTestCgiHttp10Hostˢ = "GET /test.cgi HTTP/1.0\nHost: example.com\n\n"u8;

public static void TestDir(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    ref var cwd = ref heap<@string>(out var Ꮡcwd);
    (cwd, _) = os.Getwd();
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8,
        Dir: cwd
    ));
    var expectedMap = new map<@string, @string>{
        ["cwd"u8] = cwd
    };
    runCgiTest(Ꮡt, h, getTestCgiHttp10Hostˢ, expectedMap);
    (cwd, _) = os.Getwd();
    (cwd, _) = filepath.Split(os.Args[0]);
    h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8
    ));
    expectedMap = new map<@string, @string>{
        ["cwd"u8] = cwd
    };
    runCgiTest(Ꮡt, h, getTestCgiHttp10Hostˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestCgiˢ = "testdata/test.cgi"u8;

public static void TestEnvOverride(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var (cgifile, _) = filepath.Abs(testdataTestCgiˢ);
    ref var cwd = ref heap<@string>(out var Ꮡcwd);
    (cwd, _) = os.Getwd();
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8,
        Dir: cwd,
        Env: new @string[]{
            "SCRIPT_FILENAME="u8 + cgifile,
            "REQUEST_URI=/foo/bar"u8,
            "PATH=/wibble"u8}.slice()
    ));
    var expectedMap = new map<@string, @string>{
        ["cwd"u8] = cwd,
        ["env-SCRIPT_FILENAME"u8] = cgifile,
        ["env-REQUEST_URI"u8] = "/foo/bar"u8,
        ["env-PATH"u8] = "/wibble"u8
    };
    runCgiTest(Ꮡt, h, getTestCgiHttp10Hostˢ, expectedMap);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestCgiWritestderr1ˢ = "GET /test.cgi?writestderr=1 HTTP/1.0\nHost: example.com\n\n"u8;
internal static readonly @string helloStderrˢ = "Hello, stderr!\n"u8;

public static void TestHandlerStderr(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    ref var stderr = ref heap(new strings.Builder(), out var Ꮡstderr);
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.cgi"u8,
        Stderr: new cgi_internal_test_package.strings_BuilderжWriter(Ꮡstderr)
    ));
    var rw = httptest.NewRecorder();
    var req = newRequest(getTestCgiWritestderr1ˢ);
    h.ServeHTTP(new cgi_internal_test_package.httptest_ResponseRecorderжResponseWriter(rw), req);
    {
        @string got = stderr.String();
        @string want = helloStderrˢ; if (got != want) {
            Ꮡt.Errorf("Stderr = %q; want %q"u8, got, want);
        }
    }
}

internal partial struct TestRemoveLeadingDuplicates_tests /*dyn*/ {
    internal slice<@string> env;
    internal slice<@string> want;
}

public static void TestRemoveLeadingDuplicates(ж<testing.T> Ꮡt) {
    var tests = new TestRemoveLeadingDuplicates_tests[]{
        new(
            env: new @string[]{"a=b"u8, "b=c"u8, "a=b2"u8}.slice(),
            want: new @string[]{"b=c"u8, "a=b2"u8}.slice()
        ),
        new(
            env: new @string[]{"a=b"u8, "b=c"u8, "d"u8, "e=f"u8}.slice(),
            want: new @string[]{"a=b"u8, "b=c"u8, "d"u8, "e=f"u8}.slice()
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        var got = removeLeadingDuplicates(tt.env);
        if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
            Ꮡt.Errorf("removeLeadingDuplicates(%q) = %q; want %q"u8, tt.env, got, tt.want);
        }
    }
}

} // end cgi_internal_test_package
