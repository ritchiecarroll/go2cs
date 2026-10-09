// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests a Go CGI program running under a Go CGI host process.
// Further, the two programs are the same binary, just checking
// their environment to figure out what mode to run in.
namespace go.net.http;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using url = go.net.url_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal;
using go.net;
using go.net.http;
using static go.net.http.cgi_package;

partial class cgi_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestGoFooBarABHttp10ˢ = "GET /test.go?foo=bar&a=b HTTP/1.0\nHost: example.com\n\n"u8;
internal static readonly @string textPlainCharsetUtf8ˢ = "text/plain; charset=utf-8"u8;

// This test is a CGI host (testing host.go) that runs its own binary
// as a child process testing the other half of CGI (child.go).
public static void TestHostingOurselves(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["test"u8] = "Hello CGI-in-CGI"u8,
        ["param-a"u8] = "b"u8,
        ["param-foo"u8] = "bar"u8,
        ["env-GATEWAY_INTERFACE"u8] = "CGI/1.1"u8,
        ["env-HTTP_HOST"u8] = "example.com"u8,
        ["env-PATH_INFO"u8] = ""u8,
        ["env-QUERY_STRING"u8] = "foo=bar&a=b"u8,
        ["env-REMOTE_ADDR"u8] = "1.2.3.4"u8,
        ["env-REMOTE_HOST"u8] = "1.2.3.4"u8,
        ["env-REMOTE_PORT"u8] = "1234"u8,
        ["env-REQUEST_METHOD"u8] = "GET"u8,
        ["env-REQUEST_URI"u8] = "/test.go?foo=bar&a=b"u8,
        ["env-SCRIPT_FILENAME"u8] = os.Args[0],
        ["env-SCRIPT_NAME"u8] = "/test.go"u8,
        ["env-SERVER_NAME"u8] = "example.com"u8,
        ["env-SERVER_PORT"u8] = "80"u8,
        ["env-SERVER_SOFTWARE"u8] = "go"u8
    };
    var replay = runCgiTest(Ꮡt, h, getTestGoFooBarABHttp10ˢ, expectedMap);
    {
        @string expected = textPlainCharsetUtf8ˢ;
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

internal partial struct customWriterRecorder {
    internal io.Writer w;
    public partial ref ж<net.http.httptest_package.ResponseRecorder> ResponseRecorder { get; }
}

internal static (nint n, error err) Write(this ref customWriterRecorder r, slice<byte> p) {
    return r.w.Write(p);
}

internal partial struct limitWriter {
    internal io.Writer w;
    internal nint n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pastWriteLimitˢ = "past write limit"u8;

internal static (nint n, error err) Write(this ref limitWriter w, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (len(p) > w.n) {
        p = p.slice(0, w.n);
    }
    if (len(p) > 0) {
        (n, err) = w.w.Write(p);
        w.n -= n;
    }
    if (w.n == 0) {
        err = errors.New(pastWriteLimitˢ);
    }
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComTestGoˢ = "http://example.com/test.go?write-forever=1"u8;

// If there's an error copying the child's output to the parent, test
// that we kill the child.
public static void TestKillChildAfterCopyError(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"u8
    ));
    var (req, _) = http.NewRequest(getˢ, httpExampleComTestGoˢ, default!);
    var rec = httptest.NewRecorder();
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    const nint writeLen = /* 50 << 10 */ 51200;
    var rw = Ꮡ(new customWriterRecorder(new cgi_internal_test_package.limitWriterжWriter(Ꮡ(new limitWriter(new cgi_internal_test_package.bytes_BufferжWriter(Ꮡout), writeLen))), rec));
    h.ServeHTTP(new cgi_internal_test_package.customWriterRecorderжResponseWriter(rw), req);
    if (@out.Len() != writeLen || @out.Bytes()[0] != (rune)'a') {
        Ꮡt.Errorf("unexpected output: %q"u8, @out.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getTestGoNoBody1Http10ˢ = "GET /test.go?no-body=1 HTTP/1.0\nHost: example.com\n\n"u8;

// Test that a child handler writing only headers works.
// golang.org/issue/7196
public static void TestChildOnlyHeaders(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["_body"u8] = ""u8
    };
    var replay = runCgiTest(Ꮡt, h, getTestGoNoBody1Http10ˢ, expectedMap);
    {
        @string expected = xTestValueˢ;
        @string got = replay.Header().Get(xTestHeaderˢ); if (got != expected) {
            Ꮡt.Errorf("got a X-Test-Header of %q; expected %q"u8, got, expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string postTestGoNilRequestBodyˢ = "POST /test.go?nil-request-body=1 HTTP/1.0\nHost: example.com\n\n"u8;
internal static readonly @string postTestGoNilRequestBodyˢ2 = "POST /test.go?nil-request-body=1 HTTP/1.0\nHost: example.com\nContent-Length: 0\n\n"u8;

// Test that a child handler does not receive a nil Request Body.
// golang.org/issue/39190
public static void TestNilRequestBody(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["nil-request-body"u8] = "false"u8
    };
    _ = runCgiTest(Ꮡt, h, postTestGoNilRequestBodyˢ, expectedMap);
    _ = runCgiTest(Ꮡt, h, postTestGoNilRequestBodyˢ2, expectedMap);
}

internal partial struct TestChildContentType_type /*dyn*/ {
    internal @string name;
    internal @string body;
    internal @string wantCT;
}

public static void TestChildContentType(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(Ꮡt));
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"u8
    ));
    slice<TestChildContentType_type> tests = new TestChildContentType_type[]{
        new(
            name: "no body"u8,
            wantCT: "text/plain; charset=utf-8"u8
        ),
        new(
            name: "html"u8,
            body: "<html><head><title>test page</title></head><body>This is a body</body></html>"u8,
            wantCT: "text/html; charset=utf-8"u8
        ),
        new(
            name: "text"u8,
            body: strings.Repeat(gopherˢ, 86),
            wantCT: "text/plain; charset=utf-8"u8
        ),
        new(
            name: "jpg"u8,
            body: ((@string)(new byte[]{0xff, 0xd8, 0xff})) + strings.Repeat("B"u8, 1024),
            wantCT: "image/jpeg"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestChildContentType_type(), out var Ꮡtt);
        tt = vᴛ1;

        var hʗ1 = h;
        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var expectedMap = new map<@string, @string>{["_body"u8] = ttʗ1.body};
            @string req = fmt.Sprintf("GET /test.go?exact-body=%s HTTP/1.0\nHost: example.com\n\n"u8, url.QueryEscape(ttʗ1.body));
            var replay = runCgiTest(tΔ1, hʗ1, req, expectedMap);
            {
                @string got = replay.Header().Get(contentTypeˢ2); if (got != ttʗ1.wantCT) {
                    tΔ1.Errorf("got a Content-Type of %q; expected it to start with %q"u8, got, ttʗ1.wantCT);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string immediateDisconnectˢ = "/immediate-disconnect"u8;

// golang.org/issue/7198
public static void Test500WithNoHeaders(ж<testing.T> Ꮡt) {
    want500Test(Ꮡt, immediateDisconnectˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noContentTypeˢ = "/no-content-type"u8;

public static void Test500WithNoContentType(ж<testing.T> Ꮡt) {
    want500Test(Ꮡt, noContentTypeˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyHeadersˢ = "/empty-headers"u8;

public static void Test500WithEmptyHeaders(ж<testing.T> Ꮡt) {
    want500Test(Ꮡt, emptyHeadersˢ);
}

internal static void want500Test(ж<testing.T> Ꮡt, @string path) {
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"u8
    ));
    var expectedMap = new map<@string, @string>{
        ["_body"u8] = ""u8
    };
    var replay = runCgiTest(Ꮡt, h, "GET "u8 + path + " HTTP/1.0\nHost: example.com\n\n"u8, expectedMap);
    if ((~replay).Code != 500) {
        Ꮡt.Errorf("Got code %d; want 500"u8, (~replay).Code);
    }
}

} // end cgi_internal_test_package
