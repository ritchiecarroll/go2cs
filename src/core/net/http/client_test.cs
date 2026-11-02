// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests for client.go
namespace go.net;

using bytes = bytes_package;
using context = context_package;
using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = global::go.@internal.testenv_package;
using io = io_package;
using log = log_package;
using net = net_package;
using static global::go.net.http_package;
using cookiejar = global::go.net.http.cookiejar_package;
using httptest = global::go.net.http.httptest_package;
using url = global::go.net.url_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using crypto;
using encoding;
using global::go.@internal;
using global::go.net;
using global::go.net.http;
using global::go.sync;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;
using ꓸꓸꓸany = Span<any>;

partial class http_test_package {

internal static Δhttp.HandlerFunc robotsTxtHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    w.Header().Set("Last-Modified", "sometime");
    fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "User-agent: go\nDisallow: /something/"u8);
});

// pedanticReadAll works like io.ReadAll but additionally
// verifies that r obeys the documented io.Reader contract.
internal static (slice<byte> b, error err) pedanticReadAll(io.Reader r) {
    slice<byte> b = default;

    array<byte> bufa = new(64);
    var buf = bufa[..];
    for (;;) {
        var (n, errΔ1) = r.Read(buf);
        if (n == 0 && errΔ1 == nil) {
            return (default, fmt.Errorf("Read: n=0 with err=nil"u8));
        }
        b = appendꓸꓸꓸ(b, buf.slice(0, n));
        if (AreEqual(errΔ1, io.EOF)) {
            var (nΔ1, errΔ2) = r.Read(buf);
            if (nΔ1 != 0 || !AreEqual(errΔ2, io.EOF)) {
                return (default, fmt.Errorf("Read: n=%d err=%#v after EOF"u8, nΔ1, errΔ2));
            }
            return (b, default);
        }
        if (errΔ1 != nil) {
            return (b, errΔ1);
        }
    }
}

public static void TestClient(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClient(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string userAgentˢ => ref userAgentˢᵇ;
private static readonly @string userAgentˢᵇ = "User-agent:"u8;

internal static void testClient(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(robotsTxtHandler)).ts;

    var c = ts.Client();
    var (r, err) = c.Get(ts.URL);
    slice<byte> b = default;
    if (err == nil) {
        (b, err) = pedanticReadAll(new http_test_package.io_ReadCloserᴠReader(r.Body));
        r.Body.Close();
    }
    if (err != nil) {
        t.Error(err);
    } else {
        @string s = (@string)b; if (!strings.HasPrefix(s, userAgentˢ)) {
            t.Errorf("Incorrect page body (did not begin with User-agent): %q", s);
        }
    }
}

public static void TestClientHead(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientHead(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lastModifiedˢ => ref lastModifiedˢᵇ;
private static readonly @string lastModifiedˢᵇ = "Last-Modified"u8;
internal static readonly object lastModifiedHeaderNotˢ = (@string)"Last-Modified header not found."u8;

internal static void testClientHead(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(robotsTxtHandler));
    var (r, err) = cst.c.Head(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var (_, ok) = r.Header[lastModifiedˢ, ꟷ]; if (!ok) {
            t.Error(lastModifiedHeaderNotˢ);
        }
    }
}

partial struct recordingTransport {
    internal ж<Δhttp.Request> req;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dummyImplˢ => ref dummyImplˢᵇ;
private static readonly @string dummyImplˢᵇ = "dummy impl"u8;

internal static (ж<Δhttp.Response> resp, error err) RoundTrip(this ref recordingTransport t, ж<Δhttp.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    t.req = Ꮡreq;
    return (default, errors.New(dummyImplˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpDummyFaketldˢ => ref httpDummyFaketldˢᵇ;
private static readonly @string httpDummyFaketldˢᵇ = "http://dummy.faketld/"u8;

public static void TestGetRequestFormat(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        setParallel(t);
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var tr = Ꮡ(new recordingTransport());
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        @string url = httpDummyFaketldˢ;
        client.Get(url); // Note: doesn't hit network
        if (tr.req.Method != "GET"u8) {
            t.Errorf("expected method %q; got %q", getˢ2, tr.req.Method);
        }
        if (tr.req.URL.String() != url) {
            t.Errorf("expected URL %q; got %q", url, tr.req.URL.String());
        }
        if (tr.req.Header == nil) {
            t.Errorf("expected non-nil request Header");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string keyValueˢ => ref keyValueˢᵇ;
private static readonly @string keyValueˢᵇ = @"{""key"":""value""}"u8;
internal static ref readonly @string applicationJsonˢ => ref applicationJsonˢᵇ;
private static readonly @string applicationJsonˢᵇ = "application/json"u8;
internal static ref readonly @string postˢ => ref postˢᵇ;
private static readonly @string postˢᵇ = "POST"u8;
internal static readonly object gotCloseTrueWantFalseˢ = (@string)"got Close true, want false"u8;

public static void TestPostRequestFormat(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var tr = Ꮡ(new recordingTransport());
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));

        @string url = httpDummyFaketldˢ;
        @string json = keyValueˢ;
        var b = strings.NewReader(json);
        client.Post(url, applicationJsonˢ, new http_test_package.strings_ReaderжReader(b)); // Note: doesn't hit network

        if (tr.req.Method != "POST"u8) {
            t.Errorf("got method %q, want %q", tr.req.Method, postˢ);
        }
        if (tr.req.URL.String() != url) {
            t.Errorf("got URL %q, want %q", tr.req.URL.String(), url);
        }
        if (tr.req.Header == nil) {
            t.Fatalf("expected non-nil request Header");
        }
        if (tr.req.Close) {
            t.Error(gotCloseTrueWantFalseˢ);
        }
        {
            var (g, e) = (tr.req.ContentLength, (int64)len(json)); if (g != e) {
                t.Errorf("got ContentLength %d, want %d", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static ref readonly @string bar2ˢ => ref bar2ˢᵇ;
private static readonly @string bar2ˢᵇ = "bar2"u8;
internal static ref readonly @string bazˢ => ref bazˢᵇ;
private static readonly @string bazˢᵇ = "baz"u8;
internal static ref readonly @string contentTypeˢ => ref contentTypeˢᵇ;
private static readonly @string contentTypeˢᵇ = "Content-Type"u8;
internal static ref readonly @string applicationXWwwFormˢ => ref applicationXWwwFormˢᵇ;
private static readonly @string applicationXWwwFormˢᵇ = "application/x-www-form-urlencoded"u8;
internal static ref readonly @string fooBarFooBar2BarBazˢ => ref fooBarFooBar2BarBazˢᵇ;
private static readonly @string fooBarFooBar2BarBazˢᵇ = "foo=bar&foo=bar2&bar=baz"u8;
internal static ref readonly @string barBazFooBarFooBar2ˢ => ref barBazFooBarFooBar2ˢᵇ;
private static readonly @string barBazFooBarFooBar2ˢᵇ = "bar=baz&foo=bar&foo=bar2"u8;

public static void TestPostFormRequestFormat(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var tr = Ꮡ(new recordingTransport());
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));

        @string urlStr = httpDummyFaketldˢ;
        var form = new url.Values(0);
        form.Set(fooˢ, barˢ);
        form.Add(fooˢ, bar2ˢ);
        form.Set(barˢ, bazˢ);
        client.PostForm(urlStr, form); // Note: doesn't hit network

        if (tr.req.Method != "POST"u8) {
            t.Errorf("got method %q, want %q", tr.req.Method, postˢ);
        }
        if (tr.req.URL.String() != urlStr) {
            t.Errorf("got URL %q, want %q", tr.req.URL.String(), urlStr);
        }
        if (tr.req.Header == nil) {
            t.Fatalf("expected non-nil request Header");
        }
        {
            @string g = tr.req.Header.Get(contentTypeˢ);
            @string e = applicationXWwwFormˢ; if (g != e) {
                t.Errorf("got Content-Type %q, want %q", g, e);
            }
        }
        if (tr.req.Close) {
            t.Error(gotCloseTrueWantFalseˢ);
        }
        // Depending on map iteration, body can be either of these.
        @string expectedBody = fooBarFooBar2BarBazˢ;
        @string expectedBody1 = barBazFooBarFooBar2ˢ;
        {
            var (g, e) = (tr.req.ContentLength, (int64)len(expectedBody)); if (g != e) {
                t.Errorf("got ContentLength %d, want %d", g, e);
            }
        }
        var (bodyb, err) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader(tr.req.Body));
        if (err != nil) {
            t.Fatalf("ReadAll on req.Body: %v", err);
        }
        {
            @string g = (@string)bodyb; if (g != expectedBody && g != expectedBody1) {
                t.Errorf("got body %q, want %q or %q", g, expectedBody, expectedBody1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClientRedirects(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirects(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getN10StoppedAfter10ˢ => ref getN10StoppedAfter10ˢᵇ;
private static readonly @string getN10StoppedAfter10ˢᵇ = @"Get ""/?n=10"": stopped after 10 redirects"u8;
internal static ref readonly @string headN10StoppedAfter10ˢ => ref headN10StoppedAfter10ˢᵇ;
private static readonly @string headN10StoppedAfter10ˢᵇ = @"Head ""/?n=10"": stopped after 10 redirects"u8;
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;
internal static ref readonly @string n15ˢ => ref n15ˢᵇ;
private static readonly @string n15ˢᵇ = "/?n=15"u8;
internal static ref readonly @string headˢ => ref headˢᵇ;
private static readonly @string headˢᵇ = "HEAD"u8;
internal static readonly object didnTSeeRedirectˢ = (@string)"didn't see redirect"u8;
internal static ref readonly @string noRedirectsAllowedˢ => ref noRedirectsAllowedˢᵇ;
private static readonly @string noRedirectsAllowedˢᵇ = "no redirects allowed"u8;
internal static ref readonly @string locationˢ => ref locationˢᵇ;
private static readonly @string locationˢᵇ = "Location"u8;

internal static void testClientRedirects(ж<testing.T> t, testMode mode) {
    ref var ts = ref heap<ж<httptest.Server>>(out var Ꮡts);
    ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (n, _) = strconv.Atoi(r.FormValue("n"));
        // Test Referer header. (7 is arbitrary position to test at)
        if (n == 7) {
            {
                @string g = r.Referer();
                @string e = Ꮡts.ValueSlot.URL + "/?n=6"u8; if (e != g) {
                    t.Errorf("on request ?n=7, expected referer of %q; got %q", e, g);
                }
            }
        }
        if (n < 15) {
            Redirect(w, r, fmt.Sprintf("/?n=%d"u8, n + 1), StatusTemporaryRedirect);
            return;
        }
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "n=%d"u8, n);
    }))).ts;

    var c = ts.Client();
    var (_, err) = c.Get(ts.URL);
    {
        @string e = getN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            t.Errorf("with default client Get, expected error %q, got %q", e, g);
        }
    }

    // HEAD request should also have the ability to follow redirects.
    (_, err) = c.Head(ts.URL);
    {
        @string e = headN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            t.Errorf("with default client Head, expected error %q, got %q", e, g);
        }
    }

    // Do should also follow redirects.
    var (greq, _) = NewRequest(getˢ2, ts.URL, default);
    (_, err) = c.Do(greq);
    {
        @string e = getN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            t.Errorf("with default client Do, expected error %q, got %q", e, g);
        }
    }

    // Requests with an empty Method should also redirect (Issue 12705)
    greq.Method = ""u8;
    (_, err) = c.Do(greq);
    {
        @string e = getN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            t.Errorf("with default client Do and empty Method, expected error %q, got %q", e, g);
        }
    }

    ref var checkErr = ref heap<error>(out var ᏑcheckErr);
    ref var lastVia = ref heap<slice<ж<Δhttp.Request>>>(out var ᏑlastVia);
    ref var lastReq = ref heap<ж<Δhttp.Request>>(out var ᏑlastReq);
    c.CheckRedirect = (ж<Δhttp.Request> req, slice<ж<Δhttp.Request>> via) => {
        ᏑlastReq.ValueSlot = req;
        ᏑlastVia.ValueSlot = via;
        return ᏑcheckErr.ValueSlot;
    };
    (var res, err) = c.Get(ts.URL);
    if (err != nil) {
        t.Fatalf("Get error: %v", err);
    }
    res.Body.Close();
    @string finalURL = res.Request.URL.String();
    {
        @string e = nilˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            t.Errorf("with custom client, expected error %q, got %q", e, g);
        }
    }
    if (!strings.HasSuffix(finalURL, n15ˢ)) {
        t.Errorf("expected final url to end in /?n=15; got url %q", finalURL);
    }
    {
        nint e = 15;
        nint g = len(lastVia); if (e != g) {
            t.Errorf("expected lastVia to have contained %d elements; got %d", e, g);
        }
    }

    // Test that Request.Cancel is propagated between requests (Issue 14053)
    var (creq, _) = NewRequest(headˢ, ts.URL, default);
    var cancel = new channel<EmptyStruct>(0);
    creq.Cancel = cancel.AsRecvOnly();
    {
        var (_, errΔ1) = c.Do(creq); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    if (lastReq == nil) {
        t.Fatal(didnTSeeRedirectˢ);
    }
    if (lastReq.Cancel != cancel) {
        t.Errorf("expected lastReq to have the cancel channel set on the initial req");
    }

    checkErr = errors.New(noRedirectsAllowedˢ);
    (res, err) = c.Get(ts.URL);
    {
        var (urlError, ok) = err._<ж<urlꓸError>>(ᐧ); if (!ok || !AreEqual(urlError.Err, checkErr)) {
            t.Errorf("with redirects forbidden, expected a *url.Error with our 'no redirects allowed' error inside; got %#v (%q)", err, err);
        }
    }
    if (res == nil) {
        t.Fatalf("Expected a non-nil Response on CheckRedirect failure (https://golang.org/issue/3795)");
    }
    res.Body.Close();
    if (res.Header.Get(locationˢ) == ""u8) {
        t.Errorf("no Location header in Response");
    }
}

// Tests that Client redirects' contexts are derived from the original request's context.
public static void TestClientRedirectsContext(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirectsContext(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string redirectedRequestSˢ => ref redirectedRequestSˢᵇ;
private static readonly @string redirectedRequestSˢᵇ = "redirected request's context never expired after root request canceled"u8;

internal static void testClientRedirectsContext(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Redirect(w, r, "/", StatusTemporaryRedirect);
    }))).ts;

    var (ctx, cancel) = context_package.WithCancel(context_package.Background());
    var c = ts.Client();
    var cancelʗ1 = cancel;
    c.CheckRedirect = error (ж<Δhttp.Request> reqΔ1, slice<ж<Δhttp.Request>> via) => {
        cancelʗ1();
        var selᴛ2 = reqΔ1.Context().Done();
        var selᴛ3 = time.After((time.Duration)(5000000000L));
        switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            return default;
        }
        case 1 when selᴛ3.ꟷᐳ(out _): {
            return errors.New(redirectedRequestSˢ);
        }}
        return default;
    };
    var (req, _) = NewRequestWithContext(ctx, getˢ2, ts.URL, default);
    var (_, err) = c.Do(req);
    var (ue, ok) = err._<ж<urlꓸError>>(ᐧ);
    if (!ok) {
        t.Fatalf("got error %T; want *url.Error", err);
    }
    if (!AreEqual(ue.Err, context_package.Canceled)) {
        t.Errorf("url.Error.Err = %v; want %v", ue.Err, context_package.Canceled);
    }
}

