// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using context = context_package;
using fmt = fmt_package;
using io = io_package;
using rand = math.rand_package;
using http = go.net.http_package;
using url = go.net.url_package;
using runtime = runtime_package;
using pprof = go.runtime.pprof_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.net;
using go.runtime;
using math;
using static go.net.http.httputil_package;

partial class httputil_internal_test_package {

internal partial struct eofReader {
}

internal static error Close(this eofReader n) { return default; }

internal static (nint, error) Read(this eofReader n, slice<byte> _) { return (0, io.EOF); }

internal partial struct dumpTest {
    // Either Req or GetReq can be set/nil but not both.
    public ж<http.Request> Req;
    public Func<ж<http.Request>> GetReq;
    public any Body; // optional []byte or func() io.ReadCloser to populate Req.Body
    public @string WantDump;
    public @string WantDumpOut;
    public bool MustError; // if true, the test is expected to throw an error
    public bool NoBody;    // if true, set DumpRequest{,Out} body to false
}

internal static slice<dumpTest> dumpTests = slice<dumpTest>([
    // HTTP/1.1 => chunked coding; body; empty trailer
    new(
        Req: Ꮡ(new http.Request(
            Method: "GET",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "www.google.com",
                Path: "/search"
            )),
            ProtoMajor: 1,
            ProtoMinor: 1,
            TransferEncoding: ["chunked"]
        )),

        Body: slice<byte>("abcdef"u8),

        WantDump: "GET /search HTTP/1.1\r\n"u8 + "Host: www.google.com\r\n"u8 + "Transfer-Encoding: chunked\r\n\r\n"u8 + chunk("abcdef") + chunk("")
    ),

    // Verify that DumpRequest preserves the HTTP version number, doesn't add a Host,
    // and doesn't add a User-Agent.
    new(
        Req: Ꮡ(new http.Request(
            Method: "GET",
            URL: mustParseURL("/foo"),
            ProtoMajor: 1,
            ProtoMinor: 0,
            Header: new httpꓸHeader(new map<@string, slice<@string>>{
                ["X-Foo"] = ["X-Bar"]
            })
        )),

        WantDump: "GET /foo HTTP/1.0\r\n"u8 + "X-Foo: X-Bar\r\n\r\n"u8
    ),

    new(
        Req: mustNewRequest("GET", "http://example.com/foo", default),

        WantDumpOut: "GET /foo HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Accept-Encoding: gzip\r\n\r\n"u8
    ),

    // Test that an https URL doesn't try to do an SSL negotiation
    // with a bytes.Buffer and hang with all goroutines not
    // runnable.
    new(
        Req: mustNewRequest("GET", "https://example.com/foo", default),
        WantDumpOut: "GET /foo HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Accept-Encoding: gzip\r\n\r\n"u8
    ),

