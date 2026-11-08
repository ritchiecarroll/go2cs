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

internal static @string noError = "";
internal static @string noBodyStr = "";
internal static global::go.net.http_package.ΔHeader noTrailer = default;

internal static readonly ж<slice<reqTest>> ᏑreqTests = new StaticBox<slice<reqTest>>([
    // Baseline test; All Request fields included for template use
    new(
        "GET http://www.techcrunch.com/ HTTP/1.1\r\n"u8 + "Host: www.techcrunch.com\r\n"u8 + "User-Agent: Fake\r\n"u8 + "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n"u8 + "Accept-Language: en-us,en;q=0.5\r\n"u8 + "Accept-Encoding: gzip,deflate\r\n"u8 + "Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7\r\n"u8 + "Keep-Alive: 300\r\n"u8 + "Content-Length: 7\r\n"u8 + "Proxy-Connection: keep-alive\r\n\r\n"u8 + "abcdef\n???"u8,

        Ꮡ(new Request(
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
                ["Accept-Language"] = ["en-us,en;q=0.5"],
                ["Accept-Encoding"] = ["gzip,deflate"],
                ["Accept-Charset"] = ["ISO-8859-1,utf-8;q=0.7,*;q=0.7"],
                ["Keep-Alive"] = ["300"],
                ["Proxy-Connection"] = ["keep-alive"],
                ["Content-Length"] = ["7"],
                ["User-Agent"] = ["Fake"]
            }),
            Close: false,
            ContentLength: 7,
            Host: "www.techcrunch.com",
            RequestURI: "http://www.techcrunch.com/"
        )),

        "abcdef\n",

        noTrailer,
        noError
    ),

    // GET request with no body (the normal case)
    new(
        "GET / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n\r\n"u8,

        Ꮡ(new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Path: "/"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "foo.com",
            RequestURI: "/"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // Tests that we don't parse a path that looks like a
    // scheme-relative URI as a scheme-relative URI.
    new(
        "GET //user@host/is/actually/a/path/ HTTP/1.1\r\n"u8 + "Host: test\r\n\r\n"u8,

        Ꮡ(new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Path: "//user@host/is/actually/a/path/"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "test",
            RequestURI: "//user@host/is/actually/a/path/"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // Tests a bogus absolute-path on the Request-Line (RFC 7230 section 5.3.1)
    new(
        "GET ../../../../etc/passwd HTTP/1.1\r\n"u8 + "Host: test\r\n\r\n"u8,
        nil,
        noBodyStr,
        noTrailer,
        @"parse ""../../../../etc/passwd"": invalid URI for request"
    ),

    // Tests missing URL:
    new(
        "GET  HTTP/1.1\r\n"u8 + "Host: test\r\n\r\n"u8,
        nil,
        noBodyStr,
        noTrailer,
        @"parse """": empty url"
    ),

    // Tests chunked body with trailer:
    new(
        "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + "3\r\nfoo\r\n"u8 + "3\r\nbar\r\n"u8 + "0\r\n"u8 + "Trailer-Key: Trailer-Value\r\n"u8 + "\r\n"u8,
        Ꮡ(new Request(
            Method: "POST",
            URL: Ꮡ(new url.URL(
                Path: "/"
            )),
            TransferEncoding: ["chunked"],
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            ContentLength: -1,
            Host: "foo.com",
            RequestURI: "/"
        )),

        "foobar",
        new ΔHeader(new map<@string, slice<@string>>{
            ["Trailer-Key"] = ["Trailer-Value"]
        }),
        noError
    ),

    // Tests chunked body and a bogus Content-Length which should be deleted.
    new(
        "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Content-Length: 9999\r\n\r\n"u8 + // to be removed.
            "3\r\nfoo\r\n"u8 + "3\r\nbar\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        Ꮡ(new Request(
            Method: "POST",
            URL: Ꮡ(new url.URL(
                Path: "/"
            )),
            TransferEncoding: ["chunked"],
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            ContentLength: -1,
            Host: "foo.com",
            RequestURI: "/"
        )),

        "foobar",
        noTrailer,
        noError
    ),

    // Tests chunked body and an invalid Content-Length.
    new(
        "POST / HTTP/1.1\r\n"u8 + "Host: foo.com\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Content-Length: notdigits\r\n\r\n"u8 + // raise an error
            "3\r\nfoo\r\n"u8 + "3\r\nbar\r\n"u8 + "0\r\n"u8 + "\r\n"u8,
        nil,
        noBodyStr,
        noTrailer,
        @"bad Content-Length ""notdigits"""
    ),

    // CONNECT request with domain name:
    new(
        "CONNECT www.google.com:443 HTTP/1.1\r\n\r\n",

        Ꮡ(new Request(
            Method: "CONNECT",
            URL: Ꮡ(new url.URL(
                Host: "www.google.com:443"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "www.google.com:443",
            RequestURI: "www.google.com:443"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // CONNECT request with IP address:
    new(
        "CONNECT 127.0.0.1:6060 HTTP/1.1\r\n\r\n",

        Ꮡ(new Request(
            Method: "CONNECT",
            URL: Ꮡ(new url.URL(
                Host: "127.0.0.1:6060"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "127.0.0.1:6060",
            RequestURI: "127.0.0.1:6060"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // CONNECT request for RPC:
    new(
        "CONNECT /_goRPC_ HTTP/1.1\r\n\r\n",

        Ꮡ(new Request(
            Method: "CONNECT",
            URL: Ꮡ(new url.URL(
                Path: "/_goRPC_"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Close: false,
            ContentLength: 0,
            Host: "",
            RequestURI: "/_goRPC_"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // SSDP Notify request. golang.org/issue/3692
    new(
        "NOTIFY * HTTP/1.1\r\nServer: foo\r\n\r\n",
        Ꮡ(new Request(
            Method: "NOTIFY",
            URL: Ꮡ(new url.URL(
                Path: "*"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Server"] = ["foo"]
            }),
            Close: false,
            ContentLength: 0,
            RequestURI: "*"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // OPTIONS request. Similar to golang.org/issue/3692
    new(
        "OPTIONS * HTTP/1.1\r\nServer: foo\r\n\r\n",
        Ꮡ(new Request(
            Method: "OPTIONS",
            URL: Ꮡ(new url.URL(
                Path: "*"
            )),
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Server"] = ["foo"]
            }),
            Close: false,
            ContentLength: 0,
            RequestURI: "*"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // Connection: close. golang.org/issue/8261
    new(
        "GET / HTTP/1.1\r\nHost: issue8261.com\r\nConnection: close\r\n\r\n",
        Ꮡ(new Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Path: "/"
            )),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                // This wasn't removed from Go 1.0 to
                // Go 1.3, so locking it in that we
                // keep this:
                ["Connection"] = ["close"]
            }),
            Host: "issue8261.com",
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Close: true,
            RequestURI: "/"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // HEAD with Content-Length 0. Make sure this is permitted,
    // since I think we used to send it.
    new(
        "HEAD / HTTP/1.1\r\nHost: issue8261.com\r\nConnection: close\r\nContent-Length: 0\r\n\r\n",
        Ꮡ(new Request(
            Method: "HEAD",
            URL: Ꮡ(new url.URL(
                Path: "/"
            )),
            Header: new ΔHeader(new map<@string, slice<@string>>{
                ["Connection"] = ["close"],
                ["Content-Length"] = ["0"]
            }),
            Host: "issue8261.com",
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Close: true,
            RequestURI: "/"
        )),

        noBodyStr,
        noTrailer,
        noError
    ),

    // http2 client preface:
    new(
        "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n",
        Ꮡ(new Request(
            Method: "PRI",
            URL: Ꮡ(new url.URL(
                Path: "*"
            )),
            Header: new ΔHeader(new map<@string, slice<@string>>{}),
            Proto: "HTTP/2.0",
            ProtoMajor: 2,
            ProtoMinor: 0,
            RequestURI: "*",
            ContentLength: -1,
            Close: true
        )),
        noBodyStr,
        noTrailer,
        noError
    )
]);
internal static ref slice<reqTest> reqTests => ref ((StaticBox<slice<reqTest>>)ᏑreqTests).ValueSlot;

public static void TestReadRequest(ж<testing.T> t) {
    foreach (var i in range(builtin.len(reqTests))) {
        var tt = Ꮡ(reqTests, i);
        var (req, err) = ReadRequest(bufio.NewReader(new http_test_package.strings_ReaderжReader(strings.NewReader(tt.Raw))));
        if (err != nil) {
            if (err.Error() != tt.Error) {
                t.Errorf("#%d: error %q, want error %q", i, err.Error(), tt.Error);
            }
            continue;
        }
        var rbody = req.Body;
        req.Body = default;
        @string testName = fmt.Sprintf("Test %d (%q)"u8, i, tt.Raw);
        diff(t, testName, req.OrTypedNil(), tt.Req.OrTypedNil());
        ref var bout = ref heap(new strings.Builder(), out var Ꮡbout);
        if (rbody != nil) {
            var (_, errΔ1) = io.Copy(new http_test_package.strings_BuilderжWriter(Ꮡbout), rbody);
            if (errΔ1 != nil) {
                t.Fatalf("%s: copying body: %v", testName, errΔ1);
            }
            rbody.Close();
        }
        @string body = bout.String();
        if (body != tt.Body) {
            t.Errorf("%s: Body = %q want %q", testName, body, tt.Body);
        }
        if (!reflect.DeepEqual(tt.Trailer, req.Trailer)) {
            t.Errorf("%s: Trailers differ.\n got: %v\nwant: %v", testName, req.Trailer, tt.Trailer);
        }
    }
}

// reqBytes treats req as a request (with \n delimiters) and returns it with \r\n delimiters,
// ending in \r\n\r\n
internal static slice<byte> reqBytes(@string req) {
    return slice<byte>(strings.ReplaceAll(strings.TrimSpace(req), "\n", "\r\n") + "\r\n\r\n");
}

partial struct badRequestTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal slice<byte> req;
}
internal static slice<badRequestTestsᴛ1> badRequestTests = slice<badRequestTestsᴛ1>([
    new("bad_connect_host", reqBytes("CONNECT []%20%48%54%54%50%2f%31%2e%31%0a%4d%79%48%65%61%64%65%72%3a%20%31%32%33%0a%0a HTTP/1.0")),
    new("smuggle_two_contentlen", reqBytes("""
POST / HTTP/1.1
Content-Length: 3
Content-Length: 4

abc
"""u8.Raw())),
    new("smuggle_two_content_len_head", reqBytes("""
HEAD / HTTP/1.1
Host: foo
Content-Length: 4
Content-Length: 5

1234
"""u8.Raw())),

    // golang.org/issue/22464
    new("leading_space_in_header", reqBytes("""
GET / HTTP/1.1
 Host: foo
"""u8.Raw())),
    new("leading_tab_in_header", reqBytes("""
GET / HTTP/1.1

"""u8.Raw() + "\t"u8 + @"Host: foo"u8))
]);

public static void TestReadRequest_Bad(ж<testing.T> t) {
    foreach (var (_, tt) in range(badRequestTests)) {
        var (got, err) = ReadRequest(bufio.NewReader(new http_test_package.bytes_ReaderжReader(bytes.NewReader(tt.req))));
        if (err == nil) {
            var (all, errΔ1) = io.ReadAll(got.Body);
            t.Errorf("%s: got unexpected request = %#v\n  Body = %q, %v", tt.name, got.OrTypedNil(), all, errΔ1);
        }
    }
}

} // end http_internal_test_package
