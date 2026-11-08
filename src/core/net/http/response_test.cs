// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using gzip = compress.gzip_package;
using rand = crypto.rand_package;
using fmt = fmt_package;
using token = global::go.go.token_package;
using io = io_package;
using @internal = global::go.net.http.internal_package;
using url = global::go.net.url_package;
using reflect = reflect_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using compress;
using crypto;
using global::go.go;
using global::go.net;
using global::go.net.http;
using static global::go.net.http_package;
using ꓸꓸꓸany = Span<any>;

partial class http_internal_test_package {

internal partial struct respTest {
    public @string Raw;
    public @string RawOut;
    public global::go.net.http_package.Response Resp;
    public @string Body;
}

internal static ж<global::go.net.http_package.Request> dummyReq(@string method) {
    return Ꮡ(new Request(Method: method));
}

internal static ж<global::go.net.http_package.Request> dummyReq11(@string method) {
    return Ꮡ(new Request(Method: method, Proto: "HTTP/1.1", ProtoMajor: 1, ProtoMinor: 1));
}

internal static slice<respTest> respTests = slice<respTest>([
    // Unchunked response without Content-Length.
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"] = ["close"] // TODO(rsc): Delete?
            }),
            Close: true,
            ContentLength: -1
        ),

        "Body here\n"
    ),

    // Unchunked HTTP/1.1 response without Content-Length or
    // Connection headers.
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Request: dummyReq("GET"),
            Close: true,
            ContentLength: -1
        ),

        "Body here\n"
    ),

    // Unchunked HTTP/1.1 204 response without Content-Length.
    new(
        "HTTP/1.1 204 No Content\r\n"u8 + "\r\n"u8 + "Body should not be read!\n"u8,

        "HTTP/1.1 204 No Content\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "204 No Content",
            StatusCode: 204,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Request: dummyReq("GET"),
            Close: false,
            ContentLength: 0
        ),

        ""
    ),

    // Unchunked response with Content-Length.
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"] = ["close"],
                ["Content-Length"] = ["10"]
            }),
            Close: true,
            ContentLength: 10
        ),

        "Body here\n"
    ),

    // Chunked response without Content-Length.
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "0a\r\n"u8 + "Body here\n\r\n"u8 + "09\r\n"u8 + "continued\r\n"u8 + "0\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "13\r\n"u8 + "Body here\ncontinued\r\n"u8 + "0\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: -1,
            TransferEncoding: ["chunked"]
        ),

        "Body here\ncontinued"
    ),

    // Trailer header but no TransferEncoding
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Trailer: Content-MD5, Content-Sources\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"] = ["close"],
                ["Content-Length"] = ["10"],
                ["Trailer"] = ["Content-MD5, Content-Sources"]
            }),
            Close: true,
            ContentLength: 10
        ),

        "Body here\n"
    ),

    // Chunked response with Content-Length.
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Content-Length: 10\r\n"u8 + "\r\n"u8 + "0a\r\n"u8 + "Body here\n\r\n"u8 + "0\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "a\r\n"u8 + "Body here\n\r\n"u8 + "0\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: -1,
            TransferEncoding: ["chunked"]
        ),

        "Body here\n"
    ),

    // Chunked response in response to a HEAD request
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            TransferEncoding: ["chunked"],
            Close: false,
            ContentLength: -1
        ),

        ""
    ),

    // Content-Length in response to a HEAD request
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("HEAD"),
            Header: new ΔHeader(new map<@string, slice<@string>>{["Content-Length"] = ["256"]}),
            TransferEncoding: default,
            Close: true,
            ContentLength: 256
        ),

        ""
    ),

    // Content-Length in response to a HEAD request with HTTP/1.1
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"),
            Header: new ΔHeader(new map<@string, slice<@string>>{["Content-Length"] = ["256"]}),
            TransferEncoding: default,
            Close: false,
            ContentLength: 256
        ),

        ""
    ),

    // No Content-Length or Chunked in response to a HEAD request
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "\r\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("HEAD"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            TransferEncoding: default,
            Close: true,
            ContentLength: -1
        ),

        ""
    ),

    // explicit Content-Length of 0.
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["0"]
            }),
            Close: false,
            ContentLength: 0
        ),

        ""
    ),

    // Status line without a Reason-Phrase, but trailing space.
    // (permitted by RFC 7230, section 3.1.2)
    new(
        "HTTP/1.0 303 \r\n\r\n",

        "HTTP/1.0 303 \r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "303 ",
            StatusCode: 303,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: -1
        ),

        ""
    ),

    // Status line without a Reason-Phrase, and no trailing space.
    // (not permitted by RFC 7230, but we'll accept it anyway)
    new(
        "HTTP/1.0 303\r\n\r\n",

        "HTTP/1.0 303 303\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "303",
            StatusCode: 303,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: -1
        ),

        ""
    ),

    // golang.org/issue/4767: don't special-case multipart/byteranges responses
    new(
        """
HTTP/1.1 206 Partial Content
Connection: close
Content-Type: multipart/byteranges; boundary=18a75608c8f47cef

some body
"""u8.Raw(),

        "HTTP/1.1 206 Partial Content\r\n"u8 + "Connection: close\r\n"u8 + "Content-Type: multipart/byteranges; boundary=18a75608c8f47cef\r\n"u8 + "\r\n"u8 + "some body"u8,

        new Response(
            Status: "206 Partial Content",
            StatusCode: 206,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Type"] = ["multipart/byteranges; boundary=18a75608c8f47cef"]
            }),
            Close: true,
            ContentLength: -1
        ),

        "some body"
    ),

    // Unchunked response without Content-Length, Request is nil
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"] = ["close"] // TODO(rsc): Delete?
            }),
            Close: true,
            ContentLength: -1
        ),

        "Body here\n"
    ),

    // 206 Partial Content. golang.org/issue/8923
    new(
        "HTTP/1.1 206 Partial Content\r\n"u8 + "Content-Type: text/plain; charset=utf-8\r\n"u8 + "Accept-Ranges: bytes\r\n"u8 + "Content-Range: bytes 0-5/1862\r\n"u8 + "Content-Length: 6\r\n\r\n"u8 + "foobar"u8,

        "HTTP/1.1 206 Partial Content\r\n"u8 + "Content-Length: 6\r\n"u8 + "Accept-Ranges: bytes\r\n"u8 + "Content-Range: bytes 0-5/1862\r\n"u8 + "Content-Type: text/plain; charset=utf-8\r\n"u8 + "\r\n"u8 + "foobar"u8,

        new Response(
            Status: "206 Partial Content",
            StatusCode: 206,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Accept-Ranges"] = ["bytes"],
                ["Content-Length"] = ["6"],
                ["Content-Type"] = ["text/plain; charset=utf-8"],
                ["Content-Range"] = ["bytes 0-5/1862"]
            }),
            ContentLength: 6
        ),

        "foobar"
    ),

    // Both keep-alive and close, on the same Connection line. (Issue 8840)
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "Connection: keep-alive, close\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["256"]
            }),
            TransferEncoding: default,
            Close: true,
            ContentLength: 256
        ),

        ""
    ),

    // Both keep-alive and close, on different Connection lines. (Issue 8840)
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "Connection: keep-alive\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,

        "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["256"]
            }),
            TransferEncoding: default,
            Close: true,
            ContentLength: 256
        ),

        ""
    ),

    // Issue 12785: HTTP/1.0 response with bogus (to be ignored) Transfer-Encoding.
    // Without a Content-Length.
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Transfer-Encoding: bogus\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: -1
        ),

        "Body here\n"
    ),

    // Issue 12785: HTTP/1.0 response with bogus (to be ignored) Transfer-Encoding.
    // With a Content-Length.
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Transfer-Encoding: bogus\r\n"u8 + "Content-Length: 10\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 10\r\n"u8 + "\r\n"u8 + "Body here\n"u8,

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["10"]
            }),
            Close: true,
            ContentLength: 10
        ),

        "Body here\n"
    ),

    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Encoding: gzip\r\n"u8 + "Content-Length: 23\r\n"u8 + "Connection: keep-alive\r\n"u8 + "Keep-Alive: timeout=7200\r\n\r\n"u8 + ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00})),

        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 23\r\n"u8 + "Connection: keep-alive\r\n"u8 + "Content-Encoding: gzip\r\n"u8 + "Keep-Alive: timeout=7200\r\n\r\n"u8 + ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00})),

        new Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["23"],
                ["Content-Encoding"] = ["gzip"],
                ["Connection"] = ["keep-alive"],
                ["Keep-Alive"] = ["timeout=7200"]
            }),
            Close: false,
            ContentLength: 23
        ),
        ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00}))
    ),

    // Issue 19989: two spaces between HTTP version and status.
    new(
        "HTTP/1.0  401 Unauthorized\r\n"u8 + "Content-type: text/html\r\n"u8 + "WWW-Authenticate: Basic realm=\"\"\r\n\r\n"u8 + "Your Authentication failed.\r\n"u8,

        "HTTP/1.0 401 Unauthorized\r\n"u8 + "Connection: close\r\n"u8 + "Content-Type: text/html\r\n"u8 + "Www-Authenticate: Basic realm=\"\"\r\n"u8 + "\r\n"u8 + "Your Authentication failed.\r\n"u8,

        new Response(
            Status: "401 Unauthorized",
            StatusCode: 401,
            Proto: "HTTP/1.0",
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Type"] = ["text/html"],
                ["Www-Authenticate"] = [@"Basic realm="""""]
            }),
            Close: true,
            ContentLength: -1
        ),
        "Your Authentication failed.\r\n"
    )
]);

// tests successful calls to ReadResponse, and inspects the returned Response.
// For error cases, see TestReadResponseErrors below.
public static void TestReadResponse(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(respTests)) {
        ref var tt = ref heap(new respTest(), out var Ꮡtt);
        tt = vᴛ1;

        var (resp, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.Raw))), tt.Resp.Request);
        if (err != nil) {
            t.Errorf("#%d: %v", i, err);
            continue;
        }
        var rbody = resp.Body;
        resp.Body = default;
        diff(t, fmt.Sprintf("#%d Response"u8, i), resp.OrTypedNil(), Ꮡtt.ᏑResp);
        ref var bout = ref heap(new strings.Builder(), out var Ꮡbout);
        if (rbody != nil) {
            (_, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbout), rbody);
            if (err != nil) {
                t.Errorf("#%d: %v", i, err);
                continue;
            }
            rbody.Close();
        }
        @string body = bout.String();
        if (body != tt.Body) {
            t.Errorf("#%d: Body = %q want %q", i, body, tt.Body);
        }
    }
}

public static void TestWriteResponse(ж<testing.T> t) {
    foreach (var (i, tt) in range(respTests)) {
        var (resp, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.Raw))), tt.Resp.Request);
        if (err != nil) {
            t.Errorf("#%d: %v", i, err);
            continue;
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = resp.Write(new http_test_package.bytes_BufferжWriter(Ꮡbuf));
        if (err != nil) {
            t.Errorf("#%d: %v", i, err);
            continue;
        }
        @string got = Ꮡbuf.String();
        @string want = tt.RawOut; if (got != want) {
            t.Errorf("#%d: response differs; got:\n----\n%v\n----\nwant:\n----\n%v\n----\n",
                i,
                strings.ReplaceAll(got, "\r", "\\r"),
                strings.ReplaceAll(want, "\r", "\\r"));
        }
    }
}

partial struct readResponseCloseInMiddleTestsᴛ1 /*dyn*/ {
    internal bool chunked, compressed;
}
internal static slice<readResponseCloseInMiddleTestsᴛ1> readResponseCloseInMiddleTests = [
    new(false, false),
    new(true, false),
    new(true, true)
];

internal partial struct readerAndCloser {
    /*embed*/ public io_package.Reader Reader;
    /*embed*/ public io_package.Closer Closer;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string http11200Okˢ => ref http11200Okˢᵇ;
private static readonly @string http11200Okˢᵇ = "HTTP/1.1 200 OK\r\n"u8;
internal static ref readonly @string contentLength1000000ˢ => ref contentLength1000000ˢᵇ;
private static readonly @string contentLength1000000ˢᵇ = "Content-Length: 1000000\r\n"u8;
internal static ref readonly @string contentEncodingGzipˢ => ref contentEncodingGzipˢᵇ;
private static readonly @string contentEncodingGzipˢᵇ = "Content-Encoding: gzip\r\n"u8;
internal static ref readonly @string randReaderReadFullˢ => ref randReaderReadFullˢᵇ;
private static readonly @string randReaderReadFullˢᵇ = "rand.Reader ReadFull"u8;
internal static ref readonly @string compressorCloseˢ => ref compressorCloseˢᵇ;
private static readonly @string compressorCloseˢᵇ = "compressor close"u8;
internal static ref readonly @string nextRequestHereˢ => ref nextRequestHereˢᵇ;
private static readonly @string nextRequestHereˢᵇ = "Next Request Here"u8;
internal static ref readonly @string readResponseˢ => ref readResponseˢᵇ;
private static readonly @string readResponseˢᵇ = "ReadResponse"u8;
internal static ref readonly @string gzipNewReaderˢ => ref gzipNewReaderˢᵇ;
private static readonly @string gzipNewReaderˢᵇ = "gzip.NewReader"u8;
internal static ref readonly @string byteReadFullˢ => ref byteReadFullˢᵇ;
private static readonly @string byteReadFullˢᵇ = "2500 byte ReadFull"u8;
internal static ref readonly @string readAllOnRemainderˢ => ref readAllOnRemainderˢᵇ;
private static readonly @string readAllOnRemainderˢᵇ = "ReadAll on remainder"u8;

// TestReadResponseCloseInMiddle tests that closing a body after
// reading only part of its contents advances the read to the end of
// the request, right up until the next request.
public static void TestReadResponseCloseInMiddle(ж<testing.T> t) {
    t.Parallel();
    foreach (var (_, vᴛ1) in range(readResponseCloseInMiddleTests)) {
        ref var test = ref heap(new readResponseCloseInMiddleTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        void fatalf(@string format, params ꓸꓸꓸany argsʗp) {
            var args = argsʗp.slice();
            args = appendꓸꓸꓸ(slice<any>([testʗ1.chunked, testʗ1.compressed]), args);
            t.Fatalf("on test chunked=%v, compressed=%v: "u8 + format, args.ꓸꓸꓸ);
        }
        var fatalfʗ1 = fatalf;
        void checkErr(error errΔ1, @string msg) {
            if (errΔ1 == nil) {
                return;
            }
            fatalfʗ1(msg + ": %v"u8, errΔ1);
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        buf.WriteString(http11200Okˢ);
        if (test.chunked) {
            buf.WriteString(transferEncodingChunkedˢ);
        } else {
            buf.WriteString(contentLength1000000ˢ);
        }
        io.Writer wr = new http_test_package.bytes_BufferжWriter(Ꮡbuf);
        if (test.chunked) {
            wr = new http_test_package.io_WriteCloserᴠWriter(@internal.NewChunkedWriter(wr));
        }
        if (test.compressed) {
            buf.WriteString(contentEncodingGzipˢ);
            wr = new http_test_package.gzip_WriterжWriter(gzip.NewWriter(wr));
        }
        buf.WriteString("\r\n");

        var chunk = bytes.Repeat([(byte)'x'], 1000);
        for (nint i = 0; i < 1000; i++) {
            if (test.compressed) {
                // Otherwise this compresses too well.
                var (_, errΔ2) = io.ReadFull(rand.Reader, chunk);
                checkErr(errΔ2, randReaderReadFullˢ);
            }
            wr.Write(chunk);
        }
        if (test.compressed) {
            var errΔ3 = wr._<ж<gzip.Writer>>().Close();
            checkErr(errΔ3, compressorCloseˢ);
        }
        if (test.chunked) {
            buf.WriteString("0\r\n\r\n");
        }
        buf.WriteString(nextRequestHereˢ);

        var bufr = bufio.NewReader(new http_test_package.bytes_BufferжReader(Ꮡbuf));
        var (resp, err) = ReadResponse(bufr, dummyReq(getˢ));
        checkErr(err, readResponseˢ);
        var expectedLength = (int64)(-1);
        if (!test.chunked) {
            expectedLength = 1000000;
        }
        if (resp.ContentLength != expectedLength) {
            fatalf("expected response length %d, got %d", expectedLength, resp.ContentLength);
        }
        if (resp.Body == nil) {
            fatalf("nil body");
        }
        if (test.compressed) {
            var (gzReader, errΔ1) = gzip.NewReader(resp.Body);
            checkErr(errΔ1, gzipNewReaderˢ);
            resp.Body = new http_internal_test_package.readerAndCloserжReadCloser(Ꮡ(new readerAndCloser(new http_test_package.gzip_ReaderжReader(gzReader), resp.Body)));
        }

        var rbuf = new slice<byte>(2500);
        (var n, err) = io.ReadFull(resp.Body, rbuf);
        checkErr(err, byteReadFullˢ);
        if (n != 2500) {
            fatalf("ReadFull only read %d bytes", n);
        }
        if (test.compressed == false && !bytes.Equal(bytes.Repeat([(byte)'x'], 2500), rbuf)) {
            fatalf("ReadFull didn't read 2500 'x'; got %q", (@string)rbuf);
        }
        resp.Body.Close();

        (var rest, err) = io.ReadAll(new http_test_package.bufio_ReaderжReader(bufr));
        checkErr(err, readAllOnRemainderˢ);
        {
            @string e = nextRequestHereˢ;
            @string g = (@string)rest; if (e != g) {
                g = regexp.MustCompile(@"(xx+)").ReplaceAllStringFunc(g, (@string match) => fmt.Sprintf("x(repeated x%d)"u8, builtin.len(match)));
                fatalf("remainder = %q, expected %q", g, e);
            }
        }
    }
}

internal static void diff(ж<testing.T> t, @string prefix, any have, any want) {
    t.Helper();
    var hv = reflect.ValueOf(have).Elem();
    var wv = reflect.ValueOf(want).Elem();
    if (!AreEqual(hv.Type(), wv.Type())) {
        t.Errorf("%s: type mismatch %v want %v", prefix, hv.Type(), wv.Type());
    }
    for (nint i = 0; i < hv.NumField(); i++) {
        @string name = hv.Type().Field(i).Name;
        if (!token.IsExported(name)) {
            continue;
        }
        var hf = hv.Field(i).Interface();
        var wf = wv.Field(i).Interface();
        if (!reflect.DeepEqual(hf, wf)) {
            t.Errorf("%s: %s = %v want %v", prefix, name, hf, wf);
        }
    }
}

internal partial struct responseLocationTest {
    internal @string location; // Response's Location header or ""
    internal @string requrl;   // Response.Request.URL or ""
    internal @string want;
    internal error wantErr;
}

internal static slice<responseLocationTest> responseLocationTests;
internal static void initᴛresponseLocationTests() { responseLocationTests = [
    new("/foo", "http://bar.com/baz", "http://bar.com/foo", default),
    new("http://foo.com/", "http://bar.com/baz", "http://foo.com/", default),
    new("", "http://bar.com/baz", "", ErrNoLocation),
    new("/bar", "", "/bar", default)
]; }

public static void TestLocationResponse(ж<testing.T> t) {
    foreach (var (i, tt) in range(responseLocationTests)) {
        var res = @new<global::go.net.http_package.Response>();
        res.Header = new global::go.net.http_package.ΔHeader(0);
        res.Header.Set(locationˢ, tt.location);
        if (tt.requrl != ""u8) {
            res.Request = Ꮡ(new Request());
            error errΔ1 = default;
            (res.Request.URL, errΔ1) = url.Parse(tt.requrl);
            if (errΔ1 != nil) {
                t.Fatalf("bad test URL %q: %v", tt.requrl, errΔ1);
            }
        }

        var (got, err) = res.Location();
        if (tt.wantErr != nil) {
            if (err == nil) {
                t.Errorf("%d. err=nil; want %q", i, tt.wantErr);
                continue;
            }
            @string g = err.Error();
            @string e = tt.wantErr.Error(); if (g != e) {
                t.Errorf("%d. err=%q; want %q", i, g, e);
                continue;
            }
            continue;
        }
        if (err != nil) {
            t.Errorf("%d. err=%q", i, err);
            continue;
        }
        {
            @string g = got.String();
            @string e = tt.want; if (g != e) {
                t.Errorf("%d. Location=%q; want %q", i, g, e);
            }
        }
    }
}

public static void TestResponseStatusStutter(ж<testing.T> t) {
    var r = Ꮡ(new Response(
        Status: "123 some status",
        StatusCode: 123,
        ProtoMajor: 1,
        ProtoMinor: 3
    ));
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    r.Write(new http_test_package.strings_BuilderжWriter(Ꮡbuf));
    if (strings.Contains(buf.String(), "123 123")) {
        t.Errorf("stutter in status: %s", buf.String());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string shortBodyᶜ => ref shortBodyᶜᵇ;
private static readonly @string shortBodyᶜᵇ = "Short body, not 123 bytes."u8;

public static void TestResponseContentLengthShortBody(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string shortBody = shortBodyᶜ;
        var br = bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader("HTTP/1.1 200 OK\r\n" + "Content-Length: 123\r\n" + "\r\n" + shortBody)));
        var (res, err) = ReadResponse(br, Ꮡ(new Request(Method: "GET")));
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.ContentLength != 123) {
            t.Fatalf("Content-Length = %d; want 123", res.ContentLength);
        }
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        (var n, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbuf), res.Body);
        if (n != (int64)builtin.len(shortBody)) {
            t.Errorf("Copied %d bytes; want %d, len(%q)", n, builtin.len(shortBody), shortBody);
        }
        if (buf.String() != shortBody) {
            t.Errorf("Read body %q; want %q", buf.String(), shortBody);
        }
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            t.Errorf("io.Copy error = %#v; want io.ErrUnexpectedEOF", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string messageCannotContainˢ => ref messageCannotContainˢᵇ;
private static readonly @string messageCannotContainˢᵇ = "message cannot contain multiple Content-Length headers"u8;
internal static ref readonly @string unknownˢ => ref unknownˢᵇ;
private static readonly @string unknownˢᵇ = "20X Unknown"u8;
internal static ref readonly @string abcdUnknownˢ => ref abcdUnknownˢᵇ;
private static readonly @string abcdUnknownˢᵇ = "abcd Unknown"u8;
internal static ref readonly @string unknownˢ2 => ref unknownˢ2ᵇ;
private static readonly @string unknownˢ2ᵇ = " Unknown"u8;
internal static ref readonly @string c8Okˢ => ref c8Okˢᵇ;
private static readonly @string c8Okˢᵇ = "c8 OK"u8;
internal static ref readonly @string movedPermanentlyˢ => ref movedPermanentlyˢᵇ;
private static readonly @string movedPermanentlyˢᵇ = "0x12d Moved Permanently"u8;
internal static ref readonly @string notfoundˢ => ref notfoundˢᵇ;
private static readonly @string notfoundˢᵇ = "404 NOTFOUND"u8;
internal static ref readonly @string doneˢ => ref doneˢᵇ;
private static readonly @string doneˢᵇ = "999 Done"u8;
internal static ref readonly @string http12ˢ => ref http12ˢᵇ;
private static readonly @string http12ˢᵇ = "HTTP/1.2"u8;
internal static ref readonly @string http20ˢ => ref http20ˢᵇ;
private static readonly @string http20ˢᵇ = "HTTP/2.0"u8;
internal static ref readonly @string http1100000000002ˢ => ref http1100000000002ˢᵇ;
private static readonly @string http1100000000002ˢᵇ = "HTTP/1.100000000002"u8;
internal static ref readonly @string http11ˢ => ref http11ˢᵇ;
private static readonly @string http11ˢᵇ = "HTTP/1.-1"u8;
internal static ref readonly @string httpABˢ => ref httpABˢᵇ;
private static readonly @string httpABˢᵇ = "HTTP/A.B"u8;
internal static ref readonly @string http1ˢ => ref http1ˢᵇ;
private static readonly @string http1ˢᵇ = "HTTP/1"u8;
internal static ref readonly @string contentLength10Contentˢ => ref contentLength10Contentˢᵇ;
private static readonly @string contentLength10Contentˢᵇ = "Content-Length: 10\r\nContent-Length: 7\r\n\r\nGopher hey\r\n"u8;
internal static ref readonly @string contentLength7Contentˢ => ref contentLength7Contentˢᵇ;
private static readonly @string contentLength7Contentˢᵇ = "Content-Length: 7\r\nContent-Length: 7\r\n\r\nGophers\r\n"u8;
internal static ref readonly @string contentLength0Contentˢ => ref contentLength0Contentˢᵇ;
private static readonly @string contentLength0Contentˢᵇ = "Content-Length: 0\r\nContent-Length: 7\r\n\r\nGophers\r\n"u8;
internal static ref readonly @string contentLength0Contentˢ2 => ref contentLength0Contentˢ2ᵇ;
private static readonly @string contentLength0Contentˢ2ᵇ = "Content-Length: 0\r\nContent-Length: 0 \r\n\r\nGophers\r\n"u8;
internal static ref readonly @string contentLengthContentˢ => ref contentLengthContentˢᵇ;
private static readonly @string contentLengthContentˢᵇ = "Content-Length:\r\nContent-Length:\r\n\r\nGophers\r\n"u8;
internal static ref readonly @string contentLengthContentˢ2 => ref contentLengthContentˢ2ᵇ;
private static readonly @string contentLengthContentˢ2ᵇ = "Content-Length:\r\nContent-Length: 0 \r\nConnection: close\r\n\r\nGophers\r\n"u8;
internal static ref readonly @string contentLength7Contentˢ2 => ref contentLength7Contentˢ2ᵇ;
private static readonly @string contentLength7Contentˢ2ᵇ = "Content-Length: 7\r\nContent-Length: 8\r\n\r\n"u8;
internal static ref readonly @string contentLength3Contentˢ => ref contentLength3Contentˢᵇ;
private static readonly @string contentLength3Contentˢᵇ = "Content-Length: 3\r\nContent-Length: 3\r\n\r\n"u8;
internal static ref readonly @string contentLength880Contentˢ => ref contentLength880Contentˢᵇ;
private static readonly @string contentLength880Contentˢᵇ = "Content-Length: 880\r\nContent-Length: 1\r\n\r\n"u8;
internal static ref readonly @string contentLength961Contentˢ => ref contentLength961Contentˢᵇ;
private static readonly @string contentLength961Contentˢᵇ = "Content-Length: 961\r\nContent-Length: 961\r\n\r\n"u8;
internal static readonly object malformedMimeˢ = (@string)"malformed MIME"u8;

internal partial struct TestReadResponseErrors_testCase /*dyn*/ {
    internal @string name; // optional, defaults to in
    internal @string @in;
    internal any wantErr; // nil, err value, bool value, or string substring
}

// Test various ReadResponse error cases. (also tests success cases, but mostly
// it's about errors).  This does not test anything involving the bodies. Only
// the return value from ReadResponse itself.
public static void TestReadResponseErrors(ж<testing.T> t) {
    TestReadResponseErrors_testCase status(@string s, any wantErr) {
        if (AreEqual(wantErr, true)) {
            wantErr = malformedHttpStatusCodeˢ;
        }
        return new TestReadResponseErrors_testCase(
            name: fmt.Sprintf("status %q"u8, s),
            @in: "HTTP/1.1 "u8 + s + "\r\nFoo: bar\r\n\r\n"u8,
            wantErr: wantErr
        );
    }

    TestReadResponseErrors_testCase version(@string s, any wantErr) {
        if (AreEqual(wantErr, true)) {
            wantErr = malformedHttpVersionˢ;
        }
        return new TestReadResponseErrors_testCase(
            name: fmt.Sprintf("version %q"u8, s),
            @in: s + " 200 OK\r\n\r\n"u8,
            wantErr: wantErr
        );
    }

    TestReadResponseErrors_testCase contentLength(@string statusΔ1, @string body, any wantErr) => new TestReadResponseErrors_testCase(
            name: fmt.Sprintf("status %q %q"u8, statusΔ1, body),
            @in: fmt.Sprintf("HTTP/1.1 %s\r\n%s"u8, statusΔ1, body),
            wantErr: wantErr
        );

    @string errMultiCL = messageCannotContainˢ;
    @string errEmptyCL = invalidEmptyContentˢ;

    var tests = slice<TestReadResponseErrors_testCase>([
        new("", "", io.ErrUnexpectedEOF),
        new("", "HTTP/1.1 301 Moved Permanently\r\nFoo: bar", io.ErrUnexpectedEOF),
        new("", "HTTP/1.1", malformedHttpResponseˢ),
        new("", "HTTP/2.0", malformedHttpResponseˢ),
        status(unknownˢ, true),
        status(abcdUnknownˢ, true),
        status("二百/两百 OK", true),
        status(unknownˢ2, true),
        status(c8Okˢ, true),
        status(movedPermanentlyˢ, true),
        status("200 OK", default),
        status("000 OK", default),
        status("001 OK", default),
        status(notfoundˢ, default),
        status("20 OK", true),
        status("00 OK", true),
        status("-10 OK", true),
        status("1000 OK", true),
        status(doneˢ, default),
        status("-1 OK", true),
        status("-200 OK", true),
        version(http12ˢ, default),
        version(http20ˢ, default),
        version(http1100000000002ˢ, true),
        version(http11ˢ, true),
        version(httpABˢ, true),
        version(http1ˢ, true),
        version(http11ˢ2, true),

        contentLength("200 OK", contentLength10Contentˢ, errMultiCL),
        contentLength("200 OK", contentLength7Contentˢ, default),
        contentLength("201 OK", contentLength0Contentˢ, errMultiCL),
        contentLength("300 OK", contentLength0Contentˢ2, default),
        contentLength("200 OK", contentLengthContentˢ, errEmptyCL),
        contentLength("206 OK", contentLengthContentˢ2, errMultiCL),

        // multiple content-length headers for 204 and 304 should still be checked
        contentLength("204 OK", contentLength7Contentˢ2, errMultiCL),
        contentLength("204 OK", contentLength3Contentˢ, default),
        contentLength("304 OK", contentLength880Contentˢ, errMultiCL),
        contentLength("304 OK", contentLength961Contentˢ, default),

        // golang.org/issue/22464
        new("leading space in header", "HTTP/1.1 200 OK\r\n Content-type: text/html\r\nFoo: bar\r\n\r\n", malformedMimeˢ),
        new("leading tab in header", "HTTP/1.1 200 OK\r\n\tContent-type: text/html\r\nFoo: bar\r\n\r\n", malformedMimeˢ)
    ]);

    foreach (var (i, tt) in range(tests)) {
        var br = bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)));
        var (_, rerr) = ReadResponse(br, nil);
        var err = matchErr(rerr, tt.wantErr); if (err != nil) {
            @string name = tt.name;
            if (name == ""u8) {
                name = fmt.Sprintf("%d. input %q"u8, i, tt.@in);
            }
            t.Errorf("%s: %v", name, err);
        }
    }
}

// wantErr can be nil, an error value to match exactly, or type string to
// match a substring.
internal static error matchErr(error err, any wantErr) {
    if (err == nil) {
        if (wantErr == nil) {
            return default;
        }
        var (sub, ok) = wantErr._<@string>(ᐧ); if (ok) {
            return fmt.Errorf("unexpected success; want error with substring %q"u8, sub);
        }
        return fmt.Errorf("unexpected success; want error %v"u8, wantErr);
    }
    if (wantErr == nil) {
        return fmt.Errorf("%v; want success"u8, err);
    }
    {
        var (sub, ok) = wantErr._<@string>(ᐧ); if (ok) {
            if (strings.Contains(err.Error(), sub)) {
                return default;
            }
            return fmt.Errorf("error = %v; want an error with substring %q"u8, err, sub);
        }
    }
    if (AreEqual(err, wantErr)) {
        return default;
    }
    return fmt.Errorf("%v; want %v"u8, err, wantErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string http10200OkAaaaˢ => ref http10200OkAaaaˢᵇ;
private static readonly @string http10200OkAaaaˢᵇ = "HTTP/1.0 200 OK\r\n\r\nAAAA"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string connectionCloseHeaderᶜ => ref connectionCloseHeaderᶜᵇ;
private static readonly @string connectionCloseHeaderᶜᵇ = "Connection: close"u8;

// A response should only write out single Connection: close header. Tests #19499.
public static void TestResponseWritesOnlySingleConnectionClose(ж<testing.T> t) {
    @string connectionCloseHeader = connectionCloseHeaderᶜ;

    var (res, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(http10200OkAaaaˢ))), nil);
    if (err != nil) {
        t.Fatalf("ReadResponse failed %v", err);
    }

    ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
    err = res.Write(new http_test_package.bytes_BufferжWriter(Ꮡbuf1)); if (err != nil) {
        t.Fatalf("Write failed %v", err);
    }
    (res, err) = ReadResponse(bufio.NewReader(new http_test_package.bytes_BufferжReader(Ꮡbuf1)), nil); if (err != nil) {
        t.Fatalf("ReadResponse failed %v", err);
    }

    ref var buf2 = ref heap(new strings.Builder(), out var Ꮡbuf2);
    err = res.Write(new http_test_package.strings_BuilderжWriter(Ꮡbuf2)); if (err != nil) {
        t.Fatalf("Write failed %v", err);
    }
    nint count = strings.Count(buf2.String(), connectionCloseHeader); if (count != 1) {
        t.Errorf("Found %d %q header", count, connectionCloseHeader);
    }
}

} // end http_internal_test_package
