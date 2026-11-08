// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests that use both the client & server, in both HTTP/1 and HTTP/2 mode.
namespace go.net;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using context = context_package;
using rand = crypto.rand_package;
using sha1 = crypto.sha1_package;
using tls = crypto.tls_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using log = log_package;
using maps = maps_package;
using net = net_package;
using static global::go.net.http_package;
using httptest = global::go.net.http.httptest_package;
using httptrace = global::go.net.http.httptrace_package;
using httputil = global::go.net.http.httputil_package;
using textproto = global::go.net.textproto_package;
using url = global::go.net.url_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using synctest = global::go.testing.synctest_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bufio = bufio_package;
using compress;
using crypto;
using global::go.net;
using global::go.net.http;
using global::go.sync;
using global::go.testing;
using iter = iter_package;
using static global::go.net.http_internal_test_package;
using Δhttp = global::go.net.http_package;
using ꓸꓸꓸany = Span<any>;

partial class http_test_package {

partial struct testMode /*@string*/;

internal static ref readonly testMode http1Mode => ref http1Modeᵇ;
private static readonly testMode http1Modeᵇ = "h1"u8;                       // HTTP/1.1
internal static ref readonly testMode https1Mode => ref https1Modeᵇ;
private static readonly testMode https1Modeᵇ = "https1"u8;                  // HTTPS/1.1
internal static ref readonly testMode http2Mode => ref http2Modeᵇ;
private static readonly testMode http2Modeᵇ = "h2"u8;                       // HTTP/2
internal static ref readonly testMode http2UnencryptedMode => ref http2UnencryptedModeᵇ;
private static readonly testMode http2UnencryptedModeᵇ = "h2unencrypted"u8; // HTTP/2

partial struct testAddMode /*[]testMode*/;  // default, plus these

partial struct testSkipMode /*[]testMode*/; // default, minus these

internal static @string Scheme(this testMode m) {
    var exprᴛ1 = m;
    switch (ᐧ) {
    case {} when exprᴛ1 == http1Mode || exprᴛ1 == http2UnencryptedMode: {
        return httpˢ;
    }
    case {} when exprᴛ1 == https1Mode || exprᴛ1 == http2Mode: {
        return httpsˢ;
    }}
    throw panic("unknown testMode");
}

partial struct testNotParallelOpt {
}

internal static testNotParallelOpt testNotParallel = new testNotParallelOpt();

partial interface TBRun<T> :
    testing.TB
{
    bool Run(@string _Δp0, Action<T> _Δp1);
}

// run runs a client/server test in a variety of test configurations.
//
// Tests execute in HTTP/1.1 and HTTP/2 modes by default.
// To run in a different set of configurations, pass a []testMode option.
//
// Tests call t.Parallel() by default.
// To disable parallel execution, pass the testNotParallel option.
internal static void run<T>(T t, Action<T, testMode> f, params ꓸꓸꓸany optsʗp)
    where T : TBRun<T>
{
    var opts = optsʗp.sslice();
    var modesᴷ = new stackAppend<testMode>();

    t.Helper();
    var modes = slice<testMode>([http1Mode, http2Mode]);
    var parallel = true;
    foreach (var (_, opt) in range(opts)) {
        switch (opt.type()) {
        case testAddMode optΔ1: {
            foreach (var (_, m) in range<testMode>(optΔ1)) {
                if (!slices.Contains(modes, m)) {
                    modes = append(modesᴷ, modes, m);
                }
            }
            break;
        }
        case testSkipMode optΔ1: {
            var optʗ1 = optΔ1;
            modes = slices.DeleteFunc(modes, (testMode m) => slices.Contains(optʗ1, m));
            break;
        }
        case slice<testMode> optΔ1: {
            modes = optΔ1;
            break;
        }
        case testNotParallelOpt optΔ1: {
            parallel = false;
            break;
        }
        default: {
            var optΔ1 = opt;
            t.Fatalf("unknown option type %T", optΔ1);
            break;
        }}
    }
    {
        var (tΔ1, ok) = ((any)(t).OrTypedNilOf())._<ж<testing.T>>(ᐧ); if (ok && parallel) {
            setParallel(tΔ1);
        }
    }
    foreach (var (_, mode) in range(modes)) {
        t.Run((@string)mode, (T tΔ2) => {
            tΔ2.Helper();
            var (tΔ3, ok) = ((any)(tΔ2).OrTypedNilOf())._<ж<testing.T>>(ᐧ); if (ok && parallel) {
                setParallel(tΔ3);
            }
            tΔ2.Cleanup(() => {
                afterTest(tΔ2);
            });
            f(tΔ2, mode);
        });
    }
}

// runSynctest is run combined with synctest.Run.
//
// The TB passed to f arranges for cleanup functions to be run in the synctest bubble.
internal static void runSynctest(ж<testing.T> t, Action<ж<testing.T>, testMode> f, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.sslice();

    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        synctest.Test(tΔ1, (ж<testing.T> tΔ2) => {
            f(tΔ2, mode);
        });
    }, opts.ꓸꓸꓸ);
}

partial struct clientServerTest {
    internal testing.TB t;
    internal bool h2;
    internal httpꓸHandler h;
    internal ж<httptest.Server> ts;
    internal ж<Δhttp.Transport> tr;
    internal ж<Δhttp.Client> c;
    internal ж<fakeNetListener> li;
}

internal static void close(this ref clientServerTest t) {
    t.tr.CloseIdleConnections();
    t.ts.Close();
}

internal static @string getURL(this ж<clientServerTest> Ꮡt, @string u) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var (res, err) = t.c.Get(u);
        if (err != nil) {
            t.t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var slurp, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.t.Fatal(err);
        }
        return (@string)slurp;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static @string scheme(this ref clientServerTest t) {
    if (t.h2) {
        return httpsˢ;
    }
    return httpˢ;
}

internal static Action<ж<httptest.Server>> optQuietLog;
internal static void initᴛoptQuietLog() { optQuietLog = (ж<httptest.Server> ts) => {
    ts.Config.ErrorLog = quietLog;
}; }

internal static Action<ж<httptest.Server>> optWithServerLog(ж<log.Logger> lg) {
    return (ж<httptest.Server> ts) => {
        ts.Config.ErrorLog = lg;
    };
}

internal static ж<EmptyStruct> optFakeNet = @new<EmptyStruct>();

// newClientServerTest creates and starts an httptest.Server.
//
// The mode parameter selects the implementation to test:
// HTTP/1, HTTP/2, etc. Tests using newClientServerTest should use
// the 'run' function, which will start a subtests for each tested mode.
//
// The vararg opts parameter can include functions to configure the
// test server or transport.
//
//	func(*httptest.Server) // run before starting the server
//	func(*http.Transport)
//
// The optFakeNet option configures the server and client to use a fake network implementation,
// suitable for use in testing/synctest tests.
internal static ж<clientServerTest> newClientServerTest(testing.TB t, testMode mode, httpꓸHandler h, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.slice();
    var transportFuncsᴷ = new stackAppend<Action<ж<Δhttp.Transport>>>();

    if (mode == http2Mode || mode == http2UnencryptedMode) {
        http_internal_test_package.CondSkipHTTP2(t);
    }
    var cst = Ꮡ(new clientServerTest(
        t: t,
        h2: mode == http2Mode,
        h: h
    ));

    slice<Action<ж<Δhttp.Transport>>> transportFuncs = default;

    {
        nint idx = slices.Index(opts, (any)optFakeNet.OrTypedNil()); if (idx >= 0) {
            opts = slices.Delete<slice<any>, any>(opts, idx, idx + 1);
            cst.li = fakeNetListen();
            cst.ts = Ꮡ(new httptest.Server(
                Config: Ꮡ(new Server(Handler: h)),
                Listener: new http_test_package.fakeNetListenerжListener(cst.li)
            ));
            var cstʗ1 = cst;
            transportFuncs = append(transportFuncsᴷ, transportFuncs, (ж<Δhttp.Transport> tr) => {
                var cstʗ2 = cstʗ1;
                tr.DialContext = (net.Conn, error) (context.Context ctx, @string network, @string addr) => (new http_test_package.fakeNetConnжConn(cstʗ2.li.connect()), default);
            });
        } else {
            cst.ts = httptest.NewUnstartedServer(h);
        }
    }

    if (mode == http2UnencryptedMode) {
        var p = Ꮡ(new Protocols());
        p.SetUnencryptedHTTP2(true);
        cst.ts.Config.Protocols = p;
    }

    foreach (var (_, opt) in range(opts)) {
        switch (opt.type()) {
        case Action<ж<Δhttp.Transport>> optΔ1: {
            transportFuncs = append(transportFuncs, optΔ1);
            break;
        }
        case Action<ж<httptest.Server>> optΔ1: {
            optΔ1(cst.ts);
            break;
        }
        case Action<ж<Δhttp.Server>> optΔ1: {
            optΔ1(cst.ts.Config);
            break;
        }
        default: {
            var optΔ1 = opt;
            t.Fatalf("unhandled option type %T", optΔ1);
            break;
        }}
    }

    if (cst.ts.Config.ErrorLog == nil) {
        cst.ts.Config.ErrorLog = log.New(new testLogWriter(t), ""u8, 0);
    }

    var exprᴛ1 = mode;
    switch (ᐧ) {
    case {} when exprᴛ1 == http1Mode: {
        cst.ts.Start();
        break;
    }
    case {} when exprᴛ1 == https1Mode: {
        cst.ts.StartTLS();
        break;
    }
    case {} when exprᴛ1 == http2UnencryptedMode: {
        http_internal_test_package.ExportHttp2ConfigureServer(cst.ts.Config, nil);
        cst.ts.Start();
        break;
    }
    case {} when exprᴛ1 == http2Mode: {
        http_internal_test_package.ExportHttp2ConfigureServer(cst.ts.Config, nil);
        cst.ts.TLS = cst.ts.Config.TLSConfig;
        cst.ts.StartTLS();
        break;
    }
    default: {
        t.Fatalf("unknown test mode %v", mode);
        break;
    }}
    cst.c = cst.ts.Client();
    cst.tr = cst.c.Transport._<ж<Δhttp.Transport>>();
    if (mode == http2Mode || mode == http2UnencryptedMode) {
        var err = http_internal_test_package.ExportHttp2ConfigureTransport(cst.tr); if (err != nil) {
            t.Fatal(err);
        }
    }
    foreach (var (_, f) in range(transportFuncs)) {
        f(cst.tr);
    }

    if (mode == http2UnencryptedMode) {
        var p = Ꮡ(new Protocols());
        p.SetUnencryptedHTTP2(true);
        cst.tr.Protocols = p;
    }

    var cstʗ3 = cst;
    t.Cleanup(() => {
        cstʗ3.close();
    });
    return cst;
}

