// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using log = log_package;
using os = os_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using encoding;
using io = io_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

// According to IETF 6265 Section 5.1.1.5, the year cannot be less than 1601
// The "special" cookies have values containing commas or spaces which
// are disallowed by RFC 6265 but are common in the wild.
// Quoted values (issue #46443)

[GoType("dyn")] partial struct writeSetCookiesTestsᴛ1 {
    public ж<global::go.net.http_package.ΔCookie> Cookie;
    public @string Raw;
}
internal static slice<writeSetCookiesTestsᴛ1> writeSetCookiesTests = new writeSetCookiesTestsᴛ1[]{
    new(
        Ꮡ(new ΔCookie(Name: "cookie-1"u8, Value: "v$1"u8)),
        "cookie-1=v$1"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-2"u8, Value: "two"u8, MaxAge: 3600)),
        "cookie-2=two; Max-Age=3600"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-3"u8, Value: "three"u8, Domain: ".example.com"u8)),
        "cookie-3=three; Domain=example.com"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-4"u8, Value: "four"u8, Path: "/restricted/"u8)),
        "cookie-4=four; Path=/restricted/"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-5"u8, Value: "five"u8, Domain: "wrong;bad.abc"u8)),
        "cookie-5=five"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-6"u8, Value: "six"u8, Domain: "bad-.abc"u8)),
        "cookie-6=six"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-7"u8, Value: "seven"u8, Domain: "127.0.0.1"u8)),
        "cookie-7=seven; Domain=127.0.0.1"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-8"u8, Value: "eight"u8, Domain: "::1"u8)),
        "cookie-8=eight"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-9"u8, Value: "expiring"u8, Expires: time.Unix(1257894000, 0))),
        "cookie-9=expiring; Expires=Tue, 10 Nov 2009 23:00:00 GMT"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-10"u8, Value: "expiring-1601"u8, Expires: time.Date(1601, 1, 1, 1, 1, 1, 1, time.ΔUTC))),
        "cookie-10=expiring-1601; Expires=Mon, 01 Jan 1601 01:01:01 GMT"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-11"u8, Value: "invalid-expiry"u8, Expires: time.Date(1600, 1, 1, 1, 1, 1, 1, time.ΔUTC))),
        "cookie-11=invalid-expiry"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-12"u8, Value: "samesite-default"u8, SameSite: SameSiteDefaultMode)),
        "cookie-12=samesite-default"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-13"u8, Value: "samesite-lax"u8, SameSite: SameSiteLaxMode)),
        "cookie-13=samesite-lax; SameSite=Lax"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-14"u8, Value: "samesite-strict"u8, SameSite: SameSiteStrictMode)),
        "cookie-14=samesite-strict; SameSite=Strict"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-15"u8, Value: "samesite-none"u8, SameSite: SameSiteNoneMode)),
        "cookie-15=samesite-none; SameSite=None"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-16"u8, Value: "partitioned"u8, SameSite: SameSiteNoneMode, Secure: true, Path: "/"u8, Partitioned: true)),
        "cookie-16=partitioned; Path=/; Secure; SameSite=None; Partitioned"u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-1"u8, Value: "a z"u8)),
        @"special-1=""a z"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-2"u8, Value: " z"u8)),
        @"special-2="" z"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-3"u8, Value: "a "u8)),
        @"special-3=""a """u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-4"u8, Value: " "u8)),
        @"special-4="" """u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-5"u8, Value: "a,z"u8)),
        @"special-5=""a,z"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-6"u8, Value: ",z"u8)),
        @"special-6="",z"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-7"u8, Value: "a,"u8)),
        @"special-7=""a,"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-8"u8, Value: ","u8)),
        @"special-8="","""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "empty-value"u8, Value: ""u8)),
        @"empty-value="u8
    ),
    new(
        nil,
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: ""u8)),
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "\t"u8)),
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "\r"u8)),
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "a\nb"u8, Value: "v"u8)),
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "a\nb"u8, Value: "v"u8)),
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "a\rb"u8, Value: "v"u8)),
        @""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie"u8, Value: "quoted"u8, Quoted: true)),
        @"cookie=""quoted"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie"u8, Value: "quoted with spaces"u8, Quoted: true)),
        @"cookie=""quoted with spaces"""u8
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie"u8, Value: "quoted,with,commas"u8, Quoted: true)),
        @"cookie=""quoted,with,commas"""u8
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string droppingDomainAttributeˢ = "dropping domain attribute"u8;