partial struct redirectTest {
    internal @string suffix;
    internal nint want; // response code
    internal @string redirectBody;
}

public static void TestPostRedirects(ж<testing.T> t) {
    var postRedirectTests = slice<redirectTest>([
        new("/", 200, "first"),
        new("/?code=301&next=302", 200, "c301"),
        new("/?code=302&next=302", 200, "c302"),
        new("/?code=303&next=301", 200, "c303wc301"), // Issue 9348
        new("/?code=304", 304, "c304"),
        new("/?code=305", 305, "c305"),
        new("/?code=307&next=303,308,302", 200, "c307"),
        new("/?code=308&next=302,301", 200, "c308"),
        new("/?code=404", 404, "c404")
    ]);

    var wantSegments = slice<@string>([
        @"POST / ""first""",
        @"POST /?code=301&next=302 ""c301""",
        @"GET /?code=302 """"",
        @"GET / """"",
        @"POST /?code=302&next=302 ""c302""",
        @"GET /?code=302 """"",
        @"GET / """"",
        @"POST /?code=303&next=301 ""c303wc301""",
        @"GET /?code=301 """"",
        @"GET / """"",
        @"POST /?code=304 ""c304""",
        @"POST /?code=305 ""c305""",
        @"POST /?code=307&next=303,308,302 ""c307""",
        @"POST /?code=303&next=308,302 ""c307""",
        @"GET /?code=308&next=302 """"",
        @"GET /?code=302 """"",
        @"GET / """"",
        @"POST /?code=308&next=302,301 ""c308""",
        @"POST /?code=302&next=301 ""c308""",
        @"GET /?code=301 """"",
        @"GET / """"",
        @"POST /?code=404 ""c404"""
    ]);
    @string want = strings.Join(wantSegments, "\n");
    var postRedirectTestsʗ1 = postRedirectTests;
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testRedirectsByMethod(tΔ1, mode, postˢ, postRedirectTestsʗ1, want);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string deleteˢ => ref deleteˢᵇ;
private static readonly @string deleteˢᵇ = "DELETE"u8;

public static void TestDeleteRedirects(ж<testing.T> t) {
    var deleteRedirectTests = slice<redirectTest>([
        new("/", 200, "first"),
        new("/?code=301&next=302,308", 200, "c301"),
        new("/?code=302&next=302", 200, "c302"),
        new("/?code=303", 200, "c303"),
        new("/?code=307&next=301,308,303,302,304", 304, "c307"),
        new("/?code=308&next=307", 200, "c308"),
        new("/?code=404", 404, "c404")
    ]);

    var wantSegments = slice<@string>([
        @"DELETE / ""first""",
        @"DELETE /?code=301&next=302,308 ""c301""",
        @"GET /?code=302&next=308 """"",
        @"GET /?code=308 """"",
        @"GET / """"",
        @"DELETE /?code=302&next=302 ""c302""",
        @"GET /?code=302 """"",
        @"GET / """"",
        @"DELETE /?code=303 ""c303""",
        @"GET / """"",
        @"DELETE /?code=307&next=301,308,303,302,304 ""c307""",
        @"DELETE /?code=301&next=308,303,302,304 ""c307""",
        @"GET /?code=308&next=303,302,304 """"",
        @"GET /?code=303&next=302,304 """"",
        @"GET /?code=302&next=304 """"",
        @"GET /?code=304 """"",
        @"DELETE /?code=308&next=307 ""c308""",
        @"DELETE /?code=307 ""c308""",
        @"DELETE / ""c308""",
        @"DELETE /?code=404 ""c404"""
    ]);
    @string want = strings.Join(wantSegments, "\n");
    var deleteRedirectTestsʗ1 = deleteRedirectTests;
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testRedirectsByMethod(tΔ1, mode, deleteˢ, deleteRedirectTestsʗ1, want);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contentLengthˢ => ref contentLengthˢᵇ;
private static readonly @string contentLengthˢᵇ = "Content-Length"u8;
internal static ref readonly @string codeˢ => ref codeˢᵇ;
private static readonly @string codeˢᵇ = "code"u8;
internal static ref readonly @string nextˢ => ref nextˢᵇ;
private static readonly @string nextˢᵇ = "next"u8;

internal partial struct testRedirectsByMethod_log /*dyn*/ {
    /*embed*/ public global::go.sync_package.Mutex Mutex;
    /*embed*/ public bytes_package.Buffer Buffer;
}

internal static void testRedirectsByMethod(ж<testing.T> t, testMode mode, @string method, slice<redirectTest> table, @string want) {
    ref var log = ref heap(new testRedirectsByMethod_log(), out var Ꮡlog);
    ref var ts = ref heap<ж<httptest.Server>>(out var Ꮡts);
    ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Lock();
        var (slurp, _) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader(r.Body));
        fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer)), "%s %s %q"u8, r.Method, r.RequestURI, slurp);
        {
            @string cl = r.Header.Get(contentLengthˢ); if (r.Method == "GET"u8 && len(slurp) == 0 && (r.ContentLength != 0 || cl != ""u8)) {
                fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer)), " (but with body=%T, content-length = %v, %q)"u8, r.Body, r.ContentLength, cl);
            }
        }
        Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer).WriteByte((byte)'\n');
        Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Unlock();
        var urlQuery = r.URL.Query();
        {
            @string v = urlQuery.Get(codeˢ); if (v != ""u8) {
                @string location = Ꮡts.ValueSlot.URL;
                {
                    @string final = urlQuery.Get(nextˢ); if (final != ""u8) {
                        var (first, rest, _) = strings.Cut(final, ",");
                        location = fmt.Sprintf("%s?code=%s"u8, location, first);
                        if (rest != ""u8) {
                            location = fmt.Sprintf("%s&next=%s"u8, location, rest);
                        }
                    }
                }
                var (code, _) = strconv.Atoi(v);
                if (code / 100 == 3) {
                    w.Header().Set(locationˢ, location);
                }
                w.WriteHeader(code);
            }
        }
    }))).ts;

    var c = ts.Client();
    foreach (var (_, tt) in table) {
        @string content = tt.redirectBody;
        var (req, _) = NewRequest(method, ts.URL + tt.suffix, new http_test_package.strings_ReaderжReader(strings.NewReader(content)));
        req.GetBody = (io.ReadCloser, error) () => (io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(content))), default);
        var (res, err) = c.Do(req);

        if (err != nil) {
            t.Fatal(err);
        }
        if (res.StatusCode != tt.want) {
            t.Errorf("POST %s: status code = %d; want %d", tt.suffix, res.StatusCode, tt.want);
        }
    }
    Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Lock();
    @string got = Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer).String();
    Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Unlock();

    got = strings.TrimSpace(got);
    want = strings.TrimSpace(want);

    if (got != want) {
        var (gotΔ1, wantΔ1, lines) = removeCommonLines(got, want);
        t.Errorf("Log differs after %d common lines.\n\nGot:\n%s\n\nWant:\n%s\n", lines, gotΔ1, wantΔ1);
    }
}

internal static (@string asuffix, @string bsuffix, nint commonLines) removeCommonLines(@string a, @string b) {
    nint commonLines = default;

    for (;;) {
        nint nl = strings.IndexByte(a, (byte)'\n');
        if (nl < 0) {
            return (a, b, commonLines);
        }
        @string line = a.slice(0, nl + 1);
        if (!strings.HasPrefix(b, line)) {
            return (a, b, commonLines);
        }
        commonLines++;
        a = a.slice(len(line));
        b = b.slice(len(line));
    }
}

public static void TestClientRedirectUseResponse(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirectUseResponse(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string otherˢ => ref otherˢᵇ;
private static readonly @string otherˢᵇ = "/other"u8;
internal static ref readonly @string wrongBodyˢ => ref wrongBodyˢᵇ;
private static readonly @string wrongBodyˢᵇ = "wrong body"u8;
internal static readonly object expectedNonNilRequestˢ = (@string)"expected non-nil Request.Response"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bodyᶜ => ref bodyᶜᵇ;
private static readonly @string bodyᶜᵇ = "Hello, world."u8;

internal static void testClientRedirectUseResponse(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string body = bodyᶜ;
        ref var ts = ref heap<ж<httptest.Server>>(out var Ꮡts);
        ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if (strings.Contains(r.URL.Path, otherˢ)) {
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), wrongBodyˢ);
            } else {
                w.Header().Set(locationˢ, Ꮡts.ValueSlot.URL + "/other"u8);
                w.WriteHeader(StatusFound);
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), body);
            }
        }))).ts;

        var c = ts.Client();
        c.CheckRedirect = (ж<Δhttp.Request> req, slice<ж<Δhttp.Request>> via) => {
            if (req.Response == nil) {
                t.Error(expectedNonNilRequestˢ);
            }
            return ErrUseLastResponse;
        };
        var (res, err) = c.Get(ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        if (res.StatusCode != StatusFound) {
            t.Errorf("status = %d; want %d", res.StatusCode, (nint)(StatusFound));
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var slurp, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(err);
        }
        if (((sstring)slurp) != body) {
            t.Errorf("body = %q; want %q", slurp, body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issues 17773 and 49281: don't follow a 3xx if the response doesn't
// have a Location header.
public static void TestClientRedirectNoLocation(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirectNoLocation(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ2 => ref fooˢ2ᵇ;
private static readonly @string fooˢ2ᵇ = "Foo"u8;
internal static ref readonly @string barˢ2 => ref barˢ2ᵇ;
private static readonly @string barˢ2ᵇ = "Bar"u8;

internal static void testClientRedirectNoLocation(ж<testing.T> t, testMode mode) {
    foreach (var (_, code) in slice<nint>([301, 308])) {
        t.Run(fmt.Sprint(code), (ж<testing.T> tΔ1) => {
            setParallel(tΔ1);
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                w.Header().Set(fooˢ2, barˢ2);
                w.WriteHeader(code);
            })));
            var (res, err) = cst.c.Get(cst.ts.URL);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            res.Body.Close();
            if (res.StatusCode != code) {
                tΔ1.Errorf("status = %d; want %d", res.StatusCode, code);
            }
            {
                @string got = res.Header.Get(fooˢ2); if (got != "Bar"u8) {
                    tΔ1.Errorf("Foo header = %q; want Bar", got);
                }
            }
        });
    }
}

// Don't follow a 307/308 if we can't resent the request body.
public static void TestClientRedirect308NoGetBody(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirect308NoGetBody(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someBodyˢ => ref someBodyˢᵇ;
private static readonly @string someBodyˢᵇ = "some body"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fakeURLᶜ => ref fakeURLᶜᵇ;
private static readonly @string fakeURLᶜᵇ = "https://localhost:1234/"u8;

internal static void testClientRedirect308NoGetBody(ж<testing.T> t, testMode mode) {
    @string fakeURL = fakeURLᶜ; // won't be hit
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(locationˢ, fakeURL);
        w.WriteHeader(308);
    }))).ts;
    var (req, err) = NewRequest(postˢ, ts.URL, new http_test_package.strings_ReaderжReader(strings.NewReader(someBodyˢ)));
    if (err != nil) {
        t.Fatal(err);
    }
    var c = ts.Client();
    req.GetBody = default; // so it can't rewind.
    (var res, err) = c.Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();
    if (res.StatusCode != 308) {
        t.Errorf("status = %d; want %d", res.StatusCode, (nint)308);
    }
    {
        @string got = res.Header.Get(locationˢ); if (got != fakeURL) {
            t.Errorf("Location header = %q; want %q", got, fakeURL);
        }
    }
}

internal static slice<ж<httpꓸCookie>> expectedCookies = slice<ж<httpꓸCookie>>([
    Ꮡ(new httpꓸCookie(Name: "ChocolateChip", Value: "tasty")),
    Ꮡ(new httpꓸCookie(Name: "First", Value: "Hit")),
    Ꮡ(new httpꓸCookie(Name: "Second", Value: "Hit"))
]);

internal static Δhttp.HandlerFunc echoCookiesRedirectHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    foreach (var (_, cookie) in r.Cookies()) {
        SetCookie(w, cookie);
    }
    if (r.URL.Path == "/"u8) {
        SetCookie(w, expectedCookies[1]);
        Redirect(w, r, "/second", StatusMovedPermanently);
    } else {
        SetCookie(w, expectedCookies[2]);
        w.Write(slice<byte>("hello"u8));
    }
});