partial struct testLogWriter {
    internal testing.TB t;
}

internal static partial (nint, error) Write(this testLogWriter w, slice<byte> b) {
    w.t.Logf("server log: %v", strings.TrimSpace((@string)b));
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string realnetˢ => ref realnetˢᵇ;
private static readonly @string realnetˢᵇ = "realnet"u8;
internal static ref readonly @string synctestˢ => ref synctestˢᵇ;
private static readonly @string synctestˢᵇ = "synctest"u8;

// Testing the newClientServerTest helper itself.
public static void TestNewClientServerTest(ж<testing.T> t) {
    var modes = slice<testMode>([http1Mode, https1Mode, http2Mode]);
    var modesʗ1 = modes;
    t.Run(realnetˢ, (ж<testing.T> tΔ1) => {
        run<TжTBRun>(tΔ1, (TжTBRun tΔ2Δp, testMode mode) => {
            var tΔ2 = (ж<testing.T>)tΔ2Δp;
            testNewClientServerTest(tΔ2, mode);
        }, modesʗ1);
    });
    var modesʗ2 = modes;
    t.Run(synctestˢ, (ж<testing.T> tΔ3) => {
        runSynctest(tΔ3, (ж<testing.T> tΔ4, testMode mode) => {
            testNewClientServerTest(tΔ4, mode, optFakeNet.OrTypedNil());
        }, modesʗ2);
    });
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string http11ˢ => ref http11ˢᵇ;
private static readonly @string http11ˢᵇ = "HTTP/1.1"u8;
internal static ref readonly @string http20ˢ => ref http20ˢᵇ;
private static readonly @string http20ˢᵇ = "HTTP/2.0"u8;

internal partial struct testNewClientServerTest_got /*dyn*/ {
    /*embed*/ public sync_package.Mutex Mutex;
    internal @string proto;
    internal bool hasTLS;
}

internal static void testNewClientServerTest(ж<testing.T> t, testMode mode, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.sslice();

    ref var got = ref heap(new testNewClientServerTest_got(), out var Ꮡgot);
    var h = new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            Ꮡgot.of(testNewClientServerTest_got.ᏑMutex).Lock();
            defer(Ꮡgot.of(testNewClientServerTest_got.ᏑMutex).Unlock, ref ᒐ);
            Ꮡgot.Value.proto = r.Proto;
            Ꮡgot.Value.hasTLS = r.TLS != nil;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(h), opts.ꓸꓸꓸ);
    var (_, err) = cst.c.Head(cst.ts.URL); if (err != nil) {
        t.Fatal(err);
    }
    @string wantProto = default;
    bool wantTLS = default;
    var exprᴛ1 = mode;
    switch (ᐧ) {
    case {} when exprᴛ1 == http1Mode: {
        wantProto = http11ˢ;
        wantTLS = false;
        break;
    }
    case {} when exprᴛ1 == https1Mode: {
        wantProto = http11ˢ;
        wantTLS = true;
        break;
    }
    case {} when exprᴛ1 == http2Mode: {
        wantProto = http20ˢ;
        wantTLS = true;
        break;
    }}
    if (got.proto != wantProto) {
        t.Errorf("req.Proto = %q, want %q", got.proto, wantProto);
    }
    if (got.hasTLS != wantTLS) {
        t.Errorf("req.TLS set: %v, want %v", got.hasTLS, wantTLS);
    }
}

public static void TestChunkedResponseHeaders(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testChunkedResponseHeaders(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string intentionalGibberishˢ => ref intentionalGibberishˢᵇ;
private static readonly @string intentionalGibberishˢᵇ = "intentional gibberish"u8;

internal static void testChunkedResponseHeaders(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        log.SetOutput(io.Discard); // is noisy otherwise
        defer(log.SetOutput, new os.FileжWriter(os.Stderr), ref ᒐ);
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentLengthˢ, intentionalGibberishˢ); // we check that this is deleted
            w._<Flusher>().Flush();
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "I am a chunked response."u8);
        })));

        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatalf("Get error: %v", err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        var (g, e) = (res.ContentLength, (int64)(-1)); if (g != e) {
            t.Errorf("expected ContentLength of %d; got %d", e, g);
        }
        var wantTE = slice<@string>(["chunked"]);
        if (mode == http2Mode) {
            wantTE = default;
        }
        if (!slices.Equal<slice<@string>, @string>(res.TransferEncoding, wantTE)) {
            t.Errorf("TransferEncoding = %v; want %v", res.TransferEncoding, wantTE);
        }
        var (got, haveCL) = res.Header[contentLengthˢ, ꟷ]; if (haveCL) {
            t.Errorf("Unexpected Content-Length: %q", got);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// type ΔreqFunc is a methodless func type — rendered inline as its base delegate

// h12Compare is a test that compares HTTP/1 and HTTP/2 behavior
// against each other.
partial struct h12Compare {
    public Action<Δhttp.ResponseWriter, ж<Δhttp.Request>> Handler;             // required
    public Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)> ReqFunc; // optional
    public Action<@string, ж<Δhttp.Response>> CheckResponse;                   // optional
    public Action<@string, ж<Δhttp.Response>> EarlyCheckResponse;              // optional; pre-normalize
    public slice<any> Opts;
}

internal static Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)> reqFunc(this h12Compare tt) {
    if (tt.ReqFunc == nil) {
        return ((Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>)(Δhttp.Get));
    }
    return tt.ReqFunc;
}

internal static void run(this h12Compare tt, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        setParallel(t);
        var cst1 = newClientServerTest(new http_test_package.testing_TжTB(t), http1Mode, new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(tt.Handler)), tt.Opts.ꓸꓸꓸ);
        var cst1ʗ1 = cst1;
        defer(cst1ʗ1.close, ref ᒐ);
        var cst2 = newClientServerTest(new http_test_package.testing_TжTB(t), http2Mode, new http_test_package.http_HandlerFuncᴠΔHandler(NilSafeDelegateConversion<Δhttp.HandlerFunc, Action<Δhttp.ResponseWriter, ж<Δhttp.Request>>>(tt.Handler)), tt.Opts.ꓸꓸꓸ);
        var cst2ʗ1 = cst2;
        defer(cst2ʗ1.close, ref ᒐ);

        var (res1, err) = tt.reqFunc()(cst1.c, cst1.ts.URL);
        if (err != nil) {
            t.Errorf("HTTP/1 request: %v", err);
            return;
        }
        (var res2, err) = tt.reqFunc()(cst2.c, cst2.ts.URL);
        if (err != nil) {
            t.Errorf("HTTP/2 request: %v", err);
            return;
        }

        {
            var fn = tt.EarlyCheckResponse; if (fn != nil) {
                fn(http11ˢ, res1);
                fn(http20ˢ, res2);
            }
        }

        tt.normalizeRes(t, res1, http11ˢ);
        tt.normalizeRes(t, res2, http20ˢ);
        var (res1body, res2body) = (res1.Body, res2.Body);

        var eres1 = mostlyCopy(res1);
        var eres2 = mostlyCopy(res2);
        if (!reflect.DeepEqual(eres1.OrTypedNil(), eres2.OrTypedNil())) {
            t.Errorf("Response headers to handler differed:\nhttp/1 (%v):\n\t%#v\nhttp/2 (%v):\n\t%#v",
                cst1.ts.URL, eres1.OrTypedNil(), cst2.ts.URL, eres2.OrTypedNil());
        }
        if (!reflect.DeepEqual(res1body, res2body)) {
            t.Errorf("Response bodies to handler differed.\nhttp1: %v\nhttp2: %v\n", res1body, res2body);
        }
        {
            var fn = tt.CheckResponse; if (fn != nil) {
                (res1.Body, res2.Body) = (res1body, res2body);
                fn(http11ˢ, res1);
                fn(http20ˢ, res2);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static ж<Δhttp.Response> mostlyCopy(ж<Δhttp.Response> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    ref var c = ref heap<Δhttp.Response>(r, out var Ꮡc);
    c.Body = default;
    c.TransferEncoding = default;
    c.TLS = default;
    c.Request = default;
    return Ꮡc;
}

partial struct slurpResult {
    /*embed*/ public io_package.ReadCloser ReadCloser;
    internal slice<byte> body;
    internal error err;
}

internal static @string String(this slurpResult sr) { return fmt.Sprintf("body %q; err %v"u8, sr.body, sr.err); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dateˢ => ref dateˢᵇ;
private static readonly @string dateˢᵇ = "Date"u8;

internal static void normalizeRes(this h12Compare tt, ж<testing.T> t, ж<Δhttp.Response> Ꮡres, @string wantProto) {
    ref var res = ref Ꮡres.DerefOrNull();

    if (res.Proto == wantProto || res.Proto == "HTTP/IGNORE"u8) {
        (res.Proto, res.ProtoMajor, res.ProtoMinor) = ("", 0, 0);
    } else {
        t.Errorf("got %q response; want %q", res.Proto, wantProto);
    }
    var (slurp, err) = io.ReadAll(res.Body);

    res.Body.Close();
    res.Body = new slurpResult(
        ReadCloser: io.NopCloser(new http_test_package.bytes_ReaderжReader(bytes.NewReader(slurp))),
        body: slurp,
        err: err
    );
    foreach (var (i, v) in range(res.Header[dateˢ])) {
        res.Header[dateˢ][i] = strings.Repeat("x", len(v));
    }
    if (res.Request == nil) {
        t.Errorf("for %s, no request", wantProto);
    }
    if ((res.TLS != nil) != (wantProto == "HTTP/2.0"u8)) {
        t.Errorf("TLS set = %v; want %v", res.TLS != nil, res.TLS == nil);
    }
}

// Issue 13532
public static void TestH12_HeadContentLengthNoBody(ж<testing.T> t) {
    new h12Compare(
        ReqFunc: new Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>(((Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>)(Δhttp.Head))),
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        }
    ).run(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string smallˢ => ref smallˢᵇ;
private static readonly @string smallˢᵇ = "small"u8;

public static void TestH12_HeadContentLengthSmallBody(ж<testing.T> t) {
    new h12Compare(
        ReqFunc: new Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>(((Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>)(Δhttp.Head))),
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), smallˢ);
        }
    ).run(t);
}

public static void TestH12_HeadContentLengthLargeBody(ж<testing.T> t) {
    new h12Compare(
        ReqFunc: new Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>(((Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>)(Δhttp.Head))),
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            @string chunk = strings.Repeat("x", 512 << 10);
            for (nint i = 0; i < 10; i++) {
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), chunk);
            }
        }
    ).run(t);
}

