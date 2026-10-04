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
[GoType] internal partial struct testPSL {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testPSLˢ = "testPSL"u8;

internal static @string String(this testPSL _) {
    return testPSLˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string coUkˢ = ".co.uk"u8;
internal static readonly @string coUkˢ2 = "co.uk"u8;
internal static readonly @string comˢ = "com"u8;

internal static @string PublicSuffix(this testPSL _, @string d) {
    if (d == "co.uk"u8 || strings.HasSuffix(d, coUkˢ)) {
        return coUkˢ2;
    }
    if (d == "www.buggy.psl"u8) {
        return "xy"u8;
    }
    if (d == "www2.buggy.psl"u8) {
        return comˢ;
    }
    return d.slice(strings.LastIndex(d, "."u8) + 1);
}

// newTestJar creates an empty Jar with testPSL as the public suffix list.
internal static ж<global::go.net.http.cookiejar_package.Jar> newTestJar() {
    var (jar, err) = New(Ꮡ(new Options(PublicSuffixList: new testPSL(nil))));
    if (err != default!) {
        throw panic(err);
    }
    return jar;
}


[GoType("dyn")] partial struct hasDotSuffixTestsᴛ1 {
    internal @string s, suffix;
}
internal static array<hasDotSuffixTestsᴛ1> hasDotSuffixTests = new hasDotSuffixTestsᴛ1[]{
    new(""u8, ""u8),
    new(""u8, "."u8),
    new(""u8, "x"u8),
    new("."u8, ""u8),
    new("."u8, "."u8),
    new("."u8, ".."u8),
    new("."u8, "x"u8),
    new("."u8, "x."u8),
    new("."u8, ".x"u8),
    new("."u8, ".x."u8),
    new("x"u8, ""u8),
    new("x"u8, "."u8),
    new("x"u8, ".."u8),
    new("x"u8, "x"u8),
    new("x"u8, "x."u8),
    new("x"u8, ".x"u8),
    new("x"u8, ".x."u8),
    new(".x"u8, ""u8),
    new(".x"u8, "."u8),
    new(".x"u8, ".."u8),
    new(".x"u8, "x"u8),
    new(".x"u8, "x."u8),
    new(".x"u8, ".x"u8),
    new(".x"u8, ".x."u8),
    new("x."u8, ""u8),
    new("x."u8, "."u8),
    new("x."u8, ".."u8),
    new("x."u8, "x"u8),
    new("x."u8, "x."u8),
    new("x."u8, ".x"u8),
    new("x."u8, ".x."u8),
    new("com"u8, ""u8),
    new("com"u8, "m"u8),
    new("com"u8, "om"u8),
    new("com"u8, "com"u8),
    new("com"u8, ".com"u8),
    new("com"u8, "x.com"u8),
    new("com"u8, "xcom"u8),
    new("com"u8, "xorg"u8),
    new("com"u8, "org"u8),
    new("com"u8, "rg"u8),
    new("foo.com"u8, ""u8),
    new("foo.com"u8, "m"u8),
    new("foo.com"u8, "om"u8),
    new("foo.com"u8, "com"u8),
    new("foo.com"u8, ".com"u8),
    new("foo.com"u8, "o.com"u8),
    new("foo.com"u8, "oo.com"u8),
    new("foo.com"u8, "foo.com"u8),
    new("foo.com"u8, ".foo.com"u8),
    new("foo.com"u8, "x.foo.com"u8),
    new("foo.com"u8, "xfoo.com"u8),
    new("foo.com"u8, "xfoo.org"u8),
    new("foo.com"u8, "foo.org"u8),
    new("foo.com"u8, "oo.org"u8),
    new("foo.com"u8, "o.org"u8),
    new("foo.com"u8, ".org"u8),
    new("foo.com"u8, "org"u8),
    new("foo.com"u8, "rg"u8)
}.array();

public static void TestHasDotSuffix(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in hasDotSuffixTests.ΔRangeSnapshot()) {
        var got = hasDotSuffix(tc.s, tc.suffix);
        var want = strings.HasSuffix(tc.s, "."u8 + tc.suffix);
        if (got != want) {
            Ꮡt.Errorf("s=%q, suffix=%q: got %v, want %v"u8, tc.s, tc.suffix, got, want);
        }
    }
}

// TODO: Fix canonicalHost so that all of the following malformed
// domain names trigger an error. (This list is not exhaustive, e.g.
// malformed internationalized domain names are missing.)
internal static map<@string, @string> canonicalHostTests = new map<@string, @string>{
    ["www.example.com"u8] = "www.example.com"u8,
    ["WWW.EXAMPLE.COM"u8] = "www.example.com"u8,
    ["wWw.eXAmple.CoM"u8] = "www.example.com"u8,
    ["www.example.com:80"u8] = "www.example.com"u8,
    ["192.168.0.10"u8] = "192.168.0.10"u8,
    ["192.168.0.5:8080"u8] = "192.168.0.5"u8,
    ["2001:4860:0:2001::68"u8] = "2001:4860:0:2001::68"u8,
    ["[2001:4860:0:::68]:8080"u8] = "2001:4860:0:::68"u8,
    ["www.bücher.de"u8] = "www.xn--bcher-kva.de"u8,
    ["www.example.com."u8] = "www.example.com"u8,
    ["."u8] = ""u8,
    [".."u8] = "."u8,
    ["..."u8] = ".."u8,
    [".net"u8] = ".net"u8,
    [".net."u8] = ".net"u8,
    ["a.."u8] = "a."u8,
    ["b.a.."u8] = "b.a."u8,
    ["weird.stuff..."u8] = "weird.stuff.."u8,
    ["[bad.unmatched.bracket:"u8] = "error"u8
};

public static void TestCanonicalHost(ж<testing.T> Ꮡt) {
    foreach (var (h, want) in canonicalHostTests) {
        var (got, err) = canonicalHost(h);
        if (want == "error"u8) {
            if (err == default!) {
                Ꮡt.Errorf("%q: got %q and nil error, want non-nil"u8, h, got);
            }
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("%q: %v"u8, h, err);
            continue;
        }
        if (got != want) {
            Ꮡt.Errorf("%q: got %q, want %q"u8, h, got, want);
            continue;
        }
    }
}

internal static map<@string, bool> hasPortTests = new map<@string, bool>{
    ["www.example.com"u8] = false,
    ["www.example.com:80"u8] = true,
    ["127.0.0.1"u8] = false,
    ["127.0.0.1:8080"u8] = true,
    ["2001:4860:0:2001::68"u8] = false,
    ["[2001::0:::68]:80"u8] = true
};

public static void TestHasPort(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (host, want) in hasPortTests) {
        {
            var got = hasPort(host); if (got != want) {
                Ꮡt.Errorf("%q: got %t, want %t"u8, host, got, want);
            }
        }
    }
}

