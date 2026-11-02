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
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using encoding = encoding_package;
using go.encoding;
using static go.net.url_package;

partial class url_internal_test_package {

public partial struct URLTest {
    internal @string @in;
    internal ж<global::go.net.url_package.URL> @out; // expected parse
    internal @string roundtrip;                      // expected result of reserializing the URL; empty means same as "in".
}

internal static slice<URLTest> urltests = slice<URLTest>([
    // no path
    new(
        "http://www.google.com",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com"
        )),
        ""
    ),
    // path
    new(
        "http://www.google.com/",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/"
        )),
        ""
    ),
    // path with hex escaping
    new(
        "http://www.google.com/file%20one%26two",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/file one&two",
            RawPath: "/file%20one%26two"
        )),
        ""
    ),
    // fragment with hex escaping
    new(
        "http://www.google.com/#file%20one%26two",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            Fragment: "file one&two",
            RawFragment: "file%20one%26two"
        )),
        ""
    ),
    // user
    new(
        "ftp://webmaster@www.google.com/",
        Ꮡ(new URL(
            Scheme: "ftp",
            User: User("webmaster"),
            Host: "www.google.com",
            Path: "/"
        )),
        ""
    ),
    // escape sequence in username
    new(
        "ftp://john%20doe@www.google.com/",
        Ꮡ(new URL(
            Scheme: "ftp",
            User: User("john doe"),
            Host: "www.google.com",
            Path: "/"
        )),
        "ftp://john%20doe@www.google.com/"
    ),
    // empty query
    new(
        "http://www.google.com/?",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            ForceQuery: true
        )),
        ""
    ),
    // query ending in question mark (Issue 14573)
    new(
        "http://www.google.com/?foo=bar?",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            RawQuery: "foo=bar?"
        )),
        ""
    ),
    // query
    new(
        "http://www.google.com/?q=go+language",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            RawQuery: "q=go+language"
        )),
        ""
    ),
    // query with hex escaping: NOT parsed
    new(
        "http://www.google.com/?q=go%20language",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            RawQuery: "q=go%20language"
        )),
        ""
    ),
    // %20 outside query
    new(
        "http://www.google.com/a%20b?q=c+d",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/a b",
            RawQuery: "q=c+d"
        )),
        ""
    ),
    // path without leading /, so no parsing
    new(
        "http:www.google.com/?q=go+language",
        Ꮡ(new URL(
            Scheme: "http",
            Opaque: "www.google.com/",
            RawQuery: "q=go+language"
        )),
        "http:www.google.com/?q=go+language"
    ),
    // path without leading /, so no parsing
    new(
        "http:%2f%2fwww.google.com/?q=go+language",
        Ꮡ(new URL(
            Scheme: "http",
            Opaque: "%2f%2fwww.google.com/",
            RawQuery: "q=go+language"
        )),
        "http:%2f%2fwww.google.com/?q=go+language"
    ),
    // non-authority with path; see golang.org/issue/46059
    new(
        "mailto:/webmaster@golang.org",
        Ꮡ(new URL(
            Scheme: "mailto",
            Path: "/webmaster@golang.org",
            OmitHost: true
        )),
        ""
    ),
    // non-authority
    new(
        "mailto:webmaster@golang.org",
        Ꮡ(new URL(
            Scheme: "mailto",
            Opaque: "webmaster@golang.org"
        )),
        ""
    ),
    // unescaped :// in query should not create a scheme
    new(
        "/foo?query=http://bad",
        Ꮡ(new URL(
            Path: "/foo",
            RawQuery: "query=http://bad"
        )),
        ""
    ),
    // leading // without scheme should create an authority
    new(
        "//foo",
        Ꮡ(new URL(
            Host: "foo"
        )),
        ""
    ),
    // leading // without scheme, with userinfo, path, and query
    new(
        "//user@foo/path?a=b",
        Ꮡ(new URL(
            User: User("user"),
            Host: "foo",
            Path: "/path",
            RawQuery: "a=b"
        )),
        ""
    ),
    // Three leading slashes isn't an authority, but doesn't return an error.
    // (We can't return an error, as this code is also used via
    // ServeHTTP -> ReadRequest -> Parse, which is arguably a
    // different URL parsing context, but currently shares the
    // same codepath)
    new(
        "///threeslashes",
        Ꮡ(new URL(
            Path: "///threeslashes"
        )),
        ""
    ),
    new(
        "http://user:password@google.com",
        Ꮡ(new URL(
            Scheme: "http",
            User: UserPassword("user", "password"),
            Host: "google.com"
        )),
        "http://user:password@google.com"
    ),
    // unescaped @ in username should not confuse host
    new(
        "http://j@ne:password@google.com",
        Ꮡ(new URL(
            Scheme: "http",
            User: UserPassword("j@ne", "password"),
            Host: "google.com"
        )),
        "http://j%40ne:password@google.com"
    ),
    // unescaped @ in password should not confuse host
    new(
        "http://jane:p@ssword@google.com",
        Ꮡ(new URL(
            Scheme: "http",
            User: UserPassword("jane", "p@ssword"),
            Host: "google.com"
        )),
        "http://jane:p%40ssword@google.com"
    ),
    new(
        "http://j@ne:password@google.com/p@th?q=@go",
        Ꮡ(new URL(
            Scheme: "http",
            User: UserPassword("j@ne", "password"),
            Host: "google.com",
            Path: "/p@th",
            RawQuery: "q=@go"
        )),
        "http://j%40ne:password@google.com/p@th?q=@go"
    ),
    new(
        "http://www.google.com/?q=go+language#foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            RawQuery: "q=go+language",
            Fragment: "foo"
        )),
        ""
    ),
    new(
        "http://www.google.com/?q=go+language#foo&bar",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            RawQuery: "q=go+language",
            Fragment: "foo&bar"
        )),
        "http://www.google.com/?q=go+language#foo&bar"
    ),
    new(
        "http://www.google.com/?q=go+language#foo%26bar",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "/",
            RawQuery: "q=go+language",
            Fragment: "foo&bar",
            RawFragment: "foo%26bar"
        )),
        "http://www.google.com/?q=go+language#foo%26bar"
    ),
    new(
        "file:///home/adg/rabbits",
        Ꮡ(new URL(
            Scheme: "file",
            Host: "",
            Path: "/home/adg/rabbits"
        )),
        "file:///home/adg/rabbits"
    ),
    // "Windows" paths are no exception to the rule.
    // See golang.org/issue/6027, especially comment #9.
    new(
        "file:///C:/FooBar/Baz.txt",
        Ꮡ(new URL(
            Scheme: "file",
            Host: "",
            Path: "/C:/FooBar/Baz.txt"
        )),
        "file:///C:/FooBar/Baz.txt"
    ),
    // case-insensitive scheme
    new(
        "MaIlTo:webmaster@golang.org",
        Ꮡ(new URL(
            Scheme: "mailto",
            Opaque: "webmaster@golang.org"
        )),
        "mailto:webmaster@golang.org"
    ),
    // Relative path
    new(
        "a/b/c",
        Ꮡ(new URL(
            Path: "a/b/c"
        )),
        "a/b/c"
    ),
    // escaped '?' in username and password
    new(
        "http://%3Fam:pa%3Fsword@google.com",
        Ꮡ(new URL(
            Scheme: "http",
            User: UserPassword("?am", "pa?sword"),
            Host: "google.com"
        )),
        ""
    ),
    // host subcomponent; IPv4 address in RFC 3986
    new(
        "http://192.168.0.1/",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "192.168.0.1",
            Path: "/"
        )),
        ""
    ),
    // host and port subcomponents; IPv4 address in RFC 3986
    new(
        "http://192.168.0.1:8080/",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "192.168.0.1:8080",
            Path: "/"
        )),
        ""
    ),
    // host subcomponent; IPv6 address in RFC 3986
    new(
        "http://[fe80::1]/",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[fe80::1]",
            Path: "/"
        )),
        ""
    ),
    // host and port subcomponents; IPv6 address in RFC 3986
    new(
        "http://[fe80::1]:8080/",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[fe80::1]:8080",
            Path: "/"
        )),
        ""
    ),
    // valid IPv6 host with port and path
    new(
        "https://[2001:db8::1]:8443/test/path",
        Ꮡ(new URL(
            Scheme: "https",
            Host: "[2001:db8::1]:8443",
            Path: "/test/path"
        )),
        ""
    ),
    // host subcomponent; IPv6 address with zone identifier in RFC 6874
    new(
        "http://[fe80::1%25en0]/", // alphanum zone identifier
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[fe80::1%en0]",
            Path: "/"
        )),
        ""
    ),
    // host and port subcomponents; IPv6 address with zone identifier in RFC 6874
    new(
        "http://[fe80::1%25en0]:8080/", // alphanum zone identifier
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[fe80::1%en0]:8080",
            Path: "/"
        )),
        ""
    ),
    // host subcomponent; IPv6 address with zone identifier in RFC 6874
    new(
        "http://[fe80::1%25%65%6e%301-._~]/", // percent-encoded+unreserved zone identifier
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[fe80::1%en01-._~]",
            Path: "/"
        )),
        "http://[fe80::1%25en01-._~]/"
    ),
    // host and port subcomponents; IPv6 address with zone identifier in RFC 6874
    new(
        "http://[fe80::1%25%65%6e%301-._~]:8080/", // percent-encoded+unreserved zone identifier
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[fe80::1%en01-._~]:8080",
            Path: "/"
        )),
        "http://[fe80::1%25en01-._~]:8080/"
    ),
    // alternate escapings of path survive round trip
    new(
        "http://rest.rsc.io/foo%2fbar/baz%2Fquux?alt=media",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "rest.rsc.io",
            Path: "/foo/bar/baz/quux",
            RawPath: "/foo%2fbar/baz%2Fquux",
            RawQuery: "alt=media"
        )),
        ""
    ),
    // issue 12036
    new(
        "mysql://a,b,c/bar",
        Ꮡ(new URL(
            Scheme: "mysql",
            Host: "a,b,c",
            Path: "/bar"
        )),
        ""
    ),
    // worst case host, still round trips
    new(
        "scheme://!$&'()*+,;=hello!:1/path",
        Ꮡ(new URL(
            Scheme: "scheme",
            Host: "!$&'()*+,;=hello!:1",
            Path: "/path"
        )),
        ""
    ),
    // worst case path, still round trips
    new(
        "http://host/!$&'()*+,;=:@[hello]",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "host",
            Path: "/!$&'()*+,;=:@[hello]",
            RawPath: "/!$&'()*+,;=:@[hello]"
        )),
        ""
    ),
    // golang.org/issue/5684
    new(
        "http://example.com/oid/[order_id]",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/oid/[order_id]",
            RawPath: "/oid/[order_id]"
        )),
        ""
    ),
    // golang.org/issue/12200 (colon with empty port)
    new(
        "http://192.168.0.2:8080/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "192.168.0.2:8080",
            Path: "/foo"
        )),
        ""
    ),
    new(
        "http://192.168.0.2:/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "192.168.0.2:",
            Path: "/foo"
        )),
        ""
    ),
    new(
        "http://[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:8080/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:8080",
            Path: "/foo"
        )),
        ""
    ),
    new(
        "http://[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "[2b01:e34:ef40:7730:8e70:5aff:fefe:edac]:",
            Path: "/foo"
        )),
        ""
    ),
    // golang.org/issue/7991 and golang.org/issue/12719 (non-ascii %-encoded in host)
    new(
        "http://hello.世界.com/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "hello.世界.com",
            Path: "/foo"
        )),
        "http://hello.%E4%B8%96%E7%95%8C.com/foo"
    ),
    new(
        "http://hello.%e4%b8%96%e7%95%8c.com/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "hello.世界.com",
            Path: "/foo"
        )),
        "http://hello.%E4%B8%96%E7%95%8C.com/foo"
    ),
    new(
        "http://hello.%E4%B8%96%E7%95%8C.com/foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "hello.世界.com",
            Path: "/foo"
        )),
        ""
    ),
    // golang.org/issue/10433 (path beginning with //)
    new(
        "http://example.com//foo",
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "//foo"
        )),
        ""
    ),
    // test that we can reparse the host names we accept.
    new(
        "myscheme://authority<\"hi\">/foo",
        Ꮡ(new URL(
            Scheme: "myscheme",
            Host: "authority<\"hi\">",
            Path: "/foo"
        )),
        ""
    ),
    // spaces in hosts are disallowed but escaped spaces in IPv6 scope IDs are grudgingly OK.
    // This happens on Windows.
    // golang.org/issue/14002
    new(
        "tcp://[2020::2020:20:2020:2020%25Windows%20Loves%20Spaces]:2020",
        Ꮡ(new URL(
            Scheme: "tcp",
            Host: "[2020::2020:20:2020:2020%Windows Loves Spaces]:2020"
        )),
        ""
    ),
    // test we can roundtrip magnet url
    // fix issue https://golang.org/issue/20054
    new(
        "magnet:?xt=urn:btih:c12fe1c06bba254a9dc9f519b335aa7c1367a88a&dn",
        Ꮡ(new URL(
            Scheme: "magnet",
            Host: "",
            Path: "",
            RawQuery: "xt=urn:btih:c12fe1c06bba254a9dc9f519b335aa7c1367a88a&dn"
        )),
        "magnet:?xt=urn:btih:c12fe1c06bba254a9dc9f519b335aa7c1367a88a&dn"
    ),
    new(
        "mailto:?subject=hi",
        Ꮡ(new URL(
            Scheme: "mailto",
            Host: "",
            Path: "",
            RawQuery: "subject=hi"
        )),
        "mailto:?subject=hi"
    ),
    // PostgreSQL URLs can include a comma-separated list of host:post hosts.
    // https://go.dev/issue/75859
    new(
        "postgres://host1:1,host2:2,host3:3",
        Ꮡ(new URL(
            Scheme: "postgres",
            Host: "host1:1,host2:2,host3:3",
            Path: ""
        )),
        "postgres://host1:1,host2:2,host3:3"
    ),
    new(
        "postgresql://host1:1,host2:2,host3:3",
        Ꮡ(new URL(
            Scheme: "postgresql",
            Host: "host1:1,host2:2,host3:3",
            Path: ""
        )),
        "postgresql://host1:1,host2:2,host3:3"
    ),
    // Mongodb URLs can include a comma-separated list of host:post hosts.
    new(
        "mongodb://user:password@host1:1,host2:2,host3:3",
        Ꮡ(new URL(
            Scheme: "mongodb",
            User: UserPassword("user", "password"),
            Host: "host1:1,host2:2,host3:3",
            Path: ""
        )),
        ""
    ),
    new(
        "mongodb+srv://user:password@host1:1,host2:2,host3:3",
        Ꮡ(new URL(
            Scheme: "mongodb+srv",
            User: UserPassword("user", "password"),
            Host: "host1:1,host2:2,host3:3",
            Path: ""
        )),
        ""
    )
]);

