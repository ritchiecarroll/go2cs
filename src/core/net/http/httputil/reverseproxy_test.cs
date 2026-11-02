// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Reverse proxy tests.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using net = net_package;
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using httptrace = go.net.http.httptrace_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.net;
using go.net.http;
using go.net.http.@internal;
using iter = iter_package;
using static go.net.http.httputil_package;

partial class httputil_internal_test_package {

internal static ref readonly @string fakeHopHeader => ref fakeHopHeaderᵇ;
private static readonly @string fakeHopHeaderᵇ = "X-Fake-Hop-Header-For-Test"u8;

[GoInit] internal static void init() {
    inOurTests = true;
    hopHeaders = append(hopHeaders, fakeHopHeader);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string modeˢ => ref modeˢᵇ;
private static readonly @string modeˢᵇ = "mode"u8;
internal static ref readonly @string proxyConnectionˢ => ref proxyConnectionˢᵇ;
private static readonly @string proxyConnectionˢᵇ = "Proxy-Connection"u8;
internal static ref readonly @string someNameˢ => ref someNameˢᵇ;
private static readonly @string someNameˢᵇ = "some-name"u8;
internal static ref readonly @string trailersˢ2 => ref trailersˢ2ᵇ;
private static readonly @string trailersˢ2ᵇ = "Trailers"u8;
internal static ref readonly @string notASpecialHeaderFieldˢ => ref notASpecialHeaderFieldˢᵇ;
private static readonly @string notASpecialHeaderFieldˢᵇ = "not a special header field name"u8;
internal static ref readonly @string xTrailerˢ => ref xTrailerˢᵇ;
private static readonly @string xTrailerˢᵇ = "X-Trailer"u8;
internal static ref readonly @string xFooˢ => ref xFooˢᵇ;
private static readonly @string xFooˢᵇ = "X-Foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string xMultiValueˢ => ref xMultiValueˢᵇ;
private static readonly @string xMultiValueˢᵇ = "X-Multi-Value"u8;
internal static ref readonly @string trailerValueˢ => ref trailerValueˢᵇ;
private static readonly @string trailerValueˢᵇ = "trailer_value"u8;
internal static ref readonly @string unannouncedTrailerValueˢ => ref unannouncedTrailerValueˢᵇ;
private static readonly @string unannouncedTrailerValueˢᵇ = "unannounced_trailer_value"u8;
internal static ref readonly @string closeTeˢ => ref closeTeˢᵇ;
private static readonly @string closeTeˢᵇ = "close, TE"u8;
internal static ref readonly @string barTrailersˢ => ref barTrailersˢᵇ;
private static readonly @string barTrailersˢᵇ = "bar, trailers"u8;
internal static ref readonly @string shouldBeDeletedˢ => ref shouldBeDeletedˢᵇ;
private static readonly @string shouldBeDeletedˢᵇ = "should be deleted"u8;
internal static ref readonly @string setCookieˢ => ref setCookieˢᵇ;
private static readonly @string setCookieˢᵇ = "Set-Cookie"u8;
internal static ref readonly @string xUnannouncedTrailerˢ => ref xUnannouncedTrailerˢᵇ;
private static readonly @string xUnannouncedTrailerˢᵇ = "X-Unannounced-Trailer"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string backendResponseᶜ => ref backendResponseᶜᵇ;
private static readonly @string backendResponseᶜᵇ = "I am the backend"u8;

public static void TestReverseProxy(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string backendResponse = backendResponseᶜ;
        const nint backendStatus = 404;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            if (r.Method == "GET"u8 && r.FormValue(modeˢ) == "hangup"u8) {
                var (c, _, _) = w._<http.Hijacker>().Hijack();
                c.Close();
                return;
            }
            if (len(r.TransferEncoding) > 0) {
                t.Errorf("backend got unexpected TransferEncoding: %v", r.TransferEncoding);
            }
            if (r.Header.Get(xForwardedForˢ) == ""u8) {
                t.Errorf("didn't get X-Forwarded-For header");
            }
            {
                @string c = r.Header.Get(connectionˢ); if (c != ""u8) {
                    t.Errorf("handler got Connection header value %q", c);
                }
            }
            {
                @string c = r.Header.Get("Te"); if (c != "trailers"u8) {
                    t.Errorf("handler got Te header value %q; want 'trailers'", c);
                }
            }
            {
                @string c = r.Header.Get(upgradeˢ); if (c != ""u8) {
                    t.Errorf("handler got Upgrade header value %q", c);
                }
            }
            {
                @string c = r.Header.Get(proxyConnectionˢ); if (c != ""u8) {
                    t.Errorf("handler got Proxy-Connection header value %q", c);
                }
            }
            {
                @string g = r.Host;
                @string e = someNameˢ; if (g != e) {
                    t.Errorf("backend got Host header %q, want %q", g, e);
                }
            }
            w.Header().Set(trailersˢ2, notASpecialHeaderFieldˢ);
            w.Header().Set(trailerˢ, xTrailerˢ);
            w.Header().Set(xFooˢ, barˢ);
            w.Header().Set(upgradeˢ, fooˢ);
            w.Header().Set(fakeHopHeader, fooˢ);
            w.Header().Add(xMultiValueˢ, fooˢ);
            w.Header().Add(xMultiValueˢ, barˢ);
            http.SetCookie(w, Ꮡ(new httpꓸCookie(Name: "flavor", Value: "chocolateChip")));
            w.WriteHeader(backendStatus);
            w.Write(slice<byte>(backendResponse));
            w.Header().Set(xTrailerˢ, trailerValueˢ);
            w.Header().Set(http.TrailerPrefix + "X-Unannounced-Trailer", unannouncedTrailerValueˢ);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        getReq.Host = someNameˢ;
        getReq.Header.Set(connectionˢ, closeTeˢ);
        getReq.Header.Add("Te", fooˢ);
        getReq.Header.Add("Te", barTrailersˢ);
        getReq.Header.Set(proxyConnectionˢ, shouldBeDeletedˢ);
        getReq.Header.Set(upgradeˢ, fooˢ);
        getReq.Close = true;
        (var res, err) = frontendClient.Do(getReq);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        {
            nint g = res.StatusCode;
            nint e = backendStatus; if (g != e) {
                t.Errorf("got res.StatusCode %d; expected %d", g, e);
            }
        }
        {
            @string g = res.Header.Get(xFooˢ);
            @string e = barˢ; if (g != e) {
                t.Errorf("got X-Foo %q; expected %q", g, e);
            }
        }
        {
            @string c = res.Header.Get(fakeHopHeader); if (c != ""u8) {
                t.Errorf("got %s header value %q", fakeHopHeader, c);
            }
        }
        {
            @string g = res.Header.Get(trailersˢ2);
            @string e = notASpecialHeaderFieldˢ; if (g != e) {
                t.Errorf("header Trailers = %q; want %q", g, e);
            }
        }
        {
            nint g = len(res.Header[xMultiValueˢ]);
            nint e = 2; if (g != e) {
                t.Errorf("got %d X-Multi-Value header values; expected %d", g, e);
            }
        }
        {
            nint g = len(res.Header[setCookieˢ]);
            nint e = 1; if (g != e) {
                t.Fatalf("got %d SetCookies, want %d", g, e);
            }
        }
        {
            var (g, e) = (res.Trailer, (new httpꓸHeader(new map<@string, slice<@string>>{["X-Trailer"] = default}))); if (!reflect.DeepEqual(g, e)) {
                t.Errorf("before reading body, Trailer = %#v; want %#v", g, e);
            }
        }
        {
            var cookie = res.Cookies()[0]; if (cookie.Name != "flavor"u8) {
                t.Errorf("unexpected cookie %q", cookie.Name);
            }
        }
        var (bodyBytes, _) = io.ReadAll(res.Body);
        {
            @string g = (@string)bodyBytes;
            @string e = backendResponse; if (g != e) {
                t.Errorf("got body %q; expected %q", g, e);
            }
        }
        {
            @string g = res.Trailer.Get(xTrailerˢ);
            @string e = trailerValueˢ; if (g != e) {
                t.Errorf("Trailer(X-Trailer) = %q ; want %q", g, e);
            }
        }
        {
            @string g = res.Trailer.Get(xUnannouncedTrailerˢ);
            @string e = unannouncedTrailerValueˢ; if (g != e) {
                t.Errorf("Trailer(X-Unannounced-Trailer) = %q ; want %q", g, e);
            }
        }
        res.Body.Close();

        // Test that a backend failing to be reached or one which doesn't return
        // a response results in a StatusBadGateway.
        (getReq, _) = http.NewRequest(getˢ, frontend.URL + "/?mode=hangup"u8, default);
        getReq.Close = true;
        (res, err) = frontendClient.Do(getReq);
        if (err != nil) {
            t.Fatal(err);
        }
        res.Body.Close();
        if (res.StatusCode != http.StatusBadGateway) {
            t.Errorf("request to bad proxy = %v; want 502 StatusBadGateway", res.Status);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fakeConnectionTokenᶜ => ref fakeConnectionTokenᶜᵇ;
private static readonly @string fakeConnectionTokenᶜᵇ = "X-Fake-Connection-Token"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string backendResponseᶜ1 => ref backendResponseᶜ1ᵇ;
private static readonly @string backendResponseᶜ1ᵇ = "I am the backend"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string someConnHeaderᶜ => ref someConnHeaderᶜᵇ;
private static readonly @string someConnHeaderᶜᵇ = "X-Some-Conn-Header"u8;

// Issue 16875: remove any proxied headers mentioned in the "Connection"
// header value.
public static void TestReverseProxyStripHeadersPresentInConnection(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string fakeConnectionToken = fakeConnectionTokenᶜ;
        @string backendResponse = backendResponseᶜ1;

        // someConnHeader is some arbitrary header to be declared as a hop-by-hop header
        // in the Request's Connection header.
        @string someConnHeader = someConnHeaderᶜ;

        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            {
                @string c = r.Header.Get(connectionˢ); if (c != ""u8) {
                    t.Errorf("handler got header %q = %q; want empty", connectionˢ, c);
                }
            }
            {
                @string c = r.Header.Get(fakeConnectionToken); if (c != ""u8) {
                    t.Errorf("handler got header %q = %q; want empty", fakeConnectionToken, c);
                }
            }
            {
                @string c = r.Header.Get(someConnHeader); if (c != ""u8) {
                    t.Errorf("handler got header %q = %q; want empty", someConnHeader, c);
                }
            }
            w.Header().Add(connectionˢ, "Upgrade, " + fakeConnectionToken);
            w.Header().Add(connectionˢ, someConnHeader);
            w.Header().Set(someConnHeader, shouldBeDeletedˢ);
            w.Header().Set(fakeConnectionToken, shouldBeDeletedˢ);
            io.WriteString(new httputil_test_package.http_ResponseWriterᴠWriter(w), backendResponse);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var proxyHandlerʗ1 = proxyHandler;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            proxyHandlerʗ1.ServeHTTP(w, r);
            {
                @string cΔ1 = r.Header.Get(someConnHeader); if (cΔ1 != "should be deleted"u8) {
                    t.Errorf("handler modified header %q = %q; want %q", someConnHeader, cΔ1, shouldBeDeletedˢ);
                }
            }
            {
                @string cΔ2 = r.Header.Get(fakeConnectionToken); if (cΔ2 != "should be deleted"u8) {
                    t.Errorf("handler modified header %q = %q; want %q", fakeConnectionToken, cΔ2, shouldBeDeletedˢ);
                }
            }
            var c = r.Header[connectionˢ];
            slice<@string> cf = default;
            foreach (var (_, f) in c) {
                foreach (var iᴛ1 in range<@string>(strings.SplitSeq(f, ",").Invoke)) {
                    var sf = iᴛ1;
                    {
                        sf = strings.TrimSpace(sf); if (sf != ""u8) {
                            cf = append(cf, sf);
                        }
                    }
                }
            }
            slices.Sort<slice<@string>, @string>(cf);
            var expectedValues = slice<@string>(["Upgrade", someConnHeader, fakeConnectionToken]);
            slices.Sort<slice<@string>, @string>(expectedValues);
            if (!slices.Equal<slice<@string>, @string>(cf, expectedValues)) {
                t.Errorf("handler modified header %q = %q; want %q", connectionˢ, cf, expectedValues);
            }
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        getReq.Header.Add(connectionˢ, "Upgrade, " + fakeConnectionToken);
        getReq.Header.Add(connectionˢ, someConnHeader);
        getReq.Header.Set(someConnHeader, shouldBeDeletedˢ);
        getReq.Header.Set(fakeConnectionToken, shouldBeDeletedˢ);
        (var res, err) = frontend.Client().Do(getReq);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var bodyBytes, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatalf("reading body: %v", err);
        }
        {
            @string got = (@string)bodyBytes;
            @string want = backendResponse; if (got != want) {
                t.Errorf("got body %q; want %q", got, want);
            }
        }
        {
            @string c = res.Header.Get(connectionˢ); if (c != ""u8) {
                t.Errorf("handler got header %q = %q; want empty", connectionˢ, c);
            }
        }
        {
            @string c = res.Header.Get(someConnHeader); if (c != ""u8) {
                t.Errorf("handler got header %q = %q; want empty", someConnHeader, c);
            }
        }
        {
            @string c = res.Header.Get(fakeConnectionToken); if (c != ""u8) {
                t.Errorf("handler got header %q = %q; want empty", fakeConnectionToken, c);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string backendResponseᶜ2 => ref backendResponseᶜ2ᵇ;
private static readonly @string backendResponseᶜ2ᵇ = "I am the backend"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string someConnHeaderᶜ1 => ref someConnHeaderᶜ1ᵇ;
private static readonly @string someConnHeaderᶜ1ᵇ = "X-Some-Conn-Header"u8;

public static void TestReverseProxyStripEmptyConnection(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // See Issue 46313.
        @string backendResponse = backendResponseᶜ2;

        // someConnHeader is some arbitrary header to be declared as a hop-by-hop header
        // in the Request's Connection header.
        @string someConnHeader = someConnHeaderᶜ1;

        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            {
                var c = r.Header.Values(connectionˢ); if (len(c) != 0) {
                    t.Errorf("handler got header %q = %v; want empty", connectionˢ, c);
                }
            }
            {
                @string c = r.Header.Get(someConnHeader); if (c != ""u8) {
                    t.Errorf("handler got header %q = %q; want empty", someConnHeader, c);
                }
            }
            w.Header().Add(connectionˢ, ""u8);
            w.Header().Add(connectionˢ, someConnHeader);
            w.Header().Set(someConnHeader, shouldBeDeletedˢ);
            io.WriteString(new httputil_test_package.http_ResponseWriterᴠWriter(w), backendResponse);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var proxyHandlerʗ1 = proxyHandler;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            proxyHandlerʗ1.ServeHTTP(w, r);
            {
                @string c = r.Header.Get(someConnHeader); if (c != "should be deleted"u8) {
                    t.Errorf("handler modified header %q = %q; want %q", someConnHeader, c, shouldBeDeletedˢ);
                }
            }
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        getReq.Header.Add(connectionˢ, ""u8);
        getReq.Header.Add(connectionˢ, someConnHeader);
        getReq.Header.Set(someConnHeader, shouldBeDeletedˢ);
        (var res, err) = frontend.Client().Do(getReq);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var bodyBytes, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatalf("reading body: %v", err);
        }
        {
            @string got = (@string)bodyBytes;
            @string want = backendResponse; if (got != want) {
                t.Errorf("got body %q; want %q", got, want);
            }
        }
        {
            @string c = res.Header.Get(connectionˢ); if (c != ""u8) {
                t.Errorf("handler got header %q = %q; want empty", connectionˢ, c);
            }
        }
        {
            @string c = res.Header.Get(someConnHeader); if (c != ""u8) {
                t.Errorf("handler got header %q = %q; want empty", someConnHeader, c);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string closeˢ => ref closeˢᵇ;
private static readonly @string closeˢᵇ = "close"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prevForwardedForᶜ => ref prevForwardedForᶜᵇ;
private static readonly @string prevForwardedForᶜᵇ = "client ip"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string backendResponseᶜ3 => ref backendResponseᶜ3ᵇ;
private static readonly @string backendResponseᶜ3ᵇ = "I am the backend"u8;

public static void TestXForwardedFor(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string prevForwardedFor = prevForwardedForᶜ;
        @string backendResponse = backendResponseᶜ3;
        const nint backendStatus = 404;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            if (r.Header.Get(xForwardedForˢ) == ""u8) {
                t.Errorf("didn't get X-Forwarded-For header");
            }
            if (!strings.Contains(r.Header.Get(xForwardedForˢ), prevForwardedFor)) {
                t.Errorf("X-Forwarded-For didn't contain prior data");
            }
            w.WriteHeader(backendStatus);
            w.Write(slice<byte>(backendResponse));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        getReq.Header.Set(connectionˢ, closeˢ);
        getReq.Header.Set(xForwardedForˢ, prevForwardedFor);
        getReq.Close = true;
        (var res, err) = frontend.Client().Do(getReq);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        {
            nint g = res.StatusCode;
            nint e = backendStatus; if (g != e) {
                t.Errorf("got res.StatusCode %d; expected %d", g, e);
            }
        }
        var (bodyBytes, _) = io.ReadAll(res.Body);
        {
            @string g = (@string)bodyBytes;
            @string e = backendResponse; if (g != e) {
                t.Errorf("got body %q; expected %q", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 38079: don't append to X-Forwarded-For if it's present but nil
public static void TestXForwardedFor_Omit(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            {
                @string v = r.Header.Get(xForwardedForˢ); if (v != ""u8) {
                    t.Errorf("got X-Forwarded-For header: %q", v);
                }
            }
            w.Write(slice<byte>("hi"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var oldDirector = proxyHandler.Director;
        var oldDirectorʗ1 = oldDirector;
        proxyHandler.Director = (ж<http.Request> r) => {
            r.Header[xForwardedForˢ] = default;
            oldDirectorʗ1(r);
        };

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        getReq.Host = someNameˢ;
        getReq.Close = true;
        (var res, err) = frontend.Client().Do(getReq);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        res.Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReverseProxyRewriteStripsForwarded(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var headers = slice<@string>([
            "Forwarded",
            "X-Forwarded-For",
            "X-Forwarded-Host",
            "X-Forwarded-Proto"
        ]);
        var headersʗ1 = headers;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            foreach (var (_, h) in headersʗ1) {
                {
                    @string v = r.Header.Get(h); if (v != ""u8) {
                        t.Errorf("got %v header: %q", h, v);
                    }
                }
            }
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
            var backendURLʗ1 = backendURL;
        var proxyHandler = Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(backendURLʗ1);
            }
        ));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        getReq.Host = someNameˢ;
        getReq.Close = true;
        foreach (var (_, h) in headers) {
            getReq.Header.Set(h, "x");
        }
        (var res, err) = frontend.Client().Do(getReq);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        res.Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct proxyQueryTestsᴛ1 /*dyn*/ {
    internal @string baseSuffix; // suffix to add to backend URL
    internal @string reqSuffix;  // suffix to add to frontend's request URL
    internal @string want;       // what backend should see for final request URL (without ?)
}
internal static slice<proxyQueryTestsᴛ1> proxyQueryTests = slice<proxyQueryTestsᴛ1>([
    new("", "", ""),
    new("?sta=tic", "?us=er", "sta=tic&us=er"),
    new("", "?us=er", "us=er"),
    new("?sta=tic", "", "sta=tic")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xGotQueryˢ => ref xGotQueryˢᵇ;
private static readonly @string xGotQueryˢᵇ = "X-Got-Query"u8;

public static void TestReverseProxyQuery(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Set(xGotQueryˢ, r.URL.RawQuery);
            w.Write(slice<byte>("hi"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);

        foreach (var (i, tt) in proxyQueryTests) {
            var (backendURL, err) = url.Parse(backend.URL + tt.baseSuffix);
            if (err != nil) {
                t.Fatal(err);
            }
            var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(NewSingleHostReverseProxy(backendURL)));
            var (req, _) = http.NewRequest(getˢ, frontend.URL + tt.reqSuffix, default);
            req.Close = true;
            (var res, err) = frontend.Client().Do(req);
            if (err != nil) {
                t.Fatalf("%d. Get: %v", i, err);
            }
            {
                @string g = res.Header.Get(xGotQueryˢ);
                @string e = tt.want; if (g != e) {
                    t.Errorf("%d. got query %q; expected %q", i, g, e);
                }
            }
            res.Body.Close();
            frontend.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedᶜ => ref expectedᶜᵇ;
private static readonly @string expectedᶜᵇ = "hi"u8;

public static void TestReverseProxyFlushInterval(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string expected = expectedᶜ;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(expected));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);

        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.FlushInterval = time.Microsecond;

        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(getˢ, frontend.URL, default);
        req.Close = true;
        (var res, err) = frontend.Client().Do(req);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        {
            var (bodyBytes, _) = io.ReadAll(res.Body); if (((sstring)bodyBytes) != expected) {
                t.Errorf("got body %q; expected %q", bodyBytes, expected);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct mockFlusher {
    /*embed*/ public go.net.http_package.ResponseWriter ResponseWriter;
    internal bool flushed;
}

internal static void Flush(this ref mockFlusher m) {
    m.flushed = true;
}

internal partial struct wrappedRW {
    /*embed*/ public go.net.http_package.ResponseWriter ResponseWriter;
}

internal static http.ResponseWriter Unwrap(this ref wrappedRW w) {
    return w.ResponseWriter;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedᶜ1 => ref expectedᶜ1ᵇ;
private static readonly @string expectedᶜ1ᵇ = "hi"u8;

public static void TestReverseProxyResponseControllerFlushInterval(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string expected = expectedᶜ1;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(expected));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);

        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var mf = Ꮡ(new mockFlusher());
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.FlushInterval = -1; // flush immediately
        var mfʗ1 = mf;
        var proxyHandlerʗ1 = proxyHandler;
        var proxyWithMiddleware = new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            mfʗ1.ResponseWriter = w;
            w = new httputil_internal_test_package.wrappedRWжResponseWriter(Ꮡ(new wrappedRW(new httputil_internal_test_package.mockFlusherжResponseWriter(mfʗ1))));
            proxyHandlerʗ1.ServeHTTP(w, r);
        });

        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(proxyWithMiddleware));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(getˢ, frontend.URL, default);
        req.Close = true;
        (var res, err) = frontend.Client().Do(req);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        {
            var (bodyBytes, _) = io.ReadAll(res.Body); if (((sstring)bodyBytes) != expected) {
                t.Errorf("got body %q; expected %q", bodyBytes, expected);
            }
        }
        if (!mf.flushed) {
            t.Errorf("response writer was not flushed");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myHeaderˢ => ref myHeaderˢᵇ;
private static readonly @string myHeaderˢᵇ = "MyHeader"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedᶜ2 => ref expectedᶜ2ᵇ;
private static readonly @string expectedᶜ2ᵇ = "hi"u8;

public static void TestReverseProxyFlushIntervalHeaders(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string expected = expectedᶜ2;
        var stopCh = new channel<EmptyStruct>(0);
        var stopChʗ1 = stopCh;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Add(myHeaderˢ, expected);
            w.WriteHeader(200);
            w._<http.Flusher>().Flush();
            ᐸꟷ(stopChʗ1);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        defer(ᴛ1 => close(ᴛ1), stopCh, ref ᒐ);

        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.FlushInterval = time.Microsecond;

        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(getˢ, frontend.URL, default);
        req.Close = true;

        var (ctx, cancel) = context.WithTimeout(req.Context(), (time.Duration)(10000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        req = req.WithContext(ctx);

        (var res, err) = frontend.Client().Do(req);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        if (res.Header.Get(myHeaderˢ) != expected) {
            t.Errorf("got header %q; expected %q", res.Header.Get(myHeaderˢ), expected);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object handlerNeverSawˢ = (@string)"Handler never saw CloseNotify"u8;
internal static readonly object serverClientDoReturnedˢ = (@string)"Server.Client().Do() returned nil error; want non-nil error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string backendResponseᶜ4 => ref backendResponseᶜ4ᵇ;
private static readonly @string backendResponseᶜ4ᵇ = "I am the backend"u8;

public static partial void TestReverseProxyCancellation(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string backendResponse = backendResponseᶜ4;

        var reqInFlight = new channel<EmptyStruct>(0);
        var reqInFlightʗ1 = reqInFlight;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            close(reqInFlightʗ1); // cause the client to cancel its request

            var selᴛ3 = time.After((time.Duration)(10000000000L));
            var selᴛ4 = w._<http.CloseNotifier>().CloseNotify();
            switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                // Note: this should only happen in broken implementations, and the
                // closenotify case should be instantaneous.
                t.Error(handlerNeverSawˢ);
                return;
            }
            case 1 when selᴛ4.ꟷᐳ(out _): {
                break;
            }}

            w.WriteHeader(http.StatusOK);
            w.Write(slice<byte>(backendResponse));
        })));

        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);

        backend.Config.ErrorLog = log.New(io.Discard, ""u8, 0);

        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var proxyHandler = NewSingleHostReverseProxy(backendURL);

        // Discards errors of the form:
        // http: proxy error: read tcp 127.0.0.1:44643: use of closed network connection
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0);

        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
        var frontendClientʗ1 = frontendClient;
        var getReqʗ1 = getReq;
        var reqInFlightʗ2 = reqInFlight;
        goǃ(() => {
            ᐸꟷ(reqInFlightʗ2);
            frontendClientʗ1.Transport._<ж<http.Transport>>().CancelRequest(getReqʗ1);
        });
        (var res, err) = frontendClient.Do(getReq);
        if (res != nil) {
            t.Errorf("got response %v; want nil", res.Status);
        }
        if (err == nil) {
            // This should be an error like:
            // Get "http://127.0.0.1:58079": read tcp 127.0.0.1:58079:
            //    use of closed network connection
            t.Error(serverClientDoReturnedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<http.Request> req(ж<testing.T> t, @string v) {
    var (req, err) = http.ReadRequest(bufio.NewReader(new httputil_test_package.strings_ReaderжReader(strings.NewReader(v))));
    if (err != nil) {
        t.Fatal(err);
    }
    return req;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getHttp10ˢ => ref getHttp10ˢᵇ;
private static readonly @string getHttp10ˢᵇ = "GET / HTTP/1.0\r\n\r\n"u8;

// Issue 12344
public static void TestNilBody(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("hi"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);

        var backendʗ2 = backend;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> _Δp1) => {
            var (backURL, _) = url.Parse(backendʗ2.URL);
            var rp = NewSingleHostReverseProxy(backURL);
            var r = req(t, getHttp10ˢ);
            r.Body = default; // this accidentally worked in Go 1.4 and below, so keep it working
            rp.ServeHTTP(w, r);
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (res, err) = http.Get(frontend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var slurp, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(err);
        }
        if (((sstring)slurp) != "hi"u8) {
            t.Errorf("Got %q; want %q", slurp, (@string)"hi");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 15524
public static void TestUserAgentHeader(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string gotUA = default;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            gotUA = r.Header.Get(userAgentˢ);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var proxyHandler = @new<global::go.net.http.httputil_package.ReverseProxy>();
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var backendURLʗ1 = backendURL;
        proxyHandler.Director = (ж<http.Request> req) => {
            req.URL = backendURLʗ1;
        };
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        foreach (var (_, sentUA) in slice<@string>(["explicit UA", ""])) {
            var (getReq, _) = http.NewRequest(getˢ, frontend.URL, default);
            getReq.Header.Set(userAgentˢ, sentUA);
            getReq.Close = true;
            var (res, errΔ1) = frontendClient.Do(getReq);
            if (errΔ1 != nil) {
                t.Fatalf("Get: %v", errΔ1);
            }
            res.Body.Close();
            {
                @string got = gotUA;
                @string want = sentUA; if (got != want) {
                    t.Errorf("got forwarded User-Agent %q, want %q", got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct bufferPool {
    internal Func<slice<byte>> get;
    internal Action<slice<byte>> put;
}

internal static slice<byte> Get(this bufferPool bp) {
    return bp.get();
}

internal static void Put(this bufferPool bp, slice<byte> v) {
    bp.put(v);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getBufˢ => ref getBufˢᵇ;
private static readonly @string getBufˢᵇ = "getBuf"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string msgᶜ => ref msgᶜᵇ;
private static readonly @string msgᶜᵇ = "hi"u8;

public static void TestReverseProxyGetPutBuffer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string msg = msgᶜ;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            io.WriteString(new httputil_test_package.http_ResponseWriterᴠWriter(w), msg);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);

        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
        ref var log = ref heap<slice<@string>>(out var Ꮡlog);
        void addLog(@string @event) {
            GoFrame ᒐ = default;
            try {
                Ꮡmu.Lock();
                defer(Ꮡmu.Unlock, ref ᒐ);
                Ꮡlog.ValueSlot = append(Ꮡlog.ValueSlot, @event);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        var rp = NewSingleHostReverseProxy(backendURL);
        const nint size = 1234;
            var addLogʗ1 = addLog;

            var addLogʗ2 = addLog;
        rp.BufferPool = new bufferPool(
            get: () => {
                addLogʗ1(getBufˢ);
                return new slice<byte>(size);
            },
            put: (slice<byte> p) => {
                addLogʗ2("putBuf-"u8 + strconv.Itoa(len(p)));
            }
        );
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(rp));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(getˢ, frontend.URL, default);
        req.Close = true;
        (var res, err) = frontend.Client().Do(req);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        (var slurp, err) = io.ReadAll(res.Body);
        res.Body.Close();
        if (err != nil) {
            t.Fatalf("reading body: %v", err);
        }
        if (((sstring)slurp) != msg) {
            t.Errorf("msg = %q; want %q", slurp, msg);
        }
        var wantLog = slice<@string>(["getBuf", "putBuf-"u8 + strconv.Itoa(size)]);
        Ꮡmu.Lock();
        defer(Ꮡmu.Unlock, ref ᒐ);
        if (!slices.Equal<slice<@string>, @string>(log, wantLog)) {
            t.Errorf("Log events = %q; want %q", log, wantLog);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object backendReadWrongRequestˢ = (@string)"Backend read wrong request body."u8;
internal static ref readonly @string postˢ => ref postˢᵇ;
private static readonly @string postˢᵇ = "POST"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string backendResponseᶜ5 => ref backendResponseᶜ5ᵇ;
private static readonly @string backendResponseᶜ5ᵇ = "I am the backend"u8;

public static void TestReverseProxy_Post(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string backendResponse = backendResponseᶜ5;
        const nint backendStatus = 200;
        slice<byte> requestBody = bytes.Repeat(slice<byte>("a"u8), 1 << 20);
        var requestBodyʗ1 = requestBody;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            var (slurp, errΔ1) = io.ReadAll(r.Body);
            if (errΔ1 != nil) {
                t.Errorf("Backend body read = %v", errΔ1);
            }
            if (len(slurp) != len(requestBodyʗ1)) {
                t.Errorf("Backend read %d request body bytes; want %d", len(slurp), len(requestBodyʗ1));
            }
            if (!bytes.Equal(slurp, requestBodyʗ1)) {
                t.Error(backendReadWrongRequestˢ); // 1MB; omitting details
            }
            w.Write(slice<byte>(backendResponse));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (postReq, _) = http.NewRequest(postˢ, frontend.URL, new httputil_test_package.bytes_ReaderжReader(bytes.NewReader(requestBody)));
        (var res, err) = frontend.Client().Do(postReq);
        if (err != nil) {
            t.Fatalf("Do: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        {
            nint g = res.StatusCode;
            nint e = backendStatus; if (g != e) {
                t.Errorf("got res.StatusCode %d; expected %d", g, e);
            }
        }
        var (bodyBytes, _) = io.ReadAll(res.Body);
        {
            @string g = (@string)bodyBytes;
            @string e = backendResponse; if (g != e) {
                t.Errorf("got body %q; expected %q", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public delegate (ж<http.Response>, error) RoundTripperFunc(ж<http.Request> _Δp0);

public static partial (ж<http.Response>, error) RoundTrip(this RoundTripperFunc fn, ж<http.Request> req) {
    return fn(req);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFakeTldˢ => ref httpFakeTldˢᵇ;
private static readonly @string httpFakeTldˢᵇ = "http://fake.tld/"u8;
internal static readonly object bodyNilWantANilBodyˢ = (@string)"Body != nil; want a nil Body"u8;
internal static ref readonly @string doneTestingTheˢ => ref doneTestingTheˢᵇ;
private static readonly @string doneTestingTheˢᵇ = "done testing the interesting part; so force a 502 Gateway error"u8;

// Issue 16036: send a Request with a nil Body when possible
public static void TestReverseProxy_NilBody(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (backendURL, _) = url.Parse(httpFakeTldˢ);
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        proxyHandler.Transport = new httputil_internal_test_package.RoundTripperFuncᴠRoundTripper(new RoundTripperFunc((ж<http.Request> req) => {
            if (req.Body != nil) {
                t.Error(bodyNilWantANilBodyˢ);
            }
            return (default, errors.New(doneTestingTheˢ));
        }));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (res, err) = frontend.Client().Get(frontend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.StatusCode != 502) {
            t.Errorf("status code = %v; want 502 (Gateway Error)", res.Status);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object headerNilWantANonNilˢ = (@string)"Header == nil; want a non-nil Header"u8;

// Issue 33142: always allocate the request headers
public static void TestReverseProxy_AllocatedHeader(ж<testing.T> t) {
    var proxyHandler = @new<global::go.net.http.httputil_package.ReverseProxy>();
    proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
    proxyHandler.Director = (ж<http.Request> _) => {
    };                                                    // noop
    proxyHandler.Transport = new httputil_internal_test_package.RoundTripperFuncᴠRoundTripper(new RoundTripperFunc((ж<http.Request> req) => {
        if (req.Header == nil) {
            t.Error(headerNilWantANonNilˢ);
        }
        return (default, errors.New(doneTestingTheˢ));
    }));

    proxyHandler.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), Ꮡ(new http.Request(
        Method: "GET",
        URL: Ꮡ(new url.URL(Scheme: "http", Host: "fake.tld", Path: "/")),
        Proto: "HTTP/1.0",
        ProtoMajor: 1
    )));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xHitModˢ => ref xHitModˢᵇ;
private static readonly @string xHitModˢᵇ = "X-Hit-Mod"u8;

internal partial struct TestReverseProxyModifyResponse_tests /*dyn*/ {
    internal @string url;
    internal nint wantCode;
}

// Issue 14237. Test ModifyResponse and that an error from it
// causes the proxy to return StatusBadGateway, or StatusOK otherwise.
public static void TestReverseProxyModifyResponse(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Add(xHitModˢ, fmt.Sprintf("%v"u8, r.URL.Path == "/mod"u8));
        })));
        var backendServerʗ1 = backendServer;
        defer(backendServerʗ1.Close, ref ᒐ);

        var (rpURL, _) = url.Parse(backendServer.URL);
        var rproxy = NewSingleHostReverseProxy(rpURL);
        rproxy.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        rproxy.ModifyResponse = error (ж<http.Response> resp) => {
            if (resp.Header.Get(xHitModˢ) != "true"u8) {
                return fmt.Errorf("tried to by-pass proxy"u8);
            }
            return default;
        };

        var frontendProxy = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(rproxy));
        var frontendProxyʗ1 = frontendProxy;
        defer(frontendProxyʗ1.Close, ref ᒐ);

        var tests = slice<TestReverseProxyModifyResponse_tests>([
            new(frontendProxy.URL + "/mod"u8, http.StatusOK),
            new(frontendProxy.URL + "/schedule"u8, http.StatusBadGateway)
        ]);

        foreach (var (i, tt) in tests) {
            var (resp, err) = http.Get(tt.url);
            if (err != nil) {
                t.Fatalf("failed to reach proxy: %v", err);
            }
            {
                nint g = resp.StatusCode;
                nint e = tt.wantCode; if (g != e) {
                    t.Errorf("#%d: got res.StatusCode %d; expected %d", i, g, e);
                }
            }
            resp.Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct failingRoundTripper {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someErrorˢ => ref someErrorˢᵇ;
private static readonly @string someErrorˢᵇ = "some error"u8;

internal static (ж<http.Response>, error) RoundTrip(this failingRoundTripper _Δp0, ж<http.Request> _Δp1) {
    return (default, errors.New(someErrorˢ));
}

internal partial struct staticResponseRoundTripper {
    internal ж<http.Response> res;
}

internal static (ж<http.Response>, error) RoundTrip(this staticResponseRoundTripper rt, ж<http.Request> _) {
    return (rt.res, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someErrorToTriggerˢ => ref someErrorToTriggerˢᵇ;
private static readonly @string someErrorToTriggerˢᵇ = "some error to trigger errorHandler"u8;

internal partial struct TestReverseProxyErrorHandler_tests /*dyn*/ {
    internal @string name;
    internal nint wantCode;
    internal Action<http.ResponseWriter, ж<http.Request>, error> errorHandler;
    internal http.RoundTripper transport; // defaults to failingRoundTripper
    internal Func<ж<http.Response>, error> modifyResponse;
}

public static void TestReverseProxyErrorHandler(ж<testing.T> t) {
    var tests = slice<TestReverseProxyErrorHandler_tests>([
        new(
            name: "default",
            wantCode: http.StatusBadGateway
        ),
        new(
            name: "errorhandler",
            wantCode: http.StatusTeapot,
            errorHandler: (http.ResponseWriter rw, ж<http.Request> req, error err) => {
                rw.WriteHeader(http.StatusTeapot);
            }
        ),
        new(
            name: "modifyresponse_noerr",
            transport: new staticResponseRoundTripper(
                Ꮡ(new http.Response(StatusCode: 345, Body: http.NoBody))
            ),
            modifyResponse: (ж<http.Response> res) => {
                res.StatusCode++;
                return default;
            },
            errorHandler: (http.ResponseWriter rw, ж<http.Request> req, error err) => {
                rw.WriteHeader(http.StatusTeapot);
            },
            wantCode: 346
        ),
        new(
            name: "modifyresponse_err",
            transport: new staticResponseRoundTripper(
                Ꮡ(new http.Response(StatusCode: 345, Body: http.NoBody))
            ),
            modifyResponse: (ж<http.Response> res) => {
                res.StatusCode++;
                return errors.New(someErrorToTriggerˢ);
            },
            errorHandler: (http.ResponseWriter rw, ж<http.Request> req, error err) => {
                rw.WriteHeader(http.StatusTeapot);
            },
            wantCode: http.StatusTeapot
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestReverseProxyErrorHandler_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var target = Ꮡ(new url.URL(
                    Scheme: "http",
                    Host: "dummy.tld",
                    Path: "/"
                ));
                var rproxy = NewSingleHostReverseProxy(target);
                rproxy.Transport = ttʗ1.transport;
                rproxy.ModifyResponse = ttʗ1.modifyResponse;
                if (rproxy.Transport == nil) {
                    rproxy.Transport = new failingRoundTripper();
                }
                rproxy.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
                if (ttʗ1.errorHandler != nil) {
                    rproxy.ErrorHandler = ttʗ1.errorHandler;
                }
                var frontendProxy = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(rproxy));
                var frontendProxyʗ1 = frontendProxy;
                defer(frontendProxyʗ1.Close, ref ᒐ);

                var (resp, err) = http.Get(frontendProxy.URL + "/test"u8);
                if (err != nil) {
                    tΔ1.Fatalf("failed to reach proxy: %v", err);
                }
                {
                    nint g = resp.StatusCode;
                    nint e = ttʗ1.wantCode; if (g != e) {
                        tΔ1.Errorf("got res.StatusCode %d; expected %d", g, e);
                    }
                }
                resp.Body.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thisCallWasRelayedByTheˢ => ref thisCallWasRelayedByTheˢᵇ;
private static readonly @string thisCallWasRelayedByTheˢᵇ = "this call was relayed by the reverse proxy"u8;
internal static ref readonly @string contentLengthˢ => ref contentLengthˢᵇ;
private static readonly @string contentLengthˢᵇ = "Content-Length"u8;

// Issue 16659: log errors from short read
public static void TestReverseProxy_CopyBuffer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            @string @out = thisCallWasRelayedByTheˢ;
            // Coerce a wrong content length to induce io.UnexpectedEOF
            w.Header().Set(contentLengthˢ, fmt.Sprintf("%d"u8, len(@out) * 2));
            fmt.Fprintln(new httputil_test_package.http_ResponseWriterᴠWriter(w), @out);
        })));
        var backendServerʗ1 = backendServer;
        defer(backendServerʗ1.Close, ref ᒐ);

        var (rpURL, err) = url.Parse(backendServer.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        ref var proxyLog = ref heap(new bytes.Buffer(), out var ᏑproxyLog);
        var rproxy = NewSingleHostReverseProxy(rpURL);
        rproxy.ErrorLog = log.New(new httputil_test_package.bytes_BufferжWriter(ᏑproxyLog), ""u8, log.Lshortfile);
        var donec = new channel<bool>(1);
        var donecʗ1 = donec;
        var rproxyʗ1 = rproxy;
        var frontendProxy = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            GoFrame ᒐ = default;
            try {
                var donecʗ2 = donecʗ1;
                defer(() => {
                    donecʗ2.ᐸꟷ(true);
                }, ref ᒐ);
                rproxyʗ1.ServeHTTP(w, r);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var frontendProxyʗ1 = frontendProxy;
        defer(frontendProxyʗ1.Close, ref ᒐ);

        {
            (_, err) = frontendProxy.Client().Get(frontendProxy.URL); if (err == nil) {
                t.Fatalf("want non-nil error");
            }
        }
        // The race detector complains about the proxyLog usage in logf in copyBuffer
        // and our usage below with proxyLog.Bytes() so we're explicitly using a
        // channel to ensure that the ReverseProxy's ServeHTTP is done before we
        // continue after Get.
        ᐸꟷ(donec);

        var expected = slice<@string>([
            "EOF",
            "read"
        ]);
        foreach (var (_, phrase) in expected) {
            if (!bytes.Contains(proxyLog.Bytes(), slice<byte>(phrase))) {
                t.Errorf("expected log to contain phrase %q", phrase);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct staticTransport {
    internal ж<http.Response> res;
}

internal static (ж<http.Response>, error) RoundTrip(this ref staticTransport t, ж<http.Request> r) {
    return (t.res, default);
}

public static void BenchmarkServeHTTP(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var res = Ꮡ(new http.Response(
        StatusCode: 200,
        Body: io.NopCloser(new httputil_test_package.strings_ReaderжReader(strings.NewReader(""u8)))
    ));
    var proxy = Ꮡ(new ReverseProxy(
        Director: (ж<http.Request> _) => {
        },
        Transport: new httputil_internal_test_package.staticTransportжRoundTripper(Ꮡ(new staticTransport(res)))
    ));

    var w = httptest.NewRecorder();
    var r = httptest.NewRequest(getˢ, "/", default);

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        proxy.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(w), r);
    }
}

internal partial struct TestServeHTTPDeepCopy_result /*dyn*/ {
    internal @string before, after;
}

public static void TestServeHTTPDeepCopy(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("Hello Gopher!"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var resultChan = new channel<TestServeHTTPDeepCopy_result>(1);
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var proxyHandlerʗ1 = proxyHandler;
        var resultChanʗ1 = resultChan;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            @string before = r.URL.String();
            proxyHandlerʗ1.ServeHTTP(w, r);
            @string after = r.URL.String();
            resultChanʗ1.ᐸꟷ(new TestServeHTTPDeepCopy_result(before: before, after: after));
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var want = new TestServeHTTPDeepCopy_result(before: "/", after: "/");

        (var res, err) = frontend.Client().Get(frontend.URL);
        if (err != nil) {
            t.Fatalf("Do: %v", err);
        }
        res.Body.Close();

        var got = ᐸꟷ(resultChan);
        if (got != want) {
            t.Errorf("got = %+v; want = %+v", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpFooTldˢ => ref httpFooTldˢᵇ;
private static readonly @string httpFooTldˢᵇ = "http://foo.tld/"u8;
internal static ref readonly @string fromDirectorˢ => ref fromDirectorˢᵇ;
private static readonly @string fromDirectorˢᵇ = "From-Director"u8;

// Issue 18327: verify we always do a deep copy of the Request.Header map
// before any mutations.
public static void TestClonesRequestHeaders(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        log.SetOutput(io.Discard);
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, httpFooTldˢ, default);
        req.RemoteAddr = "1.2.3.4:56789";
        var rp = Ꮡ(new ReverseProxy(
            Director: (ж<http.Request> reqΔ1) => {
                reqΔ1.Header.Set(fromDirectorˢ, "1");
            },
            Transport: new httputil_internal_test_package.roundTripperFuncᴠRoundTripper(new roundTripperFunc((ж<http.Request> reqΔ2) => {
                {
                    @string v = reqΔ2.Header.Get(fromDirectorˢ); if (v != "1"u8) {
                        t.Errorf("From-Directory value = %q; want 1", v);
                    }
                }
                return (default, io.EOF);
            }))
        ));
        rp.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), req);

        foreach (var (_, h) in slice<@string>([
            "From-Director",
            "X-Forwarded-For"
        ])) {
            if (req.Header.Get(h) != ""u8) {
                t.Errorf("%v header mutation modified caller's request", h);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal delegate (ж<http.Response>, error) roundTripperFunc(ж<http.Request> req);

internal static partial (ж<http.Response>, error) RoundTrip(this roundTripperFunc fn, ж<http.Request> req) {
    return fn(req);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string modifyResponseErrorˢ => ref modifyResponseErrorˢᵇ;
private static readonly @string modifyResponseErrorˢᵇ = "ModifyResponse error"u8;

public static void TestModifyResponseClosesBody(ж<testing.T> t) {
    var (req, _) = http.NewRequest(getˢ, httpFooTldˢ, default);
    req.RemoteAddr = "1.2.3.4:56789";
    var closeCheck = @new<checkCloser>();
    var logBuf = @new<strings.Builder>();
    var outErr = errors.New(modifyResponseErrorˢ);
        var outErrʗ1 = outErr;
    var rp = Ꮡ(new ReverseProxy(
        Director: (ж<http.Request> reqΔ1) => {
        },
        Transport: new httputil_internal_test_package.staticTransportжRoundTripper(Ꮡ(new staticTransport(Ꮡ(new http.Response(
            StatusCode: 200,
            Body: new httputil_internal_test_package.checkCloserжReadCloser(closeCheck)
        ))
        ))),
        ErrorLog: log.New(new httputil_test_package.strings_BuilderжWriter(logBuf), ""u8, 0),
        ModifyResponse: (ж<http.Response> _Δp0) => outErrʗ1
    ));
    var rec = httptest.NewRecorder();
    rp.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(rec), req);
    var res = rec.Result();
    {
        nint g = res.StatusCode;
        nint e = http.StatusBadGateway; if (g != e) {
            t.Errorf("got res.StatusCode %d; expected %d", g, e);
        }
    }
    if (!closeCheck.closed) {
        t.Errorf("body should have been closed");
    }
    {
        @string g = logBuf.String();
        @string e = outErr.Error(); if (!strings.Contains(g, e)) {
            t.Errorf("ErrorLog %q does not contain %q", g, e);
        }
    }
}

internal partial struct checkCloser {
    internal bool closed;
}

internal static error Close(this ref checkCloser cc) {
    cc.closed = true;
    return default;
}

internal static (nint, error) Read(this ref checkCloser cc, slice<byte> b) {
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object handlerShouldHaveˢ = (@string)"handler should have panicked"u8;
internal static readonly object expectedErrAbortHandlerˢ = (@string)"expected ErrAbortHandler, got"u8;

// Issue 23643: panic on body copy error
public static void TestReverseProxy_PanicBodyError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        log.SetOutput(io.Discard);
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            @string @out = thisCallWasRelayedByTheˢ;
            // Coerce a wrong content length to induce io.ErrUnexpectedEOF
            w.Header().Set(contentLengthˢ, fmt.Sprintf("%d"u8, len(@out) * 2));
            fmt.Fprintln(new httputil_test_package.http_ResponseWriterᴠWriter(w), @out);
        })));
        var backendServerʗ1 = backendServer;
        defer(backendServerʗ1.Close, ref ᒐ);

        var (rpURL, err) = url.Parse(backendServer.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        var rproxy = NewSingleHostReverseProxy(rpURL);

        // Ensure that the handler panics when the body read encounters an
        // io.ErrUnexpectedEOF
        defer(() => {
            var errΔ1 = recover();
            if (errΔ1 == nil) {
                t.Fatal(handlerShouldHaveˢ);
            }
            if (!AreEqual(errΔ1, http.ErrAbortHandler)) {
                t.Fatal(expectedErrAbortHandlerˢ, errΔ1);
            }
        }, ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, httpFooTldˢ, default);
        rproxy.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), req);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue #46866: panic without closing incoming request body causes a panic
public static partial void TestReverseProxy_PanicClosesIncomingBody(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            @string @out = thisCallWasRelayedByTheˢ;
            // Coerce a wrong content length to induce io.ErrUnexpectedEOF
            w.Header().Set(contentLengthˢ, fmt.Sprintf("%d"u8, len(@out) * 2));
            fmt.Fprintln(new httputil_test_package.http_ResponseWriterᴠWriter(w), @out);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint i = 0; i < 2; i++) {
            Ꮡwg.Add(1);
            var frontendʗ2 = frontend;
            var frontendClientʗ1 = frontendClient;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    for (nint j = 0; j < 10; j++) {
                        const int64 reqLen = /* 6 * 1024 * 1024 */ 6291456;
                        var (req, _) = http.NewRequest(postˢ, frontendʗ2.URL, new io.LimitedReaderжReader(Ꮡ(new io.LimitedReader(R: ((global::go.net.http.httputil_package.neverEnding)(rune)'x'), N: reqLen))));
                        req.ContentLength = reqLen;
                        var (resp, _) = frontendClientʗ1.Transport.RoundTrip(req);
                        if (resp != nil) {
                            io.Copy(io.Discard, resp.Body);
                            resp.Body.Close();
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestSelectFlushInterval_tests /*dyn*/ {
    internal @string name;
    internal ж<global::go.net.http.httputil_package.ReverseProxy> p;
    internal ж<http.Response> res;
    internal time.Duration want;
}

public static void TestSelectFlushInterval(ж<testing.T> t) {
    var tests = slice<TestSelectFlushInterval_tests>([
        new(
            name: "default",
            res: Ꮡ(new http.Response()),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: 123
        ),
        new(
            name: "server-sent events overrides non-zero",
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"] = slice<@string>(["text/event-stream"])
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: -1
        ),
        new(
            name: "server-sent events overrides zero",
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"] = slice<@string>(["text/event-stream"])
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 0)),
            want: -1
        ),
        new(
            name: "server-sent events with media-type parameters overrides non-zero",
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"] = slice<@string>(["text/event-stream;charset=utf-8"])
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: -1
        ),
        new(
            name: "server-sent events with media-type parameters overrides zero",
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"] = slice<@string>(["text/event-stream;charset=utf-8"])
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 0)),
            want: -1
        ),
        new(
            name: "Content-Length: -1, overrides non-zero",
            res: Ꮡ(new http.Response(
                ContentLength: -1
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: -1
        ),
        new(
            name: "Content-Length: -1, overrides zero",
            res: Ꮡ(new http.Response(
                ContentLength: -1
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 0)),
            want: -1
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestSelectFlushInterval_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = ttʗ1.p.flushInterval(ttʗ1.res);
            if (got != ttʗ1.want) {
                tΔ1.Errorf("flushLatency = %v; want %v", got, ttʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedBackendRequestˢ = (@string)"unexpected backend request"u8;
internal static ref readonly @string unexpectedRequestˢ => ref unexpectedRequestˢᵇ;
private static readonly @string unexpectedRequestˢᵇ = "unexpected request"u8;
internal static ref readonly @string http11101Switchingˢ => ref http11101Switchingˢᵇ;
private static readonly @string http11101Switchingˢᵇ = "HTTP/1.1 101 Switching Protocols\r\nConnection: upgrade\r\nUpgrade: WebSocket\r\n\r\n"u8;
internal static ref readonly @string xModifiedˢ => ref xModifiedˢᵇ;
private static readonly @string xModifiedˢᵇ = "X-Modified"u8;
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;
internal static ref readonly @string xHeaderˢ => ref xHeaderˢᵇ;
private static readonly @string xHeaderˢᵇ = "X-Header"u8;
internal static ref readonly @string xValueˢ => ref xValueˢᵇ;
private static readonly @string xValueˢᵇ = "X-Value"u8;
internal static ref readonly @string websocketˢ => ref websocketˢᵇ;
private static readonly @string websocketˢᵇ = "websocket"u8;
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "Hello\n"u8;
internal static ref readonly @string backendGotHelloˢ => ref backendGotHelloˢᵇ;
private static readonly @string backendGotHelloˢᵇ = @"backend got ""Hello"""u8;

public static void TestReverseProxyWebSocket(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            GoFrame ᒐ = default;
            try {
                if (upgradeType(r.Header) != "websocket"u8) {
                    t.Error(unexpectedBackendRequestˢ);
                    http.Error(w, unexpectedRequestˢ, 400);
                    return;
                }
                var (cΔ1, _, errΔ1) = w._<http.Hijacker>().Hijack();
                if (errΔ1 != nil) {
                    t.Error(errΔ1);
                    return;
                }
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
                io.WriteString(new httputil_test_package.net_ConnᴠWriter(cΔ1), http11101Switchingˢ);
                var bsΔ1 = bufio.NewScanner(new httputil_test_package.net_ConnᴠReader(cΔ1));
                if (!bsΔ1.Scan()) {
                    t.Errorf("backend failed to read line from client: %v", bsΔ1.Err());
                    return;
                }
                fmt.Fprintf(new httputil_test_package.net_ConnᴠWriter(cΔ1), "backend got %q\n"u8, bsΔ1.Text());
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var backendServerʗ1 = backendServer;
        defer(backendServerʗ1.Close, ref ᒐ);

        var (backURL, _) = url.Parse(backendServer.URL);
        var rproxy = NewSingleHostReverseProxy(backURL);
        rproxy.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        rproxy.ModifyResponse = error (ж<http.Response> resΔ1) => {
            resΔ1.Header.Add(xModifiedˢ, trueˢ);
            return default;
        };

        var rproxyʗ1 = rproxy;
        var handler = new http.HandlerFunc((http.ResponseWriter rw, ж<http.Request> reqΔ1) => {
            rw.Header().Set(xHeaderˢ, xValueˢ);
            rproxyʗ1.ServeHTTP(rw, reqΔ1);
            {
                @string gotΔ1 = rw.Header().Get(xModifiedˢ);
                @string wantΔ1 = trueˢ; if (gotΔ1 != wantΔ1) {
                    t.Errorf("response writer X-Modified header = %q; want %q", gotΔ1, wantΔ1);
                }
            }
        });

        var frontendProxy = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(handler));
        var frontendProxyʗ1 = frontendProxy;
        defer(frontendProxyʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(getˢ, frontendProxy.URL, default);
        req.Header.Set(connectionˢ, upgradeˢ);
        req.Header.Set(upgradeˢ, websocketˢ);

        var c = frontendProxy.Client();
        var (res, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        if (res.StatusCode != 101) {
            t.Fatalf("status = %v; want 101", res.Status);
        }

        @string got = res.Header.Get(xHeaderˢ);
        @string want = xValueˢ;
        if (got != want) {
            t.Errorf("Header(XHeader) = %q; want %q", got, want);
        }

        if (!ascii.EqualFold(upgradeType(res.Header), websocketˢ)) {
            t.Fatalf("not websocket upgrade; got %#v", res.Header);
        }
        var (rwc, ok) = res.Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            t.Fatalf("response body is of type %T; does not implement ReadWriteCloser", res.Body);
        }
        var rwcʗ1 = rwc;
        defer(() => rwcʗ1.Close(), ref ᒐ);

        {
            @string gotΔ2 = res.Header.Get(xModifiedˢ);
            @string wantΔ2 = trueˢ; if (gotΔ2 != wantΔ2) {
                t.Errorf("response X-Modified header = %q; want %q", gotΔ2, wantΔ2);
            }
        }

        io.WriteString(rwc, helloˢ);
        var bs = bufio.NewScanner(rwc);
        if (!bs.Scan()) {
            t.Fatalf("Scan: %v", bs.Err());
        }
        got = bs.Text();
        want = backendGotHelloˢ;
        if (got != want) {
            t.Errorf("got %#q, want %#q", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string finalMessageˢ => ref finalMessageˢᵇ;
private static readonly @string finalMessageˢᵇ = "final message"u8;
internal static ref readonly @string unexpectedRequestˢ2 => ref unexpectedRequestˢ2ᵇ;
private static readonly @string unexpectedRequestˢ2ᵇ = "Unexpected request"u8;

public static void TestReverseProxyWebSocketCancellation(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        nint n = 5;
        var triggerCancelCh = new channel<bool>(n);
        @string nthResponse(nint i) => fmt.Sprintf("backend response #%d\n"u8, i);
        @string terminalMsg = finalMessageˢ;

        var nthResponseʗ1 = nthResponse;
        var triggerCancelChʗ1 = triggerCancelCh;
        var cst = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            GoFrame ᒐ = default;
            try {
                {
                    @string g = upgradeType(r.Header);
                    @string ws = websocketˢ; if (g != ws) {
                        t.Errorf("Unexpected upgrade type %q, want %q", g, ws);
                        http.Error(w, unexpectedRequestˢ2, 400);
                        return;
                    }
                }
                var (conn, bufrw, errΔ1) = w._<http.Hijacker>().Hijack();
                if (errΔ1 != nil) {
                    t.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);

                @string upgradeMsg = http11101Switchingˢ;
                {
                    var (_, errΔ2) = io.WriteString(new httputil_test_package.net_ConnᴠWriter(conn), upgradeMsg); if (errΔ2 != nil) {
                        t.Error(errΔ2);
                        return;
                    }
                }
                {
                    var (_, _, errΔ3) = bufrw.Value.Reader.Value.ReadLine(); if (errΔ3 != nil) {
                        t.Errorf("Failed to read line from client: %v", errΔ3);
                        return;
                    }
                }

                for (nint i = 0; i < n; i++) {
                    {
                        var (_, errΔ4) = bufrw.Value.Writer.Value.WriteString(nthResponseʗ1(i)); if (errΔ4 != nil) {
                            var selᴛ5 = triggerCancelChʗ1;
                            switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
                            case 0 when selᴛ5.ꟷᐳ(out _): {
                                break;
                            }
                            default: {
                                t.Errorf("Writing response #%d failed: %v", i, errΔ4);
                                break;
                            }}
                            return;
                        }
                    }
                    bufrw.Value.Writer.Value.Flush();
                    time.Sleep(time.ΔSecond);
                }
                {
                    var (_, errΔ5) = bufrw.Value.Writer.Value.WriteString(terminalMsg); if (errΔ5 != nil) {
                        var selᴛ6 = triggerCancelChʗ1;
                        switch (trySelect(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
                        case 0 when selᴛ6.ꟷᐳ(out _): {
                            break;
                        }
                        default: {
                            t.Errorf("Failed to write terminal message: %v", errΔ5);
                            break;
                        }}
                    }
                }
                bufrw.Value.Writer.Value.Flush();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.Close, ref ᒐ);

        var (backendURL, _) = url.Parse(cst.URL);
        var rproxy = NewSingleHostReverseProxy(backendURL);
        rproxy.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        rproxy.ModifyResponse = error (ж<http.Response> resΔ1) => {
            resΔ1.Header.Add(xModifiedˢ, trueˢ);
            return default;
        };

        var rproxyʗ1 = rproxy;
        var triggerCancelChʗ2 = triggerCancelCh;
        var handler = new http.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (http.ResponseWriter rw, ж<http.Request> reqΔ1) => {
            rw.Header().Set(xHeaderˢ, xValueˢ);
            var (ctx, cancel) = context.WithCancel(reqΔ1.Context());
            var cancelʗ1 = cancel;
            var triggerCancelChʗ3 = triggerCancelChʗ2;
            goǃ(() => {
                ᐸꟷ(triggerCancelChʗ3);
                cancelʗ1();
            });
            rproxyʗ1.ServeHTTP(rw, reqΔ1.WithContext(ctx));
        });

        var frontendProxy = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(handler));
        var frontendProxyʗ1 = frontendProxy;
        defer(frontendProxyʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(getˢ, frontendProxy.URL, default);
        req.Header.Set(connectionˢ, upgradeˢ);
        req.Header.Set(upgradeˢ, websocketˢ);

        var (res, err) = frontendProxy.Client().Do(req);
        if (err != nil) {
            t.Fatalf("Dialing to frontend proxy: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        {
            nint g = res.StatusCode;
            nint w = 101; if (g != w) {
                t.Fatalf("Switching protocols failed, got: %d, want: %d", g, w);
            }
        }

        {
            @string g = res.Header.Get(xHeaderˢ);
            @string w = xValueˢ; if (g != w) {
                t.Errorf("X-Header mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }

        {
            @string g = upgradeType(res.Header);
            @string w = websocketˢ; if (!ascii.EqualFold(g, w)) {
                t.Fatalf("Upgrade header mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }

        var (rwc, ok) = res.Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            t.Fatalf("Response body type mismatch, got %T, want io.ReadWriteCloser", res.Body);
        }

        {
            @string got = res.Header.Get(xModifiedˢ);
            @string want = trueˢ; if (got != want) {
                t.Errorf("response X-Modified header = %q; want %q", got, want);
            }
        }

        {
            var (_, errΔ6) = io.WriteString(rwc, helloˢ); if (errΔ6 != nil) {
                t.Fatalf("Failed to write first message: %v", errΔ6);
            }
        }

        // Read loop.

        var br = bufio.NewReader(rwc);
        for (;;) {
            var (line, errΔ7) = br.ReadString((byte)'\n');
            switch (ᐧ) {
            case {} when line == terminalMsg: { // this case before "err == io.EOF"
                t.Fatalf("The websocket request was not canceled, unfortunately!");
                break;
            }
            case {} when AreEqual(errΔ7, io.EOF): {
                return;
            }
            case {} when errΔ7 != nil: {
                t.Fatalf("Unexpected error: %v", errΔ7);
                break;
            }
            case {} when line == nthResponse(0): { // We've gotten the first response back
                // Let's trigger a cancel.
                close(triggerCancelCh);
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string serverSendsˢ => ref serverSendsˢᵇ;
private static readonly @string serverSendsˢᵇ = "server sends"u8;
internal static ref readonly @string clientSendsˢ => ref clientSendsˢᵇ;
private static readonly @string clientSendsˢᵇ = "client sends"u8;
internal static readonly object connIsNotATCPConnˢ = (@string)"conn is not a TCPConn"u8;
internal static ref readonly @string tcpˢ => ref tcpˢᵇ;
private static readonly @string tcpˢᵇ = "tcp"u8;

public static void TestReverseProxyWebSocketHalfTCP(ж<testing.T> t) {
    // Issue #35892: support TCP half-close when HTTP is upgraded in the ReverseProxy.
    // Specifically testing:
    // - the communication through the reverse proxy when the client or server closes
    //   either the read or write streams
    // - that closing the write stream is propagated through the proxy and results in reading
    //   EOF at the other end of the connection

    var exprᴛ1 = runtime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8: {
        t.Skipf("not supported on %s", runtime.GOOS);
        break;
    }}

    void mustRead(ж<testing.T> tΔ1, ж<net.TCPConn> conn, @string msg) {
        var b = new slice<byte>(len(msg));
        {
            var (_, err) = conn.Read(b); if (err != nil) {
                tΔ1.Errorf("failed to read: %v", err);
            }
        }

        {
            @string got = (@string)b;
            @string want = msg; if (got != want) {
                tΔ1.Errorf("got %#q, want %#q", got, want);
            }
        }
    }

    void mustReadError(ж<testing.T> tΔ2, ж<net.TCPConn> conn, error e) {
        var b = new slice<byte>(1);
        {
            var (_, err) = conn.Read(b); if (!errors.Is(err, e)) {
                tΔ2.Errorf("failed to read error: %v", err);
            }
        }
    }

    void mustWrite(ж<testing.T> tΔ3, ж<net.TCPConn> conn, @string msg) {
        {
            var (_, err) = conn.Write(slice<byte>(msg)); if (err != nil) {
                tΔ3.Errorf("failed to write: %v", err);
            }
        }
    }

    void mustCloseRead(ж<testing.T> tΔ4, ж<net.TCPConn> conn) {
        {
            var err = conn.CloseRead(); if (err != nil) {
                tΔ4.Errorf("failed to CloseRead: %v", err);
            }
        }
    }

    void mustCloseWrite(ж<testing.T> tΔ5, ж<net.TCPConn> conn) {
        {
            var err = conn.CloseWrite(); if (err != nil) {
                tΔ5.Errorf("failed to CloseWrite: %v", err);
            }
        }
    }

        var mustCloseReadʗ1 = mustCloseRead;
        var mustReadʗ1 = mustRead;
        var mustWriteʗ1 = mustWrite;

        var mustCloseWriteʗ1 = mustCloseWrite;
        var mustReadʗ2 = mustRead;
        var mustReadErrorʗ1 = mustReadError;
        var mustWriteʗ2 = mustWrite;

        var mustCloseReadʗ2 = mustCloseRead;
        var mustReadʗ3 = mustRead;
        var mustWriteʗ3 = mustWrite;

        var mustCloseWriteʗ2 = mustCloseWrite;
        var mustReadʗ4 = mustRead;
        var mustReadErrorʗ2 = mustReadError;
        var mustWriteʗ4 = mustWrite;
    var tests = new map<@string, Action<ж<testing.T>, ж<net.TCPConn>, ж<net.TCPConn>>>{
        ["server close read"] = (ж<testing.T> tΔ6, ж<net.TCPConn> cli, ж<net.TCPConn> srv) => {
            mustCloseReadʗ1(tΔ6, srv);
            mustWriteʗ1(tΔ6, srv, serverSendsˢ);
            mustReadʗ1(tΔ6, cli, serverSendsˢ);
        },
        ["server close write"] = (ж<testing.T> tΔ7, ж<net.TCPConn> cli, ж<net.TCPConn> srv) => {
            mustCloseWriteʗ1(tΔ7, srv);
            mustWriteʗ2(tΔ7, cli, clientSendsˢ);
            mustReadʗ2(tΔ7, srv, clientSendsˢ);
            mustReadErrorʗ1(tΔ7, cli, io.EOF);
        },
        ["client close read"] = (ж<testing.T> tΔ8, ж<net.TCPConn> cli, ж<net.TCPConn> srv) => {
            mustCloseReadʗ2(tΔ8, cli);
            mustWriteʗ3(tΔ8, cli, clientSendsˢ);
            mustReadʗ3(tΔ8, srv, clientSendsˢ);
        },
        ["client close write"] = (ж<testing.T> tΔ9, ж<net.TCPConn> cli, ж<net.TCPConn> srv) => {
            mustCloseWriteʗ2(tΔ9, cli);
            mustWriteʗ4(tΔ9, srv, serverSendsˢ);
            mustReadʗ4(tΔ9, cli, serverSendsˢ);
            mustReadErrorʗ2(tΔ9, srv, io.EOF);
        }
    };

    foreach (var (name, test) in tests) {
        var testʗ1 = test;
        t.Run(name, (ж<testing.T> tΔ10) => {
            GoFrame ᒐ = default;
            try {
                ref var srv = ref heap<ж<net.TCPConn>>(out var Ꮡsrv);

                var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
                    {
                        @string g = upgradeType(r.Header);
                        @string ws = websocketˢ; if (g != ws) {
                            tΔ10.Fatalf("Unexpected upgrade type %q, want %q", g, ws);
                        }
                    }

                    var (conn, _, errΔ1) = w._<http.Hijacker>().Hijack();
                    if (errΔ1 != nil) {
                        conn.Close();
                        tΔ10.Fatalf("hijack failed: %v", errΔ1);
                    }

                    bool ok = default;
                    {
                        (Ꮡsrv.ValueSlot, ok) = conn._<ж<net.TCPConn>>(ᐧ); if (!ok) {
                            conn.Close();
                            tΔ10.Fatal(connIsNotATCPConnˢ);
                        }
                    }

                    @string upgradeMsg = http11101Switchingˢ;
                    {
                        var (_, errΔ2) = io.WriteString(new httputil_test_package.net_TCPConnжWriter(Ꮡsrv.ValueSlot), upgradeMsg); if (errΔ2 != nil) {
                            Ꮡsrv.ValueSlot.Close();
                            tΔ10.Fatalf("backend upgrade failed: %v", errΔ2);
                        }
                    }
                })));
                var backendServerʗ1 = backendServer;
                defer(backendServerʗ1.Close, ref ᒐ);

                var (backendURL, _) = url.Parse(backendServer.URL);
                var rproxy = NewSingleHostReverseProxy(backendURL);
                rproxy.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
                var rproxyʗ1 = rproxy;
                var frontendProxy = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (http.ResponseWriter rw, ж<http.Request> reqΔ1) => {
                    rproxyʗ1.ServeHTTP(rw, reqΔ1);
                })));
                var frontendProxyʗ1 = frontendProxy;
                defer(frontendProxyʗ1.Close, ref ᒐ);

                var (frontendURL, _) = url.Parse(frontendProxy.URL);
                var (addr, err) = net.ResolveTCPAddr(tcpˢ, frontendURL.Host);
                if (err != nil) {
                    tΔ10.Fatalf("failed to resolve TCP address: %v", err);
                }
                (var cli, err) = net.DialTCP(tcpˢ, nil, addr);
                if (err != nil) {
                    tΔ10.Fatalf("failed to dial TCP address: %v", err);
                }
                var cliʗ1 = cli;
                defer(() => cliʗ1.Close(), ref ᒐ);

                var (req, _) = http.NewRequest(getˢ, frontendProxy.URL, default);
                req.Header.Set(connectionˢ, upgradeˢ);
                req.Header.Set(upgradeˢ, websocketˢ);
                {
                    var errΔ1 = req.Write(new httputil_test_package.net_TCPConnжWriter(cli)); if (errΔ1 != nil) {
                        tΔ10.Fatalf("failed to write request: %v", errΔ1);
                    }
                }

                var br = bufio.NewReader(new httputil_test_package.net_TCPConnжReader(cli));
                (var resp, err) = http.ReadResponse(br, Ꮡ(new http.Request(Method: "GET")));
                if (err != nil) {
                    tΔ10.Fatalf("failed to read response: %v", err);
                }
                if (resp.StatusCode != 101) {
                    tΔ10.Fatalf("status code not 101: %v", resp.StatusCode);
                }
                if (strings.ToLower(resp.Header.Get(upgradeˢ)) != "websocket"u8 || strings.ToLower(resp.Header.Get(connectionˢ)) != "upgrade"u8) {
                    tΔ10.Fatalf("frontend upgrade failed");
                }
                defer(() => Ꮡsrv.ValueSlot.Close(), ref ᒐ);

                testʗ1(tΔ10, cli, Ꮡsrv.ValueSlot);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string upgradeˢ2 => ref upgradeˢ2ᵇ;
private static readonly @string upgradeˢ2ᵇ = "upgrade"u8;

internal partial struct TestReverseProxyUpgradeNoCloseWrite_readWriteCloserOnly /*dyn*/ {
    /*embed*/ public io_package.ReadWriteCloser ReadWriteCloser;
}

public static void TestReverseProxyUpgradeNoCloseWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // The backend hijacks the connection,
        // reads all data from the client,
        // and returns.
        var backendDone = new channel<EmptyStruct>(0);
        var backendDoneʗ1 = backendDone;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Set(connectionˢ, upgradeˢ2);
            w.Header().Set(upgradeˢ, "u");
            w.WriteHeader(101);
            var (conn, _, errΔ1) = http.NewResponseController(w).Hijack();
            if (errΔ1 != nil) {
                t.Errorf("Hijack: %v", errΔ1);
            }
            io.Copy(io.Discard, new httputil_test_package.net_ConnᴠReader(conn));
            close(backendDoneʗ1);
        })));
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }

        // The proxy includes a ModifyResponse function which replaces the response body
        // with its own wrapper, dropping the original body's CloseWrite method.
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ModifyResponse = error (ж<http.Response> respΔ1) => {
            respΔ1.Body = new TestReverseProxyUpgradeNoCloseWrite_readWriteCloserOnly(respΔ1.Body._<io.ReadWriteCloser>());
            return default;
        };
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        // The client sends a request and closes the connection.
        var (req, _) = http.NewRequest(getˢ, frontend.URL, default);
        req.Header.Set(connectionˢ, upgradeˢ2);
        req.Header.Set(upgradeˢ, "u");
        (var resp, err) = frontend.Client().Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        resp.Body.Close();

        // We expect that the client's closure of the connection is propagated to the backend.
        ᐸꟷ(backendDone);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestUnannouncedTrailer(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.WriteHeader(http.StatusOK);
            w._<http.Flusher>().Flush();
            w.Header().Set(http.TrailerPrefix + "X-Unannounced-Trailer", unannouncedTrailerValueˢ);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        (var res, err) = frontendClient.Get(frontend.URL);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }

        io.ReadAll(res.Body);
        res.Body.Close();
        {
            @string g = res.Trailer.Get(xUnannouncedTrailerˢ);
            @string w = unannouncedTrailerValueˢ; if (g != w) {
                t.Errorf("Trailer(X-Unannounced-Trailer) = %q; want %q", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSetURL(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(r.Host));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
            var backendURLʗ1 = backendURL;
        var proxyHandler = Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(backendURLʗ1);
            }
        ));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        (var res, err) = frontendClient.Get(frontend.URL);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        (var body, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatalf("Reading body: %v", err);
        }

        {
            @string got = (@string)body;
            @string want = backendURL.Host; if (got != want) {
                t.Errorf("backend got Host %q, want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestSingleJoinSlash_tests /*dyn*/ {
    internal @string slasha;
    internal @string slashb;
    internal @string expected;
}

public static void TestSingleJoinSlash(ж<testing.T> t) {
    var tests = slice<TestSingleJoinSlash_tests>([
        new("https://www.google.com/", "/favicon.ico", "https://www.google.com/favicon.ico"),
        new("https://www.google.com", "/favicon.ico", "https://www.google.com/favicon.ico"),
        new("https://www.google.com", "favicon.ico", "https://www.google.com/favicon.ico"),
        new("https://www.google.com", "", "https://www.google.com/"),
        new("", "favicon.ico", "/favicon.ico")
    ]);
    foreach (var (_, tt) in tests) {
        {
            @string got = singleJoiningSlash(tt.slasha, tt.slashb); if (got != tt.expected) {
                t.Errorf("singleJoiningSlash(%q,%q) want %q got %q",
                    tt.slasha,
                    tt.slashb,
                    tt.expected,
                    got);
            }
        }
    }
}

internal partial struct TestJoinURLPath_tests /*dyn*/ {
    internal ж<url.URL> a;
    internal ж<url.URL> b;
    internal @string wantPath;
    internal @string wantRaw;
}

public static void TestJoinURLPath(ж<testing.T> t) {
    var tests = slice<TestJoinURLPath_tests>([
        new(Ꮡ(new url.URL(Path: "/a/b")), Ꮡ(new url.URL(Path: "/c")), "/a/b/c", ""),
        new(Ꮡ(new url.URL(Path: "/a/b", RawPath: "badpath")), Ꮡ(new url.URL(Path: "c")), "/a/b/c", "/a/b/c"),
        new(Ꮡ(new url.URL(Path: "/a/b", RawPath: "/a%2Fb")), Ꮡ(new url.URL(Path: "/c")), "/a/b/c", "/a%2Fb/c"),
        new(Ꮡ(new url.URL(Path: "/a/b", RawPath: "/a%2Fb")), Ꮡ(new url.URL(Path: "/c")), "/a/b/c", "/a%2Fb/c"),
        new(Ꮡ(new url.URL(Path: "/a/b/", RawPath: "/a%2Fb%2F")), Ꮡ(new url.URL(Path: "c")), "/a/b//c", "/a%2Fb%2F/c"),
        new(Ꮡ(new url.URL(Path: "/a/b/", RawPath: "/a%2Fb/")), Ꮡ(new url.URL(Path: "/c/d", RawPath: "/c%2Fd")), "/a/b/c/d", "/a%2Fb/c%2Fd")
    ]);

    foreach (var (_, tt) in tests) {
        var (p, rp) = joinURLPath(tt.a, tt.b);
        if (p != tt.wantPath || rp != tt.wantRaw) {
            t.Errorf("joinURLPath(URL(%q,%q),URL(%q,%q)) want (%q,%q) got (%q,%q)",
                tt.a.Path, tt.a.RawPath,
                tt.b.Path, tt.b.RawPath,
                tt.wantPath, tt.wantRaw,
                p, rp);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentᶜ => ref contentᶜᵇ;
private static readonly @string contentᶜᵇ = "response_content"u8;

public static void TestReverseProxyRewriteReplacesOut(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string content = contentᶜ;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(content));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
            var backendʗ2 = backend;
        var proxyHandler = Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                (r.Out, _) = http.NewRequest(getˢ, backendʗ2.URL, default);
            }
        ));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (res, err) = frontend.Client().Get(frontend.URL);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        var (body, _) = io.ReadAll(res.Body);
        {
            @string got = (@string)body;
            @string want = content; if (got != want) {
                t.Errorf("got response %q, want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpGoDevˢ => ref httpGoDevˢᵇ;
private static readonly @string httpGoDevˢᵇ = "http://go.dev/"u8;

public static void Test1xxHeadersNotModifiedAfterRoundTrip(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // https://go.dev/issue/65123: We use httptrace.Got1xxResponse to capture 1xx responses
        // and proxy them. httptrace handlers can execute after RoundTrip returns, in particular
        // after experiencing connection errors. When this happens, we shouldn't modify the
        // ResponseWriter headers after ReverseProxy.ServeHTTP returns.
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            for (nint i = 0; i < 5; i++) {
                w.WriteHeader(103);
            }
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests

        var rw = Ꮡ(new testResponseWriter());
        var proxyHandlerʗ1 = proxyHandler;
        var rwʗ1 = rw;
        func(() => {
            GoFrame ᒐ = default;
            try {
                // Cancel the request (and cause RoundTrip to return) immediately upon
                // seeing a 1xx response.
                var (ctx, cancel) = context.WithCancel(context.Background());
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);
                    var cancelʗ2 = cancel;
                ctx = httptrace.WithClientTrace(ctx, Ꮡ(new httptrace.ClientTrace(
                    Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
                        cancelʗ2();
                        return default;
                    }
                )));

                var (req, _) = http.NewRequestWithContext(ctx, getˢ, httpGoDevˢ, default);
                proxyHandlerʗ1.ServeHTTP(new httputil_internal_test_package.testResponseWriterжResponseWriter(rwʗ1), req);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })();
        // Trigger data race while iterating over response headers.
        // When run with -race, this causes the condition in https://go.dev/issue/65123 often
        // enough to detect reliably.
        foreach ((_, _) in rw.Header()) {
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linkˢ => ref linkˢᵇ;
private static readonly @string linkˢᵇ = "Link"u8;
internal static ref readonly @string styleCssRelPreloadAsˢ => ref styleCssRelPreloadAsˢᵇ;
private static readonly @string styleCssRelPreloadAsˢᵇ = "</style.css>; rel=preload; as=style"u8;
internal static ref readonly @string scriptJsRelPreloadAsˢ => ref scriptJsRelPreloadAsˢᵇ;
private static readonly @string scriptJsRelPreloadAsˢᵇ = "</script.js>; rel=preload; as=script"u8;
internal static ref readonly @string fooJsRelPreloadAsScriptˢ => ref fooJsRelPreloadAsScriptˢᵇ;
private static readonly @string fooJsRelPreloadAsScriptˢᵇ = "</foo.js>; rel=preload; as=script"u8;
internal static readonly object unexpected1xxResponseˢ = (@string)"Unexpected 1xx response"u8;

public static void Test1xxResponses(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            var h = w.Header();
            h.Add(linkˢ, styleCssRelPreloadAsˢ);
            h.Add(linkˢ, scriptJsRelPreloadAsˢ);
            w.WriteHeader(http.StatusEarlyHints);

            h.Add(linkˢ, fooJsRelPreloadAsScriptˢ);
            w.WriteHeader(http.StatusProcessing);

            w.Write(slice<byte>("Hello"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();

        void checkLinkHeaders(ж<testing.T> tΔ1, slice<@string> expected, slice<@string> got) {
            tΔ1.Helper();

            if (len(expected) != len(got)) {
                tΔ1.Errorf("Expected %d link headers; got %d", len(expected), len(got));
            }

            foreach (var i in range(expected)) {
                if (i >= len(got)) {
                    tΔ1.Errorf("Expected %q link header; got nothing", expected[i]);

                    continue;
                }

                if (expected[i] != got[i]) {
                    tΔ1.Errorf("Expected %q link header; got %q", expected[i], got[i]);
                }
            }
        }

        uint8 respCounter = default;
            var checkLinkHeadersʗ1 = checkLinkHeaders;
        var trace = Ꮡ(new httptrace.ClientTrace(
            Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
                switch (code) {
                case var ᴛ when ᴛ == http.StatusEarlyHints: {
                    checkLinkHeadersʗ1(t, slice<@string>(["</style.css>; rel=preload; as=style", "</script.js>; rel=preload; as=script"]), header[linkˢ]);
                    break;
                }
                case var ᴛ when ᴛ == http.StatusProcessing: {
                    checkLinkHeadersʗ1(t, slice<@string>(["</style.css>; rel=preload; as=style", "</script.js>; rel=preload; as=script", "</foo.js>; rel=preload; as=script"]), header[linkˢ]);
                    break;
                }
                default: {
                    t.Error(unexpected1xxResponseˢ);
                    break;
                }}

                respCounter++;

                return default;
            }
        ));
        var (req, _) = http.NewRequestWithContext(httptrace.WithClientTrace(context.Background(), trace), getˢ, frontend.URL, default);

        (var res, err) = frontendClient.Do(req);
        if (err != nil) {
            t.Fatalf("Get: %v", err);
        }

        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        if (respCounter != 2) {
            t.Errorf("Expected 2 1xx responses; got %d", respCounter);
        }
        checkLinkHeaders(t, slice<@string>(["</style.css>; rel=preload; as=style", "</script.js>; rel=preload; as=script", "</foo.js>; rel=preload; as=script"]), res.Header[linkˢ]);

        var (body, _) = io.ReadAll(res.Body);
        if (((sstring)body) != "Hello"u8) {
            t.Errorf("Read body %q; want Hello", body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal const bool testWantsCleanQuery = true;
internal const bool testWantsRawQuery = false;

public static void TestReverseProxyQueryParameterSmugglingDirectorDoesNotParseForm(ж<testing.T> t) {
    testReverseProxyQueryParameterSmuggling(t, testWantsRawQuery, (ж<url.URL> u) => {
        var proxyHandler = NewSingleHostReverseProxy(u);
        var oldDirector = proxyHandler.Director;
        var oldDirectorʗ1 = oldDirector;
        proxyHandler.Director = (ж<http.Request> r) => {
            oldDirectorʗ1(r);
        };
        return proxyHandler;
    });
}

public static void TestReverseProxyQueryParameterSmugglingDirectorParsesForm(ж<testing.T> t) {
    testReverseProxyQueryParameterSmuggling(t, testWantsCleanQuery, (ж<url.URL> u) => {
        var proxyHandler = NewSingleHostReverseProxy(u);
        var oldDirector = proxyHandler.Director;
        var oldDirectorʗ1 = oldDirector;
        proxyHandler.Director = (ж<http.Request> r) => {
            // Parsing the form causes ReverseProxy to remove unparsable
            // query parameters before forwarding.
            r.FormValue("a");
            oldDirectorʗ1(r);
        };
        return proxyHandler;
    });
}

public static void TestReverseProxyQueryParameterSmugglingRewrite(ж<testing.T> t) {
    testReverseProxyQueryParameterSmuggling(t, testWantsCleanQuery, (ж<url.URL> u) => Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(u);
            }
        )));
}

public static void TestReverseProxyQueryParameterSmugglingRewritePreservesRawQuery(ж<testing.T> t) {
    testReverseProxyQueryParameterSmuggling(t, testWantsRawQuery, (ж<url.URL> u) => Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(u);
                r.Out.URL.RawQuery = r.In.URL.RawQuery;
            }
        )));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentᶜ1 => ref contentᶜ1ᵇ;
private static readonly @string contentᶜ1ᵇ = "response_content"u8;

internal partial struct testReverseProxyQueryParameterSmuggling_type /*dyn*/ {
    internal @string rawQuery;
    internal @string cleanQuery;
}

internal static void testReverseProxyQueryParameterSmuggling(ж<testing.T> t, bool wantCleanQuery, Func<ж<url.URL>, ж<global::go.net.http.httputil_package.ReverseProxy>> newProxy) {
    GoFrame ᒐ = default;
    try {
        @string content = contentᶜ1;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(r.URL.RawQuery));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var proxyHandler = newProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        // Don't spam output with logs of queries containing semicolons.
        backend.Config.ErrorLog = log.New(io.Discard, ""u8, 0);
        frontend.Config.ErrorLog = log.New(io.Discard, ""u8, 0);

        foreach (var (_, test) in slice<testReverseProxyQueryParameterSmuggling_type>([new(
            rawQuery: "a=1&a=2;b=3",
            cleanQuery: "a=1"
        ), new(
            rawQuery: "a=1&a=%zz&b=3",
            cleanQuery: "a=1&b=3"
        ), new(
            rawQuery: "a=%zz",
            cleanQuery: ""
        ), new(
            rawQuery: strings.Repeat("a=1&", 10000) + "a=1"u8,
            cleanQuery: ""
        )
        ])) {
            var (res, errΔ1) = frontend.Client().Get(frontend.URL + "?"u8 + test.rawQuery);
            if (errΔ1 != nil) {
                t.Fatalf("Get: %v", errΔ1);
            }
            var resʗ1 = res;
            defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
            var (body, _) = io.ReadAll(res.Body);
            @string wantQuery = test.rawQuery;
            if (wantCleanQuery) {
                wantQuery = test.cleanQuery;
            }
            {
                @string got = (@string)body;
                @string want = wantQuery; if (got != want) {
                    t.Errorf("proxy forwarded raw query %q as %q, want %q", test.rawQuery, got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someprotoˢ => ref someprotoˢᵇ;
private static readonly @string someprotoˢᵇ = "someproto"u8;
internal static ref readonly @string readErrorˢ => ref readErrorˢᵇ;
private static readonly @string readErrorˢᵇ = "read error"u8;
internal static ref readonly @string httpExampleTldˢ => ref httpExampleTldˢᵇ;
private static readonly @string httpExampleTldˢᵇ = "http://example.tld/"u8;

// Issue #72954: We should not call WriteHeader on a ResponseWriter after hijacking
// the connection.
public static void TestReverseProxyHijackCopyError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Set(upgradeˢ, someprotoˢ);
            w.WriteHeader(http.StatusSwitchingProtocols);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse(backend.URL);
        if (err != nil) {
            t.Fatal(err);
        }
            var backendURLʗ1 = backendURL;

        var proxyHandler = Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(backendURLʗ1);
            },
            ModifyResponse: (ж<http.Response> resp) => {
                resp.Body = new httputil_internal_test_package.testReadWriteCloserжReadCloser(Ꮡ(new testReadWriteCloser(
                    read: (slice<byte> _Δp0) => (0, errors.New(readErrorˢ))
                )));
                return default;
            }
        ));

        var hijacked = false;

        var rw = Ꮡ(new testResponseWriter(
            writeHeader: (nint statusCode) => {
                if (hijacked) {
                    t.Errorf("WriteHeader(%v) called after Hijack", statusCode);
                }
            },
            hijack: [MethodImpl(MethodImplOptions.NoInlining)] () => {
                hijacked = true;
                var (cli, srv) = net.Pipe();
                goǃ((ᴛ1, ᴛ2) => io.Copy(ᴛ1, ᴛ2), io.Discard, new httputil_test_package.net_ConnᴠReader(cli));
                return (srv, bufio.NewReadWriter(bufio.NewReader(new httputil_test_package.net_ConnᴠReader(srv)), bufio.NewWriter(new httputil_test_package.net_ConnᴠWriter(srv))), default);
            }
        ));
        var (req, _) = http.NewRequest(getˢ, httpExampleTldˢ, default);
        req.Header.Set(upgradeˢ, someprotoˢ);
        proxyHandler.ServeHTTP(new httputil_internal_test_package.testResponseWriterжResponseWriter(rw), req);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doesnotexist12345ˢ => ref doesnotexist12345ˢᵇ;
private static readonly @string doesnotexist12345ˢᵇ = "doesnotexist:12345"u8;
internal static ref readonly @string testDataˢ => ref testDataˢᵇ;
private static readonly @string testDataˢᵇ = @"{""test"": ""data""}"u8;

// https://go.dev/issue/75933.
public static void TestReverseProxyInvalidUpstream100ContinueDoNotHang(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        ref var proxy = ref heap<global::go.net.http.httputil_package.ReverseProxy>(new ReverseProxy(
            Transport: new http.TransportжRoundTripper(Ꮡ(new http.Transport(DisableKeepAlives: true, ExpectContinueTimeout: (time.Duration)(60000000000L)))),
            Director: (ж<http.Request> request) => {
                request.URL.Scheme = httpˢ;
                request.URL.Host = doesnotexist12345ˢ; // non-existent upstream
            }
        ), out var Ꮡproxy);
        var handler = new http.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (http.ResponseWriter w, ж<http.Request> r) => {
            Ꮡproxy.ServeHTTP(w, r);
        });
        var upstreamServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(handler));
        var upstreamServerʗ1 = upstreamServer;
        defer(upstreamServerʗ1.Close, ref ᒐ);

        var (conn, err) = net.Dial(tcpˢ, upstreamServer.Listener.Addr().String());
        if (err != nil) {
            t.Fatal(err);
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);

        @string requestBody = testDataˢ;
        @string initialRequest = fmt.Sprintf("POST %s/test-expect HTTP/1.1\r\n"u8 + "Host: %s\r\n"u8 + "Content-Type: application/json\r\n"u8 + "Content-Length: %d\r\n"u8 + "Expect: 100-continue\r\n"u8 + "\r\n"u8, upstreamServer.URL, upstreamServer.Listener.Addr().String(), len(requestBody));

        {
            var (_, errΔ1) = conn.Write(slice<byte>(initialRequest)); if (errΔ1 != nil) {
                log.Fatal(errΔ1);
            }
        }
        var buff = new slice<byte>(1024);
        {
            var (_, errΔ2) = conn.Read(buff); if (errΔ2 != nil) {
                log.Fatal(errΔ2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct testResponseWriter {
    internal httpꓸHeader h;
    internal Action<nint> writeHeader;
    internal Func<slice<byte>, (nint, error)> write;
    internal Func<(net.Conn, ж<bufio.ReadWriter>, error)> hijack;
}

internal static httpꓸHeader Header(this ref testResponseWriter rw) {
    if (rw.h == nil) {
        rw.h = new httpꓸHeader(0);
    }
    return rw.h;
}

internal static void WriteHeader(this ref testResponseWriter rw, nint statusCode) {
    if (rw.writeHeader != nil) {
        rw.writeHeader(statusCode);
    }
}

internal static (nint, error) Write(this ref testResponseWriter rw, slice<byte> p) {
    if (rw.write != nil) {
        return rw.write(p);
    }
    return (len(p), default);
}

internal static (net.Conn, ж<bufio.ReadWriter>, error) Hijack(this ref testResponseWriter rw) {
    if (rw.hijack != nil) {
        return rw.hijack();
    }
    return (default, default, errors.ErrUnsupported);
}

internal partial struct testReadWriteCloser {
    internal Func<slice<byte>, (nint, error)> read;
    internal Func<slice<byte>, (nint, error)> write;
    internal Func<error> close;
}

internal static (nint, error) Read(this ref testReadWriteCloser rc, slice<byte> p) {
    if (rc.read != nil) {
        return rc.read(p);
    }
    return (0, io.EOF);
}

internal static (nint, error) Write(this ref testReadWriteCloser rc, slice<byte> p) {
    if (rc.write != nil) {
        return rc.write(p);
    }
    return (len(p), default);
}

internal static error Close(this ref testReadWriteCloser rc) {
    if (rc.close != nil) {
        return rc.close();
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpBackendTldˢ => ref httpBackendTldˢᵇ;
private static readonly @string httpBackendTldˢᵇ = "http://backend.tld/"u8;
internal static ref readonly @string connectˢ => ref connectˢᵇ;
private static readonly @string connectˢᵇ = "CONNECT"u8;
internal static ref readonly @string bodyˢ => ref bodyˢᵇ;
private static readonly @string bodyˢᵇ = "body"u8;

public static void TestReverseProxyCONNECT(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var proxy = Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                var (backendURL, _) = url.Parse(httpBackendTldˢ);
                r.SetURL(backendURL);
            }
        ));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxy));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);

        var (req, _) = http.NewRequest(connectˢ, frontend.URL, new httputil_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ)));
        var (resp, err) = frontend.Client().Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var respʗ1 = resp;
        defer(ᴛ0 => ᴛ0.Close(), respʗ1.Body, ref ᒐ);

        {
            nint got = resp.StatusCode;
            nint want = http.StatusMethodNotAllowed; if (got != want) {
                t.Errorf("on response to CONNECT: got status %v, want %v", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end httputil_internal_test_package
