// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using url = global::go.net.url_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using global::go.net;
using static global::go.net.http_package;

partial class http_internal_test_package {

internal partial struct reqTest {
    public @string Raw;
    public ж<global::go.net.http_package.Request> Req;
    public @string Body;
    public global::go.net.http_package.ΔHeader Trailer;
    public @string Error;
}

internal static @string noError = ""u8;

internal static @string noBodyStr = ""u8;

internal static global::go.net.http_package.ΔHeader noTrailer = default!;

// Baseline test; All Request fields included for template use
// GET request with no body (the normal case)
// Tests that we don't parse a path that looks like a
// scheme-relative URI as a scheme-relative URI.
// Tests a bogus absolute-path on the Request-Line (RFC 7230 section 5.3.1)
// Tests missing URL:
// Tests chunked body with trailer:
// Tests chunked body and a bogus Content-Length which should be deleted.
// to be removed.
// Tests chunked body and an invalid Content-Length.
// raise an error
// CONNECT request with domain name:
// CONNECT request with IP address:
// CONNECT request for RPC:
// SSDP Notify request. golang.org/issue/3692
// OPTIONS request. Similar to golang.org/issue/3692
// Connection: close. golang.org/issue/8261
// This wasn't removed from Go 1.0 to
// Go 1.3, so locking it in that we
// keep this:
// HEAD with Content-Length 0. Make sure this is permitted,
// since I think we used to send it.
// http2 client preface:
internal static ж<slice<reqTest>> ᏑreqTests = new StandardBox<slice<reqTest>>(new reqTest[]{
    new(
        "GET http://www.techcrunch.com/ HTTP/1.1\r\n"u8 + "Host: www.techcrunch.com\r\n"u8 + "User-Agent: Fake\r\n"u8 + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n"u8 + "Accept-Language: en-us,en;q=0.5\r\n"u8 + "Accept-Encoding: gzip,deflate\r\n"u8 + "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7\r\n"u8 + "Keep-Alive: 300\r\n"u8 + "Content-Length: 7\r\n"u8 + "Proxy-Connection: keep-alive\r\n\r\n"u8 + "abcdef\n???"u8,
        Ꮡ(new Request(
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
                ["Accept-Language"u8] = new @string[]{"en-us,en;q=0.5"u8}.slice(),
                ["Accept-Encoding"u8] = new @string[]{"gzip,deflate"u8}.slice(),
                ["Accept-Charset"u8] = new @string[]{"ISO-8859-1,utf-8;q=0.7,*;q=0.7"u8}.slice(),
                ["Keep-Alive"u8] = new @string[]{"300"u8}.slice(),
                ["Proxy-Connection"u8] = new @string[]{"keep-alive"u8}.slice(),
                ["Content-Length"u8] = new @string[]{"7"u8}.slice(),
                ["User-Agent"u8] = new @string[]{"Fake"u8}.slice()
            }),
            Close: false,
            ContentLength: 7,
            Host: "www.techcrunch.com"u8,
            RequestURI: "http://www.techcrunch.com/"u8
        )),
        "abcdef\n"u8,
        noTrailer,
        noError
    ),
    new(
        "GET / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Path: "/"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "foo.com"u8,
            RequestURI: "/"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "GET //user@host/is/actually/a/path/ HTTP/1.1\r\n"u8 + "Host: test\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Path: "//user@host/is/actually/a/path/"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "test"u8,
            RequestURI: "//user@host/is/actually/a/path/"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "GET ../../../../etc/passwd HTTP/1.1\r\n"u8 + "Host: test\r\n\r\n"u8,
        nil,
        noBodyStr,
        noTrailer,
        @"parse ""../../../../etc/passwd"": invalid URI for request"u8
    ),
    new(
        "GET  HTTP/1.1\r\n"u8 + "Host: test\r\n\r\n"u8,
        nil,
        noBodyStr,
        noTrailer,
        @"parse """": empty url"u8
    ),
    new(
        "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + "3\r\nfoo\r\n"u8 + "3\r\nbar\r\n"u8 + "0\r\n"u8 + "Trailer-Key: Trailer-Value\r\n"u8 + "\r\n"u8,
        Ꮡ(new Request(
            Method: "POST"u8,
            URL: Ꮡ(new url.URL(
                Path: "/"u8
            )),
            TransferEncoding: new @string[]{"chunked"u8}.slice(),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            ContentLength: -1,
            Host: "foo.com"u8,
            RequestURI: "/"u8
        )),
        "foobar"u8,
        new ΔHeader(new map<@string, slice<@string>>{
            ["Trailer-Key"u8] = new @string[]{"Trailer-Value"u8}.slice()
        }),
        noError
    ),
    new(
        "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Content-Length: 9999\r\n\r\n"u8 + "3\r\nfoo\r\n"u8 + "3\r\nbar\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        Ꮡ(new Request(
            Method: "POST"u8,
            URL: Ꮡ(new url.URL(
                Path: "/"u8
            )),
            TransferEncoding: new @string[]{"chunked"u8}.slice(),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            ContentLength: -1,
            Host: "foo.com"u8,
            RequestURI: "/"u8
        )),
        "foobar"u8,
        noTrailer,
        noError
    ),
    new(
        "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Content-Length: notdigits\r\n\r\n"u8 + "3\r\nfoo\r\n"u8 + "3\r\nbar\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        nil,
        noBodyStr,
        noTrailer,
        @"bad Content-Length ""notdigits"""u8
    ),
    new(
        "CONNECT www.google.com:443 HTTP/1.1\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "CONNECT"u8,
            URL: Ꮡ(new url.URL(
                Host: "www.google.com:443"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "www.google.com:443"u8,
            RequestURI: "www.google.com:443"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "CONNECT 127.0.0.1:6060 HTTP/1.1\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "CONNECT"u8,
            URL: Ꮡ(new url.URL(
                Host: "127.0.0.1:6060"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "127.0.0.1:6060"u8,
            RequestURI: "127.0.0.1:6060"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "CONNECT /_goRPC_ HTTP/1.1\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "CONNECT"u8,
            URL: Ꮡ(new url.URL(
                Path: "/_goRPC_"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: ""u8,
            RequestURI: "/_goRPC_"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "NOTIFY * HTTP/1.1\r\nServer: foo\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "NOTIFY"u8,
            URL: Ꮡ(new url.URL(
                Path: "*"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Server"u8] = new @string[]{"foo"u8}.slice()
            }),
            Close: false,
            ContentLength: 0,
            RequestURI: "*"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "OPTIONS * HTTP/1.1\r\nServer: foo\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "OPTIONS"u8,
            URL: Ꮡ(new url.URL(
                Path: "*"u8
            )),
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Server"u8] = new @string[]{"foo"u8}.slice()
            }),
            Close: false,
            ContentLength: 0,
            RequestURI: "*"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "GET / HTTP/1.1\r\nHost: issue8261.com\r\nConnection: close\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "GET"u8,
            URL: Ꮡ(new url.URL(
                Path: "/"u8
            )),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"u8] = new @string[]{"close"u8}.slice()
            }),
            Host: "issue8261.com"u8,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Close: true,
            RequestURI: "/"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "HEAD / HTTP/1.1\r\nHost: issue8261.com\r\nConnection: close\r\nContent-Length: 0\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "HEAD"u8,
            URL: Ꮡ(new url.URL(
                Path: "/"u8
            )),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"u8] = new @string[]{"close"u8}.slice(),
                ["Content-Length"u8] = new @string[]{"0"u8}.slice()
            }),
            Host: "issue8261.com"u8,
            Proto: "HTTP/1.1"u8,
            ProtoMajor: 1,
            ProtoMinor: 1,
            Close: true,
            RequestURI: "/"u8
        )),
        noBodyStr,
        noTrailer,
        noError
    ),
    new(
        "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8,
        Ꮡ(new Request(
            Method: "PRI"u8,
            URL: Ꮡ(new url.URL(
                Path: "*"u8
            )),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Proto: "HTTP/2.0"u8,
            ProtoMajor: 2,
            ProtoMinor: 0,
            RequestURI: "*"u8,
            ContentLength: -1,
            Close: true
        )),
        noBodyStr,
        noTrailer,
        noError
    )
}.slice());
internal static ref slice<reqTest> reqTests => ref ᏑreqTests.ValueSlot;