// more useful string for debugging than fmt's struct printer
internal static @string ufmt(ж<global::go.net.url_package.URL> Ꮡu) {
    ref var u = ref Ꮡu.DerefOrNull();

    any user = default;
    any pass = default;
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
        if (err != nil) {
            Ꮡb.Errorf("Parse(%q) returned error %s", tt.@in, err);
            continue;
        }
        if (tt.roundtrip == ""u8) {
            continue;
        }
        b.StartTimer();
        @string g = default;
        for (nint i = 0; i < b.N; i++) {
            g = u.String();
        }
        b.StopTimer();
        {
            @string w = tt.roundtrip; if (b.N > 0 && g != w) {
                Ꮡb.Errorf("Parse(%q).String() == %q, want %q", tt.@in, g, w);
            }
        }
    }
}

public static void TestParse(ж<testing.T> t) {
    foreach (var (_, tt) in urltests) {
        var (u, err) = Parse(tt.@in);
        if (err != nil) {
            t.Errorf("Parse(%q) returned error %v", tt.@in, err);
            continue;
        }
        if (!reflect.DeepEqual(u.OrTypedNil(), tt.@out.OrTypedNil())) {
            t.Errorf("Parse(%q):\n\tgot  %v\n\twant %v\n", tt.@in, ufmt(u), ufmt(tt.@out));
        }
    }
}

internal static ref readonly @string pathThatLooksSchemeRelative => ref pathThatLooksSchemeRelativeᵇ;
private static readonly @string pathThatLooksSchemeRelativeᵇ = "//not.a.user@not.a.host/just/a/path"u8;

partial struct parseRequestURLTestsᴛ1 /*dyn*/ {
    internal @string url;
    internal bool expectedValid;
}
internal static slice<parseRequestURLTestsᴛ1> parseRequestURLTests = slice<parseRequestURLTestsᴛ1>([
    new("http://foo.com", true),
    new("http://foo.com/", true),
    new("http://foo.com/path", true),
    new("/", true),
    new(pathThatLooksSchemeRelative, true),
    new("//not.a.user@%66%6f%6f.com/just/a/path/also", true),
    new("*", true),
    new("http://192.168.0.1/", true),
    new("http://192.168.0.1:8080/", true),
    new("http://[fe80::1]/", true),
    new("http://[fe80::1]:8080/", true),

    // Tests exercising RFC 6874 compliance:
    new("http://[fe80::1%25en0]/", true),                 // with alphanum zone identifier
    new("http://[fe80::1%25en0]:8080/", true),            // with alphanum zone identifier
    new("http://[fe80::1%25%65%6e%301-._~]/", true),      // with percent-encoded+unreserved zone identifier
    new("http://[fe80::1%25%65%6e%301-._~]:8080/", true), // with percent-encoded+unreserved zone identifier

    new("foo.html", false),
    new("../dir/", false),
    new(" http://foo.com", false),
    new("http://192.168.0.%31/", false),
    new("http://192.168.0.%31:8080/", false),
    new("http://[fe80::%31]/", false),
    new("http://[fe80::%31]:8080/", false),
    new("http://[fe80::%31%25en0]/", false),
    new("http://[fe80::%31%25en0]:8080/", false),

    // These two cases are valid as textual representations as
    // described in RFC 4007, but are not valid as address
    // literals with IPv6 zone identifiers in URIs as described in
    // RFC 6874.
    new("http://[fe80::1%en0]/", false),
    new("http://[fe80::1%en0]:8080/", false),

    // Tests exercising RFC 3986 compliance
    new("https://[1:2:3:4:5:6:7:8]", true),             // full IPv6 address
    new("https://[2001:db8::a:b:c:d]", true),           // compressed IPv6 address
    new("https://[fe80::1%25eth0]", true),              // link-local address with zone ID (interface name)
    new("https://[fe80::abc:def%254]", true),           // link-local address with zone ID (interface index)
    new("https://[2001:db8::1]/path", true),            // compressed IPv6 address with path
    new("https://[fe80::1%25eth0]/path?query=1", true), // link-local with zone, path, and query

    new("https://[::ffff:192.0.2.1]", true),
    new("https://[:1] ", false),
    new("https://[1:2:3:4:5:6:7:8:9]", false),
    new("https://[1::1::1]", false),
    new("https://[1:2:3:]", false),
    new("https://[ffff::127.0.0.4000]", false),
    new("https://[0:0::test.com]:80", false),
    new("https://[2001:db8::test.com]", false),
    new("https://[test.com]", false),
    new("https://1:2:3:4:5:6:7:8", false),
    new("https://1:2:3:4:5:6:7:8:80", false),
    new("https://example.com:80:", false)
]);

public static void TestParseRequestURI(ж<testing.T> t) {
    foreach (var (_, test) in parseRequestURLTests) {
        var (_, errΔ1) = ParseRequestURI(test.url);
        if (test.expectedValid && errΔ1 != nil) {
            t.Errorf("ParseRequestURI(%q) gave err %v; want no error", test.url, errΔ1);
        } else if (!test.expectedValid && errΔ1 == nil) {
            t.Errorf("ParseRequestURI(%q) gave nil error; want some error", test.url);
        }
    }

    var (url, err) = ParseRequestURI(pathThatLooksSchemeRelative);
    if (err != nil) {
        t.Fatalf("Unexpected error %v", err);
    }
    if (url.Path != pathThatLooksSchemeRelative) {
        t.Errorf("ParseRequestURI path:\ngot  %q\nwant %q", url.Path, pathThatLooksSchemeRelative);
    }
}

