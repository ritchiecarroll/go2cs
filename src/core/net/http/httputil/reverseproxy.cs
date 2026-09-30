// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// HTTP reverse proxy handler
namespace go.net.http;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using mime = mime_package;
using net = net_package;
using http = go.net.http_package;
using httptrace = go.net.http.httptrace_package;
using ascii = go.net.http.@internal.ascii_package;
using textproto = go.net.textproto_package;
using url = go.net.url_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using httpguts = vendor.golang.org.x.net.http.httpguts_package;
using bufio = bufio_package;
using go.net;
using go.net.http;
using go.net.http.@internal;
using vendor.golang.org.x.net.http;
using ꓸꓸꓸany = Span<any>;

partial class httputil_package {

// A ProxyRequest contains a request to be rewritten by a [ReverseProxy].
[GoType] partial struct ProxyRequest {
    // In is the request received by the proxy.
    // The Rewrite function must not modify In.
    public ж<http.Request> In;
    // Out is the request which will be sent by the proxy.
    // The Rewrite function may modify or replace this request.
    // Hop-by-hop headers are removed from this request
    // before Rewrite is called.
    public ж<http.Request> Out;
}

// SetURL routes the outbound request to the scheme, host, and base path
// provided in target. If the target's path is "/base" and the incoming
// request was for "/dir", the target request will be for "/base/dir".
//
// SetURL rewrites the outbound Host header to match the target's host.
// To preserve the inbound request's Host header (the default behavior
// of [NewSingleHostReverseProxy]):
//
//	rewriteFunc := func(r *httputil.ProxyRequest) {
//		r.SetURL(url)
//		r.Out.Host = r.In.Host
//	}
[GoRecv] public static void SetURL(this ref ProxyRequest r, ж<url.URL> Ꮡtarget) {
    rewriteRequestURL(ref (r.Out).DerefOrNull(), Ꮡtarget);
    r.Out.Value.Host = ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xForwardedForˢ = "X-Forwarded-For"u8;
internal static readonly @string xForwardedHostˢ = "X-Forwarded-Host"u8;
internal static readonly @string xForwardedProtoˢ = "X-Forwarded-Proto"u8;
internal static readonly @string httpsˢ2 = "https"u8;

// SetXForwarded sets the X-Forwarded-For, X-Forwarded-Host, and
// X-Forwarded-Proto headers of the outbound request.
//
//   - The X-Forwarded-For header is set to the client IP address.
//   - The X-Forwarded-Host header is set to the host name requested
//     by the client.
//   - The X-Forwarded-Proto header is set to "http" or "https", depending
//     on whether the inbound request was made on a TLS-enabled connection.
//
// If the outbound request contains an existing X-Forwarded-For header,
// SetXForwarded appends the client IP address to it. To append to the
// inbound request's X-Forwarded-For header (the default behavior of
// [ReverseProxy] when using a Director function), copy the header
// from the inbound request before calling SetXForwarded:
//
//	rewriteFunc := func(r *httputil.ProxyRequest) {
//		r.Out.Header["X-Forwarded-For"] = r.In.Header["X-Forwarded-For"]
//		r.SetXForwarded()
//	}
[GoRecv] public static void SetXForwarded(this ref ProxyRequest r) {
    var (clientIP, _, err) = net.SplitHostPort((~r.In).RemoteAddr);
    if (err == default!){
        var prior = (~r.Out).Header[xForwardedForˢ];
        if (len(prior) > 0) {
            clientIP = strings.Join(prior, ", "u8) + ", "u8 + clientIP;
        }
        (~r.Out).Header.Set(xForwardedForˢ, clientIP);
    } else {
        (~r.Out).Header.Del(xForwardedForˢ);
    }
    (~r.Out).Header.Set(xForwardedHostˢ, (~r.In).Host);
    if ((~r.In).TLS == nil){
        (~r.Out).Header.Set(xForwardedProtoˢ, httpˢ);
    } else {
        (~r.Out).Header.Set(xForwardedProtoˢ, httpsˢ2);
    }
}

// ReverseProxy is an HTTP Handler that takes an incoming request and
// sends it to another server, proxying the response back to the
// client.
//
// 1xx responses are forwarded to the client if the underlying
// transport supports ClientTrace.Got1xxResponse.
[GoType] partial struct ReverseProxy {
    // Rewrite must be a function which modifies
    // the request into a new request to be sent
    // using Transport. Its response is then copied
    // back to the original client unmodified.
    // Rewrite must not access the provided ProxyRequest
    // or its contents after returning.
    //
    // The Forwarded, X-Forwarded, X-Forwarded-Host,
    // and X-Forwarded-Proto headers are removed from the
    // outbound request before Rewrite is called. See also
    // the ProxyRequest.SetXForwarded method.
    //
    // Unparsable query parameters are removed from the
    // outbound request before Rewrite is called.
    // The Rewrite function may copy the inbound URL's
    // RawQuery to the outbound URL to preserve the original
    // parameter string. Note that this can lead to security
    // issues if the proxy's interpretation of query parameters
    // does not match that of the downstream server.
    //
    // At most one of Rewrite or Director may be set.
    public Action<ж<ProxyRequest>> Rewrite;
    // Director is a function which modifies
    // the request into a new request to be sent
    // using Transport. Its response is then copied
    // back to the original client unmodified.
    // Director must not access the provided Request
    // after returning.
    //
    // By default, the X-Forwarded-For header is set to the
    // value of the client IP address. If an X-Forwarded-For
    // header already exists, the client IP is appended to the
    // existing values. As a special case, if the header
    // exists in the Request.Header map but has a nil value
    // (such as when set by the Director func), the X-Forwarded-For
    // header is not modified.
    //
    // To prevent IP spoofing, be sure to delete any pre-existing
    // X-Forwarded-For header coming from the client or
    // an untrusted proxy.
    //
    // Hop-by-hop headers are removed from the request after
    // Director returns, which can remove headers added by
    // Director. Use a Rewrite function instead to ensure
    // modifications to the request are preserved.
    //
    // Unparsable query parameters are removed from the outbound
    // request if Request.Form is set after Director returns.
    //
    // At most one of Rewrite or Director may be set.
    public Action<ж<http.Request>> Director;
    // The transport used to perform proxy requests.
    // If nil, http.DefaultTransport is used.
    public http.RoundTripper Transport;
    // FlushInterval specifies the flush interval
    // to flush to the client while copying the
    // response body.
    // If zero, no periodic flushing is done.
    // A negative value means to flush immediately
    // after each write to the client.
    // The FlushInterval is ignored when ReverseProxy
    // recognizes a response as a streaming response, or
    // if its ContentLength is -1; for such responses, writes
    // are flushed to the client immediately.
    public time.Duration FlushInterval;
    // ErrorLog specifies an optional logger for errors
    // that occur when attempting to proxy the request.
    // If nil, logging is done via the log package's standard logger.
    public ж<log.Logger> ErrorLog;
    // BufferPool optionally specifies a buffer pool to
    // get byte slices for use by io.CopyBuffer when
    // copying HTTP response bodies.
    public BufferPool BufferPool;
    // ModifyResponse is an optional function that modifies the
    // Response from the backend. It is called if the backend
    // returns a response at all, with any HTTP status code.
    // If the backend is unreachable, the optional ErrorHandler is
    // called without any call to ModifyResponse.
    //
    // If ModifyResponse returns an error, ErrorHandler is called
    // with its error value. If ErrorHandler is nil, its default
    // implementation is used.
    public Func<ж<http.Response>, error> ModifyResponse;
    // ErrorHandler is an optional function that handles errors
    // reaching the backend or errors from ModifyResponse.
    //
    // If nil, the default is to log the provided error and return
    // a 502 Status Bad Gateway response.
    public Action<http.ResponseWriter, ж<http.Request>, error> ErrorHandler;
}

// A BufferPool is an interface for getting and returning temporary
// byte slices for use by [io.CopyBuffer].
[GoType] partial interface BufferPool {
    slice<byte> Get();
    void Put(slice<byte> _Δp0);
}

internal static @string singleJoiningSlash(@string a, @string b) {
    var aslash = strings.HasSuffix(a, "/"u8);
    var bslash = strings.HasPrefix(b, "/"u8);
    switch (ᐧ) {
    case {} when aslash && bslash: {
        return a + b[1..];
    }
    case {} when !aslash && !bslash: {
        return a + "/"u8 + b;
    }}

    return a + b;
}

internal static (@string path, @string rawpath) joinURLPath(ж<url.URL> Ꮡa, ж<url.URL> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    if (a.RawPath == ""u8 && b.RawPath == ""u8) {
        return (singleJoiningSlash(a.Path, b.Path), "");
    }
    // Same as singleJoiningSlash, but uses EscapedPath to determine
    // whether a slash should be added
    @string apath = a.EscapedPath();
    @string bpath = b.EscapedPath();
    var aslash = strings.HasSuffix(apath, "/"u8);
    var bslash = strings.HasPrefix(bpath, "/"u8);
    switch (ᐧ) {
    case {} when aslash && bslash: {
        return (a.Path + b.Path[1..], apath + bpath[1..]);
    }
    case {} when !aslash && !bslash: {
        return (a.Path + "/" + b.Path, apath + "/" + bpath);
    }}

    return (a.Path + b.Path, apath + bpath);
}

// NewSingleHostReverseProxy returns a new [ReverseProxy] that routes
// URLs to the scheme, host, and base path provided in target. If the
// target's path is "/base" and the incoming request was for "/dir",
// the target request will be for /base/dir.
//
// NewSingleHostReverseProxy does not rewrite the Host header.
//
// To customize the ReverseProxy behavior beyond what
// NewSingleHostReverseProxy provides, use ReverseProxy directly
// with a Rewrite function. The ProxyRequest SetURL method
// may be used to route the outbound request. (Note that SetURL,
// unlike NewSingleHostReverseProxy, rewrites the Host header
// of the outbound request by default.)
//
//	proxy := &ReverseProxy{
//		Rewrite: func(r *ProxyRequest) {
//			r.SetURL(target)
//			r.Out.Host = r.In.Host // if desired
//		},
//	}
public static ж<ReverseProxy> NewSingleHostReverseProxy(ж<url.URL> Ꮡtarget) {
    var director = (ж<http.Request> req) => {
        rewriteRequestURL(ref (req).DerefOrNull(), Ꮡtarget);
    };
    return Ꮡ(new ReverseProxy(Director: director));
}

internal static void rewriteRequestURL(ref http.Request req, ж<url.URL> Ꮡtarget) {
    ref var target = ref Ꮡtarget.DerefOrNull();

    @string targetQuery = target.RawQuery;
    req.URL.Value.Scheme = target.Scheme;
    req.URL.Value.Host = target.Host;
    (req.URL.Value.Path, req.URL.Value.RawPath) = joinURLPath(Ꮡtarget, req.URL);
    if (targetQuery == ""u8 || (~req.URL).RawQuery == ""u8){
        req.URL.Value.RawQuery = targetQuery + (~req.URL).RawQuery;
    } else {
        req.URL.Value.RawQuery = targetQuery + "&"u8 + (~req.URL).RawQuery;
    }
}

internal static void copyHeader(httpꓸHeader dst, httpꓸHeader src) {
    foreach (var (k, vv) in src) {
        foreach (var (_, v) in vv) {
            dst.Add(k, v);
        }
    }
}

// non-standard but still sent by libcurl and rejected by e.g. google
// canonicalized version of "TE"
// not Trailers per URL above; https://www.rfc-editor.org/errata_search.php?eid=4522
// Hop-by-hop headers. These are removed when sent to the backend.
// As of RFC 7230, hop-by-hop headers are required to appear in the
// Connection header field. These are the headers defined by the
// obsoleted RFC 2616 (section 13.5.1) and are used for backward
// compatibility.
internal static slice<@string> hopHeaders = new @string[]{
    "Connection"u8,
    "Proxy-Connection"u8,
    "Keep-Alive"u8,
    "Proxy-Authenticate"u8,
    "Proxy-Authorization"u8,
    "Te"u8,
    "Trailer"u8,
    "Transfer-Encoding"u8,
    "Upgrade"u8
}.slice();

[GoRecv] internal static void defaultErrorHandler(this ref ReverseProxy p, http.ResponseWriter rw, ж<http.Request> Ꮡreq, error err) {
    p.logf("http: proxy error: %v"u8, err);
    rw.WriteHeader(http.StatusBadGateway);
}

internal static Action<http.ResponseWriter, ж<http.Request>, error> getErrorHandler(this ж<ReverseProxy> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (p.ErrorHandler != default!) {
        return p.ErrorHandler;
    }
    return Ꮡp.defaultErrorHandler;
}

// modifyResponse conditionally runs the optional ModifyResponse hook
// and reports whether the request should proceed.
internal static bool modifyResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Response> Ꮡres, ж<http.Request> Ꮡreq) {
    ref var p = ref Ꮡp.DerefOrNull();
    ref var res = ref Ꮡres.DerefOrNull();

    if (p.ModifyResponse == default!) {
        return true;
    }
    {
        var err = p.ModifyResponse(Ꮡres); if (err != default!) {
            res.Body.Close();
            Ꮡp.getErrorHandler()(rw, Ꮡreq, err);
            return false;
        }
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reverseProxyMustHaveˢ = "ReverseProxy must have exactly one of Director or Rewrite set"u8;
internal static readonly @string trailersˢ = "trailers"u8;
internal static readonly @string connectionˢ = "Connection"u8;
internal static readonly @string upgradeˢ = "Upgrade"u8;
internal static readonly @string forwardedˢ = "Forwarded"u8;
internal static readonly @string userAgentˢ = "User-Agent"u8;
internal static readonly @string trailerˢ = "Trailer"u8;

public static void ServeHTTP(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Request> Ꮡreq) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();
        ref var req = ref Ꮡreq.DerefOrNull();

        var transport = p.Transport;
        if (transport == default!) {
            transport = http.DefaultTransport;
        }
        var ctx = req.Context();
        if (ctx.Done() != default!){
        } else 
        {
            var (cn, ok) = rw._<http.CloseNotifier>(ᐧ); if (ok) {
                // CloseNotifier predates context.Context, and has been
                // entirely superseded by it. If the request contains
                // a Context that carries a cancellation signal, don't
                // bother spinning up a goroutine to watch the CloseNotify
                // channel (if any).
                //
                // If the request Context has a nil Done channel (which
                // means it is either context.Background, or a custom
                // Context implementation with no cancellation signal),
                // then consult the CloseNotifier if available.
                Action cancel = default!;
                (ctx, cancel) = context.WithCancel(ctx);
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);
                var notifyChan = cn.CloseNotify();
                var cancelʗ2 = cancel;
                var ctxʗ1 = ctx;
                var notifyChanʗ1 = notifyChan;
                goǃ(() => {
                    var selᴛ3 = notifyChanʗ1;
                    var selᴛ4 = ctxʗ1.Done();
                    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
                    case 0 when selᴛ3.ꟷᐳ(out _): {
                        cancelʗ2();
                        break;
                    }
                    case 1 when selᴛ4.ꟷᐳ(out _): {
                        break;
                    }}
                });
            }
        }
        var outreq = req.Clone(ctx);
        if (req.ContentLength == 0) {
            outreq.Value.Body = default!; // Issue 16036: nil Body for http.Transport retries
        }
        if ((~outreq).Body != default!) {
            // Reading from the request body after returning from a handler is not
            // allowed, and the RoundTrip goroutine that reads the Body can outlive
            // this handler. This can lead to a crash if the handler panics (see
            // Issue 46866). Although calling Close doesn't guarantee there isn't
            // any Read in flight after the handle returns, in practice it's safe to
            // read after closing it.
            var outreqʗ1 = outreq;
            defer(() => (~outreqʗ1).Body.Close(), ref ᒐ);
        }
        if ((~outreq).Header == default!) {
            outreq.Value.Header = new httpꓸHeader(0); // Issue 33142: historical behavior was to always allocate
        }
        if ((p.Director != default!) == (p.Rewrite != default!)) {
            Ꮡp.getErrorHandler()(rw, Ꮡreq, errors.New(reverseProxyMustHaveˢ));
            return;
        }
        if (p.Director != default!) {
            p.Director(outreq);
            if ((~outreq).Form != default!) {
                outreq.Value.URL.Value.RawQuery = cleanQueryParams((~(~outreq).URL).RawQuery);
            }
        }
        outreq.Value.Close = false;
        @string reqUpType = upgradeType((~outreq).Header);
        if (!ascii.IsPrint(reqUpType)) {
            Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("client tried to switch to invalid protocol %q"u8, reqUpType));
            return;
        }
        removeHopByHopHeaders((~outreq).Header);
        // Issue 21096: tell backend applications that care about trailer support
        // that we support trailers. (We do, but we don't go out of our way to
        // advertise that unless the incoming client request thought it was worth
        // mentioning.) Note that we look at req.Header, not outreq.Header, since
        // the latter has passed through removeHopByHopHeaders.
        if (httpguts.HeaderValuesContainsToken(req.Header["Te"u8], trailersˢ)) {
            (~outreq).Header.Set("Te"u8, trailersˢ);
        }
        // After stripping all the hop-by-hop connection headers above, add back any
        // necessary for protocol upgrades, such as for websockets.
        if (reqUpType != ""u8) {
            (~outreq).Header.Set(connectionˢ, upgradeˢ);
            (~outreq).Header.Set(upgradeˢ, reqUpType);
        }
        if (p.Rewrite != default!){
            // Strip client-provided forwarding headers.
            // The Rewrite func may use SetXForwarded to set new values
            // for these or copy the previous values from the inbound request.
            (~outreq).Header.Del(forwardedˢ);
            (~outreq).Header.Del(xForwardedForˢ);
            (~outreq).Header.Del(xForwardedHostˢ);
            (~outreq).Header.Del(xForwardedProtoˢ);
            // Remove unparsable query parameters from the outbound request.
            outreq.Value.URL.Value.RawQuery = cleanQueryParams((~(~outreq).URL).RawQuery);
            var pr = Ꮡ(new ProxyRequest(
                In: Ꮡreq,
                Out: outreq
            ));
            p.Rewrite(pr);
            outreq = pr.Value.Out;
        } else {
            {
                var (clientIP, _, errΔ1) = net.SplitHostPort(req.RemoteAddr); if (errΔ1 == default!) {
                    // If we aren't the first proxy retain prior
                    // X-Forwarded-For information as a comma+space
                    // separated list and fold multiple headers into one.
                    var (prior, ok) = (~outreq).Header[xForwardedForˢ, ꟷ];
                    var omit = ok && prior == default!; // Issue 38079: nil now means don't populate the header
                    if (len(prior) > 0) {
                        clientIP = strings.Join(prior, ", "u8) + ", "u8 + clientIP;
                    }
                    if (!omit) {
                        (~outreq).Header.Set(xForwardedForˢ, clientIP);
                    }
                }
            }
        }
        {
            var (_, ok) = (~outreq).Header[userAgentˢ, ꟷ]; if (!ok) {
                // If the outbound request doesn't have a User-Agent header set,
                // don't send the default Go HTTP client User-Agent.
                (~outreq).Header.Set(userAgentˢ, ""u8);
            }
        }
        ref var roundTripMutex = ref heap(new sync.Mutex(), out var ᏑroundTripMutex);
        bool roundTripDone = default!;
        var trace = Ꮡ(new httptrace.ClientTrace(
            Got1xxResponse: (nint code, textproto.MIMEHeader header) => {
                GoFrame ᒐ = default;
                try {
                    ᏑroundTripMutex.Lock();
                    defer(ᏑroundTripMutex.Unlock, ref ᒐ);
                    if (roundTripDone) {
                        // If RoundTrip has returned, don't try to further modify
                        // the ResponseWriter's header map.
                        return default!;
                    }
                    var h = rw.Header();
                    copyHeader(h, ((httpꓸHeader)(map<@string, slice<@string>>)header));
                    rw.WriteHeader(code);
                    // Clear headers, it's not automatically done by ResponseWriter.WriteHeader() for 1xx responses
                    clear(h);
                    return default!;
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
                finally { ᒐ.Run(); }
            }
        ));
        outreq = outreq.WithContext(httptrace.WithClientTrace(outreq.Context(), trace));
        var (res, err) = transport.RoundTrip(outreq);
        ᏑroundTripMutex.Lock();
        roundTripDone = true;
        ᏑroundTripMutex.Unlock();
        if (err != default!) {
            Ꮡp.getErrorHandler()(rw, outreq, err);
            return;
        }
        // Deal with 101 Switching Protocols responses: (WebSocket, h2c, etc)
        if ((~res).StatusCode == http.StatusSwitchingProtocols) {
            if (!Ꮡp.modifyResponse(rw, res, outreq)) {
                return;
            }
            Ꮡp.handleUpgradeResponse(rw, outreq, res);
            return;
        }
        removeHopByHopHeaders((~res).Header);
        if (!Ꮡp.modifyResponse(rw, res, outreq)) {
            return;
        }
        copyHeader(rw.Header(), (~res).Header);
        // The "Trailer" header isn't included in the Transport's response,
        // at least for *http.Transport. Build it up from Trailer.
        nint announcedTrailers = len((~res).Trailer);
        if (announcedTrailers > 0) {
            var trailerKeys = new slice<@string>(0, len((~res).Trailer));
            foreach (var (k, _) in (~res).Trailer) {
                trailerKeys = append(trailerKeys, k);
            }
            rw.Header().Add(trailerˢ, strings.Join(trailerKeys, ", "u8));
        }
        rw.WriteHeader((~res).StatusCode);
        err = Ꮡp.copyResponse(rw, (~res).Body, p.flushInterval(res));
        if (err != default!) {
            var resʗ1 = res;
            defer(() => (~resʗ1).Body.Close(), ref ᒐ);
            // Since we're streaming the response, if we run into an error all we can do
            // is abort the request. Issue 23643: ReverseProxy should use ErrAbortHandler
            // on read error while copying body.
            if (!shouldPanicOnCopyError(Ꮡreq)) {
                p.logf("suppressing panic for copyResponse error in test; copy error: %v"u8, err);
                return;
            }
            throw panic(http.ErrAbortHandler);
        }
        (~res).Body.Close(); // close now, instead of defer, to populate res.Trailer
        if (len((~res).Trailer) > 0) {
            // Force chunking if we saw a response trailer.
            // This prevents net/http from calculating the length for short
            // bodies and adding a Content-Length.
            http.NewResponseController(rw).Flush();
        }
        if (len((~res).Trailer) == announcedTrailers) {
            copyHeader(rw.Header(), (~res).Trailer);
            return;
        }
        foreach (var (kᴛ1, vv) in (~res).Trailer) {
            var k = kᴛ1;

            k = http.TrailerPrefix + k;
            foreach (var (_, v) in vv) {
                rw.Header().Add(k, v);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool inOurTests; // whether we're in our own tests

// shouldPanicOnCopyError reports whether the reverse proxy should
// panic with http.ErrAbortHandler. This is the right thing to do by
// default, but Go 1.10 and earlier did not, so existing unit tests
// weren't expecting panics. Only panic in our own tests, or when
// running under the HTTP server.
internal static bool shouldPanicOnCopyError(ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    if (inOurTests) {
        // Our tests know to handle this panic.
        return true;
    }
    if (req.Context().Value(http.ServerContextKey.OrTypedNil()) != default!) {
        // We seem to be running under an HTTP server, so
        // it'll recover the panic.
        return true;
    }
    // Otherwise act like Go 1.10 and earlier to not break
    // existing tests.
    return false;
}

// removeHopByHopHeaders removes hop-by-hop headers.
internal static void removeHopByHopHeaders(httpꓸHeader h) {
    // RFC 7230, section 6.1: Remove headers listed in the "Connection" header.
    foreach (var (_, f) in h[connectionˢ]) {
        foreach (var (_, vᴛ1) in strings.Split(f, ","u8)) {
            var sf = vᴛ1;

            {
                sf = textproto.TrimString(sf); if (sf != ""u8) {
                    h.Del(sf);
                }
            }
        }
    }
    // RFC 2616, section 13.5.1: Remove a set of known hop-by-hop headers.
    // This behavior is superseded by the RFC 7230 Connection header, but
    // preserve it for backwards compatibility.
    foreach (var (_, f) in hopHeaders) {
        h.Del(f);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentTypeˢ = "Content-Type"u8;

// flushInterval returns the p.FlushInterval value, conditionally
// overriding its value for a specific request/response.
[GoRecv] internal static time.Duration flushInterval(this ref ReverseProxy p, ж<http.Response> Ꮡres) {
    ref var res = ref Ꮡres.DerefOrNull();

    @string resCT = res.Header.Get(contentTypeˢ);
    // For Server-Sent Events responses, flush immediately.
    // The MIME type is defined in https://www.w3.org/TR/eventsource/#text-event-stream
    {
        var (baseCT, _, _) = mime.ParseMediaType(resCT); if (baseCT == "text/event-stream"u8) {
            return -1; // negative means immediately
        }
    }
    // We might have the case of streaming for which Content-Length might be unset.
    if (res.ContentLength == -1) {
        return -1;
    }
    return p.FlushInterval;
}

internal static error copyResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter dst, io.Reader src, time.Duration flushInterval) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        io.Writer w = new http_ResponseWriterᴠWriter(dst);
        if (flushInterval != 0) {
            var mlw = Ꮡ(new maxLatencyWriter(
                dst: new http_ResponseWriterᴠWriter(dst),
                flush: http.NewResponseController(dst).Flush,
                latency: flushInterval
            ));
            var mlwʗ1 = mlw;
            defer(mlwʗ1.stop, ref ᒐ);
            // set up initial timer so headers get flushed even if body writes are delayed
            mlw.Value.flushPending = true;
            mlw.Value.t = time.AfterFunc(flushInterval, mlw.delayedFlush);
            w = new maxLatencyWriterжWriter(mlw);
        }
        slice<byte> buf = default!;
        if (p.BufferPool != default!) {
            buf = p.BufferPool.Get();
            defer(Ꮡp.Value.BufferPool.Put, buf, ref ᒐ);
        }
        var (_, err) = p.copyBuffer(w, src, buf);
        return err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// copyBuffer returns any write errors or non-EOF read errors, and the amount
// of bytes written.
[GoRecv] internal static (int64, error) copyBuffer(this ref ReverseProxy p, io.Writer dst, io.Reader src, slice<byte> buf) {
    if (len(buf) == 0) {
        buf = new slice<byte>(32 * 1024);
    }
    int64 written = default!;
    while (ᐧ) {
        var (nr, rerr) = src.Read(buf);
        if (rerr != default! && !AreEqual(rerr, io.EOF) && !AreEqual(rerr, context.Canceled)) {
            p.logf("httputil: ReverseProxy read error during body copy: %v"u8, rerr);
        }
        if (nr > 0) {
            var (nw, werr) = dst.Write(buf.slice(0, nr));
            if (nw > 0) {
                written += (int64)nw;
            }
            if (werr != default!) {
                return (written, werr);
            }
            if (nr != nw) {
                return (written, io.ErrShortWrite);
            }
        }
        if (rerr != default!) {
            if (AreEqual(rerr, io.EOF)) {
                rerr = default!;
            }
            return (written, rerr);
        }
    }
}

[GoRecv] internal static void logf(this ref ReverseProxy p, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    if (p.ErrorLog != nil){
        p.ErrorLog.Printf(format, args.ꓸꓸꓸ);
    } else {
        log.Printf(format, args.ꓸꓸꓸ);
    }
}

[GoType] partial struct maxLatencyWriter {
    internal io.Writer dst;
    internal Func<error> flush;
    internal time.Duration latency; // non-zero; negative means to flush immediately
    internal sync.Mutex mu; // protects t, flushPending, and dst.Flush
    internal ж<time.Timer> t;
    internal bool flushPending;
}

internal static (nint n, error err) Write(this ж<maxLatencyWriter> Ꮡm, slice<byte> p) {
    nint n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        m.mu.Lock();
        defer(Ꮡm.of(maxLatencyWriter.Ꮡmu).Unlock, ref ᒐ);
        (n, err) = m.dst.Write(p);
        if (m.latency < 0) {
            m.flush();
            goto ᒐdone;
        }
        if (m.flushPending) {
            goto ᒐdone;
        }
        if (m.t == nil){
            m.t = time.AfterFunc(m.latency, Ꮡm.delayedFlush);
        } else {
            m.t.Reset(m.latency);
        }
        m.flushPending = true;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

internal static void delayedFlush(this ж<maxLatencyWriter> Ꮡm) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        m.mu.Lock();
        ᒐd1 = true;
        if (!m.flushPending) {
            // if stop was called but AfterFunc already started this goroutine
            return;
        }
        m.flush();
        m.flushPending = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡm.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void stop(this ж<maxLatencyWriter> Ꮡm) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        m.mu.Lock();
        ᒐd1 = true;
        m.flushPending = false;
        if (m.t != nil) {
            m.t.Stop();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡm.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static @string upgradeType(httpꓸHeader h) {
    if (!httpguts.HeaderValuesContainsToken(h[connectionˢ], upgradeˢ)) {
        return ""u8;
    }
    return h.Get(upgradeˢ);
}

internal static void handleUpgradeResponse(this ж<ReverseProxy> Ꮡp, http.ResponseWriter rw, ж<http.Request> Ꮡreq, ж<http.Response> Ꮡres) {
    GoFrame ᒐ = default;
    try {
        ref var req = ref Ꮡreq.DerefOrNull();
        ref var res = ref Ꮡres.DerefOrNull();

        @string reqUpType = upgradeType(req.Header);
        @string resUpType = upgradeType(res.Header);
        if (!ascii.IsPrint(resUpType)) {
            // We know reqUpType is ASCII, it's checked by the caller.
            Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("backend tried to switch to invalid protocol %q"u8, resUpType));
            return;
        }
        if (!ascii.EqualFold(reqUpType, resUpType)) {
            Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("backend tried to switch protocol %q when %q was requested"u8, resUpType, reqUpType));
            return;
        }
        var (backConn, ok) = res.Body._<io.ReadWriteCloser>(ᐧ);
        if (!ok) {
            Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("internal error: 101 switching protocols response with non-writable body"u8));
            return;
        }
        var rc = http.NewResponseController(rw);
        var (conn, brw, hijackErr) = rc.Hijack();
        if (errors.Is(hijackErr, new http.ProtocolErrorжerror(http.ErrNotSupported))) {
            Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("can't switch protocols using non-Hijacker ResponseWriter type %T"u8, rw));
            return;
        }
        var backConnCloseCh = new channel<bool>(0);
        var backConnʗ1 = backConn;
        var backConnCloseChʗ1 = backConnCloseCh;
        goǃ(() => {
            // Ensure that the cancellation of a request closes the backend.
            // See issue https://golang.org/issue/35559.
            var selᴛ5 = Ꮡreq.Value.Context().Done();
            var selᴛ6 = backConnCloseChʗ1;
            switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
            case 0 when selᴛ5.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ6.ꟷᐳ(out _): {
                break;
            }}
            backConnʗ1.Close();
        });
        defer(ᴛ1 => close(ᴛ1), backConnCloseCh, ref ᒐ);
        if (hijackErr != default!) {
            Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("Hijack failed on protocol switch: %v"u8, hijackErr));
            return;
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        copyHeader(rw.Header(), res.Header);
        res.Header = rw.Header();
        res.Body = default!; // so res.Write only writes the headers; we have res.Body in backConn above
        {
            var err = res.Write(new bufio_ReadWriterжWriter(brw)); if (err != default!) {
                Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("response write: %v"u8, err));
                return;
            }
        }
        {
            var err = brw.Value.Writer.Value.Flush(); if (err != default!) {
                Ꮡp.getErrorHandler()(rw, Ꮡreq, fmt.Errorf("response flush: %v"u8, err));
                return;
            }
        }
        var errc = new channel<error>(1);
        ref var spc = ref heap<switchProtocolCopier>(out var Ꮡspc);
        spc = new switchProtocolCopier(user: new net_ConnᴠReadWriter(conn), backend: new io_ReadWriteCloserᴠReadWriter(backConn));
        var spcʗ1 = spc;
        goǃ(ᴛ1 => spcʗ1.copyToBackend(ᴛ1), errc.WithDirection(GoChanDir.Send));
        var spcʗ2 = spc;
        goǃ(ᴛ1 => spcʗ2.copyFromBackend(ᴛ1), errc.WithDirection(GoChanDir.Send));
        ᐸꟷ(errc);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// switchProtocolCopier exists so goroutines proxying data back and
// forth have nice names in stacks.
[GoType] partial struct switchProtocolCopier {
    internal io.ReadWriter user, backend;
}

internal static void copyFromBackend(this switchProtocolCopier c, channel/*<-*/<error> errc) {
    var (_, err) = io.Copy(c.user, c.backend);
    errc.ᐸꟷ(err);
}

internal static void copyToBackend(this switchProtocolCopier c, channel/*<-*/<error> errc) {
    var (_, err) = io.Copy(c.backend, c.user);
    errc.ᐸꟷ(err);
}

internal static @string cleanQueryParams(@string s) {
    @string reencode(@string sΔ1) {
        var (v, _) = url.ParseQuery(sΔ1);
        return v.Encode();
    }
    for (nint i = 0; i < len(s); ) {
        switch (s[i]) {
        case (rune)';': {
            return reencode(s);
        }
        case (rune)'%': {
            if (i + 2 >= len(s) || !ishex(s[i + 1]) || !ishex(s[i + 2])) {
                return reencode(s);
            }
            i += 3;
            break;
        }
        default: {
            i++;
            break;
        }}

    }
    return s;
}

internal static bool ishex(byte c) {
    switch (ᐧ) {
    case {} when (rune)'0' <= c && c <= (rune)'9': {
        return true;
    }
    case {} when (rune)'a' <= c && c <= (rune)'f': {
        return true;
    }
    case {} when (rune)'A' <= c && c <= (rune)'F': {
        return true;
    }}

    return false;
}

} // end httputil_package
