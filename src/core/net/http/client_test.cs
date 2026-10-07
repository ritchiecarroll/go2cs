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
    w.Header().Set("Last-Modified"u8, "sometime"u8);
    fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "User-agent: go\nDisallow: /something/"u8);
});

// pedanticReadAll works like io.ReadAll but additionally
// verifies that r obeys the documented io.Reader contract.
internal static (slice<byte> b, error err) pedanticReadAll(io.Reader r) {
    slice<byte> b = default!;

    array<byte> bufa = new(64);
    var buf = bufa[..];
    while (ᐧ) {
        var (n, errΔ1) = r.Read(buf);
        if (n == 0 && errΔ1 == default!) {
            return (default!, fmt.Errorf("Read: n=0 with err=nil"u8));
        }
        b = appendꓸꓸꓸ(b, buf.slice(0, n));
        if (AreEqual(errΔ1, io.EOF)) {
            var (nΔ1, errΔ2) = r.Read(buf);
            if (nΔ1 != 0 || !AreEqual(errΔ2, io.EOF)) {
                return (default!, fmt.Errorf("Read: n=%d err=%#v after EOF"u8, nΔ1, errΔ2));
            }
            return (b, default!);
        }
        if (errΔ1 != default!) {
            return (b, errΔ1);
        }
    }
}

public static void TestClient(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClient(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string userAgentˢ = "User-agent:"u8;

internal static void testClient(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(robotsTxtHandler)).Value.ts;
    var c = ts.Client();
    var (r, err) = c.Get((~ts).URL);
    slice<byte> b = default!;
    if (err == default!) {
        (b, err) = pedanticReadAll(new http_test_package.io_ReadCloserᴠReader((~r).Body));
        (~r).Body.Close();
    }
    if (err != default!){
        Ꮡt.Error(err);
    } else 
    {
        @string s = ((@string)b); if (!strings.HasPrefix(s, userAgentˢ)) {
            Ꮡt.Errorf("Incorrect page body (did not begin with User-agent): %q"u8, s);
        }
    }
}

public static void TestClientHead(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientHead(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lastModifiedˢ = "Last-Modified"u8;
internal static readonly object lastModifiedHeaderNotˢ = (@string)"Last-Modified header not found."u8;

internal static void testClientHead(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(robotsTxtHandler));
    var (r, err) = (~cst).c.Head((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (_, ok) = (~r).Header[lastModifiedˢ, ꟷ]; if (!ok) {
            Ꮡt.Error(lastModifiedHeaderNotˢ);
        }
    }
}

[GoType] partial struct recordingTransport {
    internal ж<Δhttp.Request> req;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dummyImplˢ = "dummy impl"u8;

[GoRecv] internal static (ж<Δhttp.Response> resp, error err) RoundTrip(this ref recordingTransport t, ж<Δhttp.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    t.req = Ꮡreq;
    return (default!, errors.New(dummyImplˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpDummyFaketldˢ = "http://dummy.faketld/"u8;

public static void TestGetRequestFormat(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        setParallel(Ꮡt);
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new recordingTransport(nil));
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        @string url = httpDummyFaketldˢ;
        client.Get(url); // Note: doesn't hit network
        if ((~(~tr).req).Method != "GET"u8) {
            Ꮡt.Errorf("expected method %q; got %q"u8, getˢ2, (~(~tr).req).Method);
        }
        if ((~(~tr).req).URL.String() != url) {
            Ꮡt.Errorf("expected URL %q; got %q"u8, url, (~(~tr).req).URL.String());
        }
        if ((~(~tr).req).Header == default!) {
            Ꮡt.Errorf("expected non-nil request Header"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyValueˢ = @"{""key"":""value""}"u8;
internal static readonly @string applicationJsonˢ = "application/json"u8;
internal static readonly @string postˢ = "POST"u8;
internal static readonly object gotCloseTrueWantFalseˢ = (@string)"got Close true, want false"u8;

public static void TestPostRequestFormat(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new recordingTransport(nil));
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        @string url = httpDummyFaketldˢ;
        @string json = keyValueˢ;
        var b = strings.NewReader(json);
        client.Post(url, applicationJsonˢ, new http_test_package.strings_ReaderжReader(b)); // Note: doesn't hit network
        if ((~(~tr).req).Method != "POST"u8) {
            Ꮡt.Errorf("got method %q, want %q"u8, (~(~tr).req).Method, postˢ);
        }
        if ((~(~tr).req).URL.String() != url) {
            Ꮡt.Errorf("got URL %q, want %q"u8, (~(~tr).req).URL.String(), url);
        }
        if ((~(~tr).req).Header == default!) {
            Ꮡt.Fatalf("expected non-nil request Header"u8);
        }
        if ((~(~tr).req).Close) {
            Ꮡt.Error(gotCloseTrueWantFalseˢ);
        }
        {
            var (g, e) = (tr.Value.req.Value.ContentLength, (int64)len(json)); if (g != e) {
                Ꮡt.Errorf("got ContentLength %d, want %d"u8, g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string bar2ˢ = "bar2"u8;
internal static readonly @string bazˢ = "baz"u8;
internal static readonly @string contentTypeˢ = "Content-Type"u8;
internal static readonly @string applicationXWwwFormˢ = "application/x-www-form-urlencoded"u8;
internal static readonly @string fooBarFooBar2BarBazˢ = "foo=bar&foo=bar2&bar=baz"u8;
internal static readonly @string barBazFooBarFooBar2ˢ = "bar=baz&foo=bar&foo=bar2"u8;

public static void TestPostFormRequestFormat(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new recordingTransport(nil));
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        @string urlStr = httpDummyFaketldˢ;
        var form = new url.Values(0);
        form.Set(fooˢ, barˢ);
        form.Add(fooˢ, bar2ˢ);
        form.Set(barˢ, bazˢ);
        client.PostForm(urlStr, form); // Note: doesn't hit network
        if ((~(~tr).req).Method != "POST"u8) {
            Ꮡt.Errorf("got method %q, want %q"u8, (~(~tr).req).Method, postˢ);
        }
        if ((~(~tr).req).URL.String() != urlStr) {
            Ꮡt.Errorf("got URL %q, want %q"u8, (~(~tr).req).URL.String(), urlStr);
        }
        if ((~(~tr).req).Header == default!) {
            Ꮡt.Fatalf("expected non-nil request Header"u8);
        }
        {
            @string g = (~(~tr).req).Header.Get(contentTypeˢ);
            @string e = applicationXWwwFormˢ; if (g != e) {
                Ꮡt.Errorf("got Content-Type %q, want %q"u8, g, e);
            }
        }
        if ((~(~tr).req).Close) {
            Ꮡt.Error(gotCloseTrueWantFalseˢ);
        }
        // Depending on map iteration, body can be either of these.
        @string expectedBody = fooBarFooBar2BarBazˢ;
        @string expectedBody1 = barBazFooBarFooBar2ˢ;
        {
            var (g, e) = (tr.Value.req.Value.ContentLength, (int64)len(expectedBody)); if (g != e) {
                Ꮡt.Errorf("got ContentLength %d, want %d"u8, g, e);
            }
        }
        var (bodyb, err) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader((~(~tr).req).Body));
        if (err != default!) {
            Ꮡt.Fatalf("ReadAll on req.Body: %v"u8, err);
        }
        {
            @string g = ((@string)bodyb); if (g != expectedBody && g != expectedBody1) {
                Ꮡt.Errorf("got body %q, want %q or %q"u8, g, expectedBody, expectedBody1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClientRedirects(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirects(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getN10StoppedAfter10ˢ = @"Get ""/?n=10"": stopped after 10 redirects"u8;
internal static readonly @string headN10StoppedAfter10ˢ = @"Head ""/?n=10"": stopped after 10 redirects"u8;
internal static readonly @string nilˢ = "<nil>"u8;
internal static readonly @string n15ˢ = "/?n=15"u8;
internal static readonly @string headˢ = "HEAD"u8;
internal static readonly object didnTSeeRedirectˢ = (@string)"didn't see redirect"u8;
internal static readonly @string noRedirectsAllowedˢ = "no redirects allowed"u8;
internal static readonly @string locationˢ = "Location"u8;

internal static void testClientRedirects(ж<testing.T> Ꮡt, testMode mode) {
    ref var ts = ref heap<ж<httptest.Server>>(out var Ꮡts);
    ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (n, _) = strconv.Atoi(r.FormValue("n"u8));
        // Test Referer header. (7 is arbitrary position to test at)
        if (n == 7) {
            {
                @string g = r.Referer();
                @string e = (~Ꮡts.ValueSlot).URL + "/?n=6"u8; if (e != g) {
                    Ꮡt.Errorf("on request ?n=7, expected referer of %q; got %q"u8, e, g);
                }
            }
        }
        if (n < 15) {
            Redirect(w, r, fmt.Sprintf("/?n=%d"u8, n + 1), StatusTemporaryRedirect);
            return;
        }
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "n=%d"u8, n);
    }))).Value.ts;
    var c = ts.Client();
    var (_, err) = c.Get((~ts).URL);
    {
        @string e = getN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            Ꮡt.Errorf("with default client Get, expected error %q, got %q"u8, e, g);
        }
    }
    // HEAD request should also have the ability to follow redirects.
    (_, err) = c.Head((~ts).URL);
    {
        @string e = headN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            Ꮡt.Errorf("with default client Head, expected error %q, got %q"u8, e, g);
        }
    }
    // Do should also follow redirects.
    var (greq, _) = NewRequest(getˢ2, (~ts).URL, default!);
    (_, err) = c.Do(greq);
    {
        @string e = getN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            Ꮡt.Errorf("with default client Do, expected error %q, got %q"u8, e, g);
        }
    }
    // Requests with an empty Method should also redirect (Issue 12705)
    greq.Value.Method = ""u8;
    (_, err) = c.Do(greq);
    {
        @string e = getN10StoppedAfter10ˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            Ꮡt.Errorf("with default client Do and empty Method, expected error %q, got %q"u8, e, g);
        }
    }
    ref var checkErr = ref heap<error>(out var ᏑcheckErr);
    ref var lastVia = ref heap<slice<ж<Δhttp.Request>>>(out var ᏑlastVia);
    ref var lastReq = ref heap<ж<Δhttp.Request>>(out var ᏑlastReq);
    c.Value.CheckRedirect = (ж<Δhttp.Request> req, slice<ж<Δhttp.Request>> via) => {
        ᏑlastReq.ValueSlot = req;
        ᏑlastVia.ValueSlot = via;
        return ᏑcheckErr.ValueSlot;
    };
    (var res, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatalf("Get error: %v"u8, err);
    }
    (~res).Body.Close();
    @string finalURL = (~(~res).Request).URL.String();
    {
        @string e = nilˢ;
        @string g = fmt.Sprintf("%v"u8, err); if (e != g) {
            Ꮡt.Errorf("with custom client, expected error %q, got %q"u8, e, g);
        }
    }
    if (!strings.HasSuffix(finalURL, n15ˢ)) {
        Ꮡt.Errorf("expected final url to end in /?n=15; got url %q"u8, finalURL);
    }
    {
        nint e = 15;
        nint g = len(lastVia); if (e != g) {
            Ꮡt.Errorf("expected lastVia to have contained %d elements; got %d"u8, e, g);
        }
    }
    // Test that Request.Cancel is propagated between requests (Issue 14053)
    var (creq, _) = NewRequest(headˢ, (~ts).URL, default!);
    var cancel = new channel<EmptyStruct>(0);
    creq.Value.Cancel = cancel.WithDirection(GoChanDir.Recv);
    {
        var (_, errΔ1) = c.Do(creq); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    if (lastReq == nil) {
        Ꮡt.Fatal(didnTSeeRedirectˢ);
    }
    if ((~lastReq).Cancel != cancel) {
        Ꮡt.Errorf("expected lastReq to have the cancel channel set on the initial req"u8);
    }
    checkErr = errors.New(noRedirectsAllowedˢ);
    (res, err) = c.Get((~ts).URL);
    {
        var (urlError, ok) = err._<ж<urlꓸError>>(ᐧ); if (!ok || !AreEqual((~urlError).Err, checkErr)) {
            Ꮡt.Errorf("with redirects forbidden, expected a *url.Error with our 'no redirects allowed' error inside; got %#v (%q)"u8, err, err);
        }
    }
    if (res == nil) {
        Ꮡt.Fatalf("Expected a non-nil Response on CheckRedirect failure (https://golang.org/issue/3795)"u8);
    }
    (~res).Body.Close();
    if ((~res).Header.Get(locationˢ) == ""u8) {
        Ꮡt.Errorf("no Location header in Response"u8);
    }
}

// Tests that Client redirects' contexts are derived from the original request's context.
public static void TestClientRedirectsContext(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirectsContext(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string redirectedRequestSˢ = "redirected request's context never expired after root request canceled"u8;

internal static void testClientRedirectsContext(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Redirect(w, r, "/"u8, StatusTemporaryRedirect);
    }))).Value.ts;
    var (ctx, cancel) = context_package.WithCancel(context_package.Background());
    var c = ts.Client();
    var cancelʗ1 = cancel;
    c.Value.CheckRedirect = error (ж<Δhttp.Request> reqΔ1, slice<ж<Δhttp.Request>> via) => {
        cancelʗ1();
        var selᴛ2 = reqΔ1.Context().Done();
        var selᴛ3 = time.After((time.Duration)(5000000000L));
        switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            return default!;
        }
        case 1 when selᴛ3.ꟷᐳ(out _): {
            return errors.New(redirectedRequestSˢ);
        }}
        return default!;
    };
    var (req, _) = NewRequestWithContext(ctx, getˢ2, (~ts).URL, default!);
    var (_, err) = c.Do(req);
    var (ue, ok) = err._<ж<urlꓸError>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("got error %T; want *url.Error"u8, err);
    }
    if (!AreEqual((~ue).Err, context_package.Canceled)) {
        Ꮡt.Errorf("url.Error.Err = %v; want %v"u8, (~ue).Err, context_package.Canceled);
    }
}

[GoType] partial struct redirectTest {
    internal @string suffix;
    internal nint want; // response code
    internal @string redirectBody;
}

public static void TestPostRedirects(ж<testing.T> Ꮡt) {
    var postRedirectTests = new redirectTest[]{
        new("/"u8, 200, "first"u8),
        new("/?code=301&next=302"u8, 200, "c301"u8),
        new("/?code=302&next=302"u8, 200, "c302"u8),
        new("/?code=303&next=301"u8, 200, "c303wc301"u8), // Issue 9348

        new("/?code=304"u8, 304, "c304"u8),
        new("/?code=305"u8, 305, "c305"u8),
        new("/?code=307&next=303,308,302"u8, 200, "c307"u8),
        new("/?code=308&next=302,301"u8, 200, "c308"u8),
        new("/?code=404"u8, 404, "c404"u8)
    }.slice();
    var wantSegments = new @string[]{
        @"POST / ""first"""u8,
        @"POST /?code=301&next=302 ""c301"""u8,
        @"GET /?code=302 """""u8,
        @"GET / """""u8,
        @"POST /?code=302&next=302 ""c302"""u8,
        @"GET /?code=302 """""u8,
        @"GET / """""u8,
        @"POST /?code=303&next=301 ""c303wc301"""u8,
        @"GET /?code=301 """""u8,
        @"GET / """""u8,
        @"POST /?code=304 ""c304"""u8,
        @"POST /?code=305 ""c305"""u8,
        @"POST /?code=307&next=303,308,302 ""c307"""u8,
        @"POST /?code=303&next=308,302 ""c307"""u8,
        @"GET /?code=308&next=302 """""u8,
        @"GET /?code=302 """""u8,
        @"GET / """""u8,
        @"POST /?code=308&next=302,301 ""c308"""u8,
        @"POST /?code=302&next=301 ""c308"""u8,
        @"GET /?code=301 """""u8,
        @"GET / """""u8,
        @"POST /?code=404 ""c404"""u8
    }.slice();
    @string want = strings.Join(wantSegments, "\n"u8);
    var postRedirectTestsʗ1 = postRedirectTests;
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testRedirectsByMethod(tΔ1, mode, postˢ, postRedirectTestsʗ1, want);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string deleteˢ = "DELETE"u8;

public static void TestDeleteRedirects(ж<testing.T> Ꮡt) {
    var deleteRedirectTests = new redirectTest[]{
        new("/"u8, 200, "first"u8),
        new("/?code=301&next=302,308"u8, 200, "c301"u8),
        new("/?code=302&next=302"u8, 200, "c302"u8),
        new("/?code=303"u8, 200, "c303"u8),
        new("/?code=307&next=301,308,303,302,304"u8, 304, "c307"u8),
        new("/?code=308&next=307"u8, 200, "c308"u8),
        new("/?code=404"u8, 404, "c404"u8)
    }.slice();
    var wantSegments = new @string[]{
        @"DELETE / ""first"""u8,
        @"DELETE /?code=301&next=302,308 ""c301"""u8,
        @"GET /?code=302&next=308 """""u8,
        @"GET /?code=308 """""u8,
        @"GET / """""u8,
        @"DELETE /?code=302&next=302 ""c302"""u8,
        @"GET /?code=302 """""u8,
        @"GET / """""u8,
        @"DELETE /?code=303 ""c303"""u8,
        @"GET / """""u8,
        @"DELETE /?code=307&next=301,308,303,302,304 ""c307"""u8,
        @"DELETE /?code=301&next=308,303,302,304 ""c307"""u8,
        @"GET /?code=308&next=303,302,304 """""u8,
        @"GET /?code=303&next=302,304 """""u8,
        @"GET /?code=302&next=304 """""u8,
        @"GET /?code=304 """""u8,
        @"DELETE /?code=308&next=307 ""c308"""u8,
        @"DELETE /?code=307 ""c308"""u8,
        @"DELETE / ""c308"""u8,
        @"DELETE /?code=404 ""c404"""u8
    }.slice();
    @string want = strings.Join(wantSegments, "\n"u8);
    var deleteRedirectTestsʗ1 = deleteRedirectTests;
    run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testRedirectsByMethod(tΔ1, mode, deleteˢ, deleteRedirectTestsʗ1, want);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentLengthˢ = "Content-Length"u8;
internal static readonly @string codeˢ = "code"u8;
internal static readonly @string nextˢ = "next"u8;

[GoType("dyn")] internal partial struct testRedirectsByMethod_log {
    public partial ref global::go.sync_package.Mutex Mutex { get; }
    public partial ref bytes_package.Buffer Buffer { get; }
}

internal static void testRedirectsByMethod(ж<testing.T> Ꮡt, testMode mode, @string method, slice<redirectTest> table, @string want) {
    ref var log = ref heap(new testRedirectsByMethod_log(), out var Ꮡlog);
    ref var ts = ref heap<ж<httptest.Server>>(out var Ꮡts);
    ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Lock();
        var (slurp, _) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader((~r).Body));
        fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer)), "%s %s %q"u8, (~r).Method, (~r).RequestURI, slurp);
        {
            @string cl = (~r).Header.Get(contentLengthˢ); if ((~r).Method == "GET"u8 && len(slurp) == 0 && ((~r).ContentLength != 0 || cl != ""u8)) {
                fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer)), " (but with body=%T, content-length = %v, %q)"u8, (~r).Body, (~r).ContentLength, cl);
            }
        }
        Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer).WriteByte((rune)'\n');
        Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Unlock();
        var urlQuery = (~r).URL.Query();
        {
            @string v = urlQuery.Get(codeˢ); if (v != ""u8) {
                @string location = Ꮡts.ValueSlot.Value.URL;
                {
                    @string final = urlQuery.Get(nextˢ); if (final != ""u8) {
                        var (first, rest, _) = strings.Cut(final, ","u8);
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
    }))).Value.ts;
    var c = ts.Client();
    foreach (var (_, tt) in table) {
        @string content = tt.redirectBody;
        var (req, _) = NewRequest(method, (~ts).URL + tt.suffix, new http_test_package.strings_ReaderжReader(strings.NewReader(content)));
        req.Value.GetBody = (io.ReadCloser, error) () => (io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(content))), default!);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != tt.want) {
            Ꮡt.Errorf("POST %s: status code = %d; want %d"u8, tt.suffix, (~res).StatusCode, tt.want);
        }
    }
    Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Lock();
    @string got = Ꮡlog.of(testRedirectsByMethod_log.ᏑBuffer).String();
    Ꮡlog.of(testRedirectsByMethod_log.ᏑMutex).Unlock();
    got = strings.TrimSpace(got);
    want = strings.TrimSpace(want);
    if (got != want) {
        var (gotΔ1, wantΔ1, lines) = removeCommonLines(got, want);
        Ꮡt.Errorf("Log differs after %d common lines.\n\nGot:\n%s\n\nWant:\n%s\n"u8, lines, gotΔ1, wantΔ1);
    }
}

