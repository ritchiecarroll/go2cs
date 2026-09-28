// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bytes = bytes_package;
using encodingPkg = encoding_package;
using gob = go.encoding.gob_package;
using json = go.encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using encoding = encoding_package;
using go.encoding;
using static go.net.url_package;

partial class url_internal_test_package {

[GoType] public partial struct URLTest {
    internal @string @in;
    internal ж<global::go.net.url_package.URL> @out; // expected parse
    internal @string roundtrip; // expected result of reserializing the URL; empty means same as "in".
}

// no path
// path
// path with hex escaping
// fragment with hex escaping
// user
// escape sequence in username
// empty query
// query ending in question mark (Issue 14573)
// query
// query with hex escaping: NOT parsed
// %20 outside query
// path without leading /, so no parsing
// path without leading /, so no parsing
// non-authority with path; see golang.org/issue/46059
// non-authority
// unescaped :// in query should not create a scheme
// leading // without scheme should create an authority
// leading // without scheme, with userinfo, path, and query
// Three leading slashes isn't an authority, but doesn't return an error.
// (We can't return an error, as this code is also used via
// ServeHTTP -> ReadRequest -> Parse, which is arguably a
// different URL parsing context, but currently shares the
// same codepath)
// unescaped @ in username should not confuse host
// unescaped @ in password should not confuse host
// "Windows" paths are no exception to the rule.
// See golang.org/issue/6027, especially comment #9.
// case-insensitive scheme
// Relative path
// escaped '?' in username and password
// host subcomponent; IPv4 address in RFC 3986
// host and port subcomponents; IPv4 address in RFC 3986
// host subcomponent; IPv6 address in RFC 3986
// host and port subcomponents; IPv6 address in RFC 3986
// valid IPv6 host with port and path
// host subcomponent; IPv6 address with zone identifier in RFC 6874
// alphanum zone identifier
// host and port subcomponents; IPv6 address with zone identifier in RFC 6874
// alphanum zone identifier
// host subcomponent; IPv6 address with zone identifier in RFC 6874
// percent-encoded+unreserved zone identifier
// host and port subcomponents; IPv6 address with zone identifier in RFC 6874
// percent-encoded+unreserved zone identifier
// alternate escapings of path survive round trip
// issue 12036
// worst case host, still round trips
// worst case path, still round trips
// golang.org/issue/5684
// golang.org/issue/12200 (colon with empty port)
// Malformed IPv6 but still accepted.
// Malformed IPv6 but still accepted.
// golang.org/issue/7991 and golang.org/issue/12719 (non-ascii %-encoded in host)
// golang.org/issue/10433 (path beginning with //)
// test that we can reparse the host names we accept.
// spaces in hosts are disallowed but escaped spaces in IPv6 scope IDs are grudgingly OK.
// This happens on Windows.
// golang.org/issue/14002
// test we can roundtrip magnet url
// fix issue https://golang.org/issue/20054
internal static slice<URLTest> urltests = new URLTest[]{
    new(
        "http://www.google.com"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/file%20one%26two"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/file one&two"u8,
            RawPath: "/file%20one%26two"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/#file%20one%26two"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            Fragment: "file one&two"u8,
            RawFragment: "file%20one%26two"u8
        )),
        ""u8
    ),
    new(
        "ftp://webmaster@www.google.com/"u8,
        Ꮡ(new URL(
            Scheme: "ftp"u8,
            User: User("webmaster"u8),
            Host: "www.google.com"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "ftp://john%20doe@www.google.com/"u8,
        Ꮡ(new URL(
            Scheme: "ftp"u8,
            User: User("john doe"u8),
            Host: "www.google.com"u8,
            Path: "/"u8
        )),
        "ftp://john%20doe@www.google.com/"u8
    ),
    new(
        "http://www.google.com/?"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            ForceQuery: true
        )),
        ""u8
    ),
    new(
        "http://www.google.com/?foo=bar?"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            RawQuery: "foo=bar?"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/?q=go+language"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            RawQuery: "q=go+language"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/?q=go%20language"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            RawQuery: "q=go%20language"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/a%20b?q=c+d"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/a b"u8,
            RawQuery: "q=c+d"u8
        )),
        ""u8
    ),
    new(
        "http:www.google.com/?q=go+language"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Opaque: "www.google.com/"u8,
            RawQuery: "q=go+language"u8
        )),
        "http:www.google.com/?q=go+language"u8
    ),
    new(
        "http:%2f%2fwww.google.com/?q=go+language"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Opaque: "%2f%2fwww.google.com/"u8,
            RawQuery: "q=go+language"u8
        )),
        "http:%2f%2fwww.google.com/?q=go+language"u8
    ),
    new(
        "mailto:/webmaster@golang.org"u8,
        Ꮡ(new URL(
            Scheme: "mailto"u8,
            Path: "/webmaster@golang.org"u8,
            OmitHost: true
        )),
        ""u8
    ),
    new(
        "mailto:webmaster@golang.org"u8,
        Ꮡ(new URL(
            Scheme: "mailto"u8,
            Opaque: "webmaster@golang.org"u8
        )),
        ""u8
    ),
    new(
        "/foo?query=http://bad"u8,
        Ꮡ(new URL(
            Path: "/foo"u8,
            RawQuery: "query=http://bad"u8
        )),
        ""u8
    ),
    new(
        "//foo"u8,
        Ꮡ(new URL(
            Host: "foo"u8
        )),
        ""u8
    ),
    new(
        "//user@foo/path?a=b"u8,
        Ꮡ(new URL(
            User: User("user"u8),
            Host: "foo"u8,
            Path: "/path"u8,
            RawQuery: "a=b"u8
        )),
        ""u8
    ),
    new(
        "///threeslashes"u8,
        Ꮡ(new URL(
            Path: "///threeslashes"u8
        )),
        ""u8
    ),
    new(
        "http://user:password@google.com"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            User: UserPassword("user"u8, "password"u8),
            Host: "google.com"u8
        )),
        "http://user:password@google.com"u8
    ),
    new(
        "http://j@ne:password@google.com"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            User: UserPassword("j@ne"u8, "password"u8),
            Host: "google.com"u8
        )),
        "http://j%40ne:password@google.com"u8
    ),
    new(
        "http://jane:p@ssword@google.com"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            User: UserPassword("jane"u8, "p@ssword"u8),
            Host: "google.com"u8
        )),
        "http://jane:p%40ssword@google.com"u8
    ),
    new(
        "http://j@ne:password@google.com/p@th?q=@go"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            User: UserPassword("j@ne"u8, "password"u8),
            Host: "google.com"u8,
            Path: "/p@th"u8,
            RawQuery: "q=@go"u8
        )),
        "http://j%40ne:password@google.com/p@th?q=@go"u8
    ),
    new(
        "http://www.google.com/?q=go+language#foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            RawQuery: "q=go+language"u8,
            Fragment: "foo"u8
        )),
        ""u8
    ),
    new(
        "http://www.google.com/?q=go+language#foo&bar"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            RawQuery: "q=go+language"u8,
            Fragment: "foo&bar"u8
        )),
        "http://www.google.com/?q=go+language#foo&bar"u8
    ),
    new(
        "http://www.google.com/?q=go+language#foo%26bar"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "/"u8,
            RawQuery: "q=go+language"u8,
            Fragment: "foo&bar"u8,
            RawFragment: "foo%26bar"u8
        )),
        "http://www.google.com/?q=go+language#foo%26bar"u8
    ),
    new(
        "file:///home/adg/rabbits"u8,
        Ꮡ(new URL(
            Scheme: "file"u8,
            Host: ""u8,
            Path: "/home/adg/rabbits"u8
        )),
        "file:///home/adg/rabbits"u8
    ),
    new(
        "file:///C:/FooBar/Baz.txt"u8,
        Ꮡ(new URL(
            Scheme: "file"u8,
            Host: ""u8,
            Path: "/C:/FooBar/Baz.txt"u8
        )),
        "file:///C:/FooBar/Baz.txt"u8
    ),
    new(
        "MaIlTo:webmaster@golang.org"u8,
        Ꮡ(new URL(
            Scheme: "mailto"u8,
            Opaque: "webmaster@golang.org"u8
        )),
        "mailto:webmaster@golang.org"u8
    ),
    new(
        "a/b/c"u8,
        Ꮡ(new URL(
            Path: "a/b/c"u8
        )),
        "a/b/c"u8
    ),
    new(
        "http://%3Fam:pa%3Fsword@google.com"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            User: UserPassword("?am"u8, "pa?sword"u8),
            Host: "google.com"u8
        )),
        ""u8
    ),
    new(
        "http://192.168.0.1/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "192.168.0.1"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "http://192.168.0.1:8080/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "192.168.0.1:8080"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "http://[fe80::1]/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[fe80::1]"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "http://[fe80::1]:8080/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[fe80::1]:8080"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "https://[2001:db8::1]:8443/test/path"u8,
        Ꮡ(new URL(
            Scheme: "https"u8,
            Host: "[2001:db8::1]:8443"u8,
            Path: "/test/path"u8
        )),
        ""u8
    ),
    new(
        "http://[fe80::1%25en0]/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[fe80::1%en0]"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "http://[fe80::1%25en0]:8080/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[fe80::1%en0]:8080"u8,
            Path: "/"u8
        )),
        ""u8
    ),
    new(
        "http://[fe80::1%25%65%6e%301-._~]/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[fe80::1%en01-._~]"u8,
            Path: "/"u8
        )),
        "http://[fe80::1%25en01-._~]/"u8
    ),
    new(
        "http://[fe80::1%25%65%6e%301-._~]:8080/"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[fe80::1%en01-._~]:8080"u8,
            Path: "/"u8
        )),
        "http://[fe80::1%25en01-._~]:8080/"u8
    ),
    new(
        "http://rest.rsc.io/foo%2fbar/baz%2Fquux?alt=media"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "rest.rsc.io"u8,
            Path: "/foo/bar/baz/quux"u8,
            RawPath: "/foo%2fbar/baz%2Fquux"u8,
            RawQuery: "alt=media"u8
        )),
        ""u8
    ),
    new(
        "mysql://a,b,c/bar"u8,
        Ꮡ(new URL(
            Scheme: "mysql"u8,
            Host: "a,b,c"u8,
            Path: "/bar"u8
        )),
        ""u8
    ),
    new(
        "scheme://!$&'()*+,;=hello!:1/path"u8,
        Ꮡ(new URL(
            Scheme: "scheme"u8,
            Host: "!$&'()*+,;=hello!:1"u8,
            Path: "/path"u8
        )),
        ""u8
    ),
    new(
        "http://host/!$&'()*+,;=:@[hello]"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "host"u8,
            Path: "/!$&'()*+,;=:@[hello]"u8,
            RawPath: "/!$&'()*+,;=:@[hello]"u8
        )),
        ""u8
    ),
    new(
        "http://example.com/oid/[order_id]"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/oid/[order_id]"u8,
            RawPath: "/oid/[order_id]"u8
        )),
        ""u8
    ),
    new(
        "http://192.168.0.2:8080/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "192.168.0.2:8080"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://192.168.0.2:/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "192.168.0.2:"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://2b01:e34:ef40:7730:8e70:5aff:fefe:edac:8080/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "2b01:e34:ef40:7730:8e70:5aff:fefe:edac:8080"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://2b01:e34:ef40:7730:8e70:5aff:fefe:edac:/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "2b01:e34:ef40:7730:8e70:5aff:fefe:edac:"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:8080/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:8080"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://hello.世界.com/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "hello.世界.com"u8,
            Path: "/foo"u8
        )),
        "http://hello.%E4%B8%96%E7%95%8C.com/foo"u8
    ),
    new(
        "http://hello.%e4%b8%96%e7%95%8c.com/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "hello.世界.com"u8,
            Path: "/foo"u8
        )),
        "http://hello.%E4%B8%96%E7%95%8C.com/foo"u8
    ),
    new(
        "http://hello.%E4%B8%96%E7%95%8C.com/foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "hello.世界.com"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "http://example.com//foo"u8,
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "//foo"u8
        )),
        ""u8
    ),
    new(
        "myscheme://authority<\"hi\">/foo"u8,
        Ꮡ(new URL(
            Scheme: "myscheme"u8,
            Host: "authority<\"hi\">"u8,
            Path: "/foo"u8
        )),
        ""u8
    ),
    new(
        "tcp://[2020::2020:20:2020:2020%25Windows%20Loves%20Spaces]:2020"u8,
        Ꮡ(new URL(
            Scheme: "tcp"u8,
            Host: "[2020::2020:20:2020:2020%Windows Loves Spaces]:2020"u8
        )),
        ""u8
    ),
    new(
        "magnet:?xt=urn:btih:c12fe1c06bba254a9dc9f519b335aa7c1367a88a&dn"u8,
        Ꮡ(new URL(
            Scheme: "magnet"u8,
            Host: ""u8,
            Path: ""u8,
            RawQuery: "xt=urn:btih:c12fe1c06bba254a9dc9f519b335aa7c1367a88a&dn"u8
        )),
        "magnet:?xt=urn:btih:c12fe1c06bba254a9dc9f519b335aa7c1367a88a&dn"u8
    ),
    new(
        "mailto:?subject=hi"u8,
        Ꮡ(new URL(
            Scheme: "mailto"u8,
            Host: ""u8,
            Path: ""u8,
            RawQuery: "subject=hi"u8
        )),
        "mailto:?subject=hi"u8
    )
}.slice();