// The following are actual outputs of canonicalHost for
// malformed inputs to canonicalHost (see above).
internal static map<@string, @string> jarKeyTests = new map<@string, @string>{
    ["foo.www.example.com"u8] = "example.com"u8,
    ["www.example.com"u8] = "example.com"u8,
    ["example.com"u8] = "example.com"u8,
    ["com"u8] = "com"u8,
    ["foo.www.bbc.co.uk"u8] = "bbc.co.uk"u8,
    ["www.bbc.co.uk"u8] = "bbc.co.uk"u8,
    ["bbc.co.uk"u8] = "bbc.co.uk"u8,
    ["co.uk"u8] = "co.uk"u8,
    ["uk"u8] = "uk"u8,
    ["192.168.0.5"u8] = "192.168.0.5"u8,
    ["www.buggy.psl"u8] = "www.buggy.psl"u8,
    ["www2.buggy.psl"u8] = "buggy.psl"u8,
    [""u8] = ""u8,
    ["."u8] = "."u8,
    [".."u8] = "."u8,
    [".net"u8] = ".net"u8,
    ["a."u8] = "a."u8,
    ["b.a."u8] = "a."u8,
    ["weird.stuff.."u8] = "."u8
};

public static void TestJarKey(ж<testing.T> Ꮡt) {
    foreach (var (host, want) in jarKeyTests) {
        {
            @string got = jarKey(host, new testPSL(nil)); if (got != want) {
                Ꮡt.Errorf("%q: got %q, want %q"u8, host, got, want);
            }
        }
    }
}

// The following are actual outputs of canonicalHost for
// malformed inputs to canonicalHost.
internal static map<@string, @string> jarKeyNilPSLTests = new map<@string, @string>{
    ["foo.www.example.com"u8] = "example.com"u8,
    ["www.example.com"u8] = "example.com"u8,
    ["example.com"u8] = "example.com"u8,
    ["com"u8] = "com"u8,
    ["foo.www.bbc.co.uk"u8] = "co.uk"u8,
    ["www.bbc.co.uk"u8] = "co.uk"u8,
    ["bbc.co.uk"u8] = "co.uk"u8,
    ["co.uk"u8] = "co.uk"u8,
    ["uk"u8] = "uk"u8,
    ["192.168.0.5"u8] = "192.168.0.5"u8,
    [""u8] = ""u8,
    ["."u8] = "."u8,
    [".."u8] = ".."u8,
    [".net"u8] = ".net"u8,
    ["a."u8] = "a."u8,
    ["b.a."u8] = "a."u8,
    ["weird.stuff.."u8] = "stuff.."u8
};

public static void TestJarKeyNilPSL(ж<testing.T> Ꮡt) {
    foreach (var (host, want) in jarKeyNilPSLTests) {
        {
            @string got = jarKey(host, default!); if (got != want) {
                Ꮡt.Errorf("%q: got %q, want %q"u8, host, got, want);
            }
        }
    }
}

internal static map<@string, bool> isIPTests = new map<@string, bool>{
    ["127.0.0.1"u8] = true,
    ["1.2.3.4"u8] = true,
    ["2001:4860:0:2001::68"u8] = true,
    ["::1%zone"u8] = true,
    ["example.com"u8] = false,
    ["1.1.1.300"u8] = false,
    ["www.foo.bar.net"u8] = false,
    ["123.foo.bar.net"u8] = false
};

public static void TestIsIP(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (host, want) in isIPTests) {
        {
            var got = isIP(host); if (got != want) {
                Ꮡt.Errorf("%q: got %t, want %t"u8, host, got, want);
            }
        }
    }
}

internal static map<@string, @string> defaultPathTests = new map<@string, @string>{
    ["/"u8] = "/"u8,
    ["/abc"u8] = "/"u8,
    ["/abc/"u8] = "/abc"u8,
    ["/abc/xyz"u8] = "/abc"u8,
    ["/abc/xyz/"u8] = "/abc/xyz"u8,
    ["/a/b/c.html"u8] = "/a/b"u8,
    [""u8] = "/"u8,
    ["strange"u8] = "/"u8,
    ["//"u8] = "/"u8,
    ["/a//b"u8] = "/a/"u8,
    ["/a/./b"u8] = "/a/."u8,
    ["/a/../b"u8] = "/a/.."u8
};

public static void TestDefaultPath(ж<testing.T> Ꮡt) {
    foreach (var (path, want) in defaultPathTests) {
        {
            @string got = defaultPath(path); if (got != want) {
                Ꮡt.Errorf("%q: got %q, want %q"u8, path, got, want);
            }
        }
    }
}


[GoType("dyn")] partial struct domainAndTypeTestsᴛ1 {
    internal @string host; // host Set-Cookie header was received from
    internal @string domain; // domain attribute in Set-Cookie header
    internal @string wantDomain; // expected domain of cookie
    internal bool wantHostOnly;   // expected host-cookie flag
    internal error wantErr;  // expected error
}
internal static array<domainAndTypeTestsᴛ1> domainAndTypeTests;
internal static void initᴛdomainAndTypeTests() { domainAndTypeTests = new domainAndTypeTestsᴛ1[]{
    new("www.example.com"u8, ""u8, "www.example.com"u8, true, default!),
    new("127.0.0.1"u8, ""u8, "127.0.0.1"u8, true, default!),
    new("2001:4860:0:2001::68"u8, ""u8, "2001:4860:0:2001::68"u8, true, default!),
    new("www.example.com"u8, "example.com"u8, "example.com"u8, false, default!),
    new("www.example.com"u8, ".example.com"u8, "example.com"u8, false, default!),
    new("www.example.com"u8, "www.example.com"u8, "www.example.com"u8, false, default!),
    new("www.example.com"u8, ".www.example.com"u8, "www.example.com"u8, false, default!),
    new("foo.sso.example.com"u8, "sso.example.com"u8, "sso.example.com"u8, false, default!),
    new("bar.co.uk"u8, "bar.co.uk"u8, "bar.co.uk"u8, false, default!),
    new("foo.bar.co.uk"u8, ".bar.co.uk"u8, "bar.co.uk"u8, false, default!),
    new("127.0.0.1"u8, "127.0.0.1"u8, "127.0.0.1"u8, true, default!),
    new("2001:4860:0:2001::68"u8, "2001:4860:0:2001::68"u8, "2001:4860:0:2001::68"u8, true, default!),
    new("www.example.com"u8, "."u8, ""u8, false, errMalformedDomain),
    new("www.example.com"u8, ".."u8, ""u8, false, errMalformedDomain),
    new("www.example.com"u8, "other.com"u8, ""u8, false, errIllegalDomain),
    new("www.example.com"u8, "com"u8, ""u8, false, errIllegalDomain),
    new("www.example.com"u8, ".com"u8, ""u8, false, errIllegalDomain),
    new("foo.bar.co.uk"u8, ".co.uk"u8, ""u8, false, errIllegalDomain),
    new("127.www.0.0.1"u8, "127.0.0.1"u8, ""u8, false, errIllegalDomain),
    new("com"u8, ""u8, "com"u8, true, default!),
    new("com"u8, "com"u8, "com"u8, true, default!),
    new("com"u8, ".com"u8, "com"u8, true, default!),
    new("co.uk"u8, ""u8, "co.uk"u8, true, default!),
    new("co.uk"u8, "co.uk"u8, "co.uk"u8, true, default!),
    new("co.uk"u8, ".co.uk"u8, "co.uk"u8, true, default!)
}.array(); }

