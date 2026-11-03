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

partial struct writeSetCookiesTestsᴛ1 /*dyn*/ {
    public ж<global::go.net.http_package.ΔCookie> Cookie;
    public @string Raw;
}
internal static slice<writeSetCookiesTestsᴛ1> writeSetCookiesTests = slice<writeSetCookiesTestsᴛ1>([
    new(
        Ꮡ(new ΔCookie(Name: "cookie-1", Value: "v$1")),
        "cookie-1=v$1"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-2", Value: "two", MaxAge: 3600)),
        "cookie-2=two; Max-Age=3600"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-3", Value: "three", Domain: ".example.com")),
        "cookie-3=three; Domain=example.com"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-4", Value: "four", Path: "/restricted/")),
        "cookie-4=four; Path=/restricted/"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-5", Value: "five", Domain: "wrong;bad.abc")),
        "cookie-5=five"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-6", Value: "six", Domain: "bad-.abc")),
        "cookie-6=six"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-7", Value: "seven", Domain: "127.0.0.1")),
        "cookie-7=seven; Domain=127.0.0.1"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-8", Value: "eight", Domain: "::1")),
        "cookie-8=eight"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-9", Value: "expiring", Expires: time.Unix(1257894000, 0))),
        "cookie-9=expiring; Expires=Tue, 10 Nov 2009 23:00:00 GMT"
    ),
    // According to IETF 6265 Section 5.1.1.5, the year cannot be less than 1601
    new(
        Ꮡ(new ΔCookie(Name: "cookie-10", Value: "expiring-1601", Expires: time.Date(1601, 1, 1, 1, 1, 1, 1, time.ΔUTC))),
        "cookie-10=expiring-1601; Expires=Mon, 01 Jan 1601 01:01:01 GMT"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-11", Value: "invalid-expiry", Expires: time.Date(1600, 1, 1, 1, 1, 1, 1, time.ΔUTC))),
        "cookie-11=invalid-expiry"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-12", Value: "samesite-default", SameSite: SameSiteDefaultMode)),
        "cookie-12=samesite-default"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-13", Value: "samesite-lax", SameSite: SameSiteLaxMode)),
        "cookie-13=samesite-lax; SameSite=Lax"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-14", Value: "samesite-strict", SameSite: SameSiteStrictMode)),
        "cookie-14=samesite-strict; SameSite=Strict"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-15", Value: "samesite-none", SameSite: SameSiteNoneMode)),
        "cookie-15=samesite-none; SameSite=None"
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie-16", Value: "partitioned", SameSite: SameSiteNoneMode, Secure: true, Path: "/", Partitioned: true)),
        "cookie-16=partitioned; Path=/; Secure; SameSite=None; Partitioned"
    ),
    // The "special" cookies have values containing commas or spaces which
    // are disallowed by RFC 6265 but are common in the wild.
    new(
        Ꮡ(new ΔCookie(Name: "special-1", Value: "a z")),
        @"special-1=""a z"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-2", Value: " z")),
        @"special-2="" z"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-3", Value: "a ")),
        @"special-3=""a """
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-4", Value: " ")),
        @"special-4="" """
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-5", Value: "a,z")),
        @"special-5=""a,z"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-6", Value: ",z")),
        @"special-6="",z"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-7", Value: "a,")),
        @"special-7=""a,"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "special-8", Value: ",")),
        @"special-8="","""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "empty-value", Value: "")),
        @"empty-value="
    ),
    new(
        nil,
        @""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "")),
        @""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "\t")),
        @""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "\r")),
        @""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "a\nb", Value: "v")),
        @""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "a\nb", Value: "v")),
        @""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "a\rb", Value: "v")),
        @""
    ),
    // Quoted values (issue #46443)
    new(
        Ꮡ(new ΔCookie(Name: "cookie", Value: "quoted", Quoted: true)),
        @"cookie=""quoted"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie", Value: "quoted with spaces", Quoted: true)),
        @"cookie=""quoted with spaces"""
    ),
    new(
        Ꮡ(new ΔCookie(Name: "cookie", Value: "quoted,with,commas", Quoted: true)),
        @"cookie=""quoted,with,commas"""
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string droppingDomainAttributeˢ => ref droppingDomainAttributeˢᵇ;
private static readonly @string droppingDomainAttributeˢᵇ = "dropping domain attribute"u8;

public static void TestWriteSetCookies(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf));

        foreach (var (i, tt) in writeSetCookiesTests) {
            {
                @string g = tt.Cookie.String();
                @string e = tt.Raw; if (g != e) {
                    t.Errorf("Test %d, expecting:\n%s\nGot:\n%s\n", i, e, g);
                }
            }
        }

        {
            @string got = logbuf.String();
            @string sub = droppingDomainAttributeˢ; if (!strings.Contains(got, sub)) {
                t.Errorf("Expected substring %q in log output. Got:\n%s", sub, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct headerOnlyResponseWriter /*global::go.net.http_package.ΔHeader*/;

internal static global::go.net.http_package.ΔHeader Header(this headerOnlyResponseWriter ho) {
    return (global::go.net.http_package.ΔHeader)ho;
}

internal static (nint, error) Write(this headerOnlyResponseWriter ho, slice<byte> _) {
    throw panic("NOIMPL");
}

internal static void WriteHeader(this headerOnlyResponseWriter ho, nint _) {
    throw panic("NOIMPL");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cookie1OnePathRestrictedˢ => ref cookie1OnePathRestrictedˢᵇ;
private static readonly @string cookie1OnePathRestrictedˢᵇ = "cookie-1=one; Path=/restricted/"u8;
internal static ref readonly @string cookie2TwoMaxAge3600ˢ => ref cookie2TwoMaxAge3600ˢᵇ;
private static readonly @string cookie2TwoMaxAge3600ˢᵇ = "cookie-2=two; Max-Age=3600"u8;

public static void TestSetCookie(ж<testing.T> t) {
    var m = new global::go.net.http_package.ΔHeader(0);
    SetCookie((headerOnlyResponseWriter)m, Ꮡ(new ΔCookie(Name: "cookie-1", Value: "one", Path: "/restricted/")));
    SetCookie((headerOnlyResponseWriter)m, Ꮡ(new ΔCookie(Name: "cookie-2", Value: "two", MaxAge: 3600)));
    {
        nint l = builtin.len(m[setCookieˢ]); if (l != 2) {
            t.Fatalf("expected %d cookies, got %d", (nint)2, l);
        }
    }
    {
        @string g = m[setCookieˢ][0];
        @string e = cookie1OnePathRestrictedˢ; if (g != e) {
            t.Errorf("cookie #1: want %q, got %q", e, g);
        }
    }
    {
        @string g = m[setCookieˢ][1];
        @string e = cookie2TwoMaxAge3600ˢ; if (g != e) {
            t.Errorf("cookie #2: want %q, got %q", e, g);
        }
    }
}

partial struct addCookieTestsᴛ1 /*dyn*/ {
    public slice<ж<global::go.net.http_package.ΔCookie>> Cookies;
    public @string Raw;
}
internal static slice<addCookieTestsᴛ1> addCookieTests = slice<addCookieTestsᴛ1>([
    new(
        slice<ж<global::go.net.http_package.ΔCookie>>([]),
        ""
    ),
    new(
        slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-1", Value: "v$1"))]),
        "cookie-1=v$1"
    ),
    new(
        slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-1", Value: "v$1")),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-2", Value: "v$2")),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-3", Value: "v$3"))
        ]),
        "cookie-1=v$1; cookie-2=v$2; cookie-3=v$3"
    ),
    // Quoted values (issue #46443)
    new(
        slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-1", Value: "quoted", Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-2", Value: "quoted with spaces", Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie-3", Value: "quoted,with,commas", Quoted: true))
        ]),
        @"cookie-1=""quoted""; cookie-2=""quoted with spaces""; cookie-3=""quoted,with,commas"""
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpExampleComˢ => ref httpExampleComˢᵇ;
private static readonly @string httpExampleComˢᵇ = "http://example.com/"u8;

public static void TestAddCookie(ж<testing.T> t) {
    foreach (var (i, tt) in addCookieTests) {
        var (req, _) = NewRequest(getˢ, httpExampleComˢ, default);
        foreach (var (_, c) in tt.Cookies) {
            req.AddCookie(c);
        }
        {
            @string g = req.Header.Get(cookieˢ); if (g != tt.Raw) {
                t.Errorf("Test %d:\nwant: %s\n got: %s\n", i, tt.Raw, g);
            }
        }
    }
}

partial struct readSetCookiesTestsᴛ1 /*dyn*/ {
    internal global::go.net.http_package.ΔHeader header;
    internal slice<ж<global::go.net.http_package.ΔCookie>> cookies;
    internal @string godebug;
}
internal static slice<readSetCookiesTestsᴛ1> readSetCookiesTests = slice<readSetCookiesTestsᴛ1>([
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["Cookie-1=v$1"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1", Raw: "Cookie-1=v$1"))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "NID",
            Value: "99=YsDT5i3E-CXax-",
            Path: "/",
            Domain: ".google.ch",
            HttpOnly: true,
            Expires: time.Date(2011, 11, 23, 1, 5, 3, 0, time.ΔUTC),
            RawExpires: "Wed, 23-Nov-2011 01:05:03 GMT",
            Raw: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: ".ASPXAUTH",
            Value: "7E3AA",
            Path: "/",
            Expires: time.Date(2012, 3, 7, 14, 25, 6, 0, time.ΔUTC),
            RawExpires: "Wed, 07-Mar-2012 14:25:06 GMT",
            HttpOnly: true,
            Raw: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["ASP.NET_SessionId=foo; path=/; HttpOnly"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "ASP.NET_SessionId",
            Value: "foo",
            Path: "/",
            HttpOnly: true,
            Raw: "ASP.NET_SessionId=foo; path=/; HttpOnly"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["samesitedefault=foo; SameSite"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitedefault",
            Value: "foo",
            SameSite: SameSiteDefaultMode,
            Raw: "samesitedefault=foo; SameSite"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["samesiteinvalidisdefault=foo; SameSite=invalid"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesiteinvalidisdefault",
            Value: "foo",
            SameSite: SameSiteDefaultMode,
            Raw: "samesiteinvalidisdefault=foo; SameSite=invalid"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["samesitelax=foo; SameSite=Lax"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitelax",
            Value: "foo",
            SameSite: SameSiteLaxMode,
            Raw: "samesitelax=foo; SameSite=Lax"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["samesitestrict=foo; SameSite=Strict"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitestrict",
            Value: "foo",
            SameSite: SameSiteStrictMode,
            Raw: "samesitestrict=foo; SameSite=Strict"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>(["samesitenone=foo; SameSite=None"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "samesitenone",
            Value: "foo",
            SameSite: SameSiteNoneMode,
            Raw: "samesitenone=foo; SameSite=None"))
        ])
    ),
    // Make sure we can properly read back the Set-Cookie headers we create
    // for values containing spaces or commas:
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-1=a z"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-1", Value: "a z", Raw: @"special-1=a z"))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-2="" z"""])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-2", Value: " z", Quoted: true, Raw: @"special-2="" z"""))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-3=""a """])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-3", Value: "a ", Quoted: true, Raw: @"special-3=""a """))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-4="" """])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-4", Value: " ", Quoted: true, Raw: @"special-4="" """))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-5=a,z"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-5", Value: "a,z", Raw: @"special-5=a,z"))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-6="",z"""])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-6", Value: ",z", Quoted: true, Raw: @"special-6="",z"""))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-7=a,"])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-7", Value: "a,", Raw: @"special-7=a,"))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-8="","""])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-8", Value: ",", Quoted: true, Raw: @"special-8="","""))])
    ),
    // Make sure we can properly read back the Set-Cookie headers
    // for names containing spaces:
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"special-9 ="","""])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "special-9", Value: ",", Quoted: true, Raw: @"special-9 ="","""))])
    ),
    // Quoted values (issue #46443)
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slice<@string>([@"cookie=""quoted"""])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "cookie", Value: "quoted", Quoted: true, Raw: @"cookie=""quoted"""))])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slices.Repeat<slice<@string>, @string>(slice<@string>(["a="]), defaultCookieMaxNum + 1)}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = slices.Repeat<slice<@string>, @string>(slice<@string>(["a="]), 10)}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([]),
        godebug: "httpcookiemaxnum=5"
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = strings.Split(strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..], ";")}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a", Value: "", Quoted: false, Raw: "a="))]), defaultCookieMaxNum + 1),
        godebug: "httpcookiemaxnum=0"
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Set-Cookie"] = strings.Split(strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..], ";")}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a", Value: "", Quoted: false, Raw: "a="))]), defaultCookieMaxNum + 1),
        godebug: fmt.Sprintf("httpcookiemaxnum=%v"u8, (nint)(defaultCookieMaxNum + 1))
    )

    // TODO(bradfitz): users have reported seeing this in the
    // wild, but do browsers handle it? RFC 6265 just says "don't
    // do that" (section 3) and then never mentions header folding
    // again.
    // Header{"Set-Cookie": {"ASP.NET_SessionId=foo; path=/; HttpOnly, .ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"}},
]);