public static void TestH12_200NoBody(ж<testing.T> t) {
    new h12Compare(Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
    }).run(t);
}

public static void TestH2_204NoBody(ж<testing.T> t) { testH12_noBody(t, 204); }
public static void TestH2_304NoBody(ж<testing.T> t) { testH12_noBody(t, 304); }
public static void TestH2_404NoBody(ж<testing.T> t) { testH12_noBody(t, 404); }

internal static void testH12_noBody(ж<testing.T> t, nint status) {
    new h12Compare(Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.WriteHeader(status);
    }
    ).run(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string smallBodyˢ => ref smallBodyˢᵇ;
private static readonly @string smallBodyˢᵇ = "small body"u8;

public static void TestH12_SmallBody(ж<testing.T> t) {
    new h12Compare(Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), smallBodyˢ);
    }
    ).run(t);
}

public static void TestH12_ExplicitContentLength(ж<testing.T> t) {
    new h12Compare(Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(contentLengthˢ, "3");
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), fooˢ);
    }
    ).run(t);
}

public static void TestH12_FlushBeforeBody(ж<testing.T> t) {
    new h12Compare(Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w._<Flusher>().Flush();
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), fooˢ);
    }
    ).run(t);
}

public static void TestH12_FlushMidBody(ж<testing.T> t) {
    new h12Compare(Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), fooˢ);
        w._<Flusher>().Flush();
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), barˢ);
    }
    ).run(t);
}

public static void TestH12_Head_ExplicitLen(ж<testing.T> t) {
    new h12Compare(
        ReqFunc: new Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>(((Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>)(Δhttp.Head))),
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if (r.Method != "HEAD"u8) {
                t.Errorf("unexpected method %q", r.Method);
            }
            w.Header().Set(contentLengthˢ, "1235");
        }
    ).run(t);
}

public static void TestH12_Head_ImplicitLen(ж<testing.T> t) {
    new h12Compare(
        ReqFunc: new Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>(((Func<ж<Δhttp.Client>, @string, (ж<Δhttp.Response>, error)>)(Δhttp.Head))),
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            if (r.Method != "HEAD"u8) {
                t.Errorf("unexpected method %q", r.Method);
            }
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), fooˢ);
        }
    ).run(t);
}

public static void TestH12_HandlerWritesTooLittle(ж<testing.T> t) {
    new h12Compare(
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(contentLengthˢ, "3");
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), "12"); // one byte short
        },
        CheckResponse: (@string proto, ж<Δhttp.Response> res) => {
            var (sr, ok) = res.Body._<slurpResult>(ᐧ);
            if (!ok) {
                t.Errorf("%s body is %T; want slurpResult", proto, res.Body);
                return;
            }
            if (!AreEqual(sr.err, io.ErrUnexpectedEOF)) {
                t.Errorf("%s read error = %v; want io.ErrUnexpectedEOF", proto, sr.err);
            }
            if (((sstring)sr.body) != "12"u8) {
                t.Errorf("%s body = %q; want %q", proto, sr.body, (@string)"12");
            }
        }
    ).run(t);
}

// Tests that the HTTP/1 and HTTP/2 servers prevent handlers from
// writing more than they declared. This test does not test whether
// the transport deals with too much data, though, since the server
// doesn't make it possible to send bogus data. For those tests, see
// transport_test.go (for HTTP/1) or x/net/http2/transport_test.go
// (for HTTP/2).
public static void TestHandlerWritesTooMuch(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testHandlerWritesTooMuch(Δp0, Δp1)); }
internal static void testHandlerWritesTooMuch(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var wantBody = slice<byte>("123"u8);
        var wantBodyʗ1 = wantBody;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var rc = NewResponseController(w);
            w.Header().Set(contentLengthˢ, fmt.Sprintf("%v"u8, len(wantBodyʗ1)));
            rc.Flush();
            w.Write(wantBodyʗ1);
            rc.Flush();
            var (n, errΔ1) = io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), "x"); // too many
            if (errΔ1 == nil) {
                errΔ1 = rc.Flush();
            }
            // TODO: Check that this is ErrContentLength, not just any error.
            if (errΔ1 == nil) {
                t.Errorf("for proto %q, final write = %v, %v; want _, some error", r.Proto, n, errΔ1);
            }
        })));

        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        var (gotBody, _) = io.ReadAll(res.Body);
        if (!bytes.Equal(gotBody, wantBody)) {
            t.Fatalf("got response body: %q; want %q", gotBody, wantBody);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string acceptEncodingˢ => ref acceptEncodingˢᵇ;
private static readonly @string acceptEncodingˢᵇ = "Accept-Encoding"u8;
internal static ref readonly @string gzipˢ => ref gzipˢᵇ;
private static readonly @string gzipˢᵇ = "gzip"u8;
internal static ref readonly @string iAmSomeGzippedContentGoˢ => ref iAmSomeGzippedContentGoˢᵇ;
private static readonly @string iAmSomeGzippedContentGoˢᵇ = "I am some gzipped content. Go go go go go go go go go go go go should compress well."u8;

// Verify that both our HTTP/1 and HTTP/2 request and auto-decompress gzip.
// Some hosts send gzip even if you don't ask for it; see golang.org/issue/13298
public static void TestH12_AutoGzip(ж<testing.T> t) {
    new h12Compare(
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            @string ae = r.Header.Get(acceptEncodingˢ); if (ae != "gzip"u8) {
                t.Errorf("%s Accept-Encoding = %q; want gzip", r.Proto, ae);
            }
            w.Header().Set(contentEncodingˢ, gzipˢ);
            var gz = gzip.NewWriter(new http_test_package.http_ResponseWriterᴠWriter(w));
            io.WriteString(new http_test_package.gzip_WriterжWriter(gz), iAmSomeGzippedContentGoˢ);
            gz.Close();
        }
    ).run(t);
}

public static void TestH12_AutoGzip_Disabled(ж<testing.T> t) {
    new h12Compare(
        Opts: [
            (ж<Δhttp.Transport> tr) => {
                tr.DisableCompression = true;
            }
        ],
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%q"u8, r.Header[acceptEncodingˢ]);
            @string ae = r.Header.Get(acceptEncodingˢ); if (ae != ""u8) {
                t.Errorf("%s Accept-Encoding = %q; want empty", r.Proto, ae);
            }
        }
    ).run(t);
}