partial struct stringURLTestsᴛ1 /*dyn*/ {
    internal global::go.net.url_package.URL url;
    internal @string want;
}
internal static slice<stringURLTestsᴛ1> stringURLTests = slice<stringURLTestsᴛ1>([
    // No leading slash on path should prepend slash on String() call
    new(
        url: new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "search"
        ),
        want: "http://www.google.com/search"
    ),
    // Relative path with first element containing ":" should be prepended with "./", golang.org/issue/17184
    new(
        url: new URL(
            Path: "this:that"
        ),
        want: "./this:that"
    ),
    // Relative path with second element containing ":" should not be prepended with "./"
    new(
        url: new URL(
            Path: "here/this:that"
        ),
        want: "here/this:that"
    ),
    // Non-relative path with first element containing ":" should not be prepended with "./"
    new(
        url: new URL(
            Scheme: "http",
            Host: "www.google.com",
            Path: "this:that"
        ),
        want: "http://www.google.com/this:that"
    )
]);

public static void TestURLString(ж<testing.T> t) {
    foreach (var (_, tt) in urltests) {
        var (u, err) = Parse(tt.@in);
        if (err != nil) {
            t.Errorf("Parse(%q) returned error %s", tt.@in, err);
            continue;
        }
        @string expected = tt.@in;
        if (tt.roundtrip != ""u8) {
            expected = tt.roundtrip;
        }
        @string s = u.String();
        if (s != expected) {
            t.Errorf("Parse(%q).String() == %q (expected %q)", tt.@in, s, expected);
        }
    }

    foreach (var (_, vᴛ1) in stringURLTests) {
        var tt = vᴛ1;

        {
            @string got = tt.url.String(); if (got != tt.want) {
                t.Errorf("%+v.String() = %q; want %q", tt.url, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string userˢ => ref userˢᵇ;
private static readonly @string userˢᵇ = "user"u8;
internal static ref readonly @string passwordˢ => ref passwordˢᵇ;
private static readonly @string passwordˢᵇ = "password"u8;

internal partial struct TestURLRedacted_cases /*dyn*/ {
    internal @string name;
    internal ж<global::go.net.url_package.URL> url;
    internal @string want;
}

public static void TestURLRedacted(ж<testing.T> t) {
    var cases = slice<TestURLRedacted_cases>([
        new(
            name: "non-blank Password",
            url: Ꮡ(new URL(
                Scheme: "http",
                Host: "host.tld",
                Path: "this:that",
                User: UserPassword(userˢ, passwordˢ)
            )),
            want: "http://user:xxxxx@host.tld/this:that"
        ),
        new(
            name: "blank Password",
            url: Ꮡ(new URL(
                Scheme: "http",
                Host: "host.tld",
                Path: "this:that",
                User: User(userˢ)
            )),
            want: "http://user@host.tld/this:that"
        ),
        new(
            name: "nil User",
            url: Ꮡ(new URL(
                Scheme: "http",
                Host: "host.tld",
                Path: "this:that",
                User: UserPassword(""u8, passwordˢ)
            )),
            want: "http://:xxxxx@host.tld/this:that"
        ),
        new(
            name: "blank Username, blank Password",
            url: Ꮡ(new URL(
                Scheme: "http",
                Host: "host.tld",
                Path: "this:that"
            )),
            want: "http://host.tld/this:that"
        ),
        new(
            name: "empty URL",
            url: Ꮡ(new URL()),
            want: ""
        ),
        new(
            name: "nil URL",
            url: nil,
            want: ""
        )
    ]);

    foreach (var (_, vᴛ1) in cases) {
        ref var tt = ref heap(new TestURLRedacted_cases(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            {
                @string g = ttʗ1.url.Redacted();
                @string w = ttʗ1.want; if (g != w) {
                    tΔ1.Fatalf("got: %q\nwant: %q", g, w);
                }
            }
        });
    }
}

public partial struct EscapeTest {
    internal @string @in;
    internal @string @out;
    internal error err;
}

internal static slice<EscapeTest> unescapeTests = slice<EscapeTest>([
    new(
        "",
        "",
        default
    ),
    new(
        "abc",
        "abc",
        default
    ),
    new(
        "1%41",
        "1A",
        default
    ),
    new(
        "1%41%42%43",
        "1ABC",
        default
    ),
    new(
        "%4a",
        "J",
        default
    ),
    new(
        "%6F",
        "o",
        default
    ),
    new(
        "%", // not enough characters after %
        "",
        (global::go.net.url_package.EscapeError)(@string)"%"
    ),
    new(
        "%a", // not enough characters after %
        "",
        (global::go.net.url_package.EscapeError)(@string)"%a"
    ),
    new(
        "%1", // not enough characters after %
        "",
        (global::go.net.url_package.EscapeError)(@string)"%1"
    ),
    new(
        "123%45%6", // not enough characters after %
        "",
        (global::go.net.url_package.EscapeError)(@string)"%6"
    ),
    new(
        "%zzzzz", // invalid hex digits
        "",
        (global::go.net.url_package.EscapeError)(@string)"%zz"
    ),
    new(
        "a+b",
        "a b",
        default
    ),
    new(
        "a%20b",
        "a b",
        default
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxˢ => ref xxxˢᵇ;
private static readonly @string xxxˢᵇ = "XXX"u8;

public static void TestUnescape(ж<testing.T> t) {
    foreach (var (_, tt) in unescapeTests) {
        var (actual, err) = QueryUnescape(tt.@in);
        if (actual != tt.@out || (err != nil) != (tt.err != nil)) {
            t.Errorf("QueryUnescape(%q) = %q, %s; want %q, %s", tt.@in, actual, err, tt.@out, tt.err);
        }

        @string @in = tt.@in;
        @string @out = tt.@out;
        if (strings.Contains(tt.@in, "+")) {
            @in = strings.ReplaceAll(tt.@in, "+", "%20");
            var (actualΔ1, errΔ1) = PathUnescape(@in);
            if (actualΔ1 != tt.@out || (errΔ1 != nil) != (tt.err != nil)) {
                t.Errorf("PathUnescape(%q) = %q, %s; want %q, %s", @in, actualΔ1, errΔ1, tt.@out, tt.err);
            }
            if (tt.err == nil) {
                var (s, errΔ2) = QueryUnescape(strings.ReplaceAll(tt.@in, "+", xxxˢ));
                if (errΔ2 != nil) {
                    continue;
                }
                @in = tt.@in;
                @out = strings.ReplaceAll(s, xxxˢ, "+");
            }
        }

        (actual, err) = PathUnescape(@in);
        if (actual != @out || (err != nil) != (tt.err != nil)) {
            t.Errorf("PathUnescape(%q) = %q, %s; want %q, %s", @in, actual, err, @out, tt.err);
        }
    }
}

internal static slice<EscapeTest> queryEscapeTests = slice<EscapeTest>([
    new(
        "",
        "",
        default
    ),
    new(
        "abc",
        "abc",
        default
    ),
    new(
        "one two",
        "one+two",
        default
    ),
    new(
        "10%",
        "10%25",
        default
    ),
    new(
        " ?&=#+%!<>#\"{}|\\^[]`☺\t:/@$'()*,;",
        "+%3F%26%3D%23%2B%25%21%3C%3E%23%22%7B%7D%7C%5C%5E%5B%5D%60%E2%98%BA%09%3A%2F%40%24%27%28%29%2A%2C%3B",
        default
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noErrorˢ = (@string)"[no error]"u8;

public static void TestQueryEscape(ж<testing.T> t) {
    foreach (var (_, tt) in queryEscapeTests) {
        @string actual = QueryEscape(tt.@in);
        if (tt.@out != actual) {
            t.Errorf("QueryEscape(%q) = %q, want %q", tt.@in, actual, tt.@out);
        }

        // for bonus points, verify that escape:unescape is an identity.
        var (roundtrip, err) = QueryUnescape(actual);
        if (roundtrip != tt.@in || err != nil) {
            t.Errorf("QueryUnescape(%q) = %q, %s; want %q, %s", actual, roundtrip, err, tt.@in, noErrorˢ);
        }
    }
}

internal static slice<EscapeTest> pathEscapeTests = slice<EscapeTest>([
    new(
        "",
        "",
        default
    ),
    new(
        "abc",
        "abc",
        default
    ),
    new(
        "abc+def",
        "abc+def",
        default
    ),
    new(
        "a/b",
        "a%2Fb",
        default
    ),
    new(
        "one two",
        "one%20two",
        default
    ),
    new(
        "10%",
        "10%25",
        default
    ),
    new(
        " ?&=#+%!<>#\"{}|\\^[]`☺\t:/@$'()*,;",
        "%20%3F&=%23+%25%21%3C%3E%23%22%7B%7D%7C%5C%5E%5B%5D%60%E2%98%BA%09:%2F@$%27%28%29%2A%2C%3B",
        default
    )
]);

public static void TestPathEscape(ж<testing.T> t) {
    foreach (var (_, tt) in pathEscapeTests) {
        @string actual = PathEscape(tt.@in);
        if (tt.@out != actual) {
            t.Errorf("PathEscape(%q) = %q, want %q", tt.@in, actual, tt.@out);
        }

        // for bonus points, verify that escape:unescape is an identity.
        var (roundtrip, err) = PathUnescape(actual);
        if (roundtrip != tt.@in || err != nil) {
            t.Errorf("PathUnescape(%q) = %q, %s; want %q, %s", actual, roundtrip, err, tt.@in, noErrorˢ);
        }
    }
}

//var userinfoTests = []UserinfoTest{
//	{"user", "password", "user:password"},
//	{"foo:bar", "~!@#$%^&*()_+{}|[]\\-=`:;'\"<>?,./",
//		"foo%3Abar:~!%40%23$%25%5E&*()_+%7B%7D%7C%5B%5D%5C-=%60%3A;'%22%3C%3E?,.%2F"},
//}
public partial struct EncodeQueryTest {
    internal global::go.net.url_package.Values m;
    internal @string expected;
}

internal static slice<EncodeQueryTest> encodeQueryTests = slice<EncodeQueryTest>([
    new(default, ""),
    new(new Values(new map<@string, slice<@string>>{}), ""),
    new(new Values(new map<@string, slice<@string>>{["q"] = slice<@string>(["puppies"]), ["oe"] = slice<@string>(["utf8"])}), "oe=utf8&q=puppies"),
    new(new Values(new map<@string, slice<@string>>{["q"] = slice<@string>(["dogs", "&", "7"])}), "q=dogs&q=%26&q=7"),
    new(new Values(new map<@string, slice<@string>>{
        ["a"] = slice<@string>(["a1", "a2", "a3"]),
        ["b"] = slice<@string>(["b1", "b2", "b3"]),
        ["c"] = slice<@string>(["c1", "c2", "c3"])
    }), "a=a1&a=a2&a=a3&b=b1&b=b2&b=b3&c=c1&c=c2&c=c3"),
    new(new Values(new map<@string, slice<@string>>{
        ["a"] = slice<@string>(["a"]),
        ["b"] = slice<@string>(["b"]),
        ["c"] = slice<@string>(["c"]),
        ["d"] = slice<@string>(["d"]),
        ["e"] = slice<@string>(["e"]),
        ["f"] = slice<@string>(["f"]),
        ["g"] = slice<@string>(["g"]),
        ["h"] = slice<@string>(["h"]),
        ["i"] = slice<@string>(["i"])
    }), "a=a&b=b&c=c&d=d&e=e&f=f&g=g&h=h&i=i")
]);

public static void TestEncodeQuery(ж<testing.T> t) {
    foreach (var (_, tt) in encodeQueryTests) {
        {
            @string q = tt.m.Encode(); if (q != tt.expected) {
                t.Errorf(@"EncodeQuery(%+v) = %q, want %q", tt.m, q, tt.expected);
            }
        }
    }
}

public static void BenchmarkEncodeQuery(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in encodeQueryTests) {
        ref var tt = ref heap(new EncodeQueryTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        b.Run(tt.expected, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            while (bΔ1.Loop()) {
                ttʗ1.m.Encode();
            }
        });
    }
}

partial struct resolvePathTestsᴛ1 /*dyn*/ {
    internal @string @base, @ref, expected;
}
internal static slice<resolvePathTestsᴛ1> resolvePathTests = slice<resolvePathTestsᴛ1>([
    new("a/b", ".", "/a/"),
    new("a/b", "c", "/a/c"),
    new("a/b", "..", "/"),
    new("a/", "..", "/"),
    new("a/", "../..", "/"),
    new("a/b/c", "..", "/a/"),
    new("a/b/c", "../d", "/a/d"),
    new("a/b/c", ".././d", "/a/d"),
    new("a/b", "./..", "/"),
    new("a/./b", ".", "/a/"),
    new("a/../", ".", "/"),
    new("a/.././b", "c", "/c")
]);

public static void TestResolvePath(ж<testing.T> t) {
    foreach (var (_, test) in resolvePathTests) {
        @string got = resolvePath(test.@base, test.@ref);
        if (got != test.expected) {
            t.Errorf("For %q + %q got %q; expected %q", test.@base, test.@ref, got, test.expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string simpleˢ => ref simpleˢᵇ;
private static readonly @string simpleˢᵇ = "Simple"u8;
internal static ref readonly @string aBCˢ => ref aBCˢᵇ;
private static readonly @string aBCˢᵇ = "a/b/c"u8;
internal static ref readonly @string deepˢ => ref deepˢᵇ;
private static readonly @string deepˢᵇ = "Deep"u8;
internal static ref readonly @string backtrackˢ => ref backtrackˢᵇ;
private static readonly @string backtrackˢᵇ = "Backtrack"u8;

public static void BenchmarkResolvePath(ж<testing.B> b) {
    b.Run(simpleˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            resolvePath(aBCˢ, ".././d");
        }
    });
    b.Run(deepˢ, (ж<testing.B> bΔ2) => {
        @string @base = strings.Repeat("a/", 100) + "b"u8;
        @string @ref = "c";
        bΔ2.ResetTimer();
        while (bΔ2.Loop()) {
            resolvePath(@base, @ref);
        }
    });
    b.Run(backtrackˢ, (ж<testing.B> bΔ3) => {
        @string @base = strings.Repeat("a/", 100) + "b"u8;
        @string @ref = strings.Repeat("../", 50) + "c"u8;
        bΔ3.ResetTimer();
        while (bΔ3.Loop()) {
            resolvePath(@base, @ref);
        }
    });
}

partial struct resolveReferenceTestsᴛ1 /*dyn*/ {
    internal @string @base, rel, expected;
}
internal static slice<resolveReferenceTestsᴛ1> resolveReferenceTests = slice<resolveReferenceTestsᴛ1>([
    // Absolute URL references
    new("http://foo.com?a=b", "https://bar.com/", "https://bar.com/"),
    new("http://foo.com/", "https://bar.com/?a=b", "https://bar.com/?a=b"),
    new("http://foo.com/", "https://bar.com/?", "https://bar.com/?"),
    new("http://foo.com/bar", "mailto:foo@example.com", "mailto:foo@example.com"),

    // Path-absolute references
    new("http://foo.com/bar", "/baz", "http://foo.com/baz"),
    new("http://foo.com/bar?a=b#f", "/baz", "http://foo.com/baz"),
    new("http://foo.com/bar?a=b", "/baz?", "http://foo.com/baz?"),
    new("http://foo.com/bar?a=b", "/baz?c=d", "http://foo.com/baz?c=d"),

    // Multiple slashes
    new("http://foo.com/bar", "http://foo.com//baz", "http://foo.com//baz"),
    new("http://foo.com/bar", "http://foo.com///baz/quux", "http://foo.com///baz/quux"),

    // Scheme-relative
    new("https://foo.com/bar?a=b", "//bar.com/quux", "https://bar.com/quux"),

    // Path-relative references:

    // ... current directory
    new("http://foo.com", ".", "http://foo.com/"),
    new("http://foo.com/bar", ".", "http://foo.com/"),
    new("http://foo.com/bar/", ".", "http://foo.com/bar/"),

    // ... going down
    new("http://foo.com", "bar", "http://foo.com/bar"),
    new("http://foo.com/", "bar", "http://foo.com/bar"),
    new("http://foo.com/bar/baz", "quux", "http://foo.com/bar/quux"),

    // ... going up
    new("http://foo.com/bar/baz", "../quux", "http://foo.com/quux"),
    new("http://foo.com/bar/baz", "../../../../../quux", "http://foo.com/quux"),
    new("http://foo.com/bar", "..", "http://foo.com/"),
    new("http://foo.com/bar/baz", "./..", "http://foo.com/"),
    // ".." in the middle (issue 3560)
    new("http://foo.com/bar/baz", "quux/dotdot/../tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/../tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/.././tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/./../tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/dotdot/././../../tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/dotdot/./.././../tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/dotdot/dotdot/./../../.././././tail", "http://foo.com/bar/quux/tail"),
    new("http://foo.com/bar/baz", "quux/./dotdot/../dotdot/../dot/./tail/..", "http://foo.com/bar/quux/dot/"),

    // Remove any dot-segments prior to forming the target URI.
    // https://datatracker.ietf.org/doc/html/rfc3986#section-5.2.4
    new("http://foo.com/dot/./dotdot/../foo/bar", "../baz", "http://foo.com/dot/baz"),

    // Triple dot isn't special
    new("http://foo.com/bar", "...", "http://foo.com/..."),

    // Fragment
    new("http://foo.com/bar", ".#frag", "http://foo.com/#frag"),
    new("http://example.org/", "#!$&%27()*+,;=", "http://example.org/#!$&%27()*+,;="),

    // Paths with escaping (issue 16947).
    new("http://foo.com/foo%2fbar/", "../baz", "http://foo.com/baz"),
    new("http://foo.com/1/2%2f/3%2f4/5", "../../a/b/c", "http://foo.com/1/a/b/c"),
    new("http://foo.com/1/2/3", "./a%2f../../b/..%2fc", "http://foo.com/1/2/b/..%2fc"),
    new("http://foo.com/1/2%2f/3%2f4/5", "./a%2f../b/../c", "http://foo.com/1/2%2f/3%2f4/a%2f../c"),
    new("http://foo.com/foo%20bar/", "../baz", "http://foo.com/baz"),
    new("http://foo.com/foo", "../bar%2fbaz", "http://foo.com/bar%2fbaz"),
    new("http://foo.com/foo%2dbar/", "./baz-quux", "http://foo.com/foo%2dbar/baz-quux"),

    // RFC 3986: Normal Examples
    // https://datatracker.ietf.org/doc/html/rfc3986#section-5.4.1
    new("http://a/b/c/d;p?q", "g:h", "g:h"),
    new("http://a/b/c/d;p?q", "g", "http://a/b/c/g"),
    new("http://a/b/c/d;p?q", "./g", "http://a/b/c/g"),
    new("http://a/b/c/d;p?q", "g/", "http://a/b/c/g/"),
    new("http://a/b/c/d;p?q", "/g", "http://a/g"),
    new("http://a/b/c/d;p?q", "//g", "http://g"),
    new("http://a/b/c/d;p?q", "?y", "http://a/b/c/d;p?y"),
    new("http://a/b/c/d;p?q", "g?y", "http://a/b/c/g?y"),
    new("http://a/b/c/d;p?q", "#s", "http://a/b/c/d;p?q#s"),
    new("http://a/b/c/d;p?q", "g#s", "http://a/b/c/g#s"),
    new("http://a/b/c/d;p?q", "g?y#s", "http://a/b/c/g?y#s"),
    new("http://a/b/c/d;p?q", ";x", "http://a/b/c/;x"),
    new("http://a/b/c/d;p?q", "g;x", "http://a/b/c/g;x"),
    new("http://a/b/c/d;p?q", "g;x?y#s", "http://a/b/c/g;x?y#s"),
    new("http://a/b/c/d;p?q", "", "http://a/b/c/d;p?q"),
    new("http://a/b/c/d;p?q", ".", "http://a/b/c/"),
    new("http://a/b/c/d;p?q", "./", "http://a/b/c/"),
    new("http://a/b/c/d;p?q", "..", "http://a/b/"),
    new("http://a/b/c/d;p?q", "../", "http://a/b/"),
    new("http://a/b/c/d;p?q", "../g", "http://a/b/g"),
    new("http://a/b/c/d;p?q", "../..", "http://a/"),
    new("http://a/b/c/d;p?q", "../../", "http://a/"),
    new("http://a/b/c/d;p?q", "../../g", "http://a/g"),

    // RFC 3986: Abnormal Examples
    // https://datatracker.ietf.org/doc/html/rfc3986#section-5.4.2
    new("http://a/b/c/d;p?q", "../../../g", "http://a/g"),
    new("http://a/b/c/d;p?q", "../../../../g", "http://a/g"),
    new("http://a/b/c/d;p?q", "/./g", "http://a/g"),
    new("http://a/b/c/d;p?q", "/../g", "http://a/g"),
    new("http://a/b/c/d;p?q", "g.", "http://a/b/c/g."),
    new("http://a/b/c/d;p?q", ".g", "http://a/b/c/.g"),
    new("http://a/b/c/d;p?q", "g..", "http://a/b/c/g.."),
    new("http://a/b/c/d;p?q", "..g", "http://a/b/c/..g"),
    new("http://a/b/c/d;p?q", "./../g", "http://a/b/g"),
    new("http://a/b/c/d;p?q", "./g/.", "http://a/b/c/g/"),
    new("http://a/b/c/d;p?q", "g/./h", "http://a/b/c/g/h"),
    new("http://a/b/c/d;p?q", "g/../h", "http://a/b/c/h"),
    new("http://a/b/c/d;p?q", "g;x=1/./y", "http://a/b/c/g;x=1/y"),
    new("http://a/b/c/d;p?q", "g;x=1/../y", "http://a/b/c/y"),
    new("http://a/b/c/d;p?q", "g?y/./x", "http://a/b/c/g?y/./x"),
    new("http://a/b/c/d;p?q", "g?y/../x", "http://a/b/c/g?y/../x"),
    new("http://a/b/c/d;p?q", "g#s/./x", "http://a/b/c/g#s/./x"),
    new("http://a/b/c/d;p?q", "g#s/../x", "http://a/b/c/g#s/../x"),

    // Extras.
    new("https://a/b/c/d;p?q", "//g?q", "https://g?q"),
    new("https://a/b/c/d;p?q", "//g#s", "https://g#s"),
    new("https://a/b/c/d;p?q", "//g/d/e/f?y#s", "https://g/d/e/f?y#s"),
    new("https://a/b/c/d;p#s", "?y", "https://a/b/c/d;p?y"),
    new("https://a/b/c/d;p?q#s", "?y", "https://a/b/c/d;p?y"),

    // Empty path and query but with ForceQuery (issue 46033).
    new("https://a/b/c/d;p?q#s", "?", "https://a/b/c/d;p?"),

    // Opaque URLs (issue 66084).
    new("https://foo.com/bar?a=b", "http:opaque", "http:opaque"),
    new("http:opaque?x=y#zzz", "https:/foo?a=b#frag", "https:/foo?a=b#frag"),
    new("http:opaque?x=y#zzz", "https:foo:bar", "https:foo:bar"),
    new("http:opaque?x=y#zzz", "https:bar/baz?a=b#frag", "https:bar/baz?a=b#frag"),
    new("http:opaque?x=y#zzz", "https://user@host:1234?a=b#frag", "https://user@host:1234?a=b#frag"),
    new("http:opaque?x=y#zzz", "?a=b#frag", "http:opaque?a=b#frag")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string schemeOpaqueˢ => ref schemeOpaqueˢᵇ;
private static readonly @string schemeOpaqueˢᵇ = "scheme:opaque"u8;

public static void TestResolveReference(ж<testing.T> t) {
    ж<global::go.net.url_package.URL> mustParse(@string url) {
        var (u, err) = Parse(url);
        if (err != nil) {
            t.Fatalf("Parse(%q) got err %v", url, err);
        }
        return u;
    }
    var opaque = Ꮡ(new URL(Scheme: "scheme", Opaque: "opaque"));
    foreach (var (_, test) in resolveReferenceTests) {
        var @base = mustParse(test.@base);
        var rel = mustParse(test.rel);
        var url = @base.ResolveReference(rel);
        {
            @string got = url.String(); if (got != test.expected) {
                t.Errorf("URL(%q).ResolveReference(%q)\ngot  %q\nwant %q", test.@base, test.rel, got, test.expected);
            }
        }
        // Ensure that new instances are returned.
        if (@base == url) {
            t.Errorf("Expected URL.ResolveReference to return new URL instance.");
        }
        // Test the convenience wrapper too.
        (url, var err) = @base.Parse(test.rel);
        if (err != nil) {
            t.Errorf("URL(%q).Parse(%q) failed: %v", test.@base, test.rel, err);
        } else {
            @string got = url.String(); if (got != test.expected) {
                t.Errorf("URL(%q).Parse(%q)\ngot  %q\nwant %q", test.@base, test.rel, got, test.expected);
            } else if (@base == url) {
                // Ensure that new instances are returned for the wrapper too.
                t.Errorf("Expected URL.Parse to return new URL instance.");
            }
        }
        // Ensure Opaque resets the URL.
        url = @base.ResolveReference(opaque);
        if (url.Value != opaque.Value) {
            t.Errorf("ResolveReference failed to resolve opaque URL:\ngot  %#v\nwant %#v", url.OrTypedNil(), opaque.OrTypedNil());
        }
        // Test the convenience wrapper with an opaque URL too.
        (url, err) = @base.Parse(schemeOpaqueˢ);
        if (err != nil) {
            t.Errorf(@"URL(%q).Parse(""scheme:opaque"") failed: %v", test.@base, err);
        } else if (url.Value != opaque.Value) {
            t.Errorf("Parse failed to resolve opaque URL:\ngot  %#v\nwant %#v", opaque.OrTypedNil(), url.OrTypedNil());
        } else if (@base == url) {
            // Ensure that new instances are returned, again.
            t.Errorf("Expected URL.Parse to return new URL instance.");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpXComFooBarBar1Bar2ˢ => ref httpXComFooBarBar1Bar2ˢᵇ;
private static readonly @string httpXComFooBarBar1Bar2ˢᵇ = "http://x.com?foo=bar&bar=1&bar=2&baz"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = "Foo"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;
internal static ref readonly @string noexistˢ => ref noexistˢᵇ;
private static readonly @string noexistˢᵇ = "noexist"u8;

public static void TestQueryValues(ж<testing.T> t) {
    var (u, _) = Parse(httpXComFooBarBar1Bar2ˢ);
    var v = u.Query();
    if (len(v) != 3) {
        t.Errorf("got %d keys in Query values, want 3", len(v));
    }
    {
        @string g = v.Get(fooˢ);
        @string e = barˢ; if (g != e) {
            t.Errorf("Get(foo) = %q, want %q", g, e);
        }
    }
    // Case sensitive:
    {
        @string g = v.Get(fooˢ2);
        @string e = ""u8; if (g != e) {
            t.Errorf("Get(Foo) = %q, want %q", g, e);
        }
    }
    {
        @string g = v.Get(barˢ);
        @string e = "1"; if (g != e) {
            t.Errorf("Get(bar) = %q, want %q", g, e);
        }
    }
    {
        @string g = v.Get(bazˢ);
        @string e = ""u8; if (g != e) {
            t.Errorf("Get(baz) = %q, want %q", g, e);
        }
    }
    {
        var (h, e) = (v.Has(fooˢ), true); if (h != e) {
            t.Errorf("Has(foo) = %t, want %t", h, e);
        }
    }
    {
        var (h, e) = (v.Has(barˢ), true); if (h != e) {
            t.Errorf("Has(bar) = %t, want %t", h, e);
        }
    }
    {
        var (h, e) = (v.Has(bazˢ), true); if (h != e) {
            t.Errorf("Has(baz) = %t, want %t", h, e);
        }
    }
    {
        var (h, e) = (v.Has(noexistˢ), false); if (h != e) {
            t.Errorf("Has(noexist) = %t, want %t", h, e);
        }
    }
    v.Del(barˢ);
    {
        @string g = v.Get(barˢ);
        @string e = ""u8; if (g != e) {
            t.Errorf("second Get(bar) = %q, want %q", g, e);
        }
    }
}

internal partial struct parseTest {
    internal @string query;
    internal global::go.net.url_package.Values @out;
    internal bool ok;
}

internal static slice<parseTest> parseTests = slice<parseTest>([
    new(
        query: "a=1",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"])}),
        ok: true
    ),
    new(
        query: "a=1&b=2",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"]), ["b"] = slice<@string>(["2"])}),
        ok: true
    ),
    new(
        query: "a=1&a=2&a=banana",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1", "2", "banana"])}),
        ok: true
    ),
    new(
        query: "ascii=%3Ckey%3A+0x90%3E",
        @out: new Values(new map<@string, slice<@string>>{["ascii"] = slice<@string>(["<key: 0x90>"])}),
        ok: true
    ), new(
    query: "a=1;b=2",
    @out: new Values(new map<@string, slice<@string>>{}),
    ok: false
), new(
    query: "a;b=1",
    @out: new Values(new map<@string, slice<@string>>{}),
    ok: false
), new(
    query: "a=%3B", // hex encoding for semicolon
    @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>([";"])}),
    ok: true
),
    new(
        query: "a%3Bb=1",
        @out: new Values(new map<@string, slice<@string>>{["a;b"] = slice<@string>(["1"])}),
        ok: true
    ),
    new(
        query: "a=1&a=2;a=banana",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"])}),
        ok: false
    ),
    new(
        query: "a;b&c=1",
        @out: new Values(new map<@string, slice<@string>>{["c"] = slice<@string>(["1"])}),
        ok: false
    ),
    new(
        query: "a=1&b=2;a=3&c=4",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"]), ["c"] = slice<@string>(["4"])}),
        ok: false
    ),
    new(
        query: "a=1&b=2;c=3",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"])}),
        ok: false
    ),
    new(
        query: ";",
        @out: new Values(new map<@string, slice<@string>>{}),
        ok: false
    ),
    new(
        query: "a=1;",
        @out: new Values(new map<@string, slice<@string>>{}),
        ok: false
    ),
    new(
        query: "a=1&;",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"])}),
        ok: false
    ),
    new(
        query: ";a=1&b=2",
        @out: new Values(new map<@string, slice<@string>>{["b"] = slice<@string>(["2"])}),
        ok: false
    ),
    new(
        query: "a=1&b=2;",
        @out: new Values(new map<@string, slice<@string>>{["a"] = slice<@string>(["1"])}),
        ok: false
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = "<error>"u8;
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;

public static void TestParseQuery(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in parseTests) {
        ref var test = ref heap(new parseTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.query, (ж<testing.T> tΔ1) => {
            var (form, err) = ParseQuery(testʗ1.query);
            if (testʗ1.ok != (err == nil)) {
                @string want = errorˢ;
                if (testʗ1.ok) {
                    want = nilˢ;
                }
                tΔ1.Errorf("Unexpected error: %v, want %v", err, want);
            }
            if (len(form) != len(testʗ1.@out)) {
                tΔ1.Errorf("len(form) = %d, want %d", len(form), len(testʗ1.@out));
            }
            foreach (var (k, evs) in testʗ1.@out) {
                var (vs, ok) = form[k, ꟷ];
                if (!ok) {
                    tΔ1.Errorf("Missing key %q", k);
                    continue;
                }
                if (len(vs) != len(evs)) {
                    tΔ1.Errorf("len(form[%q]) = %d, want %d", k, len(vs), len(evs));
                    continue;
                }
                foreach (var (j, ev) in evs) {
                    {
                        @string v = vs[j]; if (v != ev) {
                            tΔ1.Errorf("form[%q][%d] = %q, want %q", k, j, v, ev);
                        }
                    }
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;

internal partial struct TestParseQueryLimits_type /*dyn*/ {
    internal nint @params;
    internal @string godebug;
    internal bool wantErr;
}

public static void TestParseQueryLimits(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestParseQueryLimits_type>([new(
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
        godebug: "urlmaxqueryparams=9",
        wantErr: true
    ), new(
        @params: defaultMaxParams + 1,
        godebug: "urlmaxqueryparams=0",
        wantErr: false
    )
    ])) {
        t.Setenv(godebugˢ, test.godebug);
        var want = new Values(new map<@string, slice<@string>>{});
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        foreach (var i in range(test.@params)) {
            if (i > 0) {
                Ꮡb.WriteString("&");
            }
            @string p = fmt.Sprintf("p%v"u8, i);
            Ꮡb.WriteString(p);
            want[p] = slice<@string>([""]);
        }
        @string query = b.String();
        var (got, err) = ParseQuery(query);
        {
            var (gotErr, wantErr) = (err != nil, test.wantErr); if (gotErr != wantErr) {
                t.Errorf("GODEBUG=%v ParseQuery(%v params) = %v, want error: %v", test.godebug, test.@params, err, wantErr);
            }
        }
        if (err != nil) {
            continue;
        }
        {
            nint gotΔ1 = len(got);
            nint wantΔ1 = test.@params; if (gotΔ1 != wantΔ1) {
                t.Errorf("GODEBUG=%v ParseQuery(%v params): got %v params, want %v", test.godebug, test.@params, gotΔ1, wantΔ1);
            }
        }
    }
}

public partial struct RequestURITest {
    internal ж<global::go.net.url_package.URL> url;
    internal @string @out;
}

internal static slice<RequestURITest> requritests = slice<RequestURITest>([
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: ""
        )),
        "/"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/a b"
        )),
        "/a%20b"
    ),
    // golang.org/issue/4860 variant 1
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Opaque: "/%2F/%2F/"
        )),
        "/%2F/%2F/"
    ),
    // golang.org/issue/4860 variant 2
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Opaque: "//other.example.com/%2F/%2F/"
        )),
        "http://other.example.com/%2F/%2F/"
    ),
    // better fix for issue 4860
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/////",
            RawPath: "/%2F/%2F/"
        )),
        "/%2F/%2F/"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/////",
            RawPath: "/WRONG/" // ignored because doesn't match Path
        )),
        "/////"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/a b",
            RawQuery: "q=go+language"
        )),
        "/a%20b?q=go+language"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/a b",
            RawPath: "/a b", // ignored because invalid
            RawQuery: "q=go+language"
        )),
        "/a%20b?q=go+language"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/a?b",
            RawPath: "/a?b", // ignored because invalid
            RawQuery: "q=go+language"
        )),
        "/a%3Fb?q=go+language"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "myschema",
            Opaque: "opaque"
        )),
        "opaque"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "myschema",
            Opaque: "opaque",
            RawQuery: "q=go+language"
        )),
        "opaque?q=go+language"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "//foo"
        )),
        "//foo"
    ),
    new(
        Ꮡ(new URL(
            Scheme: "http",
            Host: "example.com",
            Path: "/foo",
            ForceQuery: true
        )),
        "/foo?"
    )
]);