public static void TestDomainAndType(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var jar = newTestJar();
    foreach (var (_, tc) in domainAndTypeTests.ΔRangeSnapshot()) {
        var (domain, hostOnly, err) = jar.domainAndType(tc.host, tc.domain);
        if (!AreEqual(err, tc.wantErr)) {
            Ꮡt.Errorf("%q/%q: got %q error, want %v"u8,
                tc.host, tc.domain, err, tc.wantErr);
            continue;
        }
        if (err != default!) {
            continue;
        }
        if (domain != tc.wantDomain || hostOnly != tc.wantHostOnly) {
            Ꮡt.Errorf("%q/%q: got %q/%t want %q/%t"u8,
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
    if (err != default! || (~u).Scheme == ""u8 || (~u).Host == ""u8) {
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
[GoType] internal partial struct jarTest {
    internal @string description;  // The description of what this test is supposed to test
    internal @string fromURL;  // The full URL of the request from which Set-Cookie headers where received
    internal slice<@string> setCookies; // All the cookies received from fromURL
    internal @string content;  // The whole (non-expired) content of the jar
    internal slice<query> queries; // Queries to test the Jar.Cookies method
}

// query contains one test of the cookies returned from Jar.Cookies.
[GoType] internal partial struct query {
    internal @string toURL; // the URL in the Cookies call
    internal @string want; // the expected list of cookies (order matters)
}

// run runs the jarTest.
internal static void run(this jarTest test, ж<testing.T> Ꮡt, ж<global::go.net.http.cookiejar_package.Jar> Ꮡjar) {
    ref var jar = ref Ꮡjar.DerefOrNull();

    var now = tNow;
    // Populate jar with cookies.
    var setCookies = new slice<ж<httpꓸCookie>>(len(test.setCookies));
    foreach (var (i, csΔ1) in test.setCookies) {
        var cookies = (Ꮡ(new http.Response(Header: new httpꓸHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{csΔ1}.slice()})))).Cookies();
        if (len(cookies) != 1) {
            throw panic(fmt.Sprintf("Wrong cookie line %q: %#v"u8, csΔ1, cookies));
        }
        setCookies[i] = cookies[0];
    }
    Ꮡjar.setCookies(mustParseURL(test.fromURL), setCookies, now);
    now = now.Add(1001 * time.Millisecond);
    // Serialize non-expired entries in the form "name1=val1 name2=val2".
    slice<@string> cs = default!;
    foreach (var (_, submap) in jar.entries) {
        foreach (var (_, cookie) in submap) {
            if (!cookie.Expires.After(now)) {
                continue;
            }
            @string v = cookie.Value;
            if (strings.ContainsAny(v, " ,"u8) || cookie.Quoted) {
                v = @""""u8 + v + @""""u8;
            }
            cs = append(cs, cookie.Name + "="u8 + v);
        }
    }
    slices.Sort<slice<@string>, @string>(cs);
    @string got = strings.Join(cs, " "u8);
    // Make sure jar content matches our expectations.
    if (got != test.content) {
        Ꮡt.Errorf("Test %q Content\ngot  %q\nwant %q"u8,
            test.description, got, test.content);
    }
    // Test different calls to Cookies.
    foreach (var (i, query) in test.queries) {
        now = now.Add(1001 * time.Millisecond);
        slice<@string> s = default!;
        foreach (var (_, c) in Ꮡjar.cookies(mustParseURL(query.toURL), now)) {
            s = append(s, c.String());
        }
        {
            @string gotΔ1 = strings.Join(s, " "u8); if (gotΔ1 != query.want) {
                Ꮡt.Errorf("Test %q #%d\ngot  %q\nwant %q"u8, test.description, i, gotΔ1, query.want);
            }
        }
    }
}

// allowed
// rejected, can't set cookie for other IP
// rejected like in most browsers
// issue #46443
// basicsTests contains fundamental tests. Each jarTest has to be performed on
// a fresh, empty Jar.
internal static array<jarTest> basicsTests = new jarTest[]{
    new(
        "Retrieval of a plain host cookie."u8,
        "http://www.host.test/"u8,
        new @string[]{"A=a"u8}.slice(),
        "A=a"u8,
        new query[]{
            new("http://www.host.test"u8, "A=a"u8),
            new("http://www.host.test/"u8, "A=a"u8),
            new("http://www.host.test/some/path"u8, "A=a"u8),
            new("https://www.host.test"u8, "A=a"u8),
            new("https://www.host.test/"u8, "A=a"u8),
            new("https://www.host.test/some/path"u8, "A=a"u8),
            new("ftp://www.host.test"u8, ""u8),
            new("ftp://www.host.test/"u8, ""u8),
            new("ftp://www.host.test/some/path"u8, ""u8),
            new("http://www.other.org"u8, ""u8),
            new("http://sibling.host.test"u8, ""u8),
            new("http://deep.www.host.test"u8, ""u8)
        }.slice()
    ),
    new(
        "Secure cookies are not returned to http."u8,
        "http://www.host.test/"u8,
        new @string[]{"A=a; secure"u8}.slice(),
        "A=a"u8,
        new query[]{
            new("http://www.host.test"u8, ""u8),
            new("http://www.host.test/"u8, ""u8),
            new("http://www.host.test/some/path"u8, ""u8),
            new("https://www.host.test"u8, "A=a"u8),
            new("https://www.host.test/"u8, "A=a"u8),
            new("https://www.host.test/some/path"u8, "A=a"u8)
        }.slice()
    ),
    new(
        "Explicit path."u8,
        "http://www.host.test/"u8,
        new @string[]{"A=a; path=/some/path"u8}.slice(),
        "A=a"u8,
        new query[]{
            new("http://www.host.test"u8, ""u8),
            new("http://www.host.test/"u8, ""u8),
            new("http://www.host.test/some"u8, ""u8),
            new("http://www.host.test/some/"u8, ""u8),
            new("http://www.host.test/some/path"u8, "A=a"u8),
            new("http://www.host.test/some/paths"u8, ""u8),
            new("http://www.host.test/some/path/foo"u8, "A=a"u8),
            new("http://www.host.test/some/path/foo/"u8, "A=a"u8)
        }.slice()
    ),
    new(
        "Implicit path #1: path is a directory."u8,
        "http://www.host.test/some/path/"u8,
        new @string[]{"A=a"u8}.slice(),
        "A=a"u8,
        new query[]{
            new("http://www.host.test"u8, ""u8),
            new("http://www.host.test/"u8, ""u8),
            new("http://www.host.test/some"u8, ""u8),
            new("http://www.host.test/some/"u8, ""u8),
            new("http://www.host.test/some/path"u8, "A=a"u8),
            new("http://www.host.test/some/paths"u8, ""u8),
            new("http://www.host.test/some/path/foo"u8, "A=a"u8),
            new("http://www.host.test/some/path/foo/"u8, "A=a"u8)
        }.slice()
    ),
    new(
        "Implicit path #2: path is not a directory."u8,
        "http://www.host.test/some/path/index.html"u8,
        new @string[]{"A=a"u8}.slice(),
        "A=a"u8,
        new query[]{
            new("http://www.host.test"u8, ""u8),
            new("http://www.host.test/"u8, ""u8),
            new("http://www.host.test/some"u8, ""u8),
            new("http://www.host.test/some/"u8, ""u8),
            new("http://www.host.test/some/path"u8, "A=a"u8),
            new("http://www.host.test/some/paths"u8, ""u8),
            new("http://www.host.test/some/path/foo"u8, "A=a"u8),
            new("http://www.host.test/some/path/foo/"u8, "A=a"u8)
        }.slice()
    ),
    new(
        "Implicit path #3: no path in URL at all."u8,
        "http://www.host.test"u8,
        new @string[]{"A=a"u8}.slice(),
        "A=a"u8,
        new query[]{
            new("http://www.host.test"u8, "A=a"u8),
            new("http://www.host.test/"u8, "A=a"u8),
            new("http://www.host.test/some/path"u8, "A=a"u8)
        }.slice()
    ),
    new(
        "Cookies are sorted by path length."u8,
        "http://www.host.test/"u8,
        new @string[]{
            "A=a; path=/foo/bar"u8,
            "B=b; path=/foo/bar/baz/qux"u8,
            "C=c; path=/foo/bar/baz"u8,
            "D=d; path=/foo"u8}.slice(),
        "A=a B=b C=c D=d"u8,
        new query[]{
            new("http://www.host.test/foo/bar/baz/qux"u8, "B=b C=c A=a D=d"u8),
            new("http://www.host.test/foo/bar/baz/"u8, "C=c A=a D=d"u8),
            new("http://www.host.test/foo/bar"u8, "A=a D=d"u8)
        }.slice()
    ),
    new(
        "Creation time determines sorting on same length paths."u8,
        "http://www.host.test/"u8,
        new @string[]{
            "A=a; path=/foo/bar"u8,
            "X=x; path=/foo/bar"u8,
            "Y=y; path=/foo/bar/baz/qux"u8,
            "B=b; path=/foo/bar/baz/qux"u8,
            "C=c; path=/foo/bar/baz"u8,
            "W=w; path=/foo/bar/baz"u8,
            "Z=z; path=/foo"u8,
            "D=d; path=/foo"u8}.slice(),
        "A=a B=b C=c D=d W=w X=x Y=y Z=z"u8,
        new query[]{
            new("http://www.host.test/foo/bar/baz/qux"u8, "Y=y B=b C=c W=w A=a X=x Z=z D=d"u8),
            new("http://www.host.test/foo/bar/baz/"u8, "C=c W=w A=a X=x Z=z D=d"u8),
            new("http://www.host.test/foo/bar"u8, "A=a X=x Z=z D=d"u8)
        }.slice()
    ),
    new(
        "Sorting of same-name cookies."u8,
        "http://www.host.test/"u8,
        new @string[]{
            "A=1; path=/"u8,
            "A=2; path=/path"u8,
            "A=3; path=/quux"u8,
            "A=4; path=/path/foo"u8,
            "A=5; domain=.host.test; path=/path"u8,
            "A=6; domain=.host.test; path=/quux"u8,
            "A=7; domain=.host.test; path=/path/foo"u8
        }.slice(),
        "A=1 A=2 A=3 A=4 A=5 A=6 A=7"u8,
        new query[]{
            new("http://www.host.test/path"u8, "A=2 A=5 A=1"u8),
            new("http://www.host.test/path/foo"u8, "A=4 A=7 A=2 A=5 A=1"u8)
        }.slice()
    ),
    new(
        "Disallow domain cookie on public suffix."u8,
        "http://www.bbc.co.uk"u8,
        new @string[]{
            "a=1"u8,
            "b=2; domain=co.uk"u8
        }.slice(),
        "a=1"u8,
        new query[]{new("http://www.bbc.co.uk"u8, "a=1"u8)}.slice()
    ),
    new(
        "Host cookie on IP."u8,
        "http://192.168.0.10"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{new("http://192.168.0.10"u8, "a=1"u8)}.slice()
    ),
    new(
        "Domain cookies on IP."u8,
        "http://192.168.0.10"u8,
        new @string[]{
            "a=1; domain=192.168.0.10"u8,
            "b=2; domain=172.31.9.9"u8,
            "c=3; domain=.192.168.0.10"u8
        }.slice(),
        "a=1"u8,
        new query[]{
            new("http://192.168.0.10"u8, "a=1"u8),
            new("http://172.31.9.9"u8, ""u8),
            new("http://www.fancy.192.168.0.10"u8, ""u8)
        }.slice()
    ),
    new(
        "Port is ignored #1."u8,
        "http://www.host.test/"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.host.test"u8, "a=1"u8),
            new("http://www.host.test:8080/"u8, "a=1"u8)
        }.slice()
    ),
    new(
        "Port is ignored #2."u8,
        "http://www.host.test:8080/"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.host.test"u8, "a=1"u8),
            new("http://www.host.test:8080/"u8, "a=1"u8),
            new("http://www.host.test:1234/"u8, "a=1"u8)
        }.slice()
    ),
    new(
        "IPv6 zone is not treated as a host."u8,
        "https://example.com/"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("https://[::1%25.example.com]:80/"u8, ""u8)
        }.slice()
    ),
    new(
        "Retrieval of cookies with quoted values"u8,
        "http://www.host.test/"u8,
        new @string[]{
            @"cookie-1=""quoted"""u8,
            @"cookie-2=""quoted with spaces"""u8,
            @"cookie-3=""quoted,with,commas"""u8,
            @"cookie-4= ,"u8
        }.slice(),
        @"cookie-1=""quoted"" cookie-2=""quoted with spaces"" cookie-3=""quoted,with,commas"" cookie-4="" ,"""u8,
        new query[]{
            new(
                "http://www.host.test"u8,
                @"cookie-1=""quoted"" cookie-2=""quoted with spaces"" cookie-3=""quoted,with,commas"" cookie-4="" ,"""u8
            )
        }.slice()
    )
}.array();

