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

[GoType] internal partial struct respTest {
    public @string Raw;
    public @string RawOut;
    public global::go.net.http_package.Response Resp;
    public @string Body;
}

internal static ж<global::go.net.http_package.Request> dummyReq(@string method) {
    return Ꮡ(new Request(Method: method));
}

internal static ж<global::go.net.http_package.Request> dummyReq11(@string method) {
    return Ꮡ(new Request(Method: method, Proto: "HTTP/1.1"u8, ProtoMajor: 1, ProtoMinor: 1));
}

// Unchunked response without Content-Length.
// TODO(rsc): Delete?
// Unchunked HTTP/1.1 response without Content-Length or
// Connection headers.
// Unchunked HTTP/1.1 204 response without Content-Length.
// Unchunked response with Content-Length.
// Chunked response without Content-Length.
// Trailer header but no TransferEncoding
// Chunked response with Content-Length.
// Chunked response in response to a HEAD request
// Content-Length in response to a HEAD request
// Content-Length in response to a HEAD request with HTTP/1.1
// No Content-Length or Chunked in response to a HEAD request
// explicit Content-Length of 0.
// Status line without a Reason-Phrase, but trailing space.
// (permitted by RFC 7230, section 3.1.2)
// Status line without a Reason-Phrase, and no trailing space.
// (not permitted by RFC 7230, but we'll accept it anyway)
// golang.org/issue/4767: don't special-case multipart/byteranges responses
// Unchunked response without Content-Length, Request is nil
// TODO(rsc): Delete?
// 206 Partial Content. golang.org/issue/8923
// Both keep-alive and close, on the same Connection line. (Issue 8840)
// Both keep-alive and close, on different Connection lines. (Issue 8840)
// Issue 12785: HTTP/1.0 response with bogus (to be ignored) Transfer-Encoding.
// Without a Content-Length.
// Issue 12785: HTTP/1.0 response with bogus (to be ignored) Transfer-Encoding.
// With a Content-Length.
// Issue 19989: two spaces between HTTP version and status.
internal static slice<respTest> respTests = new respTest[]{
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"u8] = new @string[]{"close"u8}.slice()
            }),
            Close: true,
            ContentLength: -1
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Request: dummyReq("GET"u8),
            Close: true,
            ContentLength: -1
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 204 No Content\r\n"u8 + "\r\n"u8 + "Body should not be read!\n"u8,
        "HTTP/1.1 204 No Content\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "204 No Content"u8,
            StatusCode: 204,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Request: dummyReq("GET"u8),
            Close: false,
            ContentLength: 0
        ),
        ""u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"u8] = new @string[]{"close"u8}.slice(),
                ["Content-Length"u8] = new @string[]{"10"u8}.slice()
            }),
            Close: true,
            ContentLength: 10
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "0a\r\n"u8 + "Body here\n\r\n"u8 + "09\r\n"u8 + "continued\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "13\r\n"u8 + "Body here\ncontinued\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: -1,
            TransferEncoding: new @string[]{"chunked"u8}.slice()
        ),
        "Body here\ncontinued"u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Trailer: Content-MD5, Content-Sources\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 10\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"u8] = new @string[]{"close"u8}.slice(),
                ["Content-Length"u8] = new @string[]{"10"u8}.slice(),
                ["Trailer"u8] = new @string[]{"Content-MD5, Content-Sources"u8}.slice()
            }),
            Close: true,
            ContentLength: 10
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Content-Length: 10\r\n"u8 + "\r\n"u8 + "0a\r\n"u8 + "Body here\n\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8 + "a\r\n"u8 + "Body here\n\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: -1,
            TransferEncoding: new @string[]{"chunked"u8}.slice()
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            TransferEncoding: new @string[]{"chunked"u8}.slice(),
            Close: false,
            ContentLength: -1
        ),
        ""u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("HEAD"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{["Content-Length"u8] = new @string[]{"256"u8}.slice()}),
            TransferEncoding: default!,
            Close: true,
            ContentLength: 256
        ),
        ""u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{["Content-Length"u8] = new @string[]{"256"u8}.slice()}),
            TransferEncoding: default!,
            Close: false,
            ContentLength: 256
        ),
        ""u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "\r\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("HEAD"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            TransferEncoding: default!,
            Close: true,
            ContentLength: -1
        ),
        ""u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"u8] = new @string[]{"0"u8}.slice()
            }),
            Close: false,
            ContentLength: 0
        ),
        ""u8
    ),
    new(
        "HTTP/1.0 303 \r\n\r\n"u8,
        "HTTP/1.0 303 \r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "303 "u8,
            StatusCode: 303,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: -1
        ),
        ""u8
    ),
    new(
        "HTTP/1.0 303\r\n\r\n"u8,
        "HTTP/1.0 303 303\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "303"u8,
            StatusCode: 303,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: -1
        ),
        ""u8
    ),
    new(
        """
HTTP/1.1 206 Partial Content
Connection: close
Content-Type: multipart/byteranges; boundary=18a75608c8f47cef

some body
"""u8,
        "HTTP/1.1 206 Partial Content\r\n"u8 + "Connection: close\r\n"u8 + "Content-Type: multipart/byteranges; boundary=18a75608c8f47cef\r\n"u8 + "\r\n"u8 + "some body"u8,
        new Response(
            Status: "206 Partial Content"u8,
            StatusCode: 206,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Type"u8] = new @string[]{"multipart/byteranges; boundary=18a75608c8f47cef"u8}.slice()
            }),
            Close: true,
            ContentLength: -1
        ),
        "some body"u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"u8] = new @string[]{"close"u8}.slice()
            }),
            Close: true,
            ContentLength: -1
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 206 Partial Content\r\n"u8 + "Content-Type: text/plain; charset=utf-8\r\n"u8 + "Accept-Ranges: bytes\r\n"u8 + "Content-Range: bytes 0-5/1862\r\n"u8 + "Content-Length: 6\r\n\r\n"u8 + "foobar"u8,
        "HTTP/1.1 206 Partial Content\r\n"u8 + "Content-Length: 6\r\n"u8 + "Accept-Ranges: bytes\r\n"u8 + "Content-Range: bytes 0-5/1862\r\n"u8 + "Content-Type: text/plain; charset=utf-8\r\n"u8 + "\r\n"u8 + "foobar"u8,
        new Response(
            Status: "206 Partial Content"u8,
            StatusCode: 206,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Accept-Ranges"u8] = new @string[]{"bytes"u8}.slice(),
                ["Content-Length"u8] = new @string[]{"6"u8}.slice(),
                ["Content-Type"u8] = new @string[]{"text/plain; charset=utf-8"u8}.slice(),
                ["Content-Range"u8] = new @string[]{"bytes 0-5/1862"u8}.slice()
            }),
            ContentLength: 6
        ),
        "foobar"u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "Connection: keep-alive, close\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"u8] = new @string[]{"256"u8}.slice()
            }),
            TransferEncoding: default!,
            Close: true,
            ContentLength: 256
        ),
        ""u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 256\r\n"u8 + "Connection: keep-alive\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8,
        "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 256\r\n"u8 + "\r\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("HEAD"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"u8] = new @string[]{"256"u8}.slice()
            }),
            TransferEncoding: default!,
            Close: true,
            ContentLength: 256
        ),
        ""u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Transfer-Encoding: bogus\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: true,
            ContentLength: -1
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.0 200 OK\r\n"u8 + "Transfer-Encoding: bogus\r\n"u8 + "Content-Length: 10\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        "HTTP/1.0 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Content-Length: 10\r\n"u8 + "\r\n"u8 + "Body here\n"u8,
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"u8] = new @string[]{"10"u8}.slice()
            }),
            Close: true,
            ContentLength: 10
        ),
        "Body here\n"u8
    ),
    new(
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Encoding: gzip\r\n"u8 + "Content-Length: 23\r\n"u8 + "Connection: keep-alive\r\n"u8 + "Keep-Alive: timeout=7200\r\n\r\n"u8 + ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00})),
        "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 23\r\n"u8 + "Connection: keep-alive\r\n"u8 + "Content-Encoding: gzip\r\n"u8 + "Keep-Alive: timeout=7200\r\n\r\n"u8 + ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00})),
        new Response(
            Status: "200 OK"u8,
            StatusCode: 200,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Length"u8] = new @string[]{"23"u8}.slice(),
                ["Content-Encoding"u8] = new @string[]{"gzip"u8}.slice(),
                ["Connection"u8] = new @string[]{"keep-alive"u8}.slice(),
                ["Keep-Alive"u8] = new @string[]{"timeout=7200"u8}.slice()
            }),
            Close: false,
            ContentLength: 23
        ),
        ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00}))
    ),
    new(
        "HTTP/1.0  401 Unauthorized\r\n"u8 + "Content-type: text/html\r\n"u8 + "WWW-Authenticate: Basic realm=\"\"\r\n\r\n"u8 + "Your Authentication failed.\r\n"u8,
        "HTTP/1.0 401 Unauthorized\r\n"u8 + "Connection: close\r\n"u8 + "Content-Type: text/html\r\n"u8 + "Www-Authenticate: Basic realm=\"\"\r\n"u8 + "\r\n"u8 + "Your Authentication failed.\r\n"u8,
        new Response(
            Status: "401 Unauthorized"u8,
            StatusCode: 401,
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ProtoMinor: 0,
            Request: dummyReq("GET"u8),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Content-Type"u8] = new @string[]{"text/html"u8}.slice(),
                ["Www-Authenticate"u8] = new @string[]{@"Basic realm="""""u8}.slice()
            }),
            Close: true,
            ContentLength: -1
        ),
        "Your Authentication failed.\r\n"u8
    )
}.slice();

// tests successful calls to ReadResponse, and inspects the returned Response.
// For error cases, see TestReadResponseErrors below.
public static void TestReadResponse(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in respTests) {
        ref var tt = ref heap(new respTest(), out var Ꮡtt);
        tt = vᴛ1;

        var (resp, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.Raw))), tt.Resp.Request);
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, i, err);
            continue;
        }
        var rbody = resp.Value.Body;
        resp.Value.Body = default!;
        diff(Ꮡt, fmt.Sprintf("#%d Response"u8, i), resp.OrTypedNil(), Ꮡtt.of(respTest.ᏑResp));
        ref var bout = ref heap(new strings.Builder(), out var Ꮡbout);
        if (rbody != default!) {
            (_, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbout), rbody);
            if (err != default!) {
                Ꮡt.Errorf("#%d: %v"u8, i, err);
                continue;
            }
            rbody.Close();
        }
        @string body = bout.String();
        if (body != tt.Body) {
            Ꮡt.Errorf("#%d: Body = %q want %q"u8, i, body, tt.Body);
        }
    }
}

public static void TestWriteResponse(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in respTests) {
        var (resp, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.Raw))), tt.Resp.Request);
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, i, err);
            continue;
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = resp.Write(new http_test_package.bytes_BufferжWriter(Ꮡbuf));
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, i, err);
            continue;
        }
        {
            @string got = Ꮡbuf.String();
            @string want = tt.RawOut; if (got != want) {
                Ꮡt.Errorf("#%d: response differs; got:\n----\n%v\n----\nwant:\n----\n%v\n----\n"u8,
                    i,
                    strings.ReplaceAll(got, "\r"u8, "\\r"u8),
                    strings.ReplaceAll(want, "\r"u8, "\\r"u8));
            }
        }
    }
}


[GoType("dyn")] partial struct readResponseCloseInMiddleTestsᴛ1 {
    internal bool chunked, compressed;
}
internal static slice<readResponseCloseInMiddleTestsᴛ1> readResponseCloseInMiddleTests = new readResponseCloseInMiddleTestsᴛ1[]{
    new(false, false),
    new(true, false),
    new(true, true)
}.slice();

[GoType] internal partial struct readerAndCloser {
    public io_package.Reader Reader;
    public io_package.Closer Closer;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http11200Okˢ = "HTTP/1.1 200 OK\r\n"u8;
internal static readonly @string contentLength1000000ˢ = "Content-Length: 1000000\r\n"u8;
internal static readonly @string contentEncodingGzipˢ = "Content-Encoding: gzip\r\n"u8;
internal static readonly @string randReaderReadFullˢ = "rand.Reader ReadFull"u8;
internal static readonly @string compressorCloseˢ = "compressor close"u8;
internal static readonly @string nextRequestHereˢ = "Next Request Here"u8;
internal static readonly @string readResponseˢ = "ReadResponse"u8;
internal static readonly @string gzipNewReaderˢ = "gzip.NewReader"u8;
internal static readonly @string byteReadFullˢ = "2500 byte ReadFull"u8;
internal static readonly @string readAllOnRemainderˢ = "ReadAll on remainder"u8;

// TestReadResponseCloseInMiddle tests that closing a body after
// reading only part of its contents advances the read to the end of
// the request, right up until the next request.
public static void TestReadResponseCloseInMiddle(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    foreach (var (_, vᴛ1) in readResponseCloseInMiddleTests) {
        ref var test = ref heap(new readResponseCloseInMiddleTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        void fatalf(@string format, params ꓸꓸꓸany argsʗp) {
            var args = argsʗp.slice();
            args = appendꓸꓸꓸ(new any[]{testʗ1.chunked, testʗ1.compressed}.slice(), args);
            Ꮡt.Fatalf("on test chunked=%v, compressed=%v: "u8 + format, args.ꓸꓸꓸ);
        }
        var fatalfʗ1 = fatalf;
        void checkErr(error errΔ1, @string msg) {
            if (errΔ1 == default!) {
                return;
            }
            fatalfʗ1(msg + ": %v"u8, errΔ1);
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        buf.WriteString(http11200Okˢ);
        if (test.chunked){
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
        buf.WriteString("\r\n"u8);
        var chunk = bytes.Repeat(new byte[]{(rune)'x'}.slice(), 1000);
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
            buf.WriteString("0\r\n\r\n"u8);
        }
        buf.WriteString(nextRequestHereˢ);
        var bufr = bufio.NewReader(new http_test_package.bytes_BufferжReader(Ꮡbuf));
        var (resp, err) = ReadResponse(bufr, dummyReq(getˢ));
        checkErr(err, readResponseˢ);
        var expectedLength = (int64)(-1);
        if (!test.chunked) {
            expectedLength = 1000000;
        }
        if ((~resp).ContentLength != expectedLength) {
            fatalf("expected response length %d, got %d"u8, expectedLength, (~resp).ContentLength);
        }
        if ((~resp).Body == default!) {
            fatalf("nil body"u8);
        }
        if (test.compressed) {
            var (gzReader, errΔ1) = gzip.NewReader((~resp).Body);
            checkErr(errΔ1, gzipNewReaderˢ);
            resp.Value.Body = new http_internal_test_package.readerAndCloserжReadCloser(Ꮡ(new readerAndCloser(new http_test_package.gzip_ReaderжReader(gzReader), (~resp).Body)));
        }
        var rbuf = new slice<byte>(2500);
        (var n, err) = io.ReadFull((~resp).Body, rbuf);
        checkErr(err, byteReadFullˢ);
        if (n != 2500) {
            fatalf("ReadFull only read %d bytes"u8, n);
        }
        if (test.compressed == false && !bytes.Equal(bytes.Repeat(new byte[]{(rune)'x'}.slice(), 2500), rbuf)) {
            fatalf("ReadFull didn't read 2500 'x'; got %q"u8, ((@string)rbuf));
        }
        (~resp).Body.Close();
        (var rest, err) = io.ReadAll(new http_test_package.bufio_ReaderжReader(bufr));
        checkErr(err, readAllOnRemainderˢ);
        {
            @string e = nextRequestHereˢ;
            @string g = ((@string)rest); if (e != g) {
                g = regexp.MustCompile(@"(xx+)"u8).ReplaceAllStringFunc(g, (@string match) => fmt.Sprintf("x(repeated x%d)"u8, builtin.len(match)));
                fatalf("remainder = %q, expected %q"u8, g, e);
            }
        }
    }
}

internal static void diff(ж<testing.T> Ꮡt, @string prefix, any have, any want) {
    Ꮡt.Helper();
    var hv = reflect.ValueOf(have).Elem();
    var wv = reflect.ValueOf(want).Elem();
    if (!AreEqual(hv.Type(), wv.Type())) {
        Ꮡt.Errorf("%s: type mismatch %v want %v"u8, prefix, hv.Type(), wv.Type());
    }
    for (nint i = 0; i < hv.NumField(); i++) {
        @string name = hv.Type().Field(i).Name;
        if (!token.IsExported(name)) {
            continue;
        }
        var hf = hv.Field(i).Interface();
        var wf = wv.Field(i).Interface();
        if (!reflect.DeepEqual(hf, wf)) {
            Ꮡt.Errorf("%s: %s = %v want %v"u8, prefix, name, hf, wf);
        }
    }
}

[GoType] internal partial struct responseLocationTest {
    internal @string location; // Response's Location header or ""
    internal @string requrl; // Response.Request.URL or ""
    internal @string want;
    internal error wantErr;
}

internal static slice<responseLocationTest> responseLocationTests;
internal static void initᴛresponseLocationTests() { responseLocationTests = new responseLocationTest[]{
    new("/foo"u8, "http://bar.com/baz"u8, "http://bar.com/foo"u8, default!),
    new("http://foo.com/"u8, "http://bar.com/baz"u8, "http://foo.com/"u8, default!),
    new(""u8, "http://bar.com/baz"u8, ""u8, ErrNoLocation),
    new("/bar"u8, ""u8, "/bar"u8, default!)
}.slice(); }

public static void TestLocationResponse(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in responseLocationTests) {
        var res = @new<global::go.net.http_package.Response>();
        res.Value.Header = new global::go.net.http_package.ΔHeader(0);
        (~res).Header.Set(locationˢ, tt.location);
        if (tt.requrl != ""u8) {
            res.Value.Request = Ꮡ(new Request(nil));
            error errΔ1 = default!;
            (res.Value.Request.Value.URL, errΔ1) = url.Parse(tt.requrl);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("bad test URL %q: %v"u8, tt.requrl, errΔ1);
            }
        }
        var (got, err) = res.Location();
        if (tt.wantErr != default!) {
            if (err == default!) {
                Ꮡt.Errorf("%d. err=nil; want %q"u8, i, tt.wantErr);
                continue;
            }
            {
                @string g = err.Error();
                @string e = tt.wantErr.Error(); if (g != e) {
                    Ꮡt.Errorf("%d. err=%q; want %q"u8, i, g, e);
                    continue;
                }
            }
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("%d. err=%q"u8, i, err);
            continue;
        }
        {
            @string g = got.String();
            @string e = tt.want; if (g != e) {
                Ꮡt.Errorf("%d. Location=%q; want %q"u8, i, g, e);
            }
        }
    }
}

public static void TestResponseStatusStutter(ж<testing.T> Ꮡt) {
    var r = Ꮡ(new Response(
        Status: "123 some status"u8,
        StatusCode: 123,
        ProtoMajor: 1,
        ProtoMinor: 3
    ));
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    r.Write(new http_test_package.strings_BuilderжWriter(Ꮡbuf));
    if (strings.Contains(buf.String(), "123 123"u8)) {
        Ꮡt.Errorf("stutter in status: %s"u8, buf.String());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string shortBodyᶜ = "Short body, not 123 bytes."u8;

public static void TestResponseContentLengthShortBody(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string shortBody = shortBodyᶜ;
        var br = bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader("HTTP/1.1 200 OK\r\n" + "Content-Length: 123\r\n" + "\r\n" + shortBody)));
        var (res, err) = ReadResponse(br, Ꮡ(new Request(Method: "GET"u8)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).ContentLength != 123) {
            Ꮡt.Fatalf("Content-Length = %d; want 123"u8, (~res).ContentLength);
        }
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        (var n, err) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbuf), (~res).Body);
        if (n != (int64)builtin.len(shortBody)) {
            Ꮡt.Errorf("Copied %d bytes; want %d, len(%q)"u8, n, builtin.len(shortBody), shortBody);
        }
        if (buf.String() != shortBody) {
            Ꮡt.Errorf("Read body %q; want %q"u8, buf.String(), shortBody);
        }
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            Ꮡt.Errorf("io.Copy error = %#v; want io.ErrUnexpectedEOF"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string messageCannotContainˢ = "message cannot contain multiple Content-Length headers"u8;
internal static readonly @string unknownˢ = "20X Unknown"u8;
internal static readonly @string abcdUnknownˢ = "abcd Unknown"u8;
internal static readonly @string unknownˢ2 = " Unknown"u8;
internal static readonly @string c8Okˢ = "c8 OK"u8;
internal static readonly @string movedPermanentlyˢ = "0x12d Moved Permanently"u8;
internal static readonly @string notfoundˢ = "404 NOTFOUND"u8;
internal static readonly @string doneˢ = "999 Done"u8;
internal static readonly @string http12ˢ = "HTTP/1.2"u8;
internal static readonly @string http20ˢ = "HTTP/2.0"u8;
internal static readonly @string http1100000000002ˢ = "HTTP/1.100000000002"u8;
internal static readonly @string http11ˢ2 = "HTTP/1.-1"u8;
internal static readonly @string httpABˢ = "HTTP/A.B"u8;
internal static readonly @string http1ˢ = "HTTP/1"u8;
internal static readonly @string contentLength10Contentˢ = "Content-Length: 10\r\nContent-Length: 7\r\n\r\nGopher hey\r\n"u8;
internal static readonly @string contentLength7Contentˢ = "Content-Length: 7\r\nContent-Length: 7\r\n\r\nGophers\r\n"u8;
internal static readonly @string contentLength0Contentˢ = "Content-Length: 0\r\nContent-Length: 7\r\n\r\nGophers\r\n"u8;
internal static readonly @string contentLength0Contentˢ2 = "Content-Length: 0\r\nContent-Length: 0 \r\n\r\nGophers\r\n"u8;
internal static readonly @string contentLengthContentˢ = "Content-Length:\r\nContent-Length:\r\n\r\nGophers\r\n"u8;
internal static readonly @string contentLengthContentˢ2 = "Content-Length:\r\nContent-Length: 0 \r\nConnection: close\r\n\r\nGophers\r\n"u8;
internal static readonly @string contentLength7Contentˢ2 = "Content-Length: 7\r\nContent-Length: 8\r\n\r\n"u8;
internal static readonly @string contentLength3Contentˢ = "Content-Length: 3\r\nContent-Length: 3\r\n\r\n"u8;
internal static readonly @string contentLength880Contentˢ = "Content-Length: 880\r\nContent-Length: 1\r\n\r\n"u8;
internal static readonly @string contentLength961Contentˢ = "Content-Length: 961\r\nContent-Length: 961\r\n\r\n"u8;

[GoType("dyn")] [GoLocalName("testCase")] internal partial struct TestReadResponseErrors_testCase {
    internal @string name; // optional, defaults to in
    internal @string @in;
    internal any wantErr; // nil, err value, bool value, or string substring
}

// Test various ReadResponse error cases. (also tests success cases, but mostly
// it's about errors).  This does not test anything involving the bodies. Only
// the return value from ReadResponse itself.
public static void TestReadResponseErrors(ж<testing.T> Ꮡt) {
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
    var tests = new TestReadResponseErrors_testCase[]{
        new(""u8, ""u8, io.ErrUnexpectedEOF),
        new(""u8, "HTTP/1.1 301 Moved Permanently\r\nFoo: bar"u8, io.ErrUnexpectedEOF),
        new(""u8, "HTTP/1.1"u8, (@string)"malformed HTTP response"u8),
        new(""u8, "HTTP/2.0"u8, (@string)"malformed HTTP response"u8),
        status(unknownˢ, true),
        status(abcdUnknownˢ, true),
        status("二百/两百 OK"u8, true),
        status(unknownˢ2, true),
        status(c8Okˢ, true),
        status(movedPermanentlyˢ, true),
        status("200 OK"u8, default!),
        status("000 OK"u8, default!),
        status("001 OK"u8, default!),
        status(notfoundˢ, default!),
        status("20 OK"u8, true),
        status("00 OK"u8, true),
        status("-10 OK"u8, true),
        status("1000 OK"u8, true),
        status(doneˢ, default!),
        status("-1 OK"u8, true),
        status("-200 OK"u8, true),
        version(http12ˢ, default!),
        version(http20ˢ, default!),
        version(http1100000000002ˢ, true),
        version(http11ˢ2, true),
        version(httpABˢ, true),
        version(http1ˢ, true),
        version(http11ˢ, true),
        contentLength("200 OK"u8, contentLength10Contentˢ, errMultiCL),
        contentLength("200 OK"u8, contentLength7Contentˢ, default!),
        contentLength("201 OK"u8, contentLength0Contentˢ, errMultiCL),
        contentLength("300 OK"u8, contentLength0Contentˢ2, default!),
        contentLength("200 OK"u8, contentLengthContentˢ, errEmptyCL),
        contentLength("206 OK"u8, contentLengthContentˢ2, errMultiCL), // multiple content-length headers for 204 and 304 should still be checked

        contentLength("204 OK"u8, contentLength7Contentˢ2, errMultiCL),
        contentLength("204 OK"u8, contentLength3Contentˢ, default!),
        contentLength("304 OK"u8, contentLength880Contentˢ, errMultiCL),
        contentLength("304 OK"u8, contentLength961Contentˢ, default!), // golang.org/issue/22464

        new("leading space in header"u8, "HTTP/1.1 200 OK\r\n Content-type: text/html\r\nFoo: bar\r\n\r\n"u8, (@string)"malformed MIME"u8),
        new("leading tab in header"u8, "HTTP/1.1 200 OK\r\n\tContent-type: text/html\r\nFoo: bar\r\n\r\n"u8, (@string)"malformed MIME"u8)
    }.slice();
    foreach (var (i, tt) in tests) {
        var br = bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)));
        var (_, rerr) = ReadResponse(br, nil);
        {
            var err = matchErr(rerr, tt.wantErr); if (err != default!) {
                @string name = tt.name;
                if (name == ""u8) {
                    name = fmt.Sprintf("%d. input %q"u8, i, tt.@in);
                }
                Ꮡt.Errorf("%s: %v"u8, name, err);
            }
        }
    }
}

// wantErr can be nil, an error value to match exactly, or type string to
// match a substring.
internal static error matchErr(error err, any wantErr) {
    if (err == default!) {
        if (wantErr == default!) {
            return default!;
        }
        {
            var (sub, ok) = wantErr._<@string>(ᐧ); if (ok) {
                return fmt.Errorf("unexpected success; want error with substring %q"u8, sub);
            }
        }
        return fmt.Errorf("unexpected success; want error %v"u8, wantErr);
    }
    if (wantErr == default!) {
        return fmt.Errorf("%v; want success"u8, err);
    }
    {
        var (sub, ok) = wantErr._<@string>(ᐧ); if (ok) {
            if (strings.Contains(err.Error(), sub)) {
                return default!;
            }
            return fmt.Errorf("error = %v; want an error with substring %q"u8, err, sub);
        }
    }
    if (AreEqual(err, wantErr)) {
        return default!;
    }
    return fmt.Errorf("%v; want %v"u8, err, wantErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string http10200OkAaaaˢ = "HTTP/1.0 200 OK\r\n\r\nAAAA"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string connectionCloseHeaderᶜ = "Connection: close"u8;

// A response should only write out single Connection: close header. Tests #19499.
public static void TestResponseWritesOnlySingleConnectionClose(ж<testing.T> Ꮡt) {
    @string connectionCloseHeader = connectionCloseHeaderᶜ;
    var (res, err) = ReadResponse(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(http10200OkAaaaˢ))), nil);
    if (err != default!) {
        Ꮡt.Fatalf("ReadResponse failed %v"u8, err);
    }
    ref var buf1 = ref heap(new bytes.Buffer(), out var Ꮡbuf1);
    {
        err = res.Write(new http_test_package.bytes_BufferжWriter(Ꮡbuf1)); if (err != default!) {
            Ꮡt.Fatalf("Write failed %v"u8, err);
        }
    }
    {
        (res, err) = ReadResponse(bufio.NewReader(new http_test_package.bytes_BufferжReader(Ꮡbuf1)), nil); if (err != default!) {
            Ꮡt.Fatalf("ReadResponse failed %v"u8, err);
        }
    }
    ref var buf2 = ref heap(new strings.Builder(), out var Ꮡbuf2);
    {
        err = res.Write(new http_test_package.strings_BuilderжWriter(Ꮡbuf2)); if (err != default!) {
            Ꮡt.Fatalf("Write failed %v"u8, err);
        }
    }
    {
        nint count = strings.Count(buf2.String(), connectionCloseHeader); if (count != 1) {
            Ꮡt.Errorf("Found %d %q header"u8, count, connectionCloseHeader);
        }
    }
}

} // end http_internal_test_package
