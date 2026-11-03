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
internal static ref readonly @string getTestGoFooBarABHttp10ˢ => ref getTestGoFooBarABHttp10ˢᵇ;
private static readonly @string getTestGoFooBarABHttp10ˢᵇ = "GET /test.go?foo=bar&a=b HTTP/1.0\nHost: example.com\n\n"u8;
internal static ref readonly @string textPlainCharsetUtf8ˢ => ref textPlainCharsetUtf8ˢᵇ;
private static readonly @string textPlainCharsetUtf8ˢᵇ = "text/plain; charset=utf-8"u8;

// This test is a CGI host (testing host.go) that runs its own binary
// as a child process testing the other half of CGI (child.go).
public static void TestHostingOurselves(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));

    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"
    ));
    var expectedMap = new map<@string, @string>{
        ["test"] = "Hello CGI-in-CGI",
        ["param-a"] = "b",
        ["param-foo"] = "bar",
        ["env-GATEWAY_INTERFACE"] = "CGI/1.1",
        ["env-HTTP_HOST"] = "example.com",
        ["env-PATH_INFO"] = "",
        ["env-QUERY_STRING"] = "foo=bar&a=b",
        ["env-REMOTE_ADDR"] = "1.2.3.4",
        ["env-REMOTE_HOST"] = "1.2.3.4",
        ["env-REMOTE_PORT"] = "1234",
        ["env-REQUEST_METHOD"] = "GET",
        ["env-REQUEST_URI"] = "/test.go?foo=bar&a=b",
        ["env-SCRIPT_FILENAME"] = os.Args[0],
        ["env-SCRIPT_NAME"] = "/test.go",
        ["env-SERVER_NAME"] = "example.com",
        ["env-SERVER_PORT"] = "80",
        ["env-SERVER_SOFTWARE"] = "go"
    };
    var replay = runCgiTest(t, h, getTestGoFooBarABHttp10ˢ, expectedMap);

    {
        @string expected = textPlainCharsetUtf8ˢ;
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

internal partial struct customWriterRecorder {
    internal io.Writer w;
    /*embed*/ public ж<net.http.httptest_package.ResponseRecorder> ResponseRecorder;
}

internal static (nint n, error err) Write(this ref customWriterRecorder r, slice<byte> p) {
    return r.w.Write(p);
}

internal partial struct limitWriter {
    internal io.Writer w;
    internal nint n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pastWriteLimitˢ => ref pastWriteLimitˢᵇ;
private static readonly @string pastWriteLimitˢᵇ = "past write limit"u8;

internal static (nint n, error err) Write(this ref limitWriter w, slice<byte> p) {
    nint n = default;
    error err = default;

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
internal static ref readonly @string httpExampleComTestGoˢ => ref httpExampleComTestGoˢᵇ;
private static readonly @string httpExampleComTestGoˢᵇ = "http://example.com/test.go?write-forever=1"u8;

// If there's an error copying the child's output to the parent, test
// that we kill the child.
public static void TestKillChildAfterCopyError(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));

    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"
    ));
    var (req, _) = http.NewRequest(getˢ, httpExampleComTestGoˢ, default);
    var rec = httptest.NewRecorder();
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    const nint writeLen = /* 50 << 10 */ 51200;
    var rw = Ꮡ(new customWriterRecorder(new cgi_internal_test_package.limitWriterжWriter(Ꮡ(new limitWriter(new cgi_internal_test_package.bytes_BufferжWriter(Ꮡout), writeLen))), rec));

    h.ServeHTTP(new cgi_internal_test_package.customWriterRecorderжResponseWriter(rw), req);
    if (@out.Len() != writeLen || @out.Bytes()[0] != 'a') {
        t.Errorf("unexpected output: %q", @out.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getTestGoNoBody1Http10ˢ => ref getTestGoNoBody1Http10ˢᵇ;
private static readonly @string getTestGoNoBody1Http10ˢᵇ = "GET /test.go?no-body=1 HTTP/1.0\nHost: example.com\n\n"u8;

// Test that a child handler writing only headers works.
// golang.org/issue/7196
public static void TestChildOnlyHeaders(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));

    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"
    ));
    var expectedMap = new map<@string, @string>{
        ["_body"] = ""
    };
    var replay = runCgiTest(t, h, getTestGoNoBody1Http10ˢ, expectedMap);
    {
        @string expected = xTestValueˢ;
        @string got = replay.Header().Get(xTestHeaderˢ); if (got != expected) {
            t.Errorf("got a X-Test-Header of %q; expected %q", got, expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string postTestGoNilRequestBodyˢ => ref postTestGoNilRequestBodyˢᵇ;
private static readonly @string postTestGoNilRequestBodyˢᵇ = "POST /test.go?nil-request-body=1 HTTP/1.0\nHost: example.com\n\n"u8;
internal static ref readonly @string postTestGoNilRequestBodyˢ2 => ref postTestGoNilRequestBodyˢ2ᵇ;
private static readonly @string postTestGoNilRequestBodyˢ2ᵇ = "POST /test.go?nil-request-body=1 HTTP/1.0\nHost: example.com\nContent-Length: 0\n\n"u8;

// Test that a child handler does not receive a nil Request Body.
// golang.org/issue/39190
public static void TestNilRequestBody(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));

    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"
    ));
    var expectedMap = new map<@string, @string>{
        ["nil-request-body"] = "false"
    };
    _ = runCgiTest(t, h, postTestGoNilRequestBodyˢ, expectedMap);
    _ = runCgiTest(t, h, postTestGoNilRequestBodyˢ2, expectedMap);
}