public static void TestBasics(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in basicsTests.ΔRangeSnapshot()) {
        var jar = newTestJar();
        test.run(Ꮡt, jar);
    }
}

// delete via MaxAge
// delete via Expires
// delete via both
// MaxAge takes precedence
// updateAndDeleteTests contains jarTests which must be performed on the same
// Jar.
internal static array<jarTest> updateAndDeleteTests = new jarTest[]{
    new(
        "Set initial cookies."u8,
        "http://www.host.test"u8,
        new @string[]{
            "a=1"u8,
            "b=2; secure"u8,
            "c=3; httponly"u8,
            "d=4; secure; httponly"u8}.slice(),
        "a=1 b=2 c=3 d=4"u8,
        new query[]{
            new("http://www.host.test"u8, "a=1 c=3"u8),
            new("https://www.host.test"u8, "a=1 b=2 c=3 d=4"u8)
        }.slice()
    ),
    new(
        "Update value via http."u8,
        "http://www.host.test"u8,
        new @string[]{
            "a=w"u8,
            "b=x; secure"u8,
            "c=y; httponly"u8,
            "d=z; secure; httponly"u8}.slice(),
        "a=w b=x c=y d=z"u8,
        new query[]{
            new("http://www.host.test"u8, "a=w c=y"u8),
            new("https://www.host.test"u8, "a=w b=x c=y d=z"u8)
        }.slice()
    ),
    new(
        "Clear Secure flag from an http."u8,
        "http://www.host.test/"u8,
        new @string[]{
            "b=xx"u8,
            "d=zz; httponly"u8}.slice(),
        "a=w b=xx c=y d=zz"u8,
        new query[]{new("http://www.host.test"u8, "a=w b=xx c=y d=zz"u8)}.slice()
    ),
    new(
        "Delete all."u8,
        "http://www.host.test/"u8,
        new @string[]{
            "a=1; max-Age=-1"u8,
            "b=2; "u8 + expiresIn(-10),
            "c=2; max-age=-1; "u8 + expiresIn(-10),
            "d=4; max-age=-1; "u8 + expiresIn(10)}.slice(),
        ""u8,
        new query[]{new("http://www.host.test"u8, ""u8)}.slice()
    ),
    new(
        "Refill #1."u8,
        "http://www.host.test"u8,
        new @string[]{
            "A=1"u8,
            "A=2; path=/foo"u8,
            "A=3; domain=.host.test"u8,
            "A=4; path=/foo; domain=.host.test"u8}.slice(),
        "A=1 A=2 A=3 A=4"u8,
        new query[]{new("http://www.host.test/foo"u8, "A=2 A=4 A=1 A=3"u8)}.slice()
    ),
    new(
        "Refill #2."u8,
        "http://www.google.com"u8,
        new @string[]{
            "A=6"u8,
            "A=7; path=/foo"u8,
            "A=8; domain=.google.com"u8,
            "A=9; path=/foo; domain=.google.com"u8}.slice(),
        "A=1 A=2 A=3 A=4 A=6 A=7 A=8 A=9"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=4 A=1 A=3"u8),
            new("http://www.google.com/foo"u8, "A=7 A=9 A=6 A=8"u8)
        }.slice()
    ),
    new(
        "Delete A7."u8,
        "http://www.google.com"u8,
        new @string[]{"A=; path=/foo; max-age=-1"u8}.slice(),
        "A=1 A=2 A=3 A=4 A=6 A=8 A=9"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=4 A=1 A=3"u8),
            new("http://www.google.com/foo"u8, "A=9 A=6 A=8"u8)
        }.slice()
    ),
    new(
        "Delete A4."u8,
        "http://www.host.test"u8,
        new @string[]{"A=; path=/foo; domain=host.test; max-age=-1"u8}.slice(),
        "A=1 A=2 A=3 A=6 A=8 A=9"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=1 A=3"u8),
            new("http://www.google.com/foo"u8, "A=9 A=6 A=8"u8)
        }.slice()
    ),
    new(
        "Delete A6."u8,
        "http://www.google.com"u8,
        new @string[]{"A=; max-age=-1"u8}.slice(),
        "A=1 A=2 A=3 A=8 A=9"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=1 A=3"u8),
            new("http://www.google.com/foo"u8, "A=9 A=8"u8)
        }.slice()
    ),
    new(
        "Delete A3."u8,
        "http://www.host.test"u8,
        new @string[]{"A=; domain=host.test; max-age=-1"u8}.slice(),
        "A=1 A=2 A=8 A=9"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=1"u8),
            new("http://www.google.com/foo"u8, "A=9 A=8"u8)
        }.slice()
    ),
    new(
        "No cross-domain delete."u8,
        "http://www.host.test"u8,
        new @string[]{
            "A=; domain=google.com; max-age=-1"u8,
            "A=; path=/foo; domain=google.com; max-age=-1"u8}.slice(),
        "A=1 A=2 A=8 A=9"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=1"u8),
            new("http://www.google.com/foo"u8, "A=9 A=8"u8)
        }.slice()
    ),
    new(
        "Delete A8 and A9."u8,
        "http://www.google.com"u8,
        new @string[]{
            "A=; domain=google.com; max-age=-1"u8,
            "A=; path=/foo; domain=google.com; max-age=-1"u8}.slice(),
        "A=1 A=2"u8,
        new query[]{
            new("http://www.host.test/foo"u8, "A=2 A=1"u8),
            new("http://www.google.com/foo"u8, ""u8)
        }.slice()
    )
}.array();