public static void TestWriteSetCookies(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf));
        foreach (var (i, tt) in writeSetCookiesTests) {
            {
                @string g = tt.Cookie.String();
                @string e = tt.Raw; if (g != e) {
                    Ꮡt.Errorf("Test %d, expecting:\n%s\nGot:\n%s\n"u8, i, e, g);
                }
            }
        }
        {
            @string got = logbuf.String();
            @string sub = droppingDomainAttributeˢ; if (!strings.Contains(got, sub)) {
                Ꮡt.Errorf("Expected substring %q in log output. Got:\n%s"u8, sub, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("global::go.net.http_package.ΔHeader")] internal partial struct headerOnlyResponseWriter;

internal static global::go.net.http_package.ΔHeader Header(this headerOnlyResponseWriter ho) {
    return ((global::go.net.http_package.ΔHeader)ho);
}

internal static (nint, error) Write(this headerOnlyResponseWriter ho, slice<byte> _) {
    throw panic("NOIMPL");
}

internal static void WriteHeader(this headerOnlyResponseWriter ho, nint _) {
    throw panic("NOIMPL");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cookie1OnePathRestrictedˢ = "cookie-1=one; Path=/restricted/"u8;
internal static readonly @string cookie2TwoMaxAge3600ˢ = "cookie-2=two; Max-Age=3600"u8;

public static void TestSetCookie(ж<testing.T> Ꮡt) {
    var m = new global::go.net.http_package.ΔHeader(0);
    SetCookie(((headerOnlyResponseWriter)m), Ꮡ(new ΔCookie(Name: "cookie-1"u8, Value: "one"u8, Path: "/restricted/"u8)));
    SetCookie(((headerOnlyResponseWriter)m), Ꮡ(new ΔCookie(Name: "cookie-2"u8, Value: "two"u8, MaxAge: 3600)));
    {
        nint l = builtin.len(m[setCookieˢ]); if (l != 2) {
            Ꮡt.Fatalf("expected %d cookies, got %d"u8, (nint)(2), l);
        }
    }
    {
        @string g = m[setCookieˢ][0];
        @string e = cookie1OnePathRestrictedˢ; if (g != e) {
            Ꮡt.Errorf("cookie #1: want %q, got %q"u8, e, g);
        }
    }
    {
        @string g = m[setCookieˢ][1];
        @string e = cookie2TwoMaxAge3600ˢ; if (g != e) {
            Ꮡt.Errorf("cookie #2: want %q, got %q"u8, e, g);
        }
    }
}

// Quoted values (issue #46443)

[GoType("dyn")] partial struct addCookieTestsᴛ1 {
    public slice<ж<global::go.net.http_package.ΔCookie>> Cookies;
    public @string Raw;
}
internal static slice<addCookieTestsᴛ1> addCookieTests = new addCookieTestsᴛ1[]{
    new(
        new ж<global::go.net.http_package.ΔCookie>[]{}.slice(),
        ""u8
    ),
    new(
        new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-1"u8, Value: "v$1"u8))}.slice(),
        "cookie-1=v$1"u8
    ),
    new(
        new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-1"u8, Value: "v$1"u8)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-2"u8, Value: "v$2"u8)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-3"u8, Value: "v$3"u8))
        }.slice(),
        "cookie-1=v$1; cookie-2=v$2; cookie-3=v$3"u8
    ),
    new(
        new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-1"u8, Value: "quoted"u8, Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-2"u8, Value: "quoted with spaces"u8, Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-3"u8, Value: "quoted,with,commas"u8, Quoted: true))
        }.slice(),
        @"cookie-1=""quoted""; cookie-2=""quoted with spaces""; cookie-3=""quoted,with,commas"""u8
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComˢ = "http://example.com/"u8;

public static void TestAddCookie(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in addCookieTests) {
        var (req, _) = NewRequest(getˢ, httpExampleComˢ, default!);
        foreach (var (_, c) in tt.Cookies) {
            req.AddCookie(c);
        }
        {
            @string g = (~req).Header.Get(cookieˢ); if (g != tt.Raw) {
                Ꮡt.Errorf("Test %d:\nwant: %s\n got: %s\n"u8, i, tt.Raw, g);
            }
        }
    }
}

// Make sure we can properly read back the Set-Cookie headers we create
// for values containing spaces or commas:
// Make sure we can properly read back the Set-Cookie headers
// for names containing spaces:
// Quoted values (issue #46443)
// TODO(bradfitz): users have reported seeing this in the
// wild, but do browsers handle it? RFC 6265 just says "don't
// do that" (section 3) and then never mentions header folding
// again.
// Header{"Set-Cookie": {"ASP.NET_SessionId=foo; path=/; HttpOnly, .ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"}},

[GoType("dyn")] partial struct readSetCookiesTestsᴛ1 {
    internal global::go.net.http_package.ΔHeader header;
    internal slice<ж<global::go.net.http_package.ΔCookie>> cookies;
    internal @string godebug;
}
internal static slice<readSetCookiesTestsᴛ1> readSetCookiesTests = new readSetCookiesTestsᴛ1[]{
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"Cookie-1=v$1"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8, Raw: "Cookie-1=v$1"u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "NID"u8,
            Value: "99=YsDT5i3E-CXax-"u8,
            Path: "/"u8,
            Domain: ".google.ch"u8,
            HttpOnly: true,
            Expires: time.Date(2011, 11, 23, 1, 5, 3, 0, time.ΔUTC),
            RawExpires: "Wed, 23-Nov-2011 01:05:03 GMT"u8,
            Raw: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: ".ASPXAUTH"u8,
            Value: "7E3AA"u8,
            Path: "/"u8,
            Expires: time.Date(2012, 3, 7, 14, 25, 6, 0, time.ΔUTC),
            RawExpires: "Wed, 07-Mar-2012 14:25:06 GMT"u8,
            HttpOnly: true,
            Raw: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"ASP.NET_SessionId=foo; path=/; HttpOnly"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "ASP.NET_SessionId"u8,
            Value: "foo"u8,
            Path: "/"u8,
            HttpOnly: true,
            Raw: "ASP.NET_SessionId=foo; path=/; HttpOnly"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"samesitedefault=foo; SameSite"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitedefault"u8,
            Value: "foo"u8,
            SameSite: SameSiteDefaultMode,
            Raw: "samesitedefault=foo; SameSite"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"samesiteinvalidisdefault=foo; SameSite=invalid"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesiteinvalidisdefault"u8,
            Value: "foo"u8,
            SameSite: SameSiteDefaultMode,
            Raw: "samesiteinvalidisdefault=foo; SameSite=invalid"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"samesitelax=foo; SameSite=Lax"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitelax"u8,
            Value: "foo"u8,
            SameSite: SameSiteLaxMode,
            Raw: "samesitelax=foo; SameSite=Lax"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"samesitestrict=foo; SameSite=Strict"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitestrict"u8,
            Value: "foo"u8,
            SameSite: SameSiteStrictMode,
            Raw: "samesitestrict=foo; SameSite=Strict"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{"samesitenone=foo; SameSite=None"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitenone"u8,
            Value: "foo"u8,
            SameSite: SameSiteNoneMode,
            Raw: "samesitenone=foo; SameSite=None"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-1=a z"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-1"u8, Value: "a z"u8, Raw: @"special-1=a z"u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-2="" z"""u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-2"u8, Value: " z"u8, Quoted: true, Raw: @"special-2="" z"""u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-3=""a """u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-3"u8, Value: "a "u8, Quoted: true, Raw: @"special-3=""a """u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-4="" """u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-4"u8, Value: " "u8, Quoted: true, Raw: @"special-4="" """u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-5=a,z"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-5"u8, Value: "a,z"u8, Raw: @"special-5=a,z"u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-6="",z"""u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-6"u8, Value: ",z"u8, Quoted: true, Raw: @"special-6="",z"""u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-7=a,"u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-7"u8, Value: "a,"u8, Raw: @"special-7=a,"u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-8="","""u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-8"u8, Value: ","u8, Quoted: true, Raw: @"special-8="","""u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"special-9 ="","""u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-9"u8, Value: ","u8, Quoted: true, Raw: @"special-9 ="","""u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = new @string[]{@"cookie=""quoted"""u8}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie"u8, Value: "quoted"u8, Quoted: true, Raw: @"cookie=""quoted"""u8))}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = slices.Repeat<slice<@string>, @string>(new @string[]{"a="u8}.slice(), defaultCookieMaxNum + 1)}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = slices.Repeat<slice<@string>, @string>(new @string[]{"a="u8}.slice(), 10)}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{}.slice(),
        godebug: "httpcookiemaxnum=5"u8
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = strings.Split(strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..], ";"u8)}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a"u8, Value: ""u8, Quoted: false, Raw: "a="u8))}.slice(), defaultCookieMaxNum + 1),
        godebug: "httpcookiemaxnum=0"u8
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"u8] = strings.Split(strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..], ";"u8)}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a"u8, Value: ""u8, Quoted: false, Raw: "a="u8))}.slice(), defaultCookieMaxNum + 1),
        godebug: fmt.Sprintf("httpcookiemaxnum=%v"u8, (nint)(defaultCookieMaxNum + 1))
    )
}.slice();

internal static @string toJSON(any v) {
    var (b, err) = json.Marshal(v);
    if (err != default!) {
        return fmt.Sprintf("%#v"u8, v);
    }
    return ((@string)b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;

public static void TestReadSetCookies(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, tt) in readSetCookiesTests) {
        Ꮡt.Setenv(godebugˢ, tt.godebug);
        for (nint n = 0; n < 2; n++) {
            // to verify readSetCookies doesn't mutate its input
            var c = readSetCookies(tt.header);
            if (!reflect.DeepEqual(c, tt.cookies)) {
                Ꮡt.Errorf("#%d readSetCookies: have\n%s\nwant\n%s\n"u8, i, toJSON(c), toJSON(tt.cookies));
            }
        }
    }
}

// GODEBUG=httpcookiemaxnum should work regardless if all cookies are sent
// via one "Cookie" field, or multiple fields.

[GoType("dyn")] partial struct readCookiesTestsᴛ1 {
    internal global::go.net.http_package.ΔHeader header;
    internal @string filter;
    internal slice<ж<global::go.net.http_package.ΔCookie>> cookies;
    internal @string godebug;
}
internal static slice<readCookiesTestsᴛ1> readCookiesTests = new readCookiesTestsᴛ1[]{
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{"Cookie-1=v$1"u8, "c2=v2"u8}.slice()}),
        filter: ""u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{"Cookie-1=v$1"u8, "c2=v2"u8}.slice()}),
        filter: "c2"u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{"Cookie-1=v$1; c2=v2"u8}.slice()}),
        filter: ""u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{"Cookie-1=v$1; c2=v2"u8}.slice()}),
        filter: "c2"u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{@"Cookie-1=""v$1""; c2=""v2"""u8}.slice()}),
        filter: ""u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8, Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8, Quoted: true))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{@"Cookie-1=""v$1""; c2=v2;"u8}.slice()}),
        filter: ""u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8, Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8))
        }.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{@""u8}.slice()}),
        filter: ""u8,
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..]}.slice()}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{}.slice()
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = slices.Repeat<slice<@string>, @string>(new @string[]{"a="u8}.slice(), 10)}),
        cookies: new ж<global::go.net.http_package.ΔCookie>[]{}.slice(),
        godebug: "httpcookiemaxnum=5"u8
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = new @string[]{strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..]}.slice()}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a"u8, Value: ""u8, Quoted: false))}.slice(), defaultCookieMaxNum + 1),
        godebug: "httpcookiemaxnum=0"u8
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"u8] = slices.Repeat<slice<@string>, @string>(new @string[]{"a="u8}.slice(), defaultCookieMaxNum + 1)}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a"u8, Value: ""u8, Quoted: false))}.slice(), defaultCookieMaxNum + 1),
        godebug: fmt.Sprintf("httpcookiemaxnum=%v"u8, (nint)(defaultCookieMaxNum + 1))
    )
}.slice();