public static void TestRequestURI(ж<testing.T> t) {
    foreach (var (_, tt) in requritests) {
        @string s = tt.url.RequestURI();
        if (s != tt.@out) {
            t.Errorf("%#v.RequestURI() == %q (expected %q)", tt.url.OrTypedNil(), s, tt.@out);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string urlᶜ => ref urlᶜᵇ;
private static readonly @string urlᶜᵇ = "%gh&%ij"u8;

public static void TestParseFailure(ж<testing.T> t) {
    // Test that the first parse error is returned.
    @string url = urlᶜ;
    var (_, err) = ParseQuery(url);
    @string errStr = fmt.Sprint(err);
    if (!strings.Contains(errStr, "%gh")) {
        t.Errorf(@"ParseQuery(%q) returned error %q, want something containing %q""", url, errStr, (@string)"%gh");
    }
}

internal partial struct TestParseErrors_tests /*dyn*/ {
    internal @string @in;
    internal bool wantErr;
}

public static void TestParseErrors(ж<testing.T> t) {
    var tests = slice<TestParseErrors_tests>([
        new("http://[::1]", false),
        new("http://[::1]:80", false),
        new("http://[::1]:namedport", true), // rfc3986 3.2.3
        new("http://x:namedport", true),     // rfc3986 3.2.3
        new("http://[::1]/", false),
        new("http://[::1]a", true),
        new("http://[::1]%23", true),
        new("http://[::1%25en0]", false),    // valid zone id
        new("http://[::1]:", false),         // colon, but no port OK
        new("http://x:", false),             // colon, but no port OK
        new("http://[::1]:%38%30", true),    // not allowed: % encoding only for non-ASCII
        new("http://[::1%25%41]", false),    // RFC 6874 allows over-escaping in zone
        new("http://[%10::1]", true),        // no %xx escapes in IP address
        new("http://[::1]/%48", false),      // %xx in path is fine
        new("http://%41:8080/", true),       // not allowed: % encoding only for non-ASCII
        new("mysql://x@y(z:123)/foo", true), // not well-formed per RFC 3986, golang.org/issue/33646
        new("mysql://x@y(1.2.3.4:123)/foo", true),

        new(" http://foo.com", true),  // invalid character in schema
        new("ht tp://foo.com", true),  // invalid character in schema
        new("ahttp://foo.com", false), // valid schema characters
        new("1http://foo.com", true),  // invalid character in schema

        new("http://[]%20%48%54%54%50%2f%31%2e%31%0a%4d%79%48%65%61%64%65%72%3a%20%31%32%33%0a%0a/", true), // golang.org/issue/11208
        new("http://a b.com/", true),    // no space in host name please
        new("cache_object://foo", true), // scheme cannot have _, relative path cannot have : in first segment
        new("cache_object:foo", true),
        new("cache_object:foo/bar", true),
        new("cache_object/:foo/bar", false),

        new("http://[192.168.0.1]/", true),              // IPv4 in brackets
        new("http://[192.168.0.1]:8080/", true),         // IPv4 in brackets with port
        new("http://[::ffff:192.168.0.1]/", false),      // IPv4-mapped IPv6 in brackets
        new("http://[::ffff:192.168.0.1000]/", true),    // Out of range IPv4-mapped IPv6 in brackets
        new("http://[::ffff:192.168.0.1]:8080/", false), // IPv4-mapped IPv6 in brackets with port
        new("http://[::ffff:c0a8:1]/", false),           // IPv4-mapped IPv6 in brackets (hex)
        new("http://[not-an-ip]/", true),                // invalid IP string in brackets
        new("http://[fe80::1%foo]/", true),              // invalid zone format in brackets
        new("http://[fe80::1", true),                    // missing closing bracket
        new("http://fe80::1]/", true),                   // missing opening bracket
        new("http://[test.com]/", true),                 // domain name in brackets
        new("http://example.com[::1]", true),            // IPv6 literal doesn't start with '['
        new("http://example.com[::1", true),
        new("http://[::1", true),
        new("http://.[::1]", true),
        new("http:// [::1]", true),
        new("hxxp://mathepqo[.]serveftp(.)com:9059", true)
    ]);
    foreach (var (_, tt) in tests) {
        var (u, err) = Parse(tt.@in);
        if (tt.wantErr) {
            if (err == nil) {
                t.Errorf("Parse(%q) = %#v; want an error", tt.@in, u.OrTypedNil());
            }
            continue;
        }
        if (err != nil) {
            t.Errorf("Parse(%q) = %v; want no error", tt.@in, err);
        }
    }
}

// Issue 11202
public static void TestStarRequest(ж<testing.T> t) {
    var (u, err) = Parse("*");
    if (err != nil) {
        t.Fatal(err);
    }
    {
        @string got = u.RequestURI();
        @string want = "*"; if (got != want) {
            t.Errorf("RequestURI = %q; want %q", got, want);
        }
    }
}

internal partial struct shouldEscapeTest {
    internal byte @in;
    internal global::go.net.url_package.encoding mode;
    internal bool escape;
}

internal static slice<shouldEscapeTest> shouldEscapeTests = slice<shouldEscapeTest>([
    // Unreserved characters (§2.3)
    new((byte)'a', encodePath, false),
    new((byte)'a', encodeUserPassword, false),
    new((byte)'a', encodeQueryComponent, false),
    new((byte)'a', encodeFragment, false),
    new((byte)'a', encodeHost, false),
    new((byte)'z', encodePath, false),
    new((byte)'A', encodePath, false),
    new((byte)'Z', encodePath, false),
    new((byte)'0', encodePath, false),
    new((byte)'9', encodePath, false),
    new((byte)'-', encodePath, false),
    new((byte)'-', encodeUserPassword, false),
    new((byte)'-', encodeQueryComponent, false),
    new((byte)'-', encodeFragment, false),
    new((byte)'.', encodePath, false),
    new((byte)'_', encodePath, false),
    new((byte)'~', encodePath, false),

    // User information (§3.2.1)
    new((byte)':', encodeUserPassword, true),
    new((byte)'/', encodeUserPassword, true),
    new((byte)'?', encodeUserPassword, true),
    new((byte)'@', encodeUserPassword, true),
    new((byte)'$', encodeUserPassword, false),
    new((byte)'&', encodeUserPassword, false),
    new((byte)'+', encodeUserPassword, false),
    new((byte)',', encodeUserPassword, false),
    new((byte)';', encodeUserPassword, false),
    new((byte)'=', encodeUserPassword, false),

    // Host (IP address, IPv6 address, registered name, port suffix; §3.2.2)
    new((byte)'!', encodeHost, false),
    new((byte)'$', encodeHost, false),
    new((byte)'&', encodeHost, false),
    new((byte)'\'', encodeHost, false),
    new((byte)'(', encodeHost, false),
    new((byte)')', encodeHost, false),
    new((byte)'*', encodeHost, false),
    new((byte)'+', encodeHost, false),
    new((byte)',', encodeHost, false),
    new((byte)';', encodeHost, false),
    new((byte)'=', encodeHost, false),
    new((byte)':', encodeHost, false),
    new((byte)'[', encodeHost, false),
    new((byte)']', encodeHost, false),
    new((byte)'0', encodeHost, false),
    new((byte)'9', encodeHost, false),
    new((byte)'A', encodeHost, false),
    new((byte)'z', encodeHost, false),
    new((byte)'_', encodeHost, false),
    new((byte)'-', encodeHost, false),
    new((byte)'.', encodeHost, false)
]);

public static void TestShouldEscape(ж<testing.T> t) {
    foreach (var (_, tt) in shouldEscapeTests) {
        if (shouldEscape(tt.@in, tt.mode) != tt.escape) {
            t.Errorf("shouldEscape(%q, %v) returned %v; expected %v", tt.@in, tt.mode, !tt.escape, tt.escape);
        }
    }
}

internal partial struct timeoutError {
    internal bool timeout;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string timeoutErrorˢ => ref timeoutErrorˢᵇ;
private static readonly @string timeoutErrorˢᵇ = "timeout error"u8;

internal static @string Error(this ref timeoutError e) {
    return timeoutErrorˢ;
}

internal static bool Timeout(this ref timeoutError e) {
    return e.timeout;
}

internal partial struct temporaryError {
    internal bool temporary;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string temporaryErrorˢ => ref temporaryErrorˢᵇ;
private static readonly @string temporaryErrorˢᵇ = "temporary error"u8;

internal static @string Error(this ref temporaryError e) {
    return temporaryErrorˢ;
}

internal static bool Temporary(this ref temporaryError e) {
    return e.temporary;
}

internal partial struct timeoutTemporaryError {
    /*embed*/ internal timeoutError timeoutError;
    /*embed*/ internal temporaryError temporaryError;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string timeoutTemporaryErrorˢ => ref timeoutTemporaryErrorˢᵇ;
private static readonly @string timeoutTemporaryErrorˢᵇ = "timeout/temporary error"u8;

internal static @string Error(this ref timeoutTemporaryError e) {
    return timeoutTemporaryErrorˢ;
}

partial struct netErrorTestsᴛ1 /*dyn*/ {
    internal error err;
    internal bool timeout;
    internal bool temporary;
}
internal static slice<netErrorTestsᴛ1> netErrorTests = slice<netErrorTestsᴛ1>([new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.timeoutErrorжerror(Ꮡ(new timeoutError(timeout: true)))))),
    timeout: true,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.timeoutErrorжerror(Ꮡ(new timeoutError(timeout: false)))))),
    timeout: false,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.temporaryErrorжerror(Ꮡ(new temporaryError(temporary: true)))))),
    timeout: false,
    temporary: true
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.temporaryErrorжerror(Ꮡ(new temporaryError(temporary: false)))))),
    timeout: false,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: true), new temporaryError(temporary: true))))))),
    timeout: true,
    temporary: true
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: false), new temporaryError(temporary: true))))))),
    timeout: false,
    temporary: true
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: true), new temporaryError(temporary: false))))))),
    timeout: true,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", new url_internal_test_package.timeoutTemporaryErrorжerror(Ꮡ(new timeoutTemporaryError(new timeoutError(timeout: false), new temporaryError(temporary: false))))))),
    timeout: false,
    temporary: false
), new(
    err: new url_test_package.url_ΔErrorжerror(Ꮡ(new ΔError("Get", "http://google.com/", io.EOF))),
    timeout: false,
    temporary: false
)
]);