public static void TestUpdateAndDelete(ж<testing.T> Ꮡt) {
    var jar = newTestJar();
    foreach (var (_, test) in updateAndDeleteTests.ΔRangeSnapshot()) {
        test.run(Ꮡt, jar);
    }
}

public static void TestExpiration(ж<testing.T> Ꮡt) {
    var jar = newTestJar();
    new jarTest(
        "Expiration."u8,
        "http://www.host.test"u8,
        new @string[]{
            "a=1"u8,
            "b=2; max-age=3"u8,
            "c=3; "u8 + expiresIn(3),
            "d=4; max-age=5"u8,
            "e=5; "u8 + expiresIn(5),
            "f=6; max-age=100"u8
        }.slice(),
        "a=1 b=2 c=3 d=4 e=5 f=6"u8, // executed at t0 + 1001 ms

        new query[]{
            new("http://www.host.test"u8, "a=1 b=2 c=3 d=4 e=5 f=6"u8), // t0 + 2002 ms

            new("http://www.host.test"u8, "a=1 d=4 e=5 f=6"u8), // t0 + 3003 ms

            new("http://www.host.test"u8, "a=1 d=4 e=5 f=6"u8), // t0 + 4004 ms

            new("http://www.host.test"u8, "a=1 f=6"u8), // t0 + 5005 ms

            new("http://www.host.test"u8, "a=1 f=6"u8)
        }.slice()
    ).run(Ꮡt, // t0 + 6006 ms
 jar);
}