// more useful string for debugging than fmt's struct printer
internal static @string ufmt(ж<global::go.net.url_package.URL> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    any user = default!;
    any pass = default!;
    if (u.User != nil) {
        user = u.User.Username();
        {
            var (p, ok) = u.User.Password(); if (ok) {
                pass = p;
            }
        }
    }
    return fmt.Sprintf("opaque=%q, scheme=%q, user=%#v, pass=%#v, host=%q, path=%q, rawpath=%q, rawq=%q, frag=%q, rawfrag=%q, forcequery=%v, omithost=%t"u8,
        u.Opaque, u.Scheme, user, pass, u.Host, u.Path, u.RawPath, u.RawQuery, u.Fragment, u.RawFragment, u.ForceQuery, u.OmitHost);
}

public static void BenchmarkString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    b.ReportAllocs();
    foreach (var (_, tt) in urltests) {
        var (u, err) = Parse(tt.@in);
        if (err != default!) {
            Ꮡb.Errorf("Parse(%q) returned error %s"u8, tt.@in, err);
            continue;
        }
        if (tt.roundtrip == ""u8) {
            continue;
        }
        b.StartTimer();
        @string g = default!;
        for (nint i = 0; i < b.N; i++) {
            g = u.String();
        }
        b.StopTimer();
        {
            @string w = tt.roundtrip; if (b.N > 0 && g != w) {
                Ꮡb.Errorf("Parse(%q).String() == %q, want %q"u8, tt.@in, g, w);
            }
        }
    }
}

public static void TestParse(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in urltests) {
        var (u, err) = Parse(tt.@in);
        if (err != default!) {
            Ꮡt.Errorf("Parse(%q) returned error %v"u8, tt.@in, err);
            continue;
        }
        if (!reflect.DeepEqual(u.OrTypedNil(), tt.@out.OrTypedNil())) {
            Ꮡt.Errorf("Parse(%q):\n\tgot  %v\n\twant %v\n"u8, tt.@in, ufmt(u), ufmt(tt.@out));
        }
    }
}

internal static readonly @string pathThatLooksSchemeRelative = "//not.a.user@not.a.host/just/a/path"u8;

// Tests exercising RFC 6874 compliance:
// with alphanum zone identifier
// with alphanum zone identifier
// with percent-encoded+unreserved zone identifier
// with percent-encoded+unreserved zone identifier
// These two cases are valid as textual representations as
// described in RFC 4007, but are not valid as address
// literals with IPv6 zone identifiers in URIs as described in
// RFC 6874.
// Tests exercising RFC 3986 compliance
// full IPv6 address
// compressed IPv6 address
// link-local address with zone ID (interface name)
// link-local address with zone ID (interface index)
// compressed IPv6 address with path
// link-local with zone, path, and query

[GoType("dyn")] partial struct parseRequestURLTestsᴛ1 {
    internal @string url;
    internal bool expectedValid;
}
internal static slice<parseRequestURLTestsᴛ1> parseRequestURLTests = new parseRequestURLTestsᴛ1[]{
    new("http://foo.com"u8, true),
    new("http://foo.com/"u8, true),
    new("http://foo.com/path"u8, true),
    new("/"u8, true),
    new(pathThatLooksSchemeRelative, true),
    new("//not.a.user@%66%6f%6f.com/just/a/path/also"u8, true),
    new("*"u8, true),
    new("http://192.168.0.1/"u8, true),
    new("http://192.168.0.1:8080/"u8, true),
    new("http://[fe80::1]/"u8, true),
    new("http://[fe80::1]:8080/"u8, true),
    new("http://[fe80::1%25en0]/"u8, true),
    new("http://[fe80::1%25en0]:8080/"u8, true),
    new("http://[fe80::1%25%65%6e%301-._~]/"u8, true),
    new("http://[fe80::1%25%65%6e%301-._~]:8080/"u8, true),
    new("foo.html"u8, false),
    new("../dir/"u8, false),
    new(" http://foo.com"u8, false),
    new("http://192.168.0.%31/"u8, false),
    new("http://192.168.0.%31:8080/"u8, false),
    new("http://[fe80::%31]/"u8, false),
    new("http://[fe80::%31]:8080/"u8, false),
    new("http://[fe80::%31%25en0]/"u8, false),
    new("http://[fe80::%31%25en0]:8080/"u8, false),
    new("http://[fe80::1%en0]/"u8, false),
    new("http://[fe80::1%en0]:8080/"u8, false),
    new("https://[1:2:3:4:5:6:7:8]"u8, true),
    new("https://[2001:db8::a:b:c:d]"u8, true),
    new("https://[fe80::1%25eth0]"u8, true),
    new("https://[fe80::abc:def%254]"u8, true),
    new("https://[2001:db8::1]/path"u8, true),
    new("https://[fe80::1%25eth0]/path?query=1"u8, true),
    new("https://[::ffff:192.0.2.1]"u8, true),
    new("https://[:1] "u8, false),
    new("https://[1:2:3:4:5:6:7:8:9]"u8, false),
    new("https://[1::1::1]"u8, false),
    new("https://[1:2:3:]"u8, false),
    new("https://[ffff::127.0.0.4000]"u8, false),
    new("https://[0:0::test.com]:80"u8, false),
    new("https://[2001:db8::test.com]"u8, false),
    new("https://[test.com]"u8, false)
}.slice();

public static void TestParseRequestURI(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in parseRequestURLTests) {
        var (_, errΔ1) = ParseRequestURI(test.url);
        if (test.expectedValid && errΔ1 != default!){
            Ꮡt.Errorf("ParseRequestURI(%q) gave err %v; want no error"u8, test.url, errΔ1);
        } else 
        if (!test.expectedValid && errΔ1 == default!) {
            Ꮡt.Errorf("ParseRequestURI(%q) gave nil error; want some error"u8, test.url);
        }
    }
    var (url, err) = ParseRequestURI(pathThatLooksSchemeRelative);
    if (err != default!) {
        Ꮡt.Fatalf("Unexpected error %v"u8, err);
    }
    if ((~url).Path != pathThatLooksSchemeRelative) {
        Ꮡt.Errorf("ParseRequestURI path:\ngot  %q\nwant %q"u8, (~url).Path, pathThatLooksSchemeRelative);
    }
}

// No leading slash on path should prepend slash on String() call
// Relative path with first element containing ":" should be prepended with "./", golang.org/issue/17184
// Relative path with second element containing ":" should not be prepended with "./"
// Non-relative path with first element containing ":" should not be prepended with "./"

