// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using io = io_package;
using Δhttp = global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using strings = strings_package;
using testing = testing_package;
using global::go.net;
using global::go.net.http;
using static global::go.net.http_internal_test_package;

partial class http_test_package {

// httptestNewRequest works around https://go.dev/issue/73151.
internal static ж<Δhttp.Request> httptestNewRequest(@string method, @string target) {
    var req = httptest.NewRequest(method, target, default);
    req.URL.Scheme = ""u8;
    req.URL.Host = ""u8;
    return req;
}

internal static Δhttp.HandlerFunc okHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    w.WriteHeader(Δhttp.StatusOK);
});

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string secFetchSiteˢ => ref secFetchSiteˢᵇ;
private static readonly @string secFetchSiteˢᵇ = "Sec-Fetch-Site"u8;
internal static ref readonly @string originˢ => ref originˢᵇ;
private static readonly @string originˢᵇ = "Origin"u8;

internal partial struct TestCrossOriginProtectionSecFetchSite_tests /*dyn*/ {
    internal @string name;
    internal @string method;
    internal @string secFetchSite;
    internal @string origin;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionSecFetchSite(ж<testing.T> t) {
    var protection = Δhttp.NewCrossOriginProtection();
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var tests = slice<TestCrossOriginProtectionSecFetchSite_tests>([
        new("same-origin allowed", "POST", "same-origin", "", Δhttp.StatusOK),
        new("none allowed", "POST", "none", "", Δhttp.StatusOK),
        new("cross-site blocked", "POST", "cross-site", "", Δhttp.StatusForbidden),
        new("same-site blocked", "POST", "same-site", "", Δhttp.StatusForbidden),

        new("no header with no origin", "POST", "", "", Δhttp.StatusOK),
        new("no header with matching origin", "POST", "", "https://example.com", Δhttp.StatusOK),
        new("no header with mismatched origin", "POST", "", "https://attacker.example", Δhttp.StatusForbidden),
        new("no header with null origin", "POST", "", "null", Δhttp.StatusForbidden),

        new("GET allowed", "GET", "cross-site", "", Δhttp.StatusOK),
        new("HEAD allowed", "HEAD", "cross-site", "", Δhttp.StatusOK),
        new("OPTIONS allowed", "OPTIONS", "cross-site", "", Δhttp.StatusOK),
        new("PUT blocked", "PUT", "cross-site", "", Δhttp.StatusForbidden)
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestCrossOriginProtectionSecFetchSite_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var handlerʗ1 = handler;
        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var req = httptestNewRequest(tcʗ1.method, httpsExampleComˢ);
            if (tcʗ1.secFetchSite != ""u8) {
                req.Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
            }
            if (tcʗ1.origin != ""u8) {
                req.Header.Set(originˢ, tcʗ1.origin);
            }

            var w = httptest.NewRecorder();
            handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            if (w.Code != tcʗ1.expectedStatus) {
                tΔ1.Errorf("got status %d, want %d", w.Code, tcʗ1.expectedStatus);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsTrustedExampleˢ => ref httpsTrustedExampleˢᵇ;
private static readonly @string httpsTrustedExampleˢᵇ = "https://trusted.example"u8;

internal partial struct TestCrossOriginProtectionTrustedOriginBypass_tests /*dyn*/ {
    internal @string name;
    internal @string origin;
    internal @string secFetchSite;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionTrustedOriginBypass(ж<testing.T> t) {
    var protection = Δhttp.NewCrossOriginProtection();
    var err = protection.AddTrustedOrigin(httpsTrustedExampleˢ);
    if (err != nil) {
        t.Fatalf("AddTrustedOrigin: %v", err);
    }
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var tests = slice<TestCrossOriginProtectionTrustedOriginBypass_tests>([
        new("trusted origin without sec-fetch-site", "https://trusted.example", "", Δhttp.StatusOK),
        new("trusted origin with cross-site", "https://trusted.example", "cross-site", Δhttp.StatusOK),
        new("untrusted origin without sec-fetch-site", "https://attacker.example", "", Δhttp.StatusForbidden),
        new("untrusted origin with cross-site", "https://attacker.example", "cross-site", Δhttp.StatusForbidden)
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestCrossOriginProtectionTrustedOriginBypass_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var handlerʗ1 = handler;
        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var req = httptestNewRequest(postˢ, httpsExampleComˢ);
            req.Header.Set(originˢ, tcʗ1.origin);
            if (tcʗ1.secFetchSite != ""u8) {
                req.Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
            }

            var w = httptest.NewRecorder();
            handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            if (w.Code != tcʗ1.expectedStatus) {
                tΔ1.Errorf("got status %d, want %d", w.Code, tcʗ1.expectedStatus);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bypassˢ => ref bypassˢᵇ;
private static readonly @string bypassˢᵇ = "/bypass/"u8;
internal static ref readonly @string onlyFooˢ => ref onlyFooˢᵇ;
private static readonly @string onlyFooˢᵇ = "/only/{foo}"u8;
internal static ref readonly @string noTrailingˢ => ref noTrailingˢᵇ;
private static readonly @string noTrailingˢᵇ = "/no-trailing"u8;
internal static ref readonly @string yesTrailingˢ => ref yesTrailingˢᵇ;
private static readonly @string yesTrailingˢᵇ = "/yes-trailing/"u8;
internal static ref readonly @string putPutOnlyˢ => ref putPutOnlyˢᵇ;
private static readonly @string putPutOnlyˢᵇ = "PUT /put-only/"u8;
internal static ref readonly @string getGetOnlyˢ => ref getGetOnlyˢᵇ;
private static readonly @string getGetOnlyˢᵇ = "GET /get-only/"u8;
internal static ref readonly @string postPostOnlyˢ => ref postPostOnlyˢᵇ;
private static readonly @string postPostOnlyˢᵇ = "POST /post-only/"u8;
internal static ref readonly @string httpsAttackerExampleˢ => ref httpsAttackerExampleˢᵇ;
private static readonly @string httpsAttackerExampleˢᵇ = "https://attacker.example"u8;

internal partial struct TestCrossOriginProtectionPatternBypass_tests /*dyn*/ {
    internal @string name;
    internal @string path;
    internal @string secFetchSite;
    internal nint expectedStatus;
}

public static void TestCrossOriginProtectionPatternBypass(ж<testing.T> t) {
    var protection = Δhttp.NewCrossOriginProtection();
    protection.AddInsecureBypassPattern(bypassˢ);
    protection.AddInsecureBypassPattern(onlyFooˢ);
    protection.AddInsecureBypassPattern(noTrailingˢ);
    protection.AddInsecureBypassPattern(yesTrailingˢ);
    protection.AddInsecureBypassPattern(putPutOnlyˢ);
    protection.AddInsecureBypassPattern(getGetOnlyˢ);
    protection.AddInsecureBypassPattern(postPostOnlyˢ);
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var tests = slice<TestCrossOriginProtectionPatternBypass_tests>([
        new("bypass path without sec-fetch-site", "/bypass/", "", Δhttp.StatusOK),
        new("bypass path with cross-site", "/bypass/", "cross-site", Δhttp.StatusOK),
        new("non-bypass path without sec-fetch-site", "/api/", "", Δhttp.StatusForbidden),
        new("non-bypass path with cross-site", "/api/", "cross-site", Δhttp.StatusForbidden),

        new("redirect to bypass path without ..", "/foo/../bypass/bar", "", Δhttp.StatusForbidden),
        new("redirect to bypass path with trailing slash", "/bypass", "", Δhttp.StatusForbidden),
        new("redirect to non-bypass path with ..", "/foo/../api/bar", "", Δhttp.StatusForbidden),
        new("redirect to non-bypass path with trailing slash", "/api", "", Δhttp.StatusForbidden),

        new("wildcard bypass", "/only/123", "", Δhttp.StatusOK),
        new("non-wildcard", "/only/123/foo", "", Δhttp.StatusForbidden),

        // https://go.dev/issue/75054
        new("no trailing slash exact match", "/no-trailing", "", Δhttp.StatusOK),
        new("no trailing slash with slash", "/no-trailing/", "", Δhttp.StatusForbidden),
        new("yes trailing slash exact match", "/yes-trailing/", "", Δhttp.StatusOK),
        new("yes trailing slash without slash", "/yes-trailing", "", Δhttp.StatusForbidden),

        new("method-specific hit", "/post-only/", "", Δhttp.StatusOK),
        new("method-specific miss (PUT)", "/put-only/", "", Δhttp.StatusForbidden),
        new("method-specific miss (GET)", "/get-only/", "", Δhttp.StatusForbidden)
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestCrossOriginProtectionPatternBypass_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var handlerʗ1 = handler;
        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var req = httptestNewRequest(postˢ, "https://example.com"u8 + tcʗ1.path);
            req.Header.Set(originˢ, httpsAttackerExampleˢ);
            if (tcʗ1.secFetchSite != ""u8) {
                req.Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
            }

            var w = httptest.NewRecorder();
            handlerʗ1.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

            if (w.Code != tcʗ1.expectedStatus) {
                tΔ1.Errorf("got status %d, want %d", w.Code, tcʗ1.expectedStatus);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string crossSiteˢ => ref crossSiteˢᵇ;
private static readonly @string crossSiteˢᵇ = "cross-site"u8;
internal static ref readonly @string customErrorˢ => ref customErrorˢᵇ;
private static readonly @string customErrorˢᵇ = "custom error"u8;

public static void TestCrossOriginProtectionSetDenyHandler(ж<testing.T> t) {
    var protection = Δhttp.NewCrossOriginProtection();

    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var req = httptestNewRequest(postˢ, httpsExampleComˢ);
    req.Header.Set(secFetchSiteˢ, crossSiteˢ);

    var w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if (w.Code != Δhttp.StatusForbidden) {
        t.Errorf("got status %d, want %d", w.Code, (nint)(Δhttp.StatusForbidden));
    }

    var customErrHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter wΔ1, ж<Δhttp.Request> r) => {
        wΔ1.WriteHeader(Δhttp.StatusTeapot);
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(wΔ1), customErrorˢ);
    });
    protection.SetDenyHandler(new http_test_package.http_HandlerFuncᴠΔHandler(customErrHandler));

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if (w.Code != Δhttp.StatusTeapot) {
        t.Errorf("got status %d, want %d", w.Code, (nint)(Δhttp.StatusTeapot));
    }

    if (!strings.Contains(w.Body.String(), customErrorˢ)) {
        t.Errorf("expected custom error message, got: %q", w.Body.String());
    }

    req = httptestNewRequest(getˢ2, httpsExampleComˢ);

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if (w.Code != Δhttp.StatusOK) {
        t.Errorf("got status %d, want %d", w.Code, (nint)(Δhttp.StatusOK));
    }

    protection.SetDenyHandler(default);

    req = httptestNewRequest(postˢ, httpsExampleComˢ);
    req.Header.Set(secFetchSiteˢ, crossSiteˢ);

    w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if (w.Code != Δhttp.StatusForbidden) {
        t.Errorf("got status %d, want %d", w.Code, (nint)(Δhttp.StatusForbidden));
    }
}

internal partial struct TestCrossOriginProtectionAddTrustedOriginErrors_tests /*dyn*/ {
    internal @string name;
    internal @string origin;
    internal bool wantErr;
}

public static void TestCrossOriginProtectionAddTrustedOriginErrors(ж<testing.T> t) {
    var protection = Δhttp.NewCrossOriginProtection();

    var tests = slice<TestCrossOriginProtectionAddTrustedOriginErrors_tests>([
        new("valid origin", "https://example.com", false),
        new("valid origin with port", "https://example.com:8080", false),
        new("http origin", "http://example.com", false),
        new("missing scheme", "example.com", true),
        new("missing host", "https://", true),
        new("trailing slash", "https://example.com/", true),
        new("with path", "https://example.com/path", true),
        new("with query", "https://example.com?query=value", true),
        new("with fragment", "https://example.com#fragment", true),
        new("invalid url", "https://ex ample.com", true),
        new("empty string", "", true),
        new("null", "null", true)
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tc = ref heap(new TestCrossOriginProtectionAddTrustedOriginErrors_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var protectionʗ1 = protection;
        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var err = protectionʗ1.AddTrustedOrigin(tcʗ1.origin);
            if ((err != nil) != tcʗ1.wantErr) {
                tΔ1.Errorf("AddTrustedOrigin(%q) error = %v, wantErr %v", tcʗ1.origin, err, tcʗ1.wantErr);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsConcurrentExampleˢ => ref httpsConcurrentExampleˢᵇ;
private static readonly @string httpsConcurrentExampleˢᵇ = "https://concurrent.example"u8;
internal static ref readonly @string fooˢ5 => ref fooˢ5ᵇ;
private static readonly @string fooˢ5ᵇ = "/foo/"u8;

public static partial void TestCrossOriginProtectionAddingBypassesConcurrently(ж<testing.T> t) {
    var protection = Δhttp.NewCrossOriginProtection();
    var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

    var req = httptestNewRequest(postˢ, httpsExampleComˢ);
    req.Header.Set(originˢ, httpsConcurrentExampleˢ);
    req.Header.Set(secFetchSiteˢ, crossSiteˢ);

    var w = httptest.NewRecorder();
    handler.ServeHTTP(new http_test_package.httptest_ResponseRecorderжResponseWriter(w), req);

    if (w.Code != Δhttp.StatusForbidden) {
        t.Errorf("got status %d, want %d", w.Code, (nint)(Δhttp.StatusForbidden));
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

    if (w.Code != Δhttp.StatusOK) {
        t.Errorf("After concurrent bypass addition, got status %d, want %d", w.Code, (nint)(Δhttp.StatusOK));
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

public static void TestCrossOriginProtectionServer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var protection = Δhttp.NewCrossOriginProtection();
        protection.AddTrustedOrigin(httpsTrustedExampleˢ);
        protection.AddInsecureBypassPattern(bypassˢ);
        var handler = protection.Handler(new http_test_package.http_HandlerFuncᴠΔHandler(okHandler));

        var ts = httptest.NewServer(handler);
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);

        var tests = slice<TestCrossOriginProtectionServer_tests>([
            new("cross-site", "POST", ts.URL, "https://attacker.example", "cross-site", Δhttp.StatusForbidden),
            new("same-origin", "POST", ts.URL, "", "same-origin", Δhttp.StatusOK),
            new("origin matches host", "POST", ts.URL, ts.URL, "", Δhttp.StatusOK),
            new("trusted origin", "POST", ts.URL, "https://trusted.example", "", Δhttp.StatusOK),
            new("untrusted origin", "POST", ts.URL, "https://attacker.example", "", Δhttp.StatusForbidden),
            new("bypass path", "POST", ts.URL + "/bypass/"u8, "https://attacker.example", "", Δhttp.StatusOK)
        ]);

        foreach (var (_, vᴛ1) in range(tests)) {
            ref var tc = ref heap(new TestCrossOriginProtectionServer_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            t.Run(tc.name, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (req, err) = Δhttp.NewRequest(tcʗ1.method, tcʗ1.url, default);
                    if (err != nil) {
                        tΔ1.Fatalf("NewRequest: %v", err);
                    }
                    if (tcʗ1.origin != ""u8) {
                        req.Header.Set(originˢ, tcʗ1.origin);
                    }
                    if (tcʗ1.secFetchSite != ""u8) {
                        req.Header.Set(secFetchSiteˢ, tcʗ1.secFetchSite);
                    }
                    var client = Ꮡ(new Δhttp.Client());
                    (var resp, err) = client.Do(req);
                    if (err != nil) {
                        tΔ1.Fatalf("Do: %v", err);
                    }
                    var respʗ1 = resp;
                    defer(ᴛ0 => ᴛ0.Close(), respʗ1.Body, ref ᒐ);
                    if (resp.StatusCode != tcʗ1.expectedStatus) {
                        tΔ1.Errorf("got status %d, want %d", resp.StatusCode, tcʗ1.expectedStatus);
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