internal static (@string asuffix, @string bsuffix, nint commonLines) removeCommonLines(@string a, @string b) {
    nint commonLines = default!;

    while (ᐧ) {
        nint nl = strings.IndexByte(a, (rune)'\n');
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

public static void TestClientRedirectUseResponse(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirectUseResponse(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string otherˢ = "/other"u8;
internal static readonly @string wrongBodyˢ = "wrong body"u8;
internal static readonly object expectedNonNilRequestˢ = (@string)"expected non-nil Request.Response"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bodyᶜ = "Hello, world."u8;

internal static void testClientRedirectUseResponse(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string body = bodyᶜ;
        ref var ts = ref heap<ж<httptest.Server>>(out var Ꮡts);
        ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if (strings.Contains((~(~r).URL).Path, otherˢ)){
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), wrongBodyˢ);
            } else {
                w.Header().Set(locationˢ, (~Ꮡts.ValueSlot).URL + "/other"u8);
                w.WriteHeader(StatusFound);
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), body);
            }
        }))).Value.ts;
        var c = ts.Client();
        c.Value.CheckRedirect = (ж<Δhttp.Request> req, slice<ж<Δhttp.Request>> via) => {
            if ((~req).Response == nil) {
                Ꮡt.Error(expectedNonNilRequestˢ);
            }
            return ErrUseLastResponse;
        };
        var (res, err) = c.Get((~ts).URL);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).StatusCode != StatusFound) {
            Ꮡt.Errorf("status = %d; want %d"u8, (~res).StatusCode, (nint)(StatusFound));
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        (var slurp, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)slurp) != body) {
            Ꮡt.Errorf("body = %q; want %q"u8, slurp, body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issues 17773 and 49281: don't follow a 3xx if the response doesn't
// have a Location header.
public static void TestClientRedirectNoLocation(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirectNoLocation(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ2 = "Foo"u8;
internal static readonly @string barˢ2 = "Bar"u8;

internal static void testClientRedirectNoLocation(ж<testing.T> Ꮡt, testMode mode) {
    foreach (var (_, code) in new nint[]{301, 308}.slice()) {
        Ꮡt.Run(fmt.Sprint(code), (ж<testing.T> tΔ1) => {
            setParallel(tΔ1);
            var cst = newClientServerTest(new http_test_package.testing_TжTB(tΔ1), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
                w.Header().Set(fooˢ2, barˢ2);
                w.WriteHeader(code);
            })));
            var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (~res).Body.Close();
            if ((~res).StatusCode != code) {
                tΔ1.Errorf("status = %d; want %d"u8, (~res).StatusCode, code);
            }
            {
                @string got = (~res).Header.Get(fooˢ2); if (got != "Bar"u8) {
                    tΔ1.Errorf("Foo header = %q; want Bar"u8, got);
                }
            }
        });
    }
}

// Don't follow a 307/308 if we can't resent the request body.
public static void TestClientRedirect308NoGetBody(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirect308NoGetBody(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someBodyˢ = "some body"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fakeURLᶜ = "https://localhost:1234/"u8;

internal static void testClientRedirect308NoGetBody(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string fakeURL = fakeURLᶜ;        // won't be hit
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(locationˢ, fakeURL);
        w.WriteHeader(308);
    }))).Value.ts;
    var (req, err) = NewRequest(postˢ, (~ts).URL, new http_test_package.strings_ReaderжReader(strings.NewReader(someBodyˢ)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var c = ts.Client();
    req.Value.GetBody = default!; // so it can't rewind.
    (var res, err) = c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    if ((~res).StatusCode != 308) {
        Ꮡt.Errorf("status = %d; want %d"u8, (~res).StatusCode, (nint)(308));
    }
    {
        @string got = (~res).Header.Get(locationˢ); if (got != fakeURL) {
            Ꮡt.Errorf("Location header = %q; want %q"u8, got, fakeURL);
        }
    }
}

internal static slice<ж<httpꓸCookie>> expectedCookies = new ж<httpꓸCookie>[]{
    Ꮡ(new httpꓸCookie(Name: "ChocolateChip"u8, Value: "tasty"u8)),
    Ꮡ(new httpꓸCookie(Name: "First"u8, Value: "Hit"u8)),
    Ꮡ(new httpꓸCookie(Name: "Second"u8, Value: "Hit"u8))
}.slice();

internal static Δhttp.HandlerFunc echoCookiesRedirectHandler = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    foreach (var (_, cookie) in r.Cookies()) {
        SetCookie(w, cookie);
    }
    if ((~(~r).URL).Path == "/"u8){
        SetCookie(w, expectedCookies[1]);
        Redirect(w, r, "/second"u8, StatusMovedPermanently);
    } else {
        SetCookie(w, expectedCookies[2]);
        w.Write(slice<byte>("hello"u8));
    }
});

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textPlainˢ = "text/plain"u8;
internal static readonly @string bodyˢ = "body"u8;