public static void TestHostMismatchCookies(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testHostMismatchCookies(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string setOnServerˢ => ref setOnServerˢᵇ;
private static readonly @string setOnServerˢᵇ = "SetOnServer"u8;
internal static ref readonly @string cookiesExampleComˢ => ref cookiesExampleComˢᵇ;
private static readonly @string cookiesExampleComˢᵇ = "cookies.example.com"u8;

internal static void testHostMismatchCookies(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        foreach (var (_, cΔ1) in r.Cookies()) {
            cΔ1.Value.Value = setOnServerˢ;
            SetCookie(w, cΔ1);
        }
    }))).ts;

    var (reqURL, _) = url.Parse(ts.URL);
    ref var hostURL = ref heap<url.URL>(reqURL.Value, out var ᏑhostURL);
    hostURL.Host = cookiesExampleComˢ;

    var c = ts.Client();
    c.Jar = new http_test_package.TestJarжCookieJar(@new<TestJar>());
    c.Jar.SetCookies(reqURL, slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "First", Value: "SetOnClient"))]));
    c.Jar.SetCookies(ᏑhostURL, slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "Second", Value: "SetOnClient"))]));

    var (req, _) = NewRequest(getˢ2, ts.URL, new http_test_package.http_noBodyᴠReader(NoBody));
    req.Host = hostURL.Host;
    var (resp, err) = c.Do(req);
    if (err != nil) {
        t.Fatalf("Get: %v", err);
    }
    resp.Body.Close();

    matchReturnedCookies(t, slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "First", Value: "SetOnClient"))]), c.Jar.Cookies(reqURL));
    matchReturnedCookies(t, slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "Second", Value: "SetOnServer"))]), c.Jar.Cookies(ᏑhostURL));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string textPlainˢ => ref textPlainˢᵇ;
private static readonly @string textPlainˢᵇ = "text/plain"u8;
internal static ref readonly @string bodyˢ => ref bodyˢᵇ;
private static readonly @string bodyˢᵇ = "body"u8;

public static void TestClientSendsCookieFromJar(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var tr = Ꮡ(new recordingTransport());
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        client.Jar = new http_test_package.TestJarжCookieJar(Ꮡ(new TestJar(perURL: new map<@string, slice<ж<httpꓸCookie>>>())));
        @string us = httpDummyFaketldˢ;
        var (u, _) = url.Parse(us);
        client.Jar.SetCookies(u, expectedCookies);

        client.Get(us); // Note: doesn't hit network
        matchReturnedCookies(t, expectedCookies, tr.req.Cookies());

        client.Head(us); // Note: doesn't hit network
        matchReturnedCookies(t, expectedCookies, tr.req.Cookies());

        client.Post(us, textPlainˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ))); // Note: doesn't hit network
        matchReturnedCookies(t, expectedCookies, tr.req.Cookies());

        client.PostForm(us, new url.Values(new map<@string, slice<@string>>{})); // Note: doesn't hit network
        matchReturnedCookies(t, expectedCookies, tr.req.Cookies());

        var (req, _) = NewRequest(getˢ2, us, default);
        client.Do(req); // Note: doesn't hit network
        matchReturnedCookies(t, expectedCookies, tr.req.Cookies());

        (req, _) = NewRequest(postˢ, us, default);
        client.Do(req); // Note: doesn't hit network
        matchReturnedCookies(t, expectedCookies, tr.req.Cookies());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Just enough correctness for our redirect tests. Uses the URL.Host as the
// scope of all cookies.
partial struct TestJar {
    internal sync.Mutex m;
    internal map<@string, slice<ж<httpꓸCookie>>> perURL;
}

public static void SetCookies(this ж<TestJar> Ꮡj, ж<url.URL> Ꮡu, slice<ж<httpꓸCookie>> cookies) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var j = ref Ꮡj.DerefOrNull();
        ref var u = ref Ꮡu.DerefOrNull();

        j.m.Lock();
        ᒐd1 = true;
        if (j.perURL == nil) {
            j.perURL = new map<@string, slice<ж<httpꓸCookie>>>();
        }
        j.perURL[u.Host] = cookies;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡj.DerefOrNull().m.Unlock(); ᒐ.Run(); }
}

public static slice<ж<httpꓸCookie>> Cookies(this ж<TestJar> Ꮡj, ж<url.URL> Ꮡu) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var j = ref Ꮡj.DerefOrNull();
        ref var u = ref Ꮡu.DerefOrNull();

        j.m.Lock();
        ᒐd1 = true;
        return j.perURL[u.Host];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡj.DerefOrNull().m.Unlock(); ᒐ.Run(); }
}

public static void TestRedirectCookiesJar(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testRedirectCookiesJar(Δp0, Δp1));
}

internal static void testRedirectCookiesJar(ж<testing.T> t, testMode mode) {
    ж<httptest.Server> ts = default;
    ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(echoCookiesRedirectHandler)).ts;
    var c = ts.Client();
    c.Jar = new http_test_package.TestJarжCookieJar(@new<TestJar>());
    var (u, _) = url.Parse(ts.URL);
    c.Jar.SetCookies(u, slice<ж<httpꓸCookie>>([expectedCookies[0]]));
    var (resp, err) = c.Get(ts.URL);
    if (err != nil) {
        t.Fatalf("Get: %v", err);
    }
    resp.Body.Close();
    matchReturnedCookies(t, expectedCookies, resp.Cookies());
}

internal static void matchReturnedCookies(ж<testing.T> t, slice<ж<httpꓸCookie>> expected, slice<ж<httpꓸCookie>> given) {
    if (len(given) != len(expected)) {
        t.Logf("Received cookies: %v", given);
        t.Errorf("Expected %d cookies, got %d", len(expected), len(given));
    }
    foreach (var (_, ec) in expected) {
        var foundC = false;
        foreach (var (_, c) in given) {
            if (ec.Name == c.Name && (~ec).Value == (~c).Value) {
                foundC = true;
                break;
            }
        }
        if (!foundC) {
            t.Errorf("Missing cookie %v", ec.OrTypedNil());
        }
    }
}

public static void TestJarCalls(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testJarCalls(Δp0, Δp1), slice<testMode>([http1Mode]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpSecondhostFakeˢ => ref httpSecondhostFakeˢᵇ;
private static readonly @string httpSecondhostFakeˢᵇ = "http://secondhost.fake/secondpath"u8;
internal static ref readonly @string httpFirsthostFakeˢ => ref httpFirsthostFakeˢᵇ;
private static readonly @string httpFirsthostFakeˢᵇ = "http://firsthost.fake/"u8;
internal static ref readonly @string httpFirsthostFakeˢ2 => ref httpFirsthostFakeˢ2ᵇ;
private static readonly @string httpFirsthostFakeˢ2ᵇ = "http://firsthost.fake/nosetcookie"u8;
internal static ref readonly @string cookiesHttpFirsthostFakeˢ => ref cookiesHttpFirsthostFakeˢᵇ;
private static readonly @string cookiesHttpFirsthostFakeˢᵇ = """
Cookies("http://firsthost.fake/")
SetCookie("http://firsthost.fake/", [name=val])
Cookies("http://secondhost.fake/secondpath")
SetCookie("http://secondhost.fake/secondpath", [namesecondpath=valsecondpath])
Cookies("http://firsthost.fake/nosetcookie")

"""u8.Raw();

internal static void testJarCalls(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        @string pathSuffix = r.RequestURI[1..];
        if (r.RequestURI == "/nosetcookie"u8) {
            return; // don't set cookies for this path
        }
        SetCookie(w, Ꮡ(new httpꓸCookie(Name: "name"u8 + pathSuffix, Value: "val"u8 + pathSuffix)));
        if (r.RequestURI == "/"u8) {
            Redirect(w, r, httpSecondhostFakeˢ, 302);
        }
    }))).ts;
    var jar = @new<RecordingJar>();
    var c = ts.Client();
    c.Jar = new http_test_package.RecordingJarжCookieJar(jar);
    var tsʗ1 = ts;
    c.Transport._<ж<Δhttp.Transport>>().Dial = (@string _Δp0, @string _Δp1) => net.Dial(tcpˢ, tsʗ1.Listener.Addr().String());
    var (_, err) = c.Get(httpFirsthostFakeˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = c.Get(httpFirsthostFakeˢ2);
    if (err != nil) {
        t.Fatal(err);
    }
    @string got = jar.Ꮡlog.String();
    @string want = cookiesHttpFirsthostFakeˢ;
    if (got != want) {
        t.Errorf("Got Jar calls:\n%s\nWant:\n%s", got, want);
    }
}

// RecordingJar keeps a log of calls made to it, without
// tracking any cookies.
partial struct RecordingJar {
    internal sync.Mutex mu;
    internal bytes.Buffer log;
}

public static void SetCookies(this ж<RecordingJar> Ꮡj, ж<url.URL> u, slice<ж<httpꓸCookie>> cookies) {
    Ꮡj.logf("SetCookie(%q, %v)\n", u.OrTypedNil(), cookies);
}

public static slice<ж<httpꓸCookie>> Cookies(this ж<RecordingJar> Ꮡj, ж<url.URL> u) {
    Ꮡj.logf("Cookies(%q)\n", u.OrTypedNil());
    return default;
}

internal static void logf(this ж<RecordingJar> Ꮡj, @string format, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        var args = argsʗp.sslice();
        ref var j = ref Ꮡj.DerefOrNull();

        j.mu.Lock();
        ᒐd1 = true;
        fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡj.Ꮡlog), format, args.ꓸꓸꓸ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡj.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

public static void TestStreamingGet(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testStreamingGet(Δp0, Δp1));
}

internal static void testStreamingGet(ж<testing.T> t, testMode mode) {
    var say = new channel<@string>(0);
    var sayʗ1 = say;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w._<Flusher>().Flush();
        foreach (var str in sayʗ1) {
            w.Write(slice<byte>(str));
            w._<Flusher>().Flush();
        }
    })));

    var c = cst.c;
    var (res, err) = c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    array<byte> buf = new(10);
    foreach (var (_, str) in slice<@string>(["i", "am", "also", "known", "as", "comet"])) {
        say.ᐸꟷ(str);
        var (n, errΔ1) = io.ReadFull(res.Body, buf.slice(0, len(str)));
        if (errΔ1 != nil) {
            t.Fatalf("ReadFull on %q: %v", str, errΔ1);
        }
        if (n != len(str)) {
            t.Fatalf("Receiving %q, only read %d bytes", str, n);
        }
        @string got = (@string)(buf.slice(0, n));
        if (got != str) {
            t.Fatalf("Expected %q, got %q", str, got);
        }
    }
    builtin.close(say);
    (_, err) = io.ReadFull(res.Body, buf[0..1]);
    if (!AreEqual(err, io.EOF)) {
        t.Fatalf("at end expected EOF, got %v", err);
    }
}

partial struct writeCountingConn {
    /*embed*/ public net_package.Conn Conn;
    internal ж<nint> count;
}

// Go method set entry for the promoted 'Conn.Close()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error Close(this writeCountingConn recvᴛ) => recvᴛ.Conn.Close();

// Go method set entry for the promoted 'Conn.LocalAddr()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr LocalAddr(this writeCountingConn recvᴛ) => recvᴛ.Conn.LocalAddr();

// Go method set entry for the promoted 'Conn.Read()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this writeCountingConn recvᴛ, slice<byte> b) => recvᴛ.Conn.Read(b);

// Go method set entry for the promoted 'Conn.RemoteAddr()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static netꓸAddr RemoteAddr(this writeCountingConn recvᴛ) => recvᴛ.Conn.RemoteAddr();

// Go method set entry for the promoted 'Conn.SetDeadline()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error SetDeadline(this writeCountingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetDeadline(t);

// Go method set entry for the promoted 'Conn.SetReadDeadline()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error SetReadDeadline(this writeCountingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetReadDeadline(t);

// Go method set entry for the promoted 'Conn.SetWriteDeadline()' - provided ONLY by the embedded
// interface field in *writeCountingConn's method set; see the pointer-only satisfaction record.
internal static error SetWriteDeadline(this writeCountingConn recvᴛ, time.Time t) => recvᴛ.Conn.SetWriteDeadline(t);

internal static (nint, error) Write(this ref writeCountingConn c, slice<byte> p) {
    c.count.Value++;
    return c.Conn.Write(p);
}

// TestClientWrites verifies that client requests are buffered and we
// don't send a TCP packet per line of the http request + body.
public static void TestClientWrites(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientWrites(Δp0, Δp1), slice<testMode>([http1Mode]));
}

internal static void testClientWrites(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).ts;

    ref var writes = ref heap<nint>(0, out var Ꮡwrites);
    var dialer = (@string netz, @string addr) => {
        var (cΔ1, errΔ1) = net.Dial(netz, addr);
        if (errΔ1 == nil) {
            cΔ1 = new http_test_package.writeCountingConnжConn(Ꮡ(new writeCountingConn(cΔ1, Ꮡwrites)));
        }
        return (cΔ1, errΔ1);
    };
    var c = ts.Client();
    c.Transport._<ж<Δhttp.Transport>>().Dial = dialer;

    var (_, err) = c.Get(ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    if (writes != 1) {
        t.Errorf("Get request did %d Write calls, want 1", writes);
    }

    writes = 0;
    (_, err) = c.PostForm(ts.URL, new url.Values(new map<@string, slice<@string>>{["foo"] = slice<@string>(["bar"])}));
    if (err != nil) {
        t.Fatal(err);
    }
    if (writes != 1) {
        t.Errorf("Post request did %d Write calls, want 1", writes);
    }
}