public static void TestReadCookies(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, tt) in readCookiesTests) {
        Ꮡt.Setenv(godebugˢ, tt.godebug);
        for (nint n = 0; n < 2; n++) {
            // to verify readCookies doesn't mutate its input
            var c = readCookies(tt.header, tt.filter);
            if (!reflect.DeepEqual(c, tt.cookies)) {
                Ꮡt.Errorf("#%d readCookies:\nhave: %s\nwant: %s\n"u8, i, toJSON(c), toJSON(tt.cookies));
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string quoted0NoneMaxAge30ˢ = @"quoted0=none; max-age=30"u8;
internal static readonly @string quoted1CookieValueMaxAgeˢ = @"quoted1=""cookieValue""; max-age=31"u8;
internal static readonly @string quoted2CookieAVMaxAge32ˢ = @"quoted2=cookieAV; max-age=""32"""u8;
internal static readonly @string quoted3BothMaxAge33ˢ = @"quoted3=""both""; max-age=""33"""u8;

public static void TestSetCookieDoubleQuotes(ж<testing.T> Ꮡt) {
    var res = Ꮡ(new Response(Header: new ΔHeader(new map<@string, slice<@string>>{})));
    (~res).Header.Add(setCookieˢ, quoted0NoneMaxAge30ˢ);
    (~res).Header.Add(setCookieˢ, quoted1CookieValueMaxAgeˢ);
    (~res).Header.Add(setCookieˢ, quoted2CookieAVMaxAge32ˢ);
    (~res).Header.Add(setCookieˢ, quoted3BothMaxAge33ˢ);
    var got = res.Cookies();
    var want = new ж<global::go.net.http_package.ΔCookie>[]{
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted0"u8, Value: "none"u8, MaxAge: 30)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted1"u8, Value: "cookieValue"u8, MaxAge: 31)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted2"u8, Value: "cookieAV"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted3"u8, Value: "both"u8))
    }.slice();
    if (builtin.len(got) != builtin.len(want)) {
        Ꮡt.Fatalf("got %d cookies, want %d"u8, builtin.len(got), builtin.len(want));
    }
    foreach (var (i, w) in want) {
        var g = got[i];
        if ((~g).Name != (~w).Name || (~g).Value != (~w).Value || (~g).MaxAge != (~w).MaxAge) {
            Ꮡt.Errorf("cookie #%d:\ngot  %v\nwant %v"u8, i, g.OrTypedNil(), w.OrTypedNil());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string droppingInvalidBytesˢ = "dropping invalid bytes"u8;

[GoType("dyn")] internal partial struct TestCookieSanitizeValue_tests {
    internal @string @in;
    internal bool quoted;
    internal @string want;
}

public static void TestCookieSanitizeValue(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf));
        var tests = new TestCookieSanitizeValue_tests[]{
            new("foo"u8, false, "foo"u8),
            new("foo;bar"u8, false, "foobar"u8),
            new("foo\\bar"u8, false, "foobar"u8),
            new("foo\"bar"u8, false, "foobar"u8),
            new(((@string)(new byte[]{0x00, 0x7e, 0x7f, 0x80})), false, "\x7e"u8),
            new(@"withquotes"u8, true, @"""withquotes"""u8),
            new(@"""withquotes"""u8, true, @"""withquotes"""u8), // double quotes are not valid octets

            new("a z"u8, false, @"""a z"""u8),
            new(" z"u8, false, @""" z"""u8),
            new("a "u8, false, @"""a """u8),
            new("a,z"u8, false, @"""a,z"""u8),
            new(",z"u8, false, @""",z"""u8),
            new("a,"u8, false, @"""a,"""u8)
        }.slice();
        foreach (var (_, tt) in tests) {
            {
                @string got = sanitizeCookieValue(tt.@in, tt.quoted); if (got != tt.want) {
                    Ꮡt.Errorf("sanitizeCookieValue(%q) = %q; want %q"u8, tt.@in, got, tt.want);
                }
            }
        }
        {
            @string got = logbuf.String();
            @string sub = droppingInvalidBytesˢ; if (!strings.Contains(got, sub)) {
                Ꮡt.Errorf("Expected substring %q in log output. Got:\n%s"u8, sub, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestCookieSanitizePath_tests {
    internal @string @in, want;
}

public static void TestCookieSanitizePath(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf));
        var tests = new TestCookieSanitizePath_tests[]{
            new("/path"u8, "/path"u8),
            new("/path with space/"u8, "/path with space/"u8),
            new("/just;no;semicolon\x00orstuff/"u8, "/justnosemicolonorstuff/"u8)
        }.slice();
        foreach (var (_, tt) in tests) {
            {
                @string got = sanitizeCookiePath(tt.@in); if (got != tt.want) {
                    Ꮡt.Errorf("sanitizeCookiePath(%q) = %q; want %q"u8, tt.@in, got, tt.want);
                }
            }
        }
        {
            @string got = logbuf.String();
            @string sub = droppingInvalidBytesˢ; if (!strings.Contains(got, sub)) {
                Ꮡt.Errorf("Expected substring %q in log output. Got:\n%s"u8, sub, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestCookieValid_tests {
    internal ж<global::go.net.http_package.ΔCookie> cookie;
    internal bool valid;
}

public static void TestCookieValid(ж<testing.T> Ꮡt) {
    var tests = new TestCookieValid_tests[]{
        new(nil, false),
        new(Ꮡ(new ΔCookie(Name: ""u8)), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-value"u8, Value: "foo\"bar"u8)), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-path"u8, Path: "/foo;bar/"u8)), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-secure-for-partitioned"u8, Value: "foo"u8, Path: "/"u8, Secure: false, Partitioned: true)), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-domain"u8, Domain: "example.com:80"u8)), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-expiry"u8, Value: ""u8, Expires: time.Date(1600, 1, 1, 1, 1, 1, 1, time.ΔUTC))), false),
        new(Ꮡ(new ΔCookie(Name: "valid-empty"u8)), true),
        new(Ꮡ(new ΔCookie(Name: "valid-expires"u8, Value: "foo"u8, Path: "/bar"u8, Domain: "example.com"u8, Expires: time.Unix(0, 0))), true),
        new(Ꮡ(new ΔCookie(Name: "valid-max-age"u8, Value: "foo"u8, Path: "/bar"u8, Domain: "example.com"u8, MaxAge: 60)), true),
        new(Ꮡ(new ΔCookie(Name: "valid-all-fields"u8, Value: "foo"u8, Path: "/bar"u8, Domain: "example.com"u8, Expires: time.Unix(0, 0), MaxAge: 0)), true),
        new(Ꮡ(new ΔCookie(Name: "valid-partitioned"u8, Value: "foo"u8, Path: "/"u8, Secure: true, Partitioned: true)), true)
    }.slice();
    foreach (var (_, tt) in tests) {
        var err = tt.cookie.Valid();
        if (err != default! && tt.valid) {
            Ꮡt.Errorf("%#v.Valid() returned error %v; want nil"u8, tt.cookie.OrTypedNil(), err);
        }
        if (err == default! && !tt.valid) {
            Ꮡt.Errorf("%#v.Valid() returned nil; want error"u8, tt.cookie.OrTypedNil());
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantCookieStringᶜ = @"cookie-9=i3e01nf61b6t23bvfmplnanol3; Path=/restricted/; Domain=example.com; Expires=Tue, 10 Nov 2009 23:00:00 GMT; Max-Age=3600"u8;

public static void BenchmarkCookieString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string wantCookieString = wantCookieStringᶜ;
    var c = Ꮡ(new ΔCookie(
        Name: "cookie-9"u8,
        Value: "i3e01nf61b6t23bvfmplnanol3"u8,
        Expires: time.Unix(1257894000, 0),
        Path: "/restricted/"u8,
        Domain: ".example.com"u8,
        MaxAge: 3600
    ));
    @string benchmarkCookieString = default!;
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        benchmarkCookieString = c.String();
    }
    {
        @string have = benchmarkCookieString;
        @string want = wantCookieString; if (have != want) {
            Ꮡb.Fatalf("Have: %v Want: %v"u8, have, want);
        }
    }
}

public static void BenchmarkReadSetCookies(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var header = new ΔHeader(new map<@string, slice<@string>>{
        ["Set-Cookie"u8] = new @string[]{
            "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"u8,
            ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"u8}.slice()
    });
    var wantCookies = new ж<global::go.net.http_package.ΔCookie>[]{
        Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "NID"u8,
            Value: "99=YsDT5i3E-CXax-"u8,
            Path: "/"u8,
            Domain: ".google.ch"u8,
            HttpOnly: true,
            Expires: time.Date(2011, 11, 23, 1, 5, 3, 0, time.ΔUTC),
            RawExpires: "Wed, 23-Nov-2011 01:05:03 GMT"u8,
            Raw: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: ".ASPXAUTH"u8,
            Value: "7E3AA"u8,
            Path: "/"u8,
            Expires: time.Date(2012, 3, 7, 14, 25, 6, 0, time.ΔUTC),
            RawExpires: "Wed, 07-Mar-2012 14:25:06 GMT"u8,
            HttpOnly: true,
            Raw: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"u8))
    }.slice();
    slice<ж<global::go.net.http_package.ΔCookie>> c = default!;
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        c = readSetCookies(header);
    }
    if (!reflect.DeepEqual(c, wantCookies)) {
        Ꮡb.Fatalf("readSetCookies:\nhave: %s\nwant: %s\n"u8, toJSON(c), toJSON(wantCookies));
    }
}

public static void BenchmarkReadCookies(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var header = new ΔHeader(new map<@string, slice<@string>>{
        ["Cookie"u8] = new @string[]{
            @"de=; client_region=0; rpld1=0:hispeed.ch|20:che|21:zh|22:zurich|23:47.36|24:8.53|; rpld0=1:08|; backplane-channel=newspaper.com:1471; devicetype=0; osfam=0; rplmct=2; s_pers=%20s_vmonthnum%3D1472680800496%2526vn%253D1%7C1472680800496%3B%20s_nr%3D1471686767664-New%7C1474278767664%3B%20s_lv%3D1471686767669%7C1566294767669%3B%20s_lv_s%3DFirst%2520Visit%7C1471688567669%3B%20s_monthinvisit%3Dtrue%7C1471688567677%3B%20gvp_p5%3Dsports%253Ablog%253Aearly-lead%2520-%2520184693%2520-%252020160820%2520-%2520u-s%7C1471688567681%3B%20gvp_p51%3Dwp%2520-%2520sports%7C1471688567684%3B; s_sess=%20s_wp_ep%3Dhomepage%3B%20s._ref%3Dhttps%253A%252F%252Fwww.google.ch%252F%3B%20s_cc%3Dtrue%3B%20s_ppvl%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_ppv%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-s-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_dslv%3DFirst%2520Visit%3B%20s_sq%3Dwpninewspapercom%253D%252526pid%25253Dsports%2525253Ablog%2525253Aearly-lead%25252520-%25252520184693%25252520-%2525252020160820%25252520-%25252520u-s%252526pidt%25253D1%252526oid%25253Dhttps%2525253A%2525252F%2525252Fwww.newspaper.com%2525252F%2525253Fnid%2525253Dmenu_nav_homepage%252526ot%25253DA%3B"u8}.slice()
    });
    var wantCookies = new ж<global::go.net.http_package.ΔCookie>[]{
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "de"u8, Value: ""u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "client_region"u8, Value: "0"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "rpld1"u8, Value: "0:hispeed.ch|20:che|21:zh|22:zurich|23:47.36|24:8.53|"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "rpld0"u8, Value: "1:08|"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "backplane-channel"u8, Value: "newspaper.com:1471"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "devicetype"u8, Value: "0"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "osfam"u8, Value: "0"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "rplmct"u8, Value: "2"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "s_pers"u8, Value: "%20s_vmonthnum%3D1472680800496%2526vn%253D1%7C1472680800496%3B%20s_nr%3D1471686767664-New%7C1474278767664%3B%20s_lv%3D1471686767669%7C1566294767669%3B%20s_lv_s%3DFirst%2520Visit%7C1471688567669%3B%20s_monthinvisit%3Dtrue%7C1471688567677%3B%20gvp_p5%3Dsports%253Ablog%253Aearly-lead%2520-%2520184693%2520-%252020160820%2520-%2520u-s%7C1471688567681%3B%20gvp_p51%3Dwp%2520-%2520sports%7C1471688567684%3B"u8)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "s_sess"u8, Value: "%20s_wp_ep%3Dhomepage%3B%20s._ref%3Dhttps%253A%252F%252Fwww.google.ch%252F%3B%20s_cc%3Dtrue%3B%20s_ppvl%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_ppv%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-s-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_dslv%3DFirst%2520Visit%3B%20s_sq%3Dwpninewspapercom%253D%252526pid%25253Dsports%2525253Ablog%2525253Aearly-lead%25252520-%25252520184693%25252520-%2525252020160820%25252520-%25252520u-s%252526pidt%25253D1%252526oid%25253Dhttps%2525253A%2525252F%2525252Fwww.newspaper.com%2525252F%2525253Fnid%2525253Dmenu_nav_homepage%252526ot%25253DA%3B"u8))
    }.slice();
    slice<ж<global::go.net.http_package.ΔCookie>> c = default!;
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        c = readCookies(header, ""u8);
    }
    if (!reflect.DeepEqual(c, wantCookies)) {
        Ꮡb.Fatalf("readCookies:\nhave: %s\nwant: %s\n"u8, toJSON(c), toJSON(wantCookies));
    }
}

