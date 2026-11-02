// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using fmt = fmt_package;
using http = go.net.http_package;
using url = go.net.url_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.net;
using static go.net.http.cookiejar_package;

partial class cookiejar_internal_test_package {

// tNow is the synthetic current time used as now during testing.
internal static time.Time tNow = time.Date(2013, 1, 1, 12, 0, 0, 0, time.ΔUTC);

// testPSL implements PublicSuffixList with just two rules: "co.uk"
// and the default rule "*".
// The implementation has two intentional bugs:
//
//	PublicSuffix("www.buggy.psl") == "xy"
//	PublicSuffix("www2.buggy.psl") == "com"
internal partial struct testPSL {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testPSLˢ => ref testPSLˢᵇ;
private static readonly @string testPSLˢᵇ = "testPSL"u8;

internal static @string String(this testPSL _) {
    return testPSLˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string coUkˢ => ref coUkˢᵇ;
private static readonly @string coUkˢᵇ = ".co.uk"u8;
internal static ref readonly @string coUkˢ2 => ref coUkˢ2ᵇ;
private static readonly @string coUkˢ2ᵇ = "co.uk"u8;
internal static ref readonly @string comˢ => ref comˢᵇ;
private static readonly @string comˢᵇ = "com"u8;

internal static @string PublicSuffix(this testPSL _, @string d) {
    if (d == "co.uk"u8 || strings.HasSuffix(d, coUkˢ)) {
        return coUkˢ2;
    }
    if (d == "www.buggy.psl"u8) {
        return "xy";
    }
    if (d == "www2.buggy.psl"u8) {
        return comˢ;
    }
    return d.slice(strings.LastIndex(d, ".") + 1);
}

// newTestJar creates an empty Jar with testPSL as the public suffix list.
internal static ж<global::go.net.http.cookiejar_package.Jar> newTestJar() {
    var (jar, err) = New(Ꮡ(new Options(PublicSuffixList: new testPSL())));
    if (err != nil) {
        throw panic(err);
    }
    return jar;
}

partial struct hasDotSuffixTestsᴛ1 /*dyn*/ {
    internal @string s, suffix;
}
internal static array<hasDotSuffixTestsᴛ1> hasDotSuffixTests = new hasDotSuffixTestsᴛ1[]{
    new("", ""),
    new("", "."),
    new("", "x"),
    new(".", ""),
    new(".", "."),
    new(".", ".."),
    new(".", "x"),
    new(".", "x."),
    new(".", ".x"),
    new(".", ".x."),
    new("x", ""),
    new("x", "."),
    new("x", ".."),
    new("x", "x"),
    new("x", "x."),
    new("x", ".x"),
    new("x", ".x."),
    new(".x", ""),
    new(".x", "."),
    new(".x", ".."),
    new(".x", "x"),
    new(".x", "x."),
    new(".x", ".x"),
    new(".x", ".x."),
    new("x.", ""),
    new("x.", "."),
    new("x.", ".."),
    new("x.", "x"),
    new("x.", "x."),
    new("x.", ".x"),
    new("x.", ".x."),
    new("com", ""),
    new("com", "m"),
    new("com", "om"),
    new("com", "com"),
    new("com", ".com"),
    new("com", "x.com"),
    new("com", "xcom"),
    new("com", "xorg"),
    new("com", "org"),
    new("com", "rg"),
    new("foo.com", ""),
    new("foo.com", "m"),
    new("foo.com", "om"),
    new("foo.com", "com"),
    new("foo.com", ".com"),
    new("foo.com", "o.com"),
    new("foo.com", "oo.com"),
    new("foo.com", "foo.com"),
    new("foo.com", ".foo.com"),
    new("foo.com", "x.foo.com"),
    new("foo.com", "xfoo.com"),
    new("foo.com", "xfoo.org"),
    new("foo.com", "foo.org"),
    new("foo.com", "oo.org"),
    new("foo.com", "o.org"),
    new("foo.com", ".org"),
    new("foo.com", "org"),
    new("foo.com", "rg")
}.array();

public static void TestHasDotSuffix(ж<testing.T> t) {
    foreach (var (_, tc) in hasDotSuffixTests.ΔRangeSnapshot()) {
        var got = hasDotSuffix(tc.s, tc.suffix);
        var want = strings.HasSuffix(tc.s, "."u8 + tc.suffix);
        if (got != want) {
            t.Errorf("s=%q, suffix=%q: got %v, want %v", tc.s, tc.suffix, got, want);
        }
    }
}

internal static map<@string, @string> canonicalHostTests = new map<@string, @string>{
    ["www.example.com"] = "www.example.com",
    ["WWW.EXAMPLE.COM"] = "www.example.com",
    ["wWw.eXAmple.CoM"] = "www.example.com",
    ["www.example.com:80"] = "www.example.com",
    ["192.168.0.10"] = "192.168.0.10",
    ["192.168.0.5:8080"] = "192.168.0.5",
    ["2001:4860:0:2001::68"] = "2001:4860:0:2001::68",
    ["[2001:4860:0:::68]:8080"] = "2001:4860:0:::68",
    ["www.bücher.de"] = "www.xn--bcher-kva.de",
    ["www.example.com."] = "www.example.com",
    // TODO: Fix canonicalHost so that all of the following malformed
    // domain names trigger an error. (This list is not exhaustive, e.g.
    // malformed internationalized domain names are missing.)
    ["."] = "",
    [".."] = ".",
    ["..."] = "..",
    [".net"] = ".net",
    [".net."] = ".net",
    ["a.."] = "a.",
    ["b.a.."] = "b.a.",
    ["weird.stuff..."] = "weird.stuff..",
    ["[bad.unmatched.bracket:"] = "error"
};

public static void TestCanonicalHost(ж<testing.T> t) {
    foreach (var (h, want) in canonicalHostTests) {
        var (got, err) = canonicalHost(h);
        if (want == "error"u8) {
            if (err == nil) {
                t.Errorf("%q: got %q and nil error, want non-nil", h, got);
            }
            continue;
        }
        if (err != nil) {
            t.Errorf("%q: %v", h, err);
            continue;
        }
        if (got != want) {
            t.Errorf("%q: got %q, want %q", h, got, want);
            continue;
        }
    }
}

internal static map<@string, bool> hasPortTests = new map<@string, bool>{
    ["www.example.com"] = false,
    ["www.example.com:80"] = true,
    ["127.0.0.1"] = false,
    ["127.0.0.1:8080"] = true,
    ["2001:4860:0:2001::68"] = false,
    ["[2001::0:::68]:80"] = true
};

public static void TestHasPort(ж<testing.T> t) {
    foreach (var (host, want) in hasPortTests) {
        {
            var got = hasPort(host); if (got != want) {
                t.Errorf("%q: got %t, want %t", host, got, want);
            }
        }
    }
}

internal static map<@string, @string> jarKeyTests = new map<@string, @string>{
    ["foo.www.example.com"] = "example.com",
    ["www.example.com"] = "example.com",
    ["example.com"] = "example.com",
    ["com"] = "com",
    ["foo.www.bbc.co.uk"] = "bbc.co.uk",
    ["www.bbc.co.uk"] = "bbc.co.uk",
    ["bbc.co.uk"] = "bbc.co.uk",
    ["co.uk"] = "co.uk",
    ["uk"] = "uk",
    ["192.168.0.5"] = "192.168.0.5",
    ["www.buggy.psl"] = "www.buggy.psl",
    ["www2.buggy.psl"] = "buggy.psl",
    // The following are actual outputs of canonicalHost for
    // malformed inputs to canonicalHost (see above).
    [""] = "",
    ["."] = ".",
    [".."] = ".",
    [".net"] = ".net",
    ["a."] = "a.",
    ["b.a."] = "a.",
    ["weird.stuff.."] = "."
};

public static void TestJarKey(ж<testing.T> t) {
    foreach (var (host, want) in jarKeyTests) {
        {
            @string got = jarKey(host, new testPSL()); if (got != want) {
                t.Errorf("%q: got %q, want %q", host, got, want);
            }
        }
    }
}

internal static map<@string, @string> jarKeyNilPSLTests = new map<@string, @string>{
    ["foo.www.example.com"] = "example.com",
    ["www.example.com"] = "example.com",
    ["example.com"] = "example.com",
    ["com"] = "com",
    ["foo.www.bbc.co.uk"] = "co.uk",
    ["www.bbc.co.uk"] = "co.uk",
    ["bbc.co.uk"] = "co.uk",
    ["co.uk"] = "co.uk",
    ["uk"] = "uk",
    ["192.168.0.5"] = "192.168.0.5",
    // The following are actual outputs of canonicalHost for
    // malformed inputs to canonicalHost.
    [""] = "",
    ["."] = ".",
    [".."] = "..",
    [".net"] = ".net",
    ["a."] = "a.",
    ["b.a."] = "a.",
    ["weird.stuff.."] = "stuff.."
};

public static void TestJarKeyNilPSL(ж<testing.T> t) {
    foreach (var (host, want) in jarKeyNilPSLTests) {
        {
            @string got = jarKey(host, default); if (got != want) {
                t.Errorf("%q: got %q, want %q", host, got, want);
            }
        }
    }
}

internal static map<@string, bool> isIPTests = new map<@string, bool>{
    ["127.0.0.1"] = true,
    ["1.2.3.4"] = true,
    ["2001:4860:0:2001::68"] = true,
    ["::1%zone"] = true,
    ["example.com"] = false,
    ["1.1.1.300"] = false,
    ["www.foo.bar.net"] = false,
    ["123.foo.bar.net"] = false
};

public static void TestIsIP(ж<testing.T> t) {
    foreach (var (host, want) in isIPTests) {
        {
            var got = isIP(host); if (got != want) {
                t.Errorf("%q: got %t, want %t", host, got, want);
            }
        }
    }
}

internal static map<@string, @string> defaultPathTests = new map<@string, @string>{
    ["/"] = "/",
    ["/abc"] = "/",
    ["/abc/"] = "/abc",
    ["/abc/xyz"] = "/abc",
    ["/abc/xyz/"] = "/abc/xyz",
    ["/a/b/c.html"] = "/a/b",
    [""] = "/",
    ["strange"] = "/",
    ["//"] = "/",
    ["/a//b"] = "/a/",
    ["/a/./b"] = "/a/.",
    ["/a/../b"] = "/a/.."
};

public static void TestDefaultPath(ж<testing.T> t) {
    foreach (var (path, want) in defaultPathTests) {
        {
            @string got = defaultPath(path); if (got != want) {
                t.Errorf("%q: got %q, want %q", path, got, want);
            }
        }
    }
}

partial struct domainAndTypeTestsᴛ1 /*dyn*/ {
    internal @string host;       // host Set-Cookie header was received from
    internal @string domain;     // domain attribute in Set-Cookie header
    internal @string wantDomain; // expected domain of cookie
    internal bool wantHostOnly;  // expected host-cookie flag
    internal error wantErr;      // expected error
}
internal static array<domainAndTypeTestsᴛ1> domainAndTypeTests;
internal static void initᴛdomainAndTypeTests() { domainAndTypeTests = new domainAndTypeTestsᴛ1[]{
    new("www.example.com", "", "www.example.com", true, default),
    new("127.0.0.1", "", "127.0.0.1", true, default),
    new("2001:4860:0:2001::68", "", "2001:4860:0:2001::68", true, default),
    new("www.example.com", "example.com", "example.com", false, default),
    new("www.example.com", ".example.com", "example.com", false, default),
    new("www.example.com", "www.example.com", "www.example.com", false, default),
    new("www.example.com", ".www.example.com", "www.example.com", false, default),
    new("foo.sso.example.com", "sso.example.com", "sso.example.com", false, default),
    new("bar.co.uk", "bar.co.uk", "bar.co.uk", false, default),
    new("foo.bar.co.uk", ".bar.co.uk", "bar.co.uk", false, default),
    new("127.0.0.1", "127.0.0.1", "127.0.0.1", true, default),
    new("2001:4860:0:2001::68", "2001:4860:0:2001::68", "2001:4860:0:2001::68", true, default),
    new("www.example.com", ".", "", false, errMalformedDomain),
    new("www.example.com", "..", "", false, errMalformedDomain),
    new("www.example.com", "other.com", "", false, errIllegalDomain),
    new("www.example.com", "com", "", false, errIllegalDomain),
    new("www.example.com", ".com", "", false, errIllegalDomain),
    new("foo.bar.co.uk", ".co.uk", "", false, errIllegalDomain),
    new("127.www.0.0.1", "127.0.0.1", "", false, errIllegalDomain),
    new("com", "", "com", true, default),
    new("com", "com", "com", true, default),
    new("com", ".com", "com", true, default),
    new("co.uk", "", "co.uk", true, default),
    new("co.uk", "co.uk", "co.uk", true, default),
    new("co.uk", ".co.uk", "co.uk", true, default)
}.array(); }

public static void TestDomainAndType(ж<testing.T> t) {
    var jar = newTestJar();
    foreach (var (_, tc) in domainAndTypeTests.ΔRangeSnapshot()) {
        var (domain, hostOnly, err) = jar.domainAndType(tc.host, tc.domain);
        if (!AreEqual(err, tc.wantErr)) {
            t.Errorf("%q/%q: got %q error, want %v",
                tc.host, tc.domain, err, tc.wantErr);
            continue;
        }
        if (err != nil) {
            continue;
        }
        if (domain != tc.wantDomain || hostOnly != tc.wantHostOnly) {
            t.Errorf("%q/%q: got %q/%t want %q/%t",
                tc.host, tc.domain, domain, hostOnly,
                tc.wantDomain, tc.wantHostOnly);
        }
    }
}

// expiresIn creates an expires attribute delta seconds from tNow.
internal static @string expiresIn(nint delta) {
    var t = tNow.Add(((time.Duration)(int64)delta) * time.ΔSecond);
    return "expires="u8 + t.Format(time.RFC1123);
}

// mustParseURL parses s to a URL and panics on error.
internal static ж<url.URL> mustParseURL(@string s) {
    var (u, err) = url.Parse(s);
    if (err != nil || u.Scheme == ""u8 || u.Host == ""u8) {
        throw panic(fmt.Sprintf("Unable to parse URL %s."u8, s));
    }
    return u;
}

// jarTest encapsulates the following actions on a jar:
//  1. Perform SetCookies with fromURL and the cookies from setCookies.
//     (Done at time tNow + 0 ms.)
//  2. Check that the entries in the jar matches content.
//     (Done at time tNow + 1001 ms.)
//  3. For each query in tests: Check that Cookies with toURL yields the
//     cookies in want.
//     (Query n done at tNow + (n+2)*1001 ms.)
internal partial struct jarTest {
    internal @string description;       // The description of what this test is supposed to test
    internal @string fromURL;           // The full URL of the request from which Set-Cookie headers where received
    internal slice<@string> setCookies; // All the cookies received from fromURL
    internal @string content;           // The whole (non-expired) content of the jar
    internal slice<query> queries;      // Queries to test the Jar.Cookies method
}

// query contains one test of the cookies returned from Jar.Cookies.
internal partial struct query {
    internal @string toURL; // the URL in the Cookies call
    internal @string want;  // the expected list of cookies (order matters)
}

// run runs the jarTest.
internal static void run(this jarTest test, ж<testing.T> t, ж<global::go.net.http.cookiejar_package.Jar> Ꮡjar) {
    ref var jar = ref Ꮡjar.DerefOrNull();
    var csᴷ = new stackAppend<@string>();
    var sᴷ = new stackAppend<@string>();

    var now = tNow;

    // Populate jar with cookies.
    var setCookies = new slice<ж<httpꓸCookie>>(len(test.setCookies));
    foreach (var (i, csΔ1) in test.setCookies) {
        var cookies = (Ꮡ(new http.Response(Header: new httpꓸHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([csΔ1])})))).Cookies();
        if (len(cookies) != 1) {
            throw panic(fmt.Sprintf("Wrong cookie line %q: %#v"u8, csΔ1, cookies));
        }
        setCookies[i] = cookies[0];
    }
    Ꮡjar.setCookies(mustParseURL(test.fromURL), setCookies, now);
    now = now.Add(1001 * time.Millisecond);

