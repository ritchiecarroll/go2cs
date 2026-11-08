// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using url = global::go.net.url_package;
using strings = strings_package;
using testing = testing_package;
using iotest = global::go.testing.iotest_package;
using synctest = global::go.testing.synctest_package;
using time = time_package;
using global::go.net;
using global::go.testing;
using static global::go.net.http_package;

partial class http_internal_test_package {

internal partial struct reqWriteTest {
    public global::go.net.http_package.Request Req;
    public any Body; // optional []byte or func() io.ReadCloser to populate Req.Body
    // Any of these three may be empty to skip that test.
    public @string WantWrite; // Request.Write
    public @string WantProxy; // Request.WriteProxy
    public error WantError; // wanted error from Request.Write
}

internal static readonly ж<slice<reqWriteTest>> ᏑreqWriteTests = new StaticBox<slice<reqWriteTest>>(new slice<reqWriteTest>(27){
    // HTTP/1.1 => chunked coding; no body; no trailer
    [0] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.techcrunch.com",
                Path: "/"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Accept"] = ["text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"],
                ["Accept-Charset"] = ["ISO-8859-1,utf-8;q=0.7,*;q=0.7"],
                ["Accept-Encoding"] = ["gzip,deflate"],
                ["Accept-Language"] = ["en-us,en;q=0.5"],
                ["Keep-Alive"] = ["300"],
                ["Proxy-Connection"] = ["keep-alive"],
                ["User-Agent"] = ["Fake"]
            }),
            Body: default,
            Close: false,
            Host: "www.techcrunch.com",
            Form: new map<@string, slice<@string>>{}
        ),

        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: www.techcrunch.com\r\n"u8 + "User-Agent: Fake\r\n"u8 + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n"u8 + "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7\r\n"u8 + "Accept-Encoding: gzip,deflate\r\n"u8 + "Accept-Language: en-us,en;q=0.5\r\n"u8 + "Keep-Alive: 300\r\n"u8 + "Proxy-Connection: keep-alive\r\n\r\n"u8,

        WantProxy: "GET http://www.techcrunch.com/ HTTP/1.1\r\n"u8 + "Host: www.techcrunch.com\r\n"u8 + "User-Agent: Fake\r\n"u8 + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n"u8 + "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7\r\n"u8 + "Accept-Encoding: gzip,deflate\r\n"u8 + "Accept-Language: en-us,en;q=0.5\r\n"u8 + "Keep-Alive: 300\r\n"u8 + "Proxy-Connection: keep-alive\r\n\r\n"u8
    ),
    // HTTP/1.1 => chunked coding; body; empty trailer
    [1] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.google.com",
                Path: "/search"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            TransferEncoding: ["chunked"]
        ),

        Body: slice<byte>("abcdef"u8),

        WantWrite: "GET /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef") + chunk(""),

        WantProxy: "GET http://www.google.com/search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef") + chunk("")
    ),
    // HTTP/1.1 POST => chunked coding; body; empty trailer
    [2] = new(
        Req: new Request(
            Method: "POST",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.google.com",
                Path: "/search"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            TransferEncoding: ["chunked"]
        ),

        Body: slice<byte>("abcdef"u8),

        WantWrite: "POST /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Connection: close\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef") + chunk(""),

        WantProxy: "POST http://www.google.com/search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Connection: close\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef") + chunk("")
    ),

    // HTTP/1.1 POST with Content-Length, no chunking
    [3] = new(
        Req: new Request(
            Method: "POST",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.google.com",
                Path: "/search"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: 6
        ),

        Body: slice<byte>("abcdef"u8),

        WantWrite: "POST /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 6\r\n"u8 + "\r\n"u8 + "abcdef"u8,

        WantProxy: "POST http://www.google.com/search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 6\r\n"u8 + "\r\n"u8 + "abcdef"u8
    ),

    // HTTP/1.1 POST with Content-Length in headers
    [4] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("http://example.com/"),
            Host: "example.com",
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["10"] // ignored
            }),
            ContentLength: 6
        ),

        Body: slice<byte>("abcdef"u8),

        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 6\r\n"u8 + "\r\n"u8 + "abcdef"u8,

        WantProxy: "POST http://example.com/ HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 6\r\n"u8 + "\r\n"u8 + "abcdef"u8
    ),

    // default to HTTP/1.1
    [5] = new(
        Req: new Request(
            Method: "GET",
            URL: mustParseURL("/search"),
            Host: "www.google.com"
        ),

        WantWrite: "GET /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),

    // Request with a 0 ContentLength and a 0 byte body.
    [6] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0 // as if unset by user
        ),

        Body: io.ReadCloser () => io.NopCloser(io.LimitReader(new http_test_package.strings_ReaderжReader(strings.NewReader("xx")), 0)),

        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n0\r\n\r\n"u8,

        WantProxy: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n0\r\n\r\n"u8
    ),

    // Request with a 0 ContentLength and a nil body.
    [7] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0 // as if unset by user
        ),

        Body: io.ReadCloser () => default,

        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8,

        WantProxy: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8
    ),

    // Request with a 0 ContentLength and a 1 byte body.
    [8] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0 // as if unset by user
        ),

        Body: io.ReadCloser () => io.NopCloser(io.LimitReader(new http_test_package.strings_ReaderжReader(strings.NewReader("xx")), 1)),

        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("x") + chunk(""),

        WantProxy: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("x") + chunk("")
    ),

    // Request with a ContentLength of 10 but a 5 byte body.
    [9] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 10 // but we're going to send only 5 bytes
        ),
        Body: slice<byte>("12345"u8),
        WantError: errors.New("http: ContentLength=10 with Body length 5")
    ),

    // Request with a ContentLength of 4 but an 8 byte body.
    [10] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 4 // but we're going to try to send 8 bytes
        ),
        Body: slice<byte>("12345678"u8),
        WantError: errors.New("http: ContentLength=4 with Body length 8")
    ),

    // Request with a 5 ContentLength and nil body.
    [11] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 5 // but we'll omit the body
        ),
        WantError: errors.New("http: Request.ContentLength=5 with nil Body")
    ),

    // Request with a 0 ContentLength and a body with 1 byte content and an error.
    [12] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0 // as if unset by user
        ),

        Body: io.ReadCloser () => {
            var err = errors.New("Custom reader error");
            var errReader = iotest.ErrReader(err);
            return io.NopCloser(io.MultiReader(new http_test_package.strings_ReaderжReader(strings.NewReader("x")), errReader));
        },

        WantError: errors.New("Custom reader error")
    ),

    // Request with a 0 ContentLength and a body without content and an error.
    [13] = new(
        Req: new Request(
            Method: "POST",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0 // as if unset by user
        ),

        Body: io.ReadCloser () => {
            var err = errors.New("Custom reader error");
            var errReader = iotest.ErrReader(err);
            return io.NopCloser(errReader);
        },

        WantError: errors.New("Custom reader error")
    ),

    // Verify that DumpRequest preserves the HTTP version number, doesn't add a Host,
    // and doesn't add a User-Agent.
    [14] = new(
        Req: new Request(
            Method: "GET",
            URL: mustParseURL("/foo"),
            ProtoMajor: 1,
            ProtoMinor: 0,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["X-Foo"] = ["X-Bar"]
            })
        ),

        WantWrite: "GET /foo HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),

    // If no Request.Host and no Request.URL.Host, we send
    // an empty Host header, and don't use
    // Request.Header["Host"]. This is just testing that
    // we don't change Go 1.0 behavior.
    [15] = new(
        Req: new Request(
            Method: "GET",
            Host: "",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "",
                Path: "/search"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Host"] = ["bad.example.com"]
            })
        ),

        WantWrite: "GET /search HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n\r\n"u8
    ),

    // Opaque test #1 from golang.org/issue/4860
    [16] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.google.com",
                Opaque: "/%2F/%2F/"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{})
        ),

        WantWrite: "GET /%2F/%2F/ HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n\r\n"u8
    ),

    // Opaque test #2 from golang.org/issue/4860
    [17] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "x.google.com",
                Opaque: "//y.google.com/%2F/%2F/"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{})
        ),

        WantWrite: "GET http://y.google.com/%2F/%2F/ HTTP/1.1\r\n"u8 + "Host: x.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n\r\n"u8
    ),

    // Testing custom case in header keys. Issue 5022.
    [18] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.google.com",
                Path: "/"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["ALL-CAPS"] = ["x"]
            })
        ),

        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "ALL-CAPS: x\r\n"u8 + "\r\n"u8
    ),

    // Request with host header field; IPv6 address with zone identifier
    [19] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Host: "[fe80::1%en0]"
            ))
        ),

        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: [fe80::1]\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),

    // Request with optional host header field; IPv6 address with zone identifier
    [20] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Host: "www.example.com"
            )),
            Host: "[fe80::1%en0]:8080"
        ),

        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: [fe80::1]:8080\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),

    // CONNECT without Opaque
    [21] = new(
        Req: new Request(
            Method: "CONNECT",
            URL: Ꮡ(new url.URL(
                Scheme: "https", // of proxy.com
                Host: "proxy.com"
            ))
        ),
        // What we used to do, locking that behavior in:
        WantWrite: "CONNECT proxy.com HTTP/1.1\r\n"u8 + "Host: proxy.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),

    // CONNECT with Opaque
    [22] = new(
        Req: new Request(
            Method: "CONNECT",
            URL: Ꮡ(new url.URL(
                Scheme: "https", // of proxy.com
                Host: "proxy.com",
                Opaque: "backend:443"
            ))
        ),
        WantWrite: "CONNECT backend:443 HTTP/1.1\r\n"u8 + "Host: proxy.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),

    // Verify that a nil header value doesn't get written.
    [23] = new(
        Req: new Request(
            Method: "GET",
            URL: mustParseURL("/foo"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["X-Foo"] = ["X-Bar"],
                ["X-Idempotency-Key"] = default
            })
        ),

        WantWrite: "GET /foo HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),
    [24] = new(
        Req: new Request(
            Method: "GET",
            URL: mustParseURL("/foo"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["X-Foo"] = ["X-Bar"],
                ["X-Idempotency-Key"] = []
            })
        ),

        WantWrite: "GET /foo HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),

    [25] = new(
        Req: new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Host: "www.example.com",
                RawQuery: "new\nline" // or any CTL
            ))
        ),
        WantError: errors.New("net/http: can't write control character in Request.URL")
    ),

    [26] = new( // Request with nil body and PATCH method. Issue #40978
        Req: new Request(
            Method: "PATCH",
            URL: mustParseURL("/"),
            Host: "example.com",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0 // as if unset by user
        ),
        Body: default,
        WantWrite: "PATCH / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n\r\n"u8,
        WantProxy: "PATCH / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n\r\n"u8
    )
});
internal static ref slice<reqWriteTest> reqWriteTests => ref ((StaticBox<slice<reqWriteTest>>)ᏑreqWriteTests).ValueSlot;