public static void TestClientSendsCookieFromJar(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new recordingTransport(nil));
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        client.Value.Jar = new http_test_package.TestJarжCookieJar(Ꮡ(new TestJar(perURL: new map<@string, slice<ж<httpꓸCookie>>>())));
        @string us = httpDummyFaketldˢ;
        var (u, _) = url.Parse(us);
        (~client).Jar.SetCookies(u, expectedCookies);
        client.Get(us); // Note: doesn't hit network
        matchReturnedCookies(Ꮡt, expectedCookies, (~tr).req.Cookies());
        client.Head(us); // Note: doesn't hit network
        matchReturnedCookies(Ꮡt, expectedCookies, (~tr).req.Cookies());
        client.Post(us, textPlainˢ, new http_test_package.strings_ReaderжReader(strings.NewReader(bodyˢ))); // Note: doesn't hit network
        matchReturnedCookies(Ꮡt, expectedCookies, (~tr).req.Cookies());
        client.PostForm(us, new url.Values(new map<@string, slice<@string>>{})); // Note: doesn't hit network
        matchReturnedCookies(Ꮡt, expectedCookies, (~tr).req.Cookies());
        var (req, _) = NewRequest(getˢ2, us, default!);
        client.Do(req); // Note: doesn't hit network
        matchReturnedCookies(Ꮡt, expectedCookies, (~tr).req.Cookies());
        (req, _) = NewRequest(postˢ, us, default!);
        client.Do(req); // Note: doesn't hit network
        matchReturnedCookies(Ꮡt, expectedCookies, (~tr).req.Cookies());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Just enough correctness for our redirect tests. Uses the URL.Host as the
// scope of all cookies.
[GoType] partial struct TestJar {
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
        if (j.perURL == default!) {
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡj.DerefOrNull().m.Unlock(); ᒐ.Run(); }
}

public static void TestRedirectCookiesJar(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testRedirectCookiesJar(Δp0, Δp1));
}

internal static void testRedirectCookiesJar(ж<testing.T> Ꮡt, testMode mode) {
    ж<httptest.Server> ts = default!;
    ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(echoCookiesRedirectHandler)).Value.ts;
    var c = ts.Client();
    c.Value.Jar = new http_test_package.TestJarжCookieJar(@new<TestJar>());
    var (u, _) = url.Parse((~ts).URL);
    (~c).Jar.SetCookies(u, new ж<httpꓸCookie>[]{expectedCookies[0]}.slice());
    var (resp, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatalf("Get: %v"u8, err);
    }
    (~resp).Body.Close();
    matchReturnedCookies(Ꮡt, expectedCookies, resp.Cookies());
}

internal static void matchReturnedCookies(ж<testing.T> Ꮡt, slice<ж<httpꓸCookie>> expected, slice<ж<httpꓸCookie>> given) {
    if (len(given) != len(expected)) {
        Ꮡt.Logf("Received cookies: %v"u8, given);
        Ꮡt.Errorf("Expected %d cookies, got %d"u8, len(expected), len(given));
    }
    foreach (var (_, ec) in expected) {
        var foundC = false;
        foreach (var (_, c) in given) {
            if ((~ec).Name == (~c).Name && (~ec).Value == (~c).Value) {
                foundC = true;
                break;
            }
        }
        if (!foundC) {
            Ꮡt.Errorf("Missing cookie %v"u8, ec.OrTypedNil());
        }
    }
}

public static void TestJarCalls(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testJarCalls(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpSecondhostFakeˢ = "http://secondhost.fake/secondpath"u8;
internal static readonly @string httpFirsthostFakeˢ = "http://firsthost.fake/"u8;
internal static readonly @string httpFirsthostFakeˢ2 = "http://firsthost.fake/nosetcookie"u8;
internal static readonly @string cookiesHttpFirsthostFakeˢ = """
Cookies("http://firsthost.fake/")
SetCookie("http://firsthost.fake/", [name=val])
Cookies("http://secondhost.fake/secondpath")
SetCookie("http://secondhost.fake/secondpath", [namesecondpath=valsecondpath])
Cookies("http://firsthost.fake/nosetcookie")

"""u8;

internal static void testJarCalls(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        @string pathSuffix = (~r).RequestURI[1..];
        if ((~r).RequestURI == "/nosetcookie"u8) {
            return; // don't set cookies for this path
        }
        SetCookie(w, Ꮡ(new httpꓸCookie(Name: "name"u8 + pathSuffix, Value: "val"u8 + pathSuffix)));
        if ((~r).RequestURI == "/"u8) {
            Redirect(w, r, httpSecondhostFakeˢ, 302);
        }
    }))).Value.ts;
    var jar = @new<RecordingJar>();
    var c = ts.Client();
    c.Value.Jar = new http_test_package.RecordingJarжCookieJar(jar);
    var tsʗ1 = ts;
    (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = (@string _Δp0, @string _Δp1) => net.Dial(tcpˢ, (~tsʗ1).Listener.Addr().String());
    var (_, err) = c.Get(httpFirsthostFakeˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = c.Get(httpFirsthostFakeˢ2);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string got = jar.of(RecordingJar.Ꮡlog).String();
    @string want = cookiesHttpFirsthostFakeˢ;
    if (got != want) {
        Ꮡt.Errorf("Got Jar calls:\n%s\nWant:\n%s"u8, got, want);
    }
}

// RecordingJar keeps a log of calls made to it, without
// tracking any cookies.
[GoType] partial struct RecordingJar {
    internal sync.Mutex mu;
    internal bytes.Buffer log;
}

public static void SetCookies(this ж<RecordingJar> Ꮡj, ж<url.URL> Ꮡu, slice<ж<httpꓸCookie>> cookies) {
    Ꮡj.logf("SetCookie(%q, %v)\n"u8, Ꮡu.OrTypedNil(), cookies);
}

public static slice<ж<httpꓸCookie>> Cookies(this ж<RecordingJar> Ꮡj, ж<url.URL> Ꮡu) {
    Ꮡj.logf("Cookies(%q)\n"u8, Ꮡu.OrTypedNil());
    return default!;
}

internal static void logf(this ж<RecordingJar> Ꮡj, @string format, params ꓸꓸꓸany argsʗp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        var args = argsʗp.sslice();

        ref var j = ref Ꮡj.DerefOrNull();
        j.mu.Lock();
        ᒐd1 = true;
        fmt.Fprintf(new http_test_package.bytes_BufferжWriter(Ꮡj.of(RecordingJar.Ꮡlog)), format, args.ꓸꓸꓸ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡj.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

public static void TestStreamingGet(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testStreamingGet(Δp0, Δp1));
}

internal static void testStreamingGet(ж<testing.T> Ꮡt, testMode mode) {
    var say = new channel<@string>(0);
    var sayʗ1 = say;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w._<Flusher>().Flush();
        foreach (var str in sayʗ1) {
            w.Write(slice<byte>(str));
            w._<Flusher>().Flush();
        }
    })));
    var c = cst.Value.c;
    var (res, err) = c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    array<byte> buf = new(10);
    foreach (var (_, str) in new @string[]{"i"u8, "am"u8, "also"u8, "known"u8, "as"u8, "comet"u8}.slice()) {
        say.ᐸꟷ(str);
        var (n, errΔ1) = io.ReadFull((~res).Body, buf.slice(0, len(str)));
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("ReadFull on %q: %v"u8, str, errΔ1);
        }
        if (n != len(str)) {
            Ꮡt.Fatalf("Receiving %q, only read %d bytes"u8, str, n);
        }
        @string got = ((@string)(buf.slice(0, n)));
        if (got != str) {
            Ꮡt.Fatalf("Expected %q, got %q"u8, str, got);
        }
    }
    builtin.close(say);
    (_, err) = io.ReadFull((~res).Body, buf[0..1]);
    if (!AreEqual(err, io.EOF)) {
        Ꮡt.Fatalf("at end expected EOF, got %v"u8, err);
    }
}