    // Serialize non-expired entries in the form "name1=val1 name2=val2".
    slice<@string> cs = default;
    foreach (var (_, submap) in jar.entries) {
        foreach (var (_, cookie) in submap) {
            if (!cookie.Expires.After(now)) {
                continue;
            }

            @string v = cookie.Value;
            if (strings.ContainsAny(v, " ,") || cookie.Quoted) {
                v = @""""u8 + v + @""""u8;
            }
            cs = append(csᴷ, cs, cookie.Name + "="u8 + v);
        }
    }
    slices.Sort<slice<@string>, @string>(cs);
    @string got = strings.Join(cs, " ");

    // Make sure jar content matches our expectations.
    if (got != test.content) {
        t.Errorf("Test %q Content\ngot  %q\nwant %q",
            test.description, got, test.content);
    }

    // Test different calls to Cookies.
    foreach (var (i, query) in test.queries) {
        now = now.Add(1001 * time.Millisecond);
        slice<@string> s = default;
        foreach (var (_, c) in Ꮡjar.cookies(mustParseURL(query.toURL), now)) {
            s = append(sᴷ, s, c.String());
        }
        {
            @string gotΔ1 = strings.Join(s, " "); if (gotΔ1 != query.want) {
                t.Errorf("Test %q #%d\ngot  %q\nwant %q", test.description, i, gotΔ1, query.want);
            }
        }
    }
}

// basicsTests contains fundamental tests. Each jarTest has to be performed on
// a fresh, empty Jar.
internal static array<jarTest> basicsTests = new jarTest[]{
    new(
        "Retrieval of a plain host cookie.",
        "http://www.host.test/",
        slice<@string>(["A=a"]),
        "A=a",
        slice<query>([
            new("http://www.host.test", "A=a"),
            new("http://www.host.test/", "A=a"),
            new("http://www.host.test/some/path", "A=a"),
            new("https://www.host.test", "A=a"),
            new("https://www.host.test/", "A=a"),
            new("https://www.host.test/some/path", "A=a"),
            new("ftp://www.host.test", ""),
            new("ftp://www.host.test/", ""),
            new("ftp://www.host.test/some/path", ""),
            new("http://www.other.org", ""),
            new("http://sibling.host.test", ""),
            new("http://deep.www.host.test", "")
        ])
    ),
    new(
        "Secure cookies are not returned to http.",
        "http://www.host.test/",
        slice<@string>(["A=a; secure"]),
        "A=a",
        slice<query>([
            new("http://www.host.test", ""),
            new("http://www.host.test/", ""),
            new("http://www.host.test/some/path", ""),
            new("https://www.host.test", "A=a"),
            new("https://www.host.test/", "A=a"),
            new("https://www.host.test/some/path", "A=a")
        ])
    ),
    new(
        "Secure cookies are sent for localhost",
        "http://localhost:8910/",
        slice<@string>(["A=a; secure"]),
        "A=a",
        slice<query>([
            new("http://localhost:8910", "A=a"),
            new("http://localhost:8910/", "A=a"),
            new("http://localhost:8910/some/path", "A=a"),
            new("https://localhost:8910", "A=a"),
            new("https://localhost:8910/", "A=a"),
            new("https://localhost:8910/some/path", "A=a")
        ])
    ),
    new(
        "Secure cookies are sent for localhost (tld)",
        "http://example.LOCALHOST:8910/",
        slice<@string>(["A=a; secure"]),
        "A=a",
        slice<query>([
            new("http://example.LOCALHOST:8910", "A=a"),
            new("http://example.LOCALHOST:8910/", "A=a"),
            new("http://example.LOCALHOST:8910/some/path", "A=a"),
            new("https://example.LOCALHOST:8910", "A=a"),
            new("https://example.LOCALHOST:8910/", "A=a"),
            new("https://example.LOCALHOST:8910/some/path", "A=a")
        ])
    ),
    new(
        "Secure cookies are sent for localhost (ipv6)",
        "http://[::1]:8910/",
        slice<@string>(["A=a; secure"]),
        "A=a",
        slice<query>([
            new("http://[::1]:8910", "A=a"),
            new("http://[::1]:8910/", "A=a"),
            new("http://[::1]:8910/some/path", "A=a"),
            new("https://[::1]:8910", "A=a"),
            new("https://[::1]:8910/", "A=a"),
            new("https://[::1]:8910/some/path", "A=a")
        ])
    ),
    new(
        "Localhost only if it's a segment",
        "http://notlocalhost/",
        slice<@string>(["A=a; secure"]),
        "A=a",
        slice<query>([
            new("http://notlocalhost", ""),
            new("http://notlocalhost/", ""),
            new("http://notlocalhost/some/path", ""),
            new("https://notlocalhost", "A=a"),
            new("https://notlocalhost/", "A=a"),
            new("https://notlocalhost/some/path", "A=a")
        ])
    ),
    new(
        "Explicit path.",
        "http://www.host.test/",
        slice<@string>(["A=a; path=/some/path"]),
        "A=a",
        slice<query>([
            new("http://www.host.test", ""),
            new("http://www.host.test/", ""),
            new("http://www.host.test/some", ""),
            new("http://www.host.test/some/", ""),
            new("http://www.host.test/some/path", "A=a"),
            new("http://www.host.test/some/paths", ""),
            new("http://www.host.test/some/path/foo", "A=a"),
            new("http://www.host.test/some/path/foo/", "A=a")
        ])
    ),
    new(
        "Implicit path #1: path is a directory.",
        "http://www.host.test/some/path/",
        slice<@string>(["A=a"]),
        "A=a",
        slice<query>([
            new("http://www.host.test", ""),
            new("http://www.host.test/", ""),
            new("http://www.host.test/some", ""),
            new("http://www.host.test/some/", ""),
            new("http://www.host.test/some/path", "A=a"),
            new("http://www.host.test/some/paths", ""),
            new("http://www.host.test/some/path/foo", "A=a"),
            new("http://www.host.test/some/path/foo/", "A=a")
        ])
    ),
    new(
        "Implicit path #2: path is not a directory.",
        "http://www.host.test/some/path/index.html",
        slice<@string>(["A=a"]),
        "A=a",
        slice<query>([
            new("http://www.host.test", ""),
            new("http://www.host.test/", ""),
            new("http://www.host.test/some", ""),
            new("http://www.host.test/some/", ""),
            new("http://www.host.test/some/path", "A=a"),
            new("http://www.host.test/some/paths", ""),
            new("http://www.host.test/some/path/foo", "A=a"),
            new("http://www.host.test/some/path/foo/", "A=a")
        ])
    ),
    new(
        "Implicit path #3: no path in URL at all.",
        "http://www.host.test",
        slice<@string>(["A=a"]),
        "A=a",
        slice<query>([
            new("http://www.host.test", "A=a"),
            new("http://www.host.test/", "A=a"),
            new("http://www.host.test/some/path", "A=a")
        ])
    ),
    new(
        "Cookies are sorted by path length.",
        "http://www.host.test/",
        slice<@string>([
            "A=a; path=/foo/bar",
            "B=b; path=/foo/bar/baz/qux",
            "C=c; path=/foo/bar/baz",
            "D=d; path=/foo"]),
        "A=a B=b C=c D=d",
        slice<query>([
            new("http://www.host.test/foo/bar/baz/qux", "B=b C=c A=a D=d"),
            new("http://www.host.test/foo/bar/baz/", "C=c A=a D=d"),
            new("http://www.host.test/foo/bar", "A=a D=d")
        ])
    ),
    new(
        "Creation time determines sorting on same length paths.",
        "http://www.host.test/",
        slice<@string>([
            "A=a; path=/foo/bar",
            "X=x; path=/foo/bar",
            "Y=y; path=/foo/bar/baz/qux",
            "B=b; path=/foo/bar/baz/qux",
            "C=c; path=/foo/bar/baz",
            "W=w; path=/foo/bar/baz",
            "Z=z; path=/foo",
            "D=d; path=/foo"]),
        "A=a B=b C=c D=d W=w X=x Y=y Z=z",
        slice<query>([
            new("http://www.host.test/foo/bar/baz/qux", "Y=y B=b C=c W=w A=a X=x Z=z D=d"),
            new("http://www.host.test/foo/bar/baz/", "C=c W=w A=a X=x Z=z D=d"),
            new("http://www.host.test/foo/bar", "A=a X=x Z=z D=d")
        ])
    ),
    new(
        "Sorting of same-name cookies.",
        "http://www.host.test/",
        slice<@string>([
            "A=1; path=/",
            "A=2; path=/path",
            "A=3; path=/quux",
            "A=4; path=/path/foo",
            "A=5; domain=.host.test; path=/path",
            "A=6; domain=.host.test; path=/quux",
            "A=7; domain=.host.test; path=/path/foo"
        ]),
        "A=1 A=2 A=3 A=4 A=5 A=6 A=7",
        slice<query>([
            new("http://www.host.test/path", "A=2 A=5 A=1"),
            new("http://www.host.test/path/foo", "A=4 A=7 A=2 A=5 A=1")
        ])
    ),
    new(
        "Disallow domain cookie on public suffix.",
        "http://www.bbc.co.uk",
        slice<@string>([
            "a=1",
            "b=2; domain=co.uk"
        ]),
        "a=1",
        slice<query>([new("http://www.bbc.co.uk", "a=1")])
    ),
    new(
        "Host cookie on IP.",
        "http://192.168.0.10",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([new("http://192.168.0.10", "a=1")])
    ),
    new(
        "Domain cookies on IP.",
        "http://192.168.0.10",
        slice<@string>([
            "a=1; domain=192.168.0.10", // allowed
            "b=2; domain=172.31.9.9",   // rejected, can't set cookie for other IP
            "c=3; domain=.192.168.0.10" // rejected like in most browsers
        ]),
        "a=1",
        slice<query>([
            new("http://192.168.0.10", "a=1"),
            new("http://172.31.9.9", ""),
            new("http://www.fancy.192.168.0.10", "")
        ])
    ),
    new(
        "Port is ignored #1.",
        "http://www.host.test/",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://www.host.test", "a=1"),
            new("http://www.host.test:8080/", "a=1")
        ])
    ),
    new(
        "Port is ignored #2.",
        "http://www.host.test:8080/",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://www.host.test", "a=1"),
            new("http://www.host.test:8080/", "a=1"),
            new("http://www.host.test:1234/", "a=1")
        ])
    ),
    new(
        "IPv6 zone is not treated as a host.",
        "https://example.com/",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("https://[::1%25.example.com]:80/", "")
        ])
    ),
    new(
        "Retrieval of cookies with quoted values", // issue #46443
        "http://www.host.test/",
        slice<@string>([
            @"cookie-1=""quoted""",
            @"cookie-2=""quoted with spaces""",
            @"cookie-3=""quoted,with,commas""",
            @"cookie-4= ,"
        ]),
        @"cookie-1=""quoted"" cookie-2=""quoted with spaces"" cookie-3=""quoted,with,commas"" cookie-4="" ,""",
        slice<query>([
            new(
                "http://www.host.test",
                @"cookie-1=""quoted"" cookie-2=""quoted with spaces"" cookie-3=""quoted,with,commas"" cookie-4="" ,"""
            )
        ])
    )
}.array();