internal static @string toJSON(any v) {
    var (b, err) = json.Marshal(v);
    if (err != nil) {
        return fmt.Sprintf("%#v"u8, v);
    }
    return (@string)b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;

public static void TestReadSetCookies(ж<testing.T> t) {
    foreach (var (i, tt) in readSetCookiesTests) {
        t.Setenv(godebugˢ, tt.godebug);
        for (nint n = 0; n < 2; n++) { // to verify readSetCookies doesn't mutate its input
            var c = readSetCookies(tt.header);
            if (!reflect.DeepEqual(c, tt.cookies)) {
                t.Errorf("#%d readSetCookies: have\n%s\nwant\n%s\n", i, toJSON(c), toJSON(tt.cookies));
            }
        }
    }
}

partial struct readCookiesTestsᴛ1 /*dyn*/ {
    internal global::go.net.http_package.ΔHeader header;
    internal @string filter;
    internal slice<ж<global::go.net.http_package.ΔCookie>> cookies;
    internal @string godebug;
}
internal static slice<readCookiesTestsᴛ1> readCookiesTests = slice<readCookiesTestsᴛ1>([
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>(["Cookie-1=v$1", "c2=v2"])}),
        filter: "",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1")),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>(["Cookie-1=v$1", "c2=v2"])}),
        filter: "c2",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>(["Cookie-1=v$1; c2=v2"])}),
        filter: "",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1")),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>(["Cookie-1=v$1; c2=v2"])}),
        filter: "c2",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>([@"Cookie-1=""v$1""; c2=""v2"""])}),
        filter: "",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1", Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2", Quoted: true))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>([@"Cookie-1=""v$1""; c2=v2;"])}),
        filter: "",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1", Quoted: true)),
            Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2"))
        ])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>([@""])}),
        filter: "",
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([])
    ),
    // GODEBUG=httpcookiemaxnum should work regardless if all cookies are sent
    // via one "Cookie" field, or multiple fields.
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>([strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..]])}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([])
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slices.Repeat<slice<@string>, @string>(slice<@string>(["a="]), 10)}),
        cookies: slice<ж<global::go.net.http_package.ΔCookie>>([]),
        godebug: "httpcookiemaxnum=5"
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slice<@string>([strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..]])}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a", Value: "", Quoted: false))]), defaultCookieMaxNum + 1),
        godebug: "httpcookiemaxnum=0"
    ),
    new(
        header: new ΔHeader(new map<@string, slice<@string>>{["Cookie"] = slices.Repeat<slice<@string>, @string>(slice<@string>(["a="]), defaultCookieMaxNum + 1)}),
        cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a", Value: "", Quoted: false))]), defaultCookieMaxNum + 1),
        godebug: fmt.Sprintf("httpcookiemaxnum=%v"u8, (nint)(defaultCookieMaxNum + 1))
    )
]);