// Test304Responses verifies that 304s don't declare that they're
// chunking in their response headers and aren't allowed to produce
// output.
public static void Test304Responses(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => test304Responses(Δp0, Δp1)); }
internal static void test304Responses(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.WriteHeader(StatusNotModified);
            var (_, errΔ1) = w.Write(slice<byte>("illegal body"u8));
            if (!AreEqual(errΔ1, ErrBodyNotAllowed)) {
                t.Errorf("on Write, expected ErrBodyNotAllowed, got %v", errΔ1);
            }
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);
        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        if (len(res.TransferEncoding) > 0) {
            t.Errorf("expected no TransferEncoding; got %v", res.TransferEncoding);
        }
        (var body, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Error(err);
        }
        if (len(body) > 0) {
            t.Errorf("got unexpected body %q", (@string)body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string htmlBodyHiBodyHtmlˢ => ref htmlBodyHiBodyHtmlˢᵇ;
private static readonly @string htmlBodyHiBodyHtmlˢᵇ = "<html><body>hi</body></html>"u8;

public static void TestH12_ServerEmptyContentLength(ж<testing.T> t) {
    new h12Compare(
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header()[contentTypeˢ] = [""];
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), htmlBodyHiBodyHtmlˢ);
        }
    ).run(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fourˢ => ref fourˢᵇ;
private static readonly @string fourˢᵇ = "FOUR"u8;

public static void TestH12_RequestContentLength_Known_NonZero(ж<testing.T> t) {
    h12requestContentLength(t, () => new http_test_package.strings_ReaderжReader(strings.NewReader(fourˢ)), 4);
}

public static void TestH12_RequestContentLength_Known_Zero(ж<testing.T> t) {
    h12requestContentLength(t, () => default, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stuffˢ => ref stuffˢᵇ;
private static readonly @string stuffˢᵇ = "Stuff"u8;

internal partial struct TestH12_RequestContentLength_Unknown_type /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

public static void TestH12_RequestContentLength_Unknown(ж<testing.T> t) {
    h12requestContentLength(t, () => new TestH12_RequestContentLength_Unknown_type(new http_test_package.strings_ReaderжReader(strings.NewReader(stuffˢ))), -1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gotLengthˢ => ref gotLengthˢᵇ;
private static readonly @string gotLengthˢᵇ = "Got-Length"u8;

internal static void h12requestContentLength(ж<testing.T> t, Func<io.Reader> bodyfn, int64 wantLen) {
    new h12Compare(
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.Header().Set(gotLengthˢ, fmt.Sprint(r.ContentLength));
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Req.ContentLength=%v"u8, r.ContentLength);
        },
        ReqFunc: (ж<Δhttp.Client> c, @string url) => c.Post(url, textPlainˢ, bodyfn()),
        CheckResponse: (@string proto, ж<Δhttp.Response> res) => {
            @string got = res.Header.Get(gotLengthˢ);
            @string want = fmt.Sprint(wantLen); if (got != want) {
                t.Errorf("Proto %q got length %q; want %q", proto, got, want);
            }
        }
    ).run(t);
}

// Tests that closing the Request.Cancel channel also while still
// reading the response body. Issue 13159.
public static void TestCancelRequestMidBody(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testCancelRequestMidBody(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ2 => ref helloˢ2ᵇ;
private static readonly @string helloˢ2ᵇ = "Hello"u8;
internal static ref readonly @string worldˢ => ref worldˢᵇ;
private static readonly @string worldˢᵇ = ", world."u8;

internal static void testCancelRequestMidBody(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var unblock = new channel<bool>(0);
        var didFlush = new channel<bool>(1);
        var didFlushʗ1 = didFlush;
        var unblockʗ1 = unblock;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloˢ2);
            w._<Flusher>().Flush();
            didFlushʗ1.ᐸꟷ(true);
            ᐸꟷ(unblockʗ1);
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), worldˢ);
        })));
        defer(ᴛ1 => builtin.close(ᴛ1), unblock, ref ᒐ);

        var (req, _) = NewRequest(getˢ2, cst.ts.URL, default);
        var cancel = new channel<EmptyStruct>(0);
        req.Cancel = cancel.AsRecvOnly();

        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        ᐸꟷ(didFlush);

        // Read a bit before we cancel. (Issue 13626)
        // We should have "Hello" at least sitting there.
        var firstRead = new slice<byte>(10);
        (var n, err) = res.Body.Read(firstRead);
        if (err != nil) {
            t.Fatal(err);
        }
        firstRead = firstRead.slice(0, n);

        builtin.close(cancel);

        (var rest, err) = io.ReadAll(res.Body);
        @string all = ((sstring)firstRead) + ((sstring)rest);
        if (all != "Hello"u8) {
            t.Errorf("Read %q (%q + %q); want Hello", all, firstRead, rest);
        }
        if (!AreEqual(err, http_internal_test_package.ExportErrRequestCanceled)) {
            t.Errorf("ReadAll error = %v; want %v", err, http_internal_test_package.ExportErrRequestCanceled);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Tests that clients can send trailers to a server and that the server can read them.
public static void TestTrailersClientToServer(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testTrailersClientToServer(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilTrailerˢ => ref nilTrailerˢᵇ;
private static readonly @string nilTrailerˢᵇ = "nil Trailer"u8;
internal static ref readonly @string clientTrailerAˢ => ref clientTrailerAˢᵇ;
private static readonly @string clientTrailerAˢᵇ = "Client-Trailer-A"u8;
internal static ref readonly @string clientTrailerBˢ => ref clientTrailerBˢᵇ;
private static readonly @string clientTrailerBˢᵇ = "Client-Trailer-B"u8;
internal static ref readonly @string declClientTrailerAClientˢ => ref declClientTrailerAClientˢᵇ;
private static readonly @string declClientTrailerAClientˢᵇ = "decl: [Client-Trailer-A Client-Trailer-B], vals: valuea, valueb"u8;

internal static void testTrailersClientToServer(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        var (slurp, errΔ1) = io.ReadAll(r.Body);
        if (errΔ1 != nil) {
            t.Errorf("Server reading request body: %v", errΔ1);
        }
        if (((sstring)slurp) != "foo"u8) {
            t.Errorf("Server read request body %q; want foo", slurp);
        }
        if (r.Trailer == nil) {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), nilTrailerˢ);
        } else {
            var decl = slices.Sorted(maps.Keys<httpꓸHeader, @string, slice<@string>>(r.Trailer));
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "decl: %v, vals: %s, %s"u8,
                decl,
                r.Trailer.Get(clientTrailerAˢ),
                r.Trailer.Get(clientTrailerBˢ));
        }
    })));

    ref var req = ref heap<ж<Δhttp.Request>>(out var Ꮡreq);
    (req, _) = NewRequest(postˢ, cst.ts.URL, io.MultiReader(
        new http_test_package.eofReaderFuncᴠReader(new eofReaderFunc(() => {
            Ꮡreq.ValueSlot.Trailer[clientTrailerAˢ] = ["valuea"];
        })),
        new http_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)),
        new http_test_package.eofReaderFuncᴠReader(new eofReaderFunc(() => {
            Ꮡreq.ValueSlot.Trailer[clientTrailerBˢ] = ["valueb"];
        }))));
    req.Trailer = new httpꓸHeader(new map<@string, slice<@string>>{
        ["Client-Trailer-A"] = default, //  to be set later
        ["Client-Trailer-B"] = default  //  to be set later
    });
    req.ContentLength = -1;
    var (res, err) = cst.c.Do(req);
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ2 = wantBody(res, err, declClientTrailerAClientˢ); if (errΔ2 != nil) {
        t.Error(errΔ2);
    }
}

// Tests that servers send trailers to a client and that the client can read them.
public static void TestTrailersServerToClient(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testTrailersServerToClient(tΔ1, mode, false);
    });
}
public static void TestTrailersServerToClientFlush(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testTrailersServerToClient(tΔ1, mode, true);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trailerˢ => ref trailerˢᵇ;
private static readonly @string trailerˢᵇ = "Trailer"u8;
internal static ref readonly @string serverTrailerAServerˢ => ref serverTrailerAServerˢᵇ;
private static readonly @string serverTrailerAServerˢᵇ = "Server-Trailer-A, Server-Trailer-B"u8;
internal static ref readonly @string serverTrailerCˢ => ref serverTrailerCˢᵇ;
private static readonly @string serverTrailerCˢᵇ = "Server-Trailer-C"u8;
internal static ref readonly @string serverTrailerAˢ => ref serverTrailerAˢᵇ;
private static readonly @string serverTrailerAˢᵇ = "Server-Trailer-A"u8;
internal static ref readonly @string valueaˢ => ref valueaˢᵇ;
private static readonly @string valueaˢᵇ = "valuea"u8;
internal static ref readonly @string valuecˢ => ref valuecˢᵇ;
private static readonly @string valuecˢᵇ = "valuec"u8;
internal static ref readonly @string serverTrailerNotDeclaredˢ => ref serverTrailerNotDeclaredˢᵇ;
private static readonly @string serverTrailerNotDeclaredˢᵇ = "Server-Trailer-NotDeclared"u8;
internal static ref readonly @string shouldBeOmittedˢ => ref shouldBeOmittedˢᵇ;
private static readonly @string shouldBeOmittedˢᵇ = "should be omitted"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bodyᶜ1 => ref bodyᶜ1ᵇ;
private static readonly @string bodyᶜ1ᵇ = "Some body"u8;

internal static void testTrailersServerToClient(ж<testing.T> t, testMode mode, bool flush) {
    @string body = bodyᶜ1;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(trailerˢ, serverTrailerAServerˢ);
        w.Header().Add(trailerˢ, serverTrailerCˢ);

        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), body);
        if (flush) {
            w._<Flusher>().Flush();
        }

        // How handlers set Trailers: declare it ahead of time
        // with the Trailer header, and then mutate the
        // Header() of those values later, after the response
        // has been written (we wrote to w above).
        w.Header().Set(serverTrailerAˢ, valueaˢ);
        w.Header().Set(serverTrailerCˢ, valuecˢ); // skipping B
        w.Header().Set(serverTrailerNotDeclaredˢ, shouldBeOmittedˢ);
    })));

    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }

    var wantHeader = new httpꓸHeader(new map<@string, slice<@string>>{
        ["Content-Type"] = ["text/plain; charset=utf-8"]
    });
    nint wantLen = -1;
    if (mode == http2Mode && !flush) {
        // In HTTP/1.1, any use of trailers forces HTTP/1.1
        // chunking and a flush at the first write. That's
        // unnecessary with HTTP/2's framing, so the server
        // is able to calculate the length while still sending
        // trailers afterwards.
        wantLen = len(body);
        wantHeader[contentLengthˢ] = slice<@string>([fmt.Sprint(wantLen)]);
    }
    if (res.ContentLength != (int64)wantLen) {
        t.Errorf("ContentLength = %v; want %v", res.ContentLength, wantLen);
    }

    delete(res.Header, "Date"u8); // irrelevant for test
    if (!reflect.DeepEqual(res.Header, wantHeader)) {
        t.Errorf("Header = %v; want %v", res.Header, wantHeader);
    }

    {
        var (got, want) = (res.Trailer, (new httpꓸHeader(new map<@string, slice<@string>>{
            ["Server-Trailer-A"] = default,
            ["Server-Trailer-B"] = default,
            ["Server-Trailer-C"] = default
        }))); if (!reflect.DeepEqual(got, want)) {
            t.Errorf("Trailer before body read = %v; want %v", got, want);
        }
    }

    var errΔ1 = wantBody(res, default, body); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }

    {
        var (got, want) = (res.Trailer, (new httpꓸHeader(new map<@string, slice<@string>>{
            ["Server-Trailer-A"] = ["valuea"],
            ["Server-Trailer-B"] = default,
            ["Server-Trailer-C"] = ["valuec"]
        }))); if (!reflect.DeepEqual(got, want)) {
            t.Errorf("Trailer after body read = %v; want %v", got, want);
        }
    }
}

// Don't allow a Body.Read after Body.Close. Issue 13648.
public static void TestResponseBodyReadAfterClose(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testResponseBodyReadAfterClose(Δp0, Δp1)); }
// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bodyᶜ2 => ref bodyᶜ2ᵇ;
private static readonly @string bodyᶜ2ᵇ = "Some body"u8;

internal static void testResponseBodyReadAfterClose(ж<testing.T> t, testMode mode) {
    @string body = bodyᶜ2;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), body);
    })));
    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    res.Body.Close();
    (var data, err) = io.ReadAll(res.Body);
    if (len(data) != 0 || err == nil) {
        t.Fatalf("ReadAll returned %q, %v; want error", data, err);
    }
}

public static void TestConcurrentReadWriteReqBody(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testConcurrentReadWriteReqBody(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string expectˢ => ref expectˢᵇ;
private static readonly @string expectˢᵇ = "Expect"u8;
internal static ref readonly @string continueˢ => ref continueˢᵇ;
private static readonly @string continueˢᵇ = "100-continue"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string reqBodyᶜ => ref reqBodyᶜᵇ;
private static readonly @string reqBodyᶜᵇ = "some request body"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string resBodyᶜ => ref resBodyᶜᵇ;
private static readonly @string resBodyᶜᵇ = "some response body"u8;

internal static void testConcurrentReadWriteReqBody(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string reqBody = reqBodyᶜ;
        @string resBody = resBodyᶜ;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc([MethodImpl(MethodImplOptions.NoInlining)] (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(2);
            var didRead = new channel<bool>(1);
            // Read in one goroutine.
            var didReadʗ1 = didRead;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (dataΔ1, errΔ1) = io.ReadAll(r.Body);
                    if (((sstring)dataΔ1) != reqBody) {
                        t.Errorf("Handler read %q; want %q", dataΔ1, reqBody);
                    }
                    if (errΔ1 != nil) {
                        t.Errorf("Handler Read: %v", errΔ1);
                    }
                    didReadʗ1.ᐸꟷ(true);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            // Write in another goroutine.
            var didReadʗ2 = didRead;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    if (mode != http2Mode) {
                        // our HTTP/1 implementation intentionally
                        // doesn't permit writes during read (mostly
                        // due to it being undefined); if that is ever
                        // relaxed, change this.
                        ᐸꟷ(didReadʗ2);
                    }
                    io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), resBody);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            Ꮡwg.Wait();
        })));
        var (req, _) = NewRequest(postˢ, cst.ts.URL, new http_test_package.strings_ReaderжReader(strings.NewReader(reqBody)));
        req.Header.Add(expectˢ, continueˢ); // just to complicate things
        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        (var data, err) = io.ReadAll(res.Body);
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (err != nil) {
            t.Fatal(err);
        }
        if (((sstring)data) != resBody) {
            t.Errorf("read %q; want %q", data, resBody);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestConnectRequest(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testConnectRequest(Δp0, Δp1)); }