// Test that url.Error implements net.Error and that it forwards
public static void TestURLErrorImplementsNetError(ж<testing.T> t) {
    foreach (var (i, tt) in netErrorTests) {
        var (err, ok) = tt.err._<netꓸError>(ᐧ);
        if (!ok) {
            t.Errorf("%d: %T does not implement net.Error", i + 1, tt.err);
            continue;
        }
        if (err.Timeout() != tt.timeout) {
            t.Errorf("%d: err.Timeout(): got %v, want %v", i + 1, err.Timeout(), tt.timeout);
            continue;
        }
        if (err.Temporary() != tt.temporary) {
            t.Errorf("%d: err.Temporary(): got %v, want %v", i + 1, err.Temporary(), tt.temporary);
        }
    }
}

internal partial struct TestURLHostnameAndPort_tests /*dyn*/ {
    internal @string @in; // URL.Host field
    internal @string host;
    internal @string port;
}

public static void TestURLHostnameAndPort(ж<testing.T> t) {
    var tests = slice<TestURLHostnameAndPort_tests>([
        new("foo.com:80", "foo.com", "80"),
        new("foo.com", "foo.com", ""),
        new("foo.com:", "foo.com", ""),
        new("FOO.COM", "FOO.COM", ""), // no canonicalization
        new("1.2.3.4", "1.2.3.4", ""),
        new("1.2.3.4:80", "1.2.3.4", "80"),
        new("[1:2:3:4]", "1:2:3:4", ""),
        new("[1:2:3:4]:80", "1:2:3:4", "80"),
        new("[::1]:80", "::1", "80"),
        new("[::1]", "::1", ""),
        new("[::1]:", "::1", ""),
        new("localhost", "localhost", ""),
        new("localhost:443", "localhost", "443"),
        new("some.super.long.domain.example.org:8080", "some.super.long.domain.example.org", "8080"),
        new("[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:17000", "2001:0db8:85a3:0000:0000:8a2e:0370:7334", "17000"),
        new("[2001:0db8:85a3:0000:0000:8a2e:0370:7334]", "2001:0db8:85a3:0000:0000:8a2e:0370:7334", ""),

        // Ensure that even when not valid, Host is one of "Hostname",
        // "Hostname:Port", "[Hostname]" or "[Hostname]:Port".
        // See https://golang.org/issue/29098.
        new("[google.com]:80", "google.com", "80"),
        new("google.com]:80", "google.com]", "80"),
        new("google.com:80_invalid_port", "google.com:80_invalid_port", ""),
        new("[::1]extra]:80", "::1]extra", "80"),
        new("google.com]extra:extra", "google.com]extra:extra", "")
    ]);
    foreach (var (_, tt) in tests) {
        var u = Ꮡ(new URL(Host: tt.@in));
        @string host = u.Hostname();
        @string port = u.Port();
        if (host != tt.host) {
            t.Errorf("Hostname for Host %q = %q; want %q", tt.@in, host, tt.host);
        }
        if (port != tt.port) {
            t.Errorf("Port for Host %q = %q; want %q", tt.@in, port, tt.port);
        }
    }
}

