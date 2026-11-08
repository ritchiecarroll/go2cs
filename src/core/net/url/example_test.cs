// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using json = encoding.json_package;
using fmt = fmt_package;
using log = log_package;
using url = go.net.url_package;
using strings = strings_package;
using encoding;
using go.net;
using static go.net.url_internal_test_package;

partial class url_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myCoolBlogAboutStuffˢ => ref myCoolBlogAboutStuffˢᵇ;
private static readonly @string myCoolBlogAboutStuffˢᵇ = "my/cool+blog&about,stuff"u8;

public static void ExamplePathEscape() {
    @string path = url.PathEscape(myCoolBlogAboutStuffˢ);
    fmt.Println(path);

    // Output:
    // my%2Fcool+blog&about%2Cstuff
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string my2FcoolBlogAbout2Cstuffˢ => ref my2FcoolBlogAbout2Cstuffˢᵇ;
private static readonly @string my2FcoolBlogAbout2Cstuffˢᵇ = "my%2Fcool+blog&about%2Cstuff"u8;

public static void ExamplePathUnescape() {
    @string escapedPath = my2FcoolBlogAbout2Cstuffˢ;
    var (path, err) = url.PathUnescape(escapedPath);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(path);

    // Output:
    // my/cool+blog&about,stuff
}

public static void ExampleQueryEscape() {
    @string query = url.QueryEscape(myCoolBlogAboutStuffˢ);
    fmt.Println(query);

    // Output:
    // my%2Fcool%2Bblog%26about%2Cstuff
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string my2Fcool2Bblog26aboutˢ => ref my2Fcool2Bblog26aboutˢᵇ;
private static readonly @string my2Fcool2Bblog26aboutˢᵇ = "my%2Fcool%2Bblog%26about%2Cstuff"u8;

public static void ExampleQueryUnescape() {
    @string escapedQuery = my2Fcool2Bblog26aboutˢ;
    var (query, err) = url.QueryUnescape(escapedQuery);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(query);

    // Output:
    // my/cool+blog&about,stuff
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "name"u8;
internal static ref readonly @string avaˢ => ref avaˢᵇ;
private static readonly @string avaˢᵇ = "Ava"u8;
internal static ref readonly @string friendˢ => ref friendˢᵇ;
private static readonly @string friendˢᵇ = "friend"u8;
internal static ref readonly @string jessˢ => ref jessˢᵇ;
private static readonly @string jessˢᵇ = "Jess"u8;
internal static ref readonly @string sarahˢ => ref sarahˢᵇ;
private static readonly @string sarahˢᵇ = "Sarah"u8;
internal static ref readonly @string zoeˢ => ref zoeˢᵇ;
private static readonly @string zoeˢᵇ = "Zoe"u8;

public static void ExampleValues() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Set(nameˢ, avaˢ);
    v.Add(friendˢ, jessˢ);
    v.Add(friendˢ, sarahˢ);
    v.Add(friendˢ, zoeˢ);
    fmt.Println(v.Encode());
    fmt.Println(v.Get(nameˢ));
    fmt.Println(v.Get(friendˢ));
    fmt.Println(v[friendˢ]);
    // Output:
    // friend=Jess&friend=Sarah&friend=Zoe&name=Ava
    // Ava
    // Jess
    // [Jess Sarah Zoe]
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string catSoundsˢ => ref catSoundsˢᵇ;
private static readonly @string catSoundsˢᵇ = "cat sounds"u8;
internal static ref readonly @string meowˢ => ref meowˢᵇ;
private static readonly @string meowˢᵇ = "meow"u8;
internal static ref readonly @string mewˢ => ref mewˢᵇ;
private static readonly @string mewˢᵇ = "mew"u8;
internal static ref readonly @string mauˢ => ref mauˢᵇ;
private static readonly @string mauˢᵇ = "mau"u8;

public static void ExampleValues_Add() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Add(catSoundsˢ, meowˢ);
    v.Add(catSoundsˢ, mewˢ);
    v.Add(catSoundsˢ, mauˢ);
    fmt.Println(v[catSoundsˢ]);

    // Output:
    // [meow mew mau]
}

public static void ExampleValues_Del() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Add(catSoundsˢ, meowˢ);
    v.Add(catSoundsˢ, mewˢ);
    v.Add(catSoundsˢ, mauˢ);
    fmt.Println(v[catSoundsˢ]);

    v.Del(catSoundsˢ);
    fmt.Println(v[catSoundsˢ]);

    // Output:
    // [meow mew mau]
    // []
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mewˢ2 => ref mewˢ2ᵇ;
private static readonly @string mewˢ2ᵇ = "mew/"u8;
internal static ref readonly @string mauˢ2 => ref mauˢ2ᵇ;
private static readonly @string mauˢ2ᵇ = "mau$"u8;

public static void ExampleValues_Encode() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Add(catSoundsˢ, meowˢ);
    v.Add(catSoundsˢ, mewˢ2);
    v.Add(catSoundsˢ, mauˢ2);
    fmt.Println(v.Encode());

    // Output:
    // cat+sounds=meow&cat+sounds=mew%2F&cat+sounds=mau%24
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dogSoundsˢ => ref dogSoundsˢᵇ;
private static readonly @string dogSoundsˢᵇ = "dog sounds"u8;

public static void ExampleValues_Get() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Add(catSoundsˢ, meowˢ);
    v.Add(catSoundsˢ, mewˢ);
    v.Add(catSoundsˢ, mauˢ);
    fmt.Printf("%q\n"u8, v.Get(catSoundsˢ));
    fmt.Printf("%q\n"u8, v.Get(dogSoundsˢ));

    // Output:
    // "meow"
    // ""
}

public static void ExampleValues_Has() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Add(catSoundsˢ, meowˢ);
    v.Add(catSoundsˢ, mewˢ);
    v.Add(catSoundsˢ, mauˢ);
    fmt.Println(v.Has(catSoundsˢ));
    fmt.Println(v.Has(dogSoundsˢ));

    // Output:
    // true
    // false
}

