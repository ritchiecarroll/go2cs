// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Bridge package to expose http internals to tests in the http_test
// package.
namespace go.net;

using context = context_package;
using fmt = fmt_package;
using net = net_package;
using url = global::go.net.url_package;
using slices = slices_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using bufio = bufio_package;
using global::go.net;
using static global::go.net.http_package;

partial class http_internal_test_package {

public static @string DefaultUserAgent;
internal static void initᴛDefaultUserAgent() { DefaultUserAgent = defaultUserAgent; }
public static Func<@string, net.Conn, net.Conn> NewLoggingConn;
internal static void initᴛNewLoggingConn() { NewLoggingConn = newLoggingConn; }
public static Func<slice<byte>, time.Time, slice<byte>> ExportAppendTime = appendTime;
public static Func<ж<url.URL>, ж<url.URL>, @string, @string> ExportRefererForURL = (ж<url.URL> ᴛ0, ж<url.URL> ᴛ1, @string ᴛ2) => refererForURL(ᴛ0, ref ᴛ1.DerefOrNull(), ᴛ2);
internal static Func<ж<global::go.net.http_package.Server>, net.Conn, ж<global::go.net.http_package.conn>> ExportServerNewConn;
internal static void initᴛExportServerNewConn() { ExportServerNewConn = ((Func<ж<global::go.net.http_package.Server>, net.Conn, ж<global::go.net.http_package.conn>>)(global::go.net.http_package.newConn)); }
internal static Action<ж<global::go.net.http_package.conn>> ExportCloseWriteAndWait;
internal static void initᴛExportCloseWriteAndWait() { ExportCloseWriteAndWait = ((Action<ж<global::go.net.http_package.conn>>)(global::go.net.http_package.closeWriteAndWait)); }
public static error ExportErrRequestCanceled;
internal static void initᴛExportErrRequestCanceled() { ExportErrRequestCanceled = errRequestCanceled; }
public static error ExportErrRequestCanceledConn;
internal static void initᴛExportErrRequestCanceledConn() { ExportErrRequestCanceledConn = errRequestCanceledConn; }
public static error ExportErrServerClosedIdle;
internal static void initᴛExportErrServerClosedIdle() { ExportErrServerClosedIdle = errServerClosedIdle; }
public static Action<global::go.net.http_package.ResponseWriter, ж<global::go.net.http_package.Request>, global::go.net.http_package.FileSystem, @string, bool> ExportServeFile;
internal static void initᴛExportServeFile() { ExportServeFile = serveFile; }
public static Func<@string, (@string, @string)> ExportScanETag = scanETag;
internal static Func<ж<global::go.net.http_package.Server>, ж<global::go.net.http_package.http2Server>, error> ExportHttp2ConfigureServer;
internal static void initᴛExportHttp2ConfigureServer() { ExportHttp2ConfigureServer = http2ConfigureServer; }
public static Func<ж<url.URL>, ж<url.URL>, bool> Export_shouldCopyHeaderOnRedirect = shouldCopyHeaderOnRedirect;
public static Action<ж<bufio.Writer>, bool, nint, slice<byte>> Export_writeStatusLine = writeStatusLine;
public static Func<slice<byte>, bool> Export_is408Message = is408Message;

public static ж<time.Duration> MaxWriteWaitBeforeConnReuse;
internal static void initᴛMaxWriteWaitBeforeConnReuse() { MaxWriteWaitBeforeConnReuse = ᏑmaxWriteWaitBeforeConnReuse; }

[GoInit] internal static void init() {
    // We only want to pay for this cost during testing.
    // When not under test, these values are always nil
    // and never assigned to.
    testHookMu = new sync.MutexжLocker(@new<sync.Mutex>());
    testHookClientDoResult = (ж<global::go.net.http_package.Response> res, error err) => {
        if (err != default!){
            {
                var (_, ok) = err._<ж<urlꓸError>>(ᐧ); if (!ok) {
                    throw panic(fmt.Sprintf("unexpected Client.Do error of type %T; want *url.Error"u8, err));
                }
            }
        } else {
            if (res == nil) {
                throw panic("Client.Do returned nil, nil");
            }
            if ((~res).Body == default!) {
                throw panic("Client.Do returned nil res.Body and no error");
            }
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingHttp2TestWhenˢ = (@string)"skipping HTTP/2 test when nethttpomithttp2 build tag in use"u8;

public static void CondSkipHTTP2(testing.TB t) {
    if (omitBundledHTTP2) {
        t.Skip(skippingHttp2TestWhenˢ);
    }
}

public static Action<Action> SetEnterRoundTripHook;
internal static void initᴛSetEnterRoundTripHook() { SetEnterRoundTripHook = hookSetter(ᏑtestHookEnterRoundTrip); }
public static Action<Action> SetRoundTripRetried;
internal static void initᴛSetRoundTripRetried() { SetRoundTripRetried = hookSetter(ᏑtestHookRoundTripRetried); }

public static void SetReadLoopBeforeNextReadHook(Action fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    unnilTestHook(Ꮡf);
    testHookReadLoopBeforeNextRead = f;
}

// SetPendingDialHooks sets the hooks that run before and after handling
// pending dials.
public static void SetPendingDialHooks(Action beforeʗp, Action afterʗp) {
    ref var before = ref heap(beforeʗp, out var Ꮡbefore);
    ref var after = ref heap(afterʗp, out var Ꮡafter);

    unnilTestHook(Ꮡbefore);
    unnilTestHook(Ꮡafter);
    (testHookPrePendingDial, testHookPostPendingDial) = (before, after);
}

public static void SetTestHookServerServe(Action<ж<global::go.net.http_package.Server>, net.Listener> fn) {
    testHookServerServe = fn;
}

public static void SetTestHookProxyConnectTimeout(ж<testing.T> Ꮡt, Func<context.Context, time.Duration, (context.Context, Action)> f) {
    var orig = testHookProxyConnectTimeout;
    var origʗ1 = orig;
    Ꮡt.Cleanup(() => {
        testHookProxyConnectTimeout = origʗ1;
    });
    testHookProxyConnectTimeout = f;
}

public static global::go.net.http_package.ΔHandler NewTestTimeoutHandler(global::go.net.http_package.ΔHandler Δhandler, context.Context ctx) {
    return new global::go.net.http_package.timeoutHandlerжΔHandler(Ꮡ(new timeoutHandler(
        handler: Δhandler,
        testContext: ctx
    )));
}

// (no body)
public static void ResetCachedEnvironment() {
    resetProxyConfig();
}

internal static nint NumPendingRequestsForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.reqMu.Lock();
        ᒐd1 = true;
        return builtin.len(t.reqCanceler);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().reqMu.Unlock(); ᒐ.Run(); }
}

internal static slice<@string> /*keys*/ IdleConnKeysForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    slice<@string> keys = default!;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        keys = new slice<@string>(0);
        t.idleMu.Lock();
        defer(Ꮡt.of(global::go.net.http_package.Transport.ᏑidleMu).Unlock, ref ᒐ);
        foreach (var (key, _) in t.idleConn) {
            keys = append(keys, key.String());
        }
        slices.Sort<slice<@string>, @string>(keys);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return keys;
}

internal static nint IdleConnKeyCountForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.idleMu.Lock();
        ᒐd1 = true;
        return builtin.len(t.idleConn);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

internal static slice<@string> IdleConnStrsForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        slice<@string> ret = default!;
        t.idleMu.Lock();
        ᒐd1 = true;
        foreach (var (_, conns) in t.idleConn) {
            foreach (var (_, pc) in conns) {
                ret = append(ret, (~pc).conn.LocalAddr().String() + "/"u8 + (~pc).conn.RemoteAddr().String());
            }
        }
        slices.Sort<slice<@string>, @string>(ret);
        return ret;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

internal static slice<@string> IdleConnStrsForTesting_h2(this ж<global::go.net.http_package.Transport> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        slice<@string> ret = default!;
        var noDialPool = (~t.h2transport._<ж<global::go.net.http_package.http2Transport>>()).ConnPool._<http2noDialClientConnPool>();
        var pool = noDialPool.http2clientConnPool;
        pool.of(global::go.net.http_package.http2clientConnPool.Ꮡmu).Lock();
        var poolʗ1 = pool;
        defer(poolʗ1.of(global::go.net.http_package.http2clientConnPool.Ꮡmu).Unlock, ref ᒐ);
        foreach (var (k, ccs) in (~pool).conns) {
            foreach (var (_, cc) in ccs) {
                if (cc.idleState().canTakeNewRequest) {
                    ret = append(ret, k);
                }
            }
        }
        slices.Sort<slice<@string>, @string>(ret);
        return ret;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static nint IdleConnCountForTesting(this ж<global::go.net.http_package.Transport> Ꮡt, @string scheme, @string addr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.idleMu.Lock();
        ᒐd1 = true;
        var key = new connectMethodKey(""u8, scheme, addr, false);
        @string cacheKey = key.String();
        foreach (var (k, conns) in t.idleConn) {
            if (k.String() == cacheKey) {
                return builtin.len(conns);
            }
        }
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

internal static nint IdleConnWaitMapSizeForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.idleMu.Lock();
        ᒐd1 = true;
        return builtin.len(t.idleConnWait);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

internal static bool IsIdleForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.idleMu.Lock();
        ᒐd1 = true;
        return t.closeIdle;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.DerefOrNull().idleMu.Unlock(); ᒐ.Run(); }
}

internal static void QueueForIdleConnForTesting(this ж<global::go.net.http_package.Transport> Ꮡt) {
    Ꮡt.queueForIdleConn(nil);
}

// PutIdleTestConn reports whether it was able to insert a fresh
// persistConn for scheme, addr into the idle connection pool.
internal static bool PutIdleTestConn(this ж<global::go.net.http_package.Transport> Ꮡt, @string scheme, @string addr) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (c, _) = net.Pipe();
    ref var key = ref heap<global::go.net.http_package.connectMethodKey>(out var Ꮡkey);
    key = new connectMethodKey(""u8, scheme, addr, false);
    if (t.MaxConnsPerHost > 0) {
        // Transport is tracking conns-per-host.
        // Increment connection count to account
        // for new persistConn created below.
        t.connsPerHostMu.Lock();
        if (t.connsPerHost == default!) {
            t.connsPerHost = new map<global::go.net.http_package.connectMethodKey, nint>();
        }
        t.connsPerHost[key]++;
        t.connsPerHostMu.Unlock();
    }
    return Ꮡt.tryPutIdleConn(Ꮡ(new persistConn(
        t: Ꮡt,
        conn: c, // dummy

        closech: new channel<EmptyStruct>(0), // so it can be closed

        cacheKey: key
    ))) == default!;
}

