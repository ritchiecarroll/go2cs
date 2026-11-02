// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests for CGI (the child process perspective)
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using strings = strings_package;
using testing = testing_package;
using go.net;
using go.net.http;
using io = io_package;
using static go.net.http.cgi_package;

partial class cgi_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goclientˢ => ref goclientˢᵇ;
private static readonly @string goclientˢᵇ = "goclient"u8;
internal static ref readonly @string getˢ => ref getˢᵇ;
private static readonly @string getˢᵇ = "GET"u8;
internal static ref readonly @string textXmlˢ => ref textXmlˢᵇ;
private static readonly @string textXmlˢᵇ = "text/xml"u8;
internal static ref readonly @string elsewhereˢ => ref elsewhereˢᵇ;
private static readonly @string elsewhereˢᵇ = "elsewhere"u8;
internal static ref readonly @string fooBarˢ => ref fooBarˢᵇ;
private static readonly @string fooBarˢᵇ = "Foo-Bar"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;
internal static ref readonly @string httpExampleComPathABˢ => ref httpExampleComPathABˢᵇ;
private static readonly @string httpExampleComPathABˢᵇ = "http://example.com/path?a=b"u8;

public static void TestRequest(ж<testing.T> t) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"] = "HTTP/1.1",
        ["REQUEST_METHOD"] = "GET",
        ["HTTP_HOST"] = "example.com",
        ["HTTP_REFERER"] = "elsewhere",
        ["HTTP_USER_AGENT"] = "goclient",
        ["HTTP_FOO_BAR"] = "baz",
        ["REQUEST_URI"] = "/path?a=b",
        ["CONTENT_LENGTH"] = "123",
        ["CONTENT_TYPE"] = "text/xml",
        ["REMOTE_ADDR"] = "5.6.7.8",
        ["REMOTE_PORT"] = "54321"
    };
    var (req, err) = RequestFromMap(env);
    if (err != nil) {
        t.Fatalf("RequestFromMap: %v", err);
    }
    {
        @string g = req.UserAgent();
        @string e = goclientˢ; if (e != g) {
            t.Errorf("expected UserAgent %q; got %q", e, g);
        }
    }
    {
        @string g = req.Method;
        @string e = getˢ; if (e != g) {
            t.Errorf("expected Method %q; got %q", e, g);
        }
    }
    {
        @string g = req.Header.Get(contentTypeˢ2);
        @string e = textXmlˢ; if (e != g) {
            t.Errorf("expected Content-Type %q; got %q", e, g);
        }
    }
    {
        var (g, e) = (req.ContentLength, (int64)123); if (e != g) {
            t.Errorf("expected ContentLength %d; got %d", e, g);
        }
    }
    {
        @string g = req.Referer();
        @string e = elsewhereˢ; if (e != g) {
            t.Errorf("expected Referer %q; got %q", e, g);
        }
    }
    if (req.Header == nil) {
        t.Fatalf("unexpected nil Header");
    }
    {
        @string g = req.Header.Get(fooBarˢ);
        @string e = bazˢ; if (e != g) {
            t.Errorf("expected Foo-Bar %q; got %q", e, g);
        }
    }
    {
        @string g = req.URL.String();
        @string e = httpExampleComPathABˢ; if (e != g) {
            t.Errorf("expected URL %q; got %q", e, g);
        }
    }
    {
        @string g = req.FormValue("a");
        @string e = "b"; if (e != g) {
            t.Errorf("expected FormValue(a) %q; got %q", e, g);
        }
    }
    if (req.Trailer == nil) {
        t.Errorf("unexpected nil Trailer");
    }
    if (req.TLS != nil) {
        t.Errorf("expected nil TLS");
    }
    {
        @string e = "5.6.7.8:54321";
        @string g = req.RemoteAddr; if (e != g) {
            t.Errorf("RemoteAddr: got %q; want %q", g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleComPathABˢ => ref httpsExampleComPathABˢᵇ;
private static readonly @string httpsExampleComPathABˢᵇ = "https://example.com/path?a=b"u8;

public static void TestRequestWithTLS(ж<testing.T> t) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"] = "HTTP/1.1",
        ["REQUEST_METHOD"] = "GET",
        ["HTTP_HOST"] = "example.com",
        ["HTTP_REFERER"] = "elsewhere",
        ["REQUEST_URI"] = "/path?a=b",
        ["CONTENT_TYPE"] = "text/xml",
        ["HTTPS"] = "1",
        ["REMOTE_ADDR"] = "5.6.7.8"
    };
    var (req, err) = RequestFromMap(env);
    if (err != nil) {
        t.Fatalf("RequestFromMap: %v", err);
    }
    {
        @string g = req.URL.String();
        @string e = httpsExampleComPathABˢ; if (e != g) {
            t.Errorf("expected URL %q; got %q", e, g);
        }
    }
    if (req.TLS == nil) {
        t.Errorf("expected non-nil TLS");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pathABˢ => ref pathABˢᵇ;
private static readonly @string pathABˢᵇ = "/path?a=b"u8;

public static void TestRequestWithoutHost(ж<testing.T> t) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"] = "HTTP/1.1",
        ["HTTP_HOST"] = "",
        ["REQUEST_METHOD"] = "GET",
        ["REQUEST_URI"] = "/path?a=b",
        ["CONTENT_LENGTH"] = "123"
    };
    var (req, err) = RequestFromMap(env);
    if (err != nil) {
        t.Fatalf("RequestFromMap: %v", err);
    }
    if (req.URL == nil) {
        t.Fatalf("unexpected nil URL");
    }
    {
        @string g = req.URL.String();
        @string e = pathABˢ; if (e != g) {
            t.Errorf("URL = %q; want %q", g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpExampleComDirˢ => ref httpExampleComDirˢᵇ;
private static readonly @string httpExampleComDirˢᵇ = "http://example.com/dir/scriptname/p1/p2?a=1&b=2"u8;

public static void TestRequestWithoutRequestURI(ж<testing.T> t) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"] = "HTTP/1.1",
        ["HTTP_HOST"] = "example.com",
        ["REQUEST_METHOD"] = "GET",
        ["SCRIPT_NAME"] = "/dir/scriptname",
        ["PATH_INFO"] = "/p1/p2",
        ["QUERY_STRING"] = "a=1&b=2",
        ["CONTENT_LENGTH"] = "123"
    };
    var (req, err) = RequestFromMap(env);
    if (err != nil) {
        t.Fatalf("RequestFromMap: %v", err);
    }
    if (req.URL == nil) {
        t.Fatalf("unexpected nil URL");
    }
    {
        @string g = req.URL.String();
        @string e = httpExampleComDirˢ; if (e != g) {
            t.Errorf("URL = %q; want %q", g, e);
        }
    }
}

public static void TestRequestWithoutRemotePort(ж<testing.T> t) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"] = "HTTP/1.1",
        ["HTTP_HOST"] = "example.com",
        ["REQUEST_METHOD"] = "GET",
        ["REQUEST_URI"] = "/path?a=b",
        ["CONTENT_LENGTH"] = "123",
        ["REMOTE_ADDR"] = "5.6.7.8"
    };
    var (req, err) = RequestFromMap(env);
    if (err != nil) {
        t.Fatalf("RequestFromMap: %v", err);
    }
    {
        @string e = "5.6.7.8:0";
        @string g = req.RemoteAddr; if (e != g) {
            t.Errorf("RemoteAddr: got %q; want %q", g, e);
        }
    }
}

// CGI Specification RFC 3875 - section 4.1.16
// INCLUDED value for SERVER_PROTOCOL must be treated as an HTTP/1.0 request
public static void TestIncludedServerProtocol(ж<testing.T> t) {
    var env = new map<@string, @string>{
        ["REQUEST_METHOD"] = "GET",
        ["SERVER_PROTOCOL"] = "INCLUDED"
    };
    var (req, err) = RequestFromMap(env);
    if (req.Proto != "INCLUDED"u8) {
        t.Errorf("unexpected change to SERVER_PROTOCOL");
    }
    {
        nint major = req.ProtoMajor; if (major != 1) {
            t.Errorf("ProtoMajor: got %d, want %d", major, (nint)1);
        }
    }
    {
        nint minor = req.ProtoMinor; if (minor != 0) {
            t.Errorf("ProtoMinor: got %d, want %d", minor, (nint)0);
        }
    }
    if (err != nil) {
        t.Fatalf("expected INCLUDED to be treated as HTTP/1.0 request");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gopherˢ => ref gopherˢᵇ;
private static readonly @string gopherˢᵇ = "gopher"u8;

internal partial struct TestResponse_type /*dyn*/ {
    internal @string name;
    internal @string body;
    internal @string wantCT;
}

public static void TestResponse(ж<testing.T> t) {
    slice<TestResponse_type> tests = slice<TestResponse_type>([
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
        ref var tt = ref heap(new TestResponse_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var resp = new response(
                req: httptest.NewRequest(getˢ, "/", default),
                header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                bufw: bufio.NewWriter(new cgi_internal_test_package.bytes_BufferжWriter(Ꮡbuf))
            );
            var (n, err) = resp.Write(slice<byte>(ttʗ1.body));
            if (err != nil) {
                tΔ1.Errorf("Write: unexpected %v", err);
            }
            {
                nint want = len(ttʗ1.body); if (n != want) {
                    tΔ1.Errorf("reported short Write: got %v want %v", n, want);
                }
            }
            resp.writeCGIHeader(default);
            resp.Flush();
            {
                @string got = resp.Header().Get(contentTypeˢ2); if (got != ttʗ1.wantCT) {
                    tΔ1.Errorf("wrong content-type: got %q, want %q", got, ttʗ1.wantCT);
                }
            }
            if (!bytes.HasSuffix(buf.Bytes(), slice<byte>(ttʗ1.body))) {
                tΔ1.Errorf("body was not correctly written");
            }
        });
    }
}

} // end cgi_internal_test_package