[GoType] partial struct writeCountingConn {
    [GoEmbedded] public net_package.Conn Conn;
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

[GoRecv] internal static (nint, error) Write(this ref writeCountingConn c, slice<byte> p) {
    c.count.Value++;
    return c.Conn.Write(p);
}

// TestClientWrites verifies that client requests are buffered and we
// don't send a TCP packet per line of the http request + body.
public static void TestClientWrites(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientWrites(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

internal static void testClientWrites(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).Value.ts;
    ref var writes = ref heap<nint>(out var Ꮡwrites);
    writes = 0;
    var dialer = (@string netz, @string addr) => {
        var (cΔ1, errΔ1) = net.Dial(netz, addr);
        if (errΔ1 == default!) {
            cΔ1 = new http_test_package.writeCountingConnжConn(Ꮡ(new writeCountingConn(cΔ1, Ꮡwrites)));
        }
        return (cΔ1, errΔ1);
    };
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = dialer;
    var (_, err) = c.Get((~ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (writes != 1) {
        Ꮡt.Errorf("Get request did %d Write calls, want 1"u8, writes);
    }
    writes = 0;
    (_, err) = c.PostForm((~ts).URL, new url.Values(new map<@string, slice<@string>>{["foo"u8] = new @string[]{"bar"u8}.slice()}));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (writes != 1) {
        Ꮡt.Errorf("Post request did %d Write calls, want 1"u8, writes);
    }
}

public static void TestClientInsecureTransport(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientInsecureTransport(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsHandshakeErrorˢ = "TLS handshake error"u8;

internal static void testClientInsecureTransport(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello"u8));
    })));
    var ts = cst.Value.ts;
    var errLog = @new<strings.Builder>();
    ts.Value.Config.Value.ErrorLog = log.New(new http_test_package.strings_BuilderжWriter(errLog), ""u8, 0);
    // TODO(bradfitz): add tests for skipping hostname checks too?
    // would require a new cert for testing, and probably
    // redundant with these tests.
    var c = ts.Client();
    foreach (var (_, vᴛ1) in new bool[]{true, false}.slice()) {
        ref var insecure = ref heap(new bool(), out var Ꮡinsecure);
        insecure = vᴛ1;

        (~c).Transport._<ж<Δhttp.Transport>>().Value.TLSClientConfig = Ꮡ(new tls.Config(
            InsecureSkipVerify: insecure
        ));
        var (res, err) = c.Get((~ts).URL);
        if ((err == default!) != insecure) {
            Ꮡt.Errorf("insecure=%v: got unexpected err=%v"u8, insecure, err);
        }
        if (res != nil) {
            (~res).Body.Close();
        }
    }
    cst.close();
    if (!strings.Contains(errLog.String(), tlsHandshakeErrorˢ)) {
        Ꮡt.Errorf("expected an error log message containing 'TLS handshake error'; got %q"u8, errLog.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpLocalhost1234ˢ = "http://localhost:1234/"u8;
internal static readonly @string thisFieldIsIllegalAndˢ = "/this/field/is/illegal/and/should/error/"u8;
internal static readonly @string requestURIˢ = "RequestURI"u8;

public static void TestClientErrorWithRequestURI(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var (req, _) = NewRequest(getˢ2, httpLocalhost1234ˢ, default!);
        req.Value.RequestURI = thisFieldIsIllegalAndˢ;
        var (_, err) = DefaultClient.Do(req);
        if (err == default!) {
            Ꮡt.Fatalf("expected an error"u8);
        }
        if (!strings.Contains(err.Error(), requestURIˢ)) {
            Ꮡt.Errorf("wanted error mentioning RequestURI; got error: %v"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestClientWithCorrectTLSServerName(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientWithCorrectTLSServerName(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string serverNameᶜ = "example.com"u8;

internal static void testClientWithCorrectTLSServerName(ж<testing.T> Ꮡt, testMode mode) {
    @string serverName = serverNameᶜ;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if ((~(~r).TLS).ServerName != serverName) {
            Ꮡt.Errorf("expected client to set ServerName %q, got: %q"u8, serverName, (~(~r).TLS).ServerName);
        }
    }))).Value.ts;
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.TLSClientConfig.Value.ServerName = serverName;
    {
        var (_, err) = c.Get((~ts).URL); if (err != default!) {
            Ꮡt.Fatalf("expected successful TLS connection, got error: %v"u8, err);
        }
    }
}

public static void TestClientWithIncorrectTLSServerName(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientWithIncorrectTLSServerName(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badserverˢ = "badserver"u8;

internal static void testClientWithIncorrectTLSServerName(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    })));
    var ts = cst.Value.ts;
    var errLog = @new<strings.Builder>();
    ts.Value.Config.Value.ErrorLog = log.New(new http_test_package.strings_BuilderжWriter(errLog), ""u8, 0);
    var c = ts.Client();
    (~c).Transport._<ж<Δhttp.Transport>>().Value.TLSClientConfig.Value.ServerName = badserverˢ;
    var (_, err) = c.Get((~ts).URL);
    if (err == default!) {
        Ꮡt.Fatalf("expected an error"u8);
    }
    if (!strings.Contains(err.Error(), "127.0.0.1"u8) || !strings.Contains(err.Error(), badserverˢ)) {
        Ꮡt.Errorf("wanted error mentioning 127.0.0.1 and badserver; got error: %v"u8, err);
    }
    cst.close();
    if (!strings.Contains(errLog.String(), tlsHandshakeErrorˢ)) {
        Ꮡt.Errorf("expected an error log message containing 'TLS handshake error'; got %q"u8, errLog.OrTypedNil());
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
public static void TestTransportUsesTLSConfigServerName(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportUsesTLSConfigServerName(Δp0, Δp1), new testMode[]{https1Mode, http2Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleComˢ = "example.com"u8;
internal static readonly @string httpsSomeOtherHostTldˢ = "https://some-other-host.tld/"u8;

internal static void testTransportUsesTLSConfigServerName(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello"u8));
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    tr.Value.TLSClientConfig.Value.ServerName = exampleComˢ; // one of httptest's Server cert names
    var tsʗ1 = ts;
    tr.Value.Dial = (@string netw, @string addr) => net.Dial(netw, (~tsʗ1).Listener.Addr().String());
    var (res, err) = c.Get(httpsSomeOtherHostTldˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
}

public static void TestResponseSetsTLSConnectionState(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testResponseSetsTLSConnectionState(Δp0, Δp1), new testMode[]{https1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpsExampleComˢ = "https://example.com/"u8;
internal static readonly object responseDidnTSetTlsˢ = (@string)"Response didn't set TLS Connection State."u8;

internal static void testResponseSetsTLSConnectionState(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Write(slice<byte>("Hello"u8));
        }))).Value.ts;
        var c = ts.Client();
        var tr = (~c).Transport._<ж<Δhttp.Transport>>();
        tr.Value.TLSClientConfig.Value.CipherSuites = new uint16[]{tls.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256}.slice();
        tr.Value.TLSClientConfig.Value.MaxVersion = tls.VersionTLS12; // to get to pick the cipher suite
        var tsʗ1 = ts;
        tr.Value.Dial = (@string netw, @string addr) => net.Dial(netw, (~tsʗ1).Listener.Addr().String());
        var (res, err) = c.Get(httpsExampleComˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).TLS == nil) {
            Ꮡt.Fatal(responseDidnTSetTlsˢ);
        }
        {
            var (got, want) = (res.Value.TLS.Value.CipherSuite, tls.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256); if (got != want) {
                Ꮡt.Errorf("TLS Cipher Suite = %d; want %d"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Check that an HTTPS client can interpret a particular TLS error
// to determine that the server is speaking HTTP.
// See golang.org/issue/11111.
public static void TestHTTPSClientDetectsHTTPServer(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testHTTPSClientDetectsHTTPServer(Δp0, Δp1), new testMode[]{http1Mode}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpˢ = "http"u8;
internal static readonly @string httpsˢ = "https"u8;
internal static readonly @string httpResponseToHttpsˢ = "HTTP response to HTTPS client"u8;

internal static void testHTTPSClientDetectsHTTPServer(ж<testing.T> Ꮡt, testMode mode) {
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }))).Value.ts;
    ts.Value.Config.Value.ErrorLog = quietLog;
    var (_, err) = Get(strings.Replace((~ts).URL, httpˢ, httpsˢ, 1));
    {
        @string got = err.Error(); if (!strings.Contains(got, httpResponseToHttpsˢ)) {
            Ꮡt.Fatalf("error = %q; want error indicating HTTP response to HTTPS request"u8, got);
        }
    }
}

// Verify Response.ContentLength is populated. https://golang.org/issue/4126
public static void TestClientHeadContentLength(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientHeadContentLength(Δp0, Δp1));
}

[GoType("dyn")] internal partial struct testClientHeadContentLength_tests {
    internal @string suffix;
    internal int64 want;
}

internal static void testClientHeadContentLength(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        {
            @string v = r.FormValue("cl"u8); if (v != ""u8) {
                w.Header().Set(contentLengthˢ, v);
            }
        }
    })));
    var tests = new testClientHeadContentLength_tests[]{
        new("/?cl=1234"u8, 1234),
        new("/?cl=0"u8, 0),
        new(""u8, -1)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (req, _) = NewRequest(headˢ, (~(~cst).ts).URL + tt.suffix, default!);
        var (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~res).ContentLength != tt.want) {
            Ꮡt.Errorf("Content-Length = %d; want %d"u8, (~res).ContentLength, tt.want);
        }
        (var bs, err) = io.ReadAll((~res).Body);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (len(bs) != 0) {
            Ꮡt.Errorf("Unexpected content: %q"u8, bs);
        }
    }
}

public static void TestEmptyPasswordAuth(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testEmptyPasswordAuth(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gopherˢ = "gopher"u8;
internal static readonly @string authorizationˢ = "Authorization"u8;
internal static readonly @string basicˢ = "Basic "u8;

internal static void testEmptyPasswordAuth(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string gopher = gopherˢ;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            @string auth = (~r).Header.Get(authorizationˢ);
            if (strings.HasPrefix(auth, basicˢ)){
                @string encoded = auth[6..];
                var (decoded, errΔ1) = base64.StdEncoding.DecodeString(encoded);
                if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
                @string expected = gopher + ":"u8;
                @string s = ((@string)decoded);
                if (expected != s) {
                    Ꮡt.Errorf("Invalid Authorization header. Got %q, wanted %q"u8, s, expected);
                }
            } else {
                Ꮡt.Errorf("Invalid auth %q"u8, auth);
            }
        }))).Value.ts;
        var tsʗ1 = ts;
        defer(tsʗ1.Close, ref ᒐ);
        var (req, err) = NewRequest(getˢ2, (~ts).URL, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        req.Value.URL.Value.User = url.User(gopher);
        var c = ts.Client();
        (var resp, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var respʗ1 = resp;
        defer(() => (~respʗ1).Body.Close(), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpMy20UserMy20Passˢ = "http://My%20User:My%20Pass@dummy.faketld/"u8;
internal static readonly @string myUserMyPassˢ = "My User:My Pass"u8;

public static void TestBasicAuth(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new recordingTransport(nil));
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        @string url = httpMy20UserMy20Passˢ;
        @string expected = myUserMyPassˢ;
        client.Get(url);
        if ((~(~tr).req).Method != "GET"u8) {
            Ꮡt.Errorf("got method %q, want %q"u8, (~(~tr).req).Method, getˢ2);
        }
        if ((~(~tr).req).URL.String() != url) {
            Ꮡt.Errorf("got URL %q, want %q"u8, (~(~tr).req).URL.String(), url);
        }
        if ((~(~tr).req).Header == default!) {
            Ꮡt.Fatalf("expected non-nil request Header"u8);
        }
        @string auth = (~(~tr).req).Header.Get(authorizationˢ);
        if (strings.HasPrefix(auth, basicˢ)){
            @string encoded = auth[6..];
            var (decoded, err) = base64.StdEncoding.DecodeString(encoded);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            @string s = ((@string)decoded);
            if (expected != s) {
                Ꮡt.Errorf("Invalid Authorization header. Got %q, wanted %q"u8, s, expected);
            }
        } else {
            Ꮡt.Errorf("Invalid auth %q"u8, auth);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpMy20UserDummyFaketldˢ = "http://My%20User@dummy.faketld/"u8;
internal static readonly @string myUserˢ = "My User"u8;
internal static readonly @string myPassˢ = "My Pass"u8;

public static void TestBasicAuthHeadersPreserved(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(afterTest, new http_test_package.testing_TжTB(Ꮡt), ref ᒐ);
        var tr = Ꮡ(new recordingTransport(nil));
        var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(tr)));
        // If Authorization header is provided, username in URL should not override it
        @string url = httpMy20UserDummyFaketldˢ;
        var (req, err) = NewRequest(getˢ2, url, default!);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        req.SetBasicAuth(myUserˢ, myPassˢ);
        @string expected = myUserMyPassˢ;
        client.Do(req);
        if ((~(~tr).req).Method != "GET"u8) {
            Ꮡt.Errorf("got method %q, want %q"u8, (~(~tr).req).Method, getˢ2);
        }
        if ((~(~tr).req).URL.String() != url) {
            Ꮡt.Errorf("got URL %q, want %q"u8, (~(~tr).req).URL.String(), url);
        }
        if ((~(~tr).req).Header == default!) {
            Ꮡt.Fatalf("expected non-nil request Header"u8);
        }
        @string auth = (~(~tr).req).Header.Get(authorizationˢ);
        if (strings.HasPrefix(auth, basicˢ)){
            @string encoded = auth[6..];
            var (decoded, errΔ1) = base64.StdEncoding.DecodeString(encoded);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            @string s = ((@string)decoded);
            if (expected != s) {
                Ꮡt.Errorf("Invalid Authorization header. Got %q, wanted %q"u8, s, expected);
            }
        } else {
            Ꮡt.Errorf("Invalid auth %q"u8, auth);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestStripPasswordFromError_testCases {
    internal @string desc;
    internal @string @in;
    internal @string @out;
}

public static void TestStripPasswordFromError(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var client = Ꮡ(new Client(Transport: new http_test_package.recordingTransportжRoundTripper(Ꮡ(new recordingTransport(nil)))));
    var testCases = new TestStripPasswordFromError_testCases[]{
        new(
            desc: "Strip password from error message"u8,
            @in: "http://user:password@dummy.faketld/"u8,
            @out: @"Get ""http://user:***@dummy.faketld/"": dummy impl"u8
        ),
        new(
            desc: "Don't Strip password from domain name"u8,
            @in: "http://user:password@password.faketld/"u8,
            @out: @"Get ""http://user:***@password.faketld/"": dummy impl"u8
        ),
        new(
            desc: "Don't Strip password from path"u8,
            @in: "http://user:password@dummy.faketld/password"u8,
            @out: @"Get ""http://user:***@dummy.faketld/password"": dummy impl"u8
        ),
        new(
            desc: "Strip escaped password"u8,
            @in: "http://user:pa%2Fssword@dummy.faketld/"u8,
            @out: @"Get ""http://user:***@dummy.faketld/"": dummy impl"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tC = ref heap(new TestStripPasswordFromError_testCases(), out var ᏑtC);
        tC = vᴛ1;

        var clientʗ1 = client;
        var tCʗ1 = tC;
        Ꮡt.Run(tC.desc, (ж<testing.T> tΔ1) => {
            var (_, err) = clientʗ1.Get(tCʗ1.@in);
            if (err.Error() != tCʗ1.@out) {
                tΔ1.Errorf("Unexpected output for %q: expected %q, actual %q"u8,
                    tCʗ1.@in, tCʗ1.@out, err.Error());
            }
        });
    }
}

public static void TestClientTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonceˢ = "nonce"u8;
internal static readonly @string clientTimeoutˢ = "Client.Timeout"u8;
internal static readonly @string armˢ = "arm"u8;
internal static readonly object handlerNeverGotSlowˢ = (@string)"handler never got /slow request, but client returned response"u8;
internal static readonly object expectedErrorFromReadAllˢ = (@string)"expected error from ReadAll"u8;
internal static readonly @string clientTimeoutˢ2 = "(Client.Timeout"u8;

internal static void testClientTimeout(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
    @string nonce = default!;                // a unique per-request string
    bool sawSlowNonce = default!; // true if the handler saw /slow?nonce=<nonce>
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        _ = r.ParseForm();
        if ((~(~r).URL).Path == "/"u8) {
            Redirect(w, r, "/slow?nonce="u8 + (~r).Form.Get(nonceˢ), StatusFound);
            return;
        }
        if ((~(~r).URL).Path == "/slow"u8) {
            Ꮡmu.Lock();
            if ((~r).Form.Get(nonceˢ) == nonce){
                sawSlowNonce = true;
            } else {
                Ꮡt.Logf("mismatched nonce: received %s, want %s"u8, (~r).Form.Get(nonceˢ), nonce);
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
    for (; ᐧ ; timeout *= 2) {
        if (timeout <= 0) {
            // The only way we can feasibly hit this while the test is running is if
            // the request fails without actually waiting for the timeout to occur.
            Ꮡt.Fatalf("timeout overflow"u8);
        }
        {
            var (deadline, okΔ1) = t.Deadline(); if (okΔ1 && !time.Now().Add(timeout).Before(deadline)) {
                Ꮡt.Fatalf("failed to produce expected timeout before test deadline"u8);
            }
        }
        Ꮡt.Logf("attempting test with timeout %v"u8, timeout);
        cst.Value.c.Value.Timeout = timeout;
        Ꮡmu.Lock();
        nonce = fmt.Sprint(nextNonce);
        nextNonce++;
        sawSlowNonce = false;
        Ꮡmu.Unlock();
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL + "/?nonce="u8 + nonce);
        if (err != default!) {
            if (strings.Contains(err.Error(), clientTimeoutˢ)) {
                // Timed out before handler could respond.
                Ꮡt.Logf("timeout before response received"u8);
                continue;
            }
            if (runtime.GOOS == "windows"u8 && strings.HasPrefix(runtime.GOARCH, armˢ)) {
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
        (_, err) = io.ReadAll((~res).Body);
        (~res).Body.Close();
        if (err == default!) {
            Ꮡt.Fatal(expectedErrorFromReadAllˢ);
        }
        (var ne, ok) = err._<netꓸError>(ᐧ);
        if (!ok){
            Ꮡt.Errorf("error value from ReadAll was %T; expected some net.Error"u8, err);
        } else 
        if (!ne.Timeout()) {
            Ꮡt.Errorf("net.Error.Timeout = false; want true"u8);
        }
        if (!errors.Is(err, context_package.DeadlineExceeded)) {
            Ꮡt.Errorf("ReadAll error = %q; expected some context.DeadlineExceeded"u8, err);
        }
        {
            @string got = ne.Error(); if (!strings.Contains(got, clientTimeoutˢ2)) {
                if (runtime.GOOS == "windows"u8 && strings.HasPrefix(runtime.GOARCH, armˢ)) {
                    testenv.SkipFlaky(new http_test_package.testing_TжTB(Ꮡt), 43120);
                }
                Ꮡt.Errorf("error string = %q; missing timeout substring"u8, got);
            }
        }
        break;
    }
}

// Client.Timeout firing before getting to the body
public static void TestClientTimeout_Headers(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientTimeout_Headers(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotResponseFromGetˢ = (@string)"got response from Get; expected error"u8;
internal static readonly object netErrorTimeoutFalseWantˢ = (@string)"net.Error.Timeout = false; want true"u8;
internal static readonly @string clientTimeoutExceededˢ = "Client.Timeout exceeded"u8;

internal static void testClientTimeout_Headers(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var donec = new channel<bool>(1);
        var donecʗ1 = donec;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
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
        cst.Value.c.Value.Timeout = 5 * time.Millisecond;
        var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
        if (err == default!) {
            (~res).Body.Close();
            Ꮡt.Fatal(gotResponseFromGetˢ);
        }
        {
            var (_, okΔ1) = err._<ж<urlꓸError>>(ᐧ); if (!okΔ1) {
                Ꮡt.Fatalf("Got error of type %T; want *url.Error"u8, err);
            }
        }
        var (ne, ok) = err._<netꓸError>(ᐧ);
        if (!ok) {
            Ꮡt.Fatalf("Got error of type %T; want some net.Error"u8, err);
        }
        if (!ne.Timeout()) {
            Ꮡt.Error(netErrorTimeoutFalseWantˢ);
        }
        if (!errors.Is(err, context_package.DeadlineExceeded)) {
            Ꮡt.Errorf("ReadAll error = %q; expected some context.DeadlineExceeded"u8, err);
        }
        {
            @string got = ne.Error(); if (!strings.Contains(got, clientTimeoutExceededˢ)) {
                if (runtime.GOOS == "windows"u8 && strings.HasPrefix(runtime.GOARCH, armˢ)) {
                    testenv.SkipFlaky(new http_test_package.testing_TжTB(Ꮡt), 43120);
                }
                Ꮡt.Errorf("error string = %q; missing timeout substring"u8, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 16094: if Client.Timeout is set but not hit, a Timeout error shouldn't be
// returned.
public static void TestClientTimeoutCancel(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientTimeoutCancel(Δp0, Δp1));
}

internal static void testClientTimeoutCancel(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var testDone = new channel<EmptyStruct>(0);
        var (ctx, cancel) = context_package.WithCancel(context_package.Background());
        var testDoneʗ1 = testDone;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w._<Flusher>().Flush();
            ᐸꟷ(testDoneʗ1);
        })));
        defer(ᴛ1 => builtin.close(ᴛ1), testDone, ref ᒐ);
        cst.Value.c.Value.Timeout = (time.Duration)(3600000000000L);
        var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
        req.Value.Cancel = ctx.Done();
        var (res, err) = (~cst).c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        cancel();
        (_, err) = io.Copy(io.Discard, (~res).Body);
        if (!AreEqual(err, http_internal_test_package.ExportErrRequestCanceled)) {
            Ꮡt.Fatalf("error = %v; want errRequestCanceled"u8, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 49366: if Client.Timeout is set but not hit, no error should be returned.
public static void TestClientTimeoutDoesNotExpire(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientTimeoutDoesNotExpire(Δp0, Δp1));
}

internal static void testClientTimeoutDoesNotExpire(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("body"u8));
    })));
    cst.Value.c.Value.Timeout = (time.Duration)(3600000000000L);
    var (req, _) = NewRequest(getˢ2, (~(~cst).ts).URL, default!);
    var (res, err) = (~cst).c.Do(req);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        (_, err) = io.Copy(io.Discard, (~res).Body); if (err != default!) {
            Ꮡt.Fatalf("io.Copy(io.Discard, res.Body) = %v, want nil"u8, err);
        }
    }
    {
        err = (~res).Body.Close(); if (err != default!) {
            Ꮡt.Fatalf("res.Body.Close() = %v, want nil"u8, err);
        }
    }
}

public static void TestClientRedirectEatsBody_h1(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirectEatsBody(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ3 = "/foo"u8;
internal static readonly object serverDidnTSeeARequestˢ = (@string)"server didn't see a request"u8;
internal static readonly object serverDidnTSeeASecondˢ = (@string)"server didn't see a second request"u8;
internal static readonly object serverSawDifferentClientˢ = (@string)"server saw different client ports before & after the redirect"u8;

internal static void testClientRedirectEatsBody(ж<testing.T> Ꮡt, testMode mode) {
    var saw = new channel<@string>(2);
    var sawʗ1 = saw;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        sawʗ1.ᐸꟷ((~r).RemoteAddr);
        if ((~(~r).URL).Path == "/"u8) {
            Redirect(w, r, fooˢ3, StatusFound); // which includes a body
        }
    })));
    var (res, err) = (~cst).c.Get((~(~cst).ts).URL);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = io.ReadAll((~res).Body);
    (~res).Body.Close();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string first = default!;
    var selᴛ4 = saw;
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out first): {
        break;
    }
    default: {
        Ꮡt.Fatal(serverDidnTSeeARequestˢ);
        break;
    }}
    @string second = default!;
    var selᴛ5 = saw;
    switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
    case 0 when selᴛ5.ꟷᐳ(out second): {
        break;
    }
    default: {
        Ꮡt.Fatal(serverDidnTSeeASecondˢ);
        break;
    }}
    if (first != second) {
        Ꮡt.Fatal(serverSawDifferentClientˢ);
    }
}