public static void TestBasics(ж<testing.T> t) {
    foreach (var (_, test) in basicsTests.ΔRangeSnapshot()) {
        var jar = newTestJar();
        test.run(t, jar);
    }
}

// updateAndDeleteTests contains jarTests which must be performed on the same
// Jar.
internal static array<jarTest> updateAndDeleteTests = new jarTest[]{
    new(
        "Set initial cookies.",
        "http://www.host.test",
        slice<@string>([
            "a=1",
            "b=2; secure",
            "c=3; httponly",
            "d=4; secure; httponly"]),
        "a=1 b=2 c=3 d=4",
        slice<query>([
            new("http://www.host.test", "a=1 c=3"),
            new("https://www.host.test", "a=1 b=2 c=3 d=4")
        ])
    ),
    new(
        "Update value via http.",
        "http://www.host.test",
        slice<@string>([
            "a=w",
            "b=x; secure",
            "c=y; httponly",
            "d=z; secure; httponly"]),
        "a=w b=x c=y d=z",
        slice<query>([
            new("http://www.host.test", "a=w c=y"),
            new("https://www.host.test", "a=w b=x c=y d=z")
        ])
    ),
    new(
        "Clear Secure flag from an http.",
        "http://www.host.test/",
        slice<@string>([
            "b=xx",
            "d=zz; httponly"]),
        "a=w b=xx c=y d=zz",
        slice<query>([new("http://www.host.test", "a=w b=xx c=y d=zz")])
    ),
    new(
        "Delete all.",
        "http://www.host.test/",
        slice<@string>([
            "a=1; max-Age=-1",                       // delete via MaxAge
            "b=2; "u8 + expiresIn(-10),              // delete via Expires
            "c=2; max-age=-1; "u8 + expiresIn(-10),  // delete via both
            "d=4; max-age=-1; "u8 + expiresIn(10)]), // MaxAge takes precedence
        "",
        slice<query>([new("http://www.host.test", "")])
    ),
    new(
        "Refill #1.",
        "http://www.host.test",
        slice<@string>([
            "A=1",
            "A=2; path=/foo",
            "A=3; domain=.host.test",
            "A=4; path=/foo; domain=.host.test"]),
        "A=1 A=2 A=3 A=4",
        slice<query>([new("http://www.host.test/foo", "A=2 A=4 A=1 A=3")])
    ),
    new(
        "Refill #2.",
        "http://www.google.com",
        slice<@string>([
            "A=6",
            "A=7; path=/foo",
            "A=8; domain=.google.com",
            "A=9; path=/foo; domain=.google.com"]),
        "A=1 A=2 A=3 A=4 A=6 A=7 A=8 A=9",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=4 A=1 A=3"),
            new("http://www.google.com/foo", "A=7 A=9 A=6 A=8")
        ])
    ),
    new(
        "Delete A7.",
        "http://www.google.com",
        slice<@string>(["A=; path=/foo; max-age=-1"]),
        "A=1 A=2 A=3 A=4 A=6 A=8 A=9",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=4 A=1 A=3"),
            new("http://www.google.com/foo", "A=9 A=6 A=8")
        ])
    ),
    new(
        "Delete A4.",
        "http://www.host.test",
        slice<@string>(["A=; path=/foo; domain=host.test; max-age=-1"]),
        "A=1 A=2 A=3 A=6 A=8 A=9",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=1 A=3"),
            new("http://www.google.com/foo", "A=9 A=6 A=8")
        ])
    ),
    new(
        "Delete A6.",
        "http://www.google.com",
        slice<@string>(["A=; max-age=-1"]),
        "A=1 A=2 A=3 A=8 A=9",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=1 A=3"),
            new("http://www.google.com/foo", "A=9 A=8")
        ])
    ),
    new(
        "Delete A3.",
        "http://www.host.test",
        slice<@string>(["A=; domain=host.test; max-age=-1"]),
        "A=1 A=2 A=8 A=9",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=1"),
            new("http://www.google.com/foo", "A=9 A=8")
        ])
    ),
    new(
        "No cross-domain delete.",
        "http://www.host.test",
        slice<@string>([
            "A=; domain=google.com; max-age=-1",
            "A=; path=/foo; domain=google.com; max-age=-1"]),
        "A=1 A=2 A=8 A=9",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=1"),
            new("http://www.google.com/foo", "A=9 A=8")
        ])
    ),
    new(
        "Delete A8 and A9.",
        "http://www.google.com",
        slice<@string>([
            "A=; domain=google.com; max-age=-1",
            "A=; path=/foo; domain=google.com; max-age=-1"]),
        "A=1 A=2",
        slice<query>([
            new("http://www.host.test/foo", "A=2 A=1"),
            new("http://www.google.com/foo", "")
        ])
    )
}.array();