public static void ExampleValues_Set() {
    var v = new url.Values(new map<@string, slice<@string>>{});
    v.Add(catSoundsˢ, meowˢ);
    v.Add(catSoundsˢ, mewˢ);
    v.Add(catSoundsˢ, mauˢ);
    fmt.Println(v[catSoundsˢ]);

    v.Set(catSoundsˢ, meowˢ);
    fmt.Println(v[catSoundsˢ]);

    // Output:
    // [meow mew mau]
    // [meow]
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpBingComSearchQDotnetˢ => ref httpBingComSearchQDotnetˢᵇ;
private static readonly @string httpBingComSearchQDotnetˢᵇ = "http://bing.com/search?q=dotnet"u8;
internal static ref readonly @string httpsˢ => ref httpsˢᵇ;
private static readonly @string httpsˢᵇ = "https"u8;
internal static ref readonly @string googleComˢ => ref googleComˢᵇ;
private static readonly @string googleComˢᵇ = "google.com"u8;
internal static ref readonly @string golangˢ => ref golangˢᵇ;
private static readonly @string golangˢᵇ = "golang"u8;

public static void ExampleURL() {
    var (u, err) = url.Parse(httpBingComSearchQDotnetˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    u.Scheme = httpsˢ;
    u.Host = googleComˢ;
    var q = u.Query();
    q.Set("q", golangˢ);
    u.RawQuery = q.Encode();
    fmt.Println(u.OrTypedNil());
    // Output: https://google.com/search?q=golang
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleComFoo2fbarˢ => ref httpsExampleComFoo2fbarˢᵇ;
private static readonly @string httpsExampleComFoo2fbarˢᵇ = "https://example.com/foo%2fbar"u8;

public static void ExampleURL_roundtrip() {
    // Parse + String preserve the original encoding.
    var (u, err) = url.Parse(httpsExampleComFoo2fbarˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(u.Path);
    fmt.Println(u.RawPath);
    fmt.Println(u.String());
    // Output:
    // /foo/bar
    // /foo%2fbar
    // https://example.com/foo%2fbar
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string searchQDotnetˢ => ref searchQDotnetˢᵇ;
private static readonly @string searchQDotnetˢᵇ = "../../..//search?q=dotnet"u8;
internal static ref readonly @string httpExampleComDirectoryˢ => ref httpExampleComDirectoryˢᵇ;
private static readonly @string httpExampleComDirectoryˢᵇ = "http://example.com/directory/"u8;

public static void ExampleURL_ResolveReference() {
    var (u, err) = url.Parse(searchQDotnetˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    (var @base, err) = url.Parse(httpExampleComDirectoryˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(@base.ResolveReference(u).OrTypedNil());
    // Output:
    // http://example.com/search?q=dotnet
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string x1Y2Y3ˢ => ref x1Y2Y3ˢᵇ;
private static readonly @string x1Y2Y3ˢᵇ = @"x=1&y=2&y=3"u8;

public static void ExampleParseQuery() {
    var (m, err) = url.ParseQuery(x1Y2Y3ˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(toJSON(m));
    // Output:
    // {"x":["1"], "y":["2", "3"]}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpExampleComXY2Fzˢ => ref httpExampleComXY2Fzˢᵇ;
private static readonly @string httpExampleComXY2Fzˢᵇ = "http://example.com/x/y%2Fz"u8;
internal static readonly object pathˢ = (@string)"Path:"u8;
internal static readonly object rawPathˢ = (@string)"RawPath:"u8;
internal static readonly object escapedPathˢ = (@string)"EscapedPath:"u8;

public static void ExampleURL_EscapedPath() {
    var (u, err) = url.Parse(httpExampleComXY2Fzˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(pathˢ, u.Path);
    fmt.Println(rawPathˢ, u.RawPath);
    fmt.Println(escapedPathˢ, u.EscapedPath());
    // Output:
    // Path: /x/y/z
    // RawPath: /x/y%2Fz
    // EscapedPath: /x/y%2Fz
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpExampleComXY2Fzˢ2 => ref httpExampleComXY2Fzˢ2ᵇ;
private static readonly @string httpExampleComXY2Fzˢ2ᵇ = "http://example.com/#x/y%2Fz"u8;
internal static readonly object fragmentˢ = (@string)"Fragment:"u8;
internal static readonly object rawFragmentˢ = (@string)"RawFragment:"u8;
internal static readonly object escapedFragmentˢ = (@string)"EscapedFragment:"u8;

public static void ExampleURL_EscapedFragment() {
    var (u, err) = url.Parse(httpExampleComXY2Fzˢ2);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(fragmentˢ, u.Fragment);
    fmt.Println(rawFragmentˢ, u.RawFragment);
    fmt.Println(escapedFragmentˢ, u.EscapedFragment());
    // Output:
    // Fragment: x/y/z
    // RawFragment: x/y%2Fz
    // EscapedFragment: x/y%2Fz
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrg8000Pathˢ => ref httpsExampleOrg8000Pathˢᵇ;
private static readonly @string httpsExampleOrg8000Pathˢᵇ = "https://example.org:8000/path"u8;
internal static ref readonly @string https20010db885a30000ˢ => ref https20010db885a30000ˢᵇ;
private static readonly @string https20010db885a30000ˢᵇ = "https://[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:17000"u8;

public static void ExampleURL_Hostname() {
    var (u, err) = url.Parse(httpsExampleOrg8000Pathˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(u.Hostname());
    (u, err) = url.Parse(https20010db885a30000ˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(u.Hostname());
    // Output:
    // example.org
    // 2001:0db8:85a3:0000:0000:8a2e:0370:7334
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpˢ => ref httpˢᵇ;
private static readonly @string httpˢᵇ = "http"u8;

public static void ExampleURL_IsAbs() {
    var u = new url.URL(Host: "example.com", Path: "foo");
    fmt.Println(u.IsAbs());
    u.Scheme = httpˢ;
    fmt.Println(u.IsAbs());
    // Output:
    // false
    // true
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleComFooBarˢ => ref httpsExampleComFooBarˢᵇ;
private static readonly @string httpsExampleComFooBarˢᵇ = "https://example.com/foo/bar"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;
internal static ref readonly @string quxˢ => ref quxˢᵇ;
private static readonly @string quxˢᵇ = "qux"u8;

public static void ExampleURL_JoinPath() {
    var (u, err) = url.Parse(httpsExampleComFooBarˢ);
    if (err != nil) {
        log.Fatal(err);
    }

    fmt.Println(u.JoinPath(bazˢ, quxˢ).OrTypedNil());

    // Output:
    // https://example.com/foo/bar/baz/qux
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrgˢ => ref httpsExampleOrgˢᵇ;
private static readonly @string httpsExampleOrgˢᵇ = "https://example.org"u8;

public static void ExampleURL_MarshalBinary() {
    var (u, _) = url.Parse(httpsExampleOrgˢ);
    var (b, err) = u.MarshalBinary();
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Printf("%s\n"u8, b);
    // Output:
    // https://example.org
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "/foo"u8;
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = ":foo"u8;

public static void ExampleURL_Parse() {
    var (u, err) = url.Parse(httpsExampleOrgˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    (var rel, err) = u.Parse(fooˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(rel.OrTypedNil());
    (_, err) = u.Parse(fooˢ2);
    var (_, ok) = err._<ж<urlꓸError>>(ᐧ); if (!ok) {
        log.Fatal(err);
    }
    // Output:
    // https://example.org/foo
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrg8080ˢ => ref httpsExampleOrg8080ˢᵇ;
private static readonly @string httpsExampleOrg8080ˢᵇ = "https://example.org:8080"u8;

public static void ExampleURL_Port() {
    var (u, err) = url.Parse(httpsExampleOrgˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(u.Port());
    (u, err) = url.Parse(httpsExampleOrg8080ˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(u.Port());
    // Output:
    //
    // 8080
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrgA1A2B3ˢ => ref httpsExampleOrgA1A2B3ˢᵇ;
private static readonly @string httpsExampleOrgA1A2B3ˢᵇ = "https://example.org/?a=1&a=2&b=&=3&&&&"u8;

public static void ExampleURL_Query() {
    var (u, err) = url.Parse(httpsExampleOrgA1A2B3ˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    var q = u.Query();
    fmt.Println(q["a"]);
    fmt.Println(q.Get("b"));
    fmt.Println(q.Get(""u8));
    // Output:
    // [1 2]
    //
    // 3
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string passˢ => ref passˢᵇ;
private static readonly @string passˢᵇ = "pass"u8;
internal static ref readonly @string opaqueˢ => ref opaqueˢᵇ;
private static readonly @string opaqueˢᵇ = "opaque"u8;

public static void ExampleURL_String() {
    var u = Ꮡ(new url.URL(
        Scheme: "https",
        User: url.UserPassword("me", passˢ),
        Host: "example.com",
        Path: "foo/bar",
        RawQuery: "x=1&y=2",
        Fragment: "anchor"
    ));
    fmt.Println(u.String());
    u.Opaque = opaqueˢ;
    fmt.Println(u.String());
    // Output:
    // https://me:pass@example.com/foo/bar?x=1&y=2#anchor
    // https:opaque?x=1&y=2#anchor
}

public static void ExampleURL_UnmarshalBinary() {
    var u = Ꮡ(new url.URL());
    var err = u.UnmarshalBinary(slice<byte>("https://example.org/foo"u8));
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Printf("%s\n"u8, u.OrTypedNil());
    // Output:
    // https://example.org/foo
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string userˢ => ref userˢᵇ;
private static readonly @string userˢᵇ = "user"u8;
internal static ref readonly @string passwordˢ => ref passwordˢᵇ;
private static readonly @string passwordˢᵇ = "password"u8;
internal static ref readonly @string newerPasswordˢ => ref newerPasswordˢᵇ;
private static readonly @string newerPasswordˢᵇ = "newerPassword"u8;

public static void ExampleURL_Redacted() {
    var u = Ꮡ(new url.URL(
        Scheme: "https",
        User: url.UserPassword(userˢ, passwordˢ),
        Host: "example.com",
        Path: "foo/bar"
    ));
    fmt.Println(u.Redacted());
    u.User = url.UserPassword("me", newerPasswordˢ);
    fmt.Println(u.Redacted());
    // Output:
    // https://user:xxxxx@example.com/foo/bar
    // https://me:xxxxx@example.com/foo/bar
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleOrgPathFooˢ => ref httpsExampleOrgPathFooˢᵇ;
private static readonly @string httpsExampleOrgPathFooˢᵇ = "https://example.org/path?foo=bar"u8;

public static void ExampleURL_RequestURI() {
    var (u, err) = url.Parse(httpsExampleOrgPathFooˢ);
    if (err != nil) {
        log.Fatal(err);
    }
    fmt.Println(u.RequestURI());
    // Output: /path?foo=bar
}

internal static @string toJSON(any m) {
    var (js, err) = json.Marshal(m);
    if (err != nil) {
        log.Fatal(err);
    }
    return strings.ReplaceAll((@string)js, ",", ", ");
}

} // end url_test_package