internal delegate void eofReaderFunc();

internal static (nint n, error err) Read(this eofReaderFunc f, slice<byte> p) {
    f();
    return (0, io.EOF);
}

[GoType("dyn")] internal partial struct TestReferer_tests {
    internal @string lastReq, newReq, explicitRef; // from -> to URLs, explicitly set Referer value
    internal @string want;
}

public static void TestReferer(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestReferer_tests[]{ // don't send user:

        new(lastReq: "http://gopher@test.com"u8, newReq: "http://link.com"u8, want: "http://test.com"u8),
        new(lastReq: "https://gopher@test.com"u8, newReq: "https://link.com"u8, want: "https://test.com"u8), // don't send a user and password:

        new(lastReq: "http://gopher:go@test.com"u8, newReq: "http://link.com"u8, want: "http://test.com"u8),
        new(lastReq: "https://gopher:go@test.com"u8, newReq: "https://link.com"u8, want: "https://test.com"u8), // nothing to do:

        new(lastReq: "http://test.com"u8, newReq: "http://link.com"u8, want: "http://test.com"u8),
        new(lastReq: "https://test.com"u8, newReq: "https://link.com"u8, want: "https://test.com"u8), // https to http doesn't send a referer:

        new(lastReq: "https://test.com"u8, newReq: "http://link.com"u8, want: ""u8),
        new(lastReq: "https://gopher:go@test.com"u8, newReq: "http://link.com"u8, want: ""u8), // https to http should remove an existing referer:

        new(lastReq: "https://test.com"u8, newReq: "http://link.com"u8, explicitRef: "https://foo.com"u8, want: ""u8),
        new(lastReq: "https://gopher:go@test.com"u8, newReq: "http://link.com"u8, explicitRef: "https://foo.com"u8, want: ""u8), // don't override an existing referer:

        new(lastReq: "https://test.com"u8, newReq: "https://link.com"u8, explicitRef: "https://foo.com"u8, want: "https://foo.com"u8),
        new(lastReq: "https://gopher:go@test.com"u8, newReq: "https://link.com"u8, explicitRef: "https://foo.com"u8, want: "https://foo.com"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (l, err) = url.Parse(tt.lastReq);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var n, err) = url.Parse(tt.newReq);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string r = http_internal_test_package.ExportRefererForURL(l, n, tt.explicitRef);
        if (r != tt.want) {
            Ꮡt.Errorf("refererForURL(%q, %q) = %q; want %q"u8, tt.lastReq, tt.newReq, r, tt.want);
        }
    }
}