public static void TestUpdateAndDelete(ж<testing.T> t) {
    var jar = newTestJar();
    foreach (var (_, test) in updateAndDeleteTests.ΔRangeSnapshot()) {
        test.run(t, jar);
    }
}

public static void TestExpiration(ж<testing.T> t) {
    var jar = newTestJar();
    new jarTest(
        "Expiration.",
        "http://www.host.test",
        slice<@string>([
            "a=1",
            "b=2; max-age=3",
            "c=3; "u8 + expiresIn(3),
            "d=4; max-age=5",
            "e=5; "u8 + expiresIn(5),
            "f=6; max-age=100"
        ]),
        "a=1 b=2 c=3 d=4 e=5 f=6", // executed at t0 + 1001 ms
        slice<query>([
            new("http://www.host.test", "a=1 b=2 c=3 d=4 e=5 f=6"), // t0 + 2002 ms
            new("http://www.host.test", "a=1 d=4 e=5 f=6"),         // t0 + 3003 ms
            new("http://www.host.test", "a=1 d=4 e=5 f=6"),         // t0 + 4004 ms
            new("http://www.host.test", "a=1 f=6"),                 // t0 + 5005 ms
            new("http://www.host.test", "a=1 f=6")                  // t0 + 6006 ms
        ])
    ).run(t, jar);
}