public static void TestClientInsecureTransport(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientInsecureTransport(Δp0, Δp1), slice<testMode>([https1Mode, http2Mode]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string connectionˢ => ref connectionˢᵇ;
private static readonly @string connectionˢᵇ = "Connection"u8;
internal static ref readonly @string closeˢ => ref closeˢᵇ;
private static readonly @string closeˢᵇ = "close"u8;
internal static ref readonly @string tlsHandshakeErrorˢ => ref tlsHandshakeErrorˢᵇ;
private static readonly @string tlsHandshakeErrorˢᵇ = "TLS handshake error"u8;

internal static void testClientInsecureTransport(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello"u8));
    })));
    var ts = cst.ts;
    var errLog = @new<strings.Builder>();
    ts.Config.ErrorLog = log.New(new http_test_package.strings_BuilderжWriter(errLog), ""u8, 0);

    // TODO(bradfitz): add tests for skipping hostname checks too?
    // would require a new cert for testing, and probably
    // redundant with these tests.
    var c = ts.Client();
    foreach (var (_, vᴛ1) in slice<bool>([true, false])) {
        ref var insecure = ref heap(new bool(), out var Ꮡinsecure);
        insecure = vᴛ1;

        c.Transport._<ж<Δhttp.Transport>>().TLSClientConfig = Ꮡ(new tls.Config(
            InsecureSkipVerify: insecure,
            NextProtos: cst.tr.TLSClientConfig.NextProtos
        ));
        var (req, _) = NewRequest(getˢ2, ts.URL, default);
        req.Header.Set(connectionˢ, closeˢ); // don't reuse this connection
        var (res, err) = c.Do(req);
        if ((err == nil) != insecure) {
            t.Errorf("insecure=%v: got unexpected err=%v", insecure, err);
        }
        if (res != nil) {
            res.Body.Close();
        }
    }

    cst.close();
    if (!strings.Contains(errLog.String(), tlsHandshakeErrorˢ)) {
        t.Errorf("expected an error log message containing 'TLS handshake error'; got %q", errLog.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpLocalhost1234ˢ => ref httpLocalhost1234ˢᵇ;
private static readonly @string httpLocalhost1234ˢᵇ = "http://localhost:1234/"u8;
internal static ref readonly @string thisFieldIsIllegalAndˢ => ref thisFieldIsIllegalAndˢᵇ;
private static readonly @string thisFieldIsIllegalAndˢᵇ = "/this/field/is/illegal/and/should/error/"u8;
internal static ref readonly @string requestURIˢ => ref requestURIˢᵇ;
private static readonly @string requestURIˢᵇ = "RequestURI"u8;

public static void TestClientErrorWithRequestURI(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var (req, _) = NewRequest(getˢ2, httpLocalhost1234ˢ, default);
        req.RequestURI = thisFieldIsIllegalAndˢ;
        var (_, err) = DefaultClient.Do(req);
        if (err == nil) {
            t.Fatalf("expected an error");
        }
        if (!strings.Contains(err.Error(), requestURIˢ)) {
            t.Errorf("wanted error mentioning RequestURI; got error: %v", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClientWithCorrectTLSServerName(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientWithCorrectTLSServerName(Δp0, Δp1), slice<testMode>([https1Mode, http2Mode]));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string serverNameᶜ => ref serverNameᶜᵇ;
private static readonly @string serverNameᶜᵇ = "example.com"u8;

internal static void testClientWithCorrectTLSServerName(ж<testing.T> t, testMode mode) {
    @string serverName = serverNameᶜ;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if (r.TLS.ServerName != serverName) {
            t.Errorf("expected client to set ServerName %q, got: %q", serverName, r.TLS.ServerName);
        }
    }))).ts;

    var c = ts.Client();
    c.Transport._<ж<Δhttp.Transport>>().TLSClientConfig.ServerName = serverName;
    {
        var (_, err) = c.Get(ts.URL); if (err != nil) {
            t.Fatalf("expected successful TLS connection, got error: %v", err);
        }
    }
}

public static void TestClientWithIncorrectTLSServerName(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientWithIncorrectTLSServerName(Δp0, Δp1), slice<testMode>([https1Mode, http2Mode]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badserverˢ => ref badserverˢᵇ;
private static readonly @string badserverˢᵇ = "badserver"u8;

internal static void testClientWithIncorrectTLSServerName(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })));
    var ts = cst.ts;
    var errLog = @new<strings.Builder>();
    ts.Config.ErrorLog = log.New(new http_test_package.strings_BuilderжWriter(errLog), ""u8, 0);

    var c = ts.Client();
    c.Transport._<ж<Δhttp.Transport>>().TLSClientConfig.ServerName = badserverˢ;
    var (_, err) = c.Get(ts.URL);
    if (err == nil) {
        t.Fatalf("expected an error");
    }
    if (!strings.Contains(err.Error(), "127.0.0.1") || !strings.Contains(err.Error(), badserverˢ)) {
        t.Errorf("wanted error mentioning 127.0.0.1 and badserver; got error: %v", err);
    }

    cst.close();
    if (!strings.Contains(errLog.String(), tlsHandshakeErrorˢ)) {
        t.Errorf("expected an error log message containing 'TLS handshake error'; got %q", errLog.OrTypedNil());
    }
}

// Test for golang.org/issue/5829; the Transport should respect TLSClientConfig.ServerName
// when not empty.
//
// tls.Config.ServerName (non-empty, set to "example.com") takes
// precedence over "some-other-host.tld" which previously incorrectly
// took precedence. We don't actually connect to (or even resolve)
// "some-other-host.tld", though, because of the Transport.Dial hook.
//
// The httptest.Server has a cert with "example.com" as its name.
public static void TestTransportUsesTLSConfigServerName(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testTransportUsesTLSConfigServerName(Δp0, Δp1), slice<testMode>([https1Mode, http2Mode]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleComˢ => ref exampleComˢᵇ;
private static readonly @string exampleComˢᵇ = "example.com"u8;
internal static ref readonly @string httpsSomeOtherHostTldˢ => ref httpsSomeOtherHostTldˢᵇ;
private static readonly @string httpsSomeOtherHostTldˢᵇ = "https://some-other-host.tld/"u8;

internal static void testTransportUsesTLSConfigServerName(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello"u8));
    }))).ts;

    var c = ts.Client();
    var tr = c.Transport._<ж<Δhttp.Transport>>();
    tr.TLSClientConfig.ServerName = exampleComˢ; // one of httptest's Server cert names
    var tsʗ1 = ts;
    tr.Dial = (@string netw, @string addr) => net.Dial(netw, tsʗ1.Listener.Addr().String());
    var (res, err) = c.Get(httpsSomeOtherHostTldˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();
}

public static void TestResponseSetsTLSConnectionState(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testResponseSetsTLSConnectionState(Δp0, Δp1), slice<testMode>([https1Mode]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpsExampleComˢ => ref httpsExampleComˢᵇ;
private static readonly @string httpsExampleComˢᵇ = "https://example.com/"u8;
internal static readonly object responseDidnTSetTlsˢ = (@string)"Response didn't set TLS Connection State."u8;

internal static void testResponseSetsTLSConnectionState(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Write(slice<byte>("Hello"u8));
        }))).ts;

        var c = ts.Client();
        var tr = c.Transport._<ж<Δhttp.Transport>>();
        tr.TLSClientConfig.CipherSuites = slice<uint16>([tls.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256]);
        tr.TLSClientConfig.MaxVersion = tls.VersionTLS12; // to get to pick the cipher suite
        var tsʗ1 = ts;
        tr.Dial = (@string netw, @string addr) => net.Dial(netw, tsʗ1.Listener.Addr().String());
        var (res, err) = c.Get(httpsExampleComˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.TLS == nil) {
            t.Fatal(responseDidnTSetTlsˢ);
        }
        {
            var (got, want) = (res.TLS.CipherSuite, tls.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256); if (got != want) {
                t.Errorf("TLS Cipher Suite = %d; want %d", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Check that an HTTPS client can interpret a particular TLS error
// to determine that the server is speaking HTTP.
// See golang.org/issue/11111.
public static void TestHTTPSClientDetectsHTTPServer(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testHTTPSClientDetectsHTTPServer(Δp0, Δp1), slice<testMode>([http1Mode]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpˢ => ref httpˢᵇ;
private static readonly @string httpˢᵇ = "http"u8;
internal static ref readonly @string httpsˢ => ref httpsˢᵇ;
private static readonly @string httpsˢᵇ = "https"u8;
internal static ref readonly @string httpResponseToHttpsˢ => ref httpResponseToHttpsˢᵇ;
private static readonly @string httpResponseToHttpsˢᵇ = "HTTP response to HTTPS client"u8;

internal static void testHTTPSClientDetectsHTTPServer(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).ts;
    ts.Config.ErrorLog = quietLog;

    var (_, err) = Get(strings.Replace(ts.URL, httpˢ, httpsˢ, 1));
    {
        @string got = err.Error(); if (!strings.Contains(got, httpResponseToHttpsˢ)) {
            t.Fatalf("error = %q; want error indicating HTTP response to HTTPS request", got);
        }
    }
}

// Verify Response.ContentLength is populated. https://golang.org/issue/4126
public static void TestClientHeadContentLength(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientHeadContentLength(Δp0, Δp1));
}

internal partial struct testClientHeadContentLength_tests /*dyn*/ {
    internal @string suffix;
    internal int64 want;
}

internal static void testClientHeadContentLength(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        {
            @string v = r.FormValue("cl"); if (v != ""u8) {
                w.Header().Set(contentLengthˢ, v);
            }
        }
    })));
    var tests = slice<testClientHeadContentLength_tests>([
        new("/?cl=1234", 1234),
        new("/?cl=0", 0),
        new("", -1)
    ]);
    foreach (var (_, tt) in tests) {
        var (req, _) = NewRequest(headˢ, cst.ts.URL + tt.suffix, default);
        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        if (res.ContentLength != tt.want) {
            t.Errorf("Content-Length = %d; want %d", res.ContentLength, tt.want);
        }
        (var bs, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(err);
        }
        if (len(bs) != 0) {
            t.Errorf("Unexpected content: %q", bs);
        }
    }
}

public static void TestEmptyPasswordAuth(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testEmptyPasswordAuth(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gopherˢ => ref gopherˢᵇ;
private static readonly @string gopherˢᵇ = "gopher"u8;
internal static ref readonly @string authorizationˢ => ref authorizationˢᵇ;
private static readonly @string authorizationˢᵇ = "Authorization"u8;
internal static ref readonly @string basicˢ => ref basicˢᵇ;
private static readonly @string basicˢᵇ = "Basic "u8;

internal static void testEmptyPasswordAuth(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string gopher = gopherˢ;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            @string auth = r.Header.Get(authorizationˢ);
            if (strings.HasPrefix(auth, basicˢ)) {
                @string encoded = auth[6..];
                var (decoded, errΔ1) = base64.StdEncoding.DecodeString(encoded);
                if (errΔ1 != nil) {
                    t.Fatal(errΔ1);
                }
                @string expected = gopher + ":"u8;
                @string s = (@string)decoded;
                if (expected != s) {
                    t.Errorf("Invalid Authorization header. Got %q, wanted %q", s, expected);
                }
            } else {
                t.Errorf("Invalid auth %q", auth);
            }
        }))).ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (req, err) = NewRequest(getˢ2, ts.URL, default);
        if (err != nil) {
            t.Fatal(err);
        }
        req.URL.User = url.User(gopher);
        var c = ts.Client();
        (var resp, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var respʗ1 = resp;
        defer(ᴛ0 => ᴛ0.Close(), respʗ1.Body, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpMy20UserMy20Passˢ => ref httpMy20UserMy20Passˢᵇ;
private static readonly @string httpMy20UserMy20Passˢᵇ = "http://My%20User:My%20Pass@dummy.faketld/"u8;
internal static ref readonly @string myUserMyPassˢ => ref myUserMyPassˢᵇ;
private static readonly @string myUserMyPassˢᵇ = "My User:My Pass"u8;

public static void TestBasicAuth(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var tr = Ꮡ(new recordingTransport());
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));

        @string url = httpMy20UserMy20Passˢ;
        @string expected = myUserMyPassˢ;
        client.Get(url);

        if (tr.req.Method != "GET"u8) {
            t.Errorf("got method %q, want %q", tr.req.Method, getˢ2);
        }
        if (tr.req.URL.String() != url) {
            t.Errorf("got URL %q, want %q", tr.req.URL.String(), url);
        }
        if (tr.req.Header == nil) {
            t.Fatalf("expected non-nil request Header");
        }
        @string auth = tr.req.Header.Get(authorizationˢ);
        if (strings.HasPrefix(auth, basicˢ)) {
            @string encoded = auth[6..];
            var (decoded, err) = base64.StdEncoding.DecodeString(encoded);
            if (err != nil) {
                t.Fatal(err);
            }
            @string s = (@string)decoded;
            if (expected != s) {
                t.Errorf("Invalid Authorization header. Got %q, wanted %q", s, expected);
            }
        } else {
            t.Errorf("Invalid auth %q", auth);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpMy20UserDummyFaketldˢ => ref httpMy20UserDummyFaketldˢᵇ;
private static readonly @string httpMy20UserDummyFaketldˢᵇ = "http://My%20User@dummy.faketld/"u8;
internal static ref readonly @string myUserˢ => ref myUserˢᵇ;
private static readonly @string myUserˢᵇ = "My User"u8;
internal static ref readonly @string myPassˢ => ref myPassˢᵇ;
private static readonly @string myPassˢᵇ = "My Pass"u8;

public static void TestBasicAuthHeadersPreserved(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(t), ref ᒐ);
        var tr = Ꮡ(new recordingTransport());
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));

        // If Authorization header is provided, username in URL should not override it
        @string url = httpMy20UserDummyFaketldˢ;
        var (req, err) = NewRequest(getˢ2, url, default);
        if (err != nil) {
            t.Fatal(err);
        }
        req.SetBasicAuth(myUserˢ, myPassˢ);
        @string expected = myUserMyPassˢ;
        client.Do(req);

        if (tr.req.Method != "GET"u8) {
            t.Errorf("got method %q, want %q", tr.req.Method, getˢ2);
        }
        if (tr.req.URL.String() != url) {
            t.Errorf("got URL %q, want %q", tr.req.URL.String(), url);
        }
        if (tr.req.Header == nil) {
            t.Fatalf("expected non-nil request Header");
        }
        @string auth = tr.req.Header.Get(authorizationˢ);
        if (strings.HasPrefix(auth, basicˢ)) {
            @string encoded = auth[6..];
            var (decoded, errΔ1) = base64.StdEncoding.DecodeString(encoded);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            @string s = (@string)decoded;
            if (expected != s) {
                t.Errorf("Invalid Authorization header. Got %q, wanted %q", s, expected);
            }
        } else {
            t.Errorf("Invalid auth %q", auth);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestStripPasswordFromError_testCases /*dyn*/ {
    internal @string desc;
    internal @string @in;
    internal @string @out;
}

public static void TestStripPasswordFromError(ж<testing.T> t) {
    var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(Ꮡ(new recordingTransport()))));
    var testCases = slice<TestStripPasswordFromError_testCases>([
        new(
            desc: "Strip password from error message",
            @in: "http://user:password@dummy.faketld/",
            @out: @"Get ""http://user:***@dummy.faketld/"": dummy impl"
        ),
        new(
            desc: "Don't Strip password from domain name",
            @in: "http://user:password@password.faketld/",
            @out: @"Get ""http://user:***@password.faketld/"": dummy impl"
        ),
        new(
            desc: "Don't Strip password from path",
            @in: "http://user:password@dummy.faketld/password",
            @out: @"Get ""http://user:***@dummy.faketld/password"": dummy impl"
        ),
        new(
            desc: "Strip escaped password",
            @in: "http://user:pa%2Fssword@dummy.faketld/",
            @out: @"Get ""http://user:***@dummy.faketld/"": dummy impl"
        )
    ]);
    foreach (var (_, vᴛ1) in testCases) {
        ref var tC = ref heap(new TestStripPasswordFromError_testCases(), out var ᏑtC);
        tC = vᴛ1;

        var clientʗ1 = client;
        var tCʗ1 = tC;
        t.Run(tC.desc, (ж<testing.T> tΔ1) => {
            var (_, err) = clientʗ1.Get(tCʗ1.@in);
            if (err.Error() != tCʗ1.@out) {
                tΔ1.Errorf("Unexpected output for %q: expected %q, actual %q",
                    tCʗ1.@in, tCʗ1.@out, err.Error());
            }
        });
    }
}