//
// Tests derived from Chromium's cookie_store_unittest.h.
//
// See http://src.chromium.org/viewvc/chrome/trunk/src/net/cookies/cookie_store_unittest.h?revision=159685&content-type=text/plain
// Some of the original tests are in a bad condition (e.g.
// DomainWithTrailingDotTest) or are not RFC 6265 conforming (e.g.
// TestNonDottedAndTLD #1 and #6) and have not been ported.
// Jar is empty.
// chromiumBasicsTests contains fundamental tests. Each jarTest has to be
// performed on a fresh, empty Jar.
internal static array<jarTest> chromiumBasicsTests = new jarTest[]{
    new(
        "DomainWithTrailingDotTest."u8,
        "http://www.google.com/"u8,
        new @string[]{
            "a=1; domain=.www.google.com."u8,
            "b=2; domain=.www.google.com.."u8}.slice(),
        ""u8,
        new query[]{
            new("http://www.google.com"u8, ""u8)
        }.slice()
    ),
    new(
        "ValidSubdomainTest #1."u8,
        "http://a.b.c.d.com"u8,
        new @string[]{
            "a=1; domain=.a.b.c.d.com"u8,
            "b=2; domain=.b.c.d.com"u8,
            "c=3; domain=.c.d.com"u8,
            "d=4; domain=.d.com"u8}.slice(),
        "a=1 b=2 c=3 d=4"u8,
        new query[]{
            new("http://a.b.c.d.com"u8, "a=1 b=2 c=3 d=4"u8),
            new("http://b.c.d.com"u8, "b=2 c=3 d=4"u8),
            new("http://c.d.com"u8, "c=3 d=4"u8),
            new("http://d.com"u8, "d=4"u8)
        }.slice()
    ),
    new(
        "ValidSubdomainTest #2."u8,
        "http://a.b.c.d.com"u8,
        new @string[]{
            "a=1; domain=.a.b.c.d.com"u8,
            "b=2; domain=.b.c.d.com"u8,
            "c=3; domain=.c.d.com"u8,
            "d=4; domain=.d.com"u8,
            "X=bcd; domain=.b.c.d.com"u8,
            "X=cd; domain=.c.d.com"u8}.slice(),
        "X=bcd X=cd a=1 b=2 c=3 d=4"u8,
        new query[]{
            new("http://b.c.d.com"u8, "b=2 c=3 d=4 X=bcd X=cd"u8),
            new("http://c.d.com"u8, "c=3 d=4 X=cd"u8)
        }.slice()
    ),
    new(
        "InvalidDomainTest #1."u8,
        "http://foo.bar.com"u8,
        new @string[]{
            "a=1; domain=.yo.foo.bar.com"u8,
            "b=2; domain=.foo.com"u8,
            "c=3; domain=.bar.foo.com"u8,
            "d=4; domain=.foo.bar.com.net"u8,
            "e=5; domain=ar.com"u8,
            "f=6; domain=."u8,
            "g=7; domain=/"u8,
            "h=8; domain=http://foo.bar.com"u8,
            "i=9; domain=..foo.bar.com"u8,
            "j=10; domain=..bar.com"u8,
            "k=11; domain=.foo.bar.com?blah"u8,
            "l=12; domain=.foo.bar.com/blah"u8,
            "m=12; domain=.foo.bar.com:80"u8,
            "n=14; domain=.foo.bar.com:"u8,
            "o=15; domain=.foo.bar.com#sup"u8
        }.slice(),
        ""u8,
        new query[]{new("http://foo.bar.com"u8, ""u8)}.slice()
    ),
    new(
        "InvalidDomainTest #2."u8,
        "http://foo.com.com"u8,
        new @string[]{"a=1; domain=.foo.com.com.com"u8}.slice(),
        ""u8,
        new query[]{new("http://foo.bar.com"u8, ""u8)}.slice()
    ),
    new(
        "DomainWithoutLeadingDotTest #1."u8,
        "http://manage.hosted.filefront.com"u8,
        new @string[]{"a=1; domain=filefront.com"u8}.slice(),
        "a=1"u8,
        new query[]{new("http://www.filefront.com"u8, "a=1"u8)}.slice()
    ),
    new(
        "DomainWithoutLeadingDotTest #2."u8,
        "http://www.google.com"u8,
        new @string[]{"a=1; domain=www.google.com"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.google.com"u8, "a=1"u8),
            new("http://sub.www.google.com"u8, "a=1"u8),
            new("http://something-else.com"u8, ""u8)
        }.slice()
    ),
    new(
        "CaseInsensitiveDomainTest."u8,
        "http://www.google.com"u8,
        new @string[]{
            "a=1; domain=.GOOGLE.COM"u8,
            "b=2; domain=.www.gOOgLE.coM"u8}.slice(),
        "a=1 b=2"u8,
        new query[]{new("http://www.google.com"u8, "a=1 b=2"u8)}.slice()
    ),
    new(
        "TestIpAddress #1."u8,
        "http://1.2.3.4/foo"u8,
        new @string[]{"a=1; path=/"u8}.slice(),
        "a=1"u8,
        new query[]{new("http://1.2.3.4/foo"u8, "a=1"u8)}.slice()
    ),
    new(
        "TestIpAddress #2."u8,
        "http://1.2.3.4/foo"u8,
        new @string[]{
            "a=1; domain=.1.2.3.4"u8,
            "b=2; domain=.3.4"u8}.slice(),
        ""u8,
        new query[]{new("http://1.2.3.4/foo"u8, ""u8)}.slice()
    ),
    new(
        "TestIpAddress #3."u8,
        "http://1.2.3.4/foo"u8,
        new @string[]{"a=1; domain=1.2.3.3"u8}.slice(),
        ""u8,
        new query[]{new("http://1.2.3.4/foo"u8, ""u8)}.slice()
    ),
    new(
        "TestIpAddress #4."u8,
        "http://1.2.3.4/foo"u8,
        new @string[]{"a=1; domain=1.2.3.4"u8}.slice(),
        "a=1"u8,
        new query[]{new("http://1.2.3.4/foo"u8, "a=1"u8)}.slice()
    ),
    new(
        "TestNonDottedAndTLD #2."u8,
        "http://com./index.html"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://com./index.html"u8, "a=1"u8),
            new("http://no-cookies.com./index.html"u8, ""u8)
        }.slice()
    ),
    new(
        "TestNonDottedAndTLD #3."u8,
        "http://a.b"u8,
        new @string[]{
            "a=1; domain=.b"u8,
            "b=2; domain=b"u8}.slice(),
        ""u8,
        new query[]{new("http://bar.foo"u8, ""u8)}.slice()
    ),
    new(
        "TestNonDottedAndTLD #4."u8,
        "http://google.com"u8,
        new @string[]{
            "a=1; domain=.com"u8,
            "b=2; domain=com"u8}.slice(),
        ""u8,
        new query[]{new("http://google.com"u8, ""u8)}.slice()
    ),
    new(
        "TestNonDottedAndTLD #5."u8,
        "http://google.co.uk"u8,
        new @string[]{
            "a=1; domain=.co.uk"u8,
            "b=2; domain=.uk"u8}.slice(),
        ""u8,
        new query[]{
            new("http://google.co.uk"u8, ""u8),
            new("http://else.co.com"u8, ""u8),
            new("http://else.uk"u8, ""u8)
        }.slice()
    ),
    new(
        "TestHostEndsWithDot."u8,
        "http://www.google.com"u8,
        new @string[]{
            "a=1"u8,
            "b=2; domain=.www.google.com."u8}.slice(),
        "a=1"u8,
        new query[]{new("http://www.google.com"u8, "a=1"u8)}.slice()
    ),
    new(
        "PathTest"u8,
        "http://www.google.izzle"u8,
        new @string[]{"a=1; path=/wee"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.google.izzle/wee"u8, "a=1"u8),
            new("http://www.google.izzle/wee/"u8, "a=1"u8),
            new("http://www.google.izzle/wee/war"u8, "a=1"u8),
            new("http://www.google.izzle/wee/war/more/more"u8, "a=1"u8),
            new("http://www.google.izzle/weehee"u8, ""u8),
            new("http://www.google.izzle/"u8, ""u8)
        }.slice()
    )
}.array();