[GoType("dyn")] partial struct stringURLTestsᴛ1 {
    internal global::go.net.url_package.URL url;
    internal @string want;
}
internal static slice<stringURLTestsᴛ1> stringURLTests = new stringURLTestsᴛ1[]{
    new(
        url: new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "search"u8
        ),
        want: "http://www.google.com/search"u8
    ),
    new(
        url: new URL(
            Path: "this:that"u8
        ),
        want: "./this:that"u8
    ),
    new(
        url: new URL(
            Path: "here/this:that"u8
        ),
        want: "here/this:that"u8
    ),
    new(
        url: new URL(
            Scheme: "http"u8,
            Host: "www.google.com"u8,
            Path: "this:that"u8
        ),
        want: "http://www.google.com/this:that"u8
    )
}.slice();

public static void TestURLString(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in urltests) {
        var (u, err) = Parse(tt.@in);
        if (err != default!) {
            Ꮡt.Errorf("Parse(%q) returned error %s"u8, tt.@in, err);
            continue;
        }
        @string expected = tt.@in;
        if (tt.roundtrip != ""u8) {
            expected = tt.roundtrip;
        }
        @string s = u.String();
        if (s != expected) {
            Ꮡt.Errorf("Parse(%q).String() == %q (expected %q)"u8, tt.@in, s, expected);
        }
    }
    foreach (var (_, vᴛ1) in stringURLTests) {
        var tt = vᴛ1;

        {
            @string got = tt.url.String(); if (got != tt.want) {
                Ꮡt.Errorf("%+v.String() = %q; want %q"u8, tt.url, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userˢ = "user"u8;
internal static readonly @string passwordˢ = "password"u8;

[GoType("dyn")] internal partial struct TestURLRedacted_cases {
    internal @string name;
    internal ж<global::go.net.url_package.URL> url;
    internal @string want;
}

public static void TestURLRedacted(ж<testing.T> Ꮡt) {
    var cases = new TestURLRedacted_cases[]{
        new(
            name: "non-blank Password"u8,
            url: Ꮡ(new URL(
                Scheme: "http"u8,
                Host: "host.tld"u8,
                Path: "this:that"u8,
                User: UserPassword(userˢ, passwordˢ)
            )),
            want: "http://user:xxxxx@host.tld/this:that"u8
        ),
        new(
            name: "blank Password"u8,
            url: Ꮡ(new URL(
                Scheme: "http"u8,
                Host: "host.tld"u8,
                Path: "this:that"u8,
                User: User(userˢ)
            )),
            want: "http://user@host.tld/this:that"u8
        ),
        new(
            name: "nil User"u8,
            url: Ꮡ(new URL(
                Scheme: "http"u8,
                Host: "host.tld"u8,
                Path: "this:that"u8,
                User: UserPassword(""u8, passwordˢ)
            )),
            want: "http://:xxxxx@host.tld/this:that"u8
        ),
        new(
            name: "blank Username, blank Password"u8,
            url: Ꮡ(new URL(
                Scheme: "http"u8,
                Host: "host.tld"u8,
                Path: "this:that"u8
            )),
            want: "http://host.tld/this:that"u8
        ),
        new(
            name: "empty URL"u8,
            url: Ꮡ(new URL(nil)),
            want: ""u8
        ),
        new(
            name: "nil URL"u8,
            url: nil,
            want: ""u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in cases) {
        ref var tt = ref heap(new TestURLRedacted_cases(), out var Ꮡtt);
        tt = vᴛ1;

        var tΔ1 = Ꮡt;
        var ttʗ1 = tt;
        tΔ1.Run(tt.name, (ж<testing.T> tΔ2) => {
            {
                @string g = ttʗ1.url.Redacted();
                @string w = ttʗ1.want; if (g != w) {
                    tΔ2.Fatalf("got: %q\nwant: %q"u8, g, w);
                }
            }
        });
    }
}

[GoType] public partial struct EscapeTest {
    internal @string @in;
    internal @string @out;
    internal error err;
}

// not enough characters after %
// not enough characters after %
// not enough characters after %
// not enough characters after %
// invalid hex digits
internal static slice<EscapeTest> unescapeTests = new EscapeTest[]{
    new(
        ""u8,
        ""u8,
        default!
    ),
    new(
        "abc"u8,
        "abc"u8,
        default!
    ),
    new(
        "1%41"u8,
        "1A"u8,
        default!
    ),
    new(
        "1%41%42%43"u8,
        "1ABC"u8,
        default!
    ),
    new(
        "%4a"u8,
        "J"u8,
        default!
    ),
    new(
        "%6F"u8,
        "o"u8,
        default!
    ),
    new(
        "%"u8,
        ""u8,
        ((global::go.net.url_package.EscapeError)(@string)"%"u8)
    ),
    new(
        "%a"u8,
        ""u8,
        ((global::go.net.url_package.EscapeError)(@string)"%a"u8)
    ),
    new(
        "%1"u8,
        ""u8,
        ((global::go.net.url_package.EscapeError)(@string)"%1"u8)
    ),
    new(
        "123%45%6"u8,
        ""u8,
        ((global::go.net.url_package.EscapeError)(@string)"%6"u8)
    ),
    new(
        "%zzzzz"u8,
        ""u8,
        ((global::go.net.url_package.EscapeError)(@string)"%zz"u8)
    ),
    new(
        "a+b"u8,
        "a b"u8,
        default!
    ),
    new(
        "a%20b"u8,
        "a b"u8,
        default!
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxˢ = "XXX"u8;

public static void TestUnescape(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in unescapeTests) {
        var (actual, err) = QueryUnescape(tt.@in);
        if (actual != tt.@out || (err != default!) != (tt.err != default!)) {
            Ꮡt.Errorf("QueryUnescape(%q) = %q, %s; want %q, %s"u8, tt.@in, actual, err, tt.@out, tt.err);
        }
        @string @in = tt.@in;
        @string @out = tt.@out;
        if (strings.Contains(tt.@in, "+"u8)) {
            @in = strings.ReplaceAll(tt.@in, "+"u8, "%20"u8);
            var (actualΔ1, errΔ1) = PathUnescape(@in);
            if (actualΔ1 != tt.@out || (errΔ1 != default!) != (tt.err != default!)) {
                Ꮡt.Errorf("PathUnescape(%q) = %q, %s; want %q, %s"u8, @in, actualΔ1, errΔ1, tt.@out, tt.err);
            }
            if (tt.err == default!) {
                var (s, errΔ2) = QueryUnescape(strings.ReplaceAll(tt.@in, "+"u8, xxxˢ));
                if (errΔ2 != default!) {
                    continue;
                }
                @in = tt.@in;
                @out = strings.ReplaceAll(s, xxxˢ, "+"u8);
            }
        }
        (actual, err) = PathUnescape(@in);
        if (actual != @out || (err != default!) != (tt.err != default!)) {
            Ꮡt.Errorf("PathUnescape(%q) = %q, %s; want %q, %s"u8, @in, actual, err, @out, tt.err);
        }
    }
}

internal static slice<EscapeTest> queryEscapeTests = new EscapeTest[]{
    new(
        ""u8,
        ""u8,
        default!
    ),
    new(
        "abc"u8,
        "abc"u8,
        default!
    ),
    new(
        "one two"u8,
        "one+two"u8,
        default!
    ),
    new(
        "10%"u8,
        "10%25"u8,
        default!
    ),
    new(
        " ?&=#+%!<>#\"{}|\\^[]`☺\t:/@$'()*,;"u8,
        "+%3F%26%3D%23%2B%25%21%3C%3E%23%22%7B%7D%7C%5C%5E%5B%5D%60%E2%98%BA%09%3A%2F%40%24%27%28%29%2A%2C%3B"u8,
        default!
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noErrorˢ = (@string)"[no error]"u8;

public static void TestQueryEscape(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in queryEscapeTests) {
        @string actual = QueryEscape(tt.@in);
        if (tt.@out != actual) {
            Ꮡt.Errorf("QueryEscape(%q) = %q, want %q"u8, tt.@in, actual, tt.@out);
        }
        // for bonus points, verify that escape:unescape is an identity.
        var (roundtrip, err) = QueryUnescape(actual);
        if (roundtrip != tt.@in || err != default!) {
            Ꮡt.Errorf("QueryUnescape(%q) = %q, %s; want %q, %s"u8, actual, roundtrip, err, tt.@in, noErrorˢ);
        }
    }
}

internal static slice<EscapeTest> pathEscapeTests = new EscapeTest[]{
    new(
        ""u8,
        ""u8,
        default!
    ),
    new(
        "abc"u8,
        "abc"u8,
        default!
    ),
    new(
        "abc+def"u8,
        "abc+def"u8,
        default!
    ),
    new(
        "a/b"u8,
        "a%2Fb"u8,
        default!
    ),
    new(
        "one two"u8,
        "one%20two"u8,
        default!
    ),
    new(
        "10%"u8,
        "10%25"u8,
        default!
    ),
    new(
        " ?&=#+%!<>#\"{}|\\^[]`☺\t:/@$'()*,;"u8,
        "%20%3F&=%23+%25%21%3C%3E%23%22%7B%7D%7C%5C%5E%5B%5D%60%E2%98%BA%09:%2F@$%27%28%29%2A%2C%3B"u8,
        default!
    )
}.slice();

public static void TestPathEscape(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in pathEscapeTests) {
        @string actual = PathEscape(tt.@in);
        if (tt.@out != actual) {
            Ꮡt.Errorf("PathEscape(%q) = %q, want %q"u8, tt.@in, actual, tt.@out);
        }
        // for bonus points, verify that escape:unescape is an identity.
        var (roundtrip, err) = PathUnescape(actual);
        if (roundtrip != tt.@in || err != default!) {
            Ꮡt.Errorf("PathUnescape(%q) = %q, %s; want %q, %s"u8, actual, roundtrip, err, tt.@in, noErrorˢ);
        }
    }
}

//var userinfoTests = []UserinfoTest{
//	{"user", "password", "user:password"},
//	{"foo:bar", "~!@#$%^&*()_+{}|[]\\-=`:;'\"<>?,./",
//		"foo%3Abar:~!%40%23$%25%5E&*()_+%7B%7D%7C%5B%5D%5C-=%60%3A;'%22%3C%3E?,.%2F"},
//}
[GoType] public partial struct EncodeQueryTest {
    internal global::go.net.url_package.Values m;
    internal @string expected;
}

internal static slice<EncodeQueryTest> encodeQueryTests = new EncodeQueryTest[]{
    new(default!, ""u8),
    new(new Values(new map<@string, slice<@string>>{}), ""u8),
    new(new Values(new map<@string, slice<@string>>{["q"u8] = new @string[]{"puppies"u8}.slice(), ["oe"u8] = new @string[]{"utf8"u8}.slice()}), "oe=utf8&q=puppies"u8),
    new(new Values(new map<@string, slice<@string>>{["q"u8] = new @string[]{"dogs"u8, "&"u8, "7"u8}.slice()}), "q=dogs&q=%26&q=7"u8),
    new(new Values(new map<@string, slice<@string>>{
        ["a"u8] = new @string[]{"a1"u8, "a2"u8, "a3"u8}.slice(),
        ["b"u8] = new @string[]{"b1"u8, "b2"u8, "b3"u8}.slice(),
        ["c"u8] = new @string[]{"c1"u8, "c2"u8, "c3"u8}.slice()
    }), "a=a1&a=a2&a=a3&b=b1&b=b2&b=b3&c=c1&c=c2&c=c3"u8)
}.slice();

public static void TestEncodeQuery(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in encodeQueryTests) {
        {
            @string q = tt.m.Encode(); if (q != tt.expected) {
                Ꮡt.Errorf(@"EncodeQuery(%+v) = %q, want %q"u8, tt.m, q, tt.expected);
            }
        }
    }
}


[GoType("dyn")] partial struct resolvePathTestsᴛ1 {
    internal @string @base, @ref, expected;
}
internal static slice<resolvePathTestsᴛ1> resolvePathTests = new resolvePathTestsᴛ1[]{
    new("a/b"u8, "."u8, "/a/"u8),
    new("a/b"u8, "c"u8, "/a/c"u8),
    new("a/b"u8, ".."u8, "/"u8),
    new("a/"u8, ".."u8, "/"u8),
    new("a/"u8, "../.."u8, "/"u8),
    new("a/b/c"u8, ".."u8, "/a/"u8),
    new("a/b/c"u8, "../d"u8, "/a/d"u8),
    new("a/b/c"u8, ".././d"u8, "/a/d"u8),
    new("a/b"u8, "./.."u8, "/"u8),
    new("a/./b"u8, "."u8, "/a/"u8),
    new("a/../"u8, "."u8, "/"u8),
    new("a/.././b"u8, "c"u8, "/c"u8)
}.slice();

public static void TestResolvePath(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in resolvePathTests) {
        @string got = resolvePath(test.@base, test.@ref);
        if (got != test.expected) {
            Ꮡt.Errorf("For %q + %q got %q; expected %q"u8, test.@base, test.@ref, got, test.expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBCˢ = "a/b/c"u8;

public static void BenchmarkResolvePath(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        resolvePath(aBCˢ, ".././d"u8);
    }
}

// Absolute URL references
// Path-absolute references
// Multiple slashes
// Scheme-relative
// Path-relative references:
// ... current directory
// ... going down
// ... going up
// ".." in the middle (issue 3560)
// Remove any dot-segments prior to forming the target URI.
// https://datatracker.ietf.org/doc/html/rfc3986#section-5.2.4
// Triple dot isn't special
// Fragment
// Paths with escaping (issue 16947).
// RFC 3986: Normal Examples
// https://datatracker.ietf.org/doc/html/rfc3986#section-5.4.1
// RFC 3986: Abnormal Examples
// https://datatracker.ietf.org/doc/html/rfc3986#section-5.4.2
// Extras.
// Empty path and query but with ForceQuery (issue 46033).
// Opaque URLs (issue 66084).

[GoType("dyn")] partial struct resolveReferenceTestsᴛ1 {
    internal @string @base, rel, expected;
}
internal static slice<resolveReferenceTestsᴛ1> resolveReferenceTests = new resolveReferenceTestsᴛ1[]{
    new("http://foo.com?a=b"u8, "https://bar.com/"u8, "https://bar.com/"u8),
    new("http://foo.com/"u8, "https://bar.com/?a=b"u8, "https://bar.com/?a=b"u8),
    new("http://foo.com/"u8, "https://bar.com/?"u8, "https://bar.com/?"u8),
    new("http://foo.com/bar"u8, "mailto:foo@example.com"u8, "mailto:foo@example.com"u8),
    new("http://foo.com/bar"u8, "/baz"u8, "http://foo.com/baz"u8),
    new("http://foo.com/bar?a=b#f"u8, "/baz"u8, "http://foo.com/baz"u8),
    new("http://foo.com/bar?a=b"u8, "/baz?"u8, "http://foo.com/baz?"u8),
    new("http://foo.com/bar?a=b"u8, "/baz?c=d"u8, "http://foo.com/baz?c=d"u8),
    new("http://foo.com/bar"u8, "http://foo.com//baz"u8, "http://foo.com//baz"u8),
    new("http://foo.com/bar"u8, "http://foo.com///baz/quux"u8, "http://foo.com///baz/quux"u8),
    new("https://foo.com/bar?a=b"u8, "//bar.com/quux"u8, "https://bar.com/quux"u8),
    new("http://foo.com"u8, "."u8, "http://foo.com/"u8),
    new("http://foo.com/bar"u8, "."u8, "http://foo.com/"u8),
    new("http://foo.com/bar/"u8, "."u8, "http://foo.com/bar/"u8),
    new("http://foo.com"u8, "bar"u8, "http://foo.com/bar"u8),
    new("http://foo.com/"u8, "bar"u8, "http://foo.com/bar"u8),
    new("http://foo.com/bar/baz"u8, "quux"u8, "http://foo.com/bar/quux"u8),
    new("http://foo.com/bar/baz"u8, "../quux"u8, "http://foo.com/quux"u8),
    new("http://foo.com/bar/baz"u8, "../../../../../quux"u8, "http://foo.com/quux"u8),
    new("http://foo.com/bar"u8, ".."u8, "http://foo.com/"u8),
    new("http://foo.com/bar/baz"u8, "./.."u8, "http://foo.com/"u8),
    new("http://foo.com/bar/baz"u8, "quux/dotdot/../tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/../tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/.././tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/./../tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/dotdot/././../../tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/dotdot/./.././../tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/dotdot/dotdot/./../../.././././tail"u8, "http://foo.com/bar/quux/tail"u8),
    new("http://foo.com/bar/baz"u8, "quux/./dotdot/../dotdot/../dot/./tail/.."u8, "http://foo.com/bar/quux/dot/"u8),
    new("http://foo.com/dot/./dotdot/../foo/bar"u8, "../baz"u8, "http://foo.com/dot/baz"u8),
    new("http://foo.com/bar"u8, "..."u8, "http://foo.com/..."u8),
    new("http://foo.com/bar"u8, ".#frag"u8, "http://foo.com/#frag"u8),
    new("http://example.org/"u8, "#!$&%27()*+,;="u8, "http://example.org/#!$&%27()*+,;="u8),
    new("http://foo.com/foo%2fbar/"u8, "../baz"u8, "http://foo.com/baz"u8),
    new("http://foo.com/1/2%2f/3%2f4/5"u8, "../../a/b/c"u8, "http://foo.com/1/a/b/c"u8),
    new("http://foo.com/1/2/3"u8, "./a%2f../../b/..%2fc"u8, "http://foo.com/1/2/b/..%2fc"u8),
    new("http://foo.com/1/2%2f/3%2f4/5"u8, "./a%2f../b/../c"u8, "http://foo.com/1/2%2f/3%2f4/a%2f../c"u8),
    new("http://foo.com/foo%20bar/"u8, "../baz"u8, "http://foo.com/baz"u8),
    new("http://foo.com/foo"u8, "../bar%2fbaz"u8, "http://foo.com/bar%2fbaz"u8),
    new("http://foo.com/foo%2dbar/"u8, "./baz-quux"u8, "http://foo.com/foo%2dbar/baz-quux"u8),
    new("http://a/b/c/d;p?q"u8, "g:h"u8, "g:h"u8),
    new("http://a/b/c/d;p?q"u8, "g"u8, "http://a/b/c/g"u8),
    new("http://a/b/c/d;p?q"u8, "./g"u8, "http://a/b/c/g"u8),
    new("http://a/b/c/d;p?q"u8, "g/"u8, "http://a/b/c/g/"u8),
    new("http://a/b/c/d;p?q"u8, "/g"u8, "http://a/g"u8),
    new("http://a/b/c/d;p?q"u8, "//g"u8, "http://g"u8),
    new("http://a/b/c/d;p?q"u8, "?y"u8, "http://a/b/c/d;p?y"u8),
    new("http://a/b/c/d;p?q"u8, "g?y"u8, "http://a/b/c/g?y"u8),
    new("http://a/b/c/d;p?q"u8, "#s"u8, "http://a/b/c/d;p?q#s"u8),
    new("http://a/b/c/d;p?q"u8, "g#s"u8, "http://a/b/c/g#s"u8),
    new("http://a/b/c/d;p?q"u8, "g?y#s"u8, "http://a/b/c/g?y#s"u8),
    new("http://a/b/c/d;p?q"u8, ";x"u8, "http://a/b/c/;x"u8),
    new("http://a/b/c/d;p?q"u8, "g;x"u8, "http://a/b/c/g;x"u8),
    new("http://a/b/c/d;p?q"u8, "g;x?y#s"u8, "http://a/b/c/g;x?y#s"u8),
    new("http://a/b/c/d;p?q"u8, ""u8, "http://a/b/c/d;p?q"u8),
    new("http://a/b/c/d;p?q"u8, "."u8, "http://a/b/c/"u8),
    new("http://a/b/c/d;p?q"u8, "./"u8, "http://a/b/c/"u8),
    new("http://a/b/c/d;p?q"u8, ".."u8, "http://a/b/"u8),
    new("http://a/b/c/d;p?q"u8, "../"u8, "http://a/b/"u8),
    new("http://a/b/c/d;p?q"u8, "../g"u8, "http://a/b/g"u8),
    new("http://a/b/c/d;p?q"u8, "../.."u8, "http://a/"u8),
    new("http://a/b/c/d;p?q"u8, "../../"u8, "http://a/"u8),
    new("http://a/b/c/d;p?q"u8, "../../g"u8, "http://a/g"u8),
    new("http://a/b/c/d;p?q"u8, "../../../g"u8, "http://a/g"u8),
    new("http://a/b/c/d;p?q"u8, "../../../../g"u8, "http://a/g"u8),
    new("http://a/b/c/d;p?q"u8, "/./g"u8, "http://a/g"u8),
    new("http://a/b/c/d;p?q"u8, "/../g"u8, "http://a/g"u8),
    new("http://a/b/c/d;p?q"u8, "g."u8, "http://a/b/c/g."u8),
    new("http://a/b/c/d;p?q"u8, ".g"u8, "http://a/b/c/.g"u8),
    new("http://a/b/c/d;p?q"u8, "g.."u8, "http://a/b/c/g.."u8),
    new("http://a/b/c/d;p?q"u8, "..g"u8, "http://a/b/c/..g"u8),
    new("http://a/b/c/d;p?q"u8, "./../g"u8, "http://a/b/g"u8),
    new("http://a/b/c/d;p?q"u8, "./g/."u8, "http://a/b/c/g/"u8),
    new("http://a/b/c/d;p?q"u8, "g/./h"u8, "http://a/b/c/g/h"u8),
    new("http://a/b/c/d;p?q"u8, "g/../h"u8, "http://a/b/c/h"u8),
    new("http://a/b/c/d;p?q"u8, "g;x=1/./y"u8, "http://a/b/c/g;x=1/y"u8),
    new("http://a/b/c/d;p?q"u8, "g;x=1/../y"u8, "http://a/b/c/y"u8),
    new("http://a/b/c/d;p?q"u8, "g?y/./x"u8, "http://a/b/c/g?y/./x"u8),
    new("http://a/b/c/d;p?q"u8, "g?y/../x"u8, "http://a/b/c/g?y/../x"u8),
    new("http://a/b/c/d;p?q"u8, "g#s/./x"u8, "http://a/b/c/g#s/./x"u8),
    new("http://a/b/c/d;p?q"u8, "g#s/../x"u8, "http://a/b/c/g#s/../x"u8),
    new("https://a/b/c/d;p?q"u8, "//g?q"u8, "https://g?q"u8),
    new("https://a/b/c/d;p?q"u8, "//g#s"u8, "https://g#s"u8),
    new("https://a/b/c/d;p?q"u8, "//g/d/e/f?y#s"u8, "https://g/d/e/f?y#s"u8),
    new("https://a/b/c/d;p#s"u8, "?y"u8, "https://a/b/c/d;p?y"u8),
    new("https://a/b/c/d;p?q#s"u8, "?y"u8, "https://a/b/c/d;p?y"u8),
    new("https://a/b/c/d;p?q#s"u8, "?"u8, "https://a/b/c/d;p?"u8),
    new("https://foo.com/bar?a=b"u8, "http:opaque"u8, "http:opaque"u8),
    new("http:opaque?x=y#zzz"u8, "https:/foo?a=b#frag"u8, "https:/foo?a=b#frag"u8),
    new("http:opaque?x=y#zzz"u8, "https:foo:bar"u8, "https:foo:bar"u8),
    new("http:opaque?x=y#zzz"u8, "https:bar/baz?a=b#frag"u8, "https:bar/baz?a=b#frag"u8),
    new("http:opaque?x=y#zzz"u8, "https://user@host:1234?a=b#frag"u8, "https://user@host:1234?a=b#frag"u8),
    new("http:opaque?x=y#zzz"u8, "?a=b#frag"u8, "http:opaque?a=b#frag"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string schemeOpaqueˢ = "scheme:opaque"u8;

public static void TestResolveReference(ж<testing.T> Ꮡt) {
    ж<global::go.net.url_package.URL> mustParse(@string url) {
        var (u, err) = Parse(url);
        if (err != default!) {
            Ꮡt.Fatalf("Parse(%q) got err %v"u8, url, err);
        }
        return u;
    }
    var opaque = Ꮡ(new URL(Scheme: "scheme"u8, Opaque: "opaque"u8));
    foreach (var (_, test) in resolveReferenceTests) {
        var @base = mustParse(test.@base);
        var rel = mustParse(test.rel);
        var url = @base.ResolveReference(rel);
        {
            @string got = url.String(); if (got != test.expected) {
                Ꮡt.Errorf("URL(%q).ResolveReference(%q)\ngot  %q\nwant %q"u8, test.@base, test.rel, got, test.expected);
            }
        }
        // Ensure that new instances are returned.
        if (@base == url) {
            Ꮡt.Errorf("Expected URL.ResolveReference to return new URL instance."u8);
        }
        // Test the convenience wrapper too.
        (url, var err) = @base.Parse(test.rel);
        if (err != default!){
            Ꮡt.Errorf("URL(%q).Parse(%q) failed: %v"u8, test.@base, test.rel, err);
        } else 
        {
            @string got = url.String(); if (got != test.expected){
                Ꮡt.Errorf("URL(%q).Parse(%q)\ngot  %q\nwant %q"u8, test.@base, test.rel, got, test.expected);
            } else 
            if (@base == url) {
                // Ensure that new instances are returned for the wrapper too.
                Ꮡt.Errorf("Expected URL.Parse to return new URL instance."u8);
            }
        }
        // Ensure Opaque resets the URL.
        url = @base.ResolveReference(opaque);
        if (url.Value != opaque.Value) {
            Ꮡt.Errorf("ResolveReference failed to resolve opaque URL:\ngot  %#v\nwant %#v"u8, url.OrTypedNil(), opaque.OrTypedNil());
        }
        // Test the convenience wrapper with an opaque URL too.
        (url, err) = @base.Parse(schemeOpaqueˢ);
        if (err != default!){
            Ꮡt.Errorf(@"URL(%q).Parse(""scheme:opaque"") failed: %v"u8, test.@base, err);
        } else 
        if (url.Value != opaque.Value){
            Ꮡt.Errorf("Parse failed to resolve opaque URL:\ngot  %#v\nwant %#v"u8, opaque.OrTypedNil(), url.OrTypedNil());
        } else 
        if (@base == url) {
            // Ensure that new instances are returned, again.
            Ꮡt.Errorf("Expected URL.Parse to return new URL instance."u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpXComFooBarBar1Bar2ˢ = "http://x.com?foo=bar&bar=1&bar=2&baz"u8;
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string fooˢ2 = "Foo"u8;
internal static readonly @string bazˢ = "baz"u8;
internal static readonly @string noexistˢ = "noexist"u8;

public static void TestQueryValues(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (u, _) = Parse(httpXComFooBarBar1Bar2ˢ);
    var v = u.Query();
    if (len(v) != 3) {
        Ꮡt.Errorf("got %d keys in Query values, want 3"u8, len(v));
    }
    {
        @string g = v.Get(fooˢ);
        @string e = barˢ; if (g != e) {
            Ꮡt.Errorf("Get(foo) = %q, want %q"u8, g, e);
        }
    }
    // Case sensitive:
    {
        @string g = v.Get(fooˢ2);
        @string e = ""u8; if (g != e) {
            Ꮡt.Errorf("Get(Foo) = %q, want %q"u8, g, e);
        }
    }
    {
        @string g = v.Get(barˢ);
        @string e = "1"u8; if (g != e) {
            Ꮡt.Errorf("Get(bar) = %q, want %q"u8, g, e);
        }
    }
    {
        @string g = v.Get(bazˢ);
        @string e = ""u8; if (g != e) {
            Ꮡt.Errorf("Get(baz) = %q, want %q"u8, g, e);
        }
    }
    {
        var (h, e) = (v.Has(fooˢ), true); if (h != e) {
            Ꮡt.Errorf("Has(foo) = %t, want %t"u8, h, e);
        }
    }
    {
        var (h, e) = (v.Has(barˢ), true); if (h != e) {
            Ꮡt.Errorf("Has(bar) = %t, want %t"u8, h, e);
        }
    }
    {
        var (h, e) = (v.Has(bazˢ), true); if (h != e) {
            Ꮡt.Errorf("Has(baz) = %t, want %t"u8, h, e);
        }
    }
    {
        var (h, e) = (v.Has(noexistˢ), false); if (h != e) {
            Ꮡt.Errorf("Has(noexist) = %t, want %t"u8, h, e);
        }
    }
    v.Del(barˢ);
    {
        @string g = v.Get(barˢ);
        @string e = ""u8; if (g != e) {
            Ꮡt.Errorf("second Get(bar) = %q, want %q"u8, g, e);
        }
    }
}

[GoType] internal partial struct parseTest {
    internal @string query;
    internal global::go.net.url_package.Values @out;
    internal bool ok;
}

// hex encoding for semicolon
internal static slice<parseTest> parseTests = new parseTest[]{
    new(
        query: "a=1"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice()}),
        ok: true
    ),
    new(
        query: "a=1&b=2"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice(), ["b"u8] = new @string[]{"2"u8}.slice()}),
        ok: true
    ),
    new(
        query: "a=1&a=2&a=banana"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8, "2"u8, "banana"u8}.slice()}),
        ok: true
    ),
    new(
        query: "ascii=%3Ckey%3A+0x90%3E"u8,
        @out: new Values(new map<@string, slice<@string>>{["ascii"u8] = new @string[]{"<key: 0x90>"u8}.slice()}),
        ok: true
    ), new(
    query: "a=1;b=2"u8,
    @out: new Values(new map<@string, slice<@string>>{}),
    ok: false
), new(
    query: "a;b=1"u8,
    @out: new Values(new map<@string, slice<@string>>{}),
    ok: false
), new(
    query: "a=%3B"u8,
    @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{";"u8}.slice()}),
    ok: true
),
    new(
        query: "a%3Bb=1"u8,
        @out: new Values(new map<@string, slice<@string>>{["a;b"u8] = new @string[]{"1"u8}.slice()}),
        ok: true
    ),
    new(
        query: "a=1&a=2;a=banana"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice()}),
        ok: false
    ),
    new(
        query: "a;b&c=1"u8,
        @out: new Values(new map<@string, slice<@string>>{["c"u8] = new @string[]{"1"u8}.slice()}),
        ok: false
    ),
    new(
        query: "a=1&b=2;a=3&c=4"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice(), ["c"u8] = new @string[]{"4"u8}.slice()}),
        ok: false
    ),
    new(
        query: "a=1&b=2;c=3"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice()}),
        ok: false
    ),
    new(
        query: ";"u8,
        @out: new Values(new map<@string, slice<@string>>{}),
        ok: false
    ),
    new(
        query: "a=1;"u8,
        @out: new Values(new map<@string, slice<@string>>{}),
        ok: false
    ),
    new(
        query: "a=1&;"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice()}),
        ok: false
    ),
    new(
        query: ";a=1&b=2"u8,
        @out: new Values(new map<@string, slice<@string>>{["b"u8] = new @string[]{"2"u8}.slice()}),
        ok: false
    ),
    new(
        query: "a=1&b=2;"u8,
        @out: new Values(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice()}),
        ok: false
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ = "<error>"u8;
internal static readonly @string nilˢ = "<nil>"u8;