    // Request with Body, but Dump requested without it.
    new(
        Req: Ꮡ(new http.Request(
            Method: "POST",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "post.tld",
                Path: "/"
            )),
            ContentLength: 6,
            ProtoMajor: 1,
            ProtoMinor: 1
        )),

        Body: slice<byte>("abcdef"u8),

        WantDumpOut: "POST / HTTP/1.1\r\n"u8 + "Host: post.tld\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 6\r\n"u8 + "Accept-Encoding: gzip\r\n\r\n"u8,

        NoBody: true
    ),

    // Request with Body > 8196 (default buffer size)
    new(
        Req: Ꮡ(new http.Request(
            Method: "POST",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "post.tld",
                Path: "/"
            )),
            Header: new httpꓸHeader(new map<@string, slice<@string>>{
                ["Content-Length"] = ["8193"]
            }),

            ContentLength: 8193,
            ProtoMajor: 1,
            ProtoMinor: 1
        )),

        Body: bytes.Repeat(slice<byte>("a"u8), 8193),

        WantDumpOut: "POST / HTTP/1.1\r\n"u8 + "Host: post.tld\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 8193\r\n"u8 + "Accept-Encoding: gzip\r\n\r\n"u8 + strings.Repeat("a", 8193),
        WantDump: "POST / HTTP/1.1\r\n"u8 + "Host: post.tld\r\n"u8 + "Content-Length: 8193\r\n\r\n"u8 + strings.Repeat("a", 8193)
    ),

    new(
        GetReq: () => mustReadRequest("GET http://foo.com/ HTTP/1.1\r\n"u8 + "User-Agent: blah\r\n\r\n"u8),
        NoBody: true,
        WantDump: "GET http://foo.com/ HTTP/1.1\r\n"u8 + "User-Agent: blah\r\n\r\n"u8
    ),

    // Issue #7215. DumpRequest should return the "Content-Length" when set
    new(
        GetReq: () => mustReadRequest("POST /v2/api/?login HTTP/1.1\r\n"u8 + "Host: passport.myhost.com\r\n"u8 + "Content-Length: 3\r\n"u8 + "\r\nkey1=name1&key2=name2"u8),
        WantDump: "POST /v2/api/?login HTTP/1.1\r\n"u8 + "Host: passport.myhost.com\r\n"u8 + "Content-Length: 3\r\n"u8 + "\r\nkey"u8
    ),
    // Issue #7215. DumpRequest should return the "Content-Length" in ReadRequest
    new(
        GetReq: () => mustReadRequest("POST /v2/api/?login HTTP/1.1\r\n"u8 + "Host: passport.myhost.com\r\n"u8 + "Content-Length: 0\r\n"u8 + "\r\nkey1=name1&key2=name2"u8),
        WantDump: "POST /v2/api/?login HTTP/1.1\r\n"u8 + "Host: passport.myhost.com\r\n"u8 + "Content-Length: 0\r\n\r\n"u8
    ),

    // Issue #7215. DumpRequest should not return the "Content-Length" if unset
    new(
        GetReq: () => mustReadRequest("POST /v2/api/?login HTTP/1.1\r\n"u8 + "Host: passport.myhost.com\r\n"u8 + "\r\nkey1=name1&key2=name2"u8),
        WantDump: "POST /v2/api/?login HTTP/1.1\r\n"u8 + "Host: passport.myhost.com\r\n\r\n"u8
    ),

    // Issue 18506: make drainBody recognize NoBody. Otherwise
    // this was turning into a chunked request.
    new(
        Req: mustNewRequest("POST", "http://example.com/foo", new httputil_test_package.http_noBodyᴠReader(http.NoBody)),
        WantDumpOut: "POST /foo HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Content-Length: 0\r\n"u8 + "Accept-Encoding: gzip\r\n\r\n"u8
    ),

    // Issue 34504: a non-nil Body without ContentLength set should be chunked
    new(
        Req: Ꮡ(new http.Request(
            Method: "PUT",
            URL: Ꮡ(new url.URL(
                Scheme: "http",
                Host: "post.tld",
                Path: "/test"
            )),
            ContentLength: 0,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            Body: new httputil_internal_test_package.eofReaderжReadCloser(Ꮡ(new eofReader()))
        )),
        NoBody: true,
        WantDumpOut: "PUT /test HTTP/1.1\r\n"u8 + "Host: post.tld\r\n"u8 + "User-Agent: Go-http-client/1.1\r\n"u8 + "Transfer-Encoding: chunked\r\n"u8 + "Accept-Encoding: gzip\r\n\r\n"u8
    ),

    // Issue 54616: request with Connection header doesn't result in duplicate header.
    new(
        GetReq: () => mustReadRequest("GET / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "Connection: close\r\n\r\n"u8),
        NoBody: true,
        WantDump: "GET / HTTP/1.1\r\n"u8 + "Host: example.com\r\n"u8 + "Connection: close\r\n\r\n"u8
    )
]);