public static void TestChromiumBasics(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in chromiumBasicsTests.ΔRangeSnapshot()) {
        var jar = newTestJar();
        test.run(Ꮡt, jar);
    }
}

// chromiumDomainTests contains jarTests which must be executed all on the
// same Jar.
internal static array<jarTest> chromiumDomainTests = new jarTest[]{
    new(
        "Fill #1."u8,
        "http://www.google.izzle"u8,
        new @string[]{"A=B"u8}.slice(),
        "A=B"u8,
        new query[]{new("http://www.google.izzle"u8, "A=B"u8)}.slice()
    ),
    new(
        "Fill #2."u8,
        "http://www.google.izzle"u8,
        new @string[]{"C=D; domain=.google.izzle"u8}.slice(),
        "A=B C=D"u8,
        new query[]{new("http://www.google.izzle"u8, "A=B C=D"u8)}.slice()
    ),
    new(
        "Verify A is a host cookie and not accessible from subdomain."u8,
        "http://unused.nil"u8,
        new @string[]{}.slice(),
        "A=B C=D"u8,
        new query[]{new("http://foo.www.google.izzle"u8, "C=D"u8)}.slice()
    ),
    new(
        "Verify domain cookies are found on proper domain."u8,
        "http://www.google.izzle"u8,
        new @string[]{"E=F; domain=.www.google.izzle"u8}.slice(),
        "A=B C=D E=F"u8,
        new query[]{new("http://www.google.izzle"u8, "A=B C=D E=F"u8)}.slice()
    ),
    new(
        "Leading dots in domain attributes are optional."u8,
        "http://www.google.izzle"u8,
        new @string[]{"G=H; domain=www.google.izzle"u8}.slice(),
        "A=B C=D E=F G=H"u8,
        new query[]{new("http://www.google.izzle"u8, "A=B C=D E=F G=H"u8)}.slice()
    ),
    new(
        "Verify domain enforcement works #1."u8,
        "http://www.google.izzle"u8,
        new @string[]{"K=L; domain=.bar.www.google.izzle"u8}.slice(),
        "A=B C=D E=F G=H"u8,
        new query[]{new("http://bar.www.google.izzle"u8, "C=D E=F G=H"u8)}.slice()
    ),
    new(
        "Verify domain enforcement works #2."u8,
        "http://unused.nil"u8,
        new @string[]{}.slice(),
        "A=B C=D E=F G=H"u8,
        new query[]{new("http://www.google.izzle"u8, "A=B C=D E=F G=H"u8)}.slice()
    )
}.array();

public static void TestChromiumDomain(ж<testing.T> Ꮡt) {
    var jar = newTestJar();
    foreach (var (_, test) in chromiumDomainTests.ΔRangeSnapshot()) {
        test.run(Ꮡt, jar);
    }
}

// chromiumDeletionTests must be performed all on the same Jar.
internal static array<jarTest> chromiumDeletionTests = new jarTest[]{
    new(
        "Create session cookie a1."u8,
        "http://www.google.com"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{new("http://www.google.com"u8, "a=1"u8)}.slice()
    ),
    new(
        "Delete sc a1 via MaxAge."u8,
        "http://www.google.com"u8,
        new @string[]{"a=1; max-age=-1"u8}.slice(),
        ""u8,
        new query[]{new("http://www.google.com"u8, ""u8)}.slice()
    ),
    new(
        "Create session cookie b2."u8,
        "http://www.google.com"u8,
        new @string[]{"b=2"u8}.slice(),
        "b=2"u8,
        new query[]{new("http://www.google.com"u8, "b=2"u8)}.slice()
    ),
    new(
        "Delete sc b2 via Expires."u8,
        "http://www.google.com"u8,
        new @string[]{"b=2; "u8 + expiresIn(-10)}.slice(),
        ""u8,
        new query[]{new("http://www.google.com"u8, ""u8)}.slice()
    ),
    new(
        "Create persistent cookie c3."u8,
        "http://www.google.com"u8,
        new @string[]{"c=3; max-age=3600"u8}.slice(),
        "c=3"u8,
        new query[]{new("http://www.google.com"u8, "c=3"u8)}.slice()
    ),
    new(
        "Delete pc c3 via MaxAge."u8,
        "http://www.google.com"u8,
        new @string[]{"c=3; max-age=-1"u8}.slice(),
        ""u8,
        new query[]{new("http://www.google.com"u8, ""u8)}.slice()
    ),
    new(
        "Create persistent cookie d4."u8,
        "http://www.google.com"u8,
        new @string[]{"d=4; max-age=3600"u8}.slice(),
        "d=4"u8,
        new query[]{new("http://www.google.com"u8, "d=4"u8)}.slice()
    ),
    new(
        "Delete pc d4 via Expires."u8,
        "http://www.google.com"u8,
        new @string[]{"d=4; "u8 + expiresIn(-10)}.slice(),
        ""u8,
        new query[]{new("http://www.google.com"u8, ""u8)}.slice()
    )
}.array();