public static void TestParseQuery(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in parseTests) {
        ref var test = ref heap(new parseTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.query, (ж<testing.T> tΔ1) => {
            var (form, err) = ParseQuery(testʗ1.query);
            if (testʗ1.ok != (err == default!)) {
                @string want = errorˢ;
                if (testʗ1.ok) {
                    want = nilˢ;
                }
                tΔ1.Errorf("Unexpected error: %v, want %v"u8, err, want);
            }
            if (len(form) != len(testʗ1.@out)) {
                tΔ1.Errorf("len(form) = %d, want %d"u8, len(form), len(testʗ1.@out));
            }
            foreach (var (k, evs) in testʗ1.@out) {
                var (vs, ok) = form[k, ꟷ];
                if (!ok) {
                    tΔ1.Errorf("Missing key %q"u8, k);
                    continue;
                }
                if (len(vs) != len(evs)) {
                    tΔ1.Errorf("len(form[%q]) = %d, want %d"u8, k, len(vs), len(evs));
                    continue;
                }
                foreach (var (j, ev) in evs) {
                    {
                        @string v = vs[j]; if (v != ev) {
                            tΔ1.Errorf("form[%q][%d] = %q, want %q"u8, k, j, v, ev);
                        }
                    }
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;

[GoType("dyn")] internal partial struct TestParseQueryLimits_type {
    internal nint @params;
    internal @string godebug;
    internal bool wantErr;
}

public static void TestParseQueryLimits(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestParseQueryLimits_type[]{new(
        @params: 10,
        wantErr: false
    ), new(
        @params: defaultMaxParams,
        wantErr: false
    ), new(
        @params: defaultMaxParams + 1,
        wantErr: true
    ), new(
        @params: 10,
        godebug: "urlmaxqueryparams=9"u8,
        wantErr: true
    ), new(
        @params: defaultMaxParams + 1,
        godebug: "urlmaxqueryparams=0"u8,
        wantErr: false
    )
    }.slice()) {
        Ꮡt.Setenv(godebugˢ, test.godebug);
        var want = new Values(new map<@string, slice<@string>>{});
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        foreach (var i in range(test.@params)) {
            if (i > 0) {
                Ꮡb.WriteString("&"u8);
            }
            @string p = fmt.Sprintf("p%v"u8, i);
            Ꮡb.WriteString(p);
            want[p] = new @string[]{""u8}.slice();
        }
        @string query = b.String();
        var (got, err) = ParseQuery(query);
        {
            var (gotErr, wantErr) = (err != default!, test.wantErr); if (gotErr != wantErr) {
                Ꮡt.Errorf("GODEBUG=%v ParseQuery(%v params) = %v, want error: %v"u8, test.godebug, test.@params, err, wantErr);
            }
        }
        if (err != default!) {
            continue;
        }
        {
            nint gotΔ1 = len(got);
            nint wantΔ1 = test.@params; if (gotΔ1 != wantΔ1) {
                Ꮡt.Errorf("GODEBUG=%v ParseQuery(%v params): got %v params, want %v"u8, test.godebug, test.@params, gotΔ1, wantΔ1);
            }
        }
    }
}

[GoType] public partial struct RequestURITest {
    internal ж<global::go.net.url_package.URL> url;
    internal @string @out;
}

// golang.org/issue/4860 variant 1
// golang.org/issue/4860 variant 2
// better fix for issue 4860
// ignored because doesn't match Path
// ignored because invalid
// ignored because invalid
internal static slice<RequestURITest> requritests = new RequestURITest[]{
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: ""u8
        )),
        "/"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/a b"u8
        )),
        "/a%20b"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Opaque: "/%2F/%2F/"u8
        )),
        "/%2F/%2F/"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Opaque: "//other.example.com/%2F/%2F/"u8
        )),
        "http://other.example.com/%2F/%2F/"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/////"u8,
            RawPath: "/%2F/%2F/"u8
        )),
        "/%2F/%2F/"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/////"u8,
            RawPath: "/WRONG/"u8
        )),
        "/////"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/a b"u8,
            RawQuery: "q=go+language"u8
        )),
        "/a%20b?q=go+language"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/a b"u8,
            RawPath: "/a b"u8,
            RawQuery: "q=go+language"u8
        )),
        "/a%20b?q=go+language"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/a?b"u8,
            RawPath: "/a?b"u8,
            RawQuery: "q=go+language"u8
        )),
        "/a%3Fb?q=go+language"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "myschema"u8,
            Opaque: "opaque"u8
        )),
        "opaque"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "myschema"u8,
            Opaque: "opaque"u8,
            RawQuery: "q=go+language"u8
        )),
        "opaque?q=go+language"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "//foo"u8
        )),
        "//foo"u8
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http"u8,
            Host: "example.com"u8,
            Path: "/foo"u8,
            ForceQuery: true
        )),
        "/foo?"u8
    )
}.slice();

