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
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
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

// HTTP/1.1 => chunked coding; no body; no trailer
// HTTP/1.1 => chunked coding; body; empty trailer
// HTTP/1.1 POST => chunked coding; body; empty trailer
// HTTP/1.1 POST with Content-Length, no chunking
// HTTP/1.1 POST with Content-Length in headers
// ignored
// default to HTTP/1.1
// Request with a 0 ContentLength and a 0 byte body.
// as if unset by user
// Request with a 0 ContentLength and a nil body.
// as if unset by user
// Request with a 0 ContentLength and a 1 byte body.
// as if unset by user
// Request with a ContentLength of 10 but a 5 byte body.
// but we're going to send only 5 bytes
// Request with a ContentLength of 4 but an 8 byte body.
// but we're going to try to send 8 bytes
// Request with a 5 ContentLength and nil body.
// but we'll omit the body
// Request with a 0 ContentLength and a body with 1 byte content and an error.
// as if unset by user
// Request with a 0 ContentLength and a body without content and an error.
// as if unset by user
// Verify that DumpRequest preserves the HTTP version number, doesn't add a Host,
// and doesn't add a User-Agent.
// If no Request.Host and no Request.URL.Host, we send
// an empty Host header, and don't use
// Request.Header["Host"]. This is just testing that
// we don't change Go 1.0 behavior.
// Opaque test #1 from golang.org/issue/4860
// Opaque test #2 from golang.org/issue/4860
// Testing custom case in header keys. Issue 5022.
// Request with host header field; IPv6 address with zone identifier
// Request with optional host header field; IPv6 address with zone identifier
// CONNECT without Opaque
// of proxy.com
// What we used to do, locking that behavior in:
// CONNECT with Opaque
// of proxy.com
// Verify that a nil header value doesn't get written.
// or any CTL
// Request with nil body and PATCH method. Issue #40978
// as if unset by user
internal static ж<slice<reqWriteTest>> ᏑreqWriteTests = new StandardBox<slice<reqWriteTest>>(new slice<reqWriteTest>(27){
    [0] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "www.techcrunch.com"u8,
                Path: "/"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Accept"u8] = new @string[]{"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"u8}.slice(),
                ["Accept-Charset"u8] = new @string[]{"ISO-8859-1,utf-8;q=0.7,*;q=0.7"u8}.slice(),
                ["Accept-Encoding"u8] = new @string[]{"gzip,deflate"u8}.slice(),
                ["Accept-Language"u8] = new @string[]{"en-us,en;q=0.5"u8}.slice(),
                ["Keep-Alive"u8] = new @string[]{"300"u8}.slice(),
                ["Proxy-Connection"u8] = new @string[]{"keep-alive"u8}.slice(),
                ["User-Agent"u8] = new @string[]{"Fake"u8}.slice()
            }),
            Body: default!,
            Close: false,
            Host: "www.techcrunch.com"u8,
            Form: new map<@string, slice<@string>>{}
        ),
        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: www.techcrunch.com\r\n"u8 + "User-Agent: Fake\r\n"u8 + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n"u8 + "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7\r\n"u8 + "Accept-Encoding: gzip,deflate\r\n"u8 + "Accept-Language: en-us,en;q=0.5\r\n"u8 + "Keep-Alive: 300\r\n"u8 + "Proxy-Connection: keep-alive\r\n\r\n"u8,
        WantProxy: "GET http://www.techcrunch.com/ HTTP/1.1\r\n"u8 + "Host: www.techcrunch.com\r\n"u8 + "User-Agent: Fake\r\n"u8 + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n"u8 + "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7\r\n"u8 + "Accept-Encoding: gzip,deflate\r\n"u8 + "Accept-Language: en-us,en;q=0.5\r\n"u8 + "Keep-Alive: 300\r\n"u8 + "Proxy-Connection: keep-alive\r\n\r\n"u8
    ),
    [1] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "www.google.com"u8,
                Path: "/search"u8
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            TransferEncoding: new @string[]{"chunked"u8}.slice()
        ),
        Body: slice<byte>("abcdef"u8),
        WantWrite: "GET /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef"u8) + chunk(""u8),
        WantProxy: "GET http://www.google.com/search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef"u8) + chunk(""u8)
    ),
    [2] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "www.google.com"u8,
                Path: "/search"u8
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            TransferEncoding: new @string[]{"chunked"u8}.slice()
        ),
        Body: slice<byte>("abcdef"u8),
        WantWrite: "POST /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Connection: close\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef"u8) + chunk(""u8),
        WantProxy: "POST http://www.google.com/search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Connection: close\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef"u8) + chunk(""u8)
    ),
    [3] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "www.google.com"u8,
                Path: "/search"u8
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
    [4] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("http://example.com/"u8),
            Host: "example.com"u8,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"u8] = new @string[]{"10"u8}.slice()
            }),
            ContentLength: 6
        ),
        Body: slice<byte>("abcdef"u8),
        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 6\r\n"u8 + "\r\n"u8 + "abcdef"u8,
        WantProxy: "POST http://example.com/ HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 6\r\n"u8 + "\r\n"u8 + "abcdef"u8
    ),
    [5] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: mustParseURL("/search"u8),
            Host: "www.google.com"u8
        ),
        WantWrite: "GET /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),
    [6] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0
        ),
        Body: io.ReadCloser () => io.NopCloser(io.LimitReader(new http_test_package.strings_ReaderжReader(strings.NewReader("xx"u8)), 0)),
        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n0\r\n\r\n"u8,
        WantProxy: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n0\r\n\r\n"u8
    ),
    [7] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0
        ),
        Body: io.ReadCloser () => default!,
        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8,
        WantProxy: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8
    ),
    [8] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0
        ),
        Body: io.ReadCloser () => io.NopCloser(io.LimitReader(new http_test_package.strings_ReaderжReader(strings.NewReader("xx"u8)), 1)),
        WantWrite: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("x"u8) + chunk(""u8),
        WantProxy: "POST / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("x"u8) + chunk(""u8)
    ),
    [9] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 10
        ),
        Body: slice<byte>("12345"u8),
        WantError: errors.New("http: ContentLength=10 with Body length 5"u8)
    ),
    [10] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 4
        ),
        Body: slice<byte>("12345678"u8),
        WantError: errors.New("http: ContentLength=4 with Body length 8"u8)
    ),
    [11] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 5
        ),
        WantError: errors.New("http: Request.ContentLength=5 with nil Body"u8)
    ),
    [12] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0
        ),
        Body: io.ReadCloser () => {
            var err = errors.New("Custom reader error"u8);
            var errReader = iotest.ErrReader(err);
            return io.NopCloser(io.MultiReader(new http_test_package.strings_ReaderжReader(strings.NewReader("x"u8)), errReader));
        },
        WantError: errors.New("Custom reader error"u8)
    ),
    [13] = new(
        Req: new Request(
            Method: "POST"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0
        ),
        Body: io.ReadCloser () => {
            var err = errors.New("Custom reader error"u8);
            var errReader = iotest.ErrReader(err);
            return io.NopCloser(errReader);
        },
        WantError: errors.New("Custom reader error"u8)
    ),
    [14] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: mustParseURL("/foo"u8),
            ProtoMajor: 1,
            ProtoMinor: 0,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["X-Foo"u8] = new @string[]{"X-Bar"u8}.slice()
            })
        ),
        WantWrite: "GET /foo HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),
    [15] = new(
        Req: new Request(
            Method: "GET"u8,
            Host: ""u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: ""u8,
                Path: "/search"u8
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Host"u8] = new @string[]{"bad.example.com"u8}.slice()
            })
        ),
        WantWrite: "GET /search HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n\r\n"u8
    ),
    [16] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "www.google.com"u8,
                Opaque: "/%2F/%2F/"u8
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{})
        ),
        WantWrite: "GET /%2F/%2F/ HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n\r\n"u8
    ),
    [17] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "x.google.com"u8,
                Opaque: "//y.google.com/%2F/%2F/"u8
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{})
        ),
        WantWrite: "GET http://y.google.com/%2F/%2F/ HTTP/1.1\r\n"u8 + "Host: x.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n\r\n"u8
    ),
    [18] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "www.google.com"u8,
                Path: "/"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["ALL-CAPS"u8] = new @string[]{"x"u8}.slice()
            })
        ),
        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "ALL-CAPS: x\r\n"u8 + "\r\n"u8
    ),
    [19] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Host: "[fe80::1%en0]"u8
            ))
        ),
        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: [fe80::1]\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),
    [20] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Host: "www.example.com"u8
            )),
            Host: "[fe80::1%en0]:8080"u8
        ),
        WantWrite: "GET / HTTP/1.1\r\n"u8 + "Host: [fe80::1]:8080\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),
    [21] = new(
        Req: new Request(
            Method: "CONNECT"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "https"u8,
                Host: "proxy.com"u8
            ))
        ),
        WantWrite: "CONNECT proxy.com HTTP/1.1\r\n"u8 + "Host: proxy.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),
    [22] = new(
        Req: new Request(
            Method: "CONNECT"u8,
            URL: Ꮡ(new url.URL(
                Scheme: "https"u8,
                Host: "proxy.com"u8,
                Opaque: "backend:443"u8
            ))
        ),
        WantWrite: "CONNECT backend:443 HTTP/1.1\r\n"u8 + "Host: proxy.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "\r\n"u8
    ),
    [23] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: mustParseURL("/foo"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["X-Foo"u8] = new @string[]{"X-Bar"u8}.slice(),
                ["X-Idempotency-Key"u8] = default!
            })
        ),
        WantWrite: "GET /foo HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),
    [24] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: mustParseURL("/foo"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["X-Foo"u8] = new @string[]{"X-Bar"u8}.slice(),
                ["X-Idempotency-Key"u8] = new @string[]{}.slice()
            })
        ),
        WantWrite: "GET /foo HTTP/1.1\r\n"u8 + "Host: \r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),
    [25] = new(
        Req: new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Host: "www.example.com"u8,
                RawQuery: "new\nline"u8
            ))
        ),
        WantError: errors.New("net/http: can't write control character in Request.URL"u8)
    ),
    [26] = new(
        Req: new Request(
            Method: "PATCH"u8,
            URL: mustParseURL("/"u8),
            Host: "example.com"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0
        ),
        Body: default!,
        WantWrite: "PATCH / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n\r\n"u8,
        WantProxy: "PATCH / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n\r\n"u8
    )
});
internal static ref slice<reqWriteTest> reqWriteTests => ref ᏑreqWriteTests.ValueSlot;

