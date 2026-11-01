// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Server unit tests
namespace go.net;

using fmt = fmt_package;
using url = global::go.net.url_package;
using regexp = regexp_package;
using testing = testing_package;
using time = time_package;
using global::go.net;
using io = io_package;
using static global::go.net.http_package;

partial class http_internal_test_package {

internal partial struct TestServerTLSHandshakeTimeout_tests /*dyn*/ {
    internal ж<global::go.net.http_package.Server> s;
    internal time.Duration want;
}

public static void TestServerTLSHandshakeTimeout(ж<testing.T> t) {
    var tests = slice<TestServerTLSHandshakeTimeout_tests>([
        new(
            s: Ꮡ(new Server()),
            want: 0
        ),
        new(
            s: Ꮡ(new Server(
                ReadTimeout: -1
            )),
            want: 0
        ),
        new(
            s: Ꮡ(new Server(
                ReadTimeout: (time.Duration)(5000000000L)
            )),
            want: (time.Duration)(5000000000L)
        ),
        new(
            s: Ꮡ(new Server(
                ReadTimeout: (time.Duration)(5000000000L),
                WriteTimeout: -1
            )),
            want: (time.Duration)(5000000000L)
        ),
        new(
            s: Ꮡ(new Server(
                ReadTimeout: (time.Duration)(5000000000L),
                WriteTimeout: (time.Duration)(4000000000L)
            )),
            want: (time.Duration)(4000000000L)
        ),
        new(
            s: Ꮡ(new Server(
                ReadTimeout: (time.Duration)(5000000000L),
                ReadHeaderTimeout: 2 * time.ΔSecond,
                WriteTimeout: (time.Duration)(4000000000L)
            )),
            want: 2 * time.ΔSecond
        )
    ]);
    foreach (var (i, tt) in tests) {
        var got = tt.s.tlsHandshakeTimeout();
        if (got != tt.want) {
            t.Errorf("%d. got %v; want %v", i, got, tt.want);
        }
    }
}

internal partial struct Δhandler {
    internal nint i;
}

internal static void ServeHTTP(this Δhandler _Δp0, global::go.net.http_package.ResponseWriter _Δp1, ж<global::go.net.http_package.Request> _Δp2) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleComˢ => ref exampleComˢᵇ;
private static readonly @string exampleComˢᵇ = "example.com"u8;

internal partial struct TestFindHandler_type /*dyn*/ {
    internal @string pat;
    internal global::go.net.http_package.ΔHandler h;
}

internal partial struct TestFindHandler_typeᴛ1 /*dyn*/ {
    internal @string method;
    internal @string path;
    internal @string wantHandler;
}

public static void TestFindHandler(ж<testing.T> t) {
    var mux = NewServeMux();
    foreach (var (_, ph) in slice<TestFindHandler_type>([
        new("/", new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(1)))),
        new("/foo/", new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(2)))),
        new("/foo", new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(3)))),
        new("/bar/", new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(4)))),
        new("//foo", new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(5))))
    ])) {
        mux.Handle(ph.pat, ph.h);
    }

    foreach (var (_, test) in slice<TestFindHandler_typeᴛ1>([
        new("GET", "/", "&http.handler{i:1}"),
        new("GET", "//", @"&http.redirectHandler{url:""/"", code:307}"),
        new("GET", "/foo/../bar/./..//baz", @"&http.redirectHandler{url:""/baz"", code:307}"),
        new("GET", "/foo", "&http.handler{i:3}"),
        new("GET", "/foo/x", "&http.handler{i:2}"),
        new("GET", "/bar/x", "&http.handler{i:4}"),
        new("GET", "/bar", @"&http.redirectHandler{url:""/bar/"", code:307}"),
        new("CONNECT", "", "(http.HandlerFunc)(.*)"),
        new("CONNECT", "/", "&http.handler{i:1}"),
        new("CONNECT", "//", "&http.handler{i:1}"),
        new("CONNECT", "//foo", "&http.handler{i:5}"),
        new("CONNECT", "/foo/../bar/./..//baz", "&http.handler{i:2}"),
        new("CONNECT", "/foo", "&http.handler{i:3}"),
        new("CONNECT", "/foo/x", "&http.handler{i:2}"),
        new("CONNECT", "/bar/x", "&http.handler{i:4}"),
        new("CONNECT", "/bar", @"&http.redirectHandler{url:""/bar/"", code:307}")
    ])) {
        ref var r = ref heap(new global::go.net.http_package.Request(), out var Ꮡr);
        r.Method = test.method;
        r.Host = exampleComˢ;
        r.URL = Ꮡ(new url.URL(Path: test.path));
        var (gotH, _, _, _) = mux.findHandler(Ꮡr);
        @string got = fmt.Sprintf("%#v"u8, gotH);
        if (!regexp.MustCompile(test.wantHandler).MatchString(got)) {
            t.Errorf("%s %q: got %q, want %q", test.method, test.path, got, test.wantHandler);
        }
    }
}

