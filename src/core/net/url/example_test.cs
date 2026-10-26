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
internal static readonly @string myCoolBlogAboutStuffˢ = "my/cool+blog&about,stuff"u8;

public static void ExamplePathEscape() {
    @string path = url.PathEscape(myCoolBlogAboutStuffˢ);
    fmt.Println(path);

    // Output:
    // my%2Fcool+blog&about%2Cstuff
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string my2FcoolBlogAbout2Cstuffˢ = "my%2Fcool+blog&about%2Cstuff"u8;

public static void ExamplePathUnescape() {
    @string escapedPath = my2FcoolBlogAbout2Cstuffˢ;
    var (path, err) = url.PathUnescape(escapedPath);
    if (err != default!) {
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
internal static readonly @string my2Fcool2Bblog26aboutˢ = "my%2Fcool%2Bblog%26about%2Cstuff"u8;

public static void ExampleQueryUnescape() {
    @string escapedQuery = my2Fcool2Bblog26aboutˢ;
    var (query, err) = url.QueryUnescape(escapedQuery);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(query);

    // Output:
    // my/cool+blog&about,stuff
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameˢ = "name"u8;
internal static readonly @string avaˢ = "Ava"u8;
internal static readonly @string friendˢ = "friend"u8;
internal static readonly @string jessˢ = "Jess"u8;
internal static readonly @string sarahˢ = "Sarah"u8;
internal static readonly @string zoeˢ = "Zoe"u8;

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
internal static readonly @string catSoundsˢ = "cat sounds"u8;
internal static readonly @string meowˢ = "meow"u8;
internal static readonly @string mewˢ = "mew"u8;
internal static readonly @string mauˢ = "mau"u8;

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
internal static readonly @string mewˢ2 = "mew/"u8;
internal static readonly @string mauˢ2 = "mau$"u8;

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
internal static readonly @string dogSoundsˢ = "dog sounds"u8;

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
internal static readonly @string httpBingComSearchQDotnetˢ = "http://bing.com/search?q=dotnet"u8;
internal static readonly @string httpsˢ = "https"u8;
internal static readonly @string googleComˢ = "google.com"u8;
internal static readonly @string golangˢ = "golang"u8;

public static void ExampleURL() {
    var (u, err) = url.Parse(httpBingComSearchQDotnetˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    u.Value.Scheme = httpsˢ;
    u.Value.Host = googleComˢ;
    var q = u.Query();
    q.Set("q"u8, golangˢ);
    u.Value.RawQuery = q.Encode();
    fmt.Println(u.OrTypedNil());
    // Output: https://google.com/search?q=golang
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleComFoo2fbarˢ = "https://example.com/foo%2fbar"u8;

public static void ExampleURL_roundtrip() {
    // Parse + String preserve the original encoding.
    var (u, err) = url.Parse(httpsExampleComFoo2fbarˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println((~u).Path);
    fmt.Println((~u).RawPath);
    fmt.Println(u.String());
    // Output:
    // /foo/bar
    // /foo%2fbar
    // https://example.com/foo%2fbar
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string searchQDotnetˢ = "../../..//search?q=dotnet"u8;
internal static readonly @string httpExampleComDirectoryˢ = "http://example.com/directory/"u8;

public static void ExampleURL_ResolveReference() {
    var (u, err) = url.Parse(searchQDotnetˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    (var @base, err) = url.Parse(httpExampleComDirectoryˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(@base.ResolveReference(u).OrTypedNil());
    // Output:
    // http://example.com/search?q=dotnet
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x1Y2Y3ˢ = @"x=1&y=2&y=3"u8;

public static void ExampleParseQuery() {
    var (m, err) = url.ParseQuery(x1Y2Y3ˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(toJSON(m));
    // Output:
    // {"x":["1"], "y":["2", "3"]}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComXY2Fzˢ = "http://example.com/x/y%2Fz"u8;
internal static readonly object pathˢ = (@string)"Path:"u8;
internal static readonly object rawPathˢ = (@string)"RawPath:"u8;
internal static readonly object escapedPathˢ = (@string)"EscapedPath:"u8;

public static void ExampleURL_EscapedPath() {
    var (u, err) = url.Parse(httpExampleComXY2Fzˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(pathˢ, (~u).Path);
    fmt.Println(rawPathˢ, (~u).RawPath);
    fmt.Println(escapedPathˢ, u.EscapedPath());
    // Output:
    // Path: /x/y/z
    // RawPath: /x/y%2Fz
    // EscapedPath: /x/y%2Fz
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpExampleComXY2Fzˢ2 = "http://example.com/#x/y%2Fz"u8;
internal static readonly object fragmentˢ = (@string)"Fragment:"u8;
internal static readonly object rawFragmentˢ = (@string)"RawFragment:"u8;
internal static readonly object escapedFragmentˢ = (@string)"EscapedFragment:"u8;

public static void ExampleURL_EscapedFragment() {
    var (u, err) = url.Parse(httpExampleComXY2Fzˢ2);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(fragmentˢ, (~u).Fragment);
    fmt.Println(rawFragmentˢ, (~u).RawFragment);
    fmt.Println(escapedFragmentˢ, u.EscapedFragment());
    // Output:
    // Fragment: x/y/z
    // RawFragment: x/y%2Fz
    // EscapedFragment: x/y%2Fz
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleOrg8000Pathˢ = "https://example.org:8000/path"u8;
internal static readonly @string https20010db885a30000ˢ = "https://[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:17000"u8;

public static void ExampleURL_Hostname() {
    var (u, err) = url.Parse(httpsExampleOrg8000Pathˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(u.Hostname());
    (u, err) = url.Parse(https20010db885a30000ˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(u.Hostname());
    // Output:
    // example.org
    // 2001:0db8:85a3:0000:0000:8a2e:0370:7334
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpˢ = "http"u8;

public static void ExampleURL_IsAbs() {
    var u = new url.URL(Host: "example.com"u8, Path: "foo"u8);
    fmt.Println(u.IsAbs());
    u.Scheme = httpˢ;
    fmt.Println(u.IsAbs());
    // Output:
    // false
    // true
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleComFooBarˢ = "https://example.com/foo/bar"u8;
internal static readonly @string bazˢ = "baz"u8;
internal static readonly @string quxˢ = "qux"u8;

public static void ExampleURL_JoinPath() {
    var (u, err) = url.Parse(httpsExampleComFooBarˢ);
    if (err != default!) {
        log.Fatal(err);
    }

    fmt.Println(u.JoinPath(bazˢ, quxˢ).OrTypedNil());

    // Output:
    // https://example.com/foo/bar/baz/qux
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleOrgˢ = "https://example.org"u8;

public static void ExampleURL_MarshalBinary() {
    var (u, _) = url.Parse(httpsExampleOrgˢ);
    var (b, err) = u.MarshalBinary();
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Printf("%s\n"u8, b);
    // Output:
    // https://example.org
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "/foo"u8;
internal static readonly @string fooˢ2 = ":foo"u8;

public static void ExampleURL_Parse() {
    var (u, err) = url.Parse(httpsExampleOrgˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    (var rel, err) = u.Parse(fooˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(rel.OrTypedNil());
    (_, err) = u.Parse(fooˢ2);
    {
        var (_, ok) = err._<ж<urlꓸError>>(ᐧ); if (!ok) {
            log.Fatal(err);
        }
    }
    // Output:
    // https://example.org/foo
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleOrg8080ˢ = "https://example.org:8080"u8;

public static void ExampleURL_Port() {
    var (u, err) = url.Parse(httpsExampleOrgˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(u.Port());
    (u, err) = url.Parse(httpsExampleOrg8080ˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(u.Port());
    // Output:
    //
    // 8080
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleOrgA1A2B3ˢ = "https://example.org/?a=1&a=2&b=&=3&&&&"u8;

public static void ExampleURL_Query() {
    var (u, err) = url.Parse(httpsExampleOrgA1A2B3ˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    var q = u.Query();
    fmt.Println(q["a"u8]);
    fmt.Println(q.Get("b"u8));
    fmt.Println(q.Get(""u8));
    // Output:
    // [1 2]
    //
    // 3
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string passˢ = "pass"u8;
internal static readonly @string opaqueˢ = "opaque"u8;

public static void ExampleURL_String() {
    var u = Ꮡ(new url.URL(
        Scheme: "https"u8,
        User: url.UserPassword("me"u8, passˢ),
        Host: "example.com"u8,
        Path: "foo/bar"u8,
        RawQuery: "x=1&y=2"u8,
        Fragment: "anchor"u8
    ));
    fmt.Println(u.String());
    u.Value.Opaque = opaqueˢ;
    fmt.Println(u.String());
    // Output:
    // https://me:pass@example.com/foo/bar?x=1&y=2#anchor
    // https:opaque?x=1&y=2#anchor
}

public static void ExampleURL_UnmarshalBinary() {
    var u = Ꮡ(new url.URL(nil));
    var err = u.UnmarshalBinary(slice<byte>("https://example.org/foo"u8));
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Printf("%s\n"u8, u.OrTypedNil());
    // Output:
    // https://example.org/foo
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userˢ = "user"u8;
internal static readonly @string passwordˢ = "password"u8;
internal static readonly @string newerPasswordˢ = "newerPassword"u8;

public static void ExampleURL_Redacted() {
    var u = Ꮡ(new url.URL(
        Scheme: "https"u8,
        User: url.UserPassword(userˢ, passwordˢ),
        Host: "example.com"u8,
        Path: "foo/bar"u8
    ));
    fmt.Println(u.Redacted());
    u.Value.User = url.UserPassword("me"u8, newerPasswordˢ);
    fmt.Println(u.Redacted());
    // Output:
    // https://user:xxxxx@example.com/foo/bar
    // https://me:xxxxx@example.com/foo/bar
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleOrgPathFooˢ = "https://example.org/path?foo=bar"u8;

public static void ExampleURL_RequestURI() {
    var (u, err) = url.Parse(httpsExampleOrgPathFooˢ);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Println(u.RequestURI());
    // Output: /path?foo=bar
}

internal static @string toJSON(any m) {
    var (js, err) = json.Marshal(m);
    if (err != default!) {
        log.Fatal(err);
    }
    return strings.ReplaceAll((@string)js, ","u8, ", "u8);
}

} // end url_test_package