internal static encodingPkg.BinaryMarshaler _ᴛ1ʗ = new url_test_package.url_URLжBinaryMarshaler(((ж<global::go.net.url_package.URL>)nil));

internal static encodingPkg.BinaryUnmarshaler _ᴛ2ʗ = new url_test_package.url_URLжBinaryUnmarshaler(((ж<global::go.net.url_package.URL>)nil));

internal static encodingPkg.BinaryAppender _ᴛ3ʗ = new url_test_package.url_URLжBinaryAppender(((ж<global::go.net.url_package.URL>)nil));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsWwwGoogleComXYZˢ => ref httpsWwwGoogleComXYZˢᵇ;
private static readonly @string httpsWwwGoogleComXYZˢᵇ = "https://www.google.com/x?y=z"u8;

public static void TestJSON(ж<testing.T> t) {
    var (u, err) = Parse(httpsWwwGoogleComXYZˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var js, err) = json.Marshal(u.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }

    // If only we could implement TextMarshaler/TextUnmarshaler,
    // this would work:
    //
    // if string(js) != strconv.Quote(u.String()) {
    // 	t.Errorf("json encoding: %s\nwant: %s\n", js, strconv.Quote(u.String()))
    // }

    var u1 = @new<global::go.net.url_package.URL>();
    err = json.Unmarshal(js, u1.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    if (u1.String() != u.String()) {
        t.Errorf("json decoded to: %s\nwant: %s\n", u1.OrTypedNil(), u.OrTypedNil());
    }
}

public static void TestGob(ж<testing.T> t) {
    var (u, err) = Parse(httpsWwwGoogleComXYZˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    ref var w = ref heap(new bytes.Buffer(), out var Ꮡw);
    err = gob.NewEncoder(new url_test_package.bytes_BufferжWriter(Ꮡw)).Encode(u.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }

    var u1 = @new<global::go.net.url_package.URL>();
    err = gob.NewDecoder(new url_test_package.bytes_BufferжReader(Ꮡw)).Decode(u1.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    if (u1.String() != u.String()) {
        t.Errorf("json decoded to: %s\nwant: %s\n", u1.OrTypedNil(), u.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFooComˢ => ref httpFooComˢᵇ;
private static readonly @string httpFooComˢᵇ = "http://foo.com/"u8;

public static void TestNilUser(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var v = recover(); if (v != nil) {
                    t.Fatalf("unexpected panic: %v", v);
                }
            }
        }, ref ᒐ);

        var (u, err) = Parse(httpFooComˢ);

        if (err != nil) {
            t.Fatalf("parse err: %v", err);
        }

        {
            @string v = u.User.Username(); if (v != ""u8) {
                t.Fatalf("expected empty username, got %s", v);
            }
        }

        {
            var (v, ok) = u.User.Password(); if (v != ""u8 || ok) {
                t.Fatalf("expected empty password, got %s (%v)", v, ok);
            }
        }

        {
            @string v = u.User.String(); if (v != ""u8) {
                t.Fatalf("expected empty string, got %s", v);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpUserPasswoRdFooComˢ => ref httpUserPasswoRdFooComˢᵇ;
private static readonly @string httpUserPasswoRdFooComˢᵇ = "http://user^:passwo^rd@foo.com/"u8;

public static void TestInvalidUserPassword(ж<testing.T> t) {
    var (_, err) = Parse(httpUserPasswoRdFooComˢ);
    {
        @string got = fmt.Sprint(err);
        @string wantsub = netUrlInvalidUserinfoˢ; if (!strings.Contains(got, wantsub)) {
            t.Errorf("error = %q; want substring %q", got, wantsub);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantSubᶜ => ref wantSubᶜᵇ;
private static readonly @string wantSubᶜᵇ = "net/url: invalid control character in URL"u8;

public static void TestRejectControlCharacters(ж<testing.T> t) {
    var tests = slice<@string>([
        "http://foo.com/?foo\nbar",
        "http\r://foo.com/",
        "http://foo\x7f.com/"
    ]);
    foreach (var (_, s) in tests) {
        var (_, err) = Parse(s);
        @string wantSub = wantSubᶜ;
        {
            @string got = fmt.Sprint(err); if (!strings.Contains(got, wantSub)) {
                t.Errorf("Parse(%q) error = %q; want substring %q", s, got, wantSub);
            }
        }
    }

    // But don't reject non-ASCII CTLs, at least for now:
    {
        var (_, err) = Parse(((@string)(new byte[]{0x68, 0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x66, 0x6f, 0x6f, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x63, 0x74, 0x6c, 0x80}))); if (err != nil) {
            t.Errorf("error parsing URL with non-ASCII control byte: %v", err);
        }
    }
}

partial struct escapeBenchmarksᴛ1 /*dyn*/ {
    internal @string unescaped;
    internal @string query;
    internal @string path;
}
internal static slice<escapeBenchmarksᴛ1> escapeBenchmarks = slice<escapeBenchmarksᴛ1>([
    new(
        unescaped: "one two",
        query: "one+two",
        path: "one%20two"
    ),
    new(
        unescaped: "Фотки собак",
        query: "%D0%A4%D0%BE%D1%82%D0%BA%D0%B8+%D1%81%D0%BE%D0%B1%D0%B0%D0%BA",
        path: "%D0%A4%D0%BE%D1%82%D0%BA%D0%B8%20%D1%81%D0%BE%D0%B1%D0%B0%D0%BA"
    ),

    new(
        unescaped: "shortrun(break)shortrun",
        query: "shortrun%28break%29shortrun",
        path: "shortrun%28break%29shortrun"
    ),

    new(
        unescaped: "longerrunofcharacters(break)anotherlongerrunofcharacters",
        query: "longerrunofcharacters%28break%29anotherlongerrunofcharacters",
        path: "longerrunofcharacters%28break%29anotherlongerrunofcharacters"
    ),

    new(
        unescaped: strings.Repeat("padded/with+various%characters?that=need$some@escaping+paddedsowebreak/256bytes", 4),
        query: strings.Repeat("padded%2Fwith%2Bvarious%25characters%3Fthat%3Dneed%24some%40escaping%2Bpaddedsowebreak%2F256bytes", 4),
        path: strings.Repeat("padded%2Fwith+various%25characters%3Fthat=need$some@escaping+paddedsowebreak%2F256bytes", 4)
    )
]);

public static void BenchmarkQueryEscape(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                g = QueryEscape(tcʗ1.unescaped);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.query) {
                bΔ1.Errorf("QueryEscape(%q) == %q, want %q", tcʗ1.unescaped, g, tcʗ1.query);
            }
        });
    }
}

public static void BenchmarkPathEscape(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                g = PathEscape(tcʗ1.unescaped);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.path) {
                bΔ1.Errorf("PathEscape(%q) == %q, want %q", tcʗ1.unescaped, g, tcʗ1.path);
            }
        });
    }
}

public static void BenchmarkQueryUnescape(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (g, _) = QueryUnescape(tcʗ1.query);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.unescaped) {
                bΔ1.Errorf("QueryUnescape(%q) == %q, want %q", tcʗ1.query, g, tcʗ1.unescaped);
            }
        });
    }
}