public static void TestRequestWrite(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in reqWriteTests) {
        var tt = Ꮡ(reqWriteTests, i);
        var ttʗ1 = tt;
        void setBody() {
            if ((~ttʗ1).Body == default!) {
                return;
            }
            switch ((~ttʗ1).Body.type()) {
            case slice<byte> b: {
                ttʗ1.Value.Req.Body = io.NopCloser(new http_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
                break;
            }
            case Func<io.ReadCloser> b: {
                ttʗ1.Value.Req.Body = b();
                break;
            }}
        }
        setBody();
        if ((~tt).Req.Header == default!) {
            tt.Value.Req.Header = new global::go.net.http_package.ΔHeader(0);
        }
        ref var braw = ref heap(new strings.Builder(), out var Ꮡbraw);
        var err = tt.of(reqWriteTest.ᏑReq).Write(new http_test_package.strings_BuilderжWriter(Ꮡbraw));
        {
            @string g = fmt.Sprintf("%v"u8, err);
            @string e = fmt.Sprintf("%v"u8, (~tt).WantError); if (g != e) {
                Ꮡt.Errorf("writing #%d, err = %q, want %q"u8, i, g, e);
                continue;
            }
        }
        if (err != default!) {
            continue;
        }
        if ((~tt).WantWrite != ""u8) {
            @string sraw = braw.String();
            if (sraw != (~tt).WantWrite) {
                Ꮡt.Errorf("Test %d, expecting:\n%s\nGot:\n%s\n"u8, i, (~tt).WantWrite, sraw);
                continue;
            }
        }
        if ((~tt).WantProxy != ""u8) {
            setBody();
            ref var praw = ref heap(new strings.Builder(), out var Ꮡpraw);
            err = tt.of(reqWriteTest.ᏑReq).WriteProxy(new http_test_package.strings_BuilderжWriter(Ꮡpraw));
            if (err != default!) {
                Ꮡt.Errorf("WriteProxy #%d: %s"u8, i, err);
                continue;
            }
            @string sraw = praw.String();
            if (sraw != (~tt).WantProxy) {
                Ꮡt.Errorf("Test Proxy %d, expecting:\n%s\nGot:\n%s\n"u8, i, (~tt).WantProxy, sraw);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string transferEncodingˢ3 = "Transfer-Encoding: "u8;
internal static readonly @string foobodyˢ = "foobody"u8;
internal static readonly @string contentLength7ˢ = "Content-Length: 7"u8;
internal static readonly @string transferEncodingChunkedˢ2 = "Transfer-Encoding: chunked"u8;
internal static readonly @string oobodyˢ = "oobody"u8;

internal partial struct TestRequestWriteTransport_testCase /*dyn*/ {
    internal @string method;
    internal int64 clen; // ContentLength
    internal io.ReadCloser body;
    internal Func<@string, error> want;
    // optional:
    internal Action<ж<TestRequestWriteTransport_testCase>> init;
    internal Action afterReqRead;
}

public static void TestRequestWriteTransport(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    Func<@string, error> matchSubstr(@string substr) => error (@string written) => {
            if (!strings.Contains(written, substr)) {
                return fmt.Errorf("expected substring %q in request: %s"u8, substr, written);
            }
            return default!;
        };
    var noContentLengthOrTransferEncoding = error (@string req) => {
        if (strings.Contains(req, contentLengthˢ3)) {
            return fmt.Errorf("unexpected Content-Length in request: %s"u8, req);
        }
        if (strings.Contains(req, transferEncodingˢ3)) {
            return fmt.Errorf("unexpected Transfer-Encoding in request: %s"u8, req);
        }
        return default!;
    };
    Func<@string, error> all(params Span<Func<@string, error>> checksʗp) {
        var checks = checksʗp.slice();
        var checksʗ1 = checks;
        return error (@string req) => {
            foreach (var (_, c) in checksʗ1) {
                {
                    var err = c(req); if (err != default!) {
                        return err;
                    }
                }
            }
            return default!;
        };
    }
    var tests = new TestRequestWriteTransport_testCase[]{
        new(
            method: "GET"u8,
            want: noContentLengthOrTransferEncoding
        ),
        new(
            method: "GET"u8,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
            want: noContentLengthOrTransferEncoding
        ),
        new(
            method: "GET"u8,
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
            want: noContentLengthOrTransferEncoding
        ), // A GET with a body, with explicit content length:

        new(
            method: "GET"u8,
            clen: 7,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(foobodyˢ))),
            want: all(matchSubstr(contentLength7ˢ),
                matchSubstr(foobodyˢ))
        ), // A GET with a body, sniffing the leading "f" from "foobody".

        new(
            method: "GET"u8,
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(foobodyˢ))),
            want: all(matchSubstr(transferEncodingChunkedˢ2),
                matchSubstr("\r\n1\r\nf\r\n"u8),
                matchSubstr(oobodyˢ))
        ), // But a POST request is expected to have a body, so
 // no sniffing happens:

        new(
            method: "POST"u8,
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(foobodyˢ))),
            want: all(matchSubstr(transferEncodingChunkedˢ2),
                matchSubstr(foobodyˢ))
        ),
        new(
            method: "POST"u8,
            clen: -1,
            body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
            want: all(matchSubstr(transferEncodingChunkedˢ2))
        ), // Verify that a blocking Request.Body doesn't block forever.

        new(
            method: "GET"u8,
            clen: -1,
            init: (ж<TestRequestWriteTransport_testCase> tt) => {
                var (pr, pw) = io.Pipe();
                var pwʗ1 = pw;
                tt.Value.afterReqRead = () => {
                    pwʗ1.Close();
                };
                tt.Value.body = io.NopCloser(new io.PipeReaderжReader(pr));
            },
            want: matchSubstr(transferEncodingChunkedˢ2)
        )
    }.slice();
    foreach (var (i, vᴛ1) in tests) {
        ref var tt = ref heap(new TestRequestWriteTransport_testCase(), out var Ꮡtt);
        tt = vᴛ1;

        if (tt.init != default!) {
            tt.init(Ꮡtt);
        }
        var req = Ꮡ(new Request(
            Method: tt.method,
            URL: Ꮡ(new url.URL(
                Scheme: "http"u8,
                Host: "example.com"u8
            )),
            Header: new global::go.net.http_package.ΔHeader(0),
            ContentLength: tt.clen,
            Body: tt.body
        ));
        var (got, err) = dumpRequestOut(req, tt.afterReqRead);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: %v"u8, i, err);
            continue;
        }
        {
            var errΔ1 = tt.want(((@string)got)); if (errΔ1 != default!) {
                Ꮡt.Errorf("test[%d]: %v"u8, i, errΔ1);
            }
        }
    }
}