public static void TestClientTimeout(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nonceˢ => ref nonceˢᵇ;
private static readonly @string nonceˢᵇ = "nonce"u8;
internal static ref readonly @string clientTimeoutˢ => ref clientTimeoutˢᵇ;
private static readonly @string clientTimeoutˢᵇ = "Client.Timeout"u8;
internal static readonly object handlerNeverGotSlowˢ = (@string)"handler never got /slow request, but client returned response"u8;
internal static readonly object expectedErrorFromReadAllˢ = (@string)"expected error from ReadAll"u8;
internal static ref readonly @string clientTimeoutˢ2 => ref clientTimeoutˢ2ᵇ;
private static readonly @string clientTimeoutˢ2ᵇ = "(Client.Timeout"u8;

internal static void testClientTimeout(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    @string nonce = default;     // a unique per-request string
    bool sawSlowNonce = default; // true if the handler saw /slow?nonce=<nonce>
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        _ = r.ParseForm();
        if (r.URL.Path == "/"u8) {
            Redirect(w, r, "/slow?nonce="u8 + r.Form.Get(nonceˢ), StatusFound);
            return;
        }
        if (r.URL.Path == "/slow"u8) {
            Ꮡmu.Lock();
            if (r.Form.Get(nonceˢ) == nonce) {
                sawSlowNonce = true;
            } else {
                Ꮡt.Logf("mismatched nonce: received %s, want %s", r.Form.Get(nonceˢ), nonce);
            }
            Ꮡmu.Unlock();

            w.Write(slice<byte>("Hello"u8));
            w._<Flusher>().Flush();
            ᐸꟷ(r.Context().Done());
            return;
        }
    })));

    // Try to trigger a timeout after reading part of the response body.
    // The initial timeout is empirically usually long enough on a decently fast
    // machine, but if we undershoot we'll retry with exponentially longer
    // timeouts until the test either passes or times out completely.
    // This keeps the test reasonably fast in the typical case but allows it to
    // also eventually succeed on arbitrarily slow machines.
    var timeout = 10 * time.Millisecond;
    nint nextNonce = 0;
    for (; ; timeout *= 2) {
        if (timeout <= 0) {
            // The only way we can feasibly hit this while the test is running is if
            // the request fails without actually waiting for the timeout to occur.
            Ꮡt.Fatalf("timeout overflow");
        }
        {
            var (deadline, okΔ1) = t.Deadline(); if (okΔ1 && !time.Now().Add(timeout).Before(deadline)) {
                Ꮡt.Fatalf("failed to produce expected timeout before test deadline");
            }
        }
        Ꮡt.Logf("attempting test with timeout %v", timeout);
        cst.c.Timeout = timeout;

        Ꮡmu.Lock();
        nonce = fmt.Sprint(nextNonce);
        nextNonce++;
        sawSlowNonce = false;
        Ꮡmu.Unlock();
        var (res, err) = cst.c.Get(cst.ts.URL + "/?nonce="u8 + nonce);
        if (err != nil) {
            if (strings.Contains(err.Error(), clientTimeoutˢ)) {
                // Timed out before handler could respond.
                Ꮡt.Logf("timeout before response received");
                continue;
            }
            if (runtime.GOOS == "windows"u8 && runtime.GOARCH == "arm64"u8) {
                testenv.SkipFlaky(new http_test_package.testing_TжTB(Ꮡt), 43120);
            }
            Ꮡt.Fatal(err);
        }

        Ꮡmu.Lock();
        var ok = sawSlowNonce;
        Ꮡmu.Unlock();
        if (!ok) {
            Ꮡt.Fatal(handlerNeverGotSlowˢ);
        }

        (_, err) = io.ReadAll(res.Body);
        res.Body.Close();

        if (err == nil) {
            Ꮡt.Fatal(expectedErrorFromReadAllˢ);
        }
        (var ne, ok) = err._<netꓸError>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("error value from ReadAll was %T; expected some net.Error", err);
        } else if (!ne.Timeout()) {
            Ꮡt.Errorf("net.Error.Timeout = false; want true");
        }
        if (!errors.Is(err, context_package.DeadlineExceeded)) {
            Ꮡt.Errorf("ReadAll error = %q; expected some context.DeadlineExceeded", err);
        }
        {
            @string got = ne.Error(); if (!strings.Contains(got, clientTimeoutˢ2)) {
                if (runtime.GOOS == "windows"u8 && runtime.GOARCH == "arm64"u8) {
                    testenv.SkipFlaky(new http_test_package.testing_TжTB(Ꮡt), 43120);
                }
                Ꮡt.Errorf("error string = %q; missing timeout substring", got);
            }
        }

        break;
    }
}

// Client.Timeout firing before getting to the body
public static void TestClientTimeout_Headers(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientTimeout_Headers(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotResponseFromGetˢ = (@string)"got response from Get; expected error"u8;
internal static readonly object netErrorTimeoutFalseWantˢ = (@string)"net.Error.Timeout = false; want true"u8;
internal static ref readonly @string clientTimeoutExceededˢ => ref clientTimeoutExceededˢᵇ;
private static readonly @string clientTimeoutExceededˢᵇ = "Client.Timeout exceeded"u8;

internal static void testClientTimeout_Headers(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var donec = new channel<bool>(1);
        var donecʗ1 = donec;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ᐸꟷ(donecʗ1);
        })), (optQuietLog).OrTypedNilFunc());
        // Note that we use a channel send here and not a close.
        // The race detector doesn't know that we're waiting for a timeout
        // and thinks that the waitgroup inside httptest.Server is added to concurrently
        // with us closing it. If we timed out immediately, we could close the testserver
        // before we entered the handler. We're not timing out immediately and there's
        // no way we would be done before we entered the handler, but the race detector
        // doesn't know this, so synchronize explicitly.
        var donecʗ2 = donec;
        defer(() => {
            donecʗ2.ᐸꟷ(true);
        }, ref ᒐ);

        cst.c.Timeout = 5 * time.Millisecond;
        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err == nil) {
            res.Body.Close();
            t.Fatal(gotResponseFromGetˢ);
        }
        {
            var (_, okΔ1) = err._<ж<urlꓸError>>(ᐧ); if (!okΔ1) {
                t.Fatalf("Got error of type %T; want *url.Error", err);
            }
        }
        var (ne, ok) = err._<netꓸError>(ᐧ);
        if (!ok) {
            t.Fatalf("Got error of type %T; want some net.Error", err);
        }
        if (!ne.Timeout()) {
            t.Error(netErrorTimeoutFalseWantˢ);
        }
        if (!errors.Is(err, context_package.DeadlineExceeded)) {
            t.Errorf("ReadAll error = %q; expected some context.DeadlineExceeded", err);
        }
        {
            @string got = ne.Error(); if (!strings.Contains(got, clientTimeoutExceededˢ)) {
                if (runtime.GOOS == "windows"u8 && runtime.GOARCH == "arm64"u8) {
                    testenv.SkipFlaky(new http_test_package.testing_TжTB(t), 43120);
                }
                t.Errorf("error string = %q; missing timeout substring", got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 16094: if Client.Timeout is set but not hit, a Timeout error shouldn't be
// returned.
public static void TestClientTimeoutCancel(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientTimeoutCancel(Δp0, Δp1));
}

internal static void testClientTimeoutCancel(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var testDone = new channel<EmptyStruct>(0);
        var (ctx, cancel) = context_package.WithCancel(context_package.Background());

        var testDoneʗ1 = testDone;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w._<Flusher>().Flush();
            ᐸꟷ(testDoneʗ1);
        })));
        defer(ᴛ1 => builtin.close(ᴛ1), testDone, ref ᒐ);

        cst.c.Timeout = (time.Duration)(3600000000000L);
        var (req, _) = NewRequest(getˢ2, cst.ts.URL, default);
        req.Cancel = ctx.Done();
        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        cancel();
        (_, err) = io.Copy(io.Discard, res.Body);
        if (!AreEqual(err, http_internal_test_package.ExportErrRequestCanceled)) {
            t.Fatalf("error = %v; want errRequestCanceled", err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 49366: if Client.Timeout is set but not hit, no error should be returned.
public static void TestClientTimeoutDoesNotExpire(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientTimeoutDoesNotExpire(Δp0, Δp1));
}

internal static void testClientTimeoutDoesNotExpire(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("body"u8));
    })));

    cst.c.Timeout = (time.Duration)(3600000000000L);
    var (req, _) = NewRequest(getˢ2, cst.ts.URL, default);
    var (res, err) = cst.c.Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        (_, err) = io.Copy(io.Discard, res.Body); if (err != nil) {
            t.Fatalf("io.Copy(io.Discard, res.Body) = %v, want nil", err);
        }
    }
    {
        err = res.Body.Close(); if (err != nil) {
            t.Fatalf("res.Body.Close() = %v, want nil", err);
        }
    }
}

public static void TestClientRedirectEatsBody_h1(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirectEatsBody(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ3 => ref fooˢ3ᵇ;
private static readonly @string fooˢ3ᵇ = "/foo"u8;
internal static readonly object serverDidnTSeeARequestˢ = (@string)"server didn't see a request"u8;
internal static readonly object serverDidnTSeeASecondˢ = (@string)"server didn't see a second request"u8;
internal static readonly object serverSawDifferentClientˢ = (@string)"server saw different client ports before & after the redirect"u8;

internal static void testClientRedirectEatsBody(ж<testing.T> t, testMode mode) {
    var saw = new channel<@string>(2);
    var sawʗ1 = saw;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        sawʗ1.ᐸꟷ(r.RemoteAddr);
        if (r.URL.Path == "/"u8) {
            Redirect(w, r, fooˢ3, StatusFound); // which includes a body
        }
    })));

    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = io.ReadAll(res.Body);
    res.Body.Close();
    if (err != nil) {
        t.Fatal(err);
    }

    @string first = default;
    var selᴛ4 = saw;
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out first): {
        break;
    }
    default: {
        t.Fatal(serverDidnTSeeARequestˢ);
        break;
    }}

    @string second = default;
    var selᴛ5 = saw;
    switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out second): {
        break;
    }
    default: {
        t.Fatal(serverDidnTSeeASecondˢ);
        break;
    }}

    if (first != second) {
        t.Fatal(serverSawDifferentClientˢ);
    }
}

// eofReaderFunc is an io.Reader that runs itself, and then returns io.EOF.
internal delegate void eofReaderFunc();

internal static (nint n, error err) Read(this eofReaderFunc f, slice<byte> p) {
    f();
    return (0, io.EOF);
}

internal partial struct TestReferer_tests /*dyn*/ {
    internal @string lastReq, newReq, explicitRef; // from -> to URLs, explicitly set Referer value
    internal @string want;
}