public static void TestRequestURI(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in requritests) {
        @string s = tt.url.RequestURI();
        if (s != tt.@out) {
            Ꮡt.Errorf("%#v.RequestURI() == %q (expected %q)"u8, tt.url.OrTypedNil(), s, tt.@out);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string urlᶜ = "%gh&%ij"u8;

public static void TestParseFailure(ж<testing.T> Ꮡt) {
    // Test that the first parse error is returned.
    @string url = urlᶜ;
    var (_, err) = ParseQuery(url);
    @string errStr = fmt.Sprint(err);
    if (!strings.Contains(errStr, "%gh"u8)) {
        Ꮡt.Errorf(@"ParseQuery(%q) returned error %q, want something containing %q"""u8, url, errStr, (@string)"%gh"u8);
    }
}

[GoType("dyn")] internal partial struct TestParseErrors_tests {
    internal @string @in;
    internal bool wantErr;
}

public static void TestParseErrors(ж<testing.T> Ꮡt) {
    var tests = new TestParseErrors_tests[]{
        new("http://[::1]"u8, false),
        new("http://[::1]:80"u8, false),
        new("http://[::1]:namedport"u8, true), // rfc3986 3.2.3

        new("http://x:namedport"u8, true), // rfc3986 3.2.3

        new("http://[::1]/"u8, false),
        new("http://[::1]a"u8, true),
        new("http://[::1]%23"u8, true),
        new("http://[::1%25en0]"u8, false), // valid zone id

        new("http://[::1]:"u8, false), // colon, but no port OK

        new("http://x:"u8, false), // colon, but no port OK

        new("http://[::1]:%38%30"u8, true), // not allowed: % encoding only for non-ASCII

        new("http://[::1%25%41]"u8, false), // RFC 6874 allows over-escaping in zone

        new("http://[%10::1]"u8, true), // no %xx escapes in IP address

        new("http://[::1]/%48"u8, false), // %xx in path is fine

        new("http://%41:8080/"u8, true), // not allowed: % encoding only for non-ASCII

        new("mysql://x@y(z:123)/foo"u8, true), // not well-formed per RFC 3986, golang.org/issue/33646

        new("mysql://x@y(1.2.3.4:123)/foo"u8, true),
        new(" http://foo.com"u8, true), // invalid character in schema

        new("ht tp://foo.com"u8, true), // invalid character in schema

        new("ahttp://foo.com"u8, false), // valid schema characters

        new("1http://foo.com"u8, true), // invalid character in schema

        new("http://[]%20%48%54%54%50%2f%31%2e%31%0a%4d%79%48%65%61%64%65%72%3a%20%31%32%33%0a%0a/"u8, true), // golang.org/issue/11208

        new("http://a b.com/"u8, true), // no space in host name please

        new("cache_object://foo"u8, true), // scheme cannot have _, relative path cannot have : in first segment

        new("cache_object:foo"u8, true),
        new("cache_object:foo/bar"u8, true),
        new("cache_object/:foo/bar"u8, false),
        new("http://[192.168.0.1]/"u8, true), // IPv4 in brackets

        new("http://[192.168.0.1]:8080/"u8, true), // IPv4 in brackets with port

        new("http://[::ffff:192.168.0.1]/"u8, false), // IPv4-mapped IPv6 in brackets

        new("http://[::ffff:192.168.0.1000]/"u8, true), // Out of range IPv4-mapped IPv6 in brackets

        new("http://[::ffff:192.168.0.1]:8080/"u8, false), // IPv4-mapped IPv6 in brackets with port

        new("http://[::ffff:c0a8:1]/"u8, false), // IPv4-mapped IPv6 in brackets (hex)

        new("http://[not-an-ip]/"u8, true), // invalid IP string in brackets

        new("http://[fe80::1%foo]/"u8, true), // invalid zone format in brackets

        new("http://[fe80::1"u8, true), // missing closing bracket

        new("http://fe80::1]/"u8, true), // missing opening bracket

        new("http://[test.com]/"u8, true)
    }.slice();
    // domain name in brackets
    foreach (var (_, tt) in tests) {
        var (u, err) = Parse(tt.@in);
        if (tt.wantErr) {
            if (err == default!) {
                Ꮡt.Errorf("Parse(%q) = %#v; want an error"u8, tt.@in, u.OrTypedNil());
            }
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("Parse(%q) = %v; want no error"u8, tt.@in, err);
        }
    }
}

// Issue 11202
public static void TestStarRequest(ж<testing.T> Ꮡt) {
    var (u, err) = Parse("*"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = u.RequestURI();
        @string want = "*"u8; if (got != want) {
            Ꮡt.Errorf("RequestURI = %q; want %q"u8, got, want);
        }
    }
}

[GoType] internal partial struct shouldEscapeTest {
    internal byte @in;
    internal global::go.net.url_package.encoding mode;
    internal bool escape;
}

// Unreserved characters (§2.3)
// User information (§3.2.1)
// Host (IP address, IPv6 address, registered name, port suffix; §3.2.2)
internal static slice<shouldEscapeTest> shouldEscapeTests = new shouldEscapeTest[]{
    new((rune)'a', encodePath, false),
    new((rune)'a', encodeUserPassword, false),
    new((rune)'a', encodeQueryComponent, false),
    new((rune)'a', encodeFragment, false),
    new((rune)'a', encodeHost, false),
    new((rune)'z', encodePath, false),
    new((rune)'A', encodePath, false),
    new((rune)'Z', encodePath, false),
    new((rune)'0', encodePath, false),
    new((rune)'9', encodePath, false),
    new((rune)'-', encodePath, false),
    new((rune)'-', encodeUserPassword, false),
    new((rune)'-', encodeQueryComponent, false),
    new((rune)'-', encodeFragment, false),
    new((rune)'.', encodePath, false),
    new((rune)'_', encodePath, false),
    new((rune)'~', encodePath, false),
    new((rune)':', encodeUserPassword, true),
    new((rune)'/', encodeUserPassword, true),
    new((rune)'?', encodeUserPassword, true),
    new((rune)'@', encodeUserPassword, true),
    new((rune)'$', encodeUserPassword, false),
    new((rune)'&', encodeUserPassword, false),
    new((rune)'+', encodeUserPassword, false),
    new((rune)',', encodeUserPassword, false),
    new((rune)';', encodeUserPassword, false),
    new((rune)'=', encodeUserPassword, false),
    new((rune)'!', encodeHost, false),
    new((rune)'$', encodeHost, false),
    new((rune)'&', encodeHost, false),
    new((rune)'\'', encodeHost, false),
    new((rune)'(', encodeHost, false),
    new((rune)')', encodeHost, false),
    new((rune)'*', encodeHost, false),
    new((rune)'+', encodeHost, false),
    new((rune)',', encodeHost, false),
    new((rune)';', encodeHost, false),
    new((rune)'=', encodeHost, false),
    new((rune)':', encodeHost, false),
    new((rune)'[', encodeHost, false),
    new((rune)']', encodeHost, false),
    new((rune)'0', encodeHost, false),
    new((rune)'9', encodeHost, false),
    new((rune)'A', encodeHost, false),
    new((rune)'z', encodeHost, false),
    new((rune)'_', encodeHost, false),
    new((rune)'-', encodeHost, false),
    new((rune)'.', encodeHost, false)
}.slice();

public static void TestShouldEscape(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in shouldEscapeTests) {
        if (shouldEscape(tt.@in, tt.mode) != tt.escape) {
            Ꮡt.Errorf("shouldEscape(%q, %v) returned %v; expected %v"u8, tt.@in, tt.mode, !tt.escape, tt.escape);
        }
    }
}

[GoType] internal partial struct timeoutError {
    internal bool timeout;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeoutErrorˢ = "timeout error"u8;

[GoRecv] internal static @string Error(this ref timeoutError e) {
    return timeoutErrorˢ;
}

[GoRecv] internal static bool Timeout(this ref timeoutError e) {
    return e.timeout;
}

[GoType] internal partial struct temporaryError {
    internal bool temporary;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string temporaryErrorˢ = "temporary error"u8;

[GoRecv] internal static @string Error(this ref temporaryError e) {
    return temporaryErrorˢ;
}

[GoRecv] internal static bool Temporary(this ref temporaryError e) {
    return e.temporary;
}

[GoType] internal partial struct timeoutTemporaryError {
    internal partial ref timeoutError timeoutError { get; }
    internal partial ref temporaryError temporaryError { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeoutTemporaryErrorˢ = "timeout/temporary error"u8;

[GoRecv] internal static @string Error(this ref timeoutTemporaryError e) {
    return timeoutTemporaryErrorˢ;
}


[GoType("dyn")] partial struct netErrorTestsᴛ1 {
    internal error err;
    internal bool timeout;
    internal bool temporary;
}
internal static slice<netErrorTestsᴛ1> netErrorTests = new netErrorTestsᴛ1[]{new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.timeoutErrorжerror(Ꮡ(new timeoutError(timeout: true)))))),
    timeout: true,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.timeoutErrorжerror(Ꮡ(new timeoutError(timeout: false)))))),
    timeout: false,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.temporaryErrorжerror(Ꮡ(new temporaryError(temporary: true)))))),
    timeout: false,
    temporary: true
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.temporaryErrorжerror(Ꮡ(new temporaryError(temporary: false)))))),
    timeout: false,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: true), new temporaryError(temporary: true))))))),
    timeout: true,
    temporary: true
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: false), new temporaryError(temporary: true))))))),
    timeout: false,
    temporary: true
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: true), new temporaryError(temporary: false))))))),
    timeout: true,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: false), new temporaryError(temporary: false))))))),
    timeout: false,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get"u8, "http://google.com/"u8, io.EOF))),
    timeout: false,
    temporary: false
)
}.slice();