public static void TestReadCookies(ж<testing.T> t) {
    foreach (var (i, tt) in readCookiesTests) {
        t.Setenv(godebugˢ, tt.godebug);
        for (nint n = 0; n < 2; n++) { // to verify readCookies doesn't mutate its input
            var c = readCookies(tt.header, tt.filter);
            if (!reflect.DeepEqual(c, tt.cookies)) {
                t.Errorf("#%d readCookies:\nhave: %s\nwant: %s\n", i, toJSON(c), toJSON(tt.cookies));
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string quoted0NoneMaxAge30ˢ => ref quoted0NoneMaxAge30ˢᵇ;
private static readonly @string quoted0NoneMaxAge30ˢᵇ = @"quoted0=none; max-age=30"u8;
internal static ref readonly @string quoted1CookieValueMaxAgeˢ => ref quoted1CookieValueMaxAgeˢᵇ;
private static readonly @string quoted1CookieValueMaxAgeˢᵇ = @"quoted1=""cookieValue""; max-age=31"u8;
internal static ref readonly @string quoted2CookieAVMaxAge32ˢ => ref quoted2CookieAVMaxAge32ˢᵇ;
private static readonly @string quoted2CookieAVMaxAge32ˢᵇ = @"quoted2=cookieAV; max-age=""32"""u8;
internal static ref readonly @string quoted3BothMaxAge33ˢ => ref quoted3BothMaxAge33ˢᵇ;
private static readonly @string quoted3BothMaxAge33ˢᵇ = @"quoted3=""both""; max-age=""33"""u8;

public static void TestSetCookieDoubleQuotes(ж<testing.T> t) {
    var res = Ꮡ(new Response(Header: new ΔHeader(new map<@string, slice<@string>>{})));
    res.Header.Add(setCookieˢ, quoted0NoneMaxAge30ˢ);
    res.Header.Add(setCookieˢ, quoted1CookieValueMaxAgeˢ);
    res.Header.Add(setCookieˢ, quoted2CookieAVMaxAge32ˢ);
    res.Header.Add(setCookieˢ, quoted3BothMaxAge33ˢ);
    var got = res.Cookies();
    var want = slice<ж<global::go.net.http_package.ΔCookie>>([
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted0", Value: "none", MaxAge: 30)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted1", Value: "cookieValue", MaxAge: 31)),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted2", Value: "cookieAV")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "quoted3", Value: "both"))
    ]);
    if (builtin.len(got) != builtin.len(want)) {
        t.Fatalf("got %d cookies, want %d", builtin.len(got), builtin.len(want));
    }
    foreach (var (i, w) in want) {
        var g = got[i];
        if (g.Name != w.Name || (~g).Value != (~w).Value || g.MaxAge != w.MaxAge) {
            t.Errorf("cookie #%d:\ngot  %v\nwant %v", i, g.OrTypedNil(), w.OrTypedNil());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string droppingInvalidBytesˢ => ref droppingInvalidBytesˢᵇ;
private static readonly @string droppingInvalidBytesˢᵇ = "dropping invalid bytes"u8;

internal partial struct TestCookieSanitizeValue_tests /*dyn*/ {
    internal @string @in;
    internal bool quoted;
    internal @string want;
}

public static void TestCookieSanitizeValue(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf));

        var tests = slice<TestCookieSanitizeValue_tests>([
            new("foo", false, "foo"),
            new("foo;bar", false, "foobar"),
            new("foo\\bar", false, "foobar"),
            new("foo\"bar", false, "foobar"),
            new(((@string)(new byte[]{0x00, 0x7e, 0x7f, 0x80})), false, "\x7e"),
            new(@"withquotes", true, @"""withquotes"""),
            new(@"""withquotes""", true, @"""withquotes"""), // double quotes are not valid octets
            new("a z", false, @"""a z"""),
            new(" z", false, @""" z"""),
            new("a ", false, @"""a """),
            new("a,z", false, @"""a,z"""),
            new(",z", false, @""",z"""),
            new("a,", false, @"""a,"""),
            new("", true, @"""""")
        ]);
        foreach (var (_, tt) in tests) {
            {
                @string got = sanitizeCookieValue(tt.@in, tt.quoted); if (got != tt.want) {
                    t.Errorf("sanitizeCookieValue(%q) = %q; want %q", tt.@in, got, tt.want);
                }
            }
        }

        {
            @string got = logbuf.String();
            @string sub = droppingInvalidBytesˢ; if (!strings.Contains(got, sub)) {
                t.Errorf("Expected substring %q in log output. Got:\n%s", sub, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestCookieSanitizePath_tests /*dyn*/ {
    internal @string @in, want;
}

public static void TestCookieSanitizePath(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        ref var logbuf = ref heap(new strings.Builder(), out var Ꮡlogbuf);
        log.SetOutput(new http_test_package.strings_BuilderжWriter(Ꮡlogbuf));

        var tests = slice<TestCookieSanitizePath_tests>([
            new("/path", "/path"),
            new("/path with space/", "/path with space/"),
            new("/just;no;semicolon\x00orstuff/", "/justnosemicolonorstuff/")
        ]);
        foreach (var (_, tt) in tests) {
            {
                @string got = sanitizeCookiePath(tt.@in); if (got != tt.want) {
                    t.Errorf("sanitizeCookiePath(%q) = %q; want %q", tt.@in, got, tt.want);
                }
            }
        }

        {
            @string got = logbuf.String();
            @string sub = droppingInvalidBytesˢ; if (!strings.Contains(got, sub)) {
                t.Errorf("Expected substring %q in log output. Got:\n%s", sub, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestCookieValid_tests /*dyn*/ {
    internal ж<global::go.net.http_package.ΔCookie> cookie;
    internal bool valid;
}

public static void TestCookieValid(ж<testing.T> t) {
    var tests = slice<TestCookieValid_tests>([
        new(nil, false),
        new(Ꮡ(new ΔCookie(Name: "")), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-value", Value: "foo\"bar")), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-path", Path: "/foo;bar/")), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-secure-for-partitioned", Value: "foo", Path: "/", Secure: false, Partitioned: true)), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-domain", Domain: "example.com:80")), false),
        new(Ꮡ(new ΔCookie(Name: "invalid-expiry", Value: "", Expires: time.Date(1600, 1, 1, 1, 1, 1, 1, time.ΔUTC))), false),
        new(Ꮡ(new ΔCookie(Name: "valid-empty")), true),
        new(Ꮡ(new ΔCookie(Name: "valid-expires", Value: "foo", Path: "/bar", Domain: "example.com", Expires: time.Unix(0, 0))), true),
        new(Ꮡ(new ΔCookie(Name: "valid-max-age", Value: "foo", Path: "/bar", Domain: "example.com", MaxAge: 60)), true),
        new(Ꮡ(new ΔCookie(Name: "valid-all-fields", Value: "foo", Path: "/bar", Domain: "example.com", Expires: time.Unix(0, 0), MaxAge: 0)), true),
        new(Ꮡ(new ΔCookie(Name: "valid-partitioned", Value: "foo", Path: "/", Secure: true, Partitioned: true)), true)
    ]);

    foreach (var (_, tt) in tests) {
        var err = tt.cookie.Valid();
        if (err != nil && tt.valid) {
            t.Errorf("%#v.Valid() returned error %v; want nil", tt.cookie.OrTypedNil(), err);
        }
        if (err == nil && !tt.valid) {
            t.Errorf("%#v.Valid() returned nil; want error", tt.cookie.OrTypedNil());
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantCookieStringᶜ => ref wantCookieStringᶜᵇ;
private static readonly @string wantCookieStringᶜᵇ = @"cookie-9=i3e01nf61b6t23bvfmplnanol3; Path=/restricted/; Domain=example.com; Expires=Tue, 10 Nov 2009 23:00:00 GMT; Max-Age=3600"u8;

public static void BenchmarkCookieString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string wantCookieString = wantCookieStringᶜ;
    var c = Ꮡ(new ΔCookie(
        Name: "cookie-9",
        Value: "i3e01nf61b6t23bvfmplnanol3",
        Expires: time.Unix(1257894000, 0),
        Path: "/restricted/",
        Domain: ".example.com",
        MaxAge: 3600
    ));
    @string benchmarkCookieString = default;
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        benchmarkCookieString = c.String();
    }
    {
        @string have = benchmarkCookieString;
        @string want = wantCookieString; if (have != want) {
            Ꮡb.Fatalf("Have: %v Want: %v", have, want);
        }
    }
}

public static void BenchmarkReadSetCookies(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var header = new ΔHeader(new map<@string, slice<@string>>{
        ["Set-Cookie"] = slice<@string>([
            "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly",
            ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"])
    });
    var wantCookies = slice<ж<global::go.net.http_package.ΔCookie>>([
        Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: "NID",
            Value: "99=YsDT5i3E-CXax-",
            Path: "/",
            Domain: ".google.ch",
            HttpOnly: true,
            Expires: time.Date(2011, 11, 23, 1, 5, 3, 0, time.ΔUTC),
            RawExpires: "Wed, 23-Nov-2011 01:05:03 GMT",
            Raw: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly")),
        Ꮡ(new global::go.net.http_package.ΔCookie(
            Name: ".ASPXAUTH",
            Value: "7E3AA",
            Path: "/",
            Expires: time.Date(2012, 3, 7, 14, 25, 6, 0, time.ΔUTC),
            RawExpires: "Wed, 07-Mar-2012 14:25:06 GMT",
            HttpOnly: true,
            Raw: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"))
    ]);
    slice<ж<global::go.net.http_package.ΔCookie>> c = default;
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        c = readSetCookies(header);
    }
    if (!reflect.DeepEqual(c, wantCookies)) {
        Ꮡb.Fatalf("readSetCookies:\nhave: %s\nwant: %s\n", toJSON(c), toJSON(wantCookies));
    }
}

public static void BenchmarkReadCookies(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var header = new ΔHeader(new map<@string, slice<@string>>{
        ["Cookie"] = slice<@string>([
            @"de=; client_region=0; rpld1=0:hispeed.ch|20:che|21:zh|22:zurich|23:47.36|24:8.53|; rpld0=1:08|; backplane-channel=newspaper.com:1471; devicetype=0; osfam=0; rplmct=2; s_pers=%20s_vmonthnum%3D1472680800496%2526vn%253D1%7C1472680800496%3B%20s_nr%3D1471686767664-New%7C1474278767664%3B%20s_lv%3D1471686767669%7C1566294767669%3B%20s_lv_s%3DFirst%2520Visit%7C1471688567669%3B%20s_monthinvisit%3Dtrue%7C1471688567677%3B%20gvp_p5%3Dsports%253Ablog%253Aearly-lead%2520-%2520184693%2520-%252020160820%2520-%2520u-s%7C1471688567681%3B%20gvp_p51%3Dwp%2520-%2520sports%7C1471688567684%3B; s_sess=%20s_wp_ep%3Dhomepage%3B%20s._ref%3Dhttps%253A%252F%252Fwww.google.ch%252F%3B%20s_cc%3Dtrue%3B%20s_ppvl%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_ppv%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-s-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_dslv%3DFirst%2520Visit%3B%20s_sq%3Dwpninewspapercom%253D%252526pid%25253Dsports%2525253Ablog%2525253Aearly-lead%25252520-%25252520184693%25252520-%2525252020160820%25252520-%25252520u-s%252526pidt%25253D1%252526oid%25253Dhttps%2525253A%2525252F%2525252Fwww.newspaper.com%2525252F%2525253Fnid%2525253Dmenu_nav_homepage%252526ot%25253DA%3B"])
    });
    var wantCookies = slice<ж<global::go.net.http_package.ΔCookie>>([
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "de", Value: "")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "client_region", Value: "0")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "rpld1", Value: "0:hispeed.ch|20:che|21:zh|22:zurich|23:47.36|24:8.53|")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "rpld0", Value: "1:08|")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "backplane-channel", Value: "newspaper.com:1471")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "devicetype", Value: "0")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "osfam", Value: "0")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "rplmct", Value: "2")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "s_pers", Value: "%20s_vmonthnum%3D1472680800496%2526vn%253D1%7C1472680800496%3B%20s_nr%3D1471686767664-New%7C1474278767664%3B%20s_lv%3D1471686767669%7C1566294767669%3B%20s_lv_s%3DFirst%2520Visit%7C1471688567669%3B%20s_monthinvisit%3Dtrue%7C1471688567677%3B%20gvp_p5%3Dsports%253Ablog%253Aearly-lead%2520-%2520184693%2520-%252020160820%2520-%2520u-s%7C1471688567681%3B%20gvp_p51%3Dwp%2520-%2520sports%7C1471688567684%3B")),
        Ꮡ(new global::go.net.http_package.ΔCookie(Name: "s_sess", Value: "%20s_wp_ep%3Dhomepage%3B%20s._ref%3Dhttps%253A%252F%252Fwww.google.ch%252F%3B%20s_cc%3Dtrue%3B%20s_ppvl%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_ppv%3Dsports%25253Ablog%25253Aearly-lead%252520-%252520184693%252520-%25252020160820%252520-%252520u-s-lawyer%252C12%252C12%252C502%252C1231%252C502%252C1680%252C1050%252C2%252CP%3B%20s_dslv%3DFirst%2520Visit%3B%20s_sq%3Dwpninewspapercom%253D%252526pid%25253Dsports%2525253Ablog%2525253Aearly-lead%25252520-%25252520184693%25252520-%2525252020160820%25252520-%25252520u-s%252526pidt%25253D1%252526oid%25253Dhttps%2525253A%2525252F%2525252Fwww.newspaper.com%2525252F%2525253Fnid%2525253Dmenu_nav_homepage%252526ot%25253DA%3B"))
    ]);
    slice<ж<global::go.net.http_package.ΔCookie>> c = default;
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        c = readCookies(header, ""u8);
    }
    if (!reflect.DeepEqual(c, wantCookies)) {
        Ꮡb.Fatalf("readCookies:\nhave: %s\nwant: %s\n", toJSON(c), toJSON(wantCookies));
    }
}

internal partial struct TestParseCookie_tests /*dyn*/ {
    internal @string line;
    internal slice<ж<global::go.net.http_package.ΔCookie>> cookies;
    internal error err;
    internal @string godebug;
}

public static void TestParseCookie(ж<testing.T> t) {
    var tests = slice<TestParseCookie_tests>([
        new(
            line: "Cookie-1=v$1",
            cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1"))])
        ),
        new(
            line: "Cookie-1=v$1;c2=v2",
            cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1")), Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2"))])
        ),
        new(
            line: @"Cookie-1=""v$1"";c2=""v2""",
            cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "Cookie-1", Value: "v$1", Quoted: true)), Ꮡ(new global::go.net.http_package.ΔCookie(Name: "c2", Value: "v2", Quoted: true))])
        ),
        new(
            line: "k1=",
            cookies: slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "k1", Value: ""))])
        ),
        new(
            line: "",
            err: errBlankCookie
        ),
        new(
            line: "equal-not-found",
            err: errEqualNotFoundInCookie
        ),
        new(
            line: "=v1",
            err: errInvalidCookieName
        ),
        new(
            line: "k1=\\",
            err: errInvalidCookieValue
        ),
        new(
            line: strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..],
            err: errCookieNumLimitExceeded
        ),
        new(
            line: strings.Repeat(";a=", 10)[1..],
            err: errCookieNumLimitExceeded,
            godebug: "httpcookiemaxnum=5"
        ),
        new(
            line: strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..],
            cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a", Value: "", Quoted: false))]), defaultCookieMaxNum + 1),
            godebug: "httpcookiemaxnum=0"
        ),
        new(
            line: strings.Repeat(";a=", defaultCookieMaxNum + 1)[1..],
            cookies: slices.Repeat<slice<ж<global::go.net.http_package.ΔCookie>>, ж<global::go.net.http_package.ΔCookie>>(slice<ж<global::go.net.http_package.ΔCookie>>([Ꮡ(new global::go.net.http_package.ΔCookie(Name: "a", Value: "", Quoted: false))]), defaultCookieMaxNum + 1),
            godebug: fmt.Sprintf("httpcookiemaxnum=%v"u8, (nint)(defaultCookieMaxNum + 1))
        )
    ]);
    foreach (var (i, tt) in tests) {
        t.Setenv(godebugˢ, tt.godebug);
        var (gotCookies, gotErr) = ParseCookie(tt.line);
        if (!errors.Is(gotErr, tt.err)) {
            t.Errorf("#%d ParseCookie got error %v, want error %v", i, gotErr, tt.err);
        }
        if (!reflect.DeepEqual(gotCookies, tt.cookies)) {
            t.Errorf("#%d ParseCookie:\ngot cookies: %s\nwant cookies: %s\n", i, toJSON(gotCookies), toJSON(tt.cookies));
        }
    }
}