public static void TestReadRequest(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in reqTests) {
        var tt = Ꮡ(reqTests, i);
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader((~tt).Raw))));
        if (err != default!) {
            if (err.Error() != (~tt).Error) {
                Ꮡt.Errorf("#%d: error %q, want error %q"u8, i, err.Error(), (~tt).Error);
            }
            continue;
        }
        var rbody = req.Value.Body;
        req.Value.Body = default!;
        @string testName = fmt.Sprintf("Test %d (%q)"u8, i, (~tt).Raw);
        diff(Ꮡt, testName, req.OrTypedNil(), (~tt).Req.OrTypedNil());
        ref var bout = ref heap(new strings.Builder(), out var Ꮡbout);
        if (rbody != default!) {
            var (_, errΔ1) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbout), rbody);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("%s: copying body: %v"u8, testName, errΔ1);
            }
            rbody.Close();
        }
        @string body = bout.String();
        if (body != (~tt).Body) {
            Ꮡt.Errorf("%s: Body = %q want %q"u8, testName, body, (~tt).Body);
        }
        if (!reflect.DeepEqual((~tt).Trailer, (~req).Trailer)) {
            Ꮡt.Errorf("%s: Trailers differ.\n got: %v\nwant: %v"u8, testName, (~req).Trailer, (~tt).Trailer);
        }
    }
}