public static void TestReferer(ж<testing.T> t) {
    var tests = slice<TestReferer_tests>([
        // don't send user:
        new(lastReq: "http://gopher@test.com", newReq: "http://link.com", want: "http://test.com"),
        new(lastReq: "https://gopher@test.com", newReq: "https://link.com", want: "https://test.com"),

        // don't send a user and password:
        new(lastReq: "http://gopher:go@test.com", newReq: "http://link.com", want: "http://test.com"),
        new(lastReq: "https://gopher:go@test.com", newReq: "https://link.com", want: "https://test.com"),

        // nothing to do:
        new(lastReq: "http://test.com", newReq: "http://link.com", want: "http://test.com"),
        new(lastReq: "https://test.com", newReq: "https://link.com", want: "https://test.com"),

        // https to http doesn't send a referer:
        new(lastReq: "https://test.com", newReq: "http://link.com", want: ""),
        new(lastReq: "https://gopher:go@test.com", newReq: "http://link.com", want: ""),

        // https to http should remove an existing referer:
        new(lastReq: "https://test.com", newReq: "http://link.com", explicitRef: "https://foo.com", want: ""),
        new(lastReq: "https://gopher:go@test.com", newReq: "http://link.com", explicitRef: "https://foo.com", want: ""),

        // don't override an existing referer:
        new(lastReq: "https://test.com", newReq: "https://link.com", explicitRef: "https://foo.com", want: "https://foo.com"),
        new(lastReq: "https://gopher:go@test.com", newReq: "https://link.com", explicitRef: "https://foo.com", want: "https://foo.com")
    ]);
    foreach (var (_, tt) in tests) {
        var (l, err) = url.Parse(tt.lastReq);
        if (err != nil) {
            t.Fatal(err);
        }
        (var n, err) = url.Parse(tt.newReq);
        if (err != nil) {
            t.Fatal(err);
        }
        @string r = http_internal_test_package.ExportRefererForURL(l, n, tt.explicitRef);
        if (r != tt.want) {
            t.Errorf("refererForURL(%q, %q) = %q; want %q", tt.lastReq, tt.newReq, r, tt.want);
        }
    }
}

// issue15577Tripper returns a Response with a redirect response
// header and doesn't populate its Response.Request field.
partial struct issue15577Tripper {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this issue15577Tripper _Δp0, ж<Δhttp.Request> _Δp1) {
    var resp = Ꮡ(new Response(
        StatusCode: 303,
        Header: new map<@string, slice<@string>>{["Location"] = slice<@string>(["http://www.example.com/"])},
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)))
    ));
    return (resp, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpDummyTldˢ => ref httpDummyTldˢᵇ;
private static readonly @string httpDummyTldˢᵇ = "http://dummy.tld"u8;

// Issue 15577: don't assume the roundtripper's response populates its Request field.
public static void TestClientRedirectResponseWithoutRequest(ж<testing.T> t) {
    var c = Ꮡ(new Client(
        CheckRedirect: (ж<Δhttp.Request> _Δp0, slice<ж<Δhttp.Request>> _Δp1) => fmt.Errorf("no redirects!"u8),
        Transport: new issue15577Tripper()
    ));
    // Check that this doesn't crash:
    c.Get(httpDummyTldˢ);
}

// Issue 4800: copy (some) headers when Client follows a redirect.
// Issue 35104: Since both URLs have the same host (localhost)
// but different ports, sensitive headers like Cookie and Authorization
// are preserved.
public static void TestClientCopyHeadersOnRedirect(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientCopyHeadersOnRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string resultˢ => ref resultˢᵇ;
private static readonly @string resultˢᵇ = "Result"u8;
internal static ref readonly @string gotErrorsˢ => ref gotErrorsˢᵇ;
private static readonly @string gotErrorsˢᵇ = "got errors"u8;
internal static ref readonly @string userAgentˢ2 => ref userAgentˢ2ᵇ;
private static readonly @string userAgentˢ2ᵇ = "User-Agent"u8;
internal static ref readonly @string xFooˢ => ref xFooˢᵇ;
private static readonly @string xFooˢᵇ = "X-Foo"u8;
internal static ref readonly @string cookieˢ => ref cookieˢᵇ;
private static readonly @string cookieˢᵇ = "Cookie"u8;
internal static ref readonly @string fooBarˢ => ref fooBarˢᵇ;
private static readonly @string fooBarˢᵇ = "foo=bar"u8;
internal static ref readonly @string secretpasswordˢ => ref secretpasswordˢᵇ;
private static readonly @string secretpasswordˢᵇ = "secretpassword"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string uaᶜ => ref uaᶜᵇ;
private static readonly @string uaᶜᵇ = "some-agent/1.2"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string xfooᶜ => ref xfooᶜᵇ;
private static readonly @string xfooᶜᵇ = "foo-val"u8;

internal static void testClientCopyHeadersOnRedirect(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string ua = uaᶜ;
        @string xfoo = xfooᶜ;
        @string ts2URL = default;
        var ts1 = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var want = new httpꓸHeader(new map<@string, slice<@string>>{
                ["User-Agent"] = slice<@string>([ua]),
                ["X-Foo"] = slice<@string>([xfoo]),
                ["Referer"] = slice<@string>([ts2URL]),
                ["Accept-Encoding"] = slice<@string>(["gzip"]),
                ["Cookie"] = slice<@string>(["foo=bar"]),
                ["Authorization"] = slice<@string>(["secretpassword"])
            });
            if (!reflect.DeepEqual(r.Header, want)) {
                t.Errorf("Request.Header = %#v; want %#v", r.Header, want);
            }
            if (t.Failed()) {
                w.Header().Set(resultˢ, gotErrorsˢ);
            } else {
                w.Header().Set(resultˢ, "ok");
            }
        }))).ts;
        var ts1ʗ1 = ts1;
        var ts2 = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            Redirect(w, r, ts1ʗ1.URL, StatusFound);
        }))).ts;
        ts2URL = ts2.URL;

        var c = ts1.Client();
        c.CheckRedirect = error (ж<Δhttp.Request> r, slice<ж<Δhttp.Request>> via) => {
            var want = new httpꓸHeader(new map<@string, slice<@string>>{
                ["User-Agent"] = slice<@string>([ua]),
                ["X-Foo"] = slice<@string>([xfoo]),
                ["Referer"] = slice<@string>([ts2URL]),
                ["Cookie"] = slice<@string>(["foo=bar"]),
                ["Authorization"] = slice<@string>(["secretpassword"])
            });
            if (!reflect.DeepEqual(r.Header, want)) {
                t.Errorf("CheckRedirect Request.Header = %#v; want %#v", r.Header, want);
            }
            return default;
        };

        var (req, _) = NewRequest(getˢ2, ts2.URL, default);
        req.Header.Add(userAgentˢ2, ua);
        req.Header.Add(xFooˢ, xfoo);
        req.Header.Add(cookieˢ, fooBarˢ);
        req.Header.Add(authorizationˢ, secretpasswordˢ);
        var (res, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.StatusCode != 200) {
            t.Fatal(res.Status);
        }
        {
            @string got = res.Header.Get(resultˢ); if (got != "ok"u8) {
                t.Errorf("result = %q; want ok", got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue #70530: Once we strip a header on a redirect to a different host,
// the header should stay stripped across any further redirects.
public static void TestClientStripHeadersOnRepeatedRedirect(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientStripHeadersOnRepeatedRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string proxyAuthorizationˢ => ref proxyAuthorizationˢᵇ;
private static readonly @string proxyAuthorizationˢᵇ = "Proxy-Authorization"u8;
internal static ref readonly @string xDoneˢ => ref xDoneˢᵇ;
private static readonly @string xDoneˢᵇ = "X-Done"u8;
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;

internal static void testClientStripHeadersOnRepeatedRedirect(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string proto = default;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if (r.Host + r.URL.Path != "a.example.com/"u8) {
                {
                    @string h = r.Header.Get(authorizationˢ); if (h != ""u8) {
                        t.Errorf("on request to %v%v, Authorization=%q, want no header", r.Host, r.URL.Path, h);
                    } else {
                        @string hΔ1 = r.Header.Get(proxyAuthorizationˢ); if (hΔ1 != ""u8) {
                            t.Errorf("on request to %v%v, Proxy-Authorization=%q, want no header", r.Host, r.URL.Path, hΔ1);
                        }
                    }
                }
            }
            // Follow a chain of redirects from a to b and back to a.
            // The Authorization header is stripped on the first redirect to b,
            // and stays stripped even if we're sent back to a.
            var exprᴛ1 = r.Host + r.URL.Path;
            switch (ᐧ) {
            case {} when exprᴛ1 == "a.example.com/"u8: {
                Redirect(w, r, proto + "://b.example.com/"u8, StatusFound);
                break;
            }
            case {} when exprᴛ1 == "b.example.com/"u8: {
                Redirect(w, r, proto + "://b.example.com/redirect"u8, StatusFound);
                break;
            }
            case {} when exprᴛ1 == "b.example.com/redirect"u8: {
                Redirect(w, r, proto + "://a.example.com/redirect"u8, StatusFound);
                break;
            }
            case {} when exprᴛ1 == "a.example.com/redirect"u8: {
                w.Header().Set(xDoneˢ, trueˢ);
                break;
            }
            default: {
                t.Errorf("unexpected request to %v", r.URL.OrTypedNil());
                break;
            }}
        }))).ts;
        (proto, _, _) = strings.Cut(ts.URL, ":");

        var c = ts.Client();
        var tsʗ1 = ts;
        c.Transport._<ж<Δhttp.Transport>>().Dial = (@string _Δp0, @string _Δp1) => net.Dial(tcpˢ, tsʗ1.Listener.Addr().String());

        var (req, _) = NewRequest(getˢ2, proto + "://a.example.com/"u8, default);
        req.Header.Add(cookieˢ, fooBarˢ);
        req.Header.Add(authorizationˢ, secretpasswordˢ);
        req.Header.Add(proxyAuthorizationˢ, secretpasswordˢ);
        var (res, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.Header.Get(xDoneˢ) != "true"u8) {
            t.Fatalf("response missing expected header: X-Done=true");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClientStripHeadersOnPostToGetRedirect(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientStripHeadersOnPostToGetRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string redirectedˢ => ref redirectedˢᵇ;
private static readonly @string redirectedˢᵇ = "/redirected"u8;
internal static ref readonly @string contentˢ => ref contentˢᵇ;
private static readonly @string contentˢᵇ = "Content-"u8;
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;
internal static ref readonly @string contentEncodingˢ => ref contentEncodingˢᵇ;
private static readonly @string contentEncodingˢᵇ = "Content-Encoding"u8;
internal static ref readonly @string contentLanguageˢ => ref contentLanguageˢᵇ;
private static readonly @string contentLanguageˢᵇ = "Content-Language"u8;

internal static void testClientStripHeadersOnPostToGetRedirect(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if (r.Method == "POST"u8) {
                Redirect(w, r, redirectedˢ, StatusFound);
                return;
            } else if (r.Method != "GET"u8) {
                t.Errorf("unexpected request method: %v", r.Method);
                return;
            }
            foreach (var (key, val) in r.Header) {
                if (strings.HasPrefix(key, contentˢ)) {
                    t.Errorf("unexpected request body header after redirect: %v: %v", key, val);
                }
            }
        }))).ts;

        var c = ts.Client();

        var (req, _) = NewRequest(postˢ, ts.URL, new http_test_package.strings_ReaderжReader(strings.NewReader(helloWorldˢ)));
        req.Header.Set(contentEncodingˢ, "a");
        req.Header.Set(contentLanguageˢ, "b");
        req.Header.Set(contentLengthˢ, "c");
        req.Header.Set(contentTypeˢ, "d");
        var (res, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 22233: copy host when Client follows a relative redirect.
public static void TestClientCopyHostOnRedirect(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientCopyHostOnRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string shouldNotSeeThisResponseˢ => ref shouldNotSeeThisResponseˢᵇ;
private static readonly @string shouldNotSeeThisResponseˢᵇ = "should not see this response"u8;
internal static ref readonly @string httpˢ2 => ref httpˢ2ᵇ;
private static readonly @string httpˢ2ᵇ = "http://"u8;
internal static ref readonly @string httpsˢ2 => ref httpsˢ2ᵇ;
private static readonly @string httpsˢ2ᵇ = "https://"u8;
internal static ref readonly @string hopˢ => ref hopˢᵇ;
private static readonly @string hopˢᵇ = "/hop"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantBodyᶜ => ref wantBodyᶜᵇ;
private static readonly @string wantBodyᶜᵇ = "response body"u8;

internal static void testClientCopyHostOnRedirect(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        // Virtual hostname: should not receive any request.
        var @virtual = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            t.Errorf("Virtual host received request %v", r.URL.OrTypedNil());
            w.WriteHeader(403);
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), shouldNotSeeThisResponseˢ);
        }))).ts;
        var virtualʗ1 = @virtual;
        defer(virtualʗ1.Close, ref ᒐ);
        @string virtualHost = strings.TrimPrefix(@virtual.URL, httpˢ2);
        virtualHost = strings.TrimPrefix(virtualHost, httpsˢ2);
        t.Logf("Virtual host is %v", virtualHost);

        // Actual hostname: should not receive any request.
        @string wantBody = wantBodyᶜ;
        @string tsURL = default;
        @string tsHost = default;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var exprᴛ1 = r.URL.Path;
            switch (ᐧ) {
            case {} when exprᴛ1 == "/"u8: {
                // Relative redirect.
                if (r.Host != virtualHost) {
                    t.Errorf("Serving /: Request.Host = %#v; want %#v", r.Host, virtualHost);
                    w.WriteHeader(404);
                    return;
                }
                w.Header().Set(locationˢ, hopˢ);
                w.WriteHeader(302);
                break;
            }
            case {} when exprᴛ1 == "/hop"u8: {
                // Absolute redirect.
                if (r.Host != virtualHost) {
                    t.Errorf("Serving /hop: Request.Host = %#v; want %#v", r.Host, virtualHost);
                    w.WriteHeader(404);
                    return;
                }
                w.Header().Set(locationˢ, tsURL + "/final"u8);
                w.WriteHeader(302);
                break;
            }
            case {} when exprᴛ1 == "/final"u8: {
                if (r.Host != tsHost) {
                    t.Errorf("Serving /final: Request.Host = %#v; want %#v", r.Host, tsHost);
                    w.WriteHeader(404);
                    return;
                }
                w.WriteHeader(200);
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), wantBody);
                break;
            }
            default: {
                t.Errorf("Serving unexpected path %q", r.URL.Path);
                w.WriteHeader(404);
                break;
            }}
        }))).ts;
        tsURL = ts.URL;
        tsHost = strings.TrimPrefix(ts.URL, httpˢ2);
        tsHost = strings.TrimPrefix(tsHost, httpsˢ2);
        t.Logf("Server host is %v", tsHost);

        var c = ts.Client();
        var (req, _) = NewRequest(getˢ2, ts.URL, default);
        req.Host = virtualHost;
        var (resp, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var respʗ1 = resp;
        defer(ᴛ0 => ᴛ0.Close(), respʗ1.Body, ref ᒐ);
        if (resp.StatusCode != 200) {
            t.Fatal(resp.Status);
        }
        {
            var (got, errΔ1) = io.ReadAll(resp.Body); if (errΔ1 != nil || ((sstring)got) != wantBody) {
                t.Errorf("body = %q; want %q", got, wantBody);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 17494: cookies should be altered when Client follows redirects.
public static void TestClientAltersCookiesOnRedirect(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientAltersCookiesOnRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cycleˢ => ref cycleˢᵇ;
private static readonly @string cycleˢᵇ = "Cycle"u8;

internal static void testClientAltersCookiesOnRedirect(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        map<@string, slice<@string>> cookieMap(slice<ж<httpꓸCookie>> cs) {
            var m = new map<@string, slice<@string>>();
            foreach (var (_, cΔ1) in cs) {
                m[cΔ1.Name] = append(m[cΔ1.Name], (~cΔ1).Value);
            }
            return m;
        }

        var cookieMapʗ1 = cookieMap;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            map<@string, slice<@string>> want = default;
            var got = cookieMapʗ1(r.Cookies());

            var (cΔ2, _) = r.Cookie(cycleˢ);
            var exprᴛ1 = (~cΔ2).Value;
            switch (ᐧ) {
            case {} when exprᴛ1 == "0"u8: {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"] = slice<@string>(["OldValue1a", "OldValue1b"]),
                    ["Cookie2"] = slice<@string>(["OldValue2"]),
                    ["Cookie3"] = slice<@string>(["OldValue3a", "OldValue3b"]),
                    ["Cookie4"] = slice<@string>(["OldValue4"]),
                    ["Cycle"] = slice<@string>(["0"])
                };
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cycle", Value: "1", Path: "/")));
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie2", Path: "/", MaxAge: -1))); // Delete cookie from Header
                Redirect(w, r, "/", StatusFound);
                break;
            }
            case {} when exprᴛ1 == "1"u8: {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"] = slice<@string>(["OldValue1a", "OldValue1b"]),
                    ["Cookie3"] = slice<@string>(["OldValue3a", "OldValue3b"]),
                    ["Cookie4"] = slice<@string>(["OldValue4"]),
                    ["Cycle"] = slice<@string>(["1"])
                };
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cycle", Value: "2", Path: "/")));
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie3", Value: "NewValue3", Path: "/"))); // Modify cookie in Header
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie4", Value: "NewValue4", Path: "/"))); // Modify cookie in Jar
                Redirect(w, r, "/", StatusFound);
                break;
            }
            case {} when exprᴛ1 == "2"u8: {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"] = slice<@string>(["OldValue1a", "OldValue1b"]),
                    ["Cookie3"] = slice<@string>(["NewValue3"]),
                    ["Cookie4"] = slice<@string>(["NewValue4"]),
                    ["Cycle"] = slice<@string>(["2"])
                };
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cycle", Value: "3", Path: "/")));
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie5", Value: "NewValue5", Path: "/"))); // Insert cookie into Jar
                Redirect(w, r, "/", StatusFound);
                break;
            }
            case {} when exprᴛ1 == "3"u8: {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"] = slice<@string>(["OldValue1a", "OldValue1b"]),
                    ["Cookie3"] = slice<@string>(["NewValue3"]),
                    ["Cookie4"] = slice<@string>(["NewValue4"]),
                    ["Cookie5"] = slice<@string>(["NewValue5"]),
                    ["Cycle"] = slice<@string>(["3"])
                };
                // Don't redirect to ensure the loop ends.
                break;
            }
            default: {
                t.Errorf("unexpected redirect cycle");
                return;
            }}

            if (!reflect.DeepEqual(got, want)) {
                t.Errorf("redirect %s, Cookie = %v, want %v", (~cΔ2).Value, got, want);
            }
        }))).ts;

        var (jar, _) = cookiejar.New(nil);
        var c = ts.Client();
        c.Jar = new http_test_package.cookiejar_JarжCookieJar(jar);

        var (u, _) = url.Parse(ts.URL);
        var (req, _) = NewRequest(getˢ2, ts.URL, default);
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie1", Value: "OldValue1a")));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie1", Value: "OldValue1b")));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie2", Value: "OldValue2")));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie3", Value: "OldValue3a")));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie3", Value: "OldValue3b")));
        jar.SetCookies(u, slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "Cookie4", Value: "OldValue4", Path: "/"))]));
        jar.SetCookies(u, slice<ж<httpꓸCookie>>([Ꮡ(new httpꓸCookie(Name: "Cycle", Value: "0", Path: "/"))]));
        var (res, err) = c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.StatusCode != 200) {
            t.Fatal(res.Status);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestShouldCopyHeaderOnRedirect_tests /*dyn*/ {
    internal @string initialURL;
    internal @string destURL;
    internal bool want;
}

// Part of Issue 4800
public static void TestShouldCopyHeaderOnRedirect(ж<testing.T> t) {
    var tests = slice<TestShouldCopyHeaderOnRedirect_tests>([
        // Sensitive headers:
        new("http://foo.com/", "http://bar.com/", false),
        new("http://foo.com/", "http://bar.com/", false),
        new("http://foo.com/", "http://bar.com/", false),
        new("http://foo.com/", "https://foo.com/", true),
        new("http://foo.com:1234/", "http://foo.com:4321/", true),
        new("http://foo.com/", "http://bar.com/", false),
        new("http://foo.com/", "http://[::1%25.foo.com]/", false),

        // But subdomains should work:
        new("http://foo.com/", "http://foo.com/", true),
        new("http://foo.com/", "http://sub.foo.com/", true),
        new("http://foo.com/", "http://notfoo.com/", false),
        new("http://foo.com/", "https://foo.com/", true),
        new("http://foo.com:80/", "http://foo.com/", true),
        new("http://foo.com:80/", "http://sub.foo.com/", true),
        new("http://foo.com:443/", "https://foo.com/", true),
        new("http://foo.com:443/", "https://sub.foo.com/", true),
        new("http://foo.com:1234/", "http://foo.com/", true),

        new("http://foo.com/", "http://foo.com/", true),
        new("http://foo.com/", "http://sub.foo.com/", true),
        new("http://foo.com/", "http://notfoo.com/", false),
        new("http://foo.com/", "https://foo.com/", true),
        new("http://foo.com:80/", "http://foo.com/", true),
        new("http://foo.com:80/", "http://sub.foo.com/", true),
        new("http://foo.com:443/", "https://foo.com/", true),
        new("http://foo.com:443/", "https://sub.foo.com/", true),
        new("http://foo.com:1234/", "http://foo.com/", true)
    ]);
    foreach (var (i, tt) in tests) {
        var (u0, err) = url.Parse(tt.initialURL);
        if (err != nil) {
            t.Errorf("%d. initial URL %q parse error: %v", i, tt.initialURL, err);
            continue;
        }
        (var u1, err) = url.Parse(tt.destURL);
        if (err != nil) {
            t.Errorf("%d. dest URL %q parse error: %v", i, tt.destURL, err);
            continue;
        }
        var got = http_internal_test_package.Export_shouldCopyHeaderOnRedirect(u0, u1);
        if (got != tt.want) {
            t.Errorf("%d. shouldCopyHeaderOnRedirect(%q => %q) = %v; want %v",
                i, tt.initialURL, tt.destURL, got, tt.want);
        }
    }
}

public static void TestClientRedirectTypes(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientRedirectTypes(Δp0, Δp1));
}