//
// Tests derived from Chromium's cookie_store_unittest.h.
//
// See http://src.chromium.org/viewvc/chrome/trunk/src/net/cookies/cookie_store_unittest.h?revision=159685&content-type=text/plain
// Some of the original tests are in a bad condition (e.g.
// DomainWithTrailingDotTest) or are not RFC 6265 conforming (e.g.
// TestNonDottedAndTLD #1 and #6) and have not been ported.

// chromiumBasicsTests contains fundamental tests. Each jarTest has to be
// performed on a fresh, empty Jar.
internal static array<jarTest> chromiumBasicsTests = new jarTest[]{
    new(
        "DomainWithTrailingDotTest.",
        "http://www.google.com/",
        slice<@string>([
            "a=1; domain=.www.google.com.",
            "b=2; domain=.www.google.com.."]),
        "",
        slice<query>([
            new("http://www.google.com", "")
        ])
    ),
    new(
        "ValidSubdomainTest #1.",
        "http://a.b.c.d.com",
        slice<@string>([
            "a=1; domain=.a.b.c.d.com",
            "b=2; domain=.b.c.d.com",
            "c=3; domain=.c.d.com",
            "d=4; domain=.d.com"]),
        "a=1 b=2 c=3 d=4",
        slice<query>([
            new("http://a.b.c.d.com", "a=1 b=2 c=3 d=4"),
            new("http://b.c.d.com", "b=2 c=3 d=4"),
            new("http://c.d.com", "c=3 d=4"),
            new("http://d.com", "d=4")
        ])
    ),
    new(
        "ValidSubdomainTest #2.",
        "http://a.b.c.d.com",
        slice<@string>([
            "a=1; domain=.a.b.c.d.com",
            "b=2; domain=.b.c.d.com",
            "c=3; domain=.c.d.com",
            "d=4; domain=.d.com",
            "X=bcd; domain=.b.c.d.com",
            "X=cd; domain=.c.d.com"]),
        "X=bcd X=cd a=1 b=2 c=3 d=4",
        slice<query>([
            new("http://b.c.d.com", "b=2 c=3 d=4 X=bcd X=cd"),
            new("http://c.d.com", "c=3 d=4 X=cd")
        ])
    ),
    new(
        "InvalidDomainTest #1.",
        "http://foo.bar.com",
        slice<@string>([
            "a=1; domain=.yo.foo.bar.com",
            "b=2; domain=.foo.com",
            "c=3; domain=.bar.foo.com",
            "d=4; domain=.foo.bar.com.net",
            "e=5; domain=ar.com",
            "f=6; domain=.",
            "g=7; domain=/",
            "h=8; domain=http://foo.bar.com",
            "i=9; domain=..foo.bar.com",
            "j=10; domain=..bar.com",
            "k=11; domain=.foo.bar.com?blah",
            "l=12; domain=.foo.bar.com/blah",
            "m=12; domain=.foo.bar.com:80",
            "n=14; domain=.foo.bar.com:",
            "o=15; domain=.foo.bar.com#sup"
        ]),
        "", // Jar is empty.
        slice<query>([new("http://foo.bar.com", "")])
    ),
    new(
        "InvalidDomainTest #2.",
        "http://foo.com.com",
        slice<@string>(["a=1; domain=.foo.com.com.com"]),
        "",
        slice<query>([new("http://foo.bar.com", "")])
    ),
    new(
        "DomainWithoutLeadingDotTest #1.",
        "http://manage.hosted.filefront.com",
        slice<@string>(["a=1; domain=filefront.com"]),
        "a=1",
        slice<query>([new("http://www.filefront.com", "a=1")])
    ),
    new(
        "DomainWithoutLeadingDotTest #2.",
        "http://www.google.com",
        slice<@string>(["a=1; domain=www.google.com"]),
        "a=1",
        slice<query>([
            new("http://www.google.com", "a=1"),
            new("http://sub.www.google.com", "a=1"),
            new("http://something-else.com", "")
        ])
    ),
    new(
        "CaseInsensitiveDomainTest.",
        "http://www.google.com",
        slice<@string>([
            "a=1; domain=.GOOGLE.COM",
            "b=2; domain=.www.gOOgLE.coM"]),
        "a=1 b=2",
        slice<query>([new("http://www.google.com", "a=1 b=2")])
    ),
    new(
        "TestIpAddress #1.",
        "http://1.2.3.4/foo",
        slice<@string>(["a=1; path=/"]),
        "a=1",
        slice<query>([new("http://1.2.3.4/foo", "a=1")])
    ),
    new(
        "TestIpAddress #2.",
        "http://1.2.3.4/foo",
        slice<@string>([
            "a=1; domain=.1.2.3.4",
            "b=2; domain=.3.4"]),
        "",
        slice<query>([new("http://1.2.3.4/foo", "")])
    ),
    new(
        "TestIpAddress #3.",
        "http://1.2.3.4/foo",
        slice<@string>(["a=1; domain=1.2.3.3"]),
        "",
        slice<query>([new("http://1.2.3.4/foo", "")])
    ),
    new(
        "TestIpAddress #4.",
        "http://1.2.3.4/foo",
        slice<@string>(["a=1; domain=1.2.3.4"]),
        "a=1",
        slice<query>([new("http://1.2.3.4/foo", "a=1")])
    ),
    new(
        "TestNonDottedAndTLD #2.",
        "http://com./index.html",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://com./index.html", "a=1"),
            new("http://no-cookies.com./index.html", "")
        ])
    ),
    new(
        "TestNonDottedAndTLD #3.",
        "http://a.b",
        slice<@string>([
            "a=1; domain=.b",
            "b=2; domain=b"]),
        "",
        slice<query>([new("http://bar.foo", "")])
    ),
    new(
        "TestNonDottedAndTLD #4.",
        "http://google.com",
        slice<@string>([
            "a=1; domain=.com",
            "b=2; domain=com"]),
        "",
        slice<query>([new("http://google.com", "")])
    ),
    new(
        "TestNonDottedAndTLD #5.",
        "http://google.co.uk",
        slice<@string>([
            "a=1; domain=.co.uk",
            "b=2; domain=.uk"]),
        "",
        slice<query>([
            new("http://google.co.uk", ""),
            new("http://else.co.com", ""),
            new("http://else.uk", "")
        ])
    ),
    new(
        "TestHostEndsWithDot.",
        "http://www.google.com",
        slice<@string>([
            "a=1",
            "b=2; domain=.www.google.com."]),
        "a=1",
        slice<query>([new("http://www.google.com", "a=1")])
    ),
    new(
        "PathTest",
        "http://www.google.izzle",
        slice<@string>(["a=1; path=/wee"]),
        "a=1",
        slice<query>([
            new("http://www.google.izzle/wee", "a=1"),
            new("http://www.google.izzle/wee/", "a=1"),
            new("http://www.google.izzle/wee/war", "a=1"),
            new("http://www.google.izzle/wee/war/more/more", "a=1"),
            new("http://www.google.izzle/weehee", ""),
            new("http://www.google.izzle/", "")
        ])
    )
}.array();