public static void TestChromiumDeletion(ж<testing.T> Ꮡt) {
    var jar = newTestJar();
    foreach (var (_, test) in chromiumDeletionTests.ΔRangeSnapshot()) {
        test.run(Ꮡt, jar);
    }
}

// domainHandlingTests tests and documents the rules for domain handling.
// Each test must be performed on an empty new Jar.
internal static array<jarTest> domainHandlingTests = new jarTest[]{
    new(
        "Host cookie"u8,
        "http://www.host.test"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.host.test"u8, "a=1"u8),
            new("http://host.test"u8, ""u8),
            new("http://bar.host.test"u8, ""u8),
            new("http://foo.www.host.test"u8, ""u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Domain cookie #1"u8,
        "http://www.host.test"u8,
        new @string[]{"a=1; domain=host.test"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.host.test"u8, "a=1"u8),
            new("http://host.test"u8, "a=1"u8),
            new("http://bar.host.test"u8, "a=1"u8),
            new("http://foo.www.host.test"u8, "a=1"u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Domain cookie #2"u8,
        "http://www.host.test"u8,
        new @string[]{"a=1; domain=.host.test"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.host.test"u8, "a=1"u8),
            new("http://host.test"u8, "a=1"u8),
            new("http://bar.host.test"u8, "a=1"u8),
            new("http://foo.www.host.test"u8, "a=1"u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Host cookie on IDNA domain #1"u8,
        "http://www.bücher.test"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.bücher.test"u8, "a=1"u8),
            new("http://www.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://bücher.test"u8, ""u8),
            new("http://xn--bcher-kva.test"u8, ""u8),
            new("http://bar.bücher.test"u8, ""u8),
            new("http://bar.xn--bcher-kva.test"u8, ""u8),
            new("http://foo.www.bücher.test"u8, ""u8),
            new("http://foo.www.xn--bcher-kva.test"u8, ""u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Host cookie on IDNA domain #2"u8,
        "http://www.xn--bcher-kva.test"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.bücher.test"u8, "a=1"u8),
            new("http://www.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://bücher.test"u8, ""u8),
            new("http://xn--bcher-kva.test"u8, ""u8),
            new("http://bar.bücher.test"u8, ""u8),
            new("http://bar.xn--bcher-kva.test"u8, ""u8),
            new("http://foo.www.bücher.test"u8, ""u8),
            new("http://foo.www.xn--bcher-kva.test"u8, ""u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Domain cookie on IDNA domain #1"u8,
        "http://www.bücher.test"u8,
        new @string[]{"a=1; domain=xn--bcher-kva.test"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.bücher.test"u8, "a=1"u8),
            new("http://www.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://bücher.test"u8, "a=1"u8),
            new("http://xn--bcher-kva.test"u8, "a=1"u8),
            new("http://bar.bücher.test"u8, "a=1"u8),
            new("http://bar.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://foo.www.bücher.test"u8, "a=1"u8),
            new("http://foo.www.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Domain cookie on IDNA domain #2"u8,
        "http://www.xn--bcher-kva.test"u8,
        new @string[]{"a=1; domain=xn--bcher-kva.test"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://www.bücher.test"u8, "a=1"u8),
            new("http://www.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://bücher.test"u8, "a=1"u8),
            new("http://xn--bcher-kva.test"u8, "a=1"u8),
            new("http://bar.bücher.test"u8, "a=1"u8),
            new("http://bar.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://foo.www.bücher.test"u8, "a=1"u8),
            new("http://foo.www.xn--bcher-kva.test"u8, "a=1"u8),
            new("http://other.test"u8, ""u8),
            new("http://test"u8, ""u8)
        }.slice()
    ),
    new(
        "Host cookie on TLD."u8,
        "http://com"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://com"u8, "a=1"u8),
            new("http://any.com"u8, ""u8),
            new("http://any.test"u8, ""u8)
        }.slice()
    ),
    new(
        "Domain cookie on TLD becomes a host cookie."u8,
        "http://com"u8,
        new @string[]{"a=1; domain=com"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://com"u8, "a=1"u8),
            new("http://any.com"u8, ""u8),
            new("http://any.test"u8, ""u8)
        }.slice()
    ),
    new(
        "Host cookie on public suffix."u8,
        "http://co.uk"u8,
        new @string[]{"a=1"u8}.slice(),
        "a=1"u8,
        new query[]{
            new("http://co.uk"u8, "a=1"u8),
            new("http://uk"u8, ""u8),
            new("http://some.co.uk"u8, ""u8),
            new("http://foo.some.co.uk"u8, ""u8),
            new("http://any.uk"u8, ""u8)
        }.slice()
    ),
    new(
        "Domain cookie on public suffix is ignored."u8,
        "http://some.co.uk"u8,
        new @string[]{"a=1; domain=co.uk"u8}.slice(),
        ""u8,
        new query[]{
            new("http://co.uk"u8, ""u8),
            new("http://uk"u8, ""u8),
            new("http://some.co.uk"u8, ""u8),
            new("http://foo.some.co.uk"u8, ""u8),
            new("http://any.uk"u8, ""u8)
        }.slice()
    )
}.array();

public static void TestDomainHandling(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in domainHandlingTests.ΔRangeSnapshot()) {
        var jar = newTestJar();
        test.run(Ꮡt, jar);
    }
}

public static void TestIssue19384(ж<testing.T> Ꮡt) {
    var cookies = new ж<httpꓸCookie>[]{Ꮡ(new httpꓸCookie(Name: "name"u8, Value: "value"u8))}.slice();
    foreach (var (_, vᴛ1) in new @string[]{""u8, "."u8, ".."u8, "..."u8}.slice()) {
        ref var host = ref heap(new @string(), out var Ꮡhost);
        host = vᴛ1;

        var (jar, _) = New(nil);
        var u = Ꮡ(new url.URL(Scheme: "http"u8, Host: host, Path: "/"u8));
        {
            var got = jar.Cookies(u); if (len(got) != 0) {
                Ꮡt.Errorf("host %q, got %v"u8, host, got);
            }
        }
        jar.SetCookies(u, cookies);
        {
            var got = jar.Cookies(u); if (len(got) != 1 || (~got[0]).Value != "value"u8) {
                Ꮡt.Errorf("host %q, got %v"u8, host, got);
            }
        }
    }
}

} // end cookiejar_internal_test_package