internal partial struct testClientRedirectTypes_tests /*dyn*/ {
    internal @string method;
    internal nint serverStatus;
    internal @string wantMethod; // desired subsequent client method
}

internal static void testClientRedirectTypes(ж<testing.T> t, testMode mode) {
    var tests = new array<testClientRedirectTypes_tests>(30){
        [0] = new(method: "POST", serverStatus: 301, wantMethod: "GET"),
        [1] = new(method: "POST", serverStatus: 302, wantMethod: "GET"),
        [2] = new(method: "POST", serverStatus: 303, wantMethod: "GET"),
        [3] = new(method: "POST", serverStatus: 307, wantMethod: "POST"),
        [4] = new(method: "POST", serverStatus: 308, wantMethod: "POST"),

        [5] = new(method: "HEAD", serverStatus: 301, wantMethod: "HEAD"),
        [6] = new(method: "HEAD", serverStatus: 302, wantMethod: "HEAD"),
        [7] = new(method: "HEAD", serverStatus: 303, wantMethod: "HEAD"),
        [8] = new(method: "HEAD", serverStatus: 307, wantMethod: "HEAD"),
        [9] = new(method: "HEAD", serverStatus: 308, wantMethod: "HEAD"),

        [10] = new(method: "GET", serverStatus: 301, wantMethod: "GET"),
        [11] = new(method: "GET", serverStatus: 302, wantMethod: "GET"),
        [12] = new(method: "GET", serverStatus: 303, wantMethod: "GET"),
        [13] = new(method: "GET", serverStatus: 307, wantMethod: "GET"),
        [14] = new(method: "GET", serverStatus: 308, wantMethod: "GET"),

        [15] = new(method: "DELETE", serverStatus: 301, wantMethod: "GET"),
        [16] = new(method: "DELETE", serverStatus: 302, wantMethod: "GET"),
        [17] = new(method: "DELETE", serverStatus: 303, wantMethod: "GET"),
        [18] = new(method: "DELETE", serverStatus: 307, wantMethod: "DELETE"),
        [19] = new(method: "DELETE", serverStatus: 308, wantMethod: "DELETE"),

        [20] = new(method: "PUT", serverStatus: 301, wantMethod: "GET"),
        [21] = new(method: "PUT", serverStatus: 302, wantMethod: "GET"),
        [22] = new(method: "PUT", serverStatus: 303, wantMethod: "GET"),
        [23] = new(method: "PUT", serverStatus: 307, wantMethod: "PUT"),
        [24] = new(method: "PUT", serverStatus: 308, wantMethod: "PUT"),

        [25] = new(method: "MADEUPMETHOD", serverStatus: 301, wantMethod: "GET"),
        [26] = new(method: "MADEUPMETHOD", serverStatus: 302, wantMethod: "GET"),
        [27] = new(method: "MADEUPMETHOD", serverStatus: 303, wantMethod: "GET"),
        [28] = new(method: "MADEUPMETHOD", serverStatus: 307, wantMethod: "MADEUPMETHOD"),
        [29] = new(method: "MADEUPMETHOD", serverStatus: 308, wantMethod: "MADEUPMETHOD")
    };

    var handlerc = new channel<Δhttp.HandlerFunc>(1);

    var handlercʗ1 = handlerc;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        var h = ᐸꟷ(handlercʗ1);
        h(rw, req);
    }))).ts;

    var c = ts.Client();
    foreach (var (i, vᴛ1) in tests.ΔRangeSnapshot()) {
        ref var tt = ref heap(new testClientRedirectTypes_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var tsʗ1 = ts;
        var ttʗ1 = tt;
        handlerc.ᐸꟷ((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(locationˢ, tsʗ1.URL);
            w.WriteHeader(ttʗ1.serverStatus);
        });

        var (req, err) = NewRequest(tt.method, ts.URL, default);
        if (err != nil) {
            t.Errorf("#%d: NewRequest: %v", i, err);
            continue;
        }

        var handlercʗ2 = handlerc;
        var ttʗ2 = tt;
        c.CheckRedirect = error (ж<Δhttp.Request> reqΔ1, slice<ж<Δhttp.Request>> via) => {
            {
                @string got = reqΔ1.Method;
                @string want = ttʗ2.wantMethod; if (got != want) {
                    return fmt.Errorf("#%d: got next method %q; want %q"u8, i, got, want);
                }
            }
            handlercʗ2.ᐸꟷ((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ2) => {
                // TODO: Check that the body is valid when we do 307 and 308 support
            });
            return default;
        };

        (var res, err) = c.Do(req);
        if (err != nil) {
            t.Errorf("#%d: Response: %v", i, err);
            continue;
        }

        res.Body.Close();
    }
}

// issue18239Body is an io.ReadCloser for TestTransportBodyReadError.
// Its Read returns readErr and increments *readCalls atomically.
// Its Close returns nil and increments *closeCalls atomically.
partial struct issue18239Body {
    internal ж<int32> readCalls;
    internal ж<int32> closeCalls;
    internal error readErr;
}

internal static (nint, error) Read(this issue18239Body b, slice<byte> _) {
    atomic.AddInt32(b.readCalls, 1);
    return (0, b.readErr);
}

internal static error Close(this issue18239Body b) {
    atomic.AddInt32(b.closeCalls, 1);
    return default;
}