internal partial struct testConnectRequest_tests /*dyn*/ {
    internal ж<Δhttp.Request> req;
    internal @string want;
}

internal static void testConnectRequest(ж<testing.T> t, testMode mode) {
    var gotc = new channel<ж<Δhttp.Request>>(1);
    var gotcʗ1 = gotc;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        gotcʗ1.ᐸꟷ(r);
    })));

    var (u, err) = url.Parse(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }

    var tests = slice<testConnectRequest_tests>([
        new(
            req: Ꮡ(new Request(
                Method: "CONNECT",
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                URL: u
            )),
            want: u.Host
        ),
        new(
            req: Ꮡ(new Request(
                Method: "CONNECT",
                Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
                URL: u,
                Host: "example.com:123"
            )),
            want: "example.com:123"
        )
    ]);

    foreach (var (i, tt) in range(tests)) {
        var (res, errΔ1) = cst.c.Do(tt.req);
        if (errΔ1 != nil) {
            t.Errorf("%d. RoundTrip = %v", i, errΔ1);
            continue;
        }
        res.Body.Close();
        var req = ᐸꟷ(gotc);
        if (req.Method != "CONNECT"u8) {
            t.Errorf("method = %q; want CONNECT", req.Method);
        }
        if (req.Host != tt.want) {
            t.Errorf("Host = %q; want %q", req.Host, tt.want);
        }
        if (req.URL.Host != tt.want) {
            t.Errorf("URL.Host = %q; want %q", req.URL.Host, tt.want);
        }
    }
}

public static void TestTransportUserAgent(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testTransportUserAgent(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goHttpClient11ˢ => ref goHttpClient11ˢᵇ;
private static readonly @string goHttpClient11ˢᵇ = @"[""Go-http-client/1.1""]"u8;
internal static ref readonly @string goHttpClient20ˢ => ref goHttpClient20ˢᵇ;
private static readonly @string goHttpClient20ˢᵇ = @"[""Go-http-client/2.0""]"u8;
internal static ref readonly @string foo123ˢ => ref foo123ˢᵇ;
private static readonly @string foo123ˢᵇ = "foo/1.2.3"u8;

internal partial struct testTransportUserAgent_tests /*dyn*/ {
    internal Action<ж<Δhttp.Request>> setup;
    internal @string want;
}

internal static void testTransportUserAgent(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "%q"u8, r.Header[userAgentˢ2]);
    })));

    @string either(@string a, @string b) {
        if (mode == http2Mode) {
            return b;
        }
        return a;
    }

    var tests = slice<testTransportUserAgent_tests>([
        new(
            (ж<Δhttp.Request> r) => {
            },
            either(goHttpClient11ˢ, goHttpClient20ˢ)
        ),
        new(
            (ж<Δhttp.Request> r) => {
                r.Header.Set(userAgentˢ2, foo123ˢ);
            },
            @"[""foo/1.2.3""]"
        ),
        new(
            (ж<Δhttp.Request> r) => {
                r.Header[userAgentˢ2] = ["single", "or", "multiple"];
            },
            @"[""single""]"
        ),
        new(
            (ж<Δhttp.Request> r) => {
                r.Header.Set(userAgentˢ2, ""u8);
            },
            @"[]"
        ),
        new(
            (ж<Δhttp.Request> r) => {
                r.Header[userAgentˢ2] = default;
            },
            @"[]"
        )
    ]);
    foreach (var (i, tt) in range(tests)) {
        var (req, _) = NewRequest(getˢ2, cst.ts.URL, default);
        tt.setup(req);
        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Errorf("%d. RoundTrip = %v", i, err);
            continue;
        }
        (var slurp, err) = io.ReadAll(res.Body);
        res.Body.Close();
        if (err != nil) {
            t.Errorf("%d. read body = %v", i, err);
            continue;
        }
        if (((sstring)slurp) != tt.want) {
            t.Errorf("%d. body mismatch.\n got: %s\nwant: %s\n", i, slurp, tt.want);
        }
    }
}

public static void TestStarRequestMethod(ж<testing.T> t) {
    foreach (var (_, method) in range(slice<@string>(["FOO", "OPTIONS"]))) {
        t.Run(method, (ж<testing.T> tΔ1) => {
            run<TжTBRun>(tΔ1, (TжTBRun tΔ2Δp, testMode mode) => {
                var tΔ2 = (ж<testing.T>)tΔ2Δp;
                testStarRequest(tΔ2, method, mode);
            });
        });
    }
}
internal static void testStarRequest(ж<testing.T> t, @string method, testMode mode) {
    var gotc = new channel<ж<Δhttp.Request>>(1);
    var gotcʗ1 = gotc;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(fooˢ, barˢ);
        gotcʗ1.ᐸꟷ(r);
        w._<Flusher>().Flush();
    })));

    var (u, err) = url.Parse(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    u.Path = "*";

    var req = Ꮡ(new Request(
        Method: method,
        Header: new httpꓸHeader(new map<@string, slice<@string>>{}),
        URL: u
    ));

    (var res, err) = cst.c.Do(req);
    if (err != nil) {
        t.Fatalf("RoundTrip = %v", err);
    }
    res.Body.Close();

    @string wantFoo = barˢ;
    var wantLen = (int64)(-1);
    if (method == "OPTIONS"u8) {
        wantFoo = ""u8;
        wantLen = 0;
    }
    if (res.StatusCode != 200) {
        t.Errorf("status code = %v; want %d", res.Status, (nint)200);
    }
    if (res.ContentLength != wantLen) {
        t.Errorf("content length = %v; want %d", res.ContentLength, wantLen);
    }
    @string got = res.Header.Get(fooˢ); if (got != wantFoo) {
        t.Errorf("response \"foo\" header = %q; want %q", got, wantFoo);
    }
    var selᴛ8 = gotc;
    switch (trySelect(ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
    case 0 when selᴛ8.ꟷᐳ(out req): {
        break;
    }
    default: {
        req = default;
        break;
    }}
    if (req == nil) {
        if (method != "OPTIONS"u8) {
            t.Fatalf("handler never got request");
        }
        return;
    }
    if (req.Method != method) {
        t.Errorf("method = %q; want %q", req.Method, method);
    }
    if (req.URL.Path != "*"u8) {
        t.Errorf("URL.Path = %q; want *", req.URL.Path);
    }
    if (req.RequestURI != "*"u8) {
        t.Errorf("RequestURI = %q; want *", req.RequestURI);
    }
}

// Issue 13957
public static void TestTransportDiscardsUnneededConns(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testTransportDiscardsUnneededConns(Δp0, Δp1), slice<testMode>([http2Mode]));
}
internal static partial void testTransportDiscardsUnneededConns(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Hello, %v"u8, r.RemoteAddr);
        })));
        var cstʗ1 = cst;
        defer(cstʗ1.close, ref ᒐ);

        ref var numOpen = ref heap(new int32(), out var ᏑnumOpen); // atomic
        ref var numClose = ref heap(new int32(), out var ᏑnumClose);

        var tlsConfig = Ꮡ(new tls.Config(InsecureSkipVerify: true));
            var tlsConfigʗ1 = tlsConfig;
        var tr = Ꮡ(new Transport(
            TLSClientConfig: tlsConfig,
            DialTLS: (@string _, @string addr) => {
                time.Sleep(10 * time.Millisecond);
                var (rc, err) = net.Dial(tcpˢ, addr);
                if (err != nil) {
                    return (default, err);
                }
                atomic.AddInt32(ᏑnumOpen, 1);
                var cΔ1 = new noteCloseConn(rc, () => {
                    atomic.AddInt32(ᏑnumClose, 1);
                });
                return (new tls.ConnжConn(tls.Client(cΔ1, tlsConfigʗ1)), default);
            }
        ));
        {
            var err = http_internal_test_package.ExportHttp2ConfigureTransport(tr); if (err != nil) {
                t.Fatal(err);
            }
        }
        var trʗ1 = tr;
        defer(trʗ1.CloseIdleConnections, ref ᒐ);

        var c = Ꮡ(new Client(Transport: new Δhttp.TransportжRoundTripper(tr)));

        const nint N = 10;
        var gotBody = new channel<@string>(N);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        for (nint i = 0; i < N; i++) {
            Ꮡwg.Add(1);
            var cʗ1 = c;
            var cstʗ2 = cst;
            var gotBodyʗ1 = gotBody;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (resp, err) = cʗ1.Get(cstʗ2.ts.URL);
                    if (err != nil) {
                        // Try to work around spurious connection reset on loaded system.
                        // See golang.org/issue/33585 and golang.org/issue/36797.
                        time.Sleep(10 * time.Millisecond);
                        (resp, err) = cʗ1.Get(cstʗ2.ts.URL);
                        if (err != nil) {
                            t.Errorf("Get: %v", err);
                            return;
                        }
                    }
                    var respʗ1 = resp;
                    defer(ᴛ0 => ᴛ0.Close(), respʗ1.Body, ref ᒐ);
                    (var slurp, err) = io.ReadAll(resp.Body);
                    if (err != nil) {
                        t.Error(err);
                    }
                    gotBodyʗ1.ᐸꟷ(((@string)slurp));
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
        builtin.close(gotBody);

        @string last = default;
        foreach (var got in gotBody) {
            if (last == ""u8) {
                last = got;
                continue;
            }
            if (got != last) {
                t.Errorf("Response body changed: %q -> %q", last, got);
            }
        }

        int32 open = default;
        int32 closeΔ1 = default;
        for (nint i = 0; i < 150; i++) {
            (open, closeΔ1) = (atomic.LoadInt32(ᏑnumOpen), atomic.LoadInt32(ᏑnumClose));
            if (open < 1) {
                t.Fatalf("open = %d; want at least", open);
            }
            if (closeΔ1 == open - 1) {
                // Success
                return;
            }
            time.Sleep(10 * time.Millisecond);
        }
        t.Errorf("%d connections opened, %d closed; want %d to close", open, closeΔ1, open - 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bodyˢ2 => ref bodyˢ2ᵇ;
private static readonly @string bodyˢ2ᵇ = "Body"u8;
internal static ref readonly @string noBodyˢ => ref noBodyˢᵇ;
private static readonly @string noBodyˢᵇ = "NoBody"u8;

// tests that Transport doesn't retain a pointer to the provided request.
public static void TestTransportGCRequest(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(bodyˢ2, (ж<testing.T> tΔ2) => {
            testTransportGCRequest(tΔ2, mode, true);
        });
        tΔ1.Run(noBodyˢ, (ж<testing.T> tΔ3) => {
            testTransportGCRequest(tΔ3, mode, false);
        });
    });
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ3 => ref helloˢ3ᵇ;
private static readonly @string helloˢ3ᵇ = "Hello."u8;

internal static void testTransportGCRequest(ж<testing.T> t, testMode mode, bool body) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        io.ReadAll(r.Body);
        if (body) {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), helloˢ3);
        }
    })));

    var didGC = new channel<EmptyStruct>(0);
    var cstʗ1 = cst;
    var didGCʗ1 = didGC;
    func(() => {
        var bodyΔ1 = strings.NewReader(someBodyˢ);
        var (req, _) = NewRequest(postˢ, cstʗ1.ts.URL, new http_test_package.strings_ReaderжReader(bodyΔ1));
        runtime.AddCleanup(req, (channel<EmptyStruct> ch) => {
            builtin.close(ch);
        }, didGCʗ1);
        var (res, err) = cstʗ1.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var (_, errΔ1) = io.ReadAll(res.Body); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var errΔ2 = res.Body.Close(); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    })();
    for (;;) {
        var selᴛ9 = didGC;
        var selᴛ10 = time.After(1 * time.Millisecond);
        switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
        case 0 when selᴛ9.ꟷᐳ(out _): {
            return;
        }
        case 1 when selᴛ10.ꟷᐳ(out _): {
            runtime.GC();
            break;
        }}
    }
}

