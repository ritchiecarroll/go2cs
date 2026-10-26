// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using io = io_package;
using Δhttp = global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.net;
using global::go.net.http;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// httptestNewRequest works around https://go.dev/issue/73151.
internal static ж<Δhttp.Request> httptestNewRequest(@string method, @string target) {
    var req = httptest.NewRequest(method, target, default!);
    req.Value.URL.Value.Scheme = ""u8;
    req.Value.URL.Value.Host = ""u8;
    return req;
}

internal static Δhttp.HandlerFunc okHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    w.WriteHeader(Δhttp.StatusOK);
});

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string secFetchSiteˢ = "Sec-Fetch-Site"u8;
internal static readonly @string originˢ = "Origin"u8;

internal partial struct TestCrossOriginProtectionSecFetchSite_tests /*dyn*/ {
    internal @string name;
    internal @string method;
    internal @string secFetchSite;
    internal @string origin;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionSecFetchSite(ж<testing.T> Ꮡt) {
    var protection = Δhttp.NewCrossOriginProtection();
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var tests = new TestCrossOriginProtectionSecFetchSite_tests[]{
        new("same-origin allowed"u8, "POST"u8, "same-origin"u8, ""u8, Δhttp.StatusOK),
        new("none allowed"u8, "POST"u8, "none"u8, ""u8, Δhttp.StatusOK),
        new("cross-site blocked"u8, "POST"u8, "cross-site"u8, ""u8, Δhttp.StatusForbidden),
        new("same-site blocked"u8, "POST"u8, "same-site"u8, ""u8, Δhttp.StatusForbidden),

        new("no header with no origin"u8, "POST"u8, ""u8, ""u8, Δhttp.StatusOK),
        new("no header with matching origin"u8, "POST"u8, ""u8, "https://example.com"u8, Δhttp.StatusOK),
        new("no header with mismatched origin"u8, "POST"u8, ""u8, "https://attacker.example"u8, Δhttp.StatusForbidden),
        new("no header with null origin"u8, "POST"u8, ""u8, "null"u8, Δhttp.StatusForbidden),

        new("GET allowed"u8, "GET"u8, "cross-site"u8, ""u8, Δhttp.StatusOK),
        new("HEAD allowed"u8, "HEAD"u8, "cross-site"u8, ""u8, Δhttp.StatusOK),
        new("OPTIONS allowed"u8, "OPTIONS"u8, "cross-site"u8, ""u8, Δhttp.StatusOK),
        new("PUT blocked"u8, "PUT"u8, "cross-site"u8, ""u8, Δhttp.StatusForbidden)
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestCrossOriginProtectionSecFetchSite_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var handlerʗ1 = handler;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var req = httptestNewRequest(tcʗ1.method, httpsExampleComˢ);
            if (tcʗ1.secFetchSite != ""u8) {
                (~req).Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
            }
            if (tcʗ1.origin != ""u8) {
                (~req).Header.Set(originˢ, tcʗ1.origin);
            }

            var w = httptest.NewRecorder();
            handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            if ((~w).Code != tcʗ1.expectedStatus) {
                tΔ1.Errorf("got status %d, want %d"u8, (~w).Code, tcʗ1.expectedStatus);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsTrustedExampleˢ = "https://trusted.example"u8;

internal partial struct TestCrossOriginProtectionTrustedOriginBypass_tests /*dyn*/ {
    internal @string name;
    internal @string origin;
    internal @string secFetchSite;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionTrustedOriginBypass(ж<testing.T> Ꮡt) {
    var protection = Δhttp.NewCrossOriginProtection();
    var err = protection.AddTrustedOrigin(httpsTrustedExampleˢ);
    if (err != default!) {
        Ꮡt.Fatalf("AddTrustedOrigin: %v"u8, err);
    }
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var tests = new TestCrossOriginProtectionTrustedOriginBypass_tests[]{
        new("trusted origin without sec-fetch-site"u8, "https://trusted.example"u8, ""u8, Δhttp.StatusOK),
        new("trusted origin with cross-site"u8, "https://trusted.example"u8, "cross-site"u8, Δhttp.StatusOK),
        new("untrusted origin without sec-fetch-site"u8, "https://attacker.example"u8, ""u8, Δhttp.StatusForbidden),
        new("untrusted origin with cross-site"u8, "https://attacker.example"u8, "cross-site"u8, Δhttp.StatusForbidden)
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestCrossOriginProtectionTrustedOriginBypass_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var handlerʗ1 = handler;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var req = httptestNewRequest(postˢ, httpsExampleComˢ);
            (~req).Header.Set(originˢ, tcʗ1.origin);
            if (tcʗ1.secFetchSite != ""u8) {
                (~req).Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
            }

            var w = httptest.NewRecorder();
            handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            if ((~w).Code != tcʗ1.expectedStatus) {
                tΔ1.Errorf("got status %d, want %d"u8, (~w).Code, tcʗ1.expectedStatus);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bypassˢ = "/bypass/"u8;
internal static readonly @string onlyFooˢ = "/only/{foo}"u8;
internal static readonly @string noTrailingˢ = "/no-trailing"u8;
internal static readonly @string yesTrailingˢ = "/yes-trailing/"u8;
internal static readonly @string putPutOnlyˢ = "PUT /put-only/"u8;
internal static readonly @string getGetOnlyˢ = "GET /get-only/"u8;
internal static readonly @string postPostOnlyˢ = "POST /post-only/"u8;
internal static readonly @string httpsAttackerExampleˢ = "https://attacker.example"u8;

internal partial struct TestCrossOriginProtectionPatternBypass_tests /*dyn*/ {
    internal @string name;
    internal @string path;
    internal @string secFetchSite;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionPatternBypass(ж<testing.T> Ꮡt) {
    var protection = Δhttp.NewCrossOriginProtection();
    protection.AddInsecureBypassPattern(bypassˢ);
    protection.AddInsecureBypassPattern(onlyFooˢ);
    protection.AddInsecureBypassPattern(noTrailingˢ);
    protection.AddInsecureBypassPattern(yesTrailingˢ);
    protection.AddInsecureBypassPattern(putPutOnlyˢ);
    protection.AddInsecureBypassPattern(getGetOnlyˢ);
    protection.AddInsecureBypassPattern(postPostOnlyˢ);
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var tests = new TestCrossOriginProtectionPatternBypass_tests[]{
        new("bypass path without sec-fetch-site"u8, "/bypass/"u8, ""u8, Δhttp.StatusOK),
        new("bypass path with cross-site"u8, "/bypass/"u8, "cross-site"u8, Δhttp.StatusOK),
        new("non-bypass path without sec-fetch-site"u8, "/api/"u8, ""u8, Δhttp.StatusForbidden),
        new("non-bypass path with cross-site"u8, "/api/"u8, "cross-site"u8, Δhttp.StatusForbidden),

        new("redirect to bypass path without .."u8, "/foo/../bypass/bar"u8, ""u8, Δhttp.StatusForbidden),
        new("redirect to bypass path with trailing slash"u8, "/bypass"u8, ""u8, Δhttp.StatusForbidden),
        new("redirect to non-bypass path with .."u8, "/foo/../api/bar"u8, ""u8, Δhttp.StatusForbidden),
        new("redirect to non-bypass path with trailing slash"u8, "/api"u8, ""u8, Δhttp.StatusForbidden),

        new("wildcard bypass"u8, "/only/123"u8, ""u8, Δhttp.StatusOK),
        new("non-wildcard"u8, "/only/123/foo"u8, ""u8, Δhttp.StatusForbidden),

        // https://go.dev/issue/75054
        new("no trailing slash exact match"u8, "/no-trailing"u8, ""u8, Δhttp.StatusOK),
        new("no trailing slash with slash"u8, "/no-trailing/"u8, ""u8, Δhttp.StatusForbidden),
        new("yes trailing slash exact match"u8, "/yes-trailing/"u8, ""u8, Δhttp.StatusOK),
        new("yes trailing slash without slash"u8, "/yes-trailing"u8, ""u8, Δhttp.StatusForbidden),

        new("method-specific hit"u8, "/post-only/"u8, ""u8, Δhttp.StatusOK),
        new("method-specific miss (PUT)"u8, "/put-only/"u8, ""u8, Δhttp.StatusForbidden),
        new("method-specific miss (GET)"u8, "/get-only/"u8, ""u8, Δhttp.StatusForbidden)
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestCrossOriginProtectionPatternBypass_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var handlerʗ1 = handler;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var req = httptestNewRequest(postˢ, "https://example.com"u8 + tcʗ1.path);
            (~req).Header.Set(originˢ, httpsAttackerExampleˢ);
            if (tcʗ1.secFetchSite != ""u8) {
                (~req).Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
            }

            var w = httptest.NewRecorder();
            handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            if ((~w).Code != tcʗ1.expectedStatus) {
                tΔ1.Errorf("got status %d, want %d"u8, (~w).Code, tcʗ1.expectedStatus);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string crossSiteˢ = "cross-site"u8;
internal static readonly @string customErrorˢ = "custom error"u8;

public static void TestCrossOriginProtectionSetDenyHandler(ж<testing.T> Ꮡt) {
    var protection = Δhttp.NewCrossOriginProtection();

    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var req = httptestNewRequest(postˢ, httpsExampleComˢ);
    (~req).Header.Set(secFetchSiteˢ, crossSiteˢ);

    var w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if ((~w).Code != Δhttp.StatusForbidden) {
        Ꮡt.Errorf("got status %d, want %d"u8, (~w).Code, (nint)(Δhttp.StatusForbidden));
    }

    var customErrHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter wΔ1, ж<Δhttp.Request> r) => {
        wΔ1.WriteHeader(Δhttp.StatusTeapot);
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(wΔ1), customErrorˢ);
    });
    protection.SetDenyHandler(new http_test_package.http_HandlerFuncᴠΔHandler(customErrHandler));

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if ((~w).Code != Δhttp.StatusTeapot) {
        Ꮡt.Errorf("got status %d, want %d"u8, (~w).Code, (nint)(Δhttp.StatusTeapot));
    }

    if (!strings.Contains((~w).Body.String(), customErrorˢ)) {
        Ꮡt.Errorf("expected custom error message, got: %q"u8, (~w).Body.String());
    }

    req = httptestNewRequest(getˢ2, httpsExampleComˢ);

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if ((~w).Code != Δhttp.StatusOK) {
        Ꮡt.Errorf("got status %d, want %d"u8, (~w).Code, (nint)(Δhttp.StatusOK));
    }

    protection.SetDenyHandler(default!);

    req = httptestNewRequest(postˢ, httpsExampleComˢ);
    (~req).Header.Set(secFetchSiteˢ, crossSiteˢ);

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if ((~w).Code != Δhttp.StatusForbidden) {
        Ꮡt.Errorf("got status %d, want %d"u8, (~w).Code, (nint)(Δhttp.StatusForbidden));
    }
}

internal partial struct TestCrossOriginProtectionAddTrustedOriginErrors_tests /*dyn*/ {
    internal @string name;
    internal @string origin;
    internal bool wantErr;
}

public static void TestCrossOriginProtectionAddTrustedOriginErrors(ж<testing.T> Ꮡt) {
    var protection = Δhttp.NewCrossOriginProtection();

    var tests = new TestCrossOriginProtectionAddTrustedOriginErrors_tests[]{
        new("valid origin"u8, "https://example.com"u8, false),
        new("valid origin with port"u8, "https://example.com:8080"u8, false),
        new("http origin"u8, "http://example.com"u8, false),
        new("missing scheme"u8, "example.com"u8, true),
        new("missing host"u8, "https://"u8, true),
        new("trailing slash"u8, "https://example.com/"u8, true),
        new("with path"u8, "https://example.com/path"u8, true),
        new("with query"u8, "https://example.com?query=value"u8, true),
        new("with fragment"u8, "https://example.com#fragment"u8, true),
        new("invalid url"u8, "https://ex ample.com"u8, true),
        new("empty string"u8, ""u8, true),
        new("null"u8, "null"u8, true)
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestCrossOriginProtectionAddTrustedOriginErrors_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var protectionʗ1 = protection;
        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var err = protectionʗ1.AddTrustedOrigin(tcʗ1.origin);
            if ((err != default!) != tcʗ1.wantErr) {
                tΔ1.Errorf("AddTrustedOrigin(%q) error = %v, wantErr %v"u8, tcʗ1.origin, err, tcʗ1.wantErr);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsConcurrentExampleˢ = "https://concurrent.example"u8;
internal static readonly @string fooˢ5 = "/foo/"u8;

public static partial void TestCrossOriginProtectionAddingBypassesConcurrently(ж<testing.T> Ꮡt) {
    var protection = Δhttp.NewCrossOriginProtection();
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var req = httptestNewRequest(postˢ, httpsExampleComˢ);
    (~req).Header.Set(originˢ, httpsConcurrentExampleˢ);
    (~req).Header.Set(secFetchSiteˢ, crossSiteˢ);

    var w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if ((~w).Code != Δhttp.StatusForbidden) {
        Ꮡt.Errorf("got status %d, want %d"u8, (~w).Code, (nint)(Δhttp.StatusForbidden));
    }

    var start = new channel<EmptyStruct>(0);
    var done = new channel<EmptyStruct>(0);
    var doneʗ1 = done;
    var handlerʗ1 = handler;
    var reqʗ1 = req;
    var startʗ1 = start;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            builtin.close(startʗ1);
            defer(ᴛ1 => builtin.close(ᴛ1), doneʗ1, ref ᒐ);
            foreach (var _ᴛ1 in range(10)) {
                var wΔ1 = httptest.NewRecorder();
                handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(wΔ1), reqʗ1);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    // Add bypasses while the requests are in flight.
    ᐸꟷ(start);
    protection.AddTrustedOrigin(httpsConcurrentExampleˢ);
    protection.AddInsecureBypassPattern(fooˢ5);
    ᐸꟷ(done);

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if ((~w).Code != Δhttp.StatusOK) {
        Ꮡt.Errorf("After concurrent bypass addition, got status %d, want %d"u8, (~w).Code, (nint)(Δhttp.StatusOK));
    }
}

internal partial struct TestCrossOriginProtectionServer_tests /*dyn*/ {
    internal @string name;
    internal @string method;
    internal @string url;
    internal @string origin;
    internal @string secFetchSite;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionServer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var protection = Δhttp.NewCrossOriginProtection();
        protection.AddTrustedOrigin(httpsTrustedExampleˢ);
        protection.AddInsecureBypassPattern(bypassˢ);
        var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

        var ts = httptest.NewServer(handler);
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var tests = new TestCrossOriginProtectionServer_tests[]{
            new("cross-site"u8, "POST"u8, (~ts).URL, "https://attacker.example"u8, "cross-site"u8, Δhttp.StatusForbidden),
            new("same-origin"u8, "POST"u8, (~ts).URL, ""u8, "same-origin"u8, Δhttp.StatusOK),
            new("origin matches host"u8, "POST"u8, (~ts).URL, (~ts).URL, ""u8, Δhttp.StatusOK),
            new("trusted origin"u8, "POST"u8, (~ts).URL, "https://trusted.example"u8, ""u8, Δhttp.StatusOK),
            new("untrusted origin"u8, "POST"u8, (~ts).URL, "https://attacker.example"u8, ""u8, Δhttp.StatusForbidden),
            new("bypass path"u8, "POST"u8, (~ts).URL + "/bypass/"u8, "https://attacker.example"u8, ""u8, Δhttp.StatusOK)
        }.slice();

        foreach (var (_, vᴛ1) in tests) {
            ref var tc = ref heap(new TestCrossOriginProtectionServer_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (req, err) = Δhttp.NewRequest(tcʗ1.method, tcʗ1.url, default!);
                    if (err != default!) {
                        tΔ1.Fatalf("NewRequest: %v"u8, err);
                    }
                    if (tcʗ1.origin != ""u8) {
                        (~req).Header.Set(originˢ, tcʗ1.origin);
                    }
                    if (tcʗ1.secFetchSite != ""u8) {
                        (~req).Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
                    }
                    var client = Ꮡ(new Δhttp.Client(nil));
                    (var resp, err) = client.Do(req);
                    if (err != default!) {
                        tΔ1.Fatalf("Do: %v"u8, err);
                    }
                    var respʗ1 = resp;
                    defer(() => (~respʗ1).Body.Close(), ref ᒐ);
                    if ((~resp).StatusCode != tcʗ1.expectedStatus) {
                        tΔ1.Errorf("got status %d, want %d"u8, (~resp).StatusCode, tcʗ1.expectedStatus);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