// Issue 18239: make sure the Transport doesn't retry requests with bodies
// if Request.GetBody is not defined.
public static void TestTransportBodyReadError(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testTransportBodyReadError(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xBodyReadˢ => ref xBodyReadˢᵇ;
private static readonly @string xBodyReadˢᵇ = "X-Body-Read"u8;
internal static ref readonly @string someBodyReadErrorˢ => ref someBodyReadErrorˢᵇ;
private static readonly @string someBodyReadErrorˢᵇ = "some body read error"u8;

internal static void testTransportBodyReadError(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if (r.URL.Path == "/ping"u8) {
            return;
        }
        var buf = new slice<byte>(1);
        var (n, errΔ1) = r.Body.Read(buf);
        w.Header().Set(xBodyReadˢ, fmt.Sprintf("%v, %v"u8, n, errΔ1));
    }))).ts;
    var c = ts.Client();
    var tr = c.Transport._<ж<Δhttp.Transport>>();

    // Do one initial successful request to create an idle TCP connection
    // for the subsequent request to reuse. (The Transport only retries
    // requests on reused connections.)
    var (res, err) = c.Get(ts.URL + "/ping"u8);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();

    ref var readCallsAtomic = ref heap(new int32(), out var ᏑreadCallsAtomic);
    ref var closeCallsAtomic = ref heap(new int32(), out var ᏑcloseCallsAtomic); // atomic
    var someErr = errors.New(someBodyReadErrorˢ);
    var body = new issue18239Body(ᏑreadCallsAtomic, ᏑcloseCallsAtomic, someErr);

    (var req, err) = NewRequest(postˢ, ts.URL, body);
    if (err != nil) {
        t.Fatal(err);
    }
    req = req.WithT(t);
    (_, err) = tr.RoundTrip(req);
    if (!AreEqual(err, someErr)) {
        t.Errorf("Got error: %v; want Request.Body read error: %v", err, someErr);
    }

    // And verify that our Body wasn't used multiple times, which
    // would indicate retries. (as it buggily was during part of
    // Go 1.8's dev cycle)
    var readCalls = atomic.LoadInt32(ᏑreadCallsAtomic);
    var closeCalls = atomic.LoadInt32(ᏑcloseCallsAtomic);
    if (readCalls != 1) {
        t.Errorf("read calls = %d; want 1", readCalls);
    }
    if (closeCalls != 1) {
        t.Errorf("close calls = %d; want 1", closeCalls);
    }
}

// Make sure the retries copies the GetBody in the request.
public static void TestRedirectGetBody(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testRedirectGetBody(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string secondˢ => ref secondˢᵇ;
private static readonly @string secondˢᵇ = "/second"u8;
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;

internal static void testRedirectGetBody(ж<testing.T> t, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (bΔ1, errΔ1) = io.ReadAll(r.Body);
        if (errΔ1 != nil) {
            t.Error(errΔ1);
        }
        {
            errΔ1 = r.Body.Close(); if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
        }
        {
            @string s = (@string)bΔ1; if (s != "hello"u8) {
                t.Errorf("expected hello, got %s", s);
            }
        }
        if (r.URL.Path == "/first"u8) {
            Redirect(w, r, secondˢ, StatusTemporaryRedirect);
            return;
        }
        w.Write(slice<byte>("world"u8));
    }))).ts;
    var c = ts.Client();
    c.Transport = new http_test_package.roundTripperGetBodyжRoundTripper(Ꮡ(new roundTripperGetBody(c.Transport, t)));
    var (req, err) = NewRequest(postˢ, ts.URL + "/first"u8, new http_test_package.strings_ReaderжReader(strings.NewReader(helloˢ)));
    if (err != nil) {
        t.Fatal(err);
    }
    (var res, err) = c.Do(req.WithT(t));
    if (err != nil) {
        t.Fatal(err);
    }
    (var b, err) = io.ReadAll(res.Body);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        err = res.Body.Close(); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        @string s = (@string)b; if (s != "world"u8) {
            t.Fatalf("expected world, got %s", s);
        }
    }
}

partial struct roundTripperGetBody {
    public Δhttp.RoundTripper Transport;
    internal ж<testing.T> t;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object missingRequestGetBodyˢ = (@string)"missing Request.GetBody"u8;

internal static partial (ж<Δhttp.Response>, error) RoundTrip(this ref roundTripperGetBody r, ж<Δhttp.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    if (req.GetBody == nil) {
        r.t.Error(missingRequestGetBodyˢ);
    }
    return r.Transport.RoundTrip(Ꮡreq);
}

partial struct roundTripperWithoutCloseIdle {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this roundTripperWithoutCloseIdle _Δp0, ж<Δhttp.Request> _Δp1) {
    throw panic("unused");
}

internal delegate void roundTripperWithCloseIdle();

internal static (ж<Δhttp.Response>, error) RoundTrip(this roundTripperWithCloseIdle _Δp0, ж<Δhttp.Request> _Δp1) {
    throw panic("unused");
}

internal static partial void CloseIdleConnections(this roundTripperWithCloseIdle f) {
    f();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notClosedˢ = (@string)"not closed"u8;

public static void TestClientCloseIdleConnections(ж<testing.T> t) {
    var c = Ꮡ(new Client(Transport: new roundTripperWithoutCloseIdle()));
    c.CloseIdleConnections(); // verify we don't crash at least

    var closed = false;
    Δhttp.RoundTripper tr = new http_test_package.roundTripperWithCloseIdleᴠRoundTripper(new roundTripperWithCloseIdle(() => {
        closed = true;
    }));
    c = Ꮡ(new Client(Transport: tr));
    c.CloseIdleConnections();
    if (!closed) {
        t.Error(notClosedˢ);
    }
}

internal delegate (ж<Δhttp.Response>, error) testRoundTripper(ж<Δhttp.Request> _);

internal static partial (ж<Δhttp.Response>, error) RoundTrip(this testRoundTripper t, ж<Δhttp.Request> req) {
    return t(req);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noDeadlineˢ = (@string)"no deadline"u8;
internal static ref readonly @string notActuallyMakingAˢ => ref notActuallyMakingAˢᵇ;
private static readonly @string notActuallyMakingAˢᵇ = "not actually making a request"u8;
internal static ref readonly @string httpsExampleTldˢ => ref httpsExampleTldˢᵇ;
private static readonly @string httpsExampleTldˢᵇ = "https://example.tld/"u8;

public static void TestClientPropagatesTimeoutToContext(ж<testing.T> t) {
    var c = Ꮡ(new Client(
        Timeout: (time.Duration)(5000000000L),
        Transport: new http_test_package.testRoundTripperᴠRoundTripper(new testRoundTripper((ж<Δhttp.Request> req) => {
            var ctx = req.Context();
            var (deadline, ok) = ctx.Deadline();
            if (!ok) {
                t.Error(noDeadlineˢ);
            } else {
                t.Logf("deadline in %v", deadline.Sub(time.Now()).Round(time.ΔSecond / 10));
            }
            return (default, errors.New(notActuallyMakingAˢ));
        }))
    ));
    c.Get(httpsExampleTldˢ);
}

// Issue 33545: lock-in the behavior promised by Client.Do's
// docs about request cancellation vs timing out.
public static void TestClientDoCanceledVsTimeout(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientDoCanceledVsTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedlyGotANilErrorˢ = (@string)"Unexpectedly got a nil error"u8;

internal static void testClientDoCanceledVsTimeout(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello, World!"u8));
    })));

    var cases = slice<@string>(["timeout", "canceled"]);

    foreach (var (_, name) in cases) {
        var cstʗ1 = cst;
        t.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                context.Context ctx = default;
                Action cancel = default;
                if (name == "timeout"u8) {
                    (ctx, cancel) = context_package.WithTimeout(context_package.Background(), -time.ΔNanosecond);
                } else {
                    (ctx, cancel) = context_package.WithCancel(context_package.Background());
                    cancel();
                }
                var cancelʗ1 = cancel;
                defer(cancelʗ1, ref ᒐ);

                var (req, _) = NewRequestWithContext(ctx, getˢ2, cstʗ1.ts.URL, default);
                var (_, err) = cstʗ1.c.Do(req);
                if (err == nil) {
                    tΔ1.Fatal(unexpectedlyGotANilErrorˢ);
                }

                var ue = err._<ж<urlꓸError>>();

                bool wantIsTimeout = default;
                error wantErr = context_package.Canceled;
                if (name == "timeout"u8) {
                    wantErr = context_package.DeadlineExceeded;
                    wantIsTimeout = true;
                }
                {
                    var (g, w) = (ue.Timeout(), wantIsTimeout); if (g != w) {
                        tΔ1.Fatalf("url.Timeout() = %t, want %t", g, w);
                    }
                }
                {
                    var (g, w) = (ue.Err, wantErr); if (!AreEqual(g, w)) {
                        tΔ1.Errorf("url.Error.Err = %v; want %v", g, w);
                    }
                }
                {
                    var got = errors.Is(err, context_package.DeadlineExceeded); if (got != wantIsTimeout) {
                        tΔ1.Errorf("errors.Is(err, context.DeadlineExceeded) = %v, want %v", got, wantIsTimeout);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

partial struct nilBodyRoundTripper {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this nilBodyRoundTripper _, ж<Δhttp.Request> req) {
    return (Ꮡ(new Response(
        StatusCode: StatusOK,
        Status: StatusText(StatusOK),
        Body: default,
        Request: req
    )), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpLocalhostAnythingˢ => ref httpLocalhostAnythingˢᵇ;
private static readonly @string httpLocalhostAnythingˢᵇ = "http://localhost/anything"u8;

public static void TestClientPopulatesNilResponseBody(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var c = Ꮡ(new Client(Transport: new nilBodyRoundTripper()));

        var (resp, err) = c.Get(httpLocalhostAnythingˢ);
        if (err != nil) {
            t.Fatalf("Client.Get rejected Response with nil Body: %v", err);
        }

        if (resp.Body == nil) {
            t.Fatalf("Client failed to provide a non-nil Body as documented");
        }
        var respʗ1 = resp;
        defer(() => {
            {
                var errΔ1 = respʗ1.Body.Close(); if (errΔ1 != nil) {
                    t.Fatalf("error from Close on substitute Response.Body: %v", errΔ1);
                }
            }
        }, ref ᒐ);

        {
            var (b, errΔ2) = io.ReadAll(resp.Body); if (errΔ2 != nil) {
                t.Errorf("read error from substitute Response.Body: %v", errΔ2);
            } else if (len(b) != 0) {
                t.Errorf("substitute Response.Body was unexpectedly non-empty: %q", b);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 40382: Client calls Close multiple times on Request.Body.
public static void TestClientCallsCloseOnlyOnce(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testClientCallsCloseOnlyOnce(Δp0, Δp1));
}

internal static void testClientCallsCloseOnlyOnce(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(StatusNoContent);
    })));

    // Issue occurred non-deterministically: needed to occur after a successful
    // write (into TCP buffer) but before end of body.
    for (nint i = 0; i < 50 && !t.Failed(); i++) {
        var body = Ꮡ(new issue40382Body(t: t, n: 300000));
        var (req, err) = NewRequest(MethodPost, cst.ts.URL, new http_test_package.issue40382BodyжReader(body));
        if (err != nil) {
            t.Fatal(err);
        }
        (var resp, err) = cst.tr.RoundTrip(req);
        if (err != nil) {
            t.Fatal(err);
        }
        resp.Body.Close();
    }
}

// issue40382Body is an io.ReadCloser for TestClientCallsCloseOnlyOnce.
// Its Read reads n bytes before returning io.EOF.
// Its Close returns nil but fails the test if called more than once.
partial struct issue40382Body {
    internal ж<testing.T> t;
    internal nint n;
    internal int32 closeCallsAtomic;
}

internal static (nint, error) Read(this ref issue40382Body b, slice<byte> p) {
    var matchᴛ1 = false;
    if (b.n is 0) { matchᴛ1 = true;
        return (0, io.EOF);
    }
    if (b.n < len(p)) { matchᴛ1 = true;
        p = p.slice(0, b.n);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        foreach (var i in range(p)) {
            p[i] = (byte)'x';
        }
        b.n -= len(p);
        return (len(p), default);
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bodyClosedMoreThanOnceˢ = (@string)"Body closed more than once"u8;

internal static partial error Close(this ж<issue40382Body> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (atomic.AddInt32(Ꮡb.ᏑcloseCallsAtomic, 1) == 2) {
        b.t.Error(bodyClosedMoreThanOnceˢ);
    }
    return default;
}

public static void TestProbeZeroLengthBody(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testProbeZeroLengthBody(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentᶜ => ref contentᶜᵇ;
private static readonly @string contentᶜᵇ = "body"u8;

internal static partial void testProbeZeroLengthBody(ж<testing.T> t, testMode mode) {
    var reqc = new channel<EmptyStruct>(0);
    var reqcʗ1 = reqc;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        builtin.close(reqcʗ1);
        {
            var (_, err) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), r.Body); if (err != nil) {
                t.Errorf("error copying request body: %v", err);
            }
        }
    })));

    var (bodyr, bodyw) = io.Pipe();
    @string gotBody = default;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var bodyrʗ1 = bodyr;
    var cstʗ1 = cst;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var (req, _) = NewRequest(getˢ2, cstʗ1.ts.URL, new io.PipeReaderжReader(bodyrʗ1));
            var (res, err) = cstʗ1.c.Do(req);
            (var b, err) = io.ReadAll(res.Body);
            if (err != nil) {
                t.Error(err);
            }
            gotBody = (@string)b;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    var selᴛ6 = reqc;
    var selᴛ7 = time.After((time.Duration)(60000000000L));
    switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ), ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
    case 0 when selᴛ6.ꟷᐳ(out _): {
        // Request should be sent after trying to probe the request body for 200ms.
        break;
    }
    case 1 when selᴛ7.ꟷᐳ(out _): {
        t.Errorf("request not sent after 60s");
        break;
    }}

    // Write the request body and wait for the request to complete.
    @string content = contentᶜ;
    bodyw.Write(slice<byte>(content));
    bodyw.Close();
    Ꮡwg.Wait();
    if (gotBody != content) {
        t.Fatalf("server got body %q, want %q", gotBody, content);
    }
}

} // end http_test_package