public static partial void TestDumpRequest(ж<testing.T> t) {
    // Make a copy of dumpTests and add 10 new cases with an empty URL
    // to test that no goroutines are leaked. See golang.org/issue/32571.
    // 10 seems to be a decent number which always triggers the failure.
    var dumpTestsΔ1 = httputil_internal_test_package.dumpTests[..];
    for (nint i = 0; i < 10; i++) {
        dumpTestsΔ1 = append(dumpTestsΔ1, new dumpTest(
            Req: mustNewRequest(getˢ, ""u8, default),
            MustError: true
        ));
    }
    nint numg0 = runtime.NumGoroutine();
    foreach (var (i, tt) in range(dumpTestsΔ1)) {
        if (tt.Req != nil && tt.GetReq != nil || tt.Req == nil && tt.GetReq == nil) {
            t.Errorf("#%d: either .Req(%p) or .GetReq(%p) can be set/nil but not both", i, tt.Req.OrTypedNil(), (tt.GetReq).OrTypedNilFunc());
            continue;
        }

        ж<http.Request> freshReq(dumpTest ti) {
            var req = ti.Req;
            if (req == nil) {
                req = ti.GetReq();
            }

            if (req.Header == nil) {
                req.Header = new httpꓸHeader(0);
            }

            if (ti.Body == nil) {
                return req;
            }
            switch (ti.Body.type()) {
            case slice<byte> b: {
                req.Body = io.NopCloser(new httputil_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
                break;
            }
            case Func<io.ReadCloser> b: {
                req.Body = b();
                break;
            }
            default: {
                var b = ti.Body;
                t.Fatalf("Test %d: unsupported Body of %T", i, ti.Body);
                break;
            }}
            return req;
        }

        if (tt.WantDump != ""u8) {
            var req = freshReq(tt);
            var (dump, err) = DumpRequest(req, !tt.NoBody);
            if (err != nil) {
                t.Errorf("DumpRequest #%d: %s\nWantDump:\n%s", i, err, tt.WantDump);
                continue;
            }
            if (((sstring)dump) != tt.WantDump) {
                t.Errorf("DumpRequest %d, expecting:\n%s\nGot:\n%s\n", i, tt.WantDump, (@string)dump);
                continue;
            }
        }

        if (tt.MustError) {
            var req = freshReq(tt);
            var (_, err) = DumpRequestOut(req, !tt.NoBody);
            if (err == nil) {
                t.Errorf("DumpRequestOut #%d: expected an error, got nil", i);
            }
            continue;
        }

        if (tt.WantDumpOut != ""u8) {
            var req = freshReq(tt);
            var (dump, err) = DumpRequestOut(req, !tt.NoBody);
            if (err != nil) {
                t.Errorf("DumpRequestOut #%d: %s", i, err);
                continue;
            }
            if (((sstring)dump) != tt.WantDumpOut) {
                t.Errorf("DumpRequestOut %d, expecting:\n%s\nGot:\n%s\n", i, tt.WantDumpOut, (@string)dump);
                continue;
            }
        }
    }

    // Validate we haven't leaked any goroutines.
    nint dg = default;
    var dl = deadline(t, (time.Duration)(5000000000L), time.ΔSecond);
    while (time.Now().Before(dl)) {
        dg = runtime.NumGoroutine() - numg0; if (dg <= 4) {
            // No unexpected goroutines.
            return;
        }

        // Allow goroutines to schedule and die off.
        runtime.Gosched();
    }

    var buf = new slice<byte>(4096);
    buf = buf.slice(0, runtime.Stack(buf, true));
    t.Errorf("Unexpectedly large number of new goroutines: %d new: %s", dg, buf);
}

// deadline returns the time which is needed before t.Deadline()
// if one is configured and it is s greater than needed in the future,
// otherwise defaultDelay from the current time.
internal static time.Time deadline(ж<testing.T> Ꮡt, time.Duration defaultDelay, time.Duration needed) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (dl, ok) = t.Deadline(); if (ok) {
        dl = dl.Add(-needed); if (dl.After(time.Now())) {
            // Allow an arbitrarily long delay.
            return dl;
        }
    }

    // No deadline configured or its closer than needed from now
    // so just use the default.
    return time.Now().Add(defaultDelay);
}

internal static @string chunk(@string s) {
    return fmt.Sprintf("%x\r\n%s\r\n"u8, len(s), s);
}

internal static ж<url.URL> mustParseURL(@string s) {
    var (u, err) = url.Parse(s);
    if (err != nil) {
        throw panic(fmt.Sprintf("Error parsing URL %q: %v"u8, s, err));
    }
    return u;
}

internal static ж<http.Request> mustNewRequest(@string method, @string url, io.Reader body) {
    var (req, err) = http.NewRequest(method, url, body);
    if (err != nil) {
        throw panic(fmt.Sprintf("NewRequest(%q, %q, %p) err = %v"u8, method, url, body, err));
    }
    return req;
}

internal static ж<http.Request> mustReadRequest(@string s) {
    var (req, err) = http.ReadRequest(bufio.NewReader(new httputil_test_package.strings_ReaderжReader(strings.NewReader(s))));
    if (err != nil) {
        throw panic(err);
    }
    return req;
}