public static void BenchmarkPathUnescape(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in escapeBenchmarks) {
        ref var tc = ref heap(new escapeBenchmarksᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(""u8, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            @string g = default;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (g, _) = PathUnescape(tcʗ1.path);
            }
            bΔ1.StopTimer();
            if (g != tcʗ1.unescaped) {
                bΔ1.Errorf("PathUnescape(%q) == %q, want %q", tcʗ1.path, g, tcʗ1.unescaped);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ2 => ref nilˢ2ᵇ;
private static readonly @string nilˢ2ᵇ = "nil"u8;
internal static ref readonly @string nonNilErrorˢ => ref nonNilErrorˢᵇ;
private static readonly @string nonNilErrorˢᵇ = "non-nil error"u8;

internal partial struct TestJoinPath_tests /*dyn*/ {
    internal @string @base;
    internal slice<@string> elem;
    internal @string @out;
}

public static void TestJoinPath(ж<testing.T> t) {
    var tests = slice<TestJoinPath_tests>([
        new(
            @base: "https://go.googlesource.com",
            elem: slice<@string>(["go"]),
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com/a/b/c",
            elem: slice<@string>(["../../../go"]),
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com/",
            elem: slice<@string>(["../go"]),
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com",
            elem: slice<@string>(["../go", "../../go", "../../../go"]),
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com/../go",
            elem: default,
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com/",
            elem: slice<@string>(["./go"]),
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com//",
            elem: slice<@string>(["/go"]),
            @out: "https://go.googlesource.com/go"
        ),
        new(
            @base: "https://go.googlesource.com//",
            elem: slice<@string>(["/go", "a", "b", "c"]),
            @out: "https://go.googlesource.com/go/a/b/c"
        ),
        new(
            @base: "http://[fe80::1%en0]:8080/",
            elem: slice<@string>(["/go"])
        ),
        new(
            @base: "https://go.googlesource.com",
            elem: slice<@string>(["go/"]),
            @out: "https://go.googlesource.com/go/"
        ),
        new(
            @base: "https://go.googlesource.com",
            elem: slice<@string>(["go//"]),
            @out: "https://go.googlesource.com/go/"
        ),
        new(
            @base: "https://go.googlesource.com",
            elem: default,
            @out: "https://go.googlesource.com/"
        ),
        new(
            @base: "https://go.googlesource.com/",
            elem: default,
            @out: "https://go.googlesource.com/"
        ),
        new(
            @base: "https://go.googlesource.com/a%2fb",
            elem: slice<@string>(["c"]),
            @out: "https://go.googlesource.com/a%2fb/c"
        ),
        new(
            @base: "https://go.googlesource.com/a%2fb",
            elem: slice<@string>(["c%2fd"]),
            @out: "https://go.googlesource.com/a%2fb/c%2fd"
        ),
        new(
            @base: "https://go.googlesource.com/a/b",
            elem: slice<@string>(["/go"]),
            @out: "https://go.googlesource.com/a/b/go"
        ),
        new(
            @base: "https://go.googlesource.com/",
            elem: slice<@string>(["100%"])
        ),
        new(
            @base: "/",
            elem: default,
            @out: "/"
        ),
        new(
            @base: "a",
            elem: default,
            @out: "a"
        ),
        new(
            @base: "a",
            elem: slice<@string>(["b"]),
            @out: "a/b"
        ),
        new(
            @base: "a",
            elem: slice<@string>(["../b"]),
            @out: "b"
        ),
        new(
            @base: "a",
            elem: slice<@string>(["../../b"]),
            @out: "b"
        ),
        new(
            @base: "",
            elem: slice<@string>(["a"]),
            @out: "a"
        ),
        new(
            @base: "",
            elem: slice<@string>(["../a"]),
            @out: "a"
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        var tt = vᴛ1;

        @string wantErr = nilˢ2;
        if (tt.@out == ""u8) {
            wantErr = nonNilErrorˢ;
        }
        var (@out, err) = JoinPath(tt.@base, tt.elem.ꓸꓸꓸ);
        if (@out != tt.@out || (err == nil) != (tt.@out != ""u8)) {
            t.Errorf("JoinPath(%q, %q) = %q, %v, want %q, %v", tt.@base, tt.elem, @out, err, tt.@out, wantErr);
        }

        (var u, err) = Parse(tt.@base);
        if (err != nil) {
            if (tt.@out != ""u8) {
                t.Errorf("Parse(%q) = %v", tt.@base, err);
            }
            continue;
        }
        if (tt.@out == ""u8) {
            // URL.JoinPath doesn't return an error, so leave it unchanged
            tt.@out = tt.@base;
        }
        @out = u.JoinPath(tt.elem.ꓸꓸꓸ).String();
        if (@out != tt.@out) {
            t.Errorf("Parse(%q).JoinPath(%q) = %q, want %q", tt.@base, tt.elem, @out, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string urlstrictcolons0ˢ => ref urlstrictcolons0ˢᵇ;
private static readonly @string urlstrictcolons0ˢᵇ = "urlstrictcolons=0"u8;

internal partial struct TestParseStrictIpv6_tests /*dyn*/ {
    internal @string url;
}

public static void TestParseStrictIpv6(ж<testing.T> t) {
    t.Setenv(godebugˢ, urlstrictcolons0ˢ);

    var tests = slice<TestParseStrictIpv6_tests>([
        // Malformed URLs that used to parse.
        new("https://1:2:3:4:5:6:7:8"),
        new("https://1:2:3:4:5:6:7:8:80"),
        new("https://example.com:80:")
    ]);
    foreach (var (i, vᴛ1) in tests) {
        ref var tc = ref heap(new TestParseStrictIpv6_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(strconv.Itoa(i), (ж<testing.T> tΔ1) => {
            var (_, err) = Parse(tcʗ1.url);
            if (err != nil) {
                tΔ1.Errorf("Parse(%q) error = %v, want nil", tcʗ1.url, err);
            }
        });
    }
}

} // end url_internal_test_package