public static void TestRequestWrite(ж<testing.T> t) {
    foreach (var i in range(builtin.len(reqWriteTests))) {
        var tt = Ꮡ(reqWriteTests, i);

        var ttʗ1 = tt;
        void setBody() {
            if (ttʗ1.Body == nil) {
                return;
            }
            switch (ttʗ1.Body.type()) {
            case slice<byte> b: {
                ttʗ1.Req.Body = io.NopCloser(new http_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
                break;
            }
            case Func<io.ReadCloser> b: {
                ttʗ1.Req.Body = b();
                break;
            }}
        }
        setBody();
        if (tt.Req.Header == nil) {
            tt.Req.Header = new global::go.net.http_package.ΔHeader(0);
        }

        ref var braw = ref heap(new strings.Builder(), out var Ꮡbraw);
        var err = tt.ᏑReq.Write(new http_test_package.strings_BuilderжWriter(Ꮡbraw));
        @string g = fmt.Sprintf("%v"u8, err);
        @string e = fmt.Sprintf("%v"u8, tt.WantError); if (g != e) {
            t.Errorf("writing #%d, err = %q, want %q", i, g, e);
            continue;
        }
        if (err != nil) {
            continue;
        }

        if (tt.WantWrite != ""u8) {
            @string sraw = braw.String();
            if (sraw != tt.WantWrite) {
                t.Errorf("Test %d, expecting:\n%s\nGot:\n%s\n", i, tt.WantWrite, sraw);
                continue;
            }
        }

        if (tt.WantProxy != ""u8) {
            setBody();
            ref var praw = ref heap(new strings.Builder(), out var Ꮡpraw);
            err = tt.ᏑReq.WriteProxy(new http_test_package.strings_BuilderжWriter(Ꮡpraw));
            if (err != nil) {
                t.Errorf("WriteProxy #%d: %s", i, err);
                continue;
            }
            @string sraw = praw.String();
            if (sraw != tt.WantProxy) {
                t.Errorf("Test Proxy %d, expecting:\n%s\nGot:\n%s\n", i, tt.WantProxy, sraw);
                continue;
            }
        }
    }
}

public static void TestRequestWriteTransport(ж<testing.T> t) {
    t.Parallel();

    // Run this test in a synctest bubble, since it relies on the transport
    // successfully probing the request body within 200ms
    // (see transferWriter.probeRequestBody).
    // This occasionally flakes on slow builders (#52575) if we don't use a fake clock.
    synctest.Test(t, testRequestWriteTransport);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string transferEncodingˢ2 => ref transferEncodingˢ2ᵇ;
private static readonly @string transferEncodingˢ2ᵇ = "Transfer-Encoding: "u8;
internal static ref readonly @string foobodyˢ => ref foobodyˢᵇ;
private static readonly @string foobodyˢᵇ = "foobody"u8;
internal static ref readonly @string contentLength7ˢ => ref contentLength7ˢᵇ;
private static readonly @string contentLength7ˢᵇ = "Content-Length: 7"u8;
internal static ref readonly @string transferEncodingChunkedˢ2 => ref transferEncodingChunkedˢ2ᵇ;
private static readonly @string transferEncodingChunkedˢ2ᵇ = "Transfer-Encoding: chunked"u8;
internal static ref readonly @string oobodyˢ => ref oobodyˢᵇ;
private static readonly @string oobodyˢᵇ = "oobody"u8;

internal partial struct testRequestWriteTransport_testCase /*dyn*/ {
    internal @string method;
    internal int64 clen; // ContentLength
    internal io.ReadCloser body;
    internal Func<@string, error> want;
    // optional:
    internal Action<ж<testRequestWriteTransport_testCase>> init;
    internal Action afterReqRead;
}

internal static void testRequestWriteTransport(ж<testing.T> t) {
    Func<@string, error> matchSubstr(@string substr) => error (@string written) => {
            if (!strings.Contains(written, substr)) {
                return fmt.Errorf("expected substring %q in request: %s"u8, substr, written);
            }
            return default;
        };

    var noContentLengthOrTransferEncoding = error (@string req) => {
        if (strings.Contains(req, contentLengthˢ3)) {
            return fmt.Errorf("unexpected Content-Length in request: %s"u8, req);
        }
        if (strings.Contains(req, transferEncodingˢ2)) {
            return fmt.Errorf("unexpected Transfer-Encoding in request: %s"u8, req);
        }
        return default;
    };

    Func<@string, error> all(params Span<Func<@string, error>> checksʗp) {
        var checks = checksʗp.slice();
        var checksʗ1 = checks;
        return error (@string req) => {
            foreach (var (_, c) in range(checksʗ1)) {
                var err = c(req); if (err != nil) {
                    return err;
                }
            }
            return default;
        };
    }

    var tests = slice<testRequestWriteTransport_testCase>([
        new(
            method: "GET",
            want: noContentLengthOrTransferEncoding
        ),
        new(
            method: "GET",
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
            want: noContentLengthOrTransferEncoding
        ),
        new(
            method: "GET",
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
            want: noContentLengthOrTransferEncoding
        ),
        // A GET with a body, with explicit content length:
        new(
            method: "GET",
            clen: 7,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(foobodyˢ))),
            want: all(matchSubstr(contentLength7ˢ),
                matchSubstr(foobodyˢ))
        ),
        // A GET with a body, sniffing the leading "f" from "foobody".
        new(
            method: "GET",
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(foobodyˢ))),
            want: all(matchSubstr(transferEncodingChunkedˢ2),
                matchSubstr("\r\n1\r\nf\r\n"),
                matchSubstr(oobodyˢ))
        ),
        // But a POST request is expected to have a body, so
        // no sniffing happens:
        new(
            method: "POST",
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(foobodyˢ))),
            want: all(matchSubstr(transferEncodingChunkedˢ2),
                matchSubstr(foobodyˢ))
        ),
        new(
            method: "POST",
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
            want: all(matchSubstr(transferEncodingChunkedˢ2))
        ),
        // Verify that a blocking Request.Body doesn't block forever.
        new(
            method: "GET",
            clen: -1,
            init: (ж<testRequestWriteTransport_testCase> tt) => {
                var (pr, pw) = io.Pipe();
                var pwʗ1 = pw;
                tt.afterReqRead = () => {
                    pwʗ1.Close();
                };
                tt.body = io.NopCloser(new io.PipeReaderжReader(pr));
            },
            want: matchSubstr(transferEncodingChunkedˢ2)
        )
    ]);

    foreach (var (i, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new testRequestWriteTransport_testCase(), out var Ꮡtt);
        tt = vᴛ1;

        if (tt.init != nil) {
            tt.init(Ꮡtt);
        }
        var req = Ꮡ(new Request(
            Method: tt.method,
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "example.com"
            )),
            Header: new global::go.net.http_package.ΔHeader(0),
            ContentLength: tt.clen,
            Body: tt.body
        ));
        var (got, err) = dumpRequestOut(req, tt.afterReqRead);
        if (err != nil) {
            t.Errorf("test[%d]: %v", i, err);
            continue;
        }
        var errΔ1 = tt.want((@string)got); if (errΔ1 != nil) {
            t.Errorf("test[%d]: %v", i, errΔ1);
        }
    }
}