// issue15577Tripper returns a Response with a redirect response
// header and doesn't populate its Response.Request field.
[GoType] partial struct issue15577Tripper {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this issue15577Tripper _Δp0, ж<Δhttp.Request> _Δp1) {
    var resp = Ꮡ(new Response(
        StatusCode: 303,
        Header: new map<@string, slice<@string>>{["Location"u8] = new @string[]{"http://www.example.com/"u8}.slice()},
        Body: io.NopCloser(new http_test_package.strings_ReaderжReader(strings.NewReader(""u8)))
    ));
    return (resp, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpDummyTldˢ = "http://dummy.tld"u8;

// Issue 15577: don't assume the roundtripper's response populates its Request field.
public static void TestClientRedirectResponseWithoutRequest(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var c = Ꮡ(new Client(
        CheckRedirect: (ж<Δhttp.Request> _Δp0, slice<ж<Δhttp.Request>> _Δp1) => fmt.Errorf("no redirects!"u8),
        Transport: new issue15577Tripper(nil)
    ));
    // Check that this doesn't crash:
    c.Get(httpDummyTldˢ);
}

// Issue 4800: copy (some) headers when Client follows a redirect.
// Issue 35104: Since both URLs have the same host (localhost)
// but different ports, sensitive headers like Cookie and Authorization
// are preserved.
public static void TestClientCopyHeadersOnRedirect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientCopyHeadersOnRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resultˢ = "Result"u8;
internal static readonly @string gotErrorsˢ = "got errors"u8;
internal static readonly @string userAgentˢ2 = "User-Agent"u8;
internal static readonly @string xFooˢ = "X-Foo"u8;
internal static readonly @string cookieˢ = "Cookie"u8;
internal static readonly @string fooBarˢ = "foo=bar"u8;
internal static readonly @string secretpasswordˢ = "secretpassword"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string uaᶜ = "some-agent/1.2"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string xfooᶜ = "foo-val"u8;

internal static void testClientCopyHeadersOnRedirect(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string ua = uaᶜ;
        @string xfoo = xfooᶜ;
        @string ts2URL = default!;
        var ts1 = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var want = new httpꓸHeader(new map<@string, slice<@string>>{
                ["User-Agent"u8] = new @string[]{ua}.slice(),
                ["X-Foo"u8] = new @string[]{xfoo}.slice(),
                ["Referer"u8] = new @string[]{ts2URL}.slice(),
                ["Accept-Encoding"u8] = new @string[]{"gzip"u8}.slice(),
                ["Cookie"u8] = new @string[]{"foo=bar"u8}.slice(),
                ["Authorization"u8] = new @string[]{"secretpassword"u8}.slice()
            });
            if (!reflect.DeepEqual((~r).Header, want)) {
                Ꮡt.Errorf("Request.Header = %#v; want %#v"u8, (~r).Header, want);
            }
            if (Ꮡt.Failed()){
                w.Header().Set(resultˢ, gotErrorsˢ);
            } else {
                w.Header().Set(resultˢ, "ok"u8);
            }
        }))).Value.ts;
        var ts1ʗ1 = ts1;
        var ts2 = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            Redirect(w, r, (~ts1ʗ1).URL, StatusFound);
        }))).Value.ts;
        ts2URL = ts2.Value.URL;
        var c = ts1.Client();
        c.Value.CheckRedirect = error (ж<Δhttp.Request> r, slice<ж<Δhttp.Request>> via) => {
            var want = new httpꓸHeader(new map<@string, slice<@string>>{
                ["User-Agent"u8] = new @string[]{ua}.slice(),
                ["X-Foo"u8] = new @string[]{xfoo}.slice(),
                ["Referer"u8] = new @string[]{ts2URL}.slice(),
                ["Cookie"u8] = new @string[]{"foo=bar"u8}.slice(),
                ["Authorization"u8] = new @string[]{"secretpassword"u8}.slice()
            });
            if (!reflect.DeepEqual((~r).Header, want)) {
                Ꮡt.Errorf("CheckRedirect Request.Header = %#v; want %#v"u8, (~r).Header, want);
            }
            return default!;
        };
        var (req, _) = NewRequest(getˢ2, (~ts2).URL, default!);
        (~req).Header.Add(userAgentˢ2, ua);
        (~req).Header.Add(xFooˢ, xfoo);
        (~req).Header.Add(cookieˢ, fooBarˢ);
        (~req).Header.Add(authorizationˢ, secretpasswordˢ);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != 200) {
            Ꮡt.Fatal((~res).Status);
        }
        {
            @string got = (~res).Header.Get(resultˢ); if (got != "ok"u8) {
                Ꮡt.Errorf("result = %q; want ok"u8, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue #70530: Once we strip a header on a redirect to a different host,
// the header should stay stripped across any further redirects.
public static void TestClientStripHeadersOnRepeatedRedirect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientStripHeadersOnRepeatedRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string proxyAuthorizationˢ = "Proxy-Authorization"u8;
internal static readonly @string xDoneˢ = "X-Done"u8;
internal static readonly @string trueˢ = "true"u8;

internal static void testClientStripHeadersOnRepeatedRedirect(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string proto = default!;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if ((~r).Host + (~(~r).URL).Path != "a.example.com/"u8) {
                {
                    @string h = (~r).Header.Get(authorizationˢ); if (h != ""u8){
                        Ꮡt.Errorf("on request to %v%v, Authorization=%q, want no header"u8, (~r).Host, (~(~r).URL).Path, h);
                    } else 
                    {
                        @string hΔ1 = (~r).Header.Get(proxyAuthorizationˢ); if (hΔ1 != ""u8) {
                            Ꮡt.Errorf("on request to %v%v, Proxy-Authorization=%q, want no header"u8, (~r).Host, (~(~r).URL).Path, hΔ1);
                        }
                    }
                }
            }
            // Follow a chain of redirects from a to b and back to a.
            // The Authorization header is stripped on the first redirect to b,
            // and stays stripped even if we're sent back to a.
            var exprᴛ1 = (~r).Host + (~(~r).URL).Path;
            if (exprᴛ1 == "a.example.com/"u8) {
                Redirect(w, r, proto + "://b.example.com/"u8, StatusFound);
            }
            else if (exprᴛ1 == "b.example.com/"u8) {
                Redirect(w, r, proto + "://b.example.com/redirect"u8, StatusFound);
            }
            else if (exprᴛ1 == "b.example.com/redirect"u8) {
                Redirect(w, r, proto + "://a.example.com/redirect"u8, StatusFound);
            }
            else if (exprᴛ1 == "a.example.com/redirect"u8) {
                w.Header().Set(xDoneˢ, trueˢ);
            }
            else { /* default: */
                Ꮡt.Errorf("unexpected request to %v"u8, (~r).URL.OrTypedNil());
            }

        }))).Value.ts;
        (proto, _, _) = strings.Cut((~ts).URL, ":"u8);
        var c = ts.Client();
        var tsʗ1 = ts;
        (~c).Transport._<ж<Δhttp.Transport>>().Value.Dial = (@string _Δp0, @string _Δp1) => net.Dial(tcpˢ, (~tsʗ1).Listener.Addr().String());
        var (req, _) = NewRequest(getˢ2, proto + "://a.example.com/"u8, default!);
        (~req).Header.Add(cookieˢ, fooBarˢ);
        (~req).Header.Add(authorizationˢ, secretpasswordˢ);
        (~req).Header.Add(proxyAuthorizationˢ, secretpasswordˢ);
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).Header.Get(xDoneˢ) != "true"u8) {
            Ꮡt.Fatalf("response missing expected header: X-Done=true"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 22233: copy host when Client follows a relative redirect.
public static void TestClientCopyHostOnRedirect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientCopyHostOnRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string shouldNotSeeThisResponseˢ = "should not see this response"u8;
internal static readonly @string httpˢ2 = "http://"u8;
internal static readonly @string httpsˢ2 = "https://"u8;
internal static readonly @string hopˢ = "/hop"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantBodyᶜ = "response body"u8;

internal static void testClientCopyHostOnRedirect(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        // Virtual hostname: should not receive any request.
        var @virtual = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            Ꮡt.Errorf("Virtual host received request %v"u8, (~r).URL.OrTypedNil());
            w.WriteHeader(403);
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), shouldNotSeeThisResponseˢ);
        }))).Value.ts;
        var virtualʗ1 = @virtual;
        defer(virtualʗ1.Close, ref ᒐ);
        @string virtualHost = strings.TrimPrefix((~@virtual).URL, httpˢ2);
        virtualHost = strings.TrimPrefix(virtualHost, httpsˢ2);
        Ꮡt.Logf("Virtual host is %v"u8, virtualHost);
        // Actual hostname: should not receive any request.
        @string wantBody = wantBodyᶜ;
        @string tsURL = default!;
        @string tsHost = default!;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var exprᴛ1 = (~(~r).URL).Path;
            if (exprᴛ1 == "/"u8) {
                if ((~r).Host != virtualHost) {
                    // Relative redirect.
                    Ꮡt.Errorf("Serving /: Request.Host = %#v; want %#v"u8, (~r).Host, virtualHost);
                    w.WriteHeader(404);
                    return;
                }
                w.Header().Set(locationˢ, hopˢ);
                w.WriteHeader(302);
            }
            else if (exprᴛ1 == "/hop"u8) {
                if ((~r).Host != virtualHost) {
                    // Absolute redirect.
                    Ꮡt.Errorf("Serving /hop: Request.Host = %#v; want %#v"u8, (~r).Host, virtualHost);
                    w.WriteHeader(404);
                    return;
                }
                w.Header().Set(locationˢ, tsURL + "/final"u8);
                w.WriteHeader(302);
            }
            else if (exprᴛ1 == "/final"u8) {
                if ((~r).Host != tsHost) {
                    Ꮡt.Errorf("Serving /final: Request.Host = %#v; want %#v"u8, (~r).Host, tsHost);
                    w.WriteHeader(404);
                    return;
                }
                w.WriteHeader(200);
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), wantBody);
            }
            else { /* default: */
                Ꮡt.Errorf("Serving unexpected path %q"u8, (~(~r).URL).Path);
                w.WriteHeader(404);
            }

        }))).Value.ts;
        tsURL = ts.Value.URL;
        tsHost = strings.TrimPrefix((~ts).URL, httpˢ2);
        tsHost = strings.TrimPrefix(tsHost, httpsˢ2);
        Ꮡt.Logf("Server host is %v"u8, tsHost);
        var c = ts.Client();
        var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
        req.Value.Host = virtualHost;
        var (resp, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var respʗ1 = resp;
        defer(() => (~respʗ1).Body.Close(), ref ᒐ);
        if ((~resp).StatusCode != 200) {
            Ꮡt.Fatal((~resp).Status);
        }
        {
            var (got, errΔ1) = io.ReadAll((~resp).Body); if (errΔ1 != default! || ((sstring)got) != wantBody) {
                Ꮡt.Errorf("body = %q; want %q"u8, got, wantBody);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 17494: cookies should be altered when Client follows redirects.
public static void TestClientAltersCookiesOnRedirect(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientAltersCookiesOnRedirect(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cycleˢ = "Cycle"u8;

internal static void testClientAltersCookiesOnRedirect(ж<testing.T> Ꮡt, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        map<@string, slice<@string>> cookieMap(slice<ж<httpꓸCookie>> cs) {
            var m = new map<@string, slice<@string>>();
            foreach (var (_, cΔ1) in cs) {
                m[(~cΔ1).Name] = append(m[(~cΔ1).Name], (~cΔ1).Value);
            }
            return m;
        }
        var cookieMapʗ1 = cookieMap;
        var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            map<@string, slice<@string>> want = default!;
            var got = cookieMapʗ1(r.Cookies());
            var (cΔ2, _) = r.Cookie(cycleˢ);
            var exprᴛ1 = (~cΔ2).Value;
            if (exprᴛ1 == "0"u8) {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"u8] = new @string[]{"OldValue1a"u8, "OldValue1b"u8}.slice(),
                    ["Cookie2"u8] = new @string[]{"OldValue2"u8}.slice(),
                    ["Cookie3"u8] = new @string[]{"OldValue3a"u8, "OldValue3b"u8}.slice(),
                    ["Cookie4"u8] = new @string[]{"OldValue4"u8}.slice(),
                    ["Cycle"u8] = new @string[]{"0"u8}.slice()
                };
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cycle"u8, Value: "1"u8, Path: "/"u8)));
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie2"u8, Path: "/"u8, MaxAge: -1))); // Delete cookie from Header
                Redirect(w, r, "/"u8, StatusFound);
            }
            else if (exprᴛ1 == "1"u8) {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"u8] = new @string[]{"OldValue1a"u8, "OldValue1b"u8}.slice(),
                    ["Cookie3"u8] = new @string[]{"OldValue3a"u8, "OldValue3b"u8}.slice(),
                    ["Cookie4"u8] = new @string[]{"OldValue4"u8}.slice(),
                    ["Cycle"u8] = new @string[]{"1"u8}.slice()
                };
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cycle"u8, Value: "2"u8, Path: "/"u8)));
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie3"u8, Value: "NewValue3"u8, Path: "/"u8))); // Modify cookie in Header
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie4"u8, Value: "NewValue4"u8, Path: "/"u8))); // Modify cookie in Jar
                Redirect(w, r, "/"u8, StatusFound);
            }
            else if (exprᴛ1 == "2"u8) {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"u8] = new @string[]{"OldValue1a"u8, "OldValue1b"u8}.slice(),
                    ["Cookie3"u8] = new @string[]{"NewValue3"u8}.slice(),
                    ["Cookie4"u8] = new @string[]{"NewValue4"u8}.slice(),
                    ["Cycle"u8] = new @string[]{"2"u8}.slice()
                };
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cycle"u8, Value: "3"u8, Path: "/"u8)));
                SetCookie(w, Ꮡ(new httpꓸCookie(Name: "Cookie5"u8, Value: "NewValue5"u8, Path: "/"u8))); // Insert cookie into Jar
                Redirect(w, r, "/"u8, StatusFound);
            }
            else if (exprᴛ1 == "3"u8) {
                want = new map<@string, slice<@string>>{
                    ["Cookie1"u8] = new @string[]{"OldValue1a"u8, "OldValue1b"u8}.slice(),
                    ["Cookie3"u8] = new @string[]{"NewValue3"u8}.slice(),
                    ["Cookie4"u8] = new @string[]{"NewValue4"u8}.slice(),
                    ["Cookie5"u8] = new @string[]{"NewValue5"u8}.slice(),
                    ["Cycle"u8] = new @string[]{"3"u8}.slice()
                };
            }
            else { /* default: */
                Ꮡt.Errorf("unexpected redirect cycle"u8);
                return;
            }

            // Don't redirect to ensure the loop ends.
            if (!reflect.DeepEqual(got, want)) {
                Ꮡt.Errorf("redirect %s, Cookie = %v, want %v"u8, (~cΔ2).Value, got, want);
            }
        }))).Value.ts;
        var (jar, _) = cookiejar.New(nil);
        var c = ts.Client();
        c.Value.Jar = new http_test_package.cookiejar_JarжCookieJar(jar);
        var (u, _) = url.Parse((~ts).URL);
        var (req, _) = NewRequest(getˢ2, (~ts).URL, default!);
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie1"u8, Value: "OldValue1a"u8)));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie1"u8, Value: "OldValue1b"u8)));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie2"u8, Value: "OldValue2"u8)));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie3"u8, Value: "OldValue3a"u8)));
        req.AddCookie(Ꮡ(new httpꓸCookie(Name: "Cookie3"u8, Value: "OldValue3b"u8)));
        jar.SetCookies(u, new ж<httpꓸCookie>[]{Ꮡ(new httpꓸCookie(Name: "Cookie4"u8, Value: "OldValue4"u8, Path: "/"u8))}.slice());
        jar.SetCookies(u, new ж<httpꓸCookie>[]{Ꮡ(new httpꓸCookie(Name: "Cycle"u8, Value: "0"u8, Path: "/"u8))}.slice());
        var (res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var resʗ1 = res;
        defer(() => (~resʗ1).Body.Close(), ref ᒐ);
        if ((~res).StatusCode != 200) {
            Ꮡt.Fatal((~res).Status);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestShouldCopyHeaderOnRedirect_tests {
    internal @string initialURL;
    internal @string destURL;
    internal bool want;
}

// Part of Issue 4800
public static void TestShouldCopyHeaderOnRedirect(ж<testing.T> Ꮡt) {
    var tests = new TestShouldCopyHeaderOnRedirect_tests[]{ // Sensitive headers:

        new("http://foo.com/"u8, "http://bar.com/"u8, false),
        new("http://foo.com/"u8, "http://bar.com/"u8, false),
        new("http://foo.com/"u8, "http://bar.com/"u8, false),
        new("http://foo.com/"u8, "https://foo.com/"u8, true),
        new("http://foo.com:1234/"u8, "http://foo.com:4321/"u8, true),
        new("http://foo.com/"u8, "http://bar.com/"u8, false),
        new("http://foo.com/"u8, "http://[::1%25.foo.com]/"u8, false), // But subdomains should work:

        new("http://foo.com/"u8, "http://foo.com/"u8, true),
        new("http://foo.com/"u8, "http://sub.foo.com/"u8, true),
        new("http://foo.com/"u8, "http://notfoo.com/"u8, false),
        new("http://foo.com/"u8, "https://foo.com/"u8, true),
        new("http://foo.com:80/"u8, "http://foo.com/"u8, true),
        new("http://foo.com:80/"u8, "http://sub.foo.com/"u8, true),
        new("http://foo.com:443/"u8, "https://foo.com/"u8, true),
        new("http://foo.com:443/"u8, "https://sub.foo.com/"u8, true),
        new("http://foo.com:1234/"u8, "http://foo.com/"u8, true),
        new("http://foo.com/"u8, "http://foo.com/"u8, true),
        new("http://foo.com/"u8, "http://sub.foo.com/"u8, true),
        new("http://foo.com/"u8, "http://notfoo.com/"u8, false),
        new("http://foo.com/"u8, "https://foo.com/"u8, true),
        new("http://foo.com:80/"u8, "http://foo.com/"u8, true),
        new("http://foo.com:80/"u8, "http://sub.foo.com/"u8, true),
        new("http://foo.com:443/"u8, "https://foo.com/"u8, true),
        new("http://foo.com:443/"u8, "https://sub.foo.com/"u8, true),
        new("http://foo.com:1234/"u8, "http://foo.com/"u8, true)
    }.slice();
    foreach (var (i, tt) in tests) {
        var (u0, err) = url.Parse(tt.initialURL);
        if (err != default!) {
            Ꮡt.Errorf("%d. initial URL %q parse error: %v"u8, i, tt.initialURL, err);
            continue;
        }
        (var u1, err) = url.Parse(tt.destURL);
        if (err != default!) {
            Ꮡt.Errorf("%d. dest URL %q parse error: %v"u8, i, tt.destURL, err);
            continue;
        }
        var got = http_internal_test_package.Export_shouldCopyHeaderOnRedirect(u0, u1);
        if (got != tt.want) {
            Ꮡt.Errorf("%d. shouldCopyHeaderOnRedirect(%q => %q) = %v; want %v"u8,
                i, tt.initialURL, tt.destURL, got, tt.want);
        }
    }
}

public static void TestClientRedirectTypes(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientRedirectTypes(Δp0, Δp1));
}

[GoType("dyn")] internal partial struct testClientRedirectTypes_tests {
    internal @string method;
    internal nint serverStatus;
    internal @string wantMethod; // desired subsequent client method
}

internal static void testClientRedirectTypes(ж<testing.T> Ꮡt, testMode mode) {
    var tests = new array<testClientRedirectTypes_tests>(30){
        [0] = new(method: "POST"u8, serverStatus: 301, wantMethod: "GET"u8),
        [1] = new(method: "POST"u8, serverStatus: 302, wantMethod: "GET"u8),
        [2] = new(method: "POST"u8, serverStatus: 303, wantMethod: "GET"u8),
        [3] = new(method: "POST"u8, serverStatus: 307, wantMethod: "POST"u8),
        [4] = new(method: "POST"u8, serverStatus: 308, wantMethod: "POST"u8),
        [5] = new(method: "HEAD"u8, serverStatus: 301, wantMethod: "HEAD"u8),
        [6] = new(method: "HEAD"u8, serverStatus: 302, wantMethod: "HEAD"u8),
        [7] = new(method: "HEAD"u8, serverStatus: 303, wantMethod: "HEAD"u8),
        [8] = new(method: "HEAD"u8, serverStatus: 307, wantMethod: "HEAD"u8),
        [9] = new(method: "HEAD"u8, serverStatus: 308, wantMethod: "HEAD"u8),
        [10] = new(method: "GET"u8, serverStatus: 301, wantMethod: "GET"u8),
        [11] = new(method: "GET"u8, serverStatus: 302, wantMethod: "GET"u8),
        [12] = new(method: "GET"u8, serverStatus: 303, wantMethod: "GET"u8),
        [13] = new(method: "GET"u8, serverStatus: 307, wantMethod: "GET"u8),
        [14] = new(method: "GET"u8, serverStatus: 308, wantMethod: "GET"u8),
        [15] = new(method: "DELETE"u8, serverStatus: 301, wantMethod: "GET"u8),
        [16] = new(method: "DELETE"u8, serverStatus: 302, wantMethod: "GET"u8),
        [17] = new(method: "DELETE"u8, serverStatus: 303, wantMethod: "GET"u8),
        [18] = new(method: "DELETE"u8, serverStatus: 307, wantMethod: "DELETE"u8),
        [19] = new(method: "DELETE"u8, serverStatus: 308, wantMethod: "DELETE"u8),
        [20] = new(method: "PUT"u8, serverStatus: 301, wantMethod: "GET"u8),
        [21] = new(method: "PUT"u8, serverStatus: 302, wantMethod: "GET"u8),
        [22] = new(method: "PUT"u8, serverStatus: 303, wantMethod: "GET"u8),
        [23] = new(method: "PUT"u8, serverStatus: 307, wantMethod: "PUT"u8),
        [24] = new(method: "PUT"u8, serverStatus: 308, wantMethod: "PUT"u8),
        [25] = new(method: "MADEUPMETHOD"u8, serverStatus: 301, wantMethod: "GET"u8),
        [26] = new(method: "MADEUPMETHOD"u8, serverStatus: 302, wantMethod: "GET"u8),
        [27] = new(method: "MADEUPMETHOD"u8, serverStatus: 303, wantMethod: "GET"u8),
        [28] = new(method: "MADEUPMETHOD"u8, serverStatus: 307, wantMethod: "MADEUPMETHOD"u8),
        [29] = new(method: "MADEUPMETHOD"u8, serverStatus: 308, wantMethod: "MADEUPMETHOD"u8)
    };
    var handlerc = new channel<Δhttp.HandlerFunc>(1);
    var handlercʗ1 = handlerc;
    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter rw, ж<Δhttp.Request> req) => {
        var h = ᐸꟷ(handlercʗ1);
        h(rw, req);
    }))).Value.ts;
    var c = ts.Client();
    foreach (var (i, vᴛ1) in tests.ΔRangeSnapshot()) {
        ref var tt = ref heap(new testClientRedirectTypes_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var tsʗ1 = ts;
        var ttʗ1 = tt;
        handlerc.ᐸꟷ((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(locationˢ, (~tsʗ1).URL);
            w.WriteHeader(ttʗ1.serverStatus);
        });
        var (req, err) = NewRequest(tt.method, (~ts).URL, default!);
        if (err != default!) {
            Ꮡt.Errorf("#%d: NewRequest: %v"u8, i, err);
            continue;
        }
        var handlercʗ2 = handlerc;
        var ttʗ2 = tt;
        c.Value.CheckRedirect = error (ж<Δhttp.Request> reqΔ1, slice<ж<Δhttp.Request>> via) => {
            {
                @string got = reqΔ1.Value.Method;
                @string want = ttʗ2.wantMethod; if (got != want) {
                    return fmt.Errorf("#%d: got next method %q; want %q"u8, i, got, want);
                }
            }
            handlercʗ2.ᐸꟷ((Δhttp.ResponseWriter rw, ж<Δhttp.Request> reqΔ2) => {
            });
            // TODO: Check that the body is valid when we do 307 and 308 support
            return default!;
        };
        (var res, err) = c.Do(req);
        if (err != default!) {
            Ꮡt.Errorf("#%d: Response: %v"u8, i, err);
            continue;
        }
        (~res).Body.Close();
    }
}

