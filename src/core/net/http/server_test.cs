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

public static void TestServerTLSHandshakeTimeout(ж<testing.T> Ꮡt) {
    var tests = new TestServerTLSHandshakeTimeout_tests[]{
        new(
            s: Ꮡ(new Server(nil)),
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
    }.slice();
    foreach (var (i, tt) in tests) {
        var got = tt.s.tlsHandshakeTimeout();
        if (got != tt.want) {
            Ꮡt.Errorf("%d. got %v; want %v"u8, i, got, tt.want);
        }
    }
}

internal partial struct Δhandler {
    internal nint i;
}

internal static void ServeHTTP(this Δhandler _Δp0, global::go.net.http_package.ResponseWriter _Δp1, ж<global::go.net.http_package.Request> _Δp2) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleComˢ = "example.com"u8;

internal partial struct TestFindHandler_type /*dyn*/ {
    internal @string pat;
    internal global::go.net.http_package.ΔHandler h;
}

internal partial struct TestFindHandler_typeᴛ1 /*dyn*/ {
    internal @string method;
    internal @string path;
    internal @string wantHandler;
}

public static void TestFindHandler(ж<testing.T> Ꮡt) {
    var mux = NewServeMux();
    foreach (var (_, ph) in new TestFindHandler_type[]{
        new("/"u8, new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(1)))),
        new("/foo/"u8, new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(2)))),
        new("/foo"u8, new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(3)))),
        new("/bar/"u8, new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(4)))),
        new("//foo"u8, new http_internal_test_package.ΔhandlerжΔHandler(Ꮡ(new Δhandler(5))))
    }.slice()) {
        mux.Handle(ph.pat, ph.h);
    }
    foreach (var (_, test) in new TestFindHandler_typeᴛ1[]{
        new("GET"u8, "/"u8, "&http.handler{i:1}"u8),
        new("GET"u8, "//"u8, @"&http.redirectHandler{url:""/"", code:301}"u8),
        new("GET"u8, "/foo/../bar/./..//baz"u8, @"&http.redirectHandler{url:""/baz"", code:301}"u8),
        new("GET"u8, "/foo"u8, "&http.handler{i:3}"u8),
        new("GET"u8, "/foo/x"u8, "&http.handler{i:2}"u8),
        new("GET"u8, "/bar/x"u8, "&http.handler{i:4}"u8),
        new("GET"u8, "/bar"u8, @"&http.redirectHandler{url:""/bar/"", code:301}"u8),
        new("CONNECT"u8, "/"u8, "&http.handler{i:1}"u8),
        new("CONNECT"u8, "//"u8, "&http.handler{i:1}"u8),
        new("CONNECT"u8, "//foo"u8, "&http.handler{i:5}"u8),
        new("CONNECT"u8, "/foo/../bar/./..//baz"u8, "&http.handler{i:2}"u8),
        new("CONNECT"u8, "/foo"u8, "&http.handler{i:3}"u8),
        new("CONNECT"u8, "/foo/x"u8, "&http.handler{i:2}"u8),
        new("CONNECT"u8, "/bar/x"u8, "&http.handler{i:4}"u8),
        new("CONNECT"u8, "/bar"u8, @"&http.redirectHandler{url:""/bar/"", code:301}"u8)
    }.slice()) {
        ref var r = ref heap(new global::go.net.http_package.Request(), out var Ꮡr);
        r.Method = test.method;
        r.Host = exampleComˢ;
        r.URL = Ꮡ(new url.URL(Path: test.path));
        var (gotH, _, _, _) = mux.findHandler(Ꮡr);
        @string got = fmt.Sprintf("%#v"u8, gotH);
        if (got != test.wantHandler) {
            Ꮡt.Errorf("%s %q: got %q, want %q"u8, test.method, test.path, got, test.wantHandler);
        }
    }
}

