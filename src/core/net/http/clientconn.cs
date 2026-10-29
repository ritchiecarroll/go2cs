// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using net = net_package;
using httptrace = go.net.http.httptrace_package;
using url = go.net.url_package;
using sync = sync_package;
using System.Runtime.CompilerServices;
using go.net;
using go.net.http;
using io = io_package;

partial class http_package {

// A ClientConn is a client connection to an HTTP server.
//
// Unlike a [Transport], a ClientConn represents a single connection.
// Most users should use a Transport rather than creating client connections directly.
partial struct ClientConn {
    internal genericClientConn cc;
    internal sync.Mutex stateHookMu;
    internal Action<ж<ClientConn>> userStateHook;
    internal bool stateHookRunning;
    internal nint lastAvailable;
    internal nint lastInFlight;
    internal bool lastClosed;
}

// newClientConner is the interface implemented by HTTP/2 transports to create new client conns.
//
// The http package (this package) needs a way to ask the http2 package to
// create a client connection.
//
// Transport.TLSNextProto["h2"] contains a function which appears to do this,
// but for historical reasons it does not: The TLSNextProto function adds a
// *tls.Conn to the http2.Transport's connection pool and returns a RoundTripper
// which is backed by that connection pool. NewClientConn needs a way to get a
// single client connection out of the http2 package.
//
// The http2 package registers a RoundTripper with Transport.RegisterProtocol.
// If this RoundTripper implements newClientConner, then Transport.NewClientConn will use
// it to create new HTTP/2 client connections.
partial interface newClientConner {
    // NewClientConn creates a new client connection from a net.Conn.
    //
    // The RoundTripper returned by NewClientConn must implement genericClientConn.
    // (We don't define NewClientConn as returning genericClientConn,
    // because either we'd need to make genericClientConn an exported type
    // or define it as a type alias. Neither is particularly appealing.)
    //
    // The state hook passed here is the internal state hook
    // (ClientConn.maybeRunStateHook). The internal state hook calls
    // the user state hook (if any), which is set by the user with
    // ClientConn.SetStateHook.
    //
    // The client connection should arrange to call the internal state hook
    // when the connection closes, when requests complete, and when the
    // connection concurrency limit changes.
    //
    // The client connection must call the internal state hook when the connection state
    // changes asynchronously, such as when a request completes.
    //
    // The internal state hook need not be called after synchronous changes to the state:
    // Close, Reserve, Release, and RoundTrip calls which don't start a request
    // do not need to call the hook.
    //
    // The general idea is that if we call (for example) Close,
    // we know that the connection state has probably changed and we
    // don't need the state hook to tell us that.
    // However, if the connection closes asynchronously
    // (because, for example, the other end of the conn closed it),
    // the state hook needs to inform us.
    (RoundTripper, error) NewClientConn(net.Conn nc, Action internalStateHook);
}

// genericClientConn is an interface implemented by HTTP/2 client conns
// returned from newClientConner.NewClientConn.
//
// See the newClientConner doc comment for more information.
partial interface genericClientConn :
    io.Closer
{
    error Err();
    (ж<Response>, error) RoundTrip(ж<Request> req);
    error Reserve();
    void Release();
    nint Available();
    nint InFlight();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpNewClientConnˢ = "http: NewClientConn returned something that is not a ClientConn"u8;

// NewClientConn creates a new client connection to the given address.
//
// If scheme is "http", the connection is unencrypted.
// If scheme is "https", the connection uses TLS.
//
// The protocol used for the new connection is determined by the scheme,
// Transport.Protocols configuration field, and protocols supported by the
// server. See Transport.Protocols for more details.
//
// If Transport.Proxy is set and indicates that a request sent to the given
// address should use a proxy, the new connection uses that proxy.
//
// NewClientConn always creates a new connection,
// even if the Transport has an existing cached connection to the given host.
//
// The new connection is not added to the Transport's connection cache,
// and will not be used by [Transport.RoundTrip].
// It does not count against the MaxIdleConns and MaxConnsPerHost limits.
//
// The caller is responsible for closing the new connection.
public static (ж<ClientConn>, error) NewClientConn(this ж<Transport> Ꮡt, context.Context ctx, @string scheme, @string address) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.of(Transport.ᏑnextProtoOnce).Do(Ꮡt.onceSetNextProtoDefaults);

        var exprᴛ1 = scheme;
        switch (ᐧ) {
        case {} when exprᴛ1 == "http"u8 || exprᴛ1 == "https"u8: {
            break;
        }
        default: {
            return (default, fmt.Errorf("net/http: invalid scheme %q"u8, scheme));
        }}

        ref var host = ref heap<@string>(out var Ꮡhost);
        (host, var port, var err) = net.SplitHostPort(address);
        if (err != nil) {
            return (default, err);
        }
        if (port == ""u8) {
            port = schemePort(scheme);
        }

        ж<url.URL> proxyURL = default;
        if (t.Proxy != nil) {
            // Transport.Proxy takes a *Request, so create a fake one to pass it.
            var req = Ꮡ(new Request(
                ctx: ctx,
                Method: "GET"u8,
                URL: Ꮡ(new url.URL(
                    Scheme: scheme,
                    Host: host,
                    Path: "/"u8
                )),
                Proto: "HTTP/1.1"u8,
                ProtoMajor: 1,
                ProtoMinor: 1,
                Header: new ΔHeader(0),
                Body: NoBody,
                Host: host
            ));
            error errΔ1 = default;
            (proxyURL, errΔ1) = t.Proxy(req);
            if (errΔ1 != nil) {
                return (default, errΔ1);
            }
        }

        var cm = new connectMethod(
            targetScheme: scheme,
            targetAddr: net.JoinHostPort(host, port),
            proxyURL: proxyURL
        );

        // The state hook is a bit tricky:
        // The persistConn has a state hook which calls ClientConn.maybeRunStateHook,
        // which in turn calls the user-provided state hook (if any).
        //
        // ClientConn.maybeRunStateHook handles debouncing hook calls for both
        // HTTP/1 and HTTP/2.
        //
        // Since there's no need to change the persistConn's hook, we set it at creation time.
        var cc = Ꮡ(new ClientConn());
        const bool isClientConn = true;
        (var pconn, err) = Ꮡt.dialConn(ctx, cm, isClientConn, cc.maybeRunStateHook);
        if (err != nil) {
            return (default, err);
        }

        // Note that cc.maybeRunStateHook may have been called
        // in the short window between dialConn and now.
        // This is fine.
        cc.of(ClientConn.ᏑstateHookMu).Lock();
        var ccʗ1 = cc;
        defer(ccʗ1.of(ClientConn.ᏑstateHookMu).Unlock, ref ᒐ);
        if (pconn.alt != nil) {
            // If pconn.alt is set, this is a connection implemented in another package
            // (probably x/net/http2) or the bundled copy in h2_bundle.go.
            var (gc, ok) = pconn.alt._<genericClientConn>(ᐧ);
            if (!ok) {
                return (default, errors.New(httpNewClientConnˢ));
            }
            cc.cc = gc;
            cc.lastAvailable = gc.Available();
        } else {
            // This is an HTTP/1 connection.
            pconn.availch = new channel<EmptyStruct>(1);
            pconn.availch.ᐸꟷ(new EmptyStruct());
            cc.cc = new http1ClientConn(pconn);
            cc.lastAvailable = 1;
        }
        return (cc, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Close closes the connection.
// Outstanding RoundTrip calls are interrupted.
public static error Close(this ж<ClientConn> Ꮡcc) {
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        defer(Ꮡcc.maybeRunStateHook, ref ᒐ);
        return cc.cc.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Err reports any fatal connection errors.
// It returns nil if the connection is usable.
// If it returns non-nil, the connection can no longer be used.
public static error Err(this ref ClientConn cc) {
    return cc.cc.Err();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string httpNilRequestHeaderˢ = "http: nil Request.Header"u8;
internal static readonly @string httpNoHostInRequestUrlˢ = "http: no Host in request URL"u8;

internal static error validateClientConnRequest(ref Request req) {
    if (req.URL == nil) {
        return errors.New(httpNilRequestUrlˢ);
    }
    if (req.Header == nil) {
        return errors.New(httpNilRequestHeaderˢ);
    }
    // Validate the outgoing headers.
    {
        @string err = validateHeaders(req.Header); if (err != ""u8) {
            return fmt.Errorf("http: invalid header %s"u8, err);
        }
    }
    // Validate the outgoing trailers too.
    {
        @string err = validateHeaders(req.Trailer); if (err != ""u8) {
            return fmt.Errorf("http: invalid trailer %s"u8, err);
        }
    }
    if (req.Method != ""u8 && !validMethod(req.Method)) {
        return fmt.Errorf("http: invalid method %q"u8, req.Method);
    }
    if (req.URL.Host == ""u8) {
        return errors.New(httpNoHostInRequestUrlˢ);
    }
    return default;
}

// RoundTrip implements the [RoundTripper] interface.
//
// The request is sent on the client connection,
// regardless of the URL being requested or any proxy settings.
//
// If the connection is at its concurrency limit,
// RoundTrip waits for the connection to become available
// before sending the request.
public static (ж<Response>, error) RoundTrip(this ж<ClientConn> Ꮡcc, ж<Request> req) {
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        defer(Ꮡcc.maybeRunStateHook, ref ᒐ);
        {
            var err = validateClientConnRequest(ref (req).DerefOrNull()); if (err != nil) {
                Ꮡcc.Release();
                return (default, err);
            }
        }
        return cc.cc.RoundTrip(req);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Available reports the number of requests that may be sent
// to the connection without blocking.
// It returns 0 if the connection is closed.
public static nint Available(this ref ClientConn cc) {
    return cc.cc.Available();
}

// InFlight reports the number of requests in flight,
// including reserved requests.
// It returns 0 if the connection is closed.
public static nint InFlight(this ref ClientConn cc) {
    return cc.cc.InFlight();
}

// Reserve reserves a concurrency slot on the connection.
// If Reserve returns nil, one additional RoundTrip call may be made
// without waiting for an existing request to complete.
//
// The reserved concurrency slot is accounted as an in-flight request.
// A successful call to RoundTrip will decrement the Available count
// and increment the InFlight count.
//
// Each successful call to Reserve should be followed by exactly one call
// to RoundTrip or Release, which will consume or release the reservation.
//
// If the connection is closed or at its concurrency limit,
// Reserve returns an error.
public static error Reserve(this ж<ClientConn> Ꮡcc) {
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        defer(Ꮡcc.maybeRunStateHook, ref ᒐ);
        return cc.cc.Reserve();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Release releases an unused concurrency slot reserved by Reserve.
// If there are no reserved concurrency slots, it has no effect.
public static void Release(this ж<ClientConn> Ꮡcc) {
    GoFrame ᒐ = default;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        defer(Ꮡcc.maybeRunStateHook, ref ᒐ);
        cc.cc.Release();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// shouldRunStateHook returns the user's state hook if we should call it,
// or nil if we don't need to call it at this time.
internal static Action<ж<ClientConn>> shouldRunStateHook(this ж<ClientConn> Ꮡcc, bool stopRunning) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var cc = ref Ꮡcc.DerefOrNull();

        cc.stateHookMu.Lock();
        ᒐd1 = true;
        if (cc.cc == nil) {
            return default;
        }
        if (stopRunning) {
            cc.stateHookRunning = false;
        }
        if (cc.userStateHook == nil) {
            return default;
        }
        if (cc.stateHookRunning) {
            return default;
        }
        nint available = cc.Available();
        nint inFlight = cc.InFlight();
        bool closed = cc.Err() != nil;
        Action<ж<ClientConn>> hook = default;
        if (available > cc.lastAvailable || inFlight < cc.lastInFlight || closed != cc.lastClosed) {
            hook = cc.userStateHook;
            cc.stateHookRunning = true;
        }
        cc.lastAvailable = available;
        cc.lastInFlight = inFlight;
        cc.lastClosed = closed;
        return hook;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡcc.DerefOrNull().stateHookMu.Unlock(); ᒐ.Run(); }
}

internal static partial void maybeRunStateHook(this ж<ClientConn> Ꮡcc) {
    ref var hook = ref heap<Action<ж<ClientConn>>>(Ꮡcc.shouldRunStateHook(false), out var Ꮡhook);
    if (hook == nil) {
        return;
    }
    // Run the hook synchronously.
    //
    // This means that if, for example, the user calls resp.Body.Close to finish a request,
    // the Close call will synchronously run the hook, giving the hook the chance to
    // return the ClientConn to a connection pool before the next request is made.
    hook(Ꮡcc);
    // The connection state may have changed while the hook was running,
    // in which case we need to run it again.
    //
    // If we do need to run the hook again, do so in a new goroutine to avoid blocking
    // the current goroutine indefinitely.
    hook = Ꮡcc.shouldRunStateHook(true);
    if (hook != nil) {
        goǃ(() => {
            while (Ꮡhook.ValueSlot != nil) {
                Ꮡhook.ValueSlot(Ꮡcc);
                Ꮡhook.ValueSlot = Ꮡcc.shouldRunStateHook(true);
            }
        });
    }
}

// SetStateHook arranges for f to be called when the state of the connection changes.
// At most one call to f is made at a time.
// If the connection's state has changed since it was created,
// f is called immediately in a separate goroutine.
// f may be called synchronously from RoundTrip or Response.Body.Close.
//
// If SetStateHook is called multiple times, the new hook replaces the old one.
// If f is nil, no further calls will be made to f after SetStateHook returns.
//
// f is called when Available increases (more requests may be sent on the connection),
// InFlight decreases (existing requests complete), or Err begins returning non-nil
// (the connection is no longer usable).
public static void SetStateHook(this ж<ClientConn> Ꮡcc, Action<ж<ClientConn>> f) {
    ref var cc = ref Ꮡcc.DerefOrNull();

    cc.stateHookMu.Lock();
    cc.userStateHook = f;
    cc.stateHookMu.Unlock();
    Ꮡcc.maybeRunStateHook();
}

// http1ClientConn is a genericClientConn implementation backed by
// an HTTP/1 *persistConn (pconn.alt is nil).
partial struct http1ClientConn {
    internal ж<persistConn> pconn;
}

internal static partial (ж<Response>, error) RoundTrip(this http1ClientConn cc, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    var ctx = req.Context();
    var trace = httptrace.ContextClientTrace(ctx);

    // Convert Request.Cancel into context cancelation.
    (ctx, var cancel) = context_package.WithCancelCause(req.Context());
    if (req.Cancel != nil) {
        goǃ((ᴛ1, ᴛ2, ᴛ3) => awaitLegacyCancel(ᴛ1, ᴛ2, ref ᴛ3.DerefOrNull()), ctx, cancel, Ꮡreq);
    }

    var treq = Ꮡ(new transportRequest(Request: Ꮡreq, trace: trace, ctx: ctx, cancel: cancel));
    var (resp, err) = cc.pconn.roundTrip(treq);
    if (err != nil) {
        return (default, err);
    }
    resp.Request = Ꮡreq;
    return (resp, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string clientConnClosedˢ = "ClientConn closed"u8;

internal static error Close(this http1ClientConn cc) {
    cc.pconn.close(errors.New(clientConnClosedˢ));
    return default;
}

internal static error Err(this http1ClientConn cc) {
    var selᴛ4 = cc.pconn.closech;
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out _): {
        return cc.pconn.closed;
    }
    default: {
        return default;
    }}
}

internal static nint Available(this http1ClientConn cc) {
    GoFrame ᒐ = default;
    try {
        cc.pconn.of(persistConn.Ꮡmu).Lock();
        var ccʗ1 = cc;
        defer(ccʗ1.pconn.of(persistConn.Ꮡmu).Unlock, ref ᒐ);
        if (cc.pconn.closed != nil || cc.pconn.reserved || cc.pconn.inFlight) {
            return 0;
        }
        return 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static nint InFlight(this http1ClientConn cc) {
    GoFrame ᒐ = default;
    try {
        cc.pconn.of(persistConn.Ꮡmu).Lock();
        var ccʗ1 = cc;
        defer(ccʗ1.pconn.of(persistConn.Ꮡmu).Unlock, ref ᒐ);
        if (cc.pconn.closed == nil && (cc.pconn.reserved || cc.pconn.inFlight)) {
            return 1;
        }
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connectionIsUnavailableˢ = "connection is unavailable"u8;

internal static error Reserve(this http1ClientConn cc) {
    GoFrame ᒐ = default;
    try {
        cc.pconn.of(persistConn.Ꮡmu).Lock();
        var ccʗ1 = cc;
        defer(ccʗ1.pconn.of(persistConn.Ꮡmu).Unlock, ref ᒐ);
        if (cc.pconn.closed != nil) {
            return cc.pconn.closed;
        }
        var selᴛ5 = cc.pconn.availch;
        switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
        case 0 when selᴛ5.ꟷᐳ(out _): {
            break;
        }
        default: {
            return errors.New(connectionIsUnavailableˢ);
        }}
        cc.pconn.reserved = true;
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void Release(this http1ClientConn cc) {
    GoFrame ᒐ = default;
    try {
        cc.pconn.of(persistConn.Ꮡmu).Lock();
        var ccʗ1 = cc;
        defer(ccʗ1.pconn.of(persistConn.Ꮡmu).Unlock, ref ᒐ);
        if (cc.pconn.reserved) {
            var selᴛ6 = cc.pconn.availch.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
            switch (trySelect(selᴛ6)) {
            case 0: {
                break;
            }
            default: {
                throw panic("cannot release reservation");
                break;
            }}
            cc.pconn.reserved = false;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end http_package
