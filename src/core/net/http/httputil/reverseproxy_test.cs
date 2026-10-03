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
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using httptrace = go.net.http.httptrace_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using os = os_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using go.net;
using go.net.http;
using go.net.http.@internal;
using net = net_package;
using static go.net.http.httputil_package;

partial class httputil_internal_test_package {

internal static readonly @string fakeHopHeader = "X-Fake-Hop-Header-For-Test"u8;

[GoInit] internal static void init() {
    inOurTests = true;
    hopHeaders = append(hopHeaders, fakeHopHeader);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string modeˢ = "mode"u8;
internal static readonly @string proxyConnectionˢ = "Proxy-Connection"u8;
internal static readonly @string someNameˢ = "some-name"u8;
internal static readonly @string trailersˢ2 = "Trailers"u8;
internal static readonly @string notASpecialHeaderFieldˢ = "not a special header field name"u8;
internal static readonly @string xTrailerˢ = "X-Trailer"u8;
internal static readonly @string xFooˢ = "X-Foo"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string xMultiValueˢ = "X-Multi-Value"u8;
internal static readonly @string trailerValueˢ = "trailer_value"u8;
internal static readonly @string unannouncedTrailerValueˢ = "unannounced_trailer_value"u8;
internal static readonly @string closeTeˢ = "close, TE"u8;
internal static readonly @string barTrailersˢ = "bar, trailers"u8;
internal static readonly @string shouldBeDeletedˢ = "should be deleted"u8;
internal static readonly @string setCookieˢ = "Set-Cookie"u8;
internal static readonly @string xUnannouncedTrailerˢ = "X-Unannounced-Trailer"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string backendResponseᶜ = "I am the backend"u8;

public static void TestReverseProxy(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string backendResponse = backendResponseᶜ;
        const nint backendStatus = 404;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            if ((~r).Method == "GET"u8 && r.FormValue(modeˢ) == "hangup"u8) {
                var (c, _, _) = w._<http.Hijacker>().Hijack();
                c.Close();
                return;
            }
            if (len((~r).TransferEncoding) > 0) {
                Ꮡt.Errorf("backend got unexpected TransferEncoding: %v"u8, (~r).TransferEncoding);
            }
            if ((~r).Header.Get(xForwardedForˢ) == ""u8) {
                Ꮡt.Errorf("didn't get X-Forwarded-For header"u8);
            }
            {
                @string c = (~r).Header.Get(connectionˢ); if (c != ""u8) {
                    Ꮡt.Errorf("handler got Connection header value %q"u8, c);
                }
            }
            {
                @string c = (~r).Header.Get("Te"u8); if (c != "trailers"u8) {
                    Ꮡt.Errorf("handler got Te header value %q; want 'trailers'"u8, c);
                }
            }
            {
                @string c = (~r).Header.Get(upgradeˢ); if (c != ""u8) {
                    Ꮡt.Errorf("handler got Upgrade header value %q"u8, c);
                }
            }
            {
                @string c = (~r).Header.Get(proxyConnectionˢ); if (c != ""u8) {
                    Ꮡt.Errorf("handler got Proxy-Connection header value %q"u8, c);
                }
            }
            {
                @string g = r.Value.Host;
                @string e = someNameˢ; if (g != e) {
                    Ꮡt.Errorf("backend got Host header %q, want %q"u8, g, e);
                }
            }
            w.Header().Set(trailersˢ2, notASpecialHeaderFieldˢ);
            w.Header().Set(trailerˢ, xTrailerˢ);
            w.Header().Set(xFooˢ, barˢ);
            w.Header().Set(upgradeˢ, fooˢ);
            w.Header().Set(fakeHopHeader, fooˢ);
            w.Header().Add(xMultiValueˢ, fooˢ);
            w.Header().Add(xMultiValueˢ, barˢ);
            http.SetCookie(w, Ꮡ(new httpꓸCookie(Name: "flavor"u8, Value: "chocolateChip"u8)));
            w.WriteHeader(backendStatus);
            w.Write(slice<byte>(backendResponse));
            w.Header().Set(xTrailerˢ, trailerValueˢ);
            w.Header().Set(http.TrailerPrefix + "X-Unannounced-Trailer", unannouncedTrailerValueˢ);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        getReq.Value.Host = someNameˢ;
        (~getReq).Header.Set(connectionˢ, closeTeˢ);
        (~getReq).Header.Add("Te"u8, fooˢ);
        (~getReq).Header.Add("Te"u8, barTrailersˢ);
        (~getReq).Header.Set(proxyConnectionˢ, shouldBeDeletedˢ);
        (~getReq).Header.Set(upgradeˢ, fooˢ);
        getReq.Value.Close = true;
        (var res, err) = frontendClient.Do(getReq);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        {
            nint g = res.Value.StatusCode;
            nint e = backendStatus; if (g != e) {
                Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
            }
        }
        {
            @string g = (~res).Header.Get(xFooˢ);
            @string e = barˢ; if (g != e) {
                Ꮡt.Errorf("got X-Foo %q; expected %q"u8, g, e);
            }
        }
        {
            @string c = (~res).Header.Get(fakeHopHeader); if (c != ""u8) {
                Ꮡt.Errorf("got %s header value %q"u8, fakeHopHeader, c);
            }
        }
        {
            @string g = (~res).Header.Get(trailersˢ2);
            @string e = notASpecialHeaderFieldˢ; if (g != e) {
                Ꮡt.Errorf("header Trailers = %q; want %q"u8, g, e);
            }
        }
        {
            nint g = len((~res).Header[xMultiValueˢ]);
            nint e = 2; if (g != e) {
                Ꮡt.Errorf("got %d X-Multi-Value header values; expected %d"u8, g, e);
            }
        }
        {
            nint g = len((~res).Header[setCookieˢ]);
            nint e = 1; if (g != e) {
                Ꮡt.Fatalf("got %d SetCookies, want %d"u8, g, e);
            }
        }
        {
            var (g, e) = (res.Value.Trailer, (new httpꓸHeader(new map<@string, slice<@string>>{["X-Trailer"u8] = default!}))); if (!reflect.DeepEqual(g, e)) {
                Ꮡt.Errorf("before reading body, Trailer = %#v; want %#v"u8, g, e);
            }
        }
        {
            var cookie = res.Cookies()[0]; if ((~cookie).Name != "flavor"u8) {
                Ꮡt.Errorf("unexpected cookie %q"u8, (~cookie).Name);
            }
        }
        var (bodyBytes, _) = io.ReadAll((~res).Body);
        {
            @string g = ((@string)bodyBytes);
            @string e = backendResponse; if (g != e) {
                Ꮡt.Errorf("got body %q; expected %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Trailer.Get(xTrailerˢ);
            @string e = trailerValueˢ; if (g != e) {
                Ꮡt.Errorf("Trailer(X-Trailer) = %q ; want %q"u8, g, e);
            }
        }
        {
            @string g = (~res).Trailer.Get(xUnannouncedTrailerˢ);
            @string e = unannouncedTrailerValueˢ; if (g != e) {
                Ꮡt.Errorf("Trailer(X-Unannounced-Trailer) = %q ; want %q"u8, g, e);
            }
        }
        (~res).Body.Close();
        // Test that a backend failing to be reached or one which doesn't return
        // a response results in a StatusBadGateway.
        (getReq, _) = http.NewRequest(getˢ, (~frontend).URL + "/?mode=hangup"u8, default!);
        getReq.Value.Close = true;
        (res, err) = frontendClient.Do(getReq);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~res).Body.Close();
        if ((~res).StatusCode != http.StatusBadGateway) {
            Ꮡt.Errorf("request to bad proxy = %v; want 502 StatusBadGateway"u8, (~res).Status);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fakeConnectionTokenᶜ = "X-Fake-Connection-Token"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string backendResponseᶜ1 = "I am the backend"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string someConnHeaderᶜ = "X-Some-Conn-Header"u8;

// Issue 16875: remove any proxied headers mentioned in the "Connection"
// header value.
public static void TestReverseProxyStripHeadersPresentInConnection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string fakeConnectionToken = fakeConnectionTokenᶜ;
        @string backendResponse = backendResponseᶜ1;
        // someConnHeader is some arbitrary header to be declared as a hop-by-hop header
        // in the Request's Connection header.
        @string someConnHeader = someConnHeaderᶜ;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            {
                @string c = (~r).Header.Get(connectionˢ); if (c != ""u8) {
                    Ꮡt.Errorf("handler got header %q = %q; want empty"u8, connectionˢ, c);
                }
            }
            {
                @string c = (~r).Header.Get(fakeConnectionToken); if (c != ""u8) {
                    Ꮡt.Errorf("handler got header %q = %q; want empty"u8, fakeConnectionToken, c);
                }
            }
            {
                @string c = (~r).Header.Get(someConnHeader); if (c != ""u8) {
                    Ꮡt.Errorf("handler got header %q = %q; want empty"u8, someConnHeader, c);
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
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var proxyHandlerʗ1 = proxyHandler;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            proxyHandlerʗ1.ServeHTTP(w, r);
            {
                @string cΔ1 = (~r).Header.Get(someConnHeader); if (cΔ1 != "should be deleted"u8) {
                    Ꮡt.Errorf("handler modified header %q = %q; want %q"u8, someConnHeader, cΔ1, shouldBeDeletedˢ);
                }
            }
            {
                @string cΔ2 = (~r).Header.Get(fakeConnectionToken); if (cΔ2 != "should be deleted"u8) {
                    Ꮡt.Errorf("handler modified header %q = %q; want %q"u8, fakeConnectionToken, cΔ2, shouldBeDeletedˢ);
                }
            }
            var c = (~r).Header[connectionˢ];
            slice<@string> cf = default!;
            foreach (var (_, f) in c) {
                foreach (var (_, vᴛ1) in strings.Split(f, ","u8)) {
                    var sf = vᴛ1;

                    {
                        sf = strings.TrimSpace(sf); if (sf != ""u8) {
                            cf = append(cf, sf);
                        }
                    }
                }
            }
            slices.Sort<slice<@string>, @string>(cf);
            var expectedValues = new @string[]{"Upgrade"u8, someConnHeader, fakeConnectionToken}.slice();
            slices.Sort<slice<@string>, @string>(expectedValues);
            if (!slices.Equal<slice<@string>, @string>(cf, expectedValues)) {
                Ꮡt.Errorf("handler modified header %q = %q; want %q"u8, connectionˢ, cf, expectedValues);
            }
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        (~getReq).Header.Add(connectionˢ, "Upgrade, " + fakeConnectionToken);
        (~getReq).Header.Add(connectionˢ, someConnHeader);
        (~getReq).Header.Set(someConnHeader, shouldBeDeletedˢ);
        (~getReq).Header.Set(fakeConnectionToken, shouldBeDeletedˢ);
        (var res, err) = frontend.Client().Do(getReq);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var bodyBytes, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("reading body: %v"u8, err);
        }
        {
            @string got = ((@string)bodyBytes);
            @string want = backendResponse; if (got != want) {
                Ꮡt.Errorf("got body %q; want %q"u8, got, want);
            }
        }
        {
            @string c = (~res).Header.Get(connectionˢ); if (c != ""u8) {
                Ꮡt.Errorf("handler got header %q = %q; want empty"u8, connectionˢ, c);
            }
        }
        {
            @string c = (~res).Header.Get(someConnHeader); if (c != ""u8) {
                Ꮡt.Errorf("handler got header %q = %q; want empty"u8, someConnHeader, c);
            }
        }
        {
            @string c = (~res).Header.Get(fakeConnectionToken); if (c != ""u8) {
                Ꮡt.Errorf("handler got header %q = %q; want empty"u8, fakeConnectionToken, c);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string backendResponseᶜ2 = "I am the backend"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string someConnHeaderᶜ1 = "X-Some-Conn-Header"u8;

public static void TestReverseProxyStripEmptyConnection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // See Issue 46313.
        @string backendResponse = backendResponseᶜ2;
        // someConnHeader is some arbitrary header to be declared as a hop-by-hop header
        // in the Request's Connection header.
        @string someConnHeader = someConnHeaderᶜ1;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            {
                var c = (~r).Header.Values(connectionˢ); if (len(c) != 0) {
                    Ꮡt.Errorf("handler got header %q = %v; want empty"u8, connectionˢ, c);
                }
            }
            {
                @string c = (~r).Header.Get(someConnHeader); if (c != ""u8) {
                    Ꮡt.Errorf("handler got header %q = %q; want empty"u8, someConnHeader, c);
                }
            }
            w.Header().Add(connectionˢ, ""u8);
            w.Header().Add(connectionˢ, someConnHeader);
            w.Header().Set(someConnHeader, shouldBeDeletedˢ);
            io.WriteString(new httputil_test_package.http_ResponseWriterᴠWriter(w), backendResponse);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var proxyHandlerʗ1 = proxyHandler;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            proxyHandlerʗ1.ServeHTTP(w, r);
            {
                @string c = (~r).Header.Get(someConnHeader); if (c != "should be deleted"u8) {
                    Ꮡt.Errorf("handler modified header %q = %q; want %q"u8, someConnHeader, c, shouldBeDeletedˢ);
                }
            }
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        (~getReq).Header.Add(connectionˢ, ""u8);
        (~getReq).Header.Add(connectionˢ, someConnHeader);
        (~getReq).Header.Set(someConnHeader, shouldBeDeletedˢ);
        (var res, err) = frontend.Client().Do(getReq);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var bodyBytes, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("reading body: %v"u8, err);
        }
        {
            @string got = ((@string)bodyBytes);
            @string want = backendResponse; if (got != want) {
                Ꮡt.Errorf("got body %q; want %q"u8, got, want);
            }
        }
        {
            @string c = (~res).Header.Get(connectionˢ); if (c != ""u8) {
                Ꮡt.Errorf("handler got header %q = %q; want empty"u8, connectionˢ, c);
            }
        }
        {
            @string c = (~res).Header.Get(someConnHeader); if (c != ""u8) {
                Ꮡt.Errorf("handler got header %q = %q; want empty"u8, someConnHeader, c);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string closeˢ = "close"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prevForwardedForᶜ = "client ip"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string backendResponseᶜ3 = "I am the backend"u8;

public static void TestXForwardedFor(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string prevForwardedFor = prevForwardedForᶜ;
        @string backendResponse = backendResponseᶜ3;
        const nint backendStatus = 404;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            if ((~r).Header.Get(xForwardedForˢ) == ""u8) {
                Ꮡt.Errorf("didn't get X-Forwarded-For header"u8);
            }
            if (!strings.Contains((~r).Header.Get(xForwardedForˢ), prevForwardedFor)) {
                Ꮡt.Errorf("X-Forwarded-For didn't contain prior data"u8);
            }
            w.WriteHeader(backendStatus);
            w.Write(slice<byte>(backendResponse));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        (~getReq).Header.Set(connectionˢ, closeˢ);
        (~getReq).Header.Set(xForwardedForˢ, prevForwardedFor);
        getReq.Value.Close = true;
        (var res, err) = frontend.Client().Do(getReq);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        {
            nint g = res.Value.StatusCode;
            nint e = backendStatus; if (g != e) {
                Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
            }
        }
        var (bodyBytes, _) = io.ReadAll((~res).Body);
        {
            @string g = ((@string)bodyBytes);
            @string e = backendResponse; if (g != e) {
                Ꮡt.Errorf("got body %q; expected %q"u8, g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 38079: don't append to X-Forwarded-For if it's present but nil
public static void TestXForwardedFor_Omit(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            {
                @string v = (~r).Header.Get(xForwardedForˢ); if (v != ""u8) {
                    Ꮡt.Errorf("got X-Forwarded-For header: %q"u8, v);
                }
            }
            w.Write(slice<byte>("hi"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var oldDirector = proxyHandler.Value.Director;
        var oldDirectorʗ1 = oldDirector;
        proxyHandler.Value.Director = (ж<http.Request> r) => {
            r.Value.Header[xForwardedForˢ] = default!;
            oldDirectorʗ1(r);
        };
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        getReq.Value.Host = someNameˢ;
        getReq.Value.Close = true;
        (var res, err) = frontend.Client().Do(getReq);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReverseProxyRewriteStripsForwarded(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var headers = new @string[]{
            "Forwarded"u8,
            "X-Forwarded-For"u8,
            "X-Forwarded-Host"u8,
            "X-Forwarded-Proto"u8
        }.slice();
        var headersʗ1 = headers;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            foreach (var (_, h) in headersʗ1) {
                {
                    @string v = (~r).Header.Get(h); if (v != ""u8) {
                        Ꮡt.Errorf("got %v header: %q"u8, h, v);
                    }
                }
            }
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
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
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        getReq.Value.Host = someNameˢ;
        getReq.Value.Close = true;
        foreach (var (_, h) in headers) {
            (~getReq).Header.Set(h, "x"u8);
        }
        (var res, err) = frontend.Client().Do(getReq);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


[GoType("dyn")] partial struct proxyQueryTestsᴛ1 {
    internal @string baseSuffix; // suffix to add to backend URL
    internal @string reqSuffix; // suffix to add to frontend's request URL
    internal @string want; // what backend should see for final request URL (without ?)
}
internal static slice<proxyQueryTestsᴛ1> proxyQueryTests = new proxyQueryTestsᴛ1[]{
    new(""u8, ""u8, ""u8),
    new("?sta=tic"u8, "?us=er"u8, "sta=tic&us=er"u8),
    new(""u8, "?us=er"u8, "us=er"u8),
    new("?sta=tic"u8, ""u8, "sta=tic"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xGotQueryˢ = "X-Got-Query"u8;

public static void TestReverseProxyQuery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Set(xGotQueryˢ, (~(~r).URL).RawQuery);
            w.Write(slice<byte>("hi"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        foreach (var (i, tt) in proxyQueryTests) {
            var (backendURL, err) = url.Parse((~backend).URL + tt.baseSuffix);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(NewSingleHostReverseProxy(backendURL)));
            var (req, _) = http.NewRequest(getˢ, (~frontend).URL + tt.reqSuffix, default!);
            req.Value.Close = true;
            (var res, err) = frontend.Client().Do(req);
            if (err != default!) {
                Ꮡt.Fatalf("%d. Get: %v"u8, i, err);
            }
            {
                @string g = (~res).Header.Get(xGotQueryˢ);
                @string e = tt.want; if (g != e) {
                    Ꮡt.Errorf("%d. got query %q; expected %q"u8, i, g, e);
                }
            }
            (~res).Body.Close();
            frontend.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedᶜ = "hi"u8;

public static void TestReverseProxyFlushInterval(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string expected = expectedᶜ;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(expected));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.FlushInterval = time.Microsecond;
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        req.Value.Close = true;
        (var res, err) = frontend.Client().Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        {
            var (bodyBytes, _) = io.ReadAll((~res).Body); if (((sstring)bodyBytes) != expected) {
                Ꮡt.Errorf("got body %q; expected %q"u8, bodyBytes, expected);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct mockFlusher {
    [GoEmbedded] public go.net.http_package.ResponseWriter ResponseWriter;
    internal bool flushed;
}

[GoRecv] internal static void Flush(this ref mockFlusher m) {
    m.flushed = true;
}

[GoType] internal partial struct wrappedRW {
    [GoEmbedded] public go.net.http_package.ResponseWriter ResponseWriter;
}

[GoRecv] internal static http.ResponseWriter Unwrap(this ref wrappedRW w) {
    return w.ResponseWriter;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedᶜ1 = "hi"u8;

public static void TestReverseProxyResponseControllerFlushInterval(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string expected = expectedᶜ1;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>(expected));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var mf = Ꮡ(new mockFlusher(nil));
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.FlushInterval = -1; // flush immediately
        var mfʗ1 = mf;
        var proxyHandlerʗ1 = proxyHandler;
        var proxyWithMiddleware = new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            mfʗ1.Value.ResponseWriter = w;
            w = new httputil_internal_test_package.wrappedRWжResponseWriter(Ꮡ(new wrappedRW(new httputil_internal_test_package.mockFlusherжResponseWriter(mfʗ1))));
            proxyHandlerʗ1.ServeHTTP(w, r);
        });
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(proxyWithMiddleware));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        req.Value.Close = true;
        (var res, err) = frontend.Client().Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        {
            var (bodyBytes, _) = io.ReadAll((~res).Body); if (((sstring)bodyBytes) != expected) {
                Ꮡt.Errorf("got body %q; expected %q"u8, bodyBytes, expected);
            }
        }
        if (!(~mf).flushed) {
            Ꮡt.Errorf("response writer was not flushed"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myHeaderˢ = "MyHeader"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedᶜ2 = "hi"u8;

public static void TestReverseProxyFlushIntervalHeaders(ж<testing.T> Ꮡt) {
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
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.FlushInterval = time.Microsecond;
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        req.Value.Close = true;
        var (ctx, cancel) = context.WithTimeout(req.Context(), (time.Duration)(10000000000L));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        req = req.WithContext(ctx);
        (var res, err) = frontend.Client().Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).Header.Get(myHeaderˢ) != expected) {
            Ꮡt.Errorf("got header %q; expected %q"u8, (~res).Header.Get(myHeaderˢ), expected);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object handlerNeverSawˢ = (@string)"Handler never saw CloseNotify"u8;
internal static readonly object serverClientDoReturnedˢ = (@string)"Server.Client().Do() returned nil error; want non-nil error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string backendResponseᶜ4 = "I am the backend"u8;

public static void TestReverseProxyCancellation(ж<testing.T> Ꮡt) {
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
                Ꮡt.Error(handlerNeverSawˢ);
                return;
            }
            case 1 when selᴛ4.ꟷᐳ(out _): {
                break;
            }}
            // Note: this should only happen in broken implementations, and the
            // closenotify case should be instantaneous.
            w.WriteHeader(http.StatusOK);
            w.Write(slice<byte>(backendResponse));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        backend.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        // Discards errors of the form:
        // http: proxy error: read tcp 127.0.0.1:44643: use of closed network connection
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();
        var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        var frontendClientʗ1 = frontendClient;
        var getReqʗ1 = getReq;
        var reqInFlightʗ2 = reqInFlight;
        goǃ(() => {
            ᐸꟷ(reqInFlightʗ2);
            (~frontendClientʗ1).Transport._<ж<http.Transport>>().CancelRequest(getReqʗ1);
        });
        (var res, err) = frontendClient.Do(getReq);
        if (res != nil) {
            Ꮡt.Errorf("got response %v; want nil"u8, (~res).Status);
        }
        if (err == default!) {
            // This should be an error like:
            // Get "http://127.0.0.1:58079": read tcp 127.0.0.1:58079:
            //    use of closed network connection
            Ꮡt.Error(serverClientDoReturnedˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<http.Request> req(ж<testing.T> Ꮡt, @string v) {
    var (req, err) = http.ReadRequest(bufio.NewReader(new httputil_test_package.strings_ReaderжReader(strings.NewReader(v))));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return req;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getHttp10ˢ = "GET / HTTP/1.0\r\n\r\n"u8;

// Issue 12344
public static void TestNilBody(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("hi"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var backendʗ2 = backend;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> _Δp1) => {
            var (backURL, _) = url.Parse((~backendʗ2).URL);
            var rp = NewSingleHostReverseProxy(backURL);
            var r = req(Ꮡt, getHttp10ˢ);
            r.Value.Body = default!; // this accidentally worked in Go 1.4 and below, so keep it working
            rp.ServeHTTP(w, r);
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (res, err) = http.Get((~frontend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)slurp) != "hi"u8) {
            Ꮡt.Errorf("Got %q; want %q"u8, slurp, (@string)"hi"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 15524
public static void TestUserAgentHeader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string gotUA = default!;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            gotUA = (~r).Header.Get(userAgentˢ);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = @new<global::go.net.http.httputil_package.ReverseProxy>();
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var backendURLʗ1 = backendURL;
        proxyHandler.Value.Director = (ж<http.Request> req) => {
            req.Value.URL = backendURLʗ1;
        };
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();
        foreach (var (_, sentUA) in new @string[]{"explicit UA"u8, ""u8}.slice()) {
            var (getReq, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
            (~getReq).Header.Set(userAgentˢ, sentUA);
            getReq.Value.Close = true;
            var (res, errΔ1) = frontendClient.Do(getReq);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("Get: %v"u8, errΔ1);
            }
            (~res).Body.Close();
            {
                @string got = gotUA;
                @string want = sentUA; if (got != want) {
                    Ꮡt.Errorf("got forwarded User-Agent %q, want %q"u8, got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct bufferPool {
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
internal static readonly @string getBufˢ = "getBuf"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string msgᶜ = "hi"u8;

public static void TestReverseProxyGetPutBuffer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string msg = msgᶜ;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            io.WriteString(new httputil_test_package.http_ResponseWriterᴠWriter(w), msg);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
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
        rp.Value.BufferPool = new bufferPool(
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
        var (req, _) = http.NewRequest(getˢ, (~frontend).URL, default!);
        req.Value.Close = true;
        (var res, err) = frontend.Client().Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        (var slurp, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (err != default!) {
            Ꮡt.Fatalf("reading body: %v"u8, err);
        }
        if (((sstring)slurp) != msg) {
            Ꮡt.Errorf("msg = %q; want %q"u8, slurp, msg);
        }
        var wantLog = new @string[]{"getBuf"u8, "putBuf-"u8 + strconv.Itoa(size)}.slice();
        Ꮡmu.Lock();
        defer(Ꮡmu.Unlock, ref ᒐ);
        if (!slices.Equal<slice<@string>, @string>(log, wantLog)) {
            Ꮡt.Errorf("Log events = %q; want %q"u8, log, wantLog);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object backendReadWrongRequestˢ = (@string)"Backend read wrong request body."u8;
internal static readonly @string postˢ = "POST"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string backendResponseᶜ5 = "I am the backend"u8;

public static void TestReverseProxy_Post(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string backendResponse = backendResponseᶜ5;
        const nint backendStatus = 200;
        slice<byte> requestBody = bytes.Repeat(slice<byte>("a"u8), (1 << (int)(20)));
        var requestBodyʗ1 = requestBody;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            var (slurp, errΔ1) = io.ReadAll((~r).Body);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Backend body read = %v"u8, errΔ1);
            }
            if (len(slurp) != len(requestBodyʗ1)) {
                Ꮡt.Errorf("Backend read %d request body bytes; want %d"u8, len(slurp), len(requestBodyʗ1));
            }
            if (!bytes.Equal(slurp, requestBodyʗ1)) {
                Ꮡt.Error(backendReadWrongRequestˢ); // 1MB; omitting details
            }
            w.Write(slice<byte>(backendResponse));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (postReq, _) = http.NewRequest(postˢ, (~frontend).URL, new httputil_test_package.bytes_ReaderжReader(bytes.NewReader(requestBody)));
        (var res, err) = frontend.Client().Do(postReq);
        if (err != default!) {
            Ꮡt.Fatalf("Do: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        {
            nint g = res.Value.StatusCode;
            nint e = backendStatus; if (g != e) {
                Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
            }
        }
        var (bodyBytes, _) = io.ReadAll((~res).Body);
        {
            @string g = ((@string)bodyBytes);
            @string e = backendResponse; if (g != e) {
                Ꮡt.Errorf("got body %q; expected %q"u8, g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public delegate (ж<http.Response>, error) RoundTripperFunc(ж<http.Request> _Δp0);

public static (ж<http.Response>, error) RoundTrip(this RoundTripperFunc fn, ж<http.Request> Ꮡreq) {
    return fn(Ꮡreq);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFakeTldˢ = "http://fake.tld/"u8;
internal static readonly object bodyNilWantANilBodyˢ = (@string)"Body != nil; want a nil Body"u8;
internal static readonly @string doneTestingTheˢ = "done testing the interesting part; so force a 502 Gateway error"u8;

// Issue 16036: send a Request with a nil Body when possible
public static void TestReverseProxy_NilBody(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (backendURL, _) = url.Parse(httpFakeTldˢ);
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        proxyHandler.Value.Transport = new httputil_internal_test_package.RoundTripperFuncᴠRoundTripper(new RoundTripperFunc((ж<http.Request> req) => {
            if ((~req).Body != default!) {
                Ꮡt.Error(bodyNilWantANilBodyˢ);
            }
            return (default!, errors.New(doneTestingTheˢ));
        }));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (res, err) = frontend.Client().Get((~frontend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != 502) {
            Ꮡt.Errorf("status code = %v; want 502 (Gateway Error)"u8, (~res).Status);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object headerNilWantANonNilˢ = (@string)"Header == nil; want a non-nil Header"u8;

// Issue 33142: always allocate the request headers
public static void TestReverseProxy_AllocatedHeader(ж<testing.T> Ꮡt) {
    var proxyHandler = @new<global::go.net.http.httputil_package.ReverseProxy>();
    proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
    proxyHandler.Value.Director = (ж<http.Request> _) => {
    }; // noop
    proxyHandler.Value.Transport = new httputil_internal_test_package.RoundTripperFuncᴠRoundTripper(new RoundTripperFunc((ж<http.Request> req) => {
        if ((~req).Header == default!) {
            Ꮡt.Error(headerNilWantANonNilˢ);
        }
        return (default!, errors.New(doneTestingTheˢ));
    }));
    proxyHandler.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), Ꮡ(new http.Request(
        Method: "GET"u8,
        URL: Ꮡ(new url.URL(Scheme: "http"u8, Host: "fake.tld"u8, Path: "/"u8)),
        Proto: "HTTP/1.0"u8,
        ProtoMajor: 1
    )));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xHitModˢ = "X-Hit-Mod"u8;

[GoType("dyn")] internal partial struct TestReverseProxyModifyResponse_tests {
    internal @string url;
    internal nint wantCode;
}

// Issue 14237. Test ModifyResponse and that an error from it
// causes the proxy to return StatusBadGateway, or StatusOK otherwise.
public static void TestReverseProxyModifyResponse(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Header().Add(xHitModˢ, fmt.Sprintf("%v"u8, (~(~r).URL).Path == "/mod"u8));
        })));
        var backendServerʗ1 = backendServer;
        defer(backendServerʗ1.Close, ref ᒐ);
        var (rpURL, _) = url.Parse((~backendServer).URL);
        var rproxy = NewSingleHostReverseProxy(rpURL);
        rproxy.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        rproxy.Value.ModifyResponse = error (ж<http.Response> resp) => {
            if ((~resp).Header.Get(xHitModˢ) != "true"u8) {
                return fmt.Errorf("tried to by-pass proxy"u8);
            }
            return default!;
        };
        var frontendProxy = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(rproxy));
        var frontendProxyʗ1 = frontendProxy;
        defer(frontendProxyʗ1.Close, ref ᒐ);
        var tests = new TestReverseProxyModifyResponse_tests[]{
            new((~frontendProxy).URL + "/mod"u8, http.StatusOK),
            new((~frontendProxy).URL + "/schedule"u8, http.StatusBadGateway)
        }.slice();
        foreach (var (i, tt) in tests) {
            var (resp, err) = http.Get(tt.url);
            if (err != default!) {
                Ꮡt.Fatalf("failed to reach proxy: %v"u8, err);
            }
            {
                nint g = resp.Value.StatusCode;
                nint e = tt.wantCode; if (g != e) {
                    Ꮡt.Errorf("#%d: got res.StatusCode %d; expected %d"u8, i, g, e);
                }
            }
            (~resp).Body.Close();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct failingRoundTripper {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someErrorˢ = "some error"u8;

internal static (ж<http.Response>, error) RoundTrip(this failingRoundTripper _Δp0, ж<http.Request> _Δp1) {
    return (default!, errors.New(someErrorˢ));
}

[GoType] internal partial struct staticResponseRoundTripper {
    internal ж<http.Response> res;
}

internal static (ж<http.Response>, error) RoundTrip(this staticResponseRoundTripper rt, ж<http.Request> _) {
    return (rt.res, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someErrorToTriggerˢ = "some error to trigger errorHandler"u8;

[GoType("dyn")] internal partial struct TestReverseProxyErrorHandler_tests {
    internal @string name;
    internal nint wantCode;
    internal Action<http.ResponseWriter, ж<http.Request>, error> errorHandler;
    internal http.RoundTripper transport; // defaults to failingRoundTripper
    internal Func<ж<http.Response>, error> modifyResponse;
}

public static void TestReverseProxyErrorHandler(ж<testing.T> Ꮡt) {
    var tests = new TestReverseProxyErrorHandler_tests[]{
        new(
            name: "default"u8,
            wantCode: http.StatusBadGateway
        ),
        new(
            name: "errorhandler"u8,
            wantCode: http.StatusTeapot,
            errorHandler: (http.ResponseWriter rw, ж<http.Request> req, error err) => {
                rw.WriteHeader(http.StatusTeapot);
            }
        ),
        new(
            name: "modifyresponse_noerr"u8,
            transport: new staticResponseRoundTripper(
                Ꮡ(new http.Response(StatusCode: 345, Body: http.NoBody))
            ),
            modifyResponse: (ж<http.Response> res) => {
                res.Value.StatusCode++;
                return default!;
            },
            errorHandler: (http.ResponseWriter rw, ж<http.Request> req, error err) => {
                rw.WriteHeader(http.StatusTeapot);
            },
            wantCode: 346
        ),
        new(
            name: "modifyresponse_err"u8,
            transport: new staticResponseRoundTripper(
                Ꮡ(new http.Response(StatusCode: 345, Body: http.NoBody))
            ),
            modifyResponse: (ж<http.Response> res) => {
                res.Value.StatusCode++;
                return errors.New(someErrorToTriggerˢ);
            },
            errorHandler: (http.ResponseWriter rw, ж<http.Request> req, error err) => {
                rw.WriteHeader(http.StatusTeapot);
            },
            wantCode: http.StatusTeapot
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestReverseProxyErrorHandler_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var target = Ꮡ(new url.URL(
                    Scheme: "http"u8,
                    Host: "dummy.tld"u8,
                    Path: "/"u8
                ));
                var rproxy = NewSingleHostReverseProxy(target);
                rproxy.Value.Transport = ttʗ1.transport;
                rproxy.Value.ModifyResponse = ttʗ1.modifyResponse;
                if ((~rproxy).Transport == default!) {
                    rproxy.Value.Transport = new failingRoundTripper(nil);
                }
                rproxy.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
                if (ttʗ1.errorHandler != default!) {
                    rproxy.Value.ErrorHandler = ttʗ1.errorHandler;
                }
                var frontendProxy = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(rproxy));
                var frontendProxyʗ1 = frontendProxy;
                defer(frontendProxyʗ1.Close, ref ᒐ);
                var (resp, err) = http.Get((~frontendProxy).URL + "/test"u8);
                if (err != default!) {
                    tΔ1.Fatalf("failed to reach proxy: %v"u8, err);
                }
                {
                    nint g = resp.Value.StatusCode;
                    nint e = ttʗ1.wantCode; if (g != e) {
                        tΔ1.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
                    }
                }
                (~resp).Body.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string thisCallWasRelayedByTheˢ = "this call was relayed by the reverse proxy"u8;
internal static readonly @string contentLengthˢ = "Content-Length"u8;

// Issue 16659: log errors from short read
public static void TestReverseProxy_CopyBuffer(ж<testing.T> Ꮡt) {
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
        var (rpURL, err) = url.Parse((~backendServer).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ref var proxyLog = ref heap(new bytes.Buffer(), out var ᏑproxyLog);
        var rproxy = NewSingleHostReverseProxy(rpURL);
        rproxy.Value.ErrorLog = log.New(new httputil_test_package.bytes_BufferжWriter(ᏑproxyLog), ""u8, log.Lshortfile);
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
            (_, err) = frontendProxy.Client().Get((~frontendProxy).URL); if (err == default!) {
                Ꮡt.Fatalf("want non-nil error"u8);
            }
        }
        // The race detector complains about the proxyLog usage in logf in copyBuffer
        // and our usage below with proxyLog.Bytes() so we're explicitly using a
        // channel to ensure that the ReverseProxy's ServeHTTP is done before we
        // continue after Get.
        ᐸꟷ(donec);
        var expected = new @string[]{
            "EOF"u8,
            "read"u8
        }.slice();
        foreach (var (_, phrase) in expected) {
            if (!bytes.Contains(proxyLog.Bytes(), slice<byte>(phrase))) {
                Ꮡt.Errorf("expected log to contain phrase %q"u8, phrase);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct staticTransport {
    internal ж<http.Response> res;
}

[GoRecv] internal static (ж<http.Response>, error) RoundTrip(this ref staticTransport t, ж<http.Request> Ꮡr) {
    return (t.res, default!);
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
    var r = httptest.NewRequest(getˢ, "/"u8, default!);
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        proxy.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(w), r);
    }
}

[GoType("dyn")] [GoLocalName("result")] internal partial struct TestServeHTTPDeepCopy_result {
    internal @string before, after;
}

public static void TestServeHTTPDeepCopy(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>("Hello Gopher!"u8));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resultChan = new channel<TestServeHTTPDeepCopy_result>(1);
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        var proxyHandlerʗ1 = proxyHandler;
        var resultChanʗ1 = resultChan;
        var frontend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            @string before = (~r).URL.String();
            proxyHandlerʗ1.ServeHTTP(w, r);
            @string after = (~r).URL.String();
            resultChanʗ1.ᐸꟷ(new TestServeHTTPDeepCopy_result(before: before, after: after));
        })));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var want = new TestServeHTTPDeepCopy_result(before: "/"u8, after: "/"u8);
        (var res, err) = frontend.Client().Get((~frontend).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Do: %v"u8, err);
        }
        (~res).Body.Close();
        var got = ᐸꟷ(resultChan);
        if (got != want) {
            Ꮡt.Errorf("got = %+v; want = %+v"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpFooTldˢ = "http://foo.tld/"u8;
internal static readonly @string fromDirectorˢ = "From-Director"u8;

// Issue 18327: verify we always do a deep copy of the Request.Header map
// before any mutations.
public static void TestClonesRequestHeaders(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        log.SetOutput(io.Discard);
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, httpFooTldˢ, default!);
        req.Value.RemoteAddr = "1.2.3.4:56789"u8;
        var rp = Ꮡ(new ReverseProxy(
            Director: (ж<http.Request> reqΔ1) => {
                (~reqΔ1).Header.Set(fromDirectorˢ, "1"u8);
            },
            Transport: new httputil_internal_test_package.roundTripperFuncᴠRoundTripper(new roundTripperFunc((ж<http.Request> reqΔ2) => {
                {
                    @string v = (~reqΔ2).Header.Get(fromDirectorˢ); if (v != "1"u8) {
                        Ꮡt.Errorf("From-Directory value = %q; want 1"u8, v);
                    }
                }
                return (default!, io.EOF);
            }))
        ));
        rp.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), req);
        foreach (var (_, h) in new @string[]{
            "From-Director"u8,
            "X-Forwarded-For"u8
        }.slice()) {
            if ((~req).Header.Get(h) != ""u8) {
                Ꮡt.Errorf("%v header mutation modified caller's request"u8, h);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal delegate (ж<http.Response>, error) roundTripperFunc(ж<http.Request> req);

internal static (ж<http.Response>, error) RoundTrip(this roundTripperFunc fn, ж<http.Request> Ꮡreq) {
    return fn(Ꮡreq);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string modifyResponseErrorˢ = "ModifyResponse error"u8;

public static void TestModifyResponseClosesBody(ж<testing.T> Ꮡt) {
    var (req, _) = http.NewRequest(getˢ, httpFooTldˢ, default!);
    req.Value.RemoteAddr = "1.2.3.4:56789"u8;
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
        nint g = res.Value.StatusCode;
        nint e = http.StatusBadGateway; if (g != e) {
            Ꮡt.Errorf("got res.StatusCode %d; expected %d"u8, g, e);
        }
    }
    if (!(~closeCheck).closed) {
        Ꮡt.Errorf("body should have been closed"u8);
    }
    {
        @string g = logBuf.String();
        @string e = outErr.Error(); if (!strings.Contains(g, e)) {
            Ꮡt.Errorf("ErrorLog %q does not contain %q"u8, g, e);
        }
    }
}

[GoType] internal partial struct checkCloser {
    internal bool closed;
}

[GoRecv] internal static error Close(this ref checkCloser cc) {
    cc.closed = true;
    return default!;
}

[GoRecv] internal static (nint, error) Read(this ref checkCloser cc, slice<byte> b) {
    return (len(b), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object handlerShouldHaveˢ = (@string)"handler should have panicked"u8;
internal static readonly object expectedErrAbortHandlerˢ = (@string)"expected ErrAbortHandler, got"u8;

// Issue 23643: panic on body copy error
public static void TestReverseProxy_PanicBodyError(ж<testing.T> Ꮡt) {
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
        var (rpURL, err) = url.Parse((~backendServer).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rproxy = NewSingleHostReverseProxy(rpURL);
        // Ensure that the handler panics when the body read encounters an
        // io.ErrUnexpectedEOF
        defer(() => {
            var errΔ1 = recover();
            if (errΔ1 == default!) {
                Ꮡt.Fatal(handlerShouldHaveˢ);
            }
            if (!AreEqual(errΔ1, http.ErrAbortHandler)) {
                Ꮡt.Fatal(expectedErrAbortHandlerˢ, errΔ1);
            }
        }, ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, httpFooTldˢ, default!);
        rproxy.ServeHTTP(new httputil_test_package.httptest_ResponseRecorderжResponseWriter(httptest.NewRecorder()), req);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue #46866: panic without closing incoming request body causes a panic
public static void TestReverseProxy_PanicClosesIncomingBody(ж<testing.T> Ꮡt) {
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
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
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
                        var (req, _) = http.NewRequest(postˢ, (~frontendʗ2).URL, new io.LimitedReaderжReader(Ꮡ(new io.LimitedReader(R: ((global::go.net.http.httputil_package.neverEnding)(rune)'x'), N: reqLen))));
                        req.Value.ContentLength = reqLen;
                        var (resp, _) = (~frontendClientʗ1).Transport.RoundTrip(req);
                        if (resp != nil) {
                            io.Copy(io.Discard, (~resp).Body);
                            (~resp).Body.Close();
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

[GoType("dyn")] internal partial struct TestSelectFlushInterval_tests {
    internal @string name;
    internal ж<global::go.net.http.httputil_package.ReverseProxy> p;
    internal ж<http.Response> res;
    internal time.Duration want;
}

public static void TestSelectFlushInterval(ж<testing.T> Ꮡt) {
    var tests = new TestSelectFlushInterval_tests[]{
        new(
            name: "default"u8,
            res: Ꮡ(new http.Response(nil)),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: 123
        ),
        new(
            name: "server-sent events overrides non-zero"u8,
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"u8] = new @string[]{"text/event-stream"u8}.slice()
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: -1
        ),
        new(
            name: "server-sent events overrides zero"u8,
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"u8] = new @string[]{"text/event-stream"u8}.slice()
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 0)),
            want: -1
        ),
        new(
            name: "server-sent events with media-type parameters overrides non-zero"u8,
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"u8] = new @string[]{"text/event-stream;charset=utf-8"u8}.slice()
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: -1
        ),
        new(
            name: "server-sent events with media-type parameters overrides zero"u8,
            res: Ꮡ(new http.Response(
                Header: new httpꓸHeader(new map<@string, slice<@string>>{
                    ["Content-Type"u8] = new @string[]{"text/event-stream;charset=utf-8"u8}.slice()
                })
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 0)),
            want: -1
        ),
        new(
            name: "Content-Length: -1, overrides non-zero"u8,
            res: Ꮡ(new http.Response(
                ContentLength: -1
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 123)),
            want: -1
        ),
        new(
            name: "Content-Length: -1, overrides zero"u8,
            res: Ꮡ(new http.Response(
                ContentLength: -1
            )),
            p: Ꮡ(new ReverseProxy(FlushInterval: 0)),
            want: -1
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestSelectFlushInterval_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = ttʗ1.p.flushInterval(ttʗ1.res);
            if (got != ttʗ1.want) {
                tΔ1.Errorf("flushLatency = %v; want %v"u8, got, ttʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedBackendRequestˢ = (@string)"unexpected backend request"u8;
internal static readonly @string unexpectedRequestˢ = "unexpected request"u8;
internal static readonly @string http11101Switchingˢ = "HTTP/1.1 101 Switching Protocols\r\nConnection: upgrade\r\nUpgrade: WebSocket\r\n\r\n"u8;
internal static readonly @string xModifiedˢ = "X-Modified"u8;
internal static readonly @string trueˢ = "true"u8;
internal static readonly @string xHeaderˢ = "X-Header"u8;
internal static readonly @string xValueˢ = "X-Value"u8;
internal static readonly @string websocketˢ = "websocket"u8;
internal static readonly @string helloˢ = "Hello\n"u8;
internal static readonly @string backendGotHelloˢ = @"backend got ""Hello"""u8;

public static void TestReverseProxyWebSocket(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backendServer = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            GoFrame ᒐ = default;
            try {
                if (upgradeType((~r).Header) != "websocket"u8) {
                    Ꮡt.Error(unexpectedBackendRequestˢ);
                    http.Error(w, unexpectedRequestˢ, 400);
                    return;
                }
                var (cΔ1, _, errΔ1) = w._<http.Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var cʗ1 = cΔ1;
                defer(() => cʗ1.Close(), ref ᒐ);
                io.WriteString(new httputil_test_package.net_ConnᴠWriter(cΔ1), http11101Switchingˢ);
                var bsΔ1 = bufio.NewScanner(new httputil_test_package.net_ConnᴠReader(cΔ1));
                if (!bsΔ1.Scan()) {
                    Ꮡt.Errorf("backend failed to read line from client: %v"u8, bsΔ1.Err());
                    return;
                }
                fmt.Fprintf(new httputil_test_package.net_ConnᴠWriter(cΔ1), "backend got %q\n"u8, bsΔ1.Text());
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })));
        var backendServerʗ1 = backendServer;
        defer(backendServerʗ1.Close, ref ᒐ);
        var (backURL, _) = url.Parse((~backendServer).URL);
        var rproxy = NewSingleHostReverseProxy(backURL);
        rproxy.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        rproxy.Value.ModifyResponse = error (ж<http.Response> resΔ1) => {
            (~resΔ1).Header.Add(xModifiedˢ, trueˢ);
            return default!;
        };
        var rproxyʗ1 = rproxy;
        var handler = new http.HandlerFunc((http.ResponseWriter rw, ж<http.Request> reqΔ1) => {
            rw.Header().Set(xHeaderˢ, xValueˢ);
            rproxyʗ1.ServeHTTP(rw, reqΔ1);
            {
                @string gotΔ1 = rw.Header().Get(xModifiedˢ);
                @string wantΔ1 = trueˢ; if (gotΔ1 != wantΔ1) {
                    Ꮡt.Errorf("response writer X-Modified header = %q; want %q"u8, gotΔ1, wantΔ1);
                }
            }
        });
        var frontendProxy = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(handler));
        var frontendProxyʗ1 = frontendProxy;
        defer(frontendProxyʗ1.Close, ref ᒐ);
        var (req, _) = http.NewRequest(getˢ, (~frontendProxy).URL, default!);
        (~req).Header.Set(connectionˢ, upgradeˢ);
        (~req).Header.Set(upgradeˢ, websocketˢ);
        var c = frontendProxy.Client();
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != 101) {
            Ꮡt.Fatalf("status = %v; want 101"u8, (~res).Status);
        }
        @string got = (~res).Header.Get(xHeaderˢ);
        @string want = xValueˢ;
        if (got != want) {
            Ꮡt.Errorf("Header(XHeader) = %q; want %q"u8, got, want);
        }
        if (!ascii.EqualFold(upgradeType((~res).Header), websocketˢ)) {
            Ꮡt.Fatalf("not websocket upgrade; got %#v"u8, (~res).Header);
        }
        var (rwc, ok) = (~res).Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("response body is of type %T; does not implement ReadWriteCloser"u8, (~res).Body);
        }
        var rwcʗ1 = rwc;
        defer(() => rwcʗ1.Close(), ref ᒐ);
        {
            @string gotΔ2 = (~res).Header.Get(xModifiedˢ);
            @string wantΔ2 = trueˢ; if (gotΔ2 != wantΔ2) {
                Ꮡt.Errorf("response X-Modified header = %q; want %q"u8, gotΔ2, wantΔ2);
            }
        }
        io.WriteString(rwc, helloˢ);
        var bs = bufio.NewScanner(rwc);
        if (!bs.Scan()) {
            Ꮡt.Fatalf("Scan: %v"u8, bs.Err());
        }
        got = bs.Text();
        want = backendGotHelloˢ;
        if (got != want) {
            Ꮡt.Errorf("got %#q, want %#q"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string finalMessageˢ = "final message"u8;
internal static readonly @string unexpectedRequestˢ2 = "Unexpected request"u8;

public static void TestReverseProxyWebSocketCancellation(ж<testing.T> Ꮡt) {
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
                    @string g = upgradeType((~r).Header);
                    @string ws = websocketˢ; if (g != ws) {
                        Ꮡt.Errorf("Unexpected upgrade type %q, want %q"u8, g, ws);
                        http.Error(w, unexpectedRequestˢ2, 400);
                        return;
                    }
                }
                var (conn, bufrw, errΔ1) = w._<http.Hijacker>().Hijack();
                if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                    return;
                }
                var connʗ1 = conn;
                defer(() => connʗ1.Close(), ref ᒐ);
                @string upgradeMsg = http11101Switchingˢ;
                {
                    var (_, errΔ2) = io.WriteString(new httputil_test_package.net_ConnᴠWriter(conn), upgradeMsg); if (errΔ2 != default!) {
                        Ꮡt.Error(errΔ2);
                        return;
                    }
                }
                {
                    var (_, _, errΔ3) = bufrw.Value.Reader.Value.ReadLine(); if (errΔ3 != default!) {
                        Ꮡt.Errorf("Failed to read line from client: %v"u8, errΔ3);
                        return;
                    }
                }
                for (nint i = 0; i < n; i++) {
                    {
                        var (_, errΔ4) = bufrw.Value.Writer.Value.WriteString(nthResponseʗ1(i)); if (errΔ4 != default!) {
                            var selᴛ5 = triggerCancelChʗ1;
                            switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
                            case 0 when selᴛ5.ꟷᐳ(out _): {
                                break;
                            }
                            default: {
                                Ꮡt.Errorf("Writing response #%d failed: %v"u8, i, errΔ4);
                                break;
                            }}
                            return;
                        }
                    }
                    bufrw.Value.Writer.Value.Flush();
                    time.Sleep(time.ΔSecond);
                }
                {
                    var (_, errΔ5) = bufrw.Value.Writer.Value.WriteString(terminalMsg); if (errΔ5 != default!) {
                        var selᴛ6 = triggerCancelChʗ1;
                        switch (trySelect(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
                        case 0 when selᴛ6.ꟷᐳ(out _): {
                            break;
                        }
                        default: {
                            Ꮡt.Errorf("Failed to write terminal message: %v"u8, errΔ5);
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
        var (backendURL, _) = url.Parse((~cst).URL);
        var rproxy = NewSingleHostReverseProxy(backendURL);
        rproxy.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        rproxy.Value.ModifyResponse = error (ж<http.Response> resΔ1) => {
            (~resΔ1).Header.Add(xModifiedˢ, trueˢ);
            return default!;
        };
        var rproxyʗ1 = rproxy;
        var triggerCancelChʗ2 = triggerCancelCh;
        var handler = new http.HandlerFunc((http.ResponseWriter rw, ж<http.Request> reqΔ1) => {
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
        var (req, _) = http.NewRequest(getˢ, (~frontendProxy).URL, default!);
        (~req).Header.Set(connectionˢ, upgradeˢ);
        (~req).Header.Set(upgradeˢ, websocketˢ);
        var (res, err) = frontendProxy.Client().Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Dialing to frontend proxy: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        {
            nint g = res.Value.StatusCode;
            nint w = 101; if (g != w) {
                Ꮡt.Fatalf("Switching protocols failed, got: %d, want: %d"u8, g, w);
            }
        }
        {
            @string g = (~res).Header.Get(xHeaderˢ);
            @string w = xValueˢ; if (g != w) {
                Ꮡt.Errorf("X-Header mismatch\n\tgot:  %q\n\twant: %q"u8, g, w);
            }
        }
        {
            @string g = upgradeType((~res).Header);
            @string w = websocketˢ; if (!ascii.EqualFold(g, w)) {
                Ꮡt.Fatalf("Upgrade header mismatch\n\tgot:  %q\n\twant: %q"u8, g, w);
            }
        }
        var (rwc, ok) = (~res).Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("Response body type mismatch, got %T, want io.ReadWriteCloser"u8, (~res).Body);
        }
        {
            @string got = (~res).Header.Get(xModifiedˢ);
            @string want = trueˢ; if (got != want) {
                Ꮡt.Errorf("response X-Modified header = %q; want %q"u8, got, want);
            }
        }
        {
            var (_, errΔ6) = io.WriteString(rwc, helloˢ); if (errΔ6 != default!) {
                Ꮡt.Fatalf("Failed to write first message: %v"u8, errΔ6);
            }
        }
        // Read loop.
        var br = bufio.NewReader(rwc);
        while (ᐧ) {
            var (line, errΔ7) = br.ReadString((rune)'\n');
            switch (ᐧ) {
            case {} when line == terminalMsg: {
                Ꮡt.Fatalf("The websocket request was not canceled, unfortunately!"u8);
                break;
            }
            case {} when AreEqual(errΔ7, io.EOF): {
                return;
            }
            case {} when errΔ7 != default!: {
                Ꮡt.Fatalf("Unexpected error: %v"u8, // this case before "err == io.EOF"
 errΔ7);
                break;
            }
            case {} when line == nthResponse(0): {
                close(triggerCancelCh);
                break;
            }}

        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// We've gotten the first response back
// Let's trigger a cancel.
public static void TestUnannouncedTrailer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.WriteHeader(http.StatusOK);
            w._<http.Flusher>().Flush();
            w.Header().Set(http.TrailerPrefix + "X-Unannounced-Trailer", unannouncedTrailerValueˢ);
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();
        (var res, err) = frontendClient.Get((~frontend).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        io.ReadAll((~res).Body);
        (~res).Body.Close();
        {
            @string g = (~res).Trailer.Get(xUnannouncedTrailerˢ);
            @string w = unannouncedTrailerValueˢ; if (g != w) {
                Ꮡt.Errorf("Trailer(X-Unannounced-Trailer) = %q; want %q"u8, g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSetURL(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>((~r).Host));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
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
        (var res, err) = frontendClient.Get((~frontend).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var body, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatalf("Reading body: %v"u8, err);
        }
        {
            @string got = ((@string)body);
            @string want = backendURL.Value.Host; if (got != want) {
                Ꮡt.Errorf("backend got Host %q, want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestSingleJoinSlash_tests {
    internal @string slasha;
    internal @string slashb;
    internal @string expected;
}

public static void TestSingleJoinSlash(ж<testing.T> Ꮡt) {
    var tests = new TestSingleJoinSlash_tests[]{
        new("https://www.google.com/"u8, "/favicon.ico"u8, "https://www.google.com/favicon.ico"u8),
        new("https://www.google.com"u8, "/favicon.ico"u8, "https://www.google.com/favicon.ico"u8),
        new("https://www.google.com"u8, "favicon.ico"u8, "https://www.google.com/favicon.ico"u8),
        new("https://www.google.com"u8, ""u8, "https://www.google.com/"u8),
        new(""u8, "favicon.ico"u8, "/favicon.ico"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            @string got = singleJoiningSlash(tt.slasha, tt.slashb); if (got != tt.expected) {
                Ꮡt.Errorf("singleJoiningSlash(%q,%q) want %q got %q"u8,
                    tt.slasha,
                    tt.slashb,
                    tt.expected,
                    got);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestJoinURLPath_tests {
    internal ж<url.URL> a;
    internal ж<url.URL> b;
    internal @string wantPath;
    internal @string wantRaw;
}

public static void TestJoinURLPath(ж<testing.T> Ꮡt) {
    var tests = new TestJoinURLPath_tests[]{
        new(Ꮡ(new url.URL(Path: "/a/b"u8)), Ꮡ(new url.URL(Path: "/c"u8)), "/a/b/c"u8, ""u8),
        new(Ꮡ(new url.URL(Path: "/a/b"u8, RawPath: "badpath"u8)), Ꮡ(new url.URL(Path: "c"u8)), "/a/b/c"u8, "/a/b/c"u8),
        new(Ꮡ(new url.URL(Path: "/a/b"u8, RawPath: "/a%2Fb"u8)), Ꮡ(new url.URL(Path: "/c"u8)), "/a/b/c"u8, "/a%2Fb/c"u8),
        new(Ꮡ(new url.URL(Path: "/a/b"u8, RawPath: "/a%2Fb"u8)), Ꮡ(new url.URL(Path: "/c"u8)), "/a/b/c"u8, "/a%2Fb/c"u8),
        new(Ꮡ(new url.URL(Path: "/a/b/"u8, RawPath: "/a%2Fb%2F"u8)), Ꮡ(new url.URL(Path: "c"u8)), "/a/b//c"u8, "/a%2Fb%2F/c"u8),
        new(Ꮡ(new url.URL(Path: "/a/b/"u8, RawPath: "/a%2Fb/"u8)), Ꮡ(new url.URL(Path: "/c/d"u8, RawPath: "/c%2Fd"u8)), "/a/b/c/d"u8, "/a%2Fb/c%2Fd"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (p, rp) = joinURLPath(tt.a, tt.b);
        if (p != tt.wantPath || rp != tt.wantRaw) {
            Ꮡt.Errorf("joinURLPath(URL(%q,%q),URL(%q,%q)) want (%q,%q) got (%q,%q)"u8,
                (~tt.a).Path, (~tt.a).RawPath,
                (~tt.b).Path, (~tt.b).RawPath,
                tt.wantPath, tt.wantRaw,
                p, rp);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ = "response_content"u8;

public static void TestReverseProxyRewriteReplacesOut(ж<testing.T> Ꮡt) {
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
                (r.Value.Out, _) = http.NewRequest(getˢ, (~backendʗ2).URL, default!);
            }
        ));
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var (res, err) = frontend.Client().Get((~frontend).URL);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        var (body, _) = io.ReadAll((~res).Body);
        {
            @string got = ((@string)body);
            @string want = content; if (got != want) {
                Ꮡt.Errorf("got response %q, want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpGoDevˢ = "http://go.dev/"u8;

public static void Test1xxHeadersNotModifiedAfterRoundTrip(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

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
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var rw = Ꮡ(new testResponseWriter(nil));
        var proxyHandlerʗ1 = proxyHandler;
        var rwʗ1 = rw;
        ((Action)(() => {
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
                        return default!;
                    }
                )));
                var (req, _) = http.NewRequestWithContext(ctx, getˢ, httpGoDevˢ, default!);
                proxyHandlerʗ1.ServeHTTP(new httputil_internal_test_package.testResponseWriterжResponseWriter(rwʗ1), req);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
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
internal static readonly @string linkˢ = "Link"u8;
internal static readonly @string styleCssRelPreloadAsˢ = "</style.css>; rel=preload; as=style"u8;
internal static readonly @string scriptJsRelPreloadAsˢ = "</script.js>; rel=preload; as=script"u8;
internal static readonly @string fooJsRelPreloadAsScriptˢ = "</foo.js>; rel=preload; as=script"u8;
internal static readonly object unexpected1xxResponseˢ = (@string)"Unexpected 1xx response"u8;

public static void Test1xxResponses(ж<testing.T> Ꮡt) {
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
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = NewSingleHostReverseProxy(backendURL);
        proxyHandler.Value.ErrorLog = log.New(io.Discard, ""u8, 0); // quiet for tests
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        var frontendClient = frontend.Client();
        void checkLinkHeaders(ж<testing.T> tΔ1, slice<@string> expected, slice<@string> got) {
            tΔ1.Helper();
            if (len(expected) != len(got)) {
                tΔ1.Errorf("Expected %d link headers; got %d"u8, len(expected), len(got));
            }
            foreach (var (i, _) in expected) {
                if (i >= len(got)) {
                    tΔ1.Errorf("Expected %q link header; got nothing"u8, expected[i]);
                    continue;
                }
                if (expected[i] != got[i]) {
                    tΔ1.Errorf("Expected %q link header; got %q"u8, expected[i], got[i]);
                }
            }
        }
        uint8 respCounter = default!;
            var checkLinkHeadersʗ1 = checkLinkHeaders;
        var trace = Ꮡ(new httptrace.ClientTrace(
            Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
                var exprᴛ1 = code;
                if (exprᴛ1 == http.StatusEarlyHints) {
                    checkLinkHeadersʗ1(Ꮡt, new @string[]{"</style.css>; rel=preload; as=style"u8, "</script.js>; rel=preload; as=script"u8}.slice(), header[linkˢ]);
                }
                else if (exprᴛ1 == http.StatusProcessing) {
                    checkLinkHeadersʗ1(Ꮡt, new @string[]{"</style.css>; rel=preload; as=style"u8, "</script.js>; rel=preload; as=script"u8, "</foo.js>; rel=preload; as=script"u8}.slice(), header[linkˢ]);
                }
                else { /* default: */
                    Ꮡt.Error(unexpected1xxResponseˢ);
                }

                respCounter++;
                return default!;
            }
        ));
        var (req, _) = http.NewRequestWithContext(httptrace.WithClientTrace(context.Background(), trace), getˢ, (~frontend).URL, default!);
        (var res, err) = frontendClient.Do(req);
        if (err != default!) {
            Ꮡt.Fatalf("Get: %v"u8, err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if (respCounter != 2) {
            Ꮡt.Errorf("Expected 2 1xx responses; got %d"u8, respCounter);
        }
        checkLinkHeaders(Ꮡt, new @string[]{"</style.css>; rel=preload; as=style"u8, "</script.js>; rel=preload; as=script"u8, "</foo.js>; rel=preload; as=script"u8}.slice(), (~res).Header[linkˢ]);
        var (body, _) = io.ReadAll((~res).Body);
        if (((sstring)body) != "Hello"u8) {
            Ꮡt.Errorf("Read body %q; want Hello"u8, body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal const bool testWantsCleanQuery = true;
internal const bool testWantsRawQuery = false;

public static void TestReverseProxyQueryParameterSmugglingDirectorDoesNotParseForm(ж<testing.T> Ꮡt) {
    testReverseProxyQueryParameterSmuggling(Ꮡt, testWantsRawQuery, (ж<url.URL> u) => {
        var proxyHandler = NewSingleHostReverseProxy(u);
        var oldDirector = proxyHandler.Value.Director;
        var oldDirectorʗ1 = oldDirector;
        proxyHandler.Value.Director = (ж<http.Request> r) => {
            oldDirectorʗ1(r);
        };
        return proxyHandler;
    });
}

public static void TestReverseProxyQueryParameterSmugglingDirectorParsesForm(ж<testing.T> Ꮡt) {
    testReverseProxyQueryParameterSmuggling(Ꮡt, testWantsCleanQuery, (ж<url.URL> u) => {
        var proxyHandler = NewSingleHostReverseProxy(u);
        var oldDirector = proxyHandler.Value.Director;
        var oldDirectorʗ1 = oldDirector;
        proxyHandler.Value.Director = (ж<http.Request> r) => {
            // Parsing the form causes ReverseProxy to remove unparsable
            // query parameters before forwarding.
            r.FormValue("a"u8);
            oldDirectorʗ1(r);
        };
        return proxyHandler;
    });
}

public static void TestReverseProxyQueryParameterSmugglingRewrite(ж<testing.T> Ꮡt) {
    testReverseProxyQueryParameterSmuggling(Ꮡt, testWantsCleanQuery, (ж<url.URL> u) => Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(u);
            }
        )));
}

public static void TestReverseProxyQueryParameterSmugglingRewritePreservesRawQuery(ж<testing.T> Ꮡt) {
    testReverseProxyQueryParameterSmuggling(Ꮡt, testWantsRawQuery, (ж<url.URL> u) => Ꮡ(new ReverseProxy(
            Rewrite: (ж<global::go.net.http.httputil_package.ProxyRequest> r) => {
                r.SetURL(u);
                r.Value.Out.Value.URL.Value.RawQuery = r.Value.In.Value.URL.Value.RawQuery;
            }
        )));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ1 = "response_content"u8;

[GoType("dyn")] internal partial struct testReverseProxyQueryParameterSmuggling_type {
    internal @string rawQuery;
    internal @string cleanQuery;
}

internal static void testReverseProxyQueryParameterSmuggling(ж<testing.T> Ꮡt, bool wantCleanQuery, Func<ж<url.URL>, ж<global::go.net.http.httputil_package.ReverseProxy>> newProxy) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string content = contentᶜ1;
        var backend = httptest.NewServer(new httputil_test_package.http_HandlerFuncᴠΔHandler(new http.HandlerFunc((http.ResponseWriter w, ж<http.Request> r) => {
            w.Write(slice<byte>((~(~r).URL).RawQuery));
        })));
        var backendʗ1 = backend;
        defer(backendʗ1.Close, ref ᒐ);
        var (backendURL, err) = url.Parse((~backend).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var proxyHandler = newProxy(backendURL);
        var frontend = httptest.NewServer(new httputil_test_package.httputil_ReverseProxyжΔHandler(proxyHandler));
        var frontendʗ1 = frontend;
        defer(frontendʗ1.Close, ref ᒐ);
        // Don't spam output with logs of queries containing semicolons.
        backend.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        frontend.Value.Config.Value.ErrorLog = log.New(io.Discard, ""u8, 0);
        foreach (var (_, test) in new testReverseProxyQueryParameterSmuggling_type[]{new(
            rawQuery: "a=1&a=2;b=3"u8,
            cleanQuery: "a=1"u8
        ), new(
            rawQuery: "a=1&a=%zz&b=3"u8,
            cleanQuery: "a=1&b=3"u8
        )
        }.slice()) {
            var (res, errΔ1) = frontend.Client().Get((~frontend).URL + "?"u8 + test.rawQuery);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("Get: %v"u8, errΔ1);
            }
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            var (body, _) = io.ReadAll((~res).Body);
            @string wantQuery = test.rawQuery;
            if (wantCleanQuery) {
                wantQuery = test.cleanQuery;
            }
            {
                @string got = ((@string)body);
                @string want = wantQuery; if (got != want) {
                    Ꮡt.Errorf("proxy forwarded raw query %q as %q, want %q"u8, test.rawQuery, got, want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct testResponseWriter {
    internal httpꓸHeader h;
    internal Action<nint> writeHeader;
    internal Func<slice<byte>, (nint, error)> write;
}

[GoRecv] internal static httpꓸHeader Header(this ref testResponseWriter rw) {
    if (rw.h == default!) {
        rw.h = new httpꓸHeader(0);
    }
    return rw.h;
}

[GoRecv] internal static void WriteHeader(this ref testResponseWriter rw, nint statusCode) {
    if (rw.writeHeader != default!) {
        rw.writeHeader(statusCode);
    }
}

[GoRecv] internal static (nint, error) Write(this ref testResponseWriter rw, slice<byte> p) {
    if (rw.write != default!) {
        return rw.write(p);
    }
    return (len(p), default!);
}

} // end httputil_internal_test_package