public static void TestChromiumBasics(ж<testing.T> t) {
    foreach (var (_, test) in chromiumBasicsTests.ΔRangeSnapshot()) {
        var jar = newTestJar();
        test.run(t, jar);
    }
}

// chromiumDomainTests contains jarTests which must be executed all on the
// same Jar.
internal static array<jarTest> chromiumDomainTests = new jarTest[]{
    new(
        "Fill #1.",
        "http://www.google.izzle",
        slice<@string>(["A=B"]),
        "A=B",
        slice<query>([new("http://www.google.izzle", "A=B")])
    ),
    new(
        "Fill #2.",
        "http://www.google.izzle",
        slice<@string>(["C=D; domain=.google.izzle"]),
        "A=B C=D",
        slice<query>([new("http://www.google.izzle", "A=B C=D")])
    ),
    new(
        "Verify A is a host cookie and not accessible from subdomain.",
        "http://unused.nil",
        slice<@string>([]),
        "A=B C=D",
        slice<query>([new("http://foo.www.google.izzle", "C=D")])
    ),
    new(
        "Verify domain cookies are found on proper domain.",
        "http://www.google.izzle",
        slice<@string>(["E=F; domain=.www.google.izzle"]),
        "A=B C=D E=F",
        slice<query>([new("http://www.google.izzle", "A=B C=D E=F")])
    ),
    new(
        "Leading dots in domain attributes are optional.",
        "http://www.google.izzle",
        slice<@string>(["G=H; domain=www.google.izzle"]),
        "A=B C=D E=F G=H",
        slice<query>([new("http://www.google.izzle", "A=B C=D E=F G=H")])
    ),
    new(
        "Verify domain enforcement works #1.",
        "http://www.google.izzle",
        slice<@string>(["K=L; domain=.bar.www.google.izzle"]),
        "A=B C=D E=F G=H",
        slice<query>([new("http://bar.www.google.izzle", "C=D E=F G=H")])
    ),
    new(
        "Verify domain enforcement works #2.",
        "http://unused.nil",
        slice<@string>([]),
        "A=B C=D E=F G=H",
        slice<query>([new("http://www.google.izzle", "A=B C=D E=F G=H")])
    )
}.array();