// issue18239Body is an io.ReadCloser for TestTransportBodyReadError.
// Its Read returns readErr and increments *readCalls atomically.
// Its Close returns nil and increments *closeCalls atomically.
[GoType] partial struct issue18239Body {
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
    return default!;
}

// Issue 18239: make sure the Transport doesn't retry requests with bodies
// if Request.GetBody is not defined.
public static void TestTransportBodyReadError(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testTransportBodyReadError(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xBodyReadˢ = "X-Body-Read"u8;
internal static readonly @string someBodyReadErrorˢ = "some body read error"u8;

internal static void testTransportBodyReadError(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var ts = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        if ((~(~r).URL).Path == "/ping"u8) {
            return;
        }
        var buf = new slice<byte>(1);
        var (n, errΔ1) = (~r).Body.Read(buf);
        w.Header().Set(xBodyReadˢ, fmt.Sprintf("%v, %v"u8, n, errΔ1));
    }))).Value.ts;
    var c = ts.Client();
    var tr = (~c).Transport._<ж<Δhttp.Transport>>();
    // Do one initial successful request to create an idle TCP connection
    // for the subsequent request to reuse. (The Transport only retries
    // requests on reused connections.)
    var (res, err) = c.Get((~ts).URL + "/ping"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (~res).Body.Close();
    ref var readCallsAtomic = ref heap(new int32(), out var ᏑreadCallsAtomic);
    ref var closeCallsAtomic = ref heap(new int32(), out var ᏑcloseCallsAtomic); // atomic
    var someErr = errors.New(someBodyReadErrorˢ);
    var body = new issue18239Body(ᏑreadCallsAtomic, ᏑcloseCallsAtomic, someErr);
    (var req, err) = NewRequest(postˢ, (~ts).URL, body);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    req = req.WithT(Ꮡt);
    (_, err) = tr.RoundTrip(req);
    if (!AreEqual(err, someErr)) {
        Ꮡt.Errorf("Got error: %v; want Request.Body read error: %v"u8, err, someErr);
    }
    // And verify that our Body wasn't used multiple times, which
    // would indicate retries. (as it buggily was during part of
    // Go 1.8's dev cycle)
    var readCalls = atomic.LoadInt32(ᏑreadCallsAtomic);
    var closeCalls = atomic.LoadInt32(ᏑcloseCallsAtomic);
    if (readCalls != 1) {
        Ꮡt.Errorf("read calls = %d; want 1"u8, readCalls);
    }
    if (closeCalls != 1) {
        Ꮡt.Errorf("close calls = %d; want 1"u8, closeCalls);
    }
}