internal partial struct closeChecker {
    /*embed*/ public io_package.Reader Reader;
    internal bool closed;
}

internal static error Close(this ref closeChecker rc) {
    rc.closed = true;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myBodyˢ = "my body"u8;
internal static readonly @string httpFooComˢ = "http://foo.com/"u8;
internal static readonly object bodyNotClosedAfterWriteˢ = (@string)"body not closed after write"u8;

// TestRequestWriteClosesBody tests that Request.Write closes its request.Body.
// It also indirectly tests NewRequest and that it doesn't wrap an existing Closer
// inside a NopCloser, and that it serializes it correctly.
public static void TestRequestWriteClosesBody(ж<testing.T> Ꮡt) {
    var rc = Ꮡ(new closeChecker(Reader: new http_test_package.strings_ReaderжReader(strings.NewReader(myBodyˢ))));
    var (req, err) = NewRequest(postˢ, httpFooComˢ, new http_internal_test_package.closeCheckerжReader(rc));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var buf = @new<strings.Builder>();
    {
        var errΔ1 = req.Write(new http_test_package.strings_BuilderжWriter(buf)); if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
        }
    }
    if (!(~rc).closed) {
        Ꮡt.Error(bodyNotClosedAfterWriteˢ);
    }
    @string expected = "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk(myBodyˢ) + chunk(""u8);
    if (buf.String() != expected) {
        Ꮡt.Errorf("write:\n got: %s\nwant: %s"u8, buf.String(), expected);
    }
}