// PutIdleTestConnH2 reports whether it was able to insert a fresh
// HTTP/2 persistConn for scheme, addr into the idle connection pool.
internal static bool PutIdleTestConnH2(this ж<global::go.net.http_package.Transport> Ꮡt, @string scheme, @string addr, global::go.net.http_package.RoundTripper alt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var key = ref heap<global::go.net.http_package.connectMethodKey>(out var Ꮡkey);
    key = new connectMethodKey(""u8, scheme, addr, false);
    if (t.MaxConnsPerHost > 0) {
        // Transport is tracking conns-per-host.
        // Increment connection count to account
        // for new persistConn created below.
        t.connsPerHostMu.Lock();
        if (t.connsPerHost == default!) {
            t.connsPerHost = new map<global::go.net.http_package.connectMethodKey, nint>();
        }
        t.connsPerHost[key]++;
        t.connsPerHostMu.Unlock();
    }
    return Ꮡt.tryPutIdleConn(Ꮡ(new persistConn(
        t: Ꮡt,
        alt: alt,
        cacheKey: key
    ))) == default!;
}

// All test hooks must be non-nil so they can be called directly,
// but the tests use nil to mean hook disabled.
internal static void unnilTestHook(ж<Action> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f == default!) {
        f = nop;
    }
}