public static void TestTransportRejectsInvalidHeaders(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testTransportRejectsInvalidHeaders(Δp0, Δp1)); }
internal partial struct testTransportRejectsInvalidHeaders_tests /*dyn*/ {
    internal @string key, val;
    internal bool ok;
}

internal static void testTransportRejectsInvalidHeaders(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        fmt.Fprintf(new http_test_package.http_ResponseWriterᴠWriter(w), "Handler saw headers: %q"u8, r.Header);
    })), (optQuietLog).OrTypedNilFunc());
    cst.tr.DisableKeepAlives = true;

    var tests = slice<testTransportRejectsInvalidHeaders_tests>([
        new("Foo", "capital-key", true),                                                      // verify h2 allows capital keys
        new("Foo", ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0x00, 0x62, 0x61, 0x72})), false), // \x00 byte in value not allowed
        new("Foo", "two\nlines", false),                                                      // \n byte in value not allowed
        new("bogus\nkey", "v", false),                                                        // \n byte also not allowed in key
        new("A space", "v", false),                                                           // spaces in keys not allowed
        new("имя", "v", false),                                                               // key must be ascii
        new("name", "валю", true),                                                            // value may be non-ascii
        new("", "v", false),                                                                  // key must be non-empty
        new("k", "", true)                                                                    // value may be empty
    ]);
    foreach (var (_, tt) in range(tests)) {
        var dialedc = new channel<bool>(1);
        var dialedcʗ1 = dialedc;
        cst.tr.Dial = (@string netw, @string addr) => {
            dialedcʗ1.ᐸꟷ(true);
            return net.Dial(netw, addr);
        };
        var (req, _) = NewRequest(getˢ2, cst.ts.URL, default);
        req.Header[tt.key] = [tt.val];
        var (res, err) = cst.c.Do(req);
        slice<byte> body = default;
        if (err == nil) {
            (body, _) = io.ReadAll(res.Body);
            res.Body.Close();
        }
        bool dialed = default;
        var selᴛ11 = dialedc;
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out _): {
            dialed = true;
            break;
        }
        default: {
            break;
        }}

        if (!tt.ok && dialed) {
            t.Errorf("For key %q, value %q, transport dialed. Expected local failure. Response was: (%v, %v)\nServer replied with: %s", tt.key, tt.val, res.OrTypedNil(), err, body);
        } else if ((err == nil) != tt.ok) {
            t.Errorf("For key %q, value %q; got err = %v; want ok=%v", tt.key, tt.val, err, tt.ok);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string boomˢ => ref boomˢᵇ;
private static readonly @string boomˢᵇ = "boom"u8;
internal static ref readonly @string nilˢ2 => ref nilˢ2ᵇ;
private static readonly @string nilˢ2ᵇ = "nil"u8;
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string panicnil1ˢ => ref panicnil1ˢᵇ;
private static readonly @string panicnil1ˢᵇ = "panicnil=1"u8;
internal static ref readonly @string errAbortHandlerˢ => ref errAbortHandlerˢᵇ;
private static readonly @string errAbortHandlerˢᵇ = "ErrAbortHandler"u8;

public static void TestInterruptWithPanic(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        tΔ1.Run(boomˢ, (ж<testing.T> tΔ2) => {
            testInterruptWithPanic(tΔ2, mode, boomˢ);
        });
        tΔ1.Run(nilˢ2, (ж<testing.T> tΔ3) => {
            tΔ3.Setenv(godebugˢ, panicnil1ˢ);
            testInterruptWithPanic(tΔ3, mode, default);
        });
        tΔ1.Run(errAbortHandlerˢ, (ж<testing.T> tΔ4) => {
            testInterruptWithPanic(tΔ4, mode, ErrAbortHandler);
        });
    }, testNotParallel);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createdByˢ => ref createdByˢᵇ;
private static readonly @string createdByˢᵇ = "created by "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string msgᶜ => ref msgᶜᵇ;
private static readonly @string msgᶜᵇ = "hello"u8;

internal static void testInterruptWithPanic(ж<testing.T> t, testMode mode, any panicValue) {
    GoFrame ᒐ = default;
    try {
        @string msg = msgᶜ;

        var testDone = new channel<EmptyStruct>(0);
        defer(ᴛ1 => builtin.close(ᴛ1), testDone, ref ᒐ);

        ref var errorLog = ref heap(new lockedBytesBuffer(), out var ᏑerrorLog);
        var gotHeaders = new channel<bool>(1);
        var gotHeadersʗ1 = gotHeaders;
        var testDoneʗ1 = testDone;

        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), msg);
            w._<Flusher>().Flush();

            var selᴛ12 = gotHeadersʗ1;
            var selᴛ13 = testDoneʗ1;
            switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
            case 0 when selᴛ12.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ13.ꟷᐳ(out _): {
                break;
            }}
            throw panic(panicValue);
        })), (ж<httptest.Server> ts) => {
            ts.Config.ErrorLog = log.New(new http_test_package.lockedBytesBufferжWriter(ᏑerrorLog), ""u8, 0);
        });
        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        gotHeaders.ᐸꟷ(true);
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var slurp, err) = io.ReadAll(res.Body);
        if (((sstring)slurp) != msg) {
            t.Errorf("client read %q; want %q", slurp, msg);
        }
        if (err == nil) {
            t.Errorf("client read all successfully; want some error");
        }
        @string logOutput() {
            GoFrame ᒐ = default;
            try {
                ᏑerrorLog.of(lockedBytesBuffer.ᏑMutex).Lock();
                defer(ᏑerrorLog.of(lockedBytesBuffer.ᏑMutex).Unlock, ref ᒐ);
                return ᏑerrorLog.of(lockedBytesBuffer.ᏑBuffer).String();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        }
        var wantStackLogged = panicValue != nil && !AreEqual(panicValue, ErrAbortHandler);

        var logOutputʗ1 = logOutput;
        waitCondition(new http_test_package.testing_TжTB(t), 10 * time.Millisecond, (time.Duration d) => {
            @string gotLog = logOutputʗ1();
            if (!wantStackLogged) {
                if (gotLog == ""u8) {
                    return true;
                }
                t.Fatalf("want no log output; got: %s", gotLog);
            }
            if (gotLog == ""u8) {
                if (d > 0) {
                    t.Logf("wanted a stack trace logged; got nothing after %v", d);
                }
                return false;
            }
            if (!strings.Contains(gotLog, createdByˢ) && strings.Count(gotLog, "\n") < 6) {
                if (d > 0) {
                    t.Logf("output doesn't look like a panic stack trace after %v. Got: %s", d, gotLog);
                }
                return false;
            }
            return true;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct lockedBytesBuffer {
    /*embed*/ public sync_package.Mutex Mutex;
    /*embed*/ public bytes_package.Buffer Buffer;
}

internal static (nint, error) Write(this ж<lockedBytesBuffer> Ꮡb, slice<byte> p) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        Ꮡb.of(lockedBytesBuffer.ᏑMutex).Lock();
        ᒐd1 = true;
        return b.Buffer.Write(p);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡb.of(lockedBytesBuffer.ᏑMutex).Unlock(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string connectionCloseˢ => ref connectionCloseˢᵇ;
private static readonly @string connectionCloseˢᵇ = "Connection: close"u8;
internal static ref readonly @string fooˢ4 => ref fooˢ4ᵇ;
private static readonly @string fooˢ4ᵇ = "FOO"u8;

// Issue 15366
public static void TestH12_AutoGzipWithDumpResponse(ж<testing.T> t) {
    new h12Compare(
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var h = w.Header();
            h.Set(contentEncodingˢ, gzipˢ);
            h.Set(contentLengthˢ, "23");
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), ((@string)(new byte[]{0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0xf3, 0xf7, 0x07, 0x00, 0xab, 0x27, 0xd4, 0x1a, 0x03, 0x00, 0x00, 0x00})));
        },
        EarlyCheckResponse: (@string proto, ж<Δhttp.Response> res) => {
            if (!res.Uncompressed) {
                t.Errorf("%s: expected Uncompressed to be set", proto);
            }
            var (dump, err) = httputil.DumpResponse(res, true);
            if (err != nil) {
                t.Errorf("%s: DumpResponse: %v", proto, err);
                return;
            }
            if (strings.Contains((@string)dump, connectionCloseˢ)) {
                t.Errorf("%s: should not see \"Connection: close\" in dump; got:\n%s", proto, dump);
            }
            if (!strings.Contains((@string)dump, fooˢ4)) {
                t.Errorf("%s: should see \"FOO\" in response; got:\n%s", proto, dump);
            }
        }
    ).run(t);
}

// Issue 14607
public static void TestCloseIdleConnections(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testCloseIdleConnections(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xAddrˢ => ref xAddrˢᵇ;
private static readonly @string xAddrˢᵇ = "X-Addr"u8;
internal static readonly object didnTGetXAddrˢ = (@string)"didn't get X-Addr"u8;

internal static void testCloseIdleConnections(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(xAddrˢ, r.RemoteAddr);
    })));
    var cstʗ1 = cst;
    @string get() {
        var (res, err) = cstʗ1.c.Get(cstʗ1.ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        res.Body.Close();
        @string v = res.Header.Get(xAddrˢ);
        if (v == ""u8) {
            t.Fatal(didnTGetXAddrˢ);
        }
        return v;
    }
    @string a1 = get();
    cst.tr.CloseIdleConnections();
    @string a2 = get();
    if (a1 == a2) {
        t.Errorf("didn't close connection");
    }
}

