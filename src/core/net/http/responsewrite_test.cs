// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using io = io_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

internal partial struct respWriteTest {
    public global::go.net.http_package.Response Resp;
    public @string Raw;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcdefˢ => ref abcdefˢᵇ;
private static readonly @string abcdefˢᵇ = "abcdef"u8;

public static void TestResponseWrite(ж<testing.T> t) {
    var respWriteTests = slice<respWriteTest>([
        // HTTP/1.0, identity coding; no trailer
        new(
            new Response(
                StatusCode: 503,
                ProtoMajor: 1,
                ProtoMinor: 0,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ))),
                ContentLength: 6
            ),

            "HTTP/1.0 503 Service Unavailable\r\n"u8 + "Content-Length: 6\r\n\r\n"u8 + "abcdef"u8
        ),
        // Unchunked response without Content-Length.
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 0,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ))),
                ContentLength: -1
            ),
            "HTTP/1.0 200 OK\r\n"u8 + "\r\n"u8 + "abcdef"u8
        ),
        // HTTP/1.1 response with unknown length and Connection: close
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ))),
                ContentLength: -1,
                Close: true
            ),
            "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "abcdef"u8
        ),
        // HTTP/1.1 response with unknown length and not setting connection: close
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq11(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ))),
                ContentLength: -1,
                Close: false
            ),
            "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\n"u8 + "abcdef"u8
        ),
        // HTTP/1.1 response with unknown length and not setting connection: close, but
        // setting chunked.
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq11(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ))),
                ContentLength: -1,
                TransferEncoding: slice<@string>(["chunked"]),
                Close: false
            ),
            "HTTP/1.1 200 OK\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + "6\r\nabcdef\r\n0\r\n\r\n"u8
        ),
        // HTTP/1.1 response 0 content-length, and nil body
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq11(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: default,
                ContentLength: 0,
                Close: false
            ),
            "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8
        ),
        // HTTP/1.1 response 0 content-length, and non-nil empty body
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq11(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8))),
                ContentLength: 0,
                Close: false
            ),
            "HTTP/1.1 200 OK\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\n"u8
        ),
        // HTTP/1.1 response 0 content-length, and non-nil non-empty body
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq11(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(fooˢ))),
                ContentLength: 0,
                Close: false
            ),
            "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "\r\nfoo"u8
        ),
        // HTTP/1.1, chunked coding; empty trailer; close
        new(
            new Response(
                StatusCode: 200,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ))),
                ContentLength: 6,
                TransferEncoding: slice<@string>(["chunked"]),
                Close: true
            ),

            "HTTP/1.1 200 OK\r\n"u8 + "Connection: close\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + "6\r\nabcdef\r\n0\r\n\r\n"u8
        ),

        // Header value with a newline character (Issue 914).
        // Also tests removal of leading and trailing whitespace.
        new(
            new Response(
                StatusCode: 204,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{
                    ["Foo"] = slice<@string>([" Bar\nBaz "])
                }),
                Body: default,
                ContentLength: 0,
                TransferEncoding: slice<@string>(["chunked"]),
                Close: true
            ),

            "HTTP/1.1 204 No Content\r\n"u8 + "Connection: close\r\n"u8 + "Foo: Bar Baz\r\n"u8 + "\r\n"u8
        ),

        // Want a single Content-Length header. Fixing issue 8180 where
        // there were two.
        new(
            new Response(
                StatusCode: StatusOK,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: Ꮡ(new Request(Method: "POST")),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                ContentLength: 0,
                TransferEncoding: default,
                Body: default
            ),
            "HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n"
        ),

        // When a response to a POST has Content-Length: -1, make sure we don't
        // write the Content-Length as -1.
        new(
            new Response(
                StatusCode: StatusOK,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Request: Ꮡ(new Request(Method: "POST")),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                ContentLength: -1,
                Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ)))
            ),
            "HTTP/1.1 200 OK\r\nConnection: close\r\n\r\nabcdef"
        ),

        // Status code under 100 should be zero-padded to
        // three digits.  Still bogus, but less bogus. (be
        // consistent with generating three digits, since the
        // Transport requires it)
        new(
            new Response(
                StatusCode: 7,
                Status: "license to violate specs",
                ProtoMajor: 1,
                ProtoMinor: 0,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: default
            ),

            "HTTP/1.0 007 license to violate specs\r\nContent-Length: 0\r\n\r\n"
        ),

        // No stutter.  Status code in 1xx range response should
        // not include a Content-Length header.  See issue #16942.
        new(
            new Response(
                StatusCode: 123,
                Status: "123 Sesame Street",
                ProtoMajor: 1,
                ProtoMinor: 0,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: default
            ),

            "HTTP/1.0 123 Sesame Street\r\n\r\n"
        ),

        // Status code 204 (No content) response should not include a
        // Content-Length header.  See issue #16942.
        new(
            new Response(
                StatusCode: 204,
                Status: "No Content",
                ProtoMajor: 1,
                ProtoMinor: 0,
                Request: dummyReq(getˢ),
                Header: new ΔHeader(new map<@string, slice<@string>>{}),
                Body: default
            ),

            "HTTP/1.0 204 No Content\r\n\r\n"
        )
    ]);

    foreach (var i in range(respWriteTests)) {
        var tt = Ꮡ(respWriteTests, i);
        ref var braw = ref heap(new strings.Builder(), out var Ꮡbraw);
        var err = tt.ᏑResp.Write(new http_test_package.strings_BuilderжWriter(Ꮡbraw));
        if (err != nil) {
            t.Errorf("error writing #%d: %s", i, err);
            continue;
        }
        @string sraw = braw.String();
        if (sraw != tt.Raw) {
            t.Errorf("Test %d, expecting:\n%q\nGot:\n%q\n", i, tt.Raw, sraw);
            continue;
        }
    }
}

} // end http_internal_test_package