partial struct dumpResTestsᴛ1 /*dyn*/ {
    internal ж<http.Response> res;
    internal bool body;
    internal @string want;
}
internal static slice<dumpResTestsᴛ1> dumpResTests = slice<dumpResTestsᴛ1>([
    new(
        res: Ꮡ(new http.Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 50,
            Header: new httpꓸHeader(new map<@string, slice<@string>>{
                ["Foo"] = ["Bar"]
            }),
            Body: io.NopCloser(new httputil_test_package.strings_ReaderжReader(strings.NewReader("foo"))) // shouldn't be used
        )),
        body: false, // to verify we see 50, not empty or 3.
        want: """
HTTP/1.1 200 OK
Content-Length: 50
Foo: Bar
"""u8.Raw()
    ),

    new(
        res: Ꮡ(new http.Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 3,
            Body: io.NopCloser(new httputil_test_package.strings_ReaderжReader(strings.NewReader("foo")))
        )),
        body: true,
        want: """
HTTP/1.1 200 OK
Content-Length: 3

foo
"""u8.Raw()
    ),

    new(
        res: Ꮡ(new http.Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: -1,
            Body: io.NopCloser(new httputil_test_package.strings_ReaderжReader(strings.NewReader("foo"))),
            TransferEncoding: ["chunked"]
        )),
        body: true,
        want: """
HTTP/1.1 200 OK
Transfer-Encoding: chunked

3
foo
0
"""u8.Raw()
    ),
    new(
        res: Ꮡ(new http.Response(
            Status: "200 OK",
            StatusCode: 200,
            Proto: "HTTP/1.1",
            ProtoMajor: 1,
            ProtoMinor: 1,
            ContentLength: 0,
            Header: new httpꓸHeader(new map<@string, slice<@string>>{
                // To verify if headers are not filtered out.
                ["Foo1"] = ["Bar1"],
                ["Foo2"] = ["Bar2"]
            }),
            Body: default
        )),
        body: false, // to verify we see 0, not empty.
        want: """
HTTP/1.1 200 OK
Foo1: Bar1
Foo2: Bar2
Content-Length: 0
"""u8.Raw()
    )
]);

public static void TestDumpResponse(ж<testing.T> t) {
    foreach (var (i, tt) in range(dumpResTests)) {
        var (gotb, err) = DumpResponse(tt.res, tt.body);
        if (err != nil) {
            t.Errorf("%d. DumpResponse = %v", i, err);
            continue;
        }
        @string got = (@string)gotb;
        got = strings.TrimSpace(got);
        got = strings.ReplaceAll(got, "\r", ""u8);

        if (got != tt.want) {
            t.Errorf("%d.\nDumpResponse got:\n%s\n\nWant:\n%s\n", i, got, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string httpExampleComˢ => ref httpExampleComˢᵇ;
private static readonly @string httpExampleComˢᵇ = "http://example.com"u8;
internal static ref readonly @string goroutineˢ => ref goroutineˢᵇ;
private static readonly @string goroutineˢᵇ = "goroutine"u8;

// Issue 38352: Check for deadlock on canceled requests.
public static partial void TestDumpRequestOutIssue38352(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (testing.Short()) {
            return;
        }
        Ꮡt.Parallel();

        var timeout = (time.Duration)(10000000000L);
        var (deadline, ok) = t.Deadline(); if (ok) {
            timeout = time.Until(deadline);
            timeout -= time.ΔSecond * 2; // Leave 2 seconds to report failures.
        }
        for (nint i = 0; i < 1000; i++) {
            var delay = ((time.Duration)(int64)rand.Intn(5)) * time.Millisecond;
            var (ctx, cancel) = context.WithTimeout(context.Background(), delay);
            var cancelʗ1 = cancel;
            defer(() => cancelʗ1(), ref ᒐ);

            var r = bytes.NewBuffer(new slice<byte>(10000));
            ref var err = ref heap<error>(out var Ꮡerr);
            (var req, err) = http.NewRequestWithContext(ctx, http.MethodPost, httpExampleComˢ, new httputil_test_package.bytes_BufferжReader(r));
            if (err != nil) {
                Ꮡt.Fatal(err);
            }

            var @out = new channel<error>(0);
            var outʗ1 = @out;
            var reqʗ1 = req;
            goǃ(() => {
                (_, Ꮡerr.ValueSlot) = DumpRequestOut(reqʗ1, true);
                outʗ1.ᐸꟷ(Ꮡerr.ValueSlot);
            });

            var selᴛ1 = @out;
            var selᴛ2 = time.After(timeout);
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
                var b = Ꮡ(new strings.Builder());
                fmt.Fprintf(new httputil_test_package.strings_BuilderжWriter(b), "deadlock detected on iteration %d after %s with delay: %v\n"u8, i, timeout, delay);
                pprof.Lookup(goroutineˢ).WriteTo(new httputil_test_package.strings_BuilderжWriter(b), 1);
                Ꮡt.Fatal(b.String());
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end httputil_internal_test_package