[GoType("dyn")] internal partial struct TestParseCookie_tests {
    internal @string line;
    internal slice<ж<global::go.net.http_package.ΔCookie>> cookies;
    internal error err;
    internal @string godebug;
}

public static void TestParseCookie(ж<testing.T> Ꮡt) {
    var tests = new TestParseCookie_tests[]{
        new(
            line: "Cookie-1=v$1"u8,
            cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8))}.slice()
        ),
        new(
            line: "Cookie-1=v$1;c2=v2"u8,
            cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8)), Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8))}.slice()
        ),
        new(
            line: @"Cookie-1=""v$1"";c2=""v2"""u8,
            cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8, Quoted: true)), Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2"u8, Value: "v2"u8, Quoted: true))}.slice()
        ),
        new(
            line: "k1="u8,
            cookies: new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "k1"u8, Value: ""u8))}.slice()
        ),
        new(
            line: ""u8,
            err: errBlankCookie
        ),
        new(
            line: "equal-not-found"u8,
            err: errEqualNotFoundInCookie
        ),
        new(
            line: "=v1"u8,
            err: errInvalidCookieName
        ),
        new(
            line: "k1=\\"u8,
            err: errInvalidCookieValue
        ),
        new(
            line: strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..],
            err: errCookieNumLimitExceeded
        ),
        new(
            line: strings.Repeat(";a="u8, 10)[1..],
            err: errCookieNumLimitExceeded,
            godebug: "httpcookiemaxnum=5"u8
        ),
        new(
            line: strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..],
            cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a"u8, Value: ""u8, Quoted: false))}.slice(), defaultCookieMaxNum + 1),
            godebug: "httpcookiemaxnum=0"u8
        ),
        new(
            line: strings.Repeat(";a="u8, defaultCookieMaxNum + 1)[1..],
            cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(new ж<global::go.net.http_package.ΔCookie>[]{Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a"u8, Value: ""u8, Quoted: false))}.slice(), defaultCookieMaxNum + 1),
            godebug: fmt.Sprintf("httpcookiemaxnum=%v"u8, (nint)(defaultCookieMaxNum + 1))
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        Ꮡt.Setenv(godebugˢ, tt.godebug);
        var (gotCookies, gotErr) = ParseCookie(tt.line);
        if (!errors.Is(gotErr, tt.err)) {
            Ꮡt.Errorf("#%d ParseCookie got error %v, want error %v"u8, i, gotErr, tt.err);
        }
        if (!reflect.DeepEqual(gotCookies, tt.cookies)) {
            Ꮡt.Errorf("#%d ParseCookie:\ngot cookies: %s\nwant cookies: %s\n"u8, i, toJSON(gotCookies), toJSON(tt.cookies));
        }
    }
}

[GoType("dyn")] internal partial struct TestParseSetCookie_tests {
    internal @string line;
    internal ж<global::go.net.http_package.ΔCookie> cookie;
    internal error err;
}

public static void TestParseSetCookie(ж<testing.T> Ꮡt) {
    var tests = new TestParseSetCookie_tests[]{
        new(
            line: "Cookie-1=v$1"u8,
            cookie: Ꮡ(new ΔCookie(Name: "Cookie-1"u8, Value: "v$1"u8, Raw: "Cookie-1=v$1"u8))
        ),
        new(
            line: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "NID"u8,
                Value: "99=YsDT5i3E-CXax-"u8,
                Path: "/"u8,
                Domain: ".google.ch"u8,
                HttpOnly: true,
                Expires: time.Date(2011, 11, 23, 1, 5, 3, 0, time.ΔUTC),
                RawExpires: "Wed, 23-Nov-2011 01:05:03 GMT"u8,
                Raw: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"u8
            ))
        ),
        new(
            line: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: ".ASPXAUTH"u8,
                Value: "7E3AA"u8,
                Path: "/"u8,
                Expires: time.Date(2012, 3, 7, 14, 25, 6, 0, time.ΔUTC),
                RawExpires: "Wed, 07-Mar-2012 14:25:06 GMT"u8,
                HttpOnly: true,
                Raw: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"u8
            ))
        ),
        new(
            line: "ASP.NET_SessionId=foo; path=/; HttpOnly"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "ASP.NET_SessionId"u8,
                Value: "foo"u8,
                Path: "/"u8,
                HttpOnly: true,
                Raw: "ASP.NET_SessionId=foo; path=/; HttpOnly"u8
            ))
        ),
        new(
            line: "samesitedefault=foo; SameSite"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitedefault"u8,
                Value: "foo"u8,
                SameSite: SameSiteDefaultMode,
                Raw: "samesitedefault=foo; SameSite"u8
            ))
        ),
        new(
            line: "samesiteinvalidisdefault=foo; SameSite=invalid"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "samesiteinvalidisdefault"u8,
                Value: "foo"u8,
                SameSite: SameSiteDefaultMode,
                Raw: "samesiteinvalidisdefault=foo; SameSite=invalid"u8
            ))
        ),
        new(
            line: "samesitelax=foo; SameSite=Lax"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitelax"u8,
                Value: "foo"u8,
                SameSite: SameSiteLaxMode,
                Raw: "samesitelax=foo; SameSite=Lax"u8
            ))
        ),
        new(
            line: "samesitestrict=foo; SameSite=Strict"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitestrict"u8,
                Value: "foo"u8,
                SameSite: SameSiteStrictMode,
                Raw: "samesitestrict=foo; SameSite=Strict"u8
            ))
        ),
        new(
            line: "samesitenone=foo; SameSite=None"u8,
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitenone"u8,
                Value: "foo"u8,
                SameSite: SameSiteNoneMode,
                Raw: "samesitenone=foo; SameSite=None"u8
            ))
        ), // Make sure we can properly read back the Set-Cookie headers we create
 // for values containing spaces or commas:

        new(
            line: @"special-1=a z"u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-1"u8, Value: "a z"u8, Raw: @"special-1=a z"u8))
        ),
        new(
            line: @"special-2="" z"""u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-2"u8, Value: " z"u8, Quoted: true, Raw: @"special-2="" z"""u8))
        ),
        new(
            line: @"special-3=""a """u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-3"u8, Value: "a "u8, Quoted: true, Raw: @"special-3=""a """u8))
        ),
        new(
            line: @"special-4="" """u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-4"u8, Value: " "u8, Quoted: true, Raw: @"special-4="" """u8))
        ),
        new(
            line: @"special-5=a,z"u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-5"u8, Value: "a,z"u8, Raw: @"special-5=a,z"u8))
        ),
        new(
            line: @"special-6="",z"""u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-6"u8, Value: ",z"u8, Quoted: true, Raw: @"special-6="",z"""u8))
        ),
        new(
            line: @"special-7=a,"u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-7"u8, Value: "a,"u8, Raw: @"special-7=a,"u8))
        ),
        new(
            line: @"special-8="","""u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-8"u8, Value: ","u8, Quoted: true, Raw: @"special-8="","""u8))
        ), // Make sure we can properly read back the Set-Cookie headers
 // for names containing spaces:

        new(
            line: @"special-9 ="","""u8,
            cookie: Ꮡ(new ΔCookie(Name: "special-9"u8, Value: ","u8, Quoted: true, Raw: @"special-9 ="","""u8))
        ),
        new(
            line: ""u8,
            err: errBlankCookie
        ),
        new(
            line: "equal-not-found"u8,
            err: errEqualNotFoundInCookie
        ),
        new(
            line: "=v1"u8,
            err: errInvalidCookieName
        ),
        new(
            line: "k1=\\"u8,
            err: errInvalidCookieValue
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        var (gotCookie, gotErr) = ParseSetCookie(tt.line);
        if (!errors.Is(gotErr, tt.err)) {
            Ꮡt.Errorf("#%d ParseSetCookie got error %v, want error %v"u8, i, gotErr, tt.err);
            continue;
        }
        if (!reflect.DeepEqual(gotCookie.OrTypedNil(), tt.cookie.OrTypedNil())) {
            Ꮡt.Errorf("#%d ParseSetCookie:\ngot cookie: %s\nwant cookie: %s\n"u8, i, toJSON(gotCookie.OrTypedNil()), toJSON(tt.cookie.OrTypedNil()));
        }
    }
}

} // end http_internal_test_package