internal partial struct TestChildContentType_type /*dyn*/ {
    internal @string name;
    internal @string body;
    internal @string wantCT;
}

public static void TestChildContentType(ж<testing.T> t) {
    testenv.MustHaveExec(new cgi_internal_test_package.testing_TжTB(t));

    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"
    ));
    slice<TestChildContentType_type> tests = slice<TestChildContentType_type>([
        new(
            name: "no body",
            wantCT: "text/plain; charset=utf-8"
        ),
        new(
            name: "html",
            body: "<html><head><title>test page</title></head><body>This is a body</body></html>",
            wantCT: "text/html; charset=utf-8"
        ),
        new(
            name: "text",
            body: strings.Repeat(gopherˢ, 86),
            wantCT: "text/plain; charset=utf-8"
        ),
        new(
            name: "jpg",
            body: ((@string)(new byte[]{0xff, 0xd8, 0xff})) + strings.Repeat("B", 1024),
            wantCT: "image/jpeg"
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestChildContentType_type(), out var Ꮡtt);
        tt = vᴛ1;

        var hʗ1 = h;
        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var expectedMap = new map<@string, @string>{["_body"] = ttʗ1.body};
            @string req = fmt.Sprintf("GET /test.go?exact-body=%s HTTP/1.0\nHost: example.com\n\n"u8, url.QueryEscape(ttʗ1.body));
            var replay = runCgiTest(tΔ1, hʗ1, req, expectedMap);
            {
                @string got = replay.Header().Get(contentTypeˢ2); if (got != ttʗ1.wantCT) {
                    tΔ1.Errorf("got a Content-Type of %q; expected it to start with %q", got, ttʗ1.wantCT);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string immediateDisconnectˢ => ref immediateDisconnectˢᵇ;
private static readonly @string immediateDisconnectˢᵇ = "/immediate-disconnect"u8;

// golang.org/issue/7198
public static void Test500WithNoHeaders(ж<testing.T> t) {
    want500Test(t, immediateDisconnectˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noContentTypeˢ => ref noContentTypeˢᵇ;
private static readonly @string noContentTypeˢᵇ = "/no-content-type"u8;

public static void Test500WithNoContentType(ж<testing.T> t) {
    want500Test(t, noContentTypeˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string emptyHeadersˢ => ref emptyHeadersˢᵇ;
private static readonly @string emptyHeadersˢᵇ = "/empty-headers"u8;

public static void Test500WithEmptyHeaders(ж<testing.T> t) {
    want500Test(t, emptyHeadersˢ);
}

internal static void want500Test(ж<testing.T> t, @string path) {
    var h = Ꮡ(new Handler(
        Path: os.Args[0],
        Root: "/test.go"
    ));
    var expectedMap = new map<@string, @string>{
        ["_body"] = ""
    };
    var replay = runCgiTest(t, h, "GET "u8 + path + " HTTP/1.0\nHost: example.com\n\n"u8, expectedMap);
    if (replay.Code != 500) {
        t.Errorf("Got code %d; want 500", replay.Code);
    }
}

} // end cgi_internal_test_package