[GoType] partial struct roundTripperWithoutCloseIdle {
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

public static void TestClientCloseIdleConnections(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var c = Ꮡ(new Client(Transport: new roundTripperWithoutCloseIdle(nil)));
    c.CloseIdleConnections(); // verify we don't crash at least
    var closed = false;
    Δhttp.RoundTripper tr = new http_test_package.roundTripperWithCloseIdleᴠRoundTripper(new roundTripperWithCloseIdle(() => {
        closed = true;
    }));
    c = Ꮡ(new Client(Transport: tr));
    c.CloseIdleConnections();
    if (!closed) {
        Ꮡt.Error(notClosedˢ);
    }
}

internal delegate (ж<Δhttp.Response>, error) testRoundTripper(ж<Δhttp.Request> _);

internal static partial (ж<Δhttp.Response>, error) RoundTrip(this testRoundTripper t, ж<Δhttp.Request> Ꮡreq) {
    return t(Ꮡreq);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noDeadlineˢ = (@string)"no deadline"u8;
internal static readonly @string notActuallyMakingAˢ = "not actually making a request"u8;
internal static readonly @string httpsExampleTldˢ = "https://example.tld/"u8;

public static void TestClientPropagatesTimeoutToContext(ж<testing.T> Ꮡt) {
    var c = Ꮡ(new Client(
        Timeout: (time.Duration)(5000000000L),
        Transport: new http_test_package.testRoundTripperᴠRoundTripper(new testRoundTripper((ж<Δhttp.Request> req) => {
            var ctx = req.Context();
            var (deadline, ok) = ctx.Deadline();
            if (!ok){
                Ꮡt.Error(noDeadlineˢ);
            } else {
                Ꮡt.Logf("deadline in %v"u8, deadline.Sub(time.Now()).Round(time.ΔSecond / 10));
            }
            return (default!, errors.New(notActuallyMakingAˢ));
        }))
    ));
    c.Get(httpsExampleTldˢ);
}

// Issue 33545: lock-in the behavior promised by Client.Do's
// docs about request cancellation vs timing out.
public static void TestClientDoCanceledVsTimeout(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientDoCanceledVsTimeout(Δp0, Δp1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedlyGotANilErrorˢ = (@string)"Unexpectedly got a nil error"u8;

internal static void testClientDoCanceledVsTimeout(ж<testing.T> Ꮡt, testMode mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Write(slice<byte>("Hello, World!"u8));
    })));
    var cases = new @string[]{"timeout"u8, "canceled"u8}.slice();
    foreach (var (_, name) in cases) {
        var cstʗ1 = cst;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                context.Context ctx = default!;
                Action cancel = default!;
                if (name == "timeout"u8){
                    (ctx, cancel) = context_package.WithTimeout(context_package.Background(), -time.ΔNanosecond);
                } else {
                    (ctx, cancel) = context_package.WithCancel(context_package.Background());
                    cancel();
                }
                var cancelʗ1 = cancel;
                defer(cancelʗ1, ref ᒐ);
                var (req, _) = NewRequestWithContext(ctx, getˢ2, (~(~cstʗ1).ts).URL, default!);
                var (_, err) = (~cstʗ1).c.Do(req);
                if (err == default!) {
                    tΔ1.Fatal(unexpectedlyGotANilErrorˢ);
                }
                var ue = err._<ж<urlꓸError>>();
                bool wantIsTimeout = default!;
                error wantErr = context_package.Canceled;
                if (name == "timeout"u8) {
                    wantErr = context_package.DeadlineExceeded;
                    wantIsTimeout = true;
                }
                {
                    var (g, w) = (ue.Timeout(), wantIsTimeout); if (g != w) {
                        tΔ1.Fatalf("url.Timeout() = %t, want %t"u8, g, w);
                    }
                }
                {
                    var (g, w) = (ue.Value.Err, wantErr); if (!AreEqual(g, w)) {
                        tΔ1.Errorf("url.Error.Err = %v; want %v"u8, g, w);
                    }
                }
                {
                    var got = errors.Is(err, context_package.DeadlineExceeded); if (got != wantIsTimeout) {
                        tΔ1.Errorf("errors.Is(err, context.DeadlineExceeded) = %v, want %v"u8, got, wantIsTimeout);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

[GoType] partial struct nilBodyRoundTripper {
}

internal static (ж<Δhttp.Response>, error) RoundTrip(this nilBodyRoundTripper _, ж<Δhttp.Request> Ꮡreq) {
    return (Ꮡ(new Response(
        StatusCode: StatusOK,
        Status: StatusText(StatusOK),
        Body: default!,
        Request: Ꮡreq
    )), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpLocalhostAnythingˢ = "http://localhost/anything"u8;

public static void TestClientPopulatesNilResponseBody(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var c = Ꮡ(new Client(Transport: new nilBodyRoundTripper(nil)));
        var (resp, err) = c.Get(httpLocalhostAnythingˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Client.Get rejected Response with nil Body: %v"u8, err);
        }
        if ((~resp).Body == default!) {
            Ꮡt.Fatalf("Client failed to provide a non-nil Body as documented"u8);
        }
        var respʗ1 = resp;
        defer(() => {
            {
                var errΔ1 = (~respʗ1).Body.Close(); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("error from Close on substitute Response.Body: %v"u8, errΔ1);
                }
            }
        }, ref ᒐ);
        {
            var (b, errΔ2) = io.ReadAll((~resp).Body); if (errΔ2 != default!){
                Ꮡt.Errorf("read error from substitute Response.Body: %v"u8, errΔ2);
            } else 
            if (len(b) != 0) {
                Ꮡt.Errorf("substitute Response.Body was unexpectedly non-empty: %q"u8, b);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 40382: Client calls Close multiple times on Request.Body.
public static void TestClientCallsCloseOnlyOnce(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testClientCallsCloseOnlyOnce(Δp0, Δp1));
}

internal static void testClientCallsCloseOnlyOnce(ж<testing.T> Ꮡt, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(StatusNoContent);
    })));
    // Issue occurred non-deterministically: needed to occur after a successful
    // write (into TCP buffer) but before end of body.
    for (nint i = 0; i < 50 && !Ꮡt.Failed(); i++) {
        var body = Ꮡ(new issue40382Body(t: Ꮡt, n: 300000));
        var (req, err) = NewRequest(MethodPost, (~(~cst).ts).URL, new http_test_package.issue40382BodyжReader(body));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var resp, err) = (~cst).tr.RoundTrip(req);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (~resp).Body.Close();
    }
}

// issue40382Body is an io.ReadCloser for TestClientCallsCloseOnlyOnce.
// Its Read reads n bytes before returning io.EOF.
// Its Close returns nil but fails the test if called more than once.
[GoType] partial struct issue40382Body {
    internal ж<testing.T> t;
    internal nint n;
    internal int32 closeCallsAtomic;
}

[GoRecv] internal static (nint, error) Read(this ref issue40382Body b, slice<byte> p) {
    var matchᴛ1 = false;
    if (b.n is 0) { matchᴛ1 = true;
        return (0, io.EOF);
    }
    if (b.n < len(p)) { matchᴛ1 = true;
        p = p.slice(0, b.n);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        foreach (var (i, _) in p) {
            p[i] = (rune)'x';
        }
        b.n -= len(p);
        return (len(p), default!);
    }
    return default!;

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bodyClosedMoreThanOnceˢ = (@string)"Body closed more than once"u8;

internal static error Close(this ж<issue40382Body> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (atomic.AddInt32(Ꮡb.of(issue40382Body.ᏑcloseCallsAtomic), 1) == 2) {
        b.t.Error(bodyClosedMoreThanOnceˢ);
    }
    return default!;
}

public static void TestProbeZeroLengthBody(ж<testing.T> Ꮡt) {
    run<TжTBRun>(Ꮡt, (Δp0, Δp1) => testProbeZeroLengthBody(Δp0, Δp1));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ = "body"u8;

internal static partial void testProbeZeroLengthBody(ж<testing.T> Ꮡt, testMode mode) {
    var reqc = new channel<EmptyStruct>(0);
    var reqcʗ1 = reqc;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(Ꮡt), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        builtin.close(reqcʗ1);
        {
            var (_, err) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), (~r).Body); if (err != default!) {
                Ꮡt.Errorf("error copying request body: %v"u8, err);
            }
        }
    })));
    var (bodyr, bodyw) = io.Pipe();
    @string gotBody = default!;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var bodyrʗ1 = bodyr;
    var cstʗ1 = cst;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            var (req, _) = NewRequest(getˢ2, (~(~cstʗ1).ts).URL, new io.PipeReaderжReader(bodyrʗ1));
            var (res, err) = (~cstʗ1).c.Do(req);
            (var b, err) = io.ReadAll((~res).Body);
            if (err != default!) {
                Ꮡt.Error(err);
            }
            gotBody = ((@string)b);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var selᴛ6 = reqc;
    var selᴛ7 = time.After((time.Duration)(60000000000L));
    switch (select(ᐸꟷ(selᴛ6, ꓸꓸꓸ), ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
    case 0 when selᴛ6.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ7.ꟷᐳ(out _): {
        Ꮡt.Errorf("request not sent after 60s"u8);
        break;
    }}
    // Request should be sent after trying to probe the request body for 200ms.
    // Write the request body and wait for the request to complete.
    @string content = contentᶜ;
    bodyw.Write(slice<byte>(content));
    bodyw.Close();
    Ꮡwg.Wait();
    if (gotBody != content) {
        Ꮡt.Fatalf("server got body %q, want %q"u8, gotBody, content);
    }
}

} // end http_test_package