internal partial struct closeChecker {
    /*embed*/ public io_package.Reader Reader;
    internal bool closed;
}

internal static error Close(this ref closeChecker rc) {
    rc.closed = true;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myBodyˢ => ref myBodyˢᵇ;
private static readonly @string myBodyˢᵇ = "my body"u8;
internal static ref readonly @string httpFooComˢ => ref httpFooComˢᵇ;
private static readonly @string httpFooComˢᵇ = "http://foo.com/"u8;
internal static readonly object bodyNotClosedAfterWriteˢ = (@string)"body not closed after write"u8;

// TestRequestWriteClosesBody tests that Request.Write closes its request.Body.
// It also indirectly tests NewRequest and that it doesn't wrap an existing Closer
// inside a NopCloser, and that it serializes it correctly.
public static void TestRequestWriteClosesBody(ж<testing.T> t) {
    var rc = Ꮡ(new closeChecker(Reader: new http_test_package.strings_ReaderжReader(strings.NewReader(myBodyˢ))));
    var (req, err) = NewRequest(postˢ, httpFooComˢ, new http_internal_test_package.closeCheckerжReader(rc));
    if (err != nil) {
        t.Fatal(err);
    }
    var buf = @new<strings.Builder>();
    var errΔ1 = req.Write(new http_test_package.strings_BuilderжWriter(buf)); if (errΔ1 != nil) {
        t.Error(errΔ1);
    }
    if (!rc.closed) {
        t.Error(bodyNotClosedAfterWriteˢ);
    }
    @string expected = "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk(myBodyˢ) + chunk(""u8);
    if (buf.String() != expected) {
        t.Errorf("write:\n got: %s\nwant: %s", buf.String(), expected);
    }
}

internal static @string chunk(@string s) {
    return fmt.Sprintf("%x\r\n%s\r\n"u8, builtin.len(s), s);
}

internal static ж<url.URL> mustParseURL(@string s) {
    var (u, err) = url.Parse(s);
    if (err != nil) {
        throw panic(fmt.Sprintf("Error parsing URL %q: %v"u8, s, err));
    }
    return u;
}

internal delegate (nint, error) writerFunc(slice<byte> _);

internal static partial (nint, error) Write(this writerFunc f, slice<byte> p) { return f(p); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fakeWriteFailureˢ => ref fakeWriteFailureˢᵇ;
private static readonly @string fakeWriteFailureˢᵇ = "fake write failure"u8;

internal partial struct TestRequestWriteError_w /*dyn*/ {
    /*embed*/ public io_package.ByteWriter ByteWriter; // to avoid being wrapped by a bufio.Writer
    /*embed*/ public io_package.Writer Writer;
}

// TestRequestWriteError tests the Write err != nil checks in (*Request).write.
public static void TestRequestWriteError(ж<testing.T> t) {
    nint failAfter = 0;
    nint writeCount = 0;
    var errFail = errors.New(fakeWriteFailureˢ);

    // w is the buffered io.Writer to write the request to. It
    // fails exactly once on its Nth Write call, as controlled by
    // failAfter. It also tracks the number of calls in
    // writeCount.
        var errFailʗ1 = errFail;
    var w = new TestRequestWriteError_w(
        default,
        new http_internal_test_package.writerFuncᴠWriter(new writerFunc((slice<byte> p) => {
            error err = default;
            writeCount++;
            if (failAfter == 0) {
                err = errFailʗ1;
            }
            failAfter--;
            return (builtin.len(p), err);
        }))
    );

    var (req, _) = NewRequest(getˢ, httpExampleComˢ, default);
    UntypedInt writeCalls = 4; // number of Write calls in current implementation
    var sawGood = false;
    for (nint n = 0; n <= (nint)(writeCalls + 2); n++) {
        failAfter = n;
        writeCount = 0;
        var err = req.Write(w);
        error wantErr = default;
        if (n < writeCalls) {
            wantErr = errFail;
        }
        if (!AreEqual(err, wantErr)) {
            t.Errorf("for fail-after %d Writes, err = %v; want %v", n, err, wantErr);
            continue;
        }
        if (err == nil) {
            sawGood = true;
            if (writeCount != writeCalls) {
                t.Fatalf("writeCalls constant is outdated in test");
            }
        }
        if (writeCount > writeCalls || writeCount > n + 1) {
            t.Errorf("for fail-after %d, saw unexpectedly high (%d) write calls", n, writeCount);
        }
    }
    if (!sawGood) {
        t.Fatalf("writeCalls constant is outdated in test");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string http11204NoContentˢ => ref http11204NoContentˢᵇ;
private static readonly @string http11204NoContentˢᵇ = "HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"u8;

// dumpRequestOut is a modified copy of net/http/httputil.DumpRequestOut.
// Unlike the original, this version doesn't mutate the req.Body and
// try to restore it. It always dumps the whole body.
// And it doesn't support https.
internal static partial (slice<byte>, error) dumpRequestOut(ж<global::go.net.http_package.Request> req, Action onReadHeaders) {
    GoFrame ᒐ = default;
    try {
        // Use the actual Transport code to record what we would send
        // on the wire, but not using TCP.  Use a Transport with a
        // custom dialer that returns a fake net.Conn that waits
        // for the full input (and recording it), and then responds
        // with a dummy response.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf); // records the output
        var (pr, pw) = io.Pipe();
        var prʗ1 = pr;
        defer(() => prʗ1.Close(), ref ᒐ);
        var pwʗ1 = pw;
        defer(() => pwʗ1.Close(), ref ᒐ);
        var dr = Ꮡ(new delegateReader(c: new channel<io.Reader>(0)));

            var drʗ1 = dr;
            var pwʗ2 = pw;
        var t = Ꮡ(new Transport(
            Dial: (@string netΔ1, @string addr) => (new http_internal_test_package.dumpConnжConn(Ꮡ(new dumpConn(io.MultiWriter(new http_test_package.bytes_BufferжWriter(Ꮡbuf), new io.PipeWriterжWriter(pwʗ2)), new http_internal_test_package.delegateReaderжReader(drʗ1)))), default)
        ));
        var tʗ1 = t;
        defer(tʗ1.CloseIdleConnections, ref ᒐ);

        // Wait for the request before replying with a dummy response:
        var drʗ2 = dr;
        var prʗ2 = pr;
        goǃ(() => {
            var (reqΔ1, errΔ1) = ReadRequest(bufio.NewReader(new io.PipeReaderжReader(prʗ2)));
            if (errΔ1 == nil) {
                if (onReadHeaders != nil) {
                    onReadHeaders();
                }
                // Ensure all the body is read; otherwise
                // we'll get a partial dump.
                io.Copy(io.Discard, reqΔ1.Body);
                reqΔ1.Body.Close();
            }
            drʗ2.c.ᐸꟷ(new http_test_package.strings_ReaderжReader(strings.NewReader(http11204NoContentˢ)));
        });

        var (_, err) = t.RoundTrip(req);
        if (err != nil) {
            return (default, err);
        }
        return (buf.Bytes(), default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// delegateReader is a reader that delegates to another reader,
// once it arrives on a channel.
internal partial struct delegateReader {
    internal channel<io.Reader> c;
    internal io.Reader r; // nil until received from c
}

internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) {
    if (r.r == nil) {
        r.r = ᐸꟷ(r.c);
    }
    return r.r.Read(p);
}

// dumpConn is a net.Conn that writes to Writer and reads from Reader.
internal partial struct dumpConn {
    /*embed*/ public io_package.Writer Writer;
    /*embed*/ public io_package.Reader Reader;
}

internal static error Close(this ref dumpConn c) { return default; }
internal static netꓸAddr LocalAddr(this ref dumpConn c) { return default; }
internal static netꓸAddr RemoteAddr(this ref dumpConn c) { return default; }
internal static error SetDeadline(this ref dumpConn c, time.Time t) { return default; }
internal static error SetReadDeadline(this ref dumpConn c, time.Time t) { return default; }
internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) { return default; }

} // end http_internal_test_package