public static void TestChromiumDomain(ж<testing.T> t) {
    var jar = newTestJar();
    foreach (var (_, test) in chromiumDomainTests.ΔRangeSnapshot()) {
        test.run(t, jar);
    }
}

// chromiumDeletionTests must be performed all on the same Jar.
internal static array<jarTest> chromiumDeletionTests = new jarTest[]{
    new(
        "Create session cookie a1.",
        "http://www.google.com",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([new("http://www.google.com", "a=1")])
    ),
    new(
        "Delete sc a1 via MaxAge.",
        "http://www.google.com",
        slice<@string>(["a=1; max-age=-1"]),
        "",
        slice<query>([new("http://www.google.com", "")])
    ),
    new(
        "Create session cookie b2.",
        "http://www.google.com",
        slice<@string>(["b=2"]),
        "b=2",
        slice<query>([new("http://www.google.com", "b=2")])
    ),
    new(
        "Delete sc b2 via Expires.",
        "http://www.google.com",
        slice<@string>(["b=2; "u8 + expiresIn(-10)]),
        "",
        slice<query>([new("http://www.google.com", "")])
    ),
    new(
        "Create persistent cookie c3.",
        "http://www.google.com",
        slice<@string>(["c=3; max-age=3600"]),
        "c=3",
        slice<query>([new("http://www.google.com", "c=3")])
    ),
    new(
        "Delete pc c3 via MaxAge.",
        "http://www.google.com",
        slice<@string>(["c=3; max-age=-1"]),
        "",
        slice<query>([new("http://www.google.com", "")])
    ),
    new(
        "Create persistent cookie d4.",
        "http://www.google.com",
        slice<@string>(["d=4; max-age=3600"]),
        "d=4",
        slice<query>([new("http://www.google.com", "d=4")])
    ),
    new(
        "Delete pc d4 via Expires.",
        "http://www.google.com",
        slice<@string>(["d=4; "u8 + expiresIn(-10)]),
        "",
        slice<query>([new("http://www.google.com", "")])
    )
}.array();