public static void TestEmptyServeMux(ж<testing.T> t) {
    // Verify that a ServeMux with nothing registered
    // doesn't panic.
    var mux = NewServeMux();
    ref var r = ref heap(new global::go.net.http_package.Request(), out var Ꮡr);
    r.Method = getˢ;
    r.Host = exampleComˢ;
    r.URL = Ꮡ(new url.URL(Path: "/"));
    var (_, p) = mux.Handler(Ꮡr);
    if (p != ""u8) {
        t.Errorf(@"got %q, want """"", p);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilErrorˢ = (@string)"got nil error"u8;

internal partial struct TestRegisterErr_type /*dyn*/ {
    internal @string pattern;
    internal global::go.net.http_package.ΔHandler handler;
    internal @string wantRegexp;
}

public static void TestRegisterErr(ж<testing.T> t) {
    var mux = NewServeMux();
    var h = Ꮡ(new Δhandler());
    mux.Handle("/a", new http_internal_test_package.ΔhandlerжΔHandler(h));

    foreach (var (_, vᴛ1) in slice<TestRegisterErr_type>([
        new("", new http_internal_test_package.ΔhandlerжΔHandler(h), "invalid pattern"),
        new("/", default, "nil handler"),
        new("/", new http_test_package.http_HandlerFuncᴠΔHandler(default(global::go.net.http_package.HandlerFunc)!), "nil handler"),
        new("/{x", new http_internal_test_package.ΔhandlerжΔHandler(h), @"parsing ""/\{x"": at offset 1: bad wildcard segment"),
        new("/a", new http_internal_test_package.ΔhandlerжΔHandler(h), @"conflicts with pattern.* \(registered at .*/server_test.go:\d+")
    ])) {
        ref var test = ref heap(new TestRegisterErr_type(), out var Ꮡtest);
        test = vᴛ1;

        var muxʗ1 = mux;
        var testʗ1 = test;
        t.Run(fmt.Sprintf("%s:%#v"u8, test.pattern, test.handler), (ж<testing.T> tΔ1) => {
            var err = muxʗ1.registerErr(testʗ1.pattern, testʗ1.handler);
            if (err == nil) {
                tΔ1.Fatal(gotNilErrorˢ);
            }
            var re = regexp.MustCompile(testʗ1.wantRegexp);
            {
                @string g = err.Error(); if (!re.MatchString(g)) {
                    tΔ1.Errorf("\ngot %q\nwant string matching %q", g, testʗ1.wantRegexp);
                }
            }
        });
    }
}

internal partial struct TestExactMatch_type /*dyn*/ {
    internal @string pattern;
    internal @string path;
    internal bool want;
}

public static void TestExactMatch(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestExactMatch_type>([
        new("", "/a", false),
        new("/", "/a", false),
        new("/a", "/a", true),
        new("/a/{x...}", "/a/b", false),
        new("/a/{x}", "/a/b", true),
        new("/a/b/", "/a/b/", true),
        new("/a/b/{$}", "/a/b/", true),
        new("/a/", "/a/b/", false)
    ])) {
        ж<global::go.net.http_package.routingNode> n = default;
        if (test.pattern != ""u8) {
            var pat = mustParsePattern(new http_test_package.testing_TжTB(t), test.pattern);
            n = Ꮡ(new routingNode(pattern: pat));
        }
        var got = exactMatch(n, test.path);
        if (got != test.want) {
            t.Errorf("%q, %s: got %t, want %t", test.pattern, test.path, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string latestˢ => ref latestˢᵇ;
private static readonly @string latestˢᵇ = "latest"u8;

internal partial struct TestEscapedPathsAndPatterns_matches /*dyn*/ {
    internal @string pattern;
    internal slice<@string> paths;    // paths that match the pattern
    internal slice<@string> paths121; // paths that matched the pattern in Go 1.21.
}

public static void TestEscapedPathsAndPatterns(ж<testing.T> t) {
    var matches = slice<TestEscapedPathsAndPatterns_matches>([
        new(
            "/a", // this pattern matches a path that unescapes to "/a"
            slice<@string>(["/a", "/%61"]),
            slice<@string>(["/a", "/%61"])
        ),
        new(
            "/%62", // patterns are unescaped by segment; matches paths that unescape to "/b"
            slice<@string>(["/b", "/%62"]),
            slice<@string>(["/%2562"]) // In 1.21, patterns were not unescaped but paths were.

        ),
        new(
            "/%7B/%7D", // the only way to write a pattern that matches '{' or '}'
            slice<@string>(["/{/}", "/%7b/}", "/{/%7d", "/%7B/%7D"]),
            slice<@string>(["/%257B/%257D"]) // In 1.21, patterns were not unescaped.

        ),
        new(
            "/%x", // patterns that do not unescape are left unchanged
            slice<@string>(["/%25x"]),
            slice<@string>(["/%25x"])
        )
    ]);

    var matchesʗ1 = matches;
    void run(ж<testing.T> tΔ1, bool test121) {
        GoFrame ᒐ = default;
        try {
            defer((bool u) => {
                use121 = u;
            }, use121, ref ᒐ);
            use121 = test121;

            var mux = NewServeMux();
            foreach (var (_, m) in matchesʗ1) {
                mux.HandleFunc(m.pattern, (global::go.net.http_package.ResponseWriter w, ж<global::go.net.http_package.Request> r) => {
                });
            }

            foreach (var (_, m) in matchesʗ1) {
                var paths = m.paths;
                if (use121) {
                    paths = m.paths121;
                }
                foreach (var (_, p) in paths) {
                    var (u, err) = url.ParseRequestURI(p);
                    if (err != nil) {
                        tΔ1.Fatal(err);
                    }
                    var req = Ꮡ(new Request(
                        URL: u
                    ));
                    var (_, gotPattern) = mux.Handler(req);
                    {
                        @string g = gotPattern;
                        @string w = m.pattern; if (g != w) {
                            tΔ1.Errorf("%s: pattern: got %q, want %q", p, g, w);
                        }
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    var runʗ1 = run;
    t.Run(latestˢ, (ж<testing.T> tΔ2) => {
        runʗ1(tΔ2, false);
    });
    var runʗ2 = run;
    t.Run("1.21", (ж<testing.T> tΔ3) => {
        runʗ2(tΔ3, true);
    });
}

internal partial struct TestCleanPath_type /*dyn*/ {
    internal @string @in, want;
}

public static void TestCleanPath(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestCleanPath_type>([
        new("//", "/"),
        new("/x", "/x"),
        new("//x", "/x"),
        new("x//", "/x/"),
        new("a//b/////c", "/a/b/c"),
        new("/foo/../bar/./..//baz", "/baz")
    ])) {
        @string got = cleanPath(test.@in);
        if (got != test.want) {
            t.Errorf("%s: got %q, want %q", test.@in, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string indexˢ => ref indexˢᵇ;
private static readonly @string indexˢᵇ = "/index"u8;
internal static ref readonly @string homeˢ => ref homeˢᵇ;
private static readonly @string homeˢᵇ = "/home"u8;
internal static ref readonly @string aboutˢ => ref aboutˢᵇ;
private static readonly @string aboutˢᵇ = "/about"u8;
internal static ref readonly @string contactˢ => ref contactˢᵇ;
private static readonly @string contactˢᵇ = "/contact"u8;
internal static ref readonly @string robotsTxtˢ => ref robotsTxtˢᵇ;
private static readonly @string robotsTxtˢᵇ = "/robots.txt"u8;
internal static ref readonly @string productsˢ => ref productsˢᵇ;
private static readonly @string productsˢᵇ = "/products/"u8;
internal static ref readonly @string products1ˢ => ref products1ˢᵇ;
private static readonly @string products1ˢᵇ = "/products/1"u8;
internal static ref readonly @string products2ˢ => ref products2ˢᵇ;
private static readonly @string products2ˢᵇ = "/products/2"u8;
internal static ref readonly @string products3ˢ => ref products3ˢᵇ;
private static readonly @string products3ˢᵇ = "/products/3"u8;
internal static ref readonly @string products3ImageJpgˢ => ref products3ImageJpgˢᵇ;
private static readonly @string products3ImageJpgˢᵇ = "/products/3/image.jpg"u8;
internal static ref readonly @string adminˢ => ref adminˢᵇ;
private static readonly @string adminˢᵇ = "/admin"u8;
internal static ref readonly @string adminProductsˢ => ref adminProductsˢᵇ;
private static readonly @string adminProductsˢᵇ = "/admin/products/"u8;
internal static ref readonly @string adminProductsCreateˢ => ref adminProductsCreateˢᵇ;
private static readonly @string adminProductsCreateˢᵇ = "/admin/products/create"u8;
internal static ref readonly @string adminProductsUpdateˢ => ref adminProductsUpdateˢᵇ;
private static readonly @string adminProductsUpdateˢᵇ = "/admin/products/update"u8;
internal static ref readonly @string adminProductsDeleteˢ => ref adminProductsDeleteˢᵇ;
private static readonly @string adminProductsDeleteˢᵇ = "/admin/products/delete"u8;
internal static readonly object impossibleˢ = (@string)"impossible"u8;

public static void BenchmarkServerMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var fn = (global::go.net.http_package.ResponseWriter w, ж<global::go.net.http_package.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "OK"u8);
    };
    var mux = NewServeMux();
    mux.HandleFunc("/", fn);
    mux.HandleFunc(indexˢ, fn);
    mux.HandleFunc(homeˢ, fn);
    mux.HandleFunc(aboutˢ, fn);
    mux.HandleFunc(contactˢ, fn);
    mux.HandleFunc(robotsTxtˢ, fn);
    mux.HandleFunc(productsˢ, fn);
    mux.HandleFunc(products1ˢ, fn);
    mux.HandleFunc(products2ˢ, fn);
    mux.HandleFunc(products3ˢ, fn);
    mux.HandleFunc(products3ImageJpgˢ, fn);
    mux.HandleFunc(adminˢ, fn);
    mux.HandleFunc(adminProductsˢ, fn);
    mux.HandleFunc(adminProductsCreateˢ, fn);
    mux.HandleFunc(adminProductsUpdateˢ, fn);
    mux.HandleFunc(adminProductsDeleteˢ, fn);

    var paths = slice<@string>(["/", "/notfound", "/admin/", "/admin/foo", "/contact", "/products",
        "/products/", "/products/3/image.jpg"]);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        var (r, err) = NewRequest(getˢ, "http://example.com/" + paths[i % builtin.len(paths)], default);
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        {
            var (h, p, _, _) = mux.findHandler(r); if (h != nil && p == ""u8) {
                Ꮡb.Error(impossibleˢ);
            }
        }
    }
    b.StopTimer();
}

} // end http_internal_test_package