partial struct noteCloseConn {
    /*embed*/ public net_package.Conn Conn;
    internal Action closeFunc;
}

internal static error Close(this noteCloseConn x) {
    x.closeFunc();
    return x.Conn.Close();
}

partial struct testErrorReader {
    internal ж<testing.T> t;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedReadCallˢ = (@string)"unexpected Read call"u8;

internal static partial (nint n, error err) Read(this testErrorReader r, slice<byte> p) {
    r.t.Error(unexpectedReadCallˢ);
    return (0, io.EOF);
}

public static void TestNoSniffExpectRequestBody(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testNoSniffExpectRequestBody(Δp0, Δp1)); }
internal static void testNoSniffExpectRequestBody(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            w.WriteHeader(StatusUnauthorized);
        })));

        // Set ExpectContinueTimeout non-zero so RoundTrip won't try to write it.
        cst.tr.ExpectContinueTimeout = (time.Duration)(10000000000L);

        var (req, err) = NewRequest(postˢ, cst.ts.URL, new testErrorReader(t));
        if (err != nil) {
            t.Fatal(err);
        }
        req.ContentLength = 0; // so transport is tempted to sniff it
        req.Header.Set(expectˢ, continueˢ);
        (var res, err) = cst.tr.RoundTrip(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        if (res.StatusCode != StatusUnauthorized) {
            t.Errorf("status code = %v; want %v", res.StatusCode, (nint)(StatusUnauthorized));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestServerUndeclaredTrailers(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testServerUndeclaredTrailers(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trailerFooˢ => ref trailerFooˢᵇ;
private static readonly @string trailerFooˢᵇ = "Trailer:Foo"u8;
internal static ref readonly @string bazˢ2 => ref bazˢ2ᵇ;
private static readonly @string bazˢ2ᵇ = "Baz"u8;
internal static ref readonly @string baz2ˢ => ref baz2ˢᵇ;
private static readonly @string baz2ˢᵇ = "Baz2"u8;
internal static ref readonly @string trailerBarˢ => ref trailerBarˢᵇ;
private static readonly @string trailerBarˢᵇ = "Trailer:Bar"u8;
internal static ref readonly @string quuxˢ => ref quuxˢᵇ;
private static readonly @string quuxˢᵇ = "Quux"u8;

internal static void testServerUndeclaredTrailers(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        w.Header().Set(fooˢ2, barˢ2);
        w.Header().Set(trailerFooˢ, bazˢ2);
        w._<Flusher>().Flush();
        w.Header().Add(trailerFooˢ, baz2ˢ);
        w.Header().Set(trailerBarˢ, quuxˢ);
    })));
    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    var (_, errΔ1) = io.Copy(io.Discard, res.Body); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    res.Body.Close();
    delete(res.Header, "Date"u8);
    delete(res.Header, "Content-Type"u8);

    {
        var want = (new httpꓸHeader(new map<@string, slice<@string>>{["Foo"] = ["Bar"]})); if (!reflect.DeepEqual(res.Header, want)) {
            t.Errorf("Header = %#v; want %#v", res.Header, want);
        }
    }
    {
        var want = (new httpꓸHeader(new map<@string, slice<@string>>{["Foo"] = ["Baz", "Baz2"], ["Bar"] = ["Quux"]})); if (!reflect.DeepEqual(res.Trailer, want)) {
            t.Errorf("Trailer = %#v; want %#v", res.Trailer, want);
        }
    }
}

public static void TestBadResponseAfterReadingBody(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testBadResponseAfterReadingBody(Δp0, Δp1), slice<testMode>([http1Mode]));
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object someBogusCrapˢ = (@string)"some bogus crap"u8;
internal static readonly object expectedAnErrorToBeˢ = (@string)"expected an error to be returned from Post"u8;

internal static void testBadResponseAfterReadingBody(ж<testing.T> t, testMode mode) {
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            var (_, errΔ1) = io.Copy(io.Discard, r.Body);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            (var c, _, errΔ1) = w._<Hijacker>().Hijack();
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            var cʗ1 = c;
            defer(() => cʗ1.Close(), ref ᒐ);
            fmt.Fprintln(new http_test_package.net_ConnᴠWriter(c), someBogusCrapˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })));

    ref var closes = ref heap<nint>(0, out var Ꮡcloses);
    var (res, err) = cst.c.Post(cst.ts.URL, textPlainˢ, new countCloseReader(Ꮡcloses, new http_test_package.strings_ReaderжReader(strings.NewReader(helloˢ))));
    if (err == nil) {
        res.Body.Close();
        t.Fatal(expectedAnErrorToBeˢ);
    }
    if (closes != 1) {
        t.Errorf("closes = %d; want 1", closes);
    }
}

public static void TestWriteHeader0(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testWriteHeader0(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringInvalidWriteHeaderˢ => ref stringInvalidWriteHeaderˢᵇ;
private static readonly @string stringInvalidWriteHeaderˢᵇ = "string, invalid WriteHeader code 0"u8;
internal static readonly object expectedPanicInHandlerˢ = (@string)"expected panic in handler"u8;

internal static void testWriteHeader0(ж<testing.T> t, testMode mode) {
    var gotpanic = new channel<bool>(1);
    var gotpanicʗ1 = gotpanic;
    var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => builtin.close(ᴛ1), gotpanicʗ1, ref ᒐ);
            var gotpanicʗ2 = gotpanicʗ1;
            defer(() => {
                var e = recover(); if (e != nil) {
                    @string got = fmt.Sprintf("%T, %v"u8, e, e);
                    @string want = stringInvalidWriteHeaderˢ;
                    if (got != want) {
                        t.Errorf("unexpected panic value:\n got: %v\nwant: %v\n", got, want);
                    }
                    gotpanicʗ2.ᐸꟷ(true);

                    // Set an explicit 503. This also tests that the WriteHeader call panics
                    // before it recorded that an explicit value was set and that bogus
                    // value wasn't stuck.
                    w.WriteHeader(503);
                }
            }, ref ᒐ);
            w.WriteHeader(0);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })));
    var (res, err) = cst.c.Get(cst.ts.URL);
    if (err != nil) {
        t.Fatal(err);
    }
    if (res.StatusCode != 503) {
        t.Errorf("Response: %v %q; want 503", res.StatusCode, res.Status);
    }
    if (!ᐸꟷ(gotpanic)) {
        t.Error(expectedPanicInHandlerˢ);
    }
}

