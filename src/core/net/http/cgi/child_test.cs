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
internal static readonly @string goclientˢ = "goclient"u8;
internal static readonly @string getˢ = "GET"u8;
internal static readonly @string textXmlˢ = "text/xml"u8;
internal static readonly @string elsewhereˢ = "elsewhere"u8;
internal static readonly @string fooBarˢ = "Foo-Bar"u8;
internal static readonly @string bazˢ = "baz"u8;
internal static readonly @string httpExampleComPathABˢ = "http://example.com/path?a=b"u8;

public static void TestRequest(ж<testing.T> Ꮡt) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"u8] = "HTTP/1.1"u8,
        ["REQUEST_METHOD"u8] = "GET"u8,
        ["HTTP_HOST"u8] = "example.com"u8,
        ["HTTP_REFERER"u8] = "elsewhere"u8,
        ["HTTP_USER_AGENT"u8] = "goclient"u8,
        ["HTTP_FOO_BAR"u8] = "baz"u8,
        ["REQUEST_URI"u8] = "/path?a=b"u8,
        ["CONTENT_LENGTH"u8] = "123"u8,
        ["CONTENT_TYPE"u8] = "text/xml"u8,
        ["REMOTE_ADDR"u8] = "5.6.7.8"u8,
        ["REMOTE_PORT"u8] = "54321"u8
    };
    var (req, err) = RequestFromMap(env);
    if (err != default!) {
        Ꮡt.Fatalf("RequestFromMap: %v"u8, err);
    }
    {
        @string g = req.UserAgent();
        @string e = goclientˢ; if (e != g) {
            Ꮡt.Errorf("expected UserAgent %q; got %q"u8, e, g);
        }
    }
    {
        @string g = req.Value.Method;
        @string e = getˢ; if (e != g) {
            Ꮡt.Errorf("expected Method %q; got %q"u8, e, g);
        }
    }
    {
        @string g = (~req).Header.Get(contentTypeˢ2);
        @string e = textXmlˢ; if (e != g) {
            Ꮡt.Errorf("expected Content-Type %q; got %q"u8, e, g);
        }
    }
    {
        var (g, e) = (req.Value.ContentLength, (int64)123); if (e != g) {
            Ꮡt.Errorf("expected ContentLength %d; got %d"u8, e, g);
        }
    }
    {
        @string g = req.Referer();
        @string e = elsewhereˢ; if (e != g) {
            Ꮡt.Errorf("expected Referer %q; got %q"u8, e, g);
        }
    }
    if ((~req).Header == default!) {
        Ꮡt.Fatalf("unexpected nil Header"u8);
    }
    {
        @string g = (~req).Header.Get(fooBarˢ);
        @string e = bazˢ; if (e != g) {
            Ꮡt.Errorf("expected Foo-Bar %q; got %q"u8, e, g);
        }
    }
    {
        @string g = (~req).URL.String();
        @string e = httpExampleComPathABˢ; if (e != g) {
            Ꮡt.Errorf("expected URL %q; got %q"u8, e, g);
        }
    }
    {
        @string g = req.FormValue("a"u8);
        @string e = "b"u8; if (e != g) {
            Ꮡt.Errorf("expected FormValue(a) %q; got %q"u8, e, g);
        }
    }
    if ((~req).Trailer == default!) {
        Ꮡt.Errorf("unexpected nil Trailer"u8);
    }
    if ((~req).TLS != nil) {
        Ꮡt.Errorf("expected nil TLS"u8);
    }
    {
        @string e = "5.6.7.8:54321"u8;
        @string g = req.Value.RemoteAddr; if (e != g) {
            Ꮡt.Errorf("RemoteAddr: got %q; want %q"u8, g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleComPathABˢ = "https://example.com/path?a=b"u8;

public static void TestRequestWithTLS(ж<testing.T> Ꮡt) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"u8] = "HTTP/1.1"u8,
        ["REQUEST_METHOD"u8] = "GET"u8,
        ["HTTP_HOST"u8] = "example.com"u8,
        ["HTTP_REFERER"u8] = "elsewhere"u8,
        ["REQUEST_URI"u8] = "/path?a=b"u8,
        ["CONTENT_TYPE"u8] = "text/xml"u8,
        ["HTTPS"u8] = "1"u8,
        ["REMOTE_ADDR"u8] = "5.6.7.8"u8
    };
    var (req, err) = RequestFromMap(env);
    if (err != default!) {
        Ꮡt.Fatalf("RequestFromMap: %v"u8, err);
    }
    {
        @string g = (~req).URL.String();
        @string e = httpsExampleComPathABˢ; if (e != g) {
            Ꮡt.Errorf("expected URL %q; got %q"u8, e, g);
        }
    }
    if ((~req).TLS == nil) {
        Ꮡt.Errorf("expected non-nil TLS"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathABˢ = "/path?a=b"u8;

public static void TestRequestWithoutHost(ж<testing.T> Ꮡt) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"u8] = "HTTP/1.1"u8,
        ["HTTP_HOST"u8] = ""u8,
        ["REQUEST_METHOD"u8] = "GET"u8,
        ["REQUEST_URI"u8] = "/path?a=b"u8,
        ["CONTENT_LENGTH"u8] = "123"u8
    };
    var (req, err) = RequestFromMap(env);
    if (err != default!) {
        Ꮡt.Fatalf("RequestFromMap: %v"u8, err);
    }
    if ((~req).URL == nil) {
        Ꮡt.Fatalf("unexpected nil URL"u8);
    }
    {
        @string g = (~req).URL.String();
        @string e = pathABˢ; if (e != g) {
            Ꮡt.Errorf("URL = %q; want %q"u8, g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComDirˢ = "http://example.com/dir/scriptname/p1/p2?a=1&b=2"u8;

public static void TestRequestWithoutRequestURI(ж<testing.T> Ꮡt) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"u8] = "HTTP/1.1"u8,
        ["HTTP_HOST"u8] = "example.com"u8,
        ["REQUEST_METHOD"u8] = "GET"u8,
        ["SCRIPT_NAME"u8] = "/dir/scriptname"u8,
        ["PATH_INFO"u8] = "/p1/p2"u8,
        ["QUERY_STRING"u8] = "a=1&b=2"u8,
        ["CONTENT_LENGTH"u8] = "123"u8
    };
    var (req, err) = RequestFromMap(env);
    if (err != default!) {
        Ꮡt.Fatalf("RequestFromMap: %v"u8, err);
    }
    if ((~req).URL == nil) {
        Ꮡt.Fatalf("unexpected nil URL"u8);
    }
    {
        @string g = (~req).URL.String();
        @string e = httpExampleComDirˢ; if (e != g) {
            Ꮡt.Errorf("URL = %q; want %q"u8, g, e);
        }
    }
}

public static void TestRequestWithoutRemotePort(ж<testing.T> Ꮡt) {
    var env = new map<@string, @string>{
        ["SERVER_PROTOCOL"u8] = "HTTP/1.1"u8,
        ["HTTP_HOST"u8] = "example.com"u8,
        ["REQUEST_METHOD"u8] = "GET"u8,
        ["REQUEST_URI"u8] = "/path?a=b"u8,
        ["CONTENT_LENGTH"u8] = "123"u8,
        ["REMOTE_ADDR"u8] = "5.6.7.8"u8
    };
    var (req, err) = RequestFromMap(env);
    if (err != default!) {
        Ꮡt.Fatalf("RequestFromMap: %v"u8, err);
    }
    {
        @string e = "5.6.7.8:0"u8;
        @string g = req.Value.RemoteAddr; if (e != g) {
            Ꮡt.Errorf("RemoteAddr: got %q; want %q"u8, g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gopherˢ = "gopher"u8;

internal partial struct TestResponse_type /*dyn*/ {
    internal @string name;
    internal @string body;
    internal @string wantCT;
}

public static void TestResponse(ж<testing.T> Ꮡt) {
    slice<TestResponse_type> tests = new TestResponse_type[]{
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
        ref var tt = ref heap(new TestResponse_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var resp = new response(
                req: httptest.NewRequest(getˢ, "/"u8, default!),
                header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                bufw: bufio.NewWriter(new cgi_internal_test_package.bytes_BufferжWriter(Ꮡbuf))
            );
            var (n, err) = resp.Write(slice<byte>(ttʗ1.body));
            if (err != default!) {
                tΔ1.Errorf("Write: unexpected %v"u8, err);
            }
            {
                nint want = len(ttʗ1.body); if (n != want) {
                    tΔ1.Errorf("reported short Write: got %v want %v"u8, n, want);
                }
            }
            resp.writeCGIHeader(default!);
            resp.Flush();
            {
                @string got = resp.Header().Get(contentTypeˢ2); if (got != ttʗ1.wantCT) {
                    tΔ1.Errorf("wrong content-type: got %q, want %q"u8, got, ttʗ1.wantCT);
                }
            }
            if (!bytes.HasSuffix(buf.Bytes(), slice<byte>(ttʗ1.body))) {
                tΔ1.Errorf("body was not correctly written"u8);
            }
        });
    }
}

} // end cgi_internal_test_package