public static void TestEmptyServeMux(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Verify that a ServeMux with nothing registered
    // doesn't panic.
    var mux = NewServeMux();
    ref var r = ref heap(new global::go.net.http_package.Request(), out var Ꮡr);
    r.Method = getˢ;
    r.Host = exampleComˢ;
    r.URL = Ꮡ(new url.URL(Path: "/"u8));
    var (_, p) = mux.Handler(Ꮡr);
    if (p != ""u8) {
        Ꮡt.Errorf(@"got %q, want """""u8, p);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilErrorˢ = (@string)"got nil error"u8;

internal partial struct TestRegisterErr_type /*dyn*/ {
    internal @string pattern;
    internal global::go.net.http_package.ΔHandler handler;
    internal @string wantRegexp;
}

public static void TestRegisterErr(ж<testing.T> Ꮡt) {
    var mux = NewServeMux();
    var h = Ꮡ(new Δhandler(nil));
    mux.Handle("/a"u8, new http_internal_test_package.ΔhandlerжΔHandler(h));
    foreach (var (_, vᴛ1) in new TestRegisterErr_type[]{
        new(""u8, new http_internal_test_package.ΔhandlerжΔHandler(h), "invalid pattern"u8),
        new("/"u8, default!, "nil handler"u8),
        new("/"u8, new http_test_package.http_HandlerFuncᴠΔHandler(default(global::go.net.http_package.HandlerFunc)!), "nil handler"u8),
        new("/{x"u8, new http_internal_test_package.ΔhandlerжΔHandler(h), @"parsing ""/\{x"": at offset 1: bad wildcard segment"u8),
        new("/a"u8, new http_internal_test_package.ΔhandlerжΔHandler(h), @"conflicts with pattern.* \(registered at .*/server_test.go:\d+"u8)
    }.slice()) {
        ref var test = ref heap(new TestRegisterErr_type(), out var Ꮡtest);
        test = vᴛ1;

        var muxʗ1 = mux;
        var testʗ1 = test;
        Ꮡt.Run(fmt.Sprintf("%s:%#v"u8, test.pattern, test.handler), (ж<testing.T> tΔ1) => {
            var err = muxʗ1.registerErr(testʗ1.pattern, testʗ1.handler);
            if (err == default!) {
                tΔ1.Fatal(gotNilErrorˢ);
            }
            var re = regexp.MustCompile(testʗ1.wantRegexp);
            {
                @string g = err.Error(); if (!re.MatchString(g)) {
                    tΔ1.Errorf("\ngot %q\nwant string matching %q"u8, g, testʗ1.wantRegexp);
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

public static void TestExactMatch(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestExactMatch_type[]{
        new(""u8, "/a"u8, false),
        new("/"u8, "/a"u8, false),
        new("/a"u8, "/a"u8, true),
        new("/a/{x...}"u8, "/a/b"u8, false),
        new("/a/{x}"u8, "/a/b"u8, true),
        new("/a/b/"u8, "/a/b/"u8, true),
        new("/a/b/{$}"u8, "/a/b/"u8, true),
        new("/a/"u8, "/a/b/"u8, false)
    }.slice()) {
        ж<global::go.net.http_package.routingNode> n = default!;
        if (test.pattern != ""u8) {
            var pat = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.pattern);
            n = Ꮡ(new routingNode(pattern: pat));
        }
        var got = exactMatch(n, test.path);
        if (got != test.want) {
            Ꮡt.Errorf("%q, %s: got %t, want %t"u8, test.pattern, test.path, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string latestˢ = "latest"u8;

internal partial struct TestEscapedPathsAndPatterns_matches /*dyn*/ {
    internal @string pattern;
    internal slice<@string> paths; // paths that match the pattern
    internal slice<@string> paths121; // paths that matched the pattern in Go 1.21.
}

public static void TestEscapedPathsAndPatterns(ж<testing.T> Ꮡt) {
    var matches = new TestEscapedPathsAndPatterns_matches[]{
        new(
            "/a"u8, // this pattern matches a path that unescapes to "/a"

            new @string[]{"/a"u8, "/%61"u8}.slice(),
            new @string[]{"/a"u8, "/%61"u8}.slice()
        ),
        new(
            "/%62"u8, // patterns are unescaped by segment; matches paths that unescape to "/b"

            new @string[]{"/b"u8, "/%62"u8}.slice(),
            new @string[]{"/%2562"u8}.slice() // In 1.21, patterns were not unescaped but paths were.

        ),
        new(
            "/%7B/%7D"u8, // the only way to write a pattern that matches '{' or '}'

            new @string[]{"/{/}"u8, "/%7b/}"u8, "/{/%7d"u8, "/%7B/%7D"u8}.slice(),
            new @string[]{"/%257B/%257D"u8}.slice() // In 1.21, patterns were not unescaped.

        ),
        new(
            "/%x"u8, // patterns that do not unescape are left unchanged

            new @string[]{"/%25x"u8}.slice(),
            new @string[]{"/%25x"u8}.slice()
        )
    }.slice();
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
                    if (err != default!) {
                        tΔ1.Fatal(err);
                    }
                    var req = Ꮡ(new Request(
                        URL: u
                    ));
                    var (_, gotPattern) = mux.Handler(req);
                    {
                        @string g = gotPattern;
                        @string w = m.pattern; if (g != w) {
                            tΔ1.Errorf("%s: pattern: got %q, want %q"u8, p, g, w);
                        }
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var runʗ1 = run;
    Ꮡt.Run(latestˢ, (ж<testing.T> tΔ2) => {
        runʗ1(tΔ2, false);
    });
    var runʗ2 = run;
    Ꮡt.Run("1.21"u8, (ж<testing.T> tΔ3) => {
        runʗ2(tΔ3, true);
    });
}

internal partial struct TestCleanPath_type /*dyn*/ {
    internal @string @in, want;
}

public static void TestCleanPath(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestCleanPath_type[]{
        new("//"u8, "/"u8),
        new("/x"u8, "/x"u8),
        new("//x"u8, "/x"u8),
        new("x//"u8, "/x/"u8),
        new("a//b/////c"u8, "/a/b/c"u8),
        new("/foo/../bar/./..//baz"u8, "/baz"u8)
    }.slice()) {
        @string got = cleanPath(test.@in);
        if (got != test.want) {
            Ꮡt.Errorf("%s: got %q, want %q"u8, test.@in, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string indexˢ = "/index"u8;
internal static readonly @string homeˢ = "/home"u8;
internal static readonly @string aboutˢ = "/about"u8;
internal static readonly @string contactˢ = "/contact"u8;
internal static readonly @string robotsTxtˢ = "/robots.txt"u8;
internal static readonly @string productsˢ = "/products/"u8;
internal static readonly @string products1ˢ = "/products/1"u8;
internal static readonly @string products2ˢ = "/products/2"u8;
internal static readonly @string products3ˢ = "/products/3"u8;
internal static readonly @string products3ImageJpgˢ = "/products/3/image.jpg"u8;
internal static readonly @string adminˢ = "/admin"u8;
internal static readonly @string adminProductsˢ = "/admin/products/"u8;
internal static readonly @string adminProductsCreateˢ = "/admin/products/create"u8;
internal static readonly @string adminProductsUpdateˢ = "/admin/products/update"u8;
internal static readonly @string adminProductsDeleteˢ = "/admin/products/delete"u8;
internal static readonly object impossibleˢ = (@string)"impossible"u8;

public static void BenchmarkServerMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var fn = (global::go.net.http_package.ResponseWriter w, ж<global::go.net.http_package.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "OK"u8);
    };
    var mux = NewServeMux();
    mux.HandleFunc("/"u8, fn);
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
    var paths = new @string[]{"/"u8, "/notfound"u8, "/admin/"u8, "/admin/foo"u8, "/contact"u8, "/products"u8,
        "/products/"u8, "/products/3/image.jpg"u8}.slice();
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        var (r, err) = NewRequest(getˢ, "http://example.com/" + paths[i % builtin.len(paths)], default!);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        {
            var (h, p, _, _) = mux.findHandler(r); if (h != default! && p == ""u8) {
                Ꮡb.Error(impossibleˢ);
            }
        }
    }
    b.StopTimer();
}

} // end http_internal_test_package