internal static Action<Action> hookSetter(ж<Action> Ꮡdst) {
    return (Action fnʗp) => {
        ref var fn = ref heap(fnʗp, out var Ꮡfn);
        unnilTestHook(Ꮡfn);
        Ꮡdst.ValueSlot = fn;
    };
}

public static error ExportHttp2ConfigureTransport(ж<global::go.net.http_package.Transport> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (t2, err) = http2configureTransports(Ꮡt);
    if (err != default!) {
        return err;
    }
    t.h2transport = new global::go.net.http_package.http2Transportжh2Transport(t2);
    return default!;
}

internal static bool ExportAllConnsIdle(this ж<global::go.net.http_package.Server> Ꮡs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        s.mu.Lock();
        ᒐd1 = true;
        foreach (var (c, _) in s.activeConn) {
            var (st, unixSec) = c.getState();
            if (unixSec == 0 || st != StateIdle) {
                return false;
            }
        }
        return true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡs.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static map<global::go.net.http_package.ConnState, nint> ExportAllConnsByState(this ж<global::go.net.http_package.Server> Ꮡs) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        var states = new map<global::go.net.http_package.ConnState, nint>{};
        s.mu.Lock();
        ᒐd1 = true;
        foreach (var (c, _) in s.activeConn) {
            var (st, _) = c.getState();
            states[st] += 1;
        }
        return states;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡs.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static ж<global::go.net.http_package.Request> WithT(this ref global::go.net.http_package.Request r, ж<testing.T> Ꮡt) {
    return r.WithContext(context_package.WithValue(r.Context(), new tLogKey(nil), Ꮡt.Logf));
}

public static Action /*restore*/ ExportSetH2GoawayTimeout(time.Duration d) {
    var old = http2goAwayTimeout;
    http2goAwayTimeout = d;
    return () => {
        http2goAwayTimeout = old;
    };
}

internal static bool ExportIsReplayable(this ref global::go.net.http_package.Request r) {
    return r.isReplayable();
}

// ExportCloseTransportConnsAbruptly closes all idle connections from
// tr in an abrupt way, just reaching into the underlying Conns and
// closing them, without telling the Transport or its persistConns
// that it's doing so. This is to simulate the server closing connections
// on the Transport.
public static void ExportCloseTransportConnsAbruptly(ж<global::go.net.http_package.Transport> Ꮡtr) {
    ref var tr = ref Ꮡtr.DerefOrNull();

    tr.idleMu.Lock();
    foreach (var (_, pcs) in tr.idleConn) {
        foreach (var (_, pc) in pcs) {
            (~pc).conn.Close();
        }
    }
    tr.idleMu.Unlock();
}

// ResponseWriterConnForTesting returns w's underlying connection, if w
// is a regular *response ResponseWriter.
public static (net.Conn c, bool ok) ResponseWriterConnForTesting(global::go.net.http_package.ResponseWriter w) {
    {
        var (r, okΔ1) = w._<ж<global::go.net.http_package.response>>(ᐧ); if (okΔ1) {
            return ((~(~r).conn).rwc, true);
        }
    }
    return (default!, false);
}

[GoInit] internal static void initΔ1() {
    // Set the default rstAvoidanceDelay to the minimum possible value to shake
    // out tests that unexpectedly depend on it. Such tests should use
    // runTimeSensitiveTest and SetRSTAvoidanceDelay to explicitly raise the delay
    // if needed.
    rstAvoidanceDelay = 1 * time.ΔNanosecond;
}

// SetRSTAvoidanceDelay sets how long we are willing to wait between calling
// CloseWrite on a connection and fully closing the connection.
public static void SetRSTAvoidanceDelay(ж<testing.T> Ꮡt, time.Duration d) {
    var prevDelay = rstAvoidanceDelay;
    Ꮡt.Cleanup(() => {
        rstAvoidanceDelay = prevDelay;
    });
    rstAvoidanceDelay = d;
}

} // end http_internal_test_package