internal static @string chunk(@string s) {
    return fmt.Sprintf("%x\r\n%s\r\n"u8, builtin.len(s), s);
}

internal static ж<url.URL> mustParseURL(@string s) {
    var (u, err) = url.Parse(s);
    if (err != default!) {
        throw panic(fmt.Sprintf("Error parsing URL %q: %v"u8, s, err));
    }
    return u;
}

internal delegate (nint, error) writerFunc(slice<byte> _);

internal static partial (nint, error) Write(this writerFunc f, slice<byte> p) {
    return f(p);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fakeWriteFailureˢ = "fake write failure"u8;

internal partial struct TestRequestWriteError_w /*dyn*/ {
    /*embed*/ public io_package.ByteWriter ByteWriter; // to avoid being wrapped by a bufio.Writer
    /*embed*/ public io_package.Writer Writer;
}

// TestRequestWriteError tests the Write err != nil checks in (*Request).write.
public static void TestRequestWriteError(ж<testing.T> Ꮡt) {
    nint failAfter = 0;
    nint writeCount = 0;
    var errFail = errors.New(fakeWriteFailureˢ);
    // w is the buffered io.Writer to write the request to. It
    // fails exactly once on its Nth Write call, as controlled by
    // failAfter. It also tracks the number of calls in
    // writeCount.
        var errFailʗ1 = errFail;
    var w = new TestRequestWriteError_w(
        default!,
        new http_internal_test_package.writerFuncᴠWriter(new writerFunc((slice<byte> p) => {
            error err = default!;
            writeCount++;
            if (failAfter == 0) {
                err = errFailʗ1;
            }
            failAfter--;
            return (builtin.len(p), err);
        }))
    );
    var (req, _) = NewRequest(getˢ, httpExampleComˢ, default!);
    UntypedInt writeCalls = 4; // number of Write calls in current implementation
    var sawGood = false;
    for (nint n = 0; n <= (nint)(writeCalls + 2); n++) {
        failAfter = n;
        writeCount = 0;
        var err = req.Write(w);
        error wantErr = default!;
        if (n < writeCalls) {
            wantErr = errFail;
        }
        if (!AreEqual(err, wantErr)) {
            Ꮡt.Errorf("for fail-after %d Writes, err = %v; want %v"u8, n, err, wantErr);
            continue;
        }
        if (err == default!) {
            sawGood = true;
            if (writeCount != writeCalls) {
                Ꮡt.Fatalf("writeCalls constant is outdated in test"u8);
            }
        }
        if (writeCount > writeCalls || writeCount > n + 1) {
            Ꮡt.Errorf("for fail-after %d, saw unexpectedly high (%d) write calls"u8, n, writeCount);
        }
    }
    if (!sawGood) {
        Ꮡt.Fatalf("writeCalls constant is outdated in test"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11204NoContentˢ = "HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"u8;

// dumpRequestOut is a modified copy of net/http/httputil.DumpRequestOut.
// Unlike the original, this version doesn't mutate the req.Body and
// try to restore it. It always dumps the whole body.
// And it doesn't support https.
internal static partial (slice<byte>, error) dumpRequestOut(ж<global::go.net.http_package.Request> Ꮡreq, Action onReadHeaders) {
    GoFrame ᒐ = default;
    try {
        // Use the actual Transport code to record what we would send
        // on the wire, but not using TCP.  Use a Transport with a
        // custom dialer that returns a fake net.Conn that waits
        // for the full input (and recording it), and then responds
        // with a dummy response.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);                       // records the output
        var (pr, pw) = io.Pipe();
        var prʗ1 = pr;
        defer(() => prʗ1.Close(), ref ᒐ);
        var pwʗ1 = pw;
        defer(() => pwʗ1.Close(), ref ᒐ);
        var dr = Ꮡ(new delegateReader(c: new channel<io.Reader>(0)));
            var drʗ1 = dr;
            var pwʗ2 = pw;
        var t = Ꮡ(new Transport(
            Dial: (@string netΔ1, @string addr) => (new http_internal_test_package.dumpConnжConn(Ꮡ(new dumpConn(io.MultiWriter(new http_test_package.bytes_BufferжWriter(Ꮡbuf), new io.PipeWriterжWriter(pwʗ2)), new http_internal_test_package.delegateReaderжReader(drʗ1)))), default!)
        ));
        var tʗ1 = t;
        defer(tʗ1.CloseIdleConnections, ref ᒐ);
        // Wait for the request before replying with a dummy response:
        var drʗ2 = dr;
        var prʗ2 = pr;
        goǃ(() => {
            var (reqΔ1, errΔ1) = ReadRequest(bufio.NewReader(new io.PipeReaderжReader(prʗ2)));
            if (errΔ1 == default!) {
                if (onReadHeaders != default!) {
                    onReadHeaders();
                }
                // Ensure all the body is read; otherwise
                // we'll get a partial dump.
                io.Copy(io.Discard, (~reqΔ1).Body);
                (~reqΔ1).Body.Close();
            }
            (~drʗ2).c.ᐸꟷ(new http_test_package.strings_ReaderжReader(strings.NewReader(http11204NoContentˢ)));
        });
        var (_, err) = t.RoundTrip(Ꮡreq);
        if (err != default!) {
            return (default!, err);
        }
        return (buf.Bytes(), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// delegateReader is a reader that delegates to another reader,
// once it arrives on a channel.
internal partial struct delegateReader {
    internal channel<io.Reader> c;
    internal io.Reader r; // nil until received from c
}

internal static (nint, error) Read(this ref delegateReader r, slice<byte> p) {
    if (r.r == default!) {
        r.r = ᐸꟷ(r.c);
    }
    return r.r.Read(p);
}

// dumpConn is a net.Conn that writes to Writer and reads from Reader.
internal partial struct dumpConn {
    /*embed*/ public io_package.Writer Writer;
    /*embed*/ public io_package.Reader Reader;
}

internal static error Close(this ref dumpConn c) {
    return default!;
}

internal static netꓸAddr LocalAddr(this ref dumpConn c) {
    return default!;
}

internal static netꓸAddr RemoteAddr(this ref dumpConn c) {
    return default!;
}

internal static error SetDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

internal static error SetReadDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

internal static error SetWriteDeadline(this ref dumpConn c, time.Time t) {
    return default!;
}

} // end http_internal_test_package