// Test that url.Error implements net.Error and that it forwards
public static void TestURLErrorImplementsNetError(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in netErrorTests) {
        var (err, ok) = tt.err._<netꓸError>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("%d: %T does not implement net.Error"u8, i + 1, tt.err);
            continue;
        }
        if (err.Timeout() != tt.timeout) {
            Ꮡt.Errorf("%d: err.Timeout(): got %v, want %v"u8, i + 1, err.Timeout(), tt.timeout);
            continue;
        }
        if (err.Temporary() != tt.temporary) {
            Ꮡt.Errorf("%d: err.Temporary(): got %v, want %v"u8, i + 1, err.Temporary(), tt.temporary);
        }
    }
}

[GoType("dyn")] internal partial struct TestURLHostnameAndPort_tests {
    internal @string @in; // URL.Host field
    internal @string host;
    internal @string port;
}

public static void TestURLHostnameAndPort(ж<testing.T> Ꮡt) {
    var tests = new TestURLHostnameAndPort_tests[]{
        new("foo.com:80"u8, "foo.com"u8, "80"u8),
        new("foo.com"u8, "foo.com"u8, ""u8),
        new("foo.com:"u8, "foo.com"u8, ""u8),
        new("FOO.COM"u8, "FOO.COM"u8, ""u8), // no canonicalization

        new("1.2.3.4"u8, "1.2.3.4"u8, ""u8),
        new("1.2.3.4:80"u8, "1.2.3.4"u8, "80"u8),
        new("[1:2:3:4]"u8, "1:2:3:4"u8, ""u8),
        new("[1:2:3:4]:80"u8, "1:2:3:4"u8, "80"u8),
        new("[::1]:80"u8, "::1"u8, "80"u8),
        new("[::1]"u8, "::1"u8, ""u8),
        new("[::1]:"u8, "::1"u8, ""u8),
        new("localhost"u8, "localhost"u8, ""u8),
        new("localhost:443"u8, "localhost"u8, "443"u8),
        new("some.super.long.domain.example.org:8080"u8, "some.super.long.domain.example.org"u8, "8080"u8),
        new("[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:17000"u8, "2001:0db8:85a3:0000:0000:8a2e:0370:7334"u8, "17000"u8),
        new("[2001:0db8:85a3:0000:0000:8a2e:0370:7334]"u8, "2001:0db8:85a3:0000:0000:8a2e:0370:7334"u8, ""u8), // Ensure that even when not valid, Host is one of "Hostname",
 // "Hostname:Port", "[Hostname]" or "[Hostname]:Port".
 // See https://golang.org/issue/29098.

        new("[google.com]:80"u8, "google.com"u8, "80"u8),
        new("google.com]:80"u8, "google.com]"u8, "80"u8),
        new("google.com:80_invalid_port"u8, "google.com:80_invalid_port"u8, ""u8),
        new("[::1]extra]:80"u8, "::1]extra"u8, "80"u8),
        new("google.com]extra:extra"u8, "google.com]extra:extra"u8, ""u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var u = Ꮡ(new URL(Host: tt.@in));
        @string host = u.Hostname();
        @string port = u.Port();
        if (host != tt.host) {
            Ꮡt.Errorf("Hostname for Host %q = %q; want %q"u8, tt.@in, host, tt.host);
        }
        if (port != tt.port) {
            Ꮡt.Errorf("Port for Host %q = %q; want %q"u8, tt.@in, port, tt.port);
        }
    }
}

internal static encodingPkg.BinaryMarshaler _ᴛ1ʗ = new url_test_package.url_URLжBinaryMarshaler(((ж<global::go.net.url_package.URL>)nil));

internal static encodingPkg.BinaryUnmarshaler _ᴛ2ʗ = new url_test_package.url_URLжBinaryUnmarshaler(((ж<global::go.net.url_package.URL>)nil));

internal static encodingPkg.BinaryAppender _ᴛ3ʗ = new url_test_package.url_URLжBinaryAppender(((ж<global::go.net.url_package.URL>)nil));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsWwwGoogleComXYZˢ = "https://www.google.com/x?y=z"u8;

public static void TestJSON(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (u, err) = Parse(httpsWwwGoogleComXYZˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var js, err) = json.Marshal(u.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // If only we could implement TextMarshaler/TextUnmarshaler,
    // this would work:
    //
    // if string(js) != strconv.Quote(u.String()) {
    // 	t.Errorf("json encoding: %s\nwant: %s\n", js, strconv.Quote(u.String()))
    // }
    var u1 = @new<global::go.net.url_package.URL>();
    err = json.Unmarshal(js, u1.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (u1.String() != u.String()) {
        Ꮡt.Errorf("json decoded to: %s\nwant: %s\n"u8, u1.OrTypedNil(), u.OrTypedNil());
    }
}

public static void TestGob(ж<testing.T> Ꮡt) {
    var (u, err) = Parse(httpsWwwGoogleComXYZˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
    err = gob.NewEncoder(new url_test_package.bytes_BufferжWriter(Ꮡw)).Encode(u.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var u1 = @new<global::go.net.url_package.URL>();
    err = gob.NewDecoder(new url_test_package.bytes_BufferжReader(Ꮡw)).Decode(u1.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (u1.String() != u.String()) {
        Ꮡt.Errorf("json decoded to: %s\nwant: %s\n"u8, u1.OrTypedNil(), u.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFooComˢ = "http://foo.com/"u8;

public static void TestNilUser(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var v = recover(); if (v != default!) {
                    Ꮡt.Fatalf("unexpected panic: %v"u8, v);
                }
            }
        }, ref ᒐ);
        var (u, err) = Parse(httpFooComˢ);
        if (err != default!) {
            Ꮡt.Fatalf("parse err: %v"u8, err);
        }
        {
            @string v = (~u).User.Username(); if (v != ""u8) {
                Ꮡt.Fatalf("expected empty username, got %s"u8, v);
            }
        }
        {
            var (v, ok) = (~u).User.Password(); if (v != ""u8 || ok) {
                Ꮡt.Fatalf("expected empty password, got %s (%v)"u8, v, ok);
            }
        }
        {
            @string v = (~u).User.String(); if (v != ""u8) {
                Ꮡt.Fatalf("expected empty string, got %s"u8, v);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpUserPasswoRdFooComˢ = "http://user^:passwo^rd@foo.com/"u8;

public static void TestInvalidUserPassword(ж<testing.T> Ꮡt) {
    var (_, err) = Parse(httpUserPasswoRdFooComˢ);
    {
        @string got = fmt.Sprint(err);
        @string wantsub = netUrlInvalidUserinfoˢ; if (!strings.Contains(got, wantsub)) {
            Ꮡt.Errorf("error = %q; want substring %q"u8, got, wantsub);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantSubᶜ = "net/url: invalid control character in URL"u8;

public static void TestRejectControlCharacters(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new @string[]{
        "http://foo.com/?foo\nbar"u8,
        "http\r://foo.com/"u8,
        "http://foo\x7f.com/"u8
    }.slice();
    foreach (var (_, s) in tests) {
        var (_, err) = Parse(s);
        @string wantSub = wantSubᶜ;
        {
            @string got = fmt.Sprint(err); if (!strings.Contains(got, wantSub)) {
                Ꮡt.Errorf("Parse(%q) error = %q; want substring %q"u8, s, got, wantSub);
            }
        }
    }
    // But don't reject non-ASCII CTLs, at least for now:
    {
        var (_, err) = Parse(((@string)(new byte[]{0x68, 0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x66, 0x6f, 0x6f, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x63, 0x74, 0x6c, 0x80}))); if (err != default!) {
            Ꮡt.Errorf("error parsing URL with non-ASCII control byte: %v"u8, err);
        }
    }
}


[GoType("dyn")] partial struct escapeBenchmarksᴛ1 {
    internal @string unescaped;
    internal @string query;
    internal @string path;
}
internal static slice<escapeBenchmarksᴛ1> escapeBenchmarks = new escapeBenchmarksᴛ1[]{
    new(
        unescaped: "one two"u8,
        query: "one+two"u8,
        path: "one%20two"u8
    ),
    new(
        unescaped: "Фотки собак"u8,
        query: "%D0%A4%D0%BE%D1%82%D0%BA%D0%B8+%D1%81%D0%BE%D0%B1%D0%B0%D0%BA"u8,
        path: "%D0%A4%D0%BE%D1%82%D0%BA%D0%B8%20%D1%81%D0%BE%D0%B1%D0%B0%D0%BA"u8
    ),
    new(
        unescaped: "shortrun(break)shortrun"u8,
        query: "shortrun%28break%29shortrun"u8,
        path: "shortrun%28break%29shortrun"u8
    ),
    new(
        unescaped: "longerrunofcharacters(break)anotherlongerrunofcharacters"u8,
        query: "longerrunofcharacters%28break%29anotherlongerrunofcharacters"u8,
        path: "longerrunofcharacters%28break%29anotherlongerrunofcharacters"u8
    ),
    new(
        unescaped: strings.Repeat("padded/with+various%characters?that=need$some@escaping+paddedsowebreak/256bytes"u8, 4),
        query: strings.Repeat("padded%2Fwith%2Bvarious%25characters%3Fthat%3Dneed%24some%40escaping%2Bpaddedsowebreak%2F256bytes"u8, 4),
        path: strings.Repeat("padded%2Fwith+various%25characters%3Fthat=need$some@escaping+paddedsowebreak%2F256bytes"u8, 4)
    )
}.slice();

public static void BenchmarkQueryEscape(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default!;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                g = QueryEscape(tcʗ1.unescaped);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.query) {
                bΔ1.Errorf("QueryEscape(%q) == %q, want %q"u8, tcʗ1.unescaped, g, tcʗ1.query);
            }
        });
    }
}

public static void BenchmarkPathEscape(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default!;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                g = PathEscape(tcʗ1.unescaped);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.path) {
                bΔ1.Errorf("PathEscape(%q) == %q, want %q"u8, tcʗ1.unescaped, g, tcʗ1.path);
            }
        });
    }
}

public static void BenchmarkQueryUnescape(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default!;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (g, _) = QueryUnescape(tcʗ1.query);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.unescaped) {
                bΔ1.Errorf("QueryUnescape(%q) == %q, want %q"u8, tcʗ1.query, g, tcʗ1.unescaped);
            }
        });
    }
}