public static void TestChromiumDeletion(ж<testing.T> t) {
    var jar = newTestJar();
    foreach (var (_, test) in chromiumDeletionTests.ΔRangeSnapshot()) {
        test.run(t, jar);
    }
}

// domainHandlingTests tests and documents the rules for domain handling.
// Each test must be performed on an empty new Jar.
internal static array<jarTest> domainHandlingTests = new jarTest[]{
    new(
        "Host cookie",
        "http://www.host.test",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://www.host.test", "a=1"),
            new("http://host.test", ""),
            new("http://bar.host.test", ""),
            new("http://foo.www.host.test", ""),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Domain cookie #1",
        "http://www.host.test",
        slice<@string>(["a=1; domain=host.test"]),
        "a=1",
        slice<query>([
            new("http://www.host.test", "a=1"),
            new("http://host.test", "a=1"),
            new("http://bar.host.test", "a=1"),
            new("http://foo.www.host.test", "a=1"),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Domain cookie #2",
        "http://www.host.test",
        slice<@string>(["a=1; domain=.host.test"]),
        "a=1",
        slice<query>([
            new("http://www.host.test", "a=1"),
            new("http://host.test", "a=1"),
            new("http://bar.host.test", "a=1"),
            new("http://foo.www.host.test", "a=1"),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Host cookie on IDNA domain #1",
        "http://www.bücher.test",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://www.bücher.test", "a=1"),
            new("http://www.xn--bcher-kva.test", "a=1"),
            new("http://bücher.test", ""),
            new("http://xn--bcher-kva.test", ""),
            new("http://bar.bücher.test", ""),
            new("http://bar.xn--bcher-kva.test", ""),
            new("http://foo.www.bücher.test", ""),
            new("http://foo.www.xn--bcher-kva.test", ""),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Host cookie on IDNA domain #2",
        "http://www.xn--bcher-kva.test",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://www.bücher.test", "a=1"),
            new("http://www.xn--bcher-kva.test", "a=1"),
            new("http://bücher.test", ""),
            new("http://xn--bcher-kva.test", ""),
            new("http://bar.bücher.test", ""),
            new("http://bar.xn--bcher-kva.test", ""),
            new("http://foo.www.bücher.test", ""),
            new("http://foo.www.xn--bcher-kva.test", ""),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Domain cookie on IDNA domain #1",
        "http://www.bücher.test",
        slice<@string>(["a=1; domain=xn--bcher-kva.test"]),
        "a=1",
        slice<query>([
            new("http://www.bücher.test", "a=1"),
            new("http://www.xn--bcher-kva.test", "a=1"),
            new("http://bücher.test", "a=1"),
            new("http://xn--bcher-kva.test", "a=1"),
            new("http://bar.bücher.test", "a=1"),
            new("http://bar.xn--bcher-kva.test", "a=1"),
            new("http://foo.www.bücher.test", "a=1"),
            new("http://foo.www.xn--bcher-kva.test", "a=1"),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Domain cookie on IDNA domain #2",
        "http://www.xn--bcher-kva.test",
        slice<@string>(["a=1; domain=xn--bcher-kva.test"]),
        "a=1",
        slice<query>([
            new("http://www.bücher.test", "a=1"),
            new("http://www.xn--bcher-kva.test", "a=1"),
            new("http://bücher.test", "a=1"),
            new("http://xn--bcher-kva.test", "a=1"),
            new("http://bar.bücher.test", "a=1"),
            new("http://bar.xn--bcher-kva.test", "a=1"),
            new("http://foo.www.bücher.test", "a=1"),
            new("http://foo.www.xn--bcher-kva.test", "a=1"),
            new("http://other.test", ""),
            new("http://test", "")
        ])
    ),
    new(
        "Host cookie on TLD.",
        "http://com",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://com", "a=1"),
            new("http://any.com", ""),
            new("http://any.test", "")
        ])
    ),
    new(
        "Domain cookie on TLD becomes a host cookie.",
        "http://com",
        slice<@string>(["a=1; domain=com"]),
        "a=1",
        slice<query>([
            new("http://com", "a=1"),
            new("http://any.com", ""),
            new("http://any.test", "")
        ])
    ),
    new(
        "Host cookie on public suffix.",
        "http://co.uk",
        slice<@string>(["a=1"]),
        "a=1",
        slice<query>([
            new("http://co.uk", "a=1"),
            new("http://uk", ""),
            new("http://some.co.uk", ""),
            new("http://foo.some.co.uk", ""),
            new("http://any.uk", "")
        ])
    ),
    new(
        "Domain cookie on public suffix is ignored.",
        "http://some.co.uk",
        slice<@string>(["a=1; domain=co.uk"]),
        "",
        slice<query>([
            new("http://co.uk", ""),
            new("http://uk", ""),
            new("http://some.co.uk", ""),
            new("http://foo.some.co.uk", ""),
            new("http://any.uk", "")
        ])
    )
}.array();

public static void TestDomainHandling(ж<testing.T> t) {
    foreach (var (_, test) in domainHandlingTests.ΔRangeSnapshot()) {
        var jar = newTestJar();
        test.run(t, jar);
    }
}

public static void TestIssue19384(ж<testing.T> t) {
    var cookies = slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "name", Value: "value"))]);
    foreach (var (_, vᴛ1) in slice<@string>(["", ".", "..", "..."])) {
        ref var host = ref heap(new @string(), out var Ꮡhost);
        host = vᴛ1;

        var (jar, _) = New(nil);
        var u = Ꮡ(new url.URL(Scheme: "http", Host: host, Path: "/"));
        {
            var got = jar.Cookies(u); if (len(got) != 0) {
                t.Errorf("host %q, got %v", host, got);
            }
        }
        jar.SetCookies(u, cookies);
        {
            var got = jar.Cookies(u); if (len(got) != 1 || (~got[0]).Value != "value"u8) {
                t.Errorf("host %q, got %v", host, got);
            }
        }
    }
}

} // end cookiejar_internal_test_package