internal partial struct TestParseSetCookie_tests /*dyn*/ {
    internal @string line;
    internal ж<global::go.net.http_package.ΔCookie> cookie;
    internal error err;
}

public static void TestParseSetCookie(ж<testing.T> t) {
    var tests = slice<TestParseSetCookie_tests>([
        new(
            line: "Cookie-1=v$1",
            cookie: Ꮡ(new ΔCookie(Name: "Cookie-1", Value: "v$1", Raw: "Cookie-1=v$1"))
        ),
        new(
            line: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly",
            cookie: Ꮡ(new ΔCookie(
                Name: "NID",
                Value: "99=YsDT5i3E-CXax-",
                Path: "/",
                Domain: ".google.ch",
                HttpOnly: true,
                Expires: time.Date(2011, 11, 23, 1, 5, 3, 0, time.ΔUTC),
                RawExpires: "Wed, 23-Nov-2011 01:05:03 GMT",
                Raw: "NID=99=YsDT5i3E-CXax-; expires=Wed, 23-Nov-2011 01:05:03 GMT; path=/; domain=.google.ch; HttpOnly"
            ))
        ),
        new(
            line: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly",
            cookie: Ꮡ(new ΔCookie(
                Name: ".ASPXAUTH",
                Value: "7E3AA",
                Path: "/",
                Expires: time.Date(2012, 3, 7, 14, 25, 6, 0, time.ΔUTC),
                RawExpires: "Wed, 07-Mar-2012 14:25:06 GMT",
                HttpOnly: true,
                Raw: ".ASPXAUTH=7E3AA; expires=Wed, 07-Mar-2012 14:25:06 GMT; path=/; HttpOnly"
            ))
        ),
        new(
            line: "ASP.NET_SessionId=foo; path=/; HttpOnly",
            cookie: Ꮡ(new ΔCookie(
                Name: "ASP.NET_SessionId",
                Value: "foo",
                Path: "/",
                HttpOnly: true,
                Raw: "ASP.NET_SessionId=foo; path=/; HttpOnly"
            ))
        ),
        new(
            line: "samesitedefault=foo; SameSite",
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitedefault",
                Value: "foo",
                SameSite: SameSiteDefaultMode,
                Raw: "samesitedefault=foo; SameSite"
            ))
        ),
        new(
            line: "samesiteinvalidisdefault=foo; SameSite=invalid",
            cookie: Ꮡ(new ΔCookie(
                Name: "samesiteinvalidisdefault",
                Value: "foo",
                SameSite: SameSiteDefaultMode,
                Raw: "samesiteinvalidisdefault=foo; SameSite=invalid"
            ))
        ),
        new(
            line: "samesitelax=foo; SameSite=Lax",
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitelax",
                Value: "foo",
                SameSite: SameSiteLaxMode,
                Raw: "samesitelax=foo; SameSite=Lax"
            ))
        ),
        new(
            line: "samesitestrict=foo; SameSite=Strict",
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitestrict",
                Value: "foo",
                SameSite: SameSiteStrictMode,
                Raw: "samesitestrict=foo; SameSite=Strict"
            ))
        ),
        new(
            line: "samesitenone=foo; SameSite=None",
            cookie: Ꮡ(new ΔCookie(
                Name: "samesitenone",
                Value: "foo",
                SameSite: SameSiteNoneMode,
                Raw: "samesitenone=foo; SameSite=None"
            ))
        ),
        // Make sure we can properly read back the Set-Cookie headers we create
        // for values containing spaces or commas:
        new(
            line: @"special-1=a z",
            cookie: Ꮡ(new ΔCookie(Name: "special-1", Value: "a z", Raw: @"special-1=a z"))
        ),
        new(
            line: @"special-2="" z""",
            cookie: Ꮡ(new ΔCookie(Name: "special-2", Value: " z", Quoted: true, Raw: @"special-2="" z"""))
        ),
        new(
            line: @"special-3=""a """,
            cookie: Ꮡ(new ΔCookie(Name: "special-3", Value: "a ", Quoted: true, Raw: @"special-3=""a """))
        ),
        new(
            line: @"special-4="" """,
            cookie: Ꮡ(new ΔCookie(Name: "special-4", Value: " ", Quoted: true, Raw: @"special-4="" """))
        ),
        new(
            line: @"special-5=a,z",
            cookie: Ꮡ(new ΔCookie(Name: "special-5", Value: "a,z", Raw: @"special-5=a,z"))
        ),
        new(
            line: @"special-6="",z""",
            cookie: Ꮡ(new ΔCookie(Name: "special-6", Value: ",z", Quoted: true, Raw: @"special-6="",z"""))
        ),
        new(
            line: @"special-7=a,",
            cookie: Ꮡ(new ΔCookie(Name: "special-7", Value: "a,", Raw: @"special-7=a,"))
        ),
        new(
            line: @"special-8="",""",
            cookie: Ꮡ(new ΔCookie(Name: "special-8", Value: ",", Quoted: true, Raw: @"special-8="","""))
        ),
        // Make sure we can properly read back the Set-Cookie headers
        // for names containing spaces:
        new(
            line: @"special-9 ="",""",
            cookie: Ꮡ(new ΔCookie(Name: "special-9", Value: ",", Quoted: true, Raw: @"special-9 ="","""))
        ),
        new(
            line: "",
            err: errBlankCookie
        ),
        new(
            line: "equal-not-found",
            err: errEqualNotFoundInCookie
        ),
        new(
            line: "=v1",
            err: errInvalidCookieName
        ),
        new(
            line: "k1=\\",
            err: errInvalidCookieValue
        )
    ]);
    foreach (var (i, tt) in tests) {
        var (gotCookie, gotErr) = ParseSetCookie(tt.line);
        if (!errors.Is(gotErr, tt.err)) {
            t.Errorf("#%d ParseSetCookie got error %v, want error %v", i, gotErr, tt.err);
            continue;
        }
        if (!reflect.DeepEqual(gotCookie.OrTypedNil(), tt.cookie.OrTypedNil())) {
            t.Errorf("#%d ParseSetCookie:\ngot cookie: %s\nwant cookie: %s\n", i, toJSON(gotCookie.OrTypedNil()), toJSON(tt.cookie.OrTypedNil()));
        }
    }
}

} // end http_internal_test_package