public static void BenchmarkPathUnescape(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default!;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (g, _) = PathUnescape(tcʗ1.path);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.unescaped) {
                bΔ1.Errorf("PathUnescape(%q) == %q, want %q"u8, tcʗ1.path, g, tcʗ1.unescaped);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ2 = "nil"u8;
internal static readonly @string nonNilErrorˢ = "non-nil error"u8;

[GoType("dyn")] internal partial struct TestJoinPath_tests {
    internal @string @base;
    internal slice<@string> elem;
    internal @string @out;
}

public static void TestJoinPath(ж<testing.T> Ꮡt) {
    var tests = new TestJoinPath_tests[]{
        new(
            @base: "https://go.googlesource.com"u8,
            elem: new @string[]{"go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com/a/b/c"u8,
            elem: new @string[]{"../../../go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com/"u8,
            elem: new @string[]{"../go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com"u8,
            elem: new @string[]{"../go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com"u8,
            elem: new @string[]{"../go"u8, "../../go"u8, "../../../go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com/../go"u8,
            elem: default!,
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com/"u8,
            elem: new @string[]{"./go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com//"u8,
            elem: new @string[]{"/go"u8}.slice(),
            @out: "https://go.googlesource.com/go"u8
        ),
        new(
            @base: "https://go.googlesource.com//"u8,
            elem: new @string[]{"/go"u8, "a"u8, "b"u8, "c"u8}.slice(),
            @out: "https://go.googlesource.com/go/a/b/c"u8
        ),
        new(
            @base: "http://[fe80::1%en0]:8080/"u8,
            elem: new @string[]{"/go"u8}.slice()
        ),
        new(
            @base: "https://go.googlesource.com"u8,
            elem: new @string[]{"go/"u8}.slice(),
            @out: "https://go.googlesource.com/go/"u8
        ),
        new(
            @base: "https://go.googlesource.com"u8,
            elem: new @string[]{"go//"u8}.slice(),
            @out: "https://go.googlesource.com/go/"u8
        ),
        new(
            @base: "https://go.googlesource.com"u8,
            elem: default!,
            @out: "https://go.googlesource.com/"u8
        ),
        new(
            @base: "https://go.googlesource.com/"u8,
            elem: default!,
            @out: "https://go.googlesource.com/"u8
        ),
        new(
            @base: "https://go.googlesource.com/a%2fb"u8,
            elem: new @string[]{"c"u8}.slice(),
            @out: "https://go.googlesource.com/a%2fb/c"u8
        ),
        new(
            @base: "https://go.googlesource.com/a%2fb"u8,
            elem: new @string[]{"c%2fd"u8}.slice(),
            @out: "https://go.googlesource.com/a%2fb/c%2fd"u8
        ),
        new(
            @base: "https://go.googlesource.com/a/b"u8,
            elem: new @string[]{"/go"u8}.slice(),
            @out: "https://go.googlesource.com/a/b/go"u8
        ),
        new(
            @base: "/"u8,
            elem: default!,
            @out: "/"u8
        ),
        new(
            @base: "a"u8,
            elem: default!,
            @out: "a"u8
        ),
        new(
            @base: "a"u8,
            elem: new @string[]{"b"u8}.slice(),
            @out: "a/b"u8
        ),
        new(
            @base: "a"u8,
            elem: new @string[]{"../b"u8}.slice(),
            @out: "b"u8
        ),
        new(
            @base: "a"u8,
            elem: new @string[]{"../../b"u8}.slice(),
            @out: "b"u8
        ),
        new(
            @base: ""u8,
            elem: new @string[]{"a"u8}.slice(),
            @out: "a"u8
        ),
        new(
            @base: ""u8,
            elem: new @string[]{"../a"u8}.slice(),
            @out: "a"u8
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        @string wantErr = nilˢ2;
        if (tt.@out == ""u8) {
            wantErr = nonNilErrorˢ;
        }
        {
            var (outΔ1, errΔ1) = JoinPath(tt.@base, tt.elem.ꓸꓸꓸ); if (outΔ1 != tt.@out || (errΔ1 == default!) != (tt.@out != ""u8)) {
                Ꮡt.Errorf("JoinPath(%q, %q) = %q, %v, want %q, %v"u8, tt.@base, tt.elem, outΔ1, errΔ1, tt.@out, wantErr);
            }
        }
        @string @out = default!;
        var (u, err) = Parse(tt.@base);
        if (err == default!) {
            u = u.JoinPath(tt.elem.ꓸꓸꓸ);
            @out = u.String();
        }
        if (@out != tt.@out || (err == default!) != (tt.@out != ""u8)) {
            Ꮡt.Errorf("Parse(%q).JoinPath(%q) = %q, %v, want %q, %v"u8, tt.@base, tt.elem, @out, err, tt.@out, wantErr);
        }
    }
}

} // end url_internal_test_package