// Issue 23010: don't be super strict checking WriteHeader's code if
// it's not even valid to call WriteHeader then anyway.
public static void TestWriteHeaderNoCodeCheck(ж<testing.T> t) {
    run<TжTBRun>(t, (TжTBRun tΔ1Δp, testMode mode) => {
        var tΔ1 = (ж<testing.T>)tΔ1Δp;
        testWriteHeaderAfterWrite(tΔ1, mode, false);
    });
}
public static void TestWriteHeaderNoCodeCheck_h1hijack(ж<testing.T> t) {
    testWriteHeaderAfterWrite(t, http1Mode, true);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string foobarˢ => ref foobarˢᵇ;
private static readonly @string foobarˢᵇ = "foobar"u8;
internal static ref readonly @string httpSuperfluousResponseˢ => ref httpSuperfluousResponseˢᵇ;
private static readonly @string httpSuperfluousResponseˢᵇ = "http: superfluous response.WriteHeader call from net/http_test.testWriteHeaderAfterWrite.func1 (clientserver_test.go:"u8;
internal static ref readonly @string httpResponseWriteHeaderˢ => ref httpResponseWriteHeaderˢᵇ;
private static readonly @string httpResponseWriteHeaderˢᵇ = "http: response.WriteHeader on hijacked connection from net/http_test.testWriteHeaderAfterWrite.func1 (clientserver_test.go:"u8;

internal static void testWriteHeaderAfterWrite(ж<testing.T> t, testMode mode, bool hijack) {
    GoFrame ᒐ = default;
    try {
        ref var errorLog = ref heap(new lockedBytesBuffer(), out var ᏑerrorLog);
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            GoFrame ᒐ = default;
            try {
                if (hijack) {
                    var (conn, _, _) = w._<Hijacker>().Hijack();
                    var connʗ1 = conn;
                    defer(() => connʗ1.Close(), ref ᒐ);
                    conn.Write(slice<byte>("HTTP/1.1 200 OK\r\nContent-Length: 6\r\n\r\nfoo"u8));
                    w.WriteHeader(0); // verify this doesn't panic if there's already output; Issue 23010
                    conn.Write(slice<byte>("bar"u8));
                    return;
                }
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), fooˢ);
                w._<Flusher>().Flush();
                w.WriteHeader(0); // verify this doesn't panic if there's already output; Issue 23010
                io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), barˢ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })), (ж<httptest.Server> ts) => {
            ts.Config.ErrorLog = log.New(new http_test_package.lockedBytesBufferжWriter(ᏑerrorLog), ""u8, 0);
        });
        var (res, err) = cst.c.Get(cst.ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var body, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(err);
        }
        @string got = (@string)body;
        @string want = foobarˢ; if (got != want) {
            t.Errorf("got = %q; want %q", got, want);
        }

        // Also check the stderr output:
        if (mode == http2Mode) {
            // TODO: also emit this log message for HTTP/2?
            // We historically haven't, so don't check.
            return;
        }
        @string gotLog = strings.TrimSpace(ᏑerrorLog.of(lockedBytesBuffer.ᏑBuffer).String());
        @string wantLog = httpSuperfluousResponseˢ;
        if (hijack) {
            wantLog = httpResponseWriteHeaderˢ;
        }
        if (!strings.HasPrefix(gotLog, wantLog)) {
            t.Errorf("stderr output = %q; want %q", gotLog, wantLog);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestBidiStreamReverseProxy(ж<testing.T> t) {
    run<TжTBRun>(t, (Δp0, Δp1) => testBidiStreamReverseProxy(Δp0, Δp1), slice<testMode>([http2Mode]));
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string putˢ => ref putˢᵇ;
private static readonly @string putˢᵇ = "PUT"u8;
internal static ref readonly @string timeoutˢ => ref timeoutˢᵇ;
private static readonly @string timeoutˢᵇ = "timeout"u8;

internal static partial void testBidiStreamReverseProxy(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        var backend = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (_, errΔ1) = io.Copy(new http_test_package.http_ResponseWriterᴠWriter(w), r.Body); if (errΔ1 != nil) {
                log.Printf("bidi backend copy: %v", errΔ1);
            }
        })));

        var (backURL, err) = url.Parse(backend.ts.URL);
        if (err != nil) {
            t.Fatal(err);
        }
        var rp = httputil.NewSingleHostReverseProxy(backURL);
        rp.Transport = new Δhttp.TransportжRoundTripper(backend.tr);
        var rpʗ1 = rp;
        var proxy = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            rpʗ1.ServeHTTP(w, r);
        })));

        var bodyRes = new channel<any>(1); // error or hash.Hash
        var (pr, pw) = io.Pipe();
        var (req, _) = NewRequest(putˢ, proxy.ts.URL, new io.PipeReaderжReader(pr));
        UntypedInt size = /* 4 << 20 */ 4194304;
        var bodyResʗ1 = bodyRes;
        var pwʗ1 = pw;
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var h = sha1.New();
            var (_, errΔ2) = io.CopyN(io.MultiWriter(h, new io.PipeWriterжWriter(pwʗ1)), rand.Reader, size);
            var pwʗ2 = pwʗ1;
            goǃ(() => pwʗ2.Close());
            if (errΔ2 != nil) {
                t.Errorf("body copy: %v", errΔ2);
                bodyResʗ1.ᐸꟷ(errΔ2);
            } else {
                bodyResʗ1.ᐸꟷ(h);
            }
        });
        (var res, err) = backend.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        var hgot = sha1.New();
        (var n, err) = io.Copy(hgot, res.Body);
        if (err != nil) {
            t.Fatal(err);
        }
        if (n != size) {
            t.Fatalf("got %d bytes; want %d", n, (nint)(size));
        }
        var selᴛ14 = bodyRes;
        var selᴛ15 = time.After((time.Duration)(10000000000L));
        switch (select(ᐸꟷ(selᴛ14, ꓸꓸꓸ), ᐸꟷ(selᴛ15, ꓸꓸꓸ))) {
        case 0 when selᴛ14.ꟷᐳ(out var v): {
            switch (v.type()) {
            default: {
                var vΔ1 = v;
                t.Fatalf("body copy: %v", err);
                break;
            }
            case {} ΔvΔ1 when ΔvΔ1._<hash.Hash>(out var vΔ1): {
                if (!bytes.Equal(vΔ1.Sum(default), hgot.Sum(default))) {
                    t.Errorf("written bytes didn't match received bytes");
                }
                break;
            }}
            break;
        }
        case 1 when selᴛ15.ꟷᐳ(out _): {
            t.Fatal(timeoutˢ);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string upgradeˢ => ref upgradeˢᵇ;
private static readonly @string upgradeˢᵇ = "Upgrade"u8;
internal static ref readonly @string webSocketˢ => ref webSocketˢᵇ;
private static readonly @string webSocketˢᵇ = "WebSocket"u8;
internal static ref readonly @string httpIgnoreˢ => ref httpIgnoreˢᵇ;
private static readonly @string httpIgnoreˢᵇ = "HTTP/IGNORE"u8;

// Always use HTTP/1.1 for WebSocket upgrades.
public static void TestH12_WebSocketUpgrade(ж<testing.T> t) {
    new h12Compare(
        Handler: (Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var h = w.Header();
            h.Set(fooˢ2, barˢ);
        },
        ReqFunc: (ж<Δhttp.Client> c, @string url) => {
            var (req, _) = NewRequest(getˢ2, url, default);
            req.Header.Set(connectionˢ, upgradeˢ);
            req.Header.Set(upgradeˢ, webSocketˢ);
            return c.Do(req);
        },
        EarlyCheckResponse: (@string proto, ж<Δhttp.Response> res) => {
            if (res.Proto != "HTTP/1.1"u8) {
                t.Errorf("%s: expected HTTP/1.1, got %q", proto, res.Proto);
            }
            res.Proto = httpIgnoreˢ; // skip later checks that Proto must be 1.1 vs 2.0
        }
    ).run(t);
}

public static void TestIdentityTransferEncoding(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testIdentityTransferEncoding(Δp0, Δp1)); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string transferEncodingˢ => ref transferEncodingˢᵇ;
private static readonly @string transferEncodingˢᵇ = "Transfer-Encoding"u8;
internal static ref readonly @string identityˢ => ref identityˢᵇ;
private static readonly @string identityˢᵇ = "identity"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bodyᶜ3 => ref bodyᶜ3ᵇ;
private static readonly @string bodyᶜ3ᵇ = "body"u8;

internal static void testIdentityTransferEncoding(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        @string body = bodyᶜ3;
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var (gotBodyΔ1, _) = io.ReadAll(r.Body);
            @string got = (@string)gotBodyΔ1;
            @string want = body; if (got != want) {
                t.Errorf("got request body = %q; want %q", got, want);
            }
            w.Header().Set(transferEncodingˢ, identityˢ);
            w.WriteHeader(StatusOK);
            w._<Flusher>().Flush();
            io.WriteString(new http_test_package.http_ResponseWriterᴠWriter(w), body);
        })));
        var (req, _) = NewRequest(getˢ2, cst.ts.URL, new http_test_package.strings_ReaderжReader(strings.NewReader(body)));
        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);
        (var gotBody, err) = io.ReadAll(res.Body);
        if (err != nil) {
            t.Fatal(err);
        }
        {
            @string got = (@string)gotBody;
            @string want = body; if (got != want) {
                t.Errorf("got response body = %q; want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestEarlyHintsRequest(ж<testing.T> t) { run<TжTBRun>(t, (Δp0, Δp1) => testEarlyHintsRequest(Δp0, Δp1)); }
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

internal static void testEarlyHintsRequest(ж<testing.T> t, testMode mode) {
    GoFrame ᒐ = default;
    try {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var cst = newClientServerTest(new http_test_package.testing_TжTB(t), mode, new http_test_package.http_HandlerFuncᴠΔHandler(new Δhttp.HandlerFunc((Δhttp.ResponseWriter w, ж<Δhttp.Request> r) => {
            var h = w.Header();

            h.Add(contentLengthˢ, "123"); // must be ignored
            h.Add(linkˢ, styleCssRelPreloadAsˢ);
            h.Add(linkˢ, scriptJsRelPreloadAsˢ);
            w.WriteHeader(StatusEarlyHints);

            Ꮡwg.Wait();

            h.Add(linkˢ, fooJsRelPreloadAsScriptˢ);
            w.WriteHeader(StatusEarlyHints);

            w.Write(slice<byte>("Hello"u8));
        })));

        void checkLinkHeaders(ж<testing.T> tΔ1, slice<@string> expected, slice<@string> got) {
            tΔ1.Helper();

            if (len(expected) != len(got)) {
                tΔ1.Errorf("got %d expected %d", len(got), len(expected));
            }

            foreach (var i in range(len(expected))) {
                if (expected[i] != got[i]) {
                    tΔ1.Errorf("got %q expected %q", got[i], expected[i]);
                }
            }
        }

        void checkExcludedHeaders(ж<testing.T> tΔ2, textproto.MIMEHeader header) {
            tΔ2.Helper();

            foreach (var (_, h) in range(slice<@string>(["Content-Length", "Transfer-Encoding"]))) {
                var (v, ok) = header[h, ꟷ]; if (ok) {
                    tΔ2.Errorf("%s is %q; must not be sent", h, v);
                }
            }
        }

        uint8 respCounter = default;
            var checkExcludedHeadersʗ1 = checkExcludedHeaders;
            var checkLinkHeadersʗ1 = checkLinkHeaders;
        var trace = Ꮡ(new httptrace.ClientTrace(
            Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
                switch (respCounter) {
                case 0: {
                    checkLinkHeadersʗ1(t, ["</style.css>; rel=preload; as=style", "</script.js>; rel=preload; as=script"], header[linkˢ]);
                    checkExcludedHeadersʗ1(t, header);

                    Ꮡwg.Done();
                    break;
                }
                case 1: {
                    checkLinkHeadersʗ1(t, ["</style.css>; rel=preload; as=style", "</script.js>; rel=preload; as=script", "</foo.js>; rel=preload; as=script"], header[linkˢ]);
                    checkExcludedHeadersʗ1(t, header);
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
        var (req, _) = NewRequestWithContext(httptrace.WithClientTrace(context_package.Background(), trace), getˢ2, cst.ts.URL, default);

        var (res, err) = cst.c.Do(req);
        if (err != nil) {
            t.Fatal(err);
        }
        var resʗ1 = res;
        defer(ᴛ0 => ᴛ0.Close(), resʗ1.Body, ref ᒐ);

        checkLinkHeaders(t, ["</style.css>; rel=preload; as=style", "</script.js>; rel=preload; as=script", "</foo.js>; rel=preload; as=script"], res.Header[linkˢ]);
        @string cl = res.Header.Get(contentLengthˢ); if (cl != "123"u8) {
            t.Errorf("Content-Length is %q; want 123", cl);
        }

        var (body, _) = io.ReadAll(res.Body);
        if (((sstring)body) != "Hello"u8) {
            t.Errorf("Read body %q; want Hello", body);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_test_package