// reqBytes treats req as a request (with \n delimiters) and returns it with \r\n delimiters,
// ending in \r\n\r\n
internal static slice<byte> reqBytes(@string req) {
    return slice<byte>(strings.ReplaceAll(strings.TrimSpace(req), "\n"u8, "\r\n"u8) + "\r\n\r\n");
}

// golang.org/issue/22464

partial struct badRequestTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal slice<byte> req;
}
internal static slice<badRequestTestsᴛ1> badRequestTests = new badRequestTestsᴛ1[]{
    new("bad_connect_host"u8, reqBytes("CONNECT []%20%48%54%54%50%2f%31%2e%31%0a%4d%79%48%65%61%64%65%72%3a%20%31%32%33%0a%0a HTTP/1.0"u8)),
    new("smuggle_two_contentlen"u8, reqBytes("""
POST / HTTP/1.1
Content-Length: 3
Content-Length: 4

abc
"""u8)),
    new("smuggle_two_content_len_head"u8, reqBytes("""
HEAD / HTTP/1.1
Host: foo
Content-Length: 4
Content-Length: 5

1234
"""u8)),
    new("leading_space_in_header"u8, reqBytes("""
GET / HTTP/1.1
 Host: foo
"""u8)),
    new("leading_tab_in_header"u8, reqBytes("""
GET / HTTP/1.1

"""u8 + "\t"u8 + @"Host: foo"u8))
}.slice();

public static void TestReadRequest_Bad(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in badRequestTests) {
        var (got, err) = ReadRequest(bufio.NewReader(new http_test_package.bytes_ReaderжReader(bytes.NewReader(tt.req))));
        if (err == default!) {
            var (all, errΔ1) = io.ReadAll((~got).Body);
            Ꮡt.Errorf("%s: got unexpected request = %#v\n  Body = %q, %v"u8, tt.name, got.OrTypedNil(), all, errΔ1);
        }
    }
}

} // end http_internal_test_package
